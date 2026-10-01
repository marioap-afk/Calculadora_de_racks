# I-61 — Evidencia de la unidad (Agent Execution, Model Routing & Prompting Protocol)

Unit / Initiative / Workflow: `I-61` / I-61 / V2. Claim-Id `0e2923de-e1a7-41bf-b7db-50ec84217850`.
Contrato: [I-61-protocolo-ejecucion-agentes.md](../../initiatives/I-61-protocolo-ejecucion-agentes.md). Decisiones: [I-61.md](../decisions/I-61.md).

Estado de esta evidencia: **G0 (reclamo y bootstrap, GATE PASS del Coordinator sobre `6f1ef981`) y G1 (Discovery, §9; consolidación G1-C, §§10-11)**. No hay Freeze, piloto ni Candidato; la decisión de G1 consta en [decisiones](../decisions/I-61.md) §12.
Lo no ejecutado figura como **PENDIENTE**.

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

## 15. G3 — piloto real (en curso)

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
