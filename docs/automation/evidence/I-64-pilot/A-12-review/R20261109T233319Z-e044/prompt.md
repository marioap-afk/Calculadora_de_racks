I-64 - REVISION FORMAL DEL ARCHITECT (ARCHITECTURE_REVIEW, effort semantico Deep) - A-12 / I64-IDENTITY-BRIDGE-01
RunId: R20261109T233319Z-e044. Modo: SEPARATE SESSION (sesion nueva del Codex CLI 0.167.0-alpha.3.2, solo lectura, modelo gpt-6.1-sol, effort high;
celda acreditada por la sonda R20261109T062522Z-e8be).
Independencia: el autor del objeto es la sesion principal de I-64 (Claude, otro proveedor); tu no participaste en su redaccion.
Insumos: solo los archivos listados; no hay memoria ni transcripcion del autor. Contexto inyectado: este prompt.

ANTECEDENTES: el control negativo nc1 de F2-T2A-NAVIGATION-HOST (intento 2) agoto su camino normal: las tres corridas del Controller
(3d86, 16b6, bfc8) no cumplieron el oraculo relativo. El Coordinator ordeno una via de evidencia sustituta solo para I-64 y solo para esta
ocurrencia, y decidio versionarla en una rama lateral temporal para no mover la rama de producto antes de nc2 y nc3. Revisa el objeto A-12 exacto.

OBJETO REVISADO (commit exacto 4206b2da3a9985f429b77480cd5b5d26377f3215 de la rama lateral temporal architecture/workspace-persistente-rackcad-a12;
lee con: git show 4206b2da3a99:<ruta>; registra cada blob con: git rev-parse 4206b2da3a99:<ruta>). El worktree sigue en la rama de producto en 44a692a4; no hagas checkout):
  - Freeze V5: git show 9b43dafbe3b8874aa0ba5a85d62cbf953dbdd311:docs/initiatives/I-64-proposal-v5.md (blob esperado dc1924ff...)
  - A-12 (enmienda revisada, PROPOSED / NOT ACTIVE): docs/initiatives/I-64-A-12.md
  - verificador: docs/automation/evidence/I-64-protocol-bridge/tools/verify_identity_bridge.py
  - autoprueba sintetica: docs/automation/evidence/I-64-protocol-bridge/tools/selftest_identity_bridge.py y su resultado docs/automation/evidence/I-64-protocol-bridge/selftest-identity-result-a12.json
  - demostracion de regresion: docs/automation/evidence/I-64-protocol-bridge/tools/regression_demo_identity_bridge.py y docs/automation/evidence/I-64-protocol-bridge/regression-demo-a12-identity.json
  - verificador de A-11 (fuente de las primitivas copiadas literalmente; A-12 afirma que no lo ejecuta): docs/automation/evidence/I-64-protocol-bridge/tools/verify_testfilter_bridge.py
    (blob f9780f2f30c067243c93b6ab7175cc4b3179ca39); compara las primitivas con ese blob
  - orden del Coordinator que abre A-12 (literal): docs/automation/evidence/I-64-protocol-bridge/coordinator-a12-substitute-order.md
  - disposicion de versionado (literal): docs/automation/evidence/I-64-protocol-bridge/coordinator-a12-versioning-disposition.md
  - disposiciones de nc1: docs/automation/evidence/I-64-protocol-bridge/coordinator-a12-nc1-3d86-disposition.md y docs/automation/evidence/I-64-protocol-bridge/coordinator-a12-nc1-final-retry-disposition.md
  - cambio exacto de la rama lateral: git diff --stat 44a692a4ec386e559cc6d330b3d78b87dbad6581 4206b2da3a99
  - evidencia docs/automation/evidence/I-64-evidence.md Sec.93 y estado docs/automation/state/I-64.yml en ese commit
CONTEXTO DE LA OCURRENCIA (ignorado por Git, solo como dato):
  - salidas del Controller: artifacts/orchestration/I-64/F2-T2A-NAVIGATION-HOST-nc1/2/<RunId>/output.json para R20261109T202056Z-3d86, R20261109T203840Z-16b6 y R20261109T212644Z-bfc8
  - las tres entradas reales vinculadas en la Sec.8 de la enmienda: puedes calcular su SHA-256 y leerlas como dato
AUTORIDADES (leer en origin/main a140eb3a9f055518f0ffa5a3f927ff1c0e1c1e60 con git show a140eb3a:<ruta>):
  - docs/INITIATIVE_LIFECYCLE.md Sec.3 (M-01..M-08), Sec.5 (participacion del Architect) y Sec.6 (Freeze y enmiendas A-n)
  - docs/AUTOMATION_PLAN.md Sec.16 (16.4 relevo, 16.9 comprobaciones Handoff e Identity, 16.10, 16.11)
  - docs/automation/agent-execution/README.md Sec.8 y Sec.10 (controles negativos, nc1)
  - docs/automation/agent-execution/schemas/worker-handoff.schema.json y relay-record.schema.json
  - las ordenes literales del Coordinator citadas arriba

RESPONDE EXPLICITAMENTE (campo Answers; Answer = YES o NO; Conforming = si esa respuesta deja la enmienda conforme):
 Q01 Is A-12 materiality correct: M-04 = YES and every other M flag = NO?
 Q02 Is A-12 limited to I-64 and to this exact F2-T2A nc1 occurrence and defect signature, with no global AUTOMATION_PLAN or protocol authority,
     no change to product semantics, and no redefinition of normal Handoff semantics outside this substitute?
 Q03 Is HandoffIntegrity computed strictly from the handoff artifacts, the relay record and the bound values, without consulting repository state,
     so that a false CurrentSha cannot fail it, while every real binding or artifact defect does fail it?
 Q04 Is Identity evaluated only after HandoffIntegrity PASS and a coherent repository state, comparing the candidate CurrentSha with the authoritative
     repository HEAD, with no ancestor or descendant exception and no post-custody semantics anywhere in the verifier or the amendment?
 Q05 Is ControlResult PASS reachable only with HandoffIntegrity PASS, Identity FAIL, the mutated CurrentSha equal to the exact bound synthetic SHA,
     and that SHA naming no object in the repository?
 Q06 Is mutation exactness complete and isolated: only the top-level CurrentSha differs, proven structurally (keys, nesting, order, exact numeric lexemes)
     and at byte level, with any second mutation or reformatting failing closed?
 Q07 Is the verifier fail-closed on every malformed, ambiguous, duplicate-keyed, schema-invalid, mismatched, stale or unavailable input (including Git
     and remote failures), with exit 0 only for ControlResult PASS and no default or skip path that reaches PASS?
 Q08 Is the occurrence binding exact: Sec.8 arguments and SHA-256 values identify the real F2-T2A attempt 2, the repository occurrence is the product
     branch at 44a692a4 with HEAD = remote = tracking, BaseSha and RedSha are on that chain, and any mismatch is a STOP before execution?
 Q09 Is the side-branch provenance sound: the commit differs from 44a692a4 only in docs/ paths within the content allowed by the versioning disposition,
     with no src/, product tests, F2-T2A implementation or F2-T2B change; verifier bytes extracted from the side-branch Git blob; the product branch not moved
     or switched; and a fast-forward-only incorporation with no merge commit, rebase or cherry-pick, and no branch deletion before it is verified?
 Q10 Are the relationships correct: verbatim reuse of the accepted A-11 primitives with A-11 consumed and not executed or reused as authority; A-4 and A-6
     unmodified; the 3d86, 16b6 and bfc8 outcomes retaining their historical classification; and P-04 history unmodified?
 Q11 Is there no semantic expansion into nc2 or nc3: A-12 alters neither their oracles, preconditions nor classification, and PASS records only
     nc1 = SATISFIED_UNDER_I64_IDENTITY_BRIDGE?
 Q12 Does the synthetic adversarial coverage include every case the Coordinator ordered (T01-T15) plus adequate adversarial cases (T16-T31), and does
     the regression show every mutant failing its intended cases, including one that reproduces the observed Controller defect?
 Q13 Does the pre-activation boundary hold: no real F2-T2A evidence processed by the bridge before Owner + Architect + Coordinator activation,
     with the real occurrence only bound by SHA-256 in the amendment?
 Q14 Does any path exist where an unverifiable or ambiguous required input produces PASS? (Answer YES = existe una ruta abierta)
 Q15 Is any new Owner or I-62 authority required beyond the explicit Owner decision that A-12 already requests? (Answer YES = se requiere)

REGLAS:
  - Solo lectura: no escribas archivos, no hagas commit, push ni checkout; usa git show / git diff / git log / git rev-parse / git ls-tree
    / git cat-file y lectura de archivos.
  - NO ejecutes verify_identity_bridge.py, la autoprueba ni la regresion contra ninguna entrada real de F2-T2A (entregas del Worker o de nc1, registros
    de relevo ni nada bajo artifacts/): A-12 no esta activa. Si ejercitas las herramientas, hazlo SOLO con entradas sinteticas (la autoprueba genera
    las suyas en un directorio temporal); si el sandbox de solo lectura lo impide, declaralo en Limitations.
  - Leer el codigo fuente o calcular el SHA-256 de un archivo no es ejecutar el puente.
  - Si una orden auxiliar falla, repitela por otro medio o declara la limitacion en Limitations; no concluyas sin evidencia.
  - Severidad (LIFECYCLE Sec.5 y perfil ARCHITECTURE_REVIEW): REQUIRED si activa M-01..08 no declarado sobre un elemento
    congelable, deja un invariante sin obligacion de prueba, permite una ruta abierta ante fallos, contradice una autoridad o un
    hecho medido, o deja algo no ejecutable o no verificable; una precision sin cambio de significado es OPTIONAL.
  - Cada hallazgo: Question, ubicacion (archivo:linea), defecto, autoridad, correccion minima, y si exige cambiar la semantica
    global de I-61 (RequiresGlobalI61Change) o una decision nueva del Owner (RequiresOwnerDecision).
  - Verdict = AGREED si y solo si RequiredCount = 0. Si falta autoridad reservada del Owner: BLOCKED - OWNER DECISION.
    Toda respuesta con Conforming = false exige al menos un hallazgo REQUIRED con esa Question.
  - Separa los hallazgos REQUIRED y OPTIONAL por su campo Severity.
  - Prohibido declarar GATE PASS, Candidato, cierre, integracion o aprobacion del Owner. Toda la salida en ASCII
    (sin tildes ni escapes con barra invertida + u; nombra un caracter no ASCII como U+XXXX). Rellena ReviewSubject con los blobs que obtengas.

PERFIL ARCHITECTURE_REVIEW
Metodo: revision adversarial contra las autoridades citadas, sin escribir.
1. Lee el Freeze, los documentos duenos y el codigo afectado desde la revision fijada.
2. Cada hallazgo: ubicacion (archivo:linea), defecto, autoridad que contradice y correccion minima.
3. Severidad: REQUIRED solo si contradice una autoridad o un hecho medido, deja algo congelado ambiguo, impide ejecutar un
   paso o deja una garantia sin verificar.
4. No reabras lo ya resuelto; prefiere pocos hallazgos de alta confianza.

Informe esperado: la salida exacta del esquema rackcad-i64-architect-review/v1 (la escribe el CLI por -o).
