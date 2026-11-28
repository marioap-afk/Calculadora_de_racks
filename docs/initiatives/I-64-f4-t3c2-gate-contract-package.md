# I-64 — Paquete del contrato de F4-T3C2-COMMAND-ADVISOR

**Estado:** v3. Lo prepara la sesión principal el 2026-11-13.
- La v1 pasó por una revisión adversarial independiente: EMITIBLE TRAS CORRECCIONES (0/2/11). La v2 aplicó sus 13 hallazgos (§8).
- La re-revisión de la v2 dio EMITIBLE TRAS CORRECCIONES (0/0/11), con los 13 de la v1 resueltos. La v3 aplica sus 11 menores (§9).
- **Código base:** `7bd275e4`, el producto de F4-T3C1. HEAD es `e89948b9`, la custodia de su cierre; entre las dos no cambia nada en `src/` ni en `tests/`.
- **Autorización:** la del Owner y el Coordinator, `F4-DRAFTS/coordinator/owner-f4t3c2-authorization.md`. Ratifica el cierre de F4-T3C1, autoriza F4-T3C2 con `attempts` 1 de 3 y fija la cadena hasta el cierre sin más aprobación del Owner para la ejecución ordinaria.
- **Mapas previos:** tres exploraciones de solo lectura del código real (`map-1-command-census.md`, `map-2-advisor-controller.md` y `map-3-reasons-notice-minors.md`), que se custodian con el contrato.

Este paquete fija una subtarea; no declara GATE PASS.

## 1. Autoridades

- **El paquete de F4-T3C** (`docs/initiatives/I-64-f4-t3c-gate-contract-package.md`, v3), §6: las reglas vinculantes R-C5 a R-C14 de esta subtarea.
- **El paquete de F4-T3** (`docs/initiatives/I-64-f4-t3-gate-contract-package.md`): §8, R-C1 a R-C4 (la semántica aceptada del comando y del envoltorio), D-T3-07 y D-T3-08.
- **La autorización del Owner** (arriba). Sus requisitos se recogen sin cambio de sentido en el §3 y en los invariantes del §5.
- **Las órdenes anteriores del Owner** en `F4-DRAFTS/coordinator/`:
  - `owner-f4t3b1-close-disposition.md` (F4-E43): F4-T3C es dueña del comando, del aviso con la vía del comando, del endurecimiento de `GetRacksAtRisk`, de lo modal y de foco restante y de los menores; la regla de la replanificación reforzada única;
  - `owner-f4t3c-decomposition-decision.md` (F4-E47): F4-T3C en dos subtareas, C1 y C2;
  - `owner-f4t3c1-close-decision.md` (F4-E48): R-1, R-2 y R-4 de F4-T3C1 pasan a T3C2; R-3 a F4-T4;
  - `owner-f4t3-authorization.md`: el oráculo crítico de cero escrituras authored y sin Actualizar;
  - `owner-f4t2b2b-close-decision.md`: M-1 de F4-T2B2B (la guarda del asesor) y el envoltorio permitido por INV-F4T2B2B-11 y D-T2B2-10. La disposición F4-E35 (`f4-dispositions.md`) fija las dos mitades de M-1: la del controlador y la del reenvío del puente.
- **Decisiones del Owner sobre el comando** (`f4-dispositions.md`):
  - **C-F4-03:** «Ante un destino ambiguo falla con un mensaje claro y sin efecto».
  - **D-17:** el aviso encolado del panel y el descarte del documento entero, por la lista de racks del documento, como el botón del panel (R-B8, R-C1).
- **Cierres que no deben romperse:** F3, F4-T1, F4-T2, F4-T3A, F4-T3B1, F4-T3B2 y F4-T3C1 (`F4-T3C1-HOST-HARDENING/task-close-package.md`, con su §5).

## 2. Hechos medidos en `e89948b9` (código de `7bd275e4`)

### 2.1 El comando, el controlador y el puerto

| Id | Hecho |
|---|---|
| H-01 | `WorkspacePanelCommand.cs` (30 líneas) tiene un solo `[CommandMethod("RACKPANEL")]` (14), cuyo cuerpo llama a `WorkspaceController.GetOrCreate().OpenPanel()` (19) y `WorkspacePaletteHost.GetOrCreate().Show()` (20), y escribe con `MdiActiveDocument?.Editor.WriteMessage` (21-22). El `catch (System.Exception ex)` llama a `RackCommandSupport.Report(ex)` (24-26) |
| H-02 | `WorkspaceController` solo expone `GetOrCreate()` (67) e `IsCreated` (70). No hay ningún acceso a la instancia ya creada sin crearla. `WorkspaceLazyInstance<T>` (`Application/Workspace/WorkspaceLazyInstance.cs`, 54 líneas, `#nullable enable`) tiene `IsCreated`, `CreateCount` y `GetOrCreate()`, sin `TryGet`. Su comportamiento se prueba en `WorkspaceHostPolicyTests.cs:152-246`; no hay censo de su superficie |
| H-03 | La sesión activa solo sale del controlador por `Port.GetSnapshot().ActiveSession` (host 223-241: requiere el puente arrancado y la sesión viva). El puerto del piloto sale por `PilotEdit` (110-117), cuyo `_ = Navigation;` crea el host de navegación si no existe; no se suscribe a nada |
| H-04 | `DiscardDocument` (host 746-762) pasa por `Authorize` (Rejected si no es el documento activo) y da Busy con una lectura en curso, Discarded o Failed; con Discarded quita el aviso de veto de la sesión. `GetRacksAtRisk` (764-807) es memoria sin `Authorize`. Un comparador que falla lleva a la vía de reserva borrador a borrador, que da un valor **conocido** con inciertos (779-800); solo el `catch` externo, hoy inalcanzable, da `default` (el desconocido). La compuerta (`WorkspacePilotSupport.cs:188-190`) pasa las dos operaciones sin filtrar |
| H-05 | `EditDiscardOutcome` (`WorkspaceEditIntent.cs:294-333`): `Status` (Rejected, Discarded, Failed, Busy), `Reason`, `RemovedRackIds` y `PendingTextRackIds` |
| H-06 | Patrón Sí/No del Plugin (`RackLayoutCommands.cs:149-155`): `Keywords.Add("Si")`, `Keywords.Add("No")`, `Keywords.Default = "No"`, `AllowNone = true` y `Status == PromptStatus.OK && StringResult == "Si"`. La palabra clave va sin tilde |

### 2.2 El asesor, el puente y el controlador

| Id | Hecho |
|---|---|
| H-07 | `IWorkspaceLifecycleAdvisor` (9-22): `AdviseClose(SessionId)` y `SessionDestroyed(SessionId, bool closeSequenceIntact)`; «The advisor must not call the bridge back» (en `SessionDestroyed`) |
| H-08 | `EventBridgeCore.OnBeginDocumentClose` (357-375) parte de `var decision = CloseDecision.Veto;`, toma el Allow solo si el asesor lo da, y una excepción del asesor es Veto con `Log`. **`default(CloseDecision)` es Allow** (`BridgeOutcomes.cs:8-12`) |
| H-09 | `OnDocumentToBeDestroyed` (268-296) quita la sesión del registro y del activo y **después** llama a `TellAdvisorTheSessionEnded` (446-458, con `try` y `Log`). Dentro de `SessionDestroyed`, `TryGetSession` da false. Hay una sola llamada a cada método del asesor, y cada una lee el campo una vez |
| H-10 | `SetLifecycleAdvisor` (164-165) lanza con null; una segunda llamada reemplaza. No hay desinstalación, y `Stop()` no toca el asesor. El controlador no se libera: vive hasta que AutoCAD termina |
| H-11 | El controlador instala el host en su constructor (53, `_bridge.SetLifecycleAdvisor(_host);`) y tiene exactamente 3 `+=` (54-56), 0 `-=` y un `event` (64). El relevo `OnActiveSessionChanged` (157-171) refresca el panel si está visible, registra un fallo con `LogFault` y llama a `NotifyStateChanged()`. `ActiveSessionChanged` se dispara en `DocumentActivated` (también con la pestaña de inicio, `e.Document == null`) y en `DocumentToBeDestroyed` si el activo cambió; en este segundo caso, `_bridge.ActiveSession` ya es null |
| H-12 | No hay nada público que dé el nombre de un documento por sesión. `WorkspaceSession` no lleva nombre. `AutoCadEventBridge.FindDocumentOrNull` (335-343) es privado y recorre `DocumentManager` por `UnmanagedObject.ToInt64()`, y la superficie pública del puente está fijada en 9 nombres |
| H-13 | `GetRacksAtRisk` después de la retirada da vacío **conocido** (`DraftRegistry.RacksAtRiskOf`, 192-195). Por eso la pérdida se calcula antes de delegar `SessionDestroyed`. `RetireKnownSafe` no llama al comparador, así que en el residual conocido la pérdida que ve un envoltorio puede no coincidir con el `DraftLossNotice` del host |
| H-14 | El host ya **registra** la pérdida (`Report("WorkspaceHostCore.DraftLoss")`, 986), también con QUIT, y la encola para el panel |

### 2.3 Los motivos del Plugin que llegan al campo (R-C14)

| Id | Hecho |
|---|---|
| H-15 | Llegan al campo como «En espera: …», «Rechazado: …» o «Fallo: …» (`WorkspacePilotFieldViewModel.Describe`, 447-459) por `TryBeginEdit` y `TryReload`; el host usa `read.Reason` cuando no es null, y el lector del Plugin nunca lo da null. «Comprobar ahora» no tiene llamador en la UI |
| H-16 | Son **10 literales** (tabla T-3 del §3): `WorkspaceNavigationHost.cs` 224, 226 y 236 (`ReadOnActiveDocument`) y `Views/RackPilotIntentAdapter.cs` 80 y 85 (`LoadCatalog`) y 112, 118, 127, 131 y 135 (`Read`). 85, 131 y 135 son prefijos seguidos de `ex.Message`. **R-C14 se corrige:** la «251» que cita es el conducto `EditIntentReadResult.Failed(read.FailureReason)`, no un literal; y `ReadPreviewWitness` tiene 4 literales sin llamadores (157, 174, 178 y 182), no 2 |
| H-17 | **Ninguna prueba ni guarda fija esos literales en inglés.** Las guardas fijan la forma (`WorkspaceEditIntentGuardTests.cs:109, 354, 367 y 416-419`), que un literal en su sitio conserva. Ninguno va al registro |
| H-18 | **Copias en inglés con el mismo texto que no son del campo**, y no cambian: F3 en `WorkspaceNavigationHost.cs` 114, 116, 118, 122, 158 y 160; `RackPilotIntentAdapter.cs` 174 (igual que 127). Una guarda de tabla tiene que medirse sobre los cuerpos de `ReadOnActiveDocument`, `Read` y `LoadCatalog` |
| H-19 | `RackPilotIntentAdapter.cs` no admite `static readonly` (`WorkspaceEditIntentGuardTests.cs:152`) ni una tercera clase en el archivo del host de navegación (436); los literales se sustituyen en su sitio |

### 2.4 El aviso de veto del panel y los menores de F4-T3C1

| Id | Hecho |
|---|---|
| H-20 | `WorkspaceBrowserViewModel.RowOf` (1129-1142): el veto de la sesión presentada dice la cabeza (1137-1139) + « Descártalos con "Descartar borradores de este documento"; cada pestaña también se descarta con su "Cerrar".» (1140). El de otra sesión (1135): «No se cerró otro documento con borradores sin aplicar; actívalo para descartarlos.». **Solo nombran la vía del panel** |
| H-21 | Pruebas que fijan esos textos (`WorkspacePilotCloseSyncTests.cs`): 780-782 (`Equal` exacto) y **785 (`DoesNotContain("RACKPANELDESCARTAR")`)** en el mismo método, y 805 (`Equal` exacto del de otra sesión). Las demás usan `Contains` o `StartsWith` sobre la cabeza (797, 813, 827, 936, 941, 975; vista 365). Ninguna guarda prohíbe nombrar un comando en la UI; solo lanzarlo |
| H-22 | Cada fila de aviso tiene «Entendido» (`WorkspacePanelView.xaml`, botón de la fila) |
| H-23 | R-1: `WhyOf` (937-948) sin prueba que discrimine Busy; las dos pruebas con `Busy` usan `Contains` (251-266 y 310-345). R-2: la prueba de `_confirmUnread` (1520-1541) pasa por `CancelDiscard` antes de la segunda petición. R-4: la documentación de `TakeLossNotices` y `TakeCloseNotices` (`IWorkspacePilotEditPort.cs:72 y 75`, «leaves the queue empty») y la de `RequestDocumentDiscard` y `ConfirmDiscard` (VM 352-356 y 402-407) no reflejan el `try` ni el desconocido. Ninguna guarda fija esas frases |

### 2.5 Trampas de guardas (medidas)

| Id | Guarda | Efecto |
|---|---|---|
| G-01 | `WorkspaceHostSourceGuardTests.cs:49-56` (54) y `WorkspacePaletteHostSourceGuardTests.cs:320-330` (325) | `Equal(1, [CommandMethod)` en el archivo del comando: **cambia** |
| G-02 | `WorkspaceBridgeSourceGuardTests.cs:138-175` | `GetKeywords` está en `ForbiddenCallNames` y `HandlerForbiddenCallsFor` (159-163) no exceptúa el comando: **cambia** |
| G-03 | `WorkspaceBridgeSourceGuardTests.cs:320-351` (`DeclaresEntryPoint`) | `attributes != 1 \|\| allowed != 1` en el archivo del comando: **cambia** |
| G-04 | `WorkspaceSourceGuardExceptionTests.cs:106-117` | aplica G-02 y G-03 a los archivos reales; pasa sin cambio de texto cuando se refinan |
| G-05 | `WorkspaceSourceGuardExceptionTests.cs:250-258` (257) | exactamente 2 `string.Equals(path, (Reader\|Command)AdapterPath, StringComparison.Ordinal)` en la guarda del puente: **cambia** con la excepción nueva |
| G-06 | `CustomPropertiesCommandGuardTests.cs:55, 58 y 71-75` | censo por nombre: un comando sin clasificar falla. **Cambia** |
| G-07 | `SelectiveEditorOpenTests.cs:559-567` | censo bruto de `[CommandMethod(` en `src/RackCad.Plugin`: 39. **Cambia** a 40. Ningún comentario ni literal del Plugin puede contener esa subcadena |
| G-08 | `WorkspaceLifecycleGuardTests.cs:277-288` (284) | el literal `"_bridge.SetLifecycleAdvisor(_host);"` (M-1 de F4-T2B2B): **cambia** a una guarda de invariante (D-T3C2-08) |
| G-09 | `WorkspacePaletteHostSourceGuardTests.cs:659` y `WorkspaceLifecycleGuardTests.cs:797` | exactamente 45 pruebas en `WorkspaceHostSourceGuardTests.cs`: las guardas nuevas van en un archivo nuevo, y los cambios de ese archivo modifican pruebas existentes sin añadir ni quitar |
| G-10 | 5 censos de 9 archivos de `Plugin/Workspace` (`WorkspacePaletteHostSourceGuardTests.cs:767-776`, `WorkspaceLifecycleGuardTests.cs:794`, `WorkspaceBrowserHostGuardTests.cs:38`, `WorkspaceEditIntentGuardTests.cs:808`, `WorkspaceNavigationHostTests.cs:686`) | **no se añade ningún archivo al Plugin** |
| G-11 | 8 guardas de `+=` = 3, 7 de `-=` = 0 y 2 de `\bevent\b` = 1 en el controlador | ningún `+=` nuevo, ni `texto +=`; los textos se componen con `a = a + b` |
| G-12 | `WorkspaceHostSourceGuardTests.cs:84-89` | `WriteMessage` solo en el archivo del comando |
| G-13 | `WorkspaceHostSourceGuardTests.cs:460-471` y `WorkspaceBridgeSourceGuardTests.cs:205-252` | ningún campo o propiedad de tipo `Editor` o `Document`, ni línea `public`/`protected` que los nombre; un método `private` con un parámetro `Editor` o una variable local sí caben |
| G-14 | `WorkspacePaletteHostSourceGuardTests.cs:434-448` | el cuerpo del relevo sin `_bridge.` ni `RequestReconciliation`, y `NotifyStateChanged()` después de la guarda del panel; `WorkspaceBrowserHostGuardTests.cs:200-201`, sin `DrainNotice` |
| G-15 | Application/Workspace: `WorkspaceModelBoundaryTests.cs:26-49 y 80-105`, `WorkspaceBrowserBoundaryTests.cs:83-100 y 284-294`, `WorkspaceDraftBoundaryTests.cs:420-452`, `WorkspaceHostSourceGuardTests.cs:376-458, 495-500, 524-529 y 545-552`, `WorkspaceBridgeSourceGuardTests.cs:362-372` | sin `Autodesk.`, `System.IO`, `RackLog`, `Transaction`, `Database`, `LockDocument`, `Commit(`; nombres de tipo sin `Draft`, `Editor`, `Subscription`, `Tab` ni `Log`, y distintos de las palabras de los archivos `Browser*` y de `Drafts/`; sin `Metric`, `Provider`, `Aggregat`, `\bBom` (también en los literales) |
| G-16 | `WorkspacePilotCloseSyncGuardTests.cs:175-197` | `Take(Close\|Loss)Notices` solo en 4 archivos: los archivos nuevos no los nombran |
| G-17 | `WorkspacePilotEditGuardTests.cs:463-511` | un solo `public IWorkspacePilotEditPort` en el controlador; ningún `public … WorkspaceHostCore`; el literal del adaptador del piloto (487) |
| G-18 | `WorkspaceHostHardeningGuardTests.cs:395-473` (`LiteralViolations`) | cuenta los textos de H-07 en el host y el soporte: no se tocan esos dos archivos |
| G-19 | `WorkspaceSourceGuardExceptionTests.cs:30-61 y 260-281` | autopruebas de `DeclaresEntryPoint`; los 12 nombres de prueba de la guarda del puente se conservan |
| G-20 | `WorkspaceHostSourceGuardTests.cs:93-126` | un solo `new WorkspaceController(`; prohibidos `static readonly WorkspaceController` y un campo estático que la cachee; `Existing => Holder.Existing` cabe |
| G-21 | La build Release de la solución, con las pruebas, usa `-NoWarnings` (`eng/ci/verify-autocad-references.ps1:110-113 y 601`) y `Directory.Build.props` desactiva `Nullable` | **los cuatro archivos nuevos** (dos de Application y dos de pruebas) empiezan por `#nullable enable` (usan `?`; sin él, CS8632). **En el controlador y en el comando**, que no tienen `#nullable`, no hay anotaciones `?` de tipo referencia; `?.` y `SessionId?` sí caben. Las pruebas «sin motivo» no pasan null a `EditDiscardOutcome.Busy(string)` (CS8625): usan `reason!` (precedente `WorkspacePilotFieldViewModelTests.cs:1400`) o `" "` |
| G-22 | `WorkspaceHostSourceGuardTests.cs:435-436` | `\.Result\b` prohibido en todo el Plugin: la respuesta de `GetKeywords` se lee con `.Status` y `.StringResult`, y ninguna variable ni miembro se llama `Result` |
| G-23 | `WorkspacePilotEditGuardTests.cs:782-788` (texto con comentarios de `IWorkspacePilotEditPort.cs`) | la documentación nueva de R-4 no contiene `Transaction`, `\bDatabase\b`, `\bCommit\s*\(` ni las palabras de I-63 (sin distinguir mayúsculas) |
| G-24 | `WorkspaceLifecycleGuardTests.cs:60-73` | fija la interfaz del asesor por sus dos firmas; un cambio solo de documentación la conserva |

## 3. Decisiones de F4-T3C2-COMMAND-ADVISOR

### D-T3C2-01. El acceso sin crear (R-C6)

- `WorkspaceLazyInstance<T>` gana `public T? Existing`, con el mismo `lock`: la instancia si ya se creó, o null. No llama a la fábrica ni cambia `CreateCount`.
- `WorkspaceController` gana `public static WorkspaceController Existing => Holder.Existing;`, documentado: «la instancia ya creada o null; nunca crea».
- Sustituye el «usa `IsCreated`» de R-C1, como cambio declarado (R-C6).

### D-T3C2-02. El núcleo del comando, en Application (R-C5, R-C1, R-C7)

Una clase nueva, `src/RackCad.Application/Workspace/WorkspaceDocumentDiscardCommand.cs`: `public static class WorkspaceDocumentDiscardCommand` con

`public static void Run(IWorkspacePilotEditPort? port, Func<SessionId?> activeSession, Func<SessionId, bool> isTarget, Func<string, bool> confirm, Action<string> write)`.

El archivo empieza por `#nullable enable` (G-21).

**El destino** (C-F4-03): es la sesión activa del puente, siempre que autorice contra el documento que ejecuta el comando. `isTarget` es esa comprobación; en el Plugin, `Port.Authorize(new WorkspaceRequest(s)) == RequestDecision.Accepted` (`IWorkspaceHostPort.Authorize`, que compara con `MdiActiveDocument`). Si el puente y el documento activo divergen (por ejemplo, tras un `DocumentActivated` que falló dentro de su contención), el destino es ambiguo.

El alcance es el documento entero, como el botón del panel (D-17, R-B8, R-C1).

**El flujo** (cada paso termina el comando cuando lo dice; los textos son los de la tabla T-1):
1. Sin puerto (no hay controlador): C-01. Sin efecto.
2. `var session = activeSession();` null: C-02. Sin efecto.
   - 2b. `isTarget(session)` falso: C-15. Sin efecto, sin leer el riesgo y sin preguntar.
3. Primera lectura: `GetRacksAtRisk(new WorkspaceRequest(session))`. Una excepción cuenta como el desconocido.
4. `IsKnown` falso: C-03. Sin efecto.
5. Conocido y las dos listas vacías: C-04. Sin efecto.
6. Escribe C-05 si hay racks en riesgo y C-06 si hay inciertos, por RackId y en el orden del puerto.
7. `confirm(Q)` falso (No, Intro o Esc): C-07. Sin efecto.
8. `activeSession()` distinta de `session`: C-08. Si es la misma pero `isTarget(session)` es falso: C-15. Sin efecto en los dos casos.
9. Segunda lectura; desconocido o excepción: C-09. Sin efecto.
10. Si alguna de las dos listas cambió (en orden y por ordinal): C-10. Sin efecto.
11. `DiscardDocument(new WorkspaceRequest(session))`, la única operación con efecto, y su resultado: C-11 a C-14. Una excepción del puerto aquí da C-14 con `ex.Message`.

**Garantías:**
- **Destino:** solo la sesión activa que el puente sigue; nunca otra sesión ni otro documento.
- **Ningún rack se elige solo:** se listan todos los de riesgo y los inciertos, y la única acción es el descarte de los borradores del documento, como el botón del panel (R-B8).
- **Un comparador que falla no impide el descarte:** la vía de reserva da inciertos conocidos (H-04), que se listan aparte y se descartan con la confirmación.
- **El desconocido nunca es «nada en riesgo»** (R-C7): falla explícito, sin efecto, en las dos lecturas.
- **Excepciones de los delegados:** `confirm`, `write`, `activeSession` e `isTarget` no se contienen. Una excepción suya sale al `catch` del comando, que la reporta.
  - Antes del paso 11 no ha habido ningún descarte.
  - Si `write` lanza al escribir C-11 a C-14, el descarte ya ocurrió y ese texto se pierde. Queda el «RackCad error: …» de `RackCommandSupport.Report` (declarado).
- **Operaciones sobre el puerto:** el núcleo solo llama a `GetRacksAtRisk` y a `DiscardDocument`. Nunca llama a `DiscardRack`, `GetField`, `SetPendingText`, `CommitField`, `CrossBoundary`, `TryBeginEdit`, `TryReload`, `TryCheckNow` ni `Take*` (oráculo crítico; prueba con un doble que registra las llamadas).

**Tabla T-1 (cerrada; textos exactos):**

| Id | Cuándo | Texto |
|---|---|---|
| C-01 | sin controlador | `RackCad: No hay borradores del panel Workspace en esta sesión de AutoCAD; no se descartó nada.` |
| C-02 | sin sesión activa | `RackCad: El panel Workspace no sigue ningún documento activo en este momento; no se descartó nada.` |
| C-03 | primera lectura desconocida | `RackCad: No se pudo decidir qué borradores están en riesgo; no se descartó nada.` |
| C-04 | nada en riesgo (conocido) | `RackCad: No hay borradores en riesgo en este documento; no se descartó nada.` |
| C-05 | racks en riesgo | `RackCad: Borradores con trabajo sin aplicar: ` + RackIds unidos por `, ` + `.` |
| C-06 | inciertos | `RackCad: No se pudo decidir si tienen trabajo sin aplicar: ` + RackIds unidos por `, ` + `.` |
| Q | la pregunta | `¿Descartar los borradores de este documento?` |
| C-07 | respuesta distinta de Sí | `RackCad: No se descartó nada.` |
| C-08 | el activo cambió | `RackCad: El documento activo cambió; no se descartó nada.` |
| C-09 | segunda lectura desconocida | `RackCad: No se pudieron volver a comprobar los borradores en riesgo; no se descartó nada.` |
| C-10 | las listas cambiaron | `RackCad: Los borradores en riesgo cambiaron; no se descartó nada. Vuelve a ejecutar RACKPANELDESCARTAR para revisarlos.` |
| C-11 | Discarded | `RackCad: ` + (con racks: `Se descartaron los borradores en riesgo: ` + RackIds + `.`; sin racks: `Se descartaron los borradores del documento.`) + (con inciertos: ` Sin decidir, también descartados: ` + RackIds + `.`) + ` Borradores limpios retirados además: ` + N + `.` (N = `RemovedRackIds` que no están en ninguna de las dos listas; cultura invariante) |
| C-12 | Busy | `RackCad: No se descartaron los borradores del documento: ` + (motivo; sin motivo, `hay una lectura en curso.`) |
| C-13 | Rejected | `RackCad: No se descartaron los borradores del documento: se rechazó. ` + (motivo; sin motivo, `sin detalle.`) |
| C-14 | Failed o excepción | `RackCad: No se descartaron los borradores del documento: falló. ` + (motivo o `ex.Message`; sin ellos, `sin detalle.`) |
| C-15 | destino ambiguo (pasos 2b y 8) | `RackCad: El panel Workspace no sigue el documento activo en este momento; no se descartó nada.` |

**«Sin motivo»** es un motivo (o un `ex.Message`) nulo o en blanco (`string.IsNullOrWhiteSpace`), como en `WhyOf`. C-11 a C-14 reproducen, por RackId, `DocumentDiscardedText` y `WhyOf` del panel. Los textos viven en Application, así que la lista de `WorkspacePaletteHostSourceGuardTests.cs:311-318` (archivo del comando) no los ve; cumplen igualmente G-15.

### D-T3C2-03. El adaptador del comando (R-C1, R-C8, R-C13)

En `WorkspacePanelCommand.cs`, sin archivo nuevo del Plugin (G-10):
- Un segundo método `[CommandMethod("RACKPANELDESCARTAR")] public void RackPanelDiscard()`, sin flags ni alias. Su cuerpo:
  - toma `WorkspaceController.Existing`, **nunca** `GetOrCreate`, y no llama a `OpenPanel` ni a `Show`;
  - llama a `WorkspaceDocumentDiscardCommand.Run(controller?.PilotEdit, () => controller?.Port.GetSnapshot().ActiveSession, s => controller.Port.Authorize(new WorkspaceRequest(s)) == RequestDecision.Accepted, ConfirmOnCommandLine, WriteToCommandLine)`. El tercer delegado solo se invoca con una sesión, que implica controlador;
  - `catch (System.Exception ex) { RackCommandSupport.Report(ex); }`.
- **M-12, en el archivo entero:** `WorkspaceController.GetOrCreate(` aparece exactamente una vez, dentro de `RackPanel`.
- `private static bool ConfirmOnCommandLine(string question)`: sin documento activo, falso. Si no, `PromptKeywordOptions("\n" + question)` con `Keywords.Add("Si")`, `Keywords.Add("No")`, `Keywords.Default = "No"`, `AllowNone = true` y `Editor.GetKeywords`; verdadero solo con `PromptStatus.OK` y `StringResult` igual a `"Si"` (ordinal). La respuesta se lee con `.Status` y `.StringResult`; ningún identificador se llama `Result` (G-22).
- `internal static void WriteToCommandLine(string text)`: `MdiActiveDocument?.Editor.WriteMessage("\n" + text)`. Es el único escritor de la línea de comandos del panel (G-12): lo usan el comando y el controlador.
- **Sin subdiálogo** (R-C13): ni ventana, ni `EnterPanelModal`, ni `MessageBox`.
- **Declarado:** leer `PilotEdit` crea el host de navegación si no existía (H-03). No se suscribe a nada ni lee el dibujo; con el controlador creado por RACKPANEL, la paleta ya lo leyó.
- La documentación del archivo deja de decir «the one command of the workspace panel» y nombra los dos. Ningún comentario ni literal contiene la subcadena `[CommandMethod(` fuera de los dos atributos (G-07).

### D-T3C2-04. El envoltorio del asesor, en Application (R-C2, R-C5, R-C7, R-C12)

Una clase nueva, `src/RackCad.Application/Workspace/WorkspaceCloseAdvisorRelay.cs`:

`public sealed class WorkspaceCloseAdvisorRelay : IWorkspaceLifecycleAdvisor`, con el constructor

`(IWorkspaceLifecycleAdvisor inner, IWorkspacePilotEditPort risks, Func<SessionId, string?> documentNameOf, Func<SessionId, bool> isActive, Action<string> writeNow, Action<string, Exception>? report)`.

Los argumentos no anulables lanzan `ArgumentNullException`. El archivo empieza por `#nullable enable` (G-21).

**Contextos de registro (`report`), exactos:**
- `WorkspaceCloseAdvisorRelay.AdviseClose`: la delegación lanzó. Hoy ese fallo lo registraba el núcleo con `Log("OnBeginDocumentClose")`; ninguna prueba ni guarda lo fija con el cableado del controlador, y el cambio se declara.
- `WorkspaceCloseAdvisorRelay.VetoMessage`: falló una consulta o la escritura del aviso de veto.
- `WorkspaceCloseAdvisorRelay.SessionDestroyed`: falló la lectura del riesgo o la composición del mensaje.

Un `report` que lanza se ignora, como `Report` del host.

**`AdviseClose(session)`:**
- `var decision = CloseDecision.Veto;` y, en un `try`, `decision = inner.AdviseClose(session) == CloseDecision.Allow ? CloseDecision.Allow : CloseDecision.Veto;`. Si la delegación lanza: `report` y Veto. **Nunca parte de `default`**, que es Allow (H-08).
- Con Veto, en otro `try`, escribe V-01 o V-02 con `writeNow`. El nombre sale de `documentNameOf` y se acorta al último segmento tras `\` o `/` con operaciones de cadena (sin `System.IO`, G-15); un nombre vacío, o una consulta que lanza, da «el documento». Si `isActive` lanza, cuenta como activo.
- **La escritura y las consultas nunca cambian la decisión:** sus fallos van a `report`.

**`SessionDestroyed(session, closeSequenceIntact)`:**
- **Todo el trabajo propio previo a la delegación va contenido:** un fallo va a `report` y nunca impide delegar.
  - En un `try`, `risks.GetRacksAtRisk(new WorkspaceRequest(session))`. Una excepción cuenta como el desconocido.
  - Compone P-01, P-02 o P-03 y lo añade a la lista de mensajes pendientes. Un riesgo conocido y vacío no añade nada.
- **Después delega `inner.SessionDestroyed(session, closeSequenceIntact)`, siempre y una sola vez, sin contenerla:** su excepción llega al núcleo, que la registra como hoy (H-09).
- No llama a `documentNameOf` ni a `isActive`, porque la sesión ya no está en el puente (H-07, H-09). No consume las colas del panel (G-16).

**`public IReadOnlyList<string> TakeMessages(SessionId? activeSession)`:**
- Con null devuelve vacío y conserva los mensajes.
- Con una sesión, devuelve todos en orden y vacía la lista.

**El envoltorio no tiene más estado:** ni borradores, ni decisiones propias, ni el puente.

**Operaciones sobre el puerto:** el envoltorio solo llama a `GetRacksAtRisk`; nunca a `DiscardRack`, `DiscardDocument`, `GetField`, operaciones del campo, lecturas de la intención ni `Take*`. Se prueba con un doble que registra las llamadas.

**Declarado (R-C2 y D-T3-08):**
- **Dos ampliaciones acotadas de D-04:**
  - en `AdviseClose`, la lectura de memoria del nombre y de la actividad del documento (`TryGetSession` y `DocumentManager`, P-03) y la escritura del aviso de veto en la línea de comandos;
  - en `SessionDestroyed`, la lectura de memoria del riesgo y el mensaje diferido.
  - Ninguna lee ni escribe el dibujo ni abre un diálogo.
- **El envoltorio no vacía texto vivo:** el veto ve el pendiente del borrador sin tocar nada (D-T3-08).
- **La documentación de `IWorkspaceLifecycleAdvisor.cs`:** «Both calls … write nothing and open no dialog» pasa a decir que las dos llamadas no leen ni escriben el dibujo ni abren diálogos, y que un decorador puede escribir un aviso en la línea de comandos (I-64 F4-T3C2). Es un cambio solo de documentación (G-24), y el archivo entra en el alcance por eso.

**Tabla T-2 (cerrada; D = `«` + nombre + `»`, o `el documento`):**

| Id | Cuándo | Texto |
|---|---|---|
| V-01 | Veto del documento activo | `RackCad: No se cerró ` + D + `: puede tener borradores del panel Workspace con trabajo sin aplicar. Descártalos en el panel con "Descartar borradores de este documento" o con el comando RACKPANELDESCARTAR, y vuelve a cerrarlo.` |
| V-02 | Veto de otro documento | `RackCad: No se cerró ` + D + `: puede tener borradores del panel Workspace con trabajo sin aplicar. Actívalo primero; después descártalos en el panel con "Descartar borradores de este documento" o con el comando RACKPANELDESCARTAR, y vuelve a cerrarlo.` |
| P-01 | pérdida conocida con racks | `RackCad: Al cerrarse un documento se perdió el trabajo sin aplicar de los borradores del panel Workspace: ` + RackIds + `.` + (con inciertos: ` Sin decidir: ` + RackIds + `.`) |
| P-02 | solo inciertos | `RackCad: Al cerrarse un documento no se pudo decidir si se perdió trabajo de los borradores del panel Workspace: ` + RackIds + `.` |
| P-03 | desconocido | `RackCad: Al cerrarse un documento no se pudo decidir si se perdió trabajo de los borradores del panel Workspace.` |

«Puede tener» es neutral a propósito: el Veto significa que se podría perder trabajo, y también sale cuando la evaluación del host falla (incierto).

### D-T3C2-05. El cableado en el controlador (R-C2, R-C10, R-C12)

- **Campo:** `private readonly WorkspaceCloseAdvisorRelay _closeAdvisor;`.
- **En el constructor, tras `_pilotEdit`:**
  - `_closeAdvisor = new WorkspaceCloseAdvisorRelay(_host, _pilotEdit, DocumentNameOf, id => _bridge.ActiveSession == id, WorkspacePanelCommand.WriteToCommandLine, LogFault);`;
  - `_bridge.SetLifecycleAdvisor(_closeAdvisor);` sustituye a `_bridge.SetLifecycleAdvisor(_host);`.
  - Una sola instalación, un solo `new WorkspaceHostCore(` y ninguna suscripción nueva (G-11).
- **`private string DocumentNameOf(SessionId id)`:** `_bridge.TryGetSession` (y no destruida) y un recorrido de `AcApplication.DocumentManager` por `UnmanagedObject.ToInt64()`, como `FindDocumentOrNull`. Devuelve `Document.Name` o null. Es lectura de memoria (P-03), con variable local y sin campos de tipo `Document` (G-13).
- **El relevo `OnActiveSessionChanged`:** tras el `try`/`catch` del refresco y antes de `NotifyStateChanged()`, llama a `WriteCloseMessages();`. Es un método privado con su `try`/`catch` (`LogFault("WorkspaceController.WriteCloseMessages", ex)`) que escribe con `WorkspacePanelCommand.WriteToCommandLine` cada texto de `_closeAdvisor.TakeMessages(_bridge.ActiveSession)`.
  - En la destrucción del activo, `_bridge.ActiveSession` ya es null (H-11): el mensaje espera al siguiente `DocumentActivated` (R-C12). Con QUIT queda solo el registro del host (H-14).
  - El cuerpo del relevo no nombra `_bridge.` (G-14).
- **Ningún `WriteMessage` en el controlador** (G-12) y ningún `RackLog` fuera de `LogFault`.

### D-T3C2-06. El aviso del panel nombra las dos vías (R-C11)

En `WorkspaceBrowserViewModel.RowOf`:
- **1140, el veto de la sesión presentada:** la cola pasa a ` Descártalos con "Descartar borradores de este documento" o con el comando RACKPANELDESCARTAR; cada pestaña también se descarta con su "Cerrar".`. La cabeza no cambia.
- **1135, otra sesión:** `No se cerró otro documento con borradores sin aplicar; actívalo para descartarlos con el panel o con el comando RACKPANELDESCARTAR.`.
- Ningún texto nombra una vía de aplicar («Actualizar», «Aplicar»).

**Cambios declarados de pruebas:**
- `WorkspacePilotCloseSyncTests.cs:780-782` (texto exacto nuevo);
- 785: pasa a `Assert.Contains("RACKPANELDESCARTAR", row.Text)`;
- 805: texto exacto nuevo.
- 783-784 y las demás (`Contains`/`StartsWith`) no cambian.

**Desviación declarada de D-T3B2-04 e INV-F4T3B2-08** («se quita el aviso de veto con un `DiscardDocument` con `Discarded` de esa sesión»):
- Tras un descarte por el comando, la fila de veto del panel queda hasta «Entendido» (H-22), un veto nuevo de la sesión o su muerte (`PurgeDeadSessions`). Su botón da entonces «No hay borradores en riesgo…».
- No se puede cerrar sin romper otras guardas o el alcance: `GetRacksAtRisk` solo se admite en los dos gestos del panel (`WorkspacePilotCloseSyncGuardTests.cs:126-134`), y el host no cambia. No se añade trabajo de UI.
- **Prueba de evidencia barata,** en `WorkspacePilotCloseSyncTests.cs`:
  1. un veto presentado;
  2. un descarte externo con el doble;
  3. un `Present`;
  4. el campo queda sin pendiente y la fila sigue.

### D-T3C2-07. Los motivos del Plugin en español (R-C14 corregida)

Cada literal de la tabla T-3 se sustituye en su sitio por su español exacto. `ex.Message` no cambia, ni los literales de F3, ni `ReadPreviewWitness` (157, 174, 178 y 182), ni el `Debug.Assert` (200), ni `RackCommandSupport` («RackCad error: »).

**Tabla T-3 (cerrada):**

| n | Archivo:línea | Inglés | Español |
|---|---|---|---|
| 1 | `WorkspaceNavigationHost.cs:224` | `There is no active document.` | `No hay documento activo.` |
| 2 | `WorkspaceNavigationHost.cs:226` | `A command is in progress.` | `Hay un comando en curso.` |
| 3 | `WorkspaceNavigationHost.cs:236` | `The request does not belong to the active document.` | `La petición no es del documento activo.` |
| 4 | `RackPilotIntentAdapter.cs:80` | `The catalog is not available.` | `El catálogo no está disponible.` |
| 5 | `RackPilotIntentAdapter.cs:85` | `The catalog could not be loaded: ` | `No se pudo cargar el catálogo: ` |
| 6 | `RackPilotIntentAdapter.cs:112` | `The preview witness has no key.` | `El testigo de la vista previa no tiene clave.` |
| 7 | `RackPilotIntentAdapter.cs:118` | `The scan of the drawing carries no raw envelopes.` | `La exploración del dibujo no trajo las envolventes en bruto.` |
| 8 | `RackPilotIntentAdapter.cs:127` | `The document is locked by another operation.` | `El documento está bloqueado por otra operación.` |
| 9 | `RackPilotIntentAdapter.cs:131` | `The drawing could not be read: ` | `No se pudo leer el dibujo: ` |
| 10 | `RackPilotIntentAdapter.cs:135` | `The edit intention could not be read: ` | `No se pudo leer la intención de edición: ` |

**Comprobado:**
- ninguna traducción contiene `Document` con mayúscula (prohibido en el cuerpo de `LoadCatalog`), `Lock`, `Metric`, `Provider`, `Bom` ni las demás subcadenas de G-15;
- quedan en inglés, fuera de los tres cuerpos, exactamente: `"There is no active document."` 2 veces, `"A command is in progress."` 2, `"The request does not belong to the active document."` 1 y `"The document is locked by another operation."` 1. **Es un hecho medido, no una guarda:** la guarda de T-3 mira solo los tres cuerpos, para no congelar los literales de F3 que F7 traducirá.

### D-T3C2-08. La instalación del asesor: guarda de invariante en lugar de M-1 (R-C10)

**`WorkspaceLifecycleGuardTests.cs:277-288` se reescribe** con el mismo nombre de prueba si sigue siendo verdadero, o con uno nuevo; en los dos casos es un cambio declarado:
- se conservan 283 (una sola `.SetLifecycleAdvisor(` en el controlador), 285 (`+=` = 3), 286 (`-=` = 0) y 287 (sin `.Start(` en el constructor);
- **284 se sustituye** por este orden, en el cuerpo del constructor. Cada aguja es una **expresión regular tolerante a espacios y a saltos de línea**, no una subcadena:
  1. `_host\s*=\s*new\s+WorkspaceHostCore\s*\(`
  2. `_pilotEdit\s*=\s*new\s+PilotEditAdapter\s*\(`
  3. `_closeAdvisor\s*=\s*new\s+WorkspaceCloseAdvisorRelay\s*\(\s*_host\s*,`
  4. `_bridge\.SetLifecycleAdvisor\s*\(\s*_closeAdvisor\s*\)\s*;`

  El archivo tiene exactamente un `new WorkspaceHostCore(`, un `new WorkspaceCloseAdvisorRelay(` y una `.SetLifecycleAdvisor(`. No queda la alternativa «o `_host`»: el único asesor instalado es el envoltorio sobre el host.

  **Por qué cada elemento, más allá del «solo el orden y su conteo» de F4-E35:** es el mínimo para probar el requisito del Owner «no second independent lifecycle authority»:
  - el host primero;
  - el puerto del piloto antes del relevo, que lo recibe y lanza con null, lo que rompería RACKPANEL sin que Core lo viera;
  - el relevo construido sobre ese host;
  - el asesor instalado igual a ese relevo.

  El nombre del campo se fija porque es lo que une el relevo construido con el instalado. Se declara como ampliación acotada de F4-E35.

  El mismo texto rige INV-F4T3C2-07 y la guarda de D-T3C2-10.

Esto protege el comportamiento, no una expresión.

**La otra mitad de M-1 (F4-E35), el reenvío del puente, se ejecuta como cambio declarado:**
- `WorkspaceLifecycleGuardTests.cs:233-244` exige hoy la igualdad exacta del cuerpo aplanado. Pasa al invariante que fija F4-E35:
  - el cuerpo del `SetLifecycleAdvisor` interno de `AutoCadEventBridge` contiene exactamente una llamada `_core.SetLifecycleAdvisor(advisor)` y ninguna otra sentencia;
  - el método sigue `internal`, sin ningún `public void SetLifecycleAdvisor`.
- Se conservan los conteos de `IWorkspaceLifecycleAdvisor` (1), `+=` (16), `-=` (16) y manejadores (16) de esa misma prueba.
- El puente no cambia.

**El comportamiento se fija en Core** (D-T3C2-10), con el `EventBridgeCore` real, el host real y el envoltorio:
- la misma decisión que el host para Veto y Allow;
- una delegación que lanza da Veto, sin degradar la sesión;
- una sola llamada a `inner` por intento y por destrucción;
- `closeSequenceIntact` llega intacto;
- el envoltorio se entera después de que la sesión sale del registro;
- el riesgo se lee antes de delegar;
- un desconocido nunca se escribe como «nada en riesgo» ni se calla;
- un fallo de la escritura no cambia la decisión.

**Requisitos del Owner** sobre el envoltorio:
- **instalado una vez:** la guarda y el controlador perezoso único;
- **sin multiplicar suscripciones:** G-11;
- **sin una segunda autoridad de ciclo de vida:** toda decisión es la de `inner`, y el envoltorio no tiene estado de borradores;
- **no se traga un veto ni convierte desconocido o error en limpio:** las pruebas de arriba;
- **liberación y sustitución deterministas:** se instala una vez y no se reemplaza; el puente ya fija la sustitución (WLH:176-194), y el controlador no se libera (H-10).

### D-T3C2-09. Los censos de comandos (R-C3, R-C9), por nombre y no por número

| Guarda | Cambio declarado |
|---|---|
| `WorkspaceHostSourceGuardTests.cs:49-56` | La misma prueba (G-09): el archivo del comando declara exactamente `{RACKPANEL, RACKPANELDESCARTAR}`, cada uno una vez y por nombre, y ningún otro `[CommandMethod`; 55 sin cambio |
| `WorkspacePaletteHostSourceGuardTests.cs:320-330` | 325: el mismo invariante por nombre; 326-329 sin cambio |
| `WorkspaceBridgeSourceGuardTests.cs:159-163` | Una rama nueva en `HandlerForbiddenCallsFor` para `CommandAdapterPath`, por igualdad de ruta completa, con `ForbiddenCallsWithout("GetKeywords")`: todo lo demás sigue prohibido allí |
| `WorkspaceBridgeSourceGuardTests.cs:336-351` | `DeclaresEntryPoint`: en el archivo exacto del comando, exactamente un `[CommandMethod("RACKPANEL")]`, a lo sumo un `[CommandMethod("RACKPANELDESCARTAR")]` y ningún otro; se quitan los dos antes de `EntryPoints`; se actualiza la documentación (336-339). La existencia de exactamente uno la fija la guarda nueva |
| `WorkspaceSourceGuardExceptionTests.cs:30-61` | Autopruebas: la pareja exacta da falso; un RACKPANELDESCARTAR duplicado, con flags o con otro nombre parecido, da verdadero; los 10 casos de 36-46 siguen en verdadero |
| `WorkspaceSourceGuardExceptionTests.cs:250-258` | 257: 3 comparaciones `string.Equals(path, (Reader\|Command)AdapterPath, StringComparison.Ordinal)` (toda excepción por igualdad de ruta completa); 234 y 255 sin cambio |
| `CustomPropertiesCommandGuardTests.cs:55-58` | Un grupo nuevo con nombre, `F4PanelDiscardCommands = { "RACKPANELDESCARTAR" }`, concatenado a `Baseline`, con su documentación |
| `SelectiveEditorOpenTests.cs:534-567` | 39 → 40, con un `<para>` de I-64 F4-T3C2 que nombra `RACKPANELDESCARTAR` en `WorkspacePanelCommand.cs` |

`WorkspaceHostSourceGuardTests.cs:65-73` sigue pasando, pero su nombre deja de describir el archivo entero. La guarda nueva fija, sobre el cuerpo de `RackPanelDiscard`: `Existing`, ni `GetOrCreate`, ni `OpenPanel`, ni `Show`.

### D-T3C2-10. Las guardas y pruebas nuevas

- **`tests/RackCad.Tests/Workspace/WorkspaceCommandAdvisorTests.cs`** (Core, comportamiento):
  - `WorkspaceLazyInstance.Existing`: null antes y sin crear (`CreateCount` 0); la instancia después;
  - el núcleo del comando con un doble de `IWorkspacePilotEditPort` que registra las llamadas:
    - cada fila de T-1 con su texto exacto;
    - C-15 del paso 2b con cero llamadas a `GetRacksAtRisk` y a `DiscardDocument`;
    - C-15 del paso 8 (deja de autorizar después de «Sí») sin `DiscardDocument`;
    - `Run` solo llama a `isTarget` con la sesión que acaba de dar `activeSession()` (un doble que lo registra), que es la premisa del adaptador;
    - sin efecto antes de «Sí»;
    - la TOCTOU en las dos listas;
    - el cambio de sesión activa;
    - el desconocido en las dos lecturas;
    - el incierto que se descarta;
    - la excepción del puerto;
    - el conjunto exacto de operaciones: solo `GetRacksAtRisk` y `DiscardDocument`;
  - el envoltorio, aislado (con un `inner` programable) y sobre el `EventBridgeCore` real con el host real (patrón `Env(wire: false)` de `WorkspaceLifecycleHostTests.cs`):
    - las propiedades de D-T3C2-08 y las filas de T-2;
    - `TakeMessages(null)` conserva y `TakeMessages(sesión)` vacía;
    - una lectura del riesgo que lanza y un `report` que lanza siguen delegando exactamente una vez;
    - el conjunto exacto de operaciones: solo `GetRacksAtRisk`;
    - los contextos de `report`;
  - las pruebas que esperan una excepción o su ausencia usan `Record.Exception`, como en F4-T3C1.
- **`tests/RackCad.Tests/Workspace/WorkspaceCommandAdvisorGuardTests.cs`** (guardas de texto del Plugin, cada una con su autoprueba en negativo y por cuerpo de miembro):
  - el archivo del comando con la pareja exacta;
  - el archivo del comando con exactamente un `WorkspaceController.GetOrCreate(`, dentro de `RackPanel` (M-12);
  - el cuerpo de `RackPanelDiscard`: `WorkspaceController.Existing`, la llamada a `WorkspaceDocumentDiscardCommand.Run(` y `.Authorize(`, y ni `GetOrCreate` ni `OpenPanel` ni `Show`;
  - `GetKeywords` solo en `ConfirmOnCommandLine`, con `"Si"`, `"No"`, `Default = "No"` y `AllowNone = true`;
  - sin ventana, `EnterPanelModal` ni `MessageBox`;
  - el controlador: `Existing => Holder.Existing`; el patrón exacto de D-T3C2-08 (que va en `WorkspaceLifecycleGuardTests.cs`); `WriteCloseMessages()` en el relevo y `TakeMessages(` con `_bridge.ActiveSession`; sin `WriteMessage`;
  - T-3 **solo sobre los cuerpos** de `ReadOnActiveDocument`, `Read` y `LoadCatalog`: ningún inglés de la tabla y cada español presente (D-T3C2-07);
  - el código sin comentarios (`Src.Code`) de `IWorkspaceLifecycleAdvisor.cs` declara exactamente los dos miembros, con dos sentencias en el cuerpo de la interfaz: **solo cambia la documentación**;
  - **autopruebas de la excepción de `GetKeywords`,** con `HandlerForbiddenCallsFor`:
    - para el archivo del comando, `ed.GetKeywords(o)` no coincide, pero `GetString`, `GetSelection`, `MessageBox`, `ShowDialog`, `GetObject` y `SelectImplied` sí;
    - para el controlador, el lector, los hechos, los efectos, el host y `src/RackCad.Plugin/Workspace/WorkspacePanelCommand2.cs`, `GetKeywords` sí coincide.
- **UI** (`WorkspacePilotCloseSyncTests.cs`): D-T3C2-06 (incluida la prueba de evidencia de la desviación) y los menores de D-T3C2-11.

### D-T3C2-11. Los menores de F4-T3C1 (orden del Owner F4-E48)

- **R-1:** dos pruebas de texto exacto del cierre limpio con Busy, en `CloseTab`:
  - con el motivo `Hay una lectura del piloto en curso.`: `Se cerró la pestaña; su borrador limpio quedó en memoria: Hay una lectura del piloto en curso.`;
  - sin motivo: `…: hay una lectura en curso.`. El motivo nulo se pasa como `reason!` o `" "` (G-21).
- **R-2:** dos `CloseTab("R-1")` seguidos **sin cancelar**: el primero con `GetField` que lanza, y el segundo con la pestaña protegida y legible. El texto exacto es el habitual («La pestaña Nombre R-1 tiene un borrador…»).
- **R-4:** la documentación de `TakeLossNotices` y `TakeCloseNotices` en el puerto:
  - el aviso de pérdida cuya conversión falla se pierde y queda en el registro;
  - si `TakeCloseNotices` falla, devuelve vacío y no vacía la cola.

  También la de `RequestDocumentDiscard` y `ConfirmDiscard` en el VM: el desconocido no pregunta ni descarta. Sin cambio de código.
- **R-3** («`ErrorReason` solo en Error» del host) **sigue siendo de F4-T4**: esta subtarea no toca el host.

### D-T3C2-12. Modal y foco

- Ningún contrato concreta una obligación de código de modal o de foco:
  - `EnterPanelModal` sigue sin llamadores;
  - D-T3-05, R-B7 y R-B11 solo aplican con un subdiálogo o un foco tocado, y T3C2 no los añade.
- El comando usa la línea de comandos (R-C13).
- **Quedan para Smoke-2:** el foco tras el comando, la pregunta con la paleta visible y la escritura en `BeginDocumentClose` con el host real (R-C4).

### D-T3C2-13. Lo que no cambia y las limitaciones

**No cambian:**
- `EventBridgeCore`, `AutoCadEventBridge`, `WorkspaceHostCore`, `WorkspacePilotSupport` (la compuerta), `BrowserSession`, el XAML, el VM del campo y el del panel;
- `IWorkspaceLifecycleAdvisor`, salvo su documentación (D-T3C2-04);
- la forma pública del VM del navegador (33 firmas);
- los demás archivos del Plugin.

**Fuera de alcance, de F4-T4:**
- Actualizar, PREPARE/MUTATE/POST, la escritura authored, la mutación en el contexto de comando, la relectura tras la mutación, la contención de las pistas de mutación propia, el Undo único y la atomicidad;
- R-3.

**Limitaciones declaradas:**
- la pérdida que escribe el envoltorio es informativa y puede no coincidir con la del host en el residual conocido; el aviso del panel es la autoridad (H-13);
- con QUIT, solo el registro;
- la pérdida no nombra el documento, porque la sesión ya no está en el puente;
- la fila de veto obsoleta tras el comando, que es una desviación declarada de D-T3B2-04 (D-T3C2-06);
- leer `PilotEdit` crea el host de navegación (D-T3C2-03);
- si `write` lanza al escribir C-11 a C-14, el descarte ya ocurrió y su texto se pierde (D-T3C2-02);
- **el desconocido es residual:** si ocurriera (hoy es inalcanzable), el documento quedaría vetado (`AdviseClose` da Veto cuando la evaluación lanza) y sin descarte posible por el comando ni por el botón del documento. Es coherente con R-C7. Seguirían el descarte por pestaña del panel y la purga al morir la sesión;
- si se destruye un documento que no es el activo, el relevo no se dispara en la destrucción, y P-01 a P-03 esperan al siguiente `DocumentActivated` (compatible con R-C12);
- **descubribilidad:** el comando no entra en la ayuda (`RackCommandReference`), igual que RACKPANEL. Se descubre por V-01, V-02 y las dos filas de veto del panel;
- los motivos de F3 siguen en inglés (F7);
- `ReadPreviewWitness` queda en inglés, sin llamadores.

## 4. Pruebas de F4-T3C2

**La lista exhaustiva de pruebas y guardas nuevas es la de D-T3C2-10.** Resumen:

**Core, `WorkspaceCommandAdvisorTests.cs` (nuevo):**
- `Existing` (D-T3C2-01);
- el núcleo del comando:
  - las filas C-01 a C-15 con su texto exacto y el orden de escritura;
  - las dos variantes de C-15;
  - cero llamadas a `DiscardDocument` en todo camino sin «Sí» o con un cambio, y una llamada con «Sí» y las listas iguales;
  - el conjunto exacto de operaciones del puerto;
  - `isTarget` solo con la sesión de `activeSession()`;
- el envoltorio:
  - las propiedades de D-T3C2-08;
  - V-01, V-02, P-01, P-02 y P-03;
  - la contención antes de delegar (riesgo que lanza, `report` que lanza);
  - los contextos de `report` y el conjunto exacto de operaciones.

**Core, `WorkspaceCommandAdvisorGuardTests.cs` (nuevo):** las guardas de D-T3C2-10, cada una con su autoprueba en negativo.

**Core, cambios declarados:**
- `WorkspaceLifecycleGuardTests.cs:277-288` y 233-244 (D-T3C2-08: la instalación y el reenvío del puente);
- las guardas de D-T3C2-09 en `WorkspaceHostSourceGuardTests.cs`, `WorkspacePaletteHostSourceGuardTests.cs`, `WorkspaceBridgeSourceGuardTests.cs`, `WorkspaceSourceGuardExceptionTests.cs`, `CustomPropertiesCommandGuardTests.cs` y `SelectiveEditorOpenTests.cs`.

**UI, `WorkspacePilotCloseSyncTests.cs`:**
- los cambios declarados de 780-785 y 805;
- la prueba de evidencia de la desviación de D-T3C2-06;
- las pruebas nuevas de R-1 y R-2.

**Ninguna otra prueba existente cambia.**

**RED:**
- el esqueleto lleva ya los dos atributos y los cambios de censo, para que los censos de fuera del filtro (`CustomPropertiesCommandGuardTests`, `SelectiveEditorOpenTests`) pasen (R-C3);
- el cuerpo RED de `RackPanelDiscard` es un `try` vacío con su `catch (System.Exception ex) { RackCommandSupport.Report(ex); }`, sin `ConfirmOnCommandLine` ni `WriteToCommandLine`;
- `Run` no hace nada;
- el envoltorio solo delega (`AdviseClose` devuelve lo de `inner` y `SessionDestroyed` delega), y `TakeMessages` da vacío;
- `WorkspaceLazyInstance.Existing` da null; el controlador no cambia en el RED, así que no tiene `WorkspaceController.Existing`;
- los textos, la UI y la documentación quedan sin cambio;
- fallan solo pruebas nuevas o cambiadas, por aserción. Las que esperan una excepción o su ausencia usan `Record.Exception`.
- **Declarado:** en la CI del RED, `build-plugin` no corre (`needs: tests`), así que la compilación del Plugin con el esqueleto la comprueba el Worker en local (build Release de la solución) y la CI del GREEN.

## 5. Contrato propuesto de F4-T3C2-COMMAND-ADVISOR (a fijar en la emisión)

**`AllowedWriteScope`:** lista blanca exacta de 20 rutas, cuatro de ellas nuevas.
- `src/RackCad.Application/Workspace/`:
  - `WorkspaceDocumentDiscardCommand.cs` (nuevo);
  - `WorkspaceCloseAdvisorRelay.cs` (nuevo);
  - `WorkspaceLazyInstance.cs`;
  - `IWorkspacePilotEditPort.cs` (solo documentación);
  - `IWorkspaceLifecycleAdvisor.cs` (solo documentación);
- `src/RackCad.Plugin/Workspace/`: `WorkspacePanelCommand.cs`, `WorkspaceController.cs` y `WorkspaceNavigationHost.cs`;
- `src/RackCad.Plugin/Views/RackPilotIntentAdapter.cs`;
- `src/RackCad.UI/Workspace/WorkspaceBrowserViewModel.cs`;
- `tests/RackCad.Tests/Workspace/`:
  - `WorkspaceCommandAdvisorTests.cs` (nuevo);
  - `WorkspaceCommandAdvisorGuardTests.cs` (nuevo);
  - `WorkspaceLifecycleGuardTests.cs`, `WorkspaceHostSourceGuardTests.cs`, `WorkspacePaletteHostSourceGuardTests.cs`, `WorkspaceBridgeSourceGuardTests.cs` y `WorkspaceSourceGuardExceptionTests.cs`;
- `tests/RackCad.Tests/CustomPropertiesCommandGuardTests.cs` y `tests/RackCad.Tests/SelectiveEditorOpenTests.cs`;
- `tests/RackCad.UI.Tests/Workspace/WorkspacePilotCloseSyncTests.cs`.

**`ForbiddenWriteScope`:** prefijos compactos y rutas exactas, como en F4-T3C1, sin solaparse con ninguna ruta del `AllowedWriteScope`. Incluye:
- **explícitamente** `docs/`, `.github/`, `eng/`, `tools/`, `assets/`, `deploy/` y los archivos raíz de la solución;
- `src/RackCad.Domain/`, las demás carpetas de Application, Plugin y UI y sus archivos raíz;
- `src/RackCad.Application/Workspace/Drafts/` y, por ruta exacta, `EventBridgeCore.cs`, `WorkspaceHostCore.cs`, `WorkspacePilotSupport.cs`, `BrowserSession.cs`, `BrowserSessionRegistry.cs`, `IWorkspaceHostPort.cs`, `IWorkspaceBrowserSummaryPort.cs` y `WorkspaceEditIntent.cs`;
- los demás archivos de `Plugin/Workspace` y de `UI/Workspace`;
- las guardas de `tests/RackCad.Tests/Workspace/` no listadas y los proyectos de prueba.

**`src/RackCad.Plugin/Views/`** no puede ser un prefijo prohibido, porque contiene `RackPilotIntentAdapter.cs`, que está permitido. Sus demás archivos quedan, **de forma explícita**, al `Scope` (diff ⊆ `AllowedWriteScope`), que cubre todo lo no listado.

**Invariantes:**
- **INV-F4T3C2-01 (cero escrituras authored):** el comando solo llama a `GetRacksAtRisk` y `DiscardDocument` del puerto, y el envoltorio solo a `GetRacksAtRisk`, además de lecturas de memoria. Se prueba el conjunto exacto con un doble que registra las llamadas. Ninguna API de escritura, Actualizar, lectura de la intención ni semántica de F4-T4.
- **INV-F4T3C2-02 (sin crear):** `RACKPANELDESCARTAR` usa `WorkspaceController.Existing` y nunca `GetOrCreate`, `OpenPanel` ni `Show`; `Existing` nunca crea. El archivo del comando tiene exactamente un `WorkspaceController.GetOrCreate(`, dentro de `RackPanel` (M-12). Sin controlador: C-01 y ningún efecto.
- **INV-F4T3C2-03 (destino, C-F4-03):** solo la sesión activa que el puente sigue y que autoriza contra el documento que ejecuta.
  - Sin ella: C-02.
  - Si no autoriza (destino ambiguo): C-15, sin leer el riesgo y sin preguntar.
  - Si cambia o deja de autorizar antes del descarte: C-08 o C-15.
  - Nunca otra sesión ni otro documento.
- **INV-F4T3C2-04 (descarte explícito):**
  - lista por RackId de los racks en riesgo y de los inciertos;
  - Sí o No, con No por defecto;
  - con No, Intro, Esc o cancelar, ningún efecto;
  - la TOCTOU sobre las dos listas, en orden y por ordinal;
  - la única operación con efecto es `DiscardDocument`, después de «Sí» y de una segunda lectura igual;
  - ningún rack se elige solo y no hay `DiscardRack`.
- **INV-F4T3C2-05 (desconocido ≠ nada):**
  - en el comando y en el envoltorio, el desconocido o una excepción de la lectura del riesgo es un fallo explícito (C-03, C-09, P-03), nunca «nada en riesgo» ni silencio;
  - el incierto de un comparador que falla se lista y se puede descartar.
- **INV-F4T3C2-06 (el envoltorio):**
  - la decisión es la de `inner`, partiendo de Veto, y una excepción de `inner` es Veto;
  - una sola delegación por intento y por destrucción, con `closeSequenceIntact` intacto;
  - en `SessionDestroyed`, todo el trabajo propio previo va contenido y la delegación ocurre siempre y una vez;
  - el riesgo se lee antes de delegar `SessionDestroyed`;
  - solo llama a `GetRacksAtRisk` del puerto;
  - los contextos de `report` son los de D-T3C2-04;
  - las dos ampliaciones acotadas de D-04 están declaradas, y el envoltorio no vacía texto vivo;
  - los fallos de escritura y de consulta nunca cambian la decisión;
  - sin estado de borradores, sin consumir las colas del panel y sin llamar al puente en `SessionDestroyed`;
  - V-01, V-02, P-01, P-02 y P-03 exactos;
  - los pendientes se escriben solo en el relevo con una sesión activa.
- **INV-F4T3C2-07 (instalación):**
  - en el cuerpo del constructor y en este orden (expresiones tolerantes a espacios): `_host = new WorkspaceHostCore(`, `_pilotEdit = new PilotEditAdapter(`, `_closeAdvisor = new WorkspaceCloseAdvisorRelay(_host,` y `_bridge.SetLifecycleAdvisor(_closeAdvisor);`;
  - exactamente un `new WorkspaceHostCore(`, un `new WorkspaceCloseAdvisorRelay(` y una `.SetLifecycleAdvisor(` en el archivo;
  - `+=` = 3, `-=` = 0 y un `event`;
  - ningún archivo nuevo del Plugin;
  - `WriteMessage` solo en `WorkspacePanelCommand.cs`.
- **INV-F4T3C2-08 (censos):**
  - el archivo del comando declara exactamente `RACKPANEL` y `RACKPANELDESCARTAR`, sin flags ni alias;
  - el censo por nombre y el bruto se actualizan como en D-T3C2-09;
  - `GetKeywords` se exime solo en el archivo exacto del comando y por igualdad de ruta;
  - ninguna otra prohibición se debilita (unicidad de comandos, frontera del Plugin, propiedad del puente, sin suscripciones por pestaña, sin escritura authored).
- **INV-F4T3C2-09 (aviso del panel):** las dos filas de veto nombran el panel y `RACKPANELDESCARTAR` con los textos exactos de D-T3C2-06, sin vía de aplicar.
- **INV-F4T3C2-10 (motivos del Plugin):** los 10 literales de T-3 en su español exacto y en su sitio; `ex.Message`, F3, `ReadPreviewWitness` y el `Debug.Assert` sin cambio.
- **INV-F4T3C2-11 (menores):** R-1 y R-2 con pruebas de texto exacto; R-4 solo documentación; R-3 queda en F4-T4.
- **INV-F4T3C2-12 (guardas):**
  - solo cambian las declaradas: D-T3C2-08 (la instalación y el reenvío del puente) y D-T3C2-09, y 780-785 y 805 de la UI;
  - cada guarda congelada en una expresión pasa a un invariante;
  - las guardas nuevas tienen su autoprueba en negativo y buscan llamadas por cuerpo de miembro.
- **INV-F4T3C2-13 (regla RED):** el RED es el esqueleto del §4 y compila, no lanza y no cambia el comportamiento. En el RED fallan solo pruebas nuevas o cambiadas, por aserción, y los censos fuera del filtro pasan.
- **INV-F4T3C2-14 (límites):**
  - sin subdiálogo ni ventana, `EnterPanelModal`, `MessageBox`, Dispatcher, Timer, Task, async ni API de foco nuevos;
  - no cambian `EventBridgeCore`, `AutoCadEventBridge`, `WorkspaceHostCore`, la compuerta, `BrowserSession`, el XAML ni la forma pública del VM del navegador; `IWorkspaceLifecycleAdvisor` cambia solo su documentación;
  - sin costuras nuevas ni reflexión sobre campos privados del producto.

**`RequiredTests`:**
- `RackCad.Tests.Workspace`: `MinSelected` 2517, `ExpectRed` true;
- `RackCad.UI.Tests.Workspace`: `MinSelected` 454, `ExpectRed` true.

Las dos líneas base se midieron en la CI de `7bd275e4`.

**`ExpectedEvidence`, controles (autorización de T3C2):**
- nc1..nc3 se ejecutan con su oráculo ordinario, y **cada salida ordinaria de control se conserva tal cual**.
- Una regla local de la subtarea solo se aplica si se reproduce **exactamente** su firma:
  - PD-F4-01: lista blanca exacta, Contract antes que Scope, en nc2;
  - PD-F4-04 y PD-F4-05: la variación de Handoff o Identity en nc1;
  - PD-F4-06: la variación de Ci o Trailer sin relación con la mutación.
- **Una firma nueva no se cubre con ningún sustituto:** vuelve al Owner antes de aplicar ninguno. PD-F4-06 nunca debilita Ci ni Trailer en la verificación real.

Lo demás de `ExpectedEvidence` es como en F4-T3C1: RED y GREEN con su CI exacta, la entrega conforme a su esquema, la verificación del Controller, las revisiones directa e independiente y sin AutoCAD.

**`StopConditions` propias (F4T3C2-C01..C08):**
- **C01:** un archivo fuera del alcance, o un archivo nuevo distinto de los cuatro declarados.
- **C02:** un cambio de una guarda o prueba existente no declarado en el §4. En un intento de corrección, los archivos que esta tarea creó en un intento anterior no son guardas ni pruebas existentes.
- **C03:** escritura authored, Actualizar, semántica de F4-T4, una lectura nueva de la intención o un subdiálogo.
- **C04:** cambiar `EventBridgeCore`, `AutoCadEventBridge`, `WorkspaceHostCore`, la compuerta, `BrowserSession` o el XAML, o algo de `IWorkspaceLifecycleAdvisor` distinto de su documentación; o hace falta un archivo nuevo del Plugin, una suscripción, un `+=` o un `event` nuevos.
- **C05:** el comando tendría que crear el controlador, actuar sobre otra sesión o sobre un destino ambiguo, o elegir un rack sin listarlo.
- **C06:** la semántica congelada de D-08 o D-17, o el comportamiento cerrado de F3 a F4-T3C1 fuera de las decisiones del paquete; una prueba que necesita AutoCAD; o un csproj o la CI.
- **C07:** la tarea evalúa Long-horizon.
- **C08:** la regla de la replanificación reforzada única (F4-E31, F4-E43; autorización de T3C2).

**`attempts`:** 1 de 3.

## 6. Relación con F4-T4

Si F4-T3C2 cierra, F4-T3 queda completa en el nivel de subtareas y se prepara F4-T4. Ninguna semántica de F4-T4 entra aquí; la autorización del Owner las enumera.

## 7. Puntos para la re-revisión

1. ¿Cierran I-1 y I-2 las correcciones de D-T3C2-02 (`isTarget`, C-15) y del §5 (`ExpectedEvidence` de los controles)?
2. ¿Es comprobable el patrón exacto de D-T3C2-08, y coherente con D-T3C2-05, INV-07 y D-T3C2-10?
3. ¿Mete alguna corrección una guarda rota no declarada? En especial:
   - la documentación de `IWorkspaceLifecycleAdvisor.cs`, que entra en el alcance;
   - el delegado `isTarget` y su uso de `Port.Authorize` en el comando.
4. ¿Están todos los hallazgos de la v1 resueltos o dispuestos (§8)?

## 8. Disposición de la revisión adversarial de la v1 (EMITIBLE TRAS CORRECCIONES, 0/2/11)

| Hallazgo | Disposición |
|---|---|
| I-1 (destino ambiguo) | **Aceptado.** D-T3C2-02 define el destino (sesión activa que autoriza contra el documento que ejecuta, C-F4-03), con `isTarget` en `Run`, el paso 2b y el paso 8, la fila C-15, el adaptador con `Port.Authorize` (D-T3C2-03), INV-03 y la prueba con cero lecturas. El §1 cita C-F4-03 y D-17 |
| I-2 (controles y sustitutos) | **Aceptado.** El §5 gana la `ExpectedEvidence` de los controles: firma exacta, conservar cada salida ordinaria y una firma nueva vuelve al Owner. El `ForbiddenWriteScope` nombra `docs/` y `.github/` sin solaparse |
| M-1 (guarda de M-1 ambigua) | **Aceptado.** Patrón exacto de tres líneas, en orden y con sus conteos; sin la alternativa «o `_host`»; el mismo texto en D-08, INV-07 y D-10 |
| M-2 (M-12 en todo el archivo) | **Aceptado.** Exactamente un `WorkspaceController.GetOrCreate(` en el archivo del comando, dentro de `RackPanel` (D-T3C2-03, D-T3C2-10, INV-02) |
| M-3 (ampliaciones de D-04, texto vivo, documentación de la interfaz, contexto del registro) | **Aceptado.** Se declaran en D-T3C2-04 e INV-06. `IWorkspaceLifecycleAdvisor.cs` entra en el alcance solo por su documentación (20 rutas). Se fijan los contextos de `report` |
| M-4 (fallo propio antes de delegar) | **Aceptado.** Todo el trabajo propio previo va contenido y la delegación ocurre siempre y una vez, con su prueba |
| M-5 (la mitad del puente de M-1) | **Aceptado como «no necesitada».** La guarda del reenvío del puente ya es el invariante (un reenvío sin estado), y T3C2 no toca el puente (D-T3C2-08) |
| M-6 (operaciones exactas) | **Aceptado.** Conjunto exacto de operaciones del puerto, con un doble que registra las llamadas, para el comando y para el envoltorio (D-T3C2-02, D-T3C2-04, D-T3C2-10, INV-01) |
| M-7 (desviación de D-T3B2-04) | **Aceptado.** Se nombra «desviación declarada de D-T3B2-04 e INV-F4T3B2-08», con una prueba de evidencia en la UI (D-T3C2-06) |
| M-8 (trampas de compilación y de guardas) | **Aceptado.** G-21 a G-24: `#nullable enable`, `reason!` o `" "`, `\.Result\b` y el vocabulario de la documentación del puerto |
| M-9 (RED ambiguo) | **Aceptado.** Se fija el cuerpo RED de `RackPanelDiscard`; `Existing` en null se refiere a `WorkspaceLazyInstance`; `Record.Exception`; se declara que `build-plugin` no corre en el RED (§4) |
| M-10 (conteos de F3) | **Aceptado.** La guarda de T-3 mira solo los tres cuerpos; los conteos residuales quedan como hecho medido, no como guarda (D-T3C2-07, D-T3C2-10) |
| M-11 (precisiones) | **Aceptado.** Se declaran en D-T3C2-02 y D-T3C2-13: la escritura posterior al descarte, el desconocido residual, la espera del mensaje cuando el destruido no era el activo y la ausencia en la ayuda |

**Riesgos de Smoke-2 de la revisión:** pasan a la lista de Smoke-2 del cierre, los 10.

## 9. Disposición de la re-revisión de la v2 (EMITIBLE TRAS CORRECCIONES, 0/0/11)

Los 13 hallazgos de la v1 quedan resueltos según la re-revisión. Sus 11 menores:

| Hallazgo | Disposición |
|---|---|
| N-1 (C-02 afirma lo que no sabe) | **Aceptado.** C-02 pasa a un texto neutral: «El panel Workspace no sigue ningún documento activo en este momento; no se descartó nada.» |
| N-2 (`_pilotEdit` antes del relevo) | **Aceptado.** `_pilotEdit = new PilotEditAdapter(` entra en el orden, entre el host y el relevo (D-T3C2-08, INV-07 y D-T3C2-10) |
| N-3 (agujas literales; más que F4-E35) | **Aceptado.** Las agujas son expresiones tolerantes a espacios. D-T3C2-08 justifica cada elemento y lo declara como ampliación acotada de F4-E35 |
| N-4 (la mitad del puente de M-1) | **Aceptado: se ejecuta.** La guarda del reenvío del puente pasa al invariante de F4-E35 (una llamada `_core.SetLifecycleAdvisor(advisor)`, `internal`), con sus conteos (D-T3C2-08, §4, INV-12) |
| N-5 (§4 frente a D-T3C2-10) | **Aceptado.** El §4 remite a D-T3C2-10 como lista exhaustiva y la resume. La frase de C-15 se reescribe con sus dos variantes, y se añade que `isTarget` solo recibe la sesión de `activeSession()` |
| N-6 (C02 en un intento de corrección) | **Aceptado.** Se copia la cláusula de F4T3C1-C02 |
| N-7 (nulabilidad en pruebas y Plugin) | **Aceptado.** G-21 cubre los cuatro archivos nuevos y prohíbe anotaciones `?` de tipo referencia en el controlador y el comando |
| N-8 (solo documentación del asesor) | **Aceptado.** Guarda de los dos miembros de la interfaz sobre el código sin comentarios (D-T3C2-10) |
| N-9 (`assets/`, `deploy/`, `Views/`) | **Aceptado.** Se añaden `assets/` y `deploy/`. `Views/` queda de forma explícita al `Scope` |
| N-10 (autopruebas de la excepción de `GetKeywords`) | **Aceptado.** Autopruebas con `HandlerForbiddenCallsFor` (D-T3C2-10) |
| N-11 («sin motivo») | **Aceptado.** «Sin motivo» = nulo o en blanco (`string.IsNullOrWhiteSpace`), también para `ex.Message` |
