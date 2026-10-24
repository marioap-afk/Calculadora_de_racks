"""Paso B de prompt.template.md (kit de la RONDA 4 de A-7; decisiones §98, punto 7) sobre la salida del paso A: se reescriben la cabecera y el
alcance, el Paso 0, «Qué decidir» y «Resultado» para la ronda 4 (A62-A7R3-R1: el registro de rebase en los controles negativos sobre las entradas de
una reverificación de 16.7), y se corrigen en el resto las descripciones del cierre (preflights de las rondas 3 y 4, revisión de la ronda 3) y las
menciones históricas. Uso: python -I -B patch_prompt_r4.py
"""
import hashlib, os

PREP = os.path.dirname(os.path.abspath(__file__))
SH, KIT = os.path.join(PREP, 'shifted'), os.path.join(os.path.dirname(PREP), 'kit')
t = open(os.path.join(SH, 'prompt.template.md'), encoding='utf-8', newline='').read()


def region(t, start, end, new):
    """Sustituye [start, end) (marcas literales: start incluida, end excluida; cada una una sola vez)."""
    assert t.count(start) == 1 and t.count(end) == 1, (start[:40], end[:40], t.count(start), t.count(end))
    a, b = t.index(start), t.index(end)
    assert a < b
    return t[:a] + new + t[b:]


HEAD = '''I-62 — REVISIÓN FORMAL DEL ARCHITECT DE LA RONDA 4 DE LA ENMIENDA A-7 (admisión de los insumos del cierre de VERIFY de FX-02 en F6), acotada a la corrección de A62-A7R3-R1 (el registro de rebase en los controles negativos con invocación que se ejecutan sobre las entradas de una reverificación de AUTOMATION_PLAN 16.7) y a sus consecuencias (decisiones §98, puntos 2 y 7), con la autorización del Owner CLAUDE-CLI-I62 = A. ÚNICA invocación, sin reintento.

Eres el ARCHITECT de esta revisión: un proceso `claude-cli` nuevo, separado de la sesión autora de A-7 y de sus preflights, de las revisiones de las rondas 1 a 3, de los agentes que construyeron el banco de la medición, el kit `a6m7-measure` y las determinaciones de §91 y §92, de quienes escribieron el paquete B-min, del Coordinator y de las revisiones de A-1..A-6. Revisas en solo lectura la versión exacta de la ronda 4 de A-7 y devuelves un único veredicto formal (LIFECYCLE §5 y §6). No redactas texto normativo de sustitución, no implementas nada, no escribes, no haces commit ni push y no invocas a otros agentes.

## Identidades

```text
RunId                  = {{RUN_ID}}
InvocationId           = {{INVOCATION_ID}}
LogicalReviewRequestId = {{LOGICAL_ID}}   AttemptSeq = 1
Clon de lectura        = {{CLONE}}   (tu directorio de trabajo; HEAD separado en el commit, sin ramas locales; sin remoto)
Commit                 = {{CLONE_COMMIT}}   (registro de la ronda 4: resultado `run` de sus guardas)
Publicación ronda 4    = {{PUBLICATION}}   (A-7 ronda 4, PROPUESTA; padre {{PUB_PARENT}}, con la ronda 3 y decisiones §98)
Objeto                 = docs/initiatives/I-62-A-7.md   blob {{OBJECT_BLOB}}   ({{OBJECT_BYTES}} bytes, {{OBJECT_LINES}} líneas)
Ronda 3                = {{RUN}}\\a7-r3.md   (blob 8bcccb2fd2cea480f73ee5bf5a1583e0c9eaf1ff, commit 37ac6ca4; {{A7R3_BYTES}} bytes, {{A7R3_LINES}} líneas)
Revisión de la ronda 3 = R20261023T180846Z-62bd (auditor ACCREDITED; Architect CHANGES REQUIRED con A62-A7R3-R1 y A62-A7R3-O1..O3)
A-6 acordada           = docs/initiatives/I-62-A-6.md   blob 4bc919dd4768ab81abc74b70d3a4a3821c07feb5 (decisiones §90)
A-5 acordada           = docs/initiatives/I-62-A-5.md   blob ad94ceedacbf3dbf70f0217a583978d0e32592fc (decisiones §73)
Paquete del Architect  = ninguno (este texto hace de paquete)
Orden vigente          = {{RUN}}\\order.txt   (cuerpo de la disposición §98 del Coordinator; 3 740 bytes, {{ORDER_LINES}} líneas, SHA-256 8b73975d9822a430f1a726268d4005609974dbdcc85a0674bf00dbb9e9018acf)
```

**Recibo.** El invocador comprobó que el objeto tiene el blob `{{OBJECT_BLOB8}}…` en la publicación, en el registro y en el commit de arriba, y que el
padre de la publicación tiene la ronda 3 (`8bcccb2f…`, el mismo blob que `a7-r3.md` y que el de A-7 en `37ac6ca4`); que la A-6 acordada (`4bc919dd…`)
no cambia del padre de la publicación al commit; que `docs/automation/evidence/I-62-A7/r4/` no existía en el padre, no cambia después de la
publicación salvo por el añadido del resultado `run` y que su `MANIFEST.sha256` coincide con los archivos; que el preflight de la ronda 3
(`docs/automation/evidence/I-62-A7/r3/`) no cambia desde el padre de la publicación; que decisiones solo crece por añadido desde el registro de §97 y
que §98 es su última sección en el commit del clon; que la revisión de la ronda 3 custodiada es la de A62-A7R3-R1 y O1-O3; que el paquete B-min,
`agent-execution/`, `m7/s91/`, `m7/s92/`, `m7/bench/`, `m7/readings/`, el kit `a6m7-measure`, `r3/`, `r4/` y la revisión de la ronda 3 conservan los
árboles de los commits que los custodiaron; y que las guardas custodiadas de la ronda 4 reproducen sus resultados (`run` sobre la publicación y la
autoprueba).

**Condición de lanzamiento (compuerta 11).** Decisiones §98, punto 7 (línea {{DEC98_P7}}; `order.txt`, líneas {{ORDER_P7}}), ordena preparar el kit
acotado sin ejecutar una revisión formal sin una disposición nueva. Esta invocación solo existe si una disposición del Coordinator POSTERIOR a §98,
que nombra esta revisión, se fijó en el kit (su número, su encabezado y el SHA-256 de su texto) y el lanzador comprobó inmediatamente antes de
lanzarla, en el registro de decisiones de la rama de I-62: esa sección, una sola vez, con el encabezado y el SHA-256 fijados y el blob completo del
objeto; ninguna sección posterior a ella; la línea del Owner `A6-ADR0046-9-FX02 = A (…)` de §78 como línea completa; la línea `CLAUDE-CLI-I62 = A (…)`
vigente y el binario medido igual al del cierre; y A-7 y A-6 en la HEAD de la rama con los blobs de arriba. Esa disposición es posterior al commit
del clon y no está en el cierre: tu orden vigente es §98 (`order.txt`). No juzgas esa autorización ni las determinaciones del Owner; juzgas la ronda 4
de A-7.

**Objeto y revisión.** La ronda 3 de A-7 (`a7-r3.md`) admitió en la categoría (c) el registro de rebase de la reverificación de AUTOMATION_PLAN 16.7
(A62-A7R2-R1). Su revisión formal (R20261023T180846Z-62bd, auditor ACCREDITED) dio CHANGES REQUIRED con un REQUIRED, A62-A7R3-R1 (los controles
negativos con invocación que RAE §10 ejecuta sobre las entradas de una reverificación de 16.7 VERIFIED no admiten el registro de rebase, y su
coherencia se comprobaría con la entrega mutada o ausente del control), tres OPTIONAL (A62-A7R3-O1..O3) y A62-A7R2-R1 PARTIALLY_CORRECTED por ese
residuo. Decisiones §98 aceptó A62-A7R3-R1 como REQUIRED (punto 1, línea {{DEC98_P1}}) y autorizó la cuarta candidata «exclusivamente para resolver
A62-A7R3-R1», extendiendo la admisión del papel REBASE a los controles negativos de RAE §10 que deriven de una reverificación válida de 16.7, con la
identidad de la reverificación de origen, el vínculo de `RunId`, tarea e intento, un `RebaseMap` coherente con el registro original, `MainSha_eval`
derivado de la reverificación válida, la reproducción de las entradas sin sustituciones no autorizadas, las mutaciones propias de cada control y el
rechazo de registros ajenos, inventados o incompatibles, y sin REBASE en la VERIFY ordinaria ni en la medición de A6-7 cuando no haya una
reverificación aplicable (punto 2, línea {{DEC98_P2}}); el preflight actualiza solo las piezas dependientes y conserva A-5, A-6, EFF/EFF^1, los
oráculos y las líneas del Owner (punto 4, línea {{DEC98_P4}}); los opcionales O1-O3 se tratan sin ninguna enmienda nueva (punto 5); y los límites no
cambian (punto 8, línea {{DEC98_P8}}: sin Codex, sin consumir A6-7, sin B1 ni FX-U2 y sin modificar A-1..A-6, producción, I-65 ni D.5). A-7 §3.8
(L{{A7_S38}}) lista lo que cambió: A7-1, puntos 1 (c), 2 (c) y 5 (c), y cambios descriptivos en otras secciones, con la matriz de modalidades
resumida (L{{A7_S38_MATRIX}}) y los residuos de O1 y O2 en §3.5. La revisión de la ronda 4 es ACOTADA (§98, punto 2) a:

- la resolución de A62-A7R3-R1: la copia del registro de rebase en cada control negativo con invocación que se ejecuta sobre las entradas de una
  reverificación de 16.7 (A7-1, punto 1 (c)); la identidad de la reverificación (su `RunId` y el SHA-256 de su designación, que cumple el punto 2
  (c) con sus propias entradas); el vínculo (`UnitId`, `TaskId`, `Attempt`, `AuthorityRevision` y `Target` de la reverificación; un `RunId` propio); la
  copia del registro (mismo blob y mismo `RunId`, en el directorio del intento de la ruta del control); las copias de las demás entradas, salvo los
  hechos del relevo y el artefacto con los campos mutados; la coherencia anclada en la reverificación y `MainSha_eval` = su `MainAfterSha`; la
  ausencia y el rechazo de lo ajeno; y la no admisión en la verificación ordinaria, en sus controles y en la medición (A7-1, puntos 2 (c) y 5 (c));
- la pureza del delta: los cambios normativos solo por A62-A7R3-R1, y los descriptivos que declara §3.8 dentro de ese alcance (con los residuos de
  A62-A7R3-O1 y O2, que no cambian normas, y la enumeración de O3);
- la ausencia de regresión sobre lo que la ronda 3 encontró corregido o conforme;
- un dictamen sobre la compatibilidad con A-5 y con A-6, que el punto 4 manda conservar intactas (línea {{DEC98_P4}});
- la reconfirmación de la materialidad (M-01..M-08) y de que no hay ninguna consecuencia OWNER-RESERVED no identificada;
- las preguntas de la ronda 4 (A-7 §8, Q-A7R4-01..03).

Lo que dependa de una sesión real o de una disposición futura (la medición de A6-7, las disposiciones previas a la medición, la nueva preparación
del kit de la medición, B1) sigue como precondición con fallo cerrado. La matriz de modalidades (`modalities.md` del preflight de la ronda 4) es
evidencia de apoyo, no autoridad. Sin acreditación excepcional: si no puedes cumplir un punto con lecturas fieles, no lo des por cumplido. No se te
prohíbe señalar otro defecto material de A-7 que encuentres: hazlo como hallazgo.

'''
t = region(t, 'I-62 — REVISIÓN FORMAL DEL ARCHITECT DE LA RONDA 4', '## Herramientas e identidad', HEAD)

PASO0 = '''## Paso 0, antes de cualquier lectura sustantiva

1. **Lectura directa de RAE §10 (obligatoria).** Lee con Read `{{READ_DIRECT}}` con `offset` {{DIRECT_OFFSET}} y `limit` {{DIRECT_LIMIT}}:
   incluye las líneas {{DIRECT}} (los controles negativos se ejecutan solo sobre las entradas de la verificación `EXECUTION_VERIFIED` del último
   `WorkRunId`, con una ruta y un registro propios y copias de todas las entradas que conservan sus ids internos salvo el campo mutado; el oráculo
   relativo), que A7-1, punto 2 (c), y las filas L15 y L23 de A-7 §2 citan. Tiene que ser una llamada a Read sobre ese archivo (si es una copia fiel,
   tiene la misma numeración de líneas): la cita de A-7 no es una lectura de RAE. Regístrala en `DirectReads`
   (`docs/automation/agent-execution/README.md:{{DIRECT}}`).
2. Lee entera, con Read, `{{RUN}}\\order.txt` (la orden vigente, {{ORDER_LINES}} líneas): punto 1 (líneas {{ORDER_P1}}), punto 2 ({{ORDER_P2}}), punto 3 ({{ORDER_P3}}),
   punto 4 ({{ORDER_P4}}), punto 5 ({{ORDER_P5}}), punto 6 ({{ORDER_P6}}), punto 7 ({{ORDER_P7}}), punto 8 ({{ORDER_P8}}) y punto 9 ({{ORDER_P9}}).
3. Lee con Read las líneas {{DEC98}} de `{{READ_DEC}}` (decisiones §98). Comprueba que la línea {{DEC98_SOURCE}} declara el cuerpo pegado de §98 con
   3 740 bytes y el SHA-256 de arriba; que los puntos 1-9 (líneas {{DEC98_P1}}-{{DEC98_P9}}) registran lo que dice `order.txt` (con la redacción del registro); y
   que el punto 1 acepta A62-A7R3-R1 como REQUIRED y el punto 2 autoriza la cuarta candidata, limitada a admitir REBASE en los controles negativos que
   derivan de una reverificación válida de 16.7.
4. Lee entero, con Read, `{{READ_GRR3}}` (`{{GRR3}}`, {{GRR3_LINES}} líneas): {{GRR3_KEYS}} deben dar `A7Blob` = el blob del objeto, `Base` = el padre de la
   publicación (`{{PUB_PARENT}}`), `Head` = la publicación (`{{PUBLICATION}}`), `Mode` run y `Result` PASS.
5. Lee con Read, en `{{READ_SUMMARY3}}` (`{{SUMMARY3}}`), las líneas {{SUMMARY3_CAND}} (`Candidate.Blob` = el blob del objeto), {{SUMMARY3_HOLD}} (`Hold` =
   `Cases` y `NotHolding` vacío) y {{SUMMARY3_ST}} (autoprueba de las guardas de la ronda 4).
6. Lee con Read, en `{{READ_R2OUT}}` (`{{R2OUT}}`), las líneas {{R2OUT_KEYS}}: `RunId` = R20261023T180846Z-62bd, `ReviewedBlob` = el blob de la ronda 3
   y `RequiredFindings` con A62-A7R3-R1.

Si algo no coincide, o si un archivo del run o del objeto no se puede leer, para: `Verdict` = `"NOT_ACCREDITED"`, con el motivo en `KnownLimitations`.

'''
t = region(t, '## Paso 0, antes de cualquier lectura sustantiva', '## Cierre de insumos (solo esto)', PASO0)

OBJ = '''**Objeto**, entero: `docs/initiatives/I-62-A-7.md` (léelo con Read en `{{READ_A7}}`): {{A7_MAP}}. Texto de A7-1: L{{A7_RULE}}; puntos {{A7_POINTS}};
categorías del punto 1: {{A7_CATS}}. Tabla de categorías de §3.3: L{{A7_S33_T}}. Relación con A-6 y con R7-A: §3.4 (L{{A7_S34}}). §3.5 (L{{A7_S35}}; residuo de
`clausemap` L{{A7_S35_RES}}; residuos de A62-A7R3-O1 L{{A7_S35_O1}} y de A62-A7R3-O2 L{{A7_S35_O2}}). Correcciones de la ronda 2, §3.6 (L{{A7_S36}}). Corrección de
la ronda 3, §3.7 (L{{A7_S37}}; fila de A62-A7R2-R1 L{{A7_S37_ROW}}). Corrección de la ronda 4, §3.8 (L{{A7_S38}}): alcance L{{A7_S38_SCOPE}}; fila de A62-A7R3-R1
L{{A7_S38_ROW}}; matriz de modalidades L{{A7_S38_MATRIX}}. Fila del registro de rebase en §4: L{{A7_S4_REBASE}}. Materialidad: §5 (L{{A7_S5}}). Compatibilidad:
§6 (L{{A7_S6}}; A-5 L{{A7_S6_A5}}; A-6 L{{A7_S6_A6}}). Consecuencias reservadas al Owner: §7 (L{{A7_S7}}). Preguntas: §8 (L{{A7_S8}}; las de la ronda 4, filas
L{{A7_QR4}}).

**Preflight dirigido de la ronda 4** (`docs/automation/evidence/I-62-A7/r4/`, {{A7R4EVID_COUNT}} archivos, enteros; evidencia de apoyo, no autoridad: el
constructor `make_eic_a7.py`, el validador `check_a7_closure.py`, el formato `a7-designation-4.schema.json` (Kind REVERIFICATION_CONTROL con `Source`),
la derivación del lado del aceptante `designate_a7.py` y `derive_r4.py`, los scripts que derivan el candidato y las piezas (`make_cand_r4.py`,
`patch_tools_r4.py`, `patch_guards_r4.py`, `make_runner_r4.py`), las designaciones (`designations-r4/`: la reverificación RV, los ocho controles
sobre sus entradas, nc3 ordinario, la reejecución y la segunda recuperación), las entradas sintéticas (`inputs-rvc/`, las copias de los controles
sobre RV), las fases q0-q10 (q1-q5 y q8: las de las rondas 2 y 3, repetidas sin regresión; q9: los controles sobre RV con su cotejo de entradas y
sus negativos; q10: con y sin el instrumento, nc3 ordinario, reejecución, A62-A7R3-O1 con el instrumento y el aceptante fijados y A62-A7R3-O2 sobre
un segundo rebase real), la matriz `modalities.md`, las guardas con su autoprueba y el resultado `run`; `preflight.md` es su informe):

```text
{{A7R4EVID_TABLE}}
```

**Preflight dirigido de la ronda 3** (`docs/automation/evidence/I-62-A7/r3/`, {{A7R3EVID_COUNT}} archivos, enteros, sin cambio desde la ronda 3: la
evidencia de la reverificación misma, con `a7-designation/3`):

```text
{{A7R3EVID_TABLE}}
```

**Revisión de la ronda 3** (`docs/automation/evidence/I-62-architect-A-7/R20261023T180846Z-62bd/`, tres archivos): `output.json` (A62-A7R3-R1 y O1-O3,
la resolución de A62-A7R2-R1, la pureza del delta, la regresión, la compatibilidad, la materialidad y las preguntas: {{R2OUT_RANGES}}), `audit.json`
({{R2AUDIT_RANGES}}) y `README.md`.

```text
{{R2REV_TABLE}}
```

'''
t = region(t, '**Objeto**, entero: `docs/initiatives/I-62-A-7.md`', '**Evidencia de la medición de A6-7 (`m7/`)**', OBJ)

DECIDE = '''## Qué decidir

Se pide el veredicto formal sobre la ronda 4 exacta de A-7 (LIFECYCLE §5 y §6), acotado a A62-A7R3-R1 y a sus consecuencias (decisiones §98, puntos 2
y 4). Contesta con el texto normativo del cierre y con lo que leas.

- **`Verdict`:** `AGREED` (cero REQUIRED abiertos sobre la versión exacta), `CHANGES REQUIRED` (al menos un REQUIRED, corregible dentro del alcance de
  §98, punto 2), `REJECTED` (al menos un REQUIRED y la corrección no es admisible dentro de ese alcance: necesitaría otra disposición) o
  `BLOCKED — OWNER DECISION` (falta una autoridad reservada: una consecuencia OWNER-RESERVED que A-7 no identifica, o una que no esté dispuesta; A-7
  §7). Un REQUIRED impide el acuerdo hasta su disposición y corrección.
- **Hallazgos.** Un hallazgo es REQUIRED en los casos de LIFECYCLE §5 (activa M-01..M-08 sobre un elemento congelable, deja un invariante sin
  obligación de prueba, produce comportamiento observable distinto, o revela un elemento materialmente incorrecto, no ejecutable o no verificable);
  una precisión sin cambio de significado es OPTIONAL. Ids de esta revisión: REQUIRED `A62-A7R4-R1`, `A62-A7R4-R2`…; OPTIONAL `A62-A7R4-O1`…. Cada uno
  con la sección exacta de A-7 (`Section`), su evidencia (`Evidence`: al menos una referencia) y la corrección (`CorrectionRequired` en un REQUIRED,
  `SuggestedCorrection` en un OPTIONAL). Un REQUIRED trae además la autoridad o el invariante violado (`ViolatedInvariant`) o un contraejemplo concreto
  (`Counterexample`), al menos uno no vacío, y por qué importa (`WhyItMatters`). Un hallazgo es siempre sobre A-7. El id de la ronda 3 (`A62-A7R3-R1`)
  solo se usa como identificador de `PriorFindingResolution`, nunca como hallazgo nuevo.
- **`PriorFindingResolution`** (§98, punto 2; A-7 §3.8), una vez `A62-A7R3-R1`: `Status` `CORRECTED` (la ronda 4 corrige el hallazgo y no deja nada
  abierto; no cita ningún REQUIRED), `PARTIALLY_CORRECTED` o `NOT_CORRECTED` (citan el REQUIRED residual de esta revisión), con `Rationale`, `ClauseRefs`
  y `FindingIds`. Lo que pidió el hallazgo está en `output.json` de la ronda 3 ({{R2OUT_REQ}}) y en la fila de §3.8; contrasta el texto nuevo con
  `a7-r3.md` y con las fuentes (RAE §10, L15 y L23 de A-7 §2; los contratos del Controller de FX-02, L24; AUTOMATION_PLAN 16.4, 16.7 y 16.9, L25, L19 y
  L26; AUTOMATION_PLAN 16.6 y 16.13, L18 y L20; V14 D.3, L27; el esquema `relay-record/v2`, L21; A6-3, regla 6 (b), L{{A6_A63_R6B}}; A6-9, regla 4,
  L{{A6_A69_R4}}, y su tabla, L{{A6_DERIV}}), y con lo que §98, punto 2, exige: la identidad de la reverificación de origen; el vínculo de `RunId`, tarea e
  intento; un `RebaseMap` coherente con el registro original; `MainSha_eval` derivado de la reverificación válida; la reproducción de las entradas sin
  sustituciones no autorizadas; las mutaciones propias de cada control; el rechazo de registros ajenos, inventados o incompatibles; y la no admisión en
  la VERIFY ordinaria y en la medición de A6-7 sin una reverificación aplicable.
- **`DeltaPurity`** (§98, punto 2: «exclusivamente para resolver A62-A7R3-R1»; punto 4: solo las piezas dependientes): en `Changes`, una vez cada
  cambio de la ronda 4, con `Assessment` `WITHIN_SCOPE` (el cambio está dentro de la corrección de A62-A7R3-R1 o es descriptivo y no cambia el
  significado normativo; no cita ningún REQUIRED) u `OUTSIDE_SCOPE` (un cambio normativo que no exige A62-A7R3-R1; cita un REQUIRED):
  - `R3R1_REBASE_COPY_IN_REVERIFICATION_CONTROLS`: el registro de rebase en la reverificación y, como su copia, en cada control negativo con invocación
    que se ejecuta sobre sus entradas; ninguna otra invocación lo tiene (A7-1, punto 1 (c));
  - `R3R1_CONTROL_IDENTIFIES_REVERIFICATION`: la designación del control identifica la reverificación por su `RunId` y por el SHA-256 de su
    designación, que cumple el punto 2 (c) con sus propias entradas (punto 2 (c));
  - `R3R1_CONTROL_LINK`: el control tiene el `UnitId`, el `TaskId`, el `Attempt`, la `AuthorityRevision` y el `Target` de la reverificación y un `RunId`
    propio (punto 2 (c));
  - `R3R1_RECORD_COPY_AND_PATH`: la copia del registro, con el mismo blob y el mismo `RunId`, en el directorio del intento de la ruta del control
    (punto 2 (c));
  - `R3R1_COPIES_AND_MUTATIONS`: cada otro artefacto de (c) es la copia del de la reverificación (mismo blob y misma ruta; la delegación y la entrega,
    en la ruta del control), salvo los hechos del relevo y el artefacto con los campos que el control muta, o la entrega ausente de N4 (punto 2 (c));
  - `R3R1_ANCHORED_COHERENCE_AND_MAINSHA_EVAL`: la coherencia del registro es la de la reverificación, comprobada con sus entradas y no con la entrega
    mutada o ausente del control, y `MainSha_eval` es el `MainAfterSha` de ese registro (punto 2 (c); 16.13);
  - `R3R1_ABSENCE_AND_NON_ADMISSION`: si la reverificación identificada, su registro, la copia o alguna copia del control no cumplen el punto 2 (c), no
    hay cierre ni lanzamiento; en la verificación ordinaria, sus controles y la medición, el registro no se admite (punto 5 (c));
  - `S33_S34_S4_WITH_O3`: la fila (c) de §3.3, las notas de §3.4 (C6 y la enumeración de A62-A7R3-O3) y la fila del registro de rebase de §4;
  - `S35_RESIDUALS_O1_O2`: los residuos declarados de A62-A7R3-O1 (C1 del aceptante sin `MainAfterSha` ni las imágenes) y de A62-A7R3-O2 (segunda
    recuperación sin cierre), sin cambio normativo (§98, punto 5);
  - `DESCRIPTIVE_SECTIONS`: las demás secciones descriptivas que declara §3.8 (cabecera, §2 con L23-L27, la nota de §3.7, §3.8 con la matriz, §5 con
    M-03 y M-04, §6, §8, §10, §11 y el Anexo);
  - `PROCEDURE_PIECES_ROUND4`: las piezas del procedimiento de la ronda 4 que identifican el invalidador (constructor, validador, formato
    `a7-designation/4` y `designate_a7.py`; `derive_r4.py` sin cambio), sin cambio del texto del punto 7;
  y en el conjunto, `Status` `CONFIRMED` (todos `WITHIN_SCOPE`) o `NOT_CONFIRMED` (alguno `OUTSIDE_SCOPE`; cita un REQUIRED), con `Rationale`,
  `ClauseRefs` y `FindingIds` en cada cambio y en el conjunto.
- **`RegressionCheck`** (consecuencias de la ronda 4 sobre lo que la ronda 3 encontró corregido o conforme; `output.json` de la ronda 3,
  {{R2OUT_REG}}), una vez cada tema, con `Status` `NO_REGRESSION` (la ronda 4 lo conserva; no cita ningún REQUIRED) o `REGRESSION` (cita un REQUIRED),
  `Rationale`, `ClauseRefs` y `FindingIds`:
  - `R2R1_REVERIFICATION_RECORD_AS_CORRECTED`: el registro de rebase de la reverificación misma (papel, ruta y `RunId`, coherencia, `MainSha_eval`,
    ausencia y sustitución), que la ronda 3 encontró corregido;
  - `R2R1_NON_ADMISSION_ORDINARY_VERIFY_AND_MEASUREMENT`: la no admisión en la verificación ordinaria, en sus controles y en la medición;
  - `Q_A7R3_02_DELIVERY_FAIL_CLOSED`: en la reverificación, sin la entrega no hay cierre (Q-A7R3-02, SATISFIED en la ronda 3);
  - `Q_A7R3_03_CATEGORY_B_BASESHA`: la categoría (b) con el `BaseSha` de la delegación (Q-A7R3-03, SATISFIED en la ronda 3);
  - `R1_TRX_AND_RELAYFACTS_AS_CORRECTED`, `R2_INVALIDATOR_RULE_AND_PROCEDURE` y `R3_A62_RULE1_PRECISION`: las correcciones de las rondas 2 y 3 que
    siguen vigentes (los papeles de (c) con los TRX y los hechos del relevo; el invalidador = A7-1 en el blob acordado más el procedimiento nombrado;
    la precisión de A6-2, regla 1);
  - `T04_EFF_EFF1_B1_MAP_VALIDATION`, `T05_MAP_INVALID_ABSENT_NONCONSTRUCTIBLE`, `T06_CLAUSEMAP_COVERAGE_RULE`, `T07_MULTI_CATEGORY_ENTRIES` y
    `T09_NO_UNAUTHORIZED_CHANGES`: los temas conformes desde la ronda 1 (EFF y EFF^1 en la validación de mapas de B1; MAP_INVALID frente a entrada
    inexistente y cierre no construible; la regla de cobertura de `clausemap`; las entradas de varias categorías; la ausencia de cambios no autorizados).
- **`CompatibilityVerdicts`** (§98, punto 4, línea {{DEC98_P4}}: «Conservar A-5, A-6, EFF/EFF^1, oráculos y líneas Owner intactos»): `A5` y `A6`, cada
  uno con `Status` `COMPATIBLE` (la ronda 4 no contradice ni cambia esa enmienda acordada fuera de lo que A-7 declara; no cita ningún REQUIRED) o
  `INCOMPATIBLE` (cita un REQUIRED), `Rationale`, `ClauseRefs` (al menos una de la enmienda que juzgas) y `FindingIds`. A-5: A5-1, L{{A5_1}}, y A5-2,
  regla 4, L{{A5_2R4}} (A-5 entera en el cierre); lo que declara A-7, §6 (L{{A7_S6_A5}}). A-6: las reglas que A-7 precisa o usa (A6-2, reglas 1 y 3,
  L{{A6_A62_R1}} y L{{A6_A62_R3}}; A6-3, regla 6 (b), L{{A6_A63_R6B}}; A6-5, regla 2, L{{A6_A65_R2}}; A6-7, reglas 1, 5 y 7, L{{A6_A67_R1}}, L{{A6_A67_R5}} y
  L{{A6_A67_R7}}; A6-9, regla 4, L{{A6_A69_R4}}, y su tabla, L{{A6_DERIV}}); lo que declaran A-7, §3.4 (L{{A7_S34}}), §3.5 (residuo de O1, L{{A7_S35_O1}}) y §6
  (L{{A7_S6_A6}}). El preflight de la ronda 4 (`q6-compat.json`) es evidencia de apoyo, no autoridad.
- **`Materiality`** (LIFECYCLE §3; A-7 §5, L{{A7_S5}}): en `Triggers`, una vez cada disparador `M-01`..`M-08`, con `A7Declared` (lo que declara la columna
  «Activado» de A-7 §5: `yes` para **sí**, `no` para no) y `Assessment` (`AGREE` si coincides con esa declaración después de la ronda 4; `DISAGREE` si
  no, y entonces cita un REQUIRED); `Rationale`, `ClauseRefs` y `FindingIds` en cada disparador y en el conjunto.
- **`OwnerReservedConsequence`** (LIFECYCLE §3, OWNER-RESERVED; A-7 §7, L{{A7_S7}}): `OwnerDecisionRequired` (true si identificas una consecuencia
  OWNER-RESERVED que A-7 no identifica o que no está dispuesta; false si no) y `UnidentifiedConsequences` (cada una; vacía con false), con `Rationale`,
  `ClauseRefs` y `FindingIds`. Las líneas del Owner constan (decisiones §78, línea {{DEC78_LINE}}; A-6 §7, L{{A6_LINE_A}} y L{{A6_LINE_B}}): ninguna cuenta aquí.
- **`QuestionAnswers`** (A-7 §8, filas L{{A7_QR4}}): `Q-A7R4-01`..`Q-A7R4-03`, una vez cada una: `Answer` (tu respuesta) y `Outcome` (`SATISFIED` si la
  respuesta no exige ningún cambio de A-7, `NOT_SATISFIED` si lo exige; un NOT_SATISFIED cita al menos un hallazgo), con `ClauseRefs` y `FindingIds`.
- **`DirectReads`:** `{Ref, Purpose}` de cada fuente que leíste directamente para comprobar una cita de A-7. Incluye siempre
  `docs/automation/agent-execution/README.md:{{DIRECT}}` (Paso 0) y, como mínimo recomendado, una entrada por fuente de la lista de A-7 §2 en la que te
  apoyes.
- **Además:**
  - tu modo de revisión, si revisor y autor son la misma persona y el contexto que el runtime te inyectó (`ReviewerMode`, `SamePersonAsAuthor`,
    `InjectedContextDeclaration`). Declara también el proveedor del revisor y del autor en `SamePersonAsAuthor`. Los identificadores de cuenta que el
    runtime te inyecte (correo, uuid de organización u otros) se declaran solo por su tipo, sin reproducir su valor;
  - la identidad revisada (`ReviewedCommit`, `ReviewedPath`, `ReviewedBlob`);
  - lo que leíste (`Reads`: «ruta: rangos») y la acción siguiente que recomiendas (`RecommendedNextAction`).

'''
t = region(t, '## Qué decidir', '## Resultado', DECIDE)

RESULT = '''## Resultado

Entrega el resultado con **una** llamada a StructuredOutput, con este objeto. Lo valida un esquema estricto: no añadas campos.

```json
{
  "Schema": "rackcad-a7r4-architect-review/0",
  "RunId": "{{RUN_ID}}", "InvocationId": "{{INVOCATION_ID}}", "LogicalReviewRequestId": "{{LOGICAL_ID}}", "AttemptSeq": 1,
  "RequestedRole": "ARCHITECT", "Action": "REVIEW_DESIGN", "ReviewedUnit": "I-62",
  "ReviewedCommit": "...", "ReviewedPath": "docs/initiatives/I-62-A-7.md", "ReviewedBlob": "...",
  "ReviewerMode": "SAME-SESSION ROLE | SEPARATE SESSION | EXTERNAL HUMAN",
  "SamePersonAsAuthor": "texto: relación entre el revisor, la sesión autora de A-7 y el operador humano; proveedor del revisor y del autor",
  "ReviewerDeclaredIdentity": {"Runtime": "... o UNKNOWN", "Model": "... o UNKNOWN", "Effort": "... o UNKNOWN", "SessionOrThread": "... o UNKNOWN"},
  "InjectedContextDeclaration": {"State": "DECLARED | NONE", "Items": ["todo lo que el runtime te inyectó antes de tu primera acción; los identificadores de cuenta, solo por su tipo"]},
  "Step0": {"Basis": "INVOKER_VERIFIED_CLONE", "RAE10DirectReadL219L226": "true | false | null", "OrderReadWhole": "true | false | null",
            "Decisions98DeclaresOrderBytesAndSha256": "true | false | null", "Decisions98AcceptsR3R1AndAuthorizesFourthCandidate": "true | false | null",
            "GuardsRunModeBaseHeadResult": "true | false | null", "PreflightSummaryCandidateAndTotals": "true | false | null",
            "Round3OutputIdentifiesRunAndFindings": "true | false | null", "Note": "..."},
  "DirectReads": [{"Ref": "docs/automation/agent-execution/README.md:{{DIRECT}}", "Purpose": "..."}],
  "Reads": ["ruta: rangos"],
  "Verdict": "AGREED | CHANGES REQUIRED | REJECTED | BLOCKED — OWNER DECISION | NOT_ACCREDITED",
  "RequiredFindings": [{"FindingId": "A62-A7R4-R1", "Section": "...", "ViolatedInvariant": "...", "Counterexample": "...", "Evidence": ["ruta:inicio-fin"],
                        "WhyItMatters": "...", "CorrectionRequired": "..."}],
  "OptionalFindings": [{"FindingId": "A62-A7R4-O1", "Section": "...", "Note": "...", "Evidence": ["ruta:línea"], "SuggestedCorrection": "..."}],
  "PriorFindingResolution": [{"PriorFinding": "A62-A7R3-R1", "Status": "CORRECTED | PARTIALLY_CORRECTED | NOT_CORRECTED",
                              "Rationale": "...", "ClauseRefs": [], "FindingIds": []}],
  "DeltaPurity": {"Status": "CONFIRMED | NOT_CONFIRMED | NOT_DETERMINED",
                  "Changes": [{"Change": "R3R1_REBASE_COPY_IN_REVERIFICATION_CONTROLS | … | PROCEDURE_PIECES_ROUND4", "Assessment": "WITHIN_SCOPE | OUTSIDE_SCOPE",
                               "Rationale": "...", "ClauseRefs": [], "FindingIds": []}],
                  "Rationale": "...", "ClauseRefs": [], "FindingIds": []},
  "RegressionCheck": [{"Topic": "R2R1_REVERIFICATION_RECORD_AS_CORRECTED | … | T09_NO_UNAUTHORIZED_CHANGES", "Status": "NO_REGRESSION | REGRESSION",
                       "Rationale": "...", "ClauseRefs": [], "FindingIds": []}],
  "CompatibilityVerdicts": {"A5": {"Status": "COMPATIBLE | INCOMPATIBLE | NOT_DETERMINED", "Rationale": "...", "ClauseRefs": [], "FindingIds": []},
                            "A6": {"Status": "COMPATIBLE | INCOMPATIBLE | NOT_DETERMINED", "Rationale": "...", "ClauseRefs": [], "FindingIds": []}},
  "Materiality": {"Triggers": [{"Trigger": "M-01 | … | M-08", "A7Declared": "yes | no", "Assessment": "AGREE | DISAGREE", "Rationale": "...",
                                "ClauseRefs": [], "FindingIds": []}],
                  "Rationale": "...", "ClauseRefs": [], "FindingIds": []},
  "OwnerReservedConsequence": {"OwnerDecisionRequired": "true | false | null", "UnidentifiedConsequences": [], "Rationale": "...", "ClauseRefs": [],
                               "FindingIds": []},
  "QuestionAnswers": [{"Question": "Q-A7R4-01 | Q-A7R4-02 | Q-A7R4-03", "Outcome": "SATISFIED | NOT_SATISFIED", "Answer": "...", "ClauseRefs": [],
                       "FindingIds": []}],
  "IfAgreed": {"ScopeLimitedToR3R1AndConsequences": null, "PriorFindingCorrected": null, "DeltaLimitedToR3R1": null,
               "NoRegressionOfPriorConformities": null, "CompatibleWithA5AndA6": null, "MaterialityConfirmed": null,
               "NoUnidentifiedOwnerReservedConsequence": null, "NoUnauthorizedChanges": null, "SuitableForCoordinatorAgreement": null},
  "KnownLimitations": ["..."],
  "RecommendedNextAction": "..."
}
```

- Los valores de la plantilla son formas, no respuestas: cada campo lleva **tu** comprobación y **tu** evaluación. Donde la plantilla dice
  `"true | false | null"`, el valor es un booleano JSON (o `null`), no un texto.
- `Step0`: las siete booleanas son lo que compruebas en el Paso 0 (la primera, la lectura directa de RAE §10 L{{DIRECT}}); `null` en lo que no
  compruebes.
- `DirectReads` incluye siempre RAE §10 L{{DIRECT}}, también si te detienes en el Paso 0.
- Cobertura, una vez cada elemento: `PriorFindingResolution`, A62-A7R3-R1; `DeltaPurity.Changes`, los once cambios; `RegressionCheck`, los doce temas;
  `CompatibilityVerdicts`, A5 y A6; `Materiality.Triggers`, M-01..M-08; `QuestionAnswers`, Q-A7R4-01..03.
- Todo id de `FindingIds` existe en `RequiredFindings` u `OptionalFindings`; los ids no se repiten.
- **Con un veredicto** (todo salvo NOT_ACCREDITED):
  - las siete booleanas de `Step0` en `true`; `ReviewedCommit`, `ReviewedPath` y `ReviewedBlob` = el commit, la ruta y el blob de arriba;
  - `A7Declared` de cada disparador = lo que declara A-7 §5;
  - al menos una `ClauseRef` en cada elemento de `PriorFindingResolution`, `DeltaPurity.Changes`, `RegressionCheck`, `Materiality.Triggers` y
    `QuestionAnswers`, y en `DeltaPurity`, `CompatibilityVerdicts.A5`, `CompatibilityVerdicts.A6`, `Materiality` y `OwnerReservedConsequence`;
  - ningún `NOT_DETERMINED` (`DeltaPurity.Status`, `CompatibilityVerdicts.A5.Status`, `CompatibilityVerdicts.A6.Status`) y ningún `null` en
    `OwnerDecisionRequired`;
  - todo REQUIRED citado en `FindingIds` por al menos un elemento evaluado.
- **Con AGREED:** cero REQUIRED; A62-A7R3-R1 en CORRECTED; `DeltaPurity` CONFIRMED con los once cambios `WITHIN_SCOPE`; los doce temas en
  NO_REGRESSION; A-5 y A-6 en COMPATIBLE; los ocho disparadores en AGREE; `OwnerDecisionRequired` = false; y los nueve campos de `IfAgreed` en `true`. En
  cualquier otro caso, `IfAgreed` va todo en `null`.
- **Con CHANGES REQUIRED o REJECTED:** al menos un REQUIRED. **Con BLOCKED — OWNER DECISION:** `OwnerDecisionRequired` = true. Con
  `OwnerDecisionRequired` = true, `UnidentifiedConsequences` no vacía.
- **Sin acreditación posible** (identidad, fidelidad o insumos): no fabriques un veredicto. Pon `Verdict` = `"NOT_ACCREDITED"` y el motivo exacto en
  `KnownLimitations`.
  - `PriorFindingResolution`, `DeltaPurity.Changes`, `RegressionCheck`, `Materiality.Triggers`, `QuestionAnswers` y las listas de hallazgos pueden ir
    vacías.
  - Lo no evaluado va así: `NOT_DETERMINED` en `DeltaPurity.Status` y en el `Status` de `CompatibilityVerdicts.A5` y `.A6`; `null` en
    `OwnerDecisionRequired`, en `IfAgreed` y en las booleanas de `Step0` que no comprobaste; `ReviewedCommit` y `ReviewedBlob` con lo observado o en
    `null`; los `Rationale`, «no evaluado».
  - `NOT_DETERMINED` y `null` en esos campos solo valen con NOT_ACCREDITED.
'''
a = t.index('## Resultado')
t = t[:a] + RESULT
# ---- ediciones puntuales en lo que se conserva
R = [
    ('sus rondas 2 y 3 y sus preflights', 'sus rondas 2 a 4 y sus preflights', 1),
    ('''el preflight de la
ronda 1 (`docs/automation/evidence/I-62-A7/` fuera de `r2/` y `r3/`) y las disposiciones propuestas de O1-O8 (`r2-dispositions/`); la revisión de la
ronda 1;''', '''los preflights de
las rondas 1 y 2 (`docs/automation/evidence/I-62-A7/` fuera de `r3/` y `r4/`) y las disposiciones propuestas de O1-O8 (`r2-dispositions/`); las
revisiones de las rondas 1 y 2;''', 1),
    ('L21 en el esquema `relay-record/v2`, cuyas líneas son JSON)', 'L21 en el esquema `relay-record/v2`, cuyas líneas son JSON; L24 en los contratos del Controller de FX-02)', 1),
    ('En la ronda 3, una sola referencia a `docs/initiatives/I-62-A7.md`', 'En la ronda 2, una sola referencia a `docs/initiatives/I-62-A7.md`', 1),
]
for i, (old, new, count) in enumerate(R):
    n = t.count(old)
    if n != count:
        raise SystemExit('edición %d: %d apariciones (se esperaban %d): %r' % (i, n, count, old[:90]))
    t = t.replace(old, new)
b = t.encode('utf-8')
assert b'\r' not in b
open(os.path.join(KIT, 'prompt.template.md'), 'wb').write(b)
print('prompt.template.md', hashlib.sha256(b).hexdigest(), len(b), 'bytes')
