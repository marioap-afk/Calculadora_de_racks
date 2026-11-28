"""PB-T4b (paquete v4.8): el guion de «Saltar y duplicar» para la lista de 478. Es el de la v4.7 (make_copies_pbt4b_v47.py, que no se toca, con
MU, las ediciones de ayudante de C-0, los miembros nuevos de C-0 y las `Theory` partidas por filas de D-S5), con:
- **D-S6** (session-pbt4b-design-decisions.md): cada copia queda en el espacio de nombres raíz de su ensamblado (`RackCad.Tests` o
  `RackCad.UI.Tests`), como el resto de su proyecto (lo exige `NamespaceFolderGuardTests`), y sus clases llevan el prefijo `FullPanelCopy`:
  `FullPanelCopy<Raíz>ProbeTests`, `FullPanelCopy<Raíz>RedTests` y `FullPanelCopy<Raíz>GreenTests` (<Raíz> es la clase original sin `Tests`).
  Ningún filtro heredado las selecciona (`RackCad.Tests.Workspace` no es parte de `RackCad.Tests.FullPanelCopy…`);
- **la lista de 478** (owner-pbt4b-list-478-decision.md): la guarda de los textos del anfitrión entra en las guardas de C-5, con su copia
  `WorkspaceHostHardeningGuardFull.cs` (21 copias);
- **el mapa v5** (pbt4b-copy-map-v5.tsv: 356 mecánicas y 72 a mano) y las ediciones de pbt4b_hand_edits_v48.py (D-S1 en las dos de WPP y la tabla
  de la guarda nueva).
Nada más cambia: las pruebas no cambian de nombre, los archivos tampoco (`<Clase>Full.cs`) y M-00 sigue renombrando los tipos de primer nivel.

Uso: python -I make_copies_pbt4b_v48.py <checkout> <rev> <dir-salida> [--partition <pbt4b-copy-partition.tsv>] [--review <archivo.txt>]
La partición se lee con load_partition (una prueba partida tiene dos filas). Solo `git show` (objetos git)."""
import hashlib
import importlib.util
import os
import re
import sys

HERE = os.path.dirname(os.path.abspath(__file__))
EDITS = 'pbt4b_hand_edits_v48'
MAP = 'pbt4b-copy-map-v5.tsv'
LIST = 'pbt4b-substitution-list-478.tsv'
PREFIX = 'FullPanelCopy'


def _load(name, alias):
    spec = importlib.util.spec_from_file_location(alias, os.path.join(HERE, name + '.py'))
    mod = importlib.util.module_from_spec(spec)
    spec.loader.exec_module(mod)
    return mod


v47 = _load('make_copies_pbt4b_v47', 'make_copies_pbt4b_v47_base48')
v46 = v47.v46
mc, cm, lc = v46.mc, v46.cm, v46.lc
he = _load(EDITS, EDITS)
v46.MAP, v46.he, v47.he = MAP, he, he
USING_DRAFT = v46.USING_DRAFT
load_partition, split_variant = v47.load_partition, v47.split_variant


def class_name(stem, label):
    return PREFIX + stem + label + 'Tests'


def load_lists():
    rows = [l.split('\t') for l in open(os.path.join(HERE, LIST), encoding='utf-8').read().split('\n')[1:] if l]
    assert len(rows) == 478, len(rows)
    listed = {}
    for r in rows:
        listed.setdefault(r[1], set()).add(r[2])
    cmap = {}
    for l in open(os.path.join(HERE, MAP), encoding='utf-8').read().split('\n')[1:]:
        if l:
            c = l.split('\t')
            cmap[('tests/RackCad.Tests/Workspace/%s.cs' % c[0], c[1])] = c[4]
    return listed, cmap


def build(repo, rev, partition=None):
    """Como el de la v4.7, con D-S6 (el espacio raíz y el prefijo), la lista de 478 (la guarda nueva) y el mapa v5."""
    listed, cmap = load_lists()
    types = v46.draft_types(repo, rev)
    guards = [he.WPG, he.IG, he.EIG, he.PEG, he.RG, he.WLG, he.WPUG, he.FRP, he.FRG, he.WHHG]
    files, review = {}, {}
    if partition is not None:
        split_measured = {k for k, d in partition.items() if set(d) == {'Red', 'Green'}}
        assert split_measured == set(he.SPLIT), ('las pruebas partidas de la particion no son las de SPLIT', sorted(split_measured ^ set(he.SPLIT)))
    sources = list(mc.NINE) + guards + mc.UI
    assert set(sources) == set(listed) - {'(PB-T4a) ' + he.FRG}, sorted(set(sources) ^ set(listed))
    for path in sources:
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
            for n, h in helper_list:
                if any(new in h for _, new, _ in he.HELPER_EDITS[path]):
                    review[(path, 'ayudante ' + (n or '?'))] = h
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
                else:
                    assert (path, n) not in he.HAND, ('una prueba con edicion a mano es mecanica en el mapa', n)
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
            new_header = re.sub(r'\bclass\s+' + base_name + r'\b', 'class ' + class_name(stem, label), header, count=1)
            assert new_header != header, (path, 'la declaracion de la clase')
            classes.append(new_header + ''.join(mc.reached(''.join(chosen), helper_list)) + ''.join(chosen) + tail)
        body = ''
        for s, e, name in kept_units:
            if (s, e) == test_unit:
                body += ''.join(classes)
            else:
                u = text[s:e]
                body += cm.apply_map(u)[0] if is_c0 else u
        copy = prefix + body + suffix
        # D-S6: el espacio de nombres es el raíz del ensamblado (el del original), sin `.FullPanelCopies`
        assert ('namespace %s' % ns) in copy and ns in ('RackCad.Tests', 'RackCad.UI.Tests'), (path, ns)
        for s, e, name in kept_units:
            if name and name != base_name:
                copy = re.sub(r'\b' + re.escape(name) + r'\b', name + 'Full', copy)
        copy, _added = v46.add_using(copy, types)
        assert 'FullPanelCopies' not in copy, path
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
