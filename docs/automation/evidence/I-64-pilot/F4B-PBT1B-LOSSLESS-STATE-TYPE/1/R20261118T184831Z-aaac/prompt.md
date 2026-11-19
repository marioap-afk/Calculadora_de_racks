I-64 - DELEGACION F4B-PBT1B-LOSSLESS-STATE-TYPE - Controller, perfil CONTROLLER_PLANNING (primera delegacion de la cadena F4B-PBT1B-LOSSLESS-STATE-TYPE)
Identidad: unidad I-64; gate F4B; tarea F4B-PBT1B-LOSSLESS-STATE-TYPE; intento 1; RunId R20261118T184831Z-aaac;
           DelegationRunId R20261118T184831Z-aaac; AuthorityRevision 1464193f899b4678d1e4a2c4885a30cee19875ff (la del contrato);
           MainSha 812baf879612320653094a8aaf46aca0bac81383; BaseSha = el HEAD actual del worktree (se espera 7905b3a95311aa4de172c9ab89b5825ab57dd280: commit de emision del contrato, hijo directo de la AuthorityRevision 1464193f899b4678d1e4a2c4885a30cee19875ff:
           solo anade el contrato, su script generador, el estado y la evidencia);
           rama architecture/workspace-persistente-rackcad; worktree = el directorio de trabajo actual; Owner subagent:R20261118T184831Z-aaac
Contrato de gate: artifacts/orchestration/I-64/F4B-PBT1B-LOSSLESS-STATE-TYPE/1/R20261118T184831Z-aaac/gate-contract.json (ASCII puro; custodiado en docs/automation/evidence/I-64-pilot/F4B-PBT1B-LOSSLESS-STATE-TYPE/coordinator/gate-contract.json)
Autoridades (leer desde Git, no copiar su contenido): las del campo Authorities del contrato;
           vigencia: AuthorityRevision para la unidad (UNIT_DOC y UNIT_CHANGE), MainSha para lo demas (EXTERNAL)
Objetivo: emitir el paquete de delegacion (esquema rackcad-delegation/v1) del Worker para la tarea del contrato. Debe ser ejecutable tal cual.
Alcance: solo lectura. No escribas ningun archivo, no hagas commit ni push. No ejecutes dotnet, ni AutoCAD, ni builds.
Invariantes del Freeze: los 15 del contrato; una delegacion nunca amplia el contrato
Rutas: ver ALCANCE DE ESCRITURA abajo. Todo lo demas es de solo lectura.
           Fuente de diseno: docs/initiatives/I-64-f4b-pbt1-gate-contract-package.md v2.6 (section 2, hechos medidos, con los anclajes de PB-T1b sobre el cierre de PB-T1a 5043b49a; section 3, D-PBT1-02 a D-PBT1-11 con D-PBT1-09 (c-02) y D-PBT1-09b (c-09), D-PBT1-14 y D-PBT1-15 con la decision D2 del Owner, sus diez condiciones, la guarda G-L1 y su limite; section 5.0, las recetas y el ayudante Pasada; section 5.3, el regimen de PB-T1b, el esqueleto exacto, las clases y la lista cerrada CT-PBT1B-01 a CT-PBT1B-17; section 6.0 y 6.2, el contrato; section 7, riesgos; section 8, las decisiones del Owner; section 10, los arrastres), con su registro de mediciones docs/initiatives/I-64-f4b-pbt1-measurements.md (apartado P), las decisiones literales del Owner (docs/automation/evidence/I-64-pilot/F4B-DRAFTS/coordinator/owner-pbt1-pre-decisions.md, owner-d1-d2-decisions.md y owner-pbt1a-decisions.md) y la red de PB-T1a (tests/RackCad.UI.Tests/PushBackStateNet*.cs) como oraculo de la ventana
Criterios de aceptacion: la salida valida contra el esquema; colecciones opacas del contrato copiadas byte a byte (ver COPIA EXACTA);
           AllowedWriteScope segun ALCANCE DE ESCRITURA; celda y effort entre las EligibleCells del contrato; TaskClass y campos de la DELTA exactamente como se indican
Evidencia requerida: ejecuta de verdad estas ordenes en el worktree y usa sus resultados:
           git branch --show-current ; git rev-parse HEAD ; git rev-parse origin/main ;
           git ls-files src/RackCad.UI/Systems/PushBack (10 archivos) ; git ls-files src/RackCad.Application/Systems/PushBack (64 archivos) ; git ls-files tests/RackCad.UI.Tests (210 archivos) ; git ls-files tests/RackCad.Tests (707 archivos) ;
           y lee el codigo, las pruebas y las guardas que cites
No-touch: todo el repositorio
Condiciones de parada: S-03, P-03 y las PBT1B-C* del contrato; ante cualquiera, para y explica el bloqueo en RoutingReason.
COPIA: en las tareas anteriores de esta unidad, varias planificaciones se rechazaron solo por la copia de las colecciones opacas: un elemento de una lista omitido, un espacio anadido al principio de una cadena, una palabra perdida dentro de un nombre de archivo ("WorkspacePaletteHostGuardTests.cs" en vez de "WorkspacePaletteHostSourceGuardTests.cs") o un verbo cambiado de modo ("es" copiado como "sea"). Ninguna entrada de ninguna lista empieza ni termina con un espacio.
Una sola diferencia de un caracter o un solo elemento de mas o de menos basta para rechazar. Por la parada PBT1B-C08 del contrato, un rechazo solo por la copia admite una sola replanificacion y un segundo va con STOP al Owner.
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

CONTEXTO (hechos versionados en el HEAD; evidencia section 231 y siguientes de docs/automation/evidence/I-64-evidence.md; F4B-E01 a F4B-E17 en docs/automation/evidence/I-64-pilot/F4B-DRAFTS/f4b-dispositions.md)
- F4 esta cerrada. En F4B, PB-T0, PB-TC, PB-TC2 y PB-T1a estan CLOSED / PASS WITH LIMITATIONS (F4B-E05, F4B-E08, F4B-E12 y F4B-E16). PB-T1 (sacar de la ventana WPF el estado editable de Push Back sin perdidas) se divide en tres subtareas secuenciales (D-PBT1-01); esta es la segunda, PB-T1b: crear en Application PushBackEditableState, su comparador, su mapa de propiedad y PushBackEditorOutputGate, con las reglas explicitas de c-02 y de c-09 (F4B-E09 y F4B-E14), la copia con la excepcion de dos referencias compartidas de D2 y sus guardas. La ventana NO cambia: eso es de PB-T1c. La decision D2 del Owner (F4B-E11) y su limite van como la parada PBT1B-C17. Emitida por F4B-E17.
  Presupuesto: Attempt 1, AttemptsRemaining 2, MaxReworkLoops 3, CorrectionsAuthorized true; 1 invocacion de Worker por delegacion aceptada; un intento 3 vuelve siempre al Owner (PBT1B-C12).
- HECHOS DEL REPOSITORIO medidos por la sesion en el HEAD (comprueba los que uses):
  * Los filtros con ExpectRed true (FullyQualifiedName~RackCad.Tests.PushBackEditableState, FullyQualifiedName~RackCad.Tests.PushBackEditorOutputGate y FullyQualifiedName~RackCad.UI.Tests.PushBackEditableStateParityTests) seleccionan 0 pruebas en el HEAD. Las cinco clases de Core van en namespace RackCad.Tests y la de UI en namespace RackCad.UI.Tests, en EditorDiscardPromptCollection.
  * El RED es el ESQUELETO INERTE (regimen de F4-T2A; section 5.3): el esqueleto exacto de la section 5.3 en los archivos de src/ del AllowedWriteScope (los cuatro nuevos de Application y los miembros nuevos de ES y RMES) MAS las seis clases de prueba; cada metodo de un filtro con ExpectRed falla por asercion sobre el esqueleto, salvo las dos guardas de D2 (CT-PBT1B-14 y 15, filtro PushBackSharedReferenceGuardTests, ExpectRed false), que pasan en el RED y discriminan por sus autopruebas en negativo. El GREEN implementa solo src/ del AllowedWriteScope y no toca tests/. La ventana, el piloto, el Plugin, CS y el XAML no cambian.
  * Antes del RED (paso 1, INV-PBT1B-06): una corrida TEMPORAL de UI sobre la BaseSha, fuera de todo commit, con Assert.Fail, mide las filas (a) a (e) de c-02 y la fila (f) de c-09; las filas (b) y (f) fijan la rama de cada regla; cualquier otra linea es PBT1B-C14 (STOP antes del RED).
  * La CI corre las suites completas en Debug y compila el Plugin en Release. En el RED se espera tests y ui-tests en failure, build-ui en success y build-plugin omitido por su needs; ninguna prueba existente falla en el RED. GREEN previsto: Core 19 086 y UI 2 437 (section 6.2).
  * Lo que esta FIJADO en el paquete y se traslada a los AcceptanceCriteria como receta, sin reconstruirlo: la lista cerrada CT-PBT1B-01 a 17 con su reparto en las clases; el esqueleto exacto; la API exacta de D-PBT1-02 a D-PBT1-11 con D-PBT1-09b (INV-PBT1B-03); las reglas y las autopruebas de G-L1 de D-PBT1-15; las lineas literales de CT-PBT1B-12, 15 y 17; el Worker no ajusta lo esperado (PBT1B-C07).
  * D2 (F4B-E11): solo la linea base resuelta y el informe de modulos de cada lado se comparten por referencia (las diez condiciones de D-PBT1-15); si hiciera falta mutarlos dentro de la vida de la edicion, o compartir otro objeto mutable: PBT1B-C17, STOP y se reabre L-1.
  * Sin AutoCAD.

ALCANCE DE ESCRITURA
- AllowedWriteScope de la delegacion = EXACTAMENTE las 12 entradas del contrato, en su orden (12 archivos exactos, sin prefijos):
    src/RackCad.Application/Systems/PushBack/PushBackEditorState.cs
    src/RackCad.Application/Systems/Shared/RackModuleEditSession.cs
    src/RackCad.Application/Systems/PushBack/PushBackEditableState.cs
    src/RackCad.Application/Systems/PushBack/PushBackEditableStateComparer.cs
    src/RackCad.Application/Systems/PushBack/PushBackEditableStateOwnership.cs
    src/RackCad.Application/Systems/PushBack/PushBackEditorOutputGate.cs
    tests/RackCad.Tests/PushBackEditableStateLoadTests.cs
    tests/RackCad.Tests/PushBackEditableStateCopyTests.cs
    tests/RackCad.Tests/PushBackEditableStateOwnershipTests.cs
    tests/RackCad.Tests/PushBackEditorOutputGateTests.cs
    tests/RackCad.Tests/PushBackSharedReferenceGuardTests.cs
    tests/RackCad.UI.Tests/PushBackEditableStateParityTests.cs
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
- Todas las cadenas del contrato canonico artifacts/orchestration/I-64/F4B-PBT1B-LOSSLESS-STATE-TYPE/1/R20261118T184831Z-aaac/gate-contract.json son ASCII puro. Se copian byte a byte.
- Authorities, ForbiddenWriteScope, Invariants, RequiredTests, ExpectedEvidence, StopConditions y Objective son DATOS OPACOS de ese contrato. CARGALOS desde el archivo con una orden y reproducelos en tu salida a partir de esa carga, por ejemplo:
  python -c "import json; c=json.load(open(r'artifacts/orchestration/I-64/F4B-PBT1B-LOSSLESS-STATE-TYPE/1/R20261118T184831Z-aaac/gate-contract.json', encoding='ascii')); print(json.dumps({k: c[k] for k in ['Authorities','ForbiddenWriteScope','Invariants','RequiredTests','ExpectedEvidence','StopConditions','Objective','AllowedWriteScope']}, ensure_ascii=True, separators=(chr(44), chr(58))))"
  Mismo orden, mismos elementos, mismos textos byte a byte.
- MODO VERBAL Y GRAMATICA: el contrato usa a proposito construcciones que pueden parecer mejorables (indicativo tras si, con o comprueba; nombres en vez de verbos); se copian IGUAL. Antes de terminar, imprime cada invariante del contrato NUMERADO (por ejemplo: python -c "import json; c=json.load(open(r'artifacts/orchestration/I-64/F4B-PBT1B-LOSSLESS-STATE-TYPE/1/R20261118T184831Z-aaac/gate-contract.json', encoding='ascii')); [print(i, x) for i, x in enumerate(c['Invariants'])]") y compara tu salida con esa impresion, invariante por invariante y verbo por verbo.
- AUTHORITIES: varias autoridades citan paquetes con nombres casi iguales; se copian cada una en su indice y con su nombre exacto: Authorities[0].Path = "docs/initiatives/I-64-f4b-pbt1-gate-contract-package.md"; Authorities[1].Path = "docs/initiatives/I-64-f4b-pbt1-measurements.md"; Authorities[4].Path = "docs/initiatives/I-64-f4b-push-back-completo-package.md"; Authorities[21].Path = "docs/initiatives/I-64-f4b-pbt0-gate-contract-package.md"; Authorities[24].Path = "docs/initiatives/I-64-f4b-pbtc-gate-contract-package.md"; Authorities[26].Path = "docs/initiatives/I-64-f4b-pbtc2-gate-contract-package.md"; Authorities[27].Path = "docs/initiatives/I-64-f4b-pbtc2-measurements.md"; Authorities[32].Path = "docs/initiatives/I-64-f4-t1b-gate-contract-package.md"; Authorities[33].Path = "docs/initiatives/I-64-f4-t2-gate-contract-package.md"; Authorities[34].Path = "docs/initiatives/I-64-f4-t3-gate-contract-package.md"; Authorities[35].Path = "docs/initiatives/I-64-f4-t3c-gate-contract-package.md". No los mezcles ni los acortes.
- SIN CONFLICTOS INEXISTENTES: el contrato canonico de esta corrida es ASCII PURO (0 bytes no ASCII; compruebalo con python -c "b=open(r'artifacts/orchestration/I-64/F4B-PBT1B-LOSSLESS-STATE-TYPE/1/R20261118T184831Z-aaac/gate-contract.json', 'rb').read(); print(sum(1 for x in b if x >= 0x80))"); TaskClass se escribe exactamente como lo fija este prompt, sin tilde (la tabla de routing.md lleva tilde y NO se copia de alli). Antes de alegar cualquier conflicto o parada S-03, carga el contrato y cita en RoutingReason la orden y su salida que lo demuestran; sin esa prueba no hay conflicto y emites la delegacion completa.
- PROHIBIDO, en TODAS las cadenas copiadas: mejorar la gramatica; normalizar singular o plural o la concordancia; cambiar la puntuacion; anadir o quitar espacios;
  anadir o quitar tildes; cambiar mayusculas; reflujar, partir o unir el texto; parafrasear; corregir lo que parezca una errata. Si una frase te parece incorrecta, se copia IGUAL.
- AllowedWriteScope es la barrera PRIMARIA y exacta de escritura: copialo elemento a elemento, en el mismo orden, sin anadir, quitar, ensanchar ni normalizar ninguna entrada.
  ForbiddenWriteScope tiene exactamente las 213 entradas del contrato: ni una mas ni una menos, y ninguna con espacios anadidos.
- FORBIDDENWRITESCOPE, COPIA REFORZADA DESDE EL PRIMER INTENTO: tiene exactamente 213 entradas, de la 0 a la 212, todas distintas. Para copiarla, ejecuta esta orden, que la imprime SOLA como un array JSON compacto en una linea, sin ningun espacio entre los elementos, y reproduce cada cadena tal cual, de una vez, sin escribir las entradas de memoria:
  python -c "import json; c=json.load(open(r'artifacts/orchestration/I-64/F4B-PBT1B-LOSSLESS-STATE-TYPE/1/R20261118T184831Z-aaac/gate-contract.json', encoding='ascii')); print(json.dumps(c['ForbiddenWriteScope'], ensure_ascii=True, separators=(chr(44), chr(58))))"
  Puntos de control, comprobados contra la impresion numerada: la 0 es src/RackCad.Application/BlockNaming.cs; de la 0 a la 116, prefijos y archivos de src/ (dentro, los 52 archivos o prefijos de src/RackCad.Application/Systems/PushBack/ que no se tocan, de la 22 a la 73; la 116 es src/RackCad.UI/, entera); de la 117 a la 195, prefijos y archivos de tests/ (la 117 es tests/RackCad.Tests/Fixtures/); de la 196 a la 212, la raiz (la 196 es docs/ y la 212, la ULTIMA de la lista, .gitignore).
  Ahi termina la lista: nunca vuelvas atras ni repitas un nombre.
  Antes de terminar, cuenta los elementos de tu ForbiddenWriteScope (deben ser 213), comprueba que no hay repetidos, compara cada indice con la impresion numerada y comprueba que ninguna entrada empieza ni termina con un espacio.
- Para ForbiddenWriteScope y AllowedWriteScope, ejecuta ademas una orden que imprima cada entrada del contrato NUMERADA (por ejemplo: python -c "import json; c=json.load(open(r'artifacts/orchestration/I-64/F4B-PBT1B-LOSSLESS-STATE-TYPE/1/R20261118T184831Z-aaac/gate-contract.json', encoding='ascii')); [print(i, repr(x)) for i, x in enumerate(c['ForbiddenWriteScope'])]") y comprueba tu lista contra esa salida, indice por indice, de 0 a 212, mirando cada palabra de cada nombre de archivo (una planificacion anterior perdio la palabra Source en un nombre).
- STOPCONDITIONS: la PBT1B-C11 es larga a proposito (las firmas literales de PD-F4-01 y PD-F4-04 y la regla de coincidencia); copiala entera desde la carga, sin resumirla ni partirla. P-01 nombra la Snapshot I.
- Antes de dar la salida por terminada, compara tu salida con el contrato cargado campo por campo, cadena por cadena y contando los elementos de cada lista; la sesion ejecuta despues una comparacion determinista byte a byte
  de Invariants, RequiredTests, StopConditions, Authorities, ExpectedEvidence, Objective, AllowedWriteScope y ForbiddenWriteScope ANTES de aceptar: cualquier diferencia = salida RECHAZADA y sin Worker.
- TODA tu salida debe ser ASCII puro, tambien AcceptanceCriteria y RoutingReason: sin tildes, sin enye y sin signos no ASCII; ninguna secuencia de escape formada por barra invertida + u; ningun tabulador ni otro caracter de control dentro de las cadenas.
- RequiredTests con los mismos 23 elementos, en el mismo orden y con los mismos valores (MinSelected 13, 1, 1, 2, 3271, 8, 9, 15, 25, 7, 17, 14, 55, 10, 16, 3, 3, 2, 2, 2, 1, 4, 67; ExpectRed true solo en los tres primeros).

TASKCLASS
- TaskClass = exactamente "Implementacion transversal a capas (corta)"; PromptProfile = "ROUTINE_IMPLEMENTATION"; explica la eleccion en RoutingReason con las siete dimensiones (routing.md section 2) y la fecha de verificacion de la celda: un tipo nuevo de Application con su copiador, su comparador, su mapa y su barrera de salida, pruebas nuevas en Core y en UI, un esqueleto inerte en el RED y una medicion temporal previa (paso 1). Si tu evaluacion da Horizonte High: para con S-03 explicado en RoutingReason.
- Celda del Worker: Executor.Cell = "anthropic/subagent/claude-sonnet-5-5"; Executor.Provider = "Anthropic"; Executor.Transport = "subagent"; Model = "claude-sonnet-5-5"; Effort.Provider = "high"; Effort.Semantic = "Deep"; ModelEscalationReason = null.

DELTA DE LA TAREA (primera delegacion de la cadena F4B-PBT1B-LOSSLESS-STATE-TYPE)
- RunId = R20261118T184831Z-aaac (tambien es el DelegationRunId); Attempt = 1; AttemptsRemaining = 2; MaxReworkLoops = 3.
- Unit = "I-64"; Initiative = "I-64"; Gate = "F4B"; TaskId = "F4B-PBT1B-LOSSLESS-STATE-TYPE".
- ExpectedBranch = la salida de `git branch --show-current`; BaseSha = la salida de `git rev-parse HEAD`; ChainBaseSha = ese mismo BaseSha; ChainRedSha = null; ChainRedFiles = []; CorrectionOf = null.
- MainSha = 812baf879612320653094a8aaf46aca0bac81383 (comprueba que coincide con `git rev-parse origin/main`; si no coincide, para con S-13 explicado en RoutingReason); AuthorityRevision = la del contrato.
- ExpectedWorktree = la ruta absoluta del directorio de trabajo actual.
- ExpectedHandoffPath = artifacts/orchestration/I-64/F4B-PBT1B-LOSSLESS-STATE-TYPE/1/{WorkRunId}/worker-handoff.json (con el token literal).
- Owner = { Kind: "subagent", Id: "subagent:R20261118T184831Z-aaac" }; RoutingEnforcement = "required". Executor.Role = "Worker".
- AcceptanceCriteria: redactalos tu, en ASCII, verificables y trazables a los invariantes del contrato; incluye: el paso 1 con las filas (a) a (f) y las dos ramas declaradas (PBT1B-C14); el RED con el esqueleto exacto de la section 5.3 y las seis clases, con el criterio mecanico de INV-PBT1B-02 en cada filtro con ExpectRed y las dos guardas de D2 en verde con sus autopruebas en negativo; el GREEN solo en src/ del AllowedWriteScope con la API exacta (INV-PBT1B-03) y sin campos nuevos (INV-PBT1B-04); cada CT de CT-PBT1B-01 a 17 con su metodo en su clase y sus lineas esperadas sin ajustarlas (PBT1B-C07); D2 (INV-PBT1B-07c y PBT1B-C17); PBT1B-C15 si la paridad o la ida y vuelta dan una diferencia que no sale del esqueleto; la contencion de las corridas locales y la prueba de limpieza; ninguna prueba que cuelgue, deje un modal vivo, escriba en el perfil, necesite AutoCAD o dependa de rutas de la maquina; la CI exacta del RED y del GREEN.
- IssuedBy = "Controller Codex (planificacion F4B-PBT1B-LOSSLESS-STATE-TYPE, primera delegacion de la cadena, CONTROLLER_PLANNING)".
