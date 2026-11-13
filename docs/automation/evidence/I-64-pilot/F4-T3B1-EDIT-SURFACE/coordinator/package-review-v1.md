# Revisión adversarial independiente — Paquete del contrato de F4-T3B1 (v1)

- **Paquete revisado:** el paquete `I-64-f4-t3b-gate-contract-package.md` (carpeta de trabajo de la sesión) (v1), con las cuatro órdenes del Owner de la misma carpeta.
- **Repositorio:** worktree de I-64, rama `architecture/workspace-persistente-rackcad`, HEAD `eb55606c` (solo documentación respecto de `18471988`).
- **Método:** solo lectura (`git grep`, `git diff`, lectura de archivos). No ejecuté build ni pruebas. Las afirmaciones sobre guardas salen de leer sus regex contra el texto real.

## Veredicto

**EMITIBLE TRAS CORRECCIONES.** 0 BLOQUEANTE, 10 IMPORTANTE, 11 MENOR.

La división B1/B2/C, el presupuesto, el orden, las reglas de cierre y de controles respetan las órdenes del Owner. Los hechos H-01..H-09 y G-01..G-14 son exactos en las líneas citadas, y no encontré ninguna guarda que **rompa** con lo que exigen D-T3B1-01..12 sin estar declarada. Los defectos son de diseño de la UI frente al puerto real de T3A (preview editable sin fijar, O-01, Error), de mecánica WPF no fijada, de la regla RED y de la forma de los cambios de guarda. Todos se corrigen en el texto del paquete, sin volver al Owner.

## Hallazgos

### I-1. IMPORTANTE — Una preview con borrador limpio queda editable: el primer gesto no fija antes de ensuciar

**Dónde.** §4 D-T3B1-02 («con un borrador abierto y `CanEdit`, la caja es editable»), D-T3B1-03 (solo fija automáticamente la preview con borrador **protegido**), D-T3B1-06 (cerrar una pestaña: sin cambio) y D-T3B1-08. Código: `WorkspaceHostCore.cs:873-874` (`CanEdit` no sabe nada de la preview), `BrowserSession.cs:78-113`.

**Camino.**
1. Preview R → «Editar» → `Pin` → `TryBeginEdit` → `Admitted`: borrador limpio.
2. «Cerrar» la pestaña de R. En T3B1 no llama a `DiscardRack`, y el borrador limpio se queda en el host.
3. Seleccionar R de nuevo en el dibujo → `ApplySelectionContext` crea una **preview** de R.
4. `RebuildTabs`: `IsProtected` es false, así que no fija. `Field.Show`: `Exists`, `Clean` y `CanEdit` → **caja editable** en una preview.
5. La primera tecla hace `SetPendingText`. La pestaña es dirty **antes** de fijarse; la fija después el `RebuildTabs` del `Action`.

**Problema.** Viola la orden del Owner («The first editing gesture must fix the preview BEFORE it can become a dirty editing session»), D-08 (`proposal-v5.md:388-389`) y R-B3. Además, R-B2-4 de T3B2 (retener con pendiente y reemplazar la preview «en segundo plano») supone que una pestaña con pendiente nunca es preview. Si fallara esa suposición, `_tabs[preview] = fresh` reemplazaría la pestaña **activa**, y `ActiveRackId` dejaría de nombrar una pestaña existente. El `DiscardRack` al cerrar de T3B2 no lo elimina del todo, porque puede responder `Rejected` o `Busy`.

**Corrección.** La caja solo es editable si la pestaña **no es preview**, además de `Exists` y `CanEdit`. Sobre una preview con borrador, `Show` deja la caja en solo lectura con «Editar». El primer gesto fija la pestaña y llama a `TryBeginEdit`, que responde `AlreadyOpen` sin leer (`WorkspaceHostCore.cs:307-308`). Hace falta una prueba de este camino (cerrar, reseleccionar, teclear). Conviene añadir a R-B2-4 «en modo retenido nunca se reemplaza la pestaña activa».

### I-2. IMPORTANTE — O-01 no se cumple: un Limpio «posiblemente obsoleto» queda editable, y el campo no se refresca al presentar

**Dónde.** D-T3B1-03 (la recarga perezosa se dispara con «el primer gesto sobre el campo») frente a D-T3B1-08 («`Clean` … con `CanEdit`: la caja editable»). D-T3B1-05 renderiza solo en `StartSurface`.

**Problema.**
- `IsPossiblyStale` es ortogonal a `CanEdit` (`WorkspaceHostCore.cs:873-874`, `:891`). Con la caja editable, el primer gesto es una tecla: `SetPendingText` deja el borrador en Dirty, y `NeedsLazyReload` exige `Status == Clean` (`DraftSession.cs:291`). La recarga perezosa ya no ocurre nunca, y se edita sobre una base marcada como obsoleta. Va contra O-01 (`proposal-v5.md:439`) y R-B3 (`I-64-f4-t3-gate-contract-package.md:410`).
- Los drenajes marcan los borradores mientras la superficie está viva (`WorkspaceHostCore.cs:173`). Como `Present` no vuelve a leer el campo cuando no cambia la pestaña, ni «posiblemente obsoleto» ni la solo lectura llegan a la vista.

**Corrección.**
- `Clean` con `IsPossiblyStale` → solo lectura, con «posiblemente obsoleto». Un gesto («Editar» o el clic en la caja) llama a `TryReload` y, si responde `Reloaded` o `NotNeeded`, la caja pasa a editable.
- Cada `Present`/`Refresh` vuelve a renderizar el estado del campo desde `GetField`, que es memoria. El texto de la caja no cambia, porque por el invariante coincide con el pendiente.
- Pruebas: la marca llega con la superficie viva, y la tecla sobre un obsoleto no llama a `SetPendingText`.

### I-3. IMPORTANTE — «Recargar borrador» con `TryReload` no hace lo que dicen el Freeze ni el paquete de T3, y la contención acaba en el Error del panel

**Dónde.** D-T3B1-08 («`Error` … botón «Recargar borrador» (`TryReload`, por gesto)»).

**Problema.**
- Con el puerto real, `TryReload` sobre un borrador en Error responde `NotNeeded`, porque solo recarga un Limpio marcado (`WorkspaceHostCore.cs:464-467`). Antes pasa por `PassGate` (`:459-462`), que con un comando activo responde `Waiting`, y es una de las tres operaciones de lectura (D-T3B1-09).
- El paquete de T3 resolvió el Error así: «Recargar rematerializa desde el borrador y Descartar llama a DiscardRack» (`I-64-f4-t3-gate-contract-package.md:438`, I-5). D-08 dice «una pestaña cuya superficie falla queda en Error, conserva su borrador y ofrece Recargar o Descartar» (`proposal-v5.md:396`).
- Hoy, un fallo de un gesto del campo lo contiene el `Contained` de la vista, que llama a `_viewModel.ShowError` (`WorkspacePanelView.xaml.cs:78-88`). Eso pone en Error **todo el panel** y deshabilita la navigación, cuando D-17 e INV-25 piden el Error de la pestaña afectada (`proposal-v5.md:597-600`).

**Corrección.**
- «Recargar» del Error = `Field.Show` desde `GetField` (memoria, sin `TryReload`).
- Los gestos del campo contienen sus fallos en el estado Error del campo o de la pestaña, como `Interact` → `ContainFailure` (`WorkspaceBrowserViewModel.cs:179-193`, `:306-321`). El `Contained` de la vista queda como última defensa.
- Pruebas: Error desde `GetField` (`PilotFieldState.Error`) y Error por una excepción del VM; Recargar no llama a ninguna lectura.

### I-4. IMPORTANTE — La mecánica WPF de la caja no está fijada, y las pruebas previstas no la discriminan

**Dónde.** D-T3B1-04 y D-T3B1-12 («mostrar nunca llama a `SetPendingText`», «el rechazo … restaura la caja»), sin decir a qué nivel.

**Problema.** Tres implementaciones plausibles rompen invariantes sin que las pruebas del VM lo vean:
- **`Text` con el `UpdateSourceTrigger` por defecto (`LostFocus`).** El borrador recibe el texto solo al salir. Un cambio de documento o de selección con el foco en la caja reemplaza el origen y pisa lo tecleado: **pérdida silenciosa**, contra D-T3-08.
- **Captura en `TextChanged` del code-behind.** `TextChanged` también salta cuando el binding presenta un valor: `Show` produciría `SetPendingText`, un **dirty falso**, contra D-T2-01 y O-16. La guarda nueva («`SetPendingText` solo se nombra en la entrada de edición») no lo impide.
- **Restaurar dentro del setter.** En .NET 8 funciona: un binding bidireccional relee el origen si este notifica durante la actualización. Pero solo una prueba con un `TextBox` real lo demuestra, y el cursor salta al inicio.

**Corrección.** Fijar en D-T3B1-04 la mecánica:
- `Text="{Binding Browser.Field.Text, Mode=TwoWay, UpdateSourceTrigger=PropertyChanged}"`;
- el setter público es la única entrada de edición del usuario, y la presentación usa un campo privado con `OnPropertyChanged`;
- se prohíbe capturar en `TextChanged` y asignar `TextBox.Text` desde el código.

Añadir a `WorkspacePilotFieldViewTests` (STA, con un `TextBox` real):
- (a) `box.Text = "x"` sin perder el foco → `SetPendingText("x")` exacto;
- (b) `Show` y `Present` con un pendiente → cero `SetPendingText` **en la vista**;
- (c) un rechazo → `box.Text` restaurado;
- (d) los controles nuevos sin tamaños fijos. `WorkspaceBrowserViewTests.cs:366-400` solo mira los elementos nombrados de F3.

### I-5. IMPORTANTE — La frontera `TabChange` necesita precondiciones que el paquete no fija

**Dónde.** D-T3B1-06 («antes de `session.Activate`, si la pestaña viva tiene borrador, `CrossBoundary(… TabChange)`»).

**Problema.**
- **El host autoriza antes de mirar el borrador** (`WorkspaceHostCore.cs:692-698`). Con la presentación desfasada del documento activo, cualquier cambio de pestaña desde una pestaña con borrador **limpio** (sin pendiente) da `Aborted` y deja al usuario sin poder cambiar de pestaña hasta el siguiente aviso.
- **Si se cruza sin comprobar que el destino es otra pestaña existente,** el clic en la pestaña activa, y los dos `Click` del doble clic (gesto de D-T3B1-03), comprometen un pendiente válido sin transición, o muestran «no se pudo cambiar» con uno inválido. `Activate` sobre la activa es un no-op (`BrowserSession.cs:116-124`).
- **«Aviso con los mensajes del campo»:** con `Aborted` por `Authorize` o por un fallo, y con `Busy`, `InvalidFields` está vacío. El motivo está en `Reason`.

**Corrección.**
- Cruzar `TabChange` solo si el destino existe, es distinto de la activa y la viva tiene `PendingText != null`.
- El aviso usa `InvalidFields` o, si está vacío, `Reason`.
- Pruebas: re-clic en la activa sin llamadas, cambio desde un limpio sin `CrossBoundary`, y `Aborted` por `Authorize` con su motivo.

### I-6. IMPORTANTE — Reentrada durante la lectura del gesto: el doble tiene el gancho, pero no hay regla ni prueba

**Dónde.** D-T3B1-03, D-T3B1-12 (el doble trae «un gancho de reentrada» sin uso declarado).

**Problema.** La revisión de T3A deja abierto si `ReadIntent` bombea mensajes (`independent-review.md:232`). Si lo hace:
- un `StateChanged` puede llegar durante `TryBeginEdit` o `TryReload` (`Present`: cambio de pestaña por selección o de documento);
- al volver, el gesto renderiza el campo con la sesión y el rack **del gesto** sobre una superficie que ya es otra;
- el usuario teclea en el borrador de A viendo la superficie de B. Rompe K = 1 y el aislamiento, y no se ve.

**Corrección.** Regla: tras cada lectura, el gesto comprueba que la superficie sigue siendo la suya (sesión y rack). Si no lo es, no renderiza el resultado, rematerializa la superficie actual desde `GetField` y avisa. Prueba con el gancho del doble: cambiar de pestaña y de documento dentro de `TryBeginEdit`.

### I-7. IMPORTANTE — La regla RED con `ExpectRed false` en Core obliga a un esqueleto que el paquete no describe

**Dónde.** §5 RequiredTests e INV-F4T3B1-13 («los arrastres que solo tocan pruebas o guardas de Core **pueden** pasar en el RED»).

**Problema.** `RedPart` da `pass` solo si las pruebas fallidas del RED están entre las de `ExpectRed` (`AUTOMATION_PLAN.md:599`, `:620`). Con Core en `ExpectRed false`, **ninguna** prueba de `RackCad.Tests.Workspace` puede fallar en el RED. Pero el RED lleva ya:
- los cambios de G-01..G-03: la construcción con `_controller.PilotEdit` y las firmas de 5 y 3 parámetros;
- el de G-04: los botones nuevos de la superficie;
- las guardas nuevas, por ejemplo «la paleta pasa `_controller.PilotEdit`».

Si el esqueleto no tiene ya ese cableado y esa vista, Core falla en el RED: `RedPart` falla, hay REWORK y se gasta un intento.

**Corrección.** Decir, como R-C3 hace para T3C, que el esqueleto del RED lleva el cableado completo (paleta, panel y navegador), las firmas, el XAML con todos sus controles y handlers enlazados a un VM inerte, y todo lo que nombran las guardas de Core. «Pueden pasar» pasa a «**deben** pasar». Las pruebas de comportamiento de UI fallan por aserción.

### I-8. IMPORTANTE — El cambio de G-04 no está especificado, y la guarda actual es frágil frente al campo

**Dónde.** §2 G-04 y §5 («G-01, G-02, G-03, G-04»). R-B2-1 de T3B2 («Exige el cambio declarado de G-04 que deje T3B1»).

**Problema.**
- `WorkspaceBrowserHostGuardTests.cs:303-307` corta el tramo en el **primer** `</Border>` tras `x:Name="BrowserSurface"`. Si el campo o su aviso van en un `Border`, como el aviso de PICKFIRST (`WorkspacePanelView.xaml:67-72`), el tramo se trunca y el recuento mide otra cosa.
- El paquete no dice qué afirma G-04 tras el cambio. Si pasa a otro recuento exacto, T3B2 tendrá que volver a cambiarlo para «Descartar» y «Cancelar», contra I-1.

**Corrección.** Fijar la forma nueva:
- el tramo es el elemento `BrowserSurface` completo, con equilibrio de `<Border`/`</Border>`;
- **cota** que nombra los botones de T3B1 («Volver a leer», «Editar», «Recargar»), cada uno con su handler dentro de `Contained`;
- se conserva la lambda exacta de `:310`.

Para G-01..G-03 basta con escribir el texto exacto nuevo de cada regex en el contrato.

### I-9. IMPORTANTE — Las pruebas 5 y 6 fijan un comportamiento interino que T3B2 invierte

**Dónde.** D-T3B1-06 (la selección cambia la pestaña activa también con pendiente) y D-T3B1-12, pruebas 5 y 6. §6 R-B2-4.

**Problema.**
- R-B2-4 (P5 `proposal-v5.md:335-336`) hace que, con texto pendiente, la pestaña siga activa. La prueba 5 de T3B1 («una selección que cambia la pestaña activa con texto inválido → procede, con `Dematerialize`») queda **contradicha** por T3B2, y lo mismo la 6 cuando se apoya en ese camino.
- T3B2 tendría que cambiar pruebas de T3B1, una C02 si no está declarada, y el §6 no lo dice.

**Corrección.** Una de dos:
- (a) escribir 5 y 6 sobre caminos que sobreviven a R-B6: `DocumentChange` con inválido, y `TabChange` sobre un borrador no editable con pendiente (Applying, Unknown o Error) → `Kept`, la transición procede y al volver aparece el pendiente exacto (`WorkspaceHostCore.cs:711`, `DraftSession.cs:370-371`). Además discrimina el menor 3(a) a nivel de UI;
- (b) marcar las pruebas de selección con pendiente como interinas y nombrarlas en R-B2-4 como cambios declarados de T3B2.

### I-10. IMPORTANTE — Falta la pulsación como primer gesto

**Dónde.** D-T3B1-03 (los gestos son «Editar», el clic en la caja y el doble clic en la pestaña).

**Problema.** D-08: «**La primera pulsación en un campo fija la preview**» (`proposal-v5.md:388`, PV2-14 y A64-PV1-12). R-B3: «Una pulsación o un clic en el campo». Con el foco en la caja de solo lectura, por ejemplo tras tabular, una tecla no hace nada. Es la omisión, sin declarar, de un disparador congelado (riesgo de C06).

**Corrección.** `PreviewTextInput` (y, si hace falta, `PreviewKeyDown` para teclas imprimibles) sobre la caja en solo lectura → el gesto: fijar e intención, **sin aplicar la tecla**. G-08 lo admite, porque no nombra `Keyboard`. Prueba en la vista.

### M-1. MENOR — Trampas no listadas en el §2

**Dónde.** §2. No rompen con el diseño actual, pero el Worker puede tropezar con ellas:
- `WorkspaceBrowserHostGuardTests.cs:245-246`: `.ReadSummary(` y `.IsSessionAlive(` exactamente 1 en el VM del navegador y 0 en los demás archivos de la UI, también el VM del campo.
- `:250-251`: `BrowserSessionRegistry`, una vez en toda la UI.
- `:310-311`: la lambda exacta de «Volver a leer» y un único `ReloadSummary()`.
- `:261`: el XAML, **comentarios incluidos**, sin «Abrir en editor» ni «editor clásico». `ClassicEditorHint` contiene «editor clásico» (`PushBackPilotIntentReader.cs:56`).
- `:283-296`: el `Border` de la plantilla de la pestaña sin atributos de borde, si alguien toca la plantilla.
- `WorkspacePaletteHostSourceGuardTests.cs:590-591`: también en el XAML.
- La documentación de los menores 5 y 7, comentarios incluidos:
  - `WorkspacePilotEditGuardTests.cs:782-788` y `WorkspaceEditIntentGuardTests.cs:821-830` prohíben `Transaction`, `Database`, `Commit(`, `Provider` y `Metric`… en el texto de `IWorkspacePilotEditPort.cs` y `WorkspaceEditIntent.cs`;
  - los anclajes de documentación en `WorkspaceEditIntent.cs`: el primer `        Waiting,` y el primer `        NotAdmitted,` (`WorkspacePilotEditGuardTests.cs:854-868`), `EditDiscardOutcome` y `Discarded` (`:834-851`), y `EditIntentStatus.Rejected` (`WorkspaceLifecycleGuardTests.cs:757-772`).

  `EditCheckStatus` está después (`WorkspaceEditIntent.cs:195`), así que la documentación nueva de `Waiting` no mueve los anclajes. Conviene decirlo.

### M-2. MENOR — Dos de las guardas nuevas no cumplen I-1

**Dónde.** D-T3B1-12, «Guardas nuevas».
- «Las tres lecturas del puerto solo se nombran en los métodos de gesto» es un conjunto exacto positivo. T4 añadirá «Comprobar ahora» (`TryCheckNow`) en otro gesto y tendrá que cambiarla. Mejor en negativo y por cuerpo: no se nombran en `Present`, `Refresh`, `Show`, `Clear`, `RebuildTabs`, `SyncSurface`, `StartSurface`, `ClearSurface`, `Materialize`, constructores ni setters.
- «La UI no llama a ninguna API … de modal» prohíbe para siempre lo que R-B2-1 y F5 admiten dentro de `EnterPanelModal`. Ya lo cubre G-13 (`WorkspacePilotEditGuardTests.cs:639-660`). Limitarla a los cuerpos de T3B1, o retirarla.

### M-3. MENOR — La prueba 15 repite, en la UI, el oráculo nominal del menor 1

**Dónde.** D-T3B1-12, prueba 15 («el doble del puerto no recibe nada que escriba»).

**Problema.** El puerto no tiene ninguna operación de escritura (T3A), así que la aserción no puede fallar.

**Corrección.** Afirmar lo que discrimina:
- cero llamadas a la navegación (`ExecuteAsync`);
- el conjunto exacto de operaciones del puerto que T3B1 usa;
- y que T3B1 no llama a `DiscardRack`, `DiscardDocument`, `GetRacksAtRisk`, `TakeLossNotices`, `TakeCloseNotices` ni `TryCheckNow` (limitado a T3B1).

### M-4. MENOR — `LostFocus`, Enter, Esc y el clic en la caja: efectos colaterales sin regla

**Dónde.** D-T3B1-03 y D-T3B1-04.
- `CommitField` en una caja sin borrador da `NotEditable` (`WorkspaceHostCore.cs:646-648`). Tras un clic en la caja de solo lectura con la intención en `Waiting` o `NotAdmitted`, el `LostFocus` pisa `NoticeText` con un aviso espurio. Regla: `CommitField` solo con `PendingText != null`.
- El botón de la pestaña toma el foco en el `MouseDown`: `LostFocus` → `CommitField` ocurre **antes** del `Click`. Si el compromiso cambia «*» (un valor igual al inicial vuelve a Limpio), la tira se regenera entre el `MouseDown` y el `MouseUp` y **el clic se pierde** (T2-F6). Es raro: declararlo o probarlo.
- Enter y Esc deben marcarse `e.Handled = true`, para que no lleguen a AutoCAD (`proposal-v5.md:131-132`). Esc no revierte, pero tampoco puede cancelar un comando.
- `LostFocus` es de foco lógico: hacer clic en el dibujo **no** compromete. «Salir de la caja» solo ocurre dentro del panel. Declararlo.
- `TextBox` maneja `MouseLeftButtonDown`, así que el gesto del clic tiene que ir en `PreviewMouseLeftButtonDown` (o en `GotFocus`). Un doble clic dispara dos `Preview…`: el gesto se ejecuta como mucho una vez con lectura (`AlreadyOpen` es idempotente, `WorkspaceHostCore.cs:307-308`). Prueba: un doble clic sobre una pestaña no activa cuya activación se aborta no fija ni lanza la intención.

### M-5. MENOR — Menor 7: «el `default` nunca es un permiso» no lo puede comprobar quien consume

**Problema.** `default(PilotBoundaryResult)` vale `TabChange` y `Proceeded` (`IWorkspacePilotEditPort.cs:114-138`). Es indistinguible de un `Proceeded` real de `TabChange`, así que la UI no puede «no tratarlo como permiso».

**Corrección.** Redactarlo como obligación de quien produce el valor: ningún productor ni doble devuelve `default`. El doble de UI programa siempre la decisión de forma explícita. Si se quiere una garantía real, es un cambio del puerto, de T4.

### M-6. MENOR — Menor 3(c): el `catch` de `GetField` sí es alcanzable

**Dónde.** D-T3B1-11, menor 3 («si un doble lo alcanza»).

**Hecho.** `OpenDrafts` es visible para `RackCad.Tests`. `Begin` con un copiador que devuelve `null` deja `CurrentDraftState` en null, porque el constructor no lo comprueba (`DraftSession.cs:258-259`). `ViewOfPilotDraft` hace NRE en `WorkspaceHostCore.cs:878`, fuera de sus `try` internos, y el `catch` de `:603-607` devuelve la vista Error.

**Corrección.** Decir este medio, para que el Worker no lo declare inalcanzable.

### M-7. MENOR — T3B2 consume `GetRacksAtRisk` y las colas antes de que T3C las endurezca

**Dónde.** §6 R-B2-2 y R-B2-3, frente al menor 4 (a T3C, «que lo consume», según la orden de cierre de T3A).

**Problema.** El primer consumidor es T3B2:
- la comprobación TOCTOU de R-B8 usa `GetRacksAtRisk`;
- R-B9 consume las colas.

Sus fallos son hoy inalcanzables, pero:
- el `default` de `GetRacksAtRisk` (`WorkspaceHostCore.cs:802-806`) se leería como «sin racks en riesgo». Para la TOCTOU es la dirección segura, pero el mensaje sería falso;
- `TakeLossNotices` sin `try` (`:809-819`) perdería los avisos ya sacados.

**Corrección.** Anotarlo en el §6: el VM de T3B2 contiene esas llamadas. Avisar al Coordinator de que la premisa «T3C lo consume» no se sostiene con el orden B1, B2, C.

### M-8. MENOR — La relectura de R-B9 es aceptable, pero debe decidirse ahora

**Dónde.** §6 R-B2-3 («T3B2 declara la lectura … que aplica»).

**Análisis.** La presentación corre dentro de callbacks (H-06 de T3), así que «fuera de todo callback» en sentido literal es inimplementable sin `Dispatcher` (G-07). La lectura «desde la presentación o un gesto, nunca desde el manejador de `BeginDocumentClose`» es razonable:
- consumir es memoria y no es modal;
- el controlador no emite `StateChanged` en `BeginDocumentClose` (`WorkspaceController.cs`: los avisos salen de `OnActiveSessionChanged`, `OnCommandStateChanged`, `OnDrained` y el panel).

**Corrección.** Registrar la relectura ya, como disposición, porque cambia la letra de una regla vinculante del paquete de T3, y fijarla con una guarda. Declarar que el veto no emite `StateChanged`: el panel lo ve en el siguiente aviso, normalmente el fin del comando.

### M-9. MENOR — R-B2-4 es implementable sin otra ronda de guardas; faltan dos precisiones

**Comprobado.** Un parámetro opcional `bool keepActive = false`:
- no cambia el conjunto de miembros por nombre (`WorkspaceBrowserBoundaryTests.cs:176-197`);
- no cae en el vocabulario (`:54-67`, `:104-113`);
- no es un delegado ni una interfaz (`WorkspaceBrowserSessionTests.cs:264-268`);
- la secuencia larga usa el valor por defecto (`:600-631`) y sigue igual. INV-F3T1-11 se conserva, porque el contexto recordado no forma parte de `Detail`.

**Falta.**
- «En modo retenido nunca se reemplaza la pestaña activa» (I-1).
- Los cambios declarados de las pruebas de T3B1 que fijan el comportamiento interino (I-9).

### M-10. MENOR — Riesgo de Long-horizon

**Dónde.** §5 C07.

**Problema.** Son 19 archivos, con un VM nuevo de muchos estados, tres archivos nuevos de UI, de ellos pruebas STA de teclado, una guarda nueva con autopruebas y 7 archivos de pruebas existentes. Simular Enter en STA exige `PresentationSource` o un `HwndSource` en la prueba. Es más que T3A, que tocó 12 archivos.

**Mitigación.** Fijar la mecánica de I-4 y M-4 y la forma exacta de G-01..G-04 (I-8), para reducir la exploración. Si el planificador da High, el bloque separable son los arrastres de Core (menores 1-3), pero moverlos exige al Owner, que los fijó en T3B1.

### M-11. MENOR — Redacción y desviaciones sin registrar

- **D-T3B1-07:** «La instancia de `Tabs` solo cambia cuando cambia la protección» es falso. También cambia con la pestaña activa, el título o la preview. Debe decir que la protección la cambia solo cuando cambia.
- **D-T3B1-03:** «`Admitted` o `AlreadyOpen` → «Listo para editar»» debe depender del estado. `AlreadyOpen` sobre Applying o Unknown no es editable.
- **El `NotAdmitted` de una recarga** conserva el borrador (doc M-4). Falta decir qué se muestra.
- **D-T3B1-06** se aparta de la tabla del paquete de T3 (`I-64-f4-t3-gate-contract-package.md:76-83`), que asigna al VM de T3B `RackChange`, `PanelHide` y `DocumentClose`. Son inocuas en el host (H-02), pero deben registrarse como desviaciones de esa tabla. `Dematerialize` debe cruzarse solo en la ruta de `Present`, no después de `TabChange` ni de `CloseTab`, para que las pruebas de orden de llamadas sean inequívocas.

## Lo que comprobé y encontré correcto

- **Fidelidad a las órdenes del Owner:**
  - T3B1 y T3B2 con el contenido elegido;
  - 1 de 3 cada una, «capacidad ×2» sin hablar de 6 intentos;
  - orden B1, B2, C, y F4-T4 no empieza antes de cerrar T3C;
  - replanificación de F4-E31;
  - cierre completo (no basta el 14/14);
  - reutilización de los sustitutos de nc1 y nc2 solo con la firma exacta.
- **Reparto de R-B1..R-B12 y de la cobertura 1-18:** no se pierde nada salvo lo señalado (I-10, M-11). La 18 no tiene subdiálogo en la UI de T3, así que se queda en el host, y es coherente.
- **Residuales:**
  - «Comprobar ahora» a T4 es legítimo: solo aplica a Unknown tras un compromiso que lanzó;
  - `PanelHide` sin observar es legítimo: el host lo ignora (H-02) y el borrador ya tiene el texto;
  - sin foco es admisible por la orden («may address»), con la salvedad de Enter y Esc de M-4;
  - cerrar una pestaña sin cambio es admisible como interino, con I-1.
- **Hechos:** H-01..H-09 y G-01..G-14, en las líneas citadas.
- **Guardas:** ninguna guarda rompe con D-T3B1-01..12 tal como están sin estar declarada. Las de reflexión sobre los VM de UI (`WorkspaceBrowserViewModelTests.cs:762-786` y `:949-962`) admiten un campo de tipo VM y un `Action`. Las guardas fuera del filtro que barren `src/RackCad.UI` (unidades, propiedades personalizadas, parámetros calculados) no afectan.
- **Puerto real frente a la UI:**
  - las no bloqueantes responden `Kept` sin `Authorize` ni búsqueda (`WorkspaceHostCore.cs:681-687`);
  - `GetField` y `GetRacksAtRisk` no autorizan;
  - el primer gesto es idempotente: `AlreadyOpen`, sin lectura;
  - con la presentación desfasada del documento activo, todo rechazo es visible y no pierde texto (salvo el bloqueo de I-5).
- **WPF:** con un binding bidireccional, el setter solo lo invoca el paso del destino al origen, así que distingue edición de presentación si no hay captura en `TextChanged`. La restauración dentro del setter es viable en .NET 8. `MouseDoubleClick` se dispara una vez por doble clic, también sobre el contenedor regenerado.
- **Contrato:**
  - los 19 archivos bastan: los proyectos usan la inclusión por defecto; los valores del puerto se construyen con fábricas y constructores públicos desde `RackCad.UI.Tests`, sin `InternalsVisibleTo`; `WorkspaceEditIntent.cs` hace falta para `EditCheckStatus.Waiting`. Nada sobra;
  - los filtros seleccionan las clases nuevas, porque el nombre completo es `RackCad.UI.Tests.Workspace…`;
  - MinSelected 2251 sale del mismo producto: `eb55606c` solo cambia documentación respecto de `18471988`.
- **Arrastres:**
  - menor 1: retirar el espía es aceptable (`task-close-package.md:70`), porque la evidencia real es la guarda por cuerpo `WorkspacePilotEditGuardTests.cs:664-737`;
  - menor 2: conforme a la recomendación de la revisión;
  - menor 3(a), 3(b) y 3(d): implementables con el banco de Core;
  - menor 5, menor 7 (con M-5) y N-5: solo documentación y comentarios, compatibles con las guardas de texto (M-1).

## Riesgos que solo resuelve un smoke con AutoCAD

1. **Teclado en la paleta (D-01):** que espacio, Enter, Esc, Supr, Ctrl+Z, Ctrl+C y Ctrl+V con el foco en la caja no lleguen a AutoCAD (`KeepFocus = true` ya existe), y que el foco no se pierda al sacar el ratón de la paleta.
2. **`LostFocus` frente a `LostKeyboardFocus`:** al hacer clic en el dibujo, o al terminar Seleccionar, Ver o Centrar, no hay compromiso. El pendiente queda protegido y visible.
3. **Si `ReadIntent` bombea mensajes:** reentrada en el gesto (I-6) y teclas con `Busy` y restauración.
4. **`Authorize` a cada pulsación** con `MdiActiveDocument`, en CLOSE, CLOSEALL o QUIT y en un cambio de documento con el panel abierto.
5. **El cursor tras una restauración**, y la composición IME.
6. **El clic de pestaña perdido** cuando el compromiso del `LostFocus` cambia «*» (M-4).
7. **Un drenaje que marca obsoleto un borrador Limpio** mientras se muestra (I-2).
8. **La cultura del valor mostrado** con los ajustes de usuario de Windows, y la ida y vuelta Enter/salir sin dirty falso (T3A, riesgo 7).
9. **El doble clic sobre una pestaña no activa**, cuyo contenedor se regenera entre los dos clics.
10. **Para T3B2:** los avisos de veto sin `StateChanged` y su momento de consumo (M-8).
