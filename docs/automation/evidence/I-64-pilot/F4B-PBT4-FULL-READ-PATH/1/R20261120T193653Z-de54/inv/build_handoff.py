"""Composes worker-handoff.json from the TRX files and the command logs of this run. Usage (from the worktree root): python -I build_handoff.py"""
import json
import re
import subprocess

A = "artifacts/orchestration/I-64/F4B-PBT4-FULL-READ-PATH/1/R20261120T193653Z-de54"
DN = "<datos-locales-del-usuario>/Microsoft/dotnet/dotnet.exe"
BASE = "5aaacce3b278c1f29375de4f24c7fab232f44ad2"
RED = "214f8c75f2ca5a248a5592075c3bac8e667bd362"
GREEN = "faef702055990d1350a73bf4d254eb9e898d5670"
RED_TREE = "d9eed6ebc1b7c7b4a5518f17cd4f8ab65af0babe"
GREEN_TREE = "ef50fdf7d88bece1001172029cf96da0b8db978b"
TRAILER = "Co-Authored-By: Claude Sonnet 5.5 <noreply@anthropic.com>"
BLAME = "--blame-hang-timeout 10m --blame-hang-dump-type none"


def trx(path):
    text = open(path, encoding="utf-8").read()
    counters = re.search(r"<Counters ([^>]*)/>", text).group(1)
    total = int(re.search(r'total="(\d+)"', counters).group(1))
    executed = int(re.search(r'executed="(\d+)"', counters).group(1))
    passed = int(re.search(r'passed="(\d+)"', counters).group(1))
    failed = int(re.search(r'failed="(\d+)"', counters).group(1))
    names = sorted(set(re.findall(r'<UnitTestResult [^>]*testName="([^"]*)"[^>]*outcome="Failed"', text)))
    names += sorted(set(re.findall(r'<UnitTestResult [^>]*outcome="Failed"[^>]*testName="([^"]*)"', text)) - set(names))
    return {"Selected": total, "Passed": passed, "Failed": failed, "Skipped": total - executed, "FailedTests": sorted(set(names))}


executed, results = [], []


def add_build(ref, phase, tree, project, extra=""):
    cmd = "%s build %s -c Release --no-incremental -v:minimal%s" % (DN, project, extra)
    executed.append({"RunRef": ref, "Command": cmd, "Phase": phase, "TreeSha": tree, "ResultsFile": "%s/tests/%s" % (A, LOGS[ref])})


def add_test(ref, phase, tree, command, trx_path):
    executed.append({"RunRef": ref, "Command": command, "Phase": phase, "TreeSha": tree, "ResultsFile": trx_path})
    result = trx(trx_path)
    result["RunRef"] = ref
    results.append(result)


CORE = "tests/RackCad.Tests/RackCad.Tests.csproj"
UI = "tests/RackCad.UI.Tests/RackCad.UI.Tests.csproj"
PLUGIN = "src/RackCad.Plugin/RackCad.Plugin.csproj"
LOGS = {}
for tag, phase, tree, folder in (("b0", "RELEVANT", None, "b0"), ("sk", "RELEVANT", None, "sk"), ("red", "RED", RED_TREE, "red"), ("g0", "GREEN", GREEN_TREE, "g0")):
    for name, project, log, extra in (("core", CORE, "build-core.log", ""), ("ui", UI, "build-ui.log", ""), ("plugin", PLUGIN, "build-plugin.log", " -o %s/tests/%s/plugin-out" % (A, folder))):
        ref = "%s-build-%s" % (tag, name)
        LOGS[ref] = "%s/%s" % (folder, log)
        add_build(ref, phase, tree, project, extra)

# the baseline, over BaseSha
add_test("b0-full", "RELEVANT", None, "%s test %s --logger \"trx;LogFileName=b0full.trx\" --results-directory %s/tests/b0 %s" % (DN, CORE, A, BLAME), "%s/tests/b0/b0full.trx" % A)
add_test("b0-ws", "RELEVANT", None, "%s test %s --no-build --filter \"FullyQualifiedName~RackCad.Tests.Workspace\" --logger \"trx;LogFileName=b0ws.trx\" --results-directory %s/tests/b0 %s" % (DN, CORE, A, BLAME), "%s/tests/b0/b0ws.trx" % A)

# the temporary measurements and the pre-commit run of the new classes (never committed)
add_test("r0-path", "RELEVANT", None, "%s test %s --no-build --filter \"FullyQualifiedName~RackCad.Tests.PushBackFullReadPathTests\" --logger \"trx;LogFileName=r0-PathTests.trx\" --results-directory %s/tests/r0 %s" % (DN, CORE, A, BLAME), "%s/tests/r0/r0-PathTests.trx" % A)
add_test("r0-guard", "RELEVANT", None, "%s test %s --no-build --filter \"FullyQualifiedName~RackCad.Tests.PushBackFullReadGuardTests\" --logger \"trx;LogFileName=r0-GuardTests.trx\" --results-directory %s/tests/r0 %s" % (DN, CORE, A, BLAME), "%s/tests/r0/r0-GuardTests.trx" % A)
add_test("r0-tmp1", "RELEVANT", None, "%s test %s --no-build --filter \"FullyQualifiedName~RackCad.Tests.TmpMeasure\" --logger \"trx;LogFileName=r0-tmp.trx\" --results-directory %s/tests/r0" % (DN, CORE, A), "%s/tests/r0/r0-tmp.trx" % A)
add_test("r0-tmp2", "RELEVANT", None, "%s test %s --no-build --filter \"FullyQualifiedName~RackCad.Tests.TmpMeasurePathTests.CT_PBT4A_0\" --logger \"trx;LogFileName=r0-tmp2.trx\" --results-directory %s/tests/r0" % (DN, CORE, A), "%s/tests/r0/r0-tmp2.trx" % A)
add_test("r0-tmp3", "RELEVANT", None, "%s test %s --no-build --filter \"FullyQualifiedName~RackCad.Tests.TmpMeasurePathTests.CT_PBT4A_07\" --logger \"trx;LogFileName=r0-tmp3.trx\" --results-directory %s/tests/r0" % (DN, CORE, A), "%s/tests/r0/r0-tmp3.trx" % A)

# the RED
add_test("red-full", "RED", RED_TREE, "%s test %s --logger \"trx;LogFileName=red-full.trx\" --results-directory %s/tests/red %s" % (DN, CORE, A, BLAME), "%s/tests/red/red-full.trx" % A)


def filters(folder, prefix, phase, tree, only=None):
    for line in open("%s/tests/%s/commands.txt" % (A, folder), encoding="utf-8").read().strip().split("\n"):
        ref, command = line.split(" | ", 1)
        if not ref.startswith(prefix + "-") and not (only and ref in only):
            continue
        add_test(ref, phase, tree, command, "%s/tests/%s/%s.trx" % (A, folder, ref))


filters("redf", "red", "RED", RED_TREE)
# the GREEN
filters("gf", "g", "GREEN", GREEN_TREE)
for ref in ("g2-1", "g3-1"):
    filters("gf", "zz", "GREEN", GREEN_TREE, only=[ref])
add_test("green-full", "GREEN", GREEN_TREE, "%s test %s --no-build --logger \"trx;LogFileName=green-full.trx\" --results-directory %s/tests/gf %s" % (DN, CORE, A, BLAME), "%s/tests/gf/green-full.trx" % A)

files_changed = subprocess.run(["git", "diff", "--name-only", BASE + ".." + GREEN], capture_output=True, check=True).stdout.decode().split()

WORK = (
    "PB-T4a done in two commits over BaseSha: the RED (RedSha) and the GREEN (CurrentSha); no RED repair was needed (attempt 1 of 3, no RR). "
    "MAP CT -> method (class): CT-PBT4A-01 CT_PBT4A_01_FromReadsCarriesThePilotTheFullReadAndTheCatalogAndTheOtherStatesCarryNeither (PushBackFullReadPathTests); "
    "CT-02 CT_PBT4A_02_TheFullReadAndThePilotReadAreOfTheSameScanOfTheDrawing (PathTests); "
    "CT-03 CT_PBT4A_03_TheFiveRereadsOfAnOpenDraftReadAtTheOriginOfTheDraftAndBeginReadsAsToday (PathTests); "
    "CT-04 CT_PBT4A_04_TheOriginIsChosenByTheTextOfTheDraftAndTheRereadAtItIsPure (PathTests); "
    "CT-05 CT_PBT4A_05_WithAReaderWithoutTheNewInterfaceTheFiveRereadsUseReadIntentAsToday (PushBackFullReadGuardTests); "
    "CT-06 CT_PBT4A_06_OnlyTheFolderOfTheModelThePlanTheRunAndTheAdapterOfThePluginNameItsTypes (GuardTests); "
    "CT-07 CT_PBT4A_07_ThePluginReadsTheFullModelOverTheSameScanAndRereadsAtTheOriginWithoutReadingTheDrawingAgain (PathTests); "
    "CT-08 CT_PBT4A_08_TheNewFileOfTheFolderOfTheModelDoesNotMutateTheResolvedRackAndIsPure (GuardTests). One [Fact] per CT, no Theory; the kit is PushBackFullReadSupport (PushBackFullReadKit, PushBackOriginReaderDouble, PushBackFullReadRig). "
    "STEP 1 ANCHORS (read with git show on BaseSha / the tree, no build): EditIntentReadResult with its private constructor and FromPilot in WEI; the six sentences var read = _editIntentReader!.ReadIntent(request.Session, request.RackId, request.ChosenReference, request.Witness); in WHC (Begin first, then CheckNow, Reload, ConfirmBase, KeepMine, DiscardAndReload, each after its _drafts.Find<PushBackPilotDraftState>); RackPilotIntentAdapter.Read with CaptureDrawing and PushBackPilotIntentReader.Read and RackPilotIntentReadResult in RIA; ReadIntent, ReadOnActiveDocument, LoadCatalog outside the lock, LockDocument, ChooseOriginDefinition and FromPilot(read.Pilot) in NH with PilotIntentReader as the only other class; CT_PBT3A_12 with its [Fact]; EditIntentKit, IntentKit and the fixtures; none of the eleven new names existed in src/ or tests/ (git grep -w, 0 files each, in the base: a1/names-in-base.txt). "
    "SKELETON: sha256 of the custodied patch measured e99c87c61dec083c9b9efefd7a7cf44e0bae31fd0e477b1347618683e4d1de19 (equal to the contract), git apply --check and git apply from the root of the worktree OK; the three Release builds by project compiled with 0 errors and the same warnings by code as the baseline before any new test was written (tests/sk). "
    "SKIP OF CT-PBT3A-12 (line 60 of PushBackDraftWriteGuardTests.cs): before [Fact]; after [Fact(Skip = \"I-64 F4B PB-T4a: sustituida por CT-PBT4A-06 de PushBackFullReadGuardTests (registro declarado superseding-characterization.md; decision del Owner owner-pbt4-decisions.md, pregunta 4).\")]; the only changed line of the file (inv/inv-final.txt). "
    "TABLE CT -> ASSERTION THAT FAILS IN THE RED (all by Assert.Equal of the joined lines, first line of the StackTrace under tests/): CT-01 'fromreads piloto=True completo=True catalogo=True estado=Read' vs 'completo=False catalogo=False' (inert FromReads); CT-02 'pb-01-base piloto=Admitted completo=Admitted mismo-escaneo=True' vs 'completo=NotAdmitted mismo-escaneo=False'; CT-03 'CheckNow lector=ReadIntentAtOrigin origen-del-borrador=True' vs 'CheckNow lector=ReadIntent' (and the four other operations); CT-04 'choose uno-mutable=B2' vs 'choose uno-mutable=null'; CT-07 'ria-Read completo=1 tras-el-piloto=True' vs 'ria-Read completo=0 tras-el-piloto=False'. CT-05, CT-06 and CT-08 pass in the RED, as the package fixes. "
    "SELF-TESTS: CT-07 five (sin-lector-completo, AtOrigin-con-CaptureDrawing, segundo-AtOrigin, FromPilot-en-NH, segunda-lectura-del-piloto-en-RIA) pass as falla in the GREEN; CT-08 the 15 of G-L1 (a b c e f1 f2 f3 g1 g2 h1 h2 i j k1 k2), #nullable, namespace and the 13 terms, all falla in the RED and in the GREEN; CT-06 the ten, all falla. "
    "INV-01/04/05/07/13/14 (inv/inv-final.txt, from git diff BaseSha..CurrentSha): name-status = A PushBackOriginByText.cs, A IWorkspaceEditIntentOriginReader.cs, A the three test classes, M WEI, M WHC, M RIA, M NH, M PushBackDraftWriteGuardTests.cs and nothing else; git diff --name-only RedSha..CurrentSha = the five src files only; src/RackCad.UI, src/RackCad.Domain, RackPushBackCommands.cs and RackCommandSupport.cs empty; WHC text with spaces collapsed == BaseSha with the five sentences replaced (EQUAL, comments included; Begin untouched; Find<PushBackPilotDraftState> 9 -> 9); counts over the whole text, comments included: RIA LockDocument 1->1, StartTransaction 1->1, CaptureDrawing( 1->1, PushBackPilotIntentReader.Read( 1->1, new PushBackPilotIntentScan( 1->1, PushBackFullDraftReader.Read( 0->1, PushBackOriginByText.ReadAt( 0->1; NH LockDocument 2->2, RackPilotIntentAdapter.Read( 1->1, LoadCatalog() 1->1, AtOrigin( 0->1; signature of RackPilotIntentAdapter.Read unchanged; ReadOnActiveDocument gains originText. "
    "CENSUS OF DECLARATIONS (six code paths, base -> CurrentSha, by name; an overload is one more): WEI 192->195 (+EditIntentReadResult.FullRead, +Catalog, +FromReads; the private constructor changes signature, same name); IWorkspaceEditIntentOriginReader.cs 0->2 (+IWorkspaceEditIntentOriginReader, +ReadIntentAtOrigin); PushBackOriginByText.cs 0->4 (+PushBackOriginByText, +Choose, +AtKey, +ReadAt); WHC 127->127 (none); RIA 22->26 (+RackPilotIntentAdapter.AtOrigin, +RackPilotIntentReadResult.Full, +Input, +second constructor); NH 27->28 (+PilotIntentReader.ReadIntentAtOrigin; ReadOnActiveDocument changes signature, same name); names removed: none. The difference is exactly the closed list of PBT4A-C13 (14 names). "
    "D2: no line added to src/ mutates a PushBackSystem or a RackModuleReconciliationResult (added lines listed and scanned in inv/inv-final.txt, 0 flagged). "
    "COUNTS: every filter of RequiredTests (38) over the RED and over the GREEN, one by one, with Selected >= MinSelected (tests/redf/summary.tsv and tests/gf/summary.tsv; UI only the filters of the contract, never the whole UI suite). RED: filter 1 5 selected / 5 Failed; every other filter without Failed. GREEN: every filter without Failed; three consecutive runs of the filter with ExpectRed (g-1, g2-1, g3-1) 5/5 Passed. "
    "TOTALS: Core RED local 19137 = 19125 Passed + 5 Failed + 7 Skipped (CT-PBT4A-01 02 03 04 07 failed; the six of D-1 and CT-PBT3A-12 skipped); Core GREEN local 19137 = 19130 + 0 + 7. CI (UI 2451 = 2430 Passed + 0 Failed + 21 Skipped in both): RED run 44810371492 on RedSha: Core 19137 = 19125 + 5 + 7, jobs tests failure, ui-tests success, build-ui success, build-plugin skipped, no hang (TRX in tests/redci); GREEN run 44814418683 on CurrentSha: Core 19137 = 19130 + 0 + 7, 4 of 4 jobs success, Plugin compiled (TRX in tests/gci). Baseline of the base: Core 19129 = 19123 + 6 (b0-full), filter Workspace 3472 Passed + 6 Skipped. "
    "BUILDS (Release by project, --no-incremental): baseline, skeleton, RED and GREEN: 0 errors and the warning counts by code identical to the baseline in the three projects (diff of the per-code counts empty; baseline Core xUnit2013/2031/2029/1026/2000/2010/2017/2012, UI xUnit2031/2029/2013/2030/1026, Plugin MSB3277); no CS warning. "
    "CLEANLINESS PROBE (before / after, in line, no paths): configurator layout sha256 7AAB8FF7A3E52CE52399DE3B2C8A8AFEEC0C01720D90DAD0CA49D108BFF82621, mtime UTC 2026-11-19T21:41:29.4447298Z, identical; design library folder mtime UTC 2026-10-13T21:42:51.4479430Z and sha256 of its recursive sorted listing BB21C85D0D5A5990C2539D9923050093B985853557D8E907698EE15AE32A1D87, identical (inv/clean-before.txt, inv/clean-after.txt)."
)

EVIDENCE = [
    "RedSha 214f8c75f2ca5a248a5592075c3bac8e667bd362 (tree %s), pushed before the GREEN; CurrentSha %s (tree %s), pushed; HEAD = remote." % (RED_TREE, GREEN, GREEN_TREE),
    "CI of the RED: run 44810371492 on RedSha, push event: tests failure, ui-tests success, build-ui success, build-plugin skipped; Core 19137 = 19125 Passed + 5 Failed + 7 Skipped, UI 2451 = 2430 + 21; TRX %s/tests/redci/core/core.trx and %s/tests/redci/ui/ui.trx; the five failed are CT-PBT4A-01 02 03 04 07." % (A, A),
    "CI of the GREEN: run 44814418683 on CurrentSha, push event: 4 of 4 success, Plugin compiled; Core 19137 = 19130 + 0 + 7, UI 2451 = 2430 + 21; TRX %s/tests/gci/core/core.trx and %s/tests/gci/ui/ui.trx." % (A, A),
    "Skeleton patch: %s/a1/pbt4a-skeleton.patch (sha256 e99c87c61dec083c9b9efefd7a7cf44e0bae31fd0e477b1347618683e4d1de19); names absent in the base: %s/a1/names-in-base.txt." % (A, A),
    "Invariant check script (read only) and its outputs: %s/inv/inv_check.py; %s/inv/inv-pre-green.txt (working tree before the GREEN commit, INV-01 05 04 census); %s/inv/inv-final.txt (git diff BaseSha..CurrentSha, the one that goes with this delivery); the WHC edit was applied with %s/inv/apply_whc.py (five replacements of the literal block of D-PBT4-03)." % (A, A, A, A),
    "Local filter tables: %s/tests/redf/summary.tsv, %s/tests/gf/summary.tsv (commands in commands.txt of each folder); the filters are run by %s/inv/run_filters.sh." % (A, A, A),
    "Cleanliness probe: %s/inv/clean-before.txt and %s/inv/clean-after.txt (identical)." % (A, A),
    "The Trailer of the three commits checked with git log --format=%B BaseSha..HEAD: Co-Authored-By: Claude Sonnet 5.5 <noreply@anthropic.com> in the RED and in the GREEN (two commits, no RED repair).",
    "The handoff was composed by %s/inv/build_handoff.py from the TRX files and the command logs." % A,
]

UNEXPECTED = [
    "Temporary measurement classes (TmpMeasurePathTests with an in-memory copy of the literal GREEN of PushBackOriginByText and of the literal GREEN edits of RIA and NH, applied to the text in memory) were created under tests/RackCad.Tests only to print the expected lines of CT-PBT4A-02, 04 and 07 with Assert.Fail(string.Join(...)), run, and deleted before the RED commit; they never entered a commit (runs r0-tmp1, r0-tmp2, r0-tmp3). The implementation of src/ was not written before the RED commit.",
    "The Edit tool kept the CRLF of the existing src files; the new test files are written with LF in the working copy and git normalizes them (warning of git, no content change).",
    "Core suite duration is about 5 min 15 s to 5 min 34 s locally, inside the 20 min limit of the CI.",
]

LIMITS = [
    "Not verifiable without AutoCAD: that the five rereads really go to the origin of the draft in the drawing (the declared observable change, for the Smoke-PB); the Plugin is only checked as text (CT-PBT4A-07) and compiled (build-plugin of the CI and the local Release build).",
    "Left for PB-T4b: the draft of the panel stays PushBackPilotDraftState (nine Find<PushBackPilotDraftState> unchanged), the host does not use FullRead yet, OB-03.2, the scale rule, Actualizar by ExecuteFull and Conservar mi borrador; left for PB-T4c: Editar todo, the window and the modal; PB-T4d retires the pilot code.",
    "The CI results are of the exact CurrentSha; the verification of the Controller, the reviews and nc1 to nc3 belong to the session.",
    "The use of the machine during the test runs cannot be observed by the Worker.",
]

handoff = {
    "Schema": "rackcad-worker-handoff/v1",
    "TaskId": "F4B-PBT4-FULL-READ-PATH",
    "RunId": "R20261120T193653Z-de54",
    "DelegationRunId": "R20261120T193416Z-a53f",
    "Initiative": "I-64",
    "Gate": "F4B",
    "Attempt": 1,
    "BaseSha": BASE,
    "RedSha": RED,
    "CurrentSha": GREEN,
    "Branch": "architecture/workspace-persistente-rackcad",
    "Worktree": "<worktree-de-I-64>",
    "Pushed": True,
    "FilesChanged": files_changed,
    "WorkCompleted": WORK,
    "TestsExecuted": executed,
    "TestResults": results,
    "Evidence": EVIDENCE,
    "UnexpectedFindings": UNEXPECTED,
    "KnownLimitations": LIMITS,
    "Deviations": [],
    "OpenQuestions": [],
    "WorkerStatus": "IMPLEMENTATION_COMPLETE",
    "Disposition": "NONE",
    "TriggeredStopConditions": [],
    "RecommendedNextAction": "Controller verification over the exact CurrentSha, the direct and independent reviews, and nc1 to nc3; the Smoke-PB for the declared observable change.",
    "Worker": {"Provider": "Anthropic", "ModelRequested": "claude-sonnet-5-5", "EffortRequested": "high", "Trailer": TRAILER},
}
with open(A + "/worker-handoff.json", "w", encoding="utf-8", newline="\n") as handle:
    json.dump(handoff, handle, indent=2, ensure_ascii=False)
    handle.write("\n")
print(len(executed), len(results))
