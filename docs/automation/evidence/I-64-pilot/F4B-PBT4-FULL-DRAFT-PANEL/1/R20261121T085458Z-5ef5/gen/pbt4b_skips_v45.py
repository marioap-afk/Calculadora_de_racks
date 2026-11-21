"""PB-T4b (paquete v4.5): los `Skip` literales de la lista de 477 (pbt4b-substitution-list-477.tsv) y su aplicación, para el RED y para la
comprobación mecánica (check_skips_pbt4b_v45.py y CT-PBT4B-20). Todo en ASCII. Es pbt4b_skips.py (v4.4, que no se toca) con la lista de 477 y
el motivo de las dos de PB-T4a:
- C-0, copia mecánica: «sustituida por su copia en <Clase>Full»;
- C-0, a mano (los 60 del mapa de la copia v2): «sustituida por su equivalente escrito a mano en <Clase>Full»;
- C-5 de la v4.3: «sustituida por su equivalente en <Clase>Full», salvo CT-PBT4A-06: «sustituida por CT-PBT4B-13 de PushBackFullPanelCensusGuardTests»;
- C-5 de las cinco de la tercera tanda: lo mismo, con la decisión owner-pbt4b-breaks-decision.md;
- C-5 de las dos de PB-T4a (CT-PBT4A-03 y CT-PBT4A-05): lo mismo, con la decisión owner-pbt4b-list-477-decision.md;
- la interfaz (UI): «sustituida por su copia en <Clase>Full».
<Clase> es el nombre de la clase original sin el sufijo Tests. CT-PBT4A-06 se llama, en el cierre de PB-T4a (78a7cef3),
CT_PBT4A_06_OnlyTheFolderOfTheModelThePlanTheRunAndTheAdapterOfThePluginNameItsTypes; su fila de la lista la nombra por su prefijo.
"""
import os
import re

HERE = os.path.dirname(os.path.abspath(__file__))
REGISTRY = 'registro declarado superseding-characterization.md'
Q3 = 'decision del Owner owner-pbt4-decisions.md, pregunta 3'
LIST2 = 'decision del Owner owner-pbt4b-retire-list-decisions.md, pregunta 2'
BREAKS = 'decision del Owner owner-pbt4b-breaks-decision.md'
LIST477 = 'decision del Owner owner-pbt4b-list-477-decision.md'
T4A_FILE = 'tests/RackCad.Tests/PushBackFullReadGuardTests.cs'
T4A_MARK = '(PB-T4a) '
CT06_NAME = 'CT_PBT4A_06_OnlyTheFolderOfTheModelThePlanTheRunAndTheAdapterOfThePluginNameItsTypes'


def rows():
    out = []
    for l in open(os.path.join(HERE, 'pbt4b-substitution-list-477.tsv'), encoding='utf-8').read().split('\n')[1:]:
        if l:
            out.append(l.split('\t'))
    assert len(out) == 477
    return out


def hand_set():
    s = set()
    for l in open(os.path.join(HERE, 'pbt4b-copy-map-v2.tsv'), encoding='utf-8').read().split('\n')[1:]:
        if l:
            c = l.split('\t')
            if c[4] != 'mecanica':
                s.add(('tests/RackCad.Tests/Workspace/%s.cs' % c[0], c[1]))
    assert len(s) == 60
    return s


def stem(path):
    name = os.path.basename(path)[:-3]
    return name[:-len('Tests')] if name.endswith('Tests') else name


def reason(row, hand):
    lista, path, method, _, cause = row
    if path.startswith(T4A_MARK):
        return 'I-64 F4B PB-T4b: sustituida por CT-PBT4B-13 de PushBackFullPanelCensusGuardTests (%s; %s).' % (REGISTRY, LIST2)
    if lista == 'PB-T4b C-0':
        what = 'su equivalente escrito a mano en' if (path, method) in hand else 'su copia en'
        return 'I-64 F4B PB-T4b: sustituida por %s %sFull (%s; %s).' % (what, stem(path), REGISTRY, Q3)
    if lista == 'PB-T4b C-5':
        if 'owner-pbt4b-list-477-decision.md' in cause:
            decision = LIST477
        elif 'owner-pbt4b-breaks-decision.md' in cause:
            decision = BREAKS
        else:
            decision = LIST2
        return 'I-64 F4B PB-T4b: sustituida por su equivalente en %sFull (%s; %s).' % (stem(path), REGISTRY, decision)
    assert lista == 'PB-T4b UI', lista
    return 'I-64 F4B PB-T4b: sustituida por su copia en %sFull (%s; %s).' % (stem(path), REGISTRY, LIST2)


def by_file():
    """{archivo: {método: motivo}}; la fila de CT-PBT4A-06 va con el archivo de PB-T4a y el método «CT_PBT4A_06*», que se resuelve al aplicarla."""
    hand = hand_set()
    out = {}
    for r in rows():
        path = r[1][len(T4A_MARK):] if r[1].startswith(T4A_MARK) else r[1]
        method = 'CT_PBT4A_06*' if r[1].startswith(T4A_MARK) else r[2]
        out.setdefault(path, {})[method] = reason(r, hand)
    for texts in out.values():
        for t in texts.values():
            assert all(ord(ch) < 0x80 for ch in t), t
    return out


ATTR = re.compile(r'^([ \t]*)\[(Fact|Theory)\]([ \t]*)$')


def apply(text, reasons):
    """El texto con el `Skip` de cada prueba de `reasons` en su línea de atributo. Para si una prueba no está una sola vez, si su atributo no es
    `[Fact]` o `[Theory]` sin argumentos o si ya lleva `Skip`."""
    lines = text.split('\n')
    decl = re.compile(r'\bpublic\s+(?:async\s+)?(?:void|Task)\s+(\w+)\s*\(')
    done = {}
    for i, line in enumerate(lines):
        m = decl.search(line)
        if not m:
            continue
        name = m.group(1)
        key = name if name in reasons else ('CT_PBT4A_06*' if 'CT_PBT4A_06*' in reasons and name.startswith('CT_PBT4A_06') else None)
        if key is None:
            continue
        j = i - 1
        while j >= 0 and not ATTR.match(lines[j]):
            assert lines[j].strip().startswith('[') or not lines[j].strip() or lines[j].strip().startswith('//'), (name, lines[j])
            assert 'Skip' not in lines[j], name
            j -= 1
            assert i - j < 40, name
        a = ATTR.match(lines[j])
        assert a, (name, 'sin [Fact] ni [Theory] sin argumentos')
        lines[j] = '%s[%s(Skip = "%s")]%s' % (a.group(1), a.group(2), reasons[key].replace('"', '\\"'), a.group(3))
        assert key not in done, name
        done[key] = name
    missing = sorted(set(reasons) - set(done))
    if missing:
        raise SystemExit('PARA: pruebas de la lista que no estan en el archivo: %s' % ', '.join(missing[:5]))
    return '\n'.join(lines), done
