# FX-06 — transiciones esperadas del bucle de revisión (SOLO SUPERVISIÓN)

```text
Autoridades: V14 §20.4-§20.6, §20.5.1, §20.5.2, §20.3.1-§20.3.3, B.8.8 (I-S18, I-P13), §9.1, §9.2; A-1 D1-1..D1-15, D2-12 (D2-1..D2-9 solo si hay
             un rebase); AUTOMATION_PLAN 16.29; README §18; secuencia F.8 de F4 (tests/RackCad.Tests/I62Fixtures/OrchestrationSamples.cs) como forma.
Uso:         la supervisión compara cada par de puntos de la corrida real con esta tabla y con el validador de producción (run_validator.ps1). Es una
             expectativa de forma; no fija el contenido de los resultados de B o C (eso es del oráculo, que esta línea no conoce).
Notación:    r0 = último punto previo a la ventana (el QR del titular de FX-06); r1, r2… = puntos sucesivos (+1 exacto, I-P01); todos QU ORDINARY
             (I-P02: QU → QU); ninguno toca window, last_window, chains ni contadores de ventana (I-P05).
```

## 1. Ruta nominal (D.8 pasos 1-8)

| # | Punto | `loop.phase` | Intento | Cambios en ese punto | Precondición en p | Cláusulas |
|---|---|---|---|---|---|---|
| T0 | commit de X v1 (no es punto) | NONE | — | archivo del objeto, solo | ninguna decisión del Coordinator pendiente; RLA custodiada antes | D.8-1; §20.5.1 `ObjectFamily` |
| T1 | r1 | NONE → REVIEW_PENDING | L1/1: → BUDGET_RESERVED (`reserved_at` = r1, `reserved_utc`) | `loop.type` ARCHITECT_REVIEW; `instance_id` = `ARL-r1`; `object` = X v1; `authorization`; `action_validity` OPEN; **una** entrada nueva de `architect_budgets[]` con la RLA en OPEN; L1 OPEN (`loop_instance_id` = `ARL-r1`, `round` 1); binding B, cierre, fidelidad e invocación custodiados; contadores de la entrada a 1/1/1, `transport_reruns`[L1] = 0; `next_action` ARCHITECT con `target` X v1, `invocation_permission` = la RLA y `expected_output` `rackcad-architect-review-result/v1` | `loop.type` NONE; topes no superados (P-18) | A-1 D1-1, D1-2, D1-3, D1-7; §20.6 (crecen en la reserva); I-S18 (REVIEW_PENDING ⇒ intento en INVOCATION_PLANNED o BUDGET_RESERVED); README §18.1 |
| T2 | r2 | REVIEW_PENDING → ARCHITECT_INVOKED | L1/1: BUDGET_RESERVED → LAUNCHING | `run_id`, `launching_utc` (≤ `Validity.Until`); invocación inmutable desde aquí | vigencia OPEN; fidelidad de preflight FAITHFUL o FAITHFUL_NORMALIZED | §20.6 (regla de lanzamiento: nadie lanza sin LAUNCHING durable); I-P13 (acción nueva con vigencia OPEN) |
| — | lanzamiento de B | — | — | `codex exec` de solo lectura; cesión del directorio de la receta (OQ-13: `arch` en la variante A, el worktree de la unidad en la variante L) | huella = OD-2d; en A, `arch` en la `AuthorityRevision`; en L, worktree limpio con `HEAD` = `origin/fx/u1` | 16.4 |
| T3 | r3 (opcional; **no existe en la variante L**) | ARCHITECT_INVOKED | LAUNCHING → LAUNCHED | `launch_evidence` (`relay-record/v2` con PID + `CreationDateUtc` y `thread_id`; OQ-11) | variante A: el QU se escribe fuera de `arch`. Variante L: la Cesión de 16.4 prohíbe escribir en el worktree durante la corrida, así que el intento pasa de LAUNCHING a RESULT_RECEIVED y la prueba del lanzamiento va por `runtime_evidence` | §20.6 («cuando se acredita»); I-P13 admite también LAUNCHING → RESULT_RECEIVED; 16.4 (Cesión); §20.3.2 (la identidad observada se custodia en el `relay-record/v2` y el estado la referencia como `RuntimeEvidenceRef`) |
| T4 | r4 | ARCHITECT_INVOKED | → RESULT_RECEIVED | `result`, `output_state` PRESENT, `runtime_evidence`, `read_audit`, `input_fidelity` (preflight + postrun), `fidelity_status` | terminación acreditada (op. 7); Entry de 16.4 sin P-01 | §20.6 (la custodia precede a la ingestión); §20.3.1-§20.3.3 |
| T5 | r5 | ARCHITECT_INVOKED → CORRECTING (la ingestión publica directamente la fase siguiente) | RESULT_RECEIVED → RESULT_INGESTED (`outcome` VALID, `ingested_at` = r5) | L1 INGESTED; un linaje por REQUIRED (`issuer` ARCHITECT, `severity` REQUIRED, `opened_in` = resultado de B); disposiciones; `next_action` PRINCIPAL_COORDINATOR / CORRECT_AND_REREVIEW sobre X v1; respuestas del Principal (D.8-4) | veredicto CHANGES REQUIRED VALID | §20.5 (CHANGES REQUIRED: ingerir, responder, corregir); §20.5.2; 16.29 |
| T6 | commit de X2 (no es punto), publicado solo | CORRECTING | — | solo el `CorrectionScope` | `review_rounds` < su tope (si no, P-18 sin corregir) | §20.5 (PUBLISHED: «ese commit se publica solo»); §20.6 («una corrección sin re-revisión posible no se publica») |
| T7 | r6 | CORRECTING → PUBLISHED | — | `loop.object` = X2; `correction_rounds` 0 → 1; `corrections_by_lineage`[LIN] +1 por linaje corregido; respuestas custodiadas | `corrections_by_lineage` < su tope | I-P13 (`loop.object` solo en CORRECTING → PUBLISHED; sube `correction_rounds`); A-1 D1-10 |
| T8 | r7 | PUBLISHED → CI_VERIFIED | — | evidencia de la CI `push` del commit de X2 con los dos jobs en `success` | corrida existente y concluida ([ci-checkpoints.md](ci-checkpoints.md)) | §20.5 (CI_VERIFIED); D.2; D.8-5 |
| T9 | r8 | CI_VERIFIED → REREVIEW_PENDING | L2/1: → BUDGET_RESERVED | binding C materializado (sin decisión del Coordinator); L2 OPEN sobre X2 (`round` 2); `OpenFindings` = linajes abiertos (D1-15); contadores 2/2/2, `transport_reruns`[L2] = 0; `next_action` ARCHITECT sobre X2 | vigencia OPEN; topes | D.8-6; §20.5.1; C-40 (a); A-1 D1-14 (`BudgetSnapshot` de la entrada) |
| T10 | r9 | REREVIEW_PENDING → ARCHITECT_INVOKED | L2/1: → LAUNCHING | `run_id`, `launching_utc` | `loop.object` sin cambio (I-P13: no cambia en REREVIEW_PENDING → ARCHITECT_INVOKED) | §20.6; C-38 (negativo del cambio tardío) |
| T11 | r10 (opcional; no existe en la variante L) | ARCHITECT_INVOKED | → LAUNCHED | `launch_evidence` de C (`thread_id` distinto del de B) | igual que T3 | §20.6; 16.4 |
| T12 | r11 | ARCHITECT_INVOKED | → RESULT_RECEIVED | igual que T4 para C | igual que T4 | — |
| T13 | r12 (punto del paso 7; `Step7RecordVersion`) | → ARCHITECT_SATISFIED | → RESULT_INGESTED (VALID) | L2 INGESTED; cada linaje de B CLOSED con `closed_by` = resultado de C y `last_disposition_in`; `action_validity` ENDED con ARCHITECT_SATISFIED (`ended_at` = r12) y el mismo fin en `authorizations[0]` de la entrada; `next_action` derivado (paso 8) | resultado AGREED sobre el blob de X2, FAITHFUL o FAITHFUL_NORMALIZED; ningún REQUIRED en OPEN o STILL_OPEN en la unidad | I-S18; §20.5.2; §20.5.1 (vigencia termina en ARCHITECT_SATISFIED); D.8-7, D.8-8 |
| T14 | r13 (opcional) | ARCHITECT_SATISFIED → NONE (LOOP_CLOSED) | — | `loop.*` a NONE/`null`; entrada con `closed_at` = r13 y `closed_by` = `null` | ninguna solicitud OPEN ni intento no terminal; escalada NONE | A-1 D1-9 (sin decisión desde ARCHITECT_SATISFIED) |

En todos los puntos: los contadores no decrecen (I-P13; A-1 D1-6); como máximo una solicitud OPEN y un intento no terminal (I-S18); todo intento en
LAUNCHING o posterior tiene `run_id` e `input_fidelity` con preflight fiel (I-S18); `escalation.state` = NONE ⇔ `next_action.role` ∉ {OWNER,
COORDINATOR}; `NextAction` única (P-17).

## 2. Ramas fuera de la ruta nominal

| Rama | Disparador | Transición | Efecto en FX-06 | Cláusulas |
|---|---|---|---|---|
| INV | resultado inválido (esquema, `Reviewed*` ≠ `Target`, sin declaración de contexto, REQUIRED incompleto, AGREED con un abierto omitido, `EvaluatedObject` no equivalente, `FindingId` desconocido) | intento → RESULT_INGESTED con `outcome` INVALID; la solicitud sigue OPEN; fase pendiente; intento k+1 reservado como reejecución (`transport_reruns` +1, `architect_launches` +1) | sigue dentro del tope; agotado → STOP (P-19) | §20.5 (P-19); §20.5.2; B.10.1 |
| CTX | lectura registrada fuera del cierre | `outcome` INVALID_REVIEW_CONTEXT; el intento cuenta; reejecución | ídem | §20.3.1 (P-22) |
| ID | identidad declarada concreta ≠ observada | `outcome` CONTRADICTION; sin ingestión; la orquestación se detiene hasta la decisión (S-04) | la decisión sería intermedia: no PASS | §20.3.2 (P-23) |
| FID-PRE | preflight de fidelidad no fiel | sin lanzamiento; sin consumo si no se reservó | STOP; UNVERIFIED (o UNSUPPORTED si se mide que no existe transporte fiel) | §20.3.3 (P-24) |
| FID-POST | DEGRADED_UNBOUNDED o UNVERIFIED | `outcome` INPUT_FIDELITY_INVALID; nada se ingiere; reejecución | dentro del tope | §20.3.3 (P-25) |
| FID-BND | DEGRADED_BOUNDED | cada premisa sin manifiesto B.11 en el fixture → UNKNOWN → INVALID_PREMISE: ningún linaje cambia; nunca ARCHITECT_SATISFIED | con B: sin linajes que corregir (resultado no utilizable; OQ-07); con C: no hay ARCHITECT_SATISFIED | §20.3.3; I-S18 |
| ABS | terminó sin salida (p. ej., tope de 600 s) | RESULT_RECEIVED con `output_state` ABSENT; ingestión `outcome` NO_OUTPUT; reejecución | dentro del tope | §20.6 caso B (3); 16.4 |
| B-AGREED | B devuelve AGREED | RESULT_INGESTED → ARCHITECT_SATISFIED sobre X v1 | **UNVERIFIED** (D.8: la ruta de corrección no se ejerce) | D.8 |
| BLK | B o C devuelven BLOCKED — OWNER DECISION | RESULT_INGESTED → `escalation` OWNER con la decisión exacta; sin corrección ni invocación | no enumerado en D.8 (OQ-10) | §20.5; C-33 |
| C-CR | C devuelve CHANGES REQUIRED | con `review_rounds` en su tope: STOP y escalada (P-18) sin corregir; si no, otra ronda | no enumerado en D.8 (OQ-10) | §20.5; §20.6 |
| P18 | un intento nuevo superaría un tope | rechazo **antes** de reservar; `escalation` | STOP | §20.6; 16.29 (P-18) |
| OUT | corrección fuera del `CorrectionScope` | COORDINATOR_DECISION | decisión intermedia: no PASS | §20.5 |
| NOCAND | ninguna candidata satisface la RLA (p. ej., celda no medida, huella cambiada) | sin invocación; STOP; COORDINATOR_DECISION u Owner (huella, autenticación) | no PASS | §20.5.1; 16.20 |
| P01 | huella de `config.toml` distinta de OD-2d en Salida o Entry | STOP del transporte `codex-cli`; ninguna invocación más hasta el Owner | no PASS en esta corrida | 16.4; adapter `codex-cli` (huella) |

## 3. Recuperación por estado (caída del Principal o del proceso)

| Caso | Último estado durable del intento | Evidencia del invocador (op. 7 de `codex-cli`, ligada al `RunId`) | Destino | Cláusulas |
|---|---|---|---|---|
| A | INVOCATION_PLANNED o BUDGET_RESERVED | — | se reanuda sin consumir otro lanzamiento; antes de LAUNCHING puede replanificarse (`InvocationId` nuevo) dentro de la misma reserva | §20.6 caso A |
| B1 | LAUNCHING | no hay ningún proceso del `RunId` (línea de órdenes con `-C <directorio de la receta, OQ-13>` y el directorio del `RunId`), ni salida `-o`, ni registro de sesión nuevo con ese `cwd` en el intervalo | con la vigencia abierta: → BUDGET_RESERVED con `not_started_evidence` custodiada; sin nuevo consumo | §20.6 caso B (1); B.8.8 `not_started_evidence` |
| B2 | LAUNCHING o LAUNCHED | proceso terminado y salida accesible | → RESULT_RECEIVED | §20.6 caso B (2) |
| B3 | LAUNCHING o LAUNCHED | terminado sin salida | → RESULT_RECEIVED con salida ABSENT | §20.6 caso B (3) |
| B4 | LAUNCHING o LAUNCHED | sigue vivo | esperar, u op. 6 (cancelar) dentro del tope de 600 s | §20.6 caso B (4); 16.4 |
| B5 | LAUNCHING o LAUNCHED | no se puede acreditar | **LAUNCH_UNCERTAIN**: cuenta como lanzado; el intento siguiente es una reejecución; un resultado tardío se registra como evidencia y no se ingiere | §20.6 caso B (5) |
| C | RESULT_RECEIVED | — | ingestión idempotente desde el resultado custodiado (clave: solicitud, intento, blob); otro blob para el mismo intento es S-04 | §20.6 caso C; I-P13 |
| D | RESULT_INGESTED | — | terminal; reingestión = no-op | §20.6 caso D |

**Quién recupera.** Reanudar la misma sesión del Principal no exige designación. Una sesión nueva del Principal en la ventana necesita una
designación del Coordinator (T12b, o T16 tras un QH): es una decisión intermedia y la corrida ya no puede ser PASS (OQ-19). En ambos casos los contadores,
la solicitud y los linajes continúan (I-P13; A-1 D1-6; C-36).

## 4. Fin de la vigencia con intentos en curso (`Validity.Until`, revocación o sustitución)

| Estado al fin (`ended_utc`) | Evidencia del invocador | Destino | Cláusulas |
|---|---|---|---|
| INVOCATION_PLANNED o BUDGET_RESERVED | — | CANCELLED_BEFORE_LAUNCH, sin liberar presupuesto | §20.5.1; §20.6 |
| LAUNCHING o LAUNCHED | arranque observado anterior al fin | se completa y se ingiere (acreditación histórica) | §20.5.1 |
| LAUNCHING | prueba de no arranque | CANCELLED_BEFORE_LAUNCH; la reserva sigue contada; sin lanzamiento nuevo | §20.5.1 |
| LAUNCHING | arranque indeterminado | LAUNCH_UNCERTAIN, sin reintento silencioso | §20.5.1 |
| LAUNCHING | arranque posterior al fin | UNAUTHORIZED_LAUNCH: no se ingiere; STOP (P-20) | §20.5.1; I-P13 |

Toda acción nueva tras el fin exige una autorización nueva (COORDINATOR_DECISION): en 1-7 sería una decisión intermedia.

## 5. Comprobaciones de terminación

| Momento | Comprobación | Fuente | Si falla |
|---|---|---|---|
| antes de cada LAUNCHING | ningún proceso de la lista cerrada con `-C <directorio de la receta, OQ-13>` vivo; huella = OD-2d; binario aceptado | 16.4 (Salida, Procesos) | STOP (P-02 o P-01) |
| tras cada corrida, antes de RESULT_RECEIVED | proceso lanzado y su árbol muertos (PID + `CreationDate`); Entry de 16.4; el directorio de la receta con el mismo HEAD y limpio | adapter `codex-cli` op. 7; §9.1 | sin terminación acreditada: caso B5 (LAUNCH_UNCERTAIN) |
| antes de actualizar `arch` para C (solo variante A) | B terminado y su salida custodiada (RESULT_RECEIVED durable) | §20.6; cesión de 16.4 | no se toca `arch` |
| tras el paso 8 (si la sesión termina) | `isRunning` = false observado por la supervisión, sin actividad posterior al último punto | §9.1 (TERMINATION_ACCREDITED de una sesión de Principal); adapter `claude-desktop-session` op. 7 | sin acreditación: NO_OBSERVATION (no autoriza tomar la custodia) |

## 6. Rebase durante el bucle (solo si `main` del fixture avanza)

No previsto: el `main` del fixture no avanza durante la corrida. Si avanzara, rigen A-1 D2-1..D2-12 (reconciliación de `loop.object.commit`, del objeto
de la solicitud OPEN y del `Target` de intentos no lanzados; LAUNCHING por ResolveBranchRef; equivalencia de objetos revisados), con un QU
REBASE_RECONCILIATION antes de cualquier LAUNCHING (I-H02). Para FX-06 sería una complicación no prevista por D.8: avisar al Coordinator antes de seguir.
