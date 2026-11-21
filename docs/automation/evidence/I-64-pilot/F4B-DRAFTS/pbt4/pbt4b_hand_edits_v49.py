"""PB-T4b (paquete v4.9): las ediciones de los equivalentes, las de la v4.8 (pbt4b_hand_edits_v48.py, que no se toca) con UN cambio, CA-T4b I-01
(independent-review-v1.md, §20.10): el equivalente de WHHG `NoneOfThe30…` trae, tras su `Assert.Empty`, la autoprueba de la guarda,
`TheGuardOfTheLiteralsTripsOnAnEnglishLiteralAMissingSpanishTextAndALostIdentifier` (WHHG:501-526 de 78a7cef3): sus dos variables y sus 13
`Assert.NotEmpty` (la §20.10 de la CA-T4b dice 14; en WHHG:501-526 de 78a7cef3 son 13, todas), literales, sobre el `LiteralViolations` y la `Table` de la copia (con las cuentas 0, 6 y 0). Es lo que hace el equivalente de G-4
con la autoprueba de IG:510-517. La original pasa en vacío en el GREEN: su tabla (2, 5 y 1) ya no casa con el anfitrión, así que `LiteralViolations`
sin mutar ya da tres violaciones y sus 13 `NotEmpty` se cumplen con cualquier mutación. La original no está en la lista y no se toca.
Lo demás, como en la v4.8 (y la v4.7)."""
import copy
import importlib.util
import os

HERE = os.path.dirname(os.path.abspath(__file__))
_spec = importlib.util.spec_from_file_location('pbt4b_hand_edits_v48_base49', os.path.join(HERE, 'pbt4b_hand_edits_v48.py'))
_v48 = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(_v48)

for _name in ('H', 'DS1', 'HAND', 'HELPER_EDITS', 'C0_HELPER_EDITS', 'C0_HELPER_EDITED', 'WPP', 'EIH', 'WPRH', 'WPEH', 'WPG', 'IG', 'EIG', 'PEG', 'RG',
              'WLG', 'WPUG', 'FRP', 'FRG', 'WLHT', 'SPLIT', 'RED_KEYS', 'SPLIT_COMMENT', 'STATUS_RED', 'C0_NEW_HELPERS', 'WHHG', 'NONE30', 'INVALID_E',
              'NEVER_ERASES'):
    globals()[_name] = getattr(_v48, _name)
SELFTEST = 'TheGuardOfTheLiteralsTripsOnAnEnglishLiteralAMissingSpanishTextAndALostIdentifier'

# las líneas de la autoprueba original, WHHG:504-525, literales (la autoprueba de selftest_pbt4b_v49.py las compara con el objeto git)
SELFTEST_LINES = (
    '        var host = Src.Code(HostPath);\n'
    '        var support = Src.Code(SupportPath);\n'
    '\n'
    '        // An English literal that comes back, in the host and in the support.\n'
    '        Assert.NotEmpty(LiteralViolations(host + " \\"The reread failed.\\"", support));\n'
    '        Assert.NotEmpty(LiteralViolations(host, support + " \\"A notice of the panel is being delivered.\\""));\n'
    '        // A Spanish text that goes away (here, replaced by another), and one that is repeated.\n'
    '        Assert.NotEmpty(LiteralViolations(host.Replace("\\"Falló la relectura.\\"", "\\"Otra cosa.\\"", StringComparison.Ordinal), support));\n'
    '        Assert.NotEmpty(LiteralViolations(host + " \\"Falló la relectura.\\"", support));\n'
    '        // A lost identifier.\n'
    '        Assert.NotEmpty(LiteralViolations(host.Replace("NoDraftReason", "NoBorradorReason", StringComparison.Ordinal), support));\n'
    '        Assert.NotEmpty(LiteralViolations(host, support.Replace("InsideNoticeReason", "DentroDelAvisoReason", StringComparison.Ordinal)));\n'
    '        // The two new texts: an English original that comes back, and a Spanish text that goes away.\n'
    '        Assert.NotEmpty(LiteralViolations(host + " \\"The draft cannot be checked in its current status.\\"", support));\n'
    '        Assert.NotEmpty(LiteralViolations(host + " \\"Only a draft in conflict or with edits marked as possibly stale is kept.\\"", support));\n'
    '        Assert.NotEmpty(LiteralViolations(host.Replace("El borrador no admite comprobación en su estado actual.", "Otra cosa.", StringComparison.Ordinal), support));\n'
    '        Assert.NotEmpty(LiteralViolations(host.Replace("Solo se conserva un borrador en conflicto", "Se conserva un borrador en conflicto", StringComparison.Ordinal), support));\n'
    '        // A reused text that one operation of the reconciliation stops using changes its exact count.\n'
    '        Assert.NotEmpty(LiteralViolations(host.Replace("\\"El testigo de la vista previa no tiene clave.\\"", "\\"Otro.\\"", StringComparison.Ordinal), support));\n'
    '        // The text of the record or of the differences changes.\n'
    '        Assert.NotEmpty(LiteralViolations(host.Replace("(variable registry)", "(registro de variables)", StringComparison.Ordinal), support));\n'
    '        Assert.NotEmpty(LiteralViolations(host.Replace("Uncertain racks", "Racks inciertos", StringComparison.Ordinal), support));\n')
assert SELFTEST_LINES.count('Assert.NotEmpty(') == 13  # WHHG:501-526 tiene 13 (CA-T4b, §20.10, dice 14: medido en 78a7cef3, son 13)
_EMPTY = '        Assert.Empty(LiteralViolations(Src.Code(HostPath), Src.Code(SupportPath)));\n'
_WHY = ('\n'
        '        // I-64 F4B PB-T4b (CA-T4b I-01; owner-pbt4b-list-478-decision.md): the self-test of the guard (WHHG:501-526), over the table of this\n'
        '        // copy. In the GREEN the original self-test passes vacuously: its table (2, 5 and 1) no longer matches the host (0, 6 and 0), so\n'
        '        // LiteralViolations already answers three violations without any mutation. Here the host is clean for this table (the Assert.Empty\n'
        '        // above), so each of the 13 mutations has to make the guard trip.\n')
GUARD_EDITS = copy.deepcopy(_v48.GUARD_EDITS)
assert GUARD_EDITS[(WHHG, NONE30)] == []
GUARD_EDITS[(WHHG, NONE30)] = [(_EMPTY, _EMPTY + _WHY + SELFTEST_LINES, 1)]
