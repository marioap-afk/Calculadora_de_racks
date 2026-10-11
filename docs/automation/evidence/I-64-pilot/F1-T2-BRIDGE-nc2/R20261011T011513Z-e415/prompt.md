I-64 - DELEGACION F1-T2-BRIDGE - Controller, perfil CONTROLLER_VERIFICATION
Identidad: unidad I-64; gate F1; tarea F1-T2-BRIDGE; intento 3; RunId R20261011T011513Z-e415;
           DelegationRunId R20261010T082206Z-21c1; WorkRunId R20261010T202008Z-3df4;
           AuthorityRevision d5bdcb7aff31ba54901a3a0acdbfd81be92ab516; MainSha bb0d5522e8411f66a51fdfb3f1f0d0514b737453;
           BaseSha y CurrentSha = los de la entrega; rama architecture/workspace-persistente-rackcad;
           worktree = el directorio de trabajo actual; Owner subagent:R20261010T082206Z-21c1
Autoridades (leer desde Git, no copiar): las de la delegacion (campo Authorities); en particular
           docs/AUTOMATION_PLAN.md Sec.16 (16.3 lectura de autoridades, 16.8, 16.9 comprobaciones, 16.10 terminos de gate)
           y docs/automation/agent-execution/README.md Sec.8 (EXTERNAL); AGENTS.md (EXTERNAL);
           vigencia: AuthorityRevision para la unidad, MainSha para lo demas
Objetivo: verificar la entrega del Worker contra Git, la delegacion, el contrato y los registros del relevo, y emitir la
           verificacion (esquema rackcad-controller-verification/v1)
Alcance: solo lectura. No escribas ningun archivo, no hagas commit ni push. No ejecutes dotnet ni builds: usa los TRX.
Invariantes del Freeze: INV-F1-T2-01..14 (los de la delegacion, vinculantes en su totalidad)
Rutas y archivos calientes: las entradas de artifacts/orchestration/I-64/F1-T2-BRIDGE-nc2/3/R20261011T011513Z-e415/inputs/ (abajo) y el rango
           BaseSha..CurrentSha de la rama
Criterios de aceptacion: la salida valida contra el esquema; las 14 comprobaciones evaluadas en el orden fijo de
           AUTOMATION_PLAN 16.9 con evidencia concreta; Classification, Disposition y FailureClass coherentes con ellas
Evidencia requerida: ordenes de git reales en el worktree (rev-parse, merge-base --is-ancestor, diff --name-only,
           log --format=%B, check-ignore, show) y lectura de los TRX; cita en cada Evidence lo que obtuviste
No-touch: todo el repositorio
Condiciones de parada: S-02, S-03, S-04, S-06, S-12, S-13, P-03 y C-01..C-17 de la delegacion; ante cualquiera,
           Classification EXECUTION_BLOCKED con Disposition STOP y el id en TriggeredStopConditions
Prohibido declarar: GATE PASS, Candidato, cierre, integracion o aprobacion del Owner;
           ningun termino de AUTOMATION_PLAN Sec.16 "Terminos de gate" en tu texto libre
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
- Entradas (copias hechas por la sesion; evalua estas y no otras), bajo
  artifacts/orchestration/I-64/F1-T2-BRIDGE-nc2/3/R20261011T011513Z-e415/inputs/:
  gate-contract.json (contrato de gate emitido por orden del Coordinator, ASCII, AuthorityRevision d5bdcb7a...);
  delegation.json (paquete aceptado, RunId R20261010T082206Z-21c1);
  relay-record-planning.json (registro de la planificacion, con la evaluacion mecanica A1-A8 y nc4 en sus Notes);
  coordinator-acceptance.md (transcripcion literal de la aceptacion formal del Coordinator: ACCEPTED, A1-A8 PASS, nc4 PASS,
  un unico Worker autorizado; se versionara en la custodia);
  worker-handoff.json (la entrega; hace las veces de la de ExpectedHandoffPath con {WorkRunId} = R20261010T202008Z-3df4);
  relay-record-work.json (registro del trabajo: terminacion, cesion, relevo de entrada, hechos remotos RemoteFacts,
  modelo y effort efectivos del Worker, la senal de denegaciones y los procesos en Notes);
  owner-disposition-p01.md y owner-disposition-p01-allowlist.md (transcripciones literales de las dos disposiciones del
  Owner/Coordinator sobre P-01);
  ci-core/38085381131/core.trx y ci-ui/38085381131/ui.trx de la corrida push del CurrentSha (RemoteFacts.CurrentRun, 38085381131);
  ci-core-red/38084788650/core.trx de la corrida push del RedSha (RemoteFacts.RedRun, 38084788650).
- P-01: la Disposition STOP del registro del trabajo es el P-01 de la entrada (config.toml cambio de hash durante la cesion,
  con los mismos 105 nombres). owner-disposition-p01.md lo resuelve y declara el resultado del Worker elegible para esta
  verificacion; owner-disposition-p01-allowlist.md fija, para esta cadena, una lista cerrada de dos hashes (73890CA3... y
  64BEEBC7...) con el inventario de 105 nombres y autoriza esta invocacion. La sesion comprobo hash e inventario justo antes
  de invocarte.
  No es un STOP de esta verificacion ni una senal de Termination fallida.
- Las senales con actor "sesion -> Controller" de 16.9 (Termination, Remote, Ci, Routing, Denials) salen del registro
  del trabajo; la fecha de la entrega es Outcome.OutputWrittenUtc del registro y el inicio, Cession.StartUtc.
- Semantica del registro del trabajo (README Sec.3.1 y Sec.3.4): Exit es el estado ANTES de invocar al Worker (HEAD = remoto =
  BaseSha) y Entry el estado DESPUES; que HEAD y ls-remote pasen de BaseSha a CurrentSha entre ambos es lo esperado,
  porque el Worker hace commit y push durante la cesion. No es contradiccion.
- Los procesos no atribuibles que desaparecen en la relectura quedan CLEARED_BY_REREAD por decision del Coordinator de
  I-64 sobre P-02 (STOP solo si reaparecen de forma persistente o tocan Git, config o el worktree). No son un STOP de esta
  verificacion salvo evidencia de que tocaron algo.
- Autoridades: los valores Section de Authorities en el contrato y en la delegacion son la transliteracion ASCII de los
  encabezados reales (en Git los encabezados conservan sus tildes y la raya larga). Al leer las autoridades (16.3), localiza
  cada seccion por su encabezado comparando sin diacriticos y con la raya larga como guion; esa diferencia de representacion
  no es una discrepancia. Contract compara delegacion y contrato, ambos ASCII.
- Las pruebas y el RED: es la primera delegacion de la cadena: ChainBaseSha = BaseSha = d5bdcb7a..., ChainRedSha = null y
  ChainRedFiles = [], asi que esta entrega EXIGE RED (16.8). RedSha = 31e3c57a... (entre BaseSha y CurrentSha). Aplica cada
  RequiredTests del contrato al TRX de Core del CurrentSha (seleccion, superadas, fallidas) y al TRX de Core del RedSha (la
  parte RED: seleccion > 0 y fallos por asercion de comportamiento congelado; la corrida del RedSha falla en el job de Tests
  y salta el del Plugin, lo esperable en un RED), y contrasta con TestResults de la entrega y con el diff. RT = git diff
  --name-only ChainBaseSha RedSha -- tests/; el GREEN no debe tocar RT (16.8). Si el RED queda acreditado (RedPart = pass en
  Ci y en Tests), ChainRedFiles = RT; registralo en la Evidence de Tests. Los TRX de UI cuentan las pruebas omitidas (Skip)
  como UnitTestResult outcome=NotExecuted, en total y no en executed, con notExecuted=0 en Counters: es la convencion del
  logger, no una contradiccion.
- Scope: git diff --name-only BaseSha..CurrentSha contra AllowedWriteScope y ForbiddenWriteScope de la delegacion (los 5
  archivos de producto de T1 estan prohibidos por ruta exacta; solo archivos nuevos en Application/Workspace).
- La CI del CurrentSha: los cuatro jobs requeridos de AGENTS.md (RemoteFacts.CurrentRun del registro del trabajo).
- Trailer declarado: el campo Worker.Trailer de la entrega; modelo y effort efectivos: los del registro del trabajo.
- Terminos de gate: los de AUTOMATION_PLAN 16.10, en los campos de texto libre que alli se listan.
- Coherencia (README Sec.8): EXECUTION_VERIFIED si y solo si las 14 estan en pass (con las 14 en pass, FailureClass NONE);
  una parada se expresa con la comprobacion que la evidencia; FailureClass = la primera que no esta en pass
  (o StopCondition); Disposition segun la precedencia de 16.9 (STOP > BLOCKED > REWORK); con Result pass, RedPart nunca fail.
- TaskId F1-T2-BRIDGE; RunId R20261011T011513Z-e415; DelegationRunId R20261010T082206Z-21c1; WorkRunId R20261010T202008Z-3df4;
  Attempt 3; VerifiedSha = el CurrentSha verificado solo si Classification es EXECUTION_VERIFIED; si no, null.
- Findings: hallazgos concretos y comprobables (desviaciones declaradas, limitaciones de frontera, diferencias entre
  la entrega y Git, y la causa de cada prueba fallida en los TRX); RecommendedNextAction en una frase.
- Toda tu salida en ASCII: sin tildes ni signos no ASCII y sin secuencias de escape formadas por barra invertida + u.
