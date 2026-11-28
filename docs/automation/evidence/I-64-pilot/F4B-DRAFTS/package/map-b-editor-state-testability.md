# Mapa B — Estado editable de `RackPushBackSystemWindow` y qué se puede caracterizar (PB-T0)

> **Redaccion para el repo publico:** las rutas locales se sustituyen por marcadores (`<carpeta-del-paquete>`, `<worktree>`, `<carpeta-del-DLL-de-C2>`, `<datos-locales-del-usuario>`). El resto no cambia.

Base: HEAD `5ccaa770`, lectura de código sin compilar ni ejecutar. Cada hecho lleva su ancla `alias:línea`
(la leyenda da la ruta relativa al repo). No hay opiniones de diseño. Cuando un hecho sale de leer el código y no
de una prueba que lo mida, lleva la marca «(lectura)».

## Leyenda de alias

| Alias | Ruta relativa al repo |
|---|---|
| W | `src/RackCad.UI/Systems/PushBack/RackPushBackSystemWindow.xaml.cs` |
| WC | `src/RackCad.UI/Systems/PushBack/RackPushBackSystemWindow.Composite.cs` |
| WX | `src/RackCad.UI/Systems/PushBack/RackPushBackSystemWindow.xaml` |
| CS | `src/RackCad.Application/Systems/PushBack/PushBackCompositeEditorState.cs` |
| ES | `src/RackCad.Application/Systems/PushBack/PushBackEditorState.cs` |
| EL | `src/RackCad.Application/Systems/PushBack/PushBackEditorState.Load.cs` |
| EC | `src/RackCad.Application/Systems/PushBack/PushBackEditorCell.cs` |
| EI | `src/RackCad.Application/Systems/PushBack/PushBackEditorInputs.cs` |
| DA | `src/RackCad.Application/Systems/PushBack/PushBackEditorDesignAssembler.cs` |
| CA | `src/RackCad.Application/Systems/PushBack/PushBackCompositeEditorAssembler.cs` |
| DDA | `src/RackCad.Application/Systems/Dynamic/DynamicEditorDesignAssembler.cs` |
| DFM | `src/RackCad.Application/Systems/Dynamic/DynamicFrontMatrix.cs` |
| PM | `src/RackCad.Application/Systems/PushBack/Pilot/PushBackPilotMaterializer.cs` |
| PC | `src/RackCad.Application/Systems/PushBack/Pilot/PushBackPilotStateComparer.cs` |
| SES | `src/RackCad.UI/Editor/RackEditorSession.cs` |
| NF | `src/RackCad.UI/Controls/NumericField.cs` |
| RPC | `src/RackCad.Plugin/RackPushBackCommands.cs` |
| WLC | `tests/RackCad.UI.Tests/Workspace/WorkspaceDraftPilotWindowLoadCharacterizationTests.cs` |
| T-CL | `tests/RackCad.UI.Tests/PushBackClassicCharacterizationCloseTests.cs` |
| T-IN | `tests/RackCad.UI.Tests/PushBackClassicCharacterizationIntentTests.cs` |
| T-KB | `tests/RackCad.UI.Tests/PushBackClassicCharacterizationKeyboardTests.cs` |
| T-OW | `tests/RackCad.UI.Tests/PushBackClassicCharacterizationOwnerTests.cs` |
| T-RS | `tests/RackCad.UI.Tests/PushBackClassicCharacterizationResourceTests.cs` |
| T-SG | `tests/RackCad.UI.Tests/PushBackClassicCharacterizationSourceGuardTests.cs` |
| T-PG | `tests/RackCad.Tests/PushBackClassicCharacterizationPluginGuardTests.cs` |

---

## 0. El ciclo común que todo lo demás usa

- **H-001** El constructor (W:128-153) crea la sesión con `recompute: Recompute` y SIN catálogo (W:135); el catálogo es
  `session.Catalog` (W:136), que la sesión carga con `UiSupport.LoadCatalogSafe()` cuando no se le pasa uno (SES:83-85).
- **H-002** La ventana tiene DOS ensambladores de un sentido: `assembler` (W:137) y el que crea por dentro
  `compositeAssembler` (W:138 → CA:29-32). El diseño lo arma `compositeAssembler.BuildDesign` y lo resuelve
  `assembler.BuildFrom` (W:645-646); `assembler` también se usa en `AcceptComputation` (W:661), `Resolver` (W:293, W:310,
  W:3282, WC:893) y `AuthorizedSafety` (W:851).
- **H-003** El constructor termina en `LoadNew()` (W:152). `LoadNew` llama a `LoadFromModel` (W:272), que acaba en
  `Recompute()` (W:426), y después vuelve a llamar a `Recompute()` tras sembrar la seguridad por defecto (W:278-280). Una
  ventana nueva hace dos pasadas antes de mostrarse (lectura; el contador `RecomputePassesForTest` está en W:601).
- **H-004** `Recompute` (W:603-702): sale si `suppressSync` (W:605); con un campo numérico en error no toca el estado ni
  la sesión (W:611-619); si no, `CommitCurrentCell()` (W:621), arma y resuelve dentro de un `try` (W:630-656) y, si la
  computación es válida, ejecuta `session.SetModel` (W:660), `AcceptComputation(moduleOwner, …)` (W:661),
  `CommitDormantTailConsumption` (W:665), `lastComputation = computation` (W:666), `ClearModuleCommit` (W:682),
  `RefreshModuleSelector`/`RefreshHeaderDestinations` (W:683-684), `RefreshCompositePanel` (W:686) y fija
  `outputIsBlocked` (W:690).
- **H-005** `RequestRecompute()` es `session.Recompute.Request()` (W:594). La puerta es síncrona y, dentro de un
  `Defer()`, solo se queda la petición y la ejecuta una vez al cerrar el ámbito más externo
  (`src/RackCad.UI/Editor/RecomputeGate.cs:35-39`, `:51`, `:69-70`).
- **H-006** Tras `LoadExisting`/`LoadDesignForNew`, `LastComputation` ya es el resultado de una pasada por los controles
  (W:298/W:315 → W:426). WLC lee exactamente ese valor (WLC:72-78).

## 1. Propiedad del estado

### 1.1 Campos de la ventana con estado authored

- **H-007** `composite` (W:56-57) es un `PushBackCompositeEditorState` con dos `PushBackEditorState` nuevos. `state` es el
  lado ACTIVO (W:63) y `moduleOwner` es siempre el lado A (W:82).
- **H-008** Contenido de `composite` (CS): `SideA`/`SideB` (CS:161, CS:164), `ActiveSide` (CS:167), `ActiveSelection`
  (CS:179), `SideBPresent` (CS:245), `Gap` (CS:248), `CentralSeparator` (CS:251), `StructureOverrideA/B` (CS:254-257),
  `DefaultTopology` (CS:260, inicial `Encontradas`), `DefaultDirection` (CS:263), `topologies` (CS:38),
  `sideBEverDeclared` (CS:44), `DormantCompositeTail` (CS:58), `DormantTailLineOverrides` (CS:61) y
  `dormantTailConsumed` (CS:147).
- **H-009** Contenido de cada `PushBackEditorState` (ES): `structure` (`DynamicFrontMatrix`, ES:29), `pushFronts`
  (ES:30), `RearTopeSaque` (ES:47), `RearTopePieceId` (ES:55), `DefensePieceId` (ES:63), `workingBaseline` (ES:65),
  `moduleSession`/`moduleCommit`/`moduleSignature` (ES:110-112) y `LastModuleReconciliation` (ES:143). Cada celda Push
  Back guarda `HighEndBeamPeralte` (EC:18), `RearTopeEnabled` (EC:21), `PalletsDeepOverride` (EC:27) y `DrawPallet`
  (EC:33).
- **H-010** Carga de `composite`:
  - `LoadNew`: `SetActiveSide(A)`, `SetSideBPresent(false)`, `LoadComposite(null)` y `state.LoadNew()` (W:265-268).
  - `LoadDesignForNew`: `state.LoadFromDesign(SideAOnly(design))` y luego `LoadCompositeFromDesign(design)` (W:291-294).
  - `LoadExisting`: el mismo orden (W:308-311).
  - `Restore_Click`: el mismo camino con `preserveDormantIntent: true` (W:3280-3287).
- **H-011** `SideAOnly` (W:334-365) devuelve el MISMO objeto para un diseño no compuesto (W:336-339). Para uno compuesto
  copia `Structure` recortada a la del lado A (`PushBackCompositeStructure.SideAStructure`, W:349),
  `LegacyHighEndBeamPeralte`, `RearTope`, `DefensePieceId` y `Fronts` (W:350-362). Deja fuera `SideB` y `Composite`,
  que son las únicas otras propiedades de `PushBackDesign` (`src/RackCad.Domain/Systems/PushBack/PushBackDesign.cs:24-63`).
- **H-012** `ActiveSide` y `ActiveSelection` son contexto de interfaz: toda carga los devuelve a A (W:308-309 → CS:294-302)
  y la instantánea compuesta no guarda `ActiveSelection` (CS:1117-1134).
- **H-013** Lectura y escritura de `composite`: `Recompute` → `compositeAssembler.BuildDesign(composite, ReadInputs())`
  (W:645-646). El lado A va por `DA.BuildDesign(state.SideA, …)` (CA:59-64), `design.SideB = state.BuildSideB()` (CA:52,
  CA:65), `design.Composite = state.BuildComposite()` (CA:66), las ranuras ausentes de A y de B (CA:71-87) y
  `ApplyEffectiveCompositeness` (CA:89).
- **H-014** Lo que `DA.BuildDesign` escribe de cada lado:
  - el peralte posterior por celda (DA:417);
  - `PalletsDeepOverrides` solo si son ≥ mínimo (DA:418-422);
  - `DrawPallets` solo el `true` (DA:425);
  - en `RearTope.OffCells`, solo las celdas desactivadas (DA:426-429);
  - `RearTope` con `Saque`/`PieceId` (DA:389) y `DefensePieceId` (DA:390);
  - `DefaultPalletsDeep` (DA:411-413) y la envolvente en `Structure.Fronts[i].PalletsDeep` (DA:396-399);
  - `LegacyHighEndBeamPeralte` = constante 3.5 (DA:388; también en el lado B, CS:1057).
- **H-015** Cada armado MUTA el estado: normaliza los peraltes posteriores de las celdas contra el catálogo (DA:155 →
  ES:259-268 → EC:73-104) y reemplaza un `RearTopeSaque` ≤ 0 por el valor por defecto (DA:156-159).
- **H-016** `safetySelections` (W:84). Cargas:
  - `LoadFromModel` la vacía y la llena con copias profundas de `inputs.SafetySelections` (W:410-414), que en una carga
    salen de la seguridad del sistema RESUELTO (EL:217-223);
  - `LoadNew` la sustituye después por `PushBackSafetyAuthority(catalog).Defaults()` (W:278-279).
  
  Escrituras: `Safety_Click` la reemplaza por la lista autorizada (W:2626-2633) y `ApplyBootSections`/`ApplyDefenseSections`
  mutan sus elementos en sitio (W:2742-2780, W:2874-2969). Lectura y salida: `ReadInputs` pasa por
  `assembler.AuthorizedSafety` (W:851-854); el ensamblador la autoriza otra vez (DA:367) y la copia al diseño
  (DDA:122). Para `pb-06-seguridad` la ventana y el piloto no difieren en la seguridad: el único delta es
  `DimensionStyle` (WLC:191).
- **H-017** `generalPallet` (W:101). Carga: `LoadFromModel` (W:384-385); un `Pallet` nulo pasa a 42/48/60/1000 kg
  (W:384). Lectura: `ReadInputs` toma `Front`, `Height` y `Weight` de `generalPallet` sin pasar por controles, `Depth` de
  `DepthBox` y la unidad de `WeightUnitBox` (W:818-820). Ninguna otra línea de la ventana escribe `generalPallet`
  (lectura: solo aparece en W:101, W:385 y W:819-820).
- **H-018** `advanced` (W:106) lleva los seis ámbitos avanzados del rack:
  - Carga: `LoadFromModel` los copia de `inputs` (W:395-400), y `RecoverInputs` los saca del diseño
    (EL:215 → `src/RackCad.Application/Systems/PushBack/PushBackAdvancedRackParameters.cs:129-142`).
  - Pintado: `LoadAdvancedRackParameters` (W:1555-1579).
  - Relectura desde los controles: solo en `AdvancedRackParameter_Changed`, con `LostFocus` o `Click` (W:1592-1607,
    W:1616-1626; WX:492-531).
  - Restaurar: `RestoreRackParameters_Click` (W:1628-1634).
  - Salida: `ReadInputs` los copia tal cual (W:830-835) y el ensamblador los aplica (DA:190-193, DA:305).
- **H-019** `firstLevelDatum` (W:372; valor inicial `LowestUsablePunch`). Carga: `LoadFromModel` (W:383) con
  `inputs.FirstLevelDatum`. En una carga, ese valor sale del documento (EL:225) y `MigrateFirstLevelDatum` (EL:91-124) lo
  reexpresa a `LowestUsablePunch` reescribiendo `FirstLevelHeight` de cada frente. Lectura: `ReadInputs` (W:827).
  Salida: `sharedDesign.FirstLevelDatum` (DA:383). En WLC, `pb-10-sin-datum` no tiene delta (WLC:198).
- **H-020** `loadedDimensionViews` (W:1216) y `dimensionViewsTouched` (W:1220). Carga: `ShowDimensionViews`, llamada desde
  `LoadFromModel` (W:408, W:1228-1244). Escritura: el gesto del usuario (W:1260-1265). Lectura:
  `SelectedDimensionViews` → `DimensionViewPolicy.FromEditor` (W:1251-1257), llamada desde `ReadInputs` (W:847).
  Salida: `design.DimensionViews` (DDA:120). Prueba: la política cargada sale exacta en el primer recálculo
  (`tests/RackCad.UI.Tests/PushBackDimensionViewsWindowTests.cs:164`).
- **H-021** `frontalSide` (W:3221). No se persiste:
  - Se escribe en `FrontalSide_Changed` (W:3237-3249) y `UpdateFrontalButtons` lo vuelve a A cuando el compuesto está
    apagado (WC:92-96).
  - Se lee en `BuildFrom(…, frontalSide)` (W:646), que decide qué cortes frontales se construyen
    (DA:485-488), y en `SelectedView`, que codifica la sección (W:3090-3093).
  - Llega a la petición como `InsertSection` (T-IN:147-180).
- **H-022** `isEditingExisting` (W:87): `true` solo en `LoadExisting` (W:313) y `false` en `LoadNew`/`LoadDesignForNew`
  (W:270, W:296). Habilita «Actualizar» (W:3527) y la guarda de `RequestDraw` (W:3331-3335).
- **H-023** `sourceProject` (W:91) se fija al cargar (W:269, W:295, W:312) y viaja en cada `PushBackInsertionRequest`
  (W:3318, W:3370, W:3375) y en el proyecto de biblioteca (`WithSourceMetadataFrom`, W:3467).
- **H-024** Identidad (`session.Identity`):
  - `Adopt(null, null)` en `LoadNew` (W:271), `Adopt(null, rackName)` en `LoadDesignForNew` (W:297) y
    `Adopt(rackId, rackName)` en `LoadExisting` (W:314).
  - `SetName` en cada `TextChanged` de `NameBox` fuera de `suppressSync` (W:1201-1205) y antes de pedir dibujo o
    guardar (W:3312, W:3365, W:3464).
  - `SetName` guarda el texto tal cual y `Adopt` normaliza el id en blanco a null
    (`src/RackCad.UI/Editor/RackEditorIdentity.cs:36-39`, `:61-65`).
- **H-025** `pendingHeaderConfiguration`/`pendingHeaderSourceName` (W:1998-1999) son el «origen» de cabecera de la sesión
  de interfaz. Se fijan en `StageHeaderConfiguration` (W:2009-2019) y no se persisten: lo que se escenifica va a
  `moduleOwner.ModuleSession` (W:2048-2049).

### 1.2 Campos derivados o solo de interfaz

- **H-026** Estos no son authored: `lastComputation` (W:92), `hasValidModel`/`currentInputsAreValid` (W:89-90),
  `outputIsBlocked` (W:3484), `lastCompositeSystem` (WC:785), `moduleIds` (W:1671), `headerTargetIds/Labels` y
  `headerLineIndexes/Labels` (W:2165-2168), `copySourceIds` (W:2344), `mixedOptional`/`mixedNow` (WC:391, WC:404),
  `blankToolTips` (W:533), `matrixCards` (W:110) y `previewSurface` (W:3144).
- **H-027** `FrontBox`, `PalletHeightBox` y `WeightBox` son espejos de la celda (W:558-564, W:672-677). Se validan
  (W:728-734) pero nunca se leen para el diseño (lectura: no aparecen en `ReadInputs` ni en `ReadCellValues`).
- **H-028** El `TextBlock` `SafetySummary` existe en el XAML (WX:464). Ningún archivo `.cs` versionado de `src` lo
  referencia (`git grep SafetySummary -- src` solo devuelve WX:464).

### 1.3 Valores que solo existen en un control y se leen en cada recálculo

- **H-029** Valores que `ReadInputs` (W:812-857) relee de los controles en CADA pasada:

  | Control | Carga | Lectura |
  |---|---|---|
  | `DepthBox` | W:386 | W:819 |
  | `WeightUnitBox` | W:387 | W:820 |
  | `PalletsDeepBox` | W:388 | W:821 |
  | `PostBox` | W:390 | W:836 |
  | `PostPeralteBox` | W:391 | W:837 |
  | `ToleranceBox` | W:389 | W:838 |
  | `BeamDepthBox` | W:392 | W:839 |
  | `NumberFrontsCheck`, `NumberLevelsCheck`, `DrawRackNameCheck` | W:403-405 | W:842-844 |
  | `AnnotationScaleBox` | W:406 | W:845 |
  | `DimensionsBox` | W:407 | W:846 |
  | Las tres casillas de cotas por vista | W:408 | W:847 |

  Todos llegan al diseño por `DDA.BuildDesign` (DDA:90-124).
- **H-030** Valores que `ReadCellValues(side)` (W:781-810) relee para la celda primaria y su frente, cargados en
  `LoadSelectedFront` (W:431-484):
  - del frente: `PositionsBox`, `LevelsBox`, `FondosBox`, `DepthStartBox` y `FirstLevelHeightBox`
    (carga W:450-454, lectura W:793-797);
  - de la celda: `CellPalletFrontBox`, `CellPalletHeightBox`, `CellPalletWeightBox` y `CellClearBox`
    (carga W:456-460, lectura W:798-801);
  - `CellInOutBeamBox` y `CellInOutPeralteBox` (W:461-462 → W:802-803);
  - `CellBeamLengthOverrideBox` (W:463 → W:804, vía `OptionalVal`, W:3625-3626);
  - `CellIntermediateBeamBox` y `CellIntermediatePeralteBox` (W:464-465 → W:805-806);
  - `RearPeralteBox` (W:467-468 → W:808).
- **H-031** Controles que solo lee su propio manejador, no el recálculo:
  - `CellFondoOverrideBox` (LostFocus → W:1338-1342 → W:1392) y `CellDrawPalletCheck` (W:1344-1348 → W:1414);
  - `GapBox` (LostFocus → WC:159-175), `CentralSeparatorCheck` (WC:177-186) y `StructureOverrideBox` (WC:242-257);
  - `CellTopologyBox`, `RunDirectionBox` y `TopologyScopeBox` (WC:190-225);
  - `FrontCountBox` (W:1071-1082) y `NameBox` (W:1201-1205);
  - las cajas avanzadas (W:1616-1626), `ModuleLengthBox` (W:1852-1873) y `DerivedPostLineHeightBox` (W:2087-2094);
  - `SideBPresentCheck` (WC:101-120) y `SideSelectorBox` (WC:122-155).

### 1.4 Ida y vuelta cargar→salir sin tocar nada

- **H-032** Para los 14 diseños de un solo sentido del corpus (8 canónicos y 6 legacy, WLC:29-39), el diseño de la ventana
  tras `LoadExisting` y el que escribe el piloto difieren solo en:
  - `DimensionStyle` en `pb-06` y `pb-07` (WLC:191-192);
  - el redondeo de `AnnotationScale` y de `Fronts[0].Levels[0].PalletHeight` en `pb-08` (WLC:193-197).
  
  Un delta de otra clase hace fallar la prueba (WLC:247-248).
- **H-033** Para los 8 canónicos, lo que el piloto persiste sin edición coincide byte a byte con el diseño interior del
  sobre (`tests/RackCad.UI.Tests/Workspace/WorkspaceDraftPilotCanonicalLineEndingTests.cs:46-84`). Combinando H-032 y
  H-033, en esos 8 la ventana devuelve el diseño authored salvo `DimensionStyle` y el redondeo. Es una deducción de dos
  pruebas; ninguna compara directamente la ventana con el sobre.
- **H-034** Los diseños compuestos `pb-20-ab` (con `SideB.IsPresent = true`) y `pb-21-cola` (de un sentido, con ids de
  cola `GAP` y `B:*` en `Structure.Modules`) están en `tests/RackCad.Tests/Fixtures/I64/`, pero no en el corpus de WLC
  (WLC:29-37, WLC:328). Ninguna prueba de UI los carga en la ventana; solo los usan pruebas Core del piloto
  (`tests/RackCad.Tests/Workspace/WorkspaceDraftPilotMaterializationTests.cs:111`, `:121`, `:337`).
- **H-035** La ida y vuelta de un compuesto por la ventana la fijan
  `tests/RackCad.UI.Tests/PushBackCompositeWindowTests.cs:1554` (presencia, topes y fondo de corrida), `:1606` (ida y
  vuelta del diseño compuesto) y `:1366-1418` (blanco de B tras RACKEDITAR), además de
  `tests/RackCad.UI.Tests/PushBackCompositeLoadExistingModulesTests.cs:105-142` (personalizaciones de A y B tras
  RACKEDITAR y tras guardar y reabrir).
- **H-036** «Actualizar» recalcula otra vez antes de pedir (W:3351), así que el diseño de la petición sale de esa segunda
  pasada. No hay prueba que compare el diseño entero de la petición con el `LastComputation.Design` de la carga.
  `tests/RackCad.UI.Tests/PushBackDimensionViewsWindowTests.cs:327-386` compara solo `DimensionViews` en Actualizar.

## 2. Módulos y cola aparcada

### 2.1 Representación de los módulos

- **H-037** Los módulos son del lado A (`moduleOwner`, W:82). La sesión de edición es un `RackModuleEditSession` que
  se siembra perezosamente desde `workingBaseline.Structure` (ES:124-135, ES:167-174). Confirmar crea `moduleCommit`
  (ES:146-150; `RackModuleEditSession.Commit`, `src/RackCad.Application/Systems/Shared/RackModuleEditSession.cs:699`) y
  cancelar descarta lo escenificado (ES:153).
- **H-038** Tras cada computación válida, `AcceptComputation` llama a `SetWorkingBaseline` (DA:101-107 → ES:77-81).
  `ReseedModuleSession` conserva la sesión si la firma «id:clase» no cambió (ES:176-186, ES:189-192). Después,
  `ClearModuleCommit` (W:682 → ES:165) y la lista del panel se rehace (W:1640-1669).
- **H-039** Cargar otro rack descarta la sesión, el commit y la reconciliación (`AdoptLoadedBaseline`, ES:99-106, llamado
  desde EL:43 y EL:68).
- **H-040** El ensamblador lleva las personalizaciones como el commit aceptado o, si no lo hay, como los módulos del
  baseline (DA:57-69). Las reconcilia por `ModuleId`+`Kind` (DA:315-325) y lleva las configuraciones por línea y las
  alturas de poste derivado (DA:332-365).

### 2.2 Cola aparcada («dormida»)

- **H-041** La cola son `DormantCompositeTail` y `DormantTailLineOverrides` (CS:58, CS:61). `ParkDormantTail` sustituye
  lo aparcado solo si la fuente trae algo; una fuente vacía no borra (CS:74-96). `ClearDormantTail` es el borrado
  explícito (CS:99-104).
- **H-042** Al cargar (`LoadCompositeFromDesign`, WC:802-909) siempre se ejecuta `ClearDormantTail` (WC:822) y
  `ParkDormantTail` con los módulos y líneas de `design.Structure` cuyo id es de cola (WC:835-843;
  `IsCompositeTailId`, `src/RackCad.Application/Systems/PushBack/PushBackCompositeStructure.cs:780`). Se aparca cualquier
  diseño cuya estructura traiga ids de cola, sea compuesto o no; si no los trae, el aparcado no hace nada (CS:80-83).
- **H-043** Con `preserveDormantIntent: true` (solo `Restore_Click`, W:3287), la cola dormida, sus líneas y la presencia
  por ranura de B se capturan antes (WC:804-810) y se reponen después del borrado (WC:823-833).
- **H-044** Despertar la cola:
  1. `CA.BuildDesign` abre un intento con `BeginDormantTailConsumptionAttempt` (CA:48 → CS:131).
  2. Si el diseño es efectivamente compuesto, pasa la cola a `DA.BuildDesign` (CA:59-64), que la anexa con
     `AttachDormantTail` solo si la secuencia no es ya compuesta (DA:268, DA:288, DA:535-571).
  3. `ApplyEffectiveCompositeness` repone las líneas aparcadas y marca el consumo (CA:158-172 → CS:116-117).
  4. La ventana confirma el consumo solo con una computación válida aceptada (W:665 → CS:134-142).
- **H-045** Dormir la cola: si el diseño no es efectivamente compuesto, `ParkTailOf` aparca la cola del baseline del lado A
  (CA:54-57, CA:180-242), proyectando encima el commit confirmado de la cola (CA:202-239). Después
  `ApplyEffectiveCompositeness` retira de la estructura los módulos y líneas de cola (CA:129-155).
- **H-046** `DA.BuildDesign` con `effectivelyComposite: false` también retira la cola de la copia del baseline (DA:279-287)
  y de las intenciones reconciliadas (DA:318-324).
- **H-047** Pruebas que fijan la cola:
  - `tests/RackCad.UI.Tests/PushBackDormantTailTransactionTests.cs:136-292` (8 métodos, incluido `:292`, «cargar otro
    rack borra la intención dormida»);
  - `tests/RackCad.UI.Tests/PushBackDormantRestoreTests.cs:143-301` (7);
  - `tests/RackCad.UI.Tests/PushBackCompositeLifecycleWiringTests.cs:119-283` (7).

### 2.3 Lo que copia o pierde el estado del piloto

- **H-048** `PushBackPilotDraftState` agrupa `Editor` (un `PushBackCompositeEditorState`), `Inputs` y `Origin`
  (`src/RackCad.Application/Systems/PushBack/Pilot/PushBackPilotDraftState.cs:15-32`).
- **H-049** `Materialize` (PM:126-181):
  - devuelve `OutOfPilot` para un diseño compuesto (PM:148) y para uno que deja cola aparcada (PM:169-170);
  - reproduce la parte Application de `LoadExisting` (PM:150-172), pero llama a `SideA.LoadFromDesign(design)`
    directamente, sin `SideAOnly`, que para un diseño no compuesto es el mismo objeto (W:336-339);
  - no reproduce `LoadFromModel` ni el primer recálculo (PM:174).
- **H-050** `PushBackPilotStateComparer.Copy` (PC:58-83) copia a través de `Snapshot()`/`Restore()` sobre un estado nuevo
  (PC:68-70). No copia `workingBaseline`, la sesión ni el commit de módulos, la firma, la reconciliación ni la cola
  dormida (PC:53-56). Esto coincide con lo que guardan las instantáneas (ES:477-486; CS:1117-1134). Repone aparte
  `ActiveSelection` (PC:74-79) y `ActiveSide` (PC:81).
- **H-051** El comparador ignora la autoridad de módulos y la selección. Compara `Snapshot()` y las entradas (PC:134-149,
  PC:172-177, PC:251-278) y no lee `ActiveSelection` (PC:138-148).
- **H-052** `ComposeForWrite` (PM:196-234):
  - rechaza si el estado actual difiere del inicial en algo distinto de `AnnotationScale` (PM:212-215);
  - vuelve a materializar desde `initial.Origin` para tener una autoridad de módulos fresca (PM:217);
  - rechaza si la base cambió (PM:223-224);
  - aplica solo la escala (PM:227).
- **H-053** `KeepPilotEditsOn` (PM:242-251) devuelve `Copy(newInitial)` con la escala del estado actual. Por H-050, ese
  resultado nace sin baseline, sin módulos y sin cola (lectura).

## 3. A/B

- **H-054** La casilla «Rack de dos sentidos (lado B)» es `SideBPresentCheck` (WX:127-131). Su manejador
  `SideBPresent_Changed` (WC:101-120) llama a `composite.SetSideBPresent`, colapsa o muestra la sección
  (`UpdateCompositeEnabled`, WC:65-71) y recalcula.
- **H-055** `SetSideBPresent(true)`, la primera vez (`sideBEverDeclared == false`), ejecuta `DeclareSideBCapability`
  (CS:352-386):
  - `SideB.LoadNew()` (CS:360);
  - `AlignSlotGrid()` (CS:363);
  - `AlignSideBFirstLevel()` (CS:376);
  - pone TODOS los frentes de B con `IsActive = false` (CS:381-385).
  
  Si B vuelve de estar dormido, conserva su configuración y su presencia (CS:364-374).
- **H-056** Retirar B (`SetSideBPresent(false)`) devuelve el lado y la selección activos a A (CS:298-302) y no borra la
  configuración de B (CS:290-293).
- **H-057** `DefaultTopology` empieza en `Encontradas` (CS:260). `SetSideBPresent` lo cambia de `SoloA` a `Encontradas`
  al declarar B, y de `Encontradas` a `SoloA` al retirarlo; una elección distinta no la toca (CS:316-323).
  `LoadComposite(null)` lo fija según `SideBPresent` (CS:986). `TopologyAt` degrada la topología guardada según los lados
  presentes en la celda (CS:664-671, CS:905-913).
- **H-058** `BuildSideB` (CS:1024-1092) devuelve null si `!SideBPresent` (CS:1026-1029) o si ninguna ranura de B está
  presente (CS:1038-1047). Con eso el diseño no es compuesto (`IsComposite`,
  `src/RackCad.Domain/Systems/PushBack/PushBackDesign.cs:70`) y `CA.BuildDesign` duerme la cola (CA:52-57).
- **H-059** La presencia por ranura se deriva de «En blanco» (`IsSlotPresent`, CS:428-437) y se escribe con
  `SetSlotPresent` → `SetActive(…, allowAllBlank: OtherSideHasAnyFront)` (CS:443-454, CS:457-475; ES:238-247).
- **H-060** `AbsentSlotsOfA/B` (CS:1095-1112) se escriben en `design.Composite.AbsentSlotsA/B` (CA:71-87). Las ranuras en
  blanco de B viajan completas en `SideB.Fronts` (CS:1062-1069).
- **H-061** Al cargar, `LoadComposite` relee `AbsentSlotsA/B` (CS:1001-1009). `LoadCompositeFromDesign` reconstruye B con
  su propio diseño (WC:845-893) y vuelve a aplicar los blancos después (WC:873-901). Termina con `AlignSlotGrid`
  (WC:906; CS:552-575).
- **H-062** Pruebas que fijan A/B:
  - `tests/RackCad.Tests/PushBackBlankFrontAuthorityTests.cs` (4 Fact y 2 Theory con 5 `InlineData`, 9 casos): presencia
    derivada (`:53-67`), cualquier lado activo (`:70-85`), un lado entero en blanco deja el sistema no compuesto
    (`:88-103`), el rack entero no puede quedar en blanco (`:106-113`), el blanco quita las camas y conserva la retícula
    (`:119-136`) y la ida y vuelta (`:139-156`);
  - `tests/RackCad.Tests/PushBackPartialCompositeTests.cs:112-130` («declarar la capacidad no hace compuesto ningún
    frente») y `:132-151`;
  - `tests/RackCad.Tests/PushBackCompositeEditorTests.cs:99-121` (`BuildSideB` con ranura en blanco y `AbsentSlotsOfB`);
  - `tests/RackCad.UI.Tests/PushBackCompositeWindowTests.cs:81-105` (la casilla abre la sección y el sistema sigue no
    compuesto hasta declarar un frente) y `:108-138` (apagar y encender conserva B);
  - `tests/RackCad.UI.Tests/PushBackCompositeLifecycleWiringTests.cs:171` (B declarado sin ranuras efectivas).

## 4. Gestos de estructura, módulos, nombre y «Restaurar estándar»

- **H-063** «+ frente»/«- frente» (WX:700-701 → W:1137-1141) y el número de frentes (`FrontCountBox` LostFocus,
  W:1071-1082) llaman a `composite.SetSlotCount` (CS:522-544). Crece en los dos lados si B está declarado y las ranuras
  nuevas nacen ausentes en el lado no activo (CS:538-543).
- **H-064** «+ nivel»/«- nivel» (WX:702-703 → W:1144-1148) ejecutan `AdjustLevels` sobre los lados de la selección de
  edición (`ForEachEditedSide`, W:753-767).
- **H-065** La casilla «En blanco» de cada columna se crea en `RenderPushBackMatrix` (W:934-947), sin `x:Name`, y llama a
  `SetFrontActive` (W:1089-1133). Ese método pasa por `composite.SetSlotPresent` en los lados editados y rehúsa dejar el
  rack entero en blanco (W:1116-1123).
- **H-066** Toda mutación estructural pasa por `MutateStructure` (W:1175-1197), que no hace nada si hay un campo en error
  (W:1178). Si no lo hay: `CommitCurrentCell()` (W:1181), la mutación, el repintado bajo `suppressSync` y UN recálculo
  diferido (W:1179, W:1195).
- **H-067** No hay gesto de añadir o quitar módulos. La secuencia de módulos solo cambia cuando el ensamblador reconstruye
  (DA:216-270, `BuildDefault` en DA:218).
- **H-068** Gestos de módulo, todos escenificados en la sesión y aplicados solo con «Confirmar» (W:2454-2464):
  - longitud (W:1852-1873);
  - «Calculada» (W:1875-1890);
  - configurar cabecera (W:1897-1947);
  - copiar de otra cabecera (W:1954-1986);
  - aplicar a la selección (W:2021-2071);
  - altura de poste derivado por línea (W:2078-2108);
  - «Restaurar módulo» (W:2479-2490);
  - «Restaurar estándar» (W:2497-2501).
  
  «Cancelar» descarta (W:2466-2477).
- **H-069** Renombrar: solo existe el nombre del rack (`NameBox`, WX:47; W:1201-1205). Los nombres de módulo son
  ordinales derivados por clase, no editables (W:1700-1746). El renombrado de bloques y el nombre automático de un rack
  nuevo los hace el Plugin después de cerrar la ventana (RPC:187-190, RPC:282, RPC:300-309, RPC:339-342;
  T-PG:224-229).
- **H-070** «Restaurar estándar» → `ModuleSession.RequestStandardRestore()` (W:2499;
  `src/RackCad.Application/Systems/Shared/RackModuleEditSession.cs:682`) exige «Confirmar». En el ensamblador, el commit
  activa `standardRestore` (DA:204), que:
  - fuerza la reconstrucción (DA:211);
  - no lleva cola (DA:229-231) ni intenciones (DA:315-317);
  - no lleva configuraciones por línea (DA:334);
  - ejecuta `PushBackAdvancedRackParameters.Reset(editorInputs)` (DA:300-303).
  
  Prueba: `tests/RackCad.UI.Tests/PushBackModuleEditorWindowTests.cs:279-302`.
- **H-071** El `editorInputs` que reinicia DA:302 es el objeto nuevo que construye `ReadInputs` en cada pasada (W:814).
  Ningún camino de «Restaurar estándar» toca el portador `advanced` de la ventana (W:106), cuyas únicas escrituras son
  W:395-400, W:1599-1606 y W:1630. Tras `ClearModuleCommit` (W:682), el siguiente `ReadInputs` vuelve a copiar
  `advanced` (W:830-835) (lectura; no medido). La única prueba que pulsa `RestoreAllModulesButton`
  (`PushBackModuleEditorWindowTests.cs:295`) observa solo `IsManualOverride`.
- **H-072** «Restaurar» (barra de acciones, W:3267-3290) recarga desde `lastComputation.Design`, que es el último diseño
  VÁLIDO y ya pasó por los controles. Mantiene `NameBox.Text` (W:3288).
- **H-073** Clasificación de cambios estructurales (DA:198-213):
  - `baseline = state.WorkingBaseline?.Structure` (DA:198);
  - `commit = state.ModuleCommit` y `restoredIds` (DA:202-203);
  - `standardRestore = forceRebuild || commit.StandardRestoreRequested` (DA:204);
  - `rebuildShape` = la estructura local del lado A de un baseline compuesto, o `baseline` (DA:210);
  - `mustRebuild = standardRestore || restoredIds.Count > 0 || DynamicEditorDesignAssembler.MustRebuild(rebuildShape,
    pallet, depthLayout)` (DA:211-213).
- **H-074** `DynamicEditorDesignAssembler.MustRebuild` (DDA:44-48) responde true si no hay sistema, si no hay módulos, si
  la tarima cambia (`SamePallet` compara Front, Depth, Height y Weight con 1e-6 y no compara `WeightUnit`, DDA:50-61) o si
  el layout de fondos no coincide (`DynamicDepthGeometry.Matches`). El layout sale de la envolvente por frente y de
  `PalletsDeep` (DA:182-184).
- **H-075** Sin reconstrucción se copia el baseline (DA:273, DA:508-529) y se actualiza en sitio la altura de las
  cabeceras calculadas (DA:274 → DDA:65-84).

## 5. Pérdidas conocidas de la carga clásica

Estas pérdidas están registradas en `docs/ideas-futuras.md:1722-1736`:

- `:1722`: el encabezado de la sección;
- `:1726-1728`: (a);
- `:1730-1732`: (b), que cita `ReadInputs` en W:840-848;
- `:1734-1736`: (c) y (d), que dice que el corpus de F4-T2A no provoca ningún delta de esas clases.

La prueba es WLC, con la clasificación `ClassOf` en WLC:213-229.

### (a) Redondeo de `NumericField` a «0.###»

- **H-076** `NumericField.SetNumber(double?, string format = "0.###")` escribe el texto con `CurrentCulture` (NF:131-136).
  `Value` sale de volver a analizar ese texto (NF:138-163).
- **H-077** Cuándo: en el primer `Recompute` tras `LoadFromModel` (W:426) y en cada pasada siguiente, se releen
  redondeados los valores de H-029 con decimales (`DepthBox`, `PostPeralteBox`, `ToleranceBox`, `BeamDepthBox`,
  `AnnotationScaleBox`). Los de H-030 se releen así para la celda primaria y su frente (W:797-801, W:804). Los enteros
  pasan por `IntVal` (W:3628).
- **H-078** Qué se pierde: los dígitos a partir del cuarto decimal. En `pb-08-redondeo`, `AnnotationScale` pasa de 1.23456
  a 1.235 y `Fronts[0].Levels[0].PalletHeight` de 61.23456 a 61.235 (WLC:193-197, WLC:254-264).
- **H-079** No pasan por el redondeo en el recálculo:
  - `Front`, `Height` y `Weight` de `generalPallet` (W:819);
  - `advanced`, salvo que el usuario salga de su caja (W:830-835 frente a W:1592-1607);
  - `Gap`, salvo `LostFocus` en `GapBox` (WC:159-175; pintado en WC:721);
  - la longitud de módulo, salvo `LostFocus` con una diferencia mayor que 0.0001 (W:1865-1871);
  - las celdas y frentes no seleccionados (lectura).

### (b) `ReadInputs` pierde `DimensionStyle`

- **H-080** `DynamicAnnotationOptions` tiene `DimensionStyle`
  (`src/RackCad.Application/Systems/Dynamic/DynamicAnnotationOptions.cs:21`).
  - `RecoverInputs` lo recupera (EL:209).
  - `LoadFromModel` no lo lee (W:402-408).
  - `ReadInputs` construye `Annotations` sin él (W:840-848).
  - `DDA.BuildDesign` escribe `design.DimensionStyle = options.DimensionStyle`, es decir null (DDA:121).
- **H-081** Cuándo: en el primer recálculo de la carga. Qué se pierde: la cadena del estilo, por ejemplo «STANDARD-ISO»
  (`pb-06`) y «SIMPLEX» (`pb-07`) (WLC:191-192).
- **H-082** Fijado también por `tests/RackCad.UI.Tests/PushBackDimensionViewsWindowTests.cs:307-322`
  (`T19_H1_TheDimensionStyleStillComesOutNull`). La ventana Push Back no tiene `SetDimensionStyles`: `PushBackEditorModule`
  no lo llama (`src/RackCad.UI/Editor/EditorModules.cs:112-127`), mientras que el módulo Dinámico sí
  (`EditorModules.cs:76`, `:84`).

### (c) Ajustes de `LoadFromModel` y `LoadSelectedFront`

- **H-083** Sustituciones en `LoadFromModel`:
  - `Pallet` nulo → 42/48/60/1000 kg (W:384); `RecoverInputs` hace lo mismo (EL:230-232);
  - unidad en blanco → «kg» (W:387). `WeightUnitBox` solo ofrece «kg» y «lb» (W:140); con otra unidad no queda selección
    y `ReadInputs` escribe «kg» (W:820) (lectura);
  - `PalletsDeep` → `Max(2, …)` (W:388);
  - tolerancia ≤ 0 → valor por defecto (W:389);
  - poste en blanco → `catalog.Defaults.Post` (W:390);
  - `BeamDepth` ≤ 0 → valor por defecto (W:392);
  - escala ≤ 0 → 1.0 (W:406);
  - `Dimensions` acotado a `[0, Detailed]` (W:407).
  
  En una carga, `RecoverInputs` ya aplica las de tolerancia, `BeamDepth`, escala y `PalletsDeep` (EL:196-206).
- **H-084** `PostBox.SelectedId` asigna `SelectedValue` (`src/RackCad.UI/Controls/CatalogCombo.cs:24-28`). Un id que no
  está entre los postes del catálogo deja `SelectedId` en null, por la semántica de `Selector` de WPF (lectura, no
  medido); el ensamblador lo sustituye entonces por `catalog.Defaults.Post` (DA:169-171). `pb-12-poste-nulo` no tiene
  delta (WLC:200).
- **H-085** Combos de peralte de la celda: `SetPeralteOptions` (W:3580-3590) ofrece `AllowedPeraltes(catalog, beamId)`
  (`src/RackCad.Application/Systems/Dynamic/DynamicRackLevelGeometry.cs:172-174`). Si no hay ninguno, ofrece un único valor
  por defecto (W:3582-3586). Si el valor de la celda no está en la lista, selecciona el índice 0 (W:3588-3589), y
  `ReadCellValues` lee ese valor (W:803, W:806). El resolver usa otra regla de reserva: el valor preferido si está
  permitido y, si no, el primero
  (`src/RackCad.Application/Systems/Dynamic/DynamicRackLevelGeometry.cs:207-223`).
- **H-086** Combos de viga: si el id de la celda no está entre `CompatibleInOutBeams`/`CompatibleIntermediateBeams`
  (W:148-149, W:461, W:464), no hay selección y `ReadCellValues` conserva el id de la celda (W:802, W:805) (lectura).
- **H-087** Peralte posterior: la lista de `RearPeralteBox` está fija en el código: 3.0 a 6.0 de 0.5 en 0.5 (W:44, W:144).
  Un valor fuera de la lista se sustituye por `PushBackDefaults.HighEndBeamDefaultPeralte` = 3.5 (W:467-468;
  `src/RackCad.Domain/Systems/PushBack/PushBackDefaults.cs:25`), y eso es lo que se relee (W:808). El catálogo entregado
  permite exactamente 3;3.5;4;4.5;5;5.5;6 para `LARGUERO_ESCALON_TROQUEL_REDONDO` (`assets/catalogs/secciones.csv:10`). La
  carga normaliza contra esa lista (resolver `AllowedHighEndPeraltes`,
  `src/RackCad.Application/Systems/PushBack/PushBackResolver.cs:525-532`), así que con el catálogo entregado las dos
  listas coinciden. La sustitución solo podría darse con un catálogo que permita otros valores (lectura).
- **H-088** En el corpus de WLC no aparece ningún delta de clase (c) (WLC:206-212; `docs/ideas-futuras.md:1736`).

### (d) `CommitCurrentCell` reescribe la celda en cada recálculo

- **H-089** `CommitCurrentCell` (W:737-741) se llama:
  - en cada `Recompute` (W:621);
  - al cambiar la celda (`SelectMatrixCell`, W:1061; `SelectSingleCell`, W:1166);
  - en `MutateStructure` (W:1181).
  
  Escribe `ReadCellValues(side)` con `CommitEditorValues` (ES:272-286 → DFM:277-285). Los valores del frente van al
  frente seleccionado y los de la celda a la celda primaria; el peralte posterior va a la celda Push Back paralela
  (EC:51-54).
- **H-090** Tras una carga la selección es (0,0) (EL:181), así que el primer recálculo reescribe el frente 0 y la celda
  (0,0) desde los controles. Les alcanzan (a) y (c). El delta de `pb-08` en `PalletHeight` está justo en
  `Fronts[0].Levels[0]` (WLC:196).
- **H-091** Con «Ambos lados», la escritura alcanza a los dos lados (W:753-767). Un campo vacío por estado mixto cae al
  valor del lado que se escribe (W:781-810, W:3625-3626; WC:420-478). Pruebas:
  `tests/RackCad.UI.Tests/PushBackEditorSideOwnershipTests.cs:247-347`.

## 6. Actualizar frente a cerrar

- **H-092** `RequestDraw` (W:3323-3379), en este orden:
  1. guarda `canInsertInAutoCad` (W:3325-3329);
  2. guarda «solo RACKEDITAR» (W:3331-3335);
  3. confirma el módulo escenificado (W:3346-3349);
  4. `RequestRecompute()` (W:3351);
  5. validez (W:3352-3356);
  6. bloqueo tras el recálculo (W:3360-3363);
  7. `SetName` (W:3365) y `SetModel` (W:3366);
  8. `RequestUpdate` (W:3369-3370) o `RequestInsert` (W:3374-3375);
  9. `Close()` (W:3378).
- **H-093** La intención se anota en la sesión. `Complete` asegura el id, fija `UpdateOnly`, pone `InsertView`/
  `InsertSection` a null/-1 en una actualización, construye `InsertionRequest` y fija `InsertRequested = true`
  (SES:176-195). `RequestInsertViews` hace lo mismo para el lote (SES:145-170).
- **H-094** `OnClosing` (W:3395-3404) cancela el cierre solo si NO hay petición y `EditorClosePolicy.MayClose` responde
  false (`src/RackCad.UI/Shell/EditorClosePolicy.cs:108-113`). `PendingWork` declara solo los cambios de módulo sin
  confirmar (W:3417-3419). `Close_Click` es `Close()` (W:3381) y `CloseButton` tiene `IsCancel="True"` (WX:789).
- **H-095** Un cierre sin petición (X, Alt+F4, Escape o «Cerrar») no recalcula y pierde en silencio el texto sin confirmar
  (T-CL:665-711). Dentro de `ShowDialog`, «Cerrar» y Escape preguntan dos veces ante un «No» (T-CL:473-512).
- **H-096** El Plugin solo lee la petición después de `ShowModalWindow`. RACKPUSHBACK lee `InsertionRequest`
  (RPC:47-65) y RACKEDITAR lee `InsertRequested`, `DesignToInsert`, `SystemToInsert`, `UpdateOnly` e
  `InsertionRequest.Views` (RPC:168-193, RPC:248-254).
- **H-097** «Insertar varias…» (W:3294-3321) confirma el módulo, recalcula y comprueba la validez y el bloqueo. Después
  arma las opciones (W:3300-3310), abre el diálogo (W:3311), pide `RequestInsertViews` (W:3314-3319) y cierra (W:3320).
  No mira `canInsertInAutoCad` (T-SG:377-378), y `UpdateButtons` no lo deshabilita nunca (W:3523-3570; T-IN:330-348).
- **H-098** «Guardar en biblioteca» (W:3441-3475) no escribe en el dibujo ni marca petición:
  1. recalcula y comprueba la validez y el bloqueo;
  2. abre el `SaveFileDialog` (W:3458);
  3. ejecuta `EnsureId` (W:3463), que acuña un GUID si no lo había;
  4. `SetName` (W:3464);
  5. escribe el archivo `.rackcad.json` con `RackProjectStore().Save(project, path)` (W:3467-3468).
  
  `PromptSaveToLibrary` también crea la carpeta de biblioteca (`src/RackCad.UI/UiSupport.cs:66-68`).
- **H-099** «BOM» (W:3423-3439) recalcula, comprueba validez y bloqueo y abre `RackBomWindow` en modal (W:3438). La
  ventana Push Back no escribe nada. `RackBomWindow` puede exportar XLSX o CSV a un archivo que elige el usuario
  (`src/RackCad.UI/RackBomWindow.xaml.cs:37-80`).

## 7. Subdiálogos

- **H-100** Configurador de cabecera (`RackFrameConfiguratorWindow`):
  - se abre con `{ Owner = this }` (W:2426) y `ShowDialog` (W:2443);
  - costuras: `HeaderConfiguratorPresenter` (W:2410, W:2437-2440) y `HeaderConfiguratorDialog`, que evita crear la
    ventana (W:1525, W:1938-1940);
  - devuelve `window.Configuration ?? copy` (W:2446) → `StageHeaderConfiguration` (W:1946, W:2009-2019), escenificado en
    la sesión de módulos (W:2048-2049);
  - en `OnClosed` escribe su disposición en `<datos-locales-del-usuario>` (T-SG:343-352).
- **H-101** Seguridad (`SelectiveSafetyWindow`):
  - se abre con `Owner = this` (W:3016) y `ShowDialog` (W:3018);
  - costuras: `SafetyDialog`, que sustituye la ventana entera (W:1493, W:2615-2617), y `SafetyWindowDialog`, que solo
    sustituye `ShowDialog` (W:3028, W:3018);
  - devuelve `dialog.Result` → autorización y sustitución de `safetySelections`, más las secciones de botas, defensa y
    topes de cada lado (W:2626-2657);
  - cancelar no cambia nada ni recalcula (W:2618-2621; T-IN:352-378).
- **H-102** Botas por poste (`SafetyPerPostWindow`): `Owner = this` (W:2721), costura `BootPerPostWindowDialog`
  (W:2700, W:2722). Devuelve colocaciones a la sección (W:2728-2734), que solo se aplican si se acepta Seguridad
  (W:2642). El dueño es la ventana Push Back aunque Seguridad esté abierta (T-OW:112-159).
- **H-103** Defensa por poste (`SafetyDefensaGridWindow`): `Owner = this` (W:2865), costura `DefenseDialog` (W:1505,
  W:2849). Devuelve registros por poste a la sección (W:2867); se aplican en `ApplyDefenseSections` (W:2638,
  W:2874-2920).
- **H-104** Topes traseros (`SafetyTopeGridWindow`): `Owner = this` (W:3047), costura `RearTopeDialog` (W:1499, W:2981).
  Devuelve `TopeResult` a la sección. Al aceptar Seguridad se aplica con `LoadRearTopeConfig` en cada lado (W:2646-2654
  → ES:454-471).
- **H-105** BOM (`RackBomWindow`): `{ Owner = this }.ShowDialog()` (W:3438), sin costura. No devuelve nada.
- **H-106** Lote de vistas: `RackViewBatchDialogPresenter.TryShow(this, …)` (W:3311), que crea el diálogo con
  `Owner = owner` (`src/RackCad.UI/Views/RackViewBatchDialogPresenter.cs:52-63`). Es estático y sin costura. Devuelve la
  lista de vistas que va a `RequestInsertViews`.
- **H-107** Guardar archivo: `UiSupport.PromptSaveToLibrary(this, …)` (W:3458) → `dialog.ShowDialog(owner)`
  (`src/RackCad.UI/UiSupport.cs:81`), sin costura. Devuelve la ruta o null.
- **H-108** Aviso de cierre: `EditorDiscardPrompt.Confirm(question, owner = null)`
  (`src/RackCad.UI/Shell/EditorClosePolicy.cs:54`). Muestra `MessageBox` sin dueño cuando `owner` es null
  (`EditorClosePolicy.cs:77-79`). Su costura es `Substitute` (`EditorClosePolicy.cs:57-72`) y la prueba confirma que no
  tiene dueño WPF (T-CL:186).
- **H-109** La ventana misma no tiene dueño cuando la abren RACKPUSHBACK y RACKEDITAR (`ShowModalWindow`, RPC:47-48,
  RPC:168-170; T-OW:49-74; T-PG:207). Desde el menú y la biblioteca la abren con `{ Owner = context.Owner }`
  (`src/RackCad.UI/Editor/EditorModules.cs:114`, `:121`).

## 8. Dependencias de catálogo e infraestructura de pruebas

- **H-110** El catálogo se carga una vez al construir la ventana:
  - `LoadCatalogSafe` → `JsonRackCatalogProvider.FromBaseDirectory().Load()`; un fallo da un `RackCatalog` vacío
    (`src/RackCad.UI/UiSupport.cs:84-94`);
  - la carpeta `catalogs` se busca primero junto al ensamblado y después en `AppContext.BaseDirectory`
    (`src/RackCad.Application/Catalogs/CatalogDirectory.cs:19-45`);
  - `Load()` guarda una caché por carpeta y firma de archivos (`src/RackCad.Application/Catalogs/JsonRackCatalogProvider.cs:72-89`);
  - lee primero el `.csv` hermano de cada `.json` (`JsonRackCatalogProvider.cs:298-310`). Los archivos entregados están
    en `assets/catalogs/` (`*.csv` y `defaults.json`).
- **H-111** La ventana usa el catálogo en:
  - `PostBox` (W:147) y las listas de vigas (W:148-149, W:3576-3578);
  - los peraltes permitidos (W:3585) y las variantes de seguridad, botas y defensa (W:220-232, W:2690-2697,
    W:2823-2825);
  - los valores por defecto de seguridad (W:279) y la autorización (W:2626);
  - la vista previa (W:3132-3141, W:3156-3162), los cortes laterales del lote (W:3308) y los diálogos (W:2999,
    W:2595, W:2606).
- **H-112** Proveedores estáticos que no salen del catálogo cargado:
  - `RackFrameTemplateCatalog.Default`, una lista fija en el código
    (`src/RackCad.Application/RackFrames/RackFrameTemplateCatalog.cs:21`, `:85`), usada en DA:218 y DDA:80;
  - `SelectiveRackDefaults.TroquelPaso` (EL:103);
  - `PushBackDefaults` (`src/RackCad.Domain/Systems/PushBack/PushBackDefaults.cs:16-54`);
  - `RearPeraltes`, fijo en la ventana (W:44).
- **H-113** El constructor de la ventana no acepta catálogo (W:123-128) y no pasa ninguno a la sesión (W:135), aunque
  `RackEditorSession` sí acepta uno (SES:83). Los ensambladores Application sí reciben cualquier `RackCatalog` (DA:42,
  CA:29).
- **H-114** Infraestructura STA:
  - `StaTestRunner` mantiene un único hilo STA de fondo con una `Application` en `OnExplicitShutdown`
    (`tests/RackCad.UI.Tests/StaTestRunner.cs:42`, `:64`, `:68`);
  - instala de forma permanente `EditorDiscardPrompt.Substitute(_ => true)` (`StaTestRunner.cs:57`);
  - expone `Run(Action)` y `Run<T>(Func<T>)` (`StaTestRunner.cs:78`, `:102`).
- **H-115** `RackCad.UI.Tests` (net8.0-windows, WPF) copia `assets/catalogs/*.json;*.csv` a `catalogs/` en la salida
  (`tests/RackCad.UI.Tests/RackCad.UI.Tests.csproj:30-35`). `RackCad.UI` da `InternalsVisibleTo` a `RackCad.UI.Tests`
  (`src/RackCad.UI/RackCad.UI.csproj:13-15`). `RackCad.Tests` es net8.0, referencia solo Domain, Application y dos
  herramientas, y también copia los catálogos (`tests/RackCad.Tests/RackCad.Tests.csproj:3`, `:23-31`, `:37`).
  Application da `InternalsVisibleTo` a `RackCad.Tests` (`src/RackCad.Application/RackCad.Application.csproj:11`).
- **H-116** Las pruebas de UI construyen la ventana real con `new RackPushBackSystemWindow(canInsertInAutoCad: true)`,
  sin mostrarla (por ejemplo, T-IN:121 y T-OW:55) o mostrada fuera de pantalla con `Left = -10000`,
  `ShowActivated = false` (T-CL:69-82). Para cargar, usan `LoadExisting` con `LastComputation.Design` de otra ventana
  (T-IN:318) o un sobre de `tests/RackCad.Tests/Fixtures/I64/` (WLC:49-54, WLC:66-82).
- **H-117** Las clases que sustituyen el aviso de cierre se serializan en la colección `editor-discard-prompt`, con
  `DisableParallelization = true` (`tests/RackCad.UI.Tests/EditorDiscardPromptCollection.cs:27-31`). Las T1B Close,
  Intent, Keyboard y Owner la declaran (T-CL:38, T-IN:31, T-KB:27, T-OW:27).
- **H-118** Costuras internas de la ventana para pruebas:
  - estado: `Session`, `State`, `Assembler` (W:157-159), `CurrentPreviewPlan` y `CurrentPreviewModel` (W:163-180),
    `LastComputation`, `HasValidModel`, `CurrentInputsAreValid` y `SafetySelections` (W:182-185);
  - `BuildLibraryProjectForTest` (W:240-243), `FirstLevelDatumForTest` (W:375) y `RecomputePassesForTest` (W:601);
  - `SelectMatrixCell` (W:1047), `DesviadorLevelsPerPost` (W:1517), `EditorStateForTest` (W:1529),
    `PendingHeaderConfigurationForTest` (W:2002) y `RearTopeLevels` (W:2988);
  - las secciones de botas, defensa y topes (W:2661, W:2664, W:2801, W:2804, W:2972, W:2975), `FrontalSideForTest`
    (W:3224), `PendingWork` (W:3417), `OutputIsBlockedForTest` (W:3487) y `CompositeState` (WC:43);
  - las costuras de diálogos de H-100 a H-104.
  
  `generalPallet`, `advanced`, `loadedDimensionViews` y `dimensionViewsTouched` no tienen costura.

## 9. Caracterización existente y guardas

### 9.1 Pruebas T1B (F4-T1B)

- **H-119** T-CL (17 métodos: 16 Fact y 1 Theory con 5 `InlineData`, 21 casos) fija los caminos de cierre:
  - Escape sin y con módulo escenificado (T-CL:135-256);
  - «Cerrar» por el patrón de automatización (T-CL:260-376);
  - el modal que pregunta dos veces (T-CL:473-512);
  - `Close()` mostrada y sin mostrar (T-CL:531-576);
  - el cierre de cada inserción y de Actualizar (T-CL:580-663);
  - el texto sin confirmar que se pierde (T-CL:705-711).
- **H-120** T-IN (9 métodos: 8 Fact y 1 Theory con 4 `InlineData`, 12 casos) fija:
  - la intención de cada botón de vista (T-IN:111-145);
  - el corte frontal de B (T-IN:147-180);
  - las cuatro guardas de `RequestDraw` (T-IN:184-273);
  - la barra de acciones en las cuatro combinaciones (T-IN:311-328);
  - «Insertar varias» siempre habilitado (T-IN:330-348) y antes de su diálogo (T-IN:382-439);
  - Seguridad cancelada (T-IN:352-378).
- **H-121** T-KB (4 Fact) fija:
  - que Enter no hace nada (T-KB:50);
  - el foco declarado en `NameBox` (T-KB:125-142);
  - que `NameBox` está vacío en `LoadNew` y muestra el nombre en `LoadExisting` (T-KB:144-161);
  - el orden EXACTO de las paradas de tabulación por zona, el orden de las zonas y que no hay valores locales de
    navegación (T-KB:179-282).
- **H-122** T-OW (4 Fact) fija:
  - que la ventana no tiene dueño y su ubicación y tamaños (1280×720, mínimo 1120×672) (T-OW:49-74);
  - el dueño de Seguridad (T-OW:78-110) y de la rejilla de botas (T-OW:112-159);
  - el diálogo de lote: `CenterOwner`, «Insertar cola» como `IsDefault` y «Cancelar» como `IsCancel` (T-OW:163-187).
- **H-123** T-RS (2 Fact) fija el diccionario fusionado, los tres tokens de la barra de acciones, el estilo del shell
  (T-RS:24-48) y que el shell no usa su respaldo (T-RS:50-70).
- **H-124** T-SG (12 Fact) son guardas de TEXTO que concatenan, en orden ordinal de ruta, todos los `.cs` y `.xaml` bajo
  `src/RackCad.UI/Systems/PushBack`, sin comentarios y con espacios colapsados (T-SG:169-176). Fijan:
  - el dueño y el retorno del configurador (T-SG:216-231);
  - las ausencias de G1 (T-SG:235-248);
  - que nadie se suscribe a `InsertRequestedRaised` (T-SG:250-264);
  - los dueños de defensa, topes, BOM, lote y guardar (T-SG:268-298);
  - el host del menú (T-SG:302-319);
  - el orden de «Guardar en biblioteca» (T-SG:323-341);
  - el `OnClosed` del configurador (T-SG:343-352);
  - el orden de «Insertar varias» (T-SG:356-379).
- **H-125** T-PG (8 métodos: 7 Fact y 1 Theory con 3 filas, 10 casos) son guardas de texto del Plugin sobre:
  - RACKPUSHBACK (T-PG:195-230);
  - el orden de escrituras de RACKEDITAR (T-PG:234-270);
  - el menú (T-PG:274-291);
  - el lote y la fachada de colocación (T-PG:293-311);
  - las firmas de las tres fachadas (T-PG:352-363);
  - las primitivas caller-owned (T-PG:365-389).
- **H-126** Recuento de T1B: 55 casos de UI (21 + 12 + 4 + 4 + 2 + 12) y 10 de Core, en 6 clases de UI y 1 de Core.
  Coincide con el traspaso de F4-T1B
  (`docs/automation/evidence/I-64-pilot/F4-T1B-PILOT-CHARACTERIZATION-nc1/1/R20261114T234208Z-6d40/inputs/worker-handoff.json:24`).
  Ninguna lleva `Skip` (recuento de atributos en los 7 archivos).
- **H-127** WLC tiene 3 Fact y 1 Theory sobre 14 fixtures: 17 casos.

### 9.2 Guardas que FALLARÍAN al tocar la ventana

- **H-128** G-10
  (`tests/RackCad.Tests/Workspace/WorkspacePilotUpdateIntegrationGuardTests.cs:721-797`):
  - W está en `OwnRequestUpdateWindows` (`:725-731`) y debe tener exactamente UNA `.RequestUpdate(` sin comentarios
    (`:748-749`);
  - el censo recorre todo `src` (`:30` → `tests/RackCad.Tests/Workspace/WorkspaceHostSourceSupport.cs:64`, que quita
    comentarios en `:29-30`);
  - un archivo nuevo de `src` con `.RequestUpdate(`, incluido uno bajo `Systems/PushBack`, la hace fallar (`:782`). Hoy
    la única llamada de W está en W:3369.
- **H-129** T-SG falla si:
  - cualquier `.cs` o `.xaml` bajo `src/RackCad.UI/Systems/PushBack` contiene sin comentarios `KeyDown`,
    `PreviewKeyDown`, `InputBindings`, `KeyBinding`, `TabIndex`, `IsTabStop`, `KeyboardNavigation`, `.Focus(` o
    `DialogResult` (T-SG:240-247);
  - cambia algún literal fijado: `new RackFrameConfiguratorWindow(copy) { Owner = this };`,
    `return window.Configuration ?? copy;`, `new RackBomWindow(lastComputation.Bom) { Owner = this }.ShowDialog();`, la
    llamada `TryShow(this, …)`, `PromptSaveToLibrary(this, NameBox.Text, "sistema")`, el orden de `SaveLibrary_Click` o
    los argumentos de `InsertBatch_Click` (T-SG:223-378);
  - cualquier `.cs` de `src` añade `InsertRequestedRaised +=` (T-SG:250-264).
- **H-130** T-KB:218-282 falla si se añade, quita o reordena un control con nombre y tabulable en cualquier zona del
  shell, o si se fija un valor local de `TabIndex`, `IsTabStop` o `KeyboardNavigation`. T-OW:49-74 falla si cambian el
  título, `CenterOwner` o los tamaños. T-RS:24-48 falla si cambian los recursos de la ventana.
- **H-131** Otras guardas de texto sobre la ventana:
  - `tests/RackCad.Tests/I55DeferredProductFixCharacterizationTests.cs:8-15`: W debe contener
    `PhysicalPostIndex(LateralSectionBox.SelectedItem)` y `corte.PostIndex == section`, y no
    `Math.Max(0, LateralSectionBox.SelectedIndex)`;
  - `tests/RackCad.Tests/PushBackModuleEditorCharacterizationTests.cs:382-440`: lee W y WX en CRUDO, con comentarios.
    Exige `RestoreAllModulesButton`, `RequestStandardRestore()` y los nombres de la superficie de módulos, y prohíbe
    `forceRebuild:` y `forceRebuild)` en W (`:399-400`);
  - `tests/RackCad.UI.Tests/BlankFrontDesviadorHandoffTests.cs:311-318`: exige
    `desviadorLevelsPerPost: DesviadorLevelsPerPost()` y `showDesviadorSide: false` en W;
  - `tests/RackCad.UI.Tests/RichEditorContractTests.cs:36-53`: WX debe declarar `FocusManager.FocusedElement`;
  - `tests/RackCad.UI.Tests/Workspace/WorkspaceDraftPilotValidationParityTests.cs:82-99`: fija los atributos de
    `AnnotationScaleBox` en WX.
- **H-132** `tests/RackCad.UI.Tests/WindowCensusGuardTests.cs:86-120` falla si el ensamblado `RackCad.UI` gana un tipo
  `Window` concreto sin censar. Mira el ensamblado de producto, no el de pruebas.

### 9.3 Guardas que FALLARÍAN al añadir archivos de prueba

- **H-133** `WLC:323-332` (`TheCorpusOfTheCharacterizationIsTheCorpusOfTheFixturesDirectory`) falla si se añade un
  `pb-*.json` a `tests/RackCad.Tests/Fixtures/I64/` sin añadirlo a sus listas. La lista de canónicos de
  `WorkspaceDraftPilotCanonicalLineEndingTests` no falla con un archivo nuevo, porque solo comprueba que existan sus 8
  (`tests/RackCad.UI.Tests/Workspace/WorkspaceDraftPilotCanonicalLineEndingTests.cs:66-78`).
- **H-134** `tests/RackCad.Tests/StructuralAnalysis/StructuralAnalysisRamElementsHardeningGuardTests.cs:84-92` recorre
  TODOS los archivos de `tests/RackCad.Tests` y falla si alguno parte una palabra prohibida de ese dominio en varios
  literales. No hay ninguna otra guarda encontrada que haga un censo de archivos de prueba y falle por añadir una clase
  de prueba de UI nueva en `tests/RackCad.UI.Tests`. La búsqueda cubrió `GetFiles` y `EnumerateFiles` sobre `tests` y
  `GetTypes()` en UI.Tests.
- **H-135** Una clase nueva que sustituya `EditorDiscardPrompt` sin entrar en `EditorDiscardPromptCollection` puede
  competir con las que lo sustituyen, porque el delegado es estático y no reentrante
  (`tests/RackCad.UI.Tests/EditorDiscardPromptCollection.cs:8-15`). No hay guarda que lo imponga (lectura: el nombre
  `EditorDiscardPromptCollection` solo aparece en atributos y comentarios de `tests`).

## 10. Qué se puede fijar hoy con una prueba automática sin cambiar el producto

Las columnas de la tabla son:

- **Core**: `RackCad.Tests`, sin WPF.
- **UI-STA**: la ventana real en `StaTestRunner`.
- **AutoCAD**: necesita el host.

| Punto | Core | UI-STA | AutoCAD | Hechos |
|---|---|---|---|---|
| Propiedad del estado | Sí, la parte Application: CS/ES/DA/CA son públicos | Sí, por las costuras de H-118, `FindName` y `LastComputation.Design`. Los cuatro campos privados sin costura solo se observan por su efecto | No | H-007…H-031, H-118 |
| Módulos | Sí (ES, DA y la sesión de módulos) | Sí (`ModuleBox`, `ModuleLengthBox`, Confirmar, `EditorStateForTest`) | No | H-037…H-040, H-068 |
| Cola aparcada | Sí (`DormantCompositeTail` es público) | Sí (`CompositeState`). Hoy ninguna prueba de UI carga `pb-20-ab` ni `pb-21-cola` | No | H-041…H-047, H-034 |
| A/B | Sí | Sí | No | H-054…H-062 |
| Añadir/quitar frentes | Sí (`SetSlotCount`, `SetSlotPresent`) | Sí (`AddFrontButton`, `RemoveFrontButton` y `FrontCountBox` con `LostFocus`; la casilla «En blanco» no tiene nombre y se encuentra por el árbol visual) | No | H-063, H-065, H-066 |
| Añadir/quitar módulos | Sí, por reconstrucción: cambio de tarima o de fondos | Sí, igual | No | H-067, H-073, H-074 |
| Renombrar | No aplica | Sí, el nombre de la ventana y de la petición (`RackName`, `InsertionRequest`) | Sí, el renombrado de bloques y el nombre automático del Plugin; hoy solo hay guardas de texto | H-024, H-069 |
| Borrado de vistas | Sí, la decisión: cortes laterales y `RackViewAvailability`, en Application | La ventana no borra nada | Sí, el borrado en sí (RPC:348-356) | H-096 |
| Creación de vistas | No aplica | Sí, la intención (vista, sección y `Views`). El lote solo hasta su diálogo, que no tiene costura | Sí, los bloques | H-092, H-093, H-097, H-106 |
| Actualizar frente a cerrar | No | Sí (ya fijado por T-CL y T-IN) | No | H-092…H-095 |
| Subdiálogos | No | Sí, con las costuras de H-100 a H-104 y H-108. BOM, lote y `SaveFileDialog` solo por texto (T-SG) o con un modal real. Crear el configurador real escribe en `<datos-locales-del-usuario>` al cerrarse | No | H-100…H-109 |
| Dependencias de catálogo | Sí, con cualquier `RackCatalog` | Sí, con el catálogo entregado. La ventana no admite otro catálogo sin cambiar el producto | No | H-110…H-113 |
| Pérdida del estilo de cotas | No, ocurre en `ReadInputs` | Sí (ya fijado en `PushBackDimensionViewsWindowTests.cs:307-322` y WLC:191-192) | No | H-080…H-082 |
| Pérdida por redondeo | No, el redondeo vive en `NumericField` (WPF) | Sí (ya fijado con `pb-08` en WLC:193-197) | No | H-076…H-079 |

- **H-136** Ninguna ruta de la ventana necesita AutoCAD para ejercerse en UI-STA: `RackCad.UI` no referencia AutoCAD
  (`tests/RackCad.UI.Tests/PushBackEditorWindowTests.cs:619`), y con `canInsertInAutoCad: true` la ventana solo produce
  la petición y se cierra (W:3323-3379). Lo que necesita AutoCAD es lo que hace el Plugin después de cerrarla
  (RPC:37-71, RPC:147-380).
