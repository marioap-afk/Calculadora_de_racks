I-64 - DELEGACION F4-T1B-PILOT-CHARACTERIZATION - Controller, perfil CONTROLLER_PLANNING (primera delegacion de la cadena F4-T1B-PILOT-CHARACTERIZATION)
Identidad: unidad I-64; gate F4; tarea F4-T1B-PILOT-CHARACTERIZATION; intento 1; RunId R20261114T214147Z-851e;
           DelegationRunId R20261114T214147Z-851e; AuthorityRevision 24e3f4140543a09f8a7114abffaf06844abc9f94 (la del contrato);
           MainSha 812baf879612320653094a8aaf46aca0bac81383; BaseSha = el HEAD actual del worktree (se espera 0a6f84c298d0f542bbfdedabd82946490196e23f: commit de emision del contrato, hijo directo de la AuthorityRevision 24e3f4140543a09f8a7114abffaf06844abc9f94:
           solo anade el contrato, el estado y la evidencia);
           rama architecture/workspace-persistente-rackcad; worktree = el directorio de trabajo actual; Owner subagent:R20261114T214147Z-851e
Contrato de gate: artifacts/orchestration/I-64/F4-T1B-PILOT-CHARACTERIZATION/1/R20261114T214147Z-851e/gate-contract.json (ASCII puro; custodiado en docs/automation/evidence/I-64-pilot/F4-T1B-PILOT-CHARACTERIZATION/coordinator/gate-contract.json)
Autoridades (leer desde Git, no copiar su contenido): las del campo Authorities del contrato;
           vigencia: AuthorityRevision para la unidad (UNIT_DOC y UNIT_CHANGE), MainSha para lo demas (EXTERNAL)
Objetivo: emitir el paquete de delegacion (esquema rackcad-delegation/v1) del Worker para la tarea del contrato. Debe ser ejecutable tal cual.
Alcance: solo lectura. No escribas ningun archivo, no hagas commit ni push. No ejecutes dotnet, ni AutoCAD, ni builds.
Invariantes del Freeze: los 16 del contrato; una delegacion nunca amplia el contrato
Rutas: ver ALCANCE DE ESCRITURA abajo. Todo lo demas es de solo lectura.
           Fuente de diseno: docs/initiatives/I-64-f4-t1b-gate-contract-package.md v5 (section 2, hechos H-01 a H-24; section 3, la clausura por ruta real; section 4, decisiones D-T1B-01 a D-T1B-07 con la tabla CERRADA de mutantes de D-T1B-03, las aserciones ancla y la tabla de CT-01 a CT-28 de D-T1B-04; section 5, trazabilidad; section 6, contrato; section 9, interpretaciones; section 10, revisiones) y la autorizacion del Owner docs/automation/evidence/I-64-pilot/F4-DRAFTS/coordinator/owner-f4t1b-authorization.md
Criterios de aceptacion: la salida valida contra el esquema; colecciones opacas del contrato copiadas byte a byte (ver COPIA EXACTA);
           AllowedWriteScope segun ALCANCE DE ESCRITURA; celda y effort entre las EligibleCells del contrato; TaskClass y campos de la DELTA exactamente como se indican
Evidencia requerida: ejecuta de verdad estas ordenes en el worktree y usa sus resultados:
           git branch --show-current ; git rev-parse HEAD ; git rev-parse origin/main ;
           git ls-files src/RackCad.UI/Systems/PushBack (10 archivos) ; git ls-files tests/RackCad.UI.Tests (182 archivos) ; git ls-files src/RackCad.Plugin/Systems/PushBack (3 archivos) ; git ls-files tests/RackCad.Tests (691 archivos) ;
           y lee el codigo, las pruebas y las guardas que cites
No-touch: todo el repositorio
Condiciones de parada: S-03, P-03 y las F4T1B-C* del contrato; ante cualquiera, para y explica el bloqueo en RoutingReason.
Antecedentes: en esta unidad, cinco planificaciones se RECHAZARON por la copia: "lo leen" cambiado por "los leen" en un invariante (A5 FAIL); UNA entrada de ForbiddenWriteScope omitida (A4 FAIL); un espacio anadido delante de una entrada (" .github/", A4 FAIL); "WorkspacePaletteHostGuardTests.cs" en vez de "WorkspacePaletteHostSourceGuardTests.cs" (le falto la palabra Source; A4 FAIL); y, en la tarea anterior, "mientras la profundidad es mayor que 0" copiado como "sea" (modo verbal cambiado de indicativo a subjuntivo; A5 FAIL). Una sola diferencia de un caracter o un solo elemento de mas o de menos basta para rechazar. Por la parada F4T4A2-C08 del contrato, un rechazo admite una sola replanificacion y un segundo vuelve al Coordinator.
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

CONTEXTO (hechos versionados en el HEAD; evidencia section 198 y siguientes de docs/automation/evidence/I-64-evidence.md; F4-E57 y F4-E58 en docs/automation/evidence/I-64-pilot/F4-DRAFTS/f4-dispositions.md)
- F4-T1, F4-T2, F4-T3 y F4-T4A estan cerradas. El Owner autorizo la emision y la ejecucion de F4-T1B-PILOT-CHARACTERIZATION con el paquete v5: caracterizar con pruebas el comportamiento observable ACTUAL de la ventana clasica de Push Back (src/RackCad.UI/Systems/PushBack/RackPushBackSystemWindow.xaml y .xaml.cs) antes de que F4-T4B y F4-T4C dependan de sus costuras de escritura. Es SOLO caracterizacion: no corrige ni redisena nada; la deuda (D-T1B-06 del paquete) se registra.
  Presupuesto: Attempt 1, AttemptsRemaining 2, MaxReworkLoops 3, CorrectionsAuthorized true; 1 invocacion de Worker por delegacion aceptada.
- HECHOS DEL REPOSITORIO medidos por la sesion en el HEAD (comprueba los que uses):
  * Los dos filtros (FullyQualifiedName~RackCad.Tests.PushBackClassicCharacterization y FullyQualifiedName~RackCad.UI.Tests.PushBackClassicCharacterization) seleccionan 0 pruebas en el HEAD: ningun nombre de clase empieza por PushBackClassic. Las clases de Core estan en namespace RackCad.Tests y las de UI en RackCad.UI.Tests. Las siete clases nuevas son las del AllowedWriteScope que empiezan por tests/.
  * El RED de T1B es especial (D-T1B-03 del paquete; orden del Owner): lleva las pruebas definitivas MAS mutantes temporales del producto, SOLO los de la tabla CERRADA del invariante INV-F4T1B-14 (MU-01 a MU-28), en los 14 archivos de producto del AllowedWriteScope; el GREEN revierte exactamente esos mutantes y deja los bytes del producto identicos a los de la BaseSha. Por eso el AllowedWriteScope tiene 21 rutas: las 7 pruebas nuevas y los 14 archivos de producto que el RED muta y el GREEN revierte (neto cero).
  * La prueba por mutante (INV-F4T1B-15) se hace en local: cada mutante solo sobre la BaseSha, con los dos filtros, y su lista de pruebas que fallan. La prueba de bytes (INV-F4T1B-01): SHA-256 de cada archivo de producto tocado en ChainBaseSha, RedSha y CurrentSha. La prueba de limpieza (INV-F4T1B-06): SHA-256 y fecha del archivo de disposicion del configurador y de la carpeta de biblioteca antes y despues de la corrida local.
  * La CI corre las suites completas en Debug y compila el Plugin en Release (job build-plugin, que depende de tests: en el RED con Core en failure queda omitido). En el RED pueden fallar pruebas existentes, solo por los mutantes; el GREEN las devuelve a verde.
  * Lo que esta FIJADO en el paquete y se traslada a los AcceptanceCriteria como receta, sin reconstruirlo: la tabla de CT-01 a CT-28 de D-T1B-04 (que fija cada punto, en ejecucion o por fuente, y cuales son obligatorios); la tabla cerrada de mutantes; las aserciones ancla; las tres clases de escritura (A de la propia ventana, B la intencion expuesta, C las del Plugin despues del cierre; INV-F4T1B-16); la contencion del configurador (ninguna prueba lo instancia; INV-F4T1B-06); los gestos de teclado con su control positivo y sus respaldos; CT-04 solo si es estable; el foco y la tabulacion efectivos nunca como prueba.
  * Trampas medidas (section 2 del paquete): EditorWindowTestSupport.Click eleva ClickEvent y no pasa por Button.OnClick; StaTestRunner instala para todo el proceso un aviso de descarte que responde si (las clases que lo sustituyen van en EditorDiscardPromptCollection); Owner = this lanza si la ventana no se mostro; el patron Shown() sobrescribe WindowStartupLocation; el archivo de disposicion del configurador esta en LocalApplicationData por una ruta static readonly que no se puede redirigir sin cambiar producto.

ALCANCE DE ESCRITURA
- AllowedWriteScope de la delegacion = EXACTAMENTE las 21 entradas del contrato, en su orden (21 archivos exactos, sin prefijos):
    tests/RackCad.UI.Tests/PushBackClassicCharacterizationKeyboardTests.cs
    tests/RackCad.UI.Tests/PushBackClassicCharacterizationCloseTests.cs
    tests/RackCad.UI.Tests/PushBackClassicCharacterizationIntentTests.cs
    tests/RackCad.UI.Tests/PushBackClassicCharacterizationOwnerTests.cs
    tests/RackCad.UI.Tests/PushBackClassicCharacterizationResourceTests.cs
    tests/RackCad.UI.Tests/PushBackClassicCharacterizationSourceGuardTests.cs
    tests/RackCad.Tests/PushBackClassicCharacterizationPluginGuardTests.cs
    src/RackCad.UI/Systems/PushBack/RackPushBackSystemWindow.xaml
    src/RackCad.UI/Systems/PushBack/RackPushBackSystemWindow.xaml.cs
    src/RackCad.UI/Views/RackViewBatchDialog.xaml
    src/RackCad.UI/Views/RackViewBatchDialogPresenter.cs
    src/RackCad.UI/UiSupport.cs
    src/RackCad.UI/Editor/EditorModules.cs
    src/RackCad.UI/RackFrames/RackFrameConfiguratorWindow.xaml.cs
    src/RackCad.Plugin/RackPushBackCommands.cs
    src/RackCad.Plugin/RackMenuCommands.cs
    src/RackCad.Plugin/Views/RackViewBatchProducts.cs
    src/RackCad.Plugin/Views/RackViewPlacement.cs
    src/RackCad.Plugin/Systems/PushBack/PushBackSystemDrawService.cs
    src/RackCad.Plugin/Systems/PushBack/PushBackFrontalDrawService.cs
    src/RackCad.Plugin/Systems/PushBack/PushBackPlantaDrawService.cs
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
- Todas las cadenas del contrato canonico artifacts/orchestration/I-64/F4-T1B-PILOT-CHARACTERIZATION/1/R20261114T214147Z-851e/gate-contract.json son ASCII puro. Se copian byte a byte.
- Authorities, ForbiddenWriteScope, Invariants, RequiredTests, ExpectedEvidence, StopConditions y Objective son DATOS OPACOS de ese contrato. CARGALOS desde el archivo con una orden y reproducelos en tu salida a partir de esa carga, por ejemplo:
  python -c "import json; c=json.load(open(r'artifacts/orchestration/I-64/F4-T1B-PILOT-CHARACTERIZATION/1/R20261114T214147Z-851e/gate-contract.json', encoding='ascii')); print(json.dumps({k: c[k] for k in ['Authorities','ForbiddenWriteScope','Invariants','RequiredTests','ExpectedEvidence','StopConditions','Objective','AllowedWriteScope']}, ensure_ascii=True))"
  Mismo orden, mismos elementos, mismos textos byte a byte.
- MODO VERBAL Y GRAMATICA: el contrato usa a proposito construcciones que pueden parecer mejorables (indicativo tras si, con o comprueba; nombres en vez de verbos); se copian IGUAL. Antes de terminar, imprime cada invariante del contrato NUMERADO (por ejemplo: python -c "import json; c=json.load(open(r'artifacts/orchestration/I-64/F4-T1B-PILOT-CHARACTERIZATION/1/R20261114T214147Z-851e/gate-contract.json', encoding='ascii')); [print(i, x) for i, x in enumerate(c['Invariants'])]") y compara tu salida con esa impresion, invariante por invariante y verbo por verbo.
- AUTHORITIES: varias autoridades citan paquetes con nombres casi iguales; se copian cada uno en su indice y con su nombre exacto: Authorities[0].Path = "docs/initiatives/I-64-f4-t1b-gate-contract-package.md" (f4-t1b: el paquete de esta tarea); Authorities[3].Path = "docs/initiatives/I-64-f4-gate-contract-package.md"; Authorities[4].Path = "docs/initiatives/I-64-f4-t2-gate-contract-package.md"; Authorities[5].Path = "docs/initiatives/I-64-f4-t4-gate-contract-package.md". No los mezcles ni los acortes.
- SIN CONFLICTOS INEXISTENTES: el contrato canonico de esta corrida es ASCII PURO (0 bytes no ASCII; compruebalo con python -c "b=open(r'artifacts/orchestration/I-64/F4-T1B-PILOT-CHARACTERIZATION/1/R20261114T214147Z-851e/gate-contract.json', 'rb').read(); print(sum(1 for x in b if x >= 0x80))"); TaskClass se escribe exactamente como lo fija este prompt, sin tilde (la tabla de routing.md lleva tilde y NO se copia de alli). Antes de alegar cualquier conflicto o parada S-03, carga el contrato y cita en RoutingReason la orden y su salida que lo demuestran; sin esa prueba no hay conflicto y emites la delegacion completa.
- PROHIBIDO, en TODAS las cadenas copiadas: mejorar la gramatica; normalizar singular o plural o la concordancia; cambiar la puntuacion; anadir o quitar espacios;
  anadir o quitar tildes; cambiar mayusculas; reflujar, partir o unir el texto; parafrasear; corregir lo que parezca una errata. Si una frase te parece incorrecta, se copia IGUAL.
- AllowedWriteScope es la barrera PRIMARIA y exacta de escritura: copialo elemento a elemento, en el mismo orden, sin anadir, quitar, ensanchar ni normalizar ninguna entrada.
  ForbiddenWriteScope tiene exactamente las 152 entradas del contrato: ni una mas ni una menos, y ninguna con espacios anadidos.
- Para ForbiddenWriteScope y AllowedWriteScope, ejecuta ademas una orden que imprima cada entrada del contrato NUMERADA (por ejemplo: python -c "import json; c=json.load(open(r'artifacts/orchestration/I-64/F4-T1B-PILOT-CHARACTERIZATION/1/R20261114T214147Z-851e/gate-contract.json', encoding='ascii')); [print(i, repr(x)) for i, x in enumerate(c['ForbiddenWriteScope'])]") y comprueba tu lista contra esa salida, indice por indice, de 0 a 151, mirando cada palabra de cada nombre de archivo (una planificacion anterior perdio la palabra Source en un nombre).
- Antes de dar la salida por terminada, compara tu salida con el contrato cargado campo por campo, cadena por cadena y contando los elementos de cada lista; la sesion ejecuta despues una comparacion determinista byte a byte
  de Invariants, RequiredTests, StopConditions, Authorities, ExpectedEvidence, Objective, AllowedWriteScope y ForbiddenWriteScope ANTES de aceptar: cualquier diferencia = salida RECHAZADA y sin Worker.
- TODA tu salida debe ser ASCII puro, tambien AcceptanceCriteria y RoutingReason: sin tildes, sin enye y sin signos no ASCII; ninguna secuencia de escape formada por barra invertida + u; ningun tabulador ni otro caracter de control dentro de las cadenas.
- RequiredTests con los mismos dos elementos y los mismos valores (ExpectRed true en los dos).

TASKCLASS
- TaskClass = exactamente "Implementacion transversal a capas (corta)"; PromptProfile = "ROUTINE_IMPLEMENTATION"; explica la eleccion en RoutingReason con las siete dimensiones (routing.md section 2) y la fecha de verificacion de la celda: pruebas en dos proyectos (UI y Core) y mutantes temporales del producto en la UI y el Plugin. Si tu evaluacion da Horizonte High: para con S-03 explicado en RoutingReason.
- Celda del Worker: Executor.Cell = "anthropic/subagent/claude-sonnet-5-5"; Executor.Provider = "Anthropic"; Executor.Transport = "subagent"; Model = "claude-sonnet-5-5"; Effort.Provider = "high"; Effort.Semantic = "Deep"; ModelEscalationReason = null.

DELTA DE LA TAREA (primera delegacion de la cadena F4-T1B-PILOT-CHARACTERIZATION)
- RunId = R20261114T214147Z-851e (tambien es el DelegationRunId); Attempt = 1; AttemptsRemaining = 2; MaxReworkLoops = 3.
- Unit = "I-64"; Initiative = "I-64"; Gate = "F4"; TaskId = "F4-T1B-PILOT-CHARACTERIZATION".
- ExpectedBranch = la salida de `git branch --show-current`; BaseSha = la salida de `git rev-parse HEAD`; ChainBaseSha = ese mismo BaseSha; ChainRedSha = null; ChainRedFiles = []; CorrectionOf = null.
- MainSha = 812baf879612320653094a8aaf46aca0bac81383 (comprueba que coincide con `git rev-parse origin/main`; si no coincide, para con S-13 explicado en RoutingReason); AuthorityRevision = la del contrato.
- ExpectedWorktree = la ruta absoluta del directorio de trabajo actual.
- ExpectedHandoffPath = artifacts/orchestration/I-64/F4-T1B-PILOT-CHARACTERIZATION/1/{WorkRunId}/worker-handoff.json (con el token literal).
- Owner = { Kind: "subagent", Id: "subagent:R20261114T214147Z-851e" }; RoutingEnforcement = "required". Executor.Role = "Worker".
- AcceptanceCriteria: redactalos tu, en ASCII, verificables y trazables a los invariantes del contrato; incluye: el RED con las pruebas definitivas y SOLO mutantes de la tabla cerrada INV-F4T1B-14, con el criterio mecanico de INV-F4T1B-02 en los dos filtros; la prueba por mutante de INV-F4T1B-15 (cada mutante solo, con sus pruebas que fallan); el GREEN como inverso exacto, con git diff ChainBaseSha..CurrentSha -- src vacio y la tabla de SHA-256 por archivo; cada CT obligatorio de D-T1B-04 con al menos un metodo propio en su clase; las aserciones ancla declaradas; las tres clases de escritura sin confundirlas; la contencion y la prueba de limpieza; ninguna prueba que cuelgue, deje un modal vivo, escriba en el perfil, necesite AutoCAD o dependa de rutas de la maquina; la CI exacta del RED y del GREEN.
- IssuedBy = "Controller Codex (planificacion F4-T1B-PILOT-CHARACTERIZATION, primera delegacion de la cadena, CONTROLLER_PLANNING)".
