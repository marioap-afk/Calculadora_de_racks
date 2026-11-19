#!/usr/bin/env python3
"""Compone worker-handoff.json a partir de los resumenes y los TRX de las corridas. Se ejecuta desde la raiz del worktree; solo rutas relativas."""
import json
import os
import re
import xml.etree.ElementTree as ET

RUN = "artifacts/orchestration/I-64/F4B-PBT2B-WORKSPACE-MODE-WINDOW/1/R20261119T155931Z-1a87"
CORE = "tests/RackCad.Tests/RackCad.Tests.csproj"
UI = "tests/RackCad.UI.Tests/RackCad.UI.Tests.csproj"
NS = {"t": "http://microsoft.com/schemas/VisualStudio/TeamTest/2010"}

CORE_FILTERS = [
    "RackCad.Tests.PushBackWorkspaceModeSourceGuardTests", "RackCad.Tests.PushBackWorkspaceSharedReferenceGuardTests",
    "RackCad.Tests.PushBackDraftStatePortTests", "RackCad.Tests.PushBackDraftPortSharedReferenceGuardTests",
    "RackCad.Tests.PushBackWindowOnStateSourceGuardTests", "RackCad.Tests.PushBackWindowSharedReferenceGuardTests",
    "RackCad.Tests.PushBackSharedReferenceGuardTests", "RackCad.Tests.PushBackEditableState", "RackCad.Tests.PushBackEditorOutputGate",
    "RackCad.Tests.PushBackClassicCharacterization", "RackCad.Tests.PushBackFullEditorCharacterization",
    "RackCad.Tests.PushBackModuleEditorCharacterizationTests", "RackCad.Tests.I55DeferredProductFixCharacterizationTests",
    "RackCad.Tests.NamespaceFolderGuardTests", "RackCad.Tests.Workspace",
]
UI_FILTERS = [
    "RackCad.UI.Tests.PushBackWorkspaceModeTests", "RackCad.UI.Tests.PushBackWorkspaceApplyCloseTests",
    "RackCad.UI.Tests.PushBackFieldValidationConcordanceTests", "RackCad.UI.Tests.PushBackWindowOnStateTests",
    "RackCad.UI.Tests.PushBackEditableStateParityTests", "RackCad.UI.Tests.PushBackStateNet",
    "RackCad.UI.Tests.PushBackClassicCharacterization", "RackCad.UI.Tests.PushBackFullEditorCharacterization",
    "RackCad.UI.Tests.PushBackDeclaredCorrection", "RackCad.UI.Tests.PushBackCompositeWindowTests",
    "RackCad.UI.Tests.PushBackCompositeLifecycleWiringTests", "RackCad.UI.Tests.PushBackCompositeLoadExistingModulesTests",
    "RackCad.UI.Tests.PushBackBothModeCellBeamOverrideTests", "RackCad.UI.Tests.PushBackModuleEditorWindowTests",
    "RackCad.UI.Tests.PushBackAdvancedRackParametersWindowTests", "RackCad.UI.Tests.PushBackRichPreviewTests",
    "RackCad.UI.Tests.PushBackLibrarySaveGateTests", "RackCad.UI.Tests.DialogWindowCharacterizationTests",
    "RackCad.UI.Tests.BlankFront", "RackCad.UI.Tests.WindowCensusGuardTests", "RackCad.UI.Tests.RichEditor",
    "RackCad.UI.Tests.Workspace", "RackCad.UI.Tests.Cantilever",
]

tests_executed = []
tests_results = []


def counts(log):
    text = open(log, encoding="utf-8", errors="replace").read()
    matches = re.findall(r"Con error:\s*(\d+), Superado:\s*(\d+), Omitido:\s*(\d+), Total:\s*(\d+)", text)
    if not matches:
        return None
    failed, passed, skipped, total = (int(v) for v in matches[-1])
    return total, passed, failed, skipped


def failed_names(trx, log):
    names = []
    if trx and os.path.exists(trx):
        for r in ET.parse(trx).getroot().iter("{%s}UnitTestResult" % NS["t"]):
            if r.get("outcome") == "Failed":
                names.append(r.get("testName"))
    else:
        text = open(log, encoding="utf-8", errors="replace").read()
        names = sorted(set(re.findall(r"\]\s+(RackCad\.[\w.]+)\s+\[FAIL\]", text)))
    return sorted(names)


def add(run_ref, phase, command, results_file, log):
    tests_executed.append({"RunRef": run_ref, "Command": command, "Phase": phase, "TreeSha": None, "ResultsFile": results_file})
    c = counts(log)
    if c:
        total, passed, failed, skipped = c
        tests_results.append({"RunRef": run_ref, "Selected": total, "Passed": passed, "Failed": failed, "Skipped": skipped,
                              "FailedTests": failed_names(results_file if results_file and results_file.endswith(".trx") else None, log)})


def test_cmd(project, flt, trx, folder, no_build=True):
    return ('"$DOTNET" test %s%s --filter "FullyQualifiedName~%s" --logger "trx;LogFileName=%s" --results-directory %s/tests/%s '
            '--blame-hang-timeout 10m --blame-hang-dump-type none' % (project, " --no-build" if no_build else "", flt, trx, RUN, folder))


def runfilters(ref, phase, which):
    filters = CORE_FILTERS if which == "core" else UI_FILTERS
    project = CORE if which == "core" else UI
    for index, flt in enumerate(filters, start=1):
        prefix = "fc" if which == "core" else "fu"
        trx = "%s%d.trx" % (prefix, index)
        folder = "%s/tests/%s" % (RUN, ref)
        add("%s-%s%d" % (ref, "c" if which == "core" else "u", index), phase,
            test_cmd(project, flt, trx, ref), "%s/%s" % (folder, trx), "%s/%s%d.log" % (folder, prefix, index))


# Paso 1 (corrida temporal de Core, MUTATION)
add("p1", "MUTATION", test_cmd(CORE, "RackCad.Tests.PushBackWorkspaceSharedReferenceGuardTests", "p1.trx", "p1", no_build=False),
    RUN + "/tests/p1/p1.trx", RUN + "/tests/p1/p1.log")

# RED local, tal como se corrio (sin commit): primeras corridas y criterio completo
for name, project, flt, trx in [
    ("r1-mode", UI, "RackCad.UI.Tests.PushBackWorkspaceModeTests", "mode.trx"),
    ("r1-applyclose", UI, "RackCad.UI.Tests.PushBackWorkspaceApplyCloseTests", "applyclose.trx"),
    ("r1-concord", UI, "RackCad.UI.Tests.PushBackFieldValidationConcordanceTests", "concord.trx"),
    ("r1-core-mode", CORE, "RackCad.Tests.PushBackWorkspaceModeSourceGuardTests", "core-mode.trx"),
    ("r1-core-shared", CORE, "RackCad.Tests.PushBackWorkspaceSharedReferenceGuardTests", "core-shared.trx"),
]:
    log = {"r1-mode": "mode.log", "r1-applyclose": "applyclose.log", "r1-concord": "concord.log", "r1-core-mode": "core-mode.log",
           "r1-core-shared": "core-shared.log"}[name]
    add(name, "RED", test_cmd(project, flt, trx, "r1", no_build=(project == UI)), RUN + "/tests/r1/" + trx, RUN + "/tests/r1/" + log)
runfilters("r2", "RED", "core")
runfilters("r3", "RED", "ui")
add("r4-core-full", "RED", '"$DOTNET" test %s --no-build --logger "trx;LogFileName=core-full.trx" --results-directory %s/tests/r4 '
    '--blame-hang-timeout 10m --blame-hang-dump-type none' % (CORE, RUN), RUN + "/tests/r4/core-full.trx", RUN + "/tests/r4/core-full.log")

# Criterio del RED repetido sobre RedRepairSha
runfilters("rrl", "RED", "core")
runfilters("rru", "RED", "ui")
add("rrf-core-full", "RED", '"$DOTNET" test %s --no-build --logger "trx;LogFileName=core-full.trx" --results-directory %s/tests/rrf '
    '--blame-hang-timeout 10m --blame-hang-dump-type none' % (CORE, RUN), RUN + "/tests/rrf/core-full.trx", RUN + "/tests/rrf/core-full.log")

# GREEN
for name, flt, trx, project in [
    ("g0-mode", "RackCad.UI.Tests.PushBackWorkspaceModeTests", "PushBackWorkspaceModeTests.trx", UI),
    ("g0-applyclose", "RackCad.UI.Tests.PushBackWorkspaceApplyCloseTests", "PushBackWorkspaceApplyCloseTests.trx", UI),
    ("g0-concord", "RackCad.UI.Tests.PushBackFieldValidationConcordanceTests", "PushBackFieldValidationConcordanceTests.trx", UI),
]:
    add(name, "GREEN", test_cmd(project, flt, trx, "g0", no_build=False), RUN + "/tests/g0/" + trx,
        RUN + "/tests/g0/" + trx.replace(".trx", ".log"))
runfilters("g1c", "GREEN", "core")
runfilters("g1u", "GREEN", "ui")
add("g1f-core-full", "GREEN", '"$DOTNET" test %s --no-build --logger "trx;LogFileName=core-full.trx" --results-directory %s/tests/g1f '
    '--blame-hang-timeout 10m --blame-hang-dump-type none' % (CORE, RUN), RUN + "/tests/g1f/core-full.trx", RUN + "/tests/g1f/core-full.log")
for r in ("g1", "g2", "g3"):
    for flt, project in [("RackCad.UI.Tests.PushBackWorkspaceModeTests", UI), ("RackCad.UI.Tests.PushBackWorkspaceApplyCloseTests", UI),
                         ("RackCad.Tests.PushBackWorkspaceModeSourceGuardTests", CORE)]:
        trx = "%s-%s.trx" % (r, flt)
        add("%s-%s" % (r, flt.split(".")[-1]), "GREEN", test_cmd(project, flt, trx, "g2"), "%s/tests/g2/%s" % (RUN, trx),
            "%s/tests/g2/%s-%s.log" % (RUN, r, flt))
for r in ("h1", "h2", "h3"):
    add("%s-fingerprints" % r, "GREEN", test_cmd(UI, "RackCad.UI.Tests.PushBackStateNet", r + ".trx", "g3"),
        "%s/tests/g3/%s.trx" % (RUN, r), "%s/tests/g3/%s.log" % (RUN, r))

# Corridas de diagnostico locales sobre el filtro de CT-PBT2B-04, con una instrumentacion temporal de la clase de prueba (revertida, sin commit)
for name, log in [("diag-04a", "diag04.log"), ("diag-04b", "diag04b.log"), ("diag-04c", "diag04c.log"), ("diag-04d", "diag04d.log"), ("diag-04e", "diag04e.log")]:
    add(name, "MUTATION", '"$DOTNET" test %s --filter "FullyQualifiedName~PushBackWorkspaceApplyCloseTests.CT_PBT2B_04"' % UI,
        RUN + "/tests/g0/" + log, RUN + "/tests/g0/" + log)
add("g1-solo04", "GREEN", '"$DOTNET" test %s --filter "FullyQualifiedName~PushBackWorkspaceApplyCloseTests.CT_PBT2B_04"' % UI,
    RUN + "/tests/g1/solo04.log", RUN + "/tests/g1/solo04.log")
add("g1-uiclases", "GREEN", '"$DOTNET" test %s --no-build --filter "FullyQualifiedName~RackCad.UI.Tests.PushBackWorkspace"' % UI,
    RUN + "/tests/g1/all.log", RUN + "/tests/g1/all.log")
add("g1-core-clases", "GREEN", '"$DOTNET" test %s --filter "FullyQualifiedName~RackCad.Tests.PushBackWorkspace"' % CORE,
    RUN + "/tests/g1/core08.log", RUN + "/tests/g1/core08.log")

# Builds Release (sin conteo de pruebas): solo TestsExecuted
for ref, text in [("b0", "linea base sobre BaseSha"), ("r5", "RED"), ("rrb", "RED reparado"), ("g5", "GREEN")]:
    for proj, name, extra in [(CORE, "core", ""), (UI, "ui", ""), ("src/RackCad.Plugin/RackCad.Plugin.csproj", "plugin", " -o %s/tests/%s/plugin-out" % (RUN, ref))]:
        phase = "GREEN" if ref == "g5" else ("RED" if ref in ("r5", "rrb") else "RELEVANT")
        tests_executed.append({"RunRef": "%s-build-%s" % (ref, name), "Phase": phase, "TreeSha": None,
                               "Command": '"$DOTNET" build %s -c Release --no-incremental -v:minimal%s' % (proj, extra),
                               "ResultsFile": "%s/tests/%s/%s.log" % (RUN, ref, name)})

for item in tests_executed:
    path = item["ResultsFile"]
    assert path is None or os.path.exists(path), path

step1 = open(RUN + "/tests/p1/paso1-lineas.txt", encoding="utf-8").read().strip().splitlines()
checks = open(RUN + "/inv/check-inv-final.txt", encoding="utf-8").read().strip().splitlines()
checks_ok = [l for l in checks if l.endswith(" OK") or ": OK" in l]

handoff = {
    "Schema": "rackcad-worker-handoff/v1",
    "TaskId": "F4B-PBT2B-WORKSPACE-MODE-WINDOW",
    "RunId": "R20261119T155931Z-1a87",
    "DelegationRunId": "R20261119T154505Z-69c3",
    "Initiative": "I-64",
    "Gate": "F4B",
    "Attempt": 1,
    "BaseSha": "59839c7148fa101c2f95e1f59b36e6b052216680",
    "RedSha": "a3b86134227e716825c4fbf72c4efba03b819351",
    "CurrentSha": "03b16c88c01ebfd29a642fc6a9324eb76926bcba",
    "Branch": "architecture/workspace-persistente-rackcad",
    "Worktree": os.getcwd(),
    "Pushed": True,
    "FilesChanged": [
        "src/RackCad.UI/Systems/PushBack/PushBackEditorMode.cs",
        "src/RackCad.UI/Systems/PushBack/RackPushBackSystemWindow.xaml.cs",
        "tests/RackCad.Tests/PushBackWorkspaceModeSourceGuardTests.cs",
        "tests/RackCad.Tests/PushBackWorkspaceSharedReferenceGuardTests.cs",
        "tests/RackCad.UI.Tests/PushBackFieldValidationConcordanceTests.cs",
        "tests/RackCad.UI.Tests/PushBackWorkspaceApplyCloseTests.cs",
        "tests/RackCad.UI.Tests/PushBackWorkspaceModeTests.cs",
        "tests/RackCad.UI.Tests/PushBackWorkspaceSupport.cs",
    ],
    "WorkCompleted": None,
    "TestsExecuted": tests_executed,
    "TestResults": tests_results,
    "Evidence": None,
    "UnexpectedFindings": None,
    "KnownLimitations": None,
    "Deviations": None,
    "OpenQuestions": [],
    "WorkerStatus": "IMPLEMENTATION_COMPLETE",
    "Disposition": "NONE",
    "TriggeredStopConditions": [],
    "RecommendedNextAction": None,
    "Worker": {
        "Provider": "Anthropic",
        "ModelRequested": "claude-sonnet-5-5",
        "EffortRequested": "high",
        "Trailer": "Co-Authored-By: Claude Sonnet 5.5 <noreply@anthropic.com>",
    },
}

narrative = json.load(open(RUN + "/inv/narrative.json", encoding="utf-8"))
for key in ("WorkCompleted", "Evidence", "UnexpectedFindings", "KnownLimitations", "Deviations", "RecommendedNextAction"):
    handoff[key] = narrative[key]
handoff["Evidence"] = [e.replace("@@STEP1@@", "\n".join(step1)) for e in handoff["Evidence"]]
handoff["Evidence"].append("Salida de las comprobaciones de INV-PBT2B-01, 03, 04, 06, 07c y 14 y de PBT2B-C13 sobre BaseSha..CurrentSha (%s/inv/check-inv-final.txt): %d lineas, %d en OK; RESULTADO GLOBAL: %s"
                           % (RUN, len(checks), len(checks_ok), checks[-1].split(": ")[-1]))
with open(RUN + "/worker-handoff.json", "w", encoding="utf-8", newline="\n") as handle:
    json.dump(handoff, handle, ensure_ascii=False, indent=2)
    handle.write("\n")
print("TestsExecuted", len(tests_executed), "TestResults", len(tests_results))
