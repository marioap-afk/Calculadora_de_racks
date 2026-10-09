# I-62 — Investigación MATERIAL_ADAPTER_FINGERPRINT (foco Codex)

- **Naturaleza:** investigación preparatoria de A-3 (decisiones §57, punto 4). **No es normativa**: no es una enmienda, no acepta huellas, no
  autoriza consumo ni sondas, no cambia la receta de 16.4 y no modifica A-2 (blob `f1e1d6f08d3cf677500794c7d0019433a4dc7d4e`) ni el Freeze V14.
- **Orden:** cuerpo de la disposición §57 (3 654 bytes, SHA-256 `00a13ae527863117472bc7356458c849a8ef6f6e6255b417ee736dc3428023a3`), punto 4.
- **Autoridades leídas (solo lectura):** worktree `architecture-portabilidad-coordinador-principal` en `b088421649204742ff682e1eb1ebdafc762823d0`
  (árbol limpio). Los números de línea citados son de ese commit.
- **Configuración de Codex:** conocida solo por el archivo saneado `codex-cfg-keynames-6518EFAB.json` (107 nombres, sin valores, rutas de proyecto
  redactadas; custodia local, no publicado, así que la clasificación por nombre no se puede reproducir desde el commit; el recuento de 107 nombres
  consta en `docs/automation/evidence/I-62-F6/OD-2/R20261008T2200Z-codex-passive/result.json`) y por la evidencia custodiada. No se leyó ningún
  archivo de `%USERPROFILE%\.codex`, no se ejecutó Codex ni ninguna otra CLI de IA.
- **Fecha:** 2026-10-08.

**Convención de estado de cada afirmación**

| Etiqueta | Significado |
|---|---|
| VERIFIED-SRC(tag) | código público de `openai/codex` en ese tag (URL en §11) |
| VERIFIED-DOC | documentación pública del proveedor (URL en §11) |
| VERIFIED-LOCAL | evidencia custodiada de I-62 (sección y línea citadas) |
| INFERRED | deducción a partir de hechos verificados, sin fuente directa |
| UNVERIFIED | sin fuente; se trata como desconocido (fallo cerrado) |

**Límite de versión.** El binario vigente `3553cd6e7df5a093d8cb8301cd8088a57e0971aba71ddbe0e67f7f44a15cdf68` (app `26.1002.7124.0`) no tiene versión
observada: `codex --version` exige ejecutarlo y `codex-cli` está en STOP P-01. El último binario medido fue `codex-cli 0.160.1` (`3b8f6e33…`,
evidencia §70). Por eso el código se verificó en el tag `rust-v0.160.1` y, donde se indica, en `main` del 2026-10-08 (últimos tags publicados ese día:
`rust-v0.162.0` y `rust-v0.163.0-alpha.1`). **Que esas conductas valgan para el binario vigente es UNVERIFIED hasta observar su versión.**

---

## 0. Conclusiones

1. **La receta 16.4 no aísla la corrida de `config.toml`.** `-s read-only`, `-m` y `-c model_reasoning_effort` neutralizan el sandbox de comandos,
   el modelo y el effort, y `codex exec` fija por defecto la aprobación en `never`. Pero `codex exec` arranca de forma anticipada (Eager) los servidores
   MCP configurados, expone sus herramientas al modelo, lanza el programa `notify` tras la respuesta final del turno, carga los plugins habilitados y
   hereda `service_tier`, `windows.sandbox`, la confianza de proyectos y `shell_environment_policy` (VERIFIED-SRC `rust-v0.160.1`, salvo los puntos
   marcados en §3).
2. **La frase del paquete OD-2d «la receta de solo lectura no depende de ellas»** (owner-decision-packets L200) **no se sostiene**: las claves de
   `mcp_servers.node_repl` y `notify` configuran un servidor MCP que exec arranca y un programa que exec lanza (§3, F-02, F-03 y F-07). Esa frase no se
   puede usar como razón de no materialidad.
3. **Clasificación de los 107 nombres** (§4), hoy, sin versión observada del binario: 37 MATERIAL_SECURITY, 39 MATERIAL_CAPABILITY,
   2 MATERIAL_CONTEXT_OR_OUTPUT y 29 UNKNOWN (los 14 nombres saneados `<redactado>` y 15 que serían NON_MATERIAL_FOR_RECIPE, con razón verificada,
   solo con la versión observada dentro del rango verificado).
4. **Las 9 claves cambiadas** (§5): 8 son MATERIAL_SECURITY y 1 MATERIAL_CAPABILITY; **ninguna** es NON_MATERIAL_FOR_RECIPE. El cambio de valor es
   **compatible** con un refresco mecánico de etiquetas y rutas de versión (sincronía con la app, estructura idéntica, aritmética de longitudes), pero
   eso es INFERRED, no VERIFIED. Aunque fuera mecánico, **no es inocuo**: apunta a código nuevo (servidor `node_repl`, programa `notify` y CLI de la
   versión nueva). Es parte del evento de sucesor del runtime (A2-P2, A-3), no una variación irrelevante de configuración. Las 8 MATERIAL_SECURITY
   designan código lanzado o sus fronteras de confianza: son materiales, y A-3, regla 11, c) exige que un mecanismo futuro nunca las trate como
   acopladas a la versión ni no materiales. Con los hechos de hoy no se demuestra ninguna clase de cambio irrelevante.
5. **Corrección de un hecho de partida** (premisa de trabajo sin fuente custodiada; decisiones L1115 solo dice «cada actualización modificó
   `config.toml`»): que las tres actualizaciones cambiaran las mismas 9 claves solo está demostrado para dos (723A→9EA2, evidencia L2568-2570;
   9EA2→6518, L2879-2881). Para la primera (9002→723A) no había instantánea por clave: «No se puede probar cuál sin el archivo anterior» (evidencia
   L2488).
6. **El hash exacto es a la vez demasiado estricto y demasiado débil** (§6): para en cada refresco de versión, pero no ve reglas de ejecución, hooks,
   `AGENTS.md`, skills, contenido de plugins, configuración gestionada (incluida la entregada desde la nube) ni el código al que apuntan las rutas.
7. **Mecanismo propuesto** (§7; propuesta no normativa para la A-n que ordene OD-2-MAT: A-3 no define verificador, regla 11, a y c): un verificador
   local y ciego al valor que clasifica cada clave por un mapa cerrado, proyecta los valores sustituyendo solo tokens de versión observados de forma
   independiente y compara digests HMAC de la proyección con una línea base material acreditada. Da cuatro veredictos (IDENTICAL, VERSION_COUPLED,
   MATERIAL_CHANGE, UNKNOWN). Conserva los seis elementos del punto 4. Lee valores en el host: solo podría ejecutarse con una autorización expresa del
   Owner que nombre el programa y su blob, las superficies y la caducidad, que A-3 no pide. **Bajo la autoridad actual, P-01 sigue siendo literal** y
   A-3 solo exige la evidencia por nombre de su regla 11, b).
8. **¿Puede OD-2 evolucionar?** (§8) Técnicamente sí, para la clase VERSION_COUPLED (que, por el requisito de A-3, regla 11, c, nunca podría incluir
   las claves de lanzamiento de código ni sus fronteras de confianza). Pero cambia autoridad reservada: OD-2 es decisión del Owner,
   P-01 está definido como cambio del hash (AUTOMATION_PLAN L698) y ADR-0046 L95 lo registra, y A-2 prohíbe autorizaciones anticipadas para pares no
   observados (L224-225). Hace falta una **decisión del Owner** (el paquete OD-2-MAT, od2-material-baseline-owner-packet.md; el borrador «OD-2M»
   de §8.4 está superado) y una A-n material; no cabe en una disposición ordinaria del Coordinator. Aun con esa decisión, cada actualización exige al
   menos un acto del Owner: la autorización de consumo del bloque de sondas (A-2 §7, L304).

---

## 1. Orden y frontera de esta investigación

Orden del Coordinator (`s57-body.txt`, punto 4; registro en decisiones L1115):

- L29: «Objetivo: distinguir variaciones automáticas irrelevantes de cambios materiales de seguridad, configuración y capacidades.»
- L30: «Examinar especialmente Codex, donde cada actualización ha modificado `config.toml`.»
- L31-33: «No introducir aceptación implícita de versiones nuevas. / No suponer que un runtime sucesor es mejor. / No desactivar P-01 en producción bajo la
  autoridad actual.»
- L34-41: la propuesta debe conservar seis elementos: evidencia del cambio de binario y versión; clasificación de claves cambiadas; comparación contra
  propiedades materiales previamente acreditadas; sonda rápida de compatibilidad con presupuesto autorizado; fallo cerrado para cambios desconocidos;
  ausencia de ampliaciones implícitas de permisos o consumo.
- L43-44: «Evaluar explícitamente si OD-2 puede evolucionar desde un hash exacto de todo `config.toml` hacia una línea base material verificable sin
  requerir una nueva decisión Owner por cada cambio inocuo. / Si ese cambio modifica autoridad reservada al Owner, preparar la decisión Owner
  correspondiente; no resolverla por disposición ordinaria del Coordinator.»

## 2. Autoridades que fijan la frontera actual (literales)

| Fuente | Línea | Literal |
|---|---|---|
| AUTOMATION_PLAN 16.4 | L517-519 | «SHA-256 de `~/.codex/config.toml` y la lista ordenada de sus nombres de secciones y claves, sin valores.» |
| AUTOMATION_PLAN 16.4 | L522-523 | «la comparación de `config.toml`, cuyo cambio es STOP (P-01): ninguna invocación de Codex más hasta que decida el Owner, y se registra el diff de nombres de secciones y claves.» |
| AUTOMATION_PLAN 16.4 | L541-543 | «**Receta de Codex:** binario por ruta verificada; `-C` con el worktree de la unidad únicamente; `-s read-only`; `-m` y `-c model_reasoning_effort=…` de una celda elegible; `--output-schema`; `-o` al directorio del `RunId`; `--json`; sin `--ephemeral`, …; stdin cerrado; …; tope de 600 s; el `service_tier` de la configuración del Owner se hereda sin modificarlo.» |
| AUTOMATION_PLAN 16.9 | L698 | «\| P-01 \| Cambio del hash de `config.toml` \| STOP \|» |
| AUTOMATION_PLAN 16.19 | L1056 | «P-01 se conserva, y P-11 lo generaliza a toda huella declarada.» |
| ADR-0046 (aceptado) | L92 | «las recetas de invocación dependen del entorno (Windows, `unelevated`).» |
| ADR-0046 (aceptado) | L95 | «los efectos laterales de Codex sobre `~/.codex/config.toml`: un cambio de su hash detiene las invocaciones;» |
| V14 §7 | L330-331 | «`codex-*` declara `config.toml`; **P-01 se conserva**.» |
| V14 §13 | L777 | «\| P-11 \| huella declarada cambiada durante una cesión \| STOP (P-01 para Codex, igual) \|» |
| V14 §18 | L971 | «\| OD-2 \| línea base de huella por adapter (…) \| toda invocación afectada (…) \| antes de la primera invocación afectada \|» |
| decisiones §46 | L890 | OD-2: «antes de cada invocación afectada se recalcula; si cambia, P-01 y STOP del transporte afectado, sin aceptar la nueva huella» |
| decisiones §47 | L913 | “«aceptar el hash futuro que aparezca si el delta saneado coincide» es demasiado amplio para esa frontera” |
| decisiones §49 | L949 | «no normalizar cambios ni crear entradas a mano; `workspace-write` no recibe actualizaciones automáticas de la línea base» |
| decisiones §50 | L967 | «No aceptar `723A6898…` porque la actualización parezca legítima.» |
| A-2 regla 3 | L222-226 | «un solo bloque de medición nuevo por cada actualización observada, de como máximo dos sondas de solo lectura para el par exacto (…) La autorización es explícita y por bloque (…) Nunca es permanente ni anticipada (no se autoriza para un par todavía no observado)» (negritas omitidas) |
| A-2 regla 4 | L227 | «el Owner acepta con OD-2 la huella exacta resultante antes de cualquier invocación afectada;» |
| A-2 §7 | L304 | «para cada bloque: autorizar el consumo de sus sondas y aceptar con OD-2 la huella resultante.» |
| LIFECYCLE §3 | L94 | «Un cambio de alcance, no-objetivos, decision Owner o retiro de escenario OV es `OWNER-RESERVED`.» |
| LIFECYCLE §6 | L221 | «Una M material exige Architect + Coordinator; una materia OWNER-RESERVED exige Owner.» |
| adapter `codex-cli` | L30 | «`~/.codex/config.toml`: SHA-256 y nombres ordenados de secciones y claves, sin valores y saneados; obligatoria. P-01 se conserva» |
| adapter `codex-cli` | L57 | «Invocar la CLI puede reescribir `config.toml` (DEV-G1C-01 de I-61): por eso cada cesión compara la huella de salida y de entrada.» |
| README §13.4 | L349 | «un nombre con una ruta o una unidad (`\`, `/` o `:`) se sustituye por su sección con `<redactado>` (…), conservando el número de nombres» |
| A-3 candidata §3.6 | — | (paráfrasis, texto candidato no acordado) la huella no forma parte de la clase ni de la relación de sucesor; un cambio de huella deja el caso fuera de A-3 |

---

## 3. Conducta de la receta 16.4 (hechos del proveedor)

### 3.1 Qué neutraliza la receta y qué no

| Elemento de la receta | Efecto en `codex exec` | Estado |
|---|---|---|
| `-s read-only` | política de sistema de archivos y red **para la ejecución de comandos** | VERIFIED-DOC (`sandbox_mode`). La documentación dice que el proxy de red gobierna solo comandos con sandbox, no la búsqueda web, las apps ni MCP, y que el tráfico de apps/conectores no lo controla el proxy de comandos |
| aprobación (la receta no pasa ninguna) | exec fija `approval_policy: Some(AskForApproval::Never)` por defecto y solo la reconstruye si el revisor resuelto es AutoReview | VERIFIED-SRC(`rust-v0.160.1`, `main`); VERIFIED-LOCAL: `approval_policy` never en `turn_context` (owner-decision-packets L182) |
| `-m`, `-c model_reasoning_effort=…` | la precedencia más alta (flags y `--config`) | VERIFIED-DOC; VERIFIED-LOCAL (modelo y effort observados iguales a los pedidos, evidencia §64 y §70) |
| `--output-schema`, `-o`, `--json` | forma de la respuesta final, último mensaje a archivo, eventos JSONL | VERIFIED-SRC(`rust-v0.160.1`, `exec/src/cli.rs`) |
| sin `--ephemeral` | la sesión se persiste | VERIFIED-SRC |
| `-C <worktree>` | directorio de trabajo; decide qué entrada `projects` aplica | VERIFIED-DOC (confianza por proyecto) |
| `service_tier` | exec lo deja en `None` en sus overrides y en el turno, así que rige el de `config.toml` | VERIFIED-SRC(`rust-v0.160.1`, `exec/src/lib.rs`); coherente con 16.4 L543 |
| **no neutraliza** | `mcp_servers.*`, `notify`, `plugins.*`, `marketplaces.*`, `projects.*`, `windows.sandbox`, `shell_environment_policy.*`, `features.*` | derivado de F-01..F-20 |
| opciones existentes que la receta no usa | `--ignore-user-config` (no cargar `$CODEX_HOME/config.toml`; la autenticación sigue usando `CODEX_HOME`), `--ignore-rules` (no cargar reglas `.rules`), `--strict-config` (error ante campos no reconocidos) | VERIFIED-SRC(`rust-v0.160.1`); en el binario vigente UNVERIFIED |

### 3.2 Hechos por superficie

| Id | Hecho | Estado |
|---|---|---|
| F-01 | exec arranca un app server en proceso (`InProcessAppServerClient`), con `session_source: SessionSource::Exec` y `client_name: "codex_exec"` | VERIFIED-SRC(`rust-v0.160.1`) |
| F-02 | Los servidores MCP efectivos de la sesión se arrancan con `McpStartupPolicy::Eager` para todo origen salvo `SessionSource::SubAgent` (que usa `LazyWhenCached`). No hay rama para `Exec` | VERIFIED-SRC(`rust-v0.160.1`, `core/src/session/mcp_runtime.rs`) |
| F-03 | Las herramientas MCP que no son de apps se exponen al modelo, directamente o diferidas tras la búsqueda de herramientas, si son visibles para el modelo. El módulo de exposición no filtra por origen de sesión, sandbox ni aprobación | VERIFIED-SRC(`rust-v0.160.1`, `core/src/mcp_tool_exposure.rs`) |
| F-04 | Aprobación de llamadas MCP: modos `auto`/`prompt`/`writes`/`approve`. En `auto`, una herramienta sin anotaciones exige aprobación. Con aprobación `never`, una llamada que la exige se rechaza, salvo que antes la apruebe automáticamente una función auxiliar no inspeccionada | VERIFIED-SRC(`main`, `core/src/mcp_tool_call.rs`), parcial. **Que una llamada `node_repl` se ejecute en una corrida de solo lectura: UNVERIFIED** |
| F-05 | Proceso del servidor MCP y sandbox: no hay declaración del proveedor de que el sandbox del sistema operativo envuelva servidores MCP stdio. `node_repl` anuncia la capacidad experimental `codex/sandbox-state-meta` y se espera que arranque su núcleo JS dentro del sandbox configurado: se lo aplica él mismo con los metadatos que recibe de Codex. Existe una opción `--disable-sandbox` del servidor | auto-aplicación y opción: issue público #29338 del repositorio del proveedor (informe de usuario); que el sandbox del SO **no** lo envuelva: UNVERIFIED |
| F-06 | La app de escritorio, la CLI y la extensión del IDE comparten la configuración MCP | VERIFIED-DOC; coherente con que cada actualización de la app reescriba `mcp_servers.node_repl` (VERIFIED-LOCAL §67, §71, §87) |
| F-07 | `notify`: el registro de hooks construye el hook `after_agent` a partir de `legacy_notify_argv` si el argv y su primer elemento no están vacíos, sin comprobar `feature_enabled` ni la confianza de hooks. `run_turn` lo despacha tras la respuesta final (`if !needs_follow_up`) sin comprobar el origen de sesión. El hook lanza el proceso sin esperarlo (`spawn`), con stdio nulo, sin sandbox visible y con el JSON como último argumento (`type` = `agent-turn-complete`, `thread-id`, `turn-id`, `cwd`, `client`, `input-messages`, `last-assistant-message`). La documentación confirma que `notify` se dispara en `agent-turn-complete` | VERIFIED-SRC(`rust-v0.160.1`: `hooks/src/registry.rs`, `hooks/src/legacy_notify.rs`, `core/src/session/turn.rs`) + VERIFIED-DOC. **El enlace `notify` de config → `legacy_notify_argv` no se localizó: INFERRED.** Conclusión: «notify se ejecuta tras un turno de exec» = verificado salvo ese enlace |
| F-08 | Consecuencia para las comprobaciones de 16.4: el JSON de `notify` lleva `cwd` (la ruta del worktree) en la línea de órdenes, y el proceso puede sobrevivir a `codex.exe`. Si sigue vivo en la entrada, es participante por 16.4 L526 («cuya línea de órdenes contiene la ruta del worktree») y produce P-02 o P-12 | INFERRED de F-07; no observado (las 4 sondas custodiadas no registraron P-02) |
| F-09 | Plugins: los servidores de los plugins seleccionados se añaden salvo que el plugin esté deshabilitado. Con la feature `plugins` apagada se descartan. `codex_apps` solo entra con apps habilitadas y sesión no aislada. exec manda `disabled_plugin_ids: None` en el turno | VERIFIED-SRC(`main`: `core/src/mcp.rs`, `exec/src/lib.rs`). **Si la sesión de exec es «aislada» (`SessionIsolation::Isolated`): UNVERIFIED** |
| F-10 | Hooks: `features.hooks` es Stable y está activa por defecto. Los hooks no gestionados exigen revisión y confianza antes de ejecutarse. Habilitar un plugin no confía sus hooks. Los hooks de proyecto solo cargan con el proyecto de confianza. `HooksConfig` incluye `plugin_hook_sources` | VERIFIED-DOC + VERIFIED-SRC(`rust-v0.160.1`, `features/src/lib.rs`, `hooks/src/registry.rs`) |
| F-11 | Confianza: con un proyecto sin confianza, Codex omite las capas `.codex/` del proyecto (config, hooks, reglas); la configuración de usuario y de sistema sigue cargando. `read-only` no crea entradas de proyecto (2 de 2) y `workspace-write` sí (2 de 2) | VERIFIED-DOC; VERIFIED-LOCAL (evidencia L2427) |
| F-12 | `windows.sandbox` ∈ {`elevated`, `unelevated`, `mxc`}: implementación del sandbox de Windows | VERIFIED-SRC(`rust-v0.160.1`, `core/config.schema.json`) + VERIFIED-DOC |
| F-13 | `desktop`: el esquema del núcleo lo describe como ajustes opacos de la app de escritorio guardados en `config.toml` | VERIFIED-SRC(`rust-v0.160.1`, `core/config.schema.json`) |
| F-14 | `features.js_repl` es una bandera de compatibilidad **Removed** (la feature JS REPL se borró), `default_enabled: false`. Una clave desconocida en `[features]` se ignora con un aviso | VERIFIED-SRC(`rust-v0.160.1` y `main`, `features/src/lib.rs`) |
| F-15 | `service_tier`: `fast` se traduce al valor de petición `priority` (más consumo) | VERIFIED-DOC; herencia: VERIFIED-SRC (§3.1) |
| F-16 | Precedencia: flags y `--config` > `.codex/config.toml` de proyecto (solo con confianza) > perfil > `~/.codex/config.toml` > valores por defecto gestionados desde la nube para el workspace > sistema > valores por defecto | VERIFIED-DOC |
| F-17 | `-c` admite rutas con puntos (ejemplo del proveedor: `mcp_servers.context7.enabled=false`) y valores TOML. Con `notify` vacío no se registra el hook (F-07) | VERIFIED-DOC + VERIFIED-SRC; **no probado** en el binario vigente |
| F-18 | `shell_environment_policy.set` inyecta valores explícitos en el entorno de los subprocesos que lanza Codex | VERIFIED-DOC |
| F-19 | Las features y herramientas activas por defecto dependen del binario (p. ej., `apps`, `plugins`, `hooks`, `multi_agent` activas por defecto en `rust-v0.160.1`): un `config.toml` idéntico puede dar otra capacidad con otro binario | defaults VERIFIED-SRC; consecuencia INFERRED |
| F-20 | Los cuatro flujos `--json` custodiados (OD-2b-PROBE y OD-2d-PROBE) solo contienen eventos `thread.*`, `turn.*` e `item.*` de tipo `command_execution` y `agent_message`. No hay ningún evento de arranque MCP ni `mcp_tool_call`. **Si `node_repl` arrancó durante esas sondas no consta** | VERIFIED-LOCAL (`probe*-events.jsonl`); arranque: UNVERIFIED |

---

## 4. Clasificación de los 107 nombres del archivo saneado

**Reglas aplicadas.**
- La clase se asigna por la función de la clave en una corrida de la receta 16.4, no por quién la escribe ni por si el cambio «parece automático».
- NON_MATERIAL_FOR_RECIPE solo con razón verificada (un override de la receta la neutraliza, o el núcleo no la interpreta), y **condicionada al rango
  de versiones verificado** (`rust-v0.160.1`…`main` del 2026-10-08). Sin versión observada del binario, la razón no se puede aplicar: UNKNOWN.
- Los nombres `<redactado>` no se pueden clasificar desde el archivo saneado: UNKNOWN (fallo cerrado). Un verificador local que ve los nombres reales los
  puede resolver sin publicarlos (§7).
- Las variables `env` de `node_repl` no tienen semántica pública documentada. Su clase base sí está verificada: configuran un servidor MCP que exec
  arranca y expone (F-02, F-03). La subclase sale del nombre (INFERRED). Para el fallo cerrado basta la clase base: material.

| Nombres | n | Clase | Razón | Estado |
|---|---|---|---|---|
| `[desktop]`, `[desktop.open-in-target-preferences]`, `[desktop.open-in-target-preferences.perPath]`, `desktop.ambient-suggestions-enabled`, `desktop.conversationDetailMode`, `desktop.followUpQueueMode`, `desktop.keepRemoteControlAwakeWhilePluggedIn`, `desktop.open-in-target-preferences.global`, `desktop.reviewDelivery` | 9 | NON_MATERIAL_FOR_RECIPE | ajustes opacos de la app de escritorio; exec no los interpreta | F-13 VERIFIED-SRC; condicional a la versión |
| `[tui]`, `tui.screen_reader_detection_done` | 2 | NON_MATERIAL_FOR_RECIPE | ajustes específicos de la TUI; exec usa el app server en proceso, sin TUI | VERIFIED-SRC (esquema; F-01) |
| `model`, `model_reasoning_effort` | 2 | NON_MATERIAL_FOR_RECIPE | la receta pasa `-m` y `-c model_reasoning_effort`, con precedencia máxima; el efectivo se lee de `turn_context` | F-16 VERIFIED-DOC + VERIFIED-LOCAL; condición: la receta los pasa siempre |
| `[features]`, `features.js_repl` | 2 | NON_MATERIAL_FOR_RECIPE (condicional) | bandera Removed (no-op) en el rango verificado | F-14; fuera del rango o sin versión: UNKNOWN |
| `service_tier` | 1 | MATERIAL_CAPABILITY (consumo) | exec lo hereda (16.4 L543); un cambio de valor cambia el consumo | F-15 |
| `[windows]`, `windows.sandbox` | 2 | MATERIAL_SECURITY | implementación que hace cumplir `-s read-only`; ADR-0046 L92 registra `unelevated`; OD-4 (decisiones L891) prohíbe cambiarlo | F-12 |
| `[projects.<redactado>]` ×12 | 12 | MATERIAL_SECURITY | confianza por proyecto: decide si cargan config, hooks y reglas de la capa `.codex/` del proyecto | F-11 |
| `<redactado>` ×14 | 14 | **UNKNOWN** | nombres con ruta saneados (README L349). Hipótesis INFERRED: 12 `projects.<ruta>.trust_level` (serían MATERIAL_SECURITY; la evidencia L2430-2431 nombra esa clave en una sección de proyecto) y 2 claves de `desktop.open-in-target-preferences.perPath` (serían NON_MATERIAL) | UNVERIFIED |
| `[marketplaces.openai-bundled]`, `[marketplaces.openai-primary-runtime]` y sus `source`, `source_type` | 6 | MATERIAL_SECURITY | origen de suministro de plugins; un refresco del marketplace puede instalar o refrescar plugins, también deshabilitados | VERIFIED-DOC; si exec refresca: UNVERIFIED |
| `[plugins."…"]` ×16 y `plugins."…".enabled` ×16 | 32 | MATERIAL_CAPABILITY (con impacto de seguridad) | un plugin habilitado aporta servidores MCP, skills y hooks; los conectores (`github`, `outlook-*`, `teams`) generan tráfico fuera del proxy de comandos; los de navegador y uso del ordenador (`browser`, `chrome`, `computer-use`, `unified-computer-use`) dependen, por nombre, de `node_repl` | F-09, F-10, F-03; dependencia de `node_repl` INFERRED |
| `[mcp_servers.node_repl]`, `[mcp_servers.node_repl.env]` | 2 | MATERIAL_SECURITY | servidor MCP stdio arrancado en exec, con herramientas expuestas | F-02, F-03 |
| `mcp_servers.node_repl.command`, `mcp_servers.node_repl.args` | 2 | MATERIAL_SECURITY | ejecutable y argumentos del servidor (existe `--disable-sandbox`) | VERIFIED-DOC; F-05 |
| `mcp_servers.node_repl.env_vars` | 1 | MATERIAL_SECURITY | variables del entorno padre reenviadas al servidor | VERIFIED-DOC |
| `mcp_servers.node_repl.startup_timeout_sec` | 1 | MATERIAL_CAPABILITY | decide si el servidor llega a inicializar (10 s por defecto) y, por tanto, si sus herramientas existen en la corrida | VERIFIED-DOC |
| `env.CODEX_CLI_PATH`, `env.NODE_REPL_NODE_PATH`, `env.NODE_REPL_NODE_MODULE_DIRS` | 3 | MATERIAL_SECURITY | identidad del código que ejecuta el servidor | base VERIFIED; subclase INFERRED |
| `env.NODE_REPL_TRUSTED_CODE_PATHS`, `env.NODE_REPL_TRUSTED_SERVICES`, `env.NODE_REPL_UNTRUSTED_ENV_ALLOWLIST` | 3 | MATERIAL_SECURITY | fronteras de confianza del núcleo JS | base VERIFIED; subclase INFERRED |
| `env.SKY_CUA_NATIVE_PIPE`, `env.SKY_CUA_NATIVE_PIPE_DIRECTORY` | 2 | MATERIAL_SECURITY | canal IPC con el componente nativo de uso del ordenador | base VERIFIED; subclase INFERRED |
| `env.CODEX_HOME` | 1 | MATERIAL_SECURITY | raíz de datos de Codex visible para el servidor | base VERIFIED; subclase INFERRED |
| `env.BROWSER_USE_AVAILABLE_BACKENDS`, `env.BROWSER_USE_TINYSKY_ENABLED`, `env.BROWSER_USE_CODEX_APP_BUILD_FLAVOR`, `env.BROWSER_USE_CODEX_APP_VERSION`, `env.NODE_REPL_NATIVE_PIPE_CONNECT_TIMEOUT_MS` | 5 | MATERIAL_CAPABILITY | backends y conmutadores de capacidad, versión y tiempos | base VERIFIED; subclase INFERRED |
| `env.NODE_REPL_INSTRUCTIONS_USE_CASE_BROWSER`, `env.NODE_REPL_INSTRUCTIONS_USE_CASE_CHROME` | 2 | MATERIAL_CONTEXT_OR_OUTPUT | instrucciones de uso que el servidor puede presentar al modelo | base VERIFIED; subclase INFERRED |
| `notify` | 1 | MATERIAL_SECURITY (y CONTEXT_OR_OUTPUT) | programa externo sin sandbox tras cada turno, con la entrada y la salida del modelo y `cwd` como argumento | F-07, F-08 |
| `[shell_environment_policy.set]`, `shell_environment_policy.set.NODE_REPL_TRUSTED_BROWSER_CLIENT_SHA256S` | 2 | MATERIAL_SECURITY | inyecta una variable en el entorno de todos los comandos del modelo; por nombre, una lista de clientes de navegador de confianza | F-18; semántica INFERRED |

**Resumen (hoy, sin versión observada del binario):** 37 MATERIAL_SECURITY + 39 MATERIAL_CAPABILITY + 2 MATERIAL_CONTEXT_OR_OUTPUT + 29 UNKNOWN (los
14 `<redactado>` y 15 que serían NON_MATERIAL_FOR_RECIPE solo con la versión observada dentro del rango verificado) = 107 (39 secciones y 68 claves;
estructura igual desde OD-2c: owner-decision-packets L177, evidencia L2486, L2569, L2880).

**Claves ausentes cuya aparición sería material** (hoy caerían en «cambio estructural» y P-01): `approval_policy`, `approvals_reviewer` (con
AutoReview, exec deja de forzar `never`: VERIFIED-SRC), `sandbox_mode`, `sandbox_workspace_write`, `default_permissions`, `permissions`, `hooks`,
`model_provider(s)`, `openai_base_url`, `chatgpt_base_url`, `instructions`, `developer_instructions`, `model_instructions_file`, `profile(s)`,
`project_doc_*`, `skills`, `memories`, `web_search`, `tools`, `apps`, `browser_use`, `computer_use`, cualquier `mcp_servers.<id>` nuevo y cualquier
clave nueva en `[features]` o `plugins`.

---

## 5. Las 9 claves cambiadas

### 5.1 Hechos

| Actualización | Huella | Binario / app | Claves cambiadas por clave | Fuente |
|---|---|---|---|---|
| 1 | `9002E854…` → `723A6898…` (4 775 → 4 775 bytes) | `8aaf1547b825b104` → `5ea220ae823df3d7`; `26.930.3930.0` → `26.930.7945.0` | **no demostrable**: sin instantánea por clave; consta la etiqueta del binario en `CODEX_CLI_PATH` y «al menos otro valor» | evidencia L2482-2488; owner-decision-packets L247-249 |
| 2 | `723A6898…` → `9EA26634…` (4 775 → 4 777) | `5ea220ae823df3d7` → `979a96ce184041d1`; `26.930.7945.0` → `26.1002.6548.0` | las 9 | evidencia L2568-2570; owner-decision-packets L199 |
| 3 | `9EA26634…` → `6518EFAB…` (4 777 → 4 777) | `979a96ce184041d1` → `9691020b546a15b2`; `26.1002.6548.0` → `26.1002.7124.0` | las mismas 9 | evidencia L2879-2881; `R20261008T2200Z-codex-passive/result.json` |

Hecho de calendario de la actualización 3: binario escrito el 2026-10-07T11:57:39Z y `config.toml` el 2026-10-07T20:05:22Z (VERIFIED-LOCAL,
`result.json`). Durante unas 8 h el binario nuevo convivió con una configuración que, si la hipótesis de §5.3 es cierta, seguía apuntando a la versión
anterior. Es el caso de «actualización a medio completar» de A-2 (L223-224; A62-A2-O2).

### 5.2 Clase de la clave frente a cambio de valor

| Clave | Clase de la clave | Qué cambia probablemente (INFERRED) |
|---|---|---|
| `mcp_servers.node_repl.command` | MATERIAL_SECURITY | ruta del ejecutable del servidor dentro de la instalación versionada |
| `env.BROWSER_USE_CODEX_APP_VERSION` | MATERIAL_CAPABILITY | cadena de versión de la app (evidencia L2488) |
| `env.CODEX_CLI_PATH` | MATERIAL_SECURITY | ruta del binario con su etiqueta (evidencia L2487) |
| `env.NODE_REPL_NODE_MODULE_DIRS` | MATERIAL_SECURITY | directorios de módulos de la instalación |
| `env.NODE_REPL_NODE_PATH` | MATERIAL_SECURITY | intérprete de la instalación |
| `env.NODE_REPL_TRUSTED_CODE_PATHS` | MATERIAL_SECURITY | rutas de código de confianza de la instalación |
| `env.NODE_REPL_TRUSTED_SERVICES` | MATERIAL_SECURITY | servicios de confianza; contiene la versión de la app (evidencia L2488) |
| `env.SKY_CUA_NATIVE_PIPE_DIRECTORY` | MATERIAL_SECURITY | directorio del canal nativo (la tubería `SKY_CUA_NATIVE_PIPE` **no** cambió) |
| `notify` | MATERIAL_SECURITY | ruta del programa notificador de la app |

La clase de la clave no depende de que el valor cambie «solo» por la versión: una clave MATERIAL sigue siéndolo. Lo que el valor puede demostrar es
**cómo** cambió (§5.5), no que la clave deje de importar.

### 5.3 ¿Refresco mecánico de rutas de versión?

**A favor (INFERRED):**
- coincide en el tiempo con tres actualizaciones de la app (VERIFIED-LOCAL);
- estructura idéntica (107 nombres, 39 secciones) en las tres; ninguna clave añadida ni eliminada;
- el mismo conjunto de 9 claves en las dos actualizaciones con instantánea;
- **aritmética de longitudes:** las versiones `26.930.3930.0` y `26.930.7945.0` tienen 13 caracteres, y `26.1002.6548.0` y `26.1002.7124.0`, 14.
  Los tamaños son 4 775 → 4 775 → 4 777 → 4 777: Δ = 0, +2, 0. El +2 de la actualización 2 encaja con +1 por aparición en las dos claves donde
  consta la versión (evidencia L2488). Las demás diferencias no cambian la longitud, lo que encaja con etiquetas de longitud fija (16 hexadecimales en
  `bin\<etiqueta>`).

**En contra o sin demostrar:**
- sin valores no se puede excluir una sustitución de igual longitud (p. ej., una ruta de confianza cambiada por otra de la misma longitud);
- no se sabe si todas las rutas apuntan dentro del paquete firmado de la app o a `%USERPROFILE%` o `%LOCALAPPDATA%`;
- la primera actualización no tiene comparación por clave.

**Veredicto:** compatible con un refresco mecánico, **no verificado**. Se puede verificar sin publicar valores (EXP-MAF-01, §5.5).

### 5.4 Relevancia de seguridad aunque el cambio parezca automático

- `NODE_REPL_TRUSTED_CODE_PATHS` y `NODE_REPL_TRUSTED_SERVICES` definen fronteras de confianza de un ejecutor de código que el modelo puede invocar (F-03).
  Un actualizador que las reescribe puede ampliar el conjunto de confianza en el mismo acto. Que sea «automático» describe quién escribió, no qué se
  autoriza.
- `notify` ejecuta un programa fuera del sandbox tras cada turno y le entrega la entrada y la salida del modelo (F-07). Una ruta nueva es un programa
  nuevo con acceso a esos datos.
- `command`, `NODE_REPL_NODE_PATH`, `NODE_REPL_NODE_MODULE_DIRS` y `CODEX_CLI_PATH` fijan **qué código** corre. Aunque la proyección demuestre que solo
  cambió la versión, el código detrás de la ruta es el de la versión nueva. Esa novedad no la cubre la configuración, y tampoco la sonda: ninguna de
  las 11 propiedades de la clase de A-3 (regla 4) observa las herramientas que expone el servidor lanzado ni lo que hace el programa posterior al
  turno con la entrada y la salida del modelo (OQ-MAF-02..04); la evidencia del cambio de binario y versión solo registra el cambio.
- Conclusión: las tres son relevantes para la seguridad aunque el cambio sea mecánico. La proyección acota el cambio de configuración (sin ampliar
  conjuntos de confianza ni redirigir rutas fuera del paquete). No acredita el código nuevo ni lo hace inocuo. A-3, regla 11, c) exige
  a la A-n que ordene OD-2-MAT que nunca las trate como acopladas a la versión ni como NON_MATERIAL_FOR_RECIPE: designan código que el runtime lanza
  (ejecutable, intérprete, módulos, argumentos o entorno del servidor o programa lanzado, salvo una variable que solo lleve una etiqueta de versión
  verificada) o las fronteras de confianza de ese código. Con A-3, su cambio es un cambio de huella: P-01 y OD-2 exacta (regla 11, a).

### 5.5 Qué podría comprobar un verificador local ciego al valor (propuesta no normativa)

> **Propuesta para la A-n que ordene OD-2-MAT.** A-3 no define este verificador ni autoriza leer valores (regla 11, a y b); su regla 11, c) fija
> los requisitos que tendría que cumplir. Lo que sigue describe el diseño de la investigación, no una regla.

Correría en el host y leería el archivo localmente, **sin publicar valores**; publicaría solo una lista cerrada (punto 5; requisito de A-3, regla
11, c), y solo con una autorización expresa del Owner que nombre el blob del programa, las superficies que lee y su caducidad, que A-3 no pide
(regla 11, b).

1. **Tokens observados de forma independiente** (nunca leídos del propio `config.toml`): versión del paquete de la app y etiqueta del directorio del
   binario verificado (ruta verificada de 16.4). La lista de tipos de token es cerrada (requisito de A-3, regla 11, c). El `InstallLocation` del
   paquete solo se usa en la comprobación PATH_IN_PACKAGE, como comparación y nunca como sustitución; los prefijos del perfil no son tokens: la
   privacidad la da que las proyecciones nunca se publican.
2. **Proyección canónica** π_T(v): se lee el TOML, cada hoja se serializa de forma canónica (arrays con su orden; tablas en línea con claves ordenadas)
   y cada aparición exacta de un token se sustituye por su marcador (`<APPVER>`, `<BINLABEL>`), el más largo primero. Si queda un token
   de una versión anterior acreditada, es un **residuo** y la clave no se explica.
3. **Instantánea de la línea base acreditada**: por cada ruta de clave real p, HMAC_K(p ‖ π_{T_B}(v_B(p))), con la clave K solo local, más un digest
   de la estructura real. La línea base deriva solo de una huella aceptada con OD-2: la instantánea por clave de EV L2490 es de `723A6898…`, huella
   nunca aceptada (DEC L967), y no puede servir de línea base.
4. **Comprobación**: estructura igual; por cada clave cambiada, HMAC de su proyección actual igual a la de la línea base. Además, por clase:
   - **PATH_IN_PACKAGE**: la ruta resuelta cae dentro del `InstallLocation` del paquete con el mismo editor y la misma clase de firma que la línea base;
   - **CLI_BINDS_BINARY**: la proyección de `CODEX_CLI_PATH` vincula la ruta al binario verificado actual (detecta también una actualización a medio
     completar);
   - **TRUST_SET_EQUAL**: implícita en la igualdad de la proyección (mismo número y orden de entradas).
5. **Salida que publicaría** (lista cerrada, como exige A-3, regla 11, c): el modo y el veredicto, con sus causas por nombre de clave; el hash exacto de la
   huella observada (el que ya registra P-01) y el de la huella aceptada de la que deriva la línea base, con el digest de las reglas de la línea base
   (mapa, regla de proyección, tipos de token, conjunto, versiones verificadas y verificador); por clave, su nombre saneado (solo se publican los
   segmentos que figuran literalmente en el mapa cerrado del adapter; cualquier otro, entre comillas simples o dobles o sin ellas, se sustituye por
   un marcador con un digest con clave), su clase, si cambió y si su proyección es igual a la de la línea base; los tipos de token usados; digests
   calculados con una clave secreta solo local del host (nunca un hash sin clave de un valor ni de su proyección); y los hechos de identidad del
   runtime de la línea base y de la observación; ningún otro dato y nunca valores. `InstallLocation` no se publica, ni siquiera por su hash.

**EXP-MAF-01 (sustitución inversa, retroactiva, sin ejecutar Codex).** Sobre el archivo vigente se sustituyen los tokens actuales por los de una versión
anterior aceptada y se compara el SHA-256 del resultado con su huella aceptada. Por ejemplo, con `26.930.3930.0` y `8aaf1547b825b104`, el resultado
debería dar `9002E854…` (OD-2c).
- Igualdad del SHA-256 con `9002E854…` (OD-2c, decisiones L931): refresco mecánico **VERIFIED** desde la línea base aceptada. Es el único resultado
  que da VERIFIED.
- Desigualdad: inconcluso (un token no previsto, p. ej. la versión del runtime de Node); fallo cerrado, sin refutar.
- La comparación por clave con las instantáneas HMAC de `723A6898…` y `9EA26634…` (nunca aceptadas por OD-2) solo localiza el cambio entre dos
  huellas no aceptadas: no acredita el refresco frente a una línea base material, que deriva solo de una huella aceptada (requisito de A-3, regla
  11, c), y su resultado es, como mucho, INFERRED.

Como el `InstallLocation` del paquete contiene la versión de la app, la sustitución de la versión lo cubre (INFERRED; el propio experimento lo pone a
prueba). **Autoridad:** lee valores localmente sin publicarlos, y eso se aparta de los términos con los que el Owner aprobó OD-2 («nunca valores»,
owner-decision-packets L93) y de README L366: solo podría permitirlo una autorización expresa del Owner que nombre el programa y su blob, las
superficies que lee y su caducidad. A-3 no la pide y esta investigación no la propone ahora (paquete de OD-2-MAT, «Lectura de valores (no se pide
ahora)»); la comparación por clave de la medición pasiva (decisiones L1043) no la sustituye, y una confirmación del Coordinator tampoco.

### 5.6 Veredicto de las 9 claves

- **Clase:** material (8 MATERIAL_SECURITY, 1 MATERIAL_CAPABILITY). Ninguna NON_MATERIAL_FOR_RECIPE con la receta 16.4 vigente. Las 8
  MATERIAL_SECURITY designan código lanzado o sus fronteras de confianza: son materiales, y A-3, regla 11, c) prohíbe a cualquier A-n futura tratarlas
  como acopladas a la versión (§5.4); con A-3, una actualización real es un cambio de huella, P-01 y OD-2 exacta. La función de
  `BROWSER_USE_CODEX_APP_VERSION` como etiqueta de versión es INFERRED: sin verificar para la versión observada, cuenta también como dentro (fallo
  cerrado).
- **Cambio de valor:** probablemente un refresco mecánico de versión (INFERRED), verificable sin publicar valores, pero leyéndolos (EXP-MAF-01, que
  solo podría hacerse con una autorización expresa del Owner que nombre el programa y su blob, las superficies y la caducidad).
- **Materialidad del evento:** el cambio va unido a una versión nueva de la app y del binario, así que es material por el código, aunque la
  configuración sea equivalente módulo versión. Se trata como evento de sucesor del runtime: evidencia de binario y versión, bloque A2-P2 con
  autorización de consumo y, en su caso, A-3; la sonda no observa las superficies que esas claves designan (§5.4). **Nunca como variación
  irrelevante.**
- **Mitigación de mayor efecto (no vigente):** neutralizar en la receta `notify` (`-c notify=[]`) y `node_repl`
  (`-c mcp_servers.node_repl.enabled=false`). Con la conducta verificada en `rust-v0.160.1` (F-07, F-17), esas 9 claves podrían pasar a
  NON_MATERIAL_FOR_RECIPE con razón verificada, pero A-3, regla 11, c) exige que un mecanismo futuro nunca trate como no materiales las claves de
  lanzamiento de código: la receta endurecida reduciría la superficie que corre, no la clase de esas claves. Cambia 16.4 (A-n) y exige medir de
  nuevo (§7.6).

---

## 6. Superficies materiales fuera de la huella actual

La huella de `codex-cli` es solo `~/.codex/config.toml` (adapter L30, L40-41). Para la receta 16.4 también son materiales:

| Superficie | Por qué | Cubierta hoy |
|---|---|---|
| binario `codex.exe` | código | `BinaryHash` (invalidador; A-2 L237-239) |
| paquete de la app (servidor `node_repl`, programa `notify`, recursos) | código al que apuntan las claves de §5 | `AppVersion` (invalidador), siempre que las rutas caigan dentro del paquete: no comprobado |
| `%USERPROFILE%\.codex\rules\*.rules` | política de comandos (permitir, preguntar, prohibir); existe `--ignore-rules` | no; D.6 la enumera para B (evidencia L2501) |
| hooks de usuario (`hooks.json` o `[hooks]` en línea) y de plugins | ejecución de comandos en eventos del turno; `features.hooks` activa por defecto | no (que existan: UNVERIFIED; confianza exigida: F-10) |
| `%USERPROFILE%\.codex\AGENTS.md`, `skills\`, memorias | contexto automático | D.6 para el Principal B (evidencia L2500-2502); no en la huella de `codex-cli` |
| contenido de plugins instalados y marketplaces locales | servidores MCP, skills y hooks | no |
| configuración gestionada (`requirements.toml`, MDM, valores por defecto gestionados desde la nube para el workspace) | puede fijar sandbox, aprobación o hooks **sin cambiar ningún archivo local** | no; si aplica a esta cuenta: UNVERIFIED |
| capa `.codex/` del worktree | solo con proyecto de confianza (F-11) | indirecta: árbol Git limpio |
| entorno del proceso hijo (`PATH` con el `pwsh` del runtime primero, `CODEX_HOME`) | resolución de binarios y raíz de configuración | parcial (receta) |
| autenticación | cuenta y consumo | `codex login status` (invalidador) |
| defaults del binario (F-19) | capacidad con config idéntico | `BinaryHash` + sonda |

Conclusión: el hash exacto para en cambios mecánicos y no ve cambios materiales que ocurren fuera del archivo. Una línea base material debe declarar un
**manifiesto de superficies** por adapter, no solo un archivo.

---

## 7. Mecanismo propuesto: MATERIAL_ADAPTER_FINGERPRINT

> **Propuesta no normativa para la A-n que ordene OD-2-MAT.** A-3 no define este mecanismo ni ningún verificador, y no lee valores ni autoriza
> leerlos (regla 11, a y b): solo fija, en su regla 11, c), los requisitos que esa A-n tendría que cumplir. Este diseño es la propuesta de la
> investigación para esa A-n; donde dice «A-3, regla 11, c», se refiere a esos requisitos.

### 7.1 Principios

1. Ninguna aceptación implícita: el mecanismo clasifica y produce evidencia; aceptar una huella o una clase es un acto del Owner.
2. «Más nuevo» no es «mejor»: un sucesor se compara contra el suelo acreditado y nunca hereda por ser posterior.
3. P-01 no se desactiva: bajo la autoridad actual sigue siendo el cambio del hash (AUTOMATION_PLAN L698).
4. Fallo cerrado: toda clave, superficie o token desconocido, todo residuo y toda observación a medio actualizar dan UNKNOWN.
5. Ciego al valor: ningún valor sale del host; solo una lista cerrada (§5.5, punto 5; requisito de A-3, regla 11, c).
6. Sin ampliación: el mecanismo nunca escribe configuración, nunca cambia flags de la receta y nunca autoriza consumo.

### 7.2 Componentes (frontera neutral, AUTOMATION_PLAN 16.17)

| Componente | Dónde vive | Contenido |
|---|---|---|
| términos neutrales | núcleo (sin nombres de proveedor) | `MaterialSurface`, `KeyClass` (las cinco clases), `VersionCoupledToken`, `AccreditedMaterialBaseline` (AMB), `MafVerdict` ∈ {IDENTICAL, VERSION_COUPLED, MATERIAL_CHANGE, UNKNOWN}, regla de fallo cerrado y relación con P-01/P-11, OD-2, A2-P2 y A-3 |
| manifiesto de superficies | evidencia custodiada por blob, fijada y cambiada solo con la autoridad que declare la A-n de OD-2-MAT (requisito de A-3, regla 11, c) | archivos y fuentes materiales (§6), con su lector |
| mapa de clases | ídem | patrones de ruta de clave → clase. Cerrado: sin coincidencia, UNKNOWN. Cada NON_MATERIAL_FOR_RECIPE con razón (URL y rango de versiones) y override de la receta si lo hay; la función de cada clave con su fuente y su rango de versiones; conjunto acoplado a la versión, sin claves de lanzamiento de código ni fronteras de confianza (A-3, regla 11, c) |
| tipos de token | ídem | tipo, fuente independiente (gestor de paquetes, ruta verificada) y marcador |
| verificador | evidencia (script versionado por blob) | implementación de §5.5, con autoprueba de vectores y mutantes como `a3-guards.py` |
| AMB | host (HMAC) + evidencia (digests) | derivada **solo** de una huella ya aceptada por OD-2, con su `BinaryHash`, `AppVersion`, versión de la CLI, blob del mapa y blob de la receta |
| registro `maf-record` | evidencia custodiada | veredicto y datos saneados de §5.5 |

Para `claude-cli` u otro adapter se aplica lo mismo con su manifiesto (archivo de ajustes, configuración MCP, hooks): el núcleo no cambia.

### 7.3 Algoritmo y veredictos

1. **Precondiciones.** Hay una AMB derivada de una huella aceptada. El par (`BinaryHash`, `AppVersion`) está observado y **estable**: misma huella en al
   menos dos observaciones separadas, un solo binario y ningún proceso de actualización activo (A62-A2-O2). Si no se cumplen: UNKNOWN.
2. Hash exacto igual al de la AMB y cada superficie del manifiesto igual a la de la AMB → **IDENTICAL** (P-01 actual satisfecho).
3. Estructura real distinta (clave añadida, eliminada o renombrada) → **UNKNOWN**.
4. Por cada clave con valor cambiado:
   - NON_MATERIAL_FOR_RECIPE con razón y función válidas para la versión observada → no cuenta;
   - del conjunto VERSION_COUPLED, con su función verificada para la versión observada, proyección igual y las comprobaciones de su clase
     superadas → explicada;
   - MATERIAL_* en otro caso → **MATERIAL_CHANGE**;
   - UNKNOWN → **UNKNOWN**.
5. **VERSION_COUPLED** solo si todas las claves cambiadas quedan explicadas, hubo un cambio observado de `BinaryHash` o `AppVersion`, los tokens se
   observaron de forma independiente y no hay residuos. **Un cambio de configuración sin cambio de versión nunca es VERSION_COUPLED.**
6. El cambio de una superficie del manifiesto distinta de `config.toml` se clasifica como el de una clave (paso 4): MATERIAL_CHANGE si es
   material y no se explica, UNKNOWN si el mapa no la tiene o no se pudo observar; con la huella igual, nunca es IDENTICAL.
7. Ninguna escritura, ninguna aceptación; se publica el `maf-record`.

### 7.4 Correspondencia con los seis elementos del punto 4

| Elemento (s57 L36-41) | Cómo lo conserva el mecanismo |
|---|---|
| evidencia del cambio de binario y versión | el `maf-record` lleva solo los hechos de identidad del runtime de la línea base y de la observación (como la evidencia por nombre de A-3, regla 11, b); VERSION_COUPLED exige ese cambio observado y estable |
| clasificación de claves cambiadas | mapa cerrado sobre nombres reales resueltos localmente; publicación saneada |
| comparación con propiedades materiales acreditadas | igualdad de proyección contra la AMB (configuración) y suelo `RUNTIME_COMPATIBILITY_CLASS` de A-3 (runtime) |
| sonda rápida con presupuesto autorizado | la de A2-P2 (≤ 2 de solo lectura por par, con autorización por bloque) o la de A-3; el mecanismo no crea presupuesto |
| fallo cerrado | UNKNOWN para todo lo desconocido; con UNKNOWN o MATERIAL_CHANGE rige P-01 sin cambio |
| sin ampliación de permisos o consumo | la igualdad de proyección impide entradas de confianza nuevas. `plugins.*.enabled`, `marketplaces.*`, `service_tier`, `windows.sandbox`, `projects.*`, `shell_environment_policy.*` y toda clave nueva son MATERIAL o UNKNOWN; las claves de lanzamiento y sus fronteras de confianza nunca son acopladas a la versión ni no materiales (requisito de A-3, regla 11, c). El mecanismo no escribe ni autoriza |

### 7.5 Composición con A-2 y A-3

- **A2-P2 (acordada, sin cambio):** una actualización de la app es un evento de sucesor. VERSION_COUPLED no sustituye el bloque: sigue haciendo falta
  la disposición del Coordinator que nombra el par, la autorización de consumo del Owner y la aceptación OD-2 (A-2 L222-227). El `maf-record` se anexa a
  esa disposición y al paquete OD-2.
- **A-3 (candidata):** `SUCCESSOR_COMPATIBLE` es independiente de la huella (A-3 §3.6). El mecanismo añade la dimensión de configuración que A-3 deja
  fuera. Como la fase 2 es OWNER-RESERVED y A-3 §8 dice «esta enmienda se detiene» ante una consecuencia reservada, **la fase 2 no debe entrar en el
  delta de A-3**: va en una A-n aparte (p. ej., A-4) con su paquete del Owner. En A-3 cabe, como mucho, citar esta investigación y añadir a la sonda las
  observaciones del punto siguiente (§7.5).
- **Observaciones que conviene añadir al próximo bloque autorizado** (sin consumo nuevo: la misma invocación de solo lectura): si `node_repl` arranca
  (árbol de procesos de `codex.exe`, eventos del registro de sesión), qué herramientas MCP se exponen, si se lanza `notify`, qué programa y si sobrevive
  a `codex.exe`; `codex --version`; y, sin modelo, si `codex exec --help` lista `--ignore-user-config`, `--ignore-rules` y `--strict-config`.

### 7.6 Fases y opciones

| Fase | Contenido | Autoridad |
|---|---|---|
| **0 (ahora)** | la evidencia por nombre de A-3, regla 11, b) (hash exacto antes y después con su estabilidad, binario y versión, nombres añadidos o eliminados y estructura) en cada paquete OD-2; P-01 literal; el Owner sigue aceptando el hash exacto; ningún verificador ni lectura de valores | ninguna nueva |
| **1** | verificador como herramienta de evidencia: términos neutrales; manifiesto, mapa, tipos de token y verificador custodiados por blob, con sus guardas; lee valores en el host | solo dentro de la A-n que ordene OD-2-MAT (requisitos de A-3, regla 11, c) y con una autorización expresa del Owner que nombre el programa y su blob, las superficies y la caducidad (A-3, regla 11, b); llevarlo al descriptor con efecto de aceptación es de la fase 2 |
| **1b (opcional)** | **receta endurecida** (`-c notify=[]`, `-c mcp_servers.node_repl.enabled=false` y, si el Architect lo pide, `-c features.plugins=false` y `-c features.apps=false`). Estrecha la capacidad, pero cambia 16.4 y el runtime medido (nueva medición). `--ignore-user-config` **no** se recomienda: dejaría de heredar `service_tier` (contra 16.4 L543), `windows.sandbox` y la confianza | A-n material (M-03, M-04): Architect + Coordinator |
| **2** | aceptación por clase VERSION_COUPLED (decisión del Owner OD-2-MAT; el borrador OD-2M de §8.4 está superado); P-01 para unidades I62 pasa a dispararse con un veredicto ∉ {IDENTICAL, VERSION_COUPLED aceptado} | decisión del Owner + A-n material (M-04; M-08 posible por ADR-0046 L95) |

**Recomendación:** fase 0 ya; las fases 1 y 2, solo si el Owner encarga esa A-n (OD-2-MAT), tras EXP-MAF-01 con su autorización de lectura y tras un
bloque autorizado que despeje OQ-MAF-02..05; la 1b como pregunta al Architect en la revisión de A-3, pero fuera de su delta. Mientras tanto, el Owner
puede pausar las actualizaciones automáticas de la app durante los pilotos (owner-decision-packets L204-205): reduce los P-01 sin cambiar ninguna
autoridad.

### 7.7 Lo que el mecanismo nunca hace

Editar `config.toml`; crear entradas de confianza; cambiar `windows.sandbox`, credenciales o permisos; aceptar una huella o una clase; autorizar
consumo; anticipar un par no observado; tratar un sucesor como mejor; reutilizar una sonda entre pares; publicar valores o rutas con el nombre del
usuario.

---

## 8. ¿Puede OD-2 evolucionar a una línea base material?

### 8.1 Viabilidad técnica

Sí, para la clase VERSION_COUPLED: la verificación ciega al valor de §5.5 puede demostrar, sin publicar valores, que la diferencia con una línea base
aceptada es exactamente una sustitución de tokens de versión observados de forma independiente. Ese veredicto no acredita el código nuevo ni lo hace
inocuo, y el requisito de A-3, regla 11, c) deja fuera de esa clase las claves de lanzamiento de código y sus fronteras de confianza: con los hechos
de hoy, ninguna actualización real de `codex-cli` es VERSION_COUPLED (§5.6). Para claves NON_MATERIAL_FOR_RECIPE también, con razón verificada y
versión observada. Para cualquier otro cambio, no: requiere decisión.

### 8.2 Análisis de autoridad

| Punto | Fuente | Consecuencia |
|---|---|---|
| OD-2 es una decisión del Owner y su texto rechaza aceptar la huella nueva | V14 L971; decisiones L890 | cambiar qué se acepta es «decision Owner» → **OWNER-RESERVED** (LIFECYCLE L94) |
| P-01 se define como cambio del hash; ADR-0046 aceptado lo registra | AUTOMATION_PLAN L522-523, L698; ADR-0046 L95 | redefinir el disparador es M-04 (y M-08 posible) → A-n material, Architect + Coordinator (LIFECYCLE L221) |
| A-2 regla 3: autorización por bloque, nunca permanente ni anticipada | A-2 L222-226 | la evolución **no** puede eliminar el acto de consumo por actualización sin otra A-n que sustituya esa regla (A-2 no se edita) |
| A-2 regla 4: el Owner acepta con OD-2 «la huella exacta resultante» | A-2 L227 | ambiguo si una regla de clase preaprobada cumple «acepta (…) la huella exacta»: pregunta para el Architect y el Owner (OQ-MAF-09) |
| precedente: OD-2b/A2 rechazada por demasiado amplia | decisiones L913 | la propuesta debe distinguirse de A2 (§8.3) y presentarse como frontera nueva del Owner, no como reinterpretación de OD-2 |
| la orden prohíbe desactivar P-01 bajo la autoridad actual | s57 L33 | hasta la decisión del Owner y la A-n, P-01 literal en producción |

### 8.3 Diferencias con la opción A2 rechazada (decisiones §47)

| A2 (rechazada) | MAF |
|---|---|
| delta de **nombres** saneados | igualdad de **valores proyectados**, verificada localmente por HMAC |
| aceptaba el hash futuro de antemano | solo tras observar el par estable; nunca anticipada (A-2 regla 3) |
| no ligaba el cambio a una versión | exige un cambio observado de `BinaryHash` o `AppVersion` y tokens de fuente independiente |
| sin sonda | unida al bloque A2-P2 autorizado y, en su caso, a `SUCCESSOR_COMPATIBLE` |
| sin mapa de clases | mapa cerrado; lo desconocido es UNKNOWN |

### 8.4 Borrador de decisión del Owner (no decidida; para preparar, no para resolver)

> **Superado por el paquete OD-2-MAT (od2-material-baseline-owner-packet.md), que es el único que se presenta al Owner; la correspondencia de
> opciones está en el anexo de A-3, §8. Este borrador queda como antecedente y no se decide. Además, A-3, regla 11, c) exige que las claves de
> lanzamiento de código y sus fronteras de confianza nunca sean acopladas a la versión: 8 de las 9 claves de §5 quedarían fuera del conjunto que
> proponía B.**

`OD-2M` — línea base material de `codex-cli`. Opciones:
- **A (statu quo reforzado):** OD-2 sigue aceptando hashes exactos; cada paquete OD-2 lleva un `maf-record`, con la autorización del Owner para el
  verificador sobre valores (§5.5).
- **B (clase VERSION_COUPLED):** el Owner acepta de antemano como huella OD-2 toda huella con veredicto VERSION_COUPLED frente a la AMB vigente, con
  estas condiciones: par observado y estable; tokens de la lista cerrada aprobada; conjunto de claves VERSION_COUPLED aprobado (las 9 de §5); sin
  residuos; comprobaciones PATH_IN_PACKAGE y CLI_BINDS_BINARY superadas; bloque A2-P2 autorizado por el Owner y completo, sin cambio de huella durante
  él. Todo lo demás sigue en P-01. Requiere la A-n de la fase 2.
- **C:** B más la receta endurecida de la fase 1b, que reduce el conjunto VERSION_COUPLED a lo que la receta no neutraliza.
- **D (acción propia del Owner, compatible con A, B o C):** pausar las actualizaciones automáticas de la app durante los pilotos.

Recomendación de la investigación: **A + D ahora**; preparar B o C cuando EXP-MAF-01 y el próximo bloque autorizado despejen OQ-MAF-01..05.

### 8.5 Respuesta a la pregunta de la orden

OD-2 **puede** evolucionar hacia una línea base material verificable, pero esa evolución modifica autoridad reservada al Owner y una regla definida
como cambio del hash. Por tanto: (1) necesita una decisión del Owner (hoy, el paquete OD-2-MAT, que supera el borrador OD-2M de §8.4) y una A-n
material, no una disposición ordinaria; (2) aun con una aceptación por clase, cada actualización conserva un acto del Owner, el consumo del bloque de
sondas (A-2 L224-226, L304). Lo que desaparecería es la decisión **separada** de OD-2 por cada huella con veredicto VERSION_COUPLED (que no la hace
inocua; con los hechos de hoy, ninguna actualización real de `codex-cli` lo tendría).

---

## 9. Reservado al Owner

- Aceptar una huella (OD-2), y adoptar una línea base material o una clase (OD-2-MAT; el borrador OD-2M de §8.4 está superado), con su lista de
  tokens y su conjunto VERSION_COUPLED.
- Toda clasificación NON_MATERIAL_FOR_RECIPE que cambie cuándo para P-01.
- El consumo de cada bloque de sondas (A-2 §7) y cualquier presupuesto permanente (hoy prohibido por A-2 regla 3).
- Pausar las actualizaciones automáticas de la app; revisar valores en su propio archivo (opción B del paquete OD-2d, owner-decision-packets L256).
- Cualquier cambio de `windows.sandbox`, confianza, plugins o marketplaces en su configuración (nunca la sesión).
- EXP-MAF-01 y el verificador sobre valores, con su clave durable por host: leen valores (términos de OD-2, owner-decision-packets L93; README L366),
  así que exigirían una autorización expresa del Owner que nombre el programa y su blob, las superficies y la caducidad; hoy no se pide.

No reservado: esta investigación, el verificador como herramienta de evidencia, la redacción de la A-n y las guardas (Coordinator; Architect para la
materialidad).

## 10. Preguntas abiertas

| Id | Pregunta |
|---|---|
| OQ-MAF-01 | Versión del binario vigente `3553cd6e…`. Todo lo verificado en código vale para `rust-v0.160.1` y `main`; el binario vigente exige `codex --version` en un bloque autorizado |
| OQ-MAF-02 | ¿Arranca `node_repl` en la corrida de la receta en este host? ¿Qué herramientas expone? (árbol de procesos y registro de sesión en la sonda) |
| OQ-MAF-03 | ¿Se lanza `notify` tras el turno de exec con el binario vigente? ¿Qué programa es? ¿Sobrevive a `codex.exe` y choca con las comprobaciones de procesos de 16.4 (F-08)? |
| OQ-MAF-04 | ¿Se admiten llamadas de `node_repl` con aprobación `never` y `-s read-only` (anotaciones de la herramienta, modo de aprobación por defecto, función de autoaprobación)? |
| OQ-MAF-05 | ¿Es «aislada» la sesión de exec (`SessionIsolation`)? Afecta a `codex_apps` y al descubrimiento de servidores |
| OQ-MAF-06 | ¿Los 14 `<redactado>` son 12 `trust_level` y 2 claves `perPath`? El verificador local puede responder sin publicar nombres |
| OQ-MAF-07 | ¿Basta la lista de tokens (versión de la app y etiqueta del binario) para explicar las 9 claves, o hay otro token (p. ej., la versión de Node)? Lo responde EXP-MAF-01 |
| OQ-MAF-08 | ¿Hay hooks de usuario, reglas `.rules` relevantes o configuración gestionada (incluida la de la nube) para esta cuenta? |
| OQ-MAF-09 | ¿Una regla de clase preaprobada por el Owner cumple A-2 regla 4 («acepta con OD-2 la huella exacta resultante»), o hace falta una A-n que la sustituya? |
| OQ-MAF-10 | ¿Redefinir P-01 para unidades I62 activa M-08 respecto de ADR-0046 L95 (sección «Vigilar»), o basta M-04? |
| OQ-MAF-11 | ¿Quién reproduce el veredicto en la aceptación si solo el host puede leer los valores? (A-3 regla 5 exige que no sea una autodeclaración de la sesión que sondea) |
| OQ-MAF-12 | ¿Conviene endurecer la receta (fase 1b) aunque OD-2 no evolucione, para quitar `node_repl` y `notify` de la superficie de un Controller o un Architect de solo lectura? |

## 11. Fuentes

**Proveedor (consultadas el 2026-10-08; las páginas de `developers.openai.com/codex/...` redirigen a `learn.chatgpt.com/docs/...`):**
- Referencia de configuración: https://learn.chatgpt.com/docs/config-file/config-reference
- Configuración básica (precedencia, confianza): https://learn.chatgpt.com/docs/config-file/config-basic
- Configuración avanzada (`-c`, perfiles, `notify`): https://learn.chatgpt.com/docs/config-file/config-advanced
- MCP: https://learn.chatgpt.com/docs/extend/mcp?surface=cli
- Hooks: https://learn.chatgpt.com/docs/hooks
- `exec` (tag): https://raw.githubusercontent.com/openai/codex/rust-v0.160.1/codex-rs/exec/src/lib.rs
- `exec` (`main`): https://raw.githubusercontent.com/openai/codex/main/codex-rs/exec/src/lib.rs
- Flags de `exec`: https://raw.githubusercontent.com/openai/codex/rust-v0.160.1/codex-rs/exec/src/cli.rs
- Arranque MCP: https://raw.githubusercontent.com/openai/codex/rust-v0.160.1/codex-rs/core/src/session/mcp_runtime.rs
- Exposición MCP: https://raw.githubusercontent.com/openai/codex/rust-v0.160.1/codex-rs/core/src/mcp_tool_exposure.rs
- Aprobación MCP: https://raw.githubusercontent.com/openai/codex/main/codex-rs/core/src/mcp_tool_call.rs
- Servidores efectivos y plugins: https://raw.githubusercontent.com/openai/codex/main/codex-rs/core/src/mcp.rs
- Registro de hooks: https://raw.githubusercontent.com/openai/codex/rust-v0.160.1/codex-rs/hooks/src/registry.rs
- `notify` heredado: https://raw.githubusercontent.com/openai/codex/rust-v0.160.1/codex-rs/hooks/src/legacy_notify.rs
- Despacho tras el turno: https://raw.githubusercontent.com/openai/codex/rust-v0.160.1/codex-rs/core/src/session/turn.rs
- Esquema de configuración: https://raw.githubusercontent.com/openai/codex/rust-v0.160.1/codex-rs/core/config.schema.json
- Features (tag): https://raw.githubusercontent.com/openai/codex/rust-v0.160.1/codex-rs/features/src/lib.rs
- Features (`main`): https://raw.githubusercontent.com/openai/codex/main/codex-rs/features/src/lib.rs
- PR del campo `client` de `notify`: https://github.com/openai/codex/pull/12968
- Issue de `node_repl` y `codex/sandbox-state-meta`: https://github.com/openai/codex/issues/29338
- Tags publicados: https://github.com/openai/codex/tags

**Autoridades y evidencia de I-62** (worktree en `b0884216`): `docs/AUTOMATION_PLAN.md` (16.4, 16.9, 16.17, 16.19);
`docs/adr/0046-protocolo-de-ejecucion-delegada-de-agentes.md`; `docs/initiatives/I-62-proposal-v14.md` (§7, §13, §18, D.3);
`docs/initiatives/I-62-A-2.md` (blob `f1e1d6f0…`); `docs/INITIATIVE_LIFECYCLE.md` (§3, §6); `docs/automation/decisions/I-62.md` §46-§57;
`docs/automation/evidence/I-62-evidence.md` §64-§87; `docs/automation/evidence/I-62-prep/owner-decision-packets.md` (OD-2, OD-2b, OD-2c, OD-2d);
`docs/automation/evidence/I-62-F6/OD-2/*/result.json` y `OD-2b-PROBE/*/probe*-events.jsonl`; `docs/automation/agent-execution/adapters/codex-cli.md`;
`docs/automation/agent-execution/README.md` §13. Candidata A-3 de la preparación (`I-62-A-3.md`, §3.2, §3.6, §8), sin modificar.
