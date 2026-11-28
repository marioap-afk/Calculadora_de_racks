# -*- coding: utf-8 -*-
"""Solo lectura: comprueba la v4.7 (la de la emision de PB-T3c, F4B-E47) frente a la v4.6 custodiada y, por objetos git, frente al cierre de PB-T3b.
- los marcadores <tras el cierre de PB-T3b>: los seis sitios de la v4.6, rellenos, cada uno con su fuente nombrada en el texto, y el hecho comprobado
  en esa fuente en el cierre (fe1c67f4); en la v4.7 solo quedan las menciones del propio marcador;
- lo que no es de PB-T3c no cambia: cada seccion de la v4.6 sigue byte a byte, salvo las que dice el registro v4.6 -> v4.7 (la cabecera, las
  precondiciones, 4.1, 5.0, 5.4, 6.0, 6.3 y 8.1); el diseno (section 3), PB-T3a y PB-T3b (5.2, 5.3, 6.1 y 6.2), intactos;
- lo que el generador de PB-T3c copia del paquete al contrato (con sus funciones ascii_fold, md_fold, section, bullets y rr3_table_check): cambia solo
  el Objective (v4.7) y la linea de la CI (el total exacto de su unica rama, F4B-E46), y en el 6.0 entra la vineta del total por rama;
- la fila de CT-05 de la tabla de RR-3, enumerada: las ocho fichas de D2 y las tres reglas, en sus lineas de PushBackSharedReferenceGuardTests.cs;
- lo previsto de la CI: sin alternativa, y sus cuentas cuadran con la linea base;
- la precondicion 12 (historia) y la 13, el section 5.0 (M-07 de la seccion 13 de CA-T3b) y la numeracion (F4B-E47, 274, 275, R-23);
- sin rutas locales.
Uso (desde hs65/pbt3): python -I check_v47.py <checkout principal>   (lee el cierre de PB-T3b solo con git show y git rev-parse; el checkout va por
argumento: el guion no lleva rutas locales)"""
import difflib, hashlib, os, re, subprocess, sys, unicodedata

REPO = sys.argv[1]
CLOSE, GREEN = 'fe1c67f4efa23f2abb9b6ab9d636df54ae15f343', 'c758278548bcdd66e7c55ca15ca1faeb1a431e53'
HERE = os.path.dirname(os.path.abspath(__file__))
V46_RAW = open(os.path.join(HERE, 'I-64-f4b-pbt3-gate-contract-package-v46.md'), 'rb').read()
V47_RAW = open(os.path.join(HERE, 'I-64-f4b-pbt3-gate-contract-package-v47.md'), 'rb').read()
assert hashlib.sha256(V46_RAW).hexdigest().startswith('09210abe'), 'la v4.6 no es la custodiada'
V46, V47 = V46_RAW.decode('utf-8'), V47_RAW.decode('utf-8')
assert '\r' not in V46 and '\r' not in V47
GEN = 'make_pbt3c_contract.py'
gen = open(os.path.join(HERE, GEN), encoding='ascii').read()
PILOT = 'docs/automation/evidence/I-64-pilot'
ROWS = PILOT + '/F4B-DRAFTS/pbt3/nc3-row-count-pbt3b/count-rows-out.txt'


def git(*a):
    r = subprocess.run(['git', '-C', REPO] + list(a), capture_output=True)
    assert r.returncode == 0, a
    return r.stdout


def show(path):
    return git('show', CLOSE + ':' + path).decode('utf-8').replace('\r\n', '\n')


assert git('rev-parse', CLOSE[:8]).decode('ascii').strip() == CLOSE and git('rev-parse', CLOSE + '^').decode('ascii').strip() == GREEN
assert hashlib.sha256(git('show', CLOSE + ':docs/initiatives/I-64-f4b-pbt3-gate-contract-package.md')).hexdigest() == hashlib.sha256(V46_RAW).hexdigest(), \
    'el paquete custodiado en el cierre de PB-T3b no es esta v4.6'

# las funciones del generador de PB-T3c: el plegado, las secciones, las vinetas y la tabla de RR-3
ns = {'re': re, 'unicodedata': unicodedata}
exec(compile(gen[gen.index('def ascii_fold(s):'):gen.index('\n\n\nVARIANT = ')], 'fold', 'exec'), ns)
exec(compile(gen[gen.index('def section(head_prefix):'):gen.index('# ---- rr_pkg_check (inicio)')], 'sec', 'exec'), ns)
exec(compile(gen[gen.index('# ---- rr3_table_check (inicio)'):gen.index('# ---- rr3_table_check (fin)')], 'rr3', 'exec'), ns)


def secs(text, head):
    ns['PKG_LINES'] = text.split('\n')
    return ns['section'](head)


R = ['# check_v47.py: la v4.7 (sha256 %s, %d bytes) frente a la v4.6 (sha256 %s) y al cierre de PB-T3b (%s), por objetos git; funciones de %s (sha256 %s)'
     % (hashlib.sha256(V47_RAW).hexdigest()[:16], len(V47_RAW), hashlib.sha256(V46_RAW).hexdigest()[:16], CLOSE[:8], GEN,
        hashlib.sha256(gen.encode('ascii')).hexdigest()[:16]), '']

# ---------------------------------------------------------------- 1. los marcadores
MARK = '<tras el cierre de PB-T3b>'
L46, L47 = V46.split('\n'), V47.split('\n')
m46 = [i for i, l in enumerate(L46) if MARK in l]
m47 = [i for i, l in enumerate(L47) if MARK in l]
MENTIONS46 = ('**Dónde está PB-T3, medido en `eab27a9e`:**', '| V46-05 |')
MENTIONS47 = ('**Estado:** v4.7', '| **V47-01**', '| V46-05 |')
assert sum(l.count(MARK) for l in L46) == 8 and len(m46) == 8
fills46 = [i for i in m46 if not L46[i].startswith(MENTIONS46)]
assert len(fills46) == 6, len(fills46)
assert len(m47) == 3 and all(L47[i].startswith(MENTIONS47) for i in m47), [L47[i][:40] for i in m47]
R.append('1. los marcadores %s: en la v4.6, %d en %d lineas (%d sitios por rellenar y %d menciones); en la v4.7, %d, todas menciones (%s)'
         % (MARK, sum(l.count(MARK) for l in L46), len(m46), len(fills46), len(m46) - len(fills46), len(m47), ', '.join(L47[i][:16] for i in m47)))
# el sitio, el prefijo de la linea de la v4.6, las lineas de la v4.7 que lo rellenan, lo que dicen y su fuente
_disp = show(PILOT + '/F4B-DRAFTS/f4b-dispositions.md')
_e46 = _disp[_disp.index('\n## F4B-E46. '):]
_rows = show(ROWS)
_green = _rows[_rows.index('rv/ci-core/core.trx'):_rows.index('rv/red-ci-core/core.trx')]
_evid = show('docs/automation/evidence/I-64-evidence.md')
_evh = [int(x) for x in re.findall(r'(?m)^## (\d+)\. ', _evid)]
_disph = re.findall(r'(?m)^## (F4B-E\d+)\. ', _disp)
_meas = show('docs/initiatives/I-64-f4b-pbt3-measurements.md')
_measr = re.findall(r'(?m)^## (R-\d+)\. ', _meas)
_c3b = __import__('json').loads(show(PILOT + '/F4B-PBT3B-FULL-WRITE-RUN/coordinator/gate-contract.json'))
_ws = [r['MinSelected'] for r in _c3b['RequiredTests'] if r['Filter'] == 'FullyQualifiedName~RackCad.Tests.Workspace']
_sweep = open(os.path.join(HERE, 'sweep-pbt3c-close.txt'), encoding='utf-8').read()
SITES = [
    ('precondicion 1: la BaseSha y el estado', '   - **En `eab27a9e`** (V46-05):',
     ['   - **En `eab27a9e`** (V46-05):', '   - **En `fe1c67f4`** (V47-01):'],
     ['**La `BaseSha` de PB-T3c es la BASE de su emisión, sobre el cierre de PB-T3b** (V47-01)', 'hija directa del GREEN `c7582785`', 'RED `9fda277e`',
      'X `2bb3b759` y BASE `c7441653`', 'intento 2 de 3', 'verificación 14/14', 'Core 19 124 resultados, con 19 118 `Passed` y 6 `Skipped`',
      'UI 2 451, con 2 430 `Passed` y 21 `Skipped`', 'Fuente: `f4b-dispositions.md` (F4B-E46) y `F4B-DRAFTS/pbt3/nc3-row-count-pbt3b/count-rows-out.txt`'],
     'f4b-dispositions.md (F4B-E46, la ultima entrada) y count-rows-out.txt',
     [_disph[-1] == 'F4B-E46', GREEN[:8] in _e46, '9fda277e' in _e46, 'attempts 2 de 3' in _e46, '44707482956' in _e46,
      'Core 19 124 (6 omitidas, las de D-1) y UI 2 451 (2 430 pasadas y 21 omitidas)' in _e46,
      _green.startswith("rv/ci-core/core.trx {'Passed': 19118, 'NotExecuted': 6} 19124")]),
    ('precondicion 9: la custodia de la emision de PB-T3c', '   - **en la emisión de PB-T3c** (V46-05):',
     ['   - **en la emisión de PB-T3c** (F4B-E47; V46-05 y V47-01):'],
     ['el paquete en su versión v4.7, sobre la v4.6 que custodió `2bb3b759`', '`v47-delta.md`, `make_v47.py`, `check_v47.py` y su salida',
      '`pbt3c-anchors-out.txt`, con `measure_pbt3c.py`, y R-23 del registro', '`pbt3c-skeleton.patch`, con `make_pbt3c_skeleton.py`',
      '`compile-check-pbt3c/`, con sus registros redactados y su manifiesto',
      'La evidencia: 274 (X) y 275 (BASE).'],
     'f4b-dispositions.md, I-64-evidence.md y el registro de mediciones en el cierre (las ultimas: F4B-E46, 273 y R-22); la v4.6 en 2bb3b759',
     [_disph[-1] == 'F4B-E46' and 'F4B-E47' not in _disph, max(_evh) == 273, _measr[-1] == 'R-22',
      hashlib.sha256(git('show', '2bb3b759:docs/initiatives/I-64-f4b-pbt3-gate-contract-package.md')).hexdigest() == hashlib.sha256(V46_RAW).hexdigest()]),
    ('section 4.1: el barrido de las guardas sobre el cierre', '- **las guardas que añade PB-T3b** (CT-PBT3B-13 y CT-PBT3B-14)',
     ['- **las guardas que añade PB-T3b** (CT-PBT3B-13 y CT-PBT3B-14)'],
     ['**Repetido sobre el cierre de PB-T3b** (`fe1c67f4`; V47-01; `guard_sweep.py` y `sweep_t3b_diff.py`, la salida en `sweep-pbt3c-close.txt`)',
      'de 92 a 94 clases que leen código', '9 coincidencias nuevas', 'ninguna se pierde', '**Sin colisión.**'],
     'sweep-pbt3c-close.txt (guard_sweep.py y sweep_t3b_diff.py, custodiados en el cierre)',
     ['antes: scanned guard files: 92' in _sweep, 'despues: scanned guard files: 94' in _sweep, 'filas del corpus nuevas: 9 | perdidas: 0' in _sweep,
      bool(git('cat-file', '-t', CLOSE + ':' + PILOT + '/F4B-DRAFTS/pbt3/guard_sweep.py')),
      bool(git('cat-file', '-t', CLOSE + ':' + PILOT + '/F4B-DRAFTS/pbt3/sweep_t3b_diff.py'))]),
    ('section 6.3: los MinSelected de las guardas del escritor', '  | Core | `WorkspacePilotUpdateGuardTests`, `WorkspacePilotUpdateIntegrationGuardTests` |',
     ['  | Core | `WorkspacePilotUpdateGuardTests`, `WorkspacePilotUpdateIntegrationGuardTests` |'],
     ['**86, 123**: los `Passed` de cada clase en la CI del GREEN de PB-T3b (`44707482956`), sin los `Skip` de D-1 (1 y 2)'],
     'count-rows-out.txt (la TRX del GREEN de PB-T3b)',
     ["('WorkspacePilotUpdateGuardTests', 'Passed') 86" in _green, "('WorkspacePilotUpdateIntegrationGuardTests', 'Passed') 123" in _green,
      "('WorkspacePilotUpdateGuardTests', 'NotExecuted') 1" in _green, "('WorkspacePilotUpdateIntegrationGuardTests', 'NotExecuted') 2" in _green]),
    ('section 6.3: la nota de lo heredado de 6.2', '  - **Nota (no va al contrato).** Los `MinSelected` de lo heredado de §6.2',
     ['  - **Nota (no va al contrato).** Los `MinSelected` de lo heredado de §6.2'],
     ['ninguno baja del mínimo del contrato reemitido de PB-T3b (`Workspace` de Core, 3 261: 3 271 − 10 de D-1)', '`make_pbt3c_contract-dry.txt`',
      '`F4B-DRAFTS/pbt3/nc3-row-count-pbt3b/count-rows-out.txt`'],
     'el contrato reemitido de PB-T3b en el cierre (Workspace de Core) y el seco del generador',
     [_ws == [3261], _c3b['AuthorityRevision'].startswith('2bb3b759')]),
    ('section 6.3: la linea base de la CI y lo previsto', '- **Línea base de la CI:**', ['- **Línea base de la CI:**'],
     ['run `44707482956` sobre el GREEN `c7582785`, Core **19 124** resultados, con 19 118 `Passed` y 6 `Skipped`', 'UI **2 451**, con 2 430 `Passed` y 21 `Skipped`',
      '`F4B-DRAFTS/pbt3/nc3-row-count-pbt3b/count-rows-out.txt`', 'hay **una sola rama**',
      '**El RED:** Core **19 129** resultados, con 19 119 `Passed`, **4** `Failed` (CT-01 a 04) y 6 `Skipped`.',
      '**El GREEN:** Core **19 129**, con 19 123 `Passed`, 0 `Failed` y 6 `Skipped`.', '**UI, en los dos:** **2 451**, con 2 430 `Passed` y 21 `Skipped`.'],
     'F4B-E46 y count-rows-out.txt',
     ['44707482956' in _e46, _green.startswith("rv/ci-core/core.trx {'Passed': 19118, 'NotExecuted': 6} 19124"), 'GREEN 44707482956 core.trx ' in _rows]),
]
S46 = set(L46)
for name, pre46, pre47, needles, src, facts in SITES:
    a = [i for i in fills46 if L46[i].startswith(pre46)]
    assert len(a) == 1, (name, len(a))
    lines = []
    for p in pre47:
        b = [l for l in L47 if l.startswith(p) and l not in S46]  # las lineas nuevas o cambiadas
        assert len(b) == 1, (name, p, len(b))
        lines.append(b[0])
    txt = '\n'.join(lines)
    miss = [n for n in needles if n not in txt]
    assert MARK not in txt and not miss, (name, miss)
    assert all(facts), (name, facts)
    R.append('   %s: relleno (lineas de la v4.7: %d%s; frases comprobadas: %d); fuente: %s -> %d de %d hechos comprobados en %s'
             % (name, len(lines), "", len(needles), src, sum(facts), len(facts), CLOSE[:8]))

# ---------------------------------------------------------------- 2. lo que no es de PB-T3c no cambia
def blocks(text):
    out, cur, key = {}, [], '(cabecera)'
    for l in text.split('\n'):
        if l.startswith(('## ', '### ')):
            out[key] = '\n'.join(cur)
            key, cur = l, []
        cur.append(l)
    out[key] = '\n'.join(cur)
    return out


B46, B47 = blocks(V46), blocks(V47)
_title47 = [k for k in B47 if k.startswith('## Registro de cambios v4.6 → v4.7')]
assert len(_title47) == 1 and set(B47) - set(B46) == set(_title47) and set(B46) <= set(B47), (set(B47) ^ set(B46))
CHANGED = {'(cabecera)', '## Registro de cambios v1 → v2 (la revisión independiente de la v1 y las lecciones nuevas de PB-T1b)'}
CHANGED |= {k for k in B46 if k.startswith(('### 4.1 ', '### 5.0 ', '### 5.4 ', '### 6.0 ', '### 6.3 ', '### 8.1 '))}
assert len(CHANGED) == 8
changed = sorted(k for k in B46 if B46[k] != B47[k])
assert set(changed) == CHANGED, (set(changed) ^ CHANGED)
same = [k for k in B46 if B46[k] == B47[k]]
R.append('')
R.append('2. secciones: %d en la v4.6, %d en la v4.7 (la nueva: el registro v4.6 -> v4.7); %d iguales byte a byte; cambian solo las %d previstas: %s'
         % (len(B46), len(B47), len(same), len(changed), '; '.join(ns['ascii_fold'](k)[:28] for k in changed)))
for must in ('### D-PBT3-', '### 5.2 ', '### 5.3 ', '### 6.1 ', '### 6.2 ', '## 2. ', '### 8.2 '):
    ks = [k for k in B46 if k.startswith(must)]
    assert ks and all(B46[k] == B47[k] for k in ks), must
R.append('   intactas: el diseno (las %d D-PBT3), 5.2 y 5.3 (PB-T3a y PB-T3b), 6.1 y 6.2 (sus contratos), section 2 y 8.2'
         % len([k for k in B46 if k.startswith('### D-PBT3-')]))
# en el bloque de las precondiciones, lo de antes de ellas es igual; cambian la 1 (una vineta nueva), la 9, la 12 y entra la 13
_pk = '## Registro de cambios v1 → v2 (la revisión independiente de la v1 y las lecciones nuevas de PB-T1b)'
_cut = '**Precondiciones de emisión**'
assert B46[_pk][:B46[_pk].index(_cut)] == B47[_pk][:B47[_pk].index(_cut)]
d = [l for l in difflib.unified_diff(B46[_pk].split('\n'), B47[_pk].split('\n'), lineterm='', n=0) if l.startswith(('+', '-')) and not l.startswith(('+++', '---'))]
R.append('   las precondiciones: lo de antes, igual; %d lineas quitadas y %d anadidas (la 1, la 9, la 12 y la 13 nueva)'
         % (sum(1 for l in d if l[0] == '-'), sum(1 for l in d if l[0] == '+')))
for l in d:
    assert l[1:].startswith(('   - **En `eab27a9e`**', '   - **En `fe1c67f4`**', '   - **en la emisión de PB-T3c**', '    - **la compilación de comprobación**',
                             '13. **Para PB-T3c', '    - **el barrido de sobrecargas', '    - **la comprobación acotada de la v4.7**')), l[:60]
# en las demas secciones que cambian, las lineas quitadas y anadidas, con lo que se espera de cada una (vacias aparte)
EXPECT_SEC = {
    '### 4.1 ': (['- **las guardas que añade PB-T3b**'],
                 ['- **las guardas que añade PB-T3b**', '**El barrido de sobrecargas, de reflexión y de nombres de PB-T3c**', '- **`overload_sweep_t3c.py`**',
                  '  - **T3C-1, ', '  - **la reflexión sobre los tipos tocados**', '  - **los nombres nuevos:**', '  - **la regresión de PB-T3b:**',
                  '- **`name_check_t3c.py`**']),
    '### 5.0 ': (['**Por eso los métodos públicos nuevos'], ['**Por eso los métodos públicos nuevos', '**El total exacto de cada rama** (V47-03;']),
    '### 5.4 ': ([], ['**Una prueba, una fila** (V47-03)', '- **Nota (no va al contrato).** Las lecciones de la revisión independiente de PB-T3b']),
    '### 6.0 ': (['| PB-T3c | `PushBackWritePathPluginGuardTests` (CT-05) |'],
                 ['| PB-T3c | `PushBackWritePathPluginGuardTests` (CT-05) |', '- **El total exacto de cada rama** (F4B-E46; V47-03)']),
    '### 6.3 ': (['- **Objective** (ASCII):', '  | Core | `WorkspacePilotUpdateGuardTests`', '  - **Nota (no va al contrato).** Los `MinSelected` de lo heredado',
                  '- **Línea base de la CI:**'],
                 ['- **Objective** (ASCII):', '  | Core | `WorkspacePilotUpdateGuardTests`', '  - **Nota (no va al contrato).** Los `MinSelected` de lo heredado',
                  '- **Línea base de la CI:**']),
    '### 8.1 ': ([], ['- **F4B-E46, el cierre de PB-T3b']),
}
for pre, (want_minus, want_plus) in EXPECT_SEC.items():
    k = [x for x in B46 if x.startswith(pre)][0]
    d = [l for l in difflib.unified_diff(B46[k].split('\n'), B47[k].split('\n'), lineterm='', n=0)
         if l.startswith(('+', '-')) and not l.startswith(('+++', '---')) and l[1:].strip()]
    minus, plus = [l[1:] for l in d if l[0] == '-'], [l[1:] for l in d if l[0] == '+']
    assert len(minus) == len(want_minus) and all(m.startswith(w) for m, w in zip(minus, want_minus)), (pre, [m[:50] for m in minus])
    assert len(plus) == len(want_plus) and all(p.startswith(w) for p, w in zip(plus, want_plus)), (pre, [p[:50] for p in plus])
    R.append('   %s: %d lineas quitadas y %d anadidas, las previstas' % (ns['ascii_fold'](k)[:40], len(minus), len(plus)))
_dbl = [l for l in L46 if l.startswith('| PB-T3b | `PilotEraseScopeDouble`')]
assert len(_dbl) == 1 and _dbl[0] in S46 and L47.count(_dbl[0]) == 1
R.append('   la fila del doble de PB-T3b en la tabla de RR-3 (PilotEraseScopeDouble), igual; el generador de PB-T3c solo lee las filas "| PB-T3c |"')

# ---------------------------------------------------------------- 3. lo que el generador de PB-T3c copia
R.append('')


def copied(text):
    s60, s63 = secs(text, '### 6.0 '), secs(text, '### 6.3 Contrato de PB-T3c')
    b60, b63, b63_2 = ns['bullets'](s60, 0), ns['bullets'](s63, 0), ns['bullets'](s63, 2)
    q = [l for l in s60 if l.startswith('  > ')]
    assert len(q) == 1
    ev0 = [i for i, b in enumerate(b63_2) if b.startswith('lo de section 6.1, ')]
    assert len(ev0) == 1
    c = ([b for b in b63 if b.startswith(('Objective (ASCII)', 'Linea base de la CI'))] + [b for b in b63_2 if b.startswith(('INV-PBT3C-', 'PBT3C-C'))] +
         b63_2[ev0[0]:] + [b for b in b60 if re.match(r'^(0[7-9]|1[0-4])\. |^C\d\d(: | \()', b)] + [ns['ascii_fold'](q[0][4:]).replace('`', '')])
    return c, b60, b63, b63_2


c46, b60_46, b63_46, b63_2_46 = copied(V46)
c47, b60_47, b63_47, b63_2_47 = copied(V47)
assert len(c46) == len(c47) == 34
diffc = [(x, y) for x, y in zip(c46, c47) if x != y]
assert [y.split(' ')[0] for _, y in diffc] == ['Objective', 'Linea'], [y[:30] for _, y in diffc]
R.append('3. lo que el generador copia al contrato (34 textos: el Objective, la linea de la CI, INV-PBT3C-01 a 04, sus 4 paradas, su evidencia, los comunes '
         'del 6.0 y PBT3x-C11): cambian %d, el Objective y la linea de la CI' % len(diffc))
_o46, _o47 = diffc[0]
assert _o46.replace('PB-T3 v4.6', 'PB-T3 v4.7') == _o47, 'el Objective cambia algo mas que la version'
R.append('   el Objective: solo "PB-T3 v4.6" -> "PB-T3 v4.7"')
for (name, b46, b47) in (('6.0, nivel 0', b60_46, b60_47), ('6.3, nivel 0', b63_46, b63_47), ('6.3, nivel 2', b63_2_46, b63_2_47)):
    new = [x for x in b47 if x not in b46]
    gone = [x for x in b46 if x not in b47]
    R.append('   vinetas de %s: %d -> %d; nuevas o cambiadas %d, quitadas %d' % (name, len(b46), len(b47), len(new), len(gone)))
    for x in new:
        R.append('      + ' + x[:120])
    want = {'6.0, nivel 0': ['El total exacto de cada rama (F4B-E46; V47-03)'], '6.3, nivel 0': ['Objective (ASCII)', 'RequiredTests: ', 'Linea base de la CI'],
            '6.3, nivel 2': []}[name]
    assert all(any(x.startswith(w) for w in want) for x in new) and len(gone) == len(new) - (1 if name.startswith('6.0') else 0), (name, [x[:50] for x in new])
_req46 = [b for b in b63_46 if b.startswith('RequiredTests: ')][0]
_req47 = [b for b in b63_47 if b.startswith('RequiredTests: ')][0]
assert _req46.replace('los medidos tras PB-T3b: <tras el cierre de PB-T3b>',
                      '86, 123: los Passed de cada clase en la CI del GREEN de PB-T3b (44707482956), sin los Skip de D-1 (1 y 2)') == _req47
R.append('   RequiredTests: solo la fila de las guardas del escritor (el marcador -> "86, 123", con su fuente)')

# ---------------------------------------------------------------- 4. la tabla de RR-3: la fila de CT-05
R.append('')
CLS = ['PushBackViewEraseGuardTests', 'PushBackWritePathPluginGuardTests']
e46, i46 = ns['rr3_table_check'](secs(V46, '### 6.0 '), CLS)
e47, i47 = ns['rr3_table_check'](secs(V47, '### 6.0 '), CLS)
assert not e46 and not e47, (e46, e47)
assert i46['PushBackWritePathPluginGuardTests'] is None and i46['PushBackViewEraseGuardTests'] == i47['PushBackViewEraseGuardTests']
D2 = ['WorkingBaseline', 'workingBaseline', 'LastModuleReconciliation', 'computation.System', 'lastComputation.System', 'session.System',
      'lastCompositeSystem', 'SystemToInsert']
ct05 = i47['PushBackWritePathPluginGuardTests']
assert ct05 == (D2 + ['Rule1', 'Rule1b', 'Rule1c'], []), ct05
srg = show('tests/RackCad.Tests/PushBackSharedReferenceGuardTests.cs').split('\n')
_tok = re.findall(r'"([^"]+)"', srg[34] + srg[35])
assert _tok == D2, _tok
assert all(('private static List<string> %s(' % r) in srg[k - 1] for k, r in ((57, 'Rule1'), (60, 'Rule1b'), (68, 'Rule1c')))
R.append('4. RR-3, filas de PB-T3c: PushBackViewEraseGuardTests igual (%d lineas, %d valores); PushBackWritePathPluginGuardTests: en la v4.6 aplazada, en la '
         'v4.7 %d literales (las 8 fichas de D2, iguales a PushBackSharedReferenceGuardTests.cs:35-36 en %s, y Rule1, Rule1b y Rule1c, en sus lineas 57, 60 '
         'y 68)' % (len(i47['PushBackViewEraseGuardTests'][0]), len(i47['PushBackViewEraseGuardTests'][1]), len(ct05[0]), CLOSE[:8]))

# ---------------------------------------------------------------- 5. lo previsto de la CI: una rama, sin alternativa, con sus cuentas
ci = [b for b in b63_47 if b.startswith('Linea base de la CI')][0]
prev = ci.split('Previstos,')[1]
assert ' o ' not in prev, 'lo previsto lleva una alternativa'
nums = lambda s: int(s.replace(' ', ''))
m = re.search(r'Core (\d[\d ]*) resultados, con (\d[\d ]*) Passed y (\d+) Skipped', ci)
base_core, base_pass, base_skip = nums(m.group(1)), nums(m.group(2)), int(m.group(3))
m = re.search(r'El RED: Core (\d[\d ]*) resultados, con (\d[\d ]*) Passed, (\d+) Failed \(CT-01 a 04\) y (\d+) Skipped', ci)
red = tuple(nums(x) for x in m.groups())
m = re.search(r'El GREEN: Core (\d[\d ]*), con (\d[\d ]*) Passed, (\d+) Failed y (\d+) Skipped', ci)
grn = tuple(nums(x) for x in m.groups())
m = re.search(r'UI, en los dos: (\d[\d ]*), con (\d[\d ]*) Passed y (\d+) Skipped', ci)
ui = tuple(nums(x) for x in m.groups())
assert base_core == base_pass + base_skip == 19124 and base_skip == 6
assert red == (base_core + 5, base_pass + 1, 4, base_skip) and red[0] == red[1] + red[2] + red[3]
assert grn == (base_core + 5, base_pass + 5, 0, base_skip) and grn[0] == grn[1] + grn[2] + grn[3]
assert ui == (2451, 2430, 21) and ui[0] == ui[1] + ui[2]
R.append('5. la CI: base Core %d = %d Passed + %d Skipped; RED %d = %d + %d Failed + %d; GREEN %d = %d + %d + %d; UI %d = %d + %d en los dos; una sola rama, '
         'sin alternativa (F4B-E46)' % ((base_core, base_pass, base_skip) + red + grn + ui))

# ---------------------------------------------------------------- 6. M-07, la numeracion y el total por rama, en el texto
R.append('')
NEED = [
    ('5.0, M-07', '### 5.0 ', '**ejecutando la suite completa de Core sobre esa copia** (V47-02; CA-T3b §13 M-07)'),
    ('5.0, el total por rama', '### 5.0 ', '**El total exacto de cada rama** (V47-03; la decisión del Owner F4B-E46, sobre el nc3 de PB-T3b).'),
    ('5.4, una prueba una fila', '### 5.4 ', '**Una prueba, una fila** (V47-03): CT-PBT3C-01 a 05 son **un `[Fact]` cada una**, sin `Theory`.'),
    ('5.4, las lecciones de las recetas', '### 5.4 ', 'PBT3B-IR-M02: los números formateados dependen de la cultura, y se fijan con `InvariantCulture`'),
    ('8.1, F4B-E46', '### 8.1 ', '- **F4B-E46, el cierre de PB-T3b, con la decisión del Owner sobre su nc3**'),
]
for name, head, n in NEED:
    assert n in '\n'.join(secs(V47, head)), name
pre = B47[_pk]
for n in ('19 107 de 19 107 con los tres nombres (V47-02; CA-T3b §13 M-07; `F4B-DRAFTS/pbt3/compile-check-pbt3b/compile-check.md` y su `out-run3.txt`)',
          '**y la suite completa de Core ejecutada sobre esa copia**, en verde', '(`pbt3c-skeleton.patch`)', '(CA-T3c) da 0 bloqueantes y 0 importantes'):
    assert n in pre, n
_cc = show(PILOT + '/F4B-DRAFTS/pbt3/compile-check-pbt3b/compile-check.md')
_r3 = show(PILOT + '/F4B-DRAFTS/pbt3/compile-check-pbt3b/out-run3.txt')
assert '**19 107 de 19 107 pasadas, 0 fallidas y 0 omitidas**' in _cc and "'total': '19107', 'executed': '19107', 'passed': '19107', 'failed': '0'" in _r3
_ideas = show('docs/ideas-futuras.md')
assert all(('- **PBT3B-IR-M0%d:**' % k) in _ideas for k in (1, 2, 7))
_own = show(PILOT + '/F4B-DRAFTS/coordinator/owner-pbt3b-nc3-decision.md')
assert '«Aceptar por disposición (Recomendado)»' in _own and 'el total exacto de cada rama' in _own
for n in ('La emisión de PB-T3c es **F4B-E47**, con la evidencia 274 (X) y 275 (BASE), y la remedición, R-23 del registro.',):
    assert n in V47, n
R.append('6. en el texto: ' + '; '.join(x for x, _, _ in NEED) + '; la precondicion 12 (19 107 de 19 107, comprobado en compile-check.md y out-run3.txt del '
         'cierre) y la 13; las fuentes de V47-03 y V47-06 en el cierre (owner-pbt3b-nc3-decision.md, PBT3B-IR-M01, M02 y M07 de ideas-futuras.md); la '
         'numeracion F4B-E47, 274, 275 y R-23')

# ---------------------------------------------------------------- 7. los tramos y las rutas
R.append('')
d = list(difflib.unified_diff(L46, L47, lineterm='', n=0))
hunks = [l for l in d if l.startswith('@@')]
R.append('7. diff v4.6 -> v4.7: %d tramos, %d lineas quitadas y %d anadidas' % (len(hunks), sum(1 for l in d if l.startswith('-') and not l.startswith('---')),
                                                                         sum(1 for l in d if l.startswith('+') and not l.startswith('+++'))))
for h in hunks:
    R.append('   ' + h)
for bad in (r'(?<![A-Za-z0-9])[A-Za-z]:[\\/]', r'\bUs[e]rs[\\/]', r'\bApp[D]ata\b', r'\bscr[a]tchpad\b', r'\bTem[p][\\/]'):
    assert not re.search(bad, V47), bad
R.append('   sin rutas locales; la v4.7: %d bytes, %d lineas, sha256 %s' % (len(V47_RAW), V47.count('\n'), hashlib.sha256(V47_RAW).hexdigest()))
out = '\n'.join(R) + '\n'
open(os.path.join(HERE, 'check-v47-out.txt'), 'w', encoding='utf-8', newline='\n').write(out)
print(out)
