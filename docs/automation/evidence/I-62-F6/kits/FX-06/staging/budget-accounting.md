# FX-06 — contabilidad de presupuestos por instancia de bucle (A-1) (SOLO SUPERVISIÓN)

```text
Autoridades: V14 §20.6 (constantes, crecimiento en la reserva, comprobación antes de reservar, sin reinicios), B.8.8 (I-S18, I-P13), D.8 (topes del
             escenario), D.3 (P-07 antes de cada lanzamiento), D.5 (OV compacto); A-1 D1-1, D1-2, D1-4, D1-5, D1-6, D1-7, D1-11, D1-14; AUTOMATION_PLAN
             16.28 (nombres Budget.*), 16.29 (tabla de topes congelados); F4: Orchestration.FrozenCaps y EffectiveCaps; decisiones §55 (U-01b,
             U-02(b), U-04, U-05; A-2 candidata, no aplicada).
Estado:      cálculo previo; ningún contador real.
```

## 1. Topes congelados y tope efectivo

| Tope (`architect_budgets[].caps`) | Congelado | Cuenta | Crece en |
|---|---|---|---|
| `review_rounds` | 3 | versiones distintas del objeto sometidas a revisión | BUDGET_RESERVED del primer intento de la primera solicitud sobre una versión |
| `logical_requests` | 3 | solicitudes lógicas | BUDGET_RESERVED del primer intento de cada solicitud |
| `transport_reruns_per_request` | 2 | intentos de una solicitud después del primero | BUDGET_RESERVED de cada intento k ≥ 2 |
| `corrections_per_lineage` | 2 | versiones publicadas que responden a un linaje | PUBLISHED |
| `correction_rounds` | 2 (= 3 − 1) | versiones corregidas publicadas | PUBLISHED |
| `architect_launches` | 9 (= 3 × (1 + 2)) | intentos reservados | BUDGET_RESERVED de cada intento |

**Tope efectivo** (A-1 D1-2, D1-11): mínimo componente a componente entre los congelados y el `Budget.*` de **cada** autorización de
`authorizations[]` de la entrada; un `Budget.*` ausente no rebaja (16.28). En FX-06 hay una sola autorización (sin sustitución ni continuación: cualquiera
de las dos exigiría una decisión del Coordinator, incompatible con el PASS de D.8).

**Topes del escenario (D.8, congelados):** Architect B (solicitud 1): 1 lanzamiento + 1 reejecución (tope 2); Architect C (solicitud 2): 1 + 1 (tope 2);
total de invocaciones de Architect: **4** (≤ 9); Principal A: 1 sesión + 1 reapertura (tope 2). D.5 (OV compacto): Architect 2 + reejecuciones 2.

**`Budget.*` compatibles con D.8** (propuesta para el Coordinator; los valores de la RLA los fija él y la plantilla los deja en blanco):

| Clave | Valor compatible | Motivo |
|---|---|---|
| `Budget.review_rounds` | 2 | dos versiones revisadas: X v1 y X2 |
| `Budget.logical_requests` | 2 | dos solicitudes: L1 (B) y L2 (C) |
| `Budget.transport_reruns_per_request` | 1 | «+1 (`transport_reruns` de su solicitud)» por solicitud |
| `Budget.architect_launches` | 4 | total de D.8; = 2 × (1 + 1) |
| `Budget.correction_rounds` | 1 | una corrección (X → X2); sin la línea, el tope efectivo es 2, aunque con `review_rounds` = 2 una segunda corrección nunca podría re-revisarse y no se publicaría (§20.6) |
| `Budget.corrections_per_lineage` | 1 | cada linaje de B se corrige una vez en X2 |

## 2. Ruta nominal (una entrada, `ARL-r1`)

Contadores de la entrada **después** de cada punto (`rr` = `review_rounds`, `lr` = `logical_requests`, `al` = `architect_launches`, `tr` =
`transport_reruns[]`, `cr` = `correction_rounds`, `cl` = `corrections_by_lineage[]`):

| Punto | Evento | rr | lr | al | tr | cr | cl |
|---|---|---|---|---|---|---|---|
| r1 | apertura + L1/1 BUDGET_RESERVED | 1 | 1 | 1 | L1:0 | 0 | — |
| r2-r5 | LAUNCHING, (LAUNCHED), RESULT_RECEIVED, RESULT_INGESTED de L1/1 | 1 | 1 | 1 | L1:0 | 0 | — |
| r6 | PUBLISHED (X2) | 1 | 1 | 1 | L1:0 | 1 | LIN-k:1 por linaje corregido |
| r7 | CI_VERIFIED | 1 | 1 | 1 | L1:0 | 1 | igual |
| r8 | REREVIEW_PENDING + L2/1 BUDGET_RESERVED | 2 | 2 | 2 | L1:0, L2:0 | 1 | igual |
| r9-r12 | LAUNCHING, (LAUNCHED), RESULT_RECEIVED, RESULT_INGESTED de L2/1 → ARCHITECT_SATISFIED | 2 | 2 | 2 | L1:0, L2:0 | 1 | igual |
| r13 | LOOP_CLOSED (opcional) | 2 | 2 | 2 | igual | 1 | igual; `closed_at` = r13 |

`BudgetSnapshot` (A-1 D1-14; `role-invocation/v1` campos `ReviewRounds`, `LogicalRequests`, `ArchitectLaunches`, `TransportReruns`, `CorrectionRounds`,
`CorrectionsByLineage`) = los contadores de la entrada en el QU de la reserva, con la propia reserva ya contada (forma de la secuencia F.8 de F4): L1/1 →
1/1/1/0/0/—; L2/1 → 2/2/2/0/1/{LIN-k:1}. Una invocación replanificada dentro de la misma reserva conserva su `BudgetSnapshot`.

`orchestration.budgets` (el de V14) **no** cambia: solo cuenta solicitudes con `loop_instance_id` = `null` (REVIEWER; A-1 D1-4).

## 3. Variantes

| Variante | Puntos afectados | Cambio de contadores | Comprobación antes de reservar (topes de §1, columna «compatible») |
|---|---|---|---|
| V1: resultado de B inválido (P-19, P-22 o P-25) → L1/2 | reserva de L1/2 | al +1 (2), tr[L1] +1 (1); rr y lr sin cambio | tr[L1] + 1 ≤ 1 ✓; al + 1 ≤ 4 ✓ |
| V2: L1/2 también inválido | — | — | tr[L1] + 1 = 2 > 1 → rechazo **antes de reservar** (P-18, §20.6); en la rama INVALID de §20.5, «agotado → STOP (P-19)»; escalada |
| V3: L1/1 en LAUNCH_UNCERTAIN | ninguno nuevo en el intento incierto (ya contado en su reserva) | el siguiente intento es una reejecución: como V1 | igual que V1 |
| V4: L1/1 CANCELLED_BEFORE_LAUNCH (vigencia terminada) | — | la reserva sigue contada; nunca se libera | sin acción nueva bajo la autorización terminada |
| V5: B y C con una reejecución cada una | reservas de L1/2 y L2/2 | al = 4, tr = {L1:1, L2:1} | al = 4 ≤ 4 ✓ (tope de D.8 agotado exactamente) |
| V6: C devuelve CHANGES REQUIRED | — | — | rr = 2 = tope → **no** hay re-revisión: STOP y escalada (P-18) sin corregir (§20.5); `cr` no sube |
| V7: tercera solicitud | — | — | lr + 1 = 3 > 2 → P-18 |
| V8: segunda corrección de un mismo linaje | — | — | cl[LIN] + 1 = 2 > 1 → P-18; además rr no permitiría re-revisarla |
| V9: cambio de proveedor, modelo, sesión, binding, Principal, etiqueta o autorización | cualquiera | ninguno: no se reinicia nada | A-1 D1-6; I-P13; C-36 |

## 4. Invariantes que la supervisión comprueba en cada punto (además del validador)

- `rr` ≤ `lr`; `al` = número de intentos con `reserved_at` ≠ `null` de las solicitudes con `loop_instance_id` = `ARL-r1`; `tr(r)` = intentos reservados de
  `r` − 1; ningún contador supera `caps`; `caps` = el mínimo de §1 (A-1 D1-5; I-S18).
- Los contadores no decrecen; la entrada no desaparece; tras `closed_at`, la entrada no cambia nunca (A-1 D1-6).
- Exactamente una entrada aparece, en el par de apertura (A-1 D1-7); con `loop.type` ARCHITECT_REVIEW, su último registro de `authorizations[]` coincide
  con `loop.action_validity` (A-1 D1-5).
- `next_action.budget_remaining` = `caps` − contadores, por contador (§20.4).
- Antes de cada lanzamiento, el total de invocaciones de Architect de la corrida (incluidos los inciertos) ≤ 4 (D.3, P-07; D.8).

## 5. Presupuestos fuera del bucle

| Concepto | Tope | Autoridad |
|---|---|---|
| sondas previas de `codex-cli` para el binario vigente (re-medición de la celda, P1) | **tope congelado AGOTADO** (OQ-21 DECIDIDA, decisiones §55, U-04): V14 D.3 (topología A) fija «Sondas previas (`codex-cli`, binario actual)»: 1 + 1, tope 2, «fuera de la ronda; con OD-2», y el total de la ronda A en «≤ 15 + 2 sondas»; ya se usaron 4 (2 de OD-2b-PROBE, decisiones §47; 2 de OD-2d-PROBE, decisiones §51). Ninguna sonda de modelo más por ahora; las autorizaciones previas del Owner no amplían el tope congelado. Solo si A-2 queda AGREED: un bloque **nuevo** de como máximo 2 sondas `read-only` para el BinaryHash/AppVersion exactos (`97c57e4e…`, `26.1002.6548.0`), con aceptación del Owner de la huella resultante bajo OD-2; ningún contador se reinicia. La sonda del Architect en `D:\r62-fixture\arch` de ese bloque es la invocación medida de la celda (U-02(b)): no hay segunda medición mientras sigan iguales los invalidadores; si cambia uno, la observación caduca y una medición nueva necesitaría un tope que hoy no existe | V14 D.3 (fila «Sondas previas» y «Totales por ronda de F6»); decisiones §47, §51; decisiones §55 (U-02(b), U-04; A-2 candidata, Problema 2) |
| medición de las formas de lectura por `codex sandbox` con el binario vigente (P1b) | sin modelo; **no cubierta** por OD-2d-PROBE y **no se ejecuta ahora** (OQ-22 DECIDIDA, decisiones §55, U-05 y §13). Si sigue siendo necesaria tras A-2, su consumo se enumera en [p1b-owner-packet.md](p1b-owner-packet.md) §6; contra qué tope cuenta, si cuenta contra alguno: OQ-27 | decisiones §51 (alcance de OD-2d-PROBE), §54 («operaciones sin modelo ya autorizadas»); decisiones §55 (U-05) |
| sesiones de B de FX-04a (para el contexto de OQ-26) | Principal B: 1 base + 1 reapertura = 2, consumidas por B1 y B2; el hueco de B2 de N11 está acotado a N11, no es fungible, y N11 es NOT_APPLICABLE; B3 NO AUTORIZADA con el Freeze vigente; si A-2 queda AGREED, permitiría exactamente una B3 (Problema 1: como máximo una reejecución limpia extraordinaria por escenario tras una acreditación INVALID_LAUNCH y una disposición del Coordinator). Ninguna sesión de FX-06 cuenta aquí | V14 D.3 (tabla de FX-04a: Principal B, 1 sesión, +1 reapertura, tope 2; N11: B2, 1 sesión, 0, tope 1); decisiones §55 (U-01b; A-2 candidata, Problema 1) |
| sesiones del Principal de FX-06 | 2 (1 + 1 reapertura); cuentan solo las aperturas de la sesión del Principal en `D:\r62-fixture\A6` (`Window.PrincipalSession` del auditor); abrir otra sesión no cuenta aquí, y dentro de 1-7 exige disposición ([message-bus-auditor.md](message-bus-auditor.md) §3) | D.8 |
| `attempts`, `correction_launches`, `invocations[]` (§9.3) | sin cambio: el bucle del Architect no abre ventanas ni lanza correcciones de tarea | §9.3; 16.27; A-1 («Sin cambio: `attempts` y los contadores de §9.3») |
