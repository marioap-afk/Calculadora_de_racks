I-64 - RE-REVISION FORMAL DEL ARCHITECT (ARCHITECTURE_REVIEW, effort semantico Deep) - A-9 / I64-TESTFILTER-BRIDGE-01
RunId: R20261109T112612Z-04e8. Modo: SEPARATE SESSION (sesion nueva del Codex CLI 0.167.0-alpha.3.2, solo lectura, modelo gpt-6.1-sol, effort high;
celda acreditada por la sonda R20261109T062522Z-e8be).
Independencia: el autor del objeto es la sesion principal de I-64 (Claude, otro proveedor); tu no participaste en su redaccion.
Insumos: solo los archivos listados (versionados en Git); no hay memoria ni transcripcion del autor ni de la revision anterior.
Contexto inyectado: este prompt.

ANTECEDENTE: la revision formal R20261109T062937Z-e1d0 de A-8 (commit d4a97950c2757d09f971a66621e7262e385ad818) dio CHANGES REQUIRED con
3 REQUIRED (I64-A8-ORDINAL-FILTER-MISMATCH Q03, I64-A8-COUNTER-COHERENCE Q04, I64-A8-DUPLICATE-RESULT-FAIL-OPEN Q11). El Coordinator los
acepto y ordeno A-9, que corrige A-8 y la sustituye solo para la activacion; A-8 queda historica e inmutable. Despues dispuso la semantica
de coincidencia potencial (no el rechazo global de los FQN no ASCII). Revisa el objeto A-9 exacto.

OBJETO REVISADO (commit exacto 1a2fd4e92c38a2b5bcc17b7f09b86342fa7ef2e2 de la rama architecture/workspace-persistente-rackcad;
lee con: git show 1a2fd4e92c38:<ruta>; registra cada blob con: git rev-parse 1a2fd4e92c38:<ruta>):
  - Freeze V5: git show 9b43dafbe3b8874aa0ba5a85d62cbf953dbdd311:docs/initiatives/I-64-proposal-v5.md (blob esperado dc1924ff...)
  - A-9 (enmienda revisada, PROPOSED / NOT ACTIVE): docs/initiatives/I-64-A-9.md
  - A-8 (historica, inmutable, CHANGES REQUIRED, NOT ACTIVE): docs/initiatives/I-64-A-8.md
  - verificador corregido: docs/automation/evidence/I-64-protocol-bridge/tools/verify_testfilter_bridge.py
  - autoprueba sintetica: docs/automation/evidence/I-64-protocol-bridge/tools/selftest_testfilter_bridge.py y su resultado docs/automation/evidence/I-64-protocol-bridge/selftest-testfilter-result-a9.json
  - demostracion de regresion: docs/automation/evidence/I-64-protocol-bridge/tools/regression_demo_testfilter_bridge.py y docs/automation/evidence/I-64-protocol-bridge/regression-demo-a9-testfilter.json
    (incluye la ejecucion de la autoprueba contra el blob historico de A-8 1691003d2286a3c8f597a182597e6622da074bd3)
  - cambio exacto frente a A-8: git diff d4a97950c2757d09f971a66621e7262e385ad818 1a2fd4e92c38 -- docs/automation/evidence/I-64-protocol-bridge/tools/
  - verificador de A-6 (fuente de las primitivas copiadas literalmente; A-9 afirma que no lo ejecuta): docs/automation/evidence/I-64-protocol-bridge/tools/verify_freetext_bridge.py
    (blob 3bc7869e9f4c9c5eed0d64d7b8fee6f7002b9169); compara el bloque marcado en el verificador con ese blob
  - verificador de A-4 (fuente del endurecimiento de Git): docs/automation/evidence/I-64-protocol-bridge/tools/verify_scope_bridge.py (blob cf381ab96a3d945a9569d56bb3aaf546e3f2fce1)
  - orden del Coordinator que abre A-9 (literal): docs/automation/evidence/I-64-pilot/F2-T1-NAVIGATION-CORE/coordinator/coordinator-a9-correction-order.md
  - disposicion del Coordinator que fija la semantica de coincidencia potencial (literal): docs/automation/evidence/I-64-pilot/F2-T1-NAVIGATION-CORE/coordinator/coordinator-a9-preversioning-disposition.md
  - revision anterior: docs/automation/evidence/I-64-protocol-bridge/architect-review/R20261109T062937Z-e1d0/output.json y review-record.json
  - orden que abrio A-8: docs/automation/evidence/I-64-pilot/F2-T1-NAVIGATION-CORE/coordinator/coordinator-f2t1-nc3-53af-a8-order.md; contexto de la ocurrencia: docs/automation/evidence/I-64-pilot/F2-T1-NAVIGATION-CORE-nc3/1/
    y la verificacion aceptada docs/automation/evidence/I-64-pilot/F2-T1-NAVIGATION-CORE/1/R20261108T221347Z-ebfc/; evidencia docs/automation/evidence/I-64-evidence.md
    Sec.76 a Sec.80; estado docs/automation/state/I-64.yml (f2_t1_status, f2_a8_status, f2_a9_status)
  - hecho de aplicabilidad (fuente del repositorio, no ejecucion del puente): git show c726410f3ac764f4a80a08a2b810b5fdda276ecc:
    tests/RackCad.Tests/ProjectVariableMutationPreflightTests.cs (metodo Create_Acu<U+00F1>aUnVariableIdNUEVO)
AUTORIDADES (leer en origin/main a140eb3a9f055518f0ffa5a3f927ff1c0e1c1e60 con git show a140eb3a:<ruta>):
  - docs/INITIATIVE_LIFECYCLE.md Sec.3 (M-01..M-08), Sec.5 (participacion del Architect) y Sec.6 (Freeze y enmiendas A-n)
  - docs/AUTOMATION_PLAN.md Sec.16 (16.8 conteo y RED, 16.9 comprobacion Tests, 16.10 terminos de gate, 16.11)
  - docs/automation/agent-execution/README.md Sec.8 y Sec.10 (controles negativos, nc3)
  - semantica de VSTest para el filtro FullyQualifiedName~<valor>: operador ~ = contiene, sin distinguir mayusculas (OrdinalIgnoreCase
    de .NET, que compara unidades de codigo UTF-16); para xUnit el FullyQualifiedName es el className del TestMethod del TRX + "." + su name
  - las ordenes literales del Coordinator citadas arriba

PRIMERO determina para cada hallazgo de e1d0 (campo PriorFindings) si A-9 lo cierra (CLOSED) o lo mantiene (RETAINED), con evidencia.
Un hallazgo RETAINED exige al menos un hallazgo REQUIRED en Findings.

RESPONDE EXPLICITAMENTE (campo Answers; Answer = YES o NO; Conforming = si esa respuesta deja la enmienda conforme):
 Q01 Is A-9 materiality correct: M-04 = YES and every other M flag = NO?
 Q02 Is A-9 limited to I-64 and to substitute protocol evidence, with no global AUTOMATION_PLAN authority and no change to product semantics?
 Q03 Does the verifier fail closed exactly when non-ASCII UTF-16 units could affect the ASCII filter result, while allowing a provable
     non-match without attempting Unicode case mapping?
 Q04 Does the TRX parser fail closed on every ambiguity, with Counters limited to an explicit allowlist, total/passed/failed/notExecuted
     derived from the top-level rows, executed = passed + failed, every other supported category zero and unknown attributes rejected?
 Q05 Is RequiredTests handled correctly (MinSelected; GREEN all selected pass; RED selected failures preserved), and are
     724/724/0 and 724/673/51 bindings for the later real occurrence rather than synthetic assumptions used to make the verifier pass?
 Q06 Are ChainRedFiles derived independently with hardened Git, required equal to the six accredited files, and protected from RED->GREEN changes?
 Q07 Is mutation exactness complete: both handoffs schema-valid, only the authorized WorkCompleted mutation, exactly one prohibited
     16.10 term introduced, no compensating change?
 Q08 Are the relationships correct: verbatim reuse of the accepted A-6 primitives with A-6 not executed, A-4 Git hardening reused,
     and A-9 a distinct amendment that corrects and supersedes A-8 for activation only while A-8 stays immutable?
 Q09 Does the synthetic adversarial coverage include the A-8 cases plus long s and dotless i potential matches, the unrelated U+00F1
     FQN, an ASCII match inside a non-ASCII FQN, the surrogate pair as two UTF-16 units, the counter cases and the executionId cases,
     and does the regression against the historical A-8 blob demonstrate all three findings?
 Q10 Does the pre-activation boundary hold: no real F2-T1 evidence processed by the bridge before Owner + Architect + Coordinator activation?
 Q11 Does any path exist where an unverifiable required input produces PASS, including a duplicated execution record? (Answer YES = existe una ruta abierta)
 Q12 Is any new Owner or I-62 authority required beyond the explicit Owner decision that A-9 already requests? (Answer YES = se requiere)

REGLAS:
  - Solo lectura: no escribas archivos, no hagas commit ni push; usa git show / git diff / git log / git rev-parse / git ls-tree
    / git cat-file y lectura de archivos.
  - NO ejecutes verify_testfilter_bridge.py contra ninguna entrada real de F2-T1 (TRX de CI, entregas del Worker o de nc3, ni nada
    bajo artifacts/): A-9 no esta activa. Si ejercitas el verificador o la autoprueba, hazlo SOLO con entradas sinteticas (la
    autoprueba genera las suyas en un directorio temporal); si el sandbox de solo lectura lo impide, declaralo en Limitations.
  - Leer el codigo fuente del repositorio (por ejemplo el archivo de pruebas citado) no es ejecutar el puente.
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
