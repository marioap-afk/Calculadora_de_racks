# Revisión adversarial independiente: F4-T3A-EDIT-PORT

- Worktree: el worktree de I-64 (ruta local omitida), rama `architecture/workspace-persistente-rackcad`.
- Rango: `ea34bf7d..18471988`. RED `2522e894` y GREEN `18471988`. 12 archivos: 6 de `src` y 6 de `tests`.
- Autoridades leídas desde el repositorio:
  - `gate-contract.json` de F4-T3A: INV-F4T3A-01..14, AllowedWriteScope, ForbiddenWriteScope y C01..C08;
  - el paquete v2: §2, §4, §5, §7, §8, §9 y §10;
  - las órdenes del Owner `owner-f4t3-authorization.md` y `owner-f4t3-decomposition-decision.md`;
  - el código de `Drafts/` (`DraftSession`, `DraftRegistry`, `DraftClosePolicy`), `EventBridgeCore`, `LocalizedNumberParser`, `PushBackPilotFieldAdapter` y los helpers de guardas (`WorkspaceHostSourceSupport.cs`).
- Método: solo lectura, con `git show`, `git diff`, `git grep` y lectura de archivos. **No ejecuté build ni pruebas, y no verifiqué la CI.** Las afirmaciones sobre si una guarda pasa o falla salen de leer sus regex contra el texto del GREEN.

## Veredicto

**ACEPTABLE CON MENORES.** 0 BLOQUEANTE, 0 IMPORTANTE y 7 MENOR.

La semántica del puerto, las dos compuertas, la cola de veto, el texto mostrado y el ámbito del modal cumplen el contrato. No encontré escritura authored, lecturas del DWG fuera de las tres operaciones de la intención, `+=` nuevos ni cambios no declarados en guardas o pruebas existentes. Los hallazgos son de calidad de evidencia, robustez teórica y documentación.

## Hallazgos

### 1. MENOR — El «escritor simulado» del oráculo crítico es vacuo

**Dónde.** `tests/RackCad.Tests/Workspace/WorkspacePilotEditHostTests.cs`:
- la clase `PilotEditAuthoredWriterSpy` (`:41-44`);
- `PilotEditAuditedReader` guarda `_writer` (`:50`, `:55`) y nunca lo usa;
- `Writes` no recibe nada en todo el archivo.

**Problema.** Las aserciones `Assert.Empty(env.Writer.Writes)` (`:265`, `:402`, `:421`, `:460`, `:814`, `:1383`) son tautologías. La prueba `NoOperationOfThePortWritesAuthoredDataOfTheDrawing` (`:1344-1389`) pasaría contra cualquier implementación, también una que escribiera, porque nada puede llegar al espía. Su comentario dice «every double that could ever carry a write reports it here», y es falso. Sus otras aserciones tampoco discriminan:
- `env.Audited.Reads == env.Fake.Calls.Count` es cierta por construcción, porque el lector auditado delega siempre;
- solo `ReadCalls == 0`, `ModalHistory` vacío y `Faults` vacíos aportan algo.

**Invariante afectado.** INV-F4T3A-10 («un escritor simulado registra cero escrituras») y el oráculo crítico del Owner (D-T3-11) se cumplen solo en el nombre por esta vía.

**Por qué no es más grave.** La garantía real existe por otros medios:
- `RackCad.Application.csproj` no referencia AutoCAD;
- el núcleo no tiene ninguna dependencia de escritura;
- la guarda por cuerpo `WriteViolations` (`WorkspacePilotEditGuardTests.cs:664-737`) sí discrimina, con autopruebas.

**Recomendación.** En el mapa evidencia-por-invariante, no presentar esta prueba como evidencia del oráculo. Citar la guarda por cuerpo y el argumento estructural.

### 2. MENOR — Una guarda nueva vuelve a fijar conteos exactos y deshace D-1 (regla I-1)

**Dónde.** `WorkspacePilotEditGuardTests.cs:871-881` (`ThisTaskAddsNoFileToThePluginWorkspaceFolderNorToTheDraftsAndNoCommandNorSubscription`):
- `:874`: `Assert.Equal(5, Src.Files(DraftsFolder).Count)`;
- `:875-877`: `+=`, `-=` y `handler = (` del puente, exactamente 16 cada uno.

**Problema.** D-1 (D-T3-09, INV-F4T3A-11) cambió en `WorkspaceLifecycleGuardTests.cs` el conteo exacto de `Drafts/` por una cota que nombra cada archivo (`:655`, `:792`, `MissingDraftFiles`), precisamente para no fijar el número. La guarda nueva vuelve a fijar el número exacto en otro archivo. Además, los 16/16/16 del puente y el 9 del Plugin duplican guardas que ya existen (`WorkspaceBridgeFacadeSourceGuardTests.cs:111-113`, `WorkspaceHostC2SourceGuardTests.cs:194-196` y `WorkspaceBrowserHostGuardTests.cs:38`).

**Escenario.** Una tarea futura añade un archivo legítimo a `Drafts/`. Con D-1, solo tendría que tocar el conjunto exacto de `WorkspaceDraftBoundaryTests.cs`. Ahora tiene que tocar también una guarda de T3A que se presenta como limitada a esta tarea.

**Invariante afectado.** La regla I-1 de D-T3-10: nada de conjuntos exactos de archivo entero donde basta una cota o un cuerpo propio.

**Recomendación.** Bastaría con nombrar los 5 archivos de `Drafts/` y los 9 del Plugin como cota (como hace `MissingDraftFiles`) y quitar los 16 duplicados. De paso: `WorkspaceLifecycleGuardTests.cs:403` añade `_lossNotices.Dequeue` = 1 sobre el archivo entero. Es aceptable dentro de M-2, porque T3C no consume la cola.

### 3. MENOR — Huecos de discriminación en las pruebas de host

**(a) Frontera bloqueante con un borrador no editable y texto pendiente.** La fila de D-T3-02 dice «→ `Kept`: la transición procede y el pendiente se conserva sin aplicar», y no tiene prueba. Ninguna prueba hace `TabChange` o `RackChange` sobre un borrador en Error, Unknown o Applying con pendiente. El único `Kept` probado es el de las fronteras no bloqueantes (`:330`, `:348`, `:801`).

Pasaría todas las pruebas una implementación plausible pero incorrecta: una que comprobara `CanEdit` antes de cruzar, como hace `CommitField`, y respondiera `Aborted`, o una que mapeara `DraftBoundaryDecision.Kept` a `Aborted`. Esa implementación dejaría al usuario atrapado en una pestaña en Error con pendiente. El GREEN es correcto: el `default:` de `WorkspaceHostCore.cs:711-712` responde `Kept`. Lo que falta es la prueba.

**(b) Sin control negativo de NotePilotEdit.** Las pruebas `PathAfter` de este archivo (`:151-159`, y sus usos en `:189`, `:199`, `:217` y `:466`) solo afirman «conservative». Ninguna afirma «known-safe» tras una operación que no muta:
- `GetField`;
- `GetRacksAtRisk`;
- una frontera no bloqueante;
- `CommitField` sin pendiente;
- un `SetPendingText` rechazado o `Busy`.

Una implementación que llamara a `NotePilotEdit` en todos los caminos pasaría. Es la dirección segura, pero la pregunta «¿y en ninguno que no muta?» queda sin evidencia.

**(c) Los `catch` externos sin cubrir.** No se ejercitan los `catch` externos de `GetField` (`:603-607`, la vista `Error`) ni de `GetRacksAtRisk` (`:802-806`). `AnExceptionOfAnOperationIsContainedAsAValue...` (`:1309-1340`) no llega a ellos, porque esas dos operaciones no tocan la sonda.

**(d) Una aserción tautológica.** `WorkspaceIdleSchedulingGuardTests.cs:719` (`Assert.NotEqual("AutoCadEventBridge.IdleContinuation.Pass", "AutoCadEventBridge.IdleContinuation.OpenTransaction")`) compara dos literales. Es inocua, porque las dos aserciones anteriores sí miden el código.

**Invariante afectado.** INV-F4T3A-13 y D-T3-10: las pruebas deben discriminar.

### 4. MENOR — Contención incompleta y un fallo abierto en las operaciones de memoria

**`TakeLossNotices` y `TakeCloseNotices` sin `try/catch`.** Están en `WorkspaceHostCore.cs:809-826`. Si `new PilotLossView(...)` lanzara a mitad del bucle, los avisos ya sacados de la cola se perderían en silencio y la excepción llegaría al panel. INV-F4T3A-01 pide que «toda excepción se contiene como un valor». Hoy es inalcanzable en la práctica, porque `DraftLossNotice` garantiza listas no nulas.

**El fallo de `GetRacksAtRisk` se lee como «nada en riesgo».** El `catch` externo (`:802-806`) devuelve `default`, con las dos listas vacías. Es indistinguible de «nada en riesgo»: `PilotRiskView` no tiene indicador de fallo. Hoy también es inalcanzable, porque `Find<T>` devuelve null cuando el tipo no coincide. Pero dos consumidores de T3B y T3C se apoyan en este valor:
- R-C2 calcula la pérdida con `GetRacksAtRisk` antes de delegar `SessionDestroyed`;
- R-B8 y R-C1 hacen la comprobación TOCTOU.

Lo seguro sería devolver el fallo como incierto. Por contraste, `GetField` sí falla de forma segura: `IsProtected = true`.

### 5. MENOR — `Undecidable` solo aparece cuando el estado no es explícito

**Dónde.** `ViewOfPilotDraft` (`WorkspaceHostCore.cs:844-890`). `State` sale de `draft.Status`, y `Status` solo consulta el comparador cuando `_explicitStatus` es null (`DraftSession`, `Status => _explicitStatus ?? (IsDirty ? ...)`).

**Escenario.** Un borrador en Conflict cuyo comparador lanza:
- `State = Conflict`, no `Undecidable`;
- `IsProtected = true`, porque el `catch` de `:865`;
- `CanEdit = true`.

La letra de INV-F4T3A-02 dice «Undecidable cuando el comparador lanza (IsProtected true, CanEdit false)».

**Por qué no es más grave.** El resultado es funcionalmente seguro:
- con pendiente, `IsDirty` corta en `_pending.Count > 0` sin llamar al comparador;
- `GetRacksAtRisk` y `AdviseClose` lo tratan como incierto.

Es una ambigüedad del contrato resuelta sin declararla, y no tiene prueba: la de `Undecidable` (`:646-662`) solo cubre un borrador sin estado explícito. **Recomendación:** documentar la regla, por ejemplo «Undecidable solo cuando el estado ordinario Clean/Dirty no se puede decidir».

### 6. MENOR — En `SessionDestroyed`, `DropCloseNotice` queda fuera de los `try` y antes de la retirada

**Dónde.** `WorkspaceHostCore.cs:950`, cuerpo cerrado de F4-T2B2B.

**Problema.** El cambio es obligado por D-T3-07 («cuando la sesión se retira»), pero se insertó antes del `try` de la retirada y fuera de toda contención. El comentario del propio método dice «The retirement and the notice are contained apart». Si `DropCloseNotice` lanzara, los borradores de la sesión destruida no se retirarían y quedarían en el registro. Hoy es inalcanzable, porque es un `List<T>.RemoveAll` sobre una comparación de struct. La guarda de `SessionDestroyed` (`WorkspaceLifecycleGuardTests.cs:378-389`) sigue pasando.

**Recomendación.** Bastaría con moverlo después de la retirada, o dentro del segundo `try`. INV-F4T3A-14 pide no cambiar el comportamiento cerrado de F4-T2B2B, y hoy no lo cambia, salvo por esa vía teórica.

### 7. MENOR — Documentación del puerto que T3B va a necesitar

**`Kept` no dice que la transición procede.** En `IWorkspacePilotEditPort.cs:122-123`, el `summary` de `PilotBoundaryDecision.Kept` dice solo «The pending text was kept as it is». En una frontera bloqueante, `Kept` significa «la transición procede y el pendiente se conserva» (D-T3-02), mientras que `Aborted` y `Busy` abortan. T3B tiene que deducirlo.

**El valor por defecto es «procede».** `default(PilotBoundaryResult).Decision` vale `Proceeded`, porque el orden lo fija el contrato (`:117`). Un valor por defecto, por ejemplo de un doble o de un camino olvidado, se lee como permiso para la transición. No es un defecto de T3A, pero conviene que T3B nunca trate un valor por defecto como permiso.

**`EditCheckStatus.Waiting` sin documentar.** No tiene documentación de los dos motivos nuevos (lectura en curso y aviso del panel), aunque el decorador y el host lo devuelven. Solo se corrigió `EditIntentStatus.Waiting`, en `WorkspaceEditIntent.cs`.

**Observación sin hallazgo.** INV-F4T3A-01 dice que la sesión va en la petición «salvo TakeLossNotices», pero `TakeCloseNotices()` tampoco lleva sesión. La implementación sigue la tabla de D-T3-02, que lo define así, de modo que es una incoherencia del contrato y no de la entrega.

## Lo que comprobé y encontré correcto

### Alcance

- Los 12 archivos son exactamente el AllowedWriteScope y nada toca el ForbiddenWriteScope.
- El GREEN (`2522e894..18471988`) no toca `tests/`.
- No hay archivos nuevos en `Plugin/Workspace` ni en `Drafts/`.
- `Drafts/` no cambia.

### Oráculo crítico

- Ninguna operación ni helper del puerto, ni el decorador, nombra una API de escritura.
- No hay escritor ni transacción, y no se llama a ningún escritor.
- Las únicas lecturas del DWG están dentro de `TryBeginPilotEdit`, `TryCheckPilotEdit` y `TryReloadPilotEdit`, a través de los tres envoltorios del puerto.
- `Authorize` y la sonda solo leen memoria.
- El puente solo cambia en documentación y literales de contexto de registro.

### Puerto, operación por operación (D-T3-02)

- **Implementación y tipos.** La implementación es explícita: ninguno de los 12 nombres es público en el host y `: IWorkspaceLifecycleAdvisor` sigue siendo la primera base. El adaptador del piloto es un campo estático. Ningún tipo de `Drafts` ni del piloto cruza el puerto, y las listas nunca son null, también en `default`.
- **Lecturas de memoria.** `GetField` y `GetRacksAtRisk` no autorizan. `GetRacksAtRisk` devuelve los racks en orden ordinal en los dos caminos, con los inciertos aparte.
- **`SetPendingText`.**
  - Hace `Authorize`, comprueba la marca de lectura y `CanEdit`.
  - Guarda el texto con la clave del adaptador, también el texto vacío, y llama a `NotePilotEdit`.
- **`CommitField`.**
  - Usa `CrossBoundary(TabChange, adapter)`.
  - Sin pendiente responde `Applied` sin `NotePilotEdit`.
  - Con un texto válido lo aplica y llama a `NotePilotEdit`.
  - Con un texto inválido responde `Invalid`, con el texto intacto y sin reparar nada.
- **`CrossBoundary`.**
  - Las no bloqueantes responden `Kept` antes de `Authorize`, sin validar y sin tocar el borrador.
  - Las bloqueantes, en orden: `Authorize`, `Busy` y después la mecánica del núcleo (`Aborted`, `Proceeded` con `NotePilotEdit`, o `Kept`). Sin borrador responden `Proceeded`.
- **Descartes.**
  - `DiscardRack` usa `DiscardByUser`, sin comparador sobre el borrador descartado, quita la marca de limpio conocido y quita el aviso si la sesión queda sin riesgo.
  - `DiscardDocument` delega en `DiscardPilotEdits`.
- **Colas.** `TakeLossNotices` y `TakeCloseNotices` vacían en orden.

### Compuerta de reentrada

- La marca se pone en `try/finally` en las tres lecturas. Una lectura anidada responde `Waiting` sin leer.
- Durante una lectura, las mutaciones responden `Busy` sin efecto y la frontera bloqueante responde `Busy`.
- `EditDiscardStatus.Busy` está al final, y la guarda de prefijo ordenado lo admite.
- La prueba con el lector que llama al puerto dentro de `ReadIntent` discrimina.

### Texto mostrado (D-T3-03)

- El algoritmo es correcto:
  - `"R"` invariante;
  - desplazamiento de la coma sin exponente ni separador de miles;
  - separador decimal de la cultura actual.
- El parser con la misma cultura devuelve el mismo `double` en es-MX, en-US, de-DE e invariante, también para `double.Epsilon`, `double.MaxValue`, 1E-05 y 1E+20. Las pruebas comparan los bits.
- Los valores no finitos o ≤ 0 (−0 incluido) dan null con motivo.
- Mostrar nunca crea pendiente, y comprometer el texto mostrado no marca sucio (prueba en de-DE).

### Ámbito del modal y compuerta de avisos

- El ámbito activa antes de devolver, lleva un contador anidado, solo el último `Dispose` desactiva, el `Dispose` doble es inocuo y la salida por excepción también desactiva.
- `EnterPanelModal` va sobre `_host.SetModalRackCadActive`. El reenvío público sigue literal y la cadena `_bridge` / `_core` no cambia.
- La profundidad de avisos entra antes del `try` y sale en el `finally` de `NotifyStateChanged`.
- El decorador responde `Waiting` en las tres lecturas y delega lo demás tal cual.
- `PilotEdit` es un adaptador anidado privado que accede a `Navigation` antes de devolver. La UI no puede alcanzar el `WorkspaceHostCore`.
- No hay `+=`, `-=`, `Dispatcher`, `Timer` ni `Task` nuevos.

### Cola de veto (D-T3-07)

- Se encola en los dos caminos de Veto: con la lista de `EvaluateClose`, o incierto sin lista y sin recorrer `_drafts` cuando la evaluación lanza.
- Hay a lo sumo un aviso por sesión, el último.
- El aviso se quita al descartar, en el Allow antes de `_knownClean.Add` y en `SessionDestroyed`.
- No cambian la decisión, la marca, `_lastAllowedClose` ni ningún olvido del puente.

### Cambios en guardas y pruebas existentes

Comparé cada hunk de los 4 archivos. Todos están declarados y ninguno debilita fuera de lo declarado:
- **`WorkspaceLifecycleGuardTests.cs`:**
  - M-2: las dos firmas del asesor, la cota ≥ 9, `.DiscardAndReload(` por cuerpo y `TakeLossNotices` admitido;
  - la guarda nueva de `NotePilotEdit`, con autopruebas;
  - D-1 en los dos sitios;
  - el cuerpo de `AdviseClose`, más estricto, con su autoprueba.
- **`WorkspaceLifecycleHostTests.cs`:**
  - M-3: el cambio de etiqueta basta, porque el borrador sucio distingue las dos vías;
  - M-5: `Detail` y `Witness`, y el núcleo arrancado sin degradar.
- **`WorkspaceIdleSchedulingGuardTests.cs`:**
  - T2C 1: el intermedio en G-1 y G-2, nombrado 2 veces; `.OnIdle(` con cualquier receptor una vez; miembros `{OnIdleTick, SetPost}`;
  - T2C 2: el argumento exacto de `SetPost` en los dos órdenes; `ExecuteInApplicationContext` y `Dispatcher.Run` prohibidos;
  - T2C 4: solo literales.
- **`WorkspaceIdleSchedulerTests.cs`:** T2C 3, `TotalReads == 0` y `AssertNoReadInsideACallback` en las tres pruebas.

### RED

- El esqueleto es inerte, no lanza y devuelve valores no nulos.
- Las pruebas de comportamiento fallan por aserción contra él.
- Los cambios declarados van en el RED.

### Trampas H-10 a H-12

- Ningún nombre de tipo nuevo contiene Draft, Editor, Subscription ni «tab», ni coincide con una palabra de `BrowserSession` o de `Drafts`.
- No aparecen Transaction, Database, `Commit(`, RackLog ni Provider/Metric/Bom.
- Los dos archivos nuevos no nombran el piloto.
- El controlador conserva 3 `+=`, 0 `-=` y un `event`.

### T3B, T3C y T4 no quedan bloqueados

- R-B1..R-B12 y R-C1..R-C4 tienen las operaciones que necesitan.
- El envoltorio del asesor de T3C cabe como clase anidada del controlador.
- T4 puede añadir `Update` al final de `PilotBoundaryKind` y su operación sin romper las guardas por cuerpo; las autopruebas lo demuestran.
- La única fricción es la del hallazgo 2.

## Riesgos para el smoke con AutoCAD

No son defectos de T3A.

1. **`Authorize` a cada pulsación.** `SetPendingText` y `CommitField` dependen de `MdiActiveDocument` en el momento de la tecla. Hay que verificar que en CLOSE, CLOSEALL o QUIT, en un cambio de documento o con el foco en la paleta y sin documento MDI, un rechazo restaura la caja de forma visible (R-B4) y no pierde texto.
2. **Lo que la compuerta de reentrada no cubre.** La compuerta solo cubre el puerto. Hay que comprobar si `ReadIntent` bombea mensajes. Si los bombea, la entrada de WPF da `Busy`, que es lo correcto, pero un drenaje (`OnDrained` → `MarkPilotDraftsPossiblyStale`) o un `SessionDestroyed` durante la lectura de una recarga no pasan por la marca. La recarga hace `DiscardAndReload` sin volver a comprobar (H-04): la marca de obsoleto podría perderse con una lectura anterior al cambio. El comportamiento es previo, de F4-T2B2B, y conviene observarlo.
3. **Lo que la compuerta de avisos no cubre.** Solo cubre la entrega síncrona dentro de `NotifyStateChanged`. Escapan a ella:
   - un trabajo que un manejador de T3B difiera con `Dispatcher.BeginInvoke`;
   - la llamada directa a `OnControllerStateChanged()` del constructor de `WorkspacePaletteHost`.

   R-B10 sigue siendo una regla de diseño de T3B que el smoke debe confirmar.
4. **Los avisos de veto no emiten `StateChanged`.** El panel solo los verá en el siguiente aviso o gesto. Hay que revisar la experiencia tras un veto de CLOSE, CLOSEALL o QUIT, también de un documento no activo, y que T3B los consuma fuera de todo callback (R-B9).
5. **Edición con un comando activo o en modo degradado.** `SetPendingText` y `CommitField` no miran la quiescencia ni la degradación. Con el enganche de `BeginDocumentClose` fallido no hay veto (D-04, residual de Smoke-2). Conviene confirmar que la edición con un comando activo es aceptable según D-02 en el host real.
6. **El ámbito del modal.** Cuando T3B o F5 lo usen, hay que verificar que la marca está activa antes de que la ventana sea la activa (`ShowModalWindow`), y el residual conocido: `SetModalRackCadActive` olvida la secuencia de cierre, lo que da una retirada conservadora.
7. **Cultura.** El texto mostrado usa la `CurrentCulture` del hilo de AutoCAD, con los ajustes de usuario de Windows. Hay que probar una configuración regional distinta del idioma de AutoCAD, por ejemplo es-MX con coma decimal personalizada, y el ida y vuelta Enter/salir sin editar (sin sucio falso).
