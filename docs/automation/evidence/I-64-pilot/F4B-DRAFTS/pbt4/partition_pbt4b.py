"""PB-T4b (paquete v4.4): la partición cerrada de las copias y la comprobación de que pasan en el GREEN, desde las TRX de la precondición 6.

Uso:
    python -I partition_pbt4b.py red <core.trx> <ui.trx> <salida.tsv>
    python -I partition_pbt4b.py green <core.trx> <ui.trx>

- red: las TRX de la copia exportada con el RED en modo Probe (apply_red_pbt4b.py sin --partition). Para cada prueba de una clase
  RackCad[.UI].Tests.FullPanelCopies.<Clase>FullProbeTests: Red si todos sus resultados son Failed y Green si todos son Passed. Escribe
  <salida.tsv> (archivo, metodo, particion, resultados) y su sha256, y el número de resultados de cada clase Red y Green (el MinSelected de sus
  filtros) y de Failed del RED. PARA si una prueba mezcla resultados (una Theory no se parte), si alguna no corre o se salta, o si un fallo no es
  de aserción (su mensaje no es de Xunit.Sdk: la parada C04 del contrato).
- green: las TRX de la misma copia con la partición y el GREEN literal (apply_green_pbt4b.py). Cada prueba de FullPanelCopies tiene que pasar;
  las que no pasan se listan: pasan a equivalentes escritos a mano (D-S1), y la emisión necesita una versión del paquete con su texto.
Solo lee las TRX."""
import hashlib
import os
import re
import sys
import xml.etree.ElementTree as ET

HERE = os.path.dirname(os.path.abspath(__file__))
NS = {'t': 'http://microsoft.com/schemas/VisualStudio/TeamTest/2010'}
SOURCES = {}


def sources():
    if SOURCES:
        return SOURCES
    for l in open(os.path.join(HERE, 'pbt4b-substitution-list-475.tsv'), encoding='utf-8').read().split('\n')[1:]:
        if l:
            path = l.split('\t')[1]
            if path.startswith('(PB-T4a) '):
                continue
            name = os.path.basename(path)[:-3]
            SOURCES[name[:-len('Tests')] if name.endswith('Tests') else name] = path
    return SOURCES


def results(trx):
    root = ET.parse(trx).getroot()
    defs = {}
    for d in root.iterfind('.//t:UnitTest', NS):
        m = d.find('t:TestMethod', NS)
        defs[d.get('id')] = (m.get('className'), m.get('name'))
    out = []
    for r in root.iterfind('.//t:UnitTestResult', NS):
        cls, name = defs.get(r.get('testId'), (None, r.get('testName')))
        msg = r.find('t:Output/t:ErrorInfo/t:Message', NS)
        out.append((cls or '', name, r.get('testName'), r.get('outcome'), (msg.text or '') if msg is not None else ''))
    return out


CLASS = re.compile(r'^RackCad(?:\.UI)?\.Tests\.FullPanelCopies\.(\w+)Full(Probe|Red|Green)Tests$')


def collect(trx_paths):
    per = {}
    for p in trx_paths:
        for cls, method, test_name, outcome, msg in results(p):
            m = CLASS.match(cls)
            if not m:
                continue
            stem, kind = m.group(1), m.group(2)
            path = sources()[stem]
            per.setdefault((path, method), []).append((kind, outcome, msg))
    return per


def main():
    sys.stdout.reconfigure(encoding='utf-8', errors='replace')
    args = sys.argv[1:]
    if not args or args[0] not in ('red', 'green'):
        raise SystemExit(__doc__)
    if args[0] == 'red':
        core, ui, out = args[1:4]
        per = collect([core, ui])
        stops, rows = [], []
        for (path, method), rs in sorted(per.items()):
            outcomes = {o for _, o, _ in rs}
            if outcomes == {'Passed'}:
                part = 'Green'
            elif outcomes == {'Failed'}:
                part = 'Red'
                for _, _, msg in rs:
                    if not msg.lstrip().startswith('Assert.') and 'Xunit.Sdk' not in msg:
                        stops.append('%s.%s falla sin asercion: %s' % (os.path.basename(path), method, msg[:160]))
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
        print('Failed del RED en las copias (Core y UI):', sum(n for _, _, p, n in rows if p == 'Red'))
    else:
        core, ui = args[1:3]
        per = collect([core, ui])
        bad = [(p, m, rs) for (p, m), rs in sorted(per.items()) if {o for _, o, _ in rs} != {'Passed'}]
        print('copias en las TRX del GREEN: %d pruebas; no pasan: %d' % (len(per), len(bad)))
        for p, m, rs in bad:
            print('  D-S1: %s.%s %s' % (os.path.basename(p), m, sorted({o for _, o, _ in rs})))
        if bad:
            raise SystemExit(1)


if __name__ == '__main__':
    main()
