I-64 - DELEGACION F4B-PBT1A-LOSSLESS-STATE-NET - Controller, perfil CONTROLLER_VERIFICATION
Identidad: unidad I-64; gate F4B; tarea F4B-PBT1A-LOSSLESS-STATE-NET; intento 1; RunId R20261118T173153Z-7969;
           DelegationRunId R20261118T141628Z-d049; WorkRunId R20261118T142230Z-d31d;
           AuthorityRevision 95c8126ba88c4e355cc09c51db972ff7f757251a; MainSha 812baf879612320653094a8aaf46aca0bac81383;
           BaseSha y CurrentSha = los de la entrega; rama architecture/workspace-persistente-rackcad;
           worktree = el directorio de trabajo actual; Owner subagent:R20261118T141628Z-d049
Autoridades (leer desde Git, no copiar): las de la delegacion (campo Authorities); en particular
           docs/AUTOMATION_PLAN.md section 16 (16.3 lectura de autoridades, 16.8, 16.9 comprobaciones, 16.10 terminos de gate)
           y docs/automation/agent-execution/README.md section 8 (EXTERNAL); AGENTS.md (EXTERNAL);
           vigencia: AuthorityRevision para la unidad, MainSha para lo demas
Objetivo: verificar la entrega del Worker contra Git, la delegacion, el contrato y los registros del relevo, y emitir la
           verificacion (esquema rackcad-controller-verification/v1)
Alcance: solo lectura. No escribas ningun archivo, no hagas commit ni push. No ejecutes dotnet ni builds: usa los TRX.
Invariantes del Freeze: los 14 de la delegacion, vinculantes en su totalidad
Rutas y archivos calientes: las entradas de artifacts/orchestration/I-64/F4B-PBT1A-LOSSLESS-STATE-NET-nc3/1/R20261118T173153Z-7969/inputs/ (abajo) y el rango
           BaseSha..CurrentSha de la rama
Criterios de aceptacion: la salida valida contra el esquema; las 14 comprobaciones evaluadas en el orden fijo de
           AUTOMATION_PLAN 16.9 con evidencia concreta; Classification, Disposition y FailureClass coherentes con ellas
Evidencia requerida: ordenes de git reales en el worktree (rev-parse, merge-base --is-ancestor, diff --name-only,
           log --format=%B, check-ignore, show, status --porcelain) y lectura de los TRX; cita en cada Evidence lo que obtuviste
No-touch: todo el repositorio
Condiciones de parada: S-02, S-03, S-04, S-12, S-13, P-03, PBT1A-C01..PBT1A-C07, PBT1A-C09 y PBT1A-C10 de la delegacion; ante cualquiera,
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
  artifacts/orchestration/I-64/F4B-PBT1A-LOSSLESS-STATE-NET-nc3/1/R20261118T173153Z-7969/inputs/:
  gate-contract.json; delegation.json; relay-record-planning.json (con A1-A8 y nc4 en sus Notes);
  worker-handoff.json (hace las veces de la de ExpectedHandoffPath con {WorkRunId} = R20261118T142230Z-d31d);
  relay-record-work.json (terminacion, cesion, relevo de entrada, hechos remotos RemoteFacts, modelo y effort efectivos del Worker, denegaciones y procesos en Notes);
  ci-core/44338475965/core.trx y ci-ui/44338475965/ui.trx de la corrida push del CurrentSha; ci-core-red/44333116533/core.trx y ci-ui-red/44333116533/ui.trx de la corrida push del RedSha.
  byte-fidelity-check.json: la comparacion determinista byte a byte de las colecciones de la delegacion con el contrato, hecha por la sesion antes de aceptar (PASS en las ocho); la planificacion paso A1-A8 8/8 y nc4 (en las Notes de relay-record-planning.json).
  coordinator-disposition.md: disposicion del Owner sobre un punto declarado de la entrega; es autoridad para ese punto y, solo en lo que ratifica, prevalece sobre la letra del contrato para este intento.
- config.toml: en la salida y en la entrada del relevo el hash es la SNAPSHOT I (0B47701A...), aceptada por el Owner tras la sonda neutra R20261114T204534Z-6603 en PASS, docs/automation/evidence/I-64-pilot/F4-DRAFTS/f4-dispositions.md, F4-E57 (F4-ENVIRONMENT/snapshot-I/snapshot-i.json); no es un STOP de esta verificacion.
- Las senales con actor "sesion -> Controller" de 16.9 (Termination, Remote, Ci, Routing, Denials) salen del registro
  del trabajo; la fecha de la entrega es Outcome.OutputWrittenUtc del registro y el inicio, Cession.StartUtc.
- Semantica del registro del trabajo (README section 3.1 y section 3.4): Exit es el estado ANTES de invocar al Worker (HEAD = remoto = BaseSha) y Entry el estado DESPUES.
- Routing y transporte subagent: el modelo solicitado es el campo Model de la delegacion. El transporte subagent solo admite como parametro de lanzamiento el alias de familia, y Participant.ModelRequested del registro del trabajo guarda ese parametro literal; el modelo real de cada turno esta en Participant.ModelEffective y EffectiveSource.
- Procesos (16.4): participante es un proceso evaluado cuya linea de ordenes contiene la ruta del worktree. Los no atribuibles que desaparecen en la relectura quedan CLEARED_BY_REREAD; los hechos estan en las Notes del registro del trabajo.
- Autoridades: los valores Section son la transliteracion ASCII de los encabezados reales; localiza cada seccion comparando sin diacriticos, con la raya larga como guion y el signo de seccion como "section".
- Revisiones: AuthorityRevision = 95c8126b; BaseSha = el de la entrega, el commit de emision del contrato, HIJO DIRECTO de la AuthorityRevision. 16.3 exige que AuthorityRevision sea ancestro de BaseSha y que git diff --name-only AuthorityRevision BaseSha no contenga rutas EXTERNAL ni con secciones UNIT_CHANGE; compruebalo. Hecho medido por la sesion en la emision: ese diff tiene cinco rutas, todas bajo docs/ de la unidad: el contrato, su script generador y la salida de los anclajes medidos de nuevo sobre la base (docs/automation/evidence/I-64-pilot/F4B-PBT1A-LOSSLESS-STATE-NET/coordinator/), el estado y la evidencia; el contrato, el estado y la evidencia figuran en Authorities como UNIT_DOC, y el script generador y la salida de los anclajes no figuran (no son EXTERNAL ni UNIT_CHANGE). Si el diff que mides no coincide con este hecho, comprueba igualmente la regla de 16.3 sobre el diff real (solo rutas de docs/ de la unidad, ni EXTERNAL ni UNIT_CHANGE) y no tomes la diferencia de recuento como fallo por si sola.
- Las pruebas y el RED: primera delegacion de la cadena: ChainBaseSha = BaseSha y ChainRedSha = null en la delegacion; la entrega exige RED, que es el RedSha entre BaseSha y CurrentSha. Aplica el RequiredTests del contrato a los TRX:
  tests/RackCad.UI.Tests/RackCad.UI.Tests.csproj, FullyQualifiedName~RackCad.UI.Tests.PushBackStateNet, MinSelected 14, ExpectRed true (TRX ci-ui); con ExpectRed true debe fallar en el RED por asercion y pasar en el CurrentSha.
  tests/RackCad.UI.Tests/RackCad.UI.Tests.csproj, FullyQualifiedName~RackCad.UI.Tests.PushBackClassicCharacterization, MinSelected 55, ExpectRed false (TRX ci-ui); con ExpectRed false no se le aplica el criterio del RED: en el RED puede fallar SOLO por los mutantes, con la atribucion de la tabla T-8 de la entrega (INV-PBT1A-02); debe pasar en el CurrentSha.
  tests/RackCad.UI.Tests/RackCad.UI.Tests.csproj, FullyQualifiedName~RackCad.UI.Tests.PushBackFullEditorCharacterization, MinSelected 16, ExpectRed false (TRX ci-ui); con ExpectRed false no se le aplica el criterio del RED: en el RED puede fallar SOLO por los mutantes, con la atribucion de la tabla T-8 de la entrega (INV-PBT1A-02); debe pasar en el CurrentSha.
  tests/RackCad.UI.Tests/RackCad.UI.Tests.csproj, FullyQualifiedName~RackCad.UI.Tests.PushBackDeclaredCorrectionRoundingTests, MinSelected 3, ExpectRed false (TRX ci-ui); con ExpectRed false no se le aplica el criterio del RED: en el RED puede fallar SOLO por los mutantes, con la atribucion de la tabla T-8 de la entrega (INV-PBT1A-02); debe pasar en el CurrentSha.
  tests/RackCad.UI.Tests/RackCad.UI.Tests.csproj, FullyQualifiedName~RackCad.UI.Tests.PushBackDeclaredCorrectionDimensionStyleTests, MinSelected 2, ExpectRed false (TRX ci-ui); con ExpectRed false no se le aplica el criterio del RED: en el RED puede fallar SOLO por los mutantes, con la atribucion de la tabla T-8 de la entrega (INV-PBT1A-02); debe pasar en el CurrentSha.
  tests/RackCad.UI.Tests/RackCad.UI.Tests.csproj, FullyQualifiedName~RackCad.UI.Tests.PushBackDeclaredCorrectionPilotParityTests, MinSelected 2, ExpectRed false (TRX ci-ui); con ExpectRed false no se le aplica el criterio del RED: en el RED puede fallar SOLO por los mutantes, con la atribucion de la tabla T-8 de la entrega (INV-PBT1A-02); debe pasar en el CurrentSha.
  tests/RackCad.UI.Tests/RackCad.UI.Tests.csproj, FullyQualifiedName~RackCad.UI.Tests.PushBackDeclaredCorrectionSideBNoticeTests, MinSelected 2, ExpectRed false (TRX ci-ui); con ExpectRed false no se le aplica el criterio del RED: en el RED puede fallar SOLO por los mutantes, con la atribucion de la tabla T-8 de la entrega (INV-PBT1A-02); debe pasar en el CurrentSha.
  tests/RackCad.UI.Tests/RackCad.UI.Tests.csproj, FullyQualifiedName~RackCad.UI.Tests.PushBackDeclaredCorrectionStandardRestoreTests, MinSelected 1, ExpectRed false (TRX ci-ui); con ExpectRed false no se le aplica el criterio del RED: en el RED puede fallar SOLO por los mutantes, con la atribucion de la tabla T-8 de la entrega (INV-PBT1A-02); debe pasar en el CurrentSha.
  tests/RackCad.UI.Tests/RackCad.UI.Tests.csproj, FullyQualifiedName~RackCad.UI.Tests.PushBackDeclaredCorrection2, MinSelected 4, ExpectRed false (TRX ci-ui); con ExpectRed false no se le aplica el criterio del RED: en el RED puede fallar SOLO por los mutantes, con la atribucion de la tabla T-8 de la entrega (INV-PBT1A-02); debe pasar en el CurrentSha.
  tests/RackCad.UI.Tests/RackCad.UI.Tests.csproj, FullyQualifiedName~RackCad.UI.Tests.Cantilever, MinSelected 67, ExpectRed false (TRX ci-ui); con ExpectRed false no se le aplica el criterio del RED: en el RED puede fallar SOLO por los mutantes, con la atribucion de la tabla T-8 de la entrega (INV-PBT1A-02); debe pasar en el CurrentSha.
  tests/RackCad.UI.Tests/RackCad.UI.Tests.csproj, FullyQualifiedName~RackCad.UI.Tests.PushBackCompositeWindowTests, MinSelected 52, ExpectRed false (TRX ci-ui); con ExpectRed false no se le aplica el criterio del RED: en el RED puede fallar SOLO por los mutantes, con la atribucion de la tabla T-8 de la entrega (INV-PBT1A-02); debe pasar en el CurrentSha.
  tests/RackCad.UI.Tests/RackCad.UI.Tests.csproj, FullyQualifiedName~RackCad.UI.Tests.PushBackCompositeLifecycleWiringTests, MinSelected 7, ExpectRed false (TRX ci-ui); con ExpectRed false no se le aplica el criterio del RED: en el RED puede fallar SOLO por los mutantes, con la atribucion de la tabla T-8 de la entrega (INV-PBT1A-02); debe pasar en el CurrentSha.
  tests/RackCad.UI.Tests/RackCad.UI.Tests.csproj, FullyQualifiedName~RackCad.UI.Tests.PushBackCompositeLoadExistingModulesTests, MinSelected 3, ExpectRed false (TRX ci-ui); con ExpectRed false no se le aplica el criterio del RED: en el RED puede fallar SOLO por los mutantes, con la atribucion de la tabla T-8 de la entrega (INV-PBT1A-02); debe pasar en el CurrentSha.
  tests/RackCad.UI.Tests/RackCad.UI.Tests.csproj, FullyQualifiedName~RackCad.UI.Tests.PushBackBothModeCellBeamOverrideTests, MinSelected 10, ExpectRed false (TRX ci-ui); con ExpectRed false no se le aplica el criterio del RED: en el RED puede fallar SOLO por los mutantes, con la atribucion de la tabla T-8 de la entrega (INV-PBT1A-02); debe pasar en el CurrentSha.
  tests/RackCad.UI.Tests/RackCad.UI.Tests.csproj, FullyQualifiedName~RackCad.UI.Tests.PushBackModuleEditorWindowTests, MinSelected 13, ExpectRed false (TRX ci-ui); con ExpectRed false no se le aplica el criterio del RED: en el RED puede fallar SOLO por los mutantes, con la atribucion de la tabla T-8 de la entrega (INV-PBT1A-02); debe pasar en el CurrentSha.
  tests/RackCad.UI.Tests/RackCad.UI.Tests.csproj, FullyQualifiedName~RackCad.UI.Tests.PushBackAdvancedRackParametersWindowTests, MinSelected 9, ExpectRed false (TRX ci-ui); con ExpectRed false no se le aplica el criterio del RED: en el RED puede fallar SOLO por los mutantes, con la atribucion de la tabla T-8 de la entrega (INV-PBT1A-02); debe pasar en el CurrentSha.
  tests/RackCad.Tests/RackCad.Tests.csproj, FullyQualifiedName~RackCad.Tests.PushBackClassicCharacterization, MinSelected 10, ExpectRed false (TRX ci-core); con ExpectRed false no se le aplica el criterio del RED: en el RED puede fallar SOLO por los mutantes, con la atribucion de la tabla T-8 de la entrega (INV-PBT1A-02); debe pasar en el CurrentSha.
  tests/RackCad.Tests/RackCad.Tests.csproj, FullyQualifiedName~RackCad.Tests.PushBackFullEditorCharacterization, MinSelected 3, ExpectRed false (TRX ci-core); con ExpectRed false no se le aplica el criterio del RED: en el RED puede fallar SOLO por los mutantes, con la atribucion de la tabla T-8 de la entrega (INV-PBT1A-02); debe pasar en el CurrentSha.
  tests/RackCad.Tests/RackCad.Tests.csproj, FullyQualifiedName~RackCad.Tests.PushBackModuleEditorCharacterizationTests, MinSelected 15, ExpectRed false (TRX ci-core); con ExpectRed false no se le aplica el criterio del RED: en el RED puede fallar SOLO por los mutantes, con la atribucion de la tabla T-8 de la entrega (INV-PBT1A-02); debe pasar en el CurrentSha.
  El operador ~ de VSTest es "contiene", sin distinguir mayusculas, sobre el nombre completo: en el TRX, UnitTest/TestMethod className + "." + name; cuenta los UnitTestResult de esas pruebas. Contrasta con TestResults de la entrega y con el diff.
  RT = git diff --name-only ChainBaseSha RedSha -- tests/; el GREEN no debe tocar RT (git diff --name-only RedSha CurrentSha). Si el RED queda acreditado, ChainRedFiles = RT; registralo en la Evidence de Tests. La CI ejecuta las pruebas en configuracion Debug.
- El RED de esta tarea es por MUTANTES REVERTIDOS (section 5.2 del paquete del contrato de PB-T1 v2.4, con el patron de PB-T0; INV-PBT1A-01, INV-PBT1A-02 e INV-PBT1A-03 de la delegacion): el commit RED lleva los siete archivos nuevos de prueba y los quince mutantes de la tabla cerrada MU-T1A-01..14 (con 07a y 07b) en los dos archivos de src/ del AllowedWriteScope (MutantScope: W y WC, la ventana clasica de Push Back), y el GREEN los revierte con git restore --source=BaseSha. Esas escrituras en src/ del RED, dentro de MutantScope y revertidas, NO son escritura fuera de alcance. Comprueba con git:
  git diff --name-only ChainBaseSha CurrentSha = exactamente los siete archivos nuevos de tests/ del AllowedWriteScope; git diff ChainBaseSha CurrentSha -- src vacio; git diff --name-only RedSha CurrentSha solo con rutas de MutantScope y ninguna de tests/; git diff --name-only ChainBaseSha RedSha -- src solo con rutas de MutantScope;
  por cada archivo de src/ que el RED toco: git rev-parse ChainBaseSha:<ruta> = git rev-parse CurrentSha:<ruta> y distinto de git rev-parse RedSha:<ruta>; contrastalo con la tabla de bytes de la entrega.
  En los TRX del RED, el criterio mecanico de INV-PBT1A-02, SOLO para el filtro con ExpectRed true: todos sus resultados con outcome Failed, un Message sin el prefijo de tipo cualificado seguido de " : ", la primera linea de StackTrace en un archivo bajo tests/ (en la CI, una ruta absoluta del runner que contiene la carpeta tests), ninguno NotExecuted ni Skipped, Failed == Selected >= MinSelected. Pruebas existentes pueden fallar en el RED solo por los mutantes, con la union de la CI del RED y su atribucion declaradas en la entrega (tabla T-8); para ellas solo cuentan el cuelgue y el timeout; en el CurrentSha pasan todas.
  La CI del RED (RemoteFacts del registro del trabajo): ui-tests en failure; tests, build-ui y build-plugin en success; sin blame-hang ni timeout. Los mutantes son solo de la UI y Core no tiene RED (sus filtros tienen ExpectRed false): con Core en success, RedPart = pass, como F4-T3B1 y F4-T4A2 en su intento 2 (F4-DRAFTS/f4-dispositions.md, F4-E56; section 6.0 del paquete). Cada mutante de la tabla de la entrega instancia una fila de la tabla cerrada de INV-PBT1A-02 (section 5.2 del paquete; uno fuera de tabla es PBT1A-C04) como la sustitucion exacta de una linea de BaseSha y aparece en git diff ChainBaseSha RedSha -- src, sin otros cambios de src/ que no esten en la tabla; MU-T1A-13 esta en la linea de LoadExisting y no en la de LoadDesignForNew. La prueba por mutante aislado (INV-PBT1A-03), el paso 1 con las lineas esperadas (INV-PBT1A-04), las mediciones guardadas (INV-PBT1A-06) y la prueba de limpieza son evidencia declarada del Worker: cotejala con el diff del RED y con los TRX.
- Scope: git diff --name-only BaseSha..CurrentSha contra AllowedWriteScope (archivos exactos, sin prefijos; cuenta y lee sus entradas en el delegation.json de las entradas, no en el contrato) y ForbiddenWriteScope (prefijos seguros, F4-E21) de ese mismo delegation.json. Toda ruta cambiada debe estar cubierta por AllowedWriteScope, que es la lista blanca exacta y primaria; una ruta fuera de ella es fail aunque no figure en ForbiddenWriteScope.
- La CI del CurrentSha: los cuatro jobs requeridos de AGENTS.md (RemoteFacts.CurrentRun del registro del trabajo).
- COMPROBACIONES DETERMINISTAS OBLIGATORIAS (F4B-E15): la entrega de esta tarea es grande; no la evalues leyendo volcados largos. No vuelques archivos enteros:
  imprime solo lo que cada comprobacion necesita. Tres comprobaciones se hacen SIEMPRE con un script de solo lectura, y la evidencia del check cita la orden y su salida:
  (1) FreeText: un script que carga worker-handoff.json de las entradas y, para cada uno de los siete campos de texto libre (WorkCompleted, Evidence,
      UnexpectedFindings, KnownLimitations, Deviations, OpenQuestions y RecommendedNextAction), busca sobre el TEXTO COMPLETO del campo, sin distinguir
      mayusculas y como palabra o frase completa, cada termino de AUTOMATION_PLAN 16.10; imprime el numero de caracteres de cada campo y cada coincidencia
      (campo, termino y su contexto). Una coincidencia es fail de FreeText.
  (2) Scope: un script que lee AllowedWriteScope de delegation.json de las entradas, imprime su numero de entradas y cada entrada, y, para cada ruta de
      git diff --name-only BaseSha CurrentSha y de git diff --name-only BaseSha RedSha, imprime si esta en esa lista exacta (igualdad de cadenas);
      una ruta que no este es fail de Scope, aunque figure en el contrato.
  (3) Identity: la evidencia escribe literalmente el valor de CurrentSha de la entrega y el HEAD medido (git rev-parse HEAD), y el de
      refs/remotes/origin/<rama>, aunque coincidan.
- Trailer declarado: el campo Worker.Trailer de la entrega; modelo y effort efectivos: los del registro del trabajo.
- Terminos de gate: los de AUTOMATION_PLAN 16.10, en los campos de texto libre que alli se listan.
- Coherencia (README section 8): EXECUTION_VERIFIED si y solo si las 14 estan en pass (con las 14 en pass, FailureClass NONE);
  una parada se expresa con la comprobacion que la evidencia; FailureClass = la primera que no esta en pass (o StopCondition); Disposition segun la precedencia de 16.9 (STOP > BLOCKED > REWORK); con Result pass, RedPart nunca fail.
- TaskId F4B-PBT1A-LOSSLESS-STATE-NET; RunId R20261118T173153Z-7969; DelegationRunId R20261118T141628Z-d049; WorkRunId R20261118T142230Z-d31d; Attempt 1; VerifiedSha = el CurrentSha verificado solo si Classification es EXECUTION_VERIFIED; si no, null.
- Red: no ejecutes git ls-remote ni ninguna operacion Git de red (tu sandbox no tiene red); los hechos del remoto estan en RemoteFacts del registro del trabajo, medidos por la sesion.
- Findings: hallazgos concretos y comprobables; RecommendedNextAction en una frase.
- Toda tu salida en ASCII: sin tildes ni signos no ASCII y sin secuencias de escape formadas por barra invertida + u.
