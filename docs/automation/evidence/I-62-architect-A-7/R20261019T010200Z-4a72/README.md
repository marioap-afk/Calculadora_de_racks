# I-62 — Kit de la revisión formal única de la enmienda A-7 por un Architect independiente con `claude-cli`, acotada a A7-1 (R20261019T010200Z-4a72)

```text
LogicalReviewRequestId: L20261019T010200Z-4a72   InvocationId: I20261019T010200Z-4a72   AttemptSeq: 1   RunId: R20261019T010200Z-4a72
SessionId:      4945013a-d80e-43dc-bc95-d34dff85fcfe (uuid4, generado una vez el 2026-10-19T01:02:00Z con los demás ids; prep/ids.txt)
Orden y autorización: decisiones §93 (línea 1825 en 5158e4bc; cuerpo pegado 4 057 bytes, SHA-256 a5c37f714364bc704c5f38218ae2b1a9209052c47834dcc5349684eb141be41b
                = kit/order.txt = run/order.txt). §93 autoriza UNA revisión formal de A-7 (punto 1) y ordena el kit (puntos 2 y 6); es a la vez la orden
                del kit y la autorización de lanzamiento, y es la última sección de decisiones en la HEAD 5158e4bc del worktree
Autorización:   FIJADA §93 en kit/owner_gate.py con pin_authorization.py (Ready: número posterior a §92, una sola sección, blob completo como token,
                ninguna sección posterior): AUTH_TEXT_SHA 4f66b73832a045f86a5998922522d13267749522c491d183dd63fa7f7f4ba374 (prep/pin93/)
Consumo:        cubierto por CLAUDE-CLI-I62 = A (revisiones de ARCHITECT de I-62, solo lectura), la misma base que las rondas de A-6
Objeto:         docs/initiatives/I-62-A-7.md, ronda 1, PROPUESTA, blob 6c29747d61c5f689a195095ad3712e6d9dba62aa (27 472 bytes, 279 líneas; SHA-256
                1ab7315ad0a2592b4fef50de28c54dc5904123771dd59d24c9768e6a309b5e6f), publicado en 6fee8fd6a96ce4ae45d116e34affee4ed1b1fd40 (padre
                bb439090, sin A-7, con decisiones §92); guardas `run` PASS custodiadas en 5e3141a2f10bbfc5e4fe1dfe5823382da841efd1
Commit del clon: 5158e4bc17ad48160d8573b7f48f3923e3197f06 (HEAD de la rama; contiene la publicación, la custodia de las guardas y §93, su última sección)
Clon / run:     D:\r62-arch-a7 (HEAD separado en 5158e4bc, sin ramas ni remoto; clonado desde el directorio común de git) y D:\r62-arch-a7-run
                (order.txt = cuerpo §93, prompt.md = kit/prompt.md)
Transporte:     el de A-6 ronda 4 tras §87 sin cambio: claude-cli 2.1.299, %APPDATA%\Claude\claude-code\2.1.299\f0584be0fbaf\claude.exe, SHA-256
                295b49ec092f31a7de9c6ff286d9d8f120a4f029b9ad9a5ac0b12c35cbf4df2b; claude-opus-5-5, effort xhigh; Read, Grep y Glob (+ StructuredOutput);
                --safe-mode, sin MCP; --add-dir del run; prompt por stdin; resultado por --json-schema; tope de 3 600 s; espera de 120 s tras `auth status`
Orden:          EXACTAMENTE `python -B kit/launch.py`, desde la carpeta que contiene kit/ (esta), sin flags ni variables añadidos
Compuerta D-01: MEASURED sobre kit/claude-cli-characterization.json (R20261016T185242Z-char, SHA-256 4ba6ba58…, blob 19b8a27e en 1c960c0a y en el clon),
                byte a byte la copia del kit de A-6 ronda 4; transport_gate.py byte a byte
Compuerta 11:   BLOQUEANTE: línea del Owner de §78; CLAUDE-CLI-I62 = A y el mismo binario; A-7 en la HEAD = 6c29747d y A-6 = 4bc919dd (acordada);
                decisiones solo por añadido desde el clon; la HEAD desciende del clon; §93 fijada (encabezado, blob completo, SHA-256 del texto, una
                sola sección) y ninguna sección posterior
Estado:         kit preparado, probado y CONGELADO con §93 fijada (kit/kit-manifest.json, SHA-256 444d9ba99865dd1a6690d6682c84f2ea077438e83f3629a5625a5f238b92c0da); sonda de registro en Ok; prueba en
                seco con BlockingChecks = [] (4 y 5 no se ejecutan en seco). NO lanzado: ningún claude.exe ni CLI de modelos ejecutado; ni red
                ni credenciales
```

## Qué pide §93 y dónde lo cubre

| Exigencia (decisiones §93) | Dónde |
|---|---|
| Punto 1: UNA revisión de A-7; objeto `docs/initiatives/I-62-A-7.md`, commit 6fee8fd6, blob completo con prefijo 6c29747d identificado y verificado; alcance exclusivo A7-1 y sus consecuencias | blob 6c29747d61c5f689… verificado en la publicación, en la custodia de las guardas, en el clon y en la HEAD (`prepare_clone.py`, `make_closure.py` identity_checks, `verify_clone.py`, compuerta 11); prompt y esquema acotados a A7-1 |
| Punto 2: reutilizar transporte, auditor y compuertas con identidad demostrable; kit acotado; validación incremental (§88); sin otra jornada ni cientos de mutantes sobre componentes idénticos; cláusula exacta antes de una batería completa | derivación por sustituciones y regiones (prep/adapt_*.py, diffs en prep/diffs/), reutilización byte a byte de transport_gate.py, c3_analysis.py, read_fidelity.py, pin_authorization.py y la caracterización; matriz incremental (prep/validation-matrix.md); mutación DIRIGIDA a los sitios cambiados; la batería completa no se ejecutó (abajo, «Batería completa») |
| Punto 3: temas obligatorios, materialidad y OWNER-RESERVED | `TopicVerdicts` (once temas), `Materiality` (M-01..M-08, A7Declared + AGREE/DISAGREE), `OwnerReservedConsequence` en el esquema rackcad-a7-architect-review/0; prompt, «Qué decidir» |
| Punto 4: D-1/`contract` como impacto relacionado, sin incluirlo en A-7, sin el punto 7 del borrador | `RelatedImpacts.ContractExclusionD1` (Status, RevealsA7Defect, Concerns); el prompt dice que el recálculo del punto 7 no se incorpora; s92/ y s91/d1/ en el cierre |
| Punto 5: FC-01, FC-04, FC-10, FC-12, FC-13, FC-16, FC-24 y FC-18; criterios de PASS, casos válidos de C-24, coincidencia con la implementación | `RelatedImpacts.FailClosedProposals` (una por propuesta, tres banderas y RevealsA7Defect); s92/readings/, s91/r12/ y m7/readings/readings.md en el cierre |
| Punto 6: manifiesto de insumos y blobs; identidad del transporte; independencia; autoprueba y negativos afectados; record probe; dry run con cero bloqueos | `kit/closure.json` (190 archivos por blob) y `kit/kit-manifest.json`; compuertas 3 y 10 y verify_clone; clon aislado, sesión nueva, cierre por archivo; `kit/selftest-result.json`, `kit/mutation-targeted-result.json`, `prep/neg-verify-result.json`; `record-probe.json`; `launch-dry-run.json` |
| Punto 7: auditor acreditado para el kit; AGREED exige cero REQUIRED y ACCREDITED | `kit/post-review.py` v5.5-a7 (reglas genéricas de v5.4-a6r4 byte a byte; coherencia del esquema nuevo) |
| Punto 8: sin Codex, sin medición, sin B1/FX-U2, sin I-65, sin alterar A-1..A-6 | nada de eso se ejecutó; compuerta 11 exige A-6 = 4bc919dd en la HEAD; el cierre comprueba los blobs de A-1..A-6 |

## Independencia del preparador

Quien preparó este kit es un subagente de la sesión principal (la sesión que preparó A-7, su preflight y las disposiciones de §91 y §92). Leyó §88-§93,
el kit acreditado de A-6 ronda 4 entero (README, componentes y scripts de preparación), A-7 entera, el preflight de A-7 (`preflight.md`, partes de
`a7-guards.py` y de `run_a7.py`, los resultados de las guardas y `summary.json`) y, de los borradores de §92, la estructura de
`contract/disposition-draft.md`, de `readings/proposals.md` y `coordinator-notes.md` (para orientar el prompt por secciones). No evaluó A-7 ni los
borradores: el prompt no contiene veredicto esperado ni valoraciones; las definiciones de los estados del esquema son de forma. Leyó, para preparar
`order.txt`, el cuerpo pegado de §93 que la sesión principal ya había extraído (`s93_body.txt`, SHA-256 a5c37f71… = el que declara §93); no leyó
transcripciones, credenciales ni archivos de autenticación. La evidencia de I-62 (`I-62-evidence.md`) está en el cierre, presentada como evidencia y no
como autoridad (abajo, «Cierre»). El kit no ejecuta el constructor ni el validador del cierre de A7-1, el instrumento, el aceptante ni el kit de la
medición; solo reproduce las guardas de A-7.

## Contrato y auditor

- **Cierre** (`kit/make_closure.py` → `kit/closure.json`, en el commit del clon; cada archivo por su blob; rangos calculados desde encabezados, filas y
  líneas citadas con su primera línea comprobada): 190 archivos, 5 053 543 bytes, 77 134 líneas.
  - A-7 entera (objeto), con su cabecera, secciones, filas L1-L15 de §2, el texto de A7-1, sus siete puntos y las categorías (a)-(d), §3.3, §3.5
    (residuo declarado), §5 (M-01..M-08), §7 y §8 (Q-A7-01..06).
  - `docs/automation/evidence/I-62-A7/` entero (35 archivos; MANIFEST.sha256 comprobado: lista todo salvo el resultado `run`, custodiado después).
  - Las fuentes literales de A-7 §2 con sus rangos (V14 §20.3.1 L1079, L1098-L1099; B.9 L2231, L2233, L2250-L2251; AP 16.3 L490; 16.13 L821,
    L823-L826, L902-L908; 16.24 L1264, L1276, L1279-L1280; RAE §15.2 L565, §10 L219-L222; A-6 L532-L535, L539-L541, L600-L617, L781-L783,
    L789-L794, L841-L843, L851-L871; y el fin de A6-8, A-6 L815-L816), parseadas mecánicamente de A-7 §2 (`A7Literals`).
  - A-5 entera (A5-1, A5-2); A-6 acordada entera (reglas de cada delta, A6-3 regla 6 (b), tabla de §3.12, §5, líneas del Owner de §7); A-1..A-4 y el
    registro del Freeze (cabecera; A-7 cita sus blobs y el cierre comprueba que la cabecera de A-7 los cita).
  - Decisiones (archivo entero) con rangos de §62 (U-19), §73, §78 (línea del Owner), §90-§93 (puntos, cuerpos pegados, objeto de §93, acuerdo de
    A-6, registro de lecturas).
  - `m7/s91/` entero (determinación de I6 que A-7 cita; D-1 y r12, que §93, puntos 4 y 5, remiten; MANIFEST comprobado), `m7/s92/` entero
    (borradores de §92; MANIFEST comprobado), del banco README, designaciones y `p1-build.json` (cita del preflight), y `m7/readings/readings.md`
    (lecturas L-01..L-24 a las que remiten las propuestas FC).
  - Lo que cita el preflight de A-7 de otras evidencias: `r4/r1probes/p7-derive.json`, `p9-n4.json`, `p10-nc1.json` con sus copias legibles
    (MANIFEST de `r4/readable/` comprobado entero) y el kit `a6m7-measure` entero (A-7 §10; `frozen/E.json`, `reconstruct_runs_m7.py`, líneas del Owner).
  - El paquete B-min entero (árbol 3caa0dfa: instrumento 9a829b0e y aceptante 434ef1fc).
  - Base normativa: LIFECYCLE (§3 M-01..M-08 y OWNER-RESERVED, §5, §6), ADR-0046 y ADR-0048 (M-08), controller-contracts.md (L23, L44-L45), el mapa de
    cláusulas y su esquema (T04), los esquemas rackcad-input-closure/v1 y rackcad-role-invocation/v1 (M-02, M-05); V14 (§18, §20.3, B.9, Anexo C C-24,
    D.3, D.6), AUTOMATION_PLAN (16.3-16.7, 16.9, 16.13, 16.14, 16.22, 16.24) y el procedimiento de agent-execution (§5, §8, §10, §14, §15).
  - **`I-62-evidence.md`: incluido**, con rangos de orientación §180-§187. Motivo: A-7 cita la evidencia §181 (cabecera, «Origin») y el revisor debe
    poder leer toda fuente que necesite citar (el principio que los kits de A-6 aplicaron desde decisiones §79, punto 5). Riesgo de anclaje
    declarado: es el registro de la sesión que preparó A-7 (§186 resume A-7 y su preflight); el prompt la presenta como evidencia de hechos
    contrastable, nunca como autoridad, y extiende la advertencia a `preflight.md`, `i6-determination.md`, los borradores de s92/ y las notas.
  - **Excluidos:** `docs/automation/evidence/I-62-C1C3/` y todo material de I-65; el resto de `I-62-A6/` (r1-r4 salvo lo citado; de m7/,
    `bench/inputs/`, los demás resultados del banco, `identity/`, `owner-lines/`, las sondas de readings/); las revisiones anteriores; el repositorio
    de la entrega y el bundle binario `MAP-INVALID/candidate-b.bundle`; WORKFLOW, FOUNDATIONS, `state/I-62.yml`; transcripciones, memoria y fixture.
  - Comprobaciones de identidad (`identity_checks`): el padre de la publicación no tiene A-7; A-7 = 6c29747d en la publicación, la custodia y el clon;
    A-6 = 4bc919dd en los cuatro commits; I-62-A7/ sin cambio tras la publicación salvo el resultado `run`; árboles de B-min, agent-execution, s91,
    s92, bench, readings y del kit a6m7-measure iguales en el clon y en el commit que los custodió; decisiones por añadido desde ea575012 (§92,
    bb439090) y §93 última sección, una vez, con el blob completo como token; resultado `run` = Mode run, Base = padre, Head = 6fee8fd6, A7Blob,
    PASS; autoprueba de las guardas PASS 44/44 sobre el objeto; `summary.json` 160/160 sobre el objeto; las líneas del Owner una vez en A-6 y en
    decisiones; A-7 §5 = M-01 no, M-02..M-05 sí, M-06..M-08 no.
- **Herramientas y lectura:** las de las rondas de A-6, sin cambio (Read, Grep y Glob, y StructuredOutput; archivos de más de 60 000 bytes por
  tramos de 300 líneas; Grep solo sobre archivos; Glob solo dentro del cierre; toda denegación es violación; toda referencia solo a líneas leídas con
  Read en ese mismo archivo). **Lectura directa obligatoria: V14 B.9 L2231-L2233** (antes, V14 L2333-L2334, que decisiones §80, punto 6, pidió para
  A-6): §93, punto 3, pide «A7-1 frente a V14 B.9», y A-7 cita B.9 L2231 y L2233 (L3, L4); el auditor la exige por el mismo código, con el dato del cierre.
- **Prompt** (`kit/prompt.template.md` → `kit/prompt.md`, 77 659 bytes, 573 líneas, SHA-256 `ba280474…`): neutral (sin veredicto esperado); explica
  el objeto, la condición de lanzamiento (§93 fijada), los temas obligatorios de §93, punto 3, los impactos relacionados de los puntos 4 y 5 como
  campos separados que no condicionan el veredicto salvo que revelen un defecto de A-7, el tratamiento de las precondiciones de sesión real, el Paso 0
  (lectura directa de V14 B.9; order.txt entero; §93 bytes, SHA-256, objeto y puntos; §90 acuerdo de A-6; resultado `run`; `summary.json`), el
  cierre, la lista de lectura L1-L15 y las reglas de lectura y de referencias.
- **Esquema** (`kit/result.schema.json`, `rackcad-a7-architect-review/0`; 9 809 caracteres ASCII en forma compacta; `prep/gen_schema.py`): las mismas
  palabras clave medidas. Campos: identidad; Step0 (siete booleanas: V14 B.9 directa, orden entera, §93 bytes y SHA-256, §93 objeto y autorización,
  §90 acuerdo de A-6, resultado `run`, resumen del preflight); DirectReads; Reads; hallazgos `A62-A7-R<n>` / `A62-A7-O<n>` con la forma del
  repositorio; `TopicVerdicts` (once temas, CONFORMS | DOES_NOT_CONFORM); `Materiality` (Triggers M-01..M-08 con A7Declared yes | no y Assessment
  AGREE | DISAGREE); `OwnerReservedConsequence` (OwnerDecisionRequired y UnidentifiedConsequences); `RelatedImpacts` (`ContractExclusionD1`: Status
  NO_CONCERN | CONCERNS_FOR_COORDINATOR | NOT_DETERMINED, RevealsA7Defect, Concerns; `FailClosedProposals`: ChangesPassCriteria,
  ExcludesValidC24Cases, ReliesOnImplementationCoincidence, RevealsA7Defect, Status; Note); `QuestionAnswers` (Q-A7-01..06); IfAgreed (ocho
  booleanas); KnownLimitations; RecommendedNextAction. Veredictos: AGREED, CHANGES REQUIRED, REJECTED, BLOCKED — OWNER DECISION, NOT_ACCREDITED.
- **Auditor v5.5-a7** (`kit/post-review.py`; `prep/adapt_post_review_head.py` y `prep/adapt_post_review_tail.py` sobre la copia verificada de
  v5.4-a6r4; diff `prep/diffs/post-review.py.diff`): las reglas de lanzamiento, compuerta D-01, stream-json, transcripción, herramientas, lectura,
  fidelidad, referencias, lectura directa (dato del cierre), custodia del run, clon, `run.json`, FrozenCommandGuard, RecordProbe, Environment,
  `Categories` y `validate_schema` (Test-Json Y `schema_errors`) quedan byte a byte (85 sitios de regla idénticos a líneas de v5.4-a6r4).
  Cambian: el registro de la compuerta 11 (ObjectBlob, ObjectBlobAtHeadMatches, A6AgreedBlobAtHeadMatches; §92/§93) y la coherencia del esquema
  nuevo (`coherence`, `collect_refs` con los elementos anidados de Materiality y RelatedImpacts): unicidad y cobertura de los cuatro arreglos;
  DOES_NOT_CONFORM o DISAGREE ⇒ un REQUIRED vinculado y su opuesto sin REQUIRED; A7Declared = A-7 §5 (del cierre); OwnerDecisionRequired true ⇔
  consecuencias no vacías; RevealsA7Defect true ⇒ un REQUIRED vinculado, false ⇒ ninguno; Status de D1 y de cada propuesta coherente con Concerns y
  con las tres banderas; NOT_SATISFIED ⇒ un hallazgo; con veredicto, ClauseRefs en todo elemento, ningún NOT_DETERMINED ni null en D1 ni en
  OwnerDecisionRequired, todo REQUIRED citado; AGREED ⇒ cero REQUIRED, IfAgreed en true, los once temas CONFORMS, los ocho disparadores AGREE y
  RevealsA7Defect false en todo (las preocupaciones que no revelan un defecto no bloquean); CHANGES REQUIRED o REJECTED ⇒ algún REQUIRED.
- **Compuerta de transporte** (`kit/transport_gate.py`), `kit/c3_analysis.py`, `kit/read_fidelity.py` y `kit/pin_authorization.py`: byte a byte
  (`pin_authorization.py` conserva el texto histórico de su docstring sobre §85 y la ronda 4; su lógica usa `owner_gate.BASE_SECTION`, ahora 92).
- **Compuerta 11** (`kit/owner_gate.py`; diff `prep/diffs/owner_gate.py.diff`): las funciones de sección, token, autorización y secciones posteriores,
  byte a byte; cambian `evaluate` (objeto A-7 con el blob fijado OBJECT_BLOB, A-6 acordada en la HEAD, base §92, mensajes) y `check` (lee también
  A-6 en la HEAD). **§93 fijada** con `pin_authorization.py` sobre el texto de decisiones de la HEAD 5158e4bc (Ready, línea 1825, blob como token, sin
  secciones posteriores); solo cambian las tres líneas AUTH_* (`prep/pin93/owner_gate.pin.diff`).
- **Lanzador** (`kit/launch.py`, diff `prep/diffs/launch.py.diff`): G, R, P, D, las comprobaciones 1-11, Outcome, entorno, binario, modelo, effort, tope y
  espera, byte a byte; cambian el clon y el run, el session id, el patrón de proyecto (`D--r62-arch-a7*`), los archivos del run (sin a6-r3.md: se
  retira su comprobación de blob) y la comprobación del objeto del cierre (= `owner_gate.OBJECT_BLOB`). `--dry-run` y `--record-probe` no ejecutan
  el binario.
- **Ayudantes:** `prepare_clone.py` (precondiciones de A-7: padre sin A-7, mismo blob en publicación, custodia y clon, A-6 acordada, §93 última sección,
  I-62-A7/ por añadido); `verify_clone.py` (clon, run, caracterización —blob también en el clon—, sesión nueva y la reproducción de las guardas de A-7:
  `run` con la cabeza abreviada custodiada y autoprueba con la copia de I-62-A7/ tal como estaba al ejecutarla, ambas iguales enteras a las
  custodiadas); `make_manifest.py` (cabecera de A-7 y mutación dirigida); `finish_kit.py` (pasos documentados, con la fijación de §93 y la prueba en
  seco sin bloqueos); `mutation-post-review.py` (el juez, sin cambio de lógica) y `mutation-targeted.py` (nuevo: el mismo juez sobre los sitios
  cambiados).

## Preparación ejecutada (2026-10-19): resultados

Pasos de `kit/finish_kit.py`, ejecutados uno a uno con sus mismas órdenes para revisar cada resultado; salidas en `prep/phase/`:

1. `prepare_clone.py 5158e4bc… 6fee8fd6… 5e3141a2…` (01:07:25Z, 33 s) → `D:\r62-arch-a7` (`git clone --no-local -c core.autocrlf=false
   --single-branch --branch architecture/portabilidad-coordinador-principal <directorio común de git> D:\r62-arch-a7`; HEAD separado en 5158e4bc;
   sin ramas ni remoto; reflog y gc) y `D:\r62-arch-a7-run\order.txt`; `kit/kit-params.json` (SHA-256 `e8982bf0…`). Las doce precondiciones en
   true (`prep/phase/step1.out`); añadido a I-62-A7/ tras la publicación: solo el resultado `run`.
2. `make_closure.py --render-prompt` → `kit/prompt.md` (77 659 bytes, 573 líneas, SHA-256 `ba280474…`). Se renderizó dos veces: la primera (77 548
   bytes) antes de un ajuste neutral de la plantilla en T04 («para un fixture B1 …, donde EFF^1 lleva el estado anterior a la activación» pasó a
   atribuirse a A-7 §3.1 y al preflight); los pasos 3, 4 y 7 y la autoprueba se repitieron después. 3. copiado al run, idéntico.
4. `make_closure.py` → `kit/closure.json` (SHA-256 `7a0be56b…`): **190 archivos, 5 053 543 bytes, 77 134 líneas** (I-62-A7/: 35; m7/: 53; r4/: 7;
   kit a6m7-measure: 44; B-min: 30; el resto, 21, con el objeto). **Ningún archivo no UTF-8.** Líneas de más de 2 000 caracteres sin copia legible:
   tres JSON de `s92/contract/out/`, `bench/p1-build.json`, `frozen/m7-run.snapshot.json`, seis sobres de `bmin/e2e/out/`, V14, A-3, decisiones y
   `prompt.md` (los tres JSON de `r4/r1probes/` tienen copia legible). C1-C3 e I-65 fuera.
5. **Fijación de §93:** `pin_authorization.py prep/pin93/decisiones-5158e4bc.txt 93` (texto del blob de decisiones de la HEAD, `git cat-file blob`,
   blob 1de169d0, SHA-256 `31e2db4c…`; copia temporal borrada tras la fijación porque, como el archivo del repositorio, contiene el identificador Git
   del host; reproducible con `git cat-file blob 5158e4bc:docs/automation/decisions/I-62.md`) → Ready (línea 1825, una sola sección, posterior a §92, blob completo como token, sin secciones posteriores); las tres líneas
   AUTH_* sustituidas por `prep/pin93/apply_pin.py` (`prep/pin93/owner_gate.pin.diff`: solo esas tres líneas). `owner_gate.py` fijado: SHA-256
   `879d630a…`. Se fijó antes de la autoprueba y de la mutación: ambas ejercen el archivo fijado (L00 en estado PINNED).
6. Compuerta D-01: `transport_gate.py init` + `set MEASURED kit/claude-cli-characterization.json` (SHA-256 `4ba6ba58…`) → `kit/transport-gate.json`
   en MEASURED (SHA-256 `6f78dbd5…`); `transport_gate.py check` código 0.
7. `verify_clone.py` → **AllChecks = true** (01:54:35Z-01:56:29Z; `kit/clone-verification.json`, SHA-256 `c75502f5…`): HEAD, separado, sin ramas
   ni remoto, limpio, 4 907 archivos, sin enlaces, blobs del cierre, historia del objeto y de la A-6 acordada, commits presentes y ausentes, run,
   caracterización (blob 19b8a27e en 1c960c0a y en el clon), sin directorio de proyecto `D--r62-arch-a7*` ni transcripción con el session id;
   guardas de A-7 reproducidas desde el clon contra el worktree (solo lectura): **`run` bb439090..6fee8fd6 PASS, igual entera a la custodiada** y
   **autoprueba 44/44 mutantes muertos, igual entera a la custodiada** (con la copia de I-62-A7/ de 28 archivos tal como estaba cuando run_a7.py la
   ejecutó; `Candidate.Added` = 29); worktree sin cambio de HEAD ni de estado. (Una ejecución válida anterior, antes del segundo renderizado, dio el
   mismo resultado de las guardas.)
   **Negativas** (`prep/neg_verify_clone.py` → `prep/neg-verify-result.json`, SHA-256 `77ab9d4b…`, 01:57:03Z-02:06:18Z): **4/4 PASS**, kit real sin
   cambio: N1 registro de caracterización alterado (1 byte) → ExternalCopyOk false; N2 order.txt del kit alterado → IdenticalToKitCopy false; N3 copia
   de I-62-A7/ para la autoprueba de las guardas con preflight.md → Added 30, distinta de la custodiada; N4 cabeza completa en lugar de la abreviada →
   resultado `run` distinto (Head). Las cuatro con código 1 y AllChecks false.
8. **Autoprueba COMPLETA** sobre el clon y el run REALES (`kit/selftest-result.json`, SHA-256 `4d9655a0…`; 01:56:58Z-02:32:35Z, 35 min 37 s):
   **283/283 PASS** — 178 casos de auditoría (los genéricos 00-55, 126-149 y 152-153 heredados; 55-121 de coherencia y esquema de A-7; 150-151p del
   registro de la compuerta 11), 61 de la compuerta de transporte (G00-G58), 40 de la compuerta 11 (O00-O30 y la sonda L00, en estado PINNED con §93
   en la HEAD 5158e4bc) y 4 de coherencia lanzador-auditor (X01-X04) —, con el clon limpio y los dos archivos del run idénticos a sus copias al
   terminar; los casos de auditoría usan la autorización del kit (§93). Antes, las ejecuciones parciales (`prep/dev/quick_cases.py`) pasaron O/X/L
   (44/44) y una muestra de once casos de auditoría. Una primera autoprueba completa, iniciada a las 01:46:34Z, se detuvo en el caso 18 para aplicar
   el ajuste de T04 (paso 2) y se repitió entera.
9. **Mutación DIRIGIDA** (`kit/mutation-targeted.py` → `kit/mutation-targeted-result.json`, SHA-256 `d1ed1222`; 2026-10-19T02:33:24Z → 2026-10-19T03:14:55Z): base 282/282 casos (sin L00) en PASS; **56/56 mutantes muertos**, 0 por excepción: `post-review.py` 37/37 (los sitios de `coherence()` y de la regla del registro de la compuerta 11, de 122 sitios; SHA-256 `303321f2`), `owner_gate.py` 19/19 (todos; `879d630a`), `transport_gate.py` 0 dirigidos (idéntico). Los 85 sitios no dirigidos de `post-review.py` son líneas byte a byte de v5.4-a6r4 (comprobado) y reutilizan 133/133 de `mutation-result.json` 424c983e; `transport_gate.py` reutiliza 40/40.
10. `make_manifest.py` → `kit/kit-manifest.json` (congelación): 26 archivos de `kit/` y los dos del run; `AuthorizationState` FIJADA §93; `OwnerGateTested`: el mismo `owner_gate.py` (`879d630a…`) en la autoprueba, en la mutación dirigida y en el kit (ChangedSinceSelftestOrMutation false); **SHA-256 `444d9ba99865dd1a6690d6682c84f2ea077438e83f3629a5625a5f238b92c0da`**.
11. `python -B kit/launch.py --record-probe` (desde esta carpeta, orden congelada exacta; 2026-10-19T03:16:12Z) → código 0: `RecordProbe.Ok` y `FrozenCommandGuard.Ok` en true (argv ['-B', 'kit/launch.py', '--record-probe']); registro sustituto con Outcome COMPLETED; `tasklist` con 89 bytes, no ASCII (consola cp850), tratado en bytes; `ProcessGoneAfter` True; temporal borrado (True); sin `<run>/launch`; listado del kit sin cambios. `record-probe.json` (UTF-8, SHA-256 `f98f8b07`; bytes crudos en `prep/phase/step11.raw`, tubería cp1252). El hijo sustituto es este intérprete de Python: ningún modelo.
12. `python -B kit/launch.py --dry-run` (ídem; 2026-10-19T03:16:24Z) → código 2 (siempre en seco): **`BlockingChecks` = []**; comprobaciones 1-3 y 6-11 en Ok; `NotRunInDryRun` = ['4-Version', '5-Auth']; `FrozenCommandGuard.Ok` (argv ['-B', 'kit/launch.py', '--dry-run']); `RecordProbeOk` = True; **`AuthorizationSections` = [§93, línea 1825, HeadingMatches True, HasBlob True, TextSha256Matches True]**; `LaterSections` = []; compuerta 11: `AuthorizationPending` False, líneas del Owner A6-ADR0046-9-FX02 [1572] (§78 en la línea 1564) y CLAUDE-CLI-I62 [1085], A-7 y A-6 en la HEAD = fijados (True, True), decisiones por añadido True, desciende del clon True, binario = cierre True, HEAD del worktree `5158e4bc17ad48160d8573b7f48f3923e3197f06`; comprobación 3: binario presente, SHA-256 `295b49ec092f31a7de9c6ff286d9d8f120a4f029b9ad9a5ac0b12c35cbf4df2b`, Authenticode Valid, firmante Anthropic True; comprobación 9: 11591 caracteres; comprobación 10: MEASURED; `Environment`: 34 nombres (valores solo de las PYTHON*). `launch-dry-run.json` (UTF-8, SHA-256 `7a9e22c5`; bytes crudos en `prep/phase/step12.raw`).

## Validación incremental (decisiones §88; §93, punto 2)

Matriz completa en `prep/validation-matrix.md` y `.json` (componente → SHA-256 anterior → nuevo → dependencias → controles RE-RUN o REUSED). Resumen:

- **Reutilizados byte a byte (SHA-256 = manifiesto b1342909 del kit de A-6 ronda 4):** `transport_gate.py` (e326b5b9), `c3_analysis.py` (4fc6115e),
  `read_fidelity.py` (7b51116e), `pin_authorization.py` (a8e40707) y `claude-cli-characterization.json` (4ba6ba58). Se repiten sus controles baratos
  (G00-G58, O30, la fijación real de §93, `transport_gate.py check`, verify_clone) porque dependen del cierre nuevo; su mutación se reutiliza
  (transport_gate.py 40/40 de `mutation-result.json` 424c983e).
- **Cambiados:** `launch.py` (constantes, docstring y la retirada de la comprobación de a6-r3.md; la lógica G/R/P/D y las comprobaciones 1-11 byte a
  byte); `owner_gate.py` (evaluate y check; §93 fijada); `post-review.py` (cabecera, registro de la compuerta 11 y coherencia del esquema nuevo; 85
  sitios de regla byte a byte); `selftest-post-review.py`; `make_closure.py`; `prepare_clone.py`; `verify_clone.py`; `make_manifest.py`;
  `finish_kit.py`; `mutation-post-review.py` (docstring y una etiqueta); `prompt.template.md`; y los generados (`result.schema.json`, `prompt.md`,
  `closure.json`, `kit-params.json`, `clone-verification.json`, `transport-gate.json`, `selftest-result.json`, `kit-manifest.json`). Nuevos:
  `mutation-targeted.py` y `mutation-targeted-result.json`; `order.txt` cambia de contenido (cuerpo §93). Retirados: `a6-r3.md` (premisa de A-6
  ronda 4) y `mutation-result.json` (citado por SHA-256, no copiado: describía los módulos de A-6 ronda 4).
- **Controles repetidos:** autoprueba completa (283 casos; la exigían el cambio del auditor, del esquema, de la compuerta 11 y del cierre),
  verify_clone positiva y 4 negativas, mutación dirigida (56/56) y los controles de lanzamiento obligatorios (manifiesto, sonda de registro,
  prueba en seco con cero bloqueos).
- **Batería completa de mutación: no ejecutada.** Decisiones §88, punto 4: «La repetición completa de mutaciones se reserva para cambios en las reglas
  del auditor, del esquema, semánticos del lanzador, en las compuertas…»; aquí cambian las reglas de coherencia del auditor, el esquema y la compuerta
  11. §93, punto 2, pide no ejecutar cientos de mutantes sobre componentes idénticos e identificar la cláusula exacta antes de ejecutar una batería
  completa. Se ejecutó la mutación dirigida a TODOS los sitios cambiados (los de `coherence()`, los de la regla del registro de la compuerta 11 y
  todos los de `owner_gate.py`), lo que §88, punto 3, exige para un componente cambiado («los mutantes de seguridad afectados»); los 85 sitios no
  cambiados de `post-review.py` y los 40 de `transport_gate.py` reutilizan 133/133 y 40/40. Si el Coordinator lee §88, punto 4, como exigencia de la
  batería completa (122 + 40 + 19 = 181 mutantes; del orden de 2-3 h), la cláusula es esa y la orden es `python -B kit/mutation-post-review.py
  <salida>`; no se ejecutó.

## Desviaciones frente al kit de A-6 ronda 4, con su motivo

- **Clon en la HEAD que contiene la autorización:** §93 está en el commit del clon y es su última sección (en A-6 ronda 4 la autorización era
  posterior al clon y quedaba fuera del cierre). La compuerta 11 usa BASE_SECTION = 92 (la disposición que preparó A-7 y prohibió lanzar sin una
  disposición posterior) y exige §93 fijada y ninguna sección posterior en la HEAD del worktree; decisiones por añadido desde el clon (el texto del
  clon termina en §93). El kit se entrega con la autorización FIJADA (no pendiente), y la autoprueba y la mutación ejercen el archivo fijado.
- **Run de dos archivos** (sin premisa de una ronda anterior): `launch.py` pierde la comprobación del blob de a6-r3.md; `verify_clone.py` y
  `make_closure.py` lo mismo.
- **Lectura directa:** V14 B.9 L2231-L2233 en lugar de V14 L2333-L2334 (motivo arriba, «Herramientas y lectura»). Código del auditor sin cambio.
- **Reproducción de las guardas:** la autoprueba de `a7-guards.py` depende de los archivos de su `--evid`; se reconstruye la copia de I-62-A7/
  existente cuando se ejecutó (orden de `run_a7.py`, fase p4) y se comprueba entera (Added 29); la cabeza del resultado `run` se pasa abreviada como la
  registra el resultado custodiado. Los temporales de las guardas incluyen objetos de git de solo lectura: `verify_clone.py` los borra con `onexc`
  (la primera ejecución falló al borrarlos, sin efecto en el clon ni en el worktree; corregido antes de la ejecución válida).
- **Compuerta 11:** comprueba además la A-6 acordada en la HEAD (§93, punto 8, «Sin alterar los acuerdos A-1..A-6»); el blob del objeto se compara con
  la constante fijada OBJECT_BLOB (en A-6 ronda 4, «distinto de los de las rondas 1-3»).
- **Esquema y auditor:** campos y reglas de §93 (arriba); los impactos relacionados de §93, puntos 4 y 5, son campos separados que solo exigen un
  REQUIRED si revelan un defecto de A-7.
- **Mutación dirigida** en lugar de la batería completa (arriba).
- **`finish_kit.py`** documenta la fijación de §93 como paso 5 y exige la prueba en seco sin bloqueos; no se ejecutó entero (como en A-6 ronda 4).

## Supuestos no medidos y riesgos residuales

1. **Duración frente al tope de 3 600 s.** A-6 ronda 4 duró 1 305 s (63 turnos, 51 lecturas). Esta revisión pide once temas, materialidad y dos
   impactos relacionados sobre un cierre de 190 archivos: del orden de 70-130 llamadas, ≈ 1 500-3 000 s. El tope no se cambió (§93, punto 6).
2. **Compactación automática del contexto:** el prompt avisa; contaría como un segundo mensaje de usuario (NOT_ACCREDITED).
3. **El hijo sustituto de la sonda no es `claude.exe`:** la sonda demuestra el código común de lanzamiento y registro en este entorno, no el tamaño ni
   la duración de la salida real, ni el refresco OAuth; se repite dentro del lanzamiento real.
4. **Entorno heredado:** el lanzador no pone ni quita variables; `claude.exe` heredará el entorno de quien lance (la prueba en seco registró sus
   nombres, entre ellos varias CLAUDE_CODE_* y ANTHROPIC_BASE_URL de la sesión que preparó el kit; valores solo de las PYTHON*). Lo decide la sesión
   principal antes de lanzar, como en A-6 ronda 4.
5. **Que el esquema (9 809 caracteres compactos) se comporte con claude-cli** como los anteriores (mismas palabras clave; objetos anidados de dos
   niveles en RelatedImpacts, como ClosureRuleMechanical en A-6 ronda 4).
6. **Anclaje:** la evidencia de I-62, el preflight, la determinación de §91, los borradores de §92 y las notas del Principal son de la sesión que
   preparó A-7; el prompt lo declara y los presenta como evidencia contrastable.
7. **Lo que el revisor no puede comprobar:** el repositorio de la entrega del fixture, el bundle binario del caso tipo B1, el banco completo
   (`bench/inputs/`) y el mapa de un B1 real; el prompt le pide declararlo en KnownLimitations si una conclusión depende de ello.
8. **`pin_authorization.py`** conserva su docstring histórica (§85, ronda 4) para no cambiar su SHA-256; su lógica usa la base de `owner_gate.py` (92).
9. **Residuo heredado:** el `clone_git` del auditor usa `text=True` sin codificación (regla del clon, byte a byte); sus salidas sobre un clon limpio
   son ASCII.
10. **Contexto del preparador:** este subagente recibió automáticamente el índice de memoria de usuario de la sesión (MEMORY.md); no lo usó para el
    contenido del kit, y el revisor no lo recibe (--safe-mode, otro proyecto de claude-cli, clon y run propios).

## Host: I-64 y checkout principal (solo lectura)

- `prep/host_checks.py` (git -C con GIT_OPTIONAL_LOCKS=0; procesos por Win32_Process) a las 2026-10-19T03:17:22Z: checkout principal en `main`, HEAD `95690c28dc6268e61dff32a0cbc33cc9fde3d47f`, 0 líneas de estado, origin/main local `bb0d5522e8411f66a51fdfb3f1f0d0514b737453` (los valores de preflight-s89); worktree de I-64 (`architecture-workspace-persistente-rackcad`) en `09f2f1c3` con 0 líneas de estado; ningún acad.exe, accoreconsole.exe, dotnet.exe, testhost.exe, MSBuild.exe ni VBCSCompiler.exe, ni proceso que nombre la ruta de I-64 o el clon; memoria libre 27.0 GB, CPU 10 %.
- **Observado durante la preparación** (`prep/phase/host-checks-early.json`, 2026-10-19T01:58:27Z): un `acad.exe` sin ruta de I-64 en su línea de órdenes (iniciado el 2026-10-18 a las 18:38 hora local, antes de esta preparación) y tres `dotnet.exe` (nodos de MSBuild y VBCSCompiler del SDK 8.0.423, iniciados a las 01:53Z) que esta preparación no lanzó; ninguno nombraba el clon ni I-64. Al terminar ya no estaban. La preparación no tocó el worktree de I-64 (solo `rev-parse` y `status`).
- Los índices de git del checkout principal, del worktree de I-62 y del de I-64 conservan sus fechas anteriores a esta preparación (ninguna escritura). Tres lecturas `git status` de la primera hora (worktree de I-62, checkout principal y worktree de I-64) se hicieron sin GIT_OPTIONAL_LOCKS=0; las fechas de los índices muestran que no escribieron nada; las demás, con GIT_OPTIONAL_LOCKS=0.


## Lanzamiento (no ejecutado)

Condiciones: §93 sigue siendo la última sección de decisiones en la HEAD del worktree (ninguna sección nueva hasta lanzar y auditar), la rama
desciende de 5158e4bc, A-7 = 6c29747d y A-6 = 4bc919dd en la HEAD, las líneas del Owner intactas y el binario con SHA-256 295b49ec…; en la misma
herramienta y entorno del lanzamiento, desde esta carpeta: `python -B kit/launch.py --record-probe` (código 0) y `python -B kit/launch.py --dry-run`
(código 2, `BlockingChecks` = []); después, UNA vez, `python -B kit/launch.py` (tope 3 600 s, sin reintento) y el auditor sobre la corrida completa:

```text
python -B kit/post-review.py "%USERPROFILE%\.claude\projects\D--r62-arch-a7\4945013a-d80e-43dc-bc95-d34dff85fcfe.jsonl" D:\r62-arch-a7-run\launch\stdout.jsonl D:\r62-arch-a7 D:\r62-arch-a7-run kit kit/result.schema.json audit.json output.json
```

El auditor informa por separado `Categories.Execution` y `Categories.ArchitectVerdict`; el veredicto no es el acuerdo (§93, punto 7). Ni A-7
AGREED, ni Controller ELIGIBLE, ni FX-02 PASS, ni F6 GATE PASS se declaran aquí.

## Archivos y privacidad

`kit/` es el kit congelado (manifiesto: `kit/kit-manifest.json`). Junto a este README: `record-probe.json` y `launch-dry-run.json` (salidas de la orden
congelada, convertidas a UTF-8; bytes crudos en `prep/phase/`). `prep/` es la preparación y no se custodia como kit: `base-a6r4/` (copia verificada
del kit de A-6 ronda 4, 27 archivos con el manifiesto b1342909), `adapt_*.py` (derivaciones por sustituciones y regiones exactas sobre esa copia),
`gen_schema.py`, `pin93/` (fijación de §93), `neg_verify_clone.py` y su resultado, `make_diffs.py` y `diffs/` (cada componente frente al de A-6
ronda 4), `make_matrix.py` y la matriz, `host_checks.py`, `file_table.py`, `ids.txt`, `phase/` (salidas y tiempos de cada paso) y `dev/` (ayudas de
desarrollo).

**Privacidad:** `prep/file_table.py` comprueba que ningún archivo de `kit/`, de `prep/` (salvo la copia verificada `prep/base-a6r4/`) ni de esta carpeta contiene el nombre de usuario de Windows (largo u 8.3), la identidad Git del host ni una dirección de correo (salvo las de dominios .invalid y noreply@anthropic.com), ni una ruta de perfil con nombre: sin coincidencias. Las rutas del perfil se escriben como %USERPROFILE%, %LOCALAPPDATA%, %APPDATA% o %TEMP%; del entorno solo constan nombres de variables y los valores de las PYTHON*.

```text
SHA-256                                                              bytes  archivo
d20c8d267b3382ae9de036cb9862e3df8df107d785e0e3f977d6bbc1fbb9bbce    50801  README.md
7a9e22c5d41c9a2ccba7cff1c680223beddec9e3085366c497ac402e9f4ca669     8679  launch-dry-run.json
f98f8b0717353dda0df85b960879d237074918275105517182852d2706d2cef2     2803  record-probe.json
4fc6115e2551b68efd46a20504764f2313f894eaddbb2e59b16dcdf72c1ecbb7     6070  kit/c3_analysis.py
4ba6ba586440ce5c9374c383a9a1b787cf42a17d8451c5d27e762c555cfc2a8c    17086  kit/claude-cli-characterization.json
c75502f5507381bf8b05a5c6b6b990ba9b1d8f1f2e5aa18e432c3c134e464225    48669  kit/clone-verification.json
7a0be56b9409ee61e7c3733b8a71dbfe601434b2a41f779f68a7062a5c1421ac   219805  kit/closure.json
e0c9c1eb7693b509e21402c309ded8ab76efd54065d03cb601b7218b3d597ec2     9485  kit/finish_kit.py
444d9ba99865dd1a6690d6682c84f2ea077438e83f3629a5625a5f238b92c0da     7322  kit/kit-manifest.json
e8982bf0ed92f8b2be1fd5c9e8439f011479f733b74a9b0f17540d594d31d18a     2373  kit/kit-params.json
01f60c7ad9cfdf2d1a7fc5f89dc122c6d31893a2929a14f009c7d7209f5737af    45834  kit/launch.py
a0d411070a82fa52dbd6b90a24826603e7c4e1d8c9fbffbf6db74b4a62e444d4    81714  kit/make_closure.py
bf03ada1133dd82e396e26b87f477b419042fa2e908dbe5e13f6e2007ff55df1     8782  kit/make_manifest.py
9ed3eca1d4687fa6adb7c6d56b7ddf9c72232aa2d238070c33f4fe2e95314640     7843  kit/mutation-post-review.py
d1ed12225973bf87cc615c48fc54871075de02a3a98fdb4999321e4a6bb24b81    18496  kit/mutation-targeted-result.json
44906dfde70d5ddcf81e23eb87ac0a04927865ddff46e795b605b1c9ace65ba0     6237  kit/mutation-targeted.py
a5c37f714364bc704c5f38218ae2b1a9209052c47834dcc5349684eb141be41b     4057  kit/order.txt
879d630a08d9fab2466ff434e158ef1f199887d8eafe555e9922e81f040c36ac    17884  kit/owner_gate.py
a8e4070751ae604010149c8678aa9320766c533230bf92de3aa2e88ea6e571cc     5588  kit/pin_authorization.py
303321f265152dc03a6fc7771bb69faed47f5167a80a7f03113e2839305d9653    67915  kit/post-review.py
fc0e0b24655de3fb9bfd3d84fc11bb76cedf43b3ed83b5c59fa38f7943dc5b28     9316  kit/prepare_clone.py
ba2804747151173d9fe66838ec0ad6ab311a07462948854c3317468497afa8c8    77659  kit/prompt.md
c3ad10d5052dc2ae1fb17072a7422759a376c1a7e570a8a677ca42ef17a7e500    36424  kit/prompt.template.md
7b51116e5e4876c1416066994e282ad077661547ee5fb1dcb0fbcbd59e413cce     4925  kit/read_fidelity.py
116c393e4923f97ba6d5371a72a21b4bbb5805a88f6a35a36876d09dfbcd5305    14788  kit/result.schema.json
e6deb731a303eac56a8186ad1141f687c9118940b314107877e154b087e45700   124017  kit/selftest-post-review.py
4d9655a08b3e5f5a3969eac8c37cf367ce0c4a260aeb3ea266084b2528b967c5   145997  kit/selftest-result.json
6f78dbd5595aa2744c0b883b9af298289bf6b3b9e50473362857385ba79d14da     1543  kit/transport-gate.json
e326b5b92da5ff2b53d05a7d64f90dbb09bf098ab4eeda5a92597d6fc36e6fbf    22485  kit/transport_gate.py
feb7d615a612e4ce17301d9dc2124ab85c2a5250739e4bab793310bb351b6c20    22733  kit/verify_clone.py
8ce22665027d8e0d949347683a3c461feedee05818ea546ef3c55b9a8bba3e44     7916  prep/adapt_make_manifest.py
73c2599d5c4df78c057e2306ae736c324b55b0af4691cdce0232f72dc7e76c20     5295  prep/adapt_post_review_head.py
41a4bd486df64f792df7c2bf68bfc24e347ce8725574d360d588a9280148378b    11916  prep/adapt_post_review_tail.py
a63752af92c20b17ee04ba4b08bf8fbbadf21e97d548268a85f8add35e1db394    66922  prep/adapt_selftest.py
37e45076b4c54526389eadaf2ea42a9116660ec1d84c850a53cec436d6d1c925     2796  prep/file_table.py
92a46900badeb26c82c9d05269d227b74e745d2bffbdb7b86cb42a37c6b75b74     8166  prep/gen_schema.py
66f740a72bafda2409d678ec6a42341d209e6b79241a175c66e056a72f9f0e42     3470  prep/host_checks.py
6eba75c9f28c29beebb0e4d311bfe5dc926173fbba8cb4ffe4b1b89f734fd054      271  prep/ids.txt
8f4a5dfc30ea68f94aa5ae27343ae85358c94bf1f8db48533a7030a2f12ddede     2484  prep/make_diffs.py
e3b997417b335d1dad61461867f17e45027a8b400e32e96a6ebbc5d992197a57    12976  prep/make_matrix.py
77ab9d4b8359517fcc2ea8d2d3d0e972c11cbce127b43a9a3808d510296a1fbc     2830  prep/neg-verify-result.json
02ff5a0f3365240d3197abef37fa2225d755cd9e6e3dba73bd501d9b75f36b31     6627  prep/neg_verify_clone.py
d4a58cb9458a81abf1ad758a7ca216d9dce8dc3db461271498eaa2e71c09ffec    18047  prep/validation-matrix.json
8b2d1741aa2272a13ee9e7ce6f553cab5524ca708e3236cb895593c043d013d5     9917  prep/validation-matrix.md
51b285371fb0a4ecfe0751ffd6061fb5981384e262c881d4b639843ac4986b9d   135665  prep/base-a6r4/a6-r3.md
4fc6115e2551b68efd46a20504764f2313f894eaddbb2e59b16dcdf72c1ecbb7     6070  prep/base-a6r4/c3_analysis.py
4ba6ba586440ce5c9374c383a9a1b787cf42a17d8451c5d27e762c555cfc2a8c    17086  prep/base-a6r4/claude-cli-characterization.json
82926e1500b8b69f19e4b90b3de5c08f639256a518cdf5deb7a9165ade879ba3    81886  prep/base-a6r4/clone-verification.json
694288d489188d44a3991da7eb914bb56ec477bbb745160df22aabcd8476b3e2   392318  prep/base-a6r4/closure.json
74928bb9d4390499179305f6ee2257b3dd38cfda0a8110cce26a65bf9b5902b1     8275  prep/base-a6r4/finish_kit.py
b1342909562478e5f99015666fd59fb4e7020cd1337b43ae0a8e87d4049d1d41     7332  prep/base-a6r4/kit-manifest.json
c8d805f6d934cc0b8d18c7f7941382a1da387a91f5094e94ac97f9e983b92aa5     2267  prep/base-a6r4/kit-params.json
0ada02349afde4ffb8bb98f0eeca879190963d86dbc426c4bb94ee483e52a94c    46012  prep/base-a6r4/launch.py
1c9b52bf01ab2869bedf0f93983a3cf6ba46b50dd738f306fea719a39115ec6b    87519  prep/base-a6r4/make_closure.py
f160f9119ab57f6ca0fca672945b30a7245622ab92fcb14005a6237da323ddd8     8666  prep/base-a6r4/make_manifest.py
4eaa1c8e33d0c706b3965b62e3fe544981f1e8fcd15352564d2613254a1a1305     7699  prep/base-a6r4/mutation-post-review.py
424c983eb68eaaa7052c491feafe98e5045df0d93a0fcc752dcab4cae926cf73    54267  prep/base-a6r4/mutation-result.json
d74e2aa1ce5eacc7dd6323cf3f3acec37fd6eb25f905ca4f2479dbca0cde5232     2805  prep/base-a6r4/order.txt
87ad933732b42655a992113e2e7fcc3eb4519dbb673063cee686df97838f75bb    17091  prep/base-a6r4/owner_gate.py
a8e4070751ae604010149c8678aa9320766c533230bf92de3aa2e88ea6e571cc     5588  prep/base-a6r4/pin_authorization.py
6c51758aaa9bc981f5f733e9fa9946d5402f8a9dbcbc543e580bdd1bab4b68cc    70728  prep/base-a6r4/post-review.py
0d23faf2f468e92cda65f634b610c7cc5a5c8b6edb0d19162fbcd603cc30cf0e     9251  prep/base-a6r4/prepare_clone.py
a8dc5773499ae56cf06718f4aed1cf46723eb66e6f1511166f6bd5e3e33ed0b9   105320  prep/base-a6r4/prompt.md
da40d1050c865e7032afc2a3502df32b4e80f0bb345d61ceb1cfb9605544a931    36763  prep/base-a6r4/prompt.template.md
7b51116e5e4876c1416066994e282ad077661547ee5fb1dcb0fbcbd59e413cce     4925  prep/base-a6r4/read_fidelity.py
f3dad55b008af984fb1ca6e57f32a311fd53d42d897fba1162bd8849f5a754ed    17165  prep/base-a6r4/result.schema.json
3ae15c6364f23290b66c52ebe2fd2a5a83243c5f1cc0081d968388f857fbab8e   128908  prep/base-a6r4/selftest-post-review.py
5ac8c720634d69778e6abbd3b22ca856d37914c6c1bb800ec75d2879edc09125   153229  prep/base-a6r4/selftest-result.json
790191bdfc445fda9885a5c98dc2546eca9402153dde0fe2e3a5c1bf4fa3ce08     1550  prep/base-a6r4/transport-gate.json
e326b5b92da5ff2b53d05a7d64f90dbb09bf098ab4eeda5a92597d6fc36e6fbf    22485  prep/base-a6r4/transport_gate.py
b8eaee883d15b8a6f8a4b96e8abac6eb896f27467cfd092cfea31f82105daecf    20899  prep/base-a6r4/verify_clone.py
8907aa39309ea5524b3c4cd90f92c2897863a8b5a59852f3cf3467c3c532dc61     8405  prep/dev/fill_readme.py
d08b41977c61ba0275cfc467610e5fa9092c29d7cef1eb2c22c03b636b4833bf    13410  prep/dev/privacy-block.md
2ed45e207a4f3a03a2aeacb593002d13b192c81ef56e7bc9a592f1033ebeac2d     2091  prep/dev/quick_cases.py
42673a7c542f2f20ebe64505b6786c0989775d002e3dfeac27103b30271cdb3f    14602  prep/dev/readme-part2.md
c69f1bdd78c7df064d97d45ace8e64265ab98294237479b9dbf7da2cefd5198e    15277  prep/dev/scan-5158e4bc.out
f3b6e3092993af70737d9e50afe17520d9ae6f73481a5f529df3fda223d922e3     3370  prep/dev/scan_candidates.py
58868af754856694d551c9e80c4c9eb9bde1f99265b679d5390eb463e483f243      775  prep/dev/show_lines.py
ce6a9fb09277343ad88a6586a1683fe461b22cc4315f704ef3b611835fac8529     2187  prep/dev/st-base-590-640.txt
b8f72e7da9f630ff055545b150084180f62e8fc74fac72cf73bf5581ff67050d    11487  prep/dev/st-base-93-300.txt
0a9960c70a94277db9ba7dfe2309dd924ed7cbd5a45e7289ed8784ee2b383914     6960  prep/diffs/diff-table.json
441846ae028ee0ee030df2fb32f36cd0d4598390a4eab70f6877e63ae2cd0bf8    12398  prep/diffs/finish_kit.py.diff
aadec01d6e1fee489e14c9eefdcac4a33773859bd6993f29a9ccbacb2431d5bb    13626  prep/diffs/launch.py.diff
57d15a0513f07a2a5ad91edb801523aa09180256221c21488d2431af59225554   121319  prep/diffs/make_closure.py.diff
25afa4b111854f6e93ba1672bd2483b8e13de305ef56be823372edbb23ef7c74     8554  prep/diffs/make_manifest.py.diff
fbc83101d498b7dd993fda804d188f1666741e1ff39e89c2c529183fa9d6e872     2812  prep/diffs/mutation-post-review.py.diff
a1859afb83e9a7569159f36c6f635ddfe8abb2b9e22e0af691e714db934720f4     7033  prep/diffs/order.txt.diff
afcaa3d9e6c5eee518e6c9ec6a7f59bfaf53a6c05208a8faaa323a29cb05b60d    19899  prep/diffs/owner_gate.py.diff
84a24e8c65995dd1a67a2e5662608194f311a948f0a306c44a4e2c67f2ecd0e1    32434  prep/diffs/post-review.py.diff
e33b0783eca4bc262e89f8cecc7f502e187a1eb1ee077cee6365fd11af8c82c1    13057  prep/diffs/prep_gen_schema.py.diff
b0d30dc6c932d183144202d66b127348fc22693a5b9902ad3cc703774d95aa3f    13051  prep/diffs/prepare_clone.py.diff
f21ab957f2f5883e1fbb1f7f5408444da4854461afcba2811db207ab5ca7bdce    65568  prep/diffs/prompt.template.md.diff
b45a8c1d87868a298a1a4fa03f3fdb3eba427377bbbd64031271dc4410af9326   104503  prep/diffs/selftest-post-review.py.diff
741e4cfa7030b2f8bed4f8c9b62d4927deff8b147f761111b5d7031d6eb90904    28761  prep/diffs/verify_clone.py.diff
6aef5bd6cf0bcadb404ed1bd5889209e8f910c1fc0ccbaf7383fbe191c36352f     2515  prep/phase/host-checks-early.json
2b1365d32d7bb5ac07ddbff111732e9a2e4bef9f71c941d0cd51015e0d489b9d      596  prep/phase/host-checks.json
99e09442d2597b3f44a9c09ea9fbf3c01b659cd3fe7f161e87fae5433c614b59     2333  prep/phase/make-diffs.out
612cd703f943360c74f2a11af563c2a601fb0bb1ac36ed3d0711ff8c65853668     4877  prep/phase/mutation.raw
5b16c5187387e67bbb3400171fda1f3affc929aedcae003e582c64abea592b74       47  prep/phase/mutation.times
188f518dff37ac889dea8d7e96c1ceef534398c3d2d5008d913474fbd86569fc      373  prep/phase/neg-verify.out
76a8f1886baa05ebce36386e765a7cb587fbeb6c8675f747cafd6a25767587ad       47  prep/phase/neg-verify.times
c203b99dfd6194900fc1b2219e4df85b2912528b7056a230ed515564ea90da00      201  prep/phase/step-schema.out
00508773470c9ef738637ce6f16ac2a681ccc05f4849e925f9b9c6f340103516      799  prep/phase/step1.out
f7a271818cefffcc5a5933881c3b3a457c31620eaf2760064305da846eea9d16      145  prep/phase/step10.out
e3b0c44298fc1c149afbf4c8996fb92427ae41e4649b934ca495991b7852b855        0  prep/phase/step11.err
779d0a6482fe2d9aa3e11cb4bc4a490af70573c64d88f75d4fc5ef0c28f3cb95     2917  prep/phase/step11.raw
c2202bca32bfb8bb720bb14baeb1dd819310091bb6cf8a55788ea92ad61fa999       47  prep/phase/step11.times
e3b0c44298fc1c149afbf4c8996fb92427ae41e4649b934ca495991b7852b855        0  prep/phase/step12.err
e35dcf32215ac387f23ca66ab33f7cd58bab72b64123914adb2039975848d5a0     8974  prep/phase/step12.raw
513482b3989447120a8bb6391eeeb057cca6976b9478eb73fa8cb33228d76423       47  prep/phase/step12.times
e429feb31242e747694ce8b0ac9daabb20be4f0e54cf4c538e45abf11e4252a7      134  prep/phase/step2.out
6dca5e1aacd9f4713ab1a5cf82a76cdcbaa92c9fd0489e3f56bf6badc223946e        9  prep/phase/step3.out
61ecbc8386725f1dfadf39a8d52a639dddca2ade5773d5ca15c75eea1a2e173b     6277  prep/phase/step4.out
4c20862e51a54537c20b198f4f2e6eaee3b29ed02fa472bc82020456f3e6dd2f       57  prep/phase/step5a.out
6868b0796e9ccc859f74a8060308e0f92a636751c392fffa26657c4c767e4673      228  prep/phase/step5b.out
64129da1dc90fc5c608dbd1a68605182fb3e4e3bdd21d8892da0bd1c5fab64ed      620  prep/phase/step5c.out
7c7caef9169e56344d4ad3ef477737c1b3a7cc08bc730d39a89c5c38a1381737      414  prep/phase/step6.raw
a2a3069628af598057ee951bd41970381a197c16e72c579a1790d6c46b0c00c1       42  prep/phase/step6.times
7cfe378396e622b711b323084f7b6e9e876a63d6eff0efeb1de1ffb623063b90      819  prep/phase/step7.modules-before
24355fcc7b9a61a39216239c4dc2eba35458bead32d40cf44a126256d861d3c6    19534  prep/phase/step7.raw
995cf07a86cc2d767fe6bf2570541c9e61c929889f403b0926502e2dbbc3ff9e       47  prep/phase/step7.times
180c49d16d3f9021cbe34f53d852c19419e19cb77138201164dd68c8562f9733      271  prep/pin93/apply.out
2fbac8fa69f956e8d74a12771d1c3123d30bc989b2e75ac3022dab193e35bee9     1637  prep/pin93/apply_pin.py
be509bdb1cabde5adf8f08868677bbb7d0f1f8ad3a55e3276a7b1ef00dd7938b     1078  prep/pin93/owner_gate.pin.diff
5aabeea444d9189ffcca417c36abe241654e73148f34be760b1f933680a87038     1488  prep/pin93/pin-authorization.json
209f980ed25adc732ef0b892420f664c6ea97edf563741163f53e67cac155b41      377  prep/pin93/python-lines.txt
```

## Mutación completa y nueva congelación (sesión principal, antes de la custodia)

- **Cláusula:** decisiones §88, punto 4: la repetición completa de mutaciones se reserva para los cambios en las reglas del auditor, del esquema y de las
  compuertas, y en este kit cambiaron las tres (auditor v5.5-a7, `result.schema.json`, `owner_gate.py`); no son componentes idénticos, así que §93, punto
  2, no la excluye.
- **Orden:** `python -B kit/mutation-post-review.py <out>`, de 2026-10-19T03:22:06Z a 2026-10-19T05:23:05Z: **181 de 181
  mutantes muertos**, ninguno superviviente (`post-review.py` 122/122, `transport_gate.py` 40/40, `owner_gate.py` 19/19), línea base 282/282.
- **Incorporación:** `kit/mutation-result.json` (SHA-256 `ac6a6ebab163150a949b53478bab12a5e1c81f80cf2ef64dbff4550a3c26e3f6`) y la declaración en
  `make_manifest.py` (solo el texto del campo `Auditor` del manifiesto); manifiesto vuelto a congelar: `kit-manifest.json` SHA-256 `c27db93ef6834a3e29bf36bd66f586c75b9281c46508ee2296ba14e86eab6327`
  (27 archivos; solo se añade `mutation-result.json` y cambia `make_manifest.py`).
- **Prueba en seco repetida** sobre el kit vuelto a congelar (`launch-dry-run.json`): `BlockingChecks` = [], integridad del kit sin discrepancias,
  comprobación previa de registro conforme, compuerta 11 con §93 (línea 1825, texto `4f66b738…`) y ninguna sección posterior. La prueba en seco anterior
  del preparador se conserva como `launch-dry-run-before-full-mutation.json`.

