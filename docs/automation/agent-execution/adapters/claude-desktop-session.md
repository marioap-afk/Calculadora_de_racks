# Adapter `claude-desktop-session`

Este documento es subordinado: no crea requisitos de evidencia, estados, gates, reglas Git ni decisiones del Owner. Ante un conflicto manda la autoridad del dominio y el conflicto se
eleva como STOP.

Descriptor de adapter de la ejecución delegada I62 ([AUTOMATION_PLAN](../../../AUTOMATION_PLAN.md) 16.19), materializado por I-62 e
**inactivo** hasta su vigencia (AUTOMATION_PLAN 16.14). Aquí viven el proveedor, el ejecutable y sus recetas, que el núcleo no nombra
(AUTOMATION_PLAN 16.17). Los estados de las operaciones son los demostrados con la evidencia citada: una operación UNVERIFIED que un rol
necesita deja la celda no elegible para ese rol, y una sesión existente no acredita por sí misma invocación, cancelación ni terminación.

- **AdapterId:** `claude-desktop-session`
- **Proveedor:** Anthropic (Claude, sesión de escritorio de Claude Code).
- **Runtime:** sesión de escritorio de nivel superior; es la propia sesión del Principal, no se lanza ni se invoca.
- **Rol previsto:** PRINCIPAL_COORDINATOR (orientativo; lo decide el binding).
- **Esquema de hechos:** `schemas/adapters/claude-desktop-session.facts.v1.schema.json` (`rackcad-adapter-claude-desktop-session-facts/v1`).
- **AdapterVersion:** la que informa la app; sin fuente, `UNKNOWN`.

## Operaciones

| # | Operación | Estado | Evidencia |
|---|---|---|---|
| 1 | describir | DISPONIBLE | este descriptor |
| 2 | observar | DISPONIBLE | `get_session("self")` devuelve `sessionId`, `model`, `effort` e `isRunning` de los metadatos de la app (MEASURED en F2, evidencia de I-62 §35) |
| 3 | renderizar | NO APLICA | la sesión del Principal no se renderiza desde otra |
| 4 | invocar | NO APLICA | no se invoca: es la sesión que orquesta |
| 5 | observar el resultado | NO APLICA | no produce un resultado de invocación |
| 6 | cancelar | NO APLICA | no la cancela otra sesión |
| 7 | confirmar la terminación | UNVERIFIED | candidata: `isRunning` de los metadatos, observado por **otra** sesión autorizada con `get_session(<sessionId>)`, o atestación del Owner (Proposal V14 §9.1); nunca la propia sesión |
| 8 | clasificar procesos | UNVERIFIED | sin mecanismo medido para atribuir los procesos de la app a la sesión |
| 9 | declarar la huella | UNVERIFIED | candidata: los archivos de ajustes del cliente; no demostrada |

## Introspección

| Fuente | Nivel | Límite |
|---|---|---|
| `get_session` (`model`, `effort`, `sessionId`) | RUNTIME_OBSERVED (medido en F2) | configuración del cliente, no el modelo servido por el backend |

## Huella

`Kind` = UNVERIFIED. Ningún archivo de configuración está demostrado como huella; `NONE` exige la demostración y la aceptación del
Coordinator (AUTOMATION_PLAN 16.19).

## Hechos (`Facts`)

| Campo | Cómo se observa |
|---|---|
| `AuthState` | la sesión existe y opera: AUTHENTICATED; sin leer credenciales |
| `MetadataSource` | `get_session`, o UNAVAILABLE si la herramienta no responde |
| `ReportedModel`, `ReportedEffort` | `model` y `effort` de `get_session("self")` |
| `IsRunning` | `isRunning` de `get_session`; visto por la propia sesión no acredita la terminación |
| `PermissionMode` | `permissionMode` de `get_session` |

## Límites

- `effort` usa la escala del cliente; su traducción al effort semántico está en el catálogo.
- La autodeclaración del modelo no es fuente: solo cuentan los metadatos de la app.
