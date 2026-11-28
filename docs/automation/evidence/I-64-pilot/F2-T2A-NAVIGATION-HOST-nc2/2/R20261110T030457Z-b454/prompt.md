I-64 - DELEGACION F2-T2A-NAVIGATION-HOST - Controller, perfil CONTROLLER_VERIFICATION
Identidad: unidad I-64; gate F2; tarea F2-T2A-NAVIGATION-HOST; intento 2; RunId R20261110T030457Z-b454;
           DelegationRunId R20261109T193332Z-bafe; WorkRunId R20261109T194127Z-31c0;
           AuthorityRevision 09a1499a46d16c8251ac4575d370b551603b6372; MainSha a140eb3a9f055518f0ffa5a3f927ff1c0e1c1e60;
           BaseSha y CurrentSha = los de la entrega; rama architecture/workspace-persistente-rackcad;
           worktree = el directorio de trabajo actual; Owner subagent:R20261109T193332Z-bafe
Autoridades (leer desde Git, no copiar): las de la delegacion (campo Authorities); en particular
           docs/AUTOMATION_PLAN.md section 16 (16.3 lectura de autoridades, 16.8, 16.9 comprobaciones, 16.10 terminos de gate)
           y docs/automation/agent-execution/README.md section 8 (EXTERNAL); AGENTS.md (EXTERNAL);
           vigencia: AuthorityRevision para la unidad, MainSha para lo demas
Objetivo: verificar la entrega del Worker contra Git, la delegacion, el contrato y los registros del relevo, y emitir la
           verificacion (esquema rackcad-controller-verification/v1)
Alcance: solo lectura. No escribas ningun archivo, no hagas commit ni push. No ejecutes dotnet ni builds: usa los TRX.
Invariantes del Freeze: los 16 de la delegacion, vinculantes en su totalidad
Rutas y archivos calientes: las entradas de artifacts/orchestration/I-64/F2-T2A-NAVIGATION-HOST-nc2/2/R20261110T030457Z-b454/inputs/ (abajo) y el rango
           BaseSha..CurrentSha de la rama
Criterios de aceptacion: la salida valida contra el esquema; las 14 comprobaciones evaluadas en el orden fijo de
           AUTOMATION_PLAN 16.9 con evidencia concreta; Classification, Disposition y FailureClass coherentes con ellas
Evidencia requerida: ordenes de git reales en el worktree (rev-parse, merge-base --is-ancestor, diff --name-only,
           log --format=%B, check-ignore, show, status --porcelain) y lectura de los TRX; cita en cada Evidence lo que obtuviste
No-touch: todo el repositorio
Condiciones de parada: S-02, S-03, S-04, S-12, S-13, P-03, F2T2-C01..F2T2-C07 y F2T2A-C08..F2T2A-C10 de la delegacion; ante cualquiera,
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
  artifacts/orchestration/I-64/F2-T2A-NAVIGATION-HOST-nc2/2/R20261110T030457Z-b454/inputs/:
  gate-contract.json (contrato ASCII de F2-T2A emitido por las decisiones del Coordinator, AuthorityRevision 09a1499a...);
  delegation.json (paquete de la planificacion R20261109T193332Z-bafe);
  relay-record-planning.json (registro de la planificacion, con la evaluacion mecanica A1-A8 (8 pass) y nc4 en sus Notes);
  equivalence-check.json (comprobacion mecanica de las 20 condiciones de la preautorizacion, hecha por la sesion: 19 true y la 16 false; la aceptacion del Coordinator la da por satisfecha para esta delegacion);
  analysis.md (analisis del intento 1 referido por CorrectionOf; su SHA-256 es CorrectionOf.AnalysisSha256);
  coordinator-disposition.md (transcripciones literales, separadas por lineas =====: aceptacion de la delegacion e137, disposicion BASESHA RECOVERY, autorizacion del intento 2
  y aceptacion de la delegacion R20261109T193332Z-bafe, que autoriza una invocacion del Worker);
  worker-handoff.json (la entrega; hace las veces de la de ExpectedHandoffPath con {WorkRunId} = R20261109T194127Z-31c0);
  relay-record-work.json (registro del trabajo: terminacion, cesion, relevo de entrada, hechos remotos RemoteFacts,
  modelo y effort efectivos del Worker, la senal de denegaciones y los procesos en Notes);
  ci-core/42687835409/core.trx y ci-ui/42687835409/ui.trx de la corrida push del CurrentSha (RemoteFacts.CurrentRun, 42687835409);
  ci-core-red/42687231210/core.trx de la corrida push del RedSha (RemoteFacts.RedRun, 42687231210).
- Esta entrega es la correccion del intento 1 (CorrectionOf R20261109T184844Z-3f3e, FailureClass Ci; 16.8). La delegacion contiene los criterios corregidos.
  Evalua la entrega contra Git y los registros; green-attempt2.patch y la semilla del intento 1 no forman parte de la entrega ni de las entradas.
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
- Revisiones: AuthorityRevision = 09a1499a (la del contrato); BaseSha = 53302b85 (paquete documental de correccion), descendiente que incluye el RED 4fcea652 del intento 1; ChainBaseSha = d9d386e9.
  16.3 exige que AuthorityRevision sea ancestro de BaseSha y que git diff --name-only AuthorityRevision BaseSha no contenga rutas EXTERNAL ni con secciones UNIT_CHANGE; compruebalo.
- Las pruebas y el RED: es una correccion de la cadena: ChainBaseSha = d9d386e9..., ChainRedSha = 4fcea652... y ChainRedFiles = las 7 rutas de la delegacion. La entrega toca ChainRedFiles, asi que EXIGE RED nuevo (16.8), que es el RedSha
  (entre BaseSha y CurrentSha). Aplica los RequiredTests del contrato a los TRX: tests/RackCad.Tests/RackCad.Tests.csproj, FullyQualifiedName~RackCad.Tests.Workspace, MinSelected 656, ExpectRed true (TRX del CurrentSha y, para la parte RED, TRX del RedSha, corrida 42687231210);
  tests/RackCad.UI.Tests/RackCad.UI.Tests.csproj, FullyQualifiedName~RackCad.UI.Tests.Workspace, MinSelected 64, ExpectRed false. El operador ~ de VSTest es "contiene", sin distinguir mayusculas, sobre el nombre completo:
  en el TRX, UnitTest/TestMethod className + "." + name; cuenta los UnitTestResult de esas pruebas. Contrasta con TestResults de la entrega y con el diff. RT = git diff --name-only ChainBaseSha RedSha -- tests/; el GREEN no debe tocar RT ni ChainRedFiles
  (git diff --name-only RedSha CurrentSha). Si el RED queda acreditado (RedPart = pass en Ci y en Tests), ChainRedFiles = ChainRedFiles anterior union RT; registralo en la Evidence de Tests. La CI ejecuta las pruebas en configuracion Debug.
  Los jobs que dependen de otro fallido se omiten en la corrida del RED. El TRX de UI cuenta las pruebas omitidas (Skip) como UnitTestResult outcome=NotExecuted: es la convencion del logger.
- Scope: git diff --name-only BaseSha..CurrentSha contra AllowedWriteScope (15 rutas exactas) y ForbiddenWriteScope (56 entradas) de la delegacion.
- La CI del CurrentSha: los cuatro jobs requeridos de AGENTS.md (RemoteFacts.CurrentRun del registro del trabajo).
- Trailer declarado: el campo Worker.Trailer de la entrega; modelo y effort efectivos: los del registro del trabajo.
- Terminos de gate: los de AUTOMATION_PLAN 16.10, en los campos de texto libre que alli se listan.
- Coherencia (README section 8): EXECUTION_VERIFIED si y solo si las 14 estan en pass (con las 14 en pass, FailureClass NONE);
  una parada se expresa con la comprobacion que la evidencia; FailureClass = la primera que no esta en pass
  (o StopCondition); Disposition segun la precedencia de 16.9 (STOP > BLOCKED > REWORK); con Result pass, RedPart nunca fail.
- TaskId F2-T2A-NAVIGATION-HOST; RunId R20261110T030457Z-b454; DelegationRunId R20261109T193332Z-bafe; WorkRunId R20261109T194127Z-31c0;
  Attempt 2; VerifiedSha = el CurrentSha verificado solo si Classification es EXECUTION_VERIFIED; si no, null.
- Findings: hallazgos concretos y comprobables (desviaciones declaradas, limitaciones de frontera, diferencias entre
  la entrega y Git, y la causa de cada prueba fallida en los TRX); RecommendedNextAction en una frase.
- Toda tu salida en ASCII: sin tildes ni signos no ASCII y sin secuencias de escape formadas por barra invertida + u.
