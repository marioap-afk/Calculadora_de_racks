# probe_a3a4.py - targeted probe for ONE divergence claim (I-62, A-6, A6-5 rule 6 readings).
#
# Claim to settle: in the `contract` command (A3/A4 of AP 16.5), the pinned instrument (blob 9a829b0e) does not apply
# U-19 to the containing entry, while the pinned acceptor (blob 434ef1fc) applies U-19 to both entries. When a
# containing entry ends in "/**" and the contained entry is a different string under it, the acceptor's R7 should then
# report MISMATCH on the instrument's own facts (false REJECT_FACTS), while lists that mirror the contract coincide.
#
# Read-only on every input: the pinned bytes are taken by blob with `git cat-file blob` from the architecture worktree;
# the custodied T1 contract (blob 628d89af) with `git show` from the fixture copy. Writes only under this folder.
# Standard library only. Run: python -I probe_a3a4.py
import hashlib
import json
import os
import platform
import subprocess
import sys

HERE = os.path.dirname(os.path.abspath(__file__))
W = os.path.join(os.environ["USERPROFILE"], ".codex", "worktrees", "architecture-portabilidad-coordinador-principal")
FIX = "D:/r62-a6r2/r3/deliv"
TOOL_BLOB = "9a829b0eeb1210238052b1feac06cfa8f7964aed"
TOOL_SHA256 = "b03c3a19cef608386afd7c2e0618b884bfd7b7f6913e95d8ed70272d517a0200"
ACC_BLOB = "434ef1fccb0d15762b30244b4a60928120698411"
ACC_SHA256 = "173514aa353f6add1b11ea73ef6da68bba752bacdca64baa0551a6a7be6d8bb0"
T1_REV, T1_PATH, T1_BLOB = "95bdc29d", "docs/automation/decisions/FX-U1-T1.gate-contract.json", \
    "628d89af5f21e4e14cbe7c3c14f0dd8fb5a490e0"


def git(*args):
    env = dict((k, v) for k, v in os.environ.items() if not k.upper().startswith("GIT_"))
    env["GIT_OPTIONAL_LOCKS"] = "0"
    env["GIT_NO_REPLACE_OBJECTS"] = "1"
    p = subprocess.run(["git"] + list(args), stdout=subprocess.PIPE, stderr=subprocess.PIPE, env=env, shell=False)
    if p.returncode != 0:
        raise SystemExit("git failed: " + " ".join(args[:3]))
    return p.stdout


def blob_id(data):
    return hashlib.sha1(b"blob " + str(len(data)).encode("ascii") + b"\0" + data).hexdigest()


def sha256(data):
    return hashlib.sha256(data).hexdigest()


def write(path, data):
    os.makedirs(os.path.dirname(path), exist_ok=True)
    with open(path, "wb") as fh:
        fh.write(data)


def jbytes(obj):
    return (json.dumps(obj, ensure_ascii=True, sort_keys=True, indent=1) + "\n").encode("ascii")


def interpreter_image_sha256():
    # Same rule as bmin_accept.py:1920-1933 (sys.executable, else the image with that name under sys.base_prefix).
    name = os.path.basename(sys.executable or "") or "python"
    for path in (sys.executable, os.path.join(sys.base_prefix, name)):
        try:
            with open(path, "rb") as fh:
                return sha256(fh.read())
        except (OSError, TypeError):
            continue
    return None


def sanitize(text):
    prof = os.environ.get("USERPROFILE", "")
    user = os.environ.get("USERNAME", "")
    for form in (prof, prof.replace("\\", "/"), prof.replace("\\", "\\\\")):
        if len(form) > 3:
            text = text.replace(form, "%USERPROFILE%")
    if len(user) > 2:
        text = text.replace(user, "<usuario>")
    return text


def main():
    if sys.flags.isolated != 1:
        raise SystemExit("run with python -I")
    pinned = os.path.join(HERE, "pinned")
    tool_bytes = git("-C", W, "--no-replace-objects", "cat-file", "blob", TOOL_BLOB)
    acc_bytes = git("-C", W, "--no-replace-objects", "cat-file", "blob", ACC_BLOB)
    assert blob_id(tool_bytes) == TOOL_BLOB and sha256(tool_bytes) == TOOL_SHA256
    assert blob_id(acc_bytes) == ACC_BLOB and sha256(acc_bytes) == ACC_SHA256
    tool_path = os.path.join(pinned, "bmin_facts.py")
    acc_path = os.path.join(pinned, "bmin_accept.py")
    write(tool_path, tool_bytes)
    write(acc_path, acc_bytes)
    t1_bytes = git("-C", FIX, "--no-replace-objects", "show", T1_REV + ":" + T1_PATH)
    assert blob_id(t1_bytes) == T1_BLOB
    t1 = json.loads(t1_bytes.decode("utf-8"))
    repo = os.path.join(HERE, "repo")
    os.makedirs(repo, exist_ok=True)

    copied = ("AllowedWriteScope", "ForbiddenWriteScope", "Invariants", "StopConditions", "Authorities",
              "RequiredTests", "RoleRequirements", "SupersededCommits")

    def deleg_from(contract, allowed=None, forbidden=None):
        d = {"Schema": "rackcad-delegation/v2"}
        for k in copied:
            d[k] = contract[k]
        if allowed is not None:
            d["AllowedWriteScope"] = allowed
        if forbidden is not None:
            d["ForbiddenWriteScope"] = forbidden
        return d

    synthetic_contract = dict(t1)
    synthetic_contract["AllowedWriteScope"] = ["src/**", "docs/notes/"]
    synthetic_contract["ForbiddenWriteScope"] = ["build/", "secrets/key.txt"]
    cases = [
        ("P1-mirrored-T1", "T1 contract (blob 628d89af) and a delegation that copies its scope lists literally",
         t1, deleg_from(t1)),
        ("P2-nc2-like-T1", "T1 contract; delegation where the evidence prefix entry is replaced by the enumeration of "
         "one file under it (the substitution that RAE nc2 prescribes when a prefix covers the excluded file)",
         t1, deleg_from(t1, allowed=["docs/automation/evidence/FX-U1-agent/note-a.md",
                                     "src/Fixture.Lib/Calculator.cs", "tests/Fixture.Tests/CalculatorTests.cs"])),
        ("P3-synthetic-nested", "synthetic scope lists: '/**' containing entries over a file, a nested '/**' and a "
         "plain prefix", synthetic_contract,
         deleg_from(synthetic_contract, allowed=["src/a.cs", "src/lib/**", "docs/notes/"],
                    forbidden=["build/**", "secrets/**"])),
    ]
    exe_sha = interpreter_image_sha256()
    results = {"Schema": "rackcad-i62-a6m7-probe/0-experimental",
               "Claim": "instrument contract A3/A4 without U-19 versus acceptor with U-19 (R7)",
               "Pinned": {"Tool": {"Blob": TOOL_BLOB, "Sha256": TOOL_SHA256},
                          "Acceptor": {"Blob": ACC_BLOB, "Sha256": ACC_SHA256},
                          "T1Contract": {"Rev": T1_REV, "Path": T1_PATH, "Blob": T1_BLOB}},
               "Python": platform.python_version(), "Cases": []}
    for name, what, contract, deleg in cases:
        cdir = os.path.join(HERE, "cases", name)
        cpath, dpath = os.path.join(cdir, "CONTRACT.json"), os.path.join(cdir, "DELEG.json")
        cb, db = jbytes(contract), jbytes(deleg)
        write(cpath, cb)
        write(dpath, db)
        argv = ["contract", "--repo", repo, "--file", "CONTRACT=" + cpath, "--sha256", "CONTRACT=" + sha256(cb),
                "--file", "DELEG=" + dpath, "--sha256", "DELEG=" + sha256(db)]
        # the instrument from its pinned bytes through stdin, as the declared order does (cat-file blob | python -I -)
        tp = subprocess.run([sys.executable, "-I", "-"] + argv, input=tool_bytes, stdout=subprocess.PIPE,
                            stderr=subprocess.PIPE, shell=False, cwd=repo)
        env_path = os.path.join(cdir, "envelope.json")
        write(env_path, tp.stdout)
        closure = {"Schema": "rackcad-bmin-closure/0-experimental", "Commits": [], "RevPaths": [], "Refs": [],
                   "Paths": [], "Objects": [], "Files": [{"Name": "CONTRACT", "Sha256": sha256(cb)},
                                                        {"Name": "DELEG", "Sha256": sha256(db)}]}
        cl_path = os.path.join(cdir, "closure.json")
        write(cl_path, jbytes(closure))
        req = {"Schema": "rackcad-bmin-accept-request/0-experimental", "Repo": repo,
               "Tool": {"Path": tool_path, "PinnedBlob": TOOL_BLOB, "RunFrom": "path"},
               "Interpreter": {"Python": platform.python_version(), "ExecutableSha256": exe_sha},
               "Runs": [{"Name": "contract-1", "Envelope": env_path, "Argv": argv}],
               "Closure": {"Path": cl_path},
               "NotApplicable": {"R4": "probe: no verification", "R5": "probe: no identity/diff/scope/hashes run",
                                 "R6": "probe: no clausemap run", "R10": "probe: no relay facts"}}
        rq_path = os.path.join(cdir, "request.json")
        write(rq_path, jbytes(req))
        ap = subprocess.run([sys.executable, "-I", acc_path, "--request", rq_path], stdout=subprocess.PIPE,
                            stderr=subprocess.PIPE, shell=False)
        write(os.path.join(cdir, "accept-output.json"), ap.stdout)
        out = json.loads(ap.stdout.decode("ascii"))
        env = json.loads(tp.stdout.decode("ascii"))
        checks = dict((c["Id"], c) for c in out.get("Checks", []))
        r7 = checks.get("R7", {})
        mism = []
        for run in (r7.get("Detail") or {}).get("Runs", []):
            mism.extend(run.get("Mismatches", []))
        results["Cases"].append({
            "Name": name, "What": what, "ToolExit": tp.returncode,
            "ToolA3": (env.get("Facts") or {}).get("A3"), "ToolA4": (env.get("Facts") or {}).get("A4"),
            "AcceptorExit": ap.returncode, "Decision": out.get("Decision"),
            "Statuses": dict((k, v.get("Status")) for k, v in sorted(checks.items())),
            "R7Mismatches": mism})
    text = sanitize(json.dumps(results, ensure_ascii=True, sort_keys=True, indent=1) + "\n")
    write(os.path.join(HERE, "probe-result.json"), text.encode("ascii"))
    for c in results["Cases"]:
        print(c["Name"], c["ToolExit"], c["AcceptorExit"], c["Decision"], c["Statuses"].get("R7"), len(c["R7Mismatches"]))


if __name__ == "__main__":
    main()
