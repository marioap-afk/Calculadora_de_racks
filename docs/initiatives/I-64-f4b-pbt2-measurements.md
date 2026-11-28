# I-64 F4B — Registro de mediciones del paquete de PB-T2 (v2.1)

> **Redacción para el repo público:** las rutas locales se sustituyen por marcadores (`<checkout-principal>`, `<carpeta-del-paquete>`). El resto no cambia.

**Revisiones medidas:**
- **v1:** `2f5d3decb237913e3980aa0bbd84346a7ff91f19` (rama `architecture/workspace-persistente-rackcad`), cuyo `src/` es el final de PB-T1b.
- **v2:** la punta de la rama, `95373af6116cf120bd0a8ba490833524e33c47da`: PB-T1b CERRADA (F4B-E26). Su `src/` es **igual** al de `2f5d3dec` y al del GREEN de la corrección, `a18ba345` (registro Q-1), así que las anclas de producto de los apartados A a O valen en las tres.
- **v2.1:** la punta nueva, `833ea6bfaadfab9afc698aba7f12ad2124a78585`: PB-T1c **emitida** (F4B-E27), sin cerrar. Su `src/` y sus `tests/` son **iguales** a los de `95373af6` (registro Q-7).
- `origin/main`: `812baf879612320653094a8aaf46aca0bac81383`.

**Cómo se midió:**
- solo lectura, por objetos git desde el checkout principal, que comparte el almacén con el worktree de la rama: `git -C <checkout-principal> show <sha>:<ruta>`, `ls-tree`, `grep`, `diff`, `log`, `cat-file -e` y `rev-parse`;
- el worktree de la rama **no se leyó, no se listó y no se nombró** en ninguna orden: otro proceso trabajaba en él;
- nada se compiló ni se ejecutó; sin AutoCAD; ninguna orden git de escritura;
- guiones de Python de solo lectura, escritos con la herramienta de escritura y ejecutados con `python -I` (sin `-c`), en `<carpeta-del-paquete>`:
  - **v1:** `measure_pbt2.py` y `forbidden_pbt2.py`, con sus salidas en `_work/measure-out.txt` y `_work/forbidden-out.txt`;
  - **v2:** `measure_pbt2_v2.py` (el apartado Q), `forbidden_pbt2.py` actualizado (con PB-T2c; el apartado N) y `check_questions_v2.py` (la forma de las preguntas; Q-6), con sus salidas en `_work/measure-v2-out.txt` y `_work/forbidden-v2-out.txt`;
  - **v2.1:** `check_questions_v2.py` sobre el paquete v2.1 (Q-6) y lecturas con `git show`, `grep`, `ls-tree`, `diff` y `log` (Q-7). No hubo guiones nuevos;
- los volcados de lectura (`git show … > _work/…`) se escribieron solo en `<carpeta-del-paquete>/_work/`. Las salidas de la v2 se guardaron con la codificación de la consola, y por eso las tildes de alguna línea copiada se ven mal en el archivo; no cambia ningún dato.

**Incidencias de procedimiento (declaradas):**
- **v1:** una orden de Bash de la sesión de redacción incluyó `python3 -c "pass"` (sin efecto, sin lectura ni escritura) dentro de una línea que listaba claves de un JSON.
- **v2:** una orden de Bash terminó una tubería de `git show` con `python -I -c "import sys"`. No leyó la entrada ni escribió nada, y no tuvo efecto.

Las dos incumplen la regla «Python nunca con `-c`». No se repitieron y no cambian ninguna medición.

**Marca [T1c]:** toda ancla de W, WC o WX es de la ventana de hoy, igual en `b5c09f0b`, `2f5d3dec`, `a18ba345` y `95373af6`, porque PB-T1c no se ha emitido. Hay que **volver a medirla sobre el cierre de PB-T1c**, que aplicará D-PBT1-13 del paquete de PB-T1. En `95373af6`, ese paquete sigue en v2.7 (registro Q-1); la v2.8 es el borrador de la emisión de PB-T1c.

---

## A. Base y alcance de lo que cambió desde PB-TC2

**Órdenes:**
```
git -C <checkout-principal> rev-parse 2f5d3dec
git -C <checkout-principal> rev-parse origin/main
git -C <checkout-principal> log --oneline -15 2f5d3dec
git -C <checkout-principal> diff --stat b5c09f0b 2f5d3dec -- src
git -C <checkout-principal> diff --name-only b5c09f0b 2f5d3dec -- <W> <WC> <WX> <SES> <RID> <RPC> <RMC> <EM>
```

**Resultado:**
- A-1. `2f5d3dec` = `2f5d3decb237…`. Su último commit, «I-64 F4B PB-T1b: intento 1 revisado, decisiones del Owner F4B-E21 y F4B-E22 y analisis de la correccion», es solo de documentación.
- A-2. `git diff --stat b5c09f0b 2f5d3dec -- src` da seis archivos, todos de Application, con 1 366 inserciones:
  - `PushBackEditableState.cs` (467);
  - `PushBackEditableStateComparer.cs` (502);
  - `PushBackEditableStateOwnership.cs` (177);
  - `PushBackEditorOutputGate.cs` (42);
  - `PushBackEditorState.cs` (+16);
  - `RackModuleEditSession.cs` (+162).
- A-3. W, WC, WX, `RackEditorSession.cs`, `RackEditorIdentity.cs`, `RackPushBackCommands.cs`, `RackMenuCommands.cs` y `EditorModules.cs` tienen el **diff vacío** frente a `b5c09f0b`. Las anclas de W del paquete de PB-T1, medidas en `b5c09f0b`, valen en `2f5d3dec`.
- A-4. Líneas: W 3 700, WC 936, WX 793.
- A-5. **v2:** PB-T1b está **CERRADA** (F4B-E26, CLOSED / PASS WITH LIMITATIONS sobre `a18ba345`). La corrección del intento 2 solo tocó pruebas, y el `src/` de `2f5d3dec` es el final de PB-T1b (registro Q-1).

## B. W: miembros que separa PB-T2 [T1c]

**Orden:** `measure_pbt2.py`, apartado B (`member_span`: la línea de la firma y la de la llave que cierra el cuerpo).

| Miembro | Líneas | Nota |
|---|---|---|
| `composite` (campo) | W:56-57 | PB-T1c lo convierte en `composite => editable.Editor` (D-PBT1-13 (1)) |
| `state`, `moduleOwner` | W:63, W:82 | propiedades calculadas (`composite.Active`, `composite.SideA`) |
| `session` | W:83 | `RackEditorSession<PushBackDesign, PushBackSystem>` |
| `canInsertInAutoCad` | W:85 | se usa en W:138, :3365, :3566, :3567 y :3570 |
| `isEditingExisting` | W:87 | se usa en W:278, :304, :321, :3371, :3567 y :3582 |
| `sourceProject` | W:91 | |
| Constructores públicos | W:131-134 y W:136-161 | el segundo llama a `LoadNew()` al final (W:160) |
| `BuildLibraryProjectForTest` | W:248-251 | |
| Contrato público | W:255-264 | `InsertRequested`, `UpdateOnly`, `RackId`, `RackName`, `InsertView`, `InsertSection`, `SourceProjectToInsert`, `InsertionRequest`, `SystemToInsert` y `DesignToInsert` |
| `LoadNew` | W:270-289 | `Adopt(null, null)` en W:279 |
| `LoadDesignForNew` | W:294-307 | `Adopt(null, rackName)` en W:305 |
| `LoadExisting` | W:311-324 | `Adopt(rackId, rackName)` en W:322 |
| `LoadFromModel` | W:385-440 | `suppressSync` (W:387-437) y `Recompute()` en W:439 |
| `Recompute` | W:616-723 | `AllFieldsValid` en W:625; `CommitCurrentCell` en W:635; `currentInputsAreValid` en W:627, :664, :682 y :716; `outputIsBlocked` en W:711 |
| `AllFieldsValid` | W:727-745 | |
| `AllNumericFields` | W:749-755 | 21 cajas |
| `ReadInputs` | W:833-890 | |
| `Name_Changed` | W:1234-1238 | `SetName(NameBox.Text?.Trim())` en W:1237; conectado a `TextChanged` de `NameBox` (WX:47) (registro Q-4) |
| `ShowDimensionViews` | W:1268-1284 | pone `dimensionViewsTouched = false` (W:1271) |
| `Insert_Click` | W:3245-3249 | → `RequestDraw(view, section, false)` |
| `InsertViewAt` | W:3293-3302 | → `RequestDraw(view, section, false)` |
| `Restore_Click` | W:3307-3330 | se niega con `lastComputation` nulo (W:3309-3313) |
| `Update_Click` | W:3332 | `=> RequestDraw(null, -1, updateOnly: true)` |
| `InsertBatch_Click` | W:3334-3361 | confirma módulos (W:3336), diálogo (W:3351), `SetName` (W:3352), `RequestInsertViews` (W:3354) y `Close()` (W:3360) |
| `RequestDraw` | W:3363-3419 | guardas en W:3365 y W:3371; `SetName` en W:3405; `RequestUpdate` en W:3409; `RequestInsert` en W:3414; `Close()` en W:3418 |
| `Close_Click` | W:3421 | `=> Close()` |
| `OnClosing` | W:3435-3444 | `!InsertRequested && !EditorClosePolicy.MayClose(PendingWork())` en W:3437 |
| El comentario de ADR-0029 D8 | W:3451-3455 (la frase «dirty global artificial que ADR-0029 D8», en W:3454) | registro Q-4 |
| `PendingWork` | W:3457-3459 | solo el módulo escenificado |
| `Bom_Click` | W:3463-3479 | S-7 en W:3478 |
| `SaveLibrary_Click` | W:3481-3515 | S-9 en W:3498; `EnsureId` en W:3503; `SetName` en W:3504; el proyecto en W:3507; `Save` en W:3508 |
| `RenderPreview`, `PlanFor` | W:3141-3160 (la guarda `!hasValidModel \|\| lastComputation == null`, W:3144), W:3208 (nulo con `lastComputation` nulo) | dato de V2-22: anular el cálculo en el constructor de WORKSPACE es seguro |
| `OutputBlockedAfterRecompute` | W:3552-3561 | |
| `UpdateButtons` | W:3563-3610 | «Restaurar» deshabilitado sin cálculo válido (W:3606-3609) |
| Costuras de subdiálogos | `SafetyDialog` W:1533, `RearTopeDialog` W:1539, `DefenseDialog` W:1545, `HeaderConfiguratorDialog` W:1565, `HeaderConfiguratorPresenter` W:2450, `BootPerPostWindowDialog` W:2740, `SafetyWindowDialog` W:3068 | ninguna para el BOM ni para el diálogo de guardado |

**Recuentos (apartado C del guion):**
- `.RequestUpdate(` en W: **1** (W:3409).
- `InsertRequested` en W: W:255 y W:3437.
- `Close();`: W:3360, W:3418 y W:3421.
- `InsertBatchButton` no aparece en W: `UpdateButtons` no lo toca (W:3563-3610).

## C. WC [T1c]

| Miembro | Líneas |
|---|---|
| `CompositeState` (costura) | WC:43 |
| `InitializeCompositeSection` | WC:46-58 |
| `UpdateCompositeEnabled` | WC:65-71 |
| `UpdateSideBEmptyNotice` (el aviso de D-PB-08) | WC:85-96 |
| `UpdateFrontalButtons` | WC:103-122 (solo `FrontalSideBox`, WC:116) |
| `LoadSelectedFront` → `LoadCompositeCellPanel` → `ApplyMixedSideState` | WC:445, WC:602-639 (con «Ambos lados», una caja mixta se pinta vacía) |
| **`RefreshCompositePanel`** (v2, corregido; IR1 M-10) | **WC:738-777**. Siembra desde el estado `CentralSeparatorCheck`, `FondosFrenteLabel`, `GapBox`, `SideBPresentCheck`, `SideSelectorBox` y `StructureOverrideBox` (registro Q-4). La v1 decía WC:738-746 y solo tres controles |
| `CompositeDiagnostics`, `CompositeStatusOr`, `CompositeHasBlocking` | WC:783-786, WC:789-801, WC:803-807 |
| `LoadCompositeFromDesign` | WC:813-934 (`UpdateCompositeEnabled` en WC:933) |

`Visibility` se asigna en WC solo en :68, :95 y :116. Ningún miembro de WC toca la visibilidad de los botones de inserción. WC no llama a `session.Request*` (solo a `session.Recompute`, WC:227).

## D. WX: la barra de acciones [T1c]

- **La barra:** `RestoreButton` WX:750; `BomButton` WX:755; `SaveLibraryButton` WX:757; `UpdateButton` WX:763 (`Content="Actualizar"`); `InsertLateralButton` WX:765; `InsertFrontalEntradaButton` WX:767; `InsertFrontalPosteriorButton` WX:769; `FrontalSideBox` WX:776-780 (`Collapsed` por defecto, con el tooltip «De qué lado son los dos cortes frontales de al lado»); `InsertPlantaButton` WX:781; `InsertBatchButton` WX:783; `InsertButton` WX:785; `CloseButton` WX:790 (`IsCancel="True"`).
- **Las cajas:** `NameBox` con `TextChanged="Name_Changed"` en WX:47; `AnnotationScaleBox` en WX:68-69, con `Minimum="0" MinimumInclusive="False"`. `ApplyCellButton` y `SideSelectorBox` existen en WX.

## E. La sesión y la identidad

| Miembro | Líneas |
|---|---|
| `InsertRequested`, `InsertionRequest`, `InsertRequestedRaised` | SES:106, SES:121, SES:124 |
| `RequestInsertViews` | SES:145-170 (`EnsureId` en :157; `InsertRequested = true` en :168) |
| `RequestUpdate` → `Complete` | SES:176-177 → SES:179-195 (`EnsureId` en :186) |
| `RackEditorIdentity.SetName` | RID:36-39 |
| `RackEditorIdentity.EnsureId` | RID:45-53: acuña solo si no hay id |
| `RackEditorIdentity.Adopt` | RID:61-65 |
| `EditorPendingWork.None`, `Pending(question)`, `When(bool, question)` | `src/RackCad.UI/Shell/EditorClosePolicy.cs:16`, `:21-29`, `:33-34` (`When` = `hasPendingWork ? Pending(question) : None`) |
| `EditorDiscardPrompt.Confirm`, `Substitute` | `EditorClosePolicy.cs:54`, `:57-72`; `EditorClosePolicy.MayClose` en `:108` |

## F. Quién lee la petición clásica

**Órdenes:** `git grep -n "new RackPushBackSystemWindow(" 2f5d3dec -- src` y `git grep -n -E "window\.(InsertRequested|InsertionRequest)" 2f5d3dec -- src/RackCad.Plugin src/RackCad.UI/Editor`.

- F-1. **Los cuatro sitios que construyen la ventana:** `RackPushBackCommands.cs:47` (RPB), `:168` (RACKEDITAR), `EditorModules.cs:114` y `:121` (el menú y la biblioteca).
- F-2. **RPB:** lee `window.InsertionRequest` (RPC:50) tras `ShowModalWindow` (RPC:48) y dibuja con `DrawPushBackView` (RPC:57). Hace `system.Name = rackName;` en RPC:98.
- F-3. **RACKEDITAR:**
  - `LoadExisting` (RPC:169) y `ShowModalWindow` (RPC:170);
  - `InsertRequested` (RPC:172), `DesignToInsert` y `SystemToInsert` (RPC:178-179);
  - el GUID del sobre primero (RPC:187) y `system.Name = name;` (RPC:190);
  - `UpdateOnly` (RPC:193, :198, :241, :248 y :369) e `InsertionRequest.Views` (RPC:254).
- F-4. **El menú:** `EditorModules.cs:131` devuelve la petición solo si `InsertRequested`, y `RackMenuCommands.cs:131-143` la despacha (`pushBack.System.Name = pushBackName;` en :135).
- F-5. **El estado editable nunca vuelve al llamador:** solo el diseño, el sistema, el id, el nombre y las vistas de la petición.

## G. Application: lo que dejó PB-T1b

| Miembro | Líneas |
|---|---|
| `PushBackEditableState()` | PES:40-52 |
| `Editor`, `Name` | PES:55, PES:95 |
| `LoadDesignEditor`, `RestoreEditor` | PES:109-116, PES:119-126 |
| `AdoptCarriers` | PES:130-169 (`DimensionViewsTouched = false` en :157; la seguridad en :164-168) |
| `BuildInputs` | PES:367-405 (`AuthorizedSafety` en :399) |
| `SupportOf` | PES:454-465 |
| `PushBackEditableStateComparer.Equals` | PESC:33-43 (compara `Name` en PESC:429, dentro de PESC:403-441) |
| `PushBackEditableStateComparer.Copy` | PESC:63-136: copia sobre un estado **nuevo** (el comentario de `CopyEditor`, PESC:140-141, dice que el destino es fresco) |
| `PushBackEditorOutputGate.Diagnostics`, `IsBlocked`, `StatusOr` | POG:16-40 |
| `PushBackEditableStateOwnership.Of` (M09) | `:82` la firma; la decisión «declarada» por la tabla, en `:109-112` (registro Q-4) |
| `RackModuleCommit.SameAs` y `SameEditAs` (M12) | `RackModuleEditSession.cs:76` y `:864`; los `double` con `!=` en `:102` (`Length`) y `:136` (`Height`). PESC los usa en `:228` y `:253` (registro Q-4) |

**No hay ningún método que cargue un `PushBackEditableState` desde otro en sitio.** Por eso el diseño de PB-T2 reasigna la copia en el constructor de WORKSPACE (D-PBT2-05).

## H. El núcleo de borradores

| Miembro | Líneas |
|---|---|
| `DraftSession<TState>` | DS:239-800 |
| `DraftEditStatus` | DS:150-156 (solo `Updated` y `NotEditable`) |
| `InitialDraftState` (copia) | DS:284 |
| `CurrentDraftState` (por referencia) | DS:287 |
| `IsDirty` | DS:298 (el comparador o el texto pendiente) |
| `CanEdit` | DS:331-332: registrado y sin estado explícito, o en `Conflict` |
| `SetCurrentDraftState(TState)` | DS:345-350: guarda una **copia** con el copiador inyectado; `NotEditable` si no puede editar |
| `EnterError(string)` | DS:773 |
| `DraftRegistry.Begin<TState>` | DR:143-167 (público; «A rack id is required.» en DR:145) |
| `DraftRegistry.Discard` | DR:204 |

- H-1. `DraftIntentRead<TState>.Admitted(DraftStaleBase, TState)` (DS:44), `DraftStaleBase.Create(...)` y `DraftStaleBaseEntry` son públicos: una prueba puede nacer un `DraftSession<PushBackEditableState>` con `new DraftRegistry().Begin(...)`.
- H-2. `DraftSession.cs` y `DraftRegistry.cs` no nombran `PushBack` (las guardas `WorkspaceDraftBoundaryTests.cs:193`, `PushBack|Selectiv|…`, y `WorkspaceDraftPilotIntentGuardTests.cs:233-244`). Sus nombres de tipo no pueden contener `Editor`, `Tab`, `Handler` ni `Subscription` (`WorkspaceDraftBoundaryTests.cs:204`). **Por eso el puerto de PB-T2 no va en `Drafts/`.**
- H-3. **Las guardas del puerto del panel** prohíben que tipos de `Drafts` o de `Systems` crucen sus valores (`WorkspaceEditIntentHostTests.cs:1030-1036`, `:1053-1054`; `WorkspaceLifecycleHostTests.cs:1477-1480`), salvo la excepción declarada de `TryUpdatePilot` (`WorkspaceEditIntentHostTests.cs:1039-1046`). No miran `src/RackCad.UI/Systems/PushBack`. El diseño sigue el mismo criterio: el resultado del puerto de la ventana es un tipo propio, no `DraftEditResult`.
- H-4. Application/Systems ya usa `RackCad.Application.Workspace.Drafts` en el piloto (`PushBackPilotFieldAdapter.cs:6`, `PushBackPilotIntentReader.cs:10`, …).
- H-5. **La marca *known clean*:** toda operación del host que cambia un borrador la acaba (`NotePilotEdit`; `WorkspaceHostCore.cs:66`, `:832`; `WorkspaceLifecycleGuardTests.cs:509-520`). Es dato de A2-1 (V2-23).

## I. El piloto hoy

- I-1. `WorkspaceHostCore.TryBeginPilotEdit` nace el borrador con el comparador y el copiador del piloto (`WorkspaceHostCore.cs:347-352`). El tipo del borrador es `PushBackPilotDraftState` (`Editor`, `Inputs`, `Origin`).
- I-2. `PushBackPilotStateComparer.Copy` no copia módulos ni cola (H-PB-27; PC:58-111).
- I-3. `PushBackPilotMaterializer.Materialize` (PM:126) rechaza el compuesto y la cola (`CompositeOutOfPilotReason`, PM:106; `DormantTailOutOfPilotReason`, PM:108). `NonPilotContentReason` (PM:112) rechaza componer lo que no sea la escala.
- I-4. El panel muestra «Usa RACKEDITAR para editarlo con el editor clásico.» y no lo lanza (`PushBackPilotIntentReader.cs:56`; `WorkspacePilotFieldViewModel.cs:281`).
- I-5. El botón del panel se llama «Actualizar» (`WorkspacePanelView.xaml:239`), con el nombre accesible «Actualizar el dibujo con el borrador» (`:241`).

## J. La validación de campos

**Órdenes:** `git grep -n -E "HasError|AllFieldsValid|currentInputsAreValid =|IsValid|Validate\(" 2f5d3dec -- <W> <WC>` y lecturas de `NumericFieldValidation.cs` y `PushBackPilotFieldAdapter.cs`.

- J-1. **La regla de cada caja:** `NumericField` → `NumericFieldValidation.Validate` (`src/RackCad.UI/Controls/NumericFieldValidation.cs:61-137`), con `LocalizedNumberParser` (:102) y los mensajes «Escribe un valor.» (:63), «Valor no numérico.» (:64) y «El valor debe ser > X.» (:131, llamado en :113). La firma está en :76.
- J-2. **La compuerta de cada pasada:** `AllFieldsValid` sobre las 21 cajas (W:727-755). La usan `Recompute` (W:625) y siete manejadores: `FrontCount_Changed` (W:1087), `SetFrontActive` (W:1130), `SelectSingleCell` (W:1198), `MutateStructure` (W:1211), `ApplyScope` (W:1343), `WriteCellProperties` (W:1420) y `ApplyFrontData` (W:1506).
- J-3. **Las cajas que se validan en su manejador:** `FrontCountBox` (W:1106), `ModuleLengthBox` (W:1900), `DerivedPostLineHeightBox` (W:2127), `GapBox` (WC:193) y `StructureOverrideBox` (WC:271).
- J-4. **La validación de Application:** `PushBackAdvancedRackParameters.Validate` (`PushBackAdvancedRackParameters.cs:31`), que llama el ensamblador (`PushBackEditorDesignAssembler.cs:190`) y lanza. La ventana la convierte en un cálculo inválido (W:662-670).
- J-5. **La compuerta de salida:** `outputIsBlocked = CompositeHasBlocking(...)` (W:711; WC:803-807). La consulta `OutputBlockedAfterRecompute` (W:3552-3561) en `RequestDraw` (W:3400), `InsertBatch_Click` (W:3339), `Bom_Click` (W:3473) y `SaveLibrary_Click` (W:3493).
- J-6. **La segunda implementación de una regla de caja:** el campo de escala del panel valida con `PushBackPilotFieldAdapter.Validate` (`PushBackPilotFieldAdapter.cs:35-53`):
  - los mismos textos: «Escribe un valor.» (:27), «Valor no numérico.» (:29) y «El valor debe ser > 0.» (:31);
  - el mismo analizador: `LocalizedNumberParser.TryDouble` (:47), con `value <= 0.0` (:52).

  Es la regla de `AnnotationScaleBox` (WX:68-69, `Minimum="0" MinimumInclusive="False"`) escrita otra vez, en Application.

## K. Las guardas de texto y de reflexión que tocan estas zonas

**Orden:** `measure_pbt2.py`, apartado J. Las filas corregidas en la v2 (IR1 M-10) van marcadas.

| Guarda | Ancla | Qué fija |
|---|---|---|
| T-SG `ClassicWindowSources` | `PushBackClassicCharacterizationSourceGuardTests.cs:169-176` | concatena **todos** los `.cs` y `.xaml` de `src/RackCad.UI/Systems/PushBack` (diez archivos hoy, apartado K del guion) |
| T-SG `Body()` | `:179-203` | el partidor de cuerpos que usan las guardas de texto |
| T-SG CT-19 | `:236-248` | ninguno de `KeyDown`, `PreviewKeyDown`, `InputBindings`, `KeyBinding`, `TabIndex`, `IsTabStop`, `KeyboardNavigation`, `.Focus(` ni `DialogResult` en esa clausura |
| T-SG «InsertRequestedRaised» | `:251-264` | nadie en `src` se suscribe |
| T-SG S-7 | `:281-284` | `new RackBomWindow(lastComputation.Bom) { Owner = this }.ShowDialog();` |
| T-SG S-8 | `:287-291` | `TryShow(this, RackSystemKind.PushBack, options, out var views)` |
| T-SG S-9 | `:294-298` | `UiSupport.PromptSaveToLibrary(this, NameBox.Text, "sistema")` |
| T-SG CT-21 | `:303-319` | el host del menú, dos veces |
| T-SG CT-22 | `:324-341` | el orden prompt < `EnsureId` < `SetName` < proyecto < `Save` dentro de `SaveLibrary_Click` |
| T-SG CT-22b | `:344-352` | el configurador escribe su disposición en el perfil al cerrarse (dato de L-PBT2-11) |
| T-SG CT-23 | `:357-379` | `InsertBatch_Click`: confirma antes del diálogo; `RequestInsertViews` después; `Close()` al final; sin `canInsertInAutoCad` |
| T-SG CT-16, S-5, S-6 | `:217-231`, `:269-272`, `:275-278` | los dueños del configurador, la defensa y los topes |
| T-PG CT-24 | `PushBackClassicCharacterizationPluginGuardTests.cs:198-207` | `Lacks("Owner =")` en RACKPUSHBACK |
| T-CL `PromptProbe`, `CloseQuietly` | `PushBackClassicCharacterizationCloseTests.cs:47-65`, `:124-131` (la respuesta «Sí» en `:126`) | el patrón del `finally` de CT-PBT2B-06 |
| **G-10** (v2, corregido) | `WorkspacePilotUpdateIntegrationGuardTests.cs:725-767` | `.RequestUpdate(` sobre **todo el producto**: `Product() => Src.AllProduct()` (`:30`; registro Q-4). Una lista cerrada de permitidos: el controlador, la compuerta, cuatro ventanas con una llamada cada una (W entre ellas) y el modelo de vista del campo. La v1 decía «ningún otro archivo de `src/RackCad.UI`»: también cubre Application |
| **G9** (v2, corregido) | `:703-719` | ningún archivo **de producto** que nombre el piloto usa una API de escritura. G9 no mira las pruebas: la clase de concordancia (CT-PBT2B-10), que nombra el piloto, es una prueba. La v1 decía que «los archivos nuevos no nombran el piloto», y era falso para esa clase |
| I-55, Fact3, desviador | `I55DeferredProductFixCharacterizationTests.cs:8-15`; `:381-401`; `:311-318` | literales de W |
| Reflexión de la vista previa | `PushBackRichPreviewTests.cs:349-367` | sin métodos `DrawModel` ni `RoleBrush`; un campo `EditorPreviewSurface`; ningún campo `PreviewCanvasPainter` |
| Censo de ventanas | `WindowCensusGuardTests.cs:69` (`Utilities`, con `RackBomWindow`) | ningún `Window` nuevo sin censar |
| Núcleo de borradores | `WorkspaceDraftBoundaryTests.cs:23-56`, `:193`, `:204`; `WorkspaceDraftPilotIntentGuardTests.cs:232-244` | los archivos esperados, sin sistemas y sin nombres prohibidos |
| El puerto del panel | `WorkspaceEditIntentHostTests.cs:1030-1046`; `WorkspaceLifecycleHostTests.cs:1477-1480`; `WorkspacePilotEditGuardTests.cs:385` | los valores del puerto del panel; la excepción de `TryUpdatePilot` |
| *Known clean* | `WorkspaceLifecycleGuardTests.cs:509-520` | todo cambio de un borrador acaba la marca |
| El contrato de cierre | `RichEditorCloseContractTests.cs:69-86` («no se inventa un dirty global que el producto no tiene», `:77`) | solo Push Back y la cabecera declaran `OnClosing` (dato de Q-PBT2-1) |
| Biblioteca | `PushBackLibrarySaveGateTests.cs:95` | la prueba llega al diálogo real en una ventana sin mostrar, que lanza (`ReachedTheSaveDialog`) |
| BOM | `DialogWindowCharacterizationTests.cs:91`, `:125`, `:157` | la ventana del BOM compartida |
| Colección del aviso | `EditorDiscardPromptCollection.cs:27-30` | sin paralelismo |
| **G-L1c** (v2) | `95373af6:tests/RackCad.Tests/PushBackSharedReferenceGuardTests.cs`: `Tokens` :33, `Rebind` :39, `Members` :86, `Aliases` :110, `Violations` :152, `Inject` :203, CT-14 :212; las **quince** autopruebas en `:223-235` y `:238-239` | registro Q-3 |
| **Las huellas** (v2, corregido) | `95373af6:tests/RackCad.Tests/PushBackEditableStateCopyTests.cs`: `AllObjects(object root, Func<FieldInfo, bool> skip = null)` en **:278** y `Fingerprint` en **:1080** | la v1 (sobre `2f5d3dec`) decía `:272` y `:1039`, sin el parámetro `skip` (registro Q-3) |

## L. Prefijos libres

**Orden:** `git grep -n -F <token> 2f5d3dec -- src tests`, para cada uno.

Libres: `PushBackEditorMode`, `PushBackDraftEditor`, `PushBackDraftStatePort`, `PushBackDraftSessionPort`, `ForDraft(`, `ApplyToDraft`, `PushBackWorkspaceEditor`, `PushBackWorkspaceMode`, `CT_PBT2`, `BomWindowDialog`, `LibraryPathDialog` y el literal «Aplicar al borrador». En `95373af6` siguen libres: entre `2f5d3dec` y `95373af6` solo cambian cinco archivos de prueba de PB-T1b (registro Q-1), y ninguno es de PB-T2.

## M. Recuentos estáticos de pruebas

**Orden:** `measure_pbt2.py`, apartado M (cuenta `[Fact]` y `[Theory]` por archivo; una `Theory` con `MemberData` cuenta como 1). **No son los `MinSelected`**, que salen de las corridas de CI de los contratos anteriores, salvo las tres filas marcadas.

| Prefijo | Métodos | Archivos | `MinSelected` vigente |
|---|---|---|---|
| UI `PushBack*` | 541 | 73 | 556 medido en `5043b49a` (557 con PB-T1b) |
| UI `PushBackClassicCharacterization*` | 48 | 6 | 55 |
| Core `PushBackClassicCharacterization*` | 8 | 1 | 10 |
| UI `PushBackStateNet*` | 14 | 7 | 14 |
| Core `PushBackEditableState*` | 13 | 3 | 13 |
| UI `PushBackEditableState*` | 1 | 1 | 1 |
| Core `PushBackEditorOutputGate*` | 1 | 1 | 1 |
| Core `PushBackSharedReferenceGuard*` | 2 | 1 | 2 |
| UI `PushBackDeclaredCorrection*` | 14 | 8 | 3, 2, 2, 2, 1 (PB-TC) y 4 (PB-TC2) |
| UI `PushBackFullEditorCharacterization*` | 18 | 7 | 16 |
| Core `PushBackFullEditorCharacterization*` | 3 | 3 | 3 |
| UI `Cantilever*` | 64 | 3 | 67 |
| UI `WindowCensusGuardTests` | 8 | 1 | 8 |
| **UI `PushBackRichPreviewTests`** (v2) | 9 `Fact`, 0 `Theory` | 1 | **9**, fijado por el recuento estático (registro Q-4) |
| **UI `PushBackLibrarySaveGateTests`** (v2) | 5 `Fact`, 0 `Theory` | 1 | **5**, ídem |
| **UI `DialogWindowCharacterizationTests`** (v2) | 23 `Fact`, 0 `Theory` | 1 | **23**, ídem |

Sin ninguna `Theory`, el recuento estático es el número de casos que selecciona el filtro. Por eso las tres filas nuevas fijan su `MinSelected` (IR1 M-11).

**La línea base de la CI** (v2):
- **medida:** la CI del GREEN de la corrección de PB-T1b, `44438060317`, 4/4: Core 19 086; UI 2 437, con 2 416 pasadas y 21 omitidas (el cierre de PB-T1b; registro Q-5). La v1 citaba la del intento 1, `44405531606`, con los mismos recuentos;
- **prevista:** PB-T1c suma Core + 2 y UI + 2 (paquete de PB-T1 §6.3): **Core 19 088 y UI 2 439, con 21 `Skipped`**. Se mide en el cierre de PB-T1c.

## N. El alcance prohibido de `src/` (algoritmo `cover()` de [PKC-C])

**Órdenes:** `python -I forbidden_pbt2.py <checkout-principal> 2f5d3dec` (v1) y `python -I forbidden_pbt2.py <checkout-principal> 95373af6` (v2).

| Subtarea | Rutas permitidas | Entradas de `src/` prohibidas en `95373af6` |
|---|---|---|
| PB-T2a | 4: dos archivos nuevos de `src/RackCad.Application/Systems/PushBack/` y dos clases nuevas de `tests/RackCad.Tests/` | **87** (igual que en `2f5d3dec`) |
| PB-T2b | 8: W, `src/RackCad.UI/Systems/PushBack/PushBackEditorMode.cs` y seis archivos de prueba (cuatro de UI y dos de Core) | **60** (igual que en `2f5d3dec`) |
| PB-T2c (v2) | 3: W, WC y `tests/RackCad.UI.Tests/PushBackWorkspaceCarryoverTests.cs` | **59**. Tras PB-T2b sube a **60**, porque `PushBackEditorMode.cs` ya existe y sale como una entrada prohibida más |

Ninguna ruta de producto falta en `95373af6`, ninguna nueva existe ya y no queda nada sin cubrir (`sin-cubrir []` en las tres). El generador de cada contrato lo vuelve a calcular con sus `assert` sobre su `BaseSha`.

## O. CI: dependencias de los trabajos

**Orden:** `git show 2f5d3dec:.github/workflows/ci.yml` (igual en `95373af6`).

- `tests` (Core, ubuntu), `build-ui` y `ui-tests` (windows) son independientes. `ui-tests` tiene `timeout-minutes: 25` y `--blame-hang-timeout 5m`, porque «un cuelgue sería indefinido» (`ci.yml:213-241`).
- `build-plugin` tiene `needs: [tests, build-ui]` (`ci.yml:254-258`). **Con Core en `failure` en el RED, `build-plugin` sale omitido** (la lección M14 de PB-T1b, F4B-E22). En la v2, los tres RED tienen Core en `failure`:
  - PB-T2a, por CT-PBT2A-01 a 03;
  - PB-T2b, por CT-PBT2B-08;
  - PB-T2c, por CT-PBT2B-08 bajo MU-T2C-03.

## P. Lo que no se pudo medir

- Nada se compiló ni se ejecutó.
- **La ventana de PB-T1c no existe:** todo W, WC y WX es de hoy [T1c]. El texto de CT-PBT1C-01 a 04 (sus gestos y sus guardas) tampoco existe.
- **v2, corregido (IR1 I-01):** la v1 decía que las reglas corregidas de G-L1 «no están en `2f5d3dec`» y que las pondría la corrección. **Ya existen:** la corrección del intento 2 de PB-T1b está cerrada (F4B-E26), y G-L1c, con las quince autopruebas, está en `a18ba345` y en `95373af6` (registro Q-3).
- **Por lectura, sin medir:**
  - que un `Close()` sobre una ventana sin mostrar pase por `OnClosing` (lo usan las pruebas de PB-TC2 y PB-T1a con el aviso sustituido, D-PBTC2-08);
  - que la carga del estado en WORKSPACE sea un punto fijo del recálculo, también con una selección no por defecto y con un nombre nulo o con espacios (CT-PBT2B-02);
  - los valores de `DimensionViewPolicy.FromEditor` de la normalización N-PBT2-01, y si algún diseño del corpus trae una seguridad no autorizada (el paso 1 de PB-T2b);
  - el coste de `PushBackEditableStateComparer.Equals` por llamada (M15 de PB-T1b: CT-06 tarda 27,7 s en total);
  - el mensaje exacto de `ArgumentNullException` y de `ArgumentException` con `paramName` en .NET 8;
  - que los tres mutantes de PB-T2c, aislados, hagan fallar su CT.
- **No medido aquí:** el `config.toml` frente a la Snapshot J, el binario acreditado y la vigencia de las celdas. Los comprueba la sesión al emitir.

## Q. Mediciones nuevas de la v2 (sobre `95373af6`)

**Órdenes:**
```
python -I measure_pbt2_v2.py <checkout-principal>
python -I forbidden_pbt2.py <checkout-principal> 95373af6
python -I check_questions_v2.py <carpeta-del-paquete>/I-64-f4b-pbt2-gate-contract-package-v2.md
git -C <checkout-principal> diff --stat a18ba345 95373af6 -- tests src
git -C <checkout-principal> ls-tree --name-only 95373af6 -- docs/automation/evidence/I-64-pilot/F4B-DRAFTS/coordinator/ docs/automation/evidence/I-64-pilot/F4B-DRAFTS/pbfull/ docs/automation/evidence/I-64-pilot/F4B-DRAFTS/pbt1b/
git -C <checkout-principal> show 95373af6:src/RackCad.Application/Systems/PushBack/PushBackEditableStateOwnership.cs
git -C <checkout-principal> show 95373af6:src/RackCad.Application/Systems/Shared/RackModuleEditSession.cs
git -C <checkout-principal> show 95373af6:src/RackCad.UI/Systems/PushBack/RackPushBackSystemWindow.xaml.cs    (W:3141-3160 y W:3208)
```

### Q-1. La punta de la rama

- `95373af6` = `95373af6116cf120bd0a8ba490833524e33c47da`.
- **Los cuatro últimos commits:**
  - `95373af6` «I-64 F4B PB-T1b: cadena, decisiones del Owner y cierre (F4B-E23 a F4B-E26)»;
  - `a18ba345` «… (correccion, intento 2): GREEN, la implementacion del intento 1 restaurada sin cambios de producto»;
  - `126fd65f` «… (correccion, intento 2): RED, el esqueleto inerte devuelto y las pruebas corregidas»;
  - `2f5d3dec`.
- **Lo que cambia:**
  - `git diff --name-only 2f5d3dec 95373af6 -- src` y `git diff --name-only 2f5d3dec a18ba345 -- src`: **vacíos**;
  - `git diff --stat a18ba345 95373af6 -- tests src`: **vacío**;
  - entre `2f5d3dec` y `95373af6` solo cambian cinco archivos de prueba: `tests/RackCad.Tests/PushBackEditableStateCopyTests.cs`, `PushBackEditableStateLoadTests.cs`, `PushBackEditorOutputGateTests.cs` y `PushBackSharedReferenceGuardTests.cs`, y `tests/RackCad.UI.Tests/PushBackEditableStateParityTests.cs`.
- **El paquete de PB-T1** en `95373af6` (`docs/initiatives/I-64-f4b-pbt1-gate-contract-package.md`) sigue en **v2.7**.

### Q-2. Las decisiones custodiadas en `95373af6`

| Ruta | Existe |
|---|---|
| `F4B-DRAFTS/coordinator/owner-f4b-correction-unit-change-decision.md` (F4B-E25) | sí |
| `F4B-DRAFTS/pbt1b/owner-return-verify-stop.md` (la vuelta de F4B-E25) | sí |
| `F4B-DRAFTS/coordinator/owner-f4b-dpb07-extensiondata-decisions.md` (F4B-E23 y F4B-E24) | sí |
| `F4B-DRAFTS/pbfull/owner-return-dpb07.md` y `owner-return-extensiondata.md` (sus vueltas) | sí |
| `F4B-PBT1B-LOSSLESS-STATE-TYPE/task-close-package.md` (F4B-E26) | sí |
| `F4B-DRAFTS/coordinator/owner-f4b-disposition.md`, `owner-f4b-procedure-decisions.md`, `owner-d1-d2-decisions.md`, `owner-config-snapshot-j-decision.md`, `owner-f4b-opaque-collections-decision.md` y `owner-pbt1b-rework-decisions.md` | sí |

- **En `f4b-dispositions.md`:** F4B-E23 en :668, F4B-E24 en :684, F4B-E25 en :693 y F4B-E26 en :711.
- **F4B-E25, literal de la respuesta:** «Regla F4B y reverificar (Recomendado)». **Sus tres condiciones:**
  1. todo commit de `AuthorityRevision..BaseSha` que toca la ruta es un RedSha o un CurrentSha de una entrega de un Worker de la misma cadena;
  2. la ruta está en el AllowedWriteScope de la delegación de esa entrega;
  3. ningún commit de la sesión la toca.

  «Las rutas EXTERNAL siguen prohibidas en ese diff sin excepción».

### Q-3. G-L1c y las huellas, en `95373af6` (igual que en `a18ba345`)

- **`PushBackSharedReferenceGuardTests.cs`:** `Tokens` :33, `Rebind` :39, `Members` :86, `Aliases` :110, `Violations` :152, `Inject` :203 y `CT_PBT1B_14_…` :212.
- **`Rebind`** = `lastComputation`, `lastCompositeSystem`, `workingBaseline` y `LastModuleReconciliation`.
- **Las quince autopruebas** están en el arreglo `selfTests`: (a) a (j), sin (d), en `:223-235`, y (k1) y (k2) en `:238-239`, tras el comentario «I01 (the correction of attempt 2): an alias plus a mold». Los textos de (k1) y (k2) son:
  - (k1): `var r = side.LastModuleReconciliation; (r.Preserved as List<string>).Add("x");`
  - (k2): `var p = side.LastModuleReconciliation.Preserved; ((List<string>)p).Add("x");`
- **Sobre la extracción:** el extractor de nombres del guion se detiene en el primer `foreach`, que aparece dentro del texto de la autoprueba (e). Por eso imprime solo cuatro nombres. El recuento correcto es el de las líneas: 15.
- **IR1 citaba las autopruebas en `a18ba345:…:238-253`.** El archivo es igual en `a18ba345` y en `95373af6`, y la medición da `:223-239`. Vale esta.
- **`PushBackEditableStateCopyTests.cs`:** `AllObjects(object root, Func<FieldInfo, bool> skip = null)` en :278 y `Fingerprint` en :1080.

### Q-4. Anclas para los hallazgos

| Dato | Ancla | Para |
|---|---|---|
| `RefreshCompositePanel` | WC:738-777; siembra `CentralSeparatorCheck`, `FondosFrenteLabel`, `GapBox`, `SideBPresentCheck`, `SideSelectorBox` y `StructureOverrideBox` | M-10 |
| G-10 | `Product() => Src.AllProduct();` | M-10 |
| `Name_Changed` | W:1234; `NameBox` con `TextChanged="Name_Changed"` en WX:47 | I-07 |
| El comentario de D8 | W:3454 (en W:3451-3455) | I-08 |
| «dirty global» | `RichEditorCloseContractTests.cs:77` | I-08 |
| El comparador compara `Name` | PESC:429 | I-07 |
| `Ownership.Of` | `:82` la firma; `:109-112` la decisión «declarada» por la tabla (`FirstProperty`, `Table.Any`, `Envelope` o `Unclassified`) | I-06 (M09) |
| `SameAs` y `SameEditAs` | RMES:76 y :864; `a.Length != b.Length` en RMES:102; `DerivedPostOverrides[index].Height != …` en RMES:136; PESC:228 y :253 | I-06 (M12) |
| `PushBackRichPreviewTests`, `PushBackLibrarySaveGateTests`, `DialogWindowCharacterizationTests` | 9, 5 y 23 `Fact`, sin `Theory` | M-11 |
| CT-PB0-12 a 16 (`PushBackFullEditorCharacterizationSubdialogTests.cs`) | :38, :104, :161, :200 y :237; CT-PB0-14 usa `AbrirFueraDePantalla` (la ventana mostrada) | I-03 |
| `RenderPreview` y `PlanFor` | W:3141-3160 (la guarda en W:3144) y W:3208 | M-14 |

### Q-5. La duración de los Worker de PB-T1b (`relay-record.json`)

| Corrida | `StartUtc` | `EndUtc` | Duración |
|---|---|---|---|
| `1/R20261118T204715Z-a4e2` (el intento 1: 17 CT y el producto) | 2026-11-18T20:47:22Z | 2026-11-18T21:57:21Z | **70 min** |
| `2/R20261118T223111Z-4c04` (la corrección, solo pruebas) | 2026-11-18T22:31:17Z | 2026-11-19T01:45:56Z | **3 h 15 min**, con una interrupción del transporte |

- **La CI del GREEN de la corrección**, según el cierre: `44438060317`, 4/4; Core 19 086; UI 2 437, con 2 416 pasadas y 21 omitidas.
- **Es dato de V2-21** (IR1 M-13): la v1 ponía en PB-T2b 13 CT con el producto, frente al tope de 60 minutos (§16.4).

### Q-6. La forma de las preguntas (`check_questions_v2.py`)

| Pregunta | `header` (caracteres) | Texto (caracteres) | Opciones | La recomendada primero, y solo ella |
|---|---|---|---|---|
| Q-PBT2-1 | `PBT2 cierre` (11) | **361** en la v2.1, con el matiz de N-04 (376 en la v2) | 3 | sí |
| Q-PBT2-2 | `PBT2 bloqueo` (12) | 198 | 2 | sí |
| Q-PBT2-3 | `PBT2 valid.` (11) | 243 | 3 | sí |
| Q-PBT2-4 | `PBT2 emisión` (12) | 307 | 2 | sí |

Las cuatro tienen menos de 400 caracteres, un `header` de 12 como máximo, y de 2 a 4 opciones.

### Q-7. La v2.1: la punta `833ea6bf` y los datos de N-01 a N-07

**Órdenes:**
```
git -C <checkout-principal> rev-parse 833ea6bf
git -C <checkout-principal> log --oneline -3 833ea6bf
git -C <checkout-principal> diff --name-only 95373af6 833ea6bf -- src tests
git -C <checkout-principal> diff --name-only 95373af6 833ea6bf
git -C <checkout-principal> show 833ea6bf:docs/automation/evidence/I-64-pilot/F4B-DRAFTS/f4b-dispositions.md        (F4B-E27)
git -C <checkout-principal> show 833ea6bf:docs/initiatives/I-64-f4b-pbt1-gate-contract-package.md                     (la cabecera y CT-PBT1C-04)
git -C <checkout-principal> show 833ea6bf:src/RackCad.UI/Systems/PushBack/RackPushBackSystemWindow.xaml.cs           (W:616-672, W:3307-3330, W:3481-3515)
git -C <checkout-principal> show 833ea6bf:tests/RackCad.UI.Tests/PushBackFullEditorCharacterizationSubdialogTests.cs
git -C <checkout-principal> show 833ea6bf:tests/RackCad.UI.Tests/PushBackDeclaredCorrection2MixedBoxTests.cs
git -C <checkout-principal> show 833ea6bf:tests/RackCad.UI.Tests/PushBackDeclaredCorrectionSideBNoticeTests.cs
git -C <checkout-principal> show 833ea6bf:tests/RackCad.UI.Tests/PushBackClassicCharacterizationCloseTests.cs
git -C <checkout-principal> grep -n "static void SelectModule(\|static void ClickNamed(" 833ea6bf -- tests
```

**La punta (N-06):**
- `833ea6bf` = `833ea6bfaadfab9afc698aba7f12ad2124a78585`, «I-64 F4B PB-T1c: emite el contrato de gate (AuthorityRevision af46fcd5)». Su padre es `af46fcd5`, «I-64 F4B PB-T1: paquete del contrato v2.8 y anclajes de PB-T1c (F4B-E27)», y el de este, `95373af6`.
- `git diff --name-only 95373af6 833ea6bf -- src tests` sale **vacío**. Solo cambian documentos:
  - el paquete de PB-T1, ya en **v2.8**, y su registro;
  - el contrato emitido y su generador (`F4B-PBT1C-LOSSLESS-STATE-WINDOW/coordinator/gate-contract.json` y `make_pbt1c_contract.py`);
  - los borradores de `F4B-DRAFTS/pbt1/`;
  - `f4b-dispositions.md`, `I-64-evidence.md` y `docs/automation/state/I-64.yml`.
- **F4B-E27** está en `f4b-dispositions.md:761`: la emisión de PB-T1c con la v2.8, sin volver al Owner, con PBT1C-C13 y las dieciséis autopruebas de CT-PBT1C-04.
- **Las dieciséis de CT-PBT1C-04** ([PT1] v2.8, D-PBT1-15 y §5.4) son las quince comunes de CT-PBT1B-14 más la (d), «propia de la ventana».

**El nombre y la biblioteca (N-01):**
- En `SaveLibrary_Click` (W:3481-3515), `var path = UiSupport.PromptSaveToLibrary(this, NameBox.Text, "sistema");` está en W:3498 y `if (path == null) return;` en W:3499. `session.Identity.EnsureId();` va en W:3503 y `session.Identity.SetName(NameBox.Text?.Trim());` en W:3504.
- **Con una ruta nula no se llega a `SetName`.** Por eso la fila de CT-PBT2B-04 usa `ArchivoTemporal`, y no la ruta nula que proponía IR1-R.
- El comparador compara el nombre con `string.Equals(x.Name, y.Name, StringComparison.Ordinal)` (PESC:429), así que `null` y `""` son distintos.

**El sufijo `descartes` (N-02):**
- **El patrón:** `PushBackDeclaredCorrection2MixedBoxTests.cs:94-96` arma la línea, hace `Cerrar(mixta);` dentro del `try` y añade `" descartes=" + descartes`. Las líneas esperadas acaban en ` descartes=0` (`:177-186`).
- **`Recompute`** (W:616-672): con una caja en error vuelve antes de tocar el estado (W:623-633, «do NOT touch state/session/computation/baseline»). Si no, `ReadInputs()` se evalúa dentro del `try` del armado (W:659-660), antes de cualquier excepción de validación.
- **`Restore_Click`** (W:3307-3330): con `lastComputation?.Design == null` se niega con «Todavía no hay un sistema válido que restaurar.» (W:3311). Si no, recarga y pone «Valores restaurados al último sistema válido.».
- **`ClickNamed`** eleva `ButtonBase.ClickEvent` con `RaiseEvent` (`EditorWindowTestSupport.cs:22-27`, `:110-111`), aunque el botón esté deshabilitado u oculto.
- **`CloseQuietly`** (T-CL:124-131) pone `probe.Answer = true` y solo cierra si `window.IsVisible`.
- **`StageModuleEdit`** (T-CL:90-98) cambia en 8 la longitud del primer módulo con longitud.

**Los fixtures de la pasada 2 (N-03):**

| Original | Línea | Fixture | Gesto | Lo que fija el original |
|---|---|---|---|---|
| CT-PB0-12 | `:38`, `:43` | `pb-31-v2-modulos` | `SelectModule(w, "M3")`, `HeaderConfiguratorDialog` con la copia y `PanelClear = 40.0`, más `ConfirmModuleButton`; en otra ventana, la costura devolviendo `null` (`:70-80`) | un cambio en la salida (`confirmar delta n=149`); con `null`, `pendiente=False salida-igual=True` |
| CT-PB0-13 | `:104`, `:110` | `pb-06-seguridad` | (i): `SafetyDialog` sin `DEFENSA_MONTACARGAS` | un cambio en la salida (`i delta=n=5`) |
| CT-PB0-15 | `:200`, `:205` | `pb-06-seguridad` | `DefenseDialog` con un poste | «reach the output» |
| CT-PB0-16 | `:237`, `:242` | `pb-05-topes` | `RearTopeDialog` con su `TopeResult` | «reach the output» |

`SelectModule` es de S0 (`PushBackFullEditorCharacterizationSupport.cs:347-352`): no hay que copiarlo.

**El aviso de D-PB-08 (para `descartes` en CT-PBT2C-02):** las doce líneas de CT-PBTC-07 están en `PushBackDeclaredCorrectionSideBNoticeTests.cs:89-100`. Las dos secuencias acaban con el estado distinto del inicial: las ranuras 1 y 2 de A en blanco en `pb-01-base`, y todas las de B en blanco en `pb-20-ab`.

**El `TaskId` (N-05):** `F4B-PBT2A-DRAFT-STATE-PORT` tiene 26 caracteres, `F4B-PBT2B-WORKSPACE-MODE-WINDOW` 31 y `F4B-PBT2C-WORKSPACE-CARRYOVER` **29**.
