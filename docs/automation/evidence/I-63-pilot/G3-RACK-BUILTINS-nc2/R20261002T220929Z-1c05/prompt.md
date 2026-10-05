I-63 — DELEGACION G3-RACK-BUILTINS (G3-T2, GREEN) — Controller, perfil CONTROLLER_VERIFICATION
Identidad: unidad I-63; gate G3; tarea G3-RACK-BUILTINS; intento 2; RunId R20261002T220929Z-1c05;
           DelegationRunId R20261002T214309Z-19d7; WorkRunId R20261002T214618Z-be6d;
           AuthorityRevision ea70e3b333efec49d3c3b9bc1e5d0395adc71a01; MainSha 819955d61a6da4c811a11fbd11b5dca13f634b7c;
           BaseSha y CurrentSha = los de la entrega; rama architecture/parametros-calculados-resumen-proyecto;
           worktree = el directorio de trabajo actual; Owner subagent:R20261002T214309Z-19d7
Autoridades (leer desde Git, no copiar): las de la delegacion (campo Authorities); en particular
           docs/AUTOMATION_PLAN.md §16 (16.3 lectura de autoridades, 16.8, 16.9 comprobaciones, 16.10 terminos de gate)
           y docs/automation/agent-execution/README.md §8 (EXTERNAL); docs/initiatives/I-63-proposal-v3.md, la A-1 y la A-2
           (docs/initiatives/I-63-proposal-v3-amendment-a2-gate-verification-sequencing.md), la A-3
           (docs/initiatives/I-63-proposal-v3-amendment-a3-inv22-diagnostico.md) y docs/automation/decisions/I-63.md §2
           (autorizacion escalonada de G3 y autorizacion de G3-T2) (UNIT_DOC); AGENTS.md (EXTERNAL);
           vigencia: AuthorityRevision para la unidad, MainSha para lo demas
Objetivo: verificar la entrega del Worker contra Git, la delegacion, el contrato y los registros del relevo, y emitir la
           verificacion (esquema rackcad-controller-verification/v1)
Alcance: solo lectura. No escribas ningun archivo, no hagas commit ni push.
Invariantes del Freeze: INV-15, 17-27 (INV-20 con A-1.2; INV-22 con la A-3) y 28; D-14..D-19; reglas de G3-T2
Rutas y archivos calientes: las entradas de artifacts/orchestration/I-63/G3-RACK-BUILTINS-nc2/2/R20261002T220929Z-1c05/inputs/ (abajo) y el
           rango BaseSha..CurrentSha de la rama
Criterios de aceptacion: la salida valida contra el esquema; las 14 comprobaciones evaluadas en el orden fijo de
           AUTOMATION_PLAN 16.9 con evidencia concreta; Classification, Disposition y FailureClass coherentes con ellas
Evidencia requerida: ordenes de git reales en el worktree (rev-parse, merge-base --is-ancestor, diff --name-only,
           log --format=%B, check-ignore, show) y lectura de los TRX; cita en cada Evidence lo que obtuviste
No-touch: todo el repositorio
Condiciones de parada: S-02, S-03, S-04, S-06, S-12, S-13, P-03, C-01, C-02, C-05, C-07, C-09, C-10, C-11 y C-12; ante cualquiera,
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
  artifacts/orchestration/I-63/G3-RACK-BUILTINS-nc2/2/R20261002T220929Z-1c05/inputs/gate-contract.json (contrato de gate del Coordinator);
  artifacts/orchestration/I-63/G3-RACK-BUILTINS-nc2/2/R20261002T220929Z-1c05/inputs/delegation.json (paquete aceptado, RunId R20261002T214309Z-19d7);
  artifacts/orchestration/I-63/G3-RACK-BUILTINS-nc2/2/R20261002T220929Z-1c05/inputs/relay-record-planning.json (registro de la planificacion, con la
  aceptacion A1-A8);
  artifacts/orchestration/I-63/G3-RACK-BUILTINS-nc2/2/R20261002T220929Z-1c05/inputs/worker-handoff.json (la entrega; hace las veces de la de
  ExpectedHandoffPath con {WorkRunId} = R20261002T214618Z-be6d);
  artifacts/orchestration/I-63/G3-RACK-BUILTINS-nc2/2/R20261002T220929Z-1c05/inputs/relay-record-work.json (registro del trabajo: terminacion,
  cesion, relevo de entrada, hechos remotos RemoteFacts, modelo y effort efectivos del Worker y la senal de denegaciones
  en Notes);
  artifacts/orchestration/I-63/G3-RACK-BUILTINS-nc2/2/R20261002T220929Z-1c05/inputs/ci-core/<GhRunId>/... y .../inputs/ci-ui/<GhRunId>/... (TRX
  de las corridas push del RedSha, RemoteFacts.RedRun, y del CurrentSha, RemoteFacts.CurrentRun).
- Las senales con actor «sesion -> Controller» de 16.9 (Termination, Remote, Ci, Routing, Denials) salen del registro
  del trabajo; la fecha de la entrega es Outcome.OutputWrittenUtc del registro y el inicio, Cession.StartUtc.
- Semantica del registro del trabajo (README §3.1 y §3.4): Exit es el estado ANTES de invocar al Worker (HEAD = remoto =
  BaseSha) y Entry el estado DESPUES; que HEAD y ls-remote pasen de BaseSha a CurrentSha entre ambos es lo esperado,
  porque el Worker hace commit y push durante la cesion.
- Las pruebas: aplica el RequiredTests del contrato (filtro FullyQualifiedName~RackCad.Tests.ComputedParametersSymbols,
  MinSelected 12, ExpectRed true) a los TRX (seleccion, superadas, fallidas) y contrasta con TestResults de la entrega y
  con el diff. ExpectRed se satisface con el RED de la cadena (16.8): esta entrega es la continuacion autorizada de G3-T2, con
  ChainRedSha = 637dce7e5e7811992330b3c305358d9a7542b9f6 (RED de G3-T1, acreditado en R20261002T194212Z-73b1) y RedSha = null.
  NO exige RED salvo que git diff --name-only BaseSha..CurrentSha toque ChainRedFiles: compruebalo. Si no lo toca, RedPart es
  not_applicable en Ci y en Tests citando la corrida del ChainRedSha (RemoteFacts.RedRun = 37053113058, cuyos TRX tambien estan
  en inputs/).
- RT = git diff --name-only <ChainBaseSha> <ChainRedSha> -- tests/ ; comprueba que git diff --name-only BaseSha CurrentSha no
  toca RT ni ChainRedFiles (el GREEN no toca RT ∪ ChainRedFiles).
- Naturaleza de la entrega (decisiones §2): G3-T2 es el GREEN de la cadena, sin correccion (CorrectionOf null, sin consumo de
  attempts). La orden del Coordinator que lo autoriza y corrige la condicion de CI de la A-3 esta registrada en
  docs/automation/decisions/I-63.md §2 en f5c4a5a2 (commit solo de docs posterior a AuthorityRevision); esa misma orden fija
  AuthorityRevision = ea70e3b3 (la A-3). La decision del Owner sobre el STOP P-01 de la primera planificacion de T2 (repetirla
  con el mismo contrato y RunId nuevo; nueva linea base de config.toml) esta en el mismo §2, en 71205235 (= BaseSha). Ci exige
  la corrida push del CurrentSha con los cuatro jobs en success.
- Cobertura de invariantes: para cada INV de arriba, nombra la prueba que lo cubre y comprueba en el TRX del CurrentSha que pasa.
  El filtro selecciona las 89 del RED (y las nuevas, si las hay) y todas pasan; la suite Core completa no tiene fallidas.
  INV-22 (A-3): la entrega registra Rack.#{zzz} -> un InvalidQualifier (11, SyntaxAndLimits) en 5+6 sin arbol, y el resultado
  vigente del caso de 32 caracteres hexadecimales (un UnexpectedToken) conservado; el diff no toca
  src/RackCad.Application/Expressions/ExpressionLexer.cs, ExpressionSyntaxParser.cs ni ExpressionDiagnosticCode.cs (D-16).
  Ningun archivo de las suites de INV-28, ExpressionCoreGuardTests, ExpressionDiagnosticCatalogTests ni NamespaceFolderGuardTests
  cambia, y todas pasan en el TRX.
- Remote: el registro del trabajo trae RemoteFacts.OriginMainSha (obtenido con git fetch origin y git rev-parse origin/main); el
  esquema rackcad-relay-record/v1 no tiene ese campo en Entry, asi que no lo busques alli. Si tu sandbox no puede ejecutar
  git ls-remote, usa RemoteFacts y dilo en la Evidence.
- TRX de UI: tests/RackCad.UI.Tests tiene 17 atributos Skip (8+3+4+2); el logger TRX los registra como UnitTestResult
  NotExecuted, los cuenta en total y no en executed y deja Counters notExecuted=0 (total - executed = 17). Es la convencion del
  logger, no una contradiccion (precedentes: I-61 R20261001T035734Z-74c2; I-63 evidencia §37.4).
- Presupuesto de tiempo: tienes un tope duro de 600 s y la entrega toca produccion; no vuelques archivos grandes completos
  (TRX, diffs enteros): usa conteos y filtros (Select-String, python con salida resumida, git diff --stat, git diff --name-only)
  y lee solo los fragmentos que necesites.
- Trailer declarado: el valor literal del campo Worker.Trailer de la entrega (citalo en la Evidence tal como lo leiste);
  modelo efectivo: el del registro del trabajo.
- Terminos de gate: los de AUTOMATION_PLAN 16.10, en los campos de texto libre que alli se listan.
- Coherencia (README §8): EXECUTION_VERIFIED si y solo si las 14 estan en pass (con las 14 en pass, FailureClass NONE);
  una parada se expresa con la comprobacion que la evidencia; FailureClass = la primera que no esta en pass
  (o StopCondition); Disposition segun la precedencia de 16.9 (STOP > BLOCKED > REWORK); con Result pass, RedPart nunca fail.
- TaskId G3-RACK-BUILTINS; RunId R20261002T220929Z-1c05; DelegationRunId R20261002T214309Z-19d7; WorkRunId R20261002T214618Z-be6d;
  Attempt 2; VerifiedSha = el CurrentSha verificado solo si Classification es EXECUTION_VERIFIED; si no, null.
- Findings: hallazgos concretos y comprobables (desviaciones declaradas, limitaciones de frontera, diferencias entre
  la entrega y Git); RecommendedNextAction en una frase.
