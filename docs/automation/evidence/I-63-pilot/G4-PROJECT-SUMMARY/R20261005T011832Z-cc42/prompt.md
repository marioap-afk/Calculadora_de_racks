I-63 — DELEGACION G4-PROJECT-SUMMARY — Controller, perfil CONTROLLER_VERIFICATION
Identidad: unidad I-63; gate G4; tarea G4-PROJECT-SUMMARY; intento 2; RunId R20261005T011832Z-cc42;
           DelegationRunId R20261003T005345Z-66f3; WorkRunId R20261003T005720Z-2b14;
           AuthorityRevision 527e4b9185173e2f50c10d39a882749c09b77d2f; MainSha 819955d61a6da4c811a11fbd11b5dca13f634b7c;
           BaseSha y CurrentSha = los de la entrega; rama architecture/parametros-calculados-resumen-proyecto;
           worktree = el directorio de trabajo actual; Owner subagent:R20261003T005345Z-66f3
Autoridades (leer desde Git, no copiar): las de la delegacion (campo Authorities); en particular
           docs/AUTOMATION_PLAN.md §16 (16.3 lectura de autoridades, 16.8, 16.9 comprobaciones, 16.10 terminos de gate)
           y docs/automation/agent-execution/README.md §8 (EXTERNAL); docs/initiatives/I-63-proposal-v3.md, la A-1, la A-2
           (docs/initiatives/I-63-proposal-v3-amendment-a2-gate-verification-sequencing.md), la A-3 y docs/automation/decisions/I-63.md
           §2 (autorizacion de G4) (UNIT_DOC); AGENTS.md (EXTERNAL);
           vigencia: AuthorityRevision para la unidad, MainSha para lo demas
Objetivo: verificar la entrega del Worker contra Git, la delegacion, el contrato y los registros del relevo, y emitir la
           verificacion (esquema rackcad-controller-verification/v1)
Alcance: solo lectura. No escribas ningun archivo, no hagas commit ni push.
Invariantes del Freeze: INV-11 (parte de G4, A-2.2), 29 (con A-1.1), 30, 31, 32 (parte de G4, A-2.1) y 34 (resumen, con A-1.3);
           D-20, D-21, D-24, D-25 y D-28
Rutas y archivos calientes: las entradas de artifacts/orchestration/I-63/G4-PROJECT-SUMMARY/2/R20261005T011832Z-cc42/inputs/ (abajo) y el
           rango BaseSha..CurrentSha de la rama
Criterios de aceptacion: la salida valida contra el esquema; las 14 comprobaciones evaluadas en el orden fijo de
           AUTOMATION_PLAN 16.9 con evidencia concreta; Classification, Disposition y FailureClass coherentes con ellas
Evidencia requerida: ordenes de git reales en el worktree (rev-parse, merge-base --is-ancestor, diff --name-only,
           log --format=%B, check-ignore, show) y lectura de los TRX; cita en cada Evidence lo que obtuviste
No-touch: todo el repositorio
Condiciones de parada: S-02, S-03, S-04, S-06, S-12, S-13, P-03, C-01, C-02, C-05, C-06, C-07, C-08, C-09 y C-10; ante cualquiera,
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
  artifacts/orchestration/I-63/G4-PROJECT-SUMMARY/2/R20261005T011832Z-cc42/inputs/gate-contract.json (contrato de gate del Coordinator);
  artifacts/orchestration/I-63/G4-PROJECT-SUMMARY/2/R20261005T011832Z-cc42/inputs/delegation.json (paquete aceptado, RunId R20261003T005345Z-66f3);
  artifacts/orchestration/I-63/G4-PROJECT-SUMMARY/2/R20261005T011832Z-cc42/inputs/relay-record-planning.json (registro de la planificacion, con la
  aceptacion A1-A8);
  artifacts/orchestration/I-63/G4-PROJECT-SUMMARY/2/R20261005T011832Z-cc42/inputs/worker-handoff.json (la entrega; hace las veces de la de
  ExpectedHandoffPath con {WorkRunId} = R20261003T005720Z-2b14);
  artifacts/orchestration/I-63/G4-PROJECT-SUMMARY/2/R20261005T011832Z-cc42/inputs/relay-record-work.json (registro del trabajo: terminacion,
  cesion, relevo de entrada, hechos remotos RemoteFacts, modelo y effort efectivos del Worker y la senal de denegaciones
  en Notes);
  artifacts/orchestration/I-63/G4-PROJECT-SUMMARY/2/R20261005T011832Z-cc42/inputs/ci-core/<GhRunId>/... y .../inputs/ci-ui/<GhRunId>/... (TRX
  de las corridas push del RedSha, RemoteFacts.RedRun, y del CurrentSha, RemoteFacts.CurrentRun).
- Las senales con actor «sesion -> Controller» de 16.9 (Termination, Remote, Ci, Routing, Denials) salen del registro
  del trabajo; la fecha de la entrega es Outcome.OutputWrittenUtc del registro y el inicio, Cession.StartUtc.
- Semantica del registro del trabajo (README §3.1 y §3.4): Exit es el estado ANTES de invocar al Worker (HEAD = remoto =
  BaseSha) y Entry el estado DESPUES; que HEAD y ls-remote pasen de BaseSha a CurrentSha entre ambos es lo esperado,
  porque el Worker hace commit y push durante la cesion.
- Las pruebas: aplica el RequiredTests del contrato (filtro FullyQualifiedName~RackCad.Tests.ComputedParametersSummary,
  MinSelected 10, ExpectRed true) a los TRX (seleccion, superadas, fallidas) y contrasta con TestResults de la entrega y
  con el diff; esta entrega exige RED (ChainRedSha null en la delegacion). Comprueba que el RED falla por asercion y no
  por compilacion (los TRX del RED existen y seleccionan las pruebas).
- RT = git diff --name-only <ChainBaseSha> <RedSha> -- tests/ ; comprueba que git diff --name-only RedSha CurrentSha no
  toca RT ni ChainRedFiles; si acreditas el RED, registra en la Evidence de Tests los ChainRedFiles resultantes.
- Cobertura de invariantes: para cada INV de arriba, nombra la prueba del diff que lo observa (lee su codigo con git show
  CurrentSha:<ruta>) y su resultado en el TRX de CurrentSha. INV-11 e INV-32: la parte de G4 de la A-2 (ProjectSummary Full);
  la parte de G2 sigue en verde. INV-29 (b): una sola funcion ForbiddenReferenceFamilies con objetivo (Application -> vacio) y
  controles positivos con la MISMA funcion (UI -> WPF, Plugin -> contiene AutoCAD), con el reconocimiento de la A-1.1; su RED
  discriminante es el control positivo (orden del Coordinator). INV-30, INV-32 e INV-34: contadores observados (resoluciones,
  evaluaciones de poblacion y lecturas en el costado del lector D-26). INV-31: la tabla cerrada de D-21 en los dos sentidos.
  D-24: caracterizacion con N = 1, 10, 100 y 1000 y tres vistas, contadores como oraculo y tiempos solo registrados. Las D-xx
  del contrato se juzgan por el codigo del diff y por esas pruebas; un INV sin prueba que lo observe hace Tests = fail.
- Alcance de G4: produccion solo en src/RackCad.Application/ComputedParameters/; el cambio de RackComputedExpressionContext.cs
  es aditivo (Evaluate no cambia); MetricValue.Equals no cambia (la provenance va aparte; si cambiara: C-07); sin
  persistencia, UI, Plugin ni Create abierto (C-08). Las pruebas existentes de tests/RackCad.Tests/ComputedParameters/ (G1-G3)
  no se modifican, borran ni renombran: git diff --name-status BaseSha..CurrentSha -- tests/RackCad.Tests/ComputedParameters/
  solo contiene altas (A); las de G4 declaran namespace RackCad.Tests y sus clases empiezan por ComputedParametersSummary.
- Remote: el registro del trabajo trae RemoteFacts.OriginMainSha (git fetch origin; git rev-parse origin/main); el esquema
  rackcad-relay-record/v1 no tiene ese campo en Entry, asi que no lo busques alli. Si tu sandbox no puede ejecutar git
  ls-remote, usa RemoteFacts y dilo en la Evidence.
- TRX de UI: tests/RackCad.UI.Tests tiene 17 atributos Skip (8+3+4+2); el logger TRX los registra como UnitTestResult
  NotExecuted, los cuenta en total y no en executed y deja Counters notExecuted=0 (total - executed = 17). Es la convencion del
  logger, no una contradiccion (precedentes: I-61 R20261001T035734Z-74c2; I-63 evidencia §37.4).
- Presupuesto de tiempo: tienes un tope duro de 600 s; no vuelques archivos grandes completos (TRX, diffs enteros): usa
  conteos y filtros (Select-String, python con salida resumida, git diff --stat, git diff --name-only) y lee solo los
  fragmentos que necesites.
- Trailer declarado: el valor literal del campo Worker.Trailer de la entrega (citalo en la Evidence tal como lo leiste);
  modelo efectivo: el del registro del trabajo.
- Terminos de gate: los de AUTOMATION_PLAN 16.10, en los campos de texto libre que alli se listan.
- Coherencia (README §8): EXECUTION_VERIFIED si y solo si las 14 estan en pass (con las 14 en pass, FailureClass NONE);
  una parada se expresa con la comprobacion que la evidencia; FailureClass = la primera que no esta en pass
  (o StopCondition); Disposition segun la precedencia de 16.9 (STOP > BLOCKED > REWORK); con Result pass, RedPart nunca fail.
- RESOLUCION NORMATIVA DEL COORDINATOR (esta verificacion repite la de R20261003T012440Z-ef71, que termino en EXECUTION_BLOCKED/STOP, Contract,
  [S-03, C-10], por el getter publico RackSummary.RepresentativeDefinitionId no enumerado en D-20): el texto LITERAL de la
  decision esta en artifacts/orchestration/I-63/G4-PROJECT-SUMMARY/2/R20261005T011832Z-cc42/inputs/coordinator-decision-stop-g4.md (pegado por el
  usuario en el chat; se registrara en docs/automation/decisions/I-63.md §2 con la custodia, despues de esta verificacion, para
  no mover HEAD de CurrentSha). Resumen: el Coordinator ACEPTA RackSummary.RepresentativeDefinitionId como precision no
  material de D-20, necesaria para observar INV-11 en G4; sin A-4 y sin cambio de trabajo; S-03 RESUELTA por el Coordinator y
  C-10 NO activada tras la aclaracion; D-20 se lee como contenido semantico minimo obligatorio de RackSummary, no como
  prohibicion de propiedades auxiliares de solo lectura que exponen informacion ya congelada. Condiciones: el valor es
  EXACTAMENTE el representante ya determinado por la poblacion (ProjectPopulation / E4), proyeccion informativa; no selecciona
  representante por su cuenta, no altera Membership, Metrics ni MetricValue, no persiste, no crea otra autoridad ni otro
  recorrido o resolucion (si G4 lo recalculara por otra ruta, eso si seria STOP). Comprueba esa procedencia en el codigo de
  CurrentSha. Misma entrega, mismo GREEN 4f45f446, mismo contrato y misma delegacion; sin replanificacion ni Worker; attempts
  2/3 sin cambio. Disposicion de hallazgos de la misma decision: H-G4-01 desviacion operativa no bloqueante; H-G4-02 aceptado
  (la lectura 1 del Push Back colocado es la E5 de la poblacion; las metricas no leen; INV-34 no prohibe esa lectura de
  poblacion); H-G4-03 aceptado (RackMetricOrchestrator, una sola tabla D-28, con G1/G2 en verde); H-G4-04 compatible con el
  Freeze (R-14).
- EXCEPCION DE IDENTITY DEL COORDINATOR (texto LITERAL en artifacts/orchestration/I-63/G4-PROJECT-SUMMARY/2/R20261005T011832Z-cc42/inputs/coordinator-identity-exception-g4.md):
  tras el STOP de la verificacion anterior la sesion hizo el commit de custodia 7bd5a743 (solo docs) sobre el GREEN, asi que
  hoy HEAD = refs/remotes/origin/<rama> = 7bd5a743 y no 4f45f446. Para ESTA reverificacion, Identity trata 7bd5a743 SOLO como
  descendiente de custodia docs-only del CurrentSha 4f45f4464f5d434f7fa478fea1117d82283ee997 de la entrega, y puede pasar
  SOLO si compruebas tu mismo, con git, que: (1) 4f45f446 es ancestro de 7bd5a743 (git merge-base --is-ancestor); (2)
  git diff --name-only 4f45f446..7bd5a743 solo contiene rutas de documentacion/custodia (docs/); (3) no cambian src/, tests/,
  ningun .csproj, la CI (.github/) ni la configuracion de build; (4) los bytes funcionales son exactamente los del GREEN (por
  ejemplo, git rev-parse 4f45f446:src = git rev-parse 7bd5a743:src, y lo mismo con tests); (5) la CI funcional es la de
  4f45f446 (RemoteFacts.CurrentRun = 37085650331). Si alguna falla, Identity = fail. La excepcion NO debilita el resto de
  Identity: un CurrentSha inexistente o distinto del GREEN sigue fallando Identity. VerifiedSha, si procede, es
  4f45f4464f5d434f7fa478fea1117d82283ee997 (no 7bd5a743). Remote: ls-remote de la rama = 7bd5a743, descendiente docs-only
  del CurrentSha, con la misma excepcion. Comprobacion previa de la sesion (no sustituye la tuya):
  artifacts/orchestration/I-63/G4-PROJECT-SUMMARY/2/R20261005T011832Z-cc42/inputs/identity-exception-check.json.
- TaskId G4-PROJECT-SUMMARY; RunId R20261005T011832Z-cc42; DelegationRunId R20261003T005345Z-66f3; WorkRunId R20261003T005720Z-2b14;
  Attempt 2; VerifiedSha = el CurrentSha verificado solo si Classification es EXECUTION_VERIFIED; si no, null.
- Findings: hallazgos concretos y comprobables (desviaciones declaradas, limitaciones de frontera, diferencias entre
  la entrega y Git); RecommendedNextAction en una frase.
