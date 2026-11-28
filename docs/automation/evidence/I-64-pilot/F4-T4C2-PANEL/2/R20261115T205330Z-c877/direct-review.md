# I-64 F4-T4C2-PANEL: revisión directa de la sesión (intentos 1 y 2)

**Objeto:** todo el cambio de T4C2, `1cce48c0..b88dabf2`:
- el RED del intento 1, `a6511c04`, con el esqueleto inerte y las pruebas;
- el RED de la corrección, `f1f1c7ee`, con las tres pruebas corregidas;
- el GREEN `b88dabf2`, con el producto del parche custodiado.

Frente a `gate-contract-attempt2.json` y la parte [2] del paquete v5.

## 1. Comprobaciones mecánicas

| Comprobación | Resultado |
|---|---|
| El RED de la corrección (`a912f645..f1f1c7ee`) | solo las dos rutas de prueba de INV-F4T4C-26 |
| El GREEN (`f1f1c7ee..b88dabf2`) | solo los tres archivos de producto del parche; ninguna ruta de `tests/` |
| El GREEN frente al parche custodiado (SHA-256 `551C31C0…`) | **idéntico**: las mismas 317 líneas cambiadas, medidas por la sesión, y el mismo `patch-id` según la entrega |
| CI exacta | GREEN `43758023582` 4/4 en success, con UI Workspace 694/694; RED `43756155557` con Core en failure (10 de 3437), UI en failure (75 de 694), build-ui en success y build-plugin omitido |
| El producto total (`1cce48c0..b88dabf2 -- src`) | 5 archivos: `WorkspaceEditIntent.cs`, `WorkspaceHostCore.cs`, `WorkspacePanelView.xaml`, `WorkspacePanelView.xaml.cs` y `WorkspacePilotFieldViewModel.cs` |

## 2. Lectura del producto

- **INV-F4T4C-23, la frontera de Application:**
  - en `WorkspaceHostCore.cs` solo cambia el cuerpo de `TryCheckPilotEdit`;
  - en `WorkspaceEditIntent.cs` solo `EditCheckOutcome`: dos propiedades opcionales, el constructor y las fábricas `Recovered` y `Conflict`, con parámetros opcionales;
  - ningún valor nuevo de `EditCheckStatus`.

  **Cumple.**
- **D-T4C-12:** el diagnóstico se calcula de la misma lectura (`pilot.Read`), **antes** de `CheckNow` (`WorkspaceHostCore.cs:443`), en su propio `try` que da `null`, y llega a Recovered y a Conflict. La clasificación sigue saliendo de `CheckNow`. **Cumple.**
- **El gesto `Update()`:**
  - el orden es el del paquete: el botón; lo pendiente, que se presenta sin pedir; la frontera Update; `RequestUpdate` una vez; y `TakeUpdateProgress` tras Accepted;
  - cada valor de inicio que no es Accepted se presenta en el gesto;
  - `IsMine` se comprueba tras la frontera y tras la petición, con `PresentSurfaceChange`;
  - no hay ningún `Task`, `async`, evento ni `+=` nuevo.

  **Cumple.**
- **El botón** (`OffersUpdate`): una superficie fijada, Dirty, sin vuelo y sin una elección de T4A2 pendiente.
- **Los textos nuevos** son constantes del VM.

## 3. Hallazgos

| Id | Gravedad | Contenido |
|---|---|---|
| D-1 | Menor (a juzgar en la revisión independiente) | **El botón exige `!HasChoice`:** no se ofrece mientras haya una elección de T4A2 pendiente. El paquete lo ofrece «también con un diagnóstico persistente» y no habla de las elecciones. Es una decisión conservadora, sin efecto sobre la escritura |
| D-2 | Menor (declarada por la entrega) | **El repintado no recoge el resultado** en una vista previa ni con el campo en Error. El resultado queda en el almacén hasta que la superficie fijada de su rack se pinte otra vez. Es fiel a la regla 3 (la superficie viva de su rack) |

**Veredicto:** sin bloqueantes ni importantes. El producto es el del parche, y cumple la parte [2] y la frontera de INV-F4T4C-23.
