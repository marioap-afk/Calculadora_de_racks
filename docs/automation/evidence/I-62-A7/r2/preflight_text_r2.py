# preflight_text_r2.py - renders preflight.md (Spanish) of the round-2 preflight from the results of run_a7r2.py; the tables come
# from the results, the prose is fixed here. Stdlib only.
import hashlib, os

HERE = os.path.dirname(os.path.abspath(__file__))


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


def render(R, S, table):
    L = []
    a = L.append
    a('# I-62 A-7 — Preflight dirigido de la segunda candidata (ronda 2)')
    a('')
    reused = S.get('ReusedPhases') or {}
    a('> Supervisión, plano a. Preflight de la candidata `docs/initiatives/I-62-A-7.md` ronda 2 (blob `%s`, PROPUESTA; generada desde la ronda 1, blob '
      '`%s`, por `make_a7r2.py`, primera versión `%s`; corregida en el mismo archivo por `fix_a7r2_b.py`, blob `%s`, con las observaciones 1-3 del primer '
      'preflight de la ronda 2, y por `fix_a7r2_c.py` con las dos precisiones del segundo: la cita de AP 16.8 y la exigencia de RED indeterminada) sobre la '
      'base `%s`, según decisiones §94, puntos 2, 3 y 5, con validación incremental (§88). La coherencia con el texto de `fix_a7r2_b.py` obligó a cambiar el '
      'constructor y el validador (exigencia de RED según AP 16.8; medición sin hechos del relevo) y se repitieron q1-q6; `fix_a7r2_c.py` ya no cambia las '
      'herramientas, así que en la última corrida se repite lo que depende del texto (q0, q1-q2 con RED-UNDETERMINED, q5, q6 y las guardas) y %s. La '
      'evidencia de la ronda 1 (`docs/automation/evidence/I-62-A7/`) se reutiliza por SHA-256 y este preflight no la escribe. Aquí solo hay pruebas, '
      'resultados y hallazgos; ninguna regla se decide. Sin Codex, claude ni ningún CLI de modelo; sin red ni `gh`; sin credenciales; sin crear B1 ni '
      'ningún repositorio; nada publicado. Instrumento (`9a829b0e…`) y aceptante (`434ef1fc…`) solo desde sus objetos Git. Todo Python con `-I`. Punto de '
      'entrada único: `python -I run_a7r2.py all` (con `--reuse q3,q4` en la última corrida).' % (
          S['Candidate']['Blob'], S['PreviousRound'], S.get('CandidateRun1', '-'), S.get('CandidateRun2', '-'), S['Base'],
          ('se reutilizan por SHA-256, de la corrida anterior de este directorio, %s (piezas del procedimiento salvo el blob de A-7 y designaciones '
           'iguales byte a byte: q0, REUSE-PHASES)' % ', '.join('`%s` (`%s…`)' % (k, v[:16]) for k, v in sorted(reused.items()))) if reused else
          'se repiten también q3 y q4'))
    a('')
    a('**Resultado global: %d de %d casos se cumplen.** %s Guardas sobre el commit hipotético de publicación: **%s** (G1-G7); autoprueba de las '
      'condiciones nuevas o cambiadas: **%s**, %d de %d mutantes muertos.' % (
          S['Hold'], S['Cases'], ('No se cumplen: %s.' % ', '.join(c for _, c in S['NotHolding'])) if S['NotHolding'] else 'Todos se cumplen.',
          S['Guards']['Result'], S['Selftest']['Result'], S['Selftest']['Killed'], S['Selftest']['Total']))
    a('')
    a('## 1. Qué cambia en la ronda 2 y qué se repite')
    a('')
    a('| Cambio | Pruebas repetidas | Reutilizado por identidad (ronda 1) |')
    a('|---|---|---|')
    a('| R1: categoría (c) con seis papeles cerrados (contrato, delegación, entrega, TRX de `CurrentSha`, TRX de `RedSha` si la entrega exige RED, '
      'hechos del relevo), un artefacto por papel, ruta y blob registrados, ausencia con la clasificación de 16.9, comprobación de ausencia que no es '
      'P-22, rechazo de lo no designado; `RelayFacts` = la entrada RELAY | q1 (TRX y BOM), q2 (RelayFacts), q3 (N4 y nc1), q4 (tipo B1) | negativos de (a), '
      '(b) y (d) de la ronda 1 (`p3-neg.json`), sin cambio de regla |')
    a('| R2: invalidador = A7-1 en el blob acordado + procedimiento de construcción y validación nombrado | q5 | — |')
    a('| R3: precisión de A6-2, regla 1, dentro del ámbito | q6 (COMPAT-A6-RULES) | líneas del Owner y presupuestos de V14 D.3 (`p5-compat.json`) |')
    a('| O1, O7, O2 (descriptivos) | q7 (guardas G4) | — |')
    a('| `fix_a7r2_b.py`: punto 1 (c) «si la entrega exige RED (16.9, `Ci` y `Tests`)» y «la invocación de medición … no tiene hechos del relevo»; punto 7 '
      'con «la implementación de la derivación del cierre del aceptante»; fila (c) de §3.3 | q0 (CAND-DIFF), q1 (RED-*), q2 (RELAY-MEASUREMENT*), q5 '
      '(INV-PIECE, INV-DERIVATION), q7 (G4 rule1c-roles, rule1c-measurement, rule7-invalidator, table-c-roles, superseded-r2b) | — |')
    a('| `fix_a7r2_c.py`: fila L16 de §2 con AP 16.8 L616-L617 y la cita de «**exige RED**»; punto 1 (c) «(16.8; 16.9, `Ci` y `Tests`)»; regla 5 (c) '
      '«si la exigencia de RED de 16.8 no se puede determinar porque falta la entrega, el TRX de la corrida de `RedSha` ni se admite ni se exige, y se '
      'registra la causa» | q0 (CAND-DIFF-C), q1 (RED-UNDETERMINED), q5 (blob de A-7), q6, q7 (G3 L16, L16-168-lines, L16-168-quote, L16-168-required; G4 rule1c-roles, '
      'rule5c-169, rule5c-undetermined, rule5c-absence-check, superseded-r2c) | q3, q4 (sin dependencia del texto; REUSE-PHASES) |')
    a('| — | — | reproducción sobre R7-A (`p1-repro.json`), reconstrucción estática (`p6-recon.json`, `reconstruct_runs*.py` sin cambio), condiciones de las '
      'guardas sin cambio (`p4-guards-selftest.json`) |')
    a('')
    a('## 2. Procedimiento de la ronda 2')
    a('')
    a('- **Formato de designación `a7-designation/2`** (`a7-designation-2.schema.json`): `Artifacts` con `Role` ∈ {CONTRACT, DELEG, H, TRX_CURRENT, TRX_RED, '
      'RELAY}, `Path`, `Blob` registrado (obligatorio) y `Custody`; `Invocation.Kind` ∈ {VERIFY, MEASUREMENT} (ausente = VERIFY), añadido en esta corrida '
      'porque sin esa marca la medición no se distingue de una verificación (q2, RELAY-MEASUREMENT-DESIGNATED). El formato de la ronda 1 '
      '(`a6m7-designation/1`) solo se lee como evidencia negativa (`--legacy-designation`).')
    a('- **Constructor `make_eic_a7.py` (ronda 2)**: (a), (b) y (d) como en la ronda 1; (c) con los seis papeles, en el orden contrato, …, delegación, '
      'entrega, TRX de `CurrentSha`, TRX de `RedSha`, hechos del relevo, instrumento. **La entrega exige RED** según AP 16.8 («RED de la cadena», L616-L618, '
      'la condición que usan las filas 9 `Ci` y 10 `Tests` de 16.9): `ChainRedSha` nulo, o `git diff --name-only BaseSha..CurrentSha` toca `ChainRedFiles` '
      '(`BaseSha`, `ChainRedSha` y `ChainRedFiles` de la delegación, `CurrentSha` de la entrega); `ExpectRed` (siempre el del contrato) nombra las pruebas '
      'que deben fallar y no decide la exigencia. Sin `CurrentSha` utilizable y con `ChainRedSha` no nulo la condición es indeterminada: el TRX de `RedSha` '
      'no se admite ni se exige y la causa se registra. En la medición (`Kind` MEASUREMENT) no hay papel RELAY: designado, no se admite; sin designar, no es '
      'una ausencia. Sin contrato o delegación utilizables no hay cierre; la entrega, un TRX o el relevo ausentes, sin blob registrado, con otro blob o '
      'ilegibles no aportan entrada y se registran. La regla anterior de la herramienta (RED = algún `RequiredTests[].ExpectRed` de la delegación) queda '
      'solo como evidencia negativa (`--red-rule=expectred`).')
    a('- **Validador `check_a7_closure.py` (ronda 2)**: escrito desde el texto, sin importar el constructor (su propia implementación de la condición de '
      '16.8); comprueba los seis papeles, un artefacto por papel, el blob registrado frente a los bytes custodiados (un BOM forma parte de los bytes) y '
      'rechaza como FOREIGN lo no designado, de otro papel (un TRX de `RedSha` cuando la entrega no exige RED o no se puede determinar; hechos del relevo '
      'en la medición) o con otro blob.')
    a('- **`designate_a7.py`** (lado del aceptante): comprobación de la designación y vector de `derive_r4.py` (sin cambio) con los seis papeles; C6 nombra '
      'TRX a los dos TRX designados y no nombra los hechos del relevo. Las dos son «la implementación de la derivación del cierre del aceptante» del '
      'punto 7. `designate_m7.py` (ronda 1) sigue sin cambio para R7-A.')
    a('- **`check_citation_blobs.py`**: solo cambia para leer `a7-designation/2`; el criterio de cobertura de §3.5 es el mismo.')
    a('')
    a('## 3. Entorno, identidades y reutilización (`q0-env.json`)')
    a('')
    a(table(R['q0']['Cases']))
    a('')
    a('## 4. TRX de `CurrentSha` y de `RedSha`, exigencia de RED y fidelidad del BOM (`q1-trx.json`)')
    a('')
    a('Invocación V sobre una copia del repositorio de la entrega (objeto suelto del instrumento escrito en la copia). TRX de `RedSha` sintético '
      '(`inputs/fixture-tests-red.trx`): el TRX del kit con un resultado fallido de la prueba con `ExpectRed`, con el mismo BOM y fin de línea. Las '
      'corridas del instrumento son reales (forma declarada en pwsh 7) y el aceptante fijado recibe una solicitud mínima de datos (sin `Verification`, R4 '
      'no aplicable por A6-3, regla 6 (c)); no es la aceptación de una salida del Controller. Los casos RED-* comparan, sobre delegaciones sintéticas '
      '(copias de la del banco con `ChainRedSha`, `ChainRedFiles` o `ExpectRed` cambiados y su blob registrado), la regla de 16.8 con la regla anterior '
      'de la herramienta.')
    a('')
    a(table(R['q1']['Cases']))
    a('')
    a('## 5. `RelayFacts` (`q2-relay.json`)')
    a('')
    a(table(R['q2']['Cases']))
    a('')
    a('## 6. N4 y nc1 con la designación y el constructor de la ronda 2 (`q3-n4-nc1.json`)')
    a('')
    a('Las solicitudes custodiadas del banco (N4-A; nc1 V1, V2 y V3) se repiten tal cual, cambiando solo el `Closure` por el derivado del E de la ronda 2; '
      'los resultados del banco son `p3-n4.json` y `p4-nc1.json` del banco (por identidad).')
    a('')
    a(table(R['q3']['Cases']))
    a('')
    a('## 7. Tipo B1 con EFF^1 distinto (`q4-b1.json`, `b1-like/`)')
    a('')
    a(table(R['q4']['Cases']))
    a('')
    a('## 8. Identidad del invalidador de A6-7 (`q5-invalidator.json`)')
    a('')
    i = R['q5']['Identity']
    a('| Pieza (A7-1, punto 7) | Identidad |')
    a('|---|---|')
    a('| A-7 (candidata ronda 2) | blob `%s` |' % i['A7Blob'])
    a('| constructor `make_eic_a7.py` | SHA-256 `%s` |' % i['BuilderSha256'])
    a('| validador `check_a7_closure.py` | SHA-256 `%s` |' % i['ValidatorSha256'])
    a('| formato `a7-designation-2.schema.json` | SHA-256 `%s` |' % i['DesignationFormatSha256'])
    a('| derivación del cierre del aceptante: `designate_a7.py` | SHA-256 `%s` |' % i['DerivationDesignateSha256'])
    a('| derivación del cierre del aceptante: `derive_r4.py` (sin cambio desde la ronda 1) | SHA-256 `%s` |' % i['DerivationDeriveSha256'])
    a('')
    a('| Invocación | SHA-256 del cierre concreto (se custodia con la invocación) | ¿Válido? |')
    a('|---|---|---|')
    for r in R['q5']['Rows']:
        a('| %s | `%s` | %s |' % (r['Case'], r['ESha256'], 'sí' if r['Accepted'] else 'no'))
    a('')
    a(table(R['q5']['Cases']))
    a('')
    a('## 9. Compatibilidad con A-5 y A-6 (`q6-compat.json`)')
    a('')
    a(table(R['q6']['Cases']))
    a('')
    a('| Regla de A-6 | Cómo la toca A7-1 (ronda 2) |')
    a('|---|---|')
    for t in R['q6']['A6RulesTouched']:
        a('| %s | %s |' % (t['Regla'], t['Cómo']))
    a('')
    a('## 10. Guardas de la ronda 2 (`a7-guards.py`; `q7-guards-preview.json`, `q7-guards-selftest.json`)')
    a('')
    a('`preview` construye el commit hipotético de publicación sobre `%s`: modifica `docs/initiatives/I-62-A-7.md` (de la ronda 1, `6c29747d`, a la '
      'candidata, blob `%s`) y añade este directorio bajo `docs/automation/evidence/I-62-A7/r2/` (sin `summary.json`, `preflight.md`, `MANIFEST.sha256`, '
      '`q7-guards.json` ni su propia salida, generados después; G7 se repite al final sobre el directorio completo). Commit hipotético: `%s`. Para el commit '
      'de publicación: `python -I a7-guards.py run --base <base> --head <commit> --out <json>`. La autoprueba mata un mutante por cada condición nueva o '
      'cambiada de la ronda 2 (incluidas las cinco de `fix_a7r2_b.py` y las nueve de `fix_a7r2_c.py`; el mutante de `rule7-instance` cambió de ancla '
      'porque `fix_a7r2_b.py` movió un salto de línea); las condiciones sin cambio conservan el código y la autoprueba de la ronda 1 (%s).' % (
          S['Base'][:8], R['q7']['Preview']['CandidateBlob'], R['q7']['Preview']['Head'], '; '.join(R['q7']['Selftest'].get('Unchanged') or [])))
    a('')
    a(table(R['q7']['Cases']))
    a('')
    a('## 11. Hallazgos y observaciones')
    a('')
    nh = S['NotHolding']
    a('**Casos que no se cumplen:** %s.' % (', '.join(c for _, c in nh) if nh else 'ninguno'))
    a('')
    a('**Hallazgo sobre las herramientas (corregido en la corrida sobre `897d7fca`, no en el texto).** Con el texto corregido («si la entrega exige RED»), '
      'la regla del primer preflight de la ronda 2 (RED = algún `RequiredTests[].ExpectRed` de la delegación) **no era coherente** con AP: la '
      'exigencia de RED la define 16.8 (L616-L617: `ChainRedSha` nulo o `git diff --name-only BaseSha..CurrentSha` que toca `ChainRedFiles`) y `ExpectRed` '
      '«es siempre el del contrato» (L618), así que no la decide. Las dos divergen cuando la cadena ya acreditó un RED y la entrega no toca '
      '`ChainRedFiles` (RED-DIV-EXPECTRED: la regla anterior admitía, y sin designar exigía, un TRX de `RedSha` que 16.8 no pide; caso realista en '
      'cualquier delegación posterior a la acreditación) y, en principio, con `ChainRedSha` nulo sin ninguna prueba `ExpectRed` (RED-DIV-NULL). El '
      'constructor y el validador implementan ahora la condición de 16.8; en el banco (FX-U1) y en el tipo B1 `ChainRedSha` es nulo, así que sus resultados '
      'no cambian (q1 TRX-*, q3, q4).')
    a('')
    a('Observaciones (no se reetiquetan; ninguna cambia un resultado):')
    a('')
    a('1. **Cita de la definición de RED (aplicada por `fix_a7r2_c.py`).** El punto 1 (c) cita ahora «(16.8; 16.9, `Ci` y `Tests`)» y la fila L16 de §2 '
      'cita AP 16.8 L616-L617 con el literal de «**exige RED**»; G3 lo comprueba sobre esas líneas del blob fijado de AP (filas con varias fuentes '
      'separadas por «;»: un grupo de líneas por fuente; para las filas de una sola fuente el análisis es el mismo de antes).')
    a('2. **Exigencia de RED indeterminada (aplicada por `fix_a7r2_c.py`; RED-UNDETERMINED).** La regla 5 (c) dice ahora que, si la exigencia de RED de '
      '16.8 no se puede determinar porque falta la entrega, el TRX de `RedSha` ni se admite ni se exige y se registra la causa: es lo que ya hacían las '
      'herramientas (designado: NOT_ADMITTED con la causa UNDETERMINED; sin designar: ninguna ausencia por él y la causa en `RedBasis`), sin cambio de '
      'código.')
    a('3. **Medición sin hechos del relevo (aplicada).** Para distinguir la medición de una verificación, `a7-designation/2` lleva ahora '
      '`Invocation.Kind`; sin esa marca, unos hechos del relevo designados por error en la medición entrarían en el cierre (RELAY-MEASUREMENT-DESIGNATED). '
      'Punto del kit: la designación del kit tiene que marcar la invocación de medición.')
    a('4. **Piezas del invalidador (aplicada).** Un cambio de `designate_a7.py` o de `derive_r4.py` es un cambio del invalidador aunque el E de la '
      'invocación no cambie (INV-DERIVATION); con la lista de la primera versión de la ronda 2 no se habría notado. La disposición que nombre el '
      'procedimiento tiene que identificar los dos archivos (o los que implementen la derivación en el kit).')
    a('5. **`Ci` sin RED exigido.** Cuando la entrega no exige RED, `Ci` `RedPart` es `not_applicable` «citando la corrida del `ChainRedSha`»: cita la '
      'corrida (hechos remotos del relevo), no lee su TRX, y `Tests` `RedPart` es `not_applicable`; por eso el cierre no lleva TRX de esa corrida. Coherente '
      'con el punto 1 (c).')
    a('6. **RunId de los controles del banco** (punto del kit, sin cambio). El patrón de `RunId` de relay-record/v2 no admite los `RunId` de los controles '
      'del banco (p. ej. `…-m7c2`); ya conocido (decisiones §92, punto 6) y sin efecto en R10, que solo compara el SHA-256.')
    a('7. **Formato de designación del kit** (punto del kit, sin cambio). El kit de la medición (`a6m7-measure`) usa `a6m7-designation/1`, que no registra '
      'los blobs de la delegación, la entrega y el TRX ni los papeles TRX de `RedSha` y RELAY: la próxima preparación del kit tiene que adoptar '
      '`a7-designation/2` (o equivalente, con la marca de la medición) y designar el TRX de `RedSha` cuando la entrega exija RED y los hechos del relevo.')
    a('')
    a('## 12. Archivos')
    a('')
    a('Publicables en `docs/automation/evidence/I-62-A7/r2/` (este directorio). `MANIFEST.sha256` lista todos, incluido este documento.')
    a('')
    a('| Archivo | SHA-256 |')
    a('|---|---|')
    for n, h in _files():
        a('| `%s` | `%s` |' % (n, h))
    a('')
    return '\n'.join(L) + '\n'
