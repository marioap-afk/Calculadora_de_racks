I-63 — DELEGACION G1-RACK-METRICS — Controller, perfil CONTROLLER_VERIFICATION
Identidad: unidad I-63; gate G1; tarea G1-RACK-METRICS; intento 1 (correccion 1); RunId R20261002T062554Z-e271;
           DelegationRunId R20261002T061154Z-9f77; WorkRunId R20261002T061533Z-8e64;
           AuthorityRevision 658b35ad498520ba3c832a96eea4389df16dfa60; MainSha 819955d61a6da4c811a11fbd11b5dca13f634b7c;
           BaseSha y CurrentSha = los de la entrega; rama architecture/parametros-calculados-resumen-proyecto;
           worktree = el directorio de trabajo actual; Owner subagent:R20261002T061154Z-9f77
Autoridades (leer desde Git, no copiar): las de la delegacion (campo Authorities); en particular
           docs/AUTOMATION_PLAN.md §16 (16.3 lectura de autoridades, 16.8, 16.9 comprobaciones, 16.10 terminos de gate)
           y docs/automation/agent-execution/README.md §8 (EXTERNAL); docs/initiatives/I-63-proposal-v3.md y
           docs/initiatives/I-63-proposal-v3-amendment-a1-verificacion.md y el analysis.md del Coordinator
           docs/automation/evidence/I-63-pilot/G1-RACK-METRICS/R20261002T005944Z-1192/analysis.md (UNIT_DOC); AGENTS.md (EXTERNAL);
           vigencia: AuthorityRevision para la unidad, MainSha para lo demas
Objetivo: verificar la entrega del Worker contra Git, la delegacion, el contrato y los registros del relevo, y emitir la
           verificacion (esquema rackcad-controller-verification/v1)
Alcance: solo lectura. No escribas ningun archivo, no hagas commit ni push.
Invariantes del Freeze: INV-04, INV-05, INV-09, INV-14, INV-16, INV-34 (via de peticion), con A-1.3; D-02, D-08, D-28
Rutas y archivos calientes: las entradas de artifacts/orchestration/I-63/G1-RACK-METRICS/1/R20261002T062554Z-e271/inputs/ (abajo) y el
           rango BaseSha..CurrentSha de la rama
Criterios de aceptacion: la salida valida contra el esquema; las 14 comprobaciones evaluadas en el orden fijo de
           AUTOMATION_PLAN 16.9 con evidencia concreta; Classification, Disposition y FailureClass coherentes con ellas
Evidencia requerida: ordenes de git reales en el worktree (rev-parse, merge-base --is-ancestor, diff --name-only,
           log --format=%B, check-ignore, show) y lectura de los TRX; cita en cada Evidence lo que obtuviste
No-touch: todo el repositorio
Condiciones de parada: S-02, S-03, S-04, S-06, S-12, S-13, P-03, C-01, C-02, C-05 y C-06; ante cualquiera,
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
  artifacts/orchestration/I-63/G1-RACK-METRICS/1/R20261002T062554Z-e271/inputs/gate-contract.json (contrato de gate del Coordinator);
  artifacts/orchestration/I-63/G1-RACK-METRICS/1/R20261002T062554Z-e271/inputs/delegation.json (paquete aceptado de la correccion 1, RunId R20261002T061154Z-9f77);
  artifacts/orchestration/I-63/G1-RACK-METRICS/1/R20261002T062554Z-e271/inputs/relay-record-planning.json (registro de la planificacion, con la
  aceptacion A1-A8);
  artifacts/orchestration/I-63/G1-RACK-METRICS/1/R20261002T062554Z-e271/inputs/worker-handoff.json (la entrega; hace las veces de la de
  ExpectedHandoffPath con {WorkRunId} = R20261002T061533Z-8e64);
  artifacts/orchestration/I-63/G1-RACK-METRICS/1/R20261002T062554Z-e271/inputs/relay-record-work.json (registro del trabajo: terminacion,
  cesion, relevo de entrada, hechos remotos RemoteFacts, modelo y effort efectivos del Worker y la senal de denegaciones
  en Notes);
  artifacts/orchestration/I-63/G1-RACK-METRICS/1/R20261002T062554Z-e271/inputs/ci-core/<GhRunId>/... y .../inputs/ci-ui/<GhRunId>/... (TRX
  de las corridas push del RedSha, RemoteFacts.RedRun, y del CurrentSha, RemoteFacts.CurrentRun).
- Las senales con actor «sesion -> Controller» de 16.9 (Termination, Remote, Ci, Routing, Denials) salen del registro
  del trabajo; la fecha de la entrega es Outcome.OutputWrittenUtc del registro y el inicio, Cession.StartUtc.
- Semantica del registro del trabajo (README §3.1 y §3.4): Exit es el estado ANTES de invocar al Worker (HEAD = remoto =
  BaseSha) y Entry el estado DESPUES; que HEAD y ls-remote pasen de BaseSha a CurrentSha entre ambos es lo esperado,
  porque el Worker hace commit y push durante la cesion.
- Las pruebas: aplica el RequiredTests del contrato (filtro FullyQualifiedName~RackCad.Tests.ComputedParameters,
  MinSelected 6, ExpectRed true) a los TRX (seleccion, superadas, fallidas) y contrasta con TestResults de la entrega y
  con el diff. Cadena: ChainBaseSha f71de12b33567e8b4109d3a17c60a17d54e96fdc; ChainRedSha 1013449d61b8292b77273ceeac30b82282b975a2
  (acreditado en R20261002T005944Z-1192); ChainRedFiles = las dos pruebas de tests/RackCad.Tests/ComputedParameters/. Esta entrega
  EXIGE RED porque su diff toca ChainRedFiles (16.8): el RedSha de la entrega es el RED nuevo. Comprueba que ese RED falla por
  asercion y no por compilacion (los TRX del RED existen y seleccionan las pruebas).
- RT = git diff --name-only <ChainBaseSha> <RedSha> -- tests/ ; comprueba que git diff --name-only RedSha CurrentSha no
  toca RT ni ChainRedFiles; si acreditas el RED, registra en la Evidence de Tests los ChainRedFiles resultantes.
- Cobertura de invariantes: para cada invariante de arriba, nombra la prueba del diff que lo observa (lee su codigo con
  git show CurrentSha:<ruta>) y su resultado en el TRX de CurrentSha. INV-14 e INV-34: los contadores de resoluciones y de
  lecturas se observan en los costados que fija A-1.3 (lector D-26 y costado de resolucion), no en los stores. INV-09:
  una sola funcion de guarda, aplicada a los archivos reales de provider y a un fixture invalido con
  SelectiveGeometryResolver. Si un invariante no tiene prueba que lo observe, Tests = fail.
- Trailer declarado: el valor literal del campo Worker.Trailer de la entrega (citalo en la Evidence tal como lo leiste);
  modelo efectivo: el del registro del trabajo.
- Reglas del analysis.md: las pruebas de G1 declaran namespace RackCad.Tests y sus clases empiezan por ComputedParameters;
  NamespaceFolderGuardTests no se modifica y esta en verde en el TRX de CurrentSha; el RED nuevo toca solo los dos archivos de
  ChainRedFiles y el GREEN solo los siete de produccion; una peticion sin hermanas con RackId identificable lanza
  ArgumentException. Contrastalas con git diff --name-only, git show y los TRX.
- Terminos de gate: los de AUTOMATION_PLAN 16.10, en los campos de texto libre que alli se listan.
- Coherencia (README §8): EXECUTION_VERIFIED si y solo si las 14 estan en pass (con las 14 en pass, FailureClass NONE);
  una parada se expresa con la comprobacion que la evidencia; FailureClass = la primera que no esta en pass
  (o StopCondition); Disposition segun la precedencia de 16.9 (STOP > BLOCKED > REWORK); con Result pass, RedPart nunca fail.
- TaskId G1-RACK-METRICS; RunId R20261002T062554Z-e271; DelegationRunId R20261002T061154Z-9f77; WorkRunId R20261002T061533Z-8e64;
  Attempt 1; VerifiedSha = el CurrentSha verificado solo si Classification es EXECUTION_VERIFIED; si no, null.
- Findings: hallazgos concretos y comprobables (desviaciones declaradas, limitaciones de frontera, diferencias entre
  la entrega y Git); RecommendedNextAction en una frase.
