"""PB-T4b, precondición 6 (paquete v4.5): lectura de las TRX y de las salidas de los guiones, y las comprobaciones de lo previsto. Solo lee; escribe
solo las salidas que se le dan.

Uso:
  python -I p6_check.py summary <trx> <salida.txt>
      cuenta las filas por resultado y lista las que no son Passed (clase.metodo, la primera línea del StackTrace y el mensaje, recortados).
  python -I p6_check.py red <core.trx> <particion.tsv> <salida.txt> <ui.trx> [<ui.trx> ...]
      el RED exacto sin las CT: Core 19 586 = (19 129 - R_C) + R_C + 457, con las Failed exactamente las pruebas Red de Core de la partición, y en
      UI las Failed exactamente las Red de UI; las 457 omitidas de Core son las 450 de la lista y las 7 de la base.
  python -I p6_check.py green <core.trx> <salida.txt> <ui.trx> [<ui.trx> ...]
      el GREEN literal sin las CT: Core 19 586 = 19 129 + 0 + 457, y ninguna Failed en UI.
  python -I p6_check.py message <trx> <salida.txt>
      el mensaje de cada resultado que falla (las sondas acaban en Assert.Fail(texto)).
  python -I p6_check.py cost <salida.txt> <sonda.txt> [<sonda.txt> ...]
      lo esperado de la sonda del coste (§6.2.1, paso 7).
  python -I p6_check.py ct06 <salida.txt> <sonda.txt>
      lo esperado de la sonda de CT-06 (§5.2).
  python -I p6_check.py json <particion.tsv> <apply-red.txt> <revision> <salida.json>
      pbt4b-precondition6.json: Schema rackcad-pbt4b-precondition6/v1, Revision, PartitionSha256, CopySha, R_C, R_UI y MinSelected de las 40 clases.
"""
import hashlib
import json
import os
import re
import sys
import xml.etree.ElementTree as ET

NS = {'t': 'http://microsoft.com/schemas/VisualStudio/TeamTest/2010'}
PB4 = os.path.normpath(os.path.join(os.path.dirname(os.path.abspath(__file__)), '..', '..', 'pbt4'))
LIST = os.path.join(PB4, 'pbt4b-substitution-list-477.tsv')
CLASS = re.compile(r'^RackCad(?:\.UI)?\.Tests\.FullPanelCopies\.(\w+)Full(Probe|Red|Green)Tests$')
STEMS = ['WorkspaceLifecycleHost', 'WorkspacePilotPrepare', 'WorkspaceEditIntentHost', 'WorkspacePilotEditHost', 'WorkspaceCommandAdvisor',
         'WorkspacePilotUpdateRequest', 'WorkspacePilotUpdateHost', 'WorkspacePilotReconciliationHost', 'WorkspaceAssociativeBookkeeping',
         'PushBackWritePathGuard', 'WorkspacePilotUpdateIntegrationGuard', 'WorkspaceEditIntentGuard', 'WorkspacePilotEditGuard',
         'WorkspacePilotReconciliationGuard', 'WorkspaceLifecycleGuard', 'WorkspacePilotUpdateGuard', 'WorkspacePilotReconciliation',
         'WorkspacePilotReconciliationView', 'PushBackFullReadPath', 'PushBackFullReadGuard']


def results(trx):
    root = ET.parse(trx).getroot()
    defs = {}
    for d in root.iterfind('.//t:UnitTest', NS):
        m = d.find('t:TestMethod', NS)
        defs[d.get('id')] = (m.get('className'), m.get('name'))
    out = []
    for r in root.iterfind('.//t:UnitTestResult', NS):
        cls, name = defs.get(r.get('testId'), ('', r.get('testName')))
        stack = r.find('t:Output/t:ErrorInfo/t:StackTrace', NS)
        msg = r.find('t:Output/t:ErrorInfo/t:Message', NS)
        first = [l for l in ((stack.text or '') if stack is not None else '').replace('\r\n', '\n').split('\n') if l.strip()]
        out.append({'class': cls or '', 'method': name, 'test': r.get('testName'), 'outcome': r.get('outcome'),
                    'stack1': first[0].strip() if first else '', 'message': (msg.text or '') if msg is not None else '',
                    'duration': r.get('duration')})
    times = root.find('t:Times', NS)
    return out, (times.get('start'), times.get('finish')) if times is not None else (None, None)


def counts(rs):
    c = {}
    for r in rs:
        c[r['outcome']] = c.get(r['outcome'], 0) + 1
    return c


def listed():
    rows = [l.split('\t') for l in open(LIST, encoding='utf-8').read().split('\n')[1:] if l]
    assert len(rows) == 477
    out = set()
    for r in rows:
        path = r[1].replace('(PB-T4a) ', '')
        cls = os.path.basename(path)[:-3]
        out.add((cls, 'CT_PBT4A_06*' if r[1].startswith('(PB-T4a) ') else r[2]))
    return out


def is_listed(cls_full, method, lst):
    cls = cls_full.rsplit('.', 1)[-1]
    if (cls, method) in lst:
        return True
    return cls == 'PushBackFullReadGuardTests' and method.startswith('CT_PBT4A_06') and (cls, 'CT_PBT4A_06*') in lst


def partition(path):
    rows = [l.split('\t') for l in open(path, encoding='utf-8').read().split('\n')[1:] if l]
    return {(r[0], r[1]): (r[2], int(r[3])) for r in rows}


def stem_of(path):
    b = os.path.basename(path)[:-3]
    return b[:-len('Tests')] if b.endswith('Tests') else b


def write(path, lines):
    with open(path, 'w', encoding='utf-8', newline='\n') as f:
        f.write('\n'.join(lines) + '\n')
    print('\n'.join(lines))


def cmd_summary(trx, out):
    rs, (start, finish) = results(trx)
    lines = ['filas %d %s; inicio %s; fin %s' % (len(rs), counts(rs), start, finish)]
    for r in sorted(rs, key=lambda x: (x['outcome'], x['class'], x['test'])):
        if r['outcome'] != 'Passed':
            lines.append('  %s %s.%s | %s | %s' % (r['outcome'], r['class'], r['test'][:160], r['stack1'][:200], ' '.join(r['message'].split())[:200]))
    write(out, lines)


def red_set(part, ui):
    return {(stem_of(p), m) for (p, m), (k, n) in part.items() if k == 'Red' and p.startswith('tests/RackCad.UI.Tests/') == ui}


def failed_set(rs):
    out = set()
    for r in rs:
        if r['outcome'] == 'Failed':
            m = CLASS.match(r['class'])
            out.add((m.group(1) if m else '!' + r['class'], r['method']))
    return out


def base_skips(rs, lst):
    return sorted('%s.%s' % (r['class'], r['method']) for r in rs if r['outcome'] == 'NotExecuted' and not is_listed(r['class'], r['method'], lst))


def cmd_red(core, part_path, out, uis):
    part = partition(part_path)
    lst = listed()
    r_c = sum(n for (p, m), (k, n) in part.items() if k == 'Red' and not p.startswith('tests/RackCad.UI.Tests/'))
    r_ui = sum(n for (p, m), (k, n) in part.items() if k == 'Red' and p.startswith('tests/RackCad.UI.Tests/'))
    rs, (start, finish) = results(core)
    c = counts(rs)
    lines = ['RED Core: filas %d %s; inicio %s; fin %s; R_C de la particion %d' % (len(rs), c, start, finish, r_c)]
    ok = len(rs) == 19586 and c.get('Failed', 0) == r_c and c.get('NotExecuted', 0) == 457 and c.get('Passed', 0) == 19129 - r_c
    fs = failed_set(rs)
    want = red_set(part, False)
    lines.append('  previsto 19 586 = (19 129 - %d) + %d + 457: %s' % (r_c, r_c, ok))
    lines.append('  las Failed son exactamente las Red de Core de la particion: %s (de mas: %s; de menos: %s)' % (
        fs == want, sorted(fs - want)[:10], sorted(want - fs)[:10]))
    bs = base_skips(rs, lst)
    lines.append('  omitidas fuera de la lista (las 7 de la base): %d %s' % (len(bs), bs))
    ok = ok and fs == want and len(bs) == 7
    ui_all = []
    for u in uis:
        urs, _ = results(u)
        ui_all += urs
        lines.append('  UI %s: filas %d %s' % (os.path.basename(u), len(urs), counts(urs)))
    ufs = failed_set(ui_all)
    uwant = red_set(part, True)
    ui_red_results = sum(1 for r in ui_all if r['outcome'] == 'Failed')
    lines.append('  UI: las Failed son exactamente las Red de UI de la particion: %s; resultados Failed %d (R_UI de la particion %d)' % (
        ufs == uwant, ui_red_results, r_ui))
    ok = ok and ufs == uwant and ui_red_results == r_ui
    lines.append('RED: %s' % ('CUMPLE' if ok else 'NO CUMPLE'))
    write(out, lines)
    if not ok:
        raise SystemExit(1)


def cmd_green(core, out, uis):
    lst = listed()
    rs, (start, finish) = results(core)
    c = counts(rs)
    ok = len(rs) == 19586 and c.get('Failed', 0) == 0 and c.get('NotExecuted', 0) == 457 and c.get('Passed', 0) == 19129
    bs = base_skips(rs, lst)
    lines = ['GREEN Core: filas %d %s; inicio %s; fin %s' % (len(rs), c, start, finish),
             '  previsto 19 586 = 19 129 + 0 + 457: %s' % ok,
             '  omitidas fuera de la lista (las 7 de la base): %d %s' % (len(bs), bs)]
    for r in rs:
        if r['outcome'] == 'Failed':
            lines.append('  FALLA %s.%s | %s' % (r['class'], r['test'][:160], r['stack1'][:200]))
    ok = ok and len(bs) == 7
    for u in uis:
        urs, _ = results(u)
        uc = counts(urs)
        lines.append('  UI %s: filas %d %s' % (os.path.basename(u), len(urs), uc))
        for r in urs:
            if r['outcome'] == 'Failed':
                lines.append('  FALLA UI %s.%s | %s' % (r['class'], r['test'][:160], r['stack1'][:200]))
        ok = ok and uc.get('Failed', 0) == 0
    lines.append('GREEN: %s' % ('CUMPLE' if ok else 'NO CUMPLE'))
    write(out, lines)
    if not ok:
        raise SystemExit(1)


def cmd_message(trx, out):
    rs, _ = results(trx)
    lines = []
    for r in rs:
        if r['outcome'] != 'Passed':
            lines.append('== %s.%s %s' % (r['class'], r['method'], r['outcome']))
            lines.extend(r['message'].replace('\r\n', '\n').split('\n'))
    write(out, lines)


def parse_cost(path):
    text = open(path, encoding='utf-8').read()
    mats, med = {}, {}
    for l in text.split('\n'):
        m = re.match(r'^escenario (.+?) definiciones=\d+ \|.*\| materializaciones (.*)$', l.strip())
        if m:
            mats[m.group(1)] = m.group(2).strip()
        m = re.match(r'^medida \| (.+?) \| (.+?) \| mediana=([\d.]+) ms \| p95=([\d.]+) ms \| n=(\d+)$', l.strip())
        if m:
            med[(m.group(1), m.group(2))] = float(m.group(3))
    return mats, med


def cmd_cost(out, probes):
    lines, ok_all = [], True
    for p in probes:
        mats, med = parse_cost(p)
        lines.append('== %s' % os.path.basename(p))
        for s, v in mats.items():
            good = v == 'Begin=1 origen-encontrado=1 cae-al-Primary=1 origen-es-Primary=1'
            ok_all = ok_all and good
            lines.append('  materializaciones %s: %s (%s)' % (s, v, 'bien' if good else 'MAL'))
        for s in mats:
            if s.startswith('pb-21-cola'):
                continue  # rechazado: no materializa
            g = lambda case: med[(s, case)]
            b4, o4, k4, e4 = g('t4b Begin'), g('t4b relectura origen-encontrado'), g('t4b relectura cae-al-Primary (sin coincidencia)'), g('t4b relectura origen-es-Primary')
            b3, o3, k3, e3 = g('t4a Begin'), g('t4a relectura origen-encontrado'), g('t4a relectura cae-al-Primary (sin coincidencia)'), g('t4a relectura origen-es-Primary')
            pil, bsb, base = g('parte: solo el piloto'), g('parte: solo BuildStaleBase'), g('base-emulada (piloto+FromPilot)')
            lim1 = b4 + pil + bsb + max(10.0, 0.3 * b4)
            ok1 = o4 <= lim1
            ok2 = o4 < 0.75 * o3
            ok3 = all(x4 <= x3 + max(10.0, 0.3 * x3) for x4, x3 in ((b4, b3), (k4, k3), (e4, e3)))
            ok_all = ok_all and ok1 and ok2 and ok3
            lines.append('  %s: base %.1f | t4a Begin %.1f origen %.1f cae %.1f es-Primary %.1f | t4b Begin %.1f origen %.1f cae %.1f es-Primary %.1f | '
                         'origen<=%.1f %s; origen<0.75*t4a (%.1f) %s; sin regresion %s; aumento sobre la base: Begin +%.0f, origen +%.0f (t4a +%.0f)' % (
                             s, base, b3, o3, k3, e3, b4, o4, k4, e4, lim1, ok1, 0.75 * o3, ok2, ok3, b4 - base, o4 - base, o3 - base))
    lines.append('COSTE: %s' % ('CUMPLE' if ok_all else 'NO CUMPLE'))
    write(out, lines)
    if not ok_all:
        raise SystemExit(1)


def cmd_ct06(out, probe):
    text = open(probe, encoding='utf-8').read().replace('\r\n', '\n')
    got = [l.strip() for l in text.split('\n') if l.strip().startswith('presentar')]
    want = ['presentar estado=Presented testigo-clave=A1 testigo-texto-igual-al-origen=True borrador-igual=True origen-igual=True lecturas=1 lineas=2',
            'presentar Cambiado fuera: el nombre.', 'presentar Cambiado por ti: las anotaciones.']
    lines = ['CT-06 sonda: %d lineas' % len(got)] + ['  ' + g for g in got] + ['CT-06: %s' % ('CUMPLE' if got == want else 'NO CUMPLE')]
    write(out, lines)
    if got != want:
        raise SystemExit(1)


def cmd_json(part_path, apply_out, revision, out):
    raw = open(part_path, 'rb').read()
    part = partition(part_path)
    copy_sha = {}
    for l in open(apply_out, encoding='utf-8', errors='replace').read().split('\n'):
        m = re.match(r'^([0-9a-f]{64})\s+\d+\s+(tests/\S+Full\.cs)$', l.strip())
        if m:
            copy_sha[m.group(2)] = m.group(1)
    assert len(copy_sha) == 20, sorted(copy_sha)
    mins = {}
    for s in STEMS:
        for part_name in ('Red', 'Green'):
            mins['%sFull%sTests' % (s, part_name)] = sum(n for (p, m), (k, n) in part.items() if stem_of(p) == s and k == part_name)
    assert len(mins) == 40
    r_c = sum(n for (p, m), (k, n) in part.items() if k == 'Red' and not p.startswith('tests/RackCad.UI.Tests/'))
    r_ui = sum(n for (p, m), (k, n) in part.items() if k == 'Red' and p.startswith('tests/RackCad.UI.Tests/'))
    doc = {'Schema': 'rackcad-pbt4b-precondition6/v1', 'Revision': revision, 'PartitionSha256': hashlib.sha256(raw).hexdigest(),
           'CopySha': dict(sorted(copy_sha.items())), 'R_C': r_c, 'R_UI': r_ui, 'MinSelected': mins}
    data = (json.dumps(doc, ensure_ascii=True, indent=2) + '\n').encode('utf-8')
    with open(out, 'wb') as f:
        f.write(data)
    print('json sha256 %s; R_C=%d R_UI=%d; clases con pruebas %d de 40' % (hashlib.sha256(data).hexdigest(), r_c, r_ui, sum(1 for v in mins.values() if v)))


def main():
    sys.stdout.reconfigure(encoding='utf-8', errors='replace')
    a = sys.argv[1:]
    if not a:
        raise SystemExit(__doc__)
    if a[0] == 'summary':
        cmd_summary(a[1], a[2])
    elif a[0] == 'red':
        cmd_red(a[1], a[2], a[3], a[4:])
    elif a[0] == 'green':
        cmd_green(a[1], a[2], a[3:])
    elif a[0] == 'message':
        cmd_message(a[1], a[2])
    elif a[0] == 'cost':
        cmd_cost(a[1], a[2:])
    elif a[0] == 'ct06':
        cmd_ct06(a[1], a[2])
    elif a[0] == 'json':
        cmd_json(a[1], a[2], a[3], a[4])
    else:
        raise SystemExit(__doc__)


if __name__ == '__main__':
    main()
