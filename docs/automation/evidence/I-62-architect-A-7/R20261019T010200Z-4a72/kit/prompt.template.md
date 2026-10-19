I-62 — REVISIÓN FORMAL DEL ARCHITECT DE LA ENMIENDA A-7 (admisión de los insumos del cierre de VERIFY de FX-02 en F6), acotada a A7-1 y a sus consecuencias sobre el cierre de VERIFY de FX-02/F6 y la medición de A6-7, autorizada por la disposición del Coordinator §93 y la autorización del Owner CLAUDE-CLI-I62 = A. ÚNICA invocación, sin reintento.

Eres el ARCHITECT de esta revisión: un proceso `claude-cli` nuevo, separado de la sesión autora de A-7 y de su preflight, de los agentes que construyeron el banco de la medición, el kit `a6m7-measure` y las determinaciones de §91 y §92, de quienes escribieron el paquete B-min, del Coordinator y de las revisiones de A-1..A-6. Revisas en solo lectura la versión exacta de A-7 y devuelves un único veredicto formal (LIFECYCLE §5 y §6). No redactas texto normativo de sustitución, no implementas nada, no escribes, no haces commit ni push y no invocas a otros agentes.

## Identidades

```text
RunId                  = {{RUN_ID}}
InvocationId           = {{INVOCATION_ID}}
LogicalReviewRequestId = {{LOGICAL_ID}}   AttemptSeq = 1
Clon de lectura        = {{CLONE}}   (tu directorio de trabajo; HEAD separado en el commit, sin ramas locales; sin remoto)
Commit                 = {{CLONE_COMMIT}}
Publicación de A-7     = {{PUBLICATION}}   (A-7 ronda 1, PROPUESTA; padre {{PUB_PARENT}}, sin A-7 y con decisiones §92)
Guardas `run`          = {{GUARDS_RUN_COMMIT}}   (custodia del resultado `run` de las guardas de A-7 sobre la publicación)
Objeto                 = docs/initiatives/I-62-A-7.md   blob {{OBJECT_BLOB}}   ({{OBJECT_BYTES}} bytes, {{OBJECT_LINES}} líneas)
A-6 acordada           = docs/initiatives/I-62-A-6.md   blob 4bc919dd4768ab81abc74b70d3a4a3821c07feb5 (decisiones §90)
Paquete del Architect  = ninguno (este texto hace de paquete)
Orden vigente          = {{RUN}}\order.txt   (disposición §93 del Coordinator; 4 057 bytes, {{ORDER_LINES}} líneas, SHA-256 a5c37f714364bc704c5f38218ae2b1a9209052c47834dcc5349684eb141be41b)
```

**Recibo.** El invocador comprobó que el objeto tiene el blob `{{OBJECT_BLOB8}}…` en la publicación, en la custodia de las guardas y en el commit de arriba, y
que el padre de la publicación no tiene A-7; que la A-6 acordada (`4bc919dd…`) no cambia del padre de la publicación al commit; que
`docs/automation/evidence/I-62-A7/` no cambia después de la publicación salvo por el añadido del resultado `run` y que su `MANIFEST.sha256` (y los de
`m7/s91/` y `m7/s92/`) coinciden con los archivos; que decisiones solo crece por añadido desde el registro de §92 y que §93 es su última sección en
el commit del clon; que el paquete B-min, `agent-execution/`, `m7/s91/`, `m7/s92/`, `m7/bench/`, `m7/readings/` y el kit `a6m7-measure` conservan
los árboles de los commits que los custodiaron; y que las guardas custodiadas de A-7 reproducen sus resultados (`run` sobre la publicación y la
autoprueba).

**Condición de lanzamiento (compuerta 11).** Decisiones §92, punto 9 (línea {{DEC92_P9}}), ordenó presentar la candidata «Sin lanzar la revisión formal sin una
disposición posterior»; §93, punto 1 (línea {{DEC93_P1}}; `order.txt`, líneas {{ORDER_P1}}), autoriza UNA revisión formal independiente de A-7. Esta invocación
existe porque §93 se fijó en el kit (su número, su encabezado y el SHA-256 de su texto) y el lanzador comprobó inmediatamente antes de lanzarla, en el
registro de decisiones de la rama de I-62: esa sección, una sola vez, con el encabezado y el SHA-256 fijados y el blob completo del objeto; ninguna
sección posterior a ella; la línea del Owner `A6-ADR0046-9-FX02 = A (…)` de §78 como línea completa; la línea `CLAUDE-CLI-I62 = A (…)` vigente y el
binario medido igual al del cierre; y A-7 y A-6 en la HEAD de la rama con los blobs de arriba. §93 está en el commit del clon y en el cierre (es tu
orden). No juzgas esa autorización ni las determinaciones del Owner; juzgas A-7.

**Objeto y revisión.** Es la primera revisión formal de A-7 (PROPUESTA, ronda 1). A-7 añade una regla, A7-1 (A-7 §3.2, L{{A7_S32}}; texto de la regla
L{{A7_RULE}}), al final de V14 D.3, después de A6-8: el cierre efectivo de insumos de las VERIFY del EXECUTION_CONTROLLER de FX-02 en F6 y de la invocación
de medición de A6-7 se compone de cuatro categorías de entradas, con su origen, identidad, revisión de referencia, fidelidad y ausencia. Decisiones §93
(encabezado en la línea {{DEC93_HEAD}}): su punto 1 (línea {{DEC93_P1}}) fija el objeto y el alcance exclusivo («A7-1 y sus consecuencias sobre el cierre de VERIFY de
FX-02/F6 y la medición de A6-7»); su punto 3 (línea {{DEC93_P3}}; `order.txt`, líneas {{ORDER_P3}}) enumera lo que tienes que examinar y pide dictaminar
expresamente la materialidad y si existe alguna consecuencia OWNER-RESERVED no identificada; sus puntos 4 y 5 (líneas {{DEC93_P4}} y {{DEC93_P5}}; `order.txt`,
líneas {{ORDER_P4}} y {{ORDER_P5}}) piden examinar dos impactos relacionados SIN incluirlos en A-7; sus puntos 2 y 6 (líneas {{DEC93_P2}} y {{DEC93_P6}}) fijan el kit; su punto 7 (línea {{DEC93_P7}}) fija
que AGREED exige cero REQUIRED; y su punto 8 (línea {{DEC93_P8}}), los límites. Se pide una revisión independiente y ACOTADA sobre:

- A7-1 frente a V14 B.9, AUTOMATION_PLAN 16.13 y 16.24, RAE I6 y A-6;
- las categorías (autoridades, objetos del `Target`, artefactos custodiados, instrumento);
- la procedencia, la revisión de referencia, la identidad y la fidelidad de cada categoría;
- EFF y EFF^1 en la validación de mapas de B1;
- la distinción entre MAP_INVALID, entrada inexistente y cierre no construible;
- la regla de cobertura de las lecturas de `clausemap`;
- las entradas que pertenecen a varias categorías;
- la compatibilidad con A-5 y A-6;
- la ausencia de cambios no autorizados en contratos, oráculos, presupuestos y producción;
- la aplicabilidad a N4, nc1 y N5-N7;
- la suficiencia de los negativos y del preflight;
- el dictamen expreso de la materialidad (M-01..M-08) y de toda consecuencia OWNER-RESERVED no identificada;
- las preguntas de A-7 §8 (Q-A7-01..06).

**Impactos relacionados (campos separados).** §93, punto 4: examinar la exclusión de la orden `contract` del instrumento (D-1) como impacto operativo
relacionado, sin incluir automáticamente su corrección en A-7; el borrador la excluye y mantiene `Contract` como obligación del Controller conforme a
U-19; el recálculo por la supervisión del punto 7 del borrador NO se incorpora: requiere una determinación de autoridad separada. §93, punto 5:
conservar la clasificación de §91 y examinar FC-01, FC-04, FC-10, FC-12, FC-13, FC-16, FC-24 y FC-18: si las condiciones de fallo cerrado cambian
indirectamente los criterios normativos de PASS o excluyen casos válidos necesarios para C-24, sin aceptar interpretaciones ambiguas por mera
coincidencia con la implementación. Estos impactos van en `RelatedImpacts`: NO son una condición del veredicto sobre A-7, salvo que revelen un defecto de
A-7 (entonces, un REQUIRED sobre A-7, con su sección de A-7).

Lo que dependa de una sesión real o de una disposición futura (la medición de A6-7, las disposiciones del Coordinator previas a la medición, la nueva
preparación del kit de la medición, la reconstrucción desde un registro real, la creación de B1) se mantiene como precondición con fallo cerrado: no
lo das por demostrado, y lo que A-7 deja como precondición lo juzgas como precondición (si está definida de forma verificable y si su falta produce un
fallo cerrado). Sin acreditación excepcional: si no puedes cumplir un punto con lecturas fieles, no lo des por cumplido.

Los demás aspectos de I-62 no se te piden. No se te prohíbe señalar otro defecto material de A-7 que encuentres: hazlo como hallazgo.

## Herramientas e identidad

- Tienes **Read**, **Grep** y **Glob**, y **StructuredOutput** para entregar el resultado. No tienes Bash, PowerShell ni ejecución de código: no puedes
  ejecutar `git`, las guardas, el constructor ni el validador del cierre de A7-1, el instrumento de B-min, su aceptante ni el kit de la medición.
- La identidad la verificó el invocador antes del lanzamiento: HEAD del clon = el commit, sin ramas ni remoto, árbol limpio, los blobs del cierre y el
  SHA-256 de los archivos del run. El auditor la vuelve a verificar después. En `Step0` declara solo lo que compruebes tú con tus herramientas.

## Paso 0, antes de cualquier lectura sustantiva

1. **Lectura directa de V14 (obligatoria).** Lee con Read `{{CLONE}}\docs\initiatives\I-62-proposal-v14.md` con `offset` {{V14_DIRECT_OFFSET}} y
   `limit` {{V14_DIRECT_LIMIT}}: incluye las líneas {{V14_DIRECT}} (B.9: `Target`, `AuthorityRevision` y `CanonicalInputs[]`). Tiene que ser una llamada a Read sobre
   ese archivo: la cita que hace A-7 de esas líneas no es una lectura de V14. Regístrala en `DirectReads` (`docs/initiatives/I-62-proposal-v14.md:{{V14_DIRECT}}`).
2. Lee entera, con Read, `{{RUN}}\order.txt` (la orden vigente, {{ORDER_LINES}} líneas): punto 1 (líneas {{ORDER_P1}}), punto 2 ({{ORDER_P2}}), punto 3 ({{ORDER_P3}}),
   punto 4 ({{ORDER_P4}}), punto 5 ({{ORDER_P5}}), punto 6 ({{ORDER_P6}}), punto 7 ({{ORDER_P7}}), punto 8 ({{ORDER_P8}}) y punto 9 ({{ORDER_P9}}).
3. Lee con Read las líneas {{DEC93}} de `{{CLONE}}\docs\automation\decisions\I-62.md` (decisiones §93). Comprueba que la línea {{DEC93_SOURCE}} declara el
   cuerpo pegado de §93 con 4 057 bytes y el SHA-256 de arriba; que el párrafo de las líneas {{DEC93_OBJECT}} identifica el objeto (`docs/initiatives/I-62-A-7.md`,
   commit de publicación `6fee8fd6…`, blob completo de arriba); y que los puntos 1-9 (líneas {{DEC93_P1}}-{{DEC93_P9}}) registran lo que dice `order.txt` (con la
   redacción del registro), y el punto 1 autoriza UNA revisión formal de A-7.
4. Lee con Read las líneas {{DEC90_AGREED}} del mismo archivo (decisiones §90, acuerdo registrado): A-6 AGREED sobre el blob `4bc919dd…`, el que cita la
   cabecera de A-7.
5. Lee entero, con Read, `{{CLONE}}\{{GRR}}` ({{GRR_LINES}} líneas): {{GRR_KEYS}} deben dar `A7Blob` = el blob del objeto, `Base` = el padre de la
   publicación (`{{PUB_PARENT}}`), `Head` = la publicación (abreviada, `6fee8fd6`), `Mode` run y `Result` PASS.
6. Lee con Read, en `{{CLONE}}\{{SUMMARY}}`, las líneas {{SUMMARY_CAND}} (`Candidate.Blob` = el blob del objeto), {{SUMMARY_HOLD}} (`Hold` = `Cases` y
   `NotHolding` vacío) y {{SUMMARY_ST}} (autoprueba de las guardas).

Si algo no coincide, o si un archivo del run o del objeto no se puede leer, para: `Verdict` = `"NOT_ACCREDITED"`, con el motivo en `KnownLimitations`.

## Cierre de insumos (solo esto)

**Archivos del run** (`{{RUN}}\`):
- `order.txt`: la orden vigente (disposición §93; 4 057 bytes, {{ORDER_LINES}} líneas). Su SHA-256 es el que declara decisiones §93 en el commit. Puede ser
  referencia (`order.txt:<línea>`);
- `prompt.md`: este mismo texto. No hace falta leerlo y nunca es referencia.

Todas las rutas que siguen son relativas al clon `{{CLONE}}` y están en el commit de arriba.

**Objeto**, entero: `docs/initiatives/I-62-A-7.md`: {{A7_MAP}}. Texto de A7-1: L{{A7_RULE}}; puntos {{A7_POINTS}}; categorías del punto 1: {{A7_CATS}}.
Tabla de categorías de §3.3: L{{A7_S33_T}}. Relación con A-6 y con R7-A: §3.4 (L{{A7_S34}}). Lo que A-7 no hace y el residuo declarado de `clausemap`: §3.5
(L{{A7_S35}}; residuo L{{A7_S35_RES}}). Materialidad: §5 (L{{A7_S5}}). Consecuencias reservadas al Owner: §7 (L{{A7_S7}}). Preguntas: §8 (L{{A7_S8}}; filas
L{{A7_Q}}).

**Preflight y guardas de A-7** ({{A7EVID_COUNT}} archivos, enteros; evidencia de apoyo, no autoridad: el constructor `make_eic_a7.py` y el validador
`check_a7_closure.py` del cierre de A7-1, la comprobación de cobertura de §3.5 `check_citation_blobs.py`, la reproducción sobre R7-A, el caso tipo B1 y
sus copias `b1-like/`, los negativos de entradas ausentes, reemplazadas y ajenas, la compatibilidad con A-5 y A-6, la comprobación estática de la
reconstrucción, las guardas G1-G7 con su autoprueba y el resultado `run`; `preflight.md` es su informe):

```text
{{A7EVID_TABLE}}
```

**Evidencia de la medición de A6-7 (`m7/`)** ({{M7_COUNT}} archivos, enteros: la determinación de §91 sobre I6, `i6-determination.md`, que A-7 cita como
origen; D-1 (`s91/d1/`) y las doce lecturas (`s91/r12/`), que §93, puntos 4 y 5, remiten; los borradores de §92 (`s92/`: la disposición limitada de la
orden `contract`, `contract/disposition-draft.md`; las propuestas de las lecturas, `readings/proposals.md`; y las notas del Principal,
`coordinator-notes.md`); del banco de la medición, su README, las designaciones y `p1-build.json`; y `readings/readings.md`, que define las lecturas
L-01..L-24 a las que remiten las propuestas FC):

```text
{{M7_TABLE}}
```

**Evidencia de la ronda 4 de A-6 que cita el preflight de A-7** (§5, negativos reutilizados por identidad). Los tres JSON son de una sola línea (más de
2 000 caracteres): lee la **copia legible** de `docs/automation/evidence/I-62-A6/r4/readable/` (misma ruta relativa; `readable/MANIFEST.json` fija, para
cada copia, el archivo canónico, su blob y la transformación) y **cita la ruta de la copia**:

```text
{{R4_TABLE}}
```

**Kit de la medición `a6m7-measure`** ({{KITM7_COUNT}} archivos, enteros; congelado y sin lanzar; A-7 §10 lo cita, y el preflight de A-7 usa su
`frozen/E.json`, su `reconstruct_runs_m7.py` y las líneas del Owner `line-b.txt` y `line-od2.txt`):

```text
{{KITM7_TABLE}}
```

**Paquete B-min** ({{PKG_COUNT}} archivos, enteros; el instrumento fijado es `{{TOOL}}` ({{TOOL_LINES}} líneas: sus órdenes, entre ellas `clausemap` y
`contract`) y el aceptante fijado es `{{ACCEPTOR}}` ({{ACCEPTOR_LINES}} líneas, por tramos de 300); las notas, las pruebas y los resultados):

```text
{{PKG_TABLE}}
```

**Canónicos.** Los rangos orientan, no limitan: puedes leer entero cualquier archivo de esta lista, por tramos de 300 líneas como máximo si pasa de
60 000 bytes.
{{CLOSURE_LIST}}

**Evidencia de I-62** (`docs/automation/evidence/I-62-evidence.md`, en la lista de canónicos; rangos de orientación: {{EVIDENCE_RANGES}}). Está en el cierre
porque A-7 cita la evidencia §181 y porque el revisor debe tener acceso directo a las fuentes que necesite citar. Es el registro de la sesión que
preparó A-7 y su preflight: contiene sus resúmenes y valoraciones (también de A-7). Úsala como evidencia de hechos que puedes contrastar con los
archivos custodiados y con el texto normativo, nunca como autoridad ni como juicio sobre A-7; no adoptes sus conclusiones sin comprobarlas. Lo mismo
vale para `preflight.md`, `i6-determination.md`, los borradores de `s92/` y `coordinator-notes.md`: son de la misma sesión.

Nada más. Quedan fuera del cierre, en particular: `docs/automation/evidence/I-62-C1C3/` (C1-C3 y la nota de aislamiento de I-65: ajenos a esta
revisión); el resto de `docs/automation/evidence/I-62-A6/` (las rondas 1-4 de A-6 salvo los tres archivos de arriba y sus copias legibles; de `m7/`,
todo lo que no está listado, incluidos `bench/inputs/` y los demás resultados del banco); los directorios de las revisiones anteriores; el repositorio
de la entrega del fixture FX-U1, el bundle `MAP-INVALID/candidate-b.bundle` y los clones temporales del preflight de A-7; WORKFLOW, FOUNDATIONS,
`state/I-62.yml`; las transcripciones y la memoria. Lo que A-7, su preflight o la evidencia citen de esos archivos no lo puedes comprobar: si una
conclusión tuya depende de ello, decláralo en `KnownLimitations`.

## Lista de lectura: fuentes que cita A-7 §2 (L1-L15)

Las guardas de A-7 (G3, en el resultado `run` custodiado) comprobaron que cada literal de A-7 §2 está en las líneas citadas del blob fijado de su
fuente. Úsala como lista de comprobación: antes de apoyarte en una cita de A-7 (una fila de §2, un bloque «> », una referencia «L…»), lee la fuente
en esas líneas con Read y contrástala.

{{LIT_CHECKLIST}}

## Reglas de lectura (el invocador audita después cada llamada)

**Regla de esta revisión.** Toda referencia `ruta:línea` o `ruta:inicio-fin` apunta solo a líneas que leíste con Read **en ese mismo archivo** y que te
llegaron fielmente. Una cita de una fuente dentro de A-7 no es una lectura de la fuente: para citar `docs/initiatives/I-62-proposal-v14.md:2233`
tienes que haber leído con Read la línea 2233 de ese archivo; si solo leíste la cita en A-7, cita la línea de A-7.

Cada ruta se comprueba por su **efecto**: resuelta contra el directorio de trabajo, con `.` y `..` colapsados y los enlaces resueltos.

- **Read:**
  - usa la ruta absoluta, solo de archivos del cierre bajo `{{CLONE}}` o de los dos archivos del run;
  - en los archivos de más de 60 000 bytes ({{LARGE_FILES}}), usa `offset` y `limit` de **300 líneas como máximo**;
  - haz una sola lectura por llamada. Si una lectura falla por el tamaño de su salida, repite ese rango en tramos más cortos.
- **Grep:** solo con `path` = un **archivo** de la lista (del cierre o del run), sin `glob` ni `type`. Nunca un directorio. Sin `path`, Grep busca en
  el directorio de trabajo, que es un directorio.
- **Glob:** no la necesitas, porque las rutas están arriba. Si la usas, su alcance (`path` y patrón) solo puede contener archivos de la lista
  (`docs/automation/evidence/I-62-A7/`, `docs/automation/evidence/I-62-A6/m7/s91/`, `docs/automation/evidence/I-62-A6/m7/s92/`,
  `docs/automation/evidence/I-62-A6/m7/bench/designations/`, el kit `a6m7-measure` y el directorio del paquete B-min están enteros en el cierre; el
  resto de `I-62-A6/` no). Un patrón que alcance cualquier otro archivo cuenta como lectura fuera del cierre, aunque no leas ese archivo.
- **StructuredOutput:** una sola vez, al final, con el resultado.
- **Prohibido:**
  - cualquier otra ruta del clon o del disco, en particular `~/.claude`, `~/.codex`, el repositorio de trabajo, otros clones `D:\r62-*`, las áreas
    de trabajo `D:\r62-a7\*`, el fixture (`D:\r62-fixture\*`) y el subdirectorio `launch\` del run;
  - red y web; subagentes u otros agentes; escribir.
- Un intento rechazado o fallido fuera del cierre sigue siendo una acción fuera del contrato, y una denegación de permiso se registra como violación.
- **Lecturas fallidas.** Una llamada que falla o no devuelve contenido no acredita nada y no puede sostener una referencia.
- **Líneas de más de 2 000 caracteres.** Read las entrega truncadas: {{LONG_LINES}}. Una línea truncada no cuenta como entregada: no la cites y, si
  te hiciera falta, decláralo en `KnownLimitations`. Para los tres JSON de una sola línea de `r4/r1probes/`, lee y cita la copia de `readable/` (arriba).
- **Contexto.** El cierre es grande: lee los rangos que necesites. Una compactación automática del contexto añade a la transcripción un mensaje que el
  auditor cuenta como un segundo mensaje de usuario, y la corrida queda sin acreditar.
- **Unicode.** El texto canónico es UTF-8, con caracteres como ≤ ≥ ≠ → ∖ ∩ ∪ « » ^ y acentos.
  - Si una salida llega truncada, con «?», con «=» en lugar de ≤ o ≥, o con U+FFFD, no la uses: vuelve a leer ese rango.
  - Si no puedes obtenerla fiel, decláralo en `KnownLimitations`.
  - Un artefacto del transporte nunca es un hallazgo técnico.

## Referencias (`Evidence`, `ClauseRefs` y `DirectReads`)

- Una referencia es `ruta:línea` o `ruta:inicio-fin`: la ruta, relativa al clon para los archivos del cierre (p. ej.,
  `docs/initiatives/I-62-A-7.md:{{A7_RULE}}`), y el nombre suelto para el archivo del run que puede ser premisa (`order.txt:12`). Sin rutas absolutas ni
  barras invertidas. `prompt.md` nunca es referencia.
- Cita solo líneas que hayas leído enteras con Read en ese archivo y que te llegaron fielmente. El invocador comprueba que Read te entregó cada línea
  citada, también en `DirectReads` y en los `ClauseRefs` de los elementos anidados.
- **Números de línea exactos:** los que Read muestra a la izquierda. Una referencia desplazada, aunque sea en una línea, no se acredita. Antes de
  entregar, contrasta cada referencia con la salida de Read de la que sale.

## Qué decidir

Se pide el veredicto formal sobre la A-7 exacta (LIFECYCLE §5 y §6), acotado a A7-1 y a sus consecuencias (decisiones §93, punto 1). Contesta con el
texto normativo del cierre y con lo que leas.

- **`Verdict`:** `AGREED` (cero REQUIRED abiertos sobre la versión exacta), `CHANGES REQUIRED` (al menos un REQUIRED, corregible dentro del alcance de
  decisiones §92, punto 2 (línea {{DEC92_P2}}), y §93), `REJECTED` (al menos un REQUIRED y la corrección no es admisible dentro de ese alcance: necesitaría otra disposición)
  o `BLOCKED — OWNER DECISION` (falta una autoridad reservada: una consecuencia OWNER-RESERVED que A-7 no identifica, o una que no esté dispuesta; A-7
  §7). Un REQUIRED impide el acuerdo hasta su disposición y corrección.
- **Hallazgos.** Un hallazgo es REQUIRED en los casos de LIFECYCLE §5 (activa M-01..M-08 sobre un elemento congelable, deja un invariante sin
  obligación de prueba, produce comportamiento observable distinto, o revela un elemento materialmente incorrecto, no ejecutable o no verificable);
  una precisión sin cambio de significado es OPTIONAL. Ids de esta revisión: REQUIRED `A62-A7-R1`, `A62-A7-R2`…; OPTIONAL `A62-A7-O1`…. Cada uno
  con la sección exacta de A-7 (`Section`), su evidencia (`Evidence`: al menos una referencia) y la corrección (`CorrectionRequired` en un REQUIRED,
  `SuggestedCorrection` en un OPTIONAL). Un REQUIRED trae además la autoridad o el invariante violado (`ViolatedInvariant`) o un contraejemplo concreto
  (`Counterexample`), al menos uno no vacío, y por qué importa (`WhyItMatters`). Un hallazgo es siempre sobre A-7.
- **`TopicVerdicts`** (decisiones §93, punto 3, línea {{DEC93_P3}}), una vez cada tema, con `Status` `CONFORMS` (A-7 satisface el tema; no cita ningún REQUIRED)
  o `DOES_NOT_CONFORM` (cita al menos un REQUIRED), `Rationale`, `ClauseRefs` y `FindingIds`:
  - `T01_A71_VS_AUTHORITIES`: A7-1 frente a V14 B.9 (L2231, L2233, L2250-L2251), AUTOMATION_PLAN 16.13 (Resolve y Validate) y 16.24 (cierre y
    P-22), RAE §15.2 I6 y A-6 (A6-2, reglas 1 y 3, L{{A6_A62_R1}} y L{{A6_A62_R3}}; A6-3, reglas 4 y 6, L{{A6_A63_R4}} y L{{A6_A63_R6}}; A6-7, reglas 5 y 7, L{{A6_A67_R5}} y
    L{{A6_A67_R7}}; A6-9, regla 4, L{{A6_A69_R4}}, y su tabla, L{{A6_DERIV}}): si la ubicación y la forma de A7-1 (una A-n posterior que precisa su ámbito sin
    editar el texto de B.9, 16.24, RAE §15 ni A6-2; A-7 §3.4) es admisible, y si el resultado es coherente con esas cláusulas;
  - `T02_CATEGORIES`: si las cuatro categorías (autoridades, objetos del `Target`, artefactos custodiados, instrumento; A7-1, punto 1, y §3.3) son
    completas y cerradas para las lecturas que la verificación necesita, sin admitir insumos que no necesita;
  - `T03_PROVENANCE_REVISION_IDENTITY_FIDELITY`: para cada categoría, el origen, la revisión contra la que se comprueba su blob (I6), la identidad y el
    camino de fidelidad (A7-1, puntos 2-4), y la regla de rutas de la designación (punto 2 (c));
  - `T04_EFF_EFF1_B1_MAP_VALIDATION`: la fijación de las lecturas de Resolve y Validate en EFF y EFF^1 (A7-1, punto 2 (a); AUTOMATION_PLAN 16.13,
    MV-3, MV-4, MV-6 y MV-7) y su efecto en la validación de mapas de un fixture B1 construido según A5-1 (A-7 §3.1 y el caso tipo B1 del preflight
    sostienen que allí los blobs de EFF^1 difieren de los de la `AuthorityRevision`);
  - `T05_MAP_INVALID_ABSENT_NONCONSTRUCTIBLE`: la distinción entre MAP_INVALID (16.13), una entrada inexistente que no aporta entrada y un cierre que no se
    puede construir (A7-1, punto 5), y que ninguna ausencia se sustituya;
  - `T06_CLAUSEMAP_COVERAGE_RULE`: el residuo declarado de §3.5 (las lecturas de `clausemap` en revisiones que 16.13 no lee) y su comprobación de
    cobertura de fallo cerrado antes de lanzar (S-03), frente al instrumento fijado y al aceptante;
  - `T07_MULTI_CATEGORY_ENTRIES`: las entradas que pertenecen a varias categorías (A7-1, punto 1: el blob coincide con la referencia de cada una) y las
    rutas con un blob por revisión de lectura (punto 2 (a));
  - `T08_COMPATIBILITY_A5_A6`: la compatibilidad con A-5 (A5-1, A5-2) y A-6 (A6-2, A6-3, A6-7, A6-9 y las líneas del Owner de A-6 §7), sin editar su
    texto ni alterar sus acuerdos;
  - `T09_NO_UNAUTHORIZED_CHANGES`: la ausencia de cambios no autorizados en contratos (esquemas, `rackcad-input-closure/v1`, `rackcad-role-invocation/v1`),
    oráculos (C-24, A6-6; el relativo de RAE §10), presupuestos (filas de D.3; medición única de A6-7) y producción;
  - `T10_APPLICABILITY_N4_NC1_N5_N7`: la aplicabilidad de A7-1 a N4, nc1 y N5-N7 (controles negativos con invocación; RAE §10; `controller-contracts.md`
    L23; designaciones del banco) y a la invocación de medición de A6-7;
  - `T11_NEGATIVES_AND_PREFLIGHT`: la suficiencia de los negativos (entradas ausentes, reemplazadas y ajenas) y del preflight de A-7 (decisiones §92,
    punto 7, línea {{DEC92_P7}}) para sostener A7-1. Lo que la evidencia no ejecutó no lo das por ejecutado.
- **`Materiality`** (decisiones §93, punto 3: «Dictaminar expresamente la materialidad»; LIFECYCLE §3; A-7 §5, L{{A7_S5}}): en `Triggers`, una vez cada
  disparador `M-01`..`M-08`, con `A7Declared` (lo que declara la columna «Activado» de A-7 §5: `yes` para **sí**, `no` para no) y `Assessment`
  (`AGREE` si coincides con esa declaración; `DISAGREE` si no, y entonces cita un REQUIRED); `Rationale`, `ClauseRefs` y `FindingIds` en cada disparador y
  en el conjunto.
- **`OwnerReservedConsequence`** (decisiones §93, punto 3; LIFECYCLE §3, OWNER-RESERVED; A-7 §7, L{{A7_S7}}): `OwnerDecisionRequired` (true si identificas
  una consecuencia OWNER-RESERVED que A-7 no identifica o que no está dispuesta; false si no) y `UnidentifiedConsequences` (cada una; vacía con false),
  con `Rationale`, `ClauseRefs` y `FindingIds`. Las líneas del Owner constan (decisiones §78, línea {{DEC78_LINE}}; A-6 §7, L{{A6_LINE_A}} y L{{A6_LINE_B}}) y la (b)
  solo rige la aplicación: ninguna de las dos cuenta aquí.
- **`RelatedImpacts`** (separado del veredicto sobre A-7):
  - `ContractExclusionD1` (decisiones §93, punto 4, línea {{DEC93_P4}}; §92, punto 4, línea {{DEC92_P4}}; §91, punto 3, línea {{DEC91_P3}}; U-19 en decisiones §62,
    líneas {{DEC62_U19}}; borrador `{{S92_DRAFT}}`, notas `{{S92_NOTES}}`, D-1 en `s91/d1/`): `Status` `NO_CONCERN` o `CONCERNS_FOR_COORDINATOR` (con cada
    preocupación en `Concerns`; vacía con NO_CONCERN), y `RevealsA7Defect` (true si el examen revela un defecto de A-7: entonces cita un REQUIRED sobre
    A-7; false si no, y no cita ningún REQUIRED). El recálculo por la supervisión del punto 7 del borrador no se incorpora (§93, punto 4);
  - `FailClosedProposals` (decisiones §93, punto 5, línea {{DEC93_P5}}; §92, punto 5, línea {{DEC92_P5}}, y su registro, {{DEC92_REG}}; propuestas `{{S92_PROPOSALS}}`;
    clasificación de §91 en `s91/r12/r12.md`; lecturas en `m7/readings/readings.md`), una vez cada propuesta `FC-01`, `FC-04`, `FC-10`, `FC-12`,
    `FC-13`, `FC-16`, `FC-24` y `FC-18`: `ChangesPassCriteria` (si cambia indirectamente un criterio normativo de PASS), `ExcludesValidC24Cases` (si
    excluye casos válidos necesarios para C-24), `ReliesOnImplementationCoincidence` (si su aceptación descansa en la mera coincidencia con la
    implementación), `Status` (`CONCERNS_FOR_COORDINATOR` si alguna de las tres es true; `NO_CONCERN` si las tres son false) y `RevealsA7Defect` (como
    arriba), con `Rationale`, `ClauseRefs` y `FindingIds`;
  - `Note`: lo que quieras añadir para el Coordinator sobre estos impactos.
- **`QuestionAnswers`** (A-7 §8, filas L{{A7_Q}}): `Q-A7-01`..`Q-A7-06`, una vez cada una: `Answer` (tu respuesta) y `Outcome` (`SATISFIED` si la
  respuesta no exige ningún cambio de A-7, `NOT_SATISFIED` si lo exige; un NOT_SATISFIED cita al menos un hallazgo), con `ClauseRefs` y `FindingIds`.
- **`DirectReads`:** `{Ref, Purpose}` de cada fuente que leíste directamente para comprobar una cita de A-7. Incluye siempre
  `docs/initiatives/I-62-proposal-v14.md:{{V14_DIRECT}}` (Paso 0) y, como mínimo recomendado, una entrada por fuente de la lista de A-7 §2 en la que te apoyes.
- **Además:**
  - tu modo de revisión, si revisor y autor son la misma persona y el contexto que el runtime te inyectó (`ReviewerMode`, `SamePersonAsAuthor`,
    `InjectedContextDeclaration`). Declara también el proveedor del revisor y del autor en `SamePersonAsAuthor`. Los identificadores de cuenta que el
    runtime te inyecte (correo, uuid de organización u otros) se declaran solo por su tipo, sin reproducir su valor;
  - la identidad revisada (`ReviewedCommit`, `ReviewedPath`, `ReviewedBlob`);
  - lo que leíste (`Reads`: «ruta: rangos») y la acción siguiente que recomiendas (`RecommendedNextAction`).

Para la medición de A6-7 y la prueba previa de A6-5, regla 6 (L{{A6_A65_R6}}), y para las disposiciones del Coordinator previas a la medición (A-7, Anexo),
vale lo dicho arriba sobre las precondiciones: se juzgan como precondiciones con fallo cerrado.

## Resultado

Entrega el resultado con **una** llamada a StructuredOutput, con este objeto. Lo valida un esquema estricto: no añadas campos.

```json
{
  "Schema": "rackcad-a7-architect-review/0",
  "RunId": "{{RUN_ID}}", "InvocationId": "{{INVOCATION_ID}}", "LogicalReviewRequestId": "{{LOGICAL_ID}}", "AttemptSeq": 1,
  "RequestedRole": "ARCHITECT", "Action": "REVIEW_DESIGN", "ReviewedUnit": "I-62",
  "ReviewedCommit": "...", "ReviewedPath": "docs/initiatives/I-62-A-7.md", "ReviewedBlob": "...",
  "ReviewerMode": "SAME-SESSION ROLE | SEPARATE SESSION | EXTERNAL HUMAN",
  "SamePersonAsAuthor": "texto: relación entre el revisor, la sesión autora de A-7 y el operador humano; proveedor del revisor y del autor",
  "ReviewerDeclaredIdentity": {"Runtime": "... o UNKNOWN", "Model": "... o UNKNOWN", "Effort": "... o UNKNOWN", "SessionOrThread": "... o UNKNOWN"},
  "InjectedContextDeclaration": {"State": "DECLARED | NONE", "Items": ["todo lo que el runtime te inyectó antes de tu primera acción; los identificadores de cuenta, solo por su tipo"]},
  "Step0": {"Basis": "INVOKER_VERIFIED_CLONE", "V14DirectReadB9L2231L2233": "true | false | null", "OrderReadWhole": "true | false | null",
            "Decisions93DeclaresOrderBytesAndSha256": "true | false | null", "Decisions93IdentifiesObjectAndAuthorizesReview": "true | false | null",
            "Decisions90RecordsA6AgreedBlob": "true | false | null", "GuardsRunModeBaseHeadResult": "true | false | null",
            "PreflightSummaryCandidateAndTotals": "true | false | null", "Note": "..."},
  "DirectReads": [{"Ref": "docs/initiatives/I-62-proposal-v14.md:{{V14_DIRECT}}", "Purpose": "..."}],
  "Reads": ["ruta: rangos"],
  "Verdict": "AGREED | CHANGES REQUIRED | REJECTED | BLOCKED — OWNER DECISION | NOT_ACCREDITED",
  "RequiredFindings": [{"FindingId": "A62-A7-R1", "Section": "...", "ViolatedInvariant": "...", "Counterexample": "...", "Evidence": ["ruta:inicio-fin"],
                        "WhyItMatters": "...", "CorrectionRequired": "..."}],
  "OptionalFindings": [{"FindingId": "A62-A7-O1", "Section": "...", "Note": "...", "Evidence": ["ruta:línea"], "SuggestedCorrection": "..."}],
  "TopicVerdicts": [{"Topic": "T01_A71_VS_AUTHORITIES | … | T11_NEGATIVES_AND_PREFLIGHT", "Status": "CONFORMS | DOES_NOT_CONFORM", "Rationale": "...",
                     "ClauseRefs": [], "FindingIds": []}],
  "Materiality": {"Triggers": [{"Trigger": "M-01 | … | M-08", "A7Declared": "yes | no", "Assessment": "AGREE | DISAGREE", "Rationale": "...",
                                "ClauseRefs": [], "FindingIds": []}],
                  "Rationale": "...", "ClauseRefs": [], "FindingIds": []},
  "OwnerReservedConsequence": {"OwnerDecisionRequired": "true | false | null", "UnidentifiedConsequences": [], "Rationale": "...", "ClauseRefs": [],
                               "FindingIds": []},
  "RelatedImpacts": {"ContractExclusionD1": {"Status": "NO_CONCERN | CONCERNS_FOR_COORDINATOR | NOT_DETERMINED", "RevealsA7Defect": "true | false | null",
                                             "Concerns": [], "Rationale": "...", "ClauseRefs": [], "FindingIds": []},
                     "FailClosedProposals": [{"Proposal": "FC-01 | FC-04 | FC-10 | FC-12 | FC-13 | FC-16 | FC-24 | FC-18", "ChangesPassCriteria": "true | false",
                                              "ExcludesValidC24Cases": "true | false", "ReliesOnImplementationCoincidence": "true | false",
                                              "RevealsA7Defect": "true | false", "Status": "NO_CONCERN | CONCERNS_FOR_COORDINATOR", "Rationale": "...",
                                              "ClauseRefs": [], "FindingIds": []}],
                     "Note": "..."},
  "QuestionAnswers": [{"Question": "Q-A7-01 | … | Q-A7-06", "Outcome": "SATISFIED | NOT_SATISFIED", "Answer": "...", "ClauseRefs": [], "FindingIds": []}],
  "IfAgreed": {"ScopeLimitedToA71AndConsequences": null, "A71CompatibleWithAuthorities": null, "AllMandatoryTopicsConform": null,
               "MaterialityConfirmed": null, "NoUnidentifiedOwnerReservedConsequence": null, "NoUnauthorizedChanges": null,
               "RelatedImpactsRevealNoA7Defect": null, "SuitableForCoordinatorAgreement": null},
  "KnownLimitations": ["..."],
  "RecommendedNextAction": "..."
}
```

- Los valores de la plantilla son formas, no respuestas: cada campo lleva **tu** comprobación y **tu** evaluación. Donde la plantilla dice
  `"true | false | null"` o `"true | false"`, el valor es un booleano JSON (o `null`), no un texto.
- `Step0`: las siete booleanas son lo que compruebas en el Paso 0 (la primera, la lectura directa de V14 L{{V14_DIRECT}}); `null` en lo que no compruebes.
- `DirectReads` incluye siempre V14 L{{V14_DIRECT}}, también si te detienes en el Paso 0.
- Cobertura, una vez cada elemento: `TopicVerdicts`, los once temas; `Materiality.Triggers`, M-01..M-08; `RelatedImpacts.FailClosedProposals`, las ocho
  propuestas; `QuestionAnswers`, Q-A7-01..06.
- Todo id de `FindingIds` existe en `RequiredFindings` u `OptionalFindings`; los ids no se repiten.
- **Con un veredicto** (todo salvo NOT_ACCREDITED):
  - las siete booleanas de `Step0` en `true`; `ReviewedCommit`, `ReviewedPath` y `ReviewedBlob` = el commit, la ruta y el blob de arriba;
  - `A7Declared` de cada disparador = lo que declara A-7 §5;
  - al menos una `ClauseRef` en cada elemento de `TopicVerdicts`, `Materiality.Triggers`, `RelatedImpacts.FailClosedProposals` y `QuestionAnswers`, y en
    `Materiality`, `OwnerReservedConsequence` y `RelatedImpacts.ContractExclusionD1`;
  - ningún `NOT_DETERMINED` y ningún `null` en `ContractExclusionD1` (`Status`, `RevealsA7Defect`) ni en `OwnerDecisionRequired`;
  - todo REQUIRED citado en `FindingIds` por al menos un elemento evaluado.
- **Con AGREED:** cero REQUIRED; los once temas en CONFORMS; los ocho disparadores en AGREE; `OwnerDecisionRequired` = false; `RevealsA7Defect` = false
  en `ContractExclusionD1` y en las ocho propuestas; y los ocho campos de `IfAgreed` en `true`. Las preocupaciones de `RelatedImpacts` que no revelan un
  defecto de A-7 no impiden el acuerdo. En cualquier otro caso, `IfAgreed` va todo en `null`.
- **Con CHANGES REQUIRED o REJECTED:** al menos un REQUIRED. **Con BLOCKED — OWNER DECISION:** `OwnerDecisionRequired` = true. Con
  `OwnerDecisionRequired` = true, `UnidentifiedConsequences` no vacía.
- **Sin acreditación posible** (identidad, fidelidad o insumos): no fabriques un veredicto. Pon `Verdict` = `"NOT_ACCREDITED"` y el motivo exacto en
  `KnownLimitations`.
  - `TopicVerdicts`, `Materiality.Triggers`, `RelatedImpacts.FailClosedProposals`, `QuestionAnswers` y las listas de hallazgos pueden ir vacías.
  - Lo no evaluado va así: `NOT_DETERMINED` en `ContractExclusionD1.Status`; `null` en `ContractExclusionD1.RevealsA7Defect`, en `OwnerDecisionRequired`,
    en `IfAgreed` y en las booleanas de `Step0` que no comprobaste; `ReviewedCommit` y `ReviewedBlob` con lo observado o en `null`; los `Rationale`, «no
    evaluado».
  - `NOT_DETERMINED` y `null` en esos campos solo valen con NOT_ACCREDITED.
