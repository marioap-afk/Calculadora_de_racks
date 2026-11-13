I-64 - DELEGACION F4-T3C1-HOST-HARDENING - Controller, perfil CONTROLLER_PLANNING (primera delegacion de la cadena F4-T3C1-HOST-HARDENING)
Identidad: unidad I-64; gate F4; tarea F4-T3C1-HOST-HARDENING; intento 1; RunId R20261113T164804Z-8f54;
           DelegationRunId R20261113T164804Z-8f54; AuthorityRevision 3a53ee2dc673a114b6f8dcd9ca68f7f3f5331a82 (la del contrato);
           MainSha 812baf879612320653094a8aaf46aca0bac81383; BaseSha = el HEAD actual del worktree (se espera c04ddfb3d5a08f11f27727219c96c2934241f255: commit de emision del contrato, hijo directo de la AuthorityRevision 3a53ee2dc673a114b6f8dcd9ca68f7f3f5331a82:
           solo anade el contrato, el estado y la evidencia);
           rama architecture/workspace-persistente-rackcad; worktree = el directorio de trabajo actual; Owner subagent:R20261113T164804Z-8f54
Contrato de gate: artifacts/orchestration/I-64/F4-T3C1-HOST-HARDENING/1/R20261113T164804Z-8f54/gate-contract.json (ASCII puro; custodiado en docs/automation/evidence/I-64-pilot/F4-T3C1-HOST-HARDENING/coordinator/gate-contract.json)
Autoridades (leer desde Git, no copiar su contenido): las del campo Authorities del contrato;
           vigencia: AuthorityRevision para la unidad (UNIT_DOC y UNIT_CHANGE), MainSha para lo demas (EXTERNAL)
Objetivo: emitir el paquete de delegacion (esquema rackcad-delegation/v1) del Worker para la tarea del contrato. Debe ser ejecutable tal cual.
Alcance: solo lectura. No escribas ningun archivo, no hagas commit ni push. No ejecutes dotnet, ni AutoCAD, ni builds.
Invariantes del Freeze: los 12 del contrato; una delegacion nunca amplia el contrato
Rutas: ver ALCANCE DE ESCRITURA abajo. Todo lo demas es de solo lectura.
           Fuente de diseno: docs/initiatives/I-64-f4-t3c-gate-contract-package.md (v3: section 2, hechos H-01 a H-08 con la tabla cerrada H-07 de 28 literales, menores M-1 a M-7 y trampas de guardas G-01 a G-13; section 3, decisiones D-T3C1-01 a D-T3C1-08; section 4, pruebas; section 5, contrato propuesto; section 6, reglas de F4-T3C2; sections 8 y 9, disposiciones de las revisiones); docs/initiatives/I-64-f4-t3-gate-contract-package.md section 8 (R-C1 a R-C4); docs/initiatives/I-64-f4-t3b2-gate-contract-package.md (D-T3B2-03, D-T3B2-04, R-B2-5); los paquetes de cierre de F4-T3A, F4-T3B1 y F4-T3B2 (section 5); las ordenes del Owner owner-f4t3b1-close-disposition.md (F4-E43) y owner-f4t3c-decomposition-decision.md; el contrato.
Criterios de aceptacion: la salida valida contra el esquema; colecciones opacas del contrato copiadas byte a byte (ver COPIA EXACTA);
           AllowedWriteScope segun ALCANCE DE ESCRITURA; celda y effort entre las EligibleCells del contrato; TaskClass y campos de la DELTA exactamente como se indican
Evidencia requerida: ejecuta de verdad estas ordenes en el worktree y usa sus resultados:
           git branch --show-current ; git rev-parse HEAD ; git rev-parse origin/main ;
           git ls-files src/RackCad.Plugin/Workspace (9 archivos) ; git ls-files src/RackCad.UI/Workspace (8 archivos) ; git ls-files tests/RackCad.UI.Tests/Workspace (21 archivos) ;
           y lee el codigo, las pruebas y las guardas que cites
No-touch: todo el repositorio
Condiciones de parada: S-03, P-03 y las F4T3C1-C* del contrato; ante cualquiera, para y explica el bloqueo en RoutingReason.
Antecedentes: en esta unidad, cinco planificaciones se RECHAZARON por la copia: "lo leen" cambiado por "los leen" en un invariante (A5 FAIL); UNA entrada de ForbiddenWriteScope omitida (A4 FAIL); un espacio anadido delante de una entrada (" .github/", A4 FAIL); "WorkspacePaletteHostGuardTests.cs" en vez de "WorkspacePaletteHostSourceGuardTests.cs" (le falto la palabra Source; A4 FAIL); y, en la tarea anterior, "mientras la profundidad es mayor que 0" copiado como "sea" (modo verbal cambiado de indicativo a subjuntivo; A5 FAIL). Una sola diferencia de un caracter o un solo elemento de mas o de menos basta para rechazar. Por la parada F4T3C1-C08 del contrato, un rechazo admite una sola replanificacion y un segundo vuelve al Coordinator.
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

CONTEXTO (hechos versionados en el HEAD; evidencia section 177 y siguientes de docs/automation/evidence/I-64-evidence.md; F4-E46 y F4-E47 en docs/automation/evidence/I-64-pilot/F4-DRAFTS/f4-dispositions.md)
- F4-T1, F4-T2, F4-T3A, F4-T3B1 y F4-T3B2 estan cerradas. El Owner dividio F4-T3C en dos subtareas, cada una con su propio presupuesto, en orden C1, C2. Esta tarea es F4-T3C1-HOST-HARDENING, sin tocar el Plugin: el desconocido explicito de GetRacksAtRisk (PilotRiskView.IsKnown) y sus consumidores en la UI, las colas de avisos con try, DropCloseNotice dentro de la contencion de SessionDestroyed, los motivos del puerto del piloto en espanol con la tabla cerrada H-07, el motivo del Error a traves del puerto (ErrorReason) y los menores M-1 a M-5 y M-7 de F4-T3B2 (M-6 no cambia). Archivo nuevo: tests/RackCad.Tests/Workspace/WorkspaceHostHardeningGuardTests.cs. Oraculo critico del Owner: cero escrituras authored en el DWG; ninguna lectura nueva de la intencion.
  Presupuesto: Attempt 1, AttemptsRemaining 2, MaxReworkLoops 3, CorrectionsAuthorized true; 1 invocacion de Worker por delegacion aceptada.
- HECHOS DEL REPOSITORIO medidos por la sesion en el HEAD (comprueba los que uses):
  * Lineas base medidas en los TRX de la CI del GREEN de F4-T3B2 (23f67f56, CI 43440641847; desde entonces solo documentacion): FullyQualifiedName~RackCad.Tests.Workspace = 2331 y FullyQualifiedName~RackCad.UI.Tests.Workspace = 431, todas en verde. Las dos tienen ExpectRed true: en el RED fallan solo las pruebas nuevas, por asercion, y todas las existentes pasan (INV-F4T3C1-11).
  * El filtro de UI selecciona las clases cuyo nombre completo contiene RackCad.UI.Tests.Workspace: las clases de prueba estan en el namespace RackCad.UI.Tests y su nombre empieza por Workspace, asi que las clases nuevas tambien empiezan por Workspace.
  * La CI corre las pruebas en Debug y compila el Plugin en Release (job build-plugin); el build local del Plugin en Release produce 2 avisos MSB3277 como linea base. La suite completa de UI en Release da 2 fallos de PendingTextFieldTests.ShowOnADirtyField_* que dependen de #if DEBUG y pasan en Debug: condicion previa conocida.
  * Los textos exactos estan FIJADOS en el paquete: la tabla H-07 (28 literales en ingles y su espanol exacto) y los textos de D-T3C1-02 y D-T3C1-07; la regla de extraccion y la lista de 33 firmas de M-5; la expresion de M-4 y sus siete autopruebas. Trasladalos a los AcceptanceCriteria como receta, sin reconstruirlos.
  * Las trampas G-01 a G-13 del section 2 del paquete estan medidas: la forma del puerto (tipos valor, campos readonly, sin tipos nuevos); las subcadenas prohibidas en el puerto, el support y el host (G-04) y en la UI; SessionDestroyed con exactamente dos try; .IsSessionAlive( exactamente una vez en el VM; la prueba de F3 WorkspaceBrowserViewModelTests.cs:1006-1019 no cambia; la frase Building the panel never calls it prohibida en el VM del panel. Cambios declarados de pruebas existentes: solo las guardas de M-4 y M-5 en WorkspacePilotCloseSyncGuardTests.cs.
  * El doble WorkspacePilotEditPortDouble existe; su respuesta por defecto de GetRacksAtRisk construye las dos listas (conocida, nada en riesgo) y asi sigue; un RisksResponder puede devolver default para el desconocido.

ALCANCE DE ESCRITURA
- AllowedWriteScope de la delegacion = EXACTAMENTE las 12 entradas del contrato, en su orden (12 archivos exactos, sin prefijos):
    src/RackCad.Application/Workspace/IWorkspacePilotEditPort.cs
    src/RackCad.Application/Workspace/WorkspaceHostCore.cs
    src/RackCad.Application/Workspace/WorkspacePilotSupport.cs
    src/RackCad.UI/Workspace/WorkspaceBrowserViewModel.cs
    src/RackCad.UI/Workspace/WorkspacePanelViewModel.cs
    src/RackCad.UI/Workspace/WorkspacePilotFieldViewModel.cs
    tests/RackCad.Tests/Workspace/WorkspacePilotEditHostTests.cs
    tests/RackCad.Tests/Workspace/WorkspacePilotCloseSyncGuardTests.cs
    tests/RackCad.Tests/Workspace/WorkspaceHostHardeningGuardTests.cs
    tests/RackCad.UI.Tests/Workspace/WorkspacePilotEditPortDouble.cs
    tests/RackCad.UI.Tests/Workspace/WorkspacePilotCloseSyncTests.cs
    tests/RackCad.UI.Tests/Workspace/WorkspacePilotFieldViewModelTests.cs
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
- Todas las cadenas del contrato canonico artifacts/orchestration/I-64/F4-T3C1-HOST-HARDENING/1/R20261113T164804Z-8f54/gate-contract.json son ASCII puro. Se copian byte a byte.
- Authorities, ForbiddenWriteScope, Invariants, RequiredTests, ExpectedEvidence, StopConditions y Objective son DATOS OPACOS de ese contrato. CARGALOS desde el archivo con una orden y reproducelos en tu salida a partir de esa carga, por ejemplo:
  python -c "import json; c=json.load(open(r'artifacts/orchestration/I-64/F4-T3C1-HOST-HARDENING/1/R20261113T164804Z-8f54/gate-contract.json', encoding='ascii')); print(json.dumps({k: c[k] for k in ['Authorities','ForbiddenWriteScope','Invariants','RequiredTests','ExpectedEvidence','StopConditions','Objective','AllowedWriteScope']}, ensure_ascii=True))"
  Mismo orden, mismos elementos, mismos textos byte a byte.
- MODO VERBAL Y GRAMATICA: el contrato usa a proposito construcciones que pueden parecer mejorables (indicativo tras si, con o comprueba; nombres en vez de verbos); se copian IGUAL. Antes de terminar, imprime cada invariante del contrato NUMERADO (por ejemplo: python -c "import json; c=json.load(open(r'artifacts/orchestration/I-64/F4-T3C1-HOST-HARDENING/1/R20261113T164804Z-8f54/gate-contract.json', encoding='ascii')); [print(i, x) for i, x in enumerate(c['Invariants'])]") y compara tu salida con esa impresion, invariante por invariante y verbo por verbo. Las expresiones regulares de INV-F4T3C1-12 llevan barras invertidas: se copian con las mismas barras.
- AUTHORITIES: tres autoridades citan paquetes con nombres casi iguales; se copian cada uno en su indice y con su nombre exacto: Authorities[0].Path = "docs/initiatives/I-64-f4-t3c-gate-contract-package.md" (t3c: el paquete de esta tarea); Authorities[1].Path = "docs/initiatives/I-64-f4-t3-gate-contract-package.md" (t3, sin c); Authorities[2].Path = "docs/initiatives/I-64-f4-t3b2-gate-contract-package.md" (t3b2). Una planificacion anterior de esta unidad copio t3 en vez de t3b y se RECHAZO. Antes de terminar imprime cada autoridad NUMERADA (por ejemplo: python -c "import json; c=json.load(open(r'artifacts/orchestration/I-64/F4-T3C1-HOST-HARDENING/1/R20261113T164804Z-8f54/gate-contract.json', encoding='ascii')); [print(i, repr(x)) for i, x in enumerate(c['Authorities'])]") y comparala indice por indice y letra por letra.
- SIN CONFLICTOS INEXISTENTES: el contrato canonico de esta corrida es ASCII PURO (0 bytes no ASCII; compruebalo con python -c "b=open(r'artifacts/orchestration/I-64/F4-T3C1-HOST-HARDENING/1/R20261113T164804Z-8f54/gate-contract.json', 'rb').read(); print(sum(1 for x in b if x >= 0x80))"); TaskClass se escribe exactamente como lo fija este prompt, sin tilde (la tabla de routing.md lleva tilde y NO se copia de alli). Antes de alegar cualquier conflicto o parada S-03, carga el contrato y cita en RoutingReason la orden y su salida que lo demuestran; sin esa prueba no hay conflicto y emites la delegacion completa.
- PROHIBIDO, en TODAS las cadenas copiadas: mejorar la gramatica; normalizar singular o plural o la concordancia; cambiar la puntuacion; anadir o quitar espacios;
  anadir o quitar tildes; cambiar mayusculas; reflujar, partir o unir el texto; parafrasear; corregir lo que parezca una errata. Si una frase te parece incorrecta, se copia IGUAL.
- AllowedWriteScope es la barrera PRIMARIA y exacta de escritura: copialo elemento a elemento, en el mismo orden, sin anadir, quitar, ensanchar ni normalizar ninguna entrada.
  ForbiddenWriteScope tiene exactamente las 103 entradas del contrato: ni una mas ni una menos, y ninguna con espacios anadidos.
- Para ForbiddenWriteScope y AllowedWriteScope, ejecuta ademas una orden que imprima cada entrada del contrato NUMERADA (por ejemplo: python -c "import json; c=json.load(open(r'artifacts/orchestration/I-64/F4-T3C1-HOST-HARDENING/1/R20261113T164804Z-8f54/gate-contract.json', encoding='ascii')); [print(i, repr(x)) for i, x in enumerate(c['ForbiddenWriteScope'])]") y comprueba tu lista contra esa salida, indice por indice, de 0 a 102, mirando cada palabra de cada nombre de archivo (una planificacion anterior perdio la palabra Source en un nombre).
- Antes de dar la salida por terminada, compara tu salida con el contrato cargado campo por campo, cadena por cadena y contando los elementos de cada lista; la sesion ejecuta despues una comparacion determinista byte a byte
  de Invariants, RequiredTests, StopConditions, Authorities, ExpectedEvidence, Objective, AllowedWriteScope y ForbiddenWriteScope ANTES de aceptar: cualquier diferencia = salida RECHAZADA y sin Worker.
- TODA tu salida debe ser ASCII puro, tambien AcceptanceCriteria y RoutingReason: sin tildes, sin enye y sin signos no ASCII; ninguna secuencia de escape formada por barra invertida + u; ningun tabulador ni otro caracter de control dentro de las cadenas.
- RequiredTests con los mismos dos elementos y los mismos valores (ExpectRed true en los dos: en el de Core y en el de UI).

TASKCLASS
- TaskClass = exactamente "Implementacion transversal a capas (corta)"; PromptProfile = "ROUTINE_IMPLEMENTATION"; explica la eleccion en RoutingReason con las siete dimensiones (routing.md section 2) y la fecha de verificacion de la celda. Si tu evaluacion da Horizonte High: para con F4T3C1-C07 explicado en RoutingReason.
- Celda del Worker: Executor.Cell = "anthropic/subagent/claude-sonnet-5-5"; Executor.Provider = "Anthropic"; Executor.Transport = "subagent"; Model = "claude-sonnet-5-5"; Effort.Provider = "high"; Effort.Semantic = "Deep"; ModelEscalationReason = null.

DELTA DE LA TAREA (primera delegacion de la cadena F4-T3C1-HOST-HARDENING)
- RunId = R20261113T164804Z-8f54 (tambien es el DelegationRunId); Attempt = 1; AttemptsRemaining = 2; MaxReworkLoops = 3.
- Unit = "I-64"; Initiative = "I-64"; Gate = "F4"; TaskId = "F4-T3C1-HOST-HARDENING".
- ExpectedBranch = la salida de `git branch --show-current`; BaseSha = la salida de `git rev-parse HEAD`; ChainBaseSha = ese mismo BaseSha; ChainRedSha = null; ChainRedFiles = []; CorrectionOf = null.
- MainSha = 812baf879612320653094a8aaf46aca0bac81383 (comprueba que coincide con `git rev-parse origin/main`; si no coincide, para con S-13 explicado en RoutingReason); AuthorityRevision = la del contrato.
- ExpectedWorktree = la ruta absoluta del directorio de trabajo actual.
- ExpectedHandoffPath = artifacts/orchestration/I-64/F4-T3C1-HOST-HARDENING/1/{WorkRunId}/worker-handoff.json (con el token literal).
- Owner = { Kind: "subagent", Id: "subagent:R20261113T164804Z-8f54" }; RoutingEnforcement = "required". Executor.Role = "Worker".
- AcceptanceCriteria: redactalos tu, en ASCII, verificables y trazables a los invariantes del contrato; incluye el RED con el esqueleto inerte completo de INV-F4T3C1-11 (solo las pruebas nuevas fallan, por asercion), las pruebas de host en Core, la guarda nueva y las guardas corregidas con sus autopruebas, las pruebas de VM de la UI, el oraculo de cero escrituras authored, el GREEN que no toca tests/, el build Release del Plugin sin errores ni avisos nuevos (sin cambiar el Plugin), la CI del SHA exacto y los conteos minimos de RequiredTests.
- IssuedBy = "Controller Codex (planificacion F4-T3C1-HOST-HARDENING, primera delegacion de la cadena, CONTROLLER_PLANNING)".
