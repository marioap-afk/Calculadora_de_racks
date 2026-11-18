> **Redaccion para el repo publico:** las rutas locales se sustituyen por marcadores (`<worktree-de-I-64>`, `<carpeta-del-paquete>`). El resto no cambia.

# I-64 F4B — Registro de mediciones del paquete del contrato de PB-T1, v1

**Qué es:** las mediciones en solo lectura que sostienen el §2 del paquete `I-64-f4b-pbt1-gate-contract-package.md` v1, con la orden de cada una.

**Base:** rama `architecture/workspace-persistente-rackcad`, HEAD `eedb069c`. `main` en `812baf87`.

**Reglas:** sin compilar, sin ejecutar pruebas, sin AutoCAD, sin escribir en el worktree ni en `artifacts/`, sin Git que escriba. Los tres scripts de Python son de solo lectura, se escribieron con la herramienta de archivos y se ejecutaron con `python -I <script> <args>`, nunca con `-c`. Están junto a este registro, en `<carpeta-del-paquete>`:
- `members.py`: lista los miembros de un `.cs` con su rango de líneas (heurístico: cuenta llaves; un rango sirve para anclar, no para medir un hunk);
- `count_tests.py`: recuento **estático** de casos de prueba por prefijo de clase (`Fact` = 1; `Theory` = sus `InlineData`; `MemberData` se cuenta aparte);
- `forbidden_scope.py`: el `ForbiddenWriteScope` de `src/` con el algoritmo de `make_pbtc2_contract.py` (`cover()`), para el `AllowedWriteScope` propuesto de cada subtarea.

**Alias:** los del paquete (§2.0): W, WC, WX, CS, ES, EL, DA, CA, PC, PM, S0, SC1.

---

## A. Identidad de la base

| Id | Orden | Resultado |
|---|---|---|
| A-1 | `git -C <worktree-de-I-64> rev-parse HEAD` | `eedb069ce8ae79cda684f2620694949fd402051d` |
| A-2 | `git -C <worktree-de-I-64> status --short` | vacío |
| A-3 | `git -C <worktree-de-I-64> log --oneline -8` | `eedb069c` (emisión de PB-TC2), `8708d28f`, `5597c05b` (cierre de PB-TC), `4440f67d` (GREEN de PB-TC), `f665edac`, `48fe199e`, `a9e7195a`, `f9f2afbb` |
| A-4 | `git -C <worktree-de-I-64> diff --stat 4440f67d eedb069c -- src tests` | vacío: el producto de HEAD es el GREEN de PB-TC |
| A-5 | `wc -l` de W, WC, WX | 3690, 936 y 793. Iguales a los que midió PB-TC2 en `5597c05b` |
| A-6 | `gh run list --branch architecture/workspace-persistente-rackcad --limit 8 --json databaseId,headSha,status,conclusion,event` | `44246483921` sobre `eedb069c`, `push`, `success`. `44232324035` sobre `5597c05b`, `success` (la línea base de PB-TC2: Core 19 070/19 070; UI 2 397 Passed, 21 Skipped, 2 418) |

**Consecuencia:** como `src` y `tests` no cambian entre `5597c05b` y `eedb069c`, los conteos de la CI de `eedb069c` son los de `44232324035`. No se descargaron los TRX de `44246483921`.

## B. Inventario del estado de la ventana (W y WC)

Orden: `python -I members.py <worktree-de-I-64>/src/RackCad.UI/Systems/PushBack/RackPushBackSystemWindow.xaml.cs` (263 miembros) y lo mismo con WC (69 miembros), más `Read` de W:1-500, W:600-910, W:1078-1527, W:1528-1677, W:3290-3690 y WC:1-300, WC:404-643 y WC:700-936.

### B.1 Campos con estado authored o de carga (los portadores)

| Campo | Ancla (miembro, línea) | Siembra | Lectura | Escritura |
|---|---|---|---|---|
| `composite` (Application) | campo, W:56-57 | `LoadNew` W:273-276, `LoadDesignForNew` W:299-302, `LoadExisting` W:316-319, `Restore_Click` W:3320-3327; `LoadCompositeFromDesign` WC:827-934 | todo W y WC (`state` W:63 = `composite.Active`; `moduleOwner` W:82 = `composite.SideA`) | los manejadores y el ensamblador (DA:155-159 normaliza peraltes y saque en cada armado) |
| `generalPallet` | campo, W:101 | `LoadFromModel` W:392-393 | `ReadInputs` W:838-840, W:850-852 | `ReadInputs` W:838-840 (lo sustituye con `Keep(DepthBox, …)`) |
| `rackTolerance`, `rackPostPeralte`, `rackBeamDepth`, `rackAnnotationScale` | campos, W:106-109 | `LoadFromModel` W:398, :401, :403, :419 | `ReadInputs` W:869-871, :877 | `ReadInputs` W:841-844 (`Keep`) |
| `advanced` (un `PushBackEditorInputs`; solo sus seis avanzados) | campo, W:114 | `LoadFromModel` W:406-411 | `ReadInputs` W:862-867; `LoadAdvancedRackParameters` W:1606-1611 | `ReadAdvancedRackParameters` W:1639-1646; `RestoreRackParameters_Click` W:1670; D-PB-11 en `Recompute` W:700 |
| `firstLevelDatum` | campo, W:380 | `LoadFromModel` W:391 | `ReadInputs` W:859 | solo la carga |
| `loadedDimensionViews`, `dimensionViewsTouched` | campos, W:1249 y W:1260 | `ShowDimensionViews` W:1270-1271, desde `LoadFromModel` W:421 | `SelectedDimensionViews` W:1291-1297 | `DimensionViews_Changed` W:1303 |
| `loadedDimensionStyle` | campo, W:1256 | `LoadFromModel` W:414 | `ReadInputs` W:879 | solo la carga |
| `safetySelections` | campo, W:84 | `LoadFromModel` W:423-427; `LoadNew` W:286-287 | `ReadInputs` W:884-887 | `Safety_Click` W:2574-2698 y las secciones (W:2598-3038, 21 líneas que lo nombran) |
| `sourceProject` | campo, W:91 | `LoadNew` W:277, `LoadDesignForNew` W:303, `LoadExisting` W:320 | `RequestDraw` W:3410, :3415; `InsertBatch_Click` W:3358; `SaveLibrary_Click` W:3507; `BuildLibraryProjectForTest` W:251; `SourceProjectToInsert` W:261 | solo la carga |
| `isEditingExisting` | campo, W:87 | W:278, :304, :321 | `RequestDraw` W:3371; `UpdateButtons` W:3567, :3582 | solo la carga |
| Identidad y nombre | `session.Identity` (UI, `RackEditorIdentity`) | `Adopt` en W:279, :305, :322 | `RackId`/`RackName` W:257-258; `UpdateGuid` W:3612 | `Name_Changed` W:1234-1238; `SetName` en W:3352, :3405, :3504; `EnsureId` W:3503 |
| `frontalSide` | campo, W:3261 | `UpdateFrontalButtons` WC:119 la vuelve a A | `BuildFrom(…, frontalSide)` W:659-660; `SelectedView` W:3131-3133 | `FrontalSide_Changed` W:3277-3289 (hace `RequestRecompute`) |
| `pendingHeaderConfiguration`, `pendingHeaderSourceName` | campos, W:2038-2039 | — | `ApplyHeaderSelection_Click` W:2061-2071 | `StageHeaderConfiguration` W:2049-2059 |

### B.2 Derivados o de interfaz (no son estado editable)

`lastComputation` (W:92), `hasValidModel` y `currentInputsAreValid` (W:89-90), `outputIsBlocked` (W:3524), `lastCompositeSystem` (WC:810), `mixedOptional` y `mixedNow` (WC:416, WC:429), `matrixCards` (W:118), `blankToolTips` (W:546), `moduleIds` (W:1711), `headerTargetIds`/`Labels` y `headerLineIndexes`/`Labels` (W:2205-2208), `copySourceIds` (W:2384), `previewSurface` (W:3184), `suppressSync` (W:88), `RecomputePassesForTest` (W:614).

### B.3 Valores que solo viven en un control y que `ReadInputs` relee en cada pasada

Orden: `grep -n -w -F "<control>" W WC` y `grep -n 'x:Name="<control>"' WX`.

| Control | Siembra (`LoadFromModel`) | Lectura (`ReadInputs`) | Evento en WX |
|---|---|---|---|
| `WeightUnitBox` | W:395 | W:852 (`SelectedItem as string ?? "kg"`) | `SelectionChanged="Combo_Changed"` (WX:274) |
| `PalletsDeepBox` | W:396 (`Max(2, …)`) | W:853 (`IntVal(…, DefaultPalletsDeep)`) | `LostFocus="Input_Changed"` (WX:111) |
| `PostBox` | W:399 (en blanco → `Defaults.Post`) | W:868 (`PostBox.SelectedId`, **sin respaldo**) | `SelectionChanged="Combo_Changed"` (WX:118) |
| `NumberFrontsCheck`, `NumberLevelsCheck`, `DrawRackNameCheck` | W:415-417 | W:874-876 | WX:54, :56, :58 |
| `DimensionsBox` | W:420 (recortado a [0, 3]) | W:878 | `SelectionChanged="Combo_Changed"` (WX:73) |
| Las tres casillas de cotas | `ShowDimensionViews` W:1276-1278 | `SelectedDimensionViews` W:1291-1297, desde W:880 | WX:80, :83, :86 |

`CatalogCombo.SelectedId` es `SelectedValue as string` (`src/RackCad.UI/Controls/CatalogCombo.cs:24-28`), con `SelectedValuePath = Id` (`:18-19`). Las opciones salen de `UiSupport.ToOptions` («distinct by id, ordered by label», `:62-67`). **No medido:** cómo compara WPF un `SelectedValue` con otras mayúsculas (lectura: `Object.Equals`, ordinal). `ResolveBeamId` compara sin mayúsculas (`DynamicRackLevelGeometry.cs:200-204`).

### B.4 La carga de la parte compuesta vive en la ventana

- `SideAOnly` (W:342-373): estático y puro. Devuelve el MISMO objeto para un diseño no compuesto (W:344-347); para uno compuesto copia `Structure` recortada, `LegacyHighEndBeamPeralte`, `RearTope`, `DefensePieceId` y los `Fronts` por referencia (W:349-370).
- `LoadCompositeFromDesign` (WC:813-814 y WC:827-934): toda su lógica es de Application salvo la última línea, `UpdateCompositeEnabled()` (WC:933). Lo que hace: captura la intención dormida con `preserveDormantIntent` (WC:829-835), `SetSideBPresent` y `LoadComposite` (WC:837-838), `ClearDormantTail` (WC:847), la reposición de lo dormido (WC:848-858), el aparcado de la cola de `design.Structure` (WC:860-868), la carga del lado B por su propio diseño (WC:870-927) y `AlignSlotGrid` (WC:931).
- El piloto lo reproduce aparte (PM:150-172): es la segunda copia de la misma lógica.

## C. El flujo (quién siembra, lee y escribe)

- **Constructor** (W:136-161): crea la sesión con `recompute: Recompute` y sin catálogo (W:143); el catálogo es `session.Catalog` (W:144); los dos ensambladores (W:145-146); termina en `LoadNew()` (W:160).
- **`LoadNew`** (W:270-289): lado A, sin B y `LoadComposite(null)` (W:273-275); `state.LoadNew()` (W:276); `sourceProject = null`, `isEditingExisting = false`, `Adopt(null, null)` (W:277-279); `LoadFromModel(inputs, "")` (W:280), que recalcula (W:439); la seguridad por defecto (W:286-287); y un segundo `Recompute()` (W:288).
- **`LoadExisting`** (W:311-324) y **`LoadDesignForNew`** (W:294-307): lado A, sin B (W:316-317); `state.LoadFromDesign(SideAOnly(design), assembler.Resolver)` (W:318); `LoadCompositeFromDesign(design)` (W:319); proyecto, modo e identidad (W:320-322); `LoadFromModel(inputs, rackName)` (W:323). Solo cambian el modo (W:304 frente a W:321) y la identidad (W:305 frente a W:322).
- **`Restore_Click`** (W:3307-3330): desde `lastComputation.Design` (W:3319), el mismo camino con `preserveDormantIntent: true` (W:3327) y `LoadFromModel(inputs, NameBox.Text)` (W:3328). No toca la identidad, el proyecto ni el modo.
- **`LoadFromModel`** (W:385-440): bajo `suppressSync` (W:387), siembra los controles Y los portadores a la vez (B.1 y B.3), pinta la matriz y el panel (W:429-432) y recalcula (W:439).
- **`Recompute`** (W:616-723): campo en error → no toca nada (W:625-633); `CommitCurrentCell` (W:635, que escribe la celda primaria desde los controles por `ReadCellValues`, W:802-831); `assembler.BuildFrom(compositeAssembler.BuildDesign(composite, ReadInputs()), frontalSide)` dentro de un `try` (W:645-670); si es válida: `SetModel` (W:674), `AcceptComputation` (W:675, que fija la línea base = `computation.System`, DA:101-107), `CommitDormantTailConsumption` (W:679), `lastComputation` (W:680), D-PB-11 (W:696-702), `ClearModuleCommit` (W:697), `RefreshCompositePanel` (W:707), `outputIsBlocked` (W:711) y el estado (W:712).
- **`ReadInputs`** (W:833-890): primero actualiza los portadores con `Keep` (W:838-844); después construye unos `PushBackEditorInputs` NUEVOS (W:846-882) con los portadores, los controles de B.3, `loadedDimensionStyle` y `SelectedDimensionViews()`; la seguridad pasa por `assembler.AuthorizedSafety` (W:884-887), que da copias.
- **El ensamblador muta lo que recibe:** `NormalizePeraltes` y el saque (DA:155-159) mutan el estado del lado A; `PushBackAdvancedRackParameters.Reset(editorInputs)` (DA:302) muta los inputs de esa pasada. Hoy esos inputs son nuevos en cada pasada, así que el `advanced` de la ventana no se toca (H-PB-28, D-PB-11).

## D. Los alias con el modelo cargado

Orden: lectura de W:342-373, W:385-427, WC:827-934, EL:187-232 y `PushBackResolver.cs` (`grep -n -E "DeepCopy|Clone|= design\." …`).

| Id | Alias | Vida | Mutado después |
|---|---|---|---|
| D-1 | `SideAOnly` comparte con el diseño del llamador `Fronts[i]` y `RearTope` (W:358-370) | transitorio: lo lee `LoadFromDesign`, que resuelve; el resolver copia `RearTope` con `DeepCopy` (`PushBackResolver.cs:73`, `:121`) | no (lectura) |
| D-2 | El diseño del lado B que arma `LoadCompositeFromDesign` comparte `Structure.Pallet`, `SideB.Fronts[i]` y `FrontConfig(i)` del llamador (WC:877, :907, :915) | transitorio, hasta `SideB.LoadFromDesign` (WC:918) | no (lectura) |
| D-3 | **La cola aparcada guarda los MISMOS objetos `DynamicRackModuleDesign` y `DynamicHeaderLineOverride` del diseño cargado** (WC:863-867 → CS:85-95, `AddRange`) | toda la sesión; `Restore_Click` los vuelve a aparcar (WC:829-830, :852) | no: `AttachDormantTail` los copia al armar (DA:535-571, `headerClone.DeepCopy`) |
| D-4 | `sourceProject` es el `RackProject` del llamador (W:320) | toda la sesión; viaja por referencia en cada petición (W:3410, :3415, :3358) y en la biblioteca (W:3507) | no (metadatos I-11) |
| D-5 | La línea base resuelta es el MISMO objeto que `lastComputation.System` y que `session.System`: `AcceptComputation` (W:675 → DA:105), `lastComputation = computation` (W:680) y `session.SetModel` (W:674) | hasta el siguiente cálculo válido, que la sustituye | no por lo leído: DA la lee (`:198`, `:210`, `ModuleIntents` `:57-69`, `CopyStructureSystem` `:508-529`, que copia), CA la lee (`:182`) y ES la lee (`:169`, `:178`). `git grep -n "WorkingBaseline" -- src` da 6 líneas, todas de lectura salvo `SetWorkingBaseline` (DA:105). **No se midió en ejecución** |
| D-6 | `generalPallet = pallet` (W:393), con `pallet = inputs.Pallet`: los inputs salen de `RecoverInputs`, que clona (EL:195, `ClonePallet` EL:229-232), o de `NewDesign()` | hasta la primera pasada, que lo sustituye (W:838) | no |

`safetySelections` se llena con `DeepCopy` (W:426) y `advanced` se copia campo a campo (W:406-411): no hay alias.

## E. Los controles que siguen siendo fuente de verdad tras PB-TC y PB-TC2

**Definición** (la de H-PB-44, aceptada en [OPD] P4): un control que se relee sin que el usuario cambie su valor y, al releerse, cambia el estado o la salida.

**Tras PB-TC** (`Keep` en las diez cajas; el estilo de cotas viaja) **y PB-TC2** (las seis cajas de H-31 y la caja mixta de «Ambos lados», según su paquete v2, D-PBTC2-01 a 05): ninguna caja numérica sembrada con «0.###» cambia el estado sin edición.

| Id | Control | ¿Cambia el estado sin edición? | Alcance | Qué hace PB-T1 |
|---|---|---|---|---|
| E-1 | `PostBox` con un id fuera del catálogo (c-02) | **Sí**: la siembra no selecciona nada (W:399) y la lectura da nulo (W:868), que DA:169-171 convierte en `Defaults.Post` | solo por documento o catálogo editado (CT-PB0-17) | Regla explícita del estado (F4B-E09, pregunta 2) |
| E-2 | `PostBox` en blanco (c-01) | La ventana siembra `Defaults.Post` (W:399) y el resolver ya lo hacía (DRSR:64-66) | solo por documento | Lo mismo: la regla cubre el blanco |
| E-3 | `RearPeralteBox` con un peralte fuera de la lista fija (W:44, W:480-481) | Sí, a 3,5 | **Inalcanzable con el catálogo entregado**: la lista coincide con `secciones.csv:10` (c-03, MB H-087) | Sin cambio en la ventana; límite declarado (el estado de Application no la reproduce con otro catálogo) |
| E-4 | `CellInOutPeralteBox`, `CellIntermediatePeralteBox` con un peralte fuera de la lista del catálogo (W:3620-3630) | Sí, al índice 0 | **Inalcanzable al cargar**: la carga ya normaliza en Application (c-05/c-07; «la clase (d) no se alcanza», T-2) | Sin cambio |
| E-5 | `WeightUnitBox` con una unidad que no está en la lista (c-09) | No: CT-PB0-17 mide «ton se conserva» (T-2) | por documento | Pasa a portador; misma expresión |
| E-6 | `PalletsDeepBox`, las casillas y `DimensionsBox` | No: la siembra y la lectura coinciden (entero, booleano, índice recortado; el almacén ya normaliza `Dimensions`) | — | Pasan a portadores; misma expresión |
| E-7 | El indicador mixto obsoleto sin lado B (M-07 de PB-TC2): vaciar «Largo manual» no surte efecto y las cajas obligatorias quedan opcionales (OptionalVal W:3668; WC:580) | **No cambia el estado**: la caja no consigue editarlo | por la interfaz | Se reproduce tal cual (decisión D-PBT1-12 del paquete) |

**Conclusión:** tras PB-TC2, los únicos controles que deciden el estado con el catálogo entregado son E-1 y E-2, y los resuelve la regla de c-02. Los demás de B.3 son editores sin pérdida que hoy no tienen portador fuera del control: PB-T1 les da uno.

## F. El criterio de bloqueo de salida

Orden: `git -C <worktree-de-I-64> show 5ccaa770:src/RackCad.UI/Systems/PushBack/RackPushBackSystemWindow.Composite.cs | sed -n '770,790p'` y `grep -n -E "CompositeStatusOr|CompositeDiagnostics\(|CompositeHasBlocking|lastCompositeSystem|outputIsBlocked" W WC`.

- **En la base de la fase** (`5ccaa770`), WC:778-782 es `CompositeHasBlocking`.
- **En HEAD:** `CompositeHasBlocking` es WC:803-807 (guarda `lastCompositeSystem` y pregunta `IsBlocking`); `CompositeDiagnostics` es WC:783-786 (`composite.IntentDiagnostics()` + `PushBackCompositeDiagnostics.Evaluate(lastCompositeSystem)`); `CompositeStatusOr` es WC:789-801 (el primer bloqueante, si no el primer aviso, si no el texto de reserva).
- **Consumidores:** `Recompute` W:711-712; `OutputBlockedAfterRecompute` W:3552-3561 (desde `RequestDraw` W:3400, `InsertBatch_Click` W:3339, `Bom_Click` W:3473 y `SaveLibrary_Click` W:3493); `UpdateButtons` W:3565; `BuildLibraryProjectForTest` W:249.
- **No es `RackBomOutputGate`:** `RackBomOutputGate.For(system)` (`src/RackCad.Application/Bom/RackBomOutputGate.cs:49-59`) evalúa solo `PushBackCompositeDiagnostics.Evaluate(system)`, sin los diagnósticos de la intención. El criterio de la ventana añade `IntentDiagnostics()` (CS:629-631 → `PushBackCompositeDiagnostics.EvaluateIntent`).

## G. El copiador y el comparador del piloto

Orden: `Read` de PC (352 líneas), PM (253) y `PushBackPilotDraftState.cs` (34).

- **`Copy`** (PC:58-62) = `CopyEditor` (PC:64-83: `Snapshot()`/`Restore()` sobre un estado nuevo, y repone `ActiveSelection` y `ActiveSide`) + `CopyInputs` (PC:85-111). No lleva `workingBaseline`, `moduleSession`, `moduleCommit`, `moduleSignature`, `LastModuleReconciliation`, `DormantCompositeTail`, `DormantTailLineOverrides` ni `dormantTailConsumed` (PC:53-56 lo declara).
- **El comparador** (PC:30-35, :134-302): `Snapshot()` sin selección (PC:171-177) + las entradas. No lee la autoridad de módulos ni la cola.
- **`ComposeForWrite`** (PM:196-234) vuelve a materializar desde `initial.Origin` (PM:217) y aplica solo la escala (PM:227). **`KeepPilotEditsOn`** (PM:242-251) devuelve `Copy(newInitial)` con la escala: nace sin línea base, sin módulos y sin cola (H-PB-27).
- **El rechazo del piloto:** `CompositeOutOfPilotReason` (PM:106, devuelto en PM:148) y `DormantTailOutOfPilotReason` (PM:108, en PM:169-170).

## H. Guardas que atan el diseño (además del §4 del expediente)

| Id | Guarda | Orden | Qué fija |
|---|---|---|---|
| H-1 | **El censo del piloto** (`tests/RackCad.Tests/Workspace/WorkspaceDraftPilotStateTests.cs:204-537`, `PilotCensus.Table`, y `EveryInstanceFieldOfTheGraphIsClassifiedInExactlyOneOfTheFourClasses` en `:955-964`) | `Read` de `:195-540` y `:940-1000` | **Todo campo de instancia** del grafo de `PushBackPilotDraftState` está en la tabla y toda entrada existe: `PushBackCompositeEditorState`, `PushBackEditorState`, `DynamicFrontMatrix`, `DynamicEditorFront`/`Cell`, `PushBackEditorFront`/`Cell`, `PushBackTopologyCell`, `PushBackEditorInputs`, `PalletSpecification`, `DynamicAnnotationOptions`, `SelectiveSafetySelection` y sus configuraciones. **Un campo nuevo en cualquiera de esos tipos la hace fallar.** El expediente no la lista |
| H-2 | Los recuentos de la clase de la H-1 | `TheClassesOfTheContractAreExactlyTheFieldsTheContractNames` (`:966-982`) | Selección = 5 campos; autoridad de módulos = 8; origen = 1 |
| H-3 | G-10 | `git -C <worktree-de-I-64> grep -n -c -F ".RequestUpdate(" -- src` | 8 archivos con 1 cada uno, W incluido (W:3409). Un archivo nuevo de `src` con ese texto la rompe |
| H-4 | I-55 | `grep -n -F` en W | `PhysicalPostIndex(LateralSectionBox.SelectedItem)` W:3135; `corte.PostIndex == section` W:3224 |
| H-5 | Fact3 | `grep -n -F "forceRebuild" W`; `grep -n -F "RequestStandardRestore()" W` | `forceRebuild` solo en `<c>forceRebuild</c>` (W:2535); `RequestStandardRestore()` W:2539; DA debe contener `StandardRestoreRequested` (`PushBackModuleEditorCharacterizationTests.cs:387-391`) |
| H-6 | Desviador | `grep -n -F` en W | `desviadorLevelsPerPost: DesviadorLevelsPerPost()` W:3044; `showDesviadorSide: false` W:3048 |
| H-7 | T-SG S-7, S-9, CT-22, CT-23 | `sed -n 160,380p tests/RackCad.UI.Tests/PushBackClassicCharacterizationSourceGuardTests.cs` | Literales de W que nombran `lastComputation`, `NameBox.Text`, `session.Identity`, `sourceProject` y `moduleOwner` (W:3478, :3498, :3503-3508, :3336); el texto concatena todos los `.cs` y `.xaml` de `src/RackCad.UI/Systems/PushBack`, sin comentarios (`:169-176`) |
| H-8 | Las guardas de ensamblado de Application | `git grep -n -E "GetExportedTypes|Assembly\.GetTypes" -- tests` | Todas miran espacios de nombres de `Workspace` (o tipos concretos): un tipo nuevo en `RackCad.Application.Systems.PushBack` no las toca. `RackModuleEditSessionTests.cs:494-505` exige que ningún tipo exportado se llame `RackModuleCustomizationPolicy` |
| H-9 | El núcleo de borradores | `WorkspaceDraftPilotIntentGuardTests.cs:232-247` | `src/RackCad.Application/Workspace/Drafts/` no nombra `PushBack`, `Pilot` ni `RackCad.Application.Systems` |
| H-10 | Los constructores del plan | `WorkspacePilotPrepareGuardTests.cs:388-404` | CA, el constructor lateral y el resolver no nombran `Autodesk`, el Plugin, la UI ni `TransactionManager` |
| H-11 | Espacio de nombres = carpeta | `grep -n "Fact\|Assert\|namespace" tests/RackCad.Tests/NamespaceFolderGuardTests.cs` | Cada archivo de `src/` declara un solo `namespace` de bloque igual a su carpeta (`:93-130`); los proyectos de prueba conservan un espacio de ensamblado (`:41-49`) |
| H-12 | La lista curada de `tests/RackCad.Tests/` de PB-T0 | `grep -o '"tests/RackCad.Tests/[^"]*"' …/F4B-PBT0-CHARACTERIZATION/coordinator/gate-contract.json \| sort -u` | 18 entradas: `Fixtures/`, `Workspace/`, `StructuralAnalysis/`, `RackCad.Tests.csproj`, `RackModuleEditSessionTests.cs`, `NamespaceFolderGuardTests.cs`, las guardas de la ventana y del Plugin y las 3 clases de Core de PB-T0. Es el método para PB-T1b y PB-T1c, que escriben en `tests/RackCad.Tests/` |

**Uso de nombres de W y WC por las pruebas** (`git grep -n -F "<nombre>" -- tests | wc -l`): `SideAOnly` 0, `LoadCompositeFromDesign` 0, `generalPallet` 0, `rackTolerance` 0, `loadedDimensionStyle` 0, `loadedDimensionViews` 0, `dimensionViewsTouched` 0, `firstLevelDatum` 0, `isEditingExisting` 0, `CompositeHasBlocking` 0, `frontalSide` 0, `ReadCellValues` 0. Los únicos que aparecen en código de prueba son los literales de T-SG (`lastComputation` 2, `sourceProject` 1 y `moduleOwner` 1) y los textos de comentarios.

**Costuras internas de W y WC** (`grep -n -E "^\s+internal " W WC`): 39 líneas en W, de W:165 a W:3527 (incluidas la clase anidada `LateralViewOption` y su constructor), y `CompositeState` (WC:43): 40 en total.

## I. Recuentos estáticos de las pruebas

Orden: `python -I count_tests.py <worktree-de-I-64> UI <prefijos…>` y lo mismo con `Core`.

| Proyecto | Prefijo (`FullyQualifiedName~RackCad.<proyecto>.`…) | Ejecutables | `Skip` |
|---|---|---|---|
| UI | `PushBackClassicCharacterization` | 55 | 0 |
| UI | `PushBackFullEditorCharacterization` | 16 | 2 (SUS-03, SUS-04) |
| UI | `PushBackDeclaredCorrection` | 10 (+4 de PB-TC2 al cerrarse) | 0 |
| UI | `Cantilever` | 67 | 0 |
| UI | `PushBackModuleEditorWindowTests` | 13 | 0 |
| UI | `PushBackAdvancedRackParametersWindowTests` | 9 | 0 |
| UI | `PushBackCompositeWindowTests` | 52 | 0 |
| UI | `PushBackCompositeLifecycleWiringTests` | 7 | 0 |
| UI | `PushBackCompositeLoadExistingModulesTests` | 3 | 0 |
| UI | `PushBackBothModeCellBeamOverrideTests` | 10 | 0 |
| UI | `PushBackDormantTailTransactionTests` | 8 | 0 |
| UI | `PushBackDormantRestoreTests` | 7 | 0 |
| UI | `PushBackDimensionViewsWindowTests` | 12 | 1 (SUS-02) |
| UI | `PushBackEditorSideOwnershipTests` | 13 | 0 |
| UI | `PushBackLibrarySaveGateTests` | 5 | 0 |
| UI | `PushBack` (todas las clases `PushBack*`) | 538 | 3 |
| UI | `BlankFront` | 33 | 0 |
| UI | `WindowCensusGuardTests` | 8 | 0 |
| UI | `Workspace` | 547 (+16 `Theory` con `MemberData`) | 0 |
| Core | `PushBackClassicCharacterization` | 7 (+1 `Theory` con `MemberData`; 10 casos según el traspaso de T1B) | 0 |
| Core | `PushBackFullEditorCharacterization` | 3 | 0 |
| Core | `PushBackModuleEditorCharacterizationTests` | 15 | 0 |
| Core | `PushBackBlankFrontAuthorityTests` | 9 | 0 |
| Core | `PushBackOutputGateTests` | 8 | 0 |
| Core | `PushBack` | 1 584 (+59 `MemberData`) | 0 |
| Core | `Workspace` | 3 271 (+33 `MemberData`) | 0 |

**Límite:** son recuentos de texto. Una `Theory` con `MemberData` no se puede contar sin ejecutar. El filtro `Workspace` de UI no seleccionó la clase de WLC por su nombre de espacio; WLC (3 `Fact` y una `Theory` con `Skip`) se cuenta en PB-TC2.

**Prefijos libres** (`git -C <worktree-de-I-64> ls-files tests | grep -E "PushBackStateNet|PushBackEditableState|PushBackEditorOutputGate|PushBackWindowOnState"`): vacío. Ninguno es prefijo de una clase existente; `PushBackEditorOutputGate` no es prefijo de `PushBackOutputGateTests`.

## J. Los alcances de escritura propuestos

Orden: `python -I forbidden_scope.py <worktree-de-I-64>`.

| Subtarea | `AllowedWriteScope` | `ForbiddenWriteScope` de `src/` | Faltan en HEAD (producto) | Nuevas que ya existen | `src/` sin cubrir |
|---|---|---|---|---|---|
| PB-T1a | 9 (W, WC y 7 pruebas nuevas) | 59 (la forma de PB-TC2, con WX) | ninguna | ninguna | ninguna |
| PB-T1b | 12 (ES, `RackModuleEditSession.cs` y CS; 4 archivos nuevos de Application y 5 de prueba) | 116 | ninguna | ninguna | ninguna |
| PB-T1c | 4 (W, WC y 2 pruebas nuevas) | 59 | ninguna | ninguna | ninguna |

El listado completo, entrada a entrada, es la salida del script. Las listas de `tests/` y de la raíz siguen el método de PB-TC2 (curadas, §6 del paquete). **El script mide PB-T1b con CS dentro del alcance**; el diseño del paquete no lo necesita (D-PBT1-07) y el generador del contrato lo dejará fuera, lo que da 117 entradas en `src/`.

## K. Celdas y catálogo de modelos

Orden: `sed -n 12,16p docs/automation/agent-execution/model-catalog.md`; `grep -n "Fecha de verificación" …`.

- Frescura: `STALE` = mín(verificación + 90 días, retiro anunciado) (`model-catalog.md:14`).
- Anthropic, verificación 2026-09-30 (`:51`); el Controller `gpt-6-luna`, 2026-11-11 (PB-TC2 §6.8). Las dos celdas son `STALE` desde el **2026-12-29**.
- nc4, solo en la primera delegación de cada cadena (`docs/automation/agent-execution/README.md:232-235`).

## L. Lo que no se pudo medir

- **Nada se compiló ni se ejecutó.** Los rangos de `members.py` son heurísticos; los anclajes del paquete se leyeron además a mano.
- **El GREEN de PB-TC2 no existe todavía.** Los anclajes de `Gap_Changed`, `ModuleLength_Changed`, `ReadAdvancedRackParameters`, `ReadCellValues`, `OptionalVal` y `Val` son de HEAD, y se vuelven a medir sobre su GREEN. Por el diseño de PB-TC2 (D-PBTC2-01 a 05), las líneas de W anteriores a W:3634 no se desplazan; las posteriores se desplazan −2 hasta `Keep`, y `IntVal` más los miembros nuevos.
- **WPF:** cómo compara `SelectedValue` un id con otras mayúsculas y por qué `WeightUnitBox` conserva «ton». Lo segundo lo mide CT-PB0-17; lo primero no lo mide nadie.
- **La no mutación de una línea base adoptada (D-5):** por lectura de los seis usos de `WorkingBaseline` y de los constructores. No se midió en ejecución.
- **`RackModuleEditSession`:** su copia y su comparación no existen; el paquete las diseña por lectura de sus nueve campos (`RackModuleEditSession.cs:74-87`).
- **Los conteos de la CI de `eedb069c`:** se dan por iguales a los de `44232324035` porque `src` y `tests` no cambian; no se descargaron sus TRX.
- **El efecto de un catálogo distinto del entregado** (E-3, E-4): la ventana no admite un catálogo inyectado (H-PB-31).
- **Incidencia de procedimiento:** una orden de la medición H-12 llevaba por error una invocación `python -I -` con un heredoc vacío (sin programa). No ejecutó código, se quedó esperando y se detuvo; la medición se repitió con `grep`. No se usó `-c` en ningún momento.

---

## M. Mediciones de la v2 (tras la revisión IRT1-1)

**Base:** la misma, HEAD `eedb069c`. Las mismas reglas; ningún Python nuevo (solo `count_tests.py`, con la herramienta de archivos y `python -I`), sin `-c` y sin heredoc.

### M-1. Todos los consumidores del objeto que es la línea base (I-01)

Orden: `grep -n -E "lastComputation\.System|computation\.System|session\.System|lastCompositeSystem" W WC`; `grep -n -E "SystemToInsert|\.System\b" src/RackCad.Plugin/RackPushBackCommands.cs`; `grep -n -E "system\.(Name|[A-Z]\w*) *=[^=]" src/RackCad.Plugin/RackPushBackCommands.cs src/RackCad.Plugin/RackMenuCommands.cs src/RackCad.Plugin/Systems/PushBack/*.cs`; `git grep -n "WorkingBaseline" -- src`.

Tras una computación válida, el mismo objeto `computation.System` queda en cuatro sitios a la vez: la línea base del lado A (W:675 → DA:105), `lastComputation.System` (W:680), `session.System` (W:674) y `lastCompositeSystem` (W:711 → WC:805).

| Consumidor | Ancla | Lee o muta |
|---|---|---|
| La línea base: el armado siguiente | DA:198, DA:210, `ModuleIntents` DA:57-69, `DerivedPostLinesOf` y `LineOverridesOf` DA:72-81 (devuelven las listas del objeto), `CopyStructureSystem` DA:508-529 (copia por snapshot → resolve) | lee; las líneas se copian al sistema nuevo con `headerClone.DeepCopy` (DA:331-363) |
| La cola al dormir | `ParkTailOf` CA:180-242 | lee; **aparca referencias** (M-2) |
| La sesión de módulos | ES:169 (`OpenModuleSession`), ES:178 (`ReseedModuleSession`); RMES:116-151 («The system is READ, never captured») | lee |
| `RefreshCompositePanel` | W:707 → WC:738-777 | lee |
| El criterio de bloqueo | W:711 → WC:803-807 → `PushBackCompositeDiagnostics.Evaluate` | lee |
| Los módulos del panel | W:1732 (`RackModuleDescriptor.Describe(lastComputation.System.Structure)`) | lee |
| La vista previa | W:3181, :3189, :3201 (sistema); W:3209-3221 (planos de la computación) | lee |
| El lote de vistas | W:3348 (`PushBackSystemLateralBuilder.Cortes(lastComputation.System, catalog)`, `PushBackSystemLateralBuilder.cs:91`) | lee |
| Las peticiones de inserción | W:3357, W:3410, W:3415 (`PushBackInsertionRequest(lastComputation.System, …)`) | lo entregan al Plugin |
| **El Plugin, tras cerrar la ventana** | `RackPushBackCommands.cs:98` (`system.Name = rackName;`) y `:190` (`system.Name = name;`), y el lote de vistas del menú, `RackMenuCommands.cs:135` (`pushBack.System.Name = pushBackName;`; añadido en la v2.1 por N-M-01 de la comprobación acotada). `PushBackSystem.Name` escribe `Structure.Name` (`PushBackSystem.cs:139-144`). `ProjectVariableMutationExecutor.cs:169` resuelve su propio sistema (Selectivo) y no cuenta | **muta** `Name`. Hoy es inocuo: la ventana ya está cerrada y su estado se descarta |
| `session.System` | W:3392 (solo nulidad) | lee |

`grep -n "Name = \|\.Name\b" src/RackCad.Application/Systems/Dynamic/DynamicRackSystemResolver.cs` solo da `DrawRackName` (`:242`, `:356`): el snapshot del resolver dinámico no lleva `Name`, así que un `Name` mutado no viaja al diseño por `CopyStructureSystem`.

### M-2. La cola que aparca `ParkTailOf` apunta a la línea base y al commit (corrige HT1-12)

Orden: `sed -n 175,245p CA`; `sed -n 777p` y `sed -n 780,815p src/RackCad.Application/Systems/PushBack/PushBackCompositeStructure.cs`.

- `ParkTailOf` (CA:180-242) aparca `tail.Select(PushBackCompositeStructure.ToTailDesign)` (CA:192), y `ToTailDesign` (`PushBackCompositeStructure.cs:777`) es `ToDesign(module, …)`, que pone `HeaderConfiguration = module.AssociatedFrameConfiguration` **por referencia** (`:800-811`): el `RackFrameConfiguration` del módulo de la línea base.
- Las líneas aparcadas son **los mismos** `DynamicHeaderLineOverride` de `baseline.HeaderLineOverrides` (CA:188-190).
- Con commit, sustituye por **las mismas** intenciones y líneas del commit (CA:201-239).
- **Nadie los muta** (lectura): el despertar los copia campo a campo con `headerClone.DeepCopy` (DA:535-571, en concreto DA:557-566).

### M-3. La copia de las intenciones de RMES no es literal (I-05)

Orden: `sed -n 778,811p RMES`.

- `CopyIntent` es `private` (RMES:799) y pone `UseCalculatedHeaderConfiguration = ResolveProvenance(…)` (RMES:806-807), con `ResolveProvenance(useCalculated, configuration) => useCalculated || configuration == null` (RMES:782-783): un módulo «personalizado sin configuración» pasa a calculado.
- El despertar de la cola copia **literal** (DA:557-566).
- `RackModuleCommit` tiene un constructor `internal` (RMES:18).

### M-4. Lo que ofrece `PostBox` (M-05)

Orden: `sed -n 118,135p src/RackCad.UI/UiSupport.cs`; `head -c 400 assets/catalogs/defaults.json`; `grep -n -i "poste" assets/catalogs/secciones.csv`.

- `UiSupport.ToOptions` (`UiSupport.cs:124-133`): quita los `Id` en blanco, agrupa por `Id.Trim()` **sin mayúsculas**, se queda con el primero de cada grupo, ordena por etiqueta y crea la opción con `Id.Trim()`.
- El catálogo entregado tiene **un solo** poste, `POSTE_OMEGA_ATORNILLABLE_CON_TROQUEL_GOTA_DE_AGUA` (`secciones.csv:2`), y es el `post` por defecto (`defaults.json:2`). Con ese catálogo, las filas (a) a (e) del paso 1 de PB-T1b dan la misma salida con cualquier comparación.

### M-5. Los objetos inmutables que comparte una copia profunda de la seguridad (I-02)

Orden: `sed -n 1,60p src/RackCad.Domain/Systems/Selective/SafetyFacePiece.cs`; `sed -n 1075,1095p` y `sed -n 528,537p tests/RackCad.Tests/Workspace/WorkspaceDraftPilotStateTests.cs`.

- `SafetyFacePiece` es inmutable, con `None` estático (`SafetyFacePiece.cs:26-44`), y `DeepCopy` lo comparte.
- El censo del piloto lo admite con `PilotCensus.IsImmutable` (`:531-536`) en `TheCopySharesNoMutableObjectWithTheOriginal` (`:1078-1093`).

### M-6. La sesión de módulos se abre en cada pasada de la ventana (I-03)

Orden: `sed -n 1678,1692p W`; `sed -n 124,135p ES`.

`RefreshModuleSelector` (W:1680-1709), llamado en cada pasada válida (W:704), lee `moduleOwner.ModuleSession.Modules` (W:1688), y el getter abre la sesión si es nula (ES:124-133). Un estado que no pasa por la ventana la deja nula.

### M-7. La reentrada de `UpdateCompositeEnabled` (I-04)

Orden: `sed -n 103,122p WC`; `sed -n 3277,3289p W`; `sed -n 30,45p src/RackCad.UI/Editor/RecomputeGate.cs`.

- `UpdateFrontalButtons` (WC:103-122) pone `FrontalSideBox.SelectedIndex = 0` cuando el rack deja de ser compuesto (WC:117-121).
- `FrontalSide_Changed` (W:3277-3289) no está bajo `suppressSync` en ese momento y hace `RequestRecompute()`; `RecomputeGate.Request` es síncrono fuera de un `Defer` (`RecomputeGate.cs:35-44`).
- En la carga de hoy, esa pasada ocurre **antes** de `LoadFromModel`, que después reasigna los portadores (W:391-427).

### M-8. Los diagnósticos de la intención (M-07)

Orden: `sed -n 86,140p src/RackCad.Application/Systems/PushBack/PushBackCompositeDiagnostics.cs`; `grep -n "MinimumPalletsDeep =" src/RackCad.Application/Systems/PushBack/PushBackCellDepth.cs`; `sed -n 580,590p CS`.

- Los textos de `EvaluateIntent` (`PushBackCompositeDiagnostics.cs:94-140`), con `MinimumPalletsDeep = 2` (`PushBackCellDepth.cs:35`).
- `SetGap` guarda un negativo tal cual (CS:584).

### M-9. Recuentos nuevos

Orden: `python -I count_tests.py <worktree-de-I-64> Core NamespaceFolderGuardTests HeaderBatchContractTests RackModuleEditSessionTests`.

| Filtro | Ejecutables | `MemberData` |
|---|---|---|
| `NamespaceFolderGuardTests` | 7 | 0 |
| `HeaderBatchContractTests` | 17 | 2 |
| `RackModuleEditSessionTests` | 25 | 0 |

### M-10. Anclas corregidas

- `tests/RackCad.Tests/Workspace/WorkspaceDraftPilotIntentGuardTests.cs` tiene 245 líneas; `TheDraftCoreStillNamesNoSystem` está en `:232-244` (atributo en `:232`, método en `:233`).
- `tests/RackCad.UI.Tests/PushBackRichPreviewTests.cs:348-367` recorre por reflexión los **métodos** y los **tipos de campo** no públicos de la ventana: exige `DrawSharedPreview` y un campo `EditorPreviewSurface`, y prohíbe `DrawModel`, `RoleBrush` y un campo `PreviewCanvasPainter`. Los reenvíos de PB-T1c son propiedades, no campos; el campo nuevo `editable` es de tipo `PushBackEditableState`.
- La celda del Controller: `model-catalog.md:51` y `:97` dan la verificación del 2026-09-30, y `:14` el `STALE` del 2026-12-29 ([PKC2]:655).
- Los precedentes del RED con Core en `success`: F4-T3B1 ([PKC]:466) y F4-T4A2, intento 2 (`F4-DRAFTS/f4-dispositions.md:1917-1928`, F4-E56: «RED `43616238873` con UI en failure y Core en success»).
- `PushBackResolver.Resolve(PushBackDesign)` (`PushBackResolver.cs:41`) y `Snapshot(PushBackSystem)` (`:387`) existen: son la vía de la opción (c) de L-1.

### M-11. Lo que sigue sin medir

- La no mutación de la línea base sigue siendo **lectura**: M-1 lista todos los consumidores y solo uno muta (el Plugin, tras cerrar).
- Si `Resolve(Snapshot(<sistema compuesto aceptado>))` da un sistema equivalente para los racks A/B (la opción (c)): no medido; el anexo L1-C lo pone como paso 1 de la cadena.
- Si `ResolveProvenance` cambia algún módulo del corpus: no medido (ningún fixture trae un módulo personalizado sin configuración, por lectura de los nombres).

---

## N. La remedición sobre el producto final de PB-TC2 (v2.3; R-1 a R-13 del §8.4 de la v2.2)

**Base:** rama `architecture/workspace-persistente-rackcad` en `cd47523b` (la custodia del cierre de PB-TC2, F4B-E12), con el producto del GREEN de PB-TC2, `b5c09f0b`. `main` en `812baf87`. Ningún Worker en el worktree (lo dijo la sesión).

**Reglas:** las de siempre. Solo lectura en el worktree; sin compilar ni ejecutar pruebas; `gh` solo de lectura (`run list`, `run view`, `run view --log`; ninguna descarga). Dos scripts nuevos de solo lectura, escritos con la herramienta de archivos y ejecutados con `python -I <script> <args>`, en `<carpeta-del-paquete>`:
- `remeasure_anchors.py`: busca en W y WC el texto exacto (sin sangría) de los quince mutantes y de las líneas de las zonas [TC2], e imprime las líneas de referencia de las anclas de D-PBT1-03, D-PBT1-13, HT1-09, HT1-10b, c-02 y HT1-19. Su salida es `remeasure-anchors-out.txt`;
- `remeasure_d2.py`: las reglas de G-L1 sobre los archivos de CT-PBT1B-14 y CT-PBT1C-04, sus autopruebas en negativo y el censo de `Name` en el Plugin. Su salida es `remeasure-d2-out.txt`.

Y el generador del contrato de PB-T1a, `make_pbt1a_contract.py`, ejecutado **solo** con `--dry --print` (N-14).

### N-0. Identidad

| Orden | Resultado |
|---|---|
| `git -C <worktree-de-I-64> rev-parse HEAD` | `cd47523ba5c5d4b79a53ae5c1afcd167b5f64929` |
| `git -C <worktree-de-I-64> status --short` | vacío (también al terminar, tras el `--dry`) |
| `git -C <worktree-de-I-64> log --oneline -8` | `cd47523b` (cierre de PB-TC2), `b5c09f0b` (GREEN), `3677847c` (RED), `eedb069c` (emisión), `8708d28f`, `5597c05b`, `4440f67d`, `f665edac` |
| `git -C <worktree-de-I-64> diff --stat b5c09f0b cd47523b -- src tests` | vacío |
| `git -C <worktree-de-I-64> diff --stat eedb069c b5c09f0b -- src tests` | WC (1 línea), W (24 insertadas, 16 quitadas) y dos archivos nuevos de UI: `PushBackDeclaredCorrection2MixedBoxTests.cs` y `PushBackDeclaredCorrection2RoundingTests.cs` |
| `git -C <worktree-de-I-64> diff eedb069c b5c09f0b -- src` | los hunks de W:815-825 (5 por 5), W:1636-1646 (equilibrado), W:1902 (1 por 1), W:3631 (−2: `Val`), W:3664 (1 por 1) y W:3680 (+12: `KeepOptional`, `MixedBoth` y `KeepCell`), y WC:195 (1 por 1) |
| `wc -l` de W, WC y WX | 3700, 936 y 793 (W: +10; WC y WX sin cambio) |
| `git -C <worktree-de-I-64> diff --quiet eedb069c cd47523b -- docs/automation/agent-execution` | sin cambios: README, `routing.md`, `model-catalog.md` y los esquemas son los mismos |

### N-1. Los miembros de W y WC (R-1, R-3)

Orden: `python -I members.py <W>` y `<WC>`, comparado con `w-members.txt` y `wc-members.txt` (los de `eedb069c`). Salidas: `w-members-b5c09f0b.txt` y `wc-members-b5c09f0b.txt`.

- **WC:** idéntico.
- **W:** solo cambia el final. Sale `Val` (W:3634). Se mueven `OptionalVal` (3665-3668 → 3663-3666), `ShownFormat` (3671 → 3669), `SameShown` (3674-3678 → 3672-3676) y `Keep` (3685-3686 → 3683-3684). Entran `KeepOptional` (3687-3688), `MixedBoth` (3691-3692) y `KeepCell` (3695-3696). `IntVal` pasa de 3688 a 3698.
- **Ningún miembro anterior a W:3631 cambia de rango.** Los hunks de W:815-825, W:1636-1646 y W:1902 sustituyen líneas sin añadir ni quitar.

### N-2. Los textos de los mutantes y de las zonas [TC2] (R-1, R-2)

Orden: `python -I remeasure_anchors.py <worktree-de-I-64>`.

| Mutante | Línea de la v2.2 | Hoy | Resultado |
|---|---|---|---|
| MU-T1A-01, 02, 03, 04, 06, 07b, 08, 09, 11, 12 y 14 | W:874, :852, :817, :827, :2133, :696, :1510, :1171, :2682, :3568 y :419 | las mismas | **igual**, texto único |
| MU-T1A-05 | WC:209 | WC:209 | **igual** |
| MU-T1A-07a (`Keep`) | W:3686 | **W:3684** | **movida −2**, texto idéntico y único |
| MU-T1A-10 (`OptionalVal`, rama vacía) | W:3668 | **W:3666** | **movida −2**, texto idéntico y único |
| MU-T1A-13 (`LoadExisting`) | W:313 | W:313 | igual; el mismo texto está también en W:296 (`LoadDesignForNew`), igual que en `eedb069c`. El mutante se ancla por línea y miembro |

**Las zonas [TC2], con el texto del GREEN:** WC:198 `composite.SetGap(Keep(GapBox, composite.Gap));`; W:1905 `var length = Keep(ModuleLengthBox, module.Length);`; W:1639, :1641, :1642 y :1646 con `KeepOptional(<caja>, advanced.<campo>)`; W:818-822 con `KeepCell(<caja>, <valor>)`; W:3665 `? (MixedBoth(field) ? field.Value : KeepOptional(field, sideValue))`. Todas en la línea prevista.

**El objetivo de cada mutante movido se conserva (lectura):**
- MU-T1A-07a quita `SameShown` de `Keep`. `Keep` sigue leyendo los cuatro portadores (W:841-844) y, por `KeepCell`, las cinco cajas de la celda fuera de «Ambos lados» con B (W:3696). CT-PBT1A-07 sigue fallando. Además, ahora `Keep` lo usan también el hueco (WC:198) y la longitud del módulo (W:1905): las pruebas de PB-TC2 que fallen en el RED se atribuyen a él en la tabla T-8.
- MU-T1A-10 cambia la rama vacía de `OptionalVal`, que PB-TC2 no tocó (W:3666 es el texto de W:3668). CT-PBT1A-10 (M-07) sigue fallando.

**Core en `success` en el RED:** `git grep` en `tests/` de los quince textos y de `SameShown`, `KeepOptional`, `KeepCell` y `MixedBoth` solo da una coincidencia falsa, `Structure.Count + 1` en `WorkspaceDraftPilotMaterializationTests.cs:526`, sobre el estado del borrador. Ninguna prueba lee esas líneas de W como texto.

### N-3. Las anclas de D-PBT1-03, D-PBT1-13, HT1-09, HT1-10b, c-02 y HT1-19 (R-3, R-4, R-13)

Orden: la segunda parte de `remeasure_anchors.py`.

Todas siguen en su línea, con el texto que nombra la v2.2: W:659 (`BuildFrom`), :675 (`AcceptComputation`), :679, :696-697, :704 (`RefreshModuleSelector`), :1688, :280, :288, :439, :391, :399 (`PostBox.SelectedId = …Defaults?.Post…`), :823, :826, :838-844 (con `Keep` en :839 y :841-844), :868 (`PostCatalogId = PostBox.SelectedId,`), :3277-3289 (`FrontalSide_Changed`), :3391; WC:117-121, :783, :789, :803-807 y :933 (`UpdateCompositeEnabled();`).

- **La pasada reentrante (HT1-10b, F-03):** el camino no cambia. Las líneas de `UpdateFrontalButtons`, `FrontalSide_Changed` y `RecomputeGate` son las de `eedb069c`. Lo esperado en CT-PBT1A-14, `pasadas-restaurar=2` y `frontal=A`, no cambia por lectura; lo fija el Worker en el paso 1.
- **Los portadores en PB-T1c:** `KeepOptional` lee `advanced.<campo>` y `MixedBoth` lee `composite.ActiveSelection` y `composite.SideBPresent`. Con los reenvíos de D-PBT1-13 (1) y (2), los dos siguen leyendo lo mismo. D-PBT1-13 ya los dejaba fuera, «por la letra».

### N-4. `RackHeaderHeightBox` y el mapa de controles (R-5, R-6)

Orden: `grep -n -E "[A-Za-z]+Box\.Value\b|\bVal\(|IntVal\(|Keep\(|KeepOptional\(|KeepCell\(|OptionalVal\(" W WC`, en `eedb069c` (35 líneas) y en `b5c09f0b` (38). Además, `sed -n 425,470p` y `sed -n 560,600p` de WC (el estado mixto).

- **`RackHeaderHeightBox` ya no redondea** (W:1639: `KeepOptional(RackHeaderHeightBox, advanced.ManualHeaderHeightOverride)`). Si la caja se ve igual que el portador, devuelve el portador exacto; si está vacía, nulo, como antes. Lo mismo valen las otras tres anulables, el hueco (WC:198) y la longitud del módulo (W:1905).
- **Las lecturas crudas que quedan** son enteros o acciones del usuario: `CellFondoOverrideBox.Value` (W:1432, al aplicar), `RackSeparatorCountBox` (W:1643-1644, entero), `DerivedPostLineHeightBox.Value` (W:2133, al aplicar), `StructureOverrideBox` (WC:271 y :280, entero con «Aplicar») e `IntVal` (enteros).
- **El estado mixto:** `SetMixed(field, true)` (WC:565) vacía la caja (`mixedNow.Add` en WC:579 y `SetNumber(null)` en WC:581). Por eso una caja mixta solo tiene valor si el usuario lo escribe, y `MixedBoth` (W:3691-3692) solo manda en ese caso. El indicador obsoleto sin lado B (M-07) no cumple `MixedBoth`, porque exige B presente.
- **WX no cambia.** No hay controles nuevos ni retirados.
- **Conclusión:** la de E y HT1-15 se sostiene. Con el catálogo entregado, solo `PostBox` decide el estado (c-01 y c-02).

### N-5. La línea base de la CI (R-7)

Orden: `gh run list -R <repo> --branch architecture/workspace-persistente-rackcad --limit 6 --json databaseId,headSha,status,conclusion,event,createdAt,name`; `gh run view 44313012995 --json jobs`; y `gh run view --job <id> --log | grep -E "Passed!|Failed!"`. Solo lectura, sin descargas.

- **El último run verde de la rama:** `44313012995`, sobre `cd47523b`, `push`, `success`, con 4 de 4 trabajos en `success`.
- **Core:** `Passed! - Failed: 0, Passed: 19070, Skipped: 0, Total: 19070`.
- **UI:** `Passed! - Failed: 0, Passed: 2401, Skipped: 21, Total: 2422`.
- Es exactamente la previsión (UI 2 422 / 21 / 2 401; Core 19 070), e igual al GREEN de PB-TC2 (`44300874797`, sobre `b5c09f0b`; F4B-E12).

### N-6. Los recuentos estáticos (R-9)

Orden: `python -I count_tests.py <worktree-de-I-64> UI <prefijos>` y lo mismo con `Core`.

| Proyecto | Prefijo | `eedb069c` | `b5c09f0b` |
|---|---|---|---|
| UI | `PushBack` (todas las clases `PushBack*`) | 538 (3 `Skip`) | **542** (3 `Skip`): +4 de PB-TC2 |
| UI | `PushBackDeclaredCorrection` | 10 | **14** |
| UI | `PushBackDeclaredCorrection2` | — | **4** (2 clases) |
| UI | `PushBackDeclaredCorrectionRoundingTests`, `…DimensionStyleTests`, `…PilotParityTests`, `…SideBNoticeTests`, `…StandardRestoreTests` | 3, 2, 2, 2, 1 | iguales (una clase cada uno: ninguno es prefijo de una clase de PB-TC2) |
| UI | `PushBackClassicCharacterization`, `PushBackFullEditorCharacterization`, `Cantilever` | 55; 16 (2 `Skip`); 67 | iguales |
| UI | `PushBackCompositeWindowTests`, `…LifecycleWiringTests`, `…LoadExistingModulesTests`, `PushBackBothModeCellBeamOverrideTests`, `PushBackModuleEditorWindowTests`, `PushBackAdvancedRackParametersWindowTests` | 52, 7, 3, 10, 13, 9 | iguales |
| UI | `BlankFront`, `WindowCensusGuardTests`, `RichEditor`, `Workspace`, `PushBackRichPreviewTests` | 33, 8, 25 (4 `Skip`), 547 (+16 `MemberData`), — | iguales; `PushBackRichPreviewTests` = **9** |
| Core | `PushBackClassicCharacterization`, `PushBackFullEditorCharacterization`, `PushBackModuleEditorCharacterizationTests` | 7 (+1 `MemberData`; 10 casos), 3, 15 | iguales |
| Core | `PushBackBlankFrontAuthorityTests`, `PushBackOutputGateTests`, `PushBack`, `Workspace` | 9, 8, 1 584 (+59), 3 271 (+33) | iguales |
| Core | `NamespaceFolderGuardTests`, `HeaderBatchContractTests`, `RackModuleEditSessionTests`, `I55DeferredProductFixCharacterizationTests` | 7, 17 (+2), 25, 3 | iguales |
| UI y Core | `PushBackStateNet`, `PushBackEditableState`, `PushBackEditorOutputGate`, `PushBackSharedReferenceGuardTests`, `PushBackWindowSharedReferenceGuardTests`, `PushBackWindowOnState` | 0 | **0**: los prefijos nuevos siguen libres |

### N-7. Los alcances (R-10)

Orden: `python -I forbidden_scope.py <worktree-de-I-64>`. Salida: `forbidden-out-b5c09f0b.txt`, que es **idéntica** a `forbidden-out.txt`, la de `eedb069c`.

- **`src/`:** 59 en PB-T1a y PB-T1c (con WX). En PB-T1b, 116 con CS dentro, que son 117 sin CS (J).
- **`tests/` de PB-T1a** (por el generador, N-14): `tests/RackCad.Tests/` entero; en UI, los 51 archivos de la lista curada de [PKC2] más las dos clases de PB-TC2 y `Workspace/`, que sigue con 24 archivos. Son **55 entradas**.
- **La raíz:** las 17 de [PKC2].
- **El total de PB-T1a:** 131.
- **Las guardas de HT1-21 a HT1-24:** PB-TC2 no tocó `tests/RackCad.Tests/` ni `src/RackCad.Application`, así que el censo del piloto, `NamespaceFolderGuardTests` y las guardas de Application son las de `eedb069c`. `PushBackRichPreviewTests` tiene 9 casos y no se modificó. Los literales de T-SG siguen, como prueba la CI verde.

### N-8. CT-PBT1A-11 frente al BOM por valor exacto (R-8)

Orden: `grep -o -E` de las claves avanzadas y de números con cuatro o más decimales en `tests/RackCad.Tests/Fixtures/I64/pb-05-topes.json`.

- **Sin coincidencias:** `pb-05-topes` no trae alturas manuales ni otros valores con más precisión de la que se muestra.
- **Además, CT-11 no pasa por ninguna caja avanzada** (acepta Seguridad sin cambios).
- **Conclusión:** la agrupación por valor exacto de D1 no cambia sus líneas del BOM. Las mide el Worker en la `BaseSha`.

### N-9. La guarda G-L1 hoy (R-11)

Orden: `python -I remeasure_d2.py <worktree-de-I-64>`.

- **Los archivos:**
  - para CT-PBT1B-14, **47 archivos** que existen: ES, EL, CS, CA, DA, RMES, `RackModuleReconciliation.cs` y los 40 `.cs` de `src/RackCad.Application/Workspace/`, con 997 miembros;
  - para CT-PBT1C-04, W y WC, con 320 miembros.
- **Las reglas, sobre el texto sin comentarios y con las cadenas vaciadas:**
  - regla 1: **0** coincidencias;
  - regla 2 (alias de un nivel): **0**;
  - regla 2 con el `foreach`: **0**;
  - regla 1b (molde a una colección mutable): **0**.
- **Los alias que encuentra la regla 2, todos de lectura:** `structure` y `signature` en ES, `baseline` en CA, `baseline` y `rebuildShape` en DA, `outputIsBlocked` y `reconciliation` en W. Ninguno se muta.
- **Conclusión: hoy no hay ninguna mutación del rack resuelto ni del informe de módulos en el camino de edición.** La premisa de D2 («Their known post-editor use is limited to the existing drawing path») se sostiene.

**Las autopruebas en negativo** (cada línea sola, sobre un texto neutro):

| Autoprueba | Regla 1 | Regla 2 | Regla 2 con `foreach` | Regla 1b |
|---|---|---|---|---|
| (a) `state.WorkingBaseline.Structure.Name = "x";` | 1 | 0 | 0 | 0 |
| (b) `var b = state.WorkingBaseline; b.Structure.Modules.Clear();` | 0 | 1 | 0 | 0 |
| (c) de la v2.2: `side.LastModuleReconciliation.Preserved.Add("x");` | 1 | 0 | 0 | 0 |
| (c) compilable: `((List<string>)side.LastModuleReconciliation.Preserved).Add("x");` | **0** | 0 | 0 | **1** |
| (d) `lastComputation.System.Name = "x";` | 1 | 0 | 0 | 0 |
| (e) `foreach (var module in workingBaseline.Structure.Modules) { module.Length = 1.0; }` | **0** | **0** | **1** | 0 |

**El miembro de lista del informe** (`RackModuleReconciliation.cs:16-48`):
- `RackModuleReconciliationResult` tiene cinco listas, `Preserved`, `Adapted`, `Removed`, `Incompatible` y `Restored`. Son `IReadOnlyList<string>` de solo lectura, con `List<string>` detrás (`:142-146`, `:212`), y un constructor `internal`.
- Se crea solo en `Reconcile` (`:212`) y se asigna solo en DA:325. ES:105 lo pone a nulo.
- Sus únicos consumidores de producto son ES (`:143`) y la ventana, que lo lee en W:2559 (`moduleOwner.LastModuleReconciliation?.Describe()`).
- **Una mutación compilable solo es posible con un molde.** La autoprueba (c) de la v2.2 no compila, y la regla 1 no ve la forma con molde. Por eso la v2.3 añade la regla 1b y la forma compilable de (c), y también (e) con el alias del `foreach`.

### N-10. La huella de CT-PBT1B-16 (R-11)

Orden: `grep -n -E "^\s+(public|internal|private|protected)[^(]*(\{ get|;|=)" src/RackCad.Domain/Systems/PushBack/PushBackSystem.cs`; `sed -n 380,440p src/RackCad.Application/Systems/PushBack/PushBackResolver.cs`; `git grep -n -E "Lazy<|\?\?=|cache|Cache" -- src/RackCad.Domain/Systems/PushBack src/RackCad.Domain/Systems/Dynamic src/RackCad.Domain/Systems/Shared`.

- **`PushBackSystem` tiene:** `Structure` (un `DynamicRackSystem` resuelto), `HighEndBeamCatalogId`, `HighEndBeams`, `RearTope`, `DefensePieceId`, `SafetySelections` y `Composite` (el lado B); `Name` escribe `Structure.Name`.
- **`PushBackResolver.Snapshot` (`:387-440`) captura la intención editable:** la estructura por el snapshot dinámico, la seguridad, el peralte por frente y nivel y `RearTope`. **No captura** lo derivado ni lo resuelto (posiciones, módulos y geometría que calcula el resolver), ni `Composite`.
- **Consecuencia:** `Persist(resolver.Snapshot(<rack>))` **no ve todas** las mutaciones posibles del rack. La v2.3 sustituye la huella por un **recorrido profundo de valores** (el `AllObjects` ampliado de CT-PBT1B-07), como previó R-11.
- **Ningún tipo de esas carpetas de Domain tiene una caché perezosa:** una lectura no cambia la huella profunda.

### N-11. Las asignaciones de `Name` en el Plugin (R-12)

Orden: `git grep -n -P "\.Name\s*=(?![=>])" -- src/RackCad.Plugin` (22 líneas); el censo de `remeasure_d2.py`; `grep -n ShowModalWindow`; y `git grep -n "DrawPushBackView("`.

- **La regex estrecha de la v2.2 sobre todo el Plugin da 10 líneas, no 3.** Son `(?<![\w.])(system|\w+\.System)\s*\.\s*Name\s*=(?!=)` en `ProjectVariableMutationExecutor.cs:169`, `RackDinamicoCommands.cs:108` y `:185`, `RackMenuCommands.cs:80`, `:102` y `:135`, `RackPushBackCommands.cs:98` y `:190`, y `RackSelectivoCommands.cs:128` y `:552`. Siete son de otros sistemas. Tal como se escribió en la v2.2, CT-PBT1B-15 habría fallado en el RED.
- **El censo de la v2.3:** la regex amplia `(?<![\w.])\w+(?:\s*\.\s*\w+)*\s*\.\s*Name\s*=(?![=>])`, sin comentarios, sobre **los archivos del Plugin que contienen `PushBack`**. Hoy son 14, y da **7 líneas**:
  - `RackMenuCommands.cs:64` (`header.Configuration.Name`);
  - `:80` (`dynamic.System.Name`);
  - `:102` (`selective.System.Name`);
  - `:121` (`cantilever.Design.Name`);
  - **`:135` (`pushBack.System.Name`)**;
  - **`RackPushBackCommands.cs:98` (`system.Name = rackName;`)**;
  - **`:190` (`system.Name = name;`)**.

  Tres son de Push Back, y no hay ningún cuarto escritor.
- **`Structure.Name` solo se escribe** en el setter de `PushBackSystem.Name` (`PushBackSystem.cs:143`).
- **Las anclas del orden:**
  - `RackPushBackCommands.cs:48` `AcApplication.ShowModalWindow(window);` → `:57` `DrawPushBackView(` (su **única** llamada en `src`) → `:98` dentro de `DrawPushBackView` (`:75`);
  - `RackPushBackCommands.cs:170` `AcApplication.ShowModalWindow(window);` → `:190`, en `EditPushBack`;
  - `RackMenuCommands.cs:34` `AcApplication.ShowModalWindow(menu);` → `:135`, en `RackCad()`.

  Las tres escrituras van después de que la ventana vuelve.

### N-12. Lo que el cierre de PB-TC2 añade para PB-T1a

Orden: `git show HEAD:<…>/F4B-PBTC2-DECLARED-CORRECTIONS/task-close-package.md`; `owner-pbtc2-nc1-decisions.md`; `f4b-dispositions.md` (F4B-E11 en `:329`, F4B-E12 en `:345`).

- **nc1 de PB-TC2:**
  - el sustituto `PBTC2-NC1-IDENTITY-SIGNAL-01` dio **FAIL** por su propiedad 5 (la evidencia de Identity no escribe el SHA falso);
  - el nc1 repetido falló además en Remote;
  - queda `nc1 = SATISFIED_BY_OWNER_DISPOSITION` **solo** para PB-TC2, intento 1, y las firmas no cambian.
  - **Consecuencia:** es previsible que nc1 de PB-T1a no reproduzca la firma con el sustituto y vuelva al Owner por C11.
- **La lección IRC-M02:** las corridas temporales también corren con el aviso de descarte sustituido.
- **El arrastre de PB-TC2 a la red de PB-T1:** «La regla de "Ambos lados" en el alcance y en los datos del frente, sin prueba propia; M-07». Lo cubren CT-PBT1A-08 y 10.

### N-13. La custodia de las autoridades de PB-T1a

Orden: `git -C <worktree-de-I-64> cat-file -e HEAD:<ruta>` de cada autoridad; `git ls-tree -r --name-only HEAD -- docs/automation/evidence/I-64-pilot/F4B-DRAFTS`.

- **En HEAD:** `owner-d1-d2-decisions.md`, `owner-pbt1-pre-decisions.md`, `owner-pbtc-nc1-decision.md`, `owner-pbtc2-nc1-decisions.md`, `owner-f4b-disposition.md`, `owner-f4b-procedure-decisions.md`, `F4B-DRAFTS/pbt1/ob065-dossier.md` y `owner-return-l1.md`, los paquetes y contratos de T1B, PB-T0, PB-TC y PB-TC2, el cierre de PB-TC2 y el esquema `gate-contract.schema.json`.
- **Aún no en HEAD** (se custodian en la emisión): el paquete de PB-T1, este registro, IRT1-1 y su comprobación acotada.

### N-14. El generador del contrato de PB-T1a, en seco

Orden (la de la sesión, literal, desde `<carpeta-del-paquete>`): `python make_pbt1a_contract.py 2026-11-18T00:00:00Z 0000000000000000000000000000000000000000 0000000000000000000000000000000000000000 --dry --print`. Su salida es `dry-out.txt`.

```text
DRY allowed 9 forbidden 131 (src 59 tests 55 root 17 ) inv 14 stops 30 required 19 evidence 9 authorities 38 mutants-ok 15 bytes 39158 sha256 CAA2210AE36906B3E0DA9AAE4BE2ABDF5C1BDFACC8F632DD858AC8612B6D7B21
a custodiar en la emision (aun no en HEAD): 4 [paquete, registro, IRT1-1, comprobacion acotada de la v2]
```

- **Lo que comprueba antes de imprimir:**
  - los quince textos de los mutantes en su línea de HEAD (los [TC2], únicos);
  - las 59 entradas de `src/`, iguales a las de PB-TC2;
  - la raíz, igual a la de PB-TC2;
  - las paradas estándar y las celdas, iguales a las de PB-TC2;
  - que los prefijos nuevos están libres;
  - que el texto es ASCII, sin términos de cierre ni rutas locales.
- No escribe nada. `git status --short` del worktree, vacío antes y después.

### N-15. Incidencia de procedimiento

- **Una orden con `python -c`.** Al leer las celdas del contrato de PB-TC2, una orden pasó la salida de `git show HEAD:<…>/F4B-PBTC2-DECLARED-CORRECTIONS/coordinator/gate-contract.json` a `python -c` para imprimir tres recuentos y las celdas. Fue de solo lectura y no escribió nada, pero **incumple la regla** «Python nunca con `-c`». No se repitió, y el generador vuelve a comprobar lo mismo con sus `assert`.
- **El generador se ejecutó sin `-I`,** con la orden literal de la sesión.

### N-16. Lo que sigue sin medir

- **Nada se compiló ni se ejecutó.** Las líneas esperadas de CT-PBT1A-01 a 14, incluidas F-03 y las de BOM de CT-11, las mide el Worker en la `BaseSha`.
- **El `config.toml` frente a la Snapshot I y el binario de Codex acreditado:** son de la sesión en la emisión (precondición 6). No se leyeron.
- **Una mutación dentro de un método de otro archivo** (Domain incluido): G-L1 no la ve. La cubren las huellas profundas de los caminos ejercidos (CT-PBT1B-16 y CT-PBT1C-02).
- **El TRX de la CI de `cd47523b`:** solo se leyó la línea de resumen del log de cada trabajo; no se descargaron los TRX.

---

## O. La guarda G-L1 de la v2.4 (V23C-I01 de la comprobación acotada de la remedición)

**Base:** la misma, `cd47523b`, con el producto de `b5c09f0b`. `git status --short` del worktree, vacío antes y después.

**Reglas:** las de siempre. Hay un script nuevo de solo lectura, escrito con la herramienta de archivos y ejecutado con `python -I`: `remeasure_gl1_v24.py`, en `<carpeta-del-paquete>`. Implementa las reglas de G-L1 de la v2.4:
- **regla 1:** la cola admite indexadores, la asignación puede caer sobre un indexador, y la lista de mutadores incluye `RecalculatePositions`;
- **regla 1b:** los moldes de estilo C, `as` e `is`;
- **regla 1c:** una ficha pasada a `Refresh`, `RefreshPhysicalModel` o un `…InPlace`;
- **regla 2:** los alias por asignación, `foreach`, `is` y parámetro de lambda.

Su salida es `remeasure-gl1-v24-out.txt`.

### O-1. Las reglas sobre el código de hoy

Orden: `python -I remeasure_gl1_v24.py <worktree-de-I-64>`.

- **CT-PBT1B-14:** los 47 archivos que existen, con 997 miembros.
  - Regla 1: **0**. Regla 1b: **0**. Regla 1c: **0**. Regla 2: **0**.
  - Los alias, todos por asignación y todos de lectura: `structure` y `signature` (ES), `baseline` (CA), y `baseline` y `rebuildShape` (DA).
- **CT-PBT1C-04:** W y WC, con 320 miembros.
  - Regla 1: **0**. Regla 1b: **0**. Regla 1c: **0**. Regla 2: **0**.
  - Los alias, todos de lectura: `outputIsBlocked` y `reconciliation` (asignación) y `module` (parámetro de un lambda en W).

**Los mutadores conocidos** (`git grep -n -P "public\s+(?:static\s+)?void\s+(UpdateHeaderHeightInPlace|RefreshPhysicalModel|RefreshPanel|Refresh|RecalculatePositions)\s*\(" -- src/RackCad.Domain src/RackCad.Application`):
- `DynamicRackSystem.RecalculatePositions` (`src/RackCad.Domain/Systems/Dynamic/DynamicRackSystem.cs:146`);
- `DynamicRackSystemBuilder.Refresh` (`src/RackCad.Application/Systems/Dynamic/DynamicRackSystemBuilder.cs:137`);
- `BracingPanelMemberBuilder.RefreshPhysicalModel` (`src/RackCad.Application/RackFrames/BracingPanelMemberBuilder.cs:11`);
- `DynamicEditorDesignAssembler.UpdateHeaderHeightInPlace` (`src/RackCad.Application/Systems/Dynamic/DynamicEditorDesignAssembler.cs:65`); su único uso, DA:274, recibe el sistema que se está armando, no una ficha.

`WorkspaceHostCore.RefreshPanel` no recibe ningún sistema y no cuenta. Los `RedrawInPlace` del Plugin no están en ningún archivo de G-L1.

### O-2. Las autopruebas

Cada línea va sola, dentro de un miembro neutro (`namespace N { class K { void M() { <línea> } } }`).

| Autoprueba | Regla que salta | Resultado |
|---|---|---|
| (a) `state.WorkingBaseline.Structure.Name = "x";` | 1 | salta |
| (b) `var b = state.WorkingBaseline; b.Structure.Modules.Clear();` | 2 (asignación) | salta |
| (c) `((List<string>)side.LastModuleReconciliation.Preserved).Add("x");` | 1b (estilo C) | salta |
| (d) `lastComputation.System.Name = "x";` | 1 | salta |
| (e) `foreach (var module in workingBaseline.Structure.Modules) { module.Length = 1.0; }` | 2 (`foreach`) | salta |
| (f1) `state.WorkingBaseline.Structure.Modules[0].Length = 1.0;` | 1 (indexador) | salta |
| (f2) `lastComputation.System.Structure.Modules[0] = null;` | 1 (asignación sobre un indexador) | salta |
| (f3) `workingBaseline.Structure.Fronts[0].Levels.Clear();` | 1 (indexador) | salta |
| (g1) `(side.LastModuleReconciliation.Preserved as List<string>).Add("x");` | 1b (`as`) | salta |
| (g2) `if (side.LastModuleReconciliation.Preserved is List<string> l) { l.Add("x"); }` | 1b (`is`) y 2 (variable `l`) | salta |
| (h1) `state.WorkingBaseline.Structure.Modules.ToList().ForEach(m => m.Length = 1);` | 2 (lambda) | salta |
| (h2) `… .ForEach(m => { m.Length = 1; });` | 2 (lambda) | salta |
| (i) `state.WorkingBaseline.Structure.RecalculatePositions();` | 1 | salta |
| (j) `builder.Refresh(state.WorkingBaseline.Structure);` | 1c | salta |
| `L1-alias2`: `var s = state.WorkingBaseline; var t = s.Structure; t.Name = "x";` | ninguna | **no salta**: límite declarado (dos niveles de alias) |
| `L2-anidado`: `state.WorkingBaseline.Structure.Modules.Where(m => Ok(m)).First().Length = 1;` | ninguna | **no salta**: límite declarado (paréntesis anidados) |
| `L3-otro-metodo`: `Touch(state.WorkingBaseline);` | ninguna | **no salta**: límite declarado (otro método) |

**Las seis formas de la comprobación acotada** (sus «0» en `review-tools/check_v23.py`) ya saltan todas: son f1, f2, f3, g1, g2 y h1.

### O-3. El generador, en seco

Orden (la de la sesión, literal, desde `<carpeta-del-paquete>`): `python make_pbt1a_contract.py 2026-11-18T00:00:00Z 0000000000000000000000000000000000000000 0000000000000000000000000000000000000000 --dry --print`. Su salida es `dry-out-v24.txt`.

```text
DRY allowed 9 forbidden 131 (src 59 tests 55 root 17 ) inv 14 stops 30 required 19 evidence 9 authorities 39 mutants-ok 15 bytes 39493 sha256 1FEF73A2173EB7EDCEFA90ADC906A3030B49C525CD70AF50F5BDF591CB85A712
a custodiar en la emision (aun no en HEAD): 5 [paquete, registro, IRT1-1, comprobacion acotada de la v2, comprobacion acotada de la remedicion]
```

- Cambia frente a la v2.3 en las **39 autoridades**: entra `F4B-DRAFTS/pbt1/independent-review-v23-check.md` en `CUSTODY`, con `assert len(CUSTODY) == 5`.
- También cambian el `Objective` y `IssuedBy`, que citan la v2.4, y la descripción del registro (apartados A a O).
- Lo demás es igual: alcances, invariantes, paradas, filtros y mutantes.

### O-4. Lo que sigue sin medir

- **Lo que G-L1 no ve,** declarado como límite en D-PBT1-15 y R-T1-12:
  - una mutación dentro de un método de otro archivo por un camino que no es la regla 1c;
  - un alias de dos niveles;
  - `ref`/`out`, las tuplas, los patrones de `switch`, los cierres y las funciones locales, LINQ de consulta, `dynamic` y la reflexión;
  - una cadena con paréntesis anidados en sus argumentos.

  Lo cubren las huellas profundas de los caminos ejercidos, INV-PBT1x-14, la comprobación acotada de cada subtarea y C17.
- **Los falsos positivos futuros de la regla 2 con lambdas:** el alias de un parámetro vale para el resto del miembro, así que un parámetro con el mismo nombre en otro lambda del mismo miembro, que mute un objeto ajeno, haría saltar la guarda. Hoy no ocurre (O-1). Si ocurre, se cambia el nombre del parámetro: es más estricto, nunca más permisivo.

---

## P. Los anclajes de PB-T1b sobre el cierre de PB-T1a (v2.5; precondición 4)

**Base:** rama `architecture/workspace-persistente-rackcad` en `5043b49a`, la custodia del cierre de PB-T1a (F4B-E14, F4B-E15 y F4B-E16), con el producto de `b5c09f0b`. Ningún Worker en el worktree (lo dijo la sesión). `git status --short` del worktree, vacío antes y después.

**Reglas:** las de siempre. Solo lectura en el worktree; sin compilar ni ejecutar pruebas; `gh` solo de lectura (`run list`, `run view`, `run view --job --log`; ninguna descarga). Dos scripts nuevos de solo lectura, escritos con la herramienta de archivos y ejecutados con `python -I`, en `<carpeta-del-paquete>`:
- `measure_pbt1b_anchors.py`: lee HEAD con `git show`, `git diff` y `git ls-tree`; ejecuta con `-I` `remeasure_gl1_v24.py` (O) y `count_tests.py` (I); y consulta la CI con `gh`. No imprime rutas locales. Su salida es `pbt1b-anchors-out.txt`;
- `check_v25_skeleton.py`: compara el esqueleto del §5.3 de la v2.4 y de la v2.5 (P-7).

Y el generador del contrato de PB-T1b, `make_pbt1b_contract.py`, ejecutado **solo** con `--dry --print` (P-7).

**Orden:** `python -I measure_pbt1b_anchors.py <worktree-de-I-64> > pbt1b-anchors-out.txt`.

### P-0. Identidad

| Orden | Resultado |
|---|---|
| `git -C <worktree-de-I-64> rev-parse HEAD` | `5043b49a2bbf707520296b353a204995b90a1d79` |
| `git -C <worktree-de-I-64> status --short` | vacío |
| `git -C <worktree-de-I-64> log --oneline -6` | `5043b49a` (cierre de PB-T1a), `73d30982` (GREEN), `15f1f225` (RED), `a3b63ccc` (emisión), `95c8126b` (paquete v2.4), `cd47523b` (cierre de PB-TC2) |
| `git -C <worktree-de-I-64> diff --stat a3b63ccc 5043b49a -- src` | vacío |
| `git -C <worktree-de-I-64> diff --stat b5c09f0b HEAD -- src` | vacío: el producto sigue siendo el GREEN de PB-TC2 |
| `git -C <worktree-de-I-64> diff --stat 5043b49a -- src` | vacío (el árbol de trabajo frente al cierre de PB-T1a) |
| `git -C <worktree-de-I-64> diff --name-status b5c09f0b HEAD -- tests` | solo `A` de las siete clases `tests/RackCad.UI.Tests/PushBackStateNet*.cs` de PB-T1a |
| `git -C <worktree-de-I-64> diff --quiet cd47523b HEAD -- docs/automation/agent-execution` | sin cambios: las celdas siguen y vencen el 2026-12-29 |
| líneas con `git show HEAD:<ruta>` | ES 622, EL 234, CS 1213, DA 578, CA 300, RMES 851, W 3700 y WC 936 |

### P-1. Las anclas por miembro y línea

**104 anclas, 0 diferencias.** Cada ancla comprueba que la línea contiene el texto que cita el paquete y nombra su miembro (con la heurística de `members.py`):
- **ES** (10): `workingBaseline` (:65), `WorkingBaseline` (:71), `AdoptLoadedBaseline` (:99-106, con :105), `moduleSession`, `moduleCommit` y `moduleSignature` (:110-112), `ModuleSession` (:124), `LastModuleReconciliation` (:143) y `ReseedModuleSession` (:176-186);
- **CS** (9): `DormantCompositeTail` (:58), `DormantTailLineOverrides` (:61), `ParkDormantTail` (:74-96), `MarkDormantTailConsumed` (:116-117), `DormantTailConsumptionPending` (:145), `dormantTailConsumed` (:147), `ActiveSide` (:167), `ActiveSelection` (:179) e `IntentDiagnostics` (:629-631);
- **RMES** (10): `RackModuleCommit` (:16) y su constructor `internal` (:18), `clone` (:74), `committedLines` y `workingLines` (:82-83), `CopyIntent` en :91, `CopyLines` (:732-742), `AreEquivalentDerivedPosts` (:744-746), `AreEquivalentLines` (:750-769) y `AreEquivalent` (:818-846);
- **DA** (16): `ModuleIntents` (:57), `AcceptComputation` (:101-107), y en `BuildDesign` (:141-436) las líneas :155, :169, :176, :183, :211, :213, :218, :273, :302 y :325; `AttachDormantTail` (:557 y :565) y `ClonePallet` (:573-576);
- **CA y PCS** (6): `ParkTailOf` (:180-242, con :189, :202 y :241), `ToTailDesign` (PCS:777) y `ToDesign` (PCS:800-811);
- **el piloto, el informe, el almacén, Domain y Dynamic** (21): PC:37, :58, :64, :134, :309; PM:108, :160; `RackModuleReconciliation.cs:16` y `:212`; RFPS:87; `UiSupport.cs:124`; `RackBomOutputGate.cs:49`; DDA:44, :50, :60 y :65; `DynamicRackSystem.cs:146`; `DynamicRackSystemBuilder.cs:137`; `BracingPanelMemberBuilder.cs:11`; `PushBackSystem.cs:143`; `PushBackDesignDocument.cs:469`;
- **W y WC** (25): W:148, :149, :273, :286, :301, :318, :391, :392, :393, :395, :396, :399, :838, :840, :852, :853, :868, :2682, :3322 y :3327; WC:783, :789, :806, :827 y :933;
- **las pruebas** (7): `WorkspaceDraftPilotStateTests.cs:211`, `:501` y `:531`; `PushBackOutputGateTests.cs:41`; `WorkspaceDraftPilotMaterializationTests.cs:706`; `PushBackDeclaredCorrectionPilotParityTests.cs:20`; `HeaderBatchContractTests.cs:524`; y el oráculo de CT-PBT1B-17, `PushBackStateNetRackControlsTests.cs:133`.

**Nota:** el miembro `Copy` de PC tiene el rango 58-62 en la heurística; la cita «PC:58-111» del paquete (HT1-20) abarca `Copy` y sus ayudantes. No es una diferencia nueva: es la misma desde `eedb069c`.

### P-2. c-09 al reconstruir (F4B-E14, pregunta 2)

**El oráculo** (`git show HEAD:tests/RackCad.UI.Tests/PushBackStateNetRackControlsTests.cs`, `:198-204`), medido por PB-T1a y verde en la CI de `5043b49a`:
- `carga [PalletDepth=48 PalletWeightUnit="kg" PalletWeight=1000]`;
- `WeightUnitBox=lb [PalletDepth=48 PalletWeightUnit="kg" PalletWeight=1000]`;
- `DepthBox=54 [PalletDepth=54 PalletWeightUnit="lb" PalletWeight=1000] descartes=0`;
- `carga ton caja=[kg] salida=[PalletDepth=48 PalletWeightUnit="ton" PalletWeight=1000] actualizar=[PalletDepth=48 PalletWeightUnit="ton" PalletWeight=1000] descartes=0`;
- `carga ton y DepthBox=54 [PalletDepth=54 PalletWeightUnit="kg" PalletWeight=1000] descartes=0`;
- y las dos de `PalletsDeepBox` (`:203-204`).

**El mecanismo, por lectura de las anclas de P-1:**
- la caja solo ofrece «kg» y «lb» (W:148) y nace en «kg» (W:149);
- la carga intenta W:395 con la unidad cargada; con «ton», la caja conserva su selección (`caja=[kg]`);
- `generalPallet` guarda la unidad cargada (W:393, W:840), pero los inputs de cada pasada llevan la de la caja (W:852);
- el ensamblador clona ese pallet (DA:176) y solo reconstruye si `MustRebuild` (DA:211-213), que compara el pallet sin la unidad (`SamePallet`, DDA:50-61, la última comparación es `Weight` en :60);
- sin reconstrucción copia la línea base (DA:273), que conserva la unidad cargada; con reconstrucción construye con la de la caja (DA:218).

**Los 17 diseños de `Fixtures/I64`** (`git ls-tree` y `git show` de cada uno): `PalletWeightUnit` = «kg» en los 17. El corpus no ejerce c-09.

**OWNER-3 y OWNER-4 por lectura:** `PalletsDeepBox` se siembra con `Math.Max(2, …)` (W:396) y se lee con `IntVal` (W:853), y el ensamblador lo usa en DA:183; el portador de botas lo añade `ApplyBootSections` en `Safety_Click` (W:2682). Ninguno pasa por una normalización del estado.

### P-3. Las rutas de PB-T1b y la lista de G-L1

- **Existen** ES y RMES; **no existen** los cuatro archivos nuevos de Application ni las seis clases de prueba.
- **La lista de CT-PBT1B-14** que existe hoy: ES, EL, CS, CA, DA, RMES, `RackModuleReconciliation.cs` y los 40 `.cs` de `src/RackCad.Application/Workspace/`: **47**.
- **Los prefijos nuevos** (`PushBackEditableState`, `PushBackEditorOutputGate`, `PushBackSharedReferenceGuardTests`, `PushBackWindowSharedReferenceGuardTests` y `PushBackWindowOnState`): 0 rutas en `tests/`.

### P-4. G-L1 de la v2.4

Orden (dentro de `measure_pbt1b_anchors.py`): `python -I remeasure_gl1_v24.py <worktree-de-I-64>`.

- **CT-PBT1B-14:** 47 archivos, 997 miembros. Regla 1: **0**. Regla 1b: **0**. Regla 1c: **0**. Regla 2: **0**. Los alias, todos por asignación y de lectura: `structure` y `signature` (ES), `baseline` (CA), y `baseline` y `rebuildShape` (DA).
- **CT-PBT1C-04:** W y WC, 320 miembros. Las cuatro reglas: **0**. Los alias, de lectura: `outputIsBlocked` y `reconciliation` (asignación) y `module` (lambda).
- **Las autopruebas:** (a), (b), (c), (d), (e), (f1), (f2), (f3), (g1), (g2), (h1), (h2), (i) y (j) saltan, cada una por su regla; `L1-alias2`, `L2-anidado` y `L3-otro-metodo` no saltan, como declara el límite.
- **Es la misma salida que O-1 y O-2**: `src` no cambió desde `b5c09f0b`.

### P-5. Los recuentos estáticos de `RequiredTests`

Orden (dentro de `measure_pbt1b_anchors.py`): `python -I count_tests.py <worktree-de-I-64> UI <prefijos>` y lo mismo con `Core`.

| Proyecto | Prefijo | `5043b49a` |
|---|---|---|
| UI | `PushBackStateNet` | **14** (6 clases de prueba) |
| UI | `PushBackClassicCharacterization`, `PushBackFullEditorCharacterization`, `Cantilever` | 55; 16 (2 `Skip`); 67 |
| UI | `PushBackDeclaredCorrectionRoundingTests`, `…DimensionStyleTests`, `…PilotParityTests`, `…SideBNoticeTests`, `…StandardRestoreTests`; `PushBackDeclaredCorrection2` | 3, 2, 2, 2, 1; 4 |
| UI | `PushBack` (todas las clases `PushBack*`) | **556** (3 `Skip`): 542 + 14 de PB-T1a |
| UI | `BlankFront`, `WindowCensusGuardTests`, `RichEditor`, `Workspace`, `PushBackRichPreviewTests` | 33, 8, 25 (4 `Skip`), 547 (+16 `MemberData`), 9 |
| Core | `Workspace` | 3 271 (+33 `MemberData`) |
| Core | `PushBackOutputGateTests`, `PushBackBlankFrontAuthorityTests`, `PushBackModuleEditorCharacterizationTests` | 8, 9, 15 |
| Core | `RackModuleEditSessionTests`, `NamespaceFolderGuardTests`, `HeaderBatchContractTests` | 25, 7, 17 (+2 `MemberData`) |
| Core | `PushBackClassicCharacterization`, `PushBackFullEditorCharacterization`, `I55DeferredProductFixCharacterizationTests`, `PushBack` | 7 (+1 `MemberData`; 10 casos), 3, 3, 1 584 (+59) |
| UI y Core | `PushBackEditableState`, `PushBackEditableStateParityTests`, `PushBackEditorOutputGate`, `PushBackSharedReferenceGuardTests`, `PushBackWindowSharedReferenceGuardTests`, `PushBackWindowOnState` | **0**: libres |

**Consecuencia:** los filtros de §6.2 son los de §6.1 más los de PB-T1b; con CT-PBT1B-17, el filtro `PushBackEditableState` de Core pasa a 13. En PB-T1c, UI `PushBack` = 556 + 1 = 557, como prevé §6.3.

### P-6. La línea base de la CI

Orden (dentro de `measure_pbt1b_anchors.py`): `gh run list --branch architecture/workspace-persistente-rackcad --limit 5 --json …`; `gh run view 44361363873 --json jobs`; y `gh run view --job <id> --log`, filtrado a las líneas `Passed!` y `Failed!`. Solo lectura, sin descargas.

- **El último run verde de la rama:** `44361363873`, sobre `5043b49a`, `push`, `success`, 4 de 4 trabajos en `success`.
- **UI:** `Passed! - Failed: 0, Passed: 2415, Skipped: 21, Total: 2436`.
- **Core:** `Passed! - Failed: 0, Passed: 19070, Skipped: 0, Total: 19070`.
- **Es exactamente la previsión del cierre de PB-T1a** (UI 2 436 / 21 / 2 415; Core 19 070), igual que su GREEN `44338475965` sobre `73d30982`. Los runs anteriores: `44333116533` (el RED de PB-T1a, `failure`, esperado), `44322004186` (`a3b63ccc`) y `44321940755` (`95c8126b`), `success`.
- **El GREEN de PB-T1b, previsto:** Core 19 070 + 16 = 19 086; UI 2 437 (21 `Skipped`, 2 416 `Passed`).

### P-7. El generador del contrato de PB-T1b, en seco, y el esqueleto

**El esqueleto:** `python -I check_v25_skeleton.py I-64-f4b-pbt1-gate-contract-package-v24.md I-64-f4b-pbt1-gate-contract-package-v25.md` da `v2.5 == v2.4 con el fragmento de c-09: True` (5 líneas; 1 006 → 1 048 caracteres). El único cambio es `` `ResolveWeightUnit` devuelve `requested`; `` tras el de `ResolvePostCatalogId`.

**El generador.** Orden (la del encargo, literal, desde `<carpeta-del-paquete>`): `python make_pbt1b_contract.py 2026-11-18T00:00:00Z 0000000000000000000000000000000000000000 0000000000000000000000000000000000000000 --dry --print`. Su salida es `make_pbt1b_contract-dry.txt`:

```text
DRY allowed 12 forbidden 213 (src 117 tests 79 root 17 ) inv 15 stops 33 required 23 evidence 8 authorities 46 anchors-ok 39 skeleton-in-HEAD v2.4 bytes 53912 sha256 E50624538521B872D0D1CF5A80D54AB55B90674001E8A5295F0EF6D96503CF86
a custodiar en la emision (aun no en HEAD): 2 ['docs/automation/evidence/I-64-pilot/F4B-DRAFTS/pbt1/pbt1b-anchors-out.txt', 'docs/automation/evidence/I-64-pilot/F4B-DRAFTS/pbt1/independent-review-pbt1b-check.md']
a sustituir en la emision: paquete en HEAD v2.4 (hace falta la v2.5); registro en HEAD con el apartado P: False
```

y las 213 rutas prohibidas, numeradas.

- **Lo que comprueba antes de imprimir:**
  - la fecha de emisión, antes del 2026-12-29 (las celdas);
  - `HEAD` desciende de `5043b49a`, y `git diff` de `src` frente a `5043b49a` y a `b5c09f0b` está vacío (el anclaje de §6.0);
  - 39 anclas de ES, RMES, CS, DA y CA, con su texto en su línea;
  - **el texto exacto del esqueleto del §5.3**, extraído del paquete de HEAD y pasado a ASCII: es el de la v2.4 (la v2.5 sin `ResolveWeightUnit`); al escribir, exige el de la v2.5, el título de la v2.5 y el apartado P en el registro;
  - las 12 rutas permitidas (2 de producto que existen y 10 nuevas que no existen);
  - el `ForbiddenWriteScope`: 117 de `src/` por `cover()`, con CS, DA, CA, `Pilot/`, `Workspace/`, Domain, Plugin y UI dentro; en `tests/RackCad.Tests/`, las 15 entradas prohibidas de PB-T0 y sus 3 clases de Core (18); en `tests/RackCad.UI.Tests/`, las 54 de PB-T1a y sus 7 clases (61); la raíz, igual a la de PB-T1a (17). Todas existen en HEAD;
  - las paradas estándar, iguales a las de PB-T1a, y las celdas, iguales a las de PB-T1a y PB-TC2;
  - los 23 filtros, con los 12 heredados iguales a los de PB-T1a; los prefijos nuevos, libres;
  - 46 autoridades: las 41 de `EXISTING`, `UNIT_CHANGE` y `EXTERNAL` existen en HEAD; de las cuatro de `CUSTODY`, el paquete y el registro están en HEAD en su versión anterior (v2.4; apartados A a O) y las otras dos aún no; y este contrato;
  - el texto: ASCII, sin términos de cierre ni rutas locales.
- **No escribe nada.** `git status --short` del worktree, vacío antes y después.

### P-7b. El generador en la v2.6 (la comprobación acotada aplicada)

Lo añade la sesión principal al custodiar. La v2.6 aplica los seis hallazgos de la comprobación acotada de PB-T1b (`independent-review-pbt1b-check.md`, 0/1/5). PBT1BC-M03 añade tres autoridades (F4-T2, F4-T3 y F4-T3C), y el `--dry` del generador pasa a ser, en `make_pbt1b_contract-dry.txt`:

```
DRY allowed 12 forbidden 213 (src 117 tests 79 root 17 ) inv 15 stops 33 required 23 evidence 8 authorities 49 anchors-ok 39 skeleton-in-HEAD v2.4 bytes 55111 sha256 D6911E30176C19B7822A83BEB6EAB3EE551F5114DFF70B8ADC075B52A3B8CE09
```

Lo de P-7 (46 autoridades y `sha256 E5062453…`) es el `--dry` de la v2.5. Los recuentos de alcance, invariantes, paradas y filtros no cambian.

### P-8. Lo que sigue sin medir

- **Nada se compiló ni se ejecutó.** Las líneas del paso 1 de PB-T1b (filas (a) a (f)) las mide el Worker en la `BaseSha`.
- **La rama de c-09 con la caja en «lb»** (fila (f)): CT-PBT1A-02 midió la caja en «kg»; la lectura de WPF (`Selector.CoerceSelectedItem` rechaza un elemento que no está en `Items`) da la rama «conserva». Lo mide el paso 1; PBT1B-C14 si no es ninguna de las dos ramas.
- **El `config.toml` frente a la Snapshot I y el binario de Codex acreditado:** son de la sesión en la emisión (precondición 6). No se leyeron.
- **El contrato completo:** el `--dry` no lo imprime; sus recuentos y sus `assert` son los de arriba, y el `sha256` es el de los bytes con los SHA de ceros.
- **Los TRX de la CI de `5043b49a`:** solo se leyó la línea de resumen del log de cada trabajo de pruebas.
- **Lo que G-L1 no ve:** sin cambio (O-4).
