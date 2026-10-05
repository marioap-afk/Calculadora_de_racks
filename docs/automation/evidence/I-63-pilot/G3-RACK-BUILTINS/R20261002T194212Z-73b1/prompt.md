I-63 — DELEGACION G3-RACK-BUILTINS (G3-T1, RED) — Controller, perfil CONTROLLER_VERIFICATION
Identidad: unidad I-63; gate G3; tarea G3-RACK-BUILTINS; intento 2; RunId R20261002T194212Z-73b1;
           DelegationRunId R20261002T184114Z-1c2f; WorkRunId R20261002T184507Z-9800;
           AuthorityRevision e99621f9e285b1a7ddfcf51cd8ccf970828e0b66; MainSha 819955d61a6da4c811a11fbd11b5dca13f634b7c;
           BaseSha y CurrentSha = los de la entrega; rama architecture/parametros-calculados-resumen-proyecto;
           worktree = el directorio de trabajo actual; Owner subagent:R20261002T184114Z-1c2f
Autoridades (leer desde Git, no copiar): las de la delegacion (campo Authorities); en particular
           docs/AUTOMATION_PLAN.md §16 (16.3 lectura de autoridades, 16.8, 16.9 comprobaciones, 16.10 terminos de gate)
           y docs/automation/agent-execution/README.md §8 (EXTERNAL); docs/initiatives/I-63-proposal-v3.md, la A-1 y la A-2
           (docs/initiatives/I-63-proposal-v3-amendment-a2-gate-verification-sequencing.md) y docs/automation/decisions/I-63.md §2
           (autorizacion escalonada de G3) (UNIT_DOC); AGENTS.md (EXTERNAL);
           vigencia: AuthorityRevision para la unidad, MainSha para lo demas
Objetivo: verificar la entrega del Worker contra Git, la delegacion, el contrato y los registros del relevo, y emitir la
           verificacion (esquema rackcad-controller-verification/v1)
Alcance: solo lectura. No escribas ningun archivo, no hagas commit ni push.
Invariantes del Freeze: INV-15, 17-27 (INV-20 con A-1.2) y 28; D-14..D-19; reglas de G3-T1 (solo pruebas)
Rutas y archivos calientes: las entradas de artifacts/orchestration/I-63/G3-RACK-BUILTINS/2/R20261002T194212Z-73b1/inputs/ (abajo) y el
           rango BaseSha..CurrentSha de la rama
Criterios de aceptacion: la salida valida contra el esquema; las 14 comprobaciones evaluadas en el orden fijo de
           AUTOMATION_PLAN 16.9 con evidencia concreta; Classification, Disposition y FailureClass coherentes con ellas
Evidencia requerida: ordenes de git reales en el worktree (rev-parse, merge-base --is-ancestor, diff --name-only,
           log --format=%B, check-ignore, show) y lectura de los TRX; cita en cada Evidence lo que obtuviste
No-touch: todo el repositorio
Condiciones de parada: S-02, S-03, S-04, S-06, S-12, S-13, P-03, C-01, C-02, C-05, C-06, C-10 y C-11; ante cualquiera,
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
  artifacts/orchestration/I-63/G3-RACK-BUILTINS/2/R20261002T194212Z-73b1/inputs/gate-contract.json (contrato de gate del Coordinator);
  artifacts/orchestration/I-63/G3-RACK-BUILTINS/2/R20261002T194212Z-73b1/inputs/delegation.json (paquete aceptado, RunId R20261002T184114Z-1c2f);
  artifacts/orchestration/I-63/G3-RACK-BUILTINS/2/R20261002T194212Z-73b1/inputs/relay-record-planning.json (registro de la planificacion, con la
  aceptacion A1-A8);
  artifacts/orchestration/I-63/G3-RACK-BUILTINS/2/R20261002T194212Z-73b1/inputs/worker-handoff.json (la entrega; hace las veces de la de
  ExpectedHandoffPath con {WorkRunId} = R20261002T184507Z-9800);
  artifacts/orchestration/I-63/G3-RACK-BUILTINS/2/R20261002T194212Z-73b1/inputs/relay-record-work.json (registro del trabajo: terminacion,
  cesion, relevo de entrada, hechos remotos RemoteFacts, modelo y effort efectivos del Worker y la senal de denegaciones
  en Notes);
  artifacts/orchestration/I-63/G3-RACK-BUILTINS/2/R20261002T194212Z-73b1/inputs/ci-core/<GhRunId>/... y .../inputs/ci-ui/<GhRunId>/... (TRX
  de las corridas push del RedSha, RemoteFacts.RedRun, y del CurrentSha, RemoteFacts.CurrentRun).
- Las senales con actor «sesion -> Controller» de 16.9 (Termination, Remote, Ci, Routing, Denials) salen del registro
  del trabajo; la fecha de la entrega es Outcome.OutputWrittenUtc del registro y el inicio, Cession.StartUtc.
- Semantica del registro del trabajo (README §3.1 y §3.4): Exit es el estado ANTES de invocar al Worker (HEAD = remoto =
  BaseSha) y Entry el estado DESPUES; que HEAD y ls-remote pasen de BaseSha a CurrentSha entre ambos es lo esperado,
  porque el Worker hace commit y push durante la cesion.
- Las pruebas: aplica el RequiredTests del contrato (filtro FullyQualifiedName~RackCad.Tests.ComputedParametersSymbols,
  MinSelected 12, ExpectRed true) a los TRX (seleccion, superadas, fallidas) y contrasta con TestResults de la entrega y
  con el diff; esta entrega exige RED (ChainRedSha null en la delegacion). Comprueba que el RED falla por asercion y no
  por compilacion (los TRX del RED existen y seleccionan las pruebas).
- RT = git diff --name-only <ChainBaseSha> <RedSha> -- tests/ ; comprueba que git diff --name-only RedSha CurrentSha no
  toca RT ni ChainRedFiles; si acreditas el RED, registra en la Evidence de Tests los ChainRedFiles resultantes.
- Naturaleza de la entrega (autorizacion escalonada de G3, decisiones §2): G3-T1 es SOLO el RED de la cadena (ChainRedSha
  null): un commit de pruebas, RedSha = CurrentSha, sin src/. Por eso la corrida push de CurrentSha ES la del RED y su job Core
  falla por diseno; evalua Ci y Tests con su RedPart segun 16.9 y registra si el RED queda acreditado y sus ChainRedFiles. La
  implementacion es de G3-T2, en otra delegacion de la misma cadena, tras la A-3 del Coordinator.
- Cobertura de invariantes del RED: para cada INV de arriba, nombra la prueba del diff que lo observa (git show CurrentSha:<ruta>)
  y comprueba en el TRX del RED que falla en ejecucion (asercion o excepcion) y no por compilacion. INV-22: la entrega registra
  el resultado diagnostico observado para Rack.#{zzz} (codigos del catalogo V6, spans, sin arbol) y la prueba lo fija.
  ExpressionSymbolModelTests.cs: el diff solo cambia aserciones pre-ID20 que D-16 cambia. Ningun archivo de src/ ni de las suites
  de INV-28, ExpressionCoreGuardTests o ExpressionDiagnosticCatalogTests cambia.
- Trailer declarado: el valor literal del campo Worker.Trailer de la entrega (citalo en la Evidence tal como lo leiste);
  modelo efectivo: el del registro del trabajo.
- Terminos de gate: los de AUTOMATION_PLAN 16.10, en los campos de texto libre que alli se listan.
- Coherencia (README §8): EXECUTION_VERIFIED si y solo si las 14 estan en pass (con las 14 en pass, FailureClass NONE);
  una parada se expresa con la comprobacion que la evidencia; FailureClass = la primera que no esta en pass
  (o StopCondition); Disposition segun la precedencia de 16.9 (STOP > BLOCKED > REWORK); con Result pass, RedPart nunca fail.
- TaskId G3-RACK-BUILTINS; RunId R20261002T194212Z-73b1; DelegationRunId R20261002T184114Z-1c2f; WorkRunId R20261002T184507Z-9800;
  Attempt 2; VerifiedSha = el CurrentSha verificado solo si Classification es EXECUTION_VERIFIED; si no, null.
- Findings: hallazgos concretos y comprobables (desviaciones declaradas, limitaciones de frontera, diferencias entre
  la entrega y Git); RecommendedNextAction en una frase.
