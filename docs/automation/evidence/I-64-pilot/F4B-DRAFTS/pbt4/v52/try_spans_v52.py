"""Prueba de spans_v52.span sobre miembros conocidos (SUS-01, las bajas por compilacion de B y la prueba lenta). Imprime; no escribe."""
import importlib.util
import os
import sys

sys.dont_write_bytecode = True
HERE = os.path.dirname(os.path.abspath(__file__))
spec = importlib.util.spec_from_file_location('spans_v52', os.path.join(HERE, 'spans_v52.py'))
sp = importlib.util.module_from_spec(spec)
spec.loader.exec_module(sp)
REPO = sys.argv[1]
W = 'tests/RackCad.Tests/Workspace/'
for path, member in [
    ('tests/RackCad.UI.Tests/Workspace/WorkspaceDraftPilotWindowLoadCharacterizationTests.cs', 'TheWindowAndThePilotDifferOnlyInTheKnownClasses'),
    (W + 'WorkspacePilotPrepareTests.cs', 'ReadyPlan'),
    (W + 'WorkspacePilotUpdateScopeDouble.cs', 'tipo:PilotUpdateSetup'),
    (W + 'WorkspacePilotEditHostTests.cs', 'Key'),
    (W + 'WorkspacePilotEditHostTests.cs', 'tipo:PilotEditThrowingComparer'),
    (W + 'WorkspaceDraftPilotMaterializationTests.cs', 'Assemble'),
    (W + 'WorkspacePilotUpdateRunTests.cs', 'StatusAfter'),
    (W + 'WorkspacePilotPrepareTests.cs', 'ThePlanTypesLiveInThePilotNamespaceAndTheirSignaturesNameNoTypeOfTheHostOrOfTheDrawing'),
    (W + 'WorkspacePilotPrepareFull.cs', 'ThePlanTypesLiveInThePilotNamespaceAndTheirSignaturesNameNoTypeOfTheHostOrOfTheDrawing'),
    (W + 'WorkspacePilotPrepareFull.cs', 'InvalidD_ABuilderThatCannotBuildTheOutgoingSystemIsInvalidWithTheTypedErrorAndNothingIsWritten'),
    (W + 'WorkspacePilotPrepareGuardTests.cs', 'TheFileOfThePlanIsNullableEnabledInTheNamespaceOfThePilotWithBracesAndDeclaresTheTypesOfThePackage'),
    (W + 'WorkspacePilotPrepareGuardTests.cs', 'TheOrderOfPrepareIsTheOneOfThePackageAndNoOutcomeBeforeConflictLooksAtTheReread'),
    ('tests/RackCad.Tests/PushBackEditAllFullCopyGuardTests.cs', 'CT_PBT4B_20_TheScriptOfSkipAndDuplicateGivesTheCopiesTheKitAndTheSkipsOfTheClosedList'),
    (W + 'WorkspacePilotEditGuardTests.cs', 'TheReadGuardAllowsAnotherReadThatALaterTaskWouldAddWithItsOwnMark'),
    (W + 'WorkspacePilotEditGuardTests.cs', 'TheOperationsOfThePortAndTheDecoratorNameNoApiOfWritingTheDrawing'),
]:
    print(path.split('/')[-1], member, sp.span(REPO, path, member))
