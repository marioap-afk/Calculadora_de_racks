I-64 - DELEGACION F1-T3B-HOST-UI - Controller, perfil CONTROLLER_VERIFICATION (re-verificacion excepcional)
Identidad: unidad I-64; gate F1; tarea F1-T3B-HOST-UI; intento 3; RunId R20261011T153749Z-7910;
           DelegationRunId R20261011T081209Z-deae; WorkRunId R20261011T090257Z-3038;
           AuthorityRevision c7f389cbb2eff4be42f861d8114adc2af753a1c9; MainSha bb0d5522e8411f66a51fdfb3f1f0d0514b737453;
           BaseSha y CurrentSha = los de la entrega; rama architecture/workspace-persistente-rackcad;
           worktree = el directorio de trabajo actual; Owner subagent:R20261011T081209Z-deae
Autoridades (leer desde Git, no copiar): las de la delegacion (campo Authorities); en particular
           docs/AUTOMATION_PLAN.md Sec.16 (16.3 lectura de autoridades, 16.8, 16.9 comprobaciones, 16.10 terminos de gate)
           y docs/automation/agent-execution/README.md Sec.8 (EXTERNAL); AGENTS.md (EXTERNAL);
           vigencia: AuthorityRevision para la unidad, MainSha para lo demas
Objetivo: verificar la entrega del Worker contra Git, la delegacion, el contrato y los registros del relevo, y emitir la
           verificacion (esquema rackcad-controller-verification/v1)
Alcance: solo lectura. No escribas ningun archivo, no hagas commit ni push. No ejecutes dotnet ni builds: usa los TRX.
Invariantes del Freeze: INV-F1-T3B-01..16 (los de la delegacion, vinculantes en su totalidad)
Rutas y archivos calientes: las entradas de artifacts/orchestration/I-64/F1-T3B-HOST-UI/3/R20261011T153749Z-7910/inputs/ (abajo) y el rango
           BaseSha..CurrentSha de la rama
Criterios de aceptacion: la salida valida contra el esquema; las 14 comprobaciones evaluadas en el orden fijo de
           AUTOMATION_PLAN 16.9 con evidencia concreta; Classification, Disposition y FailureClass coherentes con ellas
Evidencia requerida: ordenes de git reales en el worktree (rev-parse, merge-base --is-ancestor, diff --name-only,
           log --format=%B, check-ignore, show) y lectura de los TRX; cita en cada Evidence lo que obtuviste
No-touch: todo el repositorio
Condiciones de parada: S-02, S-03, S-04, S-06, S-12, S-13, P-03, C-01..C-20 y C-T3-INSTRUMENTATION de la delegacion; ante
           cualquiera, Classification EXECUTION_BLOCKED con Disposition STOP y el id en TriggeredStopConditions
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
  artifacts/orchestration/I-64/F1-T3B-HOST-UI/3/R20261011T153749Z-7910/inputs/:
  gate-contract.json (contrato de gate de F1-T3B-HOST-UI emitido por orden del Coordinator, ASCII, AuthorityRevision c7f389cb...);
  delegation.json (paquete aceptado, RunId R20261011T081209Z-deae);
  relay-record-planning.json (registro de la planificacion, con la evaluacion mecanica A1-A8 y nc4 en sus Notes);
  coordinator-acceptance.md (transcripcion literal de la aceptacion formal del Coordinator: ACCEPTED, A1-A8 PASS, nc4 PASS, un
  unico Worker; fija ademas que ActiveSessionChanged DEBE ser internal; se versionara en la custodia);
  coordinator-disposition.md (transcripcion literal de la disposicion del Coordinator sobre la parada de la primera
  verificacion: autoriza esta re-verificacion excepcional y fija la excepcion T3B-SCOPE-NONSEMANTIC-WHITESPACE-01; entrada
  autoritativa suplementaria; se versionara en la custodia);
  worker-handoff.json (la entrega; hace las veces de la de ExpectedHandoffPath con {WorkRunId} = R20261011T090257Z-3038);
  relay-record-work.json (registro del trabajo: terminacion, cesion, relevo de entrada, hechos remotos RemoteFacts,
  modelo y effort efectivos del Worker, la senal de denegaciones y los procesos en Notes);
  ci-core/38129212761/core.trx y ci-ui/38129212761/ui.trx de la corrida push del CurrentSha (RemoteFacts.CurrentRun, 38129212761);
  ci-core-red/38128156191/core.trx y ci-ui-red/38128156191/ui.trx de la corrida push del RedSha (RemoteFacts.RedRun, 38128156191).
- Disposicion del Coordinator (coordinator-disposition.md): esta es la unica re-verificacion excepcional que autoriza, sobre el
  mismo producto (CurrentSha e0205b5a9f000bbd44aa1f7556d30f06411c3db7), el mismo contrato, la misma delegacion y las mismas
  evidencias RED/GREEN. Su unico efecto sobre las comprobaciones es la excepcion T3B-SCOPE-NONSEMANTIC-WHITESPACE-01 en Scope
  (abajo); las partes dirigidas a la sesion (controles, custodia, commit, parada) no son instrucciones para ti. No presupone
  el resultado de ninguna comprobacion: evalua las 14 de forma independiente con la evidencia.
- config.toml: en la salida y en la entrada del relevo el hash pertenece al conjunto cerrado A/B/C que el Owner/Coordinator
  acepto para esta cadena, con el inventario de 105 nombres; no es un STOP de esta verificacion.
- Las senales con actor "sesion -> Controller" de 16.9 (Termination, Remote, Ci, Routing, Denials) salen del registro
  del trabajo; la fecha de la entrega es Outcome.OutputWrittenUtc del registro y el inicio, Cession.StartUtc.
- Semantica del registro del trabajo (README Sec.3.1 y Sec.3.4): Exit es el estado ANTES de invocar al Worker (HEAD = remoto =
  BaseSha) y Entry el estado DESPUES; que HEAD y ls-remote pasen de BaseSha a CurrentSha entre ambos es lo esperado,
  porque el Worker hace commit y push durante la cesion. No es contradiccion.
- Procesos (16.4): participante es un proceso evaluado cuya linea de ordenes contiene la ruta del worktree. Los procesos no
  atribuibles que desaparecen en la relectura quedan CLEARED_BY_REREAD por decision del Coordinator de I-64 sobre P-02. Los
  hechos de procesos de este relevo (incluido un huerfano de otra sesion, posterior al fin de la cesion) estan en las Notes del
  registro del trabajo; evalualos con esas reglas.
- Autoridades: los valores Section de Authorities en el contrato y en la delegacion son la transliteracion ASCII de los
  encabezados reales (en Git los encabezados conservan sus tildes y la raya larga). Al leer las autoridades (16.3), localiza
  cada seccion por su encabezado comparando sin diacriticos y con la raya larga como guion; esa diferencia de representacion
  no es una discrepancia. Contract compara delegacion y contrato, ambos ASCII.
- Revisiones: AuthorityRevision = c7f389cb (commit de la decision del Coordinator) y BaseSha = ChainBaseSha = f3a9f40e (su hijo:
  revision del paquete NOT EMITTED, evidencia Sec.55 y estado). 16.3 exige que AuthorityRevision sea ancestro de BaseSha y que
  git diff --name-only AuthorityRevision BaseSha no contenga rutas EXTERNAL ni con secciones UNIT_CHANGE; compruebalo.
- Las pruebas y el RED: es la primera delegacion de la cadena: ChainBaseSha = BaseSha = f3a9f40e..., ChainRedSha = null y
  ChainRedFiles = [], asi que esta entrega EXIGE RED (16.8) en los DOS proyectos requeridos. RedSha = 9ac76ee2... (entre
  BaseSha y CurrentSha). Aplica cada RequiredTests del contrato (UI: FullyQualifiedName~RackCad.UI.Tests.Workspace, MinSelected 1;
  Core: FullyQualifiedName~RackCad.Tests.Workspace, MinSelected 487) a los TRX del CurrentSha (seleccion, superadas, fallidas) y a
  los TRX del RedSha (la parte RED: seleccion >= minimo y fallos por asercion de comportamiento de T3B; la corrida del RedSha
  falla en los jobs de pruebas y salta el del Plugin, lo esperable en un RED), y contrasta con TestResults de la entrega y con
  el diff. RT = git diff --name-only ChainBaseSha RedSha -- tests/; el GREEN no debe tocar RT (16.8). Si el RED queda acreditado
  (RedPart = pass en Ci y en Tests), ChainRedFiles = RT; registralo en la Evidence de Tests. Los TRX de UI cuentan las pruebas
  omitidas (Skip) como UnitTestResult outcome=NotExecuted, en total y no en executed: es la convencion del logger, no una
  contradiccion. La CI ejecuta las pruebas en configuracion Debug; la entrega declara ademas corridas locales en Release.
- Scope: git diff --name-only BaseSha..CurrentSha contra AllowedWriteScope (8 entradas) y ForbiddenWriteScope de la delegacion.
  WorkspacePanelCommand.cs, AutoCadEventBridge.cs y WorkspaceController.cs solo admiten sus deltas exactos (INV-F1-T3B-03, 04 y
  05) y WorkspaceHostSourceGuardTests.cs solo G1/G2/G3 (INV-F1-T3B-14): comprueba con git diff BaseSha..CurrentSha -- <archivo>
  lo que cambia en cada uno. src/RackCad.UI/Workspace/ y tests/RackCad.UI.Tests/Workspace/ solo admiten archivos nuevos.
  En WorkspaceHostSourceGuardTests.cs aplica la disposicion del Coordinator asi: (1) conserva el diff exacto original
  (git diff BaseSha..CurrentSha -- <archivo>, sin ignorar espacios); (2) identifica en el el delta de espacios de la linea
  de la constante HostCorePath; (3) aplica T3B-SCOPE-NONSEMANTIC-WHITESPACE-01 solo a ese delta exacto (la linea base y la
  linea producto que cita la disposicion, byte a byte); (4) exige que toda otra linea cambiada de ese archivo y todo otro
  archivo cambiado cumplan el contrato original (G1/G2/G3, deltas exactos, AllowedWriteScope y ForbiddenWriteScope). La
  excepcion no es una dispensa general de espacios, no amplia AllowedWriteScope y no autoriza ninguna otra linea.
- La CI del CurrentSha: los cuatro jobs requeridos de AGENTS.md (RemoteFacts.CurrentRun del registro del trabajo).
- Trailer declarado: el campo Worker.Trailer de la entrega; modelo y effort efectivos: los del registro del trabajo.
- Terminos de gate: los de AUTOMATION_PLAN 16.10, en los campos de texto libre que alli se listan.
- Coherencia (README Sec.8): EXECUTION_VERIFIED si y solo si las 14 estan en pass (con las 14 en pass, FailureClass NONE);
  una parada se expresa con la comprobacion que la evidencia; FailureClass = la primera que no esta en pass
  (o StopCondition); Disposition segun la precedencia de 16.9 (STOP > BLOCKED > REWORK); con Result pass, RedPart nunca fail.
- TaskId F1-T3B-HOST-UI; RunId R20261011T153749Z-7910; DelegationRunId R20261011T081209Z-deae; WorkRunId R20261011T090257Z-3038;
  Attempt 3; VerifiedSha = el CurrentSha verificado solo si Classification es EXECUTION_VERIFIED; si no, null.
- Findings: hallazgos concretos y comprobables (desviaciones declaradas, limitaciones de frontera, diferencias entre
  la entrega y Git, y la causa de cada prueba fallida en los TRX); RecommendedNextAction en una frase.
- Toda tu salida en ASCII: sin tildes ni signos no ASCII y sin secuencias de escape formadas por barra invertida + u.
