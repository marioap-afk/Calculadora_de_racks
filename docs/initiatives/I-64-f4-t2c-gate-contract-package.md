# I-64 — Paquete del contrato de F4-T2C (programación de las lecturas, IC-04)

**Estado:** v2.
- **Revisión de la v1**, la adversarial independiente que exige la orden del Owner: **EMITIBLE TRAS CORRECCIONES** (2 BLOQUEANTE, 7 IMPORTANTE y 10 MENOR). La v2 aplica todos los hallazgos (§9).
- **Re-revisión de la v2:** **EMITIBLE** (0/0/7). Sus siete menores, N-1 a N-7, ya están aplicados en este texto (§10).
**Base:** rama `architecture/workspace-persistente-rackcad` en `92ad237c` (custodia del cierre de F4-T2B2B); `main` en `812baf87`. Los hechos del §2 se midieron en `92ad237c`.
**Precondiciones de emisión:**
- F4-T2B2B está CLOSED / PASS WITH LIMITATIONS (F4-E35).
- El Owner autorizó F4-T2C con su propio presupuesto, `attempts` 1 de 3 (`owner-f4t2c-authorization.md`).
- El Owner fijó el alcance «todo fuera del Idle» (`owner-f4t2c-scope-decision.md`).

## 1. Autoridades

- **Freeze** (`docs/initiatives/I-64-proposal-v5.md`):
  - D-02 (`:160-175`):
    - acceso al DWG solo en el Plugin, con bloqueo y transacciones de alcance de la operación;
    - `TopTransaction == null` en Debug tras cada operación y cada drenaje (INV-14);
    - lecturas del drenaje en contexto de aplicación;
    - panel oculto, cero lecturas;
    - los subdiálogos del panel cuentan como modal RackCad;
    - sin hilos de fondo que toquen AutoCAD;
  - D-04 (`:197-232`):
    - un único puente;
    - los manejadores solo encolan (P-03);
    - el drenaje es diferido, en contexto de aplicación, con bloqueo, en reposo y sin modal RackCad, coalescido y limitado a sus cuatro operaciones (P-04);
    - Idle solo mientras haya pistas;
  - D-17: contención de toda entrada del panel, incluidos los callbacks del dispatcher (`:596-599`);
  - INV-02 (ninguna suscripción por pestaña), INV-14 e INV-23.
- **IC-04** (redacción aceptada, `docs/initiatives/I-64-implementation-clarifications.md:73-85`):
  - ninguna lectura del DWG dentro de un manejador de notificación ni dentro de un callback de Idle o de evento «simplemente porque se disparó»;
  - «después del drenaje» es aceptable; «dentro del manejador de la notificación» no lo es.
- **Disposiciones:**
  - F4-E05 (`f4-dispositions.md:31-35`): la lectura del resumen ocurre dentro de `Application.Idle`; obligación explícita de F4-T2;
  - F4-E18 (`:253-273`): T2 se divide en T2A, T2B y T2C;
  - F4-E31: un rechazo por copia antes del Worker permite una replanificación única;
  - F4-E34: la regla de devolución al Owner de D-T2B2-10 (`:790`);
  - F4-E35: cierre de F4-T2B2B y deuda PD-F4-04.
- **Paquete de F4-T2** (`docs/initiatives/I-64-f4-t2-gate-contract-package.md`):
  - T2H-10 (`:33`);
  - la fila de T2C (`:54`): «El drenaje solo marca; la lectura del resumen pasa a una operación separada posterior. **Prohíbe modificar la clausura de dependencias de la ventana**»;
  - D-T2-06 (`:224-230`): lectura en una operación separada posterior al retorno del callback, programada una sola vez, agrupada, con una excepción de guarda por archivo exacto para el planificador.
  - **Qué sustituye esta tarea** (decisión del Owner): ver D-T2C-07.
- **Órdenes del Owner** (`I-64-pilot/F4-DRAFTS/coordinator/`):
  - `owner-f4t2c-authorization.md`: autorización, alcance mínimo 1-13, pruebas A-J, smoke dirigido si es practicable y reglas del proceso;
  - `owner-f4t2c-scope-decision.md`: **«Todo fuera del Idle»**. El drenaje, con su lectura de selección, y después la lectura del resumen corren en una continuación, cuando la pila del Idle ya volvió.

## 2. Hallazgos del repositorio (medidos en `92ad237c`)

### La ruta actual que incumple IC-04

| Id | Hallazgo | Consecuencia |
|---|---|---|
| H-01 | El único manejador de `Application.Idle` es `(s, e) => Contain("Idle", OnIdle)` (`AutoCadEventBridge.cs:247-252`). `OnIdle` llama a `_core.OnIdle()` y, por cada resultado, invoca `Drained` de forma síncrona (`:258-271`) | Todo lo que cuelga de `Drained` corre **dentro** del callback de Idle |
| H-02 | `EventBridgeCore.OnIdle` (`:382-406`):<br>- olvida la secuencia de cierre de F4-T2B2B (`_lastAllowedClose = null`, `:386`) **siempre**, drene o no;<br>- drena la sesión activa con `TryDrain(new DrainConditions(state.Quiescent, _modalRackCadActive, _panelVisible))`;<br>- llama a `SyncIdle`.<br>Idle queda enganchado mientras alguna sesión no degradada tiene pistas (`IdleNeeded`, `:479-498`) | La pasada se conserva; no se la puede llamar cuando no es elegible (D-T2C-02) |
| H-03 | `WorkspaceController.OnDrained` (`WorkspaceController.cs:147-171`), manejador de `Drained`:<br>1. llama a `_host.OnDrained` y a `_navigation.OnDrained`;<br>2. llama a `AssertNoOpenTransaction` (`:153`), **antes** de la lectura del resumen;<br>3. abre la ventana de aviso (`:162`), llama a `NotifyStateChanged` (`:165`) y la cierra en un `finally` (`:169`) | La ventana de lectura del resumen está dentro de la pila del Idle; la lectura del resumen no tiene comprobación posterior de `TopTransaction` |
| H-04 | **Lectura de selección dentro del Idle:**<br>- `WorkspaceHostCore.OnDrained` (`:151`), con `ReadImplicitSelection` y `MayRead`, llama a `_reader.Read` (`:182-186`);<br>- `WorkspaceSelectionReader.Read` bloquea el documento, abre una transacción y abre referencias, definiciones y sobres (`WorkspaceSelectionReader.cs:40-117`);<br>- una excepción del lector la contiene el host con `Contain(outcome.Session, …)` (`WorkspaceHostCore.cs:203-206`), que marca la sesión como fallida en el host (`:685-687`) | Lectura del DWG en la pila del Idle (operación 1 de D-04). **En alcance por la decisión del Owner.** El trato de los fallos del host no cambia |
| H-05 | **Lectura del resumen dentro del Idle** (F4-E05):<br>1. `NotifyStateChanged` → `WorkspacePaletteHost.OnControllerStateChanged` → `_viewModel.Refresh()` (`WorkspacePaletteHost.cs:121-130`);<br>2. `WorkspacePanelViewModel`, con `GetSnapshot` (`:182`) → `Browser.Present` (`:224`);<br>3. `WorkspaceBrowserViewModel.Materialize` → `ReadSummary(..., userRequested: false)` (`WorkspaceBrowserViewModel.cs:252-272`);<br>4. `WorkspaceNavigationHost.ReadSummaryOnActiveDocument` lee solo con la ventana abierta (`WorkspaceNavigationHost.cs:110-125`);<br>5. `BrowserRackSummaries.Read` hace el barrido completo de sobres con bloqueo y transacción (`WorkspaceNavigationFacts.cs`, `Scan`) | Lectura del DWG en la pila del Idle |
| H-06 | Las demás presentaciones no leen: el cambio de documento, el cambio de estado de comando, el mostrar u ocultar de la paleta y el arranque de `RACKPANEL` llegan con la ventana cerrada, así que `ReadSummary` responde `Busy` (`WorkspaceNavigationHost.cs:117-118`). El resto del drenaje es en memoria: el marcado IC-08, `ApplyDrain` de la navegación, la caché de definiciones y el eco | Conformes; se conservan |
| H-07 | Los manejadores de notificación solo llaman a `_core.` (encolan), a `Contain`/`ContainSession`, a `NativeId(e.Document)`, a `RaiseCommandStateChanged()` y, en el cierre, a `e.Veto()`. Los casos `DocumentActivated` y `DocumentToBeDestroyed` invocan `ActiveSessionChanged` (`AutoCadEventBridge.cs:100-245`) | Conformes (P-03); se conservan. F4-T3 puede envolver el asesor o el manejador de cierre (arrastre M-1 de F4-E35) |
| H-08 | Las lecturas que pide el usuario (clic en una pestaña, recarga, navegación, intención de edición) corren en un manejador de WPF, no en un callback de AutoCAD (O3-01; IC-04: «un clic del usuario lee directamente») | Fuera de alcance; se conservan |

### Lo que fija el repositorio

| Id | Hallazgo | Consecuencia |
|---|---|---|
| H-09 | Las guardas prohíben `Dispatcher` y `System.Windows` en todo archivo de `Plugin/Workspace` y de `Application/Workspace`, con una excepción por archivo exacto para la paleta que sigue prohibiendo `Dispatcher` (`WorkspaceHostSourceGuardTests.cs:391-405`). La metaguarda fija el texto exacto de esa excepción y 45 pruebas en el archivo (`WorkspacePaletteHostSourceGuardTests.cs:637-657`; `WorkspaceLifecycleGuardTests.cs:682`) | **Una** excepción por archivo exacto (D-T2-06), añadida sin tocar la de la paleta ni el número de pruebas |
| H-10 | `\bTimer\b`, `Task`, `async`, `await`, hilos y `SynchronizationContext` están prohibidos en las carpetas Workspace (`WorkspaceHostSourceGuardTests.cs:407-429`; `WorkspaceBrowserHostGuardTests.cs:270`; `WorkspacePaletteHostSourceGuardTests.cs:240-247`). `\bTimer\b` **no** casa con `DispatcherTimer` | La excepción mantiene prohibidos expresamente `\w*Timer\b`, `DispatcherFrame`, `PushFrame`, `Yield`, `InvokeAsync` e `Invoke` del dispatcher (D-T2C-04) |
| H-11 | La guarda de la fachada exige que `private void OnIdle()` del puente contenga `_core.OnIdle()` y `Drained?.Invoke(` (`WorkspaceBridgeFacadeSourceGuardTests.cs:154-159`). Toda línea del puente con `.Database` tiene que ser un `+=` o un `-=` (`:127-128`) | `OnIdle` se conserva como pasada de drenaje y solo la invoca la continuación. La comprobación Debug usa `document.TransactionManager` |
| H-12 | Hay exactamente 16 manejadores contenidos, 16 `+=` y 16 `-=` en el puente (`WorkspaceBridgeSourceGuardTests.cs:178-184`; `WorkspacePaletteHostSourceGuardTests.cs:384-386`; `WorkspaceLifecycleGuardTests.cs:242-244`; `WorkspaceHostC2SourceGuardTests.cs`), y 3 `+=` en el controlador | Ninguna suscripción ni manejador nuevo, y ningún `+=`/`-=` nuevo de ninguna clase |
| H-13 | La superficie pública de `EventBridgeCore` se fija por reflexión (`WorkspaceBridgeFacadeSourceGuardTests.cs:206-226`) | `CanDrainNow` es el cambio declarado (1) |
| H-14 | `Plugin/Workspace` tiene 9 archivos fijados (`WorkspaceBrowserHostGuardTests.cs:38`; `WorkspacePaletteHostSourceGuardTests.cs:765-774`; `WorkspaceLifecycleGuardTests.cs:679`) | Ningún archivo nuevo en `Plugin/Workspace` |
| H-15 | Las condiciones de `TryDrain` son: sesión no destruida, en reposo, sin modal RackCad, panel visible y al menos una pista (`WorkspaceSession.cs:40-44`). `EventBridgeCore.OnIdle` añade sesión activa, no degradada y presente en el registro (`:389-395`). Una sesión destruida sale del registro (`WorkspaceSessionRegistry.cs:31-37`) aunque no vacíe sus pistas | La elegibilidad en memoria es ese predicado, compartido (D-T2C-03) |
| H-16 | La CI corre las pruebas en Linux y en Debug, y compila el Plugin en Release con `-NoWarnings` en el job `build-plugin` (`.github/workflows/ci.yml:254-317`). Las pruebas de Core no cargan AutoCAD | La temporización de pila se prueba en Core con dobles; el cableado del Plugin, con guardas de texto y el build Release de la CI; el host real, con el smoke o con Smoke-2. Lo que sea solo Debug no puede producir avisos en Release |
| H-17 | `NoHostFileLogsAMeasurementAsAWarning` prohíbe `RackLog.Warning` en las dos carpetas (`WorkspaceHostSourceGuardTests.cs:480-485`); `ThePureHostCodeNeverTouchesTheLog` prohíbe `RackLog` en `Application/Workspace` (`:487-492`) | Ninguna línea nueva de registro de medidas. El planificador registra solo por el delegado `logFault` inyectado |
| H-18 | Trampas para `WorkspaceIdleScheduler.cs` y `EventBridgeCore.cs`:<br>- `Application/Workspace` prohíbe en el código (`CodeOnly`, sin comentarios) las subcadenas `Transaction`, `\bDatabase\b`, `LockDocument` y `\bCommit\(` (`WorkspaceModelBoundaryTests.cs:39-48`);<br>- también prohíbe, sin distinguir mayúsculas, `Provider`, `Metric`, `Aggregat`, `\bBom`, `TotalRacks` y `ProjectSummary` (`WorkspaceHostSourceGuardTests.cs:537-544`; `WorkspaceBridgeSourceGuardTests.cs:362-372`);<br>- la guarda de olvidos de F4-T2B2B exige 13 `_lastAllowedClose = null;`, cada uno justo tras el retorno por no iniciado (`WorkspaceLifecycleGuardTests.cs:99`, `:148-159`);<br>- `:103` prohíbe `_lastAllowedClose` y `_states` en una misma sentencia;<br>- en `WorkspaceHostSourceGuardTests.cs`, la metaguarda exige el literal `string.Equals(path, PaletteHostPath, StringComparison.Ordinal)` (`WorkspacePaletteHostSourceGuardTests.cs:646`), así que la elección de regla no puede pasar a un `switch` | El Worker tiene que respetarlas |
| H-19 | `SetModalRackCadActive` no tiene ningún llamador fuera de las carpetas Workspace. Los comandos RackCad abren ventanas modales con `AcApplication.ShowModalWindow` (`RackSelectivoCommands.cs:40`, `RackPushBackCommands.cs:48` y otros 18) | Dentro de esos comandos el reposo en memoria es falso. Los bucles modales sin comando son el riesgo (D-T2C-03) |
| H-20 | Con la máquina K = 1 vigente, un evento que invalida un resumen **no** refresca una superficie Ready:<br>- `BrowserRackSummaries.OnDrained` solo borra la memoria (`BrowserRackSummaries.cs:85-98`);<br>- `SyncSurface` solo vuelve a materializar una superficie nueva o Pending/Busy (`WorkspaceBrowserViewModel.cs:211-225`) | Hueco de producto preexistente de F3, fuera del alcance de T2C (R-T2C-01). El smoke usa disparadores que sí materializan |
| H-21 | Hay precedente de diferido en el repositorio: `src/RackCad.UI/Editor/DispatcherRecomputeScheduler.cs:17-29` (`CurrentDispatcher` y `BeginInvoke` con prioridad Background) | El mecanismo de D-T2C-04 ya se usa en el producto |

## 3. Alcance, presupuesto y reglas

- **Presupuesto:** `attempts` 1 de 3, propio de la tarea; no hereda nada de F4-T2B2B.
- **Replanificación:** si la primera planificación del Controller falla la fidelidad literal de las colecciones, se permite **una** replanificación reforzada que no consume intentos. Se custodian las dos salidas. Un segundo rechazo vuelve al Owner (F4-E31).
- **Alcance:**
  - programación de las lecturas;
  - separación entre la pila de la notificación y la de la lectura;
  - programación única y agrupada;
  - cancelación y sesión inválida;
  - ausencia de lecturas duplicadas;
  - panel oculto;
  - semántica K = 1 del resumen.
- **Excluido** (orden del Owner):
  - la UI de edición y el texto pendiente;
  - Actualizar y el host de mutación;
  - el rediseño del ciclo de vida de `DraftRegistry`;
  - el foco de T3 y la semántica de escritura de T4;
  - la clausura de dependencias de la ventana;
  - el refresco de una superficie Ready (R-T2C-01).
- **Deudas de protocolo visibles** (no se resuelven aquí):
  - PD-F4-01: el nc2 de lista blanca exacta puede dar un orden engañoso;
  - PD-F4-02: el detector FreeText del Controller dio un falso PASS;
  - PD-F4-03: el Controller puede fallar Scope por un motivo ajeno a la mutación;
  - PD-F4-04: el orden entre Handoff e Identity puede variar.
- **Sustitutos:** no se reutiliza automáticamente ningún sustituto de controles anterior. Si un control repite un modo de fallo, vuelve al Owner antes de cualquier sustituto (F4-E29).

## 4. Decisiones

### D-T2C-01. Todo lo que lee sale de la pila del Idle (decisión del Owner)

- **El callback de Idle solo programa:** no drena, no invoca `Drained`, no abre la ventana de aviso y no lee.
- **La continuación** corre después de que el callback de Idle haya vuelto y, si sigue siendo elegible, hace la pasada de drenaje existente: el `OnIdle` del puente, que llama a `_core.OnIdle()` e invoca `Drained`. Con ella corren:
  - la lectura de selección del drenaje (H-04);
  - la ventana de aviso del controlador y la lectura del resumen de la presentación (H-03, H-05).
- **No cambian:**
  - las operaciones del drenaje (D-04);
  - la ventana de aviso del controlador y del host de navegación;
  - la máquina de la superficie K = 1 del navegador;
  - el controlador, el host de navegación, los lectores, la UI y la paleta.

  Solo cambia **la pila en la que corren**.
- **Requisito del Owner, paso a paso:**
  - evento o Idle → marcar o programar → volver del callback: lo hacen los manejadores (H-07) y `OnIdleTick`;
  - operación posterior elegible: la continuación, que vuelve a comprobar la elegibilidad;
  - validar sesión y documento, reposo y ausencia de modal RackCad: `CanDrainNow`, `TryDrain`, `MayRead` del host y el orden de `ReadSummaryOnActiveDocument`;
  - `DocumentLock`, lectura acotada y desechar todo `DBObject` y transacción: los lectores, que no cambian, más la comprobación Debug de D-T2C-05.

### D-T2C-02. El planificador: `WorkspaceIdleScheduler`, puro, en `Application/Workspace`

Archivo nuevo `src/RackCad.Application/Workspace/WorkspaceIdleScheduler.cs`: `public sealed class`, sin estado estático, sin eventos, sin `RackLog` y sin ningún tipo de AutoCAD, de WPF, de hilos ni de tareas. Respeta las trampas de H-18.

**Constructor:** `WorkspaceIdleScheduler(Func<bool> canRunNow, Action continuation, Action<string, Exception>? logFault = null)`. Los argumentos nulos, salvo `logFault`, lanzan `ArgumentNullException`.

**`void SetPost(Action<Action> post)`:**
- se asigna por asignación; con null lanza `ArgumentNullException`;
- sin `post`, el planificador nunca programa ni ejecuta nada.

**`void OnIdleTick()`** (se llama dentro del callback de Idle), en este orden:
1. si ya hay una continuación programada, no hace nada (coalescencia);
2. si no hay `post`, no hace nada;
3. si `canRunNow()` es falso, no hace nada: no hay bucle de posts mientras no es elegible (punto 9);
4. si no, marca la continuación como programada y llama a `post(Run)` **una** vez.

Nunca llama a `continuation` ni a `Run` directamente. Contención:
- una excepción de `canRunNow` o de `post` se registra y deja el planificador sin nada programado;
- nunca llega al callback.

**`Run`** (privado; solo se entrega a `post`):
1. desmarca la continuación;
2. **vuelve a evaluar `canRunNow()`**: si es falso o lanza, **no hace nada más**. No se llama a la pasada, así que no se drena, no se lee y no se olvida la secuencia de cierre de F4-T2B2B, que la pasada olvida siempre (H-02);
3. si es verdadero, ejecuta `continuation`.

Todo dentro de un `try` que registra con `logFault` y no relanza. Una excepción del propio `logFault` también se contiene. No se reprograma a sí mismo: no hay reentrada en bucle (punto 9, prueba J).

**Consultas sin efectos:**
- `bool IsScheduled { get; }`;
- `int ContinuationsRun { get; }` y `int ContinuationsSkipped { get; }`: contadores en memoria para las pruebas y para un diagnóstico futuro. No se registran.

### D-T2C-03. Elegibilidad en memoria: `EventBridgeCore.CanDrainNow()`

- **Miembro público nuevo**, cambio declarado (1) de H-13: `public bool CanDrainNow()`.
- **Predicado de la pasada.** Un método privado de elegibilidad, compartido con `OnIdle`, exige:
  - núcleo iniciado;
  - sesión activa presente en el registro y no degradada (una sesión destruida sale del registro, H-15);
  - `PendingHintCount > 0`;
  - reposo;
  - sin modal RackCad;
  - panel visible.

  `OnIdle` sigue llamando a `TryDrain`, que vuelve a comprobar las condiciones. El olvido `_lastAllowedClose = null;` de `OnIdle` sigue justo tras el retorno por no iniciado (H-18).
- **Condición propia de `CanDrainNow`:** falso mientras haya **cualquier** cierre permitido pendiente (`_lastAllowedClose.HasValue`), del documento activo o de otro (re-revisión, N-1).
  - El cierre de un documento abre su pregunta de guardar después de `BeginDocumentClose`. Una continuación bombeada dentro de esa pregunta no debe leer el documento que se cierra, ni romper la secuencia de cierre de F4-T2B2B (D-T2B2-10).
  - Solo **lee** el campo; no lo asigna.
  - La condición va en su propia sentencia, nunca en la misma que `_states`, por la trampa de H-18.
  - `OnIdle` no usa esta condición, porque olvida antes. Como `Run` vuelve a comprobar `CanDrainNow`, la pasada no corre mientras el cierre esté pendiente.
  - El bloqueo es acotado: lo levantan la destrucción, que olvida primero, o cualquier entrada del núcleo. Su residual de latencia está en D-T2C-08.
- **Es una consulta:**
  - no muta nada;
  - no asigna `_lastAllowedClose`, así que el recuento de 13 olvidos no cambia;
  - nunca lanza: contiene con `Log` y devuelve false.

### D-T2C-04. Cableado en el Plugin: solo `AutoCadEventBridge.cs`, con la única excepción de `Dispatcher`

- **Manejador de Idle:** `EventHandler handler = (s, e) => Contain("Idle", OnIdleTick);`. La línea sigue contenida, y siguen siendo 16 manejadores, 16 `+=` y 16 `-=`.
- **`private void OnIdleTick()`:** su único cuerpo es `_idleScheduler.OnIdleTick();`.
- **`private void OnIdle()`:**
  - se conserva tal cual (H-11) como pasada de drenaje;
  - **solo** la referencia la continuación que recibe el planificador, por grupo de método dentro de una contención: `() => RunIdleContinuation()`, o equivalente, cuyo cuerpo es `Contain("IdleContinuation", OnIdle)` seguido de la comprobación Debug de D-T2C-05.
- **Construcción:** el planificador se crea **una sola vez**, en el constructor del puente, con:
  - `canRunNow: _core.CanDrainNow`;
  - esa continuación;
  - `LogFault`.
- **El post:**
  - `_idleScheduler.SetPost(action => _dispatcher.BeginInvoke(DispatcherPriority.Background, action))`, una sola vez en el constructor;
  - `_dispatcher` es `Dispatcher.CurrentDispatcher`, capturado en ese constructor, que el controlador ejecuta en el hilo principal durante `RACKPANEL` (precedente: H-21);
  - **solo `BeginInvoke`**: nunca `Invoke`, `InvokeAsync`, `DispatcherTimer`, `DispatcherFrame`/`PushFrame`, `Yield`, `ExecuteInApplicationContext` ni un segundo `Idle +=`.
- **Excepción de guarda por archivo exacto** (cambio declarado (2)), en `NoHostFileUsesTheInternalAutoCadNamespaceOrAnyUiType`:
  - para `AutoCadEventBridge.cs`, el patrón deja de prohibir `\bDispatcher\b` y `System\.Windows\.Threading`;
  - sigue prohibido todo lo demás que ya prohíbe: `Autodesk.AutoCAD.Internal`, `PaletteSet`, `AddVisual`, `RackCad.UI`, `Autodesk.AutoCAD.Windows`, `UserControl` y `System\.Windows(?!\.Threading\b)`;
  - **añade** `\w*Timer\b`, `DispatcherFrame`, `PushFrame`, `\bYield\b`, `InvokeAsync` y `(?:_dispatcher|CurrentDispatcher)\s*\.\s*Invoke\b`;
  - la excepción de la paleta y su texto no cambian, y el archivo sigue con 45 pruebas;
  - G-3 fija el texto exacto de esa expresión, y el archivo nuevo de guardas prueba una copia literal con texto inyectado (I-6).
- **Sin cambios:** `WorkspaceController.cs`, `WorkspaceNavigationHost.cs`, los lectores, la UI y la paleta.

### D-T2C-05. Diagnóstico: solo la comprobación de INV-14, sin instrumentación nueva

- **Lo único nuevo solo para Debug:** la comprobación `TopTransaction == null` que D-02 e INV-14 piden al terminar cada drenaje.
  - Va en la continuación del puente, **después** de `OnIdle()`, cuando ya terminaron los manejadores de `Drained`, la lectura del resumen incluida.
  - Es un método `[Conditional("DEBUG")]` que lee `AcApplication.DocumentManager.MdiActiveDocument?.TransactionManager.TopTransaction`, con `document.TransactionManager` y no `.Database` (H-11).
  - Si hay una transacción abierta, la **registra con `LogFault`** (`RackLog.Exception`); **no** usa `Debug.Assert`. En .NET 8, un `Debug.Assert` fallido sin depurador termina AutoCAD con `Environment.FailFast` (B-2).
  - El texto del registro dice que la transacción puede no ser del panel: en un bucle anidado puede ser de otro complemento (N-5).
- **Sin contador de profundidad, sin `Debug.Assert` y sin `RackLog.Warning`.** El paquete v1 los pedía, y la revisión mostró que:
  - la propia continuación entraba por `Contain`, así que la aserción saltaba siempre;
  - terminaba el proceso;
  - chocaba con H-17.
- **La temporización de pila en el host real** no se instrumenta. Se acredita:
  - en Core, con el arnés de profundidad (§5);
  - en el Plugin, con las guardas G-2, G-3 y G-10 (solo `BeginInvoke`, `OnIdle` solo desde la continuación);
  - en el host, se registra como **limitación para Smoke-2**. La orden lo admite: «record that limitation for Smoke-2 rather than inventing evidence».
- **No queda comportamiento de depuración permanente** salvo la comprobación de INV-14 que el Freeze ya exige. El build Release no la contiene y no produce avisos (H-16).

### D-T2C-06. Puntos 1-13 del Owner

| N.º | Requisito | Cómo se cumple | Prueba o guarda |
|---|---|---|---|
| 1 | Los manejadores de eventos nunca leen el DWG | Sin cambios (H-07) y guarda negativa | A; G-1 |
| 2 | El callback de Idle nunca hace la lectura del resumen | D-T2C-01 y D-T2C-04 | B; G-2, G-10 |
| 3 | Idle puede programar y desconectarse | `OnIdleTick` programa; la continuación drena y `SyncIdle` desconecta cuando no quedan pistas | C; prueba de desconexión |
| 4 | N pistas, una lectura | Coalescencia en `TryDrain` y una sola continuación programada | D |
| 5 | Panel oculto, cero lecturas | `CanDrainNow` falso en el tic y en `Run` | F |
| 6 | Una sesión destruida o inactiva no lee | `CanDrainNow` y la pasada solo ven la sesión activa en ese momento; el resumen exige que la sesión viva y esté autorizada | E |
| 7 | El cambio de documento no lee el documento viejo | Ídem; el lector de selección y el resumen comprueban el documento activo frente a la sesión | G |
| 8 | Comando o modal retrasan | `CanDrainNow` falso en el tic y en `Run`, y también con un cierre permitido pendiente | H; prueba del cierre pendiente |
| 9 | Una lectura fallida no da vueltas en Idle | Sin post si no es elegible; `Run` no se reprograma; las pistas se consumen al drenar | J |
| 10 | Una vez elegible, exactamente una lectura | Una continuación, un drenaje y una materialización K = 1 | C, D, I |
| 11 | Ninguna suscripción por pestaña | 16, 16 y 16 sin cambios; ningún `+=` nuevo | G-6 |
| 12 | Ninguna escritura | Los archivos de producto del cambio no nombran API de escritura | G-8 |
| 13 | `TopTransaction` y `DBObject` limpios | Los lectores no cambian y usan `using` (`WorkspaceBridgeSourceGuardTests.cs:227-234`); comprobación Debug después de la pasada completa (D-T2C-05) | G-9; smoke |

### D-T2C-07. Lo que sustituye esta tarea de textos ratificados (decisión del Owner; lección de encuadre de I-55)

- **D-T2-06** decía «Drenaje: marca y no lee» y «La programa una sola vez la UI». Lo sustituye D-T2C-01:
  - el drenaje conserva su lectura de selección (D-04), pero fuera de la pila del Idle;
  - programa el puente, no la UI;
  - se conserva «una sola vez, agrupada, con una excepción de guarda por archivo exacto para el planificador».
- **La fila de T2C** del paquete de F4-T2 (`:54`) declaraba las capas «UI, Plugin y pruebas». T2C toca Application (el planificador y `CanDrainNow`), el Plugin (el puente) y las pruebas, **no** la UI.
- **Se conserva** la prohibición de modificar la clausura de dependencias de la ventana.

### D-T2C-08. Residuales y arrastres

- **R-T2C-01** (hueco de producto preexistente de F3, H-20): una superficie Ready no se refresca tras un evento que invalida su resumen. Solo una materialización nueva lee: otra pestaña, otro rack seleccionado, el cambio de documento o «Volver a leer». Arreglarlo es un cambio de UI fuera de T2C. **Se lleva al Owner** para que decida si va a F4-T3 o a otra tarea; no se reinterpreta en silencio.
- **R-T2C-02** (re-revisión, N-2): tras un cierre cancelado sin comando, las pistas anteriores al cierre no se drenan hasta la siguiente entrada del núcleo (una interacción con el dibujo, o mostrar u ocultar la paleta). Antes de T2C, el tic de Idle siguiente olvidaba y drenaba. No es un bloqueo indefinido: Idle sigue enganchado con tics en memoria y las lecturas que pide el usuario funcionan. La prueba del cierre pendiente lo fija en Core.
- **Arrastre a F4-T3:** los subdiálogos del panel deben activar `SetModalRackCadActive(true)` **antes** de `ShowModalWindow` (D-02). Desde T2C, una continuación puede bombearse dentro de su bucle modal.
- **Arrastres a Smoke-2:**
  - si la continuación corre dentro de:
    - la pregunta de guardar de CLOSE;
    - el cierre por la pestaña o por la API;
    - QUIT y CLOSEALL con varios dibujos modificados;
    - las ventanas `ShowModalWindow` de RackCad;
    - jigs y avisos de comando;
    - bucles de otros complementos;
  - la latencia y el orden reales de Background, y que la continuación siempre llega: si se perdiera, `IsScheduled` quedaría en true y no habría más drenajes;
  - `LockDocument` desde la continuación durante un cambio o un cierre de documento;
  - si el cierre por la pestaña emite CLOSE como comando;
  - el cierre cancelado y la latencia de R-T2C-02;
  - las aserciones `TopTransaction` existentes (`WorkspaceController.cs:191`, `WorkspaceSelectionReader.cs:115`), ahora alcanzables donde corra la continuación. En un bucle anidado sin comando con una transacción ajena abierta, terminarían AutoCAD con el DLL Debug, y la comprobación nueva daría un falso positivo (N-5);
  - la temporización de pila en el host (D-T2C-05).

## 5. Pruebas y guardas

### Pruebas de comportamiento en Core

Archivo nuevo `tests/RackCad.Tests/Workspace/WorkspaceIdleSchedulerTests.cs`.

**Arnés:** compone las piezas puras como lo hace el Plugin:
- `EventBridgeCore` con la fuente simulada de `WorkspaceBridgeTestSupport`;
- `WorkspaceIdleScheduler` con `canRunNow: core.CanDrainNow`;
- una continuación que llama a `core.OnIdle()` y entrega cada resultado a `WorkspaceHostCore.OnDrained`, con un lector de selección doble;
- después, una **presentación sustituta** que imita el orden de `ReadSummaryOnActiveDocument`:
  - documento activo;
  - reposo;
  - ventana de aviso del arnés abierta solo alrededor del aviso del drenaje, como el controlador;
  - autorización de la sesión;
  - memoria o lectura de `BrowserRackSummaries`, con un lector de hechos doble;
  - contención de una excepción como `Failed`;
- un `post` doble que **encola** y no ejecuta.

La presentación del arnés es un sustituto **declarado** de la UI y del host de navegación, que no cambian. La semántica K = 1 la acreditan, además, `RackCad.UI.Tests` sin cambios.

**Temporización de pila:**
- el arnés envuelve cada callback simulado (pista de objeto, de comando o de selección; tic de Idle; asignación del cierre) en un ámbito de profundidad;
- los lectores dobles registran la profundidad en cada lectura;
- toda prueba afirma que **ninguna lectura ocurrió con profundidad mayor que 0**, además del recuento.

Así se distingue la temporización de la pila, no solo el número de lecturas (orden del Owner). En el arnés, un post doble que ejecuta en el acto, o un planificador que llama a la pasada desde `OnIdleTick`, haría fallar B y C por profundidad. El post **real** del Plugin no lo ejerce Core: lo acreditan G-2, G-3 y G-10 (N-3).

| Prueba | Escenario | Afirmación |
|---|---|---|
| A | Una pista de objeto y una de fin de comando dentro de su callback | 0 lecturas durante el callback y al volver; la cola del post vacía |
| B | Tic de Idle elegible | 0 lecturas dentro del tic y al volver; exactamente 1 continuación encolada |
| C | Se ejecuta la cola, fuera de todo callback | 1 lectura de selección y 1 lectura del resumen, ambas con profundidad 0 |
| D | N pistas (N ≥ 5) y varios tics antes de ejecutar la cola | 1 continuación encolada, 1 drenaje y 1 lectura del resumen |
| E | La sesión se destruye después de programar y antes de la continuación | 0 lecturas; `Run` no llama a la pasada (`ContinuationsSkipped` 1) |
| F | Panel oculto, con pistas | Los tics no programan y no hay lecturas. Si la continuación ya estaba programada y el panel se oculta, `Run` no llama a la pasada y no lee |
| G | Programado para A; cambio a B antes de la continuación | Ninguna lectura de A contra B; solo se lee la sesión activa, si es elegible |
| H | Comando o modal RackCad activos | Ningún post durante N tics y 0 lecturas; también con la continuación ya programada |
| I | El comando termina (reposo) | El tic siguiente programa una vez; al ejecutar la cola, 1 lectura |
| J | Variante 1: el lector de selección lanza | Se contiene; `EventBridgeCore.IsDegraded(session)` es false; el host marca la sesión como fallida, que es el comportamiento vigente (H-04) y no cambia; la continuación no se reprograma; los tics siguientes no programan (no quedan pistas) ni leen |
| J | Variante 2: el lector del resumen lanza | La presentación del arnés lo contiene como `Failed` y no hay más lecturas |

**Además:**
- **Unidades del planificador:**
  - coalescencia;
  - `OnIdleTick` nunca ejecuta;
  - sin `post` no hace nada;
  - `Run` desmarca y vuelve a evaluar;
  - las excepciones de `canRunNow`, de `post`, de la continuación y del propio `logFault` quedan contenidas, y el planificador queda sin nada programado;
  - argumentos nulos;
  - los contadores.
- **Paridad:** `CanDrainNow` frente a `TryDrain` para cada condición.
- **Cierre pendiente:**
  - `CanDrainNow` es falso tras un Allow de la sesión activa **y** tras un Allow de un documento no activo, y lo sigue siendo hasta la entrada siguiente o la destrucción;
  - programado y después no elegible, la continuación no llama a la pasada;
  - una continuación no elegible no rompe `closeSequenceIntact`: el asesor de la prueba recibe `true` tras Allow, continuación y destrucción.
- **`CanDrainNow` no asigna** la secuencia de cierre.
- **Desconexión de Idle** tras el drenaje.

Todas usan `Record.Exception` donde haga falta para que fallen por aserción en el RED.

### Guardas de texto

Archivo nuevo `tests/RackCad.Tests/Workspace/WorkspaceIdleSchedulingGuardTests.cs`, sujeto a la regla I-1 (F4-E32 y F4-E33): guardas negativas o limitadas a los cuerpos propios, con prefijos ordenados o cotas en lugar de conjuntos exactos de archivo entero, y sin prohibir para siempre lo de T3 y T4.

| Guarda | Qué fija |
|---|---|
| G-1 (negativa) | Ningún bloque `case` del puente ni ninguna línea de manejador contiene `(?<![\w.])OnIdle\b` (sin `Tick`), `Drained`, `BeginInvoke`, `_idleScheduler` ni una API de lectura. Excepción: el bloque Idle, cuya línea es `Contain("Idle", OnIdleTick)`. No fija qué más puede hacer un manejador |
| G-2 | `OnIdleTick` contiene `_idleScheduler.OnIdleTick()` y ninguna de `_core.OnIdle`, `Drained`, `OnIdle(`, `LockDocument`, `StartTransaction` ni `BeginInvoke`. `(?<![\w.])OnIdle\b` aparece en el puente exactamente 2 veces (declaración y continuación). `_core\.OnIdle\s*\(` aparece exactamente una vez, dentro de `private void OnIdle()`. `Drained?.Invoke(` solo dentro de `OnIdle` |
| G-3 | El post usa `BeginInvoke` con `DispatcherPriority.Background`, en cualquiera de los dos órdenes de sobrecarga. La expresión de la excepción por archivo exacto de D-T2C-04 está literalmente en `WorkspaceHostSourceGuardTests.cs`. Una copia literal de esa expresión salta con cada texto inyectado: `DispatcherTimer`, `PushFrame`, `InvokeAsync`, `_dispatcher.Invoke(`, `System.Windows.Controls` |
| G-4 | `WorkspaceIdleScheduler.cs` es puro:<br>- sin `Autodesk`, `System.Windows`, `Dispatcher`, `Task`, `async`, `await`, `Thread`, `Timer`, `RackLog`, `event` ni estado estático;<br>- respeta H-18;<br>- en `OnIdleTick`, la única llamada a `post(` va detrás de las tres comprobaciones;<br>- en `Run`, el orden es desmarcar → `canRunNow()` → la continuación |
| G-5 | `CanDrainNow` y `OnIdle` llaman al mismo método privado de elegibilidad. `CanDrainNow` no contiene `TryDrain`, `EnqueueHint`, `SyncIdle` ni `_source.`, y no asigna ningún campo: la autoprueba usa `_lastAllowedClose\s*=(?!=)`, para que la comparación no la haga saltar. Su única condición propia es la del cierre pendiente, `_lastAllowedClose.HasValue` |
| G-6 | Ningún `+=` ni `-=` nuevo en el puente ni en el controlador (cotas existentes: 16 y 3). El planificador no declara eventos |
| G-7 | El método de la comprobación Debug de D-T2C-05 lleva `[Conditional("DEBUG")]`, usa `TransactionManager.TopTransaction` y `LogFault`, y no contiene `Debug.Assert` ni `Debug.Fail`. El puente no contiene `Debug.Assert` |
| G-8 | Los archivos de **producto** del cambio (`WorkspaceIdleScheduler.cs`, `EventBridgeCore.cs`, `AutoCadEventBridge.cs`) no nombran API de escritura: `OpenMode.ForWrite`, `UpgradeOpen`, `Commit(`, `SetImpliedSelection`, `SetCurrentView`, escritura de sobres |
| G-9 | El controlador sigue llamando a `AssertNoOpenTransaction` en `OnDrained` (presencia, no orden). El puente llama a la comprobación Debug después de `OnIdle` en la continuación |
| G-10 (cableado, I-5) | Exactamente un `new WorkspaceIdleScheduler(` en `src`, dentro del constructor del puente, con primer argumento `_core.CanDrainNow`. La continuación llama a `OnIdle` dentro de una contención, directamente o a través de un único método intermedio que la guarda sigue; se apoya en el recuento de G-2. Exactamente una **llamada** `\.SetPost\s*\(` en `src` (no cuenta la declaración del planificador), en el mismo constructor, con `BeginInvoke` y `DispatcherPriority.Background` (N-3) |

Cada guarda lleva su autoprueba con el texto que debe hacerla saltar, salvo la expresión de la excepción de D-T2C-04, que se prueba por copia literal (G-3).

### Cambios declarados en pruebas y guardas existentes (solo estos)

1. `WorkspaceBridgeFacadeSourceGuardTests.cs`, `TheCoreAddsExactlyTwoPublicMembersToWhatT2Left`: la lista gana `CanDrainNow` y el comentario lo explica; el nombre de la prueba se conserva (precedentes de C2 y de F4-T2B2B).
2. `WorkspaceHostSourceGuardTests.cs`, `NoHostFileUsesTheInternalAutoCadNamespaceOrAnyUiType`: la excepción por archivo exacto de D-T2C-04 para `AutoCadEventBridge.cs`, con la constante de su ruta. No se añade ninguna prueba al archivo (sigue con 45), y no cambian la excepción de la paleta ni su texto.

Ninguna otra prueba ni guarda existente cambia ni se debilita.

## 6. Smoke dirigido en AutoCAD (opcional, lo opera el Owner)

La orden pide un smoke dirigido «si es practicable». La sesión no opera AutoCAD (§12.1 del host), así que, si el Owner lo acepta, lo hace con una guía pegada en el chat, después de la verificación del Controller y antes del cierre.

- **DLL:** la Debug del worktree, en el SHA verificado, cargada con NETLOAD según `docs/guias/validacion-manual-autocad.md`. Aviso previo al Owner: con el DLL Debug, cualquier `Debug.Assert` existente que fallara (`TopTransaction` en el controlador y en el lector de selección) terminaría AutoCAD. Se trabaja sobre un dibujo de prueba guardado.
- **Disparadores que materializan de verdad** (H-20):
  1. con el panel visible y PICKFIRST activo, seleccionar un rack distinto (pestaña de vista previa nueva): el resumen aparece tras soltar la selección;
  2. cambiar de documento y volver: el resumen aparece tras el drenaje;
  3. iniciar un comando y dejarlo activo: no se carga nada hasta que termina;
  4. ocultar el panel, seleccionar otro rack mientras está oculto y volver a mostrarlo: aparecen el contexto nuevo y su pestaña. Un resumen Ready del mismo rack **no** se refresca (R-T2C-01, N-6);
  5. CLOSE de un dibujo modificado, con el panel visible y pistas pendientes: el cierre y la pregunta de guardar funcionan sin errores.
- **Observaciones:** comportamiento funcional y ausencia de errores en RackLog. **Este smoke no prueba la temporización de pila en el host.** Esa prueba queda como limitación para Smoke-2 (D-T2C-05), sin inventar evidencia.
- **Evidencia:** las observaciones del Owner y extractos de RackLog sin rutas ni datos de máquina (repo público).
- **Si no es practicable:** todo queda para Smoke-2.

El smoke no forma parte del contrato del Worker.

## 7. Contrato propuesto

- **AllowedWriteScope** (7 archivos exactos):
  1. `src/RackCad.Application/Workspace/WorkspaceIdleScheduler.cs` (nuevo);
  2. `src/RackCad.Application/Workspace/EventBridgeCore.cs`;
  3. `src/RackCad.Plugin/Workspace/AutoCadEventBridge.cs`;
  4. `tests/RackCad.Tests/Workspace/WorkspaceBridgeFacadeSourceGuardTests.cs` (cambio declarado 1);
  5. `tests/RackCad.Tests/Workspace/WorkspaceHostSourceGuardTests.cs` (cambio declarado 2);
  6. `tests/RackCad.Tests/Workspace/WorkspaceIdleSchedulerTests.cs` (nuevo);
  7. `tests/RackCad.Tests/Workspace/WorkspaceIdleSchedulingGuardTests.cs` (nuevo).
- **ForbiddenWriteScope:** prefijos seguros, como en F4-T2B2B:
  - el resto de `Application`, `Domain`, `UI`;
  - el resto del Plugin, incluidos los otros 8 archivos de `Plugin/Workspace`;
  - el resto de las guardas y pruebas existentes;
  - `tests/RackCad.UI.Tests/`;
  - la raíz.

  Esto cubre la clausura de dependencias de la ventana.
- **Invariantes:**
  - **INV-F4T2C-01:** D-T2C-01 y D-T2C-04.
  - **INV-F4T2C-02:** D-T2C-02.
  - **INV-F4T2C-03:** D-T2C-03.
  - **INV-F4T2C-04:** puntos 5 a 8.
  - **INV-F4T2C-05:** puntos 9 y 10.
  - **INV-F4T2C-06:** puntos 11 a 13 y D-T2C-05.
  - **INV-F4T2C-07:** las pruebas A-J y las demás del §5, con temporización de pila.
  - **INV-F4T2C-08:** las guardas G-1 a G-10 con la regla I-1.
  - **INV-F4T2C-09:** los dos cambios declarados y ningún otro.
  - **INV-F4T2C-10:** cobertura RED/GREEN, con el RED como esqueleto inerte que compila y falla por aserción.
  - **INV-F4T2C-11:** sin escritura, sin AutoCAD, sin tocar la clausura de la ventana ni el trato vigente de los fallos del host.
- **RequiredTests:**
  - `FullyQualifiedName~RackCad.Tests.Workspace`, MinSelected 1919 (línea base de `92ad237c`, CI `43294223849`), ExpectRed true;
  - `FullyQualifiedName~RackCad.UI.Tests.Workspace`, MinSelected 248, ExpectRed false.
- **StopConditions:**
  - las S y P comunes;
  - **C01:** un archivo nuevo en `Plugin/Workspace`, o un cambio en otro archivo;
  - **C02:** un cambio en guardas o pruebas existentes fuera de los dos declarados;
  - **C03:** una lectura que siga en un callback de AutoCAD, o un cambio en el controlador, el host de navegación, los lectores o la UI;
  - **C04:** un `Invoke` síncrono, `DispatcherTimer`, `PushFrame` o `ExecuteInApplicationContext`;
  - **C05:** una suscripción o un `+=` nuevo;
  - **C06:** una semántica congelada que cambie, una prueba que necesite AutoCAD, o la UI, un csproj o la CI;
  - **C07:** la tarea se evalúa como Long-horizon;
  - **C08:** la regla de replanificación de F4-E31.
- **ExpectedEvidence:** como en F4-T2B2B, con:
  - las pruebas A-J y su temporización;
  - el build Release del Plugin sin avisos nuevos (la CI del SHA exacto);
  - las revisiones directa e independiente;
  - nc1..nc3 con su oráculo ordinario;
  - la nota del smoke dirigido o de su limitación.

## 8. Puntos para la re-revisión de la v2

1. ¿Cierran D-T2C-02 (`Run` vuelve a comprobar) y D-T2C-03 (cierre pendiente) el riesgo I-1 sin cambiar el comportamiento cerrado de F4-T2B2B?
2. ¿Son implementables y discriminantes G-1 a G-10 y la expresión de la excepción, sin romper guardas no declaradas?
3. ¿Es correcta y suficiente la disposición de B-1, B-2 e I-7 (sin instrumentación nueva, temporización en el host como limitación, R-T2C-01 al Owner)?

## 9. Disposición de la revisión adversarial de la v1

| Hallazgo | Disposición en la v2 |
|---|---|
| B-1 (`RackLog.Warning` choca con H-17) | **Aceptado, opción (a):** sin ninguna línea de registro de medidas; la temporización en el host queda como limitación para Smoke-2 (D-T2C-05, D-T2C-08) |
| B-2 (`Debug.Assert` salta siempre y termina AutoCAD) | **Aceptado:** se retiran el contador de profundidad y la aserción. La comprobación Debug de INV-14 registra con `LogFault` y no termina el proceso. §6 corregido, con el aviso sobre las aserciones existentes |
| I-1 (bucles modales y secuencia de cierre) | **Aceptado:** (a) `Run` vuelve a comprobar `canRunNow`; (b) `CanDrainNow` es falso con un cierre permitido pendiente, registrado como decisión (D-T2C-03); (c) riesgos de Smoke-2; (d) arrastre a F4-T3 (D-T2C-08). Pruebas nuevas en el §5 |
| I-2 (punto 13 sin comprobación tras el resumen) | **Aceptado, opción (a):** comprobación Debug después de la pasada completa, con `TransactionManager`; G-9 fija presencia, no orden |
| I-3 (J contradice el host vigente) | **Aceptado:** J reescrita con el comportamiento vigente (H-04) |
| I-4 (G-1 como lista blanca) | **Aceptado:** G-1 negativa |
| I-5 (cableado sin guarda) | **Aceptado:** G-10 |
| I-6 (huecos de la excepción de `Dispatcher`) | **Aceptado:** la expresión mantiene prohibidos los temporizadores, `PushFrame`, `Yield`, `InvokeAsync` e `Invoke`; G-3 la fija literalmente y prueba una copia |
| I-7 (smoke mal calibrado, K = 1) | **Aceptado:** disparadores que materializan (§6); R-T2C-01 al Owner (D-T2C-08); corregida la variante del panel oculto |
| M-1, M-2 (precisión de G-2, G-3) | **Aceptados** en las guardas |
| M-3 (G-8 sobre sí misma) | **Aceptado:** solo archivos de producto |
| M-4 (`+=` en el contador) | **Sin objeto:** no hay contador; G-6 lo prohíbe igualmente |
| M-5 (trampas) | **Aceptado:** H-18 |
| M-6 (H-16 inexacto) | **Aceptado:** H-16 corregido |
| M-7 (desviaciones de textos ratificados) | **Aceptado:** D-T2C-07 |
| M-8 (presentación sustituta) | **Aceptado:** declarada, imita el orden de `ReadSummaryOnActiveDocument`; K = 1 también por `RackCad.UI.Tests` sin cambios |
| M-9 (vida del planificador; `logFault`; razonamiento) | **Aceptado:** `Run` contiene también `logFault`; H-15 y D-T2C-03 corregidos; la continuación perdida, como riesgo de Smoke-2 con los contadores en memoria |
| M-10 (instrumentación permanente) | **Aceptado:** no hay instrumentación nueva salvo la comprobación de INV-14 que exige el Freeze |

## 10. Disposición de la re-revisión de la v2 (EMITIBLE, 0/0/7)

La re-revisión confirma que B-1, B-2 e I-1 a I-7 quedan cerrados sin defectos nuevos bloqueantes ni importantes. Sus menores quedan así:

| Hallazgo | Disposición |
|---|---|
| N-1 (el cierre pendiente solo cubría la sesión activa) | **Aceptado:** `CanDrainNow` es falso con `_lastAllowedClose.HasValue`; prueba con un documento no activo (D-T2C-03, §5) |
| N-2 (latencia tras un cierre cancelado sin comando) | **Aceptado:** R-T2C-02 y riesgo de Smoke-2 (D-T2C-08) |
| N-3 (precisión de G-10 y G-2; alcance de la prueba del post) | **Aceptado:** se cuentan las llamadas `\.SetPost\s*\(`, G-10 admite un método intermedio y el §5 deja dicho que el post real lo acreditan las guardas |
| N-4 (trampas de `:103`, de la autoprueba de G-5 y de la metaguarda) | **Aceptado:** H-18 y G-5 |
| N-5 (aserciones `TopTransaction` existentes en bucles anidados) | **Aceptado:** riesgo de Smoke-2, y el texto del registro lo advierte (D-T2C-05) |
| N-6 (disparador 4 del smoke) | **Aceptado:** §6 corregido |
| N-7 (H-18 más estricto que la guarda) | **Aceptado:** H-18 corregido |
