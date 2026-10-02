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
