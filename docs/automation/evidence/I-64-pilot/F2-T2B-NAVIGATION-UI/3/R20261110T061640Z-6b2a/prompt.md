I-64 - DELEGACION F2-T2B-NAVIGATION-UI - Controller, perfil CONTROLLER_VERIFICATION
Identidad: unidad I-64; gate F2; tarea F2-T2B-NAVIGATION-UI; intento 3; RunId R20261110T061640Z-6b2a;
           DelegationRunId R20261110T055138Z-d695; WorkRunId R20261110T055410Z-f863;
           AuthorityRevision 988ccc3d2793b7627372dc58129981cdd884a04c; MainSha 812baf879612320653094a8aaf46aca0bac81383;
           (correccion D-04, CorrectionOf R20261110T053114Z-ffd2) BaseSha y CurrentSha = los de la entrega; rama architecture/workspace-persistente-rackcad;
           worktree = el directorio de trabajo actual; Owner subagent:R20261110T055138Z-d695
Autoridades (leer desde Git, no copiar): las de la delegacion (campo Authorities); en particular
           docs/AUTOMATION_PLAN.md section 16 (16.3 lectura de autoridades, 16.8, 16.9 comprobaciones, 16.10 terminos de gate)
           y docs/automation/agent-execution/README.md section 8 (EXTERNAL); AGENTS.md (EXTERNAL);
           vigencia: AuthorityRevision para la unidad, MainSha para lo demas
Objetivo: verificar la entrega del Worker contra Git, la delegacion, el contrato y los registros del relevo, y emitir la
           verificacion (esquema rackcad-controller-verification/v1)
Alcance: solo lectura. No escribas ningun archivo, no hagas commit ni push. No ejecutes dotnet ni builds: usa los TRX.
Invariantes del Freeze: los 15 de la delegacion, vinculantes en su totalidad; y los criterios de aceptacion de la correccion (T2B-F1, T2B-F2 y T2B-F3)
Rutas y archivos calientes: las entradas de artifacts/orchestration/I-64/F2-T2B-NAVIGATION-UI/3/R20261110T061640Z-6b2a/inputs/ (abajo) y el rango
           BaseSha..CurrentSha de la rama
Criterios de aceptacion: la salida valida contra el esquema; las 14 comprobaciones evaluadas en el orden fijo de
           AUTOMATION_PLAN 16.9 con evidencia concreta; Classification, Disposition y FailureClass coherentes con ellas
Evidencia requerida: ordenes de git reales en el worktree (rev-parse, merge-base --is-ancestor, diff --name-only,
           log --format=%B, check-ignore, show, status --porcelain) y lectura de los TRX; cita en cada Evidence lo que obtuviste
No-touch: todo el repositorio
Condiciones de parada: S-02, S-03, S-04, S-12, S-13, P-03, F2T2-C01..F2T2-C07 y F2T2B-C08..F2T2B-C11 de la delegacion; ante cualquiera,
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
  artifacts/orchestration/I-64/F2-T2B-NAVIGATION-UI/3/R20261110T061640Z-6b2a/inputs/:
  gate-contract.json; delegation.json; relay-record-planning.json (con A1-A8 en sus Notes; nc4 ya satisfecho en la cadena por R20261110T045014Z-131c);
  coordinator-disposition.md (decisiones autonomas de la unidad, bajo autoridad del Owner);
  worker-handoff.json (hace las veces de la de ExpectedHandoffPath con {WorkRunId} = R20261110T055410Z-f863);
  relay-record-work.json (terminacion, cesion, relevo de entrada, hechos remotos RemoteFacts, modelo y effort efectivos del Worker, denegaciones y procesos en Notes);
  ci-core/42766532521/core.trx y ci-ui/42766532521/ui.trx de la corrida push del CurrentSha; ci-core-red/42765320649/core.trx de la corrida push del RedSha (y ci-ui-red/42765320649/ui.trx si existe).
- config.toml: en la salida y en la entrada del relevo el hash es la SNAPSHOT E aceptada (2EEDDAAB..., 104 nombres); no es un STOP de esta verificacion.
- Las senales con actor "sesion -> Controller" de 16.9 (Termination, Remote, Ci, Routing, Denials) salen del registro
  del trabajo; la fecha de la entrega es Outcome.OutputWrittenUtc del registro y el inicio, Cession.StartUtc.
- Semantica del registro del trabajo (README section 3.1 y section 3.4): Exit es el estado ANTES de invocar al Worker (HEAD = remoto = BaseSha) y Entry el estado DESPUES.
- Procesos (16.4): participante es un proceso evaluado cuya linea de ordenes contiene la ruta del worktree. Los no atribuibles que desaparecen en la relectura quedan CLEARED_BY_REREAD; los hechos estan en las Notes del registro del trabajo.
- Autoridades: los valores Section son la transliteracion ASCII de los encabezados reales; localiza cada seccion comparando sin diacriticos, con la raya larga como guion y el signo de seccion como "section".
- Revisiones: AuthorityRevision = 988ccc3d; BaseSha = el de la entrega, hijo directo. 16.3 exige que AuthorityRevision sea ancestro de BaseSha y que git diff --name-only AuthorityRevision BaseSha no contenga rutas EXTERNAL ni con secciones UNIT_CHANGE; compruebalo.
- Las pruebas y el RED: correccion: ChainBaseSha = 2deb0454, ChainRedSha = f64a13b3 y ChainRedFiles (5 rutas) en la delegacion; la entrega toca ChainRedFiles, asi que exige un RED nuevo, que es el RedSha entre BaseSha y CurrentSha y no cambia producto. Aplica los RequiredTests del contrato a los TRX:
  tests/RackCad.UI.Tests/RackCad.UI.Tests.csproj, FullyQualifiedName~RackCad.UI.Tests.Workspace, MinSelected 143, ExpectRed true; tests/RackCad.Tests/RackCad.Tests.csproj, FullyQualifiedName~RackCad.Tests.Workspace, MinSelected 780, ExpectRed false (debe seguir en verde en el RED).
  El operador ~ de VSTest es "contiene", sin distinguir mayusculas, sobre el nombre completo: en el TRX, UnitTest/TestMethod className + "." + name; cuenta los UnitTestResult de esas pruebas. Contrasta con TestResults de la entrega y con el diff.
  RT = git diff --name-only ChainBaseSha RedSha -- tests/; el GREEN no debe tocar RT union ChainRedFiles (git diff --name-only RedSha CurrentSha). Si el RED queda acreditado, ChainRedFiles = la anterior union RT; registralo en la Evidence de Tests. La CI ejecuta las pruebas en configuracion Debug.
  Los jobs que dependen de otro fallido se omiten en la corrida del RED; si el job de UI no llega a correr o no deja TRX en la corrida del RED, la parte RED de UI se acredita con TestResults de la entrega y el diff, y lo anotas. El TRX de UI cuenta las pruebas omitidas (Skip) como UnitTestResult outcome=NotExecuted.
- Scope: git diff --name-only BaseSha..CurrentSha contra AllowedWriteScope (2 rutas exactas) y ForbiddenWriteScope (62 entradas) de la delegacion.
- La CI del CurrentSha: los cuatro jobs requeridos de AGENTS.md (RemoteFacts.CurrentRun del registro del trabajo).
- Trailer declarado: el campo Worker.Trailer de la entrega; modelo y effort efectivos: los del registro del trabajo.
- Terminos de gate: los de AUTOMATION_PLAN 16.10, en los campos de texto libre que alli se listan.
- Coherencia (README section 8): EXECUTION_VERIFIED si y solo si las 14 estan en pass (con las 14 en pass, FailureClass NONE);
  una parada se expresa con la comprobacion que la evidencia; FailureClass = la primera que no esta en pass (o StopCondition); Disposition segun la precedencia de 16.9 (STOP > BLOCKED > REWORK); con Result pass, RedPart nunca fail.
- TaskId F2-T2B-NAVIGATION-UI; RunId R20261110T061640Z-6b2a; DelegationRunId R20261110T055138Z-d695; WorkRunId R20261110T055410Z-f863; Attempt 3; VerifiedSha = el CurrentSha verificado solo si Classification es EXECUTION_VERIFIED; si no, null.
- Findings: hallazgos concretos y comprobables; RecommendedNextAction en una frase.
- Toda tu salida en ASCII: sin tildes ni signos no ASCII y sin secuencias de escape formadas por barra invertida + u.
