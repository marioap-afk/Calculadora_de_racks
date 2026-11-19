# -*- coding: utf-8 -*-
# I-64 F4B PB-T2, v2: mediciones nuevas de solo lectura por objetos git (apartado Q del registro).
# Uso: python -I measure_pbt2_v2.py <ruta-del-checkout-principal>
# Lee SOLO con `git -C <checkout> show|ls-tree|diff|rev-parse|log`. No escribe nada: imprime en stdout.
import re
import subprocess
import sys

REPO = sys.argv[1]
HEAD = "95373af6"
BASE = "2f5d3dec"
GREEN = "a18ba345"


def git(*args):
    out = subprocess.run(["git", "-C", REPO] + list(args), capture_output=True)
    return out.stdout.decode("utf-8", errors="replace")


def show(sha, path):
    return git("show", sha + ":" + path).replace("\r\n", "\n")


def lines_of(text, pattern):
    rx = re.compile(pattern)
    return [i + 1 for i, line in enumerate(text.split("\n")) if rx.search(line)]


def section(title):
    print()
    print("== " + title)


section("Q-1. La punta de la rama")
print("rev-parse", HEAD, "=", git("rev-parse", HEAD).strip())
print("log:", git("log", "--oneline", "-4", HEAD).strip().replace("\n", " | "))
d = git("diff", "--name-only", BASE, HEAD, "--", "src").strip()
print("diff --name-only", BASE, HEAD, "-- src:", "vacio" if not d else d)
d = git("diff", "--name-only", BASE, GREEN, "--", "src").strip()
print("diff --name-only", BASE, GREEN, "-- src:", "vacio" if not d else d)
print("tests cambiados", BASE, "..", HEAD, ":")
print(git("diff", "--name-only", BASE, HEAD, "--", "tests").rstrip())
pkg = show(HEAD, "docs/initiatives/I-64-f4b-pbt1-gate-contract-package.md")
m = re.search(r"\), (v2\.\d+)\s*$", pkg.split("\n")[2])
print("paquete de PB-T1 en HEAD, cabecera:", pkg.split("\n")[2][:140])

section("Q-2. Las decisiones custodiadas en HEAD")
for p in ["docs/automation/evidence/I-64-pilot/F4B-DRAFTS/coordinator/owner-f4b-correction-unit-change-decision.md",
          "docs/automation/evidence/I-64-pilot/F4B-DRAFTS/coordinator/owner-f4b-dpb07-extensiondata-decisions.md",
          "docs/automation/evidence/I-64-pilot/F4B-PBT1B-LOSSLESS-STATE-TYPE/task-close-package.md"]:
    ok = subprocess.run(["git", "-C", REPO, "cat-file", "-e", HEAD + ":" + p], capture_output=True).returncode == 0
    print(p, "->", "existe" if ok else "NO EXISTE")
disp = show(HEAD, "docs/automation/evidence/I-64-pilot/F4B-DRAFTS/f4b-dispositions.md")
for e in ["F4B-E23", "F4B-E24", "F4B-E25", "F4B-E26"]:
    print(e, "->", lines_of(disp, r"^## " + e + r"\."))

section("Q-3. G-L1 corregida (CT-PBT1B-14) en HEAD")
g = show(HEAD, "tests/RackCad.Tests/PushBackSharedReferenceGuardTests.cs")
print("Tokens ->", lines_of(g, r"private static readonly string\[\] Tokens"))
print("Rebind ->", lines_of(g, r"private static readonly HashSet<string> Rebind"))
print("Members ->", lines_of(g, r"private static List<string> Members\("))
print("Aliases ->", lines_of(g, r"private static Dictionary<string, string> Aliases\("))
print("Violations ->", lines_of(g, r"private static List<string> Violations\("))
print("Inject ->", lines_of(g, r"private static string Inject\("))
print("CT-14 ->", lines_of(g, r"public void CT_PBT1B_14_"))
names = re.findall(r'\("([a-z0-9]+)", "', g.split("public void CT_PBT1B_14_")[1].split("foreach")[0])
print("autopruebas de CT-14:", len(names), names)
print("lineas de las autopruebas ->", lines_of(g, r'^\s*\("(a|b|c|e|f1|f2|f3|g1|g2|h1|h2|i|j|k1|k2)", "'))
c = show(HEAD, "tests/RackCad.Tests/PushBackEditableStateCopyTests.cs")
print("AllObjects ->", lines_of(c, r"private static HashSet<object> AllObjects\("), [l for l in c.split("\n") if "private static HashSet<object> AllObjects(" in l])
print("Fingerprint ->", lines_of(c, r"private static string Fingerprint\("))

section("Q-4. Anclas para los menores")
wc = show(HEAD, "src/RackCad.UI/Systems/PushBack/RackPushBackSystemWindow.Composite.cs").split("\n")
start = next(i for i, l in enumerate(wc) if "private void RefreshCompositePanel(" in l)
depth, opened, end = 0, False, None
for j in range(start, len(wc)):
    for ch in re.sub(r"//.*$", "", wc[j]):
        if ch == "{":
            depth += 1
            opened = True
        elif ch == "}":
            depth -= 1
    if opened and depth == 0:
        end = j
        break
print("RefreshCompositePanel -> WC:%d-%d" % (start + 1, end + 1))
print("  siembra:", sorted(set(re.findall(r"(\w+(?:Check|Box|Label))\.", "\n".join(wc[start:end + 1])))))
g10 = show(HEAD, "tests/RackCad.Tests/Workspace/WorkspacePilotUpdateIntegrationGuardTests.cs")
print("G-10 Product ->", [l.strip() for l in g10.split("\n") if "Product() =>" in l])
w = show(HEAD, "src/RackCad.UI/Systems/PushBack/RackPushBackSystemWindow.xaml.cs")
print("W Name_Changed ->", lines_of(w, r"private void Name_Changed\("))
wx = show(HEAD, "src/RackCad.UI/Systems/PushBack/RackPushBackSystemWindow.xaml")
print("WX NameBox TextChanged ->", lines_of(wx, r'x:Name="NameBox".*TextChanged="Name_Changed"'))
print("W comentario D8 ->", lines_of(w, r"dirty global artificial que ADR-0029 D8"))
pesc = show(HEAD, "src/RackCad.Application/Systems/PushBack/PushBackEditableStateComparer.cs")
print("PESC Name en CarriersEqual ->", lines_of(pesc, r"x\.Name.*y\.Name|string\.Equals\(x\.Name"))
own = show(HEAD, "src/RackCad.Application/Systems/PushBack/PushBackEditableStateOwnership.cs")
print("Ownership Of (M09) ->", lines_of(own, r"public static PushBackFieldOwner Of\("))
rmes = show(HEAD, "src/RackCad.Application/Systems/Shared/RackModuleEditSession.cs")
print("RMES SameAs/!= (M12) ->", lines_of(rmes, r"public bool SameAs\(|public bool SameEditAs\("))
print("PESC SameAs uso ->", lines_of(pesc, r"\.SameAs\("))
rect = show(HEAD, "tests/RackCad.UI.Tests/RichEditorCloseContractTests.cs")
print("RichEditorCloseContractTests dirty global ->", lines_of(rect, r"dirty global"))
for f in ["tests/RackCad.UI.Tests/PushBackRichPreviewTests.cs", "tests/RackCad.UI.Tests/PushBackLibrarySaveGateTests.cs",
          "tests/RackCad.UI.Tests/DialogWindowCharacterizationTests.cs"]:
    t = show(HEAD, f)
    print(f, "Fact=%d Theory=%d" % (len(re.findall(r"\[Fact", t)), len(re.findall(r"\[Theory", t))))
sub = show(HEAD, "tests/RackCad.UI.Tests/PushBackFullEditorCharacterizationSubdialogTests.cs")
for n in ["CT_PB0_12_", "CT_PB0_13_", "CT_PB0_14_", "CT_PB0_15_", "CT_PB0_16_"]:
    print(n, "->", lines_of(sub, r"public void " + n))
print("CT_PB0_14 usa AbrirFueraDePantalla ->", "AbrirFueraDePantalla" in sub.split("CT_PB0_14_")[1].split("public void")[0])

section("Q-5. La duracion de los Worker de PB-T1b (relay-record)")
for run in ["1/R20261118T204715Z-a4e2", "2/R20261118T223111Z-4c04"]:
    rr = show(HEAD, "docs/automation/evidence/I-64-pilot/F4B-PBT1B-LOSSLESS-STATE-TYPE/" + run + "/relay-record.json")
    print(run, re.findall(r'"(StartUtc|EndUtc)": "([^"]+)"', rr)[:4])
cl = show(HEAD, "docs/automation/evidence/I-64-pilot/F4B-PBT1B-LOSSLESS-STATE-TYPE/task-close-package.md")
print("CI del GREEN de la correccion ->", [l.strip()[:160] for l in cl.split("\n") if "44438060317" in l][:1])
