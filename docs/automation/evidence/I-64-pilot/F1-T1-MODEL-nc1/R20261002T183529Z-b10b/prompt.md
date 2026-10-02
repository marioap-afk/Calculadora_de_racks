I-64 - DELEGACION F1-T1-MODEL - Controller, perfil CONTROLLER_VERIFICATION
Identidad: unidad I-64; gate F1; tarea F1-T1-MODEL; intento 3; RunId R20261002T183529Z-b10b;
           DelegationRunId R20261002T171954Z-754a; WorkRunId R20261002T181905Z-faf1;
           AuthorityRevision e0587355b0e84d98807057a56dfc50591da87489; MainSha 819955d61a6da4c811a11fbd11b5dca13f634b7c;
           BaseSha y CurrentSha = los de la entrega; rama architecture/workspace-persistente-rackcad;
           worktree = el directorio de trabajo actual; Owner subagent:R20261002T171954Z-754a
Autoridades (leer desde Git, no copiar): las de la delegacion (campo Authorities); en particular
           docs/AUTOMATION_PLAN.md Sec.16 (16.3 lectura de autoridades, 16.8, 16.9 comprobaciones, 16.10 terminos de gate)
           y docs/automation/agent-execution/README.md Sec.8 (EXTERNAL); AGENTS.md (EXTERNAL);
           vigencia: AuthorityRevision para la unidad, MainSha para lo demas
Objetivo: verificar la entrega del Worker contra Git, la delegacion, el contrato y los registros del relevo, y emitir la
           verificacion (esquema rackcad-controller-verification/v1)
Alcance: solo lectura. No escribas ningun archivo, no hagas commit ni push.
Invariantes del Freeze: INV-F1-T1-01..12 (los de la delegacion, vinculantes en su totalidad)
Rutas y archivos calientes: las entradas de artifacts/orchestration/I-64/F1-T1-MODEL-nc1/3/R20261002T183529Z-b10b/inputs/ (abajo) y el rango
           BaseSha..CurrentSha de la rama
Criterios de aceptacion: la salida valida contra el esquema; las 14 comprobaciones evaluadas en el orden fijo de
           AUTOMATION_PLAN 16.9 con evidencia concreta; Classification, Disposition y FailureClass coherentes con ellas
Evidencia requerida: ordenes de git reales en el worktree (rev-parse, merge-base --is-ancestor, diff --name-only,
           log --format=%B, check-ignore, show) y lectura de los TRX; cita en cada Evidence lo que obtuviste
No-touch: todo el repositorio
Condiciones de parada: S-02, S-03, S-04, S-06, S-12, S-13, P-03 y C-01..C-12 de la delegacion; ante cualquiera,
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
  artifacts/orchestration/I-64/F1-T1-MODEL-nc1/3/R20261002T183529Z-b10b/inputs/:
  gate-contract.json (contrato de gate reemitido en ASCII por el Coordinator, REISSUE_GATE_CONTRACT_ASCII, aceptado);
  delegation.json (paquete aceptado, RunId R20261002T171954Z-754a);
  relay-record-planning.json (registro de la planificacion 7/7, con la evaluacion mecanica A1-A8; la aceptacion formal
  la dio el Coordinator por el chat de la sesion: ACCEPTED, A1-A8 PASS, Worker final autorizado; se versionara en la custodia);
  worker-handoff.json (la entrega; hace las veces de la de ExpectedHandoffPath con {WorkRunId} = R20261002T181905Z-faf1);
  relay-record-work.json (registro del trabajo: terminacion, cesion, relevo de entrada, hechos remotos RemoteFacts,
  modelo y effort efectivos del Worker, la senal de denegaciones y los procesos en Notes);
  ci-core/37046833476/core.trx y ci-ui/37046833476/ui.trx de la corrida push del CurrentSha (RemoteFacts.CurrentRun, 37046833476).
  Esta entrega no exige RED: RemoteFacts.RedRun es null.
- Las senales con actor "sesion -> Controller" de 16.9 (Termination, Remote, Ci, Routing, Denials) salen del registro
  del trabajo; la fecha de la entrega es Outcome.OutputWrittenUtc del registro y el inicio, Cession.StartUtc.
- Semantica del registro del trabajo (README Sec.3.1 y Sec.3.4): Exit es el estado ANTES de invocar al Worker (HEAD = remoto =
  BaseSha) y Entry el estado DESPUES; que HEAD y ls-remote pasen de BaseSha a CurrentSha entre ambos es lo esperado,
  porque el Worker hace commit y push durante la cesion. No es contradiccion.
- Los procesos no atribuibles que desaparecen en la relectura quedan CLEARED_BY_REREAD por decision del Coordinator de
  I-64 sobre P-02 (STOP solo si reaparecen de forma persistente o tocan Git, config o el worktree). No son un STOP de esta
  verificacion salvo evidencia de que tocaron algo.
- Autoridades: los valores Section de Authorities en el contrato y en la delegacion son la transliteracion ASCII aceptada por
  el Coordinator de los encabezados reales (en Git los encabezados conservan sus tildes y la raya larga). Al leer las
  autoridades (16.3), localiza cada seccion por su encabezado comparando sin diacriticos y con la raya larga como guion; esa
  diferencia de representacion no es una discrepancia. Contract compara delegacion y contrato, ambos ASCII.
- Las pruebas: aplica cada RequiredTests del contrato al TRX de Core (seleccion, superadas, fallidas) y contrasta con
  TestResults de la entrega y con el diff. Esta es la entrega final de la RECUPERACION DE CONTROLES, attempt 3 (CorrectionOf
  R20261002T164250Z-f842 / StopCondition / 415811c5...): la delegacion trae ChainRedSha a9778068... (RED de la cadena ya
  acreditado por la verificacion R20261002T143410Z-ae0b, custodiada en el HEAD en
  docs/automation/evidence/I-64-pilot/F1-T1-MODEL/R20261002T143410Z-ae0b/controller-verification.json) y ChainRedFiles (5 rutas).
  git diff --name-only BaseSha..CurrentSha no toca ChainRedFiles y ChainRedSha no es null, asi que esta entrega NO exige RED
  (16.8): RedSha null y RedPart = not_applicable en Ci y en Tests, citando la corrida del ChainRedSha (37019198364, registrada
  en RemoteFacts.RedRun de docs/automation/evidence/I-64-pilot/F1-T1-MODEL/R20261002T141605Z-dc2b/relay-record.json en el HEAD).
  ExpectRed del contrato se refiere al RED de la cadena, ya acreditado. ChainBaseSha = e0587355... (el de la primera
  delegacion). Registra en la Evidence de Tests los ChainRedFiles vigentes (los 5 de la delegacion; nunca decrecen).
  Los TRX de UI cuentan las pruebas omitidas (atributos Skip) como UnitTestResult outcome=NotExecuted, en total y no en
  executed, con notExecuted=0 en Counters: es la convencion del logger, no una contradiccion.
- La CI del CurrentSha: los cuatro jobs requeridos de AGENTS.md; el detalle del job "Build Plugin without AutoCAD" (paso y
  mensaje) esta en Notes del registro del trabajo.
- Trailer declarado: el campo Worker.Trailer de la entrega; modelo efectivo: el del registro del trabajo.
- Terminos de gate: los de AUTOMATION_PLAN 16.10, en los campos de texto libre que alli se listan.
- Coherencia (README Sec.8): EXECUTION_VERIFIED si y solo si las 14 estan en pass (con las 14 en pass, FailureClass NONE);
  una parada se expresa con la comprobacion que la evidencia; FailureClass = la primera que no esta en pass
  (o StopCondition); Disposition segun la precedencia de 16.9 (STOP > BLOCKED > REWORK); con Result pass, RedPart nunca fail.
- TaskId F1-T1-MODEL; RunId R20261002T183529Z-b10b; DelegationRunId R20261002T171954Z-754a; WorkRunId R20261002T181905Z-faf1;
  Attempt 3; VerifiedSha = el CurrentSha verificado solo si Classification es EXECUTION_VERIFIED; si no, null.
- Findings: hallazgos concretos y comprobables (desviaciones declaradas, limitaciones de frontera, diferencias entre
  la entrega y Git, y la causa de cada prueba fallida en los TRX); RecommendedNextAction en una frase.
- Toda tu salida en ASCII: sin tildes ni signos no ASCII y sin secuencias de escape formadas por barra invertida + u.
