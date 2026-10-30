# I-62 — Propuesta E1: estado y reanudación utilizables por las iniciativas existentes (decisiones §108)

```text
Tipo:   propuesta breve de implementación (§108, punto 7). No implementa, no integra, no enmienda el Freeze y no activa ninguna superficie.
Base:   Freeze V14 (b64a3b64) con A-1..A-7 AGREED; F6 suspendida; I-61 integrada y vigente; main = bb0d5522.
```

## 1. Qué permite recuperar contexto hoy

- **En main (vigente, I-61):** AUTOMATION_PLAN §8 fija un `docs/automation/state/<I>.yml` (`rackcad-automation-state/v1`) por iniciativa activa como
  «estado transitorio canónico para reanudar», con `next_action`, `gate`, `state` y `last_evidence_commit`, y con Git por encima del archivo. Junto con el
  índice de arranque (CLAUDE.md → HANDOFF → WORKFLOW) es lo que usan hoy las sesiones. **Ningún código lee, valida ni actualiza ese archivo:** cada
  sesión lo interpreta y lo edita a mano.
- **En la rama de I-62 (inactivo, fuera de main):** `state/v2` con `StateV2Validator` (archivo, par e historia), `YamlSubset` (lectura y escritura
  canónicas), el runner del validador, la derivación de hechos y de decisión de FX-04a y el preflight RESUME_DECISION. Funcionan (FX-04a 23/23; I-66
  BOOTSTRAP, QU y QH válidos), pero solo sirven a una unidad que adopte el perfil I62 y cada punto exigió scripts propios.

## 2. Qué falta para que sea utilizable

1. Un comando que haga la reanudación en el worktree: hechos de Git, estado, veredicto y siguiente paso.
2. Un comando que actualice el estado al terminar una tarea, sin editar YAML a mano.
3. Una validación básica del v1. Hay deriva real: I-65 usa `state: in-progress` y un `gate` fuera del vocabulario de §8; I-66 y la rama de I-52 no tienen
   estado v1.
4. Que una sesión nueva lo encuentre sin instrucciones: una línea en CLAUDE.md.

## 3. Diseño mínimo (E1)

Un solo script, `eng/automation/initiative-state.ps1` (PowerShell 7 y git, sin dependencias nuevas; mismo patrón que `eng/validation/*.ps1`):

| Verbo | Qué hace | Qué escribe |
|---|---|---|
| `resume` | Localiza el estado de la rama actual (exactamente un `docs/automation/state/*.yml` con ese `branch:`); comprueba la rama, HEAD igual al remoto, el árbol limpio, `last_evidence_commit` alcanzable desde HEAD y los commits posteriores; valida el v1; cita la última sección de las decisiones y de la evidencia de la iniciativa. Emite `CONTINUE` (`claimed`, `implementing`, `validating` o `ci-failed` con gate `none`), `WAIT <quién>` (gate distinto de `none`, o `waiting`, `review-ready` o `integration-ready`), `DONE` (`completed`) o `STOP <motivo>` ante cualquier incoherencia o dato desconocido, junto con el `next_action` | nada |
| `update` | Al terminar una tarea: fija `next_action` (una línea) y, si se indican, `state`, `gate`, `current_phase` y `attempts`; pone `last_evidence_commit` = HEAD publicado; deja intactas las demás líneas; valida antes de escribir. `-Init` crea el archivo de una iniciativa que no lo tiene | solo ese archivo |
| `check` | La validación de `resume`, sin veredicto | nada |

Reglas: el veredicto es consultivo y solo puede añadir un STOP; nunca autoriza nada que WORKFLOW o I-61 no autoricen. Git prevalece sobre el archivo
(AUTOMATION_PLAN §8). Lo desconocido es STOP (regla de FX-01). Una unidad con `state/v2` en sombra (I-66) se informa como tal y se sigue validando con el
runner existente de `StateV2Validator`; E1 no lo sustituye ni lo amplía. Sin esquemas, auditores ni kits nuevos: el vocabulario es el de §8.

## 4. Archivos que cambian

| Archivo | Cambio |
|---|---|
| `eng/automation/initiative-state.ps1` | nuevo (unas 200 líneas) |
| `tests/RackCad.Tests/InitiativeStateScriptTests.cs` | nuevo, Core: ejecuta el script con `pwsh` sobre repositorios Git temporales (CONTINUE, WAIT, DONE; STOP por rama, por remoto, por evidencia inalcanzable y por vocabulario; ida y vuelta `update` → `resume`) |
| `docs/AUTOMATION_PLAN.md` §8 | un párrafo: el comando lee y actualiza el archivo; su veredicto no autoriza nada |
| `CLAUDE.md` | una línea en «Comandos esenciales»: `pwsh eng/automation/initiative-state.ps1 resume` al abrir el worktree de una iniciativa |
| rama de I-62 | decisiones, A-8 y estado |

No cambian WORKFLOW, LIFECYCLE, AGENTS, el código de producción, el plano (b), I-64 ni I-65.

## 5. Prueba funcional mínima

- **PF-1, lectura en los worktrees existentes:** `resume` en I-62, I-64, I-65, I-66 e I-52, sin escribir nada. Esperado: I-62 `WAIT Owner`
  (gate `owner-decision`) e I-64 `WAIT` (`waiting`), cada una con su `next_action`; I-65 `STOP` por vocabulario (`in-progress`, gate `READY…`); I-66 `STOP: sin estado v1`, señalando su `state/v2` en sombra (QH, titular siguiente
  designado); I-52 `STOP: sin estado v1`.
- **PF-2, relevo real en I-62:** esta sesión cierra la tarea E1 con `update` y publica. El Owner abre una sesión nueva en el worktree de I-62 con solo
  «I-62. Ejecuta resume y continúa». PASS si esa sesión obtiene el veredicto y el siguiente paso que dejó `update`, verificados contra Git, y continúa.
- **I-66:** su AT-3, con B ya preparado, se ejecuta una sola vez tal como está. Después I-66 sigue WORKFLOW normal hasta su cierre (AT-4 = su G1), sin
  AT nuevas.

## 6. Autoridad

Bloqueos exactos del Freeze V14:

- **B-1, alcance:** V14 no contiene E1, y LIFECYCLE §6 exige una A-n material (Architect y Coordinator) para añadir alcance tras el Freeze.
- **B-2, integración:** V14 §15 y §17 (B1 y B3 de `propuesta-entrega-1.md`): I-62 solo se integra entera, tras F6 GATE PASS, con FX-02/C-24, FX-03,
  FX-04b y FX-06 pendientes.

Cambio mínimo: **una A-8 de dos reglas**, que sustituye al borrador de cuatro (`a8-candidata-borrador.md`):

1. E1 entra en el alcance de I-62 sobre el estado v1 vigente del plano (a), consultivo y con `Applies-to: all`. No activa ninguna superficie del plano (b)
   (ni I62_R1, ni `state/v2`, ni preflight, ni bindings) y no cambia ADR-0046.
2. E1 tiene un gate propio (RED → GREEN, Core Full, CI 4/4, PF-1, PF-2 y una Owner Validation compacta: el Owner ve un `resume` correcto) y se integra
   por separado, desde una rama de entrega creada desde `origin/main` que solo contiene E1. El resto de I-62 conserva sin rebaja su Freeze, F6,
   FX-02/C-24 y OD-1.

Quién decide: el Coordinator aprueba esta propuesta y la A-8, con una sola revisión del Architect y sin rondas. El Owner autoriza expresamente la rama de
entrega (§108, punto 2) y hace la OV compacta. No hacen falta OD-1, I62_R1, A6-7, FX-02 ni Codex.

## 7. Primera acción ejecutable

Con la aprobación del Coordinator: publicar la A-8 (una página, con las obligaciones de prueba del punto 4) y su única revisión del Architect. Con
AGREED, en la rama de I-62: el RED de `InitiativeStateScriptTests` (falla porque el script no existe); después el GREEN, `update` del estado de I-62, CI y
PF-1. Para PF-2, el Owner abre una sesión nueva; la rama de entrega se crea al integrar, con la autorización del Owner.
