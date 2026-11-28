# I-64 — Revisión adversarial independiente del paquete de F4-T3B2-EDIT-CLOSE-SYNC (v1)

**Veredicto: EMITIBLE TRAS CORRECCIONES** — 0 BLOQUEANTE, 5 IMPORTANTE, 12 MENOR.

**Base revisada:** rama `architecture/workspace-persistente-rackcad` en `1a9445bd`. `git diff 1708e764 1a9445bd -- src tests` está vacío, así que el producto y las pruebas son los de `1708e764`. Solo lectura: no se compiló nada ni se tocó el repositorio.

Ningún hallazgo impide emitir el paquete, pero los cinco IMPORTANTE deben corregirse antes. I-1 haría fallar el GREEN fuera del alcance declarado. I-2 e I-3 contradicen autoridades. I-4 e I-5 dejan al Worker decisiones que el contrato debe fijar.

---

## Hallazgos IMPORTANTE

### I-1. Consumir las colas en `Present` rompe una prueba de vista de T3B1 que el paquete no declara

- **Dónde:**
  - `tests/RackCad.UI.Tests/Workspace/WorkspacePilotFieldViewTests.cs:26-36`: `Build()` construye el panel y llama a `Refresh()` sin vaciar `pilot.Calls`.
  - `:206-221`: `OnAReadOnlyBoxEscIsHandledWithNoGestureAndAModifierOrTabLaunchesNothingAndIsNotHandled`, con la aserción `Assert.DoesNotContain(pilot.Calls, c => c.Operation != GetFieldOp)` en `:218`.
  - `src/RackCad.UI/Workspace/WorkspacePanelViewModel.cs:72` y `:228`: el constructor del VM del panel y cada `Refresh` llaman a `Browser.Present`.
  - `tests/RackCad.UI.Tests/Workspace/WorkspacePilotEditPortDouble.cs:217-227`: el doble registra `TakeLossNotices` y `TakeCloseNotices` como llamadas.
  - En el paquete: D-T3B2-04 («`Present` toma las dos colas»), H-11, y en §5 `WorkspacePilotFieldViewTests.cs` «([si (c)], solo N-3)».
- **Qué está mal:** con D-T3B2-04, la construcción y el `Refresh` de `Build()` registran `TakeCloseNotices` y `TakeLossNotices` antes de la aserción, así que `:218` falla. H-11 solo examina `WorkspacePilotFieldViewModelTests.cs` y no ve esta prueba. El alcance de §5 prohíbe tocarla.
- **Por qué importa:** el GREEN no puede pasar sin editar un archivo de prueba fuera de lo declarado. Eso es la parada de cambio en pruebas existentes no declaradas, o una edición prohibida.
- **Recomendación:**
  - Declarar el cambio exacto: vaciar `pilot.Calls` tras `Build()`, o excluir `LossOp` y `CloseOp` como hace `T15` en `WorkspacePilotFieldViewModelTests.cs:1133-1144`.
  - Ampliar el alcance de ese archivo a «N-3 y esa prueba».
  - Añadir a §2 el hecho «cada `Present`, también el del constructor del panel, registra dos llamadas en el doble».
  - Mi barrido no encontró más casos:
    - `:126` usa el doble de un panel sin puerto;
    - `:393` vacía las llamadas tras `Editable()`;
    - en las pruebas de VM, `T15`, `T17` y `AssertNoReadAndNoMutation` ya toleran las colas.

### I-2. La pestaña limpia no se cierra si `DiscardRack` no responde `Discarded` (punto 1 de §6)

- **Dónde:**
  - Paquete: D-T3B2-01, segunda fila de la tabla.
  - Autoridades:
    - `docs/automation/evidence/I-64-pilot/F4-DRAFTS/coordinator/owner-f4t3-authorization.md:140-141` («Known clean tab: close without authored write»);
    - `docs/initiatives/I-64-f4-t3b-gate-contract-package.md:350` (R-B2-1: «Limpia: se cierra sin preguntar y llama a `DiscardRack`»);
    - `docs/initiatives/I-64-f4-t3-gate-contract-package.md:414` (R-B7: «… para que el borrador termine»);
    - `docs/initiatives/I-64-proposal-v5.md:390` (D-08: «sin dirty se cierra sin preguntar»).
  - Host:
    - `src/RackCad.Application/Workspace/WorkspaceHostCore.cs:726`: `Rejected` cuando `Authorize` falla, es decir, la sesión presentada no es la del documento activo;
    - `:727`: `Busy` solo por la reentrada de una lectura;
    - `:739-743`: `Failed`.
- **Qué está mal:** las tres autoridades dicen que una pestaña limpia conocida se cierra, y R-B7 añade la llamada a `DiscardRack` para que el borrador termine. El paquete condiciona el cierre al resultado del descarte. Con `Rejected` (desfase entre la presentación y el documento activo), el usuario no puede cerrar una pestaña limpia hasta la siguiente presentación.
- **Por qué importa:** contradice la letra de tres autoridades sin declararlo. Además no aporta seguridad: si el borrador limpio queda en el host, es el residual ya aceptado en T3B1 (`docs/initiatives/I-64-f4-t3b-gate-contract-package.md:181`). No está en riesgo, no veta el cierre del documento (`RacksAtRiskOf`, `src/RackCad.Application/Workspace/Drafts/DraftRegistry.cs:192-195`) y vuelve en solo lectura.
- **Recomendación:**
  - Cerrar siempre la pestaña limpia: primero `DiscardRack`, después `Close`.
  - Si el descarte no da `Discarded`, avisar sin bloquear: «la pestaña se cerró; el borrador limpio quedó en memoria: motivo».
  - Con esta regla, `T10_AClosedTabOfACleanDraft…` (`WorkspacePilotFieldViewModelTests.cs:444-464`) sigue pasando con el doble actual (`Rejected`, `AlreadyOpen` al volver). El cambio declarado de H-10 pasa a ser opcional.
  - Si se mantiene la regla del paquete, declararla como desviación de R-B2-1 y R-B7 para el Owner.
  - Añadir una prueba: el rechazo cierra y avisa.

### I-3. «Las dos vías de descarte» del aviso de veto no son las del Freeze

- **Dónde:**
  - Paquete: D-T3B2-04, aviso de veto de la sesión presentada: «el botón … y el texto que recuerda que cada pestaña se descarta con su "Cerrar"».
  - Freeze, `docs/initiatives/I-64-proposal-v5.md:608-610` (D-17): «el mensaje del veto, en la línea de comandos y en el panel, nombra las dos vías de descarte», que son la acción explícita del panel y la vía de comando (`RACKPANELDESCARTAR`).
  - `docs/initiatives/I-64-f4-t3-gate-contract-package.md:416` (R-B9) y `:426` (R-C2) usan la misma expresión con ese sentido.
- **Qué está mal:** el paquete reinterpreta las dos vías como el botón del documento y el «Cerrar» de cada pestaña. El comando no existe hasta T3C, así que T3B2 no puede nombrarlo, pero tampoco lo declara.
- **Por qué importa:** el texto del panel quedaría fuera de D-17 sin registro. Nada obliga a T3C a completarlo, y Smoke-2 (S2-06, `docs/initiatives/I-64-proposal-v5.md:886`) lo encontraría tarde.
- **Recomendación:** declarar la desviación transitoria: en T3B2 el aviso nombra la acción del panel; la vía de comando se nombra cuando exista. Añadir a las obligaciones de T3C la actualización del texto del aviso del panel. El recordatorio de «Cerrar» puede quedar como extra, sin llamarlo «la segunda vía».

### I-4. La forma de la UI nueva queda por inventar

- **Dónde:** paquete D-T3B2-01 a D-T3B2-05 y §4 (guardas «solo en los cuerpos de sus gestos de cierre y de descarte»). El código de hoy solo tiene avisos por superficie (`WorkspacePilotFieldViewModel.ShowNotice`, `src/RackCad.UI/Workspace/WorkspacePilotFieldViewModel.cs:174-179`). La contención de un gesto del navegador hoy pone en Error el resumen de la superficie (`ContainFailure`, `src/RackCad.UI/Workspace/WorkspaceBrowserViewModel.cs:470-485`).
- **Qué está mal:** el paquete fija `BrowserFieldDiscardButton` y `BrowserDiscardDocumentButton`, pero no fija:
  - **la API del VM:**
    - los gestos públicos (confirmar, cancelar, descartar el campo, descartar el documento, «Entendido»);
    - las propiedades enlazables de la confirmación (texto, visibilidad, lista);
    - la colección de filas de aviso y su tipo;
  - **dónde vive y cuánto dura el aviso del navegador:**
    - el de un rechazo de `DiscardRack`, que puede ser de una pestaña no activa;
    - el de la retención;
    - el del documento sin riesgos;
    - qué lo borra (¿la siguiente presentación, un gesto?);
  - **la vista:** los nombres en el XAML y de automatización de la confirmación, de sus dos botones y de las filas;
  - **la contención:** dónde acaba una excepción del puerto en los gestos nuevos (D-17 dice «la pestaña afectada pasa a Error y conserva su borrador»), sin poner el resumen ni el panel en Error.
- **Por qué importa:**
  - La guarda «solo en los cuerpos de sus gestos» no se puede escribir de forma determinista sin los nombres de esos miembros.
  - Una prueba de vista que afirme «exactamente "Descartar" y "Cancelar"» tampoco, sin los nombres de la vista.
  - Un aviso que se borra en cada `Present` desaparece casi al instante, porque las presentaciones son frecuentes.
  - Es el mismo hueco que obligó a fijar la «mecánica WPF» en T3B1 (`docs/initiatives/I-64-f4-t3b-gate-contract-package.md:140-144`, N-8) para acotar la exploración.
- **Recomendación:**
  - Añadir un D-T3B2 con:
    - la API pública nueva del VM del navegador (nombres y firmas);
    - las propiedades nuevas;
    - los nombres en el XAML y de automatización;
    - la vida de cada aviso (hasta el siguiente gesto, o hasta «Entendido»);
    - la contención de los gestos nuevos en un aviso del navegador, nunca en `ContainFailure`.
  - Derivar de esos nombres la lista exacta de miembros de la guarda nueva.

### I-5. Regla RED, pruebas requeridas y alcance de las pruebas sin fijar

- **Dónde:** paquete §5 «RequiredTests: Core con `ExpectRed` true … UI con `ExpectRed` true», y §4 «y una clase de vista si hace falta» frente a §5, que no la incluye. Precedente: INV-F4T3B1-13 en `docs/initiatives/I-64-f4-t3b-gate-contract-package.md:325-334` y el I-7 de su v1 (`:374`).
- **Qué está mal:**
  - T3B2 cambia la regla RED de T3B1: Core pasa de `ExpectRed` false a true.
  - No dice qué lleva el esqueleto: la firma `ApplySelectionContext(…, bool keepActive = false)` inerte, los miembros del VM, los elementos del XAML enlazados a un VM inerte y todo lo que nombran las guardas nuevas.
  - No dice qué pruebas pueden fallar en el RED (solo las nuevas, y por aserción).
  - Faltan los filtros y `MinSelected`.
  - La «clase de vista si hace falta» no tiene sitio en `AllowedWriteScope`.
- **Por qué importa:**
  - Un RED que no compila, o que falla también en las 2251 pruebas de Core existentes, sería indistinguible con `ExpectRed` true.
  - La decisión delegada del cierre de T3B1 registra una variación del Controller justo en Ci y `ExpectRed` (`docs/automation/evidence/I-64-pilot/F4-DRAFTS/coordinator/delegated-f4t3b1-close-decision.md`, nc3 y PD-F4-06).
- **Recomendación:**
  - Añadir un INV-F4T3B2 de RED:
    - el esqueleto completo y compilable, que no lanza;
    - en el RED fallan solo las pruebas nuevas, y por aserción;
    - todas las existentes pasan.
  - Fijar los filtros y `MinSelected` (Core 2251 o la cifra medida en `1708e764`; UI la cifra medida).
  - Añadir al alcance, con su nombre exacto, la clase de vista nueva (por ejemplo `WorkspacePilotCloseSyncViewTests.cs`), o declarar que las pruebas STA viven en `WorkspacePilotCloseSyncTests.cs`.

---

## Hallazgos MENOR

### M-1. La pestaña sin borrador se cierra sin `DiscardRack`: desviación de letra sin declarar (punto 2 de §6)

- **Dónde:** paquete D-T3B2-01, primera fila; R-B2-1 en `docs/initiatives/I-64-f4-t3b-gate-contract-package.md:350`.
- **Qué está mal:** R-B2-1 dice «Limpia: … llama a `DiscardRack`». En sustancia no hay contradicción:
  - la orden del Owner solo exige cerrar sin escribir;
  - R-B7 explica la llamada como «para que el borrador termine», y sin borrador no hay nada que terminar;
  - `DiscardRack` sobre un rack sin borrador responde `Discarded` con listas vacías (`WorkspaceHostCore.cs:732-737`, `DraftRegistry.cs:265-266`).

  Aun así, la regla es vinculante y la desviación no se declara.
- **Recomendación:** declararla como desviación de letra con esa justificación. No llamar a `DiscardRack` evita además que un `Rejected` bloquee el cierre (I-2).

### M-2. INV-F4T3B2-05 y D-T3B2-07 («cero lecturas del dibujo», «T3B2 no lee el dibujo») son falsas al cerrar la pestaña activa

- **Dónde:** paquete D-T3B2-07 e INV-F4T3B2-05; `src/RackCad.UI/Workspace/WorkspaceBrowserViewModel.cs:249-263` (`Interact`), `:373-387` y `:416-436` (`SyncSurface` y `Materialize`, con `ReadSummary(…, userRequested: true)` en `:427`).
- **Qué está mal:** cerrar la pestaña activa, también con «Descartar», convierte la siguiente en la superficie viva y le pide el resumen, que es una lectura del dibujo de F3.
- **Por qué importa:** un Worker o un Controller que aplique la invariante al pie de la letra (por ejemplo, el doble del resumen sin llamadas) chocaría con un comportamiento cerrado de F3.
- **Recomendación:** redactar «cero llamadas a las tres lecturas de la intención y ninguna escritura; la lectura del resumen de la nueva superficie activa, de F3, no cambia».

### M-3. Trampas de guardas existentes que el diseño roza y que el §2 no lista

Ninguna se rompe necesariamente, pero el camino natural de implementación sí las rompe:

- **`.IsSessionAlive(` exactamente una vez** en el VM del navegador (`tests/RackCad.Tests/Workspace/WorkspaceBrowserHostGuardTests.cs:244-249`). Quitar el aviso de veto «cuando su sesión deja de vivir» de una sesión que no está en el registro pide otra comprobación. Debe hacerse en el mismo bucle de `PurgeDeadSessions` (`WorkspaceBrowserViewModel.cs:265-269`), sobre las sesiones conocidas y las de los avisos, o declararse.
- **G-06:**
  - `\+=` y `-=` están prohibidos en todo el VM del navegador (`WorkspaceBrowserHostGuardTests.cs:274`) y en el del campo (`tests/RackCad.Tests/Workspace/WorkspacePilotSurfaceGuardTests.cs:330`);
  - esto incluye concatenar texto con `+=` al componer los avisos.
- **Vocabulario de la UI** (`tests/RackCad.Tests/Workspace/WorkspacePaletteHostSourceGuardTests.cs:580-593`):
  - `Registry` (sensible a mayúsculas) y `RackLog` están prohibidos en la UI; por ejemplo, un `_noticeRegistry`, o registrar la pérdida en el log;
  - `\bKeyboard` está prohibido en el código de la UI (`WorkspacePilotSurfaceGuardTests.cs:281`, `tests/RackCad.Tests/Workspace/WorkspaceNavigationHostTests.cs:919`), también `e.KeyboardDevice`. Para N-3 no se puede usar `Keyboard.IsKeyUp` ni nada parecido.
- **La sección del navegador** (`WorkspaceBrowserHostGuardTests.cs:381-400`): sin `<ContentControl`, sin tamaños fijos y sin `<Window`, `ShowDialog` ni `MessageBox`. La confirmación y las filas de aviso viven ahí.
- **`D_NoNewControlOfTheFieldHasAFixedWidthHeightMinimumOrMaximum`** (`tests/RackCad.UI.Tests/Workspace/WorkspacePilotFieldViewTests.cs:646-670`): conviene ampliar su lista con `BrowserFieldDiscardButton` y los elementos nuevos con nombre, en la clase nueva.
- **G-05:** el record `WorkspaceBrowserTabItem` no cambia (`WorkspaceBrowserHostGuardTests.cs:276`). El título sin la marca sale de `BrowserTabRecord.Name`.
- **`BrowserSession.cs`:**
  - aparte del vocabulario de H-05, no puede nombrar ningún tipo de Workspace salvo `SessionId` y `SelectionContext` (`tests/RackCad.Tests/Workspace/WorkspaceBrowserBoundaryTests.cs:83-101`); por ejemplo, `SelectionContextKind` cae;
  - lo leen dos guardas de vista previa: `tests/RackCad.Tests/Workspace/WorkspaceEditIntentGuardTests.cs:716-745` (`EditIntent|PilotIntent|…`) y `tests/RackCad.Tests/Workspace/WorkspaceDraftPilotIntentGuardTests.cs:184-212`, esta última con comentarios incluidos.
- **Recomendación:** añadirlas al §2 como lista de trampas, en el estilo de G-01..G-16 de T3B.

### M-4. Sin puerto de edición, `Present` debe ser el de antes; y construir el panel ya llama al puerto

- **Dónde:**
  - Paquete: INV-F4T3B2-01, que solo habla de `CloseTab`.
  - `src/RackCad.UI/Workspace/WorkspacePanelViewModel.cs:62-65`, documentación: «Building the panel never calls it».
  - `tests/RackCad.UI.Tests/Workspace/WorkspacePilotFieldViewModelTests.cs:92-104`, nombre `…BuildingItCallsNothing`.
- **Qué está mal:**
  1. Con «un fallo del puerto cuenta como pendiente», un `try/catch` alrededor de `_pilotEdit.GetField` con `_pilotEdit == null` daría `keepActive` verdadero siempre sin puerto. Lo cazarían las 248 pruebas del navegador, pero el contrato debe decirlo.
  2. Con D-T3B2-04, el constructor del panel consume las colas: su documentación queda falsa y el archivo está fuera del alcance.
- **Recomendación:**
  - Extender INV-F4T3B2-01: sin puerto de edición, `Present` no consume colas y no pasa `keepActive`.
  - Añadir al alcance la línea de documentación de `WorkspacePanelViewModel.cs`, o declarar que queda desfasada.

### M-5. Huecos de la retención frente a P5 `:335-336` (punto 5 de §6)

- **Dónde:** paquete D-T3B2-05; `docs/initiatives/I-64-proposal-v5.md:335-336`; `WorkspaceHostCore.cs:603-607`.
- **Qué está mal:**
  - **Sin aviso cuando R ya tiene pestaña:** «`R` ya tiene pestaña: `None`, sin activarla» no da aviso. P5 pide el aviso de la retención, y el usuario selecciona R en el dibujo sin ver efecto. El VM puede distinguirlo: `keepActive`, R con pestaña y R distinta de la activa.
  - **El fallo contenido del host no cuenta como pendiente:** la vista de fallo de `GetField` tiene `PendingText` null, mientras que «un fallo del puerto cuenta como pendiente» solo cubre la excepción. Lo conservador es retener también con una vista `Exists` que no se puede decidir (`State` Error o Undecidable, e `IsProtected`).
  - **El caso «preview activa» descarta R sin decirlo:** hoy es casi inalcanzable, porque una pestaña con pendiente siempre está fijada.
- **Recomendación:** aviso también con R abierta y no activada; criterio conservador explícito; documentar el caso residual.

### M-6. El aviso de veto «de otra sesión» no dice cuándo se clasifica ni cómo se purga

- **Dónde:** paquete D-T3B2-04.
- **Qué está mal:**
  - No dice si «de la sesión presentada» o «de otra sesión» se decide al consumir o al mostrar. Si el usuario activa ese documento, el aviso debería pasar a ofrecer el botón, como sugiere su propio texto («actívalo para descartarlos»).
  - La purga «cuando su sesión deja de vivir» de una sesión que el registro no conoce enlaza con M-3, por `IsSessionAlive`.
- **Recomendación:** clasificar al mostrar, contra `_session`, y fijar la purga.

### M-7. El descarte del campo y el del documento tienen casos sin definir

- **Dónde:** paquete D-T3B2-02 y D-T3B2-03; `src/RackCad.UI/Workspace/WorkspacePilotFieldViewModel.cs:352-354`; `WorkspaceHostCore.cs:520-530`.
- **Qué está mal:**
  - **El campo en Error sin borrador:** «Recargar», y con él «Descartar», también aparece con `_failure != null` sin borrador (`view.Exists` false). La regla de D-T3B2-01 no llama a nada sin borrador, así que no se sabe qué hace «Descartar» ni si sale del Error.
  - **Qué número da el aviso del documento:** «aviso con el número de racks descartados»; pero `DiscardAllByUser` devuelve en `RemovedRackIds` también los borradores limpios no listados. El número puede no coincidir con la lista confirmada.
- **Recomendación:**
  - Ofrecer «Descartar» solo con borrador (`Exists`), o definir que sin borrador actúa como «Recargar».
  - Informar los racks en riesgo confirmados y, aparte, los limpios retirados.

### M-8. El doble y el oráculo de la prueba 11

- **Dónde:**
  - Paquete H-09 y §4 (prueba 11: «el doble no tiene vía de escritura»).
  - Doble: `tests/RackCad.UI.Tests/Workspace/WorkspacePilotEditPortDouble.cs:15-19` (comportamiento por defecto «as the host does») y `:199-227` (sin `ThrowIfAsked`).
- **Qué está mal:**
  - No fija si el `DiscardRack` y el `DiscardDocument` por defecto pasan a imitar al host (`Discarded` y quitar el campo) o siguen en `Rejected` con respondedor.
  - No fija si las operaciones nuevas aceptan `Throws`.
  - «El doble no tiene vía de escritura» es un oráculo nominal: la lección del menor 1 de T3A, un espía no conectado a nada.
- **Recomendación:**
  - Fijar el comportamiento por defecto y el soporte de `Throws`.
  - Dar a la prueba 11 un oráculo que discrimine:
    - el conjunto de operaciones llamadas, como `T15`;
    - cero lecturas de la intención;
    - cero llamadas a la navegación (`ExecuteAsync`).

### M-9. Riesgos WPF de la confirmación y de los avisos

- **Dónde:** paquete D-T3B2-01 y D-T3B2-04; `WorkspacePanelView.xaml.cs:131` (`LostFocus` compromete); `docs/initiatives/I-64-f4-t3b-gate-contract-package.md:206` (clic perdido por la regeneración de la tira).
- **Qué está mal:**
  - **`IsDefault` e `IsCancel`:** no se prohíben en los botones de la confirmación. Un «Descartar» con `IsDefault` haría destructivo un Enter en otra parte del panel.
  - **`LostFocus` compromete antes de la confirmación:** al pulsar «Cerrar» con la caja enfocada, `LostFocus` lanza `CommitField` antes del clic. Un pendiente válido llega comprometido a la confirmación, y tras «Cancelar» ya no es pendiente. La prueba 13 en el VM no ve este orden.
  - **Clic perdido en «Cerrar»:** si ese compromiso cambia la marca, la tira se regenera entre `MouseDown` y `MouseUp` y el clic puede perderse (la misma limitación de T3B1).
  - **Filas de aviso estables:** la colección de filas debe conservar su instancia cuando no cambia, como `RebuildTabs` (T2-F6), para no perder el clic de «Entendido».
- **Recomendación:**
  - Prohibir `IsDefault` e `IsCancel` en la confirmación.
  - Declarar el compromiso por `LostFocus` y el clic perdido como limitaciones para Smoke-2.
  - Pedir la estabilidad de la colección de avisos.

### M-10. N-3: falta la prueba negativa y la reserva del host

- **Dónde:** paquete D-T3B2-06 (N-3) y §4; `tests/RackCad.UI.Tests/Workspace/WorkspacePilotFieldViewTests.cs:457-478`, que solo prueba pulsaciones no repetidas.
- **Qué está mal:** la regla dice «de esa misma tecla», pero ninguna prueba impide marcar todas las repeticiones de la caja editable. Mantener Retroceso o una flecha pulsada dejaría de funcionar.
- **Recomendación:**
  - Añadir pruebas con `MarkRepeat`, como en `:96-103`:
    - la repetición de otra tecla no queda `Handled`;
    - tras una pulsación no repetida, la memoria se borra.
  - Declarar que el efecto real en AutoCAD, que `Handled` en el túnel suprima el `WM_CHAR`, se comprueba en Smoke-2 (D-01), igual que en T3B1.

### M-11. La guarda de R-B2-3 no cubre el Plugin

- **Dónde:** `docs/initiatives/I-64-f4-t3b-gate-contract-package.md:352` («nunca desde el manejador de `BeginDocumentClose` … T3B2 lo fija con una guarda»); paquete §4.
- **Qué está mal:** la guarda propuesta solo restringe la UI. El manejador de `BeginDocumentClose` está en el Plugin, y hoy allí solo aparecen los reenvíos (`src/RackCad.Plugin/Workspace/WorkspaceController.cs:234-275`, y en Application `src/RackCad.Application/Workspace/WorkspacePilotSupport.cs:192-194`).
- **Recomendación:** que la guarda nueva, que lee archivos sin modificarlos, afirme también que en el Plugin y en Application `TakeCloseNotices` y `TakeLossNotices` solo se nombran en los reenvíos y en el host.

### M-12. La reversión del pendiente de una pestaña sana

- **Dónde:** `docs/initiatives/I-64-f4-t3b-gate-contract-package.md:162` («Esc no revierte: la reversión es el descarte explícito de T3B2»); paquete D-T3B2-02 («Descartar» solo en Error).
- **Qué está mal:** en una pestaña sana, la única reversión es «Cerrar» → «Descartar» (y volver a abrir) o descartar el documento. Es coherente con R-B2-1, pero T3B1 lo prometió como «el descarte explícito de T3B2» y el paquete no lo dice.
- **Recomendación:** declararlo, o dejarlo como mejora de F7.

---

## Respuestas a los puntos de §6

1. **La tabla de D-T3B2-01 frente al host:** la clasificación es correcta.
   - `IsProtected` es `IsDirty`, que incluye el pendiente (`src/RackCad.Application/Workspace/Drafts/DraftSession.cs:283`), o Undecidable (`WorkspaceHostCore.cs:856-871`).
   - `GetField` del host nunca lanza y su fallo es `Exists` con `IsProtected`.
   - La pestaña limpia, en cambio, debe cerrarse aunque el descarte falle (I-2).
2. **Cerrar sin borrador:** no contradice R-B2-1 en sustancia; es una desviación de letra que hay que declarar (M-1).
3. **Consumir solo en `Present`:** basta para R-B2-3, que admite la presentación o un gesto.
   - No he encontrado pérdida de avisos:
     - el veto de una sesión que muere antes de presentarse lo quita el host en `SessionDestroyed` (`WorkspaceHostCore.cs:950`);
     - el host encola la pérdida si la hay (`:961-965`);
     - un veto ya consumido se purga con la sesión.
   - El coste es la latencia ya declarada.
   - Hay tres matices:
     - consumir en `Present` rompe una prueba no declarada (I-1);
     - el constructor del panel también consume (M-4);
     - la purga de sesiones que el registro no conoce choca con G-15 (M-3).
4. **La TOCTOU por igualdad ordinal:** es suficiente. Las dos lecturas son de memoria, síncronas y sin bombeo, y el host ordena las listas (`DraftRegistry.cs:186-195`, `WorkspaceHostCore.cs:779-800`). El cambio de contenido de un rack ya listado no cambia el conjunto: el usuario confirmó por rack.
5. **El modo retenido:** cumple P5 en lo esencial y no rompe pruebas ni guardas.
   - Comprobado:
     - la API se guarda por nombre;
     - los parámetros se guardan por tipo (`bool` permitido);
     - los enums no cambian;
     - todas las presentaciones con pendiente en las dos clases de T3B1 son de otra sesión, del mismo rack, de un contexto igual al último o sin pestañas.
   - Faltan los avisos y los casos de M-5.
6. **N-3 en `PreviewKeyDown` sin `KeyUp`:** es correcto en WPF. `IsRepeat` viaja en los mismos argumentos del túnel y de la burbuja, y T3B1 ya lo usa en `WorkspacePanelView.xaml.cs:126` con su prueba `:273-294`. Faltan la prueba negativa y la reserva del host (M-10).
7. **Guardas o pruebas que el alcance rompe:** sí, `WorkspacePilotFieldViewTests.cs:218` (I-1). Más las trampas de M-3.

---

## Lo verificado y correcto

**Hechos de §2** (líneas comprobadas en `1a9445bd`; algunas difieren en una o dos líneas, sin efecto):

| Id | Comprobación |
|---|---|
| H-01 | `CloseTab` en `WorkspaceBrowserViewModel.cs:171`; `Close` en `BrowserSession.cs:140-156` |
| H-02 | Las pruebas sin puerto citadas construyen el panel o el navegador sin piloto (`WorkspaceBrowserViewModelTests.cs:43`, `:272`, `:926`; `WorkspaceBrowserViewTests.cs:29`) |
| H-03 | `IsProtected` en `:524-538`, conservador; `RebuildTabs` en `:490-522` |
| H-04 | `_lastContext` antes del tipo, en `BrowserSession.cs:83-84` |
| H-05 | Guardas por nombre (`WorkspaceBrowserBoundaryTests.cs:176-197`), enums (`:169-173`), vocabulario (`:104-113`, solo código, sin comentarios), prohibiciones (`:55-67`), tipos de parámetros (`:216-243`; `WorkspaceBrowserSessionTests.cs:258-270`). Un `bool` opcional no rompe ninguna |
| H-06 | Las cinco operaciones del host y `AdviseClose` en las líneas citadas; el veto no emite `StateChanged`. El adaptador del Plugin y la compuerta solo reenvían (`WorkspaceController.cs:234-275`; `WorkspacePilotSupport.cs:186-194`) |
| H-07, H-08 | Las guardas de límites y G-04 dejan libres las operaciones de T3B2 y admiten un botón «Descartar» en la superficie |
| H-09 | El doble responde `Rejected`, listas vacías y colas vacías, y aplica el pendiente de un no editable (`:184-193`) frente a `DraftSession.cs:370-371` |
| H-10, H-11 | Correctos para `WorkspacePilotFieldViewModelTests.cs`. Su enumeración no cubre la clase de vista (I-1) |
| H-12 | La conclusión es correcta; la lista de «cinco casos» está incompleta: además, `T05` en su variante `Kept`, la mitad `kept` de `T06`, `T10_APreview…`, `ShowAndPresent…`, `T15` y la prueba de vista `B_`, todos inocuos |
| H-13 | Correcto (`WorkspacePanelView.xaml.cs:94-128`; `WorkspacePilotFieldViewTests.cs:488`) |

**Decisiones correctas:**
- Las ramas de `keepActive` respetan «nunca se reemplaza la pestaña activa» y una sola preview.
- Con la pestaña solo dirty se conserva el camino de `Dematerialize` de T3B1 (`WorkspacePilotFieldViewModelTests.cs:793-812`).
- N-1: `Kept` sin mutar es el comportamiento del host, y solo `T06` depende del doble actual.
- La confirmación en línea es compatible con las guardas de subdiálogo y de modal (`WorkspacePilotEditGuardTests.cs:630-645`).
- El resto del alcance prohibido y las paradas son coherentes con T3B1.
