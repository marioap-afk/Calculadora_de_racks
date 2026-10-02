# I-62 — Revisión formal limpia del Architect de la Proposal V10 (R20261002T010757Z-ef59)

```text
LogicalReviewRequestId: L20261002T010757Z-ef59   InvocationId: I20261002T010757Z-ef59   AttemptSeq: 1   RunId: R20261002T010757Z-ef59
Autorización:   OWNER DECISION — CLEAN ARCHITECT REVIEW OF I-62 PROPOSAL V10 (decisiones §21); UNA invocación de codex-cli, solo revisión
Objeto:         commit 41ded86e6494e3d43e07ce97de9eedf4710a631f (CI de publicación 36948649343, push, 4/4 success)
                docs/initiatives/I-62-proposal-v10.md          blob 58f88fc602d0d4eaf3900a3514259da6b94faba2
                docs/initiatives/I-62-architect-package-v10.md blob 4d9df7edb94d44633361621623a8eb33131d87d2
Veredicto:      CHANGES REQUIRED (registro: docs/initiatives/I-62-architect-review-v10.md)
Naturaleza:     evidencia de una invocación real. output.json, closure.json, read-audit.json y runtime-evidence.json son representaciones EXPERIMENTALES
                de los contratos de la Proposal V10 (B.10.1, §20.3.1, §20.3.2); no son autoridad normativa. I-61 sigue vigente.
                IMPLEMENTATION AUTHORIZATION = NO
Medición:       la hizo la sesión autora (Claude), que lanzó el proceso; no hay observador independiente
```

## 1. Archivos custodiados

Todos son copia byte a byte de los del directorio de la corrida y usan saltos de línea LF. La identidad se comprueba con `git show <commit>:<ruta> | sha256sum`.

| Archivo | Bytes | SHA-256 | Papel |
|---|---|---|---|
| `prompt.md` | 12 449 | `d8a5a6fa3b34814d8717f67b253cfb647e3246c08cc206c473c7f4142547294e` | prompt completo entregado como argumento: cabecera de invocación, cierre, hallazgos abiertos y, literal entre marcas, la orden del Owner (5 714 bytes; SHA-256 `619b4fada8f9cb682b8e2e95013bbcbb6c221020a964d23303b62ee78f98dfb2`) |
| `schema.json` | 5 257 | `5903251e15fb04d3a5a2612d319a5ed74f0e9fd6bea8b8826195bc6f9a606295` | esquema de salida (`--output-schema`): representación experimental de `rackcad-architect-review-result/v1`, más tres campos que pide esta orden (`FocusAreas`, `FreezeReadinessAfterOD6`, `AutonomyAssessment`) |
| `closure.json` | 15 683 | `c47cef025036ad49e7ff96bbc58207c43d537060075df9ff3fb9c139504627d0` | `EffectiveInputClosure` calculado y custodiado antes del lanzamiento: 37 insumos canónicos y 9 transitivos, con su blob y su SHA-256; acciones permitidas y eximidas; obligaciones no activadas con su motivo; contexto de runtime declarado; insumos prohibidos |
| `output.json` | 36 626 | `d65ccece955289a9d4cd9e9bb133cd1cb639c36565be5b5da8af68d6e9b4b4d4` | resultado literal del Architect (`-o`) |
| `read-audit.json` | 17 031 | `feef4500d6d50054ac2d2b4c30fec64a120c5180c2a9d52ee421f53aefc75fff` | auditoría de lecturas frente al cierre y medición de la degradación de caracteres (§5) |
| `runtime-evidence.json` | 4 225 | `2c59a41d75c5964a6ace547eeef024a2f2a227ad7c2584e3b948ff51bc43b7cd` | identidad observada por el invocador (`RuntimeEvidenceRef`) |

Archivos que **no** se versionan. Se registran su identidad y los campos extraídos:

| Archivo | Bytes | SHA-256 | Motivo |
|---|---|---|---|
| `events.jsonl` (salida `--json`) | 1 777 913 | `99d4e4cb42c2eb65ca29365831b3ec0ead05cd4a2324c419ca973c596bc33593` | traza completa con el contenido leído del repositorio |
| log de sesión de Codex (`rollout-2026-10-01T19-09-50-01a0fa29-0f9e-7321-88d3-a62f3c7bfe15.jsonl`, 241 líneas) | — | `339a1f25302dd35b8d82d2d5cbdb46caf5bd07b00128b3c9112ae38c51dfc5ba` | instrucciones del runtime y estado del host |
| `stderr.txt` | 39 | `1aa26269eb1cc57f86b235a03cda53c004edb5b1e9fc99d4da4f00843293d721` | solo «Reading additional input from stdin...» |

## 2. Excepción del Owner a AGENTS.md «Leer primero» paso 4 (texto exacto)

Sección literal de la decisión del Owner, 1 158 bytes en UTF-8, SHA-256 `68cfe8dc0be48232bf7d20e08aadb4e8cbbf6fbbcb7b8a714d78958d5233b461`:

```text
OWNER DECISION ON AGENTS.md `dotnet test`
For THIS Architect review only, I explicitly authorize an exception to AGENTS.md "Leer primero" step 4:
DO NOT execute `dotnet test` from the Architect invocation.
Reason:

* the Architect is intentionally executed in a read-only sandbox;
* this action is an architectural/document review, not a product-validation gate;
* `dotnet test` may create local build/test artifacts and conflicts with the intended read-only isolation;
* publication health of the exact reviewed SHA is already independently recorded by CI run 36948649343.

IMPORTANT:
The CI evidence is NOT declared equivalent to, or a substitute for, Core local evidence.
AGENTS explicitly distinguishes local Core and CI Core as separate evidence classes.
This Owner decision is a narrowly scoped exception to the initial-read command for this Architect invocation only.
It does not:

* change AGENTS;
* establish a reusable CI-for-local substitution;
* change Candidate requirements;
* change gate test requirements;
* apply to implementation, Controller, Worker, Candidato or closure validation.

Record this exact exception in the invocation evidence.
```

Aplicación medida: `dotnet test` **no** se ejecutó (`read-audit.json`, `DotnetTestExecuted` = false). La CI 36948649343 se registra solo como salud de publicación del
SHA exacto, nunca como equivalente de Core local.

## 3. Condiciones previas (MEASURED antes de lanzar, 01:06Z-01:09Z)

| Condición | Medición |
|---|---|
| binario | `%LOCALAPPDATA%\OpenAI\Codex\bin\a51e250fa15c740a\codex.exe`, `codex-cli 0.159.2`, SHA-256 `fcd5eafefb4ff4a607f244e099e0974f66e17966b6ffda6948de2ef3a7a79530`; es el único `codex.exe` de los directorios `bin` |
| autenticación | solo la existente: «Logged in using ChatGPT»; sin leer ni modificar credenciales |
| huella de configuración | `~/.codex/config.toml`, SHA-256 `40c27b57…` antes del lanzamiento |
| modelo y effort solicitados | `-m gpt-6.1-sol`, `-c model_reasoning_effort="high"` |
| sandbox | `-s read-only` |
| proceso independiente | `codex exec` nuevo, sin memoria (`--disable memories`), con `apps`, `plugins`, `hooks` y los navegadores desactivados para esta invocación; `~/.codex/AGENTS.md` global de 0 bytes |
| clon limpio | `D:\r62-arch-v10`, clonado del remoto con `core.longpaths`, HEAD desacoplado en `41ded86e`, árbol limpio, los tres blobs verificados; el repositorio no versiona `.codex/` ni reglas `.rules` |
| sin transcripción ni memoria del autor | el prompt solo contiene la cabecera, el cierre y la orden; los insumos prohibidos están en `closure.json` |
| cierre efectivo | calculado según V10 §20.3.1 y el paquete V10 §0, hasta el punto fijo (`closure.json`) |
| colisión de escritores | la rama remota en `41ded86e` al lanzar; ningún proceso con rutas de I-62 o del clon; el único otro proceso de Codex es el `exec-server --remote` del cliente de escritorio, sin ruta de I-62 |

## 4. Ejecución, identidad observada y terminación (MEASURED; detalle en `runtime-evidence.json`)

- **Ejecución:** de 01:09:49Z a 01:18:31Z (8 min 42 s), salida 0, con `turn.completed`.
- **Consumo:** entrada 3 698 817 tokens (3 431 040 en caché) y salida 19 561 (4 494 de razonamiento); sin avisos de límite.
- **Identidad observada** (RUNTIME_OBSERVED en el registro de sesión):
  - hilo `01a0fa29-0f9e-7321-88d3-a62f3c7bfe15`, `originator` = `codex_exec`, `cwd` = el clon;
  - los dos `turn_context` dicen `gpt-6.1-sol`, `high`, `approval_policy` = `never` y sandbox `read-only`.

  El modelo servido detrás del proveedor no es observable.
- **Compactación:** a las 01:15:05Z el runtime compactó el contexto del revisor dentro del mismo hilo, con el mismo modelo, effort y sandbox.
- **Identidad declarada:** el revisor declaró UNKNOWN en modelo, effort e hilo. No es contradicción (§20.3.2).
- **Después de la corrida:**
  - hash de `config.toml` sin cambio;
  - clon en `41ded86e`, limpio y sin archivos ignorados;
  - ningún proceso del revisor vivo.

## 5. Auditoría de lecturas y fidelidad de la lectura

- **Comandos:** 41, todos de lectura y todos con `-NoProfile`.
- **Rutas leídas:** las 46 rutas del cierre. Ninguna lectura queda fuera del cierre, no hay ningún comando de escritura y `dotnet test` no se ejecutó.
- **Git:** solo `git log --oneline -10`, `git rev-parse` y `git status`.
- **Código 1:** un comando, porque `rg` no existe en el runtime; sus lecturas `Get-Content` se completaron.

**Degradación de caracteres (MEASURED).** La salida de consola del runtime **no** conservó los caracteres no ASCII del objeto revisado. En las salidas de los
comandos que leyeron `I-62-proposal-v10.md`:

| Fuente | Lo que vio el revisor |
|---|---|
| ≤ (13 en el objeto) y ≥ (14) | «=» |
| ≠ (33) | «?» |
| → (352) | U+001A |
| ⇒ (25), ⇔ (14) y ∈ (8) | perdidos |
| letras acentuadas | U+FFFD (5 915 en total) |

Ejemplo verificable: las líneas 1295 y 1854 del objeto dicen «`review_rounds` ≤ `logical_requests`», y el revisor leyó
«`review_rounds` = `logical_requests`». El resultado (`output.json`) sí tiene la codificación correcta, porque lo escribe el modelo y no la consola. La
cabecera de la invocación no forzó UTF-8 en la consola del runtime (GAP-09, evidencia §23).

## 6. Límites

1. La medición la hizo la sesión autora, que lanzó el proceso.
2. El modelo servido detrás del proveedor no es observable.
3. El registro no captura las lecturas internas del runtime.
4. La compactación de contexto ocurrió dentro del hilo, y su efecto sobre el razonamiento del revisor no es observable.
5. El texto que leyó el revisor no fue el objeto byte a byte (§5). Cuánto afecta eso a cada hallazgo no lo decide esta sesión.
