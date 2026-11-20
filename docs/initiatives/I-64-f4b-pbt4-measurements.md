> **Redacción para el repo público:** las rutas locales se sustituyen por marcadores (`<checkout-principal>`, `<carpeta-del-paquete>`). El resto no cambia.

# I-64 F4B — PB-T4: el registro de lo medido (v1 a v4.3)

**Cómo se midió.** Todo por objetos git en `7dfc7c21` (la BASE de PB-T3c, F4B-E47), con `git -C <checkout-principal> show|grep|ls-tree|log`. No se leyó ningún archivo del disco ni ningún worktree, no se compiló y no se ejecutó ninguna prueba. El guion `measure_pbt4.py` escribe `measure_pbt4-out.txt`. Las lecturas largas de los paquetes y del código se repartieron en tres lecturas delegadas con las mismas reglas; lo que cito de ellas lleva su ruta:línea en `7dfc7c21`.

**Lo que falta en `7dfc7c21`:** el GREEN de PB-T3c. Todo lo que depende de él va con el marcador `<tras el cierre de PB-T3c>`.

**Abreviaturas:** P, el paquete de la fase (`docs/initiatives/I-64-f4b-push-back-completo-package.md`, v3.1); P2 y P3, los paquetes de PB-T2 (v2.9) y PB-T3 (v4.7); OD, `F4B-DRAFTS/coordinator/owner-f4b-disposition.md`; DISP, `F4B-DRAFTS/f4b-dispositions.md`; C/, `F4B-DRAFTS/coordinator/`; IF, `docs/ideas-futuras.md`; WHC, `src/RackCad.Application/Workspace/WorkspaceHostCore.cs`; FVM, `src/RackCad.UI/Workspace/WorkspacePilotFieldViewModel.cs`; W, `src/RackCad.UI/Systems/PushBack/RackPushBackSystemWindow.xaml.cs`; PP y UR, el plan y la corrida de `Systems/PushBack/Pilot/`.

## R-0. Las revisiones

| Qué | Valor | Fuente |
|---|---|---|
| La BASE de PB-T3c | `7dfc7c21a0b30f5e04d691768b1717778e5c4ad7`, «emite el contrato de gate (AuthorityRevision e3c293b1)» | `measure_pbt4-out.txt` |
| La X de PB-T3c | `e3c293b173a9d0914803bb344595f0e48dde0390`, el paquete de PB-T3 v4.7 (F4B-E47) | ídem; `F4B-PBT3C-FULL-WRITE-ERASE/coordinator/gate-contract.json:5`, `:8` |
| El contrato de PB-T3c | 54 162 bytes | M-10 |
| PB-T3a y PB-T3b | CERRADAS: F4B-E43 (GREEN `6919e17a`) y F4B-E46 (GREEN `c7582785`, cierre `fe1c67f4`) | DISP:1328, :1470 |

## R-1. La autoridad de PB-T4 y su estado

- **La fila de PB-T4 de [OD]**, literal (OD:219-223): «ACCEPT. Add "Editar todo" to the Workspace only after PB-T1/T2 establish the draft-based editor seam. Do not make PB-T4 invent state translation logic. It should consume the already proven full Push Back draft/state API.»
- **El estado:** «PB-T4: WAITING» (OD:288-289; P:49).
- **La emisión:** ninguna autorización la cubre. F4B-E28 Q4 cubre PB-T2a a c (`C/owner-pbt2-decisions.md:35-47`) y F4B-E35 Q3, PB-T3a a c (`C/owner-f4b-pbt3-emission-red-repair-decisions.md:9-26`).
- **El contenido** (P:506): abrir la ventana en WORKSPACE sobre el borrador; el puerto generalizado; los pasos 1 a 3 de D-PB-05 y OB-05.4, con la composición de PB-T3 conectada (OB-05.5); la guarda de texto de OB-01.3; R-PB-10; el resumen y «Modificado». PB-T5 (P:507): la CI exacta, el Smoke-PB y el gate-close.
- **D-PB-01** (P:133-157, literal en el paquete), **OB-01.1 a OB-01.6** (P:163-181), **D-PB-05** con los pasos 1 a 7 (P:267-299) y **OB-05.1 a OB-05.5** (P:303-336), **OB-03.1 y OB-03.2** (P:220-221), **R-PB-10** (P:737), **D-PB-09** (P:442-457), **D-PB-10** (P:460-471), **§4** (P:477-493), **§7.3** (P:688-710), **§7.4** (P:712-722) y **§9** (P:740-750).

## R-2. El panel hoy (el piloto)

| Hecho | Fuente |
|---|---|
| Un solo `Begin`: `_drafts.Begin(..., pilot.Read, PushBackPilotStateComparer.Instance, PushBackPilotStateComparer.Copy)` | WHC:347 |
| Nueve `Find<PushBackPilotDraftState>` y ningún `Find<PushBackFullDraftState>` | WHC:403, 498, 583, 667, 754, 859, 1307, 1723, 1909 (M-1) |
| «Conservar»: `draft.KeepMineOnCurrentVersion(pilot.Read, PushBackPilotMaterializer.KeepPilotEditsOn, confirmed: true)` en `TryKeepPilotEdit` | WHC:636, :694; `KeepPilotEditsOn` en `Pilot/PushBackPilotMaterializer.cs:242` |
| «Actualizar»: `TryUpdatePilot` → `PushBackPilotUpdateRun.Precheck(draft)` y `.Execute(draft, …)` | WHC:844, :864, :895 |
| El campo de escala: `PushBackPilotFieldAdapter` | WHC:42, :1136, :1778, :1781; `Pilot/PushBackPilotFieldAdapter.cs:21`, `:35` |
| La marca *known clean*: `NotePilotEdit` | WHC:832, :926, :1139, :1167, :1221, :1252 |
| El modal del panel: `SetModalRackCadActive` | WHC:123, :163, :166; `WorkspaceController.EnterPanelModal()` en `src/RackCad.Plugin/Workspace/WorkspaceController.cs:141`, **sin llamadores** (M-2) |
| Con el modal activo, `PassGate` y `UpdateGate` responden `Waiting` | WHC:1939, :996 |
| El puerto del panel, `IWorkspacePilotEditPort` (16 operaciones) y `enum PilotBoundaryKind { TabChange, RackChange, DocumentChange, PanelHide, Dematerialize, DocumentClose, Update }`, «later tasks may add values at the end» | `src/RackCad.Application/Workspace/IWorkspacePilotEditPort.cs:27-115`, `:181` |
| La petición: `record EditIntentRequest(SessionId Session, string RackId, long? ChosenReference, EditIntentWitness? Witness)`; `record EditIntentWitness(string Key, string? Text)` | `src/RackCad.Application/Workspace/WorkspaceEditIntent.cs:13`, `:19` |
| Los botones del campo: Editar, Recargar, Descartar, Comprobar ahora, Actualizar, Descartar y recargar, Conservar mi borrador; Sí/No de la elección | `src/RackCad.UI/Workspace/WorkspacePanelView.xaml:219-267` (M-3) |
| Los gestos del campo: `Clear`, `BeginEdit`, `CommitPending`, `Rematerialize`, `CheckNow`, `Update`, `RequestDiscardAndReload`, `RequestKeepMine`, `ConfirmChoice`, `CancelChoice` | FVM:320-639 (M-3) |
| «Actualizar» del campo: `TakeUpdateProgress` → `CrossBoundary(…, PilotBoundaryKind.Update)` → `RequestUpdate` | FVM:518-570 |
| «Conservar» del campo: `RequestKeepMine` → `AskChoice` (Sí/No en línea) → `ConfirmChoice` → `_port.TryKeepMine` | FVM:583, :589, :623 |
| «Modificado»: `NameOf(PilotFieldState.Dirty)`; la pestaña con « *» si `IsProtected` (= `draft.IsDirty`) | FVM:1145; `WorkspaceBrowserViewModel.cs:1297`; WHC:1758 |
| La frontera del texto pendiente: el setter `Text` → `SetPendingText`; Enter o foco → `CommitField`; TabChange, RackChange y Update son bloqueantes | FVM:195, :404, :414; WHC:1184; `Drafts/DraftSession.cs:90`, `:518` |
| No hay «Editar todo» ni «Restaurar» en el panel | M-2 |

## R-3. El borrador

| Hecho | Fuente |
|---|---|
| `DraftRegistry.Begin<TState>(session, rackId, DraftIntentRead<TState> read, IEqualityComparer<TState> comparer, Func<TState, TState> copy)`; `Find<TState>` con `as` (otro `TState` da null) | `Drafts/DraftRegistry.cs:143`, `:169` |
| `DraftSession<TState>`: `InitialDraftState` (copia), `CurrentDraftState` (referencia), `IsDirty`, `SetCurrentDraftState` (copia; Clean, Dirty o Conflict), `KeepMineOnCurrentVersion(read, keepOn, confirmed)`, `TryBeginApply`, `CompleteApply` | `Drafts/DraftSession.cs:284`, `:287`, `:298`, `:345`, `:469`, `:510`, `:533` |
| `IDraftPendingTextAdapter<TState>` (`Validate` y `Apply`): su única implementación es la del piloto. **No hay adaptador para `PushBackFullDraftState`** | `Drafts/DraftSession.cs:108`; `Pilot/PushBackPilotFieldAdapter.cs:21` |

## R-4. El modelo completo (PB-T3a y PB-T3b), sin conectar

| Hecho | Fuente |
|---|---|
| Los ocho archivos de `Draft/` y su API: `PushBackFullDraftState(editable, origin)`; su comparador con `Copy`; el asiento (`Pass`, `LoadExisting`); el materializador (admite A/B, rechaza la cola); el lector `Read(scan, catalog)`; `ForFullWrite` y `KeepOn`; `PushBackViewDispositions.Of`; `PushBackConflictClassifier.Classify(baseState, currentRack, draft, currentMembers, catalog)` | `src/RackCad.Application/Systems/PushBack/Draft/*.cs`; P3:411-722 |
| El plan: `PrepareFull(PushBackFullPrepareInput, PushBackFullIntentResult, RackCatalog)`; `PushBackFullPrepareInput.From(DraftSession<PushBackFullDraftState>)`; la barrera con `PushBackEditorOutputGate` | PP:79, :118, :445, :569 |
| La corrida: `Precheck(DraftSession<PushBackFullDraftState>)` y `ExecuteFull(draft, request, port)` | UR:145, :201 |
| **Nada del producto los llama:** `ExecuteFull`, `PrepareFull`, `KeepOn` y `Classify` solo en `Draft/`, PP y UR | M-2 |
| **El Plugin no borra vistas** en `7dfc7c21`: nadie implementa `IPushBackPilotViewEraser`; un plan con `Erasures` daría `EraseUnsupportedReason`. `<tras el cierre de PB-T3c>`: `RackPilotMutationScope` lo implementa con `RackViewBlockEraser` | M-2; UR:505-506 |

## R-5. La ventana en WORKSPACE (PB-T2) y su puerto

| Hecho | Fuente |
|---|---|
| `enum PushBackEditorMode { Classic = 0, Workspace = 1 }` | `src/RackCad.UI/Systems/PushBack/PushBackEditorMode.cs:8` |
| La única entrada: `ForDraft(PushBackDraftEditorLaunch launch)`; no se muestra ni tiene dueño: los pone el anfitrión | W:296, :172, :170 |
| «Aplicar al borrador» llama al puerto con una copia y no cierra; sin cambios no lo llama | W:3253-3273, :3439, :3445 |
| «Cerrar»: `PendingWork()` pregunta primero por el módulo escenificado y, si no lo hay, por lo no aplicado (`WorkspaceUnappliedQuestion`) | W:3405, :3427-3432, :3455 |
| `RefuseInWorkspace()` en `RequestDraw` e `InsertBatch_Click` | W:3316 |
| BOM y «Guardar en biblioteca» se ofrecen en WORKSPACE (F4B-E23) | W:3461, :3487 |
| El puerto: `IPushBackDraftStatePort.ApplyToDraft(PushBackEditableState)`; `PushBackDraftSessionPort` y `LaunchFor` sobre `DraftSession<PushBackEditableState>`; `PushBackDraftEditorLaunch(state, rackId, sourceProject, port)` | `src/RackCad.Application/Systems/PushBack/PushBackDraftStatePort.cs:8-64`; `PushBackDraftEditorLaunch.cs:13` |
| **Nadie del producto llama a `ForDraft`, `PushBackDraftSessionPort` ni `LaunchFor`** | M-2 |
| **El desajuste:** el puerto es sobre `DraftSession<PushBackEditableState>` y el borrador completo es `DraftSession<PushBackFullDraftState>`. A-T3-11 pide el adaptador que conserva `Origin` | P3:1870 |

## R-6. Los rechazos y la validación

| Hecho | Fuente |
|---|---|
| AUTH-13: `AuthoredRawReader` lanza «AUTH-13 <ruta>: unknown member»; el lector del piloto lo traduce a «No editable aquí: …» | `Systems/Shared/AuthoredRawReader.cs:44`, `:95`; `Pilot/PushBackPilotIntentReader.cs:217-222` |
| El rechazo A/B del panel: `CompositeOutOfPilotReason` = «Rack de dos lados (A/B): fuera del piloto.» → `OutOfPilotDiagnostic` = «No editable aquí: fuera del piloto (F5)», con `ClassicEditorHint` | `Pilot/PushBackPilotMaterializer.cs:106`, `:148`; `PushBackPilotIntentReader.cs:47`, `:56`, `:229` |
| La cola: `DormantTailOutOfPilotReason` = «El diseño aparca una cola compuesta: fuera del piloto.» | `PushBackPilotMaterializer.cs:108`, `:170` |
| **PB-T3 no quitó el rechazo A/B** (OB-03.2): el lector completo lo admite, sin conectar | P3:402, :530, :958, :1862 |
| Dos validaciones de la escala: el panel (`PushBackPilotFieldAdapter.Validate`) y la ventana (`NumericFieldValidation.Validate` con las reglas de `AnnotationScaleBox`, declaradas en `RackPushBackSystemWindow.xaml:68`); CT-PBT2B-10 las compara en diez textos | `tests/RackCad.UI.Tests/PushBackFieldValidationConcordanceTests.cs:20-29`; `src/RackCad.UI/Controls/NumericFieldValidation.cs:76` |

## R-7. Las rutas clásicas de OB-01.3

`RackPushBackCommands` (`src/RackCad.Plugin/RackPushBackCommands.cs:30`); `RackMenuCommands` (`src/RackCad.Plugin/RackMenuCommands.cs:19`); `EditorModules` (`src/RackCad.UI/Editor/EditorModules.cs`, `PushBackEditorModule` en :99); `RackViewBatchExecution` (`src/RackCad.Plugin/Views/RackViewBatchExecution.cs:10`); `DrawAndPlace` y `RedrawInPlace` en las tres fachadas de `src/RackCad.Plugin/Systems/PushBack/`; `InsertRequested` e `InsertionRequest` (`src/RackCad.UI/Editor/RackEditorSession.cs:106`, `:121`; W:286, :293).

## R-8. Las pruebas que fijan el piloto en el panel (el censo de la lista para el Owner)

**Lo que fija el piloto por nombre** (M-5): `KeepPilotEditsOn` en 4 archivos; `PushBackPilotUpdateRun.Execute` en 4; `Find<PushBackPilotDraftState>` en 12; `PushBackPilotStateComparer` en 10; `PushBackPilotFieldAdapter` en 5; el rechazo A/B en 5; `NonPilotContentReason` en 2.

**Las clases que pasan por el anfitrión real y nombran Push Back** (M-6): 15 clases de `tests/RackCad.Tests/Workspace/` y las dos guardas de PB-T3, con **644 métodos en total**. Es una **cota superior**: no se ejecutó nada, y la mayoría de esos métodos prueban el ciclo de vida o los avisos sin depender del tipo del borrador.

| Clase | Métodos |
|---|---|
| WorkspacePilotEditHostTests | 59 |
| WorkspaceEditIntentHostTests | 62 |
| WorkspacePilotReconciliationHostTests | 28 |
| WorkspacePilotUpdateHostTests | 35 |
| WorkspacePilotUpdateHostRunTests | 27 |
| WorkspacePilotUpdateRequestTests | 44 |
| WorkspaceLifecycleHostTests | 69 |
| WorkspaceAssociativeBookkeepingTests | 10 |
| WorkspaceCommandAdvisorTests | 54 |
| WorkspaceEditIntentGuardTests | 66 |
| WorkspaceLifecycleGuardTests | 44 |
| WorkspacePilotUpdateIntegrationGuardTests | 43 |
| WorkspacePilotUpdateGuardTests | 42 |
| WorkspacePilotEditGuardTests | 39 |
| WorkspacePilotPrepareGuardTests | 17 |
| PushBackWritePathGuardTests (CT-PBT3B-13) | 2 |
| PushBackDraftWriteGuardTests (CT-PBT3A-12) | 3 |

**La lista exacta no se puede medir sin compilar ni ejecutar:** sale de la comprobación del §6.0 del paquete (precondición 6), sobre una copia exportada.

## R-9. Las guardas que condicionan el diseño

| Guarda | Ancla | Lo que obliga |
|---|---|---|
| `WorkspacePilotSurfaceGuardTests` | :341 | La UI del Workspace no abre diálogos (`EnterPanelModal`, `ShowModalWindow`, `ShowDialog`, `MessageBox`): el abridor va en el Plugin |
| `WorkspaceBrowserHostGuardTests` | :263 | El XAML del panel no ofrece «Abrir en editor» ni «editor clásico»; «Editar todo» no coincide |
| `WorkspacePilotCloseSyncGuardTests` | :442-444 | La forma pública de `WorkspaceBrowserViewModel` son exactamente 33 firmas: los gestos nuevos van en FVM |
| `WorkspacePilotReconciliationGuardTests` | :155, :370 | Los cinco gestos y los seis botones de la reconciliación, con sus manejadores; no prohíbe botones nuevos |
| `WorkspaceEditIntentGuardTests` | :711 | Fija la línea `KeepMineOnCurrentVersion(pilot.Read, PushBackPilotMaterializer.KeepPilotEditsOn, confirmed: true)` |
| `WorkspaceBridgeSourceGuardTests` | :139-142, :163-180 | Ningún archivo de `src/RackCad.Plugin/Workspace/` nombra `ShowModalDialog`, `ShowModelessDialog`, `ShowDialog`, `MessageBox`, `GetObject`, `OpenCloseTransaction`… |
| `WorkspaceModelBoundaryTests` | :41-42 | `Application/Workspace/` sin `System.IO`, `Transaction`, `LockDocument`, `Database`, `Commit(` ni `Autodesk.` |
| `WorkspacePilotUpdateIntegrationGuardTests` G-4 | :500-501 | `PushBackPilotUpdateRun.Execute(` lo nombra solo WHC, y exactamente una vez |
| `PushBackWritePathGuardTests` (CT-PBT3B-13) | :235-296 | Fuera del plan, la corrida y `Draft/`, nadie nombra la escritura completa; WHC solo hace `Find<PushBackPilotDraftState>` |
| `PushBackDraftWriteGuardTests` (CT-PBT3A-12) | :44-88 | Fuera de `Draft/`, PP y UR, nadie nombra los cinco tipos del modelo completo |
| `PushBackWorkspaceModeSourceGuardTests` (CT-PBT2B-08) | :178-192, :286 | Los once literales `Workspace*` de W y las once reglas del modo |

## R-10. Las decisiones y los arrastres

Las líneas de F4B-E01 a F4B-E47, en DISP. Los arrastres de PB-T4, en P2:1477-1493 (A2-1 a A2-13) y P3:1857-1872 (A-T3-1 a A-T3-11). Las decisiones al detalle:

| Qué | Literal o resumen | Fuente |
|---|---|---|
| A-T2b-OWNER-01 | «Corregir en PB-T4 (Recomendado)»: «allí 'Cerrar' hará una sola pregunta que nombre las dos cosas, con su prueba» | `C/owner-pbt2b-close-decisions.md:27-36`; DISP:1121; IF:1871 |
| Q-PBT2-3 | «En PB-T4, con prueba ya (Recomendado)»: PB-T4, «al rehacer el campo del panel sobre el estado nuevo, deja una sola» | `C/owner-pbt2-decisions.md:27-33`; OD:201 |
| D-1 de PB-T3 | «Un solo código (Recomendado)»; PB-T4 sustituye E-3, E-5 y E-6 **si quita el piloto**; «PB-T4 vuelve al Owner con su lista, como D-1» | `C/owner-pbt3-d1-decision.md:13-25`; P3:1228-1236, :1861 |
| D2 | «This is NOT authorization for general shallow copying. … STOP and reopen L-1»; L-5 lo extiende a PB-T4 | `C/owner-d1-d2-decisions.md:89-92`, `:133-154`; `docs/initiatives/I-64-f4b-pbt1-gate-contract-package.md:1458` |
| La ExtensionData anidada | «Basta así (Recomendado)»: el rechazo AUTH-13 del panel sigue | `C/owner-f4b-dpb07-extensiondata-decisions.md:27-35`; DISP:684 |
| La interfaz | «la interfaz o botones extra que haya visto los arreglamos después cuando se finalice la primera etapa, lo importante es la funcionalidad» | `C/owner-note-ui-deferred.md:5` |
| F4B-E21, E25, E35 Q4, E42, E45 y E46 | Las colecciones opacas; las rutas `UNIT_CHANGE` en una corrección; la regla RR; renombrar contra G-9; compilar el esqueleto; el total exacto de cada rama | DISP:625, :693, :986, :1298, :1435, :1470 |

## R-11. Las limitaciones abiertas que tocan PB-T4

| Limitación | Fuente |
|---|---|
| OWNER-01 de «Restaurar» (va una pasada por detrás) | DISP:859; IF:1809-1819; cierre de PB-T1c:43 |
| OWNER-01 de PB-T2c: un lado B declarado sin frentes sobrevive en el borrador; «Qué escribe el panel lo deciden PB-T3 y PB-T4» | IF:1879; D-PB-08 en P:418-440 |
| PBT3B-IR-M01 con OWNER-2 (AUTH-13 no admite `SchemaVersion` 1.1 ni nombres distintos; los pasos 5 a 7 se prueban por construcción) | `F4B-PBT3B-FULL-WRITE-RUN/2/R20261120T075251Z-a9e0/independent-review.md:120-124`; IF:1903-1905, **sin** OWNER-2 hasta el cierre de PB-T3c |
| El layout del configurador que escriben las pruebas de UI | DISP:834-837; IF:1800-1807 |

## R-12. La CI y los controles

| Hecho | Fuente |
|---|---|
| La línea base de PB-T3c: Core 19 124 (19 118 + 6 omitidas, las de D-1), UI 2 451 (2 430 + 21), corrida `44707482956` sobre `c7582785` | `F4B-PBT3C-…/gate-contract.json:697`; M-10 |
| El GREEN previsto de PB-T3c: Core 19 129 (19 123 + 6), UI 2 451 (2 430 + 21) | DISP:1536-1538 |
| **La línea base prevista de PB-T4a:** la del GREEN de PB-T3c. `<tras el cierre de PB-T3c>` | — |
| nc1: `PBT3x-NC1-CASCADE-SIGNAL-01`, que liga `--task PBT4` al prefijo `F4B-PBT4-` del `TaskId` | `F4B-DRAFTS/nc1-cascade-substitute/nc1_cascade_signal_substitute.py:41`, `:205` |
| nc2: `PBTx-NC2-EXACT-WHITELIST-F4B-01` v2, que liga `--task PBT4` al mismo prefijo y exige en `ExpectedEvidence` uno de sus dos literales, byte a byte | `nc2_exact_whitelist_substitute_f4b_v2.py:14-23`, `:410`; su `limits-declared.md` |
| nc3: sin sustituto (F4B-E46) | DISP:1506 |
| Los `TaskId` propuestos: `F4B-PBT4-FULL-DRAFT-PANEL` (25) y `F4B-PBT4-EDIT-ALL-WINDOW` (24), con el prefijo `F4B-PBT4-` | M-9 |

---

## Mediciones de la v2

**Cómo se midió.** Con `measure_pbt4_v2.py` (salida en `measure_pbt4_v2-out.txt`), por objetos git en `7dfc7c21`, con las mismas reglas. La revisión midió con `ir4_measure.py` (`ir4_measure-out.txt`).

**Abreviaturas nuevas:**
- WEI: `src/RackCad.Application/Workspace/WorkspaceEditIntent.cs`;
- NH: `src/RackCad.Plugin/Workspace/WorkspaceNavigationHost.cs`;
- RIA: `src/RackCad.Plugin/Views/RackPilotIntentAdapter.cs`;
- EIH, EIG, PEG, RG, URT e IG: `WorkspaceEditIntentHostTests`, `WorkspaceEditIntentGuardTests`, `WorkspacePilotEditGuardTests`, `WorkspacePilotReconciliationGuardTests`, `WorkspacePilotUpdateRequestTests` y `WorkspacePilotUpdateIntegrationGuardTests`, en `tests/RackCad.Tests/Workspace/`;
- [PT1]: `docs/initiatives/I-64-f4b-pbt1-gate-contract-package.md`.

**Correcciones de la v1:**
- son **17** operaciones del puerto, no 16 (PEG:44-65);
- **R-8 no es una cota superior** (§4 de la revisión): fuera de las 17 clases hay al menos `WorkspacePilotPrepareTests` (pasa por el anfitrión real), las pruebas de UI de la reconciliación y RG:116. En total, 26 archivos y 739 métodos nombran algo que cambia (`ir4_measure-out.txt` §2).

**El cuarto `TaskId`:** `F4B-PBT4-FULL-READ-PATH` (24), de PB-T4a, con el mismo prefijo.

## R-13. El camino de la lectura (B-01)

| Hecho | Fuente |
|---|---|
| `EditIntentReadResult(status, reason, PushBackPilotIntentResult? pilotRead)`; `FromPilot`, `NoActiveDocument`, `Busy`, `Rejected` y `Failed` | WEI:35-64 |
| `EditIntentRequest(SessionId Session, string RackId, long? ChosenReference, EditIntentWitness? Witness)`; `EditIntentWitness(string Key, string? Text)` | WEI:13, :19 |
| `ChooseOriginDefinition(NavigationTargetSet set, long? chosenReference)` | WEI:682-692 |
| `IWorkspaceEditIntentReader.ReadIntent(SessionId session, string rackId, long? chosenReference, EditIntentWitness? witness)` | `IWorkspaceEditIntentReader.cs:14-20` |
| NH: `ReadIntent` → `ReadOnActiveDocument` → `RackPilotIntentAdapter.LoadCatalog()` → `LockDocument` → `ChooseOriginDefinition` → `RackPilotIntentAdapter.Read(` → `FromPilot(read.Pilot)` | NH:207-251 |
| RIA: `CaptureDrawing`, `new PushBackPilotIntentScan(rackId, originKey, scan.Facts, scan.RawEnvelopes, CompletenessEvidence, witness)` y `PushBackPilotIntentReader.Read(input, catalog)`; `RackPilotIntentReadResult(Pilot, FailureReason)` | RIA:30-43, :94-142 |
| `PushBackFullDraftReader.Read(PushBackPilotIntentScan scan, RackCatalog catalog)`, que llama primero al lector del piloto | `Draft/PushBackFullDraftReader.cs:51-59` |
| `PushBackPilotIntentResult` lleva `PreviewWitness`; `PushBackFullIntentResult`, no | `Pilot/PushBackPilotIntentResult.cs:25-59` |
| WHC: seis `ReadIntent(…, request.ChosenReference, request.Witness)` (:328, :409, :503, :590, :675 y :759); seis `var pilot = read.PilotRead;`; tres `WitnessStatusOf(pilot.PreviewWitness)` (:344, :519 y :691) | `measure_pbt4_v2-out.txt` B01-g |
| `FromPilot(` en las pruebas: 12 (EIH:52, :770, :1193; `WorkspacePilotReconciliationHostTests` 9) | B01-h |

## R-14. Las guardas del camino de la lectura

| Guarda | Ancla |
|---|---|
| EIG: ningún `.PilotRead` fuera de `var pilot = read.PilotRead;`; 6 asignaciones; 6 `.ReadIntent(` | EIG:725-737 |
| EIG: NH, una `RackPilotIntentAdapter.LoadCatalog()` y una `RackPilotIntentAdapter.Read(` | EIG:414-430 |
| EIH: la forma del resultado (`PilotRead` solo con `Read`) | EIH:1175-1194 |
| EIH: un solo campo de WHC de tipo de `RackCad.Application.Systems`, contando los argumentos genéricos (`Flatten`) | EIH:1075-1099 |
| Las guardas que leen RIA o NH: `WorkspaceCommandAdvisorGuardTests` (:21-22), EIG (:25, :138, :414-430, :963, :1023, :1219), `WorkspaceHostSourceGuardTests` (:27), RG (:25) e IG (:27-28) | B01-n |

## R-15. Los miembros del puerto, K = 1 y la UI

| Hecho | Fuente |
|---|---|
| PEG: las 17 operaciones (:44-65), las seis de lectura (:68), las de memoria (:70), las que mutan (:72) y los cuerpos con marca (:82-91) | PEG:40-95 |
| PEG: los nombres de `typeof(IWorkspacePilotEditPort).GetMethods()` iguales a las 17 | PEG:156-163 |
| PEG: `WorkspaceHostCore : IWorkspaceLifecycleAdvisor, IWorkspacePilotEditPort`; el adaptador del piloto, único campo estático; las operaciones que no leen no nombran el lector | PEG:265-289 |
| PEG: dos interfaces exactas en WHC | PEG:380-385 |
| URT: los 7 nombres de `PilotBoundaryKind` | URT:1107-1109 |
| WHC: la marca `_pilotReadInProgress` en `TryBeginEdit` (:1020-1031) y las comprobaciones de `SetPendingText` (:1127), `CommitField` (:1155), `CrossBoundary` (:1208), `DiscardRack` (:1243) y `DiscardDocument` (:1270), que dan `Busy` | `git grep _pilotReadInProgress` |
| WHC: `CommitField` cruza `DraftBoundary.TabChange` con el adaptador | WHC:1150-1182 |
| `PilotFieldStatus` = { Rejected, Applied, NotEditable, Busy, Invalid } | `IWorkspacePilotEditPort.cs:143-157` |
| La UI de F4: «AskingForAnOptionCallsNoOperationOfThePortAndOnlyReadsItsMemory» | `tests/RackCad.UI.Tests/Workspace/WorkspacePilotReconciliationTests.cs:780-793` |
| RG: la autoprueba muta `public void RequestKeepMine() => AskChoice(ChoiceKind.KeepMine);` | RG:109-123 |
| La UI: 40 llamadas a `RequestKeepMine` o `ConfirmChoice` en `tests/RackCad.UI.Tests/Workspace/` | I02-i |

## R-16. La regla de la escala (B-02)

| Hecho | Fuente |
|---|---|
| Las pruebas referencian Domain y Application (Core) y Domain, Application y UI (UI); ninguna el Plugin | `RackCad.Tests.csproj:23-31`; `RackCad.UI.Tests.csproj:25-27` |
| `AnnotationScaleBox`: `Minimum="0" MinimumInclusive="False"` | W.xaml:68-69 |
| `NumericField` valida con `NumericFieldValidation.Validate` | `src/RackCad.UI/Controls/NumericField.cs:149` |
| `IDraftPendingTextAdapter<TState>.Validate` devuelve `DraftFieldValidation(bool IsValid, string? Message)`, de `Workspace.Drafts` | `Drafts/DraftSession.cs:95-110` |
| El adaptador del piloto lleva su propia regla («Escribe un valor.», «Valor no numérico.», «El valor debe ser > 0.») con `LocalizedNumberParser` | `Pilot/PushBackPilotFieldAdapter.cs:21-55` |

## R-17. Lo que el modelo completo usa del piloto (Q1)

`Draft/` usa las constantes de `PushBackPilotMaterializer` (15 líneas) y `PushBackPilotIntentReader.Read` (`PushBackFullDraftReader.cs:56`). El lector del piloto usa `PushBackPilotMaterializer.Materialize` (`PushBackPilotIntentReader.cs:225`). Fuente: Q1-a de `measure_pbt4_v2-out.txt`.

## R-18. `NaN` (I-07)

| Hecho | Fuente |
|---|---|
| `PalletTolerance` con `double.Equals` | `PushBackEditableStateComparer.cs:409` |
| `RackModuleCommit.SameAs` compara con `!=` | `Systems/Shared/RackModuleEditSession.cs:76-` |
| A-9 de PB-T1 | [PT1]:1553 |

## R-19. El abridor y la CI de UI (M-09, M-12)

| Hecho | Fuente |
|---|---|
| El abridor clásico: `AcApplication.ShowModalWindow(window);`, sin `Owner =` | `PushBackClassicCharacterizationPluginGuardTests.cs:196-209` |
| La suite de UI: un STA compartido, con un `MessageBox` modal real | `.github/workflows/ci.yml:213-222` |

## R-20. PB-T1: A-5, A-9, A-10 y R-T1-17 (I-05)

| Arrastre | Texto | Fuente |
|---|---|---|
| A-5 | «La copia de la carga del piloto (PM:150-172) — PB-T4 la retira al consumir el API» | [PT1]:1549 |
| A-9 | `SameAs` con `!=` y `NaN` | [PT1]:1553 |
| A-10 | El coste de `Equals` (M15) | [PT1]:1554 |
| R-T1-17 | El modelo paralelo interino, contra ADR-0029 D12 | [PT1]:1367 |
| D-1 | La premisa «al quitar el piloto» | `owner-pbt3-d1-decision.md:5-27` |

---

## Mediciones de la v3

**Cómo se midió.** Con `measure_pbt4_v3.py` (salida en `measure_pbt4_v3-out.txt`), por objetos git en `7dfc7c21`. La v3 se escribe con `make_v3.py` y `v3_sections.py`, que solo leen la v2 y escriben la v3.

## R-21. El lector doble y el kit del piloto (C-0; Q4)

| Hecho | Fuente |
|---|---|
| `EditIntentFakeReader : IWorkspaceEditIntentReader` responde por defecto `EditIntentKit.Admitted`, es decir, `FromPilot` | EIH:13-27, :49-52 |
| Los archivos de kit (las clases `EditIntentKit`, `PrepareKit`, `IntentKit`, `PilotKit`, `DraftKit` y `WorkspaceHostRig`) | `WorkspaceEditIntentHostTests.cs`, `WorkspacePilotPrepareTests.cs`, `WorkspaceDraftPilotIntentStaleBaseTests.cs`, `WorkspaceDraftPilotStateTests.cs`, `WorkspaceDraftSessionTests.cs` y `WorkspaceHostTestSupport.cs` |
| **Las 9 clases que empiezan borradores con ese lector y su número de métodos** | `measure_pbt4_v3-out.txt` §A |
| **Por texto,** los métodos de las 9 con alguna firma de C-1 a C-6: 2, 15, 5, 9, 1, 5, 3, 3 y 1, en el orden de la tabla | §A |

| Clase | Métodos | Con alguna firma de C-1 a C-6 |
|---|---|---|
| `WorkspaceLifecycleHostTests` | 69 | 2 |
| `WorkspacePilotPrepareTests` | 67 | 15 |
| `WorkspaceEditIntentHostTests` | 62 | 5 |
| `WorkspacePilotEditHostTests` | 59 | 9 |
| `WorkspaceCommandAdvisorTests` | 54 | 1 |
| `WorkspacePilotUpdateRequestTests` | 44 | 5 |
| `WorkspacePilotUpdateHostTests` | 35 | 3 |
| `WorkspacePilotReconciliationHostTests` | 28 | 3 |
| `WorkspaceAssociativeBookkeepingTests` | 10 | 1 |
| **Total** | **428** | **44** |

**Los demás archivos que usan `WorkspaceHostRig`** (`WorkspaceHostLifecycleTests`, `WorkspaceHostDrainTests`, `WorkspaceOwnMutationTests`, `WorkspaceHostAuthorizationTests`, `WorkspaceBridgeCommandStateTests` y `WorkspaceBridgeNoActiveDocumentTests`; 117 métodos) no tienen ninguna firma. El guion los cuenta porque busca `WorkspaceHostRig`, pero no son de C-0 si no empiezan un borrador de Push Back.

## R-22. Los reenvíos entre el panel y el anfitrión (B-05)

| Hecho | Fuente |
|---|---|
| `_pilotEdit = new PilotEditAdapter(new WorkspacePilotEditGate(_host, _noticeDepth));` | WCo:52 |
| `PilotEditAdapter : IWorkspacePilotEditPort`, anidado, con las 17 operaciones reenviadas | WCo:283-327 |
| `WorkspacePilotEditGate : IWorkspacePilotEditPort`, la compuerta de avisos: las seis lecturas dan `Waiting` dentro de un aviso; las demás reenvían | `WorkspacePilotSupport.cs:153-` |
| `ControllerViolations`: el adaptador sobre la compuerta; cada operación de la lista, solo reenviada | PEG:683-695 |
| No hay más reenvíos: `WorkspaceCloseAdvisorRelay` recibe `_pilotEdit` para los avisos de cierre | WCo:57-60 |

## R-23. Las hojas del diseño (Q2)

Las rutas de hoja distintas del `Design` (texto JSON dentro del sobre), con los índices normalizados: `pb-01-base` 136, `pb-20-ab` 165 y `pb-07-combinado` 139. Primer nivel: `Cantilever`, `DynamicSystem`, `FlowBed`, `Header`, `Kind`, `Larguero`, `PushBack`, `SchemaVersion` y `SelectiveRack`. Fuente: §B.

## R-24. Las roturas por letra (I-12, I-14)

| Guarda | Ancla |
|---|---|
| EIG: el orden de la intención y de «comprobar ahora»; un `.ReadIntent(` y un `_editIntentReader` seguido de `.` o `!` por cuerpo | EIG:563-575 |
| EIG: los tres cuerpos de la reconciliación, con su orden y una lectura cada uno | EIG:754-770 |
| La recarga: el orden y una lectura | `WorkspaceLifecycleGuardTests:642-656` |
| La base solo en sus consumidores (`BaseOrReadUse`, :648); WHC no nombra `.InitialState` ni `.InitialDraftState` | EIG:664-690, :720 |
| 11 estados de `EditIntentStatus`, con `Kept` el último | `WorkspacePilotPrepareTests:1350-1355` |
| El texto del XAML de la escala | `tests/RackCad.UI.Tests/Workspace/WorkspaceDraftPilotValidationParityTests.cs:81-98` |
| 9 archivos en `Plugin/Workspace/` | EIG:1011-1024; `WorkspaceNavigationHostTests:714-723` |
| La actualización no lee (`UpdateNeverReads`) | EIG:1123-1124 |

## R-25. El origen tras POST (I-13)

| Hecho | Fuente |
|---|---|
| `DraftApplyOutcome<TState>` expone `PostRead`, sin clave | `Drafts/DraftApplyOutcome.cs:40-67` |
| POST relee el miembro `plan.PostOriginKey` | UR:601-603 |
| `ChooseOrigin`: el único miembro mutable cuyo texto es, byte a byte, el origen del borrador | UR:640-650 |
| `PushBackPilotDraftState.Origin` | `Pilot/PushBackPilotDraftState.cs:15-32` |
| WHC ya nombra `draft.CurrentDraftState` | WHC:1772 |

## R-26. K = 1 (I-15)

`RequestUpdate` mira la marca antes que `PassGate`: `if (_pilotReadInProgress) return EditUpdateStart.Busy(PilotReadInProgressReason);` (WHC:1388). `PassGate`, en :1392.

## R-27. D2, la letra (I-16)

«L-1 SAFETY CONDITIONS», condiciones 1 a 10, con la 7: «comparing InitialDraftState vs CurrentDraftState does not depend on mutable changes inside those shared objects» (`owner-d1-d2-decisions.md:113-131`).

---

## Mediciones de la v4 (en `7f0d9aa3`, el cierre de PB-T3c)

**Cómo se midió.** Con `measure_pbt4_v4.py` (salida en `measure_pbt4_v4-out.txt`), por objetos git en `7f0d9aa3`. La v4 se escribe con `make_v4.py` y `v4_sections.py`, que solo leen la v3.

## R-28. Lo que cambia entre `7dfc7c21` y `7f0d9aa3`

En `src/` y `tests/`, 4 archivos con 726 inserciones y 4 borrados: el escritor, la fachada `RackViewBlockEraser` y dos guardas de PB-T3c. **Lo medido en `7dfc7c21` sigue valiendo.** Fuente: §E de la salida.

## R-29. Las firmas por método más ayudantes, a un nivel (B-06)

**El método:** cada método de prueba de las 9 clases, delimitado por llaves equilibradas, más el cuerpo de cada ayudante de su mismo archivo que llama por su nombre, a un nivel. Las firmas son las de D-PBT4-11.

| Clase | Métodos | Con alguna firma | Sin C-8 | Solo por el cuerpo (sin C-8) |
|---|---|---|---|---|
| `WorkspaceLifecycleHostTests` | 69 | 19 | 17 | 2 |
| `WorkspacePilotPrepareTests` | 67 | 58 | 56 | 33 |
| `WorkspaceEditIntentHostTests` | 62 | 30 | 30 | 14 |
| `WorkspacePilotEditHostTests` | 59 | 48 | 43 | 8 |
| `WorkspaceCommandAdvisorTests` | 54 | 6 | 6 | 0 |
| `WorkspacePilotUpdateRequestTests` | 44 | 43 | 43 | 6 |
| `WorkspacePilotUpdateHostTests` | 35 | 23 | 23 | 8 |
| `WorkspacePilotReconciliationHostTests` | 28 | 23 | 23 | 16 |
| `WorkspaceAssociativeBookkeepingTests` | 10 | 1 | 1 | 1 |
| **Total** | **428** | **251** | **242** | **88** |

La revisión (§14.3) contó 256 con su propio delimitador. La diferencia (de 251 a 256) viene de cómo se cortan los métodos y del recuento de C-8.

## R-30. La validación numérica (B-07)

| Hecho | Fuente |
|---|---|
| `NumericFieldValidation` (UI) solo usa `System.Globalization` y `RackCad.Application.Formatting` (`LocalizedNumberParser`) | `src/RackCad.UI/Controls/NumericFieldValidation.cs:1-2` |
| Su API pública: `NumericFieldStatus`, `NumericFieldValidationResult` (con constructor interno), `NumericFieldValidation.Validate`, `EmptyRequiredMessage` y `NotANumberMessage` | `NumericFieldValidation.cs:7-76` |
| `LocalizedNumberParser` vive en Application | `src/RackCad.Application/Formatting/LocalizedNumberParser.cs:11` |
| 27 archivos de pruebas nombran `NumericField` o `NumericFieldValidation`, y todos por su API. **Ninguno lee el texto de esos archivos:** las cuatro menciones de los archivos fuente son comentarios `<see cref>` | §C |
| La guarda de la UI clásica (`PushBackClassicCharacterizationSourceGuardTests:171`) lee `src/RackCad.UI/Systems/PushBack`, no `Controls/` | §D |

## R-31. El censo de CT-PBT1B-15 y las carpetas del abridor (B-07)

| Hecho | Fuente |
|---|---|
| 14 archivos del Plugin con `PushBack` en el código; **`WorkspaceController.cs` no es uno de ellos** | §D |
| `src/RackCad.Plugin/Views/` tiene 15 archivos; las guardas que barren esa carpeta solo miran `RackPilotIntentAdapter.cs` (EIG:1023) y `RackProjection*.cs` | §D |
| `src/RackCad.Plugin/Systems/PushBack/` tiene 3 archivos, las tres fachadas, fijadas por `CallerOwnedFacadeGuardTests:58-60` y `PushBackPluginSourceGuardTests:40-42` | §D |
| `WorkspaceHostSourceGuardTests:398-420` prohíbe `RackCad.UI` y `System.Windows` en `Plugin/Workspace/` y `Application/Workspace/`, salvo la paleta y el puente | §E |

## R-32. Las pruebas de lo que se retira (Q1 «Retirar»)

149 métodos por texto en 19 archivos (el método más los ayudantes de su archivo, a un nivel); 99 están fuera de las 9 clases del anfitrión:
- **las guardas de texto,** 35: EIG 6, PEG 12, `WorkspacePilotPrepareGuardTests` 3, `WorkspacePilotUpdateGuardTests` 5, IG 8 y `PushBackWritePathGuardTests` 1;
- **las de comportamiento,** 64:
  - `WorkspaceDraftPilotMaterializationTests` 24;
  - `WorkspaceDraftPilotStateTests` 15;
  - `WorkspaceDraftPilotIntentStaleBaseTests` 9;
  - `WorkspacePilotUpdateRunTests` 6;
  - `PushBackFullWritePrepareTests` 3;
  - 7 de la UI (`PushBackFieldValidationConcordanceTests` 1, `WorkspaceDraftPilotCanonicalLineEndingTests` 2, `WorkspaceDraftPilotValidationParityTests` 2 y `WorkspaceDraftPilotWindowLoadCharacterizationTests` 2).

La revisión (§14.3) da 130 y unos 57 con otro criterio. Fuente: §B.

## R-33. Los menores (M-25, M-27)

| Hecho | Fuente |
|---|---|
| `PushBackEditorState.SetWorkingBaseline(PushBackSystem baseline)` | `PushBackEditorState.cs:74-79` |
| El comparador no compara la línea base ni el informe de reconciliación | `PushBackEditableStateComparer.cs:17-27` |
| F4B-E48, OWNER-1 a OWNER-3 (espacio papel o bloques del usuario, purga de un nivel, bloques dinámicos) | `f4b-dispositions.md:1609` |
| El trabajo `tests` de la CI: `timeout-minutes: 20` | `.github/workflows/ci.yml:42` |

## Mediciones de la v4.1 (en `7f0d9aa3`)

Todo por objetos git (`git show`, `git ls-tree` y `git cat-file --batch`), con Python `-I`. El léxico nuevo (`lists_core.py`) vacía comentarios, cadenas (normales, verbatim, interpoladas y crudas) y caracteres antes de contar llaves, y un cuerpo de expresión acaba en el `;` de nivel cero. Las pruebas que ya llevan `Skip` no entran en ninguna lista.

## R-34. Las listas de lo elegido (I-22, I-23)

| Hecho | Fuente |
|---|---|
| PB-T4a: 1, `CT_PBT3A_12_NoSourceFileOutsideTheFolderOfTheModelNamesItsTypes` (`PushBackDraftWriteGuardTests.cs:60-95`); la rompe RIA, que nombra `PushBackFullDraftReader` | `pbt4-substitution-lists.md` |
| PB-T4b, C-0: 428 (las 9 clases: 69, 67, 62, 59, 54, 44, 35, 28 y 10) | ídem |
| PB-T4b, C-5 y C-6: 15 (CT-PBT4A-06, CT-PBT3B-13, G-4 ×2, EIG ×5, PEG ×5, RG ×1) | ídem |
| PB-T4b, C-3 de la UI: 27 (21 de `WorkspacePilotReconciliationTests` y 6 de `WorkspacePilotReconciliationViewTests`) | ídem |
| PB-T4b, C-7: 139 = 72 por texto (69 propias y E-3, E-5 y E-6) + 67 por compilación (R-36) | ídem |
| PB-T4b, total: 609; subtotal sin la retirada, 470 | ídem |
| PB-T4c: 2, PEG:155-163 y CT-PBT1B-15 (:306-403) | ídem |
| Con alguna firma de C-1 a C-6 o C-9 en las 9 clases: 237 (la v4 decía 242); sin ninguna, 191 (186) | `lists_pbt4-out.txt` |
| Comparable a las «99 propias» de Q1: 145 | `probe_overlap-out.txt` |

## R-35. El mapa de la copia de «Saltar y duplicar»

| Hecho | Fuente |
|---|---|
| Con M-01 a M-08, 375 de las 428 quedan sin nada que el mapa no cubra (copia mecánica) y 53 no: R-C4 29, R-C9 8, R-retirado 6, R-C3 5, R-C2 y R-C9 2, R-estado 1, R-retirado y R-estado 1, R-C2 1 | `copy_map_pbt4-out.txt`, `pbt4-copy-map.tsv` |
| Por clase, mecánicas / a mano: 69/0, 55/12, 57/5, 59/0, 54/0, 39/5, 12/23, 20/8 y 10/0 | ídem |
| Fuera de las pruebas, ningún resto que impida compilar las copias: solo `TryKeepMine` y `TryUpdatePilot` en dobles del puerto | ídem |
| Los tipos de primer nivel que se copian (M-00): `LifecycleScriptedAdvisor`; `PilotEditThrowingComparer`, `PilotEditThrowingProbe`, `PilotEditAuditedReader` y `PilotEditOutcomeProbe`; `CommandAdvisorPortDouble`; y las nueve clases. Los del kit, que no se copian: `EditIntentFakeReader`, `EditIntentKit` y `PrepareKit` | ídem |
| Los miembros del estado que usan las pruebas: `.Inputs.Annotations.AnnotationScale` (28), `.Inputs.PalletTolerance` (2), `.Inputs.Annotations` (1) y `state.Editor`/`state.Inputs` (3); en el estado completo, `Editable.AnnotationScale` y `Editable.PalletTolerance` (`PushBackEditableState.cs:64-66`) | `git show` |

## R-36. La retirada por compilación (Q1 «Retirarlo»)

| Hecho | Fuente |
|---|---|
| 179 pruebas sin `Skip` alcanzan lo retirado por referencia de código transitiva: 55 en las 9 clases, 57 ya en las listas y **67 nuevas** (`WorkspacePilotUpdateRunTests` 42, `WorkspaceDraftPilotOwnershipTests` 8, `WorkspaceDraftPilotStateTests` 7, `PushBackFullWritePrepareTests` 6, `PushBackDeclaredCorrectionPilotParityTests` 2, `PushBackFullWriteRunTests` 1 y `WorkspaceDraftPilotMaterializationTests` 1) | `retire_compile_pbt4-out.txt`, `pbt4-retire-compile.tsv` |
| 29 miembros de apoyo nombran lo retirado; entre ellos `PilotUpdateSetup.Run` y `ExpectedPlan` (`WorkspacePilotUpdateScopeDouble.cs:266-277`), `PilotoEscribe` y `Paridad` (`PushBackDeclaredCorrectionSupport.cs:128-149`), los campos `Comparer` (`WorkspaceDraftPilotMaterializationTests.cs:20`, `WorkspaceDraftPilotStateTests.cs:661`), `OwnershipKits.Pilot` (`WorkspaceDraftPilotOwnershipTests.cs:58-68`) y `Key = PushBackPilotFieldAdapter.AnnotationScaleKey` (`WorkspacePilotEditHostTests.cs:65`) | ídem |
| Las 55 de las 9 clases: `WorkspaceEditIntentHostTests` 2, `WorkspacePilotEditHostTests` 16, `WorkspacePilotPrepareTests` 30 y `WorkspacePilotReconciliationHostTests` 7 | ídem |

## R-37. Las guardas de PB-T4a sobre el texto literal del GREEN (dudas 4, 5, 7 y 8)

| Hecho | Fuente |
|---|---|
| EIG:139-141 fija la firma entera de `RackPilotIntentAdapter.Read`, hasta `RackCatalog catalog\s*\)`; un parámetro nuevo, obligatorio o con valor por defecto, la rompe (contrafactual, 2 de 2) | `measure_pbt4_v41-out.txt`; EIG:132-147 |
| Ninguna guarda prohíbe parámetros opcionales en el Plugin | `git grep` |
| NH no tiene `#nullable`; RIA sí (`RackPilotIntentAdapter.cs:1`) | `git show` |
| Con el texto literal del GREEN (WHC, NH y RIA), 32 de 32 aserciones medidas en verde: EIG, WLG, PEG, RG, CAG, G-9, la reflexión sobre WHC y las cuentas de NH | `measure_pbt4_v41-out.txt` |
| CAG fija 5 motivos y 2 `ex.Message` en `Read` de RIA, y 3 motivos en `ReadOnActiveDocument` de NH; no exige nada fuera de esos cuerpos | CAG:406-464 |
| Los cinco cuerpos que releen tienen `var draft = _drafts.Find<PushBackPilotDraftState>(request.Session, request.RackId);` antes de la lectura | WHC:403, :498, :583, :667, :754 |
| `PushBackPilotIntentScan` pide `IReadOnlyDictionary<string, string?>` y expone `IReadOnlyDictionary<string, string>`: de ahí la copia de `AtKey`, como UR | `PushBackPilotIntentScan.cs:29-35, :92` |

## R-38. El contrato de PB-T3c, del que copia el §6

| Hecho | Fuente |
|---|---|
| 54 162 bytes; 12 invariantes; 36 `RequiredTests` (`PushBackDraftWriteGuardTests` con `MinSelected` 3; `PushBackViewEraseGuardTests` con `ExpectRed` true); 15 `ExpectedEvidence`; 33 `StopConditions`, 17 estándar | `pbt3c-contract-dump.txt` |
| La CI del GREEN de PB-T3c: `44757675821` sobre `938a35f0`, Core 19 129 = 19 123 + 0 + 6, UI 2 451 = 2 430 + 21 | `f4b-dispositions.md`, F4B-E48 |
| CT-PBT3A-12 es un `[Fact]` (:60); su `Skip` deja 2 ejecutables en `PushBackDraftWriteGuardTests` | `PushBackDraftWriteGuardTests.cs` |

## Mediciones de la v4.2 (en `7f0d9aa3`)

## R-39. B-08: las cuentas de RIA en todo el archivo

| Hecho | Fuente |
|---|---|
| `TheReadBuildsTheInputFromTheScanAndTheEvidenceOnceAndAWitnessWithoutKeyFailsBeforeAnyInputIsBuilt` exige en **todo** RIA (`Adapter()`) un `new PushBackPilotIntentScan(` y un `PushBackPilotIntentReader.Read(`, y el orden de `Read` | EIG:350-364 (:356-357) |
| EIG:286 prohíbe `throw` en todo RIA; EIG:292-301, las API de `AdapterWrites`, un `RackBlockData.Read(` y ningún `GetObject(` | EIG:269-301 |
| Con `ReadAt` en `Draft/`, el GREEN de RIA tiene uno de cada (en `Read`) y ningún `throw`: 35 de 35 aserciones traducidas en verde, y las dos contrafactuales de la firma en rojo | `measure_pbt4_v42-out.txt` |
| `PushBackFullDraftReader.cs` ya llama a `PushBackPilotIntentReader.Read` desde `Draft/` | `Draft/PushBackFullDraftReader.cs:56` |

## R-40. El recorrido entero de las guardas de PB-T4a (I-25)

| Hecho | Fuente |
|---|---|
| 46 clases de prueba nombran WHC, WEI, NH o RIA, sus carpetas o los ayudantes de carpeta; tienen 1 026 pruebas sin `Skip` | `pbt4a-guard-walk-out.txt` |
| 764 no leen ninguno de los archivos tocados; 162 los leen sin ninguna diferencia de un literal con nombre ni ningún cuerpo que cambie; 100 quedan señaladas (67 y 33) | ídem; `pbt4a-guard-walk.tsv` |
| De las 100, leídas una a una, cae CT-PBT3A-12 (la de la lista); las otras 99 siguen en verde, por los motivos de §4.1 del paquete | `pbt4a-guard-walk-flagged.txt` |
| Los cuerpos que cambian: en WHC, los cinco que releen; en WEI, el constructor, `FromPilot`, `FromReads` y las cuatro fábricas de fallo; en NH, `ReadIntent`, `ReadIntentAtOrigin` y `ReadOnActiveDocument`; en RIA, `Read` (solo la línea del éxito) y `AtOrigin` | `pbt4a-guard-walk-out.txt` |

## R-41. Las listas tras la decisión del Owner, y el léxico

| Hecho | Fuente |
|---|---|
| `pbt4b-substitution-list.tsv`: las 470 filas de PB-T4b sin C-7, sha256 `44393b19d1f7490081c8ec9bac633ea4acbfe792103e5e5c3ca5e1f3fa1163e4`, 103 000 bytes | `split_lists_pbt4-out.txt` |
| `pbt4d-retire-list.tsv`: 139 de C-7, 55 originales y 29 miembros de apoyo, sha256 `1c9d67e8f2dac7eae3202d61a23ff5131e96123e5a6af275e5ce8a8bf71b8b71`, 55 225 bytes | ídem |
| `lists_core.py` tomaba `@"""…` por una cadena cruda; tres archivos de las listas las tienen (EIG, RG y `WorkspacePilotUpdateGuardTests`). Corregido, las listas regeneradas son iguales: `pbt4-substitution-lists.tsv` `f24c2dbc…`, `.md` `0f4c9bad…`, `pbt4-copy-map.tsv` `1091ae50…` y `pbt4-retire-compile.tsv` `e7246dc1…` | `sha256sum` |
| De las 67 por compilación, `WorkspacePilotUpdateRunTests` 42 y `PushBackFullWritePrepareTests` 6; en C-7, 48 y 9 | `pbt4d-retire-list.tsv` |

## Mediciones de la v4.3 (en `7f0d9aa3`)

## R-42. El contexto de nulabilidad y CA-T4a I-01

| Hecho | Fuente |
|---|---|
| `<Nullable>disable</Nullable>` para todos los proyectos, con el comentario de la política (un proyecto nuevo que quiera tipos de referencia anulables lo activa en su `.csproj`) | `Directory.Build.props:25-28` |
| Ningún `.csproj` lo cambia: `src/RackCad.Application`, `src/RackCad.Domain`, `src/RackCad.Plugin`, `src/RackCad.UI`, `tests/RackCad.Tests`, `tests/RackCad.UI.Tests` y las dos herramientas de `tools/` lo heredan | `git show` de cada `.csproj` |
| Llevan `#nullable enable` en su primera línea: WEI, WHC, RIA, `IWorkspaceEditIntentReader.cs` y `WorkspacePilotSupport.cs`; no lo llevan NH, WCo, FVM, `NumericFieldValidation.cs` ni ningún archivo de `Draft/` (su pureza lo prohíbe) | ídem |
| El esqueleto de la v4.2 daba `PushBackOriginByText.cs(16,29): warning CS8632` en los registros del Plugin, de Core y de UI de la compilación de la sesión; `build-plugin` falla con cualquier `warning CS` | CA-T4a §2 y §6; `eng/ci/verify-autocad-references.ps1` |
| Con la v4.3, la regla nueva no ve ningún `?` de tipo de referencia en lo que el GREEN añade a NH ni a `Draft/PushBackOriginByText.cs`, y sí ve los dos del texto de la v4.2 (contrafactual) | `measure_pbt4_v43-out.txt` |

## R-43. El recorrido de fuera de las marcas (CA-T4a M-01) y el GREEN aplicado

| Hecho | Fuente |
|---|---|
| Las marcas del recorrido (`MARKERS` de `walk_guards_pbt4a.py`) no incluyen `src`, `src/RackCad.Application` ni `src/RackCad.Plugin`, ni ven una ruta compuesta con `Path.Combine` | `walk_guards_pbt4a.py` |
| 146 archivos de prueba fuera del recorrido nombran `src/RackCad.(Application\|Plugin)`, `"src"` o un ayudante de barrido; 97 nombran como literal una carpeta antecesora de un archivo tocado o nuevo | `cat4a_walk_gap-out.txt` (sha256 `2605b5ca…70fb3`) |
| Sus literales, contra las 134 líneas que el GREEN añade o cambia y los dos archivos nuevos: solo fichas genéricas; los que leen de verdad los archivos tocados, leídos: ninguno cae | `cat4a_added_lines-all-out.txt` (`bfb4a5da…952e`); CA-T4a §5 |
| El recorrido, vuelto a correr sobre el GREEN de la v4.3: 46 clases, 1 026 pruebas, 764 / 162 / 100, y las 100 señaladas, las mismas que en la v4.2 | `pbt4a-guard-walk-out.txt`; `pbt4a-guard-walk-flagged-v43.txt` |
| `apply_green_pbt4a.py`, sobre una copia mínima: en `--dry` no escribe; de verdad, escribe los seis archivos, iguales al GREEN y con LF; una segunda corrida para (los archivos ya no son los de la revisión), y una copia con `.git` también | `selftest_apply_green-out.txt` |
| Los sha256 del `src/` del GREEN literal de PB-T4a (v4.3): `Draft/PushBackOriginByText.cs` `63084fae…7537`, `IWorkspaceEditIntentOriginReader.cs` `becb8d69…84ca`, WEI `6be957c3…5285`, WHC `63943612…3431`, RIA `658342fa…154d`, NH `915b560b…884e`; el conjunto, `7d29d396d8181e4ae83c5ec2693ed20e6f71b2846938eb07816967d33f4586cd` | ídem |
