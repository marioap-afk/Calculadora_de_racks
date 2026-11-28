I-64 - DELEGACION F4-T4-HOST-C2 - Controller, perfil CONTROLLER_PLANNING (primera delegacion de la cadena F4-T4-HOST-C2)
Identidad: unidad I-64; gate F4; tarea F4-T4-HOST-C2; intento 1; RunId R20261116T124830Z-cfb1;
           DelegationRunId R20261116T124830Z-cfb1; AuthorityRevision 6d02b485efaed0787849a0651e6704cbee94dd65 (la del contrato);
           MainSha 812baf879612320653094a8aaf46aca0bac81383; BaseSha = el HEAD actual del worktree (se espera dcf3396f7c0850c6389e9826a1ca3e544f130cc9: commit de emision del contrato, hijo directo de la AuthorityRevision 6d02b485efaed0787849a0651e6704cbee94dd65:
           solo anade el contrato, el estado y la evidencia);
           rama architecture/workspace-persistente-rackcad; worktree = el directorio de trabajo actual; Owner subagent:R20261116T124830Z-cfb1
Contrato de gate: artifacts/orchestration/I-64/F4-T4-HOST-C2/1/R20261116T124830Z-cfb1/gate-contract.json (ASCII puro; custodiado en docs/automation/evidence/I-64-pilot/F4-T4-HOST-C2/coordinator/gate-contract.json)
Autoridades (leer desde Git, no copiar su contenido): las del campo Authorities del contrato;
           vigencia: AuthorityRevision para la unidad (UNIT_DOC y UNIT_CHANGE), MainSha para lo demas (EXTERNAL)
Objetivo: emitir el paquete de delegacion (esquema rackcad-delegation/v1) del Worker para la tarea del contrato. Debe ser ejecutable tal cual.
Alcance: solo lectura. No escribas ningun archivo, no hagas commit ni push. No ejecutes dotnet, ni AutoCAD, ni builds.
Invariantes del Freeze: los 6 del contrato; una delegacion nunca amplia el contrato
Rutas: ver ALCANCE DE ESCRITURA abajo. Todo lo demas es de solo lectura.
           Fuente de diseno: docs/initiatives/I-64-f4-t4-host-c2-gate-contract-package.md v2 (section 2 hechos H-01 a H-10, section 3 alcance, section 4 decisiones D-C2-01 a D-C2-07, section 5 invariantes, section 6 paradas, section 7 despues, section 8 riesgos, section 10 disposicion de la revision); la decision del Owner docs/automation/evidence/I-64-pilot/F4-DRAFTS/coordinator/owner-f4-s2-03-v2-decision.md; el diagnostico docs/automation/evidence/I-64-pilot/F4-DRAFTS/c2/diagnosis-report-c2.md; la linea medida docs/automation/evidence/I-64-pilot/F4-DRAFTS/c2/diag3.patch; el mapa docs/automation/evidence/I-64-pilot/F4-DRAFTS/c2/map.md; y la revision docs/automation/evidence/I-64-pilot/F4-DRAFTS/c2/independent-review-v1.md
Criterios de aceptacion: la salida valida contra el esquema; colecciones opacas del contrato copiadas byte a byte (ver COPIA EXACTA);
           AllowedWriteScope segun ALCANCE DE ESCRITURA; celda y effort entre las EligibleCells del contrato; TaskClass y campos de la DELTA exactamente como se indican
Evidencia requerida: ejecuta de verdad estas ordenes en el worktree y usa sus resultados:
           git branch --show-current ; git rev-parse HEAD ; git rev-parse origin/main ;
           git ls-files src/RackCad.Plugin/Drawing (14 archivos) ; git ls-files src/RackCad.Plugin/Systems/Shared (11 archivos) ; git ls-files tests/RackCad.Tests (703 archivos) ;
           y lee el codigo, las pruebas y las guardas que cites
No-touch: todo el repositorio
Condiciones de parada: S-03, P-03 y las C2-S01 a C2-S05 del contrato; ante cualquiera, para y explica el bloqueo en RoutingReason.
COPIA: en las tareas anteriores de esta unidad, varias planificaciones se rechazaron solo por la copia de las colecciones opacas: un elemento de una lista omitido, un espacio anadido al principio de una cadena, una palabra perdida dentro de un nombre de archivo o un verbo cambiado de modo. Ninguna entrada de ninguna lista empieza ni termina con un espacio.
Una sola diferencia de un caracter o un solo elemento de mas o de menos basta para rechazar. Por la parada C2-S05 del contrato, un rechazo admite una sola replanificacion y un segundo va con STOP al Owner (D4).
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

CONTEXTO (hechos versionados en el HEAD; evidencia section 221 y siguientes de docs/automation/evidence/I-64-evidence.md, en particular 224 y la de esta emision; F4-E74 en adelante en docs/automation/evidence/I-64-pilot/F4-DRAFTS/f4-dispositions.md)
- Smoke-2 encontro F4-S2-03: tras un Actualizar del panel, U deshace la base de datos pero la pantalla sigue mostrando el redibujo hasta un REGEN. El diagnostico en AutoCAD (F4-E76) mostro que es PREEXISTENTE (RACKEDITAR clasico hace lo mismo) y que la causa es que el redibujo abre las referencias directas de la definicion para escritura sin modificarlas, lo que no deja registro de Undo. Con UNA sentencia medida en AutoCAD con un build desechable (reference.Position = reference.Position; en el bucle de LateralHeaderDrawer.RedefineSystemBlock), U y REDO refrescan la pantalla. El Owner decidio corregirlo (owner-f4-s2-03-v2-decision.md). F4-T4-HOST-C2 anade esa sentencia y su comentario fijados en D-C2-01 y una guarda de texto nueva; nada mas. Cantilever (CantileverViewMaterializer) NO cambia.
  Presupuesto: Attempt 1, AttemptsRemaining 2, MaxReworkLoops 3, CorrectionsAuthorized true; 1 invocacion de Worker por delegacion aceptada.
- HECHOS DEL REPOSITORIO medidos por la sesion en el HEAD (comprueba los que uses):
  * src/RackCad.Plugin/Drawing/LateralHeaderDrawer.cs existe; tests/RackCad.Tests/RedefinedViewReferenceUndoTouchGuardTests.cs no existe todavia. El producto y las pruebas del HEAD son los de b86c3cfe, el GREEN que cerro F4-T4-HOST-C1 (git diff b86c3cfe HEAD -- src tests vacio). El bucle esta en LateralHeaderDrawer.cs:167-171.
  * Cuentas de la base (CI push 43801607010 de b86c3cfe): el filtro FullyQualifiedName~RackCad.Tests.Workspace selecciona 3468; InsertRedrawLayerAccessCharacterizationTests 5, RackDefinitionCreatorGuardTests 26, SharedViewFoundationF1CharacterizationTests 26, StructuralSectionPluginSourceGuardTests 45; cada una de las 23 clases de regresion de C1, la cuenta de su fila de RequiredTests; UI como en C1. Solo el filtro de la clase nueva lleva ExpectRed true (minimo 4, el numero de casos fijado por el paquete); la UI no cambia.
  * El RED (INV-C2-05 y D-C2-07 del paquete): solo la prueba nueva RedefinedViewReferenceUndoTouchGuardTests, con el producto sin cambios; falla por asercion porque la sentencia no existe. La CI del RED: el job de Core en failure; el de UI en success. El GREEN anade las dos lineas de D-C2-01 y no toca la prueba del RED.
  * Lo que esta FIJADO en el paquete y se traslada a los AcceptanceCriteria como receta, sin reconstruirlo: las dos lineas exactas de D-C2-01 (el comentario en ingles y la sentencia), su sitio entre la apertura ForWrite y RecordGraphicsModified(true), con la indentacion del bucle; los 4 casos exactos de la guarda de D-C2-07, sobre codigo sin comentarios, sin fijar RecordGraphicsModified ni el orden respecto de el, con autopruebas que comprueban que su mutacion cambio el texto.
  * Reglas de las corridas locales (F4-E59, INV-C2-06): nunca la suite completa de UI en local, solo sus dos filtros de RequiredTests; ninguna prueba nueva escribe fuera de una ubicacion temporal aislada.
  * Trampas medidas (mapa y revision): InsertRedrawLayerAccessCharacterizationTests.cs:27 exige el texto tr.GetObject(referenceId, OpenMode.ForWrite) en el archivo (se conserva); las barridas de texto de todo el Plugin cuentan tambien los comentarios (RackUnitsGuardSourceTests.cs:88-101 con cualquier Insunits, SelectiveEditorOpenTests.cs:565-572 con el censo de [CommandMethod(, CustomPropertiesCommandGuardTests.cs:178-189, PushBackRoundTripSourceGuardTests.cs:66-84, I55G15Auth15ConsumerTests.cs:44-55): el comentario fijado no lleva ninguno de sus tokens; RackDefinitionCreatorGuardTests.cs:216-254 no incluye RedefineSystemBlock; la barrida G9 (WorkspacePilotUpdateIntegrationGuardTests.cs:689-699) quita los comentarios y el codigo no nombra el piloto; I55ProductCharacterizationTestSupport.Code devuelve el codigo sin comentarios. Ninguna se toca; la entrega dice como las respeta.

ALCANCE DE ESCRITURA
- AllowedWriteScope de la delegacion = EXACTAMENTE las 2 entradas del contrato, en su orden (2 archivos exactos, sin prefijos):
    src/RackCad.Plugin/Drawing/LateralHeaderDrawer.cs
    tests/RackCad.Tests/RedefinedViewReferenceUndoTouchGuardTests.cs
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
- Todas las cadenas del contrato canonico artifacts/orchestration/I-64/F4-T4-HOST-C2/1/R20261116T124830Z-cfb1/gate-contract.json son ASCII puro. Se copian byte a byte.
- Authorities, ForbiddenWriteScope, Invariants, RequiredTests, ExpectedEvidence, StopConditions y Objective son DATOS OPACOS de ese contrato. CARGALOS desde el archivo con una orden y reproducelos en tu salida a partir de esa carga, por ejemplo:
  python -c "import json; c=json.load(open(r'artifacts/orchestration/I-64/F4-T4-HOST-C2/1/R20261116T124830Z-cfb1/gate-contract.json', encoding='ascii')); print(json.dumps({k: c[k] for k in ['Authorities','ForbiddenWriteScope','Invariants','RequiredTests','ExpectedEvidence','StopConditions','Objective','AllowedWriteScope']}, ensure_ascii=True, separators=(chr(44), chr(58))))"
  Mismo orden, mismos elementos, mismos textos byte a byte.
- MODO VERBAL Y GRAMATICA: el contrato usa a proposito construcciones que pueden parecer mejorables (indicativo tras si, con o comprueba; nombres en vez de verbos); se copian IGUAL. Antes de terminar, imprime cada invariante del contrato NUMERADO (por ejemplo: python -c "import json; c=json.load(open(r'artifacts/orchestration/I-64/F4-T4-HOST-C2/1/R20261116T124830Z-cfb1/gate-contract.json', encoding='ascii')); [print(i, x) for i, x in enumerate(c['Invariants'])]") y compara tu salida con esa impresion, invariante por invariante y verbo por verbo.
- AUTHORITIES: se copian cada una en su indice y con su nombre exacto: Authorities[0].Path = "docs/initiatives/I-64-f4-t4-host-c2-gate-contract-package.md" (el paquete de esta tarea, c2); Authorities[1].Path = "docs/automation/evidence/I-64-pilot/F4-DRAFTS/coordinator/owner-f4-s2-03-v2-decision.md"; Authorities[5].Path = "docs/automation/evidence/I-64-pilot/F4-DRAFTS/c2/diagnosis-report-c2.md"; Authorities[9].Path = "docs/initiatives/I-64-proposal-v5.md"; Authorities[10].Path = "docs/automation/evidence/I-64-pilot/F4-T4-HOST-C1/coordinator/gate-contract.json" (el contrato de C1, no el de esta tarea); Authorities[14].Path = el contrato de esta tarea (F4-T4-HOST-C2). No los mezcles ni los acortes.
- SIN CONFLICTOS INEXISTENTES: el contrato canonico de esta corrida es ASCII PURO (0 bytes no ASCII; compruebalo con python -c "b=open(r'artifacts/orchestration/I-64/F4-T4-HOST-C2/1/R20261116T124830Z-cfb1/gate-contract.json', 'rb').read(); print(sum(1 for x in b if x >= 0x80))"); TaskClass se escribe exactamente como lo fija este prompt, sin tilde (la tabla de routing.md lleva tilde y NO se copia de alli). Antes de alegar cualquier conflicto o parada S-03, carga el contrato y cita en RoutingReason la orden y su salida que lo demuestran; sin esa prueba no hay conflicto y emites la delegacion completa.
- PROHIBIDO, en TODAS las cadenas copiadas: mejorar la gramatica; normalizar singular o plural o la concordancia; cambiar la puntuacion; anadir o quitar espacios;
  anadir o quitar tildes; cambiar mayusculas; reflujar, partir o unir el texto; parafrasear; corregir lo que parezca una errata. Si una frase te parece incorrecta, se copia IGUAL.
- AllowedWriteScope es la barrera PRIMARIA y exacta de escritura: copialo elemento a elemento, en el mismo orden, sin anadir, quitar, ensanchar ni normalizar ninguna entrada.
  ForbiddenWriteScope tiene exactamente las 79 entradas del contrato: ni una mas ni una menos, y ninguna con espacios anadidos.
- FORBIDDENWRITESCOPE, COPIA REFORZADA: tiene exactamente 79 entradas, de la 0 a la 78, todas distintas. Para copiarla, ejecuta esta orden, que la imprime SOLA como un array JSON compacto en una linea, sin ningun espacio entre los elementos, y reproduce cada cadena tal cual, de una vez, sin escribir las entradas de memoria:
  python -c "import json; c=json.load(open(r'artifacts/orchestration/I-64/F4-T4-HOST-C2/1/R20261116T124830Z-cfb1/gate-contract.json', encoding='ascii')); print(json.dumps(c['ForbiddenWriteScope'], ensure_ascii=True, separators=(chr(44), chr(58))))"
  Puntos de control, comprobados contra la impresion numerada: de la 0 a la 11, las raices (la 0 es docs/ y la 11 README.md); la 12 es src/RackCad.UI/, la 13 tests/RackCad.UI.Tests/, la 14 src/RackCad.Domain/ y la 15 src/RackCad.Application/; de la 16 a la 19, cuatro subraices de src/RackCad.Plugin/ (la 16 KindHandlers/ y la 19 Workspace/); de la 20 a la 55, los 36 archivos de la raiz de src/RackCad.Plugin/ (la 20 es CustomPropertiesData.cs, la 31 RackCad.Plugin.csproj y la 55 StructuralSectionCommandFlow.cs); la 56 es src/RackCad.Plugin/Drawing/Cantilever/ y la 57 src/RackCad.Plugin/Drawing/StructuralSections/; de la 58 a la 68, los 11 archivos de src/RackCad.Plugin/Drawing/ que no se tocan (la 58 es AutoCadExternalLibraryBlockQuery.cs y la 68 RackSourcePlacementCaptureAdapter.cs; LateralHeaderDrawer.cs NO esta); la 69 es tests/RackCad.Tests/Workspace/; de la 70 a la 78, nueve archivos de tests/RackCad.Tests/ (la 70 es InsertRedrawLayerAccessCharacterizationTests.cs y la 78, la ULTIMA de la lista, StructuralSectionPluginSourceGuardTests.cs).
  Ahi termina la lista: nunca vuelvas atras ni repitas un nombre.
  Antes de terminar, cuenta los elementos de tu ForbiddenWriteScope (deben ser 79), comprueba que no hay repetidos, compara cada indice con la impresion numerada y comprueba que ninguna entrada empieza ni termina con un espacio.
- Para ForbiddenWriteScope y AllowedWriteScope, ejecuta ademas una orden que imprima cada entrada del contrato NUMERADA (por ejemplo: python -c "import json; c=json.load(open(r'artifacts/orchestration/I-64/F4-T4-HOST-C2/1/R20261116T124830Z-cfb1/gate-contract.json', encoding='ascii')); [print(i, repr(x)) for i, x in enumerate(c['ForbiddenWriteScope'])]") y comprueba tu lista contra esa salida, indice por indice, de 0 a 78, mirando cada palabra de cada nombre de archivo (una planificacion anterior perdio la palabra Source en un nombre).
- Antes de dar la salida por terminada, compara tu salida con el contrato cargado campo por campo, cadena por cadena y contando los elementos de cada lista; la sesion ejecuta despues una comparacion determinista byte a byte
  de Invariants, RequiredTests, StopConditions, Authorities, ExpectedEvidence, Objective, AllowedWriteScope y ForbiddenWriteScope ANTES de aceptar: cualquier diferencia = salida RECHAZADA y sin Worker.
- TODA tu salida debe ser ASCII puro, tambien AcceptanceCriteria y RoutingReason: sin tildes, sin enye y sin signos no ASCII; ninguna secuencia de escape formada por barra invertida + u; ningun tabulador ni otro caracter de control dentro de las cadenas.
- RequiredTests con los mismos 31 elementos, en el mismo orden y con los mismos valores (ExpectRed true SOLO en el primero, FullyQualifiedName~RackCad.Tests.RedefinedViewReferenceUndoTouchGuardTests; false en los otros 30, incluidos FullyQualifiedName~RackCad.Tests.Workspace y FullyQualifiedName~RackCad.UI.Tests.Workspace).

TASKCLASS
- TaskClass = exactamente "Implementacion transversal a capas (corta)"; PromptProfile = "ROUTINE_IMPLEMENTATION"; explica la eleccion en RoutingReason con las siete dimensiones (routing.md section 2) y la fecha de verificacion de la celda: una primitiva del Plugin compartida por los escritores de todos los sistemas (LateralHeaderDrawer.RedefineSystemBlock, a traves de SystemBlockWriter) y una guarda de texto en Core; es la misma clase y la misma celda de todas las tareas de F4. Si tu evaluacion da Horizonte High: para con S-03 explicado en RoutingReason.
- Celda del Worker: Executor.Cell = "anthropic/subagent/claude-sonnet-5-5"; Executor.Provider = "Anthropic"; Executor.Transport = "subagent"; Model = "claude-sonnet-5-5"; Effort.Provider = "high"; Effort.Semantic = "Deep"; ModelEscalationReason = null.

DELTA DE LA TAREA (primera delegacion de la cadena F4-T4-HOST-C2)
- RunId = R20261116T124830Z-cfb1 (tambien es el DelegationRunId); Attempt = 1; AttemptsRemaining = 2; MaxReworkLoops = 3.
- Unit = "I-64"; Initiative = "I-64"; Gate = "F4"; TaskId = "F4-T4-HOST-C2".
- ExpectedBranch = la salida de `git branch --show-current`; BaseSha = la salida de `git rev-parse HEAD`; ChainBaseSha = ese mismo BaseSha; ChainRedSha = null; ChainRedFiles = []; CorrectionOf = null.
- MainSha = 812baf879612320653094a8aaf46aca0bac81383 (comprueba que coincide con `git rev-parse origin/main`; si no coincide, para con S-13 explicado en RoutingReason); AuthorityRevision = la del contrato.
- ExpectedWorktree = la ruta absoluta del directorio de trabajo actual.
- ExpectedHandoffPath = artifacts/orchestration/I-64/F4-T4-HOST-C2/1/{WorkRunId}/worker-handoff.json (con el token literal).
- Owner = { Kind: "subagent", Id: "subagent:R20261116T124830Z-cfb1" }; RoutingEnforcement = "required". Executor.Role = "Worker".
- AcceptanceCriteria: redactalos tu, en ASCII, verificables y trazables a los invariantes del contrato; incluye: el RED con solo la prueba nueva, que falla por asercion con el producto sin cambios, y las existentes en verde; el GREEN dentro del AllowedWriteScope sin tocar tests/ del RED; las dos lineas exactas de INV-C2-01 en su sitio y nada mas en src/ (Cantilever sin cambios); la guarda nueva con sus 4 casos exactos de INV-C2-05; ninguna regeneracion, evento ni tratamiento de Undo (INV-C2-02); las guardas, caracterizaciones y barridas de INV-C2-03 en verde y sin cambios; la contencion de las corridas locales, sin la suite completa de UI; la CI exacta del RED y del GREEN.
- IssuedBy = "Controller Codex (planificacion F4-T4-HOST-C2, primera delegacion de la cadena, CONTROLLER_PLANNING)".
