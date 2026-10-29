I-65 — DELEGACION RD11-AUTOLOADER-TABLE — Controller, perfil CONTROLLER_VERIFICATION
Identidad: unidad I-65; gate R-D11; tarea RD11-AUTOLOADER-TABLE; intento 7; RunId R20261029T011319Z-0475;
           DelegationRunId R20261029T004516Z-bf20; WorkRunId R20261029T005052Z-2be3;
           AuthorityRevision 91f82d6d83731f48d445c7ad007a814b65c6caa6; MainSha bb0d5522e8411f66a51fdfb3f1f0d0514b737453;
           BaseSha y CurrentSha = los de la entrega; rama architecture/interop-analisis-estructural;
           worktree = el directorio de trabajo actual; Owner subagent:R20261029T004516Z-bf20
Autoridades (leer desde Git, no copiar): las de la delegacion (campo Authorities); en particular
           docs/AUTOMATION_PLAN.md §16 (16.3 lectura de autoridades, 16.8, 16.9 comprobaciones, 16.10 terminos de gate)
           y docs/automation/agent-execution/README.md §8 (EXTERNAL); docs/initiatives/I-65-rd11-autoloader-gate.md
           (contrato del gate R-D11), docs/automation/decisions/I-65.md §105, la disposicion literal del Owner y su respuesta
           sobre el techo de Codex (UNIT_DOC);
           ADR-0050 (UNIT_CHANGE); ADR-0003 y AGENTS.md (EXTERNAL); vigencia: AuthorityRevision para la unidad, MainSha para lo demas
Objetivo: verificar la entrega del Worker contra Git, la delegacion, el contrato y los registros del relevo, y emitir la
           verificacion (esquema rackcad-controller-verification/v1)
Alcance: solo lectura. No escribas ningun archivo, no hagas commit ni push.
Invariantes del Freeze: los del contrato: RD-INV-01 el target copia la tabla byte a byte a Contents/structural-analysis/ram-elements/
           section-material-map.json; RD-INV-02 el verificador admite solo esa ruta, exige presencia e igualdad SHA-256 con la
           fuente y no relaja nada; RD-INV-03 harness con el valido y los cuatro negativos nuevos; RD-INV-04 solo cambia la
           prueba de distribucion; RD-INV-05 tabla, src/*.cs y el comando sin cambio; RD-INV-06 documentacion minima;
           RD-INV-07 controles del Owner; forma RED/GREEN; lo que no cambia del Freeze; controles operativos del Owner
Rutas y archivos calientes: las entradas de artifacts/orchestration/I-65/RD11-AUTOLOADER-TABLE-nc3/0/R20261029T011319Z-0475/inputs/
           (abajo) y el rango BaseSha..CurrentSha de la rama
Criterios de aceptacion: la salida valida contra el esquema; las 14 comprobaciones evaluadas en el orden fijo de
           AUTOMATION_PLAN 16.9 con evidencia concreta; Classification, Disposition y FailureClass coherentes con ellas
Evidencia requerida: ordenes de git reales en el worktree (rev-parse, merge-base --is-ancestor, diff --name-only,
           log --format=%B, check-ignore, show) y lectura de los TRX; cita en cada Evidence lo que obtuviste
No-touch: todo el repositorio
Condiciones de parada: las del contrato (S-02..S-09, S-12..S-14, P-01..P-08 y C-01..C-12); ante cualquiera,
           Classification EXECUTION_BLOCKED con Disposition STOP y el id en TriggeredStopConditions
Prohibido declarar: GATE PASS, Candidato, cierre, integracion o aprobacion del Owner;
           ningun termino de AUTOMATION_PLAN §16 «Terminos de gate» en tu texto libre
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
  artifacts/orchestration/I-65/RD11-AUTOLOADER-TABLE-nc3/0/R20261029T011319Z-0475/inputs/:
  gate-contract.json (contrato de gate del Coordinator); delegation.json (paquete aceptado, RunId R20261029T004516Z-bf20);
  relay-record-planning.json (registro de la planificacion, con la aceptacion A1-A8); worker-handoff.json (la entrega; hace
  las veces de la de ExpectedHandoffPath con {WorkRunId} = R20261029T005052Z-2be3); relay-record-work.json (registro del
  trabajo: terminacion, cesion, relevo de entrada, hechos remotos RemoteFacts, modelo y effort efectivos del Worker y la senal
  de denegaciones en Notes); ci-core/40797120699/core.trx (TRX Core de la corrida push del RedSha, RemoteFacts.RedRun),
  ci-core/40797712877/core.trx y ci-ui/40797712877/ui.trx (TRX de la corrida push del CurrentSha, RemoteFacts.CurrentRun); coordinator-dispositions-rd11.md (disposiciones del Coordinator) y scope-mechanical.json (alcance mecanico de
  la sesion), abajo.
  En RemoteFacts.TestArtifacts los Name apuntan a la copia original de la sesion (.../R20261029T010523Z-2b56/ci-core/,
  ci-ui/ y ci-core-red/); los Sha256 son los de estas mismas copias.
- Las senales con actor «sesion -> Controller» de 16.9 (Termination, Remote, Ci, Routing, Denials) salen del registro
  del trabajo; la fecha de la entrega es Outcome.OutputWrittenUtc del registro y el inicio, Cession.StartUtc.
- Semantica del registro del trabajo (README §3.1 y §3.4): Exit es el estado ANTES de invocar al Worker (HEAD = remoto =
  BaseSha) y Entry el estado DESPUES; que HEAD y ls-remote pasen de BaseSha a CurrentSha entre ambos es lo esperado,
  porque el Worker hace commit y push durante la cesion. Exit.Processes incluye, ademas, los session-descendant de la sesion
  (ayudantes del escritorio) como linea base.
- SCOPE (16.9 fila 7; decision CD-I65-F1-12): compara cada archivo de git diff --name-only BaseSha..CurrentSha con el
  AllowedWriteScope y el ForbiddenWriteScope de la DELEGACION DE ENTRADA (inputs/delegation.json), archivo por archivo, no con
  los del contrato; la comprobacion Contract (fila 4) es la que compara delegacion y contrato. Sintaxis de cobertura de
  AUTOMATION_PLAN 16.5: archivo exacto o prefijo terminado en «/». Cita en la Evidence cada archivo con la entrada que lo cubre.
- Las pruebas: aplica el RequiredTests del contrato (filtro FullyQualifiedName~RackCad.Tests.StructuralAnalysis, MinSelected 1795, ExpectRed true) a los TRX
  (seleccion, superadas, fallidas) y contrasta con TestResults de la entrega y con el diff; esta entrega exige RED
  (ChainRedSha null en la delegacion). Comprueba que el RED falla por asercion o excepcion y no por compilacion (el TRX del RED
  existe y selecciona las pruebas). Que una parte de la seleccion pase ya en el RED (autopruebas de guardas y comprobaciones
  que no dependen del codigo nuevo) es admisible y la entrega lo declara en Evidence (precedentes: I-63 G4 29/26, I-65 F1-T1
  271/167, F1-T2 80/78, F3-T1 296/269 y F3-T2 241/230).
- RT = git diff --name-only <ChainBaseSha> <RedSha> -- tests/ ; comprueba que git diff --name-only RedSha CurrentSha no
  toca RT ni ChainRedFiles; si acreditas el RED, registra en la Evidence de Tests los ChainRedFiles resultantes.
- Cobertura de invariantes: para cada invariante del contrato, nombra la prueba del diff que lo observa (lee su codigo con
  git show CurrentSha:<ruta>) y su resultado en el TRX de CurrentSha: RD-INV-01 en StructuralAnalysisPluginCommandGuardTests.ElTargetDelBundleCopiaLaTablaDesdeSuFuenteALaMismaRutaRelativaQue
  LaCompilacion (lee el XML del proyecto); RD-INV-02 en ElVerificadorDelBundleAdmiteSoloLaRutaDeLaTablaYExigeSuIdentidadConLa
  Fuente y, como comportamiento, en los casos del harness de la CI del CurrentSha (job Build Plugin without AutoCAD de
  RemoteFacts.CurrentRun; no hay TRX: el registro del trabajo cita su salida); RD-INV-03 en ElHarnessDelVerificadorCubreLaTabla
  ConElValidoYCuatroNegativos y en la salida del harness (14 casos); RD-INV-04 con git diff BaseSha..RedSha del archivo de
  pruebas (solo se sustituye un metodo); RD-INV-05 con git diff --name-only BaseSha..CurrentSha y git rev-parse
  CurrentSha:assets/structural-analysis/ram-elements/section-material-map.json; RD-INV-06 leyendo el diff de los tres
  documentos (git diff RedSha CurrentSha -- README.md docs/guias/); RD-INV-07 y los controles operativos son de proceso:
  evaluarlos con relay-record-work.json y las disposiciones D1-D6, no con una prueba. Un invariante con prueba exigida y sin prueba
  que lo observe hace Tests = fail.
- DISPOSICIONES DEL COORDINATOR (texto literal en inputs/coordinator-dispositions-rd11.md): D1 lectura de estado del checkout
  principal no declarada por el Worker (ultima llamada, git -C <checkout principal> status --short), sin efectos (K1/K2 y el
  indice sin cambios): desviacion de procedimiento, C-12 no se materializa; D2 el job del Plugin del RED se omite por needs
  (tests, build-ui) en ci.yml y el contrato solo exige el job Core en failure en el RED; D3 TreeSha null de las corridas
  locales; D4 advertencias de la compilacion Release ajenas al diff y el job del Plugin del CurrentSha sin advertencias;
  D5 procesos efimeros de la app de escritorio en la entrada (falso P-02); D6 la tabla del bundle de la CI tiene el fin de
  linea CRLF del checkout de Windows (hecho RD11-F-08 del contrato del gate, A65-F4D-01). Primera delegacion de la cadena:
  ChainBaseSha = BaseSha = 91f82d6d, ChainRedSha null, ChainRedFiles [] -> esta entrega EXIGE RED (RedSha 42b9dcdf). El
  alcance mecanico de la sesion esta en inputs/scope-mechanical.json; evalua Scope tu mismo, archivo por archivo, contra el
  AllowedWriteScope de la delegacion de entrada.
- Remote: el registro del trabajo trae RemoteFacts.OriginMainSha y RemoteFacts.LsRemoteSha; el esquema
  rackcad-relay-record/v1 no tiene OriginMainSha en Entry, asi que no lo busques alli. Si tu sandbox no puede ejecutar git
  ls-remote, usa RemoteFacts y dilo en la Evidence.
- TRX de UI: tests/RackCad.UI.Tests tiene 17 atributos Skip; el logger TRX los registra como UnitTestResult NotExecuted,
  los cuenta en total y no en executed y deja Counters notExecuted=0 (total 1654 - executed 1637 = 17). Es la convencion del
  logger, no una contradiccion (precedentes: I-61 R20261001T035734Z-74c2; I-63 evidencia §37.4).
- Presupuesto de tiempo: tienes un tope duro de 600 s; no vuelques archivos grandes completos (TRX, diffs enteros): usa
  conteos y filtros (Select-String, git diff --stat, git diff --name-only) y lee solo los fragmentos que necesites.
- Trailer declarado: el valor literal del campo Worker.Trailer de la entrega (citalo en la Evidence tal como lo leiste);
  modelo efectivo: el del registro del trabajo.
- Terminos de gate: los de AUTOMATION_PLAN 16.10, en los campos de texto libre que alli se listan.
- Coherencia (README §8): EXECUTION_VERIFIED si y solo si las 14 estan en pass (con las 14 en pass, FailureClass NONE);
  una parada se expresa con la comprobacion que la evidencia; FailureClass = la primera que no esta en pass
  (o StopCondition); Disposition segun la precedencia de 16.9 (STOP > BLOCKED > REWORK); con Result pass, RedPart nunca fail.
- TaskId RD11-AUTOLOADER-TABLE; RunId R20261029T011319Z-0475; DelegationRunId R20261029T004516Z-bf20;
  WorkRunId R20261029T005052Z-2be3; Attempt 7; VerifiedSha = el CurrentSha verificado solo si Classification es
  EXECUTION_VERIFIED; si no, null.
- Findings: hallazgos concretos y comprobables (desviaciones declaradas, limitaciones de frontera, diferencias entre
  la entrega y Git); RecommendedNextAction en una frase.
