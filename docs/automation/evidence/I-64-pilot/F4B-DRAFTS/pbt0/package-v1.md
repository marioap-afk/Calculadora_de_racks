> **Redaccion para el repo publico:** las rutas locales y las variables de entorno de carpetas del usuario se sustituyen por marcadores (`<scratchpad>`, `<worktree-de-I-64>`, `<datos-locales-del-usuario>`, `<datos-de-aplicacion-del-usuario>`). El resto no cambia.

# I-64 F4B — Paquete del contrato de PB-T0, bloque C (caracterización automática)

**Estado:** v1. La prepara la sesión principal el 2026-11-17. No está emitida y no declara GATE PASS.
- Sigue el patrón de F4-T1B v5 (`docs/initiatives/I-64-f4-t1b-gate-contract-package.md`, citado como **[T1B]**), su contrato (`I-64-pilot/F4-T1B-PILOT-CHARACTERIZATION/coordinator/gate-contract.json`, **[T1B-C]**) y su cierre (`…/task-close-package.md`, **[T1B-X]**).
- Desarrolla el §6.2 del paquete de F4B v3.1 (`I-64-push-back-completo-package-v3.md`, citado como **[P]**), incluida la fila «Efecto del redondeo» que añade la v3.1.
- **Antes de emitirse pasa por su propia revisión independiente** ([P]:601). Si esa revisión da 0 bloqueantes, 0 importantes sin resolver y ninguna decisión material nueva, se emite con attempts 1/3 sin volver al Owner ([OD]:259-268; [P] §10).

**Base:** rama `architecture/workspace-persistente-rackcad` en `5ccaa770`; `main` en `812baf87`. Todo el §2 está medido ahí, en solo lectura, sin compilar ni ejecutar.

**Mapas:** `map-a-classic-write-path.md` (**MA**, con H-061 corregido) y `map-b-editor-state-testability.md` (**MB**). Ningún hecho de este paquete se apoya solo en ellos: cada ancla que funda una decisión se comprobó en el código (§2).

**Precondiciones de emisión:**
1. La revisión independiente de este paquete, con el resultado de arriba.
2. Custodia versionada en la rama, en `docs/automation/evidence/I-64-pilot/F4B-DRAFTS/`: [P] v3.1, la transcripción de [OD], MA, MB y `f4b-dispositions.md` con F4B-E01; IC-11 en `docs/initiatives/I-64-implementation-clarifications.md` (OB-02.1). Este paquete, en `docs/initiatives/I-64-f4b-pbt0-gate-contract-package.md`. El commit de custodia es la `AuthorityRevision`.
3. `git diff 5ccaa770 <BaseSha> -- src tests` vacío. Si no lo está, se vuelve a medir el §2 antes de emitir (PBT0-C07).
4. Las celdas del §6.8 no están `STALE` (vencen el 2026-12-29) y el Snapshot de `config.toml` y el binario de Codex acreditado se comprueban al emitir (P-01).

## 1. Autoridades

| Autoridad | Qué fija |
|---|---|
| [OD] (`owner-f4b-disposition.md`) | PB-T0 «characterization / measurement only», «No product cleanup in PB-T0» (`:157-177`); emisión condicional y attempts 1/3 (`:259-268`); «Do not edit T1B characterization tests merely because F4B changes a behavior» |
| [P] §6.2 (`:557-601`) | Las filas que se fijan, su clase de prueba y la cobertura que no se duplica; el corpus (`:584-586`); el censo control → prueba (`:588`); las reglas de T1B copiadas (`:590-599`); el RED por mutantes con tabla cerrada (`:601`) |
| [P] OB-06.1, OB-06.3b, OB-06.4, OB-06.5 (`:366`, `:380`, `:383-387`, `:388`) | Pérdidas (a) y (b) en clases propias; criterio de «materially»; enumeración de los casos alcanzables de (c), (d) y clases nuevas |
| [P] §7.2 (`:675-682`) | En F4B, un rechazo de la planificación vuelve al Owner y no hay sustitutos de nc1 a nc3 |
| [P] §7.3 y D-PB-10 (`:684-706`, `:456-467`) | T1B no se edita; 1/3 por tarea; antes del intento 3, siempre al Owner |
| Proposal V5 D-15, D-16, INV-19 (`docs/initiatives/I-64-proposal-v5.md:557-593`) | Caracterización por sistema antes de extraer |
| ADR-0029 D13 (`docs/adr/0029-contrato-funcional-comun-de-ventanas-wpf.md:172-180`) | Se fija primero el comportamiento actual y no se edita para que pase |
| `docs/AUTOMATION_PLAN.md` §16.5 (`:526-548`), §16.8 (`:581-599`), §16.9 (`:601-646`), §16.11 (`:655-689`) | A1-A8; RED de la cadena; las 14 comprobaciones; paradas |
| [T1B] D-T1B-03 (`:246-374`), contrato propuesto (`:486-622`); [T1B-C] INV-F4T1B-01..16 (`:328-345`); [T1B-X] §5-§6 (`:85-136`); F4-E59 (`docs/automation/evidence/I-64-pilot/F4-DRAFTS/f4-dispositions.md:2047`) | El patrón: mutantes revertidos, tabla cerrada, prueba por mutante, anclas declaradas, contención de corridas locales |
| `docs/automation/agent-execution/model-catalog.md` (`:14`, `:48-61`, `:94-108`) | Celdas vigentes |

## 2. Hechos medidos en `5ccaa770`

Alias: **W** = `src/RackCad.UI/Systems/PushBack/RackPushBackSystemWindow.xaml.cs` (3630 líneas); **WC** = `….Composite.cs` (911); **WX** = `….xaml` (792); **CS** = `src/RackCad.Application/Systems/PushBack/PushBackCompositeEditorState.cs`; **DA** = `src/RackCad.Application/Systems/PushBack/PushBackEditorDesignAssembler.cs` (578); **WLC** = `tests/RackCad.UI.Tests/Workspace/WorkspaceDraftPilotWindowLoadCharacterizationTests.cs`.

### 2.1 La ventana: contrato, cargas y pasadas

- **H-01.** El constructor (W:128-153) carga el catálogo de la sesión (W:135-136) y termina en `LoadNew()` (W:152). No admite catálogo inyectado.
- **H-02. Contrato público** (W:247-256): `InsertRequested`, `UpdateOnly`, `RackId`, `RackName`, `InsertionRequest`, `SystemToInsert`, `DesignToInsert`.
- **H-03. Costuras internas existentes** (solo lectura para PB-T0): `Session`, `State` (W:157-159); `LastComputation`, `CurrentInputsAreValid`, `SafetySelections` (W:182-185); `RecomputePassesForTest` (W:601); `EditorStateForTest` (W:1529); `PendingHeaderConfigurationForTest` (W:2002); `PendingWork()` (W:3417); `OutputIsBlockedForTest` (W:3487); `CompositeState` (WC:43). Diálogos: `SafetyDialog` (W:1493), `RearTopeDialog` (W:1499), `DefenseDialog` (W:1505), `HeaderConfiguratorDialog` (W:1525), `BootPerPostWindowDialog` (W:2700), `SafetyWindowDialog` (W:3028). `HeaderConfiguratorPresenter` (W:2410) crea el configurador real.
- **H-04. Cargas.** `LoadExisting` (W:303-316): `SideAOnly` + `LoadCompositeFromDesign`, `Adopt(rackId, rackName)` y `LoadFromModel` (W:315). `SideAOnly` (W:334-365) devuelve el mismo objeto para un diseño no compuesto.
- **H-05. `LoadFromModel`** (W:377-427): sustituciones en W:384-392, la unidad en W:387, la escala en W:406 y el recorte de `Dimensions` a `[0, Detailed]` en W:407. `RecoverInputs` ya normaliza `PalletsDeep`, tolerancia, `BeamDepth` y escala, conserva la unidad tal cual y lleva `DimensionStyle` (`src/RackCad.Application/Systems/PushBack/PushBackEditorState.Load.cs:192-210`, `:229-232`).
- **H-06. `ReadInputs`** (W:812-857): `generalPallet` da frente, alto y peso sin controles (W:818-820); los avanzados se copian del portador (W:830-835); `Annotations` se arma sin `DimensionStyle` (W:840-848). `NumericField.SetNumber` formatea con «0.###» y la cultura actual (`src/RackCad.UI/Controls/NumericField.cs:131-136`).
- **H-07. `Recompute`** (W:603-702): `CommitCurrentCell` (W:621; W:737-741) y `compositeAssembler.BuildDesign` + `assembler.BuildFrom` dentro de un `try` (W:645-646).
- **H-08. «Actualizar»** = `RequestDraw(null, -1, true)` (W:3292). En `RequestDraw` (W:3323-3379): guardas (W:3325-3335), confirmación del módulo (W:3346-3349), **segunda pasada** (W:3351), validez y bloqueo (W:3352-3363), `SetName(NameBox.Text?.Trim())` (W:3365), `RequestUpdate` con `lastComputation.Design` (W:3369-3370) y `Close()` (W:3378). No abre ningún diálogo ni escribe.

### 2.2 Módulos, avanzados y subdiálogos

- **H-09.** «Confirmar» = `CommitModuleEdits()` + `RequestRecompute()` (W:2454-2464, las dos en W:2462-2463). «Cancelar» (W:2466-2477). «Restaurar estándar» solo pide el reinicio, sin recalcular (W:2497-2501).
- **H-10.** En DA: `standardRestore` (DA:204); `mustRebuild` (DA:211-213); sin cola en el reinicio (DA:229-231); `Reset(editorInputs)` bajo `if (standardRestore)` (DA:300-303); sin intenciones (DA:315-317). El portador `advanced` solo se escribe en W:395-400, en `ReadAdvancedRackParameters` (W:1592-1607) y en W:1628-1634; ningún camino de «Restaurar estándar» lo toca (H-PB-28, lectura: **lo mide CT-PB0-05**).
- **H-11.** El configurador: con `HeaderConfiguratorDialog` asignado no se crea ninguna ventana (W:1938-1940), y lo devuelto se escenifica en W:1946. Botón `ConfigureModuleHeaderButton` (WX:569).
- **H-12.** Seguridad (W:2534-2659): las secciones de botas, defensa y topes se construyen antes del diálogo; con `SafetyDialog` asignado no se crea `SelectiveSafetyWindow` (W:2615-2617). Al aceptar: `Authorize(chosen)` (W:2626), `ApplyDefenseSections` (W:2638), `ApplyBootSections` (W:2642) y, en un rack de un sentido, `state.LoadRearTopeConfig` (W:2653). La rejilla de botas se crea con `Owner = this` (W:2715-2721) antes de su costura (W:2722), así que exige la ventana mostrada.
- **H-13. Corpus de I64** (`tests/RackCad.Tests/Fixtures/I64/`, 17 `pb-*.json`):
  - `pb-20-ab`: compuesto, `SideB.IsPresent = true`, módulos `M1..M8`, `GAP`, `B:M8..B:M1`; `DefaultTopology = Encontradas`; `Gap = 54`;
  - `pb-21-cola`: `SideB.IsPresent = false` con los mismos ids de cola;
  - `pb-31-v2-modulos`: 3 frentes y 8 módulos;
  - `pb-06-seguridad`: `DimensionStyle = "STANDARD-ISO"`; seguridad `PROTECTOR_BOTA_H_3_16_18` y `DEFENSA_MONTACARGAS`;
  - `pb-07-combinado`: `DerivedPostHeight = 140`; `pb-05-topes`: de un sentido;
  - `pb-08-redondeo`: `AnnotationScale = 1.23456`, `Fronts[0].Levels[0].PalletHeight = 61.23456`, `Dimensions = 2`;
  - `pb-01-base`: 3 frentes; `PalletDepth 48`, `PalletTolerance 4`, `BeamDepth 6`, `FirstLevelHeight 4`; celda (0,0) con 42/60/1000 y `ClearHeight 6`; `PostPeralte` nulo.

  Ningún fixture nuevo hace falta: el compuesto con módulos y cola es `pb-20-ab`, la seguridad completa es `pb-06` y los avanzados no por defecto, `pb-07`.

### 2.3 Composición, cortes y anotaciones

- **H-14. Estado compuesto.** `SetSideBPresent` (CS:294-327; el cambio de topología por defecto en CS:316-323); `IsSlotPresent` (CS:428-437); `SetSlotPresent` → `SetActive(…, allowAllBlank: OtherSideHasAnyFront(side))` (CS:453); `SetSlotCount` (CS:522-545, `count` en CS:524). Al cargar, la cola se aparca desde `structure.Modules` con ids de cola (WC:838-842).
- **H-15. Cortes y su decisión.** `PushBackSystemLateralBuilder.Cortes` omite la frontera inexistente (`src/RackCad.Application/Systems/PushBack/PushBackSystemLateralBuilder.cs:91-114`). En un compuesto, una ranura está activa si lo está en A o en B (`PushBackCompositeStructure.cs:397-400`): **MA H-061 corregido**. RACKEDITAR decide «obsoleta» en línea: cortes y hechos de disponibilidad (`src/RackCad.Plugin/RackPushBackCommands.cs:268-271`), obsoleta si no está disponible o no hay corte con ese `PostIndex` (`:324-333`), borrado solo con supervivientes (`:348-356`). Los hechos: `PushBackViewAvailabilityFacts.Contains` (`src/RackCad.Application/Systems/Shared/RackViewAvailability.cs:106-115`, el caso de poste en `:110-111`) y `Evaluate` (`:136-146`); el códec, `RackViewCodec.Decode` (`src/RackCad.Application/Systems/Shared/RackViewCodec.cs:115`); las constantes, `RackEmbedDocument.KindPushBack` y `ViewLateral` (`src/RackCad.Application/Persistence/RackEmbedDocument.cs:20`, `:29`).
- **H-16. La computación** expone `Design`, `System`, `Bom`, `LateralPlan`, `FrontalEntradaSalida`, `FrontalPosterior`, `PlantaPlan` y `LateralCortes` (`src/RackCad.Application/Systems/PushBack/PushBackEditorComputation.cs:47-67`). `BuildFrom(design, frontalSide)` resuelve, arma el BOM y los cortes una vez (DA:466-474). `HeaderRunPlan.Flatten()` (`src/RackCad.Application/Drawing/HeaderRunPlan.cs:31`); `HeaderBlockInstance` lleva rol, pieza, bloque, vista, puntos, rotación, texto, `TextHeight`, `DimensionOffset` y `DimensionStyleName`, sin capa (`src/RackCad.Application/Drawing/HeaderBlockInstance.cs:59-101`). La capa la decide el Plugin por el rol (MA H-069).
- **H-17. Estilo y escala.** `system.DimensionStyle = design.DimensionStyle` (`src/RackCad.Application/Systems/Dynamic/DynamicRackSystemResolver.cs:246`); `DynamicViewDecorations` lo lee en `:117`, `:206` y `:285` y lo escribe en cada cota en `:402`; su escala en `:328-329`. `PushBackSideAnnotations` usa la escala en `:55` y `:79`.

### 2.4 Infraestructura de pruebas y guardas

- **H-18.** `StaTestRunner.Run` (`tests/RackCad.UI.Tests/StaTestRunner.cs:78`, `:102`) y el suelo del aviso (`:57`). `EditorDiscardPromptCollection.Name` = `"editor-discard-prompt"`, sin paralelismo (`tests/RackCad.UI.Tests/EditorDiscardPromptCollection.cs:27-30`). Ayudantes existentes, de solo uso: `ClickNamed` (`EditorWindowTestSupport.cs:22`), `SetFrontBlank` (`:67`), `SetText` (`:89`), `SetNumberAndCommit` (`:102`).
- **H-19. Guardas que los archivos nuevos deben respetar:**
  - cada archivo de prueba declara **un solo** `namespace` y es la raíz del proyecto (`tests/RackCad.Tests/NamespaceFolderGuardTests.cs:211-245`);
  - todo `.cs` de `tests/RackCad.Tests` pasa INV-14, sin `ramelements`, `python` ni `bentley` en el código (`tests/RackCad.Tests/StructuralAnalysis/StructuralAnalysisSourceGuardTests.cs:262-268`, `:416-419`, `:430-443`), y la guarda de literales partidos (`…/StructuralAnalysisRamElementsHardeningGuardTests.cs:85-92`);
  - la guarda del corpus de WLC falla con un `pb-*.json` nuevo en `Fixtures/I64` (WLC:323-332);
  - G-10 cuenta `.RequestUpdate(` en `src` (`tests/RackCad.Tests/Workspace/WorkspacePilotUpdateIntegrationGuardTests.cs:721-797`);
  - las guardas de texto de T1B sobre `Systems/PushBack` (MB H-129).
- **H-20. Cobertura existente que acota los CT** (no se duplica):
  - WLC (14 de un sentido, ventana frente al piloto) y `T19_H1_TheDimensionStyleStillComesOutNull` (`tests/RackCad.UI.Tests/PushBackDimensionViewsWindowTests.cs:309`);
  - el estilo **nombrado** en todas las vistas, con un diseño sintético y por los constructores del Plugin: `PushBackDimensionLegacyCharacterizationTests` (`tests/RackCad.Tests/PushBackDimensionLegacyCharacterizationTests.cs:35`, `:52`, `:249-251`);
  - el corte omitido en la frontera de dos frentes en blanco: `BlankFrontBoundaryTests` (`tests/RackCad.Tests/BlankFrontBoundaryTests.cs:223`, `:235`, `:310`);
  - los módulos en Core: `PushBackModuleAdoptionTests` (`:64-220`) y `PushBackCompositeModuleSequenceTests` (`:103-309`);
  - los módulos en UI: `PushBackModuleEditorWindowTests` (`:104-302`, rack nuevo) y `PushBackCompositeLoadExistingModulesTests` (compuestos construidos);
  - los subdiálogos con costura: `PushBackSafetyFlowTests`, `PushBackSafetyPerPostFlowTests`, `PushBackBootUiFlowTests`, `PushBackBootSidesUiFlowTests`, `PushBackDefensePerPostEndToEndTests`, `PushBackDefenseSideSectionsTests`, `PushBackDefenseTypePerSideTests`, `PushBackTopeSectionInSafetyTests` y `PushBackRearTopePerSideAndNoneTests`. **Ninguna llama a `LoadExisting`**, así que ninguna parte de un rack del corpus.
- **H-21.** Ningún archivo de `src` ni de `tests` contiene `PushBackFull`: el prefijo de los filtros del §6.4 selecciona hoy 0 pruebas.

## 3. Decisiones del paquete

- **D-PBT0-01. Corpus sin fixtures nuevos.** Se lee `Fixtures/I64` en solo lectura. Lo demás se construye **en código**: editando el JSON interior antes de deserializar, o con los gestos de la ventana y `LoadExisting` sobre su `LastComputation.Design` (patrón de T1B). Por tanto **no hay carpeta nueva de fixtures** y `tests/RackCad.Tests/Fixtures/` queda prohibida entera. [P] `:586` admite las dos vías.
- **D-PBT0-02. Un CT = un método `[Fact]`.** Sin `Theory` (cada fila exigiría su mutante). Partir un CT en varios métodos solo vale si el mutante de su fila detecta cada uno en aislado. Si no, es PBT0-C04.
- **D-PBT0-03. Las pérdidas (a) y (b), solo en sus clases.** Fuera de CT-PB0-18 a CT-PB0-20, toda línea de delta de clase (a) o (b) se retira antes de comparar, con la regla declarada en el §4.1. Así PB-TC sustituye esas clases sin tocar las demás (OB-06.3b).
- **D-PBT0-04. Sin guardas de texto.** Todo PB-T0 es ejecución. Las escrituras de la ventana y del Plugin ya las fija T1B (CT-22 y CT-24 a CT-27).
- **D-PBT0-05. El RED, con mutantes de comportamiento revertidos** (§5), con la mecánica de [T1B] D-T1B-03. Los mutantes de Application son necesarios porque los CT de Core no tienen otro producto que mutar.
- **D-PBT0-06. Lectura declarada de «ninguna prueba ejecuta … el BOM»** ([P]:594): se prohíben el gesto, la ventana y la exportación (`Bom_Click`, `RackBomWindow`). Leer `PushBackEditorComputation.Bom`, que es Application puro, no es «ejecutar el BOM». CT-PB0-21 lo necesita para el criterio de OB-06.4.
- **D-PBT0-07. La mitad Core de «Módulos» ya está fijada** (H-20). Se cita en el informe y no lleva CT nuevo. La de «Añadir/quitar frentes» es CT-PB0-10.

## 4. Los puntos de caracterización (lista cerrada: CT-PB0-01 a CT-PB0-21)

### 4.1 Definiciones comunes

- **`Abrir(t)`** (UI-STA): deserializa el texto del sobre `t` (`RackEmbedStore` y `RackProjectStore`), crea `new RackPushBackSystemWindow(canInsertInAutoCad: true)` y llama a `LoadExisting(project.PushBackDesign, embed.Id, embed.Name, project)`, sin mostrarla (patrón de WLC:66-82).
- **`Salida(w)`:** `RackProjectStore.Serialize(RackProject.ForPushBack(w.LastComputation.Design))` con `\r\n` → `\n` (WLC:56-57). Solo si `CurrentInputsAreValid` y `LastComputation.IsValid`; si no, el valor es `NO-VÁLIDA: <texto de StatusText>`.
- **`Sobre(t)`:** lo mismo sobre el diseño interior **deserializado otra vez** desde `t`, nunca sobre el objeto que recibió la ventana.
- **`Delta(x, y)`:** las líneas `ruta: x -> y`, en orden ordinal, con el algoritmo de WLC:95-181, copiado en el ayudante nuevo y nunca referenciado.
- **Clase de una línea:**
  - (a) es la regla numérica de WLC:213-229;
  - (b) es la ruta `$.PushBack.Structure.DimensionStyle`;
  - (c), (d) y `OTRA:<descripción>` se fijan como etiqueta en los datos esperados, con el `archivo:línea` del producto que la causa, medido en la base.
- **Filtro (D-PBT0-03):** fuera de CT-PB0-18 a CT-PB0-20 se retiran las líneas de clase (a) y (b). La entrega las lista.
- **Comparación:** igualdad ordinal exacta de la lista fijada `(línea, clase)` o de la tupla fijada. Los valores se miden en la base y se fijan tal cual.
- **Determinismo:** cultura invariante en el hilo STA, fijada y restaurada en `finally`. En UI, los números se escriben con «R». En Core solo se fijan claves, enteros, cadenas y booleanos, porque Core corre en Ubuntu en la CI.
- **Ventanas:** sin mostrar, salvo CT-PB0-14 (fuera de pantalla, `ShowActivated = false`). Toda ventana que la petición no cierre se cierra en `finally`. Todas las clases de UI van en `EditorDiscardPromptCollection`.
- **Delegados:** ninguna aserción dentro de una costura. Se captura dentro y se asevera fuera.
- **Precondiciones:** toda precondición es una aserción. Por ejemplo, `UpdateButton.IsEnabled` antes de pulsarlo, o que la costura fue llamada.

### 4.2 Los CT

**CT-PB0-01. Propiedad del estado: la carga frente al sobre.** UI · `PushBackFullEditorCharacterizationStateTests`.
- **Diseños:** `pb-20-ab`, `pb-21-cola`, `pb-31-v2-modulos` y **X-01**. X-01 se construye así: `Abrir(pb-21-cola)`, se declara B (`SideBPresentCheck`), lado B en `SideSelectorBox`, `SetFrontBlank` de la ranura 0 de B en `false` y su `LastComputation.Design` en otra ventana con `LoadExisting`.
- **Fija:** para cada diseño, `Delta(Sobre, Salida)` filtrado, como lista `(línea, clase)`, y `IsComposite` de la salida.
- **No duplica:** WLC, que cubre los 14 de un sentido frente al piloto, y H-033 de MB.

**CT-PB0-02. Propiedad del estado: la segunda pasada (Actualizar) frente a la primera** (H-PB-25). UI · `…StateTests`.
- **Diseños:** los 17 de I64 y X-01.
- **Pasos:** `S1 = Salida(Abrir)` y después `ClickNamed(UpdateButton)`.
- **Fija por diseño:**
  - el resultado: `PETICIÓN`, `BLOQUEADO: <StatusText>` o `INVÁLIDO: <StatusText>`;
  - con petición: `UpdateOnly == true`, `InsertionRequest.Views.Count == 0` y `Delta(S1, Persist(DesignToInsert))` filtrado.
- **No duplica:** ninguna prueba compara el diseño entero de la petición (MB H-036); T1B CT-06 solo fija el cierre.

**CT-PB0-03. Módulos: escenificar, confirmar y cancelar en un rack cargado.** UI · `PushBackFullEditorCharacterizationModuleTests`.
- **Diseños:** `pb-31-v2-modulos` y `pb-20-ab`.
- **Pasos:**
  1. primer módulo con longitud (`EditorStateForTest.ModuleSession`) en `ModuleBox`; `ModuleLengthBox` + 8 y `LostFocus`;
  2. «Confirmar»;
  3. otro módulo + 4 y «Cancelar»;
  4. `DepthBox` + 6, con `SetNumberAndCommit`, que fuerza la reconstrucción.
- **Fija:**
  - tras 1: `Salida` igual a la previa y `PendingWork()` con trabajo;
  - tras 2: `Delta` previo → confirmado y `PendingWork()` vacío;
  - tras 3: `Salida` igual a la confirmada;
  - tras 4: la secuencia `(ModuleId, Kind, Length, IsCalculated, IsManualOverride)` de `Structure.Modules` y el resumen público de `EditorStateForTest.LastModuleReconciliation`, que muestra la reconciliación por `ModuleId`+`Kind`.
- **No duplica:** las pruebas de módulos de H-20, que trabajan sobre un rack nuevo o construido.

**CT-PB0-04. Añadir y quitar módulos por reconstrucción.** UI · `…ModuleTests`.
- **Diseño:** `pb-31-v2-modulos`.
- **Pasos:**
  1. `DepthBox` + 12;
  2. `PalletsDeepBox` + 1;
  3. un módulo personalizado y confirmado;
  4. `RestoreAllModulesButton` y «Confirmar».
- **Fija:** tras la carga y tras cada paso, la secuencia de módulos de CT-PB0-03.
- **No duplica:** `PushBackModuleEditorWindowTests:280-302` (solo `IsManualOverride`) y `PushBackModuleAdoptionTests:175`.

**CT-PB0-05. Los avanzados tras «Restaurar estándar»** (H-PB-28). UI · `…ModuleTests`.
- **Diseño:** `pb-07-combinado`.
- **Fija** los seis campos avanzados (W:395-400) en tres momentos:
  - A0: la salida de la carga y el texto o estado de las seis cajas (W:1599-1606);
  - A1: tras `RestoreAllModulesButton` + «Confirmar», la misma lectura;
  - A2: tras Actualizar, los de `DesignToInsert`.
- **Incumplimiento:** si A2 no conserva el reinicio, el método lleva el comentario «INCUMPLIMIENTO CARACTERIZADO» con la regla de DA:296-299. No se corrige.
- **No duplica:** `PushBackModuleEditorWindowTests:280-302` y `PushBackAdvancedRackParametersTests`, que prueba la función, no la ventana.

**CT-PB0-06. Cola aparcada.** UI · `PushBackFullEditorCharacterizationCompositeTests`.
- **Diseño:** `pb-21-cola`.
- **Pasos:** carga → declarar B → activar la ranura 0 de B → retirar B.
- **Fija en cada paso:**
  - los ids de `CompositeState.DormantCompositeTail` y `SideBPresent`;
  - de la salida: `IsComposite` y los ids de `Structure.Modules`.
- **No duplica:** `PushBackDormantTailTransactionTests`, `PushBackDormantRestoreTests` y `PushBackCompositeLifecycleWiringTests`, que no cargan el corpus (MB H-034).

**CT-PB0-07. A/B.** UI · `…CompositeTests`.
- **Diseño:** `pb-20-ab`.
- **Pasos:** carga → retirar B → declarar B otra vez → todas las ranuras de B en blanco (lado B y `SetFrontBlank` en cada una).
- **Fija en cada paso:**
  - de la salida: `IsComposite`, presencia de `SideB`, `Composite.AbsentSlotsA` y `AbsentSlotsB` y `Composite.DefaultTopology`;
  - `SideBPresentCheck.IsChecked` y los ids aparcados.
- **No duplica:** `PushBackBlankFrontAuthorityTests` y `PushBackCompositeWindowTests:82-138`, `:610-680` y `:1366-1418`, que no cargan `pb-20-ab`.

**CT-PB0-08. Frentes sin B.** UI · `…CompositeTests`.
- **Diseño:** `pb-01-base`.
- **Pasos:** `AddFrontButton` → `RemoveFrontButton` → frentes 1 y 2 en blanco → frente 2 activo otra vez.
- **Fija en cada paso:**
  - el número de `Structure.Fronts` y `IsActive` de cada uno;
  - `Composite.AbsentSlotsA` y `AbsentSlotsB`;
  - la lista de `LastComputation.LateralCortes[*].PostIndex`.
- **No duplica:** `PushBackViewTests:152`, de un solo diseño, y `BlankFrontBoundaryTests:223`, Core y sin la ventana.

**CT-PB0-09. Frentes con B.** UI · `…CompositeTests`.
- **Diseño:** `pb-01-base`.
- **Pasos:** declarar B → `AddFrontButton` con A activo → lado B y la ranura 1 de B activa → ranuras 1 y 2 de A en blanco → ranura 1 de B en blanco → retirar B.
- **Fija:** lo de CT-PB0-08 más `IsComposite`.
- **No duplica:** `PushBackCompositeWindowTests:1121`, rack nuevo y sin cortes.

**CT-PB0-10. Borrado de vistas: la decisión pura.** Core · `PushBackFullEditorCharacterizationLateralCutTests`.
- **Punto de partida:** `PushBackPilotMaterializer.Materialize(pb-01-base)`, admitido.
- **Ediciones** sobre `State.Editor`:
  - de un sentido: `SetSlotCount(n+1)`, `SetSlotCount(n)`, y `SetSlotPresent(A,1,false)` + `SetSlotPresent(A,2,false)`;
  - compuesta, sobre la última: `SetSideBPresent(true)` → `SetSlotPresent(B,1,true)` → `SetSlotPresent(B,1,false)` → `SetSideBPresent(false)`.
- **Cálculo en cada estado:** diseño = `new PushBackCompositeEditorAssembler(catalog).BuildDesign(editor, inputs)`; sistema = `new PushBackResolver(catalog).Resolve(diseño)`; cortes = `new PushBackSystemLateralBuilder().Cortes(sistema, catalog)`, el mismo cálculo que `RackPushBackCommands.cs:269`.
- **Fija por par consecutivo (antes, después):**
  - las dos listas de `PostIndex`;
  - por cada `p` de «antes», el `Status` de `RackViewAvailability.Evaluate(RackViewCodec.Decode(KindPushBack, ViewLateral, p), new PushBackViewAvailabilityFacts(cortes(después), después.IsComposite))`;
  - la lista de obsoletas: `Status ≠ Available` o ningún corte con ese `PostIndex`, como `RackPushBackCommands.cs:324-329`.
- **Catálogo:** `JsonRackCatalogProvider.FromBaseDirectory().Load()`, como `PushBackDimensionLegacyCharacterizationTests:44`.
- **No duplica:** las guardas de texto de `EditPushBack` (MA H-121) y `BlankFrontBoundaryTests:223`, que solo fija la omisión del corte, no la decisión por pares ni el compuesto.

**CT-PB0-11. Renombrar.** UI · `…StateTests`.
- **Diseño:** `pb-01-base`.
- **Casos**, cada uno en su ventana: `"  Renombrado PB0  "`, `""` y `"   "`. Para cada uno: `SetText(NameBox, …)` y después `ClickNamed(UpdateButton)`.
- **Fija:**
  - `InsertRequested`, `RackName` e `InsertionRequest.RackName`;
  - `RackId`, que es igual al `Id` del sobre;
  - `SystemToInsert.Name`, tal como sale.
- **No duplica:** T1B CT-05, que fija el recorte al cerrar sin petición, ni CT-06.

**CT-PB0-12. El configurador, por costura.** UI · `PushBackFullEditorCharacterizationSubdialogTests`.
- **Diseño:** `pb-31-v2-modulos`.
- **Costura:** `HeaderConfiguratorDialog` captura su argumento y devuelve un **clon** con un cambio declarado, sin mutar el argumento. `HeaderConfiguratorPresenter` no se asigna nunca.
- **Pasos:** módulo con cabecera en `ModuleBox` → `ConfigureModuleHeaderButton` → «Confirmar». Después, otra vez con una costura que devuelve `null`.
- **Fija:**
  - que la costura se llamó;
  - `PendingHeaderConfigurationForTest` no nulo antes de confirmar;
  - el `Delta` de la salida, antes y después;
  - con `null`, nada escenificado y la salida sin cambio.
- **No duplica:** `PushBackModuleEditorWindowTests:167-245`, de rack nuevo y sin el delta de salida.

**CT-PB0-13. Seguridad aceptada.** UI · `…SubdialogTests`.
- **Diseño:** `pb-06-seguridad`.
- **Costura:** `SafetyDialog` devuelve la lista recibida sin `DEFENSA_MONTACARGAS`, la quita declarada.
- **Fija:** el `Delta` de la salida y `SafetySelections.Count`.
- **No duplica:** `PushBackSafetyFlowTests` (rack nuevo) ni T1B CT-17 (cancelar).

**CT-PB0-14. Botas por poste.** UI · `…SubdialogTests`, con la ventana **mostrada**: la rejilla lleva `Owner = this` (H-12).
- **Diseño:** `pb-06-seguridad`.
- **Costuras:** dentro de `SafetyDialog`, la configuración por poste de `BootSectionForTest`; `BootPerPostWindowDialog` fija el poste 0 en una colocación declarada y devuelve `true`; `SafetyDialog` devuelve la lista sin cambios.
- **Fija:** el `Delta` de la salida.
- **No duplica:** `PushBackBootUiFlowTests`, `PushBackBootSidesUiFlowTests` y `PushBackSafetyPerPostFlowTests`.

**CT-PB0-15. Defensa por poste.** UI · `…SubdialogTests`.
- **Diseño:** `pb-06-seguridad`.
- **Costuras:** dentro de `SafetyDialog`, la configuración por poste de `DefenseSectionForTest`; `DefenseDialog` devuelve registros declarados para el poste 0.
- **Fija:** que `DefenseDialog` se llamó y el `Delta` de la salida.
- **No duplica:** las tres pruebas de defensa de H-20.

**CT-PB0-16. Topes traseros.** UI · `…SubdialogTests`.
- **Diseño:** `pb-05-topes`, de un sentido, porque ejerce W:2653.
- **Costuras:** dentro de `SafetyDialog`, la configuración de `RearTopeSectionForTest`; `RearTopeDialog` devuelve un `TopeResult` declarado, con la celda (0,0) apagada.
- **Fija:** que la costura se llamó y el `Delta` de la salida.
- **No duplica:** `PushBackTopeSectionInSafetyTests` ni `PushBackRearTopePerSideAndNoneTests`.

**CT-PB0-17. Dependencias de catálogo y sustituciones (c).** UI · `PushBackFullEditorCharacterizationCatalogTests`.
- **Base:** `pb-01-base`. Cada caso edita el JSON interior antes de deserializar, y la misma edición da el `Sobre`.
- **Casos:**
  - c-01: `HeaderPostCatalogId = ""`;
  - c-02: `HeaderPostCatalogId = "PB0_POSTE_INEXISTENTE"`;
  - c-03: `PushBack.Fronts[0].HighEndBeamPeraltes[0] = 7.25`;
  - c-04: `Structure.Fronts[0].Levels[0].InOutBeamCatalogId = "PB0_VIGA_INEXISTENTE"`;
  - c-05: `…Levels[0].InOutBeamDepth` = máximo de `DynamicRackLevelGeometry.AllowedPeraltes(catalog, beamId)` + 0,5 (`DynamicRackLevelGeometry.cs:172-174`);
  - c-06: `…Levels[0].IntermediateBeamCatalogId = "PB0_VIGA_INEXISTENTE"`;
  - c-07: el caso c-05 en `Fronts[1].Levels[0]`, una celda no seleccionada, para la prueba de (d);
  - c-08 y c-09: `PalletWeightUnit` igual a `""` y a `"ton"`;
  - c-10 y c-11: `Dimensions` igual a 7 y a −1;
  - c-12 a c-15: `PalletsDeep = 1`, `PalletTolerance = 0`, `BeamDepth = 0` y `AnnotationScale = 0`, que normaliza `RecoverInputs`.
- **Fija por caso:** `Delta(Sobre, Salida)` filtrado, con la clase de cada línea: (c), (d), `CARGA-APPLICATION` u `OTRA:…`.
- **No duplica:** WLC, sin delta (c) en su corpus, ni `pb-12-poste-nulo`.

**CT-PB0-18. Pérdida (b): el `DimensionStyleName` del plan.** Core · `PushBackFullEditorCharacterizationDimensionStyleLossTests` (clase propia, OB-06.3b).
- **Diseños:** los interiores de `pb-06` y `pb-07` tal cual, y los mismos con `DimensionStyle = null`.
- **Cálculo:** `BuildFrom(D, A)`; cotas = las instancias con `Role == Dimension` de `PlantaPlan`, `FrontalEntradaSalida`, `FrontalPosterior`, `LateralPlan` y cada plan de `LateralCortes` (`Flatten().Instances`).
- **Fija:**
  - `IsValid`;
  - `System.Structure.DimensionStyle`;
  - el número de cotas por plan, igual con y sin estilo y mayor que 0;
  - el conjunto de `DimensionStyleName`: `{"STANDARD-ISO"}`, `{"SIMPLEX"}` y `{null}`.
- **No duplica:** el estilo nombrado sintético por el camino del Plugin, que ya fija `PushBackDimensionLegacyCharacterizationTests:249-251`; lo nuevo es el **nulo** y los nombres guardados del corpus por `BuildFrom`. Tampoco duplica `T19_H1` ni WLC (pb-06 y pb-07), que fijan la pérdida en la ventana.

**Tabla de campos de MB H-077**, que usan CT-PB0-19 y CT-PB0-21. Cada campo con su valor base, y el valor exacto = base + 0,00044:

| Fila | Campo (JSON interior) | Base | Control |
|---|---|---|---|
| F1 | `Structure.PalletDepth` | 48 | `DepthBox` |
| F2 | `Structure.PostPeralte` | 3 (nulo en el fixture) | `PostPeralteBox` |
| F3 | `Structure.PalletTolerance` | 4 | `ToleranceBox` |
| F4 | `Structure.BeamDepth` | 6 | `BeamDepthBox` |
| F5 | `Structure.AnnotationScale` | `pb-01`: 1,5; `pb-08`: su 1,23456 tal cual | `AnnotationScaleBox` |
| F6 | `Structure.Fronts[0].FirstLevelHeight` | 4 | `FirstLevelHeightBox` |
| F7 a F10 | `Structure.Fronts[0].Levels[0].{PalletFront, PalletHeight, PalletWeight, ClearHeight}` | 42, 60 (`pb-08`: 61,23456 tal cual), 1000 y 6 | `CellPallet*Box` y `CellClearBox` |
| F11 | `Structure.Fronts[0].Levels[0].BeamLengthOverride` | 100 (nulo en el fixture) | `CellBeamLengthOverrideBox` |

Una fila cuyo diseño no resuelve se fija como `NO-VÁLIDA` tal cual.

**CT-PB0-19. Pérdida (a): los campos que redondean.** UI · `PushBackFullEditorCharacterizationRoundingLossTests` (clase propia).
- **Base:** `pb-01-base`, una variante por fila F1 a F11.
- **Fija por fila:** el valor exacto escrito, el valor del campo en `Salida` y su clase: `REDONDEA`, `CONSERVA` u `OTRA`.
- **No duplica:** WLC pb-08, que fija dos campos sobre el piloto.

**CT-PB0-20. Pérdida (a): los campos que no redondean** (MB H-079). UI · `…RoundingLossTests`.
- **Filas** (valor exacto = base + 0,00044):
  - G1 a G3: `Structure.PalletFront`, `PalletHeight` y `PalletWeight` del rack, con `pb-01`;
  - G4 a G6: `DerivedPostHeight` (base 140), `ManualHeaderHeightOverride` (base 150) y `SeparatorSpacingOverride` (base 30), con `pb-01`;
  - G7: `Composite.Gap` (54), con `pb-20-ab`;
  - G8: la `Length` del primer módulo con longitud, con `IsManualOverride = true`, con `pb-31`;
  - G9: `Fronts[1].Levels[0].PalletHeight` (60), con `pb-01`.
- **Fija:** como CT-PB0-19.
- **No duplica:** nada; ninguna prueba lo fija.

**CT-PB0-21. Efecto del redondeo, para OB-06.4.** Core · `PushBackFullEditorCharacterizationRoundingEffectTests` (clase propia).
- **Filas:**
  - R0: `pb-08` tal cual (E) frente a `pb-08` con sus dos campos redondeados a «0.###» (R);
  - R1 a R11: sobre `pb-08`, la fila Fn de la tabla H-077 con el valor exacto (E) y con ese valor redondeado (R).
- **Redondeo:** `double.Parse(v.ToString("0.###", CultureInfo.InvariantCulture), CultureInfo.InvariantCulture)`.
- **Cálculo:** `BuildFrom(D, A)` para E y para R. Las claves de plan son `planta`, `frontal-es`, `frontal-post`, `lateral` y `corte-<PostIndex>`.
- **Fija por fila** (el criterio de OB-06.4):
  - **K1 (piezas):** las claves de plan donde difiere el multiconjunto `(Role, PieceId, BlockName, View)`. La capa va en el rol (H-16);
  - **K2 (niveles o posiciones):** si difieren, por frente del sistema resuelto, el número de niveles, el de posiciones o la lista de `PostIndex`;
  - **K3 (BOM):** si difieren las líneas de `Bom`, con todas sus propiedades públicas y en el orden producido;
  - **K4:** las claves de plan, con K1 igual, donde algún punto (`Insertion`, `ConnectionAnchor`) de la instancia homóloga cambia más de 0,001;
  - **K4′:** lo mismo para cualquier cambio de coordenada, mayor que 0;
  - **K5:** las claves con cualquier cambio de `TextHeight`, `DimensionOffset` o `RotationRadians`;
  - **material** = K1 no vacía, o K2, o K3, o K4 no vacía.
- **Informe:** la entrega da, por fila, el máximo |Δ| medido. **No juzga**: la decisión es del Owner antes de PB-TC (OB-06.4).
- **No duplica:** nada (fila nueva de la v3.1).

### 4.3 Filas del §6.2 sin CT nuevo

- **Creación de vistas:** T1B CT-06 a CT-09 y CT-23.
- **Actualizar frente a cerrar:** T-CL y T-IN.
- **Escrituras al volver la ventana:** T-PG y MA H-120 a H-123, con el efecto real en el bloque M.
- **La mitad Core de «Módulos»:** D-PBT0-07.

Las cuatro se citan en el informe.

### 4.4 Trazabilidad de [P] §6.2

| Fila | CT |
|---|---|
| Propiedad del estado | 01, 02 |
| Módulos | 03 (y Core citado) |
| Cola aparcada | 06 |
| A/B | 07 |
| Añadir/quitar frentes | 08, 09, 10 |
| Añadir/quitar módulos | 04, 05 |
| Renombrar | 11 |
| Borrado de vistas | 10 |
| Subdiálogos | 12 a 16 |
| Dependencias de catálogo | 17 |
| Pérdida del estilo de cotas | 18 |
| Pérdida por redondeo | 19, 20 |
| Efecto del redondeo | 21 |

### 4.5 Tablas que el Worker rellena en su entrega (`Evidence`)

- **T-1. Censo control → prueba** (MB H-029 a H-031). Cada control, con la prueba (CT, T1B u otra existente, con su `archivo:línea`) que fija su efecto en la salida, o `HUECO`.
- **T-2. Casos alcanzables** de (c), (d), `CARGA-APPLICATION` y `OTRA`, con el catálogo entregado (OB-06.5). Por caso:
  - cómo se construye;
  - la ruta y el antes → después;
  - la clase y el `archivo:línea` causante;
  - si se alcanza por la interfaz o solo por documento;
  - el CT que lo fija.
- **T-3. Efecto del redondeo:** por fila de CT-PB0-21, K1 a K5, el máximo |Δ| y «material», sí o no.
- **T-4. H-PB-28:** A0, A1 y A2.
- **T-5. Cobertura cruzada:** por fila del §6.2, las pruebas existentes citadas (H-20 y lo que el Worker encuentre).
- **T-6. Anclas:** todas las aserciones sin mutante propio, con el método que sí lo tiene ([T1B-X] §5).
- **T-7. Costuras internas usadas** (H-03), para la red de PB-T1.

## 5. El RED: tabla CERRADA de mutantes

**`MutantScope`.** Siete archivos. Todos los mutantes son **de comportamiento**, y no hay mutantes textuales porque no hay guardas de fuente:

| Archivo | Mutantes |
|---|---|
| `src/RackCad.UI/Systems/PushBack/RackPushBackSystemWindow.xaml.cs` | 01, 02, 03, 05 alt., 10 a 16, 18, 19 |
| `src/RackCad.UI/Systems/PushBack/RackPushBackSystemWindow.Composite.cs` | 06 |
| `src/RackCad.Application/Systems/PushBack/PushBackCompositeEditorState.cs` | 07, 08 |
| `src/RackCad.Application/Systems/PushBack/PushBackEditorDesignAssembler.cs` | 04, 05 |
| `src/RackCad.Application/Systems/Shared/RackViewAvailability.cs` | 09 |
| `src/RackCad.Application/Systems/Dynamic/DynamicViewDecorations.cs` | 17, 20 |
| `src/RackCad.Application/Systems/PushBack/PushBackSideAnnotations.cs` | 20 alt. |

**La tabla.** «Debe fallar» = el método del CT, con **ese mutante solo** aplicado sobre `BaseSha`. «alt.» es la única alternativa admitida, y solo si la principal no discrimina o falla por excepción.

| MU | Archivo:línea | Cambio exacto | Debe fallar |
|---|---|---|---|
| MU-PB0-01 | W:315 | Antes de `LoadFromModel(inputs, rackName);` de `LoadExisting`: `if (design.IsComposite) inputs.PalletTolerance += 1.0;` | CT-01 |
| MU-PB0-02 | W:3365 | Antes de `session.Identity.SetName(NameBox.Text?.Trim());` de `RequestDraw`: `lastComputation.Design.Structure.PalletTolerance += 1.0;` | CT-02 |
| MU-PB0-03 | W:2463 | Se quita `RequestRecompute();` de `ConfirmModule_Click` | CT-03 |
| MU-PB0-04 | DA:211-213 | `var mustRebuild = standardRestore \|\| restoredIds.Count > 0 \|\| …` → sin el término `standardRestore \|\|` | CT-04 |
| MU-PB0-05 | DA:300 | `if (standardRestore)` → `if (standardRestore && editorInputs == null)`. **Alt.:** W:2499, antes de `RequestStandardRestore();`: `advanced.DerivedPostHeight = null;` | CT-05 |
| MU-PB0-06 | WC:839-840 | El primer argumento de `ParkDormantTail` (`structure.Modules.Where(…)`) → `structure.Modules.Take(0)` | CT-06 |
| MU-PB0-07 | CS:453 | `allowAllBlank: OtherSideHasAnyFront(side)` → `allowAllBlank: false`. **Alt.:** CS:320-323, se quita la rama `else if (!present && DefaultTopology == …Encontradas)` | CT-07 |
| MU-PB0-08 | CS:524 | `Math.Max(1, requested)` → `Math.Max(1, requested > SideA.Structure.Count ? requested + 1 : requested)` | CT-08, CT-09 |
| MU-PB0-09 | `RackViewAvailability.cs:111` | `return posts.Contains(address.Variant.Index);` → `return posts.Contains(address.Variant.Index) \|\| address.Variant.Index >= 0;` | CT-10 |
| MU-PB0-10 | W:3365 | `SetName(NameBox.Text?.Trim())` → `SetName(NameBox.Text)` | CT-11 |
| MU-PB0-11 | W:1946 | `StageHeaderConfiguration(module, edited);` → `StageHeaderConfiguration(module, copy);` | CT-12 |
| MU-PB0-12 | W:2626 | `Authorize(chosen)` → `Authorize(safetySelections)` | CT-13 |
| MU-PB0-13 | W:2642 | Se quita `ApplyBootSections(bootSectionA, bootSectionB);` | CT-14 |
| MU-PB0-14 | W:2638 | Se quita `ApplyDefenseSections(defenseSectionA, defenseSectionB, defenseCarrierId);`. El inicializador de `defenseCarrierId` no es constante, así que no da CS0219 | CT-15 |
| MU-PB0-15 | W:2653 | Se quita `state.LoadRearTopeConfig(topeSection.Config);` y queda `else { }` | CT-16 |
| MU-PB0-16 | W:407 | `Math.Min((int)DimensionDetail.Detailed, …)` → `Math.Min((int)DimensionDetail.Standard, …)`. **Alt.:** W:387, `? "kg" :` → `? "lb" :` | CT-17 |
| MU-PB0-17 | `DynamicViewDecorations.cs:402` | `DimensionStyleName = style` → `DimensionStyleName = style?.ToLowerInvariant()` | CT-18 |
| MU-PB0-18 | W:406 | `AnnotationScaleBox.SetNumber(…)` → con segundo argumento `"0.######"` | CT-19 |
| MU-PB0-19 | W:819 | `generalPallet.Height` (tercer argumento de `PalletSpecification`) → `Math.Round(generalPallet.Height, 3)` | CT-20 |
| MU-PB0-20 | `DynamicViewDecorations.cs:329` | `? system.AnnotationScale :` → `? Math.Round(system.AnnotationScale, 3) :`. **Alt.:** `PushBackSideAnnotations.cs:55`, el mismo cambio | CT-21 |

**Reglas de los mutantes** (adaptan [T1B] D-T1B-03 (b) y (c)):
- **Ningún mutante:**
  - toca una asignación de `Owner`, un `ShowDialog`, un `MessageBox` ni `EditorDiscardPrompt`;
  - toca la comprobación de nulo de una costura (W:1938, :2615, :2722, :2849, :2981, :3018);
  - toca las guardas de `RequestDraw` (W:3325-3363) ni el cuerpo del constructor (W:128-153);
  - añade `.RequestUpdate(` (G-10) ni ninguno de los literales que vigila T-SG;
  - provoca una escritura;
  - cambia un camino de cierre de modo que quede un modal abierto.
- **Compilación:** compilan sin avisos nuevos: ni CS0162, CS0168, CS0169, CS0219, CS0414 ni CS8321, ni de las clases CS, MSB, NU y NETSDK. Los siete archivos ya importan `System` y `System.Linq`.
- **Un mutante que, solo o en la conjunción, hace fallar una prueba por excepción y no por aserción**, no vale. Se usa su alternativa. Si no la tiene o tampoco vale: **PBT0-C10**.
- **La tabla es exhaustiva.** Un mutante fuera de ella, o una variante no enumerada, es PBT0-C04. El Worker no inventa mutantes.
- **La conjunción:** con todos los mutantes aplicados, todos los métodos nuevos fallan por aserción (INV-PBT0-02). El Worker comprueba que ningún mutante anula a otro.
- **Pruebas existentes rotas:** se admiten **solo** por los mutantes. Se miden como unión y se atribuyen por la corrida aislada de cada uno. Se espera una unión mayor que la de T1B, porque hay mutantes en Application (R-02).
- **Correcciones:**
  - toda corrección que exija RED vuelve a aplicar todos los mutantes de los filtros con `ExpectRed`;
  - Core pasa a `ExpectRed` false solo si no se vuelve a aplicar ningún mutante de Application, y se declara;
  - las comprobaciones de producto se anclan a `ChainBaseSha`.

## 6. Contrato propuesto (a fijar en la emisión)

### 6.1 Identidad

- `Schema` `rackcad-gate-contract/v1`; `Unit` I-64; `Gate` F4B; `TaskId` `F4B-PBT0-CHARACTERIZATION`.
- Evidencia en `docs/automation/evidence/I-64-pilot/F4B-PBT0-CHARACTERIZATION/`.
- Attempts 1 de 3 (D-PB-10); `RoutingEnforcement` `required`; `CorrectionsAuthorized` true.
- **Objective:** «Caracterizar con pruebas nuevas el comportamiento observable ACTUAL del estado editable de la ventana clásica de Push Back y de las funciones Application de las que dependen PB-T1, PB-T2 y PB-T3, según el §6.2 del paquete de F4B v3.1 y la lista cerrada CT-PB0-01..21. El RED lleva solo los mutantes de la tabla cerrada MU-PB0-01..20 y el GREEN los revierte, con los bytes del producto idénticos a la base. No se corrige nada ni se ejecuta AutoCAD.»

### 6.2 `AllowedWriteScope` (17 rutas exactas)

- **UI, siete:** `tests/RackCad.UI.Tests/PushBackFullEditorCharacterization{State,Module,Composite,Subdialog,Catalog,RoundingLoss}Tests.cs` y el ayudante `tests/RackCad.UI.Tests/PushBackFullEditorCharacterizationSupport.cs`. Es `internal static`, sin pruebas, con el diff de WLC copiado, `Abrir`, `Salida`, `Sobre` y la cultura.
- **Core, tres:** `tests/RackCad.Tests/PushBackFullEditorCharacterization{LateralCut,DimensionStyleLoss,RoundingEffect}Tests.cs`. Sin ayudante compartido: lo suyo es privado de cada clase.
- **Los siete archivos de `MutantScope`**, marcados «solo mutante temporal, neto cero».

### 6.3 `ForbiddenWriteScope`

- **Producto.** Lo genera el script del contrato con el método de `make_t1b_contract.py`: por cada entrada de `src/` en `BaseSha` (`git ls-tree`), se emite el prefijo `<dir>/` **solo si ninguna ruta permitida queda debajo**; si queda alguna, se desciende. Los archivos se emiten uno a uno. El script comprueba dos cosas: que ninguna ruta permitida queda bajo una prohibida y que toda ruta de `src/` no permitida queda cubierta. La forma esperada:
  - `src/RackCad.Domain/` y `src/RackCad.Plugin/` enteros;
  - en `src/RackCad.Application/`, todo menos `Systems/`; dentro, `Cantilever/`, `FlowBed/`, `Larguero/` y `Selective/`, y en `Dynamic/`, `PushBack/` (con `Pilot/` como prefijo) y `Shared/`, cada archivo salvo los permitidos;
  - en `src/RackCad.UI/`, todo menos `Systems/PushBack/`, y ahí los ocho archivos no permitidos.
- **Pruebas:**
  - `tests/RackCad.Tests/Fixtures/`, `tests/RackCad.Tests/Workspace/`, `tests/RackCad.Tests/StructuralAnalysis/` y `tests/RackCad.UI.Tests/Workspace/`, como prefijos;
  - los dos `.csproj`;
  - `StaTestRunner.cs`, `EditorWindowTestSupport.cs` y `EditorDiscardPromptCollection.cs`;
  - las siete clases de T1B (`tests/RackCad.UI.Tests/PushBackClassicCharacterization{Keyboard,Close,Intent,Owner,Resource,SourceGuard}Tests.cs` y `tests/RackCad.Tests/PushBackClassicCharacterizationPluginGuardTests.cs`);
  - `NamespaceFolderGuardTests.cs`, `PushBackDimensionLegacyCharacterizationTests.cs`, `BlankFrontBoundaryTests.cs`, `PushBackModuleEditorWindowTests.cs` y `PushBackDimensionViewsWindowTests.cs`;
  - los archivos de [T1B-C] `:301-313`.
- **Raíz:** `docs/`, `.github/`, `eng/`, `tools/`, `assets/`, `deploy/`, `RackCad.sln`, `Directory.Build.props`, `Directory.Build.targets`, `global.json`, `NuGet.Config`, `README.md`, `AGENTS.md`, `CLAUDE.md`, `.editorconfig`, `.gitattributes` y `.gitignore`.

### 6.4 `RequiredTests`

| Proyecto | Filtro | `MinSelected` | `ExpectRed` |
|---|---|---|---|
| `tests/RackCad.Tests/RackCad.Tests.csproj` | `FullyQualifiedName~RackCad.Tests.PushBackFullEditorCharacterization` | 3 (CT-10, 18 y 21) | true |
| `tests/RackCad.UI.Tests/RackCad.UI.Tests.csproj` | `FullyQualifiedName~RackCad.UI.Tests.PushBackFullEditorCharacterization` | 18 (los demás) | true |

Hoy los dos filtros seleccionan 0 (H-21). El ayudante no tiene pruebas y no cuenta.

### 6.5 Invariantes

- **INV-PBT0-01. Sin cambio de producto.** Anclado a `ChainBaseSha`:
  - `git diff ChainBaseSha..CurrentSha -- src` vacío;
  - `--name-only` = exactamente los diez archivos nuevos de prueba;
  - `git diff --name-only BaseSha..CurrentSha` dentro de las 17 rutas;
  - por cada archivo de `MutantScope` que el RED tocó, los bytes en `CurrentSha` son los de `ChainBaseSha`. Cualquier residuo es FAIL.
- **INV-PBT0-02. RED por mutantes revertidos.**
  - El mensaje del RED es «RED con mutantes deliberados, revertidos en el GREEN», y ese SHA no sirve para ningún smoke ni para el bloque M.
  - **Criterio mecánico**, por resultado del TRX y con todos los mutantes a la vez, en los TRX del RED de cada filtro:
    - todos con `outcome = Failed`;
    - un `<Message>` sin el prefijo de tipo `^(?:[A-Za-z_][\w`]*[.+])+[A-Za-z_][\w`]* : `;
    - la primera línea de `<StackTrace>` bajo `tests/`;
    - ninguno `NotExecuted` ni `Skipped`;
    - `Failed == Selected ≥ MinSelected`.
  - El GREEN es el inverso exacto y no toca `tests/`.
  - **Para el Controller:** las escrituras del RED en `MutantScope` revertidas por el GREEN no son una escritura fuera de alcance; las filas de la tabla son conformes por construcción; la regla de la excepción y la CI sin cuelgue se juzgan sobre el RED.
- **INV-PBT0-03. Se fija lo que hay.**
  - Ninguna prueba existente se edita, se reactiva ni se borra.
  - Sin `Skip`.
  - Lo que contradice una regla documentada lleva «INCUMPLIMIENTO CARACTERIZADO» con la regla citada.
  - No se corrige nada («No product cleanup in PB-T0»).
- **INV-PBT0-04. Medido, no inferido.**
  - Todo hecho «lectura» de MA y MB se mide antes de fijarlo.
  - Las clases (c), (d), `CARGA-APPLICATION` y `OTRA` llevan el `archivo:línea` causante, medido en la base.
  - Una clase nueva se fija tal cual y se enumera en T-2, sin STOP (OB-06.5).
- **INV-PBT0-05. El aviso.** Todas las clases de UI van en `EditorDiscardPromptCollection`. Nunca un `MessageBox` real. El aviso de producción no se restaura.
- **INV-PBT0-06. Ninguna escritura nueva.**
  - Ninguna prueba ejecuta `SaveLibrary_Click`, `PromptSaveToLibrary`, `RackProjectStore.Save`, `Bom_Click`, `RackBomWindow`, `InsertBatch_Click`, `InsertViewAt` ni «Insertar».
  - Ninguna instancia `RackFrameConfiguratorWindow` ni asigna `HeaderConfiguratorPresenter`.
  - `ConfigureModuleHeaderButton` solo se pulsa con `HeaderConfiguratorDialog` ya asignado.
  - **La prueba de la limpieza:** antes y después de las corridas locales de los filtros se registran el SHA-256 y la fecha del archivo de disposición del configurador (`<datos-locales-del-usuario>`) y de la carpeta de biblioteca (`<datos-de-aplicacion-del-usuario>`). No cambian.
- **INV-PBT0-07. Las ventanas.**
  - Sin mostrar, salvo CT-14, que va fuera de pantalla y con `ShowActivated = false`.
  - Sin esperas fijas.
  - Toda ventana se cierra en `finally`, y ningún modal, dispatcher ni ventana queda vivo.
  - Ninguna aserción dentro de una costura.
- **INV-PBT0-08. Solo ejecución.** PB-T0 no escribe guardas de texto (D-PBT0-04).
- **INV-PBT0-09. Textos exactos.** Los textos de producto que se fijan, como el de `StatusText`, se copian byte a byte.
- **INV-PBT0-10. Sin infraestructura nueva.**
  - Sin costuras, sin cambios en `InternalsVisibleTo`, sin paquetes, sin P/Invoke y sin rutas propias de una máquina.
  - Solo las costuras existentes de H-03 y los ayudantes de H-18, en solo uso.
- **INV-PBT0-11. Determinismo.** Lo del §4.1: cultura invariante fijada y restaurada, orden ordinal, sin hora ni aleatoriedad. En Core, sin coma flotante fijada.
- **INV-PBT0-12. CI y compilación.**
  - **GREEN:** los cuatro jobs en `success`, con las suites completas.
  - **RED:** `tests` y `ui-tests` en `failure`, `build-ui` en `success` y `build-plugin` omitido por su `needs`, sin `blame-hang` ni timeout.
  - Sin avisos nuevos (CS, MSB, NU y NETSDK), tampoco con los mutantes.
- **INV-PBT0-13. Correcciones:** según el §5.
- **INV-PBT0-14. La tabla CERRADA de mutantes:** la del §5.
  - Un mutante fuera de ella es PBT0-C04.
  - Un mutante que no discrimina su CT en su corrida aislada es PBT0-C10.
- **INV-PBT0-15. Prueba por mutante.** El Worker aplica en local cada mutante **solo** sobre `BaseSha`, corre los dos filtros y registra, por mutante:
  - el `archivo:línea` medido en la base, con su parche custodiado;
  - las pruebas que fallan y si fallan por aserción;
  - la fila que instancia.
- **INV-PBT0-16. Las clases de escritura.**
  - PB-T0 solo ejerce la clase (B): el estado y la intención de la ventana, en ejecución.
  - La clase (A), las escrituras de la propia ventana, y la clase (C), las del Plugin, siguen fijadas por T1B y no se tocan.
- **INV-PBT0-17. El corpus.**
  - Ningún archivo nuevo de fixtures (D-PBT0-01).
  - `Fixtures/I64` en solo lectura, por la raíz del repositorio (WLC:41-54).
  - Toda construcción en código, declarada en la entrega.
- **INV-PBT0-18. Las pérdidas, en sus clases** (D-PBT0-03).
  - Solo CT-18, CT-19 y CT-20 aseveran líneas de clase (a) o (b).
  - CT-21 no asevera la pérdida: asevera su efecto en el plan.
- **INV-PBT0-19. No duplicación.** Cada CT cita la cobertura existente del §4.2, y la entrega rellena T-5.
- **INV-PBT0-20. Contención de las corridas locales** (F4-E59 y [T1B-X] §6).
  - **Nunca** se corren en local las suites completas de UI: solo los dos filtros, y la suite completa de Core si hace falta. Las completas de UI son de la CI.
  - Ninguna sonda temporal ejerce con entradas válidas un camino con diálogo o con escritura. «Actualizar», que solo anota y cierra (H-08), no es uno de ellos.
  - **MAX_PATH:** el Worker no copia árboles del repositorio, ni `bin/` ni `obj/`, a otra ubicación. Cada mutante se aplica y se revierte en el mismo worktree con `git apply` y `git restore`.
  - Las corridas que muestran ventanas se hacen sin uso interactivo del equipo, y se declara.
- **INV-PBT0-21. Guardas que los archivos nuevos mantienen en verde** (H-19):
  - un solo `namespace` igual a la raíz del proyecto;
  - en Core, sin `ramelements`, `python` ni `bentley` en el código y sin literales partidos;
  - la guarda del corpus de WLC.
- **INV-PBT0-22. Las tablas del informe** (§4.5, T-1 a T-7) van en la entrega. El Worker mide y no juzga la materialidad.
- **INV-PBT0-23. Reglas propias de F4B** ([P] §7.2 y D-PB-10):
  - un rechazo de la planificación vuelve al Owner;
  - un control nc1 a nc3 fallido vuelve al Owner, sin sustitutos;
  - antes de un intento 3, siempre al Owner.
- **INV-PBT0-24. Sin AutoCAD.** El bloque M es aparte y no consume intentos de este bloque.

### 6.6 `ExpectedEvidence`

- **Antes del Worker:**
  - el contrato y la delegación válidos;
  - la comparación byte a byte, A1-A8 y nc4, porque es la primera delegación;
  - el `AllowedWriteScope` de la delegación = el del contrato.
- **El commit RED y su push,** antes del GREEN:
  - la CI del RED de INV-PBT0-12;
  - los TRX de los dos filtros cumplen INV-PBT0-02;
  - las existentes rotas, como unión y atribuidas.
- **La prueba por mutante** (INV-PBT0-15): una corrida aislada por fila, y por alternativa usada.
- **El commit GREEN, inverso exacto, y su push:**
  - `git diff ChainBaseSha..CurrentSha -- src` vacío y `--name-only` = los diez archivos;
  - la tabla de SHA-256 por archivo de `MutantScope` tocado: `ChainBaseSha`, `RedSha` distinto y `CurrentSha` igual a `ChainBaseSha`;
  - la CI del GREEN 4/4.
- **En local:**
  - el build Release de la solución, con el Plugin, sin avisos nuevos, sobre el RED y sobre el GREEN;
  - los dos filtros sobre el RED y el GREEN;
  - tres corridas seguidas en verde del filtro de UI;
  - la prueba de la limpieza.
- **`worker-handoff.json` conforme a su esquema,** con:
  - el RED, el GREEN, el alcance, el trailer y los conteos;
  - la tabla de mutantes y el mapa CT → método;
  - T-1 a T-7 y `KnownLimitations`.
- **Después:** la verificación del Controller válida y nc1..nc3 según el README §10, sin sustitutos (INV-PBT0-23). La verificación 14/14 sola no basta para cerrar.

### 6.7 `StopConditions`

**Las estándar de [T1B-C]** (`:371-439`): S-02, S-03, S-04, S-05, S-07, S-08, S-12, S-13 y S-14, y P-01 a P-08.

**Las propias:**
- **PBT0-C01:** un CT exige cambiar producto o una costura nueva, más allá de los mutantes revertidos.
- **PBT0-C02:** habría que editar, reactivar o borrar una prueba o un ayudante existente; o tocar un `.csproj`, `InternalsVisibleTo` o los paquetes; o añadir cualquier archivo bajo `tests/RackCad.Tests/Fixtures/`.
- **PBT0-C03:** un CT necesitaría AutoCAD, un `MessageBox` real, el configurador real, el gesto, la ventana o la exportación del BOM, «Guardar en biblioteca» o una escritura en el perfil o en disco.
- **PBT0-C04:** cualquiera de estos casos:
  - un mutante fuera de la tabla o de `MutantScope`, o que viola las reglas del §5;
  - un método obligatorio que ningún mutante detecta en la conjunción;
  - una aserción sin mutante propio que no está declarada como ancla en un método detectado.
- **PBT0-C05:** la CI completa del GREEN falla fuera de las pruebas nuevas, se reproduce en una segunda corrida y no es atribuible a ellas. Si es atribuible, es REWORK; si no se reproduce, es una reejecución por BLOCKED.
- **PBT0-C06:** una ruta de `MutantScope` o un texto objetivo de la tabla no existe, o no coincide con el de la base.
- **PBT0-C07:** los hechos del §2 no se sostienen sobre `BaseSha` de un modo que cambie un CT.
- **PBT0-C08:** cualquier rechazo de la planificación del Controller, **incluido el de la comparación literal de las colecciones**. Es STOP y **vuelve al Owner**, sin replanificación ([P] §7.2).
- **PBT0-C09:** una prueba se colgaría esperando un diálogo real, dejaría vivos un modal, un dispatcher o una ventana, escribiría en el perfil o dependería de rutas de una máquina.
- **PBT0-C10:** un mutante de la tabla no discrimina su CT en la corrida aislada, o lo hace solo por excepción, y ninguna de sus alternativas vale. Es un defecto del contrato; el Worker no inventa.
- **PBT0-C11:** **cualquier control nc1, nc2 o nc3 que no dé CONTROL PASS** vuelve al Owner. Ningún sustituto de F4 (PD-F4-01, PD-F4-04 a PD-F4-07 u otros) se usa sin orden expresa ([P] §7.2).
- **PBT0-C12:** una corrección sería el intento 3. Vuelve al Owner (D-PB-10).

### 6.8 `EligibleCells`

Se copian de [T1B-C] (`:481-515`), porque siguen vigentes en `model-catalog.md`:
- **Worker:** `anthropic/subagent/claude-sonnet-5-5`, effort `high`, con `write-commit-push` (`:48-61`).
- **Controller:** `openai/process/gpt-6-luna`, effort `high` (`:94-108`), con el binario de Codex acreditado.
- **Vigencia:** las dos están verificadas el 2026-09-30 y son `STALE` desde el 2026-12-29 (`:14`).
- nc4 en la primera delegación, y nc1 a nc3.

## 7. Riesgos y lecciones de T1B aplicadas

| Id | Riesgo o lección | Tratamiento |
|---|---|---|
| R-01 | **nc2 falló en T1B con la firma de PD-F4-01** ([T1B-X] `:56-57`): Contract en fail por la cláusula de exactitud de la lista blanca, además de Scope. Es estructural del control. En F4B no hay sustituto, así que es **previsible un STOP por PBT0-C11** | Es la pregunta de procedimiento ya abierta en [P] §7.2 y §10 (`:682`, `:761`). No bloquea la emisión, pero conviene tenerla respondida antes de nc2 (§8) |
| R-02 | Mutantes en Application (04, 05, 07, 08, 09, 17 y 20): la unión de existentes rotas del RED será mayor que la de T1B, y hay que atribuirla | Admitida por INV-PBT0-02; se atribuye con las corridas aisladas |
| R-03 | La conjunción hace fallar alguna prueba por excepción | Alternativas por fila; si no hay, PBT0-C10 |
| R-04 | Las celdas vencen el 2026-12-29 | Se comprueba en la emisión y en cada delegación (A7) |
| R-05 | F4-E59 / I-02: las suites completas de UI escriben la disposición real del configurador (la deuda H-1) | INV-PBT0-20: en local solo los filtros. El prompt de la sesión no pide suites completas de UI |
| R-06 | F4-E59 / I-03: una sonda con entradas válidas en un camino con diálogo | INV-PBT0-20 y la lista prohibida de INV-PBT0-06 |
| R-07 | MAX_PATH | Sin copias de árboles; mutantes en sitio (INV-PBT0-20) |
| R-08 | Adenda de anclas de T1B: aserciones sin mutante no declaradas y referencias `archivo:línea` desplazadas | T-6 obligatoria; la tabla de mutantes da la línea medida en la base y el parche |
| R-09 | PB-T1 moverá el estado: los CT que leen costuras internas (H-03) pueden quedar frágiles | Se asevera sobre el contrato público y `LastComputation.Design` donde existe. T-7 lista las costuras usadas para el paquete de PB-T1 (OB-T1B.3: si no puede mantenerlas verdes, vuelve al Owner) |
| R-10 | La observación de T1B: la `LastComputation` del compuesto difiere de un recálculo | CT-02 la fija tal cual; no es parada |
| R-11 | Core corre en Ubuntu y local en Windows | En Core solo se fijan claves y booleanos (INV-PBT0-11) |
| R-12 | Una sola prueba muestra la ventana (CT-14) | Fuera de pantalla, en la colección, sin uso del equipo |

## 8. Decisiones para el Owner

**Ninguna nueva.** Todas las del §3 caben en [P] y en el patrón de T1B, y se declaran para la revisión independiente: corpus sin fixtures, un `Fact` por CT, la lectura de «el BOM», el filtro de (a) y (b) y la mitad Core de «Módulos» ya cubierta.

**Pregunta heredada, ya abierta en [P] §7.2 y §10** (`:682`, `:761`, «de procedimiento y sin bloquear PB-T0»): si se extiende a F4B el sustituto de nc2 de F4-T1B (`F4T1B-NC2-EXACT-WHITELIST-01`, con la firma exacta de PD-F4-01). Sin esa extensión, PB-T0 se detendrá previsiblemente en nc2 (R-01). Se recomienda hacerla llegar al Owner antes de lanzar los controles, no antes de emitir.
