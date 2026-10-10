# I-62 — Handoff de suspensión controlada (decisiones §67)

> **I-62 está SUSPENDIDA (suspensión controlada).** No se reanuda implícitamente: ninguna sesión, ningún agente y ninguna automatización puede
> retomar trabajo de I-62 sin una **disposición expresa del Coordinator** que levante esta suspensión y nombre el paso. I-62 **no está completada**:
> sus controles, su Freeze y sus acuerdos A-1..A-4 se conservan intactos.

## 1. Rama y último commit

| Elemento | Valor |
|---|---|
| Rama | `architecture/portabilidad-coordinador-principal` (worktree `%USERPROFILE%\.codex\worktrees\architecture-portabilidad-coordinador-principal`) |
| Último commit de evidencia antes de este handoff | `0afee4d3a783f214b6f5c7f4b887864b6101048a` (decisiones §66, evidencia §123; CI success) |
| Commit de este handoff | el siguiente a ese (registro de §67, evidencia §124 y estado); su SHA lo da `git log` y el informe al Owner |
| Estado (`docs/automation/state/I-62.yml`) | `current_phase` F6; `state` waiting; `gate` owner-decision |
| `main` | no se toca; integración prohibida; I-61 vigente |

## 2. Gates completados

| Fase / acuerdo | Estado | Fuente |
|---|---|---|
| G0 | GATE PASS del Coordinator (`85ae4324`) | decisiones §7 |
| F0 | COMPLETE; Freeze de la Proposal V14 (`4c617e82`, FREEZE_SHA `b64a3b64`) | decisiones §30-§32 |
| F1 | GATE PASS sobre `12660ea7` | decisiones §33 |
| F2 | COMPLETE sobre `1eddbf48` | decisiones §35 |
| F3 | GATE PASS sobre `f9234cb9` | decisiones §35 |
| F4 | GATE PASS sobre `6f0187cb` (MC_I62) | decisiones §44 |
| A-1 | AGREED (blob `c01899a7`) | decisiones §43 |
| A-2 | AGREED (`f1e1d6f0`) | decisiones §57 |
| A-3 | AGREED (blob `ea6721f7`); no materializada (Q23) | decisiones §61 |
| A-4 | AGREED (blob `5e2ba68e`) | decisiones §64 |
| **F6** | **en curso; F6 GATE PASS pendiente** | decisiones §46-§67 |

## 3. Pilotos de F6 (evidencia histórica, sin cambios)

| Escenario / control | Estado | Fuente |
|---|---|---|
| FX-01 / C-23 | PASS | decisiones §51 |
| C-22 (D.1) | PASS, con la reserva de §66.4: el contrato de T1 se emitió con el mapa inválido (pide disposición si se reanuda) | MAP-INVALID/effects.md |
| FX-04a / C-25a | PASS (B3 23/23); su oráculo no modela `Evaluate` (confirmar como prueba de portabilidad si se reanuda) | decisiones §60 |
| FX-05 / C-27 | PASS | decisiones §46-§47 |
| **FX-02 / C-24** | **UNVERIFIED** | decisiones §66, §67 |
| FX-04b / C-25b | UNSUPPORTED (medido) | evidencia F6 |
| FX-03 / C-26 | UNVERIFIED; la causa escrita «OD-3 = RECHAZAR» está desfasada (OD-3 = A desde §56); reclasificación pendiente | I-62-F6/README.md |
| FX-06 / C-39 | pendiente (la opción A depende de FX-02; la opción B, una unidad aparte) | I-62-prep/f7/f7-status.md |

## 4. FX-02: dos bloqueos independientes (los dos hay que resolverlos para llegar a VERIFIED)

1. **Controller (bloqueo A).** La sonda única A4-1 dio NOT_DEMONSTRATED sobre la identidad congelada `3553cd6e…` / `26.1002.7124.0` /
   `73890CA3…` (R20261010T000542Z-a4-1-probe). La celda `codex-cli` `gpt-6-luna/high` es NOT_ELIGIBLE para la planificación y la verificación
   (A4-1, regla 4). Sin repetición. No existe alternativa autorizada.
2. **Mapa del fixture (bloqueo B).** MAP_INVALID: el seed del fixture se construyó con MC_I62 `6f0187cb` en lugar de la base I-61 `bb0d5522`. La opción
   **B1** (repositorio de fixture nuevo) está caracterizada y probada en clones temporales (MAP_VALID). No se crea nada.

## 5. Obligaciones pendientes

- F6: FX-02 / C-24 (los dos bloqueos), FX-03 (reclasificar), FX-06 / C-39, C-42 (7, 8), la deuda de C-43 (A-3, O4) y la disposición sobre FX-04b.
  F6 GATE PASS.
- F7, READY (READY-02..09 esperan a C-24), FOUNDATIONS definitivo, Candidato, OV, cierre e integración. F7 no puede cerrar antes que F6 (Q25).
- Preguntas abiertas para el Coordinator:
  - Q22': A-2 y A-4 excluyen D.5;
  - Q23: materializar A-3;
  - Q24: el defecto está en el método de D.1;
  - Q26: OD-1 y A-2..A-4;
  - Q11: el contrato y el estado no registran A-2..A-4;
  - disposiciones de C-22 y del oráculo de C-25a;
  - presupuesto de la sesión de Principal de una unidad nueva.
- N9 y N10: registrados (§67, punto 5); hay que resellarlos y acreditar su precondición antes de ejecutarlos.
- AUTHOR: evidencia del actor real de `d30fb6a9` conservada, con la discrepancia con B.2 sin reconciliar (reconciliación estrecha solo si el paso
  resulta necesario).

## 6. Presupuestos (D.3, con A-2 y A-4)

| Fila | Consumido | Disponible | Fuente |
|---|---|---|---|
| Sesiones de Principal, ronda A | A (P1) y R (P2): 2 | 1, solo para el titular A2 designado por T16 en FX-U1 (A4-6; `A4-PRINCIPAL-A2-CONSUMO` concedida y sin usar). No cubre la unidad nueva de B1 | u14-reconciliation.md; decisiones §63-§65 |
| Sesiones de Principal, FX-04a | B1, B2 y B3 (B3 por A2-P1) | sin reapertura | decisiones §58, §60 |
| `codex-cli`, Topología A (11 base, pool 4, tope 15) | 0 | 11 base y pool 4, pero el Controller no es elegible | evidencia §120 |
| Sondas previas de `codex-cli` (+2) | agotadas (y OD-2d-PROBE, por encima del tope, contada) | 0 | A-2 §3.2 |
| Bloque A2-P2 del par `3553cd6e…/26.1002.7124.0` | 2/2 | 0; solo una actualización observada abre otro bloque | decisiones §58.2 |
| Medición de A4-1 | 1/1 (NOT_DEMONSTRATED) | 0 bajo la misma identidad (A4-1, regla 4) | decisiones §66 |
| Worker de FX-02 (`claude-subagent`) | 0 | 1 + 1 | O4 |
| Architect de FX-02 por `claude-cli` (`A4-CLAUDE-FX02-CONSUMO`) | 0 | concedido, pero la celda no es elegible (operaciones 5, 7, 8 y 9 sin medir) | decisiones §64 |
| Medición de `claude-cli` (`CLAUDE-CLI-MEDICION-FX02`) | 0 | **no pedida** (paquete preparado; riesgo R1) | evidencia §123 |

## 7. Decisiones del Owner vigentes

- OD-2 = A sobre `73890CA3…` (OD-2f; decisiones §65), con OD-2-MAT = A: cada cambio de huella es P-01 y exige una OD-2 nueva.
- OD-3 = A (alcance del fixture; §56), OD-4 = A, OD-5 = A, OD-6 = alternativa 1, OD-7 = A público. OD-1 no se ha pedido (bloquea READY-03).
- CLAUDE-CLI-I62 = A (solo ARCHITECT y REVIEWER de I-62; nunca PRINCIPAL_COORDINATOR, WORKER ni escritura).
- `A4-CLAUDE-FX02-CONSUMO` = A y `A4-PRINCIPAL-A2-CONSUMO` = A (§64): concedidas y sin usar.
- `A4-SONDA-CONSUMO` = A del trío nuevo (§65): consumida. La del trío anterior no se usó y no vale.
- `B3-CONSUMO` y `BLOQUE-CODEX-CONSUMO` = A (§58): consumidas.

## 8. Próximas autoridades necesarias (ninguna se pide durante la suspensión)

| Para | Autoridad |
|---|---|
| Levantar la suspensión | disposición expresa del Coordinator que nombre el paso |
| Bloqueo B (B1) | disposición del Coordinator (opción, invariantes, texto del manifiesto, unidad, Claim-Id y fechas); el Owner crea el repositorio público y su CI; presupuesto de la sesión de Principal de la unidad nueva (línea del Owner y, si sube un tope congelado, una A-n) |
| Bloqueo A | **o bien** una actualización real de Codex (cambio de `BinaryHash` o `AppVersion`) observada pasivamente, seguida de la disposición del Coordinator que nombre el par, del consumo del Owner por bloque (A-2, A2-P2), de una OD-2 nueva sobre la huella resultante y, si hace falta, de otra medición de A4-1 (regla 3); la actualización sola no autoriza ningún consumo (§67, punto 3). **O bien** una A-n justificada que el Coordinator decida crear (no se crea automáticamente) |
| Architect por `claude-cli` | primero, una comprobación estática de la compatibilidad del esquema canónico con `--json-schema` y de la alternativa de transporte fiel (§67, punto 4); después, la disposición del Coordinator y `CLAUDE-CLI-MEDICION-FX02` del Owner |
| N9 / N10 | resellado de `negatives.md` y precondición real acreditada |

## 9. Rutas de artefactos

| Qué | Ruta |
|---|---|
| Decisiones y evidencia | `docs/automation/decisions/I-62.md` (§1-§67); `docs/automation/evidence/I-62-evidence.md` (§1-§124) |
| Estado | `docs/automation/state/I-62.yml` |
| Resultado de A4-1 | `docs/automation/evidence/I-62-F6/OD-2/R20261010T000542Z-a4-1-probe/` |
| Kit de la sonda (congelado) | `docs/automation/evidence/I-62-F6/kits/FX-02/a4-probe-v3/` |
| MAP_INVALID y B1 | `docs/automation/evidence/I-62-F6/MAP-INVALID/` (caracterización, plan, efectos, `mv_check.py`, candidato `candidate-b.bundle` `59b24493…`) |
| Opciones de recuperación | `docs/automation/evidence/I-62-F6/FX-02/s66/recovery-options.md` |
| `claude-cli` | `docs/automation/evidence/I-62-claude-cli/FX02-measurement-package/` (sin ejecutar; `MANIFEST.json` `e67bd195…`); análisis `I-62-F6/FX-02/s65/claude-cli-architect-ops.md` |
| AUTHOR | `docs/automation/evidence/I-62-F6/FX-02/author-ref/author-observation.json`; `FX-02/s64/author-F-matrix.md` |
| N9 / N10 | `docs/automation/evidence/I-62-F6/FX-02/s66/n9-n10-disposition-draft.md` |
| Orden O4 y clon A2 | fixture `fx/u1` `95bdc29d`; `FX-02/s65/order-O4-published.md`; clon `D:\r62-fixture\A2` (se conserva sin uso) |
| F7 | `docs/automation/evidence/I-62-prep/f7/` (r5: f7-status, independent-controls, measured/r5) |
| Kit de FX-02 | `docs/automation/evidence/I-62-F6/kits/FX-02/` |

## 10. Acción exacta de recuperación

1. Esperar una **disposición expresa del Coordinator** que levante la suspensión de §67 y nombre el paso. Sin ella, no se hace nada en I-62.
2. Al reanudar, la sesión principal lee este handoff, `docs/HANDOFF.md`, las decisiones §60-§67 y la evidencia §115-§124, y comprueba que la rama está en
   el commit de este handoff o en commits posteriores del Coordinator.
3. Primera acción técnica: una **medición pasiva** de la identidad de Codex (sin modelo), para saber si hubo una actualización real (bloqueo A, vía
   A1), y la comprobación de que la huella sigue en `73890CA3…`. Si cambió, P-01: OD-2 nueva antes de cualquier invocación.
4. Después, según lo que disponga el Coordinator: B1 (bloqueo B), la evaluación del Controller (bloqueo A) y la comprobación estática de `claude-cli`.
   Ninguna sonda, sesión, repositorio o enmienda sin su autoridad propia.

## 11. Prohibiciones durante la suspensión

- Ni sondas, ni sesiones de Principal, ni repositorios, ni enmiendas, ni revisiones de arquitectura nuevas.
- Ni reescribir refs del fixture o resultados históricos, ni repetir la sonda A4-1, ni convertir una capacidad no demostrada en PASS.
- Ni modificar I-64 desde este worktree. I-64 se reanuda en su rama y su sesión, por su alternativa autoritativa B (control sustituto conforme para el
  defecto nc2), y no se declara desbloqueada antes de obtener esa autoridad y verificar el sustituto.
