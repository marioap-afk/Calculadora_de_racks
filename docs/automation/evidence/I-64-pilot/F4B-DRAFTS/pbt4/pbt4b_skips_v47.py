"""PB-T4b (paquete v4.7): los `Skip` literales de la lista de 477. Son los de pbt4b_skips_v45.py (que no se toca), con el mapa de la copia v4
(pbt4b-copy-map-v4.tsv: 70 a mano), así que las dos `Theory` de C-0 que D-S5 parte por filas llevan el motivo «sustituida por su equivalente escrito
a mano en <Clase>Full». Mismas funciones: rows, hand_set, stem, reason, by_file y apply."""
import importlib.util
import os

HERE = os.path.dirname(os.path.abspath(__file__))
_spec = importlib.util.spec_from_file_location('pbt4b_skips_v45_base47', os.path.join(HERE, 'pbt4b_skips_v45.py'))
_v45 = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(_v45)
MAP = 'pbt4b-copy-map-v4.tsv'
HAND_COUNT = 70


def hand_set():
    s = set()
    for l in open(os.path.join(HERE, MAP), encoding='utf-8').read().split('\n')[1:]:
        if l:
            c = l.split('\t')
            if c[4] != 'mecanica':
                s.add(('tests/RackCad.Tests/Workspace/%s.cs' % c[0], c[1]))
    assert len(s) == HAND_COUNT, len(s)
    return s


_v45.hand_set = hand_set
rows, stem, reason, by_file, apply = _v45.rows, _v45.stem, _v45.reason, _v45.by_file, _v45.apply
T4A_FILE, T4A_MARK, CT06_NAME = _v45.T4A_FILE, _v45.T4A_MARK, _v45.CT06_NAME
