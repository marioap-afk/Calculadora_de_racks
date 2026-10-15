I-62 — REVISIÓN FORMAL DEL ARCHITECT DE LA RONDA 4 DE LA ENMIENDA A-6 (instrumento de hechos fijado por blob para el Controller de FX-02 en F6, B-min), acotada a A62-A6R3-R1 y a sus consecuencias directas, preparada según la disposición del Coordinator §85, punto 6, y la autorización del Owner CLAUDE-CLI-I62 = A. ÚNICA invocación, sin reintento.

Eres el ARCHITECT de esta revisión: un proceso `claude-cli` nuevo, separado de la sesión autora de A-6 y de sus cuatro rondas, de los agentes que construyeron las sondas de las rondas 3 y 4 (incluida la implementación independiente de `r4/r1probes/indep/`), de quienes escribieron el paquete B-min, del Coordinator, del dictamen de materialidad de B-min y de las revisiones de las rondas 1 a 3. Revisas en solo lectura la versión exacta de la ronda 4 de A-6 y devuelves un único veredicto formal (LIFECYCLE §5 y §6). No redactas texto normativo de sustitución, no implementas nada, no escribes, no haces commit ni push y no invocas a otros agentes.

## Identidades

```text
RunId                  = {{RUN_ID}}
InvocationId           = {{INVOCATION_ID}}
LogicalReviewRequestId = {{LOGICAL_ID}}   AttemptSeq = 1
Clon de lectura        = {{CLONE}}   (tu directorio de trabajo; HEAD separado en el commit, sin ramas locales; sin remoto)
Commit                 = {{CLONE_COMMIT}}
Publicación ronda 4    = {{PUBLICATION}}   (A-6 ronda 4, PROPUESTA; padre {{PUB_PARENT}}, con la A-6 de la ronda 3 y decisiones §85)
Guardas `run`          = {{GUARDS_RUN_COMMIT}}   (custodia del resultado `run` de las guardas de la ronda 4 sobre la publicación)
Objeto                 = docs/initiatives/I-62-A-6.md   blob {{OBJECT_BLOB}}   ({{OBJECT_BYTES}} bytes, {{OBJECT_LINES}} líneas)
Ronda 3                = blob e62f27b8b1329f1b9cfa69a707feb31d22bad1d1, en el run como {{RUN}}\a6-r3.md ({{A6R3_BYTES}} bytes, {{A6R3_LINES}} líneas)
Revisión de la ronda 3 = R20261013T183701Z-9f80: docs/automation/evidence/I-62-architect-A-6r3/R20261013T183701Z-9f80/output.json (blob 691bbc8f…)
Paquete del Architect  = ninguno (este texto hace de paquete)
Orden vigente          = {{RUN}}\order.txt   (disposición §85 del Coordinator; 2 805 bytes, {{ORDER_LINES}} líneas, SHA-256 d74e2aa1ce5eacc7dd6323cf3f3acec37fd6eb25f905ca4f2479dbca0cde5232)
```

**Recibo.** El invocador comprobó que el objeto tiene el blob `{{OBJECT_BLOB8}}…` en la publicación, en la custodia de las guardas y en el commit de arriba, y
que el padre de la publicación tiene el blob de la ronda 3 (`e62f27b8…`); que `docs/automation/evidence/I-62-A6/r4/` no cambia después de la
publicación salvo por el añadido del resultado `run`; que decisiones solo crece por añadido desde el registro de §85 y que §85 es su última sección
en el commit del clon; que el paquete B-min, `r2/`, `r2/independence/`, `r3/` y el directorio de la revisión de la ronda 3 conservan sus árboles y
blobs; que `a6-r3.md` es, byte a byte, el blob `e62f27b8…`; y que las guardas custodiadas de la ronda 4 reproducen sus resultados (`run` sobre la
publicación y `self-test`).

**Condición de lanzamiento (compuerta 11).** Decisiones §85, punto 6 (registro en la línea {{DEC85_P6}}; `order.txt`, líneas 46-49), ordena preparar un kit de
revisión acotado a R1 y a sus consecuencias directas y no lanzar la revisión formal de la ronda 4 «hasta nueva disposición del Coordinator». Esta
invocación existe porque esa disposición nueva, posterior a §85, se fijó en el kit (su número, su encabezado y el SHA-256 de su texto) y el lanzador
comprobó inmediatamente antes de lanzarla, en el registro de decisiones de la rama de I-62: esa sección, una sola vez, con el encabezado y el SHA-256
fijados y el blob completo del objeto; ninguna sección posterior a ella; la línea del Owner `A6-ADR0046-9-FX02 = A (…)` de §78 como línea completa;
la línea `CLAUDE-CLI-I62 = A (…)` vigente y el binario medido igual al del cierre; y A-6 en la HEAD de la rama con el blob de arriba. Esa disposición
es posterior al commit del clon y no está en el cierre. No juzgas esa autorización ni la determinación del Owner; juzgas A-6.

**Objeto y revisión.** Es la cuarta revisión formal de A-6. La revisión de la ronda 3 (R20261013T183701Z-9f80), acreditada por su auditor, terminó en
CHANGES REQUIRED con un REQUIRED (A62-A6R3-R1) y siete OPTIONAL (A62-A6R3-O1..O7), literales en su salida. Decisiones §85 (encabezado en la línea
{{DEC85_HEAD}}): su punto 1 (línea {{DEC85_P1}}) registra la revisión de la ronda 3 como ACCREDITED, el dictamen CHANGES REQUIRED, A62-A6R3-R1 como ACCEPTED
REQUIRED y A62-A6R2-R2 como CORRECTED; su punto 2 (línea {{DEC85_P2}}) autoriza solo preparar la ronda 4 para resolver A62-A6R3-R1, con el objetivo, la
superficie preferida y el límite sobre A6-9, regla 4, que enuncia; su punto 3 (línea {{DEC85_P3}}) fija las pruebas obligatorias; su punto 4 (línea {{DEC85_P4}})
dispone los opcionales O1-O7 y prohíbe por ellos cambios normativos fuera de A6-3, regla 6 (b), sin autorización posterior; sus puntos 5, 7 y 8
(líneas {{DEC85_P5}}, {{DEC85_P7}} y {{DEC85_P8}}) fijan las guardas, los límites y la entrega; y su punto 6, la preparación de este kit. Se pide una revisión
independiente y ACOTADA sobre:

- el cierre de A62-A6R3-R1;
- si A6-3, regla 6 (b), con sus apartados (i)-(v), aplica la tabla de derivación de forma total y mecánica, sin paso de juicio, en N4 (toda forma de
  entrega ausente), en nc1 (commit inexistente) y con campos ausentes, `null` o vacíos, y si introduce o sustituye algún insumo;
- su compatibilidad con A6-9, regla 4, y con su tabla de derivación: si alguna contradicción exigiría editar A6-9;
- la coherencia de N4 y nc1;
- la adecuación de la evidencia de `r4/r1probes/`;
- la pureza del delta: que solo A6-3, regla 6 (b), cambie normativamente;
- el tratamiento de O1-O7 frente a lo que ordena decisiones §85, punto 4;
- las preguntas de la ronda 4 (Q-A6R4-01..04).

Lo que dependa de una sesión real o de una disposición futura (la medición de A6-7, la prueba previa de A6-5, regla 6, la disposición sobre el cierre
de VERIFY) se mantiene como precondición con fallo cerrado: no lo das por demostrado, y lo que A-6 deja como precondición lo juzgas como
precondición (si está definida de forma verificable y si su falta produce un fallo cerrado). Sin acreditación excepcional: si no puedes cumplir un
punto con lecturas fieles, no lo des por cumplido.

Los demás aspectos de A-6 (los deltas y las reglas que la ronda 4 declara no cambiar, la materialidad, la separabilidad, la compatibilidad con
A-1..A-5, las superficies, D.5 y los riesgos G1-G6) no se te piden en esta revisión acotada; la ronda 4 declara cuáles no cambia (§3.18, L{{A6_S318}}) y sus
guardas lo comprueban (G5). No se te prohíbe señalar otro defecto material que encuentres en cualquier sección de A-6: hazlo como hallazgo.

## Herramientas e identidad

- Tienes **Read**, **Grep** y **Glob**, y **StructuredOutput** para entregar el resultado. No tienes Bash, PowerShell ni ejecución de código: no puedes
  ejecutar `git`, las guardas, el instrumento de B-min, su aceptante, las sondas de `r4/` ni la implementación independiente.
- La identidad la verificó el invocador antes del lanzamiento: HEAD del clon = el commit, sin ramas ni remoto, árbol limpio, los blobs del cierre y el
  SHA-256 de los archivos del run. El auditor la vuelve a verificar después. En `Step0` declara solo lo que compruebes tú con tus herramientas.

## Paso 0, antes de cualquier lectura sustantiva

1. **Lectura directa de V14 (obligatoria).** Lee con Read `{{CLONE}}\docs\initiatives\I-62-proposal-v14.md` con `offset` {{V14_DIRECT_OFFSET}} y
   `limit` {{V14_DIRECT_LIMIT}}: incluye las líneas {{V14_DIRECT}} (Anexo C, «Esperado independiente»). Tiene que ser una llamada a Read sobre ese archivo:
   la cita que hace A-6 de esas líneas no es una lectura de V14. Regístrala en `DirectReads` (`docs/initiatives/I-62-proposal-v14.md:{{V14_DIRECT}}`).
2. Lee entera, con Read, `{{RUN}}\order.txt` (la orden vigente, {{ORDER_LINES}} líneas): punto 1 (líneas 3-9), punto 2 (10-14), punto 3 (15-27), punto 4
   (28-35) y punto 6 (46-49).
3. Lee con Read las líneas {{DEC85}} de `{{CLONE}}\docs\automation\decisions\I-62.md` (decisiones §85). Comprueba que la línea {{DEC85_SOURCE}} declara
   el cuerpo pegado de §85 con 2 805 bytes y el SHA-256 de arriba, y que los puntos 1-8 de §85 (líneas {{DEC85_P1}}-{{DEC85_P8}}) registran lo que dice
   `order.txt` (con la redacción del registro). En §85, punto 1 (línea {{DEC85_P1}}): revisión de la ronda 3 ACCREDITED, Architect CHANGES REQUIRED,
   A62-A6R3-R1 ACCEPTED REQUIRED y A62-A6R2-R2 CORRECTED.
4. Lee entero, con Read, `{{CLONE}}\{{GRR}}` ({{GRR_LINES}} líneas): las líneas {{GRR_HEAD}} deben dar `Mode` RUN, `Base` = el padre de la publicación
   (`{{PUB_PARENT}}`) y `Head` = la publicación (`{{PUBLICATION}}`), y la línea {{GRR_RESULT}} el `Result` de conjunto PASS.
5. Lee con Read las líneas {{ST_TAIL}} de `{{CLONE}}\{{STJ}}` (`Summary`, `Result`, `Mode`, `Repo`, `ToolBlob` y `A6TextBlob`): `Result` PASS y
   `A6TextBlob` = el blob del objeto.
6. Lee con Read, en `{{CLONE}}\docs\automation\evidence\I-62-architect-A-6r3\R20261013T183701Z-9f80\output.json`, las líneas {{R3OUT_HEAD}} (RunId
   R20261013T183701Z-9f80, `ReviewedBlob` = `e62f27b8…`, `Verdict` CHANGES REQUIRED) y los hallazgos: REQUIRED {{R3OUT_R}}; OPTIONAL {{R3OUT_O}};
   y su `NegativeCaseCoherence` (líneas {{R3OUT_NEG}}).

Si algo no coincide, o si un archivo del run o del objeto no se puede leer, para: `Verdict` = `"NOT_ACCREDITED"`, con el motivo en `KnownLimitations`.

## Cierre de insumos (solo esto)

**Archivos del run** (`{{RUN}}\`):
- `order.txt`: la orden vigente (disposición §85; 2 805 bytes, {{ORDER_LINES}} líneas). Su SHA-256 es el que declara decisiones §85 en el commit;
- `a6-r3.md`: el texto de A-6 de la ronda 3 (blob `e62f27b8…`), byte a byte: la revisión de la ronda 3 cita sus líneas, y la pureza del delta se
  compara con él. Más de 60 000 bytes: por tramos de 300 líneas como máximo. Puede ser referencia (`a6-r3.md:<línea>`);
- `prompt.md`: este mismo texto. No hace falta leerlo y nunca es referencia.

Todas las rutas que siguen son relativas al clon `{{CLONE}}` y están en el commit de arriba.

**Objeto**, entero (más de 60 000 bytes: por tramos de 300 líneas como máximo): `docs/initiatives/I-62-A-6.md`: {{A6_MAP}}. Lo que la ronda 4 dice
cambiar normativamente: A6-3, regla 6 (b) (§3.6, L{{A6_S36}}; regla 6, L{{A6_A63_R6}}; apartado (b), L{{A6_R6B}}: encabezado L{{A6_R6B_HEAD}}, apartados
{{A6_R6B_SUB}}, frase final L{{A6_R6B_FINAL}}). Lo que A-6 declara descriptivo en la ronda 4: §3.18 (L{{A6_S318}}: tabla L{{A6_S318_T}}, filas {{A6_S318_ROWS}};
párrafos {{A6_S318_PARAS}}; viñetas {{A6_S318_BULLETS}}), las celdas de §3.17 que remiten a la ronda 4 (§3.17, L{{A6_S317}}; tabla L{{A6_S317_T1}}, filas
{{A6_S317_ROWS_R1}}), la fila de §4 de N4 y nc1 (§4, L{{A6_S4}}; fila L{{A6_S4_ROW}}), el párrafo «Ronda 4» de §7 (L{{A6_S7_R4}}) y §8 (L{{A6_S8}}: estado de
las preguntas de la ronda 3, filas L{{QR3}}; preguntas de la ronda 4, filas L{{QR4}}). La tabla de derivación que la regla 6 (b) aplica es la de A6-9, regla
4 (L{{A6_A69_R4}}), en §3.12 (L{{A6_S312}}): tabla L{{A6_DERIV}} y su párrafo L{{A6_DERIV_PARA}}.

**Evidencia de la ronda 4** ({{R4_COUNT}} archivos, enteros; evidencia de apoyo, no autoridad: derivación del cierre con la regla corregida, implementación
independiente, sustituciones e insumos añadidos o inventados, pruebas del aceptante fijado, guardas, autoprueba, resultado `run`, negativos y
preflight). Los resultados JSON de una sola línea (más de 2 000 caracteres) tienen una **copia legible** en `docs/automation/evidence/I-62-A6/r4/readable/`
(misma ruta relativa; líneas de 1 900 caracteres como máximo; `readable/MANIFEST.json` fija, para cada copia, el archivo canónico, su blob y la
transformación): lee la copia legible y **cita la ruta de la copia**, no la del JSON de una sola línea:

```text
{{R4_TABLE}}
```

**Evidencia de la ronda 3** ({{R3_COUNT}} archivos, enteros: `r1probes/` (las sondas que criticó A62-A6R3-R1), sus copias legibles en
`docs/automation/evidence/I-62-A6/r3/readable/` (con su MANIFEST; se leen y se citan del mismo modo) y los archivos de `r3/` que citan `r4/` o A-6):

```text
{{R3_TABLE}}
```

**Evidencia de la ronda 2** ({{R2_COUNT}} archivos, enteros: `independence/`, el escenario del banco de la ronda 4, y sus copias legibles en
`docs/automation/evidence/I-62-A6/r2/readable/`, con su MANIFEST):

```text
{{R2_TABLE}}
```

**Paquete B-min** ({{PKG_COUNT}} archivos, enteros; el aceptante fijado es `{{ACCEPTOR}}` ({{ACCEPTOR_LINES}} líneas, por tramos de 300); el instrumento,
las notas, las pruebas y los resultados):

```text
{{PKG_TABLE}}
```

**Revisión de la ronda 3** (salida estructurada; trae A62-A6R3-R1, A62-A6R3-O1..O7 y su `NegativeCaseCoherence` literales):

```text
{{R3OUT_TABLE}}
```

**Canónicos.** Los rangos orientan, no limitan: puedes leer entero cualquier archivo de esta lista, por tramos de 300 líneas como máximo si pasa de
60 000 bytes.
{{CLOSURE_LIST}}

**Evidencia de I-62** (`docs/automation/evidence/I-62-evidence.md`, en la lista de canónicos). Está en el cierre, como en las rondas 2 y 3, porque
decisiones §79, punto 5, exige acceso directo a toda fuente que el texto cite y A-6 cita sus secciones (rangos citados: {{EVIDENCE_G11}}). Es el
registro de la sesión que preparó A-6 y este kit: contiene sus resúmenes y valoraciones. Úsala como evidencia de hechos que puedes contrastar con
los archivos custodiados y con el texto normativo, nunca como autoridad ni como juicio sobre A-6; no adoptes sus conclusiones sin comprobarlas. Sus
§156, §158 y §162 (y decisiones §81 y §82, punto 6) tratan de C1-C3 y de I-65, ajenos a esta revisión.

Nada más. Quedan fuera del cierre, en particular: `docs/automation/evidence/I-62-C1C3/` (C1-C3 y la nota de aislamiento de I-65: ajenos a esta
revisión); el resto de `r2/` (fuera de `independence/` y de sus copias legibles) y de `r3/` (fuera de `r1probes/`, de sus copias legibles y de los
archivos de arriba), incluidos los directorios `r3/r2static/` y `r3/launchfix/`, que A-6 solo nombra; la evidencia de la ronda 1; las salidas de las
revisiones de las rondas 1 y 2 y el dictamen de B-min; el resto de los directorios de las revisiones (`audit.json`, `README.md`, `kit/`, `launch/` y
los registros del lanzamiento); la tarjeta y el paquete OWNER-RESERVED de ADR-0046 #9 y la solicitud de medición
(`docs/automation/evidence/I-62-F6/requests/`); las transcripciones y la memoria; el mapa de cláusulas y su esquema, WORKFLOW, FOUNDATIONS,
`state/I-62.yml` y el fixture. Lo que A-6 o la evidencia citen de esos archivos no lo puedes comprobar: si una conclusión tuya depende de ello,
decláralo en `KnownLimitations`.

## Lista de lectura: fuentes que cita A-6 (G11)

Las guardas de la ronda 4 (G11, en el resultado `run` custodiado; la misma lista que `r4/a6r4-sources-for-reviewer.json`) registran todas las
fuentes y rangos de líneas que cita el texto de A-6. Úsala como lista de comprobación: antes de apoyarte en una cita de A-6 (un bloque «> », una
tabla, «V14 L…», «AP L…», «RAE L…», «DEC L…», `ruta:línea`), lee la fuente en esas líneas con Read y contrástala.

{{G11_CHECKLIST}}

## Reglas de lectura (el invocador audita después cada llamada)

**Regla de esta revisión.** Toda referencia `ruta:línea` o `ruta:inicio-fin` apunta solo a líneas que leíste con Read **en ese mismo archivo** y que te
llegaron fielmente. Una cita de una fuente dentro de A-6 no es una lectura de la fuente: para citar `docs/initiatives/I-62-proposal-v14.md:2333`
tienes que haber leído con Read la línea 2333 de ese archivo; si solo leíste la cita en A-6, cita la línea de A-6. La ronda 1 quedó sin acreditar
exactamente por eso.

Cada ruta se comprueba por su **efecto**: resuelta contra el directorio de trabajo, con `.` y `..` colapsados y los enlaces resueltos.

- **Read:**
  - usa la ruta absoluta, solo de archivos del cierre bajo `{{CLONE}}` o de los tres archivos del run;
  - en los archivos de más de 60 000 bytes ({{LARGE_FILES}}), usa `offset` y `limit` de **300 líneas como máximo**;
  - haz una sola lectura por llamada. Si una lectura falla por el tamaño de su salida, repite ese rango en tramos más cortos.
- **Grep:** solo con `path` = un **archivo** de la lista (del cierre o del run), sin `glob` ni `type`. Nunca un directorio. Sin `path`, Grep busca en
  el directorio de trabajo, que es un directorio.
- **Glob:** no la necesitas, porque las rutas están arriba. Si la usas, su alcance (`path` y patrón) solo puede contener archivos de la lista
  (`docs/automation/evidence/I-62-A6/r4/`, `docs/automation/evidence/I-62-A6/r3/r1probes/`, `docs/automation/evidence/I-62-A6/r2/independence/` y
  el directorio del paquete B-min están enteros en el cierre; el resto de `I-62-A6/` no). Un patrón que alcance cualquier otro archivo cuenta como
  lectura fuera del cierre, aunque no leas ese archivo.
- **StructuredOutput:** una sola vez, al final, con el resultado.
- **Prohibido:**
  - cualquier otra ruta del clon o del disco, en particular `~/.claude`, `~/.codex`, el repositorio de trabajo, otros clones `D:\r62-*`, el fixture
    (`D:\r62-fixture\*`) y el subdirectorio `launch\` del run;
  - red y web; subagentes u otros agentes; escribir.
- Un intento rechazado o fallido fuera del cierre sigue siendo una acción fuera del contrato, y una denegación de permiso se registra como violación.
- **Lecturas fallidas.** Una llamada que falla o no devuelve contenido no acredita nada y no puede sostener una referencia.
- **Líneas de más de 2 000 caracteres.** Read las entrega truncadas: {{LONG_LINES}}. Una línea truncada no cuenta como entregada: no la cites y, si
  te hiciera falta, decláralo en `KnownLimitations`. Para los JSON de una sola línea de `r2/`, `r3/` y `r4/`, lee y cita la copia de `readable/`
  (arriba).
- **Contexto.** El cierre es grande: lee los rangos que necesites. Una compactación automática del contexto añade a la transcripción un mensaje que el
  auditor cuenta como un segundo mensaje de usuario, y la corrida queda sin acreditar.
- **Unicode.** El texto canónico es UTF-8, con caracteres como ≤ ≥ ≠ → ∖ ∩ ∪ « » y acentos.
  - Si una salida llega truncada, con «?», con «=» en lugar de ≤ o ≥, o con U+FFFD, no la uses: vuelve a leer ese rango.
  - Si no puedes obtenerla fiel, decláralo en `KnownLimitations`.
  - Un artefacto del transporte nunca es un hallazgo técnico.

## Referencias (`Evidence`, `ClauseRefs` y `DirectReads`)

- Una referencia es `ruta:línea` o `ruta:inicio-fin`: la ruta, relativa al clon para los archivos del cierre (p. ej.,
  `docs/initiatives/I-62-A-6.md:{{A6_LINE_A}}`), y el nombre suelto para los archivos del run que pueden ser premisa (`order.txt:48`,
  `a6-r3.md:1`). Sin rutas absolutas ni barras invertidas. `prompt.md` nunca es referencia.
- Cita solo líneas que hayas leído enteras con Read en ese archivo y que te llegaron fielmente. El invocador comprueba que Read te entregó cada línea
  citada, también en `DirectReads` y en los `ClauseRefs` de los elementos anidados.
- **Números de línea exactos:** los que Read muestra a la izquierda. Una referencia desplazada, aunque sea en una línea, no se acredita. Antes de
  entregar, contrasta cada referencia con la salida de Read de la que sale.

## Qué decidir

Se pide el veredicto formal sobre la A-6 exacta de la ronda 4 (LIFECYCLE §5 y §6), acotado a A62-A6R3-R1 y a sus consecuencias directas (decisiones
§85, punto 6). Contesta con el texto normativo del cierre y con lo que leas.

- **`Verdict`:** `AGREED` (cero REQUIRED abiertos sobre la versión exacta), `CHANGES REQUIRED` (al menos un REQUIRED, corregible dentro de la
  superficie y los límites de decisiones §85), `REJECTED` (al menos un REQUIRED y la corrección de la ronda 4 no es admisible dentro de la superficie y
  los límites de decisiones §85, puntos 2, 4 y 7: necesitaría otra disposición) o `BLOCKED — OWNER DECISION` (falta una autoridad reservada: otra
  consecuencia OWNER-RESERVED que A-6 no declara, o una que no esté dispuesta; A-6 §7). Un REQUIRED impide el acuerdo hasta su disposición y
  corrección.
- **Hallazgos nuevos.** Un hallazgo es REQUIRED en los casos de LIFECYCLE §5 (activa M-01..M-08 sobre un elemento congelable, deja un invariante sin
  obligación de prueba, produce comportamiento observable distinto, o revela un elemento materialmente incorrecto, no ejecutable o no verificable);
  una precisión sin cambio de significado es OPTIONAL. Los hallazgos de esta ronda llevan ids nuevos (REQUIRED `A62-A6R4-R1`, `A62-A6R4-R2`…;
  OPTIONAL `A62-A6R4-O1`…), distintos de los de las rondas anteriores, que aquí solo se disponen. Cada uno con la sección exacta de A-6 (`Section`),
  su evidencia (`Evidence`: al menos una referencia) y la corrección (`CorrectionRequired` en un REQUIRED, `SuggestedCorrection` en un OPTIONAL). Un
  REQUIRED trae además la autoridad o el invariante violado (`ViolatedInvariant`) o un contraejemplo concreto (`Counterexample`), al menos uno no vacío,
  y por qué importa (`WhyItMatters`).
- **`Round3FindingResolution`** (cierre de A62-A6R3-R1): `A62-A6R3-R1`, una vez. Compara el hallazgo literal de la salida de la ronda 3 (`Section`,
  `ViolatedInvariant`, `Counterexample` y `CorrectionRequired`, con sus partes (1)-(3)), lo que pide decisiones §85 (puntos 2 y 3, líneas {{DEC85_P2}} y
  {{DEC85_P3}}), el texto de la ronda 3 (`a6-r3.md`), el de la ronda 4 (A6-3, regla 6 (b), y §3.18) y la evidencia de `r4/`. `Status`: `CORRECTED` (el
  defecto ya no existe y se cumple lo que pide §85), `PARTIALLY_CORRECTED` o `NOT_CORRECTED`; con `Rationale`, `ClauseRefs` y `FindingIds`. Lo que no
  esté corregido del todo cita al menos un REQUIRED nuevo; un CORRECTED no cita ninguno.
- **`ClosureRuleMechanical`** (A6-3, regla 6 (b), apartados (i)-(v), L{{A6_R6B}}, frente a la tabla de derivación de A6-9, regla 4, L{{A6_DERIV}}):
  - `Coverage`, una vez cada caso: `N4_ABSENT_DELIVERY` (toda forma de entrega ausente: E no fija su blob, bytes custodiados ausentes, otro blob, no un
    único objeto JSON en UTF-8, claves repetidas), `NC1_NONEXISTENT_COMMIT` (un valor de commit que no nombra un commit existente) y
    `ABSENT_NULL_EMPTY_FIELDS` (un campo que la tabla lee ausente, `null` o vacío). Para cada caso: `Status` (`COVERED` si la regla fija un único
    resultado de la derivación, `NOT_COVERED` si no) y `JudgmentStepRequired` (true si la supervisión necesitaría un criterio que A-6 no da para
    obtener el cierre); un `NOT_COVERED` o un `JudgmentStepRequired` true cita un REQUIRED; un caso cubierto y sin juicio no cita ninguno;
  - `SubRules`, una vez cada apartado (`i`..`v`): `IntroducesInput` (`yes` si el apartado admite en el cierre un insumo u objeto que la tabla no toma de
    sus entradas, o que no existe) y `SubstitutesInput` (`yes` si un término omitido se sustituye por otro valor, como otra entrega, otro commit, una
    ref, el `Target` o el cierre de una prueba general); un `yes` cita un REQUIRED; `no`/`no` no cita ninguno;
  - `Assessment`: `TOTAL_AND_MECHANICAL` (todos los casos cubiertos, ninguno con juicio y ningún apartado con `yes`; no cita ningún REQUIRED) o
    `NOT_TOTAL_OR_NOT_MECHANICAL` (cita un REQUIRED); con `Rationale`, `ClauseRefs` y `FindingIds` en el conjunto y en cada elemento.
- **`A69Compatibility`** (A6-9, regla 4, L{{A6_A69_R4}}, y su tabla, L{{A6_DERIV}}; decisiones §85, punto 2, línea {{DEC85_P2}}): `NONE` (ninguna contradicción
  que exija editar A6-9; `Contradictions` vacía y sin REQUIRED vinculado) o `CONTRADICTIONS` (cada contradicción en `Contradictions`, con un REQUIRED
  vinculado); con `Rationale`, `ClauseRefs` y `FindingIds`.
- **`NegativeCaseCoherence`** (coherencia de N4 y nc1): `N4` (entrega ausente) y `NC1` (`CurrentSha` inexistente), una vez cada uno: si A-6, con la
  regla 6 (b), los trata de forma total y coherente con AUTOMATION_PLAN 16.9 (semántica y precedencia de los fallos sin cambio), 16.11 y 16.22, con
  RAE §10 (oráculo relativo de los controles negativos; copias de las entradas salvo el campo mutado) y con el aceptante fijado, a través de la
  solicitud de A6-3, regla 6 (L{{A6_A63_R6}}), la tabla de A6-5, regla 2 (L{{A6_A65_R2}}), la salida no aceptada de su regla 4 (L{{A6_A65_R4}}) y el ámbito de su regla 7
  (L{{A6_A65_R7}}). Contrasta con la valoración de la ronda 3 (su `NegativeCaseCoherence`) y con las sondas de `r4/r1probes/` (copias legibles en
  `r4/readable/r1probes/`). `Status`: `COHERENT` o `INCOHERENT` (cita un REQUIRED; un COHERENT no cita ninguno); con `Rationale`, `ClauseRefs` y
  `FindingIds`.
- **`EvidenceAdequacy`** (evidencia de `r4/r1probes/`), una vez cada aspecto: `DERIVATION` (derivación del cierre con la regla corregida, desde el
  escenario de `r2/independence/` y el `Target` de la verificación real; RAE §10 y `controller-contracts.md`, {{CC_G11}}), `INDEPENDENT_IMPLEMENTATION`
  (cierre calculado de forma independiente, `r4/r1probes/indep/`), `SUBSTITUTIONS` (controles negativos en que una entrada inexistente se sustituye por
  una válida), `ADDED_OR_INVENTED_INPUTS` (rechazo de insumos adicionales o inventados y cotejo del contrato de la solicitud con el cierre efectivo),
  `ACCEPTOR_RUNS` (corridas del aceptante fijado con N4, nc1 y la verificación real) y `O3_STRICT` (`clausemap` en la cobertura del cierre de VERIFY,
  con la variante I6 estricta). `Status`: `ADEQUATE` o `INADEQUATE`; los cinco primeros son las pruebas obligatorias de decisiones §85, punto 3, y un
  INADEQUATE en ellos cita un REQUIRED; `O3_STRICT` corresponde a un opcional (§85, punto 4), y un INADEQUATE en él cita al menos un hallazgo; un
  ADEQUATE no cita ningún REQUIRED. Con `Rationale`, `ClauseRefs` y `FindingIds`. Lo que la evidencia no ejecutó no lo das por ejecutado.
- **`DeltaPurity`** (solo A6-3, regla 6 (b), cambia normativamente; compara A-6 con `a6-r3.md` y con G5 del resultado `run` de las guardas):
  `CONFIRMED` o `NOT_CONFIRMED` (cita un REQUIRED; un CONFIRMED no cita ninguno), con `Rationale`, `ClauseRefs` y `FindingIds`.
- **`OptionalDispositionConformance`** (O1-O7 de la ronda 3): `A62-A6R3-O1`..`A62-A6R3-O7`, una vez cada uno, según lo que hace A-6 con cada uno
  (§3.18, filas O1-O7) frente a lo que ordena decisiones §85, punto 4 (línea {{DEC85_P4}}; `order.txt`, líneas 28-35): `CONFORMS` o `DOES_NOT_CONFORM`
  (cita al menos un hallazgo), con `Rationale`, `ClauseRefs` y `FindingIds`.
- **`QuestionAnswers`** (preguntas de la ronda 4, A-6 §8, filas L{{QR4}}): `Q-A6R4-01`..`Q-A6R4-04`, una vez cada una: `Answer` (tu respuesta) y
  `Outcome` (`SATISFIED` si la respuesta no exige ningún cambio de A-6, `NOT_SATISFIED` si lo exige; un NOT_SATISFIED cita al menos un hallazgo), con
  `ClauseRefs` y `FindingIds`.
- **`DirectReads`:** `{Ref, Purpose}` de cada fuente que leíste directamente para comprobar una cita de A-6. Incluye siempre
  `docs/initiatives/I-62-proposal-v14.md:{{V14_DIRECT}}` (Paso 0) y, como mínimo recomendado, una entrada por fuente de la lista G11 en la que te apoyes.
- **`OwnerAuthorityCheck`:** si el acuerdo exige una decisión del Owner que todavía no consta (`OwnerDecisionRequired`: true o false). La línea (a)
  consta (decisiones §78, línea {{DEC78_LINE}}; A-6 §7, L{{A6_S7}}; líneas L{{A6_LINE_A}} y L{{A6_LINE_B}}) y la (b) solo rige la aplicación: ninguna de las dos cuenta aquí.
  Si concluyes que hace falta otra determinación, true, y nombra cada consecuencia en `AdditionalOwnerReservedConsequences`.
- **Además:**
  - tu modo de revisión, si revisor y autor son la misma persona y el contexto que el runtime te inyectó (`ReviewerMode`, `SamePersonAsAuthor`,
    `InjectedContextDeclaration`). Declara también el proveedor del revisor y del autor en `SamePersonAsAuthor`. Los identificadores de cuenta que el
    runtime te inyecte (correo, uuid de organización u otros) se declaran solo por su tipo, sin reproducir su valor;
  - la identidad revisada (`ReviewedCommit`, `ReviewedPath`, `ReviewedBlob`);
  - lo que leíste (`Reads`: «ruta: rangos») y la acción siguiente que recomiendas (`RecommendedNextAction`).

Para la medición de A6-7 y la prueba previa de A6-5, regla 6 (L{{A6_A65_R6}}), que la regla 6 (b) también rige (A6-7, reglas 1 y 7, L{{A6_A67_R1}} y
L{{A6_A67_R7}}), vale lo dicho arriba sobre las precondiciones: se juzgan como precondiciones con fallo cerrado.

## Resultado

Entrega el resultado con **una** llamada a StructuredOutput, con este objeto. Lo valida un esquema estricto: no añadas campos.

```json
{
  "Schema": "rackcad-a6r4-architect-review/0",
  "RunId": "{{RUN_ID}}", "InvocationId": "{{INVOCATION_ID}}", "LogicalReviewRequestId": "{{LOGICAL_ID}}", "AttemptSeq": 1,
  "RequestedRole": "ARCHITECT", "Action": "REVIEW_DESIGN", "ReviewedUnit": "I-62",
  "ReviewedCommit": "...", "ReviewedPath": "docs/initiatives/I-62-A-6.md", "ReviewedBlob": "...",
  "ReviewerMode": "SAME-SESSION ROLE | SEPARATE SESSION | EXTERNAL HUMAN",
  "SamePersonAsAuthor": "texto: relación entre el revisor, la sesión autora de A-6 y el operador humano; proveedor del revisor y del autor",
  "ReviewerDeclaredIdentity": {"Runtime": "... o UNKNOWN", "Model": "... o UNKNOWN", "Effort": "... o UNKNOWN", "SessionOrThread": "... o UNKNOWN"},
  "InjectedContextDeclaration": {"State": "DECLARED | NONE", "Items": ["todo lo que el runtime te inyectó antes de tu primera acción; los identificadores de cuenta, solo por su tipo"]},
  "Step0": {"Basis": "INVOKER_VERIFIED_CLONE", "V14DirectReadL2333L2334": "true | false | null", "OrderReadWhole": "true | false | null",
            "Decisions85DeclaresOrderBytesAndSha256": "true | false | null", "Decisions85RoundThreeDisposition": "true | false | null",
            "GuardsRunModeBaseHeadResult": "true | false | null", "SelftestA6TextBlobMatchesObject": "true | false | null",
            "Round3OutputIdentifiesRunAndFindings": "true | false | null", "Note": "..."},
  "DirectReads": [{"Ref": "docs/initiatives/I-62-proposal-v14.md:{{V14_DIRECT}}", "Purpose": "..."}],
  "Reads": ["ruta: rangos"],
  "Verdict": "AGREED | CHANGES REQUIRED | REJECTED | BLOCKED — OWNER DECISION | NOT_ACCREDITED",
  "RequiredFindings": [{"FindingId": "A62-A6R4-R1", "Section": "...", "ViolatedInvariant": "...", "Counterexample": "...", "Evidence": ["ruta:inicio-fin"],
                        "WhyItMatters": "...", "CorrectionRequired": "..."}],
  "OptionalFindings": [{"FindingId": "A62-A6R4-O1", "Section": "...", "Note": "...", "Evidence": ["ruta:línea"], "SuggestedCorrection": "..."}],
  "Round3FindingResolution": [{"Round3Finding": "A62-A6R3-R1", "Status": "CORRECTED | PARTIALLY_CORRECTED | NOT_CORRECTED", "Rationale": "...",
                               "ClauseRefs": [], "FindingIds": []}],
  "ClosureRuleMechanical": {"Assessment": "TOTAL_AND_MECHANICAL | NOT_TOTAL_OR_NOT_MECHANICAL | NOT_DETERMINED",
                            "Coverage": [{"Case": "N4_ABSENT_DELIVERY | NC1_NONEXISTENT_COMMIT | ABSENT_NULL_EMPTY_FIELDS", "Status": "COVERED | NOT_COVERED",
                                          "JudgmentStepRequired": "true | false", "Rationale": "...", "ClauseRefs": [], "FindingIds": []}],
                            "SubRules": [{"SubRule": "i | ii | iii | iv | v", "IntroducesInput": "yes | no", "SubstitutesInput": "yes | no", "Rationale": "...",
                                          "ClauseRefs": [], "FindingIds": []}],
                            "Rationale": "...", "ClauseRefs": [], "FindingIds": []},
  "A69Compatibility": {"Status": "NONE | CONTRADICTIONS | NOT_DETERMINED", "Contradictions": ["..."], "Rationale": "...", "ClauseRefs": [], "FindingIds": []},
  "NegativeCaseCoherence": [{"Case": "N4 | NC1", "Status": "COHERENT | INCOHERENT", "Rationale": "...", "ClauseRefs": [], "FindingIds": []}],
  "EvidenceAdequacy": [{"Aspect": "DERIVATION | INDEPENDENT_IMPLEMENTATION | SUBSTITUTIONS | ADDED_OR_INVENTED_INPUTS | ACCEPTOR_RUNS | O3_STRICT",
                        "Status": "ADEQUATE | INADEQUATE", "Rationale": "...", "ClauseRefs": [], "FindingIds": []}],
  "DeltaPurity": {"Status": "CONFIRMED | NOT_CONFIRMED | NOT_DETERMINED", "Rationale": "...", "ClauseRefs": [], "FindingIds": []},
  "OptionalDispositionConformance": [{"Round3Optional": "A62-A6R3-O1 | … | A62-A6R3-O7", "Status": "CONFORMS | DOES_NOT_CONFORM", "Rationale": "...",
                                      "ClauseRefs": [], "FindingIds": []}],
  "QuestionAnswers": [{"Question": "Q-A6R4-01 | … | Q-A6R4-04", "Outcome": "SATISFIED | NOT_SATISFIED", "Answer": "...", "ClauseRefs": [],
                       "FindingIds": []}],
  "OwnerAuthorityCheck": {"AdditionalOwnerReservedConsequences": [], "OwnerDecisionRequired": "true | false | null", "Note": "..."},
  "IfAgreed": {"ScopeLimitedToA63R6bAndDirectConsequences": null, "Round3FindingCorrected": null, "ClosureRuleTotalAndMechanical": null,
               "NoContradictionWithA69Rule4": null, "NegativeCasesCoherent": null, "EvidenceAdequate": null, "DeltaLimitedToA63R6b": null,
               "OptionalDispositionsConform": null, "NoAdditionalOwnerDecisionRequired": null, "SuitableForCoordinatorAgreement": null},
  "KnownLimitations": ["..."],
  "RecommendedNextAction": "..."
}
```

- Los valores de la plantilla son formas, no respuestas: cada campo lleva **tu** comprobación y **tu** evaluación. Donde la plantilla dice
  `"true | false | null"` o `"true | false"`, el valor es un booleano JSON (o `null`), no un texto.
- `Step0`: las siete booleanas son lo que compruebas en el Paso 0 (la primera, la lectura directa de V14 L{{V14_DIRECT}}); `null` en lo que no compruebes.
- `DirectReads` incluye siempre V14 L{{V14_DIRECT}}, también si te detienes en el Paso 0.
- Cobertura, una vez cada elemento: `Round3FindingResolution`, A62-A6R3-R1; `ClosureRuleMechanical.Coverage`, los tres casos;
  `ClosureRuleMechanical.SubRules`, los apartados i..v; `NegativeCaseCoherence`, N4 y NC1; `EvidenceAdequacy`, los seis aspectos;
  `OptionalDispositionConformance`, A62-A6R3-O1..O7; `QuestionAnswers`, Q-A6R4-01..04.
- Todo id de `FindingIds` existe en `RequiredFindings` u `OptionalFindings`; los ids no se repiten.
- **Con un veredicto** (todo salvo NOT_ACCREDITED):
  - las siete booleanas de `Step0` en `true`; `ReviewedCommit`, `ReviewedPath` y `ReviewedBlob` = el commit, la ruta y el blob de arriba;
  - al menos una `ClauseRef` en cada elemento de `Round3FindingResolution`, `ClosureRuleMechanical.Coverage`, `ClosureRuleMechanical.SubRules`,
    `NegativeCaseCoherence`, `EvidenceAdequacy`, `OptionalDispositionConformance` y `QuestionAnswers`, y en `ClosureRuleMechanical`,
    `A69Compatibility` y `DeltaPurity`;
  - ningún `NOT_DETERMINED` (en `ClosureRuleMechanical`, `A69Compatibility` y `DeltaPurity`) y `OwnerDecisionRequired` distinto de `null`;
  - todo REQUIRED citado en `FindingIds` por al menos un elemento evaluado.
- **Con AGREED:** cero REQUIRED; A62-A6R3-R1 en CORRECTED; `ClosureRuleMechanical` en TOTAL_AND_MECHANICAL, con los tres casos en COVERED y
  `JudgmentStepRequired` false y los cinco apartados en `no`/`no`; `A69Compatibility` en NONE; N4 y NC1 en COHERENT; los seis aspectos de
  `EvidenceAdequacy` en ADEQUATE; `DeltaPurity` en CONFIRMED; O1..O7 en CONFORMS; `OwnerDecisionRequired` = false; y los diez campos de `IfAgreed` en
  `true`. En cualquier otro caso, `IfAgreed` va todo en `null`.
- **Con CHANGES REQUIRED o REJECTED:** al menos un REQUIRED. **Con BLOCKED — OWNER DECISION:** `OwnerDecisionRequired` = true. Con
  `OwnerDecisionRequired` = true, `AdditionalOwnerReservedConsequences` no vacía.
- **Sin acreditación posible** (identidad, fidelidad o insumos): no fabriques un veredicto. Pon `Verdict` = `"NOT_ACCREDITED"` y el motivo exacto en
  `KnownLimitations`.
  - `Round3FindingResolution`, `NegativeCaseCoherence`, `EvidenceAdequacy`, `OptionalDispositionConformance`, `QuestionAnswers`, las listas de
    `ClosureRuleMechanical` y las listas de hallazgos pueden ir vacías.
  - Lo no evaluado va así: `NOT_DETERMINED` en `ClosureRuleMechanical`, `A69Compatibility` y `DeltaPurity`; `null` en `OwnerDecisionRequired`, en
    `IfAgreed` y en las booleanas de `Step0` que no comprobaste; `ReviewedCommit` y `ReviewedBlob` con lo observado o en `null`; los `Rationale`, «no
    evaluado».
  - `NOT_DETERMINED` y `null` en esos campos solo valen con NOT_ACCREDITED.
