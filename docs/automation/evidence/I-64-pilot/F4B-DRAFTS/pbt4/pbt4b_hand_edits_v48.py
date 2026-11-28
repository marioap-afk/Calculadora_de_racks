"""PB-T4b (paquete v4.8): las ediciones de los equivalentes, las de la v4.7 (pbt4b_hand_edits_v47.py, que no se toca) con dos añadidos:
- **D-S1 en dos copias de WPP** (session-pbt4b-design-decisions.md): `InvalidE_AMutableMember…` y `ThePilotNeverErasesAViewAPhantom…` fijaban que
  el piloto nunca borra una vista. Sus equivalentes afirman lo que ya fijan D-PB-04 («borrar vistas obsoletas, sí; crear vistas no está
  autorizado»), D-PBT3-09 (e) (`ObsoleteLateral` → a `Erasures`; `FrontalBOfOneWay` → `Invalid`) y D-PBT4-07: con el lateral fantasma, `Ready`
  con miembros [A1] y borrados [B2]; `side-b`, `Invalid` con `FrontalBOfOneWayPrefix`/`Suffix`; los dos descriptores, como hoy. Las ediciones
  van sobre el texto ya con M (`FullPrepareKit`), y cada una casa las veces que dice o el guion para;
- **la guarda de los textos del anfitrión, en C-5** (owner-pbt4b-list-478-decision.md, la sexta tanda): su equivalente lleva la misma tabla con
  tres cuentas nuevas, 2 -> 0, 5 -> 6 y 1 -> 0, cada una con su comentario `// before N`; la prueba no cambia (GUARD_EDITS vacío).
Lo demás (H, DS1, HAND, HELPER_EDITS, GUARD_EDITS, C0_HELPER_EDITS, SPLIT, RED_KEYS, SPLIT_COMMENT, C0_NEW_HELPERS), como en la v4.7."""
import copy
import importlib.util
import os

HERE = os.path.dirname(os.path.abspath(__file__))
_spec = importlib.util.spec_from_file_location('pbt4b_hand_edits_v47_base48', os.path.join(HERE, 'pbt4b_hand_edits_v47.py'))
_v47 = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(_v47)

H, DS1 = _v47.H, _v47.DS1
C0_HELPER_EDITS, C0_HELPER_EDITED = _v47.C0_HELPER_EDITS, _v47.C0_HELPER_EDITED
WPP, EIH, WPRH, WPEH = _v47.WPP, _v47.EIH, _v47.WPRH, _v47.WPEH
WPG, IG, EIG, PEG, RG, WLG, WPUG, FRP, FRG = _v47.WPG, _v47.IG, _v47.EIG, _v47.PEG, _v47.RG, _v47.WLG, _v47.WPUG, _v47.FRP, _v47.FRG
WLHT = _v47.WLHT
SPLIT, RED_KEYS, SPLIT_COMMENT, STATUS_RED, C0_NEW_HELPERS = _v47.SPLIT, _v47.RED_KEYS, _v47.SPLIT_COMMENT, _v47.STATUS_RED, _v47.C0_NEW_HELPERS
WHHG = 'tests/RackCad.Tests/Workspace/WorkspaceHostHardeningGuardTests.cs'
NONE30 = 'NoneOfThe30LiteralsInEnglishStaysInTheHostOrTheSupportAndTheFourIdentifiersAreKept'
INVALID_E = 'InvalidE_AMutableMemberThatIsNotAvailableInTheOutgoingSystemOrWhoseEnvelopeDoesNotDecodeIsInvalidAndNeverErasesAView'
NEVER_ERASES = 'ThePilotNeverErasesAViewAPhantomThatWasAlreadyInTheDrawingMakesTheUpdateInvalidAndIsNeverInThePlan'

HAND = copy.deepcopy(_v47.HAND)
_ERASES = ('        // I-64 F4B PB-T4b (D-S1; D-PB-04, D-PBT3-09 (e) and D-PBT4-07): the full model erases the obsolete lateral, so the phantom is not\n'
           '        // a view that is no longer available: the update is Ready, the phantom is erased and it is never in the plan of the redrawn views.\n')
NEW_HAND = {
    (WPP, INVALID_E): ([], [
        # the phantom leaves the cases that are Invalid ...
        ('            // A lateral cut at a post that does not exist: a phantom that RACKEDITAR would erase and the pilot never does.\n'
         '            ["phantom-lateral"] = FullPrepareKit.Lateral(FullPrepareKit.MissingPost),\n', '', 1),
        # ... the frontal of side B of a one-way rack is rejected with its own text, and the descriptors keep theirs ...
        ('            Assert.StartsWith((corrupt ? PushBackPilotUpdatePlanner.ViewAddressPrefix : PushBackPilotUpdatePlanner.ViewUnavailablePrefix) + "B2", result.Reason, StringComparison.Ordinal);\n'
         '            Assert.EndsWith(corrupt ? PushBackPilotUpdatePlanner.ViewAddressSuffix : PushBackPilotUpdatePlanner.ViewUnavailableSuffix, result.Reason, StringComparison.Ordinal);\n',
         '            // I-64 F4B PB-T4b (D-S1; D-PBT3-09 (e)): with the full model, the frontal of side B of a rack without side B has its own text.\n'
         '            Assert.StartsWith((corrupt ? PushBackPilotUpdatePlanner.ViewAddressPrefix : PushBackPilotUpdatePlanner.FrontalBOfOneWayPrefix) + "B2", result.Reason, StringComparison.Ordinal);\n'
         '            Assert.EndsWith(corrupt ? PushBackPilotUpdatePlanner.ViewAddressSuffix : PushBackPilotUpdatePlanner.FrontalBOfOneWaySuffix, result.Reason, StringComparison.Ordinal);\n', 1),
        # ... and the phantom is Ready, erased.
        ('        }\n\n        Assert.NotEqual(origin, originFrontal);\n',
         '        }\n\n'
         + _ERASES +
         '        var phantomMembers = FullPrepareKit.BaseOf(("A1", originFrontal), ("B2", FullPrepareKit.Lateral(FullPrepareKit.MissingPost)));\n'
         '        var phantomDraft = FullPrepareKit.DraftOf(phantomMembers, originFrontal);\n'
         '        FullPrepareKit.SetScale(phantomDraft, 2.25);\n'
         '        var phantom = FullPrepareKit.Prepare(phantomDraft, FullPrepareKit.Reread(phantomMembers));\n'
         '        Assert.Equal(PushBackPilotPrepareKind.Ready, phantom.Kind);\n'
         '        Assert.Equal(new[] { "A1" }, phantom.Plan!.Members.Select(m => m.Key).ToArray());\n'
         '        Assert.Equal(new[] { "B2" }, phantom.Plan!.Erasures.ToArray());\n\n'
         '        Assert.NotEqual(origin, originFrontal);\n', 1),
    ]),
    (WPP, NEVER_ERASES): ([], [
        ('        Assert.Equal(PushBackPilotPrepareKind.Invalid, result.Kind);\n        Assert.Null(result.Plan);\n',
         _ERASES +
         '        Assert.Equal(PushBackPilotPrepareKind.Ready, result.Kind);\n'
         '        Assert.Equal(new[] { "A1" }, result.Plan!.Members.Select(m => m.Key).ToArray());\n'
         '        Assert.Equal(new[] { "B2" }, result.Plan!.Erasures.ToArray());\n', 1),
        ('        Assert.Equal(new[] { "A1", "B2" }, ready.Plan!.Members.Select(m => m.Key).ToArray());\n',
         '        Assert.Equal(new[] { "A1", "B2" }, ready.Plan!.Members.Select(m => m.Key).ToArray());\n'
         '        Assert.Empty(ready.Plan!.Erasures);\n', 1),
    ]),
}
for _k in NEW_HAND:
    assert _k not in HAND, _k
HAND.update(NEW_HAND)

# la guarda de los textos del anfitrión (C-5, owner-pbt4b-list-478-decision.md): la tabla, con tres cuentas nuevas y su «before N»
_WHY = ' (I-64 F4B PB-T4b, owner-pbt4b-list-478-decision.md)'
HELPER_EDITS = copy.deepcopy(_v47.HELPER_EDITS)
GUARD_EDITS = copy.deepcopy(_v47.GUARD_EDITS)
assert WHHG not in HELPER_EDITS
HELPER_EDITS[WHHG] = [
    ('        ("The reread carries no result.", "La relectura no trajo resultado.", 2), // before 1\n',
     '        ("The reread carries no result.", "La relectura no trajo resultado.", 0), // before 2%s\n' % _WHY, 1),
    ('        ("The draft is being applied or is no longer registered.", "El borrador se está aplicando o ya no está registrado.", 5), // before 1\n',
     '        ("The draft is being applied or is no longer registered.", "El borrador se está aplicando o ya no está registrado.", 6), // before 5%s\n' % _WHY, 1),
    ('        ("The rack has no annotation scale.", "El rack no tiene escala de anotación.", 1),\n',
     '        ("The rack has no annotation scale.", "El rack no tiene escala de anotación.", 0), // before 1%s\n' % _WHY, 1),
]
GUARD_EDITS[(WHHG, NONE30)] = []
