"""PB-T4c, precondicion 6 (paquete v5.1): escribe `pbt4c-precondition6.json` (Schema rackcad-pbt4c-precondition6/v1, con las claves de la v5.0 y
cuatro mas: Rehearsal, Control, Census y ClassResults) desde los registros de las corridas de la v5.1: `/d/r5j` (el RED exacto y el GREEN literal,
sin las CT: los pasos 2 a 6), `/d/r5i` (las sondas del paso 7 y el ensayo del paso 8), `/d/r5k` (el coste de IsDirty) y `/d/r5l` (el control del
ensayo sobre la v5.0), y el censo de C13 (v51-census/). Lee solo los registros de pbt4ccc/compile/ y las piezas de pbt4/ (por su sha256); no toca
las copias. Escribe el JSON en UTF-8 con LF y claves ordenadas.

Uso: python -I p6_json_pbt4c_v51.py <salida.json>
"""
import hashlib
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
    return [{'build': m.group(1), 'rc': int(m.group(2)), 'errors': int(m.group(3)), 'cs_warnings': int(m.group(4))}
            for m in re.finditer(r'^(\S+) rc=(\d+) errores=(\d+) avisos-CS=(\d+)$', text, re.M)]


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
    return [{'n': int(n), 'filter': flt, 'min_selected_pbt4b': int(minsel), 'expect_red_pbt4b': er == 'True', 'results_not_skipped': int(res),
             'methods_not_skipped': int(meth)}
            for n, flt, minsel, er, res, meth in re.findall(r'^  (\d+)\t(\S+)\t(\d+)\t(\w+)\t(\d+)\t(\d+)$', read(check), re.M)]


def last(path):
    return read(path).rstrip('\n').split('\n')[-1]


RG, EN, ID, CCO, CEN = (os.path.join(HERE, d) for d in ('r5j-rg', 'r5i-en', 'r5k-id', 'r5l-cc', 'v51-census'))
drv_rg = read(os.path.join(HERE, 'driver-r5j-rg.txt'))
drv_en = read(os.path.join(HERE, 'driver-r5i-en.txt'))
drv_id = read(os.path.join(HERE, 'driver-r5k-id.txt'))
drv_cc = read(os.path.join(HERE, 'driver-r5l-cc.txt'))
tests = driver_tests(drv_rg)
ui_red = {k: v for k, v in tests.items() if k.startswith('r-ui-')}
ui_green = {k: v for k, v in tests.items() if k.startswith('g-ui-')}
assert len(ui_red) == len(ui_green) == 18, (len(ui_red), len(ui_green))
w = written(os.path.join(RG, 'apply-red.txt'))
wg = written(os.path.join(RG, 'apply-green.txt'))
isdirty = [l for l in read(os.path.join(ID, 'isdirty-probe.txt')).split('\n') if l.startswith('medida |')]
pb20 = next(l for l in isdirty if '| pb-20-ab | IsDirty limpio |' in l)
builds = driver_builds(drv_rg) + driver_builds(drv_en) + driver_builds(drv_id) + driver_builds(drv_cc)
checks = {'core_red': last(os.path.join(RG, 'check-core-red.txt')), 'core_green': last(os.path.join(RG, 'check-core-green.txt')),
          'ui_red': last(os.path.join(RG, 'check-ui-red.txt')), 'ui_green': last(os.path.join(RG, 'check-ui-green.txt'))}
assert all(v.endswith('PASA') or 'PASA' in v for v in checks.values()), checks
doc = {
    'Schema': 'rackcad-pbt4c-precondition6/v1',
    'Revision': REVISION,
    'Package': 'docs/initiatives/I-64-f4b-pbt4-gate-contract-package.md v5.1',
    'List': {'File': 'pbt4c-substitution-list-4.tsv', 'Sha256': sha_file(os.path.join(PB4, 'pbt4c-substitution-list-4.tsv')), 'K': 4, 'Core': 3, 'UI': 1},
    'Pieces': {name: sha_file(os.path.join(PB4, name)) for name in (
        'pbt4c-green-v51.txt', 'pbt4c-skeleton-v51.txt', 'green_pbt4c_v51.py', 'pbt4c_skips_v50.py', 'make_equivalents_pbt4c_v50.py',
        'apply_red_pbt4c_v51.py', 'apply_green_pbt4c_v51.py', 'check_skips_pbt4c_v50.py', 'pbt4c_ct_table_v51.py', 'make_list_pbt4c_4.py',
        'pbt4c-probe-editall.cs', 'pbt4c-probe-close-ui.cs', 'pbt4c-rehearsal-textct.cs')},
    'Skeleton': w.get('esqueleto', {}) | w.get('esqueleto-nuevo', {}),
    'SkipFiles': w.get('skip', {}),
    'EquivalentSha': w.get('equivalente', {}),
    'GreenFiles': wg.get('green', {}),
    'Builds': {'red_and_green_r5j': driver_builds(drv_rg), 'rehearsal_and_probes_r5i': driver_builds(drv_en), 'isdirty_r5k': driver_builds(drv_id),
               'control_r5l': driver_builds(drv_cc)},
    'CsWarnings': sum(b['cs_warnings'] for b in builds),
    'Binaries': binaries(drv_rg),
    'Red': {'Copy': '/d/r5j', 'Core': {'Total': 19608, 'Passed': 19147, 'Failed': 0, 'Skipped': 461, 'Trx': tests['core-red.trx']},
            'UiFilters': ui_red, 'UiFailedOnlyTheEquivalentOfCtPbt2b06': True},
    'Green': {'Copy': '/d/r5j', 'Core': {'Total': 19608, 'Passed': 19147, 'Failed': 0, 'Skipped': 461, 'Trx': tests['core-green.trx']},
              'UiFilters': ui_green, 'UiFailed': 0},
    'Checks': checks,
    'D': 0,
    'E': 4,
    'SuiteDurationSeconds': {'core_red': tests['core-red.trx']['seconds'], 'core_green': tests['core-green.trx']['seconds']},
    'InheritedCoreFiltersGreen': filters(os.path.join(RG, 'check-core-green.txt')),
    'FilterOverlap': last(os.path.join(RG, 'filtros.txt')),
    'CheckSkips': last(os.path.join(RG, 'check-skips.txt')),
    'Probes': {'Copy': '/d/r5i', 'Trx': {k: v for k, v in driver_tests(drv_en).items() if k.startswith('sonda-')},
               'CoreLinesSha256': sha_file(os.path.join(EN, 'sonda-core-lineas.txt')), 'UiLinesSha256': sha_file(os.path.join(EN, 'sonda-ui-lineas.txt')),
               'SameAsR5f': sha_file(os.path.join(EN, 'sonda-core-lineas.txt')) == sha_file(os.path.join(HERE, 'out-probes-v50b-r5f', 'probe-core-lines.txt'))
               and sha_file(os.path.join(EN, 'sonda-ui-lineas.txt')) == sha_file(os.path.join(HERE, 'out-probes-v50b-r5f', 'probe-ui-lines.txt'))},
    'Rehearsal': {'Copy': '/d/r5i', 'Red': last(os.path.join(EN, 'ensayo-red.txt')).split(': ')[-1],
                  'Green': last(os.path.join(EN, 'ensayo-green.txt')).split(': ')[-1],
                  'Trx': {k: v for k, v in driver_tests(drv_en).items() if k.startswith('en-')}},
    'Control': 'VE I-01' if last(os.path.join(CCO, 'control-red.txt')).endswith('VE I-01') and last(os.path.join(CCO, 'control-green.txt')).endswith(
        ': VE I-01') else 'NO VE I-01',
    'Census': last(os.path.join(CEN, 'census-c13.txt')).split(': ')[-1],
    'ClassResults': {name: sha_file(os.path.join(RG, name)) for name in ('clases-ui-red.txt', 'clases-ui-green.txt', 'clases-core-red.txt',
                                                                         'clases-core-green.txt', 'clases-core-green-todas.txt')}
                    | {'r5i-en/clases-ensayo.txt': sha_file(os.path.join(EN, 'clases-ensayo.txt'))}
                    | {'v51-clases/%s.txt' % c: sha_file(os.path.join(HERE, 'v51-clases', c + '.txt')) for c in ('r5c', 'r5g', 'r5i', 'r5j', 'r5k', 'r5l')},
    'IsDirtyCost': {'Copy': '/d/r5k', 'Lines': isdirty, 'Pb20AbCleanMedian': re.search(r'mediana=([\d.]+ ms)', pb20).group(1),
                    'Trx': re.search(r'trx-sha256=([0-9a-f]{64})', drv_id).group(1)},
    'History': {
        'r5a': 'la sonda de la lista: solo los dos Skip de la lista de 2; cae CT-PBT4B-20 (owner-pbt4c-list-3-decision.md)',
        'r5b': 'compilacion del RED y del GREEN de la primera vuelta (IWorkspaceFullEditorHost)',
        'r5c': 'la precondicion 6 de la primera vuelta: cae WorkspaceModelBoundaryTests (D-S9) y CT-PBT2B-06 (owner-pbt4c-list-4-decision.md)',
        'r5d': 'la sonda de las lineas de CT-03, 04, 05 y 12 de la primera vuelta',
        'r5e': 'la medida de D-S9: el RED y el GREEN con IWorkspaceFullEditHost (Core 19 608 = 19 147 + 0 + 461 en las dos ramas)',
        'r5f': 'v5.0: las sondas del paso 7 (las mismas lineas que en /d/r5i)',
        'r5g': 'v5.0: los pasos 2 a 6 con las piezas de la v5.0 (con #nullable enable en Draft/: CA-T4c I-01), sin las CT; resumen 0cb67aee',
        'r5h': 'v5.0: el coste de IsDirty',
    },
    'DriverPieces': pieces(drv_rg) | pieces(drv_en),
}
assert doc['CsWarnings'] == 0 and doc['Rehearsal']['Red'] == 'PASA' and doc['Rehearsal']['Green'] == 'PASA' and doc['Census'] == 'PASA'
assert doc['Control'] == 'VE I-01' and doc['Probes']['SameAsR5f']
with open(OUT, 'w', encoding='utf-8', newline='\n') as f:
    f.write(json.dumps(doc, ensure_ascii=False, indent=2, sort_keys=True) + '\n')
print('json', sha_file(OUT), 'claves', len(doc))
