"""PB-T4b (paquete v4.8): los `Skip` literales de la lista de 478 (pbt4b-substitution-list-478.tsv, owner-pbt4b-list-478-decision.md). Son los de
pbt4b_skips_v45.py (que no se toca) con tres cambios:
- la lista es la de 478 (las 477 y, al final, la guarda de los textos del anfitrión, en C-5);
- el mapa de la copia es el v5 (pbt4b-copy-map-v5.tsv: 356 mecánicas y 72 a mano), así que las dos de WPP de D-S1 llevan el motivo «sustituida
  por su equivalente escrito a mano en <Clase>Full»;
- la guarda nueva de C-5 lleva el motivo de C-5 con su decisión: «sustituida por su equivalente en WorkspaceHostHardeningGuardFull (registro
  declarado superseding-characterization.md; decision del Owner owner-pbt4b-list-478-decision.md)».
Mismas funciones: rows, hand_set, stem, reason, by_file y apply."""
import importlib.util
import os

HERE = os.path.dirname(os.path.abspath(__file__))
_spec = importlib.util.spec_from_file_location('pbt4b_skips_v45_base48', os.path.join(HERE, 'pbt4b_skips_v45.py'))
_v45 = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(_v45)
LIST = 'pbt4b-substitution-list-478.tsv'
MAP = 'pbt4b-copy-map-v5.tsv'
HAND_COUNT = 72
LIST478 = 'decision del Owner owner-pbt4b-list-478-decision.md'
REGISTRY, Q3, LIST2, BREAKS, LIST477 = _v45.REGISTRY, _v45.Q3, _v45.LIST2, _v45.BREAKS, _v45.LIST477


def rows():
    out = []
    for l in open(os.path.join(HERE, LIST), encoding='utf-8').read().split('\n')[1:]:
        if l:
            out.append(l.split('\t'))
    assert len(out) == 478, len(out)
    return out


def hand_set():
    s = set()
    for l in open(os.path.join(HERE, MAP), encoding='utf-8').read().split('\n')[1:]:
        if l:
            c = l.split('\t')
            if c[4] != 'mecanica':
                s.add(('tests/RackCad.Tests/Workspace/%s.cs' % c[0], c[1]))
    assert len(s) == HAND_COUNT, len(s)
    return s


_reason_v45 = _v45.reason


def reason(row, hand):
    lista, path, method, _, cause = row
    if lista == 'PB-T4b C-5' and 'owner-pbt4b-list-478-decision.md' in cause:
        return 'I-64 F4B PB-T4b: sustituida por su equivalente en %sFull (%s; %s).' % (_v45.stem(path), REGISTRY, LIST478)
    return _reason_v45(row, hand)


_v45.rows, _v45.hand_set, _v45.reason = rows, hand_set, reason
stem, by_file, apply = _v45.stem, _v45.by_file, _v45.apply
T4A_FILE, T4A_MARK, CT06_NAME = _v45.T4A_FILE, _v45.T4A_MARK, _v45.CT06_NAME
