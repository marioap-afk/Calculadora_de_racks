# FX-04a — comparison-contract-v2 (capa de comparación del fixture; decisiones §53)

No cambia ningún esquema canónico I62 ni la semántica del protocolo. Sustituye, solo para la corrida B2, el contrato de comparación de B1, que se
conserva intacto como evidencia histórica (oráculo `7fd1ef39…`, respuesta `e17265ea…`, comparación 19/22 FAIL bruto, acreditación
INVALID_TEST_ORACLE). Punto de partida canónico: QH2 `cabed54738f56e846bd77be321cace10d053094d`. Esquema de respuesta:
[response.v2.schema.json](response.v2.schema.json); herramienta: `fx04a_real.py oracle2` / `compare2` (los comandos de v1 no cambian).

## Reglas de comparación

- Igualdad exacta campo a campo de los 13 hechos y las 10 partes de la decisión (23 campos).
- Solo dos campos se comparan como conjunto (orden y repetición irrelevantes): `facts.stops_in_force` y `decision.preconditions`. `decision.next_points` es
  una secuencia y se compara en orden.
- Ninguna normalización semántica después del resultado. Un campo ausente es una diferencia.

## Campos que cambian respecto de v1 (los únicos)

| Campo | Representación de v1 | Por qué era defectuosa | Representación canónica de v2 | Fuente canónica |
|---|---|---|---|---|
| `facts.last_window` | tupla `[closure, verified_sha]`, `[null, null]` sin ventana | el contrato no definía la representación; la ausencia admitía `[null, null]` o `[]` | `{"present": false}` sin ventana; con ventana, exactamente `{"present": true, "seq", "task_id", "attempt", "closure", "verified_sha"}` | `custody.last_window` (`state/v2`; null en QH2) |
| `facts.task_intent` | tupla `[task_id, kind, attempt]` | el contrato no definía la representación; tupla y objeto eran ambas admisibles | exactamente `{"task_id", "kind", "attempt"}`; `null` sin intención | `custody.task_intent` (`state/v2`) |
| `decision.role` → `decision.next_actor_role` + `decision.planned_delegated_role` | un solo `role` = `EXECUTION_CONTROLLER` | confundía el actor de la acción inmediata con el rol delegado que se planifica después | `next_actor_role`: quien ejecuta la acción inmediata (el primer elemento de `next_points`); `planned_delegated_role`: el rol de la delegación que se planifica tras QR/Q0 (`null` si no hay) | QH con titular RELEASED → QR de un titular nuevo (16.26 T16), igual al `orchestration.next_action.role` canónico; CONTROLLER_PLANNING de D.3 punto 3 → EXECUTION_CONTROLLER (`task_intent.planned_roles`) |

**Formato explicitado sin cambio de valor:** `facts.correction_launches`, `facts.invocations` y `facts.chains` pasan de tuplas sin definir a objetos cerrados
(`{seq, task_id, failure_class}`, `{scope, launched, uncertain}`, `{task_id, state, chain_red_sha}`). En QH2 los tres son `[]`, igual que en v1.

## Campos sin cambio

`branch`, `claim_id`, `last_point`, `record_version`, `protocol`, `principal_state`, `attempts`, `stops_in_force`, `next_points`, `next_window_seq`,
`task_id`, `attempt`, `protocol_set`, `binding`, `worker` y `preconditions`, con las mismas reglas de derivación que v1. Ningún valor esperado se ha
tomado de la respuesta de B1.
