# make_readme_m7.py - writes README.md of the A6-7 preparation bench from its result files (tables generated, never copied by
# hand). Stdlib only. Called by run_m7.py (group readme) or alone: python -I make_readme_m7.py
import importlib.util, json, os, sys

sys.dont_write_bytecode = True
HERE = os.path.dirname(os.path.abspath(__file__))


def _load(name):
    s = importlib.util.spec_from_file_location(name, os.path.join(HERE, name + '.py'))
    m = importlib.util.module_from_spec(s)
    s.loader.exec_module(m)
    return m


def rj(n):
    with open(os.path.join(HERE, n), 'rb') as fh:
        return json.loads(fh.read().decode('ascii'))


def cell(s):
    return str(s).replace('|', '\\|').replace('\n', ' ')


def omitted(om):
    return '; '.join('%s %s' % (t.replace('C1.', '').replace('C4.', 'C4 ').replace('C6.', 'C6 ').replace('C2.', 'C2 '), c) for t, c in om)


def acc_str(o):
    t = o.get('Table') or {}
    s = '%s rc %s; R4 %s; R9 %s' % (o.get('Decision'), o.get('Rc'), (o.get('Checks') or {}).get('R4'), (o.get('Checks') or {}).get('R9'))
    na = sorted(k for k, v in (o.get('Checks') or {}).items() if v == 'NOT_APPLICABLE')
    mm = sorted(k for k, v in (o.get('Checks') or {}).items() if v == 'MISMATCH')
    if na:
        s += '; NOT_APPLICABLE: ' + ', '.join(na)
    if mm:
        s += '; MISMATCH: ' + ', '.join(mm)
    out = sorted(set(k.split('-', 1)[1] for k in (o.get('R9Runs') or {})))
    if out:
        s += '; fuera del cierre: ' + ', '.join(out)
    s += '; cero escrituras: %s; procesos terminados: %s' % ('si' if o.get('ZeroWrite') else 'NO', 'si' if o.get('ProcessTreeDead') else 'NO')
    if t.get('Accepted'):
        s += '; tabla: aceptada'
    else:
        why = (t.get('ConstructionViolations') or []) + (t.get('ClosureViolations') or []) + (t.get('DesignationViolations') or [])
        s += '; tabla: no aceptada' + (' (' + '; '.join(w.split(':')[0] + (': ' + w.split(':')[1].strip()[:40] if ':' in w else '') for w in why[:2]) + ')' if why else '')
    return s


def derive_str(o):
    if not o.get('Derived'):
        return 'sin derivacion (falla cerrada): ' + ', '.join(sorted(set(p['Code'] for p in o.get('Problems') or o.get('DesignationProblems') or [])))
    c = o['Counts']
    s = '%d commits, %d RevPaths, %d rutas, %d archivos (%s); omitidos: %s' % (c['Commits'], c['RevPaths'], c['Paths'], c['Files'],
                                                                              ', '.join(o['Files']), omitted(o['Omitted']))
    if o.get('Excluded'):
        s += '; entradas de R7-A fuera de E: %d' % len(o['Excluded'])
    if o.get('Bypassed'):
        s += '; comprobacion de designacion: ' + ', '.join(sorted(set(p['Code'] for p in o['Bypassed']))) + ' (derivado solo para ver el efecto)'
    return s


def desig_str(o):
    s = 'valida' if o.get('CheckOk') else 'falla cerrada: ' + ', '.join(o.get('Codes') or [])
    r = o.get('Derive_r4Alone') or {}
    if r.get('Outcome') == 'exception':
        s += '; derive_r4.py solo: excepcion %s' % r.get('Type')
    elif r.get('Outcome') == 'derived':
        s += '; derive_r4.py solo: deriva (%d rutas, omitidos: %s)' % (r['Counts']['Paths'], omitted(r['Omitted']))
    return s


def obs_str(c):
    o = c['Observed']
    n = c['Case']
    if isinstance(o, dict) and 'Checks' in o and 'Decision' in o:
        s = acc_str(o)
        if o.get('ExcludedOrders') is not None:
            s += '; ordenes excluidas: ' + ', '.join(o['ExcludedOrders'])
        if o.get('DesignationWithT1m7RunPath') is not None:
            s += '; designacion con RunPath T1-m7: ' + ', '.join(o['DesignationWithT1m7RunPath'])
        return s
    if c['Group'] == 'derive':
        return derive_str(o)
    if n == 'NAME-TABLE':
        return 'P-SAME indefinido en: %s; P-OWN indefinido en: %s' % (', '.join(o['UndefinedUnderPSame']) or 'ninguno', ', '.join(o['UndefinedUnderPOwn']) or 'ninguno')
    if n == 'DES-O3-PATTERN':
        d = o.get('Diff') or {}
        return 'candidatas por patron: %d; diferencias del cierre: %s' % (len(o['PatternCandidates']), ', '.join('%s +%d/-%d' % (k, len(v['Extra']), len(v['Missing'])) for k, v in sorted(d.items())))
    if c['Group'] == 'desig':
        return desig_str(o)
    if n in ('REG-R2', 'REG-R2-STRICT', 'REG-R4-V', 'REG-R4-N4', 'REG-R4-NC1'):
        return ('bytes identicos, SHA-256 %s' % o['Sha256Built']) if o['BytesEqual'] else 'DIFIEREN: ' + json.dumps(o.get('Diff'))[:300]
    if n == 'REG-R2-DERIVE':
        return derive_str(dict(o, Derived=True)) if o.get('Derived') else str(o)
    if n == 'E-V-INSTRUMENT':
        return '; '.join('%s: +%d/-%d' % (k, len(v.get('Added', [])), len(v.get('Removed', []))) for k, v in sorted(o.items()))
    if n == 'D-V-A-vs-R4':
        return '; '.join('%s +%d/-%d' % (k, len(v['Extra']), len(v['Missing'])) for k, v in sorted(o.items())) + '; ruta de mas: ' + ', '.join(o.get('Paths', {}).get('Extra', []))
    if n == 'N6-CLEANTREE':
        return ', '.join('%s %s' % (k, o['ControllerChecks'][k]) for k in ('CleanTree', 'Identity', 'Scope', 'Trailer', 'FreeText'))
    if n == 'N7-CONTROLLER':
        return '; '.join('%s: Handoff %s, Identity %s, fila Handoff %s, Authority %s, salida %s/%s' % (k, v['Handoff'], v['Identity'], v['HandoffRow'], v['Authority'],
                                                                                                   v['Classification'][0], v['Classification'][1]) for k, v in sorted(o.items()))
    if n == 'M-A67-COVERAGE':
        return '; '.join('%s: R7-A %s, R7-B %s' % (r['Key'], r['R7A'], r['R7B']) for r in o['Rows'] if r['Orders'])
    if n == 'E-EFF-COVERAGE':
        return 'mismos blobs en AR, EFF y EFF^1: %s; distintos: %s' % ('si' if o['AllEqual'] else 'NO', ', '.join(o['Different']) or 'ninguno')
    return json.dumps(o)[:300]


def table(cases):
    out = ['| Caso | Qué | Esperado | Observado | ¿Se cumple? |', '|---|---|---|---|---|']
    for c in cases:
        out.append('| %s | %s | %s | %s | %s |' % (cell(c['Case']), cell(c['What']), cell(c['Expected']), cell(obs_str(c)), 'sí' if c['Holds'] else '**NO**'))
    return '\n'.join(out)


def main():
    L = _load('lib_r1r4')
    env = rj('p0-env.json')
    b = rj('p1-build.json')
    v = rj('p2-v.json')
    n4 = rj('p3-n4.json')
    nc1 = rj('p4-nc1.json')
    n5 = rj('p5-n5.json')
    n6 = rj('p6-n6.json')
    n7 = rj('p7-n7.json')
    m = rj('p8-m.json')
    sm = rj('summary.json')
    by = dict((c['Case'], c) for c in b['Cases'])
    grp = lambda g: [c for c in b['Cases'] if c['Group'] == g]
    vB = [c for c in v['Cases'] if c['Case'] == 'V-B-STATIC'][0]['Observed']
    mB = [c for c in m['Cases'] if c['Case'] == 'M-B-STATIC'][0]['Observed']
    files = sm['Files']
    reused = ['derive_r4.py', 'lib_r1r4.py', 'lib_r3.py', 'lib_r4.py', 'reconstruct_runs.py', 'table_r3.py', 'table_r4.py']
    new = ['make_eic_verify.py', 'designate_m7.py', 'table_m7.py', 'lib_m7.py', 'run_m7.py', 'make_readme_m7.py']
    res_files = ['p0-env.json', 'p1-build.json', 'p2-v.json', 'p3-n4.json', 'p4-nc1.json', 'p5-n5.json', 'p6-n6.json', 'p7-n7.json', 'p8-m.json']
    groups = {}
    for c in sm['Cases']:
        groups[c['Group']] = groups.get(c['Group'], 0) + 1

    def excl_lines(o):
        rows = []
        for k, vv in sorted(o['R9Runs'].items()):
            rows.append('- `%s`: %s' % (k.split('-', 1)[1], '; '.join(vv[:4]) + ('; ...' if len(vv) > 4 else '')))
        return '\n'.join(rows)

    i6v = by['D-V-R7-A']['Observed']['I6Violations']
    txt = []
    A = txt.append
    A('# Banco de la preparación de la medición de A6-7 (I-62, A-6 AGREED, decisiones §90, puntos 2 y 3)')
    A('')
    A('Banco de la supervisión para preparar la medición del Controller de FX-02 con el instrumento: el cierre efectivo de las invocaciones VERIFY '
      '(A6-7, regla 7) en dos variantes, la designación de D, H y K por las rutas de entrada del kit (A62-A6R4-O3) con fallo cerrado ante entradas '
      'fuera de D/H/K o de tipo erróneo (O2), las pruebas estáticas de N5-N7 (O6), la invocación de medición M y la identidad del intérprete y del Git '
      'declarados. Aquí solo hay pruebas, resultados y propuestas; ninguna regla se decide. No se invocó ningún modelo ni CLI de modelo (ni Codex ni '
      'claude), no hubo red ni `gh`, no se leyó `auth.json`, `config.toml`, sesiones ni credenciales, y del runtime de Codex solo se calculó el SHA-256 '
      'de su pwsh (no se ejecutó). El instrumento (`9a829b0e…`) y el aceptante (`434ef1fc…`) se usaron solo desde sus objetos Git. Todo Python corrió '
      'con `-I`, las órdenes en secuencia.')
    A('')
    A('## 1. Qué se reutiliza y qué es nuevo')
    A('')
    A('| Archivo | Origen | SHA-256 |')
    A('|---|---|---|')
    for f in reused:
        A('| `%s` | copia byte a byte de `docs/automation/evidence/I-62-A6/r4/r1probes/` (objeto Git en `400a72d8`), sin modificar | `%s` |' % (f, files[f]))
    desc = {'make_eic_verify.py': 'nuevo: constructor mecánico de E (`rackcad-input-closure/v1`) desde una designación, variantes R7-A, R7-B y R7-B-K',
            'designate_m7.py': 'nuevo: formato de designación, comprobaciones de fallo cerrado (O2, O3) y regla de rutas del kit (KPR) antes de `derive_r4.py`',
            'table_m7.py': 'nuevo: construcción y tabla de aceptación de M (sin Verification ni RelayFacts; R4 y R10 no aplicables) y cotejo de la designación',
            'lib_m7.py': 'nuevo: escenario, entradas de cada invocación, sesiones, salidas sintéticas del Controller',
            'run_m7.py': 'nuevo: punto de entrada único (`python -I run_m7.py all`)',
            'make_readme_m7.py': 'nuevo: genera este README desde los resultados'}
    for f in new:
        A('| `%s` | %s | `%s` |' % (f, desc[f], files[f]))
    A('')
    A('**Reutilizado por identidad, sin volver a ejecutar.** Con la designación de r2 (d) y sin la entrada del instrumento, el constructor reproduce '
      'byte a byte `EIC-VERIFY.input-closure.json` (SHA-256 `%s`) y `EIC-I6-STRICT.input-closure.json` (`%s`) de r2, y los E de V, N4 y nc1 de r4 '
      '(`%s`, `%s`, `%s`). Por tanto los resultados de r4 sobre esos E siguen valiendo tal como están: D-R2, D-R2-STRICT, D-V, D-N4-* y D-NC1 de '
      '`p7-derive.json` (`099000fb…`), V-r4 y O3-STRICT de `p8-v.json` (`acb05c43…`), N4-* de `p9-n4.json` (`b3dcf564…`) y nc1 de `p10-nc1.json` '
      '(`068cd82f…`). R7-A añade la entrada del instrumento (A6-2, regla 1), así que su E no es el de r4: V, N4 y nc1 se volvieron a ejecutar con R7-A '
      '(§5 y §6). Los negativos de r4 (N4-d, N4-x, nc1-err, nc1-x y el grupo de cotejo) **no** se repitieron con R7-A.'
      % (by['REG-R2']['Observed']['Sha256Custodied'], by['REG-R2-STRICT']['Observed']['Sha256Custodied'], by['REG-R4-V']['Observed']['Sha256R4'],
         by['REG-R4-N4']['Observed']['Sha256R4'], by['REG-R4-NC1']['Observed']['Sha256R4']))
    A('')
    A('**Escenario.** El de r2 (d) y r4: la verificación de T1 sobre la entrega simulada del fixture (BASE = TIP `95bdc29d`, RED `53436cb2`, GREEN = '
      'G `b417d36c`, MAIN `fbe25347`, AuthorityRevision `1746b404`). Cada grupo trabaja sobre una copia propia de `D:\\r62-a6r2\\r3\\deliv` (solo '
      'lectura) en `D:\\r62-a6m7\\bench\\c-*`, con las refs conservadas; en cada copia, la escritura del objeto suelto del instrumento desde su blob (P1) '
      'y la comprobación del punto de uso (i)-(iv) por pwsh (P2) antes de la primera corrida (`work/state-prep.json`). nc2 y nc3 solo se derivan, sobre '
      'la copia de V. N6 usa una copia con un cambio sin commit en `src/Fixture.Lib/Calculator.cs`, escrito después de P1/P2.')
    A('')
    A('**Entradas de cada invocación** (RAE §10: copias de todas las entradas que conservan `TaskId`, `RunId`, `DelegationRunId` y `Attempt` internos '
      'salvo el campo mutado). K y el TRX son byte a byte los de r2; la delegación y la entrega, las de r2 con la copia como `ExpectedWorktree` y '
      '`Worktree`. Las copias de cada control van en `artifacts/orchestration/FX-U1/<T1-control>/0/<RunId de sus bytes>/…`; la salida propia de cada '
      'invocación, en `RunPath` = el directorio de `IgnorePath(I)` (§4). Las designaciones están en `designations/` y las copias de E, vectores, cierres, '
      'registros de derivación, solicitudes y salidas sintéticas, en `inputs/<caso>/`.')
    A('')
    A('**Corridas reales.** Como en r3 y r4: cada orden del instrumento corre por la forma declarada `& <git> --no-replace-objects cat-file blob 9a829b0e… '
      '| & <python> -I - <orden> …` en pwsh 7 (cwd = la copia); el registro de sesión se escribe en el formato de Codex y `Runs` se reconstruye con '
      '`reconstruct_runs.py`; la solicitud se construye con `table_r3.construct` (VERIFY) o `table_m7.construct_m` (M), con el `Closure` derivado; el '
      'aceptante fijado corre en solo lectura sobre la misma copia, con la comprobación de cero escrituras y de procesos. Las salidas del Controller son '
      'sintéticas (reglas y textos de r3/r4) y heredan de r4 `Authority` en fail (el mapa del fixture tiene defectos) y `Termination` en not_run: su '
      '`Classification` y su `Disposition` (STOP en todas las de V y N5-N7) no son las del oráculo, y el aceptante no las juzga.')
    A('')
    A('## 2. Cierre de VERIFY (A6-7, regla 7): constructor, regresión y lectura de I6 que necesita cada variante (`p1-build.json`)')
    A('')
    A('`make_eic_verify.py` construye E solo desde la designación (§4) y el `-C`: la tabla de obligaciones y el punto fijo son los de r2 (d) (perfil del kit: '
      '«Leer primero» y «Comandos canonicos» de `AGENTS.md`, AP 16.13 y 16.9), y los blobs se calculan siempre del árbol. Variantes:')
    A('')
    A('- **R7-A (cubrir).** Clase de autoridades (las `Authorities` del contrato, `AGENTS.md` como instrucción automática y los transitivos del punto fijo) '
      'por su blob en la AuthorityRevision. El contrato **no existe en el árbol de la AuthorityRevision** (`docs/automation/decisions/FX-U1-T1.gate-contract.json` '
      'entra en `d30fb6a9`, posterior a `1746b404`): se fija por el blob de sus bytes custodiados (`628d89af`, el `StateRef` de autorización del kit), '
      'como ya hacían r2 y r4. El objeto bajo verificación (`git diff --name-only D.BaseSha..I.Target.Commit`) y los registros de la ventana (las '
      '`Authorities` del contrato con `Class` UNIT_DOC: `FX-U1.md` y `FX-U1.yml`) por su blob en `I.Target.Commit`; en V, `I.Target.Commit` = '
      '`H.CurrentSha` = G, y en los controles el kit usa el `Target` de la verificación real (OQ-31), porque H puede faltar (N4), no nombrar un commit (nc1) '
      'o nombrar otro (N7b). D, H y el TRX por el blob Git de sus bytes custodiados (sin entrada si el archivo no existe). Y la entrada `{Path, Blob}` del '
      'instrumento de A6-2, regla 1, con su ruta de procedencia y el blob `9a829b0e…`, más su acción de A6-2, regla 2.')
    A('- **R7-B (limitar).** RAE §15.2 I6 literal sobre cada entrada Git: se excluye toda entrada cuyo blob no es el de la AuthorityRevision, también el '
      'contrato; quedan D, H, el TRX (no son entradas Git) y la entrada del instrumento (A6-2, regla 3). Las órdenes del instrumento se limitan a las '
      'lecturas que admite el cierre derivado.')
    A('- **R7-B-K.** R7-B conservando el contrato: es la variante «I6 estricta» de r2 (d) y r4 (O3-STRICT); solo comparación.')
    A('')
    A('**Lectura que necesita R7-A (punto que tiene que decidir la disposición del Coordinator; aquí no se decide).** R7-A solo es coherente con una '
      'lectura de RAE §15.2 I6 y de A6-2, regla 3, en la que la comprobación «todo blob coincide con el de la AuthorityRevision» se aplica a la clase de '
      'autoridades (autoridades, instrucciones automáticas y transitivos del punto fijo), mientras que las otras dos partes de `CanonicalInputs` según AP 16.24 '
      '(§20.3.1) — el objeto exacto y sus registros de revisión — se fijan por su blob en la revisión verificada, y los artefactos fuera de Git y el contrato '
      'custodiado por el blob de sus bytes. Con la lectura literal, A6-2, regla 3 («las demás entradas conservan sin cambio esa comprobación contra la '
      'AuthorityRevision») no admite estas %d entradas de R7-A: %s; y tampoco D, H ni el TRX, que no tienen ruta en el árbol de la AuthorityRevision. '
      '**R7-B tampoco es la lectura literal pura:** conserva D, H y el TRX porque I6 se aplica solo a las entradas Git (así se pidió); si I6 se aplicara '
      'también a ellas, ninguna invocación VERIFY tendría D ni H en E y la regla (i) las daría por ausentes. Y el kit (`controller-contracts.md` §2.1: '
      '«`CanonicalInputs[]` `{Path, Blob}` en la `AuthorityRevision`: contrato, archivo de decisiones, estado…») pide algo que no se puede cumplir para el '
      'contrato.' % (len(i6v), ', '.join('`%s` (%s)' % (p, 'ausente en la AR' if s == 'ABSENT_AT_AR' else 'otro blob') for p, s in i6v)))
    A('')
    A(table(grp('build')))
    A('')
    A('La diferencia de R7-A con el E de r2/r4 es exactamente una entrada de `CanonicalInputs` y una acción (E-V-INSTRUMENT). En el cierre derivado, la '
      'ruta de procedencia del instrumento entra en C4 (`Paths`), porque la tabla de A6-9 añade la ruta de toda entrada de E; ningún commit del fixture '
      'tiene esa ruta y el árbol de trabajo tampoco, así que no admite ninguna lectura real, pero sí pasaría la comprobación de ruta de R9 una orden que la '
      'nombrara (p. ej. `hashes --paths <ruta del instrumento>`). A6-2, regla 1, dice que esa ruta es un identificador y nunca una lectura: la tabla no la '
      'distingue (observación, sin efecto sobre ningún caso de este banco).')
    A('')
    A('## 3. Derivación del cierre del aceptante para cada invocación y variante (`p1-build.json`, grupo `derive`)')
    A('')
    A('Todas con `derive_r4.py` (tabla de A6-9 y A6-3, regla 6 (b) (i)-(v), con el registro de omisiones), después de la comprobación de designación. '
      'La transitividad en la AR cubre las lecturas de `clausemap` en EFF y EFF^1 porque en este fixture los blobs son los mismos (caso E-EFF-COVERAGE '
      'del §2); si difirieran, R7-A no las cubriría.')
    A('')
    A(table(grp('derive')))
    A('')
    A('## 4. Designación de D, H y K por las rutas del kit (O3), fallo cerrado (O2) y la función `Name` de C6 (O6) (`p1-build.json`, grupo `desig`)')
    A('')
    A('**Formato fijo por invocación** (`a6m7-designation/1`, un archivo por invocación en `designations/`): `Repo` (el `-C`), `AuthorityRevision`, '
      '`Invocation` {`UnitId`, `TaskId`, `Attempt`, `RunId`, `Target` {`Commit`, `Path`, `Blob`}}, `RunPath` (directorio de la salida propia), `Inputs` '
      '[{`Role` ∈ {DELEG, H, CONTRACT, TRX}, `Path` (ruta de E), `Custody` (archivo custodiado o `null`), `Blob` (solo CONTRACT: el blob custodiado)}], '
      '`Instrument` {`Path`, `Blob`}, `MapPath`, `Adapter`.')
    A('')
    A('**Regla (i) con O3** (`designate_m7.check`): D, H y K son las entradas de E en las rutas designadas, y «E fija su blob» significa exactamente una '
      'entrada de `CanonicalInputs` en esa ruta. Fallan cerradas (sin derivación, sin solicitud; la salida quedaría sin aceptar por A6-5, regla 4): un rol '
      'fuera de DELEG/H/CONTRACT/TRX o repetido (`ROLE_OUTSIDE_DHK`, `ROLE_REPEATED`); una ruta designada fijada dos veces o fuera de `CanonicalInputs` '
      '(`MULTIPLE_PINS`, `WRONG_CLASS`); una entrada fuera de Git (`artifacts/`) o `*.trx` que el kit no designó (`UNDESIGNATED_INPUT`); el TRX no fijado o '
      'con bytes de otro blob (`TRX_NOT_PINNED`, `TRX_UNVERIFIED`); el mapa M no fijado exactamente una vez o inutilizable (`MAP_PINS`, `MAP_UNUSABLE`); un '
      'campo no-commit que la tabla lee con un tipo que no puede usar (`D.ExpectedBranch`, `D.RunId`, `H.RunId`, `K.Authorities`, `I.*`: `WRONG_TYPE`). '
      'Un campo de commit con otro tipo sigue siendo la regla (ii) (NOT_A_COMMIT), y una entrada D/H/K ausente o ilegible, la regla (i) (INPUT_ABSENT).')
    A('')
    A('**Regla de rutas del kit propuesta (KPR), que deja la tabla de A6-9 sin cambio y mecánica:**')
    A('')
    A('- **KPR-1.** La salida propia de cada invocación VERIFY y de M (la del `-o` y la ruta ignorada de `cleantree`) está en `IgnorePath(I)` = '
      '`artifacts/orchestration/<I.UnitId>/<I.TaskId>/<I.Attempt>/<I.RunId>/output.json`, con la `I` que recibe la derivación. Para los controles, con '
      '`I.TaskId` = T1 (kit §2.1), la salida va en `T1/0/<RunId del control>/` y sus copias en `T1-<control>/…` (así lo hizo r4). Si la salida fuera a '
      '`T1-<control>/0/<RunId>/` con `I.TaskId` = T1, la ruta ignorada quedaría fuera de C4 y `cleantree` daría R9 MISMATCH (M-IGN-T1).')
    A('- **KPR-2.** Cada copia de DELEG y de H va en `artifacts/orchestration/<I.UnitId>/<dir>/<I.Attempt>/<RunId de los bytes copiados>/output.json` o '
      '`…/worker-handoff.json`: el directorio lleva el `RunId` de los bytes que se colocan allí, después de la mutación si el campo mutado es `RunId`. '
      'Así `Name(p)` (C6) es definido para todo control y coincide con el rol designado; la comprobación lo verifica y falla cerrada si no.')
    A('- **KPR-3.** `<dir>` es `I.TaskId` (la verificación) o `I.TaskId-<control>` (controles; `T1-m7` para M, cuyo `I.TaskId` es `T1-m7`).')
    A('')
    A('Con RAE §10 (copias que conservan los `RunId` internos salvo el campo mutado) y la regla P-SAME (directorio = `RunId` de la entrada de la '
      'verificación real), `Name(p)` solo queda indefinido si el campo mutado es `H.RunId` (NAME-TABLE). Ni V14 ni RAE §10 fijan qué campo mutan N7a y '
      'N7b: aquí N7a se realizó de dos formas (`DelegationRunId`, y `RunId` con las dos reglas de ruta), y N7b con `DelegationRunId` más el `CurrentSha` de '
      'otra corrida. Con P-SAME y `H.RunId` mutado (N7AR-SAME), el cierre derivado no tiene C6 H y **el registro de derivación no lo declara** (la regla '
      '6 (b) (v) no prevé esa causa): el aceptante rechaza `handoff` y `freetext` por R9 (archivo fuera del cierre), la salida no se aceptaría, el control '
      'agotaría sus reejecuciones (STOP P-04, control no superado) aunque la salida del Controller fuera correcta. Solo KPR-2 (o un cambio de la tabla) '
      'lo evita.')
    A('')
    A(table(grp('desig')))
    A('')
    A('## 5. Verificación real V con R7-A y R7-B (`p2-v.json`)')
    A('')
    A(table(v['Cases']))
    A('')
    A('**Lecturas de V que R7-B excluye** (R9 del aceptante sobre las mismas 12 corridas; las de M son las mismas):')
    A('')
    A(excl_lines(vB))
    A('')
    A('Con R7-B, V solo puede ejecutar %s; las comprobaciones `Authority` y `Contract` quedan sin hecho del instrumento (la salida sintética B cita la '
      'lectura primaria).' % ', '.join('`%s`' % x for x in v['R7BLimitedOrders']))
    A('')
    A('## 6. N4 y nc1 con R7-A (`p3-n4.json`, `p4-nc1.json`)')
    A('')
    A(table(n4['Cases'] + nc1['Cases']))
    A('')
    A('## 7. Pruebas estáticas de N5, N6, N7a y N7b (O6) (`p5-n5.json`, `p6-n6.json`, `p7-n7.json`)')
    A('')
    A('Mutaciones (V14 D.3 y G.2; RAE §10): N5, `H.TestResults` con 0 pruebas; N6, ninguna en las entradas; N7a, `H.DelegationRunId` de otra corrida '
      '(y el par O6 con `H.RunId`); N7b, `H.DelegationRunId` y `H.CurrentSha` de otra corrida (`13654f96…`, cabeza de `origin/fx/u1-fx05`). En N7b las '
      'órdenes ligadas al `CurrentSha` entregado (`identity`, `diff`, `scope`) leen ese commit, que entra en C1 por `H.CurrentSha` y `RevList`; las demás '
      'leen el `Target` (G).')
    A('')
    A('**N6 (trabajo sin commit; OQ-11).** Lo que no se puede construir: un control de RAE §10 es una copia de las entradas con un campo mutado, y el trabajo '
      'sin commit no es un campo de ninguna entrada (ni `worker-handoff/v1` ni `delegation/v2` lo representan); es un estado del árbol de trabajo del '
      '`-C`, que en la ronda real es el clon compartido por la verificación y los demás controles y en el que la sesión no escribe dentro de la ventana '
      '(W-2). Lo que sí se construyó: una copia propia con un cambio sin commit, para ver qué hacen E, el cierre y el aceptante si ese estado existiera: E '
      'y el cierre no cambian (salvo las rutas propias), `cleantree` ve el cambio y el aceptante acepta con R4 en MATCH. No demuestra cómo se produciría '
      'N6 en la ronda.')
    A('')
    A(table(n5['Cases'] + n6['Cases'] + n7['Cases']))
    A('')
    A('## 8. Invocación de medición M (A6-7, regla 1) (`p8-m.json`)')
    A('')
    A('Designación `designations/M.json`: la misma entrega, `I.TaskId` = `T1-m7`, `RunPath` = `artifacts/orchestration/FX-U1/T1-m7/0/<RunId de M>/`, copias '
      'de las entradas de la verificación bajo `T1-m7`. Solicitud sin `Verification` ni `RelayFacts`, con R4 y R10 declaradas no aplicables (A6-7, regla 1) '
      'y R5-R7 por su condición de datos.')
    A('')
    A(table(m['Cases']))
    A('')
    A('| Operación de A6-7, regla 1 | Órdenes | R7-A | R7-B |')
    A('|---|---|---|---|')
    for r in m['Coverage']:
        A('| %s | %s | %s | %s |' % (cell(r['Operation']), ', '.join(r['Orders']) if r['Orders'] else '—', r['R7A'], r['R7B']))
    A('')
    A('Con R7-B la medición no podría demostrar el cálculo de blobs y hashes, la lectura fiel de las entradas JSON y de texto en el `Target` y del contrato, '
      'ni la resolución por el mapa de cláusulas (`clausemap`, con R6 no aplicable): tres operaciones que A6-7, regla 1, exige. Las lecturas excluidas de M:')
    A('')
    A(excl_lines(mB))
    A('')
    A('## 9. Intérprete y Git declarados (`p0-env.json`)')
    A('')
    py, gi, pw, rt = env['Python'], env['Git'], env['PwshOfTheDeclaredForm'], env['CodexRuntimePwsh']
    A('| Pieza | Ruta | Versión | SHA-256 |')
    A('|---|---|---|---|')
    A('| Intérprete de la forma declarada (`sys.executable`, alias de ejecución) | `%s` | %s | `%s` (de la imagen) |' % (py['DeclaredPath'], py['Version'], py['ExecutableSha256']))
    A('| Imagen del intérprete | `%s` | %s | `%s` |' % (py['ProcessImage'], py['Version'], py['ProcessImageSha256']))
    A('| Git declarado (lanzador de Git para Windows) | `%s` | %s | `%s` (%s bytes); `--exec-path` `%s` |' % (gi['DeclaredPath'], gi['Version'], gi['Sha256'], gi['Size'], gi['ExecPath']))
    A('| pwsh de la forma declarada en este banco (no es el del runtime) | `%s` → `%s` | %s (%s) | `%s` |' % (pw['Path'], pw['ProcessImage'], pw['Version'], pw['LanguageMode'], pw['ProcessImageSha256']))
    A('| pwsh del runtime de Codex (solo SHA-256; no se ejecutó) | `%s` | — | `%s` (%s bytes) |' % (rt['Path'], rt['Sha256'], rt['Size']))
    A('')
    A('`r3/r2static` declaró el alias genérico `%USERPROFILE%\\AppData\\Local\\Microsoft\\WindowsApps\\python.exe`; este banco usó `sys.executable`, el alias '
      'del paquete. Los dos llevan a la misma imagen (mismo SHA-256), pero la orden registrada lleva la ruta literal: la disposición de la medición tiene que '
      'nombrar una sola ruta del intérprete y una del Git (`reconstruct_runs.py` acepta ambas formas: ruta absoluta a `python[3[.N]].exe` y a `git.exe`, '
      'sin distinguir mayúsculas; la del banco termina en `git.EXE`).')
    A('')
    rp, rc, pr = env['ResolutionPathOrder'], env['ResolutionCreateProcess'], env['Probes']
    A('**`git` por nombre con `PATH` = directorio del pwsh del runtime + `PATH` normal.** ' +
      ('El directorio del runtime no tiene ningún `git.*` (prueba de existencia de nombres, sin leer contenido). ' if not rp['RuntimeDirGitCandidates'] else
       'El directorio del runtime **tiene** candidatos `git.*`: %s (no se ejecutaron; las sondas no corrieron). ' % ', '.join(rp['RuntimeDirGitCandidates'])) +
      'Por el orden de `PATH` (pwsh: `PATHEXT` y `.ps1`), el primero es `%s` (%s el Git declarado). El instrumento '
      'no resuelve `git` así: lo lanza con `subprocess` (CreateProcess sin nombre de aplicación), que busca antes en el directorio de la imagen del intérprete, '
      'en el directorio actual (el `-C`, salvo con `NoDefaultCurrentDirectoryInExePath`, que vale `%s` en el entorno del banco), en System32, System y '
      'Windows, y después en `PATH`, solo `git.exe`; el primero también es `%s` (%s el declarado). Sondas reales con ese `PATH` (cwd = `c-m`): pwsh 7 '
      '`Get-Command git -All` → %s; el intérprete declarado con `subprocess.run(["git", "--exec-path"])` → `%s` (%s instalación que el Git declarado). '
      'Para la medición: una comprobación por `Get-Command` en el pwsh de la invocación no cubre el orden de CreateProcess; la que corresponde al '
      'instrumento es una orden del intérprete declarado que lance `git` por nombre con `subprocess` y muestre `--exec-path` y `--version`, registrada en el '
      'mismo registro de sesión (propuesta; no forma parte de A-6).'
      % (rp['FirstHit'], 'es' if rp['FirstHitIsDeclaredGit'] else 'NO es', rc['NoDefaultCurrentDirectoryInExePathOfTheBench'], rc['FirstHit'],
         'es' if rc['FirstHitIsDeclaredGit'] else 'NO es', ', '.join('`%s`' % x for x in pr.get('PwshGetCommandAll') or []), pr.get('PythonSubprocessGitExecPath'),
         'misma' if pr.get('SameInstallationAsDeclared') else 'OTRA'))
    A('')
    A('## 10. Resultado global, hallazgos y archivos')
    A('')
    A('Ejecución completa (`python -I run_m7.py all`) desde un área de trabajo vacía. **%d de %d casos se cumplen** (%s).%s'
      % (sm['Holding'], sm['Total'], ', '.join('%s %d' % (k, groups[k]) for k in sorted(groups)),
         '' if not sm['NotHolding'] else ' No se cumplen: ' + ', '.join(sm['NotHolding']) + '.'))
    A('')
    A('Versiones: %s; Python %s; pwsh %s; %s.' % (sm['Versions']['Git'], sm['Versions']['Python'], sm['Versions']['Pwsh'], sm['Versions']['Os']))
    A('')
    A('**Hallazgos** (observaciones distintas de lo que el texto acordado o el kit dan por hecho; ninguno se reetiqueta como superado):')
    A('')
    A('1. El contrato no está en el árbol de la AuthorityRevision: ni el kit (§2.1, «en la `AuthorityRevision`») ni I6 literal pueden fijarlo. R7-A lo fija '
      'por su blob custodiado (`628d89af`); R7-B lo excluye, K queda ausente por la regla (i) y `contract`, `clausemap` y `read-json` quedan fuera.')
    A('2. R7-A exige una lectura de I6 y de A6-2, regla 3, que la disposición tiene que nombrar (§2); R7-B también se aparta de la lectura literal al '
      'conservar D, H y el TRX.')
    A('3. Con R7-B, la medición no puede demostrar tres operaciones de A6-7, regla 1 (hashes, lectura de JSON y texto en el `Target`, `clausemap`).')
    A('4. O6 confirmado en su forma condicional: si un control muta `H.RunId` y el kit deja la copia bajo el `RunId` original, `Name(p)` no define H, el '
      'cierre pierde C6 H **sin registro de omisión**, y el aceptante rechaza `handoff` y `freetext`. KPR-2 lo evita sin tocar la tabla.')
    A('5. `IgnorePath(I)` usa `I.TaskId`: la salida de cada invocación tiene que estar en ese directorio (KPR-1); en r4, y aquí, los controles usan `T1/0/<RunId>/` '
      'para la salida y `T1-<control>/…` para las copias.')
    A('6. La ruta de procedencia del instrumento entra en C4 por la tabla de A6-9 aunque A6-2, regla 1, la declare identificador (§2; sin efecto aquí).')
    A('7. `derive_r4.py` sin la comprobación de designación: con un mapa no fijado usa un mapa vacío sin aviso, con `I.Target` no objeto lo registra como '
      'FIELD_EMPTY, con tipos erróneos o un TRX que no verifica se detiene con una excepción, y con dos entradas en la ruta de H toma la primera '
      '(grupo `desig`). La comprobación los convierte en fallo cerrado antes de derivar.')
    A('')
    A('**Lo que no queda demostrado:** las salidas del Controller y los registros de sesión son sintéticos, y las corridas usan el pwsh 7 del `PATH`, no el '
      'del runtime de Codex (que corre en ConstrainedLanguage según `r3/r2static`): la entrega de los bytes por el shell real de la invocación, la forma '
      'exacta con que Codex registra la orden declarada, que el sobre llegue sin alterar y separable de la salida de error, la resolución de `git` en el '
      'entorno real de la invocación y la reconstrucción de `Runs` desde un registro real siguen pendientes de la medición; la comprobación del punto de '
      'uso (v)-(viii) no se repitió aquí (r3/r2static); un solo escenario, un host, una versión de Git y de pwsh; N6 no es una mutación de RAE §10; qué campo '
      'mutan N7a y N7b no lo fija ninguna autoridad; la tabla de obligaciones del constructor es la de r2 (d), no un cálculo de AP 16.24 desde el texto de '
      '`AGENTS.md`; la lectura normativa del aceptante (A6-5, regla 6) y la disposición de A6-7, regla 7, no se deciden aquí.')
    A('')
    A('| SHA-256 | Archivo |')
    A('|---|---|')
    for f in sorted(k for k in files if k.endswith('.py')) + res_files + sorted(k for k in files if k.startswith('designations/')):
        if f in files:
            A('| `%s` | `%s` |' % (files[f], f))
    A('')
    A('El SHA-256 de `summary.json` y el de este README se dan fuera de ellos. Los resultados son JSON canónico ASCII saneados (`%USERPROFILE%`, '
      '`<usuario>`, `<email>`). `inputs/` contiene copias byte a byte (comprobadas sin nombre de usuario ni direcciones) de E, vectores, cierres, registros '
      'de derivación, solicitudes y salidas sintéticas; los sobres del instrumento no se copian (llevan direcciones en `CoAuthoredBy`) y se citan por '
      'SHA-256 en los resultados.')
    A('')
    data = L.sanitize_str('\n'.join(txt))
    with open(os.path.join(HERE, 'README.md'), 'wb') as fh:
        fh.write(data.encode('utf-8'))
    print('wrote README.md', L.sha256(data.encode('utf-8')))


if __name__ == '__main__':
    main()
