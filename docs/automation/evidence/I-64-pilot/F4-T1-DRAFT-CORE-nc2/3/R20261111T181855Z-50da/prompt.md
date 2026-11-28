I-64 - DELEGACION F4-T1-DRAFT-CORE - Controller, perfil CONTROLLER_VERIFICATION
Identidad: unidad I-64; gate F4; tarea F4-T1-DRAFT-CORE; intento 3; RunId R20261111T181855Z-50da;
           DelegationRunId R20261110T224523Z-d088; WorkRunId R20261111T132115Z-6453;
           AuthorityRevision c31854f81c3c301eba53e35cd99c66bcaf97e587; MainSha 812baf879612320653094a8aaf46aca0bac81383;
           BaseSha y CurrentSha = los de la entrega (CurrentSha = ce73ceb719cbc08ee4b1ee0f989ac2e5b4258649, la entrega de producto);
           HEAD = remoto = 61d83788e7fa8decd861626b7e3ca89c8f23b9b4 (punta de custodia y autoridad de la sesion, NO es CurrentSha); rama architecture/workspace-persistente-rackcad;
           worktree = el directorio de trabajo actual; Owner subagent:R20261110T224523Z-d088
Autoridades (leer desde Git, no copiar): las de la delegacion (campo Authorities); en particular
           docs/AUTOMATION_PLAN.md section 16 (16.3 lectura de autoridades, 16.8, 16.9 comprobaciones, 16.10 terminos de gate)
           y docs/automation/agent-execution/README.md section 8 (EXTERNAL); AGENTS.md (EXTERNAL);
           vigencia: AuthorityRevision para la unidad, MainSha para lo demas
Objetivo: verificar la entrega del Worker contra Git, la delegacion, el contrato y los registros del relevo, y emitir la
           verificacion (esquema rackcad-controller-verification/v1)
Alcance: solo lectura. No escribas ningun archivo, no hagas commit ni push. No ejecutes dotnet ni builds: usa los TRX.
Invariantes del Freeze: los 27 de la delegacion, vinculantes en su totalidad
Rutas y archivos calientes: las entradas de artifacts/orchestration/I-64/F4-T1-DRAFT-CORE-nc2/3/R20261111T181855Z-50da/inputs/ (abajo) y el rango
           BaseSha..CurrentSha de la rama
Criterios de aceptacion: la salida valida contra el esquema; las 14 comprobaciones evaluadas en el orden fijo de
           AUTOMATION_PLAN 16.9 con evidencia concreta; Classification, Disposition y FailureClass coherentes con ellas
Evidencia requerida: ordenes de git reales en el worktree (rev-parse, merge-base --is-ancestor, diff --name-only,
           log --format=%B, check-ignore, show, status --porcelain) y lectura de los TRX; cita en cada Evidence lo que obtuviste
No-touch: todo el repositorio
Condiciones de parada: S-02, S-03, S-04, S-12, S-13, P-03 y F4T1-C01..F4T1-C07 de la delegacion; ante cualquiera,
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
  artifacts/orchestration/I-64/F4-T1-DRAFT-CORE-nc2/3/R20261111T181855Z-50da/inputs/:
  gate-contract.json; delegation.json; relay-record-planning.json (con A1-A8 en sus Notes; nc4 ya satisfecho en la cadena por R20261110T181147Z-4f9b);
  worker-handoff.json (hace las veces de la de ExpectedHandoffPath con {WorkRunId} = R20261111T132115Z-6453);
  relay-record-work.json (terminacion, cesion, relevo de entrada, hechos remotos RemoteFacts, modelo y effort efectivos del Worker, denegaciones y procesos en Notes);
  ci-core/43029959059/core.trx de la corrida push del CurrentSha; ci-core-red/43029071176/core.trx de la corrida push del RedSha.
  coordinator-disposition.md: textos literales del Owner: la autorizacion de la sustitucion de Identity tras la custodia y la disposicion de esta segunda reverificacion, que sustituye solo la parte de red de la condicion A (autoridad de la unidad para Identity en esta reverificacion y en nc1..nc3).
  session-remote-facts.json: hecho de la sesion -> Controller: git ls-remote de la rama medido por la sesion justo antes de la cesion; es la fuente del remoto real para la condicion A.
  reissue-record.json: F4T1-HANDOFF-REISSUE-01 (autoridad: docs/automation/evidence/I-64-pilot/F4-DRAFTS/coordinator/owner-f4t1-handoff-reissue.md, F4-E12): SHA-256 de la entrega original y de la reemitida, diff estructurado y las diez comprobaciones previas.
  post-custody-identity-proof.json: prueba de la sesion de que CurrentSha es antecesor de HEAD y de que git diff --name-only CurrentSha..HEAD solo contiene rutas de docs/ (lista exacta). Compruebalo tu mismo con git; no te fies de ella.
- SEGUNDA Y ULTIMA REVERIFICACION: la reverificacion R20261111T175635Z-4bb6 (STOP por Identity A, defecto del prompt que pedia git ls-remote al Controller) se conserva como historia; no la evalues.
- REEMISION DE LA ENTREGA (F4-E12): esta es una reverificacion del MISMO intento 3, no un intento 4. worker-handoff.json es la entrega reemitida de la misma ejecucion del Worker (mismo RunId, DelegationRunId, Attempt, BaseSha, RedSha, CurrentSha, pruebas y CI); solo cambia en WorkCompleted la cita del nombre de un archivo auxiliar. relay-record-work.json es el registro de la misma ejecucion con OutputSha256 de la entrega reemitida y una nota que lo documenta; sus hechos remotos son los de la entrada del relevo del Worker, cuando HEAD = remoto = CurrentSha. La verificacion R20261111T140847Z-ce94 (REWORK por FreeText) y la entrega original se conservan como historia; no las evalues.
- SUSTITUCION DE Identity TRAS LA CUSTODIA (autoridad: coordinator-disposition.md; solo esta reverificacion y nc1..nc3). Sustituye UNICAMENTE la parte HEAD = refs/remotes/origin/<rama> = CurrentSha de Identity por estas condiciones, todas obligatorias:
  A. HEAD y refs/remotes/origin/architecture/workspace-persistente-rackcad son iguales entre si y a la punta de custodia 61d83788e7fa8decd861626b7e3ca89c8f23b9b4 (comprobacion local con git rev-parse). NO ejecutes git ls-remote ni ninguna otra operacion Git de red: tu sandbox no tiene red. La igualdad con el remoto real la aporta la SESION en session-remote-facts.json (git ls-remote medido por la sesion inmediatamente antes de esta cesion), que debe dar la misma punta;
  B. CurrentSha de la entrega es antecesor de HEAD (git merge-base --is-ancestor CurrentSha HEAD);
  C. toda ruta de git diff --name-only CurrentSha..HEAD esta bajo docs/;
  D. ninguna ruta de ese diff esta bajo src/ ni bajo tests/;
  E. no hay ningun otro cambio de producto, build o fuente entre CurrentSha y HEAD;
  F. BaseSha es antecesor de CurrentSha;
  G. RedSha esta en la cadena BaseSha..CurrentSha;
  H. la rama es la esperada;
  I. el worktree es el esperado y esta limpio.
  Si alguna falla, Identity = fail. CurrentSha sigue siendo el de la entrega (ce73ceb7); no lo sustituyas por HEAD. El resto de Identity y de 16.9 no cambia.
- config.toml: en la salida y en la entrada del relevo el hash es la SNAPSHOT F ratificada por el Owner, docs/automation/evidence/I-64-pilot/F4-DRAFTS/f4-dispositions.md, F4-E03 (AAEB58D8..., 104 nombres); no es un STOP de esta verificacion.
- Las senales con actor "sesion -> Controller" de 16.9 (Termination, Remote, Ci, Routing, Denials) salen del registro
  del trabajo; la fecha de la entrega es Outcome.OutputWrittenUtc del registro y el inicio, Cession.StartUtc.
- Semantica del registro del trabajo (README section 3.1 y section 3.4): Exit es el estado ANTES de invocar al Worker (HEAD = remoto = BaseSha) y Entry el estado DESPUES.
- Routing y transporte subagent: el modelo solicitado es el campo Model de la delegacion. El transporte subagent solo admite como parametro de lanzamiento el alias de familia, y Participant.ModelRequested del registro del trabajo guarda ese parametro literal; el modelo real de cada turno esta en Participant.ModelEffective y EffectiveSource.
- Procesos (16.4): participante es un proceso evaluado cuya linea de ordenes contiene la ruta del worktree. Los no atribuibles que desaparecen en la relectura quedan CLEARED_BY_REREAD; los hechos estan en las Notes del registro del trabajo.
- Autoridades: los valores Section son la transliteracion ASCII de los encabezados reales; localiza cada seccion comparando sin diacriticos, con la raya larga como guion y el signo de seccion como "section".
- Revisiones: AuthorityRevision = c31854f8; BaseSha = el de la entrega, hijo directo. 16.3 exige que AuthorityRevision sea ancestro de BaseSha y que git diff --name-only AuthorityRevision BaseSha no contenga rutas EXTERNAL ni con secciones UNIT_CHANGE; compruebalo. Las tres rutas de ese diff son UNIT_DOC y figuran en Authorities.
- Las pruebas y el RED: correccion bajo 16.8: ChainBaseSha es el de la primera delegacion de la cadena, ChainRedSha el RED acreditado vigente (500a336d) y CorrectionOf la verificacion anterior; esta entrega toca ChainRedFiles, asi que exige un RED nuevo, que es el RedSha entre BaseSha y CurrentSha. Aplica el RequiredTests del contrato a los TRX:
  tests/RackCad.Tests/RackCad.Tests.csproj, FullyQualifiedName~RackCad.Tests.Workspace, MinSelected 1273, ExpectRed true.
  El operador ~ de VSTest es "contiene", sin distinguir mayusculas, sobre el nombre completo: en el TRX, UnitTest/TestMethod className + "." + name; cuenta los UnitTestResult de esas pruebas. Contrasta con TestResults de la entrega y con el diff.
  RT = git diff --name-only BaseSha RedSha -- tests/. Si el RED queda acreditado, ChainRedFiles nuevos = los anteriores union RT; registralo en la Evidence de Tests. La CI ejecuta las pruebas en configuracion Debug.
- EXCEPCION ACOTADA A 16.8, SOLO PARA Checks.Tests DE ESTE INTENTO (autoridad de la unidad: docs/automation/evidence/I-64-pilot/F4-DRAFTS/coordinator/owner-f4t1-attempt3-exception.md y la disposicion F4-E10 de docs/automation/evidence/I-64-pilot/F4-DRAFTS/f4-dispositions.md; contrato INV-F4T1-26 e INV-F4T1-27):
  NO exijas que git diff --name-only RedSha CurrentSha excluya tests/RackCad.Tests/Workspace/WorkspaceDraftBoundaryTests.cs como archivo entero. En su lugar aplica este oraculo estrecho, y RedPart de Tests es pass solo si se cumplen los cuatro puntos:
  (a) las pruebas de regresion del bloque entre "// BEGIN N-1 regression (F4-T1 attempt 3)" y "// END N-1 regression (F4-T1 attempt 3)" estan en RedSha y estan entre las fallidas del TRX del RedSha, y en ese TRX no falla ninguna prueba fuera del bloque;
  (b) ejecuta desde el worktree python docs/automation/evidence/I-64-pilot/F4-T1-DRAFT-CORE/coordinator/n1_hashes.py <BaseSha> <RedSha> <CurrentSha> y exige Pass true: H1 (el bloque) y H2 (el archivo con la region de la regla sustituida) iguales en RedSha y CurrentSha, y H3 (el RED sin el bloque) igual al archivo de BaseSha; cita su salida;
  (c) git diff --name-only RedSha CurrentSha no contiene ningun archivo de RT ni de ChainRedFiles distinto de tests/RackCad.Tests/Workspace/WorkspaceDraftBoundaryTests.cs;
  (d) el TRX del CurrentSha da en verde el filtro y las pruebas del bloque.
  Cualquier cambio de pruebas, aserciones o datos de prueba en el GREEN es fail de Tests (REWORK). El resto de 16.8 y de 16.9 no cambia.
- Scope: git diff --name-only BaseSha..CurrentSha contra AllowedWriteScope (1 rutas exactas) y ForbiddenWriteScope (66 entradas) de la delegacion.
- La CI del CurrentSha: los cuatro jobs requeridos de AGENTS.md (RemoteFacts.CurrentRun del registro del trabajo).
- Trailer declarado: el campo Worker.Trailer de la entrega; modelo y effort efectivos: los del registro del trabajo.
- Terminos de gate: los de AUTOMATION_PLAN 16.10, en los campos de texto libre que alli se listan.
- Coherencia (README section 8): EXECUTION_VERIFIED si y solo si las 14 estan en pass (con las 14 en pass, FailureClass NONE);
  una parada se expresa con la comprobacion que la evidencia; FailureClass = la primera que no esta en pass (o StopCondition); Disposition segun la precedencia de 16.9 (STOP > BLOCKED > REWORK); con Result pass, RedPart nunca fail.
- TaskId F4-T1-DRAFT-CORE; RunId R20261111T181855Z-50da; DelegationRunId R20261110T224523Z-d088; WorkRunId R20261111T132115Z-6453; Attempt 3; VerifiedSha = el CurrentSha verificado solo si Classification es EXECUTION_VERIFIED; si no, null.
- Findings: hallazgos concretos y comprobables; RecommendedNextAction en una frase.
- Toda tu salida en ASCII: sin tildes ni signos no ASCII y sin secuencias de escape formadas por barra invertida + u.
