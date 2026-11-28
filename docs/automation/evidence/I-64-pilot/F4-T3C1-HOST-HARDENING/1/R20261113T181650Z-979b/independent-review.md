# Revisión adversarial independiente del código de F4-T3C1-HOST-HARDENING

- **Objeto:** el worktree de I-64, rama `architecture/workspace-persistente-rackcad`, rango `c04ddfb3..7bd275e4`: RED `b845b9a6` y GREEN `7bd275e4`, 12 archivos.
- **Autoridades leídas:** `gate-contract.json` de F4-T3C1 (INV-F4T3C1-01..12, AllowedWriteScope, ForbiddenWriteScope y F4T3C1-C01..C08), el paquete v3 `docs/initiatives/I-64-f4-t3c-gate-contract-package.md` (§2 a §6, §8 y §9), `map-4-minors-modal.md`, el contrato y el paquete de cierre de F4-T3B2, y las guardas y pruebas previas que tocan el área.
- **Modo:** solo lectura, con `git show`, `git diff` y `git grep`. No ejecuté builds, pruebas ni CI. Lo que digo de RED y GREEN sale del razonamiento sobre el código, no de una ejecución.
- **Comprobaciones mecánicas propias:** con un script de solo lectura sobre `git show`, comparé byte a byte los 28 literales de H-07 y los textos nuevos de la UI con el paquete, y apliqué la regla de extracción de M-5 sobre `c04ddfb3`, que tiene el mismo código que `23f67f56`.

## Veredicto

**ACEPTABLE CON MENORES:** 0 bloqueantes, 0 importantes y 6 menores.

El producto cumple las decisiones D-T3C1-01 a D-T3C1-07 y las invariantes INV-F4T3C1-01 a 12. No encontré ningún escenario de producto que falle. Los menores son huecos de discriminación de pruebas, documentación del puerto sin ajustar y declaraciones incompletas.

## Hallazgos

### 1. MENOR. La quita del prefijo duplicado en `WhyOf` no tiene ninguna prueba ni guarda que la discrimine

- **Dónde:** `src/RackCad.UI/Workspace/WorkspaceBrowserViewModel.cs:937-948` (Busy en 943).
- **Escenario:** el mutante que vuelve al código anterior (`case Busy: return "hay una lectura en curso. " + reason;`) pasa toda la suite.
  - Las únicas pruebas con `Busy` en `DiscardRack` usan `Assert.Contains`: `tests/RackCad.UI.Tests/Workspace/WorkspacePilotCloseSyncTests.cs:252-265` y `312-345`.
  - Ninguna guarda nueva mira `WhyOf`.
  - Tampoco el RED falló por esta parte: el cambio de GREEN no tiene evidencia RED→GREEN.
- **Qué incumple:** la letra de INV-F4T3C1-05 y D-T3C1-05 sobre `WhyOf`, que queda sin verificar.
- **Por qué es menor:** el §4 solo pide la prueba de `RefusalOf` en el VM del campo, y la implementación es correcta.
- **Propuesta:** una prueba con `EditDiscardOutcome.Busy("X")` en `CloseTab` o en `ConfirmDiscard` que afirme el texto exacto «…: X», y otra con el motivo en blanco.

### 2. MENOR. La prueba de la memoria de la confirmación sin lectura no discrimina que `Ask` la quite (M-2)

- **Dónde:**
  - el producto es correcto: `WorkspaceBrowserViewModel.cs:996` asigna `_confirmUnread = couldNotRead` sin condición, y 1007 la quita en `RemoveConfirmation`;
  - la prueba es `WorkspacePilotCloseSyncTests.cs:1521-1541`.
- **Escenario:** la prueba pasa por `CancelDiscard()` (1535), es decir por `RemoveConfirmation`, antes de la segunda petición. Sobrevive el mutante plausible `if (couldNotRead) _confirmUnread = true;` en `Ask`.
  - Con ese mutante, si se pide `CloseTab(R-1)` con `GetField` que lanza y después, sin cancelar, `CloseTab(R-2)` sobre una protegida legible, se vería «No se pudo comprobar la pestaña R-2…» aunque R-2 sí se leyó.
  - Ninguna guarda de `WorkspaceHostHardeningGuardTests.cs` mira `_confirmUnread`.
- **Qué incumple:** la regla «`Ask` lo pone, y `RemoveConfirmation` y cualquier otra petición lo quitan» (D-T3C1-07, INV-F4T3C1-07) solo está cubierta a medias. La prueba se llama `…AnyOtherRequestRemovesTheMemoryOfIt`, pero no prueba eso.

### 3. MENOR. «Solo en el estado Error» del host no está discriminado (D-T3C1-06)

- **Dónde:** `src/RackCad.Application/Workspace/WorkspaceHostCore.cs:908-910`; la prueba es `tests/RackCad.Tests/Workspace/WorkspacePilotEditHostTests.cs:1644-1663`.
- **Escenario:** el mutante sin la condición (`var errorReason = draft.ErrorReason;`) pasa la prueba `TheReasonOfTheErrorOfADraftCrossesThePortOnlyInTheStateError`.
  - El borrador Dirty de la prueba nunca entró en Error, así que su `ErrorReason` ya es null por la semántica de `DraftSession`: `Rebase` lo limpia, y solo `EnterError` o `CompleteApply` lo ponen, junto con el estado Error.
  - Ninguna guarda mira la condición.
- **Por qué es menor:** hoy no tiene efecto observable. El VM del campo también filtra por estado, y eso sí está probado en `WorkspacePilotFieldViewModelTests.cs` (`TheReasonOfTheErrorOnlyAppearsInTheStateError`). El nombre de la prueba del host afirma más de lo que verifica.

### 4. MENOR. La documentación del puerto y de los gestos no refleja la semántica nueva

- **`src/RackCad.Application/Workspace/IWorkspacePilotEditPort.cs:72` y `75`:** siguen diciendo que `TakeLossNotices` y `TakeCloseNotices` toman «every notice … and leaves the queue empty». Tras D-T3C1-03 eso ya no es exacto:
  - un aviso de pérdida cuya conversión lanza no se devuelve y solo queda en el registro;
  - si `TakeCloseNotices` falla, devuelve vacío y **no** vacía la cola.
- **`WorkspaceBrowserViewModel.cs:352-356` y `402-407`:** la documentación de `RequestDocumentDiscard` y de `ConfirmDiscard` no menciona el desconocido. Sigue diciendo «With none the notice says nothing was discarded».
- **Por qué importa:** D-T3C1-01 sí ajustó la regla general del puerto y la de `GetRacksAtRisk`. El contrato del puerto es la base de F4-T3C2, y su documentación de las colas queda contradiciendo el código.
- **Por qué es menor:** el paquete no lo exige de forma explícita para las colas.

### 5. MENOR, declarativo. Cambio no declarado de un ayudante existente del doble

- **Dónde:** `tests/RackCad.UI.Tests/Workspace/WorkspacePilotEditPortDouble.cs:96-105`. `View(...)` gana el parámetro opcional `string? errorReason = null` y lo pasa al constructor.
- **Lo que pedía el paquete:** el §4 admitía «Acaso un ayudante que devuelva `default`». Eso sí está, en 109, como `UnknownRisk()`.
- **Por qué es menor:** el cambio es inocuo. La respuesta por defecto no cambia, los usos existentes compilan igual, y está dentro del AllowedWriteScope. Pero amplía un ayudante existente sin declararlo. Con F4T3C1-C02 es un caso límite, no una violación: no cambia ninguna aserción.

### 6. MENOR, declarativo. Las limitaciones declaradas están incompletas frente a D-T3C1-08

Juzgo por la lista de limitaciones del Worker tal como la recibí en el encargo; no vi el handoff.

- **Coinciden con el paquete:** los textos de F3 en inglés, `ErrorReason` latente, el aviso de pérdida que se pierde, y los `catch` inalcanzables cubiertos solo por guardas (de D-T3C1-01 y D-T3C1-03).
- **Falta:** «el desconocido no lleva motivo en el valor: va al registro». El código lo cumple: `Report("WorkspaceHostCore.GetRacksAtRisk", ex)` y `return default;`.
- **Conviene declarar también:** hasta F4-T3C2 (R-C14), el campo mezcla idiomas.
  - Los motivos del host llegan en español.
  - Los del lector del Plugin siguen en inglés y se ven tal cual. Por ejemplo, `WorkspaceNavigationHost.cs:226` produce «En espera: A command is in progress.», porque el host devuelve `read.Reason` cuando no es null.
- **Ninguna de las limitaciones declaradas oculta un defecto.**

## Lo que comprobé y encontré correcto

### Alcance y oráculo

- **Alcance:** exactamente las 12 rutas del AllowedWriteScope. El único archivo nuevo es `WorkspaceHostHardeningGuardTests.cs`.
- **GREEN:** no toca `tests/`.
- **Archivos fuera del alcance:** no se tocan el Plugin, el controlador, el puente, los comandos, `BrowserSession` ni el XAML.
- **Llamadas nuevas:** el producto no gana ninguna llamada a `TryBeginEdit`, `TryReload`, `TryCheckNow` ni `ReadIntent`, ninguna escritura y ningún `EnterError` de borrador en `src/`.
- **Mecanismos nuevos en la UI:** no hay `+=`, `event`, `Dispatcher`, `Timer`, `Task`, `async`, API de foco, `Registry`, `RackLog`, `MessageBox` ni `ShowDialog`.
- **Costuras:** no hay costuras, reflexión ni conversiones de tipo sobre `LossNotices` o las colas.
- **F3:** no cambian la llamada a `IsSessionAlive` (`PurgeDeadSessions`) ni el `Refresh`.
- **Codificación:** los archivos conservan la codificación (UTF-8 sin BOM) y los finales de línea.

### RED

- **Esqueleto:** `IsKnown => true` sin `_known`, el parámetro `errorReason` aceptado con `ErrorReason => null`, los literales sin traducir y la UI sin cambio.
- **Pruebas que fallan:** por razonamiento, solo las nuevas, y por aserción. Las de construcción usan `Record.Exception`.

### D-T3C1-01 y D-T3C1-02

- **El valor desconocido:**
  - `_known` es `private readonly bool` y solo se asigna en el único constructor, antes de las comprobaciones de null;
  - `default` y `new PilotRiskView()` dan falso;
  - todo valor construido da verdadero;
  - la igualdad por valor ya no confunde el desconocido con «nada en riesgo».
- **El `catch` externo de `GetRacksAtRisk`:** sigue siendo exactamente `Report(...)` y `return default;`.
- **La vía de reserva y la sesión sin borradores:** dan valores conocidos, y está probado.
- **`RequestDocumentDiscard`:** mira `IsKnown` antes de `NoRiskText`, quita solo una confirmación del documento (la de una pestaña se queda), no pide confirmación y no descarta.
- **`ConfirmDiscard`:** con la segunda lectura desconocida, quita la confirmación y no descarta.
- **Textos:** coinciden byte a byte con el paquete, incluido el de solo inciertos.
- **Confirmación del documento sin racks:** las dos listas no pueden estar vacías a la vez cuando hay confirmación.

### D-T3C1-03 y D-T3C1-04

- **`TakeLossNotices`:** desencola fuera del `try`, convierte dentro y reporta y sigue.
- **`TakeCloseNotices`:** tiene el cuerpo entero en un `try` y el `catch` no vacía la cola.
- **`Authorize`:** ninguna de las dos lo llama.
- **`SessionDestroyed`:**
  - `DropCloseNotice` va dentro del primer `try`, tras la retirada;
  - `_knownClean.Remove` está en su sitio;
  - hay 2 `try` y 2 `catch (Exception ex)`, en el mismo orden;
  - lo que se observa si la retirada lanza es lo declarado: el veto queda en la cola del host hasta que el panel lo toma y lo purga.

### D-T3C1-05 y D-T3C1-06

- **Los 28 literales:** son idénticos a la tabla H-07 en NFC y en el mismo número de sitios (por ejemplo, 6 para el #2 y 4 para el #20).
  - No queda ningún literal en inglés en el host ni en el soporte.
  - Se conservan los cuatro identificadores.
  - No cambian `DescribeLoss`, `DescribeDifferences` ni `ex.Message`.
  - No hay subcadenas de G-04, ni en los textos ni en los comentarios nuevos (conteo igual antes y después).
  - No se tradujo ningún texto de F3.
- **`RefusalOf`:** solo cae al genérico sin motivo.
- **`errorReason`:** es el último parámetro opcional, y los usos de 9 argumentos no cambian.
- **El campo:** muestra el motivo del Error, después el motivo actual y después «sin detalle.».

### M-1, M-2 y M-3

- **M-1:** `RemoveConfirmation()` va en el `catch` de la rama limpia, antes de `CloseNow`. La prueba discrimina.
- **M-2:**
  - `couldNotCheck` se asigna antes de la cadena de `&&`, y los dos avisos coinciden con el paquete;
  - `_confirmUnread` solo puede ser verdadero con una confirmación `CloseTab` de esa pestaña: `Ask` lo reasigna siempre, y `RemoveConfirmation` y `ValidateConfirmation` lo limpian.
- **M-3:**
  - solo el `Present` del constructor queda en el `try` con `EnterError`;
  - el veto y las pérdidas ya tomados quedan en `_stored`, así que no se pierde nada, y el reintento recupera;
  - no rompe `WorkspaceBrowserViewModelTests.cs:1006-1019`, porque sin veto en la construcción no se pregunta por la vida;
  - se respetan G-11 y G-13 (la frase prohibida no aparece).

### Guardas de F4-T3B2 en `WorkspacePilotCloseSyncGuardTests.cs`

- **M-4:**
  - la expresión `([\w>]+(?:\[\])*\??)$` exige un carácter de palabra;
  - las siete autopruebas están, más tres extra;
  - el conjunto de lo que cuenta como declaración se reduce, así que las prohibiciones quedan iguales o más estrictas.
- **M-5:**
  - todas las guardas de la lista del §2.3 tienen una función que se puede comprobar y su autoprueba de mutación (`Rejects` lanza `XunitException`);
  - las autopruebas de 279-286 y 456-464 cubren ahora todas las alternativas de sus expresiones;
  - la línea 221 está partida;
  - no se perdió ninguna expresión regular ni ninguna aserción: la única expresión antigua que ya no está es la de `IsDeclaration`, que se sustituyó a propósito;
  - **la forma pública, comprobada por mí:** la regla de extracción sobre `c04ddfb3` da exactamente las 33 cadenas de la guarda, en el mismo orden y byte a byte. Son 41 líneas `public`: 2 records, la clase, 5 de `StoredNotice` y 33 miembros.
- **M-6:** sin cambio.
- **M-7:** la guarda de los seis gestos tiene comprobación de no vacuidad y autoprueba (incluido `x ? ContainFailure(e)`, que ejercita M-4), y hay pruebas de los cuatro gestos.

### Las demás pruebas existentes y las guardas nuevas

- **Pruebas existentes de comportamiento:** solo se les añaden pruebas, sin ningún hunk que borre una aserción.
- **Guardas de `WorkspaceHostHardeningGuardTests.cs`:**
  - buscan por cuerpo de miembro y con expresiones de llamada;
  - cada una tiene autoprueba de mutación y caso de miembro ausente.
- **F4-T3C2:** ninguna guarda prohíbe para siempre lo que necesita según el §6. No fijan `RACKPANELDESCARTAR`, `WriteMessage`, el controlador, `SetLifecycleAdvisor`, el envoltorio en una clase nueva de Application ni los textos del Plugin.
  - **Observación:** `LiteralViolations` fija el número exacto de apariciones de cada texto en español en el host y el soporte, más de lo que pide el §4. Una reutilización legítima en línea en esos dos archivos exigiría un cambio declarado. No afecta a R-C5..R-C14: la lógica de T3C2 va a clases nuevas, y R-C14 al Plugin.

## Riesgos para el smoke con AutoCAD (no son defectos)

1. **Despliegue:** la firma del constructor de `PilotFieldView` cambia (10 parámetros). Hay que desplegar `Application`, `UI` y `Plugin` juntos, con AutoCAD cerrado al recompilar. Un DLL viejo mezclado daría `MissingMethodException`.
2. **Recrear la paleta con un veto en cola (M-3, punto del cierre de F4-T3B2):**
   - comprobar que la construcción no lanza;
   - si `IsSessionAlive` falla, el panel debe mostrar «Error del panel» y «Reintentar» debe recuperarlo;
   - con el host real, lo normal es que no falle y que la presentación sea la de siempre.
3. **Idioma mixto en el campo hasta T3C2 (R-C14):** el gate del host da «Hay un comando en curso.» y «Hay una ventana de RackCad abierta.», pero el lector del Plugin puede seguir dando «A command is in progress.» o «There is no active document.».
4. **Cierre con veto y posterior destrucción:** con `DropCloseNotice` después de la retirada, verificar que la fila del veto desaparece tras cerrar el documento y que aparece la de pérdida cuando corresponde. En el camino normal no debería cambiar nada visible.
5. **Textos más largos en el aviso del navegador** (por ejemplo, el del desconocido o los de retención): verificar el ajuste de línea en la paleta estrecha. El XAML no cambia y no fija tamaños.
6. **Lo que el smoke no puede ejercitar:** el desconocido (el `catch` es inalcanzable), la retención y la confirmación «no se pudo comprobar» (exigen que `GetField` lance) y `ErrorReason` (latente hasta F4-T4). Solo las cubren las pruebas unitarias y las guardas.
7. **Modal y foco:** no cambian. Siguen pendientes de Smoke-2.
