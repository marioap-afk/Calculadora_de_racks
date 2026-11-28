# Revisión directa de la sesión — I-64 F4-T3B1-EDIT-SURFACE, intento 1

- **Rango:** `e0e0166b..5de2fab3`, con el RED `f0637250` y el GREEN `5de2fab3`; 19 archivos, exactamente el `AllowedWriteScope` del contrato. El GREEN no toca `tests/`.
- **Pruebas de la entrega y de la CI exacta:**
  - CI `43371973228` del RED: falla el job de UI (67 de 327 en el filtro de UI); Core pasa entero (2298/2298 en el filtro, 17836/17836 en la suite), como exige `ExpectRed` false;
  - CI `43373240897` del GREEN: los cuatro jobs en verde; Core 2298/2298 en el filtro; UI 327/327 en el filtro y 1964 aprobadas, 0 fallos, en la suite (Debug).
- **Modo:** lectura del diff, del paquete v3 (D-T3B1-01 a D-T3B1-12), del contrato y del Freeze (D-01), y un experimento de WPF fuera del repositorio (abajo).

## Veredicto

**Producto NO conforme por un defecto BLOQUEANTE de teclado**, que nace del propio paquete (D-T3B1-04 v3) y del contrato (INV-F4T3B1-04), no de una desviación del Worker. El resto del producto es conforme.

## Hallazgo 1 — BLOQUEANTE: la caja editable no admite texto escrito con el teclado

**Dónde:** `src/RackCad.UI/Workspace/WorkspacePanelView.xaml.cs`, `OnBrowserFieldBoxKeyDown`:

```csharp
if (_viewModel.Browser.Field.IsEditable && e.Key != Key.Tab) e.Handled = true;
```

Lo enlaza el atributo `KeyDown="OnBrowserFieldBoxKeyDown"` de `BrowserFieldBox`. Es la regla del paquete: «con la caja editable, un `KeyDown` de burbuja marca `Handled` toda tecla que el editor no haya consumido, salvo Tab».

**Problema.**
- `TextBox` no consume en `KeyDown` las teclas que producen caracteres: los recibe después, por `WM_CHAR` y `TextInput`.
- En WPF, marcar `Handled` el `KeyDown` de una tecla hace que el preproceso del `HwndSource` dé el mensaje por traducido. No se llama a `TranslateMessage`, no hay `WM_CHAR` y no llega `TextInput`.
- Con esta regla, ninguna letra, cifra, coma, punto ni espacio llega a la caja: el campo editable no se puede editar con el teclado.

**Evidencia** (`keyboard-experiment/`, junto a este archivo): una ventana WPF mínima con un `TextBox`, net8.0-windows. Se envían con `PostMessage` `WM_KEYDOWN` y `WM_KEYUP` de la tecla A y se bombea el dispatcher (el camino real de WPF):

| Caso | Resultado |
|---|---|
| sin manejador | `keyDowns=1 textInputs=1 text='a'` |
| `KeyDown` que marca todo salvo Tab (la regla de la entrega) | `keyDowns=1 textInputs=0 text=''` |
| `PreviewKeyDown` que marca todo | `keyDowns=0 textInputs=0 text=''` |

**Por qué las pruebas no lo vieron:** las pruebas de vista asignan `box.Text` directamente y lanzan `KeyDownEvent` con `RaiseEvent`. Ninguna recorre el camino `WM_KEYDOWN` → `WM_CHAR` → `TextInput`. La prueba `KeysThatTheEditorDidNotConsumeStayInThePanelWithAnEditableBoxExceptTab` fija justo el comportamiento defectuoso.

**D-01 no lo exige:** D-01 (`proposal-v5.md:130-137`) congela la obligación (ninguna tecla llega a AutoCAD con el foco en un campo), pero **no la mecánica**: `KeepFocus`, `IKeyboardInputSink`, etc. En el host, las teclas que AutoCAD intercepta (aceleradores como Ctrl+Z o Ctrl+V) se resuelven en el preproceso del host o de la paleta, no con un `Handled` de burbuja en WPF. Es materia de Smoke-2, con `KeepFocus = true`, que ya existe.

**Corrección propuesta (intento 2):**
- quitar el `KeyDown` de burbuja de la caja editable;
- conservar Enter y Esc `Handled` en el `PreviewKeyDown` de la caja editable, y la regla de la pulsación en la caja en solo lectura (el primer gesto se consume y no escribe, D-T3-04);
- una prueba que discrimine: en la caja editable, el `KeyDown` de una tecla de carácter (A, D5, Space, OemComma, OemPeriod) queda **sin** `Handled`;
- una guarda: la caja no tiene manejador de `KeyDown` de burbuja que marque `Handled`;
- la lista completa de teclas de D-01 en el host, con `KeepFocus`, queda como verificación de Smoke-2.

## Lo comprobado (conforme)

- **Cableado (D-T3B1-01):**
  - la paleta pasa `_controller.PilotEdit` como quinto argumento;
  - el VM del panel lo recibe con el quinto parámetro opcional y el navegador con el tercero;
  - sin puerto, `Field.IsAvailable` es false y el navegador es el de antes.
- **VM del campo (D-T3B1-02):**
  - editable solo con borrador, `CanEdit`, pestaña fijada y sin un Limpio posiblemente obsoleto;
  - el navegador lo renderiza al final de cada `Rebuild`, después de `RebuildTabs`, con el `IsPreview` vigente;
  - `Show` conserva el aviso y el diagnóstico de la misma superficie;
  - `CommittedText` null muestra la caja vacía y el motivo en el estado;
  - es síncrono, sin `+=`, evento ni estado estático.
- **Primer gesto (D-T3B1-03):**
  - `BeginFieldEdit` fija la preview (`PinTab`) antes de `Field.BeginEdit`;
  - `BeginEdit` elige `TryReload` para un Limpio obsoleto y `TryBeginEdit` en otro caso, con `EditIntentRequest(session, rack, null, null)`;
  - comprueba la identidad de la superficie tras la lectura (`IsMine`);
  - `NotAdmitted` guarda el diagnóstico y la indicación;
  - el doble clic actúa solo sobre la pestaña activa;
  - la fijación automática de una preview protegida ocurre en `RebuildTabs` (`Tabs` es una copia, así que es seguro fijar durante el recorrido).
- **Edición (D-T3B1-04, salvo el hallazgo 1):**
  - binding TwoWay con `UpdateSourceTrigger=PropertyChanged`;
  - el setter es la única entrada; sin efecto y con restauración si no es editable; restaura tras un rechazo; contiene sus fallos;
  - `CommitPending` solo con pendiente y caja editable;
  - Enter y Esc `Handled` en el `PreviewKeyDown` de la caja editable;
  - sin `TextChanged`, `SourceUpdated`, `TargetUpdated` ni `.Text =` en el code-behind.
- **Transiciones (D-T3B1-06):**
  - `AllowTabChange` cruza `TabChange` solo con destino existente y distinto y con pendiente, y avisa con `InvalidFields` o `Reason`;
  - `CrossObservedBoundaries` cruza `DocumentChange` o `Dematerialize` solo en `Present`, después de `Rebuild`;
  - `CloseTab` no cambia.
- **«Nombre *»** en `RebuildTabs` con `IsProtected`; el record no cambia.
- **Estados y contención (D-T3B1-08):** «Recargar» es `Rematerialize` (`GetField`, sin lectura); los fallos van al Error del campo.
- **Guardas y arrastres:** los cambios declarados G-01..G-04 y los arrastres de D-T3B1-11 están presentes. `WorkspacePilotSurfaceGuardTests.cs` lleva autopruebas.

## Observación menor

`IsProtected` del navegador devuelve false si `GetField` lanza. El puerto real nunca lanza, pero un doble sí podría: es la dirección no conservadora para «Nombre *» y la fijación. Conviene `true` en la corrección.

## Riesgos para Smoke-2

- la lista de teclas de D-01 con el foco en la caja, en solo lectura y en edición;
- `LostFocus` frente a un clic en el dibujo;
- el clic de pestaña tras un compromiso que cambia «*»;
- la relectura del binding y el cursor tras una restauración.
