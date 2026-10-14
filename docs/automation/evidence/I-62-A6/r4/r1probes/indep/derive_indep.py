"""Independent implementation of the acceptor-closure derivation
rackcad-input-closure/v1 (E) -> rackcad-bmin-closure/0-experimental (I-62, A-6 round 4).

Written only from D:\\r62-a6r4\\indep\\SPEC.md.  Standard library only.

    python -I derive_indep.py <vector.json> <out.json>

Exit codes: 0 = closure written; 2 = bench error (nothing written).
Every git call: git --no-replace-objects -C <Repo> ... (read-only commands only).
"""

import hashlib
import json
import os
import re
import subprocess
import sys

SCHEMA_OUT = "rackcad-bmin-closure/0-experimental"
E_LISTS = ("CanonicalInputs", "AllowedTransitiveInputs", "AutomaticInstructions")
HEX40 = re.compile(r"[0-9a-f]{40}")

# causes (SPEC 2.3)
INPUT_ABSENT = "INPUT_ABSENT"
FIELD_MISSING = "FIELD_MISSING"
FIELD_NULL = "FIELD_NULL"
FIELD_EMPTY = "FIELD_EMPTY"
NOT_A_COMMIT = "NOT_A_COMMIT"
PRIMITIVE_EMPTY = "PRIMITIVE_EMPTY"

# term labels (SPEC 2.3), in the order of the table
LABELS = (
    "C1.E.AuthorityRevision", "C1.D.BaseSha", "C1.D.MainSha", "C1.D.ChainBaseSha", "C1.D.ChainRedSha",
    "C1.H.BaseSha", "C1.H.CurrentSha", "C1.H.RedSha", "C1.I.Target.Commit", "C1.K.AuthorityRevision",
    "C1.K.MainSha", "C1.RevList(D.BaseSha..H.CurrentSha)", "C1.FirstParentChain(D.MainSha)",
    "C1.MergeBase(D.MainSha,K.AuthorityRevision)", "C2.origin/D.ExpectedBranch", "C4.K.Authorities",
    "C4.IgnorePath(I)", "C6.CONTRACT", "C6.DELEG", "C6.H",
)


class BenchError(Exception):
    pass


# ---------------------------------------------------------------- strict JSON

class _DupKey(Exception):
    pass


def _pairs_no_dup(pairs):
    seen = set()
    for k, _ in pairs:
        if k in seen:
            raise _DupKey(k)
        seen.add(k)
    return dict(pairs)


def _reject_constant(name):
    # NaN / Infinity / -Infinity are not JSON
    raise ValueError("non-JSON constant " + name)


def strict_json_object(data):
    """bytes -> dict, or None when the bytes are not a single JSON object in strict UTF-8
    (a BOM counts as invalid) without repeated keys anywhere."""
    if data.startswith(b"\xef\xbb\xbf"):
        return None
    try:
        text = data.decode("utf-8", "strict")
    except UnicodeDecodeError:
        return None
    try:
        # json.loads rejects any trailing non-whitespace data (exactly one value)
        value = json.loads(text, object_pairs_hook=_pairs_no_dup, parse_constant=_reject_constant)
    except (ValueError, _DupKey, RecursionError):
        return None
    if not isinstance(value, dict):
        return None
    return value


# ---------------------------------------------------------------- git

def _git_env():
    env = {k: v for k, v in os.environ.items() if not k.upper().startswith("GIT_") or k.upper() == "GIT_EXEC_PATH"}
    env["GIT_OPTIONAL_LOCKS"] = "0"
    env["GIT_TERMINAL_PROMPT"] = "0"
    return env


class Git:
    def __init__(self, repo):
        self.repo = repo
        self.env = _git_env()
        self._type_cache = {}
        self._tree_cache = {}

    def run(self, args, stdin=None):
        cmd = ["git", "--no-replace-objects", "-C", self.repo] + list(args)
        try:
            p = subprocess.run(cmd, input=stdin, stdout=subprocess.PIPE, stderr=subprocess.PIPE, env=self.env)
        except OSError as ex:
            raise BenchError("cannot run git: %s" % ex)
        return p.returncode, p.stdout, p.stderr

    def must(self, args, stdin=None):
        rc, out, err = self.run(args, stdin)
        if rc != 0:
            raise BenchError("git %s failed (%d): %s" % (" ".join(args), rc, err.decode("utf-8", "replace").strip()))
        return out

    def object_type(self, oid):
        """git cat-file -t <oid> -> 'commit' / 'tree' / ... or None."""
        if oid not in self._type_cache:
            rc, out, _ = self.run(["cat-file", "-t", oid])
            self._type_cache[oid] = out.decode("ascii", "replace").strip() if rc == 0 else None
        return self._type_cache[oid]

    def blob_bytes(self, blob):
        rc, out, _ = self.run(["cat-file", "blob", blob])
        return out if rc == 0 else None

    @staticmethod
    def _lines(out):
        return [ln for ln in out.decode("ascii").split("\n") if ln.strip()]

    def rev_list_range(self, a, b):
        # RevList(a..b): reachable from b and not from a
        return set(self._lines(self.must(["rev-list", b, "^" + a])))

    def first_parent_chain(self, x):
        return self._lines(self.must(["rev-list", "--first-parent", x]))

    def parents_of(self, commits):
        """commit -> list of parents (true parents, no walk)."""
        if not commits:
            return {}
        out = self.must(["rev-list", "--no-walk=unsorted", "--parents", "--stdin"],
                        stdin=("\n".join(commits) + "\n").encode("ascii"))
        res = {}
        for ln in self._lines(out):
            parts = ln.split()
            res[parts[0]] = parts[1:]
        return res

    def merge_base(self, x, y):
        rc, out, err = self.run(["merge-base", x, y])
        if rc == 1 and not out.strip():
            return None  # no common ancestor
        if rc != 0:
            raise BenchError("git merge-base failed (%d): %s" % (rc, err.decode("utf-8", "replace").strip()))
        lines = self._lines(out)
        if len(lines) != 1:
            raise BenchError("git merge-base printed %d lines" % len(lines))
        return lines[0]

    def tree_blobs(self, commit):
        """path -> blob id for every blob of the tree of <commit> (exact tree paths)."""
        tree = self.must(["rev-parse", "--verify", "--end-of-options", commit + "^{tree}"]).decode("ascii").strip()
        if tree not in self._tree_cache:
            out = self.must(["ls-tree", "-r", "-z", "--full-tree", tree])
            m = {}
            for rec in out.split(b"\0"):
                if not rec:
                    continue
                meta, path = rec.split(b"\t", 1)
                _mode, otype, oid = meta.decode("ascii").split(" ")
                if otype == "blob":
                    m[path.decode("utf-8", "surrogateescape")] = oid
            self._tree_cache[tree] = m
        return self._tree_cache[tree]


def git_blob_id(data):
    return hashlib.sha1(b"blob %d\0" % len(data) + data).hexdigest()


# ---------------------------------------------------------------- field reading (rule ii)

def read_field(obj, keys, label):
    """Returns (cause, value).  cause in FIELD_MISSING/FIELD_NULL/FIELD_EMPTY or None.
    Intermediate containers: absent -> MISSING, null -> NULL, "" or [] -> EMPTY, {} -> descend.
    Leaf: absent -> MISSING, null -> NULL, "" / [] / {} -> EMPTY."""
    cur = obj
    for i, k in enumerate(keys):
        if not isinstance(cur, dict):
            raise BenchError("%s: container for '%s' is %s, not an object" % (label, k, type(cur).__name__))
        if k not in cur:
            return FIELD_MISSING, None
        cur = cur[k]
        if cur is None:
            return FIELD_NULL, None
        if (isinstance(cur, str) and cur == "") or (isinstance(cur, list) and not cur):
            return FIELD_EMPTY, None
        if i == len(keys) - 1 and isinstance(cur, dict) and not cur:
            return FIELD_EMPTY, None
    return None, cur


def read_string(obj, keys, label):
    cause, v = read_field(obj, keys, label)
    if cause is None and not isinstance(v, str):
        raise BenchError("%s: field %s is %s, not a string (no cause defined)" % (label, ".".join(keys), type(v).__name__))
    return cause, v


# ---------------------------------------------------------------- derivation

class Derivation:
    def __init__(self, vector_path):
        self.vdir = os.path.dirname(os.path.abspath(vector_path))
        with open(vector_path, "rb") as f:
            vec = strict_json_object(f.read())
        if vec is None:
            raise BenchError("vector is not a strict JSON object")
        for key in ("Repo", "E", "I", "Designation", "MapPath"):
            if key not in vec:
                raise BenchError("vector lacks %s" % key)
        if not isinstance(vec["Repo"], str) or not isinstance(vec["E"], str) or not isinstance(vec["MapPath"], str):
            raise BenchError("vector Repo/E/MapPath must be strings")
        if not isinstance(vec["I"], dict) or not isinstance(vec["Designation"], dict):
            raise BenchError("vector I/Designation must be objects")
        custody = vec.get("Custody", {})
        if custody is None:
            custody = {}
        if not isinstance(custody, dict) or not all(isinstance(v, str) for v in custody.values()):
            raise BenchError("vector Custody must map paths to file names")

        self.repo = self._abs(vec["Repo"])
        self.git = Git(self.repo)
        rc, _, err = self.git.run(["rev-parse", "--git-dir"])
        if rc != 0:
            raise BenchError("Repo is not a git repository: %s" % err.decode("utf-8", "replace").strip())
        self.custody = {k: self._abs(v) for k, v in custody.items()}
        self.I = vec["I"]
        self.designation = vec["Designation"]
        self.map_path = vec["MapPath"]

        try:
            with open(self._abs(vec["E"]), "rb") as f:
                E = strict_json_object(f.read())
        except OSError as ex:
            raise BenchError("cannot read E: %s" % ex)
        if E is None:
            raise BenchError("E is not a strict JSON object")
        self.E = E
        self.entries = {}  # list name -> [(path, blob)]
        for name in E_LISTS:
            lst = E.get(name, [])
            if lst is None:
                lst = []
            if not isinstance(lst, list):
                raise BenchError("E.%s is not a list" % name)
            pairs = []
            for e in lst:
                if not isinstance(e, dict) or not isinstance(e.get("Path"), str) or not isinstance(e.get("Blob"), str):
                    raise BenchError("E.%s has an entry without string Path/Blob" % name)
                pairs.append((e["Path"], e["Blob"]))
            self.entries[name] = pairs
        self.all_pairs = []
        for name in E_LISTS:
            for pb in self.entries[name]:
                if pb not in self.all_pairs:
                    self.all_pairs.append(pb)

        self.omitted = {}   # label -> cause (one record per term)
        self.input_notes = {}

    def _abs(self, p):
        return p if os.path.isabs(p) else os.path.normpath(os.path.join(self.vdir, p))

    def omit(self, label, cause):
        assert label in LABELS, label
        if label not in self.omitted:
            self.omitted[label] = cause

    # -------------------------------------------------- E entries, bytes, inputs

    def pinned_blobs(self, path):
        return sorted({b for (p, b) in self.all_pairs if p == path})

    def entry_bytes(self, path, blob):
        """Custodied bytes of an E entry, or None if they do not exist."""
        if path in self.custody:
            try:
                with open(self.custody[path], "rb") as f:
                    return f.read()
            except OSError:
                return None
        if not HEX40.fullmatch(blob):
            return None  # cannot name a blob of the -C (and git_blob_id is always 40 lowercase hex)
        return self.git.blob_bytes(blob)

    def verified_bytes(self, path, blob):
        data = self.entry_bytes(path, blob)
        if data is None:
            return None, "bytes do not exist"
        if git_blob_id(data) != blob:
            return None, "bytes do not have the pinned blob"
        return data, None

    def designated(self, name):
        p = self.designation.get(name)
        if p is None:
            return None
        if not isinstance(p, str):
            raise BenchError("Designation.%s is not a string" % name)
        return p if p != "" else None

    def load_input(self, name):
        """Rule (i).  Returns (object or None, note)."""
        path = self.designated(name)
        if path is None:
            return None, "not designated"
        blobs = self.pinned_blobs(path)
        if not blobs:
            return None, "E does not pin its blob"
        if len(blobs) > 1:
            raise BenchError("E pins %d different blobs at the designated %s path" % (len(blobs), name))
        data, why = self.verified_bytes(path, blobs[0])
        if data is None:
            return None, why
        obj = strict_json_object(data)
        if obj is None:
            return None, "not a single JSON object in UTF-8 without repeated keys"
        return obj, "present"

    def load_map(self):
        blobs = self.pinned_blobs(self.map_path)
        if len(blobs) != 1:
            raise BenchError("E must pin exactly one blob at MapPath (found %d)" % len(blobs))
        data, why = self.verified_bytes(self.map_path, blobs[0])
        if data is None:
            raise BenchError("map M: " + why)
        M = strict_json_object(data)
        if M is None:
            raise BenchError("map M is not a strict JSON object")
        return M

    # -------------------------------------------------- commit values (rule ii)

    def commit_value(self, obj, keys, label):
        """(cause, sha): sha valid only if 40 lowercase hex and cat-file -t says commit."""
        cause, v = read_field(obj, keys, label)
        if cause is not None:
            return cause, None
        if not (isinstance(v, str) and HEX40.fullmatch(v)):
            return NOT_A_COMMIT, None
        if self.git.object_type(v) != "commit":
            return NOT_A_COMMIT, None
        return None, v

    def c1_term(self, commits, label, src, keys):
        """A single C1 term read from input src (None = absent by rule (i))."""
        if src is None:
            self.omit(label, INPUT_ABSENT)
            return None
        cause, sha = self.commit_value(src, keys, label)
        if cause is not None:
            self.omit(label, cause)
            return None
        commits.add(sha)
        return sha

    # -------------------------------------------------- the table

    def derive(self):
        E, I = self.E, self.I
        D, dnote = self.load_input("D")
        H, hnote = self.load_input("H")
        K, knote = self.load_input("K")
        self.input_notes = {"D": dnote, "H": hnote, "K": knote}
        M = self.load_map()

        # ---- C1 Commits
        commits = set()
        self.c1_term(commits, "C1.E.AuthorityRevision", E, ["AuthorityRevision"])
        d_base = self.c1_term(commits, "C1.D.BaseSha", D, ["BaseSha"])
        d_main = self.c1_term(commits, "C1.D.MainSha", D, ["MainSha"])
        self.c1_term(commits, "C1.D.ChainBaseSha", D, ["ChainBaseSha"])
        self.c1_term(commits, "C1.D.ChainRedSha", D, ["ChainRedSha"])
        self.c1_term(commits, "C1.H.BaseSha", H, ["BaseSha"])
        h_cur = self.c1_term(commits, "C1.H.CurrentSha", H, ["CurrentSha"])
        self.c1_term(commits, "C1.H.RedSha", H, ["RedSha"])
        self.c1_term(commits, "C1.I.Target.Commit", I, ["Target", "Commit"])
        k_auth = self.c1_term(commits, "C1.K.AuthorityRevision", K, ["AuthorityRevision"])
        self.c1_term(commits, "C1.K.MainSha", K, ["MainSha"])

        lbl = "C1.RevList(D.BaseSha..H.CurrentSha)"
        if D is None or H is None:
            self.omit(lbl, INPUT_ABSENT)
        elif d_base is None or h_cur is None:
            self.omit(lbl, PRIMITIVE_EMPTY)
        else:
            commits |= self.git.rev_list_range(d_base, h_cur)  # an empty set is a result, not an omission

        lbl = "C1.FirstParentChain(D.MainSha)"
        if D is None:
            self.omit(lbl, INPUT_ABSENT)
        elif d_main is None:
            self.omit(lbl, PRIMITIVE_EMPTY)
        else:
            chain = self.git.first_parent_chain(d_main)
            commits |= set(chain)
            for m, parents in self.git.parents_of(chain).items():
                if len(parents) >= 2:
                    commits |= set(parents)

        lbl = "C1.MergeBase(D.MainSha,K.AuthorityRevision)"
        if D is None or K is None:
            self.omit(lbl, INPUT_ABSENT)
        elif d_main is None or k_auth is None:
            self.omit(lbl, PRIMITIVE_EMPTY)
        else:
            mb = self.git.merge_base(d_main, k_auth)
            if mb is None:
                self.omit(lbl, PRIMITIVE_EMPTY)
            else:
                commits.add(mb)

        # ---- C2 Refs
        refs = {"HEAD", "refs/remotes/origin/main"}
        lbl = "C2.origin/D.ExpectedBranch"
        if D is None:
            self.omit(lbl, INPUT_ABSENT)
        else:
            cause, br = read_string(D, ["ExpectedBranch"], lbl)
            if cause is not None:
                self.omit(lbl, cause)
            else:
                refs.add("refs/remotes/origin/" + br)

        # ---- C3 RevPaths (identity by blob)
        revpaths = set()
        for c in sorted(commits):
            tb = self.git.tree_blobs(c)
            for (p, b) in self.all_pairs:
                if tb.get(p) == b:
                    revpaths.add((c, p))

        # ---- C4 Paths
        paths = set(p for (p, _b) in self.all_pairs)
        obligations = E.get("Obligations", [])
        if obligations is None:
            obligations = []
        if not isinstance(obligations, list):
            raise BenchError("E.Obligations is not a list")
        for o in obligations:
            if not isinstance(o, dict):
                raise BenchError("E.Obligations has a non-object element")
            if o.get("Class") != "READ":
                continue
            t = o.get("Target")
            if t is None or t == "":
                continue
            if not isinstance(t, str):
                raise BenchError("E.Obligations[].Target is not a string")
            paths.add(t)
        surfaces = M.get("Surfaces")
        files_m = M.get("Files")
        if not isinstance(surfaces, list) or not all(isinstance(s, str) for s in surfaces):
            raise BenchError("M.Surfaces is not a list of strings")
        if not isinstance(files_m, list) or not all(isinstance(f, dict) and isinstance(f.get("Path"), str) for f in files_m):
            raise BenchError("M.Files is not a list of objects with a string Path")
        paths |= set(s for s in surfaces if s != "")
        paths |= set(f["Path"] for f in files_m if f["Path"] != "")

        lbl = "C4.K.Authorities"
        if K is None:
            self.omit(lbl, INPUT_ABSENT)
        elif "Authorities" not in K:
            self.omit(lbl, FIELD_MISSING)
        elif K["Authorities"] is None:
            self.omit(lbl, FIELD_NULL)
        elif K["Authorities"] == "":
            self.omit(lbl, FIELD_EMPTY)
        elif not isinstance(K["Authorities"], list):
            raise BenchError("K.Authorities is not a list")
        else:
            for a in K["Authorities"]:  # [] contributes nothing and is not recorded
                cause, ap = read_string(a, ["Path"], lbl)
                if cause is not None:
                    self.omit(lbl, cause)  # first defective element, one record per term
                else:
                    paths.add(ap)

        lbl = "C4.IgnorePath(I)"
        parts = []
        for key in ("UnitId", "TaskId", "Attempt", "RunId"):
            cause, v = read_field(I, [key], lbl)
            if cause is not None:
                self.omit(lbl, cause)
                parts = None
                break
            if key == "Attempt":
                if isinstance(v, bool) or not isinstance(v, (int, str)):
                    raise BenchError("I.Attempt is neither an integer nor a string")
                v = str(v)
            elif not isinstance(v, str):
                raise BenchError("I.%s is not a string" % key)
            parts.append(v)
        if parts is not None:
            paths.add("artifacts/orchestration/%s/%s/%s/%s/output.json" % tuple(parts))

        # ---- C6 Files
        k_path = self.designated("K")
        deleg_re = h_re = None
        if D is None:
            self.omit("C6.DELEG", INPUT_ABSENT)
        else:
            cause, rid = read_string(D, ["RunId"], "C6.DELEG")
            if cause is not None:
                self.omit("C6.DELEG", cause)
            else:
                deleg_re = re.compile(r"artifacts/orchestration/[^/]+/[^/]+/[^/]+/" + re.escape(rid) + r"/output\.json")
        if H is None:
            self.omit("C6.H", INPUT_ABSENT)
        else:
            cause, rid = read_string(H, ["RunId"], "C6.H")
            if cause is not None:
                self.omit("C6.H", cause)
            else:
                h_re = re.compile(r"artifacts/orchestration/[^/]+/[^/]+/[^/]+/" + re.escape(rid) + r"/worker-handoff\.json")
        if K is None:
            self.omit("C6.CONTRACT", INPUT_ABSENT)

        files = set()
        seen_pairs = []
        for pb in self.entries["CanonicalInputs"]:
            if pb in seen_pairs:
                continue
            seen_pairs.append(pb)
            p, b = pb
            if k_path is not None and p == k_path:
                name = "CONTRACT"
                if K is None:
                    continue
            elif deleg_re is not None and deleg_re.fullmatch(p):
                name = "DELEG"
            elif h_re is not None and h_re.fullmatch(p):
                name = "H"
            elif p.endswith(".trx"):
                name = "TRX"
            else:
                continue
            data, why = self.verified_bytes(p, b)
            if data is None:
                raise BenchError("C6 entry %s named %s: %s" % (p, name, why))
            files.add((name, hashlib.sha256(data).hexdigest()))

        closure = {
            "Schema": SCHEMA_OUT,
            "Commits": sorted(commits),
            "RevPaths": [{"Rev": c, "Path": p} for (c, p) in sorted(revpaths)],
            "Refs": sorted(refs),
            "Paths": sorted(paths),
            "Objects": [],
            "Files": [{"Name": n, "Sha256": s} for (n, s) in sorted(files)],
        }
        omitted = [{"Term": l, "Cause": self.omitted[l]} for l in LABELS if l in self.omitted]
        return {"Closure": closure, "Omitted": omitted}


def main(argv):
    if len(argv) != 3:
        sys.stderr.write("usage: python -I derive_indep.py <vector.json> <out.json>\n")
        return 2
    if hasattr(sys, "set_int_max_str_digits"):
        sys.set_int_max_str_digits(0)
    try:
        result = Derivation(argv[1]).derive()
    except BenchError as ex:
        sys.stderr.write("BENCH_ERROR: %s\n" % ex)
        return 2
    except OSError as ex:
        sys.stderr.write("BENCH_ERROR: %s\n" % ex)
        return 2
    data = (json.dumps(result, indent=1, ensure_ascii=False) + "\n").encode("utf-8")
    with open(argv[2], "wb") as f:
        f.write(data)
    return 0


if __name__ == "__main__":
    sys.exit(main(sys.argv))
