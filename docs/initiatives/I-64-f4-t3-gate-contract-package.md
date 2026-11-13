# I-64 — Paquete del contrato de F4-T3 (UI de edición y texto pendiente del piloto)

**Estado:** v2.
- La v1 pasó la revisión adversarial independiente que exige la orden del Owner: **EMITIBLE TRAS CORRECCIONES** (0 BLOQUEANTE, 9 IMPORTANTE y 12 MENOR). La v2 aplica todos sus hallazgos (§9).
- El Owner decidió la división en **tres subtareas** (`owner-f4t3-decomposition-decision.md`).
- La re-revisión de la v2 dio **EMITIBLE TRAS CORRECCIONES** para T3A (0/2/9), todas de texto. Se aplicaron antes de emitir (§10).

**Base:** rama `architecture/workspace-persistente-rackcad` en `94b4d468` (custodia del cierre de F4-T2C); `main` en `812baf87`. Los hechos del §2 se midieron en `94b4d468`.

**Precondiciones de emisión de T3A:**
- F4-T2 está completa en sus subtareas (F4-E37).
- El Owner autorizó F4-T3 (`owner-f4t3-authorization.md`) y decidió la división.
- R-T2C-01 queda DIFERIDO a F6 o F7, fuera de T3.
- Las órdenes del Owner se custodian en el commit del paquete, que es la AuthorityRevision.

## 1. Autoridades

- **Órdenes del Owner** (`I-64-pilot/F4-DRAFTS/coordinator/`):
  - **`owner-f4t3-authorization.md`**:
    - alcance de T3: UI de edición, texto del campo, texto pendiente, presentación de la validación, marca dirty, transiciones bloqueables y no bloqueables, rematerialización, descarte explícito y modal;
    - sin mutación authored ni Actualizar; solo `AnnotationScale`;
    - requisitos del texto pendiente, de la preview y del primer gesto, de la marca dirty, del descarte, del cierre (Descartar o Cancelar) y del modal;
    - arrastres M-1..M-5 y D-1 de F4-T2B2B y los 4 menores de F4-T2C;
    - cobertura mínima 1-18 y oráculo crítico (cero escrituras authored);
    - proceso y regla de nc1 para F4.
  - **`owner-f4t3-decomposition-decision.md`**: tres subtareas, en orden A, B, C, cada una con 1 de 3 intentos.
- **Freeze** (`docs/initiatives/I-64-proposal-v5.md`):
  - D-02: foco; acciones con un comando activo; subdiálogos (`:131-139`, `:160-175`);
  - D-04 y P-03: el manejador de `BeginDocumentClose` solo lee memoria, decide el veto y emite un aviso no modal (línea de comandos y aviso encolado del panel), `:217-224`;
  - D-08: pestañas, preview, «Nombre *», cierre con Descartar o Cancelar, K = 1 y Error (`:383-398`);
  - D-10: componentes, dirty, intención, fronteras y O-16 (`:410-441`);
  - D-11 (`:457-458`);
  - D-17 (`:596-615`);
  - INV-04, INV-05, INV-09, INV-18, INV-20, INV-21, INV-22, INV-24, INV-25, INV-26, INV-27 e INV-31.
- **ADR-0032:**
  - D5: pendiente ≠ comprometido; mostrar no es editar;
  - D6: compromiso en dos fases, en «toda frontera transaccional», incluido «el gesto propio de cada campo».
- **Paquete de F4 v2** (`docs/initiatives/I-64-f4-gate-contract-package.md`): fila de T3 (`:46`), H-11, H-12, H-15 y obligaciones 5-9 y 15.
- **Paquete de F4-T2** (`docs/initiatives/I-64-f4-t2-gate-contract-package.md`): fila de T3 (`:55`), D-T2-01 (`:81-103`), D-T2-07 (`:232-234`) y la clausura (`:59-66`).
- **Paquete de F4-T2B** (`docs/initiatives/I-64-f4-t2b-gate-contract-package.md`): el testigo de la preview (`:141-144`).
- **Paquete de F4-T2B2** (`docs/initiatives/I-64-f4-t2b2-gate-contract-package.md`): `:139-140`, `:203-204`, `:280-281`, `:320-321`, `:349`, `:434` y `:437-444`.
- **Aclaraciones:** IC-03, IC-06, IC-07 e IC-08.
- **Disposiciones** (`f4-dispositions.md`):
  - F4-E02, F4-E09 y F4-E31;
  - F4-E33 (arrastres a T3, `:749-753`);
  - F4-E35 y F4-E37;
  - C-F4-01, C-F4-03 y C-F4-07 (`:99-101`, con `owner-f4-disposition-2.md:171-196`).

## 2. Hallazgos del repositorio (medidos en `94b4d468`)

### El núcleo ya hecho

| Id | Hallazgo | Consecuencia |
|---|---|---|
| H-01 | **`DraftSession`** (`Workspace/Drafts/DraftSession.cs`):<br>- texto pendiente por clave (`SetPendingText` `:338`, `RemovePendingText` `:347`, `PendingText` `:278`, `HasPendingText` `:280`);<br>- `IsDirty`: pendiente o current ≠ initial (`:283`);<br>- `Status` (`:285`); `CanEdit` **privado**: registrado y Status ∈ {Clean, Dirty, Conflict} (`:316-317`);<br>- fronteras `DraftBoundary` {TabChange, RackChange, Update, DocumentChange, HidePanel, Dematerialize, DocumentClose}; bloquean TabChange, RackChange y Update (`:76-91`);<br>- `CrossBoundary` (`:362-389`):<br>&nbsp;&nbsp;· no bloqueante → `Kept`;<br>&nbsp;&nbsp;· borrador no editable con pendiente → `Kept` (`:370-371`);<br>&nbsp;&nbsp;· bloqueante inválida → `Aborted` sin cambios, con `InvalidFields`;<br>&nbsp;&nbsp;· válida → `Apply` en dos fases y limpia el pendiente;<br>- `DiscardAndReload` vacía el pendiente (`:420`) | La semántica del pendiente y de las fronteras ya existe y está probada (F4-T1). T3 la expone y no toca `Drafts/` |
| H-02 | **`PushBackPilotFieldAdapter`**:<br>- clave única `"AnnotationScale"`;<br>- `Validate` con paridad con el `NumericField` clásico: «Escribe un valor.», «Valor no numérico.», «El valor debe ser > 0.»;<br>- `LocalizedNumberParser`: signo, decimal con punto o coma, sin miles ni exponente, cultura actual primero;<br>- `Apply(current, pendingText)` en sitio | Validación y aplicación del adaptador, sin copiarlas |
| H-03 | **Ventana clásica:**<br>- `NumericField` sin binding;<br>- `SetNumber` con `"0.###"`;<br>- texto inválido con borde rojo, sin revertir y sin revertir con Esc; Restaurar es un botón explícito;<br>- por defecto 1.0, y ≤ 0 → 1.0 al cargar;<br>- no usa `PendingTextField` | El panel no puede usar `"0.###"` (D-T2-01). `"R"` usa exponente desde 1E+15 y por debajo de 1E-05 |
| H-04 | **`WorkspaceHostCore`:**<br>- `TryBeginPilotEdit` (`:267`), `TryCheckPilotEdit` (`:352`), `TryReloadPilotEdit` (`:425`; pasos 6-8 en `:451-476`, sin volver a comprobar tras la lectura);<br>- `DiscardPilotEdits` (`:507`, toda la sesión);<br>- `NotePilotEdit` (`:530`);<br>- `internal LossNotices` (`:580`) e `internal OpenDrafts` (`:250`);<br>- `AdviseClose` (`:537-549`), fijado por `WorkspaceLifecycleGuardTests.cs:309-325`.<br>Nada tiene llamador en `src`. `InternalsVisibleTo` solo apunta a `RackCad.Tests` | No hay API fuera de Application para leer o cambiar un borrador. T3A crea su puerto (T2B2 `:139`) |
| H-05 | `EditIntentRequest(SessionId, RackId, ChosenReference, Witness)` y `EditIntentWitness(Key, Text)` (`WorkspaceEditIntent.cs:12-18`). Sin testigo, el resultado es `NotSupplied`, que no equivale a «sin cambios» (T2B `:142`). `RackPilotIntentAdapter.ReadPreviewWitness` no tiene llamador | D-T3-04 |
| H-06 | **`StateChanged` se emite siempre desde `WorkspaceController.NotifyStateChanged`** (`:174`). Lo llaman:<br>- `OpenPanel`, `ShowPanel` y `HidePanel` (`:105`, `:111`, `:117`);<br>- `OnActiveSessionChanged` (`:140`), dentro de `DocumentActivated` y `DocumentToBeDestroyed`;<br>- `OnCommandStateChanged` (`:145`), dentro de los eventos de comando;<br>- `OnDrained` (`:165`), en la continuación de T2C.<br>El host de la paleta lo relaya a `_viewModel.Refresh()` | Toda reacción de la UI a `StateChanged` puede estar dentro de un callback de AutoCAD (D-T3-06) |

### Ciclo de materialización K = 1 (paso 3 de la orden; hallazgo I-1 de la revisión)

| Id | Hallazgo |
|---|---|
| H-07 | **`BrowserSession`** (`Application/Workspace/BrowserSession.cs`):<br>- `MaxLiveSurfaces = 1` (`:49`);<br>- la superficie viva es siempre la pestaña activa: `LiveSurfaceRackId => _activeRackId` (`:71-72`);<br>- `ApplySelectionContext` (`:78-113`): un Uno(R) del dibujo activa la pestaña de R si existe; si no, crea la preview o **reemplaza la preview existente**; una pestaña fijada nunca se reemplaza;<br>- `Activate` (`:116-124`), `Pin` (`:127-134`, solo una preview), `Close` (`:140-156`, pasa a la siguiente pestaña o a la anterior) y `ApplySummary` (`:162-173`). |
| H-08 | **`WorkspaceBrowserViewModel`** (`UI/Workspace/WorkspaceBrowserViewModel.cs`):<br>- `Present` (`:124-138`), con la sincronización automática por selección;<br>- `Interact` (`:179-193`) para los gestos del usuario (`ActivateTab`, `PinTab`, `CloseTab`);<br>- `SyncSurface` (`:211-225`) reinicia la superficie (`StartSurface` `:228-237`) si cambia la sesión o el rack activo, y la vacía (`ClearSurface` `:239-248`) sin pestaña activa;<br>- `Materialize` (`:252-272`) lee el resumen;<br>- con el panel oculto no se presenta nada (`WorkspacePaletteHost.cs:125`). |

**Transiciones reales**

Desmaterializar es cambiar o vaciar la pestaña activa, y ocurre por estas vías:

| Transición | Quién la inicia | Frontera del puerto | ¿Bloquea? | Quién llama |
|---|---|---|---|---|
| Activar otra pestaña en el panel | el usuario, en el panel | `TabChange` sobre la pestaña viva, **antes** de activar | sí | VM (T3B) |
| Cambiar de rack por una acción del panel, por ejemplo una fila que abre otro rack | el usuario, en el panel | `RackChange` sobre la pestaña viva, antes del cambio | sí | VM (T3B) |
| Fijar | el usuario | ninguna | — | — |
| Cerrar una pestaña limpia | el usuario | ninguna. Se cierra y llama a `DiscardRack`, que no tiene efecto en el DWG, para que el borrador termine (D-10 «Vida»; re-revisión, M-h) | — | VM |
| Cerrar una pestaña dirty o con pendiente | el usuario | ninguna; Descartar (`DiscardRack`) o Cancelar | — | VM |
| Selección Uno(R) en el dibujo, por la sincronización | AutoCAD (drenaje) | **Nunca** bloqueante. Si la pestaña activa tiene **texto pendiente**, sigue activa y la preview de R se crea o reemplaza en segundo plano con un aviso (`P5:335-336`). Si no, también cuando solo está dirty, se activa o se crea la de R y la anterior pasa por `Dematerialize`, con su borrador conservado (re-revisión, N-2) | no | VM (T3B), con un cambio de `BrowserSession` |
| Cambio de documento | AutoCAD | `DocumentChange` sobre la pestaña viva de la sesión anterior | no | VM (T3B) |
| Ocultar el panel | el usuario o AutoCAD (paleta) | `PanelHide` | no | VM o paleta (T3B) |
| Cierre del documento, o sesión retirada | AutoCAD | `DocumentClose`, y después la retirada de T2B2B | no | VM (T3B); el host retira |

**Regla vinculante:** la sincronización automática por selección **nunca** cruza una frontera bloqueante y siempre conserva el pendiente. Ninguna transición que no inicie el usuario en el panel bloquea.

### Lo que fija el repositorio

| Id | Hallazgo | Consecuencia |
|---|---|---|
| H-09 | `IWorkspaceHostPort` es de solo lectura por guarda (`WorkspaceHostPortGuardTests.cs:24-237`); los puertos del navegador y de navegación son exactos | Un puerto nuevo, con sus propias guardas |
| H-10 | **Nombres y textos en `RackCad.Application.Workspace`.** Los nombres de tipo, anidados incluidos, no pueden:<br>- contener `Draft`, `Editor`, `Subscription` ni `Tab`; `Tab` se busca como subcadena sin distinguir mayúsculas, así que caen «Editable», «Mutable» o «Stable» (`WorkspaceModelBoundaryTests.cs:94-104`);<br>- coincidir con una palabra de `BrowserSession.cs` o de `Drafts/*.cs`, como `Status`, `Kind`, `Decision` o `Change` (`WorkspaceBrowserBoundaryTests.cs:84-100`; `WorkspaceDraftBoundaryTests.cs:441-452`).<br>El código no puede contener `Transaction`, `\bDatabase\b`, `LockDocument`, `\bCommit\(`, `RackLog`, ni, sin distinguir mayúsculas, `Provider`, `Metric`, `Aggregat`, `\bBom`, `TotalRacks` o `ProjectSummary`; por eso tampoco `IFormatProvider` (`WorkspaceModelBoundaryTests.cs:39-48`, `:81-92`; `WorkspaceHostSourceGuardTests.cs:487-492`, `:537-553`).<br>Solo el core, `IWorkspaceEditIntentReader.cs` y `WorkspaceEditIntent.cs` pueden nombrar `PushBackPilot*` (`WorkspaceEditIntentGuardTests.cs:504-514`).<br>`WorkspaceHostCore.cs`, comentarios incluidos, no puede contener `Transaction`, `Database`, `Commit(` ni «provider» (`:821-833`).<br>El adaptador del piloto no puede ser un campo de instancia del host (`WorkspaceEditIntentHostTests.cs:990-1009`).<br>`: IWorkspaceLifecycleAdvisor` sigue siendo la primera base del host (`WorkspaceLifecycleGuardTests.cs:296`) | **Trampas vinculantes para el Worker** (INV-F4T3A-12) |
| H-11 | `Plugin/Workspace` tiene 9 archivos exactos (5 guardas); `Drafts/` tiene 5 (D-1, más el conjunto exacto de `WorkspaceDraftBoundaryTests.cs:23-51`) | Ningún archivo nuevo en ninguno de los dos |
| H-12 | En el controlador: 3 `+=`, 0 `-=` y un `event` (varias guardas); sin `Dispatcher`, `Timer`, `Task` ni `async` (`WorkspacePaletteHostSourceGuardTests.cs:488-497`). `SetModalRackCadActive` público se conserva literal (`:493`). Ninguna guarda fija por reflexión los miembros públicos del controlador ni del host | Lo nuevo del controlador va sin `+=` y como clases anidadas |
| H-13 | **Arrastres de F4-T2B2B:**<br>- M-1 en `WorkspaceLifecycleGuardTests.cs:237-241` y `:282-283`;<br>- M-2 en `:386` (`_knownClean.Remove` = 9), `:459` (`.DiscardAndReload(` = 1), `:71` (dos métodos) y `:363-370` (cola `internal`);<br>- M-3 en `WorkspaceLifecycleHostTests.cs:786-806`;<br>- M-4 en `WorkspaceEditIntent.cs:82` y `:97`, con las guardas de texto de `WorkspaceEditIntentGuardTests.cs:821-833` y `WorkspaceLifecycleGuardTests.cs:642-658`;<br>- M-5 en `WorkspaceLifecycleHostTests.cs:1197-1211` y `:1422-1441`;<br>- D-1 en `:568` y `:680`;<br>- el cuerpo de `AdviseClose` (`:309-325`) | D-T3-09 |
| H-14 | **Arrastres de F4-T2C:**<br>- 1: G-1 y G-2 (`WorkspaceIdleSchedulingGuardTests.cs:51-52`, `:104-123`, `:600-606`);<br>- 2: G-3 y G-10 (`:159-168`, `:613-619`). `.Wait(` ya está prohibido (`WorkspaceHostSourceGuardTests.cs:426`, `:432`); faltan `ExecuteInApplicationContext` y `Dispatcher.Run`. Además, `.OnIdle(` con otro receptor (un alias del core) escapa al recuento;<br>- 3: tres pruebas (`WorkspaceIdleSchedulerTests.cs:1027-1067`, `:1091-1105`);<br>- 4: la documentación y los contextos de registro del puente (`AutoCadEventBridge.cs:13-24`, `:279`, `:293`), cuyos literales fijan las mutaciones de `WorkspaceIdleSchedulingGuardTests.cs:456`, `:549`, `:571-573`, `:650-651` y `:667` | D-T3-09 |
| H-15 | `SetModalRackCadActive` no tiene llamador en producción. El panel no abre subdiálogos. `ShowDialog` y `MessageBox` están prohibidos en `Plugin/Workspace` (`WorkspaceBridgeSourceGuardTests.cs:139-142`) y en la UI (`WorkspacePaletteHostSourceGuardTests.cs:514`); **`ShowModalWindow` no lo está** en el Plugin | D-T3-05 |
| H-16 | La UI del panel está muy fijada (constructor del VM, record de pestaña, `BrowserSession`, botón único, foco) | Es de F4-T3B |
| H-17 | La clausura de la ventana (F4-T2 `:59-66`) incluye `UI/Controls/**` (`PendingTextField` `internal`, `NumericField`), `Shell/**` (`EditorDiscardPrompt`), `UI/Systems/PushBack/**` y los tipos de estado y ensambladores de `Application/Systems/PushBack/**` | Se puede usar, no modificar |

## 3. Descomposición, presupuesto y reglas (decisión del Owner)

| Subtarea | Contenido |
|---|---|
| **F4-T3A-EDIT-PORT** (§4, §5) | El puerto de edición del piloto en Application y el host, sin UI y sin comando:<br>- texto mostrado;<br>- compuerta de reentrada de la lectura;<br>- compuerta de avisos;<br>- cola de avisos de veto;<br>- ámbito del modal del panel;<br>- arrastres M-2..M-5 y D-1 de F4-T2B2B y 1-4 de F4-T2C.<br>Se prueba en Core |
| **F4-T3B-EDIT-UI** (§7) | La superficie de edición del panel y su cableado en la paleta. Se prueba con pruebas de UI |
| **F4-T3C-CLOSE-DISCARD** (§8) | `RACKPANELDESCARTAR`, los mensajes del veto y de la pérdida en la línea de comandos, el envoltorio del asesor (M-1) y los cambios de censo de comandos |

- **Presupuesto:** cada subtarea empieza en `attempts` 1 de 3 (F4-E09). El Owner aceptó que la división triplica la capacidad de intentos de T3. No se describe como «9 intentos de T3».
- **Orden:** A → B → C. F4-T4 no empieza antes de cerrar T3C.
- **Paquetes:** los de T3B y T3C se fijan tras cerrar la subtarea anterior, sobre el código real. Este paquete fija ya sus reglas vinculantes (§7, §8).
- **Replanificación:** en cada subtarea, un rechazo por copia permite una replanificación reforzada única que no consume intentos. Una segunda falla con STOP al Coordinator (F4-E31).
- **Cierre:** RED y GREEN, CI del SHA exacto, revisión directa, revisión independiente, verificación del Controller, nc1..nc3 y custodia. El 14/14 del Controller no basta.
- **nc1:** si se reproduce **exactamente** la firma de PD-F4-04, se reutiliza el patrón del sustituto de identidad de la subtarea sin volver al Owner (orden de T3). La firma es:
  - el CurrentSha falso se detecta;
  - Identity = fail;
  - Handoff falla antes solo porque la entrega lleva el SHA falso;
  - BLOCKED/STOP;
  - todo lo demás coincide.

  Se registra cada uso y se conserva la salida ordinaria. Es una regla de I-64/F4.
- **Deudas visibles:** PD-F4-01 a PD-F4-04.

## 4. Decisiones de F4-T3A-EDIT-PORT

### D-T3-02. El puerto: `IWorkspacePilotEditPort`

Archivo nuevo `src/RackCad.Application/Workspace/IWorkspacePilotEditPort.cs`: la interfaz y sus valores. Reglas generales:
- Todos los resultados son valores inmutables, con listas nunca nulas, incluido el `default`.
- **Ningún tipo de `Drafts` ni del piloto cruza el puerto.** Los RackIds son `string`.
- La sesión va en `WorkspaceRequest` o `EditIntentRequest` (INV-22). La excepción es `TakeLossNotices`: los avisos son de sesiones ya destruidas.
- Solo las tres operaciones de la intención leen el DWG, por el lector de T2B2A y en una acción del usuario. Ninguna escribe.
- Toda excepción se contiene como un valor con motivo, sin degradar la sesión (como en T2B2).
- **Compuerta de reentrada (I-2; re-revisión, M-a).** El host pone una marca en memoria de «lectura del piloto en curso», en `try/finally`, alrededor de las tres operaciones que leen. Mientras la marca está activa:
  - `SetPendingText` y `CommitField` responden `Busy`;
  - `CrossBoundary` bloqueante responde `Busy`, que **aborta la transición** con aviso, igual que `Aborted`;
  - `DiscardRack` y `DiscardDocument` responden `Busy`. `EditDiscardStatus` gana `Busy` al final; su guarda de prefijo ordenado lo admite;
  - una segunda lectura anidada (`TryBeginEdit`, `TryCheckNow` o `TryReload`) responde `Waiting` sin leer.

  Todo, sin efecto. Así, una pulsación bombeada durante la lectura de una recarga no se pierde en silencio: se rechaza de forma visible y la caja se restaura (regla R-B4 de T3B).
- **Enums con nombre** (re-revisión, M-g): `PilotFieldState` (el estado de `PilotFieldView`), `PilotFieldStatus` (el resultado de `PilotFieldResult`), `PilotBoundaryDecision` (`Proceeded`, `Aborted`, `Kept`, `Busy`) y `PilotBoundaryKind`. Ningún tipo anidado de nombre simple.

| Operación | Contrato |
|---|---|
| `EditIntentOutcome TryBeginEdit(EditIntentRequest r)` | Delega en `TryBeginPilotEdit` |
| `EditCheckOutcome TryCheckNow(EditIntentRequest r)` | Delega en `TryCheckPilotEdit` |
| `EditIntentOutcome TryReload(EditIntentRequest r)` | Delega en `TryReloadPilotEdit` |
| `PilotFieldView GetField(WorkspaceRequest origin, string rackId)` | Solo memoria, **sin `Authorize`**. El panel presenta la sesión del snapshot, y una lectura de memoria no tiene efectos (como `IsSessionAlive`). Devuelve:<br>- `Exists`;<br>- `State`: `NotOpen`, `Clean`, `Dirty`, `Conflict`, `Applying`, `Unknown` (D-13), `Error` o `Undecidable` (el comparador lanzó);<br>- `IsPossiblyStale`;<br>- `IsProtected`, igual al `IsDirty` del núcleo. Con `Undecidable` vale true (sucio desconocido, F4-E29);<br>- `CanEdit`: registrado y State ∈ {Clean, Dirty, Conflict}. Con `Undecidable` vale false;<br>- `CommittedText`: según D-T3-03, o null con motivo;<br>- `PendingText`, o null;<br>- `PendingMessage`: el mensaje de `Validate`, o null.<br>Nunca lanza |
| `PilotFieldResult SetPendingText(WorkspaceRequest origin, string rackId, string text)` | **Solo una edición del usuario** (D-T2-01; mostrar nunca llama a esto). Exige `Authorize` y `CanEdit`, y que no haya lectura en curso. Guarda `SetPendingText(<clave del adaptador>, text)`; el texto vacío también se guarda. Llama a `NotePilotEdit(session)`. Devuelve:<br>- `Applied`, con el `PilotFieldView` nuevo;<br>- `Rejected`, `NotEditable` o `Busy`, con motivo. En ese caso la UI restaura la caja (R-B4) |
| `PilotFieldResult CommitField(WorkspaceRequest origin, string rackId)` | **El gesto propio del campo** (Enter o salir del campo; ADR-0032 D6). Exige `Authorize` y `CanEdit`, y que no haya lectura en curso.<br>- **Mecánica:** `CrossBoundary(DraftBoundary.TabChange, adapter)`, el mismo compromiso en dos fases de una frontera bloqueante, documentado como tal;<br>- sin pendiente → `Applied` sin cambios (idempotente);<br>- válido → aplicado, pendiente limpio y `NotePilotEdit`;<br>- inválido → `Invalid`, con el texto intacto y su mensaje, sin cambios ni auto-reparación |
| `PilotBoundaryResult CrossBoundary(WorkspaceRequest origin, string rackId, PilotBoundaryKind kind)` | `PilotBoundaryKind` es un prefijo ordenado {`TabChange`, `RackChange`, `DocumentChange`, `PanelHide`, `Dematerialize`, `DocumentClose`}. T4 puede añadir `Update` al final.<br>**Bloqueantes** (`TabChange`, `RackChange`), con `Authorize` y sin lectura en curso:<br>- inválido → `Aborted`, con los campos y mensajes, sin cambios;<br>- válido → `Proceeded`, aplicado, con el pendiente limpio y `NotePilotEdit`;<br>- borrador no editable con pendiente → `Kept`: la transición procede y el pendiente se conserva sin aplicar (`DraftSession.cs:370-371`);<br>- sin borrador → `Proceeded`.<br>**No bloqueantes:** sin `Authorize`, porque se cruzan cuando la sesión ya no es la activa. Devuelven `Kept`, con el pendiente conservado tal cual: no se valida a otro valor ni se compromete |
| `EditDiscardOutcome DiscardRack(WorkspaceRequest origin, string rackId)` | Descarte explícito de **un** borrador. Exige `Authorize` y que no haya lectura en curso. `DiscardByUser`, sin comparador, quita el borrador y su pendiente. Quita la marca «limpio conocido» de la sesión. Cero escrituras. Rack sin borrador → `Discarded` con listas vacías. La documentación de `EditDiscardOutcome` y de `Discarded` se ajusta a «un borrador o todos los de la sesión» |
| `EditDiscardOutcome DiscardDocument(WorkspaceRequest origin)` | Delega en `DiscardPilotEdits(session)`. Sin lectura en curso |
| `PilotRiskView GetRacksAtRisk(WorkspaceRequest origin)` | Solo memoria, **sin `Authorize`**: el veto de CLOSEALL o QUIT puede ser de un documento no activo, y la lista sirve para su mensaje (T3C). Devuelve:<br>- los RackIds de la sesión con borrador protegido, en orden ordinal;<br>- aparte, los inciertos: el comparador lanzó y cuentan como en riesgo.<br>Nunca lanza |
| `IReadOnlyList<PilotLossView> TakeLossNotices()` | Saca en orden todos los `DraftLossNotice` pendientes y los devuelve como valores: el valor de la sesión, los perdidos, los inciertos y sus motivos. Deja la cola vacía. La consume el panel (T3B) |
| `IReadOnlyList<PilotCloseView> TakeCloseNotices()` | Saca en orden los avisos de veto (D-T3-07) y los devuelve como valores: el valor de la sesión, los RackIds en riesgo y un **indicador** de «incierto» (no una lista). Deja la cola vacía. La consume el panel (T3B) |

**Implementación.**
- `WorkspaceHostCore` implementa el puerto **explícitamente**. Su superficie pública no crece y `: IWorkspaceLifecycleAdvisor` sigue siendo la primera base.
- Usa el registro único y el adaptador del piloto como local o campo estático, nunca como campo de instancia (H-10).
- `NotePilotEdit` se llama en todo camino que muta un borrador o su pendiente (T2B2 `:280`).
- **No cambian:** las operaciones de T2B2A y T2B2B, ni `Drafts/`.

### D-T3-03. Texto mostrado del valor comprometido (D-T2-01; M-2 de la revisión)

**Ayudante puro** `WorkspacePilotValueText`, en el archivo nuevo `src/RackCad.Application/Workspace/WorkspacePilotSupport.cs`.

**Precondición:** el valor es finito y > 0. Si no lo es, `CommittedText` = null, con un motivo, y no se crea ningún pendiente.

**Algoritmo:**
1. `s = v.ToString("R", CultureInfo.InvariantCulture)`;
2. separar la mantisa del exponente;
3. desplazar la coma en la cadena de dígitos, rellenando con ceros a la izquierda o a la derecha;
4. quitar los ceros sobrantes;
5. sustituir «.» por `CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator`.

El resultado es el mismo número decimal que `"R"`. Así:
- `LocalizedNumberParser.TryDouble(texto)` devuelve exactamente el mismo `double`;
- no hay exponente ni separador de miles;
- `Validate(texto)` es válido.

**Pruebas de propiedad:** en es-MX, en-US, de-DE e invariante, sobre un corpus con 1, 2.25, 1.23456, 0.1, 1E-05, 1E+15, 1E+20, `double.Epsilon` y `double.MaxValue`.

**Lo que no hace:**
- Mostrar nunca crea un pendiente.
- Comprometer el texto mostrado sin editarlo no cambia el valor: no hay dirty falso.

### D-T3-04. La intención sin testigo de preview (INV-27; M-3)

- T3 **no lee el valor en la preview**. La superficie de preview sigue mostrando el resumen de F3, y el campo aparece en solo lectura, con un marcador «—» en lugar de un vacío editable.
- La intención va sin testigo (`NotSupplied`).
- **Regla de T3B:** la superficie se renderiza desde el borrador recién leído y **el gesto que disparó la intención no se aplica**. Se deja un indicio visible, como «Listo para editar».
- Esto cumple INV-27: no se escribe sobre lo no visto.
- **Desviación registrada:** T2B `:141` decía «T3 captura el testigo». Mostrar el valor y su testigo en la preview queda para **F5**, junto al editor clásico (IC-03).
- N-2 y la compuerta de `ReadPreviewWitness` (F4-E33) siguen latentes hasta que ese método tenga llamador.

### D-T3-05. Ámbito del modal RackCad del panel (arrastre de T2C; orden del Owner; M-7)

**Tipo puro** `WorkspacePanelModalScope` (en `WorkspacePilotSupport.cs`), que recibe la acción de activar y desactivar:
- `Enter()` devuelve un `IDisposable` y activa **antes** de devolver;
- lleva un contador anidado: solo el último `Dispose` desactiva;
- el `Dispose` doble es idempotente.

**`WorkspaceController`** expone `public IDisposable EnterPanelModal()` sobre ese tipo, con `_host.SetModalRackCadActive`.

**Uso obligado:** todo subdiálogo del panel (`ShowModalWindow`, `ShowDialog`, `MessageBox`, en las carpetas Workspace) se abre dentro de `using (EnterPanelModal())`, antes de que el diálogo esté activo. La salida por excepción también desactiva.

**Situación en T3A:** no hay ningún subdiálogo; el ámbito existe para T3B o F5.

**Guardas nuevas** (re-revisión, M-i), redactadas sobre los dos puntos de entrada públicos:
- nadie llama a `WorkspaceController.SetModalRackCadActive`;
- a `_host.SetModalRackCadActive` solo lo llaman el reenvío literal y `EnterPanelModal`;
- la cadena interna `_bridge` y `_core` no cambia;
- toda llamada a `ShowModalWindow`, `ShowDialog` o `MessageBox` de las carpetas Workspace va dentro del ámbito. Hoy no hay ninguna.

**Relación con F4-T2B2B:** `SetModalRackCadActive` es una entrada que olvida la secuencia de cierre. Un modal entre un Allow y la destrucción da una retirada conservadora, que es segura. Es un residual de Smoke-2.

### D-T3-06. Compuerta de avisos: ninguna lectura disparada desde `StateChanged` (I-3; F4-E33 «defensa frente al drenaje»)

- **Contador puro** `WorkspaceNoticeDepth` (en `WorkspacePilotSupport.cs`).
- **Decorador puro** `WorkspacePilotEditGate : IWorkspacePilotEditPort`, en el mismo archivo, alrededor del puerto del host:
  - mientras la profundidad es > 0, `TryBeginEdit`, `TryCheckNow` y `TryReload` responden `Waiting` («dentro de un aviso del panel») sin delegar y sin leer;
  - lo demás delega.
- **El controlador:**
  - incrementa la profundidad durante todo `NotifyStateChanged`, en `try/finally`; es memoria, sin `Dispatcher` y sin `+=`;
  - expone `public IWorkspacePilotEditPort PilotEdit` como un adaptador anidado que solo delega en el decorador (M-8). La UI nunca recibe el objeto `WorkspaceHostCore`;
  - el getter accede a `Navigation` antes de devolver, para que el lector de la intención esté asignado (T2B2 `:140`).
- **Regla vinculante para T3B:** solo un gesto explícito del usuario dispara la intención, «Comprobar ahora» o la recarga. Nunca un manejador de `StateChanged`: la compuerta lo impide en ejecución en toda emisión desde `NotifyStateChanged`.
- **Vía fuera de la compuerta** (re-revisión, M-g). El host de la paleta llama a `OnControllerStateChanged()` directamente en su constructor (`WorkspacePaletteHost.cs:63`). Esa vía ocurre dentro de RACKPANEL con un comando activo y la cubre `PassGate`. T3B la revisa cuando toque la paleta.
- **Prueba:** una intención llamada dentro de un aviso simulado da `Waiting` y cero lecturas.

### D-T3-07. Avisos de veto para el panel (I-5; P-03 «aviso encolado del panel»; D-17)

- En `AdviseClose`, en cada camino que devuelve Veto, el host encola un aviso de veto en memoria con:
  - la sesión;
  - los RackIds en riesgo, que salen de la decisión de `EvaluateClose`;
  - y, si la evaluación lanzó, el indicador «incierto», sin lista de racks. `AdviseClose` no puede recorrer `_drafts`.
- La cola es acotada: a lo sumo un aviso por sesión, el último. `TakeCloseNotices` la consume.
- **Avisos caducados** (re-revisión, M-b). El aviso de una sesión se quita en estos casos:
  - cuando la sesión se descarta (`DiscardRack` deja la sesión sin riesgo, o `DiscardDocument`);
  - cuando el siguiente consejo da Allow; se inserta antes de `_knownClean.Add`, que la guarda solo exige justo antes del `return`;
  - cuando la sesión se retira.
- **Sigue igual:** no lee, no autoriza, no descarta y no retira; la marca «limpio conocido» y la decisión no cambian.
- **Cambio declarado** de `WorkspaceLifecycleGuardTests.cs:309-325`. El orden de la marca y las prohibiciones se conservan, y se admite el encolado en los caminos de Veto.
- **El texto del mensaje** es de T3B (panel) y de T3C (línea de comandos).

### D-T3-08. Invariante del texto vivo (I-6; obligación 15)

- **Invariante:** la caja nunca contiene texto del usuario que el borrador no tenga.
- **Mecanismo:**
  - cada edición del usuario se refleja en el borrador con `SetPendingText`;
  - si `SetPendingText` se rechaza (`Rejected`, `NotEditable` o `Busy`), T3B restaura la caja desde `GetField` y avisa de forma visible (R-B4).
- **Consecuencias:**
  - las fronteras no bloqueantes no necesitan capturar nada, porque el borrador ya tiene el texto;
  - el veto de `BeginDocumentClose` ve el pendiente sin vaciar nada antes. Por eso el envoltorio de T3C no vacía texto vivo, y lo declara.
- **Prueba de T3A:** tras cualquier secuencia de `SetPendingText`, aceptados o rechazados, `GetField` devuelve el último texto aceptado.

### D-T3-09. Arrastres de F4-T2B2B y F4-T2C (orden del Owner)

**De F4-T2B2B:**

| Arrastre | Disposición |
|---|---|
| M-1 | Va a T3C, donde se envuelve el asesor (§8) |
| M-2 | `_knownClean.Remove` pasa a una cota inferior, y cada operación nueva que muta tiene que llamar a `NotePilotEdit` o quitar la marca. `.DiscardAndReload(` pasa a cuerpo propio de la recarga. La interfaz del asesor se fija por sus dos firmas, sin el conteo de `)\s*;`. La cola de pérdidas sigue `internal`, y la guarda admite `TakeLossNotices` como acceso del puerto |
| D-1 | `Drafts/` contiene los 5 archivos, como cota que nombra cada uno. `WorkspaceDraftBoundaryTests.cs` no cambia |
| M-3 | `AnInPlaceEdit...` afirma solo la vía tomada (conocida segura, sin comparador), no `"silent"` |
| M-4 | La documentación de `Waiting` (puede llegar tras la lectura, en la recarga) y de `NotAdmitted` (en la recarga conserva el borrador), respetando las guardas de texto. También la documentación de `EditDiscardOutcome`/`Discarded` (D-T3-02) |
| M-5 | La prueba del NotAdmitted de la recarga afirma `Detail` y `Witness`. La de la excepción del descarte arranca el núcleo y afirma que la sesión no se degrada |

**De F4-T2C:**

| Arrastre | Disposición |
|---|---|
| 1 | G-1 y G-2 prohíben también el nombre del método intermedio que extrae G-10, en los bloques `case`, en las líneas de manejador y en `OnIdleTick`. El intermedio se nombra exactamente 2 veces (declaración y argumento del planificador). `\.OnIdle\s*\(` con cualquier receptor = 1. Los miembros del planificador que llama el puente son exactamente `OnIdleTick` y `SetPost` |
| 2 | G-3 y G-10 fijan la forma exacta del post: el argumento de `SetPost`, aplanado, es `action=>_dispatcher.BeginInvoke(DispatcherPriority.Background,action)` o su otro orden. El puente prohíbe `ExecuteInApplicationContext` y `Dispatcher\.Run\s*\(`, con autopruebas |
| 3 | Las tres pruebas del cierre pendiente afirman `TotalReads == 0` y llaman a `AssertNoReadInsideACallback` |
| 4 | La documentación de la clase del puente nombra el planificador y el post. El fallo de la pasada (`AutoCadEventBridge.IdleContinuation.Pass`) y el aviso Debug (`AutoCadEventBridge.IdleContinuation.OpenTransaction`) usan contextos distintos. Se actualizan los literales de las mutaciones de `WorkspaceIdleSchedulingGuardTests.cs` |

**De F4-E33 y de los residuales de T2B2B (I-9):**

| Arrastre | Disposición |
|---|---|
| N-5 | La vía del controlador sigue abierta. **No aplica**, y la cabecera de `WorkspaceEditIntentGuardTests.cs:16-17` se corrige en T3B, que toca la exposición |
| N-2 y la compuerta de `ReadPreviewWitness` | Latentes por D-T3-04; van a F5 |
| La defensa frente al drenaje | D-T3-06 |
| `BeginDocumentClose` sin enganchar (T2B2 `:434`) | Un fallo al enganchar degrada la sesión, pero **la edición sigue permitida en modo degradado** (D-04). `PassGate` no mira la degradación (`WorkspaceHostCore.cs:619-653`), así que la intención se admite y los borradores de esa sesión **no tienen veto**. La pérdida sigue siendo visible por `DocumentToBeDestroyed`, el registro y la cola. **Residual de Smoke-2**. T3A no cambia `PassGate`; bloquear la edición sería una A-n del Owner fuera de T3 (re-revisión, N-1) |
| `Stop()` sin asesor (T2B2 `:204`) | Residual. `Dispose` no tiene llamador; va a F7 |

### D-T3-10. Pruebas de T3A (en Core, con el banco existente)

Archivos nuevos:
- `tests/RackCad.Tests/Workspace/WorkspacePilotEditHostTests.cs`;
- `tests/RackCad.Tests/Workspace/WorkspacePilotEditGuardTests.cs`.

**Cobertura del Owner a nivel de host.** Las pruebas 9, 10, 12, 15 y 16 son de T3B.

| N.º | Prueba |
|---|---|
| 1 | Válido + `CommitField` → `CurrentDraftState` con el valor, pendiente limpio y `NotePilotEdit`. `CrossBoundary(TabChange)` válido → `Proceeded` |
| 2 | Inválido + `CommitField` → `Invalid`, con el texto intacto y el valor sin cambios |
| 3 | Inválido → `IsProtected`, en riesgo y veto del cierre |
| 4 | `TabChange` y `RackChange` con inválido → `Aborted`, con el campo y el mensaje, y el texto intacto |
| 5 | Cada no bloqueante con inválido → `Kept`, con el texto carácter a carácter, sin validar ni comprometer, sin `Authorize` |
| 6 | Tras una no bloqueante, `GetField` devuelve el mismo pendiente |
| 7 | Cambio de documento: el pendiente de A sigue en A |
| 8 | Volver a A: `GetField(A)` lo devuelve |
| 11 | `DiscardRack` quita el borrador y el pendiente de un rack, sin comparador (también con uno que lanza) y sin escritura. Los demás racks y sesiones no cambian |
| 13 | Sin descartar, el estado persiste tras consultas y fronteras no bloqueantes |
| 14 | `DiscardRack` y `DiscardDocument` quitan el estado, y el cierre siguiente se permite |
| 17 | Aislamiento: las operaciones de A no tocan B. Una mutación bloqueante de B con el documento A activo → `Rejected` |
| 18 | `EnterPanelModal`/`WorkspacePanelModalScope`:<br>- activa antes de devolver y desactiva en `Dispose`, también con excepción;<br>- con anidamiento, desactiva el último;<br>- el `Dispose` doble es idempotente;<br>- con el modal activo, la intención responde `Waiting` y `CanDrainNow` es falso |

**Además:**
- `GetField` en cada estado, incluido `Undecidable` con un comparador que lanza;
- `SetPendingText` rechazado: con `CanEdit` falso, sin `Authorize` y con una lectura en curso (`Busy`);
- la clave es siempre la del adaptador;
- el invariante del texto vivo (D-T3-08);
- **la compuerta de reentrada:** un lector simulado que, dentro de `ReadIntent`, llama a `SetPendingText` (`Busy`), y el pendiente previo sobrevive a la recarga o se rechaza de forma visible;
- **la compuerta de avisos:** la intención dentro de un aviso → `Waiting` y cero lecturas;
- D-T3-03 con sus propiedades;
- `TakeLossNotices` y `TakeCloseNotices`: en orden, vaciando la cola, con un aviso de veto por sesión y la marca de incierto;
- `GetRacksAtRisk` con un comparador que lanza y para una sesión no activa;
- **oráculo crítico:** un escritor simulado registra cero escrituras en toda operación del puerto.

**Guardas nuevas** (regla I-1: limitadas a T3A y a sus cuerpos, enums como prefijo ordenado, nunca «el puerto no tendrá jamás escrituras»), con autopruebas:
- los valores del puerto no nombran tipos de `Drafts` ni del piloto;
- la implementación es explícita en el host;
- `NotePilotEdit` está en cada operación nueva que muta;
- la compuerta de reentrada envuelve las tres lecturas;
- el decorador de avisos;
- el adaptador anidado del controlador;
- el ámbito del modal;
- **T3A no añade operaciones de escritura authored** (re-revisión, M-e). La guarda se redacta **por cuerpo**: los cuerpos de las operaciones de T3A, nombradas una a una, y los del decorador no nombran ninguna API de escritura (el patrón `WriteApis` de `WorkspaceLifecycleGuardTests.cs:533-535`). Las operaciones futuras quedan fuera de esa lista. Una autoprueba demuestra que T4 podría añadir `Update` al enum y una operación de Actualizar sin romperla. Actualizar irá por este puerto o por otro, según decida T4 (H-15 de F4).

### D-T3-11. Oráculo crítico (orden del Owner)

Editar en T3 produce **cero escrituras authored del DWG**. Si algún camino escribe estado authored: STOP y defecto de producto (parada C03).

## 5. Contrato propuesto de F4-T3A-EDIT-PORT

**AllowedWriteScope** (12 archivos exactos):

| Archivo | Papel |
|---|---|
| `src/RackCad.Application/Workspace/IWorkspacePilotEditPort.cs` | nuevo |
| `src/RackCad.Application/Workspace/WorkspacePilotSupport.cs` | nuevo: `WorkspacePilotValueText`, `WorkspacePanelModalScope`, `WorkspaceNoticeDepth` y `WorkspacePilotEditGate` |
| `src/RackCad.Application/Workspace/WorkspaceHostCore.cs` | |
| `src/RackCad.Application/Workspace/WorkspaceEditIntent.cs` | M-4, la documentación del descarte y `EditDiscardStatus.Busy` al final |
| `src/RackCad.Plugin/Workspace/WorkspaceController.cs` | |
| `src/RackCad.Plugin/Workspace/AutoCadEventBridge.cs` | T2C 4 |
| `tests/RackCad.Tests/Workspace/WorkspacePilotEditHostTests.cs` | nuevo |
| `tests/RackCad.Tests/Workspace/WorkspacePilotEditGuardTests.cs` | nuevo |
| `tests/RackCad.Tests/Workspace/WorkspaceLifecycleGuardTests.cs` | M-2, D-1 y el cuerpo de `AdviseClose` |
| `tests/RackCad.Tests/Workspace/WorkspaceLifecycleHostTests.cs` | M-3 y M-5 |
| `tests/RackCad.Tests/Workspace/WorkspaceIdleSchedulingGuardTests.cs` | T2C 1, 2 y 4 |
| `tests/RackCad.Tests/Workspace/WorkspaceIdleSchedulerTests.cs` | T2C 3 |

**ForbiddenWriteScope:** prefijos seguros:
- el resto de `Application`, incluido `Drafts/`;
- `Domain` y `UI`, con la clausura;
- el resto del Plugin, incluidos los otros 7 archivos de `Plugin/Workspace` y `WorkspacePanelCommand.cs`;
- las demás pruebas y guardas;
- `tests/RackCad.UI.Tests/`;
- la raíz.

**Invariantes:**
- **INV-F4T3A-01** a **INV-F4T3A-10:** uno por cada decisión, de D-T3-02 a D-T3-11;
- **INV-F4T3A-11:** los cambios declarados de D-T3-07 y D-T3-09 y ningún otro;
- **INV-F4T3A-12:** las trampas de H-10, H-11 y H-12 como límites vigentes;
- **INV-F4T3A-13:** la cobertura RED/GREEN:
  - el esqueleto es inerte y no lanza;
  - las pruebas de comportamiento fallan por aserción;
  - los arrastres que solo tocan guardas pueden pasar en el RED, y demuestran que discriminan con autopruebas de mutación;
- **INV-F4T3A-14:** los límites: sin UI, sin comando, sin escritura, sin tocar la clausura ni `Drafts/`, y sin `+=`.

**RequiredTests:**
- `FullyQualifiedName~RackCad.Tests.Workspace`, MinSelected **2071** (CI `43319785427` de `33e43f9f`; mismo producto que `94b4d468`), ExpectRed true;
- `FullyQualifiedName~RackCad.UI.Tests.Workspace`, MinSelected 248, ExpectRed false.

**StopConditions:** las S y P comunes, más:
- **C01:** un archivo nuevo en `Plugin/Workspace` o en `Drafts/`, o un cambio fuera del alcance;
- **C02:** un cambio en guardas o pruebas existentes fuera de los declarados;
- **C03:** una escritura authored, o una lectura del DWG fuera de las tres operaciones de la intención;
- **C04:** UI, un comando o un cambio de la clausura;
- **C05:** una suscripción o un `+=` nuevo;
- **C06:** una semántica congelada que cambie, o el comportamiento cerrado de F4-T1, F4-T2B2B o F4-T2C;
- **C07:** la tarea se evalúa como Long-horizon;
- **C08:** la regla de replanificación.

## 6. Puntos para la re-revisión de la v2

1. ¿Cierran las correcciones I-1 a I-9 sin defectos nuevos? En particular:
   - la tabla K = 1 y su regla;
   - las compuertas de reentrada y de avisos;
   - la cola de veto en `AdviseClose`;
   - el invariante del texto vivo.
2. ¿Son implementables, y discriminantes, las guardas nuevas y los cambios declarados de T3A, sin romper guardas no declaradas? Revisa en particular:
   - el nombre de los tipos nuevos frente a H-10;
   - el cuerpo de `AdviseClose`;
   - el decorador.
3. ¿Quedan en el §7 y el §8 reglas de T3B y T3C que hayan de fijarse ya en el puerto de T3A?

## 7. F4-T3B-EDIT-UI: reglas vinculantes (su paquete se fija tras cerrar T3A)

| Regla | Contenido |
|---|---|
| R-B1 | Superficie de edición en la superficie K = 1 (un VM nuevo en `src/RackCad.UI/Workspace`), con el campo, el estado, el mensaje de validación, el estado «No editable aquí» con el diagnóstico y la indicación de RACKEDITAR sin lanzarla (C-F4-07), y Error en la UI (D-08) |
| R-B2 | Marca «Nombre *» cuando `IsProtected` (D-08), en el `Title` de la pestaña |
| R-B3 | **Primer gesto.** Una pulsación o un clic en el campo, «Editar» o el doble clic: primero **fija** la preview y después lanza la intención. Con `NotSupplied`, se renderiza sin aplicar el gesto, con un indicio visible (D-T3-04). `Fijar` solo fija. Una pestaña protegida nunca es preview. **Recarga perezosa (O-01;** re-revisión, M-c**):** en una activación por aviso, la superficie de un borrador Limpio marcado muestra «posiblemente obsoleto», y el primer gesto sobre él llama a `TryReload` antes de editar |
| R-B4 | Cada edición del usuario llama a `SetPendingText`, nunca un `Show` (D-T2-01, O-16). Si se rechaza, la caja se restaura desde `GetField` y hay aviso (D-T3-08). Enter o salir del campo llama a `CommitField`. Esc no revierte (H-03); el descarte explícito es la vía de reversión |
| R-B5 | Las transiciones, según la tabla K = 1 del §2: las bloqueantes antes del cambio; si abortan, el texto sigue en su caja y hay aviso. Las no bloqueantes no abortan nunca. Rematerializar llama a `GetField` y hace `Show` del pendiente |
| R-B6 | **Sincronización por selección con texto pendiente en la pestaña activa:** la pestaña sigue activa y la preview de R va en segundo plano, con aviso (`P5:335-336`). Con la pestaña solo dirty, R se activa y la anterior se desmaterializa conservando su borrador. Exige un cambio declarado de `BrowserSession` y de sus guardas, con un vocabulario sin `Draft` ni `Dirty` |
| R-B7 | **Cerrar.** Una pestaña limpia se cierra sin preguntar y llama a `DiscardRack`, sin efecto en el DWG, para que el borrador termine. Una protegida pregunta Descartar o Cancelar en línea, sin subdiálogo; si alguna vez hay subdiálogo, va dentro de `EnterPanelModal`. Descartar llama a `DiscardRack`; Cancelar conserva todo. Cerrar nunca escribe |
| R-B8 | **«Descartar borradores de este documento»:** lista los racks (nombres de pestaña, o el RackId si no hay nombre). Tras confirmar, vuelve a leer `GetRacksAtRisk`; si el conjunto cambió, no descarta y avisa (TOCTOU, I-7). Si no hay racks en riesgo: un mensaje y ningún efecto |
| R-B9 | Avisos en el panel: `TakeCloseNotices` (veto, con las dos vías de descarte) y `TakeLossNotices` (pérdida), consumidos fuera de todo callback |
| R-B10 | Solo un gesto del usuario lanza la intención, «Comprobar ahora» o la recarga (D-T3-06) |
| R-B11 | Foco solo donde T3B lo toca directamente (R-3 de F3), sin pulido amplio |
| R-B12 | Cobertura 1-18 a nivel de UI, en especial 9, 10, 12, 15 y 16. Cambio de pestaña sin escribir la selección, la vista ni el DWG. K = 1 mientras se edita. Son cambios de guarda declarados: el constructor del VM, la paleta, `BrowserSurface`, el record de pestaña o el `Title`, el foco y el vocabulario |

## 8. F4-T3C-CLOSE-DISCARD: reglas vinculantes (su paquete se fija tras cerrar T3B)

| Regla | Contenido |
|---|---|
| R-C1 | `RACKPANELDESCARTAR`, segundo `[CommandMethod]` en `WorkspacePanelCommand.cs`. No crea el controlador: usa `IsCreated`, y `GetOrCreate` sigue apareciendo una sola vez (M-12). Opera solo sobre el documento activo. Un destino ambiguo falla con un mensaje claro y sin efecto (C-F4-03). Lista los racks en riesgo **por RackId**: el estado del piloto no lleva nombre, y los nombres de pestaña viven en la UI (re-revisión, M-f). Confirma con `GetKeywords` (Sí o No; por defecto No). Tras «Sí», vuelve a leer y, si el conjunto cambió, no descarta (TOCTOU). Sin racks en riesgo: un mensaje y ningún efecto. Cero escrituras |
| R-C2 | Envoltorio del asesor, anidado en el controlador, que delega sin cambiar la decisión y contiene sus excepciones; si la delegación lanza, Veto. **Veto:** un aviso no modal en la línea de comandos que nombra el documento, resuelto en `AdviseClose` con `TryGetSession` del puente y `DocumentManager` (lectura de memoria, P-03), y las dos vías de descarte; para un documento vetado que no es el activo, indica que hay que activarlo primero. **Pérdida:** el envoltorio la calcula con `GetRacksAtRisk(session)` **antes** de delegar `SessionDestroyed`. Es memoria sin `Authorize`, y los borradores siguen en el host hasta la retirada; no consume la cola, que es del panel (re-revisión, M-f). Se escribe en el siguiente `DocumentActivated`, en el relevo del controlador, porque el historial de órdenes del documento destruido desaparece con él; con QUIT queda solo RackLog. Ambas ampliaciones acotadas de D-04 se declaran. El envoltorio no vacía texto vivo (D-T3-08) |
| R-C3 | Cambios de guarda declarados: M-1, el censo de comandos (`WorkspaceHostSourceGuardTests.cs:36-63`, `WorkspacePaletteHostSourceGuardTests.cs:320-329`, `WorkspaceBridgeSourceGuardTests.cs:333-351`, `WorkspaceSourceGuardExceptionTests.cs:30-61`, `CustomPropertiesCommandGuardTests.cs:55` y `SelectiveEditorOpenTests.cs:566`), `GetKeywords` por archivo exacto y `WriteMessage` en el archivo del comando. Los textos no usan «aun no» ni «todavia no» (`WorkspacePaletteHostSourceGuardTests.cs:311-317`). El esqueleto del RED lleva ya el atributo del comando, para que los censos de fuera del filtro pasen |
| R-C4 | Evidencia dirigida en el host donde sea practicable (`GetKeywords`, `WriteMessage` en `BeginDocumentClose`); si no, limitación de Smoke-2 |

## 9. Disposición de la revisión adversarial de la v1

| Hallazgo | Disposición en la v2 |
|---|---|
| I-1 (falta el mapa K = 1) | **Aceptado:** H-07, H-08, la tabla de transiciones y su regla (§2) |
| I-2 (reentrada en la recarga) | **Aceptado:** compuerta de reentrada (D-T3-02), con prueba |
| I-3 (lecturas desde avisos) | **Aceptado:** compuerta de avisos (D-T3-06), con regla R-B10 y prueba |
| I-4 (datos del envoltorio) | **Aceptado:** el envoltorio pasa a T3C con su vía fijada (R-C2). `GetRacksAtRisk` y `GetField` van sin `Authorize`, justificado (D-T3-02) |
| I-5 (veto en el panel; Error) | **Aceptado:** cola de veto (D-T3-07). Error es de la UI: Recargar rematerializa desde el borrador y Descartar llama a `DiscardRack` (R-B1) |
| I-6 (texto vivo fuera del borrador) | **Aceptado:** invariante y mecanismo (D-T3-08, R-B4) |
| I-7 (TOCTOU del descarte) | **Aceptado:** R-B8 y R-C1 |
| I-8 (guardas permanentes) | **Aceptado:** guardas limitadas a T3A, enums como prefijo y autoprueba de T4 (D-T3-10). Se aplica la regla I-1 en el diseño; no se clasifica ningún fallo |
| I-9 (arrastres de F4-E33 y residuales) | **Aceptado:** tabla en D-T3-09 |
| M-1 (presupuesto y tamaño) | **Aceptado:** el Owner decidió las tres subtareas antes de emitir |
| M-2 (algoritmo del texto) | **Aceptado:** D-T3-03 |
| M-3 (preview sin valor) | **Aceptado:** destino F5, marcador e indicio (D-T3-04) |
| M-4 (trampas) | **Aceptado:** H-10 e INV-F4T3A-12 |
| M-5 (RequiredTests) | **Aceptado:** MinSelected 2071. El censo de comandos pasa a T3C (R-C3) |
| M-6 (archivo ambiguo) | **Aceptado:** un archivo con sus cuatro tipos nombrados |
| M-7 (modal) | **Aceptado:** D-T3-05, con la frase de `ShowModalWindow` corregida |
| M-8 (exposición del host) | **Aceptado:** adaptador anidado y decorador (D-T3-06) |
| M-9 (contención y semántica) | **Aceptado:** D-T3-02 (no editable → `Kept`, mecánica de `CommitField`) y R-B4 (Esc). El envoltorio, en T3C (R-C2) |
| M-10 (`Unknown`) | **Aceptado:** `Undecidable` distinto de `Unknown` |
| M-11 (coherencia) | **Aceptado:** la excepción de `TakeLossNotices`, la documentación del descarte y las órdenes custodiadas en el commit del paquete |
| M-12 (comando; alias; host) | **Aceptado:** R-C1, T2C 1 (D-T3-09) y R-C4. T3A no cambia el foco ni el modal de AutoCAD, así que no necesita evidencia de host |

## 10. Disposición de la re-revisión de la v2 (EMITIBLE TRAS CORRECCIONES, 0/2/9)

La re-revisión confirma que I-1, I-2, I-3, I-5, I-6, I-7 e I-8 y los doce menores de la v1 quedan cerrados. I-4 e I-9 quedaban parcialmente abiertos, y se cierran con N-1 y M-f.

No encuentra guardas no declaradas que rompan T3A:
- los nombres nuevos esquivan las trampas de H-10;
- el decorador y el contador pueden vivir en `Application/Workspace`;
- la profundidad en `try/finally` dentro de `NotifyStateChanged` cumple las guardas del controlador;
- el cambio del cuerpo de `AdviseClose` está declarado.

| Hallazgo | Disposición |
|---|---|
| N-1 (enganche fallido apoyado en un hecho falso) | **Aceptado:** la fila de D-T3-09 se reescribe. La edición sigue permitida en modo degradado, sin veto, y es un residual de Smoke-2. T3A no cambia `PassGate` |
| N-2 (la retención se ampliaba a lo dirty) | **Aceptado:** la tabla K = 1 y R-B6 siguen la letra de `P5:335-336`. Solo el texto pendiente retiene la pestaña activa |
| M-a (`Busy` en todos los tipos; lecturas anidadas) | **Aceptado:** D-T3-02 (`PilotBoundaryDecision.Busy` aborta, `EditDiscardStatus.Busy` al final, lecturas anidadas en `Waiting`, marca en `try/finally`) |
| M-b (incierto como indicador; avisos caducados) | **Aceptado:** D-T3-02 y D-T3-07 |
| M-c (recarga perezosa O-01) | **Aceptado:** R-B3 |
| M-d (12 o 13 archivos) | **Aceptado:** 12 |
| M-e (guarda de escritura por cuerpo) | **Aceptado:** D-T3-10 |
| M-f (datos de R-C1 y R-C2) | **Aceptado:** los racks se listan por RackId y la pérdida se calcula con `GetRacksAtRisk` antes de delegar (R-C1, R-C2) |
| M-g (vía de la paleta; enums con nombre) | **Aceptado:** D-T3-06 y D-T3-02 |
| M-h (cerrar una pestaña limpia termina el borrador) | **Aceptado:** la tabla K = 1 y R-B7 (`DiscardRack`, sin efecto en el DWG) |
| M-i (guarda del modal sobre la cadena existente) | **Aceptado:** D-T3-05, sobre los dos puntos de entrada públicos |
