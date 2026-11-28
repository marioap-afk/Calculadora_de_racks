"""PB-T4b (paquete v4.7): las ediciones de los equivalentes, las de la v4.6 (pbt4b_hand_edits_v46.py, que no se toca) y D-S5 de la sesión
(session-pbt4b-design-decisions.md): las cuatro `Theory` que mezclan resultados en el RED (la precondición 6 de la v4.6, /d/r4i) se parten por
filas en dos equivalentes escritos a mano, uno en <Clase>FullRedTests con las filas que fallan en el RED y otro en <Clase>FullGreenTests con las
que pasan, con el mismo cuerpo y las mismas aserciones; las de `MemberData`, con dos miembros de datos nuevos en la copia que reparten las filas del
original sin cambiar ninguna.

SPLIT: (archivo, método) -> la forma del reparto:
- ('inline', [valores]): las filas `[InlineData(…)]` cuyo primer argumento es uno de esos valores (el texto de la cadena, ya con las ediciones del
  equivalente) van a Red, y las demás a Green;
- ('member', original, rojo, verde): `[MemberData(nameof(original))]` pasa a `nameof(rojo)` en Red y a `nameof(verde)` en Green; los dos miembros
  nuevos están en C0_NEW_HELPERS.
RED_KEYS da, para la partición, el primer argumento de cada fila Red tal como lo escribe xUnit en el nombre del resultado."""
import copy
import importlib.util
import os

HERE = os.path.dirname(os.path.abspath(__file__))
_spec = importlib.util.spec_from_file_location('pbt4b_hand_edits_v46', os.path.join(HERE, 'pbt4b_hand_edits_v46.py'))
_v46 = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(_v46)

H, DS1, HAND, HELPER_EDITS, GUARD_EDITS = _v46.H, _v46.DS1, _v46.HAND, _v46.HELPER_EDITS, _v46.GUARD_EDITS
C0_HELPER_EDITS, C0_HELPER_EDITED = _v46.C0_HELPER_EDITS, _v46.C0_HELPER_EDITED
WPP, EIH, WPRH, WPEH = _v46.WPP, _v46.EIH, _v46.WPRH, _v46.WPEH
WPG, IG, EIG, PEG, RG, WLG, WPUG, FRP, FRG = _v46.WPG, _v46.IG, _v46.EIG, _v46.PEG, _v46.RG, _v46.WLG, _v46.WPUG, _v46.FRP, _v46.FRG
WLHT = 'tests/RackCad.Tests/Workspace/WorkspaceLifecycleHostTests.cs'

STATUS_RED = ['Applying', 'Conflict', 'Dirty', 'DirtyStale', 'Error', 'UnknownCommit', 'UnknownPost']
SPLIT = {
    (WLHT, 'EachOperationThatCanChangeADraftEndsTheMarkSoTheSessionIsRetiredConservatively'):
        ('inline', ['a-check-that-finds-a-conflict', 'a-check-that-recovers', 'a-reload']),
    (PEG, 'TheHostGuardTripsOnAPublicOperationAnOrderOfBasesOrAnAdapterThatIsAFieldOfInstance'):
        ('inline', ['private static readonly PushBackFullFieldAdapter PilotFieldAdapter = new();']),
    (WPP, 'ConfirmAppliesOnlyToCleanDirtyAndConflictAndTheOthersReadNothing'): ('member', 'StatusNames', 'StatusNamesRed', 'StatusNamesGreen'),
    (WPP, 'KeepAppliesToConflictAndToDirtyMarkedStaleApplyingWaitsAndTheOthersAreNotNeededAllWithoutReading'):
        ('member', 'StatusNames', 'StatusNamesRed', 'StatusNamesGreen'),
}
RED_KEYS = {
    (WLHT, 'EachOperationThatCanChangeADraftEndsTheMarkSoTheSessionIsRetiredConservatively'): ['a-check-that-finds-a-conflict', 'a-check-that-recovers', 'a-reload'],
    (PEG, 'TheHostGuardTripsOnAPublicOperationAnOrderOfBasesOrAnAdapterThatIsAFieldOfInstance'): ['private static readonly PushBackFullFieldAdapter PilotFieldAdapter = new();'],
    (WPP, 'ConfirmAppliesOnlyToCleanDirtyAndConflictAndTheOthersReadNothing'): STATUS_RED,
    (WPP, 'KeepAppliesToConflictAndToDirtyMarkedStaleApplyingWaitsAndTheOthersAreNotNeededAllWithoutReading'): STATUS_RED,
}
SPLIT_COMMENT = ('// I-64 F4B PB-T4b (D-S5): this Theory is split by rows by the partition of the RED; here, the rows that %s there, and in the other class '
                 'of the partition, the rest.')

_RED = ('\n'
        '    // I-64 F4B PB-T4b (D-S5): the rows of StatusNames that fail in the RED (every status but Clean, whose draft the rig edits), for the half\n'
        '    // of the Theories split by rows that goes to the Red class. With the member of the Green class they are, row by row, the rows of\n'
        '    // StatusNames.\n'
        '    public static TheoryData<string> StatusNamesRed()\n'
        '    {\n'
        '        var data = new TheoryData<string>();\n'
        '        foreach (string name in StatusNames())\n'
        '        {\n'
        '            if (!string.Equals(name, "Clean", StringComparison.Ordinal)) data.Add(name);\n'
        '        }\n'
        '\n'
        '        return data;\n'
        '    }\n')
_GREEN = ('\n'
          '    // I-64 F4B PB-T4b (D-S5): the row of StatusNames that passes in the RED (Clean), for the half of the Theories split by rows that goes to\n'
          '    // the Green class.\n'
          '    public static TheoryData<string> StatusNamesGreen()\n'
          '    {\n'
          '        var data = new TheoryData<string>();\n'
          '        foreach (string name in StatusNames())\n'
          '        {\n'
          '            if (string.Equals(name, "Clean", StringComparison.Ordinal)) data.Add(name);\n'
          '        }\n'
          '\n'
          '        return data;\n'
          '    }\n')
# (archivo) -> [(el ayudante tras el que va, el nombre del nuevo, su texto)]: miembros nuevos SOLO en la copia
C0_NEW_HELPERS = {WPP: [('StatusNames', 'StatusNamesRed', _RED), ('StatusNamesRed', 'StatusNamesGreen', _GREEN)]}
