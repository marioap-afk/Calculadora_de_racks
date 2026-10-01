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
