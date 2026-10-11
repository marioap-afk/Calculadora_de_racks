I-64 - DELEGACION F1-T3A-HOST-CORE - Worker, perfil ROUTINE_IMPLEMENTATION (primera delegacion de la cadena; unica entrega valida autorizada)
Identidad: unidad I-64; gate F1; tarea F1-T3A-HOST-CORE; intento 3; RunId R20261011T050726Z-0829;
           DelegationRunId R20261011T045535Z-0140; AuthorityRevision ee6766684f560c5128f0cc6af9c5d091de6e8353;
           MainSha bb0d5522e8411f66a51fdfb3f1f0d0514b737453; BaseSha ee6766684f560c5128f0cc6af9c5d091de6e8353;
           rama architecture/workspace-persistente-rackcad;
           worktree <worktree-local-redactado>;
           Owner subagent:R20261011T045535Z-0140
Autoridades (leer desde Git, no copiar; ruta | seccion | clase):
           - docs/initiatives/I-64-proposal-v5.md | documento completo | UNIT_DOC
           - docs/initiatives/I-64-discovery.md | documento completo | UNIT_DOC
           - docs/initiatives/I-64-workspace-persistente-rackcad.md | documento completo | UNIT_DOC
           - docs/initiatives/I-64-A-2.md | documento completo | UNIT_DOC
           - docs/initiatives/I-64-A-4.md | documento completo | UNIT_DOC
           - docs/initiatives/I-64-A-5.md | documento completo | UNIT_DOC
           - docs/initiatives/I-64-A-6.md | documento completo | UNIT_DOC
           - docs/automation/evidence/I-64-evidence.md | ## 18. Consensus Freeze (I64-CONSENSUS-V5-01) y cierre de F0 | UNIT_DOC
           - docs/automation/evidence/I-64-evidence.md | ## 46. F1-T2-BRIDGE - A-6 activa, I64-FREETEXT-BRIDGE-01 PASS y cierre documental de T2 | UNIT_DOC
           - docs/automation/evidence/I-64-evidence.md | ## 47. F1-T2-BRIDGE - Revision del cierre por el Coordinator: PASS; correccion documental del estado | UNIT_DOC
           - docs/automation/evidence/I-64-evidence.md | ## 48. F1-T3-HOST - Sonda del Controller con el CLI vigente y paquete del contrato de gate (NO emitido) | UNIT_DOC
           - docs/automation/evidence/I-64-evidence.md | ## 49. F1-T3-HOST - Decision del Coordinator: descomposicion en F1-T3A-HOST-CORE y F1-T3B-HOST-UI | UNIT_DOC
           - docs/automation/evidence/I-64-evidence.md | ## 50. F1-T3A-HOST-CORE - Contrato y planificacion, aceptacion, STOP previo al Worker y correccion de alcance | UNIT_DOC
           - docs/automation/state/I-64.yml | documento completo | UNIT_DOC
           - docs/initiatives/I-64-f1-gate-contract-proposal.md | documento completo | UNIT_DOC
           - docs/initiatives/I-64-f1-t3-gate-contract-package.md | documento completo | UNIT_DOC
           - docs/automation/evidence/I-64-pilot/F1-T3A-HOST-CORE/coordinator-decision-t3-decomposition.md | documento completo | UNIT_DOC
           - docs/automation/evidence/I-64-pilot/F1-T3A-HOST-CORE/coordinator-scope-correction.md | documento completo | UNIT_DOC
           - docs/adr/0047-workspace-persistente-modeless-rackcad.md | documento completo | UNIT_DOC
           - AGENTS.md | documento completo | EXTERNAL
           - docs/WORKFLOW.md | documento completo | EXTERNAL
           - docs/INITIATIVE_LIFECYCLE.md | documento completo | EXTERNAL
           - docs/AUTOMATION_PLAN.md | ## 16. Ejecucion delegada bajo orden del Coordinator | EXTERNAL
           - docs/automation/agent-execution/README.md | documento completo | EXTERNAL
           - docs/automation/agent-execution/routing.md | documento completo | EXTERNAL
           - docs/adr/0006-autocad-solo-en-plugin.md | documento completo | EXTERNAL
           - docs/adr/0019-shell-visual-de-editores-por-composicion.md | documento completo | EXTERNAL
           - docs/adr/0029-contrato-funcional-comun-de-ventanas-wpf.md | documento completo | EXTERNAL
           - docs/adr/0044-hechos-neutrales-de-vistas-compartidas.md | documento completo | EXTERNAL
           - docs/initiatives/I-55-proposal-v5.md | ### 4.4 La transaccion la posee `Mutate(units)` (AR4-19) | EXTERNAL
           - docs/initiatives/I-55-proposal-v5.md | ### 4.5 Capas bloqueadas (AR4-20) | EXTERNAL
           - docs/initiatives/I-55-proposal-v5.md | ### 4.6 R-25: la primera colocacion sin supervivientes (AR4-21) | EXTERNAL
           - docs/initiatives/I-55-proposal-v5.md | ### 4.7 Un solo barrido (AR4-42) | EXTERNAL
           - docs/initiatives/I-55-proposal-v5.md | ### 4.8 Estados e informe (AR4-40) | EXTERNAL
           - docs/initiatives/I-55-proposal-v5.md | ### 4.9 Guardas caller-owned (AR4-43) | EXTERNAL
           - docs/initiatives/I-55-proposal-v5.md | ### 4.10 Cuenta completa de transacciones (AR4-44) | EXTERNAL
           - docs/automation/agent-execution/model-catalog.md | documento completo | EXTERNAL
           vigencia: AuthorityRevision para la unidad (UNIT_DOC), MainSha para lo demas (EXTERNAL)
           Los valores de seccion son ASCII (sin diacriticos); en Git los encabezados conservan sus tildes.
Objetivo (texto literal del contrato, linea a linea, sin lineas en blanco):
           Implement F1-T3A-HOST-CORE: the non-visual, testable host foundation of the
           F1 Workspace panel, without PaletteSet, without WPF and without running
           AutoCAD. Required behavior includes:
           * IWorkspaceHostPort (new file in src/RackCad.Application/Workspace/),
           implemented in src/RackCad.Plugin/Workspace/, keys and values only;
           * one lazy Workspace controller per process in src/RackCad.Plugin/Workspace/;
           * the RACKPANEL command in exactly src/RackCad.Plugin/Workspace/WorkspacePanelCommand.cs;
           * starting the existing T2 bridge lazily from the controller;
           * active-session binding and SessionId authorization of every request;
           * the fine selection reader in exactly src/RackCad.Plugin/Workspace/WorkspaceSelectionReader.cs,
           used only from the drain;
           * SelectionContext.Classify / WorkspaceSession.ApplyContext path;
           * panel-visible policy and pure host policies;
           * in-memory instrumentation toggle, timing and process-memory collection
           and structured measurement points (RackLog.cs unchanged);
           * source guards and the two command censuses (+RACKPANEL, 37 -> 38);
           * scope correction: thin additive facades on the single existing core,
           exactly in src/RackCad.Application/Workspace/EventBridgeCore.cs (Authorize,
           RequestReconciliation) and src/RackCad.Plugin/Workspace/AutoCadEventBridge.cs
           (ActiveSession, TryGetSession, Authorize, RequestReconciliation); the T3A
           controller decides when to request reconciliation.
           No PaletteSet, no WPF, no RackCad.UI change, no authored writes, no
           selection or view writes, no persistence.
           T3A is a new chain.
           Therefore:
           ChainBaseSha = first T3A BaseSha
           ChainRedSha = null
           ChainRedFiles = []
           A real RED is mandatory.
           RED must be behavioral and compilable.
           No artificial compile failure.
           The RED must demonstrate at least one frozen T3A behavior not yet
           implemented.
           GREEN must not modify the RED test files after the RED commit contrary to
           16.8.
Alcance: permitido EXACTAMENTE src/RackCad.Application/Workspace/, src/RackCad.Plugin/Workspace/, tests/RackCad.Tests/Workspace/, src/RackCad.Application/Workspace/EventBridgeCore.cs, src/RackCad.Plugin/Workspace/AutoCadEventBridge.cs, tests/RackCad.Tests/CustomPropertiesCommandGuardTests.cs, tests/RackCad.Tests/SelectiveEditorOpenTests.cs (archivo exacto o prefijo terminado en /);
           prohibido docs/, .github/, assets/, eng/, deploy/, tools/, global.json, Directory.Build.props, Directory.Build.targets, RackCad.sln, src/RackCad.Domain/, src/RackCad.UI/, tests/RackCad.UI.Tests/, src/RackCad.Application/RackCad.Application.csproj, src/RackCad.Application/BlockNaming.cs, src/RackCad.Application/Bom/, src/RackCad.Application/Catalogs/, src/RackCad.Application/ComputedParameters/, src/RackCad.Application/CustomProperties/, src/RackCad.Application/Diagnostics/, src/RackCad.Application/Drawing/, src/RackCad.Application/Expressions/, src/RackCad.Application/Formatting/, src/RackCad.Application/Geometry/, src/RackCad.Application/Layout/, src/RackCad.Application/Persistence/, src/RackCad.Application/ProjectVariables/, src/RackCad.Application/RackFrames/, src/RackCad.Application/Settings/, src/RackCad.Application/StructuralSections/, src/RackCad.Application/Systems/, src/RackCad.Application/Units/, src/RackCad.Application/Views/, src/RackCad.Application/Workspace/BridgeOutcomes.cs, src/RackCad.Application/Workspace/BridgeSource.cs, src/RackCad.Application/Workspace/Hints.cs, src/RackCad.Application/Workspace/IHostEventSource.cs, src/RackCad.Application/Workspace/SelectionContext.cs, src/RackCad.Application/Workspace/SessionId.cs, src/RackCad.Application/Workspace/WorkspaceSession.cs, src/RackCad.Application/Workspace/WorkspaceSessionRegistry.cs, src/RackCad.Plugin/CustomPropertiesData.cs, src/RackCad.Plugin/CustomPropertiesExecutor.cs, src/RackCad.Plugin/InDocumentTransaction.cs, src/RackCad.Plugin/LayerHelper.cs, src/RackCad.Plugin/PluginInitializer.cs, src/RackCad.Plugin/ProjectVariableMutationExecutor.cs, src/RackCad.Plugin/ProjectVariablesData.cs, src/RackCad.Plugin/ProjectVariablesRegistry.cs, src/RackCad.Plugin/RackAyudaCommands.cs, src/RackCad.Plugin/RackBlockFinder.cs, src/RackCad.Plugin/RackCabeceraCommands.cs, src/RackCad.Plugin/RackCad.Plugin.csproj, src/RackCad.Plugin/RackCamaCommands.cs, src/RackCad.Plugin/RackCantileverCommands.cs, src/RackCad.Plugin/RackCatalogLoader.cs, src/RackCad.Plugin/RackCloner.cs, src/RackCad.Plugin/RackCommandSupport.cs, src/RackCad.Plugin/RackDinamicoCommands.cs, src/RackCad.Plugin/RackDuplicarCommands.cs, src/RackCad.Plugin/RackEnvelopeRestamp.cs, src/RackCad.Plugin/RackInventarioCommands.BomTotal.cs, src/RackCad.Plugin/RackInventarioCommands.cs, src/RackCad.Plugin/RackLayoutCommands.Fill.cs, src/RackCad.Plugin/RackLayoutCommands.cs, src/RackCad.Plugin/RackMenuCommands.cs, src/RackCad.Plugin/RackPropiedadesCommands.cs, src/RackCad.Plugin/RackProyectarCommands.cs, src/RackCad.Plugin/RackPushBackCommands.cs, src/RackCad.Plugin/RackSeccionCommands.cs, src/RackCad.Plugin/RackSelectivoCommands.cs, src/RackCad.Plugin/RackSelectivoInsertIntegration.cs, src/RackCad.Plugin/RackUnitsGuard.cs, src/RackCad.Plugin/RackVariablesCommands.cs, src/RackCad.Plugin/StructuralSectionCatalogAccess.cs, src/RackCad.Plugin/StructuralSectionCommandFlow.cs, src/RackCad.Plugin/Drawing/, src/RackCad.Plugin/KindHandlers/, src/RackCad.Plugin/Systems/, src/RackCad.Plugin/Views/, tests/RackCad.Tests/RackCad.Tests.csproj, tests/RackCad.Tests/NamespaceFolderGuardTests.cs, tests/RackCad.Tests/Workspace/WorkspaceBridgeDegradedModeTests.cs, tests/RackCad.Tests/Workspace/WorkspaceBridgeHintAndDrainTests.cs, tests/RackCad.Tests/Workspace/WorkspaceBridgeSubscriptionLifecycleTests.cs, tests/RackCad.Tests/Workspace/WorkspaceBridgeTestSupport.cs, tests/RackCad.Tests/Workspace/WorkspaceHintDrainTests.cs, tests/RackCad.Tests/Workspace/WorkspaceModelBoundaryTests.cs, tests/RackCad.Tests/Workspace/WorkspaceSelectionContextTests.cs, tests/RackCad.Tests/Workspace/WorkspaceSessionTests.cs.
           En src/RackCad.Application/Workspace/ y src/RackCad.Plugin/Workspace/ solo se anaden archivos NUEVOS, salvo los deltas
           exactos de EventBridgeCore.cs y AutoCadEventBridge.cs (INV-F1-T3A-19 y 20); los otros 8 archivos de producto de T1/T2
           no se modifican. De las pruebas existentes solo cambian WorkspaceBridgeSourceGuardTests.cs (las dos excepciones de archivo
           exacto) y los dos censos (+RACKPANEL, 37 -> 38).
Invariantes del Freeze (vinculantes en su totalidad):
           - INV-F1-T3A-01 (D-02, P-01, INV-02): un unico controlador de Workspace por proceso, en un archivo nuevo de src/RackCad.Plugin/Workspace/, creado de forma perezosa por el primer RACKPANEL; antes, cero suscripciones y cero trabajo; nada se crea ni se suscribe al cargar el plugin (PluginInitializer.cs intacto); RACKPANEL repetido no crea un segundo controlador ni duplica el puente ni sus suscripciones.
           - INV-F1-T3A-02 (D-01, D-02, decision del Coordinator): el comando RACKPANEL vive EXACTAMENTE en src/RackCad.Plugin/Workspace/WorkspacePanelCommand.cs, con un unico [CommandMethod("RACKPANEL")], sin alias y sin ningun otro CommandMethod en ese archivo ni en otro archivo nuevo; en T3A solo crea u obtiene el controlador y arranca de forma perezosa el puente de T2 (AutoCadEventBridge.Start); no crea PaletteSet ni WPF (T3B).
           - INV-F1-T3A-03 (D-02, INV-14, INV-22): IWorkspaceHostPort se declara en un archivo nuevo de src/RackCad.Application/Workspace/ y se implementa en src/RackCad.Plugin/Workspace/; solo claves y valores: ningun tipo de AutoCAD (Document, Database, Editor, Transaction, DBObject, ObjectId, Entity, Autodesk.*) ni de WPF; toda peticion lleva su SessionId y se autoriza con la fachada Authorize del puente, que delega en EventBridgeCore.Authorize y este en WorkspaceSessionRegistry.Authorize del unico registro: si el documento que la ejecuta es otro, o la sesion de origen esta destruida, se rechaza sin efectos.
           - INV-F1-T3A-04 (INV-21, C-T2-04): en F1 el puerto no expone ninguna operacion de escritura (authored, NOD, XData, seleccion, vista, persistencia ni archivo); una guarda de fuente o de reflexion falla si el puerto expone una escritura.
           - INV-F1-T3A-05 (D-02, D-03, O-17): vinculo con la sesion activa: el controlador obtiene la sesion activa y la WorkspaceSession solo a traves de las fachadas ActiveSession y TryGetSession del unico AutoCadEventBridge (que delegan en EventBridgeCore.ActiveSession y TryGetSession) y la expone por el puerto sin tipos de AutoCAD; sin documento abierto: estado vacio con acciones deshabilitadas y solo las suscripciones de la coleccion de documentos del puente de T2.
           - INV-F1-T3A-06 (D-04 P-04, D-06, P-08, decision del Coordinator): la seleccion fina se lee SOLO desde el drenaje (evento Drained del puente de T2) y EXACTAMENTE en src/RackCad.Plugin/Workspace/WorkspaceSelectionReader.cs: seleccion implicita del documento activo en solo lectura (Editor.SelectImplied), definicion de cada referencia de bloque seleccionada leida con GetObject en OpenMode.ForRead dentro de una transaccion corta creada en using y con el documento bloqueado en using (contexto de aplicacion, en reposo, sin modal RackCad), sobre leido con el lector integrado (RackBlockData.Read y RackEmbedStore.Deserialize, sin codec nuevo), sin Commit, con cache por definicion que invalidan las pistas; trabajo proporcional a las referencias seleccionadas; sin enumeracion global del dibujo; ningun manejador ordinario lee la base de datos.
           - INV-F1-T3A-07 (D-06, INV-16, INV-17): el contexto se calcula con SelectionContext.Classify y se aplica con WorkspaceSession.ApplyContext: Uno(R) solo desde un sobre interpretable con Id no vacio igual a R; sin Id (IsNullOrWhiteSpace) -> Sin identidad; ilegible o de version futura -> Diagnostico (el sondeo nunca da identidad); referencias anidadas, xref y espacio papel cuentan como no rack; nunca se estampa ni se inventa RackId; PICKFIRST=0 se refleja como estado y no se cambia.
           - INV-F1-T3A-08 (INV-03): ningun evento ni cambio de contexto escribe seleccion o vista: ningun archivo de src/RackCad.Plugin/Workspace/ llama a SetImpliedSelection, Zoom, Regen, UpdateScreen, Highlight, SendStringToExecute ni similares.
           - INV-F1-T3A-09 (INV-01, INV-33, P-01b, P-02, D-02): politica de panel visible: panel oculto -> cero lecturas del DWG mientras las pistas siguen encolandose; cuando el ciclo de vida del panel exige una reconciliacion inicial o de refresco (primer RACKPANEL, mostrar), el controlador de T3A llama a RequestReconciliation y la reconciliacion ocurre en el siguiente reposo por el drenaje normal, nunca de forma sincrona; panel visible y ocioso -> cero lecturas sin pistas pendientes. Probado en Core con un host y un lector simulados.
           - INV-F1-T3A-10 (D-02): politicas puras del host en archivos nuevos de Application, sin AutoCAD: condiciones de drenaje y de lectura, habilitacion de acciones (un comando de AutoCAD activo o un modal RackCad deshabilitan las acciones que leen o escriben el DWG), estado vacio y estado degradado; ningun hilo de fondo toca AutoCAD.
           - INV-F1-T3A-11 (D-18, decision del Coordinator): instrumentacion solo en memoria: interruptor del panel en memoria, recogida de tiempos y de memoria del proceso y puntos de medida estructurados; RackLog.cs no se modifica; ninguna medida normal se registra como excepcion ni como aviso; no se crea una API global de registro ni archivos aparte del registro; si hiciera falta registrar las medidas en el registro: STOP C-T3-INSTRUMENTATION.
           - INV-F1-T3A-12 (INV-14): StartTransaction, StartOpenCloseTransaction y LockDocument solo dentro de using; ningun campo ni propiedad de tipo Transaction, DBObject, Document, Entity ni BlockReference; la API publica de los archivos nuevos del panel no expone tipos de AutoCAD; en Debug, TopTransaction == null al terminar cada operacion y cada drenaje.
           - INV-F1-T3A-13 (INV-21, D-22): nada se persiste: ninguna escritura authored, de NOD ni de XData; nada en el registro de Windows, el perfil ni archivos.
           - INV-F1-T3A-14 (guarda de T2, decision del Coordinator): tests/RackCad.Tests/Workspace/WorkspaceBridgeSourceGuardTests.cs solo se refina con dos excepciones de ARCHIVO EXACTO, sin exclusiones de carpeta: [CommandMethod("RACKPANEL")] solo en src/RackCad.Plugin/Workspace/WorkspacePanelCommand.cs; las APIs de lectura de D-06 (SelectImplied, GetObject en lectura y las del lector integrado) solo en src/RackCad.Plugin/Workspace/WorkspaceSelectionReader.cs. La guarda sigue prohibiendo: SetImpliedSelection; mutacion de vista o de seleccion; Autodesk.AutoCAD.Internal; nuevas suscripciones a eventos gobernados fuera del puente central; lecturas de la base de datos en manejadores ordinarios; persistencia; integracion en PluginInitializer.cs; cambios de comandos o editores fuera de RACKPANEL. Ninguna prueba existente se borra ni se debilita fuera de esas dos excepciones.
           - INV-F1-T3A-15 (censos, decision del Coordinator): tests/RackCad.Tests/CustomPropertiesCommandGuardTests.cs y tests/RackCad.Tests/SelectiveEditorOpenTests.cs solo cambian para anadir RACKPANEL al censo y pasar el total exacto de 37 a 38; ningun otro cambio de censo.
           - INV-F1-T3A-16 (D-16, M-03, INV-19): sin cambio observable en los comandos existentes, RACKEDITAR ni los editores.
           - INV-F1-T3A-17 (ADR-0006, T1/T2, correccion de alcance): AutoCAD solo en el Plugin; de los 10 archivos de producto de T1/T2 solo cambian src/RackCad.Application/Workspace/EventBridgeCore.cs y src/RackCad.Plugin/Workspace/AutoCadEventBridge.cs, y solo con los deltas de INV-F1-T3A-19 y 20; los otros 8 no cambian; todas las pruebas existentes de T1/T2 siguen en verde; sin PaletteSet, WPF ni cambios en RackCad.UI en T3A.
           - INV-F1-T3A-18 (MASTER-I63-I64-02): sin tipos ni conceptos de I-63 (ComputedParameter, Metric, Provider, ProjectSummary, TotalRacks, Aggregat, Bom), tampoco en los nombres de la instrumentacion.
           - INV-F1-T3A-19 (correccion de alcance, delta exacto de src/RackCad.Application/Workspace/EventBridgeCore.cs): solo se anaden dos miembros publicos: (1) RequestDecision Authorize(WorkspaceRequest request, long executingNativeInstanceId), que delega directamente en WorkspaceSessionRegistry.Authorize del registro existente; (2) void RequestReconciliation(), que actua solo sobre la sesion activa, es no-op si no hay sesion activa o si esta degradada o retirada, encola HintKind.ImpliedSelectionChanged en esa sesion y sincroniza Idle con la politica existente; nunca drena de forma sincrona, nunca lee ni escribe el DWG, nunca llama a ApplyContext, nunca crea otra sesion, nunca anade suscripciones, nunca persiste. ActiveSession y TryGetSession no se redisenan; no se expone el WorkspaceSessionRegistry ni colecciones o estado internos mutables; ningun miembro existente (Start, Stop, OnDocumentCreated, OnDocumentActivated, OnDocumentToBeDestroyed, OnSourceEvent, OnBeginDocumentClose, OnIdle, ReportFault, la politica de Idle) cambia; Start y OnDocumentActivated NO encolan reconciliacion: T2 = mecanismo de eventos y planificacion, T3 = politica del ciclo de vida del panel.
           - INV-F1-T3A-20 (correccion de alcance, delta exacto de src/RackCad.Plugin/Workspace/AutoCadEventBridge.cs): solo se anaden fachadas finas publicas ActiveSession, TryGetSession(...), Authorize(...) y RequestReconciliation() que delegan en el unico EventBridgeCore existente; ningun segundo EventBridgeCore ni WorkspaceSessionRegistry; ninguna suscripcion, manejador, += ni -= nuevo; Hook(...), la propiedad de los eventos, los manejadores ordinarios (solo encolan), OnIdle, BeginDocumentClose, Start y Dispose no cambian; ninguna lectura ni escritura del DWG en el puente; ninguna escritura de seleccion o vista; ninguna persistencia; sin Autodesk.AutoCAD.Internal.
           - INV-F1-T3A-21 (correccion de alcance, cobertura RED/GREEN obligatoria): pruebas Core seleccionadas por el filtro que cubren (1) EventBridgeCore.RequestReconciliation con sesion activa sincronizada: encola exactamente la pista de reconciliacion prevista, deja Idle pendiente segun la politica y no drena de forma sincrona; (2) sin sesion activa: no-op, sin excepcion y sin adjuntar Idle; (3) sesion degradada o retirada: no-op, sin resurreccion y sin nueva suscripcion de sincronizacion; (4) Authorize: resultado identico a WorkspaceSessionRegistry.Authorize, sesion correcta aceptada, documento ejecutor equivocado rechazado y origen destruido rechazado; (5) guarda de fuente de AutoCadEventBridge.cs: un solo EventBridgeCore, ningun WorkspaceSessionRegistry, ninguna propiedad nueva de += / -= y ninguna lectura ni escritura del DWG introducida en el puente. Todas las pruebas existentes de T2 siguen en verde.
Celda: claude-sonnet-5-5|subagent; modelo claude-sonnet-5-5; effort high (Deep); clase "Implementacion transversal a capas (corta)"; RoutingEnforcement required
Criterios de aceptacion (del paquete aceptado):
           - El diff entre BaseSha y el SHA actual contiene solo archivos nuevos bajo src/RackCad.Application/Workspace/, src/RackCad.Plugin/Workspace/ y tests/RackCad.Tests/Workspace/, mas las modificaciones autorizadas a tests/RackCad.Tests/Workspace/WorkspaceBridgeSourceGuardTests.cs, tests/RackCad.Tests/CustomPropertiesCommandGuardTests.cs, tests/RackCad.Tests/SelectiveEditorOpenTests.cs y las fachadas aditivas exactas en src/RackCad.Application/Workspace/EventBridgeCore.cs y src/RackCad.Plugin/Workspace/AutoCadEventBridge.cs. Los otros ocho archivos de producto T1/T2 y las demas pruebas T1/T2 quedan sin cambios.
           - La fase RED agrega pruebas compilables seleccionadas por el filtro del contrato, con al menos 160 pruebas seleccionadas, que fallan por asercion de un comportamiento congelado de T3A aun no implementado; el fallo no es de compilacion ni artificial. La fase GREEN deja intactos los archivos de prueba RED.
           - RACKPANEL y el lector de seleccion fina viven exactamente en WorkspacePanelCommand.cs y WorkspaceSelectionReader.cs, respectivamente. La guarda T2 permite solo las dos excepciones de archivo exacto del contrato y sigue aplicando sus prohibiciones restantes. Los censos agregan unicamente RACKPANEL y actualizan el total de 37 a 38.
           - Las politicas puras de Application se prueban con host y lector simulados: controlador perezoso unico; RACKPANEL repetido sin duplicados; autorizacion por SessionId y rechazo sin efectos de otra sesion; puerto sin escrituras; panel oculto sin lecturas y reconciliacion al mostrar; ocioso sin trabajo; estado vacio; y mapeo de seleccion a los cinco contextos mediante Classify y ApplyContext.
           - La cobertura obligatoria de INV-F1-T3A-21 queda seleccionada por el filtro: RequestReconciliation con sesion activa, sin sesion activa y con sesion degradada o retirada; Authorize identico al registro; y guarda de fuente de las fachadas del puente. Todas las pruebas existentes de T2 permanecen en verde.
           - La instrumentacion es solo en memoria: interruptor, tiempos, memoria del proceso y puntos de medida estructurados. RackLog.cs no cambia y las medidas normales no se registran como aviso ni excepcion.
           - Se respetan namespaces por carpeta, nombres Workspace para las nuevas clases de prueba, #nullable cuando se usan anotaciones, y las guardas de T1/T2 del contrato.
           - En el commit GREEN pasan el filtro del contrato y la suite Core completa; las builds Release no emiten avisos nuevos MSB, CS, NU o NETSDK; el Plugin compila sin avisos nuevos.
           - La CI del SHA actual tiene en success los cuatro jobs requeridos: Tests (Domain + Application), UI Tests (WPF controls, net8.0-windows), Build UI (WPF, valida API de Application) y Build Plugin without AutoCAD.
           - No se introduce PaletteSet, WPF ni cambios en RackCad.UI; no se ejecuta AutoCAD, no se carga el DLL y no se modifica perfil, registro, TRUSTEDPATHS, SECURELOAD o ACL. No se ejecuta Smoke-1.
Evidencia requerida: pruebas (cada corrida filtrada demuestra seleccion > 0):
           - tests/RackCad.Tests/RackCad.Tests.csproj, filtro FullyQualifiedName~RackCad.Tests.Workspace, minimo 160, RED esperado: si
           evidencia esperada del contrato: gate/delegation package valido; nc4 segun I-61 en la primera delegacion aplicable; aceptacion A1-A8 antes de Worker; commit RED real; ChainRedSha / ChainRedFiles correctamente acreditados; commit GREEN; pruebas RED y GREEN con seleccion; worker-handoff valido; push fast-forward; CI exacta del CurrentSha verde (incluye Build Plugin without AutoCAD); Controller verification valida; diff dentro de AllowedWriteScope; guardas de fuente de INV-02, INV-03, INV-14 e INV-21 (incluida la guarda del puerto) en verde y seleccionadas por el filtro; censos de comandos en verde con RACKPANEL y total 38; cobertura de INV-F1-T3A-21 seleccionada por el filtro (RequestReconciliation con sesion activa, sin sesion activa y con sesion degradada o retirada; Authorize; guarda de fuente de las fachadas de AutoCadEventBridge) y todas las pruebas existentes de T1/T2 en verde; nc1..nc4 segun README Sec.10; nc2 y nc3 ejecutados con el Controller y conservados; si nc2 no discrimina Scope, sustituto I64-SCOPE-BRIDGE-01 bajo A-4; si nc3 se agota (P-04), sustituto I64-FREETEXT-BRIDGE-01 bajo A-6; verificadores sin cambios y sin reescribir los controles historicos; ninguna correccion: un resultado valido que la exija es STOP S-11.
           La sesion, no tu, mantiene evidencia y estado (docs/ esta prohibido).
No-touch: todo lo prohibido arriba; el checkout principal <checkout-principal-redactado> (nunca escribas alli);
           las ramas de I-52, I-62 e I-65 y main; AutoCAD: el acad.exe que pueda estar abierto pertenece a otra iniciativa:
           no lo cierres, no lo uses, no cargues DLL, no toques perfil, TRUSTEDPATHS, SECURELOAD, ACL ni registro; sin Smoke-1
Condiciones de parada: S-01..C-T3-INSTRUMENTATION del contrato:
           C-01: origin/main cambia antes de escribir (S-13, 16.7)
           C-02: se necesita tocar un archivo de producto de T1/T2 distinto de EventBridgeCore.cs y AutoCadEventBridge.cs, o estos fuera de sus deltas exactos, PluginInitializer.cs, un csproj, el sln, un comando existente, RackBlockFinder.cs, RackCommandSupport.cs, RackLog.cs, src/RackCad.UI o src/RackCad.Domain
           C-03: se necesita AutoCAD, cargar el DLL, perfil, TRUSTEDPATHS, SECURELOAD, ACL o registro
           C-04: se necesita ampliar scope
           C-05: aparece ambiguedad material del Freeze
           C-06: config.toml fuera del conjunto aceptado A/B/C o con otro inventario de nombres (P-01)
           C-07: hay conflicto de ownership o se necesita un archivo caliente (docs/HANDOFF.md, docs/ROADMAP.md, docs/adr/README.md) o codigo de I-52, I-62 o I-65
           C-08: la tarea deriva hacia T3B (PaletteSet, WPF, RackCad.UI, foco o contencion de UI) o hacia F2-F7
           C-09: se intenta introducir tipos o metricas de I-63
           C-10: se intenta modificar comandos existentes, RACKEDITAR o editores
           C-11: escritura authored, NOD o XData
           C-12: uso de Autodesk.AutoCAD.Internal
           C-13: presupuesto agotado
           C-14: la guarda de T2 o los censos de comandos necesitan un cambio distinto de las dos excepciones de archivo exacto y de +RACKPANEL / 37 -> 38
           C-15: cualquier resultado valido del Worker o del verificador exige correccion, o una verificacion valida devuelve REWORK (CorrectionsAuthorized false; AttemptsRemaining 0; S-11)
           C-16: main se mueve de forma que exige una reconciliacion semantica no resuelta
           C-17: EventBridgeCore.cs o AutoCadEventBridge.cs necesitan un cambio distinto de las fachadas aditivas autorizadas, o la reconciliacion exigiria cambiar Start, OnDocumentActivated, OnIdle, Hook o los manejadores, un segundo nucleo o registro, o drenar de forma sincrona
           C-T3-INSTRUMENTATION: registrar tiempos o memoria exigiria abusar de la semantica de error o de aviso de RackLog, modificar RackLog.cs o crear una API global de registro
           S-01 y S-10 son frontera (Owner Validation, AutoCAD): no se cruzan y se anotan en KnownLimitations; ante cualquier
           otra, parar sin continuar y devolver el bloqueo estructurado en la entrega (WorkerStatus BLOCKED, Disposition y
           TriggeredStopConditions). No hay presupuesto de correccion: no se repara nada fuera de este paso.
Prohibido declarar: GATE PASS, Candidato, cierre, integracion o aprobacion del Owner;
           ningun termino de AUTOMATION_PLAN Sec.16 "Terminos de gate" (16.10) en el texto libre
Trailer: Co-Authored-By: Claude Sonnet 5.5 <noreply@anthropic.com> (quien ejecuta), nunca de otro participante
Informe esperado: la salida exacta del esquema docs/automation/agent-execution/schemas/worker-handoff.schema.json en
           artifacts/orchestration/I-64/F1-T3A-HOST-CORE/3/R20261011T050726Z-0829/worker-handoff.json;
           Worker: commits y push antes de la entrega, y terminar tras escribirla

PERFIL ROUTINE_IMPLEMENTATION
Metodo: cambio minimo que cumple los criterios; nada de refactor oportunista.
1. Lee los archivos del alcance y las pruebas citadas antes de editar.
2. Si la entrega exige RED: commit RED primero (pruebas que fallan por asercion,
   seleccion > 0) y push; despues la implementacion.
3. Ejecuta las pruebas requeridas y las relevantes; anota comando, seleccion y resultado.
4. Commit GREEN con el resumen de estado en el cuerpo y push; luego la entrega.
5. Si algo exige salir del alcance: no lo hagas; parada y bloqueo estructurado.

DELTA DE LA TAREA (paquete de reemplazo aceptado por el Coordinator; primera delegacion de la cadena; sin presupuesto de correccion)
- ChainBaseSha = BaseSha = ee6766684f560c5128f0cc6af9c5d091de6e8353; ChainRedSha = null y ChainRedFiles = []: esta entrega EXIGE RED.
  Attempt 3; AttemptsRemaining 0; CorrectionsAuthorized false: esta es la UNICA entrega valida; si algo sale mal, se para.
- Entorno: Windows. Empieza cada orden de shell con cd al worktree por ruta absoluta (el directorio actual del shell
  puede volver al checkout principal). dotnet: usa SIEMPRE <dotnet-de-usuario-redactado> (Git Bash) o
  <dotnet-de-usuario-redactado> (PowerShell); el dotnet del PATH no sirve.
  Resultados de pruebas (--logger trx y --results-directory) y logs de build solo bajo artifacts/orchestration/I-64/F1-T3A-HOST-CORE/3/R20261011T050726Z-0829/tests/<RunRef>/.
  Edita con la herramienta de edicion o de escritura, no con heredocs de shell.
- Antes de editar: git status limpio, git rev-parse HEAD = BaseSha, git ls-remote origin
  refs/heads/architecture/workspace-persistente-rackcad = BaseSha y git rev-parse origin/main (tras git fetch) = MainSha.
  Si no, parada (S-12/S-13/C-01).
- Hechos medidos del repositorio que debes respetar (paquete de T3 Sec.3, evidencia Sec.50):
  * tests/RackCad.Tests solo referencia Domain y Application (no el Plugin ni la UI): la logica comprobable (controlador
    perezoso, politicas del host, puerto, autorizacion, reconciliacion, mapeo de seleccion, instrumentacion en memoria) va en
    tipos NUEVOS y puros de src/RackCad.Application/Workspace/, probados con host y lector simulados; los adaptadores del
    Plugin quedan finos y se comprueban con guardas de texto que leen las fuentes desde la raiz del repositorio (la carpeta que
    contiene RackCad.sln, como hacen WorkspaceModelBoundaryTests y WorkspaceBridgeSourceGuardTests).
  * API existente (solo lectura salvo los deltas autorizados): WorkspaceSessionRegistry (GetOrCreate(long), TryGet, Destroy,
    Authorize(WorkspaceRequest, long)), WorkspaceSession (Id, Selection, EnqueueHint(HintKind), TryDrain(DrainConditions),
    ApplyContext(SelectionContext)), SelectionContext.Classify(IReadOnlyList<DefinitionFact>, int nonRackEntities),
    DefinitionFact(IsInterpretable, EnvelopeId, ProbedId), HintKind, DrainOperation (ReadImplicitSelection,
    RecomputeSelectionContext, InvalidateNavigationCaches, MarkDraftsPossiblyStale), DrainConditions, DrainResult,
    DrainOutcome(SessionId Session, DrainResult Result), SessionId, RackKey, WorkspaceRequest(SessionId Origin), RequestDecision;
    EventBridgeCore (ActiveSession, TryGetSession(SessionId|long), IsDegraded, LiveSessionCount, Start, Stop, OnSourceEvent,
    OnIdle, SetPanelVisible, SetModalRackCadActive; SyncIdle es privado); AutoCadEventBridge (Start, SetPanelVisible,
    SetModalRackCadActive, evento Drained, Dispose; guarda su EventBridgeCore en un campo privado).
  * Deltas exactos autorizados (INV-F1-T3A-19 y 20): en EventBridgeCore SOLO Authorize(WorkspaceRequest request, long
    executingNativeInstanceId) => _registry.Authorize(...) y RequestReconciliation() con la semantica del contrato; en
    AutoCadEventBridge SOLO las fachadas finas ActiveSession, TryGetSession(...), Authorize(...) y RequestReconciliation() sobre el
    unico _core. EventBridgeCore.cs tiene #nullable enable; AutoCadEventBridge.cs NO: alli no uses anotaciones de referencia con
    ? (un out WorkspaceSession? da el aviso CS8632 y rompe el job de CI del Plugin); usa una firma compatible con su contexto
    (por ejemplo out WorkspaceSession session) sin cambiar la semantica. SessionId? (struct) si es valido alli.
  * Lector integrado de sobres (el mismo de RackCommandSupport.PickRackBlock, src/RackCad.Plugin/RackCommandSupport.cs:75):
    RackBlockData.Read(transaction, definitionId) devuelve el JSON del sobre de la definicion y new RackEmbedStore().Deserialize(json)
    el RackEmbedDocument (Id, Kind, View, ...). No crees un codec nuevo. En el lector, la transaccion corta va en using SIN Commit
    (la guarda de T2 prohibe .Commit( en src/RackCad.Plugin/Workspace/) y el bloqueo del documento en using.
  * Guarda de T2 tests/RackCad.Tests/Workspace/WorkspaceBridgeSourceGuardTests.cs (texto sin comentarios, literales incluidos), que
    se aplica a TODO archivo de src/RackCad.Plugin/Workspace/ (incluidos los tuyos): NothingIsHookedWhenThePluginLoads... prohibe
    [CommandMethod, IExtensionApplication, [assembly:, ModuleInitializer; TheBridgeNeverWritesSelectionOrView... prohibe
    SetImpliedSelection, Zoom, Regen, UpdateScreen, SendStringToExecute, GetSelection, GetEntity, SelectImplied, SelectAll,
    MessageBox, ShowDialog, GetObject, OpenCloseTransaction, Highlight, etc.; ThePluginWorkspaceHoldsExactlyOneSubscriberFile... y
    EveryNativeHandlerIsContained (16 lineas "handler = (" en el unico archivo suscriptor) prohiben cualquier += / -= nuevo sobre
    eventos gobernados; ThePluginWorkspaceDeclaresNoFieldOf... prohibe campos o propiedades de tipo Transaction, OpenCloseTransaction,
    DBObject, Document, Entity o BlockReference; ThePublicApiOfTheBridgeExposesNoAutoCadType prohibe lineas public/protected que
    nombren Document, Editor, Database, ObjectId, Entity, Autodesk o *EventArgs; TheBridgeHasNoPersistence prohibe System.IO, File.,
    Microsoft.Win32, Registry., .Commit(, Save(, DocumentExtension; TheBridgeFilesCarry...Concept prohibe ComputedParameter, Metric,
    ProjectSummary, TotalRacks, Aggregat, Provider y Bom (sin distinguir mayusculas) en Plugin/Workspace Y Application/Workspace
    (nombra la instrumentacion con Measurement..., nunca Metric); NoBridgeFileUsesTheInternalAutoCadNamespace prohibe
    Autodesk.AutoCAD.Internal; NoExistingPluginCommandFileReferencesTheWorkspaceBridge prohibe que archivos del Plugin FUERA de
    Workspace/ referencien el Workspace (tus archivos van todos dentro). El refinamiento autorizado de esta guarda es SOLO por
    ARCHIVO EXACTO: [CommandMethod("RACKPANEL")] permitido solo en src/RackCad.Plugin/Workspace/WorkspacePanelCommand.cs; las APIs
    de lectura de D-06 (SelectImplied, GetObject en lectura y las del lector integrado) solo en
    src/RackCad.Plugin/Workspace/WorkspaceSelectionReader.cs; ninguna exclusion de carpeta; ninguna prueba se borra ni se debilita.
    La guarda de fuente de las fachadas de AutoCadEventBridge (cobertura 5 de INV-F1-T3A-21) va en un archivo de prueba NUEVO.
  * Censos de comandos: tests/RackCad.Tests/CustomPropertiesCommandGuardTests.cs (TGrd08: lista exacta Baseline = apertura + G7 +
    G15; anade RACKPANEL como una familia nueva, sin alias, sin tocar el resto) y tests/RackCad.Tests/SelectiveEditorOpenTests.cs
    (GUARDA_EL_CENSO_DE_COMANDOS_NO_CAMBIA: Assert.Equal(37, total) contando "[CommandMethod(" en src/RackCad.Plugin/**/*.cs ->
    38). Precedente: I-55 G15 (35 -> 37).
  * Guardas de T1 (tests/RackCad.Tests/Workspace/WorkspaceModelBoundaryTests.cs) sobre todo src/RackCad.Application/Workspace/
    (texto sin comentarios): prohibido System.IO, File., Directory., StreamWriter, Microsoft.Win32, Registry., UserSettings,
    AtomicFile, Transaction (como SUBCADENA: tampoco en nombres ni comentarios de codigo), LockDocument, Database como palabra,
    Commit(, Autodesk., System.Windows, RackCad.Plugin y RackCad.UI; ningun tipo del namespace RackCad.Application.Workspace puede
    tener en el nombre Tab (tambien casa con Table, Mutable, Database), Subscription, Editor o Draft (sin distinguir mayusculas), ni
    nombre o miembro publico con ProjectSummary, Metric, Provider, TotalRacks, Aggregat o Bom. No evadas las guardas semanticamente.
  * RackLog (src/RackCad.Application/Diagnostics/RackLog.cs, PROHIBIDO) solo tiene Exception y Warning: la instrumentacion de T3A
    es SOLO EN MEMORIA (interruptor, Stopwatch, memoria del proceso, puntos de medida, instantaneas puras expuestas por la
    abstraccion del host); ninguna medida normal como aviso o excepcion; si hiciera falta registrarla: STOP C-T3-INSTRUMENTATION.
  * NamespaceFolderGuardTests: en src/ el namespace de cada archivo es la ruta de su carpeta (src/RackCad.Plugin/Workspace/X.cs ->
    RackCad.Plugin.Workspace; src/RackCad.Application/Workspace/X.cs -> RackCad.Application.Workspace) con exactamente un
    namespace de bloque (namespace X { ... }); en tests/RackCad.Tests el unico namespace es RackCad.Tests (NO
    RackCad.Tests.Workspace). Las clases de prueba nuevas se llaman Workspace... para que las seleccione el filtro (hoy 159).
  * Nullable: cada archivo de prueba nuevo que use anotaciones ? lleva #nullable enable (como las pruebas de T1/T2). Usa catch
    (System.Exception ex) en archivos que importen Autodesk.AutoCAD.Runtime.
  * El job de CI "Build Plugin without AutoCAD" publica el Plugin en Release con referencias NuGet y falla ante cualquier aviso
    MSB/CS/NU/NETSDK. Linea base local medida por la sesion en BaseSha con las DLL de AutoCAD instaladas:
    <dotnet-de-usuario-redactado> build src/RackCad.Plugin/RackCad.Plugin.csproj -c Release --no-incremental -v:minimal -o <dir temporal bajo artifacts/orchestration/I-64/F1-T3A-HOST-CORE/3/R20261011T050726Z-0829/tests/>
    -> codigo 0, 0 errores, "2 Advertencia(s)", ambas MSB3277 (conflicto de versiones de las DLL de AutoCAD de esta estacion; no
    existen en la CI). Tu build del Plugin debe dar 0 errores, 0 avisos CS/NU/NETSDK y ningun aviso MSB distinto de esos MSB3277,
    con el mismo resumen de 2. Compila SIEMPRE el Plugin con -o hacia un directorio temporal bajo artifacts/orchestration/I-64/F1-T3A-HOST-CORE/3/R20261011T050726Z-0829/tests/ (nunca su bin/), no lo
    publiques y no cargues nada en AutoCAD.
  * Ninguna suscripcion ni trabajo al cargar el plugin: PluginInitializer.cs no se toca; el controlador se crea de forma perezosa
    en el primer RACKPANEL (nunca en un inicializador estatico ni en un constructor estatico).
- Paso RED (un solo commit, con TODAS las pruebas de la cadena y TODOS los cambios de pruebas existentes autorizados):
  1. Tipos nuevos de Application como ESQUELETO COMPILABLE (firmas completas y cuerpos que aun no cumplen el comportamiento
     congelado, p. ej. valores por defecto; nunca NotImplementedException que haga fallar todo por excepcion); los miembros
     nuevos de EventBridgeCore (Authorize, RequestReconciliation) como esqueleto de la misma forma; y, si los necesitas ya, los
     adaptadores del Plugin (WorkspacePanelCommand.cs con [CommandMethod("RACKPANEL")], WorkspaceSelectionReader.cs, el
     controlador y las fachadas de AutoCadEventBridge) como esqueleto que compila.
  2. Pruebas xunit nuevas en tests/RackCad.Tests/Workspace/ (namespace RackCad.Tests) que expresan los criterios y los invariantes
     comprobables de T3A con host y lector simulados, INCLUIDA la cobertura 1-5 de INV-F1-T3A-21 (RequestReconciliation con sesion
     activa sincronizada: encola la pista, deja Idle pendiente y no drena; sin sesion activa: no-op, sin excepcion y sin Idle;
     degradada o retirada: no-op, sin resurreccion y sin nueva suscripcion; Authorize: acepta la sesion correcta, rechaza el
     documento ejecutor equivocado y el origen destruido, igual que WorkspaceSessionRegistry.Authorize; guarda de fuente de las
     fachadas: un solo EventBridgeCore, ningun WorkspaceSessionRegistry, ningun += / -= nuevo, ninguna lectura ni escritura del
     DWG en el puente), las guardas de texto de los adaptadores, el refinamiento de archivo exacto de la guarda de T2 y los dos
     censos (+RACKPANEL, 38). Al menos una prueba de comportamiento falla por asercion contra el esqueleto. Nada de pruebas que
     fallen deliberadamente sin expresar un invariante congelado.
  3. Corre el filtro del contrato: debe compilar y FALLAR POR ASERCION con seleccion >= 160 (no por error de compilacion).
     Commit RED (git add solo de archivos del alcance) y git push. Registra la corrida como Phase RED con TreeSha = el commit RED.
- Desde el commit RED NO modifiques ni anadas archivos bajo tests/ (son RT; 16.8: el GREEN no toca RT). Si una prueba necesitara
  cambio: parada y bloqueo estructurado, no la edites.
- Paso GREEN:
  1. Implementa el comportamiento en los archivos nuevos y en los deltas exactos autorizados hasta que el filtro quede verde.
     Sin Autodesk.AutoCAD.Internal; sin escrituras, persistencia, seleccion, navegacion ni vista; sin PaletteSet ni WPF.
  2. Sobre el commit GREEN (arbol limpio, TreeSha = CurrentSha): el filtro del contrato (todas en verde, seleccion >= 160, Phase
     GREEN); la suite Core completa tests/RackCad.Tests/RackCad.Tests.csproj (0 fallidas; Phase RELEVANT; incluye los censos y
     las guardas de T1/T2); dotnet build -c Release --no-incremental de tests/RackCad.Tests/RackCad.Tests.csproj y de
     tests/RackCad.UI.Tests/RackCad.UI.Tests.csproj con 0 lineas que casen \bwarning\s+(MSB|CS|NU|NETSDK)\d+\b y 0 errores; y la
     build local del Plugin descrita arriba (Phase RELEVANT). Registra comando, seleccion, resultado y conteos. Si algo falla, no
     lo arregles con otro commit: parada y bloqueo estructurado.
  3. Commit GREEN (git add solo de archivos del alcance) con este cuerpo de resumen de estado, y git push (fast-forward):
     "Estado: F1 en curso; entrega del Worker de F1-T3A-HOST-CORE (WorkRunId R20261011T050726Z-0829, intento 3) pendiente de la verificacion
     del Controller y de nc1..nc3. attempts = 3. Owner pendiente: aceptacion del ADR 0047 (propuesto) y Smoke-1 tras el cierre de F1."
- Git: sin --force, sin rebase, sin stash, sin amend, sin checkout de otros SHA, sin tocar otras ramas ni main; exactamente un
  commit RED y un commit GREEN, cada uno con su push.
- No lances subagentes ni procesos en segundo plano que sigan vivos al terminar. Los servidores de compilacion que deje dotnet
  (VBCSCompiler, nodos MSBuild) pueden seguir vivos; no los detengas. No abras ni toques AutoCAD.
- En la entrega y en los mensajes de commit no escribas rutas locales distintas de las del worktree y de artifacts/; nada de
  config.toml.
- Entrega (artifacts/orchestration/I-64/F1-T3A-HOST-CORE/3/R20261011T050726Z-0829/worker-handoff.json, UTF-8, valida contra el esquema; compruebala con pwsh Test-Json -SchemaFile):
  Schema rackcad-worker-handoff/v1; TaskId F1-T3A-HOST-CORE; RunId R20261011T050726Z-0829; DelegationRunId R20261011T045535Z-0140; Initiative I-64; Gate F1;
  Attempt 3; BaseSha = el de arriba; RedSha = el SHA del commit RED; CurrentSha = el SHA del commit GREEN (= HEAD = remoto);
  Branch y Worktree reales; Pushed true; FilesChanged = git diff --name-only BaseSha..CurrentSha; TestsExecuted y TestResults de
  cada corrida (RunRef unico, Command exacto, Phase RED/GREEN/RELEVANT, TreeSha = commit probado, ResultsFile = TRX o log, o null;
  FailedTests con los nombres reales); Worker = { Provider "Anthropic", ModelRequested "claude-sonnet-5-5", EffortRequested
  "high", Trailer "Co-Authored-By: Claude Sonnet 5.5 <noreply@anthropic.com>" }; WorkerStatus IMPLEMENTATION_COMPLETE, PARTIAL o
  BLOCKED; Disposition NONE salvo parada; listas vacias si no hay nada.
- Tope: 75 minutos. Al terminar, tu respuesta final es una linea con WorkerStatus, RedSha y CurrentSha.
