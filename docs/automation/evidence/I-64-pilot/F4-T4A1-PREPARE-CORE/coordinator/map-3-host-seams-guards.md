# Mapa 3 — Costuras del host, de la UI y guardas que fijan T4 (I-64 F4-T4)

Medido sobre la rama `architecture/workspace-persistente-rackcad`, HEAD `7297e1f1` (código `4e34bb56`). Solo lectura: sin builds, sin pruebas,
sin AutoCAD. Rutas relativas al repositorio. «Code» = texto sin comentarios (`WorkspaceHostSource.Code`, `tests/RackCad.Tests/Workspace/WorkspaceHostSourceSupport.cs:34-39`);
«Text» = texto crudo con comentarios (`:41-46`). `Files(folder)` es recursivo (`:48-58`).

---

## 1. Costuras actuales de la intención de edición

### 1.1 `IWorkspaceEditIntentReader` — `src/RackCad.Application/Workspace/IWorkspaceEditIntentReader.cs`
- `:14` `public interface IWorkspaceEditIntentReader`
- `:20` `EditIntentReadResult ReadIntent(SessionId session, string rackId, long? chosenReference, EditIntentWitness? witness);`
- Contrato `:10-12`: documento activo, sin comando y editor en reposo, catálogo fuera del bloqueo, bloqueo, autorización (INV-22), revalidación con elección del origen, una lectura. «It writes nothing».
- Se asigna por `WorkspaceHostCore.SetEditIntentReader` (`WorkspaceHostCore.cs:272-273`), nunca por constructor; el controlador lo asigna en el getter `Navigation` (`src/RackCad.Plugin/Workspace/WorkspaceController.cs:105`, `_host.SetEditIntentReader(_navigation.EditIntentReader);`).

### 1.2 Tipos valor — `src/RackCad.Application/Workspace/WorkspaceEditIntent.cs`
| Tipo | Línea | Forma |
|---|---|---|
| `EditIntentWitness` | `:12` | `sealed record EditIntentWitness(string Key, string? Text)` |
| `EditIntentRequest` | `:18` | `sealed record EditIntentRequest(SessionId Session, string RackId, long? ChosenReference, EditIntentWitness? Witness)` |
| `EditIntentReadStatus` | `:21-28` | `Read, NoActiveDocument, Busy, Rejected, Failed` |
| `EditIntentReadResult` | `:34-63` | `readonly struct`; `PilotRead` (tipo del piloto `PushBackPilotIntentResult`) solo con `Read` |
| `EditWitnessStatus` | `:69-75` | `NotEvaluated, NotSupplied, Unchanged, Changed` |
| `EditIntentStatus` | `:77-125` | 7 de T2B2A (`Unavailable…Failed`) + `NotFound, NotNeeded, Reloaded` al final |
| `EditIntentOutcome` | `:131-193` | `readonly struct`, fábricas estáticas |
| `EditCheckStatus` | `:195-220` | `Unavailable, Waiting, Rejected, NotFound, NotApplicable, Conflict, Recovered, Failed` |
| `EditCheckOutcome` | `:226-269` | `Differences` como líneas `"key:kind"` (`:245`), nunca null |
| `EditDiscardStatus` / `EditDiscardOutcome` | `:275-288` / `:294-333` | `Rejected` es el default inerte; `Busy` al final |
| `WorkspaceEditIntent` | `:336-367` | `ChooseOriginDefinition` (`:344`), `WitnessStatusOf` (`:357`) |

### 1.3 `IWorkspacePilotEditPort` — `src/RackCad.Application/Workspace/IWorkspacePilotEditPort.cs`
Interfaz `:26-83`. Reglas del puerto en `:14-24` («none of the operations writes anything», «only the three operations of the edit intention read the drawing»).

| Operación | Línea | Firma | Lee el DWG |
|---|---|---|---|
| `TryBeginEdit` | `:29` | `EditIntentOutcome TryBeginEdit(EditIntentRequest request);` | sí (1 lectura) |
| `TryCheckNow` | `:32` | `EditCheckOutcome TryCheckNow(EditIntentRequest request);` | sí, solo si Unknown/CommitThrew |
| `TryReload` | `:35` | `EditIntentOutcome TryReload(EditIntentRequest request);` | sí, solo si `NeedsLazyReload` |
| `GetField` | `:38` | `PilotFieldView GetField(WorkspaceRequest origin, string rackId);` | no |
| `SetPendingText` | `:44` | `PilotFieldResult SetPendingText(WorkspaceRequest origin, string rackId, string text);` | no |
| `CommitField` | `:51` | `PilotFieldResult CommitField(WorkspaceRequest origin, string rackId);` | no |
| `CrossBoundary` | `:58` | `PilotBoundaryResult CrossBoundary(WorkspaceRequest origin, string rackId, PilotBoundaryKind kind);` | no |
| `DiscardRack` | `:61` | `EditDiscardOutcome DiscardRack(WorkspaceRequest origin, string rackId);` | no |
| `DiscardDocument` | `:64` | `EditDiscardOutcome DiscardDocument(WorkspaceRequest origin);` | no |
| `GetRacksAtRisk` | `:70` | `PilotRiskView GetRacksAtRisk(WorkspaceRequest origin);` | no |
| `TakeLossNotices` | `:76` | `IReadOnlyList<PilotLossView> TakeLossNotices();` | no |
| `TakeCloseNotices` | `:82` | `IReadOnlyList<PilotCloseView> TakeCloseNotices();` | no |

Tipos del puerto: `PilotFieldState` `:86-107` (`NotOpen, Clean, Dirty, Conflict, Applying, Unknown, Error, Undecidable`); `PilotFieldStatus` `:110-125` (`Rejected` primero);
`PilotBoundaryDecision` `:128-145`; `PilotBoundaryKind` `:148-156` (`TabChange, RackChange, DocumentChange, PanelHide, Dematerialize, DocumentClose`, «ordered prefix: later tasks may add values at the end»);
`PilotFieldProblem` `:159`; `PilotFieldView` `:165-218` (un constructor de 10 parámetros, último `string? errorReason = null` `:177`); `PilotFieldResult` `:221-250`;
`PilotBoundaryResult` `:258-288` (aviso: su default es una respuesta real `:254-257`); `PilotRiskView` `:295-318`; `PilotLossView` `:321-350`; `PilotCloseView` `:356-372`.

Implementadores del puerto (todos tendrían que cambiar si el puerto gana un miembro): `WorkspaceHostCore` (`WorkspaceHostCore.cs:38`), `WorkspacePilotEditGate` (`WorkspacePilotSupport.cs:153`),
`PilotEditAdapter` (`WorkspaceController.cs:278`), `CommandAdvisorPortDouble` (`tests/RackCad.Tests/Workspace/WorkspaceCommandAdvisorTests.cs:17`),
`ForwardingRiskPort` (`…WorkspaceCommandAdvisorTests.cs:1104`), `WorkspacePilotEditPortDouble` (`tests/RackCad.UI.Tests/Workspace/WorkspacePilotEditPortDouble.cs:25`).

### 1.4 El host core — `src/RackCad.Application/Workspace/WorkspaceHostCore.cs`
- `:38` `public sealed class WorkspaceHostCore : IWorkspaceLifecycleAdvisor, IWorkspacePilotEditPort`.
- `:42` `private static readonly PushBackPilotFieldAdapter PilotFieldAdapter = new();` · `:44-46` las tres constantes de motivo · `:70` `private bool _pilotReadInProgress;` · `:71` `private IWorkspaceEditIntentReader? _editIntentReader;`.
- Operaciones públicas: `TryBeginPilotEdit` `:280-358` (lectura única `:311`; `_drafts.Begin(…pilot.Read…)` `:330-335`); `TryCheckPilotEdit` `:365-428` (solo `DraftStatus.Unknown && UnknownOrigin == CommitThrew` `:388-389`, lectura `:392`, `draft.CheckNow(pilot.StaleBase)` `:410`); `TryReloadPilotEdit` `:438-512` (`NeedsLazyReload` `:467`, lectura `:470`, `draft.DiscardAndReload(pilot.Read)` `:489`, `NotAllowed` → `Waiting("El borrador se está aplicando o ya no está registrado.")` `:501-502`); `DiscardPilotEdits` `:520-536`; `NotePilotEdit` `:543`.
- Implementación explícita del puerto `:553-843`. Las tres lecturas con la marca y `try/finally`: `TryBeginEdit` `:553-565`, `TryCheckNow` `:567-579`, `TryReload` `:581-593` (`if (_pilotReadInProgress) return …Waiting(PilotReadInProgressReason); _pilotReadInProgress = true; try { … } finally { _pilotReadInProgress = false; }`). Memoria: `GetField` `:596-608`. Mutantes (autoriza y mira la marca): `SetPendingText` `:611-635`, `CommitField` `:638-669`, `CrossBoundary` `:671-719`, `DiscardRack` `:722-744`, `DiscardDocument` `:746-762`. `GetRacksAtRisk` `:765-807`, `TakeLossNotices` `:809-827`, `TakeCloseNotices` `:829-843`.
- Puerta común `PassGate` `:1034-1068`, en orden: lector asignado (`:1037`), documento activo (`:1043`), modal RackCad (`:1049`), `_probe.IsActiveDocumentQuiescent` (`:1055`, «Hay un comando en curso.»), `Authorize` (`:1061`). No lee nada del DWG.
- Vista del campo `ViewOfPilotDraft` `:861-911`: `canEdit` solo en Clean/Dirty/Conflict (`:890-891`); `IsPossiblyStale` (`:910`); `ErrorReason` solo en Error (`:909`). `StateOfDraft` `:913-924` (`Applying` `:920`).
- **Qué lee y con qué bloqueo:** el core no toca el DWG; delega en `_editIntentReader.ReadIntent` (solo en `:311`, `:392`, `:470`).

### 1.5 La compuerta de avisos — `src/RackCad.Application/Workspace/WorkspacePilotSupport.cs`
- `WorkspaceNoticeDepth` `:131-146`; `WorkspacePilotEditGate : IWorkspacePilotEditPort` `:153-195`; `InsideNoticeReason = "Se está entregando un aviso del panel."` `:156`.
- Las tres lecturas: `_depth.IsInside ? …Waiting(InsideNoticeReason) : _inner.X(request)` (`:167-174`); las otras nueve son delegación literal (`:176-194`).
- Cableado: `_pilotEdit = new PilotEditAdapter(new WorkspacePilotEditGate(_host, _noticeDepth));` (`WorkspaceController.cs:51`); `NotifyStateChanged` entra y sale de la profundidad con `try/finally` (`:250-265`).

### 1.6 El lector del Plugin — `src/RackCad.Plugin/Workspace/WorkspaceNavigationHost.cs`
- `:27` `public sealed class WorkspaceNavigationHost : IWorkspaceNavigationPort, IWorkspaceBrowserSummaryPort`; `:57` `_editIntentReader = new PilotIntentReader(_targets, _authorize);`; `:64` `internal IWorkspaceEditIntentReader EditIntentReader => _editIntentReader;`.
- Clase anidada `PilotIntentReader : IWorkspaceEditIntentReader` `:196-254`; `ReadIntent` `:207-218` (contiene toda excepción como `Failed`); `ReadOnActiveDocument` `:220-253`, en este orden:
  1. `MdiActiveDocument` (`:222`) → `NoActiveDocument("No hay documento activo.")`;
  2. `!string.IsNullOrEmpty(document.CommandInProgress) || !document.Editor.IsQuiescent` (`:225-226`) → `Busy("Hay un comando en curso.")`;
  3. `RackPilotIntentAdapter.LoadCatalog()` **fuera** del bloqueo (`:228-230`);
  4. `using (document.LockDocument())` (`:232`);
  5. `_authorize(new WorkspaceRequest(session), document.UnmanagedObject.ToInt64())` (`:234-236`);
  6. `_targets.Revalidate(session, rackId, expected)` + `WorkspaceEditIntent.ChooseOriginDefinition(revalidation.Fresh, chosenReference)` (`:238-240`) — NavigationTargets solo elige el origen;
  7. `RackPilotIntentAdapter.Read(document, rackId, origin, witness?.Key, witness?.Text, catalog.Catalog)` (`:242-251`).
- Ejecuta en contexto de aplicación, síncrono, sin `ExecuteInCommandContextAsync` (`:16-18`, D-08/IC-01).

### 1.7 El adaptador del dibujo — `src/RackCad.Plugin/Views/RackPilotIntentAdapter.cs`
- `:69` `internal static class RackPilotIntentAdapter` («It reads and writes nothing else», `:62-68`).
- `LoadCatalog` `:74-87` (`LateralHeaderDrawService.LoadCatalog()`).
- `Read` `:94-142`: `AssertNoOpenTransaction(document, "before")` (`:104`) → `RackSiblingScan.CaptureDrawing(document)` (`:116`) → `new PushBackPilotIntentScan(...)` (`:120`) → `PushBackPilotIntentReader.Read(input, catalog)` (`:121`); `finally` con `"after"` (`:140`). **No toma bloqueo propio**: corre dentro del de `ReadOnActiveDocument`.
- `ReadPreviewWitness` `:149-189`: bloqueo y transacción propios (`:160-161`), `RackBlockData.Read` (`:165`), sin `Commit`. **Sin llamador en producto** (solo `WorkspaceEditIntentGuardTests`).
- `AssertNoOpenTransaction` `[Conditional("DEBUG")]` `:195-201`: `Debug.Assert(document.Database.TransactionManager.TopTransaction == null, …)`.
- El barrido real: `src/RackCad.Plugin/Views/RackSiblingScan.cs:95-96` `CaptureDrawing` → `Traverse` `:98-170` con **su propio** `using (document.LockDocument())` y `using (var transaction = …StartTransaction())` (`:112-113`), lecturas `OpenMode.ForRead`, `RackBlockData.Read` (`:121`) y `transaction.Commit();` (`:163`) de la transacción de solo lectura.
- Pertenencia: la autoridad es I-55, `RackSiblingMembership.Classify(scan.Facts, scan.RackId, null, false)` en `src/RackCad.Application/Systems/PushBack/Pilot/PushBackPilotIntentReader.cs:119`; la StaleBase sale de `Analyze` (`BuildStaleBase` `:64`, `Read` `:75`).

### 1.8 «Comprobar ahora» (`TryCheckNow`) — ¿tiene llamador en la UI?
**No.** En `src/` solo aparece en la interfaz (`IWorkspacePilotEditPort.cs:32`), el host (`WorkspaceHostCore.cs:567-579`), la compuerta (`WorkspacePilotSupport.cs:170-171`) y el adaptador del controlador (`WorkspaceController.cs:289`).
Ningún archivo de `src/RackCad.UI/` lo nombra, y la XAML no tiene «Comprobar ahora» ni «Actualizar». Además, en el producto nada lleva hoy un borrador a `Unknown`: `TryBeginApply`/`CompleteApply` no tienen llamador en `src/` (solo en pruebas, p. ej. `tests/RackCad.Tests/Workspace/WorkspaceEditIntentHostTests.cs:101-111`). Un botón que llamara hoy a `TryCheckNow` respondería siempre `NotApplicable` (`WorkspaceHostCore.cs:388-389`).

---

## 2. Dónde encajarían «PrepareUpdate» (solo lectura) y «Actualizar»

### 2.1 Puerto, host, compuerta y controlador
- **Puerto:** nueva operación al final de la interfaz (`IWorkspacePilotEditPort.cs:82`). La guarda del puerto ya admite «una operación de Update y `PilotBoundaryKind.Update` al final»: `tests/RackCad.Tests/Workspace/WorkspacePilotEditGuardTests.cs:142-151` inyecta `PilotFieldResult Update(WorkspaceRequest origin, string rackId);` y `Update,` tras `DocumentClose,`.
- **`PilotBoundaryKind.Update`**: el núcleo ya tiene `DraftBoundary.Update` bloqueante (`src/RackCad.Application/Workspace/Drafts/DraftSession.cs:80`, `IsBlocking` `:90-91`); el host solo traduce `TabChange`/`RackChange` (`WorkspaceHostCore.cs:678-680`, `:700`). Es la frontera natural del compromiso del texto pendiente antes de PREPARE (`TryBeginApply` rechaza texto pendiente: `DraftSession.cs:473`).
- **Host:** una operación pública nueva al estilo de `TryCheckPilotEdit` (pasos 1-8 con `PassGate`) más su implementación explícita envuelta en la marca de lectura (patrón `:553-593`). La guarda de la marca acepta otra lectura con su propia marca (`WorkspacePilotEditGuardTests.cs:400-407`).
- **Compuerta:** un miembro nuevo con `_depth.IsInside ? …Waiting(InsideNoticeReason) : _inner.X(request)`; la guarda solo recorre las 12 operaciones (`WorkspacePilotEditGuardTests.cs:418-435`), así que la puerta de avisos de la nueva operación no la fija nadie hasta que se declare.
- **Controlador:** `PilotEditAdapter` (`WorkspaceController.cs:278-312`) tiene que reenviarla literal (`=> _gate.X(…)`, guarda `WorkspacePilotEditGuardTests.cs:490-500`).

### 2.2 Lector del Plugin
- PREPARE (T4A) puede reutilizar tal cual `IWorkspaceEditIntentReader.ReadIntent` → `PilotIntentReader.ReadOnActiveDocument` (contexto de aplicación, un `using LockDocument`): es la «relectura autoritativa» ya existente. El resultado trae `StaleBase` y `Read` (`src/RackCad.Application/Systems/PushBack/Pilot/PushBackPilotIntentResult.cs:44-47`).
- La composición del estado a escribir existe y es pura: `PushBackPilotMaterializer.ComposeForWrite(initial, current, catalog)` (`src/RackCad.Application/Systems/PushBack/Pilot/PushBackPilotMaterializer.cs:189-215`): rechaza todo cambio que no sea `Inputs.Annotations.AnnotationScale` (`:198-201`) y rematerializa desde `initial.Origin` (base vigente) (`:203-209`). **Sin llamador en producto.** Necesita `RackCatalog`, que solo carga el Plugin (`RackPilotIntentAdapter.cs:74-87`).
- MUTATE/POST (T4B) no caben en el lector actual: ver §3 y §7.

### 2.3 VM del campo — `src/RackCad.UI/Workspace/WorkspacePilotFieldViewModel.cs`
- `:19` `public sealed class WorkspacePilotFieldViewModel : ObservableObject`; constructor único `:49` `(IWorkspacePilotEditPort port, Action changed = null)`.
- Visibilidad de botones derivada en `Apply` `:351-365`: `_showEdit` (`:357`), `_showReload` = fallo o `State == Error` (`:358`), `_showDiscard = _showReload && view.Exists` (`:359`). Propiedades `ShowEditButton` `:106`, `CanOpenByGesture` `:113`, `ShowReloadButton` `:116`, `ShowDiscardButton` `:119`; `RaiseAll` `:418-431`.
- Gestos: `Text` (setter, única entrada de `SetPendingText`) `:64-98`; `BeginEdit` `:191-239` (Clean + posiblemente obsoleto → `TryReload`, si no `TryBeginEdit`, `:201-203`); `CommitPending` `:242-277`; `Rematerialize` `:280-294` (sin lectura).
- Estado visible: `StateOf` `:367-380` (`" (posiblemente obsoleto)"` `:377`); `NameOf` `:389-402` (`"Aplicando"` `:396`, `"Desconocido"` `:397`).
- Un «Comprobar ahora» o un «Actualizar» de la superficie encaja aquí como gesto nuevo (`void CheckNow()` / `void Update()`), con un `ShowXButton` nuevo calculado en `Apply`.

### 2.4 VM del navegador — `src/RackCad.UI/Workspace/WorkspaceBrowserViewModel.cs`
- Constructor `:130` `(IWorkspaceBrowserSummaryPort port, Action tabChanged = null, IWorkspacePilotEditPort pilotEdit = null)`; `Field = new WorkspacePilotFieldViewModel(pilotEdit, OnFieldChanged);` `:135`; `public WorkspacePilotFieldViewModel Field { get; }` `:141`.
- Gestos del campo: `BeginFieldEdit` `:555-571` (fija la preview y llama `Field.BeginEdit()`), `BeginFieldEditFromBox` `:577-581`, `BeginFieldEditFromTab` `:587-593`, `ReloadField` `:596` (`=> Field.Rematerialize();`), `DiscardField` `:313`; `OnFieldChanged` `:817`.
- **Forma pública fijada: 33 firmas** (`tests/RackCad.Tests/Workspace/WorkspacePilotCloseSyncGuardTests.cs:379-414`, `CheckPublicShape` `:438-446`: `Assert.Equal(33, PublicShape.Length); Assert.Equal(33, PublicShape.Distinct().Count()); Assert.Equal(33, shape.Count);`). Un `public void UpdateField()` / `CheckField()` nuevo en el navegador cambia el censo (declarado). La alternativa sin tocar el censo es el patrón ya usado por la vista: el code-behind llama directamente al VM del campo (`field.CommitPending()` en `WorkspacePanelView.xaml.cs:130` y `:158`).

### 2.5 Vista — `src/RackCad.UI/Workspace/WorkspacePanelView.xaml` y `.xaml.cs`
- Panel del campo `BrowserFieldPanel` `:193-231`; caja `BrowserFieldBox` `:197-203`; `WrapPanel` de botones `:214-230`:
  - `BrowserFieldEditButton` `Content="Editar"` `:215-219` → `OnBrowserFieldEditClick` → `_viewModel.Browser.BeginFieldEdit()` (`.xaml.cs:83`);
  - `BrowserFieldReloadButton` `Content="Recargar"` `:220-224` → `OnBrowserFieldReloadClick` → `_viewModel.Browser.ReloadField()` (`.xaml.cs:85`);
  - `BrowserFieldDiscardButton` `Content="Descartar"` `:225-229` → `OnBrowserFieldDiscardClick` → `_viewModel.Browser.DiscardField()` (`.xaml.cs:89`).
- Botón propio de la superficie `BrowserSurfaceReloadButton` `Content="Volver a leer"` `:234-237` → `ReloadSummary()` (`.xaml.cs:75`).
- Casilla de solo lectura «Escribir en el dibujo» ligada a `CanWriteDrawing` (`.xaml:61-62`; `WorkspacePanelViewModel.cs:106`, `:220`), que hoy refleja `WorkspaceHostPolicy.Actions` (`src/RackCad.Application/Workspace/WorkspaceHostPolicy.cs:36-41`).
- Toda interacción pasa por `Contained` (`.xaml.cs:185-195`).
- Encaje: `Actualizar` y `Comprobar ahora` como botones nuevos en `:214-230`, con `Visibility` ligada a `Browser.Field.ShowXButton` y manejador `Contained(() => …)`. Restricciones de XAML en §3.6.

---

## 3. Guardas que hoy prohíben lo que T4 necesitará

### 3.1 `ExecuteInCommandContextAsync`
- `tests/RackCad.Tests/Workspace/WorkspaceNavigationHostTests.cs:436` — **todo `src`**: `Assert.Empty(Src.AllProduct().Where(f => Regex.IsMatch(f.Code, @"ExecuteInCommandContextAsync")).Select(f => f.Path));` (H-13 del paquete F4).
- `tests/RackCad.Tests/Workspace/WorkspaceHostSourceGuardTests.cs:454` — navegación: `Assert.DoesNotMatch(new Regex(@"ExecuteInCommandContextAsync|SendStringToExecute|\[CommandMethod|Autodesk\.AutoCAD\.Internal|DllImport|HwndSource"), host);`
- `tests/RackCad.Tests/Workspace/WorkspaceSourceGuardExceptionTests.cs:269` — `Assert.DoesNotContain("ExecuteInCommandContextAsync", hostCode);` (host de navegación).
- `tests/RackCad.Tests/Workspace/WorkspaceBrowserHostGuardTests.cs:259-262` — UI/Workspace + `WorkspaceNavigationHost.cs` + `WorkspaceNavigationFacts.cs`: `new Regex(@"SendStringToExecute|CommandMethod|ExecuteInCommandContextAsync")`.
- UI: `WorkspacePilotSurfaceGuardTests.cs:280` (`drawing`) y `WorkspacePilotCloseSyncGuardTests.cs:851` (`DrawingReach`), ambos con `ExecuteInCommandContextAsync`.
- Asincronía asociada (la API devuelve `Task`):
  - `WorkspaceHostSourceGuardTests.cs:426` (Plugin/Workspace + Application/Workspace): `@"Task\.Run|…|\basync\b|\bawait\b|…"`; `:452-453` sin `async|await` en las dos carpetas;
  - `WorkspaceNavigationHostTests.cs:435` y `WorkspaceSourceGuardExceptionTests.cs:267-268` (lo mismo);
  - controlador sin `Task`: `WorkspacePilotEditGuardTests.cs:510` (`\bTask\b|\basync\b`) y `WorkspaceCommandAdvisorGuardTests.cs:238`;
  - UI sin `Task|async|await|Dispatcher` salvo `WorkspaceNavigationViewModel.cs`: `WorkspacePilotSurfaceGuardTests.cs:283-289`, `WorkspacePilotCloseSyncGuardTests.cs:475` y `:496-502`, `WorkspaceBrowserHostGuardTests.cs:271-273`;
  - host core sin `Task|async|await` incluso en código: `WorkspaceEditIntentGuardTests.cs:833`;
  - compuerta sin `Task|async`: `WorkspacePilotEditGuardTests.cs:437`.

### 3.2 `LockDocument`, `StartTransaction`, `Commit(`, `OpenMode.ForWrite`, `UpgradeOpen`, `RackBlockData.Write`, `SendStringToExecute`
| Guarda | Ámbito | Aserción literal |
|---|---|---|
| `WorkspaceModelBoundaryTests.cs:41-49` | **todo** Application/Workspace (recursivo) | `@"…Transaction|LockDocument|\bDatabase\b|\bCommit\(|\bAutodesk\."` |
| `WorkspaceLifecycleGuardTests.cs:643-658` | advisor, `EventBridgeCore`, host core, `WorkspaceEditIntent`, `DraftRegistry`, `AutoCadEventBridge`, `WorkspaceController` | `WriteApis` = `OpenMode\.(?:ForWrite|ForNotify)|UpgradeOpen|DowngradeOpen|AppendEntity|\.Erase\s*\(|\.Commit\s*\(|RackBlockData\.Write|SetImpliedSelection|SetCurrentView|StartTransaction|StartOpenCloseTransaction|LockDocument|SetSystemVariable|SendStringToExecute|\bSaveAs\b|\bSave\s*\(` |
| `WorkspacePilotEditGuardTests.cs:67-69`, `:664-695`, `:697-701` | cuerpos de las 12 operaciones del host, de la compuerta y de 3 helpers; archivo de soporte entero (`:694`) | mismo `WriteApis`; soporte: `OpenMode\.(?:ForWrite|ForNotify)|UpgradeOpen|AppendEntity|StartTransaction|LockDocument|SetSystemVariable|SendStringToExecute` |
| `WorkspacePilotEditGuardTests.cs:756`, `:760-774`, `:782-788` | puerto, soporte, host (código) y puerto/soporte/host/intent (**texto, comentarios incluidos**) | `@"Transaction|\bDatabase\b|LockDocument|\bCommit\s*\(|RackLog"`; texto: `@"Transaction|\bDatabase\b|\bCommit\s*\("` |
| `WorkspaceEditIntentGuardTests.cs:821-839` | host core, `IWorkspaceEditIntentReader.cs`, `WorkspaceEditIntent.cs` (texto) | `Assert.DoesNotMatch(new Regex(@"Transaction|\bDatabase\b|\bCommit\s*\("), text);` |
| `WorkspaceHostHardeningGuardTests.cs:440-442` | literales del host y del soporte | `Transaction|Database|Commit\(|RackLog|…|LockDocument|…` |
| `WorkspaceBridgeSourceGuardTests.cs:204-208`, `:232-239` | Plugin/Workspace | toda línea con `StartTransaction|StartOpenCloseTransaction|LockDocument` lleva `using` |
| `WorkspaceHostSourceGuardTests.cs:466-477` | Plugin/Workspace | lo mismo + ningún campo de `Transaction|DBObject|Document|Database|Entity|BlockReference|ObjectId|Editor` |
| `WorkspaceBridgeSourceGuardTests.cs:295-307` | **todo** Plugin/Workspace | `PersistenceOrAuthoredWrite`: `…NamedObjectsDictionary|…|\bDBDictionary\b|\bXrecord\b|\bXData\b|\bXdata\b|\bResultBuffer\b|AppendEntity|UpgradeOpen|DowngradeOpen|\.Commit\(|\.Erase\(|SetXDictionary|SaveAs|\bSave\(|DocumentExtension` |
| `WorkspaceHostSourceGuardTests.cs:373-378` | Plugin/Workspace + Application/Workspace | `Guid\.NewGuid|new\s+RackEmbedDocument|RackEmbedComposer|RackBlockData\.Write` |
| `WorkspaceHostSourceGuardTests.cs:382-396` | Plugin/Workspace + Application/Workspace | `\b(?:SetImpliedSelection|SetCurrentView|SetSystemVariable|Zoom\w*|Regen|UpdateScreen|Highlight|Unhighlight|SendStringToExecute|SetImpliedSelectionAsync|PanTo)\b` (Effects por archivo exacto) |
| `WorkspaceHostSourceGuardTests.cs:457-464` | Plugin/Workspace + Application/Workspace | `…\bXrecord\b|\bXData\b|\bDBDictionary\b|NamedObjectsDictionary|AtomicFile|…` |
| `WorkspaceBridgeSourceGuardTests.cs:139-180` | **todo** Plugin/Workspace (refinamientos por archivo exacto) | `HandlerForbiddenCalls`: `…|UpdateScreen|Regen|SendStringToExecute|StartCommand|GetSelection|…|GetKeywords|…|GetObject|OpenCloseTransaction|…` |
| `WorkspaceNavigationHostTests.cs:440-449` | host, facts, effects, controlador, reader | `…|SendStringToExecute|\[CommandMethod|StartCommand|…` |
| `WorkspaceDraftBoundaryTests.cs:108` | Drafts | `…Transaction|LockDocument|Database|\bCommit\s*\(|\bAutodesk\.|RackLog|…` |
| `WorkspaceDraftPilotIntentGuardTests.cs:28-47` | `PushBackPilotIntentScan/Reader/Result.cs` (texto) | `Transaction`, `LockDocument`, `\bTask\b`, `async`, `await`, … |

### 3.3 Adaptador `RackPilotIntentAdapter` y barrido (`tests/RackCad.Tests/Workspace/WorkspaceEditIntentGuardTests.cs`)
- Escrituras `:282-294`: `AdapterWrites = …OpenMode\.(?:ForWrite|ForNotify)|\bForWrite\b|UpgradeOpen|DowngradeOpen|AppendEntity|\.Erase\s*\(|\.Commit\s*\(|RackBlockData\.Write|SetImpliedSelection|SetCurrentView|…|\bRegen\b|UpdateScreen|…`; `Assert.Equal(1, Src.Count(code, @"RackBlockData\.Read\s*\("));` `Assert.Equal(0, Src.Count(code, @"GetObject\s*\("));`.
- Bloqueo/transacción: `Read` sin `LockDocument|StartTransaction` (`:192`); el testigo, exactamente un `using (document.LockDocument())` y un `using (var … StartTransaction())` (`:194-196`).
- Una sola captura: `Assert.Equal(1, Src.Count(code, @"\bCaptureDrawing\s*\("));` (`:320-321`).
- Barrido: `Assert.Equal(1, Src.Count(scan, @"\.Commit\s*\("));` y sin `ForWrite|UpgradeOpen|AppendEntity|RackBlockData\.Write` (`:113-121`).
- No conoce el Workspace: `Assert.DoesNotMatch(new Regex(@"RackCad\.Application\.Workspace"), code);` (`:371-380`); y ningún archivo del Plugin fuera de Plugin/Workspace puede nombrar `RackCad.Plugin.Workspace|RackCad.Application.Workspace|AutoCadEventBridge` (`WorkspaceBridgeSourceGuardTests.cs:361-366`).
- Un solo adaptador con ese nombre en Views (`:816`).

### 3.4 Lector de la intención y host de navegación (`WorkspaceEditIntentGuardTests.cs`)
- Orden de los 7 pasos y `using (document.LockDocument())` (`:398-415`); una sola llamada en el host: `LoadCatalog` (`:422`), `RackPilotIntentAdapter\.Read\s*\(` (`:423`), `_targets\.Revalidate\s*\(` (`:424`); el cuerpo del lector no nombra `_effects|_runner|…|GetObject|\.Commit\s*\(|OpenMode|ForWrite` (`:426`).
- `Assert.Equal(2, Src.Count(host, @"\bclass\s+\w+"));` — host y lector anidado son las dos únicas clases (`:436`).
- El host no nombra el piloto: `Assert.DoesNotMatch(new Regex(@"PushBackPilot|RackCad\.Application\.Systems"), host);` y exactamente un `read.Pilot` (`:458-460`); no nombra `TryBeginPilotEdit|TryCheckPilotEdit|WorkspaceHostCore` (`:464`); el Plugin no abre el resultado: `\.StaleBase\b|\.Read\.|\bInitialState\b` (`:694-697`).
- Sin `+=`, `-=`, `.Result`, `async`, `Task.Run`; exactamente un `RackLog.Exception(` (`:472-474`).
- `.ReadIntent(` solo en el host core (`:614-616`); `SetEditIntentReader` solo en core y controlador (`:491-495`).
- **Uso de la base:** `BaseOrReadUse = \.StaleBase\b|\.InitialState\b|\b(?:pilot|result)\.Read\b|\.Entries\b` solo como argumento de `.Begin(…)`, `.DiscardAndReload(…)` o `.CheckNow(…)` (`:635-663`). Una comparación de PREPARE como `draft.StaleBase.CompareWith(pilot.StaleBase)` dispara esta guarda.
- Cuerpos de intención y comprobación sin operaciones del ciclo de vida (`:776-786`).
- **Censo exacto de Plugin/Workspace:** `Assert.Equal(9, plugin.Length);` y ningún nombre con `Intent` (`:807-810`).
- Interfaz del lector: exactamente un `ReadIntent` con su firma y sin `Draft|Navigation|PushBackPilot|Document|Autodesk` (`:841-849`).
- Formas exactas del host (`WorkspaceEditIntentHostTests.cs`): `EditIntentReadStatus` exacto (`:1017`), prefijo de `EditIntentStatus` (`:1020`), **`EditCheckStatus` exacto** (`:1022`); ningún campo de instancia del host core de un tipo de `RackCad.Application.Systems` ni `EditIntentReadResult` (`:990-1002`); los portadores de la intención sin tipos de Drafts ni de Systems (`:969-986`).

### 3.5 Puerto, compuerta y controlador (`WorkspacePilotEditGuardTests.cs`)
- Puerto: las 12 operaciones como cota inferior (`:96-97`), sin nombres de Drafts ni del piloto (`:73`, `:99`), enumeraciones con prefijo ordenado (`:75-80`) (`:90-123`).
- **Tipos valor del puerto** (`:153-157`, `:171-217`): `PilotFieldView, PilotFieldResult, PilotBoundaryResult, PilotRiskView, PilotLossView, PilotCloseView, PilotFieldProblem` son `IsValueType`, sin tipos anidados, propiedades de solo lectura y campos `IsInitOnly`; ningún tipo de `…Workspace.Drafts` ni de `…Systems` en métodos, propiedades ni constructores.
- `PilotFieldView` (`WorkspaceHostHardeningGuardTests.cs:363-369`): `string\?\s+errorReason\s*=\s*null\s*\)\s*\{` como último parámetro, `public string? ErrorReason { get; }` y **exactamente un constructor**.
- Host (`:221-298`): implementación solo explícita, sin la operación pública (`:228-232`, `:296`); el adaptador del piloto es el único campo estático (`:235-236`); solo las 3 lecturas nombran `_editIntentReader|ReadIntent|_reader|PassGate|_probe|_bridge` (`:239-244`); las mutantes autorizan y miran la marca (`:254-260`); `NotePilotEdit` en las 4 que cambian el borrador (`:263-267`); interfaces exactamente `{ IWorkspaceLifecycleAdvisor, IWorkspacePilotEditPort }` (`:297`).
- Marca de lectura (`:357-375`): `Src.Count(host, @"private\s+bool\s+_pilotReadInProgress\s*;") != 1` y al menos 3 `= true`.
- Compuerta (`:411-439`): las 3 lecturas esperan dentro de un aviso; el resto, delegación pura `=>_inner.X(…);`; sin `Dispatcher|Timer|Task|async|event|+=|static`.
- Controlador (`:463-511`): `_pilotEdit = new PilotEditAdapter(new WorkspacePilotEditGate(_host, _noticeDepth));` (`:487`); el adaptador solo delega (`:494-499`); exactamente un `public IWorkspacePilotEditPort` (`:504`); **exactamente 3 `+=`**, ningún `-=`, **un `event`** (`:507-509`); sin `Dispatcher|Timer|Task|async` (`:510`).
- Diálogos solo dentro de `EnterPanelModal` en las 3 carpetas Workspace (`:631-646`). `[CommandMethod` prohibido en controlador, soporte y puerto (`:898`).
- Nombres de tipo en `RackCad.Application.Workspace`: sin `Subscription|Editor|Draft`, ni `Tab` salvo dos excepciones (`WorkspaceModelBoundaryTests.cs:93-105`); trampas de nombre `Draft|Editor|Subscription|Tab` en puerto y soporte (`WorkspacePilotEditGuardTests.cs:747-754`).

### 3.6 UI (lecturas, forma y XAML)
- El navegador **no nombra** `TryBeginEdit|TryReload|TryCheckNow` (`WorkspacePilotCloseSyncGuardTests.cs:849`, `:857-866`).
- `TryBeginEdit|TryReload` solo en el VM del campo (`WorkspacePilotSurfaceGuardTests.cs:94`); `TryCheckNow` está prohibido en los cuerpos de los gestos de T3B1 (`BeginEdit|CommitPending|Rematerialize|BeginFieldEdit|BeginFieldEditFromBox|BeginFieldEditFromTab|ReloadField`, `:56-61`, `:90-92`). **Explícitamente libre** en un gesto nuevo (`:134-141`: `public void CheckNow() { var o = _port.TryCheckNow(r); }` pasa).
- Censo de 33 firmas del navegador (`WorkspacePilotCloseSyncGuardTests.cs:438-452`); exactamente un `public bool ShowDiscardButton =>` en el VM del campo (`:333`).
- XAML: exactamente un `Content="Editar"`, `"Recargar"`, `"Volver a leer"` y `"Descartar"` en la superficie (`:728-741`); ningún `IsDefault=`/`IsCancel=` en toda la XAML (`:707`); sin tamaños fijos en la sección (`:705`); manejadores nuevos `=> Contained(() => _viewModel.Browser.X())` (`:781-795`, cota inferior).
- VM del campo síncrono, sin `+=|-=|event|delegate|static` mutable y sin `Autodesk|RackCad.Plugin|WorkspaceHostCore|WorkspaceController|IWorkspaceHostPort` (`WorkspacePilotSurfaceGuardTests.cs:326-335`).

### 3.7 «Cero escrituras authored» de F4-T3
- Por cuerpo, en el host y la compuerta: `WorkspacePilotEditGuardTests.cs:664-737` (`TheOperationsOfThePortAndTheDecoratorNameNoApiOfWritingTheDrawing`), con la autoprueba que **admite** una operación posterior de Update (`:724-737`) y la que admite un `IWorkspacePilotEditPort.Update(…)` con `StartTransaction`/`Commit` en el host (`:344-353`). Esa permisividad queda anulada por las guardas de texto del host (`:782-788`, `WorkspaceEditIntentGuardTests.cs:830`, `WorkspaceModelBoundaryTests.cs:41-49`, `WorkspaceLifecycleGuardTests.cs:647-658`): **el host core no puede contener la mutación**.
- T3C2: INV-F4T3C2-01 (paquete `docs/initiatives/I-64-f4-t3c2-gate-contract-package.md:514`); el núcleo del comando solo llama `GetRacksAtRisk` y `DiscardDocument`, y el relevo solo `GetRacksAtRisk`, con la lista negra `DiscardRack|GetField|SetPendingText|CommitField|CrossBoundary|TryBeginEdit|TryReload|TryCheckNow|TakeLossNotices|TakeCloseNotices` (`WorkspaceCommandAdvisorGuardTests.cs:412-441`).

### 3.8 Comando y registro de comandos
- Exactamente el par `RACKPANEL`/`RACKPANELDESCARTAR` y dos atributos (`WorkspaceCommandAdvisorGuardTests.cs:36-46`; `WorkspaceHostSourceGuardTests.cs:49-62`); ningún otro archivo de Plugin/Workspace declara comandos (`WorkspaceHostSourceGuardTests.cs:64-69`); `DeclaresEntryPoint` (`WorkspaceBridgeSourceGuardTests.cs:338-359`).
- `WriteMessage` solo en el archivo del comando (`WorkspaceHostSourceGuardTests.cs:90-95`).
- `IWorkspaceLifecycleAdvisor` con exactamente dos sentencias (`WorkspaceCommandAdvisorGuardTests.cs:384-392`, C2-M-4).

### 3.9 Pistas y drenaje
- `DrainOperation` es un conjunto **exacto** de cuatro (`tests/RackCad.Tests/Workspace/WorkspaceHintDrainTests.cs:92-113`); `HintKind` no es exacto, pero cada tipo debe producir operaciones del conjunto (`:104-112`).
- Orden del drenaje del host y forma literal del marcado (`WorkspaceLifecycleGuardTests.cs:523-539`: `if\s*\(\s*outcome\.Result\.Operations\.Contains\s*\(\s*DrainOperation\.MarkDraftsPossiblyStale\s*\)\s*\)\s*MarkPilotDraftsPossiblyStale`); cuerpo del marcado (`:541-550`); cuerpo literal de `NotePilotEdit` (`:514-521`).
- Un solo archivo suscriptor, los 16 eventos gobernados, cada uno `+=` y `-=` una vez (`WorkspaceBridgeSourceGuardTests.cs:79-84`, `:101-123`); cada manejador contenido (`:183-189`).

### 3.10 Literales en español
`WorkspaceHostHardeningGuardTests.cs:398-428`, `:444-463` y `:466-473`: 28 literales con **número exacto de apariciones** en host + soporte. Por ejemplo: `"La petición no es del documento activo."` 6, `"No hay borrador para el rack."` 3, `"El testigo de la vista previa no tiene clave."` 3, `"El dibujo no está listo para leerse."` 3, `"Hay un comando en curso."` 1, `"No hay documento activo."` 1. Hay cuatro identificadores que se conservan (`:431-437`). Una operación nueva que repita un literal en línea cambia la cuenta.

---

## 4. Estado Applying y ámbito de mutación propia

### 4.1 Applying y el bloqueo de F4-T1 (todo en Application, sin llamador de producto)
- `DraftStatus { Clean, Dirty, Conflict, Applying, Unknown, Error }` (`src/RackCad.Application/Workspace/Drafts/DraftSession.cs:58-66`); `DraftUnknownOrigin { CommitThrew, PostReread }` (`:69-73`).
- `TryBeginApply` (`:467-483`): solo desde Clean o Dirty sin texto pendiente; `_registry.TryAcquireApplySlot(this)` (`:474`) → `AnotherUpdateInProgress`; `_statusBeforeAttempt = previous` (`:478`); `_explicitStatus = DraftStatus.Applying` (`:479`).
- Ranura única del registro (`src/RackCad.Application/Workspace/Drafts/DraftRegistry.cs:354-367`): `TryAcquireApplySlot` / `ReleaseApplySlot`; «discarding or retiring never frees it» (`:363`); `HasUpdateInProgress` (`:198`).
- `CompleteApply` (`DraftSession.cs:490-513`): libera la ranura en todo camino, también con el borrador desregistrado (`ClosedDetached`, `:496-499`); aplica un `EnterError` diferido (`:503-512`). Clasificación D-13 en `Classify` (`:515-565`); `Applied` solo con `PostRead` admitido rebasa y deja Limpio (`:537-557`).
- Bordes: `DiscardAndReload` → `NotAllowed` en Applying (`:410-411`); `EnterError` en Applying solo difiere (`:606-610`); `CheckNow` solo para Unknown/CommitThrew (`:571-589`).
- Clasificaciones: `DraftApplyOutcomeKind` (`src/RackCad.Application/Workspace/Drafts/DraftApplyOutcome.cs:12-37`); fábricas `NotAppliedConflict…AppliedWithWarnings` (`:69-99`); `DraftApplyRefusal` (`:108-123`).
- En el host: `StateOfDraft` → `PilotFieldState.Applying` (`WorkspaceHostCore.cs:920`); Applying no es editable (`:890-891`); `TryReload` → `Waiting` con borrador Applying (`:501-502`). **No hay marca de «actualización en curso» en el host**: `DiscardRack`/`DiscardDocument` solo miran `_pilotReadInProgress` (`:727`, `:751`), y `DraftRegistry.DiscardByUser` desregistra un borrador Applying sin liberar la ranura (`DraftRegistry.cs:262-273`).

### 4.2 Pistas propias y C-F4-04
- Paquete F4: `docs/initiatives/I-64-f4-gate-contract-package.md:27` (H-08: «`MarkDraftsPossiblyStale` es solo un valor; las pistas no llevan rack ni handle; el drenaje ocurre en Idle») y `:60` (C-F4-04: «El ámbito se indexa por (sesión, miembros) y se cierra con el fin del **propio comando**, no tras POST; o la pista de objeto lleva el handle…»). Fila de T4B en `:48`.
- IC-08 (`docs/initiatives/I-64-implementation-clarifications.md:145-151`): «Las pistas que produce la mutación propia de Actualizar no marcan los borradores. El mecanismo lo fija T4B»; las pistas se agrupan por sesión y tipo y se drenan después del ámbito; «no puede suprimir `InvalidateNavigationCaches`»; oráculo «tras Actualizar A, el borrador B no queda marcado» (INV-28). Igual en `docs/initiatives/I-64-f4-t2b2-gate-contract-package.md:312`.
- Lo único «propio» que existe hoy es el eco de la **selección**: `src/RackCad.Application/Workspace/NavigationEcho.cs:16-97` (por contenido y por sesión), consultado en el drenaje (`WorkspaceHostCore.cs:208`). En `src/` no hay ningún mecanismo de mutación propia.

### 4.3 Cómo llegan los eventos y cómo se marca «posiblemente obsoleto»
1. Manejadores nativos en `src/RackCad.Plugin/Workspace/AutoCadEventBridge.cs`: `CommandEnded` `:198-204`, `CommandCancelled` `:205-211`, `CommandFailed` `:212-218`, `ObjectAppended/Erased/Modified/Unappended/Reappended` `:226-260` (p. ej. `ObjectModified` `:240-246`). Todos hacen `ContainSession(a, () => _core.OnSourceEvent(a.Session, a.Source))`: **no leen `e`**, así que la pista no lleva handle.
2. `EventBridgeCore.OnSourceEvent` (`src/RackCad.Application/Workspace/EventBridgeCore.cs:306-348`): `CommandEnded|CommandCancelled|CommandFailed` → `live.EnqueueHint(HintKind.CommandEnded)` (`:324-328`); `Object*` → `HintKind.DatabaseObjectChanged` (`:329-335`); `CommandWillStart`/`EnteringQuiescentState` solo cambian `Quiescent` (`:317-320`).
3. `WorkspaceSession` (`src/RackCad.Application/Workspace/WorkspaceSession.cs`): `_pending` es un `HashSet<HintKind>` (`:26`), N pistas = una bandera; `TryDrain` (`:40-59`) solo con reposo, sin modal y **panel visible** (`:42`); `DatabaseObjectChanged` o `CommandEnded` → `InvalidateNavigationCaches` + `MarkDraftsPossiblyStale` (`:52-56`).
4. Idle: `AutoCadEventBridge.OnIdleTick` (`:273-276`) → continuación `RunIdleContinuation` (`:280-284`) → `OnIdle` (`:304-317`) → `EventBridgeCore.OnIdle` (`:382-403`) → evento `Drained`.
5. `WorkspaceController.OnDrained` (`:221-245`) → `WorkspaceHostCore.OnDrained` (`:164-220`): `_knownClean.Remove(...)` (`:172`) y `if (… Contains(DrainOperation.MarkDraftsPossiblyStale)) MarkPilotDraftsPossiblyStale(outcome.Session);` (`:173`).
6. `MarkPilotDraftsPossiblyStale` (`:1015-1019`) → `DraftSession.MarkPossiblyStale()` para **todos** los borradores del piloto de la sesión (`DraftSession.cs:393-399`).
7. Efecto visible: `PilotFieldView.IsPossiblyStale` (`WorkspaceHostCore.cs:910`) → «(posiblemente obsoleto)» (`WorkspacePilotFieldViewModel.cs:377`); un borrador Limpio y marcado no es editable (`:349`) y su primer gesto hace `TryReload` (`:201-203`). Un Dirty marcado sigue editable y nada lo compara hasta una acción del usuario.

---

## 5. Los dos menores importantes de F4-T3C2 (C2-M-1 y C2-M-2)
Origen: `docs/automation/evidence/I-64-pilot/F4-T3C2-COMMAND-ADVISOR/1/R20261113T215110Z-28eb/independent-review.md:18-50`; disposición `docs/automation/evidence/I-64-pilot/F4-DRAFTS/f4-dispositions.md:1650` y `:1671` («C2-M-1 y C2-M-2 van primero»).

### 5.1 C2-M-1 — Sí/No (`GetKeywords`)
- Producto (correcto hoy): `src/RackCad.Plugin/Workspace/WorkspacePanelCommand.cs:58-70`, con `return answer.Status == PromptStatus.OK && string.Equals(answer.StringResult, "Si", System.StringComparison.Ordinal);` (`:69`).
- Guarda: `tests/RackCad.Tests/Workspace/WorkspaceCommandAdvisorGuardTests.cs:133-147`. Agujas sueltas (`:138-143`):
  `@"Keywords\.Add\s*\(\s*""Si""\s*\)", @"Keywords\.Add\s*\(\s*""No""\s*\)", @"Keywords\.Default\s*=\s*""No""\s*;", @"AllowNone\s*=\s*true\s*;", @"\.Status\b", @"\.StringResult\b", @"StringComparison\.Ordinal\b", @"PromptStatus\.OK\b"`.
  Además, un único `GetKeywords` (`:135-137`) y ningún identificador `Result` (`:146`). Autopruebas en `:155-164`.
- Hueco: ninguna aguja fija el `return`. Pasan `…StringResult, "No", …` y `answer.Status != PromptStatus.OK || …"Si"…`. Sugerencia de la revisión: `PromptStatus\.OK\s*&&\s*string\.Equals\s*\(\s*\w+\.StringResult\s*,\s*"Si"\s*,`, más autopruebas con `"No"` y con `||`.

### 5.2 C2-M-2 — `Authorize` de `RACKPANELDESCARTAR`
- Producto (correcto hoy): `WorkspacePanelCommand.cs:38-55`; delegado `session => controller.Port.Authorize(new WorkspaceRequest(session)) == RequestDecision.Accepted` (`:47`), puerto `controller?.PilotEdit` (`:45`), sesión `() => controller?.Port.GetSnapshot().ActiveSession` (`:46`).
- Guarda: `WorkspaceCommandAdvisorGuardTests.cs:101-111`, con `if (!Regex.IsMatch(body, @"\.Authorize\s*\(")) yield return "it does not authorize the target against the document";` (`:106`). Autoprueba de ausencia en `:124`.
- Hueco: pasan `{ controller.Port.Authorize(...); return true; }` y `!= RequestDecision.Accepted`, y no se fijan los dos primeros argumentos de `Run`. Sugerencia: `Authorize\s*\(\s*new\s+WorkspaceRequest\s*\(\s*\w+\s*\)\s*\)\s*==\s*RequestDecision\.Accepted` y los argumentos `controller?.PilotEdit` / `controller?.Port.GetSnapshot().ActiveSession`, con autoprueba en negativo.
- Por regla del Owner, se corrigen «where T4 touches those guard surfaces». T4A no toca el archivo del comando; T4B tampoco, salvo la vía de comando registrado (§8, R-3).

---

## 6. Infraestructura para probar PREPARE en Core sin AutoCAD
- **`WorkspaceHostRig`** (`tests/RackCad.Tests/Workspace/WorkspaceHostTestSupport.cs:110-199`): core real sobre el núcleo real del puente.
  - Dobles: `WorkspaceSimulatedProbe` `:11-16` (`ActiveDocumentNativeId`, `IsActiveDocumentQuiescent`), `WorkspaceSimulatedReader` `:19-47`, `WorkspaceSimulatedBridgeLink` `:53-104`.
  - Ayudas: `WithDocument` `:137-141`, `Activate` `:144-149`, `CloseActive` `:151-155`, `CommandStarts` `:169-173`, `CommandEnds` `:175-180` (inyecta `CommandEnded` + `EnteringQuiescentState`), `Idle` `:183-193` (drena y entrega a `Host.OnDrained`). Sirve para simular la mutación propia: pistas, drenaje y marcado.
  - `WorkspaceBridgeRig` y `WorkspaceSimulatedHostSource` en `tests/RackCad.Tests/Workspace/WorkspaceBridgeTestSupport.cs:70` (`Open` `:93`) y `:15`.
- **Dobles del lector de la intención:**
  - `EditIntentFakeReader` (`tests/RackCad.Tests/Workspace/WorkspaceEditIntentHostTests.cs:17-32`): registra `Calls`, `Answer` por rack (por defecto la lectura real del piloto) y `Throw`.
  - `EditIntentKit` (`:35-61`): `RackA`/`RackB`, `TwoViews`, `Admitted`, `ReadOf(scan)` (lector real `PushBackPilotIntentReader.Read(scan, PilotKit.Catalog)`), `OriginMissing` y `MemberBecameBlocking`, que da una base distinta y sirve de **cambio externo / Conflicto**.
  - `Rig` (`:73-112`), con `UnknownDraft`, que lleva el borrador a Unknown por `TryBeginApply` + `CompleteApply(CommitThrew)` (`:101-111`).
  - `PilotEditAuditedReader` (`tests/RackCad.Tests/Workspace/WorkspacePilotEditHostTests.cs:37-56`): cuenta `Reads` y tiene el gancho `InsideRead` para reentradas durante la lectura. Además `PilotEditThrowingProbe` (`:29-34`) y `Env` (`:63-…`), que cablea el asesor sobre el puente real.
- **Fixtures de Push Back:**
  - `PilotKit` (`tests/RackCad.Tests/Workspace/WorkspaceDraftPilotStateTests.cs:30-75`): `Catalog` (`JsonRackCatalogProvider.FromBaseDirectory()`), `Canonical`, `Legacy`, `Text(fixture)`, `Admitted`, `State`, `WithCulture`.
  - `IntentKit` (`tests/RackCad.Tests/Workspace/WorkspaceDraftPilotIntentStaleBaseTests.cs:21-80`): `Rack`, `Variant(fixture, id, view, section, name, kind)`, `Member`, `Unreadable`, `Custom`, `Scan(...)`.
  - Corpus JSON en `tests/RackCad.Tests/Fixtures/I64/` (17 archivos): `pb-01-base` … `pb-08-redondeo`, `pb-10…pb-15` (legados), `pb-20-ab`, `pb-21-cola`, `pb-31-v2-modulos`.
- Pruebas de la máquina de Update a nivel de borrador: `tests/RackCad.Tests/Workspace/WorkspaceDraftApplyTests.cs` y `ComposeForWrite` en `WorkspaceDraftPilotMaterializationTests.cs`.
- **UI:** `WorkspacePilotEditPortDouble` (`tests/RackCad.UI.Tests/Workspace/WorkspacePilotEditPortDouble.cs:25-…`): constantes de operación (`:27-38`), `Calls`/`Reads()` (`:49-58`), `ReadHook` (`:61`), responders (`:66-89`), `Throws` (`:92`). Habría que añadirle la operación nueva.
- **Guardas de texto:** `WorkspaceHostSource` (`tests/RackCad.Tests/Workspace/WorkspaceHostSourceSupport.cs:15-92`: `Code`, `Text`, `Files`, `BodyOf`, `Count`). Patrón de autoprueba con `Rejects(...)` y fuente sintética (p. ej. `WorkspacePilotCloseSyncGuardTests.cs:455-470`).

---

## 7. Costuras candidatas PREPARE / MUTATE / POST (sin implementar)

| Fase | Dónde | Archivo | Qué |
|---|---|---|---|
| Compromiso del texto pendiente | Application | `IWorkspacePilotEditPort.cs:148-156` (añadir `Update` al final de `PilotBoundaryKind`), `WorkspaceHostCore.cs:671-719` (traducir a `DraftBoundary.Update`) | frontera bloqueante antes de PREPARE (`TryBeginApply` exige sin texto pendiente, `DraftSession.cs:473`) |
| PREPARE (T4A) | Application | `IWorkspacePilotEditPort.cs` (operación nueva y un tipo valor nuevo sin Drafts ni Systems), `WorkspaceHostCore.cs` (operación pública con `PassGate`, una `ReadIntent` y la marca de lectura), `WorkspacePilotSupport.cs:153-195` (puerta de avisos), `WorkspaceController.cs:278-312` (reenvío) | relectura autoritativa por el lector existente; precondiciones de `TryBeginApply` sin tomar la ranura; comparación de la base por un **consumidor nuevo** de la StaleBase (p. ej. un método de `DraftSession` que compare la base con una relectura sin cambiar estado); admisión de la relectura; plan |
| Comparación sin estado | Application/Drafts | `DraftSession.cs` (junto a `CheckNow` `:571-589`) y `DraftStaleBase.CompareWith` (`DraftStaleBase.cs:141`) | separar StaleBase (`DraftSession.cs:266`), InitialDraftState (`:269`, copia), CurrentDraftState (`:272`) y la relectura del DWG (`PushBackPilotIntentResult.StaleBase/Read`) |
| Plan inmutable | Application/Systems/PushBack/Pilot | junto a `ComposeForWrite` (`PushBackPilotMaterializer.cs:189-215`) | composición pura desde la base vigente; necesita `RackCatalog`. No puede quedar en un campo del host (`WorkspaceEditIntentHostTests.cs:990-1002`) ni cruzar el puerto (`WorkspacePilotEditGuardTests.cs:171-217`): vive dentro de la llamada |
| «Comprobar ahora» (T4A) | UI | `WorkspacePilotFieldViewModel.cs` (gesto nuevo `CheckNow()` + `ShowCheckNowButton` en `Apply` `:351-365`), `WorkspacePanelView.xaml:214-230`, `.xaml.cs` | llamada desde el VM del campo, nunca desde el navegador (`WorkspacePilotCloseSyncGuardTests.cs:849-866`); el code-behind puede llamar `_viewModel.Browser.Field.CheckNow()` sin cambiar las 33 firmas |
| MUTATE (T4B) | Plugin, archivo nuevo | **fuera** de Plugin/Workspace (censo de 9, `PersistenceOrAuthoredWrite`, `HandlerForbiddenCalls`): candidato `src/RackCad.Plugin/Views/` junto a `RackPilotIntentAdapter.cs`, que debe seguir siendo solo de lectura | `ExecuteInCommandContextAsync` por archivo exacto; `using LockDocument`; **una** transacción; relectura caller-owned y comparación dentro (D-13, `docs/initiatives/I-64-proposal-v5.md:515-518`); unidades con `SiblingRedrawTransaction.ApplyInTransaction` (`src/RackCad.Plugin/Systems/Shared/SiblingRedrawTransaction.cs:34-54`); distinguir la excepción de `Commit()` (H-02: `Mutate` la convierte en `Discarded`, `:16-30`) |
| Lectura caller-owned | Plugin/Views | `RackSiblingScan.cs` (variante aditiva de `Traverse` sobre la transacción del llamador) | hoy `Traverse` abre bloqueo y transacción propios y hace `Commit` (`:112-113`, `:163`), y `RackPilotIntentAdapter.Read` exige `TopTransaction == null` (`:104`): ninguno de los dos se puede usar dentro de MUTATE (H-03, INV-39) |
| Puente Application ↔ Plugin | Application + Plugin/Workspace | una interfaz de escritor nueva (análoga a `IWorkspaceEditIntentReader`) asignada por un setter propio; el traductor en Plugin/Workspace | el adaptador de Views no puede nombrar `RackCad.Application.Workspace` (`WorkspaceBridgeSourceGuardTests.cs:361-366`); el host de navegación tiene 2 clases exactas (`WorkspaceEditIntentGuardTests.cs:436`) y no nombra el piloto (`:460`) |
| Máquina de estados | Application | `WorkspaceHostCore.cs` | primeros llamadores de `TryBeginApply`/`CompleteApply` (`DraftSession.cs:467`, `:490`); marca de «actualización en curso» en el host (hoy no existe); `NotePilotEdit` (obligación de `docs/initiatives/I-64-f4-t2b2-gate-contract-package.md:280`) |
| Ámbito propio | Application | `EventBridgeCore.cs:306-348` y `WorkspaceSession.cs:26-59` (o un `HintKind` propio que solo produzca `InvalidateNavigationCaches`); se abre antes de MUTATE y se cierra con el `CommandEnded`/`CommandCancelled` del propio comando (`AutoCadEventBridge.cs:198-211`) | C-F4-04 / IC-08; `DrainOperation` no puede crecer (`WorkspaceHintDrainTests.cs:102`) |
| POST (T4B) | Plugin/Systems/Shared o Views | `SiblingRedrawTransaction.Post` (`:56-67`, con `document?.Editor.Regen()`), relectura y luego `CompleteApply(Applied(postRead))` | `Regen` prohibido en las carpetas Workspace (`WorkspaceHostSourceGuardTests.cs:385`, `WorkspaceBridgeSourceGuardTests.cs:140`) |
| «Actualizar» (T4B) | UI | `WorkspacePilotFieldViewModel.cs` (gesto `Update()` + `ShowUpdateButton`), `WorkspacePanelView.xaml:214-230` | síncrono (sin `Task` en UI/Workspace salvo el VM de navegación); sin `IsDefault` |
| T4C (si se justifica) | Plugin | `WorkspacePanelCommand.cs` (tercer comando), canal del plan entre clic y comando | solo si Undo no es un paso con `ExecuteInCommandContextAsync` (D-13 `:542-543`) o si la frontera asíncrona exige un canal de finalización diferido |

---

## Guardas que cambian en T4A (sin escritura)
1. **Uso de la base** (`WorkspaceEditIntentGuardTests.cs:635-663`): añadir el consumidor de PREPARE a `AllowedConsumers` (`:638`), con su cuenta por cuerpo.
2. **Puerto y tipos valor** (`WorkspacePilotEditGuardTests.cs:43-63`, `:153-157`): operación nueva en `Operations`, en `ReadOperations` (si lee) y en `PortValueTypes`.
   - En la tabla de la marca de lectura (`:359`) y en la compuerta (`:418-435`).
   - En el adaptador del controlador (`:494-499`) y en `WriteViolations` por cuerpo (`:666-684`), para probar cero escrituras en PREPARE.
   - Actualizar los 6 implementadores, dobles incluidos.
3. **`EditCheckStatus` exacto** (`WorkspaceEditIntentHostTests.cs:1022`), si «Comprobar ahora» gana estados; aplicabilidad de `TryCheckPilotEdit` (`WorkspaceHostCore.cs:388-389`, guarda de orden `WorkspaceEditIntentGuardTests.cs:562`), si se amplía.
4. **`PilotFieldView` con un constructor y `errorReason` al final** (`WorkspaceHostHardeningGuardTests.cs:363-369`), si la vista gana campos (diferencias, causa del último intento).
5. **Literales** (`WorkspaceHostHardeningGuardTests.cs:398-473`): cada literal nuevo, o repetido en línea, cambia la tabla y las cuentas.
6. **R-3 de T3C1** («`ErrorReason` solo en Error», `WorkspaceHostCore.cs:909`), como cambio declarado.
7. **UI:**
   - las 33 firmas (`WorkspacePilotCloseSyncGuardTests.cs:379-446`), solo si el navegador gana un miembro público;
   - `TheTwoGestureReads` (`WorkspacePilotSurfaceGuardTests.cs:94`), si se usa `TryReload`/`TryBeginEdit` fuera del VM del campo;
   - la cuenta de botones de la superficie (`WorkspacePilotCloseSyncGuardTests.cs:737-740`), solo si se repite un `Content`.
8. Si PREPARE necesita una segunda lectura en el lector anidado: las cuentas de una sola llamada (`WorkspaceEditIntentGuardTests.cs:422-424`).
9. C2-M-1/C2-M-2 (`WorkspaceCommandAdvisorGuardTests.cs:133-147` y `:101-111`), si el paquete decide adelantarlos. T4A no toca esa superficie.

## Guardas que cambian en T4B
1. **`ExecuteInCommandContextAsync`**: excepción por archivo exacto del adaptador de mutación en `WorkspaceNavigationHostTests.cs:436`. Se mantienen las prohibiciones del host de navegación (`WorkspaceHostSourceGuardTests.cs:454`, `WorkspaceSourceGuardExceptionTests.cs:269`, `WorkspaceBrowserHostGuardTests.cs:259-262`) y de la UI (`WorkspacePilotSurfaceGuardTests.cs:280`, `WorkspacePilotCloseSyncGuardTests.cs:851`).
2. **`async`/`await`/`Task`**, solo si el `Task` entra en las carpetas Workspace, el controlador o la UI (`WorkspaceHostSourceGuardTests.cs:426`, `:452-453`; `WorkspaceNavigationHostTests.cs:435`; `WorkspaceSourceGuardExceptionTests.cs:267-268`; `WorkspacePilotEditGuardTests.cs:510`; `WorkspaceCommandAdvisorGuardTests.cs:238`; `WorkspacePilotSurfaceGuardTests.cs:283-289`; `WorkspacePilotCloseSyncGuardTests.cs:475`; `WorkspaceBrowserHostGuardTests.cs:271-273`). En `src/RackCad.Plugin/Views/` no hay prohibición de `async`.
3. **Escrituras**, si el adaptador vive en Views: las carpetas Workspace no cambian. Hay que ampliar `CallerOwnedFacadeGuardTests` (`tests/RackCad.Tests/CallerOwnedFacadeGuardTests.cs:50-56`, hoy 4 fachadas y ninguna de Push Back, H-04) y crear guardas propias del adaptador (una transacción, `Commit` como frontera, `TopTransaction == null`).
   - **Si alguna escritura toca Plugin/Workspace**, cambian todas: `WorkspaceBridgeSourceGuardTests.cs:295-307`, `WorkspaceHostSourceGuardTests.cs:373-378`, `:382-396` y `:457-464`, `WorkspaceBridgeSourceGuardTests.cs:139-180`, y `WorkspaceLifecycleGuardTests.cs:647-658` (controlador y puente).
4. **Censos de archivos y clases**: `Assert.Equal(9, plugin.Length)` (`WorkspaceEditIntentGuardTests.cs:808`) si se añade un archivo en Plugin/Workspace; dos clases en el host de navegación (`:436`); un `RackPilotIntentAdapter.Read` (`:423`); un `CaptureDrawing`, cero `GetObject` y un `RackBlockData.Read` en el adaptador de lectura (`:292-293`, `:320-321`). La lectura caller-owned debe ir en otro archivo o declararse.
5. **Pistas y drenaje**: el orden del drenaje y la forma del marcado (`WorkspaceLifecycleGuardTests.cs:523-550`), si el ámbito se aplica en el host; el censo de eventos (`WorkspaceBridgeSourceGuardTests.cs:79-123`), si hacen falta eventos nuevos o leer `e` (handle) en los manejadores (IC-04: los manejadores no leen el DWG).
6. **Comando registrado (solo con la A-n de Undo)**: el par exacto (`WorkspaceCommandAdvisorGuardTests.cs:36-46`; `WorkspaceHostSourceGuardTests.cs:49-62`; `WorkspaceBridgeSourceGuardTests.cs:346-358`; `WorkspacePilotEditGuardTests.cs:898`) y `SendStringToExecute` (`WorkspaceHostSourceGuardTests.cs:385`, `WorkspaceNavigationHostTests.cs:443`). Aquí entran C2-M-1/C2-M-2.
7. **`WriteMessage` solo en el archivo del comando** (`WorkspaceHostSourceGuardTests.cs:90-95`), si los avisos de POST van a la línea de comandos.
8. **Advisor con dos sentencias** (`WorkspaceCommandAdvisorGuardTests.cs:391`, C2-M-4), si el ciclo de vida gana un miembro.

---

## Riesgos o decisiones materiales
- **R-1. Reparto T4A/T4B frente al paquete v2.** `docs/initiatives/I-64-f4-gate-contract-package.md:47-48` define T4A como *MUTATION-SEAM* (entrada caller-owned, frontera de `Commit()`, inyección Debug, fachadas de Push Back, C-F4-05) y pide «un único `using LockDocument` alrededor de PREPARE, MUTATE y POST». D-13 (`docs/initiatives/I-64-proposal-v5.md:510-518`) exige la relectura y la comparación **dentro** de la transacción de MUTATE. La autorización del Owner redefine T4A como PREPARE/STALE/CONFLICT de solo lectura.
  - Consecuencia: el PREPARE de T4A es una comprobación previa con su propio bloqueo, por el lector existente. T4B tiene que repetir la relectura y la comparación dentro de MUTATE con una lectura **caller-owned**, que hoy no existe (`RackSiblingScan.cs:112-113`, `:163`; `RackPilotIntentAdapter.cs:104`).
  - Hay que asignar de forma explícita esa lectura caller-owned (es una lectura: cabe en T4A) y el resto de la costura de v2.
- **R-2. `ExecuteInCommandContextAsync` devuelve `Task`, y todo el camino del puerto es síncrono por guarda** (§3.1).
  - Si la llamada desde la paleta no ejecuta el callback antes de devolver, el resultado no puede volver por el puerto sin `await`. `await` está prohibido en las carpetas Workspace, y `Task` en el controlador y en la UI del campo.
  - F2 lo usó con `await` (`git show 7bdcbfd9:src/RackCad.Plugin/Workspace/WorkspaceNavigationHost.cs`, `ExecuteAsync`), y IC-01 lo retiró.
  - Dentro del contexto de comando, `CommandInProgress` no está vacío (EXECUTEFUNCTION, Smoke-NAV). La puerta del lector (`WorkspaceNavigationHost.cs:225-226`) y `PassGate` (`WorkspaceHostCore.cs:1055-1058`, `HostProbe` `WorkspaceController.cs:361-368`) responderían Busy/Waiting. Las puertas se evalúan antes de entrar, y dentro solo se revalida la sesión.
  - Es un hecho de host que hay que medir. Si el callback es diferido, hace falta un canal de finalización: cambio material de varias guardas, y candidato real a T4C.
- **R-3. Undo.** D-13 (`docs/initiatives/I-64-proposal-v5.md:542-543`): «un Actualizar del panel es un solo paso de UNDO… Si la prueba falla, la alternativa de mecanismo es un comando registrado lanzado con `SendStringToExecute`, mediante una A-n material» (también C-F4-02, `docs/initiatives/I-64-f4-gate-contract-package.md:59`, y la parada de `:117`).
  - Esa vía exige: un tercer `CommandMethod` (par exacto), `SendStringToExecute` (prohibido en las carpetas Workspace y en la UI) y perder el resultado síncrono (la orden se encola).
  - Además, el plan tendría que vivir en el host entre el clic y el comando, y el host no puede tener campos de tipos del piloto (`WorkspaceEditIntentHostTests.cs:990-1002`).
  - Es decisión material, y es la justificación más clara de un T4C, pero solo si la medición en host falla.
- **R-4. El ámbito propio con pistas sin handle.**
  - `_pending` agrupa por tipo (`WorkspaceSession.cs:26`). Una `DatabaseObjectChanged` externa ya pendiente antes del ámbito no se distingue de las propias, salvo con un tipo de pista aparte o vaciando antes. Y no se puede suprimir `InvalidateNavigationCaches` (IC-08).
  - El marcado es grueso por sesión (`WorkspaceHostCore.cs:1015-1019`). Suprimir las pistas propias deja sin marcar a todos los borradores de la sesión para esas pistas, lo que es correcto solo si el ámbito se cierra exactamente con el fin del propio comando. Con la opción «handle en la pista» habría que leer `e` en el manejador (frontera con IC-04).
  - El Owner pide un ámbito «rack/member-bound» y que no enmascare otros racks.
- **R-5. Catálogo y plan.** `ComposeForWrite` necesita `RackCatalog` (`PushBackPilotMaterializer.cs:189-190`), que solo se carga en el Plugin. El host core no lo tiene, el puerto no puede llevar tipos del piloto ni de Drafts, y el host no puede guardarlos en campos. El «plan inmutable» o se compone en el lado del Plugin o es interno a una sola llamada.
- **R-6. «Comprobar ahora» hoy no tiene objeto.** Solo sirve a Unknown/CommitThrew (`WorkspaceHostCore.cs:388-389`), y nada lo produce hasta T4B. Decidir si el «Comprobar ahora» de T4A es esa operación (inerte hasta T4B) o un PREPARE en seco nuevo. `EditCheckStatus` es exacto (`WorkspaceEditIntentHostTests.cs:1022`).
- **R-7. Descarte durante Applying.** `DiscardRack`/`DiscardDocument` no miran una marca de actualización (`WorkspaceHostCore.cs:727`, `:751`). `DiscardByUser` desregistra el borrador Applying, y la ranura se libera en `CompleteApply` como `ClosedDetached` (`DraftRegistry.cs:262-273`, `:363`; `DraftSession.cs:496-499`). La ranura no se libera antes de tiempo, pero el borrador sí se puede perder a mitad de la operación si el comando bombea la UI. Hace falta una marca del host análoga a `_pilotReadInProgress`, que respondería Busy.
- **R-8. Comparación por texto crudo.** La StaleBase compara el texto exacto de cada sobre miembro, sin normalizar (paquete F4 §6, `docs/initiatives/I-64-f4-gate-contract-package.md:96-102`). El Owner pide comparar hechos authored «where the Freeze says semantic comparison applies»; aquí el Freeze dice texto crudo. Una reserialización equivalente de otro escritor da Conflicto: es conservador y se declara.
- **R-9. POST con `Regen`.** `SiblingRedrawTransaction.Post` hace `document?.Editor.Regen()` (`:66`), y `Regen` está prohibido en las carpetas Workspace. POST tiene que vivir fuera de ellas.
