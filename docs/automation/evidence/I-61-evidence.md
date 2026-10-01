# I-61 — Evidencia de la unidad (Agent Execution, Model Routing & Prompting Protocol)

Unit / Initiative / Workflow: `I-61` / I-61 / V2. Claim-Id `0e2923de-e1a7-41bf-b7db-50ec84217850`.
Contrato: [I-61-protocolo-ejecucion-agentes.md](../../initiatives/I-61-protocolo-ejecucion-agentes.md). Decisiones: [I-61.md](../decisions/I-61.md).

Estado de esta evidencia: **unidad integrada el 2026-10-01 por orden del Owner**; los hechos posteriores al merge están en el tag `integration/I-61`. Recorrido: G0 (§§1-8), G1 y G1-C (§§9-11), diseño y Freeze
(§§12-13), G2 (§14), G3 (§15) y Candidato final con READY-01..09 y Owner Validation PASS (§16). Las secciones anteriores son registros históricos de su momento y no se
reescriben (WORKFLOW §11.4); donde dicen PENDIENTE, lo posterior consta en las secciones siguientes.

## 1. Base de reclamo y clasificación de transición

| Hecho | Valor | Fuente |
|---|---|---|
| Base (`origin/main`) | `95690c28dc6268e61dff32a0cbc33cc9fde3d47f` (revalidada en el reclamo; `main` = `origin/main`, árbol limpio, sin operaciones Git) | `git fetch` + `git rev-parse` |
| Commit de reclamo | `21af80e81a3f96649fa6e649374ec8591548a082` (vacío; fecha 2026-09-30T15:44:44-06:00) | `git log` |
| Primer push aceptado (reclamo) | `architecture/protocolo-ejecucion-agentes` nueva en `origin`, sin force | salida de `git push -u` |
| Rama / worktree | `architecture/protocolo-ejecucion-agentes` / `.claude/worktrees/architecture-protocolo-ejecucion-agentes` (del repo principal) | `git worktree list` |
| ID | I-61 libre al reclamar: mayor ID en `origin/main` = I-60; sin I-61 en ramas, tags ni documentos | `git grep`, `git ls-remote` |
| `WORKFLOW_V2_EFFECTIVE_SHA` | `8a021fb67c16dfccd6afc18448ea7e6a71a32364`, ancestro de la base | `git merge-base --is-ancestor` (D0) |
| Fin de la pausa | `integration/I-56` (tag anotado): `Claim pause … end=2026-09-17T22:30:00Z` | `git cat-file -p integration/I-56` (D0) |
| Clasificación | **V2, T4** (posterior probado, base con el SHA efectivo, sin pausa) | [WORKFLOW](../../WORKFLOW.md) §11.3 |

## 2. Fuentes recibidas (entrada de G0)

Transferencia directa de texto (orden C61-G0-07 del Coordinator), escrita **fuera del repositorio** en `D:\Documentos\Codex\I-61-G0-inputs-texto\`.
`SHA256SUMS.txt` de esa carpeta es un manifiesto de **esta transferencia mínima** (2 archivos); no afirma haber recibido ni verificado el ZIP anterior.

| Archivo | Bytes | SHA-256 | Verificación |
|---|---:|---|---|
| `01-owner-mandate.original.txt` | 16 415 | `2cb2770c8c4044cf34862942b88811defc4615376df5edbd75343791a98d098e` | coincide con el manifiesto literal; UTF-8 sin BOM, LF, sin LF final |
| `02-coordinator-decisions.md` | 6 089 | `77681380ce7664c0274799cd824e82914c7c85be77f6e58e3da19ee550e916e9` | coincide con el manifiesto literal; UTF-8 sin BOM, LF, un LF final |

- Versionado: **solo** el mandato, como [`I-61-owner-mandate.txt`](../decisions/I-61-owner-mandate.txt) (una copia; mismo SHA-256 que arriba,
  comprobado sobre el blob de Git). El archivo 02 no se versiona: sus decisiones constan en [I-61.md](../decisions/I-61.md) §2 con atribución al Coordinator.
- Cadena de custodia: Owner → Coordinator → Executor; **sin verificación independiente contra el adjunto original**.
- Los borradores históricos (esquemas, transcripción de catálogo) no se recibieron ni se versionan; no son Freeze ni aprobación de diseño.
- Nota de fin de línea: con `core.autocrlf=true` un checkout en Windows puede convertir el `.txt` a CRLF; el hash canónico es el del blob (LF).

## 3. Coordinación de escritura de `docs/ROADMAP.md` frente a I-52

| Campo | Valor |
|---|---|
| Canal | mensajería entre sesiones locales (sesión de I-61 → sesión «I - 52»); solicitud `msg_id 2fb9ce71-0192-4b3c-ac4f-884b3fc1a0fe` |
| Respuesta | **ACUSE** de la sesión orquestadora de I-52 (única escritora de su rama; afirma autoridad sobre sus escrituras de ROADMAP): no escribe, rebasa ni publica cambios que toquen `docs/ROADMAP.md` hasta el «RELEASE» |
| Archivo y alcance | solo `docs/ROADMAP.md`; el resto del trabajo de I-52 continúa |
| INICIO | el mensaje de acuse (recibido antes del reclamo; el canal no registra hora exacta → no la invento) |
| FIN | mensaje «RELEASE» de I-61 por el mismo canal, enviado tras publicar el bootstrap (`msg_id e6660bd2-5704-4188-9bcf-cc2f9e92cd7d`); no esperó a la CI. La entrega quedó encolada en la sesión de I-52; no hay acuse de lectura (el canal no lo reporta) |
| Estado observado de I-52 antes del reclamo | tip `65e465a71e8e91d60fc9315dace1aac55c0db5de`, 0 detrás y 140 delante de la base; modifica `docs/ROADMAP.md` (líneas 471 y 478 de la base) |
| Compatibilidad textual | una fusión de ensayo de una fila en la tabla Engineering Productivity con la rama de I-52 no produjo conflicto. **No** se usa como prueba de exclusividad (C61-G0-03) |

Observación sin acción (C61-G0-05): la rama de I-52 edita en ROADMAP la fila de I-57; pendiente de procedencia/autorización, no auditada ni corregida.

## 4. Desviación local del preflight (C61-G0-04)

El Executor creó en el repositorio principal un commit **sin ref** `a10f1c0954207a17bc8fd64d1ef5a517ca25b665` para ensayar la fusión descrita en §3,
con un índice temporal que luego borró. Es un efecto local adicional al índice transitorio y una desviación del preflight de solo lectura; no es claim,
commit de producto ni evidencia de exclusividad. No se creó ref ni se ejecutó `gc`/`prune`; no se repitió. Observación atribuida al Executor.

## 5. Bootstrap (contenido versionado de G0)

Archivos nuevos: el contrato `docs/initiatives/I-61-protocolo-ejecucion-agentes.md`; `docs/automation/decisions/I-61.md` y
`docs/automation/decisions/I-61-owner-mandate.txt`; `docs/automation/state/I-61.yml`; este archivo. Archivo modificado: `docs/ROADMAP.md`
(una fila en Engineering Productivity). **Nada más**: sin cambios en normas globales, `HANDOFF`, índice ADR, `src/`, `tests/`, `assets/`, CI ni configuración.
`automation.enabled: false`; las banderas `requires_*: false` no conceden exenciones de Owner Validation ni AutoCAD.

## 6. Validación de G0

- **Comprobación documental:** `git diff --name-only` del bootstrap contra la base: 6 archivos, todos bajo `docs/` (0 fuera de `docs/`).
- **CI exacta del bootstrap** ([WORKFLOW](../../WORKFLOW.md) §4.5.2, commit documental): corrida de `event=push` **36781748452** sobre
  `head_sha` = `c701ff8f8cd3ee58f7dfab529e092d1f5e18190b`: conclusión `success`, 4/4 jobs `success` (Build UI, UI Tests, Tests Domain + Application,
  Build Plugin without AutoCAD). La del commit de reclamo (`21af80e8`, corrida 36781382840): `success`.
- La CI del commit de seguimiento de este registro (§7) es propia de ese SHA y se informa al Coordinator; no se copia aquí (evita la recursión).
- Suites Core/UI locales, builds del Plugin y Owner Validation: **no aplican a G0** (sin cambios de producto, `src/`, `tests/` ni `assets/`);
  no se ejecutaron.

## 7. Registros posteriores a la publicación del bootstrap

- Commit de bootstrap: `c701ff8f8cd3ee58f7dfab529e092d1f5e18190b` (push aceptado sin force sobre el reclamo `21af80e8`).
- `RELEASE` enviado a la sesión de I-52 tras ese push (§3). `origin/main` seguía en `95690c28…` (no avanzó por G0).
- Este registro se publica en un commit de seguimiento solo documental.

## 8. Métricas, conformidad, Owner Validation y tag

Métricas: UNKNOWN (sin gates funcionales). Conformidad: no aplica todavía. Owner Validation: no aplica a G0. Tag de integración: no existe
(`integration/I-61` se crea solo tras la integración).

## 9. G1 — Discovery (evidencia de este gate)

Resultado: [I-61-discovery.md](../../initiatives/I-61-discovery.md). **No** es Proposal, Freeze, revisión del Architect ni G1 GATE PASS.

| Hecho | Valor |
|---|---|
| Decisión del Coordinator | G0 = GATE PASS sobre `6f1ef9815ffa20f8bea946c55622efa165994f7c`; G1 autorizado ([decisiones](../decisions/I-61.md) §7) |
| Preflight delta | `HEAD` = `origin/I-61` = `6f1ef981`; `origin/main` = `95690c28` (sin cambio); tip de I-52 = `65e465a7` (sin cambio); árbol limpio; sin operaciones Git; I-52 no toca documentos de proceso |
| Transportes (re-medidos 2026-09-30) | `codex` 0.159.2 instalado y con sesión «ChatGPT», no en `PATH`; `claude` 2.1.270 instalado, **`loggedIn:false`**, no en `PATH`; **ningún worker invocado** |
| Documentación oficial | Anthropic (modelos, effort, prompting, `claude -p`) y OpenAI/Codex (modelos, `codex exec`, mejores prácticas), consultadas 2026-09-30; fuentes y alcance en Discovery §13 |
| Escrito | `docs/initiatives/I-61-discovery.md`; contrato, decisiones, estado y esta evidencia. Nada fuera de `docs/` ni de los archivos propios de I-61 |

**Comprobaciones existentes ejecutadas (caracterización; no son evidencia de Candidato ni de un gate funcional).** SDK de usuario `dotnet` 8.0.423 sobre el árbol de `6f1ef981`, sin cambios de código:

| Comando (resumido) | Resultado |
|---|---|
| `dotnet test tests/RackCad.UI.Tests --filter FullyQualifiedName~FlowBedEditorWindowTests` | 4 superadas / 0 fallos / 0 omitidas (selección > 0) |
| `dotnet test tests/RackCad.Tests --filter I60RackLogicalNameTests \| I60RackNameWiringGuardTests \| CustomPropertiesEnvelopeGuardTests \| LegacyViewPayloadCompositionCharacterizationTests` | 87 superadas / 0 fallos / 0 omitidas (selección > 0) |

No se ejecutaron Core/UI Full, builds del Plugin ni AutoCAD (no hay cambio de producto). Los resultados confirman que las pruebas que protegen el nombre en la **ventana** de la cama pasan; **no** reproducen ni descartan el defecto del piloto, que no
está cubierto en la capa Plugin (Discovery §10).

**Estado canónico.** `docs/automation/state/I-61.yml` mantiene `last_evidence_commit` en `6f1ef981` (el último commit publicado que respalda la fase anterior); el commit que publica G1 no puede autorreferenciarse. La CI exacta del SHA de G1 es un hecho
posterior al commit y se entrega en el informe al Coordinator, no en este archivo (orden de G1: no encadenar commits solo para registrarla).

## 10. G1-C — sondas de transporte (tráfico transitorio en `artifacts/orchestration/I-61/g1-transport-probe/<n>/`, ignorado por Git)

Invocaciones reales de `codex exec` (`codex-cli 0.159.2`, `%LOCALAPPDATA%\OpenAI\Codex\bin\c6fe824d725f02d7\codex.exe`), autenticado por la sesión ChatGPT existente,
modelo **solicitado** `gpt-6-luna` con `model_reasoning_effort="low"`, `--ephemeral`, un tope de 300 s por invocación. El modelo **efectivo** no consta en los eventos (UNKNOWN).
Configuración heredada: `[windows] sandbox = "unelevated"`, `service_tier = "priority"`. **Corregido en G1-C: la sonda P2 SÍ modificó la configuración global** (DEV-G1C-01, abajo).

| Sonda | Sandbox | exit | thread_id | Tokens (entrada / en caché / salida) | SHA-256 de `last.json` | Resultado verificado contra Git |
|---|---|---:|---|---|---|---|
| P1-a1 | read-only (stdin abierto) | 124 | — | — | — | colgado esperando stdin; sin salida |
| P1-a2 | read-only | 0 | 01a0f48f-7c80-7943-86db-1a53d40cbf8e | 45846 / 22272 / 263 | 5d2034b9bee2ab4281968323e835bb22bfc47434afa0a10698e4e69ef84aef90 | JSON conforme **vacío**: ningún comando se ejecutó (pwsh de WindowsApps) |
| P1-a3 | read-only | 0 | 01a0f490-e7f8-7fd0-8516-7771e430bd66 | 66996 / 42496 / 322 | 8753083bae9957c9cc5fdd178f04047717987524a5c9f2775f03b4b8a47ff26c | igual que a2 (prefijo de PATH mal formado) |
| P1-a4 | read-only | 0 | 01a0f491-9ce7-7753-881b-745142beeb73 | 44330 / 42496 / 288 | aa1c118c7afc9a24086231c30b4c5a041fd8714418cc3cde9a1adf02603bcf5d | rama, HEAD `9228bf30` y 36 estados **correctos**; escritura **denegada** |
| P2 | workspace-write (réplica en `%TEMP%`) | 0 | 01a0f492-e3a6-7870-ab6a-1eca84495a77 | 114319 / 93696 / 674 | 580d6deff193852e272b2279f569e403358c48dae36e80e0e5d551d28c434cac | 4 rechazos del sandbox al crear procesos (incluido `dotnet --version`); commit fallido en cadena; push inválido por remoto relativo mal montado (reclasificado en G1-C) |
| P3 | read-only | 0 | 01a0f493-aefb-7d61-b943-34631d6e0e07 | 44568 / 21248 / 472 | 4542d3f4593eab0f2d60153a09482a0902863a93a4c0ef1697c1440ffce5351e | red y `gh` fallan; `dotnet` 8.0.423 y `git diff` funcionan |

Uso total de las sondas: 6 invocaciones; 5 informaron uso (P1-a1 terminó por el tope sin eventos); los tokens figuran por fila. Coste monetario: **UNKNOWN** (consumo de la cuota de la suscripción
ChatGPT existente; no se compró ni activó nada).

Condiciones de invocación necesarias, medidas: stdin cerrado (`< /dev/null`) y `/c/Users/alejandra-mendoza/.cache/codex-runtimes/codex-primary-runtime/dependencies/native/powershell`
**antepuesto al PATH solo del proceso hijo**. No se cambió el PATH global ni la autenticación. La configuración global **sí** cambió durante P2 (DEV-G1C-01).

**Invocación y entorno exactos de las sondas (añadido en G1-C, FA-11).** Binario `%LOCALAPPDATA%\OpenAI\Codex\bin\c6fe824d725f02d7\codex.exe`. Línea común:
`codex exec -C <dir> -s <sandbox> --ephemeral -m gpt-6-luna -c 'model_reasoning_effort="low"' --output-schema <dir>/schema.json -o <dir>/last.json --json "<prompt>"`,
con salida en `events.jsonl` y `stderr.txt`, lanzada desde Git Bash con `timeout 300`.

| Sonda | Directorio | stdin | PATH del proceso hijo |
|---|---|---|---|
| P1-a1 | `artifacts/orchestration/I-61/g1-transport-probe/1` | heredado (abierto) | heredado |
| P1-a2 | `…/2` | `< /dev/null` | heredado |
| P1-a3 | `…/3` | `< /dev/null` | prefijo en forma Windows (`C:\Users\…\powershell`), mal formado en la lista POSIX |
| P1-a4 | `…/4` | `< /dev/null` | prefijo POSIX `/c/Users/alejandra-mendoza/.cache/codex-runtimes/codex-primary-runtime/dependencies/native/powershell` |
| P2 | `%TEMP%\i61-g1\p2\repo\.claude\worktrees\wt` (salida en `%TEMP%\i61-g1\p2\out`) | `< /dev/null` | prefijo POSIX |
| P3 | `…/5` | `< /dev/null` | prefijo POSIX |

**DEV-G1C-01 (desviación, detalle):** `%USERPROFILE%\.codex\config.toml`, líneas 114-115 añadidas durante P2 (mtime 2026-09-30 17:07:42.341 local):
`[projects.'c:\users\alejandra-mendoza\appdata\local\temp\i61-g1\p2\repo']` / `trust_level = "trusted"`. SHA-256 del archivo después del cambio:
`89F375C6EC2CF3E599073E3010AC4BD67C43F11E4EBF3FFF34FB076D60DA5281`. No se revirtió (decisión del Owner pendiente, [decisiones](../decisions/I-61.md) §11).

## 11. G1-C — otros hechos medidos

- **Preflight delta (G1-C):** `origin/main` = `95690c28` (sin cambio); rama de I-61 = `9228bf30` al empezar; I-52 avanzó a `0e73a1f960f67557eec3fea06b64a09e1a2aea80` y, al cierre de G1-C, a `c5e97b8e14c89417620801d0fa31f0a8a7c4511a`; sigue cruzándose
  solo en `docs/ROADMAP.md`, `docs/HANDOFF.md` y `docs/adr/README.md` (ningún documento de proceso ni archivo del piloto; `HANDOFF.md` añadido en G1-C).
- **Modelo efectivo de los subagentes de la sesión:** las transcripciones del workflow de investigación `wf_ede5e4fb-ea0` (11 agentes, solo lectura) registran `"model":"claude-opus-5-5"` y
  `"effort":"xhigh"` en los mensajes del asistente (transcripciones `agent-*.jsonl` del directorio del workflow en el perfil del usuario; no versionadas).
- **Workflows de apoyo de G1-C** (herramientas de esta sesión, no revisores independientes): `wf_ede5e4fb-ea0` (investigación, 11 agentes), `wf_459a09da-c13` (ayudas de revisión, ronda 1, 3 agentes) y `wf_88af0461-b57` (ronda 2, 3 agentes).
- **Registro de sesión de Codex:** un registro existente (`~/.codex/sessions/2026/09/12/rollout-…-01a096de-….jsonl`) contiene `session_meta.model` y `turn_context.model`/`effort`, lo que
  permite confirmar el modelo efectivo si no se usa `--ephemeral` (MEASURED por lectura; sin invocar).
- **Viabilidad del build Debug del Plugin (no es evidencia de Candidato):** `dotnet build src/RackCad.Plugin/RackCad.Plugin.csproj -c Debug` sobre el árbol de `9228bf30` con SDK 8.0.423:
  0 errores y 2 advertencias `MSB3277` conocidas; AutoCAD 2025 instalado y no en ejecución.

**Anclaje de los hechos «MEASURED (subagente)» (añadido en G1-C, CR-10).** Los subagentes del workflow `wf_ede5e4fb-ea0` se invocaron **sin** modelo ni effort solicitados (los heredan de la sesión); sus transcripciones registran `claude-opus-5-5` y `xhigh`. Viven en el perfil del usuario y no se versionan (`~/.claude/projects/D--Documentos-Codex-Calculadora-de-racks/dc52ceab-4024-47b6-8c86-32124f91a665/subagents/workflows/wf_ede5e4fb-ea0/`). SHA-256 de cada archivo:

```text
489c1fba020077ad6370a74183b959fe0960a0c854ade2018380cb09b3a74b11  journal.jsonl
d3d945f8d193d5d4aa07c142e403a94e6df740f25962ce2c35818f2f9776bcd6  agent-a0e0a7061354dc1cb.jsonl
2f3af369d35ab0635c2977ffd8506a4a50946a33bb986cb895b8fecda078d8b2  agent-a0f3c962d8a95e71e.jsonl
fd83089e3a5d1649b41a1f71becd98c1ff94428cfeed45aba5d202812e33f727  agent-a1dfc056a97da084e.jsonl
276bc705b947e8efdea4d7fd863a435c5d0e48f29bbe30c37cd2291c8e74f45e  agent-a2e0c250810ce4d7d.jsonl
636ffcb6cfb8a75757bd26431bc9edb376270db1093a0dda8a7672bebd986a2f  agent-a49e81727c6197c3e.jsonl
168e4c78d07d330876b18940774b159ec287a2e341f22650118d5499c0be9c3a  agent-a719e756e92f7eab0.jsonl
011c4b884340bff999e45f1bab46a6c5bbf219b386ab1f31c9a6efeedcbe6075  agent-a731242cb386580ff.jsonl
7f22f33672c1ff9e519935a2a38049d2218198331a1407c019a00abf1dfae89f  agent-aa87de9d627ac8060.jsonl
509506803cd11c1a0b13c5efa83815e945d48425dd496a3a594c0c54cdb1054c  agent-aac585b52114e00fd.jsonl
6c460c0a318076c8905d13668023398af25525293c7bb8bf30b30feb84bce499  agent-ac3937b0e4d9f58be.jsonl
c24a958bd639b56e8e5e665a13ae9183d1294dbc71305782d8604af23a60a053  agent-ac9b4007d1c6a0219.jsonl
```

## 12. Diseño — fuentes oficiales consultadas (2026-09-30)

Consultadas por la sesión de I-61 con una herramienta de recuperación web que **puede resumir**. Las citas son las que la herramienta devolvió como literales; no se verificó el texto
íntegro de la página. Es guía del proveedor, no autoridad de RackCad.

| Fuente | Proveedor | Lo que devolvió |
|---|---|---|
| `https://learn.chatgpt.com/docs/config-file/config-reference` (redirección 308 desde `https://developers.openai.com/codex/config-reference`) | OpenAI (Codex) | `service_tier`: «Preferred service tier for new turns. Use `fast` or another tier advertised by the active model; `fast` maps to the request value `priority`.» Sin datos sobre límites de uso, créditos ni coste. `model_reasoning_effort`: «Reasoning effort advertised by the selected model, such as `low`, `medium`, `high`, `xhigh`, `max`, or `ultra`. Available levels depend on the model and client.» |
| `https://learn.chatgpt.com/docs/models` | OpenAI (Codex) | Menciona los modos «Fast» y «Ultrafast» sin efecto declarado sobre límites o créditos. GPT-6.1 Sol: effort de Light a Ultra («Max and Ultra depend on your settings»); GPT-6 Luna: hasta Max, sin Ultra. Para `gpt-6-astra` lista las filas de disponibilidad «ChatGPT Credits» y «API Access», sin más detalle. Sin correspondencia explícita entre los rótulos (Light…Ultra) y los valores de configuración |

**Autenticación de la sesión responsable y de sus subagentes (MEASURED el 2026-09-30, sin registrar valores secretos ni identificadores de cuenta):**

- Variables del proceso de la sesión: `CLAUDE_CODE_ENTRYPOINT = claude-desktop`; `ANTHROPIC_BASE_URL = https://api.anthropic.com`; `CLAUDE_CODE_OAUTH_SCOPES` presente con los alcances
  `user:inference user:file_upload user:profile user:sessions:claude_code user:plugins`; existen variables de identificador de cuenta y de organización (valores no registrados).
- **Ausentes:** `ANTHROPIC_API_KEY`, `ANTHROPIC_AUTH_TOKEN`, `CLAUDE_CODE_OAUTH_TOKEN`, `CLAUDE_CODE_USE_BEDROCK`, `CLAUDE_CODE_USE_VERTEX` y `CLAUDE_CODE_USE_FOUNDRY`.
- Fuente oficial (`https://code.claude.com/docs/en/authentication`, consultada el 2026-09-30 con la misma herramienta que puede resumir): «Claude Desktop and cloud sessions do not call
  `apiKeyHelper` or read these environment variables: they use OAuth, except desktop sessions running a third-party inference configuration»; en la precedencia de credenciales, «Subscription
  OAuth credentials from `/login`. This is the default for Claude Pro, Max, Team, and Enterprise users».
- Conclusión: la sesión se autentica con OAuth de una cuenta de claude.ai (canal de suscripción), sin clave de API de pago ni proveedor de nube ni configuración de inferencia de terceros
  (la URL base es la de Anthropic). Los subagentes corren en el proceso de la sesión, que no tiene otra fuente de credenciales. El **tipo de plan** (Pro, Max, Team o Enterprise) es
  **UNKNOWN**.

## 13. Diseño — CI de G1-C, rondas de revisión y hechos medidos

**CI del commit que publica G1-C** (condición del GATE PASS de G1, decisiones §12): corrida `36796667573`; `event` = `push`; `ref` = `refs/heads/architecture/protocolo-ejecucion-agentes`;
`head_sha` = `07ef2a858616b12e5f72d2b5f9c7b3da434e63d8`; conclusión `success`; jobs `Tests (Domain + Application)`, `UI Tests (WPF controls, net8.0-windows)`,
`Build UI (WPF, valida API de Application)` y `Build Plugin without AutoCAD`, todos en `success` (consulta con `gh run view`, 2026-09-30).

**Rondas de diseño** (ayudas del Architect de esta sesión, SAME-SESSION ROLE; no independientes). Los `journal.jsonl` viven en el perfil del usuario y no se versionan; su SHA-256 es
procedencia no reverificable tras una limpieza.

| Ronda | Workflow | Objeto (blob) | Veredicto | SHA-256 de `journal.jsonl` |
|---|---|---|---|---|
| r1 | `wf_0d41af0e-fe9` | Proposal V1 `9dfa957b`, ADR `1feca318` | CHANGES REQUIRED | `f4d013bdea3f4633a3a3e8efdf6a72e60625629af49b16e649ad18fe9811dc30` |
| r2 | `wf_465b000c-68c` | V2 `229ca694`, ADR `6d18c8ba` | CHANGES REQUIRED | `ceeb6a134f6f8d7d9024aef7b4bef4d5a6a0858f0fce27316b0f586831edeb9c` |
| r3 | `wf_a0761336-ce0` | V3 `56972b06`, ADR `8403bf98` | CHANGES REQUIRED | `d0f3c13cf78083b1dfd39159096706133b4dd7bfe4623ef163a9a5c13a874cc1` |
| r4 | `wf_69f6c12a-18f` | V4 `c0359f8f`, ADR `e9a70b32` | CHANGES REQUIRED (A y B AGREED) | `b88d99609060ee8fbcd0764a5e8dbccc57c167360115718ffc2a3874e00c06f1` |
| r5 | `wf_ebced569-226` | V5 `861e7cbb`, ADR `cda4491b` | CHANGES REQUIRED | `f11104f7e26ed3f2490055807e3c94d51f51e161d02280f5afcd69bd6373db3a` |
| r6 | `wf_cc7ae11f-349` | V6 `422fa47c`, ADR `d6602de6` | CHANGES REQUIRED | `1ed643c6a75919d04092dbbe514482af8cbcb3c255dd363305dde632ea8e10e7` |
| r7 | `wf_e913851b-59e` | V7 `2bbb07fa`, ADR `c7a6f22c` | CHANGES REQUIRED | `8116d91fe2c0c15ee9cf8107f9cbba1f90a8a7af157a6bf4873435924cb23d01` |
| r8 | `wf_972c4bea-38d` | V8 `8199f211`, ADR `3c7a4ead` | **AGREED** (cuatro lentes, cero REQUIRED) | `f9ee1a619c43a530bfd536dfb0a0c732c802983b3bfc8c37db2d374ef06dcec3` |
| conf. 1 | `wf_6fa0ed72-d8c` | V9 `1cc9bf12` | CHANGES REQUIRED (N-R9-01) | `44f44148da8692206afd18f7d0583af4f00b4221f54996908c6a7774d5944bfa` |
| conf. 2 | `wf_d2995aa2-fa9` | V9 **`fccae56d`**, ADR `154e067d` | **AGREED** | `38b4ce9efded05747dff23e5f636f064c43cb13468d424dc316114b7f92797eb` |

**Modelo y effort de los subagentes revisores (MEASURED en sus transcripciones `agent-*.jsonl`):** todos registran `"model":"claude-opus-5-5"`, heredado porque no se solicitó modelo.
En r1 no se solicitó effort y registran `"effort":"xhigh"`, el de la sesión; desde r2 se solicitó `effort: 'high'` y registran `"effort":"high"`. Es evidencia **parcial** para U-04:
en subagentes de **solo lectura** el effort solicitado se aplica. Siguen sin medir la petición de un modelo distinto y la capacidad `write-commit-push` con modelo y effort
solicitados.

**EXP-07 reverificada** (Proposal V9 §17): el 2026-09-30, tras `git fetch`, `origin/main` = `95690c28`; entre las ramas remotas con commits por delante de `main`, solo
`feature/rackmirror-espejo-semantico` (I-52) toca alguna de las rutas compartidas consideradas, y solo `docs/adr/README.md`.

## 14. G2 — Protocolo materializado

### 14.1 RED → GREEN de `AgentExecutionProtocolTests` (OBL-01..06 y OBL-11 estructural)

| Fase | Árbol | Orden | Resultado |
|---|---|---|---|
| RED | sucio sobre `e9466a24` (prueba nueva y 2 de los 5 esquemas; sin README, routing, catálogo, guía ni §G) | `dotnet test tests/RackCad.Tests/RackCad.Tests.csproj --filter "FullyQualifiedName~AgentExecutionProtocolTests"` | 17 seleccionadas, 15 fallidas por aserción (artefacto ausente o §G ausente), 2 superadas (`I61_OBL02_TheStrictnessOracleDetectsMutationsInNestedObjects`, que solo usa el esquema de delegación ya presente, e `I61_OBL06_WitnessPhrasesStillExistInTheirSources`, que solo lee las fuentes de las frases testigo) |
| GREEN | sucio antes de `4da82667` | la misma | 17/17 |
| GREEN tras la revisión de G2 | sucio sobre `4da82667` (correcciones de la revisión) | la misma | 17/17; incluye las mutaciones nuevas (perfil de exactamente 26 líneas rechazado y de 25 aceptado; objeto sin `type`; `$defs`; nombre de familia de modelo) |

Suite Core completa sobre el árbol antes de `4da82667`: 12380/12380 (referencia de trabajo, no evidencia de gate; la evidencia es el Core Full sobre el SHA de cierre).

### 14.2 Sonda PR-1 (Proposal V9 §16.2)

Orden exacta (Git Bash, desde el worktree de la unidad):

```text
PATH="/c/Users/alejandra-mendoza/.cache/codex-runtimes/codex-primary-runtime/dependencies/native/powershell:$PATH" \
timeout --kill-after=10 600 "$LOCALAPPDATA/OpenAI/Codex/bin/c6fe824d725f02d7/codex.exe" exec -C "<worktree>" -s read-only \
  -m gpt-6-luna -c 'model_reasoning_effort="high"' --output-schema docs/automation/agent-execution/schemas/delegation.schema.json \
  -o <dir del RunId>/output.json --json "$(cat <dir del RunId>/prompt.md)" < /dev/null > <dir del RunId>/events.jsonl 2> <dir del RunId>/stderr.txt
```

- **Celda:** `gpt-6-luna` × CLI de Codex × `read`/`tool-use` × effort `high` (perfil CONTROLLER_PLANNING; Balanced como Eficiente con más effort). `codex-cli 0.159.2`.
- **Identidad:** `RunId` `R20261001T024407Z-a191`; directorio transitorio `artifacts/orchestration/I-61/g2-pr1/0/R20261001T024407Z-a191/`; contrato de ensayo `g2-pr1-dryrun`.
- **Relevo de salida:** árbol limpio; `HEAD` = `ls-remote` = `4da82667`; el último commit lleva el resumen de estado; `origin/main` = `95690c28`; sin participantes ni procesos no
  atribuibles; SHA-256 de `config.toml` `89F375C6…DA5281` (el mismo desde P2) y 103 nombres de claves.
- **Cesión:** 2026-10-01T02:45:26Z → 02:47:56Z; la sesión no operó sobre el worktree.
- **Resultado:** código 0, evento terminal `turn.completed`; uso 250729 tokens de entrada (203776 en caché), 5382 de salida (2994 de razonamiento); `thread_id`
  `01a0f55a-45fa-71c3-809d-a5df76a7eb4c`.
- **Modelo y effort efectivos:** `turn_context.model` = `gpt-6-luna`, `turn_context.effort` = `high` en el registro de sesión (`session_meta.model` vacío); SHA-256 del registro
  `C9B7A99476E4C29A6D9285EF2B3F131292279C027701BDBCF5F6F45885900549`. Queda medido que **sin `--ephemeral` el registro de sesión confirma el modelo y el effort efectivos**.
- **Esquema:** `--output-schema` aceptó `delegation.schema.json` (con `$schema`, `pattern`, tipos con `null` y `enum`) y la salida valida con `Test-Json`.
- **Oráculo contra Git:** `ExpectedBranch` = rama; `BaseSha` = `HEAD` `4da82667`; `RoutingReason` empieza por `PR1: ls-files=2185` (= `git ls-files`); 13 `command_execution`
  completados, todos con código 0.
- **Relevo de entrada:** `config.toml` con el mismo hash y los mismos nombres de claves (sin P-01); sin P-06 (las seis coincidencias de «credit» en los eventos son contenido de
  archivos leídos); sin participantes vivos (snapshot repetido desde PowerShell, ver DEV-G2-01).
- **Conducta observada del Controller:** detectó que la única celda del contrato de ensayo no tiene `write-commit-push` para la clase Documentación y lo declaró como bloqueo en
  `RoutingReason`, en vez de enrutar en silencio. Lo etiquetó como P-03; según `routing.md` §4 es el caso «sin celda elegible». Es una observación, no un defecto del protocolo.
- **Catálogo:** la celda queda como medida para los perfiles CONTROLLER_* (consumo cubierto, invocación probada, effort `high`).

**Custodia** (`docs/automation/evidence/I-61-pilot/g2-pr1/R20261001T024407Z-a191/`):

| Archivo | SHA-256 transitorio (procedencia) | Blob versionado | Byte a byte |
|---|---|---|---|
| `gate-contract.json` | `321a9f65433bb35d9eb447158eda04eb2d11bf8b4b0981354059842343ade29c` | `17f44e125c14bcd66e597f8a2c1811af8d0aa726` | no: escrito con CRLF; Git normaliza a LF (SHA-256 normalizado `0a87a204…1750f7`) |
| `prompt.md` | `1154f93c4a259bb595a65b6dd7e779ad5d6a963bce16742a367e6fc0cfb5a4bf` | `8054afd4e3884c8d8586ff1ec6fc57565d4da789` | sí |
| `delegation.json` (salida `-o`) | `24b1ece8d3f388c6f93f30efd1e2c997541fd462fd348ebc28cb0271c5d36990` | `a830ab3a5e17e4376bed20f63a7cf937ef9dc097` | sí |
| `relay-record.json` | `36465aed3799f292286f21cd30feb26eaa17fc2eb6a101ad8a9f8b4279fd2628` | `6886eb9a8ec05d4224d5f745a7ef7140827b3139` | no: CRLF normalizado (SHA-256 normalizado `6345ff11…59563`) |

Los eventos (`events.jsonl`, SHA-256 `4F7D9AE5D2739E1816DB713B0E9E78619D380CC7BA54604810CBA15B4A06E9C3`) y el registro de sesión no se versionan. El registro de relevo se escribió antes de
renombrar `RemoteFacts.*.RunId` a `GhRunId` en el esquema; como su `RemoteFacts` es `null`, sigue validando contra el esquema actual.

### 14.3 Controles manuales

**OBL-11** (el relevo rechaza VERIFIED incoherente; fixtures en el scratchpad de la sesión, no versionados):

| Fixture | `Test-Json` | Regla de coherencia del relevo |
|---|---|---|
| VERIFIED con las 14 en `pass` | válido | acepta |
| VERIFIED con `Scope` en `fail` | válido | **rechaza** |
| VERIFIED con `Tests` en `not_run` | válido | **rechaza** |
| REWORK coherente (`FreeText` en `fail`) | válido | acepta |
| Par inválido (`REWORK_REQUIRED` con `STOP`) | válido | **rechaza** |
| Comprobación `Scope` omitida | **inválido** | — |

Medido con la regla del README de G2 (`4da82667`); la revisión de G2 la reforzó (VERIFIED ⇔ 14 `pass`, `FailureClass` = primera no `pass`, `Result` = `pass` ⇒ `RedPart` ≠ `fail`),
sin cambiar el resultado de esos fixtures.

**OBL-08** (tabla de escenarios del Freeze §9 frente a AUTOMATION_PLAN §16 y el README): la primera revisión de G2 dio **FAIL** (el README tenía 9 de 19 filas y §16 no tenía «Sin
reinicios»). Corregido; resultado de la repetición: ver 14.4.

**DEV-G2-01 (desviación de procedimiento).** El snapshot de procesos de la entrada de PR-1, lanzado desde Git Bash, clasificó como no atribuibles dos `bash.exe` propios creados a las
02:48:10Z, después del fin de la cesión: bajo MSYS2 la cadena de `ParentProcessId` se rompe y no se reconocen como ancestros del comprobador. Repetido desde PowerShell, con la cadena
íntegra (`pwsh` ← `cmd` ← `claude`): sin participantes ni procesos no atribuibles. El README §3.2 indica ahora que el snapshot se lanza desde PowerShell. La regla de AUTOMATION_PLAN
16.4 no cambia.

### 14.4 Revisión de G2 contra el Freeze (Coordinator, con ayudas Architect SAME-SESSION ROLE)

| Paso | Ejecución | Objeto | Resultado |
|---|---|---|---|
| Revisión de G2 | workflow `wf_b3029f47-75a` (dos lentes Architect, SAME-SESSION ROLE) | commit `4da82667` más el diff de catálogo y README | **NON_CONFORMING**: 17 REQUIRED (G2-A-01..10: reglas del Freeze ausentes o cambiadas en AUTOMATION_PLAN §16 — roles y trabajo terminado, Worker subagente y cesión, exclusión de procesos, normalización del `RebaseMap`, propiedad exclusiva, «Sin reinicios», registro de `ChainRedFiles`, actor de la señal, modos de fallo y recuperación, transporte; G2-B-01..07: tabla de escenarios del README incompleta, «Sin reinicios», fortalezas y perfiles del catálogo, cláusula normativa en el perfil CONTROLLER_VERIFICATION, mutación de perfil que no ejercía la frontera de 25 líneas, rutas operativas incompletas, §14 de evidencia inexistente) y 14 OPTIONAL. OBL-08 = FAIL |
| Correcciones | sesión responsable (Executor) | AUTOMATION_PLAN §16 reescrita con fidelidad al Freeze; README, routing.md, catálogo, esquema de relevo (`GhRunId`), §G y pruebas | aplicadas, junto con los OPTIONAL de valor (regla de coherencia en ambos sentidos, oráculo relativo de los controles, construcciones de esquema no recorridas, nombres de familia) |
| Confirmación | workflow `wf_311d2621-70d` (un agente Architect) | árbol corregido | 16 REQUIRED APPLIED y G2-B-07 PARTIAL (esta sección, que se publica en el commit de cierre); **OBL-08 = PASS** (19 filas fieles); dos REQUIRED nuevos: N-01 (fila «Resultado de Claude sin datos» renombrada) y N-02 (`Processes[]` no admitía descendientes de la sesión ni huérfanos) |
| Corrección final | sesión responsable | AUTOMATION_PLAN 16.9; esquema de relevo (clases `session-descendant` y `orphan`); README §3.2 y §8 (orden fijo de 16.9); prueba de nombres de familia de tres letras | aplicadas tal como las describe la confirmación; verificación directa de la sesión (`grep`, `Test-Json` y 17/17); OBL-11 repetido con la regla final: mismos resultados |

**Decisión del Coordinator sobre G2:** se registra después del commit de cierre, con el Core Full local sobre ese SHA y su CI (LIFECYCLE §7), en el siguiente commit sustantivo.

**Datos adicionales de la fuente oficial de modelos** (`https://learn.chatgpt.com/docs/models`, misma consulta del 2026-09-30, con la herramienta que puede resumir): para Astra,
«Our most capable model for complex work»; guía general, «start with **High** for Luna or **Light** for Astra». Respaldan las líneas de fortalezas de `gpt-6-luna` y
`gpt-6-astra` en el catálogo.

### 14.5 Coordinación

I-52 confirmó por el canal entre sesiones (2026-10-01) que no tiene ningún ADR-0046 en uso ni en preparación y que no escribe `docs/adr/README.md`.

### 14.6 Borrador de la entrada de FOUNDATIONS (LIFECYCLE §4.1; se completa tras G3 y se publica en el commit de cierre)

```text
Name: Agent Execution Protocol
Status: (borrador) — solo podrá ser STABLE con ADR-0046 aceptado o el Freeze de I-61 integrado
Authority: AUTOMATION_PLAN §16 (ejecución delegada); WORKFLOW §3 (relevo) y §10; AGENTS.md (evidencia); subordinados en docs/automation/agent-execution/.
Persistence: tráfico transitorio en artifacts/orchestration/ (ignorado por Git); custodia de JSON y MD en docs/automation/evidence/<unit>-pilot/; esquemas rackcad-*/v1 en docs/automation/agent-execution/schemas/.
Mutation contract: reglas solo en AUTOMATION_PLAN §16; esquemas por versión (/v2 con ADR o A-n); catálogo mutable con fuente y fecha, sin Freeze.
Extension point: entrada de catálogo; clase de tarea o perfil nuevo (subordinado o PROMPT_TEMPLATES §G, revisado por el Coordinator); versión nueva de esquema; regla de §16 (Freeze de I-61 §15).
Decision source: ADR-0046 (propuesto) y Freeze de I-61 (docs/initiatives/I-61-proposal-v9.md).
Protecting tests: AgentExecutionProtocolTests (OBL-01..06 y OBL-11 estructural); controles del piloto OBL-07..10 (pendientes de G3).
Known limitations: independencia parcial de la verificación con un Worker subagente; efecto de consumo del service_tier heredado UNKNOWN; Worker Codex con escritura UNKNOWN; recetas dependientes de Windows y del sandbox unelevated; nivel A, sin scripts.
Last changed by: I-61
```

## 15. G3 — piloto real

### 15.1 Sonda U-04 (Proposal V9 §12.1 paso 0)

- **Clasificación provisional** (sesión como Coordinator; no vincula al Controller): clase «Implementación de pruebas», effort de partida Balanced; dimensiones: ambigüedad Low,
  sensibilidad arquitectónica Medium, amplitud Low, uso de herramientas Medium, coste de fallo Medium, repetición Low, horizonte Low. Nivel Equilibrado: `claude-sonnet-5-5` con
  `medium`, más el escalón superior (`high`). Sin alternativa de nivel inferior: `claude-haiku-4-5-20251001` está `STALE` desde el 2026-10-01 (retiro anunciado en octubre de 2026).
- **Réplicas desechables** fuera del repositorio (scratchpad de la sesión, una por celda, con remoto local desnudo); el worktree de la unidad no se tocó.
- **Invocación:** workflow `wf_9bb26e7e-466`, dos subagentes con modelo `sonnet` y effort `medium` y `high`, en paralelo, 2026-10-01T03:18:51Z → 03:19:07Z.

| Celda | Commit en el remoto local | Oráculo contra Git | Modelo / effort efectivos (transcripción) | SHA-256 de la transcripción |
|---|---|---|---|---|
| sonnet × subagente × `write-commit-push`, effort `medium` | `4f0baae23ccfa5baa2616935702ac664022ee95a` (padre `aded5ef0…`) | archivo presente; trailer `Co-Authored-By: Claude Sonnet 5.5` como última línea; `ls-remote` = commit | `claude-sonnet-5-5` / `medium` | `513b58518327403fc8cb8f133ede4c20cb4ec5f25b112eeac2cc81d77a0046e8` |
| ídem, effort `high` | `5bc79b15db95e51d87620363985d23927c7e20a7` (padre `00f2819f…`) | ídem | `claude-sonnet-5-5` / `high` | `b424b68c3f21a07b27f8f2190c1b5c23edbd880fb66a415b113d4086d1797ccd` |

Sin avisos de límite (cero coincidencias en las transcripciones); `config.toml` sin cambios. **Resultado: U-04 positiva** para las dos celdas: un subagente con modelo y effort
solicitados escribe, hace commit con su trailer y push, y la transcripción confirma lo solicitado. Las celdas pasan al catálogo y al contrato de G3.

Custodia (`docs/automation/evidence/I-61-pilot/g3-u04/<RunId>/relay-record.json`):

| RunId | SHA-256 transitorio | Blob versionado |
|---|---|---|
| `R20261001T031851Z-0b0e` (medium) | `93989c3d9636ba6a99e477b4384d4608416cd49aa5f501c14bb6abbbe6132ea7` | `7f467d5e281ee9958fec3f0020214d2ed55c69e5` (CRLF normalizado; SHA-256 normalizado `92563610…4198f`) |
| `R20261001T031851Z-0b0f` (high) | `81ba46d3657a558877103205ff6b6524fb741543ca9581aee783df34040c4d07` | `5682df17808bae645fa30405949c57fe8712f0a5` (CRLF normalizado; SHA-256 normalizado `450e9b4f…41bc8`) |

**DEV-G3-01 (desviación de procedimiento):** los `RunId` y los registros de relevo de U-04 se asignaron y escribieron al terminar la sonda, y no se tomó snapshot de procesos de
salida. La sonda corrió fuera del worktree y la entrada no muestra participantes; no hay impacto en la integridad de la rama.

### 15.2 Contrato de gate y planificación (Proposal V9 §12.1 pasos 1-3)

**Contrato de gate** (Coordinator; `artifacts/orchestration/I-61/g3-cama-d1a/0/R20261001T032333Z-4a2d/gate-contract.json`, válido contra `gate-contract.schema.json`).

| Campo | Valor |
|---|---|
| Revisiones | `AuthorityRevision` `7b8662c5` (cierre de G2); `MainSha` `95690c28` |
| Autoridades | 17 entradas: UNIT_CHANGE (AUTOMATION_PLAN preámbulo, §2 y §16; WORKFLOW §10; PROMPT_TEMPLATES §G; README, `routing.md` y tres esquemas); EXTERNAL (AUTOMATION_PLAN §3; WORKFLOW §3 y §4; AGENTS); UNIT_DOC (Proposal V9, contrato y Discovery §19). `model-catalog.md` no se declara: es NO NORMATIVO |
| Alcance permitido | el de §12.1 paso 1 |
| Alcance prohibido | el de §12.1 paso 1 salvo «el resto de comandos del Plugin», que la sintaxis de alcance no puede expresar como «Plugin menos `RackCamaCommands.cs`»: se listan las cinco rutas de edición por nombre, más `src/RackCad.UI/` y `src/RackCad.Domain/`; `AllowedWriteScope` solo admite `RackCamaCommands.cs` dentro del Plugin |
| Invariantes | INV-01..03, INV-05, INV-10, INV-P1..P3 |
| Pruebas | P1 `FullyQualifiedName~I61EditedRackNameTests` (mínimo 4, RED); P2 `FullyQualifiedName~I61CamaEditWiringGuardTests` (mínimo 1, RED); P3 UI `FullyQualifiedName~FlowBedEditorWindowTests&Name~I61_P3` (mínimo 1, sin RED; ver DEV-G3-03) |
| Paradas | S-01..S-14, P-03, P-05 y C-01 (tocar otra ruta de edición o salir del alcance) |
| Celda elegible | `claude-sonnet-5-5|subagent` con `read`, `tool-use`, `write-commit-push` y `effort-applied`; efforts probados `medium` y `high` (U-04) |
| Otros | `RoutingEnforcement: required`; `CorrectionsAuthorized: true` |

**Relevo de salida**
- El primero (03:27:00Z) encontró siete `git.exe` con la línea de órdenes ilegible: **STOP P-02**, sin invocar.
- Causa: procesos de corta vida de las apps del host, la del Owner (`ChatGPT.exe`) y la de escritorio de Claude (`claude.exe` 27584).
- El Coordinator lo resolvió sin cambio del trabajo (`analysis.md` del `RunId`; DEV-G3-02).
- El relevo repetido (03:30:18Z) quedó limpio: `HEAD` = `ls-remote` = `b677cb57`, cuyo cuerpo lleva el resumen de estado; árbol limpio; `origin/main` = `95690c28`; `config.toml` `89F375C6…DA5281`, sin
  cambios en todo G3.

**Planificación** (Controller Codex; orden de la evidencia §14.2):

| Aspecto | Resultado |
|---|---|
| Invocación | cesión 03:31:01Z → 03:32:40Z; código 0; `turn.completed`; modelo y effort efectivos `gpt-6-luna`/`high` (registro de sesión) |
| Paquete | `delegation.json` = salida `-o`, válida contra el esquema |
| Clasificación | «Implementación transversal a capas (corta)»; dimensiones Low/Medium/Medium/High/Medium/Low/Low |
| Enrutamiento | celda `claude-sonnet-5-5|subagent`, effort semántico Deep → `high`, perfil ROUTINE_IMPLEMENTATION |
| Alcance | estrechado a los cinco archivos exactos; nombra los nuevos (`EditedRackNameResolver.cs`, `I61EditedRackNameTests.cs` y `I61CamaEditWiringGuardTests.cs`) |
| Resto | copiado del contrato sin ampliarlo |

Observación: un criterio de aceptación arrastra a OBL-P1 la restricción de las pruebas Core de §13, la mutación en memoria. Es inocua.

### 15.3 nc4 y aceptación (paso 4)

| Evaluación | RunId | A1 | A2 | A3 | A4 | A5 | A6 | A7 | A8 | Disposición |
|---|---|---|---|---|---|---|---|---|---|---|
| nc4: copia con `AllowedWriteScope` + `src/RackCad.Plugin/` | `R20261001T033347Z-261b` | pass | pass | **fail** | pass | pass | pass | pass | pass | STOP (P-03), `REJECTED_BEFORE_INVOCATION` |
| Delegación real | `R20261001T032333Z-4a2d` | pass | pass | pass | pass | pass | pass | pass | pass | aceptada |

Oráculo relativo de nc4: **cumplido**. A3 queda en `fail` y las demás coinciden con la aceptación real. nc4 se evaluó antes de la aceptación, sin cortocircuito y sin contar para
ningún tope.

### 15.4 Worker (pasos 5-6)

- **Prompt:** contrato base §G.1, perfil ROUTINE_IMPLEMENTATION y delta; 124 líneas; SHA-256 `85A81BA8…DDE4A1A1`.
  - El harness de workflows antepone una línea de marco e indenta cada línea dos espacios.
  - Quitados ambos, el texto entregado tiene el mismo SHA-256: no hay divergencia entre invocación y paquete (sin P-05).
- **Invocación:** workflow `wf_306d36ac-e20`, un subagente con modelo `sonnet` y effort `high`.
  - Cesión 03:38:18Z → 03:51:53Z; duración 763608 ms, dentro del tope de 60 min; 26 llamadas de herramienta; 111631 tokens.
  - Modelo y effort efectivos en las 47 entradas de la transcripción: `claude-sonnet-5-5`/`high`.
- **Commits:**
  - RED `31211b41530f90d9410328992e0535d8be742b7a`: esqueleto con la semántica actual, OBL-P1 y la guarda OBL-P2.
  - GREEN `b8dfc900c25196b9757c2030548a226cc87516f9`: función real, cableado de `EditCama` y OBL-P3, con el resumen de estado.
  - Ambos llevan `Co-Authored-By: Claude Sonnet 5.5 <noreply@anthropic.com>` y se publicaron con push.
- **Diff** `b677cb57..b8dfc900`: `EditedRackNameResolver.cs` (nuevo), `RackCamaCommands.cs` (+2/−2 en `EditCama`), dos pruebas Core nuevas y una prueba STA. Las otras cinco rutas no
  cambian.

**Corridas locales del Worker** (entrega; TRX en el área transitoria):

| RunRef | Fase | Selección | Superadas | Fallidas |
|---|---|---|---|---|
| `red-p1` | RED | 6 | 2 | 4 (por aserción) |
| `red-p2` | RED | 10 | 6 | 4 (por aserción) |
| `green-p1` / `green-p2` / `green-p3` | GREEN sobre `b8dfc900` | 6 / 10 / 1 | 6 / 10 / 1 | 0 |
| `relevant-ui` (`FlowBedEditorWindowTests`) | RELEVANT | 5 | 5 | 0 |
| `relevant-core` (suite Core completa) | RELEVANT sobre `b8dfc900` | 12396 | 12396 | 0 |
| `mut-M1` / `mut-M2` / `mut-M3` | MUTATION (payload / `SyncName` / argumentos invertidos) | 10 cada una | 8 | 2 cada una (la guarda las detecta) |

Build Debug del Plugin sobre `b8dfc900`: 0 errores y 2 advertencias MSB3277; AutoCAD no estaba abierto.

**Relevo de entrada**
- `HEAD` = `ls-remote` = `b8dfc900`, árbol limpio y `config.toml` sin cambio.
- Ningún descendiente nuevo de la sesión: solo el `conhost.exe` persistente.
- Un `bash.exe` huérfano de otra sesión (`runinj.sh`): **STOP P-02**, resuelto por el Coordinator (`analysis.md` del `RunId`; DEV-G3-02).
- Señal `Denials`: 0 denegaciones; una orden fallida (heredoc), superada después.

### 15.5 Hechos remotos (paso 7)

| Corrida | SHA | event / ref | Jobs | TRX Core | TRX UI |
|---|---|---|---|---|---|
| 36811668907 (`RedRun`) | `31211b41` | push / `refs/heads/architecture/protocolo-ejecucion-agentes` | Tests (Domain + Application) **failure**; UI Tests y Build UI success; Build Plugin skipped | 12396 seleccionadas, 12388 superadas, 8 fallidas = las 4 + 4 pruebas `ExpectRed` | 1653, 1636 superadas, 17 omitidas |
| 36812102463 (`CurrentRun`) | `b8dfc900` | push / ídem | **los cuatro jobs requeridos en success** | 12396/12396 | 1654, 1637 superadas, 17 omitidas; P3 seleccionada 1, superada |

Las 17 omitidas son los 17 atributos `Skip` de `tests/RackCad.UI.Tests`, ya presentes en el cierre de G2. Su corrida 36809531027 da la misma forma.

### 15.6 Verificación (paso 8)

| # | RunId | Resultado | Tratamiento |
|---|---|---|---|
| 1 | `R20261001T035734Z-74c2` | válida y coherente; `EXECUTION_BLOCKED/STOP`, S-04: el TRX de UI dice `notExecuted=0` con 17 `NotExecuted` | STOP → `analysis.md`: es la convención del logger para pruebas omitidas, no contradicción. Resuelto sin cambio del trabajo; hecho añadido al registro; reverificación como reejecución 1 de 2 (DEV-G3-04) |
| 2 | `R20261001T040500Z-d387` | válida, 14 en `pass`, pero `EXECUTION_BLOCKED/STOP` (S-04: `Exit.HeadSha` ≠ `Entry.HeadSha`) | **INVALID_OUTPUT**: la regla de coherencia del README §8 la rechaza (VERIFIED ⇔ 14 `pass`). `analysis.md`: delta reescrito con dos aclaraciones factuales; reejecución 2 de 2 |
| 3 | `R20261001T041132Z-17e8` | **`EXECUTION_VERIFIED/NONE`**, `VerifiedSha` = `b8dfc900`; 14 en `pass`; `Ci` y `Tests` con `RedPart` = `pass` | RED acreditado; `ChainRedFiles` = `tests/RackCad.Tests/I61CamaEditWiringGuardTests.cs` y `tests/RackCad.Tests/I61EditedRackNameTests.cs`; delegación cerrada |

- **OBL-02:** `--output-schema` con `controller-verification.schema.json` se aceptó en la primera verificación. Las siete salidas validan con `Test-Json`.
- **OBL-11:** la regla de coherencia del relevo rechazó la salida 2 en la práctica.
- **Control manual del Coordinator** sobre el texto libre de la verificación 3 (términos de 16.10): 0 coincidencias.

### 15.7 Controles negativos nc1-nc3 (paso 9)

Los tres usan el mismo prompt que la verificación 3 (plantilla v2; solo cambian la ruta de entrada y el `RunId`) y copias de sus entradas, idénticas antes de mutar.

| Control | RunId | Mutación | Salida | Oráculo relativo |
|---|---|---|---|---|
| nc1 | `R20261001T041803Z-0ba8` | entrega con `CurrentSha` `deadbeef…beef`, inexistente | `EXECUTION_BLOCKED/STOP`, `FailureClass` `Identity` | **cumplido**: `Identity` en `fail`; las 4 anteriores, `pass` como en la real |
| nc2 | `R20261001T042312Z-78f2` | delegación sin `src/RackCad.Application/Persistence/EditedRackNameResolver.cs`, primero del diff en orden lexicográfico y sin prefijo que lo cubra | `EXECUTION_BLOCKED/STOP`, `FailureClass` `Scope` (C-01) | **cumplido**: `Scope` en `fail`; las 6 anteriores, `pass` |
| nc3 | `R20261001T042801Z-350e` | `WorkCompleted` + «GATE PASS.» | `EXECUTION_REWORK_REQUIRED/REWORK`, `FailureClass` `FreeText` | **cumplido**: `FreeText` en `fail`; las 12 anteriores y la posterior (`Denials`), como en la real |

Con nc4, **OBL-07** (nc1 y nc2) y **OBL-10** (nc4, A3-A5 y `Contract`) quedan cumplidas. Ningún control consumió `attempts` ni necesitó reejecución.

### 15.8 Mutación reproducida por la sesión (paso 10)

Como Executor, sobre `b8dfc900` con el árbol limpio, la sesión reprodujo M2: solo el sitio de `SyncName` vuelve a `window.RackName`.
- `FullyQualifiedName~I61CamaEditWiringGuardTests`: 10 seleccionadas, 8 superadas y 2 fallidas (`EditCama_UsesTheResolvedName_InPayloadAndSyncName` y
  `RealText_SyncNameSiteBackToWindowRackName_IsDetected`), igual que `mut-M2` del Worker.
- TRX SHA-256 `8581AA76…A9AAC5D95D`.
- Revertido con `git checkout --`: diff vacío y árbol limpio.

### 15.9 OBL-09: relevo y cesión

| Invocación | `config.toml` | Participantes ajenos / no atribuibles | Delegaciones abiertas | La sesión operó en la cesión |
|---|---|---|---|---|
| Planificación | sin cambio | ninguno tras el STOP P-02 de la salida 1 (resuelto) | 0 | no |
| Worker | sin cambio | ninguno; huérfano de otra sesión en la entrada (STOP P-02, resuelto) | 1 | no |
| Verificaciones 1-3 y nc1-nc3 | sin cambio | ninguno | 1, y después 0 | no |

Ningún P-01, P-05, P-06 ni P-08. Los dos P-02 fueron falsos positivos de la medición, con causa raíz y decisión registradas (DEV-G3-02). **OBL-09: cumplido.**

### 15.10 OBL-08: escenarios aplicados al registro del piloto

| Escenario (README §9) | Registro del piloto | Conforme |
|---|---|---|
| Primera delegación de una tarea | `Attempt` = `attempts` = 0; sin incremento; RED exigido (`ChainRedSha` `null`) y acreditado | sí |
| BLOCKED o fallo de transporte | reverificaciones con `RunId` nuevo: 2 de 2 en (g3-cama-d1a, VERIFICATION); sin incremento; no se alcanzó P-04 | sí |
| STOP resuelto por el Coordinator **sin** cambio del trabajo | tres casos (dos P-02 y un S-04); sin incremento de `attempts` (16.8: ningún STOP lo consume por sí mismo) | sí; camino de reverificación interpretado (DEV-G3-04) |
| Control negativo | rutas `-ncN` y registros propios, sin incremento | sí |
| Tope de invocaciones | G3: 7 invocaciones Codex (planificación, 3 verificaciones y nc1-nc3) = 5 + 2 reejecuciones de §9; ninguna se lanzó por encima del tope | sí |
| Cambio de modelo, rol o sesión | sin reinicio; `attempts` sigue en 0 | sí |

### 15.11 Métricas del piloto (F-06; datos del piloto, no métrica de LIFECYCLE §11)

| Fase | RunId | Modelo/effort efectivos | Duración | Tokens entrada | En caché | Salida | Razonamiento | `command_execution` | Resultado |
|---|---|---|---|---|---|---|---|---|---|
| Planificación | `R20261001T032333Z-4a2d` | `gpt-6-luna`/`high` | 99 s | 187582 | 136192 | 6220 | 2672 | 4 | delegación aceptada |
| Verificación 1 | `R20261001T035734Z-74c2` | `gpt-6-luna`/`high` | 279 s | 627058 | 530176 | 17470 | 11839 | 9 | BLOCKED/STOP (S-04) |
| Verificación 2 | `R20261001T040500Z-d387` | `gpt-6-luna`/`high` | 287 s | 609392 | 526592 | 14675 | 7993 | 11 | INVALID_OUTPUT |
| Verificación 3 | `R20261001T041132Z-17e8` | `gpt-6-luna`/`high` | 296 s | 967852 | 890880 | 15118 | 8940 | 17 | VERIFIED |
| nc1 | `R20261001T041803Z-0ba8` | `gpt-6-luna`/`high` | 251 s | 721957 | 630784 | 13494 | 7274 | 12 | BLOCKED/STOP (`Identity`) |
| nc2 | `R20261001T042312Z-78f2` | `gpt-6-luna`/`high` | 244 s | 663740 | 564736 | 14338 | 9075 | 10 | BLOCKED/STOP (`Scope`) |
| nc3 | `R20261001T042801Z-350e` | `gpt-6-luna`/`high` | 171 s | 377907 | 298496 | 10182 | 5710 | 6 | REWORK (`FreeText`) |
| Worker | `R20261001T033522Z-7f2d` | `claude-sonnet-5-5`/`high` | 763.6 s | — | — | — | — | 26 llamadas de herramienta | IMPLEMENTATION_COMPLETE |

- **Perfil de la tarea:** ROUTINE_IMPLEMENTATION; Executor: subagente Anthropic; PromptProfile del Worker ROUTINE_IMPLEMENTATION; ReworkLoops: 0.
- **ExecutionResult:** `EXECUTION_VERIFIED` sobre `b8dfc900`. CoordinatorResult: en la decisión de G3.
- **Consumo:** 111631 tokens del subagente. Sin avisos de límite en ninguna invocación (0 coincidencias; sin P-06).
- **Tiempo:** del contrato (03:23:33Z) al fin de nc3 (04:31:04Z) pasaron 67 min 31 s.
  - Cesiones: 27 min 7 s de Codex y 13 min 35 s del Worker.
  - Relevo, registros y resolución de los tres STOP por la sesión: unos 27 min.
  - Para los disparadores de B (§14 del Freeze), la decisión de G3 evalúa este tiempo de relevo y las tres verificaciones casi idénticas.

### 15.12 Desviaciones, hallazgos y propuestas

**DEV-G3-02 (medición del relevo).** Decisión del Coordinator en los `analysis.md` de la planificación y del Worker. Se aplica en todos los relevos de G3.

| Medición | Criterio | Efecto en G3 |
|---|---|---|
| Relectura | un proceso de la lista cerrada ilegible, un descendiente o un huérfano se vuelve a leer por PID a los 2 s; si ya no existe o cambió de `CreationDate`, no está vivo | resolvió el STOP P-02 de la salida de la planificación (procesos efímeros de las apps del host) |
| Host de consola | el `conhost.exe` cuyo padre está en la cadena del comprobador se trata como parte de esa cadena | evita marcar como descendiente nuevo la consola del propio comprobador |
| Huérfano de otra sesión (no es criterio de medición: es la resolución de un STOP) | el STOP P-02 sí se disparó; el Coordinator lo resolvió por §10.2 con causa raíz (línea legible sin la ruta del worktree, sin rastro en la transcripción del Worker) | no exime de futuros STOP de huérfano sin A-n |

La regla de AUTOMATION_PLAN 16.4 no cambia.

**DEV-G3-03 (defecto del contrato del Coordinator).** El filtro P3 `…&Name~I61_P3` selecciona 0 pruebas con el adaptador de xunit.
- El Worker usó `FullyQualifiedName~FlowBedEditorWindowTests&FullyQualifiedName~I61_P3` (1 seleccionada, superada) y lo declaró en `Deviations`.
- El Controller lo evaluó sobre el TRX con la semántica del contrato: P3 seleccionada 1 y superada.
- El defecto es del contrato, no de la entrega.

**DEV-G3-04 (camino de reverificación).**
- El Freeze no enumera qué sigue a un STOP resuelto sin cambio del trabajo cuando la clasificación fue `EXECUTION_BLOCKED`.
- El Coordinator lo trató como la recuperación de BLOCKED de 16.11: reejecución de la fase dentro del tope de 16.8, que cuenta como reejecución de §9 en el tope de invocaciones.
- La segunda reverificación siguió a una salida incoherente (INVALID_OUTPUT), con el delta del prompt reescrito y su `analysis.md`.

**Hallazgos**
- El harness de workflows enmarca e indenta el prompt del Worker. La identidad se comprueba quitando el marco.
- No hay temporizador activo del tope de 60 min del Worker (nivel A): la sesión depende de la notificación. Aquí el Worker terminó en 12 min 44 s.
- El Controller produjo dos veredictos conservadores injustificados (S-04) antes del VERIFIED. La regla mecánica de coherencia atrapó el segundo.

**Propuestas para la decisión de G3** (no bloquean; requieren A-n o una unidad posterior):
1. Aclarar el README §3.2 (relectura, host de consola y alcance del criterio de huérfano).
2. Hacer explícito en 16.11 el camino «STOP resuelto sin cambio del trabajo → reverificación como reejecución de la fase».
3. Añadir al perfil CONTROLLER_VERIFICATION, o al README §8, la semántica Exit/Entry y el bicondicional.
4. Registrar el número de pruebas omitidas en `RemoteFacts`.
5. Que el Coordinator valide cada filtro de `RequiredTests` con `dotnet test --list-tests --filter` antes de emitir el contrato.

### 15.13 Custodia de G3

Los eventos (`events.jsonl`), los registros de sesión de Codex, la transcripción del Worker y los TRX no se versionan. Sus SHA-256 y sus campos extraídos están en los registros de relevo.

| Archivo (bajo `docs/automation/evidence/I-61-pilot/`) | SHA-256 transitorio (procedencia) | Blob versionado | Byte a byte |
|---|---|---|---|
| `g3-cama-d1a/R20261001T032333Z-4a2d/gate-contract.json` | `c999bbe86814ec035cdb7103fdd23f47ae87f73195a24430028cd811cb6417ac` | `9b5ef6dfed25347310a6c2a8ecb91e48976928cc` | no: CRLF normalizado a LF (SHA-256 normalizado `16b26bf1…d18db4`) |
| `g3-cama-d1a/R20261001T032333Z-4a2d/prompt.md` | `73d24474c73b448123de2e68acfe14a0cb7565207806a6670062a9ba2616edb3` | `edcecb063d27f9cfd2d2473b28880b8692ce8e0b` | sí |
| `g3-cama-d1a/R20261001T032333Z-4a2d/delegation.json` | `af88450a84f47b08a0fa7d79b5a109211788d97095e700508eb36bf06cbc6096` | `b217e4498f42cafa006c02793fd16e513ca9b7b8` | sí |
| `g3-cama-d1a/R20261001T032333Z-4a2d/acceptance.json` | `de1c43dc0582bee72f750e9f95f6032588c06c5b6ae2f539fa76f3048d4ba55a` | `455ad35ac532f30183d5418661383cf09c63e0e6` | no: CRLF normalizado a LF (SHA-256 normalizado `2551cc12…8719d8`) |
| `g3-cama-d1a/R20261001T032333Z-4a2d/acceptance-reasons.json` | `7e47c7c12be7b12fd9834a18d0254b14563fa32631d7345f74e9676f85594ab7` | `00f95e768f7c62b2c4a536ab1814cb63487bf455` | no: CRLF normalizado a LF (SHA-256 normalizado `1d5db79f…716305`) |
| `g3-cama-d1a/R20261001T032333Z-4a2d/analysis.md` | `79b3dfe806669231ca34225feb1e9ff8adb41b6a849bb28fd2aa4cc208d50180` | `688b3a1c4af304066e4e9612e65ea64486d0a759` | sí |
| `g3-cama-d1a/R20261001T032333Z-4a2d/relay-record.json` | `86d5d1cc4f928c71b9075e1f5ea96b209454b46d5d301eee2ba36e35fa75cb5d` | `d3412280c5221071e963682d62d00a07bba56324` | no: CRLF normalizado a LF (SHA-256 normalizado `1593fc2c…897d45`) |
| `g3-cama-d1a-nc4/R20261001T033347Z-261b/delegation.json` | `90f21a85585bd4ec495309986caf41c884741446b5166406b03c9f1e90dd7b19` | `3e5fa12a016e14be9a72bcfda3833e7f86bdcfdf` | sí |
| `g3-cama-d1a-nc4/R20261001T033347Z-261b/acceptance.json` | `c1752ef404da0010828fb550d3c6f6c3ffbe797053f3e11bd87a6905e0ebb76f` | `95489ef10b3fb7901860b6347396e2cecfaa8398` | no: CRLF normalizado a LF (SHA-256 normalizado `ae290e83…957bd5`) |
| `g3-cama-d1a-nc4/R20261001T033347Z-261b/acceptance-reasons.json` | `acac3a268ca0f8be97ccb52651b6f7e075220fdaaf410d4d9a12fcba82e08410` | `9a5b838c96268fa650e3ce18a7317da1578c4063` | no: CRLF normalizado a LF (SHA-256 normalizado `89a800e9…ed2dbe`) |
| `g3-cama-d1a-nc4/R20261001T033347Z-261b/relay-record.json` | `9a875327aa8c7dec285ee270fb166c25f9c7986f890d5d4600f0ba71448e9df8` | `0520f248431dcba8be738f54bbaa552390cc1f18` | no: CRLF normalizado a LF (SHA-256 normalizado `30068c4f…2cdb3e`) |
| `g3-cama-d1a/R20261001T033522Z-7f2d/prompt.md` | `85a81ba82cc956b3c52f691c90803ab500cd782d31b8239eb5f0988bdde4a1a1` | `7b9da72fe3ebd915b660f7f09a3e91cff7a6976d` | sí |
| `g3-cama-d1a/R20261001T033522Z-7f2d/worker-handoff.json` | `7d37620952520ad1b371d6fd87281267019d783b20b5264a7cfedccfebbd55dd` | `f4696ab4c197f1b265ca3b6e6881389b463b60b8` | sí |
| `g3-cama-d1a/R20261001T033522Z-7f2d/analysis.md` | `1067d589a878503ddbf1a30a0fbb5ea17cce63cc431e6cc69d634990e4eb3f3c` | `519bc6f4c1da393f89ebc05c584a438faa606613` | sí |
| `g3-cama-d1a/R20261001T033522Z-7f2d/relay-record.json` | `f525a5583453c0c0a17273f414e6761803f4e973ff70521978475de99ea85257` | `8471e5d98f45fa9d3d4b54d4e06525f3ccea66a3` | no: CRLF normalizado a LF (SHA-256 normalizado `13981e57…d02097`) |
| `g3-cama-d1a/R20261001T035734Z-74c2/prompt.md` | `17bb409b00231e28350282071fd9baf9dc69eb1c8a91922650b92848c52bef3b` | `9650bcf21dca57ac9ef6b8760e92105c7562f848` | sí |
| `g3-cama-d1a/R20261001T035734Z-74c2/controller-verification.json` | `0fbb2359ac507e8fc0c4b8907f73887adf03a5a98a94494b87ad5f1d5effe204` | `914d0afbb95d6a7b2d94b541b487f85b7e465712` | sí |
| `g3-cama-d1a/R20261001T035734Z-74c2/analysis.md` | `a12a4e26e976ad73735c929f1b1aaaf28a7785d0e2679ce2791d26ff96f0325d` | `33ee23382a0ccf87e88925cd3e20953830b1fe1f` | sí |
| `g3-cama-d1a/R20261001T035734Z-74c2/relay-record.json` | `d2110bcac1769d4bf84026521144b90e1a8946a479543acbbfeaacd97fa9acf3` | `b0312fa2b6b3bf3ebe5a324554e3b6ad23929f26` | no: CRLF normalizado a LF (SHA-256 normalizado `4019c126…ce6736`) |
| `g3-cama-d1a/R20261001T040500Z-d387/prompt.md` | `ed5a4f6294dd017df09854ecf21f8bd0965f4fb0739044dfe64b07985e702d69` | `fd192081f772e71dd0c006b16e807c8064228944` | sí |
| `g3-cama-d1a/R20261001T040500Z-d387/controller-verification.json` | `406bae51e16b3e07c96b7461db42922237411d7ed29f882ea658f12177e7a3fc` | `204a2b6b5802888e52f01d4119be76af3126c6ec` | sí |
| `g3-cama-d1a/R20261001T040500Z-d387/analysis.md` | `a10c14fe390cc1f5bba6e0d1a9a62fcf07c03ff9cf831842199f80762ff90160` | `bf72595f761184c52047bb7a27c8ba3ebaa1581d` | sí |
| `g3-cama-d1a/R20261001T040500Z-d387/relay-record.json` | `848789de0d668ce4e629d5f2ed9044c5a65da8e5370db29462d9deffea1b8af8` | `d1e33a0bdf1fb3e197e5803b6f7723a3e0ab4437` | no: CRLF normalizado a LF (SHA-256 normalizado `526eb6db…eb02ca`) |
| `g3-cama-d1a/R20261001T041132Z-17e8/prompt.md` | `43a3113f013747a746dfed8c22e3ea79523cf35d290b89b3363ed2fae1e12f58` | `2b31365f5cc751b416ad87a1b646fa3c947cbb3a` | sí |
| `g3-cama-d1a/R20261001T041132Z-17e8/controller-verification.json` | `69e9a423b0be9b83309b03381697304cfbad73340f5052a029b4965165eaaedc` | `c84ca3d55b31c2231a15eb0465197df4ecbcd0b4` | sí |
| `g3-cama-d1a/R20261001T041132Z-17e8/relay-record.json` | `d3d27d545dedb794816641540966b54a68cefa464572765b121fa150b8ce3226` | `3e6fbe0dc7cf72091b9189a724ce6713a5f48e36` | no: CRLF normalizado a LF (SHA-256 normalizado `98117e85…739d7c`) |
| `g3-cama-d1a-nc1/R20261001T041803Z-0ba8/prompt.md` | `f42872e2dbb2c93f05715a37756eaef27ed7061cf37ba4d0329c8a391a5f0fff` | `4b6aa105768cc14feeaaeabadded1077c97a3cd6` | sí |
| `g3-cama-d1a-nc1/R20261001T041803Z-0ba8/inputs/worker-handoff.json` | `6639291d8fe9348eded329ed212be1e0f01909f12d600dc99a754de073c42695` | `5106aff20a64ef069098e3dd66230a329abd9b8b` | no: CRLF normalizado a LF (SHA-256 normalizado `9e222760…92ae6c`) |
| `g3-cama-d1a-nc1/R20261001T041803Z-0ba8/controller-verification.json` | `9105c4e2d561c197accfba34a6fbc7d6d3b9001da390e0c3518ae31fc2ff5c94` | `698332db206e4a951cbc9e03751d4ae8ec9c5701` | sí |
| `g3-cama-d1a-nc1/R20261001T041803Z-0ba8/relay-record.json` | `9c39fa65b86e72a5a18ae7e727a786622abc7895224c9f7a8901d19bd6eb34e7` | `8bbaf8092e90ec593e474c0aeae82e8feb92d5d0` | no: CRLF normalizado a LF (SHA-256 normalizado `4a36eafb…5e5c0d`) |
| `g3-cama-d1a-nc2/R20261001T042312Z-78f2/prompt.md` | `c43d2d8be8aaa5e780360c1e6ae73805d8c66c67586070e653a44d05c748ae50` | `f2d93521d4fb7b9fe5e1a7257719c89f23cc72a0` | sí |
| `g3-cama-d1a-nc2/R20261001T042312Z-78f2/inputs/delegation.json` | `6a688b7810a6dd4ce9bfaaa7e8dd50c63429cd16e932120a91d88306531d8544` | `0f35bf3b80e9e35b1c7dc88448b6a6d64fefad9e` | sí |
| `g3-cama-d1a-nc2/R20261001T042312Z-78f2/controller-verification.json` | `605fcf0c46cea4cc39e9a14a40d3d85ac51dd65188a87e860166c9e19368531d` | `e9d36cb49ae53130febba7ab26a5a6b2116530c1` | sí |
| `g3-cama-d1a-nc2/R20261001T042312Z-78f2/relay-record.json` | `ffa71cd59b4f872306cf8b29115bf624b68e8b940c589705aab13dfe808b07ef` | `9614e5dba328b6fef3766d799d23aeb92533914c` | no: CRLF normalizado a LF (SHA-256 normalizado `c5492f2b…2c29ff`) |
| `g3-cama-d1a-nc3/R20261001T042801Z-350e/prompt.md` | `50990a4a36ba9ec81c0a38c9334505e34751cdae1bd38131165c2e5fbe53dc9b` | `e79b09ae56834c56edc9ba6b086fed1cf392900d` | sí |
| `g3-cama-d1a-nc3/R20261001T042801Z-350e/inputs/worker-handoff.json` | `f215e8322bfdfb04f7590f13270a1c587b6af7a418e83403184b5f32ad1d08df` | `0889f38c131845ca1ec1010605042882ba77d451` | no: CRLF normalizado a LF (SHA-256 normalizado `15b0975c…6e06f3`) |
| `g3-cama-d1a-nc3/R20261001T042801Z-350e/controller-verification.json` | `2da7ab359512919aafdf8b758aafaccd8fbac0232047fe1cf0576fcaa951f0cd` | `498fa046e878745250cbcea97dd2ba5b103b3a87` | sí |
| `g3-cama-d1a-nc3/R20261001T042801Z-350e/relay-record.json` | `a92fe3593086755e0cafeafffebfbc49ac9667b470f5db64be608e8f7ce7522e` | `f08e14f6976e08bc0425e61dbb862d93bee865cd` | no: CRLF normalizado a LF (SHA-256 normalizado `6e8de622…60f688`) |

### 15.14 Entrada de FOUNDATIONS completada tras G3 (LIFECYCLE §4.1; se verifica en READY-06 y se publica en el commit de cierre)

```text
Name: Agent Execution Protocol
Status: STABLE al publicarse en el commit de cierre (ADR-0046 aceptado por el Owner el 2026-10-01)
Authority: AUTOMATION_PLAN §16 (ejecución delegada); WORKFLOW §3 (relevo) y §10; AGENTS.md (evidencia); subordinados en docs/automation/agent-execution/.
Persistence: tráfico transitorio en artifacts/orchestration/ (ignorado por Git); custodia de JSON y MD en docs/automation/evidence/<unit>-pilot/; esquemas rackcad-*/v1 en docs/automation/agent-execution/schemas/.
Mutation contract: reglas solo en AUTOMATION_PLAN §16; esquemas por versión (/v2 con ADR o A-n); catálogo mutable con fuente y fecha, sin Freeze.
Extension point: entrada de catálogo; clase de tarea o perfil nuevo (subordinado o PROMPT_TEMPLATES §G, revisado por el Coordinator); versión nueva de esquema; regla de §16 (Freeze de I-61 §15).
Decision source: ADR-0046 (aceptado) y Freeze de I-61 (docs/initiatives/I-61-proposal-v9.md).
Protecting tests: AgentExecutionProtocolTests (OBL-01..06 y OBL-11 estructural); I61EditedRackNameTests, I61CamaEditWiringGuardTests y FlowBedEditorWindowTests.I61_P3 (piloto, OBL-P1..P3); controles del piloto ejecutados en G3: nc1..nc4 (OBL-07, OBL-10), OBL-08 y OBL-09 por registro (evidencia §15).
Known limitations: independencia parcial de la verificación con un Worker subagente; efecto de consumo del service_tier heredado UNKNOWN; Worker Codex con escritura UNKNOWN; recetas dependientes de Windows y del sandbox unelevated; nivel A, sin scripts; medición de procesos con falsos positivos por apps del host y otras sesiones (DEV-G3-02); el Controller puede emitir paradas conservadoras injustificadas, que la regla de coherencia y el Coordinator filtran (DEV-G3-04).
Last changed by: I-61
```

## 16. Candidato final, READY, Owner Validation y cierre

### 16.1 Rondas hacia el Candidato

| Punta | READY | Resultado |
|---|---|---|
| `b69c34634fdb44513c7f993aa28c5e4e77577f33` (decisiones del Owner, decisiones §17) | READY-01..05 en verde; READY-06 **NON-CONFORMING** (falta «sin distinguir mayúsculas» del Freeze §3.3 en AUTOMATION_PLAN 16.4 y en README §3.2) | nunca se declaró Candidato; corregida en el commit siguiente, que reinició READY-02 (decisiones §18) |
| `6513777f431fd672a515d9f8a33d8611d238fbe4` | READY-01..09 satisfechos en orden (16.2) | **`FINAL_CANDIDATE_SHA`** |

### 16.2 READY-01..09 sobre `6513777f`

| READY | Evidencia |
|---|---|
| 01 | Alcance del Freeze completo (entregables §§4-8 y 11 en G2; piloto §12 en G3); sin diferimientos del Freeze y sin A-n |
| 02 | Gates G0..G3 cerrados (decisiones §§7, 12, 14 y 15); decisiones y documentos de producto versionados; la corrección de READY-06 versionada (decisiones §18) |
| 03 | Sin REQUIRED abierto ni discrepancia A; decisiones del Owner sobre ADR-0046 y DEV-G1C-01 registradas (decisiones §17) |
| 04 | `git fetch --prune`; `origin/main` = `95690c28` = merge-base, sin rebase; árbol y stash limpios; `HEAD` = upstream; `git merge-tree` con la rama activa de I-52 (`feature/rackmirror-espejo-semantico`, `b1fd5027`) sin conflicto |
| 05 | Focales Core 33/33 (`I61EditedRackNameTests`, `I61CamaEditWiringGuardTests`, `AgentExecutionProtocolTests`) y UI 5/5 (`FlowBedEditorWindowTests`, con I61_P3); CI 36819890344: `push`, `refs/heads/architecture/protocolo-ejecucion-agentes`, `head_sha` = Candidato, los cuatro jobs en `success` |
| 06 | Conformidad Architect + Coordinator **CONFORMING** sobre `6513777f`, sin REQUIRED. Modo: **SAME-SESSION ROLE** (Architect = subagente de la sesión responsable; revisor = sesión autora; **no** independiente). Repetida completa, no trasladada desde `b69c3463` |
| 07 | Árbol limpio; `HEAD` = remoto = `6513777f` |
| 08 | Matriz OV-I61-01..05 completa y asignada a I-61 (Freeze §16.3); DLL legacy de 03a y 03b preparados (16.5) |
| 09 | Acuerdo: blob `fccae56d` en `5bffe5a7`; el commit de Freeze `e9466a24` solo cambia la línea `Frozen`; blob `8cf5f9f4` idéntico en `e9466a24` y en el Candidato; ningún commit posterior toca la Proposal; sin A-n; los 16 commits de `95690c28..6513777f` llevan exactamente un `Co-Authored-By` de quien ejecutó |

### 16.3 Bloque del Candidato (guía de validación manual §7.1)

```text
Candidate SHA:         6513777f431fd672a515d9f8a33d8611d238fbe4
Base:                  95690c28dc6268e61dff32a0cbc33cc9fde3d47f (origin/main; merge-base; sin rebase)
Arbol limpio:          SI (al producir la evidencia local)
SDK resuelto:          8.0.423 (dotnet de usuario)
Core Full local:       PASS — 12396/12396
UI Full local:         PASS — 1654 seleccionadas: 1637 superadas, 17 omitidas (los Skip de base), 0 fallidas
Debug UI build:        PASS — 0 errores; RackCad.UI.dll 1.0.0+6513777f431fd672a515d9f8a33d8611d238fbe4,
                       SHA-256 4ED12E08045E129A8B0A87E7AA463DC4F882455CC1C6FDDBA94488147FA37E5D
Debug Plugin build:    PASS — 0 errores; RackCad.Plugin.dll 1.0.0+6513777f431fd672a515d9f8a33d8611d238fbe4,
                       SHA-256 1EA1565E8CB9D517CD5E3A3D911C91993A47BA2A9A1450D9CAFE3ED16B536D33
CI exact SHA:          GREEN — run 36819890344, event push, ref refs/heads/architecture/protocolo-ejecucion-agentes;
                       Tests (Domain + Application), UI Tests (WPF controls, net8.0-windows), Build UI y
                       Build Plugin without AutoCAD en success
Coverage:              run 36820312102, event workflow_dispatch, candidate_sha = HEAD del checkout = measured-sha.txt
                       = 6513777f431fd672a515d9f8a33d8611d238fbe4; artifact rackcad-coverage-cobertura id 11143381154
                       (sha256:5323ee7b2139e20722d0a8fbf93e865ff3b3800025ef7c28c7d33da71e80d1aa); line-rate 0.9087
Owner validation:      pass (16.4)
Biblioteca de bloques: D:\Base_de_datos_AutoCAD_V.0.dwg (override de %APPDATA%\RackCad\settings.json)
                       SHA-256 B4CA2248DB9C3D72487AC8B5B1E5510CDD8ABA231AB340541D91BEBCA2D560E8
                       (resuelta al cerrar; última modificación 2026-09-09, anterior a la validación)
```

### 16.4 Owner Validation (declaración del Owner)

El Owner ejecutó físicamente la validación y la entregó en la conversación de la sesión responsable el 2026-10-01. Se registra **tal como la declaró**, sin observaciones añadidas
ni evidencia reconstruida.

```text
Fecha y zona:       2026-10-01 (fecha de la declaración; la hora de ejecución no se declaró)
Validador:          Owner del repositorio
Iniciativa / rama:  I-61 / architecture/protocolo-ejecucion-agentes
Commit:             6513777f431fd672a515d9f8a33d8611d238fbe4 (FINAL_CANDIDATE_SHA declarado por el Owner)
Worktree:           D:\Documentos\Codex\Calculadora de racks\.claude\worktrees\architecture-protocolo-ejecucion-agentes
DLL entregado:      src\RackCad.Plugin\bin\Debug\net8.0-windows\RackCad.Plugin.dll del worktree,
                    ProductVersion 1.0.0+6513777f431fd672a515d9f8a33d8611d238fbe4,
                    SHA-256 1EA1565E8CB9D517CD5E3A3D911C91993A47BA2A9A1450D9CAFE3ED16B536D33
                    (la sesión comprobó antes del cierre que el archivo no se recompiló desde el build del Candidato)
Versión de AutoCAD: no declarada por el Owner; instalada en el equipo: AutoCAD 2025 (acad.exe R25.0.171.0.0)
Escenarios:         matriz OV-I61-01..05 del Freeze §16.3
Resultado:          OV-I61-01 PASS; OV-I61-02 PASS; OV-I61-03a PASS; OV-I61-03b PASS; OV-I61-04 PASS;
                    OV-I61-05 ACCEPTED / PASS
Fallos y severidad: ninguno declarado
Resultado global:   aprobado
Confirmación:       «El Owner acepta el resultado funcional y el protocolo/piloto de I-61 conforme a la matriz definida.»
```

### 16.5 DLL legacy de OV-I61-03a y 03b (Freeze §16.3)

Construidos por la sesión, como Executor, desde `git archive` en raíces cortas fuera del repositorio, con `-p:SourceRevisionId=<SHA de origen>`; la `ProductVersion` termina en el
SHA de origen en ambos casos.

| OV | SHA de origen | Ruta | ProductVersion | SHA-256 |
|---|---|---|---|---|
| 03a | `95690c28dc6268e61dff32a0cbc33cc9fde3d47f` | `%LOCALAPPDATA%\Temp\i61l-95690c28\src\RackCad.Plugin\bin\Debug\net8.0-windows\RackCad.Plugin.dll` | `1.0.0+95690c28dc6268e61dff32a0cbc33cc9fde3d47f` | `6B3479D6E2F3E9C0076B0CEDEE04296CC8E670D1C999C776EF0AE4EF3B7D601F` |
| 03b | `69daf03a35c630e453e1d9e98136f128bd0325a4` (`95690c28^1`) | `%LOCALAPPDATA%\Temp\i61l-69daf03a\src\RackCad.Plugin\bin\Debug\net8.0-windows\RackCad.Plugin.dll` | `1.0.0+69daf03a35c630e453e1d9e98136f128bd0325a4` | `F1C8139D08A72F7D5BF5FF5F1ADE1C0ABD0F87B5F2718BEC464210AC682828C1` |

### 16.6 Medición de procesos no versionada (G2 y G3)

Seguimiento de la conformidad final: los scripts del snapshot de procesos vivían en el scratchpad de la sesión y no están versionados (nivel A). Se acreditan por su SHA-256 al
cerrar y por su fecha de última modificación, anterior a su primer uso:

| Script | SHA-256 | Última modificación (UTC) | Usado en |
|---|---|---|---|
| `proc-snapshot.ps1` | `37674de0182cecb189bd0d950ae5678c1c72adca24aa28bc45b9a93218c1bc45` | 2026-10-01T02:44:47Z | PR-1 (G2) y U-04 (G3) |
| `proc-snapshot2.ps1` | `85e4f24963268accbcb20cbc1562a8ba4561a7d4cf9daf7748ede0a1e30189c6` | 2026-10-01T03:29:49Z | relevos Codex de G3 desde la salida repetida de la planificación |
| `proc-snapshot3.ps1` | `dcb8e40337082004f1ca65df569981dca596cfddd14ef32f8956299c846037d4` | 2026-10-01T03:37:49Z | relevo del Worker subagente |
| `relay-side.ps1` | `38c3f113d4479d0ec814666063118c7d2bf11b47f2576cc6477acfa4731449b2` | 2026-10-01T03:37:28Z | envoltorio de salida y entrada de los relevos de G3 |

Los tres scripts de snapshot comparan la línea de órdenes con la ruta del worktree **sin distinguir mayúsculas**: pasan ambas cadenas a minúsculas
(`$Worktree.Replace(...).ToLowerInvariant()` para la ruta con `\` y con `/`, `$lc = $cmd.ToLowerInvariant()` y
`$match = $readable -and ($lc.Contains($wtBack) -or $lc.Contains($wtFwd))`). La medición de G2 y G3 cumplió, por tanto, el Freeze §3.3, cuyo texto normativo se restauró en
READY-06.

### 16.7 Autoverificación de la sesión principal (hallazgo fuera del Freeze)

Orden del Owner del 2026-10-01: la sesión principal resuelve al empezar qué perfil, nivel y effort le corresponden y los compara con lo observable del runtime. Resolución, por
analogía con `routing.md` §§1-3 porque **no existe clase ni perfil para la sesión principal** (el hueco):

| Campo | Valor |
|---|---|
| `PRINCIPAL_SESSION_PROFILE` | PRINCIPAL_COORDINATION (propuesto): coordinación larga, decisiones de gate, interpretación de Architect, Controller y Worker, arquitectura sensible |
| `REQUIRED_CAPABILITY_LEVEL` | Frontera |
| `REQUIRED_EFFORT_CLASS` | Long-horizon (horizonte `High`; ambigüedad, sensibilidad arquitectónica y coste de fallo `High`) |
| Traducción vigente (catálogo verificado el 2026-09-30) | `claude-opus-5-5` con effort `xhigh`; `claude-fable-5-1` no es elegible (no medido) |
| `ESCALATION_CONDITIONS` | `routing.md` §4: effort → nivel → maximum, solo con evidencia registrada |

Observaciones con los metadatos de la sesión (`get_session("self")`):

| Momento (UTC) | `CURRENT_MODEL` | `CURRENT_EFFORT` | `CONFIGURATION_STATUS` | Conducta |
|---|---|---|---|---|
| ~05:54 | `claude-opus-5-5` | `medium` | `BELOW_REQUIRED` | STOP antes de trabajo sustantivo con `PRINCIPAL_SESSION_CONFIGURATION_REQUIRED` |
| ~05:56 y ~05:58 (tras el cambio del Owner) | `claude-opus-5-5` | `max` | `ABOVE_REQUIRED` | PASS, con la discrepancia anotada (el Owner indicó `xhigh`) |
| ~06:02 (inicio de este cierre) | `claude-opus-5-5` | `xhigh` | `MATCH` | PASS |

El Owner aceptó la prueba como evidencia del hueco. Se registra como **seguimiento formal fuera del Freeze de I-61** en
[ideas-futuras](../../ideas-futuras.md) (sección de I-61) y en HANDOFF; no cambia el Freeze, el Candidato ni ningún gate. La memoria local de Claude conserva una ayuda
operativa con la misma regla, que no sustituye a la autoridad versionada.

### 16.8 Reconciliación final de desviaciones y seguimientos

| Desviación | Clasificación final (decisiones §18) | Estado |
|---|---|---|
| DEV-G1C-01 (entrada `trusted` de la sonda P2) | decisión del Owner: eliminar | eliminada (decisiones §17) |
| DEV-G2-01 (snapshot desde Git Bash) | NON-MATERIAL | cerrada; README §3.2 manda PowerShell |
| DEV-G3-01 (registros de U-04 posteriores) | NON-MATERIAL | cerrada |
| DEV-G3-02 (relectura y host de consola; huérfano de otra sesión resuelto como STOP) | BEHAVIORAL-WITHIN-FREEZE | aceptada; propuesta en ideas-futuras |
| DEV-G3-03 (filtro P3 del contrato) | NON-MATERIAL | aceptada; propuesta en ideas-futuras |
| DEV-G3-04 (camino de reverificación) | BEHAVIORAL-WITHIN-FREEZE | aceptada; propuesta en ideas-futuras |

Los seguimientos no bloqueantes (las cinco propuestas del piloto de 15.12, los de la conformidad final, los hallazgos fuera de alcance del contrato §13 y la autoverificación de
la sesión principal) quedan en [ideas-futuras](../../ideas-futuras.md), sección de I-61.

### 16.9 Métricas de la unidad (LIFECYCLE §11)

| Métrica | Valor |
|---|---|
| Arquetipo inicial / final y disparadores | NEW ARCHITECTURE provisional (el mandato decía FOUNDATION EVOLUTION) / NEW ARCHITECTURE (Q-01); M-01, M-02, M-04, M-05, M-06, M-07 y M-08; el piloto, EXTENSION |
| Rondas Discovery y EXP | 2 (G1 y G1-C); EXP-04, 05, 06, 07, 08 y 09 evaluadas (Discovery) |
| Rondas Architect y modo | diseño: 8 rondas y 2 confirmaciones; G2: revisión y confirmación; READY-06: 2 (NON-CONFORMING y CONFORMING). Todas **SAME-SESSION ROLE** |
| Gates totales / verificables | 4 (G0, G1, G2, G3) / 2 funcionales (G2, G3) |
| Full locales por cierre / Candidato | Core Full en el cierre de G2 (12380) y de G3 (12396); Core Full y UI Full del Candidato (12396; 1637 + 17 omitidas) |
| Intentos e invalidaciones de Candidato | 1 Candidato declarado y validado; 1 punta (`b69c3463`) invalidada en READY-06 antes de declararse |
| Intentos CI y rojos no leídos | 16 corridas `push` en la rama (15 success, 1 failure = el RED esperado de G3, leído) y 1 `workflow_dispatch` de cobertura; 0 rojos no leídos |
| Rondas Owner de política/producto y hallazgos | mandato y órdenes de continuidad; decisiones ADR-0046 y DEV-G1C-01; 1 ronda de Owner Validation (PASS, sin hallazgos); 1 hallazgo de proceso (configuración de la sesión principal) |
| Desviaciones | DEV-G1C-01, DEV-G2-01, DEV-G3-01..04 |
| Invalidaciones Freeze | 0 (sin A-n) |
| Findings escapados después del merge | UNKNOWN (sin merge todavía) |
| Duración activa de la Owner Validation | UNKNOWN (no declarada) |

### 16.10 Integración

La integración es manual, serializada y de autoridad del Owner (WORKFLOW §11.5). Tag esperado: `integration/I-61`. `CLOSURE_SHA`, `MERGE_SHA`, la CI posterior al merge con
cobertura, la comprobación diferida de cobertura del Candidato y la limpieza pertenecen a ese tag (WORKFLOW §11.4 y §11.6); este archivo no contiene el SHA de su propio commit
de cierre.
