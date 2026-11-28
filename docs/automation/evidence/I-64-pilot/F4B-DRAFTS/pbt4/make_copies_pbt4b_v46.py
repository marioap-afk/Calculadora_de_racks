"""PB-T4b (paquete v4.6): el guion de «Saltar y duplicar» para la lista de 477. Usa las piezas de make_copies_pbt4b.py (v4.4, que no se toca: el
análisis del archivo, los miembros, los ayudantes que alcanzan, M-00) y su regla, con tres cambios respecto de la v4.5 (make_copies_pbt4b_v45.py):
- las ediciones son las de pbt4b_hand_edits_v46.py y el mapa, el v3 (pbt4b-copy-map-v3.tsv: 360 mecánicas y 68 a mano);
- **las ediciones de ayudante en las copias de C-0** (C0_HELPER_EDITS; D-S1): sobre el texto de los ayudantes ya con M, cada una las veces que dice en
  el conjunto de los ayudantes de su archivo; el original no se edita; la revisión lleva el ayudante editado;
- **MU** (la regla nueva del mapa; no es M-09 de la §19 de la revisión): cada copia que nombra un tipo de `Draft/` (un tipo declarado en src/RackCad.Application/Systems/PushBack/Draft/, fuera de cadenas y
  comentarios) y no lleva `using RackCad.Application.Systems.PushBack.Draft;` gana esa directiva tras el último `using` de su cabecera.
Da 20 archivos de copia. La configuración (EDITS y MAP) la puede cambiar un guion posterior que lo cargue, sin tocarlo.

Uso: python -I make_copies_pbt4b_v46.py <checkout> <rev> <dir-salida> [--partition <pbt4b-copy-partition.tsv>] [--review <archivo.txt>]
Solo `git show` (objetos git)."""
import hashlib
import importlib.util
import os
import re
import subprocess
import sys

HERE = os.path.dirname(os.path.abspath(__file__))
EDITS = 'pbt4b_hand_edits_v46'
MAP = 'pbt4b-copy-map-v3.tsv'
LIST = 'pbt4b-substitution-list-477.tsv'
USING_DRAFT = 'using RackCad.Application.Systems.PushBack.Draft;'
DRAFT_DIR = 'src/RackCad.Application/Systems/PushBack/Draft/'


def _load(name, alias):
    spec = importlib.util.spec_from_file_location(alias, os.path.join(HERE, name + '.py'))
    mod = importlib.util.module_from_spec(spec)
    spec.loader.exec_module(mod)
    return mod


mc = _load('make_copies_pbt4b', 'make_copies_pbt4b_base')
cm, lc = mc.cm, mc.lc
he = None


def load_edits():
    global he
    he = _load(EDITS, EDITS)
    return he


def load_lists():
    rows = [l.split('\t') for l in open(os.path.join(HERE, LIST), encoding='utf-8').read().split('\n')[1:] if l]
    assert len(rows) == 477, len(rows)
    listed = {}
    for r in rows:
        listed.setdefault(r[1], set()).add(r[2])
    cmap = {}
    for l in open(os.path.join(HERE, MAP), encoding='utf-8').read().split('\n')[1:]:
        if l:
            c = l.split('\t')
            cmap[('tests/RackCad.Tests/Workspace/%s.cs' % c[0], c[1])] = c[4]
    return listed, cmap


def draft_types(repo, rev):
    """Los tipos de primer nivel declarados en Draft/ en la revisión (con los cuatro archivos nuevos del esqueleto y del GREEN de PB-T4b)."""
    p = subprocess.run(['git', '-C', repo, 'ls-tree', '--name-only', rev, DRAFT_DIR], capture_output=True)
    names = set()
    for path in p.stdout.decode('utf-8').split('\n'):
        if path.endswith('.cs'):
            names.update(re.findall(r'\b(?:class|struct|enum|interface|record)\s+(\w+)', lc.mask(mc.git_show(repo, rev, path))))
    names.update({'PushBackAnnotationScaleRule', 'PushBackFullFieldAdapter', 'PushBackKeepMinePresentation'})
    return names


def add_using(text, types):
    """MU: la directiva de Draft/ tras el último `using` de la cabecera, si el código nombra un tipo de Draft/ y no la lleva."""
    if USING_DRAFT in text:
        return text, False
    masked = lc.mask(text)
    if not any(re.search(r'\b' + re.escape(t) + r'\b', masked) for t in types):
        return text, False
    lines = text.split('\n')
    head = next(i for i, l in enumerate(lines) if l.startswith('namespace '))
    last = max(i for i in range(head) if lines[i].startswith('using '))
    lines.insert(last + 1, USING_DRAFT)
    return '\n'.join(lines), True


def build(repo, rev, partition=None):
    """Devuelve {ruta de la copia: texto} y {(archivo, método o ayudante): texto final} de los equivalentes, las guardas y los ayudantes editados."""
    if he is None:
        load_edits()
    listed, cmap = load_lists()
    types = draft_types(repo, rev)
    guards = [he.WPG, he.IG, he.EIG, he.PEG, he.RG, he.WLG, he.WPUG, he.FRP, he.FRG]
    files, review = {}, {}
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
            # las ediciones de ayudante de C-0 (D-S1), sobre el texto con M: solo en la copia
            for old, new, times in getattr(he, 'C0_HELPER_EDITS', {}).get(path, []):
                found = sum(h.count(old) for _, h in helper_list)
                if found != times:
                    raise SystemExit('PARA (%s ayudantes de C-0): %d coincidencias de %r, se esperaban %d' % (base_name, found, old[:120], times))
                helper_list = [(n, h.replace(old, new)) for n, h in helper_list]
            # el ayudante editado puede ser un miembro de un tipo anidado o de otro tipo del archivo: también en las unidades de primer nivel
        elif not is_ui:
            for old, new, times in he.HELPER_EDITS[path]:
                found = sum(h.count(old) for _, h in helper_list)
                if found != times:
                    raise SystemExit('PARA (%s ayudantes): %d coincidencias de %r, se esperaban %d' % (base_name, found, old[:120], times))
                helper_list = [(n, h.replace(old, new)) for n, h in helper_list]
        if is_c0:
            for n, h in helper_list:
                if any(new in h for _, new, _ in getattr(he, 'C0_HELPER_EDITS', {}).get(path, [])):
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
            elif not is_ui:
                for old, new, times in he.GUARD_EDITS[(path, n)]:
                    t = mc.replace_counted(t, old, new, times, n)
                review[(path, n)] = t
            out_tests.append((n, t))
        assert len(out_tests) == len(wanted), path
        if partition is None:
            groups = [('Probe', [t for _, t in out_tests])]
        else:
            groups = []
            for label in ('Red', 'Green'):
                chosen = [t for n, t in out_tests if partition[(path, n)] == label]
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
        copy, _added = add_using(copy, types)
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
        partition = {}
        for l in open(args[args.index('--partition') + 1], encoding='utf-8').read().split('\n')[1:]:
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
        print('%s  %7d  %s%s' % (h, len(data), rel, '  (+using Draft)' if USING_DRAFT in files[rel] else ''))
    print('conjunto', hashlib.sha256(('\n'.join(lines) + '\n').encode('utf-8')).hexdigest())
    if review_path:
        with open(review_path, 'w', encoding='utf-8', newline='\n') as f:
            for (p, n), t in sorted(review.items()):
                f.write('=' * 100 + '\n%s :: %s\n' % (p, n) + '=' * 100 + '\n' + t + '\n')
        print('revision', len(review), 'entradas ->', review_path)


if __name__ == '__main__':
    main()
