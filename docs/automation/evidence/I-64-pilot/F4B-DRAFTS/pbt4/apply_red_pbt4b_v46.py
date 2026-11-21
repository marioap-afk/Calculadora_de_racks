"""PB-T4b (paquete v4.6): escribe en una COPIA EXPORTADA el RED exacto que prescribe el paquete, para la precondición 6. Es apply_red_pbt4b_v45.py
(que no se toca), con los `Skip` de pbt4b_skips_v46.py (el mapa de la copia v3) y las copias de make_copies_pbt4b_v46.py; el esqueleto (de la v4.4)
y el kit (de la v4.5) no cambian.

Uso:
    python -I apply_red_pbt4b_v46.py <dir-copia> [--repo <checkout>] [--rev <rev>] [--partition <pbt4b-copy-partition.tsv>] [--dry]
"""
import importlib.util
import os

HERE = os.path.dirname(os.path.abspath(__file__))
_spec = importlib.util.spec_from_file_location('apply_red_pbt4b_v45_base', os.path.join(HERE, 'apply_red_pbt4b_v45.py'))
base = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(base)
_RENAME = {'pbt4b_skips_v45': 'pbt4b_skips_v46', 'make_copies_pbt4b_v45': 'make_copies_pbt4b_v46'}
_load45 = base.load


def load(name):
    return _load45(_RENAME.get(name, name))


base.load = load

if __name__ == '__main__':
    base.__doc__ = __doc__
    base.main()
