# r3lib.py - shared helpers for the I-62 A-6 round-2 "R3" independence evidence.
# Standard library only. Loaded by the part scripts through importlib (they run with python -I).
#
# Never imports, executes in-process or copies logic from the pinned acceptor or the pinned tool:
# both are only executed as separate processes, exactly as obtained by blob.

import ast
import hashlib
import json
import os
import platform
import re
import subprocess
import sys

HERE = os.path.dirname(os.path.abspath(__file__))
WORK = os.environ.get("R3_WORK", "D:/r62-a6r2/r3").replace("\\", "/")
OUT = os.environ.get("R3_OUT") or os.path.normpath(os.path.join(HERE, "..", "results"))
WT = os.path.join(os.environ["USERPROFILE"], ".codex", "worktrees",
                  "architecture-portabilidad-coordinador-principal")
ORIGIN = "D:/r62-fixture/fixture-origin.git"
PKG = "docs/automation/evidence/I-62-F6/FX-02/bmin"
SCHEMAS = "docs/automation/agent-execution/schemas"

TOOL_BLOB = "9a829b0eeb1210238052b1feac06cfa8f7964aed"
ACC_BLOB = "434ef1fccb0d15762b30244b4a60928120698411"
MUTATE_BLOB = "6967938954219a6a8a2f38dc80ef5a1165966d19"

PIN = WORK + "/pin"
TOOL = PIN + "/tool/bmin_facts.py"
ACC = PIN + "/accept/bmin_accept.py"
MUTDIR = WORK + "/mut"
REAL = WORK + "/real"
DELIV = WORK + "/deliv"
SYN = WORK + "/syn"
RUNS = WORK + "/runs"

# Real FX-U1 scenario of the previous end-to-end harness (e2e.py), unchanged.
BR = "fx/u1"
BASE = "1a4fc9c62bb564d7c51fad8158876a5d5a8a5bdf"
HEADX = "ae25b596a11fe3508ac60a3ef7de5dc23df971ce"
RED = "d30fb6a9d26c0b5cb189a30b98ec97d0edcb7b99"
MAIN = "fbe25347799e5b801ef708454212335537448bd1"
TIP = "95bdc29d428b35d2188a10e1c5462d6caf2be4df"
AR = "1746b404e57d69170ecfc084fd24fa3e699916d7"
K = "docs/automation/decisions/FX-U1-T1.gate-contract.json"
STATE = "docs/automation/state/FX-U1.yml"
DEC = "docs/automation/decisions/FX-U1.md"
AP_PATH = "docs/AUTOMATION_PLAN.md"
WF_PATH = "docs/WORKFLOW.md"
MAP_PATH = "docs/automation/agent-execution/compatibility/I62-clause-map.json"
MAP_SCHEMA_PATH = "docs/automation/agent-execution/compatibility/clause-map.schema.json"
HANDOFF_SCHEMA_PATH = "docs/automation/agent-execution/schemas/worker-handoff.schema.json"
TRX_SRC = "docs/automation/evidence/I-62-F6/FX-02/U07-trx-R37949808974/fixture-tests.trx"
TRX_SHA = "4ccdf2cecba561259b877a471d577f1afa6ac4a9ad00ceb4fa0417e7c61a7513"
TRAILER = "Agent-Protocol-Normative: I-62"

R_IDS = ("R1", "R2", "R3", "R4", "R5", "R6", "R7", "R8", "R9", "R10")
NL = "\n"


# ------------------------------------------------------------------ basics

def canon(obj):
    return json.dumps(obj, ensure_ascii=True, sort_keys=True, separators=(",", ":"))


def sha256_bytes(data):
    return hashlib.sha256(data).hexdigest()


def sha256_file(path):
    with open(path, "rb") as fh:
        return sha256_bytes(fh.read())


def blob_id(data):
    """Git blob id computed by the harness (third implementation, hashlib)."""
    return hashlib.sha1(b"blob " + str(len(data)).encode("ascii") + b"\0" + data).hexdigest()


def read_bytes(path):
    with open(path, "rb") as fh:
        return fh.read()


def write_bytes(path, data):
    d = os.path.dirname(path)
    if d:
        os.makedirs(d, exist_ok=True)
    with open(path, "wb") as fh:
        fh.write(data)


def write_json_file(path, obj, pretty=False):
    if pretty:
        text = json.dumps(obj, ensure_ascii=False, indent=1, sort_keys=False) + NL
        write_bytes(path, text.encode("utf-8"))
    else:
        write_bytes(path, (canon(obj) + NL).encode("ascii"))
    return sha256_file(path)


def clean_env(extra=None):
    env = dict((k, v) for k, v in os.environ.items() if not k.upper().startswith("GIT_"))
    env["GIT_OPTIONAL_LOCKS"] = "0"
    env["GIT_TERMINAL_PROMPT"] = "0"
    env["GIT_NO_REPLACE_OBJECTS"] = "1"
    env["LC_ALL"] = "C"
    if extra:
        env.update(extra)
    return env


def git(repo, *args, inp=None, extra_env=None, ok=(0,)):
    p = subprocess.run(["git", "-C", repo] + list(args), input=inp, stdout=subprocess.PIPE,
                       stderr=subprocess.PIPE, env=clean_env(extra_env))
    if p.returncode not in ok:
        raise RuntimeError("git %s failed rc=%d: %s" % (" ".join(args[:3]), p.returncode,
                                                         p.stderr.decode("utf-8", "replace")[:300]))
    return p.stdout


def gits(repo, *args, **kw):
    return git(repo, *args, **kw).decode("utf-8").strip()


def exe_sha256():
    """SHA-256 of the interpreter image (sys.executable, or the same name under sys.base_prefix when the
    former is an App Execution Alias). Written by the harness; mirrors what R2 needs to be declared."""
    for path in (sys.executable, os.path.join(sys.base_prefix, os.path.basename(sys.executable or "python"))):
        try:
            return sha256_file(path)
        except OSError:
            continue
    raise RuntimeError("interpreter image unreadable")


# ------------------------------------------------------------------ pinned bytes

PIN_FILES = {
    "tool/bmin_facts.py": TOOL_BLOB,
    "accept/bmin_accept.py": ACC_BLOB,
    "tool/tests/mutate.py": MUTATE_BLOB,
}


def extract_pinned():
    """Writes the pinned tool, acceptor and mutate.py from their blobs (never from the working tree)."""
    out = {}
    for rel, blob in PIN_FILES.items():
        data = git(WT, "cat-file", "blob", blob)
        if blob_id(data) != blob:
            raise RuntimeError("blob mismatch for " + rel)
        write_bytes(PIN + "/" + rel, data)
        out[rel] = {"Blob": blob, "Sha256": sha256_bytes(data), "Bytes": len(data)}
    return out


def schema_bytes(name):
    """A schema of the I62 set from the worktree HEAD tree (read only)."""
    return git(WT, "show", "HEAD:" + SCHEMAS + "/" + name)


def mutants():
    """The 27 mutant definitions, read from mutate.py with ast (mutate.py is never executed or edited)."""
    src = read_bytes(PIN + "/tool/tests/mutate.py").decode("ascii")
    tree = ast.parse(src)
    for node in tree.body:
        if isinstance(node, ast.Assign) and any(isinstance(t, ast.Name) and t.id == "MUTANTS" for t in node.targets):
            return [tuple(m) for m in ast.literal_eval(node.value)]
    raise RuntimeError("MUTANTS not found")


def build_mutants():
    src = read_bytes(TOOL).decode("ascii")
    out = []
    for mid, desc, anchor, repl, hints in mutants():
        n = src.count(anchor)
        if n != 1:
            raise RuntimeError("%s anchor occurs %d times" % (mid, n))
        mutated = src.replace(anchor, repl)
        compile(mutated, mid + ".py", "exec")
        data = mutated.encode("ascii")
        path = MUTDIR + "/" + mid + "/bmin_facts.py"
        write_bytes(path, data)
        out.append({"Id": mid, "Description": desc, "Path": path, "Blob": blob_id(data), "Sha256": sha256_bytes(data),
                    "AnchorLine": src[:src.index(anchor)].count("\n") + 1})
    return out


# ------------------------------------------------------------------ process runners

def run_tool(tool, argv, isolated=True, cwd=None):
    cmd = [sys.executable] + (["-I"] if isolated else []) + [tool] + list(argv)
    p = subprocess.run(cmd, stdin=subprocess.DEVNULL, stdout=subprocess.PIPE, stderr=subprocess.PIPE,
                       env=clean_env(), cwd=cwd or WORK, timeout=900)
    return p.returncode, p.stdout


def run_acceptor(request_path):
    p = subprocess.run([sys.executable, "-I", ACC, "--request", request_path], stdin=subprocess.DEVNULL,
                       stdout=subprocess.PIPE, stderr=subprocess.PIPE, env=clean_env(), cwd=WORK, timeout=3600)
    try:
        res = json.loads(p.stdout.decode("ascii"))
    except Exception:
        res = {"Unparsable": True, "Stderr": p.stderr.decode("utf-8", "replace")[-1500:]}
    return p.returncode, res


def acceptor_summary(rc, res):
    checks = res.get("Checks", []) if isinstance(res, dict) else []
    st = dict((c.get("Id"), c.get("Status")) for c in checks)
    return {"Exit": rc, "Decision": res.get("Decision") if isinstance(res, dict) else None,
            "Status": st, "Mismatch": [i for i in R_IDS if st.get(i) == "MISMATCH"],
            "Error": res.get("Error") if isinstance(res, dict) else None}


def mismatch_fields(res, limit=12):
    """Per check, the compared fields the acceptor reported as different (names only, plus problems)."""
    out = {}
    for c in res.get("Checks", []) if isinstance(res, dict) else []:
        if c.get("Status") != "MISMATCH":
            continue
        d = c.get("Detail")
        items = []
        if isinstance(d, dict):
            for run in d.get("Runs", []) if isinstance(d.get("Runs"), list) else []:
                for m in run.get("Mismatches", []) or []:
                    items.append(run.get("Name", "?") + ":" + str(m.get("Field")) + (
                        " (" + m["Problem"] + ")" if m.get("Problem") else ""))
                for p in run.get("Problems", []) or []:
                    items.append(run.get("Name", "?") + ": " + str(p))
                for v in run.get("Violations", []) or []:
                    items.append(run.get("Name", "?") + ": " + str(v))
            for row in d.get("Checks", []) if isinstance(d.get("Checks"), list) else []:
                if row.get("Status") == "MISMATCH":
                    items.append("R4." + str(row.get("Check")) + " controller=" + str(row.get("Controller"))
                                 + " acceptor_expects=" + str(row.get("AcceptorExpects")))
            for row in d.get("AuthorityResolution", []) if isinstance(d.get("AuthorityResolution"), list) else []:
                if row.get("Status") == "MISMATCH":
                    items.append("AuthorityResolution[%s] EffectiveSha=%s ClauseMapBlob=%s"
                                 % (row.get("Index"), row.get("EffectiveSha"), row.get("ClauseMapBlob")))
            for p in d.get("Problems", []) if isinstance(d.get("Problems"), list) else []:
                items.append(str(p))
            for k in ("Error", "Detail", "Missing", "Problem"):
                if k in d and not isinstance(d[k], (dict, list)):
                    items.append(k + ": " + str(d[k]))
        out[c["Id"]] = items[:limit] + (["... (%d more)" % (len(items) - limit)] if len(items) > limit else [])
    return out


# ------------------------------------------------------------------ envelopes

ENV_FIELDS = ("Facts", "FactsSha256", "Error", "GitCalls", "Inputs")


def parse_env(raw):
    try:
        return json.loads(raw.decode("utf-8"))
    except Exception:
        return None


def env_diff(raw_a, raw_b):
    """Fields among Facts/FactsSha256/Error/GitCalls/Inputs (and raw bytes) that differ."""
    a, b = parse_env(raw_a), parse_env(raw_b)
    diffs = []
    if a is None or b is None:
        return ["<unparsable>"] if raw_a != raw_b else []
    for k in ENV_FIELDS:
        if canon(a.get(k)) != canon(b.get(k)):
            diffs.append(k)
    if not diffs and raw_a != raw_b:
        rest = [k for k in ("Runtime", "Args", "Command", "Schema") if canon(a.get(k)) != canon(b.get(k))]
        diffs.append("bytes-only" + ("(" + ",".join(rest) + ")" if rest else "(encoding)"))
    return diffs


def facts_paths_diff(raw_a, raw_b, limit=8):
    """Leaf paths of Facts that differ (for the activation record)."""
    a, b = parse_env(raw_a), parse_env(raw_b)
    if a is None or b is None:
        return []
    out = []

    def walk(x, y, path):
        if len(out) >= limit:
            return
        if isinstance(x, dict) and isinstance(y, dict):
            for k in sorted(set(x) | set(y)):
                if k not in x or k not in y:
                    out.append(path + "/" + k)
                else:
                    walk(x[k], y[k], path + "/" + k)
        elif isinstance(x, list) and isinstance(y, list) and len(x) == len(y):
            for i, (p, q) in enumerate(zip(x, y)):
                walk(p, q, path + "/" + str(i))
        elif canon(x) != canon(y):
            out.append(path or "/")
    if a.get("Error") is not None or b.get("Error") is not None:
        ea = (a.get("Error") or {}).get("Code")
        eb = (b.get("Error") or {}).get("Code")
        if ea != eb:
            out.append("Error.Code: %s -> %s" % (ea, eb))
    walk(a.get("Facts"), b.get("Facts"), "Facts")
    return out[:limit]


# ------------------------------------------------------------------ synthetic repositories

COMMIT_ENV = {"GIT_AUTHOR_NAME": "R3 Synthetic", "GIT_AUTHOR_EMAIL": "r3.invalid",
              "GIT_COMMITTER_NAME": "R3 Synthetic", "GIT_COMMITTER_EMAIL": "r3.invalid"}
DATE0 = 1760140800  # 2025-10-11T00:00:00Z; fixed so every synthetic SHA is reproducible


def hash_blob(repo, data):
    return gits(repo, "hash-object", "-w", "--no-filters", "--stdin", inp=data)


def make_tree(repo, files, base=None, removes=()):
    """Tree from an optional base tree, minus removes, plus files {path: bytes}."""
    idx = os.path.join(repo, ".git", "r3-index").replace("\\", "/")
    if os.path.exists(idx):
        os.remove(idx)
    env = {"GIT_INDEX_FILE": idx}
    if base:
        git(repo, "read-tree", base, extra_env=env)
    else:
        git(repo, "read-tree", "--empty", extra_env=env)
    for p in removes:
        git(repo, "update-index", "--force-remove", "--", p, extra_env=env)
    for p in sorted(files):
        oid = hash_blob(repo, files[p])
        git(repo, "update-index", "--add", "--cacheinfo", "100644,%s,%s" % (oid, p), extra_env=env)
    t = gits(repo, "write-tree", extra_env=env)
    os.remove(idx)
    return t


def make_commit(repo, tree, parents, message, seq):
    env = dict(COMMIT_ENV)
    env["GIT_AUTHOR_DATE"] = "@%d +0000" % (DATE0 + 60 * seq)
    env["GIT_COMMITTER_DATE"] = env["GIT_AUTHOR_DATE"]
    args = ["commit-tree", tree]
    for p in parents:
        args += ["-p", p]
    args += ["-F", "-"]
    return gits(repo, *args, inp=message.encode("utf-8"), extra_env=env)


def all_commits(repo):
    out = git(repo, "cat-file", "--batch-all-objects", "--batch-check=%(objectname) %(objecttype)").decode("ascii")
    return sorted(ln.split(" ")[0] for ln in out.splitlines() if ln.endswith(" commit"))


def tree_paths(repo, commit):
    out = git(repo, "ls-tree", "-r", "-z", "--full-tree", "--name-only", commit)
    return [x.decode("utf-8") for x in out.split(b"\0") if x]


def broad_closure(repo, extra_paths, files, branch):
    """Permissive R9 closure used in part (a): every commit object, every rev:path of every commit, every path
    of every tree plus the scenario pathspecs. R9 then only judges the form of the git calls."""
    commits = all_commits(repo)
    revpaths = []
    paths = set(extra_paths)
    for c in commits:
        for p in tree_paths(repo, c):
            revpaths.append({"Rev": c, "Path": p})
            paths.add(p)
    return {"Schema": "rackcad-bmin-closure/0-experimental", "Commits": commits, "RevPaths": revpaths,
            "Refs": ["HEAD", "refs/remotes/origin/" + branch, "refs/remotes/origin/main"],
            "Paths": sorted(paths), "Objects": [], "Files": files}


# ------------------------------------------------------------------ JSON schema subset (harness validator)

def validate_schema(inst, schema, ptr="", out=None):
    """Minimal draft 2020-12 subset used only to check that the inputs the harness builds conform to the
    I62 schemas (type, enum, const, required, properties, additionalProperties, items, minItems, minLength,
    pattern, minimum, maximum). Written for the harness; independent of both pinned pieces."""
    if out is None:
        out = []
    if schema is True or schema is None:
        return out
    if schema is False:
        out.append(ptr + ": false")
        return out
    t = schema.get("type")
    if t is not None:
        ts = t if isinstance(t, list) else [t]
        ok = False
        for x in ts:
            if (x == "null" and inst is None) or (x == "boolean" and isinstance(inst, bool)) or \
               (x == "object" and isinstance(inst, dict)) or (x == "array" and isinstance(inst, list)) or \
               (x == "string" and isinstance(inst, str)) or \
               (x == "integer" and isinstance(inst, int) and not isinstance(inst, bool)) or \
               (x == "number" and isinstance(inst, (int, float)) and not isinstance(inst, bool)):
                ok = True
        if not ok:
            out.append(ptr + ": type")
            return out
    if "enum" in schema and inst not in schema["enum"]:
        out.append(ptr + ": enum")
    if "const" in schema and inst != schema["const"]:
        out.append(ptr + ": const")
    if isinstance(inst, dict):
        for r in schema.get("required", []):
            if r not in inst:
                out.append(ptr + "/" + r + ": required")
        props = schema.get("properties", {})
        for k, v in inst.items():
            if k in props:
                validate_schema(v, props[k], ptr + "/" + k, out)
            elif schema.get("additionalProperties") is False:
                out.append(ptr + "/" + k + ": additionalProperties")
    if isinstance(inst, list):
        if "items" in schema:
            for i, v in enumerate(inst):
                validate_schema(v, schema["items"], ptr + "/" + str(i), out)
        if "minItems" in schema and len(inst) < schema["minItems"]:
            out.append(ptr + ": minItems")
    if isinstance(inst, str):
        if "minLength" in schema and len(inst) < schema["minLength"]:
            out.append(ptr + ": minLength")
        if "pattern" in schema:
            pat = schema["pattern"]
            if pat.endswith("$"):
                pat = pat[:-1] + r"\Z"
            if re.search(pat, inst) is None:
                out.append(ptr + ": pattern")
    if isinstance(inst, (int, float)) and not isinstance(inst, bool):
        if "minimum" in schema and inst < schema["minimum"]:
            out.append(ptr + ": minimum")
        if "maximum" in schema and inst > schema["maximum"]:
            out.append(ptr + ": maximum")
    return out


# ------------------------------------------------------------------ deliverable hygiene

EMAIL_RE = re.compile(r"[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Za-z]{2,}")


def short_name(name):
    """8.3 short form of the profile folder name (first six significant characters, upper case, '~1')."""
    core = "".join(c for c in name if c not in " .").upper()
    return core[:6] + "~1"


def _user_forms():
    forms = []
    prof = os.environ.get("USERPROFILE") or ""
    if len(prof) > 3:
        forms += [(prof, "%USERPROFILE%"), (prof.replace("\\", "/"), "%USERPROFILE%")]
    name = os.path.basename(prof.rstrip("\\/")) if prof else ""
    if name:
        forms.append((name, "<usuario>"))
    if name:
        forms.append((short_name(name), "<usuario>"))
    return forms


def sanitize_str(s):
    for form, rep in _user_forms():
        low = s.lower()
        k = low.find(form.lower())
        while k >= 0:
            s = s[:k] + rep + s[k + len(form):]
            low = s.lower()
            k = low.find(form.lower(), k + len(rep))
    return EMAIL_RE.sub("<email>", s)


def sanitize(obj):
    if isinstance(obj, dict):
        return dict((sanitize_str(k), sanitize(v)) for k, v in obj.items())
    if isinstance(obj, list):
        return [sanitize(x) for x in obj]
    if isinstance(obj, str):
        return sanitize_str(obj)
    return obj


def write_result(name, obj):
    """Canonical ASCII JSON (SPEC 1.5 form: sorted keys, compact separators, ensure_ascii) plus LF."""
    os.makedirs(OUT, exist_ok=True)
    path = os.path.join(OUT, name)
    data = (canon(sanitize(obj)) + NL).encode("ascii")
    write_bytes(path, data)
    return path, sha256_bytes(data)


def log(*a):
    print(*a, flush=True)


def common_header():
    return {"Python": platform.python_version(), "Git": gits(WORK, "--version") if os.path.isdir(WORK) else None,
            "ToolBlob": TOOL_BLOB, "AcceptorBlob": ACC_BLOB, "MutateBlob": MUTATE_BLOB,
            "ToolSha256": sha256_file(TOOL) if os.path.exists(TOOL) else None,
            "AcceptorSha256": sha256_file(ACC) if os.path.exists(ACC) else None}
