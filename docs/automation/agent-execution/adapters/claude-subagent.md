# Adapter `claude-subagent`

Este documento es subordinado: no crea requisitos de evidencia, estados, gates, reglas Git ni decisiones del Owner. Ante un conflicto manda la autoridad del dominio y el conflicto se
eleva como STOP.

Descriptor de adapter de la ejecución delegada I62 ([AUTOMATION_PLAN](../../../AUTOMATION_PLAN.md) 16.19), materializado por I-62 e
**inactivo** hasta su vigencia (AUTOMATION_PLAN 16.14). Aquí viven el proveedor, el ejecutable y sus recetas, que el núcleo no nombra
(AUTOMATION_PLAN 16.17). Los estados de las operaciones son los demostrados con la evidencia citada: una operación UNVERIFIED que un rol
necesita deja la celda no elegible para ese rol, y una sesión existente no acredita por sí misma invocación, cancelación ni terminación.

- **AdapterId:** `claude-subagent`
- **Proveedor:** Anthropic (Claude, subagente de una sesión de Claude Code).
- **Runtime:** subagente de la sesión, `Transport` = `session-internal`; comparte la sesión de su padre.
- **Rol previsto:** WORKER, REVIEWER (orientativo; lo decide el binding).
- **Esquema de hechos:** `schemas/adapters/claude-subagent.facts.v1.schema.json` (`rackcad-adapter-claude-subagent-facts/v1`).
- **AdapterVersion:** la de la sesión padre.

## Operaciones

| # | Operación | Estado | Evidencia |
|---|---|---|---|
| 1 | describir | DISPONIBLE | este descriptor |
| 2 | observar | DISPONIBLE | transcripción del subagente: modelo y effort heredados (MEASURED en I-61 G2 y en los pilotos de I-63 e I-64) |
| 3 | renderizar | DISPONIBLE | el prompt compuesto de la llamada (PROMPT_TEMPLATES §G para I61); para I62, la invocación renderizada desde el contrato neutral |
| 4 | invocar | DISPONIBLE | llamada cuya finalización se notifica (AUTOMATION_PLAN 16.4), medida en I-61 G2 |
| 5 | observar el resultado | DISPONIBLE | resultado final de la llamada y transcripción |
| 6 | cancelar | DISPONIBLE | tope de 60 min con detención de la tarea (16.4) |
| 7 | confirmar la terminación | DISPONIBLE | notificación de finalización + comprobación de procesos de 16.4 (ningún descendiente ni huérfano vivo) |
| 8 | clasificar procesos | DISPONIBLE | clasificación `session-descendant` y `orphan` de 16.4 |
| 9 | declarar la huella | UNVERIFIED | candidata: los ajustes de la sesión padre; no demostrada |

## Introspección

| Fuente | Nivel | Límite |
|---|---|---|
| transcripción | RUNTIME_OBSERVED | prompt enmarcado |

## Huella

`Kind` = UNVERIFIED (Proposal V14 §7, columna 9).

## Hechos (`Facts`)

| Campo | Cómo se observa |
|---|---|
| `AuthState` | el de la sesión padre |
| `ParentSessionState` | OBSERVED si la sesión padre tiene `SessionRef` observado |
| `TranscriptSource` | TRANSCRIPT tras lanzarlo; NOT_STARTED para una candidata sin lanzar |
| `CallTimeoutMinutes` | 60 (16.4) |
| `CompletionNotified` | YES o NO tras la llamada; NOT_STARTED antes |

## Límites

- Que el subagente aplique un modelo y un effort **solicitados** distintos de los heredados no está medido.
- Un subagente no es un actor independiente de su sesión padre para la dimensión Sesión (Proposal V14 §11.1).
