# I-64 F4-T4C — Mapa 1: la costura de T4B, el cableado del escritor de corrida y la ausencia de una segunda vía de escritura

**Base medida:** `b15072ff` (custodia del cierre de T4B). El producto es el de `4314c180`: `git diff 4314c180 b15072ff -- src tests` sale vacío, y el árbol de trabajo del worktree está limpio.

**Método:** solo lectura. Uso `git show`, `git grep`, `git ls-tree` y la lectura de archivos. El censo de escrituras lo hace un script de solo lectura sobre el árbol de `b15072ff`, con los comentarios quitados (`_m1_write_census.py`, junto a este mapa). No compilé, no ejecuté pruebas y no abrí AutoCAD.

**Marcas:**
- **[M]** medido, con su ancla `ruta:línea` en `b15072ff`;
- **[I]** inferencia;
- **[H]** solo lo puede probar el host (AutoCAD real).

**Abreviaturas:**
- `TW` = `tests/RackCad.Tests/Workspace/`;
- `Core` = `src/RackCad.Application/Workspace/WorkspaceHostCore.cs`;
- `Run` = `src/RackCad.Application/Systems/PushBack/Pilot/PushBackPilotUpdateRun.cs`;
- `Writer` = `src/RackCad.Plugin/Views/RackPilotUpdateWriter.cs`;
- `Ctl` = `src/RackCad.Plugin/Workspace/WorkspaceController.cs`;
- `T4BP` = `docs/initiatives/I-64-f4-t4b-gate-contract-package.md`;
- `T4P` = `docs/initiatives/I-64-f4-t4-gate-contract-package.md`;
- `orden` = la orden del Owner de preparación de T4C (`owner-f4t4c-preparation-authorization.md`, no está en el repositorio; cito sus líneas).

---

## 0. Resumen

1. La costura de T4B es **un método público del núcleo**: `TryUpdatePilot(EditIntentRequest, IPushBackPilotUpdatePort)`. Recibe el puerto como parámetro y **hoy no tiene llamador de producto**. Nadie construye `RackPilotUpdateWriter` [M].
2. **El cableado choca con varias guardas a la vez** [M]:
   - el adaptador de T4C no puede ir en `src/RackCad.Plugin/Workspace/`. Esa carpeta tiene 9 archivos fijados por cinco guardas, y prohíbe `async`, `Task`, `LockDocument` y los campos de objetos del host;
   - fuera de las carpetas Workspace, ningún archivo puede nombrar `WorkspaceHostCore` ni `WorkspaceController`;
   - `ExecuteInCommandContextAsync` está prohibido en **todo** `src/`.
3. **La opción B-2 (b) de la revisión de la v1**, un campo `IPushBackPilotUpdatePort?` en el núcleo, retendría el escritor en un objeto que vive todo el proceso. Eso choca con la letra de la orden, «not retained … not cached between updates» [M e I].
4. **El escritor depende del documento activo en un único punto**, `Begin` (`Writer:37`). Todo lo demás opera sobre el `Document` que guarda el ámbito. Para que reciba el documento exacto hay que tocar `Writer`, una guarda de T4B o las dos [M].
5. **No hay hoy una segunda vía de escritura del piloto** [M]:
   - las primitivas caller-owned de las tres fachadas de Push Back solo las llama `Writer`;
   - en Application, las llamadas de escritura del piloto solo están en `Run`;
   - las carpetas Workspace no nombran ninguna API de escritura authored.

   Pero **ninguna guarda fija ese censo para todo el producto**.
6. **La corrida modifica el DWG en más de una transacción** [M]: la importación, la transacción poseída, la purga y un `SyncName` por cada vista renombrada. Lo authored va en una sola, la poseída. Ver el §11.

---

## 1. La costura exacta de T4B

### 1.1 `TryUpdatePilot`

**H-01 [M]. La firma.** `public EditUpdateOutcome TryUpdatePilot(EditIntentRequest request, IPushBackPilotUpdatePort port)` (`Core:805`). La fija por reflexión `TW/WorkspaceEditIntentHostTests.cs:1003-1021`, que comprueba:
- que es pública;
- que devuelve `EditUpdateOutcome`;
- que sus parámetros son exactamente esos dos (`:1010`);
- que `IWorkspacePilotEditPort` **no tiene ningún método cuyo nombre contenga «Update»** (`:1012`).

**H-02 [M]. Los pasos, en `Core:805-905`.**

| Paso | Código | Anclas |
|---|---|---|
| — | `request` y `port` nulos lanzan dentro del `try` | `:811-812` |
| 1 | `UpdateGate(...)`. Cualquier valor distinto de `Open`, también `Waiting`, sale como `Rejected(reason)` | `:815-816` |
| 2 | Con el rack en blanco o sin borrador, `NoDraft(NoDraftReason)` | `:819-822` |
| 3 | `PushBackPilotUpdateRun.Precheck(draft)`: `NothingToApply`, `PendingText` o `NotApplicable` | `:825-834` |
| 4 | `draft.TryBeginApply()`: `AnotherUpdateInProgress` o `Refused(nombre del rechazo)` | `:837-843` |
| — | `catch` de los pasos 1 a 4: `Report` y `Failed(ex.Message)` | `:845-849` |
| 5 | `outcome = PushBackPilotUpdateRun.Execute(draft, new PushBackPilotUpdateRequest(documentId, request.RackId), port)` | `:856` |
| 5 bis | `outcome` empieza en `NotAppliedFailure(UpdateNotRunReason)`, y el `catch` lo sobrescribe con `NotAppliedFailure(UpdateNotRunReason + " " + ex.Message)` | `:852`, `:858-862` |
| 6 | `finally { closed = draft.CompleteApply(outcome); }`, exactamente una vez | `:863-866` |
| 7a | `!closed.DraftWasRegistered` → `ClosedDetached()`, sin causa ni clasificación | `:869` |
| 7b | `!closed.Accepted` → `Closed(NotAppliedFailure, UpdateNotRunReason, [], [])`, sea cual sea el desenlace de la corrida | `:870` |
| 7c | `closed.Status == Error` → `NotePilotEdit` y `_knownClean.Remove`, y después `ClosedInError(draft.ErrorReason)` | `:872-877` |
| 7d | La correspondencia entre `closed.Kind` y `EditUpdateStatus` | `:879-890` |
| 7e | El fin de la marca de limpio, salvo en Admission, Preflight y Failure | `:892-898` |
| 7f | `Closed(status, draft.LastApplyCause, diferencias solo en un Conflicto, draft.Warnings)` | `:900-904` |

**H-03 [M]. La puerta `UpdateGate` (`Core:908-933`)** mira, en este orden:
1. el documento activo, `_probe.ActiveDocumentNativeId` (`:912-917`), con el literal de `:915`;
2. el modal de RackCad, `_modalRackCadActive` (`:919-923`), con el literal de `:921`;
3. `Authorize(new WorkspaceRequest(session)) != RequestDecision.Accepted` → `NotThisDocumentReason` (`:925-929`).

Devuelve `documentNativeId = active.Value` (`:931`), que pasa a ser `AuthorizedDocumentNativeId`. `Authorize` es `_bridge.Authorize(request, activeNativeId)` (`Core:247-248`), y llega a `WorkspaceSessionRegistry.Authorize` (`src/RackCad.Application/Workspace/WorkspaceSessionRegistry.cs:39-46`). Esa función exige que la sesión viva y que la sesión del documento que ejecuta sea la de la petición.

**Lo que no mira, por diseño:**
- el reposo, `IsActiveDocumentQuiescent`;
- el lector (D-T4B-07, `T4BP:552-557`; `TW/WorkspacePilotUpdateHostTests.cs:96`).

**Lo que tampoco mira** [M]:
- `_pilotReadInProgress`;
- la profundidad de avisos (`WorkspaceNoticeDepth`);
- `_faulted`.

**La guarda de la puerta:** `TW/WorkspacePilotUpdateGuardTests.cs:679-718`. Fija la forma exacta de `Authorize`, el orden documento → modal → autorización, y que no hay reposo ni lector. Lleva 7 negativas.

**H-04 [M]. `Precheck` y `Execute`.**
- **`Precheck`** (`Run:115-127`) es `Prepare(From(draft), null!, null!)`, y solo para con `NotApplicable`, `PendingText` y `NothingToApply`.
- **`Execute`** (`Run:132-148`):
  - con argumentos nulos da `NotAppliedFailure(MissingArgumentsReason)`;
  - si no, `new Execution(...).Run()`;
  - su `catch` exterior da siempre `NotAppliedFailure(RunFailurePrefix + ex.Message)` (`:144-147`).
- **`Run`** (`:241-280`): `Begin` (`:248`), después `Prepare ?? Mutate ?? Post` (`:255`). Su `catch` sí mira `_sealReturned` (`:258-263`), y libera el ámbito en un `finally` propio (`:264-277`).
- **MUTATE** (`:329-414`): relectura, origen, StaleBase, destino, supervivientes y capas, un `WriteUnit` por miembro y `Seal`. El ámbito de mutación se libera en su `finally` (`:350-360`).
- **POST** (`:442-475`).

**H-05 [M]. `EditUpdateOutcome`** es un `readonly struct` con `Status`, `Reason`, `Differences` y `Warnings` (`src/RackCad.Application/Workspace/WorkspaceEditIntent.cs:430-487`). Las dos listas nunca son nulas.
- **El enum** tiene 19 estados en el orden de D-T4B-07, con `Unavailable` por defecto (`:381-426`). Lo fija `TW/WorkspacePilotUpdateHostTests.cs:416-433`.
- **`Closed(...)`** valida que las listas no sean nulas, pero **guarda las referencias sin copiarlas** (`:477-483`).
- **`ClosedDetached()`** y **`ClosedInError(reason)`** no llevan clasificación, avisos ni diferencias (`:470`, `:472`).
- **`TryUpdatePilot` nunca devuelve `Unavailable`:** no aparece en `Core:805-905`.

**H-06 [M]. Sin llamador de producto:**
- `TryUpdatePilot(` en `src/` solo está en su definición y en los dos `Report` (`Core:805`, `:847`, `:860`). Las pruebas lo llaman 7 veces en `TW/WorkspacePilotUpdateHostTests.cs` y 1 en una guarda de texto.
- `new RackPilotUpdateWriter(` no aparece en ningún archivo.
- El único implementador de producto de `IPushBackPilotUpdatePort` es `Writer:32`. El otro es el doble `TW/WorkspacePilotUpdateScopeDouble.cs:19`.
- La UI no tiene ningún «Actualizar»: no hay ningún texto `Actualizar` ni `Update` de producto en `src/RackCad.UI/Workspace/`.

---

## 2. Dónde vive el núcleo en el Plugin

**H-07 [M]. Quién crea y retiene el núcleo.**
- **La creación:** `WorkspaceController` lo crea una sola vez en su constructor privado (`Ctl:41-47`) y lo guarda en `private readonly WorkspaceHostCore _host` (`Ctl:28`).
- **El controlador** es un singleton perezoso del proceso (`Ctl:24-25`, `:77-83`), creado por el primer `RACKPANEL` (`src/RackCad.Plugin/Workspace/WorkspacePanelCommand.cs:27`) o por el anfitrión de la paleta (`src/RackCad.Plugin/Workspace/WorkspacePaletteHost.cs:45`).
- **El núcleo llega a:**
  - `WorkspaceHostPort` (`Ctl:48`);
  - `WorkspacePilotEditGate`, envuelto en `PilotEditAdapter` (`Ctl:51`);
  - `WorkspaceCloseAdvisorRelay` (`Ctl:56-62`);
  - `SetEditIntentReader`, en el getter de `Navigation` (`Ctl:105`);
  - `SetModalRackCadActive` (`Ctl:138`, `:165`).
- **No lo construye ningún otro archivo:** `new WorkspaceHostCore(` solo está en `Ctl:41`.

**H-08 [M]. El reenvío de hoy.**

La UI recibe `_controller.PilotEdit` (`WorkspacePaletteHost.cs:46`). Es `PilotEditAdapter` (`Ctl:278-318`), que reenvía literalmente cada una de las 15 operaciones a la compuerta: `=> _gate.X(...)`. La compuerta (`src/RackCad.Application/Workspace/WorkspacePilotSupport.cs:153-204`) reenvía al núcleo y responde `Waiting` a las seis lecturas dentro de un aviso. El núcleo las implementa de forma explícita (`Core:943-1277`).

**H-09 [M]. Los implementadores de `IWorkspacePilotEditPort`.** Son los cinco de H-07 de T4B más el núcleo; seis en total.

| # | Clase | Ancla |
|---|---|---|
| 0 | `WorkspaceHostCore`, con implementación explícita | `Core:38`, `:943-1277` |
| 1 | `WorkspacePilotEditGate` | `src/RackCad.Application/Workspace/WorkspacePilotSupport.cs:153` |
| 2 | `PilotEditAdapter`, anidada en el controlador | `Ctl:278` |
| 3 | `CommandAdvisorPortDouble` | `TW/WorkspaceCommandAdvisorTests.cs:17` |
| 4 | `ForwardingRiskPort` | `TW/WorkspaceCommandAdvisorTests.cs:1107` |
| 5 | `WorkspacePilotEditPortDouble` | `tests/RackCad.UI.Tests/Workspace/WorkspacePilotEditPortDouble.cs:25` |

Una operación nueva en la interfaz obliga a los seis a implementarla, el núcleo incluido [M por la regla del lenguaje].

**H-10 [M]. Las guardas que acotan el cableado.**

| Qué prohíbe | Prueba y ancla | Alcance |
|---|---|---|
| Un campo **de instancia** del núcleo de tipo `RackCad.Application.Systems*` | `NoFieldOfTheHostCoreIsOfATypeOfThePilotOrOfTheResultOfTheReadAndTheFieldOfTheReaderIsNotCalledReader`, `TW/WorkspaceEditIntentHostTests.cs:1023-1043` | Reflexión con `BindingFlags.Instance` y argumentos genéricos aplanados. Los campos `static` quedan fuera: el precedente es `PilotFieldAdapter` (`Core:42`), que tiene que ser estático por `TW/WorkspacePilotEditGuardTests.cs:243-245` |
| Un método con «Update» en el puerto del panel | `TW/WorkspaceEditIntentHostTests.cs:1012` | Reflexión sobre `IWorkspacePilotEditPort` |
| Cambiar el constructor del núcleo | `TW/WorkspaceEditIntentGuardTests.cs:555-558` | La firma exacta, con texto |
| Que el puerto del panel lleve tipos del piloto o de los borradores | `PortViolations`, que busca `\bDraft\w*\|\bPushBack\w*\|RackCad\.Application\.Systems\|Workspace\.Drafts` (`TW/WorkspacePilotEditGuardTests.cs:78`, `:95-121`); la reflexión sobre el puerto y sus valores (`:180-226`) | El archivo `IWorkspacePilotEditPort.cs` y los tipos de valor. Una operación nueva `Update` **está admitida** (`:151-160`) |
| Que el soporte nombre el piloto | `TW/WorkspaceEditIntentGuardTests.cs:509-523`: en `Application/Workspace` solo el núcleo, la interfaz del lector y `WorkspaceEditIntent.cs` pueden nombrarlo. `TrapViolations` (`TW/WorkspacePilotEditGuardTests.cs:819-833`) para el puerto y el soporte | La carpeta `src/RackCad.Application/Workspace/` |
| Que la compuerta haga algo más que delegar | `GateViolations` (`TW/WorkspacePilotEditGuardTests.cs:427-455`), con 8 negativas (`:463-478`) | Solo las 15 operaciones de `Operations` (`:44-61`): una operación 16 queda fuera |
| Que el adaptador del controlador haga algo más que reenviar literalmente | `ControllerViolations`: el adaptador sobre la compuerta (`TW/WorkspacePilotEditGuardTests.cs:506-507`) y «solo delega» (`:509-519`), con negativas (`:538-555`) | Las 15 operaciones |
| Lo que no puede tener el controlador | `ControllerViolations` | `Dispatcher\|Timer\|Task\|async` en todo el archivo (`:529`); tres `+=` exactos (`:526`); un único `public IWorkspacePilotEditPort` (`:523`); ningún `public WorkspaceHostCore` (`:522`) |
| Las API de escritura en el controlador, el puente y el núcleo, con `LockDocument`, `StartTransaction` y `.Commit(` entre ellas | `TheFilesOfTheLifecycleWriteNothingAndHoldNoObjectNorTransactionOfTheHost`, `TW/WorkspaceLifecycleGuardTests.cs:679-695` | Siete archivos enteros, `Ctl` y `Core` incluidos |
| Más o menos de 9 archivos en `src/RackCad.Plugin/Workspace/` | `TW/WorkspaceEditIntentGuardTests.cs:990`; `TW/WorkspaceBrowserHostGuardTests.cs:38`; `TW/WorkspaceLifecycleGuardTests.cs:857`; `TW/WorkspaceNavigationHostTests.cs:682-690`; la lista exacta en `TW/WorkspacePaletteHostSourceGuardTests.cs:776-783` | La carpeta |
| `async` y `await` en las dos carpetas Workspace; `.Result` en el Plugin | `TW/WorkspaceNavigationHostTests.cs:435`; `TW/WorkspaceHostSourceGuardTests.cs:452-453` y `:441-442`; `TW/WorkspaceSourceGuardExceptionTests.cs:262-271` | `Plugin/Workspace` y `Application/Workspace` |
| `ExecuteInCommandContextAsync` en **todo el producto** | `TW/WorkspaceNavigationHostTests.cs:436` | `Src.AllProduct()`, es decir, todo `src/` sin comentarios |
| `ExecuteInCommandContextAsync`, `async`, `Task` o `Dispatcher` en los archivos del puerto y en `Core` y `Ctl` | `TW/WorkspacePilotPrepareGuardTests.cs:407-430` | Ocho archivos exactos |
| Un campo de objeto del host (`Document`, `Transaction`, `ObjectId`…), o un `LockDocument` o `StartTransaction` fuera de un `using` | `TW/WorkspaceHostSourceGuardTests.cs:467-477` | `Plugin/Workspace` |
| Que algo de fuera de las carpetas Workspace nombre `WorkspaceController`, `WorkspaceHostCore`, `WorkspaceHostPort` o `WorkspaceSelectionReader` | `NothingOutsideThePluginWorkspaceReachesTheController`, `TW/WorkspaceHostSourceGuardTests.cs:155-166` | Todo `src/` |
| `Regen`, `Zoom` o `SetSystemVariable`; `RackBlockData.Write` y `RackEmbedComposer` | `TW/WorkspaceHostSourceGuardTests.cs:382-397` y `:373-377` | Las dos carpetas Workspace |

**H-11 [M]. La opción B-2 (b).**
- **Dónde:** `docs/automation/evidence/I-64-pilot/F4-DRAFTS/t4b/independent-review.md:45-61`, la revisión de la v1 de T4B.
- **Qué dice** (`:58`): «Un cambio declarado de la guarda: exactamente un campo `IPushBackPilotUpdatePort?`, con su autoprueba en negativo (un segundo campo, u otro tipo de `Systems`, la hacen saltar). El setter nuevo, con su propio censo, como el de `SetEditIntentReader`».
- **Las otras dos opciones:**
  - (a), el puerto como parámetro, que es la elegida (`T4BP:858`);
  - (c), la interfaz declarada en `WorkspaceEditIntent.cs` (`:59`).
- **Lo recoge** la re-revisión de la v2, R-M-12 (`docs/automation/evidence/I-64-pilot/F4-DRAFTS/t4b/independent-rereview.md:270-276`). La cita `T4BP:815` y §9 I-2 (`T4BP:836`).
- **El precedente del setter** es `SetEditIntentReader`: `Core:275-276`, con su censo en `TW/WorkspaceEditIntentGuardTests.cs:490-505`.

**H-12 [M]. La autoridad de T4P sobre el lugar del adaptador.** R-T4-05 dice: «`ExecuteInCommandContextAsync` solo en el archivo exacto del adaptador, nuevo en `Plugin/Views`; las carpetas Workspace siguen sin `async`, `await` ni `.Result`» (`T4P:547-550`). C-F4-02 lo pide «con excepción por archivo exacto y aviso al Owner de que se reintroduce» (`docs/initiatives/I-64-f4-gate-contract-package.md:59`).

**[I] La consecuencia.** El adaptador en `Plugin/Views` no puede nombrar `WorkspaceHostCore` (H-10, `TW/WorkspaceHostSourceGuardTests.cs:155-166`). Para llamar a `TryUpdatePilot` necesita una de estas dos cosas:
- recibir un delegado o una interfaz que no lo nombre, desde el controlador;
- un cambio declarado de esa guarda.

A la vez, el controlador no puede contener `LockDocument` ni `Task` (H-10). Las dos restricciones se encadenan.

**Opciones de cableado, sin cerrar ninguna:**

| Opción | Qué es | A favor | En contra |
|---|---|---|---|
| W-1 | B-2 (b) literal: un campo `IPushBackPilotUpdatePort?` en el núcleo y un setter | El núcleo implementa la operación 16 por sí mismo | Retiene una instancia del escritor todo el proceso, contra «not retained … not cached» (orden:156-166). Cambia la guarda de campos (`TW/WorkspaceEditIntentHostTests.cs:1023-1043`). Y el núcleo seguiría sin poder entrar en el contexto de comando |
| W-2 | Un campo del núcleo con una **fábrica** declarada en `WorkspaceEditIntent.cs`, un tipo de `Workspace` que devuelve el puerto | La fábrica queda retenida, pero no el escritor | Sigue habiendo un campo nuevo y una autoridad persistente del host (§11); tampoco resuelve el contexto de comando |
| W-3 | Sin campo. El adaptador del controlador o la compuerta interceptan la operación 16, y el adaptador de `Plugin/Views` recibe un delegado `(request, port) => _host.TryUpdatePilot(request, port)` | Ni el núcleo ni sus guardas de campos cambian; el escritor nace y muere dentro de la llamada de retorno | La implementación explícita del núcleo para la operación 16 no tendría puerto que pasar. La compuerta y el adaptador dejarían de «solo reenviar» en esa operación. Hace falta decidir si la operación pasa por la profundidad de avisos |
| W-4 | Un puerto de panel aparte, que no sea `IWorkspacePilotEditPort` | No toca los seis implementadores | Contradice la letra de `T4BP:814` («la operación del puerto `IWorkspacePilotEditPort` con sus cinco implementadores»), cambia el constructor del VM (guardas de UI) y añade una segunda superficie |

---

## 3. El escritor de corrida (`Writer`)

**H-13 [M]. La estructura.** Tres clases (`TW/WorkspacePilotUpdateGuardTests.cs:229-237`):
- **el puerto** `RackPilotUpdateWriter` (`Writer:32-52`), sin campos;
- **el ámbito de la corrida** `RackPilotUpdateScope` (`:58-209`), con los campos `Document? _document`, `_documentMatches`, `_definitions`, `_prepared`, `_staleDefinitions` y `_catalog` (`:60-66`);
- **el ámbito de mutación** `RackPilotMutationScope` (`:215-338`), con `Database`, `Transaction`, `_prepared` y `_staleDefinitions` (`:220-225`).

**H-14 [M]. El único uso del documento activo.**
- `Begin` es `return new RackPilotUpdateScope(AcApplication.DocumentManager.MdiActiveDocument, request);` (`Writer:37`).
- El ámbito compara una sola vez `document.UnmanagedObject.ToInt64() == request.AuthorizedDocumentNativeId`, en su constructor (`:68-72`).
- Después solo usa `_document`, en `:82`, `:95`, `:139`, `:153-154`, `:167`, `:185` y `:193`.
- `ValuesOf` construye **siempre** `DocumentMatches = true` (`:50`), así que la comprobación de documento de `Reread` en la corrida (`Run:378`) es vacía con el escritor real. Ya lo observó la revisión independiente de T4B (`independent-review.md:129`).
- En todo `Writer` no hay ningún otro `MdiActiveDocument`, `DocumentManager` ni `WorkingDatabase`.

**H-15 [M]. Las guardas que fijan que el puerto no tiene campos:**
- `WriterProhibitionViolations` (`TW/WorkspacePilotUpdateGuardTests.cs:185-188`). Sobre el texto de la clase del puerto prohíbe `\b(?:Transaction|DBObject|Document|ObjectId|Database|PreparedViewRedraw)\b|staleDefinitions` y `\bprivate\b|\breadonly\b`;
- su negativa (`:267-276`), que inyecta **literalmente** `private Document _document;` en el puerto y exige una violación;
- `Begin` tiene que devolver `return new RackPilotUpdateScope(` (`:216`).

**Qué haría falta para que el escritor reciba solo el documento exacto admitido.** Son opciones, sin cerrar ninguna.

| Opción | Cambio | Guardas afectadas [M] | A favor y en contra [I] |
|---|---|---|---|
| D-1 | Un constructor `RackPilotUpdateWriter(Document)` y un campo | Las dos de H-15: la prohibición y su negativa literal. Sería un cambio de guarda de T4B | A favor: es el precedente de `AutoCadProjectionWriteScopeFactory(Document)` (`src/RackCad.Plugin/Views/RackProjectionWriteScope.cs:17-21`, creado por orden en `src/RackCad.Plugin/Views/RackProjectionCommandPort.cs:28`) y de `AutoCadSiblingRedrawPort(Document, …)` (`src/RackCad.Plugin/Systems/Shared/AutoCadSiblingRedrawPort.cs:8-19`, creado en `src/RackCad.Plugin/RackSelectivoInsertIntegration.cs:162`). En contra: toca un archivo de producto y una guarda de T4B («Do not reopen T4B», orden:49) |
| D-2 | `Begin` resuelve el documento por `request.AuthorizedDocumentNativeId`, enumerando `DocumentManager` dentro de la clase de ámbito o en una ayuda | Probablemente ninguna: un identificador como `FindDocument` no casa con `\bDocument\b` | A favor: el puerto sigue sin campos. Los precedentes de la búsqueda por identidad nativa son `AutoCadEventBridge.FindDocumentOrNull` (`src/RackCad.Plugin/Workspace/AutoCadEventBridge.cs:335-343`) y `Ctl:204-215`. En contra: el escritor **vuelve a buscar** el documento en lugar de recibir el bloqueado; la coherencia entre el bloqueo y la escritura depende de que las identidades coincidan; toca `Writer` |
| D-3 | Una clase por corrida en el archivo del adaptador de T4C que implementa `IPushBackPilotUpdatePort` con el `Document` y llama a `new RackPilotUpdateScope(document, request)` (su constructor es `internal`, `Writer:68`) | Ninguna de `Writer`; el censo de implementadores de `IPushBackPilotUpdatePort` pasa a dos | A favor: no toca `Writer`. En contra: un segundo puerto de producto, y `RackPilotUpdateWriter` queda sin uso; la guarda de «sin segunda vía» tendría que definir que el escritor primitivo son los dos ámbitos de `Writer` |
| D-4 | Sin cambio: T4C comprueba `MdiActiveDocument == documento bloqueado` antes de llamar, y la comparación del constructor del ámbito (`:71`) queda como segunda línea | Ninguna | A favor: cero cambios en T4B. En contra: no es literalmente «receives only the exact current document» (orden:162); depende de qué devuelve `MdiActiveDocument` dentro del contexto de comando [H] |

**H-16 [M]. El ciclo de vida de hoy.**
- **El puerto:** nadie lo crea (H-06). No es `IDisposable` y no guarda nada. `Begin` se puede llamar cualquier número de veces, porque no hay una marca de un solo uso. Una marca así sería un campo, y lo prohíbe H-15.
- **El ámbito de la corrida:** lo crea `Begin`, dentro de `Execute`. Eso ocurre después de `UpdateGate`, del borrador, del `Precheck` y de `TryBeginApply` (`Core:815-843`, `:856`; `Run:248`). Lo libera siempre el `finally` de `Run` (`Run:264-277`). Su `Dispose` solo pone `_disposed` y vacía los diccionarios (`Writer:197-203`): no hay recurso de AutoCAD que liberar. Después, `EnsureOpen` lanza `ObjectDisposedException` (`:205-208`).
- **El ámbito de mutación:** lo crea `BeginMutation`, después de `StartTransaction` (`:143-144`). Lo libera el `finally` de `Mutate` (`Run:350-360`). Su `Dispose` hace `_transaction.Dispose()` (`Writer:297-304`), que es el aborto si no hubo `Seal`. `EnsureOwned` lanza si está liberado o si la `TopTransaction` no es la suya, comparada por `UnmanagedObject` (`:307-313`).
- **El resquicio teórico** (`independent-review.md:157`): una excepción entre `StartTransaction` y la construcción del ámbito, que solo asigna campos.

**[I] Qué significa para la orden.**
- «created after host admission»: lo cumple ya **el ámbito**, no el puerto. Como el puerto es un **parámetro** de `TryUpdatePilot`, T4C tiene que crearlo **antes** de `UpdateGate`. O hace su propia admisión antes, o cambia la firma (H-01, una guarda de T4B).
- «disposed/released on every path»: lo cumplen los ámbitos.
- «cannot be reused after run completion»: el puerto de hoy **no** lo cumple, porque se puede volver a llamar a `Begin`.
- «not retained by Workspace controller»: ninguna guarda prohíbe hoy un campo de tipo `IPushBackPilotUpdatePort` o `RackPilotUpdateWriter` en `Ctl`. La de H-10 (`TW/WorkspaceHostSourceGuardTests.cs:474`) solo prohíbe los tipos de AutoCAD.

**H-17 [M]. ¿Puede escapar un objeto de AutoCAD por la interfaz de Application?**
- **De forma estructural, no.** `src/RackCad.Application/RackCad.Application.csproj` solo referencia `RackCad.Domain`, así que ninguna firma de Application puede nombrar un tipo de AutoCAD.
- **Los valores del puerto son de Application o de Domain:** `RackSiblingScanFact` (`src/RackCad.Application/Views/Redraw/RackSiblingMembership.cs:16-40`, cadenas, booleanos y enteros), cadenas, `RackCatalog` y diccionarios de `bool` (`src/RackCad.Application/Systems/PushBack/Pilot/IPushBackPilotUpdatePort.cs:55-188`).
- **Sin reflexión:** la pureza de `Run`, del puerto y de la ayuda prohíbe `System.Reflection` y `typeof(` (`TW/WorkspacePilotUpdateGuardTests.cs:57-72`).
- **[I] Lo que sí cruza en tiempo de ejecución:**
  - la **referencia de interfaz** al ámbito de mutación (`PushBackPilotMutationStart.Scope`), que contiene la `Transaction`. Vive en una variable local de `Mutate` y se libera en su `finally`;
  - las **excepciones** de AutoCAD, como `Exception`. La corrida solo conserva su `Message` (`Run:146`, `:262`, `:348`), y el núcleo pasa el objeto a `_logFault` (`Core:1521-1531`), que es el `RackLog` del Plugin (`Ctl:275`).

  Nada se guarda después de la corrida: `Execution` es local de `Execute` (`Run:142`), y el núcleo no guarda `port` (H-10).

---

## 4. Las dependencias de lectura y de catálogo de la corrida

**H-18 [M].**

| Dependencia | Ancla | ¿Qué documento usa? | Bloqueo y escritura |
|---|---|---|---|
| `Begin` | `Writer:37` | **El activo** (`MdiActiveDocument`) | — |
| El catálogo de PREPARE: `LateralHeaderDrawService.LoadCatalog()` → `RackCatalogLoader.Load()` → `JsonRackCatalogProvider.FromBaseDirectory().Load()` | `Writer:81`; `src/RackCad.Plugin/Drawing/LateralHeaderDrawService.cs:299`; `src/RackCad.Plugin/RackCatalogLoader.cs:17-43` | Ninguno: son archivos del directorio base | Sin bloqueo. Si falla, **devuelve un catálogo vacío** y escribe en el registro (`:26-30`); con un catálogo sin perfiles ni bloques, solo un aviso (`:33-40`). No devuelve nulo, así que el `?? throw` de `Writer:81` es inalcanzable [I] |
| `CaptureDrawing(document)` → `Traverse` | `Writer:82`, `:193`; `src/RackCad.Plugin/Views/RackSiblingScan.cs:96-110` | El dado (`_document`) | Su propio `LockDocument`, anidado bajo el de T4C, y su transacción con `Commit`, de solo lectura |
| Las tres `PrepareRedraw` → `ViewBlockDraw.PrepareRedraw` | `Writer:111`, `:114`, `:117`; `src/RackCad.Plugin/Systems/Shared/ViewBlockDraw.cs:66-89` | La `Database` dada | **Un segundo catálogo**, `RackCatalogLoader.Load()` (`:84`). **Importa:** `BlockLibraryImporter.EnsureForPlan(database, built)` (`:86`). El comentario dice «Must be called inside the document lock» (`:62-63`) |
| La importación | `src/RackCad.Plugin/Drawing/BlockLibraryImporter.cs:25-38`, `:51-130` | La `Database` dada | La ruta de la biblioteca sale de la configuración del usuario (`:23`), con `File.Exists` (`:88`) y la base de datos de la biblioteca en caché (`:96`). Escribe: una transacción con `Commit` (`:68-79`) y `WblockCloneObjects` (`:123`). No toma bloqueo propio |
| `PushBackPilotSystemRebuild.Build(designText, catalog)` | `Writer:106`; `Run:205` | — | Pura, con el catálogo de PREPARE |
| `Reread` → `TraverseCore(_transaction, _database, ObjectId.Null, null)` | `Writer:240-248` | La transacción poseída | Sin bloqueo ni transacción propios, por INV-39 |
| `WriteUnit` → `RedrawInTransaction` → `SystemBlockWriter.RedefineInTransaction` | `Writer:263-274`; `ViewBlockDraw.cs:97-109`; `src/RackCad.Plugin/Systems/Shared/SystemBlockWriter.cs:126-138` | La `Database` y la transacción poseídas | La escritura authored: geometría y `RackBlockData.Write` |
| La purga, `PurgeAfterCommit` → `LateralHeaderDrawer.PurgeUnreferenced` | `Writer:162`; `SystemBlockWriter.cs:147-150`; `src/RackCad.Plugin/Drawing/LateralHeaderDrawer.cs:193-238` | La `Database` dada | Su propia transacción, con `Purge`, `Erase` y `Commit`. **No toma bloqueo:** depende del de T4C |
| `SyncName(document, …)` | `Writer:165-168`; `src/RackCad.Plugin/Drawing/RackBlockRenamer.cs:21-58` | El dado | Su propio `LockDocument` y su transacción, con `UpgradeOpen` y `Commit`, **una por clave renombrada** |
| La lectura de nombres | `Writer:172-179` | La `Database` dada | `StartOpenCloseTransaction` con `ForRead` |
| `ApplyRegen(document, true)` → `document.Editor.Regen()` | `Writer:185`; `SystemBlockWriter.cs:157-163` | El `Editor` del documento dado | — |
| El documento de la puerta | `Core:912` → `HostProbe.ActiveDocumentNativeId` (`Ctl:356-363`) | **El activo** | — |
| El reposo, para la puerta que T4C pone antes del contexto de comando | `Ctl:367-374` | **El activo**, con `Editor.IsQuiescent` y `_bridge.IsCommandActiveInActiveSession` | — |
| La autorización de la sesión | `Core:247-248` → `src/RackCad.Plugin/Workspace/AutoCadEventBridge.cs:105` → `WorkspaceSessionRegistry.cs:39-46` | La identidad nativa activa | — |

- **[I]** Dentro de `Writer`, `Begin` es lo único que depende del documento activo. Todo lo demás depende del `_document` del ámbito, o de su `Database` y su transacción.
- **[I]** La importación y la purga **dependen del bloqueo de T4C**, porque no toman el suyo.
- **[H]** Si las llamadas anidadas de `LockDocument` (`CaptureDrawing` y `SyncName`) bajo un bloqueo de escritura en el contexto de comando se comportan como se espera; también la pertenencia de las transacciones de la importación, la purga y `SyncName` al mismo paso de Undo.
- **H-19 [M]. Hay dos cargas del catálogo por corrida.** Una de PREPARE (`Writer:81`) para leer, planificar, reconstruir el sistema y calcular los nombres deseados, y otra por cada `PrepareRedraw` (`ViewBlockDraw.cs:84`), es decir, por miembro. Está declarado (§9 I-8, `T4BP:842`).

---

## 5. No hay una segunda vía de escritura

### 5.1 El censo, en `src/RackCad.Plugin` y `src/RackCad.Application`

**H-20 [M].** El script `_m1_write_census.py` cuenta, sin comentarios: `RedrawInTransaction(`, `RedrawInPlace(`, `DrawAndPlace(`, `SystemBlockWriter.`, `RackBlockData.Write`, `SetXrecord`, `UpgradeOpen(`, `OpenMode.ForWrite`, `.Commit(`, `StartTransaction(`, `LockDocument(`, `PurgeUnreferenced(`, `EnsureForPlan(`, `WblockCloneObjects`, `SyncName(`, `AppendEntity(` y `.Erase(`.
- 61 archivos tienen alguno.
- `SetXrecord`: **0** en todo el producto. Los Xrecords se escriben con `DBDictionary.SetAt` en `src/RackCad.Plugin/Systems/Shared/RackBlockData.cs:30-49`, `src/RackCad.Plugin/ProjectVariablesData.cs:126-132` y `src/RackCad.Plugin/CustomPropertiesData.cs:151-155`.

**Por camino:**

| Camino | Archivos y conteos | Clasificación |
|---|---|---|
| **Workspace (Plugin)** | `WorkspaceNavigationEffects.cs` (StartTransaction 2, LockDocument 2), `WorkspaceNavigationFacts.cs` (1 y 1), `WorkspaceNavigationHost.cs` (LockDocument 2) y `WorkspaceSelectionReader.cs` (1 y 1). **Sin** `ForWrite`, `UpgradeOpen`, `.Commit(`, `AppendEntity`, `RackBlockData.Write`, `Redraw*`, `DrawAndPlace`, `SystemBlockWriter`, `SyncName`, purga ni importación | Lecturas y el efecto de navegación (selección implícita y vista; `TW/WorkspaceHostSourceGuardTests.cs:324-333`). **Nada authored** |
| **Workspace (Application)** | Nada | Application no referencia AutoCAD (H-17) |
| **El piloto (Plugin)** | `Writer`: `RedrawInTransaction(` 3, `SystemBlockWriter.` 2 (la purga y el regen), `.Commit(` 1, `StartTransaction(` 1, `SyncName(` 1, `PrepareRedraw(` 3 (`:111`, `:114`, `:117`) y `StartOpenCloseTransaction` 1. `RackPilotIntentAdapter.cs` (StartTransaction 1, LockDocument 1) y `RackSiblingScan.cs` (1, 1, `.Commit(` 1) son de solo lectura | La única vía authored del piloto |
| **El piloto (Application)** | Solo `Run`: `PrepareUnits` (`:321`), `BeginMutation` (`:331`), `WriteUnit` (`:405`), `Seal` (`:411`) y `Post` (`:448`) | La corrida pura |
| **Las primitivas caller-owned de Push Back** | `PushBack{System,Frontal,Planta}DrawService.PrepareRedraw(` y `.RedrawInTransaction(` tienen **un único llamador de producto, `Writer`** (`:111`, `:114`, `:117`, `:266`, `:269`, `:272`). `RackPushBackCommands.cs` usa los envoltorios públicos `DrawAndPlace` y `RedrawInPlace` (`:111`, `:124`, `:129`, `:279`, `:297`, `:336`, `:432`) | La separación entre el clásico y el piloto |
| **El clásico de Push Back** (T1B) | `RackPushBackCommands.cs` (`RedrawInPlace(` 3, `DrawAndPlace(` 4, `SyncName(` 3); `KindHandlers/PushBackKindHandler.cs` reenvía a `EditPushBack`; `RackMenuCommands.cs` y `Views/RackViewBatchProducts.cs` | Fuera del piloto. Lo congelan CT-25 a CT-27 |
| **I-55** | `RackSelectivoInsertIntegration.cs` (`RedrawInTransaction(` 3, `SyncName(` 1, `.Erase(` 2), `Systems/Shared/SiblingRedrawTransaction.cs` (1 y 1) y `AutoCadSiblingRedrawPort.cs` (LockDocument 3). En Application, `Views/Redraw/RackSiblingRedrawRun.cs:95` (`port.Mutate`) | Fuera del piloto |
| **RACKPROYECTAR** | `Views/RackProjectionWriteScope.cs` (1 y 1, LockDocument 1, AppendEntity 1). En Application, `Views/Placement/RackProjectionMaterialization.cs:256`, `:272` y `:287` | Fuera del piloto |
| **El resto** | Los demás `*Commands*.cs`, los `*DrawService` de Selectivo, Dinámico, FlowBed y Cabecera, `ProjectVariableMutationExecutor.cs`, `CustomPropertiesExecutor.cs`, `RackCloner.cs`, `BlockPlacement.cs`, `RackViewPlacement.cs`, `RackNewRackName.cs`, `InDocumentTransaction.cs`, etc. | Fuera del piloto |

### 5.2 Lo que escribe una corrida del piloto

**H-21 [M].** Una corrida modifica el DWG en estas transacciones, todas bajo el bloqueo de T4C:
1. **PREPARE:** la importación de bloques de la biblioteca, por miembro: la transacción de `EnsureBlocks` y `WblockCloneObjects` (H-18). Es el residuo OM-5 declarado. No es authored.
2. **MUTATE:** **la única transacción poseída**, `StartTransaction` (`Writer:143`) y `Commit` en `Seal` (`:293`), con la geometría y el sobre `RackBlockData.Write` de cada miembro. Es lo authored.
3. **POST:**
   - la purga, en su transacción, sin authored (§2.4 de `T4BP`, categoría (b));
   - `SyncName`, **una transacción por cada clave que se renombra**, que cambia `BlockTableRecord.Name`. El clásico lo trata como «cosmético» (`RackBlockRenamer.cs:9-14`);
   - el `Regen`.

**[I]** Lo authored del rack, la geometría y el sobre, va en una sola transacción. El nombre del bloque va aparte, como en el clásico. Si el paquete trata el nombre como authored, hay más de una transacción authored (§11).

**[H]** Si las tres fases forman un solo paso lógico de Undo dentro del contexto de comando.

### 5.3 Las guardas que ya acotan las escrituras

**H-22 [M].**

| Guarda | Ancla | Alcance exacto |
|---|---|---|
| `CallerOwnedFacadeGuardTests` | `tests/RackCad.Tests/CallerOwnedFacadeGuardTests.cs:50-61`, `:102-199`, `:204-215` | Siete fachadas: `ViewBlockDraw`, las dos de Selectivo, `LateralHeaderDrawService` y las tres de Push Back. La primitiva no confirma, no abre transacción, no bloquea, no regenera ni importa, y no baja por un envoltorio self-owned. La preparación importa. Los envoltorios conservan su ciclo. **No censa llamadores** |
| `CallerOwnedTransactionGuardTests` | `tests/RackCad.Tests/CallerOwnedTransactionGuardTests.cs:48-50`, `:197-238` | Solo `SystemBlockWriter` y `LateralHeaderDrawService`, con el orden commit < purga < regen. No mira al piloto |
| `RackSiblingRedrawUnitsTests` y `SiblingRedrawSeamGuardTests` | `tests/RackCad.Tests/SiblingRedrawSeamGuardTests.cs:7-35` y `:58-81` | El adaptador de I-55: una transacción y un `Commit`, y ningún `*Commands*.cs` nombra `SiblingRedrawTransaction` (`:61-68`). No mira al piloto |
| `DebugFaultInjectionGuardTests` | mismo archivo, `:37-56` | Solo la inyección de I-55 |
| CT-27 (1) y (2) | `tests/RackCad.Tests/PushBackClassicCharacterizationPluginGuardTests.cs:314-363` y `:365-389` | Las firmas exactas de las tres fachadas de Push Back, y que `RackPushBackCommands.cs`, `PushBackKindHandler.cs`, `RackMenuCommands.cs` y `RackViewBatchProducts.cs` no llaman a `PrepareRedraw(` ni a `RedrawInTransaction(`. **No impide** otros llamadores |
| La prohibición del escritor | `TW/WorkspacePilotUpdateGuardTests.cs:164-193` | Solo `Writer`: sin `RackBlockData.Read`, `BlockTableId`, `LockDocument`, `Erase(`, `EraseViewBlocks`, `RackUnitsGuard`, `async`, `ExecuteInCommandContextAsync`, `ForWrite`, `UpgradeOpen` ni `AppendEntity`; `PrepareRedraw` solo en `PrepareUnits`; la purga solo por `PurgeAfterCommit`; el puerto sin campos |
| La estructura del escritor | `:196-227` | Un `StartTransaction` (en `BeginMutation`), un `StartOpenCloseTransaction` y un `PurgeAfterCommit` (en `Post`), un `.Commit(` (en `Seal`), y el orden de `Post` |
| La pureza de la corrida, del puerto y de la ayuda | `:57-125` | Sin `Transaction` (ni en comentarios), `Database`, `.Commit(`, `LockDocument`, `Autodesk` ni reflexión |
| `TryUpdatePilot` sin API de escritura | `ReconciliationBodies` + `WriteViolations` (`TW/WorkspacePilotEditGuardTests.cs:686-735`) | El cuerpo de `TryUpdatePilot`, las 15 operaciones explícitas y el decorador |
| Los archivos del ciclo de vida sin escrituras | `TW/WorkspaceLifecycleGuardTests.cs:679-695` | Siete archivos enteros: `Core`, `Ctl`, el puente, el núcleo del puente, el consejero, la intención y el registro |
| La carpeta Workspace sin estampar un `RackId` | `TW/WorkspaceHostSourceGuardTests.cs:373-377` | Las dos carpetas Workspace: sin `RackBlockData.Write`, `RackEmbedComposer` ni `new RackEmbedDocument` |
| La carpeta Workspace sin selección, vista ni regen | `TW/WorkspaceHostSourceGuardTests.cs:382-397` | Las dos carpetas, salvo los efectos de navegación |
| Los efectos de navegación sin nada authored | `TW/WorkspaceNavigationHostTests.cs:652-663`; `TW/WorkspaceHostSourceGuardTests.cs:324-333` | Archivos exactos |
| El lector de la selección y los hechos de navegación sin escritura | `TW/WorkspaceHostSourceGuardTests.cs:238-244`; `TW/WorkspaceNavigationHostTests.cs:550-563` | Archivos exactos |
| La programación del Idle sin escritura (G-8) | `TW/WorkspaceIdleSchedulingGuardTests.cs:596-635` | Tres archivos exactos |
| La composición para escribir, solo desde el planificador | `TW/WorkspacePilotPrepareGuardTests.cs:155-190` | `PushBackPilotMaterializer.ComposeForWrite` |
| Nada desde el drenaje ni el Idle | `TW/WorkspaceEditIntentGuardTests.cs:598-641` | `TryUpdatePilot` está en las expresiones del drenaje del núcleo y de los dos Idle (`:604`, `:608`, `:610`). **No está** en la del drenaje del controlador (`:617`) ni en la del anfitrión de navegación (`:613`) |

**[I] El hueco.** Ninguna guarda fija, para todo el producto, que:
- `TryUpdatePilot` tiene exactamente un llamador;
- `RackPilotUpdateWriter` se construye en un solo sitio;
- `IPushBackPilotUpdatePort` tiene un solo implementador de producto;
- las primitivas caller-owned de Push Back solo las llama `Writer`;
- `PushBackPilotUpdateRun.Execute` tiene un único llamador. Hoy solo lo fija el cuerpo, con `TW/WorkspacePilotUpdateGuardTests.cs:735`.

### 5.4 Lo que tendría que comprobar una guarda nueva (propuesta de contenido, no de diseño)

Para demostrar que la única escritura authored del piloto pasa por el escritor de T4B, cada fila con su autoprueba en negativo sobre el texto real:
- **G-1. El censo de producto de las primitivas de Push Back.**
  - `PushBack(System|Frontal|Planta)DrawService` + `.PrepareRedraw(` o `.RedrawInTransaction(` solo en `Writer`, en `PrepareUnits` y `WriteUnit`.
  - Negativa: inyectarlo en el adaptador de T4C o en `Ctl`.
- **G-2. Un solo implementador de producto de `IPushBackPilotUpdatePort`** (hoy `Writer:32`). Si se elige D-3, son dos, nombrados.
  - Negativa: una clase nueva `: IPushBackPilotUpdatePort` en otro archivo.
- **G-3. Una sola construcción del escritor** (o de su fábrica), en el archivo exacto del adaptador de T4C:
  - dentro de la llamada de retorno del contexto de comando y del `using` del bloqueo;
  - pasada directamente a `TryUpdatePilot`;
  - nunca asignada a un campo, ni estático ni de instancia.

  Negativas: un campo `IPushBackPilotUpdatePort` o `RackPilotUpdateWriter` en `Ctl`, en el adaptador o en el núcleo; una segunda construcción.
- **G-4. Un solo llamador de producto de `TryUpdatePilot(`**, el adaptador de T4C, y una sola llamada.
  - También: `PushBackPilotUpdateRun.Execute(` solo en `Core`.
  - Y la extensión de `:617` y `:613`: `TryUpdatePilot` tampoco en `OnDrained`, `NotifyStateChanged`, `OnActiveSessionChanged` ni `OnCommandStateChanged` del controlador, ni en los manejadores del adaptador.
- **G-5. El adaptador de T4C sin API de escritura authored:** sin `RedrawInTransaction`, `RedrawInPlace`, `DrawAndPlace`, `SystemBlockWriter`, `RackBlockData.Write`, `ForWrite`, `UpgradeOpen`, `AppendEntity`, `.Commit(`, `StartTransaction`, `SyncName`, `PurgeUnreferenced`, `EnsureForPlan`, `SetAt(` ni `.Erase(`.
  - Solo un `LockDocument` y un `ExecuteInCommandContextAsync`.
  - Y sin ninguna referencia a `RackPushBackCommands`, `EditPushBack`, `PushBackKindHandler`, `RackMenuCommands`, `RackViewBatch*`, `SiblingRedraw*`, `AutoCadSiblingRedrawPort`, `RackSelectivoInsertIntegration` ni `RackCommandSupport`.
- **G-6. Las carpetas Workspace siguen sin escrituras authored.** Hoy las cubren guardas por archivo (H-22); falta una regla única sobre las dos carpetas con la lista de G-5.
- **G-7. Las llamadas de escritura del puerto en Application** (`.PrepareUnits(`, `.BeginMutation(`, `.WriteUnit(`, `.Seal(`, `.Post(` sobre el puerto del piloto) solo en `Run`.
- **G-8, opcional y frágil [I]:** el conjunto de archivos de `src/` con `.Commit(` no crece salvo por los declarados.

---

## 6. Los arrastres de T4B

**H-23 [M]. I-01: los tres textos del host.**
- **Dueño:**
  - `Core:48` (`UpdateNotRunReason`), que se usa en `:852`, `:861` y `:870`;
  - los literales de `UpdateGate` en `:915` y `:921`.
- **Dónde está la superficie aceptada:** las constantes de `Run:50-107`.
- **La guarda que no los ve:** `TheUpdateNeverReadsTheDrawing…` (`TW/WorkspaceEditIntentGuardTests.cs:1101-1111`; los textos, en `:1108-1110`). Solo lee el cuerpo de `TryUpdatePilot`, y prohíbe `EditUpdateOutcome.(Rejected|NoDraft|…|Failed)("…`. El motivo llega por `out reason` desde `UpdateGate`, y `Closed(…, UpdateNotRunReason, …)` no está en la lista.
- **Disposición:** el Owner la ratificó y la pasó a T4C (`docs/automation/evidence/I-64-pilot/F4-T4B-MUTATE-POST/task-close-package.md:91-97`); la orden la repite (orden:189-196).
- **¿T4C lo toca de forma natural?** [I] Sí, si toca `Core` para el cableado (W-1 a W-3) o `Run` para M-01.
- **Lo que se mide al mover los textos:**
  - la tabla de 30 literales (`TW/WorkspaceHostHardeningGuardTests.cs:470-500`) **no cambia**: cuenta solo sus 30 textos, que ya excluyen estos tres;
  - **ninguna prueba** cita estos tres textos (`git grep` sin coincidencias fuera de `Core`).
- **La guarda discriminante:**
  - el cuerpo de `UpdateGate` sin `reason\s*=\s*"`;
  - `Core` sin `UpdateNotRunReason` y sin ningún literal de cadena en `TryUpdatePilot`;
  - cada texto como `PushBackPilotUpdateRun.<Constante>`.

  Negativas: inyectar `reason = "x";` en `UpdateGate` y `Closed(EditUpdateStatus.NotAppliedFailure, "x", …)` en el cuerpo.

**H-24 [M]. M-01: la totalidad defensiva.**

Hay tres lugares, no dos:
1. **El `catch` exterior de `Execute`** (`Run:144-147`): `NotAppliedFailure` sin mirar `_sealReturned`, que vive en `Execution` (`:229`). El `catch` de `Run` sí lo mira (`:258-263`).
2. **El valor por defecto y el `catch` del host** (`Core:852`, `:858-862`): `NotAppliedFailure(UpdateNotRunReason …)`.
3. **`!closed.Accepted`** (`Core:870`): `NotAppliedFailure` aunque `closed.Kind` sea `Applied` o `CommitThrew`.

Las tres son hoy inalcanzables (`independent-review.md:74-75`, `:234`):
- `Execute` captura todo;
- `TryBeginApply` se acaba de tomar, así que `CompleteApply` no puede devolver `NotApplying` (`src/RackCad.Application/Workspace/Drafts/DraftSession.cs:536-537`; `DraftApplyOutcome.cs:153`).

- **[I] El análogo en T4C.** El adaptador de T4C añade una capa exterior más: la llamada de retorno, el `Dispose` del `DocumentLock` y la tarea de `ExecuteInCommandContextAsync`. Una excepción **después** de que `TryUpdatePilot` vuelva (por ejemplo, la de liberar el bloqueo) no puede reclasificar un `Applied` o un `CommitThrew` como fallo. Es la orden, «Do not translate UNKNOWN into ordinary Failure or Success» (orden:233).
- **¿T4C lo toca?** Sí, si toca `Run` o `Core` (I-01 y el cableado), y siempre en su propio adaptador.
- **Pruebas discriminantes:**
  - una guarda de fuente: el `catch` exterior de `Execute` consulta el estado del sello (por ejemplo, `Execution` creada fuera del `try`), y `Core:870` usa `closed.Kind`;
  - o la variante conservadora: el `catch` exterior da `CommitThrew`;
  - y en T4C, una prueba del adaptador con un doble del bloqueo cuyo `Dispose` lanza después de un `Applied`. La respuesta tiene que seguir siendo `Applied`, con un aviso o en el registro.

**H-25 [M]. M-03: `ClosedInError`.**
- **El código:** `Core:872-877` responde `ClosedInError(draft.ErrorReason)` y pierde la clasificación (`closed.Kind`), `LastApplyCause`, los avisos y las diferencias.
- **El tipo:** `EditUpdateOutcome.ClosedInError` no tiene dónde llevarlas (`WorkspaceEditIntent.cs:472`).
- **Su prueba:** `AnErrorThatArrivesDuringTheRunIsAppliedAfterTheClassificationAndTheAnswerIsClosedInError` (`TW/WorkspacePilotUpdateHostTests.cs:383-397`). Afirma que la respuesta es `ClosedInError` aunque el contenido ya esté rebasado.
- **La disposición:** M-03 se ratificó como interpretación, y «Que T4C muestre también la clasificación queda como candidato» (`task-close-package.md:119`).
- **[M] Lo que es alcanzable hoy.** `DraftSession.EnterError` (`DraftSession.cs:773-788`) **no tiene llamador de producto**: los `EnterError` de `src/RackCad.UI/Workspace/WorkspacePanelViewModel.cs` son del VM. Por eso el error diferido durante la corrida solo existe en las pruebas. El `ClosedInError` alcanzable sale de `Recover`, en la rama NotApplied* (`DraftSession.cs:646-653`), sin escritura.
- **[M] El bloque diferido** (`DraftSession.cs:555-559`) pone `UnknownOrigin = null`. Está congelado byte a byte (`TW/WorkspacePilotUpdateGuardTests.cs:797-818`).
- **[I] El riesgo.** Si T4C añade un llamador de `EnterError`, por ejemplo por un fallo del diagnóstico o de la superficie, un `CommitThrew` con un error diferido acabaría en `Error` **sin** `UnknownOrigin` y con la respuesta `ClosedInError`. El desconocido quedaría tapado.
- **El análogo:** `ClosedDetached()` (`Core:869`) también descarta `closed.Kind` (`DraftSession.cs:546`).
- **¿T4C lo toca?** Sí: la presentación del desenlace (R-T4-09, `T4P:568-571`) es de T4C.
- **Pruebas discriminantes:**
  - extender `:383-397` para que exija la clasificación `Applied` y sus avisos;
  - una variante con un Conflicto y un error diferido, que exija las diferencias;
  - una con `CommitThrew` y un error diferido, que exija que el desconocido sea visible;
  - lo mismo para `ClosedDetached` tras un `Applied` o un `CommitThrew` (`TW/WorkspacePilotUpdateHostTests.cs:367-381`).

**H-26 [M]. M-06: las pruebas débiles y `Closed` sin copia** (`task-close-package.md:132-136`; `independent-review.md:288-290`, `:313-317`).

| Prueba | Ancla | Debilidad | Lo que la haría discriminante [I] |
|---|---|---|---|
| `TheRunKeepsNoTextOfTheDrawingBetweenTwoRuns` | `TW/WorkspacePilotUpdateRunTests.cs:826-835` | Dos montajes independientes | Dos corridas sobre el **mismo** doble, con dibujos distintos, que exijan que la segunda no vea los textos de la primera |
| `ThePostComparisonSeesAnotherScaleInTheStateThatTheRereadMaterializes` | `:800-810` | Solo `NotNull`; la rama `PostScaleDiffersReason` (`Run:172-173`) nunca se ejerce | Exigir exactamente `PostScaleDiffersReason` con sobres iguales y otra escala. **Puede ser inalcanzable** si la escala sale siempre de los sobres (P-08) |
| `ThePostComparisonDoesNotNormalizeAnything` | `:753-777` | Solo la clase de resultado | Exigir que la causa empiece por `PostEnvelopeDiffersPrefix + "B2"` |
| `TheDesiredNamesFollowTheRuleOfTheClassicEditorPerView` | `:691-705` | Ni el caso compuesto ni el extremo posterior (`Run:205-206`) | Añadir un rack compuesto y un corte posterior, contra la regla del clásico |
| `TheAnswerOfAnUpdateAlwaysComesFromTheClose…` | `TW/WorkspacePilotUpdateHostTests.cs:399-414` | Solo reflexión | Renombrarla, o completarla con una fila por clase de desenlace |
| `TheFactoriesOfTheOutcomeCarry…CopiesOfTheLists` | `:435-458` | No comprueba copias; `Closed` guarda las referencias (`WorkspaceEditIntent.cs:477-483`) | Que `Closed` copie al recibir, y que la prueba mute la lista de origen después |

**¿T4C lo toca?** Sí, si cambia `WorkspaceEditIntent.cs` (M-03 o la operación del panel). Las pruebas de `Run` no tocan producto.

---

## 7. La revalidación final

**H-27 [M]. Lo que ya se revalida, y dónde.**

| Condición | Dónde y cuándo |
|---|---|
| La sesión viva y autorizada contra el documento activo | `UpdateGate`, en el momento de la llamada (`Core:925-929`) |
| El documento activo presente | `Core:912-917` |
| El modal de RackCad | `Core:919-923` |
| La identidad del documento de la escritura | La captura `UpdateGate` (`:931`) y la compara **una vez** el constructor del ámbito, con `MdiActiveDocument` (`Writer:71`). La comprobación de `Reread` es vacía con el escritor real (H-14) |
| El borrador presente | `Core:819-822` |
| `Precheck` | `Core:825-834` |
| La ranura: registrado, Clean o Dirty, sin texto pendiente y libre | `TryBeginApply` (`DraftSession.cs:510-528`) |
| La lectura fresca de PREPARE: el origen por texto exacto, la admisión, el plan (con el Conflicto contra la base del borrador), los supervivientes y las capas | `Run:285-324` |
| Ninguna `TopTransaction` ajena | `BeginMutation` (`Writer:140-141`) |
| **La relectura de MUTATE dentro de la transacción poseída:** el mismo origen, la StaleBase (Conflicto con cero escrituras), el destino (claves y bloques preparados), los supervivientes y las capas | `Run:373-400`; `Writer:240-248` |
| La propiedad de la transacción en cada operación | `EnsureOwned` (`Writer:307-313`) |
| `TopTransaction == null` antes de POST | `Writer:157-158` |
| **Ningún resultado de T4A2 autoriza** | `TryUpdatePilot` no nombra `ChosenReference`, `Witness`, `PilotRead`, `.StaleBase` ni el lector (`TW/WorkspaceEditIntentGuardTests.cs:1098-1111`). La corrida lee y planifica de nuevo bajo la ranura |

**Lo que no se revalida dentro de la corrida** [M]:
- el reposo: por diseño, va en la puerta de T4C antes del contexto de comando (`T4BP:557`, §9 I-11);
- `_pilotReadInProgress`;
- la profundidad de avisos;
- la sesión y el modal **después** de `UpdateGate`, mientras dura la corrida.

**[I] Lo que puede cambiar entre la admisión en el panel y la llamada de retorno.** Es una ventana asíncrona en la que **la ranura todavía no está tomada**:
1. **El documento activo** (el usuario cambia de pestaña): `UpdateGate` da `Rejected` por la sesión, o T4C rechaza antes de crear el escritor. Prueba 2 de la orden (orden:283).
2. **La sesión se destruye** (CLOSE, CLOSEALL): `RejectedSessionDestroyed`. Los borradores se retiran (`NoDraft`). [H] Si la llamada de retorno llega a correr.
3. **Se abre un modal de RackCad:** `Rejected`, con el motivo del modal (`:816` convierte `Waiting` en `Rejected`).
4. **Empieza otro comando:** la llamada de retorno se encola. [H] El orden y el momento. Dentro de la llamada de retorno, `IsActiveDocumentQuiescent` ya no sirve, porque el propio comando interno está activo.
5. **El borrador cambia:**
   - texto pendiente → `PendingText`;
   - un `CommitField` o un `CrossBoundary` aplica un estado nuevo: **la corrida aplicaría el estado del momento de la llamada de retorno, no el del clic**;
   - un descarte → `NoDraft`;
   - un Conflicto por `TryConfirmBase` → `NotApplicable` o `InvalidStatus`.
6. **Un segundo «Actualizar»:**
   - una segunda llamada de retorno encolada correría **otra corrida T4B** cuando acabe la primera, porque la ranura se libera en `CompleteApply`;
   - si la primera no aplicó, la segunda vuelve a intentarlo;
   - la ranura de `Applying` **no cubre** esta ventana.

   Lo piden las pruebas 18 y 9 de la orden (orden:290, :300) y «The existing Applying lock must prevent concurrency» (orden:186).
7. **El dibujo cambia** por otro comando, UNDO o RACKEDITAR: lo cubren la lectura de PREPARE y la relectura de MUTATE (Conflicto o admisión). Es vinculante de T4B.
8. **Queda abierta una `TopTransaction` ajena:** el rechazo de `BeginMutation`.
9. **El documento cambia de estado de bloqueo** (otro bloqueo, solo lectura, cierre en curso): [H].
10. **El reingreso durante la corrida:** un `Regen`, un `WblockCloneObjects` o un aviso que bombee mensajes podría dejar entrar un `DiscardRack`. `DiscardRack` no mira `Applying` (`Core:1156-1178`) y `DiscardByUser` tampoco (`src/RackCad.Application/Workspace/Drafts/DraftRegistry.cs:262-272`); está pendiente R-T4-07 (`T4P:560`). El resultado sería `ClosedDetached` sin clasificación (H-25). [H]

---

## 8. Lo que solo el host puede probar

- Qué documento es `MdiActiveDocument` dentro de la llamada de retorno de `ExecuteInCommandContextAsync`, y si la llamada de retorno corre antes de que vuelva la llamada (R-T4-05, `T4P:550`).
- Los `LockDocument` anidados de `CaptureDrawing` y `SyncName` bajo el bloqueo de T4C, y la importación y la purga sin bloqueo propio.
- Si la importación, la transacción poseída, la purga, los `SyncName` y el `Regen` forman un solo paso de Undo.
- Si una llamada de retorno se encola o se descarta con un comando activo, un modal o un CLOSE en curso.
- El reingreso durante el `Regen` y `WblockCloneObjects`.
- La conservación de los bytes y los handles de POST (Smoke-2, `T4BP:797`).

---

## 9. Riesgos

- **R-01.** Las guardas se encadenan (H-10, H-12):
  - el adaptador tiene que estar fuera de `Plugin/Workspace` (`async`, `LockDocument`, 9 archivos);
  - fuera de Workspace no puede nombrar el núcleo ni el controlador (`TW/WorkspaceHostSourceGuardTests.cs:155-166`);
  - y `ExecuteInCommandContextAsync` está prohibido en todo `src/` (`TW/WorkspaceNavigationHostTests.cs:436`).

  Cualquier diseño necesita al menos un cambio de guarda declarado.
- **R-02.** Una operación 16 en `IWorkspacePilotEditPort` obliga a cambiar seis implementadores, el doble de UI incluido. Además choca con `TW/WorkspaceEditIntentHostTests.cs:1012`, que prohíbe «Update» en sus nombres.
- **R-03.** B-2 (b) literal retiene el escritor todo el proceso, contra la orden (orden:163-164).
- **R-04.** Un escritor de un solo uso, o uno con su `Document`, necesita un campo en la clase del puerto, que prohíbe una guarda de T4B con su negativa literal (H-15).
- **R-05.** La capa exterior de T4C (la llamada de retorno, el bloqueo y la tarea) puede reclasificar un desenlace ya decidido (M-01 en T4C).
- **R-06.** Un doble clic o llamadas de retorno encoladas pueden dar dos corridas T4B, porque la ranura no cubre la ventana asíncrona (§7).
- **R-07.** La guarda «nada desde el drenaje» no incluye `TryUpdatePilot` en el drenaje del controlador (`TW/WorkspaceEditIntentGuardTests.cs:617`).
- **R-08.** Hay varias cargas del catálogo por corrida, y una carga fallida da un catálogo **vacío**, no nulo (H-18, H-19).
- **R-09.** Las escrituras de POST (la purga y `SyncName`) y las de la importación van en transacciones aparte. La agrupación de Undo depende del contexto de comando [H].
- **R-10.** Con el escritor real, la única comprobación de documento es la del constructor del ámbito (H-14). Si T4C cambia cómo se obtiene el documento, esa comprobación tiene que mantenerse.
- **R-11.** `ClosedInError` y `ClosedDetached` ocultan la clasificación, también un `CommitThrew`. Un llamador nuevo de `EnterError` lo haría alcanzable (H-25).
- **R-12.** Si I-01 se mueve sin extender la guarda a `UpdateGate`, un literal nuevo del host pasaría sin verse (H-23).
- **R-13.** `TryUpdatePilot` responde `Rejected` también a las causas de espera, sin documento o con un modal (`Core:816`). La UI no puede distinguir «espera» de «rechazo» sin mirar el texto.

---

## 10. Preguntas abiertas

- **P-01.** ¿Dónde vive la operación del panel? Puede ser una operación 16 del puerto con la intercepción en el Plugin (W-3), un puerto aparte (W-4) o un campo del núcleo (W-1 o W-2). ¿Y qué devuelve **de forma síncrona**, si el desenlace de T4B llega después de la llamada de retorno: un valor provisional y el desenlace por aviso, o por cola?
- **P-02.** «created after host admission»: ¿se refiere a la instancia del puerto o al ámbito? Hoy el ámbito ya nace después de `UpdateGate` y de la ranura.
- **P-03.** ¿Puede el adaptador de T4C repetir en el Plugin las comprobaciones de `UpdateGate` (el documento, la sesión y el modal) antes de crear el escritor? ¿O conviene que `TryUpdatePilot` reciba una fábrica, lo que cambia una firma y una guarda de T4B?
- **P-04.** ¿Pasa la operación por la profundidad de avisos (`WorkspacePilotEditGate`), como las lecturas?
- **P-05.** ¿Responden `Busy` `SetPendingText`, `CommitField`, `CrossBoundary` y `Discard*` entre el clic y la llamada de retorno? ¿Se fija qué estado del borrador aplica la corrida: el del clic o el de la llamada de retorno?
- **P-06.** El nombre del bloque (`SyncName`), ¿es authored o cosmético, para la regla de una sola transacción authored y para Undo?
- **P-07.** ¿Es «reopen T4B» cambiar `Writer` o una guarda de T4B (D-1, D-2, R-04)?
- **P-08.** M-06, «la escala sola»: ¿es alcanzable la rama `PostScaleDiffersReason` (`Run:172-173`) con sobres byte a byte iguales? Si no, ¿la prueba se reformula o la rama se declara inalcanzable?
- **P-09.** ¿Entra `ClosedDetached` (`Core:869`) en el alcance de M-03?
- **P-10.** M-01: ¿se endurece con el estado del sello visible fuera de `Run`, o con un `CommitThrew` conservador? ¿Y `Core:870` con `closed.Kind`?

---

## 11. Posibles condiciones de devolución (orden:342-352)

- **«the pilot-specific writer needs a new persistent host authority».** Las vías W-1 y W-2 (H-11 y §2) dejan en el núcleo, que vive todo el proceso, un campo con el escritor o con su fábrica. Si el paquete no encuentra un cableado sin campo persistente (W-3 o W-4, con sus propios cambios de guarda), esta condición se activa.
- **«more than one authored mutation transaction is required».** Lo authored del rack va en una sola transacción, pero una corrida modifica el DWG en varias (H-21): la importación, la poseída, la purga y un `SyncName` por clave. Si el nombre del bloque se considera authored (P-06), se activa. Es el diseño aceptado de T4B (D-T4B-03), no algo que añada T4C.
- **«any other material product decision».** Hay tres:
  - (a) cambiar `Writer` o las guardas de T4B para el documento exacto o el escritor de un solo uso (H-15, R-04), frente a «Do not reopen T4B» (orden:49);
  - (b) la excepción de `ExecuteInCommandContextAsync` sobre la guarda de todo el producto (`TW/WorkspaceNavigationHostTests.cs:436`), que C-F4-02 manda **avisar** al Owner;
  - (c) M-03 con un llamador nuevo de `EnterError`: corregirlo obliga a tocar el bloque diferido congelado byte a byte (`DraftSession.cs:555-559`).
- **«DocumentLock/command context cannot wrap the T4B run cleanly»:** no lo veo por el código. `TryUpdatePilot` no toma bloqueo ni transacción, y el escritor tampoco toma el bloqueo. Que los bloqueos anidados y la agrupación de Undo funcionen solo lo puede decir el host (§8).

---

**Recuento:** 27 hechos (H-01 a H-27), 13 riesgos (R-01 a R-13) y 10 preguntas (P-01 a P-10).
