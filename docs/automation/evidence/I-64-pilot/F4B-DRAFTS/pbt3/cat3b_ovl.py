import re
import subprocess
import sys

# Uso: python -I cat3b_ovl.py <checkout> <rev>
# Barrido independiente de lo que una sobrecarga o un miembro nuevo en PP, UR o la interfaz podria romper sin cambiar ninguna firma:
# (1) las llamadas a Prepare, Execute, ComparePost y Precheck con null, default o null! en algun argumento, o con argumentos sin tipo claro;
# (2) los grupos de metodos (el nombre sin parentesis detras); (3) la reflexion sobre los tipos de PP y UR y sobre la interfaz
# (typeof(...).GetX, nameof, GetType().GetX); (4) los constructores de PushBackPilotUpdatePlan y PushBackPilotPostExpectation, y su reflexion.
sys.stdout.reconfigure(encoding="utf-8", errors="replace")
REPO, REV = sys.argv[1], sys.argv[2]


def git(*a):
    return subprocess.run(["git", "-C", REPO, *a], capture_output=True, check=True).stdout.decode("utf-8", "replace")


TYPES = r"(?:PushBackPilotUpdatePlanner|PushBackPilotUpdatePlan|PushBackPilotPostExpectation|PushBackPilotPrepareResult|PushBackPilotPrepareInput|" \
        r"PushBackPilotUpdateRun|IPushBackPilotUpdatePort|IPushBackPilotMutationScope|IPushBackPilotUpdateScope|IPushBackPilotViewEraser|PushBackPilotUnitResult)"
METHODS = r"(?:Prepare|Execute|ComparePost|Precheck)"
files = [p for p in git("ls-tree", "-r", "--name-only", REV).split("\n") if p.endswith(".cs") and p.startswith(("src/", "tests/"))]
calls, groups, refl, ctors = [], [], [], []
for p in files:
    t = git("show", REV + ":" + p)
    if not re.search(TYPES + r"|\b" + METHODS + r"\b", t):
        continue
    lines = t.split("\n")
    for k, l in enumerate(lines, 1):
        s = l.strip()
        if s.startswith("//"):
            continue
        for m in re.finditer(r"\b(PushBackPilotUpdatePlanner|PushBackPilotUpdateRun)\s*\.\s*" + METHODS + r"\b(\s*\()?", l):
            name = m.group(0)
            if m.group(2):
                # los argumentos de esta llamada, hasta el parentesis que la cierra (en la linea o en las siguientes)
                rest = "\n".join(lines[k - 1:k + 6])[rest_start(l, m) if False else 0:]
                i = rest.find(name) + len(name)
                depth, j = 1, i
                while j < len(rest) and depth:
                    depth += {"(": 1, ")": -1}.get(rest[j], 0)
                    j += 1
                args = rest[i:j - 1]
                if re.search(r"\bnull\b|\bdefault\b", args):
                    calls.append((p, k, re.sub(r"\s+", " ", (name + args + ")"))[:200]))
            else:
                groups.append((p, k, s[:200]))
        if re.search(r"typeof\s*\(\s*" + TYPES + r"\s*\)|nameof\s*\(\s*" + TYPES, l) or (re.search(TYPES, l) and re.search(r"\.Get(?:Methods?|Members?|Properties|Property|Fields?|Constructors?|Interfaces)\s*\(", l)):
            refl.append((p, k, s[:220]))
        if re.search(r"new\s+(?:PushBackPilotUpdatePlan|PushBackPilotPostExpectation)\s*\(", l):
            ctors.append((p, k, s[:200]))


def rest_start(l, m):
    return 0


print("== (1) llamadas con null o default en un argumento:", len(calls))
for x in calls:
    print("   %s:%d  %s" % x)
print("== (2) grupos de metodos (el nombre sin parentesis):", len(groups))
for x in groups:
    print("   %s:%d  %s" % x)
print("== (3) reflexion o nameof sobre los tipos:", len(refl))
for x in refl:
    print("   %s:%d  %s" % x)
print("== (4) constructores:", len(ctors))
for x in ctors:
    print("   %s:%d  %s" % x)
