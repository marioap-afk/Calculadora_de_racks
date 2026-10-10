I-64 - REVISION FORMAL DEL ARCHITECT (ARCHITECTURE_REVIEW, effort semantico Deep) - A-4 / I64-SCOPE-BRIDGE-01
RunId: R20261010T031016Z-cca9. Modo: SEPARATE SESSION (sesion nueva del Codex CLI, solo lectura, modelo gpt-6.1-sol, effort high).
Independencia: el autor del objeto es la sesion principal de I-64 (Claude, otro proveedor); tu no participaste en su redaccion ni
en revisiones anteriores. Insumos: solo archivos versionados leidos desde Git (lista abajo); no hay memoria ni transcripcion del
autor. Contexto inyectado: este prompt.

OBJETO REVISADO (todo en el commit 0a61ee3a50ea4bb01a5dce0d791a77fa4b0b3446 de la rama architecture/workspace-persistente-rackcad;
lee con: git show 0a61ee3a50ea:<ruta>; registra cada blob con: git rev-parse 0a61ee3a50ea:<ruta>):
  - Freeze V5: git show 9b43dafbe3b8874aa0ba5a85d62cbf953dbdd311:docs/initiatives/I-64-proposal-v5.md (blob esperado dc1924ff...)
  - A-1 (historica, commit 22736034): docs/initiatives/I-64-A-1.md
  - A-4 (enmienda revisada): docs/initiatives/I-64-A-4.md
  - A-2 (corregida por la A-n revisada): docs/initiatives/I-64-A-2.md
  - verificador autorizado por A-4: docs/automation/evidence/I-64-protocol-bridge/tools/verify_scope_bridge.py (blob cf381ab9..., SHA-256 a3d4ed8a...);
    anteriores: blob 8972e374 (A-3) y blob 093a570f (A-2); leelos con git cat-file -p <blob completo de A-3/A-2>
  - autoprueba: docs/automation/evidence/I-64-protocol-bridge/tools/selftest_scope_bridge.py y docs/automation/evidence/I-64-protocol-bridge/selftest-result-a4.json (31/31 PASS); regresion contra el
    verificador de A-3: docs/automation/evidence/I-64-protocol-bridge/regression-demo-a4-a3-verifier.json (Result FAIL; T26-T31 daban PASS con codigo 0);
    autopruebas anteriores conservadas: docs/automation/evidence/I-64-protocol-bridge/selftest-result.json (23/23) y selftest-result-a3.json (25/25)
  - mandato de emergencia del Owner (literal): docs/automation/evidence/I-64-protocol-bridge/owner-mandate-emergency.md
  - ordenes del Coordinator (literales): docs/automation/evidence/I-64-protocol-bridge/coordinator-order-phase-a.md, coordinator-order-phase-b.md, coordinator-order-night-run.md
  - decision del Owner sobre P-01: docs/automation/evidence/I-64-protocol-bridge/owner-decision-p01.md
  - revision independiente de A-1 y sus 2 REQUIRED: docs/automation/evidence/I-64-protocol-bridge/review/R20261010T012851Z-de0d/conformance-review.json
  - revision formal anterior del Architect sobre A-2 (CHANGES REQUIRED, 1 REQUIRED I64-A2-RUNID-FAIL-OPEN):
    docs/automation/evidence/I-64-protocol-bridge/architect-review/R20261010T024843Z-f965/architect-review.json
  - re-revision anterior del Architect sobre A-3 (CHANGES REQUIRED, 1 REQUIRED I64-A3-JSON-CONSTANT-FAIL-OPEN):
    docs/automation/evidence/I-64-protocol-bridge/architect-review/R20261010T025754Z-77a6/architect-review.json
  - sonda de la celda: docs/automation/evidence/I-64-protocol-bridge/probe/R20261010T024641Z-ed2c/ (gpt-6.1-sol/high, PASS)
  - verificacion historica de T1: docs/automation/evidence/I-64-pilot/F1-T1-MODEL/R20261002T182925Z-5c9b/controller-verification.json
  - controles historicos: docs/automation/evidence/I-64-pilot/F1-T1-MODEL-nc1/R20261002T183529Z-b10b/,
    F1-T1-MODEL-nc2/R20261002T184050Z-8415/ (FAIL; incluye coordinator-decision.md, coordinator-protocol-stop-review.md,
    protocol-debt-handoff.md e inputs/delegation.json mutada), F1-T1-MODEL-nc3/R20261002T185053Z-8b6f/ y
    F1-T1-MODEL-nc4/R20261002T004303Z-8d3c/
  - evidencia de la unidad: docs/automation/evidence/I-64-evidence.md Sec.31 a Sec.37; estado docs/automation/state/I-64.yml

ESTO ES UNA RE-REVISION (LIFECYCLE Sec.5): version completa actual = A-4 (adopta A-3 y A-2 salvo el verificador); delta explicito =
  A-4 Sec.1-3 (blob nuevo del verificador: rechazo de NaN/Infinity/-Infinity y de surrogates sueltos; git diff con
  --ignore-submodules=none, --no-relative y --no-color; Git con core.fsmonitor=false y GIT_GRAFT_FILE inexistente; casos T26-T31
  con regresion demostrada). Disposicion previa: I64-A2-RUNID-FAIL-OPEN (CLOSED en 77a6) e I64-A3-JSON-CONSTANT-FAIL-OPEN corregido
  por A-4. Rellena PriorRequiredDisposition con CLOSED o STILL OPEN para ambos Id.
  El delta es foco minimo, no limite: un hallazgo material en una parte intacta es valido.
AUTORIDADES (leer en origin/main bb0d5522e8411f66a51fdfb3f1f0d0514b737453 con git show bb0d5522:<ruta>):
  - docs/INITIATIVE_LIFECYCLE.md Sec.3 (M-01..M-08), Sec.5 (participacion del Architect) y Sec.6 (Freeze y enmiendas A-n)
  - docs/AUTOMATION_PLAN.md Sec.16 (16.4 orden de commits, 16.8 topes, 16.9 comprobacion 7 Scope, 16.10)
  - docs/automation/agent-execution/README.md Sec.10 (controles negativos) y routing.md
  - docs/WORKFLOW.md y AGENTS.md (lo aplicable)
  - Para la pregunta 12: git diff --stat 819955d61a6da4c811a11fbd11b5dca13f634b7c bb0d5522 -- docs/INITIATIVE_LIFECYCLE.md
    docs/AUTOMATION_PLAN.md docs/automation/agent-execution docs/WORKFLOW.md AGENTS.md, y git log --oneline 819955d6..bb0d5522

RESPONDE EXPLICITAMENTE (campo Answers; Answer = YES o NO segun la pregunta; Conforming = si esa respuesta deja la enmienda conforme):
 Q01 Is M-04 = YES correct?
 Q02 Is the Owner authority (mandate) sufficient for this I-64-only protocol exception?
 Q03 Does the amendment preserve the exact Scope invariant of AUTOMATION_PLAN 16.9?
 Q04 Is the mechanical verifier at least as fail-closed for Scope as the old nc2 oracle?
 Q05 Can historical nc2 remain FAIL while substitute evidence supplies closure without contradiction?
 Q06 Is the exception genuinely limited to I-64?
 Q07 Is I-62 authority or implementation unnecessary?
 Q08 Are M-01, M-02, M-03, M-05, M-06, M-07 and M-08 correctly NO?
 Q09 Are REAL PASS + ADVERSARIAL FAIL (exclusively OUTSIDE_ALLOWED_WRITE_SCOPE, excluded file identified) sufficient?
 Q10 Is there any fail-open path? (Answer YES = existe una ruta abierta)
 Q11 Does the amendment improperly modify global I-61 semantics? (Answer YES = la modifica)
 Q12 Are there hidden authority/predecessor conflicts after main = bb0d5522? (Answer YES = existen)

REGLAS:
  - Solo lectura: no escribas archivos, no hagas commit ni push; usa git show / git diff / git log / git rev-parse / git ls-tree.
  - NO ejecutes verify_scope_bridge.py ni selftest_scope_bridge.py, y no decidas por razonamiento si una ruta concreta de T1
    esta dentro de su AllowedWriteScope: revisas la enmienda, la autoridad y el codigo; el puente no se ejecuta contra T1 todavia.
  - Si una orden auxiliar falla, repitela por otro medio o declara la limitacion en Limitations; no concluyas sin evidencia.
  - Severidad (LIFECYCLE Sec.5 y perfil ARCHITECTURE_REVIEW): REQUIRED si activa M-01..08 no declarado sobre un elemento
    congelable, deja un invariante sin obligacion de prueba, permite una ruta abierta ante fallos, contradice una autoridad o un
    hecho medido, o deja algo no ejecutable o no verificable; una precision sin cambio de significado es OPTIONAL.
  - Cada hallazgo: Question, ubicacion (archivo:linea), defecto, autoridad, correccion minima, y si exige cambiar la semantica
    global de I-61 (RequiresGlobalI61Change) o una decision nueva del Owner (RequiresOwnerDecision).
  - Verdict = AGREED si y solo si RequiredCount = 0. Si falta autoridad reservada del Owner: BLOCKED - OWNER DECISION.
    Toda respuesta con Conforming = false exige al menos un hallazgo REQUIRED con esa Question.
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
