I-63 — DELEGACION G1-RACK-METRICS — Controller, perfil CONTROLLER_PLANNING
Identidad: unidad I-63; gate G1; tarea G1-RACK-METRICS; intento 2 (correccion 2); RunId R20261002T141601Z-4a2b;
           DelegationRunId R20261002T141601Z-4a2b; AuthorityRevision 658b35ad498520ba3c832a96eea4389df16dfa60;
           MainSha 819955d61a6da4c811a11fbd11b5dca13f634b7c; BaseSha = el HEAD actual del worktree;
           rama architecture/parametros-calculados-resumen-proyecto; worktree = el directorio de trabajo actual;
           Owner subagent:R20261002T141601Z-4a2b
Autoridades (leer desde Git, no copiar): EXACTAMENTE las del contrato de gate (campo Authorities, con ruta, seccion y
           clase); vigencia: AuthorityRevision para la unidad, MainSha para lo demas
Contexto que motiva la correccion (NO es autoridad; se cita solo por CorrectionOf): el analysis.md del Coordinator
           docs/automation/evidence/I-63-pilot/G1-RACK-METRICS/R20261002T062554Z-e271/analysis.md, en HEAD
Objetivo: emitir el paquete de delegacion (esquema rackcad-delegation/v1) del Worker para la CORRECCION 2 de la tarea
           descrita en el contrato de gate artifacts/orchestration/I-63/G1-RACK-METRICS/2/R20261002T141601Z-4a2b/gate-contract.json,
           segun el analysis.md citado. Esta delegacion SI se ejecutara: el Worker la recibira tal cual la emitas.
Alcance: solo lectura. No escribas ningun archivo, no hagas commit ni push.
Invariantes del Freeze: los del contrato (INV-04, INV-05, INV-09, INV-14, INV-16, INV-34 via de peticion y las
           decisiones D-02, D-08, D-28 que cita); una delegacion nunca amplia el contrato (AUTOMATION_PLAN 16.5)
Rutas y archivos calientes (solo lectura para el Worker): src/RackCad.Application/ComputedParameters/RackMetricIds.cs
           (catalogo productivo de MetricId); docs/initiatives/I-63-proposal-v3.md §5 (D-02: tabla de los seis MetricId y
           regla del token) y §6 (D-05); tests/RackCad.Tests/ComputedParameters/ (pruebas existentes, protegidas);
           tests/RackCad.Tests/NamespaceFolderGuardTests.cs (guarda de I-23)
Criterios de aceptacion: la salida valida contra el esquema; alcance, invariantes, pruebas, autoridades y paradas
           tomados del contrato sin ampliarlos; celda y effort elegidos entre las EligibleCells del contrato
Evidencia requerida: ejecuta de verdad estas ordenes en el worktree y usa sus resultados:
           git branch --show-current ; git rev-parse HEAD ; git rev-parse origin/main ;
           git show --stat fc30dc6cf0d2f97e6732bbb8c512623db91c305d ; git show --stat b5ee157d7c42a3bada7d6804cbbe3cd731405e00 ;
           git log --oneline -4
No-touch: todo el repositorio
Si tu mismo no puedes planificar (ambiguedad material o un paquete que saldria del contrato): no emitas un paquete parcial;
           explica el bloqueo en RoutingReason. Esta linea es para ti; NO es el campo StopConditions del paquete (va abajo)
Prohibido declarar: GATE PASS, Candidato, cierre, integracion o aprobacion del Owner;
           ningun termino de AUTOMATION_PLAN §16 «Terminos de gate» en el texto libre
Trailer: no aplica (no hay commits)
Informe esperado: la salida exacta del esquema rackcad-delegation/v1 (la escribe el CLI por -o)

PERFIL CONTROLLER_PLANNING
Metodo: clasificar y enrutar dentro del contrato de gate, sin escribir en Git.
1. Lee el contrato, docs/automation/agent-execution/routing.md y el catalogo.
2. Clasifica la tarea, puntua las siete dimensiones y elige entre las celdas del
   contrato el nivel mas bajo adecuado; registra RoutingReason con fechas.
3. Copia del contrato el alcance, los invariantes, las pruebas y las paradas; nunca
   los amplies. Nombra los archivos nuevos exactos.
4. Rellena el esquema de delegacion completo; null solo donde el esquema lo admite.

DELTA DE LA TAREA (G1, correccion 2 de la cadena; AUTOMATION_PLAN 16.8 y README de ejecucion delegada §9)
- RunId = R20261002T141601Z-4a2b (tambien es el DelegationRunId); Attempt = 2; AttemptsRemaining = 1; MaxReworkLoops = 3.
- Unit = "I-63"; Initiative = "I-63"; Gate = "G1"; TaskId = "G1-RACK-METRICS" (los del contrato).
- ExpectedBranch = la salida de `git branch --show-current`; BaseSha = la salida de `git rev-parse HEAD`.
- Cadena (custodiada; copiala tal cual): ChainBaseSha = f71de12b33567e8b4109d3a17c60a17d54e96fdc;
  ChainRedSha = fc30dc6cf0d2f97e6732bbb8c512623db91c305d (RED de la correccion 1, acreditado en R20261002T062554Z-e271);
  ChainRedFiles = ["tests/RackCad.Tests/ComputedParameters/RackMetricProviderPurityTests.cs",
  "tests/RackCad.Tests/ComputedParameters/RackMetricRequestTests.cs"];
  CorrectionOf = { RunId: "R20261002T062554Z-e271", FailureClass: "Authority", AnalysisSha256: "a860e8e959f7ab4e491c7856e661370cb9505af63246d785f47691f248260696" }.
- MainSha = 819955d61a6da4c811a11fbd11b5dca13f634b7c (comprueba que coincide con `git rev-parse origin/main`;
  si no coincide, para con S-13 explicado en RoutingReason); AuthorityRevision = la del contrato.
- ExpectedWorktree = la ruta absoluta del directorio de trabajo actual.
- ExpectedHandoffPath = artifacts/orchestration/I-63/G1-RACK-METRICS/2/{WorkRunId}/worker-handoff.json (con el token literal).
- Owner = { Kind: "subagent", Id: "subagent:R20261002T141601Z-4a2b" }; RoutingEnforcement = "required" (el del contrato).
- Executor.Role = "Worker"; Executor.Cell, Executor.Provider, Executor.Transport y Model = los de una celda del contrato
  con capacidad write-commit-push; Effort.Provider = uno de los TestedEfforts de esa celda; Effort.Semantic = el effort
  semantico de routing.md para la clase elegida; PromptProfile = el de esa clase.
- SEIS COLECCIONES (OBLIGATORIO, orden del Coordinator): copia EXACTAMENTE del contrato emitido, byte a byte y en el mismo
  orden, sin anadir, quitar, normalizar tildes ni ortografia, reescribir etiquetas ni estrechar nada. Son estas:
  Authorities = [{"Path": "docs/initiatives/I-63-proposal-v3.md", "Section": "archivo completo (Frozen: YES); en especial D-02, D-05..D-09, D-10a, D-11, D-17, D-26, D-28 y §20-§21", "Class": "UNIT_DOC"}, {"Path": "docs/initiatives/I-63-proposal-v3-amendment-a1-verificacion.md", "Section": "archivo completo; A-1.3 para INV-14 e INV-34", "Class": "UNIT_DOC"}, {"Path": "docs/initiatives/I-63-parametros-calculados-resumen-proyecto.md", "Section": "archivo completo", "Class": "UNIT_DOC"}, {"Path": "docs/automation/decisions/I-63.md", "Section": "§2 (orden de Freeze) y §6", "Class": "UNIT_DOC"}, {"Path": "docs/INITIATIVE_LIFECYCLE.md", "Section": "## 7. Gates funcionales", "Class": "EXTERNAL"}, {"Path": "AGENTS.md", "Section": "## Convenciones arquitectonicas (obligatorias)", "Class": "EXTERNAL"}, {"Path": "docs/adr/0043-motor-expresiones-parametricas-causas-multiples-y-recuperacion-segura.md", "Section": "D1 (nucleo neutral) y D24", "Class": "EXTERNAL"}, {"Path": "docs/AUTOMATION_PLAN.md", "Section": "## 16. Ejecución delegada bajo orden del Coordinator", "Class": "EXTERNAL"}, {"Path": "docs/automation/agent-execution/README.md", "Section": "archivo completo", "Class": "EXTERNAL"}, {"Path": "docs/automation/agent-execution/routing.md", "Section": "## 1. Clases de tarea y ## 4. Algoritmo", "Class": "EXTERNAL"}, {"Path": "docs/initiatives/PROMPT_TEMPLATES.md", "Section": "## G. Delegación de ejecución", "Class": "EXTERNAL"}]
  AllowedWriteScope = ["src/RackCad.Application/ComputedParameters/", "tests/RackCad.Tests/ComputedParameters/"]
  ForbiddenWriteScope = ["src/RackCad.Application/Expressions/", "src/RackCad.Application/Persistence/", "src/RackCad.Application/ProjectVariables/", "src/RackCad.Application/Systems/", "src/RackCad.Application/Bom/", "src/RackCad.Domain/", "src/RackCad.Plugin/", "src/RackCad.UI/", "tests/RackCad.UI.Tests/", "assets/", "deploy/", "eng/", "tools/", ".github/", "RackCad.sln", "Directory.Build.props", "Directory.Build.targets", "global.json", "NuGet.Config", ".gitattributes", ".gitignore", ".editorconfig", "AGENTS.md", "CLAUDE.md", "README.md", "docs/"]
  Invariants = ["INV-04: Rack.Frentes = numero de bahias resueltas del fondo 0 (vacias incluidas); fondo 0 con 4 (1 vacio) y fondo 1 con 6 -> 4; equivalencia con diseno sin celdas", "INV-05: Rack.FrentesVacios cuenta Levels.Count == 0 && FloorPalletCount <= 0 en el fondo 0; tarima de piso sin larguero -> no vacio", "INV-09: una sola funcion de guarda ForbiddenProviderDependencies(codigoFuente) aplicada a los providers reales y a un fixture invalido que llama a SelectiveGeometryResolver; la misma funcion debe detectarlo", "INV-14 (con A-1.3): RackMetricRequest de un Selectivo -> 1 resolucion; Push Back y Cabecera -> 0 resoluciones y 0 lecturas contadas en el costado del lector D-26", "INV-16: mezcla de kinds -> metricas Unavailable(KindIncoherent); Kind ausente -> Unavailable(KindAbsent)", "INV-34 (via de peticion, con A-1.3): Push Back ilegible -> NotSupported con 0 lecturas y 0 resoluciones; Cabecera -> NotApplicable; Selectivo divergente -> Unavailable(SiblingsDivergent) con 0 resoluciones; Selectivo ilegible -> Unavailable(DesignUnreadable); Selectivo valido -> Available", "D-02: identidad MetricId(Rack|Project, token); G1 no crea SymbolId ni namespaces", "D-08: providers puros, registro cerrado por los seis tokens de RackEmbedDocument.Kind* via KindDispatch, sin reflexion ni estado", "D-28: tabla de precedencia unica kind -> soporte de diseno -> E5 -> E4 -> efectivo/resuelto -> Available", "Ningun consumidor existente cambia de comportamiento"]
  RequiredTests = [{"Project": "tests/RackCad.Tests/RackCad.Tests.csproj", "Filter": "FullyQualifiedName~RackCad.Tests.ComputedParameters", "MinSelected": 6, "ExpectRed": true}]
  StopConditions (las 25) = [{"Id": "S-02", "Text": "architecture outside Freeze"}, {"Id": "S-03", "Text": "material ambiguity"}, {"Id": "S-04", "Text": "contradictory evidence"}, {"Id": "S-05", "Text": "destructive/irreversible action not authorized"}, {"Id": "S-06", "Text": "missing credentials/permissions"}, {"Id": "S-07", "Text": "conflict with another active owner"}, {"Id": "S-08", "Text": "branch/worktree ownership conflict"}, {"Id": "S-09", "Text": "dependency not integrated"}, {"Id": "S-12", "Text": "Worker loses context or cannot verify authority revision"}, {"Id": "S-13", "Text": "main movement invalidates exact-SHA assumptions"}, {"Id": "S-14", "Text": "initiative contract explicitly says STOP"}, {"Id": "P-01", "Text": "Cambio del hash de config.toml"}, {"Id": "P-02", "Text": "Participante vivo ajeno, proceso no atribuible o mas de una delegacion abierta"}, {"Id": "P-03", "Text": "Delegacion fuera del contrato"}, {"Id": "P-04", "Text": "Tope de BLOCKED alcanzado"}, {"Id": "P-05", "Text": "Invocacion distinta del paquete aceptado"}, {"Id": "P-06", "Text": "Aviso de limite de uso o de creditos"}, {"Id": "P-07", "Text": "Una invocacion excederia el tope de invocaciones"}, {"Id": "P-08", "Text": "La sesion opero durante una cesion"}, {"Id": "C-01", "Text": "Hace falta tocar Expressions, Persistence, ProjectVariables, Systems, Bom, Domain, Plugin o UI"}, {"Id": "C-02", "Text": "D-07, D-26 o D-28 no pueden implementarse tal como estan congelados (requiere A-n o Architect)"}, {"Id": "C-03", "Text": "Contradiccion entre el Freeze y el codigo consumido (EXP-01)"}, {"Id": "C-04", "Text": "Necesidad de leer o consumir codigo no integrado de I-64, I-62 o I-52"}, {"Id": "C-05", "Text": "Cualquier escritura fuera de AllowedWriteScope"}, {"Id": "C-06", "Text": "Un INV de G1 no puede mostrar un RED observable antes del cambio correcto"}]
  Ningun analysis.md entra en Authorities: los analisis van solo por CorrectionOf.
- La restriccion de esta correccion (crear SOLO el archivo nuevo tests/RackCad.Tests/ComputedParameters/RackMetricIdsTests.cs,
  sin tocar ningun otro: ni los siete de produccion de src/RackCad.Application/ComputedParameters/, ni los dos de
  ChainRedFiles, ni tests/RackCad.Tests/NamespaceFolderGuardTests.cs) va en Objective y AcceptanceCriteria, NO en las
  colecciones de alcance, que son las del contrato.
- Reglas del analysis.md (obligatorias; reflejalas en Objective y AcceptanceCriteria):
  - prueba explicita de D-02 en el archivo nuevo, namespace RackCad.Tests (una sola declaracion) y clase cuyo nombre
    empiece por ComputedParameters (p. ej. ComputedParametersRackMetricIdsTests), seleccionada por el filtro del contrato;
  - para cada uno de los seis MetricId congelados de D-02/D-05 [(Rack, frentes), (Rack, frentesVacios),
    (Project, totalRacks), (Project, rackCount), (Project, totalFrentes), (Project, totalFrentesVacios)]:
    (scope, token) -> el MetricId del catalogo productivo (RackMetricIds) y ese MetricId -> (scope, token); el catalogo
    productivo contiene exactamente esos seis; regla del token de D-02 (ASCII, empieza por minuscula, solo letras y
    digitos, comparador Ordinal);
  - el oraculo son los seis valores congelados del Freeze contrastados con el catalogo productivo leido en la prueba; no
    una copia tautologica del codigo productivo;
  - sin RED nuevo (16.8: ChainRedSha no es null y la entrega no toca ChainRedFiles); la prueba puede nacer verde;
  - un solo commit del Worker, que solo anade el archivo nuevo; suite Core completa en verde, con NamespaceFolderGuardTests;
  - si la prueba exigiera tocar produccion o las pruebas protegidas: parada, sin tocarlas;
  - los campos de texto libre de la entrega no contienen terminos de 16.10 (tampoco «candidato» ni «candidate»).
- AcceptanceCriteria: concretos y comprobables, con los de las reglas anteriores y la conservacion de los INV del contrato
  (las pruebas existentes de INV-04, 05, 09, 14, 16 y 34 siguen en verde sin cambios).
- ModelEscalationReason = null salvo que elijas un nivel superior al minimo adecuado (entonces, el motivo).
- IssuedBy = "Controller Codex (planificacion G1, correccion 2, CONTROLLER_PLANNING)".
