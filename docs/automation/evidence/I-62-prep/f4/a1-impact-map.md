# I-62 — Mapa de impacto de A-1 sobre F4 (dos variantes; preparación)

> **Preparación, no materialización.** A-1 es una **propuesta** (`docs/initiatives/I-62-A-1.md`, blob `09ca9328`) con los veredictos del Architect y del
> Coordinator PENDING. Este mapa no elige otra solución semántica. Fija, para cada pieza de F4, qué se implementa con el **Freeze literal** (variante A) y
> qué con **Freeze + A-1** (variante B), para que F4 empiece con la variante que resulte en cuanto haya veredicto. Los prototipos de `f4/oracle/` y
> `f4/rebase-proto/` implementan ya las dos variantes (`variant="LITERAL"` / `"A1"`).

## 1. Esquema `rackcad-automation-state/v2` (B.8.8)

| Pieza | Variante A (Freeze) | Variante B (Freeze + A-1) | Prototipo |
|---|---|---|---|
| `orchestration.budgets` | objeto único por unidad | `budgets[]` con `authorization_id`, append-only, único por autorización (D1-1) | `state_oracle.budget_entries` |
| `orchestration.review_requests[].authorization_id` | no existe | obligatorio e inmutable (D1-2) | `loop_open` en `test_oracle.py` |
| `orchestration.loop.object` | `null` solo sin bucle; nunca vuelve a `null` | también `null` en LOOP_CLOSED; imagen en REBASE_RECONCILIATION (D1-7) | `validate_orchestration_pair` |
| `orchestration.loop.phase` | enum de §20.5 | igual; LOOP_CLOSED es una transición hacia NONE, no un valor nuevo (D1-5) | `LOOP_EDGES` + `closing` |
| `RebaseMap.StateFields[]` (relay-record/v2, campo libre) | lista literal de B.8.7 | + `orchestration.loop.object.commit`, objetos de solicitudes OPEN, `Target` de intentos no lanzados (D2-1) | `rebase_proto.reconcile` |
| esquema JSON de `state/v2` (`AE/schemas/automation-state.v2.schema.json`) | `budgets` como objeto | `budgets` como array de objetos con `authorization_id`; `review_requests[].authorization_id` requerido | — (F4) |

## 2. Validador (`StateV2Validator`, guardas C-18 y C-38)

| Regla | Variante A | Variante B | Prototipo |
|---|---|---|---|
| I-S18, presupuestos | igualdades sobre todas las solicitudes; topes de unidad | por entrada `e`: igualdades sobre las solicitudes de `e.authorization_id`; topes por entrada; toda solicitud con su entrada; la autorización activa tiene entrada (D1-3) | `validate_orchestration_file` |
| I-S18, objeto | `type` = NONE ⇔ `phase` = NONE; con bucle, `object` ≠ `null` | igual + `type` = NONE ⇒ `object` = `null` | ídem |
| I-P13, contadores | no decrecen; no se reinician | por entrada; ninguna desaparece; solo cambia la de la autorización activa; una entrada nueva solo en NONE → REVIEW_PENDING con autorización nueva (D1-4) | `A1-D1-1`, `A1-D1-4` |
| I-P13, `loop.object` | solo CORRECTING → PUBLISHED y NONE → REVIEW_PENDING | + LOOP_CLOSED (→ `null`) y REBASE_RECONCILIATION (imagen, mismo `path` y `blob`) (D1-6, D2-4) | `validate_orchestration_pair` |
| transición LOOP_CLOSED | no existe: ARCHITECT_SATISFIED → NONE es inválida | QU ORDINARY desde ARCHITECT_SATISFIED, ESCALATE_OWNER resuelta o vigencia EXHAUSTED; sin solicitud OPEN ni intento no terminal; vigencia ENDED; bucle en blanco; presupuestos, solicitudes y linajes iguales (D1-5) | `A1-D1-5` |
| I-P13, vigencia | sustitución exige SUPERSEDED | igual con el bucle abierto; tras LOOP_CLOSED, autorización nueva = bucle nuevo (D1-9) | — |
| I-P13, intentos | lista de §20.6 | + INVOCATION_PLANNED → INVOCATION_PLANNED (replanificado) solo en REBASE_RECONCILIATION (D2-6) | `replan_planned` |
| I-P05 (excepción de reconciliación) | solo los SHA literales de `StateFields` | + los de la orquestación y la replanificación de §8.8 (D2-7) | `validate_pair` (rebase) |
| I-P10 | imágenes de los campos literales | + fase, presupuestos, linajes, estados y `reserved_at` iguales; `path` y `blob` iguales (D2-8) | `I-P10` en `validate_orchestration_pair` |
| I-H02 (con historia, C-15) | SHAs literales | + los commits vivos de la orquestación (D2-3) | `validate_history(…, "A1")` |
| SM-05 (no material, ambas variantes) | ARCHITECT_INVOKED → (RE)REVIEW_PENDING en la recuperación (B.1, LAUNCH_UNCERTAIN) | igual | `LOOP_EDGES` |

## 3. Procedimientos (README de agent-execution, secciones nuevas de F4) y §8.8

| Pieza | Variante A | Variante B |
|---|---|---|
| tabla del QU de reconciliación (§8.8) | filas literales | + filas de D2-2 (objeto del bucle, solicitudes OPEN, intentos no lanzados replanificados, lanzados con `Target` histórico, terminales sin cambio) |
| procedimiento del bucle | un bucle por unidad, de hecho: el segundo es inalcanzable o nace agotado (FC-01) | LOOP_CLOSED y apertura con autorización nueva; READY-06 como bucle posterior |
| reconstrucción por un sucesor (C-29) | el `Target` vivo puede apuntar a un commit que un clon limpio no tiene (FC-02, MEASURED con `rebase-proto`) | el sucesor reconstruye la imagen; el `Target` histórico se acredita por `blob` |

## 4. Pruebas: delta exacto de esperados

| Obligación | Variante A | Variante B |
|---|---|---|
| C-15 (rebase) | F.6/F.7 sin bucle activo; con bucle activo, el QU no reconcilia la orquestación y el validador literal lo acepta (registrar como limitación conocida, no como PASS de FC-02) | + bucle activo: QU reconcilia y replanifica; Q0/LAUNCHING antes del QU → I-H02; LAUNCHED conserva `Target`; imagen no acreditable → STOP |
| C-29 | caídas de F.8 sin rebase | + (g) reconstrucción a través de un QU REBASE_RECONCILIATION; (h) tras LOOP_CLOSED y bucle nuevo |
| C-31 | positivo completo de F.8 | + continuación LOOP_CLOSED → bucle nuevo con RLA-2 |
| C-34 | (a)-(e) sobre presupuestos de unidad | (a)-(e) por autorización + (f) bucle nuevo no es P-18; (g) reapertura con la misma autorización → rechazo; (h) sin reinicio de lo consumido |
| C-36 | contadores de unidad | + a través de un rebase y de un bucle a otro: ninguna entrada se reinicia |
| C-38 | negativos del Anexo C | + `loop.object` → `null` fuera de LOOP_CLOSED; LOOP_CLOSED con OPEN, con no terminal o sin vigencia ENDED; autorización reutilizada (con y sin entrada duplicada); entrada cerrada cambiada o borrada; solicitud sin `authorization_id`; reconciliación que cambia `path`, `blob`, fase, contadores o linajes; `Target` de un lanzado reescrito; replanificación sin `InvocationId` nuevo |

Los esperados de la variante B son los de `docs/initiatives/I-62-A-1.md` §5. Las 15 trazas de `I-62-A1/a1-counterexamples.py` y las transiciones A-1 de
`f4/oracle/test_oracle.py` son su forma ejecutable.

## 5. Contratos de F3 (sin cambio en ninguna variante)

`role-invocation/v1`: `Target` y `BudgetSnapshot` sin cambio de esquema (en B, el `BudgetSnapshot` se lee como la entrada de la autorización de la invocación, que
ya lleva `Authorization`). `binding/v1`, `AuthorizationRef` e `input-closure/v1`/`input-fidelity/v1`: sin cambio. `relay-record/v2`: `StateFields[].Field` es
texto libre, así que acepta los campos nuevos sin cambiar el esquema.
