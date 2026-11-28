# Revisión adversarial independiente: F4-T3B1-EDIT-SURFACE

- **Worktree:** el worktree de I-64, rama `architecture/workspace-persistente-rackcad`.
- **Rango:** `e0e0166b..5de2fab3`, con RED `f0637250` y GREEN `5de2fab3`. Son 19 archivos: 8 de `src` y 11 de `tests`.
- **Autoridades leídas desde el repositorio:**
  - el `gate-contract.json` de F4-T3B1: INV-F4T3B1-01..14, AllowedWriteScope, ForbiddenWriteScope y F4T3B1-C01..C08;
  - el paquete de F4-T3B (v3): §2 a §9;
  - el paquete de F4-T3: §2, D-T3-02 a D-T3-08 y §7 (R-B1..R-B12);
  - las órdenes del Owner `owner-f4t3-authorization.md`, `owner-f4t3a-close-decision.md` y `owner-f4t3b-decomposition-decision.md`;
  - la revisión independiente de T3A (menores 1, 2, 3, 5 y 7);
  - el código del host que toca el piloto: `ViewOfPilotDraft` y los motivos de `WorkspaceHostCore`, y `WorkspacePilotSupport`.
- **Método:** solo lectura, con `git show`, `git diff`, `git log`, `git grep` y lectura de archivos.
  - No ejecuté build ni pruebas, no verifiqué la CI y no abrí AutoCAD.
  - Que una guarda o una prueba pasa o falla, en el GREEN o en el RED, lo deduzco de leer su código contra el texto del producto.
  - No leí la revisión directa de la sesión, para no anclarme en ella.

## Veredicto

**NO ACEPTABLE** tal como está: 1 BLOQUEANTE, 0 IMPORTANTE y 7 MENOR.

- **El BLOQUEANTE (hallazgo 1) es un defecto de diseño heredado literalmente del contrato.** El Worker implementó al pie de la letra la regla de teclado de INV-F4T3B1-04 y la prueba (f) de D-T3B1-12. Con la semántica de entrada de WPF, y con la premisa que el propio paquete usa en D-T3B1-03, esa regla impide escribir caracteres en la caja editable.
- **Corregirlo exige una decisión del Coordinator** sobre el texto de INV-F4T3B1-04 y de la prueba (f), y quizá sobre G-08.
- **Si se resuelve ese punto,** la entrega sería ACEPTABLE CON MENORES. El resto cumple el contrato:
  - el oráculo crítico;
  - la regla de edición;
  - las transiciones;
  - los cambios declarados de guardas;
  - los arrastres.

## Hallazgos

### 1. BLOQUEANTE — El `KeyDown` de burbuja de la caja editable impide escribir: anula el `TextInput` de toda tecla de texto

**Dónde.**
- `src/RackCad.UI/Workspace/WorkspacePanelView.xaml.cs:129-132`:
  ```csharp
  private void OnBrowserFieldBoxKeyDown(object sender, KeyEventArgs e)
  {
      if (_viewModel.Browser.Field.IsEditable && e.Key != Key.Tab) e.Handled = true;
  }
  ```
- Va enganchado en `WorkspacePanelView.xaml:153`, `KeyDown="OnBrowserFieldBoxKeyDown"`.
- Lo fija la prueba `KeysThatTheEditorDidNotConsumeStayInThePanelWithAnEditableBoxExceptTab` (`tests/RackCad.UI.Tests/Workspace/WorkspacePilotFieldViewTests.cs:305-316`), que afirma que `Key.Z` sin modificador queda `Handled`.

**Mecanismo.**
- En WPF, los caracteres no se escriben en el `KeyDown`: llegan a la caja por `TextInput`, que nace del `WM_CHAR`.
- El `TextBox` consume en el `KeyDown`, por sus comandos de edición, solo las teclas de edición: Retroceso, Supr, flechas, Ctrl+V y similares.
- Una tecla de texto (`D0`-`D9`, `OemComma`, `OemPeriod`, letras, `NumPad*`) llega sin consumir al manejador de instancia, y este la marca `Handled`.
- La documentación de WPF (Input Overview, «Text Input») dice que el `TextInput` se genera solo si los eventos `KeyDown` y `KeyUp` no quedaron manejados. Con el `KeyDown` manejado, el `WM_CHAR` se descarta y el carácter no llega nunca a la caja.
- **El paquete se apoya en esa misma regla:** en D-T3B1-03 dice, de la pulsación en `PreviewKeyDown`, «Al marcarla no se genera TextInput, así que la tecla no se aplica a la caja». La regla de D-T3B1-04 aplica el mismo mecanismo a todas las teclas no consumidas de la caja editable, también a las de texto. El paquete se contradice.

**Escenario que falla en el host real.**
1. Una pestaña fijada con su borrador abierto: la caja es editable y muestra «1:50».
2. El usuario pulsa `2`.
3. `PreviewKeyDown` no la marca: no es Enter ni Esc.
4. El `TextBox` no la consume en el `KeyDown`, y `OnBrowserFieldBoxKeyDown` la marca `Handled`.
5. No se genera `TextInput`: la caja no cambia y no hay `SetPendingText`.

**Qué queda y qué se pierde.**
- Solo funcionan Retroceso, Supr, pegar, cortar y deshacer del propio `TextBox`.
- No se puede teclear un valor nuevo de `AnnotationScale`.
- La cobertura 1-8 del Owner no se puede ejecutar con el teclado en Smoke-2.

**Por qué la CI verde no lo ve.**
- Las pruebas de vista asignan `box.Text = ...` directamente: (a), (c), `Editable()` y las demás.
- La prueba (f) lanza `KeyDown` con `RaiseEvent`, que nunca pasa por la generación de `TextInput`.
- Ninguna prueba ejercita la cadena tecla → `TextInput` → binding.

**Qué viola.**
- El objetivo del contrato: «la edición con texto pendiente».
- INV-F4T3B1-04: «cada cambio de texto del usuario llama a SetPendingText». El usuario no puede producir ese cambio.
- La cobertura 1 de la orden del Owner: «valid text -> internal draft update».
- La regla aplicada es la letra de INV-F4T3B1-04: «un KeyDown de burbuja marca Handled toda tecla no consumida por el editor salvo Tab». Por eso la corrección es del contrato y no solo del Worker.

**Confianza.**
- Es alta, porque se basa en la semántica documentada de WPF y en la propia premisa de D-T3B1-03.
- El único escenario en que no se manifestaría es un bucle de mensajes del host que traduzca el `WM_KEYDOWN` antes de entregarlo y no consulte el resultado de WPF. Eso no es lo que hace un host que integra el teclado de WPF, como `ShowModelessWindow` o las paletas.

**Direcciones de corrección** (decisión del Coordinator):
- Que el `KeyDown` de burbuja deje pasar las teclas que producen texto, para que lleguen al `TextInput`. Las combinaciones con Ctrl o Alt (Ctrl+Z sin nada que deshacer, Ctrl+Y y similares) se retendrían sin leer `Keyboard`, por ejemplo con `KeyBinding` en el XAML, o con un cambio declarado de G-08 que permita leer los modificadores.
- Cambiar la prueba (f) para que discrimine: `KeyDown` de `D5` con la caja editable → **no** `Handled`; `Delete` sin consumir y Ctrl+Z → `Handled`.
- Si el Coordinator prefiere no corregir ahora, el **primer paso de Smoke-2** debe ser teclear un dígito en la caja editable. Con el diagnóstico de arriba, lo esperado es que falle.

### 2. MENOR — El menor 2 de T3A queda como un techo numérico, no como una cota que nombra los archivos (INV-F4T3B1-11, D-1, I-1)

**Dónde.** `tests/RackCad.Tests/Workspace/WorkspacePilotEditGuardTests.cs:873-877`: `Assert.True(Src.PluginFiles().Count <= 9)` y `Assert.True(Src.Files(DraftsFolder).Count <= 5)`. El comentario dice: «the ceiling that this task and the next ones must not pass».

**Problema.**
- INV-F4T3B1-11 pide que la guarda «nombra como cota los 5 archivos de Drafts y los 9 de Plugin/Workspace».
- La recomendación de T3A remitía al modelo de `MissingDraftFiles` (`WorkspaceLifecycleGuardTests.cs:661-684`), una cota **inferior** que nombra cada archivo y «does not fix their number».
- La entrega no nombra ningún archivo y deja un techo numérico.

**Escenario.** Es el mismo que motivó el menor 2 de T3A. Una tarea futura que añada un archivo legítimo a `Drafts/` o a `Plugin/Workspace` tiene que tocar esta guarda de T3A, ahora también por la letra del comentario («the next ones»). La regla I-1 limita las guardas a la tarea, y este comentario ata a las tareas siguientes.

**Lo que sí se cumple.** Se quitaron los 16/16/16 duplicados del puente.

### 3. MENOR — Las guardas nuevas prohíben lo que es de T4 y fijan conjuntos exactos de archivo entero (I-1)

Están en `tests/RackCad.Tests/Workspace/WorkspacePilotSurfaceGuardTests.cs`.

- **`TryCheckNow` queda prohibido en toda la UI** (`:81-82`).
  - La cabecera (`:13-16`) afirma que ninguna guarda «forbids what belongs to … F4-T4».
  - El flujo de obsoleto o conflicto, «Comprobar ahora», es de T4 (D-T3B1-09, R-B10). T4 tendrá que cambiar esta guarda.
  - INV-F4T3B1-09 solo dice que T3B1 no lo usa.
- **El XAML entero puede contener un solo `<TextBox`** (`:214`). Es un recuento exacto sobre todo el archivo, y cualquier campo futuro del panel lo rompe.
- **La paleta tiene una lista blanca cerrada de miembros de `_controller.`** (`:229-235`), sobre todo el archivo. Funciona como un conjunto exacto: «la paleta pasa _controller.PilotEdit y nada más nuevo» se podía expresar en negativo, con el recuento de `_controller.PilotEdit` y la ausencia de tipos del puerto, que la guarda ya incluye.

Ninguno de los tres bloquea T3B2, pero contradicen la regla I-1 de D-T3B1-12: «limitadas a T3B1, en negativo y por cuerpo, sin conjuntos exactos de archivo entero».

### 4. MENOR — Los gestos sobre la caja en solo lectura no dependen del estado: lecturas repetidas del DWG y lecturas sin efecto

**Dónde.**
- La vista: `WorkspacePanelView.xaml.cs:88-92`, el clic, y `:113-124`, la tecla.
- El campo: `WorkspacePilotFieldViewModel.cs:180-228`, `BeginEdit`, y `:333-347`, la editabilidad y los botones.

**Lecturas repetidas.**
- Con la caja en solo lectura, todo clic y toda tecla que no sea un modificador, Tab o Esc lanza `BeginFieldEdit`. Cuentan también las flechas, Inicio y Fin, las teclas F, Bloq Mayús y la **repetición automática**, porque `e.IsRepeat` no se filtra.
- Sin borrador y con un `NotAdmitted` previo, o con un Limpio obsoleto cuya recarga no se admitió, cada una de esas pulsaciones es un `ReadIntent` nuevo del DWG.
- Mantener pulsada una flecha sobre una caja «No editable aquí» relee el dibujo decenas de veces.
- D-T3B1-03 dice: «El gesto lee a lo sumo una vez por pestaña y estado».

**Lecturas sin efecto.**
- En el estado Error por un fallo contenido (`_failure`), el botón «Editar» se oculta, pero el clic y la tecla siguen lanzando la intención.
- `BeginEdit` no limpia `_failure`, y `EditableOf` lo exige nulo. Así, una lectura que crea el borrador (`Admitted`) deja la caja en Error y en solo lectura hasta «Recargar». Es una lectura del DWG que no se ve.
- D-T3B1-08 dice que Error es solo lectura con «Recargar».

**Doble clic sobre la pestaña activa con la caja editable.** `BeginFieldEditFromTab` no comprueba la editabilidad. El gesto da `AlreadyOpen` sin leer, pero su `ClearNotices` borra un aviso vigente, por ejemplo el de un `TabChange` abortado.

**Por qué es MENOR.** No hay escritura y todas las lecturas salen de un gesto: el oráculo y INV-F4T3B1-09 se cumplen.

### 5. MENOR — Huecos de discriminación en las pruebas de UI

**(a) Prueba 9 incompleta.**
- El gancho que comprueba, dentro de la lectura, que la pestaña ya no es preview solo se usa con «Editar» (VM `:344-362` y vista `:121-144`) y con el doble clic (VM `:346`).
- El clic (vista `:146-162`) y la pulsación (vista `:164-183`) no lo comprueban.
- Una vista que, en esos dos gestos, fijara **después** de la lectura pasaría: al final la pestaña está fijada y la caja es editable.
- D-T3B1-12, prueba 9: «con "Editar", el clic, la pulsación y el doble clic».

**(b) El «no fija» del doble clic abortado es vacuo.**
- En `AnAbortedActivationDoesNotPinNorAskForTheIntentionWithTheDoubleClickOfThatTab` (VM `:708-724`) y en `TheDoubleClickOnATabThatIsNotActiveAfterTheClicksCallsNothing` (vista `:220-247`), R-2 ya estaba fijada antes del gesto.
- La aserción `Assert.False(...IsPreview)` es cierta de antemano. Solo discrimina el `Assert.Empty(Reads())`.

**(c) El doble no aplica el `Proceeded` de `TabChange`.**
- `WorkspacePilotEditPortDouble.CrossBoundary` (`:183-187`) responde `Proceeded, StateChanged true` ante un pendiente válido, pero no lo aplica ni lo limpia.
- El host sí lo hace (D-T3-02: «válido → Proceeded, aplicado, con el pendiente limpio»).
- `T06_ComingBackToTheTabShowsTheExactPendingTextOfTheDraft` (VM `:288-303`) vuelve a R-1 con un pendiente válido que «sobrevivió» a un `TabChange`. Con el puerto real ese camino no existe: prueba el renderizado desde `GetField`, pero no un camino real de la prueba 6.

**(d) `Dematerialize` con un borrador limpio.**
- La prueba de `Dematerialize` (VM `:728-742`) usa un borrador **limpio**.
- La lista «Además» pide la pestaña activa «solo dirty (sin pendiente)», que es el camino que R-B6 conserva en T3B2.

**(e) La reentrada no aísla el rack.**
- La prueba (VM `:637-658`) hace el cambio de pestaña y el de documento en el mismo gancho, así que la sesión siempre cambia.
- Una comprobación de identidad que mirara solo la sesión pasaría. El producto compara las dos (`IsMine`).

**(f) Pruebas que pasan contra el esqueleto del RED.** Por mi lectura:
- VM:
  - `TheFieldIsNeverNull…`;
  - `ThePanelPassesThePilotPort…`;
  - `AnotherClickOnTheActiveTab…`;
  - `ActivatingATabThatDoesNotExist…`;
  - `ASelectionThatLeavesATabWithoutADraft…`;
  - `CommitFieldIsNotCalledWithoutPendingText…`;
  - `PresentAndRefreshNeverCallTheThreeReads`;
  - **`T15_…`**.
- Vista:
  - `TheFieldPanelIsShownOnlyWithThePilotPort`;
  - `OnAReadOnlyBoxEscIsHandled…`, porque el code-behind ya es el definitivo en el RED;
  - `D_NoNewControl…`;
  - `ThereIsOneBox…`.

Son negativas o estructurales y discriminan contra defectos plausibles. Pero el mapa evidencia-por-invariante no debe presentarlas, y en particular la prueba 15 del Owner, como RED → GREEN.

**(g) La prueba 16 no edita.** T16 (`:1082-1100`) no tiene pendiente: «K = 1 mientras se edita» se prueba sin edición.

### 6. MENOR — Los textos del campo mezclan español e inglés, y el Error de un borrador no tiene motivo (INV-F4T3B1-02, D-T3B1-08)

**Textos mezclados.**
- `RefusalOf`, `Describe`, `StateOf` y `AbortText` (`WorkspacePilotFieldViewModel.cs:355-369`, `:414-438`, y `WorkspaceBrowserViewModel.cs:326-334`) concatenan los motivos del host, que están en inglés. Salen textos como:
  - «El texto no se aplicó: el campo no se puede editar ahora. The field cannot be edited now.»;
  - «En espera: A command is in progress.»;
  - «Limpio: The rack has no annotation scale.»
- INV-F4T3B1-02 pide el aviso «en español y con su motivo».

**Error sin motivo.**
- Para un borrador en `PilotFieldState.Error`, `ViewOfPilotDraft` no lleva ningún motivo de error: `CommittedTextReason` solo describe el valor. El campo muestra «Error: sin detalle.».
- D-T3B1-08 pide «con el motivo». Es una limitación del puerto de T3A, pero T3B1 la hace visible.

Lo dejo para T3C, para Smoke-2 o para la decisión del Owner.

### 7. MENOR — Frontera con T3B2: dos pruebas de T3B1 fijan que la presentación no consume los avisos

**Dónde.**
- `T15_ChangingTabsCallsNoNavigationAndOnlyTheSixOperationsOfT3B1AreUsed` (VM `:1038-1080`) afirma cero `TakeLossNotices` y `TakeCloseNotices` en un escenario con varios `Refresh`.
- `ThePanelPassesThePilotPortToTheBrowserAndBuildingItCallsNothing` (VM `:92-104`) afirma `Assert.Empty(pilot.Calls)` tras construir el panel, que presenta un snapshot inicial.

**Problema.**
- R-B2-3 (§6) permite que T3B2 consuma los avisos «desde la presentación del panel o desde un gesto».
- Si T3B2 elige la presentación, tendrá que cambiar estas dos pruebas de T3B1.
- La prueba 15 la fijó el paquete, así que no es un defecto del Worker. Conviene que el paquete de T3B2 lo declare.

El resto de lo que necesita el §6 queda libre (ver «Lo correcto»).

### 8. MENOR — Un fallo al cruzar la frontera de la superficie anterior se muestra en el campo de la nueva

**Dónde.** `WorkspaceBrowserViewModel.cs:339-356`.

**Problema.**
- `CrossObservedBoundaries` corre después del `Rebuild`, cuando la superficie ya es la nueva.
- Su `catch` llama a `Field.Fail(ex)`, que pone en Error el campo de **B** por un fallo de la frontera `DocumentChange` o `Dematerialize` de **A**.
- El usuario ve el Error en un rack sano, y «Recargar» lo quita.

**Por qué es MENOR.** Hoy es inalcanzable con el puerto real, porque las fronteras no bloqueantes responden `Kept` sin tocar el borrador y el host contiene sus excepciones.

## Lo que comprobé y encontré correcto

### Alcance

- Los 19 archivos son exactamente el AllowedWriteScope, y nada toca el ForbiddenWriteScope.
- No hay archivos nuevos en `Plugin/Workspace` ni en `Drafts/`.
- El GREEN (`f0637250..5de2fab3`) solo toca el VM del navegador y el VM del campo, y ningún archivo de `tests/`.
- `Application` solo cambia en documentación, y el Plugin solo en el quinto argumento de la paleta.
- Los dos commits llevan el trailer de Claude.

### Oráculo crítico (INV-F4T3B1-09, 10 y 14)

- Las tres lecturas solo aparecen en `WorkspacePilotFieldViewModel.BeginEdit`.
- `BeginEdit` solo se alcanza desde `Browser.BeginFieldEdit`, que solo llaman los gestos: el botón «Editar», `PreviewMouseLeftButtonDown`, `PreviewKeyDown` y `MouseDoubleClick`.
- `Present`, `Show`, `Clear`, `RebuildTabs`, `SyncSurface`, `StartSurface`, `ClearSurface`, `Materialize`, `Refresh`, los constructores y los setters solo usan `GetField` y fronteras no bloqueantes.
- `TryCheckNow` no se usa.
- Ninguna operación del puerto que llama la UI escribe.
- Cambiar de pestaña no llama a la navegación: T15 lo afirma sobre el doble de la navegación.
- No hay `+=`, eventos, `Dispatcher`, `Timer`, `Task`, `async` ni API de foco nuevos. Los manejadores nuevos se declaran en el XAML, como fija D-T3B1-04.
- No hay subdiálogo ni `EnterPanelModal`.

### Regla de edición y primer gesto (INV-F4T3B1-02 y 03)

- `EditableOf` exige un borrador con `CanEdit`, una pestaña no preview, que no sea un Limpio obsoleto y que no haya un fallo contenido.
- `IsPreview` se recalcula al final de cada `Rebuild` y de `ReloadSummary`, después de `RebuildTabs`. Una pestaña recién fijada, por `PinTab` o por la fijación automática, deja de tratarse como preview en el mismo ciclo.
- El primer gesto:
  - fija antes de la intención (`BeginFieldEdit` → `PinTab` → `Field.BeginEdit`);
  - pide `new EditIntentRequest(session, rack, null, null)`;
  - pide `TryReload` solo para un Limpio obsoleto;
  - comprueba `IsMine` tras la lectura y, si la superficie cambió, rematerializa y avisa;
  - deja el aviso o el diagnóstico de `NotAdmitted`, y el aviso del `NotAdmitted` de la recarga.
- La tira fija una preview cuando su borrador está protegido.
- Los avisos y el diagnóstico se conservan en la misma superficie y se limpian con un gesto nuevo o con un cambio de superficie.
- Sin texto comprometido, la caja queda vacía y el motivo va en el estado.
- Una observación: «Listo para editar» solo aparece en un Limpio sin pendiente. Sobre un borrador Modificado editable con `AlreadyOpen` se ve «Modificado». Es compatible con «solo si queda editable», aunque D-T3B1-03 lo redacta como suficiente.

### Mecánica WPF (INV-F4T3B1-04), salvo el hallazgo 1

- El binding es exacto: `TwoWay` con `UpdateSourceTrigger=PropertyChanged`.
- El setter es la única entrada de edición:
  - sin campo editable, no llama a nada y notifica para restaurar;
  - con un rechazo, `NotEditable` o `Busy`, restaura desde `GetField` y avisa;
  - contiene sus excepciones en el Error del campo.
- La presentación cambia `_text` y notifica, sin llamar nunca a `SetPendingText`. La guarda por cuerpo y las pruebas (b) lo cubren.
- No hay `TextChanged`, `SourceUpdated`, `TargetUpdated` ni asignaciones a `.Text` en el code-behind ni en el XAML.
- `CommitPending` exige pendiente y caja editable.
- Enter y Esc quedan `Handled` en el `PreviewKeyDown`. No se usa `e.KeyboardDevice`.
- La igualdad del setter con `_text` evita un dirty falso si el binding reenvía el mismo valor.

### Transiciones, «Nombre *», estados y contención (INV-F4T3B1-05 a 08)

- **`TabChange`:**
  - solo hacia un destino que existe y es distinto de la activa, con pendiente, y antes de `Activate`;
  - `Kept` y `Proceeded` activan;
  - `Aborted` y `Busy` no activan y avisan con `InvalidFields` o con `Reason`.
- **Fronteras no bloqueantes:**
  - `Dematerialize` solo en la ruta de `Present`, nunca tras `TabChange` ni tras `CloseTab`;
  - `DocumentChange` cuando cambia la sesión;
  - `CloseTab` no cambia.
- **«Nombre *»:** sale de `GetField(...).IsProtected`, sin tocar el record. La instancia de `Tabs` se conserva si nada cambia (T2-F6).
- **Estados:**
  - Applying, Unknown y Undecidable quedan en solo lectura con su texto;
  - Error, del borrador o de un fallo contenido, ofrece «Recargar», que solo rematerializa desde `GetField`, sin `TryReload`;
  - el panel no pasa a Error.
- **Sin el puerto del piloto:** el navegador es el de antes, sin llamadas ni marca. Las 248 pruebas de UI existentes no cambian: ningún archivo previo de `tests/RackCad.UI.Tests` aparece en el diff.

### Guardas y arrastres (INV-F4T3B1-11 y 12)

- G-01, G-02 y G-03 llevan el texto exacto declarado en los tres archivos, y sus comentarios nombran F4-T3B1.
- **G-04:**
  - el tramo es el elemento `BrowserSurface` completo, equilibrado con `<Border(?=[\s>])` no autocerrados y `</Border>`, sin contar `Border.Style`;
  - hay tres autopruebas: Border anidado, autocerrado y `Border.Style`;
  - es una cota, `>= 3`, más los tres botones nombrados, cada uno una vez con su manejador dentro de `Contained`;
  - conserva la lambda exacta de «Volver a leer» y el único `public void ReloadSummary()`;
  - añade un `.ReloadSummary(` único en el code-behind, que endurece y no debilita.
- **Menor 1:** el espía de escritura y sus aserciones se retiran, y el comentario cita la guarda por cuerpo.
- **Menor 3:**
  - (a) `Kept` sobre un borrador en Error con pendiente; Unknown y Applying no pueden tener pendiente, y el test lo explica;
  - (b) la marca «limpio conocido» se conserva tras las operaciones que no mutan;
  - (c) el `catch` externo de `GetField`, alcanzado con un copiador que devuelve `null`;
  - (d) la aserción entre literales, quitada.
- **Menores 5 y 7:** la documentación llega sin mover los anclajes de `WorkspaceEditIntent.cs` (la de `EditCheckStatus.Waiting` va después de `EditIntentStatus`).
- **N-5:** solo comentarios.
- No encontré ningún otro cambio en las guardas ni en las pruebas existentes.

### RED (INV-F4T3B1-13)

- El esqueleto es inerte y no lanza:
  - el VM del campo devuelve valores neutros;
  - los métodos nuevos del navegador están vacíos;
  - el cableado, el XAML y el code-behind ya son los definitivos.
- Por lectura, las guardas de Core pasan en el RED y las pruebas de comportamiento de UI fallan por aserción, salvo las del hallazgo 5(f).

### T3B2 (§6)

- `CloseTab` y `BrowserSession` no cambian.
- G-04 queda como cota.
- Ninguna prueba de T3B1 fija la selección con pendiente en la pestaña activa. Revisé todos los escenarios con pendiente: solo hay cambios de documento, `SelectionContext.None` o el mismo rack.
- La guarda de límites no prohíbe `DiscardRack`, `GetRacksAtRisk` ni las colas de avisos.
- Las salvedades son las de los hallazgos 3 y 7.

## Riesgos para el Smoke-2 con AutoCAD

1. **El hallazgo 1, primero.** Hay que teclear un dígito, una coma o un punto en la caja editable. Si se confirma, la cobertura 1-8 no se puede ejecutar con el teclado.
2. **Teclas que llegan a AutoCAD (D-01).**
   - Con la caja en solo lectura no se marcan Tab, los modificadores ni `Key.System`, así que Alt+tecla y F10 salen del panel.
   - Con la caja editable, `Key.System` sí queda `Handled`: mientras se edita se bloquean las teclas de sistema de AutoCAD, como Alt y F10.
   - Los botones de la tira y de la superficie no marcan nada. Si el foco queda en uno de ellos tras un clic, Supr o Ctrl+Z pueden llegar al dibujo.
3. **El foco tras el gesto.**
   - «Editar» y «Recargar» se colapsan al pulsarse: el foco del teclado deja el botón invisible y puede salir de la paleta, y la siguiente tecla iría a la línea de órdenes.
   - Tras el doble clic, el foco queda en el botón de la pestaña, no en la caja.
   - T3B1 no toca el foco (R-B11). Es un residual que hay que observar.
4. **`LostFocus`.**
   - Hay que confirmar que un clic en el dibujo no compromete, porque el foco es lógico, y que el pendiente queda visible con «*».
   - El clic de pestaña tras un compromiso que cambia «*» puede perderse: es la limitación declarada en D-T3B1-10.
5. **El doble clic.**
   - Sobre una pestaña no activa, el primer clic regenera la tira, porque cambia `IsActive`, y el segundo cae en un contenedor nuevo. Hay que confirmar que `MouseDoubleClick` llega.
6. **Reentrada.**
   - `ReadIntent` se lanza dentro de `PreviewMouseLeftButtonDown` o `PreviewKeyDown`, y puede bombear mensajes con el ratón capturado o con la repetición automática activa (hallazgo 4).
   - Hay que confirmar que una pulsación durante la lectura da el aviso visible de `Busy` o `Waiting` sin perder texto en silencio.
7. **La relectura del binding.** La restauración tras un rechazo depende de que WPF relea el origen cuando el setter notifica. La prueba (c) lo cubre en STA, y conviene verlo también con una pulsación o un pegado real.
8. **Los textos en inglés del host** dentro de los avisos y del estado (hallazgo 6).
