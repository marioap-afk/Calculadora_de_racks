"""PB-T4c, precondicion 6 (paquete v5.1): escribe `compile-check.md` desde los registros de las corridas (pbt4ccc/compile/), con el resultado de
cada clase que corre (Passed, Failed y Skipped): las de UI de cada filtro, las de Core de la suite completa (en las dos ramas), las del ensayo y
las de las sondas; y, en la historia, el de cada filtro de la v5.0 (`/d/r5g`), tambien de los registros que la custodia solo lleva por sha256.
Lee solo los registros (y las tablas de clases que escribio classes_v51.py); escribe en UTF-8 con LF.

Uso: python -I compile_check_v51.py <salida.md>
"""
import glob
import hashlib
import os
import re
import sys

sys.stdout.reconfigure(encoding='utf-8', newline='\n')
HERE = os.path.dirname(os.path.abspath(__file__))
OUT = sys.argv[1]
PREFIX = 'docs/automation/evidence/I-64-pilot/F4B-DRAFTS/pbt4/cc4c/'


def sha(path):
    return hashlib.sha256(open(path, 'rb').read().replace(b'\r\n', b'\n')).hexdigest()


def read(rel):
    return open(os.path.join(HERE, rel), encoding='utf-8').read()


def last(rel):
    return read(rel).rstrip('\n').split('\n')[-1]


def first(rel):
    return read(rel).split('\n')[0]


def counts(line):
    m = re.match(r'filas (\d+) (\{[^}]*\})', line)
    c = dict((k, int(v)) for k, v in re.findall(r"'(\w+)': (\d+)", m.group(2)))
    return int(m.group(1)), c.get('Passed', 0), c.get('Failed', 0), c.get('NotExecuted', 0)


def short(s):
    return s[:8] + '…' + s[-4:]


def table_rows(rel):
    return [l for l in read(rel).split('\n') if l.startswith('| `')]


LIMIT = 156  # la custodia lleva solo por sha256 toda ruta de mas de 156 caracteres con el prefijo de cc4c/
ALL = sorted(os.path.relpath(os.path.join(d, f), HERE).replace(os.sep, '/') for d, _, fs in os.walk(HERE) for f in fs)
LONG = [(p, len(PREFIX + p)) for p in ALL if len(PREFIX + p) > LIMIT]
NEW_DIRS = ('r5i-en/', 'r5j-rg/', 'r5k-id/', 'r5l-cc/', 'v51-census/', 'v51-clases/')
NEW = [p for p in ALL if p.startswith(NEW_DIRS) or (p.startswith('driver-r5') and p[9] in 'ijkl')]
assert all(len(PREFIX + p) <= LIMIT for p in NEW), [p for p in NEW if len(PREFIX + p) > LIMIT]
RUNS = {'r5c': 'driver-redgreen-r5c.txt', 'r5g': 'driver-redgreen-v50b-r5g.txt', 'r5i': 'driver-r5i-en.txt', 'r5j': 'driver-r5j-rg.txt',
        'r5k': 'driver-r5k-id.txt', 'r5l': 'driver-r5l-cc.txt'}


def run_table(copy, what):
    """La tabla de resultados por clase de una corrida entera (v51-clases/<copia>.txt, de run_classes_v51.py sobre sus TRX)."""
    rel = 'v51-clases/%s.txt' % copy
    body = read(rel).rstrip('\n').split('\n')
    A('**El resultado de cada clase que corre en `/d/%s`** (%s; `%s`, sha256 `%s`; Passed / Failed / Skipped):' % (copy, what, rel, short(sha(os.path.join(HERE, rel)))))
    A('')
    L.extend(body)
    A('')


J = sha(os.path.join(HERE, 'pbt4c-precondition6.json'))
drv = {k: read('driver-%s.txt' % k) for k in ('r5j-rg', 'r5i-en', 'r5k-id', 'r5l-cc')}
L = []
A = L.append
A('# PB-T4c: la precondición 6 del paquete de PB-T4 v5.1')
A('')
A('**Base:** el cierre de PB-T4b, `6adff90d856026a7a51067c1e9d7248f3d81641c` (F4B-E53). Cada corrida, en una copia exportada nueva (`git archive` del '
  'checkout principal; nada más en él), fuera de todo worktree. SDK de usuario (`<datos-locales-del-usuario>\\Microsoft\\dotnet\\dotnet.exe`), con '
  '`--no-incremental -nodeReuse:false -p:UseSharedCompilation=false`; pruebas en Debug y el Plugin en Release con `-o`; cero `warning CS`; la suite '
  '**completa** de Core y, en UI, **solo los filtros** del contrato; `build-server shutdown` al final. Ninguna corrida toca el worktree, y el checkout '
  'principal queda limpio en todas (`MAINCO []`).')
A('')
A('**El resumen:** `pbt4c-precondition6.json` (Schema `rackcad-pbt4c-precondition6/v1`, con las claves de la v5.0 y `Rehearsal`, `Control`, `Census` '
  'y `ClassResults`), sha256 `%s`, que escribe `p6_json_pbt4c_v51.py` (`%s`) desde los registros de abajo. **El de la v5.0,** como historia: '
  '`pbt4c-precondition6-v50.json` (`%s`), con su registro, `compile-check-v50.md` (`%s`).'
  % (J, short(sha(os.path.join(HERE, 'p6_json_pbt4c_v51.py'))), sha(os.path.join(HERE, 'pbt4c-precondition6-v50.json')),
     sha(os.path.join(HERE, 'compile-check-v50.md'))))
A('')
A('**Los nombres de las salidas de la v5.1 son cortos** (`<copia>-rg/`, `-en/`, `-id/`, `-cc/`, `v51-census/` y `v51-clases/`; los filtros de UI, '
  'por su número `NN`): con el prefijo `%s`, la ruta más larga de las %d tiene %d caracteres (el límite es %d). **Cada corrida trae aquí la tabla '
  'de resultados de cada clase** (Passed, Failed y Skipped), de Core y de UI, también las de la v5.0 cuyas salidas la custodia solo lleva por '
  'sha256 (`/d/r5c` y `/d/r5g`).' % (PREFIX, len(NEW), max(len(PREFIX + p) for p in NEW), LIMIT))
A('')
A('## Las corridas que valen (v5.1)')
A('')
pieces = re.findall(r'^  pieza ([0-9a-f]{64}) (\S+)', drv['r5j-rg'], re.M)
A('**Las piezas** (sha256 del texto con `\\n`; las mismas en las cuatro corridas que las usan, por la cabecera de cada registro):')
A('')
A('| Pieza | sha256 |')
A('|---|---|')
for s, n in pieces:
    A('| `%s` | `%s` |' % (n, s))
A('')

# ------------------------------------------------------------------ /d/r5j
A('### `/d/r5j`: los pasos 2 a 6, el RED exacto y el GREEN literal, sin las CT')
A('')
A('Registro: `driver-r5j-rg.txt` (`%s`) y `r5j-rg/`.' % sha(os.path.join(HERE, 'driver-r5j-rg.txt')))
A('')
for line in drv['r5j-rg'].split('\n'):
    if re.match(r'^(red|green)-(core|ui|plugin) rc=', line):
        A('- %s' % line)
A('- **Los `Skip`** (`check_skips_pbt4c_v50.py`): %s (`r5j-rg/check-skips.txt`).' % last('r5j-rg/check-skips.txt'))
tests = {m.group(1): m for m in re.finditer(r'^test (\S+\.trx) rc=(\d+) segundos=(\d+) trx-sha256=([0-9a-f]{64}) :: (filas .*)$', drv['r5j-rg'], re.M)}
for b in ('red', 'green'):
    m = tests['core-%s.trx' % b]
    tot, p, f, s_ = counts(m.group(5))
    A('- **Core, %s:** **%s = %s `Passed` + %d `Failed` + %s `Skipped`** (TRX `%s`, %s s); %s (`r5j-rg/check-core-%s.txt`).'
      % (b.upper(), format(tot, ',').replace(',', ' '), format(p, ',').replace(',', ' '), f, s_, short(m.group(4)), m.group(3), last('r5j-rg/check-core-%s.txt' % b), b))
A('- **UI:** %s; %s (`r5j-rg/check-ui-red.txt` y `check-ui-green.txt`).' % (last('r5j-rg/check-ui-red.txt'), last('r5j-rg/check-ui-green.txt')))
A('- **Los filtros** (`filter_overlap_pbt4c_v50b.py`, D-S8): %s (`r5j-rg/filtros.txt`).' % last('r5j-rg/filtros.txt'))
A('- **D = 0, K = 4 y E = 4.**')
A('')
A('**Cada filtro de UI** (`NN`, por `r5j-rg/ui-filtros-r.txt` y `ui-filtros-g.txt`; resultados, Passed, Failed y Skipped):')
A('')
A('| NN | Filtro | RED | GREEN |')
A('|---|---|---|---|')
names = dict(l.split(' ', 1) for l in read('r5j-rg/ui-filtros-r.txt').strip().split('\n'))
assert names == dict(l.split(' ', 1) for l in read('r5j-rg/ui-filtros-g.txt').strip().split('\n'))
for nn in sorted(names):
    cells = []
    for b in ('r', 'g'):
        tot, p, f, s_ = counts(first('r5j-rg/%s-ui-%s-res.txt' % (b, nn)))
        cells.append('%d = %d + %d + %d' % (tot, p, f, s_))
    A('| %s | `%s` | %s | %s |' % (nn, names[nn], cells[0], cells[1]))
A('')
run_table('r5j', 'la suite completa de Core y los 18 filtros de UI, en cada rama')

# ------------------------------------------------------------------ /d/r5i
A('### `/d/r5i`: el paso 8 (el ensayo de las CT de texto) y las sondas del paso 7')
A('')
A('Registro: `driver-r5i-en.txt` (`%s`) y `r5i-en/`. El ensayo (`pbt4c-rehearsal-textct.cs`, como `tests/RackCad.Tests/TmpPbt4cTextCt.cs`) sobre el '
  'RED y, tras el GREEN, otra vez con las dos sondas.' % sha(os.path.join(HERE, 'driver-r5i-en.txt')))
A('')
for line in drv['r5i-en'].split('\n'):
    if re.match(r'^en-(red|green)-(core|ui) rc=', line):
        A('- %s' % line)
A('- **El ensayo, RED:** %s; **GREEN:** %s (`r5i-en/ensayo-red.txt` y `ensayo-green.txt`). En el RED, CT-01, 08 y 13 caen por su `Assert.Equal` '
  '(la primera línea del StackTrace, en `TmpPbt4cTextCt.cs`) y CT-07 y CT-15 pasan; en el GREEN, las cinco pasan.'
  % (last('r5i-en/ensayo-red.txt'), last('r5i-en/ensayo-green.txt')))
A('- **Las sondas:** las líneas de Core (`r5i-en/sonda-core-lineas.txt`, `%s`) y de UI (`sonda-ui-lineas.txt`, `%s`), **iguales por sha256 a las de '
  '`/d/r5f`** (la v5.0).' % (short(sha(os.path.join(HERE, 'r5i-en/sonda-core-lineas.txt'))), short(sha(os.path.join(HERE, 'r5i-en/sonda-ui-lineas.txt')))))
A('')
run_table('r5i', 'el ensayo en el RED; el ensayo y las dos sondas en el GREEN (las sondas acaban en `Assert.Fail` con sus líneas: sus dos '
                 '`Failed` son las sondas, no una caída)')

# ------------------------------------------------------------------ el censo, /d/r5k y /d/r5l
A('### El censo de C13 (paso 8, por texto; sin copia)')
A('')
A('`census_c13_v51.py` (`%s`) por objetos git de `6adff90d` y las piezas de la v5.1: **%s** (`v51-census/census-c13.txt`, `%s`).'
  % (short(sha(os.path.join(HERE, 'census_c13_v51.py'))), last('v51-census/census-c13.txt').split(': ')[-1],
     short(sha(os.path.join(HERE, 'v51-census/census-c13.txt')))))
A('')
for line in read('v51-census/census-c13.txt').split('\n')[1:-1]:
    if line:
        A('- %s' % line)
A('')
A('### `/d/r5k`: el coste de `IsDirty` (paso 7)')
A('')
A('Registro: `driver-r5k-id.txt` (`%s`) y `r5k-id/`. La sonda de PB-T4b (`pbt4bcc/compile/TmpIsDirtyCostProbe.cs`, sin tocarla) sobre el GREEN de la '
  'v5.1, en Release y sola en la máquina:' % sha(os.path.join(HERE, 'driver-r5k-id.txt')))
A('')
for line in read('r5k-id/isdirty-probe.txt').split('\n'):
    if line.startswith('medida |'):
        A('- %s' % line[len('medida | '):])
A('')
run_table('r5k', 'la sonda del coste, en Release (acaba en `Assert.Fail` con sus medidas)')
A('### `/d/r5l`: el control del ensayo sobre las piezas de la v5.0')
A('')
A('Registro: `driver-r5l-cc.txt` (`%s`) y `r5l-cc/`. El mismo ensayo sobre `apply_red_pbt4c_v50.py` y `apply_green_pbt4c_v50.py` (con '
  '`#nullable enable` en los dos archivos de `Draft/`): **RED, %s; GREEN, %s** (`r5l-cc/control-red.txt` y `control-green.txt`). CT-15 cae en las '
  'dos ramas, con su primera diferencia en la línea de la pureza; las otras cuatro, como con la v5.1.'
  % (sha(os.path.join(HERE, 'driver-r5l-cc.txt')), last('r5l-cc/control-red.txt').split(': ')[-1], last('r5l-cc/control-green.txt').split(': ')[-1]))
A('')
for line in drv['r5l-cc'].split('\n'):
    if re.match(r'^(cc-\S+ rc=|test cc-)', line):
        A('- %s' % line)
A('')
run_table('r5l', 'el ensayo sobre las piezas de la v5.0, en cada rama')

# ------------------------------------------------------------------ la historia
A('## La historia: la v5.0 y la vuelta parada')
A('')
A('**No valen para la emisión de la v5.1:** las piezas que midieron no son las de la v5.1. Se custodian como historia.')
A('')
A('| Copia | Qué midió | Registro |')
A('|---|---|---|')
A('| `/d/r5a` | La sonda de la lista: solo los dos `Skip` de la lista de 2, sin GREEN. Core 19 605 = 19 144 + 1 + 460; cae CT-PBT4B-20 → '
  '`owner-pbt4c-list-3-decision.md` | `pbt4/v50/probe-skips-r5a-driver.txt` y `pbt4/v50/out-probe-skips-r5a/` |')
A('| `/d/r5b` | La compilación del RED y del GREEN con `IWorkspaceFullEditorHost`: 0 errores y 0 `warning CS` | `driver-compile-r5b.txt` |')
A('| `/d/r5c` | La precondición 6 con la lista de 3: cae `WorkspaceModelBoundaryTests` (por «Editor») en las dos ramas y, en el GREEN, CT-PBT2B-06 → '
  'D-S9 y `owner-pbt4c-list-4-decision.md` | `driver-redgreen-r5c.txt` y `out-redgreen-v50-r5c/` |')
A('| `/d/r5d` | La sonda de CT-03, 04, 05 y 12 con las piezas de la primera vuelta | `driver-probes-r5d.txt` |')
A('| `/d/r5e` | La medida de D-S9: el RED y el GREEN con `IWorkspaceFullEditHost`; Core 19 608 = 19 147 + 0 + 461 en las dos ramas | '
  '`pbt4/v50/variant-rename/driver-redgreen-r5e.txt` |')
A('| `/d/r5f` | v5.0: las sondas del paso 7 (las mismas líneas que `/d/r5i`) | `driver-probes-v50b-r5f.txt` y `out-probes-v50b-r5f/` |')
A('| `/d/r5g` | v5.0: los pasos 2 a 6 con las piezas de la v5.0 (con `#nullable enable` en `Draft/`: CA-T4c I-01, que no vio porque no corre las CT). '
  'Core 19 608 = 19 147 + 0 + 461 en las dos ramas | `driver-redgreen-v50b-r5g.txt` y `out-redgreen-v50b-r5g/` |')
A('| `/d/r5h` | v5.0: el coste de `IsDirty` | `driver-isdirty-v50-r5h.txt` y `out-isdirty-v50-r5h/` |')
A('')
A('**Las salidas de `/d/r5c` y `/d/r5g` que la custodia solo lleva por sha256** (su ruta, con el prefijo de `cc4c/`, pasa de %d caracteres): %s. '
  'Su resultado queda aquí, en la tabla de cada filtro y en la de cada clase de su corrida.' % (LIMIT, ', '.join('`%s` (%d)' % (p, n) for p, n in LONG)))
A('')
for copy, folder in (('r5c', 'out-redgreen-v50-r5c'), ('r5g', 'out-redgreen-v50b-r5g')):
    A('**El resultado de cada filtro de `/d/%s`** (la primera línea de cada `*-outcomes.txt` de `%s/`):' % (copy, folder))
    A('')
    A('| Registro | sha256 | Resultados | Passed | Failed | Skipped |')
    A('|---|---|---|---|---|---|')
    for path in sorted(glob.glob(os.path.join(HERE, folder, '*-outcomes.txt'))):
        rel = os.path.relpath(path, HERE).replace(os.sep, '/')
        tot, p, f, s_ = counts(first(rel))
        A('| `%s` | `%s` | %d | %d | %d | %d |' % (os.path.basename(path), short(sha(path)), tot, p, f, s_))
    A('')
    run_table(copy, 'la suite completa de Core y los filtros de UI de la v5.0, en cada rama (las TRX, en la copia exportada)')
text = '\n'.join(L) + '\n'
with open(OUT, 'w', encoding='utf-8', newline='\n') as f:
    f.write(text)
print('compile-check', hashlib.sha256(text.encode('utf-8')).hexdigest(), text.count('\n'), 'lineas')
