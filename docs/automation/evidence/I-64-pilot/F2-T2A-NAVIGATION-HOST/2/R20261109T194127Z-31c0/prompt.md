I-64 - DELEGACION F2-T2A-NAVIGATION-HOST - Worker, perfil ROUTINE_IMPLEMENTATION (correccion del intento 1, CorrectionOf R20261109T184844Z-3f3e; una sola invocacion del Worker para esta delegacion)
Identidad: unidad I-64; gate F2; tarea F2-T2A-NAVIGATION-HOST; intento 2; RunId R20261109T194127Z-31c0;
           DelegationRunId R20261109T193332Z-bafe; AuthorityRevision 09a1499a46d16c8251ac4575d370b551603b6372;
           MainSha a140eb3a9f055518f0ffa5a3f927ff1c0e1c1e60; BaseSha 53302b85518a8d0eaaa07cf1af5affc97581aa95;
           rama architecture/workspace-persistente-rackcad;
           worktree <worktree-de-I-64>;
           Owner subagent:R20261109T193332Z-bafe
Autoridades (leer desde Git, no copiar; ruta | seccion | clase):
           - docs/initiatives/I-64-proposal-v5.md | D-01, D-02, D-03, D-04, D-05, D-07; invariantes INV-02, INV-03, INV-14, INV-22, INV-36, INV-38 e INV-40; section 11 F2; section 12.3 Smoke-NAV; section 13 routing de F2 | UNIT_DOC
           - docs/automation/state/I-64.yml | estado vigente completo; en especial f2_t1_status CLOSED_PASS, f2_t2_status y persistent_next_action | UNIT_DOC
           - docs/automation/evidence/I-64-pilot/F2-T1-NAVIGATION-CORE/coordinator/coordinator-f2t1-final-close.md | archivo completo: cierre de F2-T1, restricciones D-05/D-07 1-12 de F2-T2 y direccion de producto no modal | UNIT_DOC
           - docs/initiatives/I-64-f2-t2-gate-contract-package.md | hallazgos H-01..H-14, alcance, invariantes y decisiones C-F2T2-01..13 (resueltas por el Coordinator e incorporadas a este contrato) | UNIT_DOC
           - src/RackCad.Application/Workspace/NavigationTargets.cs | producto de F2-T1 (c726410f): NavigationTargets, Revalidate, ApplyDrain, InvalidateSession, EndSession; solo lectura | UNIT_CHANGE
           - src/RackCad.Application/Workspace/NavigationActions.cs | producto de F2-T1 (c726410f): NavigationActionPolicy (Plan, Decide, Execute) e INavigationEffects; solo lectura | UNIT_CHANGE
           - src/RackCad.Application/Workspace/NavigationFacts.cs | producto de F2-T1 (c726410f): NavigationDefinitionFact e INavigationFactsPort; solo lectura | UNIT_CHANGE
           - docs/AUTOMATION_PLAN.md | section 16.1 a section 16.8 | EXTERNAL
           - docs/automation/agent-execution/README.md | section 4 (A1-A8) y section 10 (nc1..nc4) | EXTERNAL
           - docs/automation/agent-execution/routing.md | archivo completo | EXTERNAL
           - docs/automation/agent-execution/model-catalog.md | celdas medidas y vigentes para Controller y Worker | EXTERNAL
           - docs/WORKFLOW.md | relevo, Git, rebase/integracion y prohibicion de integracion automatica | EXTERNAL
           vigencia: AuthorityRevision para la unidad (UNIT_DOC), MainSha para lo demas (EXTERNAL)
           Los valores de seccion son ASCII (sin diacriticos ni signo de seccion); en Git los encabezados conservan sus tildes y signos.
Objetivo (texto literal del contrato):
           Implementar la primera de las dos tareas secuenciales de F2-T2 (division aceptada por el Coordinator, C-F2T2-01): conectar el nucleo de navegacion verificado de F2-T1 (NavigationTargets, NavigationActionPolicy, INavigationFactsPort e INavigationEffects; producto c726410f) con el host AutoCAD, sin controles WPF: un puerto nuevo IWorkspaceNavigationPort en Application, sin tipos de AutoCAD y con resultado inmutable; una memoria pura de eco por contenido NavigationEcho; el adaptador de hechos WorkspaceNavigationFacts sobre RackBlockFinder.ScanEnvelopes sin cambiarlo; el ejecutor de efectos WorkspaceNavigationEffects (SELECT, VIEW y CENTER; LOCATE VIEW por la politica); y el host WorkspaceNavigationHost, que ejecuta cada accion en contexto de comando con DocumentCollection.ExecuteInCommandContextAsync, revalida dentro del callback (documento activo, sesion, existencia y atribucion), aplica los efectos, captura el resultado, devuelve el foco con API publica y purga perezosamente el estado de las sesiones que ya no viven; mas los deltas exactos de las guardas por archivo exacto. Los controles WPF de navegacion son F2-T2B; el comportamiento real en AutoCAD lo comprueba Smoke-NAV.
Alcance: permitido EXACTAMENTE estas 15 rutas (ninguna otra, sin prefijos):
           - src/RackCad.Application/Workspace/IWorkspaceNavigationPort.cs
           - src/RackCad.Application/Workspace/NavigationEcho.cs
           - src/RackCad.Plugin/Workspace/WorkspaceNavigationHost.cs
           - src/RackCad.Plugin/Workspace/WorkspaceNavigationFacts.cs
           - src/RackCad.Plugin/Workspace/WorkspaceNavigationEffects.cs
           - tests/RackCad.Tests/Workspace/WorkspaceNavigationHostTests.cs
           - tests/RackCad.Tests/Workspace/WorkspaceNavigationEchoTests.cs
           - src/RackCad.Plugin/Workspace/WorkspaceController.cs
           - src/RackCad.Plugin/Workspace/WorkspaceSelectionReader.cs
           - src/RackCad.Application/Workspace/WorkspaceHostCore.cs
           - tests/RackCad.Tests/Workspace/WorkspaceBridgeSourceGuardTests.cs
           - tests/RackCad.Tests/Workspace/WorkspaceSourceGuardExceptionTests.cs
           - tests/RackCad.Tests/Workspace/WorkspaceHostSourceGuardTests.cs
           - tests/RackCad.Tests/Workspace/WorkspacePaletteHostSourceGuardTests.cs
           - tests/RackCad.Tests/Workspace/WorkspaceNavigationAuthorityTests.cs
           prohibido src/RackCad.Application/Workspace/NavigationFacts.cs, src/RackCad.Application/Workspace/NavigationTargetSet.cs, src/RackCad.Application/Workspace/NavigationTargets.cs, src/RackCad.Application/Workspace/NavigationActions.cs, src/RackCad.Application/Workspace/EventBridgeCore.cs, src/RackCad.Application/Workspace/IWorkspaceHostPort.cs, src/RackCad.Plugin/Workspace/AutoCadEventBridge.cs, src/RackCad.Plugin/Workspace/WorkspaceHostPort.cs, src/RackCad.Plugin/Workspace/WorkspacePanelCommand.cs, src/RackCad.Plugin/Workspace/WorkspacePaletteHost.cs, src/RackCad.Plugin/RackBlockFinder.cs, src/RackCad.Plugin/RackInventarioCommands.cs, src/RackCad.Plugin/RackInventarioCommands.BomTotal.cs, src/RackCad.Plugin/PluginInitializer.cs, src/RackCad.Plugin/RackCad.Plugin.csproj, src/RackCad.Plugin/Drawing/, src/RackCad.Plugin/KindHandlers/, src/RackCad.Plugin/Systems/, src/RackCad.Plugin/Views/, src/RackCad.Application/Bom/, src/RackCad.Application/Catalogs/, src/RackCad.Application/ComputedParameters/, src/RackCad.Application/CustomProperties/, src/RackCad.Application/Diagnostics/, src/RackCad.Application/Drawing/, src/RackCad.Application/Expressions/, src/RackCad.Application/Formatting/, src/RackCad.Application/Geometry/, src/RackCad.Application/Layout/, src/RackCad.Application/Persistence/, src/RackCad.Application/ProjectVariables/, src/RackCad.Application/RackFrames/, src/RackCad.Application/Settings/, src/RackCad.Application/StructuralAnalysis/, src/RackCad.Application/StructuralSections/, src/RackCad.Application/Systems/, src/RackCad.Application/Units/, src/RackCad.Application/Views/, src/RackCad.Application/BlockNaming.cs, src/RackCad.Application/RackCad.Application.csproj, src/RackCad.Domain/, src/RackCad.UI/, tests/RackCad.UI.Tests/, tests/RackCad.Tests/StructuralAnalysis/, docs/, .github/, eng/, tools/, assets/, deploy/, RackCad.sln, Directory.Build.props, Directory.Build.targets, global.json, NuGet.Config, README.md.
Invariantes del Freeze (vinculantes en su totalidad):
           - INV-F2T2A-01 (D-05, INV-38): NavigationTargets (F2-T1, sin cambio) es la unica autoridad de objetivos de navegacion; src/RackCad.Plugin/Workspace/WorkspaceNavigationHost.cs instancia exactamente un NavigationTargets y un NavigationActionPolicy sobre el puerto de hechos de WorkspaceNavigationFacts.cs; ningun archivo del Plugin ni de la UI deriva objetivos, ordena referencias ni decide la atribucion por su cuenta.
           - INV-F2T2A-02 (D-07, INV-40, D-03, INV-22; C-F2T2-02): la unica ejecucion asincrona autorizada es la ruta publica de AutoCAD DocumentCollection.ExecuteInCommandContextAsync en src/RackCad.Plugin/Workspace/WorkspaceNavigationHost.cs; async es un mecanismo de contexto de ejecucion, no trabajo en segundo plano. El callback de contexto de comando contiene la validacion decisiva y los efectos, en este orden: (1) documento activo, (2) SessionId, (3) existencia de los objetivos, (4) atribucion de los objetivos, (5) NavigationActionPolicy.Decide, (6) efectos, (7) captura del resultado, (8) vuelta del foco. Toda lectura y todo efecto de AutoCAD de la accion ocurren dentro del contexto de comando. Ninguna accion se ejecuta despues de que la SessionId quede obsoleta, cambie el Document activo, desaparezca un objetivo o cambie su atribucion: un veredicto distinto de Act da cero efectos, invalida la cache de ese rack y pide refresco.
           - INV-F2T2A-03 (C-F2T2-02): prohibidos en todo archivo: .Wait(), .Result sobre una Task, GetAwaiter().GetResult(), cualquier espera que bloquee el hilo de UI de AutoCAD, una Task desligada o no observada (fire-and-forget), Task.Run, Task.Factory.StartNew, Thread, Timer, BackgroundWorker y ConfigureAwait(false) en la ruta de contexto de comando. async y await solo en WorkspaceNavigationHost.cs y solo para el patron aprobado. Ningun [CommandMethod] nuevo, comando oculto, comando nuevo de AutoCAD ni SendStringToExecute; RACKPANEL sigue siendo el unico comando del panel.
           - INV-F2T2A-04 (C-F2T2-03, D-01): la vuelta del foco al dibujo vive en un unico metodo aislado de WorkspaceNavigationHost.cs, solo con API publica y soportada de AutoCAD o de ventana; prohibidos en T2A Autodesk.AutoCAD.Internal, DllImport, trucos de foco Win32 y HwndSource. Degrada con seguridad: un fallo de foco se contiene y no causa ninguna mutacion del DWG ni una segunda accion de navegacion. Su eficacia real la demuestra Smoke-NAV SN-07; si alli falla, STOP tras la evidencia, sin introducir Autodesk.AutoCAD.Internal.
           - INV-F2T2A-05 (C-F2T2-04, D-05): src/RackCad.Plugin/Workspace/WorkspaceNavigationFacts.cs implementa INavigationFactsPort: consume RackBlockFinder.ScanEnvelopes sin cambiar RackBlockFinder, inspecciona solo las referencias de bloque directas de nivel superior de Model Space para obtener los handles que pide NavigationDefinitionFact y clasifica solo segun el contrato de la autoridad de F2-T1. No crea otra implementacion de NavigationTargets, no hace inventario global, no cambia RackBlockFinder.cs ni RackInventarioCommands.cs, no persiste handles y no retiene DBObject en campos; transacciones solo dentro de using.
           - INV-F2T2A-06 (D-07): src/RackCad.Plugin/Workspace/WorkspaceNavigationEffects.cs implementa INavigationEffects. SelectReferences: seleccion implicita con todas las referencias pedidas, copias incluidas, solo en Model Space; fuera de Model Space la accion no esta disponible, el resultado lo explica y no se cambia TILEMODE ni el layout. ViewReference: extension de la referencia con margen del 10 por ciento, como ZoomToRack (planta XY). CenterOnReference: mueve el centro de la vista sin cambiar su escala. LOCATE VIEW se resuelve en NavigationActionPolicy.Plan. Ninguna escritura del authored, Xrecord, XData, diccionarios, variables de sistema ni archivos, y ninguna otra mutacion de la vista.
           - INV-F2T2A-07 (C-F2T2-05, INV-14): src/RackCad.Application/Workspace/IWorkspaceNavigationPort.cs declara un puerto nuevo sin tipos de AutoCAD; IWorkspaceHostPort, WorkspaceHostPort y sus guardas no cambian. La peticion lleva la SessionId (WorkspaceRequest), el RackId y lo que pida la politica, la accion explicita y la copia y la vista explicitas cuando aplican. Puede exponer un contrato asincrono solo de .NET (por ejemplo Task<WorkspaceNavigationResult>) u otro contrato equivalente de resultado inmutable; la API exacta la propone la planificacion. El resultado es inmutable y distingue ejecutado, no disponible, rechazado, obsoleto y fallido; ningun flujo normal de la UI depende de excepciones; no se anade un bus de eventos para devolver el resultado.
           - INV-F2T2A-08 (C-F2T2-06, INV-03 completo, D-07): src/RackCad.Application/Workspace/NavigationEcho.cs es una memoria pura de eco por SessionId. Para SELECT guarda el conjunto de handles que escribio RackCad; en el drenaje normal compara como CONJUNTO el conjunto de handles de la seleccion implicita actual; si es igual, el evento es eco de navegacion y no avanza el contexto, no cambia de rack ni de pestana y no dispara otra accion de navegacion; la primera seleccion distinta reanuda la sincronizacion normal. Prohibidos el token del siguiente evento, un booleano ignoreNextSelection, ventanas de tiempo y temporizadores. El estado se borra cuando la sesion ya no vive. Ningun evento ni cambio de contexto escribe seleccion o vista.
           - INV-F2T2A-09 (C-F2T2-07): sin aviso nuevo del puente ni ampliacion de su superficie publica. En cada drenaje relacionado con la navegacion y justo antes de ejecutar una accion, el host reconcilia el estado de navegacion con la autoridad de sesiones; si TryGetSession(SessionId) falla, purga la cache de NavigationTargets de esa sesion, su estado de NavigationEcho y el estado pendiente de navegacion del host. Una SessionId purgada nunca se revive desde estado obsoleto de la UI.
           - INV-F2T2A-10 (C-F2T2-08): excepciones de guardas solo por archivo exacto; nunca por carpeta, prefijo de ruta, comodin, "todos los archivos de navegacion" ni redaccion equivalente. Cada archivo privilegiado nuevo de src/RackCad.Plugin/Workspace/ se nombra en tests/RackCad.Tests/Workspace/WorkspaceSourceGuardExceptionTests.cs. La excepcion asincrona es solo para WorkspaceNavigationHost.cs y solo para el patron de contexto de comando aprobado, y la guarda de archivo exacto la codifica. Los deltas de guardas van en el commit RED junto con las pruebas nuevas que fallan, nunca como relajacion en GREEN. Las metaguardas (seis archivos de src/RackCad.Plugin/Workspace/, 43 [Fact]/[Theory] de WorkspaceHostSourceGuardTests.cs, 3 += del controlador y demas recuentos exactos) se actualizan de forma exacta y deliberada.
           - INV-F2T2A-11 (INV-14): StartTransaction, StartOpenCloseTransaction y LockDocument solo dentro de using; ningun campo de tipo Transaction, DBObject, Document, Database, Entity, BlockReference, ObjectId ni Editor; los tipos de Application que cruzan al Plugin no contienen tipos de AutoCAD.
           - INV-F2T2A-12 (D-04, INV-02): ninguna suscripcion nueva a eventos de AutoCAD; AutoCadEventBridge.cs sigue siendo el unico suscriptor, sin cambios, y su superficie publica no cambia; los manejadores siguen sin escribir seleccion ni vista.
           - INV-F2T2A-13 (D-05, INV-36): NavigationTargets y sus hechos siguen privados del panel; WorkspaceNavigationPrivacyGuardTests.cs sigue en verde y sin cambios.
           - INV-F2T2A-14 (alcance, C-F2T2-13): solo cambian las rutas del AllowedWriteScope de la delegacion: los siete archivos nuevos y la lista explicita de archivos existentes que la planificacion enumera como subconjunto exacto de los candidatos de este contrato. No cambian NavigationFacts.cs, NavigationTargetSet.cs, NavigationTargets.cs, NavigationActions.cs, EventBridgeCore.cs, IWorkspaceHostPort.cs, AutoCadEventBridge.cs, WorkspaceHostPort.cs, WorkspacePanelCommand.cs, WorkspacePaletteHost.cs, RackBlockFinder.cs ni RackInventarioCommands.cs. Si el diseno exige cualquier otro archivo existente (por ejemplo WorkspaceSelectionRead.cs, WorkspaceSession.cs o un archivo de soporte de pruebas), STOP P-03.
           - INV-F2T2A-15 (fuera de alcance): sin controles WPF de navegacion (F2-T2B), sin F3, F4 ni F6, sin migracion de editores, sin sistemas nuevos y sin inventario global; RACKEDITAR, RACKPANEL, RACKRAMPAQUETE y los comandos clasicos sin cambio observable; los censos de comandos y de ventanas no cambian.
           - INV-F2T2A-16 (cobertura RED/GREEN): RED de comportamiento real que compila y falla por asercion, sin pruebas artificiales: eco por contenido con un contexto previo distinto (cero llamadas de seleccion o zoom y contexto sin cambio), puerto y resultado inmutable con rechazo por sesion y por revalidacion en un puerto de efectos simulado, purga de sesiones que ya no viven y las guardas de las excepciones exactas y del patron asincrono. Las 656 pruebas existentes del filtro Core siguen en verde salvo los deltas de guardas autorizados del RED; las 64 del filtro UI siguen en verde y sin cambios; el GREEN no toca los archivos de prueba del RED.
Celda: anthropic/subagent/claude-sonnet-5-5; modelo claude-sonnet-5-5; effort high (Deep); clase "Implementacion transversal a capas (corta)"; RoutingEnforcement required
Criterios de aceptacion (del paquete aceptado):
           - git diff --name-only BaseSha..CurrentSha contains only paths in this delegation AllowedWriteScope; every F2-T1 file and every forbidden path is unchanged.
           - Create and push a NEW RED commit first: this delivery touches ChainRedFiles, so it requires a new RED under 16.8. The new RED contains every test change of this delivery; product files stay the non-implementing skeletons present at BaseSha (signature adjustments only if a corrected test needs them to compile). The Core filter compiles and fails by assertion for intended missing behavior, not by compilation failure; no failing test may be impossible to satisfy under the authorized GREEN and the accepted scope; preserve the existing baseline except for the intended failing tests and exact authorized guard deltas. The GREEN commit does not modify any file of RT or ChainRedFiles.
           - Implement IWorkspaceNavigationPort with this exact API: Task<WorkspaceNavigationResult> ExecuteAsync(WorkspaceRequest origin, string rackId, NavigationActionKind action, int copyIndex = 0, NavigationViewOrder? view = null, NavigationActionKind locateWith = NavigationActionKind.View). Define WorkspaceNavigationResult as an immutable readonly record struct with statuses Executed, Unavailable, Rejected, Stale and Failed; normal rejection and failure outcomes are values, not exception flow. The request carries the SessionId, rack or context, explicit action, and explicit copy and view inputs as applicable; Application contains no AutoCAD types.
           - NavigationEcho is pure, stores expected handles as a set per SessionId, compares the drained implied-selection handles by set equality, suppresses an equal echo without applying a changed context, allows the first different selection through, and deletes state when the session is purged. Share it between WorkspaceSelectionReader and WorkspaceHostCore; pass the read handle set through those permitted files without changing WorkspaceSelectionRead.cs or WorkspaceSession.cs. If this cannot be done without changing WorkspaceSelectionRead.cs, STOP P-03.
           - In WorkspaceNavigationHost.cs, use DocumentCollection.ExecuteInCommandContextAsync with observed async/await and no Wait, Result, GetAwaiter().GetResult(), Task.Run, Task.Factory.StartNew, Thread, Timer, BackgroundWorker, detached Task or ConfigureAwait(false). Inside its callback perform, in order: active document, SessionId, target existence, target attribution, NavigationActionPolicy.Decide, effects, result capture and public-API focus return. Keep all action-related AutoCAD reads and effects in that callback; return the result through the port with no event bus. Do not add SendStringToExecute or any hidden CommandMethod; the async/await exception is limited to WorkspaceNavigationHost.cs.
           - WorkspaceNavigationFacts.cs implements INavigationFactsPort using RackBlockFinder.ScanEnvelopes unchanged and reads only direct top-level Model Space block-reference handles; classify only by the F2-T1 authority contract. WorkspaceNavigationEffects.cs implements INavigationEffects for SELECT, VIEW and CENTER, with VIEW matching the 10 percent ZoomToRack framing and CENTER preserving scale. Keep transactions in using, retain no AutoCAD objects in fields, and isolate focus return in one host method using only public APIs. WorkspaceNavigationEffects.cs may perform READ-ONLY GetObject or equivalent access only on the already-selected target for VIEW/CENTER, and only for existence, type, geometric extents and information strictly necessary for the approved view/center effect. It is not a discovery authority: no Model Space enumeration, sibling discovery, NavigationTargets derivation, alternative-reference search, inventory building or authored rack-data discovery reads. Target identity comes from NavigationTargets/NavigationActionPlan and is revalidated before the effect. WorkspaceNavigationFacts.cs remains the exact-file authority for collecting facts and handles.
           - Before every navigation drain and immediately before every action, use TryGetSession to lazily purge dead-session NavigationTargets, NavigationEcho and pending host state; a purged SessionId is never revived.
           - In WorkspaceController.cs, lazily create and expose the navigation port/host and connect the shared echo without adding AutoCAD event subscriptions or changing the three existing bridge subscriptions.
           - In WorkspaceSelectionReader.cs, feed the complete implied-selection handle set to the shared NavigationEcho during the normal drain read; preserve its read-only transaction and lock lifetime and do not change WorkspaceSelectionRead.cs. If the echo cannot be resolved within the allowed files, STOP P-03.
           - In WorkspaceHostCore.cs, query the shared NavigationEcho after the reader returns and before applying the mapped SelectionContext; matching echoes do not advance context, rack or tab state.
           - In WorkspaceBridgeSourceGuardTests.cs, update TheBridgeNeverWritesSelectionOrViewNorReadsStoredDataNorAsksNorOpensDialogs so writes are permitted only in WorkspaceNavigationEffects.cs, reads only in the exact authorized reader/facts/effects files for their approved purposes; preserve all other forbidden calls and exact-file exceptions.
           - In WorkspaceSourceGuardExceptionTests.cs, add exact-path exception assertions for WorkspaceNavigationHost.cs, WorkspaceNavigationFacts.cs and WorkspaceNavigationEffects.cs; the async exception applies only to WorkspaceNavigationHost.cs and only to the approved command-context pattern. Record the exact-file READ exception for WorkspaceNavigationEffects.cs and distinguish READ, DISCOVERY and WRITE.
           - In WorkspaceHostSourceGuardTests.cs, update NoHostFileWritesTheSelectionTheViewOrAnySystemVariable and NoHostFileStartsABackgroundThreadOrAsyncWork for the exact authorized file exceptions; keep every other host file restricted, add the approved async-pattern prohibitions, and update the exact Fact/Theory metacount deliberately. The exact-file read exception for WorkspaceNavigationEffects.cs permits only the minimum approved access to an already-selected target and includes a text guard preventing discovery there.
           - In WorkspacePaletteHostSourceGuardTests.cs, update TheFirstPlacementAddsNoPersistenceIdentityAutoloadProfileOrSecurityMechanism to the exact nine Plugin/Workspace files (six existing plus three new), and update TheRefinementsG1G2AndG3AreExactAndNothingWasRemovedFromTheT3AGuardFile to the exact revised guard metacount; preserve the three controller += invariant.
           - In WorkspaceNavigationAuthorityTests.cs, update OnlyTheAuthorityTraversesTheReferenceHandlesOfTheFacts for the exact new WorkspaceNavigationFacts.cs reader exception and extend NoEventSideFileCanReachTheNavigationEffectsOrThePolicy so the host policy/effects remain outside every event-side file. Update TheReaderApisAreUsedInNoOtherFileOfThePluginWorkspace to record the exact WorkspaceNavigationEffects.cs READ exception and include a guard against discovery in that file.
           - Keep the two required filters exactly as contracted and report selected counts: Core FullyQualifiedName~RackCad.Tests.Workspace (at least 656) and UI FullyQualifiedName~RackCad.UI.Tests.Workspace (at least 64). In the GREEN commit both filters and the complete Core and UI suites pass; Release builds of RackCad.Tests and RackCad.UI.Tests introduce no CS, NU or NETSDK warnings.
           - The exact CurrentSha push CI run has all four required jobs in success, including Build Plugin without AutoCAD; report the exact SHA, branch ref, event and job results.
           - Provide worker-handoff.json at ExpectedHandoffPath conforming to its schema, with BaseSha, CurrentSha, RED and GREEN SHAs, exact changed paths, actual scope, trailer, both filter counts, CI runs, routing/model evidence, guard-delta summary, known limitations, and an evidence-by-invariant map. Make no claim about AutoCAD beyond what CI, source and tests establish.
           - Do not add UI, AutoCAD execution, commands, F3/F4/F6 behavior or global inventory; do not modify any path outside AllowedWriteScope.
           - If WorkspaceHostC2SourceGuardTests.cs, WorkspaceBridgeFacadeSourceGuardTests.cs or WorkspaceSelectionRead.cs is needed, STOP P-03 and return to the Coordinator; do not add any of these paths.
           - Correct WorkspaceNavigationEchoTests.NothingChangedInTheSelectionReadOrTheSessionToCarryTheHandles minimally: the previous regular expression Echo|Navigation must no longer reject the unrelated existing symbol InvalidateNavigationCaches in WorkspaceSession.cs. The replacement proves the intended invariant directly: WorkspaceSelectionRead.cs remains unchanged and carries no echo or handle transport; WorkspaceSession.cs is not used to carry navigation echo handles and has no echo-specific state, property, field or transport; the approved echo mechanism remains NavigationEcho plus the exact authorized reader and host-core integration. Do not delete the invariant: the test must still detect an echo transport added to WorkspaceSession.cs, and it must be capable of passing under the authorized GREEN.
           - F2T2A-KL-01 is an accepted, diagnostic-only limitation: WorkspaceNavigationFacts may leave ProbedId null when the permitted integrated scan cannot expose the raw probe identity. Add a test in WorkspaceNavigationHostTests.cs proving that a fact with IsInterpretable false, EnvelopeId null and ProbedId null yields no NavigationTarget, and that target attribution and revalidation do not depend on ProbedId. Do not change RackBlockFinder, do not read RackBlockData or RackEmbedStore outside the authorized reader, do not add another authored-data reader and do not expand into global inventory. If ProbedId proves necessary for target attribution, navigation correctness or INV-40 safety, STOP and return to the Coordinator.
           - The saved Attempt 1 patch artifacts/orchestration/I-64/F2-T2A-NAVIGATION-HOST/1/R20261109T175557Z-a639/green-uncommitted-worktree.patch (SHA-256 32c19a574ccf85e513309ef62116a29876ebfcdf8796579043a0b1209e44c046) is reference and seed material only, not accepted GREEN evidence. The Worker must inspect it, decide what is still valid, apply only in-scope portions, correct it as needed, run the tests and create its own new RED and GREEN commits; the Worker remains responsible for the final implementation.
Evidencia requerida: pruebas (cada corrida filtrada demuestra seleccion > 0):
           - tests/RackCad.Tests/RackCad.Tests.csproj, filtro FullyQualifiedName~RackCad.Tests.Workspace, minimo 656, RED esperado: si
           - tests/RackCad.UI.Tests/RackCad.UI.Tests.csproj, filtro FullyQualifiedName~RackCad.UI.Tests.Workspace, minimo 64, RED esperado: no
           evidencia esperada del contrato: gate-contract valido; delegacion de la planificacion del Controller valida (sin Long-horizon), aceptacion A1-A8 y nc4 (primera delegacion de la cadena F2-T2A) antes de la aceptacion explicita del Coordinator (COORDINATOR_F2_T2A_ACCEPTANCE_REQUIRED); el Worker no se invoca antes de esa aceptacion.; AllowedWriteScope de la delegacion = los siete archivos nuevos del contrato + la lista explicita de archivos existentes que cambian (subconjunto exacto de los candidatos), con la razon de cada archivo existente en AcceptanceCriteria.; Commit RED y push propios antes del GREEN: pruebas nuevas en WorkspaceNavigationHostTests.cs y WorkspaceNavigationEchoTests.cs que compilan y fallan por asercion, mas los deltas exactos de las guardas existentes; ChainRedSha y ChainRedFiles acreditados.; Commit GREEN y push con cambios solo dentro del AllowedWriteScope de la delegacion; ningun archivo del RED se debilita o elimina.; Filtro Core FullyQualifiedName~RackCad.Tests.Workspace y filtro UI FullyQualifiedName~RackCad.UI.Tests.Workspace en verde sobre el GREEN; suites Core y UI completas en verde; builds Release de RackCad.Tests y RackCad.UI.Tests sin avisos nuevos CS/NU/NETSDK.; CI push del SHA exacto GREEN con los cuatro jobs en success, incluido Build Plugin without AutoCAD, que compila contra AutoCAD.NET 25.0.1 las llamadas a ExecuteInCommandContextAsync, SetImpliedSelection y la vista.; Guardas de archivo exacto que prueban: la excepcion asincrona solo en WorkspaceNavigationHost.cs y sin Wait, Result, GetResult, Task.Run, Task.Factory, Thread, Timer, BackgroundWorker ni ConfigureAwait(false); las APIs de escritura de seleccion y vista solo en WorkspaceNavigationEffects.cs; las APIs de lectura solo en WorkspaceNavigationFacts.cs y en el lector existente; ningun Autodesk.AutoCAD.Internal, DllImport ni HwndSource; ningun CommandMethod nuevo ni SendStringToExecute.; worker-handoff.json conforme al esquema con SHA RED, SHA GREEN, alcance real, trailer del Worker, conteos RED/GREEN de los dos filtros y mapa evidencia-por-invariante.; Controller verification valida; nc1..nc4 segun README section 10; A-4 y A-6 solo con sus condiciones aceptadas; A-11 NO disponible: si un defecto de protocolo del control de Tests agota nc3, STOP P-04, sin inferir puente.; El comportamiento real en AutoCAD (seleccion que sobrevive al comando interno, vista, centrado, localizar, foco, eco y aislamiento entre documentos) se comprueba solo en Smoke-NAV SN-01..SN-10 tras F2 PASS; el Worker no usa AutoCAD.
           La sesion, no tu, mantiene evidencia y estado (docs/ esta prohibido) y ejecuta la CI, la verificacion y los controles.
No-touch: todo lo prohibido arriba; el checkout principal <checkout-principal> (nunca escribas alli);
           las ramas de otras iniciativas y main; AutoCAD: no lo abras, no lo cierres, no cargues DLL, no toques perfil, TRUSTEDPATHS,
           SECURELOAD, ACL ni registro. Puede haber un acad.exe abierto por el Owner: no lo toques.
Condiciones de parada: las 27 del contrato (lista completa en artifacts/orchestration/I-64/F2-T2A-NAVIGATION-HOST/2/R20261109T193332Z-bafe/delegation.json), en especial:
           P-03: delegation outside gate contract
           F2T2-C01: a second navigation-target authority or own navigation discovery in Plugin or UI would be required
           F2T2-C02: an action would write authored data, layouts, TILEMODE or system variables, or perform an unrelated view mutation
           F2T2-C03: a guard exception by folder, path prefix or wildcard, or outside the exact authorized files, would be required
           F2T2-C04: command context or focus would need anything other than ExecuteInCommandContextAsync in WorkspaceNavigationHost.cs and public focus APIs (Autodesk.AutoCAD.Internal, DllImport, HwndSource, a hidden or new command, SendStringToExecute)
           F2T2-C05: global inventory or F3/F4/F6 behavior (tabs, drafts, editing, document-wide navigator) would be required
           F2T2-C06: the F2-T1 navigation types or the forbidden F1 files (bridge, host port, panel command, palette host, RackBlockFinder, RackInventarioCommands) would have to change
           F2T2-C07: the split task still evaluates Long-horizon
           F2T2A-C08: an existing file outside the candidate list of this contract would have to change (P-03; return to the Coordinator)
           F2T2A-C09: a blocking wait on a Task, a detached or unobserved Task, background work or ConfigureAwait(false) on the command-context path would be required
           F2T2A-C10: existing F1, C1, C2 or F2-T1 behavior regresses, or WorkspaceNavigationPrivacyGuardTests or the UI filter would need a change
           Ante cualquiera: parar sin continuar y devolver el bloqueo estructurado en la entrega (WorkerStatus BLOCKED, Disposition y
           TriggeredStopConditions). Una correccion exige una delegacion nueva: no se repara nada fuera de este paso.
Prohibido declarar: GATE PASS, Candidato, cierre, integracion o aprobacion del Owner;
           ningun termino de AUTOMATION_PLAN section 16 "Terminos de gate" (16.10) en el texto libre
Trailer: Co-Authored-By: Claude Sonnet 5.5 <noreply@anthropic.com> (quien ejecuta), nunca de otro participante
Informe esperado: la salida exacta del esquema docs/automation/agent-execution/schemas/worker-handoff.schema.json en
           artifacts/orchestration/I-64/F2-T2A-NAVIGATION-HOST/2/R20261109T194127Z-31c0/worker-handoff.json;
           Worker: commits y push antes de la entrega, y terminar tras escribirla

PERFIL ROUTINE_IMPLEMENTATION
Metodo: cambio minimo que cumple los criterios; nada de refactor oportunista.
1. Lee los archivos del alcance y las pruebas citadas antes de editar.
2. Si la entrega exige RED: commit RED primero (pruebas que fallan por asercion,
   seleccion > 0) y push; despues la implementacion.
3. Ejecuta las pruebas requeridas y las relevantes; anota comando, seleccion y resultado.
4. Commit GREEN con el resumen de estado en el cuerpo y push; luego la entrega.
5. Si algo exige salir del alcance: no lo hagas; parada y bloqueo estructurado.

DELTA DE LA TAREA (correccion del intento 1; delegacion aceptada: A1-A8 PASS y 20 condiciones mecanicas preautorizadas por el Coordinator; nc4 ya satisfecho para la cadena)
- BaseSha = 53302b85518a8d0eaaa07cf1af5affc97581aa95; ChainBaseSha = d9d386e91aa6df5db64fbb6946b7edb3542baace; ChainRedSha = 4fcea652d413414552d52f70998ce16f94f4c31b (RED acreditado del intento 1);
  ChainRedFiles = tests/RackCad.Tests/Workspace/WorkspaceBridgeSourceGuardTests.cs, tests/RackCad.Tests/Workspace/WorkspaceHostSourceGuardTests.cs, tests/RackCad.Tests/Workspace/WorkspaceNavigationAuthorityTests.cs, tests/RackCad.Tests/Workspace/WorkspaceNavigationEchoTests.cs, tests/RackCad.Tests/Workspace/WorkspaceNavigationHostTests.cs, tests/RackCad.Tests/Workspace/WorkspacePaletteHostSourceGuardTests.cs, tests/RackCad.Tests/Workspace/WorkspaceSourceGuardExceptionTests.cs.
  Esta entrega toca ChainRedFiles, asi que EXIGE un RED NUEVO (16.8). Attempt 2; AttemptsRemaining 1; CorrectionsAuthorized true; esta es la unica invocacion del Worker para esta delegacion:
  entrega lo correcto a la primera. El intento 3 no se consume automaticamente.
- ESTADO DE PARTIDA en BaseSha: ya existen los esqueletos compilables de los 2 archivos nuevos de Application y los 3 del Plugin, el parametro opcional NavigationEcho de WorkspaceHostCore.cs, las 2 pruebas nuevas y los
  deltas de las 5 guardas (RED 4fcea652). Filtro Core en CI: 755 seleccionadas, 709 pasan, 46 fallan; filtro UI 64/64. Sin GREEN. Analisis del intento 1 (leelo):
  docs/automation/evidence/I-64-pilot/F2-T2A-NAVIGATION-HOST/1/R20261109T184844Z-3f3e/analysis.md.
- CAUSA DEL INTENTO 1: WorkspaceNavigationEchoTests.NothingChangedInTheSelectionReadOrTheSessionToCarryTheHandles exigia que WorkspaceSession.cs no casara Echo|Navigation, y ese archivo (fuera de alcance) contiene
  DrainOperation.InvalidateNavigationCaches: la prueba no podia pasar. Los criterios corregidos estan arriba (RED nuevo, prueba corregida, F2T2A-KL-01 y semilla).
- SEMILLA (solo referencia; no es GREEN aceptado ni producto verificado): artifacts/orchestration/I-64/F2-T2A-NAVIGATION-HOST/1/R20261109T175557Z-a639/green-uncommitted-worktree.patch, SHA-256
  32c19a574ccf85e513309ef62116a29876ebfcdf8796579043a0b1209e44c046, producido contra 4fcea652 (src/ de BaseSha es identico). Toca solo los 8 archivos de producto del alcance. Debes inspeccionarla, decidir que sigue valido, usar solo
  porciones dentro del alcance y corregirla si hace falta; puedes aplicarla con git apply en el arbol de trabajo. La sesion no la ha aplicado. Tu respondes de la implementacion final.
- Entorno: Windows. Empieza cada orden de shell con cd al worktree por ruta absoluta (el directorio actual del shell
  puede volver al checkout principal) o usa git -C <worktree>. dotnet: usa SIEMPRE <perfil-de-usuario>/AppData/Local/Microsoft/dotnet/dotnet.exe (Git Bash) o
  <perfil-de-usuario>\AppData\Local\Microsoft\dotnet\dotnet.exe (PowerShell); el dotnet del PATH no sirve.
  Resultados de pruebas (--logger trx y --results-directory) y logs de build solo bajo artifacts/orchestration/I-64/F2-T2A-NAVIGATION-HOST/2/R20261109T194127Z-31c0/tests/<RunRef>/.
  Edita con la herramienta de edicion o de escritura, no con heredocs de shell.
- Antes de editar: git status limpio, git rev-parse HEAD = BaseSha, git ls-remote origin
  refs/heads/architecture/workspace-persistente-rackcad = BaseSha y git rev-parse origin/main (tras git fetch) = MainSha.
  Si no, parada (S-12/S-13).
- DISPOSICION DEL COORDINATOR (interpretaciones ya aceptadas del contrato, no amplian el alcance; literales en
  artifacts/orchestration/I-64/F2-T2A-NAVIGATION-HOST/coordinator/coordinator-f2t2a-acceptance.md y coordinator-f2t2a-basesha-recovery.md):
  1. WorkspaceNavigationEffects.cs may perform READ-ONLY GetObject / equivalent access on the ALREADY-SELECTED target for VIEW/CENTER.
  2. That read is only for: existence; type; geometric extents; information strictly necessary for the approved view/center effect.
  3. WorkspaceNavigationEffects.cs is NOT a discovery authority: no enumera Model Space, no descubre hermanos, no deriva NavigationTargets,
     no busca referencias alternativas, no construye inventario y no lee datos authored del rack para descubrir objetivos. La identidad del
     objetivo viene de NavigationTargets / NavigationActionPlan y se revalida antes del efecto.
  4. WorkspaceNavigationFacts.cs sigue siendo la autoridad de archivo exacto para recoger hechos y handles.
  5. WorkspaceHostC2SourceGuardTests.cs, WorkspaceBridgeFacadeSourceGuardTests.cs y WorkspaceSelectionRead.cs estan FUERA del alcance.
     Si alguno resulta necesario: STOP P-03, sin anadirlo.
  6. La excepcion asincrona (ExecuteInCommandContextAsync con async/await) se limita a WorkspaceNavigationHost.cs.
  7. WRITE se limita a la seleccion implicita y al estado de vista de VIEW, CENTER y LOCATE: sin mutacion del authored, de variables de sistema,
     del layout ni de TILEMODE. La excepcion de guarda de WorkspaceNavigationEffects.cs distingue READ de DISCOVERY y de WRITE y es de archivo
     exacto; nunca se debilita la guarda para toda la carpeta Plugin/Workspace.
  8. Prohibidos ademas: .Wait(), .Result, GetAwaiter().GetResult(), Task.Run, Task.Factory.StartNew, Thread, Timer, SendStringToExecute y un
     CommandMethod oculto. Sin controles de UI de T2B, sin pestanas F3, sin edicion F4, sin rediseno responsive, sin comandos nuevos de AutoCAD
     y sin migracion de editores.
  El texto de ExpectedEvidence que dice "las APIs de lectura solo en WorkspaceNavigationFacts.cs y en el lector existente" se lee con la
  excepcion READ del punto 1.
- HECHOS DEL REPOSITORIO medidos por la sesion en el HEAD (comprueba los que uses):
  * Proyectos: tests/RackCad.Tests referencia Domain y Application (nunca el Plugin ni la UI); tests/RackCad.UI.Tests referencia la UI.
    El Plugin solo se compila en la CI (job "Build Plugin without AutoCAD", AutoCAD.NET 25.0.1 con -p:UseAutoCADNuGetReferences=true) y
    nunca se prueba unitariamente: las pruebas de Core cubren los tipos puros de Application y GUARDAS DE TEXTO sobre las fuentes del
    Plugin, leidas con el ayudante WorkspaceHostSource (tests/RackCad.Tests/Workspace/WorkspaceHostSourceSupport.cs: Code, Text,
    CodeOnly, Files, PluginFiles, ApplicationFiles, AllProduct, BodyOf, Count). El comportamiento real en AutoCAD lo comprueba Smoke-NAV.
  * Linea base en el HEAD (Release, local): FullyQualifiedName~RackCad.Tests.Workspace = 656 pruebas de Core y
    FullyQualifiedName~RackCad.UI.Tests.Workspace = 64 pruebas de UI, todas en verde. Las pruebas de tests/RackCad.Tests/Workspace/
    usan el namespace RackCad.Tests (file-scoped), clases Workspace* y #nullable enable por archivo; el CI rechaza avisos nuevos.
  * API de F2-T1 (sin cambio): NavigationFacts.cs: record NavigationDefinitionFact(long DefinitionHandle, bool IsXref, bool
    IsInterpretable, string? EnvelopeId, string? ProbedId, string? Kind, string? View, int Section, string? Name,
    IReadOnlyList<long> ModelSpaceReferenceHandles) e interface INavigationFactsPort { ReadDefinitionFacts(SessionId) }.
    NavigationTargets.cs: NavigationTargets(INavigationFactsPort) con Resolve, ResolveForContext, IsCached, Invalidate,
    InvalidateSession, ApplyDrain(SessionId, DrainResult), EndSession, IsSessionEnded y Revalidate. NavigationActions.cs:
    NavigationActionKind {Select, View, Center, LocateView}; NavigationActionVerdict {Act, RejectedNoTargets,
    RejectedSessionMismatch, RejectedSessionDestroyed, RejectedTargetGone, RejectedNotAttributable}; NavigationActionPlan;
    NavigationActionDecision; interface INavigationEffects { SelectReferences(SessionId, IReadOnlyList<long>);
    ViewReference(SessionId, long); CenterOnReference(SessionId, long) }; NavigationActionPolicy(NavigationTargets) con
    Plan(origin, rackId, action, copyIndex = 0, view = null, locateWith = View), Decide(plan, executingSession) y
    Execute(plan, executingSession, effects). Hoy nada en src/ construye NavigationTargets ni NavigationActionPolicy ni llama a
    ApplyDrain o EndSession.
  * GUARDAS que hoy bloquean T2A (todas en tests/RackCad.Tests/Workspace/; sus deltas exactos van en el RED, C-F2T2-08):
    - WorkspaceBridgeSourceGuardTests.TheBridgeNeverWritesSelectionOrViewNorReadsStoredDataNorAsksNorOpensDialogs recorre TODO
      src/RackCad.Plugin/Workspace/ con ForbiddenCallNames (SetImpliedSelection, SetCurrentView, ZoomExtents, ZoomWindow, Zoom, PanTo,
      UpdateScreen, Regen, SendStringToExecute, StartCommand, GetSelection, GetEntity, GetString, GetPoint, GetKeywords, GetInteger,
      GetDouble, SelectImplied, SelectAll, SelectAtPoint, ShowModalDialog, ShowModelessDialog, ShowDialog, MessageBox, GetObject,
      OpenCloseTransaction, Highlight, Unhighlight); la unica excepcion es el lector (WorkspaceSelectionReader.cs, solo SelectImplied y
      GetObject) por archivo exacto, con metapruebas en WorkspaceSourceGuardExceptionTests.cs.
    - WorkspaceHostSourceGuardTests (43 [Fact]/[Theory], recuento fijado por WorkspacePaletteHostSourceGuardTests.
      TheRefinementsG1G2AndG3AreExactAndNothingWasRemovedFromTheT3AGuardFile, que tambien fija 3 += del controlador):
      NoHostFileWritesTheSelectionTheViewOrAnySystemVariable (SetImpliedSelection, SetCurrentView, SetSystemVariable, Zoom\w*, Regen,
      UpdateScreen, Highlight, Unhighlight, SendStringToExecute, PanTo en Plugin/Workspace y Application/Workspace: OJO, ningun
      identificador nuevo puede contener Zoom); NoHostFileStartsABackgroundThreadOrAsyncWork (Task.Run, Task.Factory, new Thread,
      ThreadPool, Timer, async, await, Parallel., BackgroundWorker, SynchronizationContext en Plugin y Application Workspace);
      NoHostFileUsesTheInternalAutoCadNamespaceOrAnyUiType (Autodesk.AutoCAD.Internal, PaletteSet, AddVisual, System.Windows, RackCad.UI,
      Autodesk.AutoCAD.Windows, UserControl, Dispatcher; el host de la paleta es la unica excepcion G1); TheReaderApisAreUsedInNoOther
      FileOfThePluginWorkspace (GetObject, RackBlockData, RackEmbedStore, SelectImplied fuera del lector);
      EveryTransactionAndLockInTheHostIsInsideUsingAndNoTypeOfTheHostIsHeldInAField; las guardas del controlador y del comando.
    - WorkspacePaletteHostSourceGuardTests.TheFirstPlacementAddsNoPersistenceIdentityAutoloadProfileOrSecurityMechanism congela la
      lista EXACTA de los seis archivos de src/RackCad.Plugin/Workspace/.
    - WorkspaceNavigationAuthorityTests: OnlyTheAuthorityTraversesTheReferenceHandlesOfTheFacts (solo NavigationFacts.cs y
      NavigationTargets.cs nombran ModelSpaceReferenceHandles); solo NavigationTargets.cs llama a .ReadDefinitionFacts(;
      NoEventSideFileCanReachTheNavigationEffectsOrThePolicy (ningun archivo nombra a la vez INavigationEffects, NavigationActionPolicy o
      NavigationActionPlan y IHostEventSource, HintKind, EnqueueHint, EventBridgeCore o IWorkspaceBridgeLink; EventBridgeCore.cs,
      WorkspaceHostCore.cs, WorkspaceSession.cs, Hints.cs y BridgeSource.cs no pueden nombrar tipos de navegacion de esa familia).
      Por eso la politica vive en WorkspaceNavigationHost.cs y no en WorkspaceController.cs (que declara IWorkspaceBridgeLink).
    - WorkspaceHostPortGuardTests fija IWorkspaceHostPort como solo lectura: el puerto nuevo es otro tipo y esas guardas no cambian.
    - WorkspaceModelBoundaryTests recorre todo src/RackCad.Application/Workspace/: prohibe System.IO, la SUBCADENA Transaction, la
      palabra Database, Commit(, Autodesk.; nombres de tipos con Tab, Subscription, Editor o Draft (sin distinguir mayusculas; OJO
      Stable, Table, Editable); y en nombres publicos ProjectSummary, Metric, Provider, TotalRacks, Aggregat y Bom.
  * Seleccion y drenaje: WorkspaceSelectionRead (src/RackCad.Application/Workspace/WorkspaceSelectionRead.cs, FUERA del alcance)
    transporta un DefinitionFact por referencia seleccionada y el recuento de no-racks, pero NO los handles de las referencias;
    WorkspaceSelectionReader.cs (Plugin, candidato) obtiene los ObjectId con SelectImplied; WorkspaceHostCore.cs (candidato) drena:
    _reader.Read(...) y una sola llamada session.ApplyContext(WorkspaceSelectionMapper.ToContext(read)); guardas con recuentos
    exactos en WorkspaceHostCore.cs (.ApplyContext( una vez, _reader.Invalidate( una vez, WorkspaceSelectionMapper.ToContext( una
    vez, sin SelectionContext.One( ni SelectionContext.Selection(). El eco por CONJUNTO de handles tiene que fluir por los archivos
    permitidos (por ejemplo, una instancia compartida de NavigationEcho que el lector alimenta con el conjunto leido y que el nucleo
    consulta antes de aplicar el contexto) sin tocar WorkspaceSelectionRead.cs ni WorkspaceSession.cs; si no es posible, STOP P-03
    (F2T2A-C08) explicado en RoutingReason.
  * Controlador y puente: WorkspaceController.cs (candidato) es un singleton perezoso que posee AutoCadEventBridge, WorkspaceHostCore,
    WorkspaceSelectionReader y WorkspaceHostPort, expone event Action StateChanged, tiene exactamente 3 += y declara la clase anidada
    BridgeLink : IWorkspaceBridgeLink. El puente (prohibido) expone Drained(DrainOutcome), ActiveSessionChanged, CommandStateChanged y
    las fachadas ActiveSession, Authorize, TryGetSession y RequestReconciliation; no hay aviso que nombre una SessionId destruida
    (de ahi la purga perezosa con TryGetSession).
  * Autoridades integradas (prohibido cambiarlas): RackBlockFinder.ScanEnvelopes (src/RackCad.Plugin/RackBlockFinder.cs, internal
    static; omite xref y devuelve los sobres con su recuento, sin handles de referencias); RackInventarioCommands.ZoomToRack (privado:
    centro y extension GeometricExtents de la referencia con factor 1.10, planta XY, SetCurrentView con documento bloqueado) es la
    referencia de VIEW; no se llama ni se modifica.
  * Ningun proyecto de prueba referencia el Plugin: las pruebas de RED de T2A son de Core (tipos puros de Application con puertos
    simulados, y guardas de texto de los archivos nuevos del Plugin).
  * Guardas de T2 que se aplican a TODO archivo de src/RackCad.Plugin/Workspace/ (incluidos los tuyos; lee tests/RackCad.Tests/Workspace/*Guard*.cs
    antes de escribir): las transacciones y LockDocument solo dentro de using; sin .Commit( en esa carpeta (las lecturas no confirman, y seleccion
    y vista no son escrituras de base de datos); sin campos ni propiedades de tipo Transaction, DBObject, Document, Database, Entity, BlockReference,
    ObjectId ni Editor; las lineas public/protected no nombran Document, Editor, Database, ObjectId, Entity, Autodesk ni *EventArgs; sin System.IO,
    File., Microsoft.Win32, Registry., Save(, DocumentExtension; sin [CommandMethod, IExtensionApplication, [assembly:, ModuleInitializer; sin +=/-=
    nuevos sobre eventos gobernados; sin ComputedParameter, Metric, ProjectSummary, TotalRacks, Aggregat, Provider ni Bom (sin distinguir mayusculas) en
    Plugin/Workspace ni Application/Workspace (OJO: IFormatProvider, Symmetric).
  * NamespaceFolderGuardTests: en src/ el namespace de cada archivo es la ruta de su carpeta (RackCad.Plugin.Workspace y RackCad.Application.Workspace), con
    exactamente un namespace de bloque (namespace X { ... }); en tests/RackCad.Tests el unico namespace es RackCad.Tests (file-scoped). Las clases de
    prueba nuevas se llaman Workspace... para que el filtro FullyQualifiedName~RackCad.Tests.Workspace las seleccione.
  * Nullable: los archivos de tests/RackCad.Tests/Workspace/ y de src/RackCad.Application/Workspace/ llevan #nullable enable por archivo; los archivos
    actuales de src/RackCad.Plugin/Workspace/ NO lo llevan: si un archivo no tiene #nullable enable, no uses anotaciones ? de tipo referencia (CS8632 rompe
    el job de CI del Plugin); SessionId? (struct) si vale. Usa catch (System.Exception ex) en los archivos que importen Autodesk.AutoCAD.Runtime.
  * Build local del Plugin (linea base medida por la sesion en BaseSha, con las DLL de AutoCAD instaladas):
    dotnet build src/RackCad.Plugin/RackCad.Plugin.csproj -c Release --no-incremental -v:minimal -o <dir temporal bajo artifacts/orchestration/I-64/F2-T2A-NAVIGATION-HOST/2/R20261109T194127Z-31c0/tests/>
    -> codigo 0, 0 errores, resumen "2 Advertencia(s)", ambas MSB3277 (conflicto de versiones de las DLL de AutoCAD de esta estacion; no existen en la
    CI). Tu build debe dar 0 errores, 0 avisos CS/NU/NETSDK y ningun aviso MSB distinto de MSB3277, con el mismo resumen de 2. Compila SIEMPRE el Plugin
    con -o hacia ese directorio temporal (nunca su bin/), no lo publiques y no cargues nada en AutoCAD. El job de CI "Build Plugin without AutoCAD"
    falla ante cualquier aviso MSB/CS/NU/NETSDK y compila contra AutoCAD.NET 25.0.1: usa solo API publica que exista en esa version.
  * Ninguna suscripcion ni trabajo al cargar el plugin: PluginInitializer.cs no se toca; el host de navegacion se crea de forma perezosa desde el
    controlador.
- Paso RED nuevo (un solo commit, con TODOS los cambios de pruebas de esta entrega):
  1. Cambia SOLO lo que piden los criterios: la asercion imposible en WorkspaceNavigationEchoTests.cs (corrigela con el minimo cambio; no borres la garantia) y la prueba de F2T2A-KL-01 en WorkspaceNavigationHostTests.cs.
     Si otra prueba de la cadena resultara imposible de satisfacer o hiciera falta cambiar mas pruebas: parada y bloqueo estructurado.
  2. Comprobacion previa, sin commit: aplica en el arbol la implementacion (semilla revisada y corregida) y comprueba que TODAS las pruebas de la cadena pasan (filtro Core completo en verde, incluida la prueba corregida), que el filtro UI
     sigue 64/64 y que el Plugin compila. Asi queda demostrado que ninguna prueba del RED es imposible, y ninguna falla por un archivo fuera de alcance. Guarda tu implementacion como parche propio bajo artifacts/orchestration/I-64/F2-T2A-NAVIGATION-HOST/2/R20261109T194127Z-31c0/ (por ejemplo
     green-attempt2.patch) y devuelve los archivos de producto a HEAD con git restore --source=HEAD --staged --worktree -- <rutas>.
  3. Con el producto en su esqueleto de BaseSha y solo las pruebas cambiadas, corre el filtro Core: debe compilar y FALLAR POR ASERCION (seleccion > 656), solo por el comportamiento que falta, y el filtro UI debe seguir en verde y sin
     cambios (64). Commit RED (git add solo de archivos de tests/ del alcance) y git push. Registra la corrida como Phase RED con TreeSha = el commit RED.
- Desde el commit RED NO modifiques ni anadas archivos bajo tests/ (son RT; 16.8: el GREEN no toca RT). Si una prueba necesitara cambio: parada y bloqueo
  estructurado, no la edites.
- Paso GREEN:
  1. Reaplica tu implementacion en los archivos nuevos y en WorkspaceController.cs, WorkspaceSelectionReader.cs y WorkspaceHostCore.cs hasta que el filtro
     quede verde. Si el eco no se puede resolver sin cambiar WorkspaceSelectionRead.cs: STOP P-03. Si ProbedId resultara necesario para la atribucion, la correccion de la navegacion o INV-40: STOP.
  2. Sobre el commit GREEN (arbol limpio, TreeSha = CurrentSha): los dos filtros requeridos (todas en verde, Phase GREEN; Core > 656, UI >= 64); las suites
     completas tests/RackCad.Tests/RackCad.Tests.csproj y tests/RackCad.UI.Tests/RackCad.UI.Tests.csproj (0 fallidas; Phase RELEVANT); dotnet build -c Release
     --no-incremental de ambos proyectos de prueba con 0 errores y 0 lineas que casen \bwarning\s+(MSB|CS|NU|NETSDK)\d+\b; y la build local del Plugin de arriba
     (Phase RELEVANT). Registra comando, seleccion, resultado y conteos. Antes de crear el commit GREEN comprueba lo mismo en el arbol de trabajo. Si tras el
     commit algo falla, no lo arregles con otro commit: parada y bloqueo estructurado.
  3. Commit GREEN (git add solo de archivos del alcance) con este cuerpo de resumen de estado, y git push (fast-forward):
     "Estado: F2 en curso; entrega del Worker de F2-T2A-NAVIGATION-HOST (WorkRunId R20261109T194127Z-31c0, intento 2) pendiente de la CI exacta, de la
     verificacion del Controller y de nc1..nc3. attempts = 2. F2-T2B no autorizada."
- Si paras con cambios sin commit: guarda git diff HEAD como parche bajo artifacts/orchestration/I-64/F2-T2A-NAVIGATION-HOST/2/R20261109T194127Z-31c0/, cuentalo en la entrega y deja el arbol limpio con git restore --source=HEAD --staged --worktree -- <rutas>; no dejes el arbol sucio.
- Git: sin --force, sin rebase, sin stash, sin amend, sin checkout de otros SHA, sin tocar otras ramas ni main; exactamente un commit RED y un commit GREEN,
  cada uno con su push.
- No lances subagentes ni procesos en segundo plano que sigan vivos al terminar. Los servidores de compilacion que deje dotnet (VBCSCompiler, nodos MSBuild)
  pueden seguir vivos; no los detengas. No abras ni toques AutoCAD.
- En la entrega y en los mensajes de commit no escribas rutas locales distintas de las del worktree y de artifacts/; nada de config.toml.
- Entrega (artifacts/orchestration/I-64/F2-T2A-NAVIGATION-HOST/2/R20261109T194127Z-31c0/worker-handoff.json, UTF-8, valida contra el esquema; compruebala con pwsh Test-Json -SchemaFile):
  Schema rackcad-worker-handoff/v1; TaskId F2-T2A-NAVIGATION-HOST; RunId R20261109T194127Z-31c0; DelegationRunId R20261109T193332Z-bafe; Initiative I-64; Gate F2;
  Attempt 2; BaseSha = el de arriba; RedSha = el SHA del commit RED; CurrentSha = el SHA del commit GREEN (= HEAD = remoto);
  Branch y Worktree reales; Pushed true; FilesChanged = git diff --name-only BaseSha..CurrentSha; TestsExecuted y TestResults de cada corrida (RunRef unico,
  Command exacto, Phase RED/GREEN/RELEVANT, TreeSha = commit probado, ResultsFile = TRX o log, o null; FailedTests con los nombres reales); Worker = { Provider
  "Anthropic", ModelRequested "claude-sonnet-5-5", EffortRequested "high", Trailer "Co-Authored-By: Claude Sonnet 5.5 <noreply@anthropic.com>" }; WorkerStatus
  IMPLEMENTATION_COMPLETE, PARTIAL o BLOCKED; Disposition NONE salvo parada; listas vacias si no hay nada. En WorkCompleted y Evidence describe los tipos
  creados, el resumen de deltas de guardas y el mapa evidencia-por-invariante (INV-F2T2A-01..16) con las pruebas que lo demuestran, y que parte de la semilla reutilizaste, corregiste o descartaste; en KnownLimitations, lo que
  queda sin demostrar, sin afirmar nada sobre AutoCAD mas alla de lo que establecen la CI, las fuentes y las pruebas (el comportamiento real se comprueba en
  Smoke-NAV). La sesion anade despues las corridas de CI y la evidencia de enrutamiento y modelo.
- Tope: 75 minutos (AUTOMATION_PLAN 16.4). Al terminar, tu respuesta final es una linea con WorkerStatus, RedSha y CurrentSha.
