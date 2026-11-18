I-64 - DELEGACION F4B-PBTC-DECLARED-CORRECTIONS - Controller, perfil CONTROLLER_PLANNING (primera delegacion de la cadena F4B-PBTC-DECLARED-CORRECTIONS; REPLANIFICACION UNICA tras un rechazo solo por la copia)
Identidad: unidad I-64; gate F4B; tarea F4B-PBTC-DECLARED-CORRECTIONS; intento 1; RunId R20261118T020622Z-7171;
           DelegationRunId R20261118T020622Z-7171; AuthorityRevision a9e7195a7181eacba5941e7b35da86909ce72f8d (la del contrato);
           MainSha 812baf879612320653094a8aaf46aca0bac81383; BaseSha = el HEAD actual del worktree (se espera 48fe199e3a024d98be28dc697c1e9ce0324e95a5: commit de emision del contrato, hijo directo de la AuthorityRevision a9e7195a7181eacba5941e7b35da86909ce72f8d:
           solo anade el contrato, su script generador, el estado y la evidencia);
           rama architecture/workspace-persistente-rackcad; worktree = el directorio de trabajo actual; Owner subagent:R20261118T020622Z-7171
Contrato de gate: artifacts/orchestration/I-64/F4B-PBTC-DECLARED-CORRECTIONS/1/R20261118T020622Z-7171/gate-contract.json (ASCII puro; custodiado en docs/automation/evidence/I-64-pilot/F4B-PBTC-DECLARED-CORRECTIONS/coordinator/gate-contract.json)
Autoridades (leer desde Git, no copiar su contenido): las del campo Authorities del contrato;
           vigencia: AuthorityRevision para la unidad (UNIT_DOC y UNIT_CHANGE), MainSha para lo demas (EXTERNAL)
Objetivo: emitir el paquete de delegacion (esquema rackcad-delegation/v1) del Worker para la tarea del contrato. Debe ser ejecutable tal cual.
Alcance: solo lectura. No escribas ningun archivo, no hagas commit ni push. No ejecutes dotnet, ni AutoCAD, ni builds.
Invariantes del Freeze: los 19 del contrato; una delegacion nunca amplia el contrato
Rutas: ver ALCANCE DE ESCRITURA abajo. Todo lo demas es de solo lectura.
           Fuente de diseno: docs/initiatives/I-64-f4b-pbtc-gate-contract-package.md v2.2 (section 2, hechos medidos; section 3, decisiones D-PBTC-01 a D-PBTC-11; section 4, la lista cerrada CT-PBTC-01 a 08, 10 y 14 con sus definiciones comunes de 4.1 y el mapa CT a metodo de 4.4; section 5, el RED, las sustituciones SUS-01 a SUS-04 y el registro; section 6, el contrato; section 7, riesgos y lecciones; section 8, las decisiones del Owner, decididas; section 10, el caso H-31 que NO entra), con su registro de mediciones docs/initiatives/I-64-f4b-pbtc-measurements.md, las decisiones literales del Owner (docs/automation/evidence/I-64-pilot/F4B-DRAFTS/coordinator/owner-pbtc-decisions.md) y el paquete de la fase docs/initiatives/I-64-f4b-push-back-completo-package.md v3.1 (OB-06, OB-08 y OB-T1B)
Criterios de aceptacion: la salida valida contra el esquema; colecciones opacas del contrato copiadas byte a byte (ver COPIA EXACTA);
           AllowedWriteScope segun ALCANCE DE ESCRITURA; celda y effort entre las EligibleCells del contrato; TaskClass y campos de la DELTA exactamente como se indican
Evidencia requerida: ejecuta de verdad estas ordenes en el worktree y usa sus resultados:
           git branch --show-current ; git rev-parse HEAD ; git rev-parse origin/main ;
           git ls-files src/RackCad.UI/Systems/PushBack (10 archivos) ; git ls-files src/RackCad.Application/Systems/PushBack (64 archivos) ; git ls-files tests/RackCad.UI.Tests (195 archivos) ; git ls-files tests/RackCad.Tests (707 archivos) ;
           y lee el codigo, las pruebas y las guardas que cites
No-touch: todo el repositorio
Condiciones de parada: S-03, P-03 y las PBTC-C* del contrato; ante cualquiera, para y explica el bloqueo en RoutingReason.
COPIA: en las tareas anteriores de esta unidad, varias planificaciones se rechazaron solo por la copia de las colecciones opacas: un elemento de una lista omitido, un espacio anadido al principio de una cadena, una palabra perdida dentro de un nombre de archivo ("WorkspacePaletteHostGuardTests.cs" en vez de "WorkspacePaletteHostSourceGuardTests.cs") o un verbo cambiado de modo ("es" copiado como "sea"). Ninguna entrada de ninguna lista empieza ni termina con un espacio.
Una sola diferencia de un caracter o un solo elemento de mas o de menos basta para rechazar. Por la parada PBTC-C08 del contrato, un rechazo solo por la copia admite una sola replanificacion y un segundo va con STOP al Owner.
REPLANIFICACION UNICA (parada PBTC-C08 del contrato; decision de procedimiento del Owner, F4B-E03): la planificacion anterior de esta delegacion (R20261118T020031Z-8db0) se RECHAZO solo por la copia, en un sitio. Esta es la unica replanificacion: un segundo rechazo de fidelidad vuelve al Owner.
  Invariants, indice 15 (INV-PBTC-16, el registro): copia la cadena ENTERA desde la carga, sin acortar ninguna frase; termina exactamente asi: "su paso de DECLARADA a EJECUTADA y la anotacion del paso 6 de D-PB-06 en f4b-dispositions.md son de la sesion.". Las frases largas que nombran un archivo lo nombran completo: no quites el complemento que dice DONDE se anota algo.
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

CONTEXTO (hechos versionados en el HEAD; evidencia section 231 y siguientes de docs/automation/evidence/I-64-evidence.md; F4B-E01 a F4B-E07 en docs/automation/evidence/I-64-pilot/F4B-DRAFTS/f4b-dispositions.md)
- F4 esta cerrada (CLOSED / PASS WITH LIMITATIONS). El Owner autorizo F4B (Push Back completo); PB-T0 (la caracterizacion) esta CLOSED / PASS WITH LIMITATIONS (F4B-E05). Tras las decisiones del Owner (F4B-E06) y su orden expresa de emitir (F4B-E07), PB-TC corrige en la ventana clasica de Push Back, como cambio DECLARADO de RACKEDITAR: D-PB-06 (a) el redondeo de diez campos con la regla Keep, (b) el estilo de cotas, D-PB-08 el aviso no bloqueante en la banda de estado, y D-PB-11 que el "Restaurar estandar" llegue al Actualizar. El caso H-31 (GapBox, ModuleLengthBox y las cuatro cajas avanzadas anulables) NO entra: vuelve al Owner antes de PB-T1.
  Presupuesto: Attempt 1, AttemptsRemaining 2, MaxReworkLoops 3, CorrectionsAuthorized true; 1 invocacion de Worker por delegacion aceptada; un intento 3 vuelve siempre al Owner (PBTC-C12).
- HECHOS DEL REPOSITORIO medidos por la sesion en el HEAD (comprueba los que uses):
  * El filtro con ExpectRed true (FullyQualifiedName~RackCad.UI.Tests.PushBackDeclaredCorrection) selecciona 0 pruebas en el HEAD: ningun nombre de clase empieza por PushBackDeclaredCorrection. Las clases nuevas van en namespace RackCad.UI.Tests; el ayudante PushBackDeclaredCorrectionSupport.cs es internal static y sin pruebas. Core no tiene pruebas nuevas ni RED (sus filtros van con ExpectRed false; precedente F4-T3B1).
  * El RED es ORDINARIO (D-PBTC-05 del paquete): pruebas nuevas que fallan por asercion en la base, mas la linea del atributo de las cuatro pruebas sustituidas SUS-01 a SUS-04 (pasan a Skip con el literal del paquete), y ningun archivo de src/. El GREEN toca solo los tres archivos de producto del AllowedWriteScope (la ventana clasica: .xaml.cs, .Composite.cs y .xaml) y ningun archivo de tests/.
  * Antes del RED (D-PBTC-10): una corrida TEMPORAL sobre la BaseSha de CT-PBTC-10 con Assert.Fail y la formula de CT-PB0-21 para las filas F1 y M-02; si muestra una linea bajo Modules o HeaderLineOverrides o un efecto material, es PBTC-C14: STOP y vuelta al Owner antes del GREEN.
  * La CI corre las suites completas en Debug y compila el Plugin en Release (job build-plugin, que depende de tests y de build-ui, no de ui-tests). En el RED se espera ui-tests en failure y tests, build-ui y build-plugin en success; en el RED ninguna prueba existente falla, porque el producto no cambia.
  * Lo que esta FIJADO en el paquete y se traslada a los AcceptanceCriteria como receta, sin reconstruirlo: la lista cerrada CT-PBTC-01 a 08, 10 y 14 de la section 4.2 (un CT = un metodo [Fact], sin Theory), con su mapa CT a metodo (4.4) y sus lineas esperadas; las definiciones comunes de 4.1; el diseno por linea de D-PBTC-01, 02, 03 y 11; las sustituciones SUS-01 a SUS-04 de 5.2 con su literal de Skip; las comprobaciones temporales sobre el GREEN de 5.2; ninguna ventana se muestra; el literal del aviso byte a byte; el Worker no ajusta lo esperado (PBTC-C07).
  * Sin AutoCAD: la comprobacion fisica es de Smoke-PB (INV del contrato).

ALCANCE DE ESCRITURA
- AllowedWriteScope de la delegacion = EXACTAMENTE las 13 entradas del contrato, en su orden (13 archivos exactos, sin prefijos):
    src/RackCad.UI/Systems/PushBack/RackPushBackSystemWindow.xaml.cs
    src/RackCad.UI/Systems/PushBack/RackPushBackSystemWindow.Composite.cs
    src/RackCad.UI/Systems/PushBack/RackPushBackSystemWindow.xaml
    tests/RackCad.UI.Tests/PushBackDeclaredCorrectionSupport.cs
    tests/RackCad.UI.Tests/PushBackDeclaredCorrectionRoundingTests.cs
    tests/RackCad.UI.Tests/PushBackDeclaredCorrectionDimensionStyleTests.cs
    tests/RackCad.UI.Tests/PushBackDeclaredCorrectionPilotParityTests.cs
    tests/RackCad.UI.Tests/PushBackDeclaredCorrectionSideBNoticeTests.cs
    tests/RackCad.UI.Tests/PushBackDeclaredCorrectionStandardRestoreTests.cs
    tests/RackCad.UI.Tests/Workspace/WorkspaceDraftPilotWindowLoadCharacterizationTests.cs
    tests/RackCad.UI.Tests/PushBackDimensionViewsWindowTests.cs
    tests/RackCad.UI.Tests/PushBackFullEditorCharacterizationRoundingLossTests.cs
    tests/RackCad.UI.Tests/PushBackFullEditorCharacterizationModuleTests.cs
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
- Todas las cadenas del contrato canonico artifacts/orchestration/I-64/F4B-PBTC-DECLARED-CORRECTIONS/1/R20261118T020622Z-7171/gate-contract.json son ASCII puro. Se copian byte a byte.
- Authorities, ForbiddenWriteScope, Invariants, RequiredTests, ExpectedEvidence, StopConditions y Objective son DATOS OPACOS de ese contrato. CARGALOS desde el archivo con una orden y reproducelos en tu salida a partir de esa carga, por ejemplo:
  python -c "import json; c=json.load(open(r'artifacts/orchestration/I-64/F4B-PBTC-DECLARED-CORRECTIONS/1/R20261118T020622Z-7171/gate-contract.json', encoding='ascii')); print(json.dumps({k: c[k] for k in ['Authorities','ForbiddenWriteScope','Invariants','RequiredTests','ExpectedEvidence','StopConditions','Objective','AllowedWriteScope']}, ensure_ascii=True, separators=(chr(44), chr(58))))"
  Mismo orden, mismos elementos, mismos textos byte a byte.
- MODO VERBAL Y GRAMATICA: el contrato usa a proposito construcciones que pueden parecer mejorables (indicativo tras si, con o comprueba; nombres en vez de verbos); se copian IGUAL. Antes de terminar, imprime cada invariante del contrato NUMERADO (por ejemplo: python -c "import json; c=json.load(open(r'artifacts/orchestration/I-64/F4B-PBTC-DECLARED-CORRECTIONS/1/R20261118T020622Z-7171/gate-contract.json', encoding='ascii')); [print(i, x) for i, x in enumerate(c['Invariants'])]") y compara tu salida con esa impresion, invariante por invariante y verbo por verbo.
- AUTHORITIES: varias autoridades citan paquetes con nombres casi iguales; se copian cada una en su indice y con su nombre exacto: Authorities[0].Path = "docs/initiatives/I-64-f4b-pbtc-gate-contract-package.md"; Authorities[1].Path = "docs/initiatives/I-64-f4b-pbtc-measurements.md"; Authorities[2].Path = "docs/initiatives/I-64-f4b-push-back-completo-package.md"; Authorities[10].Path = "docs/initiatives/I-64-f4-t1b-gate-contract-package.md". No los mezcles ni los acortes.
- SIN CONFLICTOS INEXISTENTES: el contrato canonico de esta corrida es ASCII PURO (0 bytes no ASCII; compruebalo con python -c "b=open(r'artifacts/orchestration/I-64/F4B-PBTC-DECLARED-CORRECTIONS/1/R20261118T020622Z-7171/gate-contract.json', 'rb').read(); print(sum(1 for x in b if x >= 0x80))"); TaskClass se escribe exactamente como lo fija este prompt, sin tilde (la tabla de routing.md lleva tilde y NO se copia de alli). Antes de alegar cualquier conflicto o parada S-03, carga el contrato y cita en RoutingReason la orden y su salida que lo demuestran; sin esa prueba no hay conflicto y emites la delegacion completa.
- PROHIBIDO, en TODAS las cadenas copiadas: mejorar la gramatica; normalizar singular o plural o la concordancia; cambiar la puntuacion; anadir o quitar espacios;
  anadir o quitar tildes; cambiar mayusculas; reflujar, partir o unir el texto; parafrasear; corregir lo que parezca una errata. Si una frase te parece incorrecta, se copia IGUAL.
- AllowedWriteScope es la barrera PRIMARIA y exacta de escritura: copialo elemento a elemento, en el mismo orden, sin anadir, quitar, ensanchar ni normalizar ninguna entrada.
  ForbiddenWriteScope tiene exactamente las 131 entradas del contrato: ni una mas ni una menos, y ninguna con espacios anadidos.
- FORBIDDENWRITESCOPE, COPIA REFORZADA DESDE EL PRIMER INTENTO: tiene exactamente 131 entradas, de la 0 a la 130, todas distintas. Para copiarla, ejecuta esta orden, que la imprime SOLA como un array JSON compacto en una linea, sin ningun espacio entre los elementos, y reproduce cada cadena tal cual, de una vez, sin escribir las entradas de memoria:
  python -c "import json; c=json.load(open(r'artifacts/orchestration/I-64/F4B-PBTC-DECLARED-CORRECTIONS/1/R20261118T020622Z-7171/gate-contract.json', encoding='ascii')); print(json.dumps(c['ForbiddenWriteScope'], ensure_ascii=True, separators=(chr(44), chr(58))))"
  Puntos de control, comprobados contra la impresion numerada: la 0 es src/RackCad.Application/; de la 0 a la 2, prefijos y archivos de src/ fuera de src/RackCad.UI/; de la 3 a la 57, src/RackCad.UI/ (la 3 es src/RackCad.UI/CatalogOption.cs y, dentro, los 7 archivos o prefijos de src/RackCad.UI/Systems/PushBack/ que no se tocan, de la 46 a la 52); de la 58 a la 113, prefijos y archivos de tests/ (la 58 es tests/RackCad.Tests/; los hermanos de tests/RackCad.UI.Tests/Workspace/ van uno a uno, de la 91 a la 113); de la 114 a la 130, la raiz (la 114 es docs/ y la 130, la ULTIMA de la lista, .gitignore).
  Ahi termina la lista: nunca vuelvas atras ni repitas un nombre.
  Antes de terminar, cuenta los elementos de tu ForbiddenWriteScope (deben ser 131), comprueba que no hay repetidos, compara cada indice con la impresion numerada y comprueba que ninguna entrada empieza ni termina con un espacio.
- Para ForbiddenWriteScope y AllowedWriteScope, ejecuta ademas una orden que imprima cada entrada del contrato NUMERADA (por ejemplo: python -c "import json; c=json.load(open(r'artifacts/orchestration/I-64/F4B-PBTC-DECLARED-CORRECTIONS/1/R20261118T020622Z-7171/gate-contract.json', encoding='ascii')); [print(i, repr(x)) for i, x in enumerate(c['ForbiddenWriteScope'])]") y comprueba tu lista contra esa salida, indice por indice, de 0 a 130, mirando cada palabra de cada nombre de archivo (una planificacion anterior perdio la palabra Source en un nombre).
- STOPCONDITIONS: la PBTC-C11 es larga a proposito (las firmas literales de PD-F4-01 y PD-F4-04 y la regla de coincidencia); copiala entera desde la carga, sin resumirla ni partirla. P-01 nombra la Snapshot I.
- Antes de dar la salida por terminada, compara tu salida con el contrato cargado campo por campo, cadena por cadena y contando los elementos de cada lista; la sesion ejecuta despues una comparacion determinista byte a byte
  de Invariants, RequiredTests, StopConditions, Authorities, ExpectedEvidence, Objective, AllowedWriteScope y ForbiddenWriteScope ANTES de aceptar: cualquier diferencia = salida RECHAZADA y sin Worker.
- TODA tu salida debe ser ASCII puro, tambien AcceptanceCriteria y RoutingReason: sin tildes, sin enye y sin signos no ASCII; ninguna secuencia de escape formada por barra invertida + u; ningun tabulador ni otro caracter de control dentro de las cadenas.
- RequiredTests con los mismos 12 elementos, en el mismo orden y con los mismos valores (MinSelected 10, 67, 55, 16, 3, 12, 9, 13, 10, 3, 9, 15; ExpectRed true solo en el primero).

TASKCLASS
- TaskClass = exactamente "Implementacion transversal a capas (corta)"; PromptProfile = "ROUTINE_IMPLEMENTATION"; explica la eleccion en RoutingReason con las siete dimensiones (routing.md section 2) y la fecha de verificacion de la celda: correccion de producto en la ventana WPF (codigo y XAML) con pruebas de UI nuevas, sustituciones declaradas y una medicion temporal previa. Si tu evaluacion da Horizonte High: para con S-03 explicado en RoutingReason.
- Celda del Worker: Executor.Cell = "anthropic/subagent/claude-sonnet-5-5"; Executor.Provider = "Anthropic"; Executor.Transport = "subagent"; Model = "claude-sonnet-5-5"; Effort.Provider = "high"; Effort.Semantic = "Deep"; ModelEscalationReason = null.

DELTA DE LA TAREA (primera delegacion de la cadena F4B-PBTC-DECLARED-CORRECTIONS)
- RunId = R20261118T020622Z-7171 (tambien es el DelegationRunId); Attempt = 1; AttemptsRemaining = 2; MaxReworkLoops = 3.
- Unit = "I-64"; Initiative = "I-64"; Gate = "F4B"; TaskId = "F4B-PBTC-DECLARED-CORRECTIONS".
- ExpectedBranch = la salida de `git branch --show-current`; BaseSha = la salida de `git rev-parse HEAD`; ChainBaseSha = ese mismo BaseSha; ChainRedSha = null; ChainRedFiles = []; CorrectionOf = null.
- MainSha = 812baf879612320653094a8aaf46aca0bac81383 (comprueba que coincide con `git rev-parse origin/main`; si no coincide, para con S-13 explicado en RoutingReason); AuthorityRevision = la del contrato.
- ExpectedWorktree = la ruta absoluta del directorio de trabajo actual.
- ExpectedHandoffPath = artifacts/orchestration/I-64/F4B-PBTC-DECLARED-CORRECTIONS/1/{WorkRunId}/worker-handoff.json (con el token literal).
- Owner = { Kind: "subagent", Id: "subagent:R20261118T020622Z-7171" }; RoutingEnforcement = "required". Executor.Role = "Worker".
- AcceptanceCriteria: redactalos tu, en ASCII, verificables y trazables a los invariantes del contrato; incluye: la medicion temporal de la base de D-PBTC-10 con PBTC-C14; el RED solo de pruebas (las nuevas y la linea del atributo de SUS-01 a SUS-04), con el criterio mecanico de INV-PBTC-02 en el filtro con ExpectRed; cada CT de CT-PBTC-01 a 08, 10 y 14 con su metodo del mapa 4.4 y sus lineas esperadas sin ajustarlas (PBTC-C07); el GREEN solo en los tres archivos de producto, con el diseno por linea de D-PBTC-01, 02, 03 y 11; las comprobaciones temporales de las sustituciones sobre el GREEN, con sus artefactos; nada del caso H-31 (INV-PBTC-15, PBTC-C10); la contencion de las corridas locales y la prueba de limpieza; ninguna prueba que cuelgue, deje un modal vivo, escriba en el perfil, necesite AutoCAD o dependa de rutas de la maquina; la CI exacta del RED y del GREEN.
- IssuedBy = "Controller Codex (planificacion F4B-PBTC-DECLARED-CORRECTIONS, primera delegacion de la cadena, CONTROLLER_PLANNING)".
