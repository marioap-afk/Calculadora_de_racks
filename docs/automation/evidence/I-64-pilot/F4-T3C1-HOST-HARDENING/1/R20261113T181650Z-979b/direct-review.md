# Revisión directa de la sesión: F4-T3C1-HOST-HARDENING, intento 1

- **Rango:** `c04ddfb3..7bd275e4`.
- **Commits:** RED `b845b9a6` y GREEN `7bd275e4`.
- **Worker:** `R20261113T165143Z-f35e`, con 282 turnos de `claude-sonnet-5-5`, esfuerzo high y 0 denegaciones.

**Veredicto: producto conforme.** Sin hallazgos propios. La revisión independiente es aparte.

## Comprobado

### Alcance
- Exactamente las 12 rutas del contrato. Una es nueva: `WorkspaceHostHardeningGuardTests.cs`.
- El GREEN no toca `tests/`: `git diff b845b9a6 7bd275e4` solo cambia los 6 archivos de `src/`.
- No cambian el Plugin, el controlador, el puente, los comandos, `BrowserSession`, el XAML, la llamada a `IsSessionAlive` del VM ni el `Refresh` de F3.

### RED (CI `43472387303`)
- Core y UI terminan en failure; el build de la UI, en success.
- Fallan 15 pruebas del filtro de Core y 14 ejecuciones del de UI, y ninguna fuera del filtro.
- Todas son métodos añadidos en el RED (91 métodos nuevos). No falla ninguna prueba existente.
- Fallan todas las pruebas de comportamiento y todas las guardas de estructura del producto. Pasan las autopruebas y las guardas de M-4, M-5 y M-7, que fijan un comportamiento ya cierto en el producto.

### GREEN (CI `43477341374`)
- Los cuatro jobs terminan en success.
- Filtros: 2517/2517 en Core y 454/454 en UI.
- Suites completas:
  - Core: 18055/18055.
  - UI: pasan 2091 y fallan 0. Hay 17 `NotExecuted`, que son las condicionales ya existentes, idénticas al RED.

### D-T3C1-01. El desconocido de `GetRacksAtRisk`
- `PilotRiskView` gana `private readonly bool _known`, que solo asigna el constructor (que sigue siendo el único), y `IsKnown => _known`. `default` es el desconocido.
- El `catch` externo de `GetRacksAtRisk` no cambia: `Report` y `return default;`.
- La documentación del puerto y la regla general se ajustan.

### D-T3C1-02. Los consumidores de la UI
- **`RequestDocumentDiscard`:** con `IsKnown` falso da el texto exacto del paquete. Quita solo una confirmación del documento y no pide confirmación.
- **`ConfirmDiscard`:** con la segunda lectura desconocida no descarta, quita la confirmación y da el texto exacto.
- **`NoRiskText`:** solo sale después de mirar `IsKnown`.
- **Solo inciertos:** la pregunta lleva el texto exacto.

### D-T3C1-03. Las colas
- **`TakeLossNotices`:** desencola fuera del `try` y convierte cada aviso dentro del suyo. Si falla, `Report` y sigue con el siguiente.
- **`TakeCloseNotices`:** todo su cuerpo va en un `try`. El `catch` hace `Report`, devuelve una lista vacía y no vacía la cola.
- Ninguna de las dos usa `Authorize`.

### D-T3C1-04. `SessionDestroyed`
- `DropCloseNotice` pasa dentro del primer `try`, después de la retirada.
- `_knownClean.Remove` sigue en su sitio, y siguen dos `try` y dos `catch`.

### D-T3C1-05. Los motivos en español
- Comprobación mecánica de los 28 literales de la tabla H-07 contra la base y el GREEN: cada uno tiene 0 apariciones en inglés y sustituye todas las que había por su español exacto.
- Se conservan los cuatro identificadores.
- `DescribeLoss`, `DescribeDifferences` y `ex.Message` quedan intactos.
- Sin duplicación en la UI:
  - `RefusalOf` muestra el motivo para `Busy` y `NotEditable`, y el genérico solo sin motivo;
  - `WhyOf` hace lo mismo para `Busy`.

### D-T3C1-06. `ErrorReason`
- Es el último parámetro opcional, `string? errorReason = null`, y los usos de 9 argumentos no cambian.
- El host lo rellena con `DraftSession.ErrorReason` solo en el estado Error.
- El VM del campo muestra el motivo del Error, después el motivo actual y después «sin detalle.».

### D-T3C1-07. Los menores de F4-T3B2
- **M-1:** `RemoveConfirmation()` en el `catch` de la rama limpia de `CloseTab`.
- **M-2:**
  - `couldNotCheck` se asigna antes de la cadena de `&&`, y los dos avisos tienen el texto exacto;
  - `_confirmUnread`: `Ask` lo pone con su argumento, que por defecto es falso, así que cualquier otra petición lo quita, y `RemoveConfirmation` lo quita;
  - la pregunta neutral tiene el texto exacto.
- **M-3:** solo el `Present` del constructor del panel va en un `try` con `EnterError`. La documentación de `browserSummary` y de `DismissNotice` está corregida, y ya no tiene la frase prohibida.
- **M-4:** la expresión `([\w>]+(?:\[\])*\??)$` y la exigencia de un carácter de palabra, con las siete autopruebas.
- **M-5:**
  - la forma pública es la lista literal de 33 firmas, comparada como conjunto y con su autoprueba;
  - las guardas existentes se factorizaron en ayudantes con autopruebas en negativo.
  - Se comprobó que no se perdió ninguna aserción ni alternativa de expresión. Los dos `Fact` retirados (`TheGuardOfTheSubscriptionsSeesTheComposedTextWithPlusEquals` y `TheGuardOfTheKeyHandlerSeesTheKeyboardDeviceAndKeyUp`) se sustituyen por `Theory` que contienen sus mismos casos y añaden otros.
  - La regla de `Scheduling` y las cuatro de alcance de la UI se conservan literalmente.
- **M-7:** las pruebas de los cuatro gestos y la guarda de cero llamadas a `ContainFailure` en los seis, con su autoprueba.
- **M-6 y D-1:** sin cambio, como dice el paquete.

### Límites
- No hay reflexión, `BindingFlags` ni costuras en las pruebas. La guarda nueva prohíbe la reflexión en los tres archivos del host y en los tres VM.
- No hay `Skip` ni `#if DEBUG` nuevos.
- No se añade ninguna lectura de la intención.

## Notas (sin hallazgo)

- La guarda de alcance de la UI prohíbe `\bRegistry\b` y `\bRackLog\b` en el VM del navegador y en el code-behind. Ya existía desde F4-T3B2 y no afecta al §6 de T3C2, cuyos mensajes van en el archivo del comando del Plugin. La revisión independiente comprueba el resto de las guardas contra el §6.
- El Worker declara 2 errores de herramienta y ninguna desviación.
