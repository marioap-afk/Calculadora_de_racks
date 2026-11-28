"""Arma la v5.1 del paquete de PB-T4 desde la v5.0 (por su sha256): CA-T4c (independent-review-v1.md, §22: EMITIBLE TRAS CORRECCIONES, 0/1/3).
- I-01: las piezas de la v5.1 (pbt4c-green-v51.txt y pbt4c-skeleton-v51.txt, sin #nullable enable en los dos archivos de Draft/ y sin el ? de
  ProjectOf), sus guiones (green_pbt4c_v51.py, apply_red_pbt4c_v51.py y apply_green_pbt4c_v51.py) y el anexo V51-A, que sustituye al V50-A;
- M-01: de donde sale cada linea de CT-03, 04, 05 y 12, linea a linea, calculado aqui frente a las lineas de la sonda (con su sha256);
- M-02: el nombre del equivalente de PEG; M-03: el comentario de W (en la pieza);
- la leccion: el paso 8 de la precondicion 6 (el ensayo de las CT de texto y el censo de C13), y lo medido de nuevo (pbt4c-precondition6.json v5.1).
PB-T4a y PB-T4b (sus §5.1, §5.2, §6.1 y §6.2) y la tabla de las CT (las lineas de pbt4c_ct_table_v51.py son las de la v5.0) no cambian: se
comprueba. El bloque literal del contrato de PB-T4c (§6.3.1) es ASCII: se comprueba. Escribe solo la v5.1.

Uso: python -I make_v51.py <repo>
"""
import hashlib
import importlib.util
import json
import os
import re
import sys

sys.stdout.reconfigure(encoding='utf-8', newline='\n')
sys.dont_write_bytecode = True
HERE = os.path.dirname(os.path.abspath(__file__))
REPO = sys.argv[1]
REV = '6adff90d'
V50 = os.path.join(HERE, 'I-64-f4b-pbt4-gate-contract-package-v50.md')
OUT = os.path.join(HERE, 'I-64-f4b-pbt4-gate-contract-package-v51.md')
CC = os.path.normpath(os.path.join(HERE, '..', 'pbt4ccc', 'compile'))
raw = open(V50, 'rb').read()
assert hashlib.sha256(raw).hexdigest() == 'd19dd8300d32244a609281b2f819e4b298da5acdd49efc64ec439b2c77dbcfe7'
t = raw.decode('utf-8')


def load(name):
    spec = importlib.util.spec_from_file_location(name, os.path.join(HERE, name + '.py'))
    mod = importlib.util.module_from_spec(spec)
    spec.loader.exec_module(mod)
    return mod


def sha(path):
    data = open(path, 'rb').read()
    assert b'\r' not in data, path
    return hashlib.sha256(data).hexdigest()


ct50 = load('pbt4c_ct_table_v50')
ct = load('pbt4c_ct_table_v51')
assert ct.CT == ct50.CT, 'las CT de la v5.1 son las de la v5.0'
mk = load('make_equivalents_pbt4c_v50')
EQ = mk.equivalents(REPO, REV)
EQSHA = {p: hashlib.sha256(EQ[p].encode('utf-8')).hexdigest() for p in mk.PATHS}
NAMES = ['pbt4c-green-v51.txt', 'pbt4c-skeleton-v51.txt', 'green_pbt4c_v51.py', 'apply_red_pbt4c_v51.py', 'apply_green_pbt4c_v51.py', 'pbt4c_ct_table_v51.py',
         'pbt4c-rehearsal-textct.cs', 'pbt4c_skips_v50.py', 'make_equivalents_pbt4c_v50.py', 'check_skips_pbt4c_v50.py', 'pbt4c-substitution-list-4.tsv',
         'pbt4c-probe-editall.cs', 'pbt4c-probe-close-ui.cs']
S = {n: sha(os.path.join(HERE, n)) for n in NAMES}
for n in ('make_pieces_v51.py', 'make_tools_v51.py', 'make_rehearsal_v51.py', 'check_rr3_v51.py'):
    S['v51/' + n] = sha(os.path.join(HERE, 'v51', n))
for n in ('p6_pbt4c_v51.sh', 'census_c13_v51.py', 'rehearsal_check_v51.py', 'classes_v51.py', 'p6_isdirty_pbt4c_v51.sh', 'p6_control50_v51.sh',
          'control_check_v51.py', 'p6_json_pbt4c_v51.py'):
    S['cc/' + n] = sha(os.path.join(CC, n))
P6 = os.path.join(CC, 'pbt4c-precondition6.json')
P6SHA = sha(P6)
p6 = json.load(open(P6, encoding='utf-8'))
assert p6['Package'].endswith('v5.1') and p6['D'] == 0 and p6['E'] == 4 and p6['CsWarnings'] == 0
assert p6['Red']['Core']['Failed'] == 0 and p6['Green']['Core']['Failed'] == 0
assert p6['Rehearsal']['Red'] == 'PASA' and p6['Rehearsal']['Green'] == 'PASA' and p6['Census'] == 'PASA' and p6['Control'] == 'VE I-01'
for n in ('pbt4c-green-v51.txt', 'pbt4c-skeleton-v51.txt'):
    assert p6['Pieces'][n] == S[n], n
CCHECK = sha(os.path.join(CC, 'compile-check.md'))
PROBE_LINES = open(os.path.join(CC, 'r5i-en', 'sonda-core-lineas.txt'), encoding='utf-8').read().split('\n')
PROBE_SHA = sha(os.path.join(CC, 'r5i-en', 'sonda-core-lineas.txt'))
assert PROBE_SHA == sha(os.path.join(CC, 'out-probes-v50b-r5f', 'probe-core-lines.txt')) == 'cbbd8457149afd6919dbfb763b63c47a60283bc44d01f05c0b92aae1652ea59f'
done = []


def R(old, new, times=1, what=None):
    global t
    n = t.count(old)
    assert n == times, (what or old[:90], n, times)
    t = t.replace(old, new)
    done.append(what or old[:60])


KEEP = []
for h_, u_ in (('### 5.1 PB-T4a', '### 5.2 PB-T4b'), ('### 5.2 PB-T4b', '### 5.3 PB-T4c'), ('### 6.1 PB-T4a', '### 6.2 PB-T4b'),
               ('### 6.2 PB-T4b', '### 6.3 PB-T4c')):
    i_ = t.index('\n' + h_) + 1
    KEEP.append(t[i_:t.index('\n' + u_, i_) + 1])
CTROWS = [l for l in t.split('\n') if re.match(r'\| \*\*\d\d\*\* \| ', l)]

# ---------------------------------------------------------------- M-01: de donde sale cada linea de CT-03, 04, 05 y 12
KV = re.compile(r'([\w-]+)=(\[[^\]]*\]|\S+)')
DERIVED = {
    'sin-borrador': 'derivada: `borrador=Clean` es el `estado-del-borrador=Clean` de la sonda (su `borrador=True` dice que existe)',
    'tras-aplicar': 'derivada: `borrador=Dirty` es el `estado-del-borrador=Dirty` de la sonda',
    'k1-cerrado': 'derivada: `lecturas-dentro` son las lecturas del lector **durante `ShowModal`**. La sonda imprime `lecturas-dentro=1`, el total del '
                  'lector tras `TryEditAll`, con la lectura de `Begin` de antes (`pbt4c-probe-editall.cs:198`); la de `Begin` es una sola '
                  '(`sin-borrador … lecturas=1`), así que durante `ShowModal` son 1 − 1 = **0**',
}
ABSENT = {'cola': 'del texto del GREEN: la sonda no la tiene (el `Begin` de la cola aparcada da `NotAdmitted`, como CT-PBT4B-02 en '
                  '`PushBackFullPanelTests.cs:230`)',
          'conflict': 'del texto del GREEN: la sonda no la tiene. En Conflict el borrador admite cambios (`DraftSession.cs:331-332`), así que abre '
                      'como Clean y Dirty',
          'error': 'del texto del GREEN: la sonda no la tiene. En Error el borrador no admite cambios (`DraftSession.cs:331-332`), así que se '
                   'rechaza como Unknown',
          'adaptador-aplica': 'del texto del GREEN: la sonda no la tiene (el adaptador sobre un borrador que admite el cambio)'}
probe = [l for l in PROBE_LINES if l and not l.startswith('## ')]
m01 = ['| CT | La línea de §5.3 | De dónde sale |', '|---|---|---|']
cats = {'tal cual': 0, 'con sus campos': 0, 'derivada': 0, 'del texto': 0}
for n in ('03', '04', '05', '12'):
    for line in next(c for c in ct.CT if c[0] == n)[2]:
        label = line.split(' ')[0]
        if line in probe:
            where, cat = 'de la sonda, **tal cual**', 'tal cual'
        else:
            same = [p for p in probe if p.split(' ')[0] == label]
            if label in ABSENT:
                assert not same, label
                where, cat = ABSENT[label], 'del texto'
            elif label in DERIVED:
                assert len(same) == 1, label
                where, cat = DERIVED[label], 'derivada'
            else:
                assert len(same) == 1, (label, same)
                want, have = dict(KV.findall(line)), dict(KV.findall(same[0]))
                assert want and all(have.get(k) == v for k, v in want.items()), (line, same[0])
                extra = sorted(set(have) - set(want))
                where, cat = 'de la sonda, **con los campos de la CT**: la sonda imprime además %s' % ', '.join('`%s`' % k for k in extra), 'con sus campos'
        cats[cat] += 1
        m01.append('| CT-%s | `%s` | %s |' % (n, line, where))
assert cats == {'tal cual': 22, 'con sus campos': 9, 'derivada': 3, 'del texto': 4}, cats
M01 = '\n'.join(m01)

# ---------------------------------------------------------------- la cabecera
HEAD = '''**Estado:** v5.1. Corrige **PB-T4c** con la comprobación acotada **CA-T4c** (`independent-review-v1.md`, §22: EMITIBLE TRAS CORRECCIONES, **0/1/3**, sin decisiones nuevas del Owner):
- **I-01:** los dos archivos nuevos de `Draft/`, sin `#nullable enable` y sin el `?` de `ProjectOf`;
- **M-01 a M-03**;
- y **la lección:** la precondición 6 no corría las CT. Gana un paso, el 8: el ensayo de las CT de texto y el censo de C13.

La precondición 6 se repitió entera en copias nuevas (`/d/r5i` a `/d/r5l`), porque cambian el esqueleto y el GREEN.

**Lo que cambia:**
- las dos piezas (`pbt4c-green-v51.txt` y `pbt4c-skeleton-v51.txt`) y sus tres guiones (`green_pbt4c_v51.py`, `apply_red_pbt4c_v51.py` y `apply_green_pbt4c_v51.py`);
- el docstring de las CT como datos (`pbt4c_ct_table_v51.py`); las CT y sus líneas son las mismas;
- §5.3 (M-01), §6.3, §6.3.1 (M-02, el paso 8 y lo medido) y §6.3.2;
- §7, §9 y §11;
- y el anexo V51-A, que sustituye al V50-A.

**Lo que no cambia:** la lista de 4, los `Skip`, los equivalentes y su `EquivalentSha`, las CT y sus líneas, los `RequiredTests`, los totales, las autoridades (salvo la sección de las n.º 0 y 65), D-S7 a D-S10, PB-T4a, PB-T4b y PB-T4d.

**Lo que sale de la v5.1 (PB-T4c):**
- las piezas, desde las de la v5.0 (`v51/make_pieces_v51.py`), y los guiones, desde los de la v5.0 (`v51/make_tools_v51.py`);
- el ensayo de las CT de texto (`pbt4c-rehearsal-textct.cs`, que escribe `v51/make_rehearsal_v51.py` desde los objetos git de la base) y el censo de C13 (`pbt4ccc/compile/census_c13_v51.py`);
- el conductor de la precondición 6 (`p6_pbt4c_v51.sh`), sus lecturas del ensayo (`rehearsal_check_v51.py`) y de cada clase (`classes_v51.py`), el coste (`p6_isdirty_pbt4c_v51.sh`), el control del ensayo sobre la v5.0 (`p6_control50_v51.sh` y `control_check_v51.py`), el resumen (`p6_json_pbt4c_v51.py`) y su registro (`compile_check_v51.py`, que escribe `compile-check.md` con el resultado de cada clase);
- la lectura RR-3 sobre esta versión (`v51/check_rr3_v51.py`);
- y `make_v51.py`, que arma esta versión desde la v5.0.

---

## Registro de cambios v5.0 → v5.1

| Id | Hallazgo | Qué cambia | Dónde |
|---|---|---|---|
| **V51-01** | **CA-T4c I-01** (importante): **CT-PBT4C-15 no podía pasar, ni en el RED ni en el GREEN.** `PushBackFullDraftLaunch.cs` y `PushBackFullDraftStatePort.cs` empezaban por `#nullable enable`, en el esqueleto y en el GREEN, y la pureza que CT-15 copia de CT-PBT3A-14 da `#nullable` en cuanto ve `^\\s*#nullable` (`PushBackFullReadGuardTests.cs:293-296` y `PushBackDraftWriteGuardTests.cs:294-298` de `6adff90d`). La precondición 6 no corre las CT, y las sondas no cubrían la CT-15 | **Sin la directiva en los dos archivos,** en las dos piezas, **y sin el `?` de `RackProject? ProjectOf`**: sin la directiva daría CS8632, porque `Directory.Build.props:28` fija `Nullable` en `disable` (CA-T4a I-01; la regla del §5.0). Los demás archivos nuevos, repasados contra las reglas de las CT de texto que les aplican, con el ensayo del paso 8 (V51-05). No cambian CT-15, la lista, los totales ni los filtros | las dos piezas; el anexo V51-A; §6.3; §6.3.1 |
| V51-02 | CA-T4c M-01 | **De dónde sale cada línea de CT-03, 04, 05 y 12, línea a línea:** 22 tal cual de la sonda, 9 con los campos de la CT, 3 derivadas (y cómo) y 4 del texto del GREEN. **`lecturas-dentro`** son las lecturas durante `ShowModal`. Las corridas: `/d/r5f` (la válida de la v5.0) y `/d/r5i` (la de la v5.1), iguales por sha256. La cita de la vuelta parada (`/d/r5d`) sale | §5.3 |
| V51-03 | CA-T4c M-02 | **El nombre del equivalente de PEG:155-163** conserva «…SeventeenOperations…» aunque su tabla tenga dieciocho operaciones: es la copia literal; lo que fija es la tabla (como la §20 M-02) | §6.3.1 |
| V51-04 | CA-T4c M-03 | **El comentario del GREEN en `PendingWork()` de W** casa con D-S10: en WORKSPACE, un módulo escenificado deja siempre cambios sin aplicar, así que la pregunta del módulo sola es la de CLASSIC. Solo comentario | la pieza del GREEN; el anexo V51-A |
| **V51-05** | **La lección de I-01: la precondición 6 no corría las CT** | **El paso 8, nuevo:** el ensayo de las CT de texto sobre `src/` (CT-01, 07, 08, 13 y 15), en clases temporales con sus líneas literales de §5.3, sobre el RED y sobre el GREEN, con su control sobre las piezas de la v5.0 (que ve I-01), y el censo de C13 y de los nombres, por texto, con sus autopruebas. **Qué cubre y qué no,** declarado | §6.3.1; §7; §11 |
| V51-06 | La precondición 6, otra vez, pasos 1 a 9 | `/d/r5i` (el ensayo y las sondas), `/d/r5j` (el RED exacto y el GREEN literal), `/d/r5k` (el coste) y `/d/r5l` (el control del ensayo sobre la v5.0). Lo previsto, con D = 0. Las de la v5.0 (`/d/r5f`, `/d/r5g` y `/d/r5h`) pasan a historia | §5.3; §6.3.1; §9; §11 |
| V51-07 | La custodia de `compile-check.md` | **El resultado de cada clase que corre** (Passed, Failed y Skipped), también de las que la custodia solo lleva por sha256; las salidas nuevas, con nombres cortos: ninguna ruta pasa de 150 caracteres con el prefijo de `cc4c/` | §6.3.2 |
| V51-08 | La versión | El `Objective` cita la v5.1. CA-T4c: la §22 y su recomprobación (§23). Los guiones y las piezas, por su nombre de la v5.1 en §6.3.2 | §6.3.1; §6.3.2 |

---

## La cabecera de la v5.0, sin cambio salvo lo que dice el registro v5.0 → v5.1
'''
TITLE = '# I-64 F4B — Paquete del contrato de PB-T4 («Editar todo» en el panel del Workspace), v5.0\n'
R(TITLE, TITLE.replace('v5.0', 'v5.1') + '\n' + HEAD + '\n', what='titulo y cabecera')
R('**Estado:** v5.0. Escribe **PB-T4c**', '**Estado de la v5.0:** Escribe **PB-T4c**')
R('**Lo que sale de la v5.0 (PB-T4c):**', '**Lo que salió de la v5.0 (PB-T4c):**')

# Lo vivo empieza tras la cabecera de la v4.9 (las cabeceras y los registros son historia), y acaba antes de los anexos.
LIVE0 = t.index('\n## La cabecera de la v4.9')
LIVE1 = t.index('\n## Anexo V49-A')
head_part, live, tail_part = t[:LIVE0], t[LIVE0:LIVE1], t[LIVE1:]


def L(old, new, times=1, what=None):
    global live
    n = live.count(old)
    assert n == times, (what or old[:90], n, times)
    live = live.replace(old, new)
    done.append(what or old[:60])


# ---------------------------------------------------------------- las piezas, por su nombre y su sha256 de la v5.1
for old, new in (('6b304a009d05ff586603ed4bbb9fa69b9973977bea09222c688ef80faa983e1b', S['pbt4c-green-v51.txt']),
                 ('48e097a7a579bd311fdeb2f6b87ff57ec0ada157573b174ad3a02935e5ab816c', S['pbt4c-skeleton-v51.txt']),
                 ('d459501c7657706be6133123ff82608f8035d135827b5b92b1839ace0cb56c6a', S['green_pbt4c_v51.py']),
                 ('3fee9212e5ad57cdd3e437dc46f62682fc1d3d7f0c921c236e1bc6e2a30f4003', S['apply_red_pbt4c_v51.py']),
                 ('eb4b4d6d94f93767469c7bc6debcabf39e0f63cd7a3d67877a559b06a31fa3ff', S['apply_green_pbt4c_v51.py']),
                 ('39728a5419264d6ce4f444fedc81e2982944dde45c391d830704c39715500e36', S['pbt4c_ct_table_v51.py']),
                 ('pbt4c-green-v50.txt', 'pbt4c-green-v51.txt'), ('pbt4c-skeleton-v50.txt', 'pbt4c-skeleton-v51.txt'),
                 ('apply_green_pbt4c_v50.py', 'apply_green_pbt4c_v51.py'), ('green_pbt4c_v50.py', 'green_pbt4c_v51.py'),
                 ('apply_red_pbt4c_v50.py', 'apply_red_pbt4c_v51.py'), ('pbt4c_ct_table_v50.py', 'pbt4c_ct_table_v51.py'),
                 ('anexo V50-A', 'anexo V51-A')):
    n = live.count(old)
    assert n >= 1, old
    live = live.replace(old, new)
    done.append('%s x%d' % (old[:24], n))

# ---------------------------------------------------------------- §5.3 (M-01) y lo medido
L('**Las líneas de CT-03, 04, 05 y 12 son las que dio la sonda de la precondición 6** (`pbt4c-probe-editall.cs`, una clase temporal sobre la '
  'copia con el GREEN; `pbt4ccc/compile/out-probes-v50-r5d/probe-editall-lines.txt`); las demás salen del texto literal del GREEN (anexo V51-A).',
  '**De dónde sale cada línea** (V51-02; CA-T4c M-01):\n'
  '- **las de CT-03, 04, 05 y 12,** de la sonda de Core sobre el GREEN literal (`pbt4c-probe-editall.cs`, una clase temporal sobre la copia con el '
  'GREEN), como dice, línea a línea, la tabla de después de la de las CT. La corrida válida de la v5.0 es `/d/r5f` '
  '(`pbt4ccc/compile/out-probes-v50b-r5f/probe-core-lines.txt`) y la de la v5.1, `/d/r5i` (`pbt4ccc/compile/r5i-en/sonda-core-lineas.txt`): '
  'iguales, sha256 `%s`;\n'
  '- **las de CT-01, 07, 08, 13 y 15,** del ensayo de las CT de texto (§6.3.1, el paso 8), que las da tal cual sobre el GREEN (y, sobre el RED, '
  'las de CT-07 y CT-15, mientras CT-01, 08 y 13 caen por su `Assert.Equal`);\n'
  '- **las de CT-09,** de la sonda de UI (`pbt4c-probe-close-ui.cs`; D-S10);\n'
  '- **las de CT-02, 06, 10, 11 y 14,** del texto literal del GREEN (anexo V51-A), sin ejecutar.' % PROBE_SHA, what='M-01 §5.3')
i = live.index('\n| **15** | **I-24:** G-L1 y la pureza sobre los dos archivos nuevos de `Draft/` de PB-T4c')
j = live.index('\n', i + 1) + 1
live = (live[:j] + '\n**De dónde sale cada línea de CT-03, 04, 05 y 12** (V51-02; CA-T4c M-01): **%d tal cual** de la sonda, **%d con los campos '
        'de la CT** (la sonda imprime además campos que la CT no fija, con los mismos valores en los que sí fija), **%d derivadas** y **%d del texto** '
        'del GREEN, que la sonda no tiene.\n\n%s\n' % (cats['tal cual'], cats['con sus campos'], cats['derivada'], cats['del texto'], M01) + live[j:])
done.append('M-01 tabla')
L('**D = 0 medido** en la precondición 6 (`/d/r5g`), con la suite completa de Core sobre el RED exacto y sobre el GREEN literal, sin las CT, y '
  'los 18 filtros de UI (`pbt4ccc/compile/pbt4c-precondition6.json`, sha256 `0cb67aee2b2d925d68235bb6444566b4222205a6cf97f472b3851fd0795b2f51`, '
  'y `compile-check.md`, `bca9fde88b3a4ac881860769260780354a4440a48a94c9690b8c08f7acaefc44`)',
  '**D = 0 medido** en la precondición 6 de la v5.1 (`/d/r5j`), con la suite completa de Core sobre el RED exacto y sobre el GREEN literal, sin '
  'las CT, y los 18 filtros de UI (`pbt4ccc/compile/pbt4c-precondition6.json`, sha256 `%s`, y `compile-check.md`, `%s`); y, en la v5.0, en '
  '`/d/r5g`, como historia' % (P6SHA, CCHECK))
L('(la precondición 6, `/d/r5g`; §6.3.1)', '(la precondición 6 de la v5.1, `/d/r5j`; §6.3.1)')

# ---------------------------------------------------------------- §6.3.1: el Objective, M-02, el paso 8 y lo medido
L('segun el paquete del contrato de PB-T4 v5.0,', 'segun el paquete del contrato de PB-T4 v5.1,')
L('UI, **`ExpectRed` true** (cae en el RED, V50-18)\n',
  'UI, **`ExpectRed` true** (cae en el RED, V50-18)\n\n'
  '**El nombre del equivalente de PEG:155-163** (V51-03; CA-T4c M-02) conserva el de su original, '
  '`ThePortDeclaresTheSeventeenOperationsWithTheEnumsAsOrderedPrefixesAndNamesNoTypeOfTheDraftsNorOfThePilot`, aunque su tabla tenga dieciocho '
  'operaciones: es la copia literal (`owner-pbt4-decisions.md`, pregunta 4), y lo que fija es la tabla, no el nombre (como la §20 M-02 de CA-T4b con '
  'las copias de D-S1).\n')
L('7. **las sondas, sobre el GREEN,** como clases temporales que no entran en ningún commit y acaban en `Assert.Fail(texto)`, cada una con su filtro: '
  '`pbt4c-probe-editall.cs` (las líneas de CT-03, 04, 05 y 12) y `pbt4c-probe-close-ui.cs` (CT-09 y D-S10); y el coste de `IsDirty` con la sonda de '
  'PB-T4b (`pbt4bcc/compile/TmpIsDirtyCostProbe.cs`), en Release;\n'
  '8. el contrato lleva por sha256 las piezas del GREEN y del esqueleto, los 4 equivalentes (`EquivalentSha`) y la lista.',
  '7. **las sondas, sobre el GREEN,** como clases temporales que no entran en ningún commit y acaban en `Assert.Fail(texto)`, cada una con su filtro: '
  '`pbt4c-probe-editall.cs` (las líneas de CT-03, 04, 05 y 12) y `pbt4c-probe-close-ui.cs` (CT-09 y D-S10); y el coste de `IsDirty` con la sonda de '
  'PB-T4b (`pbt4bcc/compile/TmpIsDirtyCostProbe.cs`), en Release;\n'
  '8. **el ensayo de las CT de texto y el censo de C13** (V51-05; nuevo en la v5.1, por CA-T4c I-01):\n'
  '   - **el ensayo:** `pbt4c-rehearsal-textct.cs` (sha256 `%s`), cinco clases temporales (`TmpPbt4cTextCt01`, `07`, `08`, `13` y `15`) que no '
  'entran en ningún commit, cada una con un `[Fact]` que acaba en el `Assert.Equal` de sus líneas literales de §5.3. Las escribe '
  '`v51/make_rehearsal_v51.py` (`%s`): CT-15 con G-L1 y la pureza de `PushBackPanelDraftGuardTests.cs` (CT-PBT4B-19) y CT-13 con '
  '`ControllerViolations` de PEG (PEG:659-707), copiados de los objetos git de la base; CT-01, 07 y 08, desde su fila de §5.3. **Sobre el RED,** '
  'CT-07 y CT-15 pasan y CT-01, 08 y 13 caen por su `Assert.Equal`; **sobre el GREEN,** las cinco pasan (`rehearsal_check_v51.py`);\n'
  '   - **su control:** el mismo ensayo sobre las piezas de la v5.0 (`p6_control50_v51.sh`) **ve I-01**: CT-15 cae en las dos ramas, con su primera '
  'diferencia en la línea de la pureza, y las otras cuatro dan lo mismo que con la v5.1 (`control_check_v51.py`);\n'
  '   - **el censo** (`census_c13_v51.py`, `%s`), por texto y por objetos git: la diferencia de declaraciones del GREEN frente a la base es, nombre a '
  'nombre y con su cuenta, la lista cerrada de PBT4C-C13, la del esqueleto es una parte de ella, nada se quita, y ningún nombre nuevo de '
  '`RackCad.Application.Workspace` cae en las reglas de `WorkspaceModelBoundaryTests:83`, :101 y :102; con siete autopruebas (una constante, una '
  'sobrecarga, una propiedad, un miembro de enumerado, un tipo anidado y un `x:Name` de más, y un campo de menos);\n'
  '9. el contrato lleva por sha256 las piezas del GREEN y del esqueleto, los 4 equivalentes (`EquivalentSha`) y la lista.\n\n'
  '**Qué cubre el paso 8 y qué no** (V51-05):\n'
  '- **Cubre, literal y en las dos ramas,** las cinco CT de texto sobre `src/` (CT-01, 07, 08, 13 y 15: todas las de PB-T4c que leen texto), con '
  'su `ExpectRed`; y, por texto, la lista cerrada de C13 y los nombres.\n'
  '- **Cubren las sondas, solo sobre el GREEN,** las líneas de CT-03, 04, 05 y 12 que dice §5.3 (22 tal cual, 9 con sus campos y 3 derivadas) y '
  'las de CT-09.\n'
  '- **No cubre** CT-02, 06 y 14 (la FVM con un doble del puerto, en UI), CT-10 (de punta a punta, con `PilotEraseScopeDouble`) ni CT-11 (las '
  'huellas de D2), ni las cuatro líneas de CT-03 y 04 que salen del texto, ni el RED de las CT de comportamiento.\n'
  '- **Por qué esas no se escriben literales:** §5.3 fija sus líneas y lo que hacen, no cómo se construyen (los fixtures de la cola, del conflicto '
  'y del error, el doble del borrado, las huellas). Un ensayo sería una versión de la sesión, no la CT del Worker. Lo que el ensayo no ve lo ven '
  'la CI del RED (C04) y la del GREEN (C07).' % (S['pbt4c-rehearsal-textct.cs'], S['v51/make_rehearsal_v51.py'], S['cc/census_c13_v51.py']),
  what='paso 8')
L('**Lo medido** (`/d/r5g` para los pasos 2 a 6, `/d/r5f` para el 7 y `/d/r5h` para el coste; la vuelta parada, `/d/r5a` a `/d/r5e`, es historia, '
  'con la medida de D-S9 en `/d/r5e`): lo previsto, con D = 0, K = 4, E = 4 y cero `warning CS`. **La duración de la suite de Core:** 785 s en el RED '
  'y 591 s en el GREEN. **El coste de `IsDirty`:** el de PB-T4b, dentro del ruido (pb-20-ab, mediana limpia 9.64 ms frente a 9.35 ms).',
  '**Lo medido en la v5.1** (V51-06): `/d/r5j` para los pasos 2 a 6, `/d/r5i` para las sondas del 7 y el ensayo del 8, `/d/r5k` para el coste y '
  '`/d/r5l` para el control del ensayo. Lo previsto, con D = 0, K = 4, E = 4 y cero `warning CS`:\n'
  '- **Core, sin las CT:** 19 608 = 19 147 + 0 + 461 en las dos ramas; **UI,** los 18 filtros: en el RED solo cae el equivalente de CT-PBT2B-06, '
  'por aserción, y en el GREEN, 0 `Failed`;\n'
  '- **el ensayo:** PASA en las dos ramas; **su control sobre la v5.0:** ve I-01; **el censo:** PASA; **las líneas de las sondas:** las mismas que '
  'en `/d/r5f`, por sha256;\n'
  '- **la duración de la suite de Core:** %d s en el RED y %d s en el GREEN;\n'
  '- **el coste de `IsDirty`:** el de PB-T4b, dentro del ruido (pb-20-ab, mediana limpia %s, frente a 9.35 ms en PB-T4b y 9.64 ms en la v5.0).\n\n'
  '**Las corridas de la v5.0** (`/d/r5f`, `/d/r5g` y `/d/r5h`) y la vuelta parada (`/d/r5a` a `/d/r5e`, con la medida de D-S9 en `/d/r5e`) son '
  'historia: las piezas que midieron no son las de la v5.1.' % (p6['SuiteDurationSeconds']['core_red'], p6['SuiteDurationSeconds']['core_green'],
                                                                p6['IsDirtyCost']['Pb20AbCleanMedian']),
  what='lo medido')
L('(medido en las TRX de `/d/r5g`: n.º 5, 85 resultados de 40 métodos)', '(medido en las TRX de `/d/r5j`: n.º 5, 85 resultados de 40 métodos)')
L('`pbt4ccc/compile/out-redgreen-v50b-r5g/filter-overlap-v50b.txt`', '`pbt4ccc/compile/r5j-rg/filtros.txt`')
L('el resumen, `pbt4c-precondition6.json`, sha256 `0cb67aee2b2d925d68235bb6444566b4222205a6cf97f472b3851fd0795b2f51`, y su registro, `compile-check.md`, '
  '`bca9fde88b3a4ac881860769260780354a4440a48a94c9690b8c08f7acaefc44`',
  'el resumen, `pbt4c-precondition6.json`, sha256 `%s`, y su registro, `compile-check.md`, `%s`' % (P6SHA, CCHECK))

# ---------------------------------------------------------------- §6.3.2
L('| **`Objective`** | §6.3.1 | ASCII, byte a byte; cita el paquete v5.0,', '| **`Objective`** | §6.3.1 | ASCII, byte a byte; cita el paquete v5.1,')
L('`v50/check_rr3_v50.py` y su salida): 0 errores;', '`v51/check_rr3_v51.py` y su salida, sobre la v5.1): 0 errores;')
L('`IsDirtyCost` y `History` (la vuelta parada). sha256 `0cb67aee2b2d925d68235bb6444566b4222205a6cf97f472b3851fd0795b2f51`; la corrida, en su '
  '`compile-check.md` (`bca9fde88b3a4ac881860769260780354a4440a48a94c9690b8c08f7acaefc44`). La carpeta va a `F4B-DRAFTS/pbt4/cc4c/` |',
  '`IsDirtyCost` y `History` (las corridas de la v5.0 y la vuelta parada); **y, desde la v5.1, con el mismo Schema** (las claves nuevas no cambian '
  'las de la v1): `Rehearsal` (el ensayo en cada rama), `Control` (el ensayo sobre la v5.0), `Census` (el censo de C13) y `ClassResults` (el sha256 '
  'de las tablas de cada clase). sha256 `%s`; la corrida, en su `compile-check.md` (`%s`), **con el resultado de cada clase que corre** (Passed, '
  'Failed y Skipped). La carpeta va a `F4B-DRAFTS/pbt4/cc4c/`; **las salidas de la v5.1 tienen nombres cortos:** ninguna ruta pasa de 150 caracteres '
  'con ese prefijo (V51-07) |' % (P6SHA, CCHECK))
L('**`BOUNDED_SECTIONS`: la §22** (la comprobación acotada de PB-T4c sobre esta versión, la v5.0; en `6adff90d`, el informe acaba en la §21, la '
  'recomprobación de la v4.9) y, si hay recomprobación, la siguiente; el revisor las escribe tras leer el paquete.',
  '**`BOUNDED_SECTIONS`: la §22** (la comprobación acotada de PB-T4c sobre la v5.0: EMITIBLE TRAS CORRECCIONES, 0/1/3) **y la §23** (su '
  'recomprobación, sobre esta versión, la v5.1).')
L('la línea BOUNDED literal que el revisor propone en su sección, en ASCII (`EMITIBLE` o `EMITIBLE TRAS CORRECCIONES`, con «B n / I n / M n»), con '
  'la cola de la recomprobación si la hay;',
  'la línea BOUNDED literal de la §22.10, en ASCII, con la cola de la recomprobación de la §23 (`; recomprobacion v5.1: EMITIBLE, B 0 / I 0 / M <n>`);')
L('  - la n.º 0, el paquete, en la v5.0, con sus secciones de PB-T4c', '  - la n.º 0, el paquete, en la v5.1, con sus secciones de PB-T4c')
L('  - la n.º 65, `independent-review-v1.md`, con su §22 (CA-T4c) y su recomprobación, si la hay.',
  '  - la n.º 65, `independent-review-v1.md`, con su §22 (CA-T4c) y su recomprobación, la §23.')
NEWROWS = (
    '| **El ensayo y el censo** (el paso 8) | `pbt4c-rehearsal-textct.cs` y `pbt4ccc/compile/census_c13_v51.py` | sha256 `%s` y `%s`; el generador del '
    'ensayo, `v51/make_rehearsal_v51.py` (`%s`); su lectura, `rehearsal_check_v51.py` (`%s`); su control, `p6_control50_v51.sh` (`%s`) y '
    '`control_check_v51.py` (`%s`). **No van al contrato:** son de la sesión, como las sondas |\n'
    % (S['pbt4c-rehearsal-textct.cs'], S['cc/census_c13_v51.py'], S['v51/make_rehearsal_v51.py'], S['cc/rehearsal_check_v51.py'],
       S['cc/p6_control50_v51.sh'], S['cc/control_check_v51.py']))
L('| **La precondición 6** | `pbt4ccc/compile/pbt4c-precondition6.json` |', NEWROWS + '| **La precondición 6** | `pbt4ccc/compile/pbt4c-precondition6.json` |')

# ---------------------------------------------------------------- §7, §9 y §11
L('\n| R-T4-16, la doble lectura del piloto |',
  '\n| **R-T4-29, las CT no corren en la precondición 6** (CA-T4c I-01; V51-05) | El paso 8: el ensayo de las CT de texto, sobre el RED y sobre el '
  'GREEN, con su control sobre la v5.0, y el censo de C13. Lo que no cubre, declarado (§6.3.1) |\n'
  '| R-T4-16, la doble lectura del piloto |')
L('- **Medidos en la copia exportada, para PB-T4c** (V50-12): K = 4, D = 0 y E = 4 (`/d/r5g`), el coste de `IsDirty` (`/d/r5h`) y las líneas de las '
  'sondas (`/d/r5f`).',
  '- **Medidos en la copia exportada, para PB-T4c** (V50-12; V51-06): K = 4, D = 0 y E = 4 (`/d/r5j`), el coste de `IsDirty` (`/d/r5k`), las líneas '
  'de las sondas y el ensayo de las CT de texto (`/d/r5i`), y el control del ensayo sobre la v5.0 (`/d/r5l`); los de la v5.0 (`/d/r5f`, `/d/r5g` y '
  '`/d/r5h`), como historia. **Lo que el ensayo no cubre:** las CT de comportamiento salvo por las sondas (§6.3.1).')
L('   - **PB-T4c** (los ocho pasos de §6.3.1; `pbt4ccc/compile/compile-check.md`), sobre copias exportadas del cierre de PB-T4b (`6adff90d`): '
  '`apply_red_pbt4c_v51.py`, la compilación sin ningún `warning CS`, `check_skips_pbt4c_v50.py` y los filtros;',
  '   - **PB-T4c** (los nueve pasos de §6.3.1; `pbt4ccc/compile/compile-check.md`), sobre copias exportadas del cierre de PB-T4b (`6adff90d`): '
  '`apply_red_pbt4c_v51.py`, la compilación sin ningún `warning CS`, `check_skips_pbt4c_v50.py` y los filtros;')
L('las sondas de las líneas de las CT y el coste de `IsDirty`; **K = 4, D = 0 y E = 4**, y la duración de la suite. Medido (`pbt4c-precondition6.json`, '
  '`0cb67aee2b2d925d68235bb6444566b4222205a6cf97f472b3851fd0795b2f51`).',
  'las sondas de las líneas de las CT y el coste de `IsDirty`; **el ensayo de las CT de texto** (PASA en las dos ramas, y su control sobre la v5.0 '
  've I-01) **y el censo de C13** (PASA); **K = 4, D = 0 y E = 4**, y la duración de la suite. Medido en la v5.1 (`pbt4c-precondition6.json`, `%s`).'
  % P6SHA)
L('**En el commit de emisión de PB-T4c,** además: esta versión (v5.0), los fragmentos de `v50/`,',
  '**En el commit de emisión de PB-T4c,** además: esta versión (v5.1) y la v5.0, los fragmentos y los guiones de `v50/` y de `v51/`,')
L('con la vuelta parada como historia y la medida de D-S9 de `v50/variant-rename/`)',
  'las de la v5.1 en `r5i-en/`, `r5j-rg/`, `r5k-id/` y `r5l-cc/`, y como historia las de la v5.0, la vuelta parada y la medida de D-S9 de '
  '`v50/variant-rename/`), el ensayo de las CT de texto y el censo de C13 con sus salidas')
live = live.replace('`/d/r5g`', '`/d/r5g`')  # las citas que quedan de /d/r5g son de la v5.0 o de su historia: se dejan

t = head_part + live

# ---------------------------------------------------------------- el anexo V51-A, en lugar del V50-A
A0 = tail_part.index('\n## Anexo V50-A. ')
annex = ['## Anexo V51-A. El texto literal de PB-T4c: el GREEN, el esqueleto y los equivalentes (v5.1)', '',
         'Copiado por `make_v51.py`; sustituye al anexo V50-A de la v5.0. **Lo que manda es el sha256 de cada pieza** (§6.3.2); este anexo es para '
         'leerlo sin generar nada. Frente a la v5.0 (V51-01 y V51-04): sin `#nullable enable` en los dos archivos de `Draft/` (las dos piezas), sin '
         'el `?` de `ProjectOf` y con el comentario nuevo de `PendingWork()` (el GREEN), y la cabecera de cada pieza.', '',
         '### La pieza del GREEN, `pbt4c-green-v51.txt` (sha256 `%s`)' % S['pbt4c-green-v51.txt'], '', '```text',
         open(os.path.join(HERE, 'pbt4c-green-v51.txt'), encoding='utf-8').read().rstrip('\n'), '```', '',
         '### La pieza del esqueleto, `pbt4c-skeleton-v51.txt` (sha256 `%s`)' % S['pbt4c-skeleton-v51.txt'], '', '```text',
         open(os.path.join(HERE, 'pbt4c-skeleton-v51.txt'), encoding='utf-8').read().rstrip('\n'), '```', '']
for p, title in ((mk.E1, 'PEG:155-163'), (mk.E2, 'CT-PBT1B-15'), (mk.E4, 'CT-PBT2B-06')):
    annex += ['### El equivalente de %s, `%s` (`EquivalentSha` `%s`)' % (title, p, EQSHA[p]), '', '```csharp', EQ[p].rstrip('\n'), '```', '']
annex += ['### El equivalente de CT-PBT4B-20, `%s` (`EquivalentSha` `%s`)' % (mk.E3, EQSHA[mk.E3]), '',
          'Es `tests/RackCad.Tests/WorkspaceFullCopyGuardTests.cs` de `6adff90d` entero (%d líneas), con dos cambios y nada más:'
          % mk.show(REPO, REV, mk.FCG).count('\n'), '',
          '1. la clase: `%s` → `%s`;' % (mk.FCG_CLASS_OLD.strip(), mk.FCG_CLASS_NEW.strip()), '', '2. `Restore`:', '', '```csharp',
          mk.FCG_RESTORE_OLD.rstrip('\n'), '```', '', 'pasa a:', '', '```csharp', mk.fcg_restore_new().rstrip('\n'), '```', '']
t = t + tail_part[:A0] + '\n' + '\n'.join(annex) + '\n'

# ---------------------------------------------------------------- comprobaciones y escritura
for k_ in KEEP:
    assert t.count(k_) == 1, 'PB-T4a o PB-T4b cambiaron'
assert [l for l in t.split('\n') if re.match(r'\| \*\*\d\d\*\* \| ', l)] == CTROWS, 'la tabla de las CT cambio'
blk = t[t.index('\n#### 6.3.1 '):t.index('\n#### 6.3.2 ')]
code = blk[blk.index('```text\n') + 8:blk.index('\n```', blk.index('```text\n'))]
assert all(ord(ch) < 0x80 for ch in code) and 'v5.1' in code and 'pbt4c-green-v51.txt' in code and 'v50' not in code.replace('pbt4c_skips_v50', '').replace(
    'check_skips_pbt4c_v50', '')
live_now = t[t.index('\n## La cabecera de la v4.9'):t.index('\n## Anexo V49-A')]
for stale in ('pbt4c-green-v50.txt', 'pbt4c-skeleton-v50.txt', 'green_pbt4c_v50.py', 'apply_red_pbt4c_v50.py', 'apply_green_pbt4c_v50.py',
              'anexo V50-A', '0cb67aee2b2d925d', 'bca9fde88b3a4ac8', 'out-probes-v50-r5d', '6b304a009d05ff58', '48e097a7a579bd31'):
    assert stale not in live_now, stale
assert '## Anexo V50-A' not in t and t.count('## Anexo V51-A') == 1
assert t.count('| **V51-01** | **CA-T4c I-01** ') == 1
assert not re.search(r'@@[A-Z0-9]+@@', t)
fences = [l for l in t.split('\n') if l.startswith('```')]
assert len(fences) % 2 == 0
data = t.encode('utf-8')
assert b'\r' not in data
open(OUT, 'wb').write(data)
print('v5.1', hashlib.sha256(data).hexdigest(), len(data), 'bytes; reemplazos', len(done))
print('M-01', cats)
print('JSON', P6SHA, '| compile-check', CCHECK)
