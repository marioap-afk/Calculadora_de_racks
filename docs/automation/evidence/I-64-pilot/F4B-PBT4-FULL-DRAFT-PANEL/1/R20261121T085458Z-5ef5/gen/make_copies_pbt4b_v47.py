"""PB-T4b (paquete v4.7): el guion de «Saltar y duplicar» para la lista de 477. Es el de la v4.6 (make_copies_pbt4b_v46.py, que no se toca, con MU y
las ediciones de ayudante de C-0), con D-S5 y el mapa v4 (pbt4b-copy-map-v4.tsv: 358 mecánicas y 70 a mano):
- **los miembros nuevos de C-0** (C0_NEW_HELPERS de pbt4b_hand_edits_v47.py): solo en la copia, tras el ayudante que dicen; cada clase de la copia
  lleva los que alcanzan sus pruebas;
- **las `Theory` partidas por filas** (SPLIT): en el modo de medida van enteras, en la clase Probe; con la partición, su texto va DOS veces, en
  <Clase>FullRedTests con las filas que fallan en el RED y en <Clase>FullGreenTests con las que pasan, con el mismo nombre, el mismo cuerpo y las
  mismas aserciones; una línea de comentario dice a qué mitad pertenece. La partición tiene que dar las dos filas (Red y Green) para cada una, y solo
  para ellas; si no, el guion para.

Uso: python -I make_copies_pbt4b_v47.py <checkout> <rev> <dir-salida> [--partition <pbt4b-copy-partition.tsv>] [--review <archivo.txt>]
La partición se lee con load_partition (una prueba partida tiene dos filas). Solo `git show` (objetos git)."""
import hashlib
import importlib.util
import os
import re
import sys

HERE = os.path.dirname(os.path.abspath(__file__))
EDITS = 'pbt4b_hand_edits_v47'
MAP = 'pbt4b-copy-map-v4.tsv'


def _load(name, alias):
    spec = importlib.util.spec_from_file_location(alias, os.path.join(HERE, name + '.py'))
    mod = importlib.util.module_from_spec(spec)
    spec.loader.exec_module(mod)
    return mod


v46 = _load('make_copies_pbt4b_v46', 'make_copies_pbt4b_v46_base')
mc, cm, lc = v46.mc, v46.cm, v46.lc
v46.MAP = MAP
he = _load(EDITS, EDITS)
v46.he = he
USING_DRAFT = v46.USING_DRAFT
LIT = re.compile(r'^\s*\[InlineData\(\s*(@"(?:[^"]|"")*"|"(?:\\.|[^"\\])*")')


def load_partition(path):
    """{(archivo, método): {etiqueta: resultados}}; una prueba partida por filas tiene Red y Green."""
    out = {}
    for l in open(path, encoding='utf-8').read().split('\n')[1:]:
        if l:
            c = l.split('\t')
            d = out.setdefault((c[0], c[1]), {})
            assert c[2] not in d, ('fila repetida', c)
            d[c[2]] = int(c[3])
    return out


def _decode(lit):
    if lit.startswith('@"'):
        return lit[2:-1].replace('""', '"')
    return re.sub(r'\\(.)', lambda m: {'n': '\n', 't': '\t', 'r': '\r', '0': '\0'}.get(m.group(1), m.group(1)), lit[1:-1])


def split_variant(t, spec, half):
    """El texto de una `Theory` partida, para la mitad `half` (Red o Green)."""
    lines = t.split('\n')
    decl = next(i for i, l in enumerate(lines) if re.search(r'\bpublic\s+(?:async\s+)?(?:void|Task)\s+\w+\s*\(', l))
    theory = next(i for i in range(decl) if re.match(r'^\s*\[Theory\]\s*$', lines[i]))
    indent = re.match(r'^(\s*)', lines[theory]).group(1)
    if spec[0] == 'inline':
        red = set(spec[1])
        keep, n_red, n_all = [], 0, 0
        for i, l in enumerate(lines):
            m = LIT.match(l) if i < decl else None
            if m:
                n_all += 1
                is_red = _decode(m.group(1)) in red
                n_red += 1 if is_red else 0
                if is_red != (half == 'Red'):
                    continue
            keep.append(l)
        assert n_red == len(red) and n_all > n_red, ('las filas Red no casan', n_red, len(red), n_all)
        lines = keep
    else:
        _, orig, red_m, green_m = spec
        old = '[MemberData(nameof(%s))]' % orig
        hits = [i for i in range(decl) if lines[i].strip() == old]
        assert len(hits) == 1, ('MemberData', hits)
        lines[hits[0]] = lines[hits[0]].replace(old, '[MemberData(nameof(%s))]' % (red_m if half == 'Red' else green_m))
    theory = next(i for i in range(len(lines)) if re.match(r'^\s*\[Theory\]\s*$', lines[i]))
    lines.insert(theory, indent + he.SPLIT_COMMENT % ('fail' if half == 'Red' else 'pass'))
    return '\n'.join(lines)


def build(repo, rev, partition=None):
    """Como el de la v4.6, con los miembros nuevos de C-0 y las `Theory` partidas por filas. `partition`: la de load_partition, o None."""
    listed, cmap = v46.load_lists()
    types = v46.draft_types(repo, rev)
    guards = [he.WPG, he.IG, he.EIG, he.PEG, he.RG, he.WLG, he.WPUG, he.FRP, he.FRG]
    files, review = {}, {}
    if partition is not None:
        split_measured = {k for k, d in partition.items() if set(d) == {'Red', 'Green'}}
        assert split_measured == set(he.SPLIT), ('las pruebas partidas de la particion no son las de SPLIT', sorted(split_measured ^ set(he.SPLIT)))
    for path in list(mc.NINE) + guards + mc.UI:
        text = mc.git_show(repo, rev, path)
        prefix, units, suffix, nskind, ns, masked = mc.parse(text)
        base_name = os.path.basename(path)[:-3]
        stem = base_name[:-len('Tests')] if base_name.endswith('Tests') else base_name
        is_c0, is_ui = path in mc.NINE, path in mc.UI
        wanted = listed[path]
        kept_units, test_unit = [], None
        for s, e in units:
            tm = mc.TYPE_DECL.search(masked[s:e])
            name = tm.group(1) if tm else None
            if is_c0 and name in mc.KIT:
                continue
            if name == base_name:
                test_unit = (s, e)
            kept_units.append((s, e, name))
        assert test_unit, path
        header, members, tail, cls = mc.class_parts(text, masked, *test_unit)
        assert cls == base_name
        helper_text, tests = [], []
        for mtext in members:
            n = mc.test_name(mtext)
            if n is None:
                helper_text.append(mtext)
            else:
                tests.append((n, mtext))
        names = [n for n, _ in tests]
        if is_c0:
            assert set(names) == wanted, (path, sorted(set(names) ^ wanted)[:3])
        else:
            assert wanted <= set(names), (path, sorted(wanted - set(names)))
        helper_list = [(mc.declared_name(h), cm.apply_map(h)[0] if is_c0 else h) for h in helper_text]
        if is_c0:
            for old, new, times in he.C0_HELPER_EDITS.get(path, []):
                found = sum(h.count(old) for _, h in helper_list)
                if found != times:
                    raise SystemExit('PARA (%s ayudantes de C-0): %d coincidencias de %r, se esperaban %d' % (base_name, found, old[:120], times))
                helper_list = [(n, h.replace(old, new)) for n, h in helper_list]
            for after, new_name, new_text in he.C0_NEW_HELPERS.get(path, []):
                at = [i for i, (n, _) in enumerate(helper_list) if n == after]
                assert len(at) == 1, ('el ayudante tras el que va el nuevo', after, at)
                assert all(n != new_name for n, _ in helper_list), new_name
                helper_list.insert(at[0] + 1, (new_name, new_text))
                review[(path, 'ayudante nuevo ' + new_name)] = new_text
            for n, h in helper_list:
                if any(new in h for _, new, _ in he.C0_HELPER_EDITS.get(path, [])):
                    review[(path, 'ayudante ' + (n or '?'))] = h
        elif not is_ui:
            for old, new, times in he.HELPER_EDITS[path]:
                found = sum(h.count(old) for _, h in helper_list)
                if found != times:
                    raise SystemExit('PARA (%s ayudantes): %d coincidencias de %r, se esperaban %d' % (base_name, found, old[:120], times))
                helper_list = [(n, h.replace(old, new)) for n, h in helper_list]
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
                        t = mc.replace_counted(t, old, new, times, n)
                    review[(path, n)] = t
            elif not is_ui:
                for old, new, times in he.GUARD_EDITS[(path, n)]:
                    t = mc.replace_counted(t, old, new, times, n)
                review[(path, n)] = t
            if (path, n) in he.SPLIT:
                assert not is_c0 or cmap[(path, n)] != 'mecanica', ('una Theory partida es a mano en el mapa', n)
                halves = {half: split_variant(t, he.SPLIT[(path, n)], half) for half in ('Red', 'Green')}
                for half, ht in halves.items():
                    review[(path, '%s (%s, D-S5)' % (n, half))] = ht
                out_tests.append((n, t, halves))
            else:
                out_tests.append((n, t, None))
        assert len(out_tests) == len(wanted), path
        if partition is None:
            groups = [('Probe', [t for _, t, _ in out_tests])]
        else:
            groups = []
            for label in ('Red', 'Green'):
                chosen = []
                for n, t, halves in out_tests:
                    labels = set(partition[(path, n)])
                    if halves is not None:
                        chosen.append(halves[label])
                    elif labels == {label}:
                        chosen.append(t)
                    else:
                        assert len(labels) == 1, ('una prueba no declarada en SPLIT con dos etiquetas', n, labels)
                if chosen:
                    groups.append((label, chosen))
        classes = []
        for label, chosen in groups:
            new_header = re.sub(r'\bclass\s+' + base_name + r'\b', 'class ' + stem + 'Full' + label + 'Tests', header, count=1)
            classes.append(new_header + ''.join(mc.reached(''.join(chosen), helper_list)) + ''.join(chosen) + tail)
        body = ''
        for s, e, name in kept_units:
            if (s, e) == test_unit:
                body += ''.join(classes)
            else:
                u = text[s:e]
                body += cm.apply_map(u)[0] if is_c0 else u
        copy = prefix + body + suffix
        copy = copy.replace('namespace %s' % ns, 'namespace %s.FullPanelCopies' % ns, 1)
        for s, e, name in kept_units:
            if name and name != base_name:
                copy = re.sub(r'\b' + re.escape(name) + r'\b', name + 'Full', copy)
        copy, _added = v46.add_using(copy, types)
        dest = os.path.join(os.path.dirname(path), stem + 'Full.cs').replace('\\', '/')
        files[dest] = copy
    return files, review


def main():
    args = sys.argv[1:]
    if len(args) < 3:
        raise SystemExit(__doc__)
    repo, rev, out = args[0], args[1], os.path.abspath(args[2])
    partition = load_partition(args[args.index('--partition') + 1]) if '--partition' in args else None
    review_path = args[args.index('--review') + 1] if '--review' in args else None
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
        print('%s  %7d  %s%s' % (h, len(data), rel, '  (+using Draft)' if USING_DRAFT in files[rel] else ''))
    print('conjunto', hashlib.sha256(('\n'.join(lines) + '\n').encode('utf-8')).hexdigest())
    if review_path:
        with open(review_path, 'w', encoding='utf-8', newline='\n') as f:
            for (p, n), t in sorted(review.items()):
                f.write('=' * 100 + '\n%s :: %s\n' % (p, n) + '=' * 100 + '\n' + t + '\n')
        print('revision', len(review), 'entradas ->', review_path)


if __name__ == '__main__':
    main()
