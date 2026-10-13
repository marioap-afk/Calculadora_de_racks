I-62 — REVISIÓN FORMAL DEL ARCHITECT DE LA RONDA 2 DE LA ENMIENDA A-6 (instrumento de hechos fijado por blob para el Controller de FX-02 en F6, B-min), preparada según la disposición del Coordinator §80, punto 6, y la autorización del Owner CLAUDE-CLI-I62 = A. ÚNICA invocación, sin reintento automático.

Eres el ARCHITECT de esta revisión: un proceso `claude-cli` nuevo, separado de la sesión autora de A-6 y de sus dos rondas, de quienes escribieron el paquete B-min, del Coordinator, del dictamen de materialidad de B-min y de la revisión de la ronda 1. Revisas en solo lectura la versión exacta de la ronda 2 de A-6 y devuelves un único veredicto formal (LIFECYCLE §5 y §6). No redactas texto normativo de sustitución, no implementas nada, no escribes, no haces commit ni push y no invocas a otros agentes.

## Identidades

```text
RunId                  = {{RUN_ID}}
InvocationId           = {{INVOCATION_ID}}
LogicalReviewRequestId = {{LOGICAL_ID}}   AttemptSeq = 1
Clon de lectura        = {{CLONE}}   (tu directorio de trabajo; HEAD separado en el commit, sin ramas locales; sin remoto)
Commit                 = {{CLONE_COMMIT}}
Publicación ronda 2    = {{PUBLICATION}}   (A-6 ronda 2, PROPUESTA; padre {{PUB_PARENT}}, con la A-6 de la ronda 1)
Guardas `run`          = {{GUARDS_RUN_COMMIT}}   (custodia del resultado `run` de las guardas de la ronda 2 sobre la publicación)
Objeto                 = docs/initiatives/I-62-A-6.md   blob {{OBJECT_BLOB}}   ({{OBJECT_BYTES}} bytes, {{OBJECT_LINES}} líneas)
Ronda 1                = blob eac473c0d2e58cf1c4594f48a8816b3833af6509, en el run como {{RUN}}\a6-r1.md ({{A6R1_BYTES}} bytes, {{A6R1_LINES}} líneas)
Revisión de la ronda 1 = R20261011T215735Z-c434: docs/automation/evidence/I-62-architect-A-6/R20261011T215735Z-c434/output.json (blob e90d685b…)
Paquete del Architect  = ninguno (este texto hace de paquete)
Orden vigente          = {{RUN}}\order.txt   (disposición §80 del Coordinator; 2 833 bytes, {{ORDER_LINES}} líneas, SHA-256 1342b5be1a5de104ac08a1115ddc999d2e6dfbe80100affaa16ea44439fa7f99)
```

**Recibo.** El invocador comprobó que el objeto tiene el blob `{{OBJECT_BLOB8}}…` en la publicación, en la custodia de las guardas y en el commit de arriba, y
que el padre de la publicación tiene el blob de la ronda 1 (`eac473c0…`); que `docs/automation/evidence/I-62-A6/r2/` no cambia después de la
publicación salvo por el añadido del resultado `run`; que decisiones solo crece por añadido desde el registro de §80; que el paquete B-min, el dictamen
y la revisión de la ronda 1 conservan sus árboles y blobs; que `a6-r1.md` es, byte a byte, el blob `eac473c0…`; y que las guardas custodiadas de la
ronda 2 reproducen sus resultados (`run` sobre la publicación y `self-test`).

**Condición de lanzamiento (compuerta 11).** Decisiones §79, punto 7, y §80, punto 5 (línea {{DEC80_P5}}), no permiten una revisión nueva sin una disposición
posterior del Coordinator. Esta invocación existe porque el lanzador lo comprobó inmediatamente antes de lanzarla, en el registro de decisiones de la rama de I-62:
la línea del Owner `A6-ADR0046-9-FX02 = A (…)` de §78 como línea completa; la línea `CLAUDE-CLI-I62 = A (…)` vigente y el binario medido igual al del
cierre; A-6 en la HEAD de la rama con el blob de arriba; y una disposición del Coordinator numerada después de §80 que nombra ese blob y este RunId.
Esa disposición es posterior a tu clon y no está en él: su ausencia en las decisiones del clon no es un defecto de A-6. No juzgas la autorización ni la
determinación del Owner; juzgas A-6, incluido el alcance que fijan sus dos líneas (§7).

**Objeto y revisión.** Es la segunda revisión formal de A-6. La ronda 1 terminó en CHANGES REQUIRED con cinco REQUIRED (A62-A6-R1..R5) y tres
OPTIONAL (A62-A6-O1..O3), literales en la salida de la ronda 1. Su auditor la dejó NOT_ACCREDITED por un único motivo: una referencia a V14
L2333-L2334 que el revisor no había leído con Read en V14, sino en la cita que hace A-6. Decisiones §79 la conserva íntegra, no concede acreditación
excepcional, acepta R1-R5 como asuntos que resolver y fija, en su punto 2, lo que debe conseguir cada corrección; su punto 4 (línea {{DEC79_P4}}) pide
disposiciones de O1-O3 y una resolución expresa de M-01. Decisiones §80, punto 3, exige el registro individual de las brechas G1-G6, y su punto 6 (`order.txt`, líneas
41-51) exige que esta revisión:

- (a) incluya la lectura directa de V14 L2333-L2334;
- (b) cubra R1-R5 y O1-O3 de la ronda 1;
- (c) evalúe G1-G6 uno por uno;
- (d) verifique la materialidad M-01;
- (e) contraste el texto normativo con el código real del aceptante;
- (f) examine la procedencia y la integridad en el momento de uso.

Sin acreditación excepcional: si no puedes cumplir una de ellas con lecturas fieles, no la des por cumplida.

## Herramientas e identidad

- Tienes **Read**, **Grep** y **Glob**, y **StructuredOutput** para entregar el resultado. No tienes Bash, PowerShell ni ejecución de código: no puedes
  ejecutar `git`, las guardas, el instrumento de B-min, su aceptante, sus pruebas ni la reproducción.
- La identidad la verificó el invocador antes del lanzamiento: HEAD del clon = el commit, sin ramas ni remoto, árbol limpio, los blobs del cierre y el
  SHA-256 de los archivos del run. El auditor la vuelve a verificar después. En `Step0` declara solo lo que compruebes tú con tus herramientas.

## Paso 0, antes de cualquier lectura sustantiva

1. **Lectura directa de V14 (obligatoria).** Lee con Read `{{CLONE}}\docs\initiatives\I-62-proposal-v14.md` con `offset` {{V14_DIRECT_OFFSET}} y
   `limit` {{V14_DIRECT_LIMIT}}: incluye las líneas {{V14_DIRECT}} (Anexo C, «Esperado independiente»). Tiene que ser una llamada a Read sobre ese archivo:
   la cita que hace A-6 de esas líneas no es una lectura de V14. Regístrala en `DirectReads` (`docs/initiatives/I-62-proposal-v14.md:{{V14_DIRECT}}`).
2. Lee entera, con Read, `{{RUN}}\order.txt` (la orden vigente, {{ORDER_LINES}} líneas): punto 3 (líneas 20-31), punto 5 (36-40) y punto 6 (41-51).
3. Lee con Read las líneas {{DEC79_80}} de `{{CLONE}}\docs\automation\decisions\I-62.md` (decisiones §79 y §80). Comprueba que la línea {{DEC80_SOURCE}}
   declara el cuerpo pegado de §80 con 2 833 bytes y el SHA-256 de arriba, y que §80, punto 6 (línea {{DEC80_P6}}), registra las mismas seis
   exigencias que `order.txt`, líneas 41-51 (con la redacción del registro). En §79, punto 1 (línea {{DEC79_P1}}): la revisión de la ronda 1
   conservada, CHANGES REQUIRED, auditor NOT_ACCREDITED, sin acreditación excepcional y R1-R5 aceptados; punto 2 (línea {{DEC79_P2}}): lo que pide
   cada corrección.
4. Lee entero, con Read, `{{CLONE}}\{{GRR}}` ({{GRR_LINES}} líneas): las líneas {{GRR_HEAD}} deben dar `Mode` RUN, `Base` = el padre de la publicación
   (`{{PUB_PARENT}}`) y `Head` = la publicación (`{{PUBLICATION}}`), y la línea {{GRR_RESULT}} el `Result` de conjunto PASS.
5. Lee con Read las líneas {{ST_TAIL}} de `{{CLONE}}\{{STJ}}` (`Summary`, `Result`, `Mode`, `Repo`, `ToolBlob` y `A6TextBlob`): `Result` PASS y
   `A6TextBlob` = el blob del objeto.
6. Lee con Read, en `{{CLONE}}\docs\automation\evidence\I-62-architect-A-6\R20261011T215735Z-c434\output.json`, las líneas {{R1OUT_HEAD}} (RunId
   R20261011T215735Z-c434, `ReviewedBlob` = `eac473c0…`, `Verdict` CHANGES REQUIRED) y los hallazgos: REQUIRED {{R1OUT_R}}; OPTIONAL {{R1OUT_O}}.

Si algo no coincide, o si un archivo del run o del objeto no se puede leer, para: `Verdict` = `"NOT_ACCREDITED"`, con el motivo en `KnownLimitations`.

## Cierre de insumos (solo esto)

**Archivos del run** (`{{RUN}}\`):
- `order.txt`: la orden vigente (disposición §80; 2 833 bytes, {{ORDER_LINES}} líneas). Su SHA-256 es el que declara decisiones §80 en el commit;
- `a6-r1.md`: el texto de A-6 de la ronda 1 (blob `eac473c0…`), byte a byte, para comparar R1-R5 y O1-O3 con la ronda 2. Más de 60 000 bytes: por
  tramos de 300 líneas como máximo. Puede ser referencia (`a6-r1.md:<línea>`);
- `prompt.md`: este mismo texto. No hace falta leerlo y nunca es referencia.

Todas las rutas que siguen son relativas al clon `{{CLONE}}` y están en el commit de arriba.

**Objeto**, entero (más de 60 000 bytes: por tramos de 300 líneas como máximo): `docs/initiatives/I-62-A-6.md`: {{A6_MAP}}.

**Evidencia de la ronda 2** ({{R2_COUNT}} archivos, enteros; evidencia de apoyo, no autoridad: guardas, autoprueba, resultados, preflight, reproducción
de R1 y R4 sobre datos reales, evidencia de independencia y reproducción de R3). Los resultados JSON de una sola línea (más de 2 000 caracteres)
tienen una **copia legible** en `docs/automation/evidence/I-62-A6/r2/readable/` (misma ruta relativa; líneas de 1 900 caracteres como máximo;
`readable/MANIFEST.json` fija, para cada copia, el archivo canónico, su blob y la transformación): lee la copia legible y **cita la ruta de la
copia**, no la del JSON de una sola línea:

```text
{{R2_TABLE}}
```

**Paquete B-min** ({{PKG_COUNT}} archivos, enteros; para el contraste con el código real del aceptante lee `{{ACCEPTOR}}` ({{ACCEPTOR_LINES}} líneas,
por tramos de 300); para la independencia, el instrumento, las notas, las pruebas y los resultados):

```text
{{PKG_TABLE}}
```

**Dictamen de materialidad de B-min y revisión de la ronda 1** (salidas estructuradas; la de la ronda 1 trae R1-R5 y O1-O3 literales):

```text
{{RULING_TABLE}}
```

**Canónicos.** Los rangos orientan, no limitan: puedes leer entero cualquier archivo de esta lista, por tramos de 300 líneas como máximo si pasa de
60 000 bytes.
{{CLOSURE_LIST}}

**Evidencia de I-62** (`docs/automation/evidence/I-62-evidence.md`, en la lista de canónicos). A diferencia de la ronda 1, está en el cierre,
porque decisiones §79, punto 5, exige acceso directo a toda fuente que el texto cite y A-6 cita sus secciones (rangos citados: {{EVIDENCE_G11}}).
Es el registro de la sesión que preparó A-6 y este kit: contiene sus resúmenes y valoraciones. Úsala como evidencia de hechos que puedes contrastar
con los archivos custodiados y con el texto normativo, nunca como autoridad ni como juicio sobre A-6; no adoptes sus conclusiones sin comprobarlas.
Sus §156 y §158 (y decisiones §81) tratan de C1-C3, ajenos a esta revisión.

Nada más. Quedan fuera del cierre, en particular: `docs/automation/evidence/I-62-C1C3/` (C1-C3, decisiones §81: ajeno a esta revisión); la
evidencia de la ronda 1 fuera de `r2/` y el resto del directorio de la revisión de la ronda 1 (`audit.json`, `README.md`, `kit/`,
`launch/`); la tarjeta y el paquete OWNER-RESERVED de ADR-0046 #9 y la solicitud de medición (`docs/automation/evidence/I-62-F6/requests/`); las
transcripciones y la memoria; `probe_tools.py` y los `reference.json`/`comparison.json` de las sondas; el mapa de cláusulas y su esquema, WORKFLOW,
FOUNDATIONS, `state/I-62.yml` y el fixture. Lo que A-6 cite de esos archivos no lo puedes comprobar: si una conclusión tuya depende de ello, decláralo
en `KnownLimitations`.

## Lista de lectura: fuentes que cita A-6 (G11)

Las guardas de la ronda 2 (G11, en el resultado `run` custodiado) registran todas las fuentes y rangos de líneas que cita el texto de A-6. Úsala como
lista de comprobación: antes de apoyarte en una cita de A-6 (un bloque «> », una tabla, «V14 L…», «AP L…», «RAE L…», «DEC L…», `ruta:línea`), lee la
fuente en esas líneas con Read y contrástala.

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
  (`docs/automation/evidence/I-62-A6/r2/` y el directorio del paquete B-min están enteros en el cierre; el resto de `I-62-A6/` no). Un patrón que
  alcance cualquier otro archivo cuenta como lectura fuera del cierre, aunque no leas ese archivo.
- **StructuredOutput:** una sola vez, al final, con el resultado.
- **Prohibido:**
  - cualquier otra ruta del clon o del disco, en particular `~/.claude`, `~/.codex`, el repositorio de trabajo, otros clones `D:\r62-*`, el fixture
    (`D:\r62-fixture\*`) y el subdirectorio `launch\` del run;
  - red y web; subagentes u otros agentes; escribir.
- Un intento rechazado o fallido fuera del cierre sigue siendo una acción fuera del contrato, y una denegación de permiso se registra como violación.
- **Lecturas fallidas.** Una llamada que falla o no devuelve contenido no acredita nada y no puede sostener una referencia.
- **Líneas de más de 2 000 caracteres.** Read las entrega truncadas: {{LONG_LINES}}. Una línea truncada no cuenta como entregada: no la cites y, si
  te hiciera falta, decláralo en `KnownLimitations`. Para los JSON de una sola línea de `r2/`, lee y cita la copia de `r2/readable/` (arriba).
- **Contexto.** El cierre es grande: lee los rangos que necesites. Una compactación automática del contexto añade a la transcripción un mensaje que el
  auditor cuenta como un segundo mensaje de usuario, y la corrida queda sin acreditar.
- **Unicode.** El texto canónico es UTF-8, con caracteres como ≤ ≥ ≠ → ∖ ∩ ∪ « » y acentos.
  - Si una salida llega truncada, con «?», con «=» en lugar de ≤ o ≥, o con U+FFFD, no la uses: vuelve a leer ese rango.
  - Si no puedes obtenerla fiel, decláralo en `KnownLimitations`.
  - Un artefacto del transporte nunca es un hallazgo técnico.

## Referencias (`Evidence`, `ClauseRefs`, `DirectReads`, `A6Refs` y `AcceptorRefs`)

- Una referencia es `ruta:línea` o `ruta:inicio-fin`: la ruta, relativa al clon para los archivos del cierre (p. ej.,
  `docs/initiatives/I-62-A-6.md:{{A6_LINE_A}}`), y el nombre suelto para los archivos del run que pueden ser premisa (`order.txt:44`,
  `a6-r1.md:1`). Sin rutas absolutas ni barras invertidas. `prompt.md` nunca es referencia.
- Cita solo líneas que hayas leído enteras con Read en ese archivo y que te llegaron fielmente. El invocador comprueba que Read te entregó cada línea
  citada, también en `DirectReads`, `A6Refs` y `AcceptorRefs`.
- **Números de línea exactos:** los que Read muestra a la izquierda. Una referencia desplazada, aunque sea en una línea, no se acredita. Antes de
  entregar, contrasta cada referencia con la salida de Read de la que sale.

## Qué decidir

Se pide el veredicto formal sobre la A-6 exacta de la ronda 2 (LIFECYCLE §5 y §6), con las seis exigencias de §80, punto 6. Contesta con el texto
normativo del cierre y con lo que leas. No se te prohíbe señalar otro defecto material que encuentres en cualquier sección de A-6.

- **`Verdict`:** `AGREED` (cero REQUIRED abiertos sobre la versión exacta), `CHANGES REQUIRED` (al menos un REQUIRED) o `BLOCKED — OWNER DECISION`
  (falta una autoridad reservada: otra consecuencia OWNER-RESERVED que A-6 no declara, o una que no esté dispuesta; A-6 §7). Un REQUIRED impide el
  acuerdo hasta su disposición y corrección.
- **Hallazgos nuevos.** Un hallazgo es REQUIRED en los casos de LIFECYCLE §5 (activa M-01..M-08 sobre un elemento congelable, deja un invariante sin
  obligación de prueba, produce comportamiento observable distinto, o revela un elemento materialmente incorrecto, no ejecutable o no verificable);
  una precisión sin cambio de significado es OPTIONAL. Los hallazgos de esta ronda llevan ids nuevos (REQUIRED `A62-A6R2-R1`, `A62-A6R2-R2`…;
  OPTIONAL `A62-A6R2-O1`…), distintos de los de la ronda 1, que aquí solo se disponen. Cada uno con la sección exacta de A-6 (`Section`), su evidencia
  (`Evidence`: al menos una referencia) y la corrección (`CorrectionRequired` en un REQUIRED, `SuggestedCorrection` en un OPTIONAL). Un REQUIRED
  trae además la autoridad o el invariante violado (`ViolatedInvariant`) o un contraejemplo concreto (`Counterexample`), al menos uno no vacío, y por
  qué importa (`WhyItMatters`).
- **`TopicVerdicts`:** los cuatro temas de decisiones §77, punto 2, que siguen siendo los temas de revisión de A-6, una vez cada uno, con `Rationale`,
  `ClauseRefs` y `FindingIds`: `Q-A6-01` (A-6 §8, L{{Q01}}; §3.3, L{{A6_S33}}), y los tres REQUIRED REVIEW TOPIC `Q-A6-02` (integridad, procedencia y autorización de
  la entrega del objeto Git suelto; fallo cerrado; L{{Q02}}), `Q-A6-06/07` (correspondencia normativa exacta entre REJECT_FACTS, INVALID_OUTPUT, FAIL y
  UNVERIFIED, sin inventar estados; L{{Q0607}}) y `Q-A6-12` (independencia efectiva del aceptante, no solo declarada; L{{Q12}}). `SATISFIED`,
  `NOT_SATISFIED` (cita al menos un hallazgo; en los tres REQUIRED REVIEW TOPIC, al menos un REQUIRED) o `NEEDS_OWNER` (con `OwnerDecisionRequired` =
  true). Un tema SATISFIED no cita ningún REQUIRED.
- **`Round1FindingResolution` (exigencia b):** cada REQUIRED de la ronda 1, `A62-A6-R1`..`A62-A6-R5`, una vez. Compara el hallazgo literal de la
  salida de la ronda 1 (`Section`, `ViolatedInvariant`, `Counterexample`, `CorrectionRequired`), lo que pide para él decisiones §79, punto 2, el texto
  de la ronda 1 (`a6-r1.md`) y el de la ronda 2 (§3.16, L{{A6_S316}}, y los deltas que nombra). `Status`: `CORRECTED` (el defecto ya no existe y se
  cumple lo que pide §79, punto 2), `PARTIALLY_CORRECTED` o `NOT_CORRECTED`; con `Rationale`, `ClauseRefs` y `FindingIds`. Lo que no esté corregido
  del todo cita al menos un REQUIRED nuevo; un CORRECTED no cita ninguno. Para R3, §80, punto 2 (línea {{DEC80_P2}}): si la evidencia de la ronda 2
  registra la reproducción de R3 como INCOMPLETE o no la acredita, R3 no puede estar CORRECTED.
- **`Round1OptionalDisposition` (exigencia b):** `A62-A6-O1`..`A62-A6-O3`, una vez cada uno: `INCORPORATED`, `ACCEPTABLE_AS_IS` (la ronda 2 no lo
  incorpora y no hace falta) o `NOT_ADDRESSED` (cita al menos un hallazgo), con `Rationale`, `ClauseRefs` y `FindingIds`.
- **`DeltaVerdicts`:** cada delta, una vez, `AGREED` o `CHANGES REQUIRED`, con su base, sus `ClauseRefs` y sus `FindingIds`: A6-1 (§3.4, L{{A6_S34}}), A6-2
  (§3.5), A6-3 (§3.6, L{{A6_S36}}), A6-4 (§3.7, L{{A6_S37}}), A6-5 (§3.8, L{{A6_S38}}), A6-6 (§3.9), A6-7 (§3.10), A6-8 (§3.11) y A6-9
  (§3.12, L{{A6_S312}}). CHANGES REQUIRED cita al menos un REQUIRED; AGREED,
  ninguno. Todo REQUIRED queda citado por al menos un delta o un tema.
- **`GapAssessment` (exigencia c):** cada brecha residual G1..G6 (A-6 §3.12: {{A6_G}}), una vez, por separado. Para cada una: el hecho no demostrado,
  el riesgo, la mitigación, la cobertura real de las pruebas, si exige cambiar un blob y el efecto normativo (§80, punto 3, línea {{DEC80_P3}}:
  ninguna brecha se declara cerrada por una mitigación no ensayada). `MitigationTested` (`yes`, `no` o `partial`) es lo que tú compruebas en la
  evidencia (`r2/`), no lo que A-6 dice de sí misma. `Assessment`: `ACCEPTABLE_RESIDUAL` (residuo declarado y aceptable tal como A-6 lo acota),
  `REQUIRES_PIECE_CHANGE` (exige cambiar el instrumento o el aceptante: otro blob, otra corrección de A-6 y otra determinación del Owner, A6-8, regla 5;
  cita un REQUIRED), `REQUIRES_TEXT_CHANGE` (A-6 debe acotarla o tratarla de otra forma; cita un REQUIRED) o `REQUIRES_OWNER_DECISION` (con
  `OwnerDecisionRequired` = true). Un ACCEPTABLE_RESIDUAL no cita ningún REQUIRED.
- **`M01Verification` (exigencia d):** M-01 de LIFECYCLE §3 frente a A-6 §5 (L{{A6_S5}}) y a la evaluación UNKNOWN del revisor de la ronda 1:
  `A6Declared` (la columna «Activado» de A-6 §5: sí → `yes`, no → `no`), `Activated` (tu evaluación: `yes`, `no` o `UNKNOWN`), `Agreement` (`AGREE`
  si coinciden; `DISAGREE` si no, con al menos un hallazgo citado), `Rationale`, `ClauseRefs` y `FindingIds`. `Activated` = tu `ReviewerAssessment`
  de M-01 en `MaterialityConfirmation`.
- **`MaterialityConfirmation`:** M-01..M-08 de LIFECYCLE §3, una vez cada uno: `A6Declared` (columna «Activado» de A-6 §5), `ReviewerAssessment`
  (`yes`, `no` o `UNKNOWN`) y `Agreement` (`AGREE` si coinciden; `DISAGREE` si no, con al menos un hallazgo citado), con la base y al menos una
  `ClauseRef`.
- **`AcceptorCodeContrast` (exigencia e):** cada afirmación de A6-5, regla 2 (L{{A6_A65_R2}}), y de A6-3, reglas 5 y 6 (L{{A6_A63_R5}} y
  L{{A6_A63_R6}}), contrastada con el código real del aceptante fijado (`{{ACCEPTOR}}`, blob `434ef1fc…`). Para cada afirmación, una vez: `A6Refs`
  (las líneas de A-6 que la enuncian) y `AcceptorRefs` (las líneas de `bmin_accept.py` que leíste con Read y que la confirman o la refutan; al menos
  una, siempre de ese archivo), `Status` (`MATCHES` o `DIFFERS`; un DIFFERS cita un REQUIRED; un MATCHES no cita ninguno), `Rationale` y `FindingIds`.
  Las citas de código de A-6, de G10 o de las notas no bastan: lee el código. Las afirmaciones:
  - `CHECK_IDS`: el conjunto de comprobaciones R1-R10 del resultado;
  - `STATUS_VALUES`: los estados de cada comprobación (`MATCH`, `MISMATCH`, `NOT_APPLICABLE`, `MANUAL`);
  - `WAIVABLE`: qué comprobaciones se pueden declarar no aplicables, y que la `Decision` sola no basta porque el aceptante también da `ACCEPT_FACTS`
    con comprobaciones declaradas no aplicables;
  - `DECISION_FORMULA`: cuándo la `Decision` es `ACCEPT_FACTS` y cuándo `REJECT_FACTS`;
  - `EXIT_CODES`: los códigos de salida 0, 1 y 64 (error de uso) y su relación con la `Decision`;
  - `R8_MANUAL`: R8 siempre en `MANUAL`;
  - `R5_DATA_CONDITION`, `R6_DATA_CONDITION`, `R7_DATA_CONDITION`: qué datos de la solicitud hacen aplicable cada una (órdenes en `Runs`; para R6,
    también una `AuthorityResolution` no vacía en la verificación) y qué pasa sin ellos;
  - `R4_INPUTS`: lo que lee R4 (la salida `controller-verification/v2`, la delegación, la entrega, la ruta ignorada, la revisión de los términos,
    `MainSha`) frente a lo que A6-3, regla 6, pone en la solicitud;
  - `R10_RELAY_FACTS`: `RelayFacts` con el SHA-256 citado;
  - `REQUEST_REPO`, `REQUEST_TOOL_FROM_OBJECT`, `REQUEST_INTERPRETER`, `REQUEST_RUNS`, `REQUEST_CLOSURE`, `REQUEST_NOT_APPLICABLE`: cada entrada de la
    solicitud que fija A6-3, reglas 5-6 (`Repo` = el `-C`; `Tool` ejecutado desde el objeto; `Interpreter`, que R2 compara; `Runs` con su argv y
    los bytes de su salida; `Closure` derivado; `NotApplicable` solo por la tabla de A6-5, regla 2), frente a cómo la lee y la usa el código.
- **`ProvenanceIntegrityAtUse` (exigencia f):** si cada pieza de la identidad del objeto efectivo en el momento de uso es suficiente para lo que pide
  R1 (§79, punto 2; Q-A6-02), una vez cada una, `ADEQUATE` o `INADEQUATE` (cita un REQUIRED; un ADEQUATE no cita ninguno), con `Rationale`,
  `ClauseRefs` y `FindingIds`. Contrasta el texto (A6-3, regla 1, L{{A6_A63_R1}}; A6-3, regla 3, L{{A6_A63_R3}}; A6-1, regla 1, L{{A6_A61_R1}}; A6-4,
  regla 1, L{{A6_A64_R1}}) con la reproducción sobre datos reales (`r2/r1r4/`) y con lo que Git garantiza o no:
  - `PREPARATION_WRITE`: la escritura de preparación (cuándo, cómo, qué registra, qué instantáneas, fallo si difieren);
  - `TIMING_AT_USE`: el momento de la comprobación (inmediatamente antes de cada lanzamiento VERIFY, tras la Salida y antes de la cesión, con la sesión
    retenida) y que nada pueda cambiar el objeto entre la comprobación y el uso;
  - `POU_I_NO_REPLACE_REFS` … `POU_VIII_NO_SHADOW_EXECUTABLES`: los pasos (i) a (viii) de A6-3, regla 3, uno por uno;
  - `DECLARED_ORDER`: la orden declarada (`<git> --no-replace-objects cat-file blob <blob> | <intérprete> -I - <orden> …`, rutas absolutas, tubería
    sin alteración de bytes);
  - `FIDELITY_BY_BYTES`: el registro de fidelidad por el recálculo del blob sobre los bytes que entrega la orden, nunca por el nombre del objeto;
  - `FAIL_CLOSED`: que cualquier diferencia o la imposibilidad de comprobar deja la invocación sin lanzar y sin contar, con S-04 y STOP.
- **`DirectReads`:** `{Ref, Purpose}` de cada fuente que leíste directamente para comprobar una cita de A-6. Incluye siempre
  `docs/initiatives/I-62-proposal-v14.md:{{V14_DIRECT}}` (Paso 0) y, como mínimo recomendado, una entrada por fuente de la lista G11 en la que te apoyes.
- **`Separability`** (A-6 §9, L{{A6_S9}}): si es cierto que dentro de A-6 ningún delta es separable (`WithinA6NoDeltaSeparable`), que A-6 es separable
  frente a A-1..A-5 (`SeparableFromA1toA5`) y que las dos líneas del Owner son separables entre sí (`OwnerLinesSeparable`): CONFIRMED o NOT_CONFIRMED,
  con la base, sus `ClauseRefs` y, si alguna es NOT_CONFIRMED, al menos un hallazgo citado.
- **`CompatibilityA1toA5`** (A-6 §6.2, L{{A6_S62}}): A-1..A-5, cada una `COMPATIBLE` o `CONFLICT`, con la base.
- **`NoChangeConfirmation`** (A-6 §3.13, L{{A6_S313}}; decisiones §76, punto 2): si A-6 deja sin cambio la receta 16.4 (`Recipe164`), el adapter
  `codex-cli` (`AdapterCodexCli`), los esquemas de salida (`OutputSchemas`), los presupuestos, topes y totales congelados de D.3, D.5 y D.8
  (`FrozenBudgetsD3D5D8`), los roles y la autoridad del Controller (`RolesAndControllerAuthority`), producción (`Production`), ADR-0046 y ADR-0048
  (`ADR0046AndADR0048`) y V14 fuera de los nueve párrafos añadidos (`V14OutsideNineParagraphs`): NO_CHANGE o CHANGE, con la base.
- **`ScopeConfirmation`:** si es cierto lo que A-6 afirma de la ubicación de sus párrafos (§1, L{{A6_S1}}; «Ubicación» de §3.4-§3.12), comparándola con
  V14 y A-5: seis párrafos al final de D.3 después de A5-2 (`D3AppendAfterA52`), dos al final de D.4 después de V14 L2554 (`D4AppendAfterV14L2554`),
  uno al final de D.6 después de V14 L2587 (`D6AppendAfterV14L2587`), y ningún texto congelado ni acordado sustituido (`NoFrozenTextReplaced`):
  CONFIRMED o NOT_CONFIRMED, con la base.
- **`D5AndOwnerValidation`** (A-6 §3.15, L{{A6_S315}}): `EXCLUSION_CORRECT`, `COVER_IN_A6` (cita al menos un REQUIRED) u `OWNER_DECISION_REQUIRED`
  (`OwnerDecisionRequired` = true), con `OwnerValidationAffected` (YES o NO), la base, sus `ClauseRefs` y sus `FindingIds`.
- **`OwnerDecisionsScope`** (A-6 §7, L{{A6_S7}}; líneas L{{A6_LINE_A}} y L{{A6_LINE_B}}; la (a), registrada en decisiones §78, línea {{DEC78_LINE}}): si la
  línea (a) (`LineA`) y la (b) (`LineB`) se limitan al alcance que enuncian (`LIMITED_TO_STATED_SCOPE`) o van más allá (`EXCEEDS_STATED_SCOPE`), en
  particular frente a «en su verificación» (R5); si son distintas y ninguna implica la otra (`SeparateAndNotImplied`); y si ninguna acepta un resultado
  por adelantado (`NoResultAcceptedInAdvance`): CONFIRMED o NOT_CONFIRMED, con la base y sus `ClauseRefs`.
- **`OwnerAuthorityCheck`:** si el acuerdo exige una decisión del Owner que todavía no consta (`OwnerDecisionRequired`: true o false). La línea (a)
  consta (§78) y la (b) solo rige la aplicación: ninguna de las dos cuenta aquí. Si concluyes que hace falta otra determinación (por ejemplo, porque
  una brecha exige otro blob), true, y nombra cada consecuencia en `AdditionalOwnerReservedConsequences`.
- **Preguntas de la ronda 2** (A-6 §8, L{{A6_S8}}; filas L{{QR2}}): contéstalas dentro del campo que corresponda (deltas, R1-R5, G1-G6); no tienen campo propio.
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
  "Schema": "rackcad-a6r2-architect-review/0",
  "RunId": "{{RUN_ID}}", "InvocationId": "{{INVOCATION_ID}}", "LogicalReviewRequestId": "{{LOGICAL_ID}}", "AttemptSeq": 1,
  "RequestedRole": "ARCHITECT", "Action": "REVIEW_DESIGN", "ReviewedUnit": "I-62",
  "ReviewedCommit": "...", "ReviewedPath": "docs/initiatives/I-62-A-6.md", "ReviewedBlob": "...",
  "ReviewerMode": "SAME-SESSION ROLE | SEPARATE SESSION | EXTERNAL HUMAN",
  "SamePersonAsAuthor": "texto: relación entre el revisor, la sesión autora de A-6 y el operador humano; proveedor del revisor y del autor",
  "ReviewerDeclaredIdentity": {"Runtime": "... o UNKNOWN", "Model": "... o UNKNOWN", "Effort": "... o UNKNOWN", "SessionOrThread": "... o UNKNOWN"},
  "InjectedContextDeclaration": {"State": "DECLARED | NONE", "Items": ["todo lo que el runtime te inyectó antes de tu primera acción; los identificadores de cuenta, solo por su tipo"]},
  "Step0": {"Basis": "INVOKER_VERIFIED_CLONE", "V14DirectReadL2333L2334": "true | false | null", "OrderReadWhole": "true | false | null",
            "Decisions80DeclaresOrderBytesAndSha256": "true | false | null", "Decisions79RoundOneDisposition": "true | false | null",
            "GuardsRunModeBaseHeadResult": "true | false | null", "SelftestA6TextBlobMatchesObject": "true | false | null",
            "Round1OutputIdentifiesRunAndFindings": "true | false | null", "Note": "..."},
  "DirectReads": [{"Ref": "docs/initiatives/I-62-proposal-v14.md:{{V14_DIRECT}}", "Purpose": "..."}],
  "Reads": ["ruta: rangos"],
  "Verdict": "AGREED | CHANGES REQUIRED | BLOCKED — OWNER DECISION | NOT_ACCREDITED",
  "RequiredFindings": [{"FindingId": "A62-A6R2-R1", "Section": "...", "ViolatedInvariant": "...", "Counterexample": "...", "Evidence": ["ruta:inicio-fin"],
                        "WhyItMatters": "...", "CorrectionRequired": "..."}],
  "OptionalFindings": [{"FindingId": "A62-A6R2-O1", "Section": "...", "Note": "...", "Evidence": ["ruta:línea"], "SuggestedCorrection": "..."}],
  "TopicVerdicts": [{"Topic": "Q-A6-01 | Q-A6-02 | Q-A6-06/07 | Q-A6-12", "Disposition": "SATISFIED | NOT_SATISFIED | NEEDS_OWNER", "Rationale": "...",
                     "ClauseRefs": ["ruta:línea"], "FindingIds": []}],
  "Round1FindingResolution": [{"Round1Finding": "A62-A6-R1 | … | A62-A6-R5", "Status": "CORRECTED | PARTIALLY_CORRECTED | NOT_CORRECTED", "Rationale": "...",
                               "ClauseRefs": [], "FindingIds": []}],
  "Round1OptionalDisposition": [{"Round1Optional": "A62-A6-O1 | A62-A6-O2 | A62-A6-O3", "Status": "INCORPORATED | ACCEPTABLE_AS_IS | NOT_ADDRESSED",
                                 "Rationale": "...", "ClauseRefs": [], "FindingIds": []}],
  "DeltaVerdicts": [{"Delta": "A6-1 | … | A6-9", "Verdict": "AGREED | CHANGES REQUIRED", "Rationale": "...", "ClauseRefs": [], "FindingIds": []}],
  "GapAssessment": [{"Gap": "G1 | … | G6", "Assessment": "ACCEPTABLE_RESIDUAL | REQUIRES_PIECE_CHANGE | REQUIRES_TEXT_CHANGE | REQUIRES_OWNER_DECISION",
                     "MitigationTested": "yes | no | partial", "Rationale": "...", "ClauseRefs": [], "FindingIds": []}],
  "M01Verification": {"A6Declared": "yes | no | NOT_DETERMINED", "Activated": "yes | no | UNKNOWN", "Agreement": "AGREE | DISAGREE | NOT_DETERMINED",
                      "Rationale": "...", "ClauseRefs": [], "FindingIds": []},
  "MaterialityConfirmation": [{"Id": "M-01 | … | M-08", "A6Declared": "yes | no", "ReviewerAssessment": "yes | no | UNKNOWN", "Agreement": "AGREE | DISAGREE",
                               "Rationale": "...", "ClauseRefs": [], "FindingIds": []}],
  "AcceptorCodeContrast": [{"Claim": "CHECK_IDS | STATUS_VALUES | … | REQUEST_NOT_APPLICABLE", "A6Refs": ["docs/initiatives/I-62-A-6.md:…"],
                            "AcceptorRefs": ["{{ACCEPTOR}}:…"], "Status": "MATCHES | DIFFERS", "Rationale": "...", "FindingIds": []}],
  "ProvenanceIntegrityAtUse": [{"Item": "PREPARATION_WRITE | TIMING_AT_USE | POU_I_NO_REPLACE_REFS | … | FAIL_CLOSED", "Status": "ADEQUATE | INADEQUATE",
                                "Rationale": "...", "ClauseRefs": [], "FindingIds": []}],
  "Separability": {"WithinA6NoDeltaSeparable": "CONFIRMED | NOT_CONFIRMED | NOT_DETERMINED", "SeparableFromA1toA5": "...", "OwnerLinesSeparable": "...",
                   "Rationale": "...", "ClauseRefs": [], "FindingIds": []},
  "CompatibilityA1toA5": {"A1": {"Assessment": "COMPATIBLE | CONFLICT | NOT_DETERMINED", "Note": "..."}, "A2": {"Assessment": "...", "Note": "..."},
                          "A3": {"Assessment": "...", "Note": "..."}, "A4": {"Assessment": "...", "Note": "..."}, "A5": {"Assessment": "...", "Note": "..."}},
  "NoChangeConfirmation": {"Recipe164": {"Assessment": "NO_CHANGE | CHANGE | NOT_DETERMINED", "Note": "..."}, "AdapterCodexCli": {"Assessment": "...", "Note": "..."},
                           "OutputSchemas": {"Assessment": "...", "Note": "..."}, "FrozenBudgetsD3D5D8": {"Assessment": "...", "Note": "..."},
                           "RolesAndControllerAuthority": {"Assessment": "...", "Note": "..."}, "Production": {"Assessment": "...", "Note": "..."},
                           "ADR0046AndADR0048": {"Assessment": "...", "Note": "..."}, "V14OutsideNineParagraphs": {"Assessment": "...", "Note": "..."}},
  "ScopeConfirmation": {"D3AppendAfterA52": {"Assessment": "CONFIRMED | NOT_CONFIRMED | NOT_DETERMINED", "Note": "..."},
                        "D4AppendAfterV14L2554": {"Assessment": "...", "Note": "..."}, "D6AppendAfterV14L2587": {"Assessment": "...", "Note": "..."},
                        "NoFrozenTextReplaced": {"Assessment": "...", "Note": "..."}},
  "D5AndOwnerValidation": {"Disposition": "EXCLUSION_CORRECT | COVER_IN_A6 | OWNER_DECISION_REQUIRED | NOT_DETERMINED",
                           "OwnerValidationAffected": "YES | NO | NOT_DETERMINED", "Rationale": "...", "ClauseRefs": [], "FindingIds": []},
  "OwnerDecisionsScope": {"LineA": {"Assessment": "LIMITED_TO_STATED_SCOPE | EXCEEDS_STATED_SCOPE | NOT_DETERMINED", "Note": "..."},
                          "LineB": {"Assessment": "...", "Note": "..."}, "SeparateAndNotImplied": "CONFIRMED | NOT_CONFIRMED | NOT_DETERMINED",
                          "NoResultAcceptedInAdvance": "CONFIRMED | NOT_CONFIRMED | NOT_DETERMINED", "Rationale": "...", "ClauseRefs": []},
  "OwnerAuthorityCheck": {"AdditionalOwnerReservedConsequences": [], "OwnerDecisionRequired": "true | false | null", "Note": "..."},
  "IfAgreed": {"ScopeLimitedToFX02F6ControllerVerify": null, "NineAppendOnlyParagraphs": null, "NoChangeToProtectedSurfaces": null,
               "FourTopicsSatisfied": null, "AllNineDeltasAgreed": null, "Round1FindingsCorrected": null, "GapsAcceptableAsResiduals": null,
               "AcceptorContrastMatches": null, "ProvenanceAtUseAdequate": null, "NoAdditionalOwnerDecisionRequired": null, "D5AndOVExcluded": null,
               "SuitableForCoordinatorAgreement": null},
  "KnownLimitations": ["..."],
  "RecommendedNextAction": "..."
}
```

- Los valores de la plantilla son formas, no respuestas: cada campo lleva **tu** comprobación y **tu** evaluación. Donde la plantilla dice
  `"true | false | null"`, el valor es un booleano JSON o `null`, no un texto.
- `Step0`: las siete booleanas son lo que compruebas en el Paso 0 (la primera, la lectura directa de V14 L{{V14_DIRECT}}); `null` en lo que no compruebes.
- `DirectReads` incluye siempre V14 L{{V14_DIRECT}}, también si te detienes en el Paso 0.
- Cobertura, una vez cada elemento: `TopicVerdicts`, los cuatro temas; `Round1FindingResolution`, A62-A6-R1..R5; `Round1OptionalDisposition`,
  A62-A6-O1..O3; `DeltaVerdicts`, A6-1..A6-9; `GapAssessment`, G1..G6; `MaterialityConfirmation`, M-01..M-08; `AcceptorCodeContrast`, las diecisiete
  afirmaciones; `ProvenanceIntegrityAtUse`, las trece piezas.
- Todo id de `FindingIds` existe en `RequiredFindings` u `OptionalFindings`; los ids no se repiten.
- **Con un veredicto** (todo salvo NOT_ACCREDITED):
  - las siete booleanas de `Step0` en `true`; `ReviewedCommit`, `ReviewedPath` y `ReviewedBlob` = el commit, la ruta y el blob de arriba;
  - al menos una `ClauseRef` en cada tema, en cada R1-R5, en cada O1-O3, en cada delta, en cada G1-G6, en cada fila de `MaterialityConfirmation`, en
    cada pieza de `ProvenanceIntegrityAtUse`, en `M01Verification`, en `Separability`, en `D5AndOwnerValidation` y en `OwnerDecisionsScope`;
  - `M01Verification` con `A6Declared` = A-6 §5, `Agreement` AGREE o DISAGREE y `Activated` = tu evaluación de M-01 en `MaterialityConfirmation`;
  - ningún `NOT_DETERMINED` (en `Separability`, `CompatibilityA1toA5`, `NoChangeConfirmation`, `ScopeConfirmation`, `D5AndOwnerValidation` y
    `OwnerDecisionsScope`) y `OwnerDecisionRequired` distinto de `null`;
  - todo REQUIRED citado por al menos un delta o un tema.
- **Con AGREED:** cero REQUIRED; los nueve deltas en AGREED; los cuatro temas en SATISFIED; R1..R5 en CORRECTED; G1..G6 en ACCEPTABLE_RESIDUAL; las
  diecisiete afirmaciones en MATCHES; las trece piezas en ADEQUATE; `OwnerDecisionRequired` = false; `D5AndOwnerValidation` en EXCLUSION_CORRECT;
  `CompatibilityA1toA5` todo COMPATIBLE; `NoChangeConfirmation` todo NO_CHANGE; `ScopeConfirmation` todo CONFIRMED; `OwnerDecisionsScope` con las dos
  líneas en LIMITED_TO_STATED_SCOPE y los otros dos campos en CONFIRMED; y los doce campos de `IfAgreed` en `true`. En cualquier otro caso, `IfAgreed`
  va todo en `null`.
- **Con CHANGES REQUIRED:** al menos un REQUIRED. **Con BLOCKED — OWNER DECISION:** `OwnerDecisionRequired` = true. Con `OwnerDecisionRequired` = true,
  `AdditionalOwnerReservedConsequences` no vacía.
- **Sin acreditación posible** (identidad, fidelidad o insumos): no fabriques un veredicto. Pon `Verdict` = `"NOT_ACCREDITED"` y el motivo exacto en
  `KnownLimitations`.
  - `TopicVerdicts`, `Round1FindingResolution`, `Round1OptionalDisposition`, `DeltaVerdicts`, `GapAssessment`, `MaterialityConfirmation`,
    `AcceptorCodeContrast`, `ProvenanceIntegrityAtUse` y las listas de hallazgos pueden ir vacías.
  - Lo no evaluado va así: `NOT_DETERMINED` en `Separability`, `CompatibilityA1toA5`, `NoChangeConfirmation`, `ScopeConfirmation`,
    `D5AndOwnerValidation` (sus dos campos), `OwnerDecisionsScope` y en `A6Declared` y `Agreement` de `M01Verification` (con `Activated` = `UNKNOWN`);
    `null` en `OwnerDecisionRequired`, en `IfAgreed` y en las booleanas de `Step0` que no comprobaste; `ReviewedCommit` y `ReviewedBlob` con lo
    observado o en `null`; los `Rationale`, «no evaluado».
  - `NOT_DETERMINED` y `null` en esos campos solo valen con NOT_ACCREDITED.
