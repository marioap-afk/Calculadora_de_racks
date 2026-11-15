# I-64 F4-T4C1-HOST: revisión adversarial independiente del código

- **Worktree:** `<worktree-de-I-64>` (ruta local redactada por la regla de custodia), rama `architecture/workspace-persistente-rackcad`, HEAD = CurrentSha.
- **SHA:**
  - BaseSha `44eafbc1`;
  - RedSha `8e57061e`;
  - CurrentSha `271a045c`.
- **Método:**
  - solo lectura, con `git -C … show/diff/grep` y la lectura de los archivos de la corrida;
  - sin `dotnet`, sin builds y sin pruebas;
  - sin checkout ni escrituras en el worktree.
- **Anclas:**
  - las de `src` y `tests` son de CurrentSha (los `tests/` son idénticos en RedSha);
  - `PKG` = `docs/initiatives/I-64-f4-t4c-gate-contract-package.md` (v5);
  - `CORE` = `src/RackCad.Application/Workspace/WorkspaceHostCore.cs`;
  - `HOSTRUN` = `src/RackCad.Application/Systems/PushBack/Pilot/PushBackPilotHostRun.cs`;
  - `ADAPTER` = `src/RackCad.Plugin/Views/RackPilotUpdateCommandContext.cs`;
  - `WRITER` = `src/RackCad.Plugin/Views/RackPilotUpdateWriter.cs`;
  - `EBC` = `src/RackCad.Application/Workspace/EventBridgeCore.cs`;
  - `TW` = `tests/RackCad.Tests/Workspace/`;
  - `IG`, `RQ`, `HR` y `OM` = los cuatro archivos nuevos de pruebas de `TW`.

## Veredicto

**REWORK**: **B/I/M = 1/1/10**.

- El alcance, el RED, los textos, el adaptador, el corredor, el escritor D-1, el ámbito propio (IC-09), R-T4-07, I-01, M-03 y M-06 están bien.
- El rework lo obliga un defecto: **un billete que pasó por `Execute` y cuya compleción no llega no caduca nunca** (B-01). Es justo el caso que la cota «Completar o comando ajeno» tiene que cubrir.
- Además, G-1 y G-10 se acotaron por carpeta sin la necesidad que alega la entrega (I-01).

## Hallazgos

| Id | Gravedad | Contenido | Evidencia | Corrección propuesta |
|---|---|---|---|---|
| **B-01** | **B** | **El billete terminado sin compleción no tiene cota.** `Execute` pone `ticket.InExecute = true` y nada lo vuelve a `false`: su `finally` solo marca `Finished`. `ExpireStaleUpdate` sale con `null` para todo billete con `InExecute`, así que un billete que pasó por `Execute` solo se libera con `Completed()` o con `SessionDestroyed`. La rama de liberación de un billete terminado (`if (ticket.Finished) return null;`) solo la alcanzan los que terminó `NotRun`. Si la compleción no llega (falla el `Post`, que la continuación cruda solo registra, o no hay contexto de sincronización dentro del contexto de comando, H-50), quedan en `Busy` o `Waiting` hasta cerrar el documento: `RequestUpdate` de la sesión, `DiscardRack`, `DiscardDocument` y las cinco operaciones de D-T4C-08 sobre ese rack. Con un borrador Dirty o en Conflicto, el cierre se veta y el descarte sigue en `Busy`: es el bloqueo de H-25 que la cota debe impedir. Contradice D-T4C-02: un billete «terminado cuya compleción no llegó, también caduca»; «Un billete en Execute nunca caduca» se refiere a *durante* `Execute`. Contradice también D-T4C-04 («Si el Post falla … la cubre la caducidad»), D-T4C-05, D-T4C-10 («también si el callback ya empezó») e INV-F4T4C-02 y 10. O-17 bis remite su interpretación a D-T4C-02. Ninguna prueba lo ejerce: la parte «inside» de la prueba de RQ solo mira un comando ajeno *durante* `Execute`. | `CORE:1604` (único `InExecute = true`), `CORE:1625`, `CORE:1436`, `CORE:1441`, `CORE:1653-1664`, `CORE:1811`; `CORE:1362`, `:1220`, `:1247`, `:1101`, `:1129`, `:1037`, `:1052`, `:1067`; `ADAPTER:55-65`; `PKG:472`, `:482`, `:616`, `:669`, `:692`, `:930`; `TW/WorkspacePilotUpdateRequestTests.cs:471-500` | En el `finally` de `Execute`, poner `ticket.InExecute = false` junto a `Finished = true`; o, en `ExpireStaleUpdate`, comprobar `ticket.InExecute && !ticket.Finished`. Añadir en RQ dos pruebas: (a) modo diferido, `RunCallback` sin `Complete` y luego `ForeignCommand()`: `RequestUpdate` se acepta, los descartes se sirven y el resultado guardado sigue, una sola vez; (b) un comando ajeno durante `Execute`: tras `Execute` el billete se libera en la siguiente comprobación, sin un segundo resultado |
| **I-01** | **I** | **G-1 y G-10, acotados por carpeta sin necesidad medida.** **G-1:** el paquete lo pide sobre todo el producto («censo de producto», M1 §5.4); IG solo lo aplica a las dos carpetas Workspace, a ADAPTER, a HOSTRUN y a la posición dentro del escritor. La justificación de la entrega («los caminos clásicos … llaman legítimamente a esas primitivas») no se sostiene en CurrentSha. Los archivos que nombran uno de los tres servicios y llaman a `.PrepareRedraw(` o `.RedrawInTransaction(` son exactamente los tres servicios, que delegan en `ViewBlockDraw`, y el escritor; `RackPushBackCommands.cs` los nombra pero no llama a esos métodos. Un censo de todo el producto con esas excepciones exactas era satisfacible. Tal como está, un archivo nuevo del Plugin fuera de esas carpetas que llame a `.PrepareRedraw(` pasa G-1, y G-9 no incluye `PrepareRedraw`. **G-10:** excluye carpetas enteras, `src/RackCad.UI/Systems/` y `src/RackCad.UI/Editor/`. Los llamadores medidos de `.RequestUpdate(` allí son cuatro archivos, y `UI/Editor` no tiene ninguno. Las dos acotaciones son decisiones del Worker, declaradas como `Deviations` y sin `OpenQuestion`, sobre guardas que INV-F4T4C-14 nombra | `TW/WorkspacePilotUpdateIntegrationGuardTests.cs:239-258`, `:631-642`; `PKG:1054`, `:1113-1116`; servicios en `src/RackCad.Plugin/Systems/PushBack/PushBack{Frontal,Planta,System}DrawService.cs` (`:58/:64`, `:52/:58`, `:60/:66`); `src/RackCad.UI/Systems/Cantilever/RackCantileverWindow.xaml.cs:1345`, `…/Dynamic/RackDynamicSystemWindow.xaml.cs:3459`, `…/PushBack/RackPushBackSystemWindow.xaml.cs:3369`, `…/Selective/RackSelectiveWindow.xaml.cs:2614`; `worker-handoff.json` (`UnexpectedFindings` [0] y [1], `Deviations` [1]) | G-1 como censo de todo el producto con el conjunto exacto {los tres servicios, `WRITER` en `PrepareUnits` y `WriteUnit`}, con la negativa de un archivo nuevo del Plugin. G-10 con las cuatro ventanas como excepción por archivo exacto y cuenta 1, sin excluir carpetas. Si no, la decisión expresa del Coordinator |
| M-01 | M | **El vuelo se mira antes de `Authorize` en tres lecturas.** En `TryConfirmBase`, `TryKeepMine` y `TryDiscardAndReload`, la comprobación del vuelo va antes de `PassGate`, que autoriza dentro de `Try*Pilot*`. La fila 5 pide que las cinco operaciones y los descartes comprueben el vuelo «después de `Authorize` y de `_pilotReadInProgress`». La guarda lo acepta porque solo exige el orden respecto a `Authorize` si el cuerpo contiene `Authorize(` (`authorize >= 0`). El efecto es pequeño: una petición no autorizada recibe `Waiting` en lugar de `Rejected`, sin efecto | `CORE:1037`, `:1052`, `:1067` frente a `CORE:558-561`; `TW/WorkspacePilotEditGuardTests.cs:311`; `PKG:1388` | Mover la comprobación dentro de `Try*Pilot*`, tras `PassGate`, o declarar la lectura y exigir en la guarda `PassGate` o `Authorize` antes del vuelo |
| M-02 | M | **La fila 4 no tiene negativa.** El censo de portadores gana los dos tipos, pero falta la negativa «Un portador sin censar» que exige INV-F4T4C-17 | `TW/WorkspaceEditIntentHostTests.cs:984-990`; `PKG:1387` | Derivar los portadores de las firmas de `IWorkspacePilotEditPort` y exigir que estén todos en el censo, con la negativa de uno nuevo sin censar |
| M-03 | M | **La guarda de textos de la fila 11 no cubre los `Busy` y `Waiting` nuevos.** Mira `UpdateGate`, `RequestUpdate`, `TakeUpdateProgress`, `ExpireStaleUpdate`, la corrida, `TryUpdatePilot` y `AnswerOf…`, pero no los siete `Busy` y `Waiting` nuevos de las cinco operaciones y los descartes, que D-T4C-13 incluye («en los Busy nuevos»). El producto usa la constante | `TW/WorkspaceEditIntentGuardTests.cs:1235-1258`; `CORE:1037`, `:1052`, `:1067`, `:1101`, `:1129`, `:1221`, `:1248`; `PKG:1032` | Añadir esos cuerpos a `UpdateTextBodies`, con una negativa |
| M-04 | M | **Hay textos visibles fuera de los seis fijados.** `HostNotRunReason`, que es una frase completa, se usa como prefijo: `HostNotRunReason + " " + ex.Message`, en HOSTRUN y en CORE, este último con un `" "` literal nuevo en CORE. El adaptador lanza `"No hay contexto de sincronización en este hilo."`, que llega al usuario tras `HostRequestFailedPrefix`. Las dos razones de `TryOpenTarget` son literales muertos: el corredor las descarta (`out _`) | `HOSTRUN:256-258`; `CORE:1593`; `ADAPTER:31`, `:71`, `:79`; `HOSTRUN:205` | Declarar el formato compuesto o usar solo `HostNotRunReason`; quitar las razones muertas o llevarlas a los textos ratificados |
| M-05 | M | **M-01 (4) incompleto.** La respuesta de un cierre no aceptado (`AnswerOfAClosedThatWasNotAccepted`, con `DescribeDifferences` y la fábrica, que lanza si recibe una lista nula) y `NotePilotEdit` con `_knownClean.Remove` siguen fuera de todo `try` en `TryUpdatePilot`. D-T4C-05 da el escape como «imposible por construcción»: es improbable, no imposible. La capa (5), el `catch` de `Execute`, lo contiene | `CORE:885-899`, `:934-951`, `:1619-1622`; `PKG:693`, `:1033` | Meter esas sentencias en el `try` de la respuesta, con el mismo respaldo |
| M-06 | M | **Faltan defensas en profundidad en el núcleo.** (a) `NotRun` y `Execute` no comprueban `Finished`: una segunda llamada terminal con el testigo vigente sobrescribiría el resultado; hoy solo la impide el cerrojo del corredor. (b) Si `TryRequest` lanza tras un callback síncrono que ya ejecutó, el `catch` da `Failed` como valor de inicio con el resultado ya guardado: dos canales, inalcanzable con el corredor del producto. (c) `IsAnyPilotDraftApplying` responde `true` en su `catch`: si el registro lanzara de forma sistemática, `DiscardDocument` quedaría en `Busy` sin cota. (d) `RunLocked` no trata un `Lock()` nulo: corre sin bloqueo | `CORE:1629-1645`, `:1597-1626`, `:1380-1395`, `:1478-1488`; `HOSTRUN:241-243` | (a) no hacer nada si `ticket.Finished`; (b) devolver `Accepted` si el billete ya está empezado o terminado; (c) `false`, o registrar y no bloquear; (d) un `Lock()` nulo es `NotRun(Failed, LockFailedPrefix…)` |
| M-07 | M | **Hay guardas esquivables.** G-4 cuenta `\.TryUpdatePilot\s*\(` y `\bTryUpdatePilot\s*\(`, así que no ve un grupo de métodos (`Func<…> f = core.TryUpdatePilot;`) fuera del núcleo. La guarda del adaptador cuenta una `.OnCompleted(` y un `.Post(`, pero no comprueba que `OnCompleted` vaya en la rama de `IsCompleted` falso ni que `Post` sea del contexto capturado. Hoy el producto es correcto | `TW/WorkspacePilotUpdateIntegrationGuardTests.cs:411-425`, `:129-149` | Contar `\bTryUpdatePilot\b` (dos apariciones en el núcleo y cero fuera); anclar `OnCompleted` tras `if (result.IsCompleted) {…return;}` y `Post` a `context` |
| M-08 | M | **La entrega no declara con precisión todos los cambios de guarda.** No lista tres: (1) el `InlineData` de `TheReadGuardTripsWithoutTheFinally…`, adaptado al cuerpo nuevo de `TryKeepMine`, consecuencia de la fila 5; (2) `ForeignCommandStartsOf` añadido a «las consultas no olvidan», coherente con D-T4C-09 y la fila 15; (3) la expresión de `WriterProhibitionViolations` ampliada con `static` y `MdiActiveDocument`, cuando la fila 13 dice «la misma expresión de hoy». Los tres endurecen o adaptan, ninguno relaja, así que no los cuento como C07 | `TW/WorkspacePilotEditGuardTests.cs:525`; `TW/WorkspaceLifecycleGuardTests.cs:223`; `TW/WorkspacePilotUpdateGuardTests.cs:205-207`; `PKG:764`, `:1396`; `worker-handoff.json` (INV-F4T4C-17) | Añadirlos a la lista de la entrega frente al inventario |
| M-09 | M | **Hay corridas locales sin declarar.** En `tests/d1/d1.trx` (3155 pruebas, 24 fallos, 09:13) y `tests/d2/d2.trx` (3385, 4 fallos, 09:28) hay corridas de Core anteriores al commit RED (09:46) que `TestsExecuted` no lista: tiene 77 entradas, todas `red_*`, `g1_*` y `g2_*`. Muestran que el producto existía antes de construir el esqueleto. No viola el orden de commits, pero la entrega debe declarar qué corrió en local (INV-F4T4C-18) | `…/R20261115T145157Z-efec/tests/d1/d1.trx`, `…/d2/d2.trx`; `git log`: RED `09:46:16`, GREEN `10:02:58` | Declararlas |
| M-10 | M | **Higiene.** La marca `Started` del billete se escribe y nunca se lee. `WRITER` conserva el `using AcApplication`, ya sin uso. Se borró un comentario ajeno sobre `TheSystemIsRebuiltFromTheDesignText…`. Hay nombres de prueba desfasados (`TheCoreAddsExactlyTwoPublicMembersToWhatT2Left`, `TheDecoratorAnswersWaitingInsideANoticeForTheThreeReads…`) | `CORE:1521`, `:1570`; `WRITER:19`; `TW/WorkspacePilotUpdateRunTests.cs` (Base `:837`); `TW/WorkspaceBridgeFacadeSourceGuardTests.cs:233` | Limpiar en el rework |

## 1. Alcance (PD-F4-07), comparado aquí y sin fiarse del Scope

| Comprobación | Resultado |
|---|---|
| `git diff --name-only BaseSha CurrentSha` | 30 rutas |
| `AllowedWriteScope` del contrato | 30 rutas |
| Cambiadas y no permitidas | **ninguna** |
| Permitidas y no cambiadas | **ninguna**: las dos listas son iguales, conjunto a conjunto |
| Bajo `ForbiddenWriteScope` (prefijos con `/` final y archivos exactos) | **ninguna** (comprobado por programa contra las 164 entradas) |
| `git diff --name-only RedSha CurrentSha -- tests` | **vacío**: el GREEN no toca `tests/` |
| `BaseSha..RedSha` | las mismas 30 rutas: el RED ya crea todo, con el producto inerte |
| T1B (INV-16) | ninguna ruta de la clausura, las fachadas, `KindHandlers/` ni los `*Commands*.cs` |
| `DraftSession`, `DraftRegistry`, `IPushBackPilotUpdatePort`, `RackSiblingScan`, `RackPilotIntentAdapter` | sin cambios |

## 2. Las invariantes frente al código

| INV | Estado | Evidencia |
|---|---|---|
| 01 | Cumple | `TryUpdatePilot` tiene un solo llamador, `PilotUpdateRun.Execute` (`CORE:1616`). HOSTRUN no nombra `Workspace` ni T4B: grep limpio. G-4 en `IG:411-440` |
| 02 | **No cumple (B-01)** | El testigo (`CORE:1375`), el `Busy` del segundo pedido (`:1362`), la liberación por `Completed` (`:1653-1664`), la caducidad por el primer `CommandWillStart` ajeno (`ExpireStaleUpdate` `:1432-1450`, con la cuenta de `NoteForeignCommandStart`), el cierre o la retirada (`:1811`) y la degradación (`ForeignCommandStartsOf` nulo, `EBC:486-487`) son correctos. Las entradas en reposo no cuentan: `SetQuiescent` no suma (`EBC:491-497`). Las reglas del mismo rack y de otro rack están en `CORE:1357-1359` y `:1442-1444`. La llamada terminal es única por el cerrojo (`HOSTRUN:198-289`). **Falla** la cota del billete terminado sin compleción |
| 03 | Cumple | El único `Task` nuevo es `Task.CompletedTask` (`ADAPTER:45`). No hay `Dispatcher`, `Timer`, `.Result`, `.Wait(` ni `GetAwaiter`. Hay una `OnCompleted` (`:55`) y un salto por `Post` (`:93`) |
| 04 | Cumple | `ExecuteInCommandContextAsync` aparece una vez y solo en `ADAPTER:33` (`git grep` en `src`) |
| 05 | Cumple | El orden objetivo → `Admit` → `Lock` → `Begin` → `CreateWriter` → `Execute` → `Retire` → desbloqueo está en `HOSTRUN:205-287`. El documento bloqueado es el del escritor (`ADAPTER:118-120`) |
| 06 | Cumple | `WRITER:35-51`: dos campos, `_spent`, un `Begin` que lanza si ya se usó, y `Retire`. `MdiActiveDocument` ya no está. El escritor es una variable local, retirada en el `finally` (`HOSTRUN:265-275`) |
| 07 | Cumple | `NotRun` solo antes de `Execute` (`HOSTRUN:207-259`). Unknown no se traduce: `Execute` guarda lo de T4B tal cual (`CORE:1616-1617`). Las tres entradas desde AutoCAD son totales (`ADAPTER:36-43`, `:57-64`, `:96-103`; `HOSTRUN:157-194`). Ver M-06 (b) |
| 08 | Cumple, con M-01 | El reposo solo se mira en `PassGate`, al aceptar (`CORE:1365`). `Admit` mira el testigo, la lectura, el modal y `Authorize` (`:1544-1579`). Las cinco operaciones responden `Busy` o `Waiting` (`:1037`, `:1052`, `:1067`, `:1101`, `:1129`) |
| 09 | Cumple | `_ownPending` y el drenaje: `WorkspaceSession.cs:30-70`. El primer `CommandEnded` cuenta como propio y cierra (`EBC:333-344`). El cierre en `SetQuiescent` va antes de la comparación (`EBC:493`), en `Degrade` (`:539`), en `NotRun` (`CORE:1640`) y en la destrucción (`EBC:290`). La regla de olvido tiene 16 entradas (`EBC:459-482`). `NoteForeignCommandStart` va en el manejador, condicionada y tras la decisión (`AutoCadEventBridge.cs:202-208`) |
| 10 | Cumple en la forma; B-01 afecta a la cota | `CORE:1220-1221`, `:1246-1248`; `DraftRegistry` sin cambios |
| 13 | Cumple, con M-05 | **I-01:** los tres textos, iguales byte a byte a los de BaseSha (`PushBackPilotUpdateRun.cs:64-68`). **M-01:** `PushBackPilotUpdateRun.cs:148-156`, `:293-298`; `CORE:885-951`. **M-03:** `WorkspaceEditIntent.cs:497-502`. **M-06:** copia al recibir (`:514`). Ningún `.EnterError(` en Application ni en el Plugin |
| 14 | Cumple, con I-01 | G-1 a G-7, G-9 y G-10 en IG, con negativas. G-1 y G-10 están acotados |
| 15 | Cumple | Ninguna API de Undo, `SendStringToExecute`, `[CommandMethod(` ni `CommandFlags` añadidos (grep del diff) |
| 16 | Cumple | Ver §1. Las pruebas de T1B, sin cambios. Su verde es local (no he verificado la CI) |
| 17 | Cumple, con M-02 y M-08 | Ver §3 |
| 18 | Cumple, con M-09 | Las pruebas nuevas son de memoria: sin `File`, `Directory`, `AppData` ni `RackLog` |
| 19 | Cumple | Ninguna línea añadida en Application nombra `Autodesk`, `Document`, `DBObject`, `Transaction` ni `ObjectId`. El bloqueo es un `IDisposable` (`HOSTRUN:53`) |
| 20 | Cumple | Ver §4 |

**Puntos que pide la orden, comprobados aparte:**
- **`PilotBoundaryKind.Update`:** va al final del enum (`IWorkspacePilotEditPort.cs:191`) y se traduce en `CrossBoundary` (`CORE:1166`, `:1187-1189`).
- **La pureza del corredor:** HOSTRUN no contiene la palabra «Transaction», ni en comentarios. Tampoco la contiene ningún archivo de la carpeta del piloto, ni HOSTRUN nombra `Task`, `async`, `await`, `Dispatcher`, `Timer`, `Autodesk`, `Database` ni `LockDocument`.
- **El adaptador:**
  - una sola llamada a `ExecuteInCommandContextAsync`;
  - `SynchronizationContext.Current` antes de la llamada (`ADAPTER:30`);
  - un `IsCompleted`, sin `GetResult`;
  - el único campo es `_document`, de `Target` (`:111`).

## 3. Los cambios de guarda frente a D-T4C-20

Diffs `BaseSha..RedSha -- tests`. Entre RedSha y CurrentSha no cambia ninguna guarda.

| Fila | Parte | Dónde | Estado |
|---|---|---|---|
| 1 | [1] | `TW/WorkspaceNavigationHostTests.cs:426-468` | Hecha: exactamente `[ADAPTER]`, una aparición; negativas en el host de navegación, una segunda llamada y la ausencia |
| 2 | [1] | `TW/WorkspaceEditIntentHostTests.cs:1016-1042` | Hecha: exactamente `RequestUpdate` y `TakeUpdateProgress`, con un tercero como negativa |
| 3 | [1] | `:1044-1083` | Hecha: un campo de `Systems`, `_updateHost`, de su tipo exacto; con negativas |
| 4 | [1] | `:984-990` | Hecha **sin negativa** (M-02) |
| 5 | [1] | `TW/WorkspacePilotEditGuardTests.cs:61-85`, `:101-146`, `:268-329`, `:353`, `:389`, `:416-475`, más el `InlineData` de `:525` | Hecha (17 operaciones, `MemoryOperations`, `MutatingOperations`, `UpdateOperations`, la regla de `ExpireStaleUpdate` y el orden del vuelo), con negativas. Relaja «autoriza» a `PassGate` solo para `RequestUpdate`, dentro de la fila. Orden frente a `Authorize`: M-01. `:525`: M-08 |
| 6 | [1] | `:564`, `:595-596` | Hecha: `RequestUpdate` espera dentro de un aviso y `TakeUpdateProgress` es un reenvío plano; con negativas |
| 7 | [1] | `:673-674` | Hecha: los 17 reenvíos literales; con negativas |
| 8 | [1] | `TW/WorkspaceLifecycleGuardTests.cs:548-567` | Hecha: la tabla no cambia, y hay una aserción aparte con negativas |
| 9 | [1] | `TW/WorkspaceEditIntentGuardTests.cs:1128-1172` | Hecha: el censo del setter; con negativas |
| 10 | [1] | `:1174-1231` | Hecha: ocho cuerpos y nueve negativas |
| 11 | [1] | `:1233-1287` | Hecha, con una cobertura incompleta (M-03) |
| 12 | [2] | — | No tocada: correcto |
| 13 | [1] | `TW/WorkspacePilotUpdateGuardTests.cs:163-180`, `:204-207`, `:234-242`, `:295-329` | Hecha. Las cuatro negativas nuevas y `:267-276` siguen. La expresión se amplió (M-08) |
| 14 | [1] | `TW/WorkspaceBridgeFacadeSourceGuardTests.cs:206-246` | Hecha: cuatro nombres y una negativa. `:91-104` sin cambios |
| 15 | [1] | `TW/WorkspaceLifecycleGuardTests.cs:112-114`, `:140-142`, `:171`, `:195-215`, `:223`, `:321` | Hecha: de 13 a 16 entradas, con negativas, y el renombrado de C2-M-5. `:176-177` y `:679-694` sin cambios. `:223`: M-08 |
| 16 | [1] | OM `:45-131`, `:336-351` | Hecha |
| 17 | [1] | OM `:132-292` | Hecha, también con la degradación provocada dentro de `OnSourceEvent` (`:228-242`) |
| 18 | [2] | — | No tocada: correcto |
| 19 | [1] | IG completo | Hecha, salvo G-1 y G-10 (I-01) |
| 20 | [1] | `TW/WorkspaceCommandAdvisorTests.cs:82-86`, `:1157-1160`; `TW/WorkspaceHostTestSupport.cs:104-125`; `tests/RackCad.UI.Tests/Workspace/WorkspacePilotEditPortDouble.cs:344-348` | Hecha. El doble de UI es inerte, como pide D-T4C-19 |
| 21 | [2] | — | No tocada: correcto |
| 22 | [1] | IG `:836-885` | Hecha: tres negativas y el orden. Las tres guardas existentes del manejador, sin cambios |
| **Fuera del inventario** | — | `TW/WorkspacePilotUpdateHostTests.cs` y `TW/WorkspacePilotUpdateRunTests.cs` | Son pruebas de comportamiento de M-03 y M-06 que pide D-T4C-13, no guardas |

**Conclusión del inventario:**
- **No veo C07.** Los cambios que el inventario no nombra literalmente son adaptaciones o endurecimientos dentro de su fila (M-08).
- **Las guardas de D-T4C-20 (a)** viven en archivos prohibidos sin cambios. Leídas frente al código, siguen satisfechas: los literales de `SetQuiescent`, los nombres de tipo, la sesión sin handles, el drenaje, los manejadores, las nueve rutas de `Plugin/Workspace` y la asincronía. La suite completa de Core sale en verde en local (`g1_core_full` 18966/18966).

## 4. El RED

- **El producto de RedSha es un esqueleto inerte:**
  - en HOSTRUN, el `Request` devuelve `false` y los textos son `""`;
  - en ADAPTER, los cuerpos están vacíos y `Lock` y `CreateWriter` devuelven `null!`;
  - el núcleo devuelve `default`;
  - `EBC`, el enlace, la sesión, la compuerta, el controlador y el escritor tienen cuerpos inertes;
  - los textos de I-01 son `""`;
  - `EditUpdateStart`, `EditUpdateProgress` y `ClosedInError(…5)` devuelven `default`.
  
  `SetPilotUpdateHost` asigna el campo y nada más, y el controlador no lo llama.
- **El TRX del RED** es `…/R20261115T145157Z-efec/tests/r1/r1.trx`:
  - selecciona 3385 pruebas, con 3169 en verde y 216 en rojo;
  - **todos los fallos son por aserción**: 183 `Assert.*` y 33 `Assert.True` con un mensaje propio («member not found», «the executor was never asked…» o «the request was refused»), sin ninguna excepción del esqueleto.
- **Ninguna prueba existente sin cambios falla.** Las que fallan en clases existentes son nuevas, tienen el cuerpo cambiado o dependen de un ayudante de guarda que cambia por su fila: 5, 7, 13 y 15.
- **Las 23 clases de regresión y los dos filtros de UI** salen en verde en el RED, en local.
- **Las pruebas nuevas que pasan en el RED** las declara la entrega. Son prohibiciones, negativas y formas, y es lo esperable. Una mención aparte: `WithoutTheEndOfTheInnerCommand…` pasa en el RED porque el ámbito no se abre, pero discrimina en GREEN junto a las demás de OM.

## 5. Los seis textos nuevos

- **Byte a byte** contra `host-texts.txt` (UTF-8, sin BOM, comparado por programa): los seis **coinciden**, sin ninguno de más:
  - `UpdateInFlightReason`;
  - `HostNotRunReason`;
  - `HostRequestFailedPrefix`;
  - `LockFailedPrefix`;
  - `HostUnavailableReason`;
  - `UpdateNotClosedReason`.
- **CORE** no tiene ningún texto visible nuevo. Solo añade diez contextos de diagnóstico de `Report`, como los de antes, y el separador `" "` de `CORE:1593` (M-04).

## 6. Defectos lógicos buscados

- **Las carreras con el callback diferido:** el testigo, la caducidad perezosa en `Admit` (`CORE:1549`) y `ForgetUpdatesOf` cubren todo esto:
  - un comando ajeno antes del callback;
  - la destrucción;
  - un id reciclado;
  - un cambio de documento, que `TryOpenTarget` para;
  - la degradación;
  - una segunda compleción.

  Un callback tardío no hace nada.
- **Un billete sin cota:** **sí, B-01.** Los demás caminos tienen su cota.
- **Una segunda corrida:** no. Hay `Busy` por sesión, el cerrojo `Pending`/`Started`/`Ended` y el testigo.
- **Una excepción hacia AutoCAD:** no. Son totales la lambda, la continuación cruda, la acción publicada, `OnCallback` y `OnCompleted`, y el manejador va en `ContainSession`. `RackLog` y `Report` no lanzan.
- **Un resultado guardado dos veces o nunca:** una sola vez en cada camino del producto. «Nunca» solo por diseño, si escapa algo de `TryUpdatePilot` (D-T4C-05, última fila). Hay sobrescrituras posibles solo ante un anfitrión que no cumple el contrato (M-06).
- **Fugas del escritor o del bloqueo:** no. `Retire` va solo con escritor, y cada liberación en su propio `try`.
- **Guardas esquivadas:** no hay alias, concatenación ni `nameof` que esquive una guarda en el producto. Las guardas tienen huecos teóricos (M-07).
- **Pruebas que no discriminan:** ninguna tautológica que oculte un defecto. El hueco real es la falta de la prueba de B-01.

## Lo no verificado

- **La CI exacta del RED y del GREEN** (los cuatro jobs): no la consulté. Solo leí los TRX y los logs locales de la corrida. El conector de GitHub no estaba disponible, y la orden excluye ejecutar nada.
- **El comportamiento en AutoCAD** del adaptador: el `ExecutionResult`, `OnCompleted`, `Post`, `LockDocument` y `TryOpenTarget`. Solo lo cubren las guardas de texto y la compilación del Plugin, que tampoco he comprobado (los logs están en `tests/b1` y `tests/g2`). Es de Smoke-2.
- **El renombrado de C2-M-5** «según su definición en T3C2». Comprobé que el nombre nuevo describe lo que la prueba afirma, no el texto de origen.
- **La delegación, nc1 a nc4 y A1 a A8, y `prompt.md`:** fuera de esta revisión de código.
