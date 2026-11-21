"""PB-T4b (paquete v4.4): el guion de «Saltar y duplicar». Genera, por objetos git de una revisión, las copias de las pruebas de la lista de 475
(pbt4b-substitution-list-475.tsv) que se sustituyen por copia o por equivalente escrito a mano, con su texto literal:
- las 9 clases de C-0, enteras: sin los tipos del kit del piloto, con el mapa cerrado M (copy_map_defs.py) y, en los 60 «a mano» del mapa de la
  copia v2 (pbt4b-copy-map-v2.tsv), con sus sustituciones H y sus ediciones literales (pbt4b_hand_edits.py);
- las guardas de la lista (C-5 sin CT-PBT4A-06, y las cinco nuevas): una copia parcial de su clase, con todos sus miembros que no son pruebas (con
  las ediciones de ayudante) y solo sus pruebas de la lista, con sus ediciones; sin M;
- las 27 de la interfaz de «Conservar»: una copia parcial sin ningún cambio.
Cada copia va en un archivo <Clase>Full.cs junto al original, en el espacio de nombres RackCad.Tests.FullPanelCopies (RackCad.UI.Tests.FullPanelCopies
en la UI), con cada tipo de primer nivel T renombrado T + Full (M-00) y la clase de prueba partida en <Clase>FullRedTests y <Clase>FullGreenTests
según la partición (o, para medirla, una sola <Clase>FullProbeTests). Las pruebas no cambian de nombre. Nada más cambia.

Uso: python -I make_copies_pbt4b.py <checkout> <rev> <dir-salida> [--partition <pbt4b-copy-partition.tsv>] [--review <archivo.txt>]
- <dir-salida>: la raíz donde se escriben las rutas tests/... (una copia exportada, o una carpeta vacía); no puede tener .git.
- sin --partition: modo de medida (una clase Probe por archivo); con --partition: la partición cerrada (método -> Red | Green).
- --review: además, escribe un archivo con el texto final de cada equivalente a mano y de cada guarda, para revisarlo.
Imprime el sha256 de cada archivo escrito y el del conjunto. Solo `git show` (objetos git)."""
import hashlib
import importlib.util
import os
import re
import subprocess
import sys

HERE = os.path.dirname(os.path.abspath(__file__))


def _load(name):
    spec = importlib.util.spec_from_file_location(name, os.path.join(HERE, name + '.py'))
    mod = importlib.util.module_from_spec(spec)
    spec.loader.exec_module(mod)
    return mod


cm = _load('copy_map_defs')
he = _load('pbt4b_hand_edits')
lc = _load('lists_core')

NINE = {'tests/RackCad.Tests/Workspace/%s.cs' % c: c for c in cm.NINE}
GUARDS = [he.WPG, he.IG, he.EIG, he.PEG, he.RG, he.WLG, he.WPUG]
UI = ['tests/RackCad.UI.Tests/Workspace/WorkspacePilotReconciliationTests.cs', 'tests/RackCad.UI.Tests/Workspace/WorkspacePilotReconciliationViewTests.cs']
KIT = ('EditIntentFakeReader', 'EditIntentKit', 'PrepareKit')
TEST_ATTR = re.compile(r'^[ \t]*\[(?:Fact|Theory)\b', re.M)
TEST_DECL = re.compile(r'\bpublic\s+(?:async\s+)?(?:void|Task)\s+(\w+)\s*\(')
TYPE_DECL = re.compile(r'\b(?:class|record|struct|interface|enum)\s+(\w+)')


def git_show(repo, rev, path):
    p = subprocess.run(['git', '-C', repo, 'show', '%s:%s' % (rev, path)], capture_output=True)
    if p.returncode:
        raise SystemExit('git show %s:%s -> %d' % (rev, path, p.returncode))
    raw = p.stdout.decode('utf-8')
    return raw.lstrip('﻿').replace('\r\n', '\n')


def _line_end(text, i):
    j = text.find('\n', i)
    return len(text) if j < 0 else j + 1


def segments(text, masked, start, end):
    """Los miembros (o tipos) entre start y end, al nivel de esa región: cada tramo va del final del anterior al final de la línea de su ';' o
    de su '}' de cierre (un '}' seguido de ';', '=', ',', ')' o '.' no cierra). Devuelve la lista de tramos y la cola."""
    out, depth, i, seg = [], 0, start, start
    while i < end:
        ch = masked[i]
        if ch == '{':
            depth += 1
        elif ch == '}':
            depth -= 1
            if depth == 0:
                j = i + 1
                while j < end and masked[j] in ' \t\r\n':
                    j += 1
                if j >= end or masked[j] not in ';=,).':
                    cut = min(_line_end(text, i), end)
                    out.append((seg, cut))
                    seg = i = cut
                    continue
        elif ch == ';' and depth == 0:
            cut = min(_line_end(text, i), end)
            out.append((seg, cut))
            seg = i = cut
            continue
        i += 1
    return out, (seg, end)


def parse(text):
    """El archivo en (prefijo, [tipos de primer nivel], sufijo, forma del espacio de nombres)."""
    masked = lc.mask(text)
    m = re.search(r'^namespace\s+([\w.]+)\s*;[ \t]*\n', masked, re.M)
    if m:
        start, end, kind = m.end(), len(text), 'file'
    else:
        m = re.search(r'^namespace\s+([\w.]+)\s*\n\{[ \t]*\n', masked, re.M)
        assert m, 'sin espacio de nombres'
        start = m.end()
        end = masked.rindex('}')
        kind = 'block'
    units, tail = segments(text, masked, start, end)
    return text[:start], [(s, e) for s, e in units], text[tail[0]:], kind, m.group(1), masked


def class_parts(text, masked, s, e):
    """Una clase: (cabecera hasta su '{', [miembros], cola desde su '}')."""
    m = re.search(r'\bclass\s+(\w+)', masked[s:e])
    brace = masked.index('{', s + m.end())
    depth, i = 0, brace
    while True:
        ch = masked[i]
        if ch == '{':
            depth += 1
        elif ch == '}':
            depth -= 1
            if depth == 0:
                break
        i += 1
    close = i
    first = _line_end(text, brace)
    members, tail = segments(text, masked, first, close)
    # la cola de la clase (comentarios o líneas en blanco tras el último miembro) va con el cierre
    return text[s:first], [text[a:b] for a, b in members], text[tail[0]:e], m.group(1)


def declared_name(member):
    """El nombre que declara un miembro que no es prueba: el tipo anidado, o el identificador ante el primer '(', '{', '=' o ';' de su
    declaración (sin sus atributos ni sus comentarios)."""
    masked = lc.mask(member)
    lines = [l for l in masked.split('\n') if l.strip() and not re.match(r'^\s*\[', l)]
    decl = ' '.join(lines)
    m = re.search(r'\b(?:class|record|struct|interface|enum)\s+(\w+)', decl)
    if m and not re.search(r'[(=]', decl[:m.start()]):
        return m.group(1)

    def ident_before(k):
        head = decl[:k].rstrip()
        if head.endswith('>'):
            depth, j = 0, len(head) - 1
            while j >= 0:
                depth += {'>': 1, '<': -1}.get(head[j], 0)
                if depth == 0:
                    break
                j -= 1
            head = head[:j].rstrip()
        mm = re.search(r'(\w+)$', head)
        return mm.group(1) if mm else None

    keywords = {'public', 'private', 'internal', 'protected', 'static', 'readonly', 'async', 'new', 'override', 'virtual', 'sealed', 'unsafe',
                'extern', 'const', 'partial', 'required', 'volatile', 'ref', 'return'}
    angle = paren = 0
    for k, ch in enumerate(decl):
        if ch == '<':
            angle += 1
        elif ch == '>':
            angle -= 1
        elif angle == 0:
            if ch == '(':
                prev = ident_before(k)
                if paren == 0 and prev and prev not in keywords:
                    return prev
                paren += 1
            elif ch == ')':
                paren -= 1
            elif paren == 0 and ch in '{=;':
                prev = ident_before(k)
                assert prev, member[:200]
                return prev
    raise AssertionError(member[:200])


def reached(tests_text, helpers):
    """Los ayudantes que alcanzan las pruebas, por nombre y de forma transitiva, en su orden."""
    keep, text, grew = set(), tests_text, True
    while grew:
        grew = False
        for i, (name, body) in enumerate(helpers):
            if i not in keep and re.search(r'\b' + re.escape(name) + r'\b', text):
                keep.add(i)
                text += '\n' + body
                grew = True
    return [body for i, (name, body) in enumerate(helpers) if i in keep]


def test_name(member):
    if not TEST_ATTR.search(member):
        return None
    m = TEST_DECL.search(member)
    assert m, member[:200]
    return m.group(1)


def replace_counted(text, old, new, times, what):
    n = text.count(old)
    if n != times:
        raise SystemExit('PARA (%s): %d coincidencias de %r, se esperaban %d' % (what, n, old[:120], times))
    return text.replace(old, new)


def load_lists():
    rows = [l.split('\t') for l in open(os.path.join(HERE, 'pbt4b-substitution-list-475.tsv'), encoding='utf-8').read().split('\n')[1:] if l]
    assert len(rows) == 475, len(rows)
    listed = {}
    for r in rows:
        listed.setdefault(r[1], set()).add(r[2])
    cmap = {}
    for l in open(os.path.join(HERE, 'pbt4b-copy-map-v2.tsv'), encoding='utf-8').read().split('\n')[1:]:
        if l:
            c = l.split('\t')
            cmap[('tests/RackCad.Tests/Workspace/%s.cs' % c[0], c[1])] = c[4]
    return listed, cmap


def build(repo, rev, partition=None):
    """Devuelve {ruta de la copia: texto} y {(archivo, método): texto final de la prueba} de los equivalentes y las guardas."""
    listed, cmap = load_lists()
    files, review = {}, {}
    for path in list(NINE) + GUARDS + UI:
        text = git_show(repo, rev, path)
        prefix, units, suffix, nskind, ns, masked = parse(text)
        base_name = os.path.basename(path)[:-3]
        stem = base_name[:-len('Tests')] if base_name.endswith('Tests') else base_name
        is_c0, is_ui = path in NINE, path in UI
        wanted = listed[path]
        kept_units, test_unit = [], None
        for s, e in units:
            u = text[s:e]
            tm = TYPE_DECL.search(masked[s:e])
            name = tm.group(1) if tm else None
            if is_c0 and name in KIT:
                continue
            if name == base_name:
                test_unit = (s, e)
            kept_units.append((s, e, name))
        assert test_unit, path
        header, members, tail, cls = class_parts(text, masked, *test_unit)
        assert cls == base_name
        helper_text, tests = [], []
        for mtext in members:
            n = test_name(mtext)
            if n is None:
                helper_text.append(mtext)
            else:
                tests.append((n, mtext))
        names = [n for n, _ in tests]
        if is_c0:
            assert set(names) == wanted, (path, sorted(set(names) ^ wanted)[:3])
        else:
            assert wanted <= set(names), (path, sorted(wanted - set(names)))
        # los ayudantes: M en C-0; las ediciones de ayudante en las guardas (cada una, las veces que dice en el conjunto de los ayudantes)
        helper_list = [(declared_name(h), cm.apply_map(h)[0] if is_c0 else h) for h in helper_text]
        if not is_c0 and not is_ui:
            for old, new, times in he.HELPER_EDITS[path]:
                found = sum(h.count(old) for _, h in helper_list)
                if found != times:
                    raise SystemExit('PARA (%s ayudantes): %d coincidencias de %r, se esperaban %d' % (base_name, found, old[:120], times))
                helper_list = [(n, h.replace(old, new)) for n, h in helper_list]
        # las pruebas
        out_tests = []
        for n, mtext in tests:
            if n not in wanted:
                continue
            t = mtext
            if is_c0:
                t = cm.apply_map(t)[0]
                if cmap[(path, n)] != 'mecanica':
                    hs, edits = he.HAND.get((path, n), ([], []))
                    for h in hs:
                        old, new = he.H[h]
                        assert old in t, (n, h)
                        t = t.replace(old, new)
                    for old, new, times in edits:
                        t = replace_counted(t, old, new, times, n)
                    review[(path, n)] = t
            elif not is_ui:
                for old, new, times in he.GUARD_EDITS[(path, n)]:
                    t = replace_counted(t, old, new, times, n)
                review[(path, n)] = t
            out_tests.append((n, t))
        assert len(out_tests) == len(wanted), path
        # las clases de la copia
        if partition is None:
            groups = [('Probe', [t for _, t in out_tests])]
        else:
            groups = []
            for label, key in (('Red', 'Red'), ('Green', 'Green')):
                chosen = [t for n, t in out_tests if partition[(path, n)] == key]
                if chosen:
                    groups.append((label, chosen))
        classes = []
        for label, chosen in groups:
            new_header = re.sub(r'\bclass\s+' + base_name + r'\b', 'class ' + stem + 'Full' + label + 'Tests', header, count=1)
            # en cada clase, solo los ayudantes que alcanzan sus pruebas (un campo privado sin uso sería un aviso del compilador)
            classes.append(new_header + ''.join(reached(''.join(chosen), helper_list)) + ''.join(chosen) + tail)
        body = ''
        for s, e, name in kept_units:
            if (s, e) == test_unit:
                body += ''.join(classes)
            else:
                u = text[s:e]
                body += cm.apply_map(u)[0] if is_c0 else u
        copy = prefix + body + suffix
        # el espacio de nombres
        copy = copy.replace('namespace %s' % ns, 'namespace %s.FullPanelCopies' % ns, 1)
        # M-00: cada tipo de primer nivel T que no es la clase de prueba ni del kit pasa a T + Full
        for s, e, name in kept_units:
            if name and name != base_name:
                copy = re.sub(r'\b' + re.escape(name) + r'\b', name + 'Full', copy)
        dest = os.path.join(os.path.dirname(path), stem + 'Full.cs').replace('\\', '/')
        files[dest] = copy
    return files, review


def main():
    args = sys.argv[1:]
    if len(args) < 3:
        raise SystemExit(__doc__)
    repo, rev, out = args[0], args[1], os.path.abspath(args[2])
    partition, review_path = None, None
    if '--partition' in args:
        p = args[args.index('--partition') + 1]
        partition = {}
        for l in open(p, encoding='utf-8').read().split('\n')[1:]:
            if l:
                c = l.split('\t')
                partition[(c[0], c[1])] = c[2]
    if '--review' in args:
        review_path = args[args.index('--review') + 1]
    if os.path.exists(os.path.join(out, '.git')):
        raise SystemExit('PARA: %s tiene .git' % out)
    sys.stdout.reconfigure(encoding='utf-8', errors='replace')
    files, review = build(repo, rev, partition)
    lines = []
    for rel in sorted(files):
        data = files[rel].encode('utf-8')
        dest = os.path.join(out, *rel.split('/'))
        os.makedirs(os.path.dirname(dest), exist_ok=True)
        with open(dest, 'wb') as f:
            f.write(data)
        h = hashlib.sha256(data).hexdigest()
        lines.append('%s  %s' % (h, rel))
        print('%s  %7d  %s' % (h, len(data), rel))
    print('conjunto', hashlib.sha256(('\n'.join(lines) + '\n').encode('utf-8')).hexdigest())
    if review_path:
        with open(review_path, 'w', encoding='utf-8', newline='\n') as f:
            for (p, n), t in sorted(review.items()):
                f.write('=' * 100 + '\n%s :: %s\n' % (p, n) + '=' * 100 + '\n' + t + '\n')
        print('revision', len(review), 'pruebas ->', review_path)


if __name__ == '__main__':
    main()
