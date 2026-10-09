# I-62 — Evidencia de la unidad (Principal Coordinator Portability & Provider-Agnostic Role Binding)

Unit / Initiative / Workflow: `I-62` / I-62 / V2. Claim-Id `5b661a17-8c18-4183-8554-3866059cba2b`.
Contrato: [I-62-portabilidad-coordinador-principal.md](../../initiatives/I-62-portabilidad-coordinador-principal.md). Decisiones: [I-62.md](../decisions/I-62.md).

Estado de esta evidencia:
- **G0 cerrado**: GATE PASS administrativo del Coordinator (C62-G0-02) sobre `85ae4324…`; hechos posteriores al bootstrap en §9.
- **F0, tramo Discovery**: entrega inicial (C62-F0-01; §10) y ronda R1 (C62-F0-02..07; §11), aceptada como base de diseño (C62-F0-08).
- **F0, diseño**: Proposal V1 (§12) y V2 (§13), ambas CHANGES REQUIRED del Coordinator (C62-F0-13, C62-F0-17); Proposal V3 con su paquete para revisión (C62-F0-20; §14). Sin Freeze.
- No hay Freeze, piloto, Candidato ni F0 GATE PASS. Lo no ejecutado figura como **PENDIENTE**.
- §§1-8 son el registro original de G0 y se conservan como se escribieron.

Clases de afirmación ([INITIATIVE_LIFECYCLE](../../INITIATIVE_LIFECYCLE.md) §4):
- `MEASURED`: lo midió la sesión responsable de G0, una sesión de Claude Desktop en el host Windows del Owner.
- `RECONSTRUCTED`: lo reportó otra fuente y aquí no se reejecutó.
- `INFERENCE` y `UNKNOWN`: según su significado.

Nada de lo `MEASURED` se atribuye al Coordinator.

## 1. Base de reclamo y clasificación de transición (MEASURED)

| Hecho | Valor | Fuente |
|---|---|---|
| Base (`origin/main`) | `819955d61a6da4c811a11fbd11b5dca13f634b7c`, revalidada inmediatamente antes del reclamo | `git fetch --prune` + `git rev-parse origin/main` |
| Commit de reclamo | `a851841ae40d63f59ac18ff64f792efd07ca036c` (vacío; `2026-10-01T10:04:12-06:00; push aceptado 2026-10-01T16:04:13Z`) | `git log` |
| Primer push aceptado (reclamo) | rama nueva `architecture/portabilidad-coordinador-principal` en `origin`, sin force | salida de `git push -u` |
| Rama / worktree | `architecture/portabilidad-coordinador-principal` / `%USERPROFILE%\.codex\worktrees\architecture-portabilidad-coordinador-principal`; distinto del principal y de los de I-52, I-63 e ID30 | `git worktree list` |
| Worktree principal | `main` sin actualizar a propósito, árbol limpio; ninguna operación Git en curso en el repositorio común | `git status`, marcadores de `.git` |
| ID | I-62 reservado por el Owner. Antes del reclamo no había rama ni tag de I-62 en el remoto. `origin/main` no menciona I-62; en la rama de I-63 solo aparece como reserva | `git ls-remote`, `git grep` |
| `WORKFLOW_V2_EFFECTIVE_SHA` | `8a021fb67c16dfccd6afc18448ea7e6a71a32364`. Derivación (§11.2): commit único con el trailer `Workflow-V2-Normative: I-56` (`afd1077c…`) y primer merge first-parent cuyo segundo padre lo alcanza y el primero no. Es ancestro de la base | `git log --grep`, `git merge-base --is-ancestor` |
| Fin de la pausa | tag anotado `integration/I-56` (objeto `41b1835b…`): `Claim pause … end=2026-09-17T22:30:00Z` | `git tag -l --format=%(contents)` |
| Clasificación | **V2, T4** (posterior probado, base con el SHA efectivo, sin pausa; consume diseño V2) | [WORKFLOW](../../WORKFLOW.md) §11.3 |

Recibo de I-61, revalidado como antecedente (MEASURED):
- `integration/I-61` es un tag anotado (`8a6f71ed…`) que apunta a `819955d6…`;
- los padres del merge son `95690c28…` y `a5e583ed…` (Closure);
- `6513777f…` (Candidato) es ancestro del Closure, y entre ambos solo cambian archivos de `docs/`;
- la corrida `36866499374` fue push sobre `main` con `head_sha` `819955d6…` y 4/4 jobs `success`.

Esto no reabre I-61.

## 2. Fuentes recibidas (entrada de G0)

Todas se custodian fuera del repositorio, en `D:\IDs\I-62\`. Los hashes los midió la sesión en el host.

| Archivo | Bytes | SHA-256 | Papel |
|---|---:|---|---|
| `I-62-owner-mandate.original.txt` | 19 004 | `2364e892d050b939c910e6b5d22ea5960d7bb36dbfe3d4fe4c349ed0f6780f0a` | mandato original; se versiona como [`I-62-owner-mandate.txt`](../decisions/I-62-owner-mandate.txt) |
| `I-62-G0-orden-claim-bootstrap.txt` | 9 263 | `5d6cb6473b36e22d761d4c0c640c40b3879fc863cc171f3013e6a49022d06f94` | orden de G0 del Owner (decisiones O62-G0-01) |
| `I-62-G0-continuacion.txt` | 5 800 | `3c288170ec0eca929c5d6327f0c7e15d277845f67ff3892a197c95ce32ee4694` | continuación del Coordinator (decisiones C62-G0-01) |
| `I-62-admision-y-propuesta-inicial.md` | 23 745 | `141294640252da522ff9c914555a7437e0c08428a8956dd2f3b1e904ed1443c0` | intake previo hecho fuera del host; antecedente, no autoridad |
| `I-62-preflight-observado.json` | 3 276 | `b97e16bc45558ee1e96e4bace0df71bb322c09a9080b0b63ea6f94942e3bd947` | observación del contenedor del intake; **no** describe este host |

**Mandato, verificación en el host (MEASURED):**
- 19 004 bytes, SHA-256 idéntico al declarado;
- UTF-8 estricto válido, sin BOM, solo LF (857, ningún CR), sin LF final (último byte `0x2E`).

`MANDATE_LOCAL = VERIFIED`.

Procedencia (RECONSTRUCTED): el Owner adjuntó el mandato como «Texto pegado.txt» en la conversación del Coordinator. El Coordinator lo copió
byte a byte y comprobó la copia en su entorno, sin acreditar la transferencia a Windows. El Owner dejó el archivo en `D:\IDs\I-62\`
(2026-10-01T15:51:03Z). Cadena de custodia: Owner → Coordinator → Owner → sesión. No hay verificación independiente contra el adjunto
original más allá de la coincidencia del hash declarado.

Blob versionado: `e352ce7e940280e6168b6a20af8192150245b95e`. `git cat-file blob` de ese objeto da 19 004 bytes y el mismo SHA-256 (MEASURED tras el commit). Con
`core.autocrlf=true` y sin regla `-text`, un checkout en Windows puede convertir el archivo a CRLF en disco: el hash canónico es el del
blob. No se cambió configuración Git ni normas para forzar la coincidencia.

## 3. Coordinación de escritura de `docs/ROADMAP.md`

Canal: mensajería entre sesiones locales de Claude Desktop. El canal no reporta hora de entrega ni de lectura: las horas son del reloj de
la sesión y ningún acuse se fecha más allá de eso. Solicitud enviada, entrega encolada y acuse son hechos distintos.

**Ronda 1** (antes de 15:35Z): sin reclamo. ID20 e ID30 acusaron; I-52 acusó tarde. La sesión se detuvo porque faltaba el mandato y envió
RELEASE o retiro a las tres. Esos acuses no se reutilizan.

**Compromiso intermedio:** I-62 dio un acuse a ID20 para la fila de su bootstrap (`af5b2e5c…`, confirmado para I-63 en `a757eb80…`).
ID20/I-63 envió su RELEASE real tras publicar su bootstrap `cba24838…`. Comprobación propia: `ls-remote` a las 15:49:30Z.

**Ronda 2** (solicitudes desde 15:53:46Z, ya con el mandato verificado):
- I-63 (`31a18cf6…`) e ID30 (`2f27fcea…`) acusaron.
- I-52 no respondió en esa ejecución, así que I-62 envió RELEASE a I-63 (`bbcba6a7…`) y a ID30 (`75cb3327…`) para no retener ventanas, y dejó abierta la solicitud a I-52 (`47213183…`).
- ID30 pidió entonces ventana para su propio bootstrap, ya como I-64; I-62 acusó (`fb8f9529…`).
- I-52 acusó la solicitud `47213183`; I-62 le avisó que esperaría el RELEASE de I-64 (`cf2073bd…`).
- I-64 publicó su bootstrap `d8a02163…` y envió su RELEASE real.

**Ventana vigente del bootstrap** (limitada a insertar la fila de I-62; HANDOFF fuera):

| Escritor | Sesión | Solicitud | Respuesta |
|---|---|---|---|
| I-52 | «I - 52» (`local_7a6025bd…`) | `47213183-fd09-4683-b666-10fb5a494ba9` | **ACUSE nuevo**: no escribe, rebasa ni publica ROADMAP hasta el RELEASE; sin escritura de ROADMAP en curso ni planeada; rebase diferido hasta después de sus corridas de host; sin reclamo ni rama incompatible. Host y demás superficies siguen |
| I-63 (ID20) | «I - 63» (`local_ea0644a0…`) | `0ca4a000-6144-475c-ad62-b88333d6688c` | **ACUSE nuevo** (independiente del `31a18cf6` cerrado): ídem; bootstrap `cba24838` publicado; sin escrituras pendientes; nada de I-63 usa I-62 ni la rama |
| I-64 (ID30) | «I - 64» (`local_1c89299c…`) | `8073ecd1-e2ae-43e8-a37f-e83848b0def2` | **ACUSE**: ídem, incluido no rebasar su fila; bootstrap `d8a02163` publicado; sin escrituras pendientes; sin conflicto |

- INICIO de la ventana: los tres acuses, recibidos antes del reclamo (comprobación de la base a las 16:03:31Z).
- FIN: el «RELEASE» de I-62 por el mismo canal tras publicar el bootstrap (§7).

I-62 no valida ni asigna los números I-63 e I-64; los registra como los comunicaron esas sesiones.

## 4. Preflight del host (MEASURED, 2026-10-01; datos saneados, sin credenciales)

| Elemento | Observación |
|---|---|
| Plataforma | Windows 11 Pro 10.0.26200; PowerShell 7.6.6 |
| Git | 2.54.0.windows.1; `core.autocrlf=true` |
| GitHub CLI | 2.96.0; autenticado (cuenta del Owner, keyring, https); ámbitos `repo`, `workflow`, `read:org`, `gist` |
| .NET | SDK 8.0.423 de usuario en `%LOCALAPPDATA%\Microsoft\dotnet`; el `dotnet` del PATH es otra instalación |
| Codex CLI | **no está en el PATH**. Ejecutable efectivo `%LOCALAPPDATA%\OpenAI\Codex\bin\de8a38d2100ae498\codex.exe`, `codex-cli 0.159.2`; `login status` = «Logged in using ChatGPT». **Hallazgo:** de cuatro directorios en `bin`, el más reciente por mtime (`5cb96978…`) **no** contiene `codex.exe`, así que localizar por «directorio más nuevo» falla. Método usado: enumerar los directorios y comprobar que el ejecutable existe |
| Claude CLI | **no está en el PATH** de PowerShell. `%USERPROFILE%\.local\bin\claude.exe` = 2.1.270; `claude auth status` → `loggedIn: false`, `authMethod: none` (**NOT_AUTHENTICATED**). La app de escritorio trae otras copias (2.1.281, 2.1.284). La autenticación del escritorio no autentica el CLI |
| Sesión principal | `get_session("self")` → `claude-opus-5-5` / `xhigh`. Es una observación de la interfaz y no acredita ninguna invocación delegada (decisiones S62-G0-04) |
| `config.toml` de Codex | SHA-256 `42E15A039EFD7E1A9197423AFB14B4358C7D60C89FEDDD734583891DB6E732A5`, mtime 2026-10-01T05:30:20Z. Difiere de los hashes registrados en I-61; antecedente preservado, causa UNKNOWN (decisiones S62-G0-03). No se leyeron ni copiaron valores |
| Procesos (una foto) | `ChatGPT.exe` ×11, `claude.exe` ×17, `codex.exe` ×2 (app-server y exec-server de la app de Codex), `node.exe` ×6, `git.exe` ×2 (status/ls-files lanzados por el mismo proceso padre de la app); sin `acad.exe` ni `dotnet.exe`. La atribución de los `git.exe` no se clasifica en G0 como falso positivo ni como escritor: no hubo delegación |
| Invocaciones de modelos | NOT_RUN (no hubo sondas, Controller ni Worker) |

## 5. Bootstrap (contenido versionado de G0)

- Archivos nuevos:
  - el contrato `docs/initiatives/I-62-portabilidad-coordinador-principal.md`;
  - `docs/automation/decisions/I-62.md` y `docs/automation/decisions/I-62-owner-mandate.txt`;
  - `docs/automation/state/I-62.yml`;
  - este archivo.
- Archivo modificado: `docs/ROADMAP.md`, una fila en Engineering Productivity.
- **Nada más**: sin cambios en normas globales, HANDOFF, FOUNDATIONS, esquemas, índice ADR, `src/`, `tests/`, `assets/`, CI ni configuración.
- `automation.enabled: false`. Los campos `requires_*` vacíos quedan pendientes y no conceden exenciones.

## 6. Validación de G0

- **Comprobación documental:** el diff contra la base solo toca los seis archivos de la allowlist de la orden (resultado en el cuerpo del
  commit de bootstrap).
- **CI exacta requerida** ([WORKFLOW](../../WORKFLOW.md) §4.5.2, commit documental): corrida de `push` sobre el SHA publicado, con los 4 jobs
  de `AGENTS.md` en `success`. **PENDIENTE** al escribir este archivo; se registra en el informe de G0 y en la siguiente escritura de la
  unidad.
- Suites Core/UI locales, builds del Plugin y Owner Validation: **no aplican a G0** (sin cambios de producto, `src/`, `tests/` ni `assets/`);
  no se ejecutaron ni se simulan.

## 7. Registros posteriores a la publicación del bootstrap

PENDIENTE: SHA del commit de bootstrap (es el commit que introduce este archivo), envío del «RELEASE» a las tres sesiones de §3 y corrida de
CI exacta. Se anotan en el informe al Coordinator y en la siguiente escritura de la unidad. No se crea un commit adicional solo para esto.

## 8. Métricas, conformidad, Owner Validation y tag

- Métricas: UNKNOWN (no hay gates funcionales).
- Conformidad: no aplica todavía.
- Owner Validation: no aplica a G0; la asignación la fija el Freeze.
- Tag de integración: no existe (`integration/I-62` solo se crea tras la integración).

## 9. Hechos posteriores a la publicación del bootstrap (cierra el PENDIENTE de §7)

| Hecho | Valor | Fuente y clase |
|---|---|---|
| Commit de bootstrap | `85ae4324988ac7564719664f8f05b3fa9c6cafbe` (el que introduce este archivo); push a las 16:06:26Z; `ls-remote` = el mismo SHA; árbol limpio y sincronizado | salida de `git push` y `git ls-remote`; MEASURED por la sesión |
| CI del bootstrap | corrida **36889478597**, attempt 1, event `push`, rama `architecture/portabilidad-coordinador-principal` (ref `refs/heads/…`), head_sha `85ae4324…`, `completed/success`. Jobs: Tests (Domain + Application), UI Tests (WPF controls, net8.0-windows), Build UI (WPF, valida API de Application) y Build Plugin without AutoCAD, los cuatro `success` | `gh run view`; MEASURED por la sesión y verificado de forma independiente por el Coordinator por GitHub (C62-G0-02) |
| CI del reclamo | corrida 36889198265, `push`, head_sha `a851841a…`, `success` | `gh run view`; MEASURED |
| RELEASE de la ventana de ROADMAP | enviados tras verificar el SHA remoto y antes de terminar la CI: I-52 `19a9b2a9-bc49-43fc-a7c4-ca2e9a0292aa`, I-63 `447d8f54-aa56-47f8-be6c-cdacfa76b817`, I-64 `3384d5c2-256a-4809-8570-db6537512fa6`. Entrega encolada; el canal no reporta lectura. El texto aclara que el RELEASE no declara CI verde | resultado de los envíos; MEASURED (envío), lectura UNKNOWN |
| Decisión de G0 | GATE PASS administrativo del Coordinator (decisiones §7, C62-G0-02). Lo que el Coordinator verificó por GitHub y lo que acepta como hecho reportado están separados allí | orden del Coordinator (§10) |

## 10. F0 — tramo Discovery (C62-F0-01)

Lo ejecutó la sesión principal directamente, sin delegación. El contenido está en el [Discovery](../../initiatives/I-62-discovery.md); aquí van los hechos
que lo respaldan.

**Orden recibida:** `I-62-F0-orden-discovery.txt`, 14 753 bytes, SHA-256 `4575521afd51cf0d0e1e64c50b0f8f932c80828c837cec64b6d677e8c768a97e`. Se custodia
fuera del repositorio (`D:\IDs\I-62\`); su contenido consta por ID en las decisiones §7.

**Apertura** (MEASURED, 16:29Z):
- `git fetch --prune`: `origin/main` = `819955d6…`, sin avance; no hizo falta rebase;
- `HEAD` = `origin/<rama>` = `85ae4324…`; árbol limpio; mismo worktree y Claim-Id;
- ramas ajenas observadas: I-52 `c1982b2a`, I-63 `b557ea3b`, I-64 `d8a02163`, sin tocarlas.

**Sesión principal** (MEASURED): `get_session("self")` a las 16:36Z → `claude-opus-5-5` / `xhigh`. Es una observación del host, comparada con el perfil
propuesto (MATCH); no es un perfil I-62 vigente.

**Prueba existente ejecutada** para contrastar afirmaciones de DC-06 (MEASURED):

| Campo | Valor |
|---|---|
| SDK resuelto | 8.0.423 (`%LOCALAPPDATA%\Microsoft\dotnet\dotnet.exe`; `global.json` 8.0.423) |
| Comando | `dotnet test tests/RackCad.Tests/RackCad.Tests.csproj --filter "FullyQualifiedName~RackCad.Tests.AgentExecutionProtocolTests" --logger "trx;LogFileName=i62-aep.trx" --results-directory <scratchpad de la sesión>` |
| SHA / árbol | `85ae4324…` / `d277aaa4c60ac165a10f9c305e020ba8d05cfd03` |
| Resultado | selección **17** (> 0); superadas 17; fallidas 0; omitidas 0; TRX `total=17 executed=17 passed=17 notExecuted=0` |
| TRX | SHA-256 `4344a953271b8e8737d34e9413b54c16b9e247243cd3a392bba52b1e06bad580`, fuera del repositorio (no versionado: contiene rutas locales); el árbol siguió limpio |
| Clase | prueba existente para contrastar una afirmación. No es RED→GREEN, evidencia de gate ni Full; no se creó ni modificó ninguna prueba |

**`config.toml`** (MEASURED, 16:33Z; método del README §3.1: SHA-256 y nombres de secciones o claves, **sin valores**; las rutas de proyecto se
redactan):
- SHA-256 `42E15A03…`, mtime 2026-10-01T05:30:20Z, 103 nombres;
- frente a `Exit.ConfigTomlKeys` del último relevo custodiado de I-61 (`g3-cama-d1a/R20261001T041132Z-17e8`, 103 nombres, `89F375C6…`): 26
  diferencias;
- 12 cabeceras de proyecto en el registro frente a 11 hoy, todas con texto distinto;
- entradas nuevas `[tui]` y `screen_reader_detection_done`;
- sale el `trust_level` eliminado por orden del Owner (decisiones de I-61 §17, referencia `37DD3559…`);
- las 12 entradas `relay-record.json` custodiadas de I-61 (02:45Z-04:31Z) tienen `ConfigChanged=false` y `89F375C6…`.

Interpretación y consecuencias: Discovery §13.1.

**Lecturas de runtime:** no se repitieron las de G0 (§4), que se citan con su fecha. Se leyó el listado de `%LOCALAPPDATA%\OpenAI\Codex\bin\` (fechas
de directorio) para contrastarlo con la ruta del binario registrada en I-61 (`bin\c6fe824d…`, ya inexistente).

**No se hizo:**
- invocaciones de Controller, Worker, Reviewer o Architect externo, subagentes, sondas de modelos ni pilotos;
- instalar, autenticar o cambiar PATH o configuración;
- escrituras en ROADMAP, HANDOFF, FOUNDATIONS, normas, esquemas, `src/`, `tests/` o superficies ajenas.

No se pidió evidencia a I-63 ni a I-64 por el canal: su evidencia publicada ya acredita que no delegaron (Discovery §1).

**Validación de esta entrega:** diff contra la base del tramo (`85ae4324`) limitado a la allowlist de C62-F0-01 y `git diff --check`; el resultado va en
el cuerpo del commit. La CI de push del commit de esta entrega se informa al Coordinator. No se registra aquí para no guardar un SHA autorreferente.

## 11. F0 — ronda R1 (C62-F0-02..07)

**CI de la entrega inicial del Discovery** (`f25dd1d53ca5da3b2d8ad1842f2c2eb8b3cf4d8c`): corrida **36894150011**, attempt 1, event `push`, rama
`architecture/portabilidad-coordinador-principal`, head_sha exacto, `completed/success`. Jobs: Build UI (WPF, valida API de Application), Tests (Domain +
Application), UI Tests (WPF controls, net8.0-windows) y Build Plugin without AutoCAD; los cuatro `success`. MEASURED por la sesión con `gh run view` y verificado
de forma independiente por el Coordinator (decisiones §9, C62-F0-02). Esta CI acredita solo `f25dd1d5`.

**Orden recibida:** `I-62-F0-R1-revision-y-continuacion.txt`, 23 224 bytes, SHA-256 `190052fa5027c15d794cad85eae9e013f6817a27ec3af79b220b3276d1fd2f56`, fuera del
repositorio (`D:\IDs\I-62\`).

**Apertura de R1** (MEASURED, 17:03Z):
- `origin/main` = `819955d6…`, sin rebase;
- rama de I-62 en `f25dd1d5…` = remoto, árbol limpio;
- I-52 `c1982b2a` (sin cambio); I-63 `dbfe1150` e I-64 `cf034e59`, cuyos commits nuevos solo tocan documentos propios (`git diff --name-only`).

**Lecturas de R1** (MEASURED; sin modificar nada):
- PROMPT_TEMPLATES `0e3ed262`: l. 3 (preámbulo condicional), l. 13 (§1) y l. 274-277 (§2: bloque `WORKFLOW V2 = NOT EFFECTIVE`, introducido en `7e9073e7`,
  2026-09-15, según `git log -L`);
- LIFECYCLE §2 y los marcadores «materialized by later I-56 normative gate»;
- lista de entradas de FOUNDATIONS (13 nombres);
- inventario de la custodia de I-61 por tipo de archivo;
- `git grep` de los esquemas `rackcad-*/v1` en las ramas remotas: fuera de I-61/I-62, solo AUTOMATION_PLAN;
- `grep` del README §3.2 buscando criterios de relectura o `conhost`: sin coincidencias;
- cláusulas de «cierre de G2» en AUTOMATION_PLAN (16.3, 16.5, 16.7).

**SP-3, inspección pasiva de metadatos** (MEASURED, entre 17:03Z y 17:15Z; C62-F0-06):
- **Ubicación y volumen:** `%USERPROFILE%\.codex\sessions\…\rollout-*.jsonl`, 62 archivos.
- **Qué se leyó:** de la primera línea, solo los campos `originator`, `source`, `thread_source`, `cli_version` y `model_provider`, y la **lista de nombres** de
  claves. De las líneas `turn_context`, solo `model`, `effort` e instante.
- **Qué no se copió:** conversaciones, identificadores de cuenta o sesión, rutas de trabajo, instrucciones ni valores de configuración.
- **Sin efectos:** ninguna invocación, sesión nueva, proceso participante ni prompt.
- **Resultado:** las sesiones de escritorio (`originator` `Codex Desktop` / `codex_work_desktop`, `source` `vscode`) registran `model` y `effort` por turno.
  Ejemplo: último `turn_context` de una sesión `codex_work_desktop`/`chatgpt_handoff` a las 14:35:09Z (2026-10-01): `gpt-6-astra`/`ultra`. Las sesiones
  `codex_exec` (I-61) registran `gpt-6-luna`/`high`.
- **Interpretación y límites:** Discovery §17. Fuente candidata; no acredita a un Principal ni habilita ninguna celda.

**No se hizo:**
- invocaciones de modelos, subagentes, Controller, Worker, Reviewer o Architect externo;
- SP-1 o SP-2;
- autenticar, instalar o cambiar PATH o configuración;
- repetir las 17 pruebas, porque ninguna afirmación de R1 lo exigía;
- escrituras fuera de la allowlist.

`docs/automation/evidence/I-62-discovery/` no se creó: no hizo falta evidencia saneada adicional.

**Validación de esta entrega:** allowlist, enlaces y `git diff --check`; resultado en el cuerpo del commit. La CI de este commit se informa al Coordinator;
no se registra aquí para no guardar un SHA autorreferente.

## 12. F0 — diseño y Proposal V1 (C62-F0-08..12)

**CI de la ronda R1** (`2b7976b02a44e2aca7feee2df473a86eda852fb0`): corrida **36898507559**, attempt 1, event `push`, rama
`architecture/portabilidad-coordinador-principal`, head_sha exacto, `completed/success`. Jobs: Tests (Domain + Application), UI Tests (WPF controls,
net8.0-windows), Build UI (WPF, valida API de Application) y Build Plugin without AutoCAD; los cuatro `success`. MEASURED por la sesión con `gh run view` y
verificado por el Coordinator (C62-F0-08). Acredita solo `2b7976b0`.

**Orden recibida:** `I-62-F0-resolucion-R1-y-orden-diseno.txt`, 21 665 bytes, SHA-256 `171fa091b7badb470b63a97c401955b2f9ee25cf540aec4d4f3c6987b91f2f3e`, fuera
del repositorio (`D:\IDs\I-62\`).

**Apertura del tramo** (MEASURED, 17:40Z):
- `origin/main` = `819955d6…`, sin rebase;
- rama de I-62 en `2b7976b0…` = remoto, árbol limpio;
- I-52 `c1982b2a` (sin cambio);
- I-63 `85293273` (`dbfe1150..85293273`: solo `I-63-*` propios);
- I-64 `c6c44828` (`cf034e59..c6c44828`: solo `I-64-*` propios);
- ninguna superficie candidata de I-62 tocada por otra rama (DC-07 por delta).

**Trabajo del tramo:** redacción directa de la Proposal V1, del paquete del Architect y del borrador del ADR sucesor. No se tomó ninguna medición nueva, no se
invocó ningún modelo ni subagente, no hubo sondas, no se repitieron las 17 pruebas (ninguna afirmación lo exigía) y no se escribió fuera de la allowlist.
`docs/automation/evidence/I-62-discovery/` no se creó.

**Validación de esta entrega:** allowlist, enlaces y `git diff --check`; resultado en el cuerpo del commit. La CI de este commit se informa al Coordinator; no
se registra aquí para no guardar un SHA autorreferente.

## 13. F0 — Proposal V2 (C62-F0-13..16)

**CI de la Proposal V1** (`ea0555913202efc96e1007f021942579b989b43d`): corrida **36902279722**, attempt 1, event `push`, rama
`architecture/portabilidad-coordinador-principal`, head_sha exacto, `completed/success`. Jobs: UI Tests (WPF controls, net8.0-windows), Tests (Domain +
Application), Build UI (WPF, valida API de Application) y Build Plugin without AutoCAD; los cuatro `success`. MEASURED por la sesión con `gh run view`
y verificado por el Coordinator, con los ids de job 110504100000, 110504100280, 110504100386 y 110504922226 (C62-F0-13). Acredita solo `ea055591`.

**Orden recibida:** `I-62-Proposal-V1-revision-y-orden-V2.txt`, 27 797 bytes, SHA-256 `101e9bbaec51c967395bce10de69dac4a241a87310e496038ce323d9b1f3fb0b`,
fuera del repositorio. Su registro recuperable está en `docs/initiatives/I-62-coordinator-review-v1.md`.

**Apertura del tramo** (MEASURED, 19:16Z):
- `origin/main` = `819955d6…`, sin rebase;
- rama de I-62 en `ea055591…` = remoto, árbol limpio;
- I-63 `85293273` e I-64 `c6c44828`, sin cambio desde el tramo anterior;
- **I-52 `88138f01`**: `c1982b2a..88138f01` toca `docs/automation/decisions/I-52.md` y `eng/research/I52Ct21dPhase2/**`; **ninguna** superficie candidata de
  I-62 (`git diff --name-only` sobre AUTOMATION_PLAN, agent-execution, PROMPT_TEMPLATES, WORKFLOW, AGENTS, CLAUDE.md, FOUNDATIONS, LIFECYCLE, la prueba
  guardiana, `.gitignore` y context packs: vacío).

**Lectura del tramo** (sin modificar nada): LIFECYCLE §§5-9 (estados de revisión, Freeze, gates, READY, conformidad).

**Identidad del contenido entregado** (blob calculado con `git hash-object` antes del commit; el commit lo da el recibo):
- `docs/initiatives/I-62-proposal-v2.md` → `47a643ddf1f22226072ede67a862133f77441e2e`;
- `docs/initiatives/I-62-coordinator-review-v1.md` → `b518a4f977ee5b18157fc45c881a8ea06d69d7cc`.

**No se hizo:**
- invocaciones de modelos ni subagentes, sondas, pilotos, fixtures, pruebas nuevas ni repetidas, tooling ni esquemas operativos;
- tocar V1, su paquete, el mandato ni superficies ajenas.

`docs/automation/evidence/I-62-discovery/` no se creó, y el Discovery no cambió: ningún hecho nuevo lo exigía.

**Validación de esta entrega:** allowlist, enlaces y `git diff --check`; resultado en el cuerpo del commit. La CI de este commit se informa al Coordinator.

## 14. F0 — Proposal V3 (C62-F0-17..20)

**CI de la Proposal V2** (`a1f5e0035f0e109d02a336c0a01917e53a53b88a`): corrida **36914815626**, attempt 1, event `push`, rama
`architecture/portabilidad-coordinador-principal`, head_sha exacto, `completed/success`. Jobs: UI Tests (110546066161), Build UI (110546066596), Tests
(Domain + Application) (110546066788) y Build Plugin without AutoCAD (110546725540), los cuatro `success`. MEASURED por la sesión y verificado por el
Coordinator (C62-F0-17). Acredita solo `a1f5e003`.

**Orden recibida:** `I-62-Proposal-V2-revision-y-orden-V3.txt`, 31 529 bytes, SHA-256 `7f55b9311b930b344bae7e1a0a6dcae64d8270bad515ccbbe38f79c8b31c9cb2`,
fuera del repositorio. Su registro recuperable está en `docs/initiatives/I-62-coordinator-review-v2.md`.

**Apertura del tramo** (MEASURED, 19:51Z):
- `origin/main` = `819955d6…`, sin rebase;
- rama de I-62 en `a1f5e003…` = remoto, árbol limpio;
- deltas por rama (`git diff --name-only`), **ninguna** superficie candidata de I-62 tocada:
  - I-52 `c1982b2a..d8078ef3`: 24 archivos, sus decisiones y `eng/research/`;
  - I-63 `85293273..23eeefc5`: 5 archivos propios;
  - I-64 `c6c44828..dcc16bed`: 4 archivos propios.

**Identidad del contenido entregado** (blob calculado con `git hash-object` antes del commit; el commit lo da el recibo):
- `docs/initiatives/I-62-proposal-v3.md` → `36b13443b3d8cdc21fe731b4a5ab263cc9fd7fb9`;
- `docs/initiatives/I-62-coordinator-review-v2.md` → `b431510af260da6f57b2c347f3378e3f1a52f6cb`.

**No se hizo:**
- invocaciones, subagentes, sondas, pilotos, sesiones nuevas, fixtures, remotos, pruebas nuevas ni repetidas, tooling ni esquemas operativos;
- tocar V1, V2, sus paquetes, los registros previos, el Discovery, el mandato ni superficies ajenas.

Las trazas de V3 (anexos E.3, F y G.2) son **análisis del diseño**, no ensayos. `docs/automation/evidence/I-62-discovery/` no se creó.

**Validación de esta entrega:** allowlist, enlaces y `git diff --check`; resultado en el cuerpo del commit. La CI de este commit se informa al Coordinator.

## 15. F0 — Proposal V4 (C62-F0-21..22)

**CI de la Proposal V3** (`486e45e797642ad589980584e16b3e50e53c1fb9`): corrida **36918816719**, attempt 1, event `push`, rama
`architecture/portabilidad-coordinador-principal`, head_sha exacto, `completed/success`. Jobs: Build UI (110559415075), Tests (Domain + Application)
(110559415090), UI Tests (110559415115) y Build Plugin without AutoCAD (110560254404), los cuatro `success`. MEASURED por la sesión y verificado por el
Coordinator (C62-F0-21). Acredita solo `486e45e7`.

**Orden recibida:** texto pegado por el Owner en la conversación, **sin archivo de origen** en `D:\IDs` y sin hash de archivo. Procedencia: la transcripción
literal recibida, 5 854 bytes en UTF-8, SHA-256 `cb3ace77e6fbf7659af23bbb13f742666b09693cba4ff43fa72773e5b8213de5`, fuera del repositorio. Su registro
recuperable está en `docs/initiatives/I-62-coordinator-review-v3.md`.

**Apertura del tramo** (MEASURED, 20:16Z):
- `origin/main` = `819955d6…`, sin rebase;
- rama de I-62 en `486e45e7…` = remoto, árbol limpio;
- deltas por rama (`git diff --name-only`), **ninguna** superficie candidata de I-62 tocada:
  - I-52 `d8078ef3`: sin cambios;
  - I-63 `23eeefc5..f0b063e9`: 1 commit, 5 archivos propios (contrato, Discovery, decisiones, evidencia y estado de I-63);
  - I-64 `dcc16bed`: sin cambios.
- revalidación antes del commit (MEASURED, 20:51Z): `origin/main` y la rama de I-62 sin cambios; I-63 `f0b063e9..a0654ae2` (2 commits) e I-64
  `dcc16bed..ab4efe86` (1 commit, Proposal V2 de I-64), solo archivos propios; I-52 sin cambios; **ningún cruce** con superficies de I-62.

**Hechos consultados para el diseño:**
- MEASURED: `docs/automation/agent-execution/schemas/gate-contract.schema.json` (`/v1`), blob `56ced893586abc4a571d842f631e435342906f6a`, último cambio en
  `4da82667`. `Authorities[]` = `{Path, Section, Class}`; `MainSha` y `AuthorityRevision`, un SHA cada uno.
- MEASURED: el contrato real de I-61 G3 (`docs/automation/evidence/I-61-pilot/g3-cama-d1a/R20261001T032333Z-4a2d/gate-contract.json`, blob
  `9b5ef6dfed25347310a6c2a8ecb91e48976928cc`, commit `5cda24f3`) cita 17 autoridades. `Section` toma tres formas: la línea de encabezado exacta, «preámbulo»
  o «documento completo».
- MEASURED: el job Core de `.github/workflows/ci.yml` usa `actions/checkout@v4` sin `fetch-depth`. INFERENCE: con el valor por defecto el checkout es
  superficial. Por eso C-20a no usa historia y C-20b es MC.
- MEASURED: §8 de AUTOMATION_PLAN (forma de `state/v1`), 16.3, 16.4, 16.6, 16.8 y 16.9, y WORKFLOW §§11.1-11.3, en `819955d6`.

**Identidad del contenido entregado** (blob calculado con `git hash-object` antes del commit; el commit lo da el recibo):
- `docs/initiatives/I-62-proposal-v4.md` → `dd9e478d737c76139b0eecc3c9e30d9beae8ccd6`;
- `docs/initiatives/I-62-coordinator-review-v3.md` → `461ef605ffc04b1df85ce6cf60d2a132e7506403`.

**No se hizo:**
- invocaciones, subagentes, Architect, sondas, pilotos, sesiones nuevas, fixtures, remotos, pruebas nuevas ni repetidas, tooling ni esquemas operativos;
- tocar V1, V2, V3, sus paquetes, los registros previos, el Discovery, el mandato ni superficies ajenas.

Las trazas de V4 (anexos E.6, F y G.2) son **análisis del diseño**, no ensayos. `docs/automation/evidence/I-62-discovery/` no se creó.

**Validación de esta entrega:** allowlist, enlaces, columnas de las tablas y `git diff --check`; resultado en el cuerpo del commit. La CI de este commit se
informa al Coordinator.

## 16. F0 — Proposal V5 (C62-F0-23..24)

**CI de la Proposal V4** (`2960b28614da14ad6ebe9eeaa9631fd787041ac3`): corrida **36924676021**, attempt 1, event `push`, rama
`architecture/portabilidad-coordinador-principal`, head_sha exacto, `completed/success`. Jobs: Tests (Domain + Application) (110578951155), Build UI
(110578951576), UI Tests (110578951642) y Build Plugin without AutoCAD (110579869740), los cuatro `success`. MEASURED por la sesión y verificado por el
Coordinator (C62-F0-23). Acredita solo `2960b286`.

**Orden recibida:** texto pegado por el Owner en la conversación, **sin archivo de origen** en `D:\IDs` y sin hash de archivo. Procedencia: la transcripción
literal recibida, 6 132 bytes en UTF-8, SHA-256 `040606ba4c0ea43dbe9db2c3fae6f4b792b2b005c97c92f8ce6d97147803a4bc`, fuera del repositorio. Su registro
recuperable está en `docs/initiatives/I-62-coordinator-review-v4.md`.

**Apertura del tramo** (MEASURED, 21:59Z):
- `origin/main` = `819955d6…`, sin rebase;
- rama de I-62 en `2960b286…` = remoto, árbol limpio;
- deltas por rama desde la última observación: I-52 `d8078ef3`, I-63 `a0654ae2` e I-64 `ab4efe86`, **sin commits nuevos**; ningún cruce.

**Hechos consultados para el diseño:**
- MEASURED: en el repositorio hay exactamente dos `gate-contract.json`, los de I-61 G2 y G3. Ninguna otra unidad ha usado la ejecución delegada. C-20c parte
  del de G3 (blob `9b5ef6df…`).
- MEASURED: AUTOMATION_PLAN 16.7, en la reverificación sin conflictos, ya usa «las imágenes y el MainSha nuevo». Es la base de `MainSha_eval`.
- MEASURED: WORKFLOW §3 (reclamo = commit de reclamo + push) y §11.1 (el primer push aceptado fija el `Claim-Id`): base del Modelo A.

**Identidad del contenido entregado** (blob calculado con `git hash-object` antes del commit; el commit lo da el recibo):
- `docs/initiatives/I-62-proposal-v5.md` → `b673b134c3f265b891e10807b6f6fd6cbe852773`;
- `docs/initiatives/I-62-coordinator-review-v4.md` → `fb21c49e80377de6102e26a4ccc99785676c6526`.

**No se hizo:**
- invocaciones, subagentes, Architect, sondas, pilotos, sesiones nuevas, fixtures, remotos, pruebas nuevas ni repetidas, tooling ni esquemas operativos;
- tocar V1-V4, sus paquetes, los registros previos, el Discovery, el mandato ni superficies ajenas.

Las trazas de V5 (§8.6 y anexos E.6, F y G.2) son **análisis del diseño**, no ensayos. `docs/automation/evidence/I-62-discovery/` no se creó.

**Validación de esta entrega:** allowlist, enlaces, columnas de las tablas, diff V4→V5 revisado por bloques y `git diff --check`; resultado en el cuerpo del
commit. La CI de este commit se informa al Coordinator.

## 17. F0 — Proposal V6 (C62-F0-25..26)

**CI de la Proposal V5** (`acd88eafa4847e4740fdf9e7899a6988508ec24a`): corrida **36933410100**, attempt 1, event `push`, rama
`architecture/portabilidad-coordinador-principal`, head_sha exacto, `completed/success`. Jobs: UI Tests (110607933716), Tests (Domain + Application)
(110607933905), Build UI (110607933947) y Build Plugin without AutoCAD (110608384588), los cuatro `success`. MEASURED por la sesión y verificado por el
Coordinator (C62-F0-25). Acredita solo `acd88eaf`.

**Órdenes recibidas** (texto pegado, sin archivo de origen en `D:\IDs`):
- un nuevo pegado de la revisión de V4, con el mismo contenido y otro formato. La sesión comprobó que `acd88eaf` seguía siendo la punta remota y no hizo nada
  nuevo;
- la revisión de V5: 4 972 bytes en UTF-8, SHA-256 `3d15bc258e3781eb59e2d14a3cd61258923c41c68d7117fcaeed5de06f6e17b6`, fuera del repositorio. Su registro
  recuperable está en `docs/initiatives/I-62-coordinator-review-v5.md`.

**Apertura del tramo** (MEASURED, 22:24Z):
- `origin/main` = `819955d6…`, sin rebase;
- rama de I-62 en `acd88eaf…` = remoto, árbol limpio;
- I-52 `d8078ef3`, I-63 `a0654ae2` e I-64 `ab4efe86`, sin commits nuevos; ningún cruce.
- revalidación antes del commit (MEASURED, 22:34Z): `origin/main` y la rama de I-62 sin cambios. I-52 `d8078ef3..352cc3de` (1 commit, sus decisiones y su
  evidencia), I-63 `a0654ae2..772ac242` (1 commit, Proposal V1 de I-63) e I-64 `ab4efe86..1f1530be` (1 commit, Proposal V3 de I-64): solo archivos
  propios; **ningún cruce** con superficies de I-62.

**Hechos consultados para el diseño** (MEASURED salvo indicación):
- WORKFLOW §10, filas «Git, reclamo, worktrees, integración, cierre, cadencia documental y transición» y «Operación del ejecutor y ejecución delegada …
  dentro de las reglas de los dueños anteriores». Su texto es idéntico en `7b8662c5` (el `AuthorityRevision` de I-61 G3) y en `819955d6`.
- WORKFLOW §11.3, último párrafo, en `819955d6`: «Tras activación, una unidad V1 lee `main` actual».
- Merge `8a021fb6` («Merge I-56: activa normativamente Workflow V2») en la historia first-parent de `origin/main`.
- WORKFLOW §4, punto 2: rebase sobre el trunk al abrir una sesión.
- El contrato de I-61 G3 clasifica el preámbulo, §2 y §16 de AUTOMATION_PLAN como `UNIT_CHANGE`.
- INFERENCE: ninguna regla de `/v1` lleva al evaluador a un texto leído en `MainSha` con independencia de las `Authorities` del contrato. Por eso el punto de
  entrada se declara como delta de I-62 bajo OD-1.

**Identidad del contenido entregado** (blob calculado con `git hash-object` antes del commit; el commit lo da el recibo):
- `docs/initiatives/I-62-proposal-v6.md` → `19672958ec095ba9f88e103240d34c62c76ee850`;
- `docs/initiatives/I-62-coordinator-review-v5.md` → `4c51a94c4790629f91006db642915e1ae11ce25b`.

**No se hizo:**
- invocaciones, subagentes, Architect, sondas, pilotos, sesiones nuevas, fixtures, remotos, pruebas nuevas ni repetidas, tooling ni esquemas operativos;
- tocar V1-V5, sus paquetes, los registros previos, el Discovery, el mandato, WORKFLOW ni ninguna superficie ajena: el punto de entrada es diseño.

Las trazas de V6 (E.3.0 y E.6) son **análisis del diseño**, no ensayos.

**Validación de esta entrega:** allowlist, enlaces, columnas de las tablas, diff V5→V6 revisado por bloques y `git diff --check`; resultado en el cuerpo del
commit. La CI de este commit se informa al Coordinator.

## 18. F0 — Proposal V7 (corrección de A62-V6-01..03)

**CI de la Proposal V6** (`3888c5c8de19b5abaf479983e56836ef2bcef76e`): corrida **36935918514**, attempt 1, event `push`, rama
`architecture/portabilidad-coordinador-principal`, head_sha exacto, `completed/success`. Jobs: Tests (Domain + Application) (110616038594), Build UI
(110616038840), UI Tests (110616038954) y Build Plugin without AutoCAD (110616675264), los cuatro `success`. MEASURED por la sesión; el Coordinator la cita
en la orden de revisión del Architect. Acredita solo `3888c5c8`.

**Órdenes recibidas** (texto pegado, sin archivo de origen en `D:\IDs`):
- la orden de revisión formal del Architect de V6. La sesión autora **no la ejecutó**: modo exigido SEPARATE SESSION, sin repositorio modificado. Verificó el
  objeto (punta `3888c5c8`, blobs `19672958…` y `028df636…`, registros v1-v5 presentes) y avisó del riesgo de memoria de proyecto;
- la orden «PROPOSAL V7 / ARCHITECT REQUIRED CORRECTIONS»: 7 543 bytes en UTF-8, SHA-256
  `a330d761a1a0825ac55351873a3894b7a799ba3fc49fe2c26402adbfa4c68373`, fuera del repositorio. Registro recuperable en
  `docs/initiatives/I-62-architect-review-v6.md`. El texto literal del dictamen del Architect no se recibió (MEASURED: no está en `D:\IDs` ni en el
  repositorio).

**Apertura del tramo** (MEASURED, 23:00Z):
- `origin/main` = `819955d6…`, sin rebase;
- rama de I-62 en `3888c5c8…` = remoto, árbol limpio;
- revalidación antes del commit (MEASURED, 23:11Z): `origin/main` y la rama de I-62 sin cambios. I-52 `352cc3de..51a66245` (2 commits, sus §§252-253),
  I-63 `772ac242..dddc215f` (Proposal V2 de I-63) e I-64 `1f1530be..e87ba32c` (Proposal V4 de I-64), solo archivos propios; **ningún cruce** con
  superficies de I-62.

**Hechos consultados para el diseño** (MEASURED):
- LIFECYCLE §5: «Toda revision declara `SAME-SESSION ROLE`, `SEPARATE SESSION` o `EXTERNAL HUMAN` y si revisor y autor son la misma persona».
- AUTOMATION_PLAN §16, preámbulo: «Ninguna otra iniciativa lo adopta por estar escrito».
- AUTOMATION_PLAN 16.7: el rebase de apertura, con `--force-with-lease`, se registra con `RebaseMap`; con cadena en curso pertenece a la tarea y no consume
  el máximo de recuperaciones.
- WORKFLOW §4, punto 2: rebase al abrir y publicación con `git push --force-with-lease`.

**Identidad del contenido entregado** (blob calculado con `git hash-object` antes del commit; el commit lo da el recibo):
- `docs/initiatives/I-62-proposal-v7.md` → `9f68c9501fb8254792f875a039e11ec7485108ef`;
- `docs/initiatives/I-62-architect-review-v6.md` → `2217239781eb47ec297c58d4313d511807c4e326`.

**No se hizo:**
- invocaciones, subagentes, Architect, sondas, pilotos, sesiones nuevas, fixtures, remotos, pruebas nuevas ni repetidas, tooling ni esquemas operativos;
- tocar V1-V6, sus paquetes, los registros previos, el Discovery, el mandato, WORKFLOW, AUTOMATION_PLAN, LIFECYCLE ni ninguna superficie ajena.

Las trazas de V7 (§§8.6-8.8, E.3.0, E.6, Anexo F) son **análisis del diseño**, no ensayos.

**Validación de esta entrega:** allowlist, enlaces, columnas de las tablas, diff V6→V7 revisado por bloques y `git diff --check`; resultado en el cuerpo del
commit. La CI de este commit se informa al Coordinator.

## 19. F0 — Proposal V8 (corrección de A62-V7-01..03)

**CI de la Proposal V7** (`3d7ec77f07d374704f2bc0b888a07bb182c32203`): corrida **36939524509**, attempt 1, event `push`, rama
`architecture/portabilidad-coordinador-principal`, head_sha exacto, `completed/success`. Jobs: Tests (Domain + Application) (110627506125), UI Tests
(110627506270), Build UI (110627506378) y Build Plugin without AutoCAD (110627841717), los cuatro `success`. MEASURED por la sesión. Acredita solo
`3d7ec77f`.

**Orden recibida:** «I-62 — PROPOSAL V8 / NARROW ARCHITECT CORRECTION», texto pegado sin archivo de origen en `D:\IDs`. 5 433 bytes en UTF-8, SHA-256
`e0a2b496c12d71e5325268bd548c668af4bf48ed934fdf23b048aaa537e5003e`, fuera del repositorio. Registro recuperable en
`docs/initiatives/I-62-architect-review-v7.md`. El texto literal del dictamen del Architect de V7 no se recibió (MEASURED: no está en `D:\IDs` ni en el
repositorio).

**Apertura del tramo** (MEASURED, 23:25Z):
- `origin/main` = `819955d6…`, sin rebase;
- rama de I-62 en `3d7ec77f…` = remoto, árbol limpio.

**Revalidación antes del commit** (MEASURED, 23:30Z):
- `origin/main` y la rama de I-62 sin cambios;
- I-52 `51a66245` e I-63 `dddc215f`, sin commits nuevos;
- I-64 `e87ba32c..9e3d289a`: 1 commit, Proposal V5 de I-64, solo archivos propios;
- **ningún cruce** con superficies de I-62.

**Identidad del contenido entregado** (blob calculado con `git hash-object` antes del commit; el commit lo da el recibo):
- `docs/initiatives/I-62-proposal-v8.md` → `667b59d7a433130e20a42bbc13d3e6f564350049`;
- `docs/initiatives/I-62-architect-review-v7.md` → `40a6d5205ff3edef9c781d69c4c9c116b76774a9`.

**No se hizo:**
- invocaciones, subagentes, Workers, Controllers, Architect, sondas, pilotos, sesiones nuevas, fixtures, remotos, pruebas, tooling ni esquemas operativos;
- tocar V1-V7, sus paquetes, los registros previos, el Discovery, el mandato, las normas compartidas ni ninguna superficie ajena.

Las trazas de V8 (§§8.8-8.9, Anexo F) son **análisis del diseño**, no ensayos.

**Validación de esta entrega:** allowlist, enlaces, columnas de las tablas, diff V7→V8 revisado por bloques y `git diff --check`; resultado en el cuerpo del
commit. La CI de este commit se informa al Coordinator.

## 20. F0 — Proposal V9 (requisito R62-AUTO-01..20)

**CI de la Proposal V8** (`d66463a5b04f79911580e2ef6e6f67263b51b5c3`): corrida **36941298033**, attempt 1, event `push`, rama
`architecture/portabilidad-coordinador-principal`, head_sha exacto, `completed/success`. Jobs: UI Tests (110633148054), Tests (Domain + Application)
(110633148232), Build UI (110633148314) y Build Plugin without AutoCAD (110633713878), los cuatro `success`. MEASURED por la sesión; el Coordinator la cita en
su orden. Acredita solo `d66463a5`.

**Orden recibida:** «I-62 — PROPOSAL V9 / AUTONOMOUS ROLE ORCHESTRATION», texto pegado sin archivo de origen en `D:\IDs`. 14 888 bytes en UTF-8, SHA-256
`26eb3bd23979c437beaae7779e8c452e7109345aa3e4f679bc7b5a139214e3d9`, fuera del repositorio. Registro recuperable en
`docs/initiatives/I-62-coordinator-requirement-auto.md`.

**Apertura del tramo** (MEASURED, 23:40Z): `origin/main` = `819955d6…`, sin rebase; rama de I-62 en `d66463a5…` = remoto, árbol limpio.

**Revalidación antes del commit** (MEASURED, 23:45Z):
- `origin/main` y la rama de I-62 sin cambios;
- I-52 `51a66245`, sin commits nuevos;
- I-63 `dddc215f..fb3c8788`: Proposal V3 de I-63;
- I-64 `9e3d289a..5570b924`: Freeze de la Proposal V5 de I-64 y F0 COMPLETE;
- solo archivos propios; **ningún cruce** con superficies de I-62.

**AUTONOMY_GAP observados durante el desarrollo de I-62** (R62-AUTO-18). Son fricción real, no ensayos. Detalle y triaje en la Proposal V9 §20.11:

| Id | Hecho (MEASURED salvo indicación) | Transporte usado |
|---|---|---|
| GAP-01 | Las revisiones del Architect de V6 y V7 no pudieron invocarse desde la sesión principal. I-61 no tiene contrato de invocación del Architect. `claude-cli` está NOT_AUTHENTICATED (Discovery). `codex-cli` como Architect no tiene invocación medida, y su huella está pendiente (OD-2) | el Owner abrió otra sesión y trajo la orden del Coordinator |
| GAP-02 | El dictamen literal del Architect de V6 y de V7 no llegó a la sesión autora (registros `I-62-architect-review-v6.md` y `-v7.md`) | resumen del Coordinator, pegado |
| GAP-03 | Todas las disposiciones del Coordinator (C62-F0-01..26 y las órdenes de V7, V8 y V9) llegaron pegadas por el Owner | Owner |
| GAP-04 | Parte de los hechos de continuación de la sesión autora vivía en su memoria de proyecto y en la prosa de `next_action` (INFERENCE, a partir de la práctica de la sesión) | memoria de sesión |
| GAP-05 | Una sesión de Claude Code abierta en la carpeta de trabajo de la sesión autora carga su memoria de proyecto; se avisó al Owner al recibir la orden de revisión del Architect de V6 | aviso manual |
| GAP-06 | El Owner volvió a pegar la revisión de V4 después de ejecutada; sin estado de orquestación, la sesión comprobó a mano que no había trabajo nuevo | comprobación manual |

**Identidad del contenido entregado** (blob calculado con `git hash-object` antes del commit; el commit lo da el recibo):
- `docs/initiatives/I-62-proposal-v9.md` → `831e3a6a87a242c872cfa0755f73e282f6c043c8`;
- `docs/initiatives/I-62-coordinator-requirement-auto.md` → `fcf941669782ccbfc1e32efa7343b2be16dd6ac4`.

**No se hizo:**
- invocaciones, subagentes, Workers, Controllers, Architect, sondas, pilotos, sesiones nuevas, fixtures, remotos, pruebas, tooling ni esquemas en producción;
- tocar V1-V8, sus paquetes, los registros previos, el Discovery, el mandato, las normas compartidas ni ninguna superficie ajena.

Las trazas de V9 (§20 y Anexos D.8 y F.8) son **análisis del diseño**, no ensayos.

**Validación de esta entrega:** allowlist, enlaces, columnas de las tablas, diff V8→V9 revisado y `git diff --check`; resultado en el cuerpo del commit. La CI
de este commit se informa al Coordinator.

## 21. F0 — Invocación acotada del Architect de la Proposal V9 (decisión del Owner)

**CI de la Proposal V9** (`b0725114e60319079c3abacf10542743ebb9303c`): corrida **36942576135**, attempt 1, event `push`, rama
`architecture/portabilidad-coordinador-principal`, head_sha exacto, `completed/success`. Jobs: Build UI (110637249416), Tests (Domain + Application)
(110637249662), UI Tests (110637249697) y Build Plugin without AutoCAD (110637654589), los cuatro `success`. MEASURED por la sesión. Acredita solo
`b0725114`.

**Decisión recibida:** «OWNER DECISION — BOUNDED ARCHITECT INVOCATION FOR I-62 V9», texto pegado en la conversación sin archivo de origen. 2 360 bytes en UTF-8,
SHA-256 `a28332e5519434da4718fe050df9b31e4821129267392e50feb6f8714582fab7`, custodiado fuera del repositorio con la transcripción de la sesión. La orden de
revisión del Coordinator que el Owner pegó antes (2 825 bytes, SHA-256 `4f7a6c34…`) va literal dentro de `prompt.md`.

**Invocación** (MEASURED; detalle y tablas en
[`I-62-architect-v9/R20261002T000511Z-ba10/README.md`](I-62-architect-v9/R20261002T000511Z-ba10/README.md)):
- **Condiciones previas:** todas acreditadas antes de invocar:
  - binario `codex-cli 0.159.2` con SHA-256;
  - autenticación ChatGPT existente;
  - `config.toml` con el mismo hash antes y después;
  - read-only, y `gpt-6.1-sol`/`high` RUNTIME_OBSERVED en el `turn_context`;
  - proceso e hilo nuevos;
  - clon limpio `D:\r62-arch-v9` en `b0725114`;
  - `memories` desactivado y sin transcripción del autor.
- **Ejecución:**
  - una sola invocación, de 00:06:09Z a 00:12:18Z, salida 0 con `turn.completed`;
  - consumo: entrada 2 395 365 tokens (2 153 600 en caché) y salida 16 647 (7 167 de razonamiento);
  - 23 comandos de lectura, sin avisos de límite.
- **Después:** clon intacto, configuración sin cambio y ningún proceso vivo.
- **Contexto inyectado:** instrucciones base, mensajes developer de skills y multiagente, `AGENTS.md` del clon y el prompt. El `world_state` con prefijos
  aprobados del host no es visible al modelo. Hubo un intento fallido de cargar el perfil de PowerShell.
- **Custodia:** `prompt.md`, `schema.json` y `output.json` se versionan byte a byte; `events.jsonl` y el log de sesión, solo por SHA-256.

**Resultado:** BLOCKED — OWNER DECISION, con A62-V9-01..06 REQUIRED y O01/O02 OPTIONAL, propuestos para ratificación. Registro:
[`I-62-architect-review-v9.md`](../../initiatives/I-62-architect-review-v9.md). El Owner **no** llevó el prompt ni el resultado entre sesiones: la sesión autora
lanzó el proceso e ingirió la salida estructurada. Es una observación, no un ensayo de FX-06 ni un PASS. Las órdenes del Owner y del Coordinator siguieron
llegando pegadas (GAP-03).

**AUTONOMY_GAP nuevos** (R62-AUTO-18). Son hechos para el triaje; la sesión no cambia la Proposal:

| Id | Hecho (MEASURED) | Efecto |
|---|---|---|
| GAP-07 | La cabecera de la invocación dio `AGENTS.md` como insumo canónico sin neutralizar su «Lectura inicial». El Architect siguió esa orden y leyó 100 líneas de `docs/HANDOFF.md`, `README.md` y `docs/ARCHITECTURE.md`, fuera de la lista | la corrida no se puede acreditar como limpia y su validez queda para el Owner. Una invocación futura debe incluir esos archivos como insumos o excluir de forma explícita las lecturas iniciales de `AGENTS.md` |
| GAP-08 | El Architect no pudo acreditar desde dentro su modelo, effort ni hilo (UNKNOWN en `output.json`). La identidad RUNTIME_OBSERVED solo existe en el log del runtime, que lee quien invoca | la identidad del revisor en un `review-result` no puede ser autodeclarada: tiene que venir de la medición de la invocación |

**Revalidación antes del commit** (MEASURED, 00:20Z):
- `origin/main` = `819955d6…`; rama de I-62 en `b0725114…` = remoto;
- I-52 `51a66245..04692eb7`: 2 commits (§254-255 de I-52), solo archivos propios;
- I-63 `fb3c8788..658b35ad`: 4 commits (Architect R3 AGREED, Freeze de la Proposal V3 y A-1), solo archivos propios;
- I-64 `5570b924..0a3fec30`: 2 commits (F1-T0/T1), solo archivos propios;
- **ningún cruce** con superficies de I-62.

**No se hizo:**
- una segunda invocación, Workers, Controllers, subagentes, sondas, pilotos, autenticación ni cambios de configuración;
- la Proposal V10, ni corregir, disponer, ratificar o rebajar hallazgos;
- decidir OD-6 o la validez de la corrida;
- tocar V1-V9, sus paquetes, los registros previos, el Discovery, el mandato, las normas compartidas o cualquier superficie ajena.

**Validación de esta entrega:** allowlist (superficies propias de I-62), enlaces, columnas de las tablas, hashes de los archivos custodiados y
`git diff --check`; resultado en el cuerpo del commit. La CI de este commit se informa al Owner.

## 22. F0 — Proposal V10 (corrección de A62-V9-01..06, GAP-07 y GAP-08)

**CI del commit de custodia** (`08509a5592e6958dcea0b5e6d9b01ee851a15b01`, §21): corrida **36945699248**, attempt 1, event `push`, rama
`architecture/portabilidad-coordinador-principal`, head_sha exacto, `completed/success`. Jobs: Tests (Domain + Application) (110647211240), Build UI
(110647211343), UI Tests (110647211502) y Build Plugin without AutoCAD (110647777343), los cuatro `success`. MEASURED por la sesión. Acredita solo
`08509a55`.

**Orden recibida:** «I-62 — PROPOSAL V10 / ARCHITECT V9 REQUIRED CORRECTIONS», texto pegado sin archivo de origen. 8 170 bytes en UTF-8, SHA-256
`c6d1e267b8f239b114284c91dd64a06450eb4cf14e24c7c584cc9864f9522b64`, fuera del repositorio. Registro recuperable en
`docs/initiatives/I-62-architect-review-v9-disposition.md`.

**Apertura del tramo** (MEASURED en el primer comando del tramo, tras recibir la orden a las 00:33Z): `origin/main` = `819955d6…`, sin rebase; rama de I-62 en `08509a55…` = remoto, árbol limpio.

**Cierre efectivo previsto para la revisión de V10** (paquete §0). Calculado sobre los blobs de la rama, que en estas rutas son iguales a `origin/main`
(`git diff --quiet origin/main HEAD` sobre ellas):

| Archivo | Blob | Origen de la obligación |
|---|---|---|
| `AGENTS.md` | `dd0d5a2d9ac3d3d8502ef7a556c8a5f1d62c9c29` | instrucción automática del runtime; «Leer primero» (pasos 1-4) |
| `docs/HANDOFF.md` | `f401093a27bb3125816b2d2478887a1cb4dc5c58` | AGENTS, paso 1 |
| `README.md` | `50bd744a6407c1895ab20ae86ed4d97b4e0eab85` | AGENTS, paso 2 |
| `docs/ARCHITECTURE.md` | `4402ff89146c1632064d45e66b94196289c55b20` | AGENTS, paso 3 |
| `docs/context-packs/README.md` | `77b1c7b4e3cf781abad9ea80938d7dc08b2b31c8` | AGENTS, paso 3 (enlace a los Context Packs) |
| `docs/context-packs/documentation-governance.md` | `fff168de38a785b3474d7f578a383740c6c65e3c` | AGENTS, paso 3, y `context_packs` del contrato de I-62 |
| `docs/ROADMAP.md` | `58da0b9c30741a1619d91c80f59bd757c88a1aff` | índice de packs («Base obligatoria») y `required_docs` del pack |
| `docs/FOUNDATIONS.md` | `a6162ffb46f595e94e199d04c8df3bc2bd36455c` | `required_docs` del pack |
| `docs/initiatives/README.md` | `f3b98a0c9375d71827e51f0cfd84a1f16f858802` | `required_docs` del pack |
| `docs/initiatives/PROMPT_TEMPLATES.md` | `0e3ed262add5154b975dab1931f729bcae2e31ea` | `required_docs` del pack |
| `CLAUDE.md` | `793489779558234abbd299f2aaadc32b8e829e58` | solo para adapters de Claude; su «Lectura inicial» no añade rutas |

En los archivos añadidos no se encontró ninguna otra obligación de lectura, con lo que el punto fijo se alcanza ahí. Las instrucciones de las plantillas de
PROMPT_TEMPLATES (líneas 183, 216 y 246) se dirigen a otros prompts. WORKFLOW, LIFECYCLE, AUTOMATION_PLAN y el contrato ya son canónicos o entran por el
índice de packs; el contrato cambia en este commit y su blob lo fija el cálculo sobre el commit exacto. El invocador recalcula el cierre sobre el commit
exacto antes de lanzar.

**Revalidación antes del commit** (MEASURED, 00:56Z):
- `origin/main` y la rama de I-62 sin cambios;
- I-52 `04692eb7..4d7fa61d`: 1 commit (§256 de I-52), solo archivos propios;
- I-63 `658b35ad..1013449d`: 2 commits (contrato de gate de G1 y su RED), solo archivos propios;
- I-64 `0a3fec30..e0587355`: 1 commit (STOP P-03 de F1-T1-MODEL), solo archivos propios;
- **ningún cruce** con superficies de I-62.

**Identidad del contenido entregado** (blob calculado con `git hash-object` antes del commit; el commit lo da el recibo):
- `docs/initiatives/I-62-proposal-v10.md` → `58f88fc602d0d4eaf3900a3514259da6b94faba2`;
- `docs/initiatives/I-62-architect-review-v9-disposition.md` → `865cf0d822d8184e2156e3f5e9196146917561d9`;
- `docs/initiatives/I-62-architect-package-v10.md` → `4d9df7edb94d44633361621623a8eb33131d87d2`.

**Defecto propio corregido** (MEASURED con PyYAML `safe_load`). `docs/automation/state/I-62.yml` no era YAML válido desde `3d7ec77f` (V7). Dos valores
sin comillas de `execution_context` contenían «: » (`Architect: V6 …` y `(Frozen: NO; …`), y el parser fallaba en la línea 19. Pasan a « = », sin cambio de
significado. `execution_context` es descriptivo y ningún lector del protocolo lo consume (B.8.1), pero el archivo debe parsear. Ahora parsea, con
`automation_state` intacto en sus nueve campos.

**No se hizo:**
- invocaciones de modelos, subagentes, Workers, Controllers, Architect, sondas, pilotos, sesiones nuevas, fixtures, remotos, pruebas, tooling ni esquemas en
  producción;
- tocar V1-V9, sus paquetes, los registros previos (incluido el de la revisión de V9), el Discovery, el mandato, las normas compartidas ni ninguna superficie
  ajena;
- decidir OD-6 ni declarar AGREED o Freeze.

Las trazas de V10 (§20 y Anexos D.8 y F.8) son **análisis del diseño**, no ensayos.

**Validación de esta entrega:** allowlist, enlaces, columnas de las tablas, diff V9→V10 revisado y `git diff --check`; resultado en el cuerpo del commit. La
CI de este commit se informa al Owner y al Coordinator.

## 23. F0 — Revisión formal limpia del Architect de la Proposal V10 (decisión del Owner)

**CI de la Proposal V10** (`41ded86e6494e3d43e07ce97de9eedf4710a631f`): corrida **36948649343**, attempt 1, event `push`, rama
`architecture/portabilidad-coordinador-principal`, head_sha exacto, `completed/success`. Jobs: UI Tests (110656401671), Tests (Domain + Application)
(110656401731), Build UI (110656401732) y Build Plugin without AutoCAD (110656808482), los cuatro `success`. MEASURED por la sesión. Acredita solo
`41ded86e`.

**Decisión recibida:** «OWNER DECISION — CLEAN ARCHITECT REVIEW OF I-62 PROPOSAL V10», texto pegado sin archivo de origen. 5 714 bytes en UTF-8, SHA-256
`619b4fada8f9cb682b8e2e95013bbcbb6c221020a964d23303b62ee78f98dfb2`; va literal dentro de `prompt.md`. La excepción de `dotnet test` (1 158 bytes, SHA-256
`68cfe8dc…`) está transcrita en el README de la corrida, §2.

**Invocación** (MEASURED; detalle en
[`I-62-architect-v10/R20261002T010757Z-ef59/README.md`](I-62-architect-v10/R20261002T010757Z-ef59/README.md), `runtime-evidence.json` y
`read-audit.json`):
- **Antes:**
  - `codex-cli 0.159.2` (SHA-256 `fcd5eafe…`) y autenticación existente;
  - `config.toml` con hash `40c27b57…` antes y después;
  - `gpt-6.1-sol`/`high` solicitados y RUNTIME_OBSERVED en los dos `turn_context`; sandbox de solo lectura;
  - hilo nuevo `01a0fa29-0f9e-7321-88d3-a62f3c7bfe15`;
  - clon limpio `D:\r62-arch-v10` en `41ded86e`, sin memoria del autor ni transcripción;
  - cierre de 37 insumos canónicos y 9 transitivos custodiado (`closure.json`);
  - sin colisión de escritores.
- **Ejecución:** una sola, de 01:09:49Z a 01:18:31Z, salida 0. Consumo: entrada 3 698 817 tokens (3 431 040 en caché) y salida 19 561. El runtime compactó
  el contexto a las 01:15:05Z dentro del mismo hilo.
- **Después:** clon limpio, configuración sin cambio y ningún proceso vivo. 41 comandos de lectura, todos sobre las 46 rutas del cierre, sin lecturas fuera de
  él y sin `dotnet test`.

**Resultado:** CHANGES REQUIRED, con A62-V10-01..04. Registro: [`I-62-architect-review-v10.md`](../../initiatives/I-62-architect-review-v10.md).

**AUTONOMY_GAP nuevo** (R62-AUTO-18). Es un hecho para el triaje; la sesión no cambia la Proposal:

| Id | Hecho (MEASURED) | Efecto |
|---|---|---|
| GAP-09 | La consola del runtime de `codex-cli` no conservó los caracteres no ASCII al leer el objeto: ≤ y ≥ → «=», ≠ → «?», → → U+001A, ⇒, ⇔ y ∈ perdidos, letras acentuadas → U+FFFD. La cabecera de la invocación no forzó UTF-8 en esa consola. La corrida de V9 ya tenía 8 781 U+FFFD en su `events.jsonl` (MEASURED ahora, fuera del repositorio; no se registró en §21) | el revisor no leyó el objeto byte a byte; en particular, «`review_rounds` ≤ `logical_requests`» (líneas 1295 y 1854) se leyó como «=», que es la premisa de A62-V10-02. Una invocación futura debe forzar la salida UTF-8 (o leer por un canal sin conversión) y comprobar la fidelidad de la lectura antes de aceptar el resultado |

**Revalidación antes del commit** (MEASURED, 01:24Z):
- `origin/main` = `819955d6…` y la rama de I-62 en `41ded86e…`, sin cambios;
- I-52 `4d7fa61d`, sin commits nuevos;
- I-63 `1013449d..29fad150`: 3 commits (cadena de G1);
- I-64 `e0587355..24074abb`: 3 commits (F1-T1-MODEL);
- **ningún cruce** con superficies de I-62.

**No se hizo:**
- reintentos, una segunda invocación, Workers, Controllers, subagentes, sondas, pilotos, autenticación ni cambios de configuración;
- editar el clon, la Proposal V11, ni corregir, disponer, ratificar o rebajar hallazgos;
- decidir OD-6 ni la validez de la revisión;
- tocar V1-V10, sus paquetes, los registros previos, el Discovery, el mandato, las normas compartidas o cualquier superficie ajena.

**Validación de esta entrega:** allowlist (superficies propias de I-62), enlaces, columnas de las tablas, hashes de los archivos custodiados, YAML del estado y
`git diff --check`; resultado en el cuerpo del commit. La CI de este commit se informa al Owner.

## 24. F0 — Proposal V11 (corrección de A62-V10-01, -03, -04 y requisito R62-FIDELITY-01..06)

**CI del commit de custodia** (`c2e2ecede5810dbd1c7fb108a7e9742c0663ee46`, §23): corrida **36950848216**, attempt 1, event `push`, rama
`architecture/portabilidad-coordinador-principal`, head_sha exacto, `completed/success`. Jobs: Build UI (110663230205), UI Tests (110663230382), Tests (Domain +
Application) (110663230430) y Build Plugin without AutoCAD (110663727615), los cuatro `success`. MEASURED por la sesión. Acredita solo `c2e2eced`.

**Orden recibida:** «I-62 — PROPOSAL V11 / V10 ARCHITECT CORRECTIONS + INPUT FIDELITY», texto pegado sin archivo de origen. 10 559 bytes en UTF-8, SHA-256
`e97c6a5bdc7d26056f518528ed046e087c099985af9977f964873340790d3a44`, fuera del repositorio. Registro recuperable en
`docs/initiatives/I-62-architect-review-v10-disposition.md`.

**Apertura del tramo** (MEASURED en el primer comando del tramo, tras recibir la orden a las 06:12Z): `origin/main` = `819955d6…`, sin rebase; rama de I-62
en `c2e2eced…` = remoto, árbol limpio.

**Fidelidad del prompt en la invocación de V10** (MEASURED ahora sobre el log de sesión `339a1f25…`, fuera del repositorio). El mensaje que recibió el revisor
es igual a `prompt.md` salvo el salto de línea final, que es la normalización permitida por §20.3.3. Tiene los mismos 68 caracteres no ASCII y ningún
U+FFFD. La degradación de GAP-09 estuvo en el camino de lectura de las herramientas, no en el prompt.

**Corpus de caracteres de V11** (MEASURED): `docs/initiatives/I-62-proposal-v11.md` contiene 36 caracteres no ASCII distintos:
`§ª«·»¿Ñ×Úáéíñóúü—…→↔⇒⇔∅∈∉−∖∧∩∪≠≤≥⊆⊇✓`. Es el mínimo que debe cubrir la prueba de fidelidad de la próxima revisión, sumado al del resto del cierre.

**Revalidación antes del commit** (MEASURED, 06:23Z):
- `origin/main` = `819955d6…` y la rama de I-62 en `c2e2eced…`, sin cambios;
- I-52 `4d7fa61d..fb6b5648`: 2 commits;
- I-63 `29fad150..b5ee157d`: 3 commits (corrección 1 de G1);
- I-64 `24074abb..b2db5326`: 1 commit;
- **ningún cruce** con superficies de I-62.

**Identidad del contenido entregado** (blob calculado con `git hash-object` antes del commit; el commit lo da el recibo):
- `docs/initiatives/I-62-proposal-v11.md` → `3e8fa9d8eb8a875069224e8ed7fd5850145e4d0e`;
- `docs/initiatives/I-62-architect-review-v10-disposition.md` → `3328d1f5e11a1e3903cfd8ee3624f9d1d966efd8`;
- `docs/initiatives/I-62-architect-package-v11.md` → `10fe74796ac94cbeaf1945ee3e47a60cb8f39ee4`.

**No se hizo:**
- invocaciones de modelos, subagentes, Workers, Controllers, Architect, sondas, pilotos, sesiones nuevas, fixtures, remotos, pruebas, tooling ni esquemas en
  producción;
- tocar V1-V10, sus paquetes, los registros previos (incluido el de la revisión de V10), el Discovery, el mandato, las normas compartidas ni ninguna
  superficie ajena;
- cambiar la semántica del presupuesto por A62-V10-02, decidir OD-6 ni declarar AGREED o Freeze.

Las trazas de V11 (§20 y Anexos D.8 y F.8) son **análisis del diseño**, no ensayos.

**Validación de esta entrega:** allowlist, enlaces, columnas de las tablas, diff V10→V11 revisado, YAML del estado y `git diff --check`; resultado en el
cuerpo del commit. La CI de este commit se informa al Owner y al Coordinator.

## 25. F0 — Revisión formal limpia del Architect de la Proposal V11 (decisión del Owner)

**CI de la Proposal V11** (`26127a69a7dbc1324566b081381cd11db6b20f35`): corrida **36973392633**, attempt 1, event `push`, rama
`architecture/portabilidad-coordinador-principal`, head_sha exacto, `completed/success`. Jobs: Tests (Domain + Application) (110731947026), Build UI
(110731947103), UI Tests (110731947117) y Build Plugin without AutoCAD (110732443330), los cuatro `success`. MEASURED por la sesión. Acredita solo
`26127a69`.

**Decisión recibida:** «OWNER DECISION — FORMAL CLEAN ARCHITECT REVIEW OF I-62 PROPOSAL V11», texto pegado sin archivo de origen. 6 415 bytes en UTF-8,
SHA-256 `f412b8ec87ea7f56a615a328fd4061933164ccce962dd0f204f498c15b5091c4`; va literal dentro de `prompt.md`. La excepción de `dotnet test` (662 bytes,
SHA-256 `9f110074…`) está transcrita en el README de la corrida, §2.

**Invocación** (MEASURED; detalle en
[`I-62-architect-v11/R20261002T140232Z-cdc5/README.md`](I-62-architect-v11/R20261002T140232Z-cdc5/README.md), con `closure.json`,
`input-fidelity-preflight.json`, `input-fidelity-postrun.json`, `runtime-evidence.json` y `read-audit.json`):
- **Antes:**
  - `codex-cli 0.159.2` (SHA-256 `fcd5eafe…`) y autenticación existente;
  - `config.toml` con hash `40c27b57…` antes y después;
  - clon limpio `D:\r62-arch-v11` en `26127a69`, sin colisión de escritores;
  - cierre de 48 insumos canónicos y 9 transitivos, cada uno con su blob y su SHA-256;
  - **preflight de fidelidad FAITHFUL_NORMALIZED por el mismo camino** (`codex sandbox` → el `pwsh` del runtime), sobre los 61 caracteres no ASCII del
    corpus, con un control negativo que detecta la pérdida.
- **Ejecución:** una sola, de 14:11:32Z a 14:31:04Z, salida 0. `gpt-6.1-sol`/`high`/read-only RUNTIME_OBSERVED en los dos `turn_context`, con una
  compactación a las 14:20:00Z. Consumo: entrada 3 807 092 tokens (3 479 552 en caché) y salida 23 956.
- **Después:**
  - clon limpio, configuración sin cambio y ningún proceso vivo;
  - prompt recibido fiel;
  - 96 comandos de lectura por las formas acreditadas, todos sobre rutas del cierre, sin lecturas directas en el `pwsh` exterior y sin `dotnet test`.
- **Fidelidad tras la corrida:** DEGRADED_BOUNDED, sin degradación de caracteres, con dos tramos estructurales acotados. Las 42 premisas son fieles y ningún
  hallazgo queda INVALID_PREMISE.

**Resultado:** CHANGES REQUIRED, con A62-V10-03 (sigue abierto) y A62-V11-01 (nuevo); CLOSED A62-V10-01, A62-V10-04 y A62-V9-04. Registro:
[`I-62-architect-review-v11.md`](../../initiatives/I-62-architect-review-v11.md).

**Hechos del transporte medidos en este tramo** (para el triaje; la sesión no cambia la Proposal):

| Id | Hecho (MEASURED) | Efecto |
|---|---|---|
| GAP-09 (actualización) | en el sandbox de `codex-cli`, `pwsh` corre en ConstrainedLanguage con la salida de consola en la página 850. `[Console]::OutputEncoding` no se puede asignar, `chcp` no cambia la codificación ya fijada y la consola del sandbox no hereda la página del padre. `cmd /c type` (los bytes atraviesan el `pwsh` sin conversión) y un `pwsh` anidado tras `chcp 65001` sí son fieles | causa medida de GAP-09. Las formas acreditadas eliminaron la degradación de caracteres en esta revisión |
| GAP-10 | la captura de `codex exec` trunca el centro de las salidas muy grandes. Una lectura completa de `docs/HANDOFF.md` (438 341 caracteres) perdió las líneas 4639-4672 y unió dos líneas. El preflight por `codex sandbox` no puede verlo, porque no pasa por esa captura | un tramo estructural acotado, sin efecto en ninguna premisa. Una invocación futura puede limitar el tamaño de cada lectura o exigir rangos para los archivos grandes; la comprobación tras la corrida sigue siendo la decisiva |
| — | Git Bash puede reescribir como rutas los «/c» de un argumento; el prompt se pasó con `MSYS_NO_PATHCONV=1` | prompt recibido fiel (comprobado en el log de sesión) |

**Revalidación antes del commit** (MEASURED, 14:38Z):
- `origin/main` = `819955d6…` y la rama de I-62 en `26127a69…`, sin cambios;
- I-52 `fb6b5648`, sin commits nuevos;
- I-63 `b5ee157d..a2d2b0a6`: 6 commits;
- I-64 `b2db5326..8d9a0c6e`: 6 commits;
- **ningún cruce** con superficies de I-62.

**No se hizo:**
- reintentos, una segunda invocación, Workers, Controllers, subagentes, pilotos, autenticación ni cambios de configuración. El preflight usó
  `codex sandbox`, que no invoca ningún modelo;
- editar el clon, la Proposal V12, ni corregir, disponer, ratificar o rebajar hallazgos;
- decidir OD-6;
- tocar V1-V11, sus paquetes, los registros previos, el Discovery, el mandato, las normas compartidas o cualquier superficie ajena.

**Validación de esta entrega:** allowlist (superficies propias de I-62), enlaces, columnas de las tablas, hashes de los archivos custodiados, YAML del estado y
`git diff --check`; resultado en el cuerpo del commit. La CI de este commit se informa al Owner.

## 26. F0 — Proposal V12 (corrección estrecha de A62-V10-03 residuo y A62-V11-01)

**CI del commit de custodia** (`902c2cbad553f0927982fcd6561c66f96262a311`, §25): corrida **37021436558**, attempt 1, event `push`, rama
`architecture/portabilidad-coordinador-principal`, head_sha exacto, `completed/success`. Jobs: Tests (Domain + Application) (110885234896), Build UI
(110885235063), UI Tests (110885235206) y Build Plugin without AutoCAD (110886024403), los cuatro `success`. MEASURED por la sesión. Acredita solo
`902c2cba`.

**Orden recibida:** «I-62 — PROPOSAL V12 / FINAL NARROW ARCHITECT CORRECTION», texto pegado sin archivo de origen. 6 591 bytes en UTF-8, SHA-256
`79fea3d7fbf8ad7f121050bc7e1d79410d9831dd30d5f885cd6ddfe31ef4c4c9`, fuera del repositorio. Registro recuperable en
`docs/initiatives/I-62-architect-review-v11-disposition.md`.

**Apertura del tramo** (MEASURED en el primer comando del tramo, tras recibir la orden a las 14:55Z): `origin/main` = `819955d6…`, sin rebase; rama de I-62
en `902c2cba…` = remoto, árbol limpio.

**Alcance estrecho:** el diff V11→V12 toca solo §20.3.3, §20.5.1, §20.6, §20.11, B.8.8, B.10.1, F.8, C-38, C-40, C-42, G.2 y la cabecera (114 inserciones
y 56 supresiones). Las demás secciones quedan sin cambio.

**Revalidación antes del commit** (MEASURED, 15:00Z):
- `origin/main` = `819955d6…` y la rama de I-62 en `902c2cba…`, sin cambios;
- I-52 `fb6b5648`, sin commits nuevos;
- I-63 `a2d2b0a6..4a6c2d88`: 1 commit;
- I-64 `8d9a0c6e..747ead04`: 1 commit;
- **ningún cruce** con superficies de I-62.

**Identidad del contenido entregado** (blob calculado con `git hash-object` antes del commit; el commit lo da el recibo):
- `docs/initiatives/I-62-proposal-v12.md` → `320cecc9bd1b68112509e510b97c768b6d505ba2`;
- `docs/initiatives/I-62-architect-review-v11-disposition.md` → `9570cceaf7059aaf438b91aa769c906e4a136ce8`;
- `docs/initiatives/I-62-architect-package-v12.md` → `f60873d15f4b62e1313bb03c9136076acbd08012`.

**No se hizo:**
- invocaciones de modelos, subagentes, Workers, Controllers, Architect, sondas, pilotos, sesiones nuevas, fixtures, remotos, pruebas, tooling ni esquemas en
  producción;
- tocar V1-V11, sus paquetes, los registros previos, el Discovery, el mandato, las normas compartidas ni ninguna superficie ajena;
- reabrir un hallazgo cerrado, decidir OD-6 ni declarar AGREED o Freeze.

Las trazas de V12 son **análisis del diseño**, no ensayos.

**Validación de esta entrega:** allowlist, enlaces, columnas de las tablas, diff V11→V12 revisado, YAML del estado y `git diff --check`; resultado en el
cuerpo del commit. La CI de este commit se informa al Owner y al Coordinator.

## 27. F0 — Revisión formal limpia del Architect de la Proposal V12 (autorización del Owner y del Coordinator)

**CI de la Proposal V12** (`d0947c5d58280f8b0bfe2808dd8b7ff79e9f76bb`, §26): corrida **37024119231**, attempt 1, event `push`, rama
`architecture/portabilidad-coordinador-principal`, head_sha exacto, `completed/success`. Jobs: Build UI (110894292229), Tests (Domain + Application)
(110894292758), UI Tests (110894293118) y Build Plugin without AutoCAD (110895062040), los cuatro `success`. MEASURED por la sesión. Acredita solo
`d0947c5d`.

**Autorización recibida:** «OWNER / COORDINATOR AUTHORIZATION — FORMAL CLEAN ARCHITECT REVIEW OF I-62 PROPOSAL V12», texto pegado sin archivo de origen.
6 177 bytes en UTF-8, SHA-256 `deba08bb25684275c89969936642820ac74ab8c0b1d31cb0b26c8595fb8e8796`; va literal dentro de `prompt.md`. La exención de
`dotnet test` (457 bytes, SHA-256 `0e2bd4d8…`) está transcrita en el README de la corrida, §2.

**Invocación** (MEASURED; detalle en
[`I-62-architect-v12/R20261002T151854Z-603d/README.md`](I-62-architect-v12/R20261002T151854Z-603d/README.md), con `closure.json`,
`input-fidelity-preflight.json`, `input-fidelity-postrun.json`, `runtime-evidence.json` y `read-audit.json`):
- **Antes:**
  - `codex-cli 0.159.2` (SHA-256 `fcd5eafe…`) y autenticación existente;
  - `config.toml` con hash `40c27b57…` antes y después;
  - clon limpio `D:\r62-arch-v12` en `d0947c5d`, sin colisión de escritores;
  - cierre de 48 insumos canónicos y 9 transitivos, cada uno con su blob y su SHA-256;
  - **preflight FAITHFUL_NORMALIZED por el mismo camino**, sobre los 61 caracteres no ASCII del corpus. Los cuatro archivos de más de 150 000 bytes se
    probaron y se leyeron solo por rangos de hasta 400 líneas; el control negativo detecta la pérdida.
- **Ejecución:** una sola, de 15:27:42Z a 15:45:22Z, salida 0. `gpt-6.1-sol`/`high`/read-only RUNTIME_OBSERVED en los dos `turn_context`, con una
  compactación a las 15:34:27Z que conservó el prompt completo. Consumo: entrada 3 975 060 tokens (3 633 024 en caché) y salida 26 793.
- **Después:**
  - clon limpio, configuración sin cambio y ningún proceso de la corrida vivo;
  - prompt recibido fiel;
  - 122 comandos de lectura por las formas acreditadas, todos sobre rutas del cierre, sin archivos grandes leídos enteros, sin lecturas directas en el
    `pwsh` exterior y sin `dotnet test`.
- **Fidelidad tras la corrida:** DEGRADED_BOUNDED, sin degradación de caracteres. La captura no se truncó. Lo visible por el modelo se truncó en 4 de 35
  llamadas `exec`, con tres rangos transitivos nunca vistos. El objeto y el paquete fueron visibles enteros, y ningún envoltorio de premisa queda afectado.

**Resultado:** CHANGES REQUIRED. A62-V10-03 CLOSED; A62-V11-01 sigue abierto como residuo (destinos de referencias cruzadas). Registro:
[`I-62-architect-review-v12.md`](../../initiatives/I-62-architect-review-v12.md).

**Hechos del transporte medidos en este tramo** (para el triaje; la sesión no cambia la Proposal):

| Id | Hecho (MEASURED) | Efecto |
|---|---|---|
| GAP-10 (captura) | con rangos de hasta 400 líneas para los archivos grandes, ninguna salida capturada en `events.jsonl` se truncó | la política de rangos protegió la captura |
| GAP-10 (lo visible por el modelo) | el revisor lee con la herramienta `exec`, cuyos límites de salida (del script y de cada comando) truncan lo que recibe el modelo aunque la captura esté completa. Pasó en 4 de 35 llamadas, al agrupar varias lecturas en un solo script, y cada vez con marca explícita «…N tokens truncated…». El preflight por `codex sandbox` no pasa por esa capa | la fidelidad tras la corrida debe medirse sobre las salidas de la herramienta en el log de sesión, no solo sobre `aggregated_output`. Una invocación futura puede limitar también la salida agregada de cada llamada |
| — | las auditorías tras la corrida de V9, V10 y V11 midieron `aggregated_output`. La re-auditoría de V11 con su log de sesión, sin invocación nueva, da 11 de 34 llamadas truncadas y 7 232 líneas pedidas nunca vistas, todas de insumos transitivos o auxiliares; el objeto y el paquete de V11 fueron visibles enteros (`v11-model-visible-reaudit.json`) | corrección de método: la independencia de las premisas de V11 se mantiene. La evidencia de V11 describía el transporte. V9 y V10 no se re-auditan (UNKNOWN). Lo dispone el Owner |
| — | la compactación conserva en el historial de reemplazo el prompt completo y un resumen cifrado. Las lecturas anteriores solo persisten en ese resumen; tras la compactación se volvieron a ver 420 de las 3 129 líneas de V12 | el efecto de la compactación sobre el razonamiento no es observable; las citas coinciden con el texto canónico |

**Revalidación antes del commit** (MEASURED, 16:09Z):
- `origin/main` = `819955d6…` y la rama de I-62 en `d0947c5d…`, sin cambios;
- I-52 `fb6b5648`, sin commits nuevos;
- I-63 `4a6c2d88..58e81b80`: 3 commits;
- I-64 `747ead04..bc24ebef`: 1 commit;
- **ningún cruce** con superficies de I-62.

**No se hizo:**
- reintentos, una segunda invocación, Workers, Controllers, subagentes, pilotos, autenticación ni cambios de configuración. El preflight usó
  `codex sandbox`, que no invoca ningún modelo, y la re-auditoría de V11 solo leyó su log de sesión;
- editar el clon, crear la Proposal V13, ni corregir, disponer, ratificar o rebajar hallazgos;
- modificar la evidencia custodiada de V11: la corrección vive en la carpeta de esta corrida;
- decidir OD-6 ni declarar Freeze;
- tocar V1-V12, sus paquetes, los registros previos, el Discovery, el mandato, las normas compartidas o cualquier superficie ajena.

**Validación de esta entrega:** allowlist (superficies propias de I-62), enlaces, columnas de las tablas, hashes de los archivos custodiados, YAML del estado y
`git diff --check`; resultado en el cuerpo del commit. La CI de este commit se informa al Owner y al Coordinator.

## 28. F0 — Proposal V13 (corrección final del residuo de A62-V11-01)

**CI del commit de custodia** (`4a4ceae712cecd3a0667b3ed3600a042b014e8d4`, §27): corrida **37032389467**, attempt 1, event `push`, rama
`architecture/portabilidad-coordinador-principal`, head_sha exacto, `completed/success`. Jobs: UI Tests (110922172313), Tests (Domain + Application)
(110922172694), Build UI (110922172717) y Build Plugin without AutoCAD (110922948088), los cuatro `success`. MEASURED por la sesión. Acredita solo
`4a4ceae7`.

**Orden recibida:** «I-62 — PROPOSAL V13 / FINAL FIDELITY RESIDUAL», texto pegado sin archivo de origen. 6 335 bytes en UTF-8, SHA-256
`809df4c33ba5544acd69f8e997808f50c48b0bcf132209aca45128cce29e0525`, fuera del repositorio. Registro recuperable en
`docs/initiatives/I-62-architect-review-v12-disposition.md`.

**Apertura del tramo** (MEASURED en el primer comando del tramo, a las 16:35Z): `origin/main` = `819955d6…`, sin rebase; rama de I-62 en `4a4ceae7…` =
remoto, árbol limpio.

**Alcance estrecho:** el diff V12→V13 (141 inserciones y 54 supresiones) toca solo:
- la cabecera, el mapa de la corrección, las fuentes, los cierres preservados y tres referencias a la versión vigente (§11.4, la tabla de retos y §20.13);
- §13 (P-25);
- §20.3.2 (compactación) y §20.3.3 (envoltorio, clausura de dependencias normativas, regla de fidelidad, representación entregada al revisor, GAP-10 y el
  caso de V11 y V12);
- §20.5 (una línea sobre P-25) y §20.11 (GAP-10 y GAP-11);
- B.1, B.8.8 (tres campos e I-S18) y B.10.1;
- C-42, G.1 y G.2.

Las demás secciones quedan sin cambio, incluida la corrección de A62-V10-03, ya cerrada.

**Revalidación antes del commit** (MEASURED, 16:43Z):
- `origin/main` = `819955d6…` y la rama de I-62 en `4a4ceae7…`, sin cambios;
- I-52 `fb6b5648`, sin commits nuevos;
- I-63 `58e81b80..669d8a39`: 1 commit;
- I-64 `bc24ebef..58d1140c`: 1 commit;
- **ningún cruce** con superficies de I-62.

**Identidad del contenido entregado** (blob calculado con `git hash-object` antes del commit; el commit lo da el recibo):
- `docs/initiatives/I-62-proposal-v13.md` → `949c6403a04570dfa7b5dcd1a3509271819dc696`;
- `docs/initiatives/I-62-architect-review-v12-disposition.md` → `3751ca998a908a37022126d23a519ab6c6aec77c`;
- `docs/initiatives/I-62-architect-package-v13.md` → en el recibo del commit (el paquete no lleva su propio blob).

**No se hizo:**
- invocaciones de modelos, subagentes, Workers, Controllers, Architect, sondas, pilotos, sesiones nuevas, fixtures, remotos, pruebas, tooling ni esquemas en
  producción;
- tocar V1-V12, sus paquetes, los registros previos, la evidencia custodiada, el Discovery, el mandato, las normas compartidas ni ninguna superficie ajena;
- reabrir un hallazgo cerrado, decidir OD-6 ni declarar AGREED o Freeze.

Las trazas de V13 son **análisis del diseño**, no ensayos.

**Validación de esta entrega:** allowlist, enlaces, columnas de las tablas, diff V12→V13 revisado, YAML del estado y `git diff --check`; resultado en el
cuerpo del commit. La CI de este commit se informa al Owner y al Coordinator.

## 29. F0 — Revisión formal final del Architect de la Proposal V13 (autorización del Owner y del Coordinator)

**CI de la Proposal V13** (`b337a59fa6879893229096f6e423d441b3c97bc6`, §28): corrida **37036044921**, attempt 1, event `push`, rama
`architecture/portabilidad-coordinador-principal`, head_sha exacto, `completed/success`. Jobs: Tests (Domain + Application) (110934394578), UI Tests
(110934394754), Build UI (110934395060) y Build Plugin without AutoCAD (110935136884), los cuatro `success`. MEASURED por la sesión. Acredita solo
`b337a59f`.

**Autorización recibida:** «OWNER / COORDINATOR AUTHORIZATION — FINAL FORMAL ARCHITECT REVIEW OF I-62 PROPOSAL V13», texto pegado sin archivo de origen.
5 369 bytes en UTF-8, SHA-256 `4e6742b9f7b5884e9555f3e41c195c561a66328cae8be4eea129560c74116ec8`; va literal dentro de `prompt.md`. La exención de
`dotnet test` (302 bytes, SHA-256 `2d0448eb…`) está transcrita en el README de la corrida, §2. **Relanzamiento:** autorizado por el Owner en la
conversación, como respuesta a la pregunta de la sesión tras el fallo previo a la sesión.

**Invocación** (MEASURED; detalle en
[`I-62-architect-v13/R20261002T165850Z-648a/README.md`](I-62-architect-v13/R20261002T165850Z-648a/README.md)):
- **Antes:**
  - `codex-cli 0.159.2` y autenticación existente; `config.toml` con hash `40c27b57…` antes y después;
  - clon limpio `D:\r62-arch-v13` en `b337a59f`, sin colisión de escritores;
  - cierre de 60 insumos canónicos y 9 transitivos;
  - preflight FAITHFUL_NORMALIZED (65 insumos por A y B completas; 4 grandes por rangos de 150 líneas; 61 caracteres no ASCII; control negativo). Una
    primera corrida del preflight dio una diferencia solo en ASCII en una lectura, que no se reprodujo.
- **Lanzamiento fallido** (17:17:28Z): salida 1 por una ruta vacía en el comando del invocador; sin sesión, sin tokens y sin lecturas.
- **Ejecución:** una sesión, de 17:19:27Z a 17:37:15Z, salida 0. `gpt-6.1-sol`/`high`/read-only RUNTIME_OBSERVED, con una compactación a las 17:30:16Z
  que conservó el prompt completo. Consumo: entrada 11 089 843 tokens (10 778 368 en caché) y salida 23 001.
- **Después:** clon limpio, configuración sin cambio y ningún proceso de la corrida vivo; 97 lecturas sobre 36 rutas del cierre, con la política de lectura
  cumplida y sin `dotnet test`.
- **Lo entregado al revisor:** 89 llamadas sin truncamientos; 97 salidas completas; objeto y paquete vistos enteros. DEGRADED_BOUNDED solo por la línea del
  diagnóstico del runtime.
- **Clausuras:** ninguna solapa un tramo degradado. Con la regla literal, las premisas de §20.3.3 y dos de la disposición de V10 quedan en UNKNOWN (32
  referencias sin resolver distintas, desde la profundidad 3).

**Resultado:** CHANGES REQUIRED. A62-V11-01 sigue abierto como residuo (terminalidad incondicional del documento completo). Registro:
[`I-62-architect-review-v13.md`](../../initiatives/I-62-architect-review-v13.md).

**Hechos medidos en este tramo** (para el triaje; la sesión no cambia la Proposal):

| Id | Hecho (MEASURED) | Efecto |
|---|---|---|
| GAP-10 | con un presupuesto de salida por llamada en el prompt (una lectura grande por llamada, menos de ~8 000 tokens), **ninguna** de las 89 llamadas llegó truncada al revisor | la mitigación operativa funcionó; la auditoría sobre lo entregado al revisor siguió siendo la comprobación decisiva |
| — | el primer lanzamiento falló antes de crear sesión por una ruta vacía construida por el invocador (`sed` con barras invertidas mal escapadas) | ninguna; el Owner autorizó el relanzamiento. La ruta se calcula ahora con `cygpath -w` y se comprueba antes de lanzar |
| — | la `NormativeDependencyClosure` literal de V13, calculada mecánicamente sobre el corpus, se extiende a casi todo el corpus: una sección entera trae todas sus referencias. Alcanza referencias que un resolutor mecánico no puede decidir sin juicio semántico: secciones que la Proposal propone y que aún no existen («§16.13»), § sin calificador presentes en varios documentos («§16.3») y § de registros que pueden apuntar a V12 o a V13 | con la regla literal, la independencia de las premisas afectadas es UNKNOWN aunque la fidelidad sea completa. La corrección que pide el Architect agranda aún más la clausura. Lo disponen el Owner y el Coordinator |

**Revalidación antes del commit** (MEASURED, 17:45Z):
- `origin/main` = `819955d6…` y la rama de I-62 en `b337a59f…`, sin cambios;
- I-52 `fb6b5648`, sin commits nuevos;
- I-63 `669d8a39..844dabb6`: 2 commits;
- I-64 `58d1140c..0b7db52f`: 2 commits;
- **ningún cruce** con superficies de I-62.

**No se hizo:**
- una segunda sesión de revisión, Workers, Controllers, subagentes, pilotos, autenticación ni cambios de configuración. El preflight usó `codex sandbox`, que
  no invoca ningún modelo;
- editar el clon, crear la Proposal V14, ni corregir, disponer, ratificar o rebajar hallazgos;
- decidir OD-6 ni declarar Freeze;
- tocar V1-V13, sus paquetes, los registros previos, la evidencia custodiada, el Discovery, el mandato, las normas compartidas o cualquier superficie ajena,
  incluido el proceso de otra sesión sobre el worktree de I-63.

**Validación de esta entrega:** allowlist (superficies propias de I-62), enlaces, columnas de las tablas, hashes de los archivos custodiados, YAML del estado y
`git diff --check`; resultado en el cuerpo del commit. La CI de este commit se informa al Owner y al Coordinator.

## 30. F0 — Proposal V14 (grafo normativo canónico y acotado; corrección final del residuo de A62-V11-01)

**CI del commit de custodia** (`f51eda39d56a498a4ec56b4604acbe3a9be25746`, §29): corrida **37043096438**, attempt 1, event `push`, rama
`architecture/portabilidad-coordinador-principal`, head_sha exacto, `completed/success`. Jobs: UI Tests (110957762431), Tests (Domain + Application)
(110957762674), Build UI (110957762730) y Build Plugin without AutoCAD (110958480246), los cuatro `success`. MEASURED por la sesión. Acredita solo
`f51eda39`.

**Orden recibida:** «I-62 — PROPOSAL V14 / BOUNDED NORMATIVE DEPENDENCY CLOSURE», texto pegado sin archivo de origen. 9 425 bytes en UTF-8, SHA-256
`346f14a0b48b4f9eb07fda1c254068605e74a2f3292780b6d0c76e326c6f7130`, fuera del repositorio. Registro recuperable en
`docs/initiatives/I-62-architect-review-v13-disposition.md`. Antes, el Owner retiró un mensaje pegado por error («ADDITIONAL REQUIRED — AUTONOMOUS ROLE
ORCHESTRATION», dirigido a V8): la sesión no lo aplicó ni lo registró como orden.

**Apertura del tramo** (MEASURED): `origin/main` = `819955d6…`, sin rebase; rama de I-62 en `f51eda39…` = remoto, árbol limpio.

**Alcance estrecho:** el diff V13→V14 (181 inserciones y 74 supresiones) toca solo:
- la cabecera, el mapa de la corrección, las fuentes, los cierres preservados y tres referencias a la versión vigente (§11.4, la tabla de retos y §20.13);
- §3.1 (nueva: destinos normativos propuestos);
- §13 (P-25);
- §20.3.3: punto 2 del envoltorio, regla de fidelidad, clausura sobre un grafo canónico y acotado, y el caso de V13;
- §20.11 (GAP-12);
- B.1, B.8.8 (campo e I-S18), B.10.1 y B.11 (nueva);
- C-42, G.1 y G.2.

Las demás secciones quedan sin cambio.

**Hecho medido que recoge V14** (GAP-12): el diagnóstico de perfil del `pwsh` del runtime apareció antepuesto a varias salidas en cada revisión medida: en
los comandos 0-4, 45, 48 y 49 de V12, y en los 0-3 y 89 de V13.

**Revalidación antes del commit** (MEASURED, 18:25Z):
- `origin/main` = `819955d6…` y la rama de I-62 en `f51eda39…`, sin cambios;
- I-52 `fb6b5648`, sin commits nuevos;
- I-63 `844dabb6..5e3306e0`: 3 commits;
- I-64 `0b7db52f..39f7caa4`: 1 commit;
- **ningún cruce** con superficies de I-62.

**Identidad del contenido entregado** (blob calculado con `git hash-object` antes del commit; el commit lo da el recibo):
- `docs/initiatives/I-62-proposal-v14.md` → `34ad80ea1bfff144bfc5169f62920a4c904c1bfa`;
- `docs/initiatives/I-62-architect-review-v13-disposition.md` → `37c1752c914d01605b36937192b677d2a4fc281e`;
- `docs/initiatives/I-62-architect-package-v14.md` → en el recibo del commit (el paquete no lleva su propio blob).

**No se hizo:**
- invocaciones de modelos, subagentes, Workers, Controllers, Architect, sondas, pilotos, sesiones nuevas, fixtures, remotos, pruebas, tooling ni esquemas en
  producción; tampoco se generó el manifiesto, que V14 define como entregable de F3;
- una clausura retroactiva para la revisión de V13;
- tocar V1-V13, sus paquetes, los registros previos, la evidencia custodiada, el Discovery, el mandato, las normas compartidas ni ninguna superficie ajena;
- reabrir un hallazgo cerrado, decidir OD-6 ni declarar AGREED o Freeze.

Las trazas de V14 son **análisis del diseño**, no ensayos.

**Validación de esta entrega:** allowlist, enlaces, columnas de las tablas, diff V13→V14 revisado, YAML del estado y `git diff --check`; resultado en el
cuerpo del commit. La CI de este commit se informa al Owner y al Coordinator.

## 31. F0 — Revisión formal final del Architect de la Proposal V14 (autorización del Owner y del Coordinator)

**CI de la Proposal V14** (`4c617e82b32b6c810b68d75fc19472efed22b393`, §30): corrida **37047587864**, attempt 1, event `push`, rama
`architecture/portabilidad-coordinador-principal`, head_sha exacto, `completed/success`. Jobs: Build UI (110972729955), UI Tests (110972730225), Tests
(Domain + Application) (110972730366) y Build Plugin without AutoCAD (110973255076), los cuatro `success`. MEASURED por la sesión. Acredita solo
`4c617e82`.

**Autorización recibida:** «OWNER / COORDINATOR AUTHORIZATION — FINAL FORMAL ARCHITECT REVIEW OF I-62 PROPOSAL V14», texto pegado sin archivo de origen.
7 728 bytes en UTF-8, SHA-256 `6c1ded51cbbba33889f11524ee504beb8ac204d940f4dd527b6ff91c6f6ee2be`; va literal dentro de `prompt.md`. La exención (306
bytes, SHA-256 `c1923b87…`) y la regla de GAP-12 (738 bytes, SHA-256 `9b5e16fa…`) están identificadas en el README de la corrida, §2.

**Invocación** (MEASURED; detalle en
[`I-62-architect-v14/R20261002T184526Z-b331/README.md`](I-62-architect-v14/R20261002T184526Z-b331/README.md)):
- **Antes:**
  - `codex-cli 0.159.2` y autenticación existente; `config.toml` con hash `40c27b57…` antes y después;
  - clon limpio `D:\r62-arch-v14` en `4c617e82`, sin colisión de escritores;
  - cierre de 74 insumos canónicos y 9 transitivos;
  - preflight FAITHFUL_NORMALIZED (78 insumos por A y B completas; 5 grandes por rangos de 150 líneas; 62 caracteres no ASCII; control negativo). La
    primera corrida falló solo por una búsqueda de prueba sin coincidencias («≈»).
- **Ejecución:** una sola, de 19:21:14Z a 19:47:25Z, salida 0. `gpt-6.1-sol`/`high`/read-only RUNTIME_OBSERVED, con una compactación a las 19:34:32Z
  que conservó el prompt. Consumo: entrada 13 279 704 tokens (12 915 200 en caché) y salida 24 668.
- **Después:** clon limpio, configuración sin cambio y ningún proceso de la corrida vivo; 117 lecturas sobre 49 rutas del cierre, con la política de lectura
  cumplida y sin `dotnet test`. Dos búsquedas fallaron sin devolver contenido.
- **Lo entregado al revisor: FAITHFUL_NORMALIZED.** 113 llamadas sin truncamientos; objeto y paquete vistos enteros; tres diagnósticos del runtime, ante los
  comandos Git 0-2, separados como metadato de transporte según la orden (GAP-12). Envoltorios fieles. Una cita del revisor omite un artículo que sí recibió.

**Resultado:** **BLOCKED — OWNER DECISION, solo por OD-6; cero REQUIRED; A62-V11-01 CLOSED.** La V14 exacta queda lista para el Consensus Freeze tras OD-6
y el acuerdo del Coordinator. Registro: [`I-62-architect-review-v14.md`](../../initiatives/I-62-architect-review-v14.md).

**Hechos medidos en este tramo:**
- la separación de GAP-12 solo afectó a la línea exacta y conocida del diagnóstico; no hizo falta ninguna otra normalización;
- el presupuesto de salida por llamada evitó de nuevo todo truncamiento visible (0 de 113).

**Revalidación antes del commit** (MEASURED, 19:54Z):
- `origin/main` = `819955d6…` y la rama de I-62 en `4c617e82…`, sin cambios;
- I-52 `fb6b5648`, sin commits nuevos;
- I-63 `5e3306e0..637dce7e`: 2 commits;
- I-64 `39f7caa4..db151227`: 2 commits;
- **ningún cruce** con superficies de I-62.

**No se hizo:**
- una segunda invocación, Workers, Controllers, subagentes, pilotos, autenticación ni cambios de configuración;
- materializar B.11, calcular una clausura retroactiva, editar el clon, crear V15, ni corregir, disponer, ratificar o rebajar hallazgos;
- decidir OD-6 ni declarar AGREED o Freeze;
- tocar V1-V14, sus paquetes, los registros previos, la evidencia custodiada, el Discovery, el mandato, las normas compartidas o cualquier superficie ajena.

**Validación de esta entrega:** allowlist (superficies propias de I-62), enlaces, columnas de las tablas, hashes de los archivos custodiados, YAML del estado y
`git diff --check`; resultado en el cuerpo del commit. La CI de este commit se informa al Owner y al Coordinator.

## 32. F0 — Decisión del Owner sobre OD-6 (alternativa 1)

**CI del commit de custodia** (`b90b01d6dc865b41370d2f9140341e5b7fd6b522`, §31): corrida **37057375285**, attempt 1, event `push`, rama
`architecture/portabilidad-coordinador-principal`, head_sha exacto, `completed/success`. Jobs: Tests (Domain + Application) (111005275529), UI Tests
(111005275765), Build UI (111005275866) y Build Plugin without AutoCAD (111005951496), los cuatro `success`. MEASURED por la sesión. Acredita solo
`b90b01d6`.

**Decisión recibida:** «OWNER DECISION — I-62 OD-6», texto pegado sin archivo de origen. 2 360 bytes en UTF-8, SHA-256
`8d1899d49b1a28d536391f00bb582b9f25435d569b978ded8aabffef86a85771`, fuera del repositorio. Resumen fiel en las decisiones, §30.

**Comprobación** (MEASURED sobre el texto de V14, commit `4c617e82`): la decisión coincide con la alternativa 1 de V14 §11.4 en las revisiones afectadas,
los predicados de SEPARATE SESSION y EXTERNAL HUMAN, el tratamiento de SAME-SESSION ROLE, la conservación de los modos y la ausencia de efecto retroactivo. No
hace falta cambiar la V14.

**Recibido sin acción** (informativo; no es una orden y no cambia nada en I-62): la sesión de I-64 comunicó un registro de deuda de protocolo dirigido a la
evolución del protocolo de ejecución. En sus controles negativos nc2, un Controller emitió Scope = pass con un `AllowedWriteScope` mutado, después de que
fallara su comparación auxiliar (PowerShell en ConstrainedLanguage). Registro en la rama `architecture/workspace-persistente-rackcad`, commit `39b45f36`,
`docs/automation/evidence/I-64-pilot/F1-T1-MODEL-nc2/R20261002T184050Z-8415/protocol-debt-handoff.md`. La sesión de I-62 lo leyó sin modificarlo.
Incorporarlo, cuándo y cómo lo deciden el Master Coordinator y el Coordinator de I-62.

**Revalidación antes del commit** (MEASURED, 21:30Z):
- `origin/main` = `819955d6…` y la rama de I-62 en `b90b01d6…`, sin cambios;
- I-52 `fb6b5648`, sin commits nuevos;
- I-63 `637dce7e..f5c4a5a2`: 3 commits;
- I-64 `db151227..39b45f36`: 1 commit;
- **ningún cruce** con superficies de I-62.

**No se hizo:** declarar AGREED ni Freeze; cambiar la V14 ni crear V15; implementar; invocar modelos o roles; tocar superficies ajenas.

**Validación de esta entrega:** allowlist, enlaces, columnas de las tablas, YAML del estado y `git diff --check`; resultado en el cuerpo del commit. La CI de
este commit se informa al Owner y al Coordinator.

## 33. F0 — AGREED del Coordinator y Consensus Freeze (materialización)

**CI del commit del registro de OD-6** (`59bf98552774b4422b70afe6066a435447ab4d65`, §32): corrida **37067323542**, attempt 1, event `push`, rama
`architecture/portabilidad-coordinador-principal`, head_sha exacto, `completed/success`. Jobs: Tests (Domain + Application) (111038288562), Build UI
(111038288802), UI Tests (111038288926) y Build Plugin without AutoCAD (111038954167), los cuatro `success`. MEASURED por la sesión. Acredita solo
`59bf9855`.

**Orden recibida:** «I-62 — COORDINATOR AGREED / CONSENSUS FREEZE ORDER», texto pegado sin archivo de origen. 4 175 bytes en UTF-8, SHA-256
`22f20aca479c34b1d0a08092da47e75f9f63925f7667814cfb8e2db50c4eb928`, fuera del repositorio. Resumen fiel en las decisiones, §31.

**Validación antes de publicar** (MEASURED, 21:39-21:45Z):
- **V14 exacta en la rama:** `HEAD:docs/initiatives/I-62-proposal-v14.md` = `34ad80ea1bfff144bfc5169f62920a4c904c1bfa`. `4c617e82` es ancestro de `HEAD`, y
  la Proposal y el paquete no cambian desde `4c617e82`.
- **Registro de OD-6:** `59bf9855` es ancestro de `HEAD`; decisiones §30 presente.
- **Revisión del Architect:** `output.json` de la corrida `R20261002T184526Z-b331` (SHA-256 `e3c7f44e…`) da BLOCKED — OWNER DECISION, `RequiredFindings`
  vacío y A62-V11-01 CLOSED, sobre `4c617e82` / `34ad80ea` / `3c3b3446`.
- **Ningún REQUIRED abierto** en el estado ni en las decisiones vigentes: el único REQUIRED, A62-V11-01, está CLOSED; los cuatro OPTIONAL abiertos no bloquean.
- **OD-6 = alternativa 1** coincide con V14 §11.4 (§32).
- **Superficies:** solo las propias de I-62; V14 y su paquete sin cambios; ROADMAP sin tocar.
- **DC-07 frente a las hermanas activas:** las tres ramas no integradas son I-52 (`fb6b5648`, sin cambios), I-63 (`4c1b8763`) e I-64 (`39b45f36`).
  - Ninguna toca `docs/AUTOMATION_PLAN.md`, `docs/WORKFLOW.md`, `docs/INITIATIVE_LIFECYCLE.md`, `AGENTS.md`, `docs/automation/agent-execution/`,
    ADR-0046 ni superficies de I-62.
  - Las tres tocan `docs/ROADMAP.md` (sus filas), que este Freeze no toca.
  - `origin/main` = `819955d6…`, sin cambios.
  - I-64 declara su F1 en BLOCKED_PROTOCOL_DEPENDENCY sobre la evolución del protocolo de ejecución (registro informativo, §32). No es un conflicto de
    archivos. Si se incorpora, lo deciden el Master Coordinator y el Coordinator de I-62: por ejemplo, con una enmienda A-n según LIFECYCLE §6.
  - **Sin conflicto ni bloqueo nuevo.**

**Freeze materializado:** `docs/initiatives/I-62-consensus-freeze.md`. Su blob y el commit del Freeze los da el recibo de publicación, porque el commit no
puede contener su propio SHA. Es inmutable desde ese commit (LIFECYCLE §6).

**No se hizo:**
- modificar la Proposal V14 (ni la línea `Frozen`), crear V15, implementar o abrir F1;
- materializar el manifiesto B.11;
- invocar modelos o roles;
- tocar superficies ajenas, ROADMAP o normas compartidas.

**Validación de esta entrega:** allowlist, enlaces, columnas de las tablas, YAML del estado y `git diff --check`; resultado en el cuerpo del commit. La CI de
este commit se informa al Owner y al Coordinator.

## 34. F1 — Apertura e implementación (orden del Coordinator)

**CI del commit del Freeze** (`b64a3b640c7ee3bd77636e7a3218ae0ebac2dd43`, §33): corrida **37068394720**, attempt 1, event `push`, rama
`architecture/portabilidad-coordinador-principal`, head_sha exacto, `completed/success`. Jobs: Tests (Domain + Application) (111041765023), Build UI
(111041765360), UI Tests (111041765545) y Build Plugin without AutoCAD (111042336278), los cuatro `success`. MEASURED por la sesión. Acredita solo
`b64a3b64`.

**Orden recibida:** «I-62 — COORDINATOR ORDER: OPEN AND IMPLEMENT F1», texto pegado sin archivo de origen. Cuerpo sin las etiquetas de pegado: 10 738 bytes
en UTF-8, SHA-256 `6638968b39f5b8f287b2ab6bc9cba71542c6cc3e5f1d7d027f28b77349223f79`, fuera del repositorio. Resumen fiel en las decisiones, §32.

### 34.1 DC-07 antes de escribir superficies compartidas

MEASURED tras `git fetch --prune`, antes de escribir la clase de prueba y las superficies (la RED es de las 22:06:19Z). `origin/main` = `819955d6…`, sin
cambios.

| Rama activa | Punta | merge-base | Cambios en superficies de F1 |
|---|---|---|---|
| I-52 `feature/rackmirror-espejo-semantico` | `fb6b5648` | `95690c28` | `docs/adr/0036-…` (A) y `docs/adr/README.md` (índice); F1 no toca el índice |
| I-63 `architecture/parametros-calculados-resumen-proyecto` | `eb58a476` | `819955d6` | ninguno |
| I-64 `architecture/workspace-persistente-rackcad` | `39b45f36` | `819955d6` | `docs/adr/0047-…` (A) |

Superficies comprobadas: `docs/AUTOMATION_PLAN.md`, `docs/automation/agent-execution/`, `docs/initiatives/PROMPT_TEMPLATES.md`, `docs/adr/`, `docs/WORKFLOW.md`,
`docs/INITIATIVE_LIFECYCLE.md`, `AGENTS.md`, `CLAUDE.md` y las clases de prueba de los protocolos. **Sin solapamiento:** ninguna rama toca un archivo que F1
modifica. F1 no edita ROADMAP, así que no necesita ventana. La comprobación se repite antes del push (cuerpo del commit).

### 34.2 Asignación del número de ADR

MEASURED tras `git fetch --prune`:
- **`main` (`819955d6`):** `docs/adr/` contiene 0001-0035 y 0037-0046 (con dos archivos 0002 históricos). No contiene 0036.
- **I-52:** añade `docs/adr/0036-rackmirror-espejo-semantico-por-copia.md`, así que 0036 queda ocupado por esa rama.
- **I-64:** añade `docs/adr/0047-workspace-persistente-modeless-rackcad.md`, en estado propuesto (commit `36c344dc`).
- **I-63:** no tiene archivo ADR. Su diseño prevé un ADR sucesor de ADR-0043 «con número asignado al integrar»; no reserva ninguno.
- **Menciones:** `git grep` de `ADR-0047..0059` y `adr/0047..0059` sin los TRX. En `main`, cero (las coincidencias de «0047» en `main` son GUID de TRX). En
  las ramas, solo las de I-64 a su propio 0047. Nadie menciona 0048 ni números mayores.

**Siguiente número libre sin colisión: 0048**, sin ambigüedad. Archivo:
`docs/adr/0048-ejecucion-delegada-portable-roles-binding-y-autoverificacion.md`, estado **propuesto**, sin fila en el índice (cierre documental, WORKFLOW §11.4).

### 34.3 Materialización del plano (b)

Cada cláusula congelada (V14) y su destino. Todo es **inactivo** hasta `I62_EFFECTIVE_SHA` (AUTOMATION_PLAN 16.14; ADR-0048 «Vigencia»; README §12):

| Cláusula congelada | Destino | Fidelidad |
|---|---|---|
| §2, tabla de roles; «El Coordinator de gates no es vinculable»; acumulación sobre la identidad observable | AUTOMATION_PLAN §16.1, bloque «Unidades I62» | filas de V14 §2, salvo «(§20.x)» → «(Proposal V14 §20.x)» en tres celdas; la acumulación, con su efecto al vincular (§11.3) |
| §4.1, perfil, tabla por acción, perfil sin binding, P-09/P-10 por acción, CUSTODY no es permiso | AUTOMATION_PLAN 16.15 | tabla idéntica; texto fiel |
| §4.1, §5, §6 y §10: autoverificación (niveles de la fuente, autodeclaración no es fuente, RUNTIME_OBSERVED mínimo, sin depender del binding) | 16.15, «Autoverificación» | sin la tabla de fuentes por adapter, que es de F2 |
| §4.2: estados, agregado, disposiciones | 16.16 | idénticos (guarda C-03) |
| §13: P-09 y P-10 | 16.16, tabla de ids | filas idénticas (comprobado por el script de materialización) |
| §4.2: ejemplos E1-E13; B.4: forma de las filas, `Causes` y `Disposition` | README §12 | tabla idéntica (guarda C-03); procedimiento con la forma de B.4, sin esquema |
| §7, operación 3; §20.3; §3, fila «PROMPT_TEMPLATES §G / adapters» | 16.17 | frontera de renderizado |
| §15, plano (b); §14.0-§14.1 (aplicabilidad y punto efectivo); §20.11 | 16.14 | cláusula de vigencia, con la excepción de 16.13 |
| §3, fila PROMPT_TEMPLATES (corrección del bloque factual de §2); Discovery §10.1 (1) y EXP-08 | PROMPT_TEMPLATES §2 | bloque factual |
| Anexo A | ADR-0048 | conserva #2-#9, supera, amplía #1, vigencia; lo pedido por la orden |

**Hecho factual de la corrección de §2** (MEASURED con la regla de WORKFLOW §11.2):
- el único commit con el trailer `Workflow-V2-Normative: I-56` es `afd1077c…`;
- el primer merge en first-parent de `origin/main` cuyo segundo padre lo alcanza y cuyo primer padre no lo alcanza es
  `8a021fb67c16dfccd6afc18448ea7e6a71a32364`;
- el bloque dice «WORKFLOW V2 = EFFECTIVE» y remite a esa derivación, sin copiar el SHA.

**Fuera de F1** (elecciones de materialización, decisiones §32): la tabla de acumulación (F3); WORKFLOW §4 (F4); `routing.md` (F2); README §§1, 3, 5,
6 y 11; los punteros de §16 y §16.3 y la propia 16.13 (F4); el delta de LIFECYCLE de OD-6 (fuera del mapa de F1). No se tocaron AGENTS, CLAUDE,
FOUNDATIONS, HANDOFF, ROADMAP, WORKFLOW, LIFECYCLE, el índice ADR ni los esquemas `/v1`.

### 34.4 C-01..C-04

| Id | Clase / autoridad | Evidencia | Resultado de la sesión |
|---|---|---|---|
| C-01 | (i) Core RG + mutation | `I62_C01_RoleTableOfSection16_1HasTheFiveNeutralRolesAndNoProviderMark`: los cinco roles en su orden y cero marcas en la tabla. `I62_C01_CoreSchemaEnumsCarryNoProviderMark`: su entrada está **vacía** en F1, porque `schemas/` solo tiene los cinco `/v1` (MEASURED); el `codex-cli` del enum de `relay-record/v1` es I61 y queda fuera del núcleo. Mutation en memoria (`I62_C01_TheMarkOraclesDetect…`): «Codex» inyectado en la tabla, un rol borrado y un enum sintético `claude-subagent`, los tres detectados. Marcas: patrones fijos, proveedores de los descriptores (ninguno antes de F2) e ids y familias del catálogo | PASS en local; la CI exacta lo acredita |
| C-02 | (ii) revisión del Coordinator | ADR-0048 contra V14 §2: los roles son semánticos; los proveedores, ejecutables, modelos y recetas solo aparecen como algo que vive en los descriptores y el catálogo. Ninguna asociación rol↔proveedor. Barrido con el mismo conjunto de 27 marcas de C-01: **0** coincidencias en el ADR (también 0 en las partes I62 del plan y en README §12) | la evidencia apoya PASS; lo decide el Coordinator |
| C-03 | (i) Core RG + mutation (fidelidad) | `I62_C03_TheOracleIsTheFrozenProposal`: el oráculo es V14, fijada por su blob `34ad80ea…`. `I62_C03_StatusRulesOfSection16EqualProposalV14Section4_2RowByRow` y `I62_C03_ReadmeExamplesEqualProposalV14Section4_2RowByRow`: 0 diferencias. Mutation (`I62_C03_TheFidelityOracleDetects…`): disposición debilitada, E4 optimista, E13 omitido, pasos 2 y 3 del agregado permutados y oráculo alterado, todos detectados | PASS en local; la CI exacta lo acredita |
| C-04 | (ii) MC | `I-62-F1/c04-design-data.json`: E1-E12 como datos de diseño con la forma de B.4, con los esperados escritos a mano desde §4.2 y B.4. `I-62-F1/c04-aggregation-mc.py`: aplica el procedimiento de README §12, paso a paso, sin leer los esperados. `I-62-F1/c04-result.json`: **14 evaluaciones, 14 PASS** (E10 antes y después de la decisión; E12 con CUSTODY y RESUME_DECISION). Control negativo con cuatro mutaciones del procedimiento, todas detectadas: m1 UNKNOWN antes que BELOW (E5, E10 antes); m2 los opcionales cuentan (E9, E12 RESUME_DECISION); m3 sin el paso 1 (E11); m4 lo no observado como MATCH (ocho evaluaciones) | PASS por la sesión; lo decide el Coordinator |

SHA-256 de los archivos de C-04 (LF, como en el repositorio): `c04-design-data.json` `7eb71197…`; `c04-aggregation-mc.py` `4aaf0bc9…`; `c04-result.json`
`23488cd5…`. Reproducción: `python c04-aggregation-mc.py c04-design-data.json <salida>`.

### 34.5 Pruebas y validación (antes del commit)

- **RED** (22:06:19Z), con el árbol `b64a3b64` + la clase de prueba nueva y sin materializar: 7 seleccionadas, **5 fallidas** (texto materializado
  ausente), 2 superadas (la fijación del oráculo y el enum de entrada vacía). TRX SHA-256 `8dcf829b…`.
- **GREEN focal** (22:10:00Z), `PrincipalPortabilityProtocolTests` + `AgentExecutionProtocolTests`: **24/24**. Las 17 de I-61 pasan con sus oráculos
  intactos. TRX `4788622f…`.
- **Core Full** sobre el árbol con todos los cambios (22:10:18Z): **12403/12403**. TRX `27aa48a9…`. Es evidencia del árbol, no del SHA del commit: el Core
  Full del SHA exacto, si el cierre de F1 lo exige, se corre aparte.
- `git diff --check` limpio. Enlaces de las cuatro superficies tocadas: 32, ninguno roto. Columnas de las tablas correctas. Encabezados únicos por archivo.
  Fines de línea LF en el repositorio.
- Contexto de la ejecución: SDK .NET 8.0.423 del usuario (`%LOCALAPPDATA%\Microsoft\dotnet`).

**No se hizo:**
- F2 (observación, descriptores, esquemas de hechos, `preflight/v1`, `relay-record/v2`, `controller-verification/v2`, huellas);
- F3 (binding, contratos `/v2`, `role-invocation`, `input-closure`, `input-fidelity`, resultados por rol, B.11, A1'-A8', 14 comprobaciones);
- F4;
- invocar modelos o roles;
- declarar el GATE PASS de F1.

La CI del commit de esta entrega se informa al Owner y al Coordinator.

## 35. F2 — Observación de capacidad, adapters y esquemas (orden del Coordinator)

**CI del commit de F1** (`12660ea76f224694ea7fce1f13b122ffb9d9ade5`, §34): corrida **37071797018**, attempt 1, event `push`, rama
`architecture/portabilidad-coordinador-principal`, head_sha exacto, `completed/success`. Jobs: Tests (Domain + Application) (111052715369), Build UI
(111052715353), UI Tests (111052715243) y Build Plugin without AutoCAD (111053262137), los cuatro `success`. **Core Full del SHA exacto** con el árbol
limpio: 12403/12403 (22:19:28Z, TRX SHA-256 `088080a3…`). MEASURED por la sesión.

**Orden recibida:** «I-62 — MAXIMUM AUTONOMOUS PROGRESS WHILE OWNER IS AWAY», texto pegado sin archivo de origen. Cuerpo sin las etiquetas de pegado:
13 947 bytes en UTF-8, SHA-256 `03b72b6388d44119aaa00c66c669b1b2d56bf7416abeaa5fc9b775da97ae89b0`. Resumen fiel en las decisiones, §33.

### 35.1 DC-07 antes de escribir

MEASURED tras `git fetch --prune` (22:37Z). `origin/main` = `819955d6…`, sin cambios. Ramas activas: I-52 `fb6b5648` (merge-base `95690c28`), I-63
`138bc3d4` (avanzó desde `eb58a476` con su G3-T2) e I-64 `39b45f36`. Ninguna toca `docs/automation/agent-execution/`, `docs/AUTOMATION_PLAN.md` ni las clases
de prueba de los protocolos; I-52 solo toca su ADR 0036 y el índice ADR, que F2 no toca. **Sin solapamiento.** El cuerpo del commit repite la
comprobación antes del push.

### 35.2 Materialización (plano b, inactivo hasta `I62_EFFECTIVE_SHA`)

| Cláusula congelada (V14) | Destino |
|---|---|
| §6 (dos objetos, invalidadores); B.3 (validación en dos fases, registro, contraste, P-14); B.2 (`HostInstanceState`) | AUTOMATION_PLAN 16.18 |
| §7 (contrato de nueve operaciones, descriptor, `Facts` única frontera abierta, huella); §13 (P-11, P-14) | AUTOMATION_PLAN 16.19 |
| §7 (tabla por adapter); §10 (fuentes de introspección); §9.1 (terminación) | `agent-execution/adapters/<id>.md` × 5 |
| B.4 | `schemas/preflight.v1.schema.json` |
| B.7 sobre `/v1` (+ B.8.2, B.8.7, B.2) | `schemas/relay-record.v2.schema.json` |
| B.7 sobre `/v1` (+ E.3.1 paso 5) | `schemas/controller-verification.v2.schema.json` |
| B.3 (esquema estricto por adapter) | `schemas/adapters/<id>.facts.v1.schema.json` × 5 |
| §6, B.3, B.4; C-07, C-08, C-10 | README §13 (producción, coherencia C1-C8, invalidación, contraste, saneamiento) |
| B.2 (`RequirementId`: lista fija por perfil y acción en routing.md) | routing §8 |

**Elecciones de implementación** (IMPLEMENTATION_CHOICE; preservan el contrato observable):
1. Esquemas nuevos con nombre versionado `*.v<n>.schema.json`, como el patrón de B.2 para los hechos. Los cinco `/v1` no cambian (C-19).
2. Sin `$ref`, `$defs`, `oneOf` ni condicionales, para que el oráculo de estrictez recorra todo. Las reglas entre campos van en README §13.2 (C1-C8),
   como hizo I-61 con la verificación (README §8).
3. Huella como un solo objeto neutral, con `Kind` CONFIG_FILE, NONE o UNVERIFIED: el archivo lo nombra el descriptor, no el núcleo.
   `AcceptanceDecisionRef` = `{Path, CommitSha, Blob}`.
4. `Action` = `LOCAL_EVIDENCE` para la «evidencia local» de §4.1; las demás acciones son las de B.9.
5. Referencias: `DescriptorRef` = `{Path, Blob}`; `SchemaRef.Path` relativo a `agent-execution/`, en la forma literal de B.2.
6. Valores:
   - `BinaryPathHash` es nullable (no aplica) para sesiones y subagentes;
   - `CatalogEntryBlob` = blob de `model-catalog.md`, la opción conservadora (cualquier cambio del catálogo invalida).
7. `relay-record/v2`:
   - `FingerprintChanged` sustituye a `ConfigChanged`;
   - `Participant.Observation[]` = `{Name, Requested, State, Value, Source, SourceSha256, Assurance, ObservedUtc}`;
   - `ReadAudit.Coverage` = RECORDED, PARTIAL, EMPTY o UNAVAILABLE, con el vocabulario de D.6;
   - SHA-256 en minúsculas también en los campos conservados (B.1).
8. `RebaseMap` de `relay-record/v2`: B.7 añade `StateFields` y `CiRuns`, y B.8.7 dice que el `RebaseMap` del diario lleva «los mismos campos» que el
   mapa durable. Se materializa la **unión** (`BranchBeforeSha`, `BranchAfterSha`, `Unmapped`, `Commits[].PatchId`). Es una reconciliación de dos cláusulas
   coherentes, no un conflicto.
9. `controller-verification/v2`:
   - `Verifier.Role` fijo en EXECUTION_CONTROLLER;
   - cada entrada de `AuthorityResolution[]` lleva `{Path, Section, Class, Mode (NORMAL | COMPAT | COMPUESTA | ENTRY), RevisionSha, Parts[], EffectiveSha,
     ClauseMapBlob, UnitProtocol}`: el registro del paso 5 de E.3.1, por entrada.
10. routing §8: los requisitos de los perfiles de rol se derivan de §1 y §3 (effort, nivel y capacidades mínimas). El nivel requerido es el primero que
    §3 lista para ese effort.
11. Neutralidad (16.17): la fila P-11 de §13 dice «STOP (P-01 para Codex, igual)», y en 16.19 se redacta «STOP (igual que P-01)», sin cambio de
    significado. El nombre del archivo de configuración vive en el descriptor de `codex-cli`. Las partes I62 de AUTOMATION_PLAN no contienen nombres de
    proveedor ni de archivos de un runtime (barrido MEASURED).
12. Descriptores: solo estados demostrados.
    - `codex-cli` DISPONIBLE por la evidencia medida de I-61 y de las revisiones de I-62; versión, autenticación y registro de sesión solo invocando.
    - `claude-desktop-session` operación 2 DISPONIBLE, medida en F2 con `get_session("self")`.
    - Las demás huellas, UNVERIFIED (V14 §7, columna 9).
13. C-01: el barrido de enums se limita al núcleo; los esquemas de hechos son la frontera del adapter (B.3) y pueden nombrar su runtime.

**Huellas para aceptación del Coordinator en F2** (V14 §7): **ninguna**. Ningún adapter declara «ninguna». `codex-cli` declara CONFIG_FILE; los otros
cuatro, UNVERIFIED.

### 35.3 C-05..C-10 y C-19

| Id | Clase | Evidencia | Resultado de la sesión |
|---|---|---|---|
| C-05 | (i) Core RG + mutation | `I62_C05_CoreSchemasAreStrictExceptFactsAndUseExactHashPatterns`; `I62_C05_CoreSchemaEnumsCarryNoProviderMark`; `I62_C05_AdapterFactsRequiresAStrictSchemaRefAndFactsIsTheOnlyOpenObject`. Mutaciones detectadas (`I62_C05_TheCoreOraclesDetect…`): `codex-cli` en un enum, un objeto anidado abierto, `Fingerprint` abierto, un patrón de SHA debilitado, `SchemaRef` fuera de `required` y `Facts` cerrado | PASS en local |
| C-06 | (ii) MC (+ OV-I62-02) | `I-62-F2/preflights/`: siete `rackcad-preflight/v1` para los cinco adapters (Principal × RESUME_DECISION, CUSTODY y LOCAL_EVIDENCE), con fase 1, fase 2 y C1-C8 en VALID (`*.validation.json`, PowerShell 7.6.6). **Estado observado en el momento:** `claude-desktop-session`, MATCH / ELIGIBLE en las tres acciones (autoverificación real de esta sesión); `claude-subagent`, UNKNOWN / NOT_ELIGIBLE (P-10; candidata sin lanzar); `claude-cli`, UNKNOWN / NOT_ELIGIBLE (no está en el `PATH`; OD-3); `codex-cli`, UNKNOWN / NOT_ELIGIBLE (sin invocar: OD-2); `codex-desktop-session`, UNKNOWN / NOT_ELIGIBLE | PASS (MC); OV-I62-02 pendiente del Owner |
| C-07 | (ii) MC | `f2-mc-result.json`, C-07. Sobre el preflight MATCH del Principal: versión, huella, instancia del host o blob de routing cambiados → requisitos UNKNOWN; un binding hipotético nuevo o la rama avanzada → sin invalidación | PASS |
| C-08 | (ii) MC | En una copia temporal del protocolo: (a) `test-null` con `Facts` no vacío, VALID, con los esquemas del núcleo sin cambios (hash antes y después); (b) propiedad inesperada → fase 2 → P-14; (c) id desconocido → C1/C2 → P-14; (d) versión incompatible → C2 → P-14; (e) huella NONE sin aceptación → C4 → P-14; (f) contraste con un Exit/Entry discrepante → S-04, y coincidente → sin discrepancia. Un `relay-record/v2` y una `controller-verification/v2` sintéticos validan con `Test-Json` | PASS |
| C-09 | (i) Core RG + mutation | `I62_C09_EachFrozenAdapterHasADescriptorWithTheNineOperationsAndAStrictFactsSchema`; mutaciones (operación omitida, estado inválido, versión del esquema rota) detectadas | PASS en local |
| C-10 | (ii) MC + (i) guarda | MC: 10 de 10 valores sensibles **ficticios** redactados (SK, BEARER, PEM, URLCRED, GH, AWS, JWT, SLACK, GAPI, KV, nombres de clave con rutas), dos rechazos de custodia fuera del texto libre y un control benigno sin cambios. Guarda: `I62_C10_NoI62SchemaDeclaresACredentialField` + mutación | PASS |
| C-19 | (i) Core G | `I62_C19_TheFiveI61SchemasAndTheI61GuardClassKeepTheirBlobs` (blobs fijados); las 17 pruebas de I-61 pasan sin cambios | PASS en local |

### 35.4 Hechos del host y frontera de OD-2 (MEASURED, sin ejecutar ningún runtime bloqueado)

- **`~/.codex/config.toml`:** SHA-256 `155933b3…` (`LastWriteTimeUtc` 21:34:25Z), igual a la línea base que el Owner aceptó **para I-63** a las 21:42Z tras
  actualizar y reiniciar la app (evidencia de I-63 §45, rama de I-63).
  - Las revisiones de I-62 vieron `40c27b57…` hasta las ~18:45Z.
  - El Discovery registró `37DD3559…` frente a `42E15A03…` observado.
  - 103 nombres de secciones y claves; 22 saneados como `<redactado>` (rutas de proyectos), como en I-61.
- **Binarios de la CLI:** `bin/a51e250fa15c740a`, SHA-256 `fcd5eafe…` (la ruta verificada; `codex-cli 0.159.2` en I-61, I-62 e I-63), y `bin/be3fd7e5c1969ff6`,
  SHA-256 `1722907a…` (instalado a las 15:39Z por la actualización de la app; `0.159.0-alpha.12.1` según I-63). En F2 **no se ejecutó** ningún binario.
- **Host:** `HostLabelHash` `63077eb1…`; `HostInstanceHash` `dc8562f9…` (`MachineGuid` disponible: la verificación de B.2 en F2 es positiva).
- **Sesión del Principal:** `get_session("self")` dio `claude-opus-5-5` y `xhigh`, que el catálogo traduce a `Long-horizon` y nivel Frontera.

### 35.5 Pruebas y validación (antes del commit)

- **RED** (22:42:17Z), con las guardas de F2 escritas y sin materializar: 16 seleccionadas, **8 fallidas** (artefactos de F2 ausentes) y 8 superadas
  (las 7 de F1 y C-19, que es G y no RG). TRX `32fe9275…`.
- **GREEN focal** (22:47:58Z), clases I62 + I61: **33/33**. TRX `f104082e…`.
- **Core Full del árbol:** en el cuerpo del commit.
- `Test-Json` de los ocho esquemas nuevos, como JSON válido y como esquemas que validan los registros de C-06 y C-08.

**No se hizo:**
- F3: binding, `binding/v1`, contratos `/v2` de gate y delegación, `role-invocation`, `input-closure`, `input-fidelity`, resultados por rol, B.11,
  A1'-A8' y las 14 comprobaciones;
- F4;
- invocar `codex-cli` o `claude-cli`, autenticar, instalar o cambiar configuración;
- leer credenciales o valores de configuración;
- declarar el GATE PASS de F2.

## 36. F2 REVIEW_READY y preparación de F3, F4, F6 y F7 (orden §33)

**CI del commit de F2** (`1eddbf48dbefd9685e68d63c0560bacf484d2dcb`, §35): corrida **37075239120**, attempt 1, event `push`, head_sha exacto,
`completed/success`. Jobs: Tests (Domain + Application) (111063540432), Build UI (111063540308), UI Tests (111063540427) y Build Plugin without AutoCAD
(111063933724), los cuatro `success`. **Core Full del SHA exacto** con el árbol limpio: 12412/12412 (22:58:43Z, TRX SHA-256 `1c84642d…`). Core Full del
árbol antes del commit: 12412/12412 (22:54:47Z, `b4212ade…`). MEASURED por la sesión.

### 36.1 Paquete de gate de F2

[`I-62-F2/f2-gate-packet.json`](I-62-F2/f2-gate-packet.json): **F2 = REVIEW_READY** (la sesión no declara GATE PASS). C-05, C-06, C-07, C-08, C-09, C-10 y C-19 en
PASS sobre `1eddbf48`, cada uno con su artefacto, su prueba o su MC y el SHA. C-06 deja OV-I62-02 al Owner. La observación de `codex-cli` que exige
invocarlo espera **OD-2, que no bloquea F2**: V14 §17 dice «OD-2 solo para medir invocando», y su frontera es la primera invocación afectada. Huellas para
aceptación del Coordinator: ninguna (§35.2).

### 36.2 Preparación (clase B; nada materializa F3+)

En [`I-62-prep/`](I-62-prep/README.md), documentación sin autoridad normativa:
- [f3-dossier.md](I-62-prep/f3-dossier.md): mapa de archivos con blobs de `1eddbf48`, planes de los nueve esquemas (las elecciones de forma marcadas
  IMPLEMENTATION_CHOICE), plan de pruebas RED → GREEN de C-11..C-14, C-30 y C-40, plan de B.11, DC-07 y disposición de la deuda nc2 de I-64;
- [f4-dossier.md](I-62-prep/f4-dossier.md): mapa de archivos, orden, matriz de transiciones y su auditoría, validadores, compatibilidad y pruebas de
  C-15..C-42;
- [freeze-issues.md](I-62-prep/freeze-issues.md): **FC-01** (sin camino para cerrar un bucle y abrir otro; presupuestos de unidad) y **FC-02** (SHAs de
  `orchestration` que un rebase deja obsoletos, sin reconciliación ni actualización posibles), con sus A-n candidatas **sin aplicar**; SM-01..SM-04 no
  materiales;
- [f6-dossier.md](I-62-prep/f6-dossier.md): FX-01..FX-06, grafo de dependencias, y que FX-04a solo necesita OD-5;
- [owner-decision-packets.md](I-62-prep/owner-decision-packets.md): OD-2, OD-3, OD-4, OD-5, OD-7 y DEP-F4-YAML, cada uno con su selección corta; ninguno se
  solicita todavía;
- [ov-scripts.md](I-62-prep/ov-scripts.md) y [f7-ready-closure.md](I-62-prep/f7-ready-closure.md): guiones de OV; borrador de FOUNDATIONS solo como
  evidencia; READY-01..09; Candidato; cierre e integración, con el conflicto previsto en el índice ADR (I-52 lo modifica ya).

**Disposición de la deuda de I-64** (f3-dossier §7):
- para las unidades I62, **B**: F3 la satisface en procedimiento (pertenencia reproducible en la `Evidence` de `Scope`, `not_run` si la comparación falla y
  comprobación mecánica del Coordinator);
- para I-64, **D**: I-64 es una unidad ANTERIOR (I61 de por vida) y las reglas de I-62 no son retroactivas (V14 §14.1), así que I-62 no puede desbloquearla.
  Decide el Master Coordinator.

### 36.3 Investigación y verificaciones (MEASURED; sin invocar runtimes bloqueados)

- **`Test-Json`** de PowerShell 7.6.6 aplica 2020-12 (`additionalProperties: false`, `pattern`, `enum`) con mensajes de error localizados.
- **`MachineGuid`** disponible: `HostInstanceState` OBSERVED en este host.
- **Contrato real de I-61 G3** (blob `9b5ef6df…`), insumo de C-20c: 17 citas, y cada encabezado citado existe una sola vez en `origin/main` y en la rama.
  `Claim-Id` para la PRE de prueba: I-61 `0e2923de-…`, I-64 `614371d5-…`.
- **Prototipo local de la derivación del mapa (E.4):** determinista (dos ejecuciones con SHA-256 `2e900f01…`) y coherente con E.5 para lo materializado
  en F1 y F2 (18 archivos: 4 MODIFIED y 14 ADDED). Las secciones de título de nivel 1 salen siempre MODIFIED (SM-04). El script queda fuera del repositorio.
- **Lector YAML:** el proyecto de pruebas no tiene ninguno, y AGENTS exige el acuerdo del Owner para añadir dependencias → DEP-F4-YAML (preparada).
- **Binarios de la CLI:** dos instalados (`a51e250f`, la ruta verificada, y `be3fd7e5`, nuevo); la receta usa la ruta verificada.

### 36.4 DC-07

`git fetch --prune` antes del commit (en su cuerpo). Ramas activas: I-52 `fb6b5648`, I-63 `138bc3d4` e I-64 `39b45f36`. Ninguna toca los archivos del
protocolo, AUTOMATION_PLAN, WORKFLOW, LIFECYCLE, las clases de prueba de los protocolos ni `docs/automation/evidence/I-62*`. I-52 modifica el índice ADR:
conflicto previsto solo en el cierre documental.

**Artefactos locales:** generadores, prototipo del mapa y TRX en el scratchpad de la sesión, fuera del repositorio; el árbol queda limpio en el commit.

### 36.5 Comprobación local de los planes de esquema de F3 (addendum)

**CI del commit de preparación** (`d8c55a971c4a52684c853a79ac11a2d47323ec57`): corrida **37076587763**, `push`, SHA exacto, `success`. Jobs: Tests
(Domain + Application) (111067750856), Build UI (111067751133), UI Tests (111067751132) y Build Plugin without AutoCAD (111068411038), los cuatro `success`.

Los nueve esquemas de F3 se redactaron como borradores locales, sin commit (F3 no está autorizado). Son estrictos, sin `oneOf`, neutrales y compilan con
`Test-Json` 2020-12. Detalle del campo `SizeOrSha256` en [f3-dossier.md](I-62-prep/f3-dossier.md) §3.10. Sin cambios en producto, pruebas ni superficies
normativas.

## 37. F3 — Binding, independencia, comprobaciones y contratos de rol (orden del Coordinator)

**CI de la entrega de F2:** `d8c55a971c4a52684c853a79ac11a2d47323ec57` → corrida **37076587763** y `c783e4d9353670e8a93cf5de1e3dee21eedddc7a` →
corrida **37076965738**: las dos con attempt 1, event `push`, head_sha exacto y `completed/success`, con los cuatro jobs `success` (Tests (Domain +
Application), UI Tests, Build UI y Build Plugin without AutoCAD). MEASURED por la sesión.

**Orden recibida:** «I-62 — COORDINATOR DECISION: F2 GATE PASS / OPEN F3 / PREPARE MATERIAL AMENDMENT A-1 FOR FC-01 / FC-02», texto pegado sin archivo de
origen. Cuerpo sin las etiquetas de pegado: 12 784 bytes en UTF-8, SHA-256 `d915ff9997caa7395fc8da2ec570996b6f9f9ec938152b47e4813aa382fe9d54`. Resumen fiel
en las decisiones, §34.

### 37.1 DC-07 antes de escribir

MEASURED tras `git fetch` (01:05Z del 2026-10-03): `origin/main` = `819955d6…`, sin cambios. Ramas activas no integradas: I-52 `fb6b5648` (merge-base
`95690c28`), I-63 `527e4b91` (avanzó a su G4 desde `138bc3d4`) e I-64 `39b45f36`. El diff de cada una desde su merge-base con `origin/main` no toca
`docs/AUTOMATION_PLAN.md`, `docs/INITIATIVE_LIFECYCLE.md`, `docs/automation/agent-execution/`, `docs/initiatives/I-62*` ni las clases de prueba de los
protocolos. **Sin solapamiento.** LIFECYCLE es una autoridad caliente (WORKFLOW §7): ninguna hermana la toca. El cuerpo del commit repite la comprobación
antes del push.

### 37.2 Materialización (plano b, inactivo hasta `I62_EFFECTIVE_SHA`)

| Cláusula congelada (V14) | Destino |
|---|---|
| §5; B.5; B.2 (`BindingRef`, `AuthorizationRef`); §20.5.1 (aceptación, materialización, vigencia de acción y acreditación histórica); P-10, P-20 | AUTOMATION_PLAN 16.20 |
| §11.1-11.3 (dimensiones, combinación, referencia, disparadores); §11.4 (puntero a LIFECYCLE) | AUTOMATION_PLAN 16.21 |
| Anexo G.1 (A1'-A8' y los deltas de `Routing`, `Termination`, `Ci`, `Tests`, `Trailer`, `Scope` y `Contract`); ADR-0046 #6 (`not_run`) | AUTOMATION_PLAN 16.22 |
| §20.3 (propiedades, invocación limpia); §20.5 (resultado inválido); §20.7 (correspondencia cerrada); P-19 | AUTOMATION_PLAN 16.23 |
| §20.3.1 (cierre); §20.3.2 (identidad); §20.3.3 (fidelidad, envoltorio, clausura, manifiesto); P-22..P-25 | AUTOMATION_PLAN 16.24 |
| §11.4, alternativa 1 (OD-6); §3.1, fila LIFECYCLE | INITIATIVE_LIFECYCLE §5 (párrafo «Unidades I62») y puntero en §9 |
| B.5, B.2, §20.5.1, §11, Anexo G.1 y G.2 | README §14 (B1-B10, criterios de §14.3, A2', evaluación de la independencia y casos C-13, G1-G7, A1'-A8', `Scope` y casos C-14) |
| B.9, §20.3.1, §20.3.3, B.11 | README §15 (R1-R9, I1-I6, F1-F7, M1-M8) |
| B.10, §20.5, §20.7 | README §16 (V1-V11 y casos C-30) |
| §5 (algoritmo); B.5 (`Cell`) | routing §9 |
| B.5, B.7, B.9, B.10, §20.3.1, §20.3.3, B.11 | los nueve esquemas (§37.3) |
| §20.3.3 punto 5; B.11 («la ruta exacta se fija en F3») | `docs/initiatives/I-62-normative-dependency-manifest.json` (§37.4) |

**No materializado** (F4 o posterior, sin autorización): `state/v2`, `orchestration`, fases del bucle, `NextAction`, QU/QH/QR, rebase, §16.13, punto de
entrada de WORKFLOW, generador de producción del mapa de cláusulas, MC_I62, el destino de los intentos en curso al terminar una vigencia y la validación
exhaustiva del manifiesto en una corrida. `model-catalog.md` no cambia: `CellId` se deriva en routing §9 (E.7 deja el catálogo para secciones nuevas
cuando hagan falta).

### 37.3 Esquemas

Todos en `docs/automation/agent-execution/schemas/`, generados por un guion de la sesión a partir de V14 campo a campo (los borradores locales de §36.5 no
se usaron como fuente). JSON Schema 2020-12, `additionalProperties: false` en todos los objetos, todo campo declarado obligatorio, `null` solo como «no
aplica», sin `$ref`, `oneOf` ni condicionales (las reglas entre campos van en README §14-§16), sin marcas de proveedor en los enums y con patrones exactos
de hash (40 hex para commit y blob; 64 hex para SHA-256; `SizeOrSha256` = `^([0-9]+|[0-9a-f]{64})$`).

| Esquema | Fuente congelada (V14) | Blob |
|---|---|---|
| `binding.v1.schema.json` | B.5; B.2 (`ActorRef`, `SessionRef`, `PreflightRef`, `AuthorizationRef`); §20.5.1 | `13501476b775ca18c4c61ba3e1793796b51067d9` |
| `gate-contract.v2.schema.json` | B.7 sobre `gate-contract/v1`; §20.5.1 (`Materialization` del REVIEWER); B.8.6 | `f644bb1941656b65c62ee474bf37752d328fd091` |
| `delegation.v2.schema.json` | B.7 sobre `delegation/v1` | `dfdbe461270e8fd92200c36bab78b9fdc3385014` |
| `role-invocation.v1.schema.json` | B.9; §20.3; §20.7 | `d54a7ae783ec83d055babdae090899908b616082` |
| `input-closure.v1.schema.json` | §20.3.1; B.9 | `6bdf71843e2c70362a5e2d0ea19c8891e22bcff6` |
| `input-fidelity.v1.schema.json` | §20.3.3; B.8.8 (`premise_independence`); B.9 | `e54990fc541b6420c3cddb45e1a7cc2fefd6c76b` |
| `architect-review-result.v1.schema.json` | B.10.0; B.10.1; §20.5.2 | `e7f5747ba3812c181237828fe72243523038e64f` |
| `reviewer-result.v1.schema.json` | B.10.0; B.10.2 | `a74288eb0e9da43f8a73701098cbff5086fee512` |
| `normative-dependency-manifest.v1.schema.json` | B.11; §20.3.3 (puntos 1-7) | `7ff2127d28de2f126b98da7d4fb2ebb9520374ef` |

**Corrección en dos esquemas de F2** (determinación 2 de las decisiones §34): `BindingRef.Location.CommitSha` → `Commit` en `relay-record.v2` y
`controller-verification.v2`, el nombre congelado de B.2. `AcceptanceDecisionRef.CommitSha` (huella NONE) no es un campo congelado y se conserva.

### 37.4 Manifiesto de dependencias normativas (B.11)

- **Ruta:** `docs/initiatives/I-62-normative-dependency-manifest.json` (IMPLEMENTATION_CHOICE que B.11 deja a F3). Valida contra
  `normative-dependency-manifest.v1.schema.json` con `Test-Json` (PowerShell 7.6.6): `True`.
- **Generador determinista:** `docs/automation/evidence/I-62-F3/gen-manifest.py`. Lee V14 y las autoridades con `git show 4c617e82:<ruta>`, así que no
  depende del disco ni de los finales de línea. Sus reglas (U1-U2, R1-R6, N, D, O, E, X) están declaradas en su cabecera. Dos ejecuciones dan el mismo
  archivo (SHA-256 `e4de08c6…` en LF). El informe `manifest-report.json` lista cada unidad incompleta y su motivo.
- **Contenido (MEASURED):** 116 secciones y 1 709 unidades de V14 (una entrada por cada una) y 32 unidades externas de seis autoridades (AUTOMATION_PLAN,
  LIFECYCLE, WORKFLOW, ADR-0046, README y PROMPT_TEMPLATES), en total 1 857 entradas y 2 896 aristas (2 823 a unidades y 73 a destinos propuestos de §3.1).
  `Revision` = `{4c617e82…, blob}` de cada documento.
- **Metadatos incompletos (`Complete` = false), sin aristas inventadas** (una unidad puede tener más de un motivo): 138 unidades con AMBIGUOUS_REFERENCE (sobre todo «16.N» sin calificador, que
  §20.3.3 punto 2.5 obliga a tratar como ambigua aunque el candidato sea uno solo, y FX-04a/FX-04b, definidos en varias filas), 20 con
  WHOLE_DOCUMENT_UNBOUNDED («AGENTS» sin conjunto de entrada), 1 con UNRESOLVED_REFERENCE (§21), 143 unidades y 11 secciones de partes que se declaran no
  normativas (§0 «resumen», §19 «Riesgos», Anexo F «análisis, no ensayo») y las 32 externas, cuyas dependencias quedan fuera de esta versión. Su
  independencia es UNKNOWN, nunca acreditada por defecto.
- **Superconjunto conservador** (determinación 3 de §34): dentro de una unidad normativa, toda referencia explícita resoluble se declara arista de control.
  Solo puede agrandar una clausura. La revisión del Architect, que B.11 exige, decide qué aristas informativas se retiran.
- **Guardas Core:** clausura, unicidad, anclaje en la revisión congelada, destinos de §3.1 y documentos enteros (`I62_B11_TheManifestIsClosedUnique…`);
  cinco aristas de control explícitas de la Proposal (C-03 → §4.2, P-15 → E.4, I-S15 → I-H01, T19 → §8.8, C-11 → P-10); y el oráculo detecta entrada
  ausente, destino colgante, unidad duplicada, referencia ambigua y una dependencia de control no declarada.

### 37.5 Obligaciones

| Obligación | Clase | Control | Resultado |
|---|---|---|---|
| B.1 en F3 | (i) Core RG | `I62_F3_CoreSchemasAreStrictNeutralAndUseExactHashPatterns`, `I62_F3_OutputContracts…`, `I62_F3_TheContractOracles…` | GREEN |
| textos de F3 | (i) Core RG | `I62_F3_TheNormativeTextsAreMaterializedOnceAndCarryNoProviderMark` | GREEN |
| B.11 | (i) Core RG | las tres guardas `I62_B11_*` | GREEN |
| C-11 | (ii) MC | `f3-mc.py`: positivo ACCEPTED; obligatorio UNKNOWN, NOT_MEASURED, cobertura UNKNOWN y requisito omitido → REJECTED/P-10; adapter, effort y modelo incompatibles → REJECTED (B3) | PASS |
| C-12 | (ii) MC | `BindingRef` vigente → pass; otra unidad, otra tarea, otra revisión, obsoleto, TRANSIENT como custodiado y commit no ancestro → rechazo A2'; candidato PENDING ≠ aceptado; `DecisionRef` fabricado → P-20; observación obsoleta o de otra acción → P-10; transición única PENDING → ACCEPTED; mismo `BindingId` con otro contenido → S-04 | PASS |
| C-13 | (ii) MC | casos 1-11 con sus esperados exactos y SAME-SESSION ROLE insuficiente en una revisión mayor | PASS |
| C-14 | (ii) MC | G.2 filas 1-7, precedencia STOP > REWORK y `c14-scope-sin-pertenencia` (comparación fallida y sin pertenencia → `not_run` → BLOCKED/STOP; fuera de alcance → fail; con pertenencia → pass) | PASS |
| C-30 | (ii) MC | (a)-(i), REVIEWER presentado como ARCHITECT → P-20 y `reviewer-result` con `Verdict` → el esquema lo rechaza | PASS |
| C-40, parte F3 | (ii) MC | (a) materializado y reproducido; (b) siete clases de candidato no elegible; (c) COORDINATOR_DECISION frente a ESCALATION_OWNER; (d) `DecisionRef` fabricado, sin `AuthorizationRef` o en el propio commit → P-20; (h) autorización caducada → rechazo | PASS |

Cada control del MC se ejecuta además con una **mutación** de su procedimiento que debe cambiar algún resultado, y la cambia: C-11, UNKNOWN como satisfecho;
C-12, sin el rebinding posterior; C-13, contexto compartido en (2) y commit dentro de la unidad en (10); C-14, precedencia invertida; C-30, fila PLAN/VERIFY
intercambiada; C-40, UNKNOWN como SATISFIED. Los registros positivos y los negativos que rechaza una regla son válidos por `Test-Json`, así que los
rechaza la regla y no el esquema. Resultado completo: `docs/automation/evidence/I-62-F3/f3-mc-result.json`. **No forman parte de F3:** G.2 filas 8-27 y C-40
(e)-(g) e (i)-(n), que necesitan `state/v2`, la orquestación o los intentos (F4).

**Deuda de I-64:** la regla de `Scope` (AUTOMATION_PLAN 16.22 y README §14.7) es un control de procedimiento para las unidades I62. No repara la deuda nc2 de
I-64, que es una unidad I61 (Proposal V14 §14.1, sin efecto retroactivo), y no lleva A-n.

### 37.6 RED → GREEN

- **Core:** RED antes de los artefactos, 22 seleccionadas y 6 fallidas por ausencia (esquemas y manifiesto); RED de la guarda de textos con los cuatro
  documentos retirados del árbol (1/1 fallida). GREEN: la clase completa en verde (§37.8).
- **MC:** el mismo `f3-mc.py` sobre el árbol de `c783e4d9` (antes de F3) da los seis controles en FAIL (esquemas y procedimientos ausentes); sobre el
  árbol de F3, los seis en PASS.

### 37.7 Elecciones de implementación (preservan el contrato observable)

1. Mismas convenciones que F2: nombres `*.v<n>.schema.json`, sin `$ref`/`oneOf`/condicionales, reglas entre campos en el README.
2. `input-fidelity/v1` como objeto discriminado (`Kind` FIDELITY | SPAN_MAP | PREMISE_INDEPENDENCE con una sola sección no nula): V14 nombra tres artefactos
   (`InputFidelityEvidenceRef`, el mapa `DegradedSpans` y `premise_independence`) con el mismo esquema de B.9.
3. `gate-contract/v2` conserva para `RoleRequirements[].EligibleCells` la forma de celda de `/v1`; `Materialization.EligibleCells` es `{Mode, Cells}` (lista
   cerrada o criterio de ADR-0046 #4).
4. `CellId` = `<AdapterId>:<modelo o ->:<EffortSemantic>`, derivado en routing §9, sin filas nuevas en el catálogo.
5. La evidencia de independencia de una revisión mayor con revisor humano va en la cabecera del registro recuperable de la revisión (V14 §11.4); F3 no
   crea un esquema para esa cabecera.
6. En C-30 (d), la alternativa 2 se evalúa como contrafactual (no está vigente) para mostrar que el binding es válido con ambas políticas.

### 37.8 Pruebas y validación (antes del commit)

- **Guardas focales:** `PrincipalPortabilityProtocolTests` y `AgentExecutionProtocolTests` (I-61), 40/40 (23 de I-62 + 17 de I-61). Siguen en verde las de F1
  (C-01, C-03), las de F2 (C-05, C-09, C-10, C-19: los cinco `/v1` byte a byte) y las de I-61.
- **Core Full sobre el árbol** (antes del commit, con los esquemas, los textos y las pruebas de F3): 12 419/12 419. El Core Full del SHA exacto se registra
  después del commit.
- **MC:** `python docs/automation/evidence/I-62-F3/f3-mc.py . docs/automation/evidence/I-62-F3` → C-11, C-12, C-13, C-14, C-30 y C-40 en PASS
  (PowerShell 7.6.6).
- **Validación:** 44 enlaces relativos de los cuatro documentos tocados, 0 rotos; tablas con el mismo número de columnas por fila; `git diff --check`
  limpio; todos los JSON nuevos parsean y el manifiesto valida contra su esquema; el estado sigue siendo YAML válido.
- **Neutralidad:** los textos nuevos de AUTOMATION_PLAN, README, routing y LIFECYCLE y los enums de los nueve esquemas no contienen marcas de proveedor
  (guardas Core).

**Blobs** de los demás archivos de la entrega (los cuatro registros de custodia no se listan a sí mismos):

| Archivo | Blob |
|---|---|
| `docs/automation/agent-execution/schemas/relay-record.v2.schema.json` | `dca5b29c1a260b679efc0fb249c73850925302f0` |
| `docs/automation/agent-execution/schemas/controller-verification.v2.schema.json` | `e1faa9eca5c50b2b3a9f91575575614366962720` |
| `docs/initiatives/I-62-normative-dependency-manifest.json` | `cce3699fec82726c446996d11f9abe77ba728706` |
| `docs/AUTOMATION_PLAN.md` | `52fd8f66591da49b3fb78f7bbe4580d76f8339e3` |
| `docs/INITIATIVE_LIFECYCLE.md` | `f19896a8f1a7c82f74bac7231636f68462a87271` |
| `docs/automation/agent-execution/README.md` | `3fb9a44b9cdcb72ddd1a575bb919f1c265d9b8ae` |
| `docs/automation/agent-execution/routing.md` | `bba08fc4686d7192deeb559752636a467103b4b1` |
| `tests/RackCad.Tests/PrincipalPortabilityProtocolTests.cs` | `0d39ea71656586231457657fa600ddb5079cc2ef` |
| `docs/automation/evidence/I-62-F3/gen-manifest.py` | `9ed08ac21160ed5287a8a462610d4d7017108ff1` |
| `docs/automation/evidence/I-62-F3/manifest-report.json` | `6adcac3abcdd7abc1171dcee7ced1c1bad036379` |
| `docs/automation/evidence/I-62-F3/f3-mc.py` | `1bab344fe24339c322c53204c99649cb1c415371` |
| `docs/automation/evidence/I-62-F3/f3-mc-result.json` | `5a8923e34b1792fdf56011ca44a37e3b5ccc8fd3` |

## 38. F3 sobre el SHA exacto y paquete de la enmienda A-1 (orden §34)

### 38.1 F3 sobre `f9234cb98f84b11f79acacb7677223424b63abc4`

- **CI exacta:** corrida **37084798120**, attempt 1, event `push`, rama `architecture/portabilidad-coordinador-principal`, head_sha exacto,
  `completed/success`. Jobs: Tests (Domain + Application) (111092723125), Build UI (111092723032), UI Tests (111092722900) y Build Plugin without AutoCAD
  (111093089874), los cuatro `success`.
- **Core Full del SHA exacto,** con el árbol limpio antes y después: 12 419/12 419 (inicio 01:06:44Z), TRX SHA-256 `5106755b58e2ab43…`.
- `git ls-remote` de la rama = `f9234cb9…` tras el push. MEASURED por la sesión.

### 38.2 Enmienda A-1 (FC-01, FC-02): preparada, sin aplicar y sin invocación

- **Registro:** [I-62-A-1.md](../../initiatives/I-62-A-1.md), con el formato de LIFECYCLE §6: Freeze identificado (FREEZE_SHA `b64a3b64…`, V14
  `4c617e82…`/`34ad80ea…`), `Applies-to: I-62`, cláusulas anteriores literales, delta exacto (D1-1..D1-9 y D2-1..D2-8), motivo, M-01..M-08 (M-02, M-03,
  M-04 y M-05 activados), efecto en C-29, C-31, C-34, C-36, C-38 y en las pruebas de rebase de C-15, consecuencias del Owner (ninguna identificada) y
  autoridades Architect + Coordinator con veredictos PENDING.
- **Paquete del Architect:** [I-62-architect-package-A-1.md](../../initiatives/I-62-architect-package-A-1.md): identidad del objeto, veredicto que se
  solicita, insumos canónicos con su blob, resumen del delta, siete preguntas y condiciones de la invocación.
- **Contra-ejemplos exactos** (preparación de F4, no producción): [a1-counterexamples.py](I-62-A1/a1-counterexamples.py) codifica, solo para los campos que
  tocan FC-01 y FC-02, el texto literal de V14 y el delta, y recorre 15 trazas simbólicas. MEASURED (resultado en
  [a1-counterexamples-result.json](I-62-A1/a1-counterexamples-result.json), todas con su esperado):
  - literal: abrir un segundo bucle es inválido por los dos caminos (§20.5 sin transición e I-P13 por `loop.object`), y además nace agotado (I-S18, P-18);
    reconciliar la orquestación tras un rebase viola I-P13, B.8.8 e I-P05; no reconciliarla es **válido** para el validador literal aunque el `Target`
    del intento siguiente ya no esté en la rama (el hueco);
  - enmendado: LOOP_CLOSED y un bucle nuevo con otra autorización son válidos; reutilizar la autorización, reiniciar o cambiar su entrada, cerrar con una
    solicitud abierta y poner `loop.object` a `null` fuera del cierre son inválidos; la reconciliación con imágenes es válida, su omisión la detecta I-H02,
    un intento LAUNCHED conserva su `Target`, y reescribirlo, cambiar el blob o cambiar contadores son inválidos.
- **Frontera de invocación:** la orden vigente no autoriza lanzar una sesión nueva del Architect. La sesión se detiene ahí con el paquete completo; no elige
  runtime ni declara veredicto. OD-2 sigue sin resolver.
- **Efecto:** solo la materialización de F4 que toca `orchestration.budgets`, `orchestration.loop`, `review_requests`, `StateFields` y la reconciliación de
  §8.8 queda bloqueada hasta el veredicto. La preparación de F4 puede continuar. F3 no cambia.

| Archivo | Blob |
|---|---|
| `docs/initiatives/I-62-A-1.md` | `09ca93285975c4b7af6471d6ae91bfa12c94a1fc` |
| `docs/initiatives/I-62-architect-package-A-1.md` | `071cecba1fb8bdefc9a4e5554dd47a0459f8a1d2` |
| `docs/automation/evidence/I-62-A1/a1-counterexamples.py` | `d7a5d1ef5558fd76c5ffe224fe71e807b44b240d` |
| `docs/automation/evidence/I-62-A1/a1-counterexamples-result.json` | `9fa501b607d56c9bd902769dcd5f669450a3cb4c` |

## 39. Preparación tras F3 GATE PASS: kit de la revisión de A-1 y preparación de F4, F6, READY, OV y cierre (órdenes §35 y §36)

**Órdenes recibidas:** §35, 7 435 bytes, SHA-256 `a2d96afb…`; §36, 12 452 bytes, SHA-256 `750640c6…` (identidades completas en las decisiones). Nada de
esta sección materializa F4 ni F6: son preparación y prototipos en `docs/automation/evidence/I-62-prep/`.

### 39.1 Revisión del Architect de A-1: lista, no lanzada (AUTONOMY_GAP)

- **Kit:** `a1-review-kit/`. MEASURED:
  - clon limpio `D:\r62-arch-a1`, detached en `bf7b0d9c`; los blobs del objeto (`09ca9328…`) y del paquete (`071cecba…`) coinciden con la orden;
  - cierre de 22 insumos canónicos y 12 transitivos con blob y SHA-256 (61 caracteres no ASCII);
  - prompt (SHA-256 `428eb581…`) y orden literal (`a2d96afb…`);
  - preflight del camino de lectura (Read): 64 líneas FAITHFUL_NORMALIZED, incluida una de 3 683 caracteres;
  - esquema estricto del resultado;
  - auditoría posterior `post-review.py`, cuya autoprueba da ACCEPTED/NOT_ACCREDITED con los motivos exactos.
- **Por qué no se lanzó:** OD-2 bloquea `codex-cli` y `codex-desktop-session`; OD-3 bloquea `claude-cli`; esta sesión no tiene `start_session`; las sesiones
  existentes («ARC - 02» es de otro proyecto, y las de revisiones anteriores arrastran contexto) no son limpias. Se creó la tarea `task_fce8049e`: un clic
  del Owner abre la sesión revisora en el clon limpio. **AUTONOMY_GAP = OWNER_CLICK_REQUIRED**. No se rodeó ninguna frontera.
- **Transportes** (`transport-options.md`, MEASURED sin invocar): `claude-cli` está instalado en `~/.local/bin/claude.exe` (Claude Code 2.1.270, fuera del
  `PATH`), con autenticación UNKNOWN (OD-3); `codex-cli` cumple, pero espera OD-2.

### 39.2 F4: IMPLEMENTATION_READY_PENDING_A1 (prototipos medidos, fuera de producción)

| Pieza | Resultado (MEASURED) |
|---|---|
| matriz de campos `f4/state-v2-fields.{json,md}` | 91 campos; 12 con SHA; los SHA fuera de `StateFields` literal son los históricos de `protocol`, `last_rebase` y los tres de la orquestación (= FC-02) |
| lector YAML `f4/yaml-subset/` | 27/27 casos; ida y vuelta de un `state/v2` de 217 líneas; 4/4 mutaciones; HEADER extrae `claim_id` de 38/39 estados reales (el otro no es un estado `rackcad-automation-state`). **DEP-F4-YAML = NOT NEEDED** |
| oráculo `f4/oracle/` | 62 transiciones, literal y A-1; 45/45 mutaciones de estado y 11/11 mutaciones del validador detectadas |
| rebase `f4/rebase-proto/` | rebase real con patch-ids iguales; lease rechazado con una escritura ajena y aceptado al reintentar; FC-02 demostrado (el `Target` vivo no existe en un clon limpio `--no-local`); A-1 reconcilia y el sucesor pasa I-H02 |
| resolver `f4/compat-proto/` | contrato real de G3 (blob `9b5ef6df`) byte a byte; Evaluate, Classify, Resolve, lectura compuesta y Validate MV-1..MV-7; C-20c-1 (a, b), C-20c-2 y N-a..N-s: 24/24; salida determinista `662ebd2f…` |
| `f4/a1-impact-map.md`, `f4/test-blueprint.md` | delta de F4 en las dos variantes; plano de pruebas C-15..C-42 con clase, fixture, casos, mutaciones, primera invariante y superficie |

**Hallazgo no material SM-05** (`freeze-issues.md`): la recuperación (§20.6 caso B.1, LAUNCH_UNCERTAIN) devuelve la fase de ARCHITECT_INVOKED a
(RE)REVIEW_PENDING; el diagrama de §20.5 no lista esa arista e I-P13 no enumera aristas de fase. Tratamiento en F4: coherencia fase/intento de I-S18 y las dos
aristas de recuperación. Ningún hallazgo material nuevo.

### 39.3 F6, decisiones del Owner, READY, OV y cierre

- **F6:** `f6/recipes.md` (fixture completo y recetas FX-01..FX-06 con la clasificación del AUTONOMY_GAP: limitación esperada hoy bajo I61; en FX-06, UNVERIFIED
  si falta una OD y UNSUPPORTED si con las OD no hay transporte); `f6/fx04a/fx04a_proto.py`: ensayo mecánico PASS (QH válido, reconstrucción desde un clon
  limpio, comparación igual con un oráculo con hash previo, artefacto transitorio invisible, N11 → UNKNOWN).
- **OD:** `owner-decision-packets.md` en formato de una línea. **OD-2, medición pasiva** (sin ejecutar binarios):
  - `config.toml` con SHA-256 `091540ED2DE6CFAC…`, escrito el 2026-10-04 a las 21:39Z, con 110 nombres (20 saneados);
  - paquete `OpenAI.Codex_26.930.3930.0`; la ruta antes verificada `bin/a51e250fa15c740a` ya no aparece;
  - la huella actual no coincide con ninguna anterior (`37DD3559`, `42E15A03`, `40c27b57`, `155933B3`), así que la línea base de I-63 también envejeció.
  Recomendación de la sesión, no decisión: `OD-2 = B`.
- **READY/OV/cierre:** `ready-candidate.md`, `ov-scripts.md` y `closure-plan.md`. En el cierre, las hermanas tocan ROADMAP (todas) y el índice ADR (I-52;
  I-64 en su cierre).
- **Investigación del repositorio:** `repo-research.md` (CI de Core superficial, sin dependencias de YAML ni de JSON Schema, sin procesos ni Git en las
  pruebas).

### 39.4 DC-07 y límites

DC-07 (2026-10-05): `origin/main` = `819955d6`; I-52 `fb6b5648`, I-63 `fdd4b651` e I-64 `39b45f36` no tocan ningún archivo de esta entrega, que está entera
bajo `docs/automation/evidence/I-62-prep/`, más las decisiones, la evidencia, el estado y el contrato de I-62. Sin cambios en superficies normativas ni de producción;
A-1 y su paquete, intactos.

### 39.5 Riesgo operativo NDC-01: precisión del manifiesto B.11 (medido tras el commit de preparación)

CI del commit de preparación `f89b5284c7c80306f13e95328b4b42c5c5c947c8`: corrida **37254512086**, push, head_sha exacto, cuatro jobs en `success`. DC-07
repetido: I-63 avanzó a `33032f61` sin tocar ningún archivo de I-62.

`f4/ndc-proto/ndc_stats.py` (MEASURED) recorre la clausura de cada una de las 1 825 unidades de V14 sobre el manifiesto de F3:
- 919 clausuras son acreditables y 906 son UNKNOWN;
- el tamaño es bimodal (mediana 1; p90 1 674; máximo 1 692);
- los bloqueos más frecuentes son las unidades externas de I-61, incompletas por diseño del generador.

Con un transporte DEGRADED_BOUNDED, casi ningún hallazgo con premisas referidas podría acreditarse. No es un defecto del Freeze: la regla falla cerrado
como debe. Es un límite de la precisión del manifiesto, cuyo remedio (retirar aristas informativas y dar entrada terminal a las definiciones externas) corresponde a la
revisión del manifiesto por el Architect (B.11). La sesión no cambia el manifiesto, que es un artefacto de F3 con GATE PASS. Detalle en `f4-dossier.md` §8.

## 40. Revisión formal del Architect de A-1: lanzamiento, identidad observada, auditoría y resultado (orden §35; instrucción §37)

CI del commit `68152b580cd6b78b2b9db8a9e26636cb9f8f2112` (riesgo NDC-01, §39.5): corrida **37255159204**, push, head_sha exacto, cuatro jobs en
`success`.

### 40.1 Lanzamiento

La tarea `task_fce8049e` salió de la cola sin lanzarse. Por la instrucción del Owner (§37), la sesión creó `task_c6305551` con el mismo texto. El control
del escritorio no sirvió (la app Claude se oculta mientras se la controla) y una tarea programada no sería limpia. El Owner la pulsó: la sesión revisora
`local_d5130a91-c4df-409d-916a-9405b506836c` corrió entre las 02:47:22Z y las 03:22:17Z del 2026-10-05. Hubo una sola invocación.

### 40.2 Identidad observada (MEASURED por el invocador)

- `claude-opus-5-5`, effort `xhigh`, Claude Code 2.1.286, sin subagentes y sin memoria del proyecto del autor (`D--r62-arch-a1/memory` vacío).
- El worktree `exciting-jones-a687da` se abrió sobre `main` del clon (`819955d6`), no sobre `bf7b0d9c`. El revisor leyó por ruta absoluta el clon detached en
  `bf7b0d9c` y comprobó HEAD, blobs y árbol limpio. `CLAUDE.md` y `AGENTS.md` son iguales en ambos commits.
- Independencia: sesión y contexto distintos; mismo operador humano y mismo proveedor y modelo. La valoración es del Coordinator.
- Al terminar, el clon y el worktree del revisor están limpios.

### 40.3 Auditoría y acreditación

`post-review.py` = **NOT_ACCREDITED**, con 16 motivos. Fidelidad: 36/36 FAITHFUL_NORMALIZED. Premisas: 39/39 citas encontradas. Esquema válido. Escaneo: sin
destinos prohibidos ni rutas de archivo fuera del cierre.

Clasificación del invocador (`audit-classification.json`):

| Clase | Motivos |
|---|---|
| OUTSIDE_CLOSURE_SEARCH (dos Grep sobre directorios; declarados) | 2 |
| UNLISTED_READONLY_METADATA (`wc`/`awk`/`grep` sobre archivos del cierre) | 6 |
| OWN_OUTPUT_READ | 2 |
| ALLOWED_ACTION_FORM (defecto de precisión de la auditoría) | 5 |
| AUDIT_FALSE_NEGATIVE (sí comprobó el hash) | 1 |

La acreditación (P-22) la decide el Coordinator; no hay reintento.

### 40.4 Resultado literal

**CHANGES REQUIRED**:
- REQUIRED A62-A1-01 a 06: presupuestos frente a sustitución con el bucle abierto; LOOP_CLOSED tras EXPIRED o REVOKED; alcance por `loop.type`
  (REVIEWER); Target de LAUNCHING en el caso B.1 tras un rebase; ingestión tras reconciliar el objeto; identidades de rama de los bindings custodiados
  frente a I-S18 y F3.
- OPTIONAL A62-A1-O1 a O6.
- `OwnerDecisionRequired` = false.

Custodia:
- `docs/automation/evidence/I-62-architect-A-1/R20261003T023945Z-cab7/` (`output.json`, SHA-256 `1a6f2c4b…`);
- registro `docs/initiatives/I-62-architect-review-A-1.md`.

La transcripción no se versiona: 2 294 522 bytes, SHA-256 `53ea184a…`.

### 40.5 Límites

No se edita A-1, no se crea su versión corregida ni A-2, no se abre F4 y no se decide nada del Owner. STOP: el Coordinator dispone la acreditación y los
hallazgos.

## 41. A-1 corregida tras la disposición del Coordinator (decisiones §38)

CI de la custodia de la revisión, `576006e83ad56f50df6c816f18f95297589de399`: corrida **37259840766**, push, head_sha exacto, cuatro jobs en `success`.

### 41.1 Objetos

| Archivo | Blob | Contenido |
|---|---|---|
| `docs/initiatives/I-62-A-1.md` | `23dd16b2b135cdb7e1e2b1e18e2b6595253e92d8` | A-1 corregida, todavía PROPUESTA: D1-1..D1-15 (identidad y presupuesto del bucle, sustitución y continuación, LOOP_CLOSED, alcance por tipo, `BudgetSnapshot`, linajes) y D2-1..D2-12 (reconciliación, LAUNCHING, `rebase_history`, ResolveBranchRef, aplicación a F3, EquivalentReviewedObject); materialidad; pruebas; OBS-A1-01; matriz y cambios frente a `09ca9328` |
| `docs/initiatives/I-62-architect-package-A-1.md` | (se publica en este commit; no lleva su propio blob) | paquete para la revisión formal nueva: insumos con blobs, nueve preguntas y condiciones con las lecciones de la corrida no acreditada |
| `docs/initiatives/I-62-architect-review-A-1-disposition.md` | `adf77aa5e529771d598239d72be020ebd5767f17` | disposición del Coordinator y matriz exacta hallazgo → delta → regla → traza |
| `docs/automation/evidence/I-62-A1/a1-counterexamples.py` | `ba7e6f951df9a4006cce3c570bae4fa8f9a3159e` | arnés reescrito (O5) |
| `docs/automation/evidence/I-62-A1/a1-counterexamples-result.json` | `70cc85d6ee84ee2847441be033acd9e52b2370d5` | resultado determinista (dos corridas idénticas byte a byte) |

### 41.2 Contra-ejemplos (MEASURED)

- **Cobertura:** 56 trazas (18 VALID, 38 INVALID), todas PASS.
  - Cada traza negativa fija el conjunto **exacto** de reglas que debe violar, y una regla incidental la haría fallar.
  - Ninguna regla del catálogo queda sin ejercitar: cada una aparece en algún conjunto esperado.
- **Por hallazgo:** A62-A1-01: 7 trazas; 02: 12; 03: 5; 04: 6; 05: 4; 06: 10. O1: 2; O2: 2; O3: 3; O4: 3; O5: 1.
- **`fc01-enmendado-object-null-fuera-del-cierre`** falla ahora solo por A1-P02.
- **V14 literal:** cinco trazas siguen mostrando los defectos del Freeze; la nueva, `fc06-literal-referencias-tras-rebase`, muestra el de las referencias de
  rama.
- **Lo modelado:** `loop.type`, la escalada, los linajes, la sustitución, la continuación, EXPIRED y REVOKED, las tres ramas de LAUNCHING, la equivalencia en la
  ingestión y la cadena de mapas con uno y dos rebases, un mapa intermedio ausente, un blob cambiado y un clon sucesor limpio.

### 41.3 Contratos F3 y límites

- **Esquemas F3:** ninguno cambia. `RebaseMap.Commits[]` ya lleva la cadena y `BudgetSnapshot` sigue siendo escalar. Se enmiendan solo **textos** F3 para
  I-62 (AUTOMATION_PLAN 16.20 paso 3 y su lista de la autorización; README §14.3 «Reproducción» y §14.4 regla 3).
- **OBS-A1-01** (fuera del delta, para el Coordinator): V14 no define la salida a NONE de un bucle REVIEWER.
- **Prototipos de F4 desactualizados:** el oráculo y el mapa de impacto de `I-62-prep/f4/` modelan el blob anterior. Se actualizan al abrir F4; no son
  producción.
- **Límites:** sin F4, sin A-2 y sin lanzar la revisión formal nueva, que necesita una autorización nueva.
- **DC-07:** I-63 avanzó a `55a66b3c` sin tocar archivos de I-62; I-52 e I-64 siguen igual.

## 42. A-1 corregida para OBS-A1-01 (decisiones §39)

CI de la corrección anterior, `fff3bbb070b065137401f81f85118ea851ea5888`: corrida **37262643940**, push, head_sha exacto, cuatro jobs en `success`.

### 42.1 Objetos

| Archivo | Blob | Contenido |
|---|---|---|
| `docs/initiatives/I-62-A-1.md` | `9c621fce0588f32115e3151b6f75413c0a167c2e` | D1-16 REVIEWER_SATISFIED y guarda de tipo; D1-17 condiciones, pertenencia y severidad BLOCKING/ADVISORY; D1-18 LOOP_CLOSED de REVIEWER (S y E); D1-19 `reviewer_closures[]`; D1-13 regla final por tipo; materialidad propia de OBS-A1-01; pruebas; matriz y cambios frente a `23dd16b2` |
| `docs/initiatives/I-62-architect-package-A-1.md` | (en este commit; no lleva su propio blob) | objeto nuevo, insumos con los esquemas `reviewer-result/v1` y `gate-contract/v2`, lo que la revisión debe cubrir según §39 y preguntas 10-12 |
| `docs/initiatives/I-62-architect-review-A-1-disposition.md` | `396c7a1451561e912529cff933b8df07ba3ef596` | segunda disposición y fila de OBS-A1-01 en la matriz, con R1..R10 |
| `docs/automation/evidence/I-62-A1/a1-counterexamples.py` | `3f5129fa6f9dcbf7120d16954fd56c0d48c24863` | reglas A1-R01..A1-R04 y V14-P20-reviewer; trazas R1..R10 |
| `docs/automation/evidence/I-62-A1/a1-counterexamples-result.json` | `4db4aac4983741b0a5454b84056b44483aa12e51` | resultado determinista (dos corridas idénticas byte a byte) |

### 42.2 Contra-ejemplos (MEASURED)

- **Cobertura:** 73 trazas (24 VALID, 49 INVALID), todas PASS. Cada negativa fija el conjunto exacto de reglas, y ninguna regla queda sin ejercitar.
- **OBS-A1-01:** 20 trazas.
  - R1: 2 (cierre con NO_FINDINGS; REVIEWER con ARCHITECT_SATISFIED → A1-R01).
  - R2: 1.
  - R3: 2 (REVIEWER_SATISFIED → A1-R02; cierre → A1-R03).
  - R4: 1 (V14-P20-reviewer).
  - R5: 2 (`instance_id` → A1-F06; entrada del Architect → A1-F04 y A1-P04).
  - R6: 1.
  - R7: 4 (EXPIRED y REVOKED válidos; reescritura a SUPERSEDED y cierre sin decisión → A1-R03).
  - R8: 2 (reinicio → A1-P06 y A1-R03; historia borrada → solo A1-R04).
  - R9: 2 (ARCHITECT_REVIEW se abre tras el cierre; sin cierre → A1-P01, A1-P02 y A1-P04).
  - R10: 3 (EXECUTION sin cambio; con identidad del Architect → A1-F06; LOOP_CLOSED aplicado a EXECUTION → A1-P01 y A1-P07).
- **Traza retirada:** `a62-a1-03-LOOP_CLOSED-no-se-extiende-a-REVIEWER` documentaba el comportamiento anterior (el REVIEWER sin cierre); la sustituyen las
  trazas R1..R10.

### 42.3 Contratos F3 y límites

- **Esquemas F3:** ninguno cambia. `reviewer-result/v1` ya trae `Disposition` y `Severity`, y `gate-contract/v2` no gana campos.
- **Requisito más estricto del contrato de gate:** ningún campo de `RoleRequirements[]` endurece la regla de B.10.2, así que en I-62 un ADVISORY no bloquea.
  Permitirlo exigiría un campo nuevo (M-05), que A-1 no introduce; se señala al Coordinator.
- **Cierre (E) del REVIEWER:** se permite con un BLOCKING abierto, que sigue abierto, y el requisito operativo queda sin satisfacer. Así se evita un bucle sin
  cierre para siempre sin fingir la satisfacción.
- **Límites:** sin F4, sin A-2 y sin lanzar al Architect.
- **Ventana de I-63:** ya liberada antes de la orden (merge `bb0d5522`). Sin rebase, sin `docs/ROADMAP.md` ni `docs/HANDOFF.md` y sin push a `main`.

## 43. Revisión formal del Architect de la A-1 corregida (decisiones §40)

CI de la corrección de OBS-A1-01, `0ad410f894a9411cc9e4f454481ba14791109133`: corrida **37264373929**, push, head_sha exacto, cuatro jobs en `success`.

### 43.1 Kit y lanzamiento

- **Clon y corrida:** clon `D:\r62-arch-a1r2` (`git clone --no-local`; `main` = `0ad410f8`; sin remoto ni otras ramas); corrida `D:\r62-arch-a1r2-run`, RunId
  R20261005T044948Z-ac67.
- **Cierre:** 26 insumos canónicos y 12 transitivos, con el contrato de acciones ID-1, ID-2, RD-1, RD-2, EX-1, EX-2 y OWN-1.
- **Prompt:** SHA-256 `03281e4f…`.
- **Auditor v2:** autoprueba PASS antes del lanzamiento.
- **Preflight:** 3 lecturas y 63 líneas FAITHFUL_NORMALIZED.
- **Lanzamiento:** el Owner pulsó `task_434932cc`. La sesión `local_aa66b6f6-77e5-43f5-b703-a06e48da8de8` corrió de 04:57:08Z a 05:19:16Z del 2026-10-05.

### 43.2 Identidad observada (MEASURED)

`claude-opus-5-5`, effort `xhigh`, Claude Code 2.1.286, 110 mensajes, sin subagentes. El worktree `modest-goldberg-d89b01` nació sobre `main` = `0ad410f8`, y el
de esta vez sí quedó en el commit exacto. La memoria `D--r62-arch-a1r2/memory` está vacía. Después de la corrida, el clon y el worktree están limpios.

### 43.3 Auditoría y acreditación

| Auditor | Resultado | Detalle |
|---|---|---|
| v2 (el del kit) | **NOT_ACCREDITED**, 57 motivos | 54 por líneas de dos heredocs `python - <<'EOF'` tomadas por comandos (EX-1: solo importan el arnés del cierre); 2 por la variable `$S` (salidas propias) sin expandir; 1 por la ruta del proyecto de `dotnet test` (EX-2) |
| v2.1 (corrección de método, solo esos tres defectos; autoprueba PASS) | **ACCREDITED**, 0 motivos | — |

- **Hechos:** sin lecturas fuera del cierre. Once Grep, cada uno sobre un archivo. Fidelidad: 30/30 FAITHFUL_NORMALIZED. Premisas: 32/32 encontradas y
  entregadas fielmente. El resultado cumple el esquema y no tiene observaciones de coherencia.
- **Desviaciones literales declaradas por el revisor:** las llamadas 4 y 20 (`cd` al clon), 36 (`git show … | sha256sum`) y 57 (`git -C <worktree>` en la
  comprobación final). Todas son de solo lectura y los dos auditores las admiten, así que el kit es inconsistente entre la tabla y la lista blanca.
- **Decisión:** la acreditación es del Coordinator, sin reintento.

### 43.4 Resultado literal

**CHANGES REQUIRED.**
- **REQUIRED:**
  - A62-A1R-01: un bucle REVIEWER agotado (EXHAUSTED) no tiene cierre;
  - A62-A1R-02: `loop.object` del REVIEWER (y de EXECUTION) en un rebase, D1-10 frente a D2-1..D2-5;
  - A62-A1R-03: la reapertura de un bucle REVIEWER con una autoridad terminada, y la enmienda F3 no declarada de la vigencia de 16.20 y del criterio
    VALIDITY de README §14.3.
- **OPTIONAL:** A62-A1R-O1..O5.
- **Disposiciones:** A62-A1-01..06 CLOSED; OBS-A1-01 STILL_OPEN.
- **Owner:** `OwnerDecisionRequired` = false.
- **Materialidad:** global, de acuerdo; para OBS-A1-01, M-05 = SÍ.

Custodia en `docs/automation/evidence/I-62-architect-A-1/R20261005T044948Z-ac67/`. Registro: `docs/initiatives/I-62-architect-review-A-1-r2.md`.
Transcripción no versionada: 2 299 386 bytes, SHA-256 `50b38f31…`.

### 43.5 Límites

Sin editar A-1, sin A-2, sin F4, sin rebase, sin ROADMAP ni HANDOFF y sin decidir materias del Owner. STOP.

## 44. Orden nocturna: rebase y A-1 corregida para A62-A1R-01..03 (decisiones §41)

CI de la custodia de la revisión formal, `1614908d37e0e5bb56d8ddf8e6775515278332c8`: corrida **37268887992**, push, head_sha exacto, cuatro jobs en
`success`.

### 44.1 Rebase (WORKFLOW §4.2)

- **Motivo:** `main` avanzó de `819955d6` a `bb0d5522` con la integración de I-63. La ventana de I-63 ya estaba liberada y no había otras ventanas
  activas. I-63 no tocó ninguna autoridad compartida (AUTOMATION_PLAN, LIFECYCLE, WORKFLOW, agent-execution, AGENTS, PROMPT_TEMPLATES ni CLAUDE).
  `git merge-tree` salió limpio.
- **Resultado:** 38 commits rebasados con patch-id igual. Nueva punta `83035d99`. Core 12618/12618 en local, por las pruebas que aportó I-63.
- **Publicación:** `git push --force-with-lease=…:1614908d`. CI de la punta rebasada `83035d99`: corrida **37270782878**, push, head_sha exacto, cuatro
  jobs en `success`.
- **Mapa original → imagen:** `I-62-prep/night-2026-10-05/rebase-map.json`. Identidades clave:

  | Original | Imagen |
  |---|---|
  | V14 `4c617e82` | `1d5cdbec` |
  | Freeze `b64a3b64` | `fb49fceb` |
  | F1 `12660ea7` | `02a81831` |
  | F2 `1eddbf48` | `ffb509e8` |
  | F3 `f9234cb9` | `42115503` |
  | `0ad410f8` | `4e36d77c` |
  | `1614908d` | `83035d99` |

  Las identidades por blob no cambian, y los registros siguen citando los SHAs originales como hechos históricos.

### 44.2 A-1 corregida

| Archivo | Blob | Contenido |
|---|---|---|
| `docs/initiatives/I-62-A-1.md` | `39c2f8317ec381fa60c3564a834278df8898101c` | D1-18 (E) con EXHAUSTED y con la revocación en la propia decisión (O2); D1-10, D1-13 y D2-4 (una regla de rebase por tipo; delta de EXECUTION declarado; enumeración exacta, O1); D1-15 (`OpenFindings` por autoridad, O3); D1-17 y D1-19 (satisfacción positiva, O5); D1-20 (identidad de la autoridad REVIEWER y no resurrección); D1-21 (enmienda propuesta de 16.20 y VALIDITY de §14.3); D2-9 (cada mapa en orden, O4); M-05 de OBS-A1-01 = sí |
| `docs/automation/evidence/I-62-A1/a1-counterexamples.py` | `078bd48c75dec242e373e10413b988f35a7ad3cf` | reglas nuevas A1-R05 y A1-R06; A1-R03, A1-P11 y A1-P16 ampliadas; 21 trazas nuevas |
| `docs/automation/evidence/I-62-A1/a1-counterexamples-result.json` | `1e534996c7a9d70d2dfbf3e0605566735009187a` | 94 trazas (32 VALID y 62 INVALID), todas PASS; determinista |
| paquete y disposición | (en este commit) | objeto nuevo, preguntas 13-15, lecciones de la revisión formal y matriz de A62-A1R-01..03 y O1..O5 |

**Contra-ejemplos (MEASURED):**
- **Antes:** 73 trazas, todas PASS. **Ahora:** 94, todas PASS, y ninguna regla queda sin ejercitar.
- **Cobertura nueva:** A62-A1R-01: 4 trazas; 02: 5; 03: 4; O1: 2; O2: 2; O3: 3; O4: 2; O5: 3.
- **Negativos:** cada uno falla por la regla buscada:
  - EXHAUSTED sin decisión, con un intento vivo o con un registro que finge REVIEWER_SATISFIED → A1-R03;
  - `loop.object` del REVIEWER o de EXECUTION sin reconciliar → I-H02;
  - reapertura tras S, tras E o tras un rebase → A1-R05;
  - satisfacción sin evidencia → A1-R06.

### 44.3 Límites

- Sin acreditar la revisión anterior ni declarar su acuerdo.
- Sin tocar V14, el Freeze, `/v1`, `main`, ROADMAP, HANDOFF, FOUNDATIONS ni el índice ADR.
- Los textos F3 de D1-21 y D2-11 son enmiendas propuestas, sin editar.

### 44.4 Publicación y coherencia del paquete

- **CI de `252be61ed5f17c66a425a983d31baf419543f66c`:** corrida **37272010368**, push, head_sha exacto, cuatro jobs en `success`.
- **Coherencia del paquete** (detectada al preparar el kit de revisión v3, antes de fijar el objeto de la revisión):
  - el paquete citaba el blob `396c7a14…` de la disposición, que ya no era el vigente;
  - la «Frontera» de §5 todavía decía que lanzar la revisión necesitaba una autorización nueva, aunque la orden nocturna (decisiones §41, punto E) ya la da;
  - la cabecera y §5 de la disposición seguían en el estado anterior.
- **Corrección:** paquete y disposición actualizados en el commit siguiente, sin cambiar A-1: el blob `39c2f831` y el arnés siguen iguales.

## 45. Kit v3 de la revisión formal de la A-1 corregida (decisiones §41, punto E)

- **Objeto:** commit `9dcfc08d5171b01df97b3a492a60218c5ab2453e` (CI **37272599376**, push, head_sha exacto, cuatro jobs en `success`).
  - A-1: blob `39c2f831`;
  - paquete: `82bea6e6`;
  - disposición: `2c7a8e7e`.
- **Kit:** `docs/automation/evidence/I-62-architect-A-1/R20261005T063359Z-86e3/`, con README y `kit/`. Contiene:
  - cierre de 29 insumos canónicos y 12 transitivos;
  - contrato de acciones idéntico a la lista blanca del auditor v3;
  - prompt SHA-256 `b221c079…`;
  - autoprueba del auditor PASS: dos transcripciones conformes ACCREDITED; la desviada, NOT_ACCREDITED por exactamente sus 17 motivos;
  - preflight de Read 3/3 fiel (108 líneas).
- **Lanzamiento:** HUMAN_LAUNCH_REQUIRED. La tarea de la app `task_7b4dd3ee` se creó una sola vez; la sesión no la lanza.
- **Límites:** sin veredicto ni acreditación. La segunda invocación que permite la orden queda sin usar.

## 46. Dossiers P4 y P8 para una A-2 futura (orden nocturna §7; sin aplicar)

CI de la custodia del kit v3, `ff424a483e5ee0ffd1d37493bab4e98aed633d70`: corrida **37273621297**, push, head_sha exacto, cuatro jobs en `success`.

| Dossier | Caso de I-63 reproducido | Prototipo (EXPERIMENTAL — NOT AUTHORIZED FOR PRODUCTION) | Resultado (MEASURED) |
|---|---|---|---|
| [P4](I-62-prep/night-2026-10-05/p4-dossier.md): `VerificationTargetSha` / `CustodyHeadSha` | §51.1: custodia `7bd5a743` sobre el GREEN `4f45f446` (CI 37085650331 medida con `gh`); §61.4: cierre `3c5019bf` sobre el Candidato `55a66b3c` | `p4/p4-custody-harness.py`: repositorios Git desechables, regla literal de 16.1 y §8.5 frente a la propuesta | 20 escenarios sintéticos y 2 reales, todos con el resultado esperado; 10 contraejemplos en los que 16.1 admite y la propuesta bloquea; 12 reglas ejercitadas; dos corridas con el mismo SHA-256 |
| [P8](I-62-prep/night-2026-10-05/p8-dossier.md): granularidad de los reintentos | §27, §35 (Q-G2-03), §39 (Q-G3-04): `attempts` 2/3 tras G1 | `p8/p8-counters-harness.py`: contadores global, por gate y por clase, con linaje del defecto | 10 escenarios × 3 validadores (literal y dos perfiles de prueba), todos PASS; 7 reglas ejercitadas; ningún tope elegido |

**Hallazgo de P4.** 16.9 #5 exige HEAD = `CurrentSha`, y 16.1 admite commits de la sesión por prefijo del diff final. En I-63, la reverificación de la
misma entrega con custodia encima obligó al Controller a improvisar una excepción. Además, la regla por prefijo admite:
- un cambio y reversión de producción;
- una autoridad normativa bajo `docs/automation/`;
- un renombre de producción hacia la custodia;
- un enlace simbólico;
- una cabeza que no desciende del SHA verificado.

**Límites.** Son propuestas para una A-2 que nadie ha acordado. No se consumió ni se declaró A-2, y nada se aplicó a V14, al Freeze, a `/v1`, a I-61 ni a
I-63.

## 47. Helpers deterministas P1/P2/P3/P5/P6 (orden nocturna §8; EXPERIMENTAL)

CI de los dossiers P4/P8, `75e93e91c1197dcb7e4095c7d0fb83421cfb1494`: corrida **37275024919**, push, head_sha exacto, cuatro jobs en `success`.

**Código y pruebas.** `I-62-prep/night-2026-10-05/helpers/`: stdlib más git. 18 pruebas OK con `python -X dev`, sin avisos.

**Lecturas reales** (`real-readings.json`):
- **P1:** detecta los cuatro `CurrentSha` mutados de nc1. El de G4 falla aunque el prompt contenga el GREEN.
- **P2:** detecta las cuatro delegaciones mutadas de nc2, y las cuatro reales pasan. El Controller de I-63 no detectó nc2 de G2 ni nc1/nc2 de G4.
- **P6:** el `ui.trx` de READY-05 de I-63 da 1637 superadas y 17 omitidas con su motivo del runner. Coinciden una a una con los `Skip =` del código en
  `55a66b3c`, sin ningún número fijado. Los TRX `core` de READY-05 y los dos focales de F0, estos con su SHA-256 registrado, también dan PASS.
- **No encontrado:** `core.trx` `00779B64…`.

**Límites.**
- Hechos, no veredictos: `assert_no_verdict` rechaza cualquier campo o valor de veredicto.
- P5 solo usa archivos ficticios.
- Ningún comportamiento congelado cambia.
- Los huecos (comodines, mayúsculas, enlaces, `Skip` dinámico) se informan sin interpretarse.

## 48. F4 experimental: secuencias combinadas y A62-A1A-01 (orden nocturna §9 y §3.A)

CI de los helpers, `4eb3ce6b82bd90c776bdd20dd1c2b652e927cba8`: corrida **37275825561**, push, head_sha exacto, cuatro jobs en `success`.

- **Secuencias combinadas** (`I-62-prep/night-2026-10-05/f4-exp/`; EXPERIMENTAL — NOT AUTHORIZED FOR PRODUCTION):
  - cinco combinaciones de la orden §9, con positivas y negativas, sobre el motor del arnés de A-1;
  - expectativas y conjuntos de reglas exactos fijados antes de la primera corrida; una corrección declarada en el README;
  - 16/16 PASS con el arnés corregido.
- **A62-A1A-01** (defecto de A-1 hallado por el autor; MEASURED):
  - **Defecto:** con el arnés del objeto revisado (blob `39c2f831`), un bucle REVIEWER nuevo con otra autoridad llegaba a REVIEWER_SATISFIED con el
    BLOCKING heredado LIN-R1 abierto (`combo-result-before.json`). D1-17 (3) solo contaba los BLOCKING «abiertos en una solicitud del bucle», en contra
    de B.10.2 y de D1-18 (E).
  - **Corrección:** D1-17 (3), su explicación y D1-18 (S) cuentan ahora todo BLOCKING con `issuer` REVIEWER de la unidad.
  - **Arnés:** 96 trazas (33 VALID y 63 INVALID), todas PASS y deterministas. Dos son nuevas: `a62-a1a-01-*`.
  - **Blobs:** A-1 `cdcd98d2752d384272285536b3874ae45c9d9832`; arnés `6b71ace7…`; resultado `75cf92f6…`; disposición `fab9896a…`.
  - **Paquete:** preguntas 16 y 17.
- **Observaciones sin cambio en A-1:**
  - **F4X-OBS-01:** un segundo rebase con un LAUNCHING sin resolver para sin publicar (D2-2).
  - **F4X-OBS-02:** una autorización sustituta con topes menores que lo consumido no se puede registrar (I-S18).
- **Efecto en la revisión:** el kit v3 (R20261005T063359Z-86e3, objeto `39c2f831`) queda sustituido sin haberse lanzado. Hace falta un kit nuevo para
  el objeto corregido.

## 49. Revisión formal 1 de la orden nocturna (R20261005T063359Z-86e3) y ciclo de corrección 1

- **Lanzamiento:**
  - el Owner pulsó `task_7b4dd3ee` (kit v3, objeto `39c2f831`, commit `9dcfc08d`);
  - la sesión revisora `local_5ac1ec2e` (`claude-opus-5-5`, `xhigh`) corrió de 06:40:39Z a 07:02:35Z;
  - es la primera de las dos invocaciones que permite la orden (punto E);
  - la sesión autora no supo del lanzamiento hasta intentar retirar la tarea; la tarea v3b (`task_008b4c96`) se retiró para no gastar la segunda
    invocación antes de ver el resultado.
- **Resultado** ([registro](../../initiatives/I-62-architect-review-A-1-r3.md); `output.json` literal):
  - CHANGES REQUIRED: A62-A1S-01 (BLOCKING heredado o de una autoridad sustituida; pertenencia por la autoridad vigente; STILL_OPEN) y A62-A1S-02
    (D1-10 quitaba al REVIEWER CORRECTING → PUBLISHED), con A62-A1S-O1..O4;
  - A62-A1-01..06 y A62-A1R-01..03 CLOSED; OBS-A1-01 STILL_OPEN; sin decisión del Owner.
- **Acreditación (MEASURED):**
  - **auditor v3:** NOT_ACCREDITED con 4 motivos, un único defecto propio: la palabra `requests`, clave del modelo del arnés, en un heredoc de solo
    lectura;
  - **corrección de método v3.1:** en archivo aparte, autoprueba PASS; ACCREDITED, 0 motivos;
  - la acreditación la decide el Coordinator.
- **Ciclo de corrección 1** (punto F):
  - **A-1 blob `03dd822d1310a0ce298eba6da2321383aa5e4887`:**
    - D1-17 con pertenencia por apertura y STILL_OPEN;
    - D1-10 REVIEWER con CORRECTING → PUBLISHED;
    - D1-10 EXECUTION y §1 con los dos deltas de EXECUTION;
    - D1-20 frente a las autoridades SUPERSEDED;
    - recuentos al día; C-38 ampliado.
  - **Arnés `6b051db7…`:** 100 trazas (34 VALID y 66 INVALID), todas PASS. Incluye la pertenencia por apertura en A1-R02/A1-R03, la regla nueva A1-P18
    y cuatro trazas nuevas `a62-a1s-*`. La traza de sustitución da VALID con la pertenencia antigua, así que discrimina.
  - **Resultado `71c46ef9…`; secuencias combinadas:** 16/16 PASS.
  - **Paquete `a97dea77…`:** pregunta 18 y textos al día (O4).
  - **Disposición `eeb29ba0…`.**

## 50. Kit v3c de la revisión formal 2 (última de la orden nocturna)

CI de la custodia de la revisión 1 y del ciclo 1, `411e01ce4176e4fe6648ea3e9ff2febc219ff438`: corrida **37278429319**, push, head_sha exacto, cuatro jobs en
`success`.

- **Kit:** `docs/automation/evidence/I-62-architect-A-1/R20261005T073911Z-2dfe/`. Objeto: `411e01ce`, A-1 `03dd822d`.
  - auditor v3.1 declarado antes de la corrida;
  - coherencia para 12 disposiciones, 14 opcionales y focos 1-19;
  - prompt SHA-256 `302898f8…`;
  - autoprueba PASS; preflight 3/3.
- **Lanzamiento:** HUMAN_LAUNCH_REQUIRED, con la tarea de la app `task_b5714b45` creada una sola vez.
- **Kit v3b** (R20261005T072212Z-e65c, para `cdcd98d2`): retirado sin lanzarse; quedan su prompt, su cierre y su manifiesto.
- **F4 experimental en C#** (N-08):
  - 9/9 en el clon aislado `D:\r62-f4-exp`;
  - diff en `I-62-prep/night-2026-10-05/f4-exp/csharp-experimental.patch`; TRX fuera del repo (SHA-256 `c07b6f52…`).

## 51. Precisión del manifiesto B.11 en clausuras pequeñas (orden nocturna §10; EXPERIMENTAL)

CI del kit v3c, `e77c51bd45dd8fcbdd95e13ee8eb79e04579e056`: ver la cola (N-13).

`I-62-prep/night-2026-10-05/b11-precision/` trabaja sobre una copia del manifiesto F3, regenerado con `gen-manifest.py` y con un informe semánticamente igual
al custodiado. Clasifica seis aristas reales de V14: dos de control, dos informativas, una ambigua y una de control que falta.

- **B11-P1 (exceso):** los punteros y las referencias de prueba son aristas de control. `B.3#p1` queda UNKNOWN por una arista informativa hacia C-08.
- **B11-P2 (omisión peligrosa):**
  - `§8.5#p1` («`Identity` … no cambia») se acredita con clausura {sí misma}, porque nombra la comprobación 16.9 #5 sin referencia de sección;
  - el manifiesto no tiene ninguna unidad de 16.9;
  - 66 líneas de V14, en 25 secciones, nombran una de sus 14 comprobaciones (candidatas, aproximado).
- **Ambigüedad legítima:** «16.1» ya falla cerrado.
- **Límites:** `Complete` nunca pasa a true. No se cambian el manifiesto ni el generador; la decisión es de la revisión B.11 del Architect.

## 52. Portabilidad: reconstrucción en un clon limpio (orden nocturna §10; EXPERIMENTAL)

CI de la precisión de B.11, `c891863d…`, y de las correcciones `01ade261…` y `aeda77c4…`: ver la cola. El kit v3c (`e77c51bd`) tiene la corrida
**37279114391**, push, head_sha exacto, cuatro jobs en `success`.

`portability/reconstruct.py` corre en un clon limpio (`--no-local`, sin conversión de fin de línea, sin remoto) de `aeda77c4`, como proceso distinto y
solo desde artefactos custodiados.
- **Resultado:** 7/7 pasos reconstruidos. El arnés de A-1, las secuencias combinadas, P4, P8 y las lecturas P1/P2 salen idénticos byte a byte; el
  unittest de los helpers da PASS; B.11 es idéntico tras el mapeo declarado.
- **PORT-01:** el generador F3 fija el commit histórico `4c617e82`, inalcanzable en un clon limpio. Se resuelve por el `rebase-map.json` custodiado
  (imagen `1d5cdbec`, mismo blob).
- **PORT-02:** las pruebas de los helpers dependían del cwd o de `I62_REPO`. Corregido.
- **PORT-03:** un defecto del propio script en su primera corrida. Corregido y declarado.
- **No es un piloto FX.**

## 53. Cierre de la orden nocturna

- **Revisión 2** (kit v3c, `task_b5714b45`):
  - la lanzó el Owner (sesión `local_8e95d755`, worktree `sad-cray-fd84e8` sobre `411e01ce`);
  - su primera llamada, `sha256sum` del prompt en `D:
62-arch-a1r5-run` (fuera de su worktree), no tiene resultado desde las 08:08:10Z: la sesión
    espera una aprobación de permiso;
  - no hay veredicto; la invocación está lanzada, así que cuenta como la segunda y última.
- **Interrupción de la sesión principal:** el registro de CI muestra un hueco entre los commits `c891863d` (07:46Z) y `01ade261` (12:33Z), consistente
  con una espera de aprobación de herramienta. La sesión no lo vio mientras ocurría: sus lecturas de hora intermedias se tomaron antes del hueco.
- **CI:**
  - `c891863d` → 37279632155;
  - `01ade261` → 37310363590;
  - las dos con push, head_sha exacto y cuatro jobs en `success`.
  - `aeda77c4` (37314793355) y `c66137b9` (37315042133) seguían en curso al escribir.
- **Informe final:** `I-62-prep/night-2026-10-05/final-report.md`.

## 54. Revisión formal 2 (R20261005T073911Z-2dfe): resultado y custodia (después del cierre de la orden nocturna)

- **Lanzamiento y corrida:**
  - el Owner pulsó `task_b5714b45`;
  - la sesión `local_8e95d755` (`claude-opus-5-5`, `xhigh`) se creó a las 08:08:03Z;
  - su primera llamada esperó una aprobación de permiso hasta que el Owner la dio, y terminó a las 13:54:38Z, dentro de la ventana de la orden.
- **Resultado** ([registro](../../initiatives/I-62-architect-review-A-1-r4.md); `output.json` literal):
  - CHANGES REQUIRED: A62-A1T-01 (D2-2: el `Target` de un intento en LAUNCHING debe resolverse por la cadena de mapas; con un segundo rebase la unidad
    queda sin transición válida) y A62-A1T-O1..O3;
  - los doce hallazgos anteriores, CLOSED; sin decisión del Owner.
- **Acreditación (MEASURED):**
  - **auditor v3.1, literal:** NOT_ACCREDITED con 6 motivos (`audit-classification.json`):
    - 3 desviaciones literales declaradas: dos `cd` a subdirectorios del clon y un `sed -i` sobre una salida propia;
    - 3 defectos del auditor: el bucle `for` mal analizado, dos veces, y la ruta con `..` no normalizada, que Git rechazó sin leer nada;
  - **sin corrección de método retroactiva:** las desviaciones literales seguirían;
  - lo demás, en verde: identidad, orden, 32 lecturas fieles, 21 premisas, clon limpio y esquema;
  - la acreditación la decide el Coordinator.
- **Límites:**
  - la orden nocturna terminó a las 13:58Z (ocho horas); la custodia se hizo a las 14:1xZ, como preservación del resultado de una invocación de la orden;
  - **A-1 no se corrige** (N-14, BLOCKED_AUTHORITY);
  - no queda ninguna invocación del Architect bajo la orden nocturna.

## 55. Corrección de A62-A1T-01 y O1..O3 (orden nueva del Coordinator, decisiones §42)

- **Estado de partida (MEASURED, 14:44Z):** la punta `b76a6dab` = remoto; `origin/main` = `bb0d5522`, sin avance; sin rebase. DC-07: I-52
  `fb6b5648` (merge-base `95690c28`) e I-64 `39b45f36` (merge-base `819955d6`) no tocan `I-62*`, AUTOMATION_PLAN, LIFECYCLE ni `agent-execution/`;
  I-63 integrada; ninguna ventana activa.
- **CI de la custodia de la revisión formal 2 (orden §11):** CI 37323117283 → `b76a6dabb35acf09436f16d8ac7dacc3a9179f7f` → push → cuatro jobs
  requeridos en `success` (comprobado con `gh run view`).
- **A-1 corregida:** `docs/initiatives/I-62-A-1.md`, blob `c01899a72b940503bb85a0fab42bc085c603fd0f` (antes `03dd822d`), PROPUESTA. El delta está
  en su §10, «Frente a `03dd822d`»:
  - **D2-2 (cont.):** el `Target` de un intento en LAUNCHING resuelve por ResolveBranchRef con la historia completa de n, incluido el mapa nuevo, y la
    punta rebasada como HEAD;
  - **fallo cerrado:** sin un eslabón o con otro blob, UNRESOLVED y STOP;
  - **sin efecto sobre el intento:** la resolución no cambia invocación, `Target`, estado, `RunId`, `reserved_at`, `BudgetSnapshot`, contadores, fase
    ni linajes; D2-6 sin cambio;
  - D2-3, D2-8, D2-9 y D2-11 alineados; C-15 (d)(e)(d2)(e2)(i), C-29 (i) y C-38 (cuarta revisión);
  - **O1:** D1-17; **O2:** §1, §5, §7, §8, §9, §11, el paquete, la descripción de A1-P02 y la etiqueta «Asserted»; **O3:** C-38, C-15 (i) y A1-R05.
  - Ninguna línea llega a 2000 caracteres: D1-17 y D2-2 se parten en dos filas.
- **RED → GREEN de A1-P08 (T2 capturado antes de corregir):**
  - el arnés RED es el final con la condición de A1-P08 de `03dd822d`, que exigía el `Target` en el mapa en curso o como ancestro de `main_before`
    (`I-62-A1/a1t01/green.diff`);
  - **RED** (`a1t01/red-result.json`): 106/118. Fallan exactamente las 12 trazas que cruzan el segundo rebase con un LAUNCHING pendiente (T2, T5a..g,
    T6, T7, T7a y T7b), y cada una solo por un A1-P08 de más;
  - **GREEN** (`a1t01/green-result.json` = `a1-counterexamples-result.json`, blob `ac77596e`; arnés `a1f4b07f`): 118/118 PASS, 39 VALID y 79 INVALID,
    sin cobertura faltante. Determinista;
  - las 100 trazas anteriores conservan su esperado.
- **Negativos, cada uno con su regla y como mutación única de un positivo:**

  | Traza | Mutación | Reglas exactas |
  |---|---|---|
  | T3a `cadena-sin-el-eslabon-de-X` | M1 sin la entrada de X | A1-P08 |
  | T3b `M1-ausente-de-la-historia` | `rebase_history` sin M1 | A1-P08, A1-P11, A1-F10 |
  | T4 `cadena-completa-con-otro-contenido` | imagen con otro blob | A1-P02, A1-P08 |
  | T5a | invocación reescrita | A1-P09 |
  | T5b, c, e, f, g | `RunId`, reserva, estado, linajes y `BudgetSnapshot` | A1-P10 |
  | T5d | presupuesto cambiado | A1-F01, A1-P10 |
  | T7a | salida de LAUNCHING sin prueba de no arranque | V14-S18-no-arranque (V14 B.8.8 literal) |
  | T7b | relanzamiento del LAUNCH_UNCERTAIN | A1-P09 |
  | O3 | reapertura con una autoridad SUPERSEDED | A1-R05 |

  Positivos: T1 (un rebase), T2 (dos rebases), T6 (no arrancó → BUDGET_RESERVED sobre X'' con la misma reserva), T7 (desconocido → LAUNCH_UNCERTAIN) y
  O1 (la primera solicitud, abierta en el par de apertura).
- **Cambios del arnés, además de A1-P08:**
  - A1-P10 incluye `run_id` y `BudgetSnapshot`;
  - comprobación literal de V14 B.8.8 sobre `not_started_evidence`; las dos trazas anteriores que salen de LAUNCHING ganan su prueba (`NS-0`);
  - A1-R05 también frente a las autoridades SUPERSEDED;
  - A1-P02, en el rebase, y A1-P08, en el objeto de la solicitud, comprueban también el contenido.
- **T8 con Git real** (`a1t01/t8-clean-clone.py`, `a1t01/t8-result.json`; repositorio desechable en `D:\r62-a1t01-t8`):
  - dos rebases reales con los mapas custodiados como `StateRef`;
  - la regla de `03dd822d` daría STOP en el segundo rebase;
  - con la historia candidata [M1, M2], X → X' → X'' se resuelve **antes** de crear n2, sobre la punta rebasada; M2 no contiene X;
  - en un clon `--no-local --single-branch`, X y X' son inalcanzables. El sucesor lee el estado y los mapas por `{path, blob}`, recalcula el PatchId de
    la imagen y resuelve X → X''. Sin M1, sin el eslabón de X o con otro blob: UNRESOLVED;
  - los SHAs cambian en cada corrida; se repiten las relaciones.
- **F4 experimental** (`combo-result-a1t01.json`, 17/17, arnés `a1f4b07f`):
  - `c3-obs-…-reconcilia-por-la-cadena` pasa a VALID (antes `…-para-sin-publicar`, INVALID por A1-P08);
  - nuevo `c3-obs-n-sin-M1-no-resuelve` → A1-P08;
  - `c3n-segundo-rebase-reescribe-el-LAUNCHING` pasa a {A1-P09}: la reescritura sigue fallando, y A1-P08 ya no aplica porque el `Target` resuelve;
  - `combo-result.json` y `combo-result-before.json` no se reescriben.
- **Portabilidad:** `portability/reconstruct.py` compara el paso 2 con `combo-result-a1t01.json` y añade T8 (paso 2b). Su corrida en un clon limpio
  del commit de esta corrección se custodia aparte.
- **Validaciones:** el arnés, las secuencias combinadas y T8 están en verde; la suite Core se ejecuta antes del commit (el cuerpo del commit da el
  resultado). No hay exención nueva de `dotnet test`.
- **Límites:** sin tocar V14, el Freeze, `main`, ROADMAP, HANDOFF, FOUNDATIONS, el índice ADR ni superficies normativas de F4; sin P4/P8 de I-63; sin
  A-2; sin AGREED, GATE PASS ni F4.

## 56. CI de la corrección, reconstrucción por un sucesor y kit v4 (orden nueva, decisiones §42)

- **CI de la corrección** `ca09ade8bb31b1ecb57b2b0d6220628c8434e78d` (A-1 blob `c01899a7`): corrida **37329298556**, push, head_sha exacto,
  cuatro jobs requeridos en `success`.
- **Reconstrucción por un sucesor (T8, orden §5)** (`portability/portability-result-a1t01.json`):
  - `reconstruct.py` corre en un clon `--no-local --single-branch -c core.autocrlf=false` de `ca09ade8`, sin remoto, donde `4c617e82` es
    inalcanzable;
  - 8/8 pasos: el arnés de A-1 y `combo-result-a1t01.json` salen idénticos byte a byte; T8 da PASS por relaciones; P4, P8 y las lecturas P1/P2,
    idénticos; el unittest de los helpers, PASS; B.11, idéntico tras el mapeo declarado (PORT-01).
- **Kit v4** (`I-62-architect-A-1/R20261005T151303Z-c64c/`, custodiado antes del lanzamiento):
  - clon `D:\r62-arch-a1t01` con `main` = `ca09ade8`, sin remoto y sin enlaces;
  - `order.txt` = la orden exacta (13 157 bytes, SHA-256 `588d6289…`);
  - contrato de acciones de la orden §7 y auditor v4;
  - selftest 11/11 PASS (los casos de la orden §7, más el escape por unión y los metadatos), con el clon limpio al terminar;
  - fidelidad previa 3/3.
- **Transporte:** la única vía limpia es una tarea de la app que lanza el Owner con un clic: HUMAN_LAUNCH_REQUIRED. La sesión prepara la acción
  una vez; no afirma que la revisión arrancó. No hay reintento automático.

## 57. Revisión formal R20261005T151303Z-c64c: resultado y custodia (orden nueva, decisiones §42, puntos E y F)

- **CI de la custodia del kit** `6a87e525860a273e2b51be8b72b2086eff8fd880`: corrida **37332220109**, push, head_sha exacto, cuatro jobs
  requeridos en `success`.
- **Lanzamiento:** la sesión autora creó una vez la tarea `task_44fd8237` (HUMAN_LAUNCH_REQUIRED), y el Owner la pulsó. La sesión
  `local_30428fbf` (`claude-opus-5-5`, `xhigh`, Claude Code 2.1.286) corrió de 15:47:29Z a 16:04:38Z en el worktree `goofy-grothendieck-4ead60`, sobre
  `main` = `ca09ade8`. No hubo reintento ni otra invocación.
- **Resultado** ([registro r5](../../initiatives/I-62-architect-review-A-1-r5.md); `output.json` literal, SHA-256 `67d2cc50…`):
  - **AGREED** sobre `ca09ade8` / A-1 `c01899a7`: cero REQUIRED;
  - A62-A1T-01 y los doce cierres, CLOSED; A62-A1T-O1..O3, CONFIRMED;
  - A62-A1U-O1, OPTIONAL: comprobar mecánicamente que un mapa no tiene una entrada compuesta;
  - sin decisión del Owner; F3 compatible; materialidad M-02..M-05 = sí.
- **Acreditación (MEASURED):**
  - **auditor v4, literal: ACCREDITED, 0 motivos** en 62 llamadas (`audit.json`, SHA-256 `57a06f9d…`);
  - el auditor no cambió después de la corrida: los 11 archivos del run coinciden con `kit-manifest.json`;
  - identidad y orden verificados antes de leer; 28 registros de fidelidad FAITHFUL_NORMALIZED; 16 premisas encontradas y entregadas fielmente;
    resultado válido contra el esquema y coherente; clon y worktree limpios en `ca09ade8`; sin cadenas laterales;
  - declaraciones del revisor, registradas sin reclasificar: `git diff` (MD-1) por tubería a `grep`, `sed -n` y `cut` (el auditor no lo cuenta);
    `combo_sequences.py` reejecutado con `runpy` (medido: sin procesos); T8 no reejecutado;
  - la acreditación la decide el Coordinator.
- **Siguiente (orden §12):** AGREED con la corrida acreditada por el auditor → A-1 (`ca09ade8`, blob `c01899a7`) se entrega para el acuerdo del
  Coordinator. Sin F4, sin A-2 y sin otra ronda. A62-A1U-O1 queda para la decisión del Coordinator: obligación de F4 o incorporación con un blob nuevo.

## 58. A-1 AGREED (decisiones §43): custodia y apertura de F4

- **CI de la custodia de la revisión** `847cdb5f2c732459035454c6a722955ba2d335c8`: corrida **37347313693**, push, head_sha exacto, cuatro jobs
  requeridos en `success`.
- **Custodia del acuerdo (orden §21):**
  - corrida del Architect R20261005T151303Z-c64c: veredicto **AGREED**, acreditación **ACCREDITED** (Coordinator);
  - veredicto del Coordinator: **AGREED**;
  - A-1 acordada: `ca09ade8bb31b1ecb57b2b0d6220628c8434e78d`, `docs/initiatives/I-62-A-1.md`, blob `c01899a72b940503bb85a0fab42bc085c603fd0f`. El
    archivo no se edita; el acuerdo se registra en decisiones §43, en la disposición (§6), en el estado y en el contrato.
- **A62-A1U-O1:** obligación de F4 (negativo mecánico de la entrada compuesta en C-15/C-38 y en el validador), no material, sin A-n.
- **F4:** implementación de producción autorizada desde la preparación (`I-62-prep/f4-dossier.md` y `I-62-prep/f4/`); la evidencia de F4 vive en
  `docs/automation/evidence/I-62-F4/`.
- **DC-07 (18:47Z):** `origin/main` = `bb0d5522`; I-52 `fb6b5648` e I-64 `39b45f36` sin cambios; ninguna ventana activa.

## 59. F4-A..F4-H materializados: paquete del gate de F4 (decisiones §43)

- **Implementación:** `6f0187cb` (árbol limpio). Superficies, RED/GREEN por corte (F4-A..F4-H6), regresión de A-1, matriz C-15..C-42 y observaciones
  F4-OBS-01..24 en [I-62-F4/README.md](I-62-F4/README.md).
- **Full sobre `6f0187cb` (AGENTS):** Core **12962/12962**; UI **1637 correctas y 17 omitidas de 1654**. Lectura P6 de los TRX reales en
  [I-62-F4/p6/p6-result.json](I-62-F4/p6/p6-result.json) (SHA-256 de cada TRX, sin copiar sus bytes).
- **CI del push de `6f0187cb`:** corrida **37391012002**, `event` = push, `ref` = `refs/heads/architecture/portabilidad-coordinador-principal`,
  `head_sha` exacto, cuatro jobs requeridos en `success`.
- **C-20b y C-20c sobre `6f0187cb`:** mapa = derivación (EQUAL, 31 archivos, 70 entradas); C-20c 32/32 con el mapa VALID (MV-1..MV-7).
- **A-1:** acordada sin cambios (`ca09ade8`, blob `c01899a7`). A62-A1U-O1 cubierta (negativo de la entrada compuesta fabricada en el validador,
  en `RebaseChain.Creditable` y en C-15/C-38).
- **DC-07 (23:41Z y antes de esta custodia):** `origin/main` = `bb0d5522`; I-52 `fb6b5648` e I-64 `39b45f36` sin cambios; los hermanos no
  tocan WORKFLOW, AUTOMATION_PLAN ni `agent-execution/`.
- **Siguiente:** revisión del Coordinator. F4 GATE PASS no se autodeclara; quedan para su decisión la tabla E.5/derivado de C-20b, C-21 y las
  observaciones F4-OBS-20..24.

## 60. F4 GATE PASS del Coordinator (decisiones §44): custodia

- **Disposición:** F4 = GATE PASS / COMPLETE sobre `6f0187cb` (evidencia `a8c6af16`). C-20b PASS con la tabla E.5/derivado
  clasificada como divergencias de previsión NO MATERIALES (sin A-n); C-21 PASS; F4-OBS-20..24 ACCEPTED (NO MATERIALES); sin hallazgo MATERIAL
  nuevo ni decisión del Owner.
- **CI de la evidencia** `a8c6af16`: corrida **37391578392**, push, `head_sha` exacto, cuatro jobs requeridos en `success` (comprobada).
- **No se hace:** cambiar la implementación para igualar las filas de E.5; reclamar las partes F6 de C-32, C-37 y C-42; decidir OD-2, OD-3, OD-4,
  OD-5 u OD-7.
- **Siguiente frontera: F6.** Antes de cualquier escenario real (FX-01..FX-06), evaluar sus prerrequisitos de decisión del Owner tal como están
  congelados (Proposal V14 §17, fila F6, y §18). La preparación de `I-62-prep/f6/` puede seguir. I-61 sigue activa; las superficies I62 siguen
  inactivas hasta `I62_EFFECTIVE_SHA`.
- **DC-07 (2026-10-06T01:59Z):** `origin/main` = `bb0d5522`; I-52 `fb6b5648` e I-64 `39b45f36` sin cambios.
- **Custodia:** un único commit de registro con su CI; sin cadena de acuses posterior.

## 61. Preflight de las decisiones del Owner de F6 (decisiones §45)

- **Medición pasiva** (2026-10-06T02:26:14Z; [od-passive.json](I-62-prep/f6/od-preflight/od-passive.json), script
  [od-passive.ps1](I-62-prep/f6/od-preflight/od-passive.ps1)): `config.toml` SHA-256 `091540ED…` (mismos bytes que el 2026-10-04, escrito
  2026-10-04T21:39:14Z); app Codex `26.930.3930.0`; binarios `codex.exe` `37762753…` y `081E4DE4…` sin ejecutar; `codex` y `claude` fuera del `PATH`;
  `claude.exe` 2.1.270.0 (`FD7F35EC…`), autenticación UNKNOWN sin invocar; `gh` como `marioap-afk` con alcances `repo` y `workflow`; RackCad público;
  sin `marioap-afk/rackcad-i62-fixture` ni `D:\r62-fixture`. `MC_I62` = `6f0187cb`.
- **Hecho previo que cambia dos paquetes:** la sonda P2 de I-61 (`workspace-write`, 2026-09-30) tuvo 4 rechazos del sandbox al crear procesos y la
  CLI añadió una entrada de confianza a `config.toml` (DEV-G1C-01).
- **Paquetes vigentes** en [owner-decision-packets.md](I-62-prep/owner-decision-packets.md), con la revisión del 2026-10-04 como historial:
  - OD-5: **aprobar** (precondición de todo F6; FX-01, FX-04a y FX-05 con ella sola; sin credenciales ni infraestructura);
  - OD-7: **aprobar, privado** (sin CI F6 no cierra; los alcances ya existen; privado por la evidencia del host);
  - OD-2: **aprobar con la línea base actual** `091540ED…` (antes B: lleva ≈ 29 h estable y P-01 cubre un cambio posterior);
  - OD-4: **aprobar solo en el fixture** (convierte FX-04b en un resultado medido, PASS o UNSUPPORTED con causa; sin sandbox elevado ni permisos);
  - OD-3: **rechazar por ahora** (FX-03 necesita además la escritura de Codex, que la medición de I-61 hace improbable; FX-06 no lo necesita con
    OD-2; revisión prevista si la sonda de OD-4 demuestra la escritura).
- **Matriz de la orden = Freeze**; ningún escenario real empieza; el siguiente paso son las decisiones del Owner.

## 62. F6 nocturno, bloque 1: decisiones del Owner, fixture, OD-4, FX-05 y kits (decisiones §46)

- **Decisiones del Owner custodiadas** (decisiones §46): OD-5, OD-7, OD-2 y OD-4 = A; OD-3 = RECHAZAR.
- **Preflight** (07:14:30Z): punta `b2f453cd` = remoto; `origin/main` = `bb0d5522`; I-52 `fb6b5648` e I-64 `39b45f36` sin cambios; sin decisiones
  posteriores; fixture inexistente.
- **F6-A, fixture D.1 pasos 1-4** ([I-62-F6](I-62-F6/README.md)): F_seed `930288c5` (37 autoridades de `MC_I62` byte a byte), F_norm `a9f6c929`,
  F_eff `fbe25347` (TEST-ACTIVATION), reclamo de FX-U1 `eea0114a`, orden FX-U1-O1 `54f2a4a8`; remoto privado `marioap-afk/rackcad-i62-fixture`.
- **CI del fixture: `Ci` = `not_run`.** GitHub Actions no crea corridas en el repositorio privado (3 pushes, sin check suite de Actions; Actions
  habilitado; flujo activo; GitHub operativo). Frontera humana: el Owner revisa Actions y la facturación de la cuenta. Sin bypass.
- **OD-4, sonda 1 de ≤ 2** (`R20261006T072306Z-od41`): archivos y herramientas SUPPORTED; commit UNSUPPORTED (`.git/index.lock` denegado por el
  sandbox); escritura fuera del espacio bloqueada; `codex-cli 0.160.0`, `gpt-6-luna`/`high` observados. **La sonda reescribió `config.toml`**
  (`091540ED…` → `9002E854…`, +1 sección `[projects.<redactado>]` con una clave): **P-01, STOP de `codex-cli`**, sin aceptar la huella nueva.
- **FX-05 / C-27: PASS** (corrida 2; la 1 se conserva como inválida por un defecto del detector): rechazo P-16 de un artefacto que nombra I-62 y del
  remoto de RackCad configurado; 0 diferencias en el estado real.
- **Clasificaciones:** FX-04b UNSUPPORTED medido (Worker Codex sin commit; sin otro adapter con escritura acreditada lanzable por B); FX-03 UNVERIFIED
  (OD-3) y, con OD-3, seguiría limitado por el mismo commit; FX-01, FX-04a HUMAN_LAUNCH_REQUIRED; FX-02 y FX-06 UNVERIFIED (P-01, sin sesión A,
  sin CI).
- **Kits** ([I-62-F6/kits](I-62-F6/kits/README.md)): tarjeta del Owner (CI, OD-2b, apertura de A con la secuencia de effort de FX-01), herramienta del
  oráculo y la comparación de FX-04a con su esquema de respuesta y la instrucción de B; paquete OD-2b y hecho nuevo de OD-3 en los paquetes.

## 63. F6 nocturno, bloque 2: kits de FX-06 y de la supervisión, matriz de obligaciones de F6, preparación de F7 y de OD-1

- **CI de RackCad del bloque 1** `b3e83419`: corrida 37431108479, push, `head_sha` exacto, cuatro jobs en `success`.
- **CI del fixture:** sin corridas hasta las 07:41Z (espera acotada de 10 min): `Ci` = `not_run` se mantiene.
- **FX-06** ([kit](I-62-F6/kits/FX-06/README.md)): oráculo fijado por hash (`43b9a238…`) fuera de todo clon; objeto X v1 con un defecto sembrado;
  borrador de la `ReviewLoopAuthorization`; directorios fijados. **Hecho de elegibilidad:** ARCHITECTURE_REVIEW (Deep) exige Equilibrado o Frontera;
  la única celda Codex posible es `gpt-6.1-sol`/`high`, sin invocación medida: su sonda de OD-2b es su medición (routing §5).
- **Matriz de F6** ([README](I-62-F6/README.md)): C-27 PASS; C-25b UNSUPPORTED medido; C-22..C-24, C-25a, C-26, C-39 y las partes F6 de C-32, C-37
  y C-42, UNVERIFIED con causa exacta. FX-03 preparado y limitado por OD-3.
- **F7:** el borrador factual de FOUNDATIONS sustituye los «previsto» de F3/F4 por hechos y añade las limitaciones medidas en F6
  ([f7-ready-closure.md](I-62-prep/f7-ready-closure.md) §1.1). **OD-1** preparado sin pedirlo: ADR-0048 tal cual; aviso de que editarlo invalida `MC_I62`.

## 64. Disposición del Coordinator tras la primera ejecución de F6 (decisiones §47): OD-2b-PROBE, OD-2c y diagnóstico de Actions

- **Custodia** (decisiones §47): FX-05 y C-27 PASS por el Coordinator; A2 no adoptada; OD-2b-PROBE = A del Owner; ADR-0048 sin cambios antes de los
  pilotos de F6.
- **Preflight** (15:03:52Z): punta `880e6814` = remoto; `origin/main` = `bb0d5522`; I-52 `fb6b5648` e I-64 `39b45f36` sin cambios; huella de
  `config.toml` = `9002E854…` (la de la sonda de OD-4, no aceptada), sin cambios desde 07:23:07Z; binario `8aaf1547b825b104` (`37762753…`) sin cambios;
  clon `A` en `54f2a4a8`, limpio.
- **OD-2b-PROBE** ([`R20261006T150704Z-od2b`](I-62-F6/OD-2b-PROBE/R20261006T150704Z-od2b/result.json)): `codex --version` = `codex-cli 0.160.0` y
  `codex login status` = ChatGPT, sin cambio de huella. Sonda 1 en `D:\r62-fixture\A`: `gpt-6-luna`/`high`, `read-only`, 8 pasos de lectura OK,
  69 231 tokens de entrada (48 384 en caché) y 1 528 de salida. Sonda 2 en `D:\r62-fixture\arch` (clon limpio nuevo en F_eff): `gpt-6.1-sol`/`high`,
  `read-only`, 6 pasos OK, 121 745 de entrada (102 656 en caché) y 1 090 de salida; es la primera invocación medida de la celda del Architect. **Huella
  `9002E854…` antes y después de cada sonda**, deltas saneados vacíos; HEAD y árboles de `A` y `arch` sin cambios (`status --ignored` vacío).
- **Hecho nuevo medido:** `read-only` no crea entradas de proyecto (2 de 2); `workspace-write` sí (2 de 2: DEV-G1C-01 de I-61 y la sonda de OD-4). La
  predicción de +2 secciones del paquete OD-2b era falsa. Para la receta 16.4 (`read-only`: Controller y Architects de FX-02 y FX-06), la huella final es
  estable salvo un reinicio o una actualización de la app de Codex.
- **Prueba exacta sin valores** (`cfg_projects.py`): el `config.toml` final, sin la sección `[projects.'d:\r62-fixture\probe-od4-1']` (clave
  `trust_level`) y su línea en blanco, tiene exactamente el SHA-256 de la línea base aceptada `091540ED…`: nada más cambió.
- **OD-2c preparado** ([paquetes](I-62-prep/owner-decision-packets.md) §OD-2c): hash exacto `9002E854457F2DBE074B66FB804FC24AA9B489C58436753CE74BCBA2BF767B1E`,
  recomendación ACEPTAR. `codex-cli` sigue en STOP P-01 hasta la decisión del Owner.
- **Actions** ([diagnóstico](I-62-F6/CI/actions-diagnostic.json), 15:13:20Z): 0 corridas. Actions habilitado (`allowed_actions` all), flujo `fixture`
  activo y bien formado (`on: push` sin filtros), `PushEvent` de `fx/u1` entregado, check suites solo de la app `claude`. El repositorio **público** de
  la misma cuenta sí ejecuta Actions. Causa probable: la facturación de Actions de la cuenta para repositorios privados; el token no tiene el alcance
  `user` para leerla y la sesión no amplía credenciales. Lista de comprobación del Owner en la [tarjeta](I-62-F6/kits/README.md) §1.
- **Corrección:** se elimina una línea duplicada del paquete OD-2b, resto de la corrupción de rutas del bloque 1.

## 65. Corrección del Owner a OD-7 (decisiones §48): auditoría previa, fixture público y CI todavía sin corridas

- **Custodia** (decisiones §48): OD-7 = A público; regla general (repositorios de CI de I-62 públicos por defecto; uno privado exige autorización
  explícita); OD-2c = A con la línea base exacta `9002E854…`: `codex-cli` sale del STOP P-01, con la huella revalidada antes de cada invocación.
- **Auditoría previa a la publicación** ([public-audit.json](I-62-F6/CI/public-audit.json), 15:48:32Z; detector
  [public_audit.py](I-62-F6/CI/public_audit.py), con los patrones de identidad del Owner fuera de la copia publicada): clon espejo de GitHub, 5 refs, 75
  objetos (6 commits, 1 etiqueta, 19 árboles, 49 blobs, ningún binario). 40 coincidencias, todas en 7 blobs idénticos a los de `MC_I62` (`6f0187cb`, ya
  públicos en la rama de I-62 de RackCad): menciones documentales de rutas genéricas y marcadores `AUTHORIZATION: <AuthorizationId>`. Los 11 archivos
  propios del fixture, sin coincidencias. Identidades sintéticas (`fixture <fixture@example.invalid>`). Del lado de GitHub: 0 secretos, variables, claves
  de despliegue, releases, issues, PRs, artefactos, entornos, despliegues, webhooks o cachés; sin Pages ni wiki; sin eventos de borrado. **Veredicto:
  nada no apto; sin STOP.**
- **Visibilidad** ([diagnóstico](I-62-F6/CI/actions-diagnostic-public.json)): `gh repo edit … --visibility public` a las 15:50:43Z; `private` = false;
  las 5 refs y la etiqueta intactas; flujo `fixture` activo; RackCad sigue público y sin cambios. No se recreó nada.
- **Disparo:** commit vacío `a0a3d48379c316e455049b62fe881446bcdc50f5` en `ci/smoke` (padre `7cf79ffc`), empujado a las 15:51:00Z. GitHub lo procesó (check
  suite de la app `claude` a las 15:51:06Z), pero **Actions no creó ninguna corrida** (consultas de 15:51Z a 16:03:50Z; la página pública dice «0 workflow
  runs», sin aviso). GitHub Actions operativo; RackCad, con el mismo `on: push:` vacío, sí corre.
- **Resultado: `Ci` = `not_run` sigue.** La hipótesis de la facturación de repositorios privados queda **refutada**. La causa es desconocida y específica de
  este repositorio. Siguiente paso: inspección del Owner con sesión de administrador, o una acción nueva autorizada sobre el fixture (ver la tarjeta §1).

## 66. Orden de continuación (decisiones §49): R1 sin corridas, R2 con corridas push y manual, y preparación de FX-01

- **R1** ([actions-recovery-r1-r2.json](I-62-F6/CI/actions-recovery-r1-r2.json)): flujo `fixture` (id `376133872`) deshabilitado a las 18:52:01Z
  (`disabled_manually`) y habilitado a las 18:52:04Z (`active`), sin tocar el archivo; commit vacío `4a276c75f98811881d10b831048406016be41a08` en
  `refs/heads/ci/smoke` (padre `a0a3d483`), ref remota verificada a las 18:52:09Z. **0 corridas** hasta las 19:01:28Z: R1 no basta.
- **R2:** commit `7d053246526861247cc117d4e2a95203bbe20eb2` en `ci/smoke` (padre `4a276c75`) que añade solo la línea `  workflow_dispatch:` bajo `on:`,
  tras `  push:`; jobs, permisos, runner, acciones y comandos sin cambios; `main`, `fx/u1` y `fixture/i62-norm` conservan el flujo original. El push
  (19:01:39Z) creó la corrida **`push` 37515854486** (`refs/heads/ci/smoke`, `head_sha` `7d053246`, `fixture-build` y `fixture-tests` en `success`); el
  disparo manual (19:02:07Z) creó la corrida **`workflow_dispatch` 37515901956** (mismo ref y SHA, los dos jobs en `success`).
- **Clasificación de `Ci`: A, provisional.** La ejecución funciona y el disparo `push` volvió con el commit que cambió el archivo del flujo. Falta ver un
  push sin cambio del flujo en una rama con el archivo original: lo probará el siguiente push real a `fx/u1` (el BOOTSTRAP del Principal A). Hipótesis
  compatible, no demostrada: el registro del flujo del primer push al repositorio vacío quedó mal y el cambio del archivo lo renovó. R3 no se usó.
- **Comprobaciones antes de abrir A** (18:54Z): clon `D:\r62-fixture\A` en `54f2a4a8` (`fx/u1`), igual a `origin` y a `github`, `status --ignored` vacío,
  identidad local `fixture <fixture@example.invalid>`; ninguna sesión de la app (activa ni archivada) en `D:\r62-fixture`; ningún directorio de memoria de
  Claude para `D:\r62-fixture`; sin `CLAUDE.md` global; huella de `config.toml` = `9002E854…` (OD-2c). La apertura de A se pidió al Owner con la secuencia
  de effort de FX-01 (tarjeta §3); sin respuestas del oráculo en ningún mensaje.
- **Preparación de B (FX-04a), hecho nuevo para decidir antes de abrir B:** `~/.codex` tiene memorias de Codex (`memories_1.sqlite`) y un `AGENTS.md`
  global vacío; D.6 exige enumerar con su SHA-256 las entradas automáticas de B y comprobar que ninguna lleve hechos de la unidad. Además, abrir B en
  un directorio nuevo con la app de Codex puede añadir una entrada de proyecto a `config.toml` (la app comparte el archivo según D.7: UNVERIFIED en F2),
  lo que sería P-01 para `codex-cli`. Ver la tarjeta §4.

## 67. P-01 por la actualización de la app de Codex: huella `723A6898…` y binario nuevo (`codex-cli` en STOP; OD-2d)

- **Hecho** ([result.json](I-62-F6/OD-2/R20261006T191621Z-p01/result.json)): en la comprobación de cierre de turno (19:16:21Z), sin ninguna invocación de
  Codex de la sesión desde las sondas de OD-2b, la huella era `723A68985165BAE40689172F4E573C47FC45D1BA0D19F9192E3120DDD28B18C8` (escrita a las
  18:58:51Z) en lugar de la línea base de OD-2c `9002E854…`. La app de Codex se actualizó (paquete `26.930.3930.0` → `26.930.7945.0`; binario nuevo
  `bin\5ea220ae823df3d7`, SHA-256 `3b8f6e33…`, escrito a las 18:56:03Z; el binario aceptado `8aaf1547b825b104` desapareció).
- **Delta sin valores:** mismos 107 nombres de clave, 39 secciones y 4 775 bytes; cambian valores. La etiqueta del binario nuevo está en
  `mcp_servers.node_repl.env.CODEX_CLI_PATH`; revertirla sola no reproduce `9002E854…`, así que cambió al menos otro valor (la versión de la app está en
  `BROWSER_USE_CODEX_APP_VERSION` y `NODE_REPL_TRUSTED_SERVICES`). No se puede probar cuál sin el archivo anterior, que nunca se guarda.
- **Disposición:** P-01, STOP de `codex-cli`; no se acepta la huella ni el binario nuevos; paquete **OD-2d** para el Owner. FX-01 y FX-04a no usan
  `codex-cli`. Mitigación: instantánea local con digests HMAC por clave (clave solo en el scratchpad) para localizar cambios futuros por nombre de clave.

## 68. Disposición tras R2 (decisiones §50): medición pasiva para OD-2d y comprobación D.6 de las entradas de Codex

- **Custodia** (decisiones §50): R2 aceptado; mensajes «continúa» de la supervisión a A autorizados para G0, QU y QH; FX-01 ya; OD-2d no aprobada; FX-06
  con la celda del Architect obsoleta.
- **Medición pasiva** ([result.json](I-62-F6/OD-2/R20261006T200612Z-od2d-passive/result.json), 20:06:12Z): `config.toml` = `723A6898…`, estable desde las
  19:18:38Z (comparación HMAC por clave sin cambios); 107 nombres de clave; app `26.930.7945.0`; binario `bin\5ea220ae823df3d7`
  (`3b8f6e33…`), el único. **Sin medir** (exigen ejecutar el binario nuevo y no hay autoridad vigente: OD-2b-PROBE se consumió con el binario anterior):
  versión de la CLI, autenticación, modelo, effort y sandbox observados, y estabilidad en sondas de solo lectura. Se propone OD-2d-PROBE.
- **D.6, entradas automáticas de Codex para B:** `AGENTS.md` global de 0 bytes; memorias vacías (`memories_1.sqlite` sin filas de memoria ni trabajos;
  `memories/` sin archivos); 49 archivos en `skills/` sin identificadores de la unidad; `rules/default.rules` (política de comandos, no contexto del modelo)
  menciona el repositorio real, sin hechos de FX-U1. **Limpias**; se repite con SHA-256 antes de abrir B.
- **A:** no abierta todavía (20:06Z): ninguna sesión en `D:\r62-fixture`; `fx/u1` en `54f2a4a8`.

## 69. FX-01 ejecutado, D.1 completo hasta el contrato de T1, CI A, QH de A con violación de I-S13 (F6-OBS-01) y FX-04a detenido antes de B

- **FX-01** ([result.json](I-62-F6/FX-01/R20261006T203145Z-fx01/result.json)): sesión A `local_6dde7eb6…` (`claude-opus-5-5`, abierta por el Owner en
  `D:\r62-fixture\A`). Cuatro preflights CUSTODY con `get_session` ligado al instante: 1 `xhigh` → MATCH / ELIGIBLE (20:31:45Z); 2 `high` →
  BELOW_REQUIRED / STOP P-09 (20:47:13Z); 3 `medium` → BELOW_REQUIRED / STOP P-09 (22:00:48Z); 4 `xhigh` → MATCH / ELIGIBLE (22:01:34Z). Los cuatro
  conformes al oráculo congelado (casos E1 y E3: agregado, causas y disposición). Sin custodia antes del MATCH final. Contraste de la supervisión con
  `get_session` en 3 de los 4 instantes. **Desviación:** la sesión se abrió en `xhigh`, no en `high` (cuatro preflights en vez de tres; el primer
  BELOW_REQUIRED llegó como bajada tras un MATCH). **C-23: PASS por la evidencia**, con la desviación para la aceptación del Coordinator.
- **D.1 pasos 5-6** ([chain.json](I-62-F6/FX-U1-chain/chain.json)): BOOTSTRAP `1746b404` (válido), G0 `5a3a7d69` (Coordinator del fixture, tres
  marcadores), QU `1a4fc9c6` (válido) y contrato de T1 `d30fb6a9` (`gate-contract/v2` válido, I62, `AuthorityRevision` = BOOTSTRAP, `MainSha` = F_eff).
  **C-22: PASS por la evidencia** (estado de contrato I62 válido de D.1), para la aceptación del Coordinator.
- **CI del fixture: clasificación A.** El push ordinario del BOOTSTRAP a `fx/u1`, sin tocar el flujo, creó la corrida 37538606357 (`fixture-build` y
  `fixture-tests` en `success`); también corrieron y pasaron G0 (37538952116), QU (37543264455), T1 (37543386267) y QH (37545694131).
- **QH** `ae25b596` (orden FX-U1-O2): forma correcta (T1 FIRST, contrato custodiado, Controller sin binding, Principal RELEASED, ventana CLOSED);
  **terminación de A acreditada** por `isRunning` = false desde las 23:17:18Z (dos observaciones de la supervisión, sin actividad tras el QH).
- **F6-OBS-01 (posible defecto material del texto congelado; decide el Coordinator):** con el archivo de decisiones ampliado después de G0 (lo exige 16.28:
  cada decisión en un bloque de `decisions/<unit>.md`), los dos `StateRef` a la decisión G0 no pueden cumplir a la vez la invariante de archivo de B.8.4
  («cada ruta existe con su blob en el árbol del propio punto»; I-S13 del validador de F4) y la lectura literal de 16.28 («`g0_acceptance` cambia una
  sola vez»). A lo detectó y lo preguntó en su sesión; el Owner eligió «mantener `bdc8e4d`». Según el código del validador de producción, esa opción
  **viola I-S13** en el QH (el árbol tiene `7001b419`), mientras que refrescar el blob cumpliría I-S13 e I-P09 (que solo vigila el estado). A atribuyó la
  elección al «Coordinator»; fue el Owner. No ejecutado: el validador de producción contra el árbol real del QH.
- **FX-04a: STOP de la línea antes de abrir B.** El oráculo depende de si el QH es canónico. Además, el kit tiene tres huecos frente a D.3 para un QH sin
  FX-02: binding del Controller sin valor previo (el oráculo asumía uno reutilizable), «STOP vigentes» y precondiciones sin campo en el esquema, y N11 sin
  entrada de `correction_launches`. Antes de abrir B: disposición del Coordinator sobre F6-OBS-01 y sobre el oráculo.
- **Huella:** `723A6898…`, estable (comparación por clave sin cambios a las 23:21Z); `codex-cli` sigue en STOP P-01 (OD-2d). D.6 de Codex, limpio (§68).

## 70. Validador de producción sobre el QH real, orden de reparación FX-U1-O3, oráculo de FX-04a corregido y OD-2d-PROBE (decisiones §51)

- **Validador de producción de F4** (`RackCad.Tests.StateV2Validator` de la DLL compilada, sin cambios de código;
  [validator-qu-qh.json](I-62-F6/FX-U1-chain/validator-qu-qh.json)) sobre el QU `1a4fc9c6` → QH `ae25b596`, con los árboles reales del origen del
  fixture: archivo del QU 0 violaciones; **archivo del QH 4**: I-S13 en `protocol.g0_acceptance.decision` y en `custody.principal.acceptance.decision`
  (`FX-U1.md` con `bdc8e4dd`, que no está en el árbol: allí es `7001b419`) e I-S16 en las mismas dos rutas (los marcadores no se pueden leer en el
  árbol); par 0, historia del QH 0, historia del par 0 y B1 0. La cadena acreditada BOOTSTRAP → QU da 0 en todas las categorías
  ([validator-boot-qu.json](I-62-F6/FX-U1-chain/validator-boot-qu.json)).
- **Inventario de `StateRef` del QH:** solo esos dos están obsoletos; `orchestration.next_action.required_inputs[1]` ya cita `FX-U1.md` con `7001b419`;
  las tres referencias al contrato de T1 (`628d89af`) y `budgets.caps` (`AUTOMATION_PLAN.md`) resuelven en el árbol.
- **Reparación:** orden FX-U1-O3 del Coordinator del fixture (`cc21e1d2`, en `origin` y `github`) con la lectura del §51 y los pasos de R (observación,
  propuesta, designación acotada, QR ORDINARY con los `StateRef` refrescados, QH, terminación). Clon limpio `D:\r62-fixture\R` en `cc21e1d2` con
  identidad sintética; sin memoria de proyecto previa. La sesión R la abre el Owner.
- **Kit de FX-04a corregido** (capa del fixture; sin esquemas I62 nuevos): binding REBIND si el estado no lleva binding del Controller;
  `facts.stops_in_force` (códigos de los STOP de la siguiente acción canónica) y `decision.preconditions` (vocabulario cerrado: designación, terminación
  acreditada del anterior, preflight CUSTODY, `main` sin avance, binding aceptado del Controller), comparados como conjuntos; N11 NOT_APPLICABLE sin
  entrada de `correction_launches`. Autoprueba con estados sintéticos, nunca con el QH `ae25b596`.
- **OD-2d-PROBE** ([result.json](I-62-F6/OD-2/R20261007T013200Z-od2d-probe/result.json)): binario `5ea220ae823df3d7` (`3b8f6e33…`), `codex-cli 0.160.1`,
  `Logged in using ChatGPT`, app `26.930.7945.0`. Sonda 1 en `A`: `gpt-6-luna`/`high`/`read-only` (66 841 / 1 254 tokens); sonda 2 en `arch`:
  `gpt-6.1-sol`/`high`/`read-only` (128 885 / 1 091), sin aviso de límite; la celda del Architect vuelve a quedar medida con el binario nuevo. **Huella
  `723A6898…` en las seis mediciones** y comparación por clave sin cambios: estable. Paquete OD-2d con esa huella y ese binario.

## 71. Reparación del QH de FX-U1 por R (QR r4, QH2 r5), oráculo de FX-04a publicado por su SHA y preparación de B (decisiones §51)

- **R** (`local_b70524e3…`, `claude-opus-5-5`/`xhigh`, abierta por el Owner en `D:\r62-fixture\R`): observación `P20261007T023103Z-01b1`
  (CUSTODY MATCH, ELIGIBLE; `badbf930`) y propuesta `B20261007T023706Z-5fdd` (`9ce50a5e`), válidas; designación acotada del Coordinator del fixture
  `1d2f14bf` (+ fe de erratas `f4f5929f` del commit citado). Mensajes humanos a R: el inicial y un «Continúa»; ninguna pregunta.
- **QR r4** `ead6119f` (T16): todos los `StateRef` a `FX-U1.md` refrescados al blob del árbol (`b7cff537`), G0 sin cambio semántico, T1 y contadores
  iguales. **Validador de producción: archivo 0, par desde el QH r3 0, historia 0, historia del par 0, B1 0.** CI 37567554756 success.
- **QH2 r5** `cabed547` (T17): RELEASED, ventana CLOSED, sin cambios semánticos, todos los `StateRef` en su árbol. **Validador: 0 en todas las
  categorías.** CI 37567674783 success ([chain](I-62-F6/FX-U1-chain/), [validator-qr-qh2.json](I-62-F6/FX-U1-chain/validator-qr-qh2.json)).
- **Terminación de R acreditada** por `isRunning` = false (03:38:55Z y ~03:40Z; última actividad 03:38:41Z, tras el push del QH2).
- **Oráculo de FX-04a desde QH2** (nunca desde `ae25b596`), guardado fuera de todo clon: **SHA-256 canónico
  `7fd1ef395643e985e0dbe98569aa19c6a3f6677168092aac11d854790c317dcb`** (archivo `75d779e4…`). Valores no publicados. N11 = NOT_APPLICABLE (sin
  `correction_launches`). [prelaunch.json](I-62-F6/FX-04a/R20261007T034100Z-fx04a/prelaunch.json).
- **Clon de B:** `D:\r62-fixture\B` en `cabed547`, limpio, solo `origin`, sin sesiones previas. **D.6 (03:40:25Z): limpias** (AGENTS.md global 0 B, memorias
  vacías, skills sin identificadores; `rules/default.rules` nombra el repositorio real sin hechos de FX-U1).
- **Segunda actualización automática de la app de Codex** (`26.1002.6548.0`; binario `979a96ce184041d1`, `97c57e4e…`, escrito a las 02:14:49Z):
  `config.toml` = `9EA26634…` (03:29:05Z), con 9 claves cambiadas, localizadas por la comparación HMAC por clave (todas de `mcp_servers.node_repl` y
  `notify`); estructura igual. **P-01 para el uso futuro de `codex-cli`; la propuesta OD-2d sobre `723A6898…` queda obsoleta.** No afecta a FX-04a.
- **Hecho de elegibilidad para B (antes de abrirla):** el catálogo congelado no tiene ninguna celda de Codex elegible de nivel Frontera (`gpt-6-astra`
  es Frontera pero de créditos; `gpt-6.1-sol`, Equilibrado; `gpt-6-luna`, Eficiente). Una B `codex-desktop-session` no puede cumplir
  `PRINCIPAL_COORDINATION.level` para RESUME_DECISION: su preflight daría BELOW_REQUIRED y FX-04a quedaría UNVERIFIED (D.3 paso 5). D.3 paso 4 admite
  como variante registrada aparte una sesión de Claude nueva. **B no se abre hasta la decisión del Coordinator.**

## 72. FX-04a: lanzamiento de B (variante de Claude, §52), auditoría D.6 y SHA-256 de la respuesta registrado antes de la comparación

- **B** `local_1a28830f…` (`claude-opus-5-5`/`xhigh`, abierta por el Owner en `D:\r62-fixture\B`, 04:52:28Z; `get_session` de la supervisión: mismo modelo
  y effort). Un solo mensaje: el texto inicial compacto de la tarjeta (mismos campos que `B-kickoff.md`, sin valores del oráculo ni mención a él).
- **Antes de B** ([prelaunch-claude-B.json](I-62-F6/FX-04a/R20261007T034100Z-fx04a/prelaunch-claude-B.json)): clon en `cabed547`, limpio; sin sesión previa de
  B; sin `CLAUDE.md` global ni memoria de proyecto; el gancho de Orca devuelve `{}`; plugin `github`; contexto que inyecta la app con cobertura declarada.
- **Preflight de B para RESUME_DECISION:** nivel Frontera, effort Long-horizon (`xhigh`), lectura de hechos remotos e introspección RUNTIME_OBSERVED, todos
  MATCH; ConfigurationStatus MATCH, Disposition ELIGIBLE.
- **Auditoría de la transcripción** ([b-run.json](I-62-F6/FX-04a/R20261007T034100Z-fx04a/b-run.json)): 41 llamadas; la única de sesiones fue
  `get_session("self")`; rutas fuera del clon, solo el scratchpad propio de B; ninguna lectura de transcripciones de A o R, de `~/.claude`, de `~/.codex`,
  de `evidence-out` ni del oráculo. Aislamiento no UNVERIFIED, con cobertura declarada.
- **Respuesta de B** ([B-response.json](I-62-F6/FX-04a/R20261007T034100Z-fx04a/B-response.json)): **SHA-256 canónico
  `e17265eab843aef230cac26b2bad8128bc350a246e67655c0b98f9b15ae280b0`**, registrado en este commit **antes** de la comparación con el oráculo
  (`7fd1ef39…`, publicado en `5121c8e3`). 27 entradas enumeradas (CANONICAL, TECHNICAL, AUTOMATIC). B2 no necesaria (N11 = NOT_APPLICABLE).

## 73. FX-04a: comparación mecánica con el oráculo de QH2 = FAIL (3 diferencias de la capa de comparación); clasificación pendiente del Coordinator

- **Orden:** el SHA-256 de la respuesta de B se hizo durable en `096e6162` (CI 37575739429 success) antes de comparar; después se publica el oráculo
  ([oracle.json](I-62-F6/FX-04a/R20261007T034100Z-fx04a/oracle.json), SHA-256 canónico verificado `7fd1ef39…`) y la comparación
  ([comparison.json](I-62-F6/FX-04a/R20261007T034100Z-fx04a/comparison.json), [análisis](I-62-F6/FX-04a/R20261007T034100Z-fx04a/comparison-analysis.json)).
- **Resultado mecánico: FAIL**, 19 de 22 campos iguales: rama, Claim-Id, QH, `record_version` 5, I62, RELEASED, intentos 0, sin lanzamientos ni
  invocaciones ni cadenas, STOP vigentes {P-01, P-10, P-16}, QR → Q0 → CONTROLLER_PLANNING, ventana 1, T1, intento 0, I62, **REBIND**, sin Worker y las
  cinco precondiciones.
- **Diferencias:** `facts.last_window` (oráculo `[null, null]`, B `[]`: los dos sin última ventana) y `facts.task_intent` (oráculo `["T1","FIRST",0]`, B el
  objeto completo con el mismo contenido) son de **representación**: el esquema de respuesta del kit no fijaba esos formatos de tupla. `decision.role`
  (oráculo `EXECUTION_CONTROLLER`, B `PRINCIPAL_COORDINATOR`) es de **significado**: el esquema no definía el campo; B da el `next_action.role` canónico del
  QH2 (el titular nuevo actúa primero con el QR) y el oráculo, el rol de la delegación que se planificará.
- **Sin cambios a posteriori:** el oráculo y el esquema no se ajustan tras ver la respuesta; el FAIL mecánico se conserva. Preflight de B MATCH/ELIGIBLE y
  aislamiento no UNVERIFIED. **La clasificación de FX-04a y C-25a es del Coordinator.**

## 74. FX-04a: B1 acreditada INVALID_TEST_ORACLE, contrato y oráculo v2 desde QH2, SHA del oráculo v2 publicado antes de B2 (decisiones §53)

- **B1 intacta:** oráculo `7fd1ef39…`, respuesta `e17265ea…`, comparación 19/22 (FAIL bruto); acreditación **INVALID_TEST_ORACLE** (§53). F6-OBS-02 =
  defecto no material del arnés.
- **comparison-contract-v2** ([documento](I-62-F6/kits/FX-04a/comparison-contract-v2.md), [esquema](I-62-F6/kits/FX-04a/response.v2.schema.json)):
  `facts.last_window` = `{"present": false}` o un objeto cerrado; `facts.task_intent` = `{"task_id", "kind", "attempt"}` o `null`; `decision.role`
  sustituido por `next_actor_role` y `planned_delegated_role`; contadores y cadenas como objetos cerrados (sin cambio de valor: `[]` en QH2); igualdad
  exacta de 23 campos, con `stops_in_force` y `preconditions` como conjuntos y `next_points` en orden. Herramienta: `fx04a_real.py oracle2` / `compare2`
  (v1 intacta), con una salvaguarda que se detiene si `next_actor_role` no coincide con el `orchestration.next_action.role` canónico.
- **Oráculo v2** solo desde QH2 `cabed547`, fuera de todo clon: **SHA-256 canónico `5e7a78d311735e4299925a9d97ac4760fe3595f1decb4d97440e17c947b64a03`**
  (archivo `ae384682…`), 23 campos, conforme al esquema v2; ningún valor tomado de B1. Publicado en este commit, antes de B2
  ([oracle-v2-published.json](I-62-F6/FX-04a/R20261007T061300Z-fx04a-b2/oracle-v2-published.json)).
- **B2 preparada** ([prelaunch-B2.json](I-62-F6/FX-04a/R20261007T061300Z-fx04a-b2/prelaunch-B2.json)): clon `D:\r62-fixture\B2` en `cabed547`, limpio, solo
  `origin`, sin referencias a B1 ni a los oráculos; sin sesión previa en esa carpeta; sin `CLAUDE.md` global; gancho de Orca `{}`; plugin `github`. Texto
  inicial neutro `B2-kickoff.md` (SHA-256 `233b0582…`): instrucción y esquema v2, sin valores esperados. Segunda sesión de Principal B (reapertura +1,
  tope 2).

## 75. Noche (decisiones §54): SHA del oráculo v2 durable antes de B2, B2 en HUMAN_LAUNCH_REQUIRED y medición pasiva de la instalación actual de Codex

- **FX-04a:** el SHA-256 canónico del oráculo v2 (`5e7a78d3…`) quedó durable en `707b4daa` (publicado a las 06:19:01Z) antes de cualquier B2; clon
  `D:\r62-fixture\B2` en `cabed547`, D.6 repetida, texto inicial neutro v2. **FX-04a = HUMAN_LAUNCH_REQUIRED** (B2 en la tarjeta de mañana).
- **Codex** ([result.json](I-62-F6/OD-2/R20261007T061915Z-od2d-night-passive/result.json)): huella `9EA26634…` estable desde las 03:29:05Z (igual a las
  03:40Z y 06:19Z); app `26.1002.6548.0`; binario único `97c57e4e…`; 9 claves de integración de la app cambiadas frente a `723A6898…`, estructura igual;
  instantánea por clave nueva. Sin ejecutar el binario. **Paquete OD-2d vigente** en
  [owner-decision-packets.md](I-62-prep/owner-decision-packets.md) (aceptación exacta condicionada a una medición controlada sin cambios).

## 76. Noche (decisiones §54): staging de FX-02, FX-06 y F7/READY hasta sus fronteras, con revisión adversarial

- **Método:** un workflow de 9 agentes (redacción, verificación adversarial contra V14, A-1, AUTOMATION_PLAN §16 y los esquemas, y revisión), solo con
  lecturas del repositorio y del origen del fixture y escritura en el scratchpad; 0 errores; los tres revisores aplicaron 16, 16 y 15 hallazgos y no
  rechazaron ninguno. La supervisión revisó el resultado antes de integrarlo y corrigió la tarjeta de A2 para que no recomiende un modo de permisos.
- **FX-02** ([kits/FX-02](I-62-F6/kits/FX-02/README.md)): secuencia de extremo a extremo de QH2 a Q7 VERIFIED de T1 (27 pasos con actor, transporte,
  punto durable, evidencia, cláusula y frontera), topes de D.3 y contabilidad, contratos del Controller y del Worker/Reviewer, plantillas de la orden
  FX-U1-O4 y de la designación de A2, tarjeta de lanzamiento de A2 y `frontiers.json`. **Hallazgo de secuencia:** ninguna escritura en el origen del
  fixture ni apertura de A2 mientras FX-04a esté abierto (B2 lee el último punto durable del origen). Decisiones pendientes: grupo (a) del Coordinator del
  fixture y **grupo (b) del Coordinator de I-62 (CD-01..CD-21)**; mientras falten, FX-02 queda UNVERIFIED con esa causa. Negativos y comprobaciones de
  supervisión sellados fuera del repositorio ([sealed-supervision-files.json](I-62-F6/kits/sealed-supervision-files.json)).
- **FX-06** ([kits/FX-06/staging](I-62-F6/kits/FX-06/staging/README.md)): secuencia de D.8 con sus fronteras (FX06-F01..F13), plantilla neutra de orden,
  RLA, contrato de invocación del Architect (cierre de entradas, fidelidad, dos invocaciones distintas), transiciones, presupuestos de A-1, puntos de CI,
  esquema de evidencia, **auditor de `OWNER_AS_MESSAGE_BUS`** (`owner_as_message_bus_audit.py`, autoprueba 40/40 con entradas sintéticas) y escaneo
  previo a la publicación (`prepublish_scan.py`, 9/9). Riesgo R-07: la cabecera de X v1 nombra el escenario (OQ-23 al Coordinator). Sin `OWNER_AS_MESSAGE_BUS`
  calculado: no hay corrida real.
- **F7 y READY** ([I-62-prep/f7](I-62-prep/f7/ready-dry-run.md)): borrador factual de FOUNDATIONS con los hechos de F6, matriz de ensayo READY-01..09
  (evidencia disponible y faltante, acciones del Owner y del Coordinator, comandos), lista de cierre documental e integración, paquetes OV, disposición de
  ideas futuras y OD-1 (sin solicitarla); ningún READY declarado; valores esperados de OV sellados.
- **Sin ejecuciones:** ningún `codex-cli`, ningún `claude-cli`, ninguna sesión abierta ni mensaje a sesiones del fixture, ninguna escritura en el fixture.

## 77. Noche (decisiones §54), segunda ronda: críticos, tarjeta de la mañana reordenada, kits corregidos y solicitud consolidada al Coordinator

- **Críticos de segunda pasada** (workflow de 4 agentes sobre `2cef994f`): 50 hallazgos, 0 BLOCKING; entre líneas 9 (4 MAJOR), FX-02 14 (8 MAJOR), FX-06 12
  (4 MAJOR) y F7 15 (5 MAJOR). Hallazgo principal: la tarjeta de la mañana pedía OD-2d antes de clasificar FX-04a y con una sola línea que autorizaba la
  sonda y aceptaba su resultado de antemano, y debajo de ella quedaban respuestas obsoletas listas para pegar.
- **Tarjeta de la mañana** ([kits/README.md](I-62-F6/kits/README.md)), reescrita por la supervisión: 1) B2; 2) una sola ronda de disposiciones del
  Coordinator, empezando por la clasificación de FX-04a, con el alcance de OD-2d-PROBE (operaciones, directorios, tope, P1b); 3) OD-2d sobre el paquete
  medido; 4) A2; 5) Principal de FX-06 en `D:\r62-fixture\A6`, solo tras el QH de FX-02 con la terminación de A2 acreditada y las precondiciones de FX06-F04.
  Cada acción lleva escenario, carpeta, runtime, modelo, effort, texto inicial, qué hacer, qué no pegar y evento de compleción; el modo de permisos lo
  elige el Owner. Las secciones anteriores pasan a [HISTORY-owner-card-2026-10-06.md](I-62-F6/kits/HISTORY-owner-card-2026-10-06.md) («no ejecutar»),
  con las respuestas obsoletas tachadas. En el paquete OD-2d, la línea condicionada se sustituye por dos actos (OD-2d-PROBE y después OD-2d), y las
  respuestas obsoletas quedan tachadas.
- **Aplicación** (workflow de 10 agentes: un autor por línea en el scratchpad, verificador adversarial, una pasada de reparación y un registro único;
  0 errores, ningún hallazgo rechazado; los verificadores encontraron 1 MAJOR y 22 MINOR, todos reparados):
  - **FX-02:** commit GREEN con el resumen de estado; bindings del Worker y del Controller de verificación dentro de la ventana (S18b, S21b); caminos
    no VERIFIED (S25n) y ciclo de corrección (S30); listas A y B del aislamiento de A2 y auditoría posterior; colocación de N8-N10 pendiente de CD-11;
    preguntas nuevas OQ-28..OQ-32 (CD-22..CD-25). Archivos de supervisión **resellados**: `negatives.md` `63e84ba3d7f06371e4c44a7c2cf671757ac34d61caf35bf56fed94524507b0e7` y
    `supervision-checks.md` `444943584d3d792a8d152377fd62ee07aa8a54de110aeb61ea3fbbf933a28512`; la versión anterior queda archivada fuera del repositorio
    ([sealed-supervision-files.json](I-62-F6/kits/sealed-supervision-files.json)).
  - **FX-06:** auditor v3.1.0, autoprueba **64/64** con entradas sintéticas, `prepublish_scan.py` 9/9, ambas reproducidas byte a byte por la
    supervisión. La ventana queda anclada al commit del paso 1, y los marcadores del archivo de decisiones que no ponga el Coordinator se marcan desde
    la RLA. Se cubren también las aperturas de otras sesiones, el relevo sin relevador conocido, el «continúa» de la supervisión anterior a la ventana
    (sin bandera hasta OQ-20), la reproducción de la materialización y la auditoría de lecturas tras la corrida, y la transición B1 con una invocación y
    un `RunId` nuevos. El texto de permisos es neutro (OQ-25) y el kit anterior queda marcado como sustituido.
  - **F7:** F-04 se separa en A2 y en el Principal de FX-06, y F-06 exige F-05. La entrada de FOUNDATIONS se limita a hechos firmes. Con `main` movido,
    la fila 8 del cierre aplica la ruta R. C-20b compara con E.5 y el Coordinator clasifica las diferencias. Las líneas de READY-09 son del cuerpo del
    commit (el único trailer de Git es `Co-Authored-By`; Q18). El ensayo de C-20b está custodiado: `EQUAL (MV-2..MV-6)`,
    [c20b-dryrun-e9473425.json](I-62-prep/f7/measured/c20b-dryrun-e9473425.json). Preguntas Q18-Q20.
- **Solicitud consolidada** ([coordinator-disposition-request.md](I-62-F6/kits/coordinator-disposition-request.md)): 65 preguntas únicas (U-01..U-65).
  Las 6 de compuerta van primero: U-01, la clasificación de FX-04a; U-02..U-05, OD-2d-PROBE, directorios, tope y P1b; U-06, los «continúa». Además: 5
  actos procedimentales de la supervisión, 9 duplicados fusionados, 10 divergencias con decisor único propuesto (el Coordinator de I-62) y un solo
  bloque de respuesta. No contiene valores esperados.
- **Sin ejecuciones:** ningún `codex-cli`, `claude-cli`, sesión, mensaje ni escritura en el fixture. A las 08:20Z la huella seguía en `9EA26634…` y el
  binario en `97c57e4e…`. `fx/u1` = `cabed547`, con la última CI del fixture (37567674783) en `success` sobre ese SHA.

## 78. FX-04a B2: corrida terminada y SHA de la respuesta durable antes de cargar el oráculo v2 (decisiones §53)

- **Lanzamiento:** el Owner abrió B2 (`local_97ac0196…`) a las 20:06:01Z en `D:\r62-fixture\B2`, con `claude-opus-5-5` y `xhigh`; el modo de permisos lo
  eligió el Owner (`bypassPermissions`, registrado del transcript). Las entradas automáticas no cambiaron respecto de `prelaunch-B2.json`.
- **Desviación del texto inicial (KICKOFF_SCHEMA_NOT_DELIVERED), causada por la supervisión:** la copia de referencia de la tarjeta de la mañana publicada
  en el chat sustituía el esquema JSON por un marcador, y el Owner pegó esa copia. B2 recibió la instrucción íntegra, pero **no**
  `response.v2.schema.json`: el mensaje tiene 1 394 caracteres (SHA-256 `fcfdfda7…`), frente a los 8 065 bytes de `B2-kickoff.md` (`233b0582…`). La
  supervisión preparó la parte que faltaba (`B2-kickoff-part2-schema.md`, `9ec1be82…`) y recomendó pegarla, pero no se pegó: la sesión tiene un solo
  mensaje humano. B2 detectó la ausencia, buscó el esquema en todas las revisiones del clon, declaró `UNKNOWN` la conformidad y entregó las cuatro claves
  pedidas con una estructura propia.
- **Terminación acreditada:** `isRunning` = false en dos observaciones (20:58:59Z y 21:00:04Z), sin actividad desde las 20:18:03Z.
- **Preflight de B2:** `P20261007T201013Z-25e8`, RESUME_DECISION, **MATCH / ELIGIBLE** (Frontera, Long-horizon, lectura y RUNTIME_OBSERVED en MATCH).
- **Aislamiento:** no queda UNVERIFIED. B2 solo usó `get_session("self")` (2 veces); leyó solo el clon y su propio scratchpad; escribió solo en su
  scratchpad; el clon sigue sin cambios (`cabed547`, árbol limpio, refs sin cambio). La cobertura es la declarada (D.6).
- **Respuesta registrada antes de comparar:** [B2-response.json](I-62-F6/FX-04a/R20261007T061300Z-fx04a-b2/B2-response.json), **SHA-256 canónico
  `74e69f35b7c3fcfb0bcdac63da98339d25c58bf07f35c24402b798463d009587`** (archivo `5226a77d…`; igual al JSON del mensaje final), con el registro de la
  corrida en [b2-run.json](I-62-F6/FX-04a/R20261007T061300Z-fx04a-b2/b2-run.json). El oráculo v2 **no** se ha cargado todavía.
- **Estado:** `docs/automation/state/I-62.yml` no era YAML válido desde las ediciones de F6 (valores de `next_action` y `f6_status` con «: » sin comillas);
  corregido con comillas, sin cambiar ningún valor (comprobado por ida y vuelta con `yaml.safe_load`).

## 79. FX-04a B2: comparación mecánica con el contrato v2 (FAIL bruto 2/23) y propuesta de la supervisión

- **Orden respetado:** respuesta guardada fuera del clon → SHA canónico `74e69f35…` durable en `4ea66233` → solo entonces se cargó el oráculo v2.
  El oráculo (`5e7a78d3…`, archivo `ae384682…`) coincide con el publicado en `707b4daa`.
- **Resultado mecánico** ([comparison-v2.json](I-62-F6/FX-04a/R20261007T061300Z-fx04a-b2/comparison-v2.json)): **FAIL bruto, 2/23**. Solo coinciden
  `facts.branch` y `facts.claim_id`; faltan 20 campos y `facts.protocol` tiene otra forma. No hay normalización posterior ni tercer oráculo.
- **Causa:** KICKOFF_SCHEMA_NOT_DELIVERED (§78). Sin el esquema, B2 no conocía las claves del contrato, así que el resultado mide la entrega y no la
  reconstrucción. No hay evidencia de que el contrato v2 sea inválido.
- **Propuesta de la supervisión** ([análisis](I-62-F6/FX-04a/R20261007T061300Z-fx04a-b2/comparison-v2-analysis.json),
  [informe para el Coordinator](I-62-F6/FX-04a/R20261007T061300Z-fx04a-b2/b2-report-for-coordinator.md)): B2 = INVALID_LAUNCH; FX-04a y C-25a =
  UNVERIFIED. El FAIL bruto se conserva. Decisiones pedidas: U-01a (clasificación), U-01b (B3 por encima del tope de D.3, con control del SHA del
  primer mensaje) y U-01c (cierre de FX-04a como UNVERIFIED con limitación, si no hay B3).
- **Lección de la supervisión:** ninguna tarjeta del Owner lleva copias abreviadas de un texto inicial. Se pega solo desde el archivo, y la supervisión
  comprueba el SHA-256 del primer mensaje en cuanto se abre la sesión.

## 80. Disposición §55: B2 INVALID_LAUNCH, B3 no autorizada, origen del fixture liberado, candidata A-2 y origen congelado de QH2

- **Clasificación registrada (§55):** B2 = INVALID_LAUNCH; el FAIL bruto 2/23 se conserva sin cambios (`74e69f35…`, `c9f9419d`); FX-04a y C-25a =
  UNVERIFIED; FX-04a sigue OPEN. B3 no está autorizada con el Freeze vigente. F6 GATE PASS es imposible mientras C-25a no se satisfaga.
- **Candidata A-2** ([I-62-A-2.md](../../initiatives/I-62-A-2.md)), PROPUESTA y no aplicada. Añade dos párrafos al final de V14 D.3 y no cambia el texto
  de ninguna otra cláusula. A2-P1 da como máximo una reejecución limpia extraordinaria por escenario de F6 tras una acreditación INVALID_LAUNCH del Coordinator y
  su disposición. A2-P2 da un bloque nuevo de como máximo dos sondas de solo lectura para un par exacto (`BinaryHash`, `AppVersion`) tras una
  actualización automática, con autorización por bloque y OD-2 sobre la huella resultante. Ninguno de los dos reinicia contadores. Material por M-03 y
  M-04. Antes de publicarla, tres críticos adversariales revisaron la fidelidad al texto del Coordinator, la exactitud de las cláusulas y las guardas;
  sus hallazgos están aplicados: se conserva el FAIL de aislamiento de D.6, +1 como máximo por fila compartida, B3 necesita la autorización de consumo
  del Owner porque OD-5 no la cubre, un solo bloque por actualización, las sondas no usadas caducan, P-07 aplica a cada bloque y se registran los
  hechos de las sondas de OD-2d-PROBE. Guardas: [a2-guards.py](I-62-A2/a2-guards.py). Comprueban el alcance del delta, aplicándolo sobre V14; las
  rutas, con decisiones y evidencia solo por añadido; los blobs congelados; y C-20b con la herramienta fijada por blob. Incluyen un modelo de
  presupuestos con 43 vectores por regla y 7 mutantes que deben fallar. El resultado sobre el commit exacto de A-2 se custodia en el commit
  siguiente. Paquete del Architect: [I-62-architect-package-A-2.md](../../initiatives/I-62-architect-package-A-2.md).
- **Hallazgo material para la aplicación de A-2:** una B3 tiene que leer los hechos remotos de QH2. Liberar el origen (§55 punto 3) permite que FX-02
  escriba en `fx/u1`, y eso cambiaría lo que B3 vería con `git ls-remote`. Para no perder esa posibilidad, la supervisión creó **antes de cualquier
  escritura** una instantánea congelada del origen: `D:\r62-fixture\fx04a-qh2-origin.git` (`git clone --mirror`, remoto eliminado; digest de refs
  `275d977b3e16bfe2f81261a443fb814e109d6fc526c0fb0c6c7c9d3a1c22713e`, igual al del origen vivo en ese momento; `fx/u1` = `cabed547`). Si se usa para
  B3, o si se prohíben las escrituras en `fx/u1` antes de B3, lo decide el Coordinator (A-2 §8, Q-A2-04).
- **Codex:** asignación congelada de sondas agotada (§55 punto 11); ninguna sonda; `codex-cli` sigue en P-01 / STOP.

## 81. Preparación tras decisiones §55 (FX-02, FX-06, F7 y registro) y kit de la revisión del Architect de A-2 custodiado antes del lanzamiento

- **Carriles §55** (workflow de 9 agentes: autor, verificador adversarial y reparación por carril; 0 errores; verificadores: FX-02 2 MAJOR y 4 MINOR,
  FX-06 3 MINOR, F7 1 MAJOR y 6 MINOR; todos reparados):
  - **FX-02:** CD-10 queda DECIDIDA EN PARTE. §55 U-03 fija solo los `-C` (Controller `A2`); los directorios de las ≤ 2 sondas del bloque nuevo
    quedan abiertos como OQ-35 / CD-28, con `A2`/`arch` como propuesta de la preparación. S11 y S12 (bloque y OD-2d) van antes de abrir A2 (S04).
    S13 espera al QR de A2 (S10). La autoridad de la sonda es la de A-2 (regla 3 de A2-P2).
  - **FX-06:** variante A (`arch`), sin A6 para el Architect. OQ-11 sigue abierta, igual que la obligatoriedad de QU LAUNCHED. Nuevo
    [p1b-owner-packet.md](I-62-F6/kits/FX-06/staging/p1b-owner-packet.md), solo si P1b sigue haciendo falta tras A-2. Nuevas OQ-26 (instantánea de
    QH2 frente a escrituras en `fx/u1`), OQ-27 (cómputo de P1b) y OQ-28 (orden de P1b). Auditor sin cambios (64/64).
  - **F7 y registro:** FX-04a OPEN/UNVERIFIED; F6 GATE PASS imposible hasta C-25a; A-2 [PENDIENTE] en la entrada de FOUNDATIONS y en READY-09. El
    registro marca U-01..U-05 como DECIDIDAS (§55) y añade U-66..U-71 y la divergencia D-11 (atribución del directorio de la sonda del Architect).
  - **Sello:** `supervision-checks.md` resellado (`729fc3ef…`; la versión anterior `44494358…` queda archivada fuera del repositorio);
    `negatives.md` sin cambio.
- **Kit de la revisión del Architect de A-2** ([README](I-62-architect-A-2/R20261008T014941Z-68fe/README.md)), adaptado del kit v4 acreditado de A-1 r5 y
  custodiado **antes** del lanzamiento:
  - clon `D:\r62-arch-a2` en `4a059afa` (solo `main`, sin remoto, limpio, sin enlaces), con los blobs de A-2 `d47f71b6…`, del paquete
    `f45a2384…`, de las guardas `91c9c2a4…` y de su resultado `33ffbb9d…`;
  - cierre con 12 insumos canónicos y 14 transitivos;
  - orden `order.txt` = el cuerpo exacto de §55 (`bfa265cd…`);
  - `prompt.md` de 23 737 bytes (`22d93ae5…`), idéntico al del directorio del run;
  - auditor v4-a2.1 con autoprueba **23/23** sobre el clon real; fidelidad previa 3/3;
  - las guardas G5 pasan en el clon (self-test 43/43, 7/7 mutantes).

  El verificador adversarial del kit encontró 2 MAJOR (política de herramientas Bash/PowerShell; premisas sobre `order.txt`) y 6 MINOR, todos
  reparados.
- **Discordancias del paquete, señaladas de forma neutral en el prompt y no resueltas:**
  - el paquete §1 y §4 nombran Q-A2-01..05, pero A-2 §8 tiene siete preguntas, y el resultado dispone las siete;
  - «la fila de P-07 en §8» corresponde en V14 a la línea 657 (§9.3); P-07 se define en AUTOMATION_PLAN §16.11.
- **Lanzamiento:** HUMAN_LAUNCH_REQUIRED. No hay transporte automático elegible: `codex-cli` está en P-01 con la asignación agotada, `claude-cli`
  sin autenticar (OD-3) y la sesión principal no puede abrir sesiones limpias. Tras este commit, la sesión crea **una sola vez** la tarjeta de tarea
  de la app, con `cwd` = el clon y el `prompt.md` custodiado; el Owner la lanza con un clic (precedente de A-1 r5, evidencia §57).

## 82. Revisión formal del Architect de A-2 (R20261008T014941Z-68fe): CHANGES REQUIRED; auditor NOT_ACCREDITED; custodia

- **Lanzamiento:** el Owner pulsó la tarea `task_241699b1`. La sesión `local_b7dce5ad…` corrió de 03:07:27Z a 03:22:01Z, con `claude-opus-5-5` y
  `xhigh`, en el clon `D:\r62-arch-a2` (worktree de la tarea). El primer y único mensaje humano contiene el texto fijo (`4daa606c…`). La
  terminación está acreditada: `isRunning` = false en dos observaciones (18:09Z y 18:20Z).
- **Veredicto** ([registro](../../initiatives/I-62-architect-review-A-2.md), [output.json](I-62-architect-A-2/R20261008T014941Z-68fe/output.json)):
  **CHANGES REQUIRED**.
  - REQUIRED: A62-A2-01. La regla 2 de A2-P1 debe conservar todo FAIL por violación observada (D.4, FX-04b, D.8), no solo el de aislamiento de D.6.
  - OPTIONAL: A62-A2-O1..O3.
  - Materialidad M-03 y M-04; ninguna decisión del Owner.
- **Auditor** v4-a2.1, literal ([audit.json](I-62-architect-A-2/R20261008T014941Z-68fe/audit.json)): **NOT_ACCREDITED**, con 3 motivos en 36 llamadas.
  - Son dos defectos de análisis del propio auditor: `python - <arg>` y `python -X utf8 -`.
  - Hay además una desviación literal declarada por el revisor: scripts Python en línea que leen archivos del cierre en el clon.
  - Pasan la identidad, el orden, la custodia del run, el clon limpio y todas las premisas.
  - La acreditación la decide el Coordinator. El auditor no se corrige después de la corrida.
- **Lectura de la supervisión:** A62-A2-01 es técnicamente correcta y de una sola frase. No cambia la aplicación a FX-04a, porque B2 no tiene
  lecturas prohibidas ni otras violaciones.
- **`claude-cli`:** el Owner informó el 2026-10-08 que autenticó la CLI. La sesión no la ha ejecutado ni medido, porque OD-3 consta RECHAZADA
  (decisiones §46). Paquete de reconsideración en [owner-decision-packets.md](I-62-prep/owner-decision-packets.md), §«OD-3 (reconsideración,
  2026-10-08)». OD-3 congelada solo cubre el fixture; la re-revisión de A-2 con `claude-cli` exigiría una autorización aparte del Owner y del
  Coordinator.

## 83. Disposición §56: A-2 corregida (A62-A2-01 y A62-A2-O1..O3), OD-3 = A y CLAUDE-CLI-I62 = A registradas

- **A-2 corregida** en el mismo archivo ([I-62-A-2.md](../../initiatives/I-62-A-2.md), blob `f1e1d6f08d3cf677500794c7d0019433a4dc7d4e`, antes `d47f71b6…`; §11 «Cambios frente
  al blob `d47f71b6`»):
  - A62-A2-01: la regla 2 conserva todo FAIL por violación observada (D.4, D.6, paso 7 de FX-04a, FX-04b, D.8), que no se acredita INVALID_LAUNCH ni
    habilita la reejecución;
  - O1: INVALID_TEST_ORACLE no es PASS ni FAIL ni habilita la reejecución;
  - O2: el par sucesor solo cuenta como observado con la huella resultante estable;
  - O3: solo los presupuestos restantes, confirmados antes por el Coordinator y el Owner.

  Literales nuevos en §2.1: las filas FAIL de FX-04b y D.8. Q-A2-02 y Q-A2-05 quedan resueltas en el delta.
- **Guardas** ([a2-guards.py](I-62-A2/a2-guards.py), blob `b72d26ea6b30b5cfceaa8140a536a08086562b24`): base fijada en el commit anterior a la corrección (`b553608c`), con A-2 y el
  paquete modificados (M). G5 tiene **50 vectores**, incluidos los nuevos P1-23..P1-27 y P2-22..P2-23, y **11 mutantes** (4 nuevos), todos eliminados
  por su vector; nuevas frases vinculadas al texto. El resultado de `run` sobre el commit exacto de la corrección se custodia en el commit siguiente.
- **Paquete nuevo del Architect** ([I-62-architect-package-A-2.md](../../initiatives/I-62-architect-package-A-2.md)): re-revisión con cierre de A62-A2-01
  y O1..O3. Transporte preferido: una celda `claude-cli` medida y elegible (§56, puntos 6-9); si no, HUMAN_LAUNCH_REQUIRED con una sesión nueva. Se
  corrigen las dos imprecisiones del paquete anterior: Q-A2-01..07, y la fila de P-07 en §9.3 de V14 definida en AUTOMATION_PLAN §16.11.
- **Decisiones del Owner registradas (§56):** OD-3 = A (solo el fixture) y CLAUDE-CLI-I62 = A (medición y uso read-only para ARCHITECT y REVIEWER de
  I-62). La caracterización acotada de `claude-cli` (§56, punto 7) es el paso siguiente; hasta entonces `claude-cli` no es elegible.

## 84. claude-cli caracterizado y elegible; kit de la re-revisión de A-2 por CLI custodiado antes del lanzamiento (decisiones §56)

- **Publicación de la A-2 corregida:** corrección en `3bbaabef` (A-2 blob `f1e1d6f0…`; guardas `b72d26ea…`). Publicación en `fd411b13` con el resultado de
  `a2-guards.py run --head 3bbaabef`: G1-G5 PASS (21 literales; C-20b EQUAL; G5 50/50 y 11 mutantes eliminados).
- **Caracterización de `claude-cli`** ([README](I-62-claude-cli/R20261008T183600Z-char/README.md), registro `20d0de2c…`):
  - el binario de usuario 2.1.270 no admite `claude-opus-5-5`;
  - el binario 2.1.293 que gestiona la app de escritorio (Authenticode Anthropic) es utilizable;
  - autenticación `claude.ai`, suscripción max, sin leer credenciales;
  - modelo y effort observados por mensaje; prompt y lectura fieles; salida estructurada;
  - cancelación y timeout por terminación del invocador;
  - sin CLAUDE.md, memoria ni ganchos en modo seguro;
  - `--add-dir` medido en C3 (PASS).

  **ARCHITECT y REVIEWER ELIGIBLE** en esta observación: instalado, autenticado, invocación medida, capacidades MATCH o ABOVE_REQUIRED, consumo cubierto
  (CLAUDE-CLI-I62 = A), STALE = false e independencia de Actor, Session y Context satisfecha. Provider PREFERRED no se cumple, y se declara.
- **Kit `R20261008T184326Z-f1e2`** ([README](I-62-architect-A-2/R20261008T184326Z-f1e2/README.md)), custodiado **antes** del lanzamiento:
  - clon limpio `D:\r62-arch-a2r` en `fd411b13`;
  - run con `order.txt` = el cuerpo exacto de §56 (`e8b00328…`), `prompt.md` (`0d1d5a4d…`), `delta.diff` y el objeto anterior;
  - el revisor solo puede usar Read, Grep y Glob;
  - auditor v5.1 con autoprueba 107/107;
  - compuerta de transporte MEASURED;
  - `launch.py --dry-run` con 9 de 10 comprobaciones en Ok (la autenticación solo se comprueba en el lanzamiento real).
- **Sin pedir al Owner otra sesión de escritorio** (decisiones §56, punto 18): la re-revisión la lanza la sesión principal por `claude-cli`.

## 85. Re-revisión formal de la A-2 corregida por claude-cli (R20261008T184326Z-f1e2): AGREED, ACCREDITED

- **Lanzamiento automático** (decisiones §56, puntos 6-9; Owner CLAUDE-CLI-I62 = A). La sesión principal lanzó la re-revisión con `launch.py` (preflight de
  10 comprobaciones en Ok, compuerta MEASURED) sobre el clon limpio `D:\r62-arch-a2r` en `fd411b13`. Sesión `26a89860…`, de 20:56:00Z a 21:10:32Z;
  salida 0; ninguna denegación; modelo `claude-opus-5-5`. No hizo falta ninguna sesión de escritorio abierta por el Owner.
- **Veredicto** ([registro](../../initiatives/I-62-architect-review-A-2-r2.md), [output.json](I-62-architect-A-2/R20261008T184326Z-f1e2/output.json)): **AGREED**.
  - A62-A2-01 y A62-A2-O1..O3: CLOSED.
  - Ningún REQUIRED; A62-A2-O4..O6 OPTIONAL.
  - Q-A2-01..07 sin hallazgo; M-03 y M-04; ninguna decisión del Owner.
- **Auditor** v5.1 custodiado ([audit.json](I-62-architect-A-2/R20261008T184326Z-f1e2/audit.json)): **ACCREDITED**, 0 motivos en 32 llamadas.
- **Siguiente:** veredicto del Coordinator sobre la A-2 exacta (blob `f1e1d6f0…`). Hasta entonces, ni B3 ni bloque de medición.

## 86. A-2 AGREED (decisiones §57): acuerdo append-only; Claude CLI aceptado como transporte; solicitudes de recuperación de F6 en preparación

- **Acuerdo:** Architect AGREED (R20261008T184326Z-f1e2, ACCREDITED) y Coordinator AGREED sobre la A-2 exacta (blob `f1e1d6f0…`; publicación `fd411b13`).
  Queda registrado append-only en las decisiones (§57), en esta evidencia y como anexo del [registro de la re-revisión](../../initiatives/I-62-architect-review-A-2-r2.md).
  `docs/initiatives/I-62-A-2.md` no cambia. A62-A2-O4..O6: ACCEPTED NON-BLOCKING OPTIONAL, documentados sin modificar el objeto.
- **Claude CLI:** la caracterización de `claude-cli` 2.1.293 (evidencia §84) queda aceptada como elegibilidad vigente para ARCHITECT y REVIEWER; transporte
  preferido para las revisiones reales autorizadas de I-62 con celda elegible y cierre limpio; nunca PRINCIPAL_COORDINATOR, WORKER ni escritura.
- **Siguiente:** solicitudes concretas de B3 (A2-P1) y del bloque de Codex (A2-P2), en una sola solicitud al Owner; A-3 con los ocho MAJOR y la
  investigación de MATERIAL_ADAPTER_FINGERPRINT.

## 87. Solicitud única de recuperación de F6 (decisiones §57): B3 extraordinaria de FX-04a y bloque de Codex; fuente QH2 preservada

- **Solicitud** ([F6-recovery-request-2026-10-08.md](I-62-F6/requests/F6-recovery-request-2026-10-08.md)). Cada aplicación de A-2 necesita una
  disposición del Coordinator que la nombre y la autorización de consumo del Owner. Las dos van en una sola respuesta del Owner (`B3-CONSUMO = A` y/o
  `BLOQUE-CODEX-CONSUMO = A`). La OD-2 sobre la huella resultante se decide después de medir.
- **B3:**
  - fila Principal B de FX-04a, tope 2 → 3 con la reejecución; total de la ronda ≤ 3 (+1 B2);
  - presupuesto restante suficiente (0 invocaciones de modelo);
  - una sesión `claude-desktop-session` abierta por el Owner, porque es un rol de Principal que `claude-cli` no cubre;
  - texto inicial `B3-kickoff.md` con los mismos bytes que el de B2 (`233b0582…`), entregado desde el archivo.
- **Fuente QH2 preservada:** el clon `D:\r62-fixture\B3` (`fx/u1` = `cabed547`, limpio) tiene como único `origin` la instantánea congelada
  `fx04a-qh2-origin.git` (refs `275d977b…`, `fsck` limpio, sin remoto). Un gancho `pre-receive` de la instantánea rechaza toda escritura, comprobado
  con un push rechazado ([prelaunch-B3.json](I-62-F6/FX-04a/R20261008T2200Z-fx04a-b3/prelaunch-B3.json)).
- **Codex, otra actualización automática** ([result.json](I-62-F6/OD-2/R20261008T2200Z-codex-passive/result.json)): binario `3553cd6e…` (escrito el
  2026-10-07T11:57Z), app `26.1002.7124.0`, `config.toml` `6518EFAB…` estable desde el 2026-10-07T20:05Z. Cambian las mismas 9 claves
  (`mcp_servers.node_repl.*` y `notify`). El par `97c57e4e…` nunca llegó a medirse. El bloque se pide para el par vigente, con un tope de 2 sondas de
  solo lectura (Controller y Architect). `codex-cli` sigue en STOP P-01.

## 88. A-3 candidata publicada (decisiones §57, punto 4): MATERIAL_ADAPTER_FINGERPRINT como requisitos; OD-2-MAT preparada como decisión del Owner

- **Objeto:** [I-62-A-3.md](../../initiatives/I-62-A-3.md), PROPUESTA (candidata), sin revisión formal (blob `ea6721f7`). Lo acompañan:
  - el [anexo no normativo](../../initiatives/I-62-A-3-annex-maf-codex.md) de `codex-cli` (`f410f7fc`);
  - el [paquete del Architect](../../initiatives/I-62-architect-package-A-3.md) (`d289329d`);
  - las guardas y su `self-test`, en [I-62-A3/](I-62-A3/);
  - la investigación: `maf-codex.md` (`a297efb2`), `od2-evolution.md` (`3b8dc620`) y el paquete de decisión
    [OD-2-MAT](I-62-A3/od2-material-baseline-owner-packet.md) (`b2c8ec8a`).
- **Los ocho MAJOR** de la segunda verificación (K-01..K-03 y X-01..X-05) están aplicados y cubiertos por las guardas.
- **Revisión adversarial repetida, rondas 3 a 8.** Hallazgos M1..M6, todos con disposición en el §12 de A-3.
  - Tras cuatro rondas con defectos MAJOR solo en el verificador de valores de la regla 11, la decisión **S-01** retira ese verificador.
  - La regla 11 queda así: la huella se compara solo por su valor exacto; todo cambio deja al sucesor fuera de SUCCESSOR_COMPATIBLE y conserva
    P-01; A-3 no define ninguna clase de cambios aceptables ni lee valores; la evidencia es solo por nombre; los requisitos para una A-n futura
    van como lista.
  - La última ronda encontró 1 MAJOR y 2 MINOR, todos aplicados. No queda ningún hallazgo abierto.
- **Guardas:**
  - `self-test` PASS: 116 vectores (1 + 19 + 96) y 105 mutantes eliminados por su vector esperado; perfil RED de V14 con 11 que pasan con el
    motivo, y 19 + 38 que fallan a ciegas.
  - `preview` PASS sobre `76b48e78`.
  - `run` (G1-G5b) va en el commit siguiente.
- **MATERIAL_ADAPTER_FINGERPRINT (Codex).** De las 9 claves que cambia cada actualización, 8 designan código que el runtime lanza o la frontera
  de confianza de ese código. No hay ninguna clase inocua demostrada. P-01 sigue sin cambio en producción.
- **OD-2.** Pasar a una línea base material cambia autoridad reservada al Owner. Por eso se prepara el paquete OD-2-MAT, con opciones A, B, C y D,
  que no se resuelve por disposición. Se añadirá a la solicitud única de recuperación. Ninguna opción acepta una huella ni autoriza leer valores.
- **Lectura de valores.** La comparación por clave está suspendida (§4 de la solicitud, `76b48e78`). El paquete trae el historial neutral de las
  lecturas anteriores.
- **Revisión formal:** pendiente de la orden del Coordinator. Su transporte previsto es `claude-cli` (decisiones §57, punto 3).

## 89. Guardas de A-3 sobre el commit de publicación: PASS; OD-2-MAT añadida a la solicitud única

- **Guardas.** `run` de [a3-guards.py](I-62-A3/a3-guards.py) con base `76b48e78` (= `A3_BASE`) y head `91e29886`, el commit que publica A-3: G1, G2, G3,
  G4, G5 y G5b PASS ([a3-guards-result.json](I-62-A3/a3-guards-result.json)).
- **Corrección antes del push.** La primera ejecución dio G2 FAIL («A3_BASE sin fijar»): había publicado las guardas con `A3_BASE` vacío, aunque
  la base ya se conocía. Fijé `A3_BASE` en el mismo commit de publicación antes de empujarlo, que pasó de `21f9ec39` a `91e29886`, y repetí la
  ejecución. Nada se empujó con `A3_BASE` vacío.
- **Solicitud única de recuperación, §5** ([solicitud](I-62-F6/requests/F6-recovery-request-2026-10-08.md)).
  - Contenido: la decisión del Owner OD-2-MAT, con sus cuatro líneas literales copiadas del paquete, y la tabla de respuesta única con
    `B3-CONSUMO`, `BLOQUE-CODEX-CONSUMO` y OD-2-MAT.
  - Índice de paquetes: [owner-decision-packets.md](I-62-prep/owner-decision-packets.md) apunta al paquete.
- **Siguiente.**
  - Disposición del Coordinator sobre B3, el bloque de Codex y la revisión formal de A-3 (transporte `claude-cli`).
  - Respuestas del Owner.

## 90. Decisiones §58: B3 y bloque de Codex en vigor; OD-2-MAT = A; revisión formal de A-3 por `claude-cli` autorizada

- **B3 (A2-P1).** En vigor con `B3-CONSUMO = A`. Comprobación previa al lanzamiento repetida el 2026-10-09: sin cambios frente a
  [prelaunch-B3.json](I-62-F6/FX-04a/R20261008T2200Z-fx04a-b3/prelaunch-B3.json).
  - Clon `fx/u1` = `cabed547`, limpio, con `origin` = la instantánea congelada (refs `275d977b…`); `fx/u1` vivo = `cabed547`.
  - Sin carpeta de proyecto de Claude para B3 y sin `CLAUDE.md` global.
  - Entradas automáticas con los mismos SHA-256.
  - Texto inicial `233b0582…` (8 065 bytes).
  - Sin sesiones de escritorio activas.
  - La abre el Owner. Mientras dure, no se ejecuta ninguna sonda de Codex, para que no haya procesos ajenos durante las comprobaciones de B3.
- **Bloque de Codex (A2-P2).** En vigor con la línea del Owner, más estricta: «sin lectura de valores». Se ejecuta tras B3.
  - Controles: huella exacta, nombres saneados y estructura, binario y versión, antes y después de cada operación, sin comparación por clave.
  - Después de las sondas: paquete de OD-2 exacta para el Owner.
- **OD-2-MAT = A.** OD-2 sigue siendo el SHA-256 exacto. El `CODEX_HOME` dedicado (D) queda como investigación posterior.
- **A-3.** Revisión formal por `claude-cli` sobre el blob `ea6721f7`: kit, cierre, auditor y preflight se custodian antes del lanzamiento.
  - Binario medido sin cambios: `2.1.293`, SHA-256 `8693c4a0…`.

## 91. FX-04a B3: corrida terminada y SHA de la respuesta durable antes de cargar el oráculo v2 (decisiones §58, punto 1)

- **Admisión:**
  - el Owner abrió B3 (`local_51e9c8c0…`) a las 03:58:12Z en `D:\r62-fixture\B3`, con `claude-opus-5-5`, `xhigh` y runtime 2.1.293;
  - el primer mensaje contiene `B3-kickoff.md` completo, byte a byte (`233b0582…`), dentro del envoltorio `<pasted_content>` que añade la app
    ([launch-check-B3.json](I-62-F6/FX-04a/R20261008T2200Z-fx04a-b3/launch-check-B3.json));
  - presupuesto: tercera y última sesión de la fila Principal B, con 0 invocaciones de modelo.
- **Terminación acreditada:** `isRunning` = false en dos observaciones (04:07:12Z y 04:08:03Z), sin actividad desde las 04:04:16Z.
- **Preflight de B3:** `P20261009T035955Z-5d9d`, RESUME_DECISION, **MATCH / ELIGIBLE**.
- **Aislamiento:** no queda UNVERIFIED.
  - Herramientas: solo `get_session("self")`.
  - Lecturas: solo el clon y su scratchpad; hechos remotos de la instantánea congelada.
  - Escrituras: solo su scratchpad. El clon no cambió (`cabed547`, limpio, refs sin cambio).
  - Cobertura: la declarada (D.6).
- **Respuesta registrada antes de comparar:** [B3-response.json](I-62-F6/FX-04a/R20261008T2200Z-fx04a-b3/B3-response.json), **SHA-256 canónico
  `4a99bbc3d8444b7d6b6478397968cca8e3efd3afed110fdd53e6d29b9ac33368`** (el único bloque JSON del mensaje final), con el registro de la corrida en
  [b3-run.json](I-62-F6/FX-04a/R20261008T2200Z-fx04a-b3/b3-run.json). El oráculo v2 **no** se ha cargado todavía.

## 92. FX-04a B3: comparación mecánica con el contrato v2 = PASS 23/23; propuesta de la supervisión

- **Orden respetado:** respuesta guardada fuera del clon → SHA canónico `4a99bbc3…` durable en `68ce619a` → solo entonces se cargó el oráculo v2
  (`5e7a78d3…`, archivo `ae384682…`), que es el mismo publicado en `707b4daa` antes de B2.
- **Resultado mecánico** ([comparison-v2.json](I-62-F6/FX-04a/R20261008T2200Z-fx04a-b3/comparison-v2.json)): **PASS, 23/23**, sin diferencias.
  Se usó la misma función `compare2` de `kits/FX-04a/fx04a_real.py` que con B2, sin normalización posterior.
- **Propuesta de la supervisión** ([análisis](I-62-F6/FX-04a/R20261008T2200Z-fx04a-b3/comparison-v2-analysis.json)):
  - B3 = corrida válida;
  - FX-04a = **PASS**;
  - C-25a = **PASS**;
  - B1 (INVALID_TEST_ORACLE) y B2 (INVALID_LAUNCH, con su FAIL bruto 2/23) se conservan como evidencia.
  La clasificación la decide el Coordinator. No hay B4.

## 93. Bloque A2-P2 de Codex (decisiones §58, punto 2): identidad estable, 2 sondas; regresión de shell de la CLI 0.162.0-alpha.2 (F6-OBS-03)

- **Identidad.** Seis mediciones, antes y después de cada operación, todas iguales:
  - binario `3553cd6e…`, el único `codex.exe`;
  - app `26.1002.7124.0`;
  - huella `6518EFAB…` con los 107 nombres saneados.
  No se leyeron valores ni se calcularon digests por clave. La versión de la CLI es nueva: **`codex-cli 0.162.0-alpha.2`**; la medida antes era
  0.160.1. Autenticación: «Logged in using ChatGPT». No hubo actualización durante el bloque, que sigue válido
  ([result.json](I-62-F6/OD-2/R20261009T041427Z-a2p2-block/result.json)).
- **Sonda 1, Controller** (`gpt-6-luna/high`, read-only, `D:\r62-fixture\A`). Completó 8/8 pasos con resultados correctos. Cada intento con el alias
  `WindowsApps\pwsh.exe` falló («CreateProcessAsUserW … Acceso denegado»), y la sonda recurrió a `cmd.exe`.
- **Sonda 2, Architect** (`gpt-6.1-sol/high`, read-only, `D:\r62-fixture\arch`). Completó 0/6 pasos: todos los comandos fallaron por la misma causa y
  no recurrió a otra shell.
- **En los dos clones,** HEAD y árbol limpio no cambiaron. Las sesiones de Codex registran `approval_policy` never y `sandbox_policy` read-only.
- **F6-OBS-03 (propuesto).** La CLI nueva no usa el `pwsh` del runtime que la receta 16.4 pone primero en el `PATH`. Es un cambio de comportamiento
  del runtime sucesor en una propiedad material: la ejecución de comandos bajo el sandbox de solo lectura. Con el criterio de A-3 (candidata), el
  sucesor no sería compatible. Lo decide el Coordinator.
- **Consecuencias.**
  - Paquete **OD-2e** para el Owner: OD-2 exacta sobre `6518EFAB…`, como exige el punto 2 de §58
    ([owner-decision-packets.md](I-62-prep/owner-decision-packets.md)). Aceptarla solo levanta P-01 por la huella.
  - El trabajo ordinario de `codex-cli` necesita además la disposición del Coordinator sobre F6-OBS-03.
  - El bloque consumió sus 2 sondas y no queda ninguna.

## 94. OD-2e = A (decisiones §59): huella exacta `6518EFAB…` aceptada; `codex-cli` sigue sin trabajo ordinario hasta resolver F6-OBS-03

- **Decisión.** El Owner aceptó la línea literal del paquete OD-2e. La medición pasiva en el momento de la aceptación coincide con lo aceptado:
  huella `6518EFAB…` (107 nombres), binario `3553cd6e…`, app `26.1002.7124.0`.
- **Efecto.** Deja de haber STOP P-01 por la huella. Un cambio futuro vuelve a ser P-01 con una OD-2 nueva.
- **Pendiente.** El trabajo ordinario de `codex-cli` espera la disposición del Coordinator sobre F6-OBS-03, la regresión de shell de la CLI
  0.162.0-alpha.2 (§93).

## 95. Decisiones §60: FX-04a / C-25a = PASS acreditado; evaluación de la ruta `cmd.exe` (F6-OBS-03) para FX-02

- **FX-04a / C-25a = PASS.** Acreditado sobre B3 tras comprobar en la evidencia custodiada sus cuatro condiciones: 23/23, terminación, aislamiento
  D.6 y orden de publicación (`707b4daa` → `68ce619a` → `013c1283`). Sin B4.
- **F6-OBS-03** ([evaluación](I-62-F6/FX-02/F6-OBS-03-cmd-route-evaluation.md)):
  - la ruta `cmd.exe` no cambia la receta 16.4;
  - la evidencia custodiada **no basta** para el contrato de ejecución de FX-02: la sonda 1 solo ejerció Git de lectura y lecturas de archivos, por
    una ruta que el modelo improvisó, y no las operaciones de VERIFY (`git diff --name-only` reproducible, hashes, mapa de cláusulas). La sonda 2
    no ejecutó nada;
  - hacer explícita la ruta es material y necesita una invocación medida nueva;
  - el Architect de FX-02 por `claude-cli` cambia V14 D.3, que fija `codex-cli`, y queda fuera de OD-3 y de CLAUDE-CLI-I62.
  Vías: V1 (A-n con la ruta explícita y medida del Controller, y el Architect por `claude-cli`), V2 (los dos roles por `claude-cli`), V3 (esperar una
  actualización de la CLI) y V4 (FX-02 UNVERIFIED). Propuesta de la preparación: V1 y, en paralelo, V3.
- **FX-02** sigue pendiente además de las disposiciones del grupo (b) (U-06..U-28 y U-68), que van en una sola solicitud con propuestas.

## 96. A-3: kit de la revisión formal por `claude-cli` custodiado antes del lanzamiento (R20261009T040327Z-58a1; decisiones §58, punto 3, y §60, punto 0)

- **Kit:** [R20261009T040327Z-58a1](I-62-architect-A-3/R20261009T040327Z-58a1/README.md). Contiene:
  - objeto exacto: A-3 `ea6721f7` y anexo `f410f7fc`, en el recibo `49288525`;
  - cierre de 27 archivos canónicos y 4 del run;
  - prompt de 23 252 bytes (`21230612…`) y esquema adaptado a A-3;
  - auditor v5.1-a3, con `selftest` 107/107 y 127 mutantes eliminados;
  - compuerta de transporte MEASURED sobre la caracterización `20d0de2c…`;
  - clon `D:
62-arch-a3` verificado (AllChecks) y run `D:
62-arch-a3-run` con cuatro archivos;
  - ensayo en seco de `launch.py`: 8 de 10 comprobaciones en Ok, ninguna bloqueante (la 4 y la 5 ejecutarían el binario).
- **Transporte:** `claude-cli` 2.1.293, el mismo binario medido (`8693c4a0…`), `claude-opus-5-5` xhigh, solo Read, Grep y Glob, tope de 7 200 s,
  una invocación sin reintento.
- **Consumo:** cubierto por la línea del Owner CLAUDE-CLI-I62 = A, la misma base que la re-revisión de A-2.
- **Estado:** custodiado y NO lanzado. A-3 no se modifica a partir de ahora.

## 97. A-3: intento 1 de la revisión = INVALID_LAUNCH sin consumo; kit del intento 2 custodiado antes de lanzarlo

- **Intento 1** (sesión `ae3590eb…`, 2026-10-09T05:18:56Z-05:19:06Z). La preflight pasó. El runtime terminó a los 10 s con un mensaje sintético
  (0 tokens, coste 0, `terminal_reason` api_error): «Failed to refresh OAuth token: another Claude Code process is refreshing it or exited
  mid-refresh». No hubo turno del modelo, ni lecturas, ni revisión. El run y el clon no cambiaron.
  - Acreditación: **INVALID_LAUNCH**, causa TRANSPORT_AUTH_REFRESH_CONFLICT
    ([accreditation.json](I-62-architect-A-3/R20261009T040327Z-58a1/attempt-1/accreditation.json)).
  - Las copias saneadas de `launch/` y de la transcripción están en el repositorio. Los originales, con identificadores de cuenta, quedan fuera
    del repositorio.
  - Causa probable: la comprobación `auth status` de la preflight dejó a medias el refresco del token justo antes de lanzar.
- **Intento 2:** el mismo kit, con `AttemptSeq` 2, `InvocationId` `I20261009T052403Z-58a1` y una sesión nueva `2e266dfe…`. Después de
  `auth status` hay una espera de 120 s, y si se repite el conflicto se clasifica sin reintento.
  - `selftest` 107/107; ensayo en seco sin bloqueos; `kit-manifest.json` `defda283…`.
  - Es el único intento que queda.

## 98. FX-02: solicitud única de desbloqueo con propuestas y candidata A-4 (decisiones §60, puntos 2 y 3)

- **Solicitud** ([FX-02-unblock-request-2026-10-09.md](I-62-F6/requests/FX-02-unblock-request-2026-10-09.md), blob `203d2887`). Contiene:
  - el estado de cada frontera de FX-02 en `c8d69fcb`;
  - una propuesta de la preparación (no es decisión) para cada disposición pendiente del grupo (b): U-06..U-28, U-68 y U-71 (b);
  - un bloque de respuesta prerrellenado para pegar;
  - la secuencia mínima ejecutable.
  U-66 queda resuelta por hecho (B3 sobre la instantánea congelada; FX-04a cerrada) y U-71 (a), decidida por §58.
- **Hallazgo.** Aunque se resuelva F6-OBS-03, el texto congelado no deja llegar a VERIFIED en FX-02, por tres huecos:
  - H-1: aceptación de los bindings del Worker y del Controller de verificación dentro de la ventana;
  - H-2: independencia REQUIRED de una candidata no arrancada;
  - H-3: presupuesto de una corrección.
- **Candidata A-4** ([I-62-A-4.md](../../initiatives/I-62-A-4.md), blob `27ffa26b`; [paquete](../../initiatives/I-62-architect-package-A-4.md), blob
  `d314afeb`). MATERIAL, solo para FX-02 / topología A:
  - A4-1: ruta de shell del Controller, explícita y medida con una invocación;
  - A4-2: Architect de FX-02 por otro adapter elegible;
  - A4-3: aceptación preautorizada de bindings dentro de la ventana;
  - A4-4: independencia de una candidata no arrancada;
  - A4-5: tope de una corrección; separable.
  Las guardas mecánicas de A-4 están pendientes. Sin revisión formal y sin aplicar.
- **Autorizaciones del Owner que A-4 necesitaría para aplicarse.** Dos líneas de consumo, con efecto solo tras el acuerdo de A-4 y la disposición
  del Coordinator:
  - `A4-SONDA-CONSUMO`: una sonda read-only de `codex-cli` fuera de A2-P2;
  - `A4-CLAUDE-FX02-CONSUMO`: `claude-cli` como Architect de FX-02.
  A4-3 a A4-5 no piden consumo nuevo.

## 99. A-3: revisión formal por `claude-cli` (intento 2) = AGREED, auditor ACCREDITED

- **Revisión** R20261009T040327Z-58a1, intento 2. Sesión `2e266dfe…`, de 05:40:38Z a 05:53:43Z, salida 0, sin fallo de transporte, 48 turnos
  (42 Read, 4 Grep).
- **Veredicto del Architect independiente: AGREED.** 0 REQUIRED; 4 OPTIONAL (A62-A3-O1..O4); `IfAgreed` todo en true, incluida «sin
  decisión del Owner».
- **Auditor v5.1-a3: ACCREDITED,** con 0 motivos y 47 llamadas
  ([registro](../../initiatives/I-62-architect-review-A-3.md); [output.json](I-62-architect-A-3/R20261009T040327Z-58a1/output.json),
  [audit.json](I-62-architect-A-3/R20261009T040327Z-58a1/audit.json),
  [runtime-evidence.json](I-62-architect-A-3/R20261009T040327Z-58a1/runtime-evidence.json)).
- **Clon** sin cambios tras la corrida. La transcripción y el `stdout.jsonl` no se versionan; sus SHA-256 están en `runtime-evidence.json`.
- **Siguiente:** veredicto del Coordinator sobre la A-3 exacta (blob `ea6721f7`). A-3 no se modifica antes.

## 100. A-3 AGREED (decisiones §61): acuerdo append-only; A-4 a revisión formal; FX-02 por grupos

- **Acuerdo.** Architect AGREED (R20261009T040327Z-58a1, ACCREDITED) y Coordinator AGREED sobre la A-3 exacta (blob `ea6721f7…`; publicación
  `91e29886`; revisión custodiada en `a23a093e`). Queda registrado en las decisiones (§61), en esta evidencia y como anexo del
  [registro de la revisión](../../initiatives/I-62-architect-review-A-3.md). `docs/initiatives/I-62-A-3.md` no cambia y no se aplica a producción.
  O1..O4 se aceptan como opcionales no bloqueantes; O4 queda como deuda de cobertura de C-43.
- **Siguiente.**
  - Guardas mecánicas de A-4 (blob `27ffa26b…`): comparación del delta con V14 y A-2, y superficies fuera de alcance.
  - Después, kit y revisión formal por `claude-cli` lanzada automáticamente.
  - En paralelo, la clasificación en cinco grupos de las decisiones de la solicitud de FX-02.
  - Ninguna sonda ni A2 antes del acuerdo de A-4 y de las autorizaciones del Owner.

## 101. A-4: guardas mecánicas y corrección previa a la revisión (decisiones §61, punto 2)

- **Guardas** [a4-guards.py](I-62-A4/a4-guards.py) (G1 literales, cabecera y privacidad; G2 rutas; G3 blobs congelados; G4 C-20b; G5 delta frente a
  V14 y A-2), con un `self-test` de 66/66 mutaciones detectadas ([a4-selftest.json](I-62-A4/a4-selftest.json)).
- **Primera ejecución, sobre la publicación `4710084b`** (base `e6fe2e08`):
  - **FAIL solo en G1:** la pregunta Q-A4-10 citaba «sin aceptación fingida», y la fuente (AUTOMATION_PLAN L1122) dice «Sin aceptación fingida».
    Es una diferencia de mayúscula fuera de §2 y de las cinco reglas.
  - G2 a G5 en PASS: las cinco reglas son un añadido puro de 90 líneas tras el último párrafo de A-2 y solo cambian el título, el Anexo D y D.3;
    cada regla abre con su cláusula de alcance a FX-02; las 23 menciones de otros escenarios o reglas son de conservación, negación o
    restricción; la fila de topes no cambia; ninguna regla menciona A-3.
- **Corrección antes de la revisión** (A-4 es una candidata sin revisar):
  - en A-4, dos líneas: la cita de Q-A4-10 al literal de la fuente, y la cabecera, que ahora dice «A-3 AGREED (decisiones §61, punto 1; blob
    `ea6721f7…`)»;
  - en el paquete, la línea del blob del borrador y la del estado de A-3.
  - Blobs nuevos: A-4 **`7d863219`** (antes `27ffa26b`) y paquete `085f30f6` (antes `d314afeb`). Las cinco reglas no cambian (G5).
- La ejecución de las guardas sobre este commit se custodia en el siguiente. La revisión formal se lanza sobre `7d863219` cuando pasen.
