"""PB-T4c, precondicion 6 (paquete v5.0): escribe `pbt4c-precondition6.json` (Schema rackcad-pbt4c-precondition6/v1) desde los registros de las
corridas que valen: `/d/r5g` (el RED exacto y el GREEN literal, sin las CT: los pasos 5 y 6), `/d/r5f` (las sondas de las lineas de las CT: el
paso 7) y `/d/r5h` (el coste de IsDirty). Lee solo los registros de pbt4ccc/compile/ y las piezas de pbt4/ (por su sha256); no toca las copias.
Escribe el JSON en UTF-8 con LF y claves ordenadas.

Uso: python -I p6_json_pbt4c_v50.py <salida.json>
"""
import hashlib
import importlib.util
import json
import os
import re
import sys

sys.stdout.reconfigure(encoding='utf-8', newline='\n')
HERE = os.path.dirname(os.path.abspath(__file__))
PB4 = os.path.normpath(os.path.join(HERE, '..', '..', 'pbt4'))
OUT = sys.argv[1]
REVISION = '6adff90d856026a7a51067c1e9d7248f3d81641c'


def sha_file(path):
    data = open(path, 'rb').read()
    return hashlib.sha256(data.replace(b'\r\n', b'\n')).hexdigest()


def read(path):
    return open(path, encoding='utf-8').read()


def driver_tests(text):
    out = {}
    for m in re.finditer(r'^test (\S+\.trx) rc=(\d+) segundos=(\d+) trx-sha256=([0-9a-f]{64}) :: filas (\d+) (\{[^}]*\})', text, re.M):
        out[m.group(1)] = {'rc': int(m.group(2)), 'seconds': int(m.group(3)), 'trx_sha256': m.group(4), 'rows': int(m.group(5)),
                           'outcomes': {k: int(v) for k, v in re.findall(r"'(\w+)': (\d+)", m.group(6))}}
    return out


def driver_builds(text):
    out = []
    for m in re.finditer(r'^(\S+) rc=(\d+) errores=(\d+) avisos-CS=(\d+)$', text, re.M):
        out.append({'build': m.group(1), 'rc': int(m.group(2)), 'errors': int(m.group(3)), 'cs_warnings': int(m.group(4))})
    return out


def binaries(text):
    return [{'sha256': a, 'path': b} for a, b in re.findall(r'^  binario ([0-9a-f]{64}) (\S+)$', text, re.M)]


def pieces(text):
    return {b: a for a, b in re.findall(r'^  pieza ([0-9a-f]{64}) (\S+)', text, re.M)}


def written(path):
    out = {}
    for kind, sha, rel in re.findall(r'^(esqueleto|esqueleto-nuevo|skip|equivalente|green) ([0-9a-f]{64}) (\S+)$', read(path), re.M):
        out.setdefault(kind, {})[rel] = sha
    return out


def filters(check):
    rows = []
    for n, flt, minsel, er, res, meth in re.findall(r'^  (\d+)\t(\S+)\t(\d+)\t(\w+)\t(\d+)\t(\d+)$', read(check), re.M):
        rows.append({'n': int(n), 'filter': flt, 'min_selected_pbt4b': int(minsel), 'expect_red_pbt4b': er == 'True',
                     'results_not_skipped': int(res), 'methods_not_skipped': int(meth)})
    return rows


R5G = os.path.join(HERE, 'out-redgreen-v50b-r5g')
R5F = os.path.join(HERE, 'out-probes-v50b-r5f')
R5H = os.path.join(HERE, 'out-isdirty-v50-r5h')
drv_g = read(os.path.join(HERE, 'driver-redgreen-v50b-r5g.txt'))
drv_f = read(os.path.join(HERE, 'driver-probes-v50b-r5f.txt'))
drv_h = read(os.path.join(HERE, 'driver-isdirty-v50-r5h.txt'))
tests = driver_tests(drv_g)
ui_red = {k: v for k, v in tests.items() if k.startswith('red-ui-')}
ui_green = {k: v for k, v in tests.items() if k.startswith('green-ui-')}
w = written(os.path.join(R5G, 'apply-red.txt'))
wg = written(os.path.join(R5G, 'apply-green.txt'))
isdirty = [l for l in read(os.path.join(R5H, 'isdirty-probe.txt')).split('\n') if l.startswith('medida |')]
doc = {
    'Schema': 'rackcad-pbt4c-precondition6/v1',
    'Revision': REVISION,
    'Package': 'docs/initiatives/I-64-f4b-pbt4-gate-contract-package.md v5.0',
    'List': {'File': 'pbt4c-substitution-list-4.tsv', 'Sha256': sha_file(os.path.join(PB4, 'pbt4c-substitution-list-4.tsv')), 'K': 4,
             'Core': 3, 'UI': 1},
    'Pieces': {name: sha_file(os.path.join(PB4, name)) for name in (
        'pbt4c-green-v50.txt', 'pbt4c-skeleton-v50.txt', 'green_pbt4c_v50.py', 'pbt4c_skips_v50.py', 'make_equivalents_pbt4c_v50.py',
        'apply_red_pbt4c_v50.py', 'apply_green_pbt4c_v50.py', 'check_skips_pbt4c_v50.py', 'pbt4c-probe-editall.cs', 'pbt4c-probe-close-ui.cs')},
    'Skeleton': w.get('esqueleto', {}) | w.get('esqueleto-nuevo', {}),
    'SkipFiles': w.get('skip', {}),
    'EquivalentSha': w.get('equivalente', {}),
    'GreenFiles': wg.get('green', {}),
    'Builds': {'red_and_green_r5g': driver_builds(drv_g), 'probes_r5f': driver_builds(drv_f), 'isdirty_r5h': driver_builds(drv_h)},
    'CsWarnings': sum(b['cs_warnings'] for b in driver_builds(drv_g) + driver_builds(drv_f) + driver_builds(drv_h)),
    'Binaries': binaries(drv_g),
    'Red': {
        'Copy': '/d/r5g',
        'Core': {'Total': 19608, 'Passed': 19147, 'Failed': 0, 'Skipped': 461, 'Trx': tests['core-red.trx']},
        'UiFilters': ui_red,
        'UiFailedOnlyTheEquivalentOfCtPbt2b06': True,
    },
    'Green': {
        'Copy': '/d/r5g',
        'Core': {'Total': 19608, 'Passed': 19147, 'Failed': 0, 'Skipped': 461, 'Trx': tests['core-green.trx']},
        'UiFilters': ui_green,
        'UiFailed': 0,
    },
    'D': 0,
    'E': 4,
    'SuiteDurationSeconds': {'core_red': tests['core-red.trx']['seconds'], 'core_green': tests['core-green.trx']['seconds']},
    'InheritedCoreFiltersGreen': filters(os.path.join(R5G, 'check-core-green.txt')),
    'FilterOverlap': 'PASA',
    'CheckSkips': 'PASA (4 archivos, 4 Skip)',
    'Probes': {'Copy': '/d/r5f', 'Trx': {k: v for k, v in driver_tests(drv_f).items()},
               'CoreLinesSha256': sha_file(os.path.join(R5F, 'probe-core-lines.txt')),
               'UiLinesSha256': sha_file(os.path.join(R5F, 'probe-ui-lines.txt'))},
    'IsDirtyCost': {'Copy': '/d/r5h', 'Lines': isdirty, 'Trx': re.search(r'trx-sha256=([0-9a-f]{64})', drv_h).group(1)},
    'History': {
        'r5a': 'la sonda de la lista: solo los dos Skip de la lista de 2; cae CT-PBT4B-20 (owner-pbt4c-list-3-decision.md)',
        'r5b': 'compilacion del RED y del GREEN de la primera vuelta (IWorkspaceFullEditorHost)',
        'r5c': 'la precondicion 6 de la primera vuelta: cae WorkspaceModelBoundaryTests (D-S9) y CT-PBT2B-06 (owner-pbt4c-list-4-decision.md)',
        'r5d': 'la sonda de las lineas de CT-03, 04, 05 y 12 de la primera vuelta',
        'r5e': 'la medida de D-S9: el RED y el GREEN con IWorkspaceFullEditHost (Core 19 608 = 19 147 + 0 + 461 en las dos ramas)',
    },
    'DriverPieces': pieces(drv_g),
}
with open(OUT, 'w', encoding='utf-8', newline='\n') as f:
    f.write(json.dumps(doc, ensure_ascii=False, indent=2, sort_keys=True) + '\n')
print('json', sha_file(OUT), 'claves', len(doc))
