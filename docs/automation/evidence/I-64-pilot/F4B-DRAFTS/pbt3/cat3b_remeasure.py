import json
import re
import subprocess
import sys

# Uso: python -I cat3b_remeasure.py <carpeta pbt3> <checkout> <rev>
# Remedicion independiente de lo que PB-T3b da por medido en el cierre de PB-T3a: S-1 a S-6 (metodo, linea, Skip), el src de Pilot/ frente a 18e4b783,
# la cobertura minima de src/ frente al ForbiddenWriteScope del seco, Workspace/ (79 y los 76), y la guarda nueva de PB-T3a (CT-12) con PP y UR.
sys.stdout.reconfigure(encoding="utf-8", errors="replace")
HERE, REPO, REV = sys.argv[1:4]


def git(*a):
    return subprocess.run(["git", "-C", REPO, *a], capture_output=True, check=True).stdout.decode("utf-8", "replace")


W = "tests/RackCad.Tests/Workspace/"
S = [("S-1", W + "WorkspacePilotPrepareGuardTests.cs", "TheFileOfThePlanComposesExactlyOnceInAMemberOfItsOwnWithAParameterSourceAndTheNameOfTheRereadEnvelope"),
     ("S-2", W + "WorkspacePilotPrepareGuardTests.cs", "TheComposeGuardTripsOnAnotherCallAnUnsourcedCompositionANullableSourceOrAnotherName"),
     ("S-3", W + "WorkspacePilotPrepareGuardTests.cs", "TheMemberPlansAreBuiltOnlyFromTheMutableMembersOfTheRereadInOrdinalOrderAndNeverErase"),
     ("S-4", W + "WorkspacePilotUpdateGuardTests.cs", "TheRunReleasesBothScopesInFinallyBlocksOfItsOwnAndNeverWithAUsing"),
     ("S-5", W + "WorkspacePilotUpdateIntegrationGuardTests.cs", "M01TheEscapeOfTheRunIsClassifiedByTheStateThatTheRunAlreadyHadAndNeverAsAPlainFailureAfterAWriteOrASeal"),
     ("S-6", W + "WorkspacePilotUpdateIntegrationGuardTests.cs", "M01TheEscapeGuardTripsOnACatchThatDoesNotConsultTheStateAndOnAnEscapeThatAnswersAPlainFailureFirst")]
for sid, path, name in S:
    lines = git("show", REV + ":" + path).split("\n")
    hits = [k for k, l in enumerate(lines) if re.search(r"\bvoid\s+" + name + r"\s*\(", l)]
    for k in hits:
        attrs = []
        j = k - 1
        while j >= 0 and (lines[j].strip().startswith("[") or lines[j].strip() == ""):
            if lines[j].strip().startswith("["):
                attrs.append(lines[j].strip()[:80])
            j -= 1
        rows = [a for a in attrs if a.startswith("[InlineData")]
        print(sid, path.split("/")[-1], "linea", k + 1, "| atributos:", [a for a in attrs if not a.startswith("[InlineData")], "| filas:", len(rows),
              "| Skip:", any("Skip" in a for a in attrs))
    if not hits:
        print(sid, "NO ENCONTRADO", name)
    print("   ocurrencias del nombre en la clase:", sum(1 for l in lines if name in l))

print("diff 18e4b783..%s -- Pilot/:" % REV[:8], repr(git("diff", "--stat", "18e4b783", REV, "--", "src/RackCad.Application/Systems/PushBack/Pilot")))
print("diff 18e4b783..%s -- src (stat):" % REV[:8], git("diff", "--stat", "18e4b783", REV, "--", "src").strip().split("\n")[-1:])

files = [p for p in git("ls-tree", "-r", "--name-only", REV, "src/").split("\n") if p]
c = json.loads(open(HERE + "/contract-dry-pbt3b.json", encoding="utf-8").read())
allowed = c["AllowedWriteScope"]
fsrc = [p for p in c["ForbiddenWriteScope"] if p.startswith("src/")]


def covered(p, entries):
    return any(p == e or (e.endswith("/") and p.startswith(e)) for e in entries)


al_src = [p for p in allowed if p.startswith("src/")]
print("src: archivos", len(files), "| forbidden src", len(fsrc), "| allowed src", al_src)
print("   archivos de src sin cubrir y no permitidos:", [p for p in files if not covered(p, fsrc) and p not in al_src][:10])
print("   permitidos cubiertos por un prohibido:", [p for p in al_src if covered(p, fsrc)])
print("   entradas que no cubren nada existente:", [e for e in fsrc if not any(covered(p, [e]) for p in files)])


def cover(paths, keep):
    # el algoritmo de [PKC-C]: un directorio sin nada permitido dentro es un prefijo; si no, se baja; los archivos, uno a uno
    out = []

    def walk(prefix, items):
        dirs, here = {}, []
        for it in items:
            rest = it[len(prefix):]
            if "/" in rest:
                dirs.setdefault(rest.split("/")[0], []).append(it)
            else:
                here.append(it)
        for f in sorted(here):
            if f not in keep:
                out.append(f)
        for d in sorted(dirs):
            sub = prefix + d + "/"
            if any(k.startswith(sub) for k in keep):
                walk(sub, dirs[d])
            else:
                out.append(sub)
    walk("src/", paths)
    return out


cv = cover(files, set(al_src))
print("cobertura calculada:", len(cv), "| igual a la del seco (como conjunto):", set(cv) == set(fsrc))
print("   en la calculada y no en el seco:", sorted(set(cv) - set(fsrc))[:10])
print("   en el seco y no en la calculada:", sorted(set(fsrc) - set(cv))[:10])

ws = [p for p in git("ls-tree", "-r", "--name-only", REV, W).split("\n") if p]
fws = [p for p in c["ForbiddenWriteScope"] if p.startswith(W)]
print("Workspace/:", len(ws), "| en el Forbidden:", len(fws), "| fuera:", [p.split("/")[-1] for p in ws if p not in fws])

g = git("show", REV + ":tests/RackCad.Tests/PushBackDraftWriteGuardTests.cs")
for k, l in enumerate(g.split("\n")):
    if re.search(r"PushBackPilotUpdatePlan\.cs|PushBackPilotUpdateRun\.cs|Systems/PushBack/Draft/|EnumerateFiles|GetFiles|Src\.|\"src", l):
        print("   CT-12 :%d %s" % (k + 1, l.strip()[:170]))
