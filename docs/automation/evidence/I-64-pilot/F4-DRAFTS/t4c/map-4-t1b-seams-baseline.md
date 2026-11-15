# MAPA 4 de F4-T4C — Superficies clásicas caracterizadas por T1B que T4C podría tocar, y línea base de pruebas y de alcance

**Base medida:** HEAD `b15072ff` (custodia del cierre de T4B; solo `docs/`), producto de `4314c180` (GREEN de T4B).
- `git diff --name-only 1d36d006 b15072ff -- src` (del GREEN de T1B al HEAD) da exactamente 10 archivos, los de T4B: `IPushBackPilotUpdatePort.cs`, `PushBackPilotSystemRebuild.cs`, `PushBackPilotUpdatePlan.cs`, `PushBackPilotUpdateRun.cs`, `DraftApplyOutcome.cs`, `DraftSession.cs`, `WorkspaceEditIntent.cs`, `WorkspaceHostCore.cs`, `Views/RackPilotUpdateWriter.cs` y `Views/RackSiblingScan.cs`. `1d36d006` es ancestro de `b15072ff`.

**Convención:**
- **[M]** medido, con su ancla `ruta:línea` en `b15072ff`;
- **[I]** inferencia razonada;
- **[H]** solo el host (AutoCAD real) puede probarlo.

**Método.** Solo lectura: `git show`, `git diff`, `git grep`, `git ls-files`, lectura de archivos y dos scripts de Python de solo lectura sobre los TRX: `_map4_trx_counts.py` y `_map4_trx_perclass.py`, en la carpeta de trabajo de la sesión, fuera del repositorio. Sin compilar, sin ejecutar pruebas y sin AutoCAD.

---

## 0. Resumen

- **[M]** Ninguna guarda de T1B lee ni ejecuta los archivos que T4C tocaría con más probabilidad: el adaptador nuevo de `Plugin/Views`, `WorkspaceController.cs`, los implementadores de `IWorkspacePilotEditPort`, `src/RackCad.UI/Workspace/**`, `WorkspaceHostCore.cs`, `PushBackPilotUpdateRun.cs`, `WorkspaceEditIntent.cs` ni `RackPilotUpdateWriter.cs`.
  - **La única guarda de T1B que alcanza a todo `src/`** es la ausencia de `InsertRequestedRaised +=` (CT-19).
- **[M] El punto de presión real es CT-27 (1).** Fija las firmas `internal` exactas de `PrepareRedraw` y `RedrawInTransaction` de las tres fachadas, que el escritor de T4B consume.
  - Cualquier cambio de esas firmas, por ejemplo para pasarle el catálogo de la corrida, es un **cambio de comportamiento caracterizado por T1B** (§9, DV-01).
- **[M] T4C sí rompe guardas que no son de T1B,** y las tiene que cambiar de forma declarada. La principal es la prohibición de `ExecuteInCommandContextAsync` en todo `src/` (`tests/RackCad.Tests/Workspace/WorkspaceNavigationHostTests.cs:436`). Las demás están en §4.4.
- **[M] La plantilla del prohibido de T4B** (70 entradas) necesita cambiar en **5 entradas** para T4C: `src/RackCad.UI/`, `src/RackCad.Plugin/Workspace/`, `IWorkspacePilotEditPort.cs`, `WorkspacePilotSupport.cs` y `tests/RackCad.UI.Tests/`. Las otras 65 pueden quedarse.
  - Al partir las raíces, la clausura de T1B y sus clases de prueba de UI dejan de quedar cubiertas por una raíz (§3).
- **[M] Línea base de `4314c180`:**
  - Core 18684/18684, con `RackCad.Tests.Workspace` = 3103;
  - UI 2310 resultados, 2293 en verde y 17 omitidas, con `RackCad.UI.Tests.Workspace` = 601;
  - T1B: UI 55 y Core 10.
- **[M] Tensión de proceso para Smoke-2:**
  - la guía y el Freeze exigen «UI Full local» en el Candidato de un hito (`docs/guias/validacion-manual-autocad.md:424-425`; `docs/initiatives/I-64-proposal-v5.md:828-829`);
  - F4-E59 prohíbe las suites completas de UI en local en las tareas con contención de perfil (`docs/automation/evidence/I-64-pilot/F4-DRAFTS/f4-dispositions.md:2082`).

---

## 1. La clausura de T1B

### 1.1 Estado de cierre

- **[M]** **CLOSED / PASS WITH LIMITATIONS** sobre `1d36d006`, con CI `43650040527` en 4/4 y `git diff 0a6f84c2 1d36d006 -- src` vacío (`docs/automation/evidence/I-64-pilot/F4-T1B-PILOT-CHARACTERIZATION/task-close-package.md:13-22`).
- **[M]** La caracterización es la **línea base de compatibilidad para T4B y T4C, sin respaldar sus rarezas** (OD-1..OD-13) (`…/task-close-package.md:9`; `f4-dispositions.md:2089`).
- **[M]** La orden del Owner para T4C (§«T1B COMPATIBILITY») manda: si T4C cambia un comportamiento heredado caracterizado por T1B, «STOP and return to Owner», sin actualizar en silencio las pruebas de caracterización.

### 1.2 Los 14 archivos de MutantScope

**[M]** La lista está en el contrato (`docs/automation/evidence/I-64-pilot/F4-T1B-PILOT-CHARACTERIZATION/coordinator/gate-contract.json:159-172`, dentro de `AllowedWriteScope` `:151-173`) y en la tabla del paquete (`docs/initiatives/I-64-f4-t1b-gate-contract-package.md:267-275`).

| # | Archivo | Clase de mutante permitida |
|---|---|---|
| 1 | `src/RackCad.UI/Systems/PushBack/RackPushBackSystemWindow.xaml` | de comportamiento |
| 2 | `src/RackCad.UI/Systems/PushBack/RackPushBackSystemWindow.xaml.cs` | de comportamiento |
| 3 | `src/RackCad.UI/Views/RackViewBatchDialog.xaml` | de comportamiento (`IsDefault`, `IsCancel`, ubicación) |
| 4 | `src/RackCad.UI/Views/RackViewBatchDialogPresenter.cs` | solo textual |
| 5 | `src/RackCad.UI/UiSupport.cs` | solo textual |
| 6 | `src/RackCad.UI/Editor/EditorModules.cs` | solo textual |
| 7 | `src/RackCad.UI/RackFrames/RackFrameConfiguratorWindow.xaml.cs` | solo textual |
| 8 | `src/RackCad.Plugin/RackPushBackCommands.cs` | solo textual |
| 9 | `src/RackCad.Plugin/RackMenuCommands.cs` | solo textual |
| 10 | `src/RackCad.Plugin/Views/RackViewBatchProducts.cs` | solo textual |
| 11 | `src/RackCad.Plugin/Views/RackViewPlacement.cs` | solo textual |
| 12 | `src/RackCad.Plugin/Systems/PushBack/PushBackSystemDrawService.cs` | solo textual |
| 13 | `src/RackCad.Plugin/Systems/PushBack/PushBackFrontalDrawService.cs` | solo textual |
| 14 | `src/RackCad.Plugin/Systems/PushBack/PushBackPlantaDrawService.cs` | solo textual |

- **[M]** `git diff --stat 1d36d006 b15072ff` sobre los 14 archivos sale vacío. T4B no tocó ninguno.
- **[M]** También salen vacíos, sobre el mismo rango, los demás archivos de producto que leen las guardas de T1B: `KindHandlers/PushBackKindHandler.cs`, `RackFrames/RackFrameConfiguratorWindow.xaml` y todo `src/RackCad.UI/Systems/PushBack/`. Lo mismo vale para las siete clases de prueba.

### 1.3 CT-01 a CT-28 (resumen) y CT-24 a CT-27 (detalle)

**[M]** La tabla completa está en `docs/initiatives/I-64-f4-t1b-gate-contract-package.md:390-419`.
- CT-18, que era opcional, no se escribió (`…/task-close-package.md:72`).
- La adenda de anclas del Owner está en `…/task-close-package.md:85-124`.

| Grupo | CT | Tipo | Dónde se fija |
|---|---|---|---|
| Teclado, foco y tabulación | CT-01, CT-10, CT-11 | Ejecución | UI, `KeyboardTests` |
| Cierre | CT-02 a CT-06 y CT-28 | Ejecución | UI, `CloseTests` |
| Intención y guardas que no cierran | CT-07 a CT-09, CT-17 y CT-23 | Ejecución (CT-23 además por fuente) | UI, `IntentTests` y `SourceGuardTests` |
| Dueños y ubicación | CT-12 a CT-14 (ejecución); CT-16, CT-20 y CT-21 (fuente) | Mixto | UI, `OwnerTests` y `SourceGuardTests` |
| Recursos | CT-15 | Ejecución | UI, `ResourceTests` |
| Ausencias y escrituras de la ventana (clase A) | CT-19 y CT-22 | Fuente | UI, `SourceGuardTests` |
| **Escrituras del Plugin tras el cierre (clase C)** | **CT-24 a CT-27** | **Fuente** | **Core, `PluginGuardTests`** |

**CT-24 a CT-27, lo que congelan** (`tests/RackCad.Tests/PushBackClassicCharacterizationPluginGuardTests.cs`). Todas son guardas de texto que normalizan CRLF, quitan comentarios y colapsan espacios (`:139-145`).

- **CT-24, RACKPUSHBACK** (`:195-230`). El cuerpo de `public void RackPushBack()` en `RackPushBackCommands.cs`:
  - contiene `new RackPushBackSystemWindow(canInsertInAutoCad: true);`, `AcApplication.ShowModalWindow(window);`, `window.InsertionRequest is PushBackInsertionRequest request`, `request.View,` y `request.Section,`;
  - **no** contiene `Owner =` ni `Views`;
  - la llamada `DrawPushBackView(request.View, …, innerSource: request.SourceProject);` sin cualificar, tras `RackUnitsGuard.WarnIfNotInches(document);`;
  - en `DrawPushBackView`, `Resolve` va antes de `system.Name = rackName;`.
- **CT-25, RACKEDITAR** (`:234-270`). El cuerpo de `internal static void EditPushBack(` fija:
  - lo que lee de la ventana: `LoadExisting`, `InsertRequested`, `DesignToInsert`, `SystemToInsert`, `RackId` y `RackName` como respaldo, `UpdateOnly` e `InsertionRequest.Views`;
  - el orden `PrepareAll` < `.RedrawInPlace(` < `RackBlockRenamer.SyncName(` < `RackCommandSupport.EraseViewBlocks(` < `document.Editor.Regen();` < `RackViewBatchExecution.Run(`;
  - exactamente 3 `regen: false` y 1 `Editor.Regen();`.
  - La debilidad de `InOrder`, que busca la primera aparición posterior, es M-02 de la revisión de T1B (`…/task-close-package.md:141`).
- **CT-26, el menú** (`:274-311`):
  - el `case PushBackInsertionRequest pushBack:` de `RackMenuCommands.cs` va en el orden `RackNewRackName.Resolve(` < `pushBack.System.Name = pushBackName;` < `RackViewBatchProducts.PushBack(…)` < `RackViewBatchExecution.Run(document, pushBackProducts,`;
  - `PlacePushBack(document, product, regen: false)` en `Views/RackViewBatchProducts.cs`;
  - el reenvío de `PlacePushBack` a `PlaceHeader(…, RackEmbedDocument.KindPushBack, regen: regen)` en `Views/RackViewPlacement.cs`.
- **CT-27 (1), las tres fachadas** (`:315-363`, `Theory` con 3 filas). Fija, en `Systems/PushBack/PushBack{System,Frontal,Planta}DrawService.cs`, las firmas exactas de:
  - `public … DrawAndPlace(…)`;
  - `public … RedrawInPlace(…)`;
  - `internal PreparedViewRedraw PrepareRedraw(Database …, ObjectId …, PushBackSystem …, string payloadJson, …)`;
  - `internal LateralHeaderDrawOutcome RedrawInTransaction(Database database, Transaction transaction, PreparedViewRedraw prepared, out IReadOnlyCollection<ObjectId> staleDefinitions)`.
  - Los literales están en `:322-347`.
- **CT-27 (2), los caminos clásicos** (`:365-389`):
  - `PrepareRedraw(` y `RedrawInTransaction(` no aparecen en `RackPushBackCommands.cs`, `KindHandlers/PushBackKindHandler.cs`, `RackMenuCommands.cs` ni `Views/RackViewBatchProducts.cs`;
  - ancla: `=> RackPushBackCommands.EditPushBack(document, blockId, embed);` en el handler.
  - «Que no tengan llamador en producción **no** se congela» (`:368`).
- **[M] Precisión de anclas.** El paquete de T4B cita CT-27 (1) en `:322-354` y CT-27 (2) en `:366-382` (`docs/initiatives/I-64-f4-t4b-gate-contract-package.md:162`, `:168`). Medido: la `TheoryData` está en `:315-350`, la `Theory` en `:352-363` y el método de CT-27 (2) en `:365-389`. El cierre de T1B cita `:322-347` y `:380-381` (`…/task-close-package.md:102`).

### 1.4 Las siete clases: qué fija cada una y sobre qué archivos

**[M]** Las cuentas por clase salen del TRX de UI y del de Core de la CI `43668817337`, sobre `4314c180`.

| Clase (archivo bajo `tests/`) | Resultados | CT | Qué toca de producto | Modo |
|---|---|---|---|---|
| `RackCad.UI.Tests/PushBackClassicCharacterizationKeyboardTests.cs` | 4 | CT-01, CT-10, CT-11 | Instancia `RackPushBackSystemWindow(canInsertInAutoCad: true)` y la muestra fuera de pantalla (`:35`, `:130`, `:149`, `:222`) | Ejecución, colección `EditorDiscardPromptCollection` (`:27`) |
| `…/PushBackClassicCharacterizationCloseTests.cs` | 21 | CT-02 a CT-06, CT-28 | La ventana, con el aviso de descarte sustituido; CT-04 hace un `ShowDialog` real con respaldo (`:403`, `:473-510`) | Ejecución, misma colección (`:38`) |
| `…/PushBackClassicCharacterizationIntentTests.cs` | 12 | CT-07 a CT-09, CT-17, CT-23 | La ventana con `newIdFactory`; CT-23 sin mostrar (`:389`) | Ejecución, misma colección (`:31`) |
| `…/PushBackClassicCharacterizationOwnerTests.cs` | 4 | CT-12 a CT-14 | La ventana; S-3 y S-4 por su costura; `RackViewBatchDialog` construido con su presentador, sin `ShowDialog` (`:163-179`) | Ejecución, misma colección (`:27`) |
| `…/PushBackClassicCharacterizationResourceTests.cs` | 2 | CT-15 | La ventana, con la plantilla aplicada y sin mostrar | Ejecución, sin colección |
| `…/PushBackClassicCharacterizationSourceGuardTests.cs` | 12 | CT-16, CT-19 a CT-22, CT-23 (fuente) | Lee **todo** `src/RackCad.UI/Systems/PushBack/**` (`*.cs` y `*.xaml`, `:169-176`), `RackFrames/RackFrameConfiguratorWindow.xaml` y `.xaml.cs`, `Views/RackViewBatchDialogPresenter.cs`, `UiSupport.cs` y `Editor/EditorModules.cs`. **CT-19 lee todo `src/**/*.cs`** buscando `InsertRequestedRaised\s*\+=` (`:250-264`) | Fuente |
| `RackCad.Tests/PushBackClassicCharacterizationPluginGuardTests.cs` | 10 | CT-24 a CT-27 | Lee `RackPushBackCommands.cs`, `RackMenuCommands.cs`, `KindHandlers/PushBackKindHandler.cs`, `Views/RackViewBatchProducts.cs`, `Views/RackViewPlacement.cs` y las tres `Systems/PushBack/*DrawService.cs` | Fuente |

- **[M]** Suman 55 de UI y 10 de Core.
- **[M] Contención de T1B:** ninguna prueba instancia `RackFrameConfiguratorWindow` ni ejerce `PromptSaveToLibrary`, el BOM, `RackProjectStore.Save`, `SaveLibrary_Click` ni `InsertBatch_Click` con entradas válidas (`…/task-close-package.md:82`).

---

## 2. Lo que T4C tocaría con más probabilidad, frente a T1B

**[I]** La lista de superficies se razona desde:
- el §8 del paquete de T4B (`docs/initiatives/I-64-f4-t4b-gate-contract-package.md:811-819`);
- el reparto del §3 (`:244-255`);
- R-T4-05 a R-T4-10 (`docs/initiatives/I-64-f4-t4-gate-contract-package.md:547-573`);
- la orden del Owner (§«PRIMARY OBJECTIVE», «EXECUTE IN COMMAND CONTEXT», «RUN-SCOPED WRITER» y «T4B I-01» a «T4A2 DIAGNOSTIC»).

### 2.1 Tabla por superficie

| Superficie | ¿La fija T1B? | ¿Una guarda de T1B se rompería con un cambio razonable? | Lo que sí la fija (fuera de T1B) |
|---|---|---|---|
| **Adaptador nuevo en `src/RackCad.Plugin/Views/`** con `ExecuteInCommandContextAsync` y `LockDocument` (R-T4-05: «solo en el archivo exacto del adaptador, nuevo en `Plugin/Views`», `T4P:548`) | **[M] No.** Ninguna guarda de T1B lee un archivo nuevo de `Plugin/Views` | **[M] Solo CT-19**, que lee todo `src/**/*.cs` (`SourceGuardTests.cs:254`): rompería si el adaptador suscribiera `InsertRequestedRaised +=`. **[I]** Un adaptador razonable no lo hace. CT-27 (2) no lee archivos nuevos | **[M]** `WorkspaceNavigationHostTests.cs:436`: ningún archivo de `src` nombra `ExecuteInCommandContextAsync`. **Rompe con certeza** (§4.4). Además, las trampas de censo de todo el Plugin: `[CommandMethod(` = 40 (`SelectiveEditorOpenTests.cs:565-572`), `Insunits` (`RackUnitsGuardSourceTests.cs:87-101`) y la composición de `RackEmbedDocument` (`CustomPropertiesEnvelopeGuardTests`) |
| **`WorkspaceController.cs`** (cableado de la instancia del puerto de actualización, T4B §8 `:815`) | **[M] No** | **[M] No** | **[M]** Varias guardas de Workspace:<br>• el adaptador anidado `PilotEditAdapter` solo reenvía `=>_gate.X(…);` por operación, y `_pilotEdit = new PilotEditAdapter(new WorkspacePilotEditGate(_host, _noticeDepth));` es literal (`WorkspacePilotEditGuardTests.cs:482-529`);<br>• exactamente 3 `+=`, 0 `-=`, 1 `event`, y sin `Task` ni `async` (`:525-529`);<br>• sin `ExecuteInCommandContextAsync`, `async`, `await`, `Task`, `Dispatcher` ni `Timer` (`WorkspacePilotPrepareGuardTests.cs:408-431`);<br>• sin `.Wait(`, `.Result`, `GetAwaiter` ni `Dispatcher` (`WorkspaceNavigationHostTests.cs:439-449`; `WorkspaceHostSourceGuardTests.cs:424-443`) |
| **Implementadores de `IWorkspacePilotEditPort`** | **[M] No** | **[M] No** | **[M]** Las clases que declaran la interfaz son **6, no 5**: `WorkspaceHostCore` (`src/RackCad.Application/Workspace/WorkspaceHostCore.cs:38`), la compuerta `WorkspacePilotEditGate` (`WorkspacePilotSupport.cs:153`), `PilotEditAdapter` (`src/RackCad.Plugin/Workspace/WorkspaceController.cs:278`), dos dobles de Core (`tests/RackCad.Tests/Workspace/WorkspaceCommandAdvisorTests.cs:17`, `:1107`) y el doble de UI (`tests/RackCad.UI.Tests/Workspace/WorkspacePilotEditPortDouble.cs:25`). H-07 de T4B dice «cinco» y deja fuera al núcleo, que implementa la interfaz de forma explícita (`T4BP:114-120`) |
| **VM y vista del panel** (`src/RackCad.UI/Workspace/**`; R-T4-09, M-03, «Aplicado con avisos» y Applying de T4A2 §8) | **[M] No.** `ClassicWindowSources` solo lee `Systems/PushBack/**` (`SourceGuardTests.cs:169-176`) | **[M] Solo CT-19**, por la misma razón que el adaptador. **[I]** Romperían si T4C pusiera un archivo nuevo bajo `src/RackCad.UI/Systems/PushBack/`, porque entraría en las ausencias de CT-19 (`KeyDown`, `.Focus(`, `DialogResult`…, `:240-247`) | **[M]** Las guardas de UI del panel:<br>• sin `Task`, `async`, `await`, `Dispatcher` ni `Timer`, salvo `WorkspaceNavigationViewModel.cs` con cuentas exactas (`WorkspacePaletteHostSourceGuardTests.cs:555-583`);<br>• `LimitViolations` (`WorkspacePilotSurfaceGuardTests.cs:278-292`);<br>• sin `+=`, `-=`, `event`, `delegate`, `Task`, `async`, `Keyboard` ni `.Focus(` en los tres VM (`WorkspaceHostHardeningGuardTests.cs:757-779`);<br>• quién llama a `TryCheckNow` y compañía (`WorkspacePilotReconciliationGuardTests`, D-T4A2-07);<br>• `DrawingReach`, `DialogReach` y `FocusReach` (`WorkspacePilotCloseSyncGuardTests.cs:851-870`) |
| **`WorkspaceHostCore.cs`** (`TryUpdatePilot` `:805-905`, `UpdateGate` `:908-933`) | **[M] No** | **[M] No** | **[M]** Las guardas del host (H-06 de T4B):<br>• sin `Transaction`, `\bDatabase\b` ni `\bCommit\s*\(` ni en comentarios;<br>• ningún campo de `Systems`;<br>• el constructor no cambia;<br>• la tabla de 30 literales;<br>• `PortMutatingOperations` con `Skip(5)` (`WorkspaceLifecycleGuardTests.cs:463-534`, según T4BP `:599`) |
| **`PushBackPilotUpdateRun.cs`** (I-01, M-01) | **[M] No** | **[M] No** | **[M]** La pureza de la corrida (`WorkspacePilotUpdateGuardTests`, D-T4B-09) y su prueba de clase (`WorkspacePilotUpdateRunTests`, 55) |
| **`EditUpdateOutcome`** (`WorkspaceEditIntent.cs`; M-06, DR-m3) | **[M] No** | **[M] No** | **[M]** El orden y los nombres de `EditUpdateStatus` (D-T4B-07, `T4BP:577-596`) y el censo de portadores (`WorkspaceEditIntentHostTests.cs:982`, según `T4BP:550`) |
| **`RackPilotUpdateWriter.cs`** | **[M] No lo lee**, pero **consume** las firmas que fija CT-27 (1): `PrepareRedraw` (`src/RackCad.Plugin/Views/RackPilotUpdateWriter.cs:111`, `:114`, `:117-123`) y `RedrawInTransaction` (`:266`, `:269`, `:272`) | **[M] Sí, si T4C cambiara la firma de una fachada:** CT-27 (1) y `CallerOwnedFacadeGuardTests`, que T4B extendió a las tres de Push Back (`tests/RackCad.Tests/CallerOwnedFacadeGuardTests.cs:58-60`) | **[M]** `WorkspacePilotUpdateGuardTests`:<br>• la clase del puerto **no puede nombrar** `Document`, `Transaction`, `ObjectId` ni `Database`, **ni tener ningún campo** (`:185-188`);<br>• sin `LockDocument`, `async`, `await` ni `ExecuteInCommandContextAsync` (`:171`) |

### 2.2 Hechos del camino de T4C que tocan lo clásico

- **[M] El escritor elige el documento él mismo:** `Begin` usa `AcApplication.DocumentManager.MdiActiveDocument` (`RackPilotUpdateWriter.cs:37`), y el ámbito compara su `UnmanagedObject` con `AuthorizedDocumentNativeId` (`:68-72`).
  - **[I]** La orden del Owner pide que el escritor «receives only the exact current document/context». Si T4C quiere pasarle el `Document`, choca con la guarda de T4B, que prohíbe que la clase del puerto nombre `Document` o tenga campos (`WorkspacePilotUpdateGuardTests.cs:185-188`). Eso es un cambio declarado de una guarda de T4B; no es de T1B.
- **[M] Dos catálogos por corrida:**
  - `ReadForPrepare` carga `LateralHeaderDrawService.LoadCatalog()` (`RackPilotUpdateWriter.cs:81`);
  - cada `PrepareRedraw` de fachada carga el suyo con `RackCatalogLoader.Load()` (`src/RackCad.Plugin/Systems/Shared/ViewBlockDraw.cs:83`).
  - T4B lo declaró (`T4BP:395`, IR M-4).
  - **[I]** La orden asigna a T4C «catalog/read dependencies required by the run». Unificar el catálogo pasándolo a `PrepareRedraw` cambia una firma `internal` que fija CT-27 (1) (`PluginGuardTests.cs:324`, `:335`, `:346`). Ese es DV-01 del §9.
- **[M] `Post` regenera con `SystemBlockWriter.ApplyRegen(document, true)`** (`RackPilotUpdateWriter.cs:185`) y purga con `SystemBlockWriter.PurgeAfterCommit` (`:162`).
  - `SystemBlockWriter.cs` vive en `src/RackCad.Plugin/Systems/Shared/`, que está en el prohibido de T4B (raíz `src/RackCad.Plugin/Systems/`).
  - No lo lee T1B.
- **[M] La puerta de Actualizar rechaza con un modal de RackCad abierto** (`WorkspaceHostCore.cs:919-923`).
  - El indicador solo lo mueven los dueños fijados por `ModalOwners` (`WorkspacePilotEditGuardTests.cs:559-600`).
  - RACKPUSHBACK y RACKEDITAR abren la ventana clásica con `ShowModalWindow` dentro de su propio comando (CT-24, `PluginGuardTests.cs:201`), sin tocar ese indicador.
  - **[I]** La puerta de reposo, que T4B pasa a la operación del puerto de T4C (`T4BP:557`), cubre ese caso porque hay un comando en curso (`WorkspaceController.cs:367-373`). No hace falta tocar `RackPushBackCommands.cs`.
  - **[H]** Que la paleta no sea operable mientras dura `ShowModalWindow` de la ventana clásica lo cubren S1-07 y S2-13 (`I-64-proposal-v5.md:855`, `:893`).
- **[M] El panel nunca abre la ventana clásica.** C-F4-07 solo muestra el diagnóstico y la indicación de usar RACKEDITAR (`f4-dispositions.md:101`), y una guarda prohíbe «Abrir en editor» o «editor clásico» en la XAML del panel (`WorkspaceBrowserHostGuardTests.cs:257-263`).
  - **[I]** CT-01 a CT-23 y CT-28 (la ventana) no están en el camino de T4C.
- **[M] RACKEDITAR está en el camino de Smoke-2, no en el de T4C:**
  - es la «modificación externa» de S2-04 y de S2-R-02 a S2-R-04 y S2-R-12 (`I-64-proposal-v5.md:884`; `docs/initiatives/I-64-f4-t4a2-gate-contract-package.md:640-651`);
  - S2-07 exige «RACKEDITAR de Push Back sigue igual que antes» (`I-64-proposal-v5.md:887`).
  - **[H]** Si el ámbito de mutación propia de T4C enmascarara las pistas de un RACKEDITAR posterior, sería un defecto de T4C (R-T4-08, «no oculta otro rack»), no un cambio de T1B.
- **[M] Lo que T4B prohibió al escritor y T4C hereda como frontera:** el escritor nunca toma `LockDocument`; el único `using LockDocument` es del adaptador de T4C (`T4BP:389`, R-T4-06 `T4P:552`).

### 2.3 Conclusión del §2

- **[I]** Con un diseño que deje las fachadas, `RackPushBackCommands.cs`, `KindHandlers/`, `RackMenuCommands.cs`, `Views/RackViewBatchProducts.cs`, `Views/RackViewPlacement.cs` y `src/RackCad.UI/Systems/PushBack/**` sin tocar, **ninguna guarda de T1B se rompe** y no se cambia ningún comportamiento caracterizado.
- **[I]** Los únicos caminos hacia «T1B behavior must change» son tres:
  - cambiar una firma de fachada, por ejemplo por el catálogo (DV-01);
  - registrar un comando en un `*Commands*.cs` por Undo (DV-02);
  - poner código del panel bajo `Systems/PushBack/` de la UI.

---

## 3. El alcance prohibido de T4B como plantilla

### 3.1 Composición medida

**[M]** `docs/automation/evidence/I-64-pilot/F4-T4B-MUTATE-POST/coordinator/gate-contract.json`:
- `AllowedWriteScope` (`:226-250`): 23 rutas exactas;
- `ForbiddenWriteScope` (`:251-321`): 70 entradas, de modo que la entrada n está en la línea 251 + n.

| Tipo | Entradas | Nº |
|---|---|---|
| Raíces de `src` | F01 `src/RackCad.UI/`, F02 `src/RackCad.Domain/`, F03 `src/RackCad.Plugin/Workspace/`, F04 `src/RackCad.Plugin/Systems/`, F05 `src/RackCad.Plugin/Drawing/`, F06 `src/RackCad.Plugin/KindHandlers/`, F07 `src/RackCad.Application/Views/Redraw/` | 7 |
| Los 18 `*Commands*.cs` | F08 a F25. **[M]** `git ls-files 'src/*Commands*.cs'` da exactamente esos 18; ninguno vive en `Plugin/Workspace` | 18 |
| Archivos exactos del Plugin | F26 `RackSelectivoInsertIntegration.cs`, F27 `RackCommandSupport.cs`, F28 `Views/RackPilotIntentAdapter.cs`, F29 `Views/RackUnsupportedSiblingInsert.cs`, F30 `Views/RackProjectionSnapshotReader.cs`, F31 `Views/RackViewBatchProducts.cs`, F32 `Views/RackViewPlacement.cs`, F33 `Views/RackViewBatchDriver.cs` | 8 |
| Archivos exactos de Application | F34 `PushBackPilotMaterializer.cs`, F35 `PushBackPilotIntentReader.cs`, F36 `PushBackPilotIntentScan.cs`, F37 `Workspace/IWorkspacePilotEditPort.cs`, F38 `Workspace/WorkspacePilotSupport.cs` | 5 |
| Proyectos | F39 `RackCad.Application.csproj`, F40 `RackCad.Plugin.csproj`, F43 `RackCad.Tests.csproj` | 3 |
| Raíces de pruebas | F41 `tests/RackCad.UI.Tests/`, F42 `tests/RackCad.Tests/Fixtures/` | 2 |
| Pruebas exactas de Core | F44 `PushBackClassicCharacterizationPluginGuardTests.cs` (T1B) y F45 a F58 (las de regresión del barrido, de I-55, de Push Back, de sobres, de unidades y de espacios de nombres) | 15 |
| Repositorio | F59 `docs/`, F60 `.github/`, F61 `eng/`, F62 `tools/`, F63 `assets/`, F64 `deploy/`, F65 `RackCad.sln`, F66 `Directory.Build.props`, F67 `Directory.Build.targets`, F68 `global.json`, F69 `NuGet.Config`, F70 `README.md` | 12 |

- **[M]** El total es 7 + 18 + 8 + 5 + 3 + 2 + 15 + 12 = 70.
- **[M]** El prohibido no es exhaustivo: el control real es la lista blanca exacta. 1251 rutas de `src/` y `tests/` no están ni en el permitido ni en el prohibido (medido sobre `git ls-files src tests`).
  - Entre ellas, 34 de `src/RackCad.Application/Workspace/` y 60 de `tests/RackCad.Tests/Workspace/`.
  - También 5 de `src/RackCad.Plugin/Views/`: `RackProjectionCommandPort.cs`, `RackProjectionKindSessions.cs`, `RackProjectionWriteScope.cs`, `RackViewBatchExecution.cs` y `RackViewBatchProductSession.cs`.

### 3.2 Qué entradas tendrían que cambiar para T4C

| Entrada | ¿Cambia para T4C? | Por qué (con su ancla) |
|---|---|---|
| **F01 `src/RackCad.UI/`** | **Sí, partirla** | **[I]** T4C necesita la UI de Actualizar: «la llamada de la UI» (`T4BP:816`), R-T4-09 (`T4P:568-571`), M-03 (`…/F4-T4B…/task-close-package.md:131`) y «Aplicado con avisos» y Applying (`T4A2P:660`). **[M]** Hoy la UI tiene 44 entradas de primer nivel (`git ls-tree`), una de ellas `Workspace/` con 8 archivos |
| **F03 `src/RackCad.Plugin/Workspace/`** | **Sí, partirla** | **[I]** El cableado del puerto de actualización y el reenvío de la operación nueva en `PilotEditAdapter` (`WorkspaceController.cs:51`, `:278-318`). **[I]** Posiblemente `AutoCadEventBridge.cs`, por el ámbito de mutación propia (R-T4-08, IC-08), que es materia de otro mapa. **[M]** La carpeta tiene 9 archivos |
| **F37 `IWorkspacePilotEditPort.cs`** | **Sí, al permitido** | **[M]** «la operación del puerto `IWorkspacePilotEditPort` con sus cinco implementadores» (`T4BP:814`) |
| **F38 `WorkspacePilotSupport.cs`** | **Sí, al permitido** | **[M]** La compuerta `WorkspacePilotEditGate` implementa la interfaz (`:153`) |
| **F41 `tests/RackCad.UI.Tests/`** | **Sí, partirla** | **[M]** El doble de UI implementa la interfaz (`WorkspacePilotEditPortDouble.cs:25`), y las pruebas de la UI de Actualizar van en `tests/RackCad.UI.Tests/Workspace/` (24 archivos). **[M]** La raíz tiene 165 entradas de primer nivel, de las que 163 son `.cs` |
| F28 `Views/RackPilotIntentAdapter.cs` | **[I] A decidir; probablemente se queda** | R-T4-06 pide O-04 dentro del bloqueo, en contexto de comando y sin la puerta de reposo (`T4P:553`). **[M]** T4B ya lee dentro de la corrida por `ReadForPrepare` y `CaptureDrawing` (`RackPilotUpdateWriter.cs:74-86`), sin el lector D. La orden: «Do not migrate unrelated Workspace reads/navigation into command context» |
| F04 `src/RackCad.Plugin/Systems/` | No | Contiene las fachadas que fija CT-27 (1) y `SystemBlockWriter`. Tocarlo es DV-01 |
| F05, F06 y F07 | No | `Drawing/`, `KindHandlers/` (CT-27 (2) y su ancla) y la costura de I-55 |
| F08 a F25 (`*Commands*.cs`) | No, salvo DV-02 | **[M]** `RackPushBackCommands.cs` y `RackMenuCommands.cs` son de MutantScope (CT-24 a CT-27). Un comando registrado para Undo (D-13, la A-n de C-F4-02, `docs/initiatives/I-64-f4-gate-contract-package.md:59`) caería aquí o en el censo de 40 `[CommandMethod(` |
| F26, F27, F29 a F36 | No | Llamadores del barrido (H-20), `EraseViewBlocks` (R-T4-11) y el piloto de lectura |
| F39, F40, F43 | No | **[I]** `ExecuteInCommandContextAsync` ya está en las referencias de AutoCAD del Plugin; a confirmar en la compilación, no aquí |
| F42, F44 a F58 | No | F44 es la caracterización del Plugin de T1B; F45 a F58 son las de regresión |
| F59 a F70 | No | — |

**[M] Recuento:** 5 de 70 cambian (F01, F03, F37, F38 y F41), 1 queda a decidir (F28) y 64 se quedan.

**[M] Además, lo que el permitido de T4B tenía y T4C no necesitaría.** Tendría que pasar al prohibido:
- `src/RackCad.Plugin/Views/RackSiblingScan.cs`, cuyo movimiento de `Traverse` congelan las guardas exactas de T4B (INV-F4T4B-17);
- `IPushBackPilotUpdatePort.cs`, `PushBackPilotSystemRebuild.cs`, `PushBackPilotUpdatePlan.cs`, `DraftSession.cs` y `DraftApplyOutcome.cs`, salvo que un arrastre los toque (lo decide el mapa de los arrastres).

### 3.3 Respuestas directas

- **¿Está prohibido el adaptador nuevo en `Plugin/Views`?**
  - **[M] No.** `src/RackCad.Plugin/Views/` no es una raíz del prohibido de T4B. Solo lo son seis archivos exactos: F28 a F33.
  - Un archivo nuevo ahí solo necesita entrar en el `AllowedWriteScope` exacto de T4C.
  - R-T4-05 lo sitúa precisamente ahí (`T4P:548`).
- **¿Está prohibido `src/RackCad.UI/`, y lo necesitaría T4C?**
  - **[M]** Sí está prohibido, como raíz F01 en `:252`.
  - **[I]** T4C lo necesita, al menos en `src/RackCad.UI/Workspace/`.
- **[M] El control nc4 de T1B y de T4B** usó como sonda la copia que amplía `AllowedWriteScope` con `src/RackCad.UI/` (`…/F4-T1B…/task-close-package.md:29`; `…/F4-T4B…/task-close-package.md:29`).
  - **[I]** En T4C esa sonda ya no discrimina, porque parte de la UI pasa a estar permitida. El contrato de T4C necesita otra raíz de sonda, por ejemplo `src/RackCad.Domain/` o `src/RackCad.UI/Systems/PushBack/`.

### 3.4 Lo que se pierde al partir las raíces

**[M]** Hoy F01 y F41 cubren por raíz dos cosas:
- la clausura de T1B en la UI: G1 `Systems/PushBack/**` (10), G2 `Controls`, `Shell`, `Themes`, `Editor` y `Preview` (17 + 9 + 2 + 12 + 3 = 43), G3 (10) y G6 (17) (`docs/initiatives/I-64-f4-t1b-gate-contract-package.md:214-221`; las cuentas de G1 y G2 están medidas de nuevo con `git ls-files`);
- las seis clases de UI de T1B y la infraestructura que comparten: `StaTestRunner.cs`, `EditorWindowTestSupport.cs` y `EditorDiscardPromptCollection.cs`, que T1B prohibió (`…/F4-T1B…/coordinator/gate-contract.json:301-303`).

**[M]** Las pruebas de UI de Workspace usan `StaTestRunner` en 9 archivos (`git grep -c`).

**Opciones para el prohibido de T4C** (sin cerrar):

| Opción | Pros | Contras |
|---|---|---|
| (a) Enumerar las raíces y los archivos de la UI salvo `Workspace/` (unas 43 entradas) y, en las pruebas de UI, prohibir explicitamente las 6 clases de T1B, los 3 ayudantes y el `.csproj` | Conserva la protección de la clausura y de T1B por entrada explícita | Más entradas; F4-E60 ya señaló el riesgo de copia de una cobertura de 174 entradas (`f4-dispositions.md:2139`) |
| (b) Prohibir solo por subraíces de la UI (`Systems/`, `Controls/`, `Shell/`, `Themes/`, `Editor/`, `Preview/`, `Views/`, `RackFrames/`, `StructuralSections/`), más los archivos de G3 y G6 del primer nivel | Menos entradas y cubre G1, G2 y casi todo G3 | Hay que enumerar G6 y los de G3 del primer nivel; olvidar uno no lo detecta la raíz |
| (c) Confiar en la lista blanca exacta y prohibir solo lo de T1B, como en T4B | Mínimo | La revisión pierde el aviso explícito; el control nc2 ya tuvo un modo de fallo con listas recortadas (PD-F4-07, `f4-dispositions.md:2183`) |

**[M] Huecos a cerrar en cualquier opción:**
- `Views/RackViewBatchExecution.cs` y `Views/RackViewBatchProductSession.cs` están en el camino clásico de RACKEDITAR y del menú (`RackViewBatchExecution.Run`, CT-25 y CT-26). T1B los prohibió (`…/F4-T1B…/gate-contract.json:225-226`), y en T4B no están en ninguna lista.

---

## 4. La línea base de pruebas de `4314c180` (CI `43668817337`)

### 4.1 Fuente y método

- **[M] TRX** (fuera del índice; `artifacts/` está ignorado):
  - `artifacts/orchestration/I-64/F4-T4B-MUTATE-POST/1/R20261115T012820Z-7a4a/ci-core/core.trx`: `Counters` total 18684, ejecutadas 18684 y en verde 18684;
  - `…/ci-ui/ui.trx`: total 2310, ejecutadas 2293 y en verde 2293.
  - `relay-record.json` del mismo directorio nombra `43668817337` y `4314c1803b7a047f0521c34d631950a475e7cd32`.
- **[M] Filtro:** `FullyQualifiedName~X` = contiene `X` sin distinguir mayúsculas, sobre `className + "." + name` del `TestMethod`, y se cuentan los `UnitTestResult`.
- **[M] Coincide** con `ci-filter-counts.json` del mismo directorio: GreenCore 3103/3103 y GreenUi 601/601.

### 4.2 Filtros principales

| Proyecto | Filtro | Seleccionadas | Resultado | Clases | Base de T4B (`43650040527`) |
|---|---|---|---|---|---|
| Core | `RackCad.Tests.Workspace` | **3103** | 3103 Passed | 68 | 2923 |
| UI | `RackCad.UI.Tests.Workspace` | **601** | 601 Passed | 20 | 601 |
| UI | `RackCad.UI.Tests.PushBackClassicCharacterization` | **55** | 55 Passed | 6 (21 + 12 + 4 + 4 + 2 + 12) | 55 |
| Core | `RackCad.Tests.PushBackClassicCharacterization` | **10** | 10 Passed | 1 | 10 |

**[M]** Las clases nuevas de T4B dentro de `Workspace`: `WorkspacePilotUpdateGuardTests` 79, `WorkspacePilotUpdateHostTests` 35 y `WorkspacePilotUpdateRunTests` 55.

### 4.3 Las 21 clases de regresión de Core del contrato de T4B

**[M]** El contrato de T4B las fija en `:343-488`.

| Clase | `4314c180` | `MinSelected` en T4B | Clase | `4314c180` | `MinSelected` en T4B |
|---|---|---|---|---|---|
| `CallerOwnedFacadeGuardTests` | **81** | 48 | `CallerOwnedTransactionGuardTests` | 15 | 15 |
| `RackSiblingRedrawUnitsTests` | 2 | 2 | `DebugFaultInjectionGuardTests` | 1 | 1 |
| `SiblingRedrawSeamGuardTests` | 2 | 2 | `RackSiblingMembershipTests` | 12 | 12 |
| `RackSiblingRedrawPlanTests` | 2 | 2 | `RackSiblingRedrawRunTests` | 6 | 6 |
| `PushBackClassicCharacterizationPluginGuardTests` | 10 | 10 | `PushBackPluginSourceGuardTests` | 11 | 11 |
| `PushBackRoundTripSourceGuardTests` | 24 | 24 | `RackProjectionCommandGuardTests` | 34 | 34 |
| `NamespaceFolderGuardTests` | 7 | 7 | `CustomPropertiesEnvelopeGuardTests` | 22 | 22 |
| `RackUnitsGuardSourceTests` | 18 | 18 | `I55G15Auth15ConsumerTests` | 5 | 5 |
| `SingleScanInsertGuardTests` | 5 | 5 | `RackViewBatchDriverGuardTests` | 4 | 4 |
| `G16FirstViewCorrectiveTests` | 61 | 61 | `RackSiblingCustomPropertiesGateTests` | 2 | 2 |
| `RackEnvelopeIdProbeTests` | 5 | 5 | | | |

- **[M]** Todas están en Passed.
- **[M]** `CallerOwnedFacadeGuardTests` creció de 48 a 81 (+33) porque T4B extendió `Facades` con las tres de Push Back (`CallerOwnedFacadeGuardTests.cs:58-60`). T4B mantuvo 48 como `MinSelected` (`T4BP:735`). **[I]** Para T4C, la base es 81.

### 4.4 Clases de guarda que limitan `ExecuteInCommandContextAsync`, `async`, el puente de eventos o el panel

**[M]** Se buscaron con `git grep` sobre `tests/`:
- `ExecuteInCommandContextAsync|\basync\b|\bawait\b`;
- `AutoCadEventBridge`;
- `WorkspacePanelView|WorkspacePanelViewModel|WorkspacePaletteHost|WorkspacePilotFieldViewModel`.

Las cuentas son exactas por `className`.

| Clase (Core, salvo indicación) | Cuenta | Qué limita (ancla) | Impacto previsible en T4C [I] |
|---|---|---|---|
| `WorkspaceNavigationHostTests` | 66 | **Ningún archivo de `src` nombra `ExecuteInCommandContextAsync`** (`:436`); sin `async` ni `await` en las dos carpetas Workspace (`:435`); el host, los hechos, los efectos, el controlador y el lector no bloquean (`:439-449`) | **Rompe con certeza** con el adaptador. Hace falta la excepción por archivo exacto (H-13 y C-F4-02, `F4P:32`, `:59`; R-T4-05) |
| `WorkspaceHostSourceGuardTests` | 45 | Sin `async`, `await`, `.Wait(`, `.Result` ni `GetAwaiter` en las dos carpetas Workspace (`:424-443`); el host de navegación sin `ExecuteInCommandContextAsync` (`:446-455`) | Se rompe si el adaptador o una espera van a una carpeta Workspace. **[M]** Su cuenta de 45 la fija otra guarda (`WorkspaceLifecycleGuardTests.cs:860`) |
| `WorkspaceSourceGuardExceptionTests` | 88 | Igual en las dos carpetas Workspace (`:262-271`) | Igual |
| `WorkspacePilotPrepareGuardTests` | 29 | Sin `ExecuteInCommandContextAsync`, `async`, `await`, `Task`, `Dispatcher` ni `Timer` en el puerto, el soporte, la intención, el materializador, el plan, la sesión, **`WorkspaceHostCore.cs` y `WorkspaceController.cs`** (`:408-431`) | El controlador no puede tener un `Task`: la operación hacia el adaptador tiene que ser síncrona |
| `WorkspacePilotEditGuardTests` | 116 | El controlador: el adaptador que solo reenvía, la construcción literal, 3 `+=` y un `event` (`:482-529`); los dueños del indicador modal (`:560-600`) | Cambio declarado para el cableado del puerto de actualización (T4BP `:815`, la opción B-2 (b)) |
| `WorkspacePilotUpdateGuardTests` | 79 | El escritor: sin `LockDocument`, `async`, `await` ni `ExecuteInCommandContextAsync` (`:171`); el puerto sin campos ni objetos de AutoCAD (`:185-188`) | Se rompe si el bloqueo o el documento se meten en el escritor |
| `WorkspaceBrowserHostGuardTests` | 19 | La UI de Workspace, el host y los hechos sin `SendStringToExecute`, `CommandMethod` ni `ExecuteInCommandContextAsync`, y sin «editor clásico» en la XAML (`:257-263`) | Ninguno, si el adaptador no está en esos archivos |
| `WorkspacePaletteHostSourceGuardTests` | 45 | La carpeta UI de Workspace sin `Task`, `async`, `await`, `Dispatcher` ni `Timer`, con una sola excepción por archivo exacto (`WorkspaceNavigationViewModel`) y cuentas exactas (`:555-583`); el archivo tiene exactamente 45 pruebas (`:665`) | Se rompe si la UI de Actualizar necesita esperar |
| `WorkspacePilotSurfaceGuardTests` | 41 | `LimitViolations` sobre la UI de Workspace (`:278-292`): el dibujo, el foco, los diálogos y la programación | Igual |
| `WorkspaceHostHardeningGuardTests` | 100 | En los tres VM: sin `+=`, `-=`, `event`, `delegate`, `Dispatcher`, `Timer`, `Task`, `async`, `await`, `Keyboard` ni `.Focus(` (`:757-779`) | Se rompe si el VM del campo se suscribe a un final de Actualizar |
| `WorkspacePilotCloseSyncGuardTests` | 118 | `DrawingReach`, que incluye `ExecuteInCommandContextAsync` (`:851`); `FocusReach` y `DialogReach` (`:853-855`), aplicados por `CheckUiReach` (`:857-870`) | Igual |
| `WorkspacePilotReconciliationGuardTests` | 22 | Quién lee y quién llama, por token, en `src/RackCad.UI/` (D-T4A2-07, `T4A2P:361` y siguientes) | Cambio declarado si el VM del campo gana el gesto de Actualizar |
| `WorkspaceEditIntentGuardTests` | 121 | El host sin `async`, `await`, `Task.Run`, `Task.Delay` ni `Dispatcher` (`:481`); la contención como Failed (`:583-596`) | — |
| `WorkspaceEditIntentHostTests` | 78 | El bucle de firmas y el censo de portadores (`:970-1047`, según `T4BP:550`, `:577`) | Si la operación nueva del puerto entra en el bucle |
| `WorkspaceLifecycleGuardTests` | 54 | `PortMutatingOperations` y `ChangesADraft` (`:463-534`); la cuenta de 45 de `WorkspaceHostSourceGuardTests` (`:860`) | Probable, por la operación nueva del puerto |
| `WorkspaceDraftPilotIntentGuardTests` | 17 | `Task`, `async` y `await` en tres archivos, con el texto crudo (`:39-46`, `:96`) | — |
| `WorkspaceCommandAdvisorGuardTests` / `WorkspaceCommandAdvisorTests` | 82 / 76 | Sin ventana, modal ni `async` (`:312-345`); los dos dobles del puerto (`:17`, `:1107`) | Los dobles cambian con la operación nueva |
| `WorkspaceIdleSchedulingGuardTests` | 124 | El cableado del puente, `Dispatcher` y `SetPost`, en todo `src` (`:772-830`) | Si el ámbito propio toca el puente |
| `WorkspaceBridgeSourceGuardTests` / `WorkspaceBridgeFacadeSourceGuardTests` / `WorkspaceHostC2SourceGuardTests` / `WorkspaceNavigationAuthorityTests` | 52 / 17 / 15 / 10 | El puente de eventos y sus fachadas | Si T4C toca `AutoCadEventBridge.cs` (IC-08) |
| `WorkspaceDraftBoundaryTests` | 152 | Las fronteras del borrador; `async` como inyección (`:310`) | — |
| `WorkspacePilotEditHostTests` / `WorkspaceNavigationPrivacyGuardTests` | 76 / 11 | La operación del puerto y `NavigationTargets` fuera del panel | — |
| `SelectiveEditorOpenTests` | 33 | El censo de `[CommandMethod(` = 40 en todo el Plugin (`:565-572`) | Solo en DV-02 |
| `CustomPropertiesCommandGuardTests` | 48 | La clasificación de los comandos | Solo en DV-02 |
| UI: `WorkspacePilotFieldViewModelTests`, `WorkspacePilotFieldViewTests`, `WorkspacePanelViewTests`, `WorkspacePilotReconciliationTests` / `ViewTests` y `WorkspacePilotCloseSyncTests` / `ViewTests` | 77, 38, 8, 125 / 9 y 78 / 17 | El VM y la vista del campo y del panel | Las que T4C ampliaría (§5) |

---

## 5. Las corridas locales (F4-E59) y las pruebas de UI que T4C tocaría

**[M] Las reglas de F4-E59:**
- «en tareas con contención de perfil, nunca suites completas de UI en local; solo en la CI» (`f4-dispositions.md:2082`);
- «los prompts prohíben las sondas con entradas válidas en caminos con diálogo o escritura» (`:2086`).
- En T4B se tradujeron a INV-F4T4B-18: en local, solo los filtros de UI de `RequiredTests` (`T4BP:768-772`).
- T4B corrió en local solo los filtros de Core, la suite completa de Core en Debug y los dos filtros de UI (`…/F4-T4B…/task-close-package.md:86`).

**[M] H-1, la causa de la regla:**
- `RackFrameConfiguratorLayoutStore` escribe en `LocalApplicationData` (`src/RackCad.UI/RackFrames/RackFrameConfiguratorLayoutStore.cs:26`, `:81`, `:98`) cuando se cierra el configurador real;
- 21 archivos de `tests/RackCad.UI.Tests` lo instancian o lo usan por `HeaderConfiguratorPresenter` (`git grep -l`), entre ellos `PushBackHeaderAuthorityWindowTests.cs`;
- H-1 sigue abierta (`…/F4-T4B…/task-close-package.md:166`).

**[I] Las pruebas de UI que T4C tocaría:**
- `tests/RackCad.UI.Tests/Workspace/WorkspacePilotEditPortDouble.cs`, porque el doble tiene que implementar la operación nueva;
- `WorkspacePilotFieldViewModelTests.cs` (77) y `WorkspacePilotFieldViewTests.cs` (38), por el botón, Applying, los avisos y M-03;
- `WorkspacePanelViewTests.cs` (8);
- posiblemente `WorkspacePilotReconciliationTests.cs` (125) y `ViewTests` (9), por `ReconcileText` y el diagnóstico de `TryCheckNow`;
- posiblemente `WorkspacePilotCloseSyncTests.cs` (78) y `ViewTests` (17);
- y clases nuevas.

**[M] Ventanas reales y escritura en el perfil:**
- En `tests/RackCad.UI.Tests/Workspace/` no aparece `new Window`, `.Show()`, `ShowDialog`, `MessageBox`, `LocalApplicationData`, `GetFolderPath`, `RackFrameConfigurator`, `EditorDiscardPrompt` ni `[Collection(` (`git grep`).
  - Las vistas se construyen como `UserControl` (`src/RackCad.UI/Workspace/WorkspacePanelView.xaml:1`) y se miden con `Measure` y `UpdateLayout`, sin mostrarse (`WorkspaceBrowserViewTests.cs:29-47`; `WorkspacePilotFieldViewTests.cs:25-35`).
- **[M] La única ventana real del filtro Workspace de UI** es `RackPushBackSystemWindow`, construida sin mostrar en `WorkspaceDraftPilotWindowLoadCharacterizationTests.cs:72` (17 resultados), con `LoadExisting` y sin el configurador.
  - En la ventana, la única escritura en disco es `SaveLibrary_Click` (`RackPushBackSystemWindow.xaml.cs:3468`), que esa prueba no ejerce.
- **[I] Conclusión:**
  - los dos filtros de UI de la línea base son seguros en local frente a H-1;
  - las seis clases de T1B muestran la ventana fuera de pantalla, pero no escriben (`…/F4-T1B…/task-close-package.md:130`);
  - la suite completa de UI no es segura en local.
- **[M] Sondas:**
  - el adaptador de T4C solo se puede ejercer con AutoCAD (ADR-0003, `PluginGuardTests.cs:15-19`), así que una sonda local del camino de escritura es imposible sin host;
  - **[I]** una sonda con un doble del puerto no escribe, pero un camino de diálogo del panel (la confirmación de «Descartar y recargar») sigue bajo la regla.

---

## 6. Smoke-2

### 6.1 Lo que exigen el §7 del paquete de T4B y la orden del Owner

- **[M] El §7 de T4B** (`T4BP:794-809`; reproducido en `…/F4-T4B…/task-close-package.md:153-162`):
  - bytes y handles conservados (R-T4-04);
  - una transacción y `TopTransaction == null`;
  - la equivalencia de las tres lecturas;
  - R-09;
  - la agrupación de Undo de la importación;
  - S2-R-10 con `SEAL=before` y `SEAL=after`;
  - una capa bloqueada en un miembro, que da NotAppliedPreflight sin residuo;
  - el movimiento de `Traverse`: Insertar de Selectivo (A), el insertar desde RACKEDITAR Push Back (B), RACKPROYECTAR (C) y el lector del piloto (D);
  - la purga sin definiciones obsoletas, con el residuo de una purga que falla declarado y no observado.
- **[M] La orden del Owner** (§«SMOKE-2»): empieza solo cuando cierra el producto de T4C; es obligatorio y es la primera validación física de la vía propia. Incluye:
  - un rack Push Back real;
  - un Actualizar real;
  - el resultado authored correcto;
  - un Undo lógico, su restauración y el Redo;
  - el conflicto;
  - **cancel**;
  - la discordancia de documento;
  - la contención de la mutación propia, sin enmascarar otro rack;
  - CLOSE, CLOSEALL y QUIT;
  - la temporización de IC-04;
  - casos de navegación con racks representativos.
  - La orden también dice: «Do NOT claim final Undo compliance before Smoke-2» (§«UNDO»).

### 6.2 Las demás obligaciones acumuladas (medidas)

| Fuente | Qué añade |
|---|---|
| Freeze §12.4 (`I-64-proposal-v5.md:877-898`) | S2-01 a S2-15. **S2-12** (no editable aquí) y **S2-15** (la distribución de admisión) piden **dibujos del Owner** |
| A-7 (`docs/initiatives/I-64-A-7.md:86-98`, `:117`) | S2-16 a S2-22 (los campos del panel y `DBMOD`); F4 PASS los exige |
| F4 §7 (`F4P:108-117`) | La lista de la orden (abrir un rack real, `DBMOD`, texto pendiente inválido…), el registro Debug de `TopTransaction == null` y la parada «UNDO que no sea un solo paso sin la A-n de D-13» |
| C-F4-06, F4-E09 (`f4-dispositions.md:102-106`) | **Sin dispensa.** Mínimo: los 22 de la orden, y además repetir SN-06c, la parte de pestañas de SN-10, Seleccionar, Ver y Localizar sobre racks representativos, y la seguridad al cambiar de documento |
| T4A2 §7 (`T4A2P:634-651`) | S2-R-01 a S2-R-12 (la reconciliación), con el historial de deshacer y `DBMOD` sin cambio |
| T4 §7 y §8 (`T4P:550`, `:573`, `:585-587`) | **La primera medida de T4C en el host:** si el callback corre antes de que vuelva la llamada y si un Actualizar da un paso de Undo. Si no, A-n. Un dibujo antiguo para `LegacyDeltas` (`T4P:182`). «El procedimiento se fija tras T4C» |
| T2 (`docs/initiatives/I-64-f4-t2-gate-contract-package.md:114`) | Al menos un rack Push Back de un solo lado, admisible y con anotaciones |
| T2B2 (`docs/initiatives/I-64-f4-t2b2-gate-contract-package.md:256-322`) | El oráculo del orden de los eventos en CLOSE y CLOSEALL; QUIT es una inferencia, a medir en S2-06 |
| T2C (`docs/initiatives/I-64-f4-t2c-gate-contract-package.md:253-262`) | La continuación dentro de los bucles modales, la latencia de Background y `LockDocument` desde la continuación |
| IC-08 (`docs/initiatives/I-64-implementation-clarifications.md:131-153`) | «Tras Actualizar A, el borrador B no queda marcado» (INV-28); la frecuencia del marcado |
| T1B D-T1B-07 (`T1BP:461-470`; `…/F4-T1B…/task-close-package.md:159`) | **Candidatos**, no obligaciones: el dueño efectivo con `ShowModalWindow`, WM_CLOSE y Alt+F4, el orden Z, el foco y la tabulación, S-2 y OD-3, y Escape y Enter desde el elemento con foco. El §7 de T1B dice «T1B no añade escenarios. Los candidatos del D-T1B-07 los decide el Owner» (`T1BP:624-626`) |

**[M] Las inyecciones:**
- T4B las creó solo en Debug: `RACKCAD_DEBUG_FAIL_PILOT_UPDATE_UNIT` y `RACKCAD_DEBUG_FAIL_PILOT_UPDATE_SEAL` (`RackPilotUpdateWriter.cs:217-218`);
- la regla del Freeze: solo por variable de entorno **del proceso**, acusadas con I-52 (`I-64-proposal-v5.md:831-832`).

### 6.3 Lo que ya existe del procedimiento

- **[M] La guía** `docs/guias/validacion-manual-autocad.md`:
  - el entorno y el DLL Debug del worktree, con su SHA-256 (`:12-65`);
  - NETLOAD (`:67-78`);
  - **el Candidato exige «UI Full local: PASS» y la CI del SHA exacto** (`:60-65`, `:395-453`, en particular `:424-425`: «Un Candidato sin UI Full local no es Candidato»).
- **[M] El Freeze, §12.1** (`I-64-proposal-v5.md:817-834`):
  - la estación principal del Owner, tras un RELEASE de I-52 y con una sola sesión de AutoCAD;
  - **un Candidato completo con la suite de UI completa en local** (`:828-829`);
  - el DLL Debug con su SHA-256 (`:830`);
  - «el Worker nunca usa AutoCAD: prepara el procedimiento y el Owner lo ejecuta» (`:833`);
  - `TopTransaction == null` en todos los hitos (`:834`).
- **[M] Los procedimientos previos de I-64:**
  - `docs/initiatives/I-64-f1-t3d-smoke-1-procedure.md`, con la plantilla `docs/automation/evidence/I-64-pilot/F1-T3D-SMOKE-PROCEDURE/operator-record-template.md`;
  - `docs/initiatives/I-64-f2-smoke-nav-procedure.md`.
- **[M] La evidencia de Smoke-NAV** (`docs/automation/evidence/I-64-pilot/F2-SMOKE-NAV/`, corridas `R20261110T064058Z-c883` y `R20261110T073451Z-e221`):
  - PASS sobre `003be859`, con SN-08 LIMITADA y SN-06c y la pestaña de SN-10 `NOT_APPLICABLE_UNTIL_F3` (`…/e221/disposition/smoke-nav-disposition.md:4-5`);
  - **operada por la sesión, por computer use, bajo la instrucción nocturna del Owner, con dibujos sintéticos** N y N2 (`…/e221/operator/operator-record.md:3-4`);
  - el Candidato declaró «UI Full local: PASS (1792/1809, 17 Skip)» (`…/e221/candidate/candidate.md:5`);
  - el kit trae guarda de procesos, captura del perfil, host-extras y medidas (`…/e221/kit/`).
- **[M] El cierre de F3** deja SN-06c y la pestaña de SN-10 pendientes (IC-02) y SN-08 LIMITED (`docs/automation/evidence/I-64-pilot/F3-BROWSER/gate-close.md:50`).
- **[M] No existe un procedimiento de Smoke-2 de I-64** (`git ls-files docs`: no hay ningún `*smoke-2*` de I-64). Los `smoke2` versionados son de I-65 (`docs/automation/evidence/I-65-f4/…`, `docs/initiatives/I-65-f4-second-smoke-procedure.md`) y sirven, como mucho, de precedente de forma.

### 6.4 El operador y los dibujos que exige C-F4-06

- **[M] El paquete de F4** (`F4P:58`):
  - «S2-12 y S2-15 exigen dibujos **del Owner**»;
  - las reglas del §12.1: RELEASE de I-52, una sola `acad.exe`, el DLL Debug con su SHA-256 y las inyecciones por variable del proceso, acusadas con I-52;
  - **«el Owner ejecuta»**;
  - «Cualquier dispensa (representativos, operación por la sesión) se da por separado».
- **[M] La disposición del Owner** (F4-E09, `f4-dispositions.md:102-106`), **«sin dispensa»**:
  - «dibujos reales y representativos de RackCad, con participación del Owner u operador»;
  - la sesión prepara la build exacta, el DLL, el procedimiento, los resultados esperados, los formularios y la recuperación;
  - F4 no cierra sin esas observaciones en el host.
- **[I] Los dibujos que se desprenden de los hechos anteriores:**
  - uno o varios del Owner con Push Back real admisible, de un solo lado y con anotaciones;
  - uno antiguo, para `LegacyDeltas`;
  - racks no editables: sobre sin Id, divergente, Cama con dos miembros y A/B (S2-12, S2-R-06);
  - dos racks del piloto en el mismo documento (S2-14, «otro rack no enmascarado»);
  - dos documentos (la discordancia, SN-08);
  - un miembro en una capa bloqueada;
  - una pieza de biblioteca faltante (S2-11);
  - un rack Selectivo y RACKPROYECTAR para los llamadores A y C del barrido.

### 6.5 Lo que solo el host puede probar

**[H]**
- Si el callback de `ExecuteInCommandContextAsync` corre antes de que vuelva la llamada (R-T4-05);
- un solo paso de Undo, con su Undo y su Redo;
- la agrupación de Undo de la importación;
- `LockDocument` sobre el documento correcto;
- las pistas propias y ajenas en el puente (IC-08);
- el orden de los eventos en CLOSE, CLOSEALL y QUIT;
- IC-04;
- la no intercalación de un comando tecleado (S2-13);
- que RACKEDITAR sigue igual (S2-07);
- y todo D-T1B-07.

---

## 7. Riesgos

| Id | Riesgo | Base |
|---|---|---|
| R-01 | La guarda de todo `src` sobre `ExecuteInCommandContextAsync` rompe con certeza; si su excepción no es por archivo exacto, se abre la puerta a la navegación | [M] `WorkspaceNavigationHostTests.cs:436`; H-13 (`F4P:32`); la orden: «only the exact Actualizar host/mutation integration path» |
| R-02 | El controlador y la UI no admiten `Task` ni `async`. Si el callback no corre de forma síncrona, el resultado de T4B no vuelve por un puerto síncrono sin bloquear (`.Wait`/`.Result`, prohibidos en las carpetas Workspace) | [M] `WorkspacePilotPrepareGuardTests.cs:408-431`, `WorkspaceHostSourceGuardTests.cs:424-443`, `WorkspacePaletteHostSourceGuardTests.cs:555-583`; [H] R-T4-05 |
| R-03 | Un bloqueo con `.Wait()` en el adaptador de `Plugin/Views` no lo detecta ninguna guarda: la de `.Result` solo mira `Plugin/Workspace` | [M] `WorkspaceHostSourceGuardTests.cs:442-443`; [H] el posible interbloqueo |
| R-04 | Al partir F01 y F41, la clausura de T1B y sus 6 clases de UI dejan de estar cubiertas por una raíz; una omisión en la enumeración no la detecta nada | [M] §3.4 |
| R-05 | La sonda nc4 heredada (`src/RackCad.UI/`) deja de discriminar en T4C | [M] los dos cierres, `:29` |
| R-06 | El número de implementadores de `IWorkspacePilotEditPort` es 6, no 5; un contrato que enumere 5 deja fuera al núcleo | [M] §2.1 |
| R-07 | `MinSelected` de `CallerOwnedFacadeGuardTests` copiado de T4B (48) no detectaría que se pierden las 33 filas de Push Back | [M] §4.3 |
| R-08 | Las cuentas de archivo fijadas (45 pruebas en `WorkspacePaletteHostSourceGuardTests` y `WorkspaceHostSourceGuardTests`, y 3 `+=` en el controlador) hacen frágil añadir pruebas o suscripciones | [M] `WorkspacePaletteHostSourceGuardTests.cs:665`; `WorkspaceLifecycleGuardTests.cs:860`; `WorkspacePilotEditGuardTests.cs:524` |
| R-09 | Unificar el catálogo de la corrida con el de las fachadas cambia firmas que fija T1B | [M] §2.2; DV-01 |
| R-10 | El Candidato de Smoke-2 exige UI Full local, contra la regla de F4-E59, mientras siga H-1 | [M] la guía (`:424-425`), el Freeze (`:828-829`) y `f4-dispositions.md:2082` |
| R-11 | Smoke-2 se apoya en RACKEDITAR como modificador externo; un fallo del ámbito propio puede parecer un cambio de RACKEDITAR | [I] §2.2; [H] S2-04 y S2-07 |
| R-12 | Las obligaciones de Smoke-2 están repartidas en al menos 10 fuentes y no hay procedimiento: es fácil omitir alguna (por ejemplo, SN-06c o S2-R-xx) | [M] §6.2 y §6.3 |

## 8. Preguntas abiertas

| Id | Pregunta | Para quién |
|---|---|---|
| P-01 | ¿Entran en Smoke-2 los candidatos D-T1B-07 («los decide el Owner»), y cuáles? No se ha encontrado una decisión expresa en la autorización de T1B ni en su cierre | Owner |
| P-02 | El Candidato de Smoke-2: ¿UI Full local, aceptando H-1 declarado (fecha cambiada, SHA-256 igual); solo CI exacta más los filtros locales, con una dispensa; o resolver H-1 antes, con una costura propia? | Owner |
| P-03 | ¿Opera Smoke-2 el Owner o un operador humano? F4P `:58` dice «el Owner ejecuta» y F4-E09 dice «con participación del Owner u operador». ¿Se admite operar por la sesión, como en Smoke-NAV? Haría falta una dispensa aparte (`F4P:58`) | Owner |
| P-04 | ¿Qué es «cancel» en la lista de Smoke-2 de la orden: Esc durante Actualizar, la cancelación del comando interno o el «No» de una confirmación? | Owner o Coordinator |
| P-05 | ¿Puede T4C cambiar `RackPilotUpdateWriter.cs` (pasarle el `Document` exacto) y la guarda de T4B que prohíbe campos y `Document` en el puerto, o eso es «reabrir T4B»? La orden: «Do not reopen T4B merely because T4C integrates it» | Owner o Coordinator |
| P-06 | ¿Se mantienen los dos catálogos por corrida, declarados, o se pide unificarlos (DV-01)? | Coordinator, y Owner si es DV-01 |
| P-07 | ¿Qué opción de prohibido (a), (b) o (c) del §3.4, y qué raíz de sonda para nc4? | Sesión, con la revisión |
| P-08 | ¿F28 (`RackPilotIntentAdapter.cs`) queda prohibido, dado que T4B ya lee dentro de la corrida? | Sesión, con el mapa de lecturas |

## 9. Posibles condiciones de devolución

Frente a «RETURN BEFORE EMISSION» de la orden del Owner.

| Id | Hecho | Condición que podría activar | Probabilidad [I] |
|---|---|---|---|
| **DV-01** | CT-27 (1) congela las firmas `internal` de `PrepareRedraw` y `RedrawInTransaction`, sin catálogo (`PluginGuardTests.cs:322-347`). Hoy hay dos cargas de catálogo por corrida (`RackPilotUpdateWriter.cs:81`; `ViewBlockDraw.cs:83`) y la orden asigna a T4C «catalog/read dependencies required by the run» | **«T1B behavior must change»**, si T4C decide pasar el catálogo de la corrida a las fachadas | Baja, si se mantiene la doble carga declarada por T4B (IR M-4) |
| **DV-02** | Si un Actualizar por `ExecuteInCommandContextAsync` no da un solo paso de Undo, D-13 manda una A-n «hacia comando registrado» (`F4P:59`). Un comando registrado cambia el censo de 40 `[CommandMethod(` (`SelectiveEditorOpenTests.cs:565-572`) y, si va en un `*Commands*.cs`, toca el prohibido; en `RackPushBackCommands.cs` tocaría MutantScope | **«Undo requires a material architecture change»** y, según dónde se registre, **«T1B behavior must change»** | [H] desconocida hasta la medida del host |
| **DV-03** | Si el callback no corre antes de que vuelva la llamada, el puerto síncrono del panel no puede devolver el resultado de T4B sin `async` en la UI o en las carpetas Workspace, que las guardas prohíben (R-02) | **«DocumentLock/command context cannot wrap the T4B run cleanly»**, u otra decisión material | [H] desconocida |
| **DV-04** | Pasar al escritor el `Document` exacto («receives only the exact current document/context») choca con la guarda de T4B sobre la clase del puerto (`WorkspacePilotUpdateGuardTests.cs:185-188`) | «Any other material product decision», si se considera reabrir T4B | Baja-media |
| **DV-05** | La guía y el Freeze exigen UI Full local para el Candidato de un hito, y F4-E59 lo prohíbe con H-1 abierta | No es una decisión de producto, pero es una decisión material del Owner **antes de Smoke-2**. No bloquea la emisión de T4C | Alta de que haga falta decidir |

**[M] No se ha encontrado ningún hecho** que obligue a T4C a cambiar RACKPUSHBACK, RACKEDITAR, el menú, la ventana clásica ni sus pruebas de caracterización (§2.3).

## 10. Recuento

- **Hechos medidos [M]:** 94 afirmaciones marcadas en §0 a §6. Algunas del §0 repiten hechos del cuerpo.
- **Inferencias [I]:** 23, en §0 a §6.
- **Solo host [H]:** 10 puntos (la lista del §6.5).
- **Riesgos:** 12 (R-01 a R-12).
- **Preguntas abiertas:** 8 (P-01 a P-08).
- **Posibles condiciones de devolución:** 5 (DV-01 a DV-05).

**Abreviaturas de anclas:**
- `T1BP` = `docs/initiatives/I-64-f4-t1b-gate-contract-package.md`;
- `T4BP` = `docs/initiatives/I-64-f4-t4b-gate-contract-package.md`;
- `T4P` = `docs/initiatives/I-64-f4-t4-gate-contract-package.md`;
- `T4A2P` = `docs/initiatives/I-64-f4-t4a2-gate-contract-package.md`;
- `F4P` = `docs/initiatives/I-64-f4-gate-contract-package.md`;
- `…/F4-T1B…` = `docs/automation/evidence/I-64-pilot/F4-T1B-PILOT-CHARACTERIZATION`;
- `…/F4-T4B…` = `docs/automation/evidence/I-64-pilot/F4-T4B-MUTATE-POST`;
- `PluginGuardTests.cs` = `tests/RackCad.Tests/PushBackClassicCharacterizationPluginGuardTests.cs`;
- `SourceGuardTests.cs` = `tests/RackCad.UI.Tests/PushBackClassicCharacterizationSourceGuardTests.cs`;
- `CloseTests.cs` y demás = sus archivos en `tests/RackCad.UI.Tests/`;
- las guardas de Workspace sin ruta están en `tests/RackCad.Tests/Workspace/`.
