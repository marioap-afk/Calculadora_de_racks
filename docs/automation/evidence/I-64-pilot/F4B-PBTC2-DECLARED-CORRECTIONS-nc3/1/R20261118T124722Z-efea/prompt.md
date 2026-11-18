I-64 - DELEGACION F4B-PBTC2-DECLARED-CORRECTIONS - Controller, perfil CONTROLLER_VERIFICATION
Identidad: unidad I-64; gate F4B; tarea F4B-PBTC2-DECLARED-CORRECTIONS; intento 1; RunId R20261118T124722Z-efea;
           DelegationRunId R20261118T055036Z-c73e; WorkRunId R20261118T113927Z-037c;
           AuthorityRevision 8708d28fa19ba912e39da5ae55c14affb1e53651; MainSha 812baf879612320653094a8aaf46aca0bac81383;
           BaseSha y CurrentSha = los de la entrega; rama architecture/workspace-persistente-rackcad;
           worktree = el directorio de trabajo actual; Owner subagent:R20261118T055036Z-c73e
Autoridades (leer desde Git, no copiar): las de la delegacion (campo Authorities); en particular
           docs/AUTOMATION_PLAN.md section 16 (16.3 lectura de autoridades, 16.8, 16.9 comprobaciones, 16.10 terminos de gate)
           y docs/automation/agent-execution/README.md section 8 (EXTERNAL); AGENTS.md (EXTERNAL);
           vigencia: AuthorityRevision para la unidad, MainSha para lo demas
Objetivo: verificar la entrega del Worker contra Git, la delegacion, el contrato y los registros del relevo, y emitir la
           verificacion (esquema rackcad-controller-verification/v1)
Alcance: solo lectura. No escribas ningun archivo, no hagas commit ni push. No ejecutes dotnet ni builds: usa los TRX.
Invariantes del Freeze: los 14 de la delegacion, vinculantes en su totalidad
Rutas y archivos calientes: las entradas de artifacts/orchestration/I-64/F4B-PBTC2-DECLARED-CORRECTIONS-nc3/1/R20261118T124722Z-efea/inputs/ (abajo) y el rango
           BaseSha..CurrentSha de la rama
Criterios de aceptacion: la salida valida contra el esquema; las 14 comprobaciones evaluadas en el orden fijo de
           AUTOMATION_PLAN 16.9 con evidencia concreta; Classification, Disposition y FailureClass coherentes con ellas
Evidencia requerida: ordenes de git reales en el worktree (rev-parse, merge-base --is-ancestor, diff --name-only,
           log --format=%B, check-ignore, show, status --porcelain) y lectura de los TRX; cita en cada Evidence lo que obtuviste
No-touch: todo el repositorio
Condiciones de parada: S-02, S-03, S-04, S-12, S-13, P-03, PBTC2-C01..PBTC2-C07, PBTC2-C09 y PBTC2-C10 de la delegacion; ante cualquiera,
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
  artifacts/orchestration/I-64/F4B-PBTC2-DECLARED-CORRECTIONS-nc3/1/R20261118T124722Z-efea/inputs/:
  gate-contract.json; delegation.json; relay-record-planning.json (con A1-A8 y nc4 en sus Notes);
  worker-handoff.json (hace las veces de la de ExpectedHandoffPath con {WorkRunId} = R20261118T113927Z-037c);
  relay-record-work.json (terminacion, cesion, relevo de entrada, hechos remotos RemoteFacts, modelo y effort efectivos del Worker, denegaciones y procesos en Notes);
  ci-core/44300874797/core.trx y ci-ui/44300874797/ui.trx de la corrida push del CurrentSha; ci-core-red/44298811020/core.trx y ci-ui-red/44298811020/ui.trx de la corrida push del RedSha.
  byte-fidelity-check.json: la comparacion determinista byte a byte de las colecciones de la delegacion con el contrato, hecha por la sesion antes de aceptar (PASS en las ocho); la planificacion paso A1-A8 8/8 y nc4 (en las Notes de relay-record-planning.json).
  coordinator-disposition.md: disposicion del Owner sobre un punto declarado de la entrega; es autoridad para ese punto y, solo en lo que ratifica, prevalece sobre la letra del contrato para este intento.
- config.toml: en la salida y en la entrada del relevo el hash es la SNAPSHOT I (0B47701A...), aceptada por el Owner tras la sonda neutra R20261114T204534Z-6603 en PASS, docs/automation/evidence/I-64-pilot/F4-DRAFTS/f4-dispositions.md, F4-E57 (F4-ENVIRONMENT/snapshot-I/snapshot-i.json); no es un STOP de esta verificacion.
- Las senales con actor "sesion -> Controller" de 16.9 (Termination, Remote, Ci, Routing, Denials) salen del registro
  del trabajo; la fecha de la entrega es Outcome.OutputWrittenUtc del registro y el inicio, Cession.StartUtc.
- Semantica del registro del trabajo (README section 3.1 y section 3.4): Exit es el estado ANTES de invocar al Worker (HEAD = remoto = BaseSha) y Entry el estado DESPUES.
- Routing y transporte subagent: el modelo solicitado es el campo Model de la delegacion. El transporte subagent solo admite como parametro de lanzamiento el alias de familia, y Participant.ModelRequested del registro del trabajo guarda ese parametro literal; el modelo real de cada turno esta en Participant.ModelEffective y EffectiveSource.
- Procesos (16.4): participante es un proceso evaluado cuya linea de ordenes contiene la ruta del worktree. Los no atribuibles que desaparecen en la relectura quedan CLEARED_BY_REREAD; los hechos estan en las Notes del registro del trabajo.
- Autoridades: los valores Section son la transliteracion ASCII de los encabezados reales; localiza cada seccion comparando sin diacriticos, con la raya larga como guion y el signo de seccion como "section".
- Revisiones: AuthorityRevision = 8708d28f; BaseSha = el de la entrega, el commit de emision del contrato, HIJO DIRECTO de la AuthorityRevision. 16.3 exige que AuthorityRevision sea ancestro de BaseSha y que git diff --name-only AuthorityRevision BaseSha no contenga rutas EXTERNAL ni con secciones UNIT_CHANGE; compruebalo. Hecho medido por la sesion en la emision: ese diff tiene cuatro rutas, todas bajo docs/ de la unidad: el contrato y su script generador (docs/automation/evidence/I-64-pilot/F4B-PBTC2-DECLARED-CORRECTIONS/coordinator/), el estado y la evidencia; el contrato, el estado y la evidencia figuran en Authorities como UNIT_DOC y el script generador no figura (no es EXTERNAL ni UNIT_CHANGE). Si el diff que mides no coincide con este hecho, comprueba igualmente la regla de 16.3 sobre el diff real (solo rutas de docs/ de la unidad, ni EXTERNAL ni UNIT_CHANGE) y no tomes la diferencia de recuento como fallo por si sola.
- Las pruebas y el RED: primera delegacion de la cadena: ChainBaseSha = BaseSha y ChainRedSha = null en la delegacion; la entrega exige RED, que es el RedSha entre BaseSha y CurrentSha. Aplica el RequiredTests del contrato a los TRX:
  tests/RackCad.UI.Tests/RackCad.UI.Tests.csproj, FullyQualifiedName~RackCad.UI.Tests.PushBackDeclaredCorrection2, MinSelected 4, ExpectRed true (TRX ci-ui); con ExpectRed true debe fallar en el RED por asercion y pasar en el CurrentSha.
  tests/RackCad.UI.Tests/RackCad.UI.Tests.csproj, FullyQualifiedName~RackCad.UI.Tests.PushBackDeclaredCorrectionRoundingTests, MinSelected 3, ExpectRed false (TRX ci-ui); con ExpectRed false esta prueba no tiene que fallar en el RED; debe pasar en el CurrentSha.
  tests/RackCad.UI.Tests/RackCad.UI.Tests.csproj, FullyQualifiedName~RackCad.UI.Tests.PushBackDeclaredCorrectionDimensionStyleTests, MinSelected 2, ExpectRed false (TRX ci-ui); con ExpectRed false esta prueba no tiene que fallar en el RED; debe pasar en el CurrentSha.
  tests/RackCad.UI.Tests/RackCad.UI.Tests.csproj, FullyQualifiedName~RackCad.UI.Tests.PushBackDeclaredCorrectionPilotParityTests, MinSelected 2, ExpectRed false (TRX ci-ui); con ExpectRed false esta prueba no tiene que fallar en el RED; debe pasar en el CurrentSha.
  tests/RackCad.UI.Tests/RackCad.UI.Tests.csproj, FullyQualifiedName~RackCad.UI.Tests.PushBackDeclaredCorrectionSideBNoticeTests, MinSelected 2, ExpectRed false (TRX ci-ui); con ExpectRed false esta prueba no tiene que fallar en el RED; debe pasar en el CurrentSha.
  tests/RackCad.UI.Tests/RackCad.UI.Tests.csproj, FullyQualifiedName~RackCad.UI.Tests.PushBackDeclaredCorrectionStandardRestoreTests, MinSelected 1, ExpectRed false (TRX ci-ui); con ExpectRed false esta prueba no tiene que fallar en el RED; debe pasar en el CurrentSha.
  tests/RackCad.UI.Tests/RackCad.UI.Tests.csproj, FullyQualifiedName~RackCad.UI.Tests.PushBackFullEditorCharacterization, MinSelected 16, ExpectRed false (TRX ci-ui); con ExpectRed false esta prueba no tiene que fallar en el RED; debe pasar en el CurrentSha.
  tests/RackCad.UI.Tests/RackCad.UI.Tests.csproj, FullyQualifiedName~RackCad.UI.Tests.PushBackClassicCharacterization, MinSelected 55, ExpectRed false (TRX ci-ui); con ExpectRed false esta prueba no tiene que fallar en el RED; debe pasar en el CurrentSha.
  tests/RackCad.UI.Tests/RackCad.UI.Tests.csproj, FullyQualifiedName~RackCad.UI.Tests.Cantilever, MinSelected 67, ExpectRed false (TRX ci-ui); con ExpectRed false esta prueba no tiene que fallar en el RED; debe pasar en el CurrentSha.
  tests/RackCad.UI.Tests/RackCad.UI.Tests.csproj, FullyQualifiedName~RackCad.UI.Tests.PushBackModuleEditorWindowTests, MinSelected 13, ExpectRed false (TRX ci-ui); con ExpectRed false esta prueba no tiene que fallar en el RED; debe pasar en el CurrentSha.
  tests/RackCad.UI.Tests/RackCad.UI.Tests.csproj, FullyQualifiedName~RackCad.UI.Tests.PushBackAdvancedRackParametersWindowTests, MinSelected 9, ExpectRed false (TRX ci-ui); con ExpectRed false esta prueba no tiene que fallar en el RED; debe pasar en el CurrentSha.
  tests/RackCad.UI.Tests/RackCad.UI.Tests.csproj, FullyQualifiedName~RackCad.UI.Tests.PushBackCompositeWindowTests, MinSelected 52, ExpectRed false (TRX ci-ui); con ExpectRed false esta prueba no tiene que fallar en el RED; debe pasar en el CurrentSha.
  tests/RackCad.UI.Tests/RackCad.UI.Tests.csproj, FullyQualifiedName~RackCad.UI.Tests.PushBackCompositeLifecycleWiringTests, MinSelected 7, ExpectRed false (TRX ci-ui); con ExpectRed false esta prueba no tiene que fallar en el RED; debe pasar en el CurrentSha.
  tests/RackCad.UI.Tests/RackCad.UI.Tests.csproj, FullyQualifiedName~RackCad.UI.Tests.PushBackCompositeLoadExistingModulesTests, MinSelected 3, ExpectRed false (TRX ci-ui); con ExpectRed false esta prueba no tiene que fallar en el RED; debe pasar en el CurrentSha.
  tests/RackCad.UI.Tests/RackCad.UI.Tests.csproj, FullyQualifiedName~RackCad.UI.Tests.PushBackBothModeCellBeamOverrideTests, MinSelected 10, ExpectRed false (TRX ci-ui); con ExpectRed false esta prueba no tiene que fallar en el RED; debe pasar en el CurrentSha.
  tests/RackCad.Tests/RackCad.Tests.csproj, FullyQualifiedName~RackCad.Tests.PushBackClassicCharacterization, MinSelected 10, ExpectRed false (TRX ci-core); con ExpectRed false esta prueba no tiene que fallar en el RED; debe pasar en el CurrentSha.
  tests/RackCad.Tests/RackCad.Tests.csproj, FullyQualifiedName~RackCad.Tests.PushBackFullEditorCharacterization, MinSelected 3, ExpectRed false (TRX ci-core); con ExpectRed false esta prueba no tiene que fallar en el RED; debe pasar en el CurrentSha.
  tests/RackCad.Tests/RackCad.Tests.csproj, FullyQualifiedName~RackCad.Tests.PushBackModuleEditorCharacterizationTests, MinSelected 15, ExpectRed false (TRX ci-core); con ExpectRed false esta prueba no tiene que fallar en el RED; debe pasar en el CurrentSha.
  El operador ~ de VSTest es "contiene", sin distinguir mayusculas, sobre el nombre completo: en el TRX, UnitTest/TestMethod className + "." + name; cuenta los UnitTestResult de esas pruebas. Contrasta con TestResults de la entrega y con el diff.
  RT = git diff --name-only ChainBaseSha RedSha -- tests/; el GREEN no debe tocar RT (git diff --name-only RedSha CurrentSha). Si el RED queda acreditado, ChainRedFiles = RT; registralo en la Evidence de Tests. La CI ejecuta las pruebas en configuracion Debug.
- El RED de esta tarea es ORDINARIO (section 5.1 del paquete; el invariante del RED de la delegacion): el commit RED solo toca tests/ (los dos archivos nuevos de prueba del AllowedWriteScope; sin sustituciones), y el GREEN solo los archivos de producto del AllowedWriteScope (la ventana clasica de Push Back: W y WC). Comprueba:
  git diff --name-only ChainBaseSha RedSha sin rutas de src/; git diff --name-only RedSha CurrentSha solo con rutas de src/ del AllowedWriteScope y ninguna de tests/.
  En los TRX del RED, el criterio mecanico solo para el filtro con ExpectRed true: todos sus resultados con outcome Failed, un Message sin el prefijo de tipo cualificado seguido de " : ", la primera linea de StackTrace en un archivo bajo tests/ (en la CI, una ruta absoluta del runner que contiene la carpeta tests), ninguno NotExecuted ni Skipped, Failed == Selected >= MinSelected. En el RED, los filtros con ExpectRed false pasan; ninguna prueba existente falla en el RED; en el CurrentSha pasan todas.
  La CI del RED (RemoteFacts del registro del trabajo): ui-tests en failure; tests, build-ui y build-plugin en success; sin blame-hang ni timeout. Core no tiene RED (ExpectRed false, precedente de PB-TC). La medicion temporal de la base de D-PBTC2-07 (PBTC2-C14) y la prueba de limpieza son evidencia declarada del Worker, con sus TRX y logs como artefactos: cotejala con el diff y con los TRX.
- Scope: git diff --name-only BaseSha..CurrentSha contra AllowedWriteScope (archivos exactos, sin prefijos; cuenta y lee sus entradas en el delegation.json de las entradas, no en el contrato) y ForbiddenWriteScope (prefijos seguros, F4-E21) de ese mismo delegation.json. Toda ruta cambiada debe estar cubierta por AllowedWriteScope, que es la lista blanca exacta y primaria; una ruta fuera de ella es fail aunque no figure en ForbiddenWriteScope.
- La CI del CurrentSha: los cuatro jobs requeridos de AGENTS.md (RemoteFacts.CurrentRun del registro del trabajo).
- Trailer declarado: el campo Worker.Trailer de la entrega; modelo y effort efectivos: los del registro del trabajo.
- Terminos de gate: los de AUTOMATION_PLAN 16.10, en los campos de texto libre que alli se listan.
- Coherencia (README section 8): EXECUTION_VERIFIED si y solo si las 14 estan en pass (con las 14 en pass, FailureClass NONE);
  una parada se expresa con la comprobacion que la evidencia; FailureClass = la primera que no esta en pass (o StopCondition); Disposition segun la precedencia de 16.9 (STOP > BLOCKED > REWORK); con Result pass, RedPart nunca fail.
- TaskId F4B-PBTC2-DECLARED-CORRECTIONS; RunId R20261118T124722Z-efea; DelegationRunId R20261118T055036Z-c73e; WorkRunId R20261118T113927Z-037c; Attempt 1; VerifiedSha = el CurrentSha verificado solo si Classification es EXECUTION_VERIFIED; si no, null.
- Red: no ejecutes git ls-remote ni ninguna operacion Git de red (tu sandbox no tiene red); los hechos del remoto estan en RemoteFacts del registro del trabajo, medidos por la sesion.
- Findings: hallazgos concretos y comprobables; RecommendedNextAction en una frase.
- Toda tu salida en ASCII: sin tildes ni signos no ASCII y sin secuencias de escape formadas por barra invertida + u.
