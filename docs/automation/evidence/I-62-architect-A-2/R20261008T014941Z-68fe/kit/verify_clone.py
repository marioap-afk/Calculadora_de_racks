"""Verificación mecánica del clon limpio y del run de la revisión de A-2 (kit adaptado del kit v4 de A-1), antes del lanzamiento.

Comprueba: HEAD = el commit de publicación; una sola rama (main); sin remotos; árbol limpio (también sin ignorados); los blobs designados; sin enlaces
simbólicos ni uniones (reparse points) en el árbol de trabajo ni entradas 120000 en el índice; core.autocrlf; commits presentes y ausentes; el run
(order.txt con su SHA-256 y prompt.md idéntico al del kit); y una preflight de EX-1 (`a2-guards.py self-test`) con la salida en un directorio
temporal junto al kit, que se borra. Escribe clone-verification.json junto a este script.
Uso: python verify_clone.py
"""
import hashlib
import json
import os
import shutil
import stat
import subprocess
import sys

KIT = os.path.dirname(os.path.abspath(__file__))
CLONE, RUN = r"D:\r62-arch-a2", r"D:\r62-arch-a2-run"
REV = "4a059afa85dce5a82f74120cad88c5ebadb61c7a"
BLOBS = {
    "docs/initiatives/I-62-A-2.md": "d47f71b66ba86c31f857f0d8c2d2437636947af0",
    "docs/initiatives/I-62-architect-package-A-2.md": "f45a2384eb1a3c9ec5654995c5b88f6c51b6742f",
    "docs/automation/evidence/I-62-A2/a2-guards.py": "91c9c2a46a4a9327ecfff44fa52a4d48e0e33935",
    "docs/automation/evidence/I-62-A2/a2-guards-result.json": "33ffbb9deb114972cfd11ade3ab448a2af1f50dd",
    "docs/initiatives/I-62-proposal-v14.md": "34ad80ea1bfff144bfc5169f62920a4c904c1bfa",
}
PRESENT = {"b280f7079e128113666e85a2f437e629d0f0d99e": "commit de A-2", "c9f9419dc4aed32242c59de489a8e78a8e369f9b": "base de A-2"}
ABSENT = {"4c617e82b32b6c810b68d75fc19472efed22b393": "commit de V14 (anterior al rebase)", "b64a3b640c7ee3bd77636e7a3218ae0ebac2dd43": "FREEZE_SHA (anterior al rebase)"}
ORDER_SHA = "bfa265cd5b3bee47059dd8178153b19916826ad94d936715fd22893a3d6d5259"


def git(*a, check=True):
    r = subprocess.run(["git", "-C", CLONE] + list(a), capture_output=True, text=True)
    if check and r.returncode:
        raise SystemExit("git %s: %s" % (a, r.stderr))
    return r


def sha(p):
    return hashlib.sha256(open(p, "rb").read()).hexdigest()


reparse, files = [], 0
for root, dirs, fs in os.walk(CLONE, followlinks=False):
    for n in dirs + fs:
        p = os.path.join(root, n)
        st = os.lstat(p)
        if stat.S_ISLNK(st.st_mode) or (getattr(st, "st_file_attributes", 0) & 0x400):
            reparse.append(p)
    files += len(fs)
blobs = {p: git("rev-parse", REV + ":" + p).stdout.strip() for p in BLOBS}
doc = {
    "Clone": CLONE,
    "Commands": [
        'git clone --no-local -c core.autocrlf=false --single-branch --branch architecture/portabilidad-coordinador-principal "D:\\Documentos\\Codex\\Calculadora de racks" D:\\r62-arch-a2',
        "git -C D:\\r62-arch-a2 checkout -q -B main " + REV,
        "git -C D:\\r62-arch-a2 branch -D architecture/portabilidad-coordinador-principal",
        "git -C D:\\r62-arch-a2 remote remove origin",
    ],
    "Head": git("rev-parse", "HEAD").stdout.strip(),
    "Branches": git("branch", "--list", "--format=%(refname:short)").stdout.split(),
    "Remotes": git("remote").stdout.split(),
    "StatusPorcelain": git("status", "--porcelain").stdout,
    "StatusIgnored": git("status", "--porcelain", "--ignored").stdout,
    "CoreAutocrlf": git("config", "--get", "core.autocrlf", check=False).stdout.strip(),
    "WorktreeList": git("worktree", "list").stdout.strip(),
    "Tags": len(git("tag", "--list").stdout.split()),
    "Blobs": {p: {"Expected": BLOBS[p], "Got": blobs[p], "Match": blobs[p] == BLOBS[p]} for p in BLOBS},
    "CommitsPresent": {c: {"What": w, "Present": git("cat-file", "-e", c + "^{commit}", check=False).returncode == 0} for c, w in PRESENT.items()},
    "CommitsAbsent": {c: {"What": w, "Present": git("cat-file", "-e", c + "^{commit}", check=False).returncode == 0} for c, w in ABSENT.items()},
    "IndexSymlinks": sum(1 for l in git("ls-files", "-s").stdout.splitlines() if l.startswith("120000 ")),
    "ReparsePointsInWorkTree": reparse, "WorkTreeFiles": files,
    "Run": {"Dir": RUN, "Files": sorted(os.listdir(RUN)),
            "order.txt": {"Bytes": os.path.getsize(os.path.join(RUN, "order.txt")), "Sha256": sha(os.path.join(RUN, "order.txt")),
                          "MatchesAuthority": sha(os.path.join(RUN, "order.txt")) == ORDER_SHA,
                          "IdenticalToKitCopy": sha(os.path.join(RUN, "order.txt")) == sha(os.path.join(KIT, "order.txt"))},
            "prompt.md": {"Bytes": os.path.getsize(os.path.join(RUN, "prompt.md")), "Sha256": sha(os.path.join(RUN, "prompt.md")),
                          "IdenticalToKitCopy": sha(os.path.join(RUN, "prompt.md")) == sha(os.path.join(KIT, "prompt.md"))}},
}
tmp = os.path.join(os.path.dirname(KIT), ".ex1")
os.makedirs(tmp, exist_ok=True)
out = os.path.join(tmp, "a2-selftest.json")
env = dict(os.environ, TEMP=tmp, TMP=tmp, TMPDIR=tmp)
r = subprocess.run([sys.executable, "-I", "-B", "docs/automation/evidence/I-62-A2/a2-guards.py", "self-test", "--out", out], cwd=CLONE,
                   capture_output=True, text=True, env=env)
g = json.load(open(out, encoding="utf-8"))["G5"]
doc["Ex1Preflight"] = {"Command": "python docs/automation/evidence/I-62-A2/a2-guards.py self-test --out <temporal junto al kit> (cwd = el clon)",
                       "ExitCode": r.returncode, "Stdout": r.stdout.strip(), "Vectors": g["Total"], "VectorsPassed": g["Passed"],
                       "Mutants": len(g["Mutants"]), "MutantsKilled": sum(1 for m in g["Mutants"] if m["Result"] == "PASS"),
                       "RulesMissing": g["RulesMissing"], "OutputSha256": sha(out)}
shutil.rmtree(tmp)
doc["StatusAfterEx1"] = git("status", "--porcelain", "--ignored").stdout
doc["AllChecks"] = (doc["Head"] == REV and doc["Branches"] == ["main"] and doc["Remotes"] == [] and doc["StatusPorcelain"] == "" and
                    doc["StatusIgnored"] == "" and all(v["Match"] for v in doc["Blobs"].values()) and
                    all(v["Present"] for v in doc["CommitsPresent"].values()) and not any(v["Present"] for v in doc["CommitsAbsent"].values()) and
                    doc["IndexSymlinks"] == 0 and not reparse and doc["CoreAutocrlf"] == "false" and doc["Run"]["Files"] == ["order.txt", "prompt.md"] and
                    doc["Run"]["order.txt"]["MatchesAuthority"] and doc["Run"]["order.txt"]["IdenticalToKitCopy"] and
                    doc["Run"]["prompt.md"]["IdenticalToKitCopy"] and doc["Ex1Preflight"]["ExitCode"] == 0 and doc["StatusAfterEx1"] == "")
with open(os.path.join(KIT, "clone-verification.json"), "w", encoding="utf-8", newline="\n") as f:
    json.dump(doc, f, ensure_ascii=False, indent=1)
    f.write("\n")
print(json.dumps({"AllChecks": doc["AllChecks"], "Head": doc["Head"], "Files": files, "Reparse": len(reparse), "Ex1": doc["Ex1Preflight"]["Stdout"]}))
sys.exit(0 if doc["AllChecks"] else 1)
