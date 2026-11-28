I-64 - DELEGACION F4-T4A2-PREPARE-UI - Controller, perfil CONTROLLER_PLANNING (primera delegacion de la cadena F4-T4A2-PREPARE-UI)
Identidad: unidad I-64; gate F4; tarea F4-T4A2-PREPARE-UI; intento 1; RunId R20261114T144612Z-2d7d;
           DelegationRunId R20261114T144612Z-2d7d; AuthorityRevision 509622ffc170717be8a2f947180cd812bf51a347 (la del contrato);
           MainSha 812baf879612320653094a8aaf46aca0bac81383; BaseSha = el HEAD actual del worktree (se espera 4eb7e9247c3ad942feea1b6a695d7ad79377a4ee: commit de emision del contrato, hijo directo de la AuthorityRevision 509622ffc170717be8a2f947180cd812bf51a347:
           solo anade el contrato, el estado y la evidencia);
           rama architecture/workspace-persistente-rackcad; worktree = el directorio de trabajo actual; Owner subagent:R20261114T144612Z-2d7d
Contrato de gate: artifacts/orchestration/I-64/F4-T4A2-PREPARE-UI/1/R20261114T144612Z-2d7d/gate-contract.json (ASCII puro; custodiado en docs/automation/evidence/I-64-pilot/F4-T4A2-PREPARE-UI/coordinator/gate-contract.json)
Autoridades (leer desde Git, no copiar su contenido): las del campo Authorities del contrato;
           vigencia: AuthorityRevision para la unidad (UNIT_DOC y UNIT_CHANGE), MainSha para lo demas (EXTERNAL)
Objetivo: emitir el paquete de delegacion (esquema rackcad-delegation/v1) del Worker para la tarea del contrato. Debe ser ejecutable tal cual.
Alcance: solo lectura. No escribas ningun archivo, no hagas commit ni push. No ejecutes dotnet, ni AutoCAD, ni builds.
Invariantes del Freeze: los 17 del contrato; una delegacion nunca amplia el contrato
Rutas: ver ALCANCE DE ESCRITURA abajo. Todo lo demas es de solo lectura.
           Fuente de diseno: docs/initiatives/I-64-f4-t4a2-gate-contract-package.md (section 2, hechos H-01 a H-21; section 3, las clases del Owner sobre los nombres existentes, con las dos tablas de TryConfirmBase y TryCheckNow; section 4, decisiones D-T4A2-01 a D-T4A2-11 con la tabla cerrada T-A2 de textos, los prefijos y los botones; section 5, trazabilidad; section 6, contrato propuesto; section 7, escenarios de Smoke-2; section 10, disposicion de la revision); docs/initiatives/I-64-f4-t4-gate-contract-package.md (D-T4A1-01 a D-T4A1-03 y R-A2-01 a R-A2-06); docs/initiatives/I-64-f4-t3-gate-contract-package.md (la tabla de transiciones); docs/initiatives/I-64-proposal-v5.md (D-10 a D-14, INV-31); IC-07; las ordenes del Owner owner-f4t4a2-authorization.md y owner-f4t4a2-overnight-decisions.md; el contrato.
Criterios de aceptacion: la salida valida contra el esquema; colecciones opacas del contrato copiadas byte a byte (ver COPIA EXACTA);
           AllowedWriteScope segun ALCANCE DE ESCRITURA; celda y effort entre las EligibleCells del contrato; TaskClass y campos de la DELTA exactamente como se indican
Evidencia requerida: ejecuta de verdad estas ordenes en el worktree y usa sus resultados:
           git branch --show-current ; git rev-parse HEAD ; git rev-parse origin/main ;
           git ls-files src/RackCad.UI/Workspace (8 archivos) ; git ls-files tests/RackCad.UI.Tests/Workspace (21 archivos) ; git ls-files tests/RackCad.Tests/Workspace (66 archivos) ; git ls-files src/RackCad.Plugin/Workspace (9 archivos) ;
           y lee el codigo, las pruebas y las guardas que cites
No-touch: todo el repositorio
Condiciones de parada: S-03, P-03 y las F4T4A2-C* del contrato; ante cualquiera, para y explica el bloqueo en RoutingReason.
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

CONTEXTO (hechos versionados en el HEAD; evidencia section 191 y siguientes de docs/automation/evidence/I-64-evidence.md; F4-E53 y F4-E54 en docs/automation/evidence/I-64-pilot/F4-DRAFTS/f4-dispositions.md)
- F4-T1, F4-T2, F4-T3 y F4-T4A1 estan cerradas. El Owner autorizo F4-T4A2-PREPARE-UI, la parte visible y de solo lectura de F4-T4 en el panel: "Comprobar ahora" en el VM del campo (src/RackCad.UI/Workspace/WorkspacePilotFieldViewModel.cs) con el despacho de D-T4A1-03 (TryConfirmBase en Clean, Dirty y Conflict; TryCheckNow en Unknown con CommitThrew); la presentacion de las clases del Owner (UNCHANGED, EXTERNAL_CHANGE, CONFLICT, NOT_FOUND / NO_LONGER_EDITABLE, READ_ERROR / UNKNOWN) sobre los nombres existentes, sin falso limpio; la reconciliacion explicita ("Descartar y recargar", "Conservar mi borrador" solo con un cambio comprometido y sin pendiente, y "No") con una confirmacion Si/No en linea en el propio campo; el diagnostico de admision y la pista del editor clasico en el Conflicto y en la confirmacion del host (D-T4A2-04, el UNICO cambio del host); el diagnostico persistente de la superficie; los avisos de INV-31 y de D-13; las guardas C2-M-1, C2-M-2, Authorize por forma exacta, M-2, M-3, M-4 y M-9; el arrastre (5) de F4-E25. Archivos nuevos: cinco de pruebas (tres en tests/RackCad.UI.Tests/Workspace y dos en tests/RackCad.Tests/Workspace). Oraculo critico del Owner: cero escrituras authored en todas las rutas, ningun falso "sin cambios", Si y No con pruebas de inversion, Authorize que falla ante inversion o sesion o documento equivocados; nada de MUTATE, POST, Undo ni de la UI de Actualizar.
  Presupuesto: Attempt 1, AttemptsRemaining 2, MaxReworkLoops 3, CorrectionsAuthorized true; 1 invocacion de Worker por delegacion aceptada.
- HECHOS DEL REPOSITORIO medidos por la sesion en el HEAD (comprueba los que uses):
  * Lineas base medidas en los TRX de la CI del GREEN de F4-T4A1 (bbd9a35e, CI 43554278196; desde entonces solo documentacion): FullyQualifiedName~RackCad.Tests.Workspace = 2843 y FullyQualifiedName~RackCad.UI.Tests.Workspace = 458, todas en verde. Las dos tienen ExpectRed true: en el RED fallan solo pruebas nuevas o la existente declarada (WorkspacePilotPrepareTests.cs:993), por asercion; las guardas nuevas sobre codigo existente pueden pasar ya en el RED (INV-F4T4A2-14).
  * Los filtros seleccionan por nombre completo: las clases de Core estan en namespace RackCad.Tests; (de archivo) y las de UI en namespace RackCad.UI.Tests (de bloque), y sus nombres empiezan por Workspace, asi que las cinco clases nuevas tambien.
  * La CI corre las pruebas en Debug y compila el Plugin en Release (job build-plugin); el build local del Plugin en Release produce 2 avisos MSB3277 como linea base. La suite completa de UI en Release da 2 fallos de PendingTextFieldTests.ShowOnADirtyField_* que dependen de #if DEBUG y pasan en Debug: condicion previa conocida.
  * Lo que esta FIJADO en el paquete y se traslada a los AcceptanceCriteria como receta, sin reconstruirlo: las dos tablas de clases de section 3; las condiciones de oferta de D-T4A2-01 y D-T4A2-02; el ciclo de la eleccion de D-T4A2-03 (instantanea, revalidacion, caja de solo lectura con una eleccion pendiente, retirada en Apply); las formas EXACTAS de D-T4A2-04 (las dos lineas de TryConfirmPilotBase, las firmas de las fabricas y la prueba 993); el diagnostico persistente de D-T4A2-06; las reglas 1 a 5 de D-T4A2-07 con sus autopruebas; C2-M-1 y C2-M-2 con las 15 mutaciones de map-2 B.3; M-2 con las formas de H-19; M-3; M-4; M-9; los ocho disenos canonicos de H-18; la tabla T-A2 de textos, los prefijos y los botones con sus AutomationProperties.Name.
  * Trampas medidas por la sesion (section 2 del paquete, H-05 y H-20): el navegador tiene una forma publica de 33 firmas exactas y NO cambia; la gemela de G-04 exige un solo Content="Editar", "Recargar", "Volver a leer" y "Descartar" en la superficie (los rotulos nuevos llevan otras palabras); todo el XAML con un solo PreviewKeyDown=, sin KeyUp, sin IsDefault ni IsCancel; el codigo subyacente sin Keyboard, e.KeyboardDevice ni .Focus(; los VM sin +=, event, delegate, Dispatcher, Task ni estado estatico; el vocabulario Metric, Provider, Bom, ProjectSummary, TotalRacks, Aggregat y ComputedParameter prohibido tambien en las cadenas de la UI; Transaction, Database, Commit( y ese vocabulario prohibidos tambien en los comentarios de WorkspaceEditIntent.cs y WorkspaceHostCore.cs; PassGate usa el literal "La peticion no es del documento activo." con su tilde real, no la constante; el identificador pilot del nucleo del host aparece en las formas de H-19 (tambien PreviewWitness, Detail y ClassicEditorHint); BaseOrReadUse conserva la alternativa de result. Cambios declarados de pruebas existentes: solo los de D-T4A2-04 y D-T4A2-07.
  * El doble de la UI (tests/RackCad.UI.Tests/Workspace/WorkspacePilotEditPortDouble.cs) gana CheckNowResponder (por defecto la respuesta actual), los tres parametros opcionales de View(...) y Reads() con las tres lecturas nuevas; ninguna asercion existente cambia.

ALCANCE DE ESCRITURA
- AllowedWriteScope de la delegacion = EXACTAMENTE las 15 entradas del contrato, en su orden (15 archivos exactos, sin prefijos):
    src/RackCad.Application/Workspace/WorkspaceEditIntent.cs
    src/RackCad.Application/Workspace/WorkspaceHostCore.cs
    src/RackCad.UI/Workspace/WorkspacePilotFieldViewModel.cs
    src/RackCad.UI/Workspace/WorkspacePanelView.xaml
    src/RackCad.UI/Workspace/WorkspacePanelView.xaml.cs
    tests/RackCad.UI.Tests/Workspace/WorkspacePilotEditPortDouble.cs
    tests/RackCad.UI.Tests/Workspace/WorkspacePilotReconciliationTests.cs
    tests/RackCad.UI.Tests/Workspace/WorkspacePilotReconciliationViewTests.cs
    tests/RackCad.UI.Tests/Workspace/WorkspaceDraftPilotCanonicalLineEndingTests.cs
    tests/RackCad.Tests/Workspace/WorkspaceCommandAdvisorGuardTests.cs
    tests/RackCad.Tests/Workspace/WorkspaceEditIntentGuardTests.cs
    tests/RackCad.Tests/Workspace/WorkspacePilotPrepareGuardTests.cs
    tests/RackCad.Tests/Workspace/WorkspacePilotPrepareTests.cs
    tests/RackCad.Tests/Workspace/WorkspacePilotReconciliationHostTests.cs
    tests/RackCad.Tests/Workspace/WorkspacePilotReconciliationGuardTests.cs
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
- Todas las cadenas del contrato canonico artifacts/orchestration/I-64/F4-T4A2-PREPARE-UI/1/R20261114T144612Z-2d7d/gate-contract.json son ASCII puro. Se copian byte a byte.
- Authorities, ForbiddenWriteScope, Invariants, RequiredTests, ExpectedEvidence, StopConditions y Objective son DATOS OPACOS de ese contrato. CARGALOS desde el archivo con una orden y reproducelos en tu salida a partir de esa carga, por ejemplo:
  python -c "import json; c=json.load(open(r'artifacts/orchestration/I-64/F4-T4A2-PREPARE-UI/1/R20261114T144612Z-2d7d/gate-contract.json', encoding='ascii')); print(json.dumps({k: c[k] for k in ['Authorities','ForbiddenWriteScope','Invariants','RequiredTests','ExpectedEvidence','StopConditions','Objective','AllowedWriteScope']}, ensure_ascii=True))"
  Mismo orden, mismos elementos, mismos textos byte a byte.
- MODO VERBAL Y GRAMATICA: el contrato usa a proposito construcciones que pueden parecer mejorables (indicativo tras si, con o comprueba; nombres en vez de verbos); se copian IGUAL. Antes de terminar, imprime cada invariante del contrato NUMERADO (por ejemplo: python -c "import json; c=json.load(open(r'artifacts/orchestration/I-64/F4-T4A2-PREPARE-UI/1/R20261114T144612Z-2d7d/gate-contract.json', encoding='ascii')); [print(i, x) for i, x in enumerate(c['Invariants'])]") y compara tu salida con esa impresion, invariante por invariante y verbo por verbo.
- AUTHORITIES: tres autoridades citan paquetes con nombres casi iguales; se copian cada uno en su indice y con su nombre exacto: Authorities[0].Path = "docs/initiatives/I-64-f4-t4a2-gate-contract-package.md" (f4-t4a2: el paquete de esta tarea); Authorities[1].Path = "docs/initiatives/I-64-f4-t4-gate-contract-package.md" (f4-t4, sin a2); Authorities[2].Path = "docs/initiatives/I-64-f4-t3-gate-contract-package.md" (f4-t3). Una planificacion anterior de esta unidad copio un nombre de paquete por otro y se RECHAZO. Antes de terminar imprime cada autoridad NUMERADA (por ejemplo: python -c "import json; c=json.load(open(r'artifacts/orchestration/I-64/F4-T4A2-PREPARE-UI/1/R20261114T144612Z-2d7d/gate-contract.json', encoding='ascii')); [print(i, repr(x)) for i, x in enumerate(c['Authorities'])]") y comparala indice por indice y letra por letra.
- SIN CONFLICTOS INEXISTENTES: el contrato canonico de esta corrida es ASCII PURO (0 bytes no ASCII; compruebalo con python -c "b=open(r'artifacts/orchestration/I-64/F4-T4A2-PREPARE-UI/1/R20261114T144612Z-2d7d/gate-contract.json', 'rb').read(); print(sum(1 for x in b if x >= 0x80))"); TaskClass se escribe exactamente como lo fija este prompt, sin tilde (la tabla de routing.md lleva tilde y NO se copia de alli). Antes de alegar cualquier conflicto o parada S-03, carga el contrato y cita en RoutingReason la orden y su salida que lo demuestran; sin esa prueba no hay conflicto y emites la delegacion completa.
- PROHIBIDO, en TODAS las cadenas copiadas: mejorar la gramatica; normalizar singular o plural o la concordancia; cambiar la puntuacion; anadir o quitar espacios;
  anadir o quitar tildes; cambiar mayusculas; reflujar, partir o unir el texto; parafrasear; corregir lo que parezca una errata. Si una frase te parece incorrecta, se copia IGUAL.
- AllowedWriteScope es la barrera PRIMARIA y exacta de escritura: copialo elemento a elemento, en el mismo orden, sin anadir, quitar, ensanchar ni normalizar ninguna entrada.
  ForbiddenWriteScope tiene exactamente las 129 entradas del contrato: ni una mas ni una menos, y ninguna con espacios anadidos.
- Para ForbiddenWriteScope y AllowedWriteScope, ejecuta ademas una orden que imprima cada entrada del contrato NUMERADA (por ejemplo: python -c "import json; c=json.load(open(r'artifacts/orchestration/I-64/F4-T4A2-PREPARE-UI/1/R20261114T144612Z-2d7d/gate-contract.json', encoding='ascii')); [print(i, repr(x)) for i, x in enumerate(c['ForbiddenWriteScope'])]") y comprueba tu lista contra esa salida, indice por indice, de 0 a 128, mirando cada palabra de cada nombre de archivo (una planificacion anterior perdio la palabra Source en un nombre).
- Antes de dar la salida por terminada, compara tu salida con el contrato cargado campo por campo, cadena por cadena y contando los elementos de cada lista; la sesion ejecuta despues una comparacion determinista byte a byte
  de Invariants, RequiredTests, StopConditions, Authorities, ExpectedEvidence, Objective, AllowedWriteScope y ForbiddenWriteScope ANTES de aceptar: cualquier diferencia = salida RECHAZADA y sin Worker.
- TODA tu salida debe ser ASCII puro, tambien AcceptanceCriteria y RoutingReason: sin tildes, sin enye y sin signos no ASCII; ninguna secuencia de escape formada por barra invertida + u; ningun tabulador ni otro caracter de control dentro de las cadenas.
- RequiredTests con los mismos dos elementos y los mismos valores (ExpectRed true en los dos).

TASKCLASS
- TaskClass = exactamente "Implementacion transversal a capas (corta)"; PromptProfile = "ROUTINE_IMPLEMENTATION"; explica la eleccion en RoutingReason con las siete dimensiones (routing.md section 2) y la fecha de verificacion de la celda. Si tu evaluacion da Horizonte High: para con F4T4A2-C07 explicado en RoutingReason.
- Celda del Worker: Executor.Cell = "anthropic/subagent/claude-sonnet-5-5"; Executor.Provider = "Anthropic"; Executor.Transport = "subagent"; Model = "claude-sonnet-5-5"; Effort.Provider = "high"; Effort.Semantic = "Deep"; ModelEscalationReason = null.

DELTA DE LA TAREA (primera delegacion de la cadena F4-T4A2-PREPARE-UI)
- RunId = R20261114T144612Z-2d7d (tambien es el DelegationRunId); Attempt = 1; AttemptsRemaining = 2; MaxReworkLoops = 3.
- Unit = "I-64"; Initiative = "I-64"; Gate = "F4"; TaskId = "F4-T4A2-PREPARE-UI".
- ExpectedBranch = la salida de `git branch --show-current`; BaseSha = la salida de `git rev-parse HEAD`; ChainBaseSha = ese mismo BaseSha; ChainRedSha = null; ChainRedFiles = []; CorrectionOf = null.
- MainSha = 812baf879612320653094a8aaf46aca0bac81383 (comprueba que coincide con `git rev-parse origin/main`; si no coincide, para con S-13 explicado en RoutingReason); AuthorityRevision = la del contrato.
- ExpectedWorktree = la ruta absoluta del directorio de trabajo actual.
- ExpectedHandoffPath = artifacts/orchestration/I-64/F4-T4A2-PREPARE-UI/1/{WorkRunId}/worker-handoff.json (con el token literal).
- Owner = { Kind: "subagent", Id: "subagent:R20261114T144612Z-2d7d" }; RoutingEnforcement = "required". Executor.Role = "Worker".
- AcceptanceCriteria: redactalos tu, en ASCII, verificables y trazables a los invariantes del contrato; incluye el RED con el esqueleto de INV-F4T4A2-14 (fallan solo pruebas nuevas o la declarada, por asercion, en Core y en UI); las pruebas de VM y de vista de "Comprobar ahora" (oferta y despacho, cada clase de las dos tablas de section 3, sin falso limpio con las teorias sobre todos los estados, el valor por defecto, un valor desconocido y una excepcion del puerto, la superficie que cambia durante la lectura); de las elecciones (oferta, instantanea y revalidacion, Si llama una vez a la elegida y cero a la otra, No no llama a nada, caja de solo lectura con una eleccion pendiente, retirada); del texto pendiente de D-T4A2-05; del diagnostico persistente; las pruebas de Core del host de section 5.2 (los escenarios 2 a 8, 13, 14 y 15 nuevos y la prueba 993 cambiada); las guardas nuevas y endurecidas con sus autopruebas en negativo por Replace del texto real (reglas 1 a 5, C2-M-1 y C2-M-2 con las 15 mutaciones, M-2, M-3 y M-4); el arrastre (5) byte a byte en Windows; el oraculo de cero escrituras authored; el GREEN que no toca tests/; el build Release de la solucion, Plugin incluido, sin errores ni avisos nuevos (tambien con el esqueleto, en local); la CI del SHA exacto y los conteos minimos de RequiredTests.
- IssuedBy = "Controller Codex (planificacion F4-T4A2-PREPARE-UI, primera delegacion de la cadena, CONTROLLER_PLANNING)".
