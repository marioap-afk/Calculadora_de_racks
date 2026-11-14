I-64 - DELEGACION F4-T4A1-PREPARE-CORE - Controller, perfil CONTROLLER_PLANNING (primera delegacion de la cadena F4-T4A1-PREPARE-CORE)
Identidad: unidad I-64; gate F4; tarea F4-T4A1-PREPARE-CORE; intento 1; RunId R20261114T023014Z-4765;
           DelegationRunId R20261114T023014Z-4765; AuthorityRevision c76f01dadf1679db3fdc827ce63a9cf758d664e8 (la del contrato);
           MainSha 812baf879612320653094a8aaf46aca0bac81383; BaseSha = el HEAD actual del worktree (se espera 7f8f9aa7d460ec059da24f8680d08d5a704ddf06: commit de emision del contrato, hijo directo de la AuthorityRevision c76f01dadf1679db3fdc827ce63a9cf758d664e8:
           solo anade el contrato, el estado y la evidencia);
           rama architecture/workspace-persistente-rackcad; worktree = el directorio de trabajo actual; Owner subagent:R20261114T023014Z-4765
Contrato de gate: artifacts/orchestration/I-64/F4-T4A1-PREPARE-CORE/1/R20261114T023014Z-4765/gate-contract.json (ASCII puro; custodiado en docs/automation/evidence/I-64-pilot/F4-T4A1-PREPARE-CORE/coordinator/gate-contract.json)
Autoridades (leer desde Git, no copiar su contenido): las del campo Authorities del contrato;
           vigencia: AuthorityRevision para la unidad (UNIT_DOC y UNIT_CHANGE), MainSha para lo demas (EXTERNAL)
Objetivo: emitir el paquete de delegacion (esquema rackcad-delegation/v1) del Worker para la tarea del contrato. Debe ser ejecutable tal cual.
Alcance: solo lectura. No escribas ningun archivo, no hagas commit ni push. No ejecutes dotnet, ni AutoCAD, ni builds.
Invariantes del Freeze: los 14 del contrato; una delegacion nunca amplia el contrato
Rutas: ver ALCANCE DE ESCRITURA abajo. Todo lo demas es de solo lectura.
           Fuente de diseno: docs/initiatives/I-64-f4-t4-gate-contract-package.md (v3: section 2, hechos H-01 a H-19; section 3, la descomposicion de F4-T4, la reubicacion de T1B y las reasignaciones; section 4, decisiones D-T4A1-01 a D-T4A1-08 con la tabla cerrada T-A1 (A1-01 y A1-02); section 5, contrato propuesto; section 10, disposiciones de las dos revisiones; sections 6 y 7 son reglas de las tareas siguientes, NO de esta); docs/initiatives/I-64-f4-gate-contract-package.md; docs/initiatives/I-64-f4-t2-gate-contract-package.md (section 3 y 152-153); docs/initiatives/I-64-proposal-v5.md (D-10 a D-14); IC-07; el contrato de F4-T1 (INV-F4T1-04, INV-F4T1-11, INV-F4T1-20); las ordenes del Owner owner-f4t4-preparation-authorization.md y owner-f4t4-decomposition-decision.md; el contrato.
Criterios de aceptacion: la salida valida contra el esquema; colecciones opacas del contrato copiadas byte a byte (ver COPIA EXACTA);
           AllowedWriteScope segun ALCANCE DE ESCRITURA; celda y effort entre las EligibleCells del contrato; TaskClass y campos de la DELTA exactamente como se indican
Evidencia requerida: ejecuta de verdad estas ordenes en el worktree y usa sus resultados:
           git branch --show-current ; git rev-parse HEAD ; git rev-parse origin/main ;
           git ls-files src/RackCad.Plugin/Workspace (9 archivos) ; git ls-files src/RackCad.Application/Systems/PushBack/Pilot (7 archivos) ; git ls-files src/RackCad.Application/Workspace/Drafts (5 archivos) ; git ls-files tests/RackCad.UI.Tests/Workspace (21 archivos) ;
           y lee el codigo, las pruebas y las guardas que cites
No-touch: todo el repositorio
Condiciones de parada: S-03, P-03 y las F4T4A1-C* del contrato; ante cualquiera, para y explica el bloqueo en RoutingReason.
Antecedentes: en esta unidad, cinco planificaciones se RECHAZARON por la copia: "lo leen" cambiado por "los leen" en un invariante (A5 FAIL); UNA entrada de ForbiddenWriteScope omitida (A4 FAIL); un espacio anadido delante de una entrada (" .github/", A4 FAIL); "WorkspacePaletteHostGuardTests.cs" en vez de "WorkspacePaletteHostSourceGuardTests.cs" (le falto la palabra Source; A4 FAIL); y, en la tarea anterior, "mientras la profundidad es mayor que 0" copiado como "sea" (modo verbal cambiado de indicativo a subjuntivo; A5 FAIL). Una sola diferencia de un caracter o un solo elemento de mas o de menos basta para rechazar. Por la parada F4T4A1-C08 del contrato, un rechazo admite una sola replanificacion y un segundo vuelve al Coordinator.
Prohibido declarar: GATE PASS, Candidato, cierre, integracion o aprobacion del Owner;
           ningun termino de AUTOMATION_PLAN section 16 "Terminos de gate" en el texto libre, ni en nombres de archivo, rutas o artefactos
Trailer: no aplica (no hay commits)
Informe esperado: la salida exacta del esquema rackcad-delegation/v1 (la escribe el CLI por -o)

PERFIL CONTROLLER_PLANNING
Metodo: clasificar y enrutar dentro del contrato de gate, sin escribir en Git.
1. Lee el contrato, docs/automation/agent-execution/routing.md y el catalogo.
2. Clasifica la tarea, puntua las siete dimensiones y elige entre las celdas del
   contrato el nivel mas bajo adecuado; registra RoutingReason con fechas.
3. Copia del contrato el alcance, los invariantes, las pruebas y las paradas; nunca
   los amplies.
4. Rellena el esquema de delegacion completo; null solo donde el esquema lo admite.

CONTEXTO (hechos versionados en el HEAD; evidencia section 187 y siguientes de docs/automation/evidence/I-64-evidence.md; F4-E51 en docs/automation/evidence/I-64-pilot/F4-DRAFTS/f4-dispositions.md)
- F4-T1, F4-T2 y F4-T3 estan cerradas. El Owner y el Coordinator autorizaron preparar F4-T4 (Actualizar), y el Owner la dividio en F4-T4A1-PREPARE-CORE, F4-T4A2-PREPARE-UI, F4-T4B-MUTATE-POST y F4-T4C-HOST-UNDO. Esta es F4-T4A1, la primera: el PREPARE de SOLO LECTURA en el nucleo de borradores, el host y el piloto: ConfirmBase ("Comprobar ahora" general) en DraftSession; la sobrecarga atomica de KeepMineOnCurrentVersion con keepOn; las operaciones del puerto TryConfirmBase, TryKeepMine y TryDiscardAndReload con sus cuerpos del host (TryConfirmPilotBase, TryKeepPilotEdit, TryDiscardAndReloadPilotEdit), la compuerta y los reenvios del controlador; los tipos nuevos del puerto (EditConfirmStatus, EditConfirmOutcome, PilotUnknownOrigin, EditIntentStatus.Kept y tres parametros opcionales de PilotFieldView); el archivo nuevo src/RackCad.Application/Systems/PushBack/Pilot/PushBackPilotUpdatePlan.cs con PushBackPilotPrepareInput.From, PushBackPilotUpdatePlanner.Prepare y el plan inmutable; los arrastres (1), (2), (3) y (6) de F4-E25 en el materializador; R-3 por guarda; C2-M-8. Archivos nuevos: ese plan y tests/RackCad.Tests/Workspace/WorkspacePilotPrepareTests.cs y WorkspacePilotPrepareGuardTests.cs. Oraculo critico del Owner: PREPARE de solo lectura, cero escrituras authored, ninguna definicion de bloque importada, el borrador no cambia ni se limpia Dirty por pasar PREPARE; nada de MUTATE, POST, Undo ni de la UI de Actualizar.
  Presupuesto: Attempt 1, AttemptsRemaining 2, MaxReworkLoops 3, CorrectionsAuthorized true; 1 invocacion de Worker por delegacion aceptada.
- HECHOS DEL REPOSITORIO medidos por la sesion en el HEAD (comprueba los que uses):
  * Lineas base medidas en los TRX de la CI del GREEN de F4-T3C2 (4e34bb56, CI 43510803096; desde entonces solo documentacion): FullyQualifiedName~RackCad.Tests.Workspace = 2668 y FullyQualifiedName~RackCad.UI.Tests.Workspace = 458, todas en verde. Core tiene ExpectRed true: en el RED fallan solo las pruebas nuevas o cambiadas, por asercion, y todas las demas pasan (INV-F4T4A1-13). UI tiene ExpectRed false: en esta tarea solo cambia el doble WorkspacePilotEditPortDouble.cs, ninguna prueba de UI cambia y el filtro de UI pasa entero tambien en el RED. El censo CustomPropertiesEnvelopeGuardTests (fuera del filtro de Core, en tests/RackCad.Tests) pasa ya en el RED porque el esqueleto lleva el miembro que compone con su llamada a Compose y la entrada declarada del censo.
  * El filtro de UI selecciona las clases cuyo nombre completo contiene RackCad.UI.Tests.Workspace: las clases de prueba estan en el namespace RackCad.UI.Tests y su nombre empieza por Workspace, asi que las clases nuevas tambien empiezan por Workspace.
  * La CI corre las pruebas en Debug y compila el Plugin en Release (job build-plugin); el build local del Plugin en Release produce 2 avisos MSB3277 como linea base. La suite completa de UI en Release da 2 fallos de PendingTextFieldTests.ShowOnADirtyField_* que dependen de #if DEBUG y pasan en Debug: condicion previa conocida.
  * Lo que esta FIJADO en el paquete y se traslada a los AcceptanceCriteria como receta, sin reconstruirlo: la tabla de ConfirmBase de D-T4A1-01 (cinco filas; en Conflict con MATCH responde Confirmed y SIGUE en Conflict); el orden de ejecucion de la sobrecarga de D-T4A1-02 (keepOn nulo, NotAllowed, NotConfirmed, NotAdmitted, copias antes de asignar, Rebase); los pasos 1 a 8 de los cuerpos del host y su aplicabilidad de D-T4A1-03; los enumerados y los parametros exactos de D-T4A1-03; los textos A1-01 y A1-02 de la tabla T-A1 y el paso de 28 a 30 filas de la tabla de literales; el orden de los siete resultados de Prepare y los casos (a) a (e) de Invalid de D-T4A1-04; el contenido del plan; la entrada nueva del censo de Compose; los literales de los arrastres de D-T4A1-05; R-3 de D-T4A1-06; la tabla de guardas de D-T4A1-07.
  * Trampas medidas por la sesion (section 2 del paquete): Drafts tiene exactamente cinco archivos (sin archivo nuevo: los tipos nuevos van en DraftSession.cs), sus nombres de tipo sin Subscription, Editor, Tab ni Handler, sus miembros sin Handler, Metric, Provider ni TotalRacks, y los tipos de sus firmas solo de System, System.Collections.Generic, System.Collections.ObjectModel, System.Text, de Drafts o SessionId (Func es de System); el puerto no nombra Draft* ni PushBack*, y los nombres de tipo del puerto y del soporte no contienen Draft, Editor, Subscription ni Tab ni son una palabra de los archivos de Drafts; AllowedConsumers de la guarda del uso de la base solo admite hoy .Begin(, .DiscardAndReload( y .CheckNow( (se extiende a .ConfirmBase( y .KeepMineOnCurrentVersion(); RackEmbedComposer esta prohibido en Application/Workspace; el censo de Compose de I-54 es exacto y ordenado por ruta, y el primer argumento debe ser un parametro RackEmbedDocument (sin ?) del miembro que llama; EditIntentStatus es un prefijo ordenado en tres guardas y termina hoy en Reloaded; PilotFieldView tiene un solo constructor y la guarda fija hoy que su ultimo parametro es string? errorReason = null; la tabla de literales fija 28 filas con sus cuentas exactas; la guarda de la marca conocido limpio recorre una lista fija de cinco operaciones. Cambios declarados de pruebas existentes: solo los de la tabla de D-T4A1-07 y de D-T4A1-08.
  * Dobles del puerto: tres, que siguen cada uno su patron: CommandAdvisorPortDouble (WorkspaceCommandAdvisorTests.cs:17) registra y lanza con Refuse<T>(nombre, request.Session); ForwardingRiskPort (:1104) reenvia y registra en _calls; WorkspacePilotEditPortDouble (UI) gana un respondedor opcional por operacion, inerte por defecto. Ninguna asercion existente cambia.

ALCANCE DE ESCRITURA
- AllowedWriteScope de la delegacion = EXACTAMENTE las 19 entradas del contrato, en su orden (19 archivos exactos, sin prefijos):
    src/RackCad.Application/Workspace/Drafts/DraftSession.cs
    src/RackCad.Application/Systems/PushBack/Pilot/PushBackPilotMaterializer.cs
    src/RackCad.Application/Systems/PushBack/Pilot/PushBackPilotUpdatePlan.cs
    src/RackCad.Application/Workspace/WorkspaceHostCore.cs
    src/RackCad.Application/Workspace/IWorkspacePilotEditPort.cs
    src/RackCad.Application/Workspace/WorkspacePilotSupport.cs
    src/RackCad.Application/Workspace/WorkspaceEditIntent.cs
    src/RackCad.Plugin/Workspace/WorkspaceController.cs
    tests/RackCad.UI.Tests/Workspace/WorkspacePilotEditPortDouble.cs
    tests/RackCad.Tests/CustomPropertiesEnvelopeGuardTests.cs
    tests/RackCad.Tests/Workspace/WorkspaceCommandAdvisorTests.cs
    tests/RackCad.Tests/Workspace/WorkspacePilotPrepareTests.cs
    tests/RackCad.Tests/Workspace/WorkspacePilotPrepareGuardTests.cs
    tests/RackCad.Tests/Workspace/WorkspaceDraftPilotMaterializationTests.cs
    tests/RackCad.Tests/Workspace/WorkspacePilotEditGuardTests.cs
    tests/RackCad.Tests/Workspace/WorkspaceLifecycleGuardTests.cs
    tests/RackCad.Tests/Workspace/WorkspaceEditIntentGuardTests.cs
    tests/RackCad.Tests/Workspace/WorkspaceEditIntentHostTests.cs
    tests/RackCad.Tests/Workspace/WorkspaceHostHardeningGuardTests.cs
  Nunca anadas una ruta fuera del contrato ni un prefijo de carpeta.

COPIA EXACTA DEL CONTRATO ASCII (A5 se evalua contra este contrato ASCII)
COPY THE CANONICAL ASCII GATE CONTRACT VALUES EXACTLY.
Do not translate.
Do not add accents.
Do not normalize spelling.
Do not improve wording.
Do not reconstruct Unicode.
Do not paraphrase.
Do not add or remove spaces.
- Todas las cadenas del contrato canonico artifacts/orchestration/I-64/F4-T4A1-PREPARE-CORE/1/R20261114T023014Z-4765/gate-contract.json son ASCII puro. Se copian byte a byte.
- Authorities, ForbiddenWriteScope, Invariants, RequiredTests, ExpectedEvidence, StopConditions y Objective son DATOS OPACOS de ese contrato. CARGALOS desde el archivo con una orden y reproducelos en tu salida a partir de esa carga, por ejemplo:
  python -c "import json; c=json.load(open(r'artifacts/orchestration/I-64/F4-T4A1-PREPARE-CORE/1/R20261114T023014Z-4765/gate-contract.json', encoding='ascii')); print(json.dumps({k: c[k] for k in ['Authorities','ForbiddenWriteScope','Invariants','RequiredTests','ExpectedEvidence','StopConditions','Objective','AllowedWriteScope']}, ensure_ascii=True))"
  Mismo orden, mismos elementos, mismos textos byte a byte.
- MODO VERBAL Y GRAMATICA: el contrato usa a proposito construcciones que pueden parecer mejorables (indicativo tras si, con o comprueba; nombres en vez de verbos); se copian IGUAL. Antes de terminar, imprime cada invariante del contrato NUMERADO (por ejemplo: python -c "import json; c=json.load(open(r'artifacts/orchestration/I-64/F4-T4A1-PREPARE-CORE/1/R20261114T023014Z-4765/gate-contract.json', encoding='ascii')); [print(i, x) for i, x in enumerate(c['Invariants'])]") y compara tu salida con esa impresion, invariante por invariante y verbo por verbo.
- AUTHORITIES: tres autoridades citan paquetes con nombres casi iguales; se copian cada uno en su indice y con su nombre exacto: Authorities[0].Path = "docs/initiatives/I-64-f4-t4-gate-contract-package.md" (f4-t4: el paquete de esta tarea); Authorities[1].Path = "docs/initiatives/I-64-f4-gate-contract-package.md" (f4, sin t4); Authorities[2].Path = "docs/initiatives/I-64-f4-t2-gate-contract-package.md" (f4-t2). Una planificacion anterior de esta unidad copio un nombre de paquete por otro y se RECHAZO. Antes de terminar imprime cada autoridad NUMERADA (por ejemplo: python -c "import json; c=json.load(open(r'artifacts/orchestration/I-64/F4-T4A1-PREPARE-CORE/1/R20261114T023014Z-4765/gate-contract.json', encoding='ascii')); [print(i, repr(x)) for i, x in enumerate(c['Authorities'])]") y comparala indice por indice y letra por letra.
- SIN CONFLICTOS INEXISTENTES: el contrato canonico de esta corrida es ASCII PURO (0 bytes no ASCII; compruebalo con python -c "b=open(r'artifacts/orchestration/I-64/F4-T4A1-PREPARE-CORE/1/R20261114T023014Z-4765/gate-contract.json', 'rb').read(); print(sum(1 for x in b if x >= 0x80))"); TaskClass se escribe exactamente como lo fija este prompt, sin tilde (la tabla de routing.md lleva tilde y NO se copia de alli). Antes de alegar cualquier conflicto o parada S-03, carga el contrato y cita en RoutingReason la orden y su salida que lo demuestran; sin esa prueba no hay conflicto y emites la delegacion completa.
- PROHIBIDO, en TODAS las cadenas copiadas: mejorar la gramatica; normalizar singular o plural o la concordancia; cambiar la puntuacion; anadir o quitar espacios;
  anadir o quitar tildes; cambiar mayusculas; reflujar, partir o unir el texto; parafrasear; corregir lo que parezca una errata. Si una frase te parece incorrecta, se copia IGUAL.
- AllowedWriteScope es la barrera PRIMARIA y exacta de escritura: copialo elemento a elemento, en el mismo orden, sin anadir, quitar, ensanchar ni normalizar ninguna entrada.
  ForbiddenWriteScope tiene exactamente las 106 entradas del contrato: ni una mas ni una menos, y ninguna con espacios anadidos.
- Para ForbiddenWriteScope y AllowedWriteScope, ejecuta ademas una orden que imprima cada entrada del contrato NUMERADA (por ejemplo: python -c "import json; c=json.load(open(r'artifacts/orchestration/I-64/F4-T4A1-PREPARE-CORE/1/R20261114T023014Z-4765/gate-contract.json', encoding='ascii')); [print(i, repr(x)) for i, x in enumerate(c['ForbiddenWriteScope'])]") y comprueba tu lista contra esa salida, indice por indice, de 0 a 105, mirando cada palabra de cada nombre de archivo (una planificacion anterior perdio la palabra Source en un nombre).
- Antes de dar la salida por terminada, compara tu salida con el contrato cargado campo por campo, cadena por cadena y contando los elementos de cada lista; la sesion ejecuta despues una comparacion determinista byte a byte
  de Invariants, RequiredTests, StopConditions, Authorities, ExpectedEvidence, Objective, AllowedWriteScope y ForbiddenWriteScope ANTES de aceptar: cualquier diferencia = salida RECHAZADA y sin Worker.
- TODA tu salida debe ser ASCII puro, tambien AcceptanceCriteria y RoutingReason: sin tildes, sin enye y sin signos no ASCII; ninguna secuencia de escape formada por barra invertida + u; ningun tabulador ni otro caracter de control dentro de las cadenas.
- RequiredTests con los mismos dos elementos y los mismos valores (ExpectRed true en el de Core y ExpectRed false en el de UI).

TASKCLASS
- TaskClass = exactamente "Implementacion transversal a capas (corta)"; PromptProfile = "ROUTINE_IMPLEMENTATION"; explica la eleccion en RoutingReason con las siete dimensiones (routing.md section 2) y la fecha de verificacion de la celda. Si tu evaluacion da Horizonte High: para con F4T4A1-C07 explicado en RoutingReason.
- Celda del Worker: Executor.Cell = "anthropic/subagent/claude-sonnet-5-5"; Executor.Provider = "Anthropic"; Executor.Transport = "subagent"; Model = "claude-sonnet-5-5"; Effort.Provider = "high"; Effort.Semantic = "Deep"; ModelEscalationReason = null.

DELTA DE LA TAREA (primera delegacion de la cadena F4-T4A1-PREPARE-CORE)
- RunId = R20261114T023014Z-4765 (tambien es el DelegationRunId); Attempt = 1; AttemptsRemaining = 2; MaxReworkLoops = 3.
- Unit = "I-64"; Initiative = "I-64"; Gate = "F4"; TaskId = "F4-T4A1-PREPARE-CORE".
- ExpectedBranch = la salida de `git branch --show-current`; BaseSha = la salida de `git rev-parse HEAD`; ChainBaseSha = ese mismo BaseSha; ChainRedSha = null; ChainRedFiles = []; CorrectionOf = null.
- MainSha = 812baf879612320653094a8aaf46aca0bac81383 (comprueba que coincide con `git rev-parse origin/main`; si no coincide, para con S-13 explicado en RoutingReason); AuthorityRevision = la del contrato.
- ExpectedWorktree = la ruta absoluta del directorio de trabajo actual.
- ExpectedHandoffPath = artifacts/orchestration/I-64/F4-T4A1-PREPARE-CORE/1/{WorkRunId}/worker-handoff.json (con el token literal).
- Owner = { Kind: "subagent", Id: "subagent:R20261114T023014Z-4765" }; RoutingEnforcement = "required". Executor.Role = "Worker".
- AcceptanceCriteria: redactalos tu, en ASCII, verificables y trazables a los invariantes del contrato; incluye el RED con el esqueleto de INV-F4T4A1-13 (en Core solo las pruebas nuevas o cambiadas fallan, por asercion; el filtro de UI pasa entero; el censo de Compose pasa), las pruebas de Core de ConfirmBase (las cinco filas), de la sobrecarga de Conservar (orden, atomicidad y admision antes de keepOn), de las tres operaciones del host (aplicabilidad antes de leer, una lectura, el consumo, _knownClean y la contencion), del planificador (los siete resultados en su orden, Conflict antes que la admision, los casos (a) a (e) de Invalid, el contenido y la igualdad del plan, que Prepare no cambia el borrador) y de los arrastres de ComposeForWrite; las guardas nuevas y las extendidas con sus autopruebas en negativo; el oraculo de cero escrituras authored; el GREEN que no toca tests/; el build Release de la solucion, Plugin incluido, sin errores ni avisos nuevos (tambien con el esqueleto, en local); la CI del SHA exacto y los conteos minimos de RequiredTests.
- IssuedBy = "Controller Codex (planificacion F4-T4A1-PREPARE-CORE, primera delegacion de la cadena, CONTROLLER_PLANNING)".
