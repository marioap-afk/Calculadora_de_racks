"""Builds the synthetic repository (synrepo/), the synthetic E/D/H/K files (synvec/) and the
synthetic vectors (vectors/syn-*.json) plus their hand-written expectations (synvec/expect.json).

    python -I make_synth.py

Writes only under D:\\r62-a6r4\\indep\\.  Deterministic (fixed identities and dates).
"""

import hashlib
import json
import os
import shutil
import subprocess

ROOT = os.path.dirname(os.path.abspath(__file__))
REPO = os.path.join(ROOT, "synrepo")
SYNVEC = os.path.join(ROOT, "synvec")
VECTORS = os.path.join(ROOT, "vectors")
SYNVEC_FWD = SYNVEC.replace("\\", "/")

ENV = {k: v for k, v in os.environ.items() if not k.upper().startswith("GIT_") or k.upper() == "GIT_EXEC_PATH"}
ENV.update({"GIT_AUTHOR_NAME": "Synth", "GIT_AUTHOR_EMAIL": "synth@example.invalid",
            "GIT_COMMITTER_NAME": "Synth", "GIT_COMMITTER_EMAIL": "synth@example.invalid"})
_clock = [1760000000]


def git(*args, stdin=None):
    env = dict(ENV)
    _clock[0] += 60
    env["GIT_AUTHOR_DATE"] = env["GIT_COMMITTER_DATE"] = "%d +0000" % _clock[0]
    p = subprocess.run(["git", "--no-replace-objects", "-C", REPO, "-c", "core.autocrlf=false"] + list(args),
                       input=stdin, stdout=subprocess.PIPE, stderr=subprocess.PIPE, env=env)
    if p.returncode != 0:
        raise SystemExit("git %s: %s" % (args, p.stderr.decode()))
    return p.stdout.decode().strip()


def blob_id(data):
    return hashlib.sha1(b"blob %d\0" % len(data) + data).hexdigest()


def write_blob(data):
    oid = git("hash-object", "-w", "--no-filters", "--stdin", stdin=data)
    assert oid == blob_id(data)
    return oid


def mktree(files):
    """files: {path: bytes} -> tree id (nested)."""
    here, sub = {}, {}
    for path, data in files.items():
        head, _, rest = path.partition("/")
        if rest:
            sub.setdefault(head, {})[rest] = data
        else:
            here[head] = data
    lines = []
    for name, data in here.items():
        lines.append("100644 blob %s\t%s" % (write_blob(data), name))
    for name, inner in sub.items():
        lines.append("040000 tree %s\t%s" % (mktree(inner), name))
    return git("mktree", stdin=("\n".join(lines) + "\n").encode("utf-8"))


def commit(files, parents, msg):
    args = ["commit-tree", mktree(files)]
    for p in parents:
        args += ["-p", p]
    return git(*(args + ["-m", msg]))


def jbytes(obj):
    return (json.dumps(obj, indent=1) + "\n").encode("utf-8")


def main():
    if os.path.isdir(REPO):
        shutil.rmtree(REPO)
    os.makedirs(REPO)
    subprocess.run(["git", "init", "-q", "-b", "main", REPO], check=True, env=ENV)
    for d in (SYNVEC, VECTORS):
        os.makedirs(d, exist_ok=True)
    for f in os.listdir(SYNVEC):
        os.remove(os.path.join(SYNVEC, f))
    for f in os.listdir(VECTORS):
        if f.startswith("syn-"):
            os.remove(os.path.join(VECTORS, f))

    M = {"Schema": "synthetic-clause-map", "Surfaces": ["AGENTS.md", "docs/"],
         "Files": [{"Path": "docs/w.md"}, {"Path": "docs/map-file.md"}]}
    M_BYTES = jbytes(M)
    TRX = b'<?xml version="1.0"?><TestRun/>\n'
    base = {"AGENTS.md": b"agents v0\n", "src/a.txt": b"a v0\n", "docs/w.md": b"w\n",
            "docs/map.json": M_BYTES, "docs/read-target.md": b"read me\n"}

    # main history: c0 -> (c1, s1) -> m (merge, first parent c1) -> c2
    c0 = commit(base, [], "c0 root")
    s1 = commit(dict(base, **{"side.txt": b"side\n"}), [c0], "s1 side")
    c1 = commit(dict(base, **{"docs/c1.md": b"c1\n"}), [c0], "c1 main")
    m_files = dict(base, **{"side.txt": b"side\n", "docs/c1.md": b"c1\n"})
    m = commit(m_files, [c1, s1], "m merge c1+s1")
    c2_files = dict(m_files, **{"docs/c2.md": b"c2\n"})
    c2 = commit(c2_files, [m], "c2 main tip")
    # feature: c2 -> r1 -> x1 -> g1
    r1_files = dict(c2_files, **{"tests/t.trx": TRX})
    r1 = commit(r1_files, [c2], "r1 red")
    x1_files = dict(r1_files, **{"docs/x1.md": b"x1\n"})
    x1 = commit(x1_files, [r1], "x1 intermediate")
    K_BASE = {"Schema": "synthetic-contract", "AuthorityRevision": c1, "MainSha": c2,
              "Authorities": [{"Path": "docs/auth-a.md"}, {"Path": "docs/auth-b.md"}]}
    K_PATH = "docs/decisions/SYN-K.json"
    K_BYTES = jbytes(K_BASE)
    g1 = commit(dict(x1_files, **{"src/a.txt": b"a v1\n", K_PATH: K_BYTES}), [x1], "g1 green")
    # unrelated root history
    u0 = commit({"u.txt": b"u0\n"}, [], "u0 unrelated root")
    u1 = commit({"u.txt": b"u1\n"}, [u0], "u1 unrelated")
    git("update-ref", "refs/heads/main", c2)
    git("update-ref", "refs/heads/fx/syn", g1)
    git("update-ref", "refs/heads/other", u1)
    tree_c2 = git("rev-parse", c2 + "^{tree}")
    nonexist = "1234567890abcdef1234567890abcdef12345678"
    rc = subprocess.run(["git", "--no-replace-objects", "-C", REPO, "cat-file", "-t", nonexist],
                        stdout=subprocess.PIPE, stderr=subprocess.PIPE, env=ENV).returncode
    assert rc != 0
    names = {"c0": c0, "s1": s1, "c1": c1, "m": m, "c2": c2, "r1": r1, "x1": x1, "g1": g1, "u0": u0, "u1": u1,
             "tree_c2": tree_c2, "nonexistent": nonexist}

    D_PATH = "artifacts/orchestration/SU/T1/0/RD001/output.json"
    H_PATH = "artifacts/orchestration/SU/T1/0/RH001/worker-handoff.json"
    TRX_PATH = "tests/t.trx"
    MAP_PATH = "docs/map.json"
    D_BASE = {"RunId": "RD001", "BaseSha": c2, "MainSha": c2, "ChainBaseSha": c2, "ChainRedSha": r1,
              "ExpectedBranch": "fx/syn"}
    H_BASE = {"RunId": "RH001", "BaseSha": c2, "CurrentSha": g1, "RedSha": r1}
    I_BASE = {"UnitId": "SU", "TaskId": "T1", "Attempt": 0, "RunId": "RC001",
              "Target": {"Commit": g1, "Path": "src/a.txt", "Blob": blob_id(b"a v1\n")}}

    trx_file = os.path.join(SYNVEC, "t.trx")
    with open(trx_file, "wb") as f:
        f.write(TRX)

    ALL = ["c0", "c1", "s1", "m", "c2", "r1", "x1", "g1"]
    H_ABSENT = {t: "INPUT_ABSENT" for t in ("C1.H.BaseSha", "C1.H.CurrentSha", "C1.H.RedSha",
                                            "C1.RevList(D.BaseSha..H.CurrentSha)", "C6.H")}
    D_ABSENT = {t: "INPUT_ABSENT" for t in ("C1.D.BaseSha", "C1.D.MainSha", "C1.D.ChainBaseSha", "C1.D.ChainRedSha",
                                            "C1.RevList(D.BaseSha..H.CurrentSha)", "C1.FirstParentChain(D.MainSha)",
                                            "C1.MergeBase(D.MainSha,K.AuthorityRevision)",
                                            "C2.origin/D.ExpectedBranch", "C6.DELEG")}
    K_ABSENT = {t: "INPUT_ABSENT" for t in ("C1.K.AuthorityRevision", "C1.K.MainSha",
                                            "C1.MergeBase(D.MainSha,K.AuthorityRevision)", "C4.K.Authorities",
                                            "C6.CONTRACT")}

    def cur(cause):
        return {"C1.H.CurrentSha": cause, "C1.RevList(D.BaseSha..H.CurrentSha)": "PRIMITIVE_EMPTY"}

    def without(d, key):
        d = dict(d)
        del d[key]
        return d

    NO_X1 = [n for n in ALL if n != "x1"]
    # name: (description, overrides, expected)
    # overrides: D/H/K = object (json-encoded) or ("raw", bytes) or None (=> not pinned in E);
    #            *_custody_override = file name; *_pin_override = blob; I = object
    cases = [
        ("all_present", "every input present and valid", {}, {"omitted": {}, "commits": ALL}),
        ("h_absent_unpinned", "E has no entry at the designated H path", {"H": None},
         {"omitted": H_ABSENT, "commits": NO_X1}),
        ("h_custody_missing", "H path pinned in E, custody file does not exist",
         {"H_custody_override": "does-not-exist.json"}, {"omitted": H_ABSENT, "commits": NO_X1}),
        ("h_blob_mismatch", "custodied H bytes have another blob than the pinned one",
         {"H_pin_override": blob_id(jbytes(H_BASE) + b" ")}, {"omitted": H_ABSENT, "commits": NO_X1}),
        ("h_invalid_json", "H bytes are not JSON (truncated)",
         {"H": ("raw", b'{"RunId": "RH001", "CurrentSha": ')}, {"omitted": H_ABSENT, "commits": NO_X1}),
        ("h_dupkey", "H has a duplicated key in a nested object",
         {"H": ("raw", ('{"RunId":"RH001","BaseSha":"%s","CurrentSha":"%s","RedSha":"%s","X":{"k":1,"k":2}}\n'
                        % (c2, g1, r1)).encode())}, {"omitted": H_ABSENT, "commits": NO_X1}),
        ("h_bom", "H is valid JSON preceded by a UTF-8 BOM",
         {"H": ("raw", b"\xef\xbb\xbf" + jbytes(H_BASE))}, {"omitted": H_ABSENT, "commits": NO_X1}),
        ("h_not_object", "H is a JSON array, not an object",
         {"H": ("raw", b"[1, 2]\n")}, {"omitted": H_ABSENT, "commits": NO_X1}),
        ("h_current_missing", "H.CurrentSha absent", {"H": without(H_BASE, "CurrentSha")},
         {"omitted": cur("FIELD_MISSING"), "commits": NO_X1}),
        ("h_current_null", "H.CurrentSha null", {"H": dict(H_BASE, CurrentSha=None)},
         {"omitted": cur("FIELD_NULL"), "commits": NO_X1}),
        ("h_current_empty", 'H.CurrentSha ""', {"H": dict(H_BASE, CurrentSha="")},
         {"omitted": cur("FIELD_EMPTY"), "commits": NO_X1}),
        ("h_current_upper", "H.CurrentSha = uppercase hex of g1", {"H": dict(H_BASE, CurrentSha=g1.upper())},
         {"omitted": cur("NOT_A_COMMIT"), "commits": NO_X1}),
        ("h_current_39hex", "H.CurrentSha = first 39 hex of g1", {"H": dict(H_BASE, CurrentSha=g1[:39])},
         {"omitted": cur("NOT_A_COMMIT"), "commits": NO_X1}),
        ("h_current_tree", "H.CurrentSha = tree id of c2", {"H": dict(H_BASE, CurrentSha=tree_c2)},
         {"omitted": cur("NOT_A_COMMIT"), "commits": NO_X1}),
        ("h_current_nonexistent", "H.CurrentSha = nonexistent 40-hex", {"H": dict(H_BASE, CurrentSha=nonexist)},
         {"omitted": cur("NOT_A_COMMIT"), "commits": NO_X1}),
        ("h_red_null", "H.RedSha null", {"H": dict(H_BASE, RedSha=None)},
         {"omitted": {"C1.H.RedSha": "FIELD_NULL"}, "commits": ALL}),
        ("d_absent_unpinned", "E has no entry at the designated D path", {"D": None},
         {"omitted": D_ABSENT, "commits": ["c1", "c2", "r1", "g1"],
          "refs": ["HEAD", "refs/remotes/origin/main"]}),
        ("k_absent_unpinned", "E has no entry at the designated K path", {"K": None},
         {"omitted": K_ABSENT, "commits": ALL}),
        ("k_invalid_json", "K pinned (custody) with verified bytes that are not JSON",
         {"K": ("raw", b"not json\n")}, {"omitted": K_ABSENT, "commits": ALL}),
        ("k_authorities_empty", "K.Authorities = [] (legitimately empty, not recorded)",
         {"K": dict(K_BASE, Authorities=[])}, {"omitted": {}, "commits": ALL}),
        ("d_branch_null", "D.ExpectedBranch null", {"D": dict(D_BASE, ExpectedBranch=None)},
         {"omitted": {"C2.origin/D.ExpectedBranch": "FIELD_NULL"}, "commits": ALL,
          "refs": ["HEAD", "refs/remotes/origin/main"]}),
        ("mergebase_none", "K.AuthorityRevision = u1 (unrelated root history)",
         {"K": dict(K_BASE, AuthorityRevision=u1)},
         {"omitted": {"C1.MergeBase(D.MainSha,K.AuthorityRevision)": "PRIMITIVE_EMPTY"}, "commits": ALL + ["u1"]}),
        ("target_nonexistent", "I.Target.Commit = nonexistent 40-hex",
         {"I": dict(I_BASE, Target=dict(I_BASE["Target"], Commit=nonexist))},
         {"omitted": {"C1.I.Target.Commit": "NOT_A_COMMIT"}, "commits": ALL}),
        ("trx_unverifiable", "custodied TRX bytes do not have the pinned blob (bench error)",
         {"TRX_pin_override": blob_id(TRX + b"x")}, {"exit": 2}),
    ]

    expect = {"names": names, "cases": {}}
    for name, desc, ov, exp in cases:
        custody = {}
        entries_ci = []

        def pin(kind, path, base_obj, git_bytes=None):
            val = ov.get(kind, base_obj) if kind in ov else base_obj
            if val is None:
                return None  # not pinned
            if git_bytes is not None and kind not in ov:
                return blob_id(git_bytes)  # bytes from the -C (no custody)
            data = val[1] if isinstance(val, tuple) else jbytes(val)
            fname = "%s-%s.json" % (name, kind)
            with open(os.path.join(SYNVEC, fname), "wb") as f:
                f.write(data)
            custody[path] = SYNVEC_FWD + "/" + ov.get(kind + "_custody_override", fname)
            return ov.get(kind + "_pin_override", blob_id(data))

        kb = pin("K", K_PATH, K_BASE, git_bytes=K_BYTES)
        if kb:
            entries_ci.append({"Path": K_PATH, "Blob": kb})
        entries_ci.append({"Path": "src/a.txt", "Blob": blob_id(b"a v0\n")})
        entries_ci.append({"Path": "src/a.txt", "Blob": blob_id(b"a v1\n")})
        db = pin("D", D_PATH, D_BASE)
        if db:
            entries_ci.append({"Path": D_PATH, "Blob": db})
        hb = pin("H", H_PATH, H_BASE)
        if hb:
            entries_ci.append({"Path": H_PATH, "Blob": hb})
        entries_ci.append({"Path": TRX_PATH, "Blob": ov.get("TRX_pin_override", blob_id(TRX))})
        custody[TRX_PATH] = SYNVEC_FWD + "/t.trx"
        E = {"Schema": "rackcad-input-closure/v1", "AuthorityRevision": c1,
             "CanonicalInputs": entries_ci,
             "AutomaticInstructions": [{"Path": "AGENTS.md", "Blob": blob_id(b"agents v0\n"), "Adapter": "x"}],
             "Obligations": [{"Class": "READ", "Target": "docs/read-target.md"},
                             {"Class": "READ", "Target": None},
                             {"Class": "ACTION_COMPATIBLE", "Target": "git status"}],
             "AllowedTransitiveInputs": [{"Path": MAP_PATH, "Blob": blob_id(M_BYTES)},
                                         {"Path": "docs/w.md", "Blob": blob_id(b"w\n")}]}
        e_name = "E-%s.json" % name
        with open(os.path.join(SYNVEC, e_name), "wb") as f:
            f.write(jbytes(E))
        vec = {"Repo": REPO.replace("\\", "/"), "E": SYNVEC_FWD + "/" + e_name,
               "I": ov.get("I", I_BASE),
               "Designation": {"D": D_PATH, "H": H_PATH, "K": K_PATH},
               "Custody": custody, "MapPath": MAP_PATH}
        with open(os.path.join(VECTORS, "syn-%s.json" % name), "wb") as f:
            f.write(jbytes(vec))
        e = {"description": desc, "exit": exp.get("exit", 0)}
        if e["exit"] == 0:
            e["omitted"] = exp["omitted"]
            e["commits"] = sorted(names[n] for n in exp["commits"])
            e["refs"] = sorted(exp.get("refs", ["HEAD", "refs/remotes/origin/fx/syn", "refs/remotes/origin/main"]))
        expect["cases"][name] = e
    with open(os.path.join(SYNVEC, "expect.json"), "wb") as f:
        f.write(jbytes(expect))
    print("built %d vectors; names: %s" % (len(cases), json.dumps(names)))


if __name__ == "__main__":
    main()
