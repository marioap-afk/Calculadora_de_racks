# I-64 F4-T4C — Revisión adversarial independiente del paquete v1 (antes de emitir)

**Veredicto:** EMITIBLE TRAS CORRECCIONES. **Recuento:** 0 bloqueantes, 12 importantes y 16 menores (§3 y §4).

## 1. Objeto, método y lo que no se pudo verificar

**Objeto:**
- el paquete v1 de F4-T4C, todavía sin emitir;
- la orden del Owner de preparación de T4C, que se cita como `OO`;
- los mapas M1 a M4, consultados solo para cruzar afirmaciones.

**Base:** `b15072ff`, con el producto de `4314c180`.

**Método:**
- se leyeron enteros el paquete y la orden;
- todo lo que funda una decisión se comprobó en el código con lecturas y con `git grep`, `git show` y `git ls-files`. No se compiló, no se ejecutó ninguna prueba y no se escribió nada en el worktree;
- **las cuentas de `RequiredTests`** se recontaron sobre el TRX de la CI `43668817337` (`artifacts/orchestration/I-64/F4-T4B-MUTATE-POST/1/R20261115T012820Z-7a4a/ci-core/core.trx`). `relay-record.json`, en el mismo directorio, liga esa CI a `4314c1803b7a…`. El recuento usó el script de solo lectura por clase de M4.

**Notación:**
- `P:n` es la línea n del paquete v1, que está fuera del repositorio;
- el resto de las rutas son relativas al repositorio;
- `CORE`, `RUN`, `WRITER`, `CTL`, `EBC`, `DS`, `FVM`, `TW` y `UW` son las abreviaturas del paquete.

**Lo que no se pudo verificar:**
- **todo lo marcado [H], que solo mide AutoCAD:**
  - el momento del callback;
  - si `EXECUTEFUNCTION` produce `CommandWillStart`, `CommandEnded` o `CommandCancelled`, y en qué orden;
  - si el `Task` de `ExecuteInCommandContextAsync` siempre se completa;
  - la agrupación de Undo;
- que los cambios propuestos compilen;
- que el archivo nuevo de `Plugin/Views` no haga saltar ninguna guarda de todo el Plugin. Se revisaron las que nombran `ExecuteInCommandContextAsync`, `LockDocument`, `Task`, `async` y los censos de carpeta, pero no todas las guardas una por una;
- el detalle por token de las reglas de UI de reconciliación (`TW/WorkspacePilotReconciliationGuardTests.cs`). Solo se comprobaron sus anclas;
- **las cuentas de las 21 clases de regresión:**
  - seis se recontaron directamente en el TRX: `CallerOwnedFacadeGuardTests` (81), `CustomPropertiesCommandGuardTests` (48), `SelectiveEditorOpenTests` (33), `WorkspacePilotUpdateGuardTests` (79), `WorkspacePilotUpdateHostTests` (35) y `WorkspacePilotUpdateRunTests` (55);
  - las demás salen de la salida de M4, que es coherente con esas seis.

---

## 2. Revisión punto por punto

### 2.1 El cumplimiento de la orden, apartado por apartado

| Apartado de `OO` | Dónde lo trata el paquete | Juicio | Hallazgos |
|---|---|---|---|
| PRIMARY OBJECTIVE | D-T4C-01; §3 | Cumple | — |
| AUTHORITATIVE MUTATION LOGIC | D-T4C-01, D-T4C-13 y D-T4C-14 | Cumple con reservas: T4C amplía archivos de T4B más allá de los arrastres | M-03, M-13 |
| EXECUTE IN COMMAND CONTEXT | D-T4C-04 y la guarda de `TW/WorkspaceNavigationHostTests.cs:436` | Cumple. La excepción es estrecha, pero la guarda del adaptador deja huecos | R-08 |
| DOCUMENT LOCK | D-T4C-04, pasos 2 a 4; D-T4C-08 | Cumple: el mismo documento se resuelve, se bloquea y recibe el escritor | R-04, M-09 |
| FINAL REVALIDATION | D-T4C-08 | **Débil:** el borrador puede cambiar entre la aceptación y el callback, y solo se declara | R-03 |
| RUN-SCOPED WRITER | D-T4C-06 y D-T4C-07 | Cumple con D-1. Pero el `Document` pasa por una interfaz de Application | R-04, M-14 |
| ONE ACTUALIZAR | D-T4C-02 | Cumple en lo esencial. Faltan el doble clic y el censo de llamadores | M-11, M-12, R-12 |
| T4B I-01 | D-T4C-13 | Cumple. Además añade seis textos nuevos a `RUN` | M-13 |
| M-01, M-03 y M-06 | D-T4C-13 | Cumple: dueño, cierre y guarda de cada uno | M-02, M-03 |
| T4A2 DIAGNOSTIC | D-T4C-12; O-10 | Cumple | M-15 |
| COMMIT FAILURE | D-T4C-05 | **Débil:** hay un camino que presenta como `Failed` una corrida que sí escribió | R-02 |
| POST | D-T4C-05 | Cumple | — |
| OWN-MUTATION | D-T4C-09, IC-09 y O-02 | **Débil:** el cierre no tiene cota estructural, y el residual choca con un oráculo vinculante y con guardas | R-05, R-06, R-07, M-01 |
| UNDO | D-T4C-15 y D-T4C-16 | Cumple: identifica los cuatro puntos y no afirma el cumplimiento | M-11 |
| Las 20 pruebas | D-T4C-17 | Cumple el mapeo. Faltan algunos dobles o casos | M-08, M-09, M-10 |
| NO SECOND WRITE PATH | D-T4C-14 | **Débil:** no lo prueba para todo el Plugin | R-12 |
| T1B COMPATIBILITY | D-T4C-18 | Cumple | — |
| PACKAGE REVIEW | §10 | Cumplidos los pasos 1 a 8. El 9 es esta revisión, y quedan el 10 y el 11 | — |
| RETURN BEFORE EMISSION | §9 | Cumple la estructura. Le faltan decisiones | R-10 y §2.9 |

### 2.2 El modelo tolerante a un callback diferido (D-T4C-02 a D-T4C-05)

**¿Dos corridas de T4B por un Actualizar, o una corrida sin admisión?**
- **En el diseño, no:**
  - `Execute` solo va tras `Admit` (P:362-372);
  - un callback repetido no hace nada (P:363);
  - un segundo clic responde `Busy` (P:283).
- **Hueco:** falta un cerrojo terminal único en el corredor. Si `OnCompleted` ya llamó a `NotRun` y el callback llega después, el orden de P:362-366 vuelve a evaluar y puede llamar a `NotRun` otra vez (M-14).

**¿Puede quedarse puesta la marca en vuelo?**
- **Sí,** si el callback no corre y el `Task` no se completa. La marca solo se quita en `Execute`, en `NotRun`, con el rechazo del ejecutor o con la destrucción (P:278-282). `OnCompleted` depende de que el `Task` termine (P:374).
- Con D-T4C-10, mientras dura la marca, `DiscardRack` y `DiscardDocument` responden `Busy` (P:611-612).
- El cierre se veta mientras haya un borrador en riesgo (`src/RackCad.Application/Workspace/WorkspaceHostCore.cs:1378-1392`), y la UI deshabilita la caja (P:551).
- **El resultado:** un documento que no se puede actualizar, ni descartar, ni cerrar sin perder el trabajo (R-01).
- H0 no mide si el `Task` siempre se completa: no está en P:798-806.

**¿Puede quedarse abierto el ámbito propio?**
- Solo si no llegan ni `CommandEnded`, `CommandCancelled` o `CommandFailed`, ni `EnteringQuiescentState`.
- En ese caso el siguiente comando del usuario queda enmascarado (R-05).

**Los caminos uno por uno:**
- **Excepción síncrona de la llamada:** `NotRun(Failed)` (P:360). **Defecto** si el callback ya corrió de forma síncrona: se presenta `Failed` después de una escritura real (R-02 (a)).
- **Tarea fallida o cancelada:** `OnCompleted` y `NotRun(Failed, HostNotRunReason)` (P:374). Correcto, salvo el cerrojo terminal (M-14).
- **Callback que no llega y `Task` sin terminar:** R-01.
- **Callback que lanza:** P:370-372 cubre el `finally`. No exige que ninguna excepción salga hacia AutoCAD: ni la de la lambda de `EnterCommandContext` (P:381), ni la de `NotRun`, ni la del registro (M-10).
- **Documento cerrado entre el clic y el callback:** `SessionDestroyed` quita la marca y el resultado, y `TryResolveActive` falla. Correcto. Falta el caso del id nativo reciclado, que `src/RackCad.Application/Workspace/WorkspaceSession.cs:18` declara posible: lo para `Admit`, pero ninguna prueba lo fija (M-09).
- **Sesión retirada:** igual que el anterior. Correcto.
- **Segundo clic:** `Busy` mientras dura el vuelo. Después de una corrida síncrona, un clic ya encolado puede pedir otra entrada en el contexto de comando, porque el gesto no comprueba el botón como hace `CheckNow` en `FVM:397-399` (M-11).
- **CLOSE, CLOSEALL y QUIT durante el vuelo:**
  - el veto por Dirty y `Busy` en «Descartar» se resuelven cuando corre el callback;
  - con la marca colgada, no se resuelven (R-01);
  - la continuación de QUIT sobre un núcleo con la sesión destruida es inocua, siempre que `NotRun` ignore las sesiones muertas (prueba 3 (b)).

**¿Se presenta siempre el resultado, y una sola vez?** No está garantizado (R-02):
- **(b)** `TryRequest` en falso con un `NotRun` que guarda un resultado: el valor de inicio `Failed` y el resultado guardado se presentan dos veces (P:263-269 y P:274);
- **(c)** en el caso síncrono, `CommandWillStart` y `EnteringQuiescentState` dan `NotifyStateChanged` dentro de la llamada (`CTL:217-219`, `:250-265`; `EBC:317-320`). El repintado anidado consume el resultado antes del `TakeUpdateProgress` del gesto (P:640), y el gesto recibe `InFlight = false` con `Unavailable` sin ninguna regla para ese caso;
- **(d)** «Consume» (P:272) contradice «el resultado sigue guardado … cuando vuelva a estar viva» (P:657), salvo que el consumo dependa de `IsMine`.

**¿Un Unknown puede acabar como fallo o como éxito?**
- Como fallo, sí: R-02 (a).
- Como éxito, en un camino hoy inalcanzable: M-03, con `!closed.Accepted` tras el sello.

**¿Se limpia Dirty solo desde el estado verificado de T4B?** Sí. T4C nunca toca el borrador (P:432-434), y `TryUpdatePilot` es la única vía (`CORE:851-905`).

### 2.3 El contexto de comando y el bloqueo (D-T4C-04 y D-T4C-08)

**¿Se bloquea exactamente el documento que se muta?** Sí:
- `TryResolveActive` exige `== MdiActiveDocument` (P:383);
- el bloqueo y el escritor reciben ese mismo objeto (P:366-368);
- `TryUpdatePilot` relee el id por `HostProbe`, que lee `MdiActiveDocument` en vivo (`CTL:356-363`), dentro de la misma pila síncrona;
- el ámbito vuelve a comparar el id (`WRITER:68-72`);
- los bloqueos anidados de `CaptureDrawing` y `SyncName` son del mismo documento (`src/RackCad.Plugin/Views/RackSiblingScan.cs:103`, `src/RackCad.Plugin/Drawing/RackBlockRenamer.cs:32`).

**TOCTOU:**
- **Documento, sesión, modal y lectura en curso:** se revalidan en el callback (P:535-547). Correcto.
- **El borrador:**
  - `SetPendingText`, `CommitField`, `CrossBoundary` (cambio de pestaña o de rack), `TryKeepMine` y `TryDiscardAndReload` siguen admitidos durante el vuelo (`CORE:1045-1153`; P:549-552);
  - el Actualizar aplica el estado del callback, que puede no ser el del clic;
  - el paquete lo declara, pero no lo lleva al §9 ni lo prueba (R-03).
- **Entre el callback, el bloqueo y MUTATE:** es una pila síncrona en contexto de comando, y la relectura de MUTATE es la autoridad. Correcto.

**La excepción por archivo:**
- es estrecha, por ruta completa y con una sola aparición;
- tiene sus dos negativas (P:402-403);
- se comprobó que las demás guardas que nombran la API no leen el archivo nuevo: `TW/WorkspaceBrowserHostGuardTests.cs:259`, `TW/WorkspaceHostSourceGuardTests.cs:454`, `TW/WorkspacePilotCloseSyncGuardTests.cs:851`, `TW/WorkspacePilotPrepareGuardTests.cs:427-429`, `TW/WorkspacePilotSurfaceGuardTests.cs:280`, `TW/WorkspacePilotUpdateGuardTests.cs:171` y `TW/WorkspaceSourceGuardExceptionTests.cs:269`.

**Huecos de la guarda del adaptador** (R-08):
- no fija el planificador de la `ContinueWith`;
- no prohíbe `Task.Run`, `Task.Factory`, `StartNew`, `Thread`, `ThreadPool`, `Timer`, `Dispatcher` ni `ConfigureAwait`;
- en cambio, la excepción de la navegación sí los prohíbe (`TW/WorkspaceSourceGuardExceptionTests.cs:270`).

**IC-01:** es compatible. La navegación sigue en contexto de aplicación (`docs/initiatives/I-64-implementation-clarifications.md:17` excluye Actualizar).

### 2.4 El cableado y el escritor (D-T4C-06 y D-T4C-07)

**¿El ejecutor es sin estado?**
- Sí, con la guarda «sin campos».
- El estado por petición vive en el corredor y en las clausuras.
- Falta una guarda que impida guardar el corredor o `IPushBackPilotHostRun` en un campo (M-14).

**¿El escritor es de un solo uso y del documento exacto?**
- Sí, con D-1: la corrida llama a `port.Begin` **una vez** (`RUN:248`).
- La negativa de `TW/WorkspacePilotUpdateGuardTests.cs:267-276` sigue disparando: inyecta `private Document _document;` sin `readonly`, y esa línea no coincide con la declaración que se quita.

**¿Puede escapar algún objeto de AutoCAD?**
- `IPushBackPilotHostPrimitives<TDocument>` es una interfaz de Application cuyos métodos devuelven y reciben el `Document` en tiempo de ejecución (P:354-359).
- INV-F4T4C-19 lo da por cumplido «por genericidad» (P:1097).
- Choca con la letra de `OO`, que dice «No AutoCAD … Document may escape through the Application-facing interface» (R-04).

**¿Los cambios de guarda de T4B son exactos y llevan su negativa?**
- Los de `WRITER`, sí.
- Los demás, no: el inventario declarado está incompleto (R-07).

**«Do not reopen T4B»:**
- D-1 se apoya en la letra de `OO`, «receives only the exact current document» y «cannot be reused», y va al Owner (O-04). Es razonable.
- **Pero T4C toca, además:**
  - `RUN` (I-01, M-01 y **seis textos nuevos**);
  - el final de `TryUpdatePilot` (M-01 y M-03);
  - `WorkspaceEditIntent.cs`.
- No hay ninguna parada para un cambio de esos archivos fuera de las líneas declaradas (M-13).

### 2.5 El ámbito de mutación propia (D-T4C-09)

**IC-08:** se cumple en lo esencial:
- lo propio se registra aparte y solo invalida, y no se suprime `InvalidateNavigationCaches` (`docs/initiatives/I-64-implementation-clarifications.md:145-151`);
- la regla de drenaje de P:567 está mal redactada: con `ImpliedSelectionChanged` en `_pending`, que siempre está tras el vaciado de PICKFIRST, la letra «alguna pista ajena» marcaría siempre (M-01).

**INV-08 e INV-28** (`docs/initiatives/I-64-proposal-v5.md:746`, `:766`):
- INV-08 se cumple: un refresco, sin reentrada;
- INV-28 está en riesgo por el residual de los `*Cancel*` (R-06).

**La coalescencia por tipo** se respeta, con un segundo conjunto (`WorkspaceSession.cs:25-58`).

**El orden real de los eventos es [H].**
- `CommandWillStart` solo cambia el reposo (`EBC:317-318`).
- `CommandCancelled` y `CommandFailed` encolan `CommandEnded` (`EBC:324-327`).
- Los dos `*Cancel*` de `EXECUTEFUNCTION` están medidos (`docs/automation/evidence/I-64-pilot/F2-SMOKE-NAV/R20261110T064058Z-c883/operator/session-log.md:10`, `:20`). Que produzcan `CommandCancelled` no está medido.

**¿El residual está bien acotado y es el único?** No:
- **(1)** sin un `CommandEnded` del comando interno, el primer `CommandEnded` posterior, que sería el de un comando del usuario sobre otro rack, se registra como propio (P:572), y sus escrituras se enmascaran (R-05);
- **(2)** si los `*Cancel*` encolan `CommandEnded` antes de la ventana, B queda marcado después de **cada** Actualizar. Eso contradice el oráculo «tras Actualizar A, el borrador B no queda marcado» y S2-14 (`docs/initiatives/I-64-proposal-v5.md:894`). El paquete lo llama «conservador» (P:585) sin consecuencia (R-06).

**¿Cubre la importación de `PrepareUnits`?** Sí: se abre antes de `TryUpdatePilot` (P:570).

**¿Se libera en éxito, en excepción y en cancelación?** Sí, si llegan las señales: `CommandEnded` o `CommandCancelled`, `NotRun` y la destrucción.

**¿Puede ocultar otro rack?** Sí, en el caso (1) anterior.

**Guardas:**
- el respaldo con `EnteringQuiescentState` choca con los literales de caso fijados `case BridgeSource.EnteringQuiescentState: return SetQuiescent(state, true);` (`TW/WorkspaceHostC2SourceGuardTests.cs:202-210`, fuera del `AllowedWriteScope`, y `TW/WorkspaceLifecycleGuardTests.cs:176-177`);
- las entradas nuevas de `EBC` no deciden la regla de olvido de la secuencia de cierre (`TW/WorkspaceLifecycleGuardTests.cs:113`, `:167`);
- todo ello va en R-07.

### 2.6 Undo y H0 (D-T4C-15 y D-T4C-16)

**¿Afirma algo que solo prueba AutoCAD?** No. Lo marca [H] y lo deja a Smoke-2 (P:764-770).
- **Falta en la lista:** el paso de Undo vacío que dejaría un `EXECUTEFUNCTION` sin escritura, por `NotRun`, Conflict o NothingToApply (M-11).

**La sonda H0** (R-10):
- **el build y su custodia:**
  - «sin commit … la fuente restaurada después» (P:791) no dice quién escribe y revisa el arnés;
  - ni dónde: debería ser fuera del worktree de I-64, que es la base del Worker;
  - ni cómo se custodia: el parche, el SHA de la base y el SHA-256 del DLL;
- **el operador:**
  - falta la regla de que los datos de máquina (hashes y horas) los mide la sesión, y que el Owner solo observa y firma. El precedente es `docs/automation/evidence/I-64-pilot/F1-SMOKE-1/R20261107T171052Z-6862/preflight/baseline-verification-session.txt:16-17`;
  - falta cerrar sin guardar y comprobar el SHA-256 del dibujo original, como en `…/F2-SMOKE-NAV/…/session-log.md:18`;
- **las autoridades con las que choca, sin nombrarlas:**
  - `OO` llama a Smoke-2 «the first physical validation of the new pilot-specific authored write path», y H0 ejercita esa vía antes;
  - `docs/initiatives/I-64-proposal-v5.md:821-822` fija los momentos de cada hito, y `:828-830` exige un Candidato completo con UI Full local y CI exacta, que un arnés sin commit no da;
  - F4-E59 prohíbe las sondas con entradas válidas en caminos de escritura (`docs/automation/evidence/I-64-pilot/F4-DRAFTS/f4-dispositions.md:2086`);
- **lo que mide:** P:798-806 cubre lo que condiciona D-T4C-02 a D-T4C-04. Le faltan:
  - si el `Task` siempre se completa (R-01);
  - si los `*Cancel*` encolan `CommandEnded` (R-06);
  - el paso vacío de Undo;
- **qué pasa con cada resultado:** P:808-810 solo dice «contradice, A-n» o «confirma». Falta una tabla por medida. Por ejemplo, un callback síncrono confirma F-1; un `CommandCancelled` previo activa R-06; varios pasos de Undo activan C02.

### 2.7 Las pruebas y las guardas (D-T4C-17 y D-T4C-14)

**Las 20 pruebas:** las 20 tienen prueba, archivo y doble (P:833-854).
- **La 12** dice «ninguna llamada de primitiva después de `Execute`», y eso contradice la 7, que pide `Retire` después (M-08).
- **La 3 (b)** dice «`Admit` falso», pero con el orden de P:364-365, `TryResolveActive` falla antes (M-09).
- **Faltan pruebas de R-02:**
  - la llamada que lanza tras un callback síncrono;
  - la presentación una sola vez;
  - el repintado anidado.
- **Falta la de M-10:** ninguna excepción sale hacia AutoCAD.

**¿Cada guarda nueva o cambiada lleva su negativa?** Las declaradas, sí. Pero el inventario no está completo (R-07):
- `PortMutatingOperations` (P:305) **no se puede satisfacer**: la guarda exige que cada cuerpo cambie un borrador y quite `_knownClean` (`TW/WorkspaceLifecycleGuardTests.cs:488-497`), y fija `Skip(5).Count() == 4` (`:540-546`). `RequestUpdate` no cambia ningún borrador;
- los literales de caso de reposo están fijados en dos guardas;
- la regla de olvido de la secuencia de cierre (13 entradas) no se decide;
- la superficie pública exacta de `AutoCadEventBridge` (`TW/WorkspaceBridgeFacadeSourceGuardTests.cs:91-104`) cambia si los reenviadores son públicos;
- «No hay otros» (P:880) es falso.

**La guarda de «sin segunda vía»:**
- **G-1** solo mira `PrepareRedraw` y `RedrawInTransaction` de Push Back;
- **G-5** solo el adaptador y el corredor;
- **G-6** solo las carpetas Workspace, con una excepción sin enumerar («salvo los efectos de navegación ya permitidos», P:737);
- ninguna ve un archivo nuevo del Plugin que nombre el piloto y escriba por otra API (R-12). Una regla «el piloto × la API de escritura» es viable: hoy solo la cumplen `WRITER` y la transacción de lectura de `src/RackCad.Plugin/Views/RackPilotIntentAdapter.cs:161`.

**F4-E59:** la contención de P:882-886 es correcta. La excepción es H0, en R-10.

### 2.8 El contrato propuesto (§6)

**`AllowedWriteScope`:**
- **no basta:** con el diseño de D-T4C-09 falta `TW/WorkspaceHostC2SourceGuardTests.cs` (R-07);
- **sobra:**
  - `src/RackCad.UI/Workspace/WorkspaceBrowserViewModel.cs`, que ya presenta `Busy` con su motivo (`:945`), salvo que se decida arrastrar C2-M-7 (M-05, M-07);
  - `TW/WorkspaceBridgeHintAndDrainTests.cs` se justifica.

**`ForbiddenWriteScope`:**
- **T1B:**
  - G1 a G6 van por entrada explícita;
  - las seis clases de UI de T1B y los soportes, también;
- **los 18 `*Commands*.cs`:** están, F08 a F25 en `docs/automation/evidence/I-64-pilot/F4-T4B-MUTATE-POST/coordinator/gate-contract.json:259-276`;
- **la costura de I-55:** está, por F04, F07 y F26 (`docs/initiatives/I-64-f4-t4b-gate-contract-package.md:148-151`).
- **Defectos** (M-06):
  - F28 (`RackPilotIntentAdapter.cs`) no tiene una decisión expresa, y M4 la deja «a decidir»;
  - «64» solo cuadra si F28 se cuenta aparte;
  - los cuatro archivos no permitidos de `src/RackCad.UI/Workspace/` no tienen entrada explícita, a diferencia de F03.

**Los invariantes:**
- **INV-03** es falso tal como está escrito: «sin `Task` … en las carpetas Workspace y en `UI/Workspace`; el único archivo con `Task` es el adaptador» (R-11). `Task` existe hoy en `src/RackCad.Application/Workspace/IWorkspaceNavigationPort.cs:94`, en `src/RackCad.Plugin/Workspace/WorkspaceNavigationHost.cs:127` y `:148`, y en `src/RackCad.UI/Workspace/WorkspaceNavigationViewModel.cs:194-206`, con `async`;
- **INV-19** depende de R-04, e **INV-17** de R-07;
- los demás son verificables.

**Las cuentas de `RequiredTests`:**
- salen de la CI de la base;
- recontadas: 81, 48 y 33, y los filtros 3103, 601, 55 y 10.

**Las paradas:**
- cubren las condiciones de devolución;
- les falta una parada para un cambio de `RUN`, `WRITER` o el final de `TryUpdatePilot` fuera de las líneas declaradas (M-13);
- les falta mapear R-06 a C12.

**La división H0, T4C1 y T4C2:**
- es coherente;
- T4C1 deja `RequestUpdate` sin llamador;
- no abre nunca el ámbito ni la marca;
- en ese estado intermedio, D-T4C-10 es inalcanzable;
- no hay ningún estado inseguro.

### 2.9 El §9

**Faltan decisiones materiales:**
- **la vida de la marca en vuelo sin callback** (R-01). Además, O-07 (a) es **inseguro sin ella**, porque convierte una marca colgada en un descarte imposible;
- **la política del borrador que cambia durante el vuelo** (R-03);
- **la lectura del `Document` genérico** frente a «may not escape» (R-04), si no se rediseña;
- **la consecuencia de un `CommandCancelled` previo a la ventana** (R-06);
- **dentro de O-01:**
  - las dispensas expresas frente a Smoke-2 como «first physical validation»;
  - las dispensas frente a §12.1, el Candidato y el hito;
  - las dispensas frente a F4-E59 (R-10);
- **los textos nuevos en `RUN`** frente a «Do not reopen T4B» (M-13).

**Recomendaciones que contradicen una autoridad:**
- **O-01 (a),** sin nombrar sus choques (R-10);
- **D-T4C-11,** paso 1, que contradice la semántica de la frontera bloqueante Update (ADR-0032 D6; `DS:88-91`, `:377-404`) (R-09).

**Condiciones de devolución activadas:**
- «any other material product decision», por O-01, O-02 y lo anterior;
- «persistent host authority», posible por O-03.
- Bien marcadas.

### 2.10 Las anclas

**Exactas en una muestra de unas 60.** Entre ellas:
- `CORE:48`, `:805-905`, `:815-816`, `:852`, `:858-862`, `:870`, `:872-877`, `:908-933`, `:915`, `:921`, `:1110-1124`, `:1156-1188`, `:1481-1515`;
- `WRITER:32-38`, `:68-72`;
- `RUN:61`, `:144-147`, `:172-173`;
- `CTL:105`, `:217-219`, `:250-265`, `:267-273`, `:321`, `:367-373`;
- `EBC:306-338`;
- `WorkspaceSession.cs:25-58`;
- `WorkspaceEditIntent.cs:381-384`, `:472`, `:474`, `:477-483`;
- `DS:510-526`, `:517`, `:555-559`, `:720-731`;
- `IWorkspacePilotEditPort.cs:166-175`;
- `FVM:56-57`, `:412-416`;
- `src/RackCad.Plugin/Workspace/AutoCadEventBridge.cs:335-343`;
- `src/RackCad.Plugin/Views/RackProjectionWriteScope.cs:23-37`;
- `RackSiblingScan.cs:103` y `RackBlockRenamer.cs:32`;
- en `TW`: `WorkspaceEditIntentHostTests.cs:982`, `:1003-1012`, `:1023-1043`, `:1056`; `WorkspacePilotEditGuardTests.cs:44-61`, `:64`, `:151-160`, `:248-253`, `:262-268`, `:427-455`, `:506-529`; `WorkspacePilotUpdateGuardTests.cs:57-125`, `:171`, `:185-188`, `:208-216`, `:267-276`; `WorkspaceLifecycleGuardTests.cs:463-477`, `:679-694`; `WorkspaceNavigationHostTests.cs:435-436`; `WorkspaceHostSourceGuardTests.cs:155-166`, `:467-477`; `WorkspaceBridgeSourceGuardTests.cs:361-366`; `WorkspacePaletteHostSourceGuardTests.cs:559-579`, `:774-782`; `WorkspaceSourceGuardExceptionTests.cs:251-259`; `WorkspaceNavigationEchoTests.cs:325-340`; `WorkspaceHostHardeningGuardTests.cs:758-760`, `:779`; `WorkspaceEditIntentGuardTests.cs:490-505`, `:509-523`, `:598-641`, `:682-685`, `:821-870`, `:1098-1111`;
- `tests/RackCad.Tests/SelectiveEditorOpenTests.cs:564-572`, donde se midieron 40 `[CommandMethod(`;
- `docs/initiatives/I-64-f4-t4-gate-contract-package.md:547-587`;
- `docs/initiatives/I-64-f4-gate-contract-package.md:32`, `:58-60`;
- `docs/initiatives/I-64-proposal-v5.md:216-219`, `:241-243`, `:494-498`, `:504`, `:510-511`, `:519`, `:542-545`, `:746`, `:766`, `:770`, `:885`, `:894`;
- `docs/initiatives/I-64-implementation-clarifications.md:5`, `:17`, `:73-85`, `:131-153`;
- `docs/initiatives/I-64-f4-t4b-gate-contract-package.md:81-83`, `:114-120`, `:325`, `:389`, `:557`, `:714`, `:794-819`, `:842-843`;
- `docs/automation/evidence/I-64-pilot/F4-DRAFTS/f4-dispositions.md:102-109`, `:1694`, `:1701`, `:1707`, `:2082`, `:2086`.

**Inexactas o falsas:**
- **P:210:** la frase «`UI Full local: PASS` es obligatorio» empieza en `docs/guias/validacion-manual-autocad.md:427` y sigue en `:428`, no solo en `:428` (M-04);
- **P:130:** «`Plugin/Views` no entra en ninguna guarda de asincronía». El escritor sí entra (`TW/WorkspacePilotUpdateGuardTests.cs:171`) (M-04);
- **P:295 e INV-03:** sobre `Task`, R-11;
- **P:638:** la frontera Update con texto pendiente, R-09;
- **P:708:** «`git grep EnterError` en `src` = solo su definición». `src/RackCad.UI/Workspace/WorkspacePanelViewModel.cs:83`, `:171`, `:180`, `:204` y `:292` tienen un `EnterError` privado que no tiene nada que ver (M-02);
- **P:959:** «T4C no toca esas superficies». T4C modifica `TW/WorkspaceLifecycleGuardTests.cs`, una superficie de C2-M-5, y `WorkspaceBrowserViewModel.cs`, la de C2-M-7. Además, C2-M-6 está asignada a T4C (`docs/automation/evidence/I-64-pilot/F4-DRAFTS/t4b/map-3-carries-guards-rules.md:158-162`) (M-05);
- **P:1046:** «64» (M-06).

---

## 3. Tabla de hallazgos

| Id | Gravedad | Contenido | Evidencia | Corrección propuesta |
|---|---|---|---|---|
| R-01 | IMPORTANTE | **La marca en vuelo no tiene cota** si el callback no corre y el `Task` no se completa. Con `Busy` en los descartes (D-T4C-10), el veto de cierre por Dirty y la caja deshabilitada, el documento no se puede actualizar, descartar ni cerrar. El paquete dice que tolera «el callback que no llega», y no es así | P:276-284, P:374, P:551, P:610-613; `CORE:1378-1392`; `src/RackCad.Application/Workspace/WorkspaceDocumentDiscardCommand.cs:146`, `:176-177` | Una liberación **protegida por el testigo**: en la siguiente entrada en reposo o en el siguiente drenaje de la sesión si el callback no empezó, o un gesto que invalide el testigo. Un callback tardío con el testigo viejo no hace nada. Pruebas HR y RQ del modo «nunca». Que H0 mida si el `Task` se completa. Llevar O-07 y esta regla juntas al §9 |
| R-02 | IMPORTANTE | **La presentación no es exacta ni única.** (a) Si la llamada lanza después de un callback síncrono que ya escribió, se da `NotRun(Failed)` y el valor de inicio `Failed`: Applied o CommitThrew se presentan como fallo, contra «Do not translate UNKNOWN». (b) `TryRequest` en falso con un `NotRun` que guarda resultado da una doble presentación. (c) El repintado anidado dentro de la llamada síncrona consume el resultado antes del `Take` del gesto, y `!InFlight` con `Unavailable` no tiene regla. (d) «Consume» frente a «sigue guardado» con `IsMine` | P:263-274, P:360, P:374, P:640, P:655-657; `CTL:217-219`, `:250-265`; `EBC:317-320` | (1) **Un cerrojo terminal en el corredor:** si el callback empezó, nunca hay `NotRun`, y `Request` cuenta como aceptado; un callback después del cerrojo no hace nada. (2) El fallo síncrono viaja por **un solo** canal, el valor de inicio o el almacén. (3) En el VM, `!InFlight` con `Unavailable` no toca lo presentado. (4) Se consume solo para la superficie viva y después de `IsMine`. Pruebas HR y UI de los cuatro casos |
| R-03 | IMPORTANTE | **TOCTOU del borrador entre la aceptación y el callback:** `CommitField`, `SetPendingText`, `CrossBoundary`, `TryKeepMine` y `TryDiscardAndReload` siguen admitidos, y el Actualizar aplica un estado que el usuario no tenía al pulsar. Se declara, sin prueba y sin decisión | P:549-552; `CORE:1045-1153`, `:688`; `OO`, «FINAL REVALIDATION» y «ONE ACTUALIZAR» | El billete captura una huella del borrador (el estado actual y el pendiente) y `Admit` la compara: si cambió, `NotRun(Rejected)`. O bien, `Busy` en las operaciones que cambian **ese** borrador durante el vuelo. Prueba RQ. Si no, O-nueva en el §9 |
| R-04 | IMPORTANTE | **Un `Document` cruza la interfaz de Application:** `IPushBackPilotHostPrimitives<TDocument>` lo devuelve y lo recibe en código de Application. INV-19 lo da por cumplido por genericidad | P:354-359, P:1097; `OO`, «RUN-SCOPED WRITER» | Un objetivo opaco implementado en `Plugin/Views` (`TryOpenTarget(long id, out IPushBackPilotHostTarget target, out string reason)`, con `Lock()` y `CreateWriter()`), con las mismas pruebas HR del orden. O llevar la lectura al Owner y reescribir INV-19 |
| R-05 | IMPORTANTE | **El ámbito propio no tiene cota estructural:** si falta el `CommandEnded` del comando interno, el primero posterior, el de un comando del usuario sobre otro rack, se registra como propio y sus escrituras se enmascaran. Queda fuera del residual declarado | P:571-576, P:587-589; `EBC:317-327` | Cerrar también con el primer `CommandWillStart` de la sesión tras `Begin`. Prueba OM: sin `CommandEnded`, un comando posterior marca B. Declararlo en IC-09 y llevarlo a H0 |
| R-06 | IMPORTANTE | **El residual de los `*Cancel*` choca con un oráculo vinculante:** si encolan `CommandEnded` antes de la ventana, B queda marcado después de cada Actualizar. Fallan IC-08, INV-28 y S2-14. El paquete lo llama conservador y no le da consecuencia | P:585; `EBC:324-327`; `docs/initiatives/I-64-implementation-clarifications.md:145-151`; `docs/initiatives/I-64-proposal-v5.md:766`, `:894`; `…/F2-SMOKE-NAV/…/session-log.md:10` | Mapear el resultado de H0: si se confirma, se activa C12, con una variante (abrir en la aceptación, con su coste de enmascaramiento) o una A-n. Añadirlo al §9 |
| R-07 | IMPORTANTE | **El inventario de cambios de guarda está incompleto, y el alcance no basta.** (a) Añadir `RequestUpdate` a `PortMutatingOperations` no se puede satisfacer: exige un cambio de borrador y `_knownClean`, y fija `Skip(5) = 4`. (b) El respaldo con `EnteringQuiescentState` (y el de R-05) rompe los literales de caso de `TW/WorkspaceHostC2SourceGuardTests.cs:202-210`, que no está en el permitido, y de `TW/WorkspaceLifecycleGuardTests.cs:176-177`. (c) Las entradas nuevas de `EBC` no deciden la regla de olvido (`:113`, `:167`, 13 entradas). (d) Los reenviadores públicos de `AutoCadEventBridge` rompen `TW/WorkspaceBridgeFacadeSourceGuardTests.cs:91-104`. «No hay otros» es falso | P:305, P:573, P:578, P:880; las guardas citadas | (a) `RequestUpdate` fuera de esa lista, con una aserción propia: no cambia ningún borrador y no nombra `TryUpdatePilot`. (b) Declarar el cambio y añadir el archivo C2 al permitido, o cerrar dentro de `SetQuiescent` mediante `SessionState`, sin los nombres prohibidos. (c) Decidir el olvido y su cuenta. (d) Reenviadores `internal`, o el cambio declarado. Cada uno con su negativa |
| R-08 | IMPORTANTE | **La guarda del adaptador** no fija el planificador de la `ContinueWith`: una continuación en el pool llamaría al núcleo, que no es seguro entre hilos. Tampoco prohíbe `Task.Run`, `Task.Factory`, `StartNew`, `Thread`, `ThreadPool`, `Timer`, `Dispatcher` ni `ConfigureAwait` | P:404-411; `TW/WorkspaceSourceGuardExceptionTests.cs:270` | Un patrón exacto `ContinueWith(…, TaskScheduler.FromCurrentSynchronizationContext())` con el contexto capturado, la lista de prohibiciones del precedente y negativas por cada una |
| R-09 | IMPORTANTE | **La frontera Update está mal descrita:** con texto pendiente válido, la frontera bloqueante lo confirma y procede (ADR-0032 D6). `Kept` solo sale si no se puede editar, y un pendiente inválido da `Aborted`. Con la letra del paquete, Actualizar no pide nada si hay texto válido escrito | P:635-638; `DS:88-91`, `:377-404` | El gesto: `Proceeded` → `RequestUpdate`; `Aborted` → el mensaje; `Kept` o `Busy` → nada. Con las pruebas del VM y de la vista |
| R-10 | IMPORTANTE | **H0 no nombra sus choques ni su custodia.** Ejercita la vía de escritura del piloto antes de Smoke-2, que `OO` llama «first physical validation». §12.1 fija los hitos y exige un Candidato completo. F4-E59 prohíbe las sondas con entradas válidas en caminos de escritura. No dice quién escribe ni quién revisa el arnés, ni dónde (fuera del worktree de I-64), ni su custodia, ni las reglas del operador, ni cerrar sin guardar con el hash del original, ni una tabla de resultado → acción | P:781-814; `docs/initiatives/I-64-proposal-v5.md:821-822`, `:828-833`; `docs/automation/evidence/I-64-pilot/F4-DRAFTS/f4-dispositions.md:2086`; los precedentes `…/F2-SMOKE-NAV/…/session-log.md:18-20` y `…/F1-SMOKE-1/…/baseline-verification-session.txt:16-17` | Nombrar en O-01 las tres dispensas que necesita. Definir: un arnés revisado en un worktree desechable, con el parche, el SHA de la base y el SHA-256 custodiados; los datos de máquina los mide la sesión y el Owner observa; el cierre sin guardar, con el hash del original comprobado. Una tabla por medida, con R-01, R-06 y el paso vacío de Undo |
| R-11 | IMPORTANTE | **INV-03 y D-T4C-02 son falsos:** hoy hay `Task` y `async` en las carpetas Workspace y en `UI/Workspace` (la navegación) | P:295, P:1081; `IWorkspaceNavigationPort.cs:94`; `WorkspaceNavigationHost.cs:127`, `:148`; `WorkspaceNavigationViewModel.cs:194-206` | «Ningún `Task` ni `async` nuevo fuera del adaptador; la navegación conserva sus excepciones exactas», con un censo por archivo y una cuenta |
| R-12 | IMPORTANTE | **«NO SECOND WRITE PATH» no se prueba para el Plugin:** G-1, G-5 y G-6 no ven un archivo nuevo del Plugin que nombre el piloto y escriba con `RedrawInPlace`, `RackBlockData.Write` o `ForWrite`. G-6 tiene una excepción sin enumerar | P:717-741; `src/RackCad.Plugin/Views/RackPilotIntentAdapter.cs:161` | G-9: en `src/RackCad.Plugin/` y `src/RackCad.Application/`, ningún archivo que nombre el piloto usa la API de escritura salvo `WRITER`, con la excepción exacta de `:161`. El censo de llamadores de `RequestUpdate` (solo `Update()` del VM) y de `TryRequest` (solo `RequestUpdate`). G-6 con los archivos y las cuentas exactos |
| M-01 | MENOR | La regla de drenaje «alguna pista ajena» marcaría con `ImpliedSelectionChanged` | P:567; `WorkspaceSession.cs:47-56` | «Solo si `_pending` contiene `DatabaseObjectChanged` o `CommandEnded`», con su prueba OM |
| M-02 | MENOR | La guarda «sin llamador de `EnterError`» está mal especificada: la UI tiene un `EnterError` privado que no tiene nada que ver | P:708; `WorkspacePanelViewModel.cs:83`, `:292` | El patrón sobre `DraftSession`, o `\.EnterError\s*\(` en Application y en Plugin |
| M-03 | MENOR | M-01 (2): con `!closed.Accepted` (`NotApplying`) y el sello devuelto, la respuesta Applied presentaría un éxito sin el rebase del borrador | P:707; `DS:536-537` | Un desenlace que no sea éxito para ese caso, como un Unknown o un error con su clasificación |
| M-04 | MENOR | Dos anclas inexactas: la de la guía y la de «`Plugin/Views` sin ninguna guarda de asincronía» | P:130, P:210 | Corregirlas |
| M-05 | MENOR | R-T4-13: T4C sí toca superficies de C2-M-5 y de C2-M-7, y C2-M-6 está asignada a T4C | P:959; `docs/automation/evidence/I-64-pilot/F4-DRAFTS/t4b/map-3-carries-guards-rules.md:158-162` | Decidir cuáles se cierran y cuáles se difieren |
| M-06 | MENOR | Prohibido: F28 sin una decisión expresa; el «64» de la cuenta; `src/RackCad.UI/Workspace/` no permitido sin entrada explícita | P:1026-1046; `…/F4-T4B-MUTATE-POST/coordinator/gate-contract.json:251-321` | Decidir F28, cuadrar la cuenta y enumerar los cuatro archivos |
| M-07 | MENOR | `WorkspaceBrowserViewModel.cs` sobra en el permitido: ya presenta `Busy` | P:994; `WorkspaceBrowserViewModel.cs:945` | Quitarlo, o justificarlo por C2-M-7 |
| M-08 | MENOR | La prueba 12 contradice la 7 | P:841, P:846 | «Ninguna escritura ni ningún segundo `CreateWriter` o `Execute`» |
| M-09 | MENOR | En la prueba 3 (b), el orden es incoherente. Falta el id nativo reciclado | P:837, P:364-365; `WorkspaceSession.cs:18` | Corregir el orden y añadir el caso del id reciclado |
| M-10 | MENOR | No se exige que ninguna excepción salga hacia AutoCAD desde el callback | P:370-372, P:381 | Un `OnCallback` total, contenido y registrado, con la prueba HR «el callback nunca lanza» |
| M-11 | MENOR | Faltan el paso de Undo vacío de un `EXECUTEFUNCTION` sin escritura y el doble clic tras una corrida síncrona | P:759-768; `FVM:397-399` | Añadirlo a «solo AutoCAD» y a H0. Que el gesto compruebe su botón, con prueba UI |
| M-12 | MENOR | No se reconcilia con R-T4-06, que dice «un segundo Actualizar recibe `AnotherUpdateInProgress`», ni con «the existing Applying lock», ahora que `Busy` va antes | P:283; `docs/initiatives/I-64-f4-t4-gate-contract-package.md:555-558` | Declarar que la ranura sigue siendo el oráculo de T4B y que el host responde `Busy` antes |
| M-13 | MENOR | Seis textos nuevos del host en `RUN`, el archivo de T4B, más allá de I-01. Ninguna parada para cambios de `RUN`, `WRITER` o el final de `TryUpdatePilot` fuera de lo declarado | P:436-444, P:1108-1121 | Llevar los textos al archivo nuevo de T4C, o justificarlos. Una parada C14 para cualquier cambio de esos archivos fuera de las líneas declaradas |
| M-14 | MENOR | Sin guarda «el corredor y `IPushBackPilotHostRun` nunca en un campo», ni un cerrojo terminal frente a `OnCompleted` | P:360, P:363 | Ampliar G-3 y el cerrojo de R-02 |
| M-15 | MENOR | A2P §8, «D-1 si añade una lectura», no se trata | `docs/initiatives/I-64-f4-t4a2-gate-contract-package.md:660` | Declarar que T4C no añade lecturas |
| M-16 | MENOR | Dos restricciones vigentes que el Worker tiene que conocer. (1) El archivo nuevo de `Pilot/` no puede contener «Transaction» ni en un comentario. (2) Los tipos anidados nuevos del núcleo no pueden llamarse con `Draft`, `Editor` ni `Tab` | `TW/WorkspacePilotUpdateGuardTests.cs:118-123`; `TW/WorkspaceModelBoundaryTests.cs:94-104` | Citarlas en la emisión |

---

## 4. Veredicto

**EMITIBLE TRAS CORRECCIONES. B/I/M = 0/12/16.**

**Sin bloqueantes.** Ningún hallazgo obliga a duplicar la política de T4B, a usar más de una transacción authored ni a cambiar T1B.

**R-01, R-02, R-04, R-05 y R-07 cambian el diseño de D-T4C-02, D-T4C-04 y D-T4C-09.** Se recomienda una re-revisión acotada de esas tres decisiones después de corregirlas, aunque `OO` solo la exige ante bloqueos de arquitectura.

**Lo que tiene que volver al Owner:**
- **O-01 (H0):** con las tres dispensas que no nombra (Smoke-2 como «first physical validation», §12.1 con su hito y su Candidato, y F4-E59), la custodia y el operador (R-10);
- **O-02 (la vía A):** con el residual corregido. Incluye el cierre con `CommandWillStart` (R-05) y la consecuencia de un `CommandCancelled` previo (R-06);
- **O-03 (el campo de larga vida)** y **O-04 (D-1)**, sin cambios;
- **O-05:** el aviso de C-F4-02;
- **O-07,** junto con la regla de la vida de la marca en vuelo (R-01);
- **nuevas:**
  - la política del borrador que cambia durante el vuelo, si no se resuelve en el diseño (R-03);
  - la lectura de INV-19, si no se rediseña el objetivo opaco (R-04);
  - la superficie de `RUN` para los textos nuevos (M-13);
- **las demás del §9 (O-06, O-08 a O-15),** sin cambios.
