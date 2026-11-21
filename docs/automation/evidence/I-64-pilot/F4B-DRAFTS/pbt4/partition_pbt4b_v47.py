"""PB-T4b (paquete v4.7): la partición cerrada de las copias y la comprobación de que pasan en el GREEN, desde las TRX de la precondición 6. Es
partition_pbt4b_v45.py (que no se toca: la lista de 477 y la regla de la aserción de INV-PBT4B-02) con D-S5: **una `Theory` que mezcla resultados en
el RED se parte por filas**, si es una de las declaradas en SPLIT de pbt4b_hand_edits_v47.py y sus filas que fallan son exactamente las declaradas
(RED_KEYS, por el primer argumento que escribe xUnit en el nombre del resultado). Entonces da dos filas, Red con las que fallan y Green con las que
pasan. Las demás causas siguen parando: una que mezcla sin estar declarada, una declarada que no mezcla o mezcla otras filas, una que no corre o se
salta, y una falta sin aserción.

Uso:
    python -I partition_pbt4b_v47.py red <core.trx> <ui.trx> <salida.tsv>
    python -I partition_pbt4b_v47.py green <core.trx> <ui.trx>
    python -I partition_pbt4b_v47.py selftest
Solo lee las TRX."""
import hashlib
import importlib.util
import os
import re
import sys

HERE = os.path.dirname(os.path.abspath(__file__))


def _load(name):
    spec = importlib.util.spec_from_file_location(name + '_m', os.path.join(HERE, name + '.py'))
    mod = importlib.util.module_from_spec(spec)
    spec.loader.exec_module(mod)
    return mod


p45 = _load('partition_pbt4b_v45')
he = _load('pbt4b_hand_edits_v47')
UI_PREFIX = p45.UI_PREFIX
FIRST = re.compile(r'\(\w+: "((?:[^"\\]|\\.)*)"(···)?')


def first_arg(test_name):
    m = FIRST.search(test_name or '')
    return (m.group(1), bool(m.group(2))) if m else (None, False)


def matches(value, shown):
    shown_text, truncated = shown
    if shown_text is None:
        return False
    return value.startswith(shown_text) if truncated else value == shown_text


def collect(trx_paths):
    per = {}
    for p in trx_paths:
        for cls, method, outcome, stack, msg, test_name in results(p):
            m = p45.CLASS.match(cls)
            if not m:
                continue
            path = p45.sources()[m.group(1)]
            per.setdefault((path, method), []).append((outcome, stack, msg, test_name))
    return per


def results(trx):
    import xml.etree.ElementTree as ET
    root = ET.parse(trx).getroot()
    defs = {}
    for d in root.iterfind('.//t:UnitTest', p45.NS):
        m = d.find('t:TestMethod', p45.NS)
        defs[d.get('id')] = (m.get('className'), m.get('name'))
    out = []
    for r in root.iterfind('.//t:UnitTestResult', p45.NS):
        cls, name = defs.get(r.get('testId'), (None, r.get('testName')))
        stack = r.find('t:Output/t:ErrorInfo/t:StackTrace', p45.NS)
        msg = r.find('t:Output/t:ErrorInfo/t:Message', p45.NS)
        out.append((cls or '', name, r.get('outcome'), (stack.text or '') if stack is not None else '', (msg.text or '') if msg is not None else '', r.get('testName')))
    return out


def selftest():
    cases = [
        (('a-reload', ''), 'X.M(operation: "a-reload")', True),
        (('private static readonly PushBackFullFieldAdapter PilotFieldAdapter = new();', ''),
         'X.M(from: "private static readonly PushBackFullFieldAdapter P"···, to: "private readonly PushBackFullFieldAdapter _adapte"···)', True),
        (('a-reload', ''), 'X.M(operation: "a-drain")', False),
        (('Clean', ''), 'X.M(name: "Clean")', True),
    ]
    got = [matches(v[0], first_arg(n)) == want for v, n, want in cases]
    print('selftest de las filas de D-S5: %d de %d' % (sum(got), len(got)))
    p45.selftest()
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
    if args[0] == 'green':
        sys.argv = [sys.argv[0]] + args
        return p45.main()
    core, ui, out = args[1:4]
    per = collect([core, ui])
    stops, rows, split_done = [], [], set()
    for (path, method), rs in sorted(per.items()):
        outcomes = {o for o, _, _, _ in rs}
        for o, stack, msg, _ in rs:
            if o == 'Failed' and not p45.by_assertion(stack):
                stops.append('%s.%s falla sin asercion (la primera linea del StackTrace no esta bajo tests/): %s' % (
                    os.path.basename(path), method, (stack.strip().split('\n') or [''])[0][:160] or msg[:160]))
        declared = (path, method) in he.RED_KEYS
        if outcomes == {'Passed'} and not declared:
            rows.append((path, method, 'Green', len(rs)))
        elif outcomes == {'Failed'} and not declared:
            rows.append((path, method, 'Red', len(rs)))
        elif outcomes == {'Failed', 'Passed'} and declared:
            keys = he.RED_KEYS[(path, method)]
            bad = []
            for o, _, _, test_name in rs:
                is_red = any(matches(k, first_arg(test_name)) for k in keys)
                if (o == 'Failed') != is_red:
                    bad.append('%s %s' % (o, test_name[-120:]))
            n_red = sum(1 for o, _, _, _ in rs if o == 'Failed')
            if bad or n_red != len(keys):
                stops.append('%s.%s (D-S5): las filas que fallan no son las declaradas: %s' % (os.path.basename(path), method, bad[:4]))
                continue
            rows.append((path, method, 'Red', n_red))
            rows.append((path, method, 'Green', len(rs) - n_red))
            split_done.add((path, method))
        elif declared:
            stops.append('%s.%s (D-S5): declarada para partirse por filas, pero sus resultados son %s' % (os.path.basename(path), method, sorted(outcomes)))
        else:
            stops.append('%s.%s: resultados %s' % (os.path.basename(path), method, sorted(outcomes)))
    missing = set(he.RED_KEYS) - set(per)
    if missing:
        stops.append('D-S5: declaradas sin resultados: %s' % sorted(missing))
    if stops:
        raise SystemExit('PARA:\n  ' + '\n  '.join(stops))
    text = 'archivo\tmetodo\tparticion\tresultados\n' + ''.join('%s\t%s\t%s\t%d\n' % r for r in rows)
    with open(out, 'w', encoding='utf-8', newline='\n') as f:
        f.write(text)
    print('particion: %d filas (%d pruebas; %d partidas por filas, D-S5); sha256 %s' % (len(rows), len(per), len(split_done), hashlib.sha256(text.encode('utf-8')).hexdigest()))
    summary = {}
    for path, method, part, n in rows:
        a = summary.setdefault((os.path.basename(path)[:-3], part), [0, 0])
        a[0] += 1
        a[1] += n
    for (name, part), (tests, n) in sorted(summary.items()):
        print('  %-50s %-5s pruebas=%3d resultados=%4d' % (name, part, tests, n))
    r_c = sum(n for p, _, part, n in rows if part == 'Red' and not p.startswith(UI_PREFIX))
    r_ui = sum(n for p, _, part, n in rows if part == 'Red' and p.startswith(UI_PREFIX))
    print('R_C=%d R_UI=%d' % (r_c, r_ui))


if __name__ == '__main__':
    main()
