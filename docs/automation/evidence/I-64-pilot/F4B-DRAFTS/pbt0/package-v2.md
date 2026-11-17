> **Redaccion para el repo publico:** las rutas locales y las variables de entorno de carpetas del usuario se sustituyen por marcadores (`<scratchpad>`, `<worktree-de-I-64>`, `<datos-locales-del-usuario>`, `<datos-de-aplicacion-del-usuario>`). El resto no cambia.

# I-64 F4B — Paquete del contrato de PB-T0, bloque C (caracterización automática)

**Estado:** v2. La prepara la sesión principal el 2026-11-17. No está emitida y no declara GATE PASS.
- **v1** (`_pbt0-package-v1.md`, custodiada como historia): revisión adversarial independiente (`pbt0-independent-review-v1.md`, **IRC1**), **EMITIBLE TRAS CORRECCIONES (0/4/21)**.
- **v2:** aplica los cuatro importantes, los 21 menores y los seis cambios OD-1 a OD-6 que IRC1 deriva de las decisiones de procedimiento del Owner del 2026-11-17. El cierre de cada uno está en el §9.
- **Antes de emitirse:** IRC1 recomienda una comprobación focalizada de la v2 por el mismo revisor. Si da 0 bloqueantes, 0 importantes sin resolver y ninguna decisión material nueva, se emite con attempts 1/3 sin volver al Owner ([OD]:259-268).

**Patrón:** el de F4-T1B v5:
- su paquete, `docs/initiatives/I-64-f4-t1b-gate-contract-package.md` (**[T1B]**);
- su contrato, `…/F4-T1B-PILOT-CHARACTERIZATION/coordinator/gate-contract.json` (**[T1B-C]**);
- su cierre, `…/task-close-package.md` (**[T1B-X]**).

**Lo que desarrolla:** el §6.2 del paquete de F4B v3.1, custodiado en `docs/initiatives/I-64-f4b-push-back-completo-package.md` en `67e28452` (**[P]**; todas las anclas `[P]:n` son de ese archivo).

**Base:** rama `architecture/workspace-persistente-rackcad` en `67e28452`, con `main` en `812baf87`. `git diff 5ccaa770 67e28452 -- src tests` está vacío: el producto es el de `5ccaa770`. Todo el §2 está medido ahí, en solo lectura, sin compilar ni ejecutar.

**Mapas:** **MA** (`map-a-classic-write-path.md`, con H-061 corregido) y **MB** (`map-b-editor-state-testability.md`), en `docs/automation/evidence/I-64-pilot/F4B-DRAFTS/package/`. Cada ancla que funda una decisión se comprobó en el código (§2).

**Precondiciones de emisión:**
1. La comprobación focalizada de esta v2, con el resultado de arriba.
2. **Custodia.** Ya están en `67e28452`:
   - [P] v3.1;
   - [OD] en `F4B-DRAFTS/coordinator/owner-f4b-disposition.md`;
   - MA, MB y las revisiones de la fase en `F4B-DRAFTS/package/`;
   - `F4B-DRAFTS/f4b-dispositions.md`, con F4B-E01 y F4B-E02;
   - IC-11.

   **Faltan, y van en el commit de emisión, que es la `AuthorityRevision`:**
   - las decisiones de procedimiento del Owner, en `F4B-DRAFTS/coordinator/owner-f4b-procedure-decisions.md` (**[DP]**), con su anotación **F4B-E03** en `f4b-dispositions.md` (OD-1);
   - este paquete, en `docs/initiatives/I-64-f4b-pbt0-gate-contract-package.md`;
   - la v1 y la revisión IRC1, en `F4B-DRAFTS/pbt0/`;
   - el procedimiento del bloque M y el script generador del contrato, junto al contrato en `docs/automation/evidence/I-64-pilot/F4B-PBT0-CHARACTERIZATION/coordinator/` (M-06).
3. `git diff 5ccaa770 <BaseSha> -- src tests` vacío. Si no lo está, se vuelve a medir el §2 (PBT0-C07).
4. Las celdas del §6.8 no están `STALE`. El `config.toml` coincide con la Snapshot aceptada vigente, hoy la **I** (`docs/automation/evidence/I-64-pilot/F4-ENVIRONMENT/snapshot-I/snapshot-i.json`; `f4-dispositions.md:1973`). El binario de Codex es el acreditado.

## 1. Autoridades

| Autoridad | Qué fija |
|---|---|
| [OD] (`F4B-DRAFTS/coordinator/owner-f4b-disposition.md`) | PB-T0 «characterization / measurement only» y «No product cleanup in PB-T0» (`:157-177`); emisión condicional y attempts 1/3 (`:259-268`) |
| [DP] (`F4B-DRAFTS/coordinator/owner-f4b-procedure-decisions.md`, F4B-E03) | Sustituye a [P] §7.2 **solo** en dos cosas: la replanificación (`:11-15`) y los sustitutos de nc1 y nc2 con firma **exacta** (`:5-9`). Además, el bloque M lo opera la sesión (`:17-21`). Cierra la pregunta de procedimiento de [P] §10 (`:765`) |
| [P] §6.2 (`:561-605`) | Las filas que se fijan y la cobertura que no se duplica; el corpus (`:588-590`); el censo control → prueba (`:592`); las reglas de T1B copiadas (`:594-603`); el RED por mutantes con tabla cerrada (`:605`) |
| [P] OB-06.1, OB-06.3b, OB-06.4 y OB-06.5 (`:370`, `:384`, `:387-391`, `:392`) | Las pérdidas (a) y (b) en clases propias; el criterio de «materially»; la enumeración de (c), (d) y las clases nuevas |
| [P] §7.2 (`:679-686`), con [DP] | Fuera de lo que cambia [DP], sigue la regla: un rechazo de la planificación o un control fallido vuelven al Owner |
| [P] §7.3 (`:688-711`) y D-PB-10 (`:460-471`) | T1B no se edita; 1/3 por tarea; antes del intento 3, siempre al Owner |
| [P] §6.3 a §6.5 (`:607-672`) y **el anexo** `pbt0-block-m-procedure.md` (custodiado con el contrato) | El bloque M, aparte del bloque C |
| Proposal V5 D-15, D-16 e INV-19 (`docs/initiatives/I-64-proposal-v5.md:557-593`) | Caracterización por sistema antes de extraer |
| ADR-0029 D13 (`docs/adr/0029-contrato-funcional-comun-de-ventanas-wpf.md:172-180`) | Se fija primero lo actual, y no se edita para que pase |
| `docs/AUTOMATION_PLAN.md` §16.5 (`:526-548`), §16.8 (`:581-599`), §16.9 (`:601-646`) y §16.11 (`:655-689`) | A1-A8; RED de la cadena; las 14 comprobaciones; las paradas |
| [T1B] D-T1B-03 (`:246-374`) y el contrato (`:486-622`); [T1B-C] INV-F4T1B-01 a 16 (`:328-345`); [T1B-X] §5 y §6 (`:85-136`); F4-E59 (`F4-DRAFTS/f4-dispositions.md:2047`) | El patrón y sus lecciones |
| Las fuentes literales de las firmas: `F4-DRAFTS/f4-dispositions.md:1103-1107`, `:1540`; `F4-DRAFTS/coordinator/owner-f4t3-authorization.md:255-265` | PD-F4-01 y PD-F4-04, transcritas en el §6.7 |
| `docs/automation/agent-execution/model-catalog.md` (`:14`, `:48-61`, `:94-108`) | Celdas vigentes |

## 2. Hechos medidos en `67e28452` (producto de `5ccaa770`)

**Alias:**
- **W** = `src/RackCad.UI/Systems/PushBack/RackPushBackSystemWindow.xaml.cs` (3630 líneas);
- **WC** = `….Composite.cs` (911);
- **WX** = `….xaml` (792);
- **CS** = `src/RackCad.Application/Systems/PushBack/PushBackCompositeEditorState.cs`;
- **DA** = `src/RackCad.Application/Systems/PushBack/PushBackEditorDesignAssembler.cs` (578);
- **WLC** = `tests/RackCad.UI.Tests/Workspace/WorkspaceDraftPilotWindowLoadCharacterizationTests.cs`.

### 2.1 La ventana: contrato, cargas y pasadas

- **H-01.** El constructor (W:128-153) carga el catálogo de la sesión (W:135-136), acepta `newIdFactory` (W:128) y termina en `LoadNew()` (W:152). No admite catálogo inyectado.
- **H-02. Contrato público** (W:247-256): `InsertRequested`, `UpdateOnly`, `RackId`, `RackName`, `InsertionRequest`, `SystemToInsert` y `DesignToInsert`.
- **H-03. Costuras existentes que PB-T0 usa** (solo lectura; es la lista cerrada de INV-PBT0-10):
  - **la ventana:**
    - `Session` y `State` (W:157-159);
    - `LastComputation`, `CurrentInputsAreValid` y `SafetySelections` (W:182-185);
    - `RecomputePassesForTest` (W:601), `EditorStateForTest` (W:1529) y `PendingHeaderConfigurationForTest` (W:2002);
    - `PendingWork()` (W:3417), `OutputIsBlockedForTest` (W:3487) y `CompositeState` (WC:43);
  - **los diálogos:** `SafetyDialog` (W:1493), `RearTopeDialog` (W:1499), `DefenseDialog` (W:1505), `HeaderConfiguratorDialog` (W:1525) y `BootPerPostWindowDialog` (W:2700);
  - **las secciones de Seguridad (I-04):**
    - en la ventana: `BootSectionForTest` y `BootSectionBForTest` (W:2661, :2664), `DefenseSectionForTest` y `DefenseSectionBForTest` (W:2801, :2804), `RearTopeSectionForTest` y `RearTopeSectionBForTest` (W:2971, :2974);
    - sus miembros públicos: en `PushBackBootSection`, `Configure()` (`src/RackCad.UI/Systems/PushBack/PushBackBootSection.cs:279`), `Button` (`:254`), `ModeBox` (`:252`), `Posts` (`:248`) y `Placement` (`:242`); en `PushBackDefenseSection`, `Configure()` (`PushBackDefenseSection.cs:205`), `Button` (`:182`) y `Posts` (`:178`); en `PushBackRearTopeSection`, `Configure()` (`PushBackRearTopeSection.cs:152`), `Button` (`:143`) y `Config` (`:146`);
  - **la rejilla de botas, `SafetyPerPostWindow` (I-04):** `SetForTest(int, BootPlacement)` (`src/RackCad.UI/SelectiveSafetyWindow.cs:1043`), `BuildResultForTest()` (`:1052`) y `Result` (`:1037`). Las usa ya `PushBackSafetyPerPostFlowTests.cs:246-251`.

  **Prohibidas:** `HeaderConfiguratorPresenter` (W:2410), que crea el configurador real, y `SafetyWindowDialog` (W:3028), que crea `SelectiveSafetyWindow`.
- **H-04. Cargas.** `LoadExisting` (W:303-316): `SideAOnly` + `LoadCompositeFromDesign`, `Adopt(rackId, rackName)` y `LoadFromModel` (W:315). `SideAOnly` (W:334-365) devuelve el mismo objeto para un diseño no compuesto.
- **H-05. `LoadFromModel`** (W:377-427):
  - sustituciones en W:384-392; la unidad en W:387, la escala en W:406 y el recorte de `Dimensions` a `[0, Detailed]` en W:407;
  - `RecoverInputs` ya normaliza `PalletsDeep`, tolerancia, `BeamDepth` y escala, conserva la unidad tal cual y lleva `DimensionStyle` (`src/RackCad.Application/Systems/PushBack/PushBackEditorState.Load.cs:192-210`, `:229-232`).
- **H-06. `ReadInputs`** (W:812-857):
  - `generalPallet` da frente, alto y peso sin pasar por los controles (W:818-820);
  - los avanzados se copian de su portador (W:830-835);
  - `Annotations` se arma sin `DimensionStyle` (W:840-848);
  - `NumericField.SetNumber` formatea con «0.###» y la cultura actual (`src/RackCad.UI/Controls/NumericField.cs:131-136`).
- **H-07. `Recompute`** (W:603-702): `CommitCurrentCell` (W:621; W:737-741), y `BuildDesign` + `BuildFrom` dentro de un `try` (W:645-646). La puerta es síncrona (`src/RackCad.UI/Editor/RecomputeGate.cs:35-44`).
- **H-08. «Actualizar»** = `RequestDraw(null, -1, true)` (W:3292). Dentro de `RequestDraw` (W:3323-3379):
  - las guardas (W:3325-3335) y la confirmación del módulo (W:3346-3349);
  - la **segunda pasada** (W:3351);
  - la validez y el bloqueo (W:3352-3363);
  - `SetName(NameBox.Text?.Trim())` (W:3365);
  - `RequestUpdate` con `lastComputation.Design` (W:3369-3370) y `Close()` (W:3378).

  No abre ningún diálogo ni escribe. «+ frente» es `SetSlotCount(state.Structure.Count + 1)` (W:1137-1138).

### 2.2 Módulos, avanzados y subdiálogos

- **H-09. Módulos en la ventana.** «Confirmar» = `CommitModuleEdits()` + `RequestRecompute()` (W:2462-2463). «Cancelar» está en W:2466-2477. «Restaurar estándar» solo pide el reinicio, sin recalcular (W:2497-2501).
- **H-10. El reinicio en DA:**
  - `standardRestore` (DA:204) y `mustRebuild` (DA:211-213);
  - sin cola en el reinicio (DA:229-231);
  - `Reset(editorInputs)` bajo `if (standardRestore)` (DA:300-303);
  - sin intenciones (DA:315-317);
  - la reconciliación es `RackModuleReconciliation` (DA:36, :50), la misma clase que fija el caso `Incompatible` en `tests/RackCad.Tests/RackModuleEditSessionTests.cs:381-399`.

  El portador `advanced` solo se escribe en W:395-400, W:1592-1607 y W:1628-1634. Lo de H-PB-28 se lee en el código y **lo mide CT-PB0-05**.
- **H-11. El configurador.** Con `HeaderConfiguratorDialog` asignado no se crea ninguna ventana (W:1938-1940), y lo devuelto se escenifica en W:1946. El botón es `ConfigureModuleHeaderButton` (WX:569). Para clonar una configuración, `new RackFrameProjectStore().DeepCopy(c)` (`src/RackCad.Application/Persistence/RackFrameProjectStore.cs:87`).
- **H-12. Seguridad** (W:2534-2659):
  - las secciones se construyen antes del diálogo (W:2565-2611);
  - con `SafetyDialog` asignado no se crea `SelectiveSafetyWindow` (W:2615-2617);
  - al aceptar: `Authorize(chosen)` (W:2626), `ApplyDefenseSections` (W:2638), `ApplyBootSections` (W:2642) y, en un rack de un sentido, `state.LoadRearTopeConfig` (W:2653);
  - **la familia DEFENSA no viaja en la lista:** sus secciones la recrean (W:2555-2558, W:2927-2959);
  - viajan en la lista, por ejemplo, el protector lateral y el desviador (`PushBackSafetyAuthority.cs:239-255`). El catálogo entregado trae `PROTECTOR_LATERAL_BOTA_H_3_16_18` de tipo `LATERAL` (`assets/catalogs/seguridad.csv`);
  - la rejilla de botas se crea con `Owner = this` (W:2716-2721) antes de su costura (W:2722), así que exige la ventana mostrada.
- **H-13. El corpus** (`tests/RackCad.Tests/Fixtures/I64/`, 17 `pb-*.json`):
  - `pb-20-ab`: compuesto; módulos `M1..M8`, `GAP`, `B:M8..B:M1`, con `B:M2` y `B:M4` de tipo `Separator`; `DefaultTopology = Encontradas`; `Gap = 54`;
  - `pb-21-cola`: los mismos ids, con `SideB.IsPresent = false`;
  - `pb-31-v2-modulos`: 3 frentes y `M1..M8`. M2, M4, M5 y M7 son `Separator` de 48; M3 es `HeaderIntermediate` 48 **manual**;
  - `pb-06-seguridad`:
    - `DimensionStyle = "STANDARD-ISO"`;
    - seguridad `PROTECTOR_BOTA_H_3_16_18` y `DEFENSA_MONTACARGAS`;
    - avanzados no por defecto: `ManualHeaderHeightOverride = 150`, `DerivedPostReinforced = false`, `SeparatorCountOverride = 3` y `SeparatorSpacingOverride = 18`;
  - `pb-07-combinado`: `DerivedPostHeight = 140` y `DimensionStyle = "SIMPLEX"`;
  - `pb-05-topes`: de un sentido;
  - `pb-08-redondeo`: `AnnotationScale = 1.23456`, `Fronts[0].Levels[0].PalletHeight = 61.23456` y `Dimensions = 2`;
  - `pb-01-base`: 3 frentes; `PalletDepth 48`, `PalletTolerance 4`, `BeamDepth 6` y `FirstLevelHeight 4`; la celda (0,0) con 42/60/1000 y `ClearHeight 6`; `PostPeralte` nulo.

  Ningún fixture nuevo hace falta.

### 2.3 Composición, cortes y anotaciones

- **H-14. Estado compuesto:**
  - `SetSideBPresent` (CS:294-324), con el cambio de topología por defecto en CS:316-323;
  - `IsSlotPresent` (CS:428-437) y `SetSlotPresent` (CS:453);
  - `SetSlotCount` (CS:522-544);
  - el manejador de la casilla, `SideBPresent_Changed` (WC:101-120), termina en `RequestRecompute()` (WC:119);
  - al cargar, la cola se aparca desde `structure.Modules` con ids de cola (WC:838-842).
- **H-15. Cortes y su decisión:**
  - `PushBackSystemLateralBuilder.Cortes` (`src/RackCad.Application/Systems/PushBack/PushBackSystemLateralBuilder.cs:91-114`);
  - en un compuesto, una ranura está activa si lo está en A o en B (`PushBackCompositeStructure.cs:397-400`), que es MA H-061 corregido;
  - RACKEDITAR: los cortes y los hechos (`src/RackCad.Plugin/RackPushBackCommands.cs:268-271`); obsoleta si no está disponible o falta su corte (`:324-333`); **la regla del último vínculo** (`:348-356`);
  - `PushBackViewAvailabilityFacts.Contains` (`src/RackCad.Application/Systems/Shared/RackViewAvailability.cs:106-115`, el poste en `:110-111`) y `Evaluate` (`:136-146`);
  - `RackViewCodec.Decode` (`src/RackCad.Application/Systems/Shared/RackViewCodec.cs:115`);
  - `RackEmbedDocument.KindPushBack` y `ViewLateral` (`src/RackCad.Application/Persistence/RackEmbedDocument.cs:20`, `:29`).
- **H-16. La computación** expone `Design`, `System`, `Bom`, `LateralPlan`, `FrontalEntradaSalida`, `FrontalPosterior`, `PlantaPlan` y `LateralCortes` (`src/RackCad.Application/Systems/PushBack/PushBackEditorComputation.cs:47-67`). `BuildFrom` resuelve con `PushBackResolver` y arma el BOM y los cortes una sola vez (DA:466-474). `HeaderRunPlan.Flatten()` está en `src/RackCad.Application/Drawing/HeaderRunPlan.cs:31`.

  **`HeaderBlockInstance`** (`src/RackCad.Application/Drawing/HeaderBlockInstance.cs:59-101`) lleva:
  - `Role`, `PieceId`, `BlockName`, `View`, `Insertion`, `ConnectionAnchor` y `RotationRadians`;
  - `MirroredX` (`:80`) y `MirroredY` (`:83`);
  - **`DynamicParameters`** (`:86`), los largos y alturas de los bloques dinámicos que dibuja el Plugin;
  - `Text`, `TextHeight`, `DimensionOffset` y `DimensionStyleName`.

  No lleva capa: la decide el Plugin por el rol (MA H-069).
- **H-17. Estilo y escala:**
  - `system.DimensionStyle = design.DimensionStyle` (`src/RackCad.Application/Systems/Dynamic/DynamicRackSystemResolver.cs:246`);
  - `DynamicViewDecorations` lee el estilo en `:117`, `:206` y `:285`, lo escribe en cada cota en `:402` y calcula la escala en `:328-329`;
  - `PushBackSideAnnotations` solo actúa en compuestos (`:50-53`, `:74-77`).

### 2.4 Infraestructura de pruebas, guardas y cobertura existente

- **H-18. Infraestructura:**
  - `StaTestRunner.Run` (`tests/RackCad.UI.Tests/StaTestRunner.cs:78`, `:102`) y el suelo del aviso (`:57`);
  - `EditorDiscardPromptCollection.Name` (`EditorDiscardPromptCollection.cs:27-30`);
  - los ayudantes, de solo uso: `ClickNamed` (`EditorWindowTestSupport.cs:22`), `SetFrontBlank` (`:67`), `SetText` (`:89`) y `SetNumberAndCommit` (`:102`);
  - `RackCad.Tests` solo referencia Domain y Application (`tests/RackCad.Tests/RackCad.Tests.csproj:23-24`): **ningún mutante de W o WC alcanza a Core**.
- **H-19. Guardas que los archivos nuevos y los mutantes deben respetar:**
  - el `namespace` único igual a la raíz del proyecto (`tests/RackCad.Tests/NamespaceFolderGuardTests.cs:211-245`);
  - INV-14, sin `ramelements`, `python` ni `bentley` en el código de Core (`tests/RackCad.Tests/StructuralAnalysis/StructuralAnalysisSourceGuardTests.cs:262-268`, `:416-419`, `:430-443`), y los literales partidos (`…/StructuralAnalysisRamElementsHardeningGuardTests.cs:85-92`);
  - el corpus de WLC (WLC:323-332);
  - G-10 (`tests/RackCad.Tests/Workspace/WorkspacePilotUpdateIntegrationGuardTests.cs:721-797`);
  - las guardas de T1B sobre la ventana: T-SG, T-KB, T-OW y T-RS (MB H-129, H-130);
  - `tests/RackCad.Tests/I55DeferredProductFixCharacterizationTests.cs:8-15`, `tests/RackCad.Tests/PushBackModuleEditorCharacterizationTests.cs:382-400` y `tests/RackCad.UI.Tests/BlankFrontDesviadorHandoffTests.cs:312-318`;
  - `tests/RackCad.Tests/ProductPolicyIndependenceGuardTests.cs:8-18` y `tests/RackCad.Tests/Workspace/WorkspacePilotPrepareGuardTests.cs:388-404`.

  IRC1 comprobó que ninguna falla con los archivos nuevos ni con los mutantes.
- **H-20. Cobertura existente que acota los CT:**
  - **la ventana frente al sobre, en los 14 de un sentido:**
    - WLC compara la ventana con el piloto (WLC:84-91, :244-245);
    - `WorkspaceDraftPilotMaterializationTests.cs:682` y `LegacyDeltas` (`:706`) fijan que el piloto devuelve el sobre;
    - los dos eslabones juntos dan la ventana frente al sobre (MB H-033);
  - `T19_H1_TheDimensionStyleStillComesOutNull` (`tests/RackCad.UI.Tests/PushBackDimensionViewsWindowTests.cs:309`);
  - el estilo nombrado sintético, por los constructores del Plugin: `PushBackDimensionLegacyCharacterizationTests.cs:249-251`;
  - el corte que se omite en la frontera entre dos frentes en blanco: `BlankFrontBoundaryTests.cs:223`, `:235` y `:310`;
  - los módulos en Core, con `PushBackModuleAdoptionTests` (`:64-220`) y `PushBackCompositeModuleSequenceTests` (`:103-309`), y el caso `Incompatible` en `RackModuleEditSessionTests.cs:381-399`;
  - los módulos en UI: `PushBackModuleEditorWindowTests` (`:104-302`) y `PushBackCompositeLoadExistingModulesTests`;
  - los subdiálogos: las nueve clases que usan las costuras de H-03. Ninguna llama a `LoadExisting`.
- **H-21.** Ningún archivo de `src` ni de `tests` contiene `PushBackFull`: los filtros del §6.4 seleccionan hoy 0.

## 3. Decisiones del paquete

- **D-PBT0-01. El corpus, sin fixtures nuevos.**
  - `Fixtures/I64` se usa en solo lectura.
  - Lo demás se construye en código: editando el JSON interior antes de deserializar, o con los gestos de la ventana y `LoadExisting` sobre su `LastComputation.Design`.
  - `tests/RackCad.Tests/Fixtures/` queda prohibida entera. [P]:590 admite las dos vías.
- **D-PBT0-02. Un CT = un método `[Fact]`, sin `Theory`.** Un CT solo se parte si el mutante de su fila detecta en aislado cada método resultante.
- **D-PBT0-03. Las pérdidas (a) y (b), solo en sus clases.** Fuera de CT-PB0-18 a CT-PB0-20, las líneas de clase (a) o (b) se retiran antes de comparar (§4.1). Así PB-TC sustituye solo esas clases (OB-06.3b).
- **D-PBT0-04. Sin guardas de texto.** Las escrituras de la ventana y del Plugin las fija T1B (CT-22 y CT-24 a CT-27).
- **D-PBT0-05. El RED, con mutantes de comportamiento revertidos** (§5), con la mecánica de [T1B] D-T1B-03.
- **D-PBT0-06. La lectura declarada del BOM.** [P]:598 dice «ninguna prueba ejecuta "Guardar en biblioteca", el BOM, el configurador real ni un `MessageBox` real». Se lee así:
  - quedan prohibidos el gesto, la ventana y la exportación (`Bom_Click`, `RackBomWindow`);
  - leer `PushBackEditorComputation.Bom`, que es Application puro y se calcula en cada recálculo (DA:472), no lo es.

  El texto y la lectura van en INV-PBT0-06.
- **D-PBT0-07. La mitad Core de «Módulos» ya está fijada** (H-20, con el caso `Incompatible`): se cita. La de «Añadir/quitar frentes» es CT-PB0-10.
- **D-PBT0-08. Ninguna ventana se muestra salvo en CT-PB0-14.** [P]:599 pide «ventanas fuera de pantalla». Una ventana sin mostrar lo cumple, y CT-PB0-14 se muestra fuera de pantalla.

## 4. Los puntos de caracterización (lista cerrada: CT-PB0-01 a CT-PB0-21)

### 4.1 Definiciones comunes

- **`Ventana()`:** `new RackPushBackSystemWindow(canInsertInAutoCad: true, newIdFactory: () => "00000000-0000-4000-8000-0000000000b0")` (M-11). Ningún flujo de PB-T0 debería acuñar un GUID; si alguno lo hiciera, sale ese.
- **`Abrir(t)`:** deserializa el texto del sobre `t` (`RackEmbedStore` y `RackProjectStore`), crea `Ventana()` y llama a `LoadExisting(project.PushBackDesign, embed.Id, embed.Name, project)`, sin mostrarla.
- **`Salida(w)`:** `RackProjectStore.Serialize(RackProject.ForPushBack(w.LastComputation.Design))` con `\r\n` → `\n` (WLC:56-57). Solo si `CurrentInputsAreValid` y `LastComputation.IsValid`; si no, `NO-VÁLIDA: <texto de StatusText>`.
- **`Sobre(t)`:** lo mismo sobre el diseño interior **deserializado otra vez** desde `t`, nunca sobre el objeto que recibió la ventana.
- **`Delta(x, y)`:** las líneas `ruta: x -> y`, en orden ordinal, con el algoritmo de recorrido de WLC:95-181, copiado y no referenciado.
- **Clase de una línea `ruta: x -> y`** (M-01; no se copia `ClassOf`):
  - **(a):** `x` e `y` son números, `x ≠ y` e `y == R3(x)`, con `R3(v) = double.Parse(v.ToString("0.###", CultureInfo.InvariantCulture), CultureInfo.InvariantCulture)`;
  - **(b):** la ruta es `$.PushBack.Structure.DimensionStyle`;
  - **(c), (d), `CARGA-APPLICATION` y `OTRA:<descripción>`:** una etiqueta en los datos esperados, con el `archivo:línea` del producto que la causa, medido en la base.
- **Filtro (D-PBT0-03):** fuera de CT-PB0-18 a CT-PB0-20 se retiran las líneas de clase (a) y (b), y la entrega las lista.
- **Comparación:** igualdad ordinal exacta de la lista o de la tupla fijadas, con los valores medidos en la base.
- **Determinismo:**
  - cultura invariante en el hilo STA, fijada y restaurada en `finally`;
  - en UI, los números se escriben con «R»;
  - en Core solo se fijan claves, enteros, cadenas y booleanos; las comparaciones de magnitud usan los umbrales declarados (0,001 y ε = 1e-9).
- **Ventanas:** sin mostrar, salvo CT-PB0-14. Se cierran en `finally`. Todas las clases de UI van en `EditorDiscardPromptCollection`.
- **Delegados:** ninguna aserción dentro de una costura: se captura dentro y se asevera fuera.
- **Precondiciones:** toda precondición es una aserción.
- **Sin teclado ni foco real** (M-21): no hay gestos de teclado ni `ProcessKey`. `LostFocus` se eleva con `RaiseEvent` (`SetNumberAndCommit`). El foco efectivo no se usa.

### 4.2 Los CT

**CT-PB0-01. Propiedad del estado: la carga frente al sobre.** UI · `PushBackFullEditorCharacterizationStateTests`.
- **Diseños:** `pb-20-ab`, `pb-21-cola`, `pb-31-v2-modulos` y **X-01**. X-01 se construye así: `Abrir(pb-21-cola)`, se declara B, lado B en `SideSelectorBox`, la ranura 0 de B activa con `SetFrontBlank` y su `LastComputation.Design` en otra `Ventana()` con `LoadExisting`, con el mismo id y nombre.
- **Fija:** por diseño, `Delta(Sobre, Salida)` filtrado como lista `(línea, clase)`, y el `IsComposite` de la salida.
- **No duplica:** la cadena WLC + `WorkspaceDraftPilotMaterializationTests.cs:682` / `LegacyDeltas` (`:706`) para los 14 de un sentido (H-20, MB H-033).

**CT-PB0-02. Propiedad del estado: la segunda pasada (Actualizar) frente a la primera.** UI · `…StateTests`.
- **Diseños:** los 17 de I64 y X-01.
- **Pasos:** `S1 = Salida(Abrir)`. Después se lee `UpdateButton.IsEnabled` y **solo si está habilitado** se pulsa con `ClickNamed`.
- **Fija por diseño** la tupla (M-08):
  - `IsEnabled`;
  - el resultado: `PETICIÓN`, `BLOQUEADO: <StatusText>`, `INVÁLIDO: <StatusText>` o `DESHABILITADO: <StatusText>`;
  - con petición: `UpdateOnly == true`, `InsertionRequest.Views.Count == 0` y `Delta(S1, Persist(DesignToInsert))` filtrado.
- **No duplica:** nada compara el diseño entero de la petición (MB H-036). T1B CT-06 solo fija el cierre.

**CT-PB0-03. Módulos: escenificar, confirmar y cancelar en un rack cargado.** UI · `PushBackFullEditorCharacterizationModuleTests`.
- **Diseños:** `pb-31-v2-modulos` (editar **M2**, cancelar **M4**) y `pb-20-ab` (editar **B:M2**, cancelar **B:M4**). Los dos son `Separator`, y se eligen en `ModuleBox` por `ModuleId` en `EditorStateForTest.ModuleSession`.
- **Pasos:**
  1. el editado, `ModuleLengthBox` + 8 y `LostFocus`;
  2. «Confirmar»;
  3. el cancelado, + 4 y «Cancelar»;
  4. `DepthBox` + 6 con `SetNumberAndCommit`, que fuerza la reconstrucción.
- **Fija:**
  - tras 1: la `Salida` igual a la previa y `PendingWork()` con trabajo;
  - tras 2: el `Delta` y `PendingWork()` vacío;
  - tras 3: la `Salida` igual a la confirmada;
  - tras 4: la secuencia `(ModuleId, Kind, Length, IsCalculated, IsManualOverride)` y las listas públicas de `EditorStateForTest.LastModuleReconciliation` (`Preserved`, `Removed`, `Incompatible`…).
- **No duplica:** H-20, que trabaja sobre racks nuevos o construidos.

**CT-PB0-04. Añadir y quitar módulos por reconstrucción.** UI · `…ModuleTests`.
- **Diseño:** `pb-31-v2-modulos`.
- **Pasos:**
  1. `DepthBox` + 12;
  2. `PalletsDeepBox` + 1;
  3. M2 + 8 y «Confirmar»;
  4. `RestoreAllModulesButton` y «Confirmar».
- **Fija:** tras la carga y tras cada paso, la secuencia de CT-PB0-03.
- **No duplica:** `PushBackModuleEditorWindowTests:280-302` y `PushBackModuleAdoptionTests:175`.

**CT-PB0-05. Los avanzados tras «Restaurar estándar»** (H-PB-28). UI · `…ModuleTests`.
- **Diseños:** `pb-06-seguridad` (cuatro avanzados no por defecto) y `pb-07-combinado` (`DerivedPostHeight`) (M-07).
- **Fija, por diseño y solo los seis campos avanzados** (W:395-400):
  - A0: la carga, en la salida y en el texto o estado de sus seis controles (W:1599-1606);
  - A1: tras `RestoreAllModulesButton` y «Confirmar»;
  - A2: tras Actualizar, en `DesignToInsert`.
- **Nada más de la salida:** pb-06 y pb-07 llevan `DimensionStyle` (OB-06.3b).
- **Incumplimiento:** si A2 no conserva el reinicio, «INCUMPLIMIENTO CARACTERIZADO», citando la regla de DA:296-299.
- **No duplica:** `PushBackModuleEditorWindowTests:280-302` y `PushBackAdvancedRackParametersTests`.

**CT-PB0-06. Cola aparcada.** UI · `PushBackFullEditorCharacterizationCompositeTests`.
- **Diseño:** `pb-21-cola`.
- **Pasos:** carga → declarar B → activar la ranura 0 de B → retirar B.
- **Fija en cada paso:** los ids de `CompositeState.DormantCompositeTail` y `SideBPresent`; de la salida, `IsComposite` y los ids de `Structure.Modules`.
- **No duplica:** las pruebas de cola de MB H-047, que no cargan el corpus.

**CT-PB0-07. A/B.** UI · `…CompositeTests`.
- **Diseño:** `pb-20-ab`.
- **Pasos:** carga → retirar B → declarar B → todas las ranuras de B en blanco.
- **Fija en cada paso:**
  - de la salida: `IsComposite`, la presencia de `SideB`, `Composite.AbsentSlotsA` y `AbsentSlotsB`, y `Composite.DefaultTopology` (la escribe `BuildComposite` siempre, CS:952-962);
  - `SideBPresentCheck.IsChecked` y los ids aparcados.
- **No duplica:** `PushBackBlankFrontAuthorityTests` y `PushBackCompositeWindowTests:82-138`, `:610-680` y `:1366-1418`.

**CT-PB0-08. Frentes sin B.** UI · `…CompositeTests`.
- **Diseño:** `pb-01-base`.
- **Pasos:** `AddFrontButton` → `RemoveFrontButton` → frentes 1 y 2 en blanco → frente 2 activo otra vez.
- **Fija en cada paso:** el número de `Structure.Fronts` e `IsActive` de cada uno, `Composite.AbsentSlotsA` y `AbsentSlotsB`, y la lista de `LastComputation.LateralCortes[*].PostIndex`.
- **No duplica:** `PushBackViewTests:152` (Core) y `BlankFrontBoundaryTests:223`.

**CT-PB0-09. Frentes con B.** UI · `…CompositeTests`.
- **Diseño:** `pb-01-base`.
- **Pasos:** declarar B → `AddFrontButton` con A activo → lado B y la ranura 1 de B activa → ranuras 1 y 2 de A en blanco → ranura 1 de B en blanco → retirar B.
- **Fija:** lo de CT-PB0-08 más `IsComposite`.
- **No duplica:** `PushBackCompositeWindowTests:1121`.

**CT-PB0-10. Borrado de vistas: la decisión pura.** Core · `PushBackFullEditorCharacterizationLateralCutTests`.
- **Punto de partida:** `PushBackPilotMaterializer.Materialize(pb-01-base)`, admitido.
- **Ediciones** sobre `State.Editor`:
  - de un sentido: `SetSlotCount(n+1)`; `SetSlotCount(n)`; `SetSlotPresent(A,1,false)` + `SetSlotPresent(A,2,false)`;
  - compuesta, sobre la última: `SetSideBPresent(true)` → `SetSlotPresent(B,1,true)` → `SetSlotPresent(B,1,false)` → `SetSideBPresent(false)`.
- **Cálculo en cada estado:**
  - diseño = `new PushBackCompositeEditorAssembler(catalog).BuildDesign(editor, inputs)`;
  - sistema = `new PushBackResolver(catalog).Resolve(diseño)`;
  - cortes = `new PushBackSystemLateralBuilder().Cortes(sistema, catalog)`, igual que `RackPushBackCommands.cs:269`.
- **Fija por par consecutivo:**
  - las dos listas de `PostIndex`;
  - por cada `p` de «antes», el `Status` de `RackViewAvailability.Evaluate(RackViewCodec.Decode(KindPushBack, ViewLateral, p), new PushBackViewAvailabilityFacts(cortes(después), después.IsComposite))`;
  - las obsoletas: `Status ≠ Available` o sin corte con ese `PostIndex`, igual que `RackPushBackCommands.cs:324-329`.
- **Catálogo:** `JsonRackCatalogProvider.FromBaseDirectory().Load()`.
- **No duplica:** las guardas de MA H-121 y `BlankFrontBoundaryTests:223`.

**CT-PB0-11. Renombrar.** UI · `…StateTests`.
- **Diseño:** `pb-01-base`.
- **Casos**, cada uno en su ventana: `"  Renombrado PB0  "`, `""` y `"   "`. En cada uno: `SetText(NameBox, …)` y `ClickNamed(UpdateButton)`.
- **Fija:** `InsertRequested`, `RackName`, `InsertionRequest.RackName`, `RackId` (igual al `Id` del sobre: no se acuña ningún GUID) y `SystemToInsert.Name`, tal cual.
- **No duplica:** T1B CT-05 ni CT-06.

**CT-PB0-12. El configurador, por costura.** UI · `PushBackFullEditorCharacterizationSubdialogTests`.
- **Diseño:** `pb-31-v2-modulos`, módulo **M3**.
- **Costura** `HeaderConfiguratorDialog` (M-18): captura su argumento y devuelve `new RackFrameProjectStore().DeepCopy(arg)` con un cambio declarado en un campo. No muta el argumento. `HeaderConfiguratorPresenter` nunca se asigna.
- **Pasos:** `ConfigureModuleHeaderButton` → «Confirmar». Después, otra vez con una costura que devuelve `null`.
- **Fija:**
  - que la costura se llamó;
  - `PendingHeaderConfigurationForTest` no nulo antes de confirmar;
  - el `Delta` de la salida;
  - con `null`: nada escenificado y la salida sin cambio.
- **No duplica:** `PushBackModuleEditorWindowTests:167-245`.

**CT-PB0-13. Seguridad aceptada.** UI · `…SubdialogTests`.
- **Diseño:** `pb-06-seguridad`. Dos pasos, cada uno en su ventana:
  - **(i) Quitar `DEFENSA_MONTACARGAS` de la lista.** Se fija como **hallazgo de caracterización** (M-03): las secciones la recrean (H-12), así que `SafetySelections.Count` y el `Side` y `AuthoredSide` de esa selección se fijan tal cual, junto con el `Delta`.
  - **(ii) Añadir a la lista recibida `PROTECTOR_LATERAL_BOTA_H_3_16_18`**, una familia que sí viaja, con su forma declarada.
- **Fija en (ii):** el `Delta` y `SafetySelections` (ids y orden).
- **No duplica:** `PushBackSafetyFlowTests` ni T1B CT-17.

**CT-PB0-14. Botas por poste.** UI · `…SubdialogTests`, con la ventana **mostrada** fuera de pantalla y `ShowActivated = false` (H-12).
- **Diseño:** `pb-06-seguridad`.
- **Costuras:**
  - dentro de `SafetyDialog`: `BootSectionForTest.Configure()`;
  - `BootPerPostWindowDialog = win => { win.SetForTest(0, <colocación declarada>); win.BuildResultForTest(); return true; }`;
  - `SafetyDialog` devuelve la lista recibida sin cambios.
- **Fija:** que la costura se llamó y el `Delta` de la salida.
- **No duplica:** `PushBackBootUiFlowTests`, `PushBackBootSidesUiFlowTests` y `PushBackSafetyPerPostFlowTests`.

**CT-PB0-15. Defensa por poste.** UI · `…SubdialogTests`.
- **Diseño:** `pb-06-seguridad`.
- **Costuras:** dentro de `SafetyDialog`, `DefenseSectionForTest.Configure()`; `DefenseDialog` devuelve registros declarados para el poste 0; `SafetyDialog` devuelve la lista sin cambios.
- **Fija:** que `DefenseDialog` se llamó y el `Delta` de la salida.
- **No duplica:** las tres clases de defensa de H-20.

**CT-PB0-16. Topes traseros.** UI · `…SubdialogTests`.
- **Diseño:** `pb-05-topes`, de un sentido, por W:2653.
- **Costuras:** dentro de `SafetyDialog`, `RearTopeSectionForTest.Configure()`; `RearTopeDialog` devuelve un `TopeResult` declarado, con la celda (0,0) apagada; `SafetyDialog` devuelve **la lista recibida sin cambios** (M-18).
- **Fija:** que la costura se llamó y el `Delta` de la salida.
- **No duplica:** `PushBackTopeSectionInSafetyTests` ni `PushBackRearTopePerSideAndNoneTests`.

**CT-PB0-17. Dependencias de catálogo y sustituciones (c).** UI · `PushBackFullEditorCharacterizationCatalogTests`.
- **Base:** `pb-01-base`. Se edita el JSON interior, y la misma edición da el `Sobre`.
- **Casos:**
  - c-01: `HeaderPostCatalogId = ""`;
  - c-02: `HeaderPostCatalogId = "PB0_POSTE_INEXISTENTE"`;
  - c-03: `PushBack.Fronts[0].HighEndBeamPeraltes[0] = 7.25`;
  - c-04: `Structure.Fronts[0].Levels[0].InOutBeamCatalogId = "PB0_VIGA_INEXISTENTE"`;
  - c-05: `…Levels[0].InOutBeamDepth` = máximo de `DynamicRackLevelGeometry.AllowedPeraltes(catalog, beamId)` + 0,5 (`DynamicRackLevelGeometry.cs:172-174`);
  - c-06: `…Levels[0].IntermediateBeamCatalogId = "PB0_VIGA_INEXISTENTE"`;
  - c-07: c-05 en `Fronts[1].Levels[0]`, para la prueba de (d);
  - c-08 y c-09: `PalletWeightUnit` igual a `""` y a `"ton"`;
  - c-10 y c-11: `Dimensions` igual a 7 y a −1;
  - c-12 a c-15: `PalletsDeep = 1`, `PalletTolerance = 0`, `BeamDepth = 0` y `AnnotationScale = 0`.
- **Fija por caso:** `Delta(Sobre, Salida)` filtrado, con la clase de cada línea.
- **No duplica:** WLC, sin delta (c), ni `pb-12-poste-nulo`.

**CT-PB0-18. Pérdida (b): el `DimensionStyleName` del plan.** Core · `PushBackFullEditorCharacterizationDimensionStyleLossTests` (clase propia, OB-06.3b).
- **Diseños:** `pb-06` y `pb-07` tal cual, y los mismos con `DimensionStyle = null`.
- **Cálculo:** `BuildFrom(D, A)`; las instancias `Role == Dimension` de los cinco planes y de cada corte.
- **Fija:**
  - `IsValid` y `System.Structure.DimensionStyle`;
  - el número de cotas por plan, igual con y sin estilo y mayor que 0;
  - los conjuntos `{"STANDARD-ISO"}`, `{"SIMPLEX"}` y `{null}`.
- **No duplica:** `PushBackDimensionLegacyCharacterizationTests:249-251`, que fija el estilo nombrado sintético; ni `T19_H1` ni WLC, que fijan la pérdida en la ventana.

**Tabla de campos de MB H-077**, que usan CT-PB0-19 y CT-PB0-21. El valor exacto es la base + 0,00044:

| Fila | Campo (JSON interior) | Base | Control |
|---|---|---|---|
| F1 | `Structure.PalletDepth` | 48 | `DepthBox` |
| F2 | `Structure.PostPeralte` | 3 (nulo en el fixture) | `PostPeralteBox` |
| F3 | `Structure.PalletTolerance` | 4 | `ToleranceBox` |
| F4 | `Structure.BeamDepth` | 6 | `BeamDepthBox` |
| F5 | `Structure.AnnotationScale` | `pb-01`: 1,5; `pb-08`: 1,23456 tal cual | `AnnotationScaleBox` |
| F6 | `Structure.Fronts[0].FirstLevelHeight` | 4 | `FirstLevelHeightBox` |
| F7 a F10 | `Structure.Fronts[0].Levels[0].{PalletFront, PalletHeight, PalletWeight, ClearHeight}` | 42, 60 (`pb-08`: 61,23456 tal cual), 1000 y 6 | `CellPallet*Box` y `CellClearBox` |
| F11 | `Structure.Fronts[0].Levels[0].BeamLengthOverride` | 100 (nulo en el fixture) | `CellBeamLengthOverrideBox` |

Una fila cuyo diseño no resuelve se fija `NO-VÁLIDA` tal cual.

**CT-PB0-19. Pérdida (a): los campos que redondean.** UI · `PushBackFullEditorCharacterizationRoundingLossTests` (clase propia).
- **Base:** `pb-01-base`, una variante por fila F1 a F11.
- **Fija por fila:** el valor exacto, el valor del campo en `Salida` y su clase: `REDONDEA`, `CONSERVA` u `OTRA`.
- **No duplica:** WLC pb-08.

**CT-PB0-20. Pérdida (a): los campos que no redondean** (MB H-079). UI · `…RoundingLossTests`. Filas, con el valor exacto igual a la base + 0,00044 (M-09):
- G1 a G3: `Structure.PalletFront`, `PalletHeight` y `PalletWeight`, en `pb-01`;
- G4: `DerivedPostHeight`, de base 140, en `pb-07`;
- G5: `ManualHeaderHeightOverride`, de base 150, en `pb-06`;
- G6: `SeparatorSpacingOverride`, de base 18, en `pb-06`;
- G7: `Composite.Gap`, de base 54, en `pb-20-ab`;
- G8: `Modules[M3].Length`, de base 48 y ya manual, en `pb-31`;
- G9: `Fronts[1].Levels[0].PalletHeight`, de base 60, en `pb-01`.

**Fija:** como CT-PB0-19.

**CT-PB0-21. Efecto del redondeo, para OB-06.4.** Core · `PushBackFullEditorCharacterizationRoundingEffectTests` (clase propia).
- **Filas:**
  - R0: `pb-08` tal cual (E) frente a `pb-08` con sus dos campos en `R3` (R);
  - R1 a R11: sobre `pb-08`, la fila Fn con su valor exacto (E) y con `R3` (R).
- **Cálculo:** `BuildFrom(D, A)` para E y para R.
- **Claves de plan:** `planta`, `frontal-es`, `frontal-post`, `lateral` y `corte-<PostIndex>`, en orden ordinal. Las listas de claves de cada K se dan en ese orden (M-10).
- **Firma de una instancia:** `(Role, PieceId, BlockName, View, MirroredX, MirroredY, claves de DynamicParameters en orden ordinal)` (I-03).
- **Fija por fila:**
  - **K1 (piezas):** las claves donde difiere el multiconjunto de firmas. La capa va en el rol;
  - **K1o:** las claves donde el multiconjunto coincide y el orden de `Flatten().Instances` no. Esas claves no se emparejan;
  - **K2:** si difieren, por frente del sistema resuelto, el número de niveles, el de posiciones o la lista de `PostIndex`;
  - **K3:** si difieren las líneas de `Bom`, con todas sus propiedades públicas y en el orden producido;
  - **emparejamiento:** en las claves sin K1 ni K1o, la instancia `i` de E con la `i` de R;
  - **K4:** las claves donde algún par cambia más de 0,001 en `Insertion`, `ConnectionAnchor` o **cualquier `DynamicParameters[clave]`** (I-03);
  - **K4′:** lo mismo con ε = 1e-9, a título informativo (M-10);
  - **K5:** las claves con cambio > 1e-9 en `TextHeight`, `DimensionOffset` o `RotationRadians`;
  - **material** = K1 no vacía, o K2, o K3, o K4 no vacía. Es una **fórmula mecánica**, no un juicio.
- **Informe:** la entrega da el máximo |Δ| por fila. La decisión de OB-06.4 es del Owner antes de PB-TC.
- **No duplica:** nada (fila nueva de la v3.1).

### 4.3 Filas del §6.2 sin CT nuevo

Se citan en el informe:
- **Creación de vistas:** T1B CT-06 a CT-09 y CT-23.
- **Actualizar frente a cerrar:** T-CL y T-IN.
- **Escrituras al volver la ventana:** T-PG y MA H-120 a H-123.
- **La regla del último vínculo** (`RackPushBackCommands.cs:348-356`; M-19): las guardas de texto de MA H-121, y su efecto en el DWG lo mide el bloque M (M-2b del anexo).
- **La mitad Core de «Módulos»:** D-PBT0-07.

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
| Borrado de vistas | 10 (y §4.3) |
| Subdiálogos | 12 a 16 |
| Dependencias de catálogo | 17 |
| Pérdida del estilo de cotas | 18 |
| Pérdida por redondeo | 19, 20 |
| Efecto del redondeo | 21 |

### 4.5 Tablas que el Worker rellena en su entrega

- **T-1. Censo control → prueba** (MB H-029 a H-031): la prueba de cada control o `HUECO`.
- **T-2. Casos alcanzables** de (c), (d), `CARGA-APPLICATION` y `OTRA` (OB-06.5), por caso:
  - la construcción y la ruta;
  - el antes → después;
  - la clase y el `archivo:línea`;
  - si se alcanza por la interfaz o solo por documento;
  - el CT.
- **T-3. Efecto del redondeo:** por fila de CT-PB0-21, K1 a K5, el máximo |Δ| y «material».
- **T-4. H-PB-28:** A0, A1 y A2 de los dos diseños.
- **T-5. Cobertura cruzada**, por fila del §6.2.
- **T-6. Anclas:** las aserciones sin mutante propio, con el método que sí lo tiene.
- **T-7. Costuras usadas** (I-04): cada costura de H-03 que la entrega usa, con el CT y el método.
- **T-8. Atribución de las existentes rotas** (§5), por análisis declarado.

## 5. El RED: tabla CERRADA de mutantes

**`MutantScope`**: seis archivos, todos con mutantes **de comportamiento** (M-02 retira `PushBackSideAnnotations.cs`):

| Archivo | Mutantes |
|---|---|
| `src/RackCad.UI/Systems/PushBack/RackPushBackSystemWindow.xaml.cs` | 01, 02, 03, 05 alt., 08, 10 a 16, 18, 19 |
| `src/RackCad.UI/Systems/PushBack/RackPushBackSystemWindow.Composite.cs` | 06, 07 alt. |
| `src/RackCad.Application/Systems/PushBack/PushBackCompositeEditorState.cs` | 07 |
| `src/RackCad.Application/Systems/PushBack/PushBackEditorDesignAssembler.cs` | 04, 05 |
| `src/RackCad.Application/Systems/Shared/RackViewAvailability.cs` | 09 |
| `src/RackCad.Application/Systems/Dynamic/DynamicViewDecorations.cs` | 17, 20 |

**La tabla.** «Debe fallar» = el método del CT, con **ese mutante solo** sobre `BaseSha`. «alt.» es la única alternativa admitida, y solo si la principal no discrimina o hace fallar un método **nuevo** por excepción.

| MU | Archivo:línea | Cambio exacto | Debe fallar |
|---|---|---|---|
| MU-PB0-01 | W:315 | Antes de `LoadFromModel(inputs, rackName);` de `LoadExisting`: `if (design.IsComposite) inputs.PalletTolerance += 1.0;` | CT-01 |
| MU-PB0-02 | W:3365 | Antes de `session.Identity.SetName(NameBox.Text?.Trim());` de `RequestDraw`: `lastComputation.Design.Structure.PalletTolerance += 1.0;` | CT-02 |
| MU-PB0-03 | W:2463 | Se quita `RequestRecompute();` de `ConfirmModule_Click` | CT-03 |
| MU-PB0-04 | DA:211-213 | `mustRebuild` sin el término `standardRestore \|\|` | CT-04 |
| MU-PB0-05 | DA:300 | `if (standardRestore)` → `if (standardRestore && editorInputs == null)`. **Alt.:** W:2499, antes de `RequestStandardRestore();`, `advanced.DerivedPostHeight = null;` | CT-05 |
| MU-PB0-06 | WC:839-840 | El primer argumento de `ParkDormantTail` → `structure.Modules.Take(0)` | CT-06 |
| MU-PB0-07 | CS:320-323 | **(I-02)** Se quita la rama `else if (!present && DefaultTopology == PushBackCellTopology.Encontradas) { … }`. **Alt.:** WC:119, se quita `RequestRecompute();` de `SideBPresent_Changed` | CT-07 |
| MU-PB0-08 | W:1138 | **(I-02)** `composite.SetSlotCount(state.Structure.Count + 1)` → `composite.SetSlotCount(state.Structure.Count + 2)` | CT-08, CT-09 |
| MU-PB0-09 | `RackViewAvailability.cs:111` | `return posts.Contains(address.Variant.Index);` → `return posts.Contains(address.Variant.Index) \|\| address.Variant.Index >= 0;` | CT-10 |
| MU-PB0-10 | W:3365 | `SetName(NameBox.Text?.Trim())` → `SetName(NameBox.Text)` | CT-11 |
| MU-PB0-11 | W:1946 | `StageHeaderConfiguration(module, edited);` → `StageHeaderConfiguration(module, copy);` | CT-12 |
| MU-PB0-12 | W:2626 | `Authorize(chosen)` → `Authorize(safetySelections)` | CT-13 |
| MU-PB0-13 | W:2642 | Se quita `ApplyBootSections(bootSectionA, bootSectionB);` | CT-14 |
| MU-PB0-14 | W:2638 | Se quita `ApplyDefenseSections(defenseSectionA, defenseSectionB, defenseCarrierId);`. El inicializador de `defenseCarrierId` no es constante, así que no da CS0219 | CT-15 |
| MU-PB0-15 | W:2653 | Se quita `state.LoadRearTopeConfig(topeSection.Config);` y queda `else { }` | CT-16 |
| MU-PB0-16 | W:407 | `Math.Min((int)DimensionDetail.Detailed, …)` → `Math.Min((int)DimensionDetail.Standard, …)`. **Alt.:** W:387, `? "kg" :` → `? "lb" :` | CT-17 |
| MU-PB0-17 | `DynamicViewDecorations.cs:402` | `DimensionStyleName = style` → `DimensionStyleName = style?.ToLowerInvariant()` | CT-18 |
| MU-PB0-18 | W:406 | `AnnotationScaleBox.SetNumber(…)` con un segundo argumento `"0.######"` | CT-19 |
| MU-PB0-19 | W:819 | `generalPallet.Height` (el tercer argumento) → `Math.Round(generalPallet.Height, 3)` | CT-20 |
| MU-PB0-20 | `DynamicViewDecorations.cs:329` | `? system.AnnotationScale :` → `? Math.Round(system.AnnotationScale, 3) :`. Sin alternativa (M-02) | CT-21 |

**Reglas de los mutantes:**
- **Ningún mutante:**
  - toca una asignación de `Owner`, un `ShowDialog`, un `MessageBox` ni `EditorDiscardPrompt`;
  - toca la comprobación de nulo de una costura (W:1938, :2615, :2722, :2849, :2981, :3018);
  - toca las guardas de `RequestDraw` (W:3325-3363) ni el cuerpo del constructor;
  - añade `.RequestUpdate(` ni literales de T-SG;
  - provoca una escritura;
  - deja un modal abierto.
- **Compilación:** compilan sin avisos nuevos (ni CS0162, CS0168, CS0169, CS0219, CS0414 ni CS8321; las clases CS, MSB, NU y NETSDK).
- **La regla de la excepción (I-01):** se aplica **solo a los métodos de los dos filtros nuevos**. Un mutante que, solo o en la conjunción, hace fallar un método nuevo por excepción y no por aserción no vale. Se usa su alternativa, y si no la tiene o tampoco vale, es PBT0-C10.

  En las pruebas existentes **solo** cuentan el cuelgue y el timeout: la regla (c) de [T1B] D-T1B-03 (CI del RED sin `blame-hang` ni timeout). Que una existente falle por excepción con los mutantes es admisible.
- **La tabla es exhaustiva.** Un mutante fuera de ella es PBT0-C04, y el Worker no inventa mutantes.
- **La conjunción:** con todos aplicados, todos los métodos nuevos fallan por aserción, y ningún mutante anula a otro.
- **Las existentes rotas** (I-02):
  - se admiten **solo** por los mutantes;
  - se miden como **unión**, en la CI del RED, que corre las suites completas;
  - se atribuyen por **análisis declarado**, como [T1B]:257: por cada una, el mutante o mutantes cuyo código ejerce, con la línea de producto y la de la prueba (T-8).
- **El apoyo de la atribución:**
  - en Core, la **suite completa de Core corre en local, aislada, por cada mutante de Application**: MU-04, MU-05, MU-07, MU-09, MU-17 y MU-20. Los mutantes de W y WC no alcanzan a Core (H-18);
  - en UI, **nunca la suite completa en local** (INV-PBT0-20): solo los filtros nuevos por mutante, más el análisis.
- **Correcciones:**
  - vuelven a aplicar todos los mutantes de los filtros con `ExpectRed`;
  - Core pasa a `ExpectRed` false solo si no se vuelve a aplicar ningún mutante de Application, y se declara;
  - las comprobaciones de producto se anclan a `ChainBaseSha`.

## 6. Contrato propuesto (a fijar en la emisión)

### 6.1 Identidad

- `Schema` `rackcad-gate-contract/v1`; `Unit` I-64; `Gate` F4B; `TaskId` `F4B-PBT0-CHARACTERIZATION`.
- Evidencia en `docs/automation/evidence/I-64-pilot/F4B-PBT0-CHARACTERIZATION/`.
- Attempts 1 de 3; `RoutingEnforcement` `required`; `CorrectionsAuthorized` true.
- **Objective:** «Caracterizar con pruebas nuevas el comportamiento observable ACTUAL del estado editable de la ventana clásica de Push Back y de las funciones Application de las que dependen PB-T1, PB-T2 y PB-T3, según el §6.2 del paquete de F4B v3.1 y la lista cerrada CT-PB0-01..21. El RED lleva solo los mutantes de la tabla cerrada MU-PB0-01..20, y el GREEN los revierte con los bytes del producto idénticos a la base. No se corrige nada ni se ejecuta AutoCAD.»

### 6.2 `AllowedWriteScope` (16 rutas exactas)

- **UI, siete:** `tests/RackCad.UI.Tests/PushBackFullEditorCharacterization{State,Module,Composite,Subdialog,Catalog,RoundingLoss}Tests.cs` y el ayudante `tests/RackCad.UI.Tests/PushBackFullEditorCharacterizationSupport.cs`. El ayudante es `internal static`, sin pruebas.
- **Core, tres:** `tests/RackCad.Tests/PushBackFullEditorCharacterization{LateralCut,DimensionStyleLoss,RoundingEffect}Tests.cs`.
- **Los seis archivos de `MutantScope`**, marcados «solo mutante temporal, neto cero».

### 6.3 `ForbiddenWriteScope`

- **Producto (M-06).** Lo genera el script generador de la emisión, custodiado con el contrato, con este algoritmo, el mismo que dio la lista de [T1B-C] `:175-300`:
  - por cada entrada de `src/` en `BaseSha` (`git ls-tree`), se emite `<dir>/` solo si ninguna ruta permitida queda debajo; si queda alguna, se desciende;
  - los archivos se emiten uno a uno;
  - el script comprueba que ninguna ruta permitida queda bajo una prohibida y que toda ruta de `src/` no permitida queda cubierta.

  **Forma esperada:**
  - `src/RackCad.Domain/` y `src/RackCad.Plugin/`, enteros;
  - en `src/RackCad.Application/`, todo menos `Systems/`; dentro, `Cantilever/`, `FlowBed/`, `Larguero/` y `Selective/`; en `Dynamic/`, `PushBack/` (con `Pilot/` como prefijo) y `Shared/`, cada archivo salvo los permitidos;
  - en `src/RackCad.UI/`, todo menos `Systems/PushBack/`, y ahí los ocho archivos no permitidos.
- **Pruebas** (M-16, rutas completas):
  - los prefijos `tests/RackCad.Tests/Fixtures/`, `tests/RackCad.Tests/Workspace/`, `tests/RackCad.Tests/StructuralAnalysis/` y `tests/RackCad.UI.Tests/Workspace/`;
  - los dos `.csproj`;
  - `tests/RackCad.UI.Tests/StaTestRunner.cs`, `tests/RackCad.UI.Tests/EditorWindowTestSupport.cs` y `tests/RackCad.UI.Tests/EditorDiscardPromptCollection.cs`;
  - las siete clases de T1B;
  - `tests/RackCad.Tests/NamespaceFolderGuardTests.cs`, `tests/RackCad.Tests/PushBackDimensionLegacyCharacterizationTests.cs`, `tests/RackCad.Tests/BlankFrontBoundaryTests.cs`, `tests/RackCad.Tests/RackModuleEditSessionTests.cs`, `tests/RackCad.Tests/I55DeferredProductFixCharacterizationTests.cs`, `tests/RackCad.Tests/PushBackModuleEditorCharacterizationTests.cs` y `tests/RackCad.Tests/ProductPolicyIndependenceGuardTests.cs`;
  - `tests/RackCad.UI.Tests/PushBackModuleEditorWindowTests.cs`, `tests/RackCad.UI.Tests/PushBackDimensionViewsWindowTests.cs`, `tests/RackCad.UI.Tests/BlankFrontDesviadorHandoffTests.cs` y `tests/RackCad.UI.Tests/PushBackSafetyPerPostFlowTests.cs`;
  - los archivos de [T1B-C] `:301-313`.
- **Raíz:** `docs/`, `.github/`, `eng/`, `tools/`, `assets/`, `deploy/`, `RackCad.sln`, `Directory.Build.props`, `Directory.Build.targets`, `global.json`, `NuGet.Config`, `README.md`, `AGENTS.md`, `CLAUDE.md`, `.editorconfig`, `.gitattributes` y `.gitignore`.

### 6.4 `RequiredTests`

| Proyecto | Filtro | `MinSelected` | `ExpectRed` |
|---|---|---|---|
| `tests/RackCad.Tests/RackCad.Tests.csproj` | `FullyQualifiedName~RackCad.Tests.PushBackFullEditorCharacterization` | 3 (CT-10, 18 y 21) | true |
| `tests/RackCad.UI.Tests/RackCad.UI.Tests.csproj` | `FullyQualifiedName~RackCad.UI.Tests.PushBackFullEditorCharacterization` | 18 (los demás) | true |

### 6.5 Invariantes

- **INV-PBT0-01. Sin cambio de producto** (anclado a `ChainBaseSha`):
  - `git diff ChainBaseSha..CurrentSha -- src` vacío;
  - `--name-only` = los diez archivos nuevos de prueba;
  - `BaseSha..CurrentSha` dentro de las 16 rutas;
  - los bytes de cada archivo de `MutantScope` tocado, en `CurrentSha`, son los de `ChainBaseSha`. Cualquier residuo es FAIL.
- **INV-PBT0-02. RED por mutantes revertidos.**
  - El mensaje es «RED con mutantes deliberados, revertidos en el GREEN», y ese SHA no sirve para ningún smoke ni para el bloque M.
  - **Criterio mecánico, solo sobre los resultados de los dos filtros nuevos** (I-01), con todos los mutantes a la vez:
    - todos `Failed`;
    - `<Message>` sin el prefijo de tipo `^(?:[A-Za-z_][\w`]*[.+])+[A-Za-z_][\w`]* : `;
    - la primera línea de `<StackTrace>` bajo `tests/`;
    - ninguno `NotExecuted` ni `Skipped`;
    - `Failed == Selected ≥ MinSelected`.
  - Las existentes rotas se atribuyen como en el §5, y para ellas solo cuentan el cuelgue y el timeout.
  - El GREEN es el inverso exacto y no toca `tests/`.
  - **Para el Controller:** las escrituras del RED en `MutantScope` revertidas por el GREEN no son una escritura fuera de alcance, y las filas de la tabla son conformes por construcción.
- **INV-PBT0-03. Se fija lo que hay.** Ninguna prueba existente se edita, se reactiva ni se borra. Sin `Skip`. «INCUMPLIMIENTO CARACTERIZADO» con la regla citada. «No product cleanup in PB-T0».
- **INV-PBT0-04. Medido, no inferido** (M-21):
  - todo hecho «lectura» de MA o MB se mide antes de fijarlo;
  - las clases distintas de (a) y (b) llevan su `archivo:línea` medido;
  - una clase nueva se fija tal cual y se enumera en T-2, sin STOP (OB-06.5);
  - **el código de medición es temporal y no entra en ningún commit**;
  - **no se caracteriza ningún gesto de teclado ni el foco real** (§4.1).
- **INV-PBT0-05. El aviso.** Todas las clases de UI van en `EditorDiscardPromptCollection`. Nunca un `MessageBox` real.
- **INV-PBT0-06. Ninguna escritura nueva** (M-12).
  - **El texto de [P]:598, copiado:** «ninguna prueba ejecuta «Guardar en biblioteca», el BOM, el configurador real ni un `MessageBox` real».
  - **La lectura declarada** (D-PBT0-06): «el BOM» es el gesto, la ventana y la exportación. Leer `PushBackEditorComputation.Bom` no es ejecutarlo.
  - **Por tanto,** ninguna prueba ejecuta `SaveLibrary_Click`, `PromptSaveToLibrary`, `RackProjectStore.Save`, `Bom_Click`, `RackBomWindow`, `InsertBatch_Click`, `InsertViewAt` ni «Insertar».
  - Ninguna instancia `RackFrameConfiguratorWindow` ni asigna `HeaderConfiguratorPresenter` ni `SafetyWindowDialog`.
  - `ConfigureModuleHeaderButton` solo se pulsa con `HeaderConfiguratorDialog` asignado.
  - **La prueba de la limpieza:** el SHA-256 y la fecha del archivo de disposición del configurador (`<datos-locales-del-usuario>`) y de la carpeta de biblioteca (`<datos-de-aplicacion-del-usuario>`), antes y después de las corridas locales. No cambian.
- **INV-PBT0-07. Las ventanas.** Sin mostrar, salvo CT-14. Sin esperas fijas. Todas se cierran en `finally`. Nada queda vivo. Ninguna aserción dentro de una costura.
- **INV-PBT0-08. Solo ejecución:** sin guardas de texto.
- **INV-PBT0-09. Textos exactos,** copiados del producto byte a byte.
- **INV-PBT0-10. Sin infraestructura nueva.**
  - Sin costuras nuevas, sin cambios en `InternalsVisibleTo`, sin paquetes, sin P/Invoke y sin rutas propias de una máquina.
  - Solo las costuras existentes de **H-03**, incluidas las de las secciones y las de `SafetyPerPostWindow` (I-04), y los ayudantes de H-18.
  - T-7 da cada una con su uso.
- **INV-PBT0-11. Determinismo** (M-11):
  - toda ventana se crea con `Ventana()`, con el GUID fijo de `newIdFactory`;
  - cultura invariante fijada y restaurada; orden ordinal;
  - sin hora ni aleatoriedad;
  - en Core, sin coma flotante fijada, con los umbrales de magnitud declarados (0,001 y 1e-9).
- **INV-PBT0-12. CI y compilación.**
  - **GREEN:** 4/4 con las suites completas.
  - **RED:** `tests` y `ui-tests` en `failure`, `build-ui` en `success` y `build-plugin` omitido, sin `blame-hang` ni timeout.
  - Sin avisos nuevos, tampoco con los mutantes.
- **INV-PBT0-13. Correcciones:** las del §5.
- **INV-PBT0-14. La tabla CERRADA de mutantes:** la del §5. Un mutante fuera de ella es PBT0-C04; uno que no discrimina su CT en aislado es PBT0-C10.
- **INV-PBT0-15. Prueba por mutante.** El Worker aplica cada mutante **solo** sobre `BaseSha` y corre:
  - los dos filtros, siempre;
  - además, si el mutante es de Application, la **suite completa de Core**.

  Registra por mutante:
  - el `archivo:línea` medido en la base y el parche custodiado;
  - las pruebas que fallan, y si las nuevas fallan por aserción;
  - la fila que instancia.
- **INV-PBT0-16. Las clases de escritura.** PB-T0 solo ejerce la clase (B). Las clases (A) y (C) las sigue fijando T1B.
- **INV-PBT0-17. El corpus.** Ningún archivo nuevo de fixtures. `Fixtures/I64` en solo lectura. Toda construcción en código va declarada.
- **INV-PBT0-18. Las pérdidas, en sus clases** (D-PBT0-03). Solo CT-18 a CT-20 aseveran (a) o (b). CT-05 fija solo los seis avanzados (M-07).
- **INV-PBT0-19. No duplicación:** el §4.2 y T-5.
- **INV-PBT0-20. Contención de las corridas locales** (F4-E59 y [T1B-X] §6):
  - **nunca** la suite completa de UI en local: solo los filtros; la de Core, sí;
  - ninguna sonda temporal con entradas válidas en un camino con diálogo o escritura. «Actualizar» no es uno de ellos (H-08);
  - **MAX_PATH:** sin copias de árboles; cada mutante, con `git apply` y `git restore` en el mismo worktree;
  - las corridas que muestran ventanas, sin uso interactivo del equipo;
  - **OD-6:** el bloque M, que la sesión opera por control de pantalla ([DP]:17-21), **no se opera mientras el Worker corre en local los filtros de UI**, y el Worker no los corre mientras haya una medición del bloque M en curso.
- **INV-PBT0-21. Guardas** (H-19): un solo `namespace` igual a la raíz; INV-14 y los literales partidos en Core; el corpus de WLC.
- **INV-PBT0-22. Las tablas del informe**, T-1 a T-8, van en la entrega. «Material» es la fórmula mecánica de CT-PB0-21. El Worker no juzga la aceptabilidad.
- **INV-PBT0-23. Reglas de F4B** ([P] §7.2 con [DP]; D-PB-10):
  - **la planificación,** según PBT0-C08;
  - **los controles,** según PBT0-C11, con las firmas literales del §6.7;
  - **antes de un intento 3, siempre al Owner.**
- **INV-PBT0-24. Sin AutoCAD.** El bloque M es aparte (anexo), no consume intentos de este bloque y respeta OD-6.

### 6.6 `ExpectedEvidence`

- **Antes del Worker:**
  - el contrato y la delegación válidos;
  - la comparación byte a byte, A1-A8 y nc4 (primera delegación);
  - el `AllowedWriteScope` de la delegación igual al del contrato;
  - si hubo una replanificación por PBT0-C08, su registro.
- **El commit RED y su push,** antes del GREEN:
  - la CI del RED de INV-PBT0-12;
  - los TRX de los filtros cumplen INV-PBT0-02;
  - la unión de existentes rotas, con T-8.
- **La prueba por mutante** de INV-PBT0-15.
- **El commit GREEN, inverso exacto, y su push:**
  - `git diff ChainBaseSha..CurrentSha -- src` vacío y `--name-only` = los diez archivos;
  - la tabla de SHA-256 por archivo tocado: `ChainBaseSha`, `RedSha` distinto, y `CurrentSha` igual a `ChainBaseSha`;
  - la CI del GREEN 4/4.
- **En local:**
  - el build Release con el Plugin, sin avisos nuevos, sobre el RED y el GREEN;
  - los filtros sobre el RED y el GREEN;
  - tres corridas seguidas en verde del filtro de UI;
  - la prueba de la limpieza.
- **`worker-handoff.json`,** con:
  - el RED, el GREEN, el alcance, el trailer y los conteos;
  - la tabla de mutantes y el mapa CT → método;
  - T-1 a T-8 y `KnownLimitations`.
- **Después:**
  - la verificación del Controller válida;
  - nc1, nc2 y nc3 según el README §10, conservando cada salida ordinaria;
  - **un control que no pasa se resuelve solo como dice PBT0-C11** (OD-4): el sustituto, únicamente con la firma literal exacta del §6.7; si no, el Owner;
  - cada uso se registra en `f4b-dispositions.md`;
  - la verificación 14/14 sola no basta para cerrar.

### 6.7 `StopConditions`

**Las estándar de [T1B-C]** (`:371-439`):
- S-02, S-03, S-04, S-05, S-07, S-08, S-12, S-13 y S-14;
- P-02 a P-08;
- **P-01, con el texto actualizado (M-05):** «`config.toml` no coincide con la Snapshot aceptada vigente (hoy la I, `F4-ENVIRONMENT/snapshot-I/snapshot-i.json`)».

**Las propias:**
- **PBT0-C01:** un CT exige cambiar producto o una costura nueva.
- **PBT0-C02:** habría que:
  - editar, reactivar o borrar una prueba o un ayudante existente;
  - tocar un `.csproj`, `InternalsVisibleTo` o los paquetes;
  - añadir algo bajo `tests/RackCad.Tests/Fixtures/`.
- **PBT0-C03:** un CT necesitaría AutoCAD, un `MessageBox` real, el configurador real, el gesto, la ventana o la exportación del BOM, «Guardar en biblioteca» o una escritura en el perfil o en disco.
- **PBT0-C04:** cualquiera de estos casos:
  - un mutante fuera de la tabla o de `MutantScope`, o que viola las reglas del §5;
  - un método nuevo que ningún mutante detecta en la conjunción;
  - una aserción sin mutante propio que no está declarada como ancla.
- **PBT0-C05:** la CI del GREEN falla fuera de las pruebas nuevas, se reproduce y no es atribuible a ellas.
- **PBT0-C06:** una ruta o un texto objetivo de la tabla no coincide con la base.
- **PBT0-C07:** los hechos del §2 no se sostienen de un modo que cambie un CT.
- **PBT0-C08 (OD-2; [DP]:11-15):**
  - un rechazo de la planificación **solo** por la comparación literal de las colecciones permite **una** replanificación con el prompt reforzado, sin gastar intento;
  - un segundo rechazo de fidelidad es STOP y **vuelve al Owner**;
  - **cualquier otro rechazo** de la planificación (A1-A8 u otra causa) es STOP y vuelve al Owner.
- **PBT0-C09:** una prueba colgaría, dejaría un modal, un dispatcher o una ventana vivos, escribiría en el perfil o dependería de rutas de una máquina.
- **PBT0-C10:** un mutante no discrimina su CT en aislado, o hace fallar un método **nuevo** por excepción, y ninguna alternativa vale. Es un defecto del contrato.
- **PBT0-C11 (OD-3; [DP]:5-9):** un control que no da CONTROL PASS se conserva tal cual, y solo hay dos salidas sin volver al Owner:
  - **nc2 con la firma exacta de PD-F4-01.** El texto literal de su fuente (`f4-dispositions.md:1103-1106`):

    > «- Scope en fail sobre la ruta quitada;
    > - Contract en fail solo por la cláusula de exactitud;
    > - `EXECUTION_BLOCKED`/STOP.»

    Y su aplicación literal (`:1540`): «ninguna otra comprobación distinta de la verificación real».

    → El sustituto **`PBT0-NC2-EXACT-WHITELIST-01`**: copia de `F4T1B-NC2-EXACT-WHITELIST-01` (`docs/automation/evidence/I-64-pilot/F4-T1B-PILOT-CHARACTERIZATION-nc2-substitute/1/R20261114T235423Z-083e/nc2_exact_whitelist_substitute.py`) con **solo** el identificador y el ámbito cambiados. Registro `nc2 = SATISFIED_UNDER_PBT0_EXACT_WHITELIST_SUBSTITUTE`.
  - **nc1 con la firma exacta de PD-F4-04.** El texto literal de su fuente (`owner-f4t3-authorization.md:255-265`):

    > «* false CurrentSha correctly detected;
    > * Identity = fail;
    > * Handoff also fails first only because the handoff itself contains the false SHA;
    > * outcome BLOCKED / STOP;
    > * all other intended checks match;»

    → El sustituto **`PBT0-NC1-IDENTITY-SIGNAL-01`**: copia de `F4T3C2-NC1-IDENTITY-SIGNAL-01` (`docs/automation/evidence/I-64-pilot/F4-T3C2-COMMAND-ADVISOR-nc1-substitute/1/R20261113T215949Z-935f/nc1_identity_signal_substitute.py`) con **solo** el identificador y el ámbito cambiados. Registro `nc1 = SATISFIED_UNDER_PBT0_NC1_IDENTITY_SIGNAL_SUBSTITUTE`.
  - **Cualquier otra firma o variación vuelve al Owner antes de cualquier sustituto** ([DP]:9). Por ejemplo:
    - PD-F4-05 y PD-F4-06;
    - nc2 con Ci o Trailer en fail además (el caso de `f4-dispositions.md:1227`);
    - cualquier nc1 que no reproduzca la firma literal (el modo de `:1223-1226`);
    - **cualquier fallo de nc3**.
  - **Cómo se decide la coincidencia:** comparando la salida del control con el texto literal, nunca por el nombre de la comprobación. Si hay duda, el Owner.
  - La sesión anota cada uso en `f4b-dispositions.md`. No se copia el texto de [T1B-C]:368 (OD-4).
- **PBT0-C12:** una corrección sería el intento 3. Vuelve al Owner.

### 6.8 `EligibleCells`

Las de [T1B-C] (`:481-515`), vigentes en `model-catalog.md`:
- **Worker:** `anthropic/subagent/claude-sonnet-5-5`, effort `high` (catálogo `:48-61`, verificada el 2026-09-30).
- **Controller:** `openai/process/gpt-6-luna`, effort `high` (catálogo `:94-108`, verificada el 2026-09-30). En [T1B-C]:513 lleva `VerifiedOn` 2026-11-11 por la sonda neutra de F4 (M-05).
- **Vigencia:** las dos son `STALE` desde el 2026-12-29, por la fecha del catálogo + 90 días (`:14`).
- nc4 en la primera delegación, y nc1 a nc3.

## 7. Riesgos y lecciones de T1B aplicadas

| Id | Riesgo o lección | Tratamiento |
|---|---|---|
| R-01 | nc2 dio la firma de PD-F4-01 en casi todas las tareas de F4 | **Resuelto por [DP] (OD-5).** Con la firma exacta, el sustituto; con cualquier variación (como `f4-dispositions.md:1227`), el Owner (PBT0-C11) |
| R-02 | **La unión de existentes rotas del RED** (I-02) | Con MU-07 y MU-08 reducidos, la estimación declarada sale de las cotas de IRC1 §2.3 sin ellos: las cargas con `LoadExisting` (66 métodos de UI), la costura `SafetyDialog` (89) y `ConfirmModuleButton` (29), más los mutantes de Application. Es **del orden de decenas, con una cota superior de unos 250 resultados**, frente a los 3 de T1B. La cifra real la da la CI del RED. Se atribuye por análisis declarado (T-8). No es parada |
| R-03 | Un mutante hace fallar por excepción un método **nuevo** | Las alternativas; si no hay, PBT0-C10. Las existentes no cuentan (I-01) |
| R-04 | Las celdas vencen el 2026-12-29 | Se comprueba en la emisión y en cada delegación (A7) |
| R-05 | F4-E59 / I-02: las suites completas de UI escriben en el perfil | Solo los filtros de UI en local; la suite de Core sí, aislada por mutante (INV-PBT0-15, -20) |
| R-06 | F4-E59 / I-03: una sonda con entradas válidas | INV-PBT0-20 e INV-PBT0-06 |
| R-07 | MAX_PATH | Sin copias de árboles |
| R-08 | La adenda de anclas de T1B | T-6 obligatoria; la línea medida y el parche |
| R-09 | PB-T1 moverá el estado: los CT que usan costuras internas pueden quedar frágiles | Donde existe, se asevera sobre el contrato público y `LastComputation.Design`. T-7 lista las costuras. **Cómo trata PB-T1 estas pruebas lo decide su propio paquete** (M-15) |
| R-10 | La `LastComputation` del compuesto difiere de un recálculo (observación de T1B) | CT-02 la fija tal cual |
| R-11 | Core corre en Ubuntu | Claves, booleanos y umbrales declarados (INV-PBT0-11) |
| R-12 | El bloque M y la corrida de UI comparten el equipo | OD-6 (INV-PBT0-20) |

## 8. Decisiones para el Owner

**Ninguna nueva.**
- Las decisiones de procedimiento que estaban abiertas ya las tomó el Owner ([DP]). Este paquete las aplica sin generalizarlas: firmas literales, y cualquier variación vuelve al Owner.
- Las correcciones de IRC1 caben en [P] y en el patrón de T1B (IRC1 §2.8).

## 9. Cierre de la revisión v1 (IRC1)

| Id | Gravedad | Dónde queda resuelto en la v2 |
|---|---|---|
| I-01 | IMPORTANTE | §5, «La regla de la excepción»; INV-PBT0-02; PBT0-C10; R-03. Solo los métodos de los filtros nuevos; en las existentes, solo el cuelgue y el timeout |
| I-02 | IMPORTANTE | §5, «Las existentes rotas» y «El apoyo de la atribución»: análisis declarado como [T1B]:257, la suite completa de Core aislada por cada mutante de Application y la de UI solo en la CI (INV-PBT0-15, -20; T-8). MU-08 pasa a W:1138 (`+ 1` → `+ 2`). MU-07 pasa a CS:320-323, con la alternativa estrecha WC:119. R-02 con la estimación |
| I-03 | IMPORTANTE | H-16; CT-PB0-21: la firma de instancia con `MirroredX`, `MirroredY` y las claves de `DynamicParameters` en K1, y los valores de `DynamicParameters` en K4 y K4′ |
| I-04 | IMPORTANTE | H-03, con las secciones y `SafetyPerPostWindow` (`SetForTest`, `BuildResultForTest`, `Result`); INV-PBT0-10; T-7; CT-14 a CT-16 con su uso |
| M-01 | MENOR | §4.1: la clase (a) es `y == R3(x)`; no se copia `ClassOf` |
| M-02 | MENOR | MU-20 sin alternativa; `PushBackSideAnnotations.cs` sale de `MutantScope` y de `AllowedWriteScope` (16 rutas) |
| M-03 | MENOR | H-12; CT-PB0-13: el paso (i) se fija como hallazgo y el paso (ii) añade `PROTECTOR_LATERAL_BOTA_H_3_16_18` |
| M-04 | MENOR | Todas las anclas de [P], a `docs/initiatives/I-64-f4b-push-back-completo-package.md` en `67e28452` |
| M-05 | MENOR | CS:294-324, CS:522-544 y W:2716-2721 corregidas; §6.8 con los dos `VerifiedOn`; P-01 con la Snapshot I |
| M-06 | MENOR | §6.3: el algoritmo descrito y el script generador custodiado con el contrato (precondición 2) |
| M-07 | MENOR | CT-PB0-05: solo los seis campos, sobre pb-06 y pb-07; INV-PBT0-18 |
| M-08 | MENOR | CT-PB0-02: `IsEnabled` en la tupla; se pulsa solo si está habilitado; `DESHABILITADO` |
| M-09 | MENOR | CT-PB0-20: G8 = M3; G4 a G6 con los valores reales de pb-07 y pb-06 |
| M-10 | MENOR | CT-PB0-21: orden ordinal de las claves, emparejamiento por índice solo sin K1 ni K1o, K4′ con ε = 1e-9 e informativa, y «material» como fórmula mecánica; INV-PBT0-22 |
| M-11 | MENOR | §4.1 `Ventana()` con `newIdFactory` fijo; INV-PBT0-11; CT-PB0-11 asevera `RackId` |
| M-12 | MENOR | INV-PBT0-06 con el texto de [P]:598 y la lectura declarada |
| M-13 | MENOR | H-20 y CT-PB0-01: `WorkspaceDraftPilotMaterializationTests.cs:682`, `LegacyDeltas` (`:706`) y MB H-033 |
| M-14 | MENOR | H-10 y H-20: `RackModuleEditSessionTests.cs:381-399`, sobre la misma `RackModuleReconciliation` que usa DA (DA:36, :50) |
| M-15 | MENOR | R-09 sin la cita de OB-T1B.3 |
| M-16 | MENOR | §6.3, rutas completas |
| M-17 | MENOR | H-19 completa |
| M-18 | MENOR | CT-PB0-12 con `RackFrameProjectStore.DeepCopy` (H-11); CT-PB0-16: «la lista recibida sin cambios» |
| M-19 | MENOR | §4.3: la regla del último vínculo, con MA H-121 y el M-2b del anexo |
| M-20 | MENOR | Precondición 2 actualizada (OD-1) |
| M-21 | MENOR | §4.1, «Sin teclado ni foco real»; INV-PBT0-04 |
| OD-1 | Decisión del Owner | Precondición 2 y §1: [DP] custodiada como F4B-E03 en `F4B-DRAFTS/coordinator/owner-f4b-procedure-decisions.md`; sustituye a [P] §7.2 solo en la replanificación y en los sustitutos de nc1 y nc2 |
| OD-2 | Decisión del Owner | PBT0-C08; INV-PBT0-23 |
| OD-3 | Decisión del Owner | PBT0-C11: las firmas de PD-F4-01 y PD-F4-04 transcritas literales; los sustitutos por copia; cualquier variación, al Owner |
| OD-4 | Decisión del Owner | §6.6 sin «sin sustitutos»; remite a PBT0-C11; no se copia [T1B-C]:368 |
| OD-5 | Decisión del Owner | R-01 y §8 |
| OD-6 | Decisión del Owner | INV-PBT0-20 e INV-PBT0-24; el anexo del bloque M, corregido igual (P-0) |
