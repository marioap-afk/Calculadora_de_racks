I-64 - DELEGACION F1-T2-BRIDGE - Worker, perfil ROUTINE_IMPLEMENTATION (primera delegacion de la cadena; unica entrega valida autorizada)
Identidad: unidad I-64; gate F1; tarea F1-T2-BRIDGE; intento 3; RunId R20261010T202008Z-3df4;
           DelegationRunId R20261010T082206Z-21c1; AuthorityRevision d5bdcb7aff31ba54901a3a0acdbfd81be92ab516;
           MainSha bb0d5522e8411f66a51fdfb3f1f0d0514b737453; BaseSha d5bdcb7aff31ba54901a3a0acdbfd81be92ab516;
           rama architecture/workspace-persistente-rackcad;
           worktree <worktree-local-redactado>;
           Owner subagent:R20261010T082206Z-21c1
Autoridades (leer desde Git, no copiar; ruta | seccion | clase):
           - docs/initiatives/I-64-proposal-v5.md | documento completo | UNIT_DOC
           - docs/initiatives/I-64-discovery.md | documento completo | UNIT_DOC
           - docs/initiatives/I-64-workspace-persistente-rackcad.md | documento completo | UNIT_DOC
           - docs/initiatives/I-64-A-2.md | documento completo | UNIT_DOC
           - docs/initiatives/I-64-A-4.md | documento completo | UNIT_DOC
           - docs/automation/evidence/I-64-evidence.md | ## 18. Consensus Freeze (I64-CONSENSUS-V5-01) y cierre de F0 | UNIT_DOC
           - docs/automation/evidence/I-64-evidence.md | ## 39. F1 - NIGHT RUN: rebase sobre main, revalidacion del producto de T1, conciliacion con I-52 e I-63 y paquete de F1-T2-BRIDGE | UNIT_DOC
           - docs/automation/evidence/I-64-evidence.md | ## 40. F1-T2-BRIDGE - Decision del Coordinator sobre el contrato de gate | UNIT_DOC
           - docs/automation/state/I-64.yml | documento completo | UNIT_DOC
           - docs/initiatives/I-64-f1-gate-contract-proposal.md | documento completo | UNIT_DOC
           - docs/initiatives/I-64-f1-t2-gate-contract-package.md | documento completo | UNIT_DOC
           - docs/automation/evidence/I-64-pilot/F1-T2-BRIDGE/coordinator-decision-gate-contract.md | documento completo | UNIT_DOC
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
Objetivo (texto literal del contrato; vinetas en linea, sin lineas en blanco):
           Implement the central F1 AutoCAD event bridge with a simulable pure event
           source/policy boundary, without host, panel or product writes.
           Required behavior includes:
           * one central subscriber; * exactly one subscription per governed source; * subscription lifecycle follows live Workspace document sessions; * handlers enqueue hints only; * deferred/coalesced drain; * Idle only while hints are pending; * BeginDocumentClose remains alive for the whole session including degraded
           mode;
           * degradation is session-local; * no event writes selection or view; * no persistence; * no authored mutation; * no existing commands/editors changed.
           T2 is a new chain.
           Therefore:
           ChainBaseSha = first T2 BaseSha
           ChainRedSha = null
           ChainRedFiles = []
           A real RED is mandatory.
           RED must be behavioral and compilable.
           No artificial compile failure.
           The RED must demonstrate at least one frozen T2 behavior not yet
           implemented.
           GREEN must not modify the RED test files after the RED commit contrary to
           16.8.
Alcance: permitido EXACTAMENTE src/RackCad.Application/Workspace/, src/RackCad.Plugin/Workspace/, tests/RackCad.Tests/Workspace/ (archivo exacto o prefijo terminado en /);
           prohibido docs/, .github/, assets/, eng/, deploy/, global.json, Directory.Build.props, Directory.Build.targets, RackCad.sln, src/RackCad.UI/, src/RackCad.Domain/, tests/RackCad.UI.Tests/, src/RackCad.Plugin/Systems/, src/RackCad.Plugin/Views/, src/RackCad.Plugin/Drawing/, src/RackCad.Plugin/PluginInitializer.cs, src/RackCad.Plugin/RackCad.Plugin.csproj, src/RackCad.Application/RackCad.Application.csproj, tests/RackCad.Tests/RackCad.Tests.csproj, tools/RackCad.StructuralSections.Import/RackCad.StructuralSections.Import.csproj, src/RackCad.Plugin/RackAyudaCommands.cs, src/RackCad.Plugin/RackCabeceraCommands.cs, src/RackCad.Plugin/RackCamaCommands.cs, src/RackCad.Plugin/RackCantileverCommands.cs, src/RackCad.Plugin/RackDinamicoCommands.cs, src/RackCad.Plugin/RackDuplicarCommands.cs, src/RackCad.Plugin/RackInventarioCommands.BomTotal.cs, src/RackCad.Plugin/RackInventarioCommands.cs, src/RackCad.Plugin/RackLayoutCommands.Fill.cs, src/RackCad.Plugin/RackLayoutCommands.cs, src/RackCad.Plugin/RackMenuCommands.cs, src/RackCad.Plugin/RackPropiedadesCommands.cs, src/RackCad.Plugin/RackProyectarCommands.cs, src/RackCad.Plugin/RackPushBackCommands.cs, src/RackCad.Plugin/RackSeccionCommands.cs, src/RackCad.Plugin/RackSelectivoCommands.cs, src/RackCad.Plugin/RackVariablesCommands.cs, src/RackCad.Application/Workspace/Hints.cs, src/RackCad.Application/Workspace/SelectionContext.cs, src/RackCad.Application/Workspace/SessionId.cs, src/RackCad.Application/Workspace/WorkspaceSession.cs, src/RackCad.Application/Workspace/WorkspaceSessionRegistry.cs.
           Dentro de src/RackCad.Application/Workspace/ SOLO se anaden archivos NUEVOS: los 5 archivos de T1 (Hints.cs, SelectionContext.cs,
           SessionId.cs, WorkspaceSession.cs, WorkspaceSessionRegistry.cs) no se modifican. Las pruebas de T1 tampoco.
Invariantes del Freeze (vinculantes en su totalidad):
           - INV-F1-T2-01 (INV-02): el numero de suscripciones es funcion exacta de los documentos con sesion y cero para un documento destruido, en los ciclos: inicializacion repetida del controlador (RACKPANEL repetido), activar A->B->A, ocultar y mostrar, alta y baja dinamica de Idle y destruccion; ninguna suscripcion por pestana, editor, view-model ni adaptador.
           - INV-F1-T2-02 (INV-02, guarda): el operador += sobre eventos de AutoCAD solo aparece en src/RackCad.Plugin/Workspace/.
           - INV-F1-T2-03 (INV-03, lado de eventos): ningun evento ni cambio de contexto escribe seleccion o vista; los manejadores ordinarios solo encolan pistas y no leen ni escriben la base de datos, no piden datos, no abren dialogos ni llaman resolvers.
           - INV-F1-T2-04 (D-04): toda excepcion de un manejador se captura y registra y nunca se propaga a AutoCAD; una excepcion del puente pasa la sesion afectada a modo degradado y retira solo sus suscripciones de sincronizacion.
           - INV-F1-T2-05 (D-04, A64-PV3-04): la suscripcion a BeginDocumentClose de una sesion vive toda la sesion, tambien degradada; su manejador solo lee estado en memoria y no lee la base de datos, no resuelve, no escribe ni abre dialogos.
           - INV-F1-T2-06 (D-04, INV-23 y P-04): las pistas pueden llegar en cualquier momento; el drenaje es diferido, coalescido y ocurre solo con el documento activo en reposo y sin modal RackCad activo; Idle solo esta suscrito mientras haya pistas pendientes.
           - INV-F1-T2-07 (INV-14): StartTransaction, StartOpenCloseTransaction y LockDocument solo dentro de using en src/RackCad.Plugin/Workspace/; ningun campo de tipo Transaction, DBObject ni Document; ninguna API publica del puente expone tipos de AutoCAD.
           - INV-F1-T2-08 (INV-21 limitado por el Coordinator para T2, D-22): el puente no tiene autoridad de persistencia ni de escritura: ninguna escritura authored, de NOD ni de XData; ninguna persistencia en perfil ni registro; ningun estado persistente de Workspace; ninguna ruta de escritura expuesta por el puente. No se crea un puerto de host/UI prematuro: la regla de IWorkspaceHostPort se verifica en T3.
           - INV-F1-T2-09 (D-02 y P-01): antes del primer RACKPANEL hay cero suscripciones; nada se suscribe al cargar el plugin.
           - INV-F1-T2-10 (D-03 e INV-22): toda pista lleva la identidad de su sesion; la sesion se retira en DocumentToBeDestroyed y el puente quita sus suscripciones; un identificador nativo reciclado no se empareja con la sesion retirada.
           - INV-F1-T2-11 (ADR-0006 y decision del Coordinator): AutoCAD solo en el Plugin; los archivos de producto de T1 en src/RackCad.Application/Workspace/ no cambian (solo archivos nuevos en Application); el modelo de Application sigue sin referencias a AutoCAD y las guardas de T1 siguen en verde.
           - INV-F1-T2-12 (MASTER-I63-I64-02): ninguna referencia a ComputedParameters, metricas, providers, ProjectSummary ni agregacion de I-63.
           - INV-F1-T2-13 (D-16 y M-03): RACKEDITAR, los comandos clasicos y los editores existentes no cambian.
           - INV-F1-T2-14 (D-01 y O-10): ningun uso de Autodesk.AutoCAD.Internal.
Celda: claude-sonnet-5-5|subagent; modelo claude-sonnet-5-5; effort high (Deep); clase "Implementacion transversal a capas (corta)"; RoutingEnforcement required
Criterios de aceptacion (del paquete aceptado):
           - El diff git diff --name-only BaseSha..CurrentSha contiene solo archivos nuevos bajo src/RackCad.Application/Workspace/, src/RackCad.Plugin/Workspace/ y tests/RackCad.Tests/Workspace/; no cambia ninguno de los cinco archivos de producto T1 ni las pruebas de T1.
           - Primero se agrega una prueba RED compilable, seleccionada por el filtro del contrato con al menos una prueba seleccionada, que falla por asercion ante un comportamiento congelado T2 aun no implementado contra un esqueleto; no falla al compilar ni usa una prueba artificial. Luego GREEN pasa sin modificar los archivos de prueba RED, conforme a 16.8.
           - El nucleo del puente en archivos nuevos de Application es puro y sin AutoCAD, y se prueba con fuente de eventos simulada: las suscripciones siguen exactamente los documentos con sesion en los ciclos de INV-F1-T2-01 y son cero tras destruccion; los manejadores solo encolan pistas con identidad de sesion; el drenaje es diferido y coalescido solo en reposo y sin modal RackCad activo; Idle existe solo con pistas pendientes; BeginDocumentClose vive toda la sesion incluso degradada; la degradacion es local a la sesion; ningun evento escribe seleccion o vista.
           - El unico suscriptor real, con += sobre eventos de AutoCAD, esta en src/RackCad.Plugin/Workspace/. Nada se suscribe al cargar el plugin. No se modifican PluginInitializer, comandos, csproj ni sln, y no se crea puerto de host/UI.
           - Hay guardas de texto en tests/RackCad.Tests/Workspace/ para INV-F1-T2-02, 07, 08, 12 y 14 sobre src/RackCad.Plugin/Workspace/.
           - Se cumplen las convenciones medidas: namespaces derivados de carpeta y un namespace de bloque; el namespace raiz de pruebas es RackCad.Tests, las nuevas clases de prueba empiezan por Workspace y el filtro del contrato las selecciona; los archivos de prueba con anotaciones nullable habilitan #nullable; las guardas existentes de T1 permanecen sin cambios y pasan sobre los nuevos archivos de Application.
           - En el commit GREEN, el filtro del contrato y la suite Core completa pasan; las builds Release no emiten avisos nuevos MSB/CS/NU/NETSDK y el Plugin compila sin avisos nuevos.
           - La CI del CurrentSha exacto, de push en la rama esperada, tiene en success los cuatro jobs requeridos por el contrato.
           - No se usa AutoCAD ni se carga el DLL; no se modifica perfil, registro, TRUSTEDPATHS, SECURELOAD ni ACL; no se ejecuta Smoke-1.
Evidencia requerida: pruebas (cada corrida filtrada demuestra seleccion > 0):
           - tests/RackCad.Tests/RackCad.Tests.csproj, filtro FullyQualifiedName~RackCad.Tests.Workspace, minimo 1, RED esperado: si
           evidencia esperada del contrato: gate/delegation package valido; nc4 segun I-61 en la primera delegacion aplicable; aceptacion A1-A8 antes de Worker; commit RED real; ChainRedSha / ChainRedFiles correctamente acreditados; commit GREEN; pruebas RED y GREEN con seleccion; worker-handoff valido; push fast-forward; CI exacta del CurrentSha verde (incluye Build Plugin without AutoCAD); Controller verification valida; diff dentro de AllowedWriteScope; guardas de fuente de INV-02, INV-14 e INV-21 en verde y seleccionadas por el filtro; nc1..nc4 segun README Sec.10; nc2 ejecutado con el Controller y conservado; si no discrimina Scope, sustituto I64-SCOPE-BRIDGE-01 bajo A-4 exactamente como esta autorizado (verificador sin cambios), sin reescribir el nc2 historico; ninguna correccion: un resultado valido que la exija es STOP S-11.
           La sesion, no tu, mantiene evidencia y estado (docs/ esta prohibido).
No-touch: todo lo prohibido arriba; el checkout principal <checkout-principal-redactado> (nunca escribas alli);
           las ramas de I-52 y de I-62 y main; AutoCAD: el acad.exe que pueda estar abierto pertenece a otra iniciativa:
           no lo cierres, no lo uses, no cargues DLL, no toques perfil, TRUSTEDPATHS, SECURELOAD, ACL ni registro; sin Smoke-1
Condiciones de parada: S-01..C-17 del contrato:
           C-01: origin/main cambia antes de escribir
           C-02: se necesita tocar src/RackCad.UI, src/RackCad.Domain, comandos existentes, PluginInitializer.cs, csproj o sln
           C-03: se necesita AutoCAD, cargar el DLL, perfil, TRUSTEDPATHS, SECURELOAD, ACL o registro
           C-04: se necesita ampliar scope
           C-05: aparece ambiguedad material del Freeze
           C-06: cambia config.toml
           C-07: hay conflicto de ownership o se necesita un archivo caliente (docs/HANDOFF.md, docs/ROADMAP.md, docs/adr/README.md)
           C-08: la tarea deriva hacia Host/PaletteSet/RACKPANEL/WPF (F1-T3-HOST)
           C-09: se intenta introducir tipos o metricas de I-63
           C-10: se intenta modificar comandos existentes o RACKEDITAR
           C-11: escritura authored, NOD o XData desde el puente
           C-12: uso de Autodesk.AutoCAD.Internal
           C-13: presupuesto agotado
           C-14: cualquier archivo fuente existente de T1 en Workspace debe cambiar (STOP C-04 por ampliacion de alcance)
           C-15: cualquier resultado valido del Worker o del verificador exige correccion, o una verificacion valida devuelve REWORK (CorrectionsAuthorized false; AttemptsRemaining 0; S-11)
           C-16: se necesita trabajo de UI o de host, o un puerto de host/UI (alcance de F1-T3-HOST)
           C-17: main se mueve de forma que exige una reconciliacion semantica no resuelta
           S-01 y S-10 son frontera (Owner Validation, AutoCAD): no se cruzan y se anotan en KnownLimitations; ante cualquier
           otra, parar sin continuar y devolver el bloqueo estructurado en la entrega (WorkerStatus BLOCKED, Disposition y
           TriggeredStopConditions). No hay presupuesto de correccion: no se repara nada fuera de este paso.
Prohibido declarar: GATE PASS, Candidato, cierre, integracion o aprobacion del Owner;
           ningun termino de AUTOMATION_PLAN Sec.16 "Terminos de gate" (16.10) en el texto libre
Trailer: Co-Authored-By: Claude Sonnet 5.5 <noreply@anthropic.com> (quien ejecuta), nunca de otro participante
Informe esperado: la salida exacta del esquema docs/automation/agent-execution/schemas/worker-handoff.schema.json en
           artifacts/orchestration/I-64/F1-T2-BRIDGE/3/R20261010T202008Z-3df4/worker-handoff.json;
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
- ChainBaseSha = BaseSha = d5bdcb7aff31ba54901a3a0acdbfd81be92ab516; ChainRedSha = null y ChainRedFiles = []: esta entrega EXIGE RED.
  Attempt 3; AttemptsRemaining 0; CorrectionsAuthorized false: esta es la UNICA entrega valida; si algo sale mal, se para.
- Entorno: Windows. Empieza cada orden de shell con cd al worktree por ruta absoluta (el directorio actual del shell
  puede volver al checkout principal). dotnet: usa SIEMPRE <dotnet-de-usuario-redactado> (Git Bash) o
  <dotnet-de-usuario-redactado> (PowerShell); el dotnet del PATH no sirve.
  Resultados de pruebas (--logger trx y --results-directory) y logs de build solo bajo artifacts/orchestration/I-64/F1-T2-BRIDGE/3/R20261010T202008Z-3df4/tests/<RunRef>/.
  Edita con la herramienta de edicion o de escritura, no con heredocs de shell.
- Antes de editar: git status limpio, git rev-parse HEAD = BaseSha, git ls-remote origin
  refs/heads/architecture/workspace-persistente-rackcad = BaseSha y git rev-parse origin/main (tras git fetch) = MainSha;
  src/RackCad.Plugin/Workspace/ no existe. Si no, parada (S-12/S-13/C-01).
- Hechos medidos del repositorio que debes respetar (evidencia Sec.40):
  * tests/RackCad.Tests solo referencia Domain y Application: el comportamiento con fuente de eventos simulada se prueba sobre
    tipos NUEVOS en src/RackCad.Application/Workspace/; el adaptador real de AutoCAD (Plugin) se comprueba con guardas de texto que
    leen las fuentes desde la raiz del repositorio (la carpeta que contiene RackCad.sln, como hace WorkspaceModelBoundaryTests).
  * Las guardas de T1 (tests/RackCad.Tests/Workspace/WorkspaceModelBoundaryTests.cs) se aplican tambien a tus archivos nuevos de
    Application: el texto de codigo (sin comentarios) de toda la carpeta no puede contener System.IO, File., Directory.,
    StreamWriter, Microsoft.Win32, Registry., UserSettings, AtomicFile, Transaction, LockDocument, Database como palabra, Commit(,
    Autodesk., System.Windows, RackCad.Plugin ni RackCad.UI; ningun tipo del namespace exacto RackCad.Application.Workspace puede
    tener en el nombre Tab, Subscription, Editor o Draft, ni nombre o miembro publico con ProjectSummary, Metric, Provider,
    TotalRacks, Aggregat o Bom (sin distinguir mayusculas). No evadas las guardas semanticamente (por ejemplo, con un subespacio
    de nombres para esconder un concepto prohibido): elige una terminologia que exprese fielmente el modelo congelado sin crear
    propiedad ni persistencia falsas.
  * NamespaceFolderGuardTests: en src/ el namespace de cada archivo es la ruta de su carpeta (src/RackCad.Plugin/Workspace/X.cs ->
    RackCad.Plugin.Workspace; src/RackCad.Application/Workspace/X.cs -> RackCad.Application.Workspace) con exactamente un
    namespace de bloque (namespace X { ... }); en tests/RackCad.Tests el unico namespace es RackCad.Tests (como en las pruebas de
    T1; NO RackCad.Tests.Workspace). Las clases de prueba nuevas se llaman Workspace... para que las seleccione el filtro.
  * Nullable: cada archivo de prueba nuevo que use anotaciones ? lleva #nullable enable (como las pruebas de T1). En el Plugin
    sigue la politica de los archivos existentes. Usa catch (System.Exception ex) en archivos que importen Autodesk.AutoCAD.Runtime.
  * El job de CI "Build Plugin without AutoCAD" publica el Plugin en Release con referencias NuGet y falla ante cualquier aviso
    MSB/CS/NU/NETSDK. Linea base local medida por la sesion en BaseSha con las DLL de AutoCAD instaladas:
    <dotnet-de-usuario-redactado> build src/RackCad.Plugin/RackCad.Plugin.csproj -c Release --no-incremental -v:minimal -o <dir temporal bajo artifacts/orchestration/I-64/F1-T2-BRIDGE/3/R20261010T202008Z-3df4/tests/>
    -> codigo 0, 0 errores, "2 Advertencia(s)", ambas MSB3277 (conflicto de versiones de las DLL de AutoCAD de esta estacion; no
    existen en la CI). Tu build del Plugin debe dar 0 errores, 0 avisos CS/NU/NETSDK y ningun aviso MSB distinto de esos MSB3277,
    con el mismo resumen de 2. Compila SIEMPRE el Plugin con -o hacia un directorio temporal bajo artifacts/orchestration/I-64/F1-T2-BRIDGE/3/R20261010T202008Z-3df4/tests/ (nunca su bin/), no lo
    publiques y no cargues nada en AutoCAD.
  * El Plugin actual no tiene ninguna suscripcion a eventos de AutoCAD; PluginInitializer.cs y los comandos no se tocan: nada se
    suscribe al cargar el plugin; el puente se crea por quien lo use (F1-T3-HOST), no aqui.
  * API publica del modelo de T1 (solo lectura): WorkspaceSessionRegistry (GetOrCreate(long), TryGet, Destroy, Authorize),
    WorkspaceSession (EnqueueHint(HintKind), TryDrain(DrainConditions), ApplyContext), HintKind, DrainConditions (IsQuiescent,
    ModalRackCadActive, PanelVisible), DrainResult, DrainOperation, SessionId, RackKey, WorkspaceRequest, RequestDecision.
    Leelos antes de disenar; si el puente no puede construirse sin cambiar uno de esos archivos: parada C-14 (no lo cambies).
- Paso RED (un solo commit, con TODAS las pruebas de la cadena):
  1. Tipos nuevos del nucleo puro del puente en src/RackCad.Application/Workspace/ como ESQUELETO COMPILABLE (firmas completas y
     cuerpos que aun no cumplen el comportamiento congelado, p. ej. valores por defecto; nunca NotImplementedException que haga
     fallar todo por excepcion), con una fuente de eventos abstracta simulable; y, si lo necesitas ya, el adaptador del Plugin en
     src/RackCad.Plugin/Workspace/ como esqueleto que compila.
  2. Pruebas xunit nuevas en tests/RackCad.Tests/Workspace/ (namespace RackCad.Tests) que expresan los criterios y los
     invariantes comprobables de T2 con fuente simulada (recuento exacto de suscripciones por fuente gobernada en los ciclos de
     INV-F1-T2-01 y cero tras la destruccion; manejadores que solo encolan con la identidad de su sesion; drenaje solo en reposo y
     sin modal RackCad activo, coalescido; Idle solo con pistas pendientes; BeginDocumentClose vivo toda la sesion tambien en modo
     degradado; degradacion local a la sesion; ningun evento escribe seleccion o vista) y las guardas de texto de INV-F1-T2-02,
     07, 08, 12 y 14 sobre src/RackCad.Plugin/Workspace/ (las de guarda pueden pasar ya en RED).
     Al menos una prueba de comportamiento falla por asercion contra el esqueleto. Nada de pruebas que fallen deliberadamente sin
     expresar un invariante congelado.
  3. Corre el filtro del contrato: debe compilar y FALLAR POR ASERCION con seleccion > 0 (no por error de compilacion).
     Commit RED (git add solo de archivos del alcance) y git push. Registra la corrida como Phase RED con TreeSha = el commit RED.
- Desde el commit RED NO modifiques ni anadas archivos bajo tests/ (son RT; 16.8: el GREEN no toca RT). Si una prueba necesitara
  cambio: parada y bloqueo estructurado, no la edites.
- Paso GREEN:
  1. Implementa el comportamiento en archivos nuevos de src/RackCad.Application/Workspace/ y en src/RackCad.Plugin/Workspace/ (el
     unico suscriptor real, += sobre los eventos de AutoCAD de D-04: coleccion de documentos; por documento con sesion
     ImpliedSelectionChanged, CommandWillStart, CommandEnded, CommandCancelled, CommandFailed, BeginDocumentClose y
     EnteringQuiescentState de su Editor; por base de datos con sesion ObjectAppended, ObjectErased, ObjectModified,
     ObjectUnappended y ObjectReappended; Application.Idle solo con pistas pendientes) hasta que el filtro quede verde.
     Sin Autodesk.AutoCAD.Internal; sin escrituras, persistencia, seleccion, navegacion ni vista; sin puerto de host/UI.
  2. Sobre el commit GREEN (arbol limpio, TreeSha = CurrentSha): el filtro del contrato (todas en verde, seleccion > 0, Phase
     GREEN); la suite Core completa tests/RackCad.Tests/RackCad.Tests.csproj (0 fallidas; Phase RELEVANT); dotnet build -c Release
     --no-incremental de tests/RackCad.Tests/RackCad.Tests.csproj y de tests/RackCad.UI.Tests/RackCad.UI.Tests.csproj con 0 lineas
     que casen \bwarning\s+(MSB|CS|NU|NETSDK)\d+\b y 0 errores; y la build local del Plugin descrita arriba (Phase RELEVANT).
     Registra comando, seleccion, resultado y conteos. Si algo falla, no lo arregles con otro commit: parada y bloqueo estructurado.
  3. Commit GREEN (git add solo de archivos del alcance) con este cuerpo de resumen de estado, y git push (fast-forward):
     "Estado: F1 en curso; entrega del Worker de F1-T2-BRIDGE (WorkRunId R20261010T202008Z-3df4, intento 3) pendiente de la verificacion
     del Controller y de nc1..nc3. attempts = 3. Owner pendiente: aceptacion del ADR 0047 (propuesto) y Smoke-1 tras el cierre de F1."
- Git: sin --force, sin rebase, sin stash, sin amend, sin checkout de otros SHA, sin tocar otras ramas ni main; exactamente un
  commit RED y un commit GREEN, cada uno con su push.
- No lances subagentes ni procesos en segundo plano que sigan vivos al terminar. Los servidores de compilacion que deje dotnet
  (VBCSCompiler, nodos MSBuild) pueden seguir vivos; no los detengas. No abras ni toques AutoCAD.
- En la entrega y en los mensajes de commit no escribas rutas locales distintas de las del worktree y de artifacts/; nada de
  config.toml.
- Entrega (artifacts/orchestration/I-64/F1-T2-BRIDGE/3/R20261010T202008Z-3df4/worker-handoff.json, UTF-8, valida contra el esquema; compruebala con pwsh Test-Json -SchemaFile):
  Schema rackcad-worker-handoff/v1; TaskId F1-T2-BRIDGE; RunId R20261010T202008Z-3df4; DelegationRunId R20261010T082206Z-21c1; Initiative I-64; Gate F1;
  Attempt 3; BaseSha = el de arriba; RedSha = el SHA del commit RED; CurrentSha = el SHA del commit GREEN (= HEAD = remoto);
  Branch y Worktree reales; Pushed true; FilesChanged = git diff --name-only BaseSha..CurrentSha; TestsExecuted y TestResults de
  cada corrida (RunRef unico, Command exacto, Phase RED/GREEN/RELEVANT, TreeSha = commit probado, ResultsFile = TRX o log, o null;
  FailedTests con los nombres reales); Worker = { Provider "Anthropic", ModelRequested "claude-sonnet-5-5", EffortRequested
  "high", Trailer "Co-Authored-By: Claude Sonnet 5.5 <noreply@anthropic.com>" }; WorkerStatus IMPLEMENTATION_COMPLETE, PARTIAL o
  BLOCKED; Disposition NONE salvo parada; listas vacias si no hay nada.
- Tope: 60 minutos. Al terminar, tu respuesta final es una linea con WorkerStatus, RedSha y CurrentSha.
