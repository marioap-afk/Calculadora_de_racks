# Mapa 2 — asesor de ciclo de vida, puente y controlador (F4-T3C2)

Medido en el worktree, rama `architecture/workspace-persistente-rackcad`, HEAD `e89948b9`. `git diff --stat 7bd275e4 HEAD -- src tests` sale vacío: el código y las pruebas de HEAD son los de `7bd275e4`. El worktree estaba limpio. Solo lectura: no se compiló ni se ejecutó nada.

## 0. Correcciones al mapa anterior (sobre 7895f397)

- Las líneas de `WorkspaceHostCore.cs` cambiaron con `7bd275e4`: `AdviseClose` está ahora en 926-953 (antes ~931 era solo la firma), `SessionDestroyed` en 963-992 y `GetRacksAtRisk` en 764-807. Los valores medidos están en §1.6.
- `WorkspaceLifecycleGuardTests.cs:620-628` termina en realidad en 632. El segundo bucle (630-631) prohíbe los campos `Document`/`Database`/… solo en 5 archivos de Application, no en el controlador.
- `WorkspaceHostSourceGuardTests.cs:393-437` son en realidad dos pruebas: 392-415 (UI/Dispatcher) y 417-437 (hilos/async).
- `WorkspacePaletteHostSourceGuardTests.cs:434-446` va de 434 a 448. `486-499` va de 488 a 498. La guarda del censo de 9 archivos no es `763-777`: la prueba es 750-777 (el `+=`=2 está en 762 y la lista de 9 archivos en 767-776).
- `WorkspacePilotEditGuardTests.cs:460-510`: `ControllerViolations` está en 463-511 y su `[Fact]` en 513-517. Además hay guardas que el mapa anterior no recogía (§2).
- `WorkspaceHostC2SourceGuardTests.cs:254-262` va de 253 a 261. `WorkspaceNavigationHostTests.cs:440-450` va de 439 a 453 y `667-677` de 666 a 679. `WorkspaceBrowserHostGuardTests.cs:140-151` va de 139 a 151, y `197-202` está dentro de 174-204 (las aserciones del relevo, en 198-203).
- `WorkspaceLifecycleGuardTests.cs:376-389` va de 375 a 389.
- El comando `RACKPANELDESCARTAR` NO existe en HEAD. `git grep` sobre `src` y `tests` solo lo encuentra en `tests/RackCad.UI.Tests/Workspace/WorkspacePilotCloseSyncTests.cs:785`, que dice `Assert.DoesNotContain("RACKPANELDESCARTAR", row.Text);`. Según `owner-f4t3c-decomposition-decision.md:6`, es trabajo de T3C2.
- Hay un hecho nuevo con riesgo: `CloseDecision` tiene `Allow` como primer miembro (`BridgeOutcomes.cs:8-12`, `Allow,` / `Veto,`). Por tanto, `default(CloseDecision)` es **Allow**.

## 1. Forma actual

### 1.1 Interfaz (`src/RackCad.Application/Workspace/IWorkspaceLifecycleAdvisor.cs`)

- 9: `public interface IWorkspaceLifecycleAdvisor`.
- 15: `CloseDecision AdviseClose(SessionId session);`. La documentación (12-13) dice `Every attempt is evaluated anew; repeating it is never consent. The core turns an exception of the advisor into a Veto.`
- 21: `void SessionDestroyed(SessionId session, bool closeSequenceIntact);`. La documentación (18-19) termina con `The advisor must not call the bridge back.`
- 5-7: `Both calls are synchronous and in memory: they read nothing of the drawing, write nothing and open no dialog.`

### 1.2 Núcleo del puente (`src/RackCad.Application/Workspace/EventBridgeCore.cs`)

- Campos: 57 `private IWorkspaceLifecycleAdvisor? _advisor;` y 61 `private SessionId? _lastAllowedClose;`. El registro es 48: `private readonly WorkspaceSessionRegistry _registry = new();`.
- 103: `public bool TryGetSession(SessionId id, out WorkspaceSession? session) => _registry.TryGet(id, out session);`. Hay otra sobrecarga por id nativo en 105-109. El activo está en 76: `public SessionId? ActiveSession => _active;`.
- 164-165: `public void SetLifecycleAdvisor(IWorkspaceLifecycleAdvisor advisor) => _advisor = advisor ?? throw new ArgumentNullException(nameof(advisor));`. La documentación (161-162) dice `a second call replaces the advisor. It is a query-like setup call: it does not forget the close sequence.`
- `Stop()` (192-210) NO limpia `_advisor`: solo libera conexiones y sesiones y pone `_started = false`. No existe ninguna API que desinstale el asesor (con `null` lanza).
- `OnBeginDocumentClose` (357-375):
  - `_lastAllowedClose = null;` / `var advisor = _advisor;` / `if (advisor == null) return CloseDecision.Allow;` / `var decision = CloseDecision.Veto;`.
  - En el `try`: `decision = advisor.AdviseClose(session) == CloseDecision.Allow ? CloseDecision.Allow : CloseDecision.Veto;` (366).
  - En el `catch (Exception ex)`: `Log("OnBeginDocumentClose", ex);` (370).
  - 373: `if (decision == CloseDecision.Allow) _lastAllowedClose = session;`.
  - No exige `_started`: no tiene la guarda `if (!_started) return`.
- `OnDocumentToBeDestroyed` (268-296):
  - `if (!_started) return;` / `var allowedBefore = _lastAllowedClose;` / `_lastAllowedClose = null;`.
  - Después vienen `_byNative.Remove(...)`, `var sequenceIntact = allowedBefore == id && !IsDegraded(id);` (280), la desconexión, `_states.Remove(id)`, `_registry.Destroy(id);` (287) y `if (_active == id) _active = null;` (288).
  - A continuación, `TellAdvisorTheSessionEnded(id, sequenceIntact);` (289) y `TrySyncIdle();` (290).
  - `catch (Exception ex) { Log("OnDocumentToBeDestroyed", ex); }`.
- `TellAdvisorTheSessionEnded` (446-458): `var advisor = _advisor; if (advisor == null) return; try { advisor.SessionDestroyed(id, closeSequenceIntact); } catch (Exception ex) { Log("OnDocumentToBeDestroyed.Advisor", ex); }`. El comentario (444-445) dice `It runs after the session left the registry and the active slot`.
- Hay una sola llamada a cada método del asesor: `advisor.AdviseClose(` en 366 y `advisor.SessionDestroyed(` en 452. Las dos leen el campo una sola vez (`var advisor = _advisor;`, 360 y 448), así que sustituir el asesor no afecta a una llamada que ya esté en curso.

### 1.3 Puente de AutoCAD (`src/RackCad.Plugin/Workspace/AutoCadEventBridge.cs`)

- 76: `internal void SetLifecycleAdvisor(IWorkspaceLifecycleAdvisor advisor) => _core.SetLifecycleAdvisor(advisor);`. La documentación (72) dice `the controller calls it once with the host when it creates the bridge`.
- 98-103, fachada: `public bool TryGetSession(SessionId id, out WorkspaceSession session) { var found = _core.TryGetSession(id, out var live); session = live; return found; }`. En 96: `public SessionId? ActiveSession => _core.ActiveSession;`.
- 57: `internal event Action ActiveSessionChanged;`. Solo se lanza en dos sitios:
  - En `DocumentActivated` (154-161), dentro de `Contain`, después del núcleo y también cuando `e.Document == null` (pestaña de inicio).
  - En `DocumentToBeDestroyed` (168-173): `var before = _core.ActiveSession; _core.OnDocumentToBeDestroyed(NativeId(e.Document)); if (_core.ActiveSession != before) ActiveSessionChanged?.Invoke();`. El relevo llega DESPUÉS de `SessionDestroyed` y en el mismo manejador, pero solo si el documento destruido era el activo.
- 180, cierre: `DocumentBeginCloseEventHandler handler = (s, e) => ContainSession(a, () => { if (_core.OnBeginDocumentClose(a.Session) == CloseDecision.Veto) e.Veto(); });`.
- Nombre del documento por sesión: **no hay nada público**.
  - `WorkspaceSession` (`WorkspaceSession.cs:16-23`) expone `Id`, `NativeInstanceId`, `IsDestroyed`, `Selection`, `PendingHintCount`… y no tiene nombre.
  - En el puente, `NativeId` (333) es `private static long NativeId(Document document) => document.UnmanagedObject.ToInt64();`. `FindDocumentOrNull` (335-343) es `private static Document FindDocumentOrNull(long nativeInstanceId)` y recorre `foreach (Document candidate in AcApplication.DocumentManager)`.
  - La superficie pública del puente está fijada en 9 nombres (§2, `WorkspaceBridgeFacadeSourceGuardTests.cs:90-104`).
- 116-126: `public void Dispose() { try { _core.Stop(); } catch … }`. Nadie la llama: `git grep` de `Dispose`/`.Stop()` en las dos carpetas Workspace solo encuentra la declaración.

### 1.4 Controlador (`src/RackCad.Plugin/Workspace/WorkspaceController.cs`)

- `using` (1-5): `System`, `System.Diagnostics`, `RackCad.Application.Diagnostics`, `RackCad.Application.Workspace` y el alias `using AcApplication = Autodesk.AutoCAD.ApplicationServices.Application;`. NO tiene `using Autodesk.AutoCAD.ApplicationServices;`, así que hoy no nombra el tipo `Document`.
- Vida del controlador:
  - 23-24: `private static readonly WorkspaceLazyInstance<WorkspaceController> Holder = new WorkspaceLazyInstance<WorkspaceController>(() => new WorkspaceController());`.
  - La documentación (10-11) dice `lives until AutoCAD ends`.
  - No implementa `IDisposable` y no tiene `Dispose` ni ninguna desinstalación.
  - `WorkspaceLazyInstance` (`WorkspaceLazyInstance.cs:39-52`) no tiene `Reset`.
- Campos (26-33): `_bridge` (`AutoCadEventBridge`), `_host` (`WorkspaceHostCore`), `_port`, `_echo`, `_noticeDepth`, `_pilotEdit` (`readonly`), `_navigation` y `_modalScope` (no `readonly`).
- Constructor (35-57):
  - `_bridge = new AutoCadEventBridge();` (37), `_echo = new NavigationEcho();`, `_host = new WorkspaceHostCore(new BridgeLink(_bridge), new HostProbe(_bridge), new WorkspaceSelectionReader(_echo), new WorkspaceInstrumentation(), LogFault, _echo);` (39-45), `_port = …`, `_pilotEdit = new PilotEditAdapter(new WorkspacePilotEditGate(_host, _noticeDepth));` (49).
  - 53: `_bridge.SetLifecycleAdvisor(_host);`.
  - 54-56: `_bridge.Drained += OnDrained;`, `_bridge.ActiveSessionChanged += OnActiveSessionChanged;`, `_bridge.CommandStateChanged += OnCommandStateChanged;`.
- Relevo (157-171): `private void OnActiveSessionChanged() { try { if (PanelVisible) _host.RefreshPanel(); } catch (System.Exception ex) { LogFault("WorkspaceController.OnActiveSessionChanged", ex); } NotifyStateChanged(); }`.
- Otros miembros:
  - 175: `private void OnCommandStateChanged() => NotifyStateChanged();`.
  - `OnDrained`: 177-201.
  - `NotifyStateChanged`: 206-221, con `_noticeDepth.Enter()` y `try/finally`.
  - `AssertNoOpenTransaction`: 223-229, `[Conditional("DEBUG")]`.
  - 231: `private static void LogFault(string context, System.Exception exception) => RackLog.Exception(context, exception);`.
- Clases anidadas: `PilotEditAdapter` (234-268), donde 263 es `public PilotRiskView GetRacksAtRisk(WorkspaceRequest origin) => _gate.GetRacksAtRisk(origin);`; `BridgeLink` (271-294), cuyo 288 es `TryGetSession` → `_bridge.TryGetSession`; y `HostProbe` (297-325), que en 310-311 dice `var document = AcApplication.DocumentManager.MdiActiveDocument; return document == null ? (long?)null : document.UnmanagedObject.ToInt64();`.
- Ya existe un precedente de lambda sesión→nativo: está en 87, en el getter `Navigation` y no en el constructor: `id => _bridge.TryGetSession(id, out var session) && !session.IsDestroyed ? session.NativeInstanceId : (long?)null`.
- `DocumentManager` aparece en 226, 310 y 321. No hay `WriteMessage`. `RackLog` solo aparece en `LogFault` (231).
- ¿Se puede reemplazar o desinstalar el asesor? Solo reemplazarlo, con otra llamada a `SetLifecycleAdvisor`, que hoy hace únicamente el constructor (una vez por proceso). Desinstalarlo no se puede. Al liberar no ocurre nada: no hay liberación del controlador, y `Stop()` del núcleo, si se llamara, no tocaría `_advisor`.

### 1.5 Archivo del comando (`src/RackCad.Plugin/Workspace/WorkspacePanelCommand.cs`)

- 1-2: `using Autodesk.AutoCAD.Runtime;` y el alias `AcApplication`. 12: `public sealed class WorkspacePanelCommand`. 14: `[CommandMethod("RACKPANEL")]`.
- 19-22: `WorkspaceController.GetOrCreate().OpenPanel(); WorkspacePaletteHost.GetOrCreate().Show(); AcApplication.DocumentManager.MdiActiveDocument?.Editor.WriteMessage("\nRackCad: panel Workspace mostrado.");`.
- 24-27: `catch (System.Exception ex) { RackCommandSupport.Report(ex); }`.
- No tiene ningún ayudante estático. `RackCommandSupport.Report` (`src/RackCad.Plugin/RackCommandSupport.cs:23-32`) hace `RackLog.Exception("RackCad command", ex)` y luego `document?.Editor.WriteMessage("\nRackCad error: " + ex.Message)`.

### 1.6 Host (`src/RackCad.Application/Workspace/WorkspaceHostCore.cs`)

- 38: `public sealed class WorkspaceHostCore : IWorkspaceLifecycleAdvisor, IWorkspacePilotEditPort`.
- Campos: 64 `private readonly HashSet<SessionId> _knownClean = new();`, 65 `private readonly Queue<DraftLossNotice> _lossNotices = new();` y 69 `private readonly List<PilotCloseView> _closeNotices = new();`. Constructor en 76-91: `logFault` es `Action<string, Exception>?`.
- `GetRacksAtRisk` (765-807), con la firma `PilotRiskView IWorkspacePilotEditPort.GetRacksAtRisk(WorkspaceRequest origin)`:
  - Primero hace `return new PilotRiskView(_drafts.RacksAtRiskOf(session), Array.Empty<string>());`. Si el comparador lanza, cae a una vía de reserva borrador a borrador (779-800): `uncertain.Add(rackId)` cuando `draft == null` o cuando `HasPendingText || IsDirty` lanza.
  - El `catch` externo (802-806) es `Report("WorkspaceHostCore.GetRacksAtRisk", ex); return default;`, que devuelve el **desconocido**.
  - No toca `_knownClean`, `_closeNotices` ni `_lossNotices`.
  - El comentario 764 dice `Memory only: no authorization (the veto of a close can belong to a document that is not the active one), never throws.`.
- `AdviseClose` (931-953):
  - Empieza con `_knownClean.Remove(session);`.
  - En el `try`: `var decision = _closePolicy.EvaluateClose(session); if (decision.IsVeto) { QueueCloseNotice(session, decision.RackIds, false); return CloseDecision.Veto; } DropCloseNotice(session); _knownClean.Add(session); return CloseDecision.Allow;`.
  - En el `catch (Exception ex)`: `Report("WorkspaceHostCore.AdviseClose", ex); QueueCloseNotice(session, Array.Empty<string>(), true); return CloseDecision.Veto;`.
- 955-959: `QueueCloseNotice` = `DropCloseNotice(session); _closeNotices.Add(new PilotCloseView(session, rackIds, uncertain));`. 961: `DropCloseNotice` = `_closeNotices.RemoveAll(notice => notice.Session == session);`.
- `SessionDestroyed` (968-992):
  - `var marked = _knownClean.Remove(session); DraftLossNotice? notice = null;`.
  - Primer `try`: `notice = marked && closeSequenceIntact ? _drafts.RetireKnownSafe(session) : _drafts.RetireConservative(session); DropCloseNotice(session);`. Su `catch` hace `Report(..., ex)`.
  - Segundo `try`: `if (notice == null || !notice.HasLoss) return; _lossNotices.Enqueue(notice); Report("WorkspaceHostCore.DraftLoss", new InvalidOperationException(DescribeLoss(notice)));`. Su `catch` hace `Report`.
  - Es decir, el host ya **registra** la pérdida (966-967 y 986), cosa que con QUIT sigue ocurriendo.
- 995: `internal IReadOnlyCollection<DraftLossNotice> LossNotices => _lossNotices;`. Application expone sus `internal` a pruebas: `src/RackCad.Application/RackCad.Application.csproj:11` `<InternalsVisibleTo Include="RackCad.Tests" />`.
- 1074-1084: `Report` llama a `_logFault?.Invoke` dentro de `try` y tiene un `catch` vacío.
- `PilotRiskView` (`IWorkspacePilotEditPort.cs:284-312`): `public bool IsKnown => _known;`. El constructor pone `_known = true` y el valor `default` es el desconocido. La documentación (286-287) dice `It is never "nothing at risk".`. El contrato del puerto (66-70) dice `When it could not be decided it answers the default value … never "nothing at risk".`.
- **Después de la retirada da vacío CONOCIDO, no desconocido:**
  - `DraftRegistry.RacksAtRiskOf` (`Drafts/DraftRegistry.cs:192-195`) devuelve `Array.Empty<string>()` si `_bySession` no tiene la sesión.
  - `RetireKnownSafe` y `RetireConservative` hacen `_bySession.Remove(session)` (302 y 345).
  - La prueba `WorkspacePilotEditHostTests.cs:1520-1536` (`ASessionWithoutDraftsAnswersKnownAndEmptyAndNeverTheUnknown`) fija que una sesión sin borradores (777) da `IsKnown` true y la lista vacía.
- Diferencia semántica que hay que tener presente:
  - `RacksAtRiskOf` cuenta `e.IsDirty || e.HasPendingText` y llama al comparador.
  - `RetireKnownSafe` no llama al comparador: así lo fija `WorkspaceLifecycleGuardTests.cs:686-699`, `\.IsDirty\b|_comparer|…` prohibido. La pérdida que «ve» el envoltorio puede, por tanto, no coincidir con el `DraftLossNotice` del host en el residual conocido (`WorkspaceLifecycleHostTests.cs:787`).
- El controlador puede llegar a `GetRacksAtRisk` por `_host` (implementación explícita, que requiere convertir a `IWorkspacePilotEditPort`) o por la compuerta. `WorkspacePilotSupport.cs:190` dice `public PilotRiskView GetRacksAtRisk(WorkspaceRequest origin) => _inner.GetRacksAtRisk(origin);`, una delegación sin compuerta.

## 2. Guardas que afectan al diseño

Se usan estas abreviaturas. LGT = `tests/RackCad.Tests/Workspace/WorkspaceLifecycleGuardTests.cs`. HSG = `WorkspaceHostSourceGuardTests.cs`. PHG = `WorkspacePaletteHostSourceGuardTests.cs`. PEG = `WorkspacePilotEditGuardTests.cs`. BSG = `WorkspaceBridgeSourceGuardTests.cs`. Todas están en `tests/RackCad.Tests/Workspace/`. `Src.Code` quita los comentarios pero NO los literales de cadena (`WorkspaceHostSourceSupport.cs:29-39`).

### 2(a) Instalar un envoltorio en lugar de `_host`

| Guarda | Aserción literal | ¿Rompe? |
|---|---|---|
| LGT:277-288 (M-1) | 283 `Assert.Equal(1, Src.Count(code, @"\.SetLifecycleAdvisor\s*\("));`. 284 `AssertInOrder(constructor, "_host = new WorkspaceHostCore(", "_bridge.SetLifecycleAdvisor(_host);");`. 285 `Assert.Equal(3, Src.Count(code, @"\+="));`. 286 `-=` = 0. 287 `Assert.DoesNotContain(".Start(", constructor);` | **SÍ, 284**: con `_bridge.SetLifecycleAdvisor(new X(_host, …));` o con una variable desaparece el literal. 283, 285, 286 y 287 se mantienen si hay una sola llamada y ningún `+=` nuevo. |
| LGT:233-246 | 239-240: el `SetLifecycleAdvisor` interno del puente es literal. 242: `Assert.Equal(1, Src.Count(code, @"\bIWorkspaceLifecycleAdvisor\b"));` en el puente. 243-245: `+=`/`-=`/`handler = (` = 16 | No, si el puente no cambia. |
| LGT:178-191 | `SetLifecycleAdvisor` del núcleo no contiene `_lastAllowedClose` | No. |
| LGT:77-89 | `_advisor = advisor ?? throw new ArgumentNullException(...)`; un solo campo `_advisor` | No. |
| HSG:137-147 | En el constructor: `DoesNotContain(".Start(")`, `"OpenPanel"` y `"RequestReconciliation"` | No (el constructor del envoltorio no debe llamar a nada con esos nombres). |
| PHG:407-422 | Lo mismo, y además `_bridge.Drained += OnDrained;` etc. en el constructor y `+=`=3 / `-=`=0 | No. |
| WorkspaceEditIntentGuardTests.cs:483-498 | 492: `Assert.DoesNotMatch(new Regex(@"SetEditIntentReader|EditIntentReader"), constructor);` | No (que el envoltorio no reciba el lector en el constructor). |
| WorkspaceNavigationHostTests.cs:666-679 | 673: `Assert.DoesNotContain("WorkspaceNavigationHost", constructor);`. 678: el controlador no tiene `\bTask\b|\basync\b` | No. |
| WorkspaceBrowserHostGuardTests.cs:139-151 | 147: `Assert.DoesNotContain("BrowserSummary", constructor);` | No. |
| PEG:463-511 | 487: el literal `_pilotEdit = new PilotEditAdapter(new WorkspacePilotEditGate(_host, _noticeDepth));`. 503: no hay `public … WorkspaceHostCore`. 507-510: `+=`=3, `-=`=0, `\bevent\b`=1, sin `Dispatcher|Timer|Task|async` | No. |
| WorkspaceFacade `WorkspaceBridgeFacadeSourceGuardTests.cs:90-104` | La superficie pública del puente = `{ "ActiveSession", "Authorize", "Dispose", "Drained", "RequestReconciliation", "SetModalRackCadActive", "SetPanelVisible", "Start", "TryGetSession" }` (101-103) | No. Rompería si se añadiera algo **público** al puente, por ejemplo un nombre de documento; lo `internal` no cuenta (regex en 94). |
| WorkspaceBridgeFacadeSourceGuardTests.cs:206-228 | Los miembros públicos de `EventBridgeCore`, por reflexión | No, si el núcleo no cambia. |
| HSG:172-182 | El controlador nombra `_bridge.ActiveSession`, `_bridge.TryGetSession(`, `_bridge.Authorize(`, `_bridge.RequestReconciliation(`; `DoesNotMatch("WorkspaceSessionRegistry|EventBridgeCore")` | No, si el envoltorio no se llama `…EventBridgeCore…`. |

### 2(b) Clase nueva en `src/RackCad.Application/Workspace` que implemente `IWorkspaceLifecycleAdvisor`

**Ninguna guarda cuenta los implementadores del asesor ni fija que solo `WorkspaceHostCore` lo implemente.** `git grep IWorkspaceLifecycleAdvisor` en `tests` solo encuentra firmas del host, del núcleo y del puente, y los dobles. Lo que sí hay:

| Guarda | Aserción literal | ¿Rompe? / condición |
|---|---|---|
| LGT:60-73 | La interfaz se fija por sus dos firmas (69-70) | No. |
| LGT:292-303 | El host la implementa de forma explícita; `typeof(WorkspaceHostCore).GetMethod("AdviseClose")` es null | No. |
| PEG:291-298 | 297: `Assert.Equal(new[] { "IWorkspaceLifecycleAdvisor", "IWorkspacePilotEditPort" }, typeof(WorkspaceHostCore).GetInterfaces()…)` | No, mientras el envoltorio sea otra clase. |
| LGT:206-219 | 218: `Assert.Equal(1, Src.Count(code, @"\.SessionDestroyed\s*\("));` sobre **EventBridgeCore** | No: `_inner.SessionDestroyed(` en el archivo nuevo no cuenta. |
| WorkspaceHostHardeningGuardTests.cs:163-205 | `SessionDestroyed` del **host** (2 `try`, 2 `catch (Exception ex)`, `DropCloseNotice` tras la retirada) | No. Solo lee `HostPath`. |
| LGT:604-612 | 607: `TryReloadPilotEdit|…|SessionDestroyed|AdviseClose` prohibidos en `OnIdle` del núcleo y del puente y en `OnDrained` del controlador | No, mientras el `OnDrained` del controlador no nombre al envoltorio por esos métodos. |
| WorkspacePilotCloseSyncGuardTests.cs:175-197 | `\bTake(?:Close|Loss)Notices\b` solo en `IWorkspacePilotEditPort.cs`, `WorkspaceHostCore.cs`, `WorkspacePilotSupport.cs` y `WorkspaceController.cs` (180-183) | Rompe **si** el archivo nuevo nombra `TakeLossNotices`/`TakeCloseNotices`. Con `GetRacksAtRisk` no rompe. |
| WorkspaceModelBoundaryTests.cs:80-91 | Para todos los tipos del espacio `RackCad.Application.Workspace` y sus miembros públicos: `ProjectSummary|Metric|Provider|TotalRacks|Aggregat|Bom\b` (IgnoreCase) | Rompe si el nombre del tipo o de un miembro público contiene, por ejemplo, `Provider`. |
| WorkspaceModelBoundaryTests.cs:93-105 | 101: ningún nombre de tipo con `Subscription|Editor|Draft`. 102: ninguno con `Tab` salvo `BrowserTabRecord`/`BrowserTabStatus` (IgnoreCase; incluye los tipos anidados) | Rompe con nombres como `…DraftLoss…`, `…Editor…` o `…Table…`/`…Notable…`. |
| WorkspaceBrowserBoundaryTests.cs:284-294 | Otra vez `Tab` (IgnoreCase) en el espacio | Igual que la anterior. |
| WorkspaceBrowserBoundaryTests.cs:83-100 | Cada nombre de tipo del espacio (salvo `Browser*`/`<*`) queda prohibido como palabra en los archivos `Browser*` | Rompe si el nombre coincide con una palabra de esos archivos. Hay que usar un nombre distintivo. |
| WorkspaceDraftBoundaryTests.cs:422-452 | Cada tipo del espacio padre salvo `SessionId` queda prohibido como palabra en `Drafts/*` | Igual: hace falta un nombre distintivo. |
| HSG:524-529 | `\b(?:class|interface|struct|record)\s+\w*Log\w*\b` prohibido en Plugin+Application | Rompe con un nombre como `…Log…`, por ejemplo `LossLog`. |
| HSG:545-552 y BSG:362-372 | `ComputedParameter|Metric|ProjectSummary|TotalRacks|Aggregat|Provider|\bBom` (IgnoreCase) en todo el código **y en los literales** de Plugin/Application | Rompe, por ejemplo, con `documentNameProvider`. |
| HSG:495-500 | Application/Workspace: `DoesNotMatch("RackLog|RackCad\.Application\.Diagnostics")` | Rompe si el envoltorio registra con `RackLog`. Debe usar `Action<string, Exception>` como el host. |
| HSG:392-415, 417-437, 451-458, 376-390 | Sin `System.Windows|Dispatcher|UserControl…`, sin `Task.Run|Thread|Timer|async|await|…`, sin `System\.IO|\bFile\.|\bDirectory\.|Registry|UserSettings…`, sin `Zoom\w*|Regen|Highlight|SendStringToExecute|SetSystemVariable…` | Rompe con cualquiera de esos nombres. Ojo: impide `System.IO.Path.GetFileName` para acortar el nombre del documento. |
| WorkspaceModelBoundaryTests.cs:26-49, BSG:406-417 | Sin `\bAutodesk\.`, `RackCad\.Plugin`, `RackCad\.UI`, `System\.Windows`, `Transaction`, `LockDocument`, `\bDatabase\b`, `\bCommit\(` | Igual. |
| WorkspaceEditIntentGuardTests.cs:504-516 | Solo `WorkspaceHostCore.cs`, la interfaz del lector y `WorkspaceEditIntent.cs` pueden nombrar `\bPushBackPilot\w*` | Rompe si el envoltorio nombra tipos del piloto. |
| WorkspaceBridgeFacadeSourceGuardTests.cs:38-49 | `WorkspaceSessionRegistry` solo en el núcleo y en su archivo | Rompe si el envoltorio lo nombra. |
| PEG:639-646 | Sin `ShowModalWindow|ShowDialog|MessageBox` fuera de `EnterPanelModal` | Rompe si el aviso fuera modal. |
| PEG:776-793 | Trampas, **solo** para `IWorkspacePilotEditPort.cs`, `WorkspacePilotSupport.cs`, el host y (por texto, con comentarios) el archivo del intento: nombres de tipo con `Draft|Editor|Subscription|Tab` (IgnoreCase) y palabras de `BrowserSession.cs`/`Drafts`; `Transaction|\bDatabase\b|LockDocument|\bCommit\s*\(|RackLog`; `\bPushBackPilot\w*` | Solo si el envoltorio se pone en `WorkspacePilotSupport.cs`. En un archivo nuevo no aplica. |
| PEG:411-439 (`GateViolations`) | El cuerpo de la clase `WorkspacePilotEditGate`: `Dispatcher|\bTimer\b|\bTask\b|\basync\b|\bevent\b|\+=|\bstatic\b` | Solo si el código se mete en el cuerpo de la compuerta. |
| HSG:554-568 | Cota inferior por nombre de archivo, no cuenta | No. |

### 2(c) El relevo `OnActiveSessionChanged` escribe los mensajes pendientes con un ayudante estático de `WorkspacePanelCommand.cs`

| Guarda | Aserción literal | ¿Rompe? / condición |
|---|---|---|
| PHG:434-448 | 440: `Regex.Match(body, @"if\s*\(\s*PanelVisible\s*\)\s*_host\.RefreshPanel\s*\(\s*\)\s*;")`. 443: `NotifyStateChanged()` después de esa guarda. 444: `Assert.Contains("catch (System.Exception ex)", body);`. 445: `LogFault(`. 446: `DoesNotContain("RequestReconciliation")`. 447: **`Assert.DoesNotContain("_bridge.", body);`** | No, si la llamada no usa `_bridge.` **en el cuerpo** del relevo. La guarda solo lee ese cuerpo; un método privado aparte no está cubierto. |
| WorkspaceBrowserHostGuardTests.cs:174-204 | 200-201: `Assert.DoesNotContain("DrainNotice", session);` en el cuerpo del relevo | Rompe si un nombre contiene `DrainNotice` (por ejemplo `DrainNotices…`). |
| PHG:424-432 y PEG:509 | `Assert.Equal(1, Src.Count(code, @"\bevent\b"));` en el controlador | No, si no se declara un evento. |
| HSG:84-89 | `foreach (var (path, code) in PluginOutside(CommandPath)) Assert.DoesNotMatch(new Regex(@"\bWriteMessage\b"), code);` | No: el controlador no escribe `WriteMessage`; la llamada está en el archivo del comando, que es la excepción. Rompe si el nombre del ayudante contiene la palabra completa `WriteMessage` y se cita en el controlador. |
| HSG:49-56, PHG:320-330, BSG:320-351 | El archivo del comando tiene exactamente 1 `[CommandMethod`, sin `CommandFlags|\[assembly|CommandClass|LispFunction|IExtensionApplication` | No por el ayudante (sí por `RACKPANELDESCARTAR`, que es de otro mapa). |
| HSG:65-73 | El comando contiene `WorkspaceController.GetOrCreate()` y `.OpenPanel()`; sin `new WorkspaceController|PaletteSet|AddVisual|System\.Windows|AutoCadEventBridge|StartTransaction|LockDocument|SelectImplied|GetObject` | No, si el ayudante no nombra eso. |
| PHG:299-309 | `GetOrCreate().OpenPanel()` antes de `WorkspacePaletteHost.GetOrCreate().Show()` | No. |
| PHG:311-318 | El comando no tiene `PaletteSet|AddVisual|System\.Windows|RackCad\.UI|Autodesk\.AutoCAD\.Windows|UserControl|Dispatcher` ni `no\s+muestra|aun\s+no|todavia\s+no|not\s+shown` (IgnoreCase, **también en literales**) | Rompe si un texto del archivo del comando dice «no muestra» o «todavia no» (sin tilde). Si los textos se componen en Application, no hay riesgo. |
| HSG:106-117 | `\bstatic\s+(?:Workspace\w+|AutoCadEventBridge)\s*\(\s*\)` prohibido | No, con `internal static void X(string …)`. |
| HSG:502-514 | Fuera del puente, `RackLog.` solo dentro de `static void LogFault(...)` | Rompe si el ayudante registra con `RackLog` fuera de un `LogFault`. |
| HSG:435-436 | `Assert.DoesNotMatch(new Regex(@"\.Result\b"), code);` en todos los archivos del Plugin | Rompe si el ayudante o el controlador usan `.Result`. |
| HSG:460-471 | Línea `^\s*(?:private|public|internal|protected)\s+(?:static\s+)?(?:readonly\s+)?(?:Transaction|DBObject|Document|Database|Entity|BlockReference|ObjectId|Editor)\b[^(]*[;=]` → `Assert.Fail` | Rompe con un campo o con una propiedad de cuerpo de expresión cuyo tipo sea `Editor`/`Document` (por ejemplo `private static Editor Ed => …;`, porque el `=` de `=>` cuenta). Un método (`(` antes) no rompe. |
| BSG:205-243 | `ForbiddenFieldLines`: declaraciones con `Transaction|OpenCloseTransaction|DBObject|Document|Entity|BlockReference` antes de `=`/`;`/`{` | Igual (por ejemplo `private Document Active { get; }`). |
| BSG:216-252 | Líneas `public`/`protected` sin `Document|DocumentCollection|Editor|Database|…|Autodesk` ni `\w+EventArgs` | Rompe si el ayudante es `public static void W(Editor e…)`. Con un parámetro `string` no rompe. |
| BSG:139-175 | Llamadas prohibidas en los archivos del Plugin: `…|GetString|…|ShowModalDialog|ShowModelessDialog|ShowDialog|MessageBox|GetObject|…|Zoom|Regen|…` | Rompe si se usa, por ejemplo, `MessageBox`. |
| WorkspaceNavigationHostTests.cs:609-622 | `WorkspacePanelCommand.cs` sin `WorkspaceNavigation|NavigationEcho|IWorkspaceNavigationPort` | No. |
| WorkspaceNavigationHostTests.cs:439-453 | Controlador sin `…|\bThread\b|ThreadPool|\bTimer\b|…|Dispatcher|SendStringToExecute|\[CommandMethod|StartCommand|…|SetFocus|user32` | No. |
| PHG:136-146 | `\bWorkspacePaletteHost\b` solo en el host de la paleta y en el comando | No. |

### 2(d) El controlador guarda mensajes pendientes en un campo (`List`/`Queue`)

| Guarda | Aserción literal | ¿Rompe? / condición |
|---|---|---|
| — | **No hay censo de campos del controlador.** `git grep` no encuentra en las pruebas ningún recuento de `readonly`/`List<`/`Queue<` sobre el controlador. | No. |
| LGT:285, HSG:184-196, PHG:414, WorkspaceHostC2SourceGuardTests.cs:137, WorkspaceIdleSchedulingGuardTests.cs:514-515, WorkspaceNavigationHostTests.cs:676, WorkspaceBrowserHostGuardTests.cs:148, PEG:507 | `+=` = 3 en el controlador (8 guardas) | Rompe con **cualquier** `+=` nuevo, también `text += …` o `_count += 1` (lo demuestra `WorkspaceIdleSchedulingGuardTests.cs:529`). |
| LGT:286, HSG:195, PHG:415, C2:138, Nav:677, Browser:149, PEG:508 | `-=` = 0 | Rompe con cualquier `-=`. |
| PHG:488-498, PEG:510 | Sin `\bTimer\b|\bThread\b|\bTask\b|\basync\b|Dispatcher|UnhandledException` | Rompe si se vacía con temporizador o hilo. |
| HSG:460-471, BSG:205-243 | Sin campos de tipo host | No, con `List<string>`. |
| LGT:620-632, WorkspaceHostC2SourceGuardTests.cs:247-261 | El controlador sin `WriteApis` (`OpenMode.ForWrite…|\.Commit\s*\(|…|LockDocument|SetSystemVariable|SendStringToExecute|\bSaveAs\b|\bSave\s*\(`) ni persistencia/registro | No. |

### 2(e) Colindantes (no son de este mapa y no se cuentan)

- Censo de comandos: `SelectiveEditorOpenTests.cs:559-567` (`Assert.Equal(39, total);`), `CustomPropertiesCommandGuardTests.cs:55` `F1PanelCommands = { "RACKPANEL" }`, HSG:36-56, PHG:320-330, BSG:320-351 y `WorkspaceSourceGuardExceptionTests.cs:36-61`.
- El texto del panel: `tests/RackCad.UI.Tests/Workspace/WorkspacePilotCloseSyncTests.cs:785` `Assert.DoesNotContain("RACKPANELDESCARTAR", row.Text);`.
- Metaguardas sobre archivos de prueba: LGT:797 `Assert.Equal(45, Src.Count(Src.Text("tests/RackCad.Tests/Workspace/WorkspaceHostSourceGuardTests.cs"), @"\[(?:Fact|Theory)\b"));` y PHG:659 `Assert.Equal(45, …)`. **Si la guarda nueva se añade a HSG, rompen las dos**: conviene ponerla en otro archivo.
- Censo de 9 archivos del Plugin: LGT:794 `Assert.Equal(9, Src.PluginFiles().Count);`, PHG:767-776 (lista exacta), `WorkspaceEditIntentGuardTests.cs:808`, `WorkspaceNavigationHostTests.cs:686` y `WorkspaceBrowserHostGuardTests.cs:38`. El adaptador tiene que vivir en el controlador y en el archivo del comando; un archivo nuevo del Plugin rompe 5 guardas.

## 3. Propiedades de comportamiento ya fijadas (base para sustituir M-1)

Todas las pruebas están en `tests/RackCad.Tests/Workspace/`. WLH = `WorkspaceLifecycleHostTests.cs`. PEH = `WorkspacePilotEditHostTests.cs`. ISC = `WorkspaceIdleSchedulerTests.cs`.

1. **Asignación**: WLH:132-142. `SetLifecycleAdvisor(null!)` da `ArgumentNullException` y el núcleo tiene un solo constructor de 2 parámetros.
2. **Sin asesor, Allow**: WLH:144-157 (`Assert.Equal(CloseDecision.Allow, rig.Core.OnBeginDocumentClose(a));`). La destrucción funciona sin asesor.
3. **La decisión es la del asesor, una llamada por intento**: WLH:159-174 (`[InlineData(CloseDecision.Allow)]`/`Veto`; `Assert.Equal(new[] { a, b }, advisor.Advised.ToArray());`).
4. **Sustitución determinista**: WLH:176-194. Tras dos `SetLifecycleAdvisor`, `Assert.Empty(first.Advised); Assert.Empty(first.Destroyed); Assert.Single(second.Advised); Assert.Single(second.Destroyed);`.
5. **El asesor que lanza da Veto en el puente**: WLH:196-221. `Assert.Equal(CloseDecision.Veto, decision);`, `Assert.Contains("OnBeginDocumentClose", fault.Context);`, `Assert.False(rig.Core.IsDegraded(a));`, las conexiones no cambian, y en el siguiente intento se vuelve a preguntar: `advisor.Advise = _ => CloseDecision.Allow; Assert.Equal(CloseDecision.Allow, …)`.
6. **La sesión degradada sigue pasando por el asesor**: WLH:223-237.
7. **Orden destrucción → asesor**: WLH:239-262. Dentro de `SessionDestroyed`, `rig.Core.TryGetSession(session, out _)` es false y `ActiveSession` ya no es la destruida: `Assert.Equal((a, false, (SessionId?)b), seen[0]); Assert.Equal((b, false, (SessionId?)null), seen[1]);`. Vale para la sesión activa y para la inactiva, con una llamada cada una.
8. **Id nativo desconocido: nadie se entera**: WLH:264-278 (`Assert.Empty(advisor.Destroyed);`).
9. **Si el asesor lanza en `SessionDestroyed`, la destrucción no se detiene**: WLH:280-308. Se cumplen `Assert.Single(advisor.Destroyed)`, sesión fuera, activo null, `IsIdleAttached` false y 0 conexiones; el fallo se registra; el id nativo reciclado crea una sesión NUEVA.
10. **Precedente de decorador sobre el host en el puente real**: WLH:310-326 (`HostThenThrowingAdvisor` delega y después lanza). En WLH:328-348, con `env.Bridge.SetLifecycleAdvisor(new HostThenThrowingAdvisor(env.Core));` el host retira igualmente los borradores (`IsRetired(old)`).
11. **Secuencia de cierre**: WLH:364-375 (Allow → destrucción = `(a, true)`); WLH:377-423 (15 entradas intermedias la rompen); 425-434, 436-445, 447-458 (el asesor que lanza en el segundo cierre la rompe); 460-471 (degradada); 473-493 (`SetLifecycleAdvisor(advisor)` y las consultas NO la rompen).
12. **Una sola llamada a `SessionDestroyed` por destrucción en el núcleo**: guarda LGT:218, junto con 3, 7 y 9.
13. **Política de veto del host**: WLH:497-673 (sucio/pendiente → Veto; limpio → Allow; «repetir nunca es consentimiento» en 615; por sesión en 647).
14. **Avisos de veto y de pérdida**: PEH:1112-1136 (uno por sesión, el último); 1138-1153 (si la evaluación falla, el aviso queda incierto, `Faults` tiene `"WorkspaceHostCore.AdviseClose"` y no se degrada); 1155-1169 (veto de un documento no activo); 1171-1217 (expira con el descarte, con el siguiente Allow y con la retirada); 1666-1689 (destruir A quita solo su aviso de veto y encola su pérdida).
15. **Pérdida en la cola y en el registro**: WLH:850-872 (`f.Context == "WorkspaceHostCore.DraftLoss"`); 874-896 (inactiva); 898-916 (sin pérdida no se encola ni se registra nada); 918-944; 946-960; 962-981 (un registro roto no detiene nada).
16. **Riesgo**: PEH:1490-1502 (el `default` es desconocido); 1504-1518; 1520-1536 (sin borradores, conocido y vacío; también para la sesión 777); 1538-1550; 1260-1280 (sesión no activa).
17. **Interacción con el idle**: ISC:1016-1130. Un Allow pendiente hace `CanDrainNow()` false; la destrucción con `AllowingAdvisor` da `Assert.Equal(new[] { (id, true) }, advisor.Ended);` (1092 y 1108).

## 4. Cómo se prueba hoy en Core

- **Puente real**: `WorkspaceBridgeTestSupport.cs:70-116` `WorkspaceBridgeRig`. En 80: `Core = new EventBridgeCore(Source, (context, error) => Faults.Add((context, error)));`, con la fuente simulada `WorkspaceSimulatedHostSource` (15-67). `Open(nativeId)` (93-97) llama a `Core.OnDocumentActivated`. **Hay que llamar antes a `rig.Core.Start()`**, porque `OnDocumentToBeDestroyed` exige `_started` (`EventBridgeCore.cs:270`). `OnBeginDocumentClose` no lo exige.
- **Host real sobre el puente real**: `WorkspaceHostTestSupport.cs:110-199` `WorkspaceHostRig`.
  - En 114-117: `Bridge = new WorkspaceBridgeRig(); Link = new WorkspaceSimulatedBridgeLink(Bridge, Probe); … Host = new WorkspaceHostCore(Link, Probe, Reader, Instrumentation, (context, error) => Faults.Add((context, error)));`.
  - `WithDocument` (137-141), `Activate` (144-149), `CloseActive` (151-155), `Idle` (183-193).
  - `Host.OpenPanel()` → `Link.Start()` (75-80) → `Rig.Core.Start()`.
- **Entorno de ciclo de vida**: WLH:48-126.
  - `Env(bool wire = true, …)` hace `if (wire) Bridge.SetLifecycleAdvisor(Core);` (60).
  - 75: `Close(...) => Bridge.OnBeginDocumentClose(session ?? Session)`. 77: `Destroy(long nativeId = 100) => Bridge.OnDocumentToBeDestroyed(nativeId)`.
  - `Seed`/`Pilot`/`MakeDirty` crean el riesgo.
  - Hay un equivalente en PEH:67-130, con `Port => Core`, `Type`, `Open` y `OpenUndecidable`.
- **¿Se puede montar un `EventBridgeCore` real con un asesor envolvente y disparar `BeginDocumentClose` y `ToBeDestroyed`?** Sí. El patrón ya existe en WLH:331-336: `var env = new Env(wire: false); … env.Bridge.SetLifecycleAdvisor(new HostThenThrowingAdvisor(env.Core)); … env.Destroy();`. Otros ejemplos: WLH:965-973 (un host propio sobre `hr.Link`/`hr.Probe` y `hr.Bridge.Core.SetLifecycleAdvisor(core)`) y el arnés de idle ISC:23 (`WorkspaceIdleHarness`), con `CloseAllowed` (163) y `Destroy` (157).
- Los dobles `LifecycleScriptedAdvisor` (WLH:13-34, que guarda `Advised`/`Destroyed` y es programable) y `AllowingAdvisor` (ISC:1018-1025) sirven como `inner` para probar el envoltorio aislado.
- Límites:
  - El relevo `OnActiveSessionChanged`, `AutoCadEventBridge` y el controlador son del Plugin. `tests/RackCad.Tests/RackCad.Tests.csproj:23-31` solo referencia Domain, Application y tools.
  - Por eso «vaciar los pendientes en el siguiente relevo con documento activo» tiene que ser un método de Application que Core pueda llamar directamente, con un escritor inyectado (por ejemplo un delegado). El controlador solo lo invocaría, y eso se cubre con guardas de texto.
  - Application ve sus `internal` desde las pruebas (`RackCad.Application.csproj:11`).

## 5. API de AutoCAD que necesita el adaptador y lo que la prohíbe

- **API necesaria**:
  - `AcApplication.DocumentManager`: enumerable, ya usado en `AutoCadEventBridge.cs:337` y `WorkspaceController.cs:226,310,321`.
  - `Document.UnmanagedObject.ToInt64()`: la identidad nativa, como en `AutoCadEventBridge.cs:333` y `WorkspaceController.cs:311`. Sirve para emparejar `WorkspaceSession.NativeInstanceId`, que se obtiene con `_bridge.TryGetSession` (`AutoCadEventBridge.cs:98-103`).
  - `Document.Name`: el repositorio no la usa en ningún sitio (`git grep` sin resultados). Es una propiedad de AutoCAD que devuelve el nombre o la ruta del documento; no está verificada aquí.
  - `DocumentManager.MdiActiveDocument`, `Document.Editor` y `Editor.WriteMessage(string)`, ya usados en `WorkspacePanelCommand.cs:21-22` y `RackCommandSupport.cs:30-31`.
  - Para escribir `foreach (Document d in AcApplication.DocumentManager)` el controlador necesitaría `using Autodesk.AutoCAD.ApplicationServices;` o el nombre calificado. La enumeración no es genérica: con `var` se obtiene `object`.
- **Lo que lo prohíbe en el controlador o fuera del archivo del comando**:
  - `WriteMessage`: HSG:84-89. Solo está permitido en `WorkspacePanelCommand.cs`.
  - Campos o propiedades de tipo `Document`/`Editor`/`Database`/…: HSG:460-471 y BSG:205-243.
  - Tipos de AutoCAD en líneas `public`/`protected` de Plugin/Workspace: BSG:216-252.
  - Diálogos y prompts (`MessageBox`, `ShowDialog`, `GetString`…): BSG:139-175 y PEG:639-646.
  - Escrituras (`LockDocument`, `StartTransaction`, `SendStringToExecute`, `SetSystemVariable`, `Save`…): LGT:616-632, WorkspaceHostC2SourceGuardTests.cs:247-261 y BSG:290-302.
  - Planificación (`Dispatcher`, `Timer`, `Task`, `Thread`, `async`): PHG:488-498, PEG:510, HSG:392-437 y WorkspaceNavigationHostTests.cs:439-453.
  - En Application/Workspace queda prohibido todo `Autodesk.` (WorkspaceModelBoundaryTests.cs:26-37, BSG:406-417), y también `System.IO` (`Path.GetFileName`): HSG:451-458 y WorkspaceModelBoundaryTests.cs:41-49. Acortar el nombre exigiría operaciones de cadena.
  - `DocumentManager`, `MdiActiveDocument`, `Editor` y `WriteMessage` están prohibidos **solo** en el host de la paleta (PHG:250-258), no en el controlador ni en el comando.
- **Avisos de ejecución** (hechos, no propuestas):
  - El relevo por destrucción llega dentro de `DocumentToBeDestroyed` (`AutoCadEventBridge.cs:168-173`), cuando `MdiActiveDocument` todavía puede ser el documento que se está destruyendo o ser null.
  - El relevo por la pestaña de inicio llega con `e.Document == null` (158-160).
  - `default(CloseDecision)` es `Allow` (`BridgeOutcomes.cs:8-12`), así que un envoltorio que devuelva `default` en caso de error convertiría un fallo en «limpio». El núcleo, en cambio, inicializa con `var decision = CloseDecision.Veto;` (`EventBridgeCore.cs:363`).

## Guardas que cambian

1. **LGT:277-288, `TheControllerWiresTheHostAsTheAdvisorOnceInItsConstructorAfterCreatingTheHostWithNoNewSubscription` (el M-1 de F4-T2B2B).**
   - **Por qué rompe:** la línea 284, `AssertInOrder(constructor, "_host = new WorkspaceHostCore(", "_bridge.SetLifecycleAdvisor(_host);");`, exige el literal `_host` como argumento, y con eso hace imposible el envoltorio que permiten INV-F4T2B2B-11 y D-T2B2-10. La desviación quedó registrada en `owner-f4t2b2b-close-decision.md:8,21` y `f4-dispositions.md:825,836`.
   - **Cambio declarado mínimo (orientado a invariante):**
     - Se conservan 283 (`.SetLifecycleAdvisor(` = 1 en el controlador), 285 (`+=` = 3), 286 (`-=` = 0) y 287 (sin `.Start(` en el constructor).
     - La 284 se sustituye por esto: la única llamada está dentro del cuerpo del constructor, después de `_host = new WorkspaceHostCore(`, y su argumento es `_host` o un envoltorio construido sobre `_host` (sin un segundo `new WorkspaceHostCore(`).
     - Se añade una guarda de **comportamiento** en Core, en un archivo nuevo para no tocar el «45» de LGT:797 y PHG:659. Usa un `EventBridgeCore` real, `WorkspaceHostRig` y el envoltorio, y fija:
       - la misma decisión que el host para Veto y Allow (WLH:159-174, 497-673);
       - que, si `inner` lanza, el resultado es Veto y la sesión no se degrada (WLH:196-221);
       - una sola llamada a `inner` por intento y por destrucción (WLH:173, 192-193);
       - que `closeSequenceIntact` se reenvía tal cual (WLH:364-375, 473-493);
       - que el envoltorio se entera después de que la sesión sale del registro (WLH:239-262);
       - que el riesgo se calcula antes de delegar `SessionDestroyed`;
       - que un `PilotRiskView` con `IsKnown` false nunca se traduce en «nada en riesgo».

Todo lo demás de §2(a)-(d) se mantiene si se respetan las condiciones de nombre y de texto señaladas: sin `+=` nuevo, sin `event`, sin `_bridge.` ni `DrainNotice` en el cuerpo del relevo, sin `WriteMessage` fuera del comando, sin `RackLog` en Application, sin `Provider`/`Log`/`Draft`/`Editor`/`Tab` en los nombres, sin `Take(Close|Loss)Notices` en el archivo nuevo y sin archivos nuevos en el Plugin.
