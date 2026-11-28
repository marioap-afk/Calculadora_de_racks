I-64 - DELEGACION F4B-PBTC2-DECLARED-CORRECTIONS - Controller, perfil CONTROLLER_PLANNING (primera delegacion de la cadena F4B-PBTC2-DECLARED-CORRECTIONS)
Identidad: unidad I-64; gate F4B; tarea F4B-PBTC2-DECLARED-CORRECTIONS; intento 1; RunId R20261118T054551Z-6ac2;
           DelegationRunId R20261118T054551Z-6ac2; AuthorityRevision 8708d28fa19ba912e39da5ae55c14affb1e53651 (la del contrato);
           MainSha 812baf879612320653094a8aaf46aca0bac81383; BaseSha = el HEAD actual del worktree (se espera eedb069ce8ae79cda684f2620694949fd402051d: commit de emision del contrato, hijo directo de la AuthorityRevision 8708d28fa19ba912e39da5ae55c14affb1e53651:
           solo anade el contrato, su script generador, el estado y la evidencia);
           rama architecture/workspace-persistente-rackcad; worktree = el directorio de trabajo actual; Owner subagent:R20261118T054551Z-6ac2
Contrato de gate: artifacts/orchestration/I-64/F4B-PBTC2-DECLARED-CORRECTIONS/1/R20261118T054551Z-6ac2/gate-contract.json (ASCII puro; custodiado en docs/automation/evidence/I-64-pilot/F4B-PBTC2-DECLARED-CORRECTIONS/coordinator/gate-contract.json)
Autoridades (leer desde Git, no copiar su contenido): las del campo Authorities del contrato;
           vigencia: AuthorityRevision para la unidad (UNIT_DOC y UNIT_CHANGE), MainSha para lo demas (EXTERNAL)
Objetivo: emitir el paquete de delegacion (esquema rackcad-delegation/v1) del Worker para la tarea del contrato. Debe ser ejecutable tal cual.
Alcance: solo lectura. No escribas ningun archivo, no hagas commit ni push. No ejecutes dotnet, ni AutoCAD, ni builds.
Invariantes del Freeze: los 14 del contrato; una delegacion nunca amplia el contrato
Rutas: ver ALCANCE DE ESCRITURA abajo. Todo lo demas es de solo lectura.
           Fuente de diseno: docs/initiatives/I-64-f4b-pbtc2-gate-contract-package.md v2 (section 2, hechos medidos; section 3, decisiones D-PBTC2-01 a D-PBTC2-08 con el diseno por linea; section 4, la lista cerrada CT-PBTC2-01 a CT-PBTC2-04; section 5, el RED y el registro, sin sustituciones; section 6, el contrato; section 7, riesgos; section 8, las decisiones del Owner, decididas; section 10, los arrastres que no son de PB-TC2), con su registro de mediciones docs/initiatives/I-64-f4b-pbtc2-measurements.md, las decisiones literales del Owner (docs/automation/evidence/I-64-pilot/F4B-DRAFTS/coordinator/owner-pbt1-pre-decisions.md) y el paquete de PB-TC docs/initiatives/I-64-f4b-pbtc-gate-contract-package.md v2.2 como patron
Criterios de aceptacion: la salida valida contra el esquema; colecciones opacas del contrato copiadas byte a byte (ver COPIA EXACTA);
           AllowedWriteScope segun ALCANCE DE ESCRITURA; celda y effort entre las EligibleCells del contrato; TaskClass y campos de la DELTA exactamente como se indican
Evidencia requerida: ejecuta de verdad estas ordenes en el worktree y usa sus resultados:
           git branch --show-current ; git rev-parse HEAD ; git rev-parse origin/main ;
           git ls-files src/RackCad.UI/Systems/PushBack (10 archivos) ; git ls-files src/RackCad.Application/Systems/PushBack (64 archivos) ; git ls-files tests/RackCad.UI.Tests (201 archivos) ; git ls-files tests/RackCad.Tests (707 archivos) ;
           y lee el codigo, las pruebas y las guardas que cites
No-touch: todo el repositorio
Condiciones de parada: S-03, P-03 y las PBTC2-C* del contrato; ante cualquiera, para y explica el bloqueo en RoutingReason.
COPIA: en las tareas anteriores de esta unidad, varias planificaciones se rechazaron solo por la copia de las colecciones opacas: un elemento de una lista omitido, un espacio anadido al principio de una cadena, una palabra perdida dentro de un nombre de archivo ("WorkspacePaletteHostGuardTests.cs" en vez de "WorkspacePaletteHostSourceGuardTests.cs") o un verbo cambiado de modo ("es" copiado como "sea"). Ninguna entrada de ninguna lista empieza ni termina con un espacio.
Una sola diferencia de un caracter o un solo elemento de mas o de menos basta para rechazar. Por la parada PBTC2-C08 del contrato, un rechazo solo por la copia admite una sola replanificacion y un segundo va con STOP al Owner.
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

CONTEXTO (hechos versionados en el HEAD; evidencia section 231 y siguientes de docs/automation/evidence/I-64-evidence.md; F4B-E01 a F4B-E10 en docs/automation/evidence/I-64-pilot/F4B-DRAFTS/f4b-dispositions.md)
- F4 esta cerrada. En F4B, PB-T0 y PB-TC estan CLOSED / PASS WITH LIMITATIONS (F4B-E05 y F4B-E08). Antes del contrato de PB-T1 el Owner decidio (F4B-E09) corregir en una tarea propia, PB-TC2, lo que quedo fuera de PB-TC: H-31 (GapBox, ModuleLengthBox y las cuatro cajas avanzadas anulables, con la regla Keep) y OWNER-1 (en Ambos lados, con el lado B presente, una caja mixta que recibe un valor lo aplica a los dos lados); y autorizo emitirla sin volver a el (F4B-E10). c-02 y pb-21-cola van a PB-T1, no aqui.
  Presupuesto: Attempt 1, AttemptsRemaining 2, MaxReworkLoops 3, CorrectionsAuthorized true; 1 invocacion de Worker por delegacion aceptada; un intento 3 vuelve siempre al Owner (PBTC2-C12).
- HECHOS DEL REPOSITORIO medidos por la sesion en el HEAD (comprueba los que uses):
  * El filtro con ExpectRed true (FullyQualifiedName~RackCad.UI.Tests.PushBackDeclaredCorrection2) selecciona 0 pruebas en el HEAD. Las dos clases nuevas van en namespace RackCad.UI.Tests. Las pruebas de PB-TC entran en RequiredTests clase por clase porque su prefijo seleccionaria tambien las clases nuevas.
  * El RED es ORDINARIO: solo los dos archivos nuevos de prueba, ningun archivo de src/ y ninguna sustitucion. El GREEN toca solo W y WC (la ventana clasica: .xaml.cs y .Composite.cs) y ningun archivo de tests/; WX no se toca.
  * Antes del RED (D-PBTC2-07): una corrida TEMPORAL sobre la BaseSha con la formula de CT-PB0-21 para las seis filas T2-1 a T2-6; si alguna es material, PBTC2-C14: STOP y vuelta al Owner antes del GREEN.
  * La CI corre las suites completas en Debug y compila el Plugin en Release. En el RED se espera ui-tests en failure y tests, build-ui y build-plugin en success; ninguna prueba existente falla en el RED.
  * Lo que esta FIJADO en el paquete y se traslada a los AcceptanceCriteria como receta, sin reconstruirlo: la lista cerrada CT-PBTC2-01 a 04 (un CT = un metodo [Fact]) con sus lineas esperadas; el diseno por linea de D-PBTC2-01 a 05; las ventanas y el contador de descartes de D-PBTC2-08; el Worker no ajusta lo esperado (PBTC2-C07).
  * Sin AutoCAD.

ALCANCE DE ESCRITURA
- AllowedWriteScope de la delegacion = EXACTAMENTE las 4 entradas del contrato, en su orden (4 archivos exactos, sin prefijos):
    src/RackCad.UI/Systems/PushBack/RackPushBackSystemWindow.xaml.cs
    src/RackCad.UI/Systems/PushBack/RackPushBackSystemWindow.Composite.cs
    tests/RackCad.UI.Tests/PushBackDeclaredCorrection2RoundingTests.cs
    tests/RackCad.UI.Tests/PushBackDeclaredCorrection2MixedBoxTests.cs
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
- Todas las cadenas del contrato canonico artifacts/orchestration/I-64/F4B-PBTC2-DECLARED-CORRECTIONS/1/R20261118T054551Z-6ac2/gate-contract.json son ASCII puro. Se copian byte a byte.
- Authorities, ForbiddenWriteScope, Invariants, RequiredTests, ExpectedEvidence, StopConditions y Objective son DATOS OPACOS de ese contrato. CARGALOS desde el archivo con una orden y reproducelos en tu salida a partir de esa carga, por ejemplo:
  python -c "import json; c=json.load(open(r'artifacts/orchestration/I-64/F4B-PBTC2-DECLARED-CORRECTIONS/1/R20261118T054551Z-6ac2/gate-contract.json', encoding='ascii')); print(json.dumps({k: c[k] for k in ['Authorities','ForbiddenWriteScope','Invariants','RequiredTests','ExpectedEvidence','StopConditions','Objective','AllowedWriteScope']}, ensure_ascii=True, separators=(chr(44), chr(58))))"
  Mismo orden, mismos elementos, mismos textos byte a byte.
- MODO VERBAL Y GRAMATICA: el contrato usa a proposito construcciones que pueden parecer mejorables (indicativo tras si, con o comprueba; nombres en vez de verbos); se copian IGUAL. Antes de terminar, imprime cada invariante del contrato NUMERADO (por ejemplo: python -c "import json; c=json.load(open(r'artifacts/orchestration/I-64/F4B-PBTC2-DECLARED-CORRECTIONS/1/R20261118T054551Z-6ac2/gate-contract.json', encoding='ascii')); [print(i, x) for i, x in enumerate(c['Invariants'])]") y compara tu salida con esa impresion, invariante por invariante y verbo por verbo.
- AUTHORITIES: varias autoridades citan paquetes con nombres casi iguales; se copian cada una en su indice y con su nombre exacto: Authorities[0].Path = "docs/initiatives/I-64-f4b-pbtc2-gate-contract-package.md"; Authorities[1].Path = "docs/initiatives/I-64-f4b-pbtc2-measurements.md"; Authorities[4].Path = "docs/initiatives/I-64-f4b-push-back-completo-package.md"; Authorities[9].Path = "docs/initiatives/I-64-f4b-pbtc-gate-contract-package.md"; Authorities[16].Path = "docs/initiatives/I-64-f4-t1b-gate-contract-package.md". No los mezcles ni los acortes.
- SIN CONFLICTOS INEXISTENTES: el contrato canonico de esta corrida es ASCII PURO (0 bytes no ASCII; compruebalo con python -c "b=open(r'artifacts/orchestration/I-64/F4B-PBTC2-DECLARED-CORRECTIONS/1/R20261118T054551Z-6ac2/gate-contract.json', 'rb').read(); print(sum(1 for x in b if x >= 0x80))"); TaskClass se escribe exactamente como lo fija este prompt, sin tilde (la tabla de routing.md lleva tilde y NO se copia de alli). Antes de alegar cualquier conflicto o parada S-03, carga el contrato y cita en RoutingReason la orden y su salida que lo demuestran; sin esa prueba no hay conflicto y emites la delegacion completa.
- PROHIBIDO, en TODAS las cadenas copiadas: mejorar la gramatica; normalizar singular o plural o la concordancia; cambiar la puntuacion; anadir o quitar espacios;
  anadir o quitar tildes; cambiar mayusculas; reflujar, partir o unir el texto; parafrasear; corregir lo que parezca una errata. Si una frase te parece incorrecta, se copia IGUAL.
- AllowedWriteScope es la barrera PRIMARIA y exacta de escritura: copialo elemento a elemento, en el mismo orden, sin anadir, quitar, ensanchar ni normalizar ninguna entrada.
  ForbiddenWriteScope tiene exactamente las 129 entradas del contrato: ni una mas ni una menos, y ninguna con espacios anadidos.
- FORBIDDENWRITESCOPE, COPIA REFORZADA DESDE EL PRIMER INTENTO: tiene exactamente 129 entradas, de la 0 a la 128, todas distintas. Para copiarla, ejecuta esta orden, que la imprime SOLA como un array JSON compacto en una linea, sin ningun espacio entre los elementos, y reproduce cada cadena tal cual, de una vez, sin escribir las entradas de memoria:
  python -c "import json; c=json.load(open(r'artifacts/orchestration/I-64/F4B-PBTC2-DECLARED-CORRECTIONS/1/R20261118T054551Z-6ac2/gate-contract.json', encoding='ascii')); print(json.dumps(c['ForbiddenWriteScope'], ensure_ascii=True, separators=(chr(44), chr(58))))"
  Puntos de control, comprobados contra la impresion numerada: la 0 es src/RackCad.Application/; de la 0 a la 2, prefijos y archivos de src/ fuera de src/RackCad.UI/; de la 3 a la 58, src/RackCad.UI/ (la 3 es src/RackCad.UI/CatalogOption.cs y, dentro, los 8 archivos o prefijos de src/RackCad.UI/Systems/PushBack/ que no se tocan, de la 46 a la 53); de la 59 a la 111, prefijos y archivos de tests/ (la 59 es tests/RackCad.Tests/); de la 112 a la 128, la raiz (la 112 es docs/ y la 128, la ULTIMA de la lista, .gitignore).
  Ahi termina la lista: nunca vuelvas atras ni repitas un nombre.
  Antes de terminar, cuenta los elementos de tu ForbiddenWriteScope (deben ser 129), comprueba que no hay repetidos, compara cada indice con la impresion numerada y comprueba que ninguna entrada empieza ni termina con un espacio.
- Para ForbiddenWriteScope y AllowedWriteScope, ejecuta ademas una orden que imprima cada entrada del contrato NUMERADA (por ejemplo: python -c "import json; c=json.load(open(r'artifacts/orchestration/I-64/F4B-PBTC2-DECLARED-CORRECTIONS/1/R20261118T054551Z-6ac2/gate-contract.json', encoding='ascii')); [print(i, repr(x)) for i, x in enumerate(c['ForbiddenWriteScope'])]") y comprueba tu lista contra esa salida, indice por indice, de 0 a 128, mirando cada palabra de cada nombre de archivo (una planificacion anterior perdio la palabra Source en un nombre).
- STOPCONDITIONS: la PBTC2-C11 es larga a proposito (las firmas literales de PD-F4-01 y PD-F4-04 y la regla de coincidencia); copiala entera desde la carga, sin resumirla ni partirla. P-01 nombra la Snapshot I.
- Antes de dar la salida por terminada, compara tu salida con el contrato cargado campo por campo, cadena por cadena y contando los elementos de cada lista; la sesion ejecuta despues una comparacion determinista byte a byte
  de Invariants, RequiredTests, StopConditions, Authorities, ExpectedEvidence, Objective, AllowedWriteScope y ForbiddenWriteScope ANTES de aceptar: cualquier diferencia = salida RECHAZADA y sin Worker.
- TODA tu salida debe ser ASCII puro, tambien AcceptanceCriteria y RoutingReason: sin tildes, sin enye y sin signos no ASCII; ninguna secuencia de escape formada por barra invertida + u; ningun tabulador ni otro caracter de control dentro de las cadenas.
- RequiredTests con los mismos 18 elementos, en el mismo orden y con los mismos valores (MinSelected 4, 3, 2, 2, 2, 1, 16, 55, 67, 13, 9, 52, 7, 3, 10, 10, 3, 15; ExpectRed true solo en el primero).

TASKCLASS
- TaskClass = exactamente "Implementacion transversal a capas (corta)"; PromptProfile = "ROUTINE_IMPLEMENTATION"; explica la eleccion en RoutingReason con las siete dimensiones (routing.md section 2) y la fecha de verificacion de la celda: correccion de producto en la ventana WPF con pruebas de UI nuevas y una medicion temporal previa. Si tu evaluacion da Horizonte High: para con S-03 explicado en RoutingReason.
- Celda del Worker: Executor.Cell = "anthropic/subagent/claude-sonnet-5-5"; Executor.Provider = "Anthropic"; Executor.Transport = "subagent"; Model = "claude-sonnet-5-5"; Effort.Provider = "high"; Effort.Semantic = "Deep"; ModelEscalationReason = null.

DELTA DE LA TAREA (primera delegacion de la cadena F4B-PBTC2-DECLARED-CORRECTIONS)
- RunId = R20261118T054551Z-6ac2 (tambien es el DelegationRunId); Attempt = 1; AttemptsRemaining = 2; MaxReworkLoops = 3.
- Unit = "I-64"; Initiative = "I-64"; Gate = "F4B"; TaskId = "F4B-PBTC2-DECLARED-CORRECTIONS".
- ExpectedBranch = la salida de `git branch --show-current`; BaseSha = la salida de `git rev-parse HEAD`; ChainBaseSha = ese mismo BaseSha; ChainRedSha = null; ChainRedFiles = []; CorrectionOf = null.
- MainSha = 812baf879612320653094a8aaf46aca0bac81383 (comprueba que coincide con `git rev-parse origin/main`; si no coincide, para con S-13 explicado en RoutingReason); AuthorityRevision = la del contrato.
- ExpectedWorktree = la ruta absoluta del directorio de trabajo actual.
- ExpectedHandoffPath = artifacts/orchestration/I-64/F4B-PBTC2-DECLARED-CORRECTIONS/1/{WorkRunId}/worker-handoff.json (con el token literal).
- Owner = { Kind: "subagent", Id: "subagent:R20261118T054551Z-6ac2" }; RoutingEnforcement = "required". Executor.Role = "Worker".
- AcceptanceCriteria: redactalos tu, en ASCII, verificables y trazables a los invariantes del contrato; incluye: la medicion temporal de la base de D-PBTC2-07 con PBTC2-C14; el RED solo con los dos archivos nuevos de prueba, con el criterio mecanico del invariante del RED en el filtro con ExpectRed; cada CT de CT-PBTC2-01 a 04 con su metodo y sus lineas esperadas sin ajustarlas (PBTC2-C07); el GREEN solo en W y WC con el diseno por linea de D-PBTC2-01 a 05; las ventanas y el contador de descartes de D-PBTC2-08; nada fuera de H-31 y OWNER-1; la contencion de las corridas locales y la prueba de limpieza; ninguna prueba que cuelgue, deje un modal vivo, escriba en el perfil, necesite AutoCAD o dependa de rutas de la maquina; la CI exacta del RED y del GREEN.
- IssuedBy = "Controller Codex (planificacion F4B-PBTC2-DECLARED-CORRECTIONS, primera delegacion de la cadena, CONTROLLER_PLANNING)".
