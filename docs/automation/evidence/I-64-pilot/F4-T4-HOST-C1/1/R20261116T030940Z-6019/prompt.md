I-64 - DELEGACION F4-T4-HOST-C1 - Controller, perfil CONTROLLER_PLANNING (primera delegacion de la cadena F4-T4-HOST-C1)
Identidad: unidad I-64; gate F4; tarea F4-T4-HOST-C1; intento 1; RunId R20261116T030940Z-6019;
           DelegationRunId R20261116T030940Z-6019; AuthorityRevision b28e75ad8a31bd490d67adbc74a294f35082c17c (la del contrato);
           MainSha 812baf879612320653094a8aaf46aca0bac81383; BaseSha = el HEAD actual del worktree (se espera 16ce44459f05c6b80d48dee63e1210b1acd5f3e1: commit de emision del contrato, hijo directo de la AuthorityRevision b28e75ad8a31bd490d67adbc74a294f35082c17c:
           solo anade el contrato, el estado y la evidencia);
           rama architecture/workspace-persistente-rackcad; worktree = el directorio de trabajo actual; Owner subagent:R20261116T030940Z-6019
Contrato de gate: artifacts/orchestration/I-64/F4-T4-HOST-C1/1/R20261116T030940Z-6019/gate-contract.json (ASCII puro; custodiado en docs/automation/evidence/I-64-pilot/F4-T4-HOST-C1/coordinator/gate-contract.json)
Autoridades (leer desde Git, no copiar su contenido): las del campo Authorities del contrato;
           vigencia: AuthorityRevision para la unidad (UNIT_DOC y UNIT_CHANGE), MainSha para lo demas (EXTERNAL)
Objetivo: emitir el paquete de delegacion (esquema rackcad-delegation/v1) del Worker para la tarea del contrato. Debe ser ejecutable tal cual.
Alcance: solo lectura. No escribas ningun archivo, no hagas commit ni push. No ejecutes dotnet, ni AutoCAD, ni builds.
Invariantes del Freeze: los 11 del contrato; una delegacion nunca amplia el contrato
Rutas: ver ALCANCE DE ESCRITURA abajo. Todo lo demas es de solo lectura.
           Fuente de diseno: docs/initiatives/I-64-f4-t4-host-c1-gate-contract-package.md v2 (section 2 hechos H-01 a H-13, section 3 alcance, section 4 decisiones D-C1-01 a D-C1-09, section 5 invariantes, section 6 IC-10, section 7 paradas, section 10 disposicion de la revision); la IC-10 de docs/initiatives/I-64-implementation-clarifications.md; el diagnostico docs/automation/evidence/I-64-pilot/F4-DRAFTS/c1/diagnosis-report.md; el mapa de guardas docs/automation/evidence/I-64-pilot/F4-DRAFTS/c1/map.md; y la orden del Owner docs/automation/evidence/I-64-pilot/F4-DRAFTS/coordinator/owner-f4-smoke2-fix-order.md
Criterios de aceptacion: la salida valida contra el esquema; colecciones opacas del contrato copiadas byte a byte (ver COPIA EXACTA);
           AllowedWriteScope segun ALCANCE DE ESCRITURA; celda y effort entre las EligibleCells del contrato; TaskClass y campos de la DELTA exactamente como se indican
Evidencia requerida: ejecuta de verdad estas ordenes en el worktree y usa sus resultados:
           git branch --show-current ; git rev-parse HEAD ; git rev-parse origin/main ;
           git ls-files src/RackCad.Application/Systems/PushBack/Pilot (12 archivos) ; git ls-files src/RackCad.Application/Workspace (40 archivos) ; git ls-files src/RackCad.Plugin/Views (14 archivos) ; git ls-files src/RackCad.Plugin/Workspace (9 archivos) ; git ls-files tests/RackCad.Tests/Workspace (77 archivos) ;
           y lee el codigo, las pruebas y las guardas que cites
No-touch: todo el repositorio
Condiciones de parada: S-03, P-03, las F4T4C-C* que conserva el contrato y las C1-S01 a C1-S06; ante cualquiera, para y explica el bloqueo en RoutingReason.
COPIA: en las tareas anteriores de esta unidad, varias planificaciones se rechazaron solo por la copia de las colecciones opacas: un elemento de una lista omitido, un espacio anadido al principio de una cadena, una palabra perdida dentro de un nombre de archivo o un verbo cambiado de modo. Ninguna entrada de ninguna lista empieza ni termina con un espacio.
Una sola diferencia de un caracter o un solo elemento de mas o de menos basta para rechazar. Por la parada C1-S06 del contrato, un rechazo admite una sola replanificacion y un segundo va con STOP al Owner (D4).
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

CONTEXTO (hechos versionados en el HEAD; evidencia section 217 y siguientes de docs/automation/evidence/I-64-evidence.md, en particular 221 y 222; F4-E70 a F4-E74 en docs/automation/evidence/I-64-pilot/F4-DRAFTS/f4-dispositions.md)
- Smoke-2 (el Owner operando en AutoCAD con un dibujo real) encontro dos defectos del producto b88dabf2 y el Owner ordeno corregirlos antes de seguir: F4-E72 (el Actualizar del piloto redibuja sin el rotulo del nombre del rack, porque el escritor reconstruye el sistema sin el Name del sobre) y F4-S2-01 (cada lectura del panel y cada fin de orden disparan ObjectModified de AssocNetwork, el puente los reenvia y el nucleo los encola como cambio ajeno, lo que marca el borrador como posiblemente obsoleto). El diagnostico en AutoCAD con una build desechable valido las dos correcciones. F4-T4-HOST-C1 las implementa: C1 (PushBackPilotSystemRebuild.BuildForMember y su llamada en el escritor) y C2 (ObjectEventSubject, EventBridgeCore.OnObjectEvent y SubjectOf en el puente, con IC-10). F4-S2-03 (Undo y Regen) queda FUERA (INV-C1-07, C1-S04).
  Presupuesto: Attempt 1, AttemptsRemaining 2, MaxReworkLoops 3, CorrectionsAuthorized true; 1 invocacion de Worker por delegacion aceptada.
- HECHOS DEL REPOSITORIO medidos por la sesion en el HEAD (comprueba los que uses):
  * Las 9 rutas existentes del AllowedWriteScope existen; las 2 nuevas (tests/RackCad.Tests/Workspace/WorkspaceAssociativeBookkeepingTests.cs y tests/RackCad.Tests/Workspace/WorkspaceBridgeObjectSubjectGuardTests.cs) no existen todavia. El producto y las pruebas del HEAD son los de b88dabf2 (git diff b88dabf2 HEAD -- src tests vacio).
  * Cuentas de la base (CI push 43758023582 de b88dabf2): el filtro FullyQualifiedName~RackCad.Tests.Workspace selecciona 3437 y el de UI FullyQualifiedName~RackCad.UI.Tests.Workspace 694, todas en verde; PushBackClassicCharacterization 55; cada una de las 23 clases de regresion de Core, la cuenta de su fila de RequiredTests. Solo el filtro de Workspace de Core lleva ExpectRed true; la UI no cambia en esta tarea.
  * El RED (INV-C1-10 y D-C1-09 del paquete): cuerpos inertes (BuildForMember devuelve Build(member.DesignText, catalog) sin nombre; OnObjectEvent es return OnSourceEvent(session, source); sin su propio olvido ni su clasificacion; el enumerado ObjectEventSubject y la forma nueva de los cinco bloques del puente ya estan). En el RED pasan el cambio de guarda 1 y las fijaciones; fallan por asercion los cambios de guarda 2 a 5 (el texto aun tiene 16 olvidos) y las pruebas nuevas de comportamiento. La CI del RED: el job de Core en failure (ExpectRed true solo en el filtro de Workspace de Core); el de UI en success.
  * Lo que esta FIJADO en el paquete y se traslada a los AcceptanceCriteria como receta, sin reconstruirlo: el contrato de BuildForMember (D-C1-01: argumentos nulos, sobre ilegible con UnreadableEnvelopeReason, Name en crudo); la forma exacta de OnObjectEvent (D-C1-02: firma, el olvido justo despues de if (!_started) return false; y antes del try, la contabilidad sin pista ni SyncIdle, todo lo demas igual que OnSourceEvent); SubjectOf solo por tipo; los cinco cambios de guarda exactos de D-C1-03 con su autoprueba en negativo; las pruebas RED 1 a 5 de D-C1-09 con sus nombres.
  * Reglas de las corridas locales (F4-E59, INV-C1-11): nunca la suite completa de UI en local, solo sus dos filtros de RequiredTests; ninguna prueba nueva escribe fuera de una ubicacion temporal aislada.
  * Trampas medidas (mapa y revision): OnSourceEvent(SessionId, BridgeSource) se busca por su firma textual en tests/RackCad.Tests/Workspace/WorkspaceLifecycleGuardTests.cs:133 y tests/RackCad.Tests/Workspace/WorkspaceHostC2SourceGuardTests.cs:205 (no cambia); tests/RackCad.Tests/Workspace/WorkspaceIdleSchedulingGuardTests.cs:129 exige que el literal ContainSession(a, () => _core.OnSourceEvent(a.Session, a.Source)); siga en el puente (sigue en ImpliedSelectionChanged y en los tres finales de orden); los nombres de tipo de RackCad.Application.Workspace no pueden casar con Subscription, Editor, Draft ni Tab (tests/RackCad.Tests/Workspace/WorkspaceModelBoundaryTests.cs, tests/RackCad.Tests/Workspace/WorkspaceBrowserBoundaryTests.cs); la pureza de PushBackPilotSystemRebuild.cs (tests/RackCad.Tests/Workspace/WorkspacePilotUpdateGuardTests.cs PureFiles); los kits PrepareKit solo se deserializan (tests/RackCad.Tests/Workspace/WorkspacePilotUpdateGuardTests.cs:848-866). Ninguna de esas se toca; la entrega dice como las respeta.

ALCANCE DE ESCRITURA
- AllowedWriteScope de la delegacion = EXACTAMENTE las 11 entradas del contrato, en su orden (11 archivos exactos, sin prefijos):
    src/RackCad.Application/Systems/PushBack/Pilot/PushBackPilotSystemRebuild.cs
    src/RackCad.Plugin/Views/RackPilotUpdateWriter.cs
    src/RackCad.Application/Workspace/EventBridgeCore.cs
    src/RackCad.Application/Workspace/BridgeSource.cs
    src/RackCad.Plugin/Workspace/AutoCadEventBridge.cs
    tests/RackCad.Tests/Workspace/WorkspacePilotUpdateRunTests.cs
    tests/RackCad.Tests/Workspace/WorkspacePilotUpdateGuardTests.cs
    tests/RackCad.Tests/Workspace/WorkspaceBridgeFacadeSourceGuardTests.cs
    tests/RackCad.Tests/Workspace/WorkspaceLifecycleGuardTests.cs
    tests/RackCad.Tests/Workspace/WorkspaceAssociativeBookkeepingTests.cs
    tests/RackCad.Tests/Workspace/WorkspaceBridgeObjectSubjectGuardTests.cs
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
- Todas las cadenas del contrato canonico artifacts/orchestration/I-64/F4-T4-HOST-C1/1/R20261116T030940Z-6019/gate-contract.json son ASCII puro. Se copian byte a byte.
- Authorities, ForbiddenWriteScope, Invariants, RequiredTests, ExpectedEvidence, StopConditions y Objective son DATOS OPACOS de ese contrato. CARGALOS desde el archivo con una orden y reproducelos en tu salida a partir de esa carga, por ejemplo:
  python -c "import json; c=json.load(open(r'artifacts/orchestration/I-64/F4-T4-HOST-C1/1/R20261116T030940Z-6019/gate-contract.json', encoding='ascii')); print(json.dumps({k: c[k] for k in ['Authorities','ForbiddenWriteScope','Invariants','RequiredTests','ExpectedEvidence','StopConditions','Objective','AllowedWriteScope']}, ensure_ascii=True, separators=(chr(44), chr(58))))"
  Mismo orden, mismos elementos, mismos textos byte a byte.
- MODO VERBAL Y GRAMATICA: el contrato usa a proposito construcciones que pueden parecer mejorables (indicativo tras si, con o comprueba; nombres en vez de verbos); se copian IGUAL. Antes de terminar, imprime cada invariante del contrato NUMERADO (por ejemplo: python -c "import json; c=json.load(open(r'artifacts/orchestration/I-64/F4-T4-HOST-C1/1/R20261116T030940Z-6019/gate-contract.json', encoding='ascii')); [print(i, x) for i, x in enumerate(c['Invariants'])]") y compara tu salida con esa impresion, invariante por invariante y verbo por verbo.
- AUTHORITIES: se copian cada una en su indice y con su nombre exacto: Authorities[0].Path = "docs/initiatives/I-64-f4-t4-host-c1-gate-contract-package.md" (el paquete de esta tarea); Authorities[7].Path = "docs/initiatives/I-64-implementation-clarifications.md"; Authorities[8].Path = "docs/initiatives/I-64-f4-t4c-gate-contract-package.md" (f4-t4c); Authorities[9].Path = "docs/initiatives/I-64-f4-t2b-gate-contract-package.md" (f4-t2b); Authorities[10].Path = "docs/initiatives/I-64-proposal-v5.md". No los mezcles ni los acortes.
- SIN CONFLICTOS INEXISTENTES: el contrato canonico de esta corrida es ASCII PURO (0 bytes no ASCII; compruebalo con python -c "b=open(r'artifacts/orchestration/I-64/F4-T4-HOST-C1/1/R20261116T030940Z-6019/gate-contract.json', 'rb').read(); print(sum(1 for x in b if x >= 0x80))"); TaskClass se escribe exactamente como lo fija este prompt, sin tilde (la tabla de routing.md lleva tilde y NO se copia de alli). Antes de alegar cualquier conflicto o parada S-03, carga el contrato y cita en RoutingReason la orden y su salida que lo demuestran; sin esa prueba no hay conflicto y emites la delegacion completa.
- PROHIBIDO, en TODAS las cadenas copiadas: mejorar la gramatica; normalizar singular o plural o la concordancia; cambiar la puntuacion; anadir o quitar espacios;
  anadir o quitar tildes; cambiar mayusculas; reflujar, partir o unir el texto; parafrasear; corregir lo que parezca una errata. Si una frase te parece incorrecta, se copia IGUAL.
- AllowedWriteScope es la barrera PRIMARIA y exacta de escritura: copialo elemento a elemento, en el mismo orden, sin anadir, quitar, ensanchar ni normalizar ninguna entrada.
  ForbiddenWriteScope tiene exactamente las 119 entradas del contrato: ni una mas ni una menos, y ninguna con espacios anadidos.
- FORBIDDENWRITESCOPE, COPIA REFORZADA: tiene exactamente 119 entradas, de la 0 a la 118, todas distintas. Para copiarla, ejecuta esta orden, que la imprime SOLA como un array JSON compacto en una linea, sin ningun espacio entre los elementos, y reproduce cada cadena tal cual, de una vez, sin escribir las entradas de memoria:
  python -c "import json; c=json.load(open(r'artifacts/orchestration/I-64/F4-T4-HOST-C1/1/R20261116T030940Z-6019/gate-contract.json', encoding='ascii')); print(json.dumps(c['ForbiddenWriteScope'], ensure_ascii=True, separators=(chr(44), chr(58))))"
  Puntos de control, comprobados contra la impresion numerada: de la 0 a la 11, las raices (la 0 es docs/ y la 11 README.md); la 12 es src/RackCad.UI/, la 13 tests/RackCad.UI.Tests/ y la 14 src/RackCad.Domain/; la 15 es src/RackCad.Plugin/Drawing/, la 16 src/RackCad.Plugin/KindHandlers/ y la 17 src/RackCad.Plugin/Systems/; de la 18 a la 35, los 18 archivos *Commands*.cs de la raiz de src/RackCad.Plugin/ (la 18 es src/RackCad.Plugin/RackAyudaCommands.cs y la 35 src/RackCad.Plugin/RackVariablesCommands.cs); de la 36 a la 38, tres archivos de src/RackCad.Plugin/Views/ (RackPilotUpdateCommandContext.cs, RackSiblingScan.cs, RackPilotIntentAdapter.cs) y la 39 src/RackCad.Plugin/Workspace/WorkspaceController.cs; de la 40 a la 57, las 18 subraices de src/RackCad.Application/ que no son Systems ni Workspace (la 40 es src/RackCad.Application/Bom/ y la 57 src/RackCad.Application/Views/); de la 58 a la 63, seis subraices de src/RackCad.Application/Systems/ (la 58 Cantilever/ y la 63 Shared/); de la 64 a la 68, cinco archivos de src/RackCad.Application/Systems/PushBack/Pilot/; la 69 es src/RackCad.Application/Workspace/Drafts/; de la 70 a la 102, los 33 archivos de src/RackCad.Application/Workspace/ que no se tocan (la 70 es BridgeOutcomes.cs, la 74 Hints.cs, la 93 WorkspaceHostCore.cs y la 102 WorkspaceSessionRegistry.cs); de la 103 a la 118, dieciseis archivos de tests/RackCad.Tests/Workspace/ (la 103 es WorkspaceHostTestSupport.cs y la 118, la ULTIMA de la lista, WorkspacePilotPrepareTests.cs).
  Ahi termina la lista: nunca vuelvas atras ni repitas un nombre.
  Antes de terminar, cuenta los elementos de tu ForbiddenWriteScope (deben ser 119), comprueba que no hay repetidos, compara cada indice con la impresion numerada y comprueba que ninguna entrada empieza ni termina con un espacio.
- Para ForbiddenWriteScope y AllowedWriteScope, ejecuta ademas una orden que imprima cada entrada del contrato NUMERADA (por ejemplo: python -c "import json; c=json.load(open(r'artifacts/orchestration/I-64/F4-T4-HOST-C1/1/R20261116T030940Z-6019/gate-contract.json', encoding='ascii')); [print(i, repr(x)) for i, x in enumerate(c['ForbiddenWriteScope'])]") y comprueba tu lista contra esa salida, indice por indice, de 0 a 118, mirando cada palabra de cada nombre de archivo (una planificacion anterior perdio la palabra Source en un nombre).
- Antes de dar la salida por terminada, compara tu salida con el contrato cargado campo por campo, cadena por cadena y contando los elementos de cada lista; la sesion ejecuta despues una comparacion determinista byte a byte
  de Invariants, RequiredTests, StopConditions, Authorities, ExpectedEvidence, Objective, AllowedWriteScope y ForbiddenWriteScope ANTES de aceptar: cualquier diferencia = salida RECHAZADA y sin Worker.
- TODA tu salida debe ser ASCII puro, tambien AcceptanceCriteria y RoutingReason: sin tildes, sin enye y sin signos no ASCII; ninguna secuencia de escape formada por barra invertida + u; ningun tabulador ni otro caracter de control dentro de las cadenas.
- RequiredTests con los mismos 26 elementos, en el mismo orden y con los mismos valores (ExpectRed true SOLO en el primero, FullyQualifiedName~RackCad.Tests.Workspace; false en los otros 25, incluido FullyQualifiedName~RackCad.UI.Tests.Workspace).

TASKCLASS
- TaskClass = exactamente "Implementacion transversal a capas (corta)"; PromptProfile = "ROUTINE_IMPLEMENTATION"; explica la eleccion en RoutingReason con las siete dimensiones (routing.md section 2) y la fecha de verificacion de la celda: Application (PushBackPilotSystemRebuild, EventBridgeCore y BridgeSource), el Plugin (una llamada del escritor y los cinco manejadores de objeto del puente) y pruebas de Core. Si tu evaluacion da Horizonte High: para con S-03 explicado en RoutingReason.
- Celda del Worker: Executor.Cell = "anthropic/subagent/claude-sonnet-5-5"; Executor.Provider = "Anthropic"; Executor.Transport = "subagent"; Model = "claude-sonnet-5-5"; Effort.Provider = "high"; Effort.Semantic = "Deep"; ModelEscalationReason = null.

DELTA DE LA TAREA (primera delegacion de la cadena F4-T4-HOST-C1)
- RunId = R20261116T030940Z-6019 (tambien es el DelegationRunId); Attempt = 1; AttemptsRemaining = 2; MaxReworkLoops = 3.
- Unit = "I-64"; Initiative = "I-64"; Gate = "F4"; TaskId = "F4-T4-HOST-C1".
- ExpectedBranch = la salida de `git branch --show-current`; BaseSha = la salida de `git rev-parse HEAD`; ChainBaseSha = ese mismo BaseSha; ChainRedSha = null; ChainRedFiles = []; CorrectionOf = null.
- MainSha = 812baf879612320653094a8aaf46aca0bac81383 (comprueba que coincide con `git rev-parse origin/main`; si no coincide, para con S-13 explicado en RoutingReason); AuthorityRevision = la del contrato.
- ExpectedWorktree = la ruta absoluta del directorio de trabajo actual.
- ExpectedHandoffPath = artifacts/orchestration/I-64/F4-T4-HOST-C1/1/{WorkRunId}/worker-handoff.json (con el token literal).
- Owner = { Kind: "subagent", Id: "subagent:R20261116T030940Z-6019" }; RoutingEnforcement = "required". Executor.Role = "Worker".
- AcceptanceCriteria: redactalos tu, en ASCII, verificables y trazables a los invariantes del contrato; incluye: el RED con los cuerpos inertes de INV-C1-10, con las fijaciones y el cambio de guarda 1 en verde y los cambios de guarda 2 a 5 y las pruebas nuevas en rojo por asercion, declarados; el GREEN dentro del AllowedWriteScope sin tocar tests/ del RED; BuildForMember con su contrato de D-C1-01 y el escritor que lo llama una vez sin nombrar Build; OnObjectEvent con su firma, el olvido en su sitio, la contabilidad sin pista y todo lo demas igual que OnSourceEvent; SubjectOf solo por tipo y sin leer el objeto; los cinco bloques de objeto del puente con su forma exacta y los demas bloques sin cambios; OnSourceEvent sin cambios; los cinco cambios de guarda de INV-C1-06, exactos y con su autoprueba en negativo; ninguna regeneracion ni tratamiento de Undo (INV-C1-07); las pruebas RED 1 a 5 del paquete; las pruebas de T1B y las 23 clases de regresion en verde; la contencion de las corridas locales, sin la suite completa de UI; la CI exacta del RED y del GREEN.
- IssuedBy = "Controller Codex (planificacion F4-T4-HOST-C1, primera delegacion de la cadena, CONTROLLER_PLANNING)".
