# I-64 F4-T4C1-HOST, intento 2: re-revisión adversarial independiente del código de la corrección

- **Worktree:** `<worktree-de-I-64>` (ruta local redactada), rama `architecture/workspace-persistente-rackcad`, HEAD = CurrentSha `b2631084`.
- **SHA:**
  - intento 1: BaseSha `44eafbc1`, RED `8e57061e`, GREEN `271a045c`;
  - intento 2: BaseSha `12c248b0` (`git diff --name-only 271a045c 12c248b0` solo toca `docs/`, así que el producto es el de `271a045c`), RED `1268bb9d`, GREEN `b2631084`.
- **Método:**
  - solo lectura, con `git -C … show/diff/grep/blame` y la lectura de la carpeta de la corrida `artifacts/orchestration/I-64/F4-T4C1-HOST/2/R20261115T165352Z-3e8e/` (TRX, logs y `worker-handoff.json`);
  - sin `dotnet`, sin builds ni pruebas, sin checkout y sin escribir en el worktree.
- **Anclas:** son de `b2631084` salvo que se diga otra cosa.
  - `CORE` = `src/RackCad.Application/Workspace/WorkspaceHostCore.cs`;
  - `HOSTRUN` = `src/RackCad.Application/Systems/PushBack/Pilot/PushBackPilotHostRun.cs`;
  - `ADAPTER` = `src/RackCad.Plugin/Views/RackPilotUpdateCommandContext.cs`;
  - `WRITER` = `src/RackCad.Plugin/Views/RackPilotUpdateWriter.cs`;
  - `RUN` = `src/RackCad.Application/Systems/PushBack/Pilot/PushBackPilotUpdateRun.cs`;
  - `TW` = `tests/RackCad.Tests/Workspace/`, y dentro de esa carpeta:
    - `RQ` = `WorkspacePilotUpdateRequestTests.cs`, `HR` = `WorkspacePilotUpdateHostRunTests.cs`, `HD` = `WorkspacePilotUpdateHostDouble.cs`;
    - `IG` = `WorkspacePilotUpdateIntegrationGuardTests.cs`, `PEG` = `WorkspacePilotEditGuardTests.cs`;
    - `EIG` = `WorkspaceEditIntentGuardTests.cs`, `EIH` = `WorkspaceEditIntentHostTests.cs`;
    - `PUG` = `WorkspacePilotUpdateGuardTests.cs`, `LG` = `WorkspaceLifecycleGuardTests.cs`;
  - `RUNDIR` = la carpeta de la corrida de arriba;
  - `PKG` = `docs/initiatives/I-64-f4-t4c-gate-contract-package.md` (v5).

## Veredicto

**ACEPTABLE CON MENORES**: **B/I/M = 0/0/5** en hallazgos nuevos o no corregidos.

- **B-01 está corregido**, y sus dos pruebas fallan por aserción contra el producto del intento 1.
- **I-01 está corregido:** G-1 es un censo de todo `src/` con sus cuatro excepciones exactas, y G-10 ya no excluye carpetas.
- **M-01 a M-10 están corregidos.** Quedan cinco menores:
  - un hueco nuevo en la guarda del vuelo (N-01);
  - un texto visible nuevo en inglés (N-02);
  - una rama de M-06 (b) sin prueba (N-03);
  - M-11 a medias (N-04);
  - una omisión en la declaración del RED (N-05).
- **Regresiones:** no he encontrado ninguna en el billete, la cota, la llamada terminal, el almacén, el adaptador, el ámbito propio, R-T4-07, los arrastres de T4B, la frontera de Application ni T1B.
- **INV-F4T4C-22** se cumple en lo que he podido verificar.

## 1. Los hallazgos del intento 1, uno por uno

En la columna «¿Discrimina?», «r1» es la corrida local del RED: el TRX `RUNDIR/tests/r1/r1.trx` (árbol `cdf146ff` = árbol de `1268bb9d`, con el producto de `271a045c`). Tiene 3421 pruebas, 3361 en verde y 60 en rojo.

| Id | Estado | Corrección en el producto | ¿Discrimina? (r1 frente a `271a045c`) | Evidencia |
|---|---|---|---|---|
| **B-01** | **Corregido** | El `finally` de `Execute` pone `InExecute = false` junto a `Finished = true` si el testigo es el vigente. `ExpireStaleUpdate` libera entonces el billete terminado con el primer comando ajeno posterior o con la degradación (`starts` nulo), y sale sin guardar nada (`if (ticket.Finished) return null;`). El cierre y la retirada siguen por `ForgetUpdatesOf`. Durante `Execute` el billete no caduca | **Sí**, las dos fallan por aserción. **(a)** `ATicketThatEndedWhoseCompletionNeverArrives…`: «Expected Discarded, Actual Busy». **(b)** `AForeignCommandDuringTheUpdate…`: «Expected Accepted, Actual Busy». **(a)** comprueba además el `Busy` antes de la cota y que el resultado `Applied` sigue y se recoge una sola vez: si se guardara un segundo resultado, `Take` lo vería. **(b)** comprueba el vuelo durante `Execute` (`during.InFlight`), el `Busy` reentrante, la liberación en la comprobación siguiente y el `Applied` sin un segundo resultado | `CORE:1650-1659`, `:1453`, `:1458`, `:1511-1522`; `RQ:578-609`, `:612-640`; r1 |
| **I-01** | **Corregido** | Solo pruebas. **G-1:** censo de todo `src/` (`Src.AllProduct()`). Un archivo que nombra uno de los tres `PushBack*DrawService` y llama a `.PrepareRedraw(` o `.RedrawInTransaction(` solo puede ser uno de los tres servicios o `WRITER`. `WRITER` conserva la posición en `PrepareUnits` y `WriteUnit`, y la ausencia de un servicio también dispara la guarda. **G-10:** sin carpetas excluidas. Las cuatro ventanas son excepciones por archivo exacto, con cuenta 1, y el gesto del VM queda admitido para F4-T4C2. Medido en `b2631084`: los archivos que nombran las fachadas y llaman a las primitivas son exactamente los tres servicios y `WRITER`. Los `.RequestUpdate(` están en los dos reenvíos, en las cuatro ventanas (1 cada una) y en la implementación explícita del núcleo, que la guarda excluye | La regla pasa en r1, como declara la entrega: es código de prueba y el producto ya la cumplía. Las negativas son reales: un archivo nuevo en `Plugin/Views`, `Plugin/Drawing` o `UI/Editor`; `RackPushBackCommands.cs`; un servicio de Selective; una fachada que falta; las carpetas antes excluidas; una cuenta de 2; una ventana que pierde su `RequestUpdate` | `IG:282-311`, `:330-347`, `:725-760`, `:776-795` |
| M-01 | **Corregido**, con N-01 en la guarda | El vuelo pasa a `TryConfirmPilotBase`, `TryKeepPilotEdit` y `TryDiscardAndReloadPilotEdit`, después de `PassGate`. Los reenvíos del puerto solo marcan la lectura. `PassGate` no tiene efectos (`CORE:1909-1943`) | **Sí.** Fallan `TheHostImplementsThePortExplicitly…` («… does not look at the update in flight»), las teorías del vuelo y `ARequestOfASessionThatIsNoLongerTheActiveOne…` («Expected Rejected, Actual Waiting»). `FirstAuthorization` toma el mínimo de `Authorize(` y `PassGate(`, y un cuerpo sin autorización dispara la guarda | `CORE:564`, `:647`, `:735`, `:1048-1088`; `PEG:82-100`, `:316-329`, `:494-526`; `RQ:760-776` |
| M-02 | **Corregido**, con una reserva (O-3) | Solo pruebas. El censo de portadores se deriva del ensamblado por la familia de nombres y se exige completo, con negativas que quitan un portador del censo | Pasa en r1, declarado | `EIH:958-990` |
| M-03 | **Corregido** | Solo pruebas. `FlightTextBodies` cubre los siete cuerpos con la constante y sin literales, con una negativa por cuerpo y otra que sustituye la constante | **Sí:** 11 filas en rojo. Cuatro filas nuevas pasan sin estar declaradas: N-05 | `EIG:1292-1375` |
| M-04 (+ D-2) | **Corregido**, con N-02 | `HostNotRunReason` se usa entero en el corredor y en `Begin` del núcleo, y la excepción va al registro. `CORE` ya no tiene el `" "` en `PilotUpdateRun`. `TryOpenTarget` devuelve `PushBackPilotUpdateRun.DocumentMismatchReason`, sin literales. Respecto a `44eafbc1`, el único `" "` de `CORE` es el de T4B en `TryUpdatePilot`, que es anterior | **Sí:** fallan HR (Begin y CreateWriter), RQ (`failure is "Begin" or "CreateWriter"`), `M04…` y la guarda del adaptador | `HOSTRUN:256-264`; `CORE:1613`; `ADAPTER:68-85`; `IG:148-159`, `:243-253`, `:988-1018`; `HR:243-297` |
| M-05 | **Corregido** | La respuesta del cierre no aceptado, `NotePilotEdit` y `_knownClean.Remove` entran en el `try` de la respuesta. El respaldo del no aceptado es `ClosedInError(UpdateNotClosedReason…)`: desviación declarada (O-2) | **Sí:** fallan `M01TheClose…` y `M05TheCloseGuard…` | `CORE:894-928`; `IG:913-984` |
| M-06 (a) | **Corregido** | `Execute` sale sin nada si el billete ya terminó (`ticket = null; return;`, así que el `finally` no actúa), y `NotRun` sale si `Finished` | **Sí:** «Expected Applied, Actual NothingToApply». Cubre `Execute` dos veces, `NotRun` tras `Execute` y `NotRun` dos veces | `CORE:1625-1629`, `:1667`; `RQ:664-707` |
| M-06 (b) | **Corregido**, con N-03 | Si `TryRequest` lanza y el billete ya no está, o está empezado o terminado, el valor de inicio es `Accepted`. Si no, `Failed` y se quita el billete | **Sí:** «Expected Accepted, Actual Failed». La contraria (`…ThrowsBefore…`, modo diferido) pasa en r1 y está declarada | `CORE:1395-1406`; `RQ:710-742`; `HD:241`, `:256` |
| M-06 (c) | **Corregido** | `IsAnyPilotDraftApplying` registra y responde `false` en su `catch`. Durante el vuelo, `DiscardDocument` sigue en `Busy` por `_tickets` | **Sí**, por texto: «the catch does not keep the exception to log it». No es una prueba de comportamiento: el registro no se puede inyectar (limitación declarada) | `CORE:1495-1508`, `:1257-1259`; `RQ:744-755` |
| M-06 (d) | **Corregido**, con N-02 | Un `Lock()` nulo lanza dentro de `locking` y termina en `NotRun(Failed, LockFailedPrefix + …)`, sin `Begin`, sin escritor y sin nada que liberar | **Sí:** «Expected 0, Actual 1» (`Begins`) | `HOSTRUN:241-245`, `:256-264`; `HR:263-280`; `HD:72`, `:89` |
| M-07 | **Corregido** | Solo pruebas. G-4 cuenta `\bTryUpdatePilot\b` sobre el código sin cadenas: 2 en el núcleo y 0 fuera, con la negativa del grupo de métodos en tres archivos. La guarda del adaptador ancla `OnCompleted` tras la rama `if (result.IsCompleted) {…return;}` y `Post` a `context` mediante `Hop`, con 2 llamadas `Hop(context, completed)` | G-4 pasa en r1, declarado. La guarda del adaptador falla (por M-04 en el mismo cuerpo) | `IG:485-500`, `:520-532`, `:148-154`, `:226-241` |
| M-08 | **Corregido** | La entrega lista frente a D-T4C-20 las tres que faltaban: el `InlineData` de la fila 5, que ahora vuelve exactamente al de `44eafbc1`; `ForeignCommandStartsOf` en la fila 15; y la expresión de `WriterProhibitionViolations` en la fila 13 | — | `worker-handoff.json`, `Evidence` («CAMBIOS DE GUARDA») y `KnownLimitations` [2] |
| M-09 | **Corregido** | `TestsExecuted` tiene 79 corridas, que son exactamente las 79 carpetas con TRX de `RUNDIR/tests`. Las cuatro carpetas de builds (`b0`, `r0`, `g2` y `gr`) se declaran en `Evidence` («BUILDS», «CORRIDAS LOCALES»). Los `TreeSha` son `cdf146ff` en las del RED y `bcc39e84`, el árbol de `b2631084`, en las del GREEN | — | `worker-handoff.json`; `RUNDIR/tests/` |
| M-10 | **Corregido** | `Started` se escribe en `Admit` y se lee en `RequestUpdate`. `WRITER` pierde `using AcApplication` (estaba en `271a045c:…RackPilotUpdateWriter.cs:19`), sin otros usos. Se restaura el comentario, idéntico al de `44eafbc1:…:837`. Se renombran las dos pruebas, y los nombres nuevos describen lo que afirman | Falla `TheWriterDoesNotKeepTheAlias…`. Los renombres pasan | `CORE:1541`, `:1590`, `:1402`; `TW/WorkspacePilotUpdateRunTests.cs:891`; `TW/WorkspaceBridgeFacadeSourceGuardTests.cs:233`; `PEG:633`; `PUG:267-275` |
| M-11 | **Parcial** (N-04) | Los xUnit2029 de OM, HR e IG se corrigen (`Assert.DoesNotContain`). Queda un aviso nuevo de T4C1 | — | `RUNDIR/tests/gr/build-core-tests.log`; `LG:557` |

## 2. Hallazgos nuevos o no corregidos

| Id | Gravedad | Contenido | Evidencia | Corrección propuesta |
|---|---|---|---|---|
| **N-01** | M | **La guarda de la fila 5 dejó de mirar los tres reenvíos del puerto, y no prohíbe el vuelo en ellos.** `FlightBodies` pasa de `IWorkspacePilotEditPort.TryConfirmBase`, `TryKeepMine` y `TryDiscardAndReload` a los cuerpos públicos `Try*Pilot*`. `ReadGateViolations` solo exige fragmentos de los reenvíos (`Contains` e `IsMatch`), no que sean exclusivos. Así, el defecto exacto de M-01 se puede reintroducir sin que nada salte: un `if (UpdateInFlightForRequest(request)) return …Waiting(…)` al principio de un reenvío, antes de `_pilotReadInProgress` y de `PassGate`, conservando la comprobación interna. No lo detecta `HostViolations` ni `ReadGateViolations`, y tampoco `FlightTextViolations`, que solo lee los cuerpos internos. Las negativas nuevas solo mutan los cuerpos internos. La guarda del intento 1 sí exigía, en el reenvío, el vuelo después de la marca. Es una relajación no declarada de una guarda de la fila 5 que cambia en esta tarea; no la cuento como C07 porque la fila sigue siendo [1] y el producto cumple. Lo he comprobado por lectura de las guardas que nombran el vuelo (`git grep` de `UpdateInFlightFor`, `FlightCheck` y `ExpireStaleUpdate` en `tests/`), no ejecutando | `PEG:82-93`, `:316-329`, `:478-526`, `:541-563`; `EIG:1292-1302`; `CORE:1048-1088` | En `HostViolations`, exigir que los tres reenvíos no contengan `FlightCheck`, o una sola comprobación entre el reenvío y el cuerpo interno, con la negativa del vuelo inyectado al principio de un reenvío |
| **N-02** | M | **La defensa M-06 (d) añade un texto visible nuevo, en inglés.** `HOSTRUN:244` lanza `InvalidOperationException("The target did not answer a lock.")` con `locking == true`, y la causa es `LockFailedPrefix + ex.Message` (`:260-262`). El usuario leería «No se pudo bloquear el documento: The target did not answer a lock.». Es un literal del producto fuera de los seis textos ratificados (D1): la clase de texto que M-04 quitaba. La prueba solo mira `StartsWith(LockFailedPrefix)`, y la entrega («TEXTOS DEL HOST») justifica el prefijo para un bloqueo que lanza, pero no declara este literal. Es inalcanzable con ADAPTER, porque `LockDocument()` no devuelve nulo | `HOSTRUN:241-245`, `:256-264`; `HR:263-280`; `worker-handoff.json` («TEXTOS DEL HOST») | Declararlo, o componer el prefijo con un texto ratificado (o sin causa), y fijar la causa exacta en `HR` |
| **N-03** | M | **M-06 (b): la rama «empezado o terminado» no tiene prueba.** `AnExecutorThatThrowsAfterTheCallbackBegan…` corre en modo `Synchronous`: el doble llama al callback y a la compleción dentro de la petición (`HD:169-174`), así que cuando el ejecutor lanza (`HD:256`) el billete ya no está y solo se ejerce `current == null`. El caso que ordena M-06 (b), con el callback hecho y la compleción pendiente (el diferido que midió H0, con el billete presente y `Started` o `Finished`), no lo ejerce ninguna prueba. El mutante `if (current == null) return EditUpdateStart.Accepted();` pasaría la suite. La contraria (`RQ:729`) solo fija «no empezado». Por lectura, el producto es correcto | `CORE:1401-1402`; `RQ:710-742`; `HD:169-174`, `:256` | Un `ScriptedExecutor` que haga `Admit`, `Begin` y `Execute` (o `NotRun`), lance sin `Completed` y espere `Accepted`, con el resultado una sola vez |
| **N-04** | M | **M-11 a medias: sigue un aviso nuevo de T4C1.** `gr/build-core-tests.log` da `WorkspaceLifecycleGuardTests.cs(557,9): warning xUnit2012`. La línea `Assert.False(PortMutatingOperations.Any(…))` la introdujo el RED del intento 1 (`git blame` → `8e57061e`), no existe en `44eafbc1` y el archivo está en el alcance. INV-F4T4C-21 pide «ningún otro aviso nuevo de compilación». La entrega dice «todos ya presentes antes», lo que solo es cierto frente a `271a045c`. Los otros dos avisos en archivos de la tarea son anteriores a `44eafbc1`: `EIG:991` y `TW/WorkspaceNavigationHostTests.cs:722` (`:690` en la base) | `RUNDIR/tests/gr/build-core-tests.log`; `LG:557`; `git blame` | `Assert.DoesNotContain(PortMutatingOperations, p => …)`, y corregir la frase de la entrega |
| **N-05** | M | **El RED no declara todas las pruebas nuevas que pasan.** En r1, cuatro filas de la teoría nueva `TheFlightTextGuardTripsWhenTheConstantOfTheFlightIsReplacedByALiteral` pasan: `SetPendingText`, `CommitField`, `DiscardRack` y `DiscardDocument`. Es lo esperable, porque esos cuerpos ya tenían la constante. `Evidence` («RED, pruebas nuevas o corregidas que ya pasan») no las nombra, e INV-F4T4C-22 solo admite que pasen «si la entrega lo declara». No afecta a las pruebas obligatorias, que fallan todas | r1; `EIG:1347-1375`; `worker-handoff.json` (`Evidence`) | Añadirlas a la declaración |

## 3. Regresiones frente a lo que el intento 1 tenía bien

| Área | Resultado | Evidencia |
|---|---|---|
| Billete y testigo | Sin cambios. El testigo protege también la compleción tardía de un billete que B-01 liberó, si ya hay uno nuevo en la sesión (`CurrentTicket` por testigo en `Completed`) | `CORE:1386-1387`, `:1442-1443`, `:1690-1691` |
| La cota | Mejora (B-01). En `Execute` el billete nunca caduca. Antes de `Execute`, igual que antes. Después, se libera sin un segundo resultado. `InFlight` sigue siendo «hay billete y no está terminado» (`PKG:468`) | `CORE:1429`, `:1449-1468` |
| La llamada terminal única | Sin regresión: el cerrojo del corredor no cambia y el núcleo la refuerza (M-06 (a)) | `HOSTRUN:198-232`; `CORE:1625`, `:1667` |
| El almacén | Sin cambio de semántica. Una fuente por camino: `Execute`, `NotRun`, la caducidad de un no terminado y `Completed` sin callback | `CORE:1431`, `:1460`, `:1644`, `:1670-1672`, `:1696` |
| El adaptador | Solo cambia `TryOpenTarget`. La captura del planificador, la llamada única, `OnCompleted`, `Post`, `Target` y la totalidad siguen igual | `ADAPTER:23-123` |
| El ámbito propio (IC-09) | `EventBridgeCore.cs`, `WorkspaceSession.cs`, `AutoCadEventBridge.cs` y `WorkspaceController.cs` no cambian en el intento 2 (`12c248b0..b2631084`: 15 rutas) | `git diff --name-only` |
| R-T4-07 | Sin regresión: `Busy` en los dos descartes durante el vuelo y sobre un `Applying`. M-06 (c) solo quita el `Busy` sin cota ante un registro que falla | `CORE:1231-1232`, `:1257-1259` |
| I-01, M-01, M-03 y M-06 de T4B | `RUN` y `WorkspaceEditIntent.cs` sin cambios. `TryUpdatePilot`, frente a `44eafbc1`, solo con M-01, M-03 y M-05 | diffs `44eafbc1..b2631084` |
| `UpdateGate` | Sin cambios en el intento 2. Frente a la base, solo las constantes de I-01 | idem |
| `WRITER` | Solo pierde el `using` (D-T4C-07) | diff `1268bb9d..b2631084` |
| La frontera de Application | Las líneas nuevas de Application no nombran tipos de AutoCAD (`InvalidOperationException`, `Report`) | `HOSTRUN:244`; `CORE` |
| T1B | Ninguna ruta de la clausura, de las fachadas, de `KindHandlers/` ni de los `*Commands*.cs`. Las clases de regresión en verde en local (`g301`-`g323`, `h302`-`h324`) | `RUNDIR/tests` |
| El orden de M-01 en las tres lecturas | Cambio de comportamiento buscado: una sesión no activa recibe `Rejected` y no `Waiting`. Durante el vuelo, el borrador no cambia y no se lee nada | `RQ:760-776` |

## 4. INV-F4T4C-22

| Requisito | Resultado | Evidencia |
|---|---|---|
| El RED solo cambia `tests/` | **Cumple**: 11 rutas en `12c248b0..1268bb9d` | `git diff --name-only` |
| El GREEN solo cambia `src/` | **Cumple**: 4 rutas en `1268bb9d..b2631084` | idem |
| El RED falla por aserción contra `271a045c`: las dos de B-01, la guarda de M-01 y las de M-06 | **Cumple.** En r1 todas las pilas de los 60 fallos empiezan en marcos de `RackCad.Tests`, y ninguna excepción sale del producto. Las obligatorias fallan con los mensajes de la tabla 1 | r1 |
| Las demás nuevas o corregidas pueden pasar si se declaran | **Cumple, salvo N-05** | r1; `Evidence` |
| Core de Workspace con `ExpectRed` true y UI con `ExpectRed` false | **En local, cumple:** r1 en rojo; `u201` 601/601 y `u202` 55/55. Las 23 de regresión en verde en el RED | `RUNDIR/tests` |
| El alcance de 30 rutas | **Cumple.** `44eafbc1..b2631084` sin `docs/` = el `AllowedWriteScope`, como conjuntos iguales. Ninguna ruta toca las 163 entradas prohibidas (comprobado por programa) | contrato del intento 2 |
| Ninguna guarda fuera de las filas [1] (C07) | **Cumple.** Las filas tocadas son la 4, 5, 6, 11, 13, 14 y 19. OM, HR, RQ y HD son archivos creados por la tarea; en OM solo hay xUnit2029, equivalente. `WorkspacePilotUpdateRunTests.cs` solo recupera el comentario. La prueba nueva del alias en `PUG:267-275` (M-10) endurece la fila 13 y no relaja nada. La única relajación es N-01, dentro de la fila 5 | diff `12c248b0..1268bb9d` |
| `RUN`, `WRITER`, `TryUpdatePilot` y `UpdateGate` dentro de sus líneas (C14) | **Cumple** | tabla 3 |
| Sin la suite completa de UI en local | **Cumple**: solo los dos filtros (`v301`, `v302`, `w301`, `w302`) | `TestsExecuted` |
| Sin proceso en segundo plano | Hay una desviación declarada (O-7), de proceso | `Deviations` [4] |

## 5. La entrega

- **`FilesChanged`:** 15 rutas, iguales al diff `12c248b0..b2631084`.
- **El trailer** (Sonnet 5.5) coincide con el `Worker`.
- **La lista de cambios de guarda (M-08)** está completa frente al diff. La fila 15 se declara sin cambio nuevo, y es correcto.
- **Las corridas locales (M-09)** están todas declaradas.
- **Faltan tres cosas:**
  - las cuatro filas que pasan en el RED (N-05);
  - la frase sobre los avisos (N-04);
  - el literal de N-02.
- **`OpenQuestions` vacía:** es razonable, porque las desviaciones son interpretaciones declaradas.

## 6. Observaciones (no cuentan en el recuento)

- **O-1, la cobertura de B-01:** solo se prueba el comando ajeno. La degradación, el cierre y la retirada de un billete terminado no tienen prueba propia, aunque comparten el mismo camino (`CORE:1453-1458`, `:1511-1522`). La compleción tardía de un billete liberado por B-01, con uno nuevo ya aceptado, la cubre el testigo por construcción. No la prueba nada, porque el doble solo guarda el último callback. El contrato exige solo (a) y (b).
- **O-2, la desviación de M-05:** para un cierre no aceptado de tipo `NotApplied*`, la ruta principal responde `Closed(NotAppliedFailure, UpdateNotRunReason)` (`CORE:962-964`) y el respaldo `ClosedInError(UpdateNotClosedReason, status…)` (`CORE:922-923`). Las dos formas son coherentes en no ser un éxito, pero no son iguales. Está declarado y es prácticamente inalcanzable.
- **O-3, M-02:** deriva los portadores por el patrón `^Edit(Intent|Check|Confirm|Update)(Request|Witness|Outcome|Start|Progress)$` (`EIH:966-975`), no por las firmas del puerto, como proponía la revisión. Un portador nuevo con otro nombre escaparía. Cumple la letra de INV-F4T4C-21.
- **O-4, G-1:** es «nombra una fachada y llama a una primitiva en el mismo archivo». Dos archivos (uno que expone la fachada y otro que llama a `.PrepareRedraw(` sin nombrarla) lo esquivarían. Coincide con la medida de la revisión y con la letra del contrato: es un hueco teórico de una guarda de texto.
- **O-5, `WithoutStrings`:** no trata las cadenas literales `@"…"` ni el carácter `'"'` (`IG:485`). Es teórico.
- **O-6:** la desviación de M-06 (b) dice que «el billete que ya no está solo lo quita una compleción». También lo quitan la caducidad dentro de `Admit` (`CORE:1569`) y `ForgetUpdatesOf`. En los dos casos hay un resultado guardado o ya no hay sesión, así que `Accepted` sigue siendo un solo canal. Es una cuestión de redacción.
- **O-7:** la entrega declara que la herramienta pasó a segundo plano tres órdenes largas, que el Worker esperó. Al cerrar la cesión, la foto de procesos de `RUNDIR` solo tiene `bash`, `pwsh` y el servicio del sandbox, sin `dotnet` ni `testhost`. Si eso cumple «ni proceso en segundo plano» lo decide el Controller.
- **O-8:** `Begin` no mira `Finished` (`CORE:1606`). Solo es alcanzable con un anfitrión que no respete el cerrojo, y M-06 (a) solo pedía `Execute` y `NotRun`.

## 7. Lo no verificado

- **La CI exacta** de `1268bb9d` y `b2631084` (los cuatro jobs): no la consulté. El conector de GitHub no estaba disponible y la orden excluye ejecutar nada.
- **Que las corridas locales se hicieran sobre los árboles declarados:** solo es coherente con los `TreeSha` y con las horas. r1 corrió de 11:09 a 11:11, antes del commit del RED (11:16:00). g1 corrió de 11:17 a 11:19 y gf terminó a las 11:27, antes del commit del GREEN (11:31:54).
- **La compilación y los avisos:** solo he leído los logs de `RUNDIR/tests` (`b0`, `g2`, `gr` y `r0`); no he compilado. N-04 se basa en el log y en `git blame`.
- **N-01 y N-03** se basan en la lectura de las guardas y de los dobles, no en ejecutar mutantes.
- **El comportamiento en AutoCAD** del adaptador, de `LockDocument` y de la compleción es de Smoke-2.
- **Fuera de esta revisión de código:** la replanificación de la delegación (`R20261115T164833Z-353a`), el `prompt.md`, nc1 a nc3 y la custodia.
