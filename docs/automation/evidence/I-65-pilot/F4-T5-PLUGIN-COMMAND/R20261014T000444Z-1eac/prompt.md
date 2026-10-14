I-65 — DELEGACION F4-T5-PLUGIN-COMMAND — Controller, perfil CONTROLLER_VERIFICATION
Identidad: unidad I-65; gate F4; tarea F4-T5-PLUGIN-COMMAND; intento 6; RunId R20261014T000444Z-1eac;
           DelegationRunId R20261013T234354Z-7203; WorkRunId R20261013T234619Z-b5ef;
           AuthorityRevision 2151ef557a116cd45eb127d8205f902ae8d335a6; MainSha bb0d5522e8411f66a51fdfb3f1f0d0514b737453;
           BaseSha y CurrentSha = los de la entrega; rama architecture/interop-analisis-estructural;
           worktree = el directorio de trabajo actual; Owner subagent:R20261013T234354Z-7203
Autoridades (leer desde Git, no copiar): las de la delegacion (campo Authorities); en particular
           docs/AUTOMATION_PLAN.md §16 (16.3 lectura de autoridades, 16.8, 16.9 comprobaciones, 16.10 terminos de gate)
           y docs/automation/agent-execution/README.md §8 (EXTERNAL); docs/initiatives/I-65-proposal-v3.1.md (Frozen: yes),
           docs/initiatives/I-65-f4-implementation.md (concrecion de F4, §6 y §11), docs/automation/decisions/I-65.md §37, §41 y §42 (UNIT_DOC);
           ADR-0050 (UNIT_CHANGE); AGENTS.md (EXTERNAL); vigencia: AuthorityRevision para la unidad, MainSha para lo demas
Objetivo: verificar la entrega del Worker contra Git, la delegacion, el contrato y los registros del relevo, y emitir la
           verificacion (esquema rackcad-controller-verification/v1)
Alcance: solo lectura. No escribas ningun archivo, no hagas commit ni push.
Invariantes del Freeze: los del contrato: comando minimo de D-13, fallo cerrado y visible, perfil solo de la tabla, prohibiciones de D-13 e INV-12 con su guarda,
           distribucion de la tabla en la salida de compilacion, ediciones exactas de cuatro pruebas existentes, compilacion, forma
           del cambio y restricciones del Owner, regla de censos frente a I-64 y controles operativos del Owner
Rutas y archivos calientes: las entradas de artifacts/orchestration/I-65/F4-T5-PLUGIN-COMMAND/6/R20261014T000444Z-1eac/inputs/
           (abajo) y el rango BaseSha..CurrentSha de la rama
Criterios de aceptacion: la salida valida contra el esquema; las 14 comprobaciones evaluadas en el orden fijo de
           AUTOMATION_PLAN 16.9 con evidencia concreta; Classification, Disposition y FailureClass coherentes con ellas
Evidencia requerida: ordenes de git reales en el worktree (rev-parse, merge-base --is-ancestor, diff --name-only,
           log --format=%B, check-ignore, show) y lectura de los TRX; cita en cada Evidence lo que obtuviste
No-touch: todo el repositorio
Condiciones de parada: las del contrato (S-02..S-09, S-12..S-14, P-01..P-08, C-01..C-12); ante cualquiera,
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
  artifacts/orchestration/I-65/F4-T5-PLUGIN-COMMAND/6/R20261014T000444Z-1eac/inputs/:
  gate-contract.json (contrato de gate del Coordinator); delegation.json (paquete aceptado, RunId R20261013T234354Z-7203);
  relay-record-planning.json (registro de la planificacion, con la aceptacion A1-A8); worker-handoff.json (la entrega; hace
  las veces de la de ExpectedHandoffPath con {WorkRunId} = R20261013T234619Z-b5ef); relay-record-work.json (registro del
  trabajo: terminacion, cesion, relevo de entrada, hechos remotos RemoteFacts, modelo y effort efectivos del Worker y la senal
  de denegaciones en Notes); ci-core/38487208404/core.trx (TRX Core de la corrida push del RedSha, RemoteFacts.RedRun),
  ci-core/38487578349/core.trx y ci-ui/38487578349/ui.trx (TRX de la corrida push del CurrentSha, RemoteFacts.CurrentRun); coordinator-dispositions-f4t5c3.md (disposiciones del Coordinator), scope-mechanical.json y
  scope-mechanical-red1.json (alcance mecanico de la sesion) y analysis-attempt5.md (causa raiz del REWORK del intento 5), abajo.
  En RemoteFacts.TestArtifacts los Name apuntan a la copia original de la sesion (.../R20261014T000444Z-1eac/ci-core/,
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
- Las pruebas: aplica el RequiredTests del contrato (filtro FullyQualifiedName~RackCad.Tests.StructuralAnalysisPluginCommand, MinSelected 12, ExpectRed true) a los TRX
  (seleccion, superadas, fallidas) y contrasta con TestResults de la entrega y con el diff; esta entrega exige RED
  (ChainRedSha null en la delegacion). Comprueba que el RED falla por asercion o excepcion y no por compilacion (el TRX del RED
  existe y selecciona las pruebas). Que una parte de la seleccion pase ya en el RED (autopruebas de guardas y comprobaciones
  que no dependen del codigo nuevo) es admisible y la entrega lo declara en Evidence (precedentes: I-63 G4 29/26, I-65 F1-T1
  271/167, F1-T2 80/78, F3-T1 296/269 y F3-T2 241/230).
- RT = git diff --name-only <ChainBaseSha> <RedSha> -- tests/ ; comprueba que git diff --name-only RedSha CurrentSha no
  toca RT ni ChainRedFiles; si acreditas el RED, registra en la Evidence de Tests los ChainRedFiles resultantes.
- Cobertura de invariantes: para cada invariante del contrato, nombra la prueba del diff que lo observa (lee su codigo con
  git show CurrentSha:<ruta>) y su resultado en el TRX de CurrentSha: archivo del comando unico con [CommandMethod( una sola vez y RACKRAMPAQUETE solo en el atributo, flujo de D-13 (seleccion,
  kind, estacion, cargas, carpeta nueva, catalogo, tabla junto al ensamblado, entrada de F4-T2, paquete y publicacion con la carpeta
  embebida), fallo cerrado y visible a nivel del comando (reglas C15-C18 de la guarda: una sola publicacion despues de las tres
  comprobaciones != Valid; cada rama no Valid informa con su resultado y sus diagnosticos y vuelve antes de publicar; ayudante visible
  con editor.WriteMessage; ninguna otra escritura de archivos; con autopruebas por variante) y con la tabla del producto (Unsupported
  sin paquete), perfil solo de la tabla, prohibiciones de D-13/INV-12, Content del .csproj, censo de nombres y conteo 37 -> 38 solo
  con RACKRAMPAQUETE, listas de G1 con la unica ruta del comando y forma del cambio. El invariante de controles operativos es de
  proceso: evaluarlo con relay-record-work.json, no con una prueba. Un invariante con prueba exigida y sin prueba
  que lo observe hace Tests = fail.
- DISPOSICIONES DEL COORDINATOR (texto literal en inputs/coordinator-dispositions-f4t5c3.md): D1 la cadena (ChainBaseSha 283ed613;
  ChainRedSha a2a8b868 acreditado; 21 ChainRedFiles; RED por correccion desactivada y GREEN que restaura f1d9a51d); D2 dos commits RED
  (28fe112e desactivacion sin pruebas y e413b9a2 con las reglas): desviacion de procedimiento sin efecto, el RedSha es e413b9a2; D3 la
  observacion a nivel del comando es estructural (no hay seam permitido). CORRECCION FINAL (intento 6; CorrectionOf R20261013T233523Z-b1dd,
  Tests): esta entrega EXIGE RED. El alcance mecanico de la sesion esta en inputs/scope-mechanical.json; evalua Scope tu mismo, archivo por
  archivo, contra el AllowedWriteScope de la delegacion de entrada.
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
- TaskId F4-T5-PLUGIN-COMMAND; RunId R20261014T000444Z-1eac; DelegationRunId R20261013T234354Z-7203;
  WorkRunId R20261013T234619Z-b5ef; Attempt 6; VerifiedSha = el CurrentSha verificado solo si Classification es
  EXECUTION_VERIFIED; si no, null.
- Findings: hallazgos concretos y comprobables (desviaciones declaradas, limitaciones de frontera, diferencias entre
  la entrega y Git); RecommendedNextAction en una frase.
