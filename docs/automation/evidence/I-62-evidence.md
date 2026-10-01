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
