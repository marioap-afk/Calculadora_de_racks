# I-47 — Proposal V4: contrato de variables de proyecto (ID22A)

> **Documental. No autoriza implementacion.** No toca `src/` ni `tests/`. **No se escribe el ADR
> todavia.** **No se declara `Coordinator=AGREED` ni `Architect=AGREED`.**
>
> ```
> PROPOSAL VERSION:   V4.3
> Sustituye a:        I-47-proposal-v3.md  (commit 4470c6b)   — V1 y V2 ya supersedidas por V3
> Base del analisis:  9e25d5291daa13c4846112241be6e52526429a47  (Discovery)
> origin/main:        306e18ed4676e5e96b54d59402c9a230efb137d3  (sin avanzar)
> Premisas:           C-1..C-6 (Gate C) + C2-1..C2-9 (Gate C2) + C3-1..C3-8 (Gate C3)
>                     + C4-1..C4-14 (Reconciliacion V4)
> ```
>
> ## Reconciliacion
>
> El **Architect Review sobre V3 fue `DISAGREED`** (4 BLOCKER, 5 HIGH, 7 MEDIUM, 4 LOW).
> **El Coordinador retira su `AGREED` sobre V3 y acepta los findings materiales.** V4 los cierra.
>
> Dos de los cuatro BLOCKER no eran defectos de redaccion: eran **decisiones ya ratificadas que no se
> podian implementar contra el arbol real** — la promocion pegajosa no tenia portador (B2) y la
> frontera transaccional estaba nombrada sobre la clase equivocada para las vistas laterales del
> propio slice (B3). Un tercero, B1, era una **operacion no definida**: nada gobernaba el acto de
> **vincular**. El cuarto, B4, era el **punto de resolucion** del valor efectivo, sin el cual dibujo y
> BOM pueden discrepar.
>
> La correspondencia completa finding → resolucion esta en **§0-ter**.
>
> ## V4.1 — un BLOCKER mas, encontrado sobre la superficie de V4
>
> Al revisar **lo que V4 anadia**, el Coordinador encontro **V4-01**: el contrato asume que **todas las
> vistas de un mismo `RackId` cargan el mismo `Design` completo**, y **no dice que hacer si ya estan
> divergentes**. Ese estado es **alcanzable** —por commits parciales historicos— y **no** se puede
> resolver eligiendo una vista arbitraria. Se corrige en **D-12 §autoridad authored multi-vista** y se
> propaga a D-13 y a D-17. El detalle esta en **§0-quater**.
>
> ## V4.2 — Architect Review de V4.1: `DISAGREED`, y su reconciliacion
>
> El **Architect Review sobre V4.1 fue `DISAGREED`** (1 BLOCKER, 1 HIGH, 1 MEDIUM, 1 LOW). El
> Coordinador **retira su AGREED sobre V4.1** y **acepta los cuatro findings**, modificando
> parcialmente la correccion propuesta para N2.
>
> - **N1 (BLOCKER)** — el «Paso 0» de V4.1 exigia igualdad authored completa **antes** de clasificar
>   consumidores, y eso **contradice C4-13/M4**: un rack divergente pero **ajeno** bloquearia toda
>   operacion de variable. Ademas aplicaba un tipo del Selectivo a *cualquier* `RackId`. Se corrige
>   distinguiendo **dos familias de operacion** (§0-quinquies y D-12).
> - **N2 (HIGH)** — D-04-bis decidia el punto unico de resolucion sin nombrar las costuras que fuerza.
>   Se aceptan y se nombran, **con una modificacion del Coordinador**: `RACKLISTA` **no** entra.
> - **N3 (MEDIUM)** — el `MutationPlan` de la prueba 17 no existia como artefacto. Se define en D-13.
> - **N4 (LOW)** — referencias editoriales obsoletas. Corregidas.
>
> El detalle esta en **§0-quinquies**.
>
> ## V4.3 — dos interacciones que V4.2 dejo abiertas
>
> V4.2 cierra N1-N4, pero al revisar **lo que V4.2 anadia** aparecieron dos consecuencias suyas:
>
> - **N5 (BLOCKER)** — V4.2 le da a `BuildBom` el registro y a `RACKBOMTOTAL` un snapshot unico, pero
>   el comando **sigue cotizando desde UNA sibling arbitraria**: agrupa por `RackId` conservando el
>   **primer** envelope que encuentra. Y **V4-01 ya declaro alcanzable** que las hermanas diverjan. Un
>   snapshot correcto del registro aplicado a un authored arbitrario **sigue cotizando mal**.
> - **N6 (HIGH)** — D-12 quedo corregido en V4.2, pero **D-13 conservaba el viejo «Paso 0»** como si
>   la igualdad authored fuera universal y anterior a la clasificacion. Los dos textos se contradecian.
>
> El detalle esta en **§0-sexies**. **N1-N4 siguen cerrados** y **no se reabre** ninguna decision
> C4 ni C4.2.

## 0. Premisas — no se comparan

Las seis decisiones del dueno son **entradas**, no opciones. Ninguna alternativa de este documento las
contradice; cuando una alternativa razonable queda excluida **por** una premisa, se dice cual y por
que, en vez de omitirla.

| # | Premisa |
|---|---|
| C-1 | DWG sin `ProjectVariables` = **registro vacio**; **sin migracion**; **literales preservados** |
| C-2 | Propiedad = **`Literal`** o **`ProjectVariableReference`**; la referencia **gobierna el valor efectivo** |
| C-3 | Cambiar una variable **propaga y redibuja todos sus consumidores en UNA operacion** |
| C-4 | **RACKDUPLICAR conserva el mismo `VariableId`** |
| C-5 | **ID22A no unifica** `ClearHeight` de otros sistemas; el slice es **Selectivo** |
| C-6 | **`defaults.json` convive**; **no** se convierte en `ProjectVariables` |

Diferido por el dueno: **WBLOCK y copia entre dibujos**. Minimo exigible: **guardar, cerrar y
reabrir el mismo DWG** (§8, D-18).

### Premisas anadidas en el Gate C2

| # | Premisa |
|---|---|
| C2-1 | **ID22B = formulas/expresiones entre variables.** No se disenan aqui |
| C2-2 | **ID21 = referencias a propiedades de racks**, previstas como **`RackId` + `PropertyId`**. No se disena aqui |
| C2-3 | Se separan tres conceptos distintos: **`ProjectVariable`**, **`PropertyValue<T>`** y **`PropertyId`** (§1-bis) |
| C2-4 | La referencia persistida es **tipada**; ID22A acepta **solo** `kind: projectVariable` |
| C2-5 | `[JsonExtensionData]` anadido ahora **no** protege frente a una version pre-I-47 **ya compilada** |
| C2-6 | `SystemBlockWriter.RedrawInPlace` **commitea por bloque**: el lote **no** es todo-o-nada |
| C2-7 | Un round-trip del Store **no** prueba `SelectiveKindHandler.RestampDesign` |
| C2-8 | La transferencia/fusion de variables entre dibujos **no** pertenece a ID22B: es futuro separado |
| C2-9 | El contrato y la fila de ROADMAP deben actualizarse **antes de produccion** si UI/comando forman parte del plan final (§14) |

### Premisas anadidas en el Gate C3

| # | Premisa |
|---|---|
| C3-1 | **D-07 queda decidido: F2.** F3 se **rechaza** por una razon semantica, no de formato (D-07) |
| C3-2 | F2 se corrige a **promocion de schema PEGAJOSA**, no «major segun contenido actual» (D-07) |
| C3-3 | **D-10 queda decidido: F1.** Borrar una variable con consumidores **se bloquea** y se listan; «desvincular todos materializando + borrar» solo como **accion explicita separada** |
| C3-4 | `RedefineSystemBlock` usa la **`Transaction` del llamador**; transaccion **unica** para `ProjectVariables` + geometria + payloads |
| C3-5 | **`BlockLibraryImporter.EnsureForPlan` NO forma parte del preflight ni del lote**: puede mutar el `Database` **fuera** de la transaccion. La propagacion **verifica** que las definiciones ya existan y **aborta** si falta alguna; **no importa ni repara** la biblioteca como efecto colateral |
| C3-6 | La **viabilidad** de la transaccion unica **deja de ser pregunta de contrato**. La validacion real en AutoCAD es **gate de implementacion / Owner Validation**, no condicion previa de Consensus Freeze |
| C3-7 | **WORKFLOW §2 manda sobre CF-2**: el **contrato** de iniciativa si refleja el alcance final antes de produccion; **ROADMAP no se modifica en un gate intermedio** y se actualiza **al integrar/cerrar**. ROADMAP sale del Consensus Freeze y pasa al **checklist de cierre** |
| C3-8 | Se retiran de «preguntas abiertas»: F2 vs F3, D-10, viabilidad conceptual de la transaccion unica, y asignar numero a H3. El **coste de propagacion** queda como **riesgo/metrica de implementacion**, no como decision de arquitectura |

### Premisas anadidas en la Reconciliacion V4

| # | Premisa |
|---|---|
| C4-1 | **LINK queda definido** (B1). Sobre un rack existente: conservar el literal **authored**, anadir la `ProjectVariableReference`, **promover** el schema, resolver el efectivo y redibujar **todas sus vistas** por el camino de D-13. **El binding gobierna de inmediato.** Sobre un rack **sin vistas todavia**: se guarda el binding y las inserciones futuras usan el efectivo |
| C4-2 | **El portador del estado authored es el `SelectivePalletDesignDocument` deserializado, NO el Domain** (B2). Entre load/edit/save debe preservar `SchemaVersion`, `PropertyValues`, el **literal authored** y `ExtensionData`. El guardado **no puede** reconstruirse solo con `From(SelectivePalletDesign…)` |
| C4-3 | **D-13 cubre TODOS los writers del slice** (B3): los caminos de `SystemBlockWriter` **y** `LateralHeaderDrawService.RedrawInPlace`. La frontera transaction-aware debe permitir redefinir **frontal, planta y lateral** bajo la transaccion del llamador. `EnsureForPlan`/import **prohibido** dentro de la propagacion; verificacion **read-only** de definiciones antes de mutar |
| C4-4 | **UN solo punto de resolucion, en Application** (B4): `SelectiveEffectiveDesignResolver.Resolve(authoredDocument, projectVariables) -> SelectivePalletDesign efectivo`. Dibujo, BOM y preview consumen **unicamente** el diseno efectivo. La UI recibe **valor efectivo + estado authored del binding** |
| C4-5 | **Literal authored, mientras haya binding**: el `VerticalClearance` persistido es un literal **congelado e INACTIVO**. Un cambio de variable **no** lo sobrescribe. `RACKEDITAR` Save con binding activo **no** copia el textbox efectivo al literal |
| C4-6 | **UNLINK sano**: resolver el efectivo actual → escribirlo al literal → quitar el binding → el schema **permanece pegajoso** → redibujar. **Efecto geometrico nulo** |
| C4-7 | **Referencia rota**: en la resolucion normal, **error visible y sin fallback**. Se anade **reparacion explicita**: se puede desvincular usando el **literal authored almacenado**, avisando de que **no existe valor efectivo** y de que la geometria **puede cambiar**; solo por accion explicita; quita el binding, **conserva el schema 2.x** y redibuja. Es **reparacion, no fallback silencioso** |
| C4-8 | **`ProjectVariablesDocument` tiene contrato propio**: `SchemaVersion` desde el dia 1 (`1.0`), store y guard propios, `ExtensionData`, **major superior = error**, `VariableType` desconocido = **error**, `Definition.kind` desconocido = **error**. **Nunca ignorar datos que el build no entiende.** ID22B decidira su evolucion |
| C4-9 | **Sticky del Selectivo como invariante monotonica**: `stored major > read major soportado` ⇒ **ERROR**, nunca escribir ni degradar · `stored 2.x` ⇒ permanece `2.x` preservando el minor mayor · `stored 1.x` + `PropertyValues` ⇒ `2.0` · `stored 1.x` sin `PropertyValues` ⇒ linea `1.x`. `hasPropertyValues` se evalua sobre el **authored document a escribir**. **Read support de I-47 = `2.x`.** Portador = authored document, no Domain |
| C4-10 | **Biblioteca `.rackcad.json`**: el export es un **artefacto NUEVO**. Materializa el efectivo, **elimina `PropertyValues`**, escribe `SelectiveRack` **literal-only en linea `1.x`** y **no hereda** el schema pegajoso del rack embebido. Se anade el `SchemaGuard` anidado que falta para `RackProjectDocument.SelectiveRack` **en I-47+**, sin afirmar que proteja binarios pre-I-47. **ID22A nunca exporta un `SelectiveRack` promovido o vinculado** |
| C4-11 | **Unidad de atomicidad del registro**: un Xrecord contiene todo el `ProjectVariablesDocument`, pero la unidad **logica** es **una operacion confirmada** — `Create` / `Rename` / `ChangeValue` / `Delete` / `Link` / `Unlink` / `RepairBroken` / `UnlinkAllAndDelete`. Si una UI futura entrega varios cambios en un Apply, **el change-set entero es una sola unidad** y usa la **union** de consumidores. **Nunca apply parcial** |
| C4-12 | **Frontera Plugin → UI**: la UI **no conoce AutoCAD**. El Plugin lee el NOD y `ScanEnvelopes` y entrega **DTOs puros**: variables, estado de binding, valor efectivo, resumenes de consumidores y estado roto. La UI devuelve **peticiones/intenciones**; Plugin y Application ejecutan |
| C4-13 | **Descubrimiento de consumidores**: **no** filtrar por `DirectReferenceCount` —una definicion vinculada con 0 referencias **sigue siendo consumidor**—; **no** bloquear una operacion por un rack corrupto **no relacionado**; identificar **positivamente** los consumidores del `VariableId` objetivo mediante un **probe puro** de `PropertyValues`; consumidor confirmado + illegible/irresoluble ⇒ **abortar**; sin referencia al objetivo ⇒ **ignorar** para esa operacion |
| C4-14 | **`PropertyId`**: token canonico **exacto**, comparacion **`Ordinal`**, slice `selective.verticalClearance`. Un `PropertyId` **desconocido** presente en `PropertyValues` es **error visible**, nunca caida silenciosa al literal |

## 0-bis. Tabla V3 → V4

Solo lo que cambia. Lo no listado se conserva **literalmente** de V3.

| Decision | V3 decia | V4 dice | Cierra |
|---|---|---|---|
| **D-04** | El literal «se conserva y es lo que D-11 devuelve al desvincular» | **Contradiccion eliminada.** El literal es **authored, congelado e inactivo** mientras hay binding; el desvinculado escribe el **efectivo** (D-11). Se separan `authored` y `effective` como conceptos con nombre | B4, M7 |
| **D-04-bis** | — | **NUEVA.** `SelectiveEffectiveDesignResolver`: **un solo** punto de resolucion en Application. Dibujo, BOM y preview consumen **solo** el efectivo | B4 |
| **D-01-bis** | — | **NUEVA.** Contrato propio del `ProjectVariablesDocument`: version, store, guard, `ExtensionData`, y **tres errores duros** (major, `VariableType`, `Definition.kind`) | H2 |
| **D-07** | Helper `ResolveStickyWriteVersion(stored, hasPropertyValues)`, sin decir de donde sale `stored` | **Portador nombrado**: el authored document deserializado. Invariante **monotonica** con rama de **ERROR** para major superior. Read support `2.x`. Garantia de F2 **acotada al camino embebido** | B2, H1, M5 |
| **D-08-bis** | — | **NUEVA.** Reparacion explicita de una referencia rota, usando el literal authored, con aviso de cambio geometrico | H3 |
| **D-11-bis** | — | **NUEVA — LINK.** La operacion que V3 no definia | B1 |
| **D-11** | Materializar el efectivo (correcto) | Sin cambio de decision; se explicita la secuencia y el efecto geometrico nulo | — |
| **D-12** | Aborta ante cualquier bloque sin `Id`/`Kind` o illegible | **Probe positivo** por `VariableId`; **sin** filtro de `DirectReferenceCount`; un rack corrupto **no relacionado** no bloquea | M1, M4 |
| **D-13** | Frontera sobre `SystemBlockWriter` | **Ambos writers**, con `LateralHeaderDrawService.RedrawInPlace` nombrado; `EnsureForPlan` prohibido dentro; **unidad de atomicidad** del registro definida | B3, H4 |
| **D-15** | Materializar al exportar | **Artefacto nuevo**, `PropertyValues` eliminado, linea `1.x`, **sin** herencia del sticky; guard anidado en I-47+ | H1, M6 |
| **D-16** | Ventana + chip, sin decir como llegan los datos | **Frontera Plugin → UI** con DTOs puros; la UI devuelve intenciones | H5 |
| **D-17** | Sin cobertura para D-12/D-13; solo el mensaje de D-08 | Asignacion completa a Core y a Plugin/AutoCAD | M2, M3 |
| **§1-bis** | `PropertyId` sin politica de comparacion | **`Ordinal`**, token canonico; `PropertyId` desconocido = error visible | L4 |
| **§13 P4** | «major condicional» | **promocion pegajosa** | L1 |
| **D-07 (nota)** | «cinco DTO hermanos lo tienen» | Conjunto real corregido; `RackFrameProjectDocument` tambien carece de `[JsonExtensionData]` | L2 |
| **§0 / D-01** | Referencia colgante «§9.2» | Repuntada a §8/D-18 | L3 |

## 0-ter. Findings del Architect → resolucion en V4

| # | Finding | Resuelto en |
|---|---|---|
| **B1** | Nada define la accion LINK | **D-11-bis** (C4-1) |
| **B2** | La promocion pegajosa no tiene portador: `From(...)` reconstruye desde Domain y pierde `SchemaVersion` | **D-07 §portador** (C4-2) + **CF-3** |
| **B3** | D-13 nombra la clase equivocada; el lateral del slice usa un segundo writer que ademas llama a `EnsureForPlan` | **D-13** (C4-3) |
| **B4** | Punto de resolucion sin definir; D-04 contradice a D-11; el BOM cotiza el literal | **D-04 + D-04-bis** (C4-4, C4-5) |
| **H1** | La garantia de F2 se aplica en un solo call site; la ruta de biblioteca no esta guardada | **D-07 §alcance** + **D-15** (C4-10) |
| **H2** | `ProjectVariablesDocument` sin contrato de versionado | **D-01-bis** (C4-8) |
| **H3** | Referencia rota ⇒ rack irreparable | **D-08-bis** (C4-7) |
| **H4** | Granularidad del registro vs unidad de atomicidad | **D-13 §unidad** (C4-11) |
| **H5** | D-10 y D-16b necesitan datos del dibujo dentro de una ventana que no puede leerlo | **D-16 §frontera** (C4-12) |
| **M1** | D-12 no dice si hereda el filtro `Copies > 0` | **D-12** (C4-13) |
| **M2** | Sin verificacion asignada a D-12/D-13 | **D-17** |
| **M3** | De D-08 solo se verifica el mensaje, no el aborto | **D-17** |
| **M4** | El aborto de D-12 es mas ancho de lo que el arbol tolera | **D-12** (C4-13) |
| **M5** | El helper sticky tiene una rama de downgrade para major ≥3 | **D-07 §invariante** (C4-9) |
| **M6** | Schema de una entrada de biblioteca materializada sin especificar | **D-15** (C4-10) |
| **M7** | Guardar sobrescribe el literal con el efectivo | **D-04 §authored** (C4-5) |
| **L1** | P4 nombra «major condicional» | **§13** |
| **L2** | «cinco DTO hermanos» es incorrecto | **D-07 (nota)** |
| **L3** | Referencia colgante §9.2 | **§0 / D-01** |
| **L4** | `PropertyId` sin politica de comparacion; clave desconocida cae al literal | **§1-bis** (C4-14) |

**Se conservan sin cambio** las decisiones que el Architect no impugno: D-01 (NOD), D-02, D-03, D-05,
D-06, D-09, D-10 (F1), D-14, D-17-bis, D-18, D-19, §1-bis (separacion de conceptos), §10 y §11.

## 0-quater. Diff V4 → V4.1

| # | Punto | V4 decia | V4.1 dice |
|---|---|---|---|
| **V4-01** | **Autoridad authored cuando las vistas hermanas divergen** | Asumia, sin decirlo, que todas las vistas de un `RackId` llevan el mismo `Design`. **Silencio total** sobre el caso divergente | **D-12 §autoridad authored multi-vista**: agrupar todas las hermanas, leer sus authored documents completos y **comprobar que son la misma autoridad**. Iguales ⇒ **una sola snapshot logica**. Distintas ⇒ **ABORTAR**, nombrando el `RackId`, con **cero mutacion** y **sin** elegir frontal, primera vista ni mayoria |
| **6** | D-04, ejemplo persistido | El comentario llamaba al literal *«sigue siendo el fallback»* incluso con binding presente | Con binding el literal es **congelado e INACTIVO**; llamarlo *fallback* contradecia a D-08. Corregido |
| **7** | §12.2 | Listaba «a que iniciativa pertenece H3» como **pregunta abierta** | **C3-8 ya retiro esa necesidad.** H3 queda como **futuro separado, fuera de alcance y sin numero**. Deja de ser pregunta |
| **8** | D-12, rack corrupto | Prometia ignorar todo rack corrupto **«ajeno»** | Se ignora **solo si el probe puede DEMOSTRAR que no referencia** el `VariableId` objetivo. Un payload **completamente indescifrable es indistinguible ⇒ aborta** por seguridad |
| **5** | D-17 | Sin cobertura del caso multi-vista | **Tres pruebas Core nuevas** (§8, puntos 15-17) |

**Nada mas cambia.** Las decisiones D-01..D-19, D-01-bis, D-04-bis, D-08-bis, D-11-bis y las premisas
C-1..C-6, C2-1..C2-9, C3-1..C3-8 y C4-1..C4-14 se conservan **literalmente**.

## 0-quinquies. Diff V4.1 → V4.2

| # | Punto | V4.1 decia | V4.2 dice |
|---|---|---|---|
| **N1** | Orden entre probe de consumidores y autoridad multi-vista | «Paso 0, **antes de clasificar**, resolver o mutar **cualquier `RackId`**»: igualdad authored completa primero, clasificacion despues | **Dos familias.** **Target-variable**: el probe corre **por vista** ANTES de exigir igualdad; todas NEGATIVE ⇒ **se ignora el rack** sin exigirle igualdad. **Target-rack**: el `RackId` esta en scope por decision del usuario, **no** se usa el probe para decidir si se inspecciona. Ademas, solo `kind=selective` entra al analisis de bindings |
| **N2** | Costuras de D-04-bis | Decision tomada, costuras **sin nombrar** | Nombradas: **`IRackKindHandler.BuildBom`** con tercer argumento, **`RACKBOMTOTAL`** con snapshot unico del NOD, y el **split authored/effective** en `RackSelectiveWindow`. **`RACKLISTA` NO cambia** y **no lee el NOD** — modificacion del Coordinador sobre la correccion propuesta. `OutputBlockedReason` **no** cambia |
| **N3** | `MutationPlan` | Mencionado **solo** en la prueba 17 | **Definido en D-13** como artefacto **puro** de Application: `VariableMutationPreflightResult` = `Success/Error` + `MutationPlan`. Sin `ObjectId`/`Database`/`Transaction`. Preflight fallido ⇒ plan **vacio** |
| **N4** | Referencias editoriales | «Architect debe revisar V4»; CF-4 «ninguno declarado sobre V4» | Actualizadas al estado real: AGREED **retirado** sobre V4.1, `Architect=DISAGREED` sobre V4.1, **ninguno declarado sobre V4.2** |

**Nada mas cambia.** D-01, D-01-bis, D-02..D-11, D-11-bis, D-14..D-19 y las premisas C-1..C-6,
C2-1..C2-9, C3-1..C3-8, C4-1..C4-14 se conservan **literalmente**.

### Premisas anadidas en V4.2

| # | Premisa |
|---|---|
| C4.2-1 | **ID22A considera para bindings SOLO `kind=selective`.** Ningun otro kind participa del analisis de vinculos |
| C4.2-2 | **Operaciones target-variable** (`ChangeValue`, deteccion de consumidores de `Delete`, `UnlinkAllAndDelete`): el **probe corre por vista hermana** antes de exigir igualdad authored. Todas NEGATIVE ⇒ **ignorar el rack**, sin exigirle igualdad |
| C4.2-3 | **Operaciones target-rack** (`Link`, `Unlink`, `RepairBroken`): el `RackId` esta en scope **por decision del usuario**; **no** se usa el probe para decidir si se inspecciona. Siempre se exige una sola autoridad authored logica |
| C4.2-4 | **`Create` no hace consumer scan. `Rename` no hace consumer scan ni redibujo** (D-09 intacto) |
| C4.2-5 | **`BuildBom` recibe un tercer argumento** `ProjectVariablesDocument`; `RACKBOMTOTAL` lee el NOD **una vez** y usa **el mismo snapshot** en todo el BOM. **`RACKLISTA` no cambia ni lee el NOD.** `OutputBlockedReason` **no** cambia |
| C4.2-6 | **`MutationPlan`** es un artefacto **puro** de Application, sin tipos de AutoCAD; preflight fallido ⇒ plan vacio |

## 0-sexies. Diff V4.2 → V4.3

| # | Punto | V4.2 decia | V4.3 dice |
|---|---|---|---|
| **N5** | Autoridad authored del BOM drawing-wide | `BuildBom` recibe el registro y `RACKBOMTOTAL` lee **un** snapshot del NOD. **Nada** sobre de que sibling sale el authored | **D-04-bis §d2**: para cada `RackId` Selectivo colocado, reunir **todas** las hermanas, leerlas, y exigir **igualdad authored completa** con el comparador de V4-01. Indescifrable o divergente ⇒ **no hay autoridad BOM**, se **reporta** por la politica visible existente y **NUNCA** se elige otra sibling |
| **N6** | Estructura del preflight en D-13 | «comprobar la autoridad authored multi-vista (**paso 0, arriba**)», universal y previo a clasificar — contradiciendo el D-12 ya corregido | **Dos preflights nombrados**: **A. Semantico** (Application puro, sigue D-12 exactamente) → **B. Fisico** (Plugin/AutoCAD). Se retira la expresion «Paso 0» |

**Nada mas cambia.** N1-N4 permanecen cerrados; C4-1..C4-14 y C4.2-1..C4.2-6 se conservan
**literalmente**.

### Premisas anadidas en V4.3

| # | Premisa |
|---|---|
| C4.3-1 | **El BOM no usa el probe tri-estado de D-12.** Para cotizar, **todo** el diseno Selectivo es relevante, asi que no hay «candidatos»: se exige igualdad authored completa a **todo** `RackId` Selectivo colocado que el comando vaya a cotizar |
| C4.3-2 | **El preflight tiene dos mitades nombradas**: **semantico** (puro, Application, produce `VariableMutationPreflightResult`) y **fisico** (Plugin/AutoCAD, resuelve `ObjectId`, planes y definiciones). El fisico corre **solo tras** un `Success` del semantico y **todavia antes** de mutar. La expresion «Paso 0» queda retirada |

## 1. Metodo, y una regla que gobierna todo el documento

**Ninguna opcion gana por existir ya.** El Discovery (§2.1) demostro que la autoridad de nivel dibujo
es **mecanismo nuevo**: cero `NamedObjectsDictionary`, cero `SummaryInfo`/`Ldata`/`RegAppTable`/`XData`
en `src/`, cero `WorkingDatabase` en todo el repositorio. Por tanto **no hay un camino barato**, y la
tentacion de elegir el unico almacen existente —el Xrecord de `RackBlockData` sobre la definicion de
bloque— seria elegir por coste de escritura, no por adecuacion. Ese razonamiento se rechaza
explicitamente en D-01.

Cada decision se presenta como: **alternativas → tradeoffs → recomendacion → por que se descarta el
resto**. Lo que no esta decidido se dice en §12, no se disimula.

Toda cita de codigo esta verificada contra el SHA de la cabecera.

---

## 1-bis. Tres conceptos separados (C2-3)

V1 los mezclaba: hablaba del «vinculo» sin distinguir **lo que la variable es** de **como una propiedad
la usa**. La separacion no es cosmetica — es lo que permite que ID22B e ID21 crezcan **sin chocar entre
si**, porque **extienden tipos distintos**.

```
ProjectVariable                         ← el registro del DIBUJO
    VariableId : string   (GUID)          identidad estable, nunca derivada del nombre  (D-02)
    Name       : string                   solo etiqueta; renombrar no toca a nadie       (D-09)
    Type       : VariableType             ID22A: Length (pulgadas)                       (D-03)
    Definition : VariableDefinition       ID22A: Literal(valor)
                                          ── ID22B anadira: Expression(...)              (C2-1)

PropertyValue<T>                        ← como una PROPIEDAD toma su valor
    Literal(T)                            el escalar de siempre                          (C-1)
    ProjectVariableReference(VariableId)  la referencia gobierna el efectivo             (C-2)
                                          ── ID21 anadira: RackPropertyReference(RackId, PropertyId)  (C2-2)

PropertyId : string estable             ← que propiedad es, de forma persistente
    slice de ID22A: "selective.verticalClearance"
```

**El punto arquitectonico, y la razon de separarlos ahora:**

| Iniciativa | Que extiende | Que NO toca |
|---|---|---|
| **ID22A** (esta) | funda los tres | — |
| **ID22B** — formulas | **`ProjectVariable.Definition`**: `Literal` → tambien `Expression` | `PropertyValue<T>` no cambia: una propiedad sigue apuntando a un `VariableId` y le da igual como esa variable calcule su valor |
| **ID21** — refs a propiedades de racks | **`PropertyValue<T>`**: un tercer caso `RackPropertyReference(RackId, PropertyId)` | `ProjectVariable` no cambia: una referencia rack→rack **no** pasa por el registro del dibujo |

Es decir: **ID22B vive dentro de la variable; ID21 vive dentro de la propiedad.** Si ambos conceptos
compartieran un solo tipo —como insinuaba V1—, cada una de las dos iniciativas obligaria a tocar lo de
la otra. Separados, cada una crece por su lado.

**ID22A no implementa ninguno de los dos.** `Definition` tiene hoy un solo caso (`Literal`) y
`PropertyValue<T>` dos (`Literal`, `ProjectVariableReference`). Lo unico que ID22A se compromete a
hacer es **dejar los dos puntos de extension abiertos y persistidos de forma discriminada**, para que
anadir un caso sea aditivo y no una ruptura de esquema.

### `PropertyId`: por que es un concepto y no una cadena cualquiera

`PropertyId` identifica **la propiedad**, no el campo C# ni la caja de la UI. Es estable, se persiste y
sobrevive a renombrados de codigo. Para el slice: **`selective.verticalClearance`**.

Existe por dos razones concretas, y la segunda es la que lo hace obligatorio ahora:

1. Es la clave del mapa persistido de D-04.
2. **ID21 lo necesita como la mitad de su par `(RackId, PropertyId)`.** Una referencia a «la holgura
   vertical de aquel rack» solo se puede escribir si esa propiedad tiene un identificador estable e
   independiente del rack. Introducirlo despues obligaria a reescribir lo ya persistido.

Los ids se declaran como **constantes en un solo sitio**, siguiendo la disciplina que
`CatalogBlockParameters` ya usa para los nombres de parametro de bloque
(`SelectiveRackDefaults.LengthParam`, `PeralteParam`, `PalletAltoParam`), para que un renombrado de C#
no pueda romperlos en silencio.

**Politica de comparacion (C4-14), corregida en V4.** V3 no la fijaba, y el Architect registro que sin
ella una clave desconocida cae al literal en silencio (L4).

- `PropertyId` es un **token canonico exacto**. Se compara **`Ordinal`** — sensible a mayusculas,
  sin normalizacion, sin recorte semantico. No hay alias ni sinonimos.
- El unico `PropertyId` de ID22A es **`selective.verticalClearance`**.
- **Un `PropertyId` desconocido presente en `PropertyValues` es ERROR VISIBLE**, nunca una caida
  silenciosa al literal. Un build que no sabe que propiedad es esa **no puede** afirmar que el literal
  sea el valor correcto: el documento declara una autoridad que el build no entiende, y eso es
  exactamente el caso que D-01-bis prohibe ignorar.

> **Asimetria deliberada, declarada para que no se lea como descuido:** `VariableId` se compara
> **`OrdinalIgnoreCase`** (D-02, siguiendo la politica del GUID de rack en `FindRackBlocks`) y
> `PropertyId` se compara **`Ordinal`**. Son cosas distintas: el primero es un GUID generado por la
> maquina, cuya representacion hexadecimal varia de caja entre escritores; el segundo es un token
> **autorado en el codigo** y declarado en una constante, donde una diferencia de caja significa que
> alguien escribio otra cosa.

---

## 2. Fundacion

### D-01 — Autoridad unica `ProjectVariables` por DWG

**Requisito**: exactamente **un** registro por dibujo, estructurado, que sobreviva a guardar/reabrir y
que no dependa de que el usuario no toque nada.

| Alt | Mecanismo | A favor | En contra |
|---|---|---|---|
| **A1** | **NOD**: `Database.NamedObjectsDictionaryId` abierto como `DBDictionary` → sub-`DBDictionary` `RACKCAD_PROJECT` → `Xrecord` con JSON troceado | Unicidad **estructural**: hay exactamente un NOD por `Database`. Es el sitio que AutoCAD define para datos de aplicacion a nivel dibujo. **No entra en la `BlockTable`**, asi que `RackBlockFinder.ScanEnvelopes` no lo ve y no necesita exclusion nueva. El troceado a <=255 ya esta probado en `RackBlockData` | API nueva en el Plugin (crear el sub-diccionario y `AddNewlyCreatedDBObject`). No lo cubre ninguna suite (R2) |
| **A2** | Diccionario de extension del **BTR de Model Space**, reutilizando `RackBlockData` tal cual | Coste de escritura casi nulo | La unicidad es **por convencion** («siempre elegimos Model Space»), no estructural: cada layout de papel es otro BTR. Dice «esto pertenece al bloque Model Space», que es **falso**. Elegirla seria exactamente el error que §1 prohibe |
| **A3** | Definicion de bloque dedicada (`RACKCAD_PROJECT_VARS`) con `RackBlockData` | Reutilizacion total | **`ScanEnvelopes` la leeria** (no es layout, ni anonima, ni xref) y la tomaria por un rack: exige una exclusion nueva en el barrido. El usuario puede renombrarla, explotarla o purgarla. Peor opcion |
| **A4** | `Database.SummaryInfo` (propiedades personalizadas) | Nivel dibujo por definicion; visible en las propiedades del DWG | Mapa plano `string→string` **editable por el usuario** desde la UI de AutoCAD: un registro tipado se corrompe en silencio. Sin estructura |
| **A5** | Archivo sidecar junto al DWG | Testeable sin AutoCAD | **Deja de existir en cuanto el DWG se copia solo.** Contradice el objetivo: el registro debe viajar dentro del dibujo |

**Recomendacion: A1.** No por reutilizacion —no hay ninguna— sino porque es **la unica cuya unicidad
por dibujo es estructural** y la unica que no obliga a tocar el barrido existente. A2 se descarta
aunque sea la mas barata: su unicidad depende de una convencion que nadie puede hacer cumplir, y §1
prohibe elegir por coste.

**Reparto por capas** (AGENTS conv. 1, [ADR-0006](../adr/0006-autocad-solo-en-plugin.md)):

```
RackCad.Application/Persistence/   ProjectVariablesDocument (DTO) + ProjectVariablesStore
                                   (Serialize/Deserialize)  ← PURO, cubierto por la suite Core
RackCad.Plugin/                    ProjectVariablesData.Read/Write(Transaction, Database)
                                   ← unico codigo que toca el NOD; deliberadamente minimo
```

Ese reparto es la **mitigacion de R2**: el almacen del DWG no tiene hoy ninguna prueba de
comportamiento (§2.3 del Discovery), asi que todo lo que pueda vivir fuera del Plugin, vive fuera.

**Precision de API (C2 punto 7).** El acceso correcto es
**`Database.NamedObjectsDictionaryId`** —un `ObjectId`— abierto dentro de la transaccion como
`DBDictionary`; sobre el se crea o se lee la entrada `RACKCAD_PROJECT`. Es una correccion de
**nomenclatura**, no de decision: A1 sigue siendo la recomendacion.

> Nota: la afirmacion negativa del Discovery §2.1 **no se ve afectada**. La busqueda uso la cadena
> `NamedObjectsDictionary`, que es **subcadena** de `NamedObjectsDictionaryId`, asi que habria
> encontrado ambas formas. Los ceros siguen siendo ceros.

**A verificar cuando haya AutoCAD** (no se afirma aqui): que `PURGE` no toque una entrada del NOD con
Xrecord propio. Es la unica duda operativa de A1 dentro del alcance minimo de §8, D-18.

### D-01-bis — Contrato propio del `ProjectVariablesDocument` (C4-8)

> **NUEVA en V4.** El Architect registro (H2) que V3 nombraba este documento **dos veces** —el
> diagrama de capas y la tabla de pruebas— y **nunca** le daba contrato de versionado, en un
> documento cuya seccion mas larga trata precisamente de versionado. Todo documento persistido del
> repositorio tiene el suyo; el que ID22A **funda** no lo tenia.

El registro del dibujo es un documento persistido de pleno derecho y se le exige lo mismo que a sus
hermanos, **desde el dia uno**:

| Elemento | Contrato |
|---|---|
| `SchemaVersion` | Presente desde el dia 1, valor inicial **`"1.0"`**. Es **independiente** de la linea del diseno selectivo: son dos documentos distintos y sus versiones no se mezclan |
| Store propio | `ProjectVariablesStore` en `RackCad.Application/Persistence`, **puro**, con `Serialize`/`Deserialize` — el Plugin solo aporta el acceso al NOD |
| Guard propio | `SchemaGuard.CheckReadable(stored, current, "Las variables de proyecto")`. **Major superior = ERROR**, con el mensaje ya existente, no con `null` silencioso |
| `ExtensionData` | `[JsonExtensionData]` en la raiz **desde el dia 1**, para que un build I-47 preserve lo que escriba un build posterior del mismo major |
| Ausencia | Un DWG sin la entrada del NOD se lee como **registro vacio** (C-1). Eso **no** es un error: es un proyecto con cero variables |

**Tres errores duros, y ninguno se degrada a aviso** (C4-8):

```
SchemaVersion con major > el soportado   ->  ERROR visible. No se lee, no se escribe, no se degrada.
VariableType desconocido                 ->  ERROR visible.
Definition.kind desconocido              ->  ERROR visible.
```

**La regla que los une: NUNCA ignorar datos que el build no entiende.** Un `VariableType` o un
`Definition.kind` que este build no conoce significa que **otra version escribio autoridad que aqui no
se puede resolver**. Continuar equivaldria a dibujar con una autoridad que no se comprende, que es la
familia de fallos que el proyecto persigue desde I-03 y la misma doctrina de D-08.

> Esto es lo que **habilita** a ID22B sin disenarlo: cuando `Definition` gane el caso `Expression`, un
> build I-47 encontrara un `kind` desconocido y **fallara de forma visible** en vez de evaluar una
> variable cuyo valor no sabe calcular. **ID22B decidira su propia evolucion de version**; ID22A solo
> garantiza que el encuentro no sea silencioso.

**Relacion con el schema del Selectivo:** son **dos lineas de version independientes**. La promocion
pegajosa de D-07 gobierna `SelectivePalletDesignDocument`; `ProjectVariablesDocument` tiene la suya y
**no** se promueve por el hecho de que un rack se vincule.

### D-02 — `VariableId` estable e independiente de `Name`

| Alt | Forma | Tradeoff |
|---|---|---|
| **B1** | **GUID** (`Guid.NewGuid().ToString()`) | Estable por construccion; **precedente directo**: `RackEmbedDocument.Id` es exactamente eso para la identidad del rack. Ilegible a ojo |
| B2 | Slug derivado del nombre (`holgura-vertical`) | Legible; **pero renombrar rompe las referencias**, que es justo lo que el requisito prohibe. Descartada por definicion |
| B3 | Entero incremental | Compacto; colisiona en cuanto dos conjuntos de variables se encuentren, y no gana legibilidad real |

**Recomendacion: B1**, comparado **`OrdinalIgnoreCase`** —la misma politica que `FindRackBlocks` usa
para el GUID del rack (`RackCommandSupport.cs:124`)—, para no introducir una segunda regla de
comparacion de identidades en el mismo repositorio.

`Name` es **solo etiqueta**: se muestra, se edita libremente y **no participa en ninguna resolucion**.

### D-03 — `Type` y `Definition` separados; ID22A solo `Length` literal en pulgadas

> **Corregido en V2 (C2-1).** V1 justificaba el discriminador diciendo que «ID22B agregara tipos». Eso
> era **incorrecto**: ID22B son **formulas**. La justificacion real es otra —y mas fuerte—, y ademas
> obliga a **partir el modelo en dos ejes** que V1 tenia fundidos en uno.

Una variable tiene **dos** cosas que pueden crecer por separado, y confundirlas es lo que V1 hacia:

```
Type       — QUE es el valor            ID22A: Length (pulgadas)
Definition — DE DONDE sale el valor     ID22A: Literal(valor)
                                        ID22B: Expression(...)   ← el eje que crece con formulas
```

| Alt | Modelo | Tradeoff |
|---|---|---|
| **C1** | **`Type` discriminado + `Definition` discriminada**, ejes independientes | Additivo en los dos ejes por separado; un tipo o una definicion desconocidos son **detectables** (D-08 los reporta, no los ignora) |
| C2 | `object` + etiqueta de tipo | Flexible y opaco; obliga a `switch` de casteo en cada consumidor |
| C3 | **Sin tipo**: todo `double` literal | Lo mas simple hoy y lo que bloquea **las dos** continuaciones (ver abajo) |
| C4 | Una coleccion por tipo | Sin casteos; multiplica el formato persistido por cada tipo nuevo |
| C5 | `Type` discriminado pero **valor literal fijo** (sin `Definition`) | Suficiente para ID22A; **ID22B tendria que romper el esquema** para meter una expresion donde hoy hay un escalar |

**Recomendacion: C1.** ID22A acepta **exactamente un** `Type` —`Length`, en **pulgadas**, coherente
con [ADR-0005](../adr/0005-estrategia-de-unidades.md), sin introducir conversion— y **exactamente una**
`Definition`: `Literal`.

**Por que el discriminador de `Type` sigue siendo necesario, ahora que ID22B no lo justifica:**

1. **Candidatos no numericos ya identificados.** El Discovery §4.7 senala `DimensionStyle` —una cadena,
   elegida de una tabla de nivel dibujo y hoy guardada por rack— como el candidato mas claro despues
   del slice. Un modelo sin tipo obliga a romper el esquema para admitirlo.
2. **Una formula necesita operandos tipados.** ID22B no puede evaluar `a + b` sin saber si suma
   longitudes o concatena texto, ni comprobar coherencia de unidades. El tipo es **precondicion** de
   ID22B, aunque ID22B no agregue tipos.

**Y `Definition` es el punto de extension que ID22B ocupa.** Persistirla discriminada desde el primer
dia es lo que permite que `Expression` entre **como un caso mas**, aditivo, sin subir el `SchemaVersion`
mayor y sin tocar `PropertyValue<T>` (§1-bis).

C3 y C5 se descartan aunque hoy bastarian: son las decisiones que mas caro salen de corregir, y cada
una bloquea una continuacion distinta ya definida por el dueno.

### D-04 — `Literal` vs `ProjectVariableReference`: la forma de la propiedad

Premisa C-2: si hay referencia, **la referencia gobierna**. La pregunta abierta es **como se persiste
el vinculo**.

| Alt | Forma | A favor | En contra |
|---|---|---|---|
| **D1** | Campo hermano por propiedad: `VerticalClearance` (double, existente) **+** `VerticalClearanceVariableId` (string, nuevo nullable) | Minimo, explicito, **cero cadenas magicas**, trivial de probar | **Un campo nuevo por cada propiedad vinculable**: ID22B con N propiedades = N campos |
| D2 | Sustituir el escalar por un objeto `{ mode, literal, variableId }` | Expresivo | **Rompe la compatibilidad del campo escalar existente** y exigiria migracion ⇒ **prohibida por C-1**. Descartada por premisa |
| ~~D3~~ | ~~`Bindings: { "<slot>": "<variableId>" }`~~ — **RETIRADA en V2** | Un solo campo | **El valor es una cadena desnuda**: no dice **que clase** de referencia es, asi que ID21 (`RackId` + `PropertyId`) no cabe sin cambiar el tipo del valor. Sustituida por D4 |
| **D4** | **`PropertyValues: { "<PropertyId>": { "kind": "projectVariable", "variableId": "<id>" } }`** — referencia **tipada** y discriminada | Un solo campo **y** un punto de extension explicito: `kind` es exactamente donde ID21 anade `rackProperty`. Un `kind` desconocido es **detectable** (D-08) | Formato algo mas verboso que una cadena |

**Recomendacion: D4 (corregida en V2 por C2-3 y C2-4).**

V1 recomendaba D3. **Se retira** porque su valor era una cadena desnuda: `slot → variableId` codifica
«esta propiedad apunta a **una variable**» como una suposicion implicita del formato. En cuanto ID21
introduce `RackPropertyReference(RackId, PropertyId)` —dos datos, no uno—, D3 obliga a cambiar el tipo
del valor del mapa, que **es** una ruptura de esquema. D4 lo evita reconociendo desde el principio que
lo persistido es un **caso de `PropertyValue<T>`**, no un id suelto.

**Contrato de D-04:**

1. **El escalar legacy se conserva como el literal.** `SelectivePalletDesignDocument.VerticalClearance`
   sigue existiendo, con su tipo y su nombre. No se sustituye, no se migra, no se vacia (C-1). Esto
   descarta D2 por premisa, igual que en V1.
2. **La referencia vive en un mapa aparte**, tecleado por `PropertyId` (§1-bis) y con valor
   **discriminado por `kind`**.
3. **ID22A acepta un solo `kind`: `projectVariable`.** Ningun otro se implementa.
4. **Un `kind` desconocido es error visible**, nunca se ignora ni cae al literal (D-08).
5. **ID21 anadira `rackProperty`** —`{ "kind": "rackProperty", "rackId": "...", "propertyId": "..." }`—
   como **caso adicional**, sin tocar los existentes ni subir el `SchemaVersion` mayor. **No se
   implementa aqui**, y este documento no disena su resolucion.

Forma persistida del slice:

```jsonc
{
  "VerticalClearance": 6.0,                       // literal AUTHORED: intacto y, con binding, INACTIVO
  "PropertyValues": {
    "selective.verticalClearance": { "kind": "projectVariable", "variableId": "9f3c…" }
  }
}
```

**Las claves son constantes, no literales dispersos.** Es la disciplina que `CatalogBlockParameters`
ya usa para los nombres de parametro de bloque (`SelectiveRackDefaults.LengthParam`, `PeralteParam`,
`PalletAltoParam`), y neutraliza el unico argumento serio contra un mapa tecleado por cadena.

**D1 sigue siendo defendible si el dueno prefiere el minimo absoluto** para una sola propiedad, pero su
coste sube con la correccion: con ID21 definido, D1 necesitaria **dos** campos hermanos nuevos por
propiedad (`…VariableId` y despues `…RackId`/`…PropertyId`), o un campo hermano cuyo tipo cambia. El
tradeoff pasa de «cuando pago el escalar» a «cuantas veces rompo el esquema».

**Semantica del valor efectivo:**

```
valor efectivo(propiedad) =
    hay entrada en PropertyValues  →  se resuelve segun su kind      (C-2: la referencia gobierna)
                                      ID22A: projectVariable → valor de esa variable
    no hay entrada                 →  el literal authored            (C-1: literal preservado)
```

> **Precision de V4.1 (punto 6).** La segunda rama **no** es un «fallback» del binding: es el caso en
> que **no hay binding**. Con binding presente el literal authored esta **congelado e INACTIVO** y
> **nunca** se usa como valor — ni siquiera si la referencia no resuelve, que es lo que **D-08**
> prohibe expresamente. La unica via por la que un literal authored vuelve a gobernar con un binding
> escrito es **`RepairBroken`** (D-08-bis), que es explicita, avisada y retira el binding.

#### El literal `authored`, con la contradiccion de V3 eliminada (C4-5)

> **Correccion de V4 (B4, M7).** V3 decia aqui: *«El literal nunca se borra al vincular: se conserva y
> es lo que D-11 devuelve al desvincular.»* Eso describia la opcion **G2**, que **D-11 rechaza
> explicitamente** como «el bug facil» por producir un salto geometrico. El documento normativo se
> contradecia sobre un comportamiento que mueve geometria. Se elimina.

Se nombran **dos** cosas que V3 confundia en una:

| Nombre | Que es | Quien lo escribe |
|---|---|---|
| **`authored`** | El valor **que el usuario tecleo**, persistido en `VerticalClearance`. Mientras haya binding esta **congelado e INACTIVO** | solo el usuario, **editando sin binding**; y el **UNLINK** (D-11) |
| **`effective`** | El valor que **gobierna** dibujo, BOM y preview. Con binding, sale de la variable; sin binding, **es** el authored | nadie: **se calcula**, no se persiste (D-04-bis) |

Las tres reglas que se derivan, y que cierran M7:

1. **Un cambio de variable NO sobrescribe el literal authored.** La propagacion redibuja; no reescribe
   la intencion del usuario.
2. **`RACKEDITAR` Save con binding activo NO copia el textbox efectivo al literal.** Hoy
   [`RackSelectiveWindow.xaml.cs`](../../src/RackCad.UI/Systems/Selective/RackSelectiveWindow.xaml.cs)
   lee `ClearanceBox.Text` y lo escribe en `VerticalClearance`; con binding activo ese camino queda
   **cortado** y el literal persistido no se toca.
3. **El literal authored solo cambia por UNLINK** (D-11), que escribe en el el **efectivo** —no el
   authored anterior—, y por eso el desvinculado tiene efecto geometrico nulo.

> Consecuencia honesta: tras un UNLINK el authored **ya no es** lo que el usuario tecleo originalmente,
> sino el efectivo materializado. Es deliberado y es lo que hace segura la operacion. Lo que **no**
> ocurre nunca es que el literal cambie **por si solo** mientras el binding esta vivo.



---

### D-04-bis — UN solo punto de resolucion, en Application (C4-4)

> **NUEVA en V4.** El Architect registro (B4) que V3 **nunca decia donde** se resuelve una referencia.
> Consecuencia verificada en el arbol: el BOM re-resuelve el diseno por su cuenta
> —[`SelectiveKindHandler.cs:29`](../../src/RackCad.Plugin/KindHandlers/SelectiveKindHandler.cs)
> llama a `new SelectiveGeometryResolver().Resolve(design, catalog)` y
> [`SelectiveGeometryResolver.cs:96`](../../src/RackCad.Application/Systems/Selective/SelectiveGeometryResolver.cs)
> lee `design.VerticalClearance`, el **literal**— sin acceso al registro. El dibujo mostraria la
> variable y la **cotizacion el literal congelado**. Eso viola la convencion 2 de AGENTS: cuando
> dibujo, BOM y UI deben coincidir en un numero, la regla vive en **UNA** funcion de Application.

**Contrato conceptual:**

```
RackCad.Application/Systems/Selective/

    SelectiveEffectiveDesignResolver.Resolve(
        authoredDocument : SelectivePalletDesignDocument,   // lo persistido, con PropertyValues
        projectVariables : ProjectVariablesDocument         // el registro del dibujo (o vacio)
    ) -> SelectivePalletDesign                              // el diseno EFECTIVO
```

Es **puro**: sin AutoCAD, sin I/O, sin catalogo. Recibe los dos documentos y devuelve el diseno de
Domain que todo lo demas consume.

```
   PERSISTIDO                        CALCULADO                      CONSUMIDORES
   ----------                        ---------                      ------------

   SelectivePalletDesignDocument
     VerticalClearance  ──authored──┐
       (congelado e INACTIVO         │
        mientras haya binding)       │
     PropertyValues                  ├──> SelectiveEffectiveDesignResolver ──> SelectivePalletDesign
       "selective.verticalClearance" │         .Resolve(authored, variables)        (EFECTIVO)
         { kind, variableId } ───────┤                                                   │
     SchemaVersion / ExtensionData   │                                                   ├──> DIBUJO   (frontal/lateral/planta)
                                     │                                                   ├──> BOM      (RACKBOMTOTAL incluido)
   ProjectVariablesDocument  (NOD)   │                                                   └──> PREVIEW
     ProjectVariable                 │
       VariableId / Name             │                                             UI  <── efectivo + estado authored
       Type / Definition ────────────┘                                                   (D-16-frontera)
```

**Nadie mas resuelve.** El authored no llega a dibujo, BOM ni preview: **solo** llega al resolver y a
la UI, y a la UI acompanado de su estado de binding para poder pintarlo.

**Quien consume que — y esto es lo vinculante:**

| Consumidor | Consume | NO consume |
|---|---|---|
| Dibujo (`SelectiveGeometryResolver`, builders frontal/lateral/planta) | el **efectivo** | el authored |
| **BOM** (`SelectiveKindHandler`, `SelectiveBomBuilder`, `RACKBOMTOTAL`) | el **efectivo** | el authored |
| Preview del editor | el **efectivo** | el authored |
| UI del editor | **valor efectivo + estado authored del binding** (D-16) | — |

**Ninguno de ellos resuelve bindings.** No hay logica de resolucion en geometria, ni en BOM, ni en la
UI: **todos reciben un `SelectivePalletDesign` ya efectivo** y no saben si nacio de un literal o de una
variable. Esa es la propiedad que hace imposible la divergencia de B4.

**Dos consecuencias de diseno:**

1. **El punto de entrada se desplaza, no se duplica.** Hoy los consumidores parten de
   `document.ToDomain()`. Pasan a partir de `Resolve(document, variables)`. `ToDomain()` sigue
   existiendo y es, exactamente, el caso **sin binding**.
2. **Es la pieza mas testeable de todo ID22A**: dos documentos puros dentro, un Domain fuera. Toda la
   semantica de C-2, D-08, D-08-bis y C4-5 se prueba aqui, en la suite Core, sin AutoCAD (D-17).

#### Las costuras que esta decision fuerza (N2, nombradas en V4.2)

> **Correccion de V4.2.** El Architect (N2) registro que V4/V4.1 **decidian** el punto unico de
> resolucion pero **no nombraban** las costuras que obliga a tocar, una de ellas una **interfaz
> compartida por seis handlers**. CF-3 solo decia «el desplazamiento de dibujo/BOM/preview a
> consumirlo». Se nombran las tres, y se **acota** una: **`RACKLISTA` no entra** — modificacion del
> Coordinador sobre la correccion propuesta.

**Costura 1 — el BOM, que es la unica de alcance drawing-wide.**

Estado verificado hoy:

```csharp
// src/RackCad.Plugin/KindHandlers/IRackKindHandler.cs
BillOfMaterials BuildBom(RackEmbedDocument embed, RackCatalog catalog);      // <-- sin variables

// src/RackCad.Plugin/KindHandlers/SelectiveKindHandler.cs
var design = new SelectivePalletDesignStore().Deserialize(embed.Design)?.ToDomain();   // <-- AUTHORED
var system = new SelectiveGeometryResolver().Resolve(design, catalog);
return SelectiveBomBuilder.Build(system, catalog);
```

`ToDomain()` es exactamente el camino **authored sin resolver**, y es el que hoy alimenta la
cotizacion. Contrato conceptual final:

```
BuildBom(RackEmbedDocument embed, RackCatalog catalog, ProjectVariablesDocument projectVariables)
```

**Un tercer argumento, no un objeto de contexto nuevo.** Y con el reparto explicito:

| Kind | Que hace con el argumento |
|---|---|
| `selective` | lo usa: `SelectiveEffectiveDesignResolver.Resolve(authored, projectVariables)` |
| `cabecera`, `dynamic`, `cama`, `pushback`, `cantilever` | **lo ignoran** en ID22A |

**`RACKBOMTOTAL`**: lee `ProjectVariables` del NOD **UNA vez al inicio**, usa **el mismo snapshot**
durante todo el BOM y lo pasa a cada `BuildBom`. Un unico snapshot es lo que impide que dos racks del
mismo total se coticen contra estados distintos del registro.

##### Y de que authored se cotiza (N5, corregido en V4.3)

> **Correccion de V4.3.** Un snapshot correcto del **registro** aplicado a un authored **arbitrario**
> sigue cotizando mal. El arbol agrupa por `RackId` conservando el **primer** envelope que encuentra:
>
> ```csharp
> // src/RackCad.Plugin/RackInventarioCommands.BomTotal.cs
> if (!byRack.TryGetValue(embed.Id, out var aggregate))
> {
>     byRack[embed.Id] = new RackAggregate { Embed = embed, Copies = copies };   // <-- la PRIMERA hermana
> }
> else if (copies > aggregate.Copies) { aggregate.Copies = copies; }             // las demas solo suben Copies
> ...
> var bom = BuildRackBom(handlers[i], aggregate.Embed, catalog);                 // <-- cotiza desde ESA
> ```
>
> Es decir, hoy el BOM sale de la hermana que el recorrido de la `BlockTable` toque primero. **V4-01 ya
> declaro alcanzable** que las hermanas diverjan, asi que eso es elegir autoridad por orden de
> iteracion.

**Para cada `RackId` Selectivo colocado que el comando vaya a cotizar:**

```
1. reunir TODAS las sibling views presentes
2. leer y deserializar su authored SelectivePalletDesignDocument
3. alguna INDESCIFRABLE   -> NO elegir otra hermana.
                             El rack NO tiene autoridad BOM inequivoca.
                             Reportarlo por la politica visible existente.
4. todas legibles         -> comprobar igualdad authored semantica COMPLETA,
                             con el MISMO comparador de V4-01 (mismaAutoridadAuthored)
5. DIVERGEN               -> NO frontal, NO primera, NO mayoria.
                             Reportar que las vistas del RackId tienen disenos divergentes.
                             NO construir el BOM de ese rack desde una hermana arbitraria.
6. COINCIDEN              -> una authored snapshot logica
                             -> resolver con el MISMO snapshot de ProjectVariables del comando
                             -> BuildBom desde el EFECTIVO
```

**El BOM no usa el probe tri-estado de D-12** (C4.3-1). Aquel existe para decidir **relevancia** frente
a una variable objetivo; para cotizar, **todo** el diseno Selectivo es relevante — el poste, los
fondos, los largueros y la seguridad se cobran igual haya binding o no. No hay candidatos que filtrar:
se le exige igualdad authored a **todo** rack Selectivo que se vaya a cotizar.

**La politica de RACKBOMTOTAL sobre un rack no construible NO cambia.** El arbol ya tiene dos canales
y ambos se conservan tal cual:

| Situacion | Politica **existente**, que se conserva |
|---|---|
| Kind sin handler | **aborta el total** (`KindHandlerDispatch.TryResolveAll`) |
| Diagnostico bloqueante del propio sistema | **aborta el total** (`RackBomOutputGate.DescribeBlocked`) — *«un total al que le falta un rack no puede parecer completo»* |
| **Payload ilegible** de un rack | **se omite ese rack con aviso visible** (`RackBomOutputGate.DescribeUnreadable`), nunca en silencio |

Lo unico que V4.3 anade es **un motivo nuevo que entra por el tercer canal**: un `RackId` Selectivo con
hermanas **divergentes** —o con alguna **indescifrable**— es un rack **sin autoridad BOM**, y se trata
como el payload ilegible: **se omite con aviso visible que nombra el rack**. No se inventa un canal
nuevo, no se cambia el reparto abortar/omitir, y **no** se cotiza desde una hermana elegida a dedo.

El mensaje vive donde ya viven sus hermanos: `RackBomOutputGate`, en
**`RackCad.Application/Bom/`** — puro, y por tanto verificable en la suite Core junto a
`DescribeBlocked` y `DescribeUnreadable`.

**`OutputBlockedReason` NO cambia por ID22A.** Su firma se queda como esta; para el Selectivo devuelve
`null` (`SelectiveKindHandler`), asi que no es una via de divergencia para este slice.

**Costura 2 — `RACKLISTA`: NO entra, y conviene decirlo expresamente.**

- **No** es consumidor del diseno efectivo.
- **No** lee el NOD.
- **No** cambia.

Su modelo es solo identidad y etiquetas: `RackListEntry` lleva `Id`, `Name`, `Kind`, `KindLabel`,
`ViewsLabel` y `ViewCount` — **ningun valor de diseno**. Hacerle leer el registro seria coste y
superficie sin beneficio.

**Costura 3 — el editor Selectivo.**

Arbol actual: `RackSelectiveWindow.BuildDesign(out string)` produce **un** `SelectivePalletDesign`, y
`BuildSystem(out string)` lo resuelve contra el catalogo dejando `lastSystem`. Ese unico objeto sirve
hoy **a la vez** para previsualizar y para persistir.

**En un rack vinculado eso deja de ser sostenible**, porque C4-5 exige que el guardado **no** escriba
el efectivo en el literal. La separacion conceptual que debe existir:

| | AUTHORED | EFFECTIVE |
|---|---|---|
| Que es | el estado **que se persistira** | el estado que **gobierna la geometria** |
| Conserva | `PropertyValues`, `SchemaVersion`, `ExtensionData` y el **literal authored** | nada: se **calcula** |
| Se obtiene | del portador (C4-2) | `SelectiveEffectiveDesignResolver` |
| Alimenta | el guardado del payload | `SelectiveGeometryResolver` → preview → `lastSystem` → BOM local del editor → geometria de dibujo |
| Con binding activo | Save **NO** copia el `VerticalClearance` efectivo al literal | es lo que el usuario ve en el campo |

**Un solo `SelectivePalletDesign` no puede seguir significando authored y effective a la vez en un
rack vinculado.** Esa es la costura, y queda nombrada.

**No se prescribe una refactorizacion WPF mayor de la necesaria.** No se pide reescribir la ventana ni
adoptar un patron nuevo: se pide que el objeto que se **persiste** y el que se **dibuja** dejen de ser
el mismo cuando hay binding. Como se implemente es de la fase de implementacion; **que exista la
separacion** es del contrato, y entra en **CF-3**.

**Alternativa descartada: resolver en cada consumidor.** Es lo que V3 dejaba implicito por omision.
Duplica la regla en tres sitios como minimo, contradice la convencion 2 de AGENTS y es exactamente el
defecto B4.

---

## 3. El vertical slice

### D-05 — `SelectivePalletDesign.VerticalClearance`, y nada mas

C-5 fija el slice. En ID22A **una sola propiedad** es vinculable:

```csharp
// src/RackCad.Domain/Systems/Selective/SelectivePalletDesign.cs:29-30
/// <summary>Vertical clearance ("holgura") above a pallet inside its clear opening (in). Editable; default 6".</summary>
public double VerticalClearance { get; set; } = 6.0;
```

Es un buen slice por tres razones verificadas en el Discovery: existe **solo** en el Selectivo (9
lineas, 6 archivos), tiene **un unico consumidor geometrico** (`SelectiveGeometryResolver.cs:96`), y su
efecto es **visible en el dibujo**, asi que la validacion del dueno puede confirmarlo a ojo.

**Fuera del slice, por C-5**: `ClearHeight` (Dinamico/Push Back, por nivel), `RequestedClearHeight`
(Cantilever), `PalletTolerance` y `PalletDepth`. El Discovery §4.2-4.4 sigue describiendo esas
asimetrias como **hechos del arbol**; ID22A no las toca.

### D-06 — `ClearOverride` conserva **exactamente** su precedencia actual

La cadena vigente, verificada:

```csharp
// src/RackCad.Application/Systems/Selective/SelectiveGeometryResolver.cs:395-402
private static double SeparationFor(SelectiveCell cell, double palletAltoBelow, double clearance, double paso)
{
    if (cell.ClearOverride.HasValue && cell.ClearOverride.Value > 0.0)
    {
        return Math.Max(paso, RoundUpToMultiple(cell.ClearOverride.Value, paso));
    }
    return Separation(palletAltoBelow, clearance, cell.BeamPeralte, paso);
}
```

**La variable sustituye el argumento `clearance`, y nada mas.** Es decir:

```
ClearOverride de la celda   (manual, por celda)        ← GANA, sin cambios
    ↓ si no hay
valor efectivo del rack     (variable si hay binding, si no el literal)
```

**Alternativa considerada y descartada**: que la variable de proyecto tambien pise `ClearOverride`.
Se descarta porque invierte una precedencia que hoy existe y esta probada, convertiria un override
manual en un valor ignorado sin aviso, y **ninguna premisa la pide**. C-2 dice que la referencia
gobierna **el valor de la propiedad**, no que el proyecto gobierne las excepciones por celda.

Consecuencia practica y deseable: `ClearOverride` sigue siendo la **valvula de escape** para el caso
en que una celda concreta deba desviarse del estandar del proyecto.

---

## 4. Persistencia

### D-07 — Compatibilidad hacia atras, y ausencia del registro = vacio

Dos limites distintos, y **el segundo tiene un agujero verificado**.

**Limite 1 — el registro del dibujo.** C-1: un DWG sin entrada en el NOD **no es un error ni un
dibujo a migrar**; es un proyecto con **cero variables**. Se lee como registro vacio, todos los racks
siguen siendo literales, y nada cambia en el dibujo. Es tambien lo que ocurre con un DWG creado por
una version anterior, sin ningun trabajo adicional.

**Limite 2 — el diseno del rack.** Aqui esta el hallazgo:

```
DTO con [JsonExtensionData]:  RackEmbedDocument, RackProjectDocument, PushBackDesignDocument,
                              FlowBedDocument, CantileverLineDocument, LargueroDocument
DTO SIN [JsonExtensionData]:  SelectivePalletDesignDocument      ← el del slice
```

`SelectivePalletDesignDocument` **tiene su propio `SchemaVersion` (`"1.0"`) pero no preserva campos
desconocidos**. Consecuencia concreta: si una version nueva escribe el vinculo dentro del diseno
selectivo y una version **anterior** abre ese dibujo, edita el rack y lo vuelve a guardar, **el
vinculo desaparece en silencio** y el rack queda como literal sin que nadie lo note. Es exactamente la
clase de fallo silencioso que el proyecto persigue desde I-03.

> ### ⚠ Correccion de V2 (C2-5): V1 se equivocaba aqui
>
> V1 recomendaba anadir `[JsonExtensionData]` a `SelectivePalletDesignDocument` **«para evitar la
> perdida silenciosa»**. Eso es **falso**, y no es un matiz: un binario **pre-I-47 ya compilado** no
> tiene ese atributo y **nunca lo tendra**. Anadirlo hoy no cambia el comportamiento de ninguna version
> que ya exista. La perdida silenciosa que V1 decia evitar **seguiria ocurriendo exactamente igual**.

### Dos compatibilidades distintas, que V1 fundia en una

| | Que es | ¿Se puede garantizar desde I-47? |
|---|---|---|
| **Backward compatibility** | Una version **nueva** lee un documento **viejo** | **SI**, y ya esta resuelto: campo nullable ausente = legado, `SchemaVersionPolicy.IsReadable` acepta major menor o ausente |
| **Downgrade / forward round-trip** | Una version **vieja, ya compilada**, lee un documento **nuevo**, lo edita y lo vuelve a guardar | **Solo con mecanismos que esa version vieja YA tenga compilados.** Nada que se anada ahora al codigo la alcanza |

**La consecuencia manda sobre el diseno:** el unico control que I-47 tiene sobre una version anterior
es **lo que esa version ya ejecuta**. Y hay exactamente dos cosas ya compiladas y verificadas:

```csharp
// src/RackCad.Application/Persistence/SelectivePalletDesignStore.cs:43  — YA compilado en versiones anteriores
SchemaGuard.CheckReadable(document?.SchemaVersion, SelectivePalletDesignDocument.CurrentSchemaVersion, "El diseño del selectivo");
```
```csharp
// src/RackCad.Application/Persistence/SchemaGuard.cs  — LANZA, no devuelve null
throw new InvalidOperationException(
    what + " fue creado con una versión más nueva de RackCad (esquema " + storedVersion +
    "); actualiza la aplicación para abrirlo.");
```
```csharp
// src/RackCad.Application/Persistence/RackEmbedComposer.cs  — YA compilado: el SOBRE si preserva desconocidos
ExtensionData = source?.ExtensionData
```

### Alternativas, y por que F3 se rechaza

| Alt | Donde vive la referencia | Que hace una version **pre-I-47** con un rack vinculado |
|---|---|---|
| F1 | Diseno selectivo, `SchemaVersion` sigue `1.0` | **Lo desvincula en silencio.** Inaceptable |
| **F2 — DECIDIDA** | Diseno selectivo, con **promocion pegajosa de schema** | **Se niega a abrirlo**, con el mensaje ya compilado de `SchemaGuard`. Falla **visible**, cero corrupcion |
| ~~F3~~ | En el **sobre** (`RackEmbedDocument`) | **RECHAZADA en V3.** Lo conserva... y ese es justamente el problema (abajo) |
| E3 (V1) | Diseno, sin nada mas | Igual que F1 |

**Por que F3 se rechaza (C3-1).** El argumento a su favor era que una version vieja **preserva** el
binding, gracias a `[JsonExtensionData]` + `RackEmbedComposer`, ya compilados. Es cierto — y es
**insuficiente**, porque preservar bytes no es entender semantica:

> Una version pre-I-47 **conserva** la referencia en el sobre, pero **no sabe que existe**. Al abrir
> ese rack lee la propiedad de su **literal**, y con el literal **edita y redibuja**. El resultado es
> geometria **coherente con el literal** y **contradictoria con la autoridad** que una version nueva
> resolvera despues: el dibujo muestra una cosa y el modelo dice otra, sin que nada falle.

F3 convierte un fallo ruidoso en una **divergencia silenciosa entre dibujo y autoridad**, que es peor
que lo que intentaba evitar y es exactamente lo que el proyecto persigue desde I-03. **La preservacion
sin comprension no es compatibilidad.**

> **Nota conservada de V2, por honestidad:** V1 objetaba a F3 que «el vinculo quedaria duplicado por
> cada vista». Ese argumento estaba mal calibrado —el diseno entero ya viaja duplicado en el Xrecord de
> cada bloque-vista— y **no** es el motivo del rechazo. F3 cae por semantica, no por duplicacion.

### Contrato de compatibilidad — DECIDIDO (F2 con promocion pegajosa)

**F2 queda decidida** (C3-1): la referencia vive en el **diseno selectivo**, y el `SchemaVersion` del
documento se **promueve de forma pegajosa** (C3-2). «Pegajosa» significa **monotona**: sube y no baja.

```
documento 1.x que NUNCA ha llevado PropertyValues   ->  se sigue escribiendo 1.x
introducir PropertyValues                           ->  PROMUEVE a 2.0
documento ya almacenado como 2.x                    ->  se escribe 2.x SIEMPRE
    aunque se elimine el ultimo binding             ->  NO vuelve a 1.x
desvincular (D-11)                                  ->  materializa el literal y borra la referencia,
                                                        pero NO degrada el schema
minor posterior del mismo major                     ->  se preserva segun SchemaVersionPolicy
```

**Por que pegajosa y no «segun el contenido actual».** Una regla que mira solo el contenido hace que el
documento **oscile** `1.0 -> 2.0 -> 1.0` al vincular y desvincular. Eso tiene tres consecuencias malas y
ninguna buena:

1. **«Que versiones pueden abrir este rack» pasaria a depender de estado transitorio**, imposible de
   explicar a un usuario.
2. **El dibujo ya ha sido tocado por la semantica nueva.** Aunque este rack quede sin vinculos, el DWG
   puede tener un registro `ProjectVariables` y otros racks vinculados; devolverlo a 1.x afirma una
   compatibilidad que el archivo, como conjunto, ya no tiene.
3. Un ciclo de bajada y subida es una fuente clasica de perdidas sutiles. Monotono no la tiene.

**Coste aceptado, dicho sin adornos:** un rack que se vinculo **una vez** queda fuera del alcance de las
versiones anteriores **para siempre**, incluso despues de desvincularlo. Es deliberado.

**Lo que F2 garantiza, exactamente:**

- Un DWG sin registro de variables se lee como **registro vacio** (C-1). Sin migracion.
- Un rack **que nunca se vinculo** conserva `1.x` y **cualquier version anterior lo sigue abriendo y
  editando** con normalidad. El caso comun no se degrada.
- Un rack **promovido** es **ilegible** para una version anterior, que lo dice con un mensaje claro y
  **no lo modifica** — ni lo desvincula, ni lo redibuja con el literal.
- Una version I-47+ lee y escribe ambos.

**Lo que F2 NO garantiza:**

- **No** permite que una version anterior edite un rack promovido. Lo impide a proposito.
- **No** es reversible: la promocion no se deshace (es el punto).
- **No** hace nada retroactivo sobre documentos que una version anterior ya guardo.
- **No** protege campos anadidos **dentro** de tipos anidados sin version propia.
- **No cubre la ruta de BIBLIOTECA, y V3 lo afirmaba de mas** (H1). El guard que sostiene todo lo
  anterior vive en **un solo call site**,
  [`SelectivePalletDesignStore.cs:43`](../../src/RackCad.Application/Persistence/SelectivePalletDesignStore.cs).
  El **mismo DTO** llega a disco tambien como
  [`RackProjectDocument.SelectiveRack`](../../src/RackCad.Application/Persistence/RackProjectDocument.cs),
  y ahi **nadie comprueba su version**: `RackProjectStore` guarda el envoltorio, y sus cuatro payloads
  anidados hermanos **si** estan guardados (`SystemRegistry.Default` para FlowBed, Larguero, PushBack y
  Cantilever) — **el selectivo no**. La garantia de F2 queda **acotada al camino embebido en el DWG**.
  Lo que protege la ruta de biblioteca es **D-15** (nunca se exporta un documento promovido ni
  vinculado), no el mecanismo de schema.

### El portador del estado authored, y por que V3 no lo tenia (C4-2, cierra B2)

> **Correccion de V4.** V3 proponia `ResolveStickyWriteVersion(storedVersion, hasPropertyValues)` sin
> decir **de donde sale `storedVersion` en el momento de escribir**. El Architect verifico que **no
> sale de ningun sitio**: el camino de guardado es
> [`RackSelectivoCommands.cs:232`](../../src/RackCad.Plugin/RackSelectivoCommands.cs) →
> `SelectivePalletDesignDocument.From(design, id, name)`, que construye un documento **nuevo** a partir
> del **Domain**; y `grep -c SchemaVersion src/RackCad.Domain/Systems/Selective/SelectivePalletDesign.cs`
> devuelve **0**. El viaje `Document → Domain → Document` **destruye** la version almacenada, junto con
> `PropertyValues` y cualquier campo desconocido. **La promocion pegajosa, tal como V3 la escribio, era
> inimplementable.**

**El portador es el `SelectivePalletDesignDocument` DESERIALIZADO, no el Domain** (C4-2).

```
load   :  json  ->  SelectivePalletDesignDocument   <-- AUTHORED DOCUMENT: se CONSERVA vivo
                          |
                          +--> Resolve(authored, variables) -> SelectivePalletDesign efectivo  (D-04-bis)
                                     |
edit   :                             +--> dibujo / BOM / preview / editor
                          |
save   :  authored document ACTUALIZADO (no reconstruido)  ->  json
```

Entre `load`, `edit` y `save` el authored document debe preservar **cuatro** cosas:

| Debe sobrevivir | Por que |
|---|---|
| `SchemaVersion` | es la entrada de la invariante pegajosa; sin el no hay `stored` |
| `PropertyValues` | es el binding; perderlo **es** el desvinculado silencioso que F2 existe para impedir |
| El **literal authored** | C4-5: congelado e inactivo mientras haya binding |
| `ExtensionData` | preservacion de campos de un build posterior del mismo major |

**Regla vinculante: el guardado no puede reconstruirse exclusivamente con
`From(SelectivePalletDesign…)`.** Ese metodo se conserva para el caso que hoy sirve —crear un documento
**nuevo** desde un diseno— pero deja de ser el camino del **guardado de un rack existente**, que debe
**actualizar** el documento authored en vez de fabricar otro. Es trabajo real, esta reconocido en
**CF-3**, y no se da por hecho.

> **El alcance de esto es mayor que la version.** El mismo defecto explica por que un binding
> sobreviviria a guardar y reabrir pero **moriria en RACKDUPLICAR** (D-14): aquel restamp tambien
> re-serializa a traves del DTO. Un unico portador correcto cierra los dos casos.

### La invariante de escritura, ahora monotonica (C4-9, cierra M5)

V3 escribia la regla en tres ramas y el Architect encontro una **cuarta implicita que degradaba**: con
`stored = "3.x"` y la constante `"2.0"`, `ResolveWriteVersion` ve majors distintos y devuelve `"2.0"`.
Era inalcanzable solo porque el guard de lectura dispara antes — es decir, la seguridad dependia de
**otro** componente, no de la propia funcion.

**Se especifica como invariante, con una rama de ERROR explicita:**

```
resolveStickyWriteVersion(stored, hasPropertyValues) :

  1. major(stored) > READ_MAJOR_SOPORTADO   ->  ERROR.  No se escribe. No se degrada. No se abre.
  2. major(stored) == 2                     ->  permanece 2.x, preservando el minor MAYOR
  3. major(stored) <= 1  y  hasPropertyValues  ->  "2.0"          (promocion)
  4. major(stored) <= 1  y  no hasPropertyValues -> linea 1.x     (camino legado, intacto)
```

- **La invariante que la funcion garantiza por si sola: el resultado NUNCA es de major inferior al
  almacenado.** No delega esa propiedad en el guard de lectura.
- **`READ_MAJOR_SOPORTADO` de I-47 es `2`**: el build lee y escribe la linea `2.x`. Sin esto,
  `SchemaGuard` rechazaria los documentos que el propio build acaba de promover.
- **`hasPropertyValues` se evalua sobre el AUTHORED DOCUMENT QUE SE VA A ESCRIBIR**, no sobre el que se
  leyo: vincular y guardar en el mismo paso debe promover.
- **Leer y escribir dejan de compartir constante.** `SelectivePalletDesignDocument` necesita la linea
  legada `1.x` que aun escribe y la promovida `2.0`; y la constante de **lectura** es `2.x`. Debe ser
  explicito, no implicito.

`SchemaVersionPolicy.ResolveWriteVersion` **no se modifica**: el helper nuevo lo usa para el caso 2
(preservar el minor mayor dentro del mismo major). Los otros cuatro stores siguen igual.

### Se conserva `[JsonExtensionData]` en el DTO selectivo

**Si, pero con la garantia correcta y por un motivo distinto del que daba V1.**

- **No** protege frente a versiones pre-I-47. Ninguna afirmacion de este documento depende ya de eso.
- **Si** protege de I-47 en adelante: una version N preservara los campos que escriba una version N+1
  del mismo major.
- Corrige una asimetria real, pero **no la que V3 afirmaba** (L2). El conjunto verificado con
  `[JsonExtensionData]` es de **seis**: `RackEmbedDocument`, `RackProjectDocument`,
  `PushBackDesignDocument`, `FlowBedDocument`, `CantileverLineDocument` y `LargueroDocument`. El del
  slice **no es la unica excepcion**: `RackFrameProjectDocument` tambien esta **versionado y sin**
  extension data — y esta **anidado dentro** del propio documento selectivo
  (`SelectivePalletDesignDocument.PostCabeceras`). Ese caso anidado pertenece a «Lo que F2 NO
  garantiza» y se declara alli, no se resuelve aqui.

Es, por tanto, una mejora **justificada por si misma**, no un prerequisito del que dependa la
compatibilidad. Si el dueno prefiere no tocar el DTO selectivo, **F2 sigue funcionando igual**.

### D-08 — `VariableId` inexistente = **error visible**, jamas fallback silencioso

Un binding que apunta a una variable que ya no existe es alcanzable (borrado, dibujo tocado por otra
version, edicion manual). **Nunca** debe resolverse cayendo al literal: eso cambiaria la geometria en
silencio.

| Alt | Comportamiento | Tradeoff |
|---|---|---|
| A | Caer al literal | Silencioso y **por eso inaceptable**: el dibujo cambia y nadie se entera |
| B | Excepcion cruda | Visible, pero rompe la operacion sin mensaje util |
| **C** | **Preflight** que aborta **toda** la operacion con un mensaje que **nombra el rack y la variable** | Consistente con lo que el repositorio ya hace |

**Recomendacion: C**, y con los precedentes ya escritos, no con maquinaria nueva:

```csharp
// src/RackCad.Application/Persistence/KindDispatch.cs:137  — el mensaje vive en Application, es puro y testeable
public static string NotRecognized(string kind) => "RackCad: tipo de rack no reconocido (" + kind + ").";
```

```csharp
// src/RackCad.Plugin/RackCommandSupport.cs — PreflightInnerSources
// "...If ANY block carries an incompatible-MAJOR or wrong-kind inner design, the whole edit ABORTS
//  with a visible message and NO block is modified (no partial update)."
```

El mensaje de «variable inexistente» debe vivir en **Application** (como `KindDispatchMessages`), de
modo que la suite Core pueda probarlo sin AutoCAD, y el Plugin solo lo muestre.

---

### D-08-bis — Reparacion explicita de una referencia rota (C4-7, cierra H3)

> **NUEVA en V4.** El Architect registro (H3) que V3 creaba **un estado sin salida**: D-08 aborta al
> resolver una referencia rota; D-11 exige que el desvinculado materialice un **valor efectivo** que
> por definicion no existe; y D-16b pone el boton «Desvincular» **dentro** del editor, que D-08 impide
> abrir. El rack quedaba **irreparable**.

**La resolucion normal no cambia**: referencia rota ⇒ **error visible**, **sin fallback**. D-08 sigue
intacto y esto **no** lo debilita.

Lo que se anade es una **operacion distinta y explicita**, `RepairBroken`:

```
RepairBroken(rack, propertyId) :
    1. NO se intenta resolver: se sabe que no hay valor efectivo.
    2. Se AVISA al usuario, antes de actuar:
         - la variable referida no existe;
         - NO hay valor efectivo que materializar;
         - se usara el LITERAL AUTHORED almacenado;
         - la GEOMETRIA PUEDE CAMBIAR.
    3. Solo con confirmacion explicita:
         - se quita el binding;
         - el literal authored queda como valor activo (ya estaba persistido, C4-5);
         - el SchemaVersion PERMANECE 2.x  (pegajoso, C4-9: no baja);
         - se redibuja por el camino de D-13.
```

**Por que esto es reparacion y no el fallback que D-08 prohibe** — la distincion es exacta y merece
enunciarse, porque desde fuera el valor final es el mismo:

| | Fallback silencioso (**prohibido**) | `RepairBroken` (**permitido**) |
|---|---|---|
| Quien lo inicia | el sistema, al resolver | el **usuario**, con una accion nombrada |
| Cuando ocurre | en cualquier lectura | solo tras confirmar |
| Que sabe el usuario | nada | que no hay efectivo y que **la geometria puede cambiar** |
| Estado final | binding intacto, valor mentido | binding **retirado**, estado coherente |

El literal authored **esta ahi** precisamente por C4-5: se congelo al vincular y ningun cambio de
variable lo toco. Es lo unico honesto que queda por usar cuando la autoridad desaparecio, y usarlo con
aviso **no** es afirmar que sea el valor correcto — es devolver el rack a un estado que el usuario
puede corregir.

> **`RepairBroken` es una de las ocho operaciones confirmadas** de la unidad de atomicidad (D-13,
> C4-11), y por tanto se propaga y se commitea como cualquier otra.

Alternativa descartada: **recrear la variable ausente**. Adivinaria un valor que nadie autorizo y
resucitaria una autoridad borrada a proposito.

## 5. Ciclo de vida de una variable

### D-09 — Renombrar es por `Id`

Consecuencia directa de D-02: renombrar cambia `Name` y **nada mas**. Ningun consumidor se toca,
ningun rack se redibuja, no hace falta preflight. Es la razon de ser de un id independiente del
nombre, y por eso no tiene alternativas que comparar.

### D-10 — Borrar: bloquear vs referencias rotas

> Aviso de notacion: las etiquetas `F1`/`F2`/`F3` de esta decision son **locales a D-10** y no tienen
> relacion con las `F1`/`F2`/`F3` de D-07. El dueno decidio **F1 aqui** y **F2 alli**.

| Alt | Comportamiento | Tradeoff |
|---|---|---|
| **F1** | **Bloquear** mientras existan consumidores, **listandolos** | Ningun estado roto se crea nunca. Exige el barrido de consumidores — que **ya existe** (D-12). El usuario debe desvincular antes: mas pasos, pero ninguno sorpresa |
| F2 | Permitir y dejar referencias rotas | «Barato» al borrar y caro despues: traslada el fallo a un momento futuro y a otra persona |
| F3 | Borrar y **desvincular automaticamente** todos los consumidores | Comodo, pero **reescribe en silencio el diseno persistido de N racks** como efecto colateral de un borrado. Destructivo y sorprendente |

**DECIDIDA: F1** (C3-3). No es ya una recomendacion: borrar una variable **con consumidores se
bloquea**, y la operacion **lista los consumidores** que lo impiden. Con dos matices que forman parte
de la decision:

1. **F3 puede existir SOLO como accion explicita y separada** —«desvincular todos materializando» y
   despues borrar—, **nunca** como efecto automatico del borrado. El usuario tiene el camino comodo
   **y** ve lo que va a pasar. El desvinculado de esa accion es el de D-11: **materializa** el valor
   efectivo, asi que ningun rack cambia de geometria al ejecutarla.
2. **La ruta de error de F2 se implementa igualmente.** Aunque el borrado se bloquee, una referencia
   colgante sigue siendo alcanzable por otros caminos (otra version, edicion externa), asi que D-08 no
   es opcional por haber elegido F1.

### D-11-bis — VINCULAR (C4-1, cierra B1)

> **NUEVA en V4.** El Architect registro (B1) que V3 definia **21 decisiones** y **ninguna** gobernaba
> el acto de vincular — la operacion con **mayor efecto geometrico** y la puerta de entrada de todo
> ID22A. C-3 hablaba de «cambiar una variable», no de «crear un vinculo», asi que D-13 no la cubria: una
> implementacion conforme podia crear un binding y **dejar el dibujo mostrando el valor viejo**.

**`Link(rack, propertyId, variableId)` sobre un rack EXISTENTE, en este orden:**

```
1. CONSERVAR el literal authored           -> VerticalClearance persistido NO se toca (C4-5).
                                              Queda congelado e INACTIVO.
2. ANADIR la referencia                    -> PropertyValues["selective.verticalClearance"]
                                                 = { kind: "projectVariable", variableId: ... }
3. PROMOVER el schema                       -> primera aparicion de PropertyValues => 2.0  (C4-9)
4. RESOLVER el efectivo                     -> SelectiveEffectiveDesignResolver.Resolve(...)  (D-04-bis)
5. REDIBUJAR TODAS SUS VISTAS               -> preflight / lote / commit unico / UN Regen  (D-13)
```

**El binding gobierna de inmediato.** No hay estado intermedio en el que el vinculo exista y el dibujo
siga mostrando el authored: los pasos 1-5 son **una sola operacion confirmada** (C4-11) y, si el
preflight falla, **no se escribe ninguno de ellos** — ni el binding, ni la promocion, ni la geometria.

**Rack SIN vistas todavia** (definicion sin bloques dibujados, o un diseno que aun no se ha insertado):
se **guarda el binding** y no hay nada que redibujar. **Las inserciones futuras usan el efectivo**,
porque toda insercion parte del diseno resuelto por D-04-bis. No es un caso especial del contrato: es el
mismo contrato con el conjunto de vistas vacio.

**Simetria con UNLINK, que es lo que hace el par comprensible:**

| | `Link` | `Unlink` (D-11) |
|---|---|---|
| Literal authored | se **congela**, no se toca | se **sobrescribe** con el efectivo |
| `PropertyValues` | se **anade** la entrada | se **quita** la entrada |
| Schema | **promueve** a `2.0` | **permanece** `2.x` (pegajoso) |
| Efecto geometrico | **el que la variable imponga** — puede ser visible, y es el objetivo | **nulo** |
| Redibujo | todas las vistas, por D-13 | todas las vistas, por D-13 |

**Que el efecto geometrico de `Link` sea visible es correcto y deliberado**: el usuario esta pidiendo
que el rack pase a obedecer al proyecto. Lo que seria un defecto es que **no** se viera hasta la
siguiente edicion, que es exactamente lo que V3 permitia.

### D-11 — Desvincular: materializar el valor efectivo

| Alt | Al desvincular, el literal queda… | Efecto geometrico inmediato |
|---|---|---|
| **G1** | **= valor efectivo actual** (el de la variable) | **Ninguno.** El rack sigue dibujando exactamente igual |
| G2 | = el literal anterior al vinculo | **Salto visible**: el rack cambia de forma al desvincular |
| G3 | = default o cero | Peor version de G2 |

**DECIDIDA: G1 (materializar).** Es la unica con efecto geometrico nulo en el instante de desvincular,
que es lo que hace la operacion segura y comprensible.

**Secuencia exacta (C4-6):**

```
Unlink(rack, propertyId) :
    1. RESOLVER el efectivo actual                     (D-04-bis)
    2. ESCRIBIR ese efectivo en el literal authored    <-- paso ACTIVO
    3. QUITAR el binding de PropertyValues
    4. El SchemaVersion PERMANECE 2.x                  (pegajoso, C4-9: nunca baja)
    5. REDIBUJAR por el camino de D-13                 -> efecto geometrico NULO
```

**Aviso de implementacion, porque G2 es el bug facil:** si al limpiar el binding se deja el campo
literal como estaba, el resultado **es G2 sin haberlo elegido** — el rack salta a un valor viejo.
El paso 2 es **activo** y merece su propia prueba (D-17).

**El paso 4 no es una omision.** El schema no baja aunque el rack quede sin ningun binding: la
promocion es monotona (C4-9) y el motivo esta en D-07 — un documento que oscila hace que «que versiones
pueden abrir este rack» dependa de estado transitorio.

**Caso rota:** si la referencia esta rota, el paso 1 **no puede** ejecutarse. Ese caso **no** entra por
aqui: entra por **D-08-bis** (`RepairBroken`), que avisa de que no hay efectivo y de que la geometria
puede cambiar. `Unlink` **nunca** cae al literal en silencio.

---

## 6. Propagacion (C-3)

### D-12 — Descubrimiento de consumidores, deduplicado por `RackId`

El barrido ya existe y ya es de nivel dibujo: `RackBlockFinder.ScanEnvelopes` recorre las
**definiciones** de la `BlockTable`. Pero devuelve **una entrada por bloque-vista**, y un rack tiene
varias, asi que hay que agrupar.

```csharp
// src/RackCad.Plugin/RackInventarioCommands.BomTotal.cs — el precedente de agrupacion
if (embed == null || string.IsNullOrWhiteSpace(embed.Id) || string.IsNullOrWhiteSpace(embed.Kind)) { continue; }
var copies = envelope.DirectReferenceCount;
if (!byRack.TryGetValue(embed.Id, out var aggregate)) { byRack[embed.Id] = new RackAggregate { ... }; }
```

**Deduplicacion recomendada: por `embed.Id` (`OrdinalIgnoreCase`)** — un `RackId` = un rack logico,
aunque tenga frontal, lateral y planta.

**Y aqui hay una diferencia con el precedente que hay que decidir a proposito.** El Discovery (H-02)
registro que conviven **dos politicas de filtro**: `FindRackBlocks` exige solo el GUID;
`RACKBOMTOTAL`/`RACKLISTA` exigen `Id` **y** `Kind`, y **saltan en silencio** (`continue`) lo que no
cumple. Para un BOM, saltar un bloque ilegible produce un total incompleto y visible. **Para una
propagacion, saltar en silencio produce un dibujo incorrecto y mudo**: un rack vinculado que no se
redibuja queda mostrando el valor viejo.

#### Las tres reglas del descubrimiento (C4-13, cierran M1 y M4)

V3 decia solo «no hereda la politica de `continue`; aborta ante un bloque sin `Id`, sin `Kind` o
illegible». El Architect encontro que eso era **a la vez demasiado estrecho y demasiado ancho**: no
decia nada del filtro de referencias (M1) y bloqueaba por racks corruptos ajenos (M4).

**1. NO se filtra por `DirectReferenceCount`.** El precedente citado arriba lo usa porque un BOM cotiza
lo **colocado**; una propagacion mantiene coherente lo **persistido**. **Una definicion vinculada con
cero referencias sigue siendo consumidor** y debe redibujarse: si no, queda con geometria vieja y la
saca a la luz la siguiente insercion (`RACKLAYOUT` inserta referencias sobre `seed.DefinitionId`).

**2. Los consumidores se identifican POSITIVAMENTE, con un probe puro.** No se clasifica por lo que
falta, sino por lo que se encuentra: se busca en `PropertyValues` una entrada cuyo valor referencie el
`VariableId` objetivo.

El probe es **tri-estado**, no booleano — la distincion es la que N1 obligo a hacer explicita:

```
probe(vistaHermana, variableIdObjetivo) : POSITIVE | NEGATIVE | INDETERMINATE   // PURO, suite Core

    el payload RackCad de esa vista NO deserializa   -> INDETERMINATE
    entrada = authoredDocument.PropertyValues[propertyId]        // comparacion Ordinal (C4-14)
    entrada != null && entrada.kind == "projectVariable"
                    && idIgual(entrada.variableId, objetivo)     -> POSITIVE
    en otro caso                                                 -> NEGATIVE
```

`NEGATIVE` significa **«se pudo leer y demostrablemente no referencia el objetivo»**. `INDETERMINATE`
significa **«no se pudo leer, asi que no se puede afirmar nada»**. Nunca se colapsan.

**3. El alcance del analisis: solo el Selectivo.** ID22A considera para bindings **unicamente**
`kind=selective` (C4.2-1). Los racks `dynamic`, `pushback`, `cantilever`, `cabecera` y `cama` no
participan: no tienen `PropertyValues` en esta iniciativa y su payload **no es** un
`SelectivePalletDesignDocument`. V4.1 decia «cualquier `RackId`» y eso era un error de tipo.

#### Autoridad authored multi-vista (V4-01) — corregido en V4.2 por N1

> **Correccion de V4.2.** V4.1 ponia la igualdad authored completa **antes** de clasificar
> consumidores, sobre **cualquier** `RackId`. El Architect (N1) demostro que eso **contradice
> C4-13/M4**: un rack cuyas vistas divergen en un campo cualquiera, y que **demostrablemente no
> referencia** la variable objetivo, abortaria **toda** operacion de variable del dibujo — justo lo
> que M4 habia cerrado. La correccion no es debilitar V4-01: es **ordenar** probe y comparacion, y
> distinguir **dos familias de operacion**.

##### Familia A — operaciones TARGET-VARIABLE

Aplica a **`ChangeValue`**, a la **deteccion de consumidores de `Delete`** y a **`UnlinkAllAndDelete`**.
Aqui el conjunto de racks afectados **se descubre**, no se conoce de antemano.

```
1. ScanEnvelopes
2. quedarse SOLO con kind = selective                         (C4.2-1)
3. agrupar las vistas Selectivo por RackId
4. ANTES de elegir autoridad authored:
   ejecutar el probe contra el VariableId objetivo EN CADA VISTA HERMANA, independientemente

   por RackId:
     TODAS NEGATIVE                  -> IGNORAR el rack.
                                        NO se le exige igualdad authored para esta operacion.
     alguna INDETERMINATE            -> ABORTAR toda la operacion.
     mezcla POSITIVE y NEGATIVE      -> ABORTAR: las hermanas DISCREPAN sobre el binding objetivo.
     TODAS POSITIVE                  -> deserializar los authored completos;
                                        exigir igualdad semantica COMPLETA;
                                          iguales    -> UN consumidor logico;
                                          divergentes-> ABORTAR.
```

**La linea que cierra N1 es la primera:** un rack cuyas vistas divergen pero que **todas** dicen
`NEGATIVE` **se ignora**, y su divergencia —real— no bloquea nada, porque es **ajena a esta
operacion**. Sigue siendo un defecto del dibujo; simplemente no es asunto de esta variable.

**Y la tercera es la que conserva V4-01:** si una hermana dice `POSITIVE` y otra `NEGATIVE`, la
divergencia esta **en el binding mismo**. No hay respuesta correcta que no sea abortar.

**Nunca se usa la primera vista, ni la frontal, ni la mayoritaria.** Ninguna heuristica, en ninguna
de las cuatro ramas.

##### Familia B — operaciones TARGET-RACK

Aplica a **`Link`**, **`Unlink`** y **`RepairBroken`**. Aqui el `RackId` **ya esta en scope por
decision explicita del usuario**, que pico ese rack.

```
NO se usa el consumer probe para decidir si se inspecciona.   <-- el rack YA esta en scope

antes de mutar:
    recoger TODAS sus vistas hermanas Selectivo PRESENTES
    deserializar el authored COMPLETO de cada una
    exigir UNA sola autoridad authored logica
      divergencia o indescifrable -> ABORT, y MutationPlan VACIO (D-13)
```

**Por que esta separacion es imprescindible, y no una comodidad:** un rack **todavia no vinculado**
da `NEGATIVE` en **todas** sus vistas. Con las reglas de la Familia A se **ignoraria** — y `Link`
es precisamente la operacion que se aplica a un rack no vinculado. Sin la Familia B, **`Link` no
podria vincular nada**.

##### Sin barrido innecesario

| Operacion | Consumer scan | Redibujo |
|---|---|---|
| `Create` | **no** | no |
| `Rename` | **no** | **no** — D-09 intacto: cambia `Name` y nada mas |
| `ChangeValue`, `Delete`, `UnlinkAllAndDelete` | si (Familia A) | los consumidores |
| `Link`, `Unlink`, `RepairBroken` | **no** (Familia B) | las vistas de ese rack |

`Create` no puede tener consumidores: la variable acaba de nacer. `Rename` no toca ningun valor
efectivo, porque las referencias van por `VariableId` y no por `Name` (D-02, D-09).

##### Que significa «la misma autoridad authored»

Comparacion **por valor sobre el documento authored**, no byte a byte, y **funcion pura** de
Application:

```
mismaAutoridadAuthored(docs : IReadOnlyList<SelectivePalletDesignDocument>) : bool      // PURO, suite Core
```

Entra **todo lo que puede cambiar el diseno efectivo o el estado de vinculo**: los literales authored,
`PropertyValues` **y** `SchemaVersion` — una vista promovida a `2.x` junto a una hermana en `1.x`
**es** divergencia, y es el rastro que deja un `Link` interrumpido a mitad del bucle de vistas.

Quedan **fuera** los campos del **sobre** que legitimamente difieren por vista: `View` y `Section`.

> **Vistas fantasma:** un bloque-vista cuyo fondo o corte ya no existe se **borra** en el flujo actual
> (`staleViewBlocks`), no se compara. Una hermana ausente **no es** divergencia: la divergencia es que
> dos hermanas **presentes** discrepen.

#### Reparacion de la divergencia: quien puede, y quien no

**Ninguna operacion de variable repara.** `ChangeValue`, `Link`, `Unlink`, `Delete`, `RepairBroken`,
`Create`, `Rename` y `UnlinkAllAndDelete` **abortan** ante divergencia y **no** la arreglan. Reconciliar
significa **elegir cual de los disenos divergentes es el bueno**, y eso es una decision del usuario
sobre **su** rack, no un efecto colateral de tocar una variable del proyecto.

**Quien si puede: `RACKEDITAR` desde una vista que el usuario elige explicitamente.** Ese flujo ya tiene
la forma correcta y no hay que inventar nada:

1. el usuario **pica una referencia concreta** — `RackCommandSupport.PickRackBlock` lee el payload de
   **esa** definicion, asi que **la vista elegida es la autoridad authored**;
2. al guardar, el diseno se serializa **una vez** (`designJson`, arriba) y se escribe en **todas** las
   hermanas via `FindRackBlocks(id)`.

El resultado es que **guardar restampa todas las vistas desde la autoridad seleccionada**, que es
exactamente una reconciliacion — deliberada, del usuario, y sobre una vista que el eligio.

> **Requisito a preservar, no mecanismo nuevo (V4-01, punto 4).** Esta propiedad **existe hoy** y V4.1
> **no** inventa otra. Lo que hace es declararla **requisito del contrato**: la implementacion de V4 no
> puede romperla. El riesgo es concreto y nace de **C4-2**: si el «portador del estado authored» se
> implementa **por vista** —cada vista actualizando su propio documento deserializado— el restampado
> desaparece y **cada hermana conservaria su divergencia**, convirtiendo un defecto reparable en
> permanente.
>
> **Por tanto el portador de C4-2 es UNO por `RackId`** —el authored document de la vista elegida por el
> usuario— **y no uno por vista**. Es la lectura correcta de C4-2 y aqui queda explicita.
>
> Si tras implementar V4 el flujo real de `RACKEDITAR` **no** pudiera garantizarlo, eso **se senala
> como incumplimiento del requisito** y se decide entonces; **no** se sustituye por otro mecanismo de
> reconciliacion inventado en este documento.

### D-13 — Preflight completo, commit unico y **un solo** `Regen`

> ### ⚠ Correccion de V2 (C2-6): V1 daba por atomico un lote que no lo es
>
> V1 listaba «redibujo en sitio» y «un solo `Regen`» como piezas ya existentes y presentaba el bucle
> sobre N racks como si heredara la atomicidad de RACKEDITAR. **No la hereda.** `RedrawInPlace`
> **abre y commitea su propia transaccion en cada llamada**:

```csharp
// src/RackCad.Plugin/Systems/Shared/SystemBlockWriter.cs — RedrawInPlace
using (document.LockDocument())
{
    BlockLibraryImporter.EnsureForPlan(database, plan);
    using (var transaction = database.TransactionManager.StartTransaction())
    {
        outcome = drawer.RedefineSystemBlock(database, transaction, blockId, plan, out staleDefs);
        RackBlockData.Write(transaction, blockId, payloadJson);
        transaction.Commit();                       // ← COMMIT por bloque
    }
    LateralHeaderDrawer.PurgeUnreferenced(database, staleDefs);   // ← ademas, purga DESPUES del commit
    ApplyRegen(document, regen);
}
```

Y el metodo entero esta envuelto en `try/catch` que devuelve `HeaderPlacementResult.Failure(...)`. Por
tanto, un bucle sobre N racks que falle en el rack **k** deja **k-1 racks ya commiteados** y el resto
sin tocar: el dibujo queda con **dos valores distintos para la misma variable**, que es exactamente el
estado que C-3 prohibe. Esto **no** lo arregla anadir un preflight: el preflight evita empezar mal, no
deshace lo ya commiteado si algo falla a mitad.

**Lo que realmente existe, corregido:**

| Pieza | Estado real |
|---|---|
| Preflight de lote que aborta antes de trabajar | **existe** — `KindHandlerDispatch.TryResolveAll`, `RackCommandSupport.PreflightInnerSources` |
| Redibujo de **un** bloque | **existe** — `RedrawInPlace`, pero **con transaccion propia** |
| **Atomicidad sobre N bloques** | **NO existe** — y V1 decia que si |
| `Regen` unico | **existe** como convencion (`regen: false` + un `Regen()` final), no como garantia del writer |
| Aplicar diseno y redibujar **sin ventana** | **NO existe** (Discovery H-03) |

**Contrato exigido (los cuatro puntos, no negociables):**

1. **Preflight completo antes de mutar nada**, en **dos mitades nombradas** (§preflight, abajo).
   Cualquier fallo de cualquiera de las dos aborta **antes** de la primera escritura.
2. **Coherencia conjunta**: el registro `ProjectVariables` **y** todos los consumidores quedan en un
   estado mutuamente consistente. No se admite «variable ya cambiada, racks a medio redibujar».
3. **Commit unico, o un mecanismo equivalente con rollback real.** Una operacion, un punto de no
   retorno.
4. **Un solo `Regen`**, al final, despues del commit.

#### Las DOS mitades del preflight (N6, corregido en V4.3)

> **Correccion de V4.3.** V4.2 dejo D-12 correcto pero **D-13 conservaba el viejo «Paso 0»**, redactado
> como si la igualdad authored fuera **universal y anterior** a la clasificacion. Eso contradecia al
> propio D-12 corregido. **La expresion «Paso 0» queda retirada** y el preflight se parte en las dos
> mitades que de hecho tiene, que viven en **capas distintas**.

##### A. Preflight SEMANTICO — Application, puro

Produce **`VariableMutationPreflightResult`** (§artefacto). **Sigue EXACTAMENTE D-12**, sin anadir ni
reordenar nada:

```
target-variable  (ChangeValue · consumidores de Delete · UnlinkAllAndDelete)
    1. probe TRI-ESTADO por vista hermana, PRIMERO
    2. igualdad authored completa SOLO en los grupos que resultaron relevantes

target-rack      (Link · Unlink · RepairBroken)
    igualdad authored completa del rack que el usuario selecciono explicitamente
```

**Fallo ⇒ `Error` + `MutationPlan` vacio.** Sin `ObjectId`, sin `Database`, sin `Transaction`: esta
mitad se verifica **entera en la suite Core**.

##### B. Preflight FISICO — Plugin / AutoCAD

Corre **solo despues** de un `Success` del semantico, y **todavia antes** de mutar:

```
1. resolver los ObjectId de las vistas implicadas
2. construir los planes de TODAS las vistas          (frontal · planta · lateral corte a corte)
3. comprobar READ-ONLY que existen TODAS las definiciones de bloque requeridas
       blockTable.Has(nombre) sobre los nombres de cada plan
4. cualquier fallo  ->  CERO mutacion, nombrando lo que falta
```

**Solo despues de que ambas mitades pasen** se abre el `DocumentLock` y la `Transaction`, se escriben
registro, geometria y payloads, y se hace **UN** commit. Y **despues** del commit: purga acumulada y
**UN** `Regen`.

```
A. semantico (puro)  ->  B. fisico (AutoCAD, read-only)  ->  lock + transaction
                                                              registro + geometria + payloads
                                                              UN commit
                                                          ->  purga acumulada + UN Regen
```

La separacion no es cosmetica: **A es testeable sin AutoCAD y B no lo es**, asi que meter en B algo que
podia estar en A reduce lo que la CI puede garantizar.

**Frontera arquitectonica EXIGIDA (no se implementa aqui) — ahora sobre TODOS los writers del slice
(C4-3, cierra B3).**

> **Correccion de V4.** V3 nombraba **`SystemBlockWriter`** como el unico orquestador que commitea por
> su cuenta. El Architect verifico que **las vistas laterales del propio slice no pasan por el**:
> [`RackSelectivoCommands.cs:154`](../../src/RackCad.Plugin/RackSelectivoCommands.cs) llama, en un bucle
> por corte, a `lateralService.RedrawInPlace(...)`, es decir a
> [`LateralHeaderDrawService.RedrawInPlace`](../../src/RackCad.Plugin/Drawing/LateralHeaderDrawService.cs),
> **un segundo writer independiente** que abre su propio `LockDocument`, **llama a
> `BlockLibraryImporter.EnsureForPlan`**, abre su propia transaccion, **commitea**, purga y regenera.
> Con la frontera de V3, «una transaccion, un commit» era **inalcanzable para el slice elegido**, y
> C3-5 —que prohibe `EnsureForPlan` dentro de la operacion— era **inimplementable en ese camino**.

**Los dos writers quedan nombrados, y los tres tipos de vista bajo la misma transaccion:**

| Vista del Selectivo | Writer de hoy | Estado de hoy |
|---|---|---|
| **Frontal** | `SelectiveFrontalDrawService` → `ViewBlockDraw` → `SystemBlockWriter.RedrawInPlace` | transaccion propia + commit |
| **Planta** | `SelectivePlantaDrawService` → `ViewBlockDraw` → `SystemBlockWriter.RedrawInPlace` | transaccion propia + commit |
| **Lateral (cortes)** | **`LateralHeaderDrawService.RedrawInPlace`**, una llamada **por corte** | `EnsureForPlan` + transaccion propia + commit + purge + regen, **todo por su cuenta** |

```
PROPUESTO — una frontera transaction-aware que cubra los TRES:

  el orquestador abre UN document lock y UNA transaccion, y entonces:
      - escribe el registro ProjectVariables                       (misma transaccion)
      - redefine FRONTAL  bajo esa transaccion                     (sin commit propio)
      - redefine PLANTA   bajo esa transaccion                     (sin commit propio)
      - redefine LATERAL, corte a corte, bajo esa transaccion      (sin commit propio)
      - escribe los payloads de todas las vistas                   (misma transaccion)
      - UN commit
  despues del commit:
      - purga ACUMULADA de las definiciones huerfanas de todos los racks y todas las vistas
      - UN Editor.Regen()
```

La pieza que lo hace viable ya existe y no cambia de firma: `LateralHeaderDrawer.RedefineSystemBlock`
**ya recibe la `Transaction`**, y los **dos** writers la usan. Lo que cambia es **quien crea y commitea
la transaccion**, en ambos.

**`EnsureForPlan` / cualquier importacion queda PROHIBIDA dentro de la propagacion** (C3-5), y esto
ahora **incluye la ruta lateral**, que es donde de hecho estaba. En su lugar, **verificacion read-only
antes de mutar**:

```
preflight, de solo lectura, sobre el plan de CADA vista de CADA consumidor:
    para cada nombre de bloque del plan (LooseInstances + Headers.SelectMany(Instances)):
        blockTable.Has(nombre)  ?
    si falta alguno  ->  ABORTA, nombrando los que faltan
                         antes de tocar la variable y antes de tocar ningun consumidor
```

**No se importa ni se repara la biblioteca como efecto colateral** de cambiar una variable. Razon
verificada, ademas de la transaccional: con el archivo de biblioteca ausente, `EnsureBlocks` **devuelve
0 en silencio**, asi que apoyarse en el convertiria una biblioteca ausente en **geometria incompleta sin
aviso**.

#### El artefacto del preflight: `VariableMutationPreflightResult` (N3, nuevo en V4.2)

> **Correccion de V4.2.** El Architect (N3) registro que la prueba 17 aseveraba sobre un «plan de
> mutacion» que el contrato **no definia en ninguna parte**. Se define aqui, y **solo** con lo que hace
> falta: **no** es un framework generico.

```
VariableMutationPreflightResult          // PURO, en Application. Salida del preflight.
    Success | Error(mensaje visible)
    MutationPlan                          // vacio/null cuando hay Error

MutationPlan                              // SOLO mutaciones SEMANTICAS previstas
    registryMutation                      // que le pasa al ProjectVariablesDocument
    rackMutations : por RackId
        authoredOutput                    // el authored document que se persistira
        effectiveOutputRequerido          // el diseno efectivo que la geometria debe reflejar
```

**Lo que el `MutationPlan` NO contiene** —y es la mitad importante de la definicion—:

```
NADA de AutoCAD:   ObjectId · Database · Transaction · BlockTableRecord · Editor
```

Por eso es **puro** y por eso la prueba 17 puede verificarse **entera en Core**, sin dibujo.

**Regla, sin excepcion: preflight fallido ⇒ `MutationPlan` vacio.** No un plan parcial, no un plan con
los racks que si pasaron. Vacio. Es la forma verificable de la promesa «cero mutacion» de D-12 y
C4-11, y lo que convierte «aborta» en algo que se puede **probar** en vez de solo afirmar.

**El Plugin viene despues.** Con un `Success` en la mano, el Plugin traduce el plan a las mutaciones
**fisicas** —resolver `ObjectId`, verificar definiciones de bloque, abrir el lock y la transaccion— bajo
las reglas de D-13. El plan dice **que** debe cambiar; el Plugin decide **como** lo escribe.

#### La unidad de atomicidad del registro (C4-11, cierra H4)

El Architect registro que el registro entero vive en **un** `Xrecord` —luego toda escritura es
todo-o-nada sobre **todas** las variables— mientras que la unidad de D-13 era **una variable**. V3 no
resolvia el desajuste.

**La unidad logica es UNA OPERACION CONFIRMADA**, no una variable y no un documento:

```
Create · Rename · ChangeValue · Delete · Link · Unlink · RepairBroken · UnlinkAllAndDelete
```

Cada una es **una** unidad: un preflight, una transaccion, un commit, un `Regen`.

**Si una UI futura entrega varios cambios en un solo Apply, el change-set ENTERO es una sola unidad**, y
entonces:

- el conjunto de consumidores es la **UNION** de los consumidores de todos los cambios del change-set;
- el preflight cubre esa union completa **antes** de mutar nada;
- **nunca hay apply parcial**: o entran todos los cambios y todos sus consumidores, o no entra ninguno.

Eso hace que la granularidad del `Xrecord` deje de ser un problema: escribir el documento completo del
registro **es** la forma natural de confirmar una unidad, sea de un cambio o de veinte.

#### Viabilidad de la transaccion unica

**Deja de ser una pregunta de contrato** (C3-6). El contrato es el de los cuatro puntos de arriba, y se
sostiene: la importacion de bloques queda **fuera** por el punto anterior —**en los dos writers**, no
solo en uno—, y `RedefineSystemBlock` ya esta escrito para trabajar sobre una transaccion ajena y ya lo
usan ambos.

Lo que sigue siendo necesario es **comprobarlo ejecutando**, y eso es un **gate de implementacion y de
Owner Validation** (§14, CF-5'), no una condicion previa del Consensus Freeze ni una incognita de
diseno.

**Alternativa descartada: propagacion perezosa** (marcar los racks y redibujar cuando se abran).
Evitaria el barrido, pero contradice C-3 —«en una operacion»— y dejaria el dibujo en un estado
intermedio donde dos racks vinculados a la misma variable muestran valores distintos.

**Riesgo abierto y honesto**: no hay medicion de cuanto tarda esto en un dibujo grande. El repositorio
sabe que fijar parametros dinamicos por referencia es lento y por eso existe el patron ARRAY; una
propagacion sobre muchos racks es **trabajo nuevo de la misma familia**. Y una transaccion unica sobre
N racks **agrava** la cuestion, porque mantiene abierto mas tiempo un estado mas grande. No se estima
aqui.

### D-14 — RACKEDITAR y RACKDUPLICAR

**RACKEDITAR.** El editor abre un rack que puede estar vinculado. Dos exigencias:

1. El campo muestra el **valor efectivo** y se ve **vinculado** (D-16), no editable como literal.
2. **Guardar no debe desvincular en silencio.** Es el riesgo real: la ventana lee cajas de texto y
   arma un diseno (`RackSelectiveWindow.xaml.cs:2256` escribe `VerticalClearance = clearance`). Si el
   camino de guardado ignora el binding, cada edicion del rack lo **desvincula sin pedirlo**.
   Desvincular debe ser una **accion explicita** (D-11), nunca un efecto de guardar.

**RACKDUPLICAR.** C-4: la copia **conserva el `VariableId`**. El contrato queda:

```
RackId          →  NUEVO en la copia independiente   (comportamiento actual, no cambia)
VariableId(s)   →  IDENTICOS                          (C-4)
```

C-4 es coherente sin esfuerzo porque **una copia se queda en el mismo dibujo**, donde la variable
existe. Pero hay un detalle del arbol que la convierte en un requisito **activo**, no automatico:

```csharp
// src/RackCad.Plugin/KindHandlers/SelectiveKindHandler.cs — RestampDesign
var store = new SelectivePalletDesignStore();
var design = store.Deserialize(designJson);
design.Id = newId;
design.Name = copyName;
return store.Serialize(design);
```

**El restamp del selectivo NO copia el JSON: lo re-serializa a traves del DTO.** Cualquier campo que
`SelectivePalletDesignDocument` no declare **se pierde en RACKDUPLICAR**. (Los kinds `dynamic`, `cama`
y `pushback` devuelven el JSON tal cual, asi que no comparten el problema; `cabecera` y `cantilever`
si.)

**Consecuencia que refuerza D-04 y D-07:** si la referencia viaja en el **diseno selectivo** (D-07 F2),
tiene que ser un **campo declarado** del DTO. Un campo no declarado sobreviviria a guardar y reabrir
pero **moriria en la primera duplicacion**, porque este restamp re-serializa a traves del DTO y el DTO
selectivo no preserva desconocidos.

> Precision heredada de V2, **actualizada**: este argumento era especifico de F2 y **F2 es ahora la
> opcion decidida** (D-07), asi que **aplica sin condiciones**. Bajo la rechazada F3 no habria hecho
> falta, porque `RestampEnvelope` re-serializa el mismo `RackEmbedDocument` y su `[JsonExtensionData]`
> si conserva lo desconocido — pero F3 cayo por semantica, no por esto. La decision de D-14 no cambia:
> el restamp **no debe tocar** las referencias, y eso **merece prueba de comportamiento propia**
> (D-17-bis), no un round-trip del Store.

**Detalle adicional del arbol**: RACKDUPLICAR clona **solo la definicion del bloque-vista que el
usuario pico**; las vistas hermanas del mismo rack no se copian. La propagacion (D-12) debe partir del
`RackId`, no de la vista, para no razonar sobre medio rack.

### D-15 — Guardar/exportar a la biblioteca un rack vinculado

Un `VariableId` **solo tiene sentido dentro de su dibujo**. La biblioteca
(`RackDesignLibrary`, ficheros `*.rackcad.json`) es **cruzada entre dibujos** por definicion, asi que
un diseno exportado con un vinculo llega a destino apuntando a nada.

| Alt | Al exportar | Tradeoff |
|---|---|---|
| **H1** | **Materializar**: la entrada de biblioteca guarda el **valor efectivo** como literal | La biblioteca sigue siendo portable y siempre valida. Se pierde la intencion «esto era una variable» |
| H2 | Conservar el vinculo y resolver o fallar al importar | Preserva la intencion; produce **importaciones que fallan** en cualquier dibujo que no tenga esa variable — la mayoria |
| H3 | Exportar el vinculo **y** una copia de la variable, fusionando al importar | Lo mas completo y lo mas caro: exige semantica de **fusion y conflicto** entre conjuntos de variables |

**DECIDIDA: H1 (materializar), y con el contrato completo que V3 no daba (C4-10).**

El repositorio ya trata asi la frontera de la biblioteca: una entrada **nunca** conserva la identidad
del rack — al abrirla se adopta identidad nula y se acuna un GUID nuevo al insertar. Si el `RackId` —un
identificador con alcance de dibujo— ya se descarta al cruzar, **un `VariableId` se descarta por la
misma razon**.

**El export es un ARTEFACTO NUEVO, no una copia del documento embebido:**

```
exportar a biblioteca(rack) :
    1. RESOLVER el efectivo                   (D-04-bis)
    2. MATERIALIZAR: el efectivo pasa a ser el literal de la entrada
    3. ELIMINAR PropertyValues                -> la entrada es LITERAL-ONLY
    4. ESCRIBIR SelectiveRack en la LINEA 1.x -> NO hereda el schema pegajoso del rack embebido
```

**El punto 4 es el que V3 no decia (M6).** Un rack embebido promovido esta en `2.x`; su entrada de
biblioteca **no**. Y no es una excepcion a la regla pegajosa: es que **son documentos distintos**. La
promocion pegajosa describe la vida de **un** documento a lo largo de sus reescrituras; el export
**crea otro**, sin `PropertyValues` y sin historia, y un documento literal-only pertenece a la linea
`1.x`. Mantenerlo en `2.x` haria que **cualquier** version anterior rechazara una plantilla que es
perfectamente compatible con ella.

**Invariante vinculante: ID22A NUNCA exporta un `SelectiveRack` promovido ni vinculado.** Si el
materializado no puede completarse —por ejemplo, con una referencia rota—, el export **falla de forma
visible**; no exporta el authored congelado haciendolo pasar por el valor bueno.

**El guard anidado que falta (H1).** El Architect verifico que `RackProjectDocument.SelectiveRack`
**no** pasa por ningun `SchemaGuard`, mientras sus cuatro payloads anidados hermanos si. Se anade:

```
SchemaGuard.CheckReadable(document.SelectiveRack?.SchemaVersion, <constante de lectura selectiva>,
                          "El diseño del selectivo")
```

**Con una advertencia explicita que no se puede omitir:** ese guard es codigo **nuevo**, luego
**protege a I-47 en adelante y NO a los binarios pre-I-47 ya compilados**. No se le atribuye una
garantia retroactiva — es el mismo error que V1 cometio con `[JsonExtensionData]` y que C2-5 corrigio.
Lo que protege la ruta de biblioteca frente a versiones anteriores es la **invariante** de arriba: que
nunca sale por ahi un documento promovido ni vinculado.

**H3 sigue siendo territorio de ID22B/futuro** (fusion de conjuntos de variables entre dibujos), y
**C2-8** ya la saco de ID22B: no pertenece a ninguna iniciativa definida.

## 7. Interfaz de usuario

> **Nota de alcance.** La fila de I-47 en ROADMAP —y el §3 del contrato— listan «UI» y «comando nuevo»
> como fuera de alcance. Describen lo que **no se construye**, y siguen siendo ciertas: esta seccion
> **disena en papel** y no toca `src/`. Si el dueno acepta D-16, el **contrato** debe reflejarlo antes
> de produccion (§14, CF-1) y la **fila de ROADMAP** se actualiza **al cerrar** (§14-bis), que es el
> unico momento que WORKFLOW seccion 2 permite.

### D-16a — Donde vive la ventana central

| Alt | Superficie | Tradeoff |
|---|---|---|
| **J1** | Entrada en `RackMainMenuWindow` + miembro nuevo en `MainMenuAction`, leido tras cerrar el modal | **Precedente exacto y reciente**: asi se resolvio la unica accion del menu que **no es un rack** |
| J2 | `[CommandMethod]` propio (p. ej. `RACKVARIABLES`) sin entrada de menu | Accesible pero **no descubrible** — el problema que I-36C existio para arreglar |
| J3 | Pestana dentro de cada editor de rack | Repetida N veces; el registro es **del dibujo**, no del rack |

**Recomendacion: J1 y J2 juntos** —entrada de menu **y** comando con alias, como tiene todo lo demas—.
El precedente literal:

```csharp
// src/RackCad.Plugin/RackMenuCommands.cs:36-40
if (menu.RequestedAction == MainMenuAction.GenerateStructuralSection)
{
    StructuralSectionCommandFlow.Run(document);
    return;
}
```

Y la regla de crecimiento del enum, que este caso **respeta** (un miembro mas, sin payload):

```csharp
// src/RackCad.UI/MainMenuAction.cs:17-18
/// This enum stays small on purpose. If it ever grows a family of variants with payloads, that is the
/// signal to give it its own typed hierarchy — not to widen the enum.
```

Dos consecuencias verificadas: la lista de botones del menu es **XAML fijo** (no hay registro de
entradas), y el comando nuevo debe anadirse a `RackCommandReference` —que ya arrastra el hueco H-04 del
Discovery—. Ademas, `WindowCensusGuardTests` obliga a **clasificar** cualquier ventana nueva en uno de
cuatro arquetipos; una ventana de variables encaja en `Utilities` o `ConfigurationDialogs`, y el
arquetipo elegido arrastra su contrato (`DialogWindowContractTests`).

### D-16-frontera — Como llegan los datos del dibujo a la UI (C4-12, cierra H5)

> **NUEVA en V4.** El Architect registro (H5) que **dos comportamientos ya decididos** necesitan datos
> con alcance de dibujo dentro de una ventana WPF que **estructuralmente no puede leer el dibujo**:
> D-10 F1 debe **listar los consumidores** que impiden un borrado, y D-16b debe mostrar **que variable**
> gobierna el campo. `RackCad.UI` no referencia AutoCAD (ADR-0006) y D-01 solo nombraba, del lado del
> Plugin, un helper de lectura/escritura del NOD.

**La regla, y no admite excepcion: la UI no conoce AutoCAD.**

```
Plugin  (unico que toca AutoCAD)
    lee el NOD                      -> ProjectVariablesDocument
    lee RackBlockFinder.ScanEnvelopes -> envelopes + authored documents
    resuelve con D-04-bis            -> efectivos
    |
    |  entrega DTOs PUROS (Application), sin ObjectId, sin Transaction, sin Database
    v
UI  (WPF)
    pinta variables, chips, listas y avisos
    |
    |  devuelve PETICIONES / INTENCIONES, no acciones
    v
Plugin + Application  ejecutan la operacion confirmada (C4-11)
```

**Los cinco DTO que cruzan la frontera**, todos puros y todos testeables en la suite Core:

| DTO | Contenido |
|---|---|
| **Variables** | la lista del registro: `VariableId`, `Name`, `Type`, `Definition` |
| **Estado de binding** | por propiedad: vinculada o no, y a que `VariableId` |
| **Valor efectivo** | el numero que gobierna hoy, ya resuelto (D-04-bis) |
| **Resumenes de consumidores** | que racks usan cada variable — lo que D-10 F1 debe listar |
| **Estado roto** | que bindings no resuelven — lo que D-08-bis debe ofrecer reparar |

**La UI devuelve intenciones, no efectos.** Un clic en «Desvincular» produce una **peticion**
`Unlink(rack, propertyId)`; no muta nada por su cuenta. Es lo que permite que las ocho operaciones
confirmadas (C4-11) tengan **un solo** camino de ejecucion, con su preflight y su commit, sea cual sea
la superficie que las dispare.

> Ventaja lateral que conviene declarar: como el DTO es puro y la UI solo pinta, **el comportamiento de
> la ventana de variables y del chip se prueba entero en la suite UI sin AutoCAD** (D-17), igual que ya
> se prueban los editores existentes.

### D-16b — Estado visual del vinculo: el problema real

**No existe ningun control de «heredado / sobrescrito» en el repositorio.** Lo que hay son cuatro
soluciones ad-hoc, y una de ellas es un aviso serio:

| Patron | Donde | Que ensena |
|---|---|---|
| Solo prosa | Selectivo: etiqueta «Manual (opcional · vacio = auto)» + tooltip | **Auto y manual se ven identicos.** Es el listón que hay que superar, no el ejemplo a copiar |
| Asterisco en la tarjeta | Push Back, `PushBackMatrixCardModel`: *«distinguir "hereda 4" de "pidio 4"»* | Marca minima y barata que **si** funciona |
| Estado «mixto» | Push Back compuesto: `field.SetNumber(null)` + `IsOptional = true` + **tabla lateral** `HashSet<NumericField>` | El campo **no puede** expresar el estado por si mismo |
| Boton explicito | Push Back «Restaurar fondo» + combo de alcance | Deshacer un override es **accion nombrada**, no un borrado implicito |

El tercero es la advertencia central. Push Back necesito una estructura paralela porque un `NumericField`
vacio ya significa dos cosas —«mixto» y «sin override»—. **Un estado «vinculado» seria un tercer
significado del mismo hueco**, y repetiria el mismo apano.

Ademas hay una colision tecnica concreta, verificada: `NumericField` **es dueno de su `BorderBrush`** y
guarda/restaura la procedencia del consumidor (valor local **o** binding) al entrar y salir del estado
de error. Un adorno de «vinculado» que pinte el borde **choca** con esa maquinaria, que asume ser la
unica escritora.

| Alt | Como se muestra el vinculo | Tradeoff |
|---|---|---|
| K1 | Caja vacia + tooltip | Repite el error del Selectivo: **invisible** |
| K2 | Pintar el borde del campo | **Colisiona** con `ApplyErrorBorder` |
| **K3** | **Adorno adyacente**: chip/etiqueta con el nombre de la variable junto al campo, y el campo en **solo lectura** mostrando el valor efectivo, mas un boton **«Desvincular»** explicito | No toca `BorderBrush`; el valor efectivo es visible; el estado es **inequivoco** (hay chip o no lo hay); y el desvinculado es accion nombrada, como «Restaurar fondo» |

**Recomendacion: K3.** Nace de las dos lecciones que el arbol ya pago: el estado tiene que **verse**
(leccion del asterisco de Push Back) y no puede vivir en la ausencia de valor (leccion del estado
mixto).

**Detalle util para el slice**: `ClearanceBox` es hoy un **`TextBox` plano**, no un `NumericField`, asi
que la colision K2 **no aplica todavia** a este campo concreto — pero aplicaria a cualquier campo que
ID22B vincule, y por eso la recomendacion no depende de ese accidente.

---

## 8. Verificacion

### D-17 — Que se prueba automaticamente y que exige AutoCAD

El reparto lo dicta un hecho duro del repositorio: **ningun proyecto de pruebas referencia
`RackCad.Plugin`** (`RackCad.Tests` → Domain + Application; `RackCad.UI.Tests` → Domain + Application +
UI). No hay forma de ejecutar un `[CommandMethod]` en CI, y no la va a haber: el Plugin referencia
AutoCAD ([ADR-0003](../adr/0003-referencias-autocad-para-ci.md)) y la CI no tiene AutoCAD.

> **Corregido en V4 (M2, M3).** V3 no asignaba **ninguna** verificacion a D-12/D-13 —la propagacion que
> C-3 existe para gobernar— y de D-08 solo verificaba **el mensaje**, no el **aborto**. Ambas quedan
> asignadas.

**A la suite Core (pura, sin AutoCAD):**

| # | Que se verifica | Decision |
|---|---|---|
| 1 | `ProjectVariablesDocument`: schema, store, guard, `ExtensionData`, y los **tres errores duros** | D-01-bis |
| 2 | **Resolver efectivo**: con binding gana la variable; sin binding gana el authored | D-04-bis |
| 3 | **`Link`**: authored congelado, `PropertyValues` anadido, schema promovido, efectivo resuelto | D-11-bis |
| 4 | **`Unlink`**: el efectivo se escribe al literal, el binding desaparece, el schema **no baja** | D-11 |
| 5 | **`RepairBroken`**: usa el authored, quita el binding, conserva `2.x`, y **no** es alcanzable sin accion explicita | D-08-bis |
| 6 | **Probe y clasificacion de consumidores**: confirmado / ajeno / indistinguible | D-12 |
| 7 | **Preflight y aborto**: con un consumidor irresoluble, la operacion **no produce ninguna mutacion** | D-12, D-13 |
| 8 | **`PropertyId` desconocido** ⇒ error visible, nunca caida al literal | §1-bis, C4-14 |
| 9 | **`kind` de referencia desconocido** ⇒ error visible | D-04 |
| 10 | **Monotonicidad del sticky**: las cuatro ramas, incluida la de **ERROR** por major superior | D-07, C4-9 |
| 11 | **Preservacion del estado authored** en `load → edit → save`: version, `PropertyValues`, literal y `ExtensionData` | D-07 portador, C4-2 |
| 12 | **Materializacion y schema de la biblioteca**: literal-only, sin `PropertyValues`, en linea `1.x` | D-15 |
| 13 | **El restamp conserva el binding** y cambia `RackId`/`Name` | D-17-bis, C-4 |
| 14 | **Precedencia de `ClearOverride`** sobre el efectivo, sin cambios | D-06 |
| **15** | **Vistas hermanas con la MISMA snapshot authored ⇒ el preflight PASA** y produce **una sola** autoridad logica | D-12 §multi-vista (V4-01) |
| **16** | **Todas POSITIVE + authored divergente ⇒ ABORTA**, y el error **nombra el `RackId`**. Se cubren las tres formas de divergencia: literal authored, `PropertyValues` y `SchemaVersion` | D-12 Familia A (V4-01, N1) |
| **17** | **Con abort, el `MutationPlan` queda VACIO**: ni registro, ni el rack divergente, ni **ningun otro consumidor**. Se comprueba sobre `VariableMutationPreflightResult`, que es **puro** — sin dibujo | D-13 §preflight, C4-11 (N3) |
| **18** | **Target-variable: rack DIVERGENTE pero con TODAS las vistas `NEGATIVE` ⇒ se IGNORA** y la operacion **continua** con normalidad. Es la prueba que fija N1: la divergencia ajena no bloquea | D-12 Familia A (C4.2-2) |
| **19** | **Mezcla `POSITIVE`/`NEGATIVE` entre hermanas ⇒ ABORTA**: las vistas discrepan sobre el binding objetivo | D-12 Familia A (C4.2-2) |
| **20** | **`Link` sobre un rack hoy `NEGATIVE` en todas sus vistas ⇒ SI entra al preflight multi-vista completo** y no se ignora. Es la prueba que justifica la Familia B | D-12 Familia B (C4.2-3) |
| **21** | **Payload RackCad indescifrable en un candidato relevante ⇒ ABORTA** (`INDETERMINATE`), tanto en Familia A como en Familia B | D-12 (C4-13, C4.2-2/3) |
| **22** | **BOM: hermanas con authored IGUAL ⇒ UNA autoridad BOM**, y el efectivo se resuelve contra el snapshot unico del comando | D-04-bis §d2 (N5, C4.3-1) |
| **23** | **BOM: hermanas con authored DIVERGENTE ⇒ NO hay autoridad / NO hay entrada de BOM** para ese rack. Se comprueba que **no** se produce un BOM a partir de ninguna hermana | D-04-bis §d2 (N5) |
| **24** | **BOM: una hermana INDESCIFRABLE ⇒ NUNCA se selecciona otra como fallback.** El rack queda sin autoridad BOM; se comprueba explicitamente que el resultado **no** procede de la hermana legible | D-04-bis §d2 (N5) |

Los puntos 7 y 11 son los que V3 no tenia, y son **precisamente** los que cierran B2 y M2: ambos son
**puros** y por tanto plenamente verificables — el preflight opera sobre documentos, no sobre el
dibujo, y el portador es un DTO.

Los puntos **15-17** nacieron en V4.1 y cierran **V4-01**; el **17** es el que de verdad importa, y en
V4.2 ya se apoya en un artefacto **definido** (`VariableMutationPreflightResult`, D-13): no basta con
que el preflight devuelva error, hay que comprobar que **no queda nada planificado**.

Los puntos **22-24** son los de V4.3 y cierran **N5**. El **24** es el que de verdad fija la regla: no
basta con comprobar que el resultado es «sin BOM», hay que comprobar que **no salio de la hermana que
si se podia leer** — que es exactamente el atajo que el arbol tomaria hoy. Los tres son puros: operan
sobre documentos authored y un `ProjectVariablesDocument`, sin dibujo.

Los puntos **18-21** son los de V4.2 y cierran **N1**. El **18** y el **20** son los dos que fijan la
correccion, y conviene leerlos juntos porque son simetricos: **18** prueba que un rack divergente pero
**ajeno** NO bloquea (Familia A), y **20** prueba que un rack **no vinculado** —`NEGATIVE` en todas sus
vistas— **si** se inspecciona cuando el usuario lo elige (Familia B). Sin el 20, `Link` no podria
vincular nada; sin el 18, un dibujo heredado quedaria bloqueado para siempre.

**A la suite UI (WPF, sin AutoCAD, sobre los DTO de D-16-frontera):**

| Que se verifica | Decision |
|---|---|
| La ventana de variables: alta, renombrado, borrado bloqueado con lista de consumidores | D-16a, D-10 |
| El estado visual del vinculo: chip presente, campo de solo lectura, boton «Desvincular» | D-16b |
| La entrada de menu y `MainMenuAction` | D-16a |

**Al dueno, en AutoCAD (Owner Validation, CF-5'):**

| Que se verifica |
|---|
| **Lote transaccional sobre frontal + planta + lateral**, con **un** commit |
| **Fallo inducido ⇒ CERO commits parciales**: ningun consumidor queda modificado |
| **Definicion de bloque ausente ⇒ CERO mutacion**, y el aborto nombra la que falta |
| **`save` / `close` / `reopen`** del mismo DWG conserva registro y bindings |
| El registro del **NOD sobrevive a `PURGE`** |

**Consecuencia de diseno, no de pruebas**: cuanto mas codigo viva en Application, mas cae del lado
verificable. Es la misma razon por la que D-01 deja en el Plugin solo el acceso al diccionario.

**Aviso**: `SelectiveWindowTestSupport` es **el unico sitio autorizado** para construir
`RackSelectiveWindow` (lo impone `SelectiveWindowConstructionGuardTests`). Cualquier prueba de UI del
slice pasa por ahi.

### D-17-bis — Verificar C-4 de verdad: el restamp de RACKDUPLICAR

> ### ⚠ Correccion de V2 (C2-7)
>
> V1 proponia cubrir «la supervivencia del vinculo al restamp» con **un round-trip de
> `SelectivePalletDesignStore`**. Eso **no prueba nada del restamp**: solo prueba que el serializador
> serializa. La transformacion que C-4 exige —cambiar `RackId` y `Name`, **conservar** las
> referencias— vive en `SelectiveKindHandler.RestampDesign`, en el **Plugin**, que **ninguna suite
> puede cargar**. V1 confundia «el DTO declara el campo» con «la transformacion lo preserva».

**Lo que hay que verificar por COMPORTAMIENTO**, no por serializacion:

```
dado   un diseno selectivo con RackId=A, Name="Rack 1" y PropertyValues={ selective.verticalClearance → V }
cuando se aplica el restamp de una copia independiente con newId=B, copyName="Rack 1 (1)"
entonces  RackId  == B        (cambia)
          Name    == "Rack 1 (1)"  (cambia)
          PropertyValues  ==  { selective.verticalClearance → V }   (SE CONSERVA, mismo VariableId)
```

| Alt | Como se verifica | Tradeoff |
|---|---|---|
| L1 | Round-trip del Store (lo que decia V1) | **No prueba el restamp.** Descartada |
| L2 | Guarda de texto sobre `SelectiveKindHandler.cs` | Detecta que el codigo *menciona* algo; **no** que el comportamiento sea correcto. Insuficiente sola |
| **L3** | **Extraer la transformacion a Application** como funcion pura y probarla por comportamiento; el handler del Plugin queda como delegacion de una linea; **mas** una guarda de texto que impida que alguien la reimplemente en el Plugin | Es la unica que verifica **la transformacion**. Coste: un tipo nuevo en Application y tocar el handler |

**Recomendacion: L3.** Y no es un patron nuevo: el repositorio **ya movio a Application** la logica de
despacho por kind y sus mensajes —`KindDispatch` / `KindDispatchMessages` viven en
`RackCad.Application/Persistence/` y el Plugin solo los consume—. Aplicar lo mismo al restamp del
diseno es continuar una decision ya tomada, no inventar una frontera.

Forma sugerida (no se implementa):

```
RackCad.Application/Persistence/   SelectiveDesignRestamp.Restamp(json, newId, copyName) → json
                                   ← PURO. Aqui viven la transformacion y su prueba de comportamiento.

RackCad.Plugin/KindHandlers/       SelectiveKindHandler.RestampDesign  →  delega, una linea
                                   ← cubierto por guarda de texto: que delegue y no reimplemente
```

**Por que la guarda de texto sigue haciendo falta**: extraer la funcion no impide que alguien vuelva a
escribir la transformacion dentro del Plugin. La guarda —del mismo tipo que
`MainMenuStructuralSectionAccessGuardTests`— fija que el handler **delega**, que es lo unico que la
suite puede afirmar sobre codigo que no puede ejecutar.

**Alcance de la correccion**: esto **no reabre D-14**, cuya decision (el vinculo debe ser campo
declarado y el restamp no debe tocarlo) sigue intacta. Cambia **como se verifica**, no que se decide.

### D-18 — Supervivencia minima exigida

**Guardar, cerrar y reabrir el mismo DWG**, con el registro y los vinculos intactos y el dibujo
identico. Es el minimo del dueno y **no es automatizable**: exige AutoCAD, y por tanto al dueno
(WORKFLOW §6). **WBLOCK y copia entre dibujos quedan fuera**, diferidos.

---

## 9. ADR

### D-19 — Hace falta un ADR, y uno solo

Los criterios de [`docs/adr/README.md`](../adr/README.md) se cumplen sobradamente: se funda una
**autoridad de persistencia nueva** en el DWG, se fija una **identidad** nueva y se cambia el
**contrato de una propiedad** de diseno. Ademas, WORKFLOW §8 exige que el ADR se escriba **antes** de
implementar.

**Recomendacion: un unico ADR** que cubra autoridad (D-01), identidad y tipos (D-02, D-03) y forma del
vinculo con su precedencia (D-04, D-06). Partirlo en tres produciria tres documentos que solo se
entienden juntos.

Nace en estado **`propuesto`**. **Solo el dueno lo acepta**: es la regla del indice de ADR y la
practica del repositorio (I-45 cerro con ADR-0033 aun `propuesto`).

---

## 10. Evolucion a ID22B e ID21

> **Reescrita en V2.** V1 declaraba no saber que eran. El dueno los definio en el Gate C2 (C2-1, C2-2),
> asi que esta seccion pasa de «propiedades genericas que no bloquean nada» a **que extiende cada una y
> que le deja preparado ID22A**. **Ninguna de las dos se disena ni se implementa aqui.**

```
ID22B — formulas/expresiones entre variables      →  extiende  ProjectVariable.Definition
ID21  — referencias a propiedades de racks        →  extiende  PropertyValue<T>
        (RackId + PropertyId)
```

Extienden **tipos distintos** (§1-bis). Esa es la propiedad que hace que puedan avanzar en cualquier
orden, o a la vez, sin que una obligue a tocar lo de la otra.

### Lo que ID22A deja preparado para **ID22B** (formulas)

| Preparado | Por que ID22B lo necesita | Gracias a |
|---|---|---|
| `Definition` **discriminada** desde el primer dia, con un solo caso (`Literal`) | `Expression` entra como **caso adicional**, aditivo, sin subir el major | D-03 |
| `Type` explicito y separado de `Definition` | Una expresion necesita **operandos tipados** y coherencia de unidades; sin tipo no hay evaluacion correcta | D-03 |
| `VariableId` estable e independiente del nombre | Una formula referencia **variables por id**; si el id derivara del nombre, renombrar romperia formulas | D-02 |
| `PropertyValue<T>` **no** menciona como se calcula la variable | Una propiedad apunta a un `VariableId` y le da igual si es literal o expresion: **ID22B no toca las propiedades** | §1-bis |

**Lo que ID22A NO resuelve y ID22B tendra que resolver entero**: el grafo de dependencias entre
variables, la deteccion de ciclos, el orden de evaluacion y que ocurre al borrar una variable **usada
por una formula** (D-10 hoy solo cuenta consumidores **propiedad→variable**, no **variable→variable**).

### Lo que ID22A deja preparado para **ID21** (refs a propiedades de racks)

| Preparado | Por que ID21 lo necesita | Gracias a |
|---|---|---|
| Referencia persistida **tipada** con `kind` | `rackProperty` entra como **tercer `kind`**, sin cambiar el tipo del valor ni romper el esquema | D-04 |
| **`PropertyId` estable** como concepto de primera clase | Es **la mitad del par** `(RackId, PropertyId)`. Introducirlo despues obligaria a reescribir lo ya persistido | §1-bis |
| `RackId` ya existe y ya es estable | Es `RackEmbedDocument.Id`, el GUID que sobrevive a ediciones | Discovery §2.2 |
| Un `kind` desconocido es **error visible** | Un dibujo tocado por versiones distintas no degrada en silencio | D-08 |
| Descubrimiento agrupado por `RackId` | ID21 necesita saber **que racks dependen de que rack** | D-12 |

**Lo que ID22A NO resuelve y ID21 tendra que resolver entero**: la resolucion rack→rack, sus ciclos, el
orden de redibujo cuando un rack depende de otro que a su vez cambia, y **que pasa al exportar a la
biblioteca un rack con referencias a otro rack** — D-15/H1 hoy materializa referencias a **variables**,
y una referencia a **otro rack** plantea el mismo problema con una respuesta que puede no ser la misma.

### Un limite comun que conviene ver ahora

Hoy la propagacion (D-13) es de **profundidad 1**: variable → consumidores. **ID22B anade
variable→variable** y **ID21 anade rack→rack**, y ambas convierten el preflight en un recorrido de
**grafo**, no de lista. El contrato de D-13 **no debe asumir profundidad 1** en su forma persistida ni
en su modelo de errores. No se disena aqui; se senala para que la implementacion de ID22A no cierre esa
puerta por descuido.

**Lo que ID22A deja deliberadamente sin resolver, y no es deuda sino alcance**: unificar `ClearHeight`
(C-5), WBLOCK y copia entre dibujos (diferido), formulas (ID22B), referencias rack→rack (ID21), y la
**transferencia/fusion de conjuntos de variables entre dibujos** — que, corregido en V2, **no pertenece
a ID22B ni a ninguna iniciativa definida** (D-15).

---

## 11. Alternativas descartadas — resumen

| Descartada | Por que |
|---|---|
| A2 — Model Space BTR | Unicidad por convencion, no estructural; y seria elegir por coste (§1) |
| A3 — Bloque dedicado | `ScanEnvelopes` lo leeria como rack; el usuario puede purgarlo o explotarlo |
| A4 — `SummaryInfo` | Editable por el usuario desde AutoCAD; sin estructura |
| A5 — Sidecar | No viaja dentro del DWG |
| B2 — Id derivado del nombre | Renombrar rompe referencias: contradice el requisito |
| B3 — Entero incremental | Colisiona al juntar conjuntos; no gana legibilidad |
| C3 / C5 — Sin tipo / sin `Definition` discriminada | Cada una bloquea una continuacion ya definida: variables no numericas, y las formulas de ID22B |
| D2 — Sustituir el escalar por objeto | Exigiria migracion: **prohibida por C-1** |
| **D3 — `Bindings: slot → string`** (recomendada en V1) | **Retirada en V2**: el valor es una cadena desnuda, asi que el par `(RackId, PropertyId)` de ID21 no cabe sin cambiar el tipo del valor |
| F1 / E3 — Referencia en el diseno con major sin cambiar | Perdida **silenciosa** en el round-trip de una version pre-I-47, que `[JsonExtensionData]` **no** puede evitar |
| **F3 — Referencia en el sobre** (viva en V2) | **Rechazada en V3**: una version vieja la **preserva** pero **no la entiende**; editaria y redibujaria con el literal, dejando geometria **incoherente con la autoridad**. Preservar bytes no es compatibilidad |
| **Major «segun contenido actual»** (mecanica de F2 en V2) | Hace **oscilar** el schema al vincular y desvincular: que versiones abren el rack dependeria de estado transitorio. Sustituida por **promocion pegajosa** |
| `EnsureForPlan` dentro del preflight/lote (V2) | **Muta el `Database` fuera de la transaccion del llamador** y con biblioteca ausente devuelve 0 en silencio. Sustituido por **verificar y abortar** |
| L1 — Round-trip del Store como prueba del restamp | **No prueba el restamp**: solo prueba el serializador |
| «Variable pisa `ClearOverride`» | Invierte una precedencia probada; ninguna premisa lo pide |
| A (D-08) — Fallback al literal | Cambia la geometria en silencio |
| F2 / F3 **de D-10** (etiquetas locales, no las de D-07) | Traslada el fallo al futuro / reescribe N racks como efecto colateral de un borrado |
| G2 / G3 (D-11) | Salto geometrico visible al desvincular |
| Propagacion perezosa | Contradice C-3 y permite estados intermedios inconsistentes |
| H2 (D-15) | Importaciones que fallan en casi cualquier dibujo destino |

---

## 12. Preguntas realmente abiertas

### 12.1 Estado tras la reconciliacion

**V3 afirmaba que no quedaba ninguna decision de producto abierta. Era falso**, y el Architect lo
registro como su desacuerdo material principal: B1 (vincular) era una decision de producto sin tomar y
B4 (punto de resolucion) una decision de arquitectura sin tomar. **V4 las toma, con las premisas
C4-1..C4-14 del dueno.**

| Antes abierto | Estado en V4 |
|---|---|
| La accion `Link` | **DECIDIDA** — D-11-bis (C4-1) |
| El punto de resolucion | **DECIDIDO** — D-04-bis (C4-4) |
| Semantica del literal authored | **DECIDIDA** — D-04 (C4-5) |
| Reparacion de referencia rota | **DECIDIDA** — D-08-bis (C4-7) |
| Contrato del `ProjectVariablesDocument` | **DECIDIDO** — D-01-bis (C4-8) |
| Portador del estado authored | **DECIDIDO** — D-07 (C4-2) |
| Frontera transaccional completa | **DECIDIDA** — D-13 (C4-3) |
| Unidad de atomicidad del registro | **DECIDIDA** — D-13 (C4-11) |
| Frontera Plugin → UI | **DECIDIDA** — D-16-frontera (C4-12) |
| Descubrimiento de consumidores | **DECIDIDO** — D-12 (C4-13) |
| Politica de `PropertyId` | **DECIDIDA** — §1-bis (C4-14) |
| Schema de la biblioteca | **DECIDIDO** — D-15 (C4-10) |

### 12.2 Lo que sigue abierto

1. **La aceptacion del contrato por sus dos revisores.** `Coordinator` y `Architect` deben coincidir
   sobre **la misma version**, y **V4.3 no declara ninguno de los dos**. El estado real, sin adornos:
   `Architect=DISAGREED` sobre **V3** y sobre **V4.1**; el `Coordinator` **retiro** su AGREED en ambas
   ocasiones y **no lo ha declarado sobre V4.2**; el `Architect` **no llego a revisar V4.2**, porque
   V4.3 la sustituye antes. **La version a revisar ahora es V4.3.**
2. **El coste de la propagacion** sigue sin medir (riesgo P6). No es una decision de arquitectura.
**Ninguna de las dos bloquea la revision del Architect sobre V4.3.**

> **Corregido en V4.1 (punto 7).** V4 listaba aqui una tercera pregunta: «a que iniciativa pertenece la
> fusion de conjuntos de variables entre dibujos». **No es una pregunta abierta**: **C3-8 ya retiro esa
> necesidad**. H3 es un **futuro separado, fuera de alcance y sin numero**, y no necesita ninguno para
> que ID22A cierre. Presentarlo como pendiente sugeria un trabajo de asignacion que nadie debe.

## 13. Riesgos de esta propuesta

| # | Riesgo | Mitigacion incorporada |
|---|---|---|
| P1 | **El acceso al NOD no lo cubre ninguna suite** y no lo cubrira | D-01 deja en el Plugin solo el acceso; DTO, store, resolver, mensajes y materializacion viven en Application y **si** se prueban (D-17) |
| P2 | **`PURGE` sobre la entrada del NOD**: no verificado | Owner Validation (CF-5'). **No se afirma en ningun sentido** |
| P3 | **Desvinculado silencioso al guardar** desde RACKEDITAR | C4-5 corta el camino: con binding activo el guardado **no** escribe el literal |
| P4 | **Perdida silenciosa del vinculo** por round-trip antiguo o por el restamp | Promocion pegajosa + **portador authored** (C4-2) + D-17-bis |
| P5 | **Un tercer significado del campo vacio** en la UI | D-16b: adorno adyacente, no ausencia de valor |
| P6 | **Coste de propagacion desconocido** | Riesgo y **metrica de implementacion** (C3-8). Una transaccion unica sobre N racks **agrava** el perfil: mantiene mas estado abierto mas tiempo |
| P7 | **Archivos calientes**: el slice toca `SelectivePalletDesign.cs` y `RackSelectiveWindow.xaml.cs` | Serializar con cualquier otra iniciativa del Selectivo (WORKFLOW seccion 7) |
| P8 | **Sobre-ingenieria** | Apuesta consciente por la evolucion limpia a ID22B/ID21. La revision adversarial **no** sostuvo ningun hallazgo de sobre-ingenieria |
| P9 | La transaccion unica **hay que verificarla ejecutando** | Gate de implementacion / Owner Validation (CF-5') |
| P10 | **Profundidad 1 asumida por descuido** | §10 lo senala; ni la forma persistida ni el modelo de errores deben cerrar esa puerta |
| **P11** | **El alcance real siguio creciendo en V4.1, V4.2 y V4.3**: dos writers, un portador nuevo, un resolver nuevo con **tres costuras** (una de ellas una interfaz de seis handlers), un documento nuevo con store y guard, el comparador multi-vista con probe tri-estado **reutilizado tambien por el BOM**, el artefacto de preflight **partido en dos mitades**, y ocho operaciones confirmadas. **Cada ronda de revision ha encontrado algo**, y eso es informacion sobre el estado del contrato, no solo sobre las rondas | Todo ello esta **declarado** en CF-3 como trabajo reconocido, no dado por hecho. Es la consecuencia honesta de cerrar los BLOCKER, no un descubrimiento posterior |
| **P12** | **`SelectivePalletDesignDocument` es un DTO muy tocado**: el portador cambia como se guarda **todo** el selectivo, no solo la holgura | El cambio es de **fontaneria de guardado**, no de forma del documento; los golden y el round-trip existentes son la red. Se prueba en Core (D-17 punto 11) |

## 14. Condiciones de Consensus Freeze

| # | Condicion |
|---|---|
| **CF-1** | **El contrato de iniciativa refleja el alcance final antes de produccion.** Hoy [`I-47-project-variables-foundation.md`](I-47-project-variables-foundation.md) §3 excluye UI y comando, redactado para DISCOVERY; D-16 los incluye |
| **CF-2** | **El ADR existe, escrito antes de implementar** (WORKFLOW seccion 8), en estado `propuesto`. **Solo el dueno lo acepta.** **V4 no lo escribe todavia**, por instruccion expresa |
| **CF-3** | **Los prerequisitos estan reconocidos como trabajo.** La lista, **ampliada en V4**: (a) el **portador del estado authored** — el guardado del selectivo deja de reconstruirse con `From(...)` (C4-2, B2); (b) el helper **sticky monotonico** con su rama de ERROR, sus dos lineas de version y su constante de **lectura `2.x`** (C4-9); (c) el **writer de lote transaction-aware sobre los DOS writers**, incluido `LateralHeaderDrawService`, con verificacion read-only de definiciones (C4-3, B3); (d) el **`SelectiveEffectiveDesignResolver`** y el desplazamiento de dibujo/BOM/preview a consumirlo (C4-4, B4), **con sus tres costuras nombradas (N2)**: **d1** `IRackKindHandler.BuildBom` gana un tercer argumento `ProjectVariablesDocument` —los cinco kinds no-Selectivo lo ignoran—; **d2** `RACKBOMTOTAL` lee el NOD **una vez** y usa **el mismo snapshot** en todo el BOM, **y deja de cotizar desde una sibling arbitraria**: para cada `RackId` Selectivo colocado reune todas sus hermanas, exige **igualdad authored completa** con el comparador de V4-01 y, si divergen o alguna es indescifrable, **omite ese rack con aviso visible** por la politica existente en vez de elegir una (N5, C4.3-1); **d3** el **split authored/effective** en `RackSelectiveWindow.BuildDesign`/`BuildSystem`, de modo que lo que se persiste y lo que se dibuja dejen de ser el mismo objeto en un rack vinculado. **`RACKLISTA` NO entra**, y `OutputBlockedReason` **no** cambia; (e) el **`ProjectVariablesDocument`** con store y guard propios (C4-8); (f) el **`SchemaGuard` anidado** para `RackProjectDocument.SelectiveRack` (C4-10); (g) la **extraccion del restamp** a Application (D-17-bis); **(h)** la **autoridad authored multi-vista** con su comparador puro y **su probe tri-estado por vista** (Familia A / Familia B), con el portador de (a) siendo **UNO por `RackId`** —no uno por vista— para no destruir el restampado que hace de `RACKEDITAR` una reconciliacion (V4-01, N1); **(i)** el artefacto **`VariableMutationPreflightResult` / `MutationPlan`**, puro y sin tipos de AutoCAD (N3), **partido en sus dos mitades**: el preflight **semantico** en Application y el **fisico** en el Plugin, con el fisico corriendo solo tras un `Success` del semantico y todavia antes de mutar (N6, C4.3-2) |
| **CF-4** | **Ambos revisores AGREED sobre la MISMA version.** Estado real tras V4.3: `Architect=DISAGREED` sobre **V3** (`4470c6b`) y sobre **V4.1** (`9301f0f`); el `Coordinator` **retiro** su AGREED en ambas ocasiones y **NO lo ha declarado sobre V4.2**; el `Architect` **no ha revisado V4.2**; y **V4.3 no tiene AGREED de nadie** |

**CF-5' — donde vive la validacion en AutoCAD.** No es condicion de Freeze; es gate de implementacion
y Owner Validation, con el alcance de la tabla de D-17.

## 14-bis. Checklist de cierre (no es Freeze)

En la **sesion de integracion**, como ultimo commit de la rama (WORKFLOW seccion 4.5.4, momento 3 de su
seccion 2):

- **`docs/ROADMAP.md`**: actualizar la fila de I-47 —hoy lista «UI, comando nuevo» entre lo fuera de
  alcance— y marcar el cierre. **Unico momento legitimo para tocarla.**
- `docs/HANDOFF.md` §8-12.
