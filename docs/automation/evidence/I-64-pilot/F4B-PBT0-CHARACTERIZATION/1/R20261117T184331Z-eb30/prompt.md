I-64 - DELEGACION F4B-PBT0-CHARACTERIZATION - Controller, perfil CONTROLLER_PLANNING (primera delegacion de la cadena F4B-PBT0-CHARACTERIZATION)
Identidad: unidad I-64; gate F4B; tarea F4B-PBT0-CHARACTERIZATION; intento 1; RunId R20261117T184331Z-eb30;
           DelegationRunId R20261117T184331Z-eb30; AuthorityRevision ca4165bb8aa0a1949bd15c327ed2f082e0eb8cdb (la del contrato);
           MainSha 812baf879612320653094a8aaf46aca0bac81383; BaseSha = el HEAD actual del worktree (se espera f2eedd9a0d03c13edc0a92a0949d2c0b64bd78f4: commit de emision del contrato, hijo directo de la AuthorityRevision ca4165bb8aa0a1949bd15c327ed2f082e0eb8cdb:
           solo anade el contrato, su script generador, el estado y la evidencia);
           rama architecture/workspace-persistente-rackcad; worktree = el directorio de trabajo actual; Owner subagent:R20261117T184331Z-eb30
Contrato de gate: artifacts/orchestration/I-64/F4B-PBT0-CHARACTERIZATION/1/R20261117T184331Z-eb30/gate-contract.json (ASCII puro; custodiado en docs/automation/evidence/I-64-pilot/F4B-PBT0-CHARACTERIZATION/coordinator/gate-contract.json)
Autoridades (leer desde Git, no copiar su contenido): las del campo Authorities del contrato;
           vigencia: AuthorityRevision para la unidad (UNIT_DOC y UNIT_CHANGE), MainSha para lo demas (EXTERNAL)
Objetivo: emitir el paquete de delegacion (esquema rackcad-delegation/v1) del Worker para la tarea del contrato. Debe ser ejecutable tal cual.
Alcance: solo lectura. No escribas ningun archivo, no hagas commit ni push. No ejecutes dotnet, ni AutoCAD, ni builds.
Invariantes del Freeze: los 24 del contrato; una delegacion nunca amplia el contrato
Rutas: ver ALCANCE DE ESCRITURA abajo. Todo lo demas es de solo lectura.
           Fuente de diseno: docs/initiatives/I-64-f4b-pbt0-gate-contract-package.md v2.1 (section 2, hechos H-01 a H-21; section 3, decisiones D-PBT0-01 a D-PBT0-08; section 4, la lista cerrada CT-PB0-01 a CT-PB0-21 con sus definiciones comunes de 4.1 y las tablas T-1 a T-8 de 4.5; section 5, la tabla CERRADA de mutantes MU-PB0-01 a MU-PB0-20 y sus reglas; section 6, el contrato; section 7, riesgos y lecciones de T1B; section 9, cierre de las revisiones) y el paquete de la fase docs/initiatives/I-64-f4b-push-back-completo-package.md v3.1 (section 6.2)
Criterios de aceptacion: la salida valida contra el esquema; colecciones opacas del contrato copiadas byte a byte (ver COPIA EXACTA);
           AllowedWriteScope segun ALCANCE DE ESCRITURA; celda y effort entre las EligibleCells del contrato; TaskClass y campos de la DELTA exactamente como se indican
Evidencia requerida: ejecuta de verdad estas ordenes en el worktree y usa sus resultados:
           git branch --show-current ; git rev-parse HEAD ; git rev-parse origin/main ;
           git ls-files src/RackCad.UI/Systems/PushBack (10 archivos) ; git ls-files src/RackCad.Application/Systems/PushBack (64 archivos) ; git ls-files tests/RackCad.UI.Tests (188 archivos) ; git ls-files tests/RackCad.Tests (704 archivos) ;
           y lee el codigo, las pruebas y las guardas que cites
No-touch: todo el repositorio
Condiciones de parada: S-03, P-03 y las PBT0-C* del contrato; ante cualquiera, para y explica el bloqueo en RoutingReason.
COPIA: en las tareas anteriores de esta unidad, varias planificaciones se rechazaron solo por la copia de las colecciones opacas: un elemento de una lista omitido, un espacio anadido al principio de una cadena, una palabra perdida dentro de un nombre de archivo ("WorkspacePaletteHostGuardTests.cs" en vez de "WorkspacePaletteHostSourceGuardTests.cs") o un verbo cambiado de modo ("es" copiado como "sea"). Ninguna entrada de ninguna lista empieza ni termina con un espacio.
Una sola diferencia de un caracter o un solo elemento de mas o de menos basta para rechazar. Por la parada PBT0-C08 del contrato, un rechazo solo por la copia admite una sola replanificacion y un segundo va con STOP al Owner.
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

CONTEXTO (hechos versionados en el HEAD; evidencia section 231 y siguientes de docs/automation/evidence/I-64-evidence.md; F4B-E01 a F4B-E04 en docs/automation/evidence/I-64-pilot/F4B-DRAFTS/f4b-dispositions.md)
- F4 esta cerrada (CLOSED / PASS WITH LIMITATIONS). El Owner autorizo F4B (Push Back completo) y PB-T0 se emitio con attempts 1 de 3 por su autorizacion condicional (F4B-E04): caracterizar con pruebas nuevas el comportamiento observable ACTUAL del estado editable de la ventana clasica de Push Back (src/RackCad.UI/Systems/PushBack/RackPushBackSystemWindow.xaml.cs y .Composite.cs) y de las funciones Application de las que dependen PB-T1, PB-T2 y PB-T3. Es SOLO caracterizacion: "No product cleanup in PB-T0"; las rarezas medidas se fijan tal cual.
  Presupuesto: Attempt 1, AttemptsRemaining 2, MaxReworkLoops 3, CorrectionsAuthorized true; 1 invocacion de Worker por delegacion aceptada; un intento 3 vuelve siempre al Owner (PBT0-C12).
- HECHOS DEL REPOSITORIO medidos por la sesion en el HEAD (comprueba los que uses):
  * Los dos filtros (FullyQualifiedName~RackCad.Tests.PushBackFullEditorCharacterization y FullyQualifiedName~RackCad.UI.Tests.PushBackFullEditorCharacterization) seleccionan 0 pruebas en el HEAD: ningun nombre de clase empieza por PushBackFullEditor. Las clases de Core van en namespace RackCad.Tests y las de UI en RackCad.UI.Tests. Los diez archivos nuevos son las rutas del AllowedWriteScope que empiezan por tests/ (nueve clases de prueba y el ayudante internal static PushBackFullEditorCharacterizationSupport.cs, sin pruebas).
  * El RED es especial (D-PBT0-05 del paquete, mecanica de D-T1B-03 de T1B): lleva las pruebas definitivas MAS mutantes temporales del producto, SOLO los de la tabla CERRADA del invariante INV-PBT0-14 (MU-PB0-01 a MU-PB0-20), en los 6 archivos de producto del AllowedWriteScope (todos con mutantes de comportamiento); el GREEN revierte exactamente esos mutantes y deja los bytes del producto identicos a los de la BaseSha. Por eso el AllowedWriteScope tiene 16 rutas: los 10 archivos nuevos de prueba y los 6 archivos de producto que el RED muta y el GREEN revierte (neto cero).
  * La prueba por mutante (INV-PBT0-15) se hace en local: cada mutante solo sobre la BaseSha, con los dos filtros, y ademas la suite completa de Core si el mutante es de Application (MU-PB0-04, 05, 07, 09, 17 y 20); nunca la suite completa de UI en local (INV-PBT0-20). La prueba de bytes (INV-PBT0-01): SHA-256 de cada archivo de producto tocado en ChainBaseSha, RedSha y CurrentSha. La prueba de limpieza (INV-PBT0-06): SHA-256 y fecha del archivo de disposicion del configurador y de la carpeta de biblioteca antes y despues de las corridas locales.
  * La CI corre las suites completas en Debug y compila el Plugin en Release (job build-plugin, que depende de tests: en el RED con Core en failure queda omitido). En el RED pueden fallar pruebas existentes, solo por los mutantes; se miden como union en la CI del RED y se atribuyen por analisis declarado (T-8); el GREEN las devuelve a verde.
  * Lo que esta FIJADO en el paquete y se traslada a los AcceptanceCriteria como receta, sin reconstruirlo: la lista cerrada CT-PB0-01 a CT-PB0-21 de la section 4.2 (un CT = un metodo [Fact], sin Theory, D-PBT0-02); las definiciones comunes de la section 4.1 (Ventana(), Abrir, Salida, Sobre, Delta, las clases (a) y (b) y el filtro de D-PBT0-03); la tabla cerrada de mutantes y la regla de la excepcion (solo para los metodos de los dos filtros nuevos); las tablas T-1 a T-8 de la entrega; la lectura declarada del BOM (D-PBT0-06): leer PushBackEditorComputation.Bom no es ejecutarlo; ninguna ventana se muestra salvo en CT-PB0-14 (D-PBT0-08); ningun fixture nuevo y tests/RackCad.Tests/Fixtures/I64 en solo lectura (D-PBT0-01).
  * Sin AutoCAD: el bloque M de PB-T0 (medicion fisica) ya lo hizo la sesion aparte y no es parte de esta delegacion (INV-PBT0-24).

ALCANCE DE ESCRITURA
- AllowedWriteScope de la delegacion = EXACTAMENTE las 16 entradas del contrato, en su orden (16 archivos exactos, sin prefijos):
    tests/RackCad.UI.Tests/PushBackFullEditorCharacterizationStateTests.cs
    tests/RackCad.UI.Tests/PushBackFullEditorCharacterizationModuleTests.cs
    tests/RackCad.UI.Tests/PushBackFullEditorCharacterizationCompositeTests.cs
    tests/RackCad.UI.Tests/PushBackFullEditorCharacterizationSubdialogTests.cs
    tests/RackCad.UI.Tests/PushBackFullEditorCharacterizationCatalogTests.cs
    tests/RackCad.UI.Tests/PushBackFullEditorCharacterizationRoundingLossTests.cs
    tests/RackCad.UI.Tests/PushBackFullEditorCharacterizationSupport.cs
    tests/RackCad.Tests/PushBackFullEditorCharacterizationLateralCutTests.cs
    tests/RackCad.Tests/PushBackFullEditorCharacterizationDimensionStyleLossTests.cs
    tests/RackCad.Tests/PushBackFullEditorCharacterizationRoundingEffectTests.cs
    src/RackCad.UI/Systems/PushBack/RackPushBackSystemWindow.xaml.cs
    src/RackCad.UI/Systems/PushBack/RackPushBackSystemWindow.Composite.cs
    src/RackCad.Application/Systems/PushBack/PushBackCompositeEditorState.cs
    src/RackCad.Application/Systems/PushBack/PushBackEditorDesignAssembler.cs
    src/RackCad.Application/Systems/Shared/RackViewAvailability.cs
    src/RackCad.Application/Systems/Dynamic/DynamicViewDecorations.cs
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
- Todas las cadenas del contrato canonico artifacts/orchestration/I-64/F4B-PBT0-CHARACTERIZATION/1/R20261117T184331Z-eb30/gate-contract.json son ASCII puro. Se copian byte a byte.
- Authorities, ForbiddenWriteScope, Invariants, RequiredTests, ExpectedEvidence, StopConditions y Objective son DATOS OPACOS de ese contrato. CARGALOS desde el archivo con una orden y reproducelos en tu salida a partir de esa carga, por ejemplo:
  python -c "import json; c=json.load(open(r'artifacts/orchestration/I-64/F4B-PBT0-CHARACTERIZATION/1/R20261117T184331Z-eb30/gate-contract.json', encoding='ascii')); print(json.dumps({k: c[k] for k in ['Authorities','ForbiddenWriteScope','Invariants','RequiredTests','ExpectedEvidence','StopConditions','Objective','AllowedWriteScope']}, ensure_ascii=True, separators=(chr(44), chr(58))))"
  Mismo orden, mismos elementos, mismos textos byte a byte.
- MODO VERBAL Y GRAMATICA: el contrato usa a proposito construcciones que pueden parecer mejorables (indicativo tras si, con o comprueba; nombres en vez de verbos); se copian IGUAL. Antes de terminar, imprime cada invariante del contrato NUMERADO (por ejemplo: python -c "import json; c=json.load(open(r'artifacts/orchestration/I-64/F4B-PBT0-CHARACTERIZATION/1/R20261117T184331Z-eb30/gate-contract.json', encoding='ascii')); [print(i, x) for i, x in enumerate(c['Invariants'])]") y compara tu salida con esa impresion, invariante por invariante y verbo por verbo.
- AUTHORITIES: varias autoridades citan paquetes con nombres casi iguales; se copian cada una en su indice y con su nombre exacto: Authorities[0].Path = "docs/initiatives/I-64-f4b-pbt0-gate-contract-package.md"; Authorities[1].Path = "docs/initiatives/I-64-f4b-push-back-completo-package.md"; Authorities[8].Path = "docs/initiatives/I-64-f4-t1b-gate-contract-package.md". No los mezcles ni los acortes.
- SIN CONFLICTOS INEXISTENTES: el contrato canonico de esta corrida es ASCII PURO (0 bytes no ASCII; compruebalo con python -c "b=open(r'artifacts/orchestration/I-64/F4B-PBT0-CHARACTERIZATION/1/R20261117T184331Z-eb30/gate-contract.json', 'rb').read(); print(sum(1 for x in b if x >= 0x80))"); TaskClass se escribe exactamente como lo fija este prompt, sin tilde (la tabla de routing.md lleva tilde y NO se copia de alli). Antes de alegar cualquier conflicto o parada S-03, carga el contrato y cita en RoutingReason la orden y su salida que lo demuestran; sin esa prueba no hay conflicto y emites la delegacion completa.
- PROHIBIDO, en TODAS las cadenas copiadas: mejorar la gramatica; normalizar singular o plural o la concordancia; cambiar la puntuacion; anadir o quitar espacios;
  anadir o quitar tildes; cambiar mayusculas; reflujar, partir o unir el texto; parafrasear; corregir lo que parezca una errata. Si una frase te parece incorrecta, se copia IGUAL.
- AllowedWriteScope es la barrera PRIMARIA y exacta de escritura: copialo elemento a elemento, en el mismo orden, sin anadir, quitar, ensanchar ni normalizar ninguna entrada.
  ForbiddenWriteScope tiene exactamente las 265 entradas del contrato: ni una mas ni una menos, y ninguna con espacios anadidos.
- FORBIDDENWRITESCOPE, COPIA REFORZADA DESDE EL PRIMER INTENTO: tiene exactamente 265 entradas, de la 0 a la 264, todas distintas. Para copiarla, ejecuta esta orden, que la imprime SOLA como un array JSON compacto en una linea, sin ningun espacio entre los elementos, y reproduce cada cadena tal cual, de una vez, sin escribir las entradas de memoria:
  python -c "import json; c=json.load(open(r'artifacts/orchestration/I-64/F4B-PBT0-CHARACTERIZATION/1/R20261117T184331Z-eb30/gate-contract.json', encoding='ascii')); print(json.dumps(c['ForbiddenWriteScope'], ensure_ascii=True, separators=(chr(44), chr(58))))"
  Puntos de control, comprobados contra la impresion numerada: la 0 es src/RackCad.Application/BlockNaming.cs; de la 0 a la 154, archivos y subraices de src/RackCad.Application/ (muchos archivos sueltos de Systems/Dynamic/, Systems/PushBack/ y Systems/Shared/, uno por uno); la 155 es src/RackCad.Domain/ y la 156 src/RackCad.Plugin/; de la 157 a la 212, src/RackCad.UI/ (la 157 es src/RackCad.UI/CatalogOption.cs y, dentro, los 8 archivos de src/RackCad.UI/Systems/PushBack/ que no se tocan, de la 200 a la 207); la 213 es tests/RackCad.Tests/Fixtures/ y de la 213 a la 247, prefijos y archivos de tests/ (la 247 es tests/RackCad.Tests/CallerOwnedFacadeGuardTests.cs); de la 248 a la 264, la raiz (la 248 es docs/ y la 264, la ULTIMA de la lista, .gitignore).
  Ahi termina la lista: nunca vuelvas atras ni repitas un nombre.
  Antes de terminar, cuenta los elementos de tu ForbiddenWriteScope (deben ser 265), comprueba que no hay repetidos, compara cada indice con la impresion numerada y comprueba que ninguna entrada empieza ni termina con un espacio.
- Para ForbiddenWriteScope y AllowedWriteScope, ejecuta ademas una orden que imprima cada entrada del contrato NUMERADA (por ejemplo: python -c "import json; c=json.load(open(r'artifacts/orchestration/I-64/F4B-PBT0-CHARACTERIZATION/1/R20261117T184331Z-eb30/gate-contract.json', encoding='ascii')); [print(i, repr(x)) for i, x in enumerate(c['ForbiddenWriteScope'])]") y comprueba tu lista contra esa salida, indice por indice, de 0 a 264, mirando cada palabra de cada nombre de archivo (una planificacion anterior perdio la palabra Source en un nombre).
- STOPCONDITIONS: la PBT0-C11 es larga a proposito (las firmas literales de PD-F4-01 y PD-F4-04 y la regla de coincidencia); copiala entera desde la carga, sin resumirla ni partirla. P-01 nombra la Snapshot I.
- Antes de dar la salida por terminada, compara tu salida con el contrato cargado campo por campo, cadena por cadena y contando los elementos de cada lista; la sesion ejecuta despues una comparacion determinista byte a byte
  de Invariants, RequiredTests, StopConditions, Authorities, ExpectedEvidence, Objective, AllowedWriteScope y ForbiddenWriteScope ANTES de aceptar: cualquier diferencia = salida RECHAZADA y sin Worker.
- TODA tu salida debe ser ASCII puro, tambien AcceptanceCriteria y RoutingReason: sin tildes, sin enye y sin signos no ASCII; ninguna secuencia de escape formada por barra invertida + u; ningun tabulador ni otro caracter de control dentro de las cadenas.
- RequiredTests con los mismos dos elementos, en el mismo orden y con los mismos valores (MinSelected 3 y 18; ExpectRed true en los dos).

TASKCLASS
- TaskClass = exactamente "Implementacion transversal a capas (corta)"; PromptProfile = "ROUTINE_IMPLEMENTATION"; explica la eleccion en RoutingReason con las siete dimensiones (routing.md section 2) y la fecha de verificacion de la celda: pruebas en dos proyectos (UI y Core) y mutantes temporales del producto en la UI y en Application. Si tu evaluacion da Horizonte High: para con S-03 explicado en RoutingReason.
- Celda del Worker: Executor.Cell = "anthropic/subagent/claude-sonnet-5-5"; Executor.Provider = "Anthropic"; Executor.Transport = "subagent"; Model = "claude-sonnet-5-5"; Effort.Provider = "high"; Effort.Semantic = "Deep"; ModelEscalationReason = null.

DELTA DE LA TAREA (primera delegacion de la cadena F4B-PBT0-CHARACTERIZATION)
- RunId = R20261117T184331Z-eb30 (tambien es el DelegationRunId); Attempt = 1; AttemptsRemaining = 2; MaxReworkLoops = 3.
- Unit = "I-64"; Initiative = "I-64"; Gate = "F4B"; TaskId = "F4B-PBT0-CHARACTERIZATION".
- ExpectedBranch = la salida de `git branch --show-current`; BaseSha = la salida de `git rev-parse HEAD`; ChainBaseSha = ese mismo BaseSha; ChainRedSha = null; ChainRedFiles = []; CorrectionOf = null.
- MainSha = 812baf879612320653094a8aaf46aca0bac81383 (comprueba que coincide con `git rev-parse origin/main`; si no coincide, para con S-13 explicado en RoutingReason); AuthorityRevision = la del contrato.
- ExpectedWorktree = la ruta absoluta del directorio de trabajo actual.
- ExpectedHandoffPath = artifacts/orchestration/I-64/F4B-PBT0-CHARACTERIZATION/1/{WorkRunId}/worker-handoff.json (con el token literal).
- Owner = { Kind: "subagent", Id: "subagent:R20261117T184331Z-eb30" }; RoutingEnforcement = "required". Executor.Role = "Worker".
- AcceptanceCriteria: redactalos tu, en ASCII, verificables y trazables a los invariantes del contrato; incluye: el RED con las pruebas definitivas y SOLO mutantes de la tabla cerrada INV-PBT0-14, con el criterio mecanico de INV-PBT0-02 en los dos filtros; la prueba por mutante de INV-PBT0-15 (cada mutante solo, con sus pruebas que fallan, y la suite completa de Core para los de Application); el GREEN como inverso exacto, con git diff ChainBaseSha..CurrentSha -- src vacio y la tabla de SHA-256 por archivo; cada CT de CT-PB0-01 a CT-PB0-21 con su metodo propio en su clase; las anclas declaradas (T-6); las tablas T-1 a T-8; solo la clase de escritura (B) (INV-PBT0-16); la contencion y la prueba de limpieza; ninguna prueba que cuelgue, deje un modal vivo, escriba en el perfil, necesite AutoCAD o dependa de rutas de la maquina; la CI exacta del RED y del GREEN.
- IssuedBy = "Controller Codex (planificacion F4B-PBT0-CHARACTERIZATION, primera delegacion de la cadena, CONTROLLER_PLANNING)".
