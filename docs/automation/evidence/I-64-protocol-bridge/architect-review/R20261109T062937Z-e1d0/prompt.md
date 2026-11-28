I-64 - REVISION FORMAL DEL ARCHITECT (ARCHITECTURE_REVIEW, effort semantico Deep) - A-8 / I64-TESTFILTER-BRIDGE-01
RunId: R20261109T062937Z-e1d0. Modo: SEPARATE SESSION (sesion nueva del Codex CLI 0.167.0-alpha.3.2, solo lectura, modelo gpt-6.1-sol, effort high;
celda acreditada por la sonda R20261109T062522Z-e8be).
Independencia: el autor del objeto es la sesion principal de I-64 (Claude, otro proveedor); tu no participaste en su redaccion.
Insumos: solo los archivos listados (versionados en Git); no hay memoria ni transcripcion del autor. Contexto inyectado: este prompt.

OBJETO REVISADO (commit exacto d4a97950c2757d09f971a66621e7262e385ad818 de la rama architecture/workspace-persistente-rackcad;
lee con: git show d4a97950c275:<ruta>; registra cada blob con: git rev-parse d4a97950c275:<ruta>):
  - Freeze V5: git show 9b43dafbe3b8874aa0ba5a85d62cbf953dbdd311:docs/initiatives/I-64-proposal-v5.md (blob esperado dc1924ff...)
  - A-8 (enmienda revisada, PROPOSED / NOT ACTIVE): docs/initiatives/I-64-A-8.md
  - verificador: docs/automation/evidence/I-64-protocol-bridge/tools/verify_testfilter_bridge.py (blob 1691003d..., SHA-256 4bccdc9f...)
  - autoprueba sintetica: docs/automation/evidence/I-64-protocol-bridge/tools/selftest_testfilter_bridge.py y su resultado docs/automation/evidence/I-64-protocol-bridge/selftest-testfilter-result-a8.json (35/35 PASS)
  - demostracion de regresion: docs/automation/evidence/I-64-protocol-bridge/tools/regression_demo_testfilter_bridge.py y docs/automation/evidence/I-64-protocol-bridge/regression-demo-a8-testfilter-mutants.json
  - verificador de A-6 (fuente de las primitivas copiadas literalmente; A-8 afirma que no lo ejecuta): docs/automation/evidence/I-64-protocol-bridge/tools/verify_freetext_bridge.py
    (blob 3bc7869e9f4c9c5eed0d64d7b8fee6f7002b9169); compara el bloque marcado en el verificador de A-8 con ese blob
  - verificador de A-4 (fuente del endurecimiento de Git): docs/automation/evidence/I-64-protocol-bridge/tools/verify_scope_bridge.py (blob cf381ab96a3d945a9569d56bb3aaf546e3f2fce1)
  - orden del Coordinator que abre A-8 (literal): docs/automation/evidence/I-64-pilot/F2-T1-NAVIGATION-CORE/coordinator/coordinator-f2t1-nc3-53af-a8-order.md
  - contexto de la ocurrencia (custodiado): docs/automation/evidence/I-64-pilot/F2-T1-NAVIGATION-CORE-nc3/1/ (46c1, 041d, 53af: output.json, coordinator-classification.json,
    custody-manifest.json); verificacion aceptada docs/automation/evidence/I-64-pilot/F2-T1-NAVIGATION-CORE/1/R20261108T221347Z-ebfc/; evidencia
    docs/automation/evidence/I-64-evidence.md Sec.76 a Sec.78; estado docs/automation/state/I-64.yml (f2_t1_status, f2_a8_status)
  - precedentes de puentes sustitutos de I-64: docs/initiatives/I-64-A-2.md (mecanismo de evidencia sustituta y condiciones 1-10),
    docs/initiatives/I-64-A-4.md y docs/initiatives/I-64-A-6.md
AUTORIDADES (leer en origin/main a140eb3a9f055518f0ffa5a3f927ff1c0e1c1e60 con git show a140eb3a:<ruta>):
  - docs/INITIATIVE_LIFECYCLE.md Sec.3 (M-01..M-08), Sec.5 (participacion del Architect) y Sec.6 (Freeze y enmiendas A-n)
  - docs/AUTOMATION_PLAN.md Sec.16 (16.8 conteo y RED, 16.9 comprobacion Tests, 16.10 terminos de gate, 16.11)
  - docs/automation/agent-execution/README.md Sec.8 y Sec.10 (controles negativos, nc3)
  - semantica de VSTest para el filtro FullyQualifiedName~<valor>: operador ~ = contiene, sin distinguir mayusculas; para xUnit el
    FullyQualifiedName es Namespace.Clase.Metodo (el className del TestMethod del TRX + "." + su name)

RESPONDE EXPLICITAMENTE (campo Answers; Answer = YES o NO; Conforming = si esa respuesta deja la enmienda conforme):
 Q01 Is A-8 materiality correct: M-04 = YES and every other M flag = NO?
 Q02 Is A-8 limited to I-64 and to substitute protocol evidence, with no global AUTOMATION_PLAN authority and no change to product semantics?
 Q03 Does the verifier decide selection only by FullyQualifiedName~X = case-insensitive substring over className + "." + method name?
 Q04 Does the TRX parser fail closed on every ambiguity, with counters, definitions and results required to be coherent?
 Q05 Is RequiredTests handled correctly (MinSelected; GREEN all selected pass; RED selected failures preserved), and are
     724/724/0 and 724/673/51 bindings for the later real occurrence rather than synthetic assumptions used to make the verifier pass?
 Q06 Are ChainRedFiles derived independently with hardened Git, required equal to the six accredited files, and protected from RED->GREEN changes?
 Q07 Is mutation exactness complete: both handoffs schema-valid, only the authorized WorkCompleted mutation, exactly one prohibited
     16.10 term introduced, no compensating change?
 Q08 Is the relationship with A-6 correct: verbatim reuse of accepted parsing/term semantics, A-6 not executed, A-8 a distinct bridge?
 Q09 Does the synthetic adversarial coverage include display-name, class-only, case-sensitive and method-only counting, the Theory
     argument false positive, the method-only location true positive, malformed/unreadable inputs, ChainRedFiles failures and an
     unauthorized additional mutation?
 Q10 Does the pre-activation boundary hold: no real F2-T1 evidence processed by the bridge before Owner + Architect + Coordinator activation?
 Q11 Does any path exist where an unverifiable required input produces PASS? (Answer YES = existe una ruta abierta)
 Q12 Is any new Owner or I-62 authority required beyond the explicit Owner decision that A-8 already requests? (Answer YES = se requiere)

REGLAS:
  - Solo lectura: no escribas archivos, no hagas commit ni push; usa git show / git diff / git log / git rev-parse / git ls-tree
    / git cat-file y lectura de archivos.
  - NO ejecutes verify_testfilter_bridge.py contra ninguna entrada real de F2-T1 (TRX de CI, entregas del Worker o de nc3, ni nada
    bajo artifacts/): A-8 no esta activa. Si ejercitas el verificador o la autoprueba, hazlo SOLO con entradas sinteticas (la
    autoprueba genera las suyas en un directorio temporal); si el sandbox de solo lectura lo impide, declaralo en Limitations.
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
    (sin tildes ni escapes con barra invertida + u). Rellena ReviewSubject con los blobs que obtengas.

PERFIL ARCHITECTURE_REVIEW
Metodo: revision adversarial contra las autoridades citadas, sin escribir.
1. Lee el Freeze, los documentos duenos y el codigo afectado desde la revision fijada.
2. Cada hallazgo: ubicacion (archivo:linea), defecto, autoridad que contradice y correccion minima.
3. Severidad: REQUIRED solo si contradice una autoridad o un hecho medido, deja algo congelado ambiguo, impide ejecutar un
   paso o deja una garantia sin verificar.
4. No reabras lo ya resuelto; prefiere pocos hallazgos de alta confianza.

Informe esperado: la salida exacta del esquema rackcad-i64-architect-review/v1 (la escribe el CLI por -o).
