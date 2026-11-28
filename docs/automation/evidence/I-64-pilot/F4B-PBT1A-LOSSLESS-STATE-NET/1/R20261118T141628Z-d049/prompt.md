I-64 - DELEGACION F4B-PBT1A-LOSSLESS-STATE-NET - Controller, perfil CONTROLLER_PLANNING (primera delegacion de la cadena F4B-PBT1A-LOSSLESS-STATE-NET)
Identidad: unidad I-64; gate F4B; tarea F4B-PBT1A-LOSSLESS-STATE-NET; intento 1; RunId R20261118T141628Z-d049;
           DelegationRunId R20261118T141628Z-d049; AuthorityRevision 95c8126ba88c4e355cc09c51db972ff7f757251a (la del contrato);
           MainSha 812baf879612320653094a8aaf46aca0bac81383; BaseSha = el HEAD actual del worktree (se espera a3b63ccc8c8c7ec758960c10f872c371338ce104: commit de emision del contrato, hijo directo de la AuthorityRevision 95c8126ba88c4e355cc09c51db972ff7f757251a:
           solo anade el contrato, su script generador, el estado y la evidencia);
           rama architecture/workspace-persistente-rackcad; worktree = el directorio de trabajo actual; Owner subagent:R20261118T141628Z-d049
Contrato de gate: artifacts/orchestration/I-64/F4B-PBT1A-LOSSLESS-STATE-NET/1/R20261118T141628Z-d049/gate-contract.json (ASCII puro; custodiado en docs/automation/evidence/I-64-pilot/F4B-PBT1A-LOSSLESS-STATE-NET/coordinator/gate-contract.json)
Autoridades (leer desde Git, no copiar su contenido): las del campo Authorities del contrato;
           vigencia: AuthorityRevision para la unidad (UNIT_DOC y UNIT_CHANGE), MainSha para lo demas (EXTERNAL)
Objetivo: emitir el paquete de delegacion (esquema rackcad-delegation/v1) del Worker para la tarea del contrato. Debe ser ejecutable tal cual.
Alcance: solo lectura. No escribas ningun archivo, no hagas commit ni push. No ejecutes dotnet, ni AutoCAD, ni builds.
Invariantes del Freeze: los 14 del contrato; una delegacion nunca amplia el contrato
Rutas: ver ALCANCE DE ESCRITURA abajo. Todo lo demas es de solo lectura.
           Fuente de diseno: docs/initiatives/I-64-f4b-pbt1-gate-contract-package.md v2.4 (section 2, hechos medidos y remedidos sobre b5c09f0b; section 3, D-PBT1-01 la division en tres subtareas, D-PBT1-12, D-PBT1-14 y D-PBT1-15 con la decision D2 del Owner y su limite; section 5.0, las recetas X-01, F-01, F-02 y F-03 y el ayudante; section 5.2, el regimen de PB-T1a, las clases, la lista cerrada CT-PBT1A-01 a CT-PBT1A-14 y la tabla CERRADA de mutantes MU-T1A-01 a MU-T1A-14 con 07a y 07b; section 6.0 y 6.1, el contrato; section 7, riesgos; section 8, las decisiones del Owner, decididas, y la remedicion; section 10, los arrastres que no son de PB-T1a), con su registro de mediciones docs/initiatives/I-64-f4b-pbt1-measurements.md, las decisiones literales del Owner (docs/automation/evidence/I-64-pilot/F4B-DRAFTS/coordinator/owner-pbt1-pre-decisions.md y owner-d1-d2-decisions.md) y el paquete de PB-T0 docs/initiatives/I-64-f4b-pbt0-gate-contract-package.md v2.1 como patron del RED por mutantes revertidos
Criterios de aceptacion: la salida valida contra el esquema; colecciones opacas del contrato copiadas byte a byte (ver COPIA EXACTA);
           AllowedWriteScope segun ALCANCE DE ESCRITURA; celda y effort entre las EligibleCells del contrato; TaskClass y campos de la DELTA exactamente como se indican
Evidencia requerida: ejecuta de verdad estas ordenes en el worktree y usa sus resultados:
           git branch --show-current ; git rev-parse HEAD ; git rev-parse origin/main ;
           git ls-files src/RackCad.UI/Systems/PushBack (10 archivos) ; git ls-files src/RackCad.Application/Systems/PushBack (64 archivos) ; git ls-files tests/RackCad.UI.Tests (203 archivos) ; git ls-files tests/RackCad.Tests (707 archivos) ;
           y lee el codigo, las pruebas y las guardas que cites
No-touch: todo el repositorio
Condiciones de parada: S-03, P-03 y las PBT1A-C* del contrato; ante cualquiera, para y explica el bloqueo en RoutingReason.
COPIA: en las tareas anteriores de esta unidad, varias planificaciones se rechazaron solo por la copia de las colecciones opacas: un elemento de una lista omitido, un espacio anadido al principio de una cadena, una palabra perdida dentro de un nombre de archivo ("WorkspacePaletteHostGuardTests.cs" en vez de "WorkspacePaletteHostSourceGuardTests.cs") o un verbo cambiado de modo ("es" copiado como "sea"). Ninguna entrada de ninguna lista empieza ni termina con un espacio.
Una sola diferencia de un caracter o un solo elemento de mas o de menos basta para rechazar. Por la parada PBT1A-C08 del contrato, un rechazo solo por la copia admite una sola replanificacion y un segundo va con STOP al Owner.
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

CONTEXTO (hechos versionados en el HEAD; evidencia section 231 y siguientes de docs/automation/evidence/I-64-evidence.md; F4B-E01 a F4B-E13 en docs/automation/evidence/I-64-pilot/F4B-DRAFTS/f4b-dispositions.md)
- F4 esta cerrada. En F4B, PB-T0, PB-TC y PB-TC2 estan CLOSED / PASS WITH LIMITATIONS (F4B-E05, F4B-E08 y F4B-E12). PB-T1 (sacar de la ventana WPF el estado editable de Push Back sin perdidas) se divide en tres subtareas secuenciales (D-PBT1-01); esta es la primera, PB-T1a: SOLO la red de caracterizacion de la ventana clasica, con pruebas nuevas y un RED por mutantes revertidos; el producto no cambia. El estado nuevo es de PB-T1b y la ventana sobre el estado, de PB-T1c: nada de eso se hace aqui. La decision D2 del Owner (F4B-E11) y su limite van como la parada PBT1A-C17. Emitida por F4B-E13.
  Presupuesto: Attempt 1, AttemptsRemaining 2, MaxReworkLoops 3, CorrectionsAuthorized true; 1 invocacion de Worker por delegacion aceptada; un intento 3 vuelve siempre al Owner (PBT1A-C12).
- HECHOS DEL REPOSITORIO medidos por la sesion en el HEAD (comprueba los que uses):
  * El filtro con ExpectRed true (FullyQualifiedName~RackCad.UI.Tests.PushBackStateNet) selecciona 0 pruebas en el HEAD. Los siete archivos nuevos (seis clases de prueba y el ayudante PushBackStateNetSupport, internal static y sin pruebas) van en namespace RackCad.UI.Tests. Las pruebas de PB-TC entran en RequiredTests clase por clase porque su prefijo seleccionaria tambien las de PB-TC2.
  * El RED es por MUTANTES REVERTIDOS (el patron de PB-T0): los siete archivos nuevos de prueba MAS los quince mutantes de la tabla CERRADA de INV-PBT1A-02 (MU-T1A-01 a MU-T1A-14, con 07a y 07b), juntos, en W y WC (la ventana clasica: .xaml.cs y .Composite.cs); el GREEN es git restore --source=BaseSha de W y WC y no toca tests/. Por eso el AllowedWriteScope tiene 9 rutas: W, WC y los siete archivos nuevos; git diff BaseSha..CurrentSha -- src queda vacio. WX no se toca.
  * Antes del RED (paso 1, INV-PBT1A-04): el Worker mide las lineas esperadas en la BaseSha con codigo TEMPORAL que acaba en Assert.Fail con las lineas, tambien con el aviso de descarte sustituido, y las transcribe en la entrega antes del RED; esas corridas no entran en ningun commit. Despues, cada mutante aplicado AISLADO sobre la BaseSha hace fallar por asercion el CT de su fila (INV-PBT1A-03); si no, PBT1A-C04.
  * La CI corre las suites completas en Debug y compila el Plugin en Release. En el RED se espera ui-tests en failure y tests, build-ui y build-plugin en success (los mutantes son solo de la UI: Core sigue en success, RedPart = pass por F4-E56). En el RED pueden fallar pruebas existentes de UI SOLO por los mutantes, con la atribucion de la tabla T-8 (MU-T1A-07a puede hacer fallar tambien pruebas de PB-TC2); el GREEN las devuelve a verde.
  * Lo que esta FIJADO en el paquete y se traslada a los AcceptanceCriteria como receta, sin reconstruirlo: la lista cerrada CT-PBT1A-01 a 14 con su reparto en las seis clases (un CT = un metodo [Fact] que acaba en un Assert.Equal); la tabla cerrada de mutantes (sustitucion exacta de una linea; MU-T1A-13 anclado por linea y miembro, W:313, nunca W:296); las recetas X-01, F-01, F-02 y F-03 de la section 5.0 con la autocomprobacion de F-03; el corpus y los guiones G-1 a G-6 de CT-PBT1A-14, en ese orden; la ventana fuera de pantalla de CT-PBT1A-11; el aviso de descarte sustituido con el sufijo descartes=<n>; el Worker no ajusta lo esperado (PBT1A-C07).
  * INV-PBT1A-13: el Worker declara en UnexpectedFindings cada linea fijada que muestre una perdida o un control releido que cambia el estado o la salida sin edicion, distinta de c-01, c-02 y M-07; no la corrige.
  * Sin AutoCAD.

ALCANCE DE ESCRITURA
- AllowedWriteScope de la delegacion = EXACTAMENTE las 9 entradas del contrato, en su orden (9 archivos exactos, sin prefijos):
    src/RackCad.UI/Systems/PushBack/RackPushBackSystemWindow.xaml.cs
    src/RackCad.UI/Systems/PushBack/RackPushBackSystemWindow.Composite.cs
    tests/RackCad.UI.Tests/PushBackStateNetSupport.cs
    tests/RackCad.UI.Tests/PushBackStateNetRackControlsTests.cs
    tests/RackCad.UI.Tests/PushBackStateNetCellAndFrontTests.cs
    tests/RackCad.UI.Tests/PushBackStateNetCompositeTests.cs
    tests/RackCad.UI.Tests/PushBackStateNetModuleAndSafetyTests.cs
    tests/RackCad.UI.Tests/PushBackStateNetActionsTests.cs
    tests/RackCad.UI.Tests/PushBackStateNetFingerprintTests.cs
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
- Todas las cadenas del contrato canonico artifacts/orchestration/I-64/F4B-PBT1A-LOSSLESS-STATE-NET/1/R20261118T141628Z-d049/gate-contract.json son ASCII puro. Se copian byte a byte.
- Authorities, ForbiddenWriteScope, Invariants, RequiredTests, ExpectedEvidence, StopConditions y Objective son DATOS OPACOS de ese contrato. CARGALOS desde el archivo con una orden y reproducelos en tu salida a partir de esa carga, por ejemplo:
  python -c "import json; c=json.load(open(r'artifacts/orchestration/I-64/F4B-PBT1A-LOSSLESS-STATE-NET/1/R20261118T141628Z-d049/gate-contract.json', encoding='ascii')); print(json.dumps({k: c[k] for k in ['Authorities','ForbiddenWriteScope','Invariants','RequiredTests','ExpectedEvidence','StopConditions','Objective','AllowedWriteScope']}, ensure_ascii=True, separators=(chr(44), chr(58))))"
  Mismo orden, mismos elementos, mismos textos byte a byte.
- MODO VERBAL Y GRAMATICA: el contrato usa a proposito construcciones que pueden parecer mejorables (indicativo tras si, con o comprueba; nombres en vez de verbos); se copian IGUAL. Antes de terminar, imprime cada invariante del contrato NUMERADO (por ejemplo: python -c "import json; c=json.load(open(r'artifacts/orchestration/I-64/F4B-PBT1A-LOSSLESS-STATE-NET/1/R20261118T141628Z-d049/gate-contract.json', encoding='ascii')); [print(i, x) for i, x in enumerate(c['Invariants'])]") y compara tu salida con esa impresion, invariante por invariante y verbo por verbo.
- AUTHORITIES: varias autoridades citan paquetes con nombres casi iguales; se copian cada una en su indice y con su nombre exacto: Authorities[0].Path = "docs/initiatives/I-64-f4b-pbt1-gate-contract-package.md"; Authorities[1].Path = "docs/initiatives/I-64-f4b-pbt1-measurements.md"; Authorities[5].Path = "docs/initiatives/I-64-f4b-push-back-completo-package.md"; Authorities[16].Path = "docs/initiatives/I-64-f4b-pbt0-gate-contract-package.md"; Authorities[19].Path = "docs/initiatives/I-64-f4b-pbtc-gate-contract-package.md"; Authorities[21].Path = "docs/initiatives/I-64-f4b-pbtc2-gate-contract-package.md"; Authorities[22].Path = "docs/initiatives/I-64-f4b-pbtc2-measurements.md"; Authorities[25].Path = "docs/initiatives/I-64-f4-t1b-gate-contract-package.md". No los mezcles ni los acortes.
- SIN CONFLICTOS INEXISTENTES: el contrato canonico de esta corrida es ASCII PURO (0 bytes no ASCII; compruebalo con python -c "b=open(r'artifacts/orchestration/I-64/F4B-PBT1A-LOSSLESS-STATE-NET/1/R20261118T141628Z-d049/gate-contract.json', 'rb').read(); print(sum(1 for x in b if x >= 0x80))"); TaskClass se escribe exactamente como lo fija este prompt, sin tilde (la tabla de routing.md lleva tilde y NO se copia de alli). Antes de alegar cualquier conflicto o parada S-03, carga el contrato y cita en RoutingReason la orden y su salida que lo demuestran; sin esa prueba no hay conflicto y emites la delegacion completa.
- PROHIBIDO, en TODAS las cadenas copiadas: mejorar la gramatica; normalizar singular o plural o la concordancia; cambiar la puntuacion; anadir o quitar espacios;
  anadir o quitar tildes; cambiar mayusculas; reflujar, partir o unir el texto; parafrasear; corregir lo que parezca una errata. Si una frase te parece incorrecta, se copia IGUAL.
- AllowedWriteScope es la barrera PRIMARIA y exacta de escritura: copialo elemento a elemento, en el mismo orden, sin anadir, quitar, ensanchar ni normalizar ninguna entrada.
  ForbiddenWriteScope tiene exactamente las 131 entradas del contrato: ni una mas ni una menos, y ninguna con espacios anadidos.
- FORBIDDENWRITESCOPE, COPIA REFORZADA DESDE EL PRIMER INTENTO: tiene exactamente 131 entradas, de la 0 a la 130, todas distintas. Para copiarla, ejecuta esta orden, que la imprime SOLA como un array JSON compacto en una linea, sin ningun espacio entre los elementos, y reproduce cada cadena tal cual, de una vez, sin escribir las entradas de memoria:
  python -c "import json; c=json.load(open(r'artifacts/orchestration/I-64/F4B-PBT1A-LOSSLESS-STATE-NET/1/R20261118T141628Z-d049/gate-contract.json', encoding='ascii')); print(json.dumps(c['ForbiddenWriteScope'], ensure_ascii=True, separators=(chr(44), chr(58))))"
  Puntos de control, comprobados contra la impresion numerada: la 0 es src/RackCad.Application/; de la 0 a la 2, prefijos y archivos de src/ fuera de src/RackCad.UI/; de la 3 a la 58, src/RackCad.UI/ (la 3 es src/RackCad.UI/CatalogOption.cs y, dentro, los 8 archivos o prefijos de src/RackCad.UI/Systems/PushBack/ que no se tocan, de la 46 a la 53); de la 59 a la 113, prefijos y archivos de tests/ (la 59 es tests/RackCad.Tests/); de la 114 a la 130, la raiz (la 114 es docs/ y la 130, la ULTIMA de la lista, .gitignore).
  Ahi termina la lista: nunca vuelvas atras ni repitas un nombre.
  Antes de terminar, cuenta los elementos de tu ForbiddenWriteScope (deben ser 131), comprueba que no hay repetidos, compara cada indice con la impresion numerada y comprueba que ninguna entrada empieza ni termina con un espacio.
- Para ForbiddenWriteScope y AllowedWriteScope, ejecuta ademas una orden que imprima cada entrada del contrato NUMERADA (por ejemplo: python -c "import json; c=json.load(open(r'artifacts/orchestration/I-64/F4B-PBT1A-LOSSLESS-STATE-NET/1/R20261118T141628Z-d049/gate-contract.json', encoding='ascii')); [print(i, repr(x)) for i, x in enumerate(c['ForbiddenWriteScope'])]") y comprueba tu lista contra esa salida, indice por indice, de 0 a 130, mirando cada palabra de cada nombre de archivo (una planificacion anterior perdio la palabra Source en un nombre).
- STOPCONDITIONS: la PBT1A-C11 es larga a proposito (las firmas literales de PD-F4-01 y PD-F4-04 y la regla de coincidencia); copiala entera desde la carga, sin resumirla ni partirla. P-01 nombra la Snapshot I.
- Antes de dar la salida por terminada, compara tu salida con el contrato cargado campo por campo, cadena por cadena y contando los elementos de cada lista; la sesion ejecuta despues una comparacion determinista byte a byte
  de Invariants, RequiredTests, StopConditions, Authorities, ExpectedEvidence, Objective, AllowedWriteScope y ForbiddenWriteScope ANTES de aceptar: cualquier diferencia = salida RECHAZADA y sin Worker.
- TODA tu salida debe ser ASCII puro, tambien AcceptanceCriteria y RoutingReason: sin tildes, sin enye y sin signos no ASCII; ninguna secuencia de escape formada por barra invertida + u; ningun tabulador ni otro caracter de control dentro de las cadenas.
- RequiredTests con los mismos 19 elementos, en el mismo orden y con los mismos valores (MinSelected 14, 55, 16, 3, 2, 2, 2, 1, 4, 67, 52, 7, 3, 10, 13, 9, 10, 3, 15; ExpectRed true solo en el primero).

TASKCLASS
- TaskClass = exactamente "Implementacion transversal a capas (corta)"; PromptProfile = "ROUTINE_IMPLEMENTATION"; explica la eleccion en RoutingReason con las siete dimensiones (routing.md section 2) y la fecha de verificacion de la celda: pruebas de caracterizacion nuevas de la ventana WPF con mutantes temporales del producto en la ventana, una medicion temporal previa (paso 1) y ningun cambio neto de producto. Si tu evaluacion da Horizonte High: para con S-03 explicado en RoutingReason.
- Celda del Worker: Executor.Cell = "anthropic/subagent/claude-sonnet-5-5"; Executor.Provider = "Anthropic"; Executor.Transport = "subagent"; Model = "claude-sonnet-5-5"; Effort.Provider = "high"; Effort.Semantic = "Deep"; ModelEscalationReason = null.

DELTA DE LA TAREA (primera delegacion de la cadena F4B-PBT1A-LOSSLESS-STATE-NET)
- RunId = R20261118T141628Z-d049 (tambien es el DelegationRunId); Attempt = 1; AttemptsRemaining = 2; MaxReworkLoops = 3.
- Unit = "I-64"; Initiative = "I-64"; Gate = "F4B"; TaskId = "F4B-PBT1A-LOSSLESS-STATE-NET".
- ExpectedBranch = la salida de `git branch --show-current`; BaseSha = la salida de `git rev-parse HEAD`; ChainBaseSha = ese mismo BaseSha; ChainRedSha = null; ChainRedFiles = []; CorrectionOf = null.
- MainSha = 812baf879612320653094a8aaf46aca0bac81383 (comprueba que coincide con `git rev-parse origin/main`; si no coincide, para con S-13 explicado en RoutingReason); AuthorityRevision = la del contrato.
- ExpectedWorktree = la ruta absoluta del directorio de trabajo actual.
- ExpectedHandoffPath = artifacts/orchestration/I-64/F4B-PBT1A-LOSSLESS-STATE-NET/1/{WorkRunId}/worker-handoff.json (con el token literal).
- Owner = { Kind: "subagent", Id: "subagent:R20261118T141628Z-d049" }; RoutingEnforcement = "required". Executor.Role = "Worker".
- AcceptanceCriteria: redactalos tu, en ASCII, verificables y trazables a los invariantes del contrato; incluye: el paso 1 (las lineas esperadas medidas en la BaseSha y transcritas antes del RED); el RED con los siete archivos nuevos y SOLO los quince mutantes de la tabla cerrada, con el criterio mecanico de INV-PBT1A-02 en el filtro con ExpectRed; la prueba por mutante aislado de INV-PBT1A-03 con PBT1A-C04; la tabla T-8 de las pruebas existentes que fallan en el RED; el GREEN como git restore --source=BaseSha de W y WC, con git diff BaseSha..CurrentSha -- src vacio; cada CT de CT-PBT1A-01 a 14 con su metodo en su clase y sus lineas esperadas sin ajustarlas (PBT1A-C07); CT-PBT1A-14 con el corpus, las recetas y los guiones en su orden y la autocomprobacion de F-03; la lista de INV-PBT1A-13; la parada PBT1A-C17; la contencion de las corridas locales y la prueba de limpieza; ninguna prueba que cuelgue, deje un modal vivo, escriba en el perfil, necesite AutoCAD o dependa de rutas de la maquina; la CI exacta del RED y del GREEN.
- IssuedBy = "Controller Codex (planificacion F4B-PBT1A-LOSSLESS-STATE-NET, primera delegacion de la cadena, CONTROLLER_PLANNING)".
