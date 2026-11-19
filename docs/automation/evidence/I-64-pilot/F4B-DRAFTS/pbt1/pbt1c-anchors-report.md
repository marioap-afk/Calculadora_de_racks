# I-64 F4B PB-T1c — anclas del paquete v2.7 sobre `2f5d3dec`

**Medidor de solo lectura.** Todo se leyó por objetos git desde el checkout principal (`show`, `diff`, `ls-tree`, `rev-parse`, `grep`, `cat-file --batch`). No se leyó, listó ni nombró el worktree con cesión activa. No se escribió en el repositorio, no se compiló y no se ejecutó ninguna prueba.

**Los scripts y sus salidas** (en `hs65/pbt1c/`):
- `measure_pbt1c_anchors.py` → `anchors-out.txt`: tareas (a) a (g);
- `count_tests_git.py` → `counts-2f5d3dec.txt`: tarea (h). Aplica el criterio de `count_tests.py`, pero lee la revisión con `ls-tree` + `cat-file` en vez de un árbol de trabajo. Calibrado sobre `5043b49a`, reproduce las cifras de P-5 (UI `PushBack` 556, `PushBackStateNet` 14, `Workspace` 547 +16; Core `Workspace` 3 271 +33, `PushBackClassicCharacterization` 7 +1, …);
- `simulate_pbt1c_green.py` → `simulate-out.txt` y `sim/`: tarea (i). Aplica D-PBT1-13 a W y WC de forma mecánica y en memoria, y mide sobre el resultado los invariantes de PB-T1c, CT-PBT1C-03 y G-L1. Es una lectura del diseño, no el producto: no compila.

**Numeración del paquete:** las líneas que cito son las de la v2.7 de entrada. La copia del repositorio en `2f5d3dec` tiene una línea más, en `:6` (la nota de la emisión de PB-T1b), así que a partir de ahí va +1.

**Dos archivos ajenos:** durante la medición aparecieron en `hs65/pbt1c/` dos archivos que no son míos, `make_pbt1c_contract.py` y `contract-dry-2f5d3dec.json`. No los leí ni los toqué.

## (a) `src/RackCad.UI` entre `b5c09f0b` y `2f5d3dec`

| Comprobación | Resultado |
|---|---|
| `git diff --quiet b5c09f0b 2f5d3dec -- src/RackCad.UI` | rc 0, vacío |
| árbol `src/RackCad.UI` | `c38679f8…` en las dos revisiones |
| `git diff --name-status b5c09f0b 2f5d3dec -- src` | solo Application: `A` de los cuatro archivos nuevos de PB-T1b; `M` de ES y RMES |
| líneas de W / WC / WX | 3700 / 936 / 793 |

**PASS.** Las anclas de UI remedidas sobre `b5c09f0b` valen tal cual.

## (b) Las anclas de D-PBT1-13, D-PBT1-14 y del §6.3

**59 anclas, 0 DIFF.** El texto completo de cada línea está en `anchors-out.txt`.
- **WX:** D-PBT1-13 y el §6.3 no citan ninguna línea de WX.
- **El §6.3** no cita líneas propias: remite a D-PBT1-13 (4).

| Ancla | Lo que dice el paquete | Lo que hay en 2f5d3dec | |
|---|---|---|---|
| W:56-57 | el campo `composite` | `private readonly PushBackCompositeEditorState composite =` / `new …(new PushBackEditorState(), new PushBackEditorState());` | PASS |
| W:84 | `safetySelections` | `private readonly List<SelectiveSafetySelection> safetySelections = new …` | PASS |
| W:101 | `generalPallet` | `private PalletSpecification generalPallet = PushBackEditorInputs.NewDesign().Pallet;` | PASS |
| W:106-109 | `rackTolerance`, `rackPostPeralte`, `rackBeamDepth`, `rackAnnotationScale` | las cuatro declaraciones `private double …` | PASS |
| W:114 | `advanced` | `private readonly PushBackEditorInputs advanced = PushBackEditorInputs.NewDesign();` | PASS |
| W:380 | `firstLevelDatum` | `private int? firstLevelDatum = (int)…LowestUsablePunch;` | PASS |
| W:1249, :1256, :1260 | `loadedDimensionViews`, `loadedDimensionStyle`, `dimensionViewsTouched` | sus tres declaraciones | PASS |
| W:273-276 | la mitad del editor de `LoadNew` | `SetActiveSide(A)`; `SetSideBPresent(false)`; `LoadComposite(null)`; `var inputs = state.LoadNew();` | PASS |
| W:286-287 | la seguridad por defecto | `safetySelections.Clear();` / `…AddRange(new PushBackSafetyAuthority(catalog).Defaults());` | PASS |
| W:299-302 y W:316-319 | la mitad del editor de `LoadDesignForNew` y de `LoadExisting` | `SetActiveSide`; `SetSideBPresent(false)`; `var inputs = state.LoadFromDesign(SideAOnly(design), assembler.Resolver);`; `LoadCompositeFromDesign(design);` | PASS |
| WC:933 | `UpdateCompositeEnabled();` de `LoadCompositeFromDesign` | `UpdateCompositeEnabled();`, y `}` en WC:934 | PASS |
| W:3320-3327 | la mitad del editor de `Restore_Click` | :3320-3322 como las cargas, y :3327 `LoadCompositeFromDesign(design, preserveDormantIntent: true);` (:3323-3326, blanco y comentario); en :3328, `LoadFromModel(inputs, NameBox.Text);` | PASS |
| W:385-440 (HT1-07) | `LoadFromModel`, con el `try` bajo `suppressSync` | :387 `suppressSync = true;`, :388 `try`, :436-437, :439 `Recompute();` | PASS |
| W:391, :393, :398, :401, :403, :406-411, :414, :419 y :423-427 | las asignaciones a portadores | las 21 líneas, cada una con su portador | PASS |
| W:390, :392, :394-397, :399-400, :402, :413, :415-418 y :420-421 | las siembras | las 16 líneas | PASS (M-1) |
| W:432, W:421 | `LoadAdvancedRackParameters` y `ShowDimensionViews`, después | sí | PASS |
| W:838-840 | la `PalletSpecification` de `ReadInputs` (5.º argumento) | `generalPallet.Weight, generalPallet.WeightUnit);` en :840 | PASS |
| W:852 | la expresión de la unidad | `generalPallet.Weight, WeightUnitBox.SelectedItem as string ?? "kg"),` | PASS |
| W:841-844 | las cuatro de `Keep` | `rackTolerance = Keep(ToleranceBox, rackTolerance);` … `rackAnnotationScale = Keep(…);` | PASS |
| W:853 | `PalletsDeep` | `PalletsDeep = IntVal(PalletsDeepBox, DynamicRackDefaults.DefaultPalletsDeep),` | PASS |
| W:868 | `PostCatalogId` | `PostCatalogId = PostBox.SelectedId,` | PASS |
| W:823 y W:826 | el respaldo de las vigas | `… as string ?? cell.InOutBeamCatalogId,` / `… ?? cell.IntermediateBeamCatalogId,` | PASS |
| DA:169-171 | el respaldo del poste | `IsNullOrWhiteSpace(editorInputs.PostCatalogId) ? catalog.Defaults?.Post : editorInputs.PostCatalogId;` | PASS |
| W:874-876 y W:878 | las tres casillas y `Dimensions` | sus cuatro expresiones | PASS |
| W:1295-1297 | las tres de cotas | `Dimensions{Frontal,Lateral,Planta}Check.IsChecked == true` | PASS |
| W:1291-1297 y W:880 | `SelectedDimensionViews` y su único uso | la declaración en :1291 y su único uso, en :880 | PASS |
| W:342-373 | `SideAOnly` | sí; sus usos son :301, :318 y :3322 | PASS |
| WC:813-814 y WC:827-934 | los dos `LoadCompositeFromDesign` | sí | PASS |
| W:279, :305, :322 | `Adopt` | sí | PASS |
| W:1237, :3352, :3405, :3504 | `SetName` | `session.Identity.SetName(NameBox.Text?.Trim());` | PASS |
| W:131-161; W:270, :294 y :311 (D-PBT1-14) | los constructores y las firmas públicas | sí | PASS |
| W:165-193 y WC:43 (§4) | las costuras | `Session` … `SafetySelections`; `CompositeState` | PASS |
| W:2559 (D-PBT1-15) | el único lector del informe | `var reconciliation = moduleOwner.LastModuleReconciliation?.Describe();` | PASS |

**Lecturas que no cambian el diseño:**
- **Dentro del `try` de `LoadFromModel`, fuera de las dos listas de (4):** W:404, :412, :422 y :428 (blancos), :405 (el comentario «adopt the four advanced scopes…», que queda obsoleto) y :429-432.
- **Escrituras de portadores fuera de `LoadFromModel` y `ReadInputs`:** solo W:1270-1271 (`ShowDimensionViews`) y W:1303 (`DimensionViews_Changed`). Siguen por el reenvío.
- **Lecturas de los portadores:** en W y WC no se leen en ningún otro sitio.

## (c) INV-PBT1C-03: los miembros de la lista en 2f5d3dec

Los rangos son los de la heurística de `members.py` (registro N-1); la última columna es la documentación que hay encima de cada miembro. **Todos existen.** No existen todavía `SyncName`, `EditableStateForTest` ni `editable`.

| Miembro | Rango | Documentación encima |
|---|---|---|
| `composite` | W:56-57 | W:49-55 |
| `safetySelections` | W:84 | — |
| **`generalPallet`** | **la heurística no lo separa**: lo incluye en `lastComputation`, W:92-101, porque W:92 acaba en un comentario de línea. La declaración está en W:101 | W:94-100 |
| `rackTolerance`, `rackPostPeralte`, `rackBeamDepth`, `rackAnnotationScale` | W:106, :107, :108, :109 | W:103-105 (`//`) |
| `advanced` | W:114 | W:111-113 |
| `firstLevelDatum` | W:380 | W:375-379 |
| `loadedDimensionViews`, `loadedDimensionStyle`, `dimensionViewsTouched` | W:1249, :1256, :1260 | W:1244-1248, :1251-1255, :1258-1259 |
| `LoadNew`, `LoadDesignForNew`, `LoadExisting` | W:270-289, 294-307, 311-324 | W:268-269, 291-293, 309-310 |
| `SideAOnly` | W:342-373 | **W:326-341** |
| `LoadFromModel`, `ReadInputs`, `Name_Changed` | W:385-440, 833-890, 1234-1238 | — |
| `SelectedDimensionViews` | W:1291-1297 | **W:1289-1290** |
| `Restore_Click`, `InsertBatch_Click`, `RequestDraw`, `SaveLibrary_Click` | W:3307-3330, 3334-3361, 3363-3419, 3481-3515 | W:3304-3306 (`Restore_Click`) |
| `LoadCompositeFromDesign` (los dos) | WC:813-814 y WC:827-934 | **WC:812 y WC:816-826** |
| `CompositeDiagnostics`, `CompositeStatusOr`, `CompositeHasBlocking` | WC:783-786, 789-801, 803-807 | WC:779-782, 788, — |

## (d) INV-PBT1C-04: los literales de T-SG

Las guardas están en `tests/RackCad.UI.Tests/PushBackClassicCharacterizationSourceGuardTests.cs`: S-7 en `:280-284`, S-9 en `:293-298`, CT-22 en `:323-341` y CT-23 en `:356-379`, como dice el §4.

| Guarda | Literal | En W, byte a byte | En WC |
|---|---|---|---|
| CT-22 (1) | `UiSupport.PromptSaveToLibrary(this,` | W:3498 | — |
| CT-22 (2) | `session.Identity.EnsureId();` | W:3503 | — |
| CT-22 (3) | `session.Identity.SetName(NameBox.Text?.Trim());` | W:3504 (y en :1237, :3352 y :3405) | — |
| CT-22 (4) | `var project = RackProject.ForPushBack(lastComputation.Design).WithSourceMetadataFrom(sourceProject);` | W:3507 | — |
| CT-22 (5) | `new RackProjectStore().Save(project, path);` | W:3508 | — |
| S-9 | `UiSupport.PromptSaveToLibrary(this, NameBox.Text, "sistema")` | W:3498 | — |
| S-7 | `new RackBomWindow(lastComputation.Bom) { Owner = this }.ShowDialog();` | W:3478 | — |
| CT-23 (1 a 4) | `if (moduleOwner.ModuleSession.HasPendingChanges) moduleOwner.CommitModuleEdits();` · `Views.RackViewBatchDialogPresenter.TryShow(this, …, out var views)` · `session.RequestInsertViews(views` · el último `Close();` | W:3336, :3351, :3354 y :3360 | — |
| CT-23 (las anclas `Has`) | `PushBackCut(…EntradaSalida…A)`, `PushBackCut(…Posterior…B)`, `Whole(DimensionViewKind.Planta)`, `Post(cut.PostIndex)` y `RackViewCodec.Encode(…)` | W:3342, :3345, :3346, :3350 y :3356 | — |

- **El orden:** CT-22 va en W:3498 < 3503 < 3504 < 3507 < 3508; CT-23, en W:3336 < 3351 < 3354 < 3360.
- **Las guardas, reimplementadas sobre el texto normalizado de `Systems/PushBack`** (los comentarios quitados como `StripCSharpComments` y los espacios colapsados): CT-22, S-9, S-7, CT-23 y CT-19 dan **PASS**.
- **Los sitios de `SetName` y `Adopt` son exactamente los siete, y no hay más en W ni en WC:**
  - `Adopt`: W:279 (`LoadNew`), W:305 (`LoadDesignForNew`) y W:322 (`LoadExisting`);
  - `SetName`: W:1237 (`Name_Changed`), W:3352 (`InsertBatch_Click`), W:3405 (`RequestDraw`) y W:3504 (`SaveLibrary_Click`).
- **Nadie más cambia el nombre:**
  - `RackEditorIdentity.Name` solo cambia en `SetName` y en `Adopt` (`src/RackCad.UI/Editor/RackEditorIdentity.cs:36-39` y `:61-65`);
  - `RackEditorSession` solo llama a `EnsureId` (`:157` y `:186`).

**PASS.**

## (e) INV-PBT1C-05: las cuentas y los literales

| Qué | Medido en 2f5d3dec |
|---|---|
| `.RequestUpdate(` (con la regex de G-10) | W: **1**, en W:3409, igual con y sin comentarios; WC: 0 |
| I-55 (`I55DeferredProductFixCharacterizationTests.cs:8-15`) | `PhysicalPostIndex(LateralSectionBox.SelectedItem)` en W:3135; `corte.PostIndex == section` en W:3224; `Math.Max(0, LateralSectionBox.SelectedIndex)` ausente |
| Fact3 (`PushBackModuleEditorCharacterizationTests.cs:381-401`) | `RequestStandardRestore()` en W:2539; `forceRebuild:` y `forceRebuild)` ausentes; `forceRebuild` solo en W:2535 (prosa); `StandardRestoreRequested` en DA:204; `RestoreAllModulesButton` en WX:664 |
| El desviador (`BlankFrontDesviadorHandoffTests.cs:311-318`) | `desviadorLevelsPerPost: DesviadorLevelsPerPost()` en W:3044; `showDesviadorSide: false` en W:3048 |
| Las líneas con `SetNumber(` | W, 24: 394, 396, 397, 400, 402, 418, 458, 463-467, 469-471, 473, 476, 574-576, 1399, 1625, 1813 y 1833. WC, 5: 340, 348, 581, 746 y 749. **Ninguna de las líneas que D-PBT1-13 quita o sustituye la contiene**: las seis de `LoadFromModel` son siembras y se quedan |
| Las siembras de D-PBT1-13 (4) | las 16 líneas de (b), con su texto en `anchors-out.txt` |

**PASS.**

## (f) INV-PBT1C-06: las costuras internas

**40 en total: 39 en W y 1 en WC**, con el mismo nombre y tipo que en `b5c09f0b`. `EditableStateForTest` no aparece en `src/` ni en `tests/`. **PASS.**

| Línea | Nombre | Tipo |
|---|---|---|
| W:165, :166, :167 | `Session`, `State`, `Assembler` | `RackEditorSession<PushBackDesign, PushBackSystem>`, `PushBackEditorState`, `PushBackEditorDesignAssembler` |
| W:171, :181 | `CurrentPreviewPlan`, `CurrentPreviewModel` | `HeaderRunPlan`, `PushBackPreviewModel` |
| W:190-193 | `LastComputation`, `HasValidModel`, `CurrentInputsAreValid`, `SafetySelections` | `PushBackEditorComputation`, `bool`, `bool`, `IReadOnlyList<SelectiveSafetySelection>` |
| W:195, :197 | la clase `LateralViewOption` y su constructor | — |
| W:207, :214, :217 | `LateralViewOptions`, `PhysicalPostIndex`, `RestoredLateralSelectionIndex` | estáticos |
| W:228, :248 | `SafetyElementsForDialog`, `BuildLibraryProjectForTest` | `IReadOnlyList<SafetyElementCatalogEntry>`, `RackProject` |
| W:383, :614 | `FirstLevelDatumForTest`, `RecomputePassesForTest` | `int?`, `int` |
| W:1080 | `SelectMatrixCell` | `void` |
| W:1533, :1539, :1545 | `SafetyDialog`, `RearTopeDialog`, `DefenseDialog` | `Func<…>` |
| W:1557, :1565, :1569 | `DesviadorLevelsPerPost`, `HeaderConfiguratorDialog`, `EditorStateForTest` | `IReadOnlyList<int>`, `Func<RackFrameConfiguration, RackFrameConfiguration>`, `PushBackEditorState` |
| W:2042, :2450 | `PendingHeaderConfigurationForTest`, `HeaderConfiguratorPresenter` | `RackFrameConfiguration`, `Action<RackFrameConfiguratorWindow>` |
| W:2701, :2704, :2740 | `BootSectionForTest`, `BootSectionBForTest`, `BootPerPostWindowDialog` | `PushBackBootSection` (las dos), `Func<SafetyPerPostWindow, bool?>` |
| W:2841, :2844, :3012, :3015 | `DefenseSection(B)ForTest`, `RearTopeSection(B)ForTest` | `PushBackDefenseSection`, `PushBackRearTopeSection` |
| W:3028, :3068 | `RearTopeLevels`, `SafetyWindowDialog` | `IReadOnlyList<int>`, `Func<SelectiveSafetyWindow, bool?>` |
| W:3264, :3457, :3527 | `FrontalSideForTest`, `PendingWork`, `OutputIsBlockedForTest` | `PushBackSide`, `EditorPendingWork`, `bool` |
| WC:43 | `CompositeState` | `PushBackCompositeEditorState` |

## (g) La API de Application que usa D-PBT1-13 (EST = `PushBackEditableState.cs`, GATE = `PushBackEditorOutputGate.cs`)

**0 desajustes.** Cada miembro existe y es público, con una firma y un tipo compatibles, y con setter donde la ventana escribe. Los valores iniciales del constructor (EST:40-52) son los de los inicializadores de W.

| Uso en D-PBT1-13 | Miembro en 2f5d3dec | ¿Compatible? |
|---|---|---|
| (1) `editable.Editor` | `public PushBackCompositeEditorState Editor { get; }` (EST:55) | sí |
| (2) `SafetySelections` | `public List<SelectiveSafetySelection> SafetySelections { get; }` (EST:92): es una `List`, así que el reenvío admite `Clear`/`Add`/`AddRange` (W:2669-2672, :2803, :2994) y la costura `IReadOnlyList` de W:193 | sí |
| (2) `Pallet`; `PalletTolerance`, `PostPeralte`, `BeamDepth`, `AnnotationScale` | `PalletSpecification { get; set; }` (EST:59); `double { get; set; }` (EST:63-66) | sí |
| (2) `Advanced` | `PushBackEditorInputs { get; }` (EST:90); la ventana muta sus propiedades (W:700, W:1639-1646) | sí |
| (2) `FirstLevelDatum`, `LoadedDimensionViews`, `DimensionStyle`, `DimensionViewsTouched` | `int?`, `DimensionViewVisibility?`, `string` y `bool`, todos `{ get; set; }` (EST:69, :80, :77, :83) | sí |
| (3) `LoadNewEditor()` | `PushBackEditorInputs LoadNewEditor()` (EST:100): el mismo cuerpo que W:273-276 | sí |
| (3) `SeedDefaultSafety(catalog)` | `void SeedDefaultSafety(RackCatalog)` (EST:173); `catalog` es `RackCatalog` (W:47) | sí |
| (3) `LoadDesignEditor(design, assembler.Resolver)` y `RestoreEditor(...)` | `PushBackEditorInputs …(PushBackDesign, PushBackResolver)` (EST:109, :119); `assembler.Resolver` es `PushBackResolver` (DA:84) | sí |
| (4) `AdoptCarriers(inputs, catalog)` | `void AdoptCarriers(PushBackEditorInputs, RackCatalog)` (EST:130) | sí |
| (5) `BuildInputs(assembler)` | `PushBackEditorInputs BuildInputs(PushBackEditorDesignAssembler)` (EST:367) | sí |
| (5) `PalletsDeep`, `PostCatalogId`, las tres casillas, `Dimensions` y las tres de cotas | `int`, `string`, `bool` ×3, `DimensionDetail` y `bool` ×3, todos `{ get; set; }` (EST:61-62, :71-74, :85-87) | sí |
| (7) `CompositeDiagnostics` | `IReadOnlyList<PushBackCompositeDiagnostic> Diagnostics(PushBackCompositeEditorState, PushBackSystem)` (GATE:16): el tipo de WC:783 | sí |
| (7) `CompositeStatusOr` | `string StatusOr(PushBackCompositeEditorState, PushBackSystem, string)` (GATE:28) | sí |
| (7) `CompositeHasBlocking` | `bool IsBlocked(PushBackCompositeEditorState, PushBackSystem)` (GATE:23) | sí |
| (8) `editable.Name` | `string Name { get; set; }` (EST:95) | sí |
| (9) y CT-01/02 | `PushBackEditableState` (`sealed` y público, con constructor público); `PushBackEditableStateComparer.Instance`, `Equals` y `Copy` estático (Comparer:31, :33, :63) | sí |

**Equivalencias, por lectura:**
- **Los cuerpos idénticos:** los de `Diagnostics`/`StatusOr`/`IsBlocked` son los de WC:783-806, y el de `LoadNewEditor`, el de W:273-276.
- **`AdoptCarriers`** reproduce W:391-427 y W:1270-1278. Cambian solo la unidad (D-PBT1-09b) y el poste (D-PBT1-09).
- **`BuildInputs`** reproduce W:846-887, con `DimensionViewPolicy.FromEditor` como `SelectedDimensionViews`.
- **`LoadDesignEditor` y `RestoreEditor`** son W:299-302 y W:3320-3322 más WC:827-931, sin `UpdateCompositeEnabled()`, que D-PBT1-13 pone en la línea siguiente. Copian la cola aparcada, y el diseño no la comparte (PB-T1b).
- **M11 de PB-T1b** (con un diseño nulo, las dos cargas mutan antes de lanzar) no se alcanza desde la ventana: W:296, W:313 y W:3309 salen antes.

## (h) `RequiredTests` del §6.3: recuento estático en 2f5d3dec (criterio de `count_tests.py`)

‡ = cambiará, o puede cambiar, con la corrección de PB-T1b que está en curso: lo mide la emisión. Según `analysis.md` §4, la corrección no añade métodos, así que se espera la misma cifra.

| Proyecto | Filtro | §6.3 | Medido | |
|---|---|---|---|---|
| UI | `PushBackWindowOnStateTests` | 2 | 0 (prefijo libre) | — |
| Core | `PushBackWindowOnStateSourceGuardTests` | 1 | 0 (libre) | — |
| Core | `PushBackWindowSharedReferenceGuardTests` | 1 | 0 (libre) | — |
| Core | `PushBackSharedReferenceGuardTests` | 2 | 2 (1 clase) | ‡ (I01, M07) |
| UI | `PushBack` | 557 | **557** (69 clases, 3 `Skip`) | ‡ (incluye `PushBackEditableStateParityTests`, M08) |
| UI | `BlankFront`, `WindowCensusGuardTests`, `RichEditor`, `Workspace` | 33, 8, 25, 547 | 33, 8, 25 (+4 `Skip`), 547 (+16 `MemberData`) | — |
| UI | `PushBackRichPreviewTests` | «el medido» | **9** | — |
| Core | `PushBackEditableState` | 13 | 13 (Load 5, Copy 7, Ownership 1) | ‡ |
| Core | `PushBackEditorOutputGate` | 1 | 1 | ‡ (M13) |
| Core | `PushBackClassicCharacterization` | 10 | 7 + 1 `Theory` con `MemberData` (10 casos, T1B) | — |
| Core | `PushBackFullEditorCharacterization`, `PushBackModuleEditorCharacterizationTests`, `I55DeferredProductFixCharacterizationTests`, `NamespaceFolderGuardTests`, `Workspace` | 3, 15, 3, 7, 3 271 | 3, 15, 3, 7, 3 271 (+33 `MemberData`) | — |
| UI | `Cantilever` | 67 | 67 | — |

- **Todas coinciden con el §6.3.**
- **La previsión del GREEN de PB-T1c**, si la corrección mantiene el número de métodos: Core 19 086 + 2 = 19 088; UI 2 437 + 2 = 2 439.

## (i) Lo que no se puede hacer tocando solo W y WC, o que contradice INV-PBT1C-01 a 07c

**D-PBT1-13 se puede hacer entero tocando solo W y WC.** No necesita nada de Application (g). La simulación mecánica (`simulate-out.txt`; 37 hunks en W, y 4 o 5 en WC) da esto:

| Comprobación | Resultado |
|---|---|
| T-SG (CT-22, S-9, S-7, CT-23, CT-19) | PASS |
| `SyncName();` justo después de los siete `SetName`/`Adopt` | sí |
| `.RequestUpdate(` | 1 |
| Tokens prohibidos en las líneas añadidas | ninguno |
| Literales de I-55, Fact3 y el desviador | presentes; `forceRebuild:`/`)` ausentes |
| `SetNumber(` añadidas / quitadas | 0 / 0 |
| Las 16 siembras | idénticas y en su orden |
| `editable.AdoptCarriers(inputs, catalog);` | primera sentencia del `try` |
| `UpdateCompositeEnabled();` | en la línea siguiente a los dos `LoadDesignEditor` y al `RestoreEditor` |
| Costuras | 40 → 41 (+`EditableStateForTest`), sin pérdidas |
| CT-PBT1C-03 | cumplida en la simulación; en la base, falla en todas sus cláusulas (el RED es válido) |
| INV-PBT1C-07c | ninguna asignación de `Name` sobre una referencia de D2 |
| **G-L1 de CT-PBT1C-04** | **0** en la base (320 miembros) y en la simulación (310). Los alias, de lectura: `module`, `outputIsBlocked` y `reconciliation` |
| **G-L1 de CT-PBT1B-14** sobre los 51 archivos de 2f5d3dec (1 063 miembros) | **0** |
| Las 14 autopruebas de D-PBT1-15 | saltan |

**La pasada reentrante (HT1-10b), por lectura:**
- su `ReadInputs` escribe en el estado lo que muestran unos controles todavía sin sembrar;
- `AdoptCarriers` reescribe todos esos campos antes de la primera siembra;
- lo único que pasa de una pasada a otra es el `current` de `ResolveWeightUnit`, que es la unidad de la caja: la premisa de D-PBT1-09b.

**No hay hecho nuevo.** Lo que sí contradice INV-PBT1C-03, la `ExpectedEvidence` y CT-PBT1C-04 va abajo, en I-1 a I-3.

## DIFF ordenados por gravedad

### BLOQUEANTE

Ninguno. Las 59 anclas dan PASS, la API está completa (0 desajustes), los recuentos coinciden y el diseño cabe en W y WC.

### IMPORTANTE (hay que corregir el paquete)

**I-1. La `ExpectedEvidence` de PB-T1c, heredada del §6.1, no se puede cumplir.**
- **Paquete:**
  - §6.3: «**`ExpectedEvidence`:** como §6.1, con la salida de INV-PBT1C-03 a 05 sobre el diff…»;
  - §6.1: «el paso 1 con su TRX y su log; el RED y su CI (ui-tests en `failure`; tests, build-ui y build-plugin en `success`; sin cuelgue); la tabla T-8; los quince mutantes aislados; … la lista de INV-PBT1A-13»;
  - §6.0: «En PB-T1b y PB-T1c, Core falla en el RED».
- **Medido:**
  - en `.github/workflows/ci.yml`, `build-plugin` tiene `needs: [tests, build-ui]`;
  - el §6.3 pone en Core `PushBackWindowOnStateSourceGuardTests` con `ExpectRed` true;
  - así, en la CI del RED, `tests` sale en `failure` y `build-plugin` queda **omitido**. Es M14 de PB-T1b: F4B-E22 lo ratificó solo para PB-T1b y dijo que «el paquete de PB-T1c corrige el texto» (`owner-pbt1b-rework-decisions.md`);
  - además, PB-T1c no tiene paso 1, ni T-8, ni mutantes, ni lista INV-PBT1A-13.
- **Corrección:** escribir la `ExpectedEvidence` de PB-T1c entera:
  - la CI del RED con `tests` y `ui-tests` en `failure`, `build-ui` en `success` y `build-plugin` omitido;
  - sin paso 1, sin T-8 y sin mutantes;
  - en su lugar, la salida de las autopruebas de CT-04, la de INV-PBT1C-03 a 05 y la clase de huellas tres veces.

**I-2. INV-PBT1C-03 da los miembros por su nombre, sin rangos, y no se cumple al pie de la letra.**
- **Paquete:** «cada hunk … cae en un miembro de esta lista, por su rango en `BaseSha` (la definición de «tocar un miembro» de [PKC2] INV-PBTC2-03)». [PKC2] sí daba las líneas, y también las de documentación («y W:3641, en su documentación»).
- **Medido** (simulación):
  - **(a)** con la heurística de `members.py` del registro, el reenvío de `generalPallet` (W:101) cae en `lastComputation` (W:92-101), un miembro que no está en la lista;
  - **(b)** retirar `SideAOnly`, `SelectedDimensionViews` y los dos `LoadCompositeFromDesign` **con su documentación** deja líneas fuera de todo miembro: W:325-341, W:1288-1290, WC:811-812 y WC:815-826;
  - solo pasa si se dejan huérfanos esos comentarios. Entonces la documentación de `SideAOnly` queda pegada a `firstLevelDatum`, y las de WC delante de la llave que cierra la clase;
  - con W:101 corregida y la documentación dentro del rango, pasa: 0 hunks fuera.
- **Corrección:**
  - dar los rangos por línea en `BaseSha`, con la documentación: W:49-57, :84, :94-101, :103-109, :111-114, :375-380, :1244-1249, :1251-1256 y :1258-1260; `SideAOnly` W:325-373; `SelectedDimensionViews` W:1288-1297; `LoadCompositeFromDesign` WC:811-934; y lo demás, con los rangos de (c);
  - admitir las dos inserciones entre miembros (`SyncName`, `EditableStateForTest`);
  - decir si se admite un comentario nuevo sobre `editable`.

**I-3. CT-PBT1C-04: falta la autoprueba (c) y no está la de I01.**
- **Paquete:**
  - §5.4: «las autopruebas (a), (b), (d), (e), (f1), (f2), (f3), (g1), (g2), (h1), (h2), (i) y (j) de D-PBT1-15». Son 13, sin (c);
  - D-PBT1-15 (:700-713): la lista con (c), «y, en CT-PBT1C-04, (d)», y «las catorce autopruebas saltan».
- **Medido:**
  - (c), el molde de estilo C de la regla 1b, salta;
  - la autoprueba «alias + molde» de la corrección de PB-T1b (I01, `analysis.md`: `var r = side.LastModuleReconciliation; (r.Preserved as List<string>).Add("x");`) salta **solo** si la regla 1b se aplica también a los alias de la regla 2. Sin eso (el defecto I01), no salta. Lo mismo con un molde de estilo C sobre un alias de `lastCompositeSystem`.
- **Corrección:** que CT-PBT1C-04 nombre las 14 de D-PBT1-15 y la de I01, y exija la regla 1b sobre los alias, como CT-PBT1B-14 corregida.

### MENOR (de redacción)

**M-1. Dos «siembras» son variables locales.**
- **Paquete,** D-PBT1-13 (4): «Las siembras de los controles no cambian ni un carácter (W:390, :392, …, :413, …)».
- **Medido:** W:392 (`var pallet = …`) y W:413 (`var options = …`) son locales, no siembras. No cambia nada; es exactitud.

**M-2. Una siembra escribe dos portadores.**
- **Paquete,** D-PBT1-13 (4): «se quitan las asignaciones a portadores (W:391 …)» y «ninguna siembra lee un portador».
- **Medido:** es cierto. Pero `ShowDimensionViews`, llamado en W:421, **escribe** dos portadores (W:1270-1271), y `DimensionViews_Changed` escribe otro (W:1303).
  - Siguen por el reenvío, con los mismos valores que `AdoptCarriers`, así que el resultado es equivalente;
  - conviene decirlo;
  - además, el comentario de W:405 queda obsoleto.

**M-3. El filtro UI `PushBack` falla en el RED.**
- **Paquete,** §6.3: UI `PushBack` va con `ExpectRed` false, y contiene `PushBackWindowOnStateTests`, que va con `ExpectRed` true.
- **Medido:** en el RED, ese filtro falla exactamente por los 2 métodos nuevos.
  - El §6.1 declara la excepción equivalente para los mutantes; el §6.3 no.
  - Conviene declararla: «UI `PushBack` falla en el RED solo por `PushBackWindowOnStateTests`».

**M-4. Las cifras de dos filtros.**
- **Paquete,** §6.3: `PushBackRichPreviewTests` «el medido», y UI `PushBack` 557.
- **Medido:**
  - `PushBackRichPreviewTests` = 9 (igual que en N-6);
  - UI `PushBack` = 557 en la `BaseSha` y 559 en el GREEN; 557 sirve como `MinSelected`.
