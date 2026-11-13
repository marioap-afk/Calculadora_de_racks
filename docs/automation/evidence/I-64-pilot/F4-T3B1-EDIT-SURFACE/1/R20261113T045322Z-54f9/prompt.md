I-64 - DELEGACION F4-T3B1-EDIT-SURFACE - Controller, perfil CONTROLLER_PLANNING (primera delegacion de la cadena F4-T3B1-EDIT-SURFACE)
Identidad: unidad I-64; gate F4; tarea F4-T3B1-EDIT-SURFACE; intento 1; RunId R20261113T045322Z-54f9;
           DelegationRunId R20261113T045322Z-54f9; AuthorityRevision 495f10e60d7bc80ac8ea6f5affad127980290451 (la del contrato);
           MainSha 812baf879612320653094a8aaf46aca0bac81383; BaseSha = el HEAD actual del worktree (se espera e0e0166bba1701c710c9527f27794f97decd6a87: commit de emision del contrato, hijo directo de la AuthorityRevision 495f10e60d7bc80ac8ea6f5affad127980290451:
           solo anade el contrato, el estado y la evidencia);
           rama architecture/workspace-persistente-rackcad; worktree = el directorio de trabajo actual; Owner subagent:R20261113T045322Z-54f9
Contrato de gate: artifacts/orchestration/I-64/F4-T3B1-EDIT-SURFACE/1/R20261113T045322Z-54f9/gate-contract.json (ASCII puro; custodiado en docs/automation/evidence/I-64-pilot/F4-T3B1-EDIT-SURFACE/coordinator/gate-contract.json)
Autoridades (leer desde Git, no copiar su contenido): las del campo Authorities del contrato;
           vigencia: AuthorityRevision para la unidad (UNIT_DOC y UNIT_CHANGE), MainSha para lo demas (EXTERNAL)
Objetivo: emitir el paquete de delegacion (esquema rackcad-delegation/v1) del Worker para la tarea del contrato. Debe ser ejecutable tal cual.
Alcance: solo lectura. No escribas ningun archivo, no hagas commit ni push. No ejecutes dotnet, ni AutoCAD, ni builds.
Invariantes del Freeze: los 14 del contrato; una delegacion nunca amplia el contrato
Rutas: ver ALCANCE DE ESCRITURA abajo. Todo lo demas es de solo lectura.
           Fuente de diseno: docs/initiatives/I-64-f4-t3b-gate-contract-package.md (v2: section 2, hallazgos H-01 a H-11 y guardas G-01 a G-16; section 3, division en T3B1 y T3B2; section 4, decisiones D-T3B1-01 a D-T3B1-12; section 5, contrato propuesto con los textos exactos nuevos de G-01 a G-04; section 8, disposiciones; la section 6 son reglas de T3B2, fuera de esta tarea); docs/initiatives/I-64-f4-t3-gate-contract-package.md (section 2, tabla K = 1; section 4, el puerto; section 7, R-B1 a R-B12); docs/initiatives/I-64-proposal-v5.md (congelada) D-01, D-02, D-08, D-10 y D-17; las ordenes del Owner owner-f4t3-authorization.md, owner-f4t3a-close-decision.md y owner-f4t3b-decomposition-decision.md; el contrato.
Criterios de aceptacion: la salida valida contra el esquema; colecciones opacas del contrato copiadas byte a byte (ver COPIA EXACTA);
           AllowedWriteScope segun ALCANCE DE ESCRITURA; celda y effort entre las EligibleCells del contrato; TaskClass y campos de la DELTA exactamente como se indican
Evidencia requerida: ejecuta de verdad estas ordenes en el worktree y usa sus resultados:
           git branch --show-current ; git rev-parse HEAD ; git rev-parse origin/main ;
           git ls-files src/RackCad.Plugin/Workspace (9 archivos) ; git ls-files src/RackCad.UI/Workspace (7 archivos) ; git ls-files src/RackCad.Application/Workspace (33 archivos fuera de Drafts y 5 en Drafts) ;
           y lee el codigo, las pruebas y las guardas que cites
No-touch: todo el repositorio
Condiciones de parada: S-03, P-03 y las F4T3B1-C* del contrato; ante cualquiera, para y explica el bloqueo en RoutingReason.
Antecedentes: en esta unidad, cinco planificaciones se RECHAZARON por la copia: "lo leen" cambiado por "los leen" en un invariante (A5 FAIL); UNA entrada de ForbiddenWriteScope omitida (A4 FAIL); un espacio anadido delante de una entrada (" .github/", A4 FAIL); "WorkspacePaletteHostGuardTests.cs" en vez de "WorkspacePaletteHostSourceGuardTests.cs" (le falto la palabra Source; A4 FAIL); y, en la tarea anterior, "mientras la profundidad es mayor que 0" copiado como "sea" (modo verbal cambiado de indicativo a subjuntivo; A5 FAIL). Una sola diferencia de un caracter o un solo elemento de mas o de menos basta para rechazar. Por la parada F4T3B1-C08 del contrato, un rechazo admite una sola replanificacion y un segundo vuelve al Coordinator.
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

CONTEXTO (hechos versionados en el HEAD; evidencia section 167 y siguientes de docs/automation/evidence/I-64-evidence.md; F4-E39 y F4-E40 en docs/automation/evidence/I-64-pilot/F4-DRAFTS/f4-dispositions.md)
- F4-T1, F4-T2 y F4-T3A estan cerradas. El Owner dividio F4-T3 en T3A, T3B y T3C, y F4-T3B en T3B1 y T3B2, cada una con su propio presupuesto, en orden B1, B2, C. Esta tarea es F4-T3B1-EDIT-SURFACE: la superficie de edicion del campo AnnotationScale del piloto en la superficie K = 1 del navegador del panel, sobre el puerto IWorkspacePilotEditPort de F4-T3A (WorkspaceController.PilotEdit). Crea WorkspacePilotFieldViewModel.cs (archivo nuevo en src/RackCad.UI/Workspace); pasa el puerto de la paleta al VM del panel y al del navegador (quinto y tercer parametro opcional); anade el campo, sus gestos y sus botones a la superficie del XAML y del code-behind; y hace los arrastres de los menores 1, 2, 3, 5 y 7 de F4-T3A en pruebas, guardas y documentacion. El cierre de pestanas con Descartar o Cancelar, el descarte del documento, los avisos de veto y de perdida y la retencion por seleccion son de F4-T3B2 y no se delegan aqui. Oraculo critico del Owner: cero escrituras authored en el DWG.
  Presupuesto: Attempt 1, AttemptsRemaining 2, MaxReworkLoops 3, CorrectionsAuthorized true; 1 invocacion de Worker por delegacion aceptada.
- HECHOS DEL REPOSITORIO medidos por la sesion en el HEAD (comprueba los que uses):
  * Lineas base de la CI del commit 18471988 (corrida 43355611879; mismo producto que el HEAD: desde entonces solo cambio documentacion): FullyQualifiedName~RackCad.Tests.Workspace = 2251 y FullyQualifiedName~RackCad.UI.Tests.Workspace = 248, todas en verde. Esta tarea anade pruebas de UI: el filtro de UI tiene ExpectRed true y el de Core ExpectRed false (las pruebas de Core pasan en el RED porque el esqueleto lleva el cableado y la vista completos, INV-F4T3B1-13).
  * El filtro de UI selecciona las clases cuyo nombre completo contiene RackCad.UI.Tests.Workspace: las clases de prueba estan en el namespace RackCad.UI.Tests y su nombre empieza por Workspace, asi que las clases nuevas tambien empiezan por Workspace.
  * La CI corre las pruebas en Debug y compila el Plugin en Release (job build-plugin); el build local del Plugin en Release produce 2 avisos MSB3277 como linea base. La suite completa de UI en Release da 2 fallos de PendingTextFieldTests.ShowOnADirtyField_* que dependen de #if DEBUG y pasan en Debug: condicion previa conocida.
  * La mecanica WPF de la caja y del teclado esta FIJADA en D-T3B1-04 del paquete (binding TwoWay con UpdateSourceTrigger=PropertyChanged, el setter como unica entrada de edicion, PreviewKeyDown y KeyDown con Handled, sin e.KeyboardDevice): trasladala a los AcceptanceCriteria como receta, sin reconstruirla.
  * Las guardas G-01 a G-16 del section 2 del paquete estan medidas en el HEAD; los cambios declarados G-01 a G-04 tienen su texto exacto nuevo en el section 5 y en INV-F4T3B1-12. Ningun archivo nuevo en src/RackCad.Plugin/Workspace (9) ni en Drafts (5).
  * El banco de pruebas de UI tiene dobles internal sealed de los puertos del host, del resumen y de la navegacion y StaTestRunner para las vistas; no tiene doble de IWorkspacePilotEditPort: es uno de los archivos nuevos.

ALCANCE DE ESCRITURA
- AllowedWriteScope de la delegacion = EXACTAMENTE las 19 entradas del contrato, en su orden (19 archivos exactos, sin prefijos):
    src/RackCad.UI/Workspace/WorkspacePilotFieldViewModel.cs
    src/RackCad.UI/Workspace/WorkspaceBrowserViewModel.cs
    src/RackCad.UI/Workspace/WorkspacePanelViewModel.cs
    src/RackCad.UI/Workspace/WorkspacePanelView.xaml
    src/RackCad.UI/Workspace/WorkspacePanelView.xaml.cs
    src/RackCad.Plugin/Workspace/WorkspacePaletteHost.cs
    src/RackCad.Application/Workspace/IWorkspacePilotEditPort.cs
    src/RackCad.Application/Workspace/WorkspaceEditIntent.cs
    tests/RackCad.Tests/Workspace/WorkspacePilotSurfaceGuardTests.cs
    tests/RackCad.Tests/Workspace/WorkspaceBrowserHostGuardTests.cs
    tests/RackCad.Tests/Workspace/WorkspacePaletteHostSourceGuardTests.cs
    tests/RackCad.Tests/Workspace/WorkspaceNavigationHostTests.cs
    tests/RackCad.Tests/Workspace/WorkspacePilotEditGuardTests.cs
    tests/RackCad.Tests/Workspace/WorkspacePilotEditHostTests.cs
    tests/RackCad.Tests/Workspace/WorkspaceIdleSchedulingGuardTests.cs
    tests/RackCad.Tests/Workspace/WorkspaceEditIntentGuardTests.cs
    tests/RackCad.UI.Tests/Workspace/WorkspacePilotEditPortDouble.cs
    tests/RackCad.UI.Tests/Workspace/WorkspacePilotFieldViewModelTests.cs
    tests/RackCad.UI.Tests/Workspace/WorkspacePilotFieldViewTests.cs
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
- Todas las cadenas del contrato canonico artifacts/orchestration/I-64/F4-T3B1-EDIT-SURFACE/1/R20261113T045322Z-54f9/gate-contract.json son ASCII puro. Se copian byte a byte.
- Authorities, ForbiddenWriteScope, Invariants, RequiredTests, ExpectedEvidence, StopConditions y Objective son DATOS OPACOS de ese contrato. CARGALOS desde el archivo con una orden y reproducelos en tu salida a partir de esa carga, por ejemplo:
  python -c "import json; c=json.load(open(r'artifacts/orchestration/I-64/F4-T3B1-EDIT-SURFACE/1/R20261113T045322Z-54f9/gate-contract.json', encoding='ascii')); print(json.dumps({k: c[k] for k in ['Authorities','ForbiddenWriteScope','Invariants','RequiredTests','ExpectedEvidence','StopConditions','Objective','AllowedWriteScope']}, ensure_ascii=True))"
  Mismo orden, mismos elementos, mismos textos byte a byte.
- MODO VERBAL Y GRAMATICA: el contrato usa a proposito construcciones que pueden parecer mejorables (indicativo tras si, con o comprueba; nombres en vez de verbos); se copian IGUAL. Antes de terminar, imprime cada invariante del contrato NUMERADO (por ejemplo: python -c "import json; c=json.load(open(r'artifacts/orchestration/I-64/F4-T3B1-EDIT-SURFACE/1/R20261113T045322Z-54f9/gate-contract.json', encoding='ascii')); [print(i, x) for i, x in enumerate(c['Invariants'])]") y compara tu salida con esa impresion, invariante por invariante y verbo por verbo. Las expresiones regulares de INV-F4T3B1-12 llevan barras invertidas: se copian con las mismas barras.
- PROHIBIDO, en TODAS las cadenas copiadas: mejorar la gramatica; normalizar singular o plural o la concordancia; cambiar la puntuacion; anadir o quitar espacios;
  anadir o quitar tildes; cambiar mayusculas; reflujar, partir o unir el texto; parafrasear; corregir lo que parezca una errata. Si una frase te parece incorrecta, se copia IGUAL.
- AllowedWriteScope es la barrera PRIMARIA y exacta de escritura: copialo elemento a elemento, en el mismo orden, sin anadir, quitar, ensanchar ni normalizar ninguna entrada.
  ForbiddenWriteScope tiene exactamente las 95 entradas del contrato: ni una mas ni una menos, y ninguna con espacios anadidos.
- Para ForbiddenWriteScope y AllowedWriteScope, ejecuta ademas una orden que imprima cada entrada del contrato NUMERADA (por ejemplo: python -c "import json; c=json.load(open(r'artifacts/orchestration/I-64/F4-T3B1-EDIT-SURFACE/1/R20261113T045322Z-54f9/gate-contract.json', encoding='ascii')); [print(i, repr(x)) for i, x in enumerate(c['ForbiddenWriteScope'])]") y comprueba tu lista contra esa salida, indice por indice, de 0 a 94, mirando cada palabra de cada nombre de archivo (una planificacion anterior perdio la palabra Source en un nombre).
- Antes de dar la salida por terminada, compara tu salida con el contrato cargado campo por campo, cadena por cadena y contando los elementos de cada lista; la sesion ejecuta despues una comparacion determinista byte a byte
  de Invariants, RequiredTests, StopConditions, Authorities, ExpectedEvidence, Objective, AllowedWriteScope y ForbiddenWriteScope ANTES de aceptar: cualquier diferencia = salida RECHAZADA y sin Worker.
- TODA tu salida debe ser ASCII puro, tambien AcceptanceCriteria y RoutingReason: sin tildes, sin enye y sin signos no ASCII; ninguna secuencia de escape formada por barra invertida + u; ningun tabulador ni otro caracter de control dentro de las cadenas.
- RequiredTests con los mismos dos elementos y los mismos valores (ExpectRed false en el de Core y true en el de UI).

TASKCLASS
- TaskClass = exactamente "Implementacion transversal a capas (corta)"; PromptProfile = "ROUTINE_IMPLEMENTATION"; explica la eleccion en RoutingReason con las siete dimensiones (routing.md section 2) y la fecha de verificacion de la celda. Si tu evaluacion da Horizonte High: para con F4T3B1-C07 explicado en RoutingReason.
- Celda del Worker: Executor.Cell = "anthropic/subagent/claude-sonnet-5-5"; Executor.Provider = "Anthropic"; Executor.Transport = "subagent"; Model = "claude-sonnet-5-5"; Effort.Provider = "high"; Effort.Semantic = "Deep"; ModelEscalationReason = null.

DELTA DE LA TAREA (primera delegacion de la cadena F4-T3B1-EDIT-SURFACE)
- RunId = R20261113T045322Z-54f9 (tambien es el DelegationRunId); Attempt = 1; AttemptsRemaining = 2; MaxReworkLoops = 3.
- Unit = "I-64"; Initiative = "I-64"; Gate = "F4"; TaskId = "F4-T3B1-EDIT-SURFACE".
- ExpectedBranch = la salida de `git branch --show-current`; BaseSha = la salida de `git rev-parse HEAD`; ChainBaseSha = ese mismo BaseSha; ChainRedSha = null; ChainRedFiles = []; CorrectionOf = null.
- MainSha = 812baf879612320653094a8aaf46aca0bac81383 (comprueba que coincide con `git rev-parse origin/main`; si no coincide, para con S-13 explicado en RoutingReason); AuthorityRevision = la del contrato.
- ExpectedWorktree = la ruta absoluta del directorio de trabajo actual.
- ExpectedHandoffPath = artifacts/orchestration/I-64/F4-T3B1-EDIT-SURFACE/1/{WorkRunId}/worker-handoff.json (con el token literal).
- Owner = { Kind: "subagent", Id: "subagent:R20261113T045322Z-54f9" }; RoutingEnforcement = "required". Executor.Role = "Worker".
- AcceptanceCriteria: redactalos tu, en ASCII, verificables y trazables a los invariantes del contrato; incluye el RED con el esqueleto que lleva el cableado y la vista completos (las pruebas de Core pasan en el RED y las de comportamiento de UI fallan por asercion), las pruebas de VM y de vista (STA, TextBox real) y las guardas de D-T3B1-12, los arrastres de D-T3B1-11, el oraculo de cero escrituras authored, el GREEN que no toca tests/, el build Release del Plugin sin errores ni avisos nuevos, la CI del SHA exacto y los conteos minimos de RequiredTests.
- IssuedBy = "Controller Codex (planificacion F4-T3B1-EDIT-SURFACE, primera delegacion de la cadena, CONTROLLER_PLANNING)".
