"""PB-T4b (paquete v4.5): el guion de «Saltar y duplicar» para la lista de 477 (pbt4b-substitution-list-477.tsv). Es make_copies_pbt4b.py (v4.4,
que no se toca) con tres cambios, y nada más:
- las ediciones son las de pbt4b_hand_edits_v45.py (M-01 de la revisión y las dos de PB-T4a);
- las guardas incluyen las dos clases de PB-T4a de la lista de 477 (PushBackFullReadPathTests y PushBackFullReadGuardTests), con la misma regla que
  las demás: una copia parcial, sin M, con sus pruebas de la lista y sus ediciones, y los ayudantes que alcanzan;
- la lista es la de 477; la revisión, por defecto, el cierre de PB-T4a (78a7cef3), donde están esas dos clases.
Da 20 archivos de copia.

Uso: python -I make_copies_pbt4b_v45.py <checkout> <rev> <dir-salida> [--partition <pbt4b-copy-partition.tsv>] [--review <archivo.txt>]
Solo `git show` (objetos git)."""
import importlib.util
import os
import sys

HERE = os.path.dirname(os.path.abspath(__file__))


def _load(name, alias):
    spec = importlib.util.spec_from_file_location(alias, os.path.join(HERE, name + '.py'))
    mod = importlib.util.module_from_spec(spec)
    spec.loader.exec_module(mod)
    return mod


mc = _load('make_copies_pbt4b', 'make_copies_pbt4b_base')
he = _load('pbt4b_hand_edits_v45', 'pbt4b_hand_edits_v45')
LIST = 'pbt4b-substitution-list-477.tsv'
mc.he = he
mc.GUARDS = [he.WPG, he.IG, he.EIG, he.PEG, he.RG, he.WLG, he.WPUG, he.FRP, he.FRG]


def load_lists():
    rows = [l.split('\t') for l in open(os.path.join(HERE, LIST), encoding='utf-8').read().split('\n')[1:] if l]
    assert len(rows) == 477, len(rows)
    listed = {}
    for r in rows:
        listed.setdefault(r[1], set()).add(r[2])
    cmap = {}
    for l in open(os.path.join(HERE, 'pbt4b-copy-map-v2.tsv'), encoding='utf-8').read().split('\n')[1:]:
        if l:
            c = l.split('\t')
            cmap[('tests/RackCad.Tests/Workspace/%s.cs' % c[0], c[1])] = c[4]
    return listed, cmap


mc.load_lists = load_lists
build = mc.build

if __name__ == '__main__':
    mc.__doc__ = __doc__
    mc.main()
