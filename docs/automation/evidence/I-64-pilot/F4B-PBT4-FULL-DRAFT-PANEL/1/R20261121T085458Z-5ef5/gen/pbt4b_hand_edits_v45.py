"""PB-T4b (paquete v4.5): las ediciones de los equivalentes, las de la v4.4 (pbt4b_hand_edits.py, que no se toca) con tres cambios:
- M-01 de la revisión (§18): el ayudante BaseOrReadUse de la copia de EIG busca las dos lecturas, `pilot` y `full`, no solo `full`;
- CT-PBT4A-03 y CT-PBT4A-05 (owner-pbt4b-list-477-decision.md): sus equivalentes, copias parciales de sus clases de PB-T4a sin M, con el ayudante
  del kit sobre el borrador completo (FullReadRig) y, en CT-05, el lector del kit sin la interfaz nueva, que responde FromReads
  (FullEditIntentFakeReader). Sus ayudantes no cambian.
H, HAND y DS1 son los de la v4.4."""
import copy
import importlib.util
import os

HERE = os.path.dirname(os.path.abspath(__file__))
_spec = importlib.util.spec_from_file_location('pbt4b_hand_edits', os.path.join(HERE, 'pbt4b_hand_edits.py'))
_v44 = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(_v44)

H, HAND, DS1 = _v44.H, _v44.HAND, _v44.DS1
WPP, EIH, WPRH = _v44.WPP, _v44.EIH, _v44.WPRH
WPG, IG, EIG, PEG, RG, WLG, WPUG = _v44.WPG, _v44.IG, _v44.EIG, _v44.PEG, _v44.RG, _v44.WLG, _v44.WPUG
FRP = 'tests/RackCad.Tests/PushBackFullReadPathTests.cs'
FRG = 'tests/RackCad.Tests/PushBackFullReadGuardTests.cs'
CT03 = 'CT_PBT4A_03_TheFiveRereadsOfAnOpenDraftReadAtTheOriginOfTheDraftAndBeginReadsAsToday'
CT05 = 'CT_PBT4A_05_WithAReaderWithoutTheNewInterfaceTheFiveRereadsUseReadIntentAsToday'

HELPER_EDITS = copy.deepcopy(_v44.HELPER_EDITS)
_old = HELPER_EDITS[EIG][0]
assert _old[0].startswith('    private static readonly Regex BaseOrReadUse = new(')
HELPER_EDITS[EIG][0] = (
    _old[0],
    '    private static readonly Regex BaseOrReadUse = new(@"\\.StaleBase\\b|\\.InitialState\\b|\\b(?:pilot|full|result)\\.Read\\b|\\b(?:pilot|full)\\s*[?!]?\\s*\\.\\s*Read\\b|\\.Entries\\b");\n',
    1)
HELPER_EDITS[FRP] = []
HELPER_EDITS[FRG] = []

GUARD_EDITS = copy.deepcopy(_v44.GUARD_EDITS)
GUARD_EDITS[(FRP, CT03)] = [
    ('            Paso(log, () => new PushBackFullReadRig(opening).Begin());\n',
     '            // I-64 F4B PB-T4b (owner-pbt4b-list-477-decision.md): the draft of the panel is the full one, so the rig finds the full draft.\n'
     '            Paso(log, () => new FullReadRig(opening).Begin());\n', 1),
    ('                var rig = new PushBackFullReadRig(reader);\n', '                var rig = new FullReadRig(reader);\n', 1),
]
GUARD_EDITS[(FRG, CT05)] = [
    ('                var reader = new EditIntentFakeReader();\n',
     '                // I-64 F4B PB-T4b (owner-pbt4b-list-477-decision.md): a reader without the new interface that answers with the full read too\n'
     '                // (FromReads), as the reader of the Plugin does; the draft of the panel is the full one, so the rig finds the full draft.\n'
     '                var reader = new FullEditIntentFakeReader();\n', 1),
    ('                var rig = new PushBackFullReadRig(reader);\n', '                var rig = new FullReadRig(reader);\n', 1),
]
