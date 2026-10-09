# I-62 A-3 — Anexo: `MATERIAL_ADAPTER_FINGERPRINT` aplicado a `codex-cli` (no normativo)

```text
Anexo de:      docs/initiatives/I-62-A-3.md (regla 11 del delta y §3.9), candidata. No forma parte del delta ni lo modifica
Naturaleza:    motivo y clasificación saneada. No acepta huellas, no autoriza consumo ni sondas, no cambia la receta de AUTOMATION_PLAN 16.4,
               no define ni adopta ningún verificador ni ninguna clase de cambios aceptables (A-3 no los define: regla 11, a y c) y no lee
               valores ni autoriza leerlos. La clasificación y las familias son de la investigación; el §7 es una propuesta no normativa para la
               A-n que ordene la decisión del Owner OD-2-MAT, que tendría que cumplir los requisitos de la regla 11, c)
Base:          b0884216 (worktree de preparación, solo lectura). Las líneas citadas son de los blobs de ese commit
Fuentes:       investigación docs/automation/evidence/I-62-A3/maf-codex.md (estado de cada hecho: VERIFIED-SRC, VERIFIED-DOC, VERIFIED-LOCAL,
               INFERRED o UNVERIFIED, con sus URLs) y od2-evolution.md; archivo saneado de nombres codex-cfg-keynames-6518EFAB.json (107
               nombres, sin valores, rutas de proyecto redactadas; SHA-256 a349bea7acec96a99f0c2b054c740adfec6f6fac04f87c62655150b7b28360f2),
               de custodia local y no publicado: la clasificación por nombre de §3 no se puede reproducir desde el commit publicado, y el
               recuento de 107 nombres sale solo de docs/automation/evidence/I-62-F6/OD-2/R20261008T2200Z-codex-passive/result.json
               (blob bda544f0, KeyNamesCount y las 9 claves cambiadas)
Estado Codex:  binario 3553cd6e7df5a093d8cb8301cd8088a57e0971aba71ddbe0e67f7f44a15cdf68, app 26.1002.7124.0, huella 6518EFAB… estable desde
               2026-10-07T20:05Z; codex-cli en STOP P-01 (huella vigente sin aceptar por OD-2)
Privacidad:    ningún valor de configuración; rutas con %USERPROFILE% y %LOCALAPPDATA%
```

Este anexo lleva los nombres de producto que el delta no puede llevar (A-3 §3.8: neutralidad del delta). Las etiquetas de estado son las que fijó
la investigación; este anexo no las vuelve a verificar. El código del proveedor se leyó en el tag `rust-v0.160.1` (última CLI medida, evidencia
§70) y en `main` del 2026-10-08. **El binario vigente no tiene versión observada**: que esas conductas valgan para él es UNVERIFIED hasta una
observación autorizada de `codex --version`.

## 1. Hechos medidos: tres actualizaciones automáticas

| Actualización | Huella (bytes) | Binario / app | Claves cambiadas, por clave | Fuente |
|---|---|---|---|---|
| 1 | `9002E854…` → `723A6898…` (4 775 → 4 775) | `bin\8aaf1547b825b104` → `bin\5ea220ae823df3d7`; `26.930.3930.0` → `26.930.7945.0` | **no demostrable**: no había instantánea por clave; consta la etiqueta del binario nuevo en `CODEX_CLI_PATH` y al menos otro valor | evidencia L2486-2488 |
| 2 | `723A6898…` → `9EA26634…` (4 775 → 4 777) | `bin\979a96ce184041d1`; `26.1002.6548.0` | 9 claves, todas de `mcp_servers.node_repl` y `notify`; estructura igual | evidencia L2568-2570; paquetes L199 |
| 3 | `9EA26634…` → `6518EFAB…` (4 777 → 4 777) | `3553cd6e…`; `26.1002.7124.0` | «las mismas 9 claves» | evidencia L2879-2882 |

- **Corrección de una premisa de trabajo** (sin fuente custodiada; decisiones §57, punto 4, solo dice «cada actualización modificó
  `config.toml`», decisiones L1115): que las tres actualizaciones cambiaran las mismas 9 claves solo está medido para la 2 y la 3. Para la 1, la
  evidencia dice: «No se puede probar cuál sin el archivo anterior, que nunca se guarda» (evidencia L2488).
- **Calendario de la 3.** El binario se escribió el 2026-10-07T11:57Z y `config.toml` a las 20:05Z (evidencia L2879-2880): unas 8 h con el binario
  nuevo y la configuración anterior. Es el caso de observación a medio completar que la regla 1, d) no admite como sucesor y cuya estabilidad
  registra la evidencia de la regla 11, b).
- **Las 9 claves:** `mcp_servers.node_repl.command`; `mcp_servers.node_repl.env.` `BROWSER_USE_CODEX_APP_VERSION`, `CODEX_CLI_PATH`,
  `NODE_REPL_NODE_MODULE_DIRS`, `NODE_REPL_NODE_PATH`, `NODE_REPL_TRUSTED_CODE_PATHS`, `NODE_REPL_TRUSTED_SERVICES` y
  `SKY_CUA_NATIVE_PIPE_DIRECTORY`; y `notify` (paquetes L199). Solo cambian valores; ningún nombre se añade ni se elimina.

## 2. Qué neutraliza la receta 16.4 y qué no

La receta (AUTOMATION_PLAN L541-543) fija `-s read-only`, `-m` y `-c model_reasoning_effort` y hereda el `service_tier` del Owner. Según la
investigación (maf-codex.md §3):

| Superficie | Efecto en `codex exec` | Estado |
|---|---|---|
| sandbox de comandos | `-s read-only` gobierna la ejecución de comandos; el tráfico de apps y conectores queda fuera del proxy de comandos | VERIFIED-DOC |
| aprobación | `exec` la fija en `never` salvo que el revisor resuelto sea AutoReview; el registro de la sonda de OD-2b lo confirma (paquetes L182) | VERIFIED-SRC; VERIFIED-LOCAL |
| modelo y effort | `-m` y `-c` tienen la precedencia más alta | VERIFIED-DOC; VERIFIED-LOCAL |
| `service_tier` | `exec` no lo fija: rige el de `config.toml` (16.4 lo hereda a propósito) | VERIFIED-SRC |
| servidores MCP | `exec` arranca los configurados de forma anticipada y expone sus herramientas al modelo sin filtrar por origen de sesión, sandbox ni aprobación | VERIFIED-SRC |
| llamadas de `node_repl` | si se admiten con aprobación `never`, y si el sandbox del sistema envuelve al servidor | UNVERIFIED |
| `notify` | se lanza tras la respuesta final de cada turno, sin esperar, sin sandbox visible, con `cwd`, los mensajes de entrada y el último mensaje del asistente como argumento | VERIFIED-SRC, salvo el enlace de la clave de configuración con la lista interna de argumentos (INFERRED) |
| procesos de 16.4 | un `notify` que sobreviva a `codex.exe` lleva la ruta del worktree en su línea de órdenes y podría disparar P-02 o P-12 | INFERRED; ninguna de las cuatro sondas custodiadas registró P-02 |
| arranque real de `node_repl` en las sondas pasadas | los flujos `--json` custodiados no tienen eventos MCP | UNVERIFIED |
| opciones no usadas | `--ignore-user-config`, `--ignore-rules` y `--strict-config` existen en `rust-v0.160.1` | VERIFIED-SRC; en el binario vigente, UNVERIFIED |

**Corrección de un registro anterior.** El paquete de OD-2d dice de esas claves: «la receta de solo lectura no depende de ellas» (paquetes L200).
Con lo anterior no se sostiene: configuran un servidor que `exec` arranca y un programa que `exec` lanza. Esa frase no sirve como razón de no
materialidad.

## 3. Clasificación de los 107 nombres

Reglas: la clase se asigna por la función de la clave en una corrida de la receta 16.4, no por quién la escribe ni porque el cambio «parezca
automático». NON_MATERIAL_FOR_RECIPE solo con una razón verificada y para el rango de versiones verificado; sin versión observada del binario, la
razón no se puede aplicar y la clave es UNKNOWN (criterio de la investigación: A-3 no define clases). Los nombres `<redactado>` no se pueden
clasificar desde el archivo saneado.

| Nombres | n | Clase | Razón | Estado |
|---|---|---|---|---|
| `[desktop]`, `[desktop.open-in-target-preferences]`, `[desktop.open-in-target-preferences.perPath]` y 6 claves `desktop.*` | 9 | NON_MATERIAL_FOR_RECIPE (condicional) | ajustes opacos de la app de escritorio; `exec` no los interpreta | VERIFIED-SRC; condicional a la versión |
| `[tui]`, `tui.screen_reader_detection_done` | 2 | NON_MATERIAL_FOR_RECIPE (condicional) | ajustes de la TUI; `exec` no usa la TUI | VERIFIED-SRC |
| `model`, `model_reasoning_effort` | 2 | NON_MATERIAL_FOR_RECIPE (condicional) | la receta pasa `-m` y `-c model_reasoning_effort` con precedencia máxima | VERIFIED-DOC y VERIFIED-LOCAL; condición: la receta los pasa siempre |
| `[features]`, `features.js_repl` | 2 | NON_MATERIAL_FOR_RECIPE (condicional) | bandera retirada (sin efecto) en el rango verificado | VERIFIED-SRC; fuera del rango o sin versión, UNKNOWN |
| `service_tier` | 1 | MATERIAL_CAPABILITY (consumo) | `exec` lo hereda; un cambio cambia el consumo | VERIFIED-DOC |
| `[windows]`, `windows.sandbox` | 2 | MATERIAL_SECURITY (sandbox) | implementación que hace cumplir `-s read-only`; OD-4 prohíbe cambiarla (decisiones L891) | VERIFIED-SRC |
| `[projects.<redactado>]` ×12 | 12 | MATERIAL_SECURITY (confianza de espacio de trabajo) | decide si cargan la configuración, los hooks y las reglas de la capa `.codex/` del proyecto | VERIFIED-DOC |
| `<redactado>` ×14 | 14 | **UNKNOWN** | nombres con ruta saneados; hipótesis: 12 `trust_level` de proyecto (serían MATERIAL_SECURITY) y 2 claves `perPath` (serían NON_MATERIAL) | UNVERIFIED |
| `[marketplaces.*]` ×2 y sus `source`, `source_type` | 6 | MATERIAL_SECURITY (origen de suministro de extensiones) | origen de los plugins | VERIFIED-DOC |
| `[plugins."…"]` ×16 y `plugins."…".enabled` ×16 | 32 | MATERIAL_CAPABILITY (extensiones habilitadas) | un plugin habilitado aporta servidores, skills y hooks; los conectores generan tráfico fuera del proxy de comandos | VERIFIED-SRC; dependencia de `node_repl` por nombre, INFERRED |
| `[mcp_servers.node_repl]`, `[mcp_servers.node_repl.env]` | 2 | MATERIAL_SECURITY | servidor arrancado por `exec`, con herramientas expuestas | VERIFIED-SRC |
| `mcp_servers.node_repl.command`, `.args`, `.env_vars` | 3 | MATERIAL_SECURITY | ejecutable, argumentos y entorno heredado del servidor | VERIFIED-DOC |
| `mcp_servers.node_repl.startup_timeout_sec` | 1 | MATERIAL_CAPABILITY | decide si el servidor llega a inicializar | VERIFIED-DOC |
| `env.CODEX_CLI_PATH`, `env.NODE_REPL_NODE_PATH`, `env.NODE_REPL_NODE_MODULE_DIRS` | 3 | MATERIAL_SECURITY | identidad del código que ejecuta el servidor | clase base VERIFIED; subclase INFERRED |
| `env.NODE_REPL_TRUSTED_CODE_PATHS`, `env.NODE_REPL_TRUSTED_SERVICES`, `env.NODE_REPL_UNTRUSTED_ENV_ALLOWLIST` | 3 | MATERIAL_SECURITY | fronteras de confianza del núcleo del servidor | clase base VERIFIED; subclase INFERRED |
| `env.SKY_CUA_NATIVE_PIPE`, `env.SKY_CUA_NATIVE_PIPE_DIRECTORY` | 2 | MATERIAL_SECURITY | canal con el componente nativo de uso del ordenador | clase base VERIFIED; subclase INFERRED |
| `env.CODEX_HOME` | 1 | MATERIAL_SECURITY | raíz de datos visible para el servidor | clase base VERIFIED; subclase INFERRED |
| `env.BROWSER_USE_AVAILABLE_BACKENDS`, `env.BROWSER_USE_TINYSKY_ENABLED`, `env.BROWSER_USE_CODEX_APP_BUILD_FLAVOR`, `env.BROWSER_USE_CODEX_APP_VERSION`, `env.NODE_REPL_NATIVE_PIPE_CONNECT_TIMEOUT_MS` | 5 | MATERIAL_CAPABILITY | backends, conmutadores, versión y tiempos | clase base VERIFIED; subclase INFERRED |
| `env.NODE_REPL_INSTRUCTIONS_USE_CASE_BROWSER`, `env.NODE_REPL_INSTRUCTIONS_USE_CASE_CHROME` | 2 | MATERIAL_CONTEXT_OR_OUTPUT | instrucciones que el servidor puede presentar al modelo | clase base VERIFIED; subclase INFERRED |
| `notify` | 1 | MATERIAL_SECURITY (y de contexto o salida) | programa externo sin sandbox tras cada turno, que recibe la entrada y la salida del modelo | VERIFIED-SRC salvo el enlace citado (INFERRED) |
| `[shell_environment_policy.set]`, `shell_environment_policy.set.NODE_REPL_TRUSTED_BROWSER_CLIENT_SHA256S` | 2 | MATERIAL_SECURITY | inyecta una variable en el entorno de todos los comandos del modelo | VERIFIED-DOC; semántica de la variable, INFERRED |

**Resumen (hoy, sin versión observada del binario):** 37 MATERIAL_SECURITY, 39 MATERIAL_CAPABILITY, 2 MATERIAL_CONTEXT_OR_OUTPUT y 29 UNKNOWN (los
14 `<redactado>` y 15 que serían NON_MATERIAL_FOR_RECIPE solo con la versión observada dentro del rango verificado): 107 nombres (39 secciones y 68
claves).

**Claves ausentes cuya aparición sería material** (hoy, cambio de estructura: UNKNOWN y P-01): `approval_policy`, `approvals_reviewer` (con
AutoReview, `exec` deja de forzar `never`), `sandbox_mode`, `sandbox_workspace_write`, `default_permissions`, `permissions`, `hooks`,
`model_provider(s)`, URLs base del proveedor, `instructions`, `developer_instructions`, `model_instructions_file`, `profile(s)`, `project_doc_*`,
`skills`, `memories`, `web_search`, `tools`, `apps`, cualquier `mcp_servers.<id>` nuevo y cualquier clave nueva en `[features]` o `plugins`.

## 4. Familias de seguridad, lanzamiento y confianza (clasificación de la investigación; requisito de A-3, regla 11, c, para la A-n)

| Familia neutral (requisito de la regla 11, c) | Nombres de `codex-cli` |
|---|---|
| sandbox | `[windows]`, `windows.sandbox` |
| política de aprobación o de permisos | hoy ausentes (`approval_policy`, `approvals_reviewer`, `sandbox_mode`, `permissions`, `default_permissions`): su aparición es cambio de estructura |
| confianza de los espacios de trabajo | `[projects.<redactado>]` y, si la hipótesis se confirma, sus `trust_level` |
| extensiones habilitadas | `plugins."…".enabled` |
| origen de suministro de extensiones | `[marketplaces.*]`, `source`, `source_type` |
| modo de consumo | `service_tier` |
| código lanzado (ejecutable, intérprete, módulos, argumentos o entorno de un servidor o programa lanzado; la salvedad de una variable que solo lleve una etiqueta de versión verificada es solo de la investigación: la regla 11, c) no la contiene) | `mcp_servers.node_repl.command`, `.args` y `.env_vars`; todas las variables de `mcp_servers.node_repl.env` (la investigación exceptuaría una que solo lleve una etiqueta de versión verificada, salvedad que no es requisito de la regla 11, c; entre ellas `CODEX_CLI_PATH`, `NODE_REPL_NODE_PATH`, `NODE_REPL_NODE_MODULE_DIRS` y `SKY_CUA_NATIVE_PIPE_DIRECTORY`); `notify`; `[shell_environment_policy.set]` y toda `shell_environment_policy.set.*` (entorno de todos los subprocesos que lanza Codex, maf-codex.md F-18) |
| fronteras de confianza de ese código | `env.NODE_REPL_TRUSTED_CODE_PATHS`, `env.NODE_REPL_TRUSTED_SERVICES`, `env.NODE_REPL_UNTRUSTED_ENV_ALLOWLIST`; `shell_environment_policy.set.NODE_REPL_TRUSTED_BROWSER_CLIENT_SHA256S` (por su nombre, una lista de clientes de confianza) |

A-3 no clasifica claves: con su regla 11, a), cualquier cambio de estas claves es un cambio de huella (P-01 y OD-2 exacta). La regla 11, c) exige a la
A-n que ordene OD-2-MAT que las claves que fijan el sandbox, la política de aprobación o de permisos, la confianza de los espacios de trabajo, las
extensiones habilitadas o su origen, o el modo de consumo, y las que designan código que el runtime ejecuta o lanza y las fronteras de confianza de
ese código, nunca sean acopladas a la versión ni no materiales, con cualquier semántica: una proyección igual acotaría el cambio de configuración,
pero no acreditaría el código nuevo ni lo haría inocuo. La pertenencia a una familia la decide la función de la clave en la receta, no la etiqueta de un mapa, y una clave
cuya función no esté verificada para la versión observada cuenta como dentro: por eso `env.BROWSER_USE_CODEX_APP_VERSION`, cuya función de etiqueta de
versión es INFERRED, también cuenta hoy como dentro. Las ocho familias de la tabla son requisito de la regla 11, c); solo la salvedad de la etiqueta
de versión de la fila «código lanzado» es de la investigación y no es requisito de A-3.

## 5. Las 9 claves cambiadas

| Clave | Clase | Qué cambia probablemente (INFERRED) |
|---|---|---|
| `mcp_servers.node_repl.command` | MATERIAL_SECURITY | ruta del ejecutable del servidor dentro de la instalación versionada |
| `env.BROWSER_USE_CODEX_APP_VERSION` | MATERIAL_CAPABILITY | cadena de versión de la app (evidencia L2488) |
| `env.CODEX_CLI_PATH` | MATERIAL_SECURITY | ruta del binario con su etiqueta (evidencia L2487) |
| `env.NODE_REPL_NODE_MODULE_DIRS` | MATERIAL_SECURITY | directorios de módulos de la instalación |
| `env.NODE_REPL_NODE_PATH` | MATERIAL_SECURITY | intérprete de la instalación |
| `env.NODE_REPL_TRUSTED_CODE_PATHS` | MATERIAL_SECURITY | rutas de código de confianza de la instalación |
| `env.NODE_REPL_TRUSTED_SERVICES` | MATERIAL_SECURITY | servicios de confianza; contiene la versión de la app (evidencia L2488) |
| `env.SKY_CUA_NATIVE_PIPE_DIRECTORY` | MATERIAL_SECURITY | directorio del canal nativo (la tubería `SKY_CUA_NATIVE_PIPE` no cambió) |
| `notify` | MATERIAL_SECURITY | ruta del programa notificador de la app |

- **Clase:** 8 MATERIAL_SECURITY y 1 MATERIAL_CAPABILITY; ninguna NON_MATERIAL_FOR_RECIPE con la receta vigente. Las 8 MATERIAL_SECURITY designan
  código lanzado (`command`, `CODEX_CLI_PATH`, `NODE_REPL_NODE_PATH`, `NODE_REPL_NODE_MODULE_DIRS`, `SKY_CUA_NATIVE_PIPE_DIRECTORY` y `notify`) o
  sus fronteras de confianza (`NODE_REPL_TRUSTED_CODE_PATHS` y `NODE_REPL_TRUSTED_SERVICES`): son materiales, y la regla 11, c) prohíbe a cualquier
  A-n futura tratarlas como acopladas a la versión o no materiales (§4). `BROWSER_USE_CODEX_APP_VERSION` cuenta hoy también como dentro, porque su
  función de etiqueta de versión es INFERRED.
- **Conclusión para el caso real:** con los hechos de hoy no se demuestra ninguna clase de cambio irrelevante: 8 de las 9 claves son de
  lanzamiento o de confianza, materiales, y la novena solo tiene una función de etiqueta de versión INFERRED. La localización de estas 9 claves
  (evidencia §71 y §87) se hizo por comparación por clave, que lee valores; A-3 no la hace ni la pide (regla 11, b).
- **Cambio de valor:** compatible con un refresco mecánico de etiquetas y rutas de versión (coincide con las tres actualizaciones de la app, la
  estructura es idéntica, el mismo conjunto en las dos actualizaciones con instantánea, y los tamaños 4 775 → 4 775 → 4 777 → 4 777 encajan con
  versiones de 13 y 14 caracteres en las dos claves que la contienen). Es INFERRED: sin valores no se excluye una sustitución de igual longitud.
- **Relevancia de seguridad:** aunque el refresco sea mecánico, `command`, `NODE_REPL_NODE_PATH`, `NODE_REPL_NODE_MODULE_DIRS`, `CODEX_CLI_PATH` y
  `notify` designan el código que corre, y las rutas nuevas apuntan al de la versión nueva. Una proyección igual acota el cambio de configuración; no
  acredita el código nuevo. La sonda solo lo detecta si altera una de las 11 propiedades de la clase (regla 4): las herramientas que expone el
  servidor lanzado solo cuentan si el catálogo de herramientas inyectado se observa dentro de la propiedad 8 (`DeclaredRuntimeContext`, V14 L1081),
  y lo que hace el programa posterior al turno con la entrada y la salida del modelo no lo observa ninguna; si no, UNVERIFIED (OQ-MAF-02..04). La
  identidad del runtime solo registra el cambio.
- **Con A-3** (regla 11, a), un cambio de estas claves es un cambio de huella: P-01 y OD-2 exacta, con la evidencia por nombre de la regla 11, b).

## 6. Superficies materiales fuera de la huella actual

La huella de `codex-cli` es solo `~/.codex/config.toml` (adapter L30). Para la receta 16.4 también son materiales, según la investigación:

| Superficie | Por qué | Cubierta hoy |
|---|---|---|
| binario `codex.exe` | código | `BinaryHash` |
| paquete de la app (servidor `node_repl`, programa `notify`) | código al que apuntan las claves de §5 | `AppVersion`, si las rutas caen dentro del paquete: no comprobado |
| `%USERPROFILE%\.codex\rules\*.rules` | política de comandos | no (D.6 la enumera para el Principal B) |
| hooks de usuario y de plugins | ejecución en eventos del turno; `features.hooks` activa por defecto | no; que existan, UNVERIFIED |
| `%USERPROFILE%\.codex\AGENTS.md`, skills, memorias | contexto automático | D.6 para el Principal B; no en la huella de `codex-cli` |
| contenido de plugins y marketplaces locales | servidores, skills y hooks | no |
| configuración gestionada (local o entregada desde la nube) | puede fijar sandbox, aprobación o hooks sin cambiar ningún archivo local | no; si aplica a esta cuenta, UNVERIFIED |
| defaults del binario | la misma configuración puede dar otra capacidad con otro binario | `BinaryHash` registra el cambio; la sonda solo lo detecta si altera una de las 11 propiedades: una herramienta nueva por defecto solo cuenta si el catálogo de herramientas inyectado se observa dentro de la propiedad 8 (`DeclaredRuntimeContext`, V14 L1081); si no, UNVERIFIED |

Por eso la regla 11, c) exige a un mecanismo futuro un manifiesto de superficies por adapter, custodiado por blob, no solo un archivo. A-3 no lo
define: el cambio de otra de estas superficies no es un cambio de la huella declarada, y solo lo detectan, si altera lo que observan, las demás
comprobaciones de V14 y la clase de la regla 4 (tabla anterior).

## 7. Propuesta no normativa para la A-n de OD-2-MAT: verificador local ciego al valor y EXP-MAF-01

A-3 no define este verificador ni lo autoriza (regla 11, a y b): es una propuesta de la investigación para la A-n que ordene OD-2-MAT, que
tendría que cumplir los requisitos de la regla 11, c). Lee valores, así que solo podría ejecutarse con una autorización expresa del Owner que
nombre el programa y su blob, las superficies que lee y su caducidad.

- **Tokens** observados con independencia de la propia huella: versión del paquete de la app y etiqueta del directorio del binario verificado (ruta
  verificada de 16.4). Lista cerrada. La ubicación de instalación del paquete solo se usa en la comprobación adicional, como comparación y nunca
  como sustitución; los prefijos del perfil no son tokens: la privacidad la da que las proyecciones nunca se publican.
- **Proyección:** cada valor, serializado de forma canónica, con cada aparición exacta de un token sustituida por su marcador, el más largo primero;
  un token de una versión anterior acreditada que queda es un **resto** y la clave no se explica.
- **Línea base material:** por clave, un digest con clave solo local de la proyección, derivado de una huella aceptada con OD-2 (con una clave solo
  local, como la instantánea por clave de EV L2490, que es de `723A6898…`, huella nunca aceptada, y no puede servir de línea base), más un digest
  de la estructura.
- **Comprobaciones adicionales** por clase: la ruta resuelta cae dentro de la instalación del paquete; la proyección de `CODEX_CLI_PATH` liga la ruta
  al binario verificado actual (detecta también una actualización a medio completar).
- **Salida que se publicaría** (lista cerrada, como exige la regla 11, c): modo y veredicto, con sus causas por nombre de clave; el SHA-256 de la
  huella observada y el de la huella aceptada de la que deriva la línea base, con el digest de sus reglas (mapa, proyección, tipos de token, conjunto,
  versiones verificadas y verificador); por clave, nombre saneado (solo los segmentos que figuran literalmente en el mapa cerrado del adapter;
  cualquier otro, como las rutas de `[projects.<redactado>]`, entre comillas simples o dobles o sin ellas, por un marcador con digest con clave),
  clase, cambiada sí/no y proyección igual sí/no; tipos de token; digests con clave solo local (nunca un hash sin clave); y los hechos de identidad
  del runtime. Ningún otro dato (ni etiquetas, ni horas de escritura, ni hashes de rutas de instalación) y nunca valores.
- **EXP-MAF-01** (retroactivo, sin ejecutar Codex): sustituir en el archivo vigente los tokens actuales por los de una versión aceptada y comparar el
  SHA-256 con su huella aceptada (p. ej., `9002E854…`). Igualdad: refresco mecánico VERIFIED; desigualdad: inconcluso, fallo cerrado. La comparación
  por clave con las instantáneas de `723A6898…` y `9EA26634…` (nunca aceptadas por OD-2) solo localiza el cambio entre dos huellas no aceptadas: no
  acredita el refresco frente a una línea base material, que deriva solo de una huella aceptada (regla 11, c), y su resultado es, como mucho,
  INFERRED. Solo la igualdad del SHA-256 con `9002E854…` (OD-2c, DEC L931) da VERIFIED.
- **Autoridad:** el verificador y EXP-MAF-01 leerían valores en el host, aunque no los publiquen, y eso se aparta de los términos con los que
  el Owner aprobó OD-2 («nunca valores», paquetes L93) y de README L366. A-3 no lo autoriza ni lo pide (regla 11, b), y la investigación no propone
  ninguna autorización ahora: solo podría permitirlo una autorización expresa del Owner que nombre el programa y su blob, las superficies que lee y
  su caducidad, que ni una disposición del Coordinator ni una autorización de medición o de consumo sustituyen. El paquete de OD-2-MAT, sección
  «Lectura de valores (no se pide ahora)», registra la suspensión de la localización por clave desde la decisión S-01, posterior al commit
  `b0884216` que custodia la última localización por clave (evidencia L2879-2881; solicitud de recuperación L45), publicada en el §4 de la
  solicitud de recuperación (commit `76b48e78`), y la historia completa de las lecturas anteriores, sin calificarlas.

## 8. Correspondencia entre las opciones del Owner y la regla 11

El paquete de decisión del Owner es `od2-material-baseline-owner-packet.md` (OD-2-MAT, PENDIENTE). La investigación de la huella material propuso
un borrador con otra numeración («OD-2M», maf-codex.md §8.4); para no presentar al Owner dos decisiones sobre la misma materia, A-3 solo referencia
OD-2-MAT, y la correspondencia es:

A-3 solo compara la huella por su valor exacto (regla 11, a); cualquier mecanismo de huella material lo tendría que introducir la A-n siguiente
que ordenara la opción elegida (previsiblemente A-4; su número lo fija LIFECYCLE §6 al publicarse), con los requisitos de la regla 11, c).

| OD-2-MAT | Borrador «OD-2M» de maf-codex.md | Qué tendría que fijar la A-4 (regla 11, c) |
|---|---|---|
| A (mantener la huella exacta) | A (statu quo con el registro del verificador como evidencia) | ninguna A-4: la huella, por su valor exacto (regla 11, a); la evidencia por nombre de la regla 11, b) acompaña cada solicitud de OD-2 exacta; ningún verificador ni lectura de valores |
| B (A-4: claves volátiles neutralizadas en la receta, comparadas por nombre y con digests con clave por clave, que leen valores en el host sin registrarlos) | C (B más receta endurecida) | un modo que acepte por clase con la proyección por neutralización y cómo compone con la regla 1, c de A-3; exige medir la neutralización, una clave durable por host y una autorización de lectura de valores del Owner |
| C (A-4: predicados de valor evaluados mecánicamente sin registrar valores) | B (clase VERSION_COUPLED por proyección ciega al valor) | un modo que acepte por clase con la proyección de §7 y cómo compone con la regla 1, c de A-3; exige una clave durable por host y una autorización de lectura de valores del Owner |
| D (A-4: configuración dedicada de `codex-cli` con OD-2 exacta sobre ella) | — | huella exacta sobre otra configuración; si la app deja de reescribirla, A-3 se aplica a `codex-cli` sin cambiar su texto, con la misma limitación de la sonda (§5 y §6) |
| — | D (pausar las actualizaciones automáticas durante los pilotos) | ninguno: acción propia del Owner, compatible con cualquier opción |

Ninguna de las opciones A-D autoriza leer valores, y el paquete no pide ninguna lectura de valores: la A-n que encarguen B o C necesitaría su
propia autorización expresa del Owner, que nombre el programa y su blob, las superficies y la caducidad (paquete, «Lectura de valores (no se pide
ahora)»).

La recomendación de preparación del paquete (A ahora; D a estudiar tras F6 si el Coordinator lo ordena) es del paquete; A-3 no la adopta ni la
discute.

## 9. Preguntas abiertas (de la investigación)

| Id | Pregunta |
|---|---|
| OQ-MAF-01 | versión del binario vigente `3553cd6e…` (exige `codex --version` en un bloque autorizado) |
| OQ-MAF-02 | si `node_repl` arranca en la corrida de la receta y qué herramientas expone (árbol de procesos y registro de sesión en la sonda) |
| OQ-MAF-03 | si se lanza `notify` tras el turno, qué programa es y si sobrevive a `codex.exe` |
| OQ-MAF-04 | si se admiten llamadas de `node_repl` con aprobación `never` y `-s read-only` |
| OQ-MAF-05 | si la sesión de `exec` es aislada |
| OQ-MAF-06 | qué son los 14 `<redactado>` (el verificador local puede responder sin publicar nombres) |
| OQ-MAF-07 | si la lista de tokens basta para explicar las 9 claves (lo responde EXP-MAF-01) |
| OQ-MAF-08 | si hay hooks, reglas `.rules` relevantes o configuración gestionada para esta cuenta |
| OQ-MAF-09 | si una regla de clase adoptada por el Owner cumple A-2 regla 4 («la huella exacta resultante», A-2 L227) o hace falta una A-n que la sustituya |
| OQ-MAF-10 | si redefinir P-01 para las unidades I62 activa M-08 frente a ADR-0046 L95 |
| OQ-MAF-11 | quién reproduce el veredicto en la aceptación si solo el host puede leer los valores (A-3 regla 5: nunca una autodeclaración) |
| OQ-MAF-12 | si conviene endurecer la receta (`-c notify=[]`, `-c mcp_servers.node_repl.enabled=false`) aunque OD-2 no evolucione; cambia 16.4 y exige medir de nuevo; `--ignore-user-config` no se recomienda porque dejaría de heredar `service_tier`, `windows.sandbox` y la confianza |

Las observaciones de OQ-MAF-01..05 caben en la misma invocación de solo lectura de un bloque ya autorizado, sin consumo adicional; pedirlas o no es
materia de la solicitud del bloque, no de A-3.

## 10. Fuentes

Proveedor (consultadas el 2026-10-08 por la investigación, sin enviar datos locales):

- https://learn.chatgpt.com/docs/config-file/config-reference
- https://learn.chatgpt.com/docs/config-file/config-basic
- https://learn.chatgpt.com/docs/config-file/config-advanced
- https://learn.chatgpt.com/docs/config-file/environment-variables
- https://learn.chatgpt.com/docs/extend/mcp?surface=cli
- https://learn.chatgpt.com/docs/hooks
- https://github.com/openai/codex/blob/main/docs/config.md
- https://raw.githubusercontent.com/openai/codex/rust-v0.160.1/codex-rs/exec/src/lib.rs
- https://raw.githubusercontent.com/openai/codex/rust-v0.160.1/codex-rs/exec/src/cli.rs
- https://raw.githubusercontent.com/openai/codex/rust-v0.160.1/codex-rs/core/src/session/mcp_runtime.rs
- https://raw.githubusercontent.com/openai/codex/rust-v0.160.1/codex-rs/core/src/mcp_tool_exposure.rs
- https://raw.githubusercontent.com/openai/codex/main/codex-rs/core/src/mcp_tool_call.rs
- https://raw.githubusercontent.com/openai/codex/main/codex-rs/core/src/mcp.rs
- https://raw.githubusercontent.com/openai/codex/rust-v0.160.1/codex-rs/hooks/src/registry.rs
- https://raw.githubusercontent.com/openai/codex/rust-v0.160.1/codex-rs/hooks/src/legacy_notify.rs
- https://raw.githubusercontent.com/openai/codex/rust-v0.160.1/codex-rs/core/src/session/turn.rs
- https://raw.githubusercontent.com/openai/codex/rust-v0.160.1/codex-rs/core/config.schema.json
- https://raw.githubusercontent.com/openai/codex/rust-v0.160.1/codex-rs/features/src/lib.rs
- https://github.com/openai/codex/issues/29338

I-62 (en `b0884216`): `docs/automation/evidence/I-62-evidence.md` (blob `6a2b1569…`; §67, §71, §87);
`docs/automation/evidence/I-62-prep/owner-decision-packets.md` (blob `cdb86114…`; L182, L199, L200);
`docs/automation/decisions/I-62.md` (blob `27591f16…`; L891, L1043); `docs/AUTOMATION_PLAN.md` (blob `f525cb1e…`; L541-543);
`docs/automation/agent-execution/adapters/codex-cli.md` (blob `155f3469…`; L30).
