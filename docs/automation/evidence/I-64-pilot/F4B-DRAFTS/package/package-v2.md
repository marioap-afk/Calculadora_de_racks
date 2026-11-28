# I-64 F4B — Push Back completo: paquete de la fase (v2)

> **Redaccion para el repo publico:** las rutas locales se sustituyen por marcadores (`<carpeta-del-paquete>`, `<worktree>`, `<carpeta-del-DLL-de-C2>`, `<datos-locales-del-usuario>`). El resto no cambia.

**Estado:** v2. Incorpora la disposición del Owner y del Coordinator «I-64 — F4B PUSH BACK COMPLETO / OWNER / COORDINATOR DISPOSITION» (2026-11-17), transcrita literal en `owner-f4b-disposition.md`. En este paquete se cita como **[OD]**. Pendiente de la revisión adversarial independiente que pide [OD].

**Base:** rama `architecture/workspace-persistente-rackcad` en `5ccaa770`; `main` en `812baf87`. Todos los hechos se miden ahí, en solo lectura.

**Cambios respecto a la v1:**
- §3 recoge las diez decisiones con el texto literal del Owner;
- §4 fija los dos modos de la ventana;
- §5 rehace la descomposición: PB-TC queda separada de PB-T1;
- §6 define PB-T0 al detalle;
- §7 recoge las reglas de proceso;
- la aclaración nueva es IC-11, porque IC-08 a IC-10 ya existen.

**Mapas de hechos** (se custodian con el paquete):
- `map-a-classic-write-path.md`: qué escribe el camino clásico al volver la ventana;
- `map-b-editor-state-testability.md`: dónde vive el estado editable y qué se puede caracterizar.

«MA §n» y «MB §n» remiten a sus apartados.

## 0. Objetivo y estado de las fases

**Objetivo de F4B:** que todo lo que hoy se edita de un rack Push Back con RACKEDITAR pueda editarse desde el Workspace como borrador, sin tocar el dibujo hasta el «Actualizar» del panel:
- todo o nada;
- un solo paso de Deshacer;
- conflicto antes de escribir;
- veto al cerrar.

Entran las ediciones estructurales y los racks A/B. RACKEDITAR sigue como superficie de compatibilidad.

**Lo que fija [OD]:**
- «F4B extends the proven Workspace architecture; it does not reopen F4»;
- «F5-A waits until F4B closes»;
- el orden es «F4B -> F5-A», con la razón: «the complete Push Back editor will establish the reusable full-editor migration pattern before Selectivo/Dinámico/Cantilever are connected».

| Fase o tarea | Estado ([OD]) |
|---|---|
| F4 | CLOSED / PASS WITH LIMITATIONS |
| F4B | AUTHORIZED |
| PB-T0 | CONDITIONALLY AUTHORIZED AFTER v2 REVIEW |
| PB-T1, PB-T2, PB-T3, PB-T4 y PB-T5 | WAITING |
| F5-A | AUTHORIZED / WAITING FOR F4B |
| F6 y F7 | PENDING |

PB-TC (§5) es una tarea que este paquete separa de PB-T1 por orden de D-PB-06. Su estado es **WAITING**, como las demás.

## 1. Autoridades vinculantes

| Autoridad | Qué fija |
|---|---|
| **[OD]** | Manda sobre todo lo demás de este paquete. Es la decisión de F4B, de D-PB-01 a D-PB-10 y de las tareas |
| Proposal V5 D-15 (`docs/initiatives/I-64-proposal-v5.md:557-582`) | El contrato de adaptador por sistema: (1) superficie alojable; (2) intención separada del cierre; (3) ida y vuelta del estado sin la ventana; (4) diálogos con el dueño que decide el host; (5) escritor caller-owned. Caracterización previa. «No se reescribe lógica de dominio» |
| D-16 (`:586-593`) e INV-19 | Sin cambio observable en RACKEDITAR ni en los comandos clásicos. Si la extracción cambiara algo observable: M-03 y **STOP**. [OD] admite solo las correcciones autorizadas expresamente (D-PB-06 y D-PB-08) |
| D-08, D-10 y D-13 (`:393-397`, `:410-438`, `:500-546`) | K = 1 superficie viva; sin modelo paralelo; los gestos de campo se comparten con la ventana clásica; Actualizar todo o nada y un solo paso de Deshacer |
| INV-09, 10, 15, 18, 19, 25, 28 y 29 (`:747-767`) | Los criterios por sistema |
| §11 (`:783`, `:803-805`, `:812`) | «no hay micro-gates»; A64-PV1-21: sin cierre ni integración parcial; caracterización antes de migrar |
| No-objetivos (`:75`, `:80`) | Insertar vistas y `BindingIntent` desde el panel siguen en el clásico |
| IC-03, IC-06, IC-07 e IC-09 (`docs/initiatives/I-64-implementation-clarifications.md`) | «Abrir en editor clásico»; el piloto no es la migración; A/B, cola aparcada y módulos como residual; el ámbito de mutación propia del Actualizar |
| ADR-0029 D7 a D13 (`docs/adr/0029-contrato-funcional-comun-de-ventanas-wpf.md:119-180`) | Una política de cierre; dirty por ámbito; `Owner`/`CenterOwner`; foco; «Crear un segundo modelo paralelo está prohibido»; caracterización primero y nunca editarla para que pase; un cambio de comportamiento exige decisión del Owner |
| El cierre de F4 (`docs/automation/evidence/I-64-pilot/F4-DRAFTS/gate-close.md`) | Lo que ya funciona (PREPARE, MUTATE y POST de T4; el veto; el conflicto) y sus limitaciones |
| T1B (`docs/initiatives/I-64-f4-t1b-gate-contract-package.md` y `I-64-pilot/F4-T1B-PILOT-CHARACTERIZATION/`) | La caracterización vigente de la ventana clásica: 65 pruebas, según el recuento de MB §9 |

## 2. Hallazgos del repositorio (medidos en `5ccaa770`)

Los hallazgos H-PB-01 a H-PB-15 de la v1 siguen vigentes, con las correcciones que traen los mapas (MA y MB). En el texto de este paquete se citan por su número.

| Id | Hallazgo (resumen) | Fuente |
|---|---|---|
| H-PB-01 | El núcleo de borradores es genérico (`DraftSession<TState>`, `DraftRegistry`, veto) | v1 |
| H-PB-02 | El host, el puerto y la UI están atados al campo único `AnnotationScale` | v1 |
| H-PB-03 | El copiador del borrador pierde los módulos y la cola aparcada, y `BuildDesign` depende de ellos. A/B y cola van juntos | v1; MB §2 |
| H-PB-04 | `ComposeForWrite` y `KeepPilotEditsOn` solo admiten la escala | v1; MB §2 |
| H-PB-05 | La escritura del panel ya redibuja todas las vistas desde el diseño completo, en una transacción y un paso de Deshacer | v1; MA §5 |
| H-PB-06 | A la escritura del panel le faltan borrar, crear, renombrar y la barrera de diagnósticos, y su `ComparePost` mira la escala | v1; MA §1-§5 |
| H-PB-07 | La ventana guarda estado authored en campos y en controles; no tiene ViewModel | v1; MB §1 |
| H-PB-08 | Actualizar va unido al cierre (`RequestDraw` → `Close()`) | v1; MB §6 |
| H-PB-09 | Subdiálogos con `Owner = this` | v1; MB §7 |
| H-PB-10 | El panel mide 320 px y el editor pide 1120 × 672 | v1 |
| H-PB-11 | La identidad de la ventana está fijada por pruebas y guardas: G-10, guardas de texto y T1B | v1; MB §9 |
| H-PB-12 | Las pérdidas (a) a (d) de la carga clásica | v1; MB §5 |
| H-PB-13 | Paso 64: la casilla A/B declara capacidad, no contenido | v1; MB §3; ratificado por [OD] |
| H-PB-14 | No hay racks A/B reales | v1 |
| H-PB-15 | La clasificación de lo estructural vive en el ensamblador | v1; MB §4 |

**Hallazgos nuevos de los mapas:** [se completan en el §2.1 con MA y MB].

### 2.1 Hallazgos nuevos

**Del camino clásico de escritura (MA):**

| Id | Hallazgo | Fuente | Consecuencia |
|---|---|---|---|
| H-PB-16 | **El «Actualizar» clásico NUNCA crea vistas.** La única creación de `EditPushBack` está en la rama `!window.UpdateOnly`, es decir, en Insertar (`src/RackCad.Plugin/RackPushBackCommands.cs:369-374`). El clásico solo crea las vistas que el usuario **pide** con RPB, el menú o RACKEDITAR→Insertar. Un frente nuevo, un poste nuevo o el lado B no crean vistas solas | MA H-054, H-064 | Para igualar al «Actualizar» clásico, el Actualizar del panel **no necesita crear vistas**. Crear vistas es Insertar, que sigue en el clásico (Proposal `:75`). Ver §6.4 |
| H-PB-17 | **El único borrado clásico es el de cortes laterales obsoletos:** el `PostIndex` del sobre ya no está entre los cortes del sistema nuevo, con la regla del último vínculo (`RackPushBackCommands.cs:320-356`). La planta y los frontales nunca se borran. Los cortes dependen del número de frentes y de los frentes en blanco, no de niveles, módulos ni lado B (`PushBackSystemLateralBuilder.cs:91-114`) | MA H-060 a H-063 | Define qué es «vista hermana obsoleta» para OB-04.1. La decisión está escrita en línea en el Plugin, sin función pura (MA H-110): PB-T3 la extrae sin cambiar el criterio |
| H-PB-18 | **Un frontal del lado B se redibuja aunque el rack haya dejado de ser compuesto:** el redibujo frontal no consulta la disponibilidad (`RackPushBackCommands.cs:325` solo para laterales), y el constructor ignora el lado en un rack no compuesto | MA H-058 | Hay que medir en AutoCAD (§6, bloque M) qué queda dibujado. PB-T3 no puede decidir sin esa medición si un frontal B es «obsoleto» |
| H-PB-19 | **El «Actualizar» clásico no es atómico:** por vista hace una o más transacciones de importación, una de redefinición, una de purga y una de renombrado, más las del borrado, todas con su propio commit. Una vista que falla se salta en silencio (`SystemBlockWriter.cs:98-101`; `RackPushBackCommands.cs:280`). Cuántos pasos de Deshacer deja es [NO MEDIBLE SIN AUTOCAD] | MA H-050, H-055 | El panel es más estricto: una transacción, todo o nada, un paso (H0, H-44). En F4B el panel conserva su atomicidad. Igualar al clásico es igualar el **resultado**, no sus transacciones |
| H-PB-20 | **Renombrar** cambia `system.Name`, el `Name` del sobre de cada vista redibujada y el nombre de la definición por `SyncName`. `SyncName` no hace nada si el nombre ya lo usa otro bloque y no añade sufijo. El GUID nunca cambia (`RackPushBackCommands.cs:186-190`; `RackBlockRenamer.cs:21-58`) | MA H-079 a H-085 | PB-T3 compone con el nombre del borrador (hoy usa el del sobre: MA H-099, H-103) y reutiliza `SyncName` |
| H-PB-21 | **Las barreras del clásico están en la ventana** (entradas inválidas y salida bloqueada en `RequestDraw`); el Plugin no vuelve a comprobar `RackBomOutputGate` | MA H-086, H-092 | El panel no pasa por `RequestDraw`. Necesita su propia barrera de diagnósticos bloqueantes en PREPARE (PB-T3), con el mismo criterio que la ventana |
| H-PB-22 | **El panel ya rechaza lo que no sabe hacer, con mensaje:** «El piloto no borra vistas y la vista … ya no está disponible en el sistema saliente: usa RACKEDITAR.» (`PushBackPilotUpdatePlan.cs:293-295`, `:398-401`) | MA H-101 | Es el patrón para OB-04.2 mientras no exista el borrado, y para cualquier caso no soportado |
| H-PB-23 | **Lo que el código no deja medir** (pasos de Deshacer del clásico, aspecto del frontal B, capa de una referencia insertada, el contenido real del Xrecord de una vista insertada tras una edición, el nombre visible tras renombrar) está listado en MA §11 | MA §11 | Bloque M de PB-T0 (§6) |

**Del estado del editor (MB):**

| Id | Hallazgo | Fuente | Consecuencia |
|---|---|---|---|
| H-PB-24 | **El estado authored está repartido en cuatro sitios:** (1) `composite`, es decir, los dos `PushBackEditorState`, la presencia, las topologías y la cola aparcada (Application, público); (2) campos de la ventana (`safetySelections`, `generalPallet`, `advanced`, `firstLevelDatum`, `loadedDimensionViews`/`dimensionViewsTouched`); (3) controles que `ReadInputs` relee **en cada pasada**; (4) controles que solo lee su manejador (`GapBox`, `CellFondoOverrideBox`, `NameBox`…). Cuatro de los campos de (2) no tienen costura de prueba | MB H-007 a H-031, H-118 | El mapa de propiedad de OB-05.1 se puede derivar del código. PB-T0 fija sus efectos sobre la salida, porque los campos sin costura solo se ven por su efecto |
| H-PB-25 | **Ninguna prueba de UI carga en la ventana los diseños compuesto (`pb-20-ab`) ni con cola (`pb-21-cola`).** Ninguna compara el diseño entero de la petición de Actualizar (segunda pasada) con el de la carga | MB H-034, H-036 | Hueco principal de caracterización. Es lo primero que PB-T1 rompería sin que se viera. Lo cubre el bloque C de PB-T0 |
| H-PB-26 | **La carga aparca la cola de cualquier diseño cuya estructura traiga ids de cola** (`GAP`, `B:*`), sea compuesto o no (`…Composite.cs:822`, `:835-843`) | MB H-042 | PB-T0 lo fija; D-PB-03 lo exige representado sin pérdidas |
| H-PB-27 | **El copiador del piloto pierde, y `ComposeForWrite` repone desde la base:** `Copy` no lleva baseline, sesión ni commit de módulos ni cola; `KeepPilotEditsOn` devuelve un estado sin ellos; `ComposeForWrite` vuelve a materializar desde `initial.Origin` | MB H-050 a H-053 | Confirma H-PB-03. PB-T1 hace el copiador completo |
| H-PB-28 | **«Restaurar estándar» parece no reiniciar de forma estable los parámetros avanzados.** El reinicio se aplica al `editorInputs` de una pasada, mientras el portador `advanced` de la ventana no se toca y la pasada siguiente lo vuelve a copiar. Es lectura de código, sin medir; la única prueba que pulsa el botón no lo observa | MB H-070, H-071 | PB-T0 lo **mide y lo fija tal como es** (sin corregir: «No product cleanup in PB-T0»). Si es un defecto, se registra como deuda, y su corrección sería una decisión nueva |
| H-PB-29 | **Las pérdidas, con su línea:** (a) `NumericField.SetNumber` formatea con «0.###», y cada recálculo relee las cajas de H-029 y las de la celda primaria (`NumericField.cs:131-163`); (b) `ReadInputs` arma las anotaciones sin `DimensionStyle` (`.xaml.cs:840-848`); (c) las sustituciones de `LoadFromModel` y de los combos de peralte; la lista fija de peralte posterior coincide hoy con el catálogo entregado; (d) `CommitCurrentCell` reescribe el frente 0 y la celda (0,0) en cada recálculo | MB H-076 a H-091 | (a) y (b) ya están fijadas por `WorkspaceDraftPilotWindowLoadCharacterizationTests` (pb-06, pb-07 y pb-08) y por `T19_H1_TheDimensionStyleStillComesOutNull`. **PB-TC tendrá que sustituirlas con registro** (§7.3) |
| H-PB-30 | **La ruta del piloto no pasa por la ventana:** `Materialize` reproduce la parte Application de `LoadExisting`, sin `LoadFromModel` ni el primer recálculo, y por eso conserva `DimensionStyle` y los decimales | MB H-049, H-032 | En PB-TC, la prueba «Workspace» del paso 4 de D-PB-06 es que la ruta del Workspace conserva el valor (ya cierto, se fija) y que la ventana ahora coincide con ella |
| H-PB-31 | **La ventana no admite un catálogo inyectado** (el constructor no lo acepta y la sesión carga `LoadCatalogSafe`). Las pruebas de UI usan el catálogo entregado, copiado a la salida | MB H-110 a H-115 | La caracterización de PB-T0 usa el catálogo entregado. Lo que solo cambiaría con otro catálogo, como el peralte posterior de (c), se describe y no se prueba |
| H-PB-32 | **Guardas que saltarían:** G-10; los tokens y literales de T-SG en todo `Systems/PushBack`; el orden exacto de tabulación de T-KB; las guardas de texto de I-55, módulos, desviador, foco y `AnnotationScaleBox`; el censo de ventanas. Al añadir pruebas: la guarda del corpus de WLC falla con un `pb-*.json` nuevo en `Fixtures/I64`; la de RAM Elements recorre todo `tests/RackCad.Tests`; una clase que sustituya el aviso de cierre debe ir en `EditorDiscardPromptCollection` | MB H-128 a H-135 | PB-T0 no pone fixtures en `Fixtures/I64` (§6.2). PB-T1 y PB-T2 tienen que convivir con estas guardas o sustituirlas con registro (§7.3) |
| H-PB-33 | **Lo que la ventana escribe por su cuenta:** «Guardar en biblioteca» escribe un `.rackcad.json` (y `EnsureId` acuña un GUID si no lo había); el BOM puede exportar XLSX o CSV; el configurador guarda su disposición en `<datos-locales-del-usuario>` al cerrarse. Ninguno escribe en el DWG | MB H-098 a H-100 | Datos para D-PB-07. PB-T0 no los ejecuta (como INV-F4T1B-06) |

## 3. Las diez decisiones (texto literal de [OD])

Cada decisión lleva el texto literal del Owner y, después, lo que este paquete deduce de ella como obligación (**OB-n**). Una OB no añade nada que [OD] no diga: si una OB pareciera ir más lejos, manda el texto literal.

### D-PB-01. La forma del editor completo

**Texto del Owner (literal):**

> ACCEPT RECOMMENDATION.
> The Workspace exposes:
> "Editar todo"
> which opens the existing Push Back editor as a modal/full-size editing
> surface loaded from the Workspace draft.
> In Workspace mode:
>
> * the editor reads from the draft;
> * its internal Apply/Actualizar action modifies the draft only;
> * it performs ZERO authored DWG writes;
> * the Workspace remains the authority for the final Actualizar;
> * T4 PREPARE / MUTATE / POST still performs the authored write;
> * the drawing cannot be edited while the modal editor is open.
>
> Do not build a second full Push Back editor.
> Do not try to fit the current ~1120 px editor inside the ~320 px panel.
> RACKEDITAR behavior must remain observably compatible.
> If the same window is used in two modes:
> CLASSIC MODE:
> existing RACKEDITAR semantics
> WORKSPACE MODE:
> draft-in / draft-out semantics
> The mode must be explicit.
> Do not let a Workspace invocation accidentally execute the classic
> drawing-write path.
> If changing the button label in Workspace mode improves clarity, that is
> allowed only if the package declares it and RACKEDITAR mode remains
> unchanged.

**Obligaciones:**
- **OB-01.1:** una sola ventana, `RackPushBackSystemWindow`, con dos modos **explícitos** (§4). No se crea un segundo editor.
- **OB-01.2:** en modo WORKSPACE, cero escrituras authored en el DWG desde la ventana y desde lo que la ventana pida. La escritura sigue siendo PREPARE, MUTATE y POST de T4, lanzada por el «Actualizar» del panel.
- **OB-01.3:** una invocación del modo WORKSPACE no puede llegar al camino clásico de escritura (`RackPushBackCommands`, MA §1). Lo prueba una guarda con su autoprueba en negativo (PB-T2).
- **OB-01.4:** modal. Mientras el editor está abierto no se puede editar el dibujo, igual que con RACKEDITAR.
- **OB-01.5:** el modo CLASSIC no cambia de forma observable (T1B en verde).
- **OB-01.6 (rótulos, declarados aquí como permite [OD]):**
  - en modo WORKSPACE, el botón «Actualizar» de la ventana se rotula **«Aplicar al borrador»**, con un tooltip que dice que el dibujo cambia con el «Actualizar» del panel;
  - «Insertar», los cuatro botones por vista e «Insertar varias…» no se muestran en modo WORKSPACE, porque escribirían en el DWG (OB-01.2);
  - en modo CLASSIC no cambia ningún rótulo.

  Si la revisión o PB-T2 encuentran que ocultarlos rompe algo de T1B en modo CLASSIC, se vuelve al Owner.

### D-PB-02. El lugar en el plan

**Texto del Owner (literal):**

> Authorize introduction of:
> F4B — PUSH BACK COMPLETO
> between:
> F4
> and
> F5-A
> F4 remains CLOSED / PASS WITH LIMITATIONS.
> F4B extends the proven Workspace architecture; it does not reopen F4.
> F5-A waits until F4B closes.

**Obligaciones:**
- **OB-02.1:** F4B se registra como disposición F4B-E01 en `I-64-pilot/F4B-DRAFTS/f4b-dispositions.md` y como aclaración **IC-11** en `I-64-implementation-clarifications.md`. La Proposal congelada no se edita.
- **OB-02.2:** F4B no reabre F4. Una limitación de F4 que F4B toque se cita, no se reescribe.
- **OB-02.3:** no hay integración parcial en `main` (A64-PV1-21). `main` no se toca.

### D-PB-03. A/B y cola aparcada en el alcance

**Texto del Owner (literal):**

> ACCEPT RECOMMENDATION.
> A/B Push Back racks are IN SCOPE for F4B.
> The current pilot rejection of A/B racks must be removed only after the full
> draft state can represent:
>
> * side A;
> * side B;
> * modules/fronts;
> * parked queue state;
> * all authored state required for correct redraw.
>
> Do not admit A/B racks before that state is lossless.

**Obligaciones:**
- **OB-03.1:** el rechazo del piloto («fuera del piloto (F5)», `CompositeOutOfPilotReason` y `DormantTailOutOfPilotReason`) **sigue** hasta que PB-T1 pruebe la ida y vuelta sin pérdidas de los lados A y B, los módulos y frentes, la cola aparcada y todo el estado authored del redibujo.
- **OB-03.2:** quitar el rechazo es un paso explícito, con su prueba, en la primera tarea cuyo contrato lo autorice. No antes de PB-T1 cerrada; previsto en PB-T3 o PB-T4.

### D-PB-04. Vistas: borrar sí; crear, solo tras PB-T0

**Texto del Owner (literal):**

> ACCEPT RECOMMENDATION WITH THE PROPOSED BOUNDARY.
> F4B may delete obsolete sibling views when the new full design no longer
> requires them.
> Creating NEW views is NOT yet authorized by this decision.
> PB-T0 must characterize:
>
> * when the classic path creates a new view;
> * view naming;
> * placement;
> * orientation;
> * layer;
> * scale;
> * annotations;
> * relationship to existing sibling views;
> * Undo behavior.
>
> After PB-T0:
> if creation semantics are sufficiently characterized and fit the T4
> architecture, include creation in the later write task.
> If not:
> return to Owner before adding view creation.

**Obligaciones:**
- **OB-04.1:** PB-T3 puede borrar vistas hermanas obsoletas dentro de la transacción única. Usa una primitiva caller-owned con guarda de fachada.
- **OB-04.2:** crear vistas **no** está autorizado. Hasta que PB-T0 lo caracterice y el contrato de PB-T3 lo incluya, una edición que exija crear una vista se **rechaza** en el panel con un diagnóstico que remite a RACKEDITAR.
- **OB-04.3:** PB-T0 caracteriza los nueve puntos de la lista (§6.3, M-6). Al cerrar PB-T0, la sesión decide con la tabla del §6.4. Si la creación no encaja en T4, **vuelve al Owner** antes de añadirla.

### D-PB-05. «Conservar mi borrador» con el editor completo

**Texto del Owner (literal):**

> MODIFY THE RECOMMENDATION SLIGHTLY.
> Do NOT define it as:
> "the entire stale serialized draft wins."
> Instead:
> "the user's FULL EDITABLE PUSH BACK DESIGN wins for every field owned by the
> full Push Back editor, after explicit conflict presentation."
> At reconciliation time:
>
> 1. authoritative current rack is reread;
> 2. external differences are shown/classified;
> 3. user explicitly chooses "Conservar mi borrador";
> 4. all fields OWNED by the full Push Back editor come from the user's draft;
> 5. envelope/infrastructure fields NOT owned by that editor come from the
> current authoritative rack;
> 6. unknown/future ExtensionData must not be silently destroyed;
> 7. identity/schema/session metadata must not be rolled back from a stale
> draft.
>
> Therefore:
> FULL EDITABLE DESIGN wins
> but:
> STALE ENVELOPE / NON-EDITOR METADATA does NOT.
> This preserves the safety rule established in F4 while allowing a true full
> Push Back editor.
> "Conservar mi borrador" must be an informed destructive choice.
> Before accepting it, show enough information to distinguish at least:
>
> * fields changed externally;
> * fields changed by the user;
> * overlapping changes;
> * structural changes such as modules/fronts/side B;
> * view-related implications where applicable.
>
> Do not require a generic JSON diff as the user-facing UX.

**Obligaciones:**
- **OB-05.1 (mapa de propiedad):** PB-T1 define y fija con pruebas qué campos son **propiedad del editor completo** y cuáles son **sobre o infraestructura**: identidad, esquema, sesión, `ExtensionData` desconocida y lo que no edita la ventana. El mapa se deriva de lo que la ventana carga y escribe (MB §1). No se inventa.
- **OB-05.2 (composición):** PB-T3 generaliza `ComposeForWrite` y `KeepPilotEditsOn`:
  - los campos del editor salen del borrador;
  - el sobre sale del rack actual releído;
  - `ExtensionData` desconocida se conserva (I-11);
  - la identidad, el esquema y la sesión nunca vuelven atrás desde un borrador viejo.

  Lo prueba una prueba por cada uno de los siete pasos.
- **OB-05.3 (clasificación):** un clasificador puro de Application separa:
  - cambiado fuera;
  - cambiado por el usuario;
  - solapado;
  - estructural (módulos, frentes, lado B);
  - implicación de vistas (las que se borrarían o, si llega a autorizarse, se crearían).

  Lo construye PB-T3; PB-T4 lo presenta. **Nunca** se presenta un diff JSON genérico.
- **OB-05.4:** «Conservar mi borrador» es una elección destructiva informada: confirmación explícita después de la presentación. Sin elección no se escribe.

### D-PB-06. Las pérdidas de la carga clásica

**Texto del Owner (literal):**

> ACCEPT THE PRODUCT CORRECTIONS, BUT NOT AS AN INCIDENTAL REFACTOR.
> The two known losses:
>
> * saved dimension style;
> * rounding behavior/state;
>
> may be corrected in F4B because the shared full editor state must otherwise
> reproduce data loss in both Workspace and RACKEDITAR.
> However they are explicit changes to classic behavior.
> Therefore:
>
> 1. characterize the CURRENT lossy behavior first;
> 2. add RED tests proving each loss;
> 3. implement the corrected shared state/load behavior;
> 4. prove both:
>    * Workspace;
>    * RACKEDITAR
> now preserve the intended value;
> 5. update the compatibility characterization explicitly;
> 6. record this as an intentional classic-editor behavior correction, not as
> "no observable change".
>
> Do not hide these corrections inside PB-T1 state extraction.
> If either correction turns out to alter drawing geometry or another
> downstream contract materially:
> return to Owner.

**Obligaciones:**
- **OB-06.1:** PB-T0 caracteriza el comportamiento con pérdida ACTUAL de (a), el redondeo, y de (b), el estilo de cotas guardado (§6.2; lo ya fijado por WLC y por `T19_H1` se cita y no se duplica).
- **OB-06.2:** las correcciones van en una tarea propia, **PB-TC**, **antes** de PB-T1 (§5). Así PB-T1 extrae el estado ya corregido y no tiene que reproducir una pérdida desde un estado que deja de depender de los controles.
- **OB-06.3:** PB-TC hace el paso 2 (RED por pérdida), el 3, el 4 sobre las dos rutas que existen en ese momento y el 5 con un registro declarado que sustituye la caracterización (§7.3). El paso 6 lo cumple su registro en `f4b-dispositions.md`. PB-T2 vuelve a probar el paso 4 en modo WORKSPACE.
- **OB-06.3b:** las pruebas vigentes que fijan la pérdida (`T19_H1_TheDimensionStyleStillComesOutNull` y las expectativas de pb-06, pb-07 y pb-08 de WLC) no se editan. Se sustituyen según el §7.3, con el registro declarado.
- **OB-06.4:** si alguna corrección cambia la geometría dibujada u otro contrato aguas abajo de forma material, PB-TC hace STOP y **vuelve al Owner**.
- **OB-06.5:** (c) y (d) no están autorizadas. PB-T0 las mide. Una corrección suya sería una decisión nueva del Owner.

### D-PB-07. «Guardar en biblioteca», BOM y lote de vistas en el modo WORKSPACE

**Texto del Owner:** [OD] **no trae texto para D-PB-07.**

**Lo que fijan otras partes de [OD]:**
- D-PB-01: «ZERO authored DWG writes» y «Do not let a Workspace invocation accidentally execute the classic drawing-write path». En modo WORKSPACE eso deja fuera **Insertar** y el **lote de vistas** (OB-01.6), porque escriben en el DWG.
- PB-T0 caracteriza los subdiálogos y «all drawing writes initiated after the classic editor returns».

**Lo que queda abierto:** si «Guardar en biblioteca» (escribe un archivo de biblioteca, no el DWG) y «BOM» (MB §6) se ofrecen en modo WORKSPACE. Hasta la decisión:
- **OB-07.1:** la sesión **no** decide por analogía. La pregunta va al Owner antes de emitir el contrato de PB-T2, el primero al que afecta, con lo que PB-T0 mida.
- **OB-07.2:** no bloquea PB-T0.

### D-PB-08. El lado B declarado sin frentes

**Texto del Owner (literal):**

> Ratify the measured behavior:
> checking "Rack de dos sentidos (lado B)" means side B is permitted to exist.
> It does NOT by itself create persisted side-B content.
> If all side-B fronts remain inactive/empty:
> side B is not persisted
> and reopening may show the checkbox unchecked.
> This is:
> DESIGN / CHARACTERIZED BEHAVIOR
> not a persistence defect.
> However:
> accept D-PB-08 conceptually:
> the UI should make this consequence understandable.
> Do not silently imply that checking the box alone creates a persisted side B.

**Obligaciones:**
- **OB-08.1:** la regla (capacidad sin presencia) se mantiene, y su caracterización (`PushBackBlankFrontAuthorityTests`) sigue en verde.
- **OB-08.2:** se añade una indicación **no bloqueante** visible cuando el lado B está declarado y no tiene ningún frente activo. Dice que el lado B no se guardará así. Va en los dos modos, porque es la misma ventana.
- **OB-08.3:** en RACKEDITAR es un cambio observable autorizado. Va en PB-TC, con la misma disciplina que D-PB-06: RED, registro declarado y anotación en `f4b-dispositions.md`.
- **OB-08.4:** la presentación concreta (texto y lugar) la propone el paquete de PB-TC. Si la revisión de ese paquete la considera una decisión material de producto, vuelve al Owner.

### D-PB-09. Smoke-PB

**Texto del Owner (literal):**

> ACCEPT WITH ONE DISTINCTION.
> Smoke-PB may use intentionally fabricated A/B test racks because no
> representative real A/B rack currently exists.
> But label them explicitly:
> CONSTRUCTED REPRESENTATIVE TEST CASES
> not:
> real production examples.
> Smoke-PB should still use real existing Push Back racks for the behaviors
> where real examples exist.

**Obligaciones:**
- **OB-09.1:** cada rack A/B fabricado se rotula en la guía, el registro y la evidencia como **CONSTRUCTED REPRESENTATIVE TEST CASES**.
- **OB-09.2:** el resto de los comportamientos se prueban sobre racks Push Back reales, en copias de trabajo. Los originales nunca se abren.

### D-PB-10. Presupuesto

**Texto del Owner (literal):**

> Each separately contracted PB task starts at:
> 1 / 3
> No shared 18-attempt pool.
> Attempt 3 always returns to Owner unless separately authorized later.

**Obligaciones:**
- **OB-10.1:** cada tarea contratada (PB-T0, PB-TC, PB-T1, PB-T2, PB-T3, PB-T4 y PB-T5, y sus subtareas si se dividen) empieza en 1/3.
- **OB-10.2:** el intento 2 lo puede decidir la sesión por delegación, como en F4. Antes del intento 3 se vuelve **siempre** al Owner.

### Disposiciones de [OD] sobre las tareas, T1B y RACKEDITAR

Se recogen literales en el §5 (cada tarea) y en el §7 (T1B y RACKEDITAR).

## 4. Los dos modos de la ventana

[OD] exige que el modo sea explícito y que el modo WORKSPACE no pueda llegar al camino clásico de escritura. El diseño de PB-T2 lo cumple así:

| | CLASSIC | WORKSPACE |
|---|---|---|
| Quién la abre | RACKEDITAR, RPB y el menú (los cuatro sitios de T1B H-02) | Solo el «Editar todo» del panel |
| De dónde carga | El dibujo (`LoadExisting`), la biblioteca o nada | El estado del borrador (PB-T1) |
| Botón principal | «Actualizar» e «Insertar», sin cambios | «Aplicar al borrador» (OB-01.6) |
| Qué hace el botón | Anota la intención y cierra. Escribe quien llamó (MA §1) | Devuelve el estado editado al `DraftSession`. Ninguna escritura en el DWG |
| Cerrar sin aplicar | Política actual (T1B) | El borrador queda como estaba. Sin escribir |
| Dueño de los subdiálogos | Como hoy (T1B CT-16, CT-20 y CT-21) | El que decide el host (D-15, punto 4) |
| Guardas | T1B en verde | Una guarda de «sin camino clásico» con su negativa (OB-01.3) |

- **El modo se fija al construir la ventana**, con un valor explícito que no tiene valor por defecto en el lado WORKSPACE.
- **El modo CLASSIC sigue siendo el de los cuatro sitios actuales,** sin tocar sus llamadas.
- La ventana en modo WORKSPACE **no recibe** el comando clásico ni sus servicios de dibujo.

## 5. Descomposición

**Cómo se cita en esta tabla:** la columna «Texto de [OD]» pone cada texto del Owner en una sola línea. Se omiten los saltos de línea y las viñetas («* ») del original, y «…» marca un corte. Manda el texto literal de `owner-f4b-disposition.md`.

| Tarea | Contenido | Depende de | Texto de [OD] |
|---|---|---|---|
| **PB-T0 — Caracterización y medición** | §6. Sin producto | — | «ACCEPT. PB-T0 is characterization / measurement only. … No product cleanup in PB-T0.» |
| **PB-TC — Correcciones declaradas** | D-PB-06 (a) y (b), y D-PB-08, con RED, corrección, prueba en RACKEDITAR y en la ruta del Workspace que exista, y registro declarado | PB-T0 | Los de D-PB-06 y D-PB-08 (§3) |
| **PB-T1 — Estado editable sin pérdidas fuera de la ventana** | El estado de la ventana pasa a Application, sin pérdidas y sin alias. La ventana lo usa en modo CLASSIC sin cambio observable. Copiador y comparador completos (módulos, cola y A/B). El mapa de propiedad (OB-05.1) | PB-TC | «ACCEPT IN PRINCIPLE. PB-T1 extracts a lossless Push Back editable state outside the WPF window. Hard requirements: module state preserved; parked queue preserved; A/B preserved; no aliasing; no WPF control becomes source of truth; state round-trip lossless for supported classic racks; classic RACKEDITAR still works through the extracted state. Do not rewrite the entire editor merely to obtain an MVVM architecture. The goal is state separation, not aesthetic refactoring.» |
| **PB-T2 — Aplicar, cerrar y escribir separados; los dos modos** | Separar «aplicar al estado», «cerrar» y «escribir». El modo explícito (§4) y la guarda de «sin camino clásico». El dueño de los subdiálogos lo decide el host. La validación de campos es una sola. Se repite la prueba de D-PB-06 en WORKSPACE | PB-T1 | «ACCEPT IN PRINCIPLE. Separate: "apply edits to current editing state" from: "close editor" and from: "write authored DWG state". Workspace mode must return edited state to DraftSession. Classic RACKEDITAR may continue its compatible existing outer workflow. Avoid two independent implementations of field validation.» |
| **PB-T3 — Escritura completa** | El plan y la corrida generalizados; borrar vistas (caller-owned); renombrar; la barrera de diagnósticos; `ComparePost` genérico; la composición de D-PB-05 y su clasificador; crear vistas solo si PB-T0 lo autoriza. PREPARE → MUTATE → POST y un solo Actualizar lógico | PB-T1 (en paralelo con PB-T2) | «ACCEPT IN PRINCIPLE. The Workspace authored writer may expand to full Push Back design. It must eventually support, as characterized/authorized: field changes; modules/fronts; side B; rename; deleting obsolete views; blocking authored write on diagnostics/errors; view creation only if PB-T0 subsequently authorizes it. Still require: PREPARE -> MUTATE -> POST and one logical Update.» |
| **PB-T4 — «Editar todo» en el panel** | Abre la ventana en modo WORKSPACE sobre el borrador; el puerto generalizado; la presentación de D-PB-05; el resumen y «Modificado» | PB-T2 y PB-T3 | «ACCEPT. Add "Editar todo" to the Workspace only after PB-T1/T2 establish the draft-based editor seam. Do not make PB-T4 invent state translation logic. It should consume the already proven full Push Back draft/state API.» |
| **PB-T5 — Cierre** | CI exacta, Smoke-PB (D-PB-09) y gate-close de F4B | PB-T4 | D-PB-09 |

**Por qué PB-TC va antes de PB-T1.** [OD] prohíbe esconder las correcciones en PB-T1. Si fueran después, PB-T1 tendría que reproducir la pérdida (a), el redondeo, desde un estado que ya no depende del control (H-PB-12, MB §5). Eso es imposible sin fingir el redondeo. Con PB-TC antes, PB-T1 extrae el comportamiento ya corregido y caracterizado.

**La prueba en el Workspace de D-PB-06:**
- PB-TC la hace sobre la ruta del Workspace que existe en ese momento (el piloto: MB §2, MB §5);
- PB-T2 la repite en modo WORKSPACE (OB-06.3).

**Cada tarea** pasa por la cadena de F4: paquete con revisión independiente, contrato, planificación, Worker (RED y GREEN), CI exacta, verificación, revisiones directa e independiente, controles nc1 a nc3 y cierre por D2. PB-T0 la adapta en el §6.5.

## 6. PB-T0 al detalle

**Texto del Owner (literal):**

> ACCEPT.
> PB-T0 is characterization / measurement only.
> It must establish the missing facts before structural migration.
> At minimum characterize:
>
> * editor state ownership;
> * module state;
> * parked queue;
> * A/B behavior;
> * add/remove fronts;
> * add/remove modules;
> * rename;
> * view deletion;
> * view creation;
> * classic update/close distinction;
> * subdialogs;
> * catalog dependencies;
> * dimension-style loss;
> * rounding loss;
> * all drawing writes initiated after the classic editor returns.
>
> No product cleanup in PB-T0.

### 6.1 Forma de PB-T0

PB-T0 es **una tarea con presupuesto 1/3**. Tiene tres bloques, y ninguno cambia producto:

| Bloque | Qué es | Quién | Producto en el commit |
|---|---|---|---|
| **D — Mapa documental** | MA y MB, verificados por la revisión independiente de este paquete, más las correcciones que esa revisión pida | La sesión (ya hecho, en revisión) | Ninguno. Se custodia como evidencia |
| **C — Caracterización automática** | Pruebas nuevas que fijan el comportamiento **actual** de lo que PB-T1, PB-T2 y PB-T3 van a tocar (§6.2) | Worker, por la cadena de F4 con contrato propio | Solo archivos nuevos de prueba. Sin `src/`, salvo los mutantes del RED revertidos en el GREEN (patrón de T1B) |
| **M — Medición física** | Lo que solo AutoCAD puede mostrar y F4B necesita (§6.3) | La sesión, por control de pantalla, como en Smoke-2 | Ninguno. Registro, inventarios y capturas custodiados |

**El informe de PB-T0** (`pb-t0-report.md`) responde a cada uno de los quince puntos del Owner y a los nueve de vistas de D-PB-04, con su fuente: D, C o M. Incluye además la tabla de decisión del §6.4. PB-T0 se cierra cuando el informe está completo, el bloque C está cerrado por la cadena y el bloque M está custodiado.

**Orden:**
1. el bloque D ya existe;
2. el bloque M puede ir en paralelo con la preparación del contrato del bloque C, porque sus hechos sirven a PB-T3 y no a las pruebas de C;
3. el bloque C sigue la cadena completa.

### 6.2 Bloque C — qué se fija con pruebas

Cada punto lleva su clase de prueba:
- **UI-STA**: la ventana real, sin mostrar o fuera de pantalla;
- **Core**: Application, sin WPF.

Antes de escribir nada, el Worker cruza cada punto con la cobertura existente (MB §9 y las pruebas que cita MB) y **no duplica** lo ya fijado. Lo que ya está fijado se cita en el informe con su prueba.

| Punto del Owner | Qué se fija | Clase | Cobertura existente que no se duplica |
|---|---|---|---|
| Propiedad del estado | Para cada diseño del corpus de PB-T0: la salida de la carga (`LastComputation.Design`) frente al diseño interior del sobre, con los deltas **clasificados** (las clases (a) a (d) y cualquier otra que aparezca, que se fija tal cual); y la salida de la segunda pasada, la de Actualizar, frente a la primera (H-PB-25) | UI-STA | WLC (los 14 de un sentido) |
| Módulos | Escenificar, confirmar y cancelar sobre un rack modular cargado; la reconciliación por `ModuleId`+`Kind` en la salida | UI-STA y Core | `PushBackModuleEditorWindowTests`, `PushBackCompositeLoadExistingModulesTests` |
| Cola aparcada | Cargar en la ventana `pb-21-cola` y un compuesto con cola; aparcado y despertar al declarar B con un frente activo; la salida de cada paso | UI-STA | `PushBackDormantTailTransactionTests`, `PushBackDormantRestoreTests`, `PushBackCompositeLifecycleWiringTests` (que no cargan esos diseños) |
| A/B | Cargar `pb-20-ab`; declarar B sin frentes (no compuesto); con un frente (compuesto); quitar B; las ranuras ausentes en la salida | UI-STA | `PushBackBlankFrontAuthorityTests`, `PushBackCompositeWindowTests` |
| Añadir/quitar frentes | `+ frente`, `- frente` y «En blanco», con B declarado y sin él: la estructura de salida y **los cortes laterales** que da `PushBackSystemLateralBuilder` antes y después (la parte pura del borrado) | UI-STA y Core | `LateralCortes_ProduceOnePlanPerTransversePost` (de un diseño) |
| Añadir/quitar módulos | No hay gesto directo (MB H-067): cambian por reconstrucción (tarima, fondos o «Restaurar estándar»). La secuencia de módulos de salida en cada caso; y H-PB-28 (los avanzados tras «Restaurar estándar»), medido y fijado tal cual | UI-STA | `PushBackModuleEditorWindowTests:279-302` (solo `IsManualOverride`) |
| Renombrar | `NameBox` → `RackName` y la petición; un nombre en blanco | UI-STA | T1B CT-05 (recorte) |
| Borrado de vistas | La decisión pura: con los diseños antes y después de cada edición del punto «frentes», qué `PostIndex` desaparecen de los cortes. El borrado en el DWG es del bloque M | Core | Las guardas de texto de `EditPushBack` (MA H-121) |
| Creación de vistas | La intención que expone la ventana (vista, sección y `Views`) | — | T1B CT-06 a CT-09 y CT-23. **Nada nuevo** |
| Actualizar frente a cerrar | — | — | T1B (T-CL y T-IN). **Nada nuevo** |
| Subdiálogos | Lo que **devuelve** a la ventana cada subdiálogo con costura (configurador, seguridad, botas, defensas y topes) y cómo cambia la salida | UI-STA | T1B fija los dueños y los textos, no el estado devuelto |
| Dependencias de catálogo | Las sustituciones de (c) que dependen del catálogo entregado (poste en blanco o desconocido, peralte fuera de la lista, combos de viga) | UI-STA | WLC (sin delta de clase (c) en su corpus) |
| Pérdida del estilo de cotas | — | — | `T19_H1_TheDimensionStyleStillComesOutNull` y WLC pb-06 y pb-07. **Nada nuevo** |
| Pérdida por redondeo | Qué campos redondean (la lista de MB H-077) y cuáles no (MB H-079), campo a campo | UI-STA | WLC pb-08 (dos campos) |
| Escrituras al volver la ventana | — | — | Guardas de texto de T1B (T-PG) y las de MA H-120 a H-123. El efecto real es del bloque M |

**Corpus de PB-T0:**
- los 14 diseños de WLC y `pb-20-ab` y `pb-21-cola`, leídos de `tests/RackCad.Tests/Fixtures/I64/` **sin añadir nada a esa carpeta** (H-PB-32);
- los diseños nuevos que hagan falta (compuesto con módulos y cola, seguridad completa, avanzados no por defecto), en una carpeta nueva, por ejemplo `tests/RackCad.Tests/Fixtures/F4B-PBT0/`, o construidos en código.

**Reglas de T1B que el contrato de PB-T0 copia en su texto** (no se heredan por analogía, §7.2):
- ninguna prueba existente se edita, se reactiva ni se borra;
- sin `Skip`;
- se fija lo que hay: si algo contradice el diseño, se comenta «INCUMPLIMIENTO CARACTERIZADO»;
- ninguna prueba ejecuta «Guardar en biblioteca», el BOM, el configurador real ni un `MessageBox` real;
- ventanas fuera de pantalla;
- textos copiados byte a byte;
- sin costuras nuevas ni cambios en `InternalsVisibleTo`;
- determinismo (GUID de `newIdFactory` y cultura fijada);
- las clases que sustituyen el aviso de cierre van en `EditorDiscardPromptCollection`.

**RED:** se hace con mutantes deliberados del producto, revertidos en el GREEN, con una **tabla cerrada** en el paquete del contrato de PB-T0 y la prueba por mutante, como INV-F4T1B-02 e INV-F4T1B-14 a 15. Es el único RED conocido que funciona para pruebas de caracterización, que pasan sobre la base. El paquete del contrato de PB-T0 fija la tabla, el `AllowedWriteScope`, el `ForbiddenWriteScope`, los invariantes, `RequiredTests` y las paradas. Pasa por su propia revisión independiente antes de emitirse.

### 6.3 Bloque M — medición física en AutoCAD

**Qué se mide:** solo lo que el código no deja medir (MA §11) y F4B necesita.

| Id | Medición | Para qué | Hecho |
|---|---|---|---|
| M-1 | RACKEDITAR → «Actualizar» tras **añadir un frente** a un rack real con planta, frontales y cortes laterales: qué definiciones cambian, si aparece alguna vista nueva (se espera que no, H-PB-16) y los nombres | Igualdad de PB-T3 | MA H-054 |
| M-2 | «Actualizar» tras **quitar un frente** cuyo poste tenía corte lateral: el corte obsoleto se borra; la regla del último vínculo | OB-04.1 | MA H-060 a H-063 |
| M-3 | «Actualizar» tras **renombrar**: nombres de las definiciones, `Name` de cada Xrecord y la colisión con un nombre ya usado | PB-T3 | MA H-079 a H-085 |
| M-4 | **Conversión a A/B** (declarar B y activar un frente) y vuelta a un sentido, sobre un rack fabricado y rotulado CONSTRUCTED REPRESENTATIVE TEST CASES: qué queda dibujado en los frontales del lado B | H-PB-18 | MA H-058 |
| M-5 | **Deshacer** de cada «Actualizar» de M-1 a M-4: cuántos `U` hacen falta para volver al estado anterior, comprobado con el inventario de definiciones y Xrecord después de cada `U` | La igualdad de resultado y el registro de que el clásico no es atómico | MA H-012, H-055 |
| M-6 | **Creación por Insertar**, para cerrar los nueve puntos de D-PB-04 aunque F4B no cree vistas: nombre y sufijo, colocación (jig), orientación, capa de la referencia, escala, cotas y textos dentro del bloque, enlace con las hermanas (GUID), el contenido del Xrecord de la vista nueva tras una edición, y Deshacer | OB-04.3 | MA H-064 a H-070, H-077 |

**Cómo se mide** (reglas de Smoke-2 y de la memoria de operación):
- **DLL:** el DLL Debug del Candidato de F4 que validó Smoke-2 (`<carpeta-del-DLL-de-C2>`, registrado en `F4-SMOKE-2/prep/dll-c2.md`). PB-T0 comprueba antes de medir que entre su commit y `5ccaa770` no hay diferencias en `src/`. Si las hubiera, STOP. Sin compilar nada nuevo y sin arnés: es el comportamiento clásico tal cual.
- **Dibujos:**
  - copias de trabajo nuevas, hechas con `copy` de los originales y comprobadas por SHA-256 antes y después;
  - **los originales nunca se abren**;
  - nada se guarda sobre un original;
  - al terminar, se cierra sin guardar o se guarda solo la copia.
- **Operación:** la sesión, por control de pantalla, con las reglas medidas en Smoke-2:
  - textos en trozos de ≤ 16 caracteres;
  - líneas de ≤ 1024 caracteres;
  - 3 s entre `defun`;
  - una captura antes de cada clic en el panel;
  - cada captura se abre y se revisa al tomarla.
- **Lo que no se toca:** Trusted Locations, SECURELOAD ni ninguna configuración de seguridad.
- **Inventarios:** las funciones LISP de inventario de Smoke-2, tecleadas por documento: definiciones con su GUID, `Name`, vista y sección desde el Xrecord, y referencias.
- **Evidencia:** registro con hora; inventarios antes y después de cada paso; capturas recortadas del panel y de la línea de comandos, sin rutas ni nombres de cliente. Cada medición queda como hecho medido, o como **NO OBSERVABLE** si AutoCAD no lo muestra. No se finge una condición interna.
- **Si una medición contradice MA,** manda la medición y se corrige el mapa. Si contradice un supuesto de este paquete, se registra y se revisa la tarea afectada antes de su contrato.

### 6.4 La decisión sobre la creación de vistas al cerrar PB-T0

| Resultado de PB-T0 | Decisión |
|---|---|
| M-1 y M-4 confirman H-PB-16: el «Actualizar» clásico no crea vistas | PB-T3 iguala al clásico **sin crear vistas**. Crear vistas sigue siendo Insertar, en el clásico (Proposal `:75`). No se añade creación, así que no hace falta volver al Owner (OB-04.3) |
| M-1 o M-4 muestran que el «Actualizar» clásico crea alguna vista | Se caracteriza con los nueve puntos de M-6. Si encaja en PREPARE → MUTATE → POST de T4, entra en el contrato de PB-T3. Si no, **vuelve al Owner** antes de añadirla |
| Cualquier resultado | Mientras PB-T3 no tenga creación autorizada, una edición del borrador que en el clásico exigiría una vista nueva se rechaza con un diagnóstico que remite a RACKEDITAR (OB-04.2) |

### 6.5 La cadena de PB-T0

- **Bloque C:**
  - la cadena de F4 completa: paquete del contrato con revisión independiente, emisión, planificación del Controller, A1-A8, nc4, Worker (RED con mutantes y GREEN), CI exacta, verificación, revisiones directa e independiente, nc1 a nc3 y cierre;
  - la replanificación y los sustitutos, según el §7.2.
- **Bloque M:** no consume intentos del bloque C. Un error del operador se repite sin gastar intento, como en Smoke-2. Un resultado que exija cambiar producto **no se corrige en PB-T0**: se registra para la tarea que corresponda.
- **El cierre de PB-T0:**
  - el informe completo;
  - la tabla del §6.4 resuelta;
  - la custodia;
  - la anotación en `f4b-dispositions.md`.

## 7. Reglas de proceso, T1B y RACKEDITAR

### 7.1 Presupuesto e intentos

Se aplica D-PB-10 (OB-10.1 y OB-10.2).

### 7.2 Replanificación y sustitutos de control

Las reglas de F4 atan solo a las tareas que nombran. En F4B **no se heredan por analogía**: cada contrato de F4B que quiera una regla la fija **en su texto**.
- **Replanificación:** el contrato de cada tarea de F4B fija la de F4 (F4-E31, F4-E43): un rechazo de la planificación solo por la comparación literal de las colecciones permite una replanificación única con el prompt reforzado, sin gastar intento; un segundo rechazo vuelve al Owner.
- **Sustitutos de nc1 a nc3:** se aplica la regla permanente del Owner: «Un sustituto de control solo se reutiliza cuando la firma sea EXACTAMENTE una de las ya autorizadas. No generalizar por nombre de check.»
  - Las firmas autorizadas son PD-F4-01 (nc2, lista blanca exacta) y PD-F4-04 (nc1, Handoff antes que Identity).
  - Cualquier otra firma, o una variación, vuelve al Owner antes de usar un sustituto.

### 7.3 T1B y los cambios declarados

**Texto del Owner (literal):**

> Do not edit T1B characterization tests merely because F4B changes a
> behavior.
> For behavior intentionally preserved:
> T1B must remain green.
> For D-PB-06 or another explicitly authorized behavior change:
> create a declared superseding characterization/change record.
> Do not silently weaken the old guard.

**Obligaciones:**
- **OB-T1B.1:** ninguna prueba de T1B se edita para que pase.
- **OB-T1B.2:** cuando un cambio autorizado (D-PB-06, D-PB-08 o los rótulos de OB-01.6 en modo WORKSPACE) choca con una prueba o guarda vigente, se sigue el patrón de I-39:
  - la prueba vieja queda con `Skip` y un motivo que nombra el registro;
  - se añade el contrato equivalente que sí se ejecuta;
  - se crea el registro declarado `I-64-pilot/F4B-DRAFTS/superseding-characterization.md`, con la prueba vieja, la nueva, la autoridad y el motivo.
- **OB-T1B.3:** la guarda G-10 (una sola `.RequestUpdate(`) y las guardas de texto de T1B solo se sustituyen así, nunca se debilitan sin registro.

### 7.4 RACKEDITAR

**Texto del Owner (literal):**

> RACKEDITAR remains a compatibility surface throughout F4B.
> Observable behavior must remain unchanged except for explicitly authorized
> corrections such as D-PB-06.
> No forced migration to Workspace.
> No removal.

**Obligación OB-RE.1:** RACKEDITAR, RPB y el menú siguen abriendo la ventana en modo CLASSIC con su comportamiento actual. Solo cambian D-PB-06 (a) y (b) y D-PB-08, que se registran en `f4b-dispositions.md` y en el registro declarado.

## 8. Riesgos

| Id | Riesgo | Tratamiento |
|---|---|---|
| R-PB-01 | Mover el estado de una ventana de unas 4500 líneas sin cambiar su comportamiento | PB-T0 primero; PB-T1 sin estética (es la orden de [OD]); T1B y la caracterización de PB-T0 como red |
| R-PB-02 | La semántica del compuesto y de la cola aparcada (H14: «dormir no persiste») | PB-T0 la fija; la cola aparcada vive en el borrador igual que en el clásico |
| R-PB-03 | Borrar vistas dentro de la transacción única es nuevo en el panel | Primitiva caller-owned con guarda; inyecciones UNIT y SEAL; Smoke-PB |
| R-PB-04 | No hay racks A/B reales | CONSTRUCTED REPRESENTATIVE TEST CASES (D-PB-09) |
| R-PB-05 | Un adaptador a la medida de Push Back que no sirva para F5-A | El ciclo de vida del panel y del puerto, genérico en el tipo de estado; lo propio de Push Back, en `Systems/PushBack` (ADR-0029 D12) |
| R-PB-06 | Un Actualizar grande tarda segundos | Se mide en Smoke-PB; sin cambio en F4B |
| R-PB-07 | Una corrección de D-PB-06 cambia la geometría | STOP y vuelta al Owner (OB-06.4) |
| R-PB-08 | La medición física de PB-T0 depende del operador (el paso 50 de Smoke-2) | Las reglas de operación del §6.3; captura antes de cada clic; cada medición con su captura abierta y revisada |

## 9. Requisitos de cierre de F4B

- Todas las tareas cerradas.
- T1B en verde o sustituida con registro.
- Los INV del §1 cubiertos para Push Back completo.
- CI exacta del Candidato.
- Smoke-PB PASS, con los casos fabricados rotulados.
- Firma del Owner.
- Sin integración parcial en `main`.

## 10. Siguiente

1. Revisión adversarial independiente de esta v2 (orden de [OD]).
2. Resolver todo bloqueante y todo importante.
3. **Volver al Owner solo si** la revisión introduce una decisión **material de producto nueva**. D-PB-07 no lo es: ya está abierta y no bloquea PB-T0 (OB-07.2).
4. Si la revisión termina con 0 bloqueantes, 0 importantes sin resolver y ninguna decisión material nueva: se prepara el paquete del contrato de PB-T0 (§6), con su propia revisión independiente como en F4, y se emite con attempts 1/3 sin volver al Owner, por la autorización condicional de [OD].
