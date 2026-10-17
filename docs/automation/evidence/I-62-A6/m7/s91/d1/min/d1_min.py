# d1_min.py - minimal reproducible case of divergence D-1 (I-62, A-6; decisiones §91, punto 3).
#
# D-1: in the `contract` order (A3/A4 of AP 16.5) the pinned instrument (blob 9a829b0e) applies U-19 to neither entry of a
# containment (covers(b, e), bmin_facts.py:739/741), while the pinned acceptor (blob 434ef1fc) applies U-19 to both
# (contained(a, b) = covers(u19(b), u19(a)), bmin_accept.py:722-732). This script builds the smallest contract/delegation
# pairs on which the instrument's `contract` facts and the acceptor's R7 differ, and the nearest pairs on which they agree.
#
# Repo state: a fresh repository with NO commit, NO ref other than the unborn HEAD, NO file in the tree, holding exactly one
# loose object: the instrument blob, written with `git hash-object -w --no-filters --stdin` from the bytes read by blob in
# the I-62 worktree (read only). `contract` makes no Git call (GitCalls = []); the repository only serves the declared form.
#
# Instrument: the declared form, in pwsh 7, cwd = the repository:
#   & '<git>' --no-replace-objects cat-file blob 9a829b0e... | & '<python>' -I - contract --repo <repo> --file ... --sha256 ...
# Acceptor: the pinned bytes (434ef1fc) written to pinned/bmin_accept.py, verified by blob and SHA-256 before every run,
# executed with `python -I`, read only, Tool.RunFrom = object (the loose object of the repository). Requests as the probes of
# readings/probes (R4, R5, R6, R10 not applicable; no verification).
#
# Stdlib only. Writes only under the folder of this script. Run: python -I d1_min.py
import hashlib
import json
import os
import platform
import re
import shutil
import subprocess
import sys

HERE = os.path.dirname(os.path.abspath(__file__)).replace("\\", "/")   # forward slashes: shell-safe tokens, as the bench
W =os.path.join(os.environ["USERPROFILE"], ".codex", "worktrees", "architecture-portabilidad-coordinador-principal")
TOOL_BLOB = "9a829b0eeb1210238052b1feac06cfa8f7964aed"
TOOL_SHA256 = "b03c3a19cef608386afd7c2e0618b884bfd7b7f6913e95d8ed70272d517a0200"
ACC_BLOB = "434ef1fccb0d15762b30244b4a60928120698411"
ACC_SHA256 = "173514aa353f6add1b11ea73ef6da68bba752bacdca64baa0551a6a7be6d8bb0"
# declared interpreter and Git of the m7 bench (p0-env.json): checked, never assumed
DECL_PY_SHA256 = "ad169f4cb4bfb78c7a5c030a4529c19d6643276778e33994c93e145b6191c3ec"
DECL_PY_VERSION = "3.13.14"
DECL_GIT_SHA256 = "81ef35ae005ca9318018d18e3327578ce939fb99feaad6b2d7c8ab15f3de8db5"
SAFE = re.compile(r"[A-Za-z0-9_./:=+~-]+\Z")
EMAIL = re.compile(r"[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Za-z]{2,}")

CONTRACT_REQUIRED = ("Schema", "Unit", "Gate", "TaskId", "Objective", "AuthorityRevision", "MainSha", "Authorities",
                     "AllowedWriteScope", "ForbiddenWriteScope", "Invariants", "RequiredTests", "ExpectedEvidence",
                     "StopConditions", "RoutingEnforcement", "CorrectionsAuthorized", "IssuedBy", "IssuedUtc",
                     "RoleRequirements", "ProtocolSet", "MaterializationCloseSha", "SupersededCommits", "SupersededBaseSha")
DELEG_FIELDS = ("AllowedWriteScope", "ForbiddenWriteScope", "Invariants", "StopConditions", "Authorities", "RequiredTests",
                "RoleRequirements", "SupersededCommits")


def genv():
    e = dict((k, v) for k, v in os.environ.items() if not k.upper().startswith("GIT_"))
    e["GIT_OPTIONAL_LOCKS"] = "0"
    e["GIT_TERMINAL_PROMPT"] = "0"
    return e


def git(repo, *args, inp=None):
    cmd = ["git"] + (["-C", repo] if repo else []) + list(args)
    return subprocess.run(cmd, input=inp, stdin=None if inp is not None else subprocess.DEVNULL,
                          capture_output=True, env=genv())


def blob_id(b):
    return hashlib.sha1(b"blob " + str(len(b)).encode("ascii") + b"\0" + b).hexdigest()


def sha256(b):
    return hashlib.sha256(b).hexdigest()


def rbytes(p):
    with open(p, "rb") as fh:
        return fh.read()


def wbytes(p, b):
    os.makedirs(os.path.dirname(p), exist_ok=True)
    with open(p, "wb") as fh:
        fh.write(b)


def jbytes(o):
    return (json.dumps(o, ensure_ascii=True, sort_keys=True, indent=1) + "\n").encode("ascii")


def sanitize(text):
    prof = os.environ.get("USERPROFILE", "")
    for form in (prof, prof.replace("\\", "/"), prof.replace("\\", "\\\\")):
        if len(form) > 3:
            text = re.sub(re.escape(form), "%USERPROFILE%", text, flags=re.IGNORECASE)
    user = os.path.basename(prof)
    if len(user) > 2:
        text = re.sub(re.escape(user), "<usuario>", text, flags=re.IGNORECASE)
    return EMAIL.sub("<email>", text)


def pinned(blob, want256):
    p = git(None, "--no-replace-objects", "-C", W, "cat-file", "blob", blob)
    if p.returncode != 0 or blob_id(p.stdout) != blob or sha256(p.stdout) != want256:
        raise SystemExit("pinned blob not readable or wrong: " + blob)
    return p.stdout


def interpreter_sha256():
    # same rule as bmin_accept.py (sys.executable, else the image with that name under sys.base_prefix)
    name = os.path.basename(sys.executable or "") or "python"
    for path in (sys.executable, os.path.join(sys.base_prefix, name)):
        try:
            return sha256(rbytes(path)), path
        except (OSError, TypeError):
            continue
    return None, None


def pw_sq(s):
    return "'" + s.replace("'", "''") + "'"


def contract_obj(allowed, forbidden):
    c = dict((k, []) for k in CONTRACT_REQUIRED)
    for k in ("Unit", "Gate", "TaskId", "Objective", "AuthorityRevision", "MainSha", "RoutingEnforcement", "IssuedBy",
              "IssuedUtc", "ProtocolSet", "MaterializationCloseSha", "SupersededBaseSha"):
        c[k] = ""
    c["Schema"] = "rackcad-gate-contract/v2"
    c["CorrectionsAuthorized"] = []
    c["AllowedWriteScope"] = list(allowed)
    c["ForbiddenWriteScope"] = list(forbidden)
    return c


def deleg_obj(allowed, forbidden):
    d = dict((k, []) for k in DELEG_FIELDS)
    d["Schema"] = "rackcad-delegation/v2"
    d["AllowedWriteScope"] = list(allowed)
    d["ForbiddenWriteScope"] = list(forbidden)
    return d


# The cases. K = contract lists, D = delegation lists. A3: every D.Allowed entry against K.Allowed (K entry contains);
# A4: every K.Forbidden entry against D.Forbidden (D entry contains). Prediction column = what each reading gives.
CASES = [
    ("MIN-A3", "D-1 minimo en A3: entrada contenedora del contrato 'a/**', contenida distinta bajo ella",
     (["a/**"], []), (["a/b"], [])),
    ("NEAR-A3-SAME", "vecino que coincide: la delegacion copia la cadena del contrato",
     (["a/**"], []), (["a/**"], [])),
    ("NEAR-A3-NORM", "vecino que coincide: el contrato ya escribe el prefijo normalizado 'a/'",
     (["a/"], []), (["a/b"], [])),
    ("MIN-A3-PREFIX", "D-1 con la contenida igual a la forma U-19 de la contenedora ('a/' bajo 'a/**')",
     (["a/**"], []), (["a/"], [])),
    ("MIN-A4", "D-1 minimo en A4: entrada contenedora de la delegacion 'a/**', prohibida del contrato 'a/b'",
     ([], ["a/b"]), ([], ["a/**"])),
    ("NEAR-A4-NORM", "vecino que coincide en A4: la delegacion escribe 'a/'",
     ([], ["a/b"]), ([], ["a/"])),
    ("NEAR-A3-NONE", "vecino que coincide sin contencion en ninguna pieza ('a/b' frente a 'c/**'): R7 compara hechos, no el resultado de A3",
     (["c/**"], []), (["a/b"], [])),
    ("MIN-A3-SETS", "D-1 con el mismo resultado de A3 en las dos lecturas: 'a/b' contenida por 'a/b' (las dos piezas) y por 'a/**' (solo el aceptante)",
     (["a/**", "a/b"], []), (["a/b"], [])),
]


def u19(e):
    return e[:-2] if e.endswith("/**") else e


def cov(entry, path):
    return path == entry or (entry.endswith("/") and path.startswith(entry))


def predicted(K, D):
    """Both readings computed independently here (not by either piece): instrument reading = no U-19; normative = U-19 on both."""
    (ka, kf), (da, df) = K, D
    out = {}
    for name, f in (("NoU19", lambda b, e: cov(b, e)), ("U19Both", lambda b, e: cov(u19(b), u19(e)))):
        out[name] = {"A3": [{"Entry": e, "ContainedBy": [b for b in ka if f(b, e)]} for e in da],
                     "A4": [{"Entry": e, "ContainedBy": [g for g in df if f(g, e)]} for e in kf]}
    return out


def main():
    if sys.flags.isolated != 1:
        raise SystemExit("run with python -I")
    tool = pinned(TOOL_BLOB, TOOL_SHA256)
    accb = pinned(ACC_BLOB, ACC_SHA256)
    acc_path = os.path.join(HERE, "pinned", "bmin_accept.py")
    wbytes(acc_path, accb)
    git_exe = os.path.normpath(shutil.which("git"))
    pwsh = shutil.which("pwsh")
    py_sha, _ = interpreter_sha256()
    env_rec = {"Python": {"Version": platform.python_version(), "ExecutableSha256": py_sha,
                          "EqualsDeclared": py_sha == DECL_PY_SHA256 and platform.python_version() == DECL_PY_VERSION},
               "Git": {"Sha256": sha256(rbytes(git_exe)), "EqualsDeclared": sha256(rbytes(git_exe)) == DECL_GIT_SHA256,
                       "Version": git(None, "--version").stdout.decode("ascii", "replace").strip()},
               "Pwsh": {"Version": subprocess.run([pwsh, "-NoLogo", "-NoProfile", "-NonInteractive", "-Command",
                                                   "$PSVersionTable.PSVersion.ToString()"], capture_output=True,
                                                  stdin=subprocess.DEVNULL, env=genv()).stdout.decode("ascii", "replace").strip()}}
    if not (env_rec["Python"]["EqualsDeclared"] and env_rec["Git"]["EqualsDeclared"]):
        raise SystemExit("interpreter or Git differ from the declared ones: " + json.dumps(env_rec))
    # repository: no commit, one loose object (the instrument blob)
    repo = HERE + "/repo"
    if os.path.isdir(repo):
        def onexc(f, q, e):
            os.chmod(q, 0o666)      # loose objects are read-only
            f(q)
        shutil.rmtree(repo, onexc=onexc)
    os.makedirs(repo)
    r = git(repo, "-c", "init.defaultBranch=main", "init", "-q")
    if r.returncode != 0:
        raise SystemExit("git init failed")
    w = git(repo, "hash-object", "-w", "--no-filters", "--stdin", inp=tool)
    oid = w.stdout.decode("ascii").strip()
    objs = []
    for root, _d, fs in os.walk(os.path.join(repo, ".git", "objects")):
        for f in fs:
            objs.append(os.path.relpath(os.path.join(root, f), os.path.join(repo, ".git", "objects")).replace(os.sep, "/"))
    refs = git(repo, "for-each-ref").stdout.decode("ascii", "replace").split()
    head = git(repo, "rev-parse", "-q", "--verify", "HEAD")
    # point of use, (iii): the bytes that the declared shell pipeline delivers recompute to the pinned blob
    p3 = subprocess.run([pwsh, "-NoLogo", "-NoProfile", "-NonInteractive", "-Command",
                         "& " + pw_sq(git_exe) + " --no-replace-objects cat-file blob " + TOOL_BLOB + " | & " + pw_sq(git_exe)
                         + " hash-object --no-filters --stdin"], cwd=repo, capture_output=True, stdin=subprocess.DEVNULL, env=genv())
    repo_rec = {"Path": "min/repo", "WriteReturnedId": oid, "IdEqualsPinned": oid == TOOL_BLOB, "Objects": sorted(objs),
                "Refs": refs, "HeadResolves": head.returncode == 0,
                "PipelineRecomputedBlob": p3.stdout.decode("ascii", "replace").strip(),
                "PipelineBlobEqualsPinned": p3.stdout.decode("ascii", "replace").strip() == TOOL_BLOB}
    if not (repo_rec["IdEqualsPinned"] and repo_rec["PipelineBlobEqualsPinned"] and repo_rec["Objects"] == [TOOL_BLOB[:2] + "/" + TOOL_BLOB[2:]]):
        raise SystemExit("repository state is not the minimal one: " + json.dumps(repo_rec))
    results = {"Schema": "rackcad-i62-d1-min/0-experimental", "Pinned": {"Tool": {"Blob": TOOL_BLOB, "Sha256": TOOL_SHA256},
                                                                          "Acceptor": {"Blob": ACC_BLOB, "Sha256": ACC_SHA256}},
               "Environment": env_rec, "Repo": repo_rec, "Cases": []}
    for name, what, K, D in CASES:
        cdir = HERE + "/cases/" + name
        kb, db = jbytes(contract_obj(*K)), jbytes(deleg_obj(*D))
        kp, dp = cdir + "/CONTRACT.json", cdir + "/DELEG.json"
        wbytes(kp, kb)
        wbytes(dp, db)
        argv = ["contract", "--repo", repo, "--file", "CONTRACT=" + kp, "--sha256", "CONTRACT=" + sha256(kb),
                "--file", "DELEG=" + dp, "--sha256", "DELEG=" + sha256(db)]
        for a in argv:
            if not SAFE.match(a):
                raise SystemExit("unsafe token: " + a)
        inner = ("& " + pw_sq(git_exe) + " --no-replace-objects cat-file blob " + TOOL_BLOB + " | & " + pw_sq(sys.executable)
                 + " -I - " + " ".join(argv))
        tp = subprocess.run([pwsh, "-NoLogo", "-NoProfile", "-NonInteractive", "-Command", inner], cwd=repo,
                            capture_output=True, stdin=subprocess.DEVNULL, env=genv())
        env_path = os.path.join(cdir, "envelope.json")
        wbytes(env_path, tp.stdout)
        closure = {"Schema": "rackcad-bmin-closure/0-experimental", "Commits": [], "RevPaths": [], "Refs": [], "Paths": [],
                   "Objects": [], "Files": [{"Name": "CONTRACT", "Sha256": sha256(kb)}, {"Name": "DELEG", "Sha256": sha256(db)}]}
        clp = os.path.join(cdir, "closure.json")
        wbytes(clp, jbytes(closure))
        req = {"Schema": "rackcad-bmin-accept-request/0-experimental", "Repo": repo,
               "Tool": {"PinnedBlob": TOOL_BLOB, "RunFrom": "object", "GitObject": {"GitDir": repo, "Blob": TOOL_BLOB}},
               "Interpreter": {"Python": platform.python_version(), "ExecutableSha256": py_sha},
               "Runs": [{"Name": "contract-1", "Envelope": env_path, "Argv": argv}],
               "Closure": {"Path": clp, "Sha256": sha256(rbytes(clp))},
               "NotApplicable": {"R4": "d1 minimal case: no verification", "R5": "d1 minimal case: no identity/diff/scope/hashes run",
                                 "R6": "d1 minimal case: no clausemap run", "R10": "d1 minimal case: no relay facts"}}
        rqp = os.path.join(cdir, "request.json")
        wbytes(rqp, jbytes(req))
        if blob_id(rbytes(acc_path)) != ACC_BLOB or sha256(rbytes(acc_path)) != ACC_SHA256:
            raise SystemExit("acceptor copy changed")
        before = git(repo, "count-objects", "-v").stdout
        ap = subprocess.run([sys.executable, "-I", acc_path, "--request", rqp], capture_output=True, stdin=subprocess.DEVNULL,
                            env=genv(), cwd=HERE)
        after = git(repo, "count-objects", "-v").stdout
        wbytes(os.path.join(cdir, "accept-output.json"), ap.stdout)
        out = json.loads(ap.stdout.decode("ascii"))
        env = json.loads(tp.stdout.decode("ascii"))
        checks = dict((c["Id"], c) for c in out.get("Checks", []))
        mism = []
        for run in ((checks.get("R7") or {}).get("Detail") or {}).get("Runs", []):
            mism.extend(run.get("Mismatches", []))
        st = dict((k, v.get("Status")) for k, v in sorted(checks.items()))
        facts_level_accept = (ap.returncode == 0 and out.get("Decision") == "ACCEPT_FACTS"
                              and all(st.get(x) == "MATCH" for x in ("R1", "R2", "R3", "R7", "R9")) and st.get("R8") == "MANUAL"
                              and all(st.get(x) == "NOT_APPLICABLE" for x in ("R4", "R5", "R6", "R10")))
        pred = predicted(K, D)
        facts = env.get("Facts") or {}
        results["Cases"].append({
            "Name": name, "What": what,
            "Contract": {"AllowedWriteScope": K[0], "ForbiddenWriteScope": K[1], "Sha256": sha256(kb), "Bytes": len(kb)},
            "Delegation": {"AllowedWriteScope": D[0], "ForbiddenWriteScope": D[1], "Sha256": sha256(db), "Bytes": len(db)},
            "ToolExit": tp.returncode, "ToolStderr": sanitize(tp.stderr.decode("utf-8", "replace"))[:400],
            "EnvelopeSha256": sha256(tp.stdout), "EnvelopeHasEmail": bool(EMAIL.search(tp.stdout.decode("ascii", "replace"))),
            "ToolGitCalls": env.get("GitCalls"), "ToolA3": facts.get("A3"), "ToolA4": facts.get("A4"),
            "PredictedNoU19": pred["NoU19"], "PredictedU19Both": pred["U19Both"],
            "ToolEqualsNoU19Prediction": {"A3": facts.get("A3") == pred["NoU19"]["A3"], "A4": facts.get("A4") == pred["NoU19"]["A4"]},
            "AcceptorExit": ap.returncode, "Decision": out.get("Decision"), "Statuses": st, "R7Mismatches": mism,
            "FactsLevelAccepted": facts_level_accept, "AcceptorStdoutSha256": sha256(ap.stdout),
            "RepoUnchangedByAcceptor": before == after, "Diverge": bool(mism)})
        print(name, tp.returncode, ap.returncode, out.get("Decision"), st.get("R7"), len(mism), flush=True)
    text = sanitize(json.dumps(results, ensure_ascii=True, sort_keys=True, indent=1) + "\n")
    wbytes(os.path.join(HERE, "min-result.json"), text.encode("ascii"))
    print("wrote min-result.json", sha256(text.encode("ascii")))


if __name__ == "__main__":
    main()
