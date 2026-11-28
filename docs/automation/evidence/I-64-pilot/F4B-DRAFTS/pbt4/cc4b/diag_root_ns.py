"""PB-T4b, DIAGNÓSTICO para el análisis de lo que cae fuera de las 477 (no es la precondición 6 ni cambia el paquete): las 20 copias con el espacio de
nombres raíz de su ensamblado en lugar de `…FullPanelCopies`, en una COPIA EXPORTADA.

Uso:
  python -I diag_root_ns.py rewrite <dir-copia> <salida.txt>
      en cada `<Clase>Full.cs` de tests/ que declara `namespace RackCad.Tests.FullPanelCopies` o `namespace RackCad.UI.Tests.FullPanelCopies` (de archivo o de bloque),
      esa línea pasa a `namespace RackCad.Tests;` o `namespace RackCad.UI.Tests;`, y nada más; para si no son 20 o si `FullPanelCopies` sale en
      otro sitio.
  python -I diag_root_ns.py check <particion.tsv> <salida.txt> <trx> [<trx> ...]
      las clases de las copias en el espacio raíz (`RackCad(.UI)?.Tests.<Raíz>Full(Red|Green)Tests`): sus Failed, por prueba, frente a las filas Red
      de la partición, y todo lo que falla fuera de ellas.
Solo escribe en la copia (rewrite) y en la salida."""
import collections
import importlib.util
import os
import re
import sys

HERE = os.path.dirname(os.path.abspath(__file__))
sys.stdout.reconfigure(encoding='utf-8', errors='replace')


def rewrite(copy_dir, out):
    lines, n = [], 0
    for sub, root in (('tests/RackCad.Tests', 'RackCad.Tests'), ('tests/RackCad.UI.Tests', 'RackCad.UI.Tests')):
        base = os.path.join(copy_dir, *sub.split('/'))
        for dp, dns, fs in os.walk(base):
            dns[:] = [d for d in dns if d not in ('bin', 'obj')]
            for f in sorted(fs):
                if not f.endswith('Full.cs'):
                    continue
                p = os.path.join(dp, f)
                text = open(p, 'rb').read().decode('utf-8')
                old = 'namespace %s.FullPanelCopies' % root
                if old not in text:
                    continue
                assert text.count(old) == 1 and text.count('FullPanelCopies') == 1, p
                form = 'de archivo' if (old + ';') in text else 'de bloque'
                new = text.replace(old, 'namespace %s' % root)
                with open(p, 'wb') as fh:
                    fh.write(new.encode('utf-8'))
                n += 1
                lines.append('%s: %s -> namespace %s (%s)' % (os.path.relpath(p, copy_dir).replace(os.sep, '/'), old, root, form))
    lines.append('copias reescritas: %d' % n)
    with open(out, 'w', encoding='utf-8', newline='\n') as fh:
        fh.write('\n'.join(lines) + '\n')
    print('\n'.join(lines))
    if n != 20:
        raise SystemExit(1)


def check(part_path, out, trxs):
    spec = importlib.util.spec_from_file_location('p6c', os.path.join(HERE, 'p6_check_v47.py'))
    p6 = importlib.util.module_from_spec(spec)
    spec.loader.exec_module(p6)
    cls_rx = re.compile(r'^RackCad(?:\.UI)?\.Tests\.(\w+)Full(Probe|Red|Green)Tests$')
    part = p6.partition(part_path)
    want = {(p6.stem_of(p), 'Red', m): n for (p, m, k), n in part.items() if k == 'Red'}
    got, other, counts = collections.Counter(), [], collections.Counter()
    for t in trxs:
        rs, _ = p6.results(t)
        for r in rs:
            counts[r['outcome']] += 1
            if r['outcome'] != 'Failed':
                continue
            m = cls_rx.match(r['class'])
            if m and m.group(1) in p6.STEMS:
                got[(m.group(1), m.group(2), r['method'])] += 1
            else:
                other.append('%s.%s | %s | %s' % (r['class'], r['method'], r['stack1'][:160], ' '.join(r['message'].split())[:300]))
    more = sorted('%s.%s.%s x%d' % (k + (got[k],)) for k in got if got[k] != want.get(k))
    less = sorted('%s.%s.%s x%d' % (k + (want[k],)) for k in want if got.get(k) != want[k])
    lines = ['resultados %s' % dict(counts),
             'Failed de las copias: %d; las Red de la particion: %d' % (sum(got.values()), sum(want.values())),
             'iguales a la particion (clase Red y numero de resultados): %s; de mas %s; de menos %s' % (dict(got) == want, more[:10], less[:10]),
             'Failed fuera de las copias: %d' % len(other)] + ['  ' + o for o in other]
    with open(out, 'w', encoding='utf-8', newline='\n') as fh:
        fh.write('\n'.join(lines) + '\n')
    print('\n'.join(lines))


if __name__ == '__main__':
    a = sys.argv[1:]
    if a and a[0] == 'rewrite':
        rewrite(a[1], a[2])
    elif a and a[0] == 'check':
        check(a[1], a[2], a[3:])
    else:
        raise SystemExit(__doc__)
