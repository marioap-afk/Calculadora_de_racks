"""PB-T4b (paquete v4.9): el guion de «Saltar y duplicar». Es el de la v4.8 (make_copies_pbt4b_v48.py, que no se toca: D-S6, la lista de 478 y el
mapa v5) con las ediciones de pbt4b_hand_edits_v49.py, cuyo único cambio es CA-T4b I-01: el equivalente de WHHG `NoneOfThe30…` trae la autoprueba de
la guarda sobre su tabla. Solo cambia `WorkspaceHostHardeningGuardFull.cs`; las otras 20 copias son byte a byte las de la v4.8.

Uso: python -I make_copies_pbt4b_v49.py <checkout> <rev> <dir-salida> [--partition <pbt4b-copy-partition.tsv>] [--review <archivo.txt>]
Solo `git show` (objetos git)."""
import importlib.util
import os

HERE = os.path.dirname(os.path.abspath(__file__))
EDITS = 'pbt4b_hand_edits_v49'


def _load(name, alias):
    spec = importlib.util.spec_from_file_location(alias, os.path.join(HERE, name + '.py'))
    mod = importlib.util.module_from_spec(spec)
    spec.loader.exec_module(mod)
    return mod


v48 = _load('make_copies_pbt4b_v48', 'make_copies_pbt4b_v48_base49')
he = _load(EDITS, EDITS)
v48.he, v48.v46.he, v48.v47.he = he, he, he
mc, cm, lc, v46, v47 = v48.mc, v48.cm, v48.lc, v48.v46, v48.v47
USING_DRAFT, PREFIX = v48.USING_DRAFT, v48.PREFIX
load_partition, split_variant, class_name, load_lists = v48.load_partition, v48.split_variant, v48.class_name, v48.load_lists
build, main = v48.build, v48.main

if __name__ == '__main__':
    main()
