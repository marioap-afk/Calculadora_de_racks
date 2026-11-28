I-64 - DELEGACION F4-T2B1-INTENT-READ - Controller, perfil CONTROLLER_VERIFICATION
Identidad: unidad I-64; gate F4; tarea F4-T2B1-INTENT-READ; intento 1; RunId R20261112T084307Z-16bc;
           DelegationRunId R20261112T081014Z-84ff; WorkRunId R20261112T081315Z-4a75;
           AuthorityRevision d6587a06803ab618698cbd342f38e2cf31f87af7; MainSha 812baf879612320653094a8aaf46aca0bac81383;
           BaseSha y CurrentSha = los de la entrega; rama architecture/workspace-persistente-rackcad;
           worktree = el directorio de trabajo actual; Owner subagent:R20261112T081014Z-84ff
Autoridades (leer desde Git, no copiar): las de la delegacion (campo Authorities); en particular
           docs/AUTOMATION_PLAN.md section 16 (16.3 lectura de autoridades, 16.8, 16.9 comprobaciones, 16.10 terminos de gate)
           y docs/automation/agent-execution/README.md section 8 (EXTERNAL); AGENTS.md (EXTERNAL);
           vigencia: AuthorityRevision para la unidad, MainSha para lo demas
Objetivo: verificar la entrega del Worker contra Git, la delegacion, el contrato y los registros del relevo, y emitir la
           verificacion (esquema rackcad-controller-verification/v1)
Alcance: solo lectura. No escribas ningun archivo, no hagas commit ni push. No ejecutes dotnet ni builds: usa los TRX.
Invariantes del Freeze: los 14 de la delegacion, vinculantes en su totalidad
Rutas y archivos calientes: las entradas de artifacts/orchestration/I-64/F4-T2B1-INTENT-READ/1/R20261112T084307Z-16bc/inputs/ (abajo) y el rango
           BaseSha..CurrentSha de la rama
Criterios de aceptacion: la salida valida contra el esquema; las 14 comprobaciones evaluadas en el orden fijo de
           AUTOMATION_PLAN 16.9 con evidencia concreta; Classification, Disposition y FailureClass coherentes con ellas
Evidencia requerida: ordenes de git reales en el worktree (rev-parse, merge-base --is-ancestor, diff --name-only,
           log --format=%B, check-ignore, show, status --porcelain) y lectura de los TRX; cita en cada Evidence lo que obtuviste
No-touch: todo el repositorio
Condiciones de parada: S-02, S-03, S-04, S-12, S-13, P-03 y F4T2B1-C01..F4T2B1-C07 de la delegacion; ante cualquiera,
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
  artifacts/orchestration/I-64/F4-T2B1-INTENT-READ/1/R20261112T084307Z-16bc/inputs/:
  gate-contract.json; delegation.json; relay-record-planning.json (con A1-A8 y nc4 en sus Notes);
  worker-handoff.json (hace las veces de la de ExpectedHandoffPath con {WorkRunId} = R20261112T081315Z-4a75);
  relay-record-work.json (terminacion, cesion, relevo de entrada, hechos remotos RemoteFacts, modelo y effort efectivos del Worker, denegaciones y procesos en Notes);
  ci-core/43184046954/core.trx y ci-ui/43184046954/ui.trx de la corrida push del CurrentSha; ci-core-red/43183114141/core.trx y ci-ui-red/43183114141/ui.trx de la corrida push del RedSha.
  byte-fidelity-check.json: la comparacion determinista byte a byte de las colecciones de la delegacion con el contrato, hecha por la sesion antes de aceptar (PASS en las ocho); la planificacion paso A1-A8 8/8 y nc4 (en las Notes de relay-record-planning.json).
- config.toml: en la salida y en la entrada del relevo el hash es la SNAPSHOT G aceptada por el Owner, docs/automation/evidence/I-64-pilot/F4-DRAFTS/f4-dispositions.md, F4-E13 (7C1516A9..., 104 nombres); no es un STOP de esta verificacion.
- Las senales con actor "sesion -> Controller" de 16.9 (Termination, Remote, Ci, Routing, Denials) salen del registro
  del trabajo; la fecha de la entrega es Outcome.OutputWrittenUtc del registro y el inicio, Cession.StartUtc.
- Semantica del registro del trabajo (README section 3.1 y section 3.4): Exit es el estado ANTES de invocar al Worker (HEAD = remoto = BaseSha) y Entry el estado DESPUES.
- Routing y transporte subagent: el modelo solicitado es el campo Model de la delegacion. El transporte subagent solo admite como parametro de lanzamiento el alias de familia, y Participant.ModelRequested del registro del trabajo guarda ese parametro literal; el modelo real de cada turno esta en Participant.ModelEffective y EffectiveSource.
- Procesos (16.4): participante es un proceso evaluado cuya linea de ordenes contiene la ruta del worktree. Los no atribuibles que desaparecen en la relectura quedan CLEARED_BY_REREAD; los hechos estan en las Notes del registro del trabajo.
- Autoridades: los valores Section son la transliteracion ASCII de los encabezados reales; localiza cada seccion comparando sin diacriticos, con la raya larga como guion y el signo de seccion como "section".
- Revisiones: AuthorityRevision = d6587a06; BaseSha = el de la entrega, hijo directo. 16.3 exige que AuthorityRevision sea ancestro de BaseSha y que git diff --name-only AuthorityRevision BaseSha no contenga rutas EXTERNAL ni con secciones UNIT_CHANGE; compruebalo. Las tres rutas de ese diff estan bajo docs/: el contrato, el estado y la evidencia, que figuran en Authorities como UNIT_DOC.
- Las pruebas y el RED: primera delegacion de la cadena: ChainBaseSha = BaseSha y ChainRedSha = null en la delegacion; la entrega exige RED, que es el RedSha entre BaseSha y CurrentSha. Aplica el RequiredTests del contrato a los TRX:
  tests/RackCad.Tests/RackCad.Tests.csproj, FullyQualifiedName~RackCad.Tests.Workspace, MinSelected 1468, ExpectRed true (TRX ci-core);
  tests/RackCad.UI.Tests/RackCad.UI.Tests.csproj, FullyQualifiedName~RackCad.UI.Tests.Workspace, MinSelected 248, ExpectRed false (TRX ci-ui): con ExpectRed false esta prueba no tiene que fallar en el RED; debe pasar en el CurrentSha.
  El operador ~ de VSTest es "contiene", sin distinguir mayusculas, sobre el nombre completo: en el TRX, UnitTest/TestMethod className + "." + name; cuenta los UnitTestResult de esas pruebas. Contrasta con TestResults de la entrega y con el diff.
  RT = git diff --name-only ChainBaseSha RedSha -- tests/; el GREEN no debe tocar RT (git diff --name-only RedSha CurrentSha). Si el RED queda acreditado, ChainRedFiles = RT; registralo en la Evidence de Tests. La CI ejecuta las pruebas en configuracion Debug.
- Scope: git diff --name-only BaseSha..CurrentSha contra AllowedWriteScope (7 entradas: seis archivos exactos y el prefijo de fixtures nuevo) y ForbiddenWriteScope (58 entradas, prefijos seguros, F4-E21) de la delegacion. Toda ruta cambiada debe estar cubierta por AllowedWriteScope, que es la lista blanca exacta y primaria; una ruta fuera de ella es fail aunque no figure en ForbiddenWriteScope.
- La CI del CurrentSha: los cuatro jobs requeridos de AGENTS.md (RemoteFacts.CurrentRun del registro del trabajo).
- Trailer declarado: el campo Worker.Trailer de la entrega; modelo y effort efectivos: los del registro del trabajo.
- Terminos de gate: los de AUTOMATION_PLAN 16.10, en los campos de texto libre que alli se listan.
- Coherencia (README section 8): EXECUTION_VERIFIED si y solo si las 14 estan en pass (con las 14 en pass, FailureClass NONE);
  una parada se expresa con la comprobacion que la evidencia; FailureClass = la primera que no esta en pass (o StopCondition); Disposition segun la precedencia de 16.9 (STOP > BLOCKED > REWORK); con Result pass, RedPart nunca fail.
- TaskId F4-T2B1-INTENT-READ; RunId R20261112T084307Z-16bc; DelegationRunId R20261112T081014Z-84ff; WorkRunId R20261112T081315Z-4a75; Attempt 1; VerifiedSha = el CurrentSha verificado solo si Classification es EXECUTION_VERIFIED; si no, null.
- Red: no ejecutes git ls-remote ni ninguna operacion Git de red (tu sandbox no tiene red); los hechos del remoto estan en RemoteFacts del registro del trabajo, medidos por la sesion.
- Findings: hallazgos concretos y comprobables; RecommendedNextAction en una frase.
- Toda tu salida en ASCII: sin tildes ni signos no ASCII y sin secuencias de escape formadas por barra invertida + u.
