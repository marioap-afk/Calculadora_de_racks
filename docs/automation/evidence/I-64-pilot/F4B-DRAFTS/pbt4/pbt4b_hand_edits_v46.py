"""PB-T4b (paquete v4.6): las ediciones de los equivalentes, las de la v4.5 (pbt4b_hand_edits_v45.py, que no se toca) y, por D-S1 (la precondición 6 de
la v4.5, /d/r4g y el diagnóstico /d/r4h: ocho copias «mecánicas» que no compilan sobre el modelo completo), las de esas ocho:
- WorkspacePilotEditHostTests: el ayudante Env.OpenUndecidable DE LA COPIA abre el borrador con la lectura completa (C0_HELPER_EDITS; el original no se
  edita), lo que hace «a mano» a las cuatro pruebas que lo usan, sin edición propia; y las dos pruebas que abren el borrador en su cuerpo, con la
  lectura completa (HAND);
- WorkspacePilotPrepareTests: TheSnapshotIsBuiltOnlyByFrom… con H-02 (PushBackFullPrepareInput.From), y PrepareDoesNotPutTheDraftInConflict… con la
  lectura completa de la misma base (FullHandKit.FullOf) al abrir el borrador.
Las ediciones van sobre el texto ya con M (como HAND)."""
import copy
import importlib.util
import os

HERE = os.path.dirname(os.path.abspath(__file__))
_spec = importlib.util.spec_from_file_location('pbt4b_hand_edits_v45', os.path.join(HERE, 'pbt4b_hand_edits_v45.py'))
_v45 = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(_v45)

H, DS1 = _v45.H, _v45.DS1
WPP, EIH, WPRH = _v45.WPP, _v45.EIH, _v45.WPRH
WPEH = 'tests/RackCad.Tests/Workspace/WorkspacePilotEditHostTests.cs'
WPG, IG, EIG, PEG, RG, WLG, WPUG, FRP, FRG = _v45.WPG, _v45.IG, _v45.EIG, _v45.PEG, _v45.RG, _v45.WLG, _v45.WPUG, _v45.FRP, _v45.FRG
HELPER_EDITS = _v45.HELPER_EDITS
GUARD_EDITS = _v45.GUARD_EDITS

_OPEN_PILOT = '        var read = FullEditIntentKit.Admitted("R9").PilotRead!.Read;\n'
_OPEN_FULL = ('        // I-64 F4B PB-T4b (D-S1): the draft of the panel is the full one, so it is begun with the full read of the same scan.\n'
              '        var read = FullEditIntentKit.Admitted("R9").FullRead!.Read;\n')

HAND = copy.deepcopy(_v45.HAND)
NEW_HAND = {
    # las cuatro que usan Env.OpenUndecidable: a mano por la edición del ayudante, sin edición propia
    (WPEH, 'DiscardRackNeedsNoComparerAndRemovesAnUndecidableDraftToo'): ([], []),
    (WPEH, 'AComparerThatThrowsMakesTheFieldUndecidableProtectedAndNotEditableAndGetFieldNeverThrows'): ([], []),
    (WPEH, 'TheRacksAtRiskAreOrderedOrdinallyAndAComparerThatThrowsMakesThemUncertainApart'): ([], []),
    (WPEH, 'TheFallbackByDraftIsKnownAndKeepsTheUncertainOnesApart'): ([], []),
    # las dos que abren el borrador en su cuerpo
    (WPEH, 'TheOuterCatchOfGetFieldAnswersTheViewInErrorWhenTheCopierOfTheStateAnswersNull'): ([], [(_OPEN_PILOT, _OPEN_FULL, 1)]),
    (WPEH, 'TheReasonOfAFieldThatCannotBeReadIsInSpanish'): ([], [(_OPEN_PILOT, _OPEN_FULL, 1)]),
    # las dos de WPP
    (WPP, 'TheSnapshotIsBuiltOnlyByFromWithCopiesAndTheStatusBeforeAnAttemptWhileApplying'): (['H-02'], []),
    (WPP, 'PrepareDoesNotPutTheDraftInConflictNorCleanDirtyNorTakeTheSlotOfTheUpdate'): ([], [
        ('        var begun = registry.Begin(DraftKit.S1, FullPrepareKit.Rack, read.Read, PushBackFullDraftStateComparer.Instance, PushBackFullDraftStateComparer.Copy);\n',
         '        // I-64 F4B PB-T4b (D-S1): the draft of the panel is the full one, begun with the full read of the same base.\n'
         '        var begun = registry.Begin(DraftKit.S1, FullPrepareKit.Rack, FullHandKit.FullOf(read)!.Read, PushBackFullDraftStateComparer.Instance, PushBackFullDraftStateComparer.Copy);\n', 1),
    ]),
}
for k in NEW_HAND:
    assert k not in HAND, k
HAND.update(NEW_HAND)

# las ediciones de AYUDANTE en las copias de C-0 (en la copia, nunca en el original), sobre el texto con M; cada una, las veces que dice en el
# conjunto de los ayudantes de su archivo
C0_HELPER_EDITS = {
    WPEH: [
        ('            var read = FullEditIntentKit.Admitted(rack).PilotRead!.Read;\n',
         '            // I-64 F4B PB-T4b (D-S1): the draft of the panel is the full one, so it is begun with the full read of the same scan.\n'
         '            var read = FullEditIntentKit.Admitted(rack).FullRead!.Read;\n', 1),
    ],
}
C0_HELPER_EDITED = {WPEH: ['OpenUndecidable']}
