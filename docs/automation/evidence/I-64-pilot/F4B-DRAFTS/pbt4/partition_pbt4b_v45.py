# Redaccion para el repo publico: en los casos de prueba de selftest, la letra de unidad de las rutas inventadas se escribe partida ('D' + ':...'), para que la redaccion no las tome por rutas locales. El valor de cada cadena y la autoprueba no cambian.
"""PB-T4b (paquete v4.5): la partición cerrada de las copias y la comprobación de que pasan en el GREEN, desde las TRX de la precondición 6.
Es partition_pbt4b.py (v4.4, que no se toca) con la lista de 477 y, por I-02 de la revisión (§18), la regla de la falta por aserción de
INV-PBT4B-02: la primera línea del StackTrace del resultado está bajo tests/ (como un NullReferenceException en el código de la prueba, porque
Find<PushBackFullDraftState> da null con el anfitrión del piloto). Una falta cuya primera línea no está bajo tests/ (en src/, o sin ruta) para.

Uso:
    python -I partition_pbt4b_v45.py red <core.trx> <ui.trx> <salida.tsv>
    python -I partition_pbt4b_v45.py green <core.trx> <ui.trx>
    python -I partition_pbt4b_v45.py selftest

- red: las TRX de la copia exportada con el RED en modo Probe (apply_red_pbt4b_v45.py sin --partition). Para cada prueba de una clase
  RackCad[.UI].Tests.FullPanelCopies.<Clase>FullProbeTests: Red si todos sus resultados son Failed por aserción y Green si todos son Passed.
  Escribe <salida.tsv> (archivo, metodo, particion, resultados) y su sha256, el número de resultados de cada clase Red y Green (el MinSelected de
  sus filtros) y R_C y R_UI (los resultados de las clases Red de Core y de UI). PARA si una prueba mezcla resultados (una Theory no se parte), si
  alguna no corre o se salta, o si una falta no es por aserción (la parada C04 del contrato).
- green: las TRX de la misma copia con la partición y el GREEN literal (apply_green_pbt4b_v45.py). Cada prueba de FullPanelCopies tiene que pasar;
  las que no pasan se listan: pasan a equivalentes escritos a mano (D-S1), y la emisión necesita una versión del paquete con su texto.
- selftest: la regla de la aserción sobre cuatro StackTrace fabricados.
Solo lee las TRX."""
import hashlib
import os
import re
import sys
import xml.etree.ElementTree as ET

HERE = os.path.dirname(os.path.abspath(__file__))
NS = {'t': 'http://microsoft.com/schemas/VisualStudio/TeamTest/2010'}
SOURCES = {}
UI_PREFIX = 'tests/RackCad.UI.Tests/'


def sources():
    if SOURCES:
        return SOURCES
    for l in open(os.path.join(HERE, 'pbt4b-substitution-list-477.tsv'), encoding='utf-8').read().split('\n')[1:]:
        if l:
            path = l.split('\t')[1]
            if path.startswith('(PB-T4a) '):
                continue
            name = os.path.basename(path)[:-3]
            SOURCES[name[:-len('Tests')] if name.endswith('Tests') else name] = path
    return SOURCES


def by_assertion(stack):
    """La regla de INV-PBT4B-02: la primera línea del StackTrace está bajo tests/ (con las barras normalizadas)."""
    lines = [l for l in (stack or '').replace('\r\n', '\n').split('\n') if l.strip()]
    return bool(lines) and re.search(r'[/\\]tests[/\\]', lines[0]) is not None


def results(trx):
    root = ET.parse(trx).getroot()
    defs = {}
    for d in root.iterfind('.//t:UnitTest', NS):
        m = d.find('t:TestMethod', NS)
        defs[d.get('id')] = (m.get('className'), m.get('name'))
    out = []
    for r in root.iterfind('.//t:UnitTestResult', NS):
        cls, name = defs.get(r.get('testId'), (None, r.get('testName')))
        stack = r.find('t:Output/t:ErrorInfo/t:StackTrace', NS)
        msg = r.find('t:Output/t:ErrorInfo/t:Message', NS)
        out.append((cls or '', name, r.get('outcome'), (stack.text or '') if stack is not None else '', (msg.text or '') if msg is not None else ''))
    return out


CLASS = re.compile(r'^RackCad(?:\.UI)?\.Tests\.FullPanelCopies\.(\w+)Full(Probe|Red|Green)Tests$')


def collect(trx_paths):
    per = {}
    for p in trx_paths:
        for cls, method, outcome, stack, msg in results(p):
            m = CLASS.match(cls)
            if not m:
                continue
            path = sources()[m.group(1)]
            per.setdefault((path, method), []).append((outcome, stack, msg))
    return per


def selftest():
    cases = [
        ('   at RackCad.Tests.FullPanelCopies.XFullProbeTests.M() in D' + ':\\r\\tests\\RackCad.Tests\\Workspace\\XFull.cs:line 10', True),
        ('   at RackCad.Tests.FullPanelCopies.XFullProbeTests.M() in /home/r/tests/RackCad.Tests/XFull.cs:line 3\n   at System.Foo()', True),
        ('   at RackCad.Application.Workspace.WorkspaceHostCore.TryBeginPilotEdit(EditIntentRequest request) in D' + ':\\r\\src\\RackCad.Application\\Workspace\\WorkspaceHostCore.cs:line 9\n'
         '   at RackCad.Tests.FullPanelCopies.XFullProbeTests.M() in D' + ':\\r\\tests\\RackCad.Tests\\XFull.cs:line 3', False),
        ('', False),
    ]
    got = [by_assertion(s) == want for s, want in cases]
    print('selftest de la regla de la asercion: %d de %d' % (sum(got), len(got)))
    if not all(got):
        raise SystemExit(1)


def main():
    sys.stdout.reconfigure(encoding='utf-8', errors='replace')
    args = sys.argv[1:]
    if not args or args[0] not in ('red', 'green', 'selftest'):
        raise SystemExit(__doc__)
    if args[0] == 'selftest':
        selftest()
        return
    if args[0] == 'red':
        core, ui, out = args[1:4]
        per = collect([core, ui])
        stops, rows = [], []
        for (path, method), rs in sorted(per.items()):
            outcomes = {o for o, _, _ in rs}
            if outcomes == {'Passed'}:
                part = 'Green'
            elif outcomes == {'Failed'}:
                part = 'Red'
                for _, stack, msg in rs:
                    if not by_assertion(stack):
                        stops.append('%s.%s falla sin asercion (la primera linea del StackTrace no esta bajo tests/): %s' % (
                            os.path.basename(path), method, (stack.strip().split('\n') or [''])[0][:160] or msg[:160]))
            else:
                stops.append('%s.%s: resultados %s' % (os.path.basename(path), method, sorted(outcomes)))
                continue
            rows.append((path, method, part, len(rs)))
        if stops:
            raise SystemExit('PARA:\n  ' + '\n  '.join(stops))
        text = 'archivo\tmetodo\tparticion\tresultados\n' + ''.join('%s\t%s\t%s\t%d\n' % r for r in rows)
        with open(out, 'w', encoding='utf-8', newline='\n') as f:
            f.write(text)
        print('particion: %d pruebas; sha256 %s' % (len(rows), hashlib.sha256(text.encode('utf-8')).hexdigest()))
        summary = {}
        for path, method, part, n in rows:
            k = (os.path.basename(path)[:-3], part)
            a = summary.setdefault(k, [0, 0])
            a[0] += 1
            a[1] += n
        for (name, part), (tests, n) in sorted(summary.items()):
            print('  %-50s %-5s pruebas=%3d resultados=%4d' % (name, part, tests, n))
        r_c = sum(n for p, _, part, n in rows if part == 'Red' and not p.startswith(UI_PREFIX))
        r_ui = sum(n for p, _, part, n in rows if part == 'Red' and p.startswith(UI_PREFIX))
        print('R_C=%d R_UI=%d' % (r_c, r_ui))
    else:
        core, ui = args[1:3]
        per = collect([core, ui])
        bad = [(p, m, rs) for (p, m), rs in sorted(per.items()) if {o for o, _, _ in rs} != {'Passed'}]
        print('copias en las TRX del GREEN: %d pruebas; no pasan: %d' % (len(per), len(bad)))
        for p, m, rs in bad:
            print('  D-S1: %s.%s %s' % (os.path.basename(p), m, sorted({o for o, _, _ in rs})))
        if bad:
            raise SystemExit(1)


if __name__ == '__main__':
    main()
