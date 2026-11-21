"""PB-T4c, precondicion 6 (paquete v5.0, con la lista de 4 de owner-pbt4c-list-4-decision.md y D-S9): la lectura de las TRX y las comprobaciones
de lo previsto. Es p6_check_pbt4c_v50.py (que no se toca; el de la primera vuelta, con la lista de 3) con la cuarta prueba de la lista, en UI, y su
equivalente. Solo lee; escribe solo las salidas que se le dan, en UTF-8 con LF.

Uso:
  python -I p6_check_pbt4c_v50b.py summary <trx> <salida.txt>
  python -I p6_check_pbt4c_v50b.py core <red|green> <core.trx> <contrato-pbt4b.txt> <salida.txt>
      la suite completa de Core sin las CT: 19 608 = 19 147 Passed + 0 Failed + 461 Skipped en las dos ramas; las omitidas son las 458 de la base y
      las tres de Core de la lista, con su motivo literal; los tres equivalentes de Core pasan; y los filtros heredados de Core (resultados no
      omitidos y metodos distintos).
  python -I p6_check_pbt4c_v50b.py ui <red|green> <salida.txt> <ui.trx> [<ui.trx> ...]
      los filtros de UI: CT-PBT2B-06 omitida con su motivo literal; su equivalente, PushBackEditAllApplyCloseTests, Failed por asercion en el RED
      (la ventana del esqueleto no cambia) y Passed en el GREEN; ninguna otra Failed.
"""
import os
import sys
import xml.etree.ElementTree as ET

NS = {'t': 'http://microsoft.com/schemas/VisualStudio/TeamTest/2010'}

LISTED_CORE = {
    'RackCad.Tests.WorkspacePilotEditGuardTests.ThePortDeclaresTheSeventeenOperationsWithTheEnumsAsOrderedPrefixesAndNamesNoTypeOfTheDraftsNorOfThePilot':
        'I-64 F4B PB-T4c: sustituida por su equivalente en PushBackEditAllPortEquivalenceGuardTests (registro declarado superseding-characterization.md; decision del Owner owner-pbt4-decisions.md, pregunta 4).',
    'RackCad.Tests.PushBackSharedReferenceGuardTests.CT_PBT1B_15_TheWritersOfNameInThePluginRunAfterTheWindowCloses':
        'I-64 F4B PB-T4c: sustituida por su equivalente en PushBackEditAllNameWritersGuardTests (registro declarado superseding-characterization.md; decision del Owner owner-pbt4-decisions.md, pregunta 4).',
    'RackCad.Tests.WorkspaceFullCopyGuardTests.CT_PBT4B_20_TheScriptOfSkipAndDuplicateGivesTheCopiesTheKitAndTheSkipsOfTheClosedList':
        'I-64 F4B PB-T4c: sustituida por su equivalente en PushBackEditAllFullCopyGuardTests (registro declarado superseding-characterization.md; decision del Owner owner-pbt4c-list-3-decision.md).',
}
LISTED_UI = {
    'RackCad.UI.Tests.PushBackWorkspaceApplyCloseTests.CT_PBT2B_06_ClosingAsksOnlyForWhatWasNotApplied':
        'I-64 F4B PB-T4c: sustituida por su equivalente en PushBackEditAllApplyCloseTests (registro declarado superseding-characterization.md; decision del Owner owner-pbt4c-list-4-decision.md).',
}
EQUIVALENTS_CORE = [
    'RackCad.Tests.PushBackEditAllPortEquivalenceGuardTests.ThePortDeclaresTheSeventeenOperationsWithTheEnumsAsOrderedPrefixesAndNamesNoTypeOfTheDraftsNorOfThePilot',
    'RackCad.Tests.PushBackEditAllNameWritersGuardTests.CT_PBT1B_15_TheWritersOfNameInThePluginRunAfterTheWindowCloses',
    'RackCad.Tests.PushBackEditAllFullCopyGuardTests.CT_PBT4B_20_TheScriptOfSkipAndDuplicateGivesTheCopiesTheKitAndTheSkipsOfTheClosedList',
]
EQUIVALENT_UI = 'RackCad.UI.Tests.PushBackEditAllApplyCloseTests.CT_PBT2B_06_ClosingAsksOnlyForWhatWasNotApplied'
EXPECTED = {'total': 19608, 'Passed': 19147, 'Failed': 0, 'NotExecuted': 461}


def rows(trx):
    root = ET.parse(trx).getroot()
    defs = {}
    for ut in root.iterfind('.//t:TestDefinitions/t:UnitTest', NS):
        tm = ut.find('t:TestMethod', NS)
        defs[ut.get('id')] = (tm.get('className'), tm.get('name'))
    out = []
    for r in root.iterfind('.//t:Results/t:UnitTestResult', NS):
        cls, name = defs.get(r.get('testId'), ('?', '?'))
        msg = r.find('t:Output/t:ErrorInfo/t:Message', NS)
        st = r.find('t:Output/t:ErrorInfo/t:StackTrace', NS)
        out.append({'cls': cls, 'fqn': cls + '.' + name, 'outcome': r.get('outcome'), 'test': r.get('testName'),
                    'message': (msg.text or '') if msg is not None else '', 'stack': (st.text or '') if st is not None else '',
                    'start': r.get('startTime'), 'end': r.get('endTime')})
    return out


def counts(rs):
    c = {}
    for r in rs:
        c[r['outcome']] = c.get(r['outcome'], 0) + 1
    return c


def first_line(text):
    for line in (text or '').split('\n'):
        if line.strip():
            return line.strip()[:200]
    return ''


def write(path, lines):
    with open(path, 'w', encoding='utf-8', newline='\n') as f:
        f.write('\n'.join(lines) + '\n')


def summary(trx, out):
    rs = rows(trx)
    starts = sorted(r['start'] for r in rs if r['start'])
    ends = sorted(r['end'] for r in rs if r['end'])
    lines = ['filas %d %s; inicio %s; fin %s' % (len(rs), dict(sorted(counts(rs).items())), starts[0] if starts else '-', ends[-1] if ends else '-')]
    for r in rs:
        if r['outcome'] != 'Passed':
            lines.append('  %s %s | %s | %s' % (r['outcome'], r['fqn'], first_line(r['stack']), first_line(r['message'])))
    write(out, lines)


def inherited(dump, project):
    out = []
    for line in open(dump, encoding='utf-8').read().split('\n'):
        c = line.split('\t')
        if len(c) == 5 and c[0].isdigit() and c[1] == project:
            out.append((int(c[0]), c[2], c[3], c[4]))
    return out


def core(branch, trx, dump, out):
    rs = rows(trx)
    c = counts(rs)
    ok = len(rs) == EXPECTED['total'] and all(c.get(k, 0) == v for k, v in EXPECTED.items() if k != 'total')
    lines = ['rama %s: filas %d %s' % (branch, len(rs), dict(sorted(c.items()))),
             'totales %s: %d = %d Passed + %d Failed + %d Skipped, esperado 19608 = 19147 + 0 + 461'
             % ('OK' if ok else 'DISTINTO', len(rs), c.get('Passed', 0), c.get('Failed', 0), c.get('NotExecuted', 0))]
    failed = [r for r in rs if r['outcome'] == 'Failed']
    for r in failed:
        lines.append('  Failed %s | %s | %s' % (r['fqn'], first_line(r['stack']), first_line(r['message'])))
    skipped = [r for r in rs if r['outcome'] == 'NotExecuted']
    listed = [r for r in skipped if r['fqn'] in LISTED_CORE]
    reasons = all(LISTED_CORE[r['fqn']] in (r['message'] + r['test']) for r in listed)
    lines.append('omitidas de la lista (Core): %d (esperado 3), motivos literales=%s' % (len(listed), reasons))
    lines.append('omitidas de la base (las 458 de PB-T4b): %d' % (len(skipped) - len(listed)))
    eq = {r['fqn']: r['outcome'] for r in rs if r['fqn'] in EQUIVALENTS_CORE}
    for fqn in EQUIVALENTS_CORE:
        lines.append('equivalente %s %s' % (fqn.split('.')[2], eq.get(fqn, 'AUSENTE')))
    eq_ok = all(eq.get(f) == 'Passed' for f in EQUIVALENTS_CORE)
    starts = sorted(r['start'] for r in rs if r['start'])
    ends = sorted(r['end'] for r in rs if r['end'])
    lines.append('inicio %s fin %s' % (starts[0] if starts else '-', ends[-1] if ends else '-'))
    lines.append('filtros heredados de Core (n.o, filtro, MinSelected de PB-T4b, ExpectRed de PB-T4b, resultados no omitidos, metodos distintos no omitidos):')
    for n, flt, minsel, er in inherited(dump, 'tests/RackCad.Tests/RackCad.Tests.csproj'):
        needle = flt.split('~', 1)[1]
        sel = [r for r in rs if needle in r['fqn'] and r['outcome'] != 'NotExecuted']
        lines.append('  %d\t%s\t%s\t%s\t%d\t%d' % (n, flt, minsel, er, len(sel), len({r['fqn'] for r in sel})))
    verdict = ok and len(listed) == 3 and reasons and eq_ok and not failed
    lines.append('core %s: %s' % (branch, 'PASA' if verdict else 'PARA'))
    write(out, lines)
    return verdict


def ui(branch, out, trxs, dump=None):
    lines, other_failed, seen_listed, eq_outcome, eq_assert = [], 0, None, None, None
    all_rows = []
    for t in trxs:
        rs = rows(t)
        all_rows.extend(rs)
        lines.append('%s filas %d %s' % (os.path.basename(t), len(rs), dict(sorted(counts(rs).items()))))
        for r in rs:
            if r['fqn'] in LISTED_UI:
                seen_listed = (r['outcome'], LISTED_UI[r['fqn']] in (r['message'] + r['test']))
            if r['fqn'] == EQUIVALENT_UI:
                eq_outcome = r['outcome']
                eq_assert = first_line(r['stack']).find('\\tests\\') >= 0 or first_line(r['stack']).find('/tests/') >= 0
            if r['outcome'] == 'Failed' and r['fqn'] != EQUIVALENT_UI:
                other_failed += 1
                lines.append('  Failed %s | %s | %s' % (r['fqn'], first_line(r['stack']), first_line(r['message'])))
    want = 'Failed' if branch == 'red' else 'Passed'
    lines.append('omitida de la lista (UI): %s' % (seen_listed,))
    lines.append('equivalente PushBackEditAllApplyCloseTests %s (esperado %s)%s' % (eq_outcome, want, (' por-asercion=%s' % eq_assert) if eq_outcome == 'Failed' else ''))
    good = other_failed == 0 and seen_listed == ('NotExecuted', True) and eq_outcome == want and (branch != 'red' or eq_assert)
    lines.append('ui %s: %s (otras Failed %d)' % (branch, 'PASA' if good else 'PARA', other_failed))
    write(out, lines)
    return good


if __name__ == '__main__':
    sys.stdout.reconfigure(encoding='utf-8', newline='\n')
    mode = sys.argv[1]
    if mode == 'summary':
        summary(sys.argv[2], sys.argv[3])
    elif mode == 'core':
        sys.exit(0 if core(sys.argv[2], sys.argv[3], sys.argv[4], sys.argv[5]) else 1)
    elif mode == 'ui':
        sys.exit(0 if ui(sys.argv[2], sys.argv[3], sys.argv[4:]) else 1)
    else:
        sys.exit(2)
