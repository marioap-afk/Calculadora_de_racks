# Adapter `codex-desktop-session`

Este documento es subordinado: no crea requisitos de evidencia, estados, gates, reglas Git ni decisiones del Owner. Ante un conflicto manda la autoridad del dominio y el conflicto se
eleva como STOP.

Descriptor de adapter de la ejecución delegada I62 ([AUTOMATION_PLAN](../../../AUTOMATION_PLAN.md) 16.19), materializado por I-62 e
**inactivo** hasta su vigencia (AUTOMATION_PLAN 16.14). Aquí viven el proveedor, el ejecutable y sus recetas, que el núcleo no nombra
(AUTOMATION_PLAN 16.17). Los estados de las operaciones son los demostrados con la evidencia citada: una operación UNVERIFIED que un rol
necesita deja la celda no elegible para ese rol, y una sesión existente no acredita por sí misma invocación, cancelación ni terminación.

- **AdapterId:** `codex-desktop-session`
- **Proveedor:** OpenAI (Codex, sesión de la app de escritorio).
- **Runtime:** sesión de escritorio de nivel superior, candidata a Principal en la topología B; no se lanza ni se invoca.
- **Rol previsto:** PRINCIPAL_COORDINATOR (topología B) (orientativo; lo decide el binding).
- **Esquema de hechos:** `schemas/adapters/codex-desktop-session.facts.v1.schema.json` (`rackcad-adapter-codex-desktop-session-facts/v1`).
- **AdapterVersion:** la del paquete de la app; sin fuente, `UNKNOWN`.

## Operaciones

| # | Operación | Estado | Evidencia |
|---|---|---|---|
| 1 | describir | DISPONIBLE | este descriptor |
| 2 | observar | UNVERIFIED | candidata SP-3: `turn_context.model` y `effort` de los registros de sesión de la app; no leídos en F2 (sesiones ajenas, Proposal V14 D.6) y sin ligar a unidad y rol |
| 3 | renderizar | NO APLICA | la sesión del Principal no se renderiza desde otra |
| 4 | invocar | NO APLICA | no se invoca: es la sesión que orquesta |
| 5 | observar el resultado | NO APLICA | no produce un resultado de invocación |
| 6 | cancelar | NO APLICA | no la cancela otra sesión |
| 7 | confirmar la terminación | UNVERIFIED | sin observador conocido; se admite la atestación del Owner (§9.1) |
| 8 | clasificar procesos | UNVERIFIED | sin mecanismo medido para atribuir los procesos de la app a la sesión |
| 9 | declarar la huella | UNVERIFIED | ¿comparte `~/.codex/config.toml`? La app reescribió ese archivo al actualizarse y reiniciarse (evidencia de I-63 §45), pero que la sesión lo lea no está medido |

## Introspección

| Fuente | Nivel | Límite |
|---|---|---|
| `turn_context.model/effort` (SP-3) | RUNTIME_OBSERVED, **candidata** | ligar a unidad y rol; no generalizar formatos |

## Huella

`Kind` = UNVERIFIED hasta demostrar si comparte `config.toml`.

## Hechos (`Facts`)

| Campo | Cómo se observa |
|---|---|
| `AuthState` | UNKNOWN sin una sesión ligada a la unidad |
| `AppPackage` | paquete instalado de la app, si se observa sin abrir sesiones |
| `TurnContextSource` | CANDIDATE mientras SP-3 no esté demostrada |
| `SharesConfigFile` | UNKNOWN hasta demostrarlo |

## Límites

- La topología B depende de OD-3 y OD-4 (Proposal V14 §18).
