# I-62 — Kit de la revisión formal de la ronda 4 de A-6 por un Architect independiente con `claude-cli`, acotada a A62-A6R3-R1 (R20261014T180556Z-5e02)

```text
LogicalReviewRequestId: L20261014T180556Z-5e02   InvocationId: I20261014T180556Z-5e02   AttemptSeq: 1   RunId: R20261014T180556Z-5e02
SessionId:      4cfdcf7d-0aa5-4a96-98ac-0c0a4832ec5b (uuid4, generado una vez el 2026-10-14T18:05:56Z con los demás ids; prep/ids.txt)
Preparación:    decisiones §85, punto 6 («Preparar el kit de revisión acotado a R1 y sus consecuencias directas. No lanzar revisión formal de
                ronda 4 hasta nueva disposición del Coordinator»); cuerpo pegado de §85: 2 805 bytes, SHA-256
                d74e2aa1ce5eacc7dd6323cf3f3acec37fd6eb25f905ca4f2479dbca0cde5232 = kit/order.txt = run/order.txt; registrado en decisiones §85 (línea
                1684, commit ceb85977)
Autorización:   PENDIENTE. El lanzamiento exige una disposición del Coordinator POSTERIOR a §85 que todavía no existe. kit/owner_gate.py la fija con
                AUTH_SECTION, AUTH_HEADING y AUTH_TEXT_SHA, que este kit entrega en None: mientras falte cualquiera, la compuerta 11 BLOQUEA con
                «authorization pending: a Coordinator disposition after §85 must be pinned (section number, heading, text SHA-256) and the kit
                re-frozen». Procedimiento para fijarla y volver a congelar: sección «Fijar la autorización y volver a congelar»
Consumo:        cubierto por CLAUDE-CLI-I62 = A (revisiones de ARCHITECT de I-62, solo lectura), la misma base que las rondas 1 a 3
Objeto:         docs/initiatives/I-62-A-6.md, ronda 4, PROPUESTA, blob 4bc919dd4768ab81abc74b70d3a4a3821c07feb5 (151 865 bytes, 1 447 líneas),
                publicado en d66f7f4b6a7031938588378b6eef7f1c3d4e4cf7 (padre ceb85977, con la ronda 3, e62f27b8, y decisiones §85); guardas `run`
                PASS custodiadas en 088b3227a22ef6ab28b7ba43d5aa90de68d5b08f
Commit del clon: 088b3227a22ef6ab28b7ba43d5aa90de68d5b08f (HEAD de la rama; contiene §85, la publicación y la custodia de las guardas; §85 es su
                última sección de decisiones)
Ronda 3:        blob e62f27b8b1329f1b9cfa69a707feb31d22bad1d1, en el run como a6-r3.md (kit/a6-r3.md, 135 665 bytes, 1 320 líneas, extraído con
                `git cat-file blob` del worktree, solo lectura; sustituye al a6-r2.md de la ronda 3); salida de la revisión R20261013T183701Z-9f80
                en el cierre (output.json, 691bbc8f)
Clon / run:     D:\r62-arch-a6r4 (HEAD separado en 088b3227, sin ramas ni remoto; clonado desde el directorio común de git) y D:\r62-arch-a6r4-run
                (order.txt = day3\s85_body.txt, prompt.md = kit/prompt.md, a6-r3.md = blob e62f27b8)
Transporte:     el de las rondas 1 a 3 sin cambio: claude-cli 2.1.293, %APPDATA%\Claude\claude-code\2.1.293\83cb0bd7fed4\claude.exe, SHA-256
                8693c4a02dde7441d0066ede68af8ddfc408bb982d77e12b506286268224e6fa; claude-opus-5-5, effort xhigh; Read, Grep y Glob (+ StructuredOutput);
                --safe-mode, sin MCP; --add-dir del run; prompt por stdin; resultado por --json-schema; tope de 3 600 s; espera de 120 s tras
                `auth status`; ArgsTemplate idéntica a la de las rondas anteriores
Orden:          EXACTAMENTE `python -B kit/launch.py`, desde la carpeta que contiene kit/ (esta), sin flags ni variables añadidos; la guarda de
                launch.py rechaza cualquier otra
Compuerta D-01: MEASURED sobre R20261008T183600Z-char (claude-cli-characterization.json, SHA-256 20d0de2c…, blob 541956eb), fijada sobre la
                copia del clon D:\r62-arch-a6r4; transport_gate.py byte a byte el de la ronda 3
Compuerta 11:   11-OwnerAndAuthorizationGate (kit/owner_gate.py), BLOQUEANTE: línea del Owner de §78; CLAUDE-CLI-I62 = A y el mismo binario; A-6
                en la HEAD = 4bc919dd; decisiones solo por añadido desde el clon; la HEAD desciende del clon; y la autorización: PENDIENTE (bloquea)
                hasta fijar la disposición posterior a §85; fijada, la lógica de la ronda 3 (encabezado, blob completo como token, SHA-256 del texto,
                una sola sección y BLOQUEO ante cualquier sección posterior), con el número posterior a §85
Estado:         kit preparado, probado y CONGELADO con la autorización pendiente (kit/kit-manifest.json, SHA-256 6c8807beb692f8604ce2d2381aac676fac5664b9862b1cd2b141d798e36b6553); sonda de
                registro en Ok; prueba en seco con BlockingChecks = ["11-OwnerAndAuthorizationGate"] y el único problema «authorization pending: …».
                NO lanzado: ningún claude.exe ni CLI de modelos ejecutado; ni red ni credenciales
```

## Qué pide la orden del kit y dónde lo cubre

| Exigencia (orden del kit y decisiones §85, punto 6) | Dónde |
|---|---|
| Derivar el kit de la ronda 4 del kit acreditado de la ronda 3 por sustituciones exactas con diffs, cambiando solo lo que la ronda 4 necesita | `prep/adapt_*.py` con `prep/subst.py` (cada fuente fijada por su SHA-256 del manifiesto custodiado de la ronda 3, 771d9f46…; cada sustitución, una sola coincidencia) para `launch.py`, `owner_gate.py`, `post-review.py`, `prepare_clone.py`, `verify_clone.py`, `mutation-post-review.py`, `make_manifest.py` y `finish_kit.py`; adaptados a mano con su diff (`prep/make_diffs.py`): `make_closure.py`, `prompt.template.md`, `selftest-post-review.py`, `result.schema.json` y `prep/gen_schema.py`; byte a byte (`prep/copy_premises.py`): `transport_gate.py`, `c3_analysis.py` y `read_fidelity.py`; nuevo: `pin_authorization.py` |
| 1. Ids nuevos, generados una vez | `prep/ids.txt` (RunId, InvocationId, LogicalReviewRequestId con el mismo sufijo, AttemptSeq 1, SessionId uuid4) |
| 2. Clon en la HEAD del worktree, como en la ronda 3; `verify_clone.py` reproduce las guardas de la ronda 4 | `kit/prepare_clone.py 088b3227… d66f7f4b… 088b3227…` y `kit/verify_clone.py` (`a6r4-guards.py run` ceb85977..d66f7f4b y `self-test`, ambas iguales enteras a las custodiadas) |
| 3. Cierre | `kit/make_closure.py` → `kit/closure.json` (abajo, «Contrato y auditor») |
| 4. Prompt y esquema acotados a A62-A6R3-R1 y sus consecuencias directas | `kit/prompt.template.md` → `kit/prompt.md`; `prep/gen_schema.py` → `kit/result.schema.json` (`rackcad-a6r4-architect-review/0`) |
| 5. Auditor v5.4-a6r4 | `kit/post-review.py` (`prep/adapt_post_review.py`): las reglas de v5.3 byte a byte salvo la coherencia del esquema y el registro de la compuerta 11; Outcome (categoría de ejecución) y Verdict (Architect) por separado |
| 6. Lanzador, compuerta de transporte y sonda de registro sin cambio de lógica | `kit/launch.py` (`prep/adapt_launch.py`: solo constantes, docstring y dos comentarios); `kit/transport_gate.py` byte a byte |
| 7. Compuerta 11 bloqueante con la autorización pendiente; `pin_authorization.py`; los dos estados en la autoprueba | `kit/owner_gate.py` (`prep/adapt_owner_gate.py`), `kit/pin_authorization.py`; casos O00-O30, L00 y 150-151p de la autoprueba |
| 8. Congelación, README y diffs | `kit/kit-manifest.json` (`kit/make_manifest.py`), este README, `prep/diffs/` |
| 9. Sonda de registro y prueba en seco desde esta carpeta | `record-probe.json` y `launch-dry-run.json` (abajo, pasos 10 y 11) |

## Independencia del preparador

Quien preparó este kit es un subagente de la sesión principal (la sesión que preparó A-6 y sus cuatro rondas). Leyó la orden del kit, decisiones
§83-§85, el kit acreditado de la ronda 3 entero (README, componentes y scripts de preparación), la salida de la revisión de la ronda 3 (hallazgos
A62-A6R3-R1 y O1-O7, `NegativeCaseCoherence`, identidad y Step0), el diff de A-6 entre las rondas 3 y 4 y, de la A-6 de la ronda 4, las partes que
el cierre y el prompt citan por líneas (A6-3, regla 6; la tabla de derivación de §3.12; §3.17; §3.18; §4; §7; §8). De `r4/` leyó la cabecera de
`a6r4-guards.py`, la estructura de G11 del resultado `run` y los MANIFEST de `readable/`; no leyó el contenido de las sondas de `r4/r1probes/`, la
implementación independiente ni `bmin_accept.py`. El prompt no contiene veredicto esperado ni valoraciones de la preparación; las definiciones de los
estados del esquema son de forma (qué significa cada valor y qué exige el auditor), no respuestas. La evidencia de I-62 (`I-62-evidence.md`) está en
el cierre, como en las rondas 2 y 3, presentada como evidencia y no como autoridad. El kit no ejecuta el paquete B-min, el aceptante ni las sondas;
solo reproduce las guardas de la ronda 4.

## Contrato y auditor

- **Cierre** (`kit/make_closure.py`, en el commit del clon; cada archivo por su blob; rangos calculados desde encabezados, filas y líneas citadas):
  A-6 de la ronda 4 entera (mapa de secciones; reglas de cada delta; A6-3, regla 6 (b), L600-617, con su encabezado, sus apartados (i)-(v) y su frase
  final; la tabla de derivación de §3.12, L848-872, y su párrafo; G1-G6; las tablas, filas y párrafos de §3.17 y de §3.18, con sus cuatro viñetas;
  la fila de §4 de N4 y nc1; las líneas del Owner y los párrafos «Ronda 3» y «Ronda 4» de §7; las filas de §8 Q-A6R3-01..04 y Q-A6R4-01..04);
  `docs/automation/evidence/I-62-A6/r4/` entero (219 archivos, con `readable/` y su MANIFEST comprobado); de `r3/`, `r1probes/` entero, sus copias
  legibles (con el MANIFEST de `r3/readable/`, comprobado entero) y los seis archivos de `r3/` que citan `r4/` o A-6 (`a6r3-guards.py`,
  `launchfix/README.md`, `make_candidate_commit_a6r3.py`, `neg_controls_a6r3.py`, `preflight.md`, `r2static/README.md`; lista fijada R3_CITED y
  comprobada mecánicamente sobre los textos): 41 archivos; de `r2/`, `independence/` entero y sus copias legibles (con el MANIFEST de `r2/readable/`,
  comprobado entero): 24 archivos; la salida de la revisión de la ronda 3 (`output.json`, 691bbc8f) con rangos por A62-A6R3-R1, A62-A6R3-O1..O7 y
  `NegativeCaseCoherence`; el paquete B-min entero (árbol 3caa0dfa); la base normativa de la ronda 3 (V14, AUTOMATION_PLAN, ADR-0046, ADR-0048,
  LIFECYCLE, A-1..A-5, agent-execution README con §10 L219-L228, routing.md, adapters/codex-cli.md, cinco esquemas, controller-contracts.md con L23 y
  L44-L45) y decisiones §43, §57, §61, §64 y §73 en adelante, con las filas de §84 y §85 y sus líneas de bytes y SHA-256; toda fuente de G11 del
  resultado `run` custodiado de la ronda 4 (comprobado igual a `r4/a6r4-sources-for-reviewer.json`), con sus rangos, incluida `I-62-evidence.md`
  (§140 en adelante; evidencia y no autoridad; prefijo comprobado del blob que fija G11); run: `order.txt` y `a6-r3.md` (premisas) y `prompt.md`.
  **Excluidos:** `docs/automation/evidence/I-62-C1C3/` y todo material de I-65; el resto de `r2/` y de `r3/` (incluidos los directorios
  `r3/r2static/` y `r3/launchfix/`, que A-6 solo nombra); la evidencia de la ronda 1; las salidas de las revisiones de las rondas 1 y 2 y el dictamen
  de B-min; el resto de los directorios de las revisiones; transcripciones, memoria, fixture. Comprobaciones de identidad: el padre de la
  publicación tiene `e62f27b8`; la publicación, la custodia de las guardas y el clon, `4bc919dd`; `r4/` sin cambio tras la publicación salvo el
  resultado `run`; el resultado `run` = Mode RUN, Base = padre, Head = publicación, PASS; la autoprueba = PASS con A6TextBlob, ToolBlob (56b59c50)
  y 815/815; decisiones crece por añadido desde `4e090ed4` (ceb85977, §85 registrada) y §85 es su última sección numerada; las líneas del Owner una
  vez en A-6 y en decisiones; los blobs de G11 iguales en el clon (salvo decisiones y la evidencia, que crecen por añadido); árboles fijados de
  B-min, agent-execution, `r2/`, `r2/independence/`, `r3/` y la revisión de la ronda 3; A-6 §5 «Activado» = M-01 no, M-02..M-05 sí, M-06 y M-07 no,
  M-08 sí.
- **Herramientas y lectura:** las de las rondas 1 a 3, sin cambio (solo Read, Grep y Glob, y StructuredOutput; archivos de más de 60 000 bytes por
  tramos de 300 líneas; Grep solo sobre archivos; Glob solo dentro del cierre; toda denegación es violación; toda referencia solo a líneas leídas con
  Read en ese mismo archivo; lectura directa obligatoria de V14 L2333-L2334).
- **Prompt** (`kit/prompt.template.md` → `kit/prompt.md`): neutral (sin veredicto esperado); explica el objeto de la ronda 4, los ocho puntos
  acotados (cierre de A62-A6R3-R1; regla 6 (b) total y mecánica, sin juicio, sin insumos introducidos ni sustituidos; compatibilidad con A6-9,
  regla 4; N4 y nc1; evidencia de `r4/r1probes/`; pureza del delta; O1-O7 frente a §85, punto 4; Q-A6R4-01..04), que lo que dependa de una sesión
  real sigue como precondición con fallo cerrado, que los demás aspectos no se piden pero cualquier defecto material puede señalarse, la regla de
  lectura, la lista G11, que `a6-r3.md` es el blob `e62f27b8` y que la compuerta 11 comprobó, antes del lanzamiento, la disposición posterior a §85
  fijada en el kit (posterior al commit del clon y fuera del cierre). 105 320 bytes, 715 líneas, SHA-256 `a8dc5773…`.
- **Esquema** (`kit/result.schema.json`, `rackcad-a6r4-architect-review/0`; 11 409 caracteres ASCII en forma compacta; `prep/gen_schema.py`): solo las palabras clave ya medidas
  con claude-cli 2.1.293. Campos: identidad, Step0 (siete booleanas: V14 directa, orden entera, §85 declara bytes y SHA-256, §85 dispone la ronda 3,
  resultado `run`, A6TextBlob, salida de la ronda 3), DirectReads, Reads, hallazgos nuevos `A62-A6R4-R<n>` / `A62-A6R4-O<n>`,
  `Round3FindingResolution` (A62-A6R3-R1: CORRECTED | PARTIALLY_CORRECTED | NOT_CORRECTED), `ClosureRuleMechanical` (Assessment
  TOTAL_AND_MECHANICAL | NOT_TOTAL_OR_NOT_MECHANICAL; `Coverage` de N4_ABSENT_DELIVERY, NC1_NONEXISTENT_COMMIT y ABSENT_NULL_EMPTY_FIELDS con
  COVERED | NOT_COVERED y JudgmentStepRequired; `SubRules` i..v con IntroducesInput y SubstitutesInput yes | no; con referencias en cada elemento),
  `A69Compatibility` (NONE | CONTRADICTIONS con la lista), `NegativeCaseCoherence` (N4, NC1), `EvidenceAdequacy` (DERIVATION,
  INDEPENDENT_IMPLEMENTATION, SUBSTITUTIONS, ADDED_OR_INVENTED_INPUTS, ACCEPTOR_RUNS, O3_STRICT: ADEQUATE | INADEQUATE), `DeltaPurity` (CONFIRMED |
  NOT_CONFIRMED), `OptionalDispositionConformance` (A62-A6R3-O1..O7: CONFORMS | DOES_NOT_CONFORM), `QuestionAnswers` (Q-A6R4-01..04: Outcome
  SATISFIED | NOT_SATISFIED y Answer), OwnerAuthorityCheck, IfAgreed (diez campos), KnownLimitations, RecommendedNextAction. Veredictos: AGREED,
  CHANGES REQUIRED, REJECTED, BLOCKED — OWNER DECISION, NOT_ACCREDITED.
- **Compuerta de transporte** (`kit/transport_gate.py`): byte a byte la de la ronda 3 (D-01 y la decodificación de Windows). `kit/c3_analysis.py` y
  `kit/read_fidelity.py`, byte a byte.
- **Compuerta 11** (`kit/owner_gate.py`; diff `prep/diffs/owner_gate.diff`; comprobación 11 de `launch.py`, BLOQUEANTE, evaluada también en seco
  porque solo lee git, con GIT_OPTIONAL_LOCKS=0): (a) la línea del Owner `A6-ADR0046-9-FX02 = A (…)` (SHA-256 `83152eae…`) como línea completa
  después de `## 78.`, ninguna otra «A6-ADR0046-9-FX02 =» y la misma línea en la A-6 de la ronda 4 del commit del clon; (b) `CLAUDE-CLI-I62 = A`
  (SHA-256 `fbeade41…`) como línea completa, ninguna otra «CLAUDE-CLI-I62 =», y el SHA-256 medido del binario = el del cierre; (c) A-6 en la HEAD =
  `4bc919dd` (distinto de los blobs de las rondas 1 a 3); (d) **autorización**: `AUTH_SECTION`, `AUTH_HEADING` y `AUTH_TEXT_SHA` en None (como se
  entrega el kit) ⇒ BLOQUEO con `PENDING_REASON` y sin evaluar ninguna sección (el registro lleva `AuthorizationPending` = true,
  `AuthorizationSections` = [], `LaterSections` = [] y, solo como información, `SectionsAfterBase`, las secciones posteriores a §85); con las tres
  fijadas, la lógica de la ronda 3: el número posterior a §85, exactamente una sección con ese número, su encabezado fijado, el blob completo de la
  ronda 4 como token literal y el SHA-256 de su texto fijado; (e) con la autorización fijada, **ninguna sección numerada después de ella** (la lista
  en `LaterSections`); además, decisiones del árbol de trabajo = la de la HEAD, la HEAD desciende del commit del clon y las decisiones del clon son
  prefijo exacto de las de la HEAD. `evaluate()` recibe la autorización como registro (`auth`; por defecto, la del módulo) para que la autoprueba
  ejerza los dos estados sin tocar el archivo.
- **`kit/pin_authorization.py`** (nuevo): dado el archivo de decisiones (texto del blob, sin CR) y un número de sección, imprime los tres valores
  que hay que fijar, las tres líneas de Python que sustituyen a las que llevan None y las comprobaciones de la sección con las mismas funciones de la
  compuerta (`section_text`, `authorization_sections`, `later_sections`): número posterior a §85, una sola sección, el blob completo de
  `closure.json`, ninguna sección posterior. No edita el kit ni ningún archivo; código 0 solo si todo está listo.
- **Auditor v5.4-a6r4** (`kit/post-review.py`, derivado de v5.3-a6r3 con `prep/adapt_post_review.py` por once sustituciones exactas y la docstring;
  diff `prep/diffs/post-review.diff`): las reglas de lanzamiento, compuerta D-01, stream-json, transcripción, herramientas, lectura, fidelidad,
  referencias, lectura directa de V14, custodia del run, clon, `run.json` (Outcome, OutcomeCategories, PostRunErrors, campos), FrozenCommandGuard,
  RecordProbe, Environment y `Categories` quedan byte a byte. Cambios: el registro de la compuerta 11 en `preflight.json` (la autorización fijada en
  el `owner_gate.py` del kit, con una regla nueva para la autorización pendiente o un registro con `AuthorizationPending` distinto de false, y la
  regla de la ronda 3 comparada con los valores fijados y con `A6R4Blob`); la coherencia del esquema nuevo (unicidad y cobertura de los siete
  arreglos, anidados incluidos; cada estado no favorable ⇒ un REQUIRED vinculado, y su opuesto sin REQUIRED: R1 no CORRECTED, caso NOT_COVERED o con
  juicio, apartado que introduce o sustituye, regla NOT_TOTAL, A69 CONTRADICTIONS (con la lista), N4/NC1 INCOHERENT, DeltaPurity NOT_CONFIRMED, y
  aspecto obligatorio de §85, punto 3, INADEQUATE; O3_STRICT INADEQUATE, opcional DOES_NOT_CONFORM o pregunta NOT_SATISFIED ⇒ un hallazgo
  vinculado; TOTAL_AND_MECHANICAL exige todos los casos cubiertos sin juicio y todos los apartados no/no; ClauseRefs en cada elemento; ningún
  NOT_DETERMINED con veredicto; todo REQUIRED citado; AGREED con todo lo exigido; CHANGES REQUIRED o REJECTED con algún REQUIRED); y la retirada
  de la regla de referencias de AcceptorCodeContrast, campo que el esquema nuevo no tiene. `collect_refs` recoge también los ClauseRefs de
  `ClosureRuleMechanical.Coverage` y `.SubRules`. Además, `validate_schema` exige Test-Json Y una comprobación en proceso del subconjunto de
  palabras clave medido (`schema_errors`): el pwsh 7.6.7 instalado el 2026-10-14 (desviaciones, abajo) solo valida con Test-Json el primer nivel de
  anidamiento.
- **Lanzador** (`kit/launch.py`, `prep/adapt_launch.py`, diez sustituciones exactas y la docstring; diff `prep/diffs/launch.diff` frente al
  `launch.py` de la ronda 3): G, R, P, D, las comprobaciones 1-11, Outcome, entorno, binario, modelo, effort, tope y espera, byte a byte; cambian el
  clon y el run, el session id, el directorio y el patrón de proyecto (`D--r62-arch-a6r4*`), `a6-r3.md` (blob e62f27b8) en lugar de `a6-r2.md`, el
  objeto del cierre distinto también del blob de la ronda 3 y dos comentarios de la salida de `--dry-run`. No reintenta. `--dry-run` y
  `--record-probe` no ejecutan el binario.
- **Ayudantes:** `prepare_clone.py` (clon desde el directorio común de git y run; en el commit del clon, §85 como última sección; `kit-params.json`),
  `make_closure.py`, `verify_clone.py` (guardas de la ronda 4 reproducidas; worktree sin cambio de HEAD ni de estado), `make_manifest.py` (la
  cabecera registra la autorización fijada o pendiente y el SHA-256 de `owner_gate.py` en la autoprueba y en la mutación frente al actual),
  `finish_kit.py` (con la autorización pendiente, exige que el único bloqueo de la prueba en seco sea la compuerta 11 con el único problema
  «authorization pending: …»), `mutation-post-review.py`; `read_fidelity.py` sin cambio.

## Preparación ejecutada (2026-10-14): resultados

Pasos de `kit/finish_kit.py`, ejecutados uno a uno con sus mismos comandos para revisar cada resultado; salidas en `prep/phase/`:

1. `prepare_clone.py 088b3227… d66f7f4b… 088b3227…` → `D:\r62-arch-a6r4` (`git clone --no-local -c core.autocrlf=false --single-branch --branch
   architecture/portabilidad-coordinador-principal <directorio común de git> D:\r62-arch-a6r4`; HEAD separado en `088b3227`; sin ramas ni remoto;
   reflog y gc) y `D:\r62-arch-a6r4-run` (order.txt, a6-r3.md); `kit/kit-params.json` (SHA-256 `c8d805f6…`; padre de la publicación `ceb85977…`;
   añadido a `r4/` tras la publicación: solo el resultado `run`). Precondiciones: todas en true (`prep/phase/step1.out`).
2. `make_closure.py --render-prompt` → `kit/prompt.md` (105 320 bytes, 715 líneas, SHA-256 `a8dc5773…`). Se renderizó dos veces: la primera
   (105 316 bytes) antes de un cambio de redacción neutral de la plantilla («Su registro y su evidencia, descriptivos» → «Lo que A-6 declara
   descriptivo en la ronda 4»); los pasos 3, 4 y 6 se repitieron después. 3. copiado al run, idéntico.
4. `make_closure.py` → `kit/closure.json` (SHA-256 `8c70a195…`): **337 archivos, 6 973 215 bytes, 129 476 líneas** (r4: 219 archivos, 2 469 276
   bytes, 61 652 líneas; r3: 41, 983 733, 16 010; r2: 24, 971 662, 20 283; paquete B-min: 30, 541 797, 10 175; el resto, 23 archivos, 2 006 747
   bytes, 21 356 líneas). **Ningún archivo no UTF-8.** Líneas de más de 2 000 caracteres sin copia legible: los seis sobres de `bmin/e2e/out/`, V14,
   A-3, decisiones y `prompt.md` (los JSON de una línea de `r2/independence/`, `r3/r1probes/` y `r4/r1probes/` tienen su copia en `readable/`).
   MANIFEST de `r2/readable/`, `r3/readable/` y `r4/readable/` comprobados enteros. C1-C3 e I-65 fuera.
5. Compuerta D-01: `transport_gate.py init` + `set MEASURED` sobre `D:\r62-arch-a6r4\…\claude-cli-characterization.json` (SHA-256 `20d0de2c…`) →
   `kit/transport-gate.json` en MEASURED (SHA-256 `b0bf2d95…`).
6. `verify_clone.py` → **AllChecks = true** (2026-10-14T19:59:43Z-20:04:21Z; `kit/clone-verification.json`, SHA-256 `16d9290c…`): HEAD, separado,
   sin ramas ni remoto, limpio, 4 467 archivos en el árbol, sin enlaces, blobs del cierre, historia del objeto, commits, run, caracterización;
   guardas de la ronda 4 reproducidas desde el clon contra el worktree (solo lectura): **`run` ceb85977..d66f7f4b PASS, igual entera a la
   custodiada** y **`self-test` 815/815 mutantes muertos, igual entera a la custodiada**; worktree sin cambio de HEAD ni de estado. (La primera
   ejecución, 18:30:52Z-18:36:19Z, antes del segundo renderizado del prompt, dio el mismo resultado.)
7. Autoprueba sobre el clon y el run REALES (`kit/selftest-result.json`, SHA-256 `eac70103…`): **297/297 PASS** — 195 casos de auditoría, 61 de la
   compuerta de transporte (G00-G58), 37 de la compuerta 11 (O00-O30 y la sonda L00) y 4 de coherencia lanzador-auditor (X01-X04) —, con el clon
   limpio y los tres archivos del run idénticos a sus copias al terminar; 2026-10-14T20:04:27Z-20:57:28Z. Los casos de auditoría usan la
   autorización SINTÉTICA (§86). **L00** (worktree en `088b3227`): estado PENDIENTE, compuerta 11 sin Ok con el único problema «authorization
   pending: …», `AuthorizationSections` = [], `LaterSections` = [], `SectionsAfterBase` = []. Antes de la autoprueba completa, las ejecuciones
   parciales (`prep/dev/quick_cases.py`; salidas `prep/dev/quick-a1.out` y `quick-a2.out`) encontraron tres defectos, corregidos: el TypeError de
   `owner_gate.later_sections` con un número no entero (caso O10b), la regresión de Test-Json del pwsh 7.6.7 en los objetos anidados (casos 95 y
   112-117; de ahí `schema_errors` en el auditor y los casos 118b-118f) y la expectativa del caso 151p.
8. Mutación sobre el clon real (`kit/mutation-result.json`, SHA-256 `424c983e…`): **191/191 mutantes muertos**, base 296/296 (sin L00):
   `post-review.py` 133/133 (`4f46caa6…`), `transport_gate.py` 40/40 (`e326b5b9…`), `owner_gate.py` 18/18 (`d442608a…`); 2026-10-15T08:42:14Z →
   11:18:07Z. 3 de ellos (los sitios L289, L292 y L298 de `decode_cli_output`, como en la ronda 3) muertos por excepción, registrada en `KilledBy`.
9. `make_manifest.py` → `kit/kit-manifest.json` (congelación): 25 archivos de `kit/` y los tres del run; `AuthorizationState` PENDIENTE;
   `OwnerGateTested`: el mismo `owner_gate.py` (`d442608a…`) en la autoprueba, en la mutación y en el kit; **SHA-256
   `6c8807beb692f8604ce2d2381aac676fac5664b9862b1cd2b141d798e36b6553`**.
10. `python -B kit/launch.py --record-probe` (desde esta carpeta, orden congelada exacta; 2026-10-15T11:18:35Z) → código 0: `RecordProbe.Ok` y
    `FrozenCommandGuard.Ok` en true (argv ['-B', 'kit/launch.py', '--record-probe']); registro sustituto con Outcome COMPLETED; `tasklist` con 89
    bytes, no ASCII (consola cp850), tratado en bytes; `ProcessGoneAfter` true; temporal borrado; sin `<run>/launch`; listado del kit sin cambios.
    `record-probe.json` (UTF-8, SHA-256 `cfe6ba3f…`; bytes crudos en `prep/phase/step10.raw`, tubería cp1252).
11. `python -B kit/launch.py --dry-run` (ídem; 2026-10-15T11:18:36Z) → código 2 (siempre en seco): **`BlockingChecks` =
    ["11-OwnerAndAuthorizationGate"]**, con el único problema «authorization pending: a Coordinator disposition after §85 must be pinned (section
    number, heading, text SHA-256) and the kit re-frozen»; comprobaciones 1-3 y 6-10 en Ok; `NotRunInDryRun` = ['4-Version', '5-Auth'];
    `FrozenCommandGuard.Ok` (argv ['-B', 'kit/launch.py', '--dry-run']); `RecordProbeOk` = True; `AuthorizationSections` = []; `LaterSections` = [];
    compuerta 11: `AuthorizationPending` true, líneas del Owner A6-ADR0046-9-FX02 [1572] (§78 en la línea 1564) y CLAUDE-CLI-I62 [1085], A-6 en la
    HEAD = fijado, decisiones por añadido, worktree limpio, desciende del clon, binario = cierre, `SectionsAfterBase` = [], HEAD del worktree
    `088b3227`; comprobación 9: 13 441 caracteres; `Environment`: 33 nombres (valores solo de las PYTHON*: PYTHONDONTWRITEBYTECODE, PYTHONUSERBASE).
    `launch-dry-run.json` (UTF-8, SHA-256 `eba7cf4a…`; bytes crudos en `prep/phase/step11.raw`).

## Fijar la autorización y volver a congelar (después de la disposición del Coordinator posterior a §85)

El kit se entrega con la autorización PENDIENTE: `python -B kit/launch.py` se niega (código 2) por la compuerta 11 mientras `kit/owner_gate.py`
tenga `AUTH_SECTION`, `AUTH_HEADING` y `AUTH_TEXT_SHA` en None. Cuando el Coordinator dicte la disposición que autorice el lanzamiento y la sesión
principal la registre en decisiones (por añadido, en la rama de I-62; con el blob completo `4bc919dd4768ab81abc74b70d3a4a3821c07feb5` en su texto):

1. Guardar el texto del blob de decisiones de la HEAD sin conversión de fin de línea (en Git Bash, no en PowerShell, que reescribe los fines de
   línea): `git -C "$USERPROFILE/.codex/worktrees/architecture-portabilidad-coordinador-principal" cat-file blob HEAD:docs/automation/decisions/I-62.md > <temporal>/decisiones.txt`.
2. Desde esta carpeta: `python -B kit/pin_authorization.py <temporal>/decisiones.txt <n>` (n = el número de la sección nueva). Exige `Ready` = true
   (código 0): número posterior a §85, una sola sección con ese número, el blob completo como token, ninguna sección posterior; imprime los tres
   valores y las tres líneas `PythonLines`. No edita nada.
3. Sustituir en `kit/owner_gate.py` SOLO las tres líneas `AUTH_SECTION = None …`, `AUTH_HEADING = None …` y `AUTH_TEXT_SHA = None …` por las tres de
   `PythonLines` (el diff frente al `owner_gate.py` congelado, SHA-256 `d442608a77b78d96546c042cc396cb1b553c69673a5368aa526953f63060b049`, debe tener exactamente esas tres líneas). Nada más cambia: ni
   `closure.json` (no fija la autorización), ni `prompt.md` (no la nombra), ni el auditor (compara con los valores del `owner_gate.py` del kit).
4. Volver a congelar: `python -B kit/make_manifest.py` (manifiesto completo; su cabecera pasa a `AuthorizationState` FIJADA y registra en
   `OwnerGateTested` que `owner_gate.py` cambió desde la autoprueba y la mutación, que se ejecutaron sobre la lógica idéntica con la autorización
   pendiente; la autoprueba puede repetirse entera, `python -B kit/selftest-post-review.py <salida>`, y entonces L00 exige el estado FIJADO).
5. Con la orden congelada, desde esta carpeta y en el mismo entorno del lanzamiento: `python -B kit/launch.py --record-probe` (código 0) y
   `python -B kit/launch.py --dry-run` (código 2, `BlockingChecks` = [], `AuthorizationSections` = [§n con HeadingMatches, HasBlob y
   TextSha256Matches], `LaterSections` = []).
6. Custodiar el kit vuelto a congelar y lanzar una vez (abajo).

## Condiciones exactas para el lanzamiento (nada lanzado)

1. La disposición del Coordinator posterior a §85 fijada en `kit/owner_gate.py` y el kit vuelto a congelar y custodiado (sección anterior). Si
   cambia cualquier otro archivo de `kit/`, `make_manifest.py` (o `--gate` tras un `transport_gate.py set`) y volver a custodiar.
2. Que la rama siga descendiendo de `088b3227…`, que decisiones crezca solo por añadido desde el clon y sin ninguna sección posterior a la fijada,
   que A-6 en la HEAD siga siendo `4bc919dd…`, que la línea del Owner de §78 y la de CLAUDE-CLI-I62 = A sigan como líneas completas únicas y que el
   binario siga siendo el de SHA-256 `8693c4a0…`.
3. En la MISMA herramienta, shell y entorno que se usarán para lanzar, desde esta carpeta, sin flags ni variables añadidos: `python -B
   kit/launch.py --record-probe` → código 0; `python -B kit/launch.py --dry-run` → código 2 con `BlockingChecks` = []; 4 y 5 solo se ejecutan en el
   lanzamiento real.
4. Lanzar UNA vez, con la orden congelada exacta, desde esta carpeta:

   ```text
   python -B kit/launch.py
   ```

   Sin reintento.
5. Al terminar, el auditor sobre la corrida completa, desde esta carpeta (los resultados se conservan sin modificación):

   ```text
   python -B kit/post-review.py "%USERPROFILE%\.claude\projects\D--r62-arch-a6r4\4cfdcf7d-0aa5-4a96-98ac-0c0a4832ec5b.jsonl" D:\r62-arch-a6r4-run\launch\stdout.jsonl D:\r62-arch-a6r4 D:\r62-arch-a6r4-run kit kit/result.schema.json audit.json output.json
   ```

   El auditor informa por separado `Categories.Execution` (el `Outcome` de `run.json`) y `Categories.ArchitectVerdict`; cualquier defecto de
   integridad es un motivo y deja NOT_ACCREDITED. El veredicto no es el acuerdo. Ni A-6 AGREED, ni FX-02 PASS, ni F6 GATE PASS se declaran aquí.

## Desviaciones frente a la ronda 3 (además de ids, rutas, objeto, esquema, coherencia del auditor y compuerta 11), con su motivo

- **Validación del esquema en el auditor (cambio de entorno):** el `pwsh.exe` del PATH (alias de la Microsoft Store) se actualizó el 2026-10-14 a
  las 09:33 (hora local) a la 7.6.7, cuyo Test-Json solo valida el primer nivel de anidamiento (medido con esquemas mínimos: un enum o una propiedad
  sobrante dentro de un objeto anidado pasan; `prep/dev/schema_probe*.py`). La autoprueba de la ronda 3 (2026-10-13) sí los detectaba. El auditor
  conserva la llamada a Test-Json byte a byte y exige además `schema_errors`, una comprobación en proceso del mismo subconjunto de palabras clave
  medido con claude-cli (una palabra clave desconocida es un error); el motivo «no valida contra result.schema.json» no cambia. Casos nuevos
  118b-118f (violaciones anidadas que solo ve la comprobación en proceso).
- **Veredictos:** la orden del kit nombra AGREED, CHANGES REQUIRED y REJECTED; el esquema conserva además BLOCKED — OWNER DECISION (veredicto de
  LIFECYCLE §5 cuando falta autoridad reservada, ligado a `OwnerAuthorityCheck`) y NOT_ACCREDITED (la parada con fallo cerrado del Paso 0 y de la
  falta de fidelidad, de la que dependen el prompt y el auditor, como en las rondas 1 a 3). REJECTED exige, como CHANGES REQUIRED, al menos un
  REQUIRED; el prompt lo define como «no admisible dentro de la superficie y los límites de decisiones §85, puntos 2, 4 y 7».
- **EvidenceAdequacy:** un INADEQUATE en los cinco aspectos que son pruebas obligatorias de §85, punto 3, exige un REQUIRED vinculado; en
  O3_STRICT (un opcional de §85, punto 4) basta un hallazgo vinculado. La distinción sale de la propia §85 (punto 3 frente a punto 4).
- **Preguntas de la ronda 4:** tienen campo propio (`QuestionAnswers`, con Outcome y Answer); en la ronda 3 las preguntas se contestaban dentro de
  otros campos. No hay campo `RealSessionPreconditions`: la regla (lo que depende de una sesión real sigue como precondición con fallo cerrado) va en
  el prompt.
- **Cierre:** conserva `routing.md` y `adapters/codex-cli.md`, que forman parte de la base normativa de la ronda 3 aunque la enumeración de la
  orden no los nombra; no lleva las salidas de las revisiones de las rondas 1 y 2 ni el dictamen de B-min (la orden no los pide; G3 de las guardas
  comprueba sus objetos); de `r3/` solo archivos citados (no los directorios `r2static/` y `launchfix/`, que A-6 nombra en §3.17 y que tratan de R2 y
  del lanzador); los MANIFEST de `r2/readable/` y `r3/readable/` se comprueban enteros, pero solo entran las copias cuyo canónico está en el cierre.
  Rangos de orientación nuevos: RAE §10 L219-L228 y controller-contracts.md L23 y L44-L45 (los nombra la orden).
- **`owner_gate.py`:** además de la autorización pendiente, una guarda para un `AUTH_SECTION` que no sea entero (la autoprueba, caso O10b, hizo caer
  la primera versión con TypeError en `later_sections`; se corrigió en `prep/adapt_owner_gate.py` antes de la autoprueba completa).
- **Auditor, regla de la compuerta 11:** compara también `AuthorizationSection` y `AuthorizationHeading` del registro con los valores fijados (en la
  ronda 3 el encabezado se comprobaba solo por `HeadingMatches`), y añade la regla de la autorización pendiente.
- **`make_manifest.py`:** la cabecera registra `AuthorizationPins`, `AuthorizationState` y `OwnerGateTested` (SHA-256 de `owner_gate.py` en la
  autoprueba, en la mutación y actual) para que el manifiesto vuelto a generar tras fijar la autorización lo deje visible.
- **`finish_kit.py`:** con la autorización pendiente, la prueba en seco del preparador exige `BlockingChecks` = [11-OwnerAndAuthorizationGate] con el
  único problema «authorization pending: …» (en la ronda 3, `BlockingChecks` = []).
- **`prepare_clone.py`:** la comprobación «§84 nombra el blob» pasa a «§85 es la última sección del commit del clon» (§85 es anterior al blob de la
  ronda 4 y no puede nombrarlo; la autorización que lo nombre será posterior). En la ronda 4 el commit de las guardas y el del clon coinciden.
- **`verify_clone.py`:** commits presentes de la ronda 3 añadidos (publicación a128efef y custodia de su revisión f402c4e7).
- **`launch.py`:** dos comentarios de la salida de `--dry-run` y la docstring; ningún cambio de lógica.
- **Autoprueba:** los casos de auditoría usan una autorización SINTÉTICA (§86 añadida a las decisiones del clon; valores de
  `pin_authorization.values`) puesta en el objeto módulo `owner_gate` del auditor mientras el kit la tiene pendiente (nunca en el archivo); L00 es
  consciente del estado (pendiente: solo el motivo pendiente; fijada: Ok con la sección fijada); casos nuevos O00c (estado del módulo tal como está
  en el kit), O30 (`pin_authorization`), 117-118f (esquema), 125j-125k (referencias anidadas) y 151m-151p (registro de la compuerta 11).
- **Higiene de la preparación (sin efecto en el kit):** al empezar, el preparador ejecutó una vez `git status --short` (solo lectura) sobre el
  checkout principal y escribió cuatro listados temporales de SHA-256 en `/tmp` de Git Bash, que borró en seguida; nada más se escribió fuera de
  esta carpeta, del clon y del run (salvo el temporal propio de `--record-probe`, que el lanzador borra).

## Supuestos no medidos y puntos abiertos

1. **La autorización no existe todavía.** El kit no puede lanzarse hasta fijarla; cualquier sección posterior a §85 que no sea la autorización (o
   una segunda) también hay que revisarla antes de fijar (la compuerta, fijada, bloquea ante una sección posterior a la fijada; mientras está
   pendiente, `SectionsAfterBase` las lista como información).
2. **Duración frente al tope de 3 600 s.** La ronda 3 duró 1 119 s (54 turnos, 43 lecturas). La ronda 4 es más acotada, pero pide contrastar la
   regla 6 (b) con la tabla de derivación, las sondas de `r4/r1probes/` (copias legibles grandes, p. ej. `p7-derive.json`) y la implementación
   independiente: del orden de 60-120 llamadas, ≈ 1 200-2 800 s. El tope no se ha cambiado (orden del kit, punto 6).
3. **El hijo sustituto no es `claude.exe`:** la sonda demuestra el código común de lanzamiento y registro en este entorno, no el tamaño ni la
   duración de la salida real, ni el refresco OAuth. La sonda se repite dentro del lanzamiento real.
4. **Entorno heredado:** el lanzador no pone ni quita variables; `claude.exe` hereda el entorno de quien lance, incluidas las CLAUDE* y ANTHROPIC*
   que `preflight.json` registra por su nombre. Lo decide la sesión principal antes de lanzar; el kit no lo cambia.
5. **Residuo no corregido (heredado):** el `clone_git` del auditor usa `text=True` sin codificación (regla del clon, byte a byte); sus salidas sobre
   un clon limpio son ASCII. `owner_gate.py` pasa `GIT_OPTIONAL_LOCKS=0` a sus propias lecturas de git; no afecta al entorno de `claude.exe`.
6. **Líneas de más de 2 000 caracteres sin copia legible** (los seis sobres de `bmin/e2e/out/`, V14, A-3, decisiones y `prompt.md`, que no es
   premisa): como en las rondas anteriores; el prompt las lista y prohíbe citarlas.
7. **Compactación automática del contexto:** el prompt avisa; contaría como un segundo mensaje de usuario.
8. **Que el esquema (11 409 caracteres compactos) se comporte con claude-cli** como los anteriores (mismas palabras clave).
9. **Autoprueba y mutación sobre la lógica pendiente:** se ejecutaron con `owner_gate.py` en el estado entregado (autorización pendiente; los casos de
   auditoría con la autorización sintética). Fijar la autorización cambia solo tres constantes; si se quiere evidencia sobre el archivo fijado, la
   autoprueba se repite entera (L00 exige entonces el estado FIJADO).

## Archivos y privacidad

`kit/` es el kit congelado (manifiesto: `kit/kit-manifest.json`). Junto a este README: `record-probe.json` (salida de `python -B kit/launch.py
--record-probe`) y `launch-dry-run.json` (salida de `python -B kit/launch.py --dry-run`), convertidas a UTF-8 desde la codificación de la tubería
(bytes crudos en `prep/phase/`). `prep/` es la preparación y no se custodia como kit: `subst.py`, `copy_premises.py` y `adapt_*.py` (derivan por
sustituciones exactas), `gen_schema.py`, `make_diffs.py`, `file_table.py`, `assemble_readme.py` y `readme.md` (este README), `ids.txt`, `diffs/` (cada
archivo frente al de la ronda 3), `phase/` (salidas de cada paso) y `dev/` (ayudas de desarrollo: ejecución parcial de la autoprueba).

**Privacidad:** `prep/file_table.py` comprueba que ningún archivo de `kit/`, de `prep/` ni de esta carpeta contiene el nombre de usuario de Windows,
largo u 8.3, ni una dirección de correo (salvo las de prueba del dominio reservado example.invalid): sin coincidencias. Las rutas del perfil se
escriben como %USERPROFILE%, %LOCALAPPDATA%, %APPDATA% o %TEMP%; del entorno solo constan nombres de variables y los valores de las PYTHON*.

```text
SHA-256                                                           bytes  archivo
51b285371fb0a4ecfe0751ffd6061fb5981384e262c881d4b639843ac4986b9d  135665  kit/a6-r3.md                         (= blob e62f27b8, ronda 3)
4fc6115e2551b68efd46a20504764f2313f894eaddbb2e59b16dcdf72c1ecbb7    6070  kit/c3_analysis.py                   (= ronda 3, 4fc6115e)
16d9290c68fc704797d8798e1d8d83bba4e2bc5163f664e42232a5f26fb415b2   81482  kit/clone-verification.json
8c70a195fd0b1b7e34f3b9095b3669df34b84a173c3e117fedccd117acdefc42  392128  kit/closure.json
74928bb9d4390499179305f6ee2257b3dd38cfda0a8110cce26a65bf9b5902b1    8275  kit/finish_kit.py
6c8807beb692f8604ce2d2381aac676fac5664b9862b1cd2b141d798e36b6553    7166  kit/kit-manifest.json
c8d805f6d934cc0b8d18c7f7941382a1da387a91f5094e94ac97f9e983b92aa5    2267  kit/kit-params.json
2802716c22483aebfb7c666b65d0ecd66c50e77de82117278a17b1e5605d00a2   46012  kit/launch.py                        (de launch.py de la ronda 3)
74c88d269de6fb0d1a8785c4eb9e71f5d16a7472b86a2db60ee3080f973e5ced   87243  kit/make_closure.py
f160f9119ab57f6ca0fca672945b30a7245622ab92fcb14005a6237da323ddd8    8666  kit/make_manifest.py
4eaa1c8e33d0c706b3965b62e3fe544981f1e8fcd15352564d2613254a1a1305    7699  kit/mutation-post-review.py
424c983eb68eaaa7052c491feafe98e5045df0d93a0fcc752dcab4cae926cf73   54267  kit/mutation-result.json
d74e2aa1ce5eacc7dd6323cf3f3acec37fd6eb25f905ca4f2479dbca0cde5232    2805  kit/order.txt                        (= day3\s85_body.txt, cuerpo §85)
d442608a77b78d96546c042cc396cb1b553c69673a5368aa526953f63060b049   17040  kit/owner_gate.py                    (compuerta 11, pendiente)
a8e4070751ae604010149c8678aa9320766c533230bf92de3aa2e88ea6e571cc    5588  kit/pin_authorization.py             (nuevo)
4f46caa66c5fcd299000e2d2f0c42ebf3a13c4c9fd027f0a57b314c2a8fc485f   70728  kit/post-review.py                   (auditor v5.4-a6r4)
0d23faf2f468e92cda65f634b610c7cc5a5c8b6edb0d19162fbcd603cc30cf0e    9251  kit/prepare_clone.py
a8dc5773499ae56cf06718f4aed1cf46723eb66e6f1511166f6bd5e3e33ed0b9  105320  kit/prompt.md
da40d1050c865e7032afc2a3502df32b4e80f0bb345d61ceb1cfb9605544a931   36763  kit/prompt.template.md
7b51116e5e4876c1416066994e282ad077661547ee5fb1dcb0fbcbd59e413cce    4925  kit/read_fidelity.py                 (= ronda 3, 7b51116e)
f3dad55b008af984fb1ca6e57f32a311fd53d42d897fba1162bd8849f5a754ed   17165  kit/result.schema.json               (rackcad-a6r4-architect-review/0)
a86eeac7fb857ec8066563e6284a8c2c572450cf8b1923b41c6e25944aab9529  128908  kit/selftest-post-review.py
eac70103a87bb89b4cd1be0440ff2e18ace474214d26ac4ce2ca20e6e4172953  152585  kit/selftest-result.json
b0bf2d95e50f03dc29ffa2e7f3c8ebf0f1ddf1997144373b1d62ad543f54a4e1    1602  kit/transport-gate.json
e326b5b92da5ff2b53d05a7d64f90dbb09bf098ab4eeda5a92597d6fc36e6fbf   22485  kit/transport_gate.py                (= ronda 3, e326b5b9)
bc028e3fd13a555087cf8fbf5fec1b43ecff8573ef49a99f75058b1fd2f655d4   19413  kit/verify_clone.py
0b8a68d987709ccd1fa2d5c6c91e9df1c6620805793bd845fcc1298bc6a7b7d4   13521  prep/adapt_launch.py
94ca2f3167fb0eda13d6063e4b421c4aba516f3d6c2d09f6b35113ece21bda59   35352  prep/adapt_misc.py
023e6d4044c0837eb14e716c6b72dff854aa03b8664ce81d5decfb91942908c8   19225  prep/adapt_owner_gate.py
6543edf710c0b8867a3776b11f633414fe8a4e4d0c20adeaeb3e34b993adb32c   32801  prep/adapt_post_review.py
433f7848de1a917b3d9e7c38615d8e0fdc33473c96e8f808a59ab2bb5b1a0c0a    1186  prep/assemble_readme.py
22ac27cc402bf17c575b66af77a36cdd1b33008bf4b59e51961a17aec98f46c2    3431  prep/copy_premises.py
5a8f9a7ed1072c6fbd84469f533c5ef594a3edf94fc5ac44212b626c5b2adcc0    2967  prep/file_table.py
fa5e34cf4280dbf17b4da69093e9d542a5f2325f6e011c5448f6c3f42a9be8b0    9205  prep/gen_schema.py
672e239c0d1d94256d87320e004ce928e1153c30eac6a2250c2610b7d3f064aa     204  prep/ids.txt
0bf715461f6455b1325c316749c57faf133c53129d9e06dac2f5403708cefaa3    3139  prep/make_diffs.py
942acb3178cfb4a08db33bbff32f6db445cddaedf2cacdd5405ad8948ba534e3   40084  prep/readme.md
c5b6d7d43f4d679e6beacf8c7e20dd617e0ae12c8c3abc16aefbac93dd885152    2057  prep/subst.py
d6b775e32513773f92da5cf338b29017e7e644a836cf6f2d6c6fa08a739dc03a    7008  prep/dev/fill_readme.py
fbf02a69eede24b908fd142b88108f86298e3f6ab454fb84677301053ba219e2    1505  prep/dev/pin-clone85.out
ac84ea96ddc2355f05a219cfc71dee839b4e6f5d2d1f2f8f37c13eb2e58f8086    1437  prep/dev/pin-synth86.out
b862e2c8f84dc0af957885040010385727f82fce737a9ac340100e2816d3e26b    8105  prep/dev/quick-a1.out
639df6d74b9bbf7493ed37f30533e1af0d2c2f558793dc09191b3d6d3bb5136b     611  prep/dev/quick-a2.out
e558df466bbf9e5dc21c274afa23e06e76ca18f751941b766e02b898cece9465    2046  prep/dev/quick_cases.py
dee608fe7f9015cf85c335d99526e88b78827226b12458544e1d7615f9d834c0    2147  prep/dev/quick_owner_gate.py
02c7547cbf27f5d9577fca3cb94a6087d5768423f5f4621564bb50dc9d320bc3    5563  prep/dev/r3-cites.txt
29f3cebf7cc9b4f8f0af5af855f1ea529a3394374f49160a377599b087b84199    2115  prep/dev/scan_r3_cites.py
2c4bcf8e7b50a233310e58eac469a5966ca1077c1b73d0d48a4365d8f9d768e8    1160  prep/dev/schema_probe.py
ad4072b9cbd7362223419b6cf481df4ce41f250f4e1caca7618fb7ff387a37f0    1702  prep/dev/schema_probe2.py
f10cc227b19d02abc034c1a1fd639c146a55afa08b77eaa286209346ec16ddd5    2435  prep/dev/schema_probe3.py
8b6b283909062e987a74a9cf9c389349adac5258343a743da939941b7b189e13    7186  prep/diffs/finish_kit.diff
ee75dbcac6adb478d049e3cd8b87908827ac79e598305f43b242ed930989933e   13740  prep/diffs/gen_schema.diff
6989657c955a171d183cdc86e26d481579a7bf50b4f977f20c09fa438b6b678a   12965  prep/diffs/launch.diff
1639017e73c610032e16a9a318f7d86320c469725d9e2d84230dc588c16cab45   69441  prep/diffs/make_closure.diff
1c5f0037d80d9db5829b537ee965509c1b8d1b5f8d7dd1ead735dd066d3f30d1    7104  prep/diffs/make_manifest.diff
126607c2e043fe2fdb038b8fcb592b64307daeff8e123bab48bb2d58e30b348e    1984  prep/diffs/mutation-post-review.diff
9d08f2ba4bb8ea85c6c071df774f3ea0c7b9be134a01f49c41dc69815361020a   18714  prep/diffs/owner_gate.diff
c8c7df81e5bd8db7b7099f94809161d2f1d5ea3ad20a91c56979530e7fa23164   36638  prep/diffs/post-review.diff
73cb8cd28320632a789672a0aec29aa62351fbeabd90d25d8b8a01e3e45a131c   10882  prep/diffs/prepare_clone.diff
4557dad57d11352062990bd67e4a743d1dff44b73bdadedac995095e27fc8c00   59733  prep/diffs/prompt.template.diff
daa00fa81d4d288e4bb3c25be0afc9031a6adcab52df87fc6b5e742825539181   18649  prep/diffs/result.schema.diff
2074ad1ec57e113580094d07b08aa624d38934f00c44832559777c6dbba75552  103964  prep/diffs/selftest-post-review.diff
bdf584c16b6ace748d4cf2fc01f88e7d751fcc19e4bd5286eba0e620b538cf1f   12901  prep/diffs/verify_clone.diff
83a7aad2c2e1b3e57ab5df859dffec49b4b342531b858c91246035365703c7f6     396  prep/phase/adapt-launch.out
a310938a76eff1ea9be8b781bc129aa2ddc9148bb9e8fe226a72082e16ff1c18    1740  prep/phase/adapt-misc.out
37d945c8e015fcd42dbe1b24c624a6d6318fedcda72a6c39ac26eb263899f66f     410  prep/phase/adapt-owner-gate.out
c9ab766443afdeab085cd5113fef315c87eebed5a399333cd58adf482eb8ccee     449  prep/phase/adapt-post-review.out
4c5a78e9d7a2ede18d30b4602db71c36c00dec9e207a4c3fa4f14b5fe5b39197     631  prep/phase/make-diffs.out
4ac0dcdfc4867ec61d7784c8e8c5e90434b29695469d24445a204354e5911e65     202  prep/phase/step-schema.out
bb11be0a365e2ecbacb5c83d3b2df36b00736e9dea96a021074b6bfc878d3a7c     919  prep/phase/step0-copy.out
a8dfb05b4c91edbeaa216962534e9e298602a2a623aeea295e885e7da6b7cbb4     777  prep/phase/step1.out
e3b0c44298fc1c149afbf4c8996fb92427ae41e4649b934ca495991b7852b855       0  prep/phase/step10.err
8489e75daf580080dfe4068d8762ff93be86710619f8b6c4bc4438ff2d0b2d76    2917  prep/phase/step10.raw
daef5289130ce2ae29560263391bd1335fe1583f87db7eb41c05f37e8dbbe0a9      42  prep/phase/step10.times
e3b0c44298fc1c149afbf4c8996fb92427ae41e4649b934ca495991b7852b855       0  prep/phase/step11.err
6a0251e8386c5012f9c36a4ffaa02deb877fccd300f0ccfdde2181039033c0f1    7909  prep/phase/step11.raw
60fa1d35491251935ad6345c0be9790d3f09582efa878492f1e1e5edd5845d7c      42  prep/phase/step11.times
2fbc19d6324482e9ae2db1dd42b7ba01be64f93f3c4d66de559b61698d0f4023     135  prep/phase/step2.out
3efc7c15d909adffc36463d57df9a0aa98ceb0029dca7d43b0c8dc20269277d1      30  prep/phase/step3.out
c852197f596eca9c59b1e51578e6eb019f67499ccfc70e2239dc0e29e49f5958    8958  prep/phase/step4.out
4fea9b06858800f975c5818078f679167bc0e211adfc22f3aff5e01f4cf4ec2a     309  prep/phase/step5.out
7a1040ec08de12df9ad237439ce1fe4ac28bc24865395f67c6618da81a57f3f1     413  prep/phase/step6.raw
9d53fb269be56a227261a9bd77bf4a2dbe1c7a2884c9ab7810cf961f12c68cf5      42  prep/phase/step6.times
355cb2504a4ef401670724c286ba0780257c853cb2e34a3fae86d67a15b0092d   20523  prep/phase/step7.raw
cdc30d6d4ea6b1cde588c75cd91ee7a7ad0bc8d37d408a1212e879feb72ce34f      42  prep/phase/step7.times
9960e82ecf467cd85687117902fc972d071b5b72f005260f98901e9efee9fe5e   14705  prep/phase/step8.raw
717d36bd90ea886294dc9d25c54c9572fffdc0850a45e26dc6188d59961eaa69      42  prep/phase/step8.times
b67d4a054d795645c229dee498fa077e257b54dc707865591d3d2ed2d3911c8d     145  prep/phase/step9.out
eba7cf4aedfc4ae90749b1059f81124879656f56a875241d61c241e54fdfd8ec    7912  launch-dry-run.json
cfe6ba3f3a5806499b66e39374fd15b908322b37af997bf62d649aba39553286    2919  record-probe.json
```
