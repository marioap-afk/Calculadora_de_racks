I-65 — DELEGACION F4-T3-DUMP-AND-READERS — Controller, perfil CONTROLLER_VERIFICATION
Identidad: unidad I-65; gate F4; tarea F4-T3-DUMP-AND-READERS; intento 2; RunId R20261011T194727Z-d7ea;
           DelegationRunId R20261011T183102Z-7511; WorkRunId R20261011T183618Z-05a9;
           AuthorityRevision c73058f88368196309975616298523317ace2813; MainSha bb0d5522e8411f66a51fdfb3f1f0d0514b737453;
           BaseSha y CurrentSha = los de la entrega; rama architecture/interop-analisis-estructural;
           worktree = el directorio de trabajo actual; Owner subagent:R20261011T183102Z-7511
Autoridades (leer desde Git, no copiar): las de la delegacion (campo Authorities); en particular
           docs/AUTOMATION_PLAN.md §16 (16.3 lectura de autoridades, 16.8, 16.9 comprobaciones, 16.10 terminos de gate)
           y docs/automation/agent-execution/README.md §8 (EXTERNAL); docs/initiatives/I-65-proposal-v3.1.md (Frozen: yes),
           docs/initiatives/I-65-f4-implementation.md (concrecion de F4), docs/automation/decisions/I-65.md §26 (UNIT_DOC);
           ADR-0050 (UNIT_CHANGE); AGENTS.md (EXTERNAL); vigencia: AuthorityRevision para la unidad, MainSha para lo demas
Objetivo: verificar la entrega del Worker contra Git, la delegacion, el contrato y los registros del relevo, y emitir la
           verificacion (esquema rackcad-controller-verification/v1)
Alcance: solo lectura. No escribas ningun archivo, no hagas commit ni push.
Invariantes del Freeze: los del contrato: script fijo de volcado, solo lectura y contrato de unidades, contenido del volcado y API frente a P1, lector estricto
           con conversion al marco neutral e ida y vuelta de REF-CANT-01, lectores de la lectura de vuelta y del manifiesto, tipos
           neutrales, INV-14 y forma del cambio (CD-I65-F4-13), restricciones del Owner
Rutas y archivos calientes: las entradas de artifacts/orchestration/I-65/F4-T3-DUMP-AND-READERS/2/R20261011T194727Z-d7ea/inputs/
           (abajo) y el rango BaseSha..CurrentSha de la rama
Criterios de aceptacion: la salida valida contra el esquema; las 14 comprobaciones evaluadas en el orden fijo de
           AUTOMATION_PLAN 16.9 con evidencia concreta; Classification, Disposition y FailureClass coherentes con ellas
Evidencia requerida: ordenes de git reales en el worktree (rev-parse, merge-base --is-ancestor, diff --name-only,
           log --format=%B, check-ignore, show) y lectura de los TRX; cita en cada Evidence lo que obtuviste
No-touch: todo el repositorio
Condiciones de parada: las del contrato (S-02..S-09, S-12..S-14, P-01..P-08, C-01..C-11); ante cualquiera,
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
  artifacts/orchestration/I-65/F4-T3-DUMP-AND-READERS/2/R20261011T194727Z-d7ea/inputs/:
  gate-contract.json (contrato de gate del Coordinator); delegation.json (paquete aceptado, RunId R20261011T183102Z-7511);
  relay-record-planning.json (registro de la planificacion, con la aceptacion A1-A8); worker-handoff.json (la entrega; hace
  las veces de la de ExpectedHandoffPath con {WorkRunId} = R20261011T183618Z-05a9); relay-record-work.json (registro del
  trabajo: terminacion, cesion, relevo de entrada, hechos remotos RemoteFacts, modelo y effort efectivos del Worker y la senal
  de denegaciones en Notes); ci-core/38166596013/core.trx (TRX Core de la corrida push del RedSha, RemoteFacts.RedRun),
  ci-core/38166926118/core.trx y ci-ui/38166926118/ui.trx (TRX de la corrida push del CurrentSha, RemoteFacts.CurrentRun); analysis-f4t3.md (causa raiz del STOP del Worker), coordinator-dispositions-f4t3.md (decision del Coordinator),
  owner-disposition-f4t3.original.txt (disposicion literal del Owner), previous-verification-R20261011T192137Z-51fd.json y
  previous-verification-coherence.txt (verificacion anterior y su coherencia), abajo.
  En RemoteFacts.TestArtifacts los Name apuntan a la copia original de la sesion (.../R20261011T194727Z-d7ea/ci-core/,
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
- Las pruebas: aplica el RequiredTests del contrato (filtro FullyQualifiedName~RackCad.Tests.StructuralAnalysisRamElementsDump, MinSelected 25, ExpectRed true) a los TRX
  (seleccion, superadas, fallidas) y contrasta con TestResults de la entrega y con el diff; esta entrega exige RED
  (ChainRedSha null en la delegacion). Comprueba que el RED falla por asercion o excepcion y no por compilacion (el TRX del RED
  existe y selecciona las pruebas). Que una parte de la seleccion pase ya en el RED (autopruebas de guardas y comprobaciones
  que no dependen del codigo nuevo) es admisible y la entrega lo declara en Evidence (precedentes: I-63 G4 29/26, I-65 F1-T1
  271/167, F1-T2 80/78, F3-T1 296/269 y F3-T2 241/230).
- RT = git diff --name-only <ChainBaseSha> <RedSha> -- tests/ ; comprueba que git diff --name-only RedSha CurrentSha no
  toca RT ni ChainRedFiles; si acreditas el RED, registra en la Evidence de Tests los ChainRedFiles resultantes.
- Cobertura de invariantes: para cada invariante del contrato, nombra la prueba del diff que lo observa (lee su codigo con
  git show CurrentSha:<ruta>) y su resultado en el TRX de CurrentSha: el script fijo (imports solo del modulo del proveedor, json y math; carga util ASCII decodificada por json.loads con la
  carpeta validada por la regla de la carpeta del paquete; puerta de capacidad antes de todo; un unico ram-dump.json en modo x dentro de
  la carpeta embebida; script ASCII; DUMP OK o estado de fallo), solo lectura y contrato de unidades (guarda estatica con autoprueba
  sobre variantes sinteticas: add, setter, analyze, units.set incondicional, sin restauracion en finally, otro open; par inicial, par
  de lectura y restauracion registrados), contenido del volcado y funciones del nucleo de la puerta contenidas en las 207 de P1, lector
  estricto (propiedades desconocidas, duplicadas o de tipo erroneo -> Invalid; estado de fallo -> rechazo; par distinto de LB_IN ->
  Invalid con el codigo del par de unidades), conversion al marco neutral (inversa de D-04 en puntos, vectores y banderas) y
  posicion absoluta de cada carga (incluida la bandera de porcentaje), ida y vuelta de REF-CANT-01 con tolerancia 1e-9, lectores de la
  lectura de vuelta (aprobado solo con READBACK OK; ningun estado de fallo ni rechazo gate aprobado) y del manifiesto (huella y version),
  tipos neutrales sin identidad de proveedor, sin Systems ni Providers, sin literales de conversion y sin E/S, e INV-14 y forma del
  cambio (ninguna prueba existente cambia; la exencion de INV-14 sigue en cuatro rutas exactas; ningun literal partido). Un invariante con prueba exigida y sin prueba
  que lo observe hace Tests = fail.
- NUEVA VERIFICACION AUTORIZADA POR EL OWNER sobre el mismo CurrentSha 76db7642d1bb9494c7f70155d3d3cabe45152bb3. Historia: la
  entrega declara WorkerStatus BLOCKED, Disposition STOP y C-08 por UNA invocacion python --version con la salida descartada,
  posterior a sus dos commits (causa raiz en inputs/analysis-f4t3.md); el Coordinator decidio resolverla sin cambio del trabajo
  (inputs/coordinator-dispositions-f4t3.md, CD-I65-F4-14); la verificacion anterior R20261011T192137Z-51fd (copia en
  inputs/previous-verification-R20261011T192137Z-51fd.json, con su comprobacion de coherencia en
  inputs/previous-verification-coherence.txt) devolvio EXECUTION_BLOCKED/STOP con FailureClass StopCondition y las 14 comprobaciones
  en pass, combinacion que incumple la regla mecanica de coherencia de README §8 (INVALID_OUTPUT); esa salida no se modifica.
- DISPOSICION DEL OWNER (texto literal en inputs/owner-disposition-f4t3.original.txt, SHA-256
  66124E3CCC090F9D57F5C664D6DB7BF4424DC78DC2D824F78F94141220B51C00; se versionara con esos mismos bytes en
  docs/automation/decisions/I-65-owner-disposition-f4t3.original.txt despues de esta verificacion, como las disposiciones anteriores,
  para no mover HEAD de CurrentSha): AUTOMATION_PLAN 16.11 reserva al Owner la decision sobre un STOP cuando la materia lo requiere;
  el Owner acepta la desviacion C-08 exclusivamente para esta ejecucion historica de F4-T3, bajo los hechos que enumera, sin eliminar
  ni debilitar C-08 para ejecuciones futuras, y autoriza esta verificacion incorporando su disposicion. Comprueba tu mismo esos hechos
  (invocacion posterior al commit GREEN; ningun archivo del repositorio modificado; pruebas y producto sin cambios; arbol limpio;
  CurrentSha 76db7642; CI del GREEN en verde) en el registro del trabajo, en Git y en los TRX.
- Tu salida debe ser coherente con README §8: EXECUTION_VERIFIED si y solo si las 14 comprobaciones estan en pass (FailureClass
  NONE); una clasificacion distinta exige al menos una comprobacion que no este en pass, con FailureClass = la primera de ellas (o
  StopCondition junto a ella). Si concluyes que la disposicion del Owner no puede resolver C-08 sin una modificacion formal de
  autoridad, dilo expresamente en Findings y expresalo de forma coherente (la comprobacion que lo evidencia en fail). Decide segun la
  evidencia; nadie te pide un resultado concreto. Esta es la PRIMERA delegacion de la cadena (Attempt 2 de la unidad; ChainRedSha
  null). Los Name de RemoteFacts.TestArtifacts del registro del trabajo apuntan al directorio de la verificacion anterior
  (R20261011T192137Z-51fd); las copias de inputs/ tienen los mismos SHA-256.
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
- TaskId F4-T3-DUMP-AND-READERS; RunId R20261011T194727Z-d7ea; DelegationRunId R20261011T183102Z-7511;
  WorkRunId R20261011T183618Z-05a9; Attempt 2; VerifiedSha = el CurrentSha verificado solo si Classification es
  EXECUTION_VERIFIED; si no, null.
- Findings: hallazgos concretos y comprobables (desviaciones declaradas, limitaciones de frontera, diferencias entre
  la entrega y Git); RecommendedNextAction en una frase.
