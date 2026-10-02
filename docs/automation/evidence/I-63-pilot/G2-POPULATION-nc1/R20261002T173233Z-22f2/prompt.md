I-63 — DELEGACION G2-POPULATION — Controller, perfil CONTROLLER_VERIFICATION
Identidad: unidad I-63; gate G2; tarea G2-POPULATION; intento 2; RunId R20261002T173233Z-22f2;
           DelegationRunId R20261002T164053Z-586f; WorkRunId R20261002T164522Z-a109;
           AuthorityRevision 669d8a391f208e1077f136a406fd89058bc0ce6e; MainSha 819955d61a6da4c811a11fbd11b5dca13f634b7c;
           BaseSha y CurrentSha = los de la entrega; rama architecture/parametros-calculados-resumen-proyecto;
           worktree = el directorio de trabajo actual; Owner subagent:R20261002T164053Z-586f
Autoridades (leer desde Git, no copiar): las de la delegacion (campo Authorities); en particular
           docs/AUTOMATION_PLAN.md §16 (16.3 lectura de autoridades, 16.8, 16.9 comprobaciones, 16.10 terminos de gate)
           y docs/automation/agent-execution/README.md §8 (EXTERNAL); docs/initiatives/I-63-proposal-v3.md, la A-1 y la A-2
           (docs/initiatives/I-63-proposal-v3-amendment-a2-gate-verification-sequencing.md) (UNIT_DOC); AGENTS.md (EXTERNAL);
           vigencia: AuthorityRevision para la unidad, MainSha para lo demas
Objetivo: verificar la entrega del Worker contra Git, la delegacion, el contrato y los registros del relevo, y emitir la
           verificacion (esquema rackcad-controller-verification/v1)
Alcance: solo lectura. No escribas ningun archivo, no hagas commit ni push.
Invariantes del Freeze: INV-01, 02, 03, 06, 07, 08, 10, 11 (parte de G2, A-2.2), 12, 13, 32 (parte de G2, A-2.1), 33 y 35;
           D-10, D-10a, D-11, D-12, D-20 (nivel Population), D-26 y D-27
Rutas y archivos calientes: las entradas de artifacts/orchestration/I-63/G2-POPULATION-nc1/2/R20261002T173233Z-22f2/inputs/ (abajo) y el
           rango BaseSha..CurrentSha de la rama
Criterios de aceptacion: la salida valida contra el esquema; las 14 comprobaciones evaluadas en el orden fijo de
           AUTOMATION_PLAN 16.9 con evidencia concreta; Classification, Disposition y FailureClass coherentes con ellas
Evidencia requerida: ordenes de git reales en el worktree (rev-parse, merge-base --is-ancestor, diff --name-only,
           log --format=%B, check-ignore, show) y lectura de los TRX; cita en cada Evidence lo que obtuviste
No-touch: todo el repositorio
Condiciones de parada: S-02, S-03, S-04, S-06, S-12, S-13, P-03, C-01, C-02, C-05, C-06, C-07 y C-08; ante cualquiera,
           Classification EXECUTION_BLOCKED con Disposition STOP y el id en TriggeredStopConditions
Prohibido declarar: GATE PASS, Candidato, cierre, integracion o aprobacion del Owner;
           ningun termino de AUTOMATION_PLAN §16 «Terminos de gate» en tu texto libre
Trailer: no aplica (no hay commits)
Informe esperado: la salida exacta del esquema rackcad-controller-verification/v1 (la escribe el CLI por -o)

PERFIL CONTROLLER_VERIFICATION
Metodo: verificar contra Git y los registros, sin escribir.
1. Evalua las 14 comprobaciones en su orden fijo con ordenes de git reales
   (rev-parse, merge-base, diff --name-only, log, check-ignore, show).
2. Cada comprobacion: pass, fail o not_run, con la evidencia concreta.
3. Rellena Classification, Disposition y FailureClass segun AUTOMATION_PLAN 16.9,
   mirando todas las comprobaciones, no una sola.
4. Contrasta la CI y los conteos del registro de relevo con el diff; no te fies de
   lo que declare la entrega.

DELTA DE LA VERIFICACION
- Entradas (copias hechas por la sesion; evalua estas y no otras):
  artifacts/orchestration/I-63/G2-POPULATION-nc1/2/R20261002T173233Z-22f2/inputs/gate-contract.json (contrato de gate del Coordinator);
  artifacts/orchestration/I-63/G2-POPULATION-nc1/2/R20261002T173233Z-22f2/inputs/delegation.json (paquete aceptado, RunId R20261002T164053Z-586f);
  artifacts/orchestration/I-63/G2-POPULATION-nc1/2/R20261002T173233Z-22f2/inputs/relay-record-planning.json (registro de la planificacion, con la
  aceptacion A1-A8);
  artifacts/orchestration/I-63/G2-POPULATION-nc1/2/R20261002T173233Z-22f2/inputs/worker-handoff.json (la entrega; hace las veces de la de
  ExpectedHandoffPath con {WorkRunId} = R20261002T164522Z-a109);
  artifacts/orchestration/I-63/G2-POPULATION-nc1/2/R20261002T173233Z-22f2/inputs/relay-record-work.json (registro del trabajo: terminacion,
  cesion, relevo de entrada, hechos remotos RemoteFacts, modelo y effort efectivos del Worker y la senal de denegaciones
  en Notes);
  artifacts/orchestration/I-63/G2-POPULATION-nc1/2/R20261002T173233Z-22f2/inputs/ci-core/<GhRunId>/... y .../inputs/ci-ui/<GhRunId>/... (TRX
  de las corridas push del RedSha, RemoteFacts.RedRun, y del CurrentSha, RemoteFacts.CurrentRun).
- Las senales con actor «sesion -> Controller» de 16.9 (Termination, Remote, Ci, Routing, Denials) salen del registro
  del trabajo; la fecha de la entrega es Outcome.OutputWrittenUtc del registro y el inicio, Cession.StartUtc.
- Semantica del registro del trabajo (README §3.1 y §3.4): Exit es el estado ANTES de invocar al Worker (HEAD = remoto =
  BaseSha) y Entry el estado DESPUES; que HEAD y ls-remote pasen de BaseSha a CurrentSha entre ambos es lo esperado,
  porque el Worker hace commit y push durante la cesion.
- Las pruebas: aplica el RequiredTests del contrato (filtro FullyQualifiedName~RackCad.Tests.ComputedParametersPopulation,
  MinSelected 13, ExpectRed true) a los TRX (seleccion, superadas, fallidas) y contrasta con TestResults de la entrega y
  con el diff; esta entrega exige RED (ChainRedSha null en la delegacion). Comprueba que el RED falla por asercion y no
  por compilacion (los TRX del RED existen y seleccionan las pruebas).
- RT = git diff --name-only <ChainBaseSha> <RedSha> -- tests/ ; comprueba que git diff --name-only RedSha CurrentSha no
  toca RT ni ChainRedFiles; si acreditas el RED, registra en la Evidence de Tests los ChainRedFiles resultantes.
- Cobertura de invariantes: para cada INV de arriba, nombra la prueba del diff que lo observa (lee su codigo con git show
  CurrentSha:<ruta>) y su resultado en el TRX de CurrentSha. INV-11 e INV-32: solo la parte de G2 de la A-2 (ProjectSummary
  Full es de G4 y su ausencia no es fallo). INV-33: una sola funcion de guarda que acepta el OutputBlockedReason de
  src/RackCad.Plugin/KindHandlers/PushBackKindHandler.cs en CurrentSha y rechaza el fixture con el texto de 819955d6; en el
  RedSha la misma guarda rechazaba el metodo vigente. INV-35: una sola funcion de guarda sobre los BuildBom de los seis
  handlers, con control positivo. INV-13: la funcion de Application que llama el handler conserva Allow -> null, Deny(m) ->
  m, catalogo nulo -> Loaded(vacio) y Undetermined -> null. Las D-xx del contrato se juzgan por el codigo del diff y por
  esas pruebas; un INV sin prueba que lo observe hace Tests = fail.
- Alcance de G2: la unica edicion del Plugin es OutputBlockedReason de PushBackKindHandler.cs; ProjectSummary Full,
  RackSummary.Metrics y RackComputedExpressionContext no deben existir en el diff (C-08); las pruebas de G2 declaran
  namespace RackCad.Tests y sus clases empiezan por ComputedParametersPopulation; las de G1 siguen intactas y en verde.
- Trailer declarado: el valor literal del campo Worker.Trailer de la entrega (citalo en la Evidence tal como lo leiste);
  modelo efectivo: el del registro del trabajo.
- Terminos de gate: los de AUTOMATION_PLAN 16.10, en los campos de texto libre que alli se listan.
- Coherencia (README §8): EXECUTION_VERIFIED si y solo si las 14 estan en pass (con las 14 en pass, FailureClass NONE);
  una parada se expresa con la comprobacion que la evidencia; FailureClass = la primera que no esta en pass
  (o StopCondition); Disposition segun la precedencia de 16.9 (STOP > BLOCKED > REWORK); con Result pass, RedPart nunca fail.
- TaskId G2-POPULATION; RunId R20261002T173233Z-22f2; DelegationRunId R20261002T164053Z-586f; WorkRunId R20261002T164522Z-a109;
  Attempt 2; VerifiedSha = el CurrentSha verificado solo si Classification es EXECUTION_VERIFIED; si no, null.
- Findings: hallazgos concretos y comprobables (desviaciones declaradas, limitaciones de frontera, diferencias entre
  la entrega y Git); RecommendedNextAction en una frase.
