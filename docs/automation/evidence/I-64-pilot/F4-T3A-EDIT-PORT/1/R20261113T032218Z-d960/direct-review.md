# Revisión directa de la sesión — I-64 F4-T3A-EDIT-PORT, intento 1

- **Rango:** `ea34bf7d..18471988`, con el RED `2522e894` y el GREEN `18471988`; 12 archivos, exactamente el `AllowedWriteScope` del contrato. El GREEN no toca `tests/`.
- **Pruebas de la entrega y de la CI exacta:**
  - CI `43351822533` del RED: falla el job de Core por aserción (110 de 2251 en el filtro de Core; los 110 son de `RackCad.Tests.Workspace`), UI 248/248;
  - CI `43355611879` del GREEN: los cuatro jobs en verde; Core 2251/2251 en el filtro y 17789/17789 en la suite; UI 248/248 en el filtro y 1885 aprobadas, 0 fallos, en la suite (Debug, como la CI).
- **Modo:** lectura del diff, del paquete v2 (D-T3-02 a D-T3-11), del contrato y de las órdenes del Owner, y del código de `Drafts/` que el puerto usa (`DraftSession`, `DraftRegistry`, `DraftClosePolicy`), que no cambia.

## Veredicto

**Producto conforme.** Sin defectos de corrección frente a INV-F4T3A-01..14 ni frente al oráculo crítico. Dos MENORES de pruebas y guardas, más observaciones para T3B.

## Lo comprobado

### El puerto (`IWorkspacePilotEditPort.cs`)
- Las 12 operaciones con las firmas de D-T3-02; los cuatro enums con nombre, de nivel superior, en el orden del paquete (`PilotBoundaryKind` termina en `DocumentClose`, así que T4 puede añadir `Update` al final).
- Los valores son `readonly struct` o `readonly record struct`, copian las listas con `ToArray()` y devuelven listas vacías en el `default`. Ningún tipo de `Drafts` ni del piloto aparece; los RackIds son `string`.

### El host (`WorkspaceHostCore`)
- Implementa el puerto **explícitamente**. `IWorkspaceLifecycleAdvisor` sigue siendo la primera base. La superficie pública no crece. El adaptador es `private static readonly` (H-10).
- **Compuerta de reentrada:** las tres lecturas ponen `_pilotReadInProgress` en `try/finally` y una lectura anidada da `Waiting` sin leer. Con la marca:
  - `SetPendingText` y `CommitField` dan `Busy`;
  - `CrossBoundary` bloqueante da `Busy`;
  - `DiscardRack` y `DiscardDocument` dan `EditDiscardStatus.Busy`, añadido al final del enum.
- **`GetField`, `GetRacksAtRisk` y las no bloqueantes:** sin `Authorize`. Las no bloqueantes devuelven `Kept` antes de tocar el borrador: no validan, no comprometen y no autorizan.
- **`SetPendingText`:** exige `Authorize`, la ausencia de marca y `CanEdit`. Guarda con `AnnotationScaleKey`, también el texto vacío, y llama a `NotePilotEdit`. `DraftEditStatus` solo tiene `Updated` y `NotEditable`, así que no hay un rechazo falso por texto repetido.
- **`CommitField` y la bloqueante:** usan `draft.CrossBoundary(TabChange/RackChange, adaptador)`.
  - `Proceeded` llama a `NotePilotEdit` solo si el estado cambió. Es seguro: limpiar el pendiente nunca ensucia, y la marca ya la quitó el `SetPendingText` previo. La marca solo se pone con un Allow sin racks en riesgo, es decir, sin pendiente.
  - `Aborted` → `Invalid`, con el texto intacto y el mensaje del campo.
  - `Kept` del núcleo (no editable con pendiente) → `Kept`.
  - Sin borrador → `Proceeded`.
- **`DiscardRack`:** usa `DiscardByUser`, que no llama al comparador. Quita la marca. Rack vacío → `Discarded` con listas vacías.
- **`DiscardDocument`:** delega en `DiscardPilotEdits`, que quita la marca.
- **`GetRacksAtRisk`:** usa el `RacksAtRiskOf` del registro, en orden ordinal. Si el comparador lanza, decide borrador por borrador, con el pendiente primero (sin comparador). Los indecidibles van aparte, también en orden ordinal porque `RackIdsOf` ya lo está.
- **`ViewOfPilotDraft`:**
  - `Status` lanza → `Undecidable`, con `IsProtected` true y `CanEdit` false;
  - `CanEdit` = registrado y {Clean, Dirty, Conflict};
  - el texto comprometido sale de D-T3-03;
  - el mensaje del pendiente sale de `Validate`.
- **Excepciones:** se contienen como valores con motivo; `Report` no degrada la sesión.
- **Cola de veto:**
  - `AdviseClose` encola en sus dos caminos de Veto: los racks de la decisión, o el indicador incierto sin lista en el `catch`;
  - en el Allow quita el aviso justo antes de `_knownClean.Add`;
  - `SessionDestroyed` lo quita al retirar la sesión;
  - `DiscardRack` lo quita si la sesión queda sin riesgo, y `DiscardDocument` siempre que descarta;
  - es una lista con a lo sumo un aviso por sesión, el último. `TakeCloseNotices` la vacía en orden.
  - No cambian la decisión, la marca ni la secuencia de `_lastAllowedClose`.
- **`TakeLossNotices`:** saca la cola interna en orden, como valores. Es la única llamada a `Dequeue` y no es pública en el host.

### Soporte (`WorkspacePilotSupport.cs`)
- **`WorkspacePilotValueText`:**
  - no finito o ≤ 0 → null con motivo;
  - si no, desplaza la coma del texto `"R"` invariante según el exponente, quita los ceros sobrantes y pone el separador decimal de la cultura;
  - comprobé a mano 1E-05 → `0.00001`, 1E+15, `double.Epsilon` (`5E-324`) y `double.MaxValue` (309 dígitos enteros);
  - sin exponente ni separador de miles.
- **`WorkspacePanelModalScope`:**
  - activa antes de devolver;
  - contador anidado: solo el último desactiva;
  - el `Dispose` doble es idempotente, porque la retención se anula.
- **`WorkspaceNoticeDepth`:** nunca baja de 0.
- **`WorkspacePilotEditGate`:** da `Waiting` en las tres lecturas dentro de un aviso; el resto es delegación literal.

### Plugin
- **`WorkspaceController`:**
  - la profundidad envuelve `NotifyStateChanged` en `try/finally`;
  - `PilotEdit` accede a `Navigation` y devuelve un adaptador anidado que solo delega en el decorador; la UI nunca recibe el host;
  - `EnterPanelModal()` va sobre el ámbito, con `_host.SetModalRackCadActive`;
  - ningún `+=` nuevo ni `Dispatcher` nuevo.
- **`AutoCadEventBridge` (T2C 4):** documentación de la clase y los dos contextos de registro distintos. El cableado no cambia.

### Pruebas existentes
Los cuatro archivos solo llevan los cambios declarados:
- **M-2:**
  - las dos firmas del asesor, sin el conteo de `)\s*;`;
  - `_knownClean.Remove` ≥ 9;
  - `.DiscardAndReload(` en el cuerpo de la recarga;
  - `TakeLossNotices` como acceso del puerto;
  - la guarda de `NotePilotEdit` por operación, con autopruebas.
- **D-1:** la cota que nombra los 5 archivos de `Drafts/`, con autoprueba.
- **M-3:** la etiqueta `known-safe`.
- **M-5:** `Detail` y `Witness` de la recarga `NotAdmitted`; el núcleo arrancado y la sesión no degradada en el fallo del descarte.
- **El cuerpo de `AdviseClose`:** el orden de la marca se conserva y se añaden el encolado y el `Drop`, con autopruebas.
- **T2C 1-4:**
  - el intermedio prohibido en los manejadores y en `OnIdleTick`, nombrado 2 veces;
  - `.OnIdle(` con cualquier receptor = 1;
  - los miembros del planificador = {`OnIdleTick`, `SetPost`};
  - el argumento exacto de `SetPost` en los dos órdenes;
  - `ExecuteInApplicationContext` y `Dispatcher.Run` prohibidos;
  - `TotalReads == 0` y `AssertNoReadInsideACallback` en las tres pruebas del cierre pendiente;
  - los literales de contexto actualizados.

## Hallazgos

### 1. MENOR: el «escritor simulado» del oráculo crítico no está conectado a nada
- **Dónde:** `tests/RackCad.Tests/Workspace/WorkspacePilotEditHostTests.cs:36-44` (`PilotEditAuthoredWriterSpy`) y los `Assert.Empty(env.Writer.Writes)` (`:265`, `:402`, `:421`, `:460`, `:814` y `NoOperationOfThePortWritesAuthoredDataOfTheDrawing`).
- **Qué pasa:** su propia documentación dice que nada del host, del puerto ni de los dobles está cableado a él. `Writes` nunca recibe nada, así que esas aserciones no pueden fallar con ninguna implementación.
- **Por qué no es un defecto de producto:** el oráculo de T3A se sostiene de forma estructural:
  - Application no tiene dependencia de escritura;
  - la guarda `TheOperationsOfThePortAndTheDecoratorNameNoApiOfWritingTheDrawing` prohíbe el patrón `WriteApis` en los cuerpos de las 12 operaciones y del decorador, con autopruebas por operación;
  - la prueba del oráculo sí discrimina las lecturas: `Fake.Calls == Audited.Reads`, `Hr.Reader.ReadCalls == 0`, `ModalHistory` vacío y sin fallos.
- **Disposición propuesta:** registrar en la custodia que el escritor simulado de D-T3-10 es nominal y que el oráculo de T3A es la guarda por cuerpo más la ausencia de dependencia de escritura. T4, que sí tendrá un escritor, debe conectar un espía real.

### 2. MENOR: la guarda nueva de límites repite conteos exactos de archivo entero
- **Dónde:** `WorkspacePilotEditGuardTests.cs:871-881` (`ThisTaskAddsNoFileToThePluginWorkspaceFolderNorToTheDraftsAndNoCommandNorSubscription`).
- **Qué fija:** `Src.Files(Drafts/).Count == 5`, `PluginFiles == 9`, y 16 `+=`, 16 `-=` y 16 líneas de manejador en el puente.
- **Por qué importa:** todos duplican guardas exactas que ya existen:
  - `WorkspaceDraftBoundaryTests.ExpectedFiles`;
  - `TheFoldersAndTheGuardsOfTheOlderTasksKeepTheirShape`;
  - `WorkspaceBridgeFacadeSourceGuardTests:111`.

  Por eso no añaden ninguna restricción nueva. Pero D-1 convirtió precisamente el 5 exacto de `WorkspaceLifecycleGuardTests` en una cota, para que una tarea posterior cambie una guarda y no varias, y esta prueba vuelve a añadir otro sitio que cambiar (regla I-1). El nombre la limita a «esta tarea», pero la comprobación es global y permanente.
- **Disposición propuesta:** residual declarado para el paquete de T3B, que puede reducirla a cotas o retirarla como cambio declarado.

### Observaciones para T3B (no defectos)
- **El `default` de las decisiones:** el de `PilotBoundaryResult` lee `Decision == Proceeded`, porque el paquete fijó el orden {`Proceeded`, `Aborted`, `Kept`, `Busy`}. T3B nunca debe tratar un valor por defecto como una decisión.
- **`DiscardRack` y el comparador:** el descarte en sí no lo llama, pero, para decidir si quita el aviso de veto, `DiscardRack` consulta el riesgo de los borradores que quedan. Si el comparador lanza, el aviso se conserva, lo que es conservador.
- **Las colas sin `try`:** `TakeLossNotices` y `TakeCloseNotices` no van envueltas en `try`. No hay un camino realista de excepción: solo hacen `Dequeue`, `ToArray` y constructores que copian listas no nulas.
- **La suite de UI en Release:** da 2 fallos de `PendingTextFieldTests.ShowOnADirtyField_*`, que dependen de `#if DEBUG` (`PendingTextField.cs:101`, de 2026-09-03). La CI corre en Debug y allí pasan. Es una condición previa ajena a la tarea, declarada por el Worker.

## Riesgos para Smoke-2 (sin cambio respecto del paquete)
- El foco y el modal de AutoCAD frente al ámbito del panel.
- El orden real de `BeginDocumentClose` y `DocumentToBeDestroyed` con la cola de veto.
- Un `BeginDocumentClose` sin enganchar: edición degradada sin veto (D-T3-09).
- La vía del constructor de la paleta fuera de la compuerta de avisos (D-T3-06, para T3B).
