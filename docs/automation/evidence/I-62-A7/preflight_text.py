# preflight_text.py - renders preflight.md (Spanish) from the results of run_a7.py; the tables come from the results, the
# prose is fixed here. Stdlib only.
import hashlib, os

HERE = os.path.dirname(os.path.abspath(__file__))
PREV_BLOB = 'a2aed3cc894037d6b42d462fbec9d6d0c7f629a6'
PREV_BLOB2 = 'df8925b26229e7b678d450faa9e983cb5ef6cc13'


def _files():
    out = []
    for root, dirs, names in os.walk(HERE):
        dirs[:] = sorted(d for d in dirs if d != '__pycache__')
        for n in sorted(names):
            if n in ('preflight.md', 'MANIFEST.sha256'):
                continue
            p = os.path.join(root, n)
            with open(p, 'rb') as fh:
                out.append((os.path.relpath(p, HERE).replace('\\', '/'), hashlib.sha256(fh.read()).hexdigest()))
    return out


def _case(R, ph, cid):
    return next((c for c in R[ph]['Cases'] if c['Caso'] == cid), None)


def render(R, S, table):
    p1, p2, p3, p4, p5, p6 = R['p1'], R['p2'], R['p3'], R['p4'], R['p5'], R['p6']
    L = []
    a = L.append
    a('# I-62 A-7 — Preflight de la candidata (ronda 1, corregida)')
    a('')
    a('> Supervisión, plano a. Preflight de la candidata `docs/initiatives/I-62-A-7.md` (blob `%s`, PROPUESTA ronda 1, corregida dos veces en sitio por el '
      'Coordinator tras los preflights sobre los blobs `%s` y `%s`; no enviada, sin revisión) sobre la base `%s`, según decisiones §92, puntos 2, 3, 6 y 7. '
      'Aquí solo hay pruebas, resultados y hallazgos; ninguna regla se decide. No se ejecutó Codex, claude ni ningún CLI de modelo, no hubo red ni `gh`, '
      'no se leyeron credenciales, no se creó B1 ni ningún repositorio en GitHub y no se publicó nada. El instrumento (`9a829b0e…`) y el aceptante '
      '(`434ef1fc…`) se usaron solo desde sus objetos Git, sin modificar. Todo Python con `-I`. Punto de entrada único: `python -I run_a7.py all`.'
      % (S['Candidate']['Blob'], PREV_BLOB, PREV_BLOB2, S['Base']))
    a('')
    a('**Resultado global: %d de %d casos se cumplen.** %s Guardas sobre el commit hipotético: **%s** (G1-G7); autoprueba: **%s**, %d de %d mutantes '
      'muertos.' % (S['Hold'], S['Cases'], ('No se cumplen: %s (hallazgos de §9).' % ', '.join(c for _, c in S['NotHolding'])) if S['NotHolding']
                    else 'Todos se cumplen.', S['Guards']['Result'], S['Selftest']['Result'], S['Selftest']['Killed'], S['Selftest']['Total']))
    a('')
    a('Versiones: %s; Python %s; pwsh %s; %s.' % (S['Versions'].get('git'), S['Versions'].get('python'), S['Versions'].get('pwsh'), S['Versions'].get('os')))
    a('')
    a('Las lecturas del texto corregido son las predeterminadas del constructor, del validador y de la precondición. Los conmutadores de las lecturas '
      'de la ronda 1 antes de su corrección (`--absent-a=no-closure`, `--path-rule=strict`) solo se usan como evidencia negativa (casos `*-OLD-*`).')
    a('')
    a('## 0. Entorno e identidades (`p0-env.json`)')
    a('')
    a('Componentes reutilizados, copias byte a byte (SHA-256 comprobado contra la fuente): del banco de la medición `make_eic_verify.py` (R7-A), '
      '`derive_r4.py`, `designate_m7.py` y `reconstruct_runs.py`; del kit `a6m7-measure`, `reconstruct_runs_m7.py`. El repositorio de la entrega del '
      'banco (`D:\\r62-a6r2\\r3\\deliv`) se copió a un área propia para los commits sintéticos de los negativos.')
    a('')
    a(table(R['p0']['Cases']))
    a('')
    a('## 1. Constructor de A7-1 (`make_eic_a7.py`)')
    a('')
    a('«R7-A más»: importa sin cambio `make_eic_verify.py` (tabla de obligaciones, punto fijo, listas y su orden) y solo cambia la revisión en que fija '
      'cada entrada (A7-1, regla 2) y el tratamiento de las ausencias (regla 5). Un blob por revisión de lectura en que difiera; un par ya fijado en '
      '`CanonicalInputs` no se repite.')
    a('')
    a('| Categoría | Entradas | Revisión | Ausencia |')
    a('|---|---|---|---|')
    a('| (a) | citas UNIT_DOC / UNIT_CHANGE; `AGENTS.md` (instrucción automática); transitivos del punto fijo (WORKFLOW, README de agent-execution, esquema '
      'de la entrega, AUTOMATION_PLAN por «Leer primero») | `AuthorityRevision` | sin entrada, registrada (16.3, 16.24); `AGENTS.md` o AUTOMATION_PLAN '
      'ausentes en la AR: sin cierre (el punto fijo de 16.24 del perfil del kit no se puede calcular) |')
    a('| (a) | citas EXTERNAL; `WORKFLOW.md` y `AUTOMATION_PLAN.md` en Evaluate | `MainSha_eval` = `K.MainSha` | sin entrada, registrada |')
    a('| (a) | mapa, esquema y cada MODIFIED del mapa; `WORKFLOW.md` y `AUTOMATION_PLAN.md` en Validate | EFF = Derive(`MainSha_eval`) | sin entrada, '
      'registrada (16.13: MAP_INVALID) |')
    a('| (a) | cada MODIFIED del mapa; `AUTOMATION_PLAN.md` en Validate | EFF^1 | sin entrada, registrada |')
    a('| (b) | objeto exacto; rutas de `git diff --name-only D.BaseSha..Target.commit`; `decisions/<UnitId>.md` y `state/<UnitId>.yml` | `Target.commit` | '
      'objeto ausente: sin cierre; ruta borrada o registro ausente: sin entrada, registrada |')
    a('| (c) | contrato, delegación, entrega, TRX en las rutas de la designación | blob Git de los bytes custodiados (y el registrado, si la designación lo trae) | '
      'contrato o delegación ausentes, de otro blob o ilegibles: sin cierre; entrega o TRX: sin entrada, registrada, nada sustituido |')
    a('| (d) | `{ruta de procedencia, 9a829b0e…}` y su acción (A6-2) | blob de A-6 | solo si la designación lo admite |')
    a('')
    a('Los archivos ADDED y ENTRY del mapa (Validate solo compara su blob, MV-4) no son entradas: en FX-U1 serían %d.'
      % p1['Rows'][0]['no-entry'].get('NotAdmittedMapFiles', 0))
    a('')
    a('## 2. Validador (`check_a7_closure.py`)')
    a('')
    a('Escrito desde el texto de A-7 (§3.2, §3.3), sin importar el constructor, `make_eic_verify.py` ni `derive_r4.py`: calcula sus propias referencias '
      'desde la designación y el `-C` (su propio Derive de EFF, su propia lectura de K, D y H). Cada entrada de las tres listas debe pertenecer al menos a '
      'una categoría (si no, FOREIGN); pertenece a una categoría cuando su blob es la referencia de esa categoría para su ruta, así que una entrada en '
      'varias categorías coincide con cada referencia (OVERLAP, admitido y registrado). Exige las entradas obligatorias que existen (MISSING); rechaza '
      'duplicados dentro de una lista con el mismo `RequiredBy`, (b)/(c)/(d) fuera de `CanonicalInputs`, acciones del instrumento no admitidas, el '
      '`RequiredBy` de los transitivos que no esté en la AR y la regla de rutas de 2 (c) (bajo `artifacts/orchestration/`, en un directorio con el '
      '`RunId` de los bytes); con el objeto del `Target` ausente, o sin contrato o delegación utilizables, exige que no haya cierre (NoClosureExpected). '
      'Una ausencia en (a) no es obligatoria y se registra. Admite la misma entrada en varias listas y una ruta con un blob por revisión de lectura.')
    a('')
    a('## 3. Reproducción sobre R7-A (§92, punto 7) (`p1-repro.json`)')
    a('')
    a('Para cada designación del banco: E de R7-A reconstruido con la copia del constructor del banco (y comparado con el E custodiado), E de A7-1 con el '
      'texto corregido, validación, y la precondición de §3.5. En FX-U1 los blobs coinciden en la AR, EFF, EFF^1 y `MainSha_eval` (EFF = `MainSha_eval` = '
      '`fbe25347`, EFF^1 = `930288c5`); `PROMPT_TEMPLATES.md`, MODIFIED en el mapa, no existe ni en EFF ni en EFF^1 y, con la regla 5 (a) corregida, no '
      'aporta entrada (Validate dará MAP_INVALID).')
    a('')
    a(table(p1['Cases']))
    a('')
    a('| Caso | E de R7-A custodiado en el banco (SHA-256) | E de A7-1 (SHA-256) | ¿Igual? | Regla 5 (a) de la ronda 1 |')
    a('|---|---|---|---|---|')
    for r in p1['Rows']:
        a('| %s | `%s` | `%s` | %s | %s |' % (r['Case'], r['R7ACustodiedSha256'], r['no-entry'].get('Sha256'), 'sí' if r['no-entry'].get('EqualsR7A') else 'no',
                                            'sin cierre' if not r['no-closure']['Closure'] else 'cierre'))
    k = p1['Kit']
    a('| kit (M, congelado) | `%s` | igual al congelado: %s | %s | %s |' % (k['FrozenESha256'], 'sí' if k['A7EqualsFrozen'] else 'no',
                                                                     'sí' if k['A7EqualsFrozen'] else 'no', 'sin cierre' if k['OldReading5aNoClosure'] else 'cierre'))
    a('')
    a('El E congelado del kit se valida con la designación del kit apuntada a una copia del repositorio de la entrega y a copias de sus entradas (mismos '
      'commits, mismos bytes); `D:\\r62-a6m7\\m7-run` no se leyó.')
    a('')
    a('## 4. Caso B1-like y residuo declarado (`p2-b1.json`, `b1-like/`, `check_citation_blobs.py`)')
    a('')
    c = p2['Commits']
    a('Clon temporal de `candidate-b.bundle` (custodiado en `MAP-INVALID/`, MAP_VALID: EFF `%s`, EFF^1 `%s`, con los blobs I-61 de la base del mapa). Solo en el '
      'clon: una unidad sintética FX-U2 sobre el reclamo `%s` (AR `%s`, BaseSha `%s`, `Target.commit` `%s`; identidad `*.invalid`, fechas fijas), el objeto '
      'suelto del instrumento escrito desde su blob y las entradas custodiadas con las formas del kit de FX-02 y valores sintéticos. Una corrida real de '
      '`clausemap` por la forma declarada en pwsh 7 (no es el pwsh del runtime de Codex) y el aceptante fijado con la solicitud de la medición (sin '
      '`Verification` ni `RelayFacts`); sin registro de sesión: el caso solo prueba R9. La variante `uc` (AR `%s`) cambia el archivo UNIT_CHANGE citado '
      'antes de la AR: es el residuo que §3.5 declara.'
      % (c['EFF'][:8], c['EFF^1'][:8], c['Claim'][:8], c['AuthorityRevision'][:8], c['BaseSha'][:8], c['Target'][:8], c['Variant-uc']['AuthorityRevision'][:8]))
    a('')
    a('`check_citation_blobs.py` es la comprobación de fallo cerrado de §3.5 (código 0: se puede lanzar; 1: no hay lanzamiento, S-03; 2: no evaluable). '
      'Modo predeterminado y único normativo: `coverage`, el criterio del texto vigente (si alguna lectura de contenido que `clausemap` hace de una cita '
      'del contrato en EFF^1, EFF o `MainSha_eval` cae en una revisión en que el blob de esa ruta no es el de una entrada del cierre, no hay lanzamiento; '
      'una revisión en que la ruta no existe no aporta lectura). Modos `text` (toda cita con el mismo blob en la AR, EFF, EFF^1 y `MainSha_eval`: la '
      'redacción anterior) y `unit` (lo mismo, solo para las citas UNIT_DOC y UNIT_CHANGE) solo como evidencia negativa.')
    a('')
    a(table(p2['Cases']))
    a('')
    a('## 5. Negativos (§92, punto 7) (`p3-neg.json`)')
    a('')
    a('Ausentes, sobre el constructor (y el validador cuando hay cierre); reemplazados y ajenos, sobre el validador con copias mutadas del E de V (o del '
      'B1-like cuando el caso lo exige). Commits sintéticos solo en la copia del repositorio de la entrega: `%s` (borra `src/Fixture.Lib/Fixture.Lib.csproj`), '
      '`%s` (borra `docs/automation/state/FX-U1.yml`) y `%s` (el registro de decisiones vuelve al blob de la AR).'
      % (p3['SyntheticCommits']['DiffPathDeleted'][:8], p3['SyntheticCommits']['RecordAbsent'][:8], p3['SyntheticCommits']['RecordAtAR'][:8]))
    a('')
    a(table(p3['Cases']))
    a('')
    a('**Reutilizado por identidad, sin volver a ejecutar** (negativos de sustitución e inflación al nivel de la derivación del cierre del aceptante; '
      '`derive_r4.py` y `designate_m7.py` sin cambio):')
    a('')
    a('| Archivo | SHA-256 | Casos | ¿Identidad comprobada? |')
    a('|---|---|---|---|')
    for f in p3['ReusedByIdentity']['Files']:
        a('| `%s` | `%s` | %s | %s |' % (f['Path'], f['Sha256'], f['Cases'], 'sí' if f['Verified'] else 'no'))
    a('')
    a('## 6. Guardas de alcance (`a7-guards.py`; `p4-guards-preview.json`, `p4-guards-selftest.json`)')
    a('')
    a('`preview` construye un commit hipotético sobre `%s` en un repositorio desnudo temporal propio (alternates de solo lectura; ni refs, ni índice, ni '
      'árbol del repositorio de I-62 cambian) con la candidata como `docs/initiatives/I-62-A-7.md` (blob `%s`) y este directorio bajo '
      '`docs/automation/evidence/I-62-A7/` (sin `summary.json`, `preflight.md`, `MANIFEST.sha256`, `p4-guards.json` ni su propia salida, generados '
      'después; G7 se repite al final sobre el directorio completo). G3 conserva las citas de §2 (sin cambio en la corrección); G4 y G5 comprueban además '
      'el texto corregido y rechazan las frases superadas de la ronda 1. Para el commit de publicación: `python -I a7-guards.py run --base <base> --head '
      '<commit> --out <json>`. Commit hipotético: `%s`.' % (S['Base'][:8], p4['Preview']['CandidateBlob'], p4['Preview']['Head']))
    a('')
    a(table(p4['Cases']))
    a('')
    a('## 7. Compatibilidad y preservación (`p5-compat.json`)')
    a('')
    a(table(p5['Cases']))
    a('')
    a('| Regla de A-6 | Cómo la toca A7-1 |')
    a('|---|---|')
    for t in p5['A6RulesTouched']:
        a('| %s | %s |' % (t['Regla'], t['Cómo']))
    a('')
    a('## 8. Reconstrucción, comprobación estática (§92, punto 6) (`p6-recon.json`, `recon_static.py`)')
    a('')
    a('Comparación por `ast.dump` (sin atributos de posición) de `reconstruct_runs.py` (`5dc9f78e…`) y `reconstruct_runs_m7.py` (`1eceecee…`), con la '
      'normalización declarada en cada fila. **No se declara demostrada la reconstrucción sobre Codex 26.1013**: esto es solo estático; la reconstrucción '
      'desde un registro real sigue siendo una precondición con fallo cerrado de la medición.')
    a('')
    a(table(p6['Cases']))
    a('')
    a('| Garantía | Dónde | ¿Idéntica? | Ampliación de la v2 |')
    a('|---|---|---|---|')
    for g in p6['Raw']['Guarantees']:
        a('| %s | %s | %s | %s |' % (g['Guarantee'], g['Where'], 'sí' if g['Identical'] else 'no', g.get('Widening', '—')))
    a('')
    a('## 9. Hallazgos')
    a('')
    a('**Hallazgos de los preflights anteriores (blobs `%s` y `%s`) frente al texto vigente:**' % (PREV_BLOB, PREV_BLOB2))
    a('')
    a('| Id | Hallazgo | Corrección del texto | Estado ahora | Casos |')
    a('|---|---|---|---|---|')
    a('| F1 | regla 5 (a) literal: sin cierre en FX-U1 (PROMPT_TEMPLATES.md) | 5 (a): sin entrada; la tratan 16.3, 16.13 o 16.24 | resuelto: E A7 = E R7-A en las 11 y en el kit | REPRO-*, REPRO-OLD-5A, NEG-ABS-AUTH(-OLD) |')
    a('| F2 | `<TaskId>` de 2 (c) frente a la ruta de los controles | 2 (c): ruta de la tarea o del control (RAE §10), directorio = `RunId` de los bytes | resuelto: las 11 cumplen; N7AR-SAME rechazada | REPRO-OLD-PATH, NEG-PATH-RUNID |')
    a('| F3 | «una ruta figura una vez por cada blob distinto» | sustituida: un blob por revisión en que difiera; la misma entrada en varias listas si 16.24 lo exige | resuelto | REPRO-UNIQ |')
    a('| F4 | contrato o delegación ausentes | 5 (c): sin cierre y sin lanzamiento; entrega o TRX: sin entrada | resuelto | NEG-ABS-CONTRACT-*, NEG-ABS-DELEG-*, NEG-ABS-H-* (como N4), NEG-ABS-TRX-* |')
    a('| F5 | «los archivos del mapa» en lectura amplia | 1 (a) y 2 (a) estrechadas | resuelto: un ADDED del mapa es FOREIGN | NEG-FOR-WIDE |')
    a('| F6 | lecturas de `clausemap` fuera de A7-1 | residuo declarado en §3.5 y Q-A7-06; comprobación de fallo cerrado del kit (S-03) | residuo esperado y bloqueado por la precondición | B1-UC-R9, CIT-B1-UC |')
    a('| F7 | E3 no nombrada | `WORKFLOW.md` y `AUTOMATION_PLAN.md` en Evaluate (`MainSha_eval`) | resuelto | REPRO-* |')
    a('| F8 | cabecera de `reconstruct_runs_m7.py` | fuera de A-7 | sin cambio (precisión de la cabecera, neutral) | RECON-HEADER |')
    a('| F9 | precedencia entre categorías | regla 1: al menos una; con varias, el blob de cada referencia | resuelto | NEG-POS-OVERLAP |')
    a('| F10 | la precondición de §3.5 («blobs distintos en esas revisiones») bloqueaba el caso B1-like principal por la cita EXTERNAL de AUTOMATION_PLAN.md | '
      '§3.5: criterio de cobertura (toda lectura de contenido de `clausemap` de una cita está fijada por el cierre) | resuelto: el principal se puede lanzar, '
      'B1-UC sigue bloqueado, FX-U1 pasa; la redacción anterior queda como evidencia negativa (modo `text`) | CIT-B1-MAIN, CIT-B1-UC, CIT-FXU1 |')
    a('| F11 | ausencia de la ruta en alguna revisión, no definida en §3.5 | «una revisión en que la ruta no existe no aporta lectura» | resuelto: una cita ausente en su '
      'revisión de lectura y leída en otra revisión queda fuera del cierre y bloquea por cobertura | CIT-* (modo `coverage`) |')
    a('| F12 | «con el blob registrado»: el formato de designación solo registra el blob del contrato | 2 (c): la designación «registra la ruta y el blob de cada uno» '
      '(contrato, delegación, entrega y TRX) | texto resuelto; el formato `a6m7-designation/1` del banco no se cambia: registra solo el blob del contrato, y la próxima '
      'preparación del kit tiene que registrar los de la delegación, la entrega y el TRX | DES-REG, DES-REG-B1, NEG-ABS-*-OTRO-BLOB |')
    a('')
    a('**Nota para la próxima preparación del kit (F12).** Con la regla 2 (c) vigente, cada designación tiene que registrar la ruta y el blob de los cuatro '
      'artefactos. El formato `a6m7-designation/1` que usan el banco y el kit actual solo trae `Blob` para el contrato; el validador comprueba el blob '
      'registrado cuando está y avisa cuando falta (`UnregisteredBlobs`; DES-REG: DELEG, H y TRX en las once designaciones del banco). Las designaciones '
      'B1-like de este preflight ya registran los cuatro (DES-REG-B1). El formato del banco no se ha cambiado.')
    a('')
    a('**Observación sin efecto.** La regla 2 (c) solo restringe el segmento del `RunId`; la función `Name(p)` de C6 (A6-9) sigue necesitando '
      '`artifacts/orchestration/<u>/<t>/<a>/<RunId>/output.json|worker-handoff.json`, que `designate_m7.py` (KPR-2) comprueba antes de derivar. Las once '
      'designaciones del banco y las B1-like cumplen ambas.')
    a('')
    a('## 10. Archivos')
    a('')
    a('Publicables en `docs/automation/evidence/I-62-A7/` (este directorio). `MANIFEST.sha256` lista todos, incluido este documento.')
    a('')
    a('| Archivo | SHA-256 |')
    a('|---|---|')
    for n, h in _files():
        a('| `%s` | `%s` |' % (n, h))
    a('')
    return '\n'.join(L) + '\n'
