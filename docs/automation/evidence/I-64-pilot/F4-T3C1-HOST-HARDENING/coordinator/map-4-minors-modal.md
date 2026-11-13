# Mapa 4: menores de F4-T3B2, modal y foco, D-1 (explorador, HEAD `7895f397`)

Abreviaturas: VM = `src/RackCad.UI/Workspace/WorkspaceBrowserViewModel.cs`; PVM = `WorkspacePanelViewModel.cs`; G = `tests/RackCad.Tests/Workspace/WorkspacePilotCloseSyncGuardTests.cs`; T = `tests/RackCad.UI.Tests/Workspace/WorkspacePilotCloseSyncTests.cs`.

## 1. Menores M-1 a M-7 (todos confirmados)

- **M-1:**
  - en VM:291-294, el `catch` interior solo compone el aviso y después llama a `CloseNow` (296), que conserva la confirmación de otra pestaña;
  - la prueba T:315-345, caso Throws, no tiene confirmación previa;
  - **arreglo:** `RemoveConfirmation()` en ese `catch`, con una prueba que pida antes la confirmación de una protegida y luego cierre una limpia con `Throws`.
- **M-2:**
  - VM:1004 escribe siempre «…de: » + títulos;
  - es alcanzable por la vía de reserva del host (774-800);
  - el `default` del `catch` (802-805) es el endurecimiento de T3C;
  - la única prueba (T:612-630) tiene racks;
  - casos límite del mismo tipo: VM:1000 con `ReadField` null y VM:657-660 cuando `KeepsActive` cae en su `catch`;
  - **arreglo:** condicionar la frase a `_confirmRacks.Count > 0`, con una prueba de solo inciertos.
- **M-3:**
  - el constructor de PVM (73) llama a `Present` sin `try` (la vía contenida es `Query`, 183-195);
  - VM:221-222 llama a `TakeNotices` y `PurgeDeadSessions`, que en 625-630 consulta `IsSessionAlive` de las sesiones de los vetos sin contención;
  - la documentación de PVM:60 dice «never when the panel is built»;
  - la prueba de F3 `WorkspaceBrowserViewModelTests.cs:913-920` construye el panel sin `pilotEdit`;
  - el doble ya tiene `ThrowOnAlive` (`WorkspaceBrowserSummaryPortDouble.cs:44`, 65-68);
  - relacionado: la documentación de `DismissNotice` (516) dice «asks nothing», pero `Publish` llama a `GetField` a través de `IsProtected` (1278).
- **M-4:**
  - G:46 usa `([\w>\]\?]+)$`, que tras `=>` captura `>`;
  - afecta a `CallCount` (33-40), a 148, 151 y a `CallsOutside` (65-86);
  - las autopruebas (188-191) no cubren `=>` ni `?`;
  - **arreglo:** exigir que el token empiece por `\w`, con autopruebas.
- **M-5:**
  - no tienen autoprueba 196-203, 205-213, 215-225, 229-255, 288-299, 421-438 y 468-485, ni tampoco 323-374 y 404-417;
  - la autoprueba de 456-464 no cubre `e.SystemKey` y siguientes; la de 279-286 solo cubre `+=`, `-=`, `event` y `delegate`;
  - en 221 hay dos `Assert` en una sola línea;
  - la guarda de la forma pública solo comprueba presencia: el VM tiene 41 líneas `public` en HEAD frente a 24 en `b5ae014d`, y 5 son de la clase anidada privada `StoredNotice`, así que «exactamente» debe contar solo los miembros de la clase exterior.
- **M-6:**
  - G:450 aplica la expresión del teclado y del foco a todo el code-behind, aunque `body` ya está extraído (444);
  - G:451 prohíbe `KeyUp` en todo el XAML y G:452 exige un solo `PreviewKeyDown=` (hoy en `WorkspacePanelView.xaml:202`, `BrowserFieldBox`);
  - G:389 prohíbe `IsDefault` e `IsCancel` en todo el XAML;
  - H-08 original: `WorkspacePilotFieldViewTests.cs:486-495`;
  - **arreglo:** 450 sobre `body`, 451-452 sobre `TagOf(xaml, "BrowserFieldBox")` y 389 sobre los `TagOf` de los botones nuevos;
  - además, T:785 y la igualdad exacta de T:780-782 cambian cuando el aviso nombre el comando.
- **M-7:**
  - `ContainFailure` (VM:1219-1233) se llama desde 596 y 614 (`Interact`); `CloseTab` llega a ella por `CloseNow` (884);
  - ninguna prueba afirma `SurfaceStatusText`;
  - **arreglo:** pruebas en T:348-364, 556, 575 y 738-764 que lo afirmen, y una guarda con `CallCount(RequiredBody(...), "ContainFailure") == 0` en los seis gestos, con su autoprueba.

## 2. Modal y foco

- **Dónde está:** `WorkspacePanelModalScope` (`WorkspacePilotSupport.cs:81-120`); `EnterPanelModal` (`WorkspaceController.cs:123-127`) y su reenvío `SetModalRackCadActive` (152). **Nadie los llama en `src/`**; solo las pruebas.
- **Guardas:**
  - `WorkspacePilotEditGuardTests.cs:545-606` y 631-660: todo diálogo de las carpetas Workspace va dentro de `using (EnterPanelModal())`, «Today there is none»;
  - G-05 (`WorkspacePilotSurfaceGuardTests.cs:278-297`), `WorkspaceNavigationHostTests.cs:915-922`, `WorkspacePaletteHostSourceGuardTests.cs:515`, y G:450 y 473-474.
- **El paquete de F4-T3:** D-T3-05 (197-216), R-B7 (414), R-B10 (417) y R-B11 (418). R-C1 a R-C4 no tienen ninguna regla de modal ni de foco. D-T3B1-10: el foco es residual de F7.
- **Conclusión:** ningún contrato concreta una obligación de código de modal o de foco para T3C. Queda:
  - la validación de Smoke-2;
  - si T3C añade un subdiálogo, `EnterPanelModal`;
  - si T3C añade teclado, un cambio declarado de G:389, G:450-452 y G-05.

## 3. D-1 y bloqueos

- **D-1** (G:154-169) solo busca `\bTake(?:Close|Loss)Notices\b` en 4 archivos permitidos. No mira `GetRacksAtRisk` ni `DiscardDocument`, así que no bloquea el comando ni el envoltorio si no nombran las colas (R-C2: «no consume la cola»).
- **Las demás guardas de T3B2 no bloquean:** 95-114 solo miran la UI, 297 el VM del panel y 472 el VM y el code-behind.
- **Sí bloquean, fuera de T3B2:**
  - `WorkspaceLifecycleGuardTests.cs:280-283` (M-1);
  - `WorkspacePilotEditGuardTests.cs:463-511`: el adaptador solo delega, un `public IWorkspacePilotEditPort`, `+=` = 3 y un `event`;
  - el censo de comandos de R-C3.
- `GetRacksAtRisk` es una implementación explícita en el host (765): hay que llamarlo a través de la interfaz.
