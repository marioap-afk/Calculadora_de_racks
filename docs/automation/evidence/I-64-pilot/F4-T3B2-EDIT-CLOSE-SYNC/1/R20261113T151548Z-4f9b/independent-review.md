# I-64 F4-T3B2-EDIT-CLOSE-SYNC: revisión adversarial independiente del código

- **Objeto:** el worktree de I-64, rama `architecture/workspace-persistente-rackcad`, rango `b5ae014d..23f67f56`: RED `c3de6116` y GREEN `23f67f56`, 13 archivos.
- **Método:** solo lectura (`git show`, `git diff` y lectura de archivos). No se ejecutaron builds ni pruebas, y no se abrió AutoCAD. Lo que depende de ejecutarlas, como el verde o rojo de CI, no se comprobó aquí.
- **Autoridades leídas:**
  - el `gate-contract.json` (INV-F4T3B2-01..15, AllowedWriteScope, ForbiddenWriteScope y F4T3B2-C01..C08);
  - el paquete de F4-T3B2 v3 (§2 a §5 y §7 a §9);
  - del paquete de F4-T3B, el §6 (R-B2-1..6);
  - las órdenes del Owner de F4-T3 y de la disposición del cierre de F4-T3B1 (F4-E43);
  - del paquete de cierre de F4-T3B1, N-1, N-2 y N-3;
  - el puerto y su implementación en el host, solo en lectura.

## Veredicto

**ACEPTABLE CON MENORES.** Hay 0 hallazgos BLOQUEANTES, 0 IMPORTANTES y 7 MENORES.

La implementación cumple la tabla de cierre, la confirmación en línea, la TOCTOU, el consumo de las colas, los avisos, la retención y los arrastres N-1 y N-3. No hay ninguna escritura del dibujo, ninguna llamada a las tres lecturas de la intención, ningún Actualizar ni ninguna API prohibida nueva en la UI. Los menores son de letra, de documentación, de robustez de las guardas y de huecos de prueba. Ninguno cambia el comportamiento exigido en los caminos normales.

## Hallazgos

### M-1 (MENOR). El cierre limpio con una excepción del puerto no quita una confirmación pendiente (INV-F4T3B2-12)

**Dónde:** `src/RackCad.UI/Workspace/WorkspaceBrowserViewModel.cs:283-296` (`CloseTab`, rama del borrador no protegido).

La excepción de `DiscardRack` se captura en el `try` interior. La pestaña se cierra (`CloseNow`) y el aviso dice el motivo, pero no se llama a `RemoveConfirmation()`.

**Escenario:**
1. Con la pestaña protegida R-1, el usuario pulsa «Cerrar» y queda pendiente la confirmación de R-1.
2. Después pulsa «Cerrar» en R-2, que tiene un borrador limpio, y el puerto lanza en `DiscardRack`.
3. R-2 se cierra con el aviso «Se cerró la pestaña; su borrador limpio quedó en memoria: …», pero la confirmación de R-1 sigue visible.

La confirmación de R-1 sigue siendo válida para su propia pestaña, así que no hay riesgo de datos. Sin embargo, INV-F4T3B2-12 dice literalmente que «una excepción del puerto en un gesto de F4-T3B2 deja el aviso con el motivo **y quita la confirmación**». Los demás caminos con excepción sí la quitan, a través de `FailGesture`.

**Solución:** quitar la confirmación también en ese `catch`, o declarar la excepción a la regla. Ninguna prueba cubre este caso.

### M-2 (MENOR). La confirmación del descarte del documento con solo inciertos tiene un texto roto (D-T3B2-03)

**Dónde:** `WorkspaceBrowserViewModel.cs:1003-1006` (`ConfirmationTextOf`, tipo `DiscardDocument`).

**Escenario:** `GetRacksAtRisk` devuelve `RackIds` vacía y `UncertainRackIds = [R-4]`. Es la vía de reserva del host (`WorkspaceHostCore.cs`, `GetRacksAtRisk`) cuando falla un comparador. El texto queda así: «¿Descartar los borradores de este documento? Se perderá el trabajo sin aplicar de: . No se pudo decidir: R-4.»

La lista vacía deja «de: .». Es un caso alcanzable, no el que se declaró inalcanzable.

**Casos límite del mismo tipo** (textos que afirman algo que el VM no sabe):
- Si el puerto no puede leer el campo, la confirmación de cierre dice «tiene un borrador con trabajo sin aplicar» (`:1000`).
- Si `GetField` lanza, el aviso de la retención dice «conserva su texto pendiente» (`:657-660`).

### M-3 (MENOR). Con un veto en cola, construir el panel ya pregunta al puerto de resumen (F3; INV-F4T3B2-07 y 15)

**Dónde:** `WorkspaceBrowserViewModel.cs:221-222` y `:621-640`, `WorkspacePanelViewModel.cs:73` (y su documentación en `:58-61`).

**Qué cambia:**
- Antes, `PurgeDeadSessions` solo recorría `KnownSessions`, que está vacío al construir el panel.
- Ahora el `Present` del constructor consume las colas y la purga recorre también las sesiones de los vetos recién tomados.

**Escenario:** el panel se construye con el puerto del piloto y con un veto en la cola del host. Entonces `IWorkspaceBrowserSummaryPort.IsSessionAlive` se llama durante la construcción.

**Por qué importa:**
- La documentación cerrada de F3 del parámetro `browserSummary` dice «never when the panel is built». La prueba de F3 `BuildingThePanelAsksNeitherTheSummaryPortNorTheNavigationPort` no lo detecta porque se construye sin el puerto del piloto.
- El `Present` del constructor no está contenido como `Query`. Si `IsSessionAlive` lanzara, el constructor del VM del panel lanzaría.

En el Plugin esa consulta es una búsqueda en memoria (`nativeInstanceOf`), así que el riesgo real es bajo.

**Solución:** documentarlo junto a la frase que ya se corrigió para `pilotEdit`, o aplazar la purga de vetos fuera de la primera presentación.

**Relacionado:**
- La documentación de `DismissNotice` (`:516`) dice «It asks nothing of the port». En realidad `Publish()` vuelve a leer `GetField` por cada pestaña, solo memoria, y la prueba lo admite.

### M-4 (MENOR). Punto ciego de las guardas de llamadas: una llamada sin calificar tras `=>` o `?` cuenta como declaración

**Dónde:** `tests/RackCad.Tests/Workspace/WorkspacePilotCloseSyncGuardTests.cs:43-48` (`IsDeclaration`), usado por `CallCount` (`:33-40`) y por todas las guardas de llamadas.

**Por qué falla:** la expresión `([\w>\]\?]+)$` acepta `>` y `?` como final de un «tipo». Con `before = "... =>"` captura `>`, y la llamada se toma por la declaración del miembro.

**Mutación que pasa sin detectarse:** `private void RefreshNotices() => TakeNotices();`, llamada desde un gesto:
- `CallCount(code, "TakeNotices")` sigue dando 1 (`:148`);
- `CallsOutside(...)` no la ve.

Ocurre lo mismo con un ternario (`c ? Wrapper() : …`) y con una comparación `a > Name(...)`. Las autopruebas (`:178-192`) no cubren la forma `=>`.

Hoy el código es correcto. El problema es que la guarda que fija INV-F4T3B2-07 («TakeNotices, llamado solo desde Present») no es tan estricta como dice su comentario («an unqualified call that is not the declaration»).

### M-5 (MENOR). Varias guardas nuevas no tienen autoprueba en negativo; la de la forma pública solo comprueba presencia (§4 del paquete)

El §4 pide que cada guarda tenga su autoprueba en negativo.

**Guardas sin autoprueba propia:**
- `TheBrowserViewModelPassesKeepActiveToApplySelectionContext` (`:196-203`);
- `ApplySelectionContextHasTheExactSignature…` (`:205-213`);
- `BrowserSessionKeepsTheVocabulary…` (`:215-225`);
- `TheBrowserViewModelGainsExactlyThePublicMembers…` (`:229-255`);
- `ThePanelViewModelOnlyChangesTheDocumentationOfPilotEdit` (`:288-299`);
- `EveryNewHandlerDelegatesInsideContained…` (`:421-438`);
- `TheUiOfTheTaskReachesNeither…` (`:468-485`).

**La guarda de la forma pública:** se llama «GainsExactly…», pero solo comprueba que cada firma aparece una vez. Un miembro público adicional no la haría fallar, así que no fija el «exactamente» de INV-F4T3B2-12. Lo comprobé a mano: el GREEN no añade ningún miembro público de más.

**Forma:** en `:221` hay dos sentencias `Assert.DoesNotMatch` en una sola línea.

### M-6 (MENOR). Dos guardas fijan para todo el panel más de lo que pide esta tarea

**Dónde:**
- `WorkspacePilotCloseSyncGuardTests.cs:450-452` prohíbe:
  - `KeyUp` en **todo** el XAML;
  - exige **exactamente un** `PreviewKeyDown=` en todo el XAML;
  - prohíbe `e.SystemKey`, `e.ImeProcessedKey`, `e.IsDown` y `e.IsToggled` en **todo** el code-behind.
- `:389` prohíbe `IsDefault` e `IsCancel` en **todo** el XAML del panel.

**Qué pide el contrato:**
- N-3 se refiere a la caja del campo. La prueba existente de F4-T3B1 (H-08) solo fijaba que **la caja** no tiene `KeyUp`.
- INV-F4T3B2-03 y 12 hablan de los **botones nuevos**.

**Por qué importa:** F4-T3C hereda «la integración modal y de foco que quede de F4-T3». Por ejemplo, un Esc sobre la confirmación en línea o un manejador de teclado en otro control tendría que modificar esta guarda, aunque el cambio no tocara la caja.

**Lo que no bloquea a F4-T3C:**
- Las prohibiciones de foco y diálogos de `:468-485` duplican la guarda existente G-05 (`WorkspacePilotSurfaceGuardTests`), que ya cubre toda la UI. No son nuevas, pero F4-T3C tendrá que tocar dos guardas en lugar de una.
- Ninguna guarda fija para siempre lo que F4-T3C debe hacer:
  - el comando `RACKPANELDESCARTAR` vive en el Plugin y no se restringe;
  - el texto del aviso con la vía del comando no lo fija ninguna guarda de fuente;
  - el endurecimiento de `GetRacksAtRisk` cabe en el host y en los dos miembros permitidos del VM.
- La aserción `DoesNotContain("RACKPANELDESCARTAR")` de `WorkspacePilotCloseSyncTests.cs:785` es una prueba de comportamiento de la desviación transitoria de esta tarea, redundante con el `Assert.Equal` exacto de `:780-782`. F4-T3C deberá declararla como cambio de prueba existente.

### M-7 (MENOR). «Nunca ContainFailure» no está cubierto por ninguna prueba ni guarda (INV-F4T3B2-12)

**Qué no se comprueba:**
- Ninguna prueba nueva afirma `SurfaceStatusText` ni el estado de la superficie tras una excepción de un gesto de F4-T3B2.
- Ninguna guarda prohíbe `ContainFailure` en los cuerpos de `CloseTab`, `DiscardField`, `RequestDocumentDiscard`, `ConfirmDiscard`, `CancelDiscard` y `DismissNotice`.

**Hoy es correcto:** las excepciones del puerto se contienen antes de llegar a `Interact`, y `ContainFailure` solo es alcanzable por fallos que no son del puerto. Pero una implementación plausible incorrecta, «aviso + ContainFailure», pasaría todas las pruebas. Las que miran `HasError` solo ven el Error del panel, no el de la superficie.

## Lo que comprobé y está correcto

### Alcance y regla RED/GREEN

- Los 13 archivos son exactamente el AllowedWriteScope: 3 nuevos (A) y 10 modificados (M). Ninguno cae en el ForbiddenWriteScope.
- El GREEN solo toca `src/` (5 archivos) y no cambia `tests/`.
- El RED es un esqueleto compilable e inerte que no lanza:
  - miembros vacíos o falsos;
  - `ShowDiscardButton => false`;
  - `keepActive` ignorado;
  - XAML completo con nombres, enlaces y manejadores;
  - el doble completo con N-1.
- Revisé las pruebas nuevas contra el RED: las que fallan lo hacen por aserción, y no hay `NullReference` ni una excepción antes de la aserción.
- Los trailers son de Claude.
- Cambios a pruebas existentes, hunk por hunk:
  - `WorkspacePilotFieldViewTests.cs`: un hunk, P-01 exacto, que excluye `TakeLossNotices` y `TakeCloseNotices`;
  - `WorkspacePilotFieldViewModelTests.cs`: un hunk, solo la mitad Kept de `T06`, P-05;
  - `WorkspaceBrowserSessionTests.cs`: solo pruebas añadidas al final;
  - el doble: D-T3B2-08 y N-1. Las cinco operaciones llaman a `ThrowIfAsked` y las respuestas por defecto no cambian.

### Oráculo crítico

- Ningún camino de cerrar o descartar llama a `TryBeginEdit`, `TryReload`, `TryCheckNow`, `CrossBoundary`, `SetPendingText` ni `CommitField`, ni escribe.
- Cerrar la pestaña activa no cruza ninguna frontera. La única lectura posterior es el `ReadSummary` de F3, a través de `Interact`.
- En las líneas añadidas a `src/` no aparece ninguno de estos: `+=`, `event`, `Dispatcher`, `Timer`, `Task`, `async`, API de foco, `Registry`, `RackLog`, `MessageBox`, `ShowDialog`, `Window`, `EnterPanelModal`, `IsDefault` ni `IsCancel`.
- Las cuentas de G-03 se mantienen: `.IsSessionAlive(` 1, `.ReadSummary(` 1 y `BrowserSessionRegistry` 1.

### Tabla de cierre (D-T3B2-01, INV-02 a 05)

- Sin borrador: cierra sin descarte.
- No protegida: llama a `DiscardRack` y cierra siempre. Si no es `Discarded` o lanza, avisa con «Se cerró la pestaña; su borrador limpio quedó en memoria: motivo», sin `Field.Fail`.
- Protegida, también si `GetField` lanza: pide confirmación y no cierra.
- «Cancelar» solo hace `BeginGesture` y `RemoveConfirmation`, sin llamadas al puerto. La prueba T13 exige `Calls` vacío.
- «Descartar» llama a `DiscardRack(confirmSession, confirmRack)`. Solo con `Discarded` cierra y llama a `ForgetRisk`; con otro resultado avisa y no cierra.
- La confirmación es única y de tres tipos. Se sustituye con `Ask` y se anula en `ValidateConfirmation` en estos casos:
  - cambia la sesión;
  - su pestaña deja de existir;
  - la del campo, cambia la superficie viva.

### Descarte del documento (D-T3B2-03, INV-06)

- Se lee `GetRacksAtRisk` de la sesión presentada.
- Con las dos listas vacías, solo hay aviso y ningún efecto.
- Si hay riesgos, la confirmación nombra los racks por su título sin la marca (o por el `RackId`) y, aparte, los inciertos. Guarda las dos listas.
- `ConfirmDiscard` vuelve a leer y compara las dos listas en orden y con comparación ordinal (`SameList`). Si cambiaron, no descarta y muestra la lista nueva con su aviso; si la nueva está vacía, quita la confirmación y da el aviso sin efecto.
- Con `Discarded`:
  - el aviso nombra los confirmados y, aparte, el número de limpios retirados;
  - se quita el veto de esa sesión;
  - se reconstruye y se llama a `Field.Rematerialize`;
  - no se cierra ninguna pestaña.
- Las pruebas discriminan una TOCTOU que no compara, que compara como conjunto, que ignora mayúsculas o que ignora la lista de inciertos.

### Avisos (D-T3B2-04, INV-07 y 08)

- `TakeCloseNotices` y `TakeLossNotices` se llaman solo en el método privado `TakeNotices`, cada una en su `try`. `TakeNotices` solo se llama desde `Present`. Ningún gesto vuelve a llamar a `Present`: `_tabChanged` solo vuelve a presentar la navegación.
- Un fallo deja el aviso del navegador después de la reconstrucción.
- Hay un veto por sesión, que se sustituye en su sitio, y se clasifica al mostrarlo:
  - el texto del veto es el exacto del paquete;
  - no menciona Actualizar, Aplicar ni el comando.
- La pérdida da los RackId y los inciertos con su motivo, y solo se quita con «Entendido».
- La purga del veto está en el mismo bucle de `PurgeDeadSessions`, con una sola llamada textual a `IsSessionAlive`.
- Tras un `DiscardRack` con `Discarded`, el rack sale del veto, y la fila se quita si queda vacía y no es incierta.
- Las filas conservan su instancia por valor.

### Retención (D-T3B2-05, INV-09 y 10)

- `ApplySelectionContext(SelectionContext context, bool keepActive = false)` cumple las restricciones:
  - sin sobrecarga, sin valores nuevos, sin tipos nuevos y con vocabulario neutro;
  - consume el contexto antes de retener;
  - nunca reemplaza la activa;
  - las ramas `None`, `Replaced` y `Created` son las de la tabla.
- El VM pasa `keepActive` solo con el puerto del piloto y verdadero si hay `PendingText` o si `GetField` lanza. No lo pasa por `Error` ni `Undecidable` sin pendiente.
- El aviso sigue la regla del contexto anterior por sesión.
- Una pestaña solo dirty cruza `Dematerialize` como en F4-T3B1.

### Campo en Error (INV-11)

- `ShowDiscardButton = ShowReloadButton && view.Exists`.
- `DiscardField` sigue la regla sin cerrar la pestaña.
- `Field.Fail` solo se aplica con la pestaña abierta y viva (`FailFieldIfLive`) y nunca en el cierre limpio.

### Forma de la UI (INV-12)

- Los miembros públicos y el record son exactamente los del paquete, en el archivo del VM.
- Los nombres del XAML, los contenidos y las automatizaciones coinciden con el paquete.
- Los elementos nuevos están entre la tira y la superficie y fuera de `BrowserTabStrip`, sin tamaños fijos ni `ContentControl`.
- Los manejadores son `=> Contained(() => …)`.
- La superficie sigue con un solo «Editar», un «Recargar», un «Volver a leer» y un «Descartar».

### N-1 y N-3 (INV-13)

- N-1: el doble responde `Kept` con `CanEdit` falso en una frontera bloqueante, y T06 recorre `Kept`.
- N-3: solo usa `e.Key` y `e.IsRepeat`, sin `KeyUp`. Una pulsación no repetida borra el recuerdo. Enter y Esc conservan su regla. Hay pruebas en negativo.

### Sin puerto (INV-01)

`CloseTab` vuelve a ser el de F3. `Present` no consume ni retiene. Los gestos nuevos no hacen nada.

### Cobertura 11-14 y la lista del §4

- La prueba 11 usa el oráculo de conjunto exacto (`AssertOnly`), con cero lecturas de la intención y cero llamadas a la navegación.
- Las pruebas 12-14 están a nivel de VM y de vista.
- Están todos los casos de la lista, incluidas la contención del cierre activo, la purga, el consumo solo en `Present` y el fallo de las colas.

## Riesgos para el Smoke-2 con AutoCAD (no son defectos)

1. **El compromiso por `LostFocus` antes de «Cerrar»** (declarado): un texto válido llega aplicado a la confirmación, y «Cancelar» lo deja aplicado.
   - Ese compromiso dispara `OnFieldChanged` y luego `Rebuild`.
   - Si la tira cambia de valor durante la pulsación (una preview fijada o un título), el clic en «Cerrar» se pierde. Es el clic perdido declarado.
2. **El foco tras los gestos:**
   - Al cerrar la pestaña cuyo botón tiene el foco, su contenedor se destruye.
   - Al confirmar, el panel de confirmación se colapsa con el botón enfocado.
   - En la paleta de AutoCAD el foco puede ir a la línea de órdenes o quedar en la caja, que se reutiliza con K = 1. Una tecla posterior sobre la caja de la nueva superficie, en solo lectura, lanzaría el primer gesto de **otro** rack. Hay que comprobar dónde queda el foco tras «Cerrar», «Descartar» y «Cancelar».
3. **La confirmación puede quedar fuera de la vista:**
   - La confirmación y los avisos aparecen **encima** de la superficie.
   - Con la paleta baja y desplazada hasta la superficie, pulsar «Descartar» del campo en Error abre la confirmación arriba, fuera de la vista.
   - No hay `BringIntoView`, porque las API de foco y desplazamiento están prohibidas. El usuario puede creer que no pasó nada.
4. **N-3:** queda por comprobar que el `Handled` del túnel suprime el carácter de las repeticiones en la caja alojada en AutoCAD (declarado).
5. **El teclado sobre los botones nuevos:** Enter y Space sobre un botón enfocado de la confirmación (sin `IsDefault` ni `IsCancel`). Hay que ver si AutoCAD los intercepta hacia la línea de órdenes.
6. **El veto no emite `StateChanged`** (declarado): el aviso aparece en la presentación siguiente, normalmente al terminar el comando. Hay que comprobarlo con un intento real de cerrar el documento con un borrador protegido.
7. **M-3 en el host real:** abrir o recrear la paleta con un veto en cola. La construcción del panel llama a `IsSessionAlive` y no está contenida.
