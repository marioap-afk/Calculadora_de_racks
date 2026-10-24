I-62 — REVISIÓN FORMAL DEL ARCHITECT DE LA RONDA 4 DE LA ENMIENDA A-7 (admisión de los insumos del cierre de VERIFY de FX-02 en F6), acotada a la corrección de A62-A7R3-R1 (el `RebaseMap` de la reverificación de AUTOMATION_PLAN 16.7) y a sus consecuencias (decisiones §98, puntos 2, 3 y 6), con la autorización del Owner CLAUDE-CLI-I62 = A. ÚNICA invocación, sin reintento.

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
Ronda 3                = {{RUN}}\a7-r3.md   (blob 8bcccb2fd2cea480f73ee5bf5a1583e0c9eaf1ff, commit 37ac6ca4; {{A7R3_BYTES}} bytes, {{A7R3_LINES}} líneas)
Revisión de la ronda 3 = R20261023T180846Z-62bd (auditor NOT_ACCREDITED; Architect CHANGES REQUIRED con A62-A7R3-R1 y A62-A7R3-O1..O4)
A-6 acordada           = docs/initiatives/I-62-A-6.md   blob 4bc919dd4768ab81abc74b70d3a4a3821c07feb5 (decisiones §90)
A-5 acordada           = docs/initiatives/I-62-A-5.md   blob ad94ceedacbf3dbf70f0217a583978d0e32592fc (decisiones §73)
Paquete del Architect  = ninguno (este texto hace de paquete)
Orden vigente          = {{RUN}}\order.txt   (cuerpo de la disposición §98 del Coordinator; 3 740 bytes, {{ORDER_LINES}} líneas, SHA-256 8b73975d9822a430f1a726268d4005609974dbdcc85a0674bf00dbb9e9018acf)
```

**Recibo.** El invocador comprobó que el objeto tiene el blob `{{OBJECT_BLOB8}}…` en la publicación, en el registro y en el commit de arriba, y que el
padre de la publicación tiene la ronda 3 (`8bcccb2f…`, el mismo blob que `a7-r3.md` y que el de A-7 en `37ac6ca4`); que la A-6 acordada (`4bc919dd…`)
no cambia del padre de la publicación al commit; que `docs/automation/evidence/I-62-A7/r4/` no existía en el padre, no cambia después de la
publicación salvo por el añadido del resultado `run` y que su `MANIFEST.sha256` coincide con los archivos; que el preflight de la ronda 3
(`docs/automation/evidence/I-62-A7/r3/`) no cambia desde el padre de la publicación; que decisiones solo crece por añadido desde el registro de §97 y
que §98 es su última sección en el commit del clon; que la revisión de la ronda 3 custodiada es la de A62-A7R3-R1 y O1-O4; que el paquete B-min,
`agent-execution/`, `m7/s91/`, `m7/s92/`, `m7/bench/`, `m7/readings/`, el kit `a6m7-measure`, `r2/`, `r3/` y la revisión de la ronda 3 conservan los
árboles de los commits que los custodiaron; y que las guardas custodiadas de la ronda 4 reproducen sus resultados (`run` sobre la publicación y la
autoprueba).

**Condición de lanzamiento (compuerta 11).** Decisiones §98, punto 7 (línea {{DEC96_P7}}; `order.txt`, líneas {{ORDER_P7}}), ordena preparar el kit
acotado sin ejecutar una revisión formal sin una disposición nueva. Esta invocación solo existe si una disposición del Coordinator POSTERIOR a
§98, que nombra esta revisión, se fijó en el kit (su número, su encabezado y el SHA-256 de su texto) y el lanzador comprobó inmediatamente antes de
lanzarla, en el registro de decisiones de la rama de I-62: esa sección, una sola vez, con el encabezado y el SHA-256 fijados y el blob completo del
objeto; ninguna sección posterior a ella; la línea del Owner `A6-ADR0046-9-FX02 = A (…)` de §78 como línea completa; la línea `CLAUDE-CLI-I62 = A (…)`
vigente y el binario medido igual al del cierre; y A-7 y A-6 en la HEAD de la rama con los blobs de arriba. Esa disposición es posterior al commit
del clon y no está en el cierre: tu orden vigente es §98 (`order.txt`). No juzgas esa autorización ni las determinaciones del Owner; juzgas la ronda 4
de A-7.

**Objeto y revisión.** La ronda 3 de A-7 (`a7-r3.md`) corrigió A62-A7-R1..R3 de la ronda 1. Su revisión formal (R20261023T180846Z-62bd) dio CHANGES
REQUIRED con un REQUIRED, A62-A7R3-R1 (la categoría (c) no admite el `RebaseMap` de la reverificación de AUTOMATION_PLAN 16.7, que está en el ámbito
de A7-1: sin él, la reverificación lee fuera del cierre o no fija `MainSha_eval`), y cuatro OPTIONAL (A62-A7R3-O1..O4), con R1-R3 de la ronda 1
corregidos salvo el residuo de R1 que nombra A62-A7R3-R1; el auditor la dejó NOT_ACCREDITED por una referencia no canónica del revisor. Decisiones §98
aceptó A62-A7R3-R1 como REQUIRED (punto 1, línea {{DEC96_P1}}) y autorizó la cuarta candidata limitada a la admisión del `RebaseMap` en la
reverificación de 16.7, «exclusivamente cuando la modalidad de reverificación lo requiere» (punto 2, línea {{DEC96_P2}}), y a las superficies que
dependen de ella (punto 3, línea {{DEC96_P3}}: la categoría (c), el constructor, el validador, el formato de la designación y las referencias
descriptivas; sin modificar A-6, AUTOMATION_PLAN 16.24 ni RAE). A-7 §3.8 (L{{A7_S37}}) lista lo que cambió: A7-1, puntos 1 (c), 2 (c) y 5 (c), y
cambios descriptivos en otras secciones; los opcionales A62-A7R3-O1..O4 no se incorporan. La revisión de la ronda 4 es ACOTADA (§98, puntos 2, 3 y 6) a:

- la resolución de A62-A7R3-R1: el registro de rebase de la sesión (`relay-record/v2` con `Phase: REBASE` y su `RebaseMap`) como séptimo papel de
  (c), solo en la reverificación de 16.7 sin conflictos; su origen y su ruta (el directorio del intento, con su propio `RunId`: AUTOMATION_PLAN 16.6);
  su identidad y su blob; su coherencia, con el rechazo de un registro incoherente; `MainSha_eval` = su `MainAfterSha` (AUTOMATION_PLAN 16.13); su
  fidelidad; su ausencia, su sustitución o su pertenencia a otra corrida; y su no admisión en cualquier otra invocación (A7-1, puntos 1 (c), 2 (c) y
  5 (c));
- la pureza del delta: los cambios normativos solo por A62-A7R3-R1, y los descriptivos que declara §3.8 dentro de ese alcance;
- la ausencia de regresión sobre lo que la ronda 3 encontró corregido (R1-R3 de la ronda 1) y sobre los temas que la ronda 1 encontró CONFORMS;
- un **dictamen expreso sobre la compatibilidad con A-5 y con A-6** (§98, punto 6, línea {{DEC96_P6}});
- la reconfirmación de la materialidad (M-01..M-08) y de que no hay ninguna consecuencia OWNER-RESERVED no identificada;
- las preguntas de la ronda 4 (A-7 §8, Q-A7R4-01..03).

Los opcionales de la ronda 3 (A62-A7R3-O1..O4) y los impactos relacionados (la exclusión de `contract`, D-1, y las propuestas de fallo cerrado FC) no
se revisan en esta ronda. Lo que dependa de una sesión real o de una disposición futura (la medición de A6-7, las disposiciones previas a la medición,
la nueva preparación del kit de la medición, B1) sigue como precondición con fallo cerrado. Sin acreditación excepcional: si no puedes cumplir un
punto con lecturas fieles, no lo des por cumplido. No se te prohíbe señalar otro defecto material de A-7 que encuentres: hazlo como hallazgo.

## Herramientas e identidad

- Tienes **Read**, **Grep** y **Glob**, y **StructuredOutput** para entregar el resultado. No tienes Bash, PowerShell ni ejecución de código: no puedes
  ejecutar `git`, las guardas, el constructor ni el validador del cierre de A7-1, el instrumento de B-min, su aceptante ni el kit de la medición.
- La identidad la verificó el invocador antes del lanzamiento: HEAD del clon = el commit, sin ramas ni remoto, árbol limpio, los blobs del cierre y el
  SHA-256 de los archivos del run. El auditor la vuelve a verificar después. En `Step0` declara solo lo que compruebes tú con tus herramientas.

## Paso 0, antes de cualquier lectura sustantiva

1. **Lectura directa de AUTOMATION_PLAN 16.7 (obligatoria).** Lee con Read `{{READ_AP}}` con `offset` {{AP_DIRECT_OFFSET}} y `limit`
   {{AP_DIRECT_LIMIT}}: incluye las líneas {{AP_DIRECT}} (la reverificación «después de escribir, sin conflictos», con el `RebaseMap` como entrada), que
   A7-1, puntos 1 (c) y 2 (c), y la fila L19 de A-7 §2 citan. Tiene que ser una llamada a Read sobre ese archivo (si es una copia fiel, tiene la misma
   numeración de líneas): la cita de A-7 no es una lectura de AUTOMATION_PLAN. Regístrala en `DirectReads` (`docs/AUTOMATION_PLAN.md:{{AP_DIRECT}}`).
2. Lee entera, con Read, `{{RUN}}\order.txt` (la orden vigente, {{ORDER_LINES}} líneas): punto 1 (líneas {{ORDER_P1}}), punto 2 ({{ORDER_P2}}), punto 3 ({{ORDER_P3}}),
   punto 4 ({{ORDER_P4}}), punto 5 ({{ORDER_P5}}), punto 6 ({{ORDER_P6}}), punto 7 ({{ORDER_P7}}), punto 8 ({{ORDER_P8}}) y punto 9 ({{ORDER_P9}}).
3. Lee con Read las líneas {{DEC96}} de `{{READ_DEC}}` (decisiones §98). Comprueba que la línea {{DEC96_SOURCE}} declara el cuerpo pegado de §98 con
   3 740 bytes y el SHA-256 de arriba; que los puntos 1-9 (líneas {{DEC96_P1}}-{{DEC96_P9}}) registran lo que dice `order.txt` (con la redacción del registro); y
   que el punto 1 acepta A62-A7R3-R1 como REQUIRED y el punto 2 autoriza la cuarta candidata, limitada a la admisión del `RebaseMap`.
4. Lee entero, con Read, `{{READ_GRR3}}` (`{{GRR3}}`, {{GRR3_LINES}} líneas): {{GRR3_KEYS}} deben dar `A7Blob` = el blob del objeto, `Base` = el padre de la
   publicación (`{{PUB_PARENT}}`), `Head` = la publicación (`{{PUBLICATION}}`), `Mode` run y `Result` PASS.
5. Lee con Read, en `{{READ_SUMMARY3}}` (`{{SUMMARY3}}`), las líneas {{SUMMARY3_CAND}} (`Candidate.Blob` = el blob del objeto), {{SUMMARY3_HOLD}} (`Hold` =
   `Cases` y `NotHolding` vacío) y {{SUMMARY3_ST}} (autoprueba de las guardas de la ronda 4).
6. Lee con Read, en `{{READ_R2OUT}}` (`{{R2OUT}}`), las líneas {{R2OUT_KEYS}}: `RunId` = R20261023T180846Z-62bd, `ReviewedBlob` = el blob de la ronda 3
   y `RequiredFindings` con A62-A7R3-R1.

Si algo no coincide, o si un archivo del run o del objeto no se puede leer, para: `Verdict` = `"NOT_ACCREDITED"`, con el motivo en `KnownLimitations`.

## Cierre de insumos (solo esto)

**Archivos del run** (`{{RUN}}\`):
- `order.txt`: la orden vigente (disposición §98; 3 740 bytes, {{ORDER_LINES}} líneas). Su SHA-256 es el que declara decisiones §98 en el commit. Puede ser
  referencia (`order.txt:<línea>`);
- `a7-r3.md`: el texto de la ronda 3 de A-7 (blob `8bcccb2f…`), byte a byte, para comparar la corrección con el texto que la ronda 3 revisó. Puede ser
  referencia (`a7-r3.md:<línea>`);
- `prompt.md`: este mismo texto. No hace falta leerlo y nunca es referencia;
- {{FAITHFUL_COUNT}} **copias fieles** `fiel__*.txt` (abajo, «Copias fieles»): cada una se lee EN LUGAR de su original; nunca son referencia.

**Copias fieles (lectura byte a byte).** Read no entrega fielmente los bytes de algunos archivos del cierre: elimina en silencio un BOM inicial (una
eliminación silenciosa nunca es una entrega fiel), trunca las líneas largas y no garantiza los bytes de controles, CR sueltos, NUL, UTF-8 inválido,
U+FEFF interior, separadores de línea Unicode ni de los caracteres no ASCII cuya entrega no se ha medido. Cada archivo así tiene en el run una copia
fiel (formato RACKCAD-COPIA-FIEL/1) con los bytes exactos del original y **la misma numeración de líneas**: la línea n de la copia es la línea n del
original. En la copia, `\\` es una barra invertida y `\xHH` es un byte escrito explícitamente (el BOM es `\xEF\xBB\xBF`, un TAB `\x09`, un CR `\x0D`,
cada byte de un carácter no medido o de un UTF-8 inválido el suyo, y un espacio al final de una línea `\x20`); fuera de eso, el texto es literal. Una
línea del original que no cabe termina en `\>` y sigue en el apéndice, después de la última línea del original, en las líneas `@n.2|…`, `@n.3|…`, que
se concatenan en orden y sin separador. El pie de la copia declara la ruta, el blob, el SHA-256 y las marcas del original. **Lee siempre la copia y
nunca el original** de estos archivos:
{{FAITHFUL_COPIES}}

Todas las rutas que siguen son relativas al clon `{{CLONE}}` y están en el commit de arriba. Son las {{CLOSURE_COUNT}} rutas del cierre: **las únicas
rutas que puedes leer y citar** (además de `order.txt` y `a7-r3.md`), escritas exactamente así.

**Objeto**, entero: `docs/initiatives/I-62-A-7.md` (léelo con Read en `{{READ_A7}}`): {{A7_MAP}}. Texto de A7-1: L{{A7_RULE}}; puntos {{A7_POINTS}};
categorías del punto 1: {{A7_CATS}}. Tabla de categorías de §3.3: L{{A7_S33_T}}. Relación con A-6 y con R7-A: §3.4 (L{{A7_S34}}). §3.5 (L{{A7_S35}}; residuo
L{{A7_S35_RES}}). Correcciones de la ronda 3, §3.7 (L{{A7_S36}}). Corrección de la ronda 4, §3.8 (L{{A7_S37}}): alcance L{{A7_S37_SCOPE}}; fila de A62-A7R3-R1
L{{A7_S37_ROW}}. Fila del registro de rebase en §4: L{{A7_S4_REBASE}}. Materialidad: §5 (L{{A7_S5}}). Compatibilidad: §6 (L{{A7_S6}}; A-5 L{{A7_S6_A5}}; A-6
L{{A7_S6_A6}}). Consecuencias reservadas al Owner: §7 (L{{A7_S7}}). Preguntas: §8 (L{{A7_S8}}; las de la ronda 4, filas L{{A7_QR3}}).

**Preflight dirigido de la ronda 4** (`docs/automation/evidence/I-62-A7/r4/`, {{A7R4EVID_COUNT}} archivos, enteros; evidencia de apoyo, no autoridad: el
constructor `make_eic_a7.py`, el validador `check_a7_closure.py`, el formato `a7-designation-3.schema.json`, la derivación del lado del aceptante
`designate_a7.py` y `derive_r4.py`, las designaciones (con `designations-r3/RV.json`, la de la reverificación), las entradas sintéticas (con
`inputs-rebase/`, el registro de rebase de una reverificación real del banco), el caso tipo B1 `b1-like/`, las fases q0-q8 (q6, la compatibilidad;
q8, el `RebaseMap`: presente, ausente, sustituido, de otra corrida, `RunId` incorrecto, blob alterado, `MainSha_eval`, uso en una VERIFY ordinaria),
las guardas con su autoprueba y el resultado `run`; `preflight.md` es su informe):

```text
{{A7R4EVID_TABLE}}
```

**Preflight dirigido de la ronda 3** (`docs/automation/evidence/I-62-A7/r3/`, {{A7R3EVID_COUNT}} archivos, enteros, sin cambio desde la ronda 3: la
evidencia de R1-R3 corregidos, con `a7-designation/2`):

```text
{{A7R3EVID_TABLE}}
```

**Revisión de la ronda 3** (`docs/automation/evidence/I-62-architect-A-7/R20261023T180846Z-62bd/`, tres archivos): `output.json` (A62-A7R3-R1 y O1-O4,
la resolución de R1-R3, la pureza del delta, la regresión, la materialidad y las preguntas: {{R2OUT_RANGES}}), `audit.json` ({{R2AUDIT_RANGES}}) y
`README.md`.

```text
{{R2REV_TABLE}}
```

**Evidencia de la medición de A6-7 (`m7/`)** ({{M7_COUNT}} archivos, enteros; orientación: la determinación de §91, `i6-determination.md`, que A-7 cita
como origen; D-1 (`s91/d1/`) y las doce lecturas (`s91/r12/`); los borradores de §92 (`s92/`); del banco, su README (la referencia R7-A), las
designaciones, `p1-build.json` y los resultados de N4 y nc1 (`p3-n4.json`, `p4-nc1.json`) con los que comparan los preflights; y
`readings/readings.md`):

```text
{{M7_TABLE}}
```

**Kit de la medición `a6m7-measure`** ({{KITM7_COUNT}} archivos, enteros; congelado y sin lanzar; A-7 §10 lo cita):

```text
{{KITM7_TABLE}}
```

**Paquete B-min** ({{PKG_COUNT}} archivos, enteros; el instrumento fijado es `{{TOOL}}` ({{TOOL_LINES}} líneas) y el aceptante fijado es `{{ACCEPTOR}}`
({{ACCEPTOR_LINES}} líneas, por tramos de 300)):

```text
{{PKG_TABLE}}
```

**Canónicos.** Los rangos orientan, no limitan: puedes leer entero cualquier archivo de esta lista, por tramos de 300 líneas como máximo si pasa de
60 000 bytes.
{{CLOSURE_LIST}}

**Evidencia de I-62** (`docs/automation/evidence/I-62-evidence.md`, en la lista de canónicos; rangos de orientación: {{EVIDENCE_RANGES}}). Es el registro
de las sesiones que prepararon A-7, sus rondas 2 y 3 y sus preflights: contiene sus resúmenes y valoraciones (también de A-7). Úsala como evidencia de
hechos que puedes contrastar con los archivos custodiados y con el texto normativo, nunca como autoridad ni como juicio sobre A-7; no adoptes sus
conclusiones sin comprobarlas. Lo mismo vale para los `preflight.md` de las dos rondas, `i6-determination.md`, los borradores de `s92/` y
`coordinator-notes.md`: son de la misma supervisión.

Nada más. Quedan fuera del cierre, en particular: `docs/automation/evidence/I-62-C1C3/` (C1-C3 y la nota de aislamiento de I-65); el preflight de la
ronda 1 (`docs/automation/evidence/I-62-A7/` fuera de `r2/` y `r3/`) y las disposiciones propuestas de O1-O8 (`r2-dispositions/`); la revisión de la
ronda 1; el resto de `docs/automation/evidence/I-62-A6/` (también `r4/`); el kit y los registros del lanzamiento de la revisión de la ronda 3 (salvo los
tres archivos de arriba) y los directorios de las revisiones anteriores; el repositorio de la entrega del fixture FX-U1, el bundle
`MAP-INVALID/candidate-b.bundle` y los clones temporales de los preflights; WORKFLOW, FOUNDATIONS, `state/I-62.yml`; las transcripciones y la memoria. Lo
que A-7, sus preflights o la evidencia citen de esos archivos no lo puedes comprobar: si una conclusión tuya depende de ello, decláralo en
`KnownLimitations`.

## Lista de lectura: fuentes que cita A-7 §2 (L1-L27)

Las guardas de la ronda 4 (G3, en el resultado `run` custodiado) comprobaron que cada literal de A-7 §2 está en las líneas citadas del blob fijado
de su fuente (L16 y L19 con dos grupos de líneas de AUTOMATION_PLAN; L21 en el esquema `relay-record/v2`, cuyas líneas son JSON). Úsala como lista de
comprobación: antes de apoyarte en una cita de A-7 (una fila de §2, un bloque «> », una referencia «L…»), lee la fuente en esas líneas con Read y
contrástala.

{{LIT_CHECKLIST}}

## Reglas de lectura (el invocador audita después cada llamada)

**Regla de esta revisión.** Toda referencia `ruta:línea` o `ruta:inicio-fin` apunta solo a líneas que leíste con Read **en ese mismo archivo** (o, si
tiene copia fiel, en su copia fiel) y que te llegaron fielmente. Una cita de una fuente dentro de A-7 no es una lectura de la fuente: para citar
`docs/AUTOMATION_PLAN.md:594` tienes que haber leído con Read la línea 594 de ese archivo o de su copia fiel; si solo leíste la cita en A-7, cita la
línea de A-7.

Cada ruta se comprueba por su **efecto**: resuelta contra el directorio de trabajo, con `.` y `..` colapsados y los enlaces resueltos.

- **Read:**
  - usa la ruta absoluta, solo de archivos del cierre bajo `{{CLONE}}` o de los archivos del run (`order.txt`, `a7-r3.md`, `prompt.md` y las copias
    fieles);
  - nunca leas con Read el original de un archivo que tiene copia fiel (lista de «Copias fieles»): lee la copia;
  - en los archivos de más de 60 000 bytes ({{LARGE_FILES}}), usa `offset` y `limit` de **300 líneas como máximo**;
  - haz una sola lectura por llamada. Si una lectura falla por el tamaño de su salida, repite ese rango en tramos más cortos.
- **Grep:** solo con `path` = un **archivo** de la lista (del cierre o del run), sin `glob` ni `type`. Nunca un directorio. Sin `path`, Grep busca en
  el directorio de trabajo, que es un directorio.
- **Glob:** no la necesitas, porque las rutas están arriba. Si la usas, su alcance (`path` y patrón) solo puede contener archivos de la lista
  (`docs/automation/evidence/I-62-A7/r4/`, `docs/automation/evidence/I-62-A7/r3/`, `docs/automation/evidence/I-62-A6/m7/s91/`,
  `docs/automation/evidence/I-62-A6/m7/s92/`, `docs/automation/evidence/I-62-A6/m7/bench/designations/`, el kit `a6m7-measure` y el directorio del
  paquete B-min están enteros en el cierre; el resto de `I-62-A7/`, de `I-62-A6/` y de `I-62-architect-A-7/` no). Un patrón que alcance cualquier otro
  archivo cuenta como lectura fuera del cierre, aunque no leas ese archivo.
- **StructuredOutput:** una sola vez, al final, con el resultado.
- **Prohibido:**
  - cualquier otra ruta del clon o del disco, en particular `~/.claude`, `~/.codex`, el repositorio de trabajo, otros clones `D:\r62-*`, las áreas
    de trabajo `D:\r62-a7\*`, el fixture (`D:\r62-fixture\*`) y el subdirectorio `launch\` del run;
  - red y web; subagentes u otros agentes; escribir.
- Un intento rechazado o fallido fuera del cierre sigue siendo una acción fuera del contrato, y una denegación de permiso se registra como violación.
- **Lecturas fallidas.** Una llamada que falla o no devuelve contenido no acredita nada y no puede sostener una referencia.
- **Archivos que Read no entrega fielmente** (BOM, líneas largas, controles, CR sueltos, NUL, UTF-8 inválido, caracteres no medidos): léelos solo por
  su copia fiel (arriba). Una línea con `\>` al final solo cuenta como entregada si lees también sus continuaciones `@n.2|…` del apéndice. Si, aun así,
  una línea no te llega fiel, no la cites y, si te hiciera falta, decláralo en `KnownLimitations`.
- **Contexto.** El cierre es grande: lee los rangos que necesites. Una compactación automática del contexto añade a la transcripción un mensaje que el
  auditor cuenta como un segundo mensaje de usuario, y la corrida queda sin acreditar.
- **Unicode.** El texto canónico es UTF-8, con caracteres como ≤ ≥ ≠ → ∖ ∩ ∪ « » ^ y acentos.
  - Si una salida llega truncada, con «?», con «=» en lugar de ≤ o ≥, o con U+FFFD, no la uses: vuelve a leer ese rango.
  - Si no puedes obtenerla fiel, decláralo en `KnownLimitations`.
  - Un artefacto del transporte nunca es un hallazgo técnico.

## Referencias (`Evidence`, `ClauseRefs` y `DirectReads`)

- Una referencia es `ruta:línea` o `ruta:inicio-fin`: la ruta, **relativa** al clon para los archivos del cierre (p. ej.,
  `docs/initiatives/I-62-A-7.md:{{A7_RULE}}`), y el nombre suelto para los archivos del run que pueden ser premisa (`order.txt:12`, `a7-r3.md:130`). Sin
  unidad (`D:`), sin barra inicial, sin `.` ni `..`, sin barras invertidas ni rutas absolutas. `prompt.md` nunca es referencia.
- **La ruta es exactamente una de las {{CLOSURE_COUNT}} rutas del cierre de arriba** (las tablas, el objeto y la lista de canónicos), o `order.txt` o
  `a7-r3.md`: cópiala de este texto, letra a letra; no la reconstruyas de memoria. El esquema del resultado rechaza las rutas que no son de las familias
  del cierre, y el auditor comprueba cada referencia contra el archivo real del commit: que la ruta es un archivo del cierre y que sus líneas existen en
  él. En la ronda 3, una sola referencia a `docs/initiatives/I-62-A7.md` (sin el guion de `A-7`, un archivo que no existe) dejó la revisión sin acreditar.
- Una copia fiel nunca es referencia: cita la ruta del **original** con el número de línea que muestra la copia (es el mismo). El invocador comprueba
  cada copia contra los bytes del original y acredita la línea del original solo si la copia te la entregó entera y fiel.
- Cita solo líneas que hayas leído enteras con Read en ese archivo (o en su copia fiel) y que te llegaron fielmente. El invocador comprueba que Read te
  entregó cada línea citada, también en `DirectReads` y en los `ClauseRefs` de los elementos anidados.
- **Números de línea exactos:** los que Read muestra a la izquierda. Una referencia desplazada, aunque sea en una línea, no se acredita. El final de un
  rango no pasa del número de líneas del archivo (en las tablas de arriba).
- **Autocomprobación antes de entregar.** Recorre cada referencia del resultado (`Evidence`, `ClauseRefs` de todos los elementos, también los anidados,
  y `DirectReads`) y comprueba, una por una: (1) su ruta aparece literalmente en este texto como ruta del cierre, o es `order.txt` o `a7-r3.md`; (2) es
  relativa, con `/`, sin unidad, sin barra inicial y sin `..`; (3) sus líneas existen en ese archivo; (4) las leíste con Read en ese archivo o en su
  copia fiel. Corrige o quita la que no pase las cuatro.

## Qué decidir

Se pide el veredicto formal sobre la ronda 4 exacta de A-7 (LIFECYCLE §5 y §6), acotado a A62-A7R3-R1 y a sus consecuencias (decisiones §98, puntos 2,
3 y 6). Contesta con el texto normativo del cierre y con lo que leas.

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
  `a7-r3.md` y con las fuentes (AUTOMATION_PLAN 16.6, 16.7 y 16.13, L18-L20 de A-7 §2; el esquema `relay-record/v2`, L21; V14 B.8.7, L22; A6-3, regla 6
  (b), L{{A6_A63_R6B}}; A6-9, regla 4, L{{A6_A69_R4}}, y su tabla, L{{A6_DERIV}}), y con lo que §98, punto 2, pide definir: origen y ruta; identidad y blob;
  `RunId`; fidelidad; ausencia; sustitución o pertenencia a otra corrida; rechazo ante incoherencias; derivación de `MainSha_eval`; y la no admisión en
  las verificaciones ordinarias.
- **`DeltaPurity`** (§98, punto 2: corrección «limitada a resolver la admisión de RebaseMap»; punto 3: solo las superficies que dependen de ella): en
  `Changes`, una vez cada cambio de la ronda 4, con `Assessment` `WITHIN_SCOPE` (el cambio está dentro de la corrección de A62-A7R3-R1 o es descriptivo
  y no cambia el significado normativo; no cita ningún REQUIRED) u `OUTSIDE_SCOPE` (un cambio normativo que no exige A62-A7R3-R1; cita un REQUIRED):
  - `R3R1_REBASE_ROLE_REVERIFICATION_ONLY`: el registro de rebase como séptimo papel de (c), solo en la reverificación de 16.7 sin conflictos; ninguna
    otra invocación lo tiene (A7-1, punto 1 (c));
  - `R3R1_REBASE_PATH_AND_RUNID`: su ruta, en el directorio del intento, en un directorio con su propio `RunId` (16.6), distinto de los de la
    delegación, la entrega y la reverificación, registrado en la designación (punto 2 (c));
  - `R3R1_REBASE_COHERENCE`: las condiciones de coherencia del registro (punto 2 (c));
  - `R3R1_MAINSHA_EVAL_FROM_REBASEMAP`: `MainSha_eval` = el `MainAfterSha` del `RebaseMap`, posterior a su `MainBeforeSha` y antecesor de `Target.commit`
    (punto 2 (c); 16.13);
  - `R3R1_ABSENCE_AND_NON_ADMISSION`: sin un registro presente, con blob y `RunId` registrados, legible y coherente, o sin la entrega, no hay cierre ni
    lanzamiento y nada lo sustituye; fuera de la reverificación, no se admite (punto 5 (c));
  - `S33_ROW_C_AND_S34_C6_NOTE`: la fila (c) de §3.3 y la nota de §3.4 sobre C6 (el registro de rebase no recibe nombre en C6);
  - `DESCRIPTIVE_SECTIONS`: las demás secciones descriptivas que declara §3.8 (cabecera, §2 con L18-L22, §4, §5 con M-03 y M-04, §6, §8, §10, §11 y el
    Anexo);
  - `PROCEDURE_PIECES_ROUND3`: las piezas del procedimiento de la ronda 4 que identifican el invalidador (constructor, validador, formato
    `a7-designation/3` y `designate_a7.py`; `derive_r4.py` sin cambio), sin cambio del texto del punto 7;
  y en el conjunto, `Status` `CONFIRMED` (todos `WITHIN_SCOPE`) o `NOT_CONFIRMED` (alguno `OUTSIDE_SCOPE`; cita un REQUIRED), con `Rationale`,
  `ClauseRefs` y `FindingIds` en cada cambio y en el conjunto.
- **`RegressionCheck`** (consecuencias de la ronda 4 sobre lo que la ronda 3 encontró corregido o conforme; `output.json` de la ronda 3,
  {{R2OUT_REG}}), una vez cada tema, con `Status` `NO_REGRESSION` (la ronda 4 lo conserva; no cita ningún REQUIRED) o `REGRESSION` (cita un REQUIRED),
  `Rationale`, `ClauseRefs` y `FindingIds`:
  - `R1_TRX_AND_RELAYFACTS_AS_CORRECTED`: la corrección de R1 de la ronda 1 (los papeles de (c), los TRX de `CurrentSha` y de `RedSha` con la exigencia
    de RED de 16.8, los hechos del relevo y `RelayFacts`);
  - `R2_INVALIDATOR_RULE_AND_PROCEDURE`: el invalidador = A7-1 en el blob acordado más el procedimiento nombrado (punto 7);
  - `R3_A62_RULE1_PRECISION`: la precisión de A6-2, regla 1, dentro del ámbito;
  - `T04_EFF_EFF1_B1_MAP_VALIDATION`, `T05_MAP_INVALID_ABSENT_NONCONSTRUCTIBLE`, `T06_CLAUSEMAP_COVERAGE_RULE`, `T07_MULTI_CATEGORY_ENTRIES` y
    `T09_NO_UNAUTHORIZED_CHANGES`: los temas que la ronda 1 encontró CONFORMS (EFF y EFF^1 en la validación de mapas de B1; MAP_INVALID frente a
    entrada inexistente y cierre no construible; la regla de cobertura de `clausemap`; las entradas de varias categorías; la ausencia de cambios no
    autorizados).
- **`CompatibilityVerdicts`** (§98, punto 6, línea {{DEC96_P6}}: «un dictamen explícito sobre compatibilidad con A-5, además de A-6»): `A5` y `A6`,
  cada uno con `Status` `COMPATIBLE` (la ronda 4 no contradice ni cambia esa enmienda acordada fuera de lo que A-7 declara; no cita ningún REQUIRED) o
  `INCOMPATIBLE` (cita un REQUIRED), `Rationale`, `ClauseRefs` (al menos una de la enmienda que juzgas) y `FindingIds`. A-5: A5-1, L{{A5_1}}, y A5-2,
  regla 4, L{{A5_2R4}} (A-5 entera en el cierre); lo que declara A-7, §6 (L{{A7_S6_A5}}). A-6: las reglas que A-7 precisa o usa (A6-2, reglas 1 y 3,
  L{{A6_A62_R1}} y L{{A6_A62_R3}}; A6-3, regla 6 (b), L{{A6_A63_R6B}}; A6-5, regla 2, L{{A6_A65_R2}}; A6-7, reglas 1, 5 y 7, L{{A6_A67_R1}}, L{{A6_A67_R5}} y
  L{{A6_A67_R7}}; A6-9, regla 4, L{{A6_A69_R4}}, y su tabla, L{{A6_DERIV}}); lo que declaran A-7, §3.4 (L{{A7_S34}}) y §6 (L{{A7_S6_A6}}). El preflight de la
  ronda 4 (`q6-compat.json`) es evidencia de apoyo, no autoridad.
- **`Materiality`** (LIFECYCLE §3; A-7 §5, L{{A7_S5}}): en `Triggers`, una vez cada disparador `M-01`..`M-08`, con `A7Declared` (lo que declara la columna
  «Activado» de A-7 §5: `yes` para **sí**, `no` para no) y `Assessment` (`AGREE` si coincides con esa declaración después de la ronda 4; `DISAGREE` si
  no, y entonces cita un REQUIRED); `Rationale`, `ClauseRefs` y `FindingIds` en cada disparador y en el conjunto.
- **`OwnerReservedConsequence`** (LIFECYCLE §3, OWNER-RESERVED; A-7 §7, L{{A7_S7}}): `OwnerDecisionRequired` (true si identificas una consecuencia
  OWNER-RESERVED que A-7 no identifica o que no está dispuesta; false si no) y `UnidentifiedConsequences` (cada una; vacía con false), con `Rationale`,
  `ClauseRefs` y `FindingIds`. Las líneas del Owner constan (decisiones §78, línea {{DEC78_LINE}}; A-6 §7, L{{A6_LINE_A}} y L{{A6_LINE_B}}): ninguna cuenta aquí.
- **`QuestionAnswers`** (A-7 §8, filas L{{A7_QR3}}): `Q-A7R4-01`..`Q-A7R4-03`, una vez cada una: `Answer` (tu respuesta) y `Outcome` (`SATISFIED` si la
  respuesta no exige ningún cambio de A-7, `NOT_SATISFIED` si lo exige; un NOT_SATISFIED cita al menos un hallazgo), con `ClauseRefs` y `FindingIds`.
- **`DirectReads`:** `{Ref, Purpose}` de cada fuente que leíste directamente para comprobar una cita de A-7. Incluye siempre
  `docs/AUTOMATION_PLAN.md:{{AP_DIRECT}}` (Paso 0) y, como mínimo recomendado, una entrada por fuente de la lista de A-7 §2 en la que te apoyes.
- **Además:**
  - tu modo de revisión, si revisor y autor son la misma persona y el contexto que el runtime te inyectó (`ReviewerMode`, `SamePersonAsAuthor`,
    `InjectedContextDeclaration`). Declara también el proveedor del revisor y del autor en `SamePersonAsAuthor`. Los identificadores de cuenta que el
    runtime te inyecte (correo, uuid de organización u otros) se declaran solo por su tipo, sin reproducir su valor;
  - la identidad revisada (`ReviewedCommit`, `ReviewedPath`, `ReviewedBlob`);
  - lo que leíste (`Reads`: «ruta: rangos») y la acción siguiente que recomiendas (`RecommendedNextAction`).

## Resultado

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
  "Step0": {"Basis": "INVOKER_VERIFIED_CLONE", "AP167DirectReadL593L595": "true | false | null", "OrderReadWhole": "true | false | null",
            "Decisions98DeclaresOrderBytesAndSha256": "true | false | null", "Decisions98AcceptsR3R1AndAuthorizesThirdCandidate": "true | false | null",
            "GuardsRunModeBaseHeadResult": "true | false | null", "PreflightSummaryCandidateAndTotals": "true | false | null",
            "Round2OutputIdentifiesRunAndFindings": "true | false | null", "Note": "..."},
  "DirectReads": [{"Ref": "docs/AUTOMATION_PLAN.md:{{AP_DIRECT}}", "Purpose": "..."}],
  "Reads": ["ruta: rangos"],
  "Verdict": "AGREED | CHANGES REQUIRED | REJECTED | BLOCKED — OWNER DECISION | NOT_ACCREDITED",
  "RequiredFindings": [{"FindingId": "A62-A7R4-R1", "Section": "...", "ViolatedInvariant": "...", "Counterexample": "...", "Evidence": ["ruta:inicio-fin"],
                        "WhyItMatters": "...", "CorrectionRequired": "..."}],
  "OptionalFindings": [{"FindingId": "A62-A7R4-O1", "Section": "...", "Note": "...", "Evidence": ["ruta:línea"], "SuggestedCorrection": "..."}],
  "PriorFindingResolution": [{"PriorFinding": "A62-A7R3-R1", "Status": "CORRECTED | PARTIALLY_CORRECTED | NOT_CORRECTED",
                              "Rationale": "...", "ClauseRefs": [], "FindingIds": []}],
  "DeltaPurity": {"Status": "CONFIRMED | NOT_CONFIRMED | NOT_DETERMINED",
                  "Changes": [{"Change": "R3R1_REBASE_ROLE_REVERIFICATION_ONLY | … | PROCEDURE_PIECES_ROUND3", "Assessment": "WITHIN_SCOPE | OUTSIDE_SCOPE",
                               "Rationale": "...", "ClauseRefs": [], "FindingIds": []}],
                  "Rationale": "...", "ClauseRefs": [], "FindingIds": []},
  "RegressionCheck": [{"Topic": "R1_TRX_AND_RELAYFACTS_AS_CORRECTED | … | T09_NO_UNAUTHORIZED_CHANGES", "Status": "NO_REGRESSION | REGRESSION",
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
- `Step0`: las siete booleanas son lo que compruebas en el Paso 0 (la primera, la lectura directa de AUTOMATION_PLAN L{{AP_DIRECT}}); `null` en lo que no
  compruebes.
- `DirectReads` incluye siempre AUTOMATION_PLAN L{{AP_DIRECT}}, también si te detienes en el Paso 0.
- Cobertura, una vez cada elemento: `PriorFindingResolution`, A62-A7R3-R1; `DeltaPurity.Changes`, los ocho cambios; `RegressionCheck`, los ocho temas;
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
- **Con AGREED:** cero REQUIRED; A62-A7R3-R1 en CORRECTED; `DeltaPurity` CONFIRMED con los ocho cambios `WITHIN_SCOPE`; los ocho temas en
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
