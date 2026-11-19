# Redaccion para el repo publico: las rutas locales se sustituyen por marcadores (<scratchpad>, <worktree-de-I-64>, <checkout-principal>, <datos-locales-del-usuario>). El resto no cambia.
# Read-only anchor check for the independent review of the PB-T3 package v1.
# Only runs `git show <rev>:<path>` and `git ls-tree` on the main checkout (shared object store).
import re
import subprocess
import sys

REPO = "<checkout-principal>"
REV = "2f5d3dec"
sys.stdout.reconfigure(encoding="utf-8", errors="replace")


def show(path):
    r = subprocess.run(["git", "-C", REPO, "show", REV + ":" + path], capture_output=True, text=True, encoding="utf-8", errors="replace")
    return r.stdout.splitlines() if r.returncode == 0 else None


def lines(path, a, b, width=150):
    t = show(path)
    if t is None:
        print("### " + path + " MISSING")
        return
    print("### " + path + ":" + str(a) + "-" + str(b) + " (total " + str(len(t)) + ")")
    for i in range(a, min(b, len(t)) + 1):
        print("  " + str(i) + ": " + t[i - 1].strip()[:width])


def count_methods(path):
    t = show(path)
    if t is None:
        return "MISSING"
    text = "\n".join(t)
    return len(re.findall(r"\[(?:Fact|Theory)\b", text))


A = "src/RackCad.Application/"
P = A + "Systems/PushBack/Pilot/"
checks = [
    (A + "Workspace/WorkspaceHostCore.cs", 692, 696),
    (A + "Workspace/WorkspaceHostCore.cs", 844, 848),
    (A + "Workspace/WorkspaceHostCore.cs", 893, 897),
    (A + "Workspace/WorkspaceHostCore.cs", 345, 354),
    ("src/RackCad.Plugin/RackPushBackCommands.cs", 184, 200),
    ("src/RackCad.Plugin/RackPushBackCommands.cs", 266, 280),
    ("src/RackCad.Plugin/RackPushBackCommands.cs", 318, 358),
    ("src/RackCad.Plugin/RackPushBackCommands.cs", 366, 384),
    (P + "PushBackPilotFieldAdapter.cs", 22, 41),
    (P + "PushBackPilotStateComparer.cs", 56, 60),
    (A + "Persistence/RackEmbedDocument.cs", 72, 76),
    (A + "Persistence/RackProjectDocument.cs", 49, 53),
    (A + "Persistence/RackEmbedComposer.cs", 40, 62),
    (A + "Persistence/RackProjectStore.cs", 58, 62),
    (A + "Persistence/PushBackDesignDocument.cs", 90, 99),
    (A + "Views/Redraw/RackSiblingMembership.cs", 60, 66),
    ("tests/RackCad.Tests/Workspace/WorkspacePilotUpdateScopeDouble.cs", 11, 16),
    ("src/RackCad.Application/RackCad.Application.csproj", 9, 13),
    (P + "PushBackPilotIntentResult.cs", 25, 42),
    ("docs/automation/evidence/I-64-pilot/F4-SMOKE-2/run/run-log-2.md", 48, 53),
    ("tests/RackCad.Tests/PushBackFullEditorCharacterizationLateralCutTests.cs", 13, 25),
    ("tests/RackCad.Tests/Workspace/WorkspaceDraftPilotIntentStaleBaseTests.cs", 13, 22),
    ("docs/AUTOMATION_PLAN.md", 555, 560),
]
for path, a, b in checks:
    lines(path, a, b)

sd = show(A + "Systems/Registry/SystemRegistry.Default.cs")
if sd is None:
    r = subprocess.run(["git", "-C", REPO, "ls-tree", "-r", "--name-only", REV, "--", "src"], capture_output=True, text=True)
    cands = [x for x in r.stdout.splitlines() if x.endswith("SystemRegistry.Default.cs")]
    print("SystemRegistry.Default.cs candidates: " + ", ".join(cands))
    if cands:
        lines(cands[0], 165, 170)
else:
    lines(A + "Systems/Registry/SystemRegistry.Default.cs", 165, 170)

W = "tests/RackCad.Tests/Workspace/"
for cls in ["WorkspacePilotPrepareTests", "WorkspacePilotUpdateRunTests", "WorkspaceDraftPilotMaterializationTests", "WorkspacePilotUpdateHostTests",
            "WorkspacePilotUpdateRequestTests", "WorkspacePilotReconciliationHostTests", "WorkspaceDraftPilotIntentReadTests", "WorkspaceDraftPilotStateTests",
            "WorkspacePilotPrepareGuardTests", "WorkspacePilotUpdateGuardTests", "WorkspacePilotUpdateIntegrationGuardTests", "WorkspaceDraftPilotIntentGuardTests"]:
    print("METHODS " + cls + " = " + str(count_methods(W + cls + ".cs")))
for path in ["tests/RackCad.Tests/I58F1CharacterizationTests.cs", "tests/RackCad.Tests/I58F1OracleChecks.cs",
             "tests/RackCad.Tests/CustomPropertiesEnvelopeGuardTests.cs", "tests/RackCad.Tests/NamespaceFolderGuardTests.cs",
             "tests/RackCad.Tests/PushBackSharedReferenceGuardTests.cs"]:
    print("METHODS " + path + " = " + str(count_methods(path)))

fp = show("tests/RackCad.UI.Tests/PushBackStateNetFingerprintTests.cs")
if fp is not None:
    text = "\n".join(fp)
    print("bloqueado=True occurrences: " + str(text.count("bloqueado=True")) + "; bloqueado=False: " + str(text.count("bloqueado=False")))
else:
    print("PushBackStateNetFingerprintTests.cs MISSING")
