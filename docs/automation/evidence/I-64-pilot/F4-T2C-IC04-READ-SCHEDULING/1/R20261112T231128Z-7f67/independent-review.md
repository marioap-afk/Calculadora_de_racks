# Revisión adversarial independiente: I-64 F4-T2C-IC04-READ-SCHEDULING

- Rango: `d5d94a76..33e43f9f`. RED `703bda9d` y GREEN `33e43f9f`. 7 archivos, todos dentro del AllowedWriteScope.
- Modo: solo lectura (git show, git diff, git grep y lectura de archivos). No se ejecutaron build ni pruebas, y no se abrió AutoCAD.
- Autoridades leídas:
  - el gate-contract de F4-T2C;
  - el paquete v2 (§2, §4, §5, §9, §10);
  - `owner-f4t2c-authorization.md` y `owner-f4t2c-scope-decision.md`;
  - INV-F4T2B2B-01 e INV-F4T2B2B-04.

## Veredicto: ACEPTABLE CON MENORES (0 BLOQUEANTE, 0 IMPORTANTE, 4 MENOR)

**Código de producción.** Es correcto y cumple la letra del contrato:
- el callback de Idle solo programa;
- la continuación es diferida de verdad (`BeginInvoke` a `Background`) y vuelve a comprobar la elegibilidad;
- `CanDrainNow` es el predicado compartido más la condición del cierre pendiente;
- no hay `+=` ni `-=` nuevos;
- no queda ninguna lectura del DWG en un callback de AutoCAD.

**Lo que falla.** Los cuatro hallazgos son de precisión de guardas y pruebas, y de trazabilidad. El primero responde a la pregunta 5: en el Plugin, un `OnIdleTick` que drena a través del método intermedio pasa todas las guardas. Lo clasifico como MENOR para ser coherente con la calibración del proyecto: N-3 de la re-revisión, un hueco de la misma familia en la precisión de G-2 y G-10, fue MENOR. Aun así, recomiendo corregirlo antes del cierre o registrarlo como residual explícito. El post real y la temporización de pila en el Plugin solo los acreditan G-2, G-3 y G-10 (N-3). La corrección cabe en el archivo nuevo de guardas, que en un intento de corrección no cuenta como guarda existente (F4T2C-C02).

## Hallazgos

### 1. MENOR (el más relevante): G-1 y G-2 no cubren el método intermedio de la continuación

**Dónde:**
- `tests/RackCad.Tests/Workspace/WorkspaceIdleSchedulingGuardTests.cs:51-52`: `HandlerForbidden`;
- `:104-123`: `TickAndPassViolations`, con la lista prohibida en `:108`;
- `:600-606`: G-10 admite un único método intermedio.
- Código afectado: `src/RackCad.Plugin/Workspace/AutoCadEventBridge.cs:270-281` (`OnIdleTick` y `RunIdleContinuation`).

**Escenario 1: `OnIdleTick` drena.** Esta mutación drena y lee de forma síncrona dentro del callback de Idle:

```csharp
private void OnIdleTick() { _idleScheduler.OnIdleTick(); RunIdleContinuation(); }
```

Pasa todas las guardas:
- **G-2:** el cuerpo contiene `_idleScheduler.OnIdleTick()`. Ninguna de `_core\.OnIdle|Drained|OnIdle\s*\(|LockDocument|StartTransaction|BeginInvoke` casa con `RunIdleContinuation(`. `OnIdle` sin `Tick` sigue apareciendo 2 veces, y `_core.OnIdle(` y `Drained?.Invoke(` una vez cada uno.
- **G-1:** `OnIdleTick` no es un bloque `case` ni una línea de manejador.
- **G-9, G-10** y las guardas existentes no cambian.

**Escenario 2: un manejador de documento llama a la continuación.** Añadir `RunIdleContinuation();` dentro del bloque `DocumentActivated`, después de `ActiveSessionChanged?.Invoke();`, provoca una lectura dentro del callback del documento. Tampoco salta nada:
- `HandlerForbidden` no incluye el nombre del método intermedio;
- el orden que fijan las guardas C2 y de la paleta (`Contain < core < raise < hook`) se conserva.

**Por qué importa.** El espíritu de INV-F4T2C-01 es que `OnIdle` solo corra desde la continuación del planificador, y D-T2C-04 lo repite. G-2 lo fija contando las referencias a `OnIdle`, pero G-10 (por N-3) admite un intermediario cuyo nombre no cuentan ni G-1 ni G-2. Así, «OnIdle solo lo nombra la continuación» deja de implicar «la pasada solo corre desde el planificador».

Las pruebas de Core detectan un `OnIdleTick` que drena, pero solo al nivel del planificador. El cableado del Plugin no lo ejerce ninguna prueba.

No contradice la letra de INV-F4T2C-08: su G-1 enumera nombres que no incluyen el intermediario. El hueco viene del diseño del paquete.

**Corrección sugerida** (solo en el archivo nuevo, con autoprueba):
- G-1 y G-2 prohíben también el nombre del intermediario que G-10 extrae (`build.Groups[1]`), en los bloques `case`, en las líneas de manejador y en `OnIdleTick`;
- o bien se exige que el intermediario se nombre exactamente 2 veces en el puente: su declaración y el argumento de `new WorkspaceIdleScheduler(`;
- o, como `OnIdleTick` es un cuerpo propio (I-1 lo admite), se fija su cuerpo exacto: `privatevoidOnIdleTick(){_idleScheduler.OnIdleTick();}`.

### 2. MENOR: G-3 y G-10 no fijan la forma del post; hay maneras de ejecutar en el acto que no salta nada

**Dónde:**
- `WorkspaceIdleSchedulingGuardTests.cs:159-168`: `BackgroundBeginInvoke` y `PostViolations`;
- `:613-619`: G-10 solo pide que el texto de `.SetPost(...;` contenga `BeginInvoke` y `DispatcherPriority.Background`;
- `tests/RackCad.Tests/Workspace/WorkspaceHostSourceGuardTests.cs:407`: la expresión `bridgeForbidden`;
- código vigente correcto: `AutoCadEventBridge.cs:41`.

**Escenarios.** Estas variantes del post pasan G-3, G-10, la excepción por archivo exacto y `NoHostFileStartsABackgroundThreadOrAsyncWork`, cuya regla genérica no prohíbe `.Wait(`:
- `SetPost(action => _dispatcher.BeginInvoke(DispatcherPriority.Background, action).Wait());` en el hilo del dispatcher, `DispatcherOperation.Wait()` hace `Dispatcher.PushFrame`, así que la continuación corre anidada **dentro** del callback de Idle;
- `SetPost(action => { _dispatcher.BeginInvoke(DispatcherPriority.Background, action); action(); });` es un post que ejecuta en el acto.

Además, con la excepción del puente siguen permitidos:
- `ExecuteInApplicationContext`, prohibido expresamente por D-T2C-04, INV-F4T2C-06 y F4T2C-C04, y que ejecuta síncronamente si ya se está en contexto de aplicación;
- `Dispatcher.Run()`.

**Por qué importa.** Las pruebas de Core no ejercen el post real; N-3 lo deja a las guardas. Una regresión plausible del tipo «asegurar que corre» rompería IC-04 sin que salte ninguna guarda.

**Lo que no es defecto.** El contrato fija literalmente la expresión de la excepción, y la entrega la respeta. Lo que falta está en el archivo nuevo.

**Corrección sugerida** (en G-3 o G-10 del archivo nuevo, sin tocar la expresión contratada):
- exigir que el argumento de `SetPost`, aplanado, sea exactamente `action=>_dispatcher.BeginInvoke(DispatcherPriority.Background,action)` (o el otro orden de la sobrecarga);
- prohibir en el puente `\.Wait\s*\(`, `ExecuteInApplicationContext` y `Dispatcher\.Run\s*\(`, con su autoprueba.

### 3. MENOR: tres pruebas del cierre pendiente no afirman la profundidad ni el recuento de lecturas

**Dónde:** `tests/RackCad.Tests/Workspace/WorkspaceIdleSchedulerTests.cs`:
- `:1027-1046`: `AnAllowedCloseOfTheActiveSessionMakesCanDrainNowFalseUntilTheNextEntryOfTheCore`;
- `:1048-1067`: `AnAllowedCloseOfADocumentThatIsNotActiveAlsoMakesCanDrainNowFalse`;
- `:1091-1105`: `CanDrainNowDoesNotEndThePendingCloseSequence`.

**Escenario.** Usan el arnés con lectores dobles y hacen tics. Aun así, no llaman a `AssertNoReadInsideACallback` ni afirman `TotalReads == 0`.

INV-F4T2C-07 dice «toda prueba afirma que ninguna lectura ocurrió con profundidad mayor que 0, además del recuento». En la práctica, un planificador que drenara en línea ya lo detectan indirectamente:
- las pistas, que seguirían pendientes;
- `CanDrainNow`, que tendría que volver a ser verdadero.

El incumplimiento es solo literal.

**Otras pruebas.** E (dos), F-oculto, G-cambio, H (cuatro casos) y la de la continuación previa al cierre afirman `TotalReads == 0`, lo que implica la profundidad. Las positivas (C, D, F-2, G-2, I y J) sí llaman a `AssertNoReadInsideACallback`.

**Corrección:** añadir `Assert.Equal(0, h.TotalReads)` y `AssertNoReadInsideACallback(h)` a esas tres.

### 4. MENOR: trazabilidad y documentación del puente

**Dónde:**
- `AutoCadEventBridge.cs:13-24`: la documentación de la clase;
- `:279` y `:293`: los contextos de registro.

**Escenario.**
- La documentación de la clase sigue diciendo que el puente es dueño de todo `+=`/`-=` «and nothing else». Ahora también posee el planificador y el post al dispatcher.
- El fallo contenido de la pasada (`Contain("IdleContinuation", OnIdle)`) y el aviso Debug de transacción abierta registran ambos con el contexto `AutoCadEventBridge.IdleContinuation`. Un extracto de RackLog del smoke solo los distingue por el tipo y el texto de la excepción.

**Por qué importa:** solo afecta a la claridad de la evidencia del smoke y a D-T2C-05. No hay impacto funcional.

**Fuera del alcance (no corregir aquí):** el texto `Busy` de `WorkspaceNavigationHost.cs:117-118` («not read inside a notice of AutoCAD») queda algo desfasado, pero ese archivo está prohibido.

## Lo que comprobé y está correcto

### Callbacks de AutoCAD
- El único manejador de Idle es `(s, e) => Contain("Idle", OnIdleTick)` (`AutoCadEventBridge.cs:260`), y `OnIdleTick` solo llama a `_idleScheduler.OnIdleTick()`.
- Ningún bloque `case` ni línea de manejador drena, invoca `Drained`, postea ni lee. `DocumentActivated` y `DocumentToBeDestroyed` siguen notificando y la presentación responde `Busy` fuera de la ventana de aviso (H-06).
- `private void OnIdle()` no cambia. Solo lo nombra `RunIdleContinuation` (`Contain("IdleContinuation", OnIdle)`), seguido de la comprobación Debug.

### El planificador (`src/RackCad.Application/Workspace/WorkspaceIdleScheduler.cs`)
- Es puro: sin estático, sin eventos, sin `RackLog` y sin tipos de host.
- Los argumentos nulos lanzan; `logFault` es opcional.
- `OnIdleTick` sigue el orden programado → sin post → `canRunNow`. Marca **antes** de `post(Run)` y nunca ejecuta por sí mismo.
- `Run` desmarca primero, vuelve a evaluar dentro de un `try` y solo entonces ejecuta. No se reprograma.
- Están contenidas las excepciones de `canRunNow`, de `post`, de la continuación y del propio `logFault` (`Report`).
- **Ningún camino de código deja `IsScheduled` en true.** Desmarcar es la primera sentencia de `Run` y no puede lanzar. Si `post` lanza, se desmarca.
- **No hay bucle Idle → post → Idle.** Con `CanDrainNow` verdadero, `TryDrain` drena siempre: en el registro no hay sesiones destruidas, así que las condiciones coinciden. Las pistas se vacían antes de invocar `Drained`.
- **No hay doble lectura de una misma petición.** `_scheduled` coalesce, y la pasada consume las pistas antes de cualquier bombeo posible.

### `EventBridgeCore` (`src/RackCad.Application/Workspace/EventBridgeCore.cs`)
- `TryGetDrainable` es el predicado exacto de la pasada: iniciado, sesión activa en `_states` y en el registro, no degradada, pistas > 0, reposo, sin modal y panel visible.
- `OnIdle` lo usa y sigue llamando a `TryDrain`. La refactorización es equivalente: `TryDrain` no tiene efectos cuando no se permite.
- `CanDrainNow` comparte ese predicado y añade en su propia sentencia `!_lastAllowedClose.HasValue` (cualquier cierre pendiente). No asigna nada, no nombra `_states` y contiene con `Log`.
- Se mantienen los 13 olvidos, y el de `OnIdle` sigue justo tras el retorno por no iniciado. La guarda `_lastAllowedClose[^;]*_states` no salta, y G-5 tampoco.

### F4-T2B2B
- La secuencia de cierre no se rompe: la continuación no elegible no llama a la pasada, así que no olvida, y `closeSequenceIntact` llega true.
- Que los tics de Idle ya no olviden es una consecuencia declarada de D-T2C-02 y D-T2C-03. Su latencia es R-T2C-02.
- `CanDrainNow` es una consulta, compatible con INV-F4T2B2B-04.

### Cableado del puente
- Un solo `new WorkspaceIdleScheduler(_core.CanDrainNow, RunIdleContinuation, LogFault)`, en el constructor.
- `_dispatcher = Dispatcher.CurrentDispatcher` se captura una vez en el constructor.
- Hay un solo `SetPost(action => _dispatcher.BeginInvoke(DispatcherPriority.Background, action))`, y se resuelve a la sobrecarga `(DispatcherPriority, Delegate)`.
- No hay `Invoke`, `InvokeAsync`, temporizadores ni frames. Se mantienen 16 manejadores, 16 `+=` y 16 `-=`, y 3 `+=` en el controlador.
- La comprobación Debug es `[Conditional("DEBUG")]` y usa `document?.TransactionManager.TopTransaction`, sin `.Database`. Registra con `LogFault` y no hay `Debug.Assert` en el puente.
- En Release: `using System.Diagnostics` sigue en uso por el atributo, y no hay ambigüedad con `RackCad.Application.Diagnostics` (solo `RackLog`) ni con `System.Windows.Threading`. No veo ninguna causa de aviso nuevo.

### Guardas existentes (comprobadas por inspección, no ejecutadas)
- Fachada: superficie pública del puente, `.Database` solo en hookups, cuerpos de T2, `_core.OnIdle()` y `Drained?.Invoke(` dentro de `OnIdle`.
- Lifecycle: 13 olvidos, 16/16/16, Veto, archivos sin escritura.
- C2 y paleta: avisos, orden en `DocumentActivated` y `DocumentToBeDestroyed`, metaguarda con el literal `PaletteHostPath`, 45 pruebas.
- `WorkspaceBridgeSourceGuardTests`: llamadas prohibidas, transacciones dentro de `using`, campos y superficie pública.
- `WorkspaceModelBoundaryTests` y las trampas de nombres de H-18.
- `TheApplicationWorkspaceFilesStayFreeOfAutoCadAndOfThePlugin`.

Ninguna salta con el GREEN.

### Cambios declarados y alcance
- Solo cambian los dos archivos declarados:
  - en la fachada, `CanDrainNow` en la lista y el comentario, con el nombre de la prueba conservado;
  - en el host, la excepción por archivo exacto con la constante `BridgePath` que ya existía.
- La expresión del puente es la del contrato: no quita nada salvo `\bDispatcher\b` y `System.Windows.Threading`, y añade `\w*Timer\b`, `DispatcherFrame`, `PushFrame`, `\bYield\b`, `InvokeAsync` e `Invoke` del dispatcher.
- La excepción de la paleta no cambia y el archivo sigue con 45 pruebas.
- El GREEN no toca `tests/`. No hay archivo nuevo en `Plugin/Workspace`, ni cambios en el controlador, el host de navegación, los lectores, la UI, los csproj o la CI.

### RED (`703bda9d`)
- Compila contra un esqueleto inerte: `CanDrainNow` devuelve false y el planificador no programa.
- Las pruebas positivas fallan por aserción: `BodyOf` y `Mutate` usan `Assert`, y se usa `Record.Exception` donde hace falta.
- Pasan en el RED, por naturaleza, las negativas y las autopruebas de expresiones: F-oculto, H sin post, 8 de los 9 casos de paridad, G-6, G-8 y las copias de G-3.
- Verifiqué que esas negativas sí discriminan implementaciones GREEN incorrectas:
  - un `CanDrainNow` que ignora la visibilidad, el reposo, el modal, las pistas o la degradación;
  - un `CanDrainNow` que solo mira el cierre de la sesión activa;
  - un `Run` que no vuelve a comprobar (E, F-2, G, H-2 y la del cierre dan `PassCalls` 1);
  - un `OnIdleTick` que ejecuta en línea (B y C por profundidad);
  - un post doble inmediato (`TheHarnessTellsAReadInside...`).

### Regla I-1
- Las guardas nuevas se limitan a cuerpos propios o a cotas:
  - G-1 tiene una cota inferior de 16 bloques;
  - los recuentos exactos (`OnIdle` 2, `BeginInvoke` 1, `CurrentDispatcher` 1, `new WorkspaceIdleScheduler(` 1) los pide el contrato.
- No prohíben nada propio de F4-T3 (envolver el asesor o el cierre, `SetModalRackCadActive` antes de `ShowModalWindow`) ni de F4-T4: G-8 amplía la lista de escritura solo en tres archivos donde la escritura ya estaba prohibida.
- G-9 fija `AssertNoOpenTransaction` por INV-14, que ya lo exige.

## Riesgos para el smoke con AutoCAD (no son defectos)

1. **El dispatcher bombea en bucles anidados.** El arnés modela el dispatcher solo a profundidad 0 (`RunQueue` exige `Depth == 0`), pero el `BeginInvoke` real puede ejecutarse dentro de cualquier bucle anidado del hilo principal:
   - otro suscriptor de Idle que llama a `DoEvents` en la misma notificación de Idle, con lo que la lectura queda dentro del callback de Idle;
   - diálogos o progresos de otros complementos;
   - modales de RackCad abiertos desde la paleta sin comando (arrastre a F4-T3);
   - avisos de LISP sin comando, como `(getpoint)` o `(entsel)`. Ahí el reposo en memoria es verdadero y el real es falso: `TryDrain` consume las pistas, `MayDrain`/`MayRead` y el resumen se niegan, y el contexto queda desfasado hasta la siguiente pista. Es preexistente si Idle también disparaba ahí, pero la ventana puede ser mayor.
2. **Cierre.**
   - La pregunta de guardar de CLOSE, el cierre por la pestaña o por la API, y QUIT/CLOSEALL con varios dibujos modificados: confirmar que `_lastAllowedClose` bloquea a tiempo y que el cierre por la pestaña emite CLOSE como comando.
   - El cierre cancelado sin comando (R-T2C-02): las pistas no se drenan hasta la siguiente entrada del núcleo.
3. **Cambio de documento.** Entre el cambio real de `MdiActiveDocument` y `DocumentActivated`, la continuación puede ver `_active` antiguo. El lector devuelve Failed o `MayDrain` lo niega, así que no hay lectura cruzada, pero las pistas se consumen sin leer. Hay que mirar también `LockDocument` desde la continuación durante un cambio o un cierre.
4. **Latencia.** `Background` queda por debajo de Input: con entrada continua (por ejemplo, ratón en movimiento) el drenaje puede retrasarse.
5. **Continuación perdida.** Si el dispatcher se cierra o se deshabilita, `IsScheduled` queda en true y no hay más drenajes. Los contadores `ContinuationsRun` y `ContinuationsSkipped` no son observables desde el Plugin, así que en Smoke-2 solo se vería el síntoma: el panel deja de actualizarse.
6. **Aserciones Debug existentes.** `WorkspaceController.cs:191` y `WorkspaceSelectionReader.cs:115` pueden llegar a correr dentro de un bucle anidado con una transacción ajena abierta. Con el DLL Debug, harían `FailFast` en AutoCAD. La comprobación nueva solo registra, y puede dar el falso positivo de N-5.
7. **Reentrada.** Si la actualización de la UI dentro del aviso bombea mensajes y llegan pistas nuevas, una segunda continuación puede correr anidada dentro de la primera. Su `CloseDrainNotice` en el `finally` cerraría antes de tiempo la ventana de aviso de la exterior.
8. **Temporización de pila en el host.** Sigue sin prueba (D-T2C-05), y el smoke no la demuestra. R-T2C-01 tampoco cambia: una superficie Ready no se refresca, así que conviene usar los disparadores que materializan del §6.
