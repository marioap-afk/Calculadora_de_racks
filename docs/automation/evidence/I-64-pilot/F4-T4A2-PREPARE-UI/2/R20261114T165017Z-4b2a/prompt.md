I-64 - DELEGACION F4-T4A2-PREPARE-UI - Controller, perfil CONTROLLER_VERIFICATION
Identidad: unidad I-64; gate F4; tarea F4-T4A2-PREPARE-UI; intento 2; RunId R20261114T165017Z-4b2a;
           DelegationRunId R20261114T161620Z-f15d; WorkRunId R20261114T162137Z-b48d;
           AuthorityRevision 192103f845ad3143fdc8add9aa4239babddd17ee; MainSha 812baf879612320653094a8aaf46aca0bac81383;
           BaseSha y CurrentSha = los de la entrega; rama architecture/workspace-persistente-rackcad;
           worktree = el directorio de trabajo actual; Owner subagent:R20261114T161620Z-f15d
Autoridades (leer desde Git, no copiar): las de la delegacion (campo Authorities); en particular
           docs/AUTOMATION_PLAN.md section 16 (16.3 lectura de autoridades, 16.8, 16.9 comprobaciones, 16.10 terminos de gate)
           y docs/automation/agent-execution/README.md section 8 (EXTERNAL); AGENTS.md (EXTERNAL);
           vigencia: AuthorityRevision para la unidad, MainSha para lo demas
Objetivo: verificar la entrega del Worker contra Git, la delegacion, el contrato y los registros del relevo, y emitir la
           verificacion (esquema rackcad-controller-verification/v1)
Alcance: solo lectura. No escribas ningun archivo, no hagas commit ni push. No ejecutes dotnet ni builds: usa los TRX.
Invariantes del Freeze: los 18 de la delegacion, vinculantes en su totalidad
Rutas y archivos calientes: las entradas de artifacts/orchestration/I-64/F4-T4A2-PREPARE-UI/2/R20261114T165017Z-4b2a/inputs/ (abajo) y el rango
           BaseSha..CurrentSha de la rama
Criterios de aceptacion: la salida valida contra el esquema; las 14 comprobaciones evaluadas en el orden fijo de
           AUTOMATION_PLAN 16.9 con evidencia concreta; Classification, Disposition y FailureClass coherentes con ellas
Evidencia requerida: ordenes de git reales en el worktree (rev-parse, merge-base --is-ancestor, diff --name-only,
           log --format=%B, check-ignore, show, status --porcelain) y lectura de los TRX; cita en cada Evidence lo que obtuviste
No-touch: todo el repositorio
Condiciones de parada: S-02, S-03, S-04, S-12, S-13, P-03 y F4T4A2-C01..F4T4A2-C07 y F4T4A2-C09 de la delegacion; ante cualquiera,
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
  artifacts/orchestration/I-64/F4-T4A2-PREPARE-UI/2/R20261114T165017Z-4b2a/inputs/:
  gate-contract.json; delegation.json; relay-record-planning.json (con A1-A8 y nc4 en sus Notes);
  worker-handoff.json (hace las veces de la de ExpectedHandoffPath con {WorkRunId} = R20261114T162137Z-b48d);
  relay-record-work.json (terminacion, cesion, relevo de entrada, hechos remotos RemoteFacts, modelo y effort efectivos del Worker, denegaciones y procesos en Notes);
  ci-core/43617317131/core.trx y ci-ui/43617317131/ui.trx de la corrida push del CurrentSha; ci-core-red/43616238873/core.trx y ci-ui-red/43616238873/ui.trx de la corrida push del RedSha.
  byte-fidelity-check.json: la comparacion determinista byte a byte de las colecciones de la delegacion con el contrato, hecha por la sesion antes de aceptar (PASS en las ocho); la planificacion paso A1-A8 8/8 y nc4 (en las Notes de relay-record-planning.json).
- config.toml: en la salida y en la entrada del relevo el hash es la SNAPSHOT H aceptada por el Owner, docs/automation/evidence/I-64-pilot/F4-DRAFTS/f4-dispositions.md, F4-E46 (A859B66C..., owner-f4-snapshot-h-decision.md); no es un STOP de esta verificacion.
- Las senales con actor "sesion -> Controller" de 16.9 (Termination, Remote, Ci, Routing, Denials) salen del registro
  del trabajo; la fecha de la entrega es Outcome.OutputWrittenUtc del registro y el inicio, Cession.StartUtc.
- Semantica del registro del trabajo (README section 3.1 y section 3.4): Exit es el estado ANTES de invocar al Worker (HEAD = remoto = BaseSha) y Entry el estado DESPUES.
- Routing y transporte subagent: el modelo solicitado es el campo Model de la delegacion. El transporte subagent solo admite como parametro de lanzamiento el alias de familia, y Participant.ModelRequested del registro del trabajo guarda ese parametro literal; el modelo real de cada turno esta en Participant.ModelEffective y EffectiveSource.
- Procesos (16.4): participante es un proceso evaluado cuya linea de ordenes contiene la ruta del worktree. Los no atribuibles que desaparecen en la relectura quedan CLEARED_BY_REREAD; los hechos estan en las Notes del registro del trabajo.
- Autoridades: los valores Section son la transliteracion ASCII de los encabezados reales; localiza cada seccion comparando sin diacriticos, con la raya larga como guion y el signo de seccion como "section".
- Revisiones: AuthorityRevision = 192103f8; BaseSha = el de la entrega, el commit de emision del contrato, HIJO DIRECTO de la AuthorityRevision. 16.3 exige que AuthorityRevision sea ancestro de BaseSha y que git diff --name-only AuthorityRevision BaseSha no contenga rutas EXTERNAL ni con secciones UNIT_CHANGE; compruebalo. Hecho medido por la sesion en la emision del intento 2: ese diff tiene tres rutas, todas bajo docs/ de la unidad: el contrato del intento 2 (gate-contract-attempt2.json), el estado y la evidencia, que figuran en Authorities como UNIT_DOC. Si el diff que mides no coincide con este hecho, comprueba igualmente la regla de 16.3 sobre el diff real (solo rutas de docs/ de la unidad, ni EXTERNAL ni UNIT_CHANGE) y no tomes la diferencia de recuento como fallo por si sola.
- Las pruebas y el RED: CORRECCION (intento 2): ChainBaseSha = 4eb7e9247c3ad942feea1b6a695d7ad79377a4ee y ChainRedSha = 1fe36cf9cddc2bb67f1795175c2e818e6569e177 en la delegacion, con ChainRedFiles = las 10 rutas del RED del intento 1; la entrega exige el RED de la correccion, que es el RedSha entre BaseSha y CurrentSha. Aplica el RequiredTests del contrato a los TRX:
  tests/RackCad.Tests/RackCad.Tests.csproj, FullyQualifiedName~RackCad.Tests.Workspace, MinSelected 2843, ExpectRed false (TRX ci-core); con ExpectRed false esta prueba no tiene que fallar en el RED; debe pasar en el CurrentSha.
  tests/RackCad.UI.Tests/RackCad.UI.Tests.csproj, FullyQualifiedName~RackCad.UI.Tests.Workspace, MinSelected 458, ExpectRed true (TRX ci-ui); con ExpectRed true debe fallar en el RED por asercion y pasar en el CurrentSha.
  El operador ~ de VSTest es "contiene", sin distinguir mayusculas, sobre el nombre completo: en el TRX, UnitTest/TestMethod className + "." + name; cuenta los UnitTestResult de esas pruebas. Contrasta con TestResults de la entrega y con el diff.
  RT = git diff --name-only ChainBaseSha RedSha -- tests/; el GREEN no debe tocar RT (git diff --name-only RedSha CurrentSha). Si el RED queda acreditado, ChainRedFiles = las 10 rutas anteriores union RT; registralo en la Evidence de Tests. La CI ejecuta las pruebas en configuracion Debug.
- Scope: git diff --name-only BaseSha..CurrentSha contra AllowedWriteScope (archivos exactos, sin prefijos; cuenta y lee sus entradas en el delegation.json de las entradas, no en el contrato) y ForbiddenWriteScope (prefijos seguros, F4-E21) de ese mismo delegation.json. Toda ruta cambiada debe estar cubierta por AllowedWriteScope, que es la lista blanca exacta y primaria; una ruta fuera de ella es fail aunque no figure en ForbiddenWriteScope.
- La CI del CurrentSha: los cuatro jobs requeridos de AGENTS.md (RemoteFacts.CurrentRun del registro del trabajo).
- Trailer declarado: el campo Worker.Trailer de la entrega; modelo y effort efectivos: los del registro del trabajo.
- Terminos de gate: los de AUTOMATION_PLAN 16.10, en los campos de texto libre que alli se listan.
- Coherencia (README section 8): EXECUTION_VERIFIED si y solo si las 14 estan en pass (con las 14 en pass, FailureClass NONE);
  una parada se expresa con la comprobacion que la evidencia; FailureClass = la primera que no esta en pass (o StopCondition); Disposition segun la precedencia de 16.9 (STOP > BLOCKED > REWORK); con Result pass, RedPart nunca fail.
- TaskId F4-T4A2-PREPARE-UI; RunId R20261114T165017Z-4b2a; DelegationRunId R20261114T161620Z-f15d; WorkRunId R20261114T162137Z-b48d; Attempt 2; VerifiedSha = el CurrentSha verificado solo si Classification es EXECUTION_VERIFIED; si no, null.
- Red: no ejecutes git ls-remote ni ninguna operacion Git de red (tu sandbox no tiene red); los hechos del remoto estan en RemoteFacts del registro del trabajo, medidos por la sesion.
- Findings: hallazgos concretos y comprobables; RecommendedNextAction en una frase.
- Toda tu salida en ASCII: sin tildes ni signos no ASCII y sin secuencias de escape formadas por barra invertida + u.
