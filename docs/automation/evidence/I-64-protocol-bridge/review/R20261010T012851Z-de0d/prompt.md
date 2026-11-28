I-64 - REVISION INDEPENDIENTE DE CONFORMIDAD (solo lectura) - A-1 / I64-SCOPE-BRIDGE-01
RunId: R20261010T012851Z-de0d. Modo: SEPARATE SESSION (sesion nueva del Codex CLI, solo lectura). Autor del objeto revisado: la sesion
principal de I-64 (Claude); tu no participaste en su redaccion. Insumos: solo los archivos versionados que se citan abajo,
leidos desde Git; no hay contexto privado del autor. Contexto inyectado: este prompt.

OBJETO REVISADO (commit exacto de la rama architecture/workspace-persistente-rackcad):
  commit 22736034dc177ddc181a0b513317cdb754f5bd18
  - docs/initiatives/I-64-A-1.md (enmienda A-1, propuesta)
  - docs/automation/evidence/I-64-protocol-bridge/tools/verify_scope_bridge.py (verificador)
  - docs/automation/evidence/I-64-protocol-bridge/tools/selftest_scope_bridge.py (autoprueba)
  - docs/automation/evidence/I-64-protocol-bridge/selftest-result.json (resultado de la autoprueba)
  Lee cada uno con: git show 22736034dc17:<ruta>, y registra su blob con: git rev-parse 22736034dc17:<ruta>.

AUTORIDADES (leer desde Git; no copiar su contenido):
  - docs/INITIATIVE_LIFECYCLE.md Sec.3 (materialidad M-01..M-08) y Sec.6 (enmiendas A-n)
  - docs/AUTOMATION_PLAN.md Sec.16.4 (orden de commits; paso 5) y Sec.16.9 (comprobacion 7 Scope)
  - docs/automation/agent-execution/README.md Sec.10 (controles negativos; nc2)
  - Freeze de I-64: git show 9b43dafbe3b8874aa0ba5a85d62cbf953dbdd311:docs/initiatives/I-64-proposal-v5.md (cabecera y Sec.13)
  - docs/automation/evidence/I-64-evidence.md Sec.31 y Sec.32; docs/automation/state/I-64.yml
  - docs/automation/evidence/I-64-pilot/F1-T1-MODEL-nc2/R20261002T184050Z-8415/ (coordinator-decision.md,
    coordinator-protocol-stop-review.md, protocol-debt-handoff.md, controller-verification.json, oracle-result.json)
  - Para comprobar que no cambia texto normativo: git diff --name-only 39b45f36383e6017da38e5bf91e08e485298a4fc 22736034dc17

REVISA SOLO ESTOS SEIS TEMAS (uno por campo de Topics):
  1. AuthorityAndMateriality: A-1 tiene el formato de enmienda de LIFECYCLE Sec.6 (Freeze identificado por commit/ruta/blob,
     Applies-to, clausula anterior, delta exacto, motivo, M-01..08, evidencia, append-only) y su autoridad Coordinator-only es
     suficiente porque M-01..M-08 = NO; contrasta cada M con su definicion.
  2. ScopeInvariantPreserved: el verificador conserva exactamente el invariante de Scope de AUTOMATION_PLAN 16.9 (comprobacion 7):
     toda ruta de git diff --name-only BaseSha..CurrentSha cubierta por AllowedWriteScope y sin interseccion con
     ForbiddenWriteScope, con cobertura = archivo exacto o prefijo terminado en "/". Cualquier diferencia debe ser solo mas estricta.
  3. FailOpenRoutes: existe alguna ruta por la que el verificador devuelva PASS o codigo de salida 0 sin que todas las
     comprobaciones hayan pasado (excepciones, argumentos, entorno de Git, objetos reemplazados, renombrados, rutas ambiguas,
     JSON duplicado o malformado, SHA invalido, diff vacio, repositorio anidado, salida no determinista, etc.).
  4. HistoricalNc2RemainsFail: A-1 conserva el nc2 historico R20261002T184050Z-8415 como FAIL y no altera su evidencia.
  5. NoI61OrI62NormativeChange: el commit no cambia texto normativo de I-61 (AUTOMATION_PLAN, README, routing, esquemas)
     ni de I-62, y A-1 no reinterpreta I-61: solo anade un requisito de evidencia de I-64.
  6. FutureUseStrictlyScoped: el uso futuro queda limitado a I-64 (Applies-to, blob fijado del verificador, condiciones 1-5,
     validez hasta una autoridad de protocolo integrada) y no puede extenderse a otras unidades ni debilitar otros controles.

REGLAS:
  - Solo lectura: no escribas archivos, no hagas commit ni push. Usa git show / git diff / git log / git rev-parse / git ls-tree.
  - NO ejecutes verify_scope_bridge.py ni selftest_scope_bridge.py, y no evalues por razonamiento si alguna ruta concreta de
    F1-T1-MODEL esta o no dentro de su AllowedWriteScope: revisas el diseno y el codigo, no ejecutas el control de Scope.
  - Si una orden auxiliar falla, repitela por otro medio o declara la limitacion en Limitations; no concluyas sin evidencia.
  - Severidad (perfil ARCHITECTURE_REVIEW): REQUIRED solo si contradice una autoridad o un hecho medido, deja algo congelado
    ambiguo, impide ejecutar un paso, deja una garantia sin verificar o permite una ruta abierta ante fallos; si no, OPTIONAL.
    Prefiere pocos hallazgos de alta confianza. Cada hallazgo: ubicacion (archivo:linea), defecto, autoridad y correccion minima.
  - Verdict = AGREED y Conformance = CONFORMING si y solo si RequiredCount = 0; RequiredCount = numero de hallazgos REQUIRED.
    Un tema NON_CONFORMING exige al menos un hallazgo REQUIRED de ese tema.
  - Prohibido declarar GATE PASS, Candidato, cierre, integracion o aprobacion del Owner.
  - Toda tu salida en ASCII (sin tildes ni escapes con barra invertida + u). Rellena ReviewSubject con los blobs que obtengas.

PERFIL ARCHITECTURE_REVIEW
Metodo: revision adversarial contra las autoridades citadas, sin escribir.
1. Lee el Freeze, los documentos duenos y el codigo afectado desde la revision fijada.
2. Cada hallazgo: ubicacion (archivo:linea), defecto, autoridad que contradice y correccion minima.
3. Severidad: REQUIRED solo si contradice una autoridad o un hecho medido, deja algo congelado ambiguo, impide ejecutar un
   paso o deja una garantia sin verificar.
4. No reabras lo ya resuelto; prefiere pocos hallazgos de alta confianza.

Informe esperado: la salida exacta del esquema rackcad-i64-conformance-review/v1 (la escribe el CLI por -o).
