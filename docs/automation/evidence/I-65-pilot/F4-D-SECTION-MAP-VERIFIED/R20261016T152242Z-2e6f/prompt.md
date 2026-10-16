I-65 — DELEGACION F4-D-SECTION-MAP-VERIFIED — Controller, perfil CONTROLLER_VERIFICATION
Identidad: unidad I-65; gate F4; tarea F4-D-SECTION-MAP-VERIFIED; intento 6; RunId R20261016T152242Z-2e6f;
           DelegationRunId R20261016T144346Z-4eaf; WorkRunId R20261016T145459Z-8301;
           AuthorityRevision e886b5577ad565ef4a55bb2e535b7d463f345b79; MainSha bb0d5522e8411f66a51fdfb3f1f0d0514b737453;
           BaseSha y CurrentSha = los de la entrega; rama architecture/interop-analisis-estructural;
           worktree = el directorio de trabajo actual; Owner subagent:R20261016T144346Z-4eaf
Autoridades (leer desde Git, no copiar): las de la delegacion (campo Authorities); en particular
           docs/AUTOMATION_PLAN.md §16 (16.3 lectura de autoridades, 16.8, 16.9 comprobaciones, 16.10 terminos de gate)
           y docs/automation/agent-execution/README.md §8 (EXTERNAL); docs/initiatives/I-65-f4-d-mapping-verified-contract.md
           (contrato de F4-D), docs/automation/decisions/I-65.md §62, la orden literal del Owner y la tabla objetivo
           docs/automation/evidence/I-65-f4/f4d-table-target.json (UNIT_DOC);
           ADR-0050 (UNIT_CHANGE); AGENTS.md (EXTERNAL); vigencia: AuthorityRevision para la unidad, MainSha para lo demas
Objetivo: verificar la entrega del Worker contra Git, la delegacion, el contrato y los registros del relevo, y emitir la
           verificacion (esquema rackcad-controller-verification/v1)
Alcance: solo lectura. No escribas ningun archivo, no hagas commit ni push.
Invariantes del Freeze: los del contrato: D-INV-01 solo las dos filas y sus campos; D-INV-02 blob de la tabla igual al del archivo objetivo y pruebas con
           sus literales; D-INV-03 DIF-01 no cambia el valor esperado ni las tolerancias; D-INV-04 REF-CANT-01 Valid y paquete
           Valid en memoria con la tabla distribuida y huella sensible a un campo verificado; D-INV-05 cobertura proposed con
           tablas de fixture; D-INV-06 sin escrituras fuera de la carpeta temporal ni publicacion; D-INV-07 controles del Owner;
           forma RED/GREEN; INV-14 y guardas; lo que no cambia del Freeze; controles operativos del Owner
Rutas y archivos calientes: las entradas de artifacts/orchestration/I-65/F4-D-SECTION-MAP-VERIFIED/6/R20261016T152242Z-2e6f/inputs/
           (abajo) y el rango BaseSha..CurrentSha de la rama
Criterios de aceptacion: la salida valida contra el esquema; las 14 comprobaciones evaluadas en el orden fijo de
           AUTOMATION_PLAN 16.9 con evidencia concreta; Classification, Disposition y FailureClass coherentes con ellas
Evidencia requerida: ordenes de git reales en el worktree (rev-parse, merge-base --is-ancestor, diff --name-only,
           log --format=%B, check-ignore, show) y lectura de los TRX; cita en cada Evidence lo que obtuviste
No-touch: todo el repositorio
Condiciones de parada: las del contrato (S-02..S-09, S-12..S-14, P-01..P-08, C-01..C-10 y C-12); ante cualquiera,
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
  artifacts/orchestration/I-65/F4-D-SECTION-MAP-VERIFIED/6/R20261016T152242Z-2e6f/inputs/:
  gate-contract.json (contrato de gate del Coordinator); delegation.json (paquete aceptado, RunId R20261016T144346Z-4eaf);
  relay-record-planning.json (registro de la planificacion, con la aceptacion A1-A8); worker-handoff.json (la entrega; hace
  las veces de la de ExpectedHandoffPath con {WorkRunId} = R20261016T145459Z-8301); relay-record-work.json (registro del
  trabajo: terminacion, cesion, relevo de entrada, hechos remotos RemoteFacts, modelo y effort efectivos del Worker y la senal
  de denegaciones en Notes); ci-core/38921140743/core.trx (TRX Core de la corrida push del RedSha, RemoteFacts.RedRun),
  ci-core/38922575054/core.trx y ci-ui/38922575054/ui.trx (TRX de la corrida push del CurrentSha, RemoteFacts.CurrentRun); coordinator-dispositions-f4d.md (disposiciones del Coordinator) y scope-mechanical.json (alcance mecanico de
  la sesion), abajo.
  En RemoteFacts.TestArtifacts los Name apuntan a la copia original de la sesion (.../R20261016T152242Z-2e6f/ci-core/,
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
- Las pruebas: aplica el RequiredTests del contrato (filtro FullyQualifiedName~RackCad.Tests.StructuralAnalysis, MinSelected 1789, ExpectRed true) a los TRX
  (seleccion, superadas, fallidas) y contrasta con TestResults de la entrega y con el diff; esta entrega exige RED
  (ChainRedSha null en la delegacion). Comprueba que el RED falla por asercion o excepcion y no por compilacion (el TRX del RED
  existe y selecciona las pruebas). Que una parte de la seleccion pase ya en el RED (autopruebas de guardas y comprobaciones
  que no dependen del codigo nuevo) es admisible y la entrega lo declara en Evidence (precedentes: I-63 G4 29/26, I-65 F1-T1
  271/167, F1-T2 80/78, F3-T1 296/269 y F3-T2 241/230).
- RT = git diff --name-only <ChainBaseSha> <RedSha> -- tests/ ; comprueba que git diff --name-only RedSha CurrentSha no
  toca RT ni ChainRedFiles; si acreditas el RED, registra en la Evidence de Tests los ChainRedFiles resultantes.
- Cobertura de invariantes: para cada invariante del contrato, nombra la prueba del diff que lo observa (lee su codigo con
  git show CurrentSha:<ruta>) y su resultado en el TRX de CurrentSha: D-INV-01/D-INV-02 (filas, literales, lector) en StructuralAnalysisSectionMapVerifiedTests y en las ediciones de
  StructuralAnalysisRamElementsMapReaderTests, con la igualdad de blobs (git rev-parse CurrentSha:assets/structural-analysis/
  ram-elements/section-material-map.json frente a git rev-parse AuthorityRevision:docs/automation/evidence/I-65-f4/
  f4d-table-target.json); D-INV-03 (490 pcf, G null, 0.3) en las mismas pruebas de literales; D-INV-04 (REF-CANT-01 Valid con los
  mismos diagnosticos que con la tabla verified de fixture, paquete Valid en memoria, huella) en StructuralAnalysisSectionMap
  VerifiedTests y StructuralAnalysisPluginCommandProductTableTests; D-INV-05 (precedencia y propagacion con tablas proposed de
  fixture) en StructuralAnalysisRamElementsPackageValidationTests y StructuralAnalysisRamElementsTranslationTests; D-INV-06 en las
  pruebas de paquete en memoria; forma RED/GREEN e INV-14 con el diff y las guardas del TRX. Los invariantes D-INV-07 y de
  controles operativos son de proceso: evaluarlos con relay-record-work.json y las disposiciones D2, no con una prueba. Un invariante con prueba exigida y sin prueba
  que lo observe hace Tests = fail.
- DISPOSICIONES DEL COORDINATOR (texto literal en inputs/coordinator-dispositions-f4d.md): D1 modelo de la prueba editada
  de la tabla del producto construido con esa tabla (admisible, sin debilitamiento); D2 archivo temporal de sondeo TmpProbe.cs
  escrito y borrado dentro del worktree, nunca en un commit (desviacion de procedimiento sin efectos; sin C-12 ni C-05);
  D3 causa de la CI roja del GREEN: la carpeta de paquete de tres pruebas (Path.GetTempPath() en Linux) no es una ruta
  Windows absoluta y la fabrica da RAM-PACKAGE-FOLDER-INVALID; no es un diagnostico del modelo ni de la tabla (C-03 no se
  activa). La CI del CurrentSha NO esta en verde: evalua Ci y Tests con los TRX. Primera delegacion de la cadena: ChainBaseSha
  = BaseSha = e886b557, ChainRedSha null, ChainRedFiles [] -> esta entrega EXIGE RED (RedSha fe9b6720). El alcance mecanico
  de la sesion esta en inputs/scope-mechanical.json; evalua Scope tu mismo, archivo por archivo, contra el
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
- TaskId F4-D-SECTION-MAP-VERIFIED; RunId R20261016T152242Z-2e6f; DelegationRunId R20261016T144346Z-4eaf;
  WorkRunId R20261016T145459Z-8301; Attempt 6; VerifiedSha = el CurrentSha verificado solo si Classification es
  EXECUTION_VERIFIED; si no, null.
- Findings: hallazgos concretos y comprobables (desviaciones declaradas, limitaciones de frontera, diferencias entre
  la entrega y Git); RecommendedNextAction en una frase.
