### PB-T4c

**Desde la v5.0, entero y literal** (V50-01 a V50-03, V50-08, V50-10, V50-14 y V50-15): medido por objetos git en el cierre de PB-T4b (`6adff90d`, F4B-E53). El texto del GREEN y del esqueleto es el de las piezas `pbt4c-green-v50.txt` y `pbt4c-skeleton-v50.txt` (§6.3.2), copiadas en el anexo V50-A. Las citas de líneas son de `6adff90d` (`v50/remeasure_pbt4c_v50-out.txt`).

#### D-PBT4-12. El gesto, con `CommitField` como frontera (R-PB-10; I-02)

- **El botón** (PV): `BrowserFieldEditAllButton`, `Content="Editar todo"`, `AutomationProperties.Name="Editar el rack completo en el editor"`, en el `WrapPanel` de los botones del campo (PV:218-254), justo tras «Editar» (PV:219-223). Su `Visibility` es `Browser.Field.ShowEditAllButton` y su `Click`, `OnBrowserFieldEditAllClick`, que en `WorkspacePanelView.xaml.cs` es `Contained(() => _viewModel.Browser.Field.EditAll())` (junto a `OnBrowserFieldEditClick`, :83).
- **Cuándo se ofrece** (V50-14): la v4.9 decía «en las mismas condiciones que «Editar»». «Editar» (`ShowEditButton`, FVM:1136) solo se ofrece con la caja en solo lectura, así que desaparecería en cuanto el borrador está abierto y editable. Por eso `ShowEditAllButton` tiene las condiciones de «Editar» **salvo la de solo lectura**:
  - una superficie fijada (no la vista previa: el gesto no pasa por el navegador, que fija la pestaña de «Editar»);
  - sin fallo, sin diagnóstico que se quede y sin elección pendiente;
  - y sin borrador, o con uno que el editor abre (`CanEdit`: registrado y Clean, Dirty o Conflict).
- **El gesto `EditAll()`** (FVM, junto a `BeginEdit`):
  1. `GetField`; con texto pendiente, `_port.CommitField(origin, rack)`, «the same two-phase step as a blocking boundary» (WHC:1187):
     - `Invalid`: no abre; el campo muestra el texto tal cual, con su mensaje, y el aviso «No se abrió el editor: <mensaje>.»;
     - `Busy` o `Rejected`: no abre, y el aviso es el de hoy para un texto que no se aplicó (`RefusalOf`, FVM:1225);
     - `Applied` o `NotEditable`: sigue;
  2. `_port.TryEditAll(new EditIntentRequest(session, rack, null, null))`: sin referencia (el origen lo guarda el anfitrión, D-PBT4-03) y sin testigo;
  3. si la superficie cambió entre tanto, el aviso de hoy (`SurfaceChangedText`); si el anfitrión no empezó el borrador con una lectura no admitida, su diagnóstico y su pista, como `BeginEdit`; si no, el aviso de `EditAllNoticeOf`: «Editor cerrado: sus cambios están en el borrador.» con algún aplicar, «Editor cerrado sin cambios en el borrador.» sin ninguno, y para lo demás el prefijo de hoy («En espera: », «Rechazado: », el de `Describe` de la intención) o «No se abrió el editor: <motivo>.»;
  4. `Render()` (`GetField`) y `_changed?.Invoke()`: el navegador rehace su tira (el resumen y « *» salen de la vista del campo, como hoy).
- **Sin valor nuevo en `PilotBoundaryKind`:** URT:1108-1109 sigue en verde.
- **El anfitrión no abre si el borrador aún tiene texto pendiente,** por si el gesto no lo confirmó: «Hay texto sin confirmar en el campo.».

#### D-PBT4-13. El anfitrión, los dos reenvíos y el puerto generalizado (A-T3-11, A2-1; B-05)

**`TryEditAll` es el único miembro nuevo del puerto** (`IWorkspacePilotEditPort.cs`, al final, tras `TakeUpdateProgress`), con cuerpo por defecto, `=> EditAllOutcome.NotAvailable(null);`, para que el doble `WorkspacePilotEditPortDouble` no se edite (C02). **`EditAllOutcome`** (un valor) y **`EditAllStatus`** (`NotAvailable`, `Waiting`, `Rejected`, `NotBegun`, `Refused`, `Closed` y `Failed`; el valor por defecto es `NotAvailable`) van en WEI, tras `EditIntentOutcome`. `EditAllOutcome` lleva `Status`, `Reason`, `Begin` (el `EditIntentOutcome` de la intención que no empezó el borrador) y `Applied` (los aplicar del editor).

**En WHC** (V50-03):
- el campo `_fullEditHost` (`IWorkspaceFullEditHost?`, de `Application.Workspace`: EIH:1079 sigue con un solo campo de `Systems`, `_updateHost`) y su asignación `SetFullEditHost`, junto a `SetPilotUpdateHost` (WHC:292);
- **la operación del puerto, explícita,** `EditAllOutcome IWorkspacePilotEditPort.TryEditAll(EditIntentRequest request)`, tras la de `TryDiscardAndReload` (WHC:1130-1142): **la marca de lectura como envoltorio** (`_pilotReadInProgress`, como `TryKeepMine`, WHC:1116-1128), **mantenida durante `ShowModal`**;
- **su cuerpo,** el ayudante privado `EditAllOfPilot`, con las reglas de `TryKeepPilotEdit` (WHC:652-751; I-09): `PassGate` primero (`Unavailable` → `NotAvailable`, `Waiting` y `Rejected`), `UpdateInFlightForRequest` después, la contención con `Report("WorkspaceHostCore.TryEditAll", …)`, y `NotePilotEdit(session)` por cada aplicar.

**Los resultados** (medidos con la sonda de la precondición 6, §5.3):
- sin anfitrión del editor: `NotAvailable`, «No editable aquí: el panel no tiene el editor completo.», sin leer;
- **sin borrador, lo empieza:** `TryBeginPilotEdit` (su lectura, su admisión y su diagnóstico); si no admite (la cola aparcada, AUTH-13), `NotBegun` con ese resultado y sin abrir;
- Clean, Dirty o Conflict: abre;
- Applying, o un borrador que ya no está registrado: `Waiting`, «El borrador se está aplicando o se retiró: el editor no se abre.»;
- Unknown, Error o Undecidable (`CanEdit` falso): `Refused`, «El borrador no admite el editor completo en su estado actual.»;
- con texto pendiente: `Refused`, «Hay texto sin confirmar en el campo.»;
- otra sesión: `Rejected`, el motivo de `PassGate`;
- al cerrar: `Closed`, con los aplicar.

**Los dos reenvíos entre el panel y el anfitrión** (B-05; R-22). El panel recibe `new PilotEditAdapter(new WorkspacePilotEditGate(_host, _noticeDepth))` (WCo:52). Con cuerpo por defecto, ninguno reenviaría. Los dos reenvían, de forma explícita:
- **la compuerta** (`WorkspacePilotSupport.cs:153-209`): `public EditAllOutcome TryEditAll(EditIntentRequest request) => _depth.IsInside ? EditAllOutcome.Waiting(InsideNoticeReason) : _inner.TryEditAll(request);`, tras `RequestUpdate`. Lo prueba CT-PBT4C-12;
- **el adaptador del Plugin** (`PilotEditAdapter`, WCo:283-327): `public EditAllOutcome TryEditAll(EditIntentRequest request) => _gate.TryEditAll(request);`, al final. Lo prueba la guarda de texto CT-PBT4C-13.

**El lanzamiento y el adaptador del puerto,** en `Draft/` (puros, CT-PBT4C-15):
- `PushBackFullDraftLaunch.For(DraftSession<PushBackFullDraftState> draft, IPushBackDraftStatePort port)`: el `Editable` actual del borrador (la ventana lo copia al cargar), el `RackId` y el proyecto del sobre del `Origin` (`RackEmbedStore` y `RackProjectStore`, como `PushBackFullDraftMaterializer`; nulo si el texto no lo trae, que el lanzamiento admite);
- `PushBackFullDraftStatePort : IPushBackDraftStatePort` (`ctor(draft, Action applied)`): `ApplyToDraft(edited)` llama a `SetCurrentDraftState(new PushBackFullDraftState(edited, draft.CurrentDraftState.Origin))`, así que **el origen se conserva**; con `Updated`, llama una vez a `applied()` y responde `Applied`; si no, `NotEditable`.

`PushBackDraftSessionPort` y `LaunchFor` (PB-T2a) se conservan sin uso, con su registro en DISP.

**Las restricciones medidas que el GREEN cumple** (V50-10; las guardas siguen en verde en la suite completa, §4.3):
- **WHC conserva sus dos interfaces:** la copia de PEG:379-386 en `WorkspacePilotEditGuardFull.cs:266-272`, que fija `CopySha`, exige `IWorkspaceLifecycleAdvisor` e `IWorkspacePilotEditPort` y nada más. Por eso `TryEditAll` va en el puerto, y no en una interfaz nueva de WHC;
- **los nombres de los tipos del puerto y del soporte** evitan `Draft|Editor|Subscription|Tab` (PEG:1015, `NameViolations`): `EditAllOutcome` y `EditAllStatus` los evitan, e `IWorkspaceFullEditHost` va en su propio archivo;
- **ningún tipo de `RackCad.Application.Workspace` lleva `Subscription|Editor|Draft` en su nombre** (`WorkspaceModelBoundaryTests:101`, por reflexión): por eso el host del editor completo se llama `IWorkspaceFullEditHost` (D-S9; el paquete nombraba `IWorkspaceFullEditorHost` desde la v4, y la precondición 6 lo vio caer en `/d/r5c`);
- **el comentario del puerto** conserva «only the six operations of the edit intention read the drawing» y «while one of the six reads is in progress» (PEG:1079). `TryEditAll` solo lee por la intención, y su comentario lo dice;
- **`EditAllOutcome` queda fuera del censo de portadores** (`CarriersWithoutCensus`, EIH y su copia: `^Edit(?:Intent|Check|Confirm|Update)(?:Request|Witness|Outcome|Start|Progress)$`);
- **ninguno de los 30 textos de la guarda de WHHG se repite** en WHC ni en el soporte (el equivalente de `NoneOfThe30…` cuenta cada uno): `TryEditAll` usa textos propios y, donde comparte uno, su constante (`PilotReadInProgressReason`, `InsideNoticeReason`);
- `ControllerViolations` (PEG:659-707): un solo `public IWorkspacePilotEditPort`, tres `+=`, ningún `-=` y un solo `event` en WCo;
- la compuerta no gana `static` (`GateViolations`, PEG:602-631, y su prueba, :633);
- en las carpetas del Workspace, ningún diálogo fuera de `EnterPanelModal` (`ModalScopes` y `DialogViolations`, PEG:809-839, y su prueba, :841): `FullEditorModal` solo llama a `_open`.

#### D-PBT4-14. Los estados de cada operación durante el editor (I-01; I-15)

**Medido** con la sonda de la precondición 6 (CT-PBT4C-05), con la ventana abierta (dentro de `ShowModal`; el doble pone el modal):

| Operación | Responde | Por qué |
|---|---|---|
| `SetPendingText`, `CommitField`, `CrossBoundary` (una frontera bloqueante), `DiscardRack` y `DiscardDocument` | `Busy`, «Hay una lectura del piloto en curso.» | Miran la marca (WHC:1165, :1193, :1246, :1281 y :1308) |
| `TryBeginEdit`, `TryCheckNow`, `TryReload`, `TryConfirmBase`, `TryKeepMine`, `TryDiscardAndReload` y `TryEditAll` | `Waiting`, el mismo texto | La marca, en el envoltorio de cada una (WHC:1058-1142 y el de `TryEditAll`) |
| **`RequestUpdate`** | **`Busy`, el mismo texto** | **Mira la marca antes que nada** (WHC:1426), antes de `PassGate` (:1430) |
| `GetField`, `GetRacksAtRisk`, `TakeLossNotices`, `TakeCloseNotices` y `TakeUpdateProgress` | De memoria, como hoy | |

El texto de `Busy` dice «lectura en curso» mientras el editor está abierto. **Ninguna lectura del dibujo** dentro (`lecturas-dentro=0`), y al cerrar el panel vuelve a responder. Se declara: en AutoCAD la paleta está deshabilitada por el modal WPF, y eso solo se ve en el Smoke-PB.

#### D-PBT4-15. El modal y el abridor, en dos piezas (I-01; B-07; M-14)

**Por qué dos piezas** (medido en `6adff90d`). Una sola no cabe en ningún sitio sin romper una guarda:
- **en WCo** (anidada) tendría que nombrar la ventana, de `RackCad.UI.Systems.PushBack`, y `WorkspaceHostSourceGuardTests.cs:398-421` prohíbe `RackCad.UI` en todo archivo de `Plugin/Workspace/` salvo la paleta y el puente;
- **en un archivo nuevo de `Plugin/Workspace/`** rompería las dos guardas de 9 archivos (EIG:1011-1024; `WorkspaceNavigationHostTests:713-723`);
- **fuera de `Plugin/Workspace/`,** si implementara una interfaz de `RackCad.Application.Workspace`, rompería `WorkspaceBridgeSourceGuardTests.cs:361-366`.

**Las dos piezas:**
1. **El modal, anidado en WCo,** tras `PilotEditAdapter`: `private sealed class FullEditorModal : IWorkspaceFullEditHost`, con `ctor(WorkspaceController controller, Action<object> open)` y un solo método, `public void ShowModal<TLaunch>(TLaunch launch) where TLaunch : class { using (_controller.EnterPanelModal()) _open(launch); }`.
   - **No nombra `RackCad.UI`** (398-421 sigue en verde) **ni `PushBack`** (WCo no entra en el censo de CT-PBT1B-15).
   - Se asigna en el getter de la navegación de WCo, justo tras `_host.SetPilotUpdateHost(new RackPilotUpdateCommandContext());` (WCo:110): `_host.SetFullEditHost(new FullEditorModal(this, RackWorkspaceEditorWindow.Show));`. EIG:1155-1184 sigue en verde.
2. **La ventana, en `src/RackCad.Plugin/Views/RackWorkspaceEditorWindow.cs`:** `internal static class RackWorkspaceEditorWindow`, con un solo método, `internal static void Show(object launch)`:
   - `var window = RackPushBackSystemWindow.ForDraft((PushBackDraftEditorLaunch)launch);` (W:296-300);
   - `AcApplication.ShowModalWindow(window);`, sin `Owner`, como el abridor clásico (`PushBackClassicCharacterizationPluginGuardTests.cs:195-210`);
   - **no nombra `RackCad.Application.Workspace`** (361-366 sigue en verde) ni ningún término de G-9 (IG:593-595); no lee ni escribe el dibujo, ni hace nada tras cerrar.

**`IWorkspaceFullEditHost`,** en `src/RackCad.Application/Workspace/IWorkspaceFullEditHost.cs`, con el método genérico `void ShowModal<TLaunch>(TLaunch launch) where TLaunch : class;`. WHC lo llama con el `PushBackDraftEditorLaunch`. Se declara como excepción a R-PB-05 y a ADR-0029 D12.

**La guarda que rompe: CT-PBT1B-15** (`PushBackSharedReferenceGuardTests.cs:306-403`, «archivos=14» en **:384**; la v4.9 citaba :381). `RackWorkspaceEditorWindow.cs` nombra `PushBack` (la ventana y el lanzamiento), así que el censo pasa a 15. **Ninguna carpeta lo evita:** todo archivo nuevo que abra la ventana nombra `PushBack`, y ponerlo en uno de los 14 existentes mezclaría el abridor con el camino clásico o con el escritor (R-31). Se sustituye con registro (C-5), con su equivalente: «archivos=15 (los 14 y el abridor); asignaciones=7», lo demás igual (§5.3).

#### D-PBT4-16. La guarda de OB-01.3 (M-05, M-09)

CT-PBT4C-07 mira **seis sujetos**, en texto sin comentarios:
- `RackWorkspaceEditorWindow.cs`;
- el cuerpo de `FullEditorModal` en WCo;
- los cuerpos de `TryEditAll` y de `EditAllOfPilot` en WHC;
- el de `EditAll` en FVM;
- `Draft/PushBackFullDraftLaunch.cs`;
- `Draft/PushBackFullDraftStatePort.cs`.

Ninguno nombra `RackPushBackCommands`, `RackMenuCommands`, `EditorModules`, `RackViewBatchExecution`, `DrawAndPlace`, `RedrawInPlace`, `InsertRequested` ni `InsertionRequest`. La autoprueba en negativo inyecta cada nombre en cada sujeto (48). En el RED, un sujeto que falta es su texto vacío, y la guarda pasa.

**`RackWorkspaceEditorWindow.cs` no lleva** `LockDocument`, transacción ni nada tras cerrar.

#### D-PBT4-17. «Cerrar» con las dos cosas (A-T2b-OWNER-01; M-12; D-S7 y D-S10)

`PendingWork()` de W (W:3427-3433), **solo en WORKSPACE**:
- un módulo escenificado **y** cambios sin aplicar: una sola pregunta, `WorkspaceStagedAndUnappliedQuestion` = «Hay un cambio de módulo sin confirmar y cambios sin aplicar al borrador; se perderán los dos al cerrar. ¿Deseas continuar?», con un `if` antes de la línea de hoy, que no cambia;
- solo cambios sin aplicar: la de hoy, «Hay cambios sin aplicar al borrador…»;
- **en CLASSIC, nada cambia:** `HasUnappliedDraftChanges()` es `false` fuera de WORKSPACE (W:3439-3441), y un módulo escenificado pregunta lo de hoy.

**Medido (D-S10):** en WORKSPACE, **escenificar un módulo deja siempre cambios sin aplicar**, porque el estado editable de la ventana lleva la autoridad de módulos (W:177: «el editor, la cola, la autoridad de modulos y los portadores»). La sonda de UI sobre el GREEN literal (`/d/r5f`, `pbt4c-probe-close-ui.cs`) lo vio con los dos gestos que escenifican: la cabecera con `HeaderConfiguratorDialog` y `StageModuleEdit` dan la pregunta combinada; el conmutador `AdvancedModulesToggle` o la sola elección del módulo no dejan nada pendiente. **Así que en WORKSPACE «solo un módulo escenificado» no se puede alcanzar:** la rama de `PendingWork()` que pregunta solo por el módulo queda para CLASSIC. La decisión del Owner (A-T2b-OWNER-01, F4B-E38) se cumple con la pregunta combinada.

**Lo que cambia en una prueba existente:** CT-PBT2B-06, ventana 4, fijaba que con `StageModuleEdit` mandaba la pregunta del módulo. Está en la lista de 4 (`owner-pbt4c-list-4-decision.md`), con su equivalente.

**La prueba** (CT-PBT4C-09) lee `PendingWork()` sin cerrar la ventana y cubre lo que se puede alcanzar (D-S10): `ambas`, `solo-sin-aplicar`, `ninguna` y `clasico-modulo`. **Escenifica la cabecera con `HeaderConfiguratorDialog` (W:1488) y `ConfigureModuleHeaderButton`** (D-S7): `ConfigureModuleHeader` llama a la costura en lugar de `ShowHeaderConfigurator` (W:1901-1903), así que no se construye el configurador. La v4.9 nombraba `HeaderConfiguratorPresenter` (W:2373), que recibe un `RackFrameConfiguratorWindow` real ya construido, y C05 lo prohíbe. W solo cambia `PendingWork()` y su constante; `W.xaml` no cambia.

