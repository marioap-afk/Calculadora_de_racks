# I-62 — Transportes para una revisión limpia del Architect (investigación; nada se invoca)

> **Preparación (Track H).** Medido el 2026-10-04 sin invocar ningún modelo, sin autenticar, sin cambiar configuración y sin leer archivos de credenciales.
> Motivo: la revisión de A-1 está autorizada, pero su lanzamiento espera un clic del Owner (AUTONOMY_GAP = OWNER_CLICK_REQUIRED; `a1-review-kit/README.md`).

## 1. Matriz

| Transporte | ¿Crea una sesión nueva? | Aislamiento de contexto | Modelo/effort observables | Solo lectura | Autenticación | ¿Clic del Owner? | ¿OD? | ¿Reutilizable en FX-06? |
|---|---|---|---|---|---|---|---|---|
| **Tarea de la app** (`spawn_task`, sesión de escritorio en un clon limpio fuera del repositorio) | sí (sesión CCD nueva, worktree fresco) | bueno: otro repositorio ⇒ sin memoria del proyecto autor; sin `~/.claude/CLAUDE.md` de usuario (MEASURED); inyecta el `CLAUDE.md` del clon | sí: `get_session` (modelo, effort) + transcripción (modelo por mensaje) | por instrucción y por el modo de permisos de la sesión nueva; no hay sandbox de SO | la de la app | **sí** | ninguna | **no**: el clic es un relevo manual (AUTONOMY_GAP; C-37) |
| **`codex-cli`** (`exec -s read-only`, clon limpio, `--disable` memorias/apps/plugins) | sí (`thread_id` propio) | bueno (medido en las revisiones V10-V14) | sí: `turn_context` del registro de sesión | **sí**, sandbox | suscripción existente | no | **OD-2** (huella: la de hoy es `091540ED…`, distinta de todas las anteriores) | sí: binding previsto del Architect (D.8) |
| **`codex-desktop-session`** | la abre el Owner en la app | depende de la memoria y las instrucciones de la app | sí (registro de sesión) | configurable | la de la app | **sí** | OD-2 si comparte `config.toml` | como Principal B, no como Architect invocado |
| **`claude-cli`** (`~/.local/bin/claude.exe`, Claude Code 2.1.270, fuera del `PATH`; SHA-256 `FD7F35EC…`) | sí (modo sin interfaz, `-p`) | bueno desde un clon limpio (otro proyecto ⇒ sin memoria del autor) | sí: evento inicial con el modelo y transcripción | por permisos y herramientas permitidas; sin sandbox de SO | **UNKNOWN** (el Discovery la registró NOT_AUTHENTICATED; no se ha vuelto a medir porque medir exige ejecutarla) | no | **OD-3** | sí: binding alternativo del Architect (D.8) |
| **`claude-subagent`** (herramienta de agentes de esta sesión) | contexto nuevo, pero **la misma sesión** (B.2: un subagente comparte la `SessionRef` de su padre) | parcial | el modelo se elige | con herramientas de solo lectura | la de la sesión | no | ninguna | como Worker o Reviewer (D.3), **no** como Architect de una revisión mayor (Sesión REQUIRED) |
| **sesiones existentes** (`SendMessage`) | no | malo: arrastran contexto de otras revisiones y la memoria del proyecto | sí | no garantizado | — | no | — | no |
| **sesión en la nube** (`start_session` con destino cloud) | sí | bueno | sí | configurable | la cuenta | — | — | posible en el futuro; **la herramienta no está disponible en esta sesión** |
| **EXTERNAL HUMAN** | una revisión humana | humano distinto de autor y operador | n/a | n/a | n/a | es humana | — | no (no es autonomía) |

## 2. Conclusión: qué debe aportar I-62

Hoy no existe un transporte limpio, separado y lanzable sin intervención humana que no esté detrás de una OD:
- **con OD-2:** `codex-cli` ya cumple todo (diseñado y medido en I-61 y en las revisiones de I-62); la OD fija su línea base de huella;
- **con OD-3:** `claude-cli` está instalado (2.1.270) y su descriptor necesita la medición de las operaciones 2-7; sin OD-3 su autenticación es UNKNOWN;
- **sin OD:** solo la tarea de la app, que exige un clic. Para FX-06 eso es un relevo manual: UNVERIFIED si falta una OD, UNSUPPORTED si, con las OD concedidas,
  ningún runtime puede lanzar un Architect independiente (D.8).

**Capacidad que cierra el AUTONOMY_GAP:** un adapter de Architect con las operaciones 2-7 en DISPONIBLE (observar, renderizar, invocar, observar el resultado,
cancelar y confirmar la terminación) que el Principal pueda lanzar por programa, con sesión propia, contexto aislado, solo lectura e identidad observada. Las
vías son `codex-cli` (OD-2), `claude-cli` (OD-3, más su medición) o un adapter de sesión de escritorio con una operación programática de «crear sesión» que
hoy esta sesión no tiene.
