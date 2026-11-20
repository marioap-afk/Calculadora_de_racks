import re
import subprocess
import sys

# Uso: python -I cat3a_reflect.py <checkout> <rev>
# Las guardas por reflexion: cada GetTypes()/GetExportedTypes() de tests/, con las lineas que filtran los tipos (Where, Name, Namespace).
sys.stdout.reconfigure(encoding="utf-8", errors="replace")
REPO, REV = sys.argv[1], sys.argv[2]


def git(*a):
    return subprocess.run(["git", "-C", REPO, *a], capture_output=True, check=True).stdout.decode("utf-8", "replace")


for p in [x for x in git("ls-tree", "-r", "--name-only", REV, "tests/").splitlines() if x.endswith(".cs")]:
    lines = git("show", REV + ":" + p).split("\n")
    for k, l in enumerate(lines):
        if re.search(r"\.Get(?:Exported)?Types\(\)", l):
            ctx = lines[max(0, k - 3):k + 9]
            print("=== %s:%d" % (p, k + 1))
            for c in ctx:
                print("   ", c.rstrip()[:190])
