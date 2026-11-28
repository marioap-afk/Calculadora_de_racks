I-64 - DELEGACION F1-T3B-HOST-UI - Worker, perfil ROUTINE_IMPLEMENTATION (primera delegacion de la cadena; unica entrega valida autorizada)
Identidad: unidad I-64; gate F1; tarea F1-T3B-HOST-UI; intento 3; RunId R20261011T090257Z-3038;
           DelegationRunId R20261011T081209Z-deae; AuthorityRevision c7f389cbb2eff4be42f861d8114adc2af753a1c9;
           MainSha bb0d5522e8411f66a51fdfb3f1f0d0514b737453; BaseSha f3a9f40e0d0057e14a6eb7a6681590b78cecf37f;
           rama architecture/workspace-persistente-rackcad;
           worktree <worktree-local-redactado>;
           Owner subagent:R20261011T081209Z-deae
Autoridades (leer desde Git, no copiar; ruta | seccion | clase):
           - docs/initiatives/I-64-proposal-v5.md | documento completo | UNIT_DOC
           - docs/initiatives/I-64-discovery.md | documento completo | UNIT_DOC
           - docs/initiatives/I-64-workspace-persistente-rackcad.md | documento completo | UNIT_DOC
           - docs/initiatives/I-64-A-2.md | documento completo | UNIT_DOC
           - docs/initiatives/I-64-A-4.md | documento completo | UNIT_DOC
           - docs/initiatives/I-64-A-5.md | documento completo | UNIT_DOC
           - docs/initiatives/I-64-A-6.md | documento completo | UNIT_DOC
           - docs/automation/evidence/I-64-evidence.md | ## 18. Consensus Freeze (I64-CONSENSUS-V5-01) y cierre de F0 | UNIT_DOC
           - docs/automation/evidence/I-64-evidence.md | ## 49. F1-T3-HOST - Decision del Coordinator: descomposicion en F1-T3A-HOST-CORE y F1-T3B-HOST-UI | UNIT_DOC
           - docs/automation/evidence/I-64-evidence.md | ## 50. F1-T3A-HOST-CORE - Contrato y planificacion, aceptacion, STOP previo al Worker y correccion de alcance | UNIT_DOC
           - docs/automation/evidence/I-64-evidence.md | ## 51. F1-T3A-HOST-CORE - Worker, verificacion, controles, puentes y cierre documental | UNIT_DOC
           - docs/automation/evidence/I-64-evidence.md | ## 52. F1-T3A-HOST-CORE - Revision del cierre por el Coordinator: PASS; correccion documental del estado | UNIT_DOC
           - docs/automation/evidence/I-64-evidence.md | ## 53. F1-T3B-HOST-UI - Inspeccion de T3A, enrutamiento y paquete del contrato de gate (NOT EMITTED) | UNIT_DOC
           - docs/automation/evidence/I-64-evidence.md | ## 54. F1-T3B-HOST-UI - Decision del Coordinator sobre el contrato (AUTHORIZED) | UNIT_DOC
           - docs/automation/state/I-64.yml | documento completo | UNIT_DOC
           - docs/initiatives/I-64-f1-gate-contract-proposal.md | documento completo | UNIT_DOC
           - docs/automation/evidence/I-64-pilot/F1-T3A-HOST-CORE/coordinator-decision-t3-decomposition.md | documento completo | UNIT_DOC
           - docs/automation/evidence/I-64-pilot/F1-T3A-HOST-CORE/coordinator-scope-correction.md | documento completo | UNIT_DOC
           - docs/automation/evidence/I-64-pilot/F1-T3A-HOST-CORE/coordinator-close-review.md | documento completo | UNIT_DOC
           - docs/automation/evidence/I-64-pilot/F1-T3A-HOST-CORE/task-close-package.md | documento completo | UNIT_DOC
           - docs/automation/evidence/I-64-pilot/F1-T3B-HOST-UI/coordinator-decision-t3b-contract.md | documento completo | UNIT_DOC
           - docs/adr/0047-workspace-persistente-modeless-rackcad.md | documento completo | UNIT_DOC
           - AGENTS.md | documento completo | EXTERNAL
           - docs/WORKFLOW.md | documento completo | EXTERNAL
           - docs/INITIATIVE_LIFECYCLE.md | documento completo | EXTERNAL
           - docs/AUTOMATION_PLAN.md | ## 16. Ejecucion delegada bajo orden del Coordinator | EXTERNAL
           - docs/automation/agent-execution/README.md | documento completo | EXTERNAL
           - docs/automation/agent-execution/routing.md | documento completo | EXTERNAL
           - docs/automation/agent-execution/model-catalog.md | documento completo | EXTERNAL
           - docs/adr/0006-autocad-solo-en-plugin.md | documento completo | EXTERNAL
           - docs/adr/0019-shell-visual-de-editores-por-composicion.md | documento completo | EXTERNAL
           - docs/adr/0029-contrato-funcional-comun-de-ventanas-wpf.md | documento completo | EXTERNAL
           vigencia: AuthorityRevision para la unidad (UNIT_DOC), MainSha para lo demas (EXTERNAL)
           Los valores de seccion son ASCII (sin diacriticos); en Git los encabezados conservan sus tildes.
Objetivo (texto literal del contrato, linea a linea, sin lineas en blanco):
           Implement F1-T3B-HOST-UI: the visual host of the F1 Workspace panel over the
           closed T3A host foundation, without running AutoCAD. Required behavior includes:
           * exactly one PaletteSet per process, owned by the new exact file
           src/RackCad.Plugin/Workspace/WorkspacePaletteHost.cs, created lazily from the
           RACKPANEL path, without toolID and without Load/Save handlers, close = hide,
           palette title "RackCad";
           * AddVisual of exactly one RackCad.UI root UserControl (new files in
           src/RackCad.UI/Workspace/) labelled "Workspace", whose view-model is fed only
           by the injected IWorkspaceHostPort and host-neutral delegates;
           * RACKPANEL (minimum delta of WorkspacePanelCommand.cs): obtain the existing
           WorkspaceController, call OpenPanel(), create/obtain the single
           WorkspacePaletteHost and show the palette;
           * event-driven refresh, no timer and no polling: AutoCadEventBridge raises an
           internal in-memory ActiveSessionChanged from its existing DocumentActivated
           handler (and DocumentToBeDestroyed when the active session changes);
           WorkspaceController relays it (RefreshPanel only while the panel is visible)
           and exposes StateChanged after drains and panel transitions; the palette host
           pulls GetSnapshot / GetMeasurementReport only on StateChanged or explicit UI
           interaction;
           * presentation of NotStarted, NoDocument (empty state), Ready and Degraded,
           action availability and block reason, PICKFIRST notice, the five selection
           contexts and the in-memory instrumentation, with the instrumentation toggle
           injected as a host-neutral delegate;
           * focus through public host APIs only (PaletteSet.KeepFocus or equivalent);
           containment of every panel-originated exception with a panel Error state;
           * local merge of Themes/AppStyles.xaml in the root control;
           * exact guard refinements G1/G2/G3 in WorkspaceHostSourceGuardTests.cs.
           No navigation, tabs, drafts, authored writes, inventory, persistence,
           Autodesk.AutoCAD.Internal, timers or AutoCAD execution. Smoke-1 is separate.
           T3B is a new chain.
           Therefore:
           ChainBaseSha = first T3B BaseSha
           ChainRedSha = null
           ChainRedFiles = []
           A real RED is mandatory in both required test projects.
           RED must be behavioral and compilable and fail by assertion.
           No artificial failure.
           GREEN must not modify the RED test files after the RED commit contrary to
           16.8.
Alcance: permitido EXACTAMENTE src/RackCad.UI/Workspace/, tests/RackCad.UI.Tests/Workspace/, src/RackCad.Plugin/Workspace/WorkspacePaletteHost.cs, src/RackCad.Plugin/Workspace/WorkspacePanelCommand.cs, src/RackCad.Plugin/Workspace/WorkspaceController.cs, src/RackCad.Plugin/Workspace/AutoCadEventBridge.cs, tests/RackCad.Tests/Workspace/WorkspacePaletteHostSourceGuardTests.cs, tests/RackCad.Tests/Workspace/WorkspaceHostSourceGuardTests.cs (archivo exacto o prefijo terminado en /);
           prohibido docs/, .github/, assets/, eng/, deploy/, tools/, global.json, Directory.Build.props, Directory.Build.targets, RackCad.sln, src/RackCad.Domain/, src/RackCad.UI/CatalogOption.cs, src/RackCad.UI/Controls/, src/RackCad.UI/Editor/, src/RackCad.UI/EnumDisplayConverter.cs, src/RackCad.UI/MainMenuAction.cs, src/RackCad.UI/ObservableObject.cs, src/RackCad.UI/Preview/, src/RackCad.UI/PreviewCanvasPainter.cs, src/RackCad.UI/ProjectVariableRepairText.cs, src/RackCad.UI/RackBomWindow.xaml, src/RackCad.UI/RackBomWindow.xaml.cs, src/RackCad.UI/RackCad.UI.csproj, src/RackCad.UI/RackCommandHelpWindow.cs, src/RackCad.UI/RackCommandReference.cs, src/RackCad.UI/RackConsolidatedBomWindow.xaml, src/RackCad.UI/RackConsolidatedBomWindow.xaml.cs, src/RackCad.UI/RackCustomPropertiesWindow.xaml, src/RackCad.UI/RackCustomPropertiesWindow.xaml.cs, src/RackCad.UI/RackDesignLibraryWindow.xaml, src/RackCad.UI/RackDesignLibraryWindow.xaml.cs, src/RackCad.UI/RackFrames/, src/RackCad.UI/RackListRow.cs, src/RackCad.UI/RackListWindow.xaml, src/RackCad.UI/RackListWindow.xaml.cs, src/RackCad.UI/RackMainMenuWindow.xaml, src/RackCad.UI/RackMainMenuWindow.xaml.cs, src/RackCad.UI/RackProjectVariablesWindow.xaml, src/RackCad.UI/RackProjectVariablesWindow.xaml.cs, src/RackCad.UI/RackWarehouseFillWindow.cs, src/RackCad.UI/RackWarehouseLayoutWindow.cs, src/RackCad.UI/SafetyDefensaGridWindow.cs, src/RackCad.UI/SafetyDefenseFace.cs, src/RackCad.UI/SafetyDesviadorGridWindow.cs, src/RackCad.UI/SafetyGuiaEntradaGridWindow.cs, src/RackCad.UI/SafetyParrillaGridWindow.cs, src/RackCad.UI/SafetyTopeGridWindow.cs, src/RackCad.UI/SelectiveSafetyWindow.cs, src/RackCad.UI/Shell/, src/RackCad.UI/StructuralSections/, src/RackCad.UI/Systems/, src/RackCad.UI/Themes/, src/RackCad.UI/UiSupport.cs, src/RackCad.UI/Views/, tests/RackCad.UI.Tests/RackCad.UI.Tests.csproj, tests/RackCad.UI.Tests/StaTestRunner.cs, tests/RackCad.UI.Tests/WindowCensusGuardTests.cs, tests/RackCad.UI.Tests/UiSystemBoundaryGuardTests.cs, tests/RackCad.UI.Tests/CustomPropertiesHelpCensusTests.cs, src/RackCad.Application/RackCad.Application.csproj, src/RackCad.Application/BlockNaming.cs, src/RackCad.Application/Bom/, src/RackCad.Application/Catalogs/, src/RackCad.Application/ComputedParameters/, src/RackCad.Application/CustomProperties/, src/RackCad.Application/Diagnostics/, src/RackCad.Application/Drawing/, src/RackCad.Application/Expressions/, src/RackCad.Application/Formatting/, src/RackCad.Application/Geometry/, src/RackCad.Application/Layout/, src/RackCad.Application/Persistence/, src/RackCad.Application/ProjectVariables/, src/RackCad.Application/RackFrames/, src/RackCad.Application/Settings/, src/RackCad.Application/StructuralSections/, src/RackCad.Application/Systems/, src/RackCad.Application/Units/, src/RackCad.Application/Views/, src/RackCad.Application/Workspace/, src/RackCad.Plugin/CustomPropertiesData.cs, src/RackCad.Plugin/CustomPropertiesExecutor.cs, src/RackCad.Plugin/InDocumentTransaction.cs, src/RackCad.Plugin/LayerHelper.cs, src/RackCad.Plugin/PluginInitializer.cs, src/RackCad.Plugin/ProjectVariableMutationExecutor.cs, src/RackCad.Plugin/ProjectVariablesData.cs, src/RackCad.Plugin/ProjectVariablesRegistry.cs, src/RackCad.Plugin/RackAyudaCommands.cs, src/RackCad.Plugin/RackBlockFinder.cs, src/RackCad.Plugin/RackCabeceraCommands.cs, src/RackCad.Plugin/RackCad.Plugin.csproj, src/RackCad.Plugin/RackCamaCommands.cs, src/RackCad.Plugin/RackCantileverCommands.cs, src/RackCad.Plugin/RackCatalogLoader.cs, src/RackCad.Plugin/RackCloner.cs, src/RackCad.Plugin/RackCommandSupport.cs, src/RackCad.Plugin/RackDinamicoCommands.cs, src/RackCad.Plugin/RackDuplicarCommands.cs, src/RackCad.Plugin/RackEnvelopeRestamp.cs, src/RackCad.Plugin/RackInventarioCommands.BomTotal.cs, src/RackCad.Plugin/RackInventarioCommands.cs, src/RackCad.Plugin/RackLayoutCommands.Fill.cs, src/RackCad.Plugin/RackLayoutCommands.cs, src/RackCad.Plugin/RackMenuCommands.cs, src/RackCad.Plugin/RackPropiedadesCommands.cs, src/RackCad.Plugin/RackProyectarCommands.cs, src/RackCad.Plugin/RackPushBackCommands.cs, src/RackCad.Plugin/RackSeccionCommands.cs, src/RackCad.Plugin/RackSelectivoCommands.cs, src/RackCad.Plugin/RackSelectivoInsertIntegration.cs, src/RackCad.Plugin/RackUnitsGuard.cs, src/RackCad.Plugin/RackVariablesCommands.cs, src/RackCad.Plugin/StructuralSectionCatalogAccess.cs, src/RackCad.Plugin/StructuralSectionCommandFlow.cs, src/RackCad.Plugin/Drawing/, src/RackCad.Plugin/KindHandlers/, src/RackCad.Plugin/Systems/, src/RackCad.Plugin/Views/, src/RackCad.Plugin/Workspace/WorkspaceHostPort.cs, src/RackCad.Plugin/Workspace/WorkspaceSelectionReader.cs, tests/RackCad.Tests/RackCad.Tests.csproj, tests/RackCad.Tests/NamespaceFolderGuardTests.cs, tests/RackCad.Tests/CustomPropertiesCommandGuardTests.cs, tests/RackCad.Tests/SelectiveEditorOpenTests.cs, tests/RackCad.Tests/Workspace/WorkspaceBridgeDegradedModeTests.cs, tests/RackCad.Tests/Workspace/WorkspaceBridgeFacadeSourceGuardTests.cs, tests/RackCad.Tests/Workspace/WorkspaceBridgeHintAndDrainTests.cs, tests/RackCad.Tests/Workspace/WorkspaceBridgeReconciliationTests.cs, tests/RackCad.Tests/Workspace/WorkspaceBridgeSourceGuardTests.cs, tests/RackCad.Tests/Workspace/WorkspaceBridgeSubscriptionLifecycleTests.cs, tests/RackCad.Tests/Workspace/WorkspaceBridgeTestSupport.cs, tests/RackCad.Tests/Workspace/WorkspaceHintDrainTests.cs, tests/RackCad.Tests/Workspace/WorkspaceHostAuthorizationTests.cs, tests/RackCad.Tests/Workspace/WorkspaceHostDrainTests.cs, tests/RackCad.Tests/Workspace/WorkspaceHostLifecycleTests.cs, tests/RackCad.Tests/Workspace/WorkspaceHostPolicyTests.cs, tests/RackCad.Tests/Workspace/WorkspaceHostPortGuardTests.cs, tests/RackCad.Tests/Workspace/WorkspaceHostSourceSupport.cs, tests/RackCad.Tests/Workspace/WorkspaceHostTestSupport.cs, tests/RackCad.Tests/Workspace/WorkspaceInstrumentationTests.cs, tests/RackCad.Tests/Workspace/WorkspaceModelBoundaryTests.cs, tests/RackCad.Tests/Workspace/WorkspaceSelectionContextTests.cs, tests/RackCad.Tests/Workspace/WorkspaceSelectionMappingTests.cs, tests/RackCad.Tests/Workspace/WorkspaceSessionTests.cs, tests/RackCad.Tests/Workspace/WorkspaceSourceGuardExceptionTests.cs.
           src/RackCad.UI/Workspace/ y tests/RackCad.UI.Tests/Workspace/ son carpetas NUEVAS (solo archivos nuevos);
           WorkspacePaletteHost.cs y WorkspacePaletteHostSourceGuardTests.cs son archivos nuevos exactos; WorkspacePanelCommand.cs,
           AutoCadEventBridge.cs y WorkspaceController.cs solo con sus deltas exactos (INV-F1-T3B-03, 04 y 05);
           WorkspaceHostSourceGuardTests.cs solo con G1/G2/G3 (INV-F1-T3B-14). Ningun otro archivo cambia.
Invariantes del Freeze (vinculantes en su totalidad):
           - INV-F1-T3B-01 (D-01, P-01, D-02): un unico PaletteSet por proceso, construido solo en src/RackCad.Plugin/Workspace/WorkspacePaletteHost.cs (archivo nuevo exacto) y de forma perezosa con WorkspaceLazyInstance en la ruta de RACKPANEL; nada se crea al cargar el plugin (PluginInitializer.cs intacto, sin constructor estatico ni ModuleInitializer); construido sin toolID (ningun Guid) y sin manejadores Load ni Save; titulo "RackCad"; acoplable y flotante; cerrar = ocultar; RACKPANEL repetido no crea una segunda paleta ni un segundo control raiz; la paleta vive hasta que AutoCAD termina (sin liberacion en Terminate()).
           - INV-F1-T3B-02 (D-01, D-02): AddVisual exactamente una vez con exactamente un control raiz UserControl de RackCad.UI (archivo nuevo en src/RackCad.UI/Workspace/, namespace RackCad.UI.Workspace) rotulado "Workspace"; el Plugin crea la raiz e inyecta IWorkspaceHostPort (WorkspaceController.Port) y delegados neutrales (sin tipos de AutoCAD ni del Plugin) para el interruptor de instrumentacion y, solo si hace falta, callbacks puramente visuales; la raiz y su view-model no obtienen servicios de host de forma estatica.
           - INV-F1-T3B-03 (D-01, decision del Coordinator; delta minimo de WorkspacePanelCommand.cs): RACKPANEL sigue siendo el unico comando, exactamente en ese archivo, con un unico [CommandMethod("RACKPANEL")] y sin alias; obtiene el WorkspaceController existente, llama a OpenPanel(), crea u obtiene el unico WorkspacePaletteHost y muestra la paleta; ninguna logica de PaletteSet en el comando (sin PaletteSet, AddVisual, System.Windows, RackCad.UI ni Autodesk.AutoCAD.Windows en ese archivo); el mensaje de la linea de comandos ya no dice que la interfaz no se muestra; catch (System.Exception) con RackCommandSupport.Report; los demas comandos no cambian; los censos de comandos (38) y de ayuda no cambian.
           - INV-F1-T3B-04 (D-03, P-02, INV-33, decision del Coordinator; delta exacto de AutoCadEventBridge.cs): una unica notificacion en memoria ActiveSessionChanged (Action sin argumentos, sin tipos de AutoCAD), declarada internal para que la superficie publica del puente que fija WorkspaceBridgeFacadeSourceGuardTests no cambie; solo la disparan, dentro del mismo Contain(...) y despues de actualizar el nucleo, el manejador existente de DocumentActivated (tras _core.OnDocumentActivated) y el de DocumentToBeDestroyed (tras _core.OnDocumentToBeDestroyed y solo si _core.ActiveSession cambio); Start() no la dispara; ningun += ni -= nuevo (16 y 16), ninguna linea handler = ( nueva (16), ningun cambio en Hook, OnIdle, Dispose, BeginDocumentClose ni en las fachadas; EventBridgeCore.cs no cambia y OnDocumentActivated no encola reconciliacion. Si la notificacion tuviera que ser publica: STOP al Coordinator.
           - INV-F1-T3B-05 (decision del Coordinator; delta exacto de WorkspaceController.cs): el controlador se suscribe exactamente a _bridge.Drained y _bridge.ActiveSessionChanged (dos +=, ningun -=) y a ningun evento de AutoCAD; expone una unica notificacion publica en memoria StateChanged (Action sin argumentos, sin tipos de AutoCAD); con ActiveSessionChanged: si PanelVisible llama al _host.RefreshPanel() existente y despues avisa con StateChanged; con el panel oculto no pide reconciliacion; con Drained: conserva _host.OnDrained(outcome) y su contencion y despues avisa con StateChanged; OpenPanel, ShowPanel y HidePanel avisan con StateChanged tras su transicion; cada manejador y cada aviso quedan contenidos con LogFault; sin persistencia, hilos ni temporizadores; ningun otro miembro cambia.
           - INV-F1-T3B-06 (D-04, D-18 P-01b y P-02, INV-01, INV-33): refresco solo por eventos: el host de la paleta se suscribe a WorkspaceController.StateChanged y solo entonces, o ante una interaccion explicita del usuario, el view-model consulta IWorkspaceHostPort.GetSnapshot() y, cuando haga falta, GetMeasurementReport(); ningun DispatcherTimer, System.Threading.Timer, System.Timers, sondeo periodico, Task, async ni hilo en ningun archivo de T3B; ninguna consulta repetida por el paso del tiempo; panel oculto: una notificacion de documento activo no pide reconciliacion ni lee el DWG; panel visible: la activacion pide como maximo una reconciliacion y, terminada, el reposo vuelve a cero trabajo hasta una nueva pista o evento de estado.
           - INV-F1-T3B-07 (D-02, ADR-0006, INV-13): src/RackCad.UI/Workspace/ solo usa contratos y valores neutrales de RackCad.Application.Workspace (IWorkspaceHostPort, WorkspaceHostSnapshot, SelectionContext, WorkspaceMeasurementReport y similares) y delegados inyectados; no nombra RackCad.Plugin, Autodesk (ni en literales), WorkspaceController, WorkspacePaletteHost, el WorkspaceHostPort concreto, WorkspaceSelectionReader ni WorkspaceHostCore; RackCad.UI.csproj sin cambios; namespace RackCad.UI.Workspace de bloque; x:Class y URIs segun UiSystemBoundaryGuardTests; ninguna ventana nueva (censo de ventanas 31).
           - INV-F1-T3B-08 (D-02, D-03, D-06, INV-16, INV-17, INV-36 rotulos): el view-model presenta de forma determinista, para la misma instantanea: NotStarted, NoDocument (estado vacio con acciones deshabilitadas), Ready (sesion activa) y Degraded; la disponibilidad de acciones y su motivo (WorkspaceActionBlock); PICKFIRST desactivado; los cinco contextos (Ninguno; Uno(R) con el RackId exacto de la instantanea; Seleccion con N racks y M otros, rotulada como seleccion actual y nunca como total del proyecto; Sin identidad; Diagnostico); nunca inventa ni estampa un RackId; para presentar no llama a Authorize ni a GetSelectionContext.
           - INV-F1-T3B-09 (D-17, INV-25 panel): toda entrada originada en el panel queda contenida en su frontera: eventos de la UI, llamadas del view-model, manejador de StateChanged del controlador, manejador de visibilidad de la paleta, refresco del host y delegado del interruptor; ninguna excepcion llega al bucle de mensajes de AutoCAD; sin manejadores globales (Dispatcher.UnhandledException, Application.DispatcherUnhandledException, AppDomain.UnhandledException); ante un fallo contenido la UI pasa al estado Error y los comandos clasicos siguen disponibles; en el Plugin los fallos solo se registran con LogFault (RackLog.Exception).
           - INV-F1-T3B-10 (D-17, PV2-13): el estado Error es un estado de presentacion del panel con mensaje y accion de reintento; reintentar vuelve a consultar la instantanea; Error nunca escribe, no cambia sesiones ni pide reconciliacion por si mismo, y se recupera a la presentacion normal.
           - INV-F1-T3B-11 (ADR-0019, convencion de recursos): el control raiz fusiona /RackCad.UI;component/Themes/AppStyles.xaml en sus propios recursos y resuelve sin App.xaml ni recursos de aplicacion; Themes/ y Shell/ sin cambios; URIs con ;component/.
           - INV-F1-T3B-12 (D-01, INV-24, O-10): foco solo con mecanismos publicos: PaletteSet.KeepFocus u otra API publica equivalente; sin Autodesk.AutoCAD.Internal, SetFocusToDwgView, reflexion sobre AutoCAD, IKeyboardInputSink propio ni ganchos Win32; el panel no devuelve el foco al dibujo en F1; en T3B basta la verificacion estatica; el comportamiento real de teclado y foco es de Smoke-1 (S1-06a..h).
           - INV-F1-T3B-13 (D-18, decision del Coordinator): la UI presenta GetMeasurementReport() del puerto inyectado (activado, puntos por secuencia con tiempo, memoria y recuento, y memoria actual del proceso) en solo lectura; las medidas se refrescan solo con StateChanged o con una interaccion explicita; el interruptor llega como delegado neutral inyectado por el host visual hacia WorkspaceController.SetInstrumentationEnabled; IWorkspaceHostPort no cambia; nada se escribe en el registro, en archivos ni en la configuracion; RackLog.cs sin cambios; ningun nombre con Metric, Provider, Bom, ProjectSummary, TotalRacks ni Aggregat en los archivos nuevos.
           - INV-F1-T3B-14 (G1/G2/G3, decision del Coordinator): tests/RackCad.Tests/Workspace/WorkspaceHostSourceGuardTests.cs solo cambia en: G1 - NoHostFileUsesTheInternalAutoCadNamespaceOrAnyUiType admite PaletteSet, AddVisual, Autodesk.AutoCAD.Windows, RackCad.UI y los tipos publicos de host WPF necesarios solo en src/RackCad.Plugin/Workspace/WorkspacePaletteHost.cs, donde sigue prohibiendo Autodesk.AutoCAD.Internal, cualquier Dispatcher y Timer (las demas guardas, sin cambios, siguen prohibiendo alli el trabajo asincrono o en segundo plano y la lectura o escritura del DWG); G2 - NothingOutsideThePluginWorkspaceReachesTheController compara identificadores completos (WorkspaceController, WorkspaceHostPort, WorkspaceSelectionReader, WorkspaceHostCore) para que RackCad.UI pueda nombrar IWorkspaceHostPort sin poder nombrar el controlador ni tipos del Plugin; G3 - la guarda de suscripciones del controlador admite exactamente _bridge.Drained += y _bridge.ActiveSessionChanged += y ningun -=; ninguna prueba se borra, se salta ni se debilita; sin exclusiones por carpeta.
           - INV-F1-T3B-15 (alcance, decision del Coordinator): solo cambian las 8 rutas de AllowedWriteScope; no se reabren EventBridgeCore.cs, WorkspaceHostCore.cs, IWorkspaceHostPort.cs, WorkspaceHostPort.cs ni otro producto de src/RackCad.Application/Workspace/; todas las pruebas existentes de T1, T2 y T3A siguen en verde, y fuera de G1/G2/G3 ninguna cambia (WorkspaceBridgeFacadeSourceGuardTests, WorkspaceBridgeSourceGuardTests y WorkspaceSourceGuardExceptionTests incluidas); sin navegacion, pestanas, borradores, escrituras authored, inventario, persistencia ni conceptos de I-63; fuera de T3B: ayuda de RACKPANEL, indicador de modal en comandos clasicos, liberacion en Terminate(), diff de perfil y Smoke-1.
           - INV-F1-T3B-16 (cobertura RED/GREEN obligatoria): RED de comportamiento real en los dos filtros, que compila y falla por asercion sin pruebas artificiales. UI (clases Workspace* en tests/RackCad.UI.Tests/Workspace/, namespace RackCad.UI.Tests, puerto simulado): estados vacio, documento y sesion; los cinco contextos; estado Error por excepcion contenida (puerto que lanza, delegado que lanza) y reintento; el estado visible no crea trabajo (sin aviso de estado no hay consultas; con un aviso, una consulta de GetSnapshot y, si procede, de GetMeasurementReport; nunca Authorize ni GetSelectionContext); recursos sin App.xaml; presentacion e interruptor de la instrumentacion. Core (tests/RackCad.Tests/Workspace/WorkspacePaletteHostSourceGuardTests.cs, texto de las fuentes): estructura de WorkspacePaletteHost.cs (perezoso, un solo PaletteSet sin toolID ni Load/Save, AddVisual una vez, cerrar = ocultar, suscripcion a StateChanged, contencion, sin Internal, Dispatcher, temporizadores ni DWG); delta del comando; ActiveSessionChanged interno del puente solo desde DocumentActivated y DocumentToBeDestroyed tras el nucleo, sin += ni -= nuevos; relevo del controlador (dos suscripciones, RefreshPanel solo si visible, StateChanged tras drenaje y transiciones); limite de la UI. El GREEN conserva la cadena RED sin debilitar pruebas.
Celda: claude-sonnet-5-5|subagent; modelo claude-sonnet-5-5; effort high (Deep); clase "Implementacion transversal a capas (corta)"; RoutingEnforcement required
Criterios de aceptacion (del paquete aceptado):
           - git diff --name-only BaseSha..CurrentSha stays within the eight AllowedWriteScope entries: new files only under src/RackCad.UI/Workspace/ and tests/RackCad.UI.Tests/Workspace/, the exact new Plugin host and Core source guard files, and only the specified deltas to WorkspacePanelCommand.cs, AutoCadEventBridge.cs, WorkspaceController.cs, and WorkspaceHostSourceGuardTests.cs; no other T1/T2/T3A product or test file changes.
           - In both required projects, RED consists of compilable behavioral tests selected by the contract filters (UI >= 1; Core >= 487) that fail by assertion against a compiling skeleton; there is no artificial failure or compile failure. G1/G2/G3 are in the RED commit and existing tests remain green. GREEN passes without changing the RED test files.
           - WorkspacePaletteHost.cs satisfies INV-F1-T3B-01, 02, 06, 09, and 12 and is covered by the new source guard.
           - WorkspacePanelCommand.cs implements only the INV-F1-T3B-03 command delta and its existing guard passes.
           - AutoCadEventBridge.cs implements internal ActiveSessionChanged per INV-F1-T3B-04 while preserving the public surface and the 16/16/16 subscription and handler counts.
           - WorkspaceController.cs implements the event relay per INV-F1-T3B-05 and G3.
           - The UI root and view-model under src/RackCad.UI/Workspace/ satisfy INV-F1-T3B-06, 07, 08, 10, 11, and 13; they use no timer or polling and are exercised in RackCad.UI.Tests with a simulated port.
           - INV-F1-T3B-09 containment is exercised in UI tests with a throwing port and a throwing delegate, without global exception handlers.
           - G1/G2/G3 are implemented exactly per INV-F1-T3B-14; all other T1/T2/T3A guards and tests remain unchanged and green.
           - On the GREEN commit, both required filters and both complete suites (Core and UI) pass. Release builds of RackCad.Tests, RackCad.UI.Tests, and the Plugin have no new MSB/CS/NU/NETSDK warnings; the Plugin has only the two baseline MSB3277 warnings.
           - CI for the exact CurrentSha has all four required jobs in success.
           - No AutoCAD, DLL loading, profile, registry, TRUSTEDPATHS, SECURELOAD, or ACL operation is performed, and Smoke-1 is not performed.
Evidencia requerida: pruebas (cada corrida filtrada demuestra seleccion > 0):
           - tests/RackCad.UI.Tests/RackCad.UI.Tests.csproj, filtro FullyQualifiedName~RackCad.UI.Tests.Workspace, minimo 1, RED esperado: si
           - tests/RackCad.Tests/RackCad.Tests.csproj, filtro FullyQualifiedName~RackCad.Tests.Workspace, minimo 487, RED esperado: si
           evidencia esperada del contrato: gate/delegation package valido; nc4 segun I-61 en la primera delegacion aplicable; aceptacion A1-A8 antes de Worker; commit RED real en los dos proyectos requeridos (UI y Core); ChainRedSha / ChainRedFiles correctamente acreditados; commit GREEN; pruebas RED y GREEN con seleccion en los dos filtros (UI y Core); worker-handoff valido; push fast-forward; CI exacta del CurrentSha verde (los cuatro jobs, incluidos Build Plugin without AutoCAD y UI Tests); Controller verification valida; diff dentro de AllowedWriteScope (8 entradas); guardas G1/G2/G3 en verde y seleccionadas por el filtro de Core; las demas pruebas de T1/T2/T3A en verde y sin modificar; censos de comandos (38), de ventanas (31) y de ayuda sin cambios; cobertura de INV-F1-T3B-16 en los dos filtros; builds Release de RackCad.Tests, RackCad.UI.Tests y del Plugin con 0 errores; el Plugin solo con los avisos MSB3277 de la linea base y ningun aviso nuevo CS/NU/NETSDK; nc1..nc4 segun README Sec.10; nc2 y nc3 ejecutados con el Controller y conservados; si nc2 no discrimina Scope, sustituto I64-SCOPE-BRIDGE-01 bajo A-4; si nc3 se agota (P-04), sustituto I64-FREETEXT-BRIDGE-01 bajo A-6 solo con todas sus precondiciones; verificadores sin cambios y sin reescribir los controles historicos; REGLA DE nc3 PARA F1-T3B-HOST-UI (decision del Coordinator, explicita en este contrato): oraculo esperado del nc3 sintetico FreeText = fail, FailureClass = FreeText, Classification = EXECUTION_REWORK_REQUIRED, Disposition = REWORK; si el Controller detecta correctamente la violacion FreeText inyectada, informa FailureClass FreeText y conserva las demas relaciones esperadas de la verificacion, pero convierte el REWORK sintetico en STOP solo por aplicar las reglas AttemptsRemaining / CorrectionsAuthorized / S-11 de la tarea real, esa salida de control es INVALID_OUTPUT y no un nc3 PASS; se permiten los dos reintentos normales por INVALID_OUTPUT o transporte; si todas las salidas permitidas se agotan en el mismo modo de fallo, Controller nc3 = NOT PASSED / P-04 y A-6 solo si se cumplen todas sus precondiciones existentes; esta regla no se infiere ni se extiende por analogia a otras tareas; ninguna correccion: un resultado valido que la exija es STOP S-11.
           La sesion, no tu, mantiene evidencia y estado (docs/ esta prohibido) y ejecuta la verificacion y los controles.
No-touch: todo lo prohibido arriba; el checkout principal <checkout-principal-redactado> (nunca escribas alli);
           las ramas de I-52, I-62 e I-65 y main; AutoCAD: el acad.exe que pueda estar abierto pertenece a otra iniciativa:
           no lo cierres, no lo uses, no cargues DLL, no toques perfil, TRUSTEDPATHS, SECURELOAD, ACL ni registro; sin Smoke-1
Condiciones de parada: S-01..C-T3-INSTRUMENTATION del contrato:
           C-01: origin/main cambia antes de escribir (S-13, 16.7)
           C-02: se necesita tocar un archivo fuera de las 8 entradas de AllowedWriteScope, o estas fuera de sus deltas exactos: entre otros EventBridgeCore.cs, WorkspaceHostCore.cs, IWorkspaceHostPort.cs, WorkspaceHostPort.cs u otro producto de src/RackCad.Application/Workspace/, WorkspaceBridgeFacadeSourceGuardTests.cs u otra prueba de T1/T2/T3A, PluginInitializer.cs, un csproj, el sln, Directory.Build.*, un comando existente, RackCommandSupport.cs, RackLog.cs, Themes/, Shell/ o cualquier archivo de src/RackCad.UI fuera de Workspace/ (STOP al Coordinator)
           C-03: se necesita AutoCAD, cargar el DLL, perfil, TRUSTEDPATHS, SECURELOAD, ACL o registro
           C-04: se necesita ampliar scope
           C-05: aparece ambiguedad material del Freeze
           C-06: config.toml fuera del conjunto aceptado A/B/C o con otro inventario de nombres (P-01)
           C-07: hay conflicto de ownership o se necesita un archivo caliente (docs/HANDOFF.md, docs/ROADMAP.md, docs/adr/README.md) o codigo de I-52, I-62 o I-65
           C-08: la tarea deriva hacia navegacion, seleccion o vista (F2), pestanas (F3), borradores o Actualizar (F4), editores (F5), inventario (F6), F1-T3C o Smoke-1
           C-09: se intenta introducir tipos o metricas de I-63
           C-10: cambio observable en comandos existentes, RACKEDITAR, editores o ventanas existentes, o un cambio de los censos de comandos, ventanas o ayuda
           C-11: escritura authored, NOD, XData, archivos, registro, perfil o estado persistido de la paleta
           C-12: uso de Autodesk.AutoCAD.Internal, SetFocusToDwgView, reflexion sobre AutoCAD, IKeyboardInputSink propio o ganchos Win32
           C-13: presupuesto agotado
           C-14: una guarda de T1/T2/T3A necesita un cambio distinto de G1/G2/G3, o una exclusion por carpeta, o se borra, se salta o se debilita una prueba
           C-15: cualquier resultado valido del Worker o del verificador exige correccion, o una verificacion valida devuelve REWORK (CorrectionsAuthorized false; AttemptsRemaining 0; S-11)
           C-16: main se mueve de forma que exige una reconciliacion semantica no resuelta
           C-17: el refresco exigiria un temporizador, sondeo, hilo, Task o async, una suscripcion nueva de AutoCAD, un cambio de EventBridgeCore, encolar en OnDocumentActivated o una notificacion publica nueva en AutoCadEventBridge
           C-18: el control raiz o el view-model necesitan un tipo de AutoCAD o del Plugin, obtener servicios de host de forma estatica o un cambio de IWorkspaceHostPort
           C-19: la contencion exigiria un manejador global de excepciones (Dispatcher.UnhandledException, Application.DispatcherUnhandledException o AppDomain.UnhandledException)
           C-20: el alcance derivado resulta Long-horizon
           C-T3-INSTRUMENTATION: presentar o activar la instrumentacion exigiria escribir en el registro, en archivos o en la configuracion, abusar de la semantica de error o de aviso de RackLog, modificar RackLog.cs o crear una API global de registro
           S-01 y S-10 son frontera (Owner Validation, AutoCAD): no se cruzan y se anotan en KnownLimitations; ante cualquier
           otra, parar sin continuar y devolver el bloqueo estructurado en la entrega (WorkerStatus BLOCKED, Disposition y
           TriggeredStopConditions). No hay presupuesto de correccion: no se repara nada fuera de este paso.
Prohibido declarar: GATE PASS, Candidato, cierre, integracion o aprobacion del Owner;
           ningun termino de AUTOMATION_PLAN Sec.16 "Terminos de gate" (16.10) en el texto libre
Trailer: Co-Authored-By: Claude Sonnet 5.5 <noreply@anthropic.com> (quien ejecuta), nunca de otro participante
Informe esperado: la salida exacta del esquema docs/automation/agent-execution/schemas/worker-handoff.schema.json en
           artifacts/orchestration/I-64/F1-T3B-HOST-UI/3/R20261011T090257Z-3038/worker-handoff.json;
           Worker: commits y push antes de la entrega, y terminar tras escribirla

PERFIL ROUTINE_IMPLEMENTATION
Metodo: cambio minimo que cumple los criterios; nada de refactor oportunista.
1. Lee los archivos del alcance y las pruebas citadas antes de editar.
2. Si la entrega exige RED: commit RED primero (pruebas que fallan por asercion,
   seleccion > 0) y push; despues la implementacion.
3. Ejecuta las pruebas requeridas y las relevantes; anota comando, seleccion y resultado.
4. Commit GREEN con el resumen de estado en el cuerpo y push; luego la entrega.
5. Si algo exige salir del alcance: no lo hagas; parada y bloqueo estructurado.

DELTA DE LA TAREA (paquete aceptado por el Coordinator; primera delegacion de la cadena; sin presupuesto de correccion)
- ChainBaseSha = BaseSha = f3a9f40e0d0057e14a6eb7a6681590b78cecf37f; ChainRedSha = null y ChainRedFiles = []: esta entrega EXIGE RED en LOS DOS proyectos.
  Attempt 3; AttemptsRemaining 0; CorrectionsAuthorized false: esta es la UNICA entrega valida; si algo sale mal, se para.
- Interpretacion del Coordinator en la aceptacion (vinculante): AutoCadEventBridge.ActiveSessionChanged DEBE ser internal, no public;
  no se modifica WorkspaceBridgeFacadeSourceGuardTests.cs; la superficie publica del puente no crece.
- Entorno: Windows. Empieza cada orden de shell con cd al worktree por ruta absoluta (el directorio actual del shell
  puede volver al checkout principal). dotnet: usa SIEMPRE <dotnet-de-usuario-redactado> (Git Bash) o
  <dotnet-de-usuario-redactado> (PowerShell); el dotnet del PATH no sirve.
  Resultados de pruebas (--logger trx y --results-directory) y logs de build solo bajo artifacts/orchestration/I-64/F1-T3B-HOST-UI/3/R20261011T090257Z-3038/tests/<RunRef>/.
  Edita con la herramienta de edicion o de escritura, no con heredocs de shell.
- Antes de editar: git status limpio, git rev-parse HEAD = BaseSha, git ls-remote origin
  refs/heads/architecture/workspace-persistente-rackcad = BaseSha y git rev-parse origin/main (tras git fetch) = MainSha.
  Si no, parada (S-12/S-13/C-01).
- DISENO EXACTO (decision y aceptacion del Coordinator; paquete docs/initiatives/I-64-f1-t3b-gate-contract-package.md, seccion 15,
  solo como referencia; la autoridad es el contrato):
  * src/RackCad.Plugin/Workspace/AutoCadEventBridge.cs (delta exacto): un evento "internal event Action ActiveSessionChanged;" (sin
    argumentos, sin tipos de AutoCAD). Se dispara SOLO dentro de los dos manejadores existentes y dentro de su mismo Contain(...),
    despues de la llamada al nucleo: DocumentActivated tras _core.OnDocumentActivated(...); DocumentToBeDestroyed tras
    _core.OnDocumentToBeDestroyed(...) y SOLO si _core.ActiveSession cambio (compara antes y despues). La linea con "handler = (" de
    cada manejador debe seguir conteniendo Contain( y no contener RequestReconciliation; siguen exactamente 16 lineas "handler = (",
    16 += y 16 -= en el archivo; Start(), Dispose(), Hook, OnIdle y las fachadas no cambian; ningun miembro public nuevo (la
    superficie publica la fija WorkspaceBridgeFacadeSourceGuardTests); EventBridgeCore.cs no cambia. El archivo NO tiene #nullable:
    ninguna anotacion de referencia con ? (CS8632); SessionId? (struct) si es valido.
  * src/RackCad.Plugin/Workspace/WorkspaceController.cs (delta exacto): en el constructor, ademas de "_bridge.Drained += OnDrained;",
    "_bridge.ActiveSessionChanged += ...;" (exactamente dos += en el archivo y ningun -=; el constructor sigue sin contener .Start(,
    OpenPanel ni RequestReconciliation). Una notificacion publica "public event Action StateChanged;" (sin argumentos). Manejador de
    ActiveSessionChanged: si PanelVisible, _host.RefreshPanel(); despues avisa con StateChanged. OnDrained: conserva
    _host.OnDrained(outcome) y su try/catch con LogFault y despues avisa con StateChanged. OpenPanel, ShowPanel y HidePanel avisan con
    StateChanged tras su transicion. Cada aviso y cada manejador contenidos (try/catch (System.Exception ex) con LogFault); sin hilos,
    temporizadores ni persistencia; ningun otro miembro cambia. El archivo NO tiene #nullable.
  * src/RackCad.Plugin/Workspace/WorkspacePaletteHost.cs (nuevo exacto; namespace RackCad.Plugin.Workspace de bloque): posee la
    UNICA PaletteSet del proceso, creada de forma perezosa con WorkspaceLazyInstance<WorkspacePaletteHost> en un campo estatico de
    solo lectura (como el controlador; nunca un constructor estatico); new PaletteSet("RackCad") (constructor solo con nombre, sin
    toolID ni Guid); sin manejadores Load ni Save; cerrar = ocultar; KeepFocus = true (API publica); crea el view-model y el control
    raiz de RackCad.UI.Workspace pasando controller.Port (IWorkspaceHostPort) y el delegado del interruptor (hacia
    WorkspaceController.SetInstrumentationEnabled), y hace AddVisual("Workspace", raiz) una sola vez; se suscribe a la
    visibilidad de la paleta (manejador PRIVADO y contenido: mostrar -> ShowPanel, ocultar -> HidePanel) y a
    WorkspaceController.StateChanged (manejador contenido que pide al view-model consultar el puerto); expone un metodo para
    mostrar la paleta. Sin Autodesk.AutoCAD.Internal, sin Dispatcher (en ningun uso), sin Timer, sin async/Task/hilos, sin
    transacciones ni lectura o escritura del DWG, sin WriteMessage, sin acceso al puente, al nucleo, al lector ni al registro.
    Ninguna firma public o protected con tipos de AutoCAD ni *EventArgs. Fallos solo a "private static void LogFault(string,
    System.Exception)" que llama a RackLog.Exception.
  * src/RackCad.Plugin/Workspace/WorkspacePanelCommand.cs (delta minimo): WorkspaceController.GetOrCreate(); OpenPanel(); obtener o
    crear WorkspacePaletteHost y mostrar la paleta; sin logica de PaletteSet en el comando (no nombres PaletteSet, AddVisual,
    System.Windows, RackCad.UI ni Autodesk.AutoCAD.Windows en este archivo); el mensaje ya no dice que la interfaz no se muestra;
    mismo catch (System.Exception ex) con RackCommandSupport.Report; un solo [CommandMethod("RACKPANEL")], sin alias.
  * src/RackCad.UI/Workspace/ (nuevos; namespace RackCad.UI.Workspace de bloque, en la columna 0): un view-model (base
    RackCad.UI.ObservableObject) cuyo constructor recibe IWorkspaceHostPort y el delegado del interruptor (Action<bool> u otro
    delegado neutral) y nunca obtiene servicios de host de forma estatica; consulta el puerto SOLO cuando el host se lo pide (tras
    StateChanged) o ante una interaccion explicita (reintentar, interruptor), nunca por tiempo; presenta NotStarted, NoDocument
    (estado vacio, acciones deshabilitadas), Ready (sesion activa), Degraded y Error (con mensaje y reintento), la disponibilidad
    de acciones y su motivo, PICKFIRST desactivado, los cinco contextos (Seleccion rotulada como seleccion actual, nunca como total
    del proyecto) y la instrumentacion (GetMeasurementReport) con su interruptor; un UserControl raiz (XAML con x:Class
    RackCad.UI.Workspace.<Archivo> y su .xaml.cs) rotulado "Workspace" que fusiona en SUS recursos
    <ResourceDictionary Source="/RackCad.UI;component/Themes/AppStyles.xaml" /> (sin App.xaml, sin recursos globales). Nada de
    Autodesk (tampoco en literales ni comentarios no // ), RackCad.Plugin, WorkspaceController, WorkspacePaletteHost, el
    WorkspaceHostPort concreto, WorkspaceSelectionReader ni WorkspaceHostCore; sin DispatcherTimer ni otros temporizadores; sin
    Dispatcher.UnhandledException, Application.DispatcherUnhandledException ni AppDomain.UnhandledException. RackCad.UI tiene
    Nullable deshabilitado: no uses ? en tipos de referencia salvo con #nullable enable en el archivo. Usa nombres
    Measurement..., nunca Metric, Provider, Bom, ProjectSummary, TotalRacks ni Aggregat.
  * tests/RackCad.Tests/Workspace/WorkspaceHostSourceGuardTests.cs (solo G1/G2/G3): G1 - en
    NoHostFileUsesTheInternalAutoCadNamespaceOrAnyUiType, para src/RackCad.Plugin/Workspace/WorkspacePaletteHost.cs y solo para el,
    la regla admite PaletteSet, AddVisual, System.Windows, RackCad.UI, Autodesk.AutoCAD.Windows y UserControl, y sigue prohibiendo
    Autodesk.AutoCAD.Internal, Dispatcher y Timer; los demas archivos conservan la regla completa. G2 - en
    NothingOutsideThePluginWorkspaceReachesTheController, identificadores completos:
    \bWorkspaceController\b|\bWorkspaceHostPort\b|\bWorkspaceSelectionReader\b|\bWorkspaceHostCore\b. G3 - en
    TheControllerSubscribesOnlyToTheDrainedEventOfTheBridge: exactamente dos +=, que son _bridge.Drained += y
    _bridge.ActiveSessionChanged +=, y ningun -=. Ninguna prueba se borra ni se salta; sin exclusiones por carpeta.
- Hechos medidos del repositorio que debes respetar (inventario de la sesion; paquete, secciones 3, 4 y 15):
  * tests/RackCad.Tests referencia Domain y Application; tests/RackCad.UI.Tests referencia Domain, Application y UI; ninguno
    referencia el Plugin: el Plugin se comprueba con guardas de texto que leen las fuentes desde la raiz del repositorio (usa los
    ayudantes de tests/RackCad.Tests/Workspace/WorkspaceHostSourceSupport.cs: Code, PluginFiles, ApplicationFiles, AllProduct,
    Count, BodyOf, que ya quitan comentarios). La UI se prueba de verdad con StaTestRunner.Run (tests/RackCad.UI.Tests/StaTestRunner.cs:
    un hilo STA compartido con una Application vacia), construyendo los controles dentro de la llamada; un puerto simulado vive en
    tests/ (las guardas que exigen el puerto solo en el Plugin recorren solo src/).
  * Guardas de T3A sobre TODO archivo de src/RackCad.Plugin/Workspace/ y src/RackCad.Application/Workspace/ (texto sin comentarios):
    sin Task.Run, Task.Factory, new Thread, ThreadPool, Timer, async, await, Parallel., BackgroundWorker ni SynchronizationContext;
    sin System.IO, File., Directory., Registry, UserSettings, XData, NOD ni SetEnvironmentVariable; sin constructor estatico
    "static Workspace...()", ModuleInitializer, IExtensionApplication ni [assembly; sin Guid.NewGuid; sin SetImpliedSelection,
    SetCurrentView, SetSystemVariable, Zoom..., Regen, UpdateScreen, Highlight, SendStringToExecute ni PanTo; RackLog. solo dentro de
    "static void LogFault(...)" y nunca RackLog.Warning; ningun tipo cuyo nombre contenga Log (tambien Dialog, Catalog o Logic);
    nada que contenga Metric, Provider, Aggregat, ProjectSummary, TotalRacks o Bom sin distinguir mayusculas (tambien
    IFormatProvider, AggregateException o Symmetric); ningun campo de tipo Transaction, DBObject, Document, Database, Entity,
    BlockReference, ObjectId ni Editor; StartTransaction y LockDocument solo en using (no los necesitas); solo
    WorkspacePanelCommand.cs usa WriteMessage y [CommandMethod; WorkspaceSelectionReader solo lo nombra el controlador.
  * Guardas de T2 sobre todo archivo de src/RackCad.Plugin/Workspace/: += y -= sobre eventos gobernados de AutoCAD (DocumentActivated,
    Idle, CommandEnded y el resto de la lista) solo en AutoCadEventBridge.cs (PaletteSet.StateChanged no es gobernado); prohibidas
    como palabra completa SetImpliedSelection, SetCurrentView, Zoom, PanTo, UpdateScreen, Regen, SendStringToExecute, StartCommand,
    GetSelection, GetEntity, GetString, GetPoint, GetKeywords, GetInteger, GetDouble, SelectImplied, SelectAll, SelectAtPoint,
    ShowModalDialog, ShowModelessDialog, ShowDialog, MessageBox, GetObject, OpenCloseTransaction, Highlight y Unhighlight; ninguna
    linea public o protected cuya firma nombre Document, DocumentCollection, Editor, Database, Transaction, DBObject, ObjectId,
    Entity, Autodesk o un *EventArgs; sin Save(, SaveAs, Registry., UserSettings, DocumentExtension ni .Commit(; ningun archivo
    del Plugin nombra WorkspaceSessionRegistry; WorkspaceSourceGuardExceptionTests (no se toca) exige que la guarda de T2 siga
    con exactamente sus dos excepciones de archivo exacto. PluginInitializer.cs no se toca.
  * Censos: los de comandos (CustomPropertiesCommandGuardTests y SelectiveEditorOpenTests, total 38, PROHIBIDOS) leen el texto
    crudo de src/RackCad.Plugin/**/*.cs, comentarios incluidos: no escribas [CommandMethod(" en ningun comentario ni literal de un
    archivo nuevo. WindowCensusGuardTests cuenta subclases concretas de Window (31): no crees ninguna Window. RACKPANEL no se anade
    a la ayuda.
  * Frontera UI/AutoCAD (recorren todo src/RackCad.UI): "Autodesk.AutoCAD" ausente de toda linea no comentada con // (literales
    incluidos); \bAutodesk\. ausente del codigo; RackCad.UI.csproj solo con WPF (PROHIBIDO tocarlo); RackCad.UI no referencia
    ningun ensamblado Ac* ni Autodesk.
  * NamespaceFolderGuardTests: src/RackCad.UI/Workspace/X.cs -> "namespace RackCad.UI.Workspace" de bloque, uno solo, en la
    columna 0 (no file-scoped); src/RackCad.Plugin/Workspace/X.cs -> RackCad.Plugin.Workspace igual; las pruebas declaran
    exactamente RackCad.UI.Tests (en tests/RackCad.UI.Tests/Workspace/) o RackCad.Tests (en tests/RackCad.Tests/Workspace/), con
    o sin bloque. UiSystemBoundaryGuardTests: cada .xaml con x:Class = RackCad.UI.Workspace.<Archivo> y su .xaml.cs con ese
    namespace; toda URI /RackCad.UI;component/... apunta a un archivo existente y todo Source="...xaml" lleva ;component/.
  * Filtros: las clases de prueba nuevas se llaman Workspace... (namespace RackCad.UI.Tests -> seleccionadas por
    FullyQualifiedName~RackCad.UI.Tests.Workspace, hoy 0; namespace RackCad.Tests -> FullyQualifiedName~RackCad.Tests.Workspace,
    hoy 486). Cada archivo de prueba nuevo que use anotaciones ? lleva #nullable enable.
  * Recursos: no hay App.xaml; precedente de fusion local: src/RackCad.UI/RackListWindow.xaml; RackCad.UI.Shell.ShellResources
    (internal) expone el diccionario compartido; un DynamicResource no cae al diccionario del tema. No copies
    RackFrameConfiguratorWindow.Dispatcher.UnhandledException (manejador global prohibido).
  * El job de CI "Build Plugin without AutoCAD" compila el Plugin en Release con AutoCAD.NET 25.0.1 y falla ante cualquier aviso
    MSB/CS/NU/NETSDK. Linea base local medida por la sesion en BaseSha (registro en artifacts/orchestration/I-64/F1-T3B-HOST-UI/3/R20261011T090257Z-3038/baseline/):
    <dotnet-de-usuario-redactado> build src/RackCad.Plugin/RackCad.Plugin.csproj -c Release --no-incremental -v:minimal -o <dir temporal bajo artifacts/orchestration/I-64/F1-T3B-HOST-UI/3/R20261011T090257Z-3038/tests/>
    -> codigo 0, 0 errores y solo 2 avisos MSB3277 (conflicto de versiones de las DLL de AutoCAD de esta estacion; no existen en la
    CI). Tu build del Plugin debe dar 0 errores, 0 avisos CS/NU/NETSDK y ningun aviso MSB distinto de esos MSB3277. Compila SIEMPRE
    el Plugin con -o hacia un directorio temporal bajo artifacts/orchestration/I-64/F1-T3B-HOST-UI/3/R20261011T090257Z-3038/tests/ (nunca su bin/), no lo publiques y no cargues nada en AutoCAD.
- Paso RED (un solo commit, con TODAS las pruebas nuevas de la cadena y G1/G2/G3):
  1. Producto como ESQUELETO COMPILABLE (firmas completas y cuerpos que aun no cumplen el comportamiento congelado, p. ej. valores
     por defecto; nunca NotImplementedException que haga fallar todo por excepcion): view-model y control raiz de la UI, host de la
     paleta, y los deltas del comando, del puente y del controlador en la medida en que las pruebas los necesiten para compilar.
  2. Pruebas xunit nuevas: en tests/RackCad.UI.Tests/Workspace/ (namespace RackCad.UI.Tests) los estados vacio, documento y sesion,
     los cinco contextos, Error y reintento (puerto que lanza, delegado que lanza), la presentacion y el interruptor de la
     instrumentacion, los recursos locales sin App.xaml y la ausencia de consultas al host sin StateChanged ni interaccion
     explicita; en tests/RackCad.Tests/Workspace/WorkspacePaletteHostSourceGuardTests.cs (namespace RackCad.Tests) la estructura de
     propietario unico de la paleta, la ausencia de sondeo y temporizadores, el ActiveSessionChanged internal, el relevo exacto del
     ciclo de vida, las dos suscripciones internas del controlador, la superficie publica del puente sin crecer, la ausencia de
     Autodesk.AutoCAD.Internal y la frontera de la UI. Mas G1/G2/G3 en WorkspaceHostSourceGuardTests.cs.
  3. Corre los dos filtros del contrato: deben compilar y FALLAR POR ASERCION (UI con seleccion >= 1, Core con seleccion >= 487),
     nunca por error de compilacion; las pruebas existentes no modificadas siguen en verde. Commit RED (git add solo de archivos
     del alcance) y git push. Registra cada corrida como Phase RED con TreeSha = el commit RED.
- Desde el commit RED NO modifiques ni anadas archivos bajo tests/ (son RT; 16.8: el GREEN no toca RT). Si una prueba necesitara
  cambio: parada y bloqueo estructurado, no la edites.
- Paso GREEN:
  1. Implementa el comportamiento en los archivos nuevos y en los deltas exactos hasta que los dos filtros queden verdes.
  2. Sobre el commit GREEN (arbol limpio, TreeSha = CurrentSha): los dos filtros (todas en verde, Phase GREEN); las dos suites
     completas tests/RackCad.Tests/RackCad.Tests.csproj y tests/RackCad.UI.Tests/RackCad.UI.Tests.csproj (0 fallidas; Phase
     RELEVANT; incluyen censos y guardas de T1/T2/T3A); dotnet build -c Release --no-incremental de los dos proyectos de prueba con 0
     lineas que casen \bwarning\s+(MSB|CS|NU|NETSDK)\d+\b y 0 errores; y la build local del Plugin descrita arriba (Phase
     RELEVANT). Registra comando, seleccion, resultado y conteos. Si algo falla, no lo arregles con otro commit: parada y bloqueo
     estructurado.
  3. Commit GREEN (git add solo de archivos del alcance) con este cuerpo de resumen de estado, y git push (fast-forward):
     "Estado: F1 en curso; entrega del Worker de F1-T3B-HOST-UI (WorkRunId R20261011T090257Z-3038, intento 3) pendiente de la verificacion
     del Controller y de nc1..nc3. attempts = 3. Owner pendiente: aceptacion del ADR 0047 (propuesto) y Smoke-1 tras el cierre de F1."
- Git: sin --force, sin rebase, sin stash, sin amend, sin checkout de otros SHA, sin tocar otras ramas ni main; exactamente un
  commit RED y un commit GREEN, cada uno con su push.
- No lances subagentes ni procesos en segundo plano que sigan vivos al terminar. Los servidores de compilacion que deje dotnet
  (VBCSCompiler, nodos MSBuild) pueden seguir vivos; no los detengas. No abras ni toques AutoCAD.
- En la entrega y en los mensajes de commit no escribas rutas locales distintas de las del worktree y de artifacts/; nada de
  config.toml.
- Entrega (artifacts/orchestration/I-64/F1-T3B-HOST-UI/3/R20261011T090257Z-3038/worker-handoff.json, UTF-8, valida contra el esquema; compruebala con pwsh Test-Json -SchemaFile):
  Schema rackcad-worker-handoff/v1; TaskId F1-T3B-HOST-UI; RunId R20261011T090257Z-3038; DelegationRunId R20261011T081209Z-deae; Initiative I-64; Gate F1;
  Attempt 3; BaseSha = el de arriba; RedSha = el SHA del commit RED; CurrentSha = el SHA del commit GREEN (= HEAD = remoto);
  Branch y Worktree reales; Pushed true; FilesChanged = git diff --name-only BaseSha..CurrentSha; TestsExecuted y TestResults de
  cada corrida (RunRef unico, Command exacto, Phase RED/GREEN/RELEVANT, TreeSha = commit probado, ResultsFile = TRX o log, o null;
  FailedTests con los nombres reales); Worker = { Provider "Anthropic", ModelRequested "claude-sonnet-5-5", EffortRequested
  "high", Trailer "Co-Authored-By: Claude Sonnet 5.5 <noreply@anthropic.com>" }; WorkerStatus IMPLEMENTATION_COMPLETE, PARTIAL o
  BLOCKED; Disposition NONE salvo parada; listas vacias si no hay nada.
- Tope: 60 minutos (AUTOMATION_PLAN 16.4). Al terminar, tu respuesta final es una linea con WorkerStatus, RedSha y CurrentSha.
