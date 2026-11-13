# I-64 — Paquete del contrato de F4-T3B (UI de edición del panel)

**Estado:** v3.
- La v1 pasó la revisión adversarial independiente que exige la orden del Owner: **EMITIBLE TRAS CORRECCIONES** (0 BLOQUEANTE, 10 IMPORTANTE y 11 MENOR). La v2 aplica todos sus hallazgos (§8).
- La re-revisión de la v2 dio **EMITIBLE TRAS CORRECCIONES** (0/2/6), todas de texto. La v3 las aplica antes de emitir (§9).

**Base:** rama `architecture/workspace-persistente-rackcad` en `eb55606c` (custodia del cierre de F4-T3A); `main` en `812baf87`. Los hechos del §2 se midieron en `eb55606c` con cuatro exploraciones de solo lectura del código real.

**Precondiciones de emisión de T3B1:**
- F4-T3A está CLOSED / PASS WITH LIMITATIONS sobre `18471988` (F4-E39).
- El Owner dividió F4-T3B en dos subtareas (`owner-f4t3b-decomposition-decision.md`).
- Las órdenes del Owner se custodian en el commit del paquete, que es la AuthorityRevision.

## 1. Autoridades

- **Órdenes del Owner** (`I-64-pilot/F4-DRAFTS/coordinator/`):
  - `owner-f4t3-authorization.md`: el alcance de T3, la cobertura mínima 1-18, el oráculo crítico (cero escrituras authored), sin Actualizar, solo `AnnotationScale`, y el proceso;
  - `owner-f4t3-decomposition-decision.md`: T3A, T3B y T3C;
  - `owner-f4t3a-close-decision.md`: los menores 1, 2, 3, 5 y 7 de T3A pasan a T3B; los menores 4 y 6, a T3C;
  - `owner-f4t3a-nc2-decision.md`: la regla de nc2 para T3B y T3C;
  - **`owner-f4t3b-decomposition-decision.md`**: T3B1-EDIT-SURFACE y T3B2-EDIT-CLOSE-SYNC, en orden B1, B2, C, cada una con 1 de 3 intentos.
- **Paquete de F4-T3** (`docs/initiatives/I-64-f4-t3-gate-contract-package.md`): la tabla K = 1 y su regla (§2), D-T3-02 a D-T3-08 (el puerto y su semántica), las reglas R-B1..R-B12 (§7), R-C1..R-C4 (§8) y las disposiciones (§9, §10).
- **Cierre de F4-T3A** (`I-64-pilot/F4-T3A-EDIT-PORT/task-close-package.md`, §5): los menores y los riesgos de Smoke-2.
- **Freeze** (`docs/initiatives/I-64-proposal-v5.md`): D-01 y D-02 (teclado y foco de la paleta; subdiálogos), D-08 (pestañas, preview, «la primera pulsación en un campo fija la preview», «Nombre *», cierre con Descartar o Cancelar, K = 1, Error), D-10 (componentes, dirty, intención, fronteras, O-01, O-16), D-17, P5 `:335-336`, INV-25, INV-26 e INV-27.
- **ADR-0032:**
  - D5: pendiente ≠ comprometido; mostrar no es editar;
  - D6: compromiso en dos fases, también en el gesto propio del campo.
- **Disposiciones** (`f4-dispositions.md`): F4-E09, F4-E21, F4-E29, F4-E31 y F4-E38..E39.

## 2. Hallazgos del repositorio (medidos en `eb55606c`)

### Lo que ya existe

| Id | Hallazgo | Consecuencia |
|---|---|---|
| H-01 | **El puerto** `IWorkspacePilotEditPort` (T3A): 12 operaciones, valores públicos; `PilotFieldView` con `Exists`, `State`, `IsPossiblyStale`, `IsProtected`, `CanEdit`, `CommittedText`, `PendingText`, `PendingMessage`. `CanEdit` no sabe nada de la preview ni de la marca de obsoleto (`WorkspaceHostCore.cs:873-874`). `WorkspaceController.PilotEdit` lo expone como un adaptador anidado sobre la compuerta de avisos. **No tiene llamador** en `src` | T3B1 lo conecta y decide la edición en la UI |
| H-02 | **Las fronteras no bloqueantes** del host devuelven `Kept` sin buscar el borrador (`WorkspaceHostCore.cs:681-687`): su momento no tiene efecto en el host. El borrador ya guarda el texto vivo (D-T3-08). **La bloqueante autoriza antes de mirar el borrador** (`:692-698`): con la presentación desfasada del documento activo, responde `Aborted` con el motivo en `Reason` e `InvalidFields` vacío | Cruzar las no bloqueantes es declarativo; la bloqueante necesita precondiciones (D-T3B1-06) |
| H-03 | **`TryBeginEdit` sobre un borrador existente** responde `AlreadyOpen` sin leer (`WorkspaceHostCore.cs:307-308`). **`TryReload` solo recarga un Limpio marcado** y responde `NotNeeded` en cualquier otro estado (`:464-467`); antes pasa por `PassGate` (`:459-462`), que con un comando activo responde `Waiting`. Los drenajes marcan los borradores como posiblemente obsoletos mientras la superficie está viva (`:173`) | El primer gesto es idempotente; «Recargar» del Error no puede ser `TryReload` (D-T3B1-08) |
| H-04 | **El VM del navegador** (`WorkspaceBrowserViewModel.cs`): constructor `(IWorkspaceBrowserSummaryPort port, Action tabChanged = null)` (`:64-68`); `Present` (`:124-138`) aplica la selección con `ApplySelectionContext` (`:135`) y descarta el `BrowserChange`; `Interact` (`:179-193`) ejecuta la operación **antes** de tocar la superficie (`:185`) y contiene los fallos con `ContainFailure` (`:306-321`); `SyncSurface` (`:211-225`) reinicia la superficie con `StartSurface` (`:228-237`) si cambia la sesión o el rack activo, y la vacía con `ClearSurface` (`:239-248`); `RebuildTabs` (`:326-344`) calcula el título y conserva la instancia de `Tabs` si no cambia nada (T2-F6) | Los puntos de enganche de T3B1 |
| H-05 | **La vista** (`WorkspacePanelView.xaml`, `:81-146`): una tira de pestañas (`ItemsControl` con `WrapPanel`) y la superficie, con cuatro `TextBlock` y un botón, «Volver a leer» (`:141-144`). El code-behind (`.xaml.cs:65-88`) envuelve cada clic en `Contained`, que lleva todo fallo a `_viewModel.ShowError` (el Error de **todo el panel**). Hoy solo hay `Click` de `Button`: sin `TextBox`, doble clic, teclado ni foco | El campo y sus gestos son nuevos; sus fallos se contienen en el campo (D-T3B1-08) |
| H-06 | **La paleta** (`WorkspacePaletteHost.cs:46`) construye `new WorkspacePanelViewModel(_controller.Port, _controller.SetInstrumentationEnabled, _controller.Navigation, _controller.BrowserSummary)`; el VM del panel (`WorkspacePanelViewModel.cs:62-69`) construye `new WorkspaceBrowserViewModel(browserSummary, RepresentNavigation)` | El puerto entra por los constructores |
| H-07 | **Ningún mecanismo diferido** en la UI ni en la paleta: solo el puente postea la continuación de T2C. Están fuera de un callback de AutoCAD los gestos WPF y el `StateChanged` que sale de `OnDrained`, indistinguible de los demás. La paleta nunca le dice al VM que el panel se ocultó: el único gancho es el `case Hide` de `OnPaletteStateChanged`, ya dentro del callback (`WorkspacePaletteHost.cs:108-110`). El veto de `BeginDocumentClose` no emite `StateChanged` | La intención solo puede salir de un gesto (R-B10); `PanelHide` no se observa en la UI |
| H-08 | **El texto de la intención:** `PushBackPilotIntentReader` lleva los diagnósticos «No editable aquí: …» y `ClassicEditorHint`, que contiene «editor clásico» (`PushBackPilotIntentReader.cs:56`). El XAML, **comentarios incluidos**, no puede contener «Abrir en editor» ni «editor clásico» (`WorkspaceBrowserHostGuardTests.cs:261`) | La indicación de RACKEDITAR llega solo por binding |
| H-09 | **El banco de pruebas de UI** (`tests/RackCad.UI.Tests/Workspace`, 248 casos en el filtro): dobles `internal sealed` de los puertos del host, del resumen y de la navegación; `StaTestRunner` para las vistas; **sin doble de `IWorkspacePilotEditPort`**. Las clases de prueba están en el namespace `RackCad.UI.Tests`, y el filtro `FullyQualifiedName~RackCad.UI.Tests.Workspace` las selecciona porque su nombre empieza por `Workspace`. Las pruebas de teclado de otras suites usan `KeyEventArgs` con `HwndSource` y `RaiseEvent` (`SelectiveCommitCoverageTests.cs:34-43`) | Las clases nuevas empiezan por `Workspace` |
| H-10 | **`PendingTextField`** (`UI/Controls`, clausura de la ventana) es `internal` y su `Show` sobre un campo dirty lanza en Debug; su modelo (Stage/Apply en el control) no es el del borrador | T3B1 no lo usa: el pendiente vive en el borrador |
| H-11 | **Un `catch` de T3A alcanzable:** `OpenDrafts` es visible para `RackCad.Tests`; `Begin` con un copiador que devuelve `null` deja `CurrentDraftState` en null (`DraftSession.cs:258-259`), y `ViewOfPilotDraft` hace NRE en `WorkspaceHostCore.cs:878`, fuera de sus `try` internos: el `catch` de `GetField` (`:603-607`) devuelve la vista Error | Menor 3(c) (D-T3B1-11) |

### Las guardas que fijan la UI (medidas)

| Id | Guarda | Tipo | ¿Bloquea T3B1? |
|---|---|---|---|
| G-01 | La construcción del VM del panel en la paleta, con 4 argumentos exactos: `WorkspacePaletteHostSourceGuardTests.cs:192`, `WorkspaceBrowserHostGuardTests.cs:213-215`, `WorkspaceNavigationHostTests.cs:947` | texto exacto | **sí** → cambio declarado (§5) |
| G-02 | La firma del VM del panel, de 4 parámetros: `WorkspaceBrowserHostGuardTests.cs:225-227`, `WorkspacePaletteHostSourceGuardTests.cs:539` | texto exacto | **sí** → cambio declarado (§5) |
| G-03 | La construcción del VM del navegador, `new WorkspaceBrowserViewModel(browserSummary, RepresentNavigation)`: `WorkspaceBrowserHostGuardTests.cs:229` | texto exacto | **sí** → cambio declarado (§5) |
| G-04 | La superficie contiene exactamente un `<Button` (desde `x:Name="BrowserSurface"` hasta el **primer** `</Border>`): `WorkspaceBrowserHostGuardTests.cs:303-311` | recuento exacto | **sí** → cambio declarado (§5) |
| G-05 | El record `WorkspaceBrowserTabItem(string RackId, string Title, bool IsActive, bool IsPreview)`: `WorkspaceBrowserHostGuardTests.cs:274`; `WorkspaceBrowserViewModelTests.cs:770-773` | texto y conjunto exactos | no: «Nombre *» va en `Title` |
| G-06 | Sin `+=`, `-=`, `event`, `delegate` ni estáticos en el VM del navegador: `WorkspaceBrowserHostGuardTests.cs:272`; sin eventos declarados: `WorkspaceBrowserViewModelTests.cs:961` | prohibición | no, si el VM del campo avisa al navegador con un `Action` del constructor |
| G-07 | Sin `Dispatcher`, `Timer`, `Task`, `async`, `Thread` en la UI: `WorkspacePaletteHostSourceGuardTests.cs:549`; en el VM del navegador, además `.Result`, `.Wait(`: `WorkspaceBrowserHostGuardTests.cs:269-271` | prohibición | no |
| G-08 | Sin `Keyboard`, `FocusManager`, `SetFocus`, `.Focus(` ni `InputManager` en los `.cs` de la UI: `WorkspaceNavigationHostTests.cs:919` (`LostKeyboardFocus` y `KeyboardFocusChangedEventArgs` caen; `LostFocus`, `PreviewTextInput`, `PreviewKeyDown`, `KeyEventArgs` y `TextCompositionEventArgs` no) | prohibición | no, si T3B1 no toca el foco |
| G-09 | Sin `Loaded`, `IsVisibleChanged`, `SizeChanged`, `CompositionTarget` en el code-behind: `WorkspacePaletteHostSourceGuardTests.cs:629`, `WorkspaceNavigationHostTests.cs:911` | prohibición | no |
| G-10 | Sin `<ContentControl`, `<TabControl`, `<TabItem` ni tamaños fijos en la sección del navegador: `WorkspaceBrowserHostGuardTests.cs:316-333`; sin tamaños en los elementos nombrados: `WorkspaceBrowserViewTests.cs:366-400` | prohibición | no |
| G-11 | Sin `Provider`, `Metric`, `\bBom`… sin distinguir mayúsculas, en la UI, **también en el XAML** (`WorkspacePaletteHostSourceGuardTests.cs:590-591`; cae `IFormatProvider`); `\.Commit\s*\(` prohibido (`WorkspaceNavigationHostTests.cs:920`; `CommitField(` no cae) | prohibición | no, con cuidado |
| G-12 | Un `namespace RackCad.UI.Workspace` exacto por archivo: `WorkspacePaletteHostSourceGuardTests.cs:602` | texto exacto | no |
| G-13 | `ShowModalWindow`, `ShowDialog` y `MessageBox` solo dentro de `using (…EnterPanelModal())`; en la UI siguen prohibidos (`WorkspacePilotEditGuardTests.cs:639-660`; `WorkspacePaletteHostSourceGuardTests.cs:514`) | prohibición | no: T3B1 no abre subdiálogos |
| G-14 | La paleta: miembros públicos `{GetOrCreate, IsCreated, Show}`, 2 `+=`, `using` exactos, sin `GetField`/reflexión (`WorkspacePaletteHostSourceGuardTests.cs:267-269`, `:291`; `WorkspaceNavigationHostTests.cs:948-950`) | varios | no: la paleta solo pasa `_controller.PilotEdit` |
| G-15 | `.ReadSummary(` y `.IsSessionAlive(` exactamente 1 en el VM del navegador y 0 en los demás archivos de la UI (`WorkspaceBrowserHostGuardTests.cs:245-246`); `BrowserSessionRegistry` una vez en toda la UI (`:250-251`); la lambda exacta de «Volver a leer» y un único `ReloadSummary()` (`:310-311`); el `Border` de la plantilla de pestaña sin atributos de borde, que siguen en los `Setter` del estilo (`:283-296`) | recuentos y textos exactos | no, si el VM del campo no lee el resumen y el `MouseDoubleClick` va en el `Button` de la plantilla, no en su `Border` |
| G-16 | **Las guardas de texto del puerto y de la intención**, comentarios incluidos: sin `Transaction`, `Database`, `Commit(`, `Provider`, `Metric`… en `IWorkspacePilotEditPort.cs` y `WorkspaceEditIntent.cs` (`WorkspacePilotEditGuardTests.cs:782-788`; `WorkspaceEditIntentGuardTests.cs:821-830`). Los anclajes de documentación de `WorkspaceEditIntent.cs`: el primer `        Waiting,` y el primer `        NotAdmitted,`, que son los de `EditIntentStatus` (`WorkspacePilotEditGuardTests.cs:854-868`), `EditDiscardOutcome` y `Discarded` (`:834-851`) y `EditIntentStatus.Rejected` (`WorkspaceLifecycleGuardTests.cs:757-772`). `EditCheckStatus` va después (`WorkspaceEditIntent.cs:195`) | prohibiciones y anclajes | no: la documentación nueva de `EditCheckStatus.Waiting` no mueve los anclajes |

## 3. Descomposición y reglas (decisión del Owner)

| Subtarea | Contenido |
|---|---|
| **F4-T3B1-EDIT-SURFACE** (§4, §5) | El cableado paleta → panel → navegador → VM del campo; el campo en la superficie K = 1; el primer gesto; la edición y el compromiso; la rematerialización; «Nombre *»; la frontera bloqueante `TabChange`; las no bloqueantes que la UI observa; los estados (Error, «No editable aquí», indicación de RACKEDITAR); y los menores 1, 2, 3, 5 y 7 de T3A. Cobertura 1-10 y 15-18 |
| **F4-T3B2-EDIT-CLOSE-SYNC** (§6) | Cerrar con Descartar o Cancelar en línea (R-B7), también el descarte del borrador en Error; descartar los borradores del documento con TOCTOU (R-B8); los avisos de veto y de pérdida en el panel (R-B9); la sincronización por selección que retiene la pestaña con pendiente (R-B6, `BrowserSession`). Cobertura 11-14 |

- **Presupuesto:** cada subtarea empieza en `attempts` 1 de 3 (F4-E09). El Owner aceptó que la división duplica la capacidad de T3B. No se describe como «6 intentos de T3B».
- **Orden:** B1 → B2 → C. F4-T4 no empieza antes de cerrar T3C.
- **Paquete de T3B2:** se fija tras cerrar T3B1, sobre el código real. Este paquete fija ya sus reglas vinculantes (§6).
- **Replanificación:** un rechazo por copia permite una replanificación reforzada única que no consume intentos; una segunda falla con STOP al Coordinator (F4-E31).
- **Cierre:** RED y GREEN, CI del SHA exacto, revisión directa, revisión independiente, verificación del Controller, nc1..nc3 y custodia. El 14/14 del Controller no basta.
- **Controles:** si se reproduce **exactamente** la firma de PD-F4-04 (nc1) o la de PD-F4-01 (nc2), se reutiliza el patrón del sustituto de la subtarea sin volver al Owner, se registra cada uso y se conserva la salida ordinaria.

## 4. Decisiones de F4-T3B1-EDIT-SURFACE

### D-T3B1-01. El cableado

- **Paleta:** `new WorkspacePanelViewModel(_controller.Port, _controller.SetInstrumentationEnabled, _controller.Navigation, _controller.BrowserSummary, _controller.PilotEdit)`. Sigue sin nombrar ningún tipo de puerto, sin `using` nuevo, con 2 `+=` y con `_controller.Navigation` una vez. Cambio declarado de G-01.
- **VM del panel:** quinto parámetro opcional `IWorkspacePilotEditPort pilotEdit = null`; construye `new WorkspaceBrowserViewModel(browserSummary, RepresentNavigation, pilotEdit)`. Cambios declarados de G-02 y G-03.
- **VM del navegador:** tercer parámetro opcional `IWorkspacePilotEditPort pilotEdit = null`, con un único constructor público (las pruebas llaman con 1 y con 2 argumentos).
- **Sin el puerto del piloto el navegador se comporta exactamente como hoy:** sin campo, sin «*», sin llamadas. Las 248 pruebas existentes no cambian.
- La UI nunca recibe `WorkspaceHostCore` ni el controlador (ya prohibido).

### D-T3B1-02. El VM del campo

Archivo nuevo `src/RackCad.UI/Workspace/WorkspacePilotFieldViewModel.cs`: `public sealed class WorkspacePilotFieldViewModel : ObservableObject`.
- Lo crea y lo posee el VM del navegador, que lo expone como `Browser.Field`, nunca nulo. Sin puerto, `IsAvailable` es false y la vista no muestra nada del campo.
- Recibe en su constructor el puerto y un `Action` del navegador que invoca tras cada operación que cambia el borrador, para que el navegador rehaga la tira («Nombre *») y fije la pestaña si hace falta. Es un método pasado como argumento: ni `+=` ni evento (G-06).
- **Editable** si y solo si:
  - el borrador existe y `CanEdit`;
  - la pestaña de la superficie **no es preview**;
  - y no es un Limpio posiblemente obsoleto (O-01).

  En cualquier otro caso la caja está en solo lectura.
- **Lo que muestra:**
  - `Text`: el texto de la caja, `PendingText ?? CommittedText`; sin borrador, el marcador «—» (D-T3-04);
  - el botón «Editar» cuando la caja está en solo lectura y un gesto puede abrirla (sin borrador, preview con borrador u obsoleto);
  - `StateText`: el estado en español (Limpio, Modificado, Conflicto, Aplicando, Desconocido, Error, Sin decidir) y «posiblemente obsoleto» cuando `IsPossiblyStale`;
  - `MessageText`: el mensaje de `Validate` del pendiente (`PendingMessage`);
  - `NoticeText`: el último aviso de un rechazo, un aborto, una espera, una intención no admitida o un fallo, en español y con su motivo;
  - `NotEditableText` y `ClassicEditorHintText`: con `NotAdmitted`, el diagnóstico «No editable aquí: …» y la indicación de RACKEDITAR, sin lanzarla (C-F4-07). Llegan por binding (H-08).
- **Entradas que lo cambian:** `Show(session, rack, isPreview)` y `Clear()`, que llama el navegador; las de los gestos (D-T3B1-03, D-T3B1-04).
- **El navegador vuelve a renderizar el campo al final de cada `Rebuild`** (`Present`, `Interact` y el `Action` del campo), **después** de `RebuildTabs`, con el `IsPreview` vigente: así una pestaña recién fijada (por `PinTab` o por la fijación automática) no sigue tratándose como preview. El renderizado lee `GetField`, que es memoria, y actualiza el estado, la editabilidad y el texto; por el invariante del texto vivo, el texto de la caja no cambia: si hay pendiente, coincide con él.
- **El renderizado conserva el aviso:** `NoticeText` y el diagnóstico de `NotAdmitted` de la misma sesión y rack se mantienen hasta un gesto nuevo o un cambio de superficie (un `NotAdmitted` no deja borrador y `GetField` responde `NotOpen`; el motivo de un `Waiting` vive solo en el VM).
- **Sin texto comprometido:** si `CommittedText` es null (D-T3-03), la caja muestra el texto vacío y el motivo (`CommittedTextReason`) aparece en el estado; la editabilidad sigue la regla de arriba.
- **Síncrono y sin estado estático** (G-07).

### D-T3B1-03. El primer gesto (R-B3, D-08, D-T3-04)

- **Gestos sobre la caja en solo lectura o sobre la pestaña activa:**
  - el botón «Editar» de la superficie;
  - un clic en la caja, en `PreviewMouseLeftButtonDown`, porque `TextBox` maneja `MouseLeftButtonDown`;
  - **una pulsación** en la caja en solo lectura (D-08: «la primera pulsación en un campo fija la preview»), en **`PreviewKeyDown`**: cualquier tecla que no sea un modificador, Tab ni Esc lanza el gesto y queda **`Handled`**. Al marcarla no se genera `TextInput`, así que la tecla **no se aplica** a la caja, que ya puede ser editable, ni sale del panel. Esc en solo lectura queda `Handled` sin gesto. Si además se usa `PreviewTextInput`, con la caja en solo lectura se marca siempre `Handled`. `e.KeyboardDevice` no se usa: contiene «Keyboard» y cae en G-08;
  - el doble clic en el botón de la pestaña **activa** (`MouseDoubleClick`): actúa solo si, tras los clics, la pestaña es la activa.
- **Orden:** el navegador **fija** primero la pestaña si es preview (`BrowserSession.Pin`) y después pide la intención: `TryBeginEdit(new EditIntentRequest(session, rack, null, null))`, sin testigo (`NotSupplied`).
- **Resultados de `TryBeginEdit`:**
  - `Admitted` o `AlreadyOpen`: el campo se renderiza desde `GetField`. Si queda editable, muestra «Listo para editar»; si no (Applying, Unknown, Error, Sin decidir), su estado. El gesto no se aplica;
  - `Waiting`, `Unavailable`, `Rejected` o `Failed`: el aviso con el motivo, sin cambios;
  - `NotAdmitted`: el estado «No editable aquí» con el diagnóstico y la indicación de RACKEDITAR.
- **Recarga perezosa (O-01):** un Limpio posiblemente obsoleto está en solo lectura, con «posiblemente obsoleto» (D-T3B1-02). El gesto llama a `TryReload`:
  - `Reloaded` o `NotNeeded`: renderiza de nuevo, y la caja pasa a editable si el estado lo permite;
  - `NotAdmitted`: el borrador se conserva tal como estaba (M-4 de T3A); el campo muestra el aviso con el diagnóstico y sigue en solo lectura;
  - lo demás: el aviso.
- **Reentrada (si `ReadIntent` bombea mensajes):** tras cada lectura, el gesto comprueba que la superficie sigue siendo la suya (la misma sesión y el mismo rack). Si no lo es, no renderiza el resultado: rematerializa la superficie actual desde `GetField` y avisa.
- **Una pestaña con borrador nunca es editable siendo preview:** el gesto la fija antes. Además, al reconstruir la tira, una pestaña preview con borrador **protegido** se fija (`Pin`, solo memoria).
- **El gesto lee a lo sumo una vez por pestaña y estado:** un segundo gesto sobre un borrador abierto da `AlreadyOpen` sin leer.
- **Fijar** sigue fijando y nada más.

### D-T3B1-04. Editar y comprometer (R-B4, D-T3-08)

**Mecánica WPF fijada:**
- la caja es un `TextBox` con `Text="{Binding Browser.Field.Text, Mode=TwoWay, UpdateSourceTrigger=PropertyChanged}"`;
- el **setter público** de `Text` es la única entrada de edición del usuario;
- la presentación cambia un campo privado y llama a `OnPropertyChanged`;
- **prohibido:** capturar el texto en `TextChanged` y asignar `TextBox.Text` desde el código.

**Semántica:**
- **Cada cambio de texto del usuario** llama a `SetPendingText(origin, rack, text)`.
  - `Applied`: el campo y la tira se actualizan;
  - `Rejected`, `NotEditable` o `Busy`: el setter **restaura** el valor desde `GetField` (`PendingText ?? CommittedText`) y notifica dentro de la actualización (el binding relee el origen), y hay aviso.
- **El setter contiene sus propios fallos:** una excepción dentro del setter la captura el motor de binding de WPF y no llega al `Contained` de la vista, así que el setter la convierte en el Error del campo y restaura la caja. **Con el campo no editable, el setter no hace nada y restaura** (la caja en solo lectura ya lo impide en la vista; la prueba 10 en el VM depende de ello).
- **Mostrar nunca llama a `SetPendingText`** (D-T2-01, O-16).
- **Enter o salir de la caja** (`LostFocus`) llama a `CommitField`, **solo con pendiente (`PendingText != null`) y con la caja editable** (un Unknown o Applying con pendiente daría `NotEditable` y un aviso espurio).
  - `Applied`: renderiza;
  - `Invalid`: el texto sigue en la caja, con su mensaje, sin reparar nada;
  - lo demás: aviso.
- **Teclado (D-01):**
  - Enter y Esc se marcan `e.Handled = true` en el `PreviewKeyDown` de la caja editable;
  - con la caja **editable**, un `KeyDown` de burbuja marca `Handled` toda tecla que el editor no haya consumido, salvo Tab (que se deja a la navegación), para que Supr, Ctrl+Z, Ctrl+V y demás no salgan hacia AutoCAD (ERASE, UNDO o PASTECLIP serían un cambio del DWG nacido de una tecla del panel);
  - con la caja en **solo lectura**, la regla de la pulsación de D-T3B1-03;
  - las dos reglas se combinan sin conflicto: el `KeyDown` solo se marca con la caja editable, así que no anula el gesto del `PreviewKeyDown`;
  - la lista completa de teclas de D-01 en el host real es de Smoke-2.
- Esc no revierte (H-03 de T3): la reversión es el descarte explícito de T3B2.
- **`LostFocus` es de foco lógico:** un clic en el dibujo **no** compromete; «salir de la caja» ocurre dentro del panel. El pendiente queda protegido y visible.
- **Invariante del texto vivo:** la caja nunca contiene texto del usuario que el borrador no tenga.

### D-T3B1-05. Rematerializar (R-B5)

Cuando una pestaña pasa a ser la superficie (`StartSurface`: cambio de pestaña, selección o documento), el navegador llama a `Field.Show(session, rack, isPreview)`: una lectura de memoria (`GetField`) que muestra el pendiente exacto, o el comprometido. Sin borrador, el marcador «—».

### D-T3B1-06. Las transiciones de la tabla K = 1

| Transición | En T3B1 |
|---|---|
| Activar otra pestaña en el panel | **Bloqueante**, antes de `session.Activate`, solo si el destino existe, es **distinto** de la activa y la pestaña viva tiene **pendiente** (`PendingText != null`): `CrossBoundary(origin, viva, TabChange)`. `Proceeded` o `Kept` → se activa; `Aborted` o `Busy` → no se activa, el texto sigue en su caja y hay aviso con los `InvalidFields` o, si están vacíos, con `Reason`. Sin pendiente, sin llamada |
| Cambiar de rack por una acción del panel | Ninguna acción del panel lo hace hoy: sin llamador en T3B1 (desviación registrada de la tabla de T3) |
| Fijar | Ninguna frontera |
| Selección Uno(R) que cambia la pestaña activa | **No bloqueante**, solo en la ruta de `Present`: tras el cambio, si la pestaña anterior tenía borrador, `CrossBoundary(origen anterior, rack anterior, Dematerialize)`. **Interino:** T3B1 no retiene la pestaña con pendiente; eso es R-B6 (T3B2), que lo cambia. Ninguna prueba de T3B1 fija la selección con pendiente |
| Cambio de documento | **No bloqueante:** `CrossBoundary(origen anterior, rack anterior, DocumentChange)` cuando cambia la sesión presentada |
| Ocultar el panel | **No se observa** en la UI (H-07). No hace falta capturar nada (D-T3-08) y la frontera no tiene efecto en el host (H-02). Desviación registrada de la tabla de T3 |
| Cerrar el documento o retirar la sesión | Lo hace el host (T2B2B); los mensajes son de T3C. Desviación registrada de la tabla de T3 |
| Cerrar una pestaña | **Sin cambio en T3B1** (R-B7 es de T3B2). Residual declarado: cerrar una pestaña deja su borrador en el host; si está protegido veta el cierre del documento y su pendiente vuelve cuando el rack se abre de nuevo, en solo lectura hasta el primer gesto si es preview |

La sincronización por selección **nunca** cruza una frontera bloqueante. `Dematerialize` no se cruza después de `TabChange` ni de `CloseTab`.

### D-T3B1-07. «Nombre *» (R-B2)

`RebuildTabs` añade « *» al título de cada pestaña cuyo `GetField(origin, rack).IsProtected` es true (memoria, sin `Authorize`). El record no cambia (G-05). La protección cambia la instancia de `Tabs` solo cuando cambia (T2-F6). Sin puerto, igual que hoy.

### D-T3B1-08. Los estados y la contención (R-B1, D-08, D-17)

- `Clean`, `Dirty` y `Conflict` con `CanEdit`, fijada y no obsoleta: la caja editable.
- `Applying`, `Unknown` y `Undecidable`: en solo lectura, con su texto.
- `Error` (`PilotFieldState.Error` o un fallo contenido del campo): en solo lectura, con el motivo y el botón «Recargar», que **rematerializa desde el borrador** (`Field.Show` desde `GetField`, memoria, sin `TryReload`; paquete de T3, I-5; D-08). El descarte del borrador en Error es de T3B2, con R-B7.
- `NotAdmitted`: «No editable aquí» y la indicación de RACKEDITAR.
- **Contención (D-17, INV-25):** los gestos del campo contienen sus fallos en el estado Error del campo, como `Interact` → `ContainFailure`; nunca llevan el panel entero a Error. El `Contained` de la vista queda como última defensa.

### D-T3B1-09. Solo un gesto lee (R-B10, D-T3-06)

- `TryBeginEdit` y `TryReload` se llaman solo desde los métodos de gesto del navegador y del campo; `TryCheckNow` no se usa en T3B1 (el flujo de obsoleto o conflicto es de T4).
- `Present`, `Refresh`, `Show`, `Clear`, `RebuildTabs`, `SyncSurface`, `StartSurface`, `ClearSurface`, `Materialize`, los constructores y los setters nunca nombran las tres lecturas.
- **La vía del constructor de la paleta** (`OnControllerStateChanged()` directo, fuera de la compuerta de avisos): sin cambio. Es inocua porque ninguna presentación lee.

### D-T3B1-10. Foco, modal y oráculo

- **Foco (R-B11):** T3B1 no llama a ninguna API de foco (G-08 sin cambio). La pérdida de foco al activar o fijar una pestaña queda como residual de F7.
- **El clic de pestaña tras un compromiso:** el botón de la pestaña toma el foco en el `MouseDown`, así que `LostFocus` → `CommitField` ocurre antes del `Click`. Si ese compromiso cambia «*» (un valor igual al inicial vuelve a Limpio), la tira se regenera entre el `MouseDown` y el `MouseUp` y el clic puede perderse. Es raro y no pierde texto: limitación declarada y riesgo de Smoke-2.
- **Modal:** T3B1 no abre subdiálogos; `EnterPanelModal` no se usa (G-13 sin cambio).
- **Oráculo crítico:** la UI solo llama a operaciones del puerto; ninguna escribe (T3A). Cambiar de pestaña no llama a la navegación ni escribe la selección, la vista ni el DWG.

### D-T3B1-11. Arrastres de F4-T3A (orden del Owner)

| Arrastre | Disposición en T3B1 |
|---|---|
| Menor 1 | `PilotEditAuthoredWriterSpy` y sus aserciones vacías se retiran de `WorkspacePilotEditHostTests.cs`. La prueba del oráculo afirma lo que sí discrimina (`Hr.Reader.ReadCalls == 0`, `ModalHistory` vacío, sin fallos) y su comentario cita la guarda por cuerpo `TheOperationsOfThePortAndTheDecoratorNameNoApiOfWritingTheDrawing` y la ausencia de dependencia de escritura de Application |
| Menor 2 | `ThisTaskAddsNoFile...` (`WorkspacePilotEditGuardTests.cs:871-881`) deja de fijar recuentos exactos: nombra como cota los 5 archivos de `Drafts/` y los 9 de `Plugin/Workspace` y quita los 16 duplicados de `+=`, `-=` y manejadores, que ya fijan otras guardas |
| Menor 3 | Pruebas de host nuevas en `WorkspacePilotEditHostTests.cs`:<br>- (a) la frontera bloqueante con un borrador no editable (Error, Unknown o Applying) y pendiente → `Kept`, con la transición permitida y el pendiente intacto;<br>- (b) `GetField`, `GetRacksAtRisk`, una no bloqueante, `CommitField` sin pendiente y un `SetPendingText` rechazado o `Busy` conservan la marca «limpio conocido» (vía `known-safe`);<br>- (c) el `catch` externo de `GetField` (vista `Error`), alcanzado con `OpenDrafts` y un copiador que devuelve null (H-11); el de `GetRacksAtRisk` es inalcanzable hoy y va a T3C con el menor 4.<br>- (d) se quita la aserción entre literales de `WorkspaceIdleSchedulingGuardTests.cs:719` |
| Menor 5 | Documentación: `PilotFieldState.Undecidable` se da solo cuando el estado ordinario Limpio/Modificado no se puede decidir; con un estado explícito (Conflict, Applying, Unknown, Error) se conserva ese estado y `IsProtected` es true. Sin cambio de producto |
| Menor 7 | Documentación del puerto: `PilotBoundaryDecision.Kept` en una bloqueante significa que la transición procede con el pendiente conservado; **ningún productor ni doble devuelve un `PilotBoundaryResult` `default`** (vale `TabChange`/`Proceeded` y es indistinguible de uno real); `EditCheckStatus.Waiting` nombra los dos motivos nuevos (lectura en curso y aviso del panel), sin mover los anclajes de G-16 |
| N-5 | La cabecera de `WorkspaceEditIntentGuardTests.cs:16-17` y su comentario `:611-612` dicen que la exposición a la UI existe desde F4-T3A/T3B1 (el puerto del piloto), sin cambiar ninguna aserción |

### D-T3B1-12. Pruebas de T3B1

**Archivos nuevos:**
- `tests/RackCad.UI.Tests/Workspace/WorkspacePilotEditPortDouble.cs`: un doble `internal sealed` del puerto, con registro de llamadas por operación (con el origen, el rack, el texto y el `kind`), `GetField` programable por rack, respuestas programables de `SetPendingText`, `CommitField`, `CrossBoundary` y de las tres lecturas (siempre explícitas, nunca `default`), y un gancho que ejecuta una acción dentro de una lectura;
- `tests/RackCad.UI.Tests/Workspace/WorkspacePilotFieldViewModelTests.cs`: el VM del campo y su integración con el navegador;
- `tests/RackCad.UI.Tests/Workspace/WorkspacePilotFieldViewTests.cs`: la vista (STA) con un `TextBox` real, sus gestos y los botones;
- `tests/RackCad.Tests/Workspace/WorkspacePilotSurfaceGuardTests.cs`: las guardas de T3B1, con autopruebas.

**Cobertura del Owner a nivel de UI** (11-14 son de T3B2). Las pruebas 5 y 6 usan caminos que R-B6 no cambia:

| N.º | Prueba |
|---|---|
| 1 | Texto válido + Enter → `SetPendingText` y `CommitField` con el texto exacto; el campo muestra el comprometido sin pendiente |
| 2 | Texto inválido → sigue en la caja con su mensaje tras `CommitField` (`Invalid`) |
| 3 | Texto inválido → la pestaña muestra «Nombre *» |
| 4 | Activar otra pestaña con texto inválido → `CrossBoundary(TabChange)` `Aborted`: la pestaña activa no cambia, el texto sigue y hay aviso |
| 5 | Cambio de documento con texto inválido → procede, con `DocumentChange` sobre la pestaña de A y sin validar; y activar otra pestaña desde un borrador no editable (Error, Unknown o Applying) con pendiente → `Kept`, la transición procede |
| 6 | Volver a la pestaña o al documento → la caja muestra el pendiente exacto (`GetField`) |
| 7 | Cambio de documento → el pendiente de A sigue en A |
| 8 | Volver al documento A → la caja muestra el pendiente de A |
| 9 | El primer gesto sobre una preview la fija **antes** de pedir la intención, con «Editar», el clic, la pulsación y el doble clic. Como `Pin` no es una llamada del puerto, el gancho del doble comprueba dentro de `TryBeginEdit` que la pestaña de `Browser.Tabs` ya no es preview |
| 10 | Una pestaña con borrador nunca es editable siendo preview: cerrar la pestaña de un borrador limpio, volver a seleccionar el rack y teclear → la tecla no llega a `SetPendingText`; el gesto fija y responde `AlreadyOpen`. Una preview con borrador protegido se fija al reconstruir la tira |
| 15 | Cambiar de pestaña: cero llamadas a la navegación (`ExecuteAsync`); las operaciones del puerto llamadas en los escenarios de T3B1 son solo `GetField`, `SetPendingText`, `CommitField`, `CrossBoundary`, `TryBeginEdit` y `TryReload`: ninguna llamada a `DiscardRack`, `DiscardDocument`, `GetRacksAtRisk`, `TakeLossNotices`, `TakeCloseNotices` ni `TryCheckNow` |
| 16 | K = 1 mientras se edita: una superficie y un campo; el campo sigue a la pestaña activa |
| 17 | Aislamiento: cada llamada lleva el origen de su sesión y su rack; las de A nunca llevan B |
| 18 | Sin subdiálogo en T3B1; el ámbito del modal sigue probado en el host (T3A) |

**Además:**
- `Admitted`, `AlreadyOpen` (también sobre Applying o Unknown), `Waiting`, `Rejected`, `NotAdmitted` (con diagnóstico e indicación) y `Failed` del primer gesto;
- la recarga perezosa: la marca de obsoleto llega con la superficie viva (por una presentación), la caja queda en solo lectura, la tecla no llama a `SetPendingText`, y el gesto llama a `TryReload`; su `NotAdmitted` conserva el borrador;
- la reentrada: con el gancho del doble, un cambio de pestaña y otro de documento dentro de `TryBeginEdit` → el resultado no se renderiza sobre la superficie nueva;
- `TabChange`: un nuevo clic en la activa no llama a nada; un cambio desde un borrador limpio no llama a `CrossBoundary`; un `Aborted` por `Authorize` muestra su `Reason`;
- `Dematerialize`, por un camino que R-B6 no cambia: una selección con la pestaña activa **solo dirty** (sin pendiente) → R se activa y la anterior cruza `Dematerialize`;
- el renderizado conserva el aviso: `NotAdmitted` → `Present` → el diagnóstico y la indicación siguen; y una pestaña fijada por `PinTab` o por la fijación automática deja de tratarse como preview en el mismo `Rebuild`;
- `CommittedText` null con su motivo: la caja vacía y el motivo en el estado;
- el setter: con el campo no editable no llama a `SetPendingText` y restaura; una excepción dentro del setter queda en el Error del campo con la caja restaurada;
- `CommitField` no se llama con un pendiente sobre un borrador no editable;
- el rechazo de `SetPendingText` (`Rejected`, `NotEditable`, `Busy`) restaura la caja y avisa;
- `CommitField` sin pendiente no se llama (el `LostFocus` tras un gesto en `Waiting` no pisa el aviso);
- Esc no cambia nada; Enter y Esc quedan `Handled`;
- un doble clic sobre una pestaña no activa cuya activación se aborta no fija ni lanza la intención;
- los estados de D-T3B1-08: Error desde `GetField` y Error por una excepción del VM, con «Recargar» que no llama a ninguna lectura; el panel no pasa a Error;
- `Present` y `Refresh` nunca llaman a las tres lecturas;
- sin puerto del piloto, el navegador es el de antes (las 248 pruebas no cambian).

**En la vista (STA, `TextBox` real):**
- (a) `box.Text = "x"` sin perder el foco → `SetPendingText("x")` exacto;
- (b) `Show` y `Present` con un pendiente → cero `SetPendingText`;
- (c) un rechazo → `box.Text` restaurado;
- (d) los controles nuevos sin tamaños fijos;
- (e) en solo lectura, `Keyboard.PreviewKeyDownEvent` con `D5` → el gesto, `Handled` y `box.Text` sin cambio (las pruebas lanzan el evento de túnel: `RaiseEvent` no encadena el túnel con la burbuja, y el patrón de H-09 lanza `KeyDownEvent`; el código de prueba no está sujeto a G-08). Si se usa `PreviewTextInput`, la prueba lanza también el `TextInput` cuando no queda marcado, como el sistema de entrada;
- (f) con la caja editable, `KeyDown` de `Delete` y de `Z` sin consumir → `Handled`; Tab no.

**Guardas nuevas** (regla I-1: limitadas a T3B1, en negativo y por cuerpo, sin conjuntos exactos de archivo entero), con autopruebas:
- las tres lecturas del puerto no se nombran en `Present`, `Refresh`, `Show`, `Clear`, `RebuildTabs`, `SyncSurface`, `StartSurface`, `ClearSurface`, `Materialize`, los constructores ni los setters;
- `SetPendingText` solo se nombra en el setter de `Text` del VM del campo;
- ni el code-behind ni el XAML capturan `TextChanged` (`TextChanged\s*=` en el XAML, porque un handler con `RoutedEventArgs` vale para `TextChangedEventHandler` por contravarianza), `SourceUpdated` ni `TargetUpdated`, y el code-behind no asigna `\.Text\s*=(?!=)`;
- la paleta pasa `_controller.PilotEdit` y nada más nuevo;
- la UI no nombra ninguna API de escritura ni de foco.

## 5. Contrato propuesto de F4-T3B1-EDIT-SURFACE

**AllowedWriteScope** (19 archivos exactos):

| Archivo | Papel |
|---|---|
| `src/RackCad.UI/Workspace/WorkspacePilotFieldViewModel.cs` | nuevo |
| `src/RackCad.UI/Workspace/WorkspaceBrowserViewModel.cs` | |
| `src/RackCad.UI/Workspace/WorkspacePanelViewModel.cs` | |
| `src/RackCad.UI/Workspace/WorkspacePanelView.xaml` | |
| `src/RackCad.UI/Workspace/WorkspacePanelView.xaml.cs` | |
| `src/RackCad.Plugin/Workspace/WorkspacePaletteHost.cs` | solo el argumento |
| `src/RackCad.Application/Workspace/IWorkspacePilotEditPort.cs` | solo documentación (menores 5 y 7) |
| `src/RackCad.Application/Workspace/WorkspaceEditIntent.cs` | solo documentación (menor 7) |
| `tests/RackCad.Tests/Workspace/WorkspacePilotSurfaceGuardTests.cs` | nuevo |
| `tests/RackCad.Tests/Workspace/WorkspaceBrowserHostGuardTests.cs` | G-01, G-02, G-03, G-04 |
| `tests/RackCad.Tests/Workspace/WorkspacePaletteHostSourceGuardTests.cs` | G-01, G-02 |
| `tests/RackCad.Tests/Workspace/WorkspaceNavigationHostTests.cs` | G-01 |
| `tests/RackCad.Tests/Workspace/WorkspacePilotEditGuardTests.cs` | menor 2 |
| `tests/RackCad.Tests/Workspace/WorkspacePilotEditHostTests.cs` | menores 1 y 3 |
| `tests/RackCad.Tests/Workspace/WorkspaceIdleSchedulingGuardTests.cs` | menor 3(d) |
| `tests/RackCad.Tests/Workspace/WorkspaceEditIntentGuardTests.cs` | N-5 (comentarios) |
| `tests/RackCad.UI.Tests/Workspace/WorkspacePilotEditPortDouble.cs` | nuevo |
| `tests/RackCad.UI.Tests/Workspace/WorkspacePilotFieldViewModelTests.cs` | nuevo |
| `tests/RackCad.UI.Tests/Workspace/WorkspacePilotFieldViewTests.cs` | nuevo |

**Cambios declarados de guardas (texto exacto nuevo):**
- **G-01** (las tres guardas): `new\s+WorkspacePanelViewModel\s*\(\s*_controller\.Port\s*,\s*_controller\.SetInstrumentationEnabled\s*,\s*_controller\.Navigation\s*,\s*_controller\.BrowserSummary\s*,\s*_controller\.PilotEdit\s*\)`.
- **G-02:** en `WorkspaceBrowserHostGuardTests.cs`, `public\s+WorkspacePanelViewModel\s*\(\s*IWorkspaceHostPort\s+\w+\s*,\s*Action<bool>\s+\w+\s*,\s*IWorkspaceNavigationPort\s+\w+\s*=\s*null\s*,\s*IWorkspaceBrowserSummaryPort\s+browserSummary\s*=\s*null\s*,\s*IWorkspacePilotEditPort\s+pilotEdit\s*=\s*null\s*\)`; en `WorkspacePaletteHostSourceGuardTests.cs`, la misma con `\w+` en los dos últimos nombres.
- **G-03:** `new\s+WorkspaceBrowserViewModel\s*\(\s*browserSummary\s*,\s*RepresentNavigation\s*,\s*pilotEdit\s*\)`, una vez.
- **G-04:** el tramo es el elemento `BrowserSurface` **completo**, con equilibrio de aperturas `<Border(?=[\s>])` que no se cierran en sí mismas (`/>`) y de cierres `</Border>` (`<Border.Style>` no cuenta), con una autoprueba de cada caso; la guarda pasa a una **cota** que nombra los botones de T3B1 («Volver a leer», «Editar», «Recargar»), cada uno una vez y con su handler dentro de `Contained`; se conservan la lambda exacta de «Volver a leer» y el único `ReloadSummary()` (`:310-311`). T3B2 añade sus botones sin cambiar la guarda.
- Los comentarios de G-01 a G-04 nombran F4-T3B1.

**ForbiddenWriteScope:** prefijos seguros:
- el resto de `Application`, incluidos `Drafts/`, `WorkspaceHostCore.cs`, `WorkspacePilotSupport.cs` y `BrowserSession.cs` (R-B6 es de T3B2);
- `Domain`;
- el resto de la UI, con la clausura (`UI/Controls/**`, `Shell/**`, `UI/Systems/PushBack/**`, `Themes/**`);
- el resto del Plugin, incluidos el controlador, el puente y `WorkspacePanelCommand.cs`;
- las demás pruebas y guardas, también las de `tests/RackCad.UI.Tests` fuera de los 3 archivos nuevos;
- la raíz.

**Invariantes:**
- **INV-F4T3B1-01** a **INV-F4T3B1-11:** uno por cada decisión, de D-T3B1-01 a D-T3B1-11;
- **INV-F4T3B1-12:** los cambios declarados G-01 a G-04 con su texto nuevo y los de D-T3B1-11, y ningún otro;
- **INV-F4T3B1-13:** la cobertura RED/GREEN:
  - el esqueleto del RED lleva ya el cableado completo (paleta, panel y navegador), las firmas, el VM del campo inerte, el XAML con todos sus controles y handlers enlazados a ese VM inerte, y todo lo que nombran las guardas de Core;
  - **las pruebas de Core deben pasar en el RED** (ExpectRed false);
  - las pruebas de comportamiento de UI fallan por aserción;
  - el esqueleto es inerte y no lanza;
- **INV-F4T3B1-14:** los límites: sin cambio de producto en Application ni en el Plugin más allá de la documentación y del argumento de la paleta; sin `+=`, evento, `Dispatcher`, `Timer` ni `Task` nuevos; sin foco; sin subdiálogo; sin comando; sin escritura.

**RequiredTests:**
- `FullyQualifiedName~RackCad.Tests.Workspace`, MinSelected **2251** (CI `43355611879` de `18471988`; el mismo producto que `eb55606c`), ExpectRed false;
- `FullyQualifiedName~RackCad.UI.Tests.Workspace`, MinSelected 248, ExpectRed true.

**StopConditions:** las S y P comunes, más:
- **C01:** un archivo fuera del alcance, o uno nuevo en `Plugin/Workspace` o en `Drafts/`;
- **C02:** un cambio en guardas o pruebas existentes fuera de los declarados;
- **C03:** una escritura authored, una lectura del DWG fuera de las tres operaciones de la intención, o una de ellas llamada fuera de un gesto;
- **C04:** un cambio de producto en el host, el controlador, el puente, `BrowserSession`, la clausura o un comando;
- **C05:** una suscripción, un `+=`, un evento, un `Dispatcher`, un `Timer` o un `Task` nuevos;
- **C06:** una semántica congelada que cambie, o el comportamiento cerrado de F3, F4-T1, F4-T2 o F4-T3A;
- **C07:** la tarea se evalúa como Long-horizon;
- **C08:** la regla de replanificación.

## 6. F4-T3B2-EDIT-CLOSE-SYNC: reglas vinculantes (su paquete se fija tras cerrar T3B1)

| Regla | Contenido |
|---|---|
| R-B2-1 (R-B7) | **Cerrar una pestaña.** Limpia: se cierra sin preguntar y llama a `DiscardRack`, sin efecto en el DWG. Protegida: Descartar o Cancelar **en línea**, sin subdiálogo; si alguna vez hay subdiálogo, dentro de `using (EnterPanelModal())`. Descartar llama a `DiscardRack`; Cancelar conserva todo. Cerrar nunca escribe. El borrador en Error se descarta por la misma vía. Sus botones caben en la cota de G-04 que deja T3B1 |
| R-B2-2 (R-B8) | **«Descartar borradores de este documento»:** lista los racks (nombres de pestaña, o el RackId). Tras confirmar, vuelve a leer `GetRacksAtRisk`; si el conjunto cambió, no descarta y avisa (TOCTOU). Sin racks en riesgo: un mensaje y ningún efecto. `DiscardDocument` |
| R-B2-3 (R-B9) | **Avisos en el panel:** `TakeCloseNotices` (veto, con las dos vías de descarte) y `TakeLossNotices` (pérdida). **Relectura de «fuera de todo callback», decidida ya:** se consumen desde la presentación del panel o desde un gesto, nunca desde el manejador de `BeginDocumentClose`. Consumir es memoria y el aviso es un texto no modal. El veto no emite `StateChanged`: el panel lo ve en el siguiente aviso, normalmente el fin del comando. T3B2 lo fija con una guarda |
| R-B2-4 (R-B6) | **Sincronización por selección con texto pendiente en la pestaña activa:** la pestaña sigue activa y la preview de R va en segundo plano, con aviso (`P5:335-336`). En modo retenido **nunca se reemplaza la pestaña activa**. Con la pestaña solo dirty, R se activa y la anterior pasa por `Dematerialize`. El cambio mínimo de `BrowserSession` es un parámetro opcional `bool keepActive = false` de `ApplySelectionContext`, sin valores nuevos de `BrowserChange` y con el contexto consumido también en modo retenido; su vocabulario no usa `Draft` ni `Dirty`. No cambia ninguna prueba de T3B1, que no fija la selección con pendiente |
| R-B2-5 | **Consumo anterior al endurecimiento de T3C:** T3B2 es el primer consumidor de `GetRacksAtRisk` y de las colas, antes de que T3C las endurezca (menor 4). Su VM contiene esas llamadas, y un fallo de las colas se muestra como aviso. **El fallo de `GetRacksAtRisk` no llega al VM:** el host lo devuelve como `default`, indistinguible de «nada en riesgo» (`WorkspaceHostCore.cs:802-806`). Hoy es inalcanzable y su dirección es segura para la comprobación TOCTOU (el conjunto «cambió», así que no se descarta); el mensaje «sin racks en riesgo» puede ser falso, y queda como limitación declarada de T3B2 hasta el endurecimiento de T3C. Opcionalmente, T3B2 contrasta con `GetField(...).IsProtected` de las pestañas conocidas y, si discrepan, avisa «no se pudo decidir» |
| R-B2-6 | Cobertura 11-14 a nivel de UI, más la vía TOCTOU, los avisos y la retención |

## 7. Puntos para la re-revisión de la v2

1. ¿Cierran las correcciones I-1 a I-10 sin defectos nuevos? En particular: la regla de edición (D-T3B1-02), O-01 y la reentrada (D-T3B1-03), la mecánica WPF (D-T3B1-04), las precondiciones de `TabChange` (D-T3B1-06), la contención (D-T3B1-08), la regla RED (INV-F4T3B1-13) y la forma nueva de G-04 (§5).
2. ¿Son implementables, discriminantes y de I-1 las pruebas y las guardas nuevas, incluidas las de la vista con un `TextBox` real?
3. ¿Queda alguna guarda no declarada que rompa con la v2 (por ejemplo, con `PreviewTextInput`, `PreviewKeyDown`, `MouseDoubleClick` o `e.Handled` en el code-behind)?
4. ¿Sigue T3B1 dentro de un horizonte razonable?

## 8. Disposición de la revisión adversarial de la v1

| Hallazgo | Disposición en la v2 |
|---|---|
| I-1 (preview editable sin fijar) | **Aceptado:** editable solo si no es preview (D-T3B1-02); el gesto fija y responde `AlreadyOpen`; prueba 10; R-B2-4 nunca reemplaza la activa |
| I-2 (O-01 sin cumplir; el campo no se refresca) | **Aceptado:** un Limpio obsoleto en solo lectura y la recarga por gesto (D-T3B1-03); cada presentación renderiza desde `GetField` (D-T3B1-02); pruebas |
| I-3 («Recargar» con `TryReload`; Error del panel) | **Aceptado:** «Recargar» rematerializa desde el borrador y los fallos se contienen en el campo (D-T3B1-08) |
| I-4 (mecánica WPF) | **Aceptado:** binding fijado, setter como única entrada, prohibiciones y pruebas de vista (D-T3B1-04, D-T3B1-12) |
| I-5 (precondiciones de `TabChange`) | **Aceptado:** destino distinto y existente con pendiente; aviso con `InvalidFields` o `Reason` (D-T3B1-06) |
| I-6 (reentrada) | **Aceptado:** la comprobación tras cada lectura y su prueba (D-T3B1-03) |
| I-7 (regla RED) | **Aceptado:** el esqueleto lleva el cableado y la vista; Core debe pasar en el RED (INV-F4T3B1-13) |
| I-8 (forma de G-04; G-01..G-03) | **Aceptado:** textos exactos nuevos y la cota de G-04 sobre el elemento completo (§5) |
| I-9 (pruebas 5 y 6 interinas) | **Aceptado, opción (a):** 5 y 6 sobre caminos que R-B6 no cambia; la selección con pendiente queda interina y sin prueba en T3B1 |
| I-10 (pulsación como primer gesto) | **Aceptado:** `PreviewTextInput` sobre la caja en solo lectura, sin aplicar la tecla (D-T3B1-03) |
| M-1 (trampas) | **Aceptado:** G-15 y G-16 en el §2; H-08 con los comentarios del XAML |
| M-2 (guardas nuevas e I-1) | **Aceptado:** en negativo y por cuerpo; la del modal se retira (la cubre G-13) |
| M-3 (prueba 15 nominal) | **Aceptado:** cero navegación y el conjunto de operaciones usadas (prueba 15) |
| M-4 (efectos colaterales) | **Aceptado:** `CommitField` solo con pendiente; Enter y Esc `Handled`; `LostFocus` lógico; `PreviewMouseLeftButtonDown`; el clic perdido como limitación (D-T3B1-04, D-T3B1-10) |
| M-5 (el `default` no se comprueba al consumir) | **Aceptado:** obligación del productor (menor 7) |
| M-6 (el `catch` de `GetField` es alcanzable) | **Aceptado:** H-11 y el menor 3(c) |
| M-7 (T3B2 consume antes que T3C endurezca) | **Aceptado:** R-B2-5. La premisa de la orden de cierre de T3A («T3C lo consume») no se sostiene con el orden B1, B2, C: se informa al Owner |
| M-8 (relectura de R-B9) | **Aceptado:** decidida ya en R-B2-3 |
| M-9 (precisiones de R-B2-4) | **Aceptado:** R-B2-4 |
| M-10 (Long-horizon) | **Aceptado:** la mecánica fijada reduce la exploración; si la planificación da High, STOP C07 y vuelta al Owner con el bloque separable (los arrastres de Core) |
| M-11 (redacción y desviaciones) | **Aceptado:** D-T3B1-07, D-T3B1-03 (estado de «Listo para editar» y el `NotAdmitted` de la recarga) y D-T3B1-06 (desviaciones registradas; `Dematerialize` solo en `Present`) |

## 9. Disposición de la re-revisión de la v2 (EMITIBLE TRAS CORRECCIONES, 0/2/6)

La re-revisión confirma que la v2 cierra los 10 IMPORTANTE y los 11 MENOR de la v1, con las salvedades de N-1 y N-2, y no encuentra ninguna guarda no declarada que rompa con lo que introduce la v2 (`PreviewTextInput`, `PreviewKeyDown`, `PreviewMouseLeftButtonDown`, `LostFocus`, `MouseDoubleClick`, `e.Handled`, el `TextBox`, los botones nuevos, los nombres de automatización y el atributo nuevo en el `Button` de la plantilla de pestaña).

| Hallazgo | Disposición en la v3 |
|---|---|
| N-1 (la tecla del primer gesto se aplica; D-01 a medias) | **Aceptado:** la pulsación en `PreviewKeyDown` con `Handled`; el `KeyDown` de burbuja marca `Handled` lo no consumido con la caja editable, salvo Tab; sin `e.KeyboardDevice`; pruebas (e) y (f); la lista de D-01 en Smoke-2 (D-T3B1-03, D-T3B1-04, D-T3B1-12) |
| N-2 (R-B2-5 promete lo que T3B2 no puede cumplir) | **Aceptado, opción (a):** R-B2-5 se redacta a lo alcanzable, con la limitación declarada hasta T3C; se informa al Owner |
| N-3 (`isPreview` desfasado tras fijar) | **Aceptado:** el campo se renderiza al final de cada `Rebuild`, después de `RebuildTabs` (D-T3B1-02) |
| N-4 (el renderizado borra el aviso) | **Aceptado:** se conservan `NoticeText` y el diagnóstico de `NotAdmitted` de la misma superficie hasta un gesto nuevo o un cambio de superficie (D-T3B1-02); prueba |
| N-5 (contención y editabilidad del setter; `CommitField` no editable) | **Aceptado:** D-T3B1-04 y pruebas |
| N-6 (guardas nuevas y G-04) | **Aceptado:** `TextChanged` en el XAML, `SourceUpdated`, `TargetUpdated` y `\.Text\s*=(?!=)` (D-T3B1-12); el equilibrio de G-04 con `<Border(?=[\s>])` y autopruebas (§5); G-15 redactada sobre el `Border` de la plantilla (§2) |
| N-7 (huecos de pruebas) | **Aceptado:** el gancho comprueba la fijación dentro de `TryBeginEdit` (prueba 9); el evento de túnel en las pruebas de teclado; una prueba de `Dematerialize` con una pestaña solo dirty; `CommittedText` null (D-T3B1-02, D-T3B1-12) |
| N-8 (horizonte) | **Aceptado:** el prompt de planificación cita D-T3B1-04 como receta. La salida C07 hacia el Owner, con el bloque separable de los arrastres de Core, sigue prevista |
