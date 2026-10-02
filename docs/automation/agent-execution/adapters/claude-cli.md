# Adapter `claude-cli`

Este documento es subordinado: no crea requisitos de evidencia, estados, gates, reglas Git ni decisiones del Owner. Ante un conflicto manda la autoridad del dominio y el conflicto se
eleva como STOP.

Descriptor de adapter de la ejecución delegada I62 ([AUTOMATION_PLAN](../../../AUTOMATION_PLAN.md) 16.19), materializado por I-62 e
**inactivo** hasta su vigencia (AUTOMATION_PLAN 16.14). Aquí viven el proveedor, el ejecutable y sus recetas, que el núcleo no nombra
(AUTOMATION_PLAN 16.17). Los estados de las operaciones son los demostrados con la evidencia citada: una operación UNVERIFIED que un rol
necesita deja la celda no elegible para ese rol, y una sesión existente no acredita por sí misma invocación, cancelación ni terminación.

- **AdapterId:** `claude-cli`
- **Proveedor:** Anthropic (Claude, CLI).
- **Runtime:** proceso de CLI lanzado por la sesión, `Transport` = `external-process`.
- **Rol previsto:** REVIEWER, ARCHITECT (orientativo; lo decide el binding).
- **Esquema de hechos:** `schemas/adapters/claude-cli.facts.v1.schema.json` (`rackcad-adapter-claude-cli-facts/v1`).
- **AdapterVersion:** solo se observa invocándola; sin invocar, `UNKNOWN`.

## Operaciones

| # | Operación | Estado | Evidencia |
|---|---|---|---|
| 1 | describir | DISPONIBLE | este descriptor |
| 2 | observar | UNVERIFIED | NOT_AUTHENTICATED (Discovery de I-62); no está en el `PATH` del host (MEASURED en F2). Autenticarla es OD-3 |
| 3 | renderizar | UNVERIFIED | sin invocación medida |
| 4 | invocar | UNVERIFIED | sin invocación medida |
| 5 | observar el resultado | UNVERIFIED | sin invocación medida |
| 6 | cancelar | UNVERIFIED | sin invocación medida |
| 7 | confirmar la terminación | UNVERIFIED | sin invocación medida |
| 8 | clasificar procesos | UNVERIFIED | sin invocación medida |
| 9 | declarar la huella | UNVERIFIED | sin invocación medida |

## Introspección

| Fuente | Nivel | Límite |
|---|---|---|
| — | UNKNOWN | NOT_AUTHENTICATED |

## Huella

`Kind` = UNVERIFIED.

## Hechos (`Facts`)

| Campo | Cómo se observa |
|---|---|
| `AuthState` | NOT_AUTHENTICATED o UNKNOWN; nunca se leen credenciales |
| `BinaryFound` | `where.exe claude` (sin ejecutarla) |
| `CliVersion` | NOT_OBSERVED sin invocación |

## Límites

- Ninguna operación más allá de la descripción se demuestra antes de OD-3 (Proposal V14 §18).
