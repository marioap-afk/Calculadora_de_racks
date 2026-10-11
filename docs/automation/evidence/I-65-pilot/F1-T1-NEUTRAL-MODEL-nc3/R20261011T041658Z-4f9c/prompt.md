I-65 — DELEGACION F1-T1-NEUTRAL-MODEL — Controller, perfil CONTROLLER_VERIFICATION
Identidad: unidad I-65; gate F1; tarea F1-T1-NEUTRAL-MODEL; intento 0; RunId R20261011T041658Z-4f9c;
           DelegationRunId R20261011T032206Z-7516; WorkRunId R20261011T032758Z-3556;
           AuthorityRevision 9d0c661968542350929af267ada689b943898206; MainSha bb0d5522e8411f66a51fdfb3f1f0d0514b737453;
           BaseSha y CurrentSha = los de la entrega; rama architecture/interop-analisis-estructural;
           worktree = el directorio de trabajo actual; Owner subagent:R20261011T032206Z-7516
Autoridades (leer desde Git, no copiar): las de la delegacion (campo Authorities); en particular
           docs/AUTOMATION_PLAN.md §16 (16.3 lectura de autoridades, 16.8, 16.9 comprobaciones, 16.10 terminos de gate)
           y docs/automation/agent-execution/README.md §8 (EXTERNAL); docs/initiatives/I-65-proposal-v3.1.md (Frozen: yes),
           docs/automation/decisions/I-65.md §9, §10 y §11 (CD-I65-F1-01..10) (UNIT_DOC); ADR-0050 (UNIT_CHANGE); AGENTS.md
           (EXTERNAL); vigencia: AuthorityRevision para la unidad, MainSha para lo demas
Objetivo: verificar la entrega del Worker contra Git, la delegacion, el contrato y los registros del relevo, y emitir la
           verificacion (esquema rackcad-controller-verification/v1)
Alcance: solo lectura. No escribas ningun archivo, no hagas commit ni push.
Invariantes del Freeze: los del contrato: INV-01, 02, 03, 04, 05 (partes neutrales), 11, 14, 15 y 16, con D-02, D-03, D-06,
           D-07, D-08, D-10b, D-12 y D-13, la condicion de supuestos (Q-O3) y la forma del cambio
Rutas y archivos calientes: las entradas de artifacts/orchestration/I-65/F1-T1-NEUTRAL-MODEL-nc3/0/R20261011T041658Z-4f9c/inputs/
           (abajo) y el rango BaseSha..CurrentSha de la rama
Criterios de aceptacion: la salida valida contra el esquema; las 14 comprobaciones evaluadas en el orden fijo de
           AUTOMATION_PLAN 16.9 con evidencia concreta; Classification, Disposition y FailureClass coherentes con ellas
Evidencia requerida: ordenes de git reales en el worktree (rev-parse, merge-base --is-ancestor, diff --name-only,
           log --format=%B, check-ignore, show) y lectura de los TRX; cita en cada Evidence lo que obtuviste
No-touch: todo el repositorio
Condiciones de parada: las del contrato (S-02..S-09, S-12..S-14, P-01..P-08, C-01..C-10); ante cualquiera,
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
  artifacts/orchestration/I-65/F1-T1-NEUTRAL-MODEL-nc3/0/R20261011T041658Z-4f9c/inputs/:
  gate-contract.json (contrato de gate del Coordinator); delegation.json (paquete aceptado, RunId R20261011T032206Z-7516);
  relay-record-planning.json (registro de la planificacion, con la aceptacion A1-A8); worker-handoff.json (la entrega; hace
  las veces de la de ExpectedHandoffPath con {WorkRunId} = R20261011T032758Z-3556); relay-record-work.json (registro del
  trabajo: terminacion, cesion, relevo de entrada, hechos remotos RemoteFacts, modelo y effort efectivos del Worker y la senal
  de denegaciones en Notes); ci-core/38109425352/core.trx (TRX Core de la corrida push del RedSha, RemoteFacts.RedRun),
  ci-core/38109857453/core.trx y ci-ui/38109857453/ui.trx (TRX de la corrida push del CurrentSha, RemoteFacts.CurrentRun).
  En RemoteFacts.TestArtifacts los Name apuntan a la copia original de la sesion (.../R20261011T040119Z-5751/ci-core/,
  ci-ui/ y ci-core-red/); los Sha256 son los de estas mismas copias.
- Las senales con actor «sesion -> Controller» de 16.9 (Termination, Remote, Ci, Routing, Denials) salen del registro
  del trabajo; la fecha de la entrega es Outcome.OutputWrittenUtc del registro y el inicio, Cession.StartUtc.
- Semantica del registro del trabajo (README §3.1 y §3.4): Exit es el estado ANTES de invocar al Worker (HEAD = remoto =
  BaseSha) y Entry el estado DESPUES; que HEAD y ls-remote pasen de BaseSha a CurrentSha entre ambos es lo esperado,
  porque el Worker hace commit y push durante la cesion. Exit.Processes incluye, ademas, los session-descendant de la sesion
  (ayudantes del escritorio) como linea base; Entry.Processes es la foto de entrada, y las Notes del registro documentan la
  relectura por PID de dos bash.exe efimeros creados despues del fin de la cesion.
- Las pruebas: aplica el RequiredTests del contrato (filtro FullyQualifiedName~RackCad.Tests.StructuralAnalysis,
  MinSelected 40, ExpectRed true) a los TRX (seleccion, superadas, fallidas) y contrasta con TestResults de la entrega y
  con el diff; esta entrega exige RED (ChainRedSha null en la delegacion). Comprueba que el RED falla por asercion o excepcion
  y no por compilacion (el TRX del RED existe y selecciona las pruebas). Que una parte de la seleccion pase ya en el RED
  (autopruebas de guardas sobre codigo sintetico, comprobaciones estructurales) es admisible y la entrega lo declara en
  Evidence; el RED se acredita si las pruebas del filtro estan entre las fallidas del TRX del RedSha en la medida que
  observan el cambio (precedente: I-63 G4, 29 seleccionadas y 26 fallidas).
- RT = git diff --name-only <ChainBaseSha> <RedSha> -- tests/ ; comprueba que git diff --name-only RedSha CurrentSha no
  toca RT ni ChainRedFiles; si acreditas el RED, registra en la Evidence de Tests los ChainRedFiles resultantes.
- Cobertura de invariantes: para cada invariante del contrato, nombra la prueba del diff que lo observa (lee su codigo con
  git show CurrentSha:<ruta>) y su resultado en el TRX de CurrentSha: INV-01 (G1 con lista de permiso vacia, G2, G3, con
  autoprueba), INV-02 y G4 (cantidades sin constructor ni conversion desde double; literales prohibidos en StructuralAnalysis),
  INV-03 (conversiones exactas y tolerancia 1e-12), INV-15 y D-10b (lector estricto con la forma de CD-I65-F1-05), INV-16
  (propietarios unicos, igualdad bit a bit de StructuralSectionUnits, guarda de src/), D-02 y D-08 (modelo, resultado,
  precedencia CD-I65-F1-04, solo Valid expone el modelo), D-06/D-07 (material solo por el perfil; MaterialGrade sin
  efecto; seccion desconocida Invalid; C/L y sistema no US_CUSTOMARY Unsupported; propiedades ausentes explicitas), INV-05
  (partes neutrales), INV-04 y D-12 (estabilidad y sensibilidad por componente), D-13 (BuildStructuralModel con doble de
  prueba), INV-11 e INV-14 (guardas con autoprueba). La condicion de supuestos (Q-O3) y la forma del cambio (namespaces,
  ninguna prueba existente tocada, sin Systems/Providers/Domain/Plugin/UI/.csproj, solo BCL) se juzgan por el codigo y el
  diff, no exigen una prueba propia. Un invariante con prueba exigida y sin prueba que lo observe hace Tests = fail.
- Alcance: produccion solo en src/RackCad.Application/StructuralAnalysis/ (fuera de Providers/),
  src/RackCad.Application/Units/MassUnits.cs, src/RackCad.Application/Units/ForceUnits.cs y
  src/RackCad.Application/StructuralSections/StructuralSectionUnits.cs; git diff --name-status BaseSha..CurrentSha -- tests/
  solo contiene altas (A) bajo tests/RackCad.Tests/StructuralAnalysis/; las pruebas nuevas declaran namespace RackCad.Tests
  y sus clases empiezan por StructuralAnalysis. Comprueba el alcance MECANICAMENTE (cada archivo del diff cubierto por
  AllowedWriteScope y ninguno por ForbiddenWriteScope), no por impresion.
- Remote: el registro del trabajo trae RemoteFacts.OriginMainSha (git fetch origin; git rev-parse origin/main) y
  RemoteFacts.LsRemoteSha; el esquema rackcad-relay-record/v1 no tiene OriginMainSha en Entry, asi que no lo busques alli.
  Si tu sandbox no puede ejecutar git ls-remote, usa RemoteFacts y dilo en la Evidence.
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
- TaskId F1-T1-NEUTRAL-MODEL; RunId R20261011T041658Z-4f9c; DelegationRunId R20261011T032206Z-7516;
  WorkRunId R20261011T032758Z-3556; Attempt 0; VerifiedSha = el CurrentSha verificado solo si Classification es
  EXECUTION_VERIFIED; si no, null.
- Findings: hallazgos concretos y comprobables (desviaciones declaradas, limitaciones de frontera, diferencias entre
  la entrega y Git); RecommendedNextAction en una frase.
