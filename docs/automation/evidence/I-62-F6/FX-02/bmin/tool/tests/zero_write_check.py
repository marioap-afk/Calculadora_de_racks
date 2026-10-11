#!/usr/bin/env python3
"""Standalone zero-write check for one bmin_facts.py command on a clone.

Takes a snapshot of the clone (sha256 of .git/index, `git for-each-ref`,
`git status --porcelain --ignored` and the full listing of files with size and
mtime, .git included), runs `python -I bmin_facts.py <command> ...`, takes the
snapshot again and compares. The snapshot's own git calls use
GIT_OPTIONAL_LOCKS=0 so that they do not refresh the index.

Usage:
  python zero_write_check.py --repo <clone> [--tool <bmin_facts.py>] -- <command> <args...>

Prints one JSON object: {"Equal", "Rc", "ErrorCode", "FactsSha256", "FactKeys", "Differences"}.
Exit status 0 only when the snapshots are equal.
"""

import hashlib
import json
import os
import subprocess
import sys

HERE = os.path.dirname(os.path.abspath(__file__))


def git_env():
    env = dict((k, v) for k, v in os.environ.items() if not k.upper().startswith("GIT_"))
    env.update({"GIT_OPTIONAL_LOCKS": "0", "GIT_TERMINAL_PROMPT": "0", "LC_ALL": "C"})
    return env


def snapshot(repo):
    index = os.path.join(repo, ".git", "index")
    idx = None
    if os.path.exists(index):
        with open(index, "rb") as fh:
            idx = hashlib.sha256(fh.read()).hexdigest()
    refs = subprocess.run(["git", "for-each-ref"], cwd=repo, env=git_env(), stdout=subprocess.PIPE).stdout
    status = subprocess.run(["git", "status", "--porcelain", "--ignored"], cwd=repo, env=git_env(),
                            stdout=subprocess.PIPE).stdout
    files = []
    for dirpath, dirnames, filenames in os.walk(repo):
        dirnames.sort()
        for fn in sorted(filenames):
            full = os.path.join(dirpath, fn)
            info = os.stat(full)
            files.append([os.path.relpath(full, repo).replace("\\", "/"), info.st_size, info.st_mtime_ns])
    return {"index": idx, "refs": refs.decode("utf-8", "replace"), "status": status.decode("utf-8", "replace"),
            "files": files}


def main(argv):
    if "--" not in argv:
        raise SystemExit(__doc__)
    head, cmd = argv[:argv.index("--")], argv[argv.index("--") + 1:]
    opts = dict(zip(head[0::2], head[1::2]))
    repo = opts["--repo"]
    tool = opts.get("--tool") or os.path.join(HERE, "..", "bmin_facts.py")
    before = snapshot(repo)
    p = subprocess.run([sys.executable, "-I", tool] + cmd, stdout=subprocess.PIPE, stderr=subprocess.PIPE)
    after = snapshot(repo)
    diffs = {}
    for key in ("index", "refs", "status"):
        if before[key] != after[key]:
            diffs[key] = {"before": before[key], "after": after[key]}
    if before["files"] != after["files"]:
        b = set(map(tuple, before["files"]))
        a = set(map(tuple, after["files"]))
        diffs["files"] = {"only_before": sorted(b - a)[:20], "only_after": sorted(a - b)[:20]}
    out = {"Equal": not diffs, "Rc": p.returncode, "ErrorCode": None, "FactsSha256": None, "FactKeys": None,
           "StdoutAscii": all(x < 0x80 for x in p.stdout), "Differences": diffs}
    try:
        env = json.loads(p.stdout.decode("ascii"))
        out["ErrorCode"] = (env.get("Error") or {}).get("Code")
        out["FactsSha256"] = env.get("FactsSha256")
        if isinstance(env.get("Facts"), dict):
            out["FactKeys"] = sorted(env["Facts"])
    except ValueError:
        out["ErrorCode"] = "<stdout is not a JSON envelope>"
    print(json.dumps(out, sort_keys=True))
    return 0 if not diffs else 1


if __name__ == "__main__":
    sys.exit(main(sys.argv[1:]))
