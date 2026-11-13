# Mapa 2: envoltorio del asesor y controlador (explorador, HEAD `7895f397`)

## Condicionantes del diseño

- Un `WriteMessage` en `WorkspaceController.cs` rompe `WorkspaceHostSourceGuardTests.cs:84-89`, y R-C3 no lo declara: R-C3 solo declara `WriteMessage` en el archivo del comando.
- Dentro de `SessionDestroyed`, la sesión ya no está en el registro, así que `TryGetSession` da false.
- El relevo `OnActiveSessionChanged` se dispara en la activación **y** en la destrucción del documento activo.

## 1. La interfaz y el flujo

- **Interfaz:** `IWorkspaceLifecycleAdvisor.cs:9-22`, con `CloseDecision AdviseClose(SessionId)` y `void SessionDestroyed(SessionId, bool closeSequenceIntact)`. «The advisor must not call the bridge back» (19).
- **Implementaciones:**
  - en el producto, solo `WorkspaceHostCore` (38, 912 y 947), de forma explícita;
  - en las pruebas, `LifecycleScriptedAdvisor` (`WorkspaceLifecycleHostTests.cs:13`) y `AllowingAdvisor` (`WorkspaceIdleSchedulerTests.cs:1018`).
- **`EventBridgeCore`:**
  - `_advisor` en 57; `SetLifecycleAdvisor` (164-165) lanza con null y una segunda llamada lo reemplaza;
  - `OnBeginDocumentClose` (357-375): sin asesor, Allow; si el asesor lanza, Veto con `Log`; con Allow guarda `_lastAllowedClose`;
  - `OnDocumentToBeDestroyed` (268-296): calcula `sequenceIntact`, desengancha, hace `_registry.Destroy(id)` y anula el activo; **después** llama a `TellAdvisorTheSessionEnded` (446-458, con `try` y `Log`);
  - `Destroy` quita la sesión de `_live` (`WorkspaceSessionRegistry.cs:30-36`), así que `TryGetSession` (103, 105-109) da false dentro de `SessionDestroyed`.
- **`AutoCadEventBridge`:**
  - 76 `SetLifecycleAdvisor`; la fachada `TryGetSession`, en 98-103;
  - 180: `if (_core.OnBeginDocumentClose(a.Session) == CloseDecision.Veto) e.Veto();`;
  - `DocumentActivated` (151-164): llama a `OnNoActiveDocument` o a `OnDocumentActivated` y después a `ActiveSessionChanged?.Invoke()`;
  - `DocumentToBeDestroyed` (165-176) también dispara `ActiveSessionChanged` si cambió el activo;
  - `FindDocumentOrNull` (335-343, privado) recorre `DocumentManager` por `UnmanagedObject.ToInt64()`.

## 2. `WorkspaceController.cs`

- **Constructor (35-57):** `_host = new WorkspaceHostCore(new BridgeLink(_bridge), new HostProbe(_bridge), …, LogFault, _echo)` (39-45), luego `_bridge.SetLifecycleAdvisor(_host);` (53) y tres `+=` (54-56).
- **Clases anidadas:** `PilotEditAdapter` (234-268), `BridgeLink` (271-294) y `HostProbe` (297-325).
- `_pilotEdit` (49) es el adaptador sobre el gate. El gate delega `GetRacksAtRisk` sin mirar la profundidad (`WorkspacePilotSupport.cs:190`).
- El host recibe `GetRacksAtRisk(WorkspaceRequest)`, no un `SessionId` (765; `SessionId.cs:22`).
- 67 `GetOrCreate`; 70 `IsCreated`.
- **El relevo `OnActiveSessionChanged`** (159-171): `try { if (PanelVisible) _host.RefreshPanel(); } catch { LogFault }` y después `NotifyStateChanged()`.
- **Registro:** `RackLog` solo aparece en `LogFault` (231). El host ya registra la pérdida con `Report("WorkspaceHostCore.DraftLoss")` (965, 1053-1057), que pasa por `LogFault`.
- **`DocumentManager`** ya se usa en 226, 310 y 321.
- **`WriteMessage`:** el controlador no tiene ninguno; hoy solo está en el comando.

## 3. Guardas que fija el controlador y qué rompería un envoltorio anidado que escribe

| Guarda | Qué fija | Efecto |
|---|---|---|
| `WorkspaceLifecycleGuardTests.cs:278-288` | `.SetLifecycleAdvisor(` = 1; el orden ctor `_host = new WorkspaceHostCore(` → `_bridge.SetLifecycleAdvisor(_host);`; `+=` = 3; `-=` = 0 | `SetLifecycleAdvisor(new Envoltorio…)` **rompe**: es M-1. El arrastre fija solo el orden `_host = new WorkspaceHostCore(` → `_bridge.SetLifecycleAdvisor(` y su conteo (`f4-dispositions.md:866`) |
| ídem, 233-246 y 248-263 | el reenvío literal del puente; 16 `+=`/`-=`/manejadores; `Veto()` = 1 | — |
| ídem, 605-612 | `OnDrained` del controlador no nombra `AdviseClose` ni `SessionDestroyed` | — |
| ídem, 620-628 | las API de escritura en el controlador | — |
| ídem, 791-798 | 9 archivos de Plugin/Workspace; 45 `[Fact\|Theory]` en `WorkspaceHostSourceGuardTests` | añadir o quitar una prueba allí **rompe** |
| **`WorkspaceHostSourceGuardTests.cs:84-89`** | `\bWriteMessage\b` prohibido fuera de `WorkspacePanelCommand.cs` | **rompe** si el envoltorio escribe en el controlador |
| ídem, 185-196, 461-471, 489-493, 503-514, 393-415 y 418-437 | `+=` = 3; sin campos `Document` ni `Editor`; sin `RackLog.Warning`; `RackLog.` solo en `LogFault`; sin Dispatcher, System.Windows, Timer, Thread ni async | registrar fuera de `LogFault` rompe |
| `WorkspacePaletteHostSourceGuardTests.cs:407-422, 424-432, 434-446, 486-499 y 763-777` | `+=` = 3; `\bevent\b` = 1; el relevo sin `_bridge.` ni `RequestReconciliation`, y anuncia después del refresco; sin Task, Timer, Thread, async, Dispatcher ni UnhandledException; 9 archivos exactos | un `texto +=`, o la palabra «event» o «Task» en un literal, también cuenta: `CodeOnly` solo quita comentarios |
| `WorkspacePilotEditGuardTests.cs:460-510 y 537-572` | 3 `+=`; 1 `event`; un solo `public IWorkspacePilotEditPort`; ningún `public WorkspaceHostCore`; `_host.SetModalRackCadActive(` = 2 | — |
| `WorkspaceIdleSchedulingGuardTests.cs:507-517`; `WorkspaceHostC2SourceGuardTests.cs:129-151 y 254-262`; `WorkspaceNavigationHostTests.cs:440-450 y 667-677`; `WorkspaceBrowserHostGuardTests.cs:140-151 y 197-202` | `+=` = 3; `OnCommandStateChanged` literal; persistencia; `SendStringToExecute`, `StartCommand` y `SetFocus`; el relevo no nombra `DrainNotice` | — |
| `WorkspaceBridgeSourceGuardTests.cs:102-123, 139-175 y 205-252` (aplicadas a los archivos reales en `WorkspaceSourceGuardExceptionTests.cs:106-117`; 52-61 prohíbe RACKPANEL en el controlador) | solo el puente engancha eventos de AutoCAD; prohíbe `GetKeywords`, `GetString`, `MessageBox`, `Show*Dialog` y `GetObject`; sin campos `Document`; ninguna línea `public` o `protected` con `Document` o `Editor` | `WriteMessage` no está en esa lista. El controlador no puede suscribirse a `DocumentActivated` |
| `WorkspacePilotCloseSyncGuardTests.cs:153-167` | `Take(Close\|Loss)Notices` solo en 4 archivos, el controlador entre ellos | — |
| `WorkspaceBridgeFacadeSourceGuardTests.cs:59-66, 91-104 y 206-228` | la fachada `TryGetSession`; la superficie pública exacta del puente y del núcleo | — |

## 4. `WorkspaceHostCore`

- **`AdviseClose` (912-934):** `_knownClean.Remove`, y después:
  - Veto: `QueueCloseNotice(session, decision.RackIds, false)`;
  - Allow: `DropCloseNotice` y `_knownClean.Add`;
  - si lanza: `Report`, `QueueCloseNotice(…, true)` y Veto.
- **`QueueCloseNotice` (936-940):** quita y añade, así que hay un aviso por sesión.
- **`DropCloseNotice`:** en 942, y llamado también en 734, 754 y 950.
- **`SessionDestroyed` (947-971):**
  - fuera de `try`: `_knownClean.Remove` y `DropCloseNotice`;
  - primer `try`: la retirada (`RetireKnownSafe` o `RetireConservative`);
  - segundo `try`: la pérdida (`_lossNotices.Enqueue` y `Report`).
  - **Después de la retirada, `GetRacksAtRisk` da vacío**: por eso el envoltorio lo calcula ANTES de delegar.

## 5. Qué se puede probar sin AutoCAD

- **En Core** (`WorkspaceHostRig`, `Env`): el asesor que lanza da Veto; el orden destrucción → asesor; `GetRacksAtRisk` antes y después; las colas.
- **No se puede probar en Core:** el texto del aviso, la resolución con `DocumentManager`, `WriteMessage` y el relevo. Quedan en guardas de texto y en Smoke-2 (R-C4).
- **Implicación:** un núcleo de Application (por ejemplo, un asesor envolvente puro que reciba un «emisor de mensajes» abstracto y un «resolvedor de nombre» abstracto) sería ejecutable en Core. El controlador lo conectaría con un adaptador de escritura. Pero `WriteMessage` en el controlador necesita un cambio declarado de la guarda 84-89, o hay que poner el emisor en otro archivo del Plugin, lo que cambia el censo de 9 archivos.
