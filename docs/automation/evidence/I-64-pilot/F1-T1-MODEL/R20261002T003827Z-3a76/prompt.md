I-64 - DELEGACION F1-T1-MODEL - Controller, perfil CONTROLLER_PLANNING (planificacion SUSTITUTIVA)
Identidad: unidad I-64; gate F1; tarea F1-T1-MODEL; intento 0; RunId R20261002T003827Z-3a76;
           DelegationRunId R20261002T003827Z-3a76; AuthorityRevision e0587355b0e84d98807057a56dfc50591da87489;
           MainSha 819955d61a6da4c811a11fbd11b5dca13f634b7c; BaseSha = el HEAD actual del worktree (se espera e0587355b0e84d98807057a56dfc50591da87489);
           rama architecture/workspace-persistente-rackcad; worktree = el directorio de trabajo actual; Owner subagent:R20261002T003827Z-3a76
Contrato de gate: artifacts/orchestration/I-64/F1-T1-MODEL/0/R20261002T003827Z-3a76/gate-contract.json (UTF-8; reemitido por el Coordinator)
Autoridades (leer desde Git, no copiar su contenido): las del campo Authorities del contrato;
           vigencia: AuthorityRevision para la unidad (UNIT_DOC), MainSha para lo demas (EXTERNAL)
Objetivo: emitir el paquete de delegacion (esquema rackcad-delegation/v1) del Worker para la tarea del contrato.
           Esta delegacion SI se ejecutara: el Worker la recibira tal cual la emitas, tras la aceptacion del Coordinator.
Alcance: solo lectura. No escribas ningun archivo, no hagas commit ni push.
Invariantes del Freeze: los del contrato (INV-F1-T1-01..12); una delegacion nunca amplia el contrato
Rutas y archivos calientes: ninguno existente de produccion; todo lo nuevo vive dentro del AllowedWriteScope del contrato.
           Fuente de diseno: docs/initiatives/I-64-proposal-v5.md (congelada) Sec.0, D-02..D-06, Sec.10, Sec.11 (F1) y Anexo A.
Criterios de aceptacion: la salida valida contra el esquema; colecciones del contrato copiadas byte a byte (ver COPIA EXACTA);
           celda y effort entre las EligibleCells del contrato; TaskClass segun la regla de abajo
Evidencia requerida: ejecuta de verdad estas ordenes en el worktree y usa sus resultados:
           git branch --show-current ; git rev-parse HEAD ; git rev-parse origin/main ;
           git ls-tree --name-only HEAD docs/adr/ (para confirmar que docs/adr/0047-* no existe)
No-touch: todo el repositorio
Condiciones de parada: S-03, P-03 y C-01..C-12 del contrato; ante cualquiera, para y explica el bloqueo en RoutingReason
Prohibido declarar: GATE PASS, Candidato, cierre, integracion o aprobacion del Owner;
           ningun termino de AUTOMATION_PLAN Sec.16 "Terminos de gate" en el texto libre
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

CONTEXTO (hechos; no son contradiccion)
- Esta es la SEGUNDA planificacion de F1-T1-MODEL. La primera (R20261002T001349Z-aad5) fue rechazada por el Coordinator
  (P-03 confirmado: no copio literalmente invariantes, autoridades ni prohibiciones) y nunca se entrega al Worker.
  El Coordinator autorizo esta planificacion sustitutiva (P-07) y reemitio el contrato con las mismas reglas materiales
  y un empaquetado corregido. Su decision llego por el chat de la sesion y se versionara en el commit de custodia posterior:
  por eso docs/automation/state/I-64.yml y la evidencia Sec.21 en el HEAD aun dicen "espera decision del Coordinator".
  Eso es el estado anterior a la decision, no una contradiccion (AUTOMATION_PLAN 16.4: no hay commit de la sesion
  entre la planificacion y la aceptacion).
- No hay ADR 0047 en la base; el Worker lo crea.

COPIA EXACTA (regla del Coordinator; A4 y A5 se evaluan contra el texto literal del contrato)
- Authorities, AllowedWriteScope, ForbiddenWriteScope, Invariants, RequiredTests, ExpectedEvidence y StopConditions
  son DATOS OPACOS. Copialos EXACTAMENTE desde gate-contract.json, elemento a elemento, en el mismo orden y con los
  mismos caracteres Unicode: no anadas ni quites tildes, enes o rayas; no normalices Unicode; no reformules, resumas,
  corrijas, traduzcas ni cambies mayusculas o comillas; no cambies los titulos de seccion (Section) ni las rutas;
  no interpretes equivalencias semanticas. No anadas ni quites elementos. No estreches AllowedWriteScope a archivos
  concretos: copialo tal cual (3 entradas). No expandas los prefijos de ForbiddenWriteScope (17 entradas).
  RequiredTests con el mismo Project, Filter, MinSelected y ExpectRed.
- Copia tambien Objective exactamente desde el contrato.
- Lee el contrato con una orden que decodifique UTF-8 (por ejemplo: Get-Content -Raw -Encoding utf8 <ruta>) y vuelve a
  comprobar tu salida contra el archivo antes de emitirla.

TASKCLASS (regla del Coordinator)
- TaskClass describe el trabajo que ejecutara el WORKER, no al Controller: NUNCA "Controller: planificacion / verificacion"
  ni CONTROLLER_PLANNING. El trabajo es implementacion + pruebas del modelo puro de Workspace en RackCad.Application
  (mas el ADR 0047 propuesto), con escritura, commit y push, alcance acotado y effort semantico Balanced.
- Usa literalmente la etiqueta de la columna "Clase" de routing.md Sec.1 que corresponda (con sus tildes tal como esta en
  routing.md) y su Perfil como PromptProfile; explica la eleccion en RoutingReason.
- Celda del Worker: claude-sonnet-5-5|subagent; Effort.Provider = "medium" (unico TestedEffort de la celda);
  Effort.Semantic = "Balanced"; ModelEscalationReason = null salvo escalado respaldado por evidencia (si lo hubiera y
  no cupiera en las celdas del contrato, para y explicalo).

DELTA DE LA TAREA
- RunId = R20261002T003827Z-3a76 (tambien es el DelegationRunId); Attempt = 0; AttemptsRemaining = 3; MaxReworkLoops = 3.
- Unit = "I-64"; Initiative = "I-64"; Gate = "F1"; TaskId = "F1-T1-MODEL" (los del contrato).
- ExpectedBranch = la salida de `git branch --show-current`; BaseSha = la salida de `git rev-parse HEAD`;
  ChainBaseSha = ese mismo BaseSha; ChainRedSha = null; ChainRedFiles = []; CorrectionOf = null.
- MainSha = 819955d61a6da4c811a11fbd11b5dca13f634b7c (comprueba que coincide con `git rev-parse origin/main`;
  si no coincide, para con S-13 explicado en RoutingReason); AuthorityRevision = la del contrato (e0587355...).
- ExpectedWorktree = la ruta absoluta del directorio de trabajo actual.
- ExpectedHandoffPath = artifacts/orchestration/I-64/F1-T1-MODEL/0/{WorkRunId}/worker-handoff.json (con el token literal).
- Owner = { Kind: "subagent", Id: "subagent:R20261002T003827Z-3a76" }; RoutingEnforcement = "required" (el del contrato).
- Executor.Role = "Worker"; Executor.Cell, Executor.Provider, Executor.Transport y Model = los de la celda del contrato.
- AcceptanceCriteria (este campo SI lo redactas tu): concretos y comprobables, derivados SOLO de la Proposal V5 congelada y
  del contrato, limitados al modelo puro de F1 (sin host, sin puente real, sin seleccion real, sin navegacion, sin
  escritura authored): sesion por instancia abierta de documento y sin reemparejar tras destruirse (D-03, INV-07, INV-22);
  pistas encoladas como hints y drenaje solo en reposo y sin modal RackCad, coalescido, con las operaciones permitidas
  (D-04, INV-01, INV-23, INV-33); contextos de seleccion Ninguno, Uno, Seleccion (N), Sin identidad y Diagnostico, con
  "sin Id" = IsNullOrWhiteSpace y sin inventar RackId (D-05, D-06, INV-16, INV-17); sin estado por pestana que duplique
  suscripciones (INV-F1-T1-08); sin referencias a AutoCAD (INV-F1-T1-01); pruebas con espacio de nombres
  RackCad.Tests.Workspace para casar con el filtro del contrato; ADR 0047 en estado propuesto, proyeccion del Anexo A,
  presente antes del commit GREEN; RED real primero (pruebas compilables contra un esqueleto, seleccion >= 1, fallo por
  asercion de comportamiento congelado; sin fallo de compilacion ni prueba artificial) y despues GREEN para el mismo
  conjunto; diff dentro de AllowedWriteScope.
- IssuedBy = "Controller Codex (planificacion sustitutiva F1-T1-MODEL, CONTROLLER_PLANNING)".
