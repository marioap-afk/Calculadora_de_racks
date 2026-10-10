# ccli-meas — medición única de `claude-cli` para FX-02 (operaciones 5, 7, 8 y 9)

> I-62, plano a. Paquete **preparado y no ejecutado** (decisiones §66, punto 6: «preparar la medición de las operaciones 5, 7, 8 y 9 sin
> ejecutarla; pedir al Owner `CLAUDE-CLI-MEDICION-FX02` cuando exista un paquete exacto y vigente»). Quien lo preparó no invocó claude en
> ningún momento: ni `--version` ni `auth status`.
>
> Ubicación: `%TEMP%\claude\D--Documentos-Codex-Calculadora-de-racks\e5124bf0-ed19-48a1-97ba-574baf9d33a2\scratchpad\i62\day3\ccli-meas\`.
> Identidad exacta del paquete: **`MANIFEST.json` SHA-256 `e67bd195f4ebfab61cdf9bda6c104ae7ab303cd9e13b2655e8be97cadd586814`** (14 archivos;
> `SHA256SUMS.txt` lista todos). README, `SHA256SUMS.txt` y `dry-run-report.*` quedan fuera del manifiesto: no determinan la corrida.

## 1. Propósito

El análisis `docs/automation/evidence/I-62-F6/FX-02/s65/claude-cli-architect-ops.md` (blob `09808dc8`) concluye que el Architect de FX-02 por
`claude-cli` necesita las operaciones 2-9 del adapter (AUTOMATION_PLAN 16.19) y que faltan **5** (observar el resultado con el contrato
canónico `rackcad-architect-review-result/v1`), **7** (confirmar la terminación de todo el árbol), **8** (clasificar procesos) y **9**
(declarar la huella). Este paquete las mide en **una sola invocación read-only, sin reintento**, en un clon temporal de lectura del fixture.
No acepta el resultado de la revisión, no edita el descriptor ni el catálogo y no amplía OD-3, `CLAUDE-CLI-I62` ni `A4-CLAUDE-FX02-CONSUMO`.

## 2. Contenido

| Archivo | Papel |
|---|---|
| `run-constants.json` | RunId `R20261010T012414Z-d6d8`, InvocationId, session id `ee381259-e0ca-4806-b264-09aa32783f39`, binario, flags, clon, tiempos, lista cerrada de procesos, nombres de variables de referencia (corrida A-4) |
| `gates.template.json` | Línea exacta del Owner (SHA-256 `608ba443…`) y plantilla del marcador del Coordinator; resto de puertas |
| `launch.py` | Preflight (G1-G11) y el ÚNICO lanzamiento; `--dry-run` = solo preflight, sin ejecutar el binario y sin escribir |
| `sampler.ps1` | Muestreador CIM (`Get-CimInstance Win32_Process`, 1 s) del árbol; modo `Once` para las instantáneas de G4 y de S1/S2/S3. Configuración por stdin |
| `proctree.py` | Fuente rápida: Toolhelp32 cada 100 ms + handles conservados de cada miembro (identidad, hora y código de salida exactos) |
| `ccli_common.py` | Puertas puras, huella (SHA-256 y **nombres** de claves), clasificación, saneado y barrido de fugas |
| `schemacheck.py` | Validador stdlib del subconjunto 2020-12 del esquema canónico (falla cerrado) + Test-Json de PowerShell 7 |
| `audit.py` | Comparación/auditoría: veredicto por operación, custodia saneada y barrido de fugas |
| `make_clone.py` | Crea y verifica el clon `D:\r62-fixture\tmp-ccli-meas` |
| `dryrun.py` | Ensayo en seco (23 pasos) |
| `prompt.md` | Texto por stdin (5 708 bytes, SHA-256 `928e950c…`): revisión breve del contrato T1 con Glob, Grep y Read y una llamada StructuredOutput |
| `architect-review-result.v1.schema.json` | Copia VERBATIM del esquema canónico (SHA-256 `9f4639da…`, blob `e7f5747b…`) |
| `schema-source.json` | Origen del esquema: worktree de I-62 en `6d626794` (introducido en `42115503`) y fixture en `d30fb6a9`, mismo blob |
| `binding-stub.json` | Objeto exacto al que apunta `ReviewerBinding.Sha256` (`025a971a…`); no es un binding del protocolo |

## 3. La invocación

- **Binario:** `%APPDATA%\Claude\claude-code\2.1.293\83cb0bd7fed4\claude.exe`, SHA-256 `8693c4a02dde7441d0066ede68af8ddfc408bb982d77e12b506286268224e6fa`,
  Authenticode de «Anthropic, PBC». Con 2.1.295 instalado al lado, el paquete fija 2.1.293 y **se niega** si falta o si su hash cambia (también
  justo antes del `Popen`).
- **Argumentos:** los flags medidos `-p --model claude-opus-5-5 --effort xhigh --output-format stream-json --verbose --safe-mode --strict-mcp-config
  --no-chrome --tools Read,Grep,Glob --permission-mode dontAsk --permission-prompts none` + `--session-id <uuid fijado>` + `--json-schema <esquema>`
  (plantilla de la corrida C1 de la caracterización, sin `--add-dir`). Línea de órdenes: 13 205 caracteres.
- **Esquema:** el canónico **VERBATIM**, pasado como `json.dumps(json.load(f), separators=(',', ':'), ensure_ascii=True)` (mismo documento JSON;
  SHA-256 del argumento `de075bac…`). Ver el riesgo R1.
- **Prompt** por stdin; **cwd** = el clon; entorno heredado de la sesión que lanza (igual que en las revisiones medidas de A-2, A-3 y A-4).
- **Objeto revisado:** `docs/automation/decisions/FX-U1-T1.gate-contract.json` en `d30fb6a9` (blob `628d89af…`) de `D:\r62-fixture\fixture-origin.git`.
- **Una sola ejecución del binario.** No hay `--version` ni `auth status`: la versión la dan el SHA-256 fijado y el `init`; así tampoco se
  provoca el conflicto de renovación OAuth que dejó en INVALID_LAUNCH el intento 1 de la revisión de A-3.
- **Tope:** 1 800 s; si vence, `taskkill /T /F` del árbol y terminación de cada identidad por handle.

## 4. Criterios por operación (los aplica `audit.py`; DEMONSTRATED solo si se cumplen todos)

Antes: **ABORTED_BEFORE_LAUNCH** (el muestreador no arrancó o el binario cambió: el binario no llega a ejecutarse), **INVALID_LAUNCH**
(«Failed to refresh OAuth token» con 0 tokens: `TRANSPORT_AUTH_REFRESH_CONFLICT`) o **LAUNCHED**. Fuera de LAUNCHED, las cuatro quedan
NOT_DEMONSTRATED y no hay reintento bajo esta autoridad.

**Op 5 — observar el resultado con el contrato canónico.** R5.1 la CLI aceptó el esquema (ningún «--json-schema is not a valid JSON Schema»;
`init` con `StructuredOutput` en `tools`); R5.2 un solo `result`, `subtype` success, `is_error` false, `session_id` = el fijado; R5.3
`structured_output` presente y objeto; R5.4 igual al input de la última llamada StructuredOutput; R5.5 válido contra el esquema canónico con el
validador stdlib **y** con Test-Json, que deben coincidir. El eco de los valores fijos del prompt se registra sin bloquear.

**Op 7 — confirmar la terminación de todo el árbol.** R7.1 corrida representativa (`init` y al menos una llamada Read, Grep o Glob); R7.2 salida
de la raíz observada (código de salida) e identidad terminada por handle; R7.3 en el último control (S1 a los 10 s, o S2 a los 60 s si S1 tenía
restos) ninguna identidad del árbol viva, por handles **y** por CIM, y terminación **natural** (sin tope vencido ni restos matados); R7.4 ningún
huérfano vivo; R7.5 cobertura: hueco máximo de la fuente rápida ≤ 0,5 s y de CIM ≤ 2,5 s, sin errores del muestreador; R7.6 toda identidad vista
por CIM está en la fuente rápida.

**Op 8 — clasificar procesos.** R8.1 = R7.1; R8.2 raíz observada, clase `launched-root` y ruta = binario fijado; R8.3 todo miembro con una clase de
la tabla cerrada y ninguno `unclassified`; R8.4 coherencia entre fuentes (identidad, nombre y ruta); R8.5 ningún proceso **ajeno** al árbol con la
ruta del clon en su línea de órdenes durante la ventana; R8.6 = R7.5. Tabla cerrada (primera regla que aplica):

| Clase | Regla |
|---|---|
| `launched-root` | identidad (PID + CreationDate) del proceso lanzado |
| `cli-self-child` | ExecutablePath = binario fijado, distinto de la raíz (p. ej., un ripgrep embebido que se relanza con el mismo ejecutable) |
| `cli-bundled-helper` | ExecutablePath bajo `%APPDATA%\Claude\claude-code\` |
| `search-helper` | `rg.exe` |
| `vcs-helper` | `git.exe`, `git-*.exe` |
| `shell` | `cmd.exe`, `powershell.exe`, `pwsh.exe`, `bash.exe`, `sh.exe`, `wsl.exe` (se listan además como inesperados con solo Read/Grep/Glob) |
| `console-host` | `conhost.exe`, `openconsole.exe` |
| `runtime` | `node.exe`, `bun.exe` |
| `other` | nombre legible no cubierto arriba (p. ej., `reg.exe` si la CLI consulta las claves de política) |
| `unclassified` | ni nombre ni ruta legibles |

Pertenencia al árbol: PPID = un miembro conocido y CreationDate del hijo ≥ la del padre (guarda contra la reutilización del PID; además, mientras
el handle está abierto el PID no puede reutilizarse). Huérfano (16.4 adaptada): proceso de la lista cerrada (la de 16.4 más `rg.exe`, `cmd.exe`,
`sh.exe`, `conhost.exe`, `bun.exe`) creado en la ventana, fuera del árbol, cuyo padre no existe o tiene una CreationDate posterior.

**Op 9 — declarar la huella.** R9.1 `init` con `claude_code_version` = 2.1.293 y `session_id` = el fijado; R9.2 `CLAUDE_CONFIG_DIR` ausente; R9.3
huella antes y después, **estable** (mismos SHA-256 y existencia en todos los candidatos de usuario, proyecto y política, y mismos nombres de
variables); R9.4 declaración derivable: `CONFIG_FILES` (SHA-256 + nombres de claves de los archivos presentes, más los nombres de las variables
heredadas) o `NINGUNA` (todo ausente; exige además la aceptación registrada del Coordinator, 16.19). Candidatos: `%USERPROFILE%\.claude\settings.json`
y `settings.local.json`; `<clon>\.claude\settings.json` y `settings.local.json`; `%ProgramFiles%\ClaudeCode\managed-settings.json`, su carpeta
`managed-settings.d` y `managed-mcp.json`; los equivalentes heredados de `%ProgramData%`; las claves `HKLM`/`HKCU\SOFTWARE\Policies\ClaudeCode`
(rutas de política tomadas de las cadenas del propio binario); `%USERPROFILE%\.claude.json` solo con metadatos (archivo de estado, se espera que
cambie, nunca se lee). Variables: nombres `CLAUDE*`/`ANTHROPIC*` y una lista fija (proxies, `NODE_OPTIONS`…), comparados con los de la corrida A-4.

**Qué se puede demostrar y qué no (op 9).** Sí: qué archivos existen, su SHA-256, los nombres de sus claves, que no cambian durante la corrida, los
nombres de las variables heredadas y la configuración efectiva que declara el `init` (`tools`, `mcp_servers`, `permissionMode`, `plugins`, agentes,
`apiKeySource`). Hoy (ensayo) solo existe el archivo de usuario, con las claves `agentPushNotifEnabled`, `autoUpdatesChannel`, `enabledPlugins`,
`extraKnownMarketplaces`, `hooks`, `skipWorkflowUsageWarning`, `statusLine` y `theme` (y sus subclaves): la declaración esperada es `CONFIG_FILES`
con ese archivo. Según la ayuda del binario 2.1.293, `--safe-mode` desactiva las personalizaciones (plugins instalados, ganchos, MCP…) pero la
política gestionada sigue aplicando y los permisos funcionan con normalidad, así que el archivo de usuario sigue siendo candidato a regir; la
coincidencia entre los plugins del `init` y la clave `enabledPlugins` es evidencia por nombres. No: qué claves o valores aplicó realmente la CLI
(los valores nunca se leen), el efecto de los valores de las variables heredadas, ni que la CLI no consulte otro archivo (no hay trazas de acceso
sin privilegios de administrador).

## 5. Puertas (la preflight se niega, código 2, sin escribir nada)

| Puerta | Condición |
|---|---|
| G1 | El archivo de decisiones de I-62 **en HEAD** del worktree contiene como líneas completas exactas la línea del Owner (§8) y el marcador del Coordinator con el SHA-256 de `MANIFEST.json` (§9) |
| G2 | Binario 2.1.293 presente, SHA-256 exacto, Authenticode válida de Anthropic |
| G3 | Clon `D:\r62-fixture\tmp-ccli-meas`: HEAD `d30fb6a9`, solo `main`, sin remoto, limpio con ignorados, blob del objeto, sin `.claude/`, sin enlaces, solo la historia de HEAD, `core.autocrlf=false` |
| G4 | Ningún proceso con el session id del paquete en su línea de órdenes (ningún claude lanzado por el paquete vivo) y ninguno ajeno con la ruta del clon |
| G5 | Un solo uso: `run/` no existe. El lanzamiento crea `run/` y `run/ONE-SHOT.marker`, que **no se borran nunca** |
| G6 | `MANIFEST.json` íntegro; ningún archivo no listado ni `__pycache__` |
| G7 | `CLAUDE_CONFIG_DIR` ausente |
| G8 | Sin transcripción con el session id y sin carpeta de proyecto `D--r62-fixture-tmp-ccli-meas` en `%USERPROFILE%\.claude\projects` |
| G9 | Línea de órdenes < 32 000 caracteres |
| G10 | Muestreador CIM operativo |
| G11 | Argumentos = plantilla medida; argumento del esquema = el documento canónico |

## 6. Procedimiento (sesión principal, desde PowerShell, tras G1)

```powershell
$p = "$env:TEMP\claude\D--Documentos-Codex-Calculadora-de-racks\e5124bf0-ed19-48a1-97ba-574baf9d33a2\scratchpad\i62\day3\ccli-meas"
python -I -B "$p\make_clone.py"            # crea y verifica D:\r62-fixture\tmp-ccli-meas (código 0)
python -I -B "$p\launch.py" --dry-run      # BlockingChecks debe ser []
# ninguna orden de claude (ni auth status) en los 120 s previos; la sesión no opera mientras dura la corrida
python -I -B "$p\launch.py"                # ÚNICO lanzamiento (minutos); escribe run\launch\*
python -I -B "$p\audit.py"                 # run\audit.json y run\custody\ (saneado, con SHA256SUMS.txt)
```

Después: si `CustodyReady` = true, custodiar **solo** `run\custody\*` (p. ej., en `docs/automation/evidence/I-62-claude-cli/R20261010T012414Z-d6d8/`),
nunca `stdout.jsonl`, `proc-cim.jsonl` ni la transcripción (contienen rutas del perfil y el contexto de sesión; se custodian sus SHA-256). Borrar el
clon `D:\r62-fixture\tmp-ccli-meas` tras la auditoría. **No borrar `run\`**. El descriptor y el catálogo solo se actualizan después, con
disposición propia y en el orden de §4 del análisis.

## 7. Riesgos

- **R1 — rechazo del esquema VERBATIM (alto).** Análisis estático del binario 2.1.293 leído como datos (DR-17, sin ejecutarlo): la ruta de
  `--json-schema` valida el esquema con la clase `Ajv` por defecto, cuyo meta-esquema es draft-07, y llama a `validateSchema`; el canónico
  declara `"$schema": "https://json-schema.org/draft/2020-12/schema"`, y esa llamada busca ese meta-esquema, no lo encuentra y lanza «no schema
  with key or ref», que la CLI convierte en «Error: --json-schema is not a valid JSON Schema: …» y termina **antes de llamar al modelo**
  (INFERENCIA; offsets en `dry-run-report.json`). Consecuencia probable: op 5 NOT_DEMONSTRATED (R5.1, la CLI no acepta el canónico tal cual) y 7, 8
  y 9 NOT_DEMONSTRATED por corrida no representativa (sin `init`), con la autoridad consumida y casi sin consumo de cuota. El paquete conserva
  el esquema VERBATIM por orden: medir la aceptación es el objetivo. Alternativa **no aplicada**: un renderizado sin `$schema` (las demás palabras
  clave pertenecen al vocabulario de draft-07 y se compilarían), validando después contra el canónico VERBATIM; sería otro paquete, con su
  disposición, y quizá una línea del Owner que lo nombre.
- **R2 — auto-actualización del binario.** La app de escritorio ya instaló 2.1.295. Si borra 2.1.293 o cambia su hash, G2 (y la segunda medición
  antes del `Popen`) se niega; toda la medición de `claude-cli` queda STALE y habría que medir el binario nuevo (regla de A-3).
- **R3 — renovación OAuth.** El paquete no ejecuta `auth status`, pero otra sesión puede estar renovando el token. Un fallo rápido con «Failed to
  refresh OAuth token» y 0 tokens se clasifica `TRANSPORT_AUTH_REFRESH_CONFLICT` → INVALID_LAUNCH; sin reintento; decide el Coordinator.
- **R4 — entorno heredado.** La CLI hereda de la sesión que lanza unas 30 variables `CLAUDE*`/`ANTHROPIC*` (entre ellas `ANTHROPIC_BASE_URL`,
  `CLAUDE_CODE_SDK_HAS_HOST_AUTH_REFRESH`, `CLAUDE_EFFORT`); forman parte de la configuración efectiva y solo se custodian sus nombres. El
  ensayo coincide con los nombres de la corrida A-4 (0 añadidas, 0 ausentes).
- **R5 — límites del muestreo.** Un proceso que vive menos de ~100 ms y no deja descendientes vivos puede no observarse; sus nietos los cubre la
  regla de huérfanos. Las instantáneas CIM tardan entre 3 y 8 s (arranque de PowerShell): por eso Residual = vivo según cualquiera de las dos fuentes.
- **R6 — divergencia de validadores.** Test-Json (.NET) acepta un salto de línea final antes de `$`; el validador stdlib sigue ECMA-262 y no. Si
  discrepan, R5.5 falla (conservador) (DR-13).
- **R7 — actividad ajena.** Si otra sesión lanza procesos con la ruta del clon o deja huérfanos de la lista cerrada durante la ventana, 7 u 8
  quedan NOT_DEMONSTRATED con los PID y nombres listados: la sesión principal no debe operar durante la corrida.
- **R8 — HEAD del worktree.** G1 lee las decisiones en HEAD: las líneas deben estar **commiteadas** antes del lanzamiento.

## 8. Línea exacta para el Owner (sin cambios respecto del análisis; no se halló error de hecho)

```text
CLAUDE-CLI-MEDICION-FX02 = A (1 invocación read-only de claude-cli 2.1.293, binario 8693c4a02dde7441d0066ede68af8ddfc408bb982d77e12b506286268224e6fa, celda claude-opus-5-5 xhigh, solo Read, Grep y Glob, en un clon temporal de lectura del fixture, para medir las operaciones 5 (esquema canónico de la revisión del Architect), 7, 8 y 9 de su descriptor; sin reintento; sin escritura; sin lectura de credenciales ni de valores de configuración; no acepta su resultado ni amplía OD-3, CLAUDE-CLI-I62 ni A4-CLAUDE-FX02-CONSUMO)
```

Coherencia comprobada: el paquete ejecuta el binario una sola vez (sin `--version` ni `auth status`), con solo Read, Grep y Glob (el runtime añade
StructuredOutput como canal de salida, como en las revisiones anteriores), en un clon temporal, sin reintento, y la huella lee solo nombres de claves.

## 9. Qué debe nombrar la disposición del Coordinator

La disposición debe contener, como **línea completa y literal** (la comprueba G1), el marcador:

```text
CCLI-MEDICION-FX02-DISPOSICION = AUTORIZADA (paquete ccli-meas, MANIFEST.json SHA-256 e67bd195f4ebfab61cdf9bda6c104ae7ab303cd9e13b2655e8be97cadd586814, esquema VERBATIM, sin reintento)
```

y nombrar: el paquete por el SHA-256 de `MANIFEST.json`; el RunId `R20261010T012414Z-d6d8` y el session id `ee381259-e0ca-4806-b264-09aa32783f39`;
el binario 2.1.293 `8693c4a0…`; el clon `D:\r62-fixture\tmp-ccli-meas` de `fixture-origin.git` en `d30fb6a9` y el objeto (blob `628d89af…`); el
esquema canónico (blob `e7f5747b…`) en modo VERBATIM **con la aceptación expresa del riesgo R1** (o, si no lo acepta, pedir el paquete variante);
una sola invocación sin reintento; el destino de la custodia saneada; y que el resultado no se acepta ni autoriza editar el descriptor o el catálogo.
Cualquier cambio en un archivo del manifiesto cambia ese SHA-256 y exige otra disposición.

## 10. Ensayo en seco

`dryrun.py` (23/23 en Pass; `dry-run-report.md` y `dry-run-report.json`, saneados): guardas anti-claude y un rastreador sobre el propio ensayo
(ningún descendiente fue claude); clon de ensayo `D:\r62-fixture\tmp-ccli-dry` creado, verificado y borrado; preflight real que bloquea **solo**
G1; G1-G11 con casos positivos y negativos (procesos señuelo para G4, matriz de 9 casos para G1, binario falso, clon alterado, `run/`, manifiesto
alterado, `CLAUDE_CONFIG_DIR`, transcripción previa); árbol natural (cmd → ping) con 7 y 8 DEMONSTRATED; árbol con un resto vivo (matado y
confirmado: KILLED_RESIDUAL); tope vencido (TIMEOUT_TREE_KILLED); huérfanos sintéticos; 11 casos de esquema con los dos validadores; cuatro streams
sintéticos (éxito, rechazo del esquema, conflicto OAuth, sin result); saneado y fugas; huella (claves solo del archivo de usuario); análisis
estático de R1; `audit.py` de extremo a extremo sobre tres corridas sintéticas; barrido de fugas del paquete; limpieza final.
