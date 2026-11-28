# MAPA 2 de F4-T4C — El contexto de comando, el `DocumentLock` y la frontera de transacción y de Undo

Mapa de hechos medidos para el paquete de contrato de I-64 F4-T4C. Es de solo lectura y se preparó sin AutoCAD. No diseña el paquete: mide, señala opciones con sus pros y contras, y deja aparte los riesgos, las preguntas y las posibles condiciones de devolución.

**Base medida.**
- Rama `architecture/workspace-persistente-rackcad`, HEAD `b15072ff` (custodia del cierre de T4B, solo `docs/`).
- El producto es el de `4314c180`, el GREEN de T4B.
- Todas las anclas `ruta:línea` son relativas al repositorio y están medidas en `b15072ff`, salvo las marcadas como «histórico», que llevan su commit.

**Cómo leer cada hecho.**
- **[M]** medido, con su ancla;
- **[I]** inferencia, que no está medida;
- **[H]** lo que solo puede probar el host (AutoCAD real).

**Abreviaturas de las anclas:**

| Abreviatura | Ruta |
|---|---|
| `FRZ` | `docs/initiatives/I-64-proposal-v5.md` |
| `F4P` | `docs/initiatives/I-64-f4-gate-contract-package.md` |
| `T4P` | `docs/initiatives/I-64-f4-t4-gate-contract-package.md` |
| `T4BP` | `docs/initiatives/I-64-f4-t4b-gate-contract-package.md` |
| `T4BC` | `docs/automation/evidence/I-64-pilot/F4-T4B-MUTATE-POST/task-close-package.md` |
| `ICL` | `docs/initiatives/I-64-implementation-clarifications.md` |
| `DSP` | `docs/automation/evidence/I-64-pilot/F4-DRAFTS/f4-dispositions.md` |
| `EV` | `docs/automation/evidence/I-64-evidence.md` |
| `ONT` | `docs/automation/evidence/I-64-overnight/overnight-dispositions.md` |
| `OF2` | `docs/automation/evidence/I-64-pilot/F3-BROWSER/coordinator/owner-f2-close-ratification-f3-authorization.md` |
| `OD2` | `docs/automation/evidence/I-64-pilot/F4-DRAFTS/coordinator/owner-f4-disposition-2.md` |
| `OT4` | `docs/automation/evidence/I-64-pilot/F4-DRAFTS/coordinator/owner-f4t4-preparation-authorization.md` |
| `SNAV1` | `docs/automation/evidence/I-64-pilot/F2-SMOKE-NAV/R20261110T064058Z-c883` (la parada de Smoke-NAV y sus cuatro diagnósticos) |
| `SNAV2` | `docs/automation/evidence/I-64-pilot/F2-SMOKE-NAV/R20261110T073451Z-e221` (el PASS de Smoke-NAV) |
| `TW` | `tests/RackCad.Tests/Workspace` |
| `PW` / `PV` / `AW` | `src/RackCad.Plugin/Workspace` / `src/RackCad.Plugin/Views` / `src/RackCad.Application/Workspace` |
| `OO` | La orden del Owner de preparación de T4C. Está en el chat y **todavía no está custodiada en el repositorio**; se cita por sus apartados |

---

## 0. Resumen

1. **La prohibición de `ExecuteInCommandContextAsync`.** Hoy está prohibido en el código de **todo `src`**, comentarios fuera (`TW/WorkspaceNavigationHostTests.cs:436`). Además, `async` y `await` están prohibidos en las dos carpetas Workspace, y `Task` y `Dispatcher` en `UI/Workspace` (y `Task` también en `WorkspaceHostCore.cs` y `WorkspaceController.cs`).
   - No existe ninguna excepción por archivo para el adaptador de T4C.
2. **La evidencia de host del callback.** Mide que corre en el comando interno `EXECUTEFUNCTION` y fuera del contexto de aplicación, y que vacía la selección implícita (`*Cancel*` x2).
   - **No mide** si el callback corre antes de que vuelva la llamada.
   - Hay una observación del operador («una navegación en curso durante varios segundos») que **apunta a un callback diferido** [I].
3. **Hoy no hay ninguna marca de Undo** en `src`, ningún `SendStringToExecute` ni ningún `CommandFlags`.
   - **Ningún Undo de un comando de RackCad** está medido en el repositorio.
   - El Undo tras navegar es `UNCHARACTERIZED` (`ICL:55`).
4. **Qué bloqueo toma cada pieza.** El escritor de T4B no toma ningún bloqueo.
   - Solo `CaptureDrawing` y `SyncName` toman un bloqueo propio y anidado.
   - **La importación** (`PrepareRedraw` → `WblockCloneObjects`), **la transacción de MUTATE**, **la purga** y el **regen** dependen del bloqueo exterior.
   - Esto corrige `T4BP:389`, que da la purga por bloqueada.
5. **Escrituras de un Actualizar del piloto:**
   - **una** transacción authored (MUTATE);
   - además, hasta una transacción de purga, hasta N transacciones de `SyncName` y hasta N `WblockCloneObjects`, que van **sin transacción**;
   - y tres lecturas con transacción propia.
6. **Dónde puede vivir el `using LockDocument`.** Varias guardas reducen las ubicaciones posibles:
   - no puede ir en `WorkspaceHostCore.cs` ni en `WorkspaceController.cs` (`TW/WorkspaceLifecycleGuardTests.cs:679-694`);
   - tampoco en `RackPilotUpdateWriter.cs` (`TW/WorkspacePilotUpdateGuardTests.cs:171`);
   - un archivo de `Plugin/Views` no puede nombrar `RackCad.Application.Workspace` (`TW/WorkspaceBridgeSourceGuardTests.cs:361-366`).

---

## 1. `ExecuteInCommandContextAsync`

### 1.1 La guarda que hoy lo prohíbe en todo `src` (H-13)

**H2-01 [M].** La prueba exacta es `TheNavigationRunsSynchronouslyAndNoProductFileUsesTheInternalCommand`, en `TW/WorkspaceNavigationHostTests.cs:426-437`.
- `:436`: `Assert.Empty(Src.AllProduct().Where(f => Regex.IsMatch(f.Code, @"ExecuteInCommandContextAsync"))…)`.
- **Su alcance:** `Src.AllProduct()` es `Files("src")`, todos los `*.cs` de `src` salvo `obj` y `bin` (`TW/WorkspaceHostSourceSupport.cs:48-64`).
- Lee `f.Code`, que es `CodeOnly`: quita los comentarios `//`, `///` y `/* */` (`:29-30`). Por eso el comentario `PW/WorkspaceNavigationHost.cs:17`, que nombra la API, no la dispara.
- La misma prueba fija que no hay `async` ni `await` en el host de navegación, ni en las dos carpetas Workspace (`:431-435`).

**H2-02 [M]. Otras guardas que nombran la API**, con su alcance:

| Guarda | Alcance | Qué prohíbe |
|---|---|---|
| `TW/WorkspaceHostSourceGuardTests.cs:445-455` | El host de navegación | La API, `SendStringToExecute`, `[CommandMethod`, `Internal`, `DllImport` y `HwndSource` (`:454`). Además, `async`/`await` en las dos carpetas Workspace (`:451-452`) |
| `TW/WorkspaceSourceGuardExceptionTests.cs:261-271` | El host de navegación | La API (`:269`) y las formas de bloquear (`:270`). Además, `async`/`await` en las dos carpetas Workspace (`:266-268`) |
| `TW/WorkspaceBrowserHostGuardTests.cs:256-263` | La carpeta `UI/Workspace`, el host y el adaptador de hechos | `SendStringToExecute|CommandMethod|ExecuteInCommandContextAsync` |
| `TW/WorkspacePilotSurfaceGuardTests.cs:278-290` | La carpeta `UI/Workspace` | El alcance del dibujo, con la API (`:280`) |
| `TW/WorkspacePilotCloseSyncGuardTests.cs:851` | El VM del navegador y el code-behind | `DrawingReach`, con la API |
| `TW/WorkspacePilotPrepareGuardTests.cs:407-430` | Los archivos puros de la tarea, más `WorkspaceHostCore.cs` y `WorkspaceController.cs` | La API, `async`, `await`, `\bTask\b`, `Dispatcher` y `Timer`. En los puros, también `.Result` |
| `TW/WorkspacePilotUpdateGuardTests.cs:164-191` | `PV/RackPilotUpdateWriter.cs` | La API, `LockDocument`, `async` y `await`, entre otros (`:171`) |

### 1.2 Las guardas de asincronía en las carpetas Workspace y en `UI/Workspace`

**H2-03 [M]. En `src/RackCad.Plugin/Workspace/` y `src/RackCad.Application/Workspace/`**:
- `TW/WorkspaceHostSourceGuardTests.cs:423-443` prohíbe `Task.Run`, `Task.Factory`, `new Thread`, `ThreadPool`, `Timer`, `async`, `await`, `Parallel.`, `BackgroundWorker` y `SynchronizationContext` (`:426`), y en todos los archivos `GetAwaiter|ConfigureAwait|.Wait(` (`:432`, `:438`);
- prohíbe `.Result` en el Plugin (`:441-442`);
- el host de navegación tiene su propia regla, que además prohíbe `ContinueWith`, `StartNew`, `Task.Delay` y `async void` (`:431`).
- **Las carpetas son recursivas** (`TW/WorkspaceHostSourceSupport.cs:52`). `src/RackCad.Plugin/Views/` **no** entra en estas guardas.

**H2-04 [M]. En `src/RackCad.UI/Workspace/`**, `TW/WorkspacePaletteHostSourceGuardTests.cs:554-581`:
- prohíbe `DispatcherTimer`, `Dispatcher`, `Timer`, `Task`, `async`, `await`, `Thread` y lo demás (`:557`);
- con **una sola excepción por archivo exacto**, `WorkspaceNavigationViewModel.cs`: un `async`, un `await`, un `await _port.ExecuteAsync(` y `public async Task NavigateAsync(` (`:559-579`);
- la regla de planificación de `TW/WorkspacePilotSurfaceGuardTests.cs:283-289` repite la misma excepción.

**H2-05 [M].** **`\bTask\b` está prohibido** en:
- `AW/WorkspaceHostCore.cs` y `PW/WorkspaceController.cs` (`TW/WorkspacePilotPrepareGuardTests.cs:418-429`);
- `IWorkspacePilotEditPort.cs`, `WorkspacePilotSupport.cs`, `WorkspaceEditIntent.cs`, el plan, `DraftSession.cs` y el materializador (`:410-427`);
- el VM del navegador (`TW/WorkspaceBrowserHostGuardTests.cs:271-273`);
- y el VM del campo del piloto, por la regla de la carpeta (H2-04).

**Inferencia [I].** Ninguna operación del puerto del panel, ni del núcleo, ni del controlador, puede devolver ni guardar un `Task`. Si el adaptador de T4C recibe un `Task` de `ExecuteInCommandContextAsync`, tiene que resolverlo dentro de su propio archivo de `Plugin/Views`, sin entregarlo hacia arriba.

### 1.3 Historia: por qué se retiró

**H2-06 [M]. La parada de Smoke-NAV** (`SNAV1`, sobre el candidato histórico `7bdcbfd9`):
- **SN-01** (`operator/session-log.md:10`): al pulsar Seleccionar, la línea de órdenes muestra `*Cancel*` dos veces, el panel vuelve a «Ninguno» y la selección no sobrevive.
- **SN-03** (`:11`): Ver da el mismo resultado y la vista no cambia; `(ssget _I)` = `nil`.
- **El tercer intento** (`:12`): «Hay una navegacion en curso durante varios segundos; despues *Cancel* x2 y Ninguno; vista sin cambios».
- **Por UI Automation** (`:14-15`), sin ratón, el resultado es el mismo. **STOP** en `:16`.

**H2-07 [M]. Los diagnósticos** se hicieron con builds instrumentados que no eran candidato, sin commit y con la fuente restaurada (`SNAV1/operator/session-log.md:19-27`).
- **DIAG-1**, traza de `RackLog` (`SNAV1/logs/diag-all-racklog-raw.txt:1-7`):
  - `ExecuteAsync begin … isAppCtx=True mdiActive=True` a las 07:13:28.940;
  - `callback enter isAppCtx=False cmdNames=EXECUTEFUNCTION` a las .972 (32 ms después);
  - `authorize decision=Accepted`;
  - `after SetCurrentView current center=(50,30)`;
  - `callback exit status=Executed`;
  - `ExecuteAsync end` a las 29.091.
- **DIAG-2** (`:8-15`, sin devolver el foco) da 48 ms entre `begin` y `enter`, y el mismo `*Cancel*` x2 (`session-log.md:22`).
- **DIAG-3**, en contexto de aplicación con `LockDocument` y sin la API (`:23-24`): sin `*Cancel*`, y la selección se conserva; **la vista no cambia**.
- **La causa** (`:25`): `SetCurrentView` va dentro de una transacción que se cierra sin `Commit`, y la vista se revierte; `ZoomToRack` confirma su transacción. La pérdida de la selección se debe a `ExecuteInCommandContextAsync`.
- **DIAG-4** (`:26-27`), en contexto de aplicación con bloqueo y con la vista fuera de la transacción de lectura:
  - Ver, Centrar, Localizar y Seleccionar funcionan; `sslength` = 4;
  - `DBMOD` = 16, con el bit 1 en 0;
  - el foco vuelve al dibujo.

**H2-08 [M]. Las disposiciones.**
- `ONT:70-83` (D-08) registra las causas (1) y (2) de H2-07 y descarta el foco (3).
  - Elige la corrección estrecha F2-T2-HOST-C1: contexto de aplicación, sin comando interno, sin comando en curso y con el documento bloqueado (`:79`).
  - Su riesgo: la navegación «deja de pasar por un comando, de modo que no entra en la pila de comandos de AutoCAD» (`:83`).
- `ONT:85-95` (D-09): Smoke-NAV PASS sobre `003be859`.
  - **SN-08 = LIMITADA**: «la ejecución síncrona no deja intervalo en el que una petición quede pendiente» (`:89`).
- **IC-01** (`ICL:5-41`) ratifica la sustitución **solo** para Seleccionar, Ver, Centrar y Localizar vista.
  - **No** se aplica a Actualizar ni a las rutas de escritura de F4 (`:17`).
  - Fija los 15 invariantes (`:21-39`). El 5 es «`DocumentLock` durante la operación» (`:29`).
- La ratificación literal del Owner (`OF2:24-48`; `EV:5938`) dice que «It does NOT automatically apply to: Actualizar; authored writes; … F4 write paths».
- **IC-02** (`ICL:51`, `:55`): SN-08 queda «LIMITED», y «Deshacer y Rehacer tras navegar la vista» quedan **UNCHARACTERIZED**.

**H2-09 [M]. La reintroducción para Actualizar.**
- `F4P:32` (H-13): «C-F4-02 reintroduce una API retirada: excepción por archivo exacto y aviso al Owner».
- `F4P:59` (C-F4-02): `ExecuteInCommandContextAsync` como vía declarada, con «excepción por archivo exacto», la medición de escrituras y de Undo, y «si UNDO no es un solo paso: A-n material hacia comando registrado (D-13)».
- `DSP:107-109` (F4-E09) y `OD2:246-262`: ratificada «for the explicit Actualizar mutation … intentionally DIFFERENT from the F2 navigation host clarification».
  - El motivo: «Actualizar is an authored DWG mutation and the Freeze requires the write boundary to execute through the controlled mutation path».
- `OT4:121-125` y `DSP:1694` (F4-E51): «RATIFIED and REQUIRED … Limit the exception to exact T4 mutation adapter files».
- `DSP:1701`: «La medición en host de Undo es el primer paso de T4C». `DSP:1707`: «no hay ninguna API de Undo ni de contexto de comando».
- `OO`, «EXECUTE IN COMMAND CONTEXT»: «REQUIRED … only the exact Actualizar host/mutation integration path. Do not migrate unrelated Workspace reads/navigation into command context».

### 1.4 Lo que mide y lo que no mide la evidencia de host sobre el callback

**H2-10 [M]. Lo que está medido** (H2-07):
- el callback corre con `IsApplicationContext` = falso y con `EXECUTEFUNCTION` como orden en curso;
- dentro del callback se pudo autorizar la sesión contra `MdiActiveDocument` (`Accepted`). La vía de los hechos y de los efectos tomaba allí su propio `using (document.LockDocument())` (histórico, `git show 7bdcbfd9:src/RackCad.Plugin/Workspace/WorkspaceNavigationFacts.cs:34` y `…WorkspaceNavigationEffects.cs:54`, `:91`), sin fallar: la traza llega a `Executed`;
- se pierde la selección implícita (PICKFIRST).

**H2-11 [M]. Lo que no está medido.** La instrumentación registró `begin` antes del `await` y `end` después (`SNAV1/logs/diag-all-racklog-raw.txt:1`, `:7`).
- Ninguna línea registra el instante en que **vuelve la llamada** a `ExecuteInCommandContextAsync`, ni si el `Task` devuelto ya estaba completo.
- El código histórico lo esperaba con `await` (`docs/automation/evidence/I-64-pilot/F2-T2A-NAVIGATION-HOST/2/R20261109T194127Z-31c0/green-attempt2.patch:679-695`).
- **Por tanto, «el callback corre antes de que vuelva la llamada» no está medido.**

**H2-12 [I]. Indicio de un callback diferido.** El texto «Hay una navegación en curso.» es el estado `Busy` que el VM pone antes de `await _port.ExecuteAsync` (histórico, `git show 7bdcbfd9:src/RackCad.UI/Workspace/WorkspaceNavigationViewModel.cs:226-238`, `:331`).
- Que el operador lo viera «durante varios segundos» (`SNAV1/operator/session-log.md:12`) implica que el hilo de la UI pudo pintar mientras el `Task` seguía pendiente.
- Lo más probable es que la llamada volviera **antes** de que corriera el callback, segundos antes en ese intento.
- En DIAG-1 y DIAG-2 el intervalo fue de 32 a 48 ms, lo que no distingue los dos casos.
- La alternativa (un bucle de mensajes anidado que pinta durante una ejecución síncrona) no está descartada. **[H]** Solo el host lo decide.

### 1.5 La excepción por archivo exacto de C-F4-02 y R-T4-05

**H2-13 [M]. Lo que dice la regla.** `T4P:547-550` (R-T4-05):
- «`ExecuteInCommandContextAsync` solo en el archivo exacto del adaptador, nuevo en `Plugin/Views`»;
- «las carpetas Workspace siguen sin `async`, `await` ni `.Result`»;
- y «el primer paso de T4C es medir en host si el callback corre antes de que vuelva la llamada y si un Actualizar da un solo paso de Undo. Si no es así, va una A-n al Owner (D-13)».

**H2-14 [M]. Lo que hoy obliga a cambiar la excepción:**
- `TW/WorkspaceNavigationHostTests.cs:436` tiene que pasar de «vacío» a «exactamente `[<ruta del adaptador>]`», con una autoprueba en negativo.
  - El precedente del patrón es la excepción por archivo exacto de la UI (`TW/WorkspacePaletteHostSourceGuardTests.cs:559-579`), con cuentas exactas.
  - También lo son los refinamientos por ruta completa de `TW/WorkspaceBridgeSourceGuardTests.cs` y `TW/WorkspaceSourceGuardExceptionTests.cs:251-259`, que comparan con `string.Equals(path, …)` y nunca por prefijo.
- Las otras guardas de H2-02 no se tocan si el adaptador vive en `Plugin/Views` y no es el escritor.

**H2-15 [M]. Dos guardas condicionan la forma del adaptador en `Plugin/Views`:**
- **Ningún archivo del Plugin fuera de `src/RackCad.Plugin/Workspace/` puede nombrar** `AutoCadEventBridge`, `RackCad.Plugin.Workspace` ni `RackCad.Application.Workspace` (`TW/WorkspaceBridgeSourceGuardTests.cs:361-366`).
  - `TryUpdatePilot`, `EditIntentRequest`, `EditUpdateOutcome` y `SessionId` viven en `RackCad.Application.Workspace` (`AW/WorkspaceHostCore.cs:805`; `AW/WorkspaceEditIntent.cs:381-419`).
- **El escritor no puede nombrar** la API, `LockDocument`, `async` ni `await` (`TW/WorkspacePilotUpdateGuardTests.cs:171`), así que el adaptador no puede ser `RackPilotUpdateWriter.cs`.

**Inferencia [I].** El adaptador de `Plugin/Views` tiene que ser agnóstico del Workspace.
- **El llamador** sería un archivo de `Plugin/Workspace`, que sí puede nombrar los tipos del Workspace y `RackCad.Plugin.Views`; es el precedente de `PW/WorkspaceNavigationHost.cs:5` y `:242`.
- **El adaptador** recibiría, por ejemplo, un delegado síncrono que ejecuta `TryUpdatePilot` dentro del callback.
- **Los archivos de `Plugin/Workspace`** no pueden usar `async`, `await` ni `.Result`, pero sí nombrar `Task`, salvo `WorkspaceController.cs` (H2-05).

**Opciones para la ubicación** [I]:

| Opción | Pros | Contras |
|---|---|---|
| (a) El adaptador en `Plugin/Views`, agnóstico, con un delegado; el llamador en `Plugin/Workspace` | Cumple la letra de R-T4-05 y no toca H2-15 | Una costura de delegado más, que se prueba solo por guarda de fuente (H2-40) |
| (b) El adaptador en `Plugin/Workspace` | Nombra directamente el núcleo | Contradice «nuevo en `Plugin/Views`». Exige excepciones de `async` o `Task` en las carpetas Workspace, prohibidas por R-T4-05 |
| (c) Ampliar `TW/WorkspaceBridgeSourceGuardTests.cs:361-366` con una excepción por archivo exacto | Un adaptador directo | Es un cambio de guarda que hay que declarar, y abre la frontera del Workspace en `Plugin/Views` |

---

## 2. Por dónde entra hoy una acción del panel en el Plugin

### 2.1 La cadena, el hilo y el contexto

**H2-16 [M]. La cadena:**
- **La paleta** es un `PaletteSet` con `AddVisual("Workspace", _root)`. El VM recibe `_controller.PilotEdit` (`PW/WorkspacePaletteHost.cs:46-53`).
- **El controlador** entrega al panel un `PilotEditAdapter`, que solo reenvía (`PW/WorkspaceController.cs:278-318`). Por debajo está la compuerta de avisos, `new WorkspacePilotEditGate(_host, _noticeDepth)` (`:51`), que responde `Waiting` dentro de un aviso (`AW/WorkspacePilotSupport.cs:170-171`).
- **Un ejemplo, «Comprobar ahora»:**
  1. `WorkspacePilotFieldViewModel.CheckNow()` llama **en síncrono** a `_port.TryCheckNow(request)` o a `_port.TryConfirmBase(request)` (`src/RackCad.UI/Workspace/WorkspacePilotFieldViewModel.cs:397-440`, `:411`);
  2. la compuerta y el adaptador lo pasan al núcleo, `IWorkspacePilotEditPort.TryCheckNow` (`AW/WorkspaceHostCore.cs:957-969`), con la marca `_pilotReadInProgress`;
  3. el núcleo llama al lector inyectado (`PW/WorkspaceController.cs:105`, `_host.SetEditIntentReader(...)`);
  4. y el lector es `WorkspaceNavigationHost.PilotIntentReader.ReadOnActiveDocument` (`PW/WorkspaceNavigationHost.cs:220-253`).
- **El hilo [M]:** todo es síncrono sobre el hilo de la UI de AutoCAD.
  - DIAG-1 mide `isAppCtx=True` al entrar en el host desde un botón de la paleta (`SNAV1/logs/diag-all-racklog-raw.txt:1`).
  - Las guardas de H2-03 prohíben cualquier hilo o tarea en las carpetas Workspace.
- **Comandos registrados [M]:** ninguna acción del panel tiene uno. Solo existen `RACKPANEL` y `RACKPANELDESCARTAR` (`PW/WorkspacePanelCommand.cs:22-55`).
  - `DeclaresEntryPoint` fija en el adaptador de comandos exactamente un `RACKPANEL` y como mucho un `RACKPANELDESCARTAR` (`TW/WorkspaceBridgeSourceGuardTests.cs:345-357`).
  - El censo de `[CommandMethod(` en el Plugin es exactamente **40** (`tests/RackCad.Tests/SelectiveEditorOpenTests.cs:564-572`).
  - **Ningún comando usa `CommandFlags`** (no aparece en `src`), y ninguno usa `IsApplicationContext` ni `DocumentLockMode`.
- **Hoy no existe una operación de Actualizar en el puerto del panel**: `TryUpdatePilot` no tiene llamador de producto (`T4BC:9`; `AW/WorkspaceHostCore.cs:797-804`).

### 2.2 Cómo toman `LockDocument` las operaciones actuales del piloto

**H2-17 [M]:**

| Operación | Comprobaciones previas | Bloqueo | Transacción | Ancla |
|---|---|---|---|---|
| Lectura de la intención («Comprobar ahora», «Conservar», recarga) | `MdiActiveDocument`; `CommandInProgress` vacío **y** `IsQuiescent`, si no `Busy`; el catálogo **fuera** del bloqueo | `using (document.LockDocument())`, y dentro `_authorize`, `Revalidate` y `RackPilotIntentAdapter.Read` | La de `CaptureDrawing` (propia, con `Commit`) | `PW/WorkspaceNavigationHost.cs:222-252` |
| `RackPilotIntentAdapter.Read` | `AssertNoOpenTransaction` en Debug, antes y después | **Ninguno propio**: el comentario dice que corre «INSIDE the lock that the operation of the host already holds» (`PV/RackPilotIntentAdapter.cs:66`). Pero llama a `CaptureDrawing`, que **sí** toma el suyo | La de `CaptureDrawing` | `PV/RackPilotIntentAdapter.cs:94-142`, `:116` |
| `ReadPreviewWitness` | — | El suyo | Propia, sin `Commit` (se aborta, solo lee) | `PV/RackPilotIntentAdapter.cs:149-189`, `:160-161` |
| `RackSiblingScan.CaptureDrawing` → `Traverse` | — | El suyo, anidado | `StartTransaction` + `TraverseCore` + **`Commit`** de una transacción que solo lee | `PV/RackSiblingScan.cs:95-110` |
| La navegación (Seleccionar, Ver, Centrar, Localizar) | El documento, `CommandInProgress` y `IsQuiescent` | El exterior en el host, y otro **anidado** en los hechos y en los efectos | De lectura, y la vista se escribe después | `PW/WorkspaceNavigationHost.cs:154-171`; `PW/WorkspaceNavigationFacts.cs:56-57`; `PW/WorkspaceNavigationEffects.cs:54`, `:96` |
| `TryUpdatePilot` (T4B) | Su puerta propia **sin reposo**: el documento activo, el modal y `Authorize` | **Ninguno**: el núcleo no puede nombrarlo (H2-31) | Las de su puerto | `AW/WorkspaceHostCore.cs:805-933` |

**H2-18 [M]. El reposo dentro del contexto de comando.**
- La lectura del host rechaza con `Busy` si `CommandInProgress` no está vacío (`PW/WorkspaceNavigationHost.cs:225-226`).
- `HostProbe.IsActiveDocumentQuiescent` exige `Editor.IsQuiescent` y que el puente no vea ningún comando activo (`PW/WorkspaceController.cs:367-373`).

**Inferencia [I].** Dentro del callback (`EXECUTEFUNCTION`, H2-10) esas dos comprobaciones fallarían siempre. **Por eso:**
- la lectura del host no se puede usar dentro del contexto de comando;
- la puerta de reposo tiene que ir **antes** de entrar, como ya prevé `T4BP:557` («La puerta de reposo pasa a la operación del puerto de T4C, antes de entrar en el contexto de comando»);
- y la lectura de O-04 dentro del callback es la de la corrida de T4B (`ReadForPrepare` → `CaptureDrawing`, sin puerta de reposo; `PV/RackPilotUpdateWriter.cs:74-86`), como pide `T4P:553`.
- **[H]** El valor de `CommandInProgress` y de `IsQuiescent` dentro del callback.

### 2.3 El patrón con bloqueo que fijó F2 para navegar

**H2-19 [M].** «Vista en transacción sin `Commit` se revierte»:
- lo midió DIAG-3 (H2-07);
- quedó en el código como un comentario y una forma fija: la transacción **solo lee la extensión** y la vista se escribe después de cerrarla, dentro del bloqueo (`PW/WorkspaceNavigationEffects.cs:54-61`, `:87`, `:96-114`);
- el contrato son los 15 invariantes de IC-01 (`ICL:21-39`).

**Lo que implica para T4C:**
- **[M]** `Post` comprueba primero que `TopTransaction == null` (`PV/RackPilotUpdateWriter.cs:157-158`). Libera la lectura de nombres, una `StartOpenCloseTransaction` sin `Commit` (`:172-179`), **antes** del regen (`:183-190`).
  - Así, el regen no queda dentro de una transacción sin confirmar. Lo fija la guarda de orden (`TW/WorkspacePilotUpdateGuardTests.cs:217-223`).
- **[M]** `BeginMutation` rechaza una `TopTransaction` ajena con `TOP_TRANSACTION_OPEN` (`PV/RackPilotUpdateWriter.cs:140-141`). Con eso la corrida da NotAppliedPreflight (`src/RackCad.Application/Systems/PushBack/Pilot/PushBackPilotUpdateRun.cs:331-336`).
- **[I]** T4C **no puede** envolver la corrida en una transacción propia: MUTATE se rechazaría, y un regen o un efecto que quedaran dentro se revertirían.
- **[H]** Que `EXECUTEFUNCTION` no tenga una `TopTransaction` abierta al entrar el callback.

---

## 3. La propiedad del `DocumentLock`

### 3.1 Qué toma un bloqueo propio y qué depende del exterior

**H2-20 [M].** R-T4-06 pide «un único `using LockDocument` alrededor de PREPARE, MUTATE y POST» (`T4P:552`; `FRZ:510-511`).
- El escritor de T4B «no toma `LockDocument`»: el `using` único es del adaptador de T4C (`T4BP:389`; `PV/RackPilotUpdateWriter.cs:26-27`).
- La guarda `TW/WorkspacePilotUpdateGuardTests.cs:171` lo fija.

| Pieza que llama la corrida | ¿Bloqueo propio? | ¿Escribe? | Ancla |
|---|---|---|---|
| `RackSiblingScan.CaptureDrawing`/`Traverse`, en `ReadForPrepare` y en `Post` | **Sí**, anidado (`:103`) | No: confirma una transacción que solo lee | `PV/RackSiblingScan.cs:95-110` |
| `RackSiblingScan.TraverseCore`, en `Reread` | No | No | `PV/RackSiblingScan.cs:112`; `PV/RackPilotUpdateWriter.cs:243` |
| `*DrawService.PrepareRedraw` → `ViewBlockDraw.PrepareRedraw` → `BlockLibraryImporter.EnsureForPlan` | **No.** El comentario dice «Must be called inside the document lock» (`ViewBlockDraw.cs:63`) | **Sí**: `WblockCloneObjects` | `src/RackCad.Plugin/Systems/PushBack/PushBack{Planta,System,Frontal}DrawService.cs:51-52`, `:58-60`, `:56-58`; `src/RackCad.Plugin/Systems/Shared/ViewBlockDraw.cs:59-89` |
| `BeginMutation`, `WriteUnit` y `Seal` (MUTATE) | No | **Sí** (authored) | `PV/RackPilotUpdateWriter.cs:134-145`, `:250-295` |
| `SystemBlockWriter.PurgeAfterCommit` → `LateralHeaderDrawer.PurgeUnreferenced` | **No** | **Sí** (borra BTR huérfanos) | `src/RackCad.Plugin/Systems/Shared/SystemBlockWriter.cs:147-150`; `src/RackCad.Plugin/Drawing/LateralHeaderDrawer.cs:193-238` |
| `RackBlockRenamer.SyncName` | **Sí**, anidado (`:32`) | **Sí**, solo si renombra (`:50`) | `src/RackCad.Plugin/Drawing/RackBlockRenamer.cs:21-58` |
| La lectura de nombres de `Post` | No | No (`StartOpenCloseTransaction`) | `PV/RackPilotUpdateWriter.cs:172-179` |
| `SystemBlockWriter.ApplyRegen` | **No** (`Editor.Regen()`) | No, en la base de datos [I] | `src/RackCad.Plugin/Systems/Shared/SystemBlockWriter.cs:157-163` |

**H2-21 [M]. Corrección a `T4BP:389`.** Dice: «Los bloqueos anidados de `CaptureDrawing`, `SyncName` y **la purga** ya existen».
- **La purga no toma bloqueo** (`LateralHeaderDrawer.cs:193-238`; `SystemBlockWriter.cs:147-150`).
- Tampoco lo toman la importación ni el regen.
- Esas tres escrituras (importación, MUTATE y purga) dependen del bloqueo exterior de T4C.

**H2-22 [I]. Sin el bloqueo exterior**, en contexto de aplicación, las escrituras darían `eLockViolation`:
- la de la importación la **traga** `BlockLibraryImporter`: la registra y devuelve 0 (`src/RackCad.Plugin/Drawing/BlockLibraryImporter.cs:126-131`). Por tanto aparecería después como piezas faltantes;
- la de MUTATE daría NotAppliedFailure.
- **En contexto de comando,** el documento podría estar bloqueado ya por el comando interno, y la falta del bloqueo explícito **quedaría oculta**.
- **[H]** El estado de bloqueo del documento dentro de `EXECUTEFUNCTION`.
- Que el `using` exista exactamente una vez solo lo puede fijar una guarda de fuente (§5.1).

### 3.2 ¿Es reentrante el bloqueo del mismo documento?

**H2-23 [M]. Evidencia indirecta de que lo es**, sin una prueba dedicada:
- **En contexto de aplicación, anidado:** el candidato del PASS de Smoke-NAV, `003be859`, ya tenía:
  - el bloqueo exterior del host (histórico, `git show 003be859:src/RackCad.Plugin/Workspace/WorkspaceNavigationHost.cs:92`);
  - y bloqueos anidados del mismo documento en los hechos (`…WorkspaceNavigationFacts.cs:34`) y en los efectos (`…WorkspaceNavigationEffects.cs:54`, `:96`).
  - SN-01, SN-03 y SN-04 dan PASS (`SNAV2/operator/operator-record.md:8`, `:10-11`).
- **Dentro de `EXECUTEFUNCTION`:** los bloqueos de los hechos y de los efectos del candidato histórico no fallaron en DIAG-1 (H2-10).
- **En comandos registrados** (el precedente de producto, sin un registro de medición propio):
  - RACKEDITAR corre con el documento bloqueado por el comando;
  - dentro, cada `RedrawInPlace` toma `LockDocument` (`src/RackCad.Plugin/Systems/Shared/SystemBlockWriter.cs:77`) y cada `SyncName` también (`RackBlockRenamer.cs:32`) (`src/RackCad.Plugin/RackPushBackCommands.cs:279-282`, `:297-300`, `:336-339`).
- **El riesgo ya se había registrado:** F4-E33 pasó «bloqueos anidados» y «DBMOD y UNDO tras el `Commit` de lectura del barrido» a Smoke-2 (`DSP:690`, `:755-756`).
- El mapa 1 de T4B ya lo anotó como inferencia: «Los bloqueos anidados son reentrantes en AutoCAD, pero no se ha medido aquí» (`docs/automation/evidence/I-64-pilot/F4-DRAFTS/t4b/map-1-plugin-write-path.md:427`).
- **[I]** Es reentrante para el modo por defecto (`LockDocument()` sin argumentos, el único que se usa en `src`).
- **[H]** La combinación exacta de T4C: el comando interno, el `using` exterior, y dentro `CaptureDrawing`, `WblockCloneObjects`, MUTATE, la purga, `SyncName` y otro `CaptureDrawing`.

### 3.3 ¿Cómo se garantiza que se bloquea el `Document` que se muta?

**H2-24 [M]. La cadena de identidad del documento hoy:**
1. `TryUpdatePilot.UpdateGate` lee `_probe.ActiveDocumentNativeId`, que es `MdiActiveDocument.UnmanagedObject.ToInt64()` (`PW/WorkspaceController.cs:356-363`).
   - Exige `Authorize(session) == Accepted` contra ese documento (`AW/WorkspaceHostCore.cs:908-933`; `Authorize` en `:247-248`).
   - Pasa el id en `PushBackPilotUpdateRequest.AuthorizedDocumentNativeId` (`:856`).
2. `RackPilotUpdateWriter.Begin` **vuelve a leer** `AcApplication.DocumentManager.MdiActiveDocument` (`PV/RackPilotUpdateWriter.cs:37`).
   - Calcula `_documentMatches` **una vez**, en el constructor del ámbito (`:68-72`).
3. `ReadForPrepare` devuelve `DocumentMatches = false` si no coincide (`:77`), y la corrida da NotAppliedPreflight(`DocumentMismatchReason`) (`PushBackPilotUpdateRun.cs:61`, `:290`).
4. `PrepareUnits`, `BeginMutation` y `Post` usan el `_document` capturado (`:95`, `:139`, `:153`).

**Inferencia [I].** El documento mutado es el `MdiActiveDocument` del momento de `Begin`.
- El bloqueo de T4C se tiene que tomar sobre **ese mismo** objeto. Hoy son dos resoluciones independientes de `MdiActiveDocument`: la del adaptador de T4C y la del escritor. Solo coinciden porque nada cambia el documento activo dentro de un callback síncrono.
- **Ninguna guarda ni prueba fija hoy** que el documento bloqueado sea el escritor.

**H2-25 [M]. La guarda de T4B impide pasar el documento al puerto:**
- prohíbe en la clase del puerto cualquier campo o cualquier mención de `Document`, `ObjectId`, `Transaction`, `Database` o `PreparedViewRedraw` (`TW/WorkspacePilotUpdateGuardTests.cs:181-184`);
- su autoprueba inyecta justamente `private Document _document;` y espera que dispare (`:267-276`).
- `OO`, «RUN-SCOPED WRITER», pide que el escritor «receives only the exact current document/context».

**Opciones** [I]:

| Opción | Pros | Contras |
|---|---|---|
| (a) Se conserva la resolución por `MdiActiveDocument` y la comparación por id nativo. T4C bloquea el `MdiActiveDocument` cuyo id coincide con el de la sesión, y una guarda o una prueba fija la igualdad: el id del bloqueado es el `AuthorizedDocumentNativeId` | T4B no cambia | El «receives only» de la orden se cumple solo por construcción, no por tipo |
| (b) Pasar el `Document` al escritor (un constructor por corrida) | Una sola resolución | Rompe la guarda de T4B (H2-25), con un cambio declarado en un archivo de T4B. Hay un precedente con `Document` en el constructor: `PV/RackProjectionWriteScope.cs:17-21` |
| (c) Un ámbito que posee el bloqueo (precedente `PV/RackProjectionWriteScope.cs:23-37`, que guarda el `DocumentLock` en el ámbito) | El bloqueo vive exactamente lo que el ámbito | El escritor prohíbe `LockDocument` (`:171`). Además, el ámbito de T4B no abarca la puerta de `TryUpdatePilot` |

**H2-26 [M]. Un documento cerrado o equivocado da cero escrituras**, por tres capas:
- `UpdateGate`: sin documento activo da `Rejected`, «No hay un documento activo», con `EditGate.Waiting` (`AW/WorkspaceHostCore.cs:913-917`); con otra sesión, `NotThisDocumentReason` (`:925-929`);
- el `_documentMatches` del escritor (H2-24);
- y la clasificación de la corrida.

**[I]** Si T4C conserva una referencia a un `Document` desde la admisión hasta el callback y ese documento se cierra, `LockDocument()` sobre él lanzaría. Resolver el documento **dentro** del callback evita cruzar la frontera con el objeto.

### 3.4 Dónde puede vivir el `using LockDocument`

**H2-27 [M]:**

| Archivo | ¿Puede tomar `LockDocument`? | Ancla |
|---|---|---|
| `AW/WorkspaceHostCore.cs` y `PW/WorkspaceController.cs` | **No** (`WriteApis` incluye `LockDocument`, `StartTransaction` y `.Commit(`) | `TW/WorkspaceLifecycleGuardTests.cs:679-694` |
| El núcleo, el puerto y el soporte | **No** (también en comentarios, en tres archivos) | `T4BP:108-109`; `TW/WorkspacePilotEditGuardTests.cs:812-849` |
| `PV/RackPilotUpdateWriter.cs` | **No** | `TW/WorkspacePilotUpdateGuardTests.cs:171` |
| `PW/WorkspaceNavigationHost.cs` | Sí, dos veces hoy (`:163`, `:232`); las guardas fijan el orden y las clases | `TW/WorkspaceNavigationHostTests.cs:456-470`; `TW/WorkspaceEditIntentGuardTests.cs:405-457` |
| Un archivo nuevo de `Plugin/Views` | No está prohibido por guarda | — |

**[I]** El bloqueo cabe naturalmente en el adaptador nuevo de `Plugin/Views` (opción (a) de §1.5), dentro del callback, alrededor de la llamada al delegado que ejecuta `TryUpdatePilot`.
- Por la forma de `TryUpdatePilot`, todo PREPARE, MUTATE y POST, la ranura y el `CompleteApply` en `finally` (`AW/WorkspaceHostCore.cs:851-866`) quedan dentro del `using`.
- La puerta y la comprobación previa de `TryUpdatePilot` (`:814-834`) también quedarían dentro.

---

## 4. La frontera de transacción y de Undo del escritor de T4B

### 4.1 Cada transacción y cada escritura, en orden

**H2-28 [M].** Un Actualizar con N miembros, por `PushBackPilotUpdateRun.Execute` (`src/RackCad.Application/Systems/PushBack/Pilot/PushBackPilotUpdateRun.cs:132-146`, `:248`, `:288`, `:321`, `:331`, `:376`, `:405`, `:411`, `:448`):

| # | Operación | Transacción | ¿Escribe en el DWG? | Bloqueo propio | Ancla |
|---|---|---|---|---|---|
| 1 | `Begin`: el ámbito y el documento | — | No | No | `PV/RackPilotUpdateWriter.cs:34-38` |
| 2 | `ReadForPrepare`: el catálogo (`LateralHeaderDrawService.LoadCatalog`, archivo y caché) | — | No | No | `:81` |
| 3 | `ReadForPrepare`: `CaptureDrawing` | **T1**: `StartTransaction` + `Commit` (solo lee) | No | **Sí** | `:82`; `PV/RackSiblingScan.cs:103-108` |
| 4 | `PrepareUnits`, por miembro: `PrepareRedraw` → `RackCatalogLoader.Load` | — | No | No | `src/RackCad.Plugin/Systems/Shared/ViewBlockDraw.cs:84` |
| 5 | Por miembro: `EnsureForPlan` → `EnsureBlocks` → existencia | **T2.k**: `StartTransaction` + `Commit` (solo lee) | No | No | `src/RackCad.Plugin/Drawing/BlockLibraryImporter.cs:68-80` |
| 6 | Por miembro, si faltan bloques y existe la biblioteca: la transacción de la base de **la biblioteca** y `source.WblockCloneObjects(ids, db.BlockTableId, …, DuplicateRecordCloning.Ignore, …)` | **Ninguna transacción** en la base del dibujo | **Sí** (OM-5) | No | `:96-124`, `:123` |
| 7 | Por miembro: la consulta final de disponibilidad | **T3.k**: `StartTransaction` + `Commit` (solo lee) | No | No | `src/RackCad.Plugin/Drawing/AutoCadLibraryBlockQuery.cs:24-36`; `src/RackCad.Application/Systems/Shared/LibraryBlockRequirements.cs:175-179` |
| 8 | `BeginMutation`: comprueba `TopTransaction` y la abre | **TM**: `StartTransaction` | — | No | `PV/RackPilotUpdateWriter.cs:140-143` |
| 9 | `Reread`: `TraverseCore` | dentro de TM | No | No | `:240-248` |
| 10 | `WriteUnit` × N: `RedrawInTransaction` → `RedefineSystemBlock` + `RackBlockData.Write` | dentro de TM | **Sí** (authored) | No | `:250-287`; `src/RackCad.Plugin/Systems/Shared/SystemBlockWriter.cs:126-138` |
| 11 | `Seal`: `_transaction.Commit()` | **TM confirmada** | **Sí** (authored) | No | `PV/RackPilotUpdateWriter.cs:289-295` |
| 12 | El `Dispose` del ámbito de MUTATE (`_transaction.Dispose()`) | — | No (sin `Seal`, es el aborto) | No | `:297-304` |
| 13 | `Post` 1: `TopTransaction != null` hace volver sin hacer nada | — | No | No | `:157-158` |
| 14 | `Post` 2: `PurgeAfterCommit` con la unión | **TP**: `StartTransaction` + `Purge` + `Erase` + `Commit`; si no hay candidatas, no se abre (`LateralHeaderDrawer.cs:195-198`) | **Sí** (no authored, categoría (b)) | **No** | `:162`; `src/RackCad.Plugin/Drawing/LateralHeaderDrawer.cs:193-238` |
| 15 | `Post` 3: `SyncName` por cada clave con nombre deseado | **TS.k**: `StartTransaction`, y `Commit` **solo** si renombra; si no, aborto de lectura | **Sí**, si renombra (no authored, POST) | **Sí** | `:165-168`; `src/RackCad.Plugin/Drawing/RackBlockRenamer.cs:32-50` |
| 16 | `Post` 4: la lectura de nombres | `StartOpenCloseTransaction`, sin `Commit` | No | No | `PV/RackPilotUpdateWriter.cs:172-179` |
| 17 | `Post` 5: `ApplyRegen(document, true)` → `Editor.Regen()` | — | No, en la base de datos [I] | No | `:183-190` |
| 18 | `Post` 6: `CaptureDrawing` | **T4**: `StartTransaction` + `Commit` (solo lee) | No | **Sí** | `:193` |
| 19 | El `Dispose` del ámbito exterior | — | No | No | `:197-203` |

**H2-29 [M]. Recuento por Actualizar:**
- **Transacción authored que escribe: exactamente una** (TM).
  - La fijan las guardas: un `StartTransaction`, en `BeginMutation`, y un `.Commit(`, en `Seal`, en todo el archivo (`TW/WorkspacePilotUpdateGuardTests.cs:208-213`).
- **Escrituras no authored:**
  - hasta una transacción de purga (TP);
  - hasta N transacciones de `SyncName` (TS.k);
  - hasta N `WblockCloneObjects` **sin transacción** de la base del dibujo.
- **Transacciones de solo lectura confirmadas:** T1 y T4, más 2N en la importación (T2.k y T3.k).
- **Por la semántica de D-13** (`FRZ:512-520`), solo MUTATE es authored: la importación es residuo OM-5 y el renombrado y la purga son POST.
- **[I]** La condición «more than one authored mutation transaction is required» de `OO` no se activa por esto. Pero **un Undo de un solo paso tiene que agrupar las 1 + (0..1) + (0..N) + (0..N) escrituras** [H].

### 4.2 Las marcas de Undo en `src`

**H2-30 [M]. No hay ninguna en todo `src`.** Una búsqueda con `git grep` en `b15072ff` da **cero** apariciones en el código de:
- `UndoRecording`, `DisableUndoRecording`, `StartUndoMark`/`EndUndoMark`;
- `UNDO BEGIN`/`UNDO END`, `_.UNDO`, `"UNDO`;
- `SendStringToExecute`, `PostCommand`, `Editor.Command`/`CommandAsync`, `acedCommand` y `UndoController`.
- Las palabras «undo» o «redo» solo aparecen en comentarios ajenos a esto, o en un delegado de `AutoCadEventBridge.cs:350-353` que deshace una suscripción.
- Coincide con `T4P:154-156` (H-15) y con `docs/initiatives/I-55-discovery.md:538` («No hay marcas de `UNDO` (`StartUndoMark`/`EndUndoMark`) en `src/`: lo que agrupa un `UNDO` lo decide AutoCAD»).

### 4.3 Cómo forma su paso de Undo el Actualizar clásico

**H2-31 [M]. El camino.** El Actualizar clásico de Push Back no es `RACKPUSHBACK`:
- `RACKPUSHBACK` (`src/RackCad.Plugin/RackPushBackCommands.cs:36-70`) solo crea y dibuja un rack nuevo.
- El Actualizar es `RACKEDITAR`: `[CommandMethod("RACKEDITAR")]`, sin flags (`src/RackCad.Plugin/RackMenuCommands.cs:153-187`) → `PushBackKindHandler.Edit` (`src/RackCad.Plugin/KindHandlers/PushBackKindHandler.cs:24`) → `RackPushBackCommands.EditPushBack` (`src/RackCad.Plugin/RackPushBackCommands.cs:147-384`), con `window.UpdateOnly`.

**Dentro del comando, en orden:**
- `ShowModalWindow` (`:170`);
- las comprobaciones previas (`:204-237`);
- el catálogo (`:268`);
- y luego, **por vista**: `RedrawInPlace(..., regen: false)` con su propio `LockDocument` anidado, `EnsureForPlan`, **su propia transacción con su propio `Commit`** y su purga (`src/RackCad.Plugin/Systems/Shared/SystemBlockWriter.cs:67-102`), seguido de `SyncName` con su bloqueo y su transacción (`:279-282`, `:297-309`, `:336-342`);
- `EraseViewBlocks` de los cortes obsoletos (`:351`);
- **un** `Editor.Regen()` (`:366`).
- No es atómico (`T4P:146`, H-13).

**Inferencia [I].** Todas esas transacciones caen dentro de un único comando registrado y modal.
- AutoCAD agrupa en un paso de Undo lo que modifica un comando, así que un `U` deshace todo el RACKEDITAR.
- **No hay ninguna medición en el repositorio:**
  - Smoke-1 solo midió el `UNDO` de órdenes nativas: «`UNDO` 3 → ERASE COPY MOVE» (por ejemplo, `docs/automation/evidence/I-64-pilot/F1-SMOKE-1/R20261107T171052Z-6862/operator/operator-record.md:107`);
  - la fila `OV-ID18-06` de I-55 («`U` tras un lote | Registrar lo observado», `docs/initiatives/I-55-g16-owner-validation.md:52`) solo quedó como un PASS global del grupo, «sin detalles por fila» (`docs/automation/evidence/I-55-evidence.md:34-36`).

**H2-32 [I]. La diferencia con el piloto:**

| | El clásico | El piloto |
|---|---|---|
| Qué agrupa el Undo | Un comando registrado | Si corre en `EXECUTEFUNCTION`, el comando interno |
| Commits authored | N, uno por vista | 1 |

- La agrupación del piloto se puede suponer por analogía con el clásico, **pero no está medida**.
- `D-13` congela el observable: «un Actualizar del panel es un solo paso de UNDO… Si la prueba falla, la alternativa de mecanismo es un comando registrado lanzado con `SendStringToExecute`, mediante una A-n material» (`FRZ:542-543`; INV-32 en `FRZ:770`; S2-05 en `FRZ:885`).

---

## 5. Undo, en dos listas separadas

### 5.1 Lo que pueden probar las pruebas unitarias

**H2-33 [M]. Restricción de partida:**
- ningún proyecto de prueba referencia el Plugin (`tests/RackCad.Tests/RackCad.Tests.csproj:23-31`; `tests/RackCad.UI.Tests/RackCad.UI.Tests.csproj:25-27`);
- el Plugin solo se cubre leyendo su fuente como texto (`TW/WorkspaceHostSourceSupport.cs:11-13`);
- la CI compila el Plugin solo en Release (`T4BP:144-146`);
- ONT D-08 lo recuerda: «ningún proyecto de prueba ejecuta el Plugin» (`ONT:72`).

**Lo que sí pueden probar:**
1. **Con dobles en Core**, si la lógica de admisión del host se escribe en Application como una corrida pura con delegados inyectados (el precedente es `WorkspaceNavigationRunner`, `AW/IWorkspaceNavigationPort.cs:100-120`, probado con su doble de efectos):
   - una entrada en el contexto de comando por cada Actualizar;
   - un bloqueo, del documento de destino;
   - un escritor creado y liberado por corrida;
   - una invocación de `TryUpdatePilot`;
   - cero invocaciones si falla la entrada, el bloqueo o la revalidación;
   - el orden: el reposo antes de entrar, y dentro, la revalidación de la sesión, el bloqueo, la corrida y la liberación.
   - Equivale a los puntos 1-11, 17-19 de `OO`, «T4C AUTOMATED TESTS».
2. **La corrida de T4B ya prueba con su doble** (`T4BC:69-83`, `:85-87`):
   - una transacción authored en el doble (`Begin`, `BeginMutation` y `Seal` una vez);
   - las filas de la frontera;
   - y que `Post` va después de `Seal`.
3. **Con guardas de fuente sobre el Plugin**, con autopruebas en negativo:
   - **un** `ExecuteInCommandContextAsync`, solo en el archivo exacto (H2-14);
   - **un** `using (… .LockDocument())` en el adaptador, que envuelve la llamada a la corrida;
   - ningún `StartTransaction`, `Commit` ni `TopTransaction` en el adaptador;
   - ninguna `TopTransaction` abierta por T4C alrededor de la corrida;
   - y el escritor sin cambios (las guardas de D-T4B-09 siguen en verde).
4. **«Una sola transacción authored»** en el Plugin solo se prueba por guarda: las cuentas exactas de `TW/WorkspacePilotUpdateGuardTests.cs:208-213`.

**Lo que no pueden probar:** que el contexto de comando o el bloqueo **se comportan** como se espera en AutoCAD.

### 5.2 Lo que solo puede probar AutoCAD real

**H2-34 [H]:**
1. **El callback antes de que vuelva la llamada** (`T4P:550`):
   - el orden entre «la llamada volvió» y «el callback empezó»;
   - el estado del `Task` al volver;
   - y si la UI puede pintar en medio (H2-11, H2-12).
2. **Un solo paso lógico de Undo** por Actualizar (S2-05, `FRZ:885`):
   - qué nombre aparece en la lista de Undo;
   - si la importación (H2-28 #6), MUTATE, la purga y los `SyncName` caen en el **mismo** grupo;
   - y si un Actualizar que acaba en NotApplied **con** residuo OM-5 deja un paso de Undo propio, solo de importación.
3. **Que Undo restaura y Redo rehace:** el estado previo y el posterior, y que «el siguiente Actualizar da Conflicto» (S2-05).
4. **La agrupación de Undo de la importación** (`T4BP:801`; `T4BC:158`).
5. **El estado del documento en `EXECUTEFUNCTION`:**
   - si está ya bloqueado;
   - la reentrada del `using` exterior más los de `CaptureDrawing` y `SyncName` (H2-23);
   - `CommandInProgress` e `IsQuiescent`;
   - `TopTransaction == null` al entrar (§12.1 de `FRZ:834`).
6. **Que el comando interno vacía PICKFIRST** también en Actualizar (`*Cancel*` x2), y su efecto en el contexto de selección del panel (R-T4-09, `T4P:570`).
7. **Los eventos del propio comando interno** (`CommandWillStart`, `ObjectModified` y `CommandEnded` de `EXECUTEFUNCTION`), y su encaje con el ámbito de mutación propia (C-F4-04, `F4P:60`). El mapa del ámbito de mutación propia los trata en detalle.
8. **DBMOD y Undo tras el `Commit` de lectura del barrido** (`DSP:756`).

### 5.3 Las mediciones de host que ya existen

**H2-35 [M].** En el repositorio solo hay dos hitos de host de I-64:
- **Smoke-1** (F1): varias ocurrencias en `docs/automation/evidence/I-64-pilot/F1-SMOKE-1/`. El Undo, solo de órdenes nativas (H2-31).
- **Smoke-NAV** (F2): `SNAV1`, con la parada y DIAG-1 a DIAG-4, y `SNAV2`, con el PASS.
- **No hay ningún Smoke-2.** No existe un «Smoke-2 de F2 o F3»: F3 cerró sin hito de host y le pasó sus obligaciones a validaciones posteriores (`ICL:87-89`).
- `EV` menciona `Smoke-2` 21 veces, siempre como algo futuro.

**H2-36 [M]. Lo que de eso sirve a T4C:**

| Hecho | Fuente |
|---|---|
| El contexto (aplicación o comando) y el nombre del comando interno | H2-07, H2-10 |
| La pérdida de PICKFIRST | H2-06 |
| Que un `LockDocument` dentro del callback no falló | H2-10 |
| La reentrada en contexto de aplicación | H2-23 |
| «Vista sin `Commit` se revierte» | H2-19 |

**Lo que no sirve:**
- la sincronía del callback (H2-11);
- cualquier escritura en contexto de comando: DIAG-1 solo escribió la vista, y dentro de una transacción no confirmada;
- y cualquier Undo de RackCad.

### 5.4 Lo que haría falta, sabiendo que la sesión no puede tocar AutoCAD

**H2-37 [M]. Quién opera el host:**
- el Worker nunca usa AutoCAD; prepara el procedimiento y el Owner lo ejecuta (`FRZ:833`);
- C-F4-06 no tiene dispensa: dibujos reales y participación del Owner u operador (`DSP:102-106`; `OD2:198-212`);
- las inyecciones se activan solo en Debug y por variable de entorno del proceso, y se acusan con I-52 (`FRZ:831-832`);
- `OO`, «SMOKE-2»: «Smoke-2 begins only after T4C product closes».
- Smoke-NAV `SNAV2` lo operó la sesión (`ONT:88`). Ese precedente ya no vale para T4C, por esta orden.

**H2-38 [I]. La tensión:**
- R-T4-05 pone la medición del callback y del Undo como **primer paso** de T4C (`T4P:550`; `DSP:1701`);
- la sesión no puede operarla;
- y el único hito previsto, Smoke-2, va **después** de cerrar el producto.

**Opciones:**

| Opción | Pros | Contras |
|---|---|---|
| (a) Una **sonda de host previa** operada por el Owner, con un build Debug de diagnóstico que no es candidato, como DIAG-1 a DIAG-4. Registra en `RackLog`: `IsApplicationContext`, `CommandInProgress`, la hora antes de llamar, **al volver la llamada** (con `Task.IsCompleted`), al entrar y al salir del callback, y una escritura mínima con su Undo | Responde a R-T4-05 antes de fijar el diseño | Necesita una autorización de host propia (I-52 RELEASE, §12.1). El código de la sonda contradice H-13 y no se puede comitear (precedente: `SNAV1/operator/session-log.md:19`, «sin commit; fuente restaurada») |
| (b) Dividir T4C en un producto con instrumentación Debug, una sonda del Owner y el resto | La sonda mide el adaptador real | Gasta intentos y presupuesto. Hace falta una decisión del Owner sobre la división |
| (c) Medir en Smoke-2, con la A-n de D-13 preparada | No necesita un hito nuevo | Contradice «primer paso». Un callback diferido se descubriría con el producto ya cerrado |

---

## 6. Los fallos del host que T4C tiene que contener y clasificar

**H2-39 [M]. Los resultados que ya existen**, en `EditUpdateStatus` (`AW/WorkspaceEditIntent.cs:381-419`):
- los de la puerta y la comprobación previa: `Rejected`, `NoDraft`, `NothingToApply`, `PendingText`, `NotApplicable`, `AnotherUpdateInProgress` y `Refused`;
- los de la corrida tras `CompleteApply`: `NotApplied{Conflict,Admission,Preflight,Failure}`, `CommitThrew` (el borrador queda Unknown(CommitThrew)), `Applied`, `AppliedWithWarnings`, `AppliedPostRereadFailed` (Unknown(PostReread)), `ClosedDetached` y `ClosedInError`;
- y **`Failed`**: «A failure of the host BEFORE the update began» (`:417-418`; `T4BP:594`, fila 18).

**Cómo los produce `TryUpdatePilot`:**
- `Failed` sale solo de una excepción **antes** de `TryBeginApply` (`AW/WorkspaceHostCore.cs:845-849`).
- Después, la respuesta sale siempre del cierre del borrador (`:851-904`).

**H2-40 [M]. Lo que se escapa de esa totalidad.** Las líneas `:868-904` (el cálculo de `status`, `DescribeDifferences` y `EditUpdateOutcome.Closed`) quedan **fuera** de cualquier `try`, después del `finally` de `CompleteApply`.
- Una excepción allí sale de `TryUpdatePilot` con el borrador ya cerrado: Applied, Conflict o el que sea.
- El arrastre M-01 de T4B («la totalidad defensiva … hoy es inalcanzable», `T4BC:130`) habla del `catch` exterior de `Execute` y de `!closed.Accepted`. **No** cubre estas líneas.
- **[I]** Si el callback de T4C convierte esa excepción en «no aplicado» o en `Failed`, contradice la orden «Do not translate UNKNOWN into ordinary Failure or Success» de `OO`.

**H2-41. Tabla de los fallos del host** [M para el código, I para el comportamiento de AutoCAD, H para la confirmación]:

| Fallo | Dónde | ¿Corre T4B? | Escrituras | Cómo distinguirlo de T4B (opciones) |
|---|---|---|---|---|
| **El contexto de comando no arranca o lanza al llamarlo**: una excepción síncrona de `ExecuteInCommandContextAsync`, o un `Task` fallado o cancelado **sin** callback | En el adaptador, antes del callback | **No**: la ranura no se tomó | Cero | (a) `EditUpdateStatus.Failed`, con una causa del host. Encaja con su definición («BEFORE the update began»). (b) Un estado nuevo del host. **[I]** No debe usarse `NotAppliedFailure`, que supone que el borrador cerró su ranura |
| **El callback no llega a correr nunca** (un documento cerrado, AutoCAD que sale o un comando interno descartado) | Entre la admisión y el callback | No | Cero | Lo mismo. **[I]** Con un callback diferido, la UI se quedaría sin respuesta: no hay `Task`, temporizador ni `Dispatcher` permitidos (H2-03 a H2-05) |
| **El bloqueo falla**: `LockDocument()` lanza, por ejemplo con `eLockViolation` o sobre un documento ya liberado | En el callback, antes de `TryUpdatePilot` | No | Cero | `Failed`, con «el documento está bloqueado por otra operación». Precedente del texto: `PV/RackPilotIntentAdapter.cs:125-128` |
| **El documento cambió o se cerró** entre la admisión y el callback | En el callback | Solo su puerta: `Rejected`, «No hay un documento activo» o `NotThisDocumentReason` (`AW/WorkspaceHostCore.cs:913-929`). Si la puerta pasara, el escritor da NotAppliedPreflight(`DocumentMismatchReason`) | Cero | (a) T4C revalida antes de bloquear y responde `Rejected`/`Failed`. (b) Dejar que lo responda `TryUpdatePilot`. **[I]** (a) evita bloquear un documento equivocado. **[H]** Si el cierre se veta (D-17) cuando hay un borrador dirty pendiente |
| **La cancelación** | Sin definir en el código | — | — | **Pregunta Q2-05.** **[I]** Dentro de un callback síncrono no hay un punto en que el usuario pueda cancelar: T4B no pide nada al usuario. Lo que puede cancelarse es la orden interna **antes** del callback (como la fila 1) |
| **Una excepción después de que `TryUpdatePilot` devolvió**: el `Dispose` del bloqueo, o el `Task` que falla después del callback | En el adaptador | Sí, y ya decidió | Las de T4B | **[I]** Se conserva el resultado capturado de T4B. Opciones: (a) añadir un aviso, (b) solo registrarlo. Nunca reclasificar |
| **Una excepción dentro de `TryUpdatePilot` después de `CompleteApply`** (H2-40) | En el núcleo | Sí, y el borrador ya cerró | Las de T4B | **[I]** No se puede reconstruir el `EditUpdateOutcome`, pero el estado del borrador sí es autoritativo: `GetField` y `LastApplyCause`. Opciones: (a) que T4C devuelva un resultado que remite al estado del borrador; (b) cerrar el hueco en el núcleo (M-01 ampliado, que toca un archivo de T4B) |
| **Un segundo Actualizar** | Antes del callback, si el callback es diferido | Si llega al callback: la ranura da `AnotherUpdateInProgress` solo mientras la primera corrida esté **dentro** de la ranura | — | **[I]** La ranura (`TryBeginApply`) se toma **dentro** del callback (`AW/WorkspaceHostCore.cs:837`). Con un callback diferido, dos pulsaciones encoladas darían dos corridas en serie, y la segunda podría volver a escribir tras un Conflict. Hace falta una marca del host «en vuelo» desde la admisión (`OO`, «ONE ACTUALIZAR») |

**H2-42 [M]. Lo que separa un resultado de T4B de uno del host:**
- **Un resultado de T4B** sale **siempre** de `CompleteApply`: la ranura se tomó, y `Reason` es `LastApplyCause` (`T4BP:591`).
- **Un fallo del host,** en cambio:
  - no toma la ranura;
  - no llama a `CompleteApply` ni a `NotePilotEdit`;
  - y deja el borrador en el estado que tenía.
- Una prueba de Core puede fijarlo: tras un fallo del host simulado, el estado y la causa del borrador no cambian y la cuenta de `Begin` del puerto es 0.

---

## 7. Riesgos

- **R2-01 (alto).** **Callback diferido** (H2-12):
  - el resultado no puede volver en síncrono por ninguna superficie, porque `Task` está prohibido en el núcleo, el controlador, el puerto y la UI (H2-05);
  - la ranura no protege el intervalo entre la pulsación y el callback (H2-41, última fila);
  - y SN-08, «una petición de A ejecutada con B activo», se vuelve provocable.
- **R2-02 (alto).** **Agrupación de Undo.** Un Actualizar tiene hasta 2N + 2 escrituras fuera de MUTATE (H2-29).
  - Si el comando interno no las agrupa en un paso, o si la importación queda como paso aparte, la alternativa de D-13 (`FRZ:542-543`) necesita varias cosas:
    - un `[CommandMethod]` nuevo, con el censo de 40 (`SelectiveEditorOpenTests.cs:571`) y `DeclaresEntryPoint`;
    - y `SendStringToExecute`, prohibido en al menos seis guardas (H2-02; `TW/WorkspaceLifecycleGuardTests.cs:681`; `TW/WorkspacePilotEditGuardTests.cs:74`).
  - Además, ese mecanismo también es diferido [I].
- **R2-03 (medio).** **Un bloqueo que falta y queda oculto.** En contexto de comando, la falta del `using` explícito podría no verse (H2-22). Solo una guarda de fuente lo detecta.
- **R2-04 (medio).** **Dos resoluciones independientes del documento**, la del adaptador y la del escritor (H2-24), sin ninguna prueba que fije que coinciden.
- **R2-05 (medio).** **Una excepción del núcleo después de `CompleteApply`** (H2-40), no cubierta por M-01, con riesgo de reclasificar un Applied o un CommitThrew.
- **R2-06 (medio).** **La inexactitud de `T4BP:389` sobre la purga** (H2-21). Una guarda o una revisión que se apoye en ella podría admitir un bloqueo de la purga «por dentro», que no existe.
- **R2-07 (bajo).** **El catálogo dentro del bloqueo.** Con el `using` único, `LoadCatalog` y los N `RackCatalogLoader.Load` quedan dentro del bloqueo (H2-28 #2 y #4).
  - El patrón de T2B2 lo deja **fuera** (`docs/initiatives/I-64-f4-t2b2-gate-contract-package.md:79`; `PW/WorkspaceNavigationHost.cs:228`).
  - Es un coste o un riesgo de E/S bajo el bloqueo, y no una regla congelada para Actualizar.
- **R2-08 (bajo).** **Pérdida de PICKFIRST tras Actualizar** (H2-06): puede cambiar el contexto de selección del panel justo después de POST.
- **R2-09 (medio).** **La sonda de host del «primer paso»** (H2-38) no tiene vehículo autorizado. Si se aplaza a Smoke-2, un fallo de sincronía o de Undo se descubre con el producto ya cerrado.

## 8. Preguntas abiertas

- **Q2-01.** ¿Cuál es el vehículo de la medición de R-T4-05: una sonda previa del Owner, una división de T4C o Smoke-2 (H2-38)? ¿Y quién autoriza la ventana de host?
- **Q2-02.** ¿Basta la opción (a) de H2-25 para cumplir «receives only the exact current document/context», o el Owner quiere que el escritor reciba el documento, con un cambio declarado en una guarda de T4B?
- **Q2-03.** Si el callback resulta diferido:
  - ¿se acepta un modelo de «extracción» (la UI muestra «Aplicando» y lee el resultado del borrador en el siguiente `StateChanged`), sin `Task`?
  - ¿o es una A-n material?
- **Q2-04.** ¿Qué estado de `EditUpdateStatus` usa un fallo del host antes de la corrida: `Failed` o uno nuevo? ¿Y cómo se presenta una excepción del host **después** de un resultado de T4B (H2-41)?
- **Q2-05.** ¿Qué es «cancel» en `OO` («released on cancellation», y «cancel» en Smoke-2)?
  - ¿La cancelación de la orden interna antes del callback?
  - ¿El «cancel conflict without write» de la lista del Owner (`OD2:229`)?
  - ¿O un `Esc` del usuario?
- **Q2-06.** ¿Se cierra en T4C el hueco de H2-40, dentro de M-01, con un cambio en `WorkspaceHostCore.cs`? ¿O queda declarado?
- **Q2-07.** ¿Dónde vive el adaptador: en `Plugin/Views`, agnóstico y con un delegado, o con una excepción de `TW/WorkspaceBridgeSourceGuardTests.cs:361-366` (§1.5)?
- **Q2-08.** ¿Se acepta el catálogo dentro del único bloqueo (R2-07), o se carga fuera y se entrega a la corrida? La segunda opción toca el escritor de T4B.

## 9. Posibles condiciones de devolución («RETURN BEFORE EMISSION» de `OO`)

- **PD2-1. «DocumentLock/command context cannot wrap the T4B run cleanly».** Si la sonda confirma que el callback es diferido (H2-12, R2-01):
  - el bloqueo y la corrida siguen envolviéndose limpiamente **dentro** del callback;
  - pero el resultado y la exclusión de un segundo Actualizar no se pueden devolver por la superficie síncrona actual;
  - y la respuesta exige un mecanismo nuevo de entrega del resultado a la UI, por el veto de `Task` (H2-05). Es **material**.
- **PD2-2. «Undo requires a material architecture change».** Si un Actualizar no da un solo paso de Undo en `EXECUTEFUNCTION` (R2-02), la alternativa de D-13 es un comando registrado con `SendStringToExecute` (`FRZ:542-543`), que ya está declarada como A-n material.
- **PD2-3. «any other material product decision».** La sonda del primer paso de R-T4-05 no tiene vehículo (H2-38, Q2-01): la sesión no opera AutoCAD y Smoke-2 va después del producto.
- **PD2-4. «the pilot-specific writer needs a new persistent host authority» (improbable).** Una marca «en vuelo» del host (H2-41) sería **en memoria**, no persistente. No se activa salvo que el diseño la persista.
- **PD2-5. «T4B cannot be invoked without duplicating mutation policy» (no se activa con lo medido).** `TryUpdatePilot` se puede llamar dentro del callback tal cual (`T4BP:557`), y la lectura de O-04 ya está en la corrida (H2-18).
  - Haría falta reabrir T4B solo en tres casos: si el Owner elige la opción (b) de H2-25, si se cierra H2-40 en el núcleo (Q2-06) o si el catálogo sale fuera del bloqueo (Q2-08).
  - En esos casos es un cambio declarado en archivos de T4B, contra «Do not reopen T4B merely because T4C integrates it».
- **PD2-6. «more than one authored mutation transaction is required» (no se activa).** Hay exactamente una (H2-29). Las demás escrituras son POST o residuo OM-5, según D-13.

## 10. Recuento

| | Ids | Número |
|---|---|---|
| Hechos | H2-01 a H2-42 | 42 |
| Riesgos | R2-01 a R2-09 | 9 |
| Preguntas | Q2-01 a Q2-08 | 8 |
| Posibles condiciones de devolución | PD2-1 a PD2-6; tres posibles y tres que no se activan con lo medido | 6 |
