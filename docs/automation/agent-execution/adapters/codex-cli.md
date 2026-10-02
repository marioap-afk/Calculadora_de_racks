# Adapter `codex-cli`

Este documento es subordinado: no crea requisitos de evidencia, estados, gates, reglas Git ni decisiones del Owner. Ante un conflicto manda la autoridad del dominio y el conflicto se
eleva como STOP.

Descriptor de adapter de la ejecución delegada I62 ([AUTOMATION_PLAN](../../../AUTOMATION_PLAN.md) 16.19), materializado por I-62 e
**inactivo** hasta su vigencia (AUTOMATION_PLAN 16.14). Aquí viven el proveedor, el ejecutable y sus recetas, que el núcleo no nombra
(AUTOMATION_PLAN 16.17). Los estados de las operaciones son los demostrados con la evidencia citada: una operación UNVERIFIED que un rol
necesita deja la celda no elegible para ese rol, y una sesión existente no acredita por sí misma invocación, cancelación ni terminación.

- **AdapterId:** `codex-cli`
- **Proveedor:** OpenAI (Codex, CLI).
- **Runtime:** proceso de CLI (`codex exec`) lanzado por la sesión, `Transport` = `external-process`.
- **Rol previsto:** EXECUTION_CONTROLLER, ARCHITECT, WORKER (orientativo; lo decide el binding).
- **Esquema de hechos:** `schemas/adapters/codex-cli.facts.v1.schema.json` (`rackcad-adapter-codex-cli-facts/v1`).
- **AdapterVersion:** la de la CLI observada en la invocación (`codex --version`); sin invocar, `UNKNOWN`.

## Operaciones

| # | Operación | Estado | Evidencia |
|---|---|---|---|
| 1 | describir | DISPONIBLE | este descriptor |
| 2 | observar | DISPONIBLE | estático sin invocar: ruta y SHA-256 del binario, huella de `config.toml`; la versión, la autenticación y el registro de sesión solo invocando (MEASURED en I-61 y en las revisiones de I-62, evidencia §§21-31). Con la huella sin línea base aceptada, invocar exige OD-2 |
| 3 | renderizar | DISPONIBLE | receta de AUTOMATION_PLAN 16.4: binario por ruta verificada, `-C <worktree>`, `-s read-only`, `-m` y `-c model_reasoning_effort=…` de una celda elegible, `--output-schema`, `-o`, `--json`, sin `--ephemeral`, stdin cerrado y el `pwsh` del runtime primero en el `PATH` |
| 4 | invocar | DISPONIBLE | `codex exec` de solo lectura medido (I-61 G2-G3; revisiones de I-62); binario a revalidar en cada invocación. Escritura (WORKER) no demostrada: OD-4 |
| 5 | observar el resultado | DISPONIBLE | `-o` al directorio del `RunId` y eventos `--json` |
| 6 | cancelar | DISPONIBLE | tope de 600 s con el árbol del proceso terminado y la muerte confirmada (16.4) |
| 7 | confirmar la terminación | DISPONIBLE | PID + `CreationDate` del proceso lanzado y de su árbol, muertos en la entrada (16.4) |
| 8 | clasificar procesos | DISPONIBLE | lista cerrada de 16.4 (`codex*.exe` y exclusión nominal de `codex-windows-sandbox-service.exe`) |
| 9 | declarar la huella | DISPONIBLE | `~/.codex/config.toml`: SHA-256 y nombres ordenados de secciones y claves, sin valores y saneados; obligatoria. P-01 se conserva |

## Introspección

| Fuente | Nivel | Límite |
|---|---|---|
| registro de sesión sin `--ephemeral` (`turn_context.model` y `effort`) | RUNTIME_OBSERVED | solo para el binario medido; un binario nuevo se mide de nuevo |

## Huella

`Kind` = CONFIG_FILE sobre `~/.codex/config.toml`: `Sha256` del archivo y `KeyNames` saneados (README §13). Un cambio durante la cesión es
STOP P-01 (P-11 lo generaliza). La línea base aceptable para invocar en una unidad I62 es OD-2 (Proposal V14 §18).

## Hechos (`Facts`)

| Campo | Cómo se observa |
|---|---|
| `AuthState` | solo invocando la CLI; sin invocar, UNKNOWN (nunca se leen credenciales) |
| `BinaryLabel` | nombre del directorio de versión que contiene el binario verificado |
| `BinarySha256` | SHA-256 del binario |
| `CliVersion` | `codex --version`, solo invocando; sin invocar, NOT_OBSERVED |
| `SandboxMode` | el modo que pide la receta (`read-only`) |
| `SessionLog` | PERSISTED (sin `--ephemeral`) |
| `RuntimeShellFirstInPath` | YES si el `pwsh` del runtime va primero en el `PATH` del proceso hijo |

## Límites

- Invocar la CLI puede reescribir `config.toml` (DEV-G1C-01 de I-61): por eso cada cesión compara la huella de salida y de entrada.
- La app de escritorio instala binarios nuevos junto a los anteriores; el binario se elige por ruta verificada, nunca por el más reciente.
- El `pwsh` del sandbox corre en ConstrainedLanguage (evidencia de I-62 y de I-64): las comparaciones estructuradas con `[pscustomobject]` fallan.
