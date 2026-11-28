I-64 - DELEGACION F2-T1-NAVIGATION-CORE - Controller, perfil CONTROLLER_VERIFICATION
Identidad: unidad I-64; gate F2; tarea F2-T1-NAVIGATION-CORE; intento 1; RunId R20261109T054710Z-53af;
           DelegationRunId R20261108T181429Z-adae; WorkRunId R20261108T182756Z-ec7e;
           AuthorityRevision 90f5872e89660655a93606e16d50f4bc19aa3c9c; MainSha a140eb3a9f055518f0ffa5a3f927ff1c0e1c1e60;
           BaseSha y CurrentSha = los de la entrega; rama architecture/workspace-persistente-rackcad;
           worktree = el directorio de trabajo actual; Owner subagent:R20261108T181429Z-adae
Autoridades (leer desde Git, no copiar): las de la delegacion (campo Authorities); en particular
           docs/AUTOMATION_PLAN.md section 16 (16.3 lectura de autoridades, 16.8, 16.9 comprobaciones, 16.10 terminos de gate)
           y docs/automation/agent-execution/README.md section 8 (EXTERNAL); AGENTS.md (EXTERNAL);
           vigencia: AuthorityRevision para la unidad, MainSha para lo demas
Objetivo: verificar la entrega del Worker contra Git, la delegacion, el contrato y los registros del relevo, y emitir la
           verificacion (esquema rackcad-controller-verification/v1)
Alcance: solo lectura. No escribas ningun archivo, no hagas commit ni push. No ejecutes dotnet ni builds: usa los TRX.
Invariantes del Freeze: los 14 de la delegacion, vinculantes en su totalidad
Rutas y archivos calientes: las entradas de artifacts/orchestration/I-64/F2-T1-NAVIGATION-CORE-nc3/1/R20261109T054710Z-53af/inputs/ (abajo) y el rango
           BaseSha..CurrentSha de la rama
Criterios de aceptacion: la salida valida contra el esquema; las 14 comprobaciones evaluadas en el orden fijo de
           AUTOMATION_PLAN 16.9 con evidencia concreta; Classification, Disposition y FailureClass coherentes con ellas
Evidencia requerida: ordenes de git reales en el worktree (rev-parse, merge-base --is-ancestor, diff --name-only,
           log --format=%B, check-ignore, show) y lectura de los TRX; cita en cada Evidence lo que obtuviste
No-touch: todo el repositorio
Condiciones de parada: S-02, S-03, S-04, S-12, S-13, P-03 y F2-C01..F2-C08 de la delegacion; ante cualquiera,
           Classification EXECUTION_BLOCKED con Disposition STOP y el id en TriggeredStopConditions
Prohibido declarar: GATE PASS, Candidato, cierre, integracion o aprobacion del Owner;
           ningun termino de AUTOMATION_PLAN section 16 "Terminos de gate" en tu texto libre
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
  artifacts/orchestration/I-64/F2-T1-NAVIGATION-CORE-nc3/1/R20261109T054710Z-53af/inputs/:
  gate-contract.json (contrato ASCII de F2-T1, reemision REISSUE_GATE_CONTRACT_ASCII del contrato del Coordinator, AuthorityRevision
  90f5872e...) y contract-equivalence.json (registro de equivalencia mecanica de la reemision);
  delegation.json (paquete de la planificacion R20261108T181429Z-adae);
  relay-record-planning.json (registro de la planificacion, con la evaluacion mecanica A1-A8 (8 pass) y nc4 en sus Notes);
  coordinator-disposition.md (transcripcion literal de la disposicion del Coordinator: contrato ASCII, celdas, presupuesto y la
  secuencia que autoriza invocar al Worker si la delegacion real pasa A1-A8 y nc4 produce su rechazo, sin otra ronda);
  worker-handoff.json (la entrega; hace las veces de la de ExpectedHandoffPath con {WorkRunId} = R20261108T182756Z-ec7e);
  relay-record-work.json (registro del trabajo: terminacion, cesion, relevo de entrada, hechos remotos RemoteFacts,
  modelo y effort efectivos del Worker, la senal de denegaciones y los procesos en Notes);
  ci-core/42511528392/core.trx y ci-ui/42511528392/ui.trx de la corrida push del CurrentSha (RemoteFacts.CurrentRun, 42511528392);
  ci-core-red/42511103159/core.trx de la corrida push del RedSha (RemoteFacts.RedRun, 42511103159).
- config.toml: en la salida y en la entrada del relevo el hash es la SNAPSHOT D del conjunto aceptado A/B/C/D (377D0C3C..., 104
  nombres, lista ordenada EB8775C5...); no es un STOP de esta verificacion.
- Las senales con actor "sesion -> Controller" de 16.9 (Termination, Remote, Ci, Routing, Denials) salen del registro
  del trabajo; la fecha de la entrega es Outcome.OutputWrittenUtc del registro y el inicio, Cession.StartUtc.
- Semantica del registro del trabajo (README section 3.1 y section 3.4): Exit es el estado ANTES de invocar al Worker (HEAD = remoto =
  BaseSha) y Entry el estado DESPUES; que HEAD y ls-remote pasen de BaseSha a CurrentSha entre ambos es lo esperado,
  porque el Worker hace commit y push durante la cesion. No es contradiccion.
- Procesos (16.4): participante es un proceso evaluado cuya linea de ordenes contiene la ruta del worktree. Los procesos no
  atribuibles que desaparecen en la relectura quedan CLEARED_BY_REREAD por decision del Coordinator de I-64 sobre P-02. Los
  hechos de procesos de este relevo estan en las Notes del registro del trabajo; evalualos con esas reglas.
- Autoridades: los valores Section de Authorities en el contrato y en la delegacion son la transliteracion ASCII de los
  encabezados reales (en Git los encabezados conservan sus tildes, la raya larga y el signo de seccion, transliterado como
  "section"). Al leer las autoridades (16.3), localiza cada seccion por su encabezado comparando sin diacriticos, con la raya
  larga como guion y el signo de seccion como "section"; esa diferencia de representacion no es una discrepancia. Contract
  compara delegacion y contrato, ambos ASCII.
- Revisiones: AuthorityRevision = 90f5872e (la del contrato) y BaseSha = ChainBaseSha = 9082bcbc (su hijo: custodia de la apertura
  de F2, de la sonda del Controller y del contrato ASCII, evidencia section 76 y estado). 16.3 exige que AuthorityRevision sea
  ancestro de BaseSha y que git diff --name-only AuthorityRevision BaseSha no contenga rutas EXTERNAL ni con secciones
  UNIT_CHANGE; compruebalo.
- Las pruebas y el RED: es la primera delegacion de la cadena: ChainBaseSha = BaseSha = 9082bcbc..., ChainRedSha = null y
  ChainRedFiles = [], asi que esta entrega EXIGE RED (16.8) en el proyecto requerido (Core). RedSha = f2761979... (entre
  BaseSha y CurrentSha). Aplica el RequiredTests del contrato (tests/RackCad.Tests/RackCad.Tests.csproj, FullyQualifiedName~Workspace,
  MinSelected 645, ExpectRed true) a los TRX de Core del CurrentSha (seleccion, superadas, fallidas) y del RedSha (la parte RED:
  seleccion >= minimo y fallos por asercion del comportamiento de F2-T1; la corrida del RedSha falla en el job de pruebas de
  Core, lo esperable en un RED), y contrasta con TestResults de la entrega y con el diff. RT = git diff --name-only ChainBaseSha
  RedSha -- tests/; el GREEN no debe tocar RT (16.8). Si el RED queda acreditado (RedPart = pass en Ci y en Tests),
  ChainRedFiles = RT; registralo en la Evidence de Tests. El TRX de UI del CurrentSha es informativo (el contrato no exige pruebas
  de UI) y cuenta las pruebas omitidas (Skip) como UnitTestResult outcome=NotExecuted: es la convencion del logger. La CI ejecuta
  las pruebas en configuracion Debug.
- Scope: git diff --name-only BaseSha..CurrentSha contra AllowedWriteScope (2 entradas) y ForbiddenWriteScope (17 entradas) de la
  delegacion.
- La CI del CurrentSha: los cuatro jobs requeridos de AGENTS.md (RemoteFacts.CurrentRun del registro del trabajo).
- Trailer declarado: el campo Worker.Trailer de la entrega; modelo y effort efectivos: los del registro del trabajo.
- Terminos de gate: los de AUTOMATION_PLAN 16.10, en los campos de texto libre que alli se listan.
- Coherencia (README section 8): EXECUTION_VERIFIED si y solo si las 14 estan en pass (con las 14 en pass, FailureClass NONE);
  una parada se expresa con la comprobacion que la evidencia; FailureClass = la primera que no esta en pass
  (o StopCondition); Disposition segun la precedencia de 16.9 (STOP > BLOCKED > REWORK); con Result pass, RedPart nunca fail.
- TaskId F2-T1-NAVIGATION-CORE; RunId R20261109T054710Z-53af; DelegationRunId R20261108T181429Z-adae; WorkRunId R20261108T182756Z-ec7e;
  Attempt 1; VerifiedSha = el CurrentSha verificado solo si Classification es EXECUTION_VERIFIED; si no, null.
- Findings: hallazgos concretos y comprobables (desviaciones declaradas, limitaciones de frontera, diferencias entre
  la entrega y Git, y la causa de cada prueba fallida en los TRX); RecommendedNextAction en una frase.
- Toda tu salida en ASCII: sin tildes ni signos no ASCII y sin secuencias de escape formadas por barra invertida + u.

POST-CUSTODY IDENTITY MODE
For this verification, the verified product commit may be an ancestor of the
current branch HEAD because documentation/evidence custody was committed
after product verification.
When POST_CUSTODY_VERIFIED_DELIVERY_DESCENDANT facts are supplied:
* treat CurrentSha as the verified product SHA;
* treat the current HEAD/remote branch as the custody/control HEAD;
* Identity passes only when CurrentSha is an ancestor of HEAD and every path
in CurrentSha..HEAD is an authorized documentation/evidence/state path;
* any product, test, project, deploy or asset change after CurrentSha makes
Identity fail;
* report CurrentSha and HEAD separately;
* never claim HEAD == CurrentSha when they differ.
Remote must report the ACTUAL current remote branch HEAD, not the historical
CurrentSha.

POST_CUSTODY_VERIFIED_DELIVERY_DESCENDANT FACTS
(measured by the responsible session at 2026-11-09T05:47:11Z, immediately before this invocation)
VERIFIED_PRODUCT_SHA = CurrentSha = c726410f3ac764f4a80a08a2b810b5fdda276ecc
CONTROL_HEAD_SHA = 7002678f46b3816aa06df4cb77a945cbf250af54
MainSha = a140eb3a9f055518f0ffa5a3f927ff1c0e1c1e60
Identity PASS requires ALL of:
1. VERIFIED_PRODUCT_SHA = c726410f3ac764f4a80a08a2b810b5fdda276ecc
2. HEAD = actual origin branch = CONTROL_HEAD_SHA
3. VERIFIED_PRODUCT_SHA is ancestor of CONTROL_HEAD_SHA
4. diff VERIFIED_PRODUCT_SHA..CONTROL_HEAD_SHA contains documentation / automation evidence / state changes only
5. no src/ changes
6. no tests/ changes
7. no project-file changes
8. no deploy/ or assets/ changes
9. exact-SHA CI for CONTROL_HEAD_SHA is successful
10. origin/main is unchanged from the declared MainSha
Measured values:
- git rev-parse HEAD = 7002678f46b3816aa06df4cb77a945cbf250af54
- git ls-remote origin refs/heads/architecture/workspace-persistente-rackcad = 7002678f46b3816aa06df4cb77a945cbf250af54
- git ls-remote origin refs/heads/main = a140eb3a9f055518f0ffa5a3f927ff1c0e1c1e60
- git merge-base --is-ancestor c726410f3ac764f4a80a08a2b810b5fdda276ecc 7002678f46b3816aa06df4cb77a945cbf250af54: exit 0
- git diff --name-only c726410f3ac764f4a80a08a2b810b5fdda276ecc..7002678f46b3816aa06df4cb77a945cbf250af54: 206 paths, all under docs/; none under src/, tests/, deploy/ or assets/; no project files
- exact-SHA CI of CONTROL_HEAD_SHA: run 42568842371, event push, branch architecture/workspace-persistente-rackcad, head 7002678f46b3816aa06df4cb77a945cbf250af54, conclusion success; jobs: Tests (Domain + Application) = success; Build UI (WPF, valida API de Application) = success; UI Tests (WPF controls, net8.0-windows) = success; Build Plugin without AutoCAD = success

CONTROLLER OUTPUT HYGIENE
When a restricted AUTOMATION_PLAN 16.10 term is detected in an input
artifact, never reproduce that restricted term literally in
Controller-generated free text.
In:
Evidence
Findings
UnexpectedFindings
KnownLimitations
Deviations
OpenQuestions
RecommendedNextAction
refer to it only as:
"a prohibited gate term"
or:
"the injected restricted 16.10 term"
Do not quote the source token.
This rule changes only output hygiene.
It does not alter FreeText detection.
