"""I-62 A-6, preflight (decisiones §76, punto 5): ejecuta en orden la autoprueba, el modo preview sobre la HEAD, los controles negativos, el commit
candidato hipotético y el modo preview sobre ese commit, y comprueba que el worktree de I-62 no cambia.

Solo lectura del worktree de I-62 (la ruta se deriva de USERPROFILE y nunca se escribe en las salidas). Temporales solo bajo D:/r62-a6-tmp, que se
borra al terminar; nunca se toca D:/r62-fixture. Ninguna invocación de modelo, de CLI de modelos ni de red.
Uso: python -I -B run_preflight_a6.py
"""
import hashlib
import json
import os
import shutil
import subprocess
import sys

HERE = os.path.dirname(os.path.abspath(__file__))
WT = os.path.join(os.environ["USERPROFILE"], ".codex", "worktrees", "architecture-portabilidad-coordinador-principal")
A6_LOCAL = os.path.normpath(os.path.join(HERE, "..", "..", "..", "initiatives", "I-62-A-6.md"))
TMP = "D:/r62-a6-tmp"
PY = [sys.executable, "-I", "-B"]


def rmtree_ro(path):
    def _writable(func, p, exc):
        os.chmod(p, 0o600)
        func(p)
    if os.path.exists(path):
        shutil.rmtree(path, onexc=_writable)


def wt_digest():
    def h(*args):
        r = subprocess.run(["git", "-C", WT] + list(args), capture_output=True)
        return hashlib.sha256(r.stdout).hexdigest()
    head = subprocess.run(["git", "-C", WT, "rev-parse", "HEAD"], capture_output=True, encoding="utf-8").stdout.strip()
    return {"Head": head, "ForEachRef": h("for-each-ref"), "StatusPorcelainIgnored": h("status", "--porcelain", "--ignored"),
            "IndexSha256": hashlib.sha256(open(subprocess.run(["git", "-C", WT, "rev-parse", "--path-format=absolute", "--git-path", "index"],
                                                              capture_output=True, encoding="utf-8").stdout.strip(), "rb").read()).hexdigest()}


def step(name, cmd):
    r = subprocess.run(cmd, capture_output=True)
    line = r.stdout.decode("utf-8", "replace").strip().splitlines()[-1:] or [""]
    print("%-28s rc=%d %s" % (name, r.returncode, line[0][:300]))
    return r.returncode


def main():
    try:
        sys.stdout.reconfigure(encoding="utf-8")
    except Exception:
        pass
    assert "r62-fixture" not in TMP
    os.environ["GIT_OPTIONAL_LOCKS"] = "0"   # heredado por las guardas y los guiones: ninguna lectura refresca el índice del worktree
    rmtree_ro(TMP)
    os.makedirs(TMP + "/gt")
    os.makedirs(os.path.join(HERE, "out"), exist_ok=True)
    before = wt_digest()
    guards = os.path.join(HERE, "a6-guards.py")
    rc = {}
    rc["self-test"] = step("self-test", PY + [guards, "self-test", "--repo", WT, "--a6", A6_LOCAL, "--out", os.path.join(HERE, "a6-selftest.json"),
                                              "--tmp", TMP + "/gt"])
    rc["preview-head"] = step("preview (HEAD)", PY + [guards, "preview", "--repo", WT, "--a6", A6_LOCAL, "--pkgdir", HERE,
                                                      "--out", os.path.join(HERE, "out", "a6-guards-result-preview-head.json"), "--tmp", TMP + "/gt"])
    rc["negatives"] = step("negatives", PY + [os.path.join(HERE, "neg_controls_a6.py"), "--out", os.path.join(HERE, "out", "neg_controls_a6.json")])
    rc["candidate"] = step("candidate commit", PY + [os.path.join(HERE, "make_candidate_commit_a6.py"), "--dest", TMP + "/rc",
                                                     "--out", os.path.join(HERE, "out", "candidate-commit.json")])
    cand = json.load(open(os.path.join(HERE, "out", "candidate-commit.json"), encoding="utf-8"))["Candidate"]
    rc["preview-candidate"] = step("preview (candidato)", PY + [guards, "preview", "--repo", WT, "--a6", A6_LOCAL, "--pkgdir", HERE,
                                                                 "--cand-repo", TMP + "/rc", "--cand-head", cand,
                                                                 "--out", os.path.join(HERE, "a6-guards-result-preview.json"), "--tmp", TMP + "/gt"])
    rmtree_ro(TMP)
    after = wt_digest()
    dig = {"Before": before, "After": after, "Equal": before == after, "TmpRemoved": not os.path.exists(TMP),
           "Note": "digest del worktree de I-62 (HEAD, refs, estado con ignorados e índice) antes y después del preflight"}
    with open(os.path.join(HERE, "out", "worktree-digest.json"), "w", encoding="utf-8", newline="\n") as fh:
        fh.write(json.dumps(dig, ensure_ascii=False, indent=1) + "\n")
    print("worktree sin cambios: %s; temporales borrados: %s" % (dig["Equal"], dig["TmpRemoved"]))
    ok = all(v == 0 for v in rc.values()) and dig["Equal"] and dig["TmpRemoved"]
    sys.exit(0 if ok else 1)


if __name__ == "__main__":
    main()
