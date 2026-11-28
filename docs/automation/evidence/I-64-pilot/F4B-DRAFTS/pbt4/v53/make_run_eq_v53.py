"""PB-T4d (paquete v5.3): escribe las 29 sustitutas de WorkspacePilotUpdateRunTests (WPUR) sobre el modelo completo, `PushBackRetirePilotRunTests.cs`.

Fuentes: owner-pbt4d-run-coverage-decision.md (las 21, con el mapa de PB-T4b y a mano donde no case), owner-pbt4d-deadcode-27-decisions.md
(pregunta 2: las 21 y la 06, 07, 16, 19, 29 y 42; pregunta 1: la n.o 10, a mano, con el Invalid FrontalBOfOneWay dentro de la corrida) y
owner-pbt4d-cat4d-decisions.md (pregunta 1, CA-T4d I-01: la 48 y la 49, que prueban PushBackPilotSystemRebuild, que se queda; la 49 lleva a mano
solo el comentario del lector, que pasa a ser el del modelo completo). Desde la v5.2, el comentario de la clase da el numero real (CA-T4d M-06).
Cada prueba se copia de 6c2820ba (por objetos git) con su atributo, sus InlineData y el comentario que la precede, y se le aplica el mapa de
PB-T4b (MapM de CT-PBT4B-20) mas las dos entradas de la corrida (PilotUpdateSetup -> FullUpdateSetup, Execute( -> ExecuteFull(). A mano: la
n.o 10 (texto entero, HAND_10), la n.o 43 (HAND_43: la lectura y la comparacion del modelo completo) y el ayudante FullUpdateSetup (la copia de
PilotUpdateSetup, WorkspacePilotUpdateScopeDouble.cs:238-279, con FULL_SETUP_EDITS). Comprueba que cada una de las demas es el mapa exacto de su
original. Escribe el archivo y el registro en UTF-8 con LF. Solo lee el repo.

Uso: python -I make_run_eq_v53.py <repo> <salida.cs> <salida-registro>
"""
import hashlib
import re
import subprocess
import sys

sys.dont_write_bytecode = True
REPO, OUT, LOG = sys.argv[1:4]
REV = '6c2820bab4d8b41e3e229e23f3a1638ccbb04735'
WPUR = 'tests/RackCad.Tests/Workspace/WorkspacePilotUpdateRunTests.cs'
DOUBLE = 'tests/RackCad.Tests/Workspace/WorkspacePilotUpdateScopeDouble.cs'

# Las 29, por su numero en el orden del archivo (v52/cov-tests-r6k.txt) y su nombre.
CHOSEN = [
    ('06', 'ThereIsNoOriginWhenNoMutableMemberHasTheTextOfTheOriginOfTheDraft'),
    ('07', 'SeveralMembersWithTheTextOfTheOriginAreAPreflightWithTheirOwnReasonAndNotTheMisleadingOneOfTheReader'),
    ('08', 'ADifferenceOfTheBaseInPrepareIsAConflictWithItsDifferencesAndNothingIsImported'),
    ('10', 'AViewThatIsNotAvailableInTheOutgoingSystemIsAPreflightAndNothingIsImported'),
    ('11', 'ACleanDraftIsNothingToApplyAndAConflictDraftIsNotApplicableAndAPendingTextIsPendingTextEvenWithoutTheSlot'),
    ('12', 'WithoutSurvivorsNothingIsImportedAndTheRunStops'),
    ('15', 'AnImportThatFailsIsAPreflightWithItsCauseAndNoScopeOfMutationIsStarted'),
    ('16', 'AForeignScopeOpenIsRejectedAsAValueAndNothingIsRereadWrittenOrReleased'),
    ('19', 'ADocumentThatChangedBetweenPrepareAndMutateIsAPreflightWithoutWrites'),
    ('20', 'ABlockThatIsNotTheOneThatWasPreparedIsAPreflightAndNothingIsWritten'),
    ('21', 'TheSurvivorsAndTheLockedLayersAreCheckedAgainInMutateAndNothingIsWritten'),
    ('23', 'AbortThatFailsAfterAUnitStartedIsCommitThrewWithAbortFailedAndNoRollbackIsClaimed'),
    ('24', 'AReleaseThatFailsWithoutSealAndWithoutAUnitStartedIsNotAppliedFailureAndNotAnUncertainOutcome'),
    ('26', 'ASealThatReturnsAndAReleaseThatThrowsGoesToPostAndTheReleaseIsAWarning'),
    ('28', 'APostWithoutValuesOrWithoutItsRereadIsAppliedPostRereadFailed'),
    ('29', 'AForeignScopeOpenAtPostIsAppliedPostRereadFailedWithItsCauseAndTheWarnings'),
    ('30', 'AComparisonThatThrowsAfterTheSealIsAppliedPostRereadFailed'),
    ('31', 'AnOuterReleaseThatThrowsAfterPostTurnsAppliedIntoAppliedWithWarningsAndKeepsTheOtherWarnings'),
    ('32', 'AnOuterReleaseThatThrowsOutsidePostAddsItsCauseToTheClassificationThatWasAlreadyDecided'),
    ('33', 'AnOuterReleaseThatThrowsOnAConflictIsDroppedBecauseAConflictCarriesNoCause'),
    ('34', 'AConflictOrAPreflightOfMutateStaysWhatItIsEvenWhenTheReleaseOfTheScopeOfMutateThrows'),
    ('35', 'ASealThatThrewKeepsBeingCommitThrewBecauseOfTheSealEvenIfTheAbortThrowsToo'),
    ('37', 'TheRunNeverThrowsForMissingArgumentsOrForAPortThatAnswersNothing'),
    ('40', 'ANameThatDiffersAfterTheRenameIsAWarningAndACaseOnlyDifferenceIsNot'),
    ('41', 'TheMissingPiecesAndAFailedRegenerationAreWarningsAndTheStateStaysClean'),
    ('42', 'ThePostComparisonDoesNotNormalizeAnything'),
    ('43', 'ThePostComparisonFailsForAnotherSetOfKeysAnotherEnvelopeAndAnotherScale'),
    ('48', 'TheSystemIsRebuiltFromTheDesignTextOfAMemberOfThePlan'),
    ('49', 'TheSystemRebuiltForAMemberCarriesTheRackNameOfItsEnvelope'),
]
# La n.o 49, a mano sobre el mapa: solo su comentario del lector (el del piloto se retira; la lectura es la del modelo completo).
HAND_49 = [('        // Every member of the plan of a rack carries the name of its envelope, and nothing else of the system changes. (The reader of the pilot\n'
            '        // admits a rack whose views share one name, so the three views carry the same one here; different names come below, by hand.)\n',
            '        // Every member of the plan of a rack carries the name of its envelope, and nothing else of the system changes. (The full reader\n'
            '        // admits a rack whose views share one name, so the three views carry the same one here; different names come below, by hand.)\n')]
HAND_49_NOTE = ('    // I-64 F4B PB-T4d (owner-pbt4d-cat4d-decisions.md, pregunta 1): the map, and by hand only the comment of the reader; the plan is the one\n'
                '    // of the full PREPARE (FullUpdateSetup) and the rebuild, PushBackPilotSystemRebuild, stays.\n')
HAND_48_NOTE = ('    // I-64 F4B PB-T4d (owner-pbt4d-cat4d-decisions.md, pregunta 1): the map; the plan is the one of the full PREPARE (FullUpdateSetup) and\n'
                '    // the rebuild, PushBackPilotSystemRebuild, stays.\n')
# Los ayudantes de WPUR que usan las 29 (StatusAfter va tras la n.o 31, como en el original).
HELPERS = ['Boom', 'Cause']
AFTER = {'31': 'StatusAfter'}
BEFORE = {'40': 'NamedScan'}

# El mapa de PB-T4b (MapM de PushBackEditAllFullCopyGuardTests) y las dos entradas de la corrida.
MAP = [
    (r'\bEditIntentFakeReader\b', 'FullEditIntentFakeReader'),
    (r'\bEditIntentKit\b', 'FullEditIntentKit'),
    (r'\bPrepareKit\b', 'FullPrepareKit'),
    (r'\bPushBackPilotDraftState\b', 'PushBackFullDraftState'),
    (r'\bPushBackPilotStateComparer\b', 'PushBackFullDraftStateComparer'),
    (r'\bPushBackPilotFieldAdapter\b', 'PushBackFullFieldAdapter'),
    (r'\.Inputs\.Annotations!?\.AnnotationScale\b', '.Editable.AnnotationScale'),
    (r'\.Inputs\.PalletTolerance\b', '.Editable.PalletTolerance'),
    (r'\bPilotUpdateSetup\b', 'FullUpdateSetup'),
    (r'\bPushBackPilotUpdateRun\.Execute\(', 'PushBackPilotUpdateRun.ExecuteFull('),
]

NOTE = '    // I-64 F4B PB-T4d'

HAND_10 = '''    // I-64 F4B PB-T4d (owner-pbt4d-deadcode-27-decisions.md, pregunta 1): by hand. The full model erases a lateral that is not available, and
    // BuildCore no longer has a view that is not available (the mode of the pilot that never erases is retired with it); the Invalid of PREPARE
    // inside the run is the frontal of side B of a rack without side B, prepared as in CT-PBT3B-04 (ii), and nothing is imported.
    [Fact]
    public void AViewThatIsNotAvailableInTheOutgoingSystemIsAPreflightAndNothingIsImported()
    {
        var scan = IntentKit.Scan(FullPrepareKit.Rack, "A1", IntentKit.Member("A1", FullPrepareKit.Frontal("pb-20-ab")), IntentKit.Member("FB", FullPrepareKit.Frontal("pb-20-ab", 2)));
        var draft = FullWriteKit.DraftOf(FullWriteKit.Read(scan));
        FullWriteKit.Edit(draft, state => state.Editable.Editor.SetSideBPresent(false));
        var port = new PilotUpdateScopeDouble(FullWriteKit.ItemsOf(scan));
        Assert.True(draft.TryBeginApply().IsStarted);

        var outcome = PushBackPilotUpdateRun.ExecuteFull(draft, new PushBackPilotUpdateRequest(100, FullPrepareKit.Rack), port);

        Assert.Equal(DraftApplyOutcomeKind.NotAppliedPreflight, outcome.Kind);
        Assert.Equal(PushBackPilotUpdatePlanner.FrontalBOfOneWayPrefix + "FB" + PushBackPilotUpdatePlanner.FrontalBOfOneWaySuffix, outcome.Cause);
        Assert.DoesNotContain("PrepareUnits", port.Calls);
    }
'''

# La n.o 43, a mano sobre el mapa: la lectura del modelo completo y su comparacion de POST (la del piloto, ComparePost, se retira).
HAND_43 = [(r'\bIntentKit\.Read\(', 'FullWriteKit.Read('), (r'\bComparePost\(', 'ComparePostFull(')]
HAND_43_NOTE = ('    // I-64 F4B PB-T4d (owner-pbt4d-run-coverage-decision.md): by hand over the map. The comparison of POST of the full model, ComparePostFull,\n'
                '    // over the full read of each re-read (the comparison of the pilot, ComparePost, is retired); the cases and the causes are the same.\n')

# El ayudante: la copia de PilotUpdateSetup con el mapa y estas ediciones a mano (traspaso de la fase 3, seccion 4, punto 1).
FULL_SETUP_EDITS = [
    ('/// <summary>I-64 F4-T4B: the standard rack of the tests of the update, a draft with an edited scale and its double.</summary>\n',
     '/// <summary>\n'
     '/// I-64 F4B PB-T4d (owner-pbt4d-run-coverage-decision.md): the copy of PilotUpdateSetup for the FULL model, the helper of the substitutes of\n'
     '/// the run: the standard rack, a full draft born from the full read with an edited scale, and the double of the port; the plan is the one of\n'
     '/// the full PREPARE and the run is ExecuteFull.\n'
     '/// </summary>\n'),
    ('        var read = IntentKit.Read(Scan);\n', '        var read = FullWriteKit.Read(Scan);\n'),
    ('        Draft = FullPrepareKit.DraftOf(read);\n', '        Draft = FullWriteKit.DraftOf(read);\n'),
    ('        FullPrepareKit.SetScale(Draft, scale);\n', '        FullWriteKit.Edit(Draft, s => s.Editable.AnnotationScale = scale);\n'),
    ('        var result = PushBackPilotUpdatePlanner.Prepare(PushBackPilotPrepareInput.From(Draft), IntentKit.Read(Scan), PilotKit.Catalog);\n',
     '        var result = PushBackPilotUpdatePlanner.PrepareFull(PushBackFullPrepareInput.From(Draft), FullWriteKit.Read(Scan), PilotKit.Catalog);\n'),
    ('    /// <summary>The plan that the run builds for the standard rack, computed apart with the pure PREPARE.</summary>\n',
     '    /// <summary>The plan that the run builds for the standard rack, computed apart with the pure full PREPARE.</summary>\n'),
]

HEADER = '''#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Persistence;
using RackCad.Application.Systems.PushBack.Draft;
using RackCad.Application.Systems.PushBack.Pilot;
using RackCad.Application.Views.Redraw;
using RackCad.Application.Workspace.Drafts;
using Xunit;

namespace RackCad.Tests;

/// <summary>
/// I-64 F4B PB-T4d (owner-pbt4d-run-coverage-decision.md; owner-pbt4d-deadcode-27-decisions.md, pregunta 2; owner-pbt4d-cat4d-decisions.md,
/// pregunta 1): the substitutes, over the full model, of the 29 tests of WorkspacePilotUpdateRunTests that exercise lines or branches of code
/// that stays and that nothing else exercises, measured line by line and branch by branch: 27 of the generic run (Execution&lt;TState&gt;) and
/// of the plan, over ExecuteFull, and 2 of the rebuild of the system of a member (PushBackPilotSystemRebuild), over the plan of the full
/// PREPARE. The run of the pilot that they used as a vehicle is retired; the run, the planner and the reader are the real ones of the full
/// model, with the double of the port that simulates the drawing. Each test keeps the name and the assertions of its original, with the map
/// of PB-T4b (the kit of the copies, the full draft and ExecuteFull); the ones that do not fit the map say so. Of the other 20 tests of the
/// original class, 19 are deleted with their record and one stays in it.
/// </summary>
public class PushBackRetirePilotRunTests
{
'''


def sha(t):
    return hashlib.sha256(t.encode('utf-8')).hexdigest()


def show(path):
    return subprocess.run(['git', '-C', REPO, 'show', REV + ':' + path], capture_output=True, check=True).stdout.decode('utf-8-sig').replace('\r\n', '\n')


def apply(text, rules):
    for pattern, repl in rules:
        text = re.sub(pattern, repl, text)
    return text


def member(lines, name, kind):
    """El texto de un miembro de cuatro espacios: su comentario contiguo, sus atributos y su cuerpo hasta la llave de cierre."""
    if kind == 'test':
        pat = re.compile(r'^    public void ' + re.escape(name) + r'\(')
    else:
        pat = re.compile(r'^    private static [^=(]*\b' + re.escape(name) + r'\(')
    starts = [i for i, l in enumerate(lines) if pat.match(l)]
    assert len(starts) == 1, (name, starts)
    a = starts[0]
    while a > 0 and (lines[a - 1].startswith('    [') or (lines[a - 1].startswith('    //') and not lines[a - 1].startswith('    // ---'))):
        a -= 1
    b = starts[0]
    if '=>' in lines[b]:  # un miembro de expresion: hasta la linea que acaba en ';'
        while not lines[b].rstrip().endswith(';'):
            b += 1
        return '\n'.join(lines[a:b + 1]) + '\n'
    while lines[b] != '    }':
        b += 1
    return '\n'.join(lines[a:b + 1]) + '\n'


base = show(WPUR)
lines = base.split('\n')
log = ['# las 29 sustitutas de WPUR sobre el modelo completo, desde %s:%s (base %s)' % (REV, WPUR, sha(base))]
parts = []
for h in HELPERS:
    original = member(lines, h, 'helper')
    copy = apply(original, MAP)
    parts.append(copy)
    log.append('ayudante %s mapa original=%s copia=%s' % (h, sha(original)[:16], sha(copy)[:16]))
mechanical = hand = 0
for num, name in CHOSEN:
    if num in BEFORE:
        original = member(lines, BEFORE[num], 'helper')
        copy = apply(original, MAP)
        parts.append(copy)
        log.append('ayudante %s mapa original=%s copia=%s' % (BEFORE[num], sha(original)[:16], sha(copy)[:16]))
    original = member(lines, name, 'test')
    if num == '10':
        copy = HAND_10
        how = 'a-mano'
        hand += 1
    elif num == '43':
        mapped = apply(original, MAP)
        copy = HAND_43_NOTE + apply(mapped, HAND_43)
        assert copy != HAND_43_NOTE + mapped
        how = 'mapa+a-mano(lectura y comparacion completas)'
        hand += 1
    elif num == '49':
        mapped = apply(original, MAP)
        copy = mapped
        for old, new in HAND_49:
            assert copy.count(old) == 1, ('49: el comentario del lector no esta una vez', old[:80])
            copy = copy.replace(old, new, 1)
        copy = HAND_49_NOTE + copy
        assert 'PilotUpdateSetup' not in copy and 'PrepareKit.' not in copy.replace('FullPrepareKit.', '')
        how = 'mapa+a-mano(el comentario del lector)'
        hand += 1
    elif num == '48':
        copy = HAND_48_NOTE + apply(original, MAP)
        assert 'PilotUpdateSetup' not in copy
        how = 'mapa'
        mechanical += 1
    else:
        copy = apply(original, MAP)
        assert 'PushBackPilotDraftState' not in copy and 'PilotUpdateSetup' not in copy and 'PrepareKit.' not in copy.replace('FullPrepareKit.', '')
        assert not re.search(r'\bExecute\(', copy) and not re.search(r'\bIntentKit\.Read\(', copy) and not re.search(r'\bComparePost\(', copy), (num, name)
        how = 'mapa'
        mechanical += 1
    parts.append(copy)
    log.append('%s %s %s original=%s copia=%s' % (num, how, name, sha(original)[:16], sha(copy)[:16]))
    if num in AFTER:
        original = member(lines, AFTER[num], 'helper')
        copy = apply(original, MAP)
        parts.append(copy)
        log.append('ayudante %s mapa original=%s copia=%s' % (AFTER[num], sha(original)[:16], sha(copy)[:16]))
body = HEADER + '\n'.join(parts) + '}\n'

double = show(DOUBLE).split('\n')
start = double.index('/// <summary>I-64 F4-T4B: the standard rack of the tests of the update, a draft with an edited scale and its double.</summary>')
setup = '\n'.join(double[start:]).rstrip('\n') + '\n'
assert setup.rstrip('\n').endswith('}') and 'internal sealed class PilotUpdateSetup' in setup
setup_copy = apply(setup, MAP)
for old, new in FULL_SETUP_EDITS:
    assert setup_copy.count(old) == 1, ('FullUpdateSetup: el texto no esta una vez', old[:80])
    setup_copy = setup_copy.replace(old, new, 1)
for word in ('IntentKit.Read(', 'PushBackPilotPrepareInput', 'PushBackPilotDraftState', 'SetScale', 'FullPrepareKit.DraftOf', '.Execute('):
    assert word not in setup_copy, word
log.append('ayudante FullUpdateSetup: copia de PilotUpdateSetup (%s:%d-%d) con el mapa y %d ediciones a mano; original=%s copia=%s'
           % (DOUBLE, start + 1, len(double) - 1, len(FULL_SETUP_EDITS), sha(setup)[:16], sha(setup_copy)[:16]))
text = body + '\n' + setup_copy
assert '\r' not in text and text.endswith('}\n')
with open(OUT, 'w', encoding='utf-8', newline='\n') as f:
    f.write(text)
log.append('pruebas %d (mapa %d, a mano %d); archivo %s %d lineas' % (len(CHOSEN), mechanical, hand, sha(text), text.count('\n')))
with open(LOG, 'w', encoding='utf-8', newline='\n') as f:
    f.write('\n'.join(log) + '\n')
print(log[-1])
