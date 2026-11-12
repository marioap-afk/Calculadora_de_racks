# Revisión adversarial independiente: paquete del contrato de F4-T2C (v1)

**Objeto:** `I-64-f4-t2c-gate-contract-package.md` (v1), frente a `owner-f4t2c-authorization.md` y `owner-f4t2c-scope-decision.md`.
**Repositorio:** worktree `architecture-workspace-persistente-rackcad`, HEAD `92ad237c`. Solo lectura: no se ejecutaron builds, pruebas ni AutoCAD.

## Veredicto

**EMITIBLE TRAS CORRECCIONES.**

La v1 no se puede emitir tal como está. El mecanismo central es sólido y no hace falta rediseñarlo: el Idle solo programa, el planificador es puro y coalescido, `CanDrainNow` comparte predicado con `TryDrain`, y la continuación usa `BeginInvoke(Background)` sobre el Dispatcher del hilo principal.

Los dos bloqueantes están en la instrumentación de D-T2C-05 y se corrigen dentro del paquete. Hay además siete importantes:
- uno de comportamiento (bucles de mensajes anidados y la secuencia de cierre de F4-T2B2B);
- uno de evidencia (punto 13);
- uno de especificación de pruebas (J);
- tres de guardas;
- uno del smoke dirigido (K = 1).

Recuento: **2 BLOQUEANTES, 7 IMPORTANTES, 10 MENORES.**

---

## Hallazgos

### B-1 (BLOQUEANTE). `RackLog.Warning` en el puente choca con una guarda existente no declarada

- **Sección:** D-T2C-05 («una línea `RackLog.Warning` dentro de un método `[Conditional("DEBUG")]`»), §5 «Cambios declarados (solo estos)», INV-F4T2C-10 y C02.
- **Repositorio:** `tests/RackCad.Tests/Workspace/WorkspaceHostSourceGuardTests.cs:480-485`, `NoHostFileLogsAMeasurementAsAWarning`. La guarda recorre todos los archivos de `src/RackCad.Plugin/Workspace/` y de `src/RackCad.Application/Workspace/` y exige `Assert.DoesNotMatch(new Regex(@"RackLog\.Warning"), code)`. Lee el texto del código, sin comentarios, así que `[Conditional("DEBUG")]` no la esquiva.
- **Problema:** el contrato pide algo que una guarda vigente prohíbe, y a la vez declara que ninguna otra guarda cambia. El Worker solo tiene dos salidas:
  - rompe la guarda (CI en rojo);
  - la modifica (parada C02).

  El §2 tampoco lista esta guarda entre las que «fija el repositorio».
- **Corrección:** elegir una de estas opciones y escribirla en el paquete.
  - (a) Quitar la línea de `RackLog` y registrar la prueba de temporización en host como limitación para Smoke-2. La orden del Owner lo admite: «record that limitation for Smoke-2 rather than inventing evidence».
  - (b) Declarar un tercer cambio de guarda, por archivo exacto (`AutoCadEventBridge.cs`), que admita `RackLog.Warning` solo dentro de los métodos `[Conditional("DEBUG")]` de D-T2C-05. Lleva su autoprueba, no añade pruebas al archivo de 45 y tiene que decidirlo el Owner, porque relaja INV-F1-T3A-11.

  En los dos casos hay que actualizar el §2 (H-09..H-16), INV-F4T2C-10 y C02.

### B-2 (BLOQUEANTE). El `Debug.Assert` de profundidad salta siempre y, en .NET 8, termina AutoCAD

- **Sección:** D-T2C-04 (la continuación es `() => Contain("IdleContinuation", OnIdle)`), D-T2C-05 (`_callbackDepth` lo suben y bajan `Contain` y `ContainSession`; al empezar `OnIdle`, `Debug.Assert(_callbackDepth == 0)`), §6 («ningún aviso de `Debug.Assert` aparece»).
- **Problema 1, contradicción interna:** la propia continuación entra por `Contain`, así que al empezar `OnIdle` la profundidad vale 1. La aserción falla en **todos** los drenajes del DLL Debug. Ninguna prueba lo detecta: el código está en el Plugin, que las pruebas de Core no cargan, y la CI compila el Plugin en Release.
- **Problema 2, consecuencia:** en .NET 8, que es el runtime de AutoCAD 2025, un `Debug.Assert` fallido sin depurador adjunto no muestra ningún cuadro. Termina el proceso con `Environment.FailFast`, salvo que alguien haya configurado un listener. El §6 prescribe justo el DLL Debug para el smoke del Owner, así que AutoCAD se cerraría en el primer drenaje, con el dibujo del Owner abierto. La frase «ningún aviso de `Debug.Assert` aparece» describe el comportamiento de .NET Framework.
- **Problema 3, poco valor:** el contador solo ve los `Contain` y `ContainSession` del puente. No detecta una continuación anidada en un callback de AutoCAD o de otro complemento. Lo único que detectaría es un post síncrono (`Invoke`), y eso ya lo fija G-3 por texto.
- **Corrección:**
  - (a) Si se conserva un detector, que no cuente la contención de la propia continuación. Por ejemplo, un `ContainContinuation` aparte, o que solo cuenten los 16 manejadores.
  - (b) Sustituir `Debug.Assert` por un diagnóstico que no termine el proceso: un contador en memoria, solo en Debug, y la vía de registro que resulte de B-1.
  - (c) Mejor aún, mover el contador de profundidad al planificador puro, con `EnterCallback`/`ExitCallback` y una consulta de «la pasada corrió con profundidad 0». Así lo prueban las pruebas de Core.
  - (d) Corregir el texto del §6.

  Si el contador se queda en el puente, usar `++`/`--` y decrementar en `finally` (ver M-4).

### I-1 (IMPORTANTE). La continuación corre en bucles de mensajes anidados donde el Idle nunca corría, y `_core.OnIdle()` olvida la secuencia de cierre aunque no drene

- **Secciones:** D-T2C-02 («`Run` … ejecuta `continuation`» sin volver a comprobar), D-T2C-03 y la respuesta a los puntos 2, 3 y 8 de la revisión.
- **Hechos:**
  - Un `DispatcherOperation` de prioridad Background lo procesa **cualquier** bucle de mensajes del hilo principal, no solo el bucle principal de AutoCAD. Eso incluye:
    - la pregunta «¿Guardar cambios?» de un cierre;
    - los `AcApplication.ShowModalWindow(...)` de los comandos RackCad (`RackSelectivoCommands.cs:40`, `RackPushBackCommands.cs:48` y 18 más);
    - los avisos y jigs dentro de comandos;
    - el `MessageBox` de otro complemento.

    `Application.Idle` sale de la pasada de reposo del bucle principal y no se dispara en esos bucles modales.
  - La protección es solo el predicado en memoria:
    - el reposo se aprende por `CommandWillStart`/`EnteringQuiescentState`;
    - el modal RackCad **nunca** se activa en el producto: `SetModalRackCadActive` no tiene ningún llamador fuera de las carpetas Workspace (búsqueda en `src`).
  - `EventBridgeCore.OnIdle` olvida `_lastAllowedClose` **siempre**, drene o no (`EventBridgeCore.cs:385-386`).
  - `BeginDocumentClose` se dispara antes de la pregunta de guardar (de ahí el «cierre cancelado» de los arrastres de F4-E35). Si la continuación pendiente corre dentro de esa pregunta:
    - con CLOSE como comando, no drena (no hay reposo), pero olvida la secuencia; `sequenceIntact` queda en false y `RetireConservative` sustituye a `RetireKnownSafe` (`EventBridgeCore.cs:271-280`; `WorkspaceHostCore.cs`, `SessionDestroyed`);
    - en un cierre sin comando (pestaña o API, por medir) o en QUIT/CLOSEALL, que activa documentos que no ejecutan comandos, el predicado puede ser verdadero; entonces drena y lee la selección y el resumen dentro del modal de cierre.

    Es una ruta nueva hacia el trabajo cerrado de F4-T2B2B: el marcado IC-08 y la secuencia de D-T2B2-10. Además, la «regla de devolución» de F4-E34 (`f4-dispositions.md:790`) devuelve al Owner los falsos inciertos.
- **Corrección:**
  - (a) `Run` desmarca y **vuelve a evaluar `canRunNow()`** antes de llamar a la continuación; si es falso, no hace nada (no llama a `_core.OnIdle()` ni olvida). Hay que adaptar la segunda frase de F y añadir dos pruebas en Core:
    - programado y después no elegible: la continuación no corre;
    - una continuación no elegible no rompe `closeSequenceIntact` (el asesor de la prueba recibe `true` tras Allow y destrucción).
  - (b) Registrar como decisión, o preguntar al Owner, si `CanDrainNow` debe ser falso mientras el cierre permitido de la sesión activa sigue abierto (`_lastAllowedClose == _active`). Es solo lectura, pero G-5 hoy prohíbe nombrar `_lastAllowedClose` en `CanDrainNow`.
  - (c) Añadir a los riesgos de Smoke-2:
    - CLOSE de un dibujo modificado con el panel visible y pistas pendientes;
    - cierre por la pestaña;
    - QUIT y CLOSEALL con varios dibujos modificados;
    - un comando RackCad con ventana modal.
  - (d) Arrastre a F4-T3: los subdiálogos del panel tienen que activar `SetModalRackCadActive(true)` **antes** de `ShowModalWindow`, porque desde T2C la continuación corre dentro de ellos (D-02, «Subdiálogos del panel»).

### I-2 (IMPORTANTE). El punto 13 no se cumple para la lectura del resumen: G-9 comprueba antes de la lectura

- **Sección:** D-T2C-06, fila 13 («`AssertNoOpenTransaction` del controlador sigue tras el drenaje»), G-9.
- **Repositorio:** `WorkspaceController.cs:153` (`AssertNoOpenTransaction()`) va **antes** de `OpenDrainNotice` (`:162`), de `NotifyStateChanged` (`:165`) y de la lectura del resumen que provoca.
  - `WorkspaceNavigationFacts.Scan` no comprueba `TopTransaction`;
  - el lector de selección sí la comprueba (`WorkspaceSelectionReader.cs:115`).

  D-02 e INV-14 piden la comprobación en Debug al terminar **cada operación y cada drenaje**. La lectura que mueve T2C (F4-E05) es justo la que no tiene comprobación posterior. G-9, además, fijaría ese orden.
- **Corrección:**
  - (a) En la continuación del puente, después de `OnIdle()` (es decir, después de los manejadores de `Drained`, resumen incluido), una comprobación `[Conditional("DEBUG")]` de `TopTransaction == null`. Debe usar `document.TransactionManager` y no `document.Database…`, porque `WorkspaceBridgeFacadeSourceGuardTests.cs:127-128` exige que toda línea del puente con `.Database` sea un `+=`/`-=`. No debe terminar el proceso (B-2).
  - (b) O bien declarar que, para el resumen, el punto 13 lo acreditan las guardas de `using` (`WorkspaceBridgeSourceGuardTests.cs:227-234`) y el smoke, y no G-9.

  G-9 tiene que fijar presencia, no el orden respecto de `OpenDrainNotice`.

### I-3 (IMPORTANTE). La prueba J contradice el comportamiento vigente del host, que está fuera del alcance

- **Sección:** §5, J («… la sesión no se degrada por el lector»).
- **Repositorio:** una excepción del lector de selección dentro de `WorkspaceHostCore.OnDrained` acaba en `Contain(outcome.Session, …)` (`WorkspaceHostCore.cs:203-206`), que hace `_faulted.Add(session)` (`:685-687`). La sesión pasa a degradada en el host: `MayDrain` es falso y el estado del panel, Degraded. `WorkspaceHostCore.cs` no está en el `AllowedWriteScope`.
- **Problema:** tal como está escrita, J o no se puede implementar o empuja al Worker a cambiar el host (C01). Tampoco dice qué se espera en la variante del resumen.
- **Corrección:** J tiene que afirmar:
  - `EventBridgeCore.IsDegraded(session) == false` (el núcleo del puente no se degrada);
  - el host marca la sesión como fallida, que es el comportamiento vigente y no cambia;
  - la continuación no se reprograma;
  - los tics siguientes no programan, porque no quedan pistas;
  - no hay más lecturas.

  En la variante del resumen, la presentación del arnés contiene la excepción como `Failed`, igual que `WorkspaceNavigationHost.ReadSummary` (`:94-105`).

### I-4 (IMPORTANTE). G-1 es una lista blanca de «el manejador no hace nada más», ya es falsa hoy y contradice I-1

- **Sección:** G-1 («Las 16 líneas de manejador del puente llaman solo a miembros `_core.` o a `OnIdleTick`»).
- **Repositorio:**
  - las líneas de manejador llaman hoy a `Contain(`, `ContainSession(`, `NativeId(e.Document)`, `RaiseCommandStateChanged()` y `e.Veto()` (`AutoCadEventBridge.cs:133-249`);
  - los cuerpos de varias líneas de `DocumentActivated` y `DocumentToBeDestroyed` invocan `ActiveSessionChanged?.Invoke()`;
  - la cabecera de `WorkspaceLifecycleGuardTests.cs:16-19` recoge la regla I-1: fijar «Veto() si y solo si el núcleo decide Veto», **nunca** «el manejador no hace nada más». F4-T3 puede envolver el asesor o el manejador de cierre (arrastre M-1 de F4-E35).
- **Corrección:** guarda negativa. Ningún bloque `case` ni ninguna línea de manejador contiene:
  - `(?<![\w.])OnIdle\b` (sin el `Tick`);
  - `Drained`;
  - `BeginInvoke`;
  - `_idleScheduler`, salvo el bloque Idle, cuyo cuerpo es `Contain("Idle", OnIdleTick)`;
  - ninguna API de lectura.

### I-5 (IMPORTANTE). Ninguna guarda fija el cableado del planificador en el Plugin

- **Sección:** D-T2C-04, G-2 y G-3.
- **Problema:** A-J usan en el arnés `canRunNow: core.CanDrainNow`. Nada impide que el Plugin pase `() => true`, otra continuación, ningún `SetPost` o dos planificadores. Ejemplos de cableado erróneo verosímil:
  - `canRunNow: () => true` produce un bucle Idle → post → continuación → sin drenaje → Idle mientras haya pistas no elegibles (panel oculto, comando activo). Se queda girando a prioridad Background y A-J siguen en verde;
  - sin `SetPost`, no hay ningún drenaje en todo el proceso y no queda ningún registro.
- **Corrección:** una guarda que fije:
  - exactamente un `new WorkspaceIdleScheduler(` en el producto, en el constructor del puente;
  - que su primer argumento sea `_core.CanDrainNow`;
  - que su continuación contenga `OnIdle` por grupo de método dentro de una contención;
  - exactamente una llamada a `SetPost(`, en el constructor, con `BeginInvoke` y `DispatcherPriority.Background`.

### I-6 (IMPORTANTE). La excepción de `Dispatcher` abre huecos que ninguna guarda cierra, y no tiene autoprueba posible

- **Sección:** D-T2C-04 (excepción por archivo exacto), G-3, cambio declarado (2).
- **Repositorio:**
  - `NoHostFileStartsABackgroundThreadOrAsyncWork` (`WorkspaceHostSourceGuardTests.cs:409-429`) prohíbe `\bTimer\b`, que **no** casa con `DispatcherTimer`: entre «Dispatcher» y «Timer» no hay límite de palabra;
  - tras la excepción, nada prohíbe en el puente `DispatcherTimer`, `DispatcherFrame`/`PushFrame` (un bombeo anidado, es decir, reentrada), `Dispatcher.Yield`, `InvokeAsync` ni `_dispatcher.Invoke`. G-3 solo lista una parte;
  - la expresión de la excepción sería una variable local dentro de un `[Fact]` de un archivo que debe seguir con 45 pruebas (`WorkspacePaletteHostSourceGuardTests.cs:657`; `WorkspaceLifecycleGuardTests.cs:682`). No se puede ejercitar desde otro archivo, y el paquete exige que cada guarda lleve su autoprueba.
- **Corrección:** el patrón del puente mantiene prohibidos:
  - `\w*Timer\b`, `DispatcherFrame`, `PushFrame`, `\bYield\b`, `InvokeAsync`, `(?:_dispatcher|CurrentDispatcher)\s*\.\s*Invoke\b`;
  - `System\.Windows(?!\.Threading\b)`;
  - lo demás que ya prohíbe.

  G-3 fija el texto exacto de esa expresión en `WorkspaceHostSourceGuardTests.cs`, como hace la metaguarda con la de la paleta (`WorkspacePaletteHostSourceGuardTests.cs:648`). El archivo nuevo de guardas prueba una copia literal de la expresión con texto inyectado.

### I-7 (IMPORTANTE). El smoke dirigido está mal calibrado: con la máquina K = 1 vigente, un evento que invalida no refresca el resumen visible

- **Sección:** §6, observación clave, pasos 1-2 («MOVE o un cambio de propiedad del rack … el resumen se refresca después»); D-T2C-01 («Ni el controlador ni la UI cambian»).
- **Repositorio:**
  - `BrowserRackSummaries.OnDrained` solo borra la memoria (`BrowserRackSummaries.cs:85-98`);
  - `WorkspaceBrowserViewModel.SyncSurface` solo vuelve a materializar si la superficie es nueva o si estaba Pending/Busy (`WorkspaceBrowserViewModel.cs:211-225`, condición en `:224`);
  - tras MOVE, el contexto pasa a None y `BrowserSession.ApplySelectionContext` no cambia nada (`BrowserSession.cs:83-88`). Volver a seleccionar el mismo rack tampoco reinicia la superficie (`:90-92`).

  Por tanto, tras MOVE o un cambio de propiedad, el drenaje no lee el resumen y la superficie Ready sigue mostrando lo anterior hasta «Volver a leer», un cambio de pestaña o un cambio de documento. La observación del §6 no se produciría, o se malinterpretaría. La orden del Owner pide justo que, tras invalidar el resumen activo, «the summary later refreshes correctly».
- **Corrección:**
  - (a) Para la lectura del resumen en la continuación, usar disparadores que la produzcan de verdad: seleccionar otro rack con PICKFIRST (pestaña de vista previa nueva), o cambiar de documento (Busy, y lectura en el drenaje siguiente).
  - (b) Llevar al Owner, como pregunta o residual explícito, que una superficie Ready no se refresca tras un evento que invalida su resumen. Es un hueco de producto preexistente de F3 y no se debe reinterpretar en silencio. Si el Owner quiere refresco, es un cambio de UI fuera del alcance de T2C.
  - (c) Corregir también la variante «panel oculto (no hay líneas de drenaje)»: una continuación ya programada ejecutaría la pasada, y con D-T2C-05 escribiría su línea, salvo que se adopte la corrección (a) de I-1.

---

### M-1 (MENOR). G-2 está mal formulada

D-T2C-04 pasa `OnIdle` como grupo de método (`Contain("IdleContinuation", OnIdle)`, sin paréntesis), y el cuerpo de `OnIdle` contiene `_core.OnIdle(`. «`OnIdle(` aparece exactamente en su declaración y en la continuación» no cuadra con ninguna de las dos cosas. Hay que especificar:
- `(?<![\w.])OnIdle\b` exactamente 2 veces (declaración y continuación);
- `_core\.OnIdle\s*\(` exactamente una vez, dentro de `private void OnIdle()`.

### M-2 (MENOR). G-3 necesita una expresión precisa

El puente contiene legítimamente `Drained?.Invoke(`, `ActiveSessionChanged?.Invoke()` y `CommandStateChanged?.Invoke()`. «`.Invoke(` sobre el dispatcher» tiene que ser una expresión anclada al receptor. G-3 debe admitir los dos órdenes de sobrecarga de `BeginInvoke`, `(DispatcherPriority, Delegate)` y `(Delegate, DispatcherPriority)`, para no rechazar una implementación correcta.

### M-3 (MENOR). G-8 tiene que limitarse a los archivos de producto del cambio

Las autopruebas de las guardas nuevas inyectan `Commit(`, `UpgradeOpen` y otras. Si G-8 recorre «los archivos del cambio», incluidos los de prueba, salta sobre sí misma.

### M-4 (MENOR). Si el contador del puente sigue, `++`/`--` y `finally`

`_callbackDepth += 1` o `-= 1` rompería los recuentos exactos de 16 `+=` y 16 `-=`:
- `WorkspaceBridgeFacadeSourceGuardTests.cs:111-112`;
- `WorkspacePaletteHostSourceGuardTests.cs:384-385`;
- `WorkspaceLifecycleGuardTests.cs:242-243`;
- `WorkspaceHostC2SourceGuardTests.cs`, `TheCorrectionsAddNoAutoCadSubscriptionNorHandler`.

El decremento tiene que ir en `finally`.

### M-5 (MENOR). Trampas del repositorio que el §2 no lista

- Al extraer el predicado compartido en `EventBridgeCore.OnIdle`, `_lastAllowedClose = null;` tiene que seguir justo después de `if (!_started) return outcomes;` y antes de cualquier rama (`WorkspaceLifecycleGuardTests.cs:148-159`), con el recuento 13 (`:99`).
- Todo archivo de `Application/Workspace` prohíbe en código las subcadenas `Transaction`, `\bDatabase\b`, `LockDocument` y `\bCommit\(` (`WorkspaceModelBoundaryTests.cs:39-48`).
- También prohíbe, sin distinguir mayúsculas, `Provider`, `Metric`, `Aggregat`, `\bBom`, `TotalRacks` y `ProjectSummary` (`WorkspaceHostSourceGuardTests.cs:537-544`; `WorkspaceBridgeSourceGuardTests.cs:362-372`).
- Prohíbe además `RackLog` (`WorkspaceHostSourceGuardTests.cs:487-492`).

`WorkspaceIdleScheduler.cs` y `CanDrainNow` tienen que respetar todo esto. Conviene decírselo al Worker.

### M-6 (MENOR). H-16 es inexacto

La CI sí compila el Plugin, en el job `build-plugin` (`windows-latest`), en Release y con `-NoWarnings`:
- `.github/workflows/ci.yml:254-317`;
- `eng/ci/verify-autocad-references.ps1:595-596`.

El «build Release del Plugin sin avisos nuevos» queda cubierto por la CI del SHA exacto. Por eso cualquier campo o método solo para Debug no puede generar avisos en Release.

### M-7 (MENOR). Desviaciones de textos ratificados sin registrar

- La fila de T2C (`I-64-f4-t2-gate-contract-package.md:54`) declara las capas «UI, Plugin y pruebas»; el paquete toca Application y no la UI.
- D-T2-06 (`:224-230`) dice «Drenaje: marca y no lee» y «La programa … la UI».

La decisión del Owner (`owner-f4t2c-scope-decision.md`) lo justifica, pero el paquete tiene que registrar expresamente qué partes de D-T2-06 y de esa fila quedan sustituidas. Es la lección de encuadre de I-55.

### M-8 (MENOR). La «presentación» del arnés es un sustituto

No reproduce las reglas de materialización de `WorkspaceBrowserViewModel` ni la puerta de `WorkspaceNavigationHost`. El «1 lectura del resumen» de C, D e I depende del sustituto. Hay que:
- declararlo;
- hacer que imite al menos el orden de `ReadSummaryOnActiveDocument`: ventana, autorización y memoria;
- acreditar K = 1 por «UI y host de navegación sin cambios» y `RackCad.UI.Tests`.

### M-9 (MENOR). Vida del planificador y precisión de D-T2C-03

- Si la operación del Dispatcher se aborta o el Dispatcher se apaga, `IsScheduled` queda en true para siempre y no hay más drenajes en todo el proceso, sin ningún registro. Es muy improbable en el hilo principal fuera del cierre de AutoCAD, pero conviene una detección por recuento de tics, sin temporizadores, o registrarlo como riesgo.
- `Run` tiene que contener también una excepción de `logFault`, igual que `EventBridgeCore.Log`.
- D-T2C-03 razona mal: «una sesión destruida no tiene pistas porque `EnqueueHint` lo impide». `Destroy` no vacía `_pending`. Lo que protege es que el registro retira la sesión (`WorkspaceSessionRegistry.cs:31-37`).

### M-10 (MENOR). Instrumentación permanente en Debug

El DLL Debug es el que usa el Owner en sus validaciones. Instrumentación permanente que registra en cada drenaje choca con la letra «do not ship permanent debug behavior». Conviene fijar su retirada tras Smoke-2, o ligarla al interruptor de instrumentación en memoria que ya existe.

---

## Lo comprobado y encontrado correcto

- **Mapa del §2:** H-01..H-08 se verificaron con sus líneas (diferencias de una o dos líneas, sin efecto).
  - El drenaje solo lee la selección (H-04).
  - Son en memoria:
    - el marcado IC-08 (`WorkspaceHostCore.cs:159-160`, `MarkPilotDraftsPossiblyStale`);
    - `NavigationTargets.ApplyDrain` del runner;
    - la invalidación de la caché de definiciones;
    - la purga del eco.
  - `ActiveSessionChanged`, `CommandStateChanged`, mostrar u ocultar la paleta y `RACKPANEL` responden `Busy` con la ventana cerrada (`WorkspaceNavigationHost.cs:117-118`).
  - Las lecturas que pide el usuario corren en manejadores WPF.
  - No encontré otra ruta de lectura del DWG en callbacks de AutoCAD.
- **Defensa en profundidad en el momento de leer:**
  - el lector de selección comprueba el documento activo frente al identificador nativo de la sesión y, por `MayRead`, el `Editor.IsQuiescent` real;
  - el resumen comprueba `CommandInProgress`, `IsQuiescent`, la ventana y la autorización de la sesión.

  Una pasada sobre un estado en memoria obsoleto, por ejemplo LISP sin comando, puede consumir pistas, pero no lee.
- **Puntos 6 y 7:** la continuación no captura ninguna sesión, sino que drena la activa en el momento de ejecutarse. Una sesión destruida desaparece del registro, así que no hay lectura obsoleta ni cruzada.
- **Paridad y bucles:**
  - `CanDrainNow` y `TryDrain` coinciden en el mismo instante;
  - no hay un caso «verdadero pero no drena» sin cambio de estado entre el tic y la continuación;
  - con el cableado correcto (ver I-5) no hay bucle Idle → post → Idle;
  - todas las transiciones de `IdleNeeded` a falso tienen su `SyncIdle` sin la pasada por tic: encolar, destrucción, degradación, drenaje y `Stop`;
  - desmarcar antes de ejecutar no crea dobles lecturas: las pistas se consumen dentro de la misma continuación, y el Idle no se dispara dentro de ella.
- **Mecanismo:**
  - `Dispatcher.CurrentDispatcher` capturado en el constructor del puente es el del hilo principal (RACKPANEL), y `BeginInvoke` siempre encola;
  - el contexto es el mismo que el de los manejadores WPF de la paleta, que ya bloquean el documento (`WorkspaceNavigationHost.cs:154-171`);
  - hay precedente en el repositorio: `src/RackCad.UI/Editor/DispatcherRecomputeScheduler.cs:17-29`, con `CurrentDispatcher` y `BeginInvoke` a prioridad Background;
  - las alternativas son peores:
    - `ExecuteInApplicationContext` es síncrono desde el contexto de aplicación;
    - un segundo tic de Idle sigue estando dentro de un callback de Idle;
    - `SendStringToExecute` y P/Invoke están prohibidos.
- **Guardas sin cambios necesarios:**
  - 16 manejadores, 16 `+=` y 16 `-=` (si no se usa `+=` en el contador);
  - 9 archivos en `Plugin/Workspace`, exactos (`WorkspacePaletteHostSourceGuardTests.cs:765-774`);
  - 45 pruebas;
  - superficie pública del puente;
  - prohibiciones de lectura y escritura del puente;
  - los olvidos de la secuencia de cierre (13).

  El cambio declarado (1), la lista por reflexión, es correcto, y no hay otra lista por reflexión de `EventBridgeCore`. Las pruebas de comportamiento existentes llaman a `core.OnIdle()` directamente y siguen valiendo. Ninguna prueba lee el texto del caso Idle de forma que rompa el cambio de `OnIdle` a `OnIdleTick`.
- **Alcance:** aparte de B-1 y de lo que añadan I-2 e I-6, los 7 archivos bastan. No se toca ningún archivo de la clausura de la ventana, ni `WorkspaceController.cs`, ni `WorkspaceHostCore.cs`, ni `NotePilotEdit`.

## Riesgos que solo resuelve un smoke con AutoCAD (Smoke-2 o el smoke dirigido)

1. Si la continuación corre dentro de:
   - la pregunta de guardar de CLOSE (con comando);
   - el cierre por pestaña o API;
   - QUIT y CLOSEALL con varios dibujos modificados;
   - las ventanas `ShowModalWindow` de RackCad;
   - jigs y avisos de comando.

   Para cada caso, con qué estado en memoria corre, y si rompe `sequenceIntact` o lee dentro del modal (I-1).
2. La latencia y el orden reales de Background frente a entrada del usuario y tics de Idle. Que la continuación siempre llega a ejecutarse e `IsScheduled` nunca queda bloqueado.
3. `LockDocument` desde la continuación durante un cambio o un cierre de documento (rutas de `LockViolation`).
4. Si el cierre por la pestaña emite CLOSE como comando, porque cambia el reposo en memoria durante la pregunta.
5. Otros complementos que bombean mensajes dentro de sus callbacks de AutoCAD. Una continuación anidada ahí no la detecta ningún contador del puente.
6. La observación clave del Owner (refresco del resumen tras invalidar) depende de K = 1 (I-7): hay que usar el cambio de selección a otro rack o el cambio de documento.
7. Con el DLL Debug, un `Debug.Assert` fallido termina AutoCAD (B-2). Incluso las aserciones `TopTransaction` existentes tienen esa consecuencia si alguna vez fallan, y el Owner tiene que saberlo antes de operar.
8. Que la prueba de temporización de pila en host sea fiable con la instrumentación que resulte de B-1 y B-2. Si no lo es, queda como limitación para Smoke-2, como admite la orden.
