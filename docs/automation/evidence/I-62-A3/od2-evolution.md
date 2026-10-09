# I-62 — OD-2: de la huella exacta a una línea base material (investigación MATERIAL_ADAPTER_FINGERPRINT)

```text
Documento:   investigación de preparación para A-3. No es una enmienda, no es una disposición y no decide nada.
Orden:       disposición del Coordinator, decisiones §57, punto 4, dos últimas frases (texto pegado, 3 654 bytes,
             SHA-256 00a13ae527863117472bc7356458c849a8ef6f6e6255b417ee736dc3428023a3; custodiado en DEC L1115)
Autoridades: solo lectura, worktree %USERPROFILE%\.codex\worktrees\architecture-portabilidad-coordinador-principal,
             HEAD b088421649204742ff682e1eb1ebdafc762823d0
Fecha:       2026-10-08
Estado:      codex-cli en STOP P-01 (huella vigente 6518EFAB… sin aceptar por OD-2)
Paquete:     od2-material-baseline-owner-packet.md (este directorio), decisión del Owner OD-2-MAT, pendiente
```

**Siglas de cita** (todas sobre HEAD `b0884216`; «L» = número de línea del blob):

| Sigla | Archivo | Blob |
|---|---|---|
| V14 | `docs/initiatives/I-62-proposal-v14.md` | `34ad80ea` |
| FRZ | `docs/initiatives/I-62-consensus-freeze.md` | `0f6b8608` |
| A2 | `docs/initiatives/I-62-A-2.md` (AGREED, no se modifica) | `f1e1d6f0` |
| LC | `docs/INITIATIVE_LIFECYCLE.md` | `f19896a8` |
| AP | `docs/AUTOMATION_PLAN.md` | `f525cb1e` |
| RD | `docs/automation/agent-execution/README.md` | `592dcfd4` |
| DESC | `docs/automation/agent-execution/adapters/codex-cli.md` | `155f3469` |
| PRE | `docs/automation/agent-execution/schemas/preflight.v1.schema.json` | `6a054079` |
| RR2 | `docs/automation/agent-execution/schemas/relay-record.v2.schema.json` | `dca5b29c` |
| DEC | `docs/automation/decisions/I-62.md` | `27591f16` |
| EV | `docs/automation/evidence/I-62-evidence.md` | `6a2b1569` |
| PKT | `docs/automation/evidence/I-62-prep/owner-decision-packets.md` | `cdb86114` |
| REQ | `docs/automation/evidence/I-62-F6/requests/F6-recovery-request-2026-10-08.md` | `8fb8ec55` |
| PAS | `docs/automation/evidence/I-62-F6/OD-2/R20261008T2200Z-codex-passive/result.json` | `bda544f0` |
| ADR46 / ADR48 | `docs/adr/0046-…md` / `docs/adr/0048-…md` | `da275e1a` / `e1bd8d91` |
| A3 | `docs/initiatives/I-62-A-3.md` (candidata, publicada en el mismo commit que esta investigación; se cita por sección y regla, nunca por línea) | — (A-3 fija el blob de esta investigación; fijar aquí el de A-3 crearía una dependencia circular) |
| KN | archivo saneado de nombres `codex-cfg-keynames-6518EFAB.json` (solo nombres saneados, sin valores; custodia local, no publicado: la clasificación por nombre no se puede reproducir desde el commit; el recuento de 107 nombres consta en PAS) | SHA-256 `a349bea7…` |

## 0. Conclusiones

1. **OD-2 hoy** es una decisión del Owner (V14 §18) que acepta **una huella concreta y exacta** del adapter `codex-cli`: el SHA-256 de todo
   `~/.codex/config.toml`, con sus nombres de clave saneados. Las dos aceptaciones registradas son valores exactos (`091540ED…`, DEC L890;
   `9002E854…`, DEC L931). Desde A-2 (AGREED), cada bloque de A2-P2 termina en una OD-2 sobre «la huella exacta resultante» (A2 L227).
2. **P-01 depende de OD-2** de dos maneras: la línea base aceptada es el valor de referencia antes de cada invocación (DEC L890; DESC L23), y la
   única salida de un P-01 es otra decisión del Owner (AP L522-523). El STOP vigente es del primer tipo: `6518EFAB…` no está aceptada (PAS).
3. **Una línea base material verificable es posible en abstracto, pero no con los hechos de hoy.** Las 9 claves que cambian en cada actualización son
   todas, por su semántica documentada, claves que designan ejecutables que Codex lanza, el entorno de ese proceso o el comando de notificación
   (§2.2). Las únicas familias plausiblemente inocuas (`desktop.*`, `tui.*`, 11 nombres) no han cambiado nunca. La clase volátil y la clase inocua son
   disjuntas: proyectar por nombre de clave aceptaría cambios de valor que nadie ha visto en claves de lanzamiento y de confianza. Según la
   investigación de la huella material (anexo de A-3, §5), 8 de esas 9 claves designan código lanzado o sus fronteras de confianza, materiales: con
   los hechos de hoy, no se demuestra ninguna clase de cambio irrelevante (A-3 no define clases ni verificador: su regla 11 compara la huella solo por
   su valor exacto).
4. **Autoridad del Owner: SÍ.** Una línea base material pre-acepta por regla una clase de artefactos futuros, mueve la aceptación de cada instancia
   del Owner a una regla que evalúa la sesión, contradice la frontera fijada por el Coordinator (DEC L913) y el texto acordado de A-2 (A2 L227), y
   cambia la semántica de las salvaguardas del host que el Owner autorizó con OD-5 (V14 L910-913). Es materia OWNER-RESERVED (LC L94, L221) y no
   puede resolverse por disposición ordinaria del Coordinator (DEC L1115).
5. **Freeze: SÍ.** Exige una A-n MATERIAL con consecuencia OWNER-RESERVED (Owner, Architect independiente y Coordinator). **Vehículo: una A-4
   separada**, no dentro de A-3 (§5.3).
6. **Hasta que el Owner decida:** OD-2 sigue siendo el hash exacto; P-01 no cambia; `codex-cli` sigue en STOP; A-3 conserva su exclusión de todo
   cambio de huella (A3 regla 1, c; Q-A3-10). La ruta de F6 sigue siendo `BLOQUE-CODEX-CONSUMO` y, después de medir, una OD-2 exacta (REQ L50, L62).
7. **Paquete preparado:** `od2-material-baseline-owner-packet.md`, con la respuesta `OD-2-MAT = A | B | C | D (…)`. Ninguna opción autoriza leer
   valores, y el paquete no pide ninguna lectura de valores (sección «Lectura de valores (no se pide ahora)»).
8. **Corrección de una premisa de la tarea:** la localización por clave de las 9 claves está **medida en dos** de las tres actualizaciones (EV
   L2568-2570; EV L2879-2882; PAS). En la primera (`9002E854…` → `723A6898…`) solo se probó la misma estructura y al menos dos valores cambiados, sin
   poder decir cuáles (EV L2486-2488).

## 1. Qué es OD-2 hoy

### 1.1 Quién decide

El Owner. La tabla «Decisiones del Owner» de V14 §18 (V14 L965-975) la enumera:

**V14 L971**

> | OD-2 | línea base de huella por adapter (`config.toml`: `37DD3559…` registrada frente a `42E15A03…` observada) | toda invocación afectada (`codex-cli` en A, B y FX-04b; `codex-desktop-session` si comparte) | antes de la primera invocación afectada |

**V14 L977**

> Cada solicitud va aparte, con la ruta y el hash actuales, el efecto, el consumo, el alcance y las restricciones. El silencio no es decisión.

**LC L94** y **LC L221**

> Un cambio de alcance, no-objetivos, decision Owner o retiro de escenario OV es `OWNER-RESERVED`.

> Una M material exige Architect + Coordinator; una materia OWNER-RESERVED exige Owner.

**RD L422-423** (procedimiento I62 materializado e inactivo):

> materializa. Si ninguna candidata satisface la autorización: sin invocación, STOP y COORDINATOR_DECISION, o ESCALATION_OWNER cuando lo que falta es
> materia del Owner (autenticación, huella, compra).

El Coordinator no acepta huellas de `codex-cli`. Su única autoridad sobre huellas es aceptar en F2 una huella «ninguna» (`Kind` NONE), que no
aplica a un adapter que declara `config.toml` (V14 L330-331; PRE L273). En el ciclo de F6 el Coordinator dispuso el procedimiento y excluyó
expresamente que su orden implicara aceptación (DEC L915: «no se infiere aceptación del Owner de esta orden»).

### 1.2 Qué acepta

| Aspecto | Contenido | Fuente |
|---|---|---|
| Objeto | la **huella** del adapter: para `Kind` CONFIG_FILE, el SHA-256 del archivo y sus nombres de sección y clave, sin valores y saneados | AP L1055-1056; RD L303; PRE L275-281; DESC L40 |
| Forma de la aceptación | un valor exacto; si la huella cambia, P-01 y STOP **sin aceptar la nueva** | DEC L890; DEC L931; DEC L949 |
| Frontera fijada por el Coordinator | «OD-2 acepta una huella concreta del adapter»; aceptar por delta un hash futuro «es demasiado amplio para esa frontera» | DEC L913 |
| Binario | las dos aceptaciones decididas (§46, §48) solo fijan el SHA-256 del archivo. Desde OD-2d, el paquete pide también el binario exacto (`OD-2d = A (línea base <huella medida>; binario <SHA-256 medido>)`, PKT L212; REQ L50), A-2 liga cada bloque a un par exacto (A2 L222-223) y el `BinaryHash` es invalidador de la observación medida (DEC L1068) | PKT L212; A2 L222-227; DEC L1068 |
| Términos de seguridad bajo los que el Owner aceptó | «solo el hash del archivo y nombres saneados; nunca valores; `auth.json` no se lee» | PKT L93 |
| Lo que la aceptación no autoriza | «aceptar en silencio otra huella: si el archivo difiere en la invocación […], es **P-01 → STOP** y una solicitud nueva de OD-2 con el valor nuevo» | PKT L91 |

Literales de las dos decisiones:

**DEC L890**

> | **OD-2** | **APPROVED / A** | huella aceptada de `~/.codex/config.toml`: `091540ED2DE6CFAC5C12110337305FFCABCEEEE0D8CB696F7E04EA057F9F3D66`; antes de cada invocación afectada se recalcula; si cambia, P-01 y STOP del transporte afectado, sin aceptar la nueva huella |

**DEC L931** (extracto)

> el Owner declara en este mensaje que OD-2c «sigue aprobada» con la línea base exacta `9002E854457F2DBE074B66FB804FC24AA9B489C58436753CE74BCBA2BF767B1E`; […] Cualquier cambio posterior de la huella sigue siendo P-01 / STOP

**A2 L227** (enmienda AGREED)

> > 4. el Owner acepta con OD-2 la huella exacta resultante antes de cualquier invocación afectada;

### 1.3 Dónde está definida

| Capa | Cita | Qué fija |
|---|---|---|
| Freeze (V14) | L330-331 (§7), L777 (P-11), L910-913 (OD-5: salvaguardas del host), L940 (F2: «huellas aceptadas»; «OD-2 solo para medir **invocando** `codex-cli`»), L971 y L977 (§18), L991 (riesgo 7), L2443 (D.3, «con OD-2»), L2603 (D.7), L3290 (G.1: P-01 conservado) | OD-2 como decisión del Owner por adapter; P-01 conservado y generalizado por P-11 |
| A-2 (AGREED) | L42-44, L222-227 (reglas 3 y 4 de A2-P2), L269 (M-01: «el Owner ya acepta las huellas (OD-2)»), L301-304 (§7) | por bloque: consumo del Owner y OD-2 sobre la huella exacta resultante |
| I-61 activo | AP L518-519, L522-523 (16.4), L665 y L698 (16.9, 16.11) | P-01 = cambio del hash de `config.toml`; STOP hasta que decida el Owner |
| I62 materializado e inactivo | AP L1055-1056 (16.19); RD L112-113, L303, L366, L422-423; DESC L23, L30, L40-41; PRE L261-322; RR2 L774-776 | huella = SHA-256 + nombres; P-11; invocar sin línea base aceptada exige OD-2 |
| Paquetes | PKT L80-96 (OD-2), L132-166 (OD-2b, sustituido), L168-188 (OD-2c), L190-216 (OD-2d vigente), L218-267 (OD-2d obsoleta) | la forma exacta de cada solicitud |
| Decisiones | DEC L890 (§46), L913 y L915 (§47), L931 (§48), L949 (§49), L967 (§50), L989 (§51), L1065-1070 (§55), L1100 (§56), L1115 (§57) | aceptaciones exactas, frontera de OD-2, OD-2d no aprobada, OD-2d-PROBE, A-2, A-3 |
| ADR | ADR46 L95 (aceptado: «un cambio de su hash detiene las invocaciones»); ADR48 L5 (Owner, «único que lo acepta (OD-1)») y L134 | la consecuencia vigilada y el sucesor para las unidades I62 |

### 1.4 Cómo depende P-01 de OD-2

Hay dos comprobaciones distintas:

| Comprobación | Regla | Referencia | Salida |
|---|---|---|---|
| **(a) Cesión** (salida frente a entrada de una invocación) | AP L522-523 (I-61 activo); V14 L777 y RR2 L776 (P-11, I62) | la huella de la salida | AP L522-523: «ninguna invocación de Codex más hasta que decida el Owner» |
| **(b) Línea base** (antes de cada invocación afectada) | DEC L890; DEC L949; DESC L23: «Con la huella sin línea base aceptada, invocar exige OD-2» | la huella aceptada por OD-2 | una OD-2 nueva sobre el valor nuevo (PKT L91) |

**AP L522-523**

> 3. **Entrada:** el participante y sus descendientes terminaron; las comprobaciones de `WORKFLOW.md` §3; y la comparación de `config.toml`, cuyo cambio es STOP (P-01): ninguna invocación
>    de Codex más hasta que decida el Owner, y se registra el diff de nombres de secciones y claves.

OD-2 es, por tanto, el valor de referencia de (b) y la única salida de (a) y (b). En F6, esas comparaciones son además las salvaguardas del host que
autoriza el Owner (V14 L910-911). El STOP vigente es de tipo (b): ninguna invocación de la sesión cambió la huella; la cambió la app (PAS).

### 1.5 Cadena de huellas y estado vigente

| Huella (`config.toml`) | Origen del cambio | Estado | Fuente |
|---|---|---|---|
| `091540ED…` | coincide con una actualización de la app (2377 → 3930); causa no demostrada | **OD-2 = A** | PKT L334; DEC L890 |
| `9002E854…` | +1 sección `[projects.<ruta>]` escrita por la sonda de OD-4 (`workspace-write`) | **OD-2c = A** (última aceptada) | PKT L175-179; DEC L931 |
| `723A6898…` | actualización 1 (app `26.930.7945.0`, binario `3b8f6e33…`) | no aprobada (DEC L967); medida con OD-2d-PROBE (DEC L989) | EV L2484-2490 |
| `9EA26634…` | actualización 2 (app `26.1002.6548.0`, binario `97c57e4e…`) | nunca medida por invocación | EV L2568-2570 |
| `6518EFAB…` | actualización 3 (app `26.1002.7124.0`, binario `3553cd6e…`) | **vigente, sin aceptar → P-01** | EV L2879-2882; PAS |

Entre el 2026-10-06 y el 2026-10-08 la familia OD-2 ocupó cinco actos del Owner o del Coordinator (OD-2, OD-2b-PROBE, OD-2c, OD-2d-PROBE y la OD-2d
pendiente) y dos paquetes quedaron obsoletos antes de decidirse (PKT L218-223; EV L2568-2570). Ese coste es el que motiva la pregunta del §57.

## 2. Qué cambia en cada actualización de Codex

### 2.1 Tres actualizaciones observadas

| # | Binario escrito (UTC) | App | Huella anterior → nueva | `config.toml` escrito | Localización por clave | Fuente |
|---|---|---|---|---|---|---|
| 1 | 2026-10-06T18:56:03Z | `26.930.7945.0` | `9002E854…` → `723A6898…` | 18:58:51Z (≈ 3 min después) | **no medida**: misma estructura (107 nombres, 39 secciones, 4 775 bytes); al menos `CODEX_CLI_PATH` y otro valor; «No se puede probar cuál sin el archivo anterior, que nunca se guarda» | EV L2484-2488 |
| 2 | 2026-10-07T02:14:49Z | `26.1002.6548.0` | `723A6898…` → `9EA26634…` | 03:29:05Z (≈ 74 min después) | **9 claves**, por comparación HMAC por clave | EV L2568-2570 |
| 3 | 2026-10-07T11:57:39Z | `26.1002.7124.0` | `9EA26634…` → `6518EFAB…` | 20:05:22Z (≈ 8 h después) | **las mismas 9 claves**; ninguna añadida ni eliminada | EV L2879-2882; PAS |

Dos hechos de esta tabla importan para cualquier regla:

- **El retraso varía de minutos a horas.** La reescritura de `config.toml` no coincide con la instalación del binario. Una regla de «coincidencia con
  un cambio de par observado» tendría que admitir una ventana amplia, en la que también cabría un cambio ajeno a la actualización. A-2 ya exige que
  el par cuente como observado solo «con su huella resultante estable» (A2 L223).
- **La instantánea por clave vive en el scratchpad de la sesión** («clave solo en el scratchpad», EV L2490). No es durable ni la puede reproducir
  otra sesión. No existe instantánea por clave de la última huella aceptada (`9002E854…`).

### 2.2 Las 9 claves, por su semántica documentada

Semántica pública del proveedor (referencia de configuración de Codex, §9). Las variables de entorno concretas de `node_repl` no están documentadas
públicamente; para ellas solo se indica lo que dice el nombre, sin valor probatorio.

| Clave cambiada | Semántica documentada | ¿La neutraliza la receta 16.4? | Lectura |
|---|---|---|---|
| `mcp_servers.node_repl.command` | «Launcher command for an MCP stdio server.» | no | ejecutable que Codex lanza |
| `mcp_servers.node_repl.env.{BROWSER_USE_CODEX_APP_VERSION, CODEX_CLI_PATH, NODE_REPL_NODE_MODULE_DIRS, NODE_REPL_NODE_PATH, NODE_REPL_TRUSTED_CODE_PATHS, NODE_REPL_TRUSTED_SERVICES, SKY_CUA_NATIVE_PIPE_DIRECTORY}` (7) | `env`: «Environment variables forwarded to the MCP stdio server.» | no | entorno de ese proceso; por el nombre: versión de la app, ruta de la CLI, rutas del runtime de Node, **listas de confianza** y un directorio de pipe |
| `notify` | «Command invoked for notifications; receives a JSON payload from Codex.» | no | ejecutable que Codex lanza |

La receta (DESC L24) fija `-s read-only`, `-m` y `-c model_reasoning_effort=…`. Según el proveedor, «CLI flags and `--config` overrides» tienen la
precedencia más alta, así que el modelo, el effort y el sandbox de `config.toml` no deciden la invocación. La receta **no** fija `mcp_servers.*` ni
`notify`. Si `codex exec` arranca los servidores MCP o ejecuta `notify` no está documentado en la referencia consultada y **no se ha medido**:
es UNKNOWN. La frase del paquete OD-2d «la receta de solo lectura no depende de ellas» (PKT L200) no tiene una medición detrás.

### 2.3 Estructura de la huella vigente (solo nombres)

KN: 107 nombres (39 secciones; 14 nombres de clave saneados porque contienen una ruta, RD L349). Por familias:

| Familia | Nombres | Clase por semántica | ¿Cambió en alguna actualización medida? |
|---|---|---|---|
| `mcp_servers.node_repl.*` | 22 | ejecución y entorno de un proceso lanzado | **sí**: 8 de sus 20 claves |
| `notify` | 1 | ejecución | **sí** |
| `projects.*` (12 secciones) + 14 claves saneadas | 26 | confianza (`trust_level`) y rutas; las 14 claves no se pueden clasificar desde el archivo saneado | no (en las actualizaciones); sí con `workspace-write` (PKT L178) |
| `windows.*`, `shell_environment_policy.*`, `features.*` | 6 | sandbox, entorno inyectado, funciones | no |
| `plugins.*`, `marketplaces.*` | 38 | extensiones habilitadas y su origen | no |
| `model`, `model_reasoning_effort`, `service_tier` | 3 | capacidad y consumo (los dos primeros los fija la receta; `service_tier`, no) | no |
| `desktop.*`, `tui.*` | 11 | interfaz de la app | no |

Las únicas familias que podrían declararse inocuas por su nombre (`desktop.*`, `tui.*`) nunca cambiaron. Las 9 claves volátiles están todas en la
familia de ejecución. Una proyección que ignore la interfaz no ahorra ninguna decisión; una que ignore las 9 claves trata como inocuas claves de
ejecución y de confianza.

## 3. ¿Puede OD-2 pasar a una línea base material verificable?

### 3.1 Definición

Una **línea base material** es un par (*H₀*, *R*): *H₀* es una huella aceptada de forma exacta por el Owner y *R* es un predicado mecánico, custodiado
y reproducible, que decide, sin otra decisión del Owner, si una huella posterior *H₁* cuenta como aceptada. La línea base exacta de hoy es el caso
*R*(*H₀*, *H₁*) ⇔ *H₁* = *H₀*.

### 3.2 Requisitos mínimos de *R*

| Id | Requisito | Por qué | Requisito del §57, punto 4, que conserva |
|---|---|---|---|
| R1 | anclaje: *H₀* aceptada por una OD-2 exacta, con una instantánea por clave custodiada (lee valores: solo con la autorización expresa del Owner que esa A-n necesite) | hoy no existe para `9002E854…` (§2.1) | comparación con propiedades acreditadas |
| R2 | lista cerrada *V* de claves volátiles por adapter, por nombre exacto, declarada en el descriptor | sin lista cerrada no hay fallo cerrado | clasificación de claves cambiadas |
| R3 | estructura igual: mismos nombres, sin altas ni bajas | una clave nueva puede ser un permiso nuevo | fallo cerrado ante lo desconocido |
| R4 | igualdad por clave fuera de *V* (digest por clave) | localizar el cambio exige comparar clave a clave | clasificación; fallo cerrado |
| R5 | coincidencia con un cambio observado del par (`BinaryHash`, `AppVersion`) y huella resultante estable (A2 L223) | liga el cambio a la actualización | evidencia del cambio de binario y versión |
| R6 | ningún cambio durante una cesión: P-11 y P-01 de cesión siguen siendo STOP | la cesión no se reinterpreta | sin desactivar P-01 |
| R7 | semántica de *V*: o las claves de *V* quedan inertes en la invocación (receta), o sus valores cumplen un predicado | sin R7, *R* acepta valores no vistos en claves de ejecución (§2.2) | ninguna ampliación implícita de permisos |
| R8 | cualquier otra diferencia o UNKNOWN → P-01 y OD-2 exacta | | fallo cerrado |
| R9 | cada aceptación por regla queda custodiada (blob de *R*, *H₀*, *H₁*, digests, quién evaluó) y el aceptante la reproduce; el Owner puede revocar *R* | verificabilidad | sin aceptación implícita no registrada |
| R10 | solo unidades I62 y el plano (c); las unidades I61 conservan P-01 de AP L698 | V14 criterio 13 (L128); §57, punto 4 | P-01 en producción intacto |

### 3.3 Obstáculos medidos

1. **La clase volátil no es inocua por nombre** (§2.2, §2.3). R7 es obligatorio, y cualquiera de sus dos formas cambia algo más que OD-2.
2. **Neutralizar en la receta (R7, forma 1)** exigiría añadir a la receta overrides de máxima precedencia, p. ej. `mcp_servers.<id>.enabled` («Disable
   an MCP server without removing its configuration.») y un `notify` vacío. No está medido que `codex exec` los respete. Además cambia la receta del
   descriptor (DESC L24), que es superficie de C-20b.
3. **Predicados de valor (R7, forma 2)** exigen leer valores de forma mecánica, aunque no se registren. Eso choca con RD L366 («Nunca se leen ni se
   registran valores de configuración ni credenciales») y con los términos de seguridad bajo los que el Owner aceptó OD-2 (PKT L93: «nunca valores»).
   V14 L991 solo limita lo que se registra, pero el texto materializado y el paquete aprobado hablan también de leer.
4. **Digests por clave verificables.** Un SHA-256 sin clave de un valor de baja entropía (un modelo, un effort, un booleano) se invierte con un
   diccionario, y publicarlo equivale a publicar el valor (C-10; V14 L991). Un HMAC con clave solo es reproducible donde está la clave. Hoy la clave
   vive en el scratchpad (EV L2490): haría falta una clave durable por host, que es estado persistente nuevo en la máquina del Owner. La huella ya está
   ligada al host (`HostInstanceHash` es invalidador, RD L306), así que una clave por host es coherente, pero hay que decidirla.
5. **Ventana de coincidencia** (§2.1): R5 necesita una ventana de horas, que debilita la atribución del cambio a la actualización.
6. **Beneficio limitado en F6.** Con A-2, cada actualización ya exige un bloque nuevo, con disposición del Coordinator y consumo autorizado por el
   Owner (A2 L224-225), y nunca por adelantado (A2 L225). Una línea base material quita solo la OD-2 posterior. Por actualización, el Owner pasa de
   dos actos (consumo antes, OD-2 después) a uno, y con una A-n que cumpla la regla 11, c) de A-3 solo en los cambios que esa A-n pudiera explicar:
   con los hechos de hoy, ninguno de los observados (8 de las 9 claves son de lanzamiento o de confianza, y la función de la novena no está
   verificada para la versión observada). Además, materializarla invalida `MC_I62`, lo que obliga a resembrar el fixture y a repetir los
   pilotos afectados (V14 L898-899; C-21, V14 L2360).

### 3.4 Diseños posibles

| Opción | Qué es | Autoridad de OD-2 | Cumple R7 por |
|---|---|---|---|
| **A** | la huella exacta de hoy | sin cambio | — (no hay *R*) |
| **B** | línea base material por clase cerrada *V*, digests con clave por clave (leen valores en el host sin registrarlos), con las claves de *V* neutralizadas en la receta | cambia: pre-acepta una clase | neutralización (medida antes) |
| **C** | como B, pero con predicados de valor sobre *V*, evaluados sin registrar valores, sin tocar la receta | cambia: pre-acepta una clase | lectura mecánica de valores |
| **D** | aislar la configuración del transporte: `CODEX_HOME` dedicado para `codex-cli` (el proveedor documenta `CODEX_HOME` como raíz de la configuración y la autenticación), con OD-2 exacta sobre su `config.toml` | sin cambio | no aplica: la app escribe en su propio `~/.codex` |

D no es una evolución de OD-2, pero ataca la causa: si la app no reescribe el `config.toml` dedicado, una actualización solo cambia la identidad del
binario. Entonces OD-2 exacta sigue valiendo sin otra pregunta, y A-3 podría aplicarse a `codex-cli`, porque A3 regla 1, c exige que no cambie la
huella. Que la app ni la CLI en `read-only` lo reescriban es una **hipótesis sin medir** (la CLI solo escribió entradas de proyecto en
`workspace-write`, PKT L184). D exige además una autenticación nueva de la CLI hecha por el Owner (credencial nueva en el host; la sesión nunca la
lee), un `config.toml` mínimo que fija el Owner (la sesión no escribe `config.toml`, PKT L185) y cambiar en el descriptor la ruta de la huella y el
entorno de la receta (superficie de C-20b, `MC_I62` nuevo).

### 3.5 Cobertura de lo que el §57, punto 4, exige conservar

| Exigencia (DEC L1115) | A | B | C | D |
|---|---|---|---|---|
| evidencia del cambio de binario y versión | sí (paquete) | sí (R5) | sí (R5) | sí |
| clasificación de claves cambiadas | parcial: solo por nombre (nombres añadidos o eliminados y estructura, A-3, regla 11, b), que en un cambio solo de valores, como las tres actualizaciones medidas de `codex-cli`, no identifica ninguna clave cambiada; la clasificación de las 9 claves está solo en el anexo de A-3 (no normativo, §5) y procede de localizaciones por clave anteriores que leyeron valores (EV §71 y §87); localizar los cambios de valor queda diferido a una autorización expresa del Owner (A-3, regla 11, b), que A-3 no pide | normativa (R2-R4) | normativa + predicados | innecesaria mientras la huella no cambie; si cambia, OD-2 exacta |
| comparación con propiedades materiales acreditadas | la hace el Owner al decidir | *R* + sonda (A2-P2; A-3 si se enmienda su regla 1) | ídem | sonda; A-3 aplicable sin enmendarla |
| sonda rápida con presupuesto autorizado | A2-P2 | A2-P2 | A2-P2 | A2-P2 |
| fallo cerrado ante cambios desconocidos | total (P-01) | R3, R8 | R3, R8 | total (P-01) |
| ninguna ampliación implícita de permisos o consumo | sí; no lee valores | sí, si la neutralización solo reduce lo que corre; el consumo sigue por bloque; amplía lo que la sesión lee (valores, para los digests con clave) | amplía lo que la sesión lee (valores) | sí; añade una credencial, por decisión del Owner |
| sin aceptación implícita de versiones nuevas | sí | aceptación de cada huella por regla: explícita en la clase, implícita en la instancia | ídem | sí |
| sin desactivar P-01 en producción | sí | sí (R10) | sí (R10) | sí |

## 4. ¿Modifica autoridad reservada al Owner? — **SÍ**

1. **Cambia el objeto de una decisión del Owner.** El Owner aceptó dos veces un valor exacto (DEC L890, DEC L931) bajo un paquete que excluía
   «aceptar en silencio otra huella» (PKT L91). Una línea base material acepta hoy valores que todavía no existen. Cambiar una decisión del Owner es
   OWNER-RESERVED (LC L94).
2. **Mueve quién acepta cada instancia (M-01).** La aceptación de cada huella concreta pasaría del Owner a una regla que evalúa la sesión o el aceptante:
   «Cambia quien posee una regla o valor» (LC L83).
3. **Reabre una frontera fijada por el Coordinator.** El Coordinator ya resolvió que aceptar un hash futuro por su delta «es demasiado amplio para esa
   frontera» (DEC L913). La orden vigente impide ampliarla por disposición ordinaria (DEC L1115).

   **DEC L913**

   > | **OD-2b** | **A2 no se adopta** como aceptación automática de una huella futura. Se acepta el diagnóstico (la CLI añade una entrada de confianza de proyecto; P-01 paró bien el transporte), pero OD-2 acepta una huella concreta del adapter, y «aceptar el hash futuro que aparezca si el delta saneado coincide» es demasiado amplio para esa frontera. Se divide en **OD-2b-PROBE** y **OD-2c** (aceptación del hash exacto) |

4. **Contradice un texto acordado.** A-2 regla 4 (A2 L227) y su §7 (A2 L301-304) ponen «aceptar con OD-2 la huella resultante» entre las decisiones
   reservadas al Owner por bloque, y su M-01 = no se apoya en que «el Owner ya acepta las huellas (OD-2)» (A2 L269). A-2 no se edita: solo otra A-n
   puede corregirla o sustituirla (LC L216).
5. **Cambia la semántica única de OD-5 en F6.** Las salvaguardas del host, entre ellas la «huella antes y después», las autoriza el Owner (V14 L910-911).
   Para cambiarlas:

   **V14 L913**

   > Si el Owner no acepta esa semántica, F6 no procede y cualquier otra vía exige una A-n. **No** se congelan dos semánticas.

6. **Es una decisión de seguridad sobre el host y las credenciales del Owner.** Con B y con C, la sesión lee valores en el host, aunque no los
   registre (digests con clave por clave; con C, además, predicados de valor), contra los términos aprobados (PKT L93; RD L366). Con B, además, el
   Owner aceptaría sin verlos cambios de valor en claves de ejecución neutralizadas en la receta (una A-n que cumpla la regla 11, c) de
   A-3, solo tras medir la neutralización y verificar su función para la versión observada; sin ellas, solo si la A-n se apartara de ese
   requisito de forma declarada y con la autoridad del Owner). La huella es materia del Owner (RD L423). Con A no se leen
   valores: A-3 no define verificador y excluye la localización por clave de los cambios de valor (regla 11, b); toda lectura de valores exige una
   autorización expresa del Owner que nombre el programa y su blob, las superficies y la caducidad, que no se pide ahora. La historia completa de
   las lecturas anteriores está en el paquete (sección «Lectura de valores (no se pide ahora)»); esta investigación no afirma ni niega si las
   aprobaciones de entonces cubrían leer valores.
7. **Reinterpreta la consecuencia vigilada de un ADR aceptado (M-08, por duda).** ADR46 L95: «los efectos laterales de Codex sobre
   `~/.codex/config.toml`: un cambio de su hash detiene las invocaciones». Para las unidades I62, el sucesor ADR-0048 solo lo acepta el Owner
   (ADR48 L5, OD-1).

**Objeción considerada.** V14 L971 dice «línea base de huella», no «SHA-256 exacto». Pero la huella de un adapter CONFIG_FILE está definida como el
SHA-256 y los nombres (AP L1055-1056; RD L303; PRE L275-281; DESC L40). Además, V14 L977 pide cada solicitud «con la ruta y el hash actuales», P-11
detiene cualquier cambio de huella en una cesión (V14 L777) y A-2 dice «huella exacta» (A2 L227). La lectura «línea base = clase» no existe sin
cambiar esos textos, y «No existe enmienda semantica invisible» (LC L218).

**Objeción 2.** «El Owner aprobaría la regla una vez; sigue decidiendo él». Es cierto, y por eso es una decisión del Owner: cambia la forma de una
decisión reservada (de instancia a clase) y además necesita una A-n para operar (§5). El Coordinator no puede hacerlo por disposición.

**D no modifica la autoridad de OD-2** (sigue el hash exacto), pero exige otras decisiones del Owner: una autenticación nueva y configuración
persistente nueva en su host. Por eso también va en el paquete.

## 5. Freeze, vehículo y materialidad

### 5.1 Elementos congelados o acordados afectados (B y C)

| Elemento | Cita | Efecto |
|---|---|---|
| huella del adapter | V14 L330-331 (§7); AP L1055-1056 | la aceptación deja de ser igualdad de hash |
| P-11 / P-01 en I62 | V14 L777; V14 L3290 | sin cambio en la cesión (R6); cambia la comparación con la línea base |
| OD-2 | V14 L971, L977 | la solicitud deja de ser «el hash actual» en los casos de *R* |
| OD-5 (salvaguardas del host en F6) | V14 L910-913 | cambia «huella antes y después» frente a la línea base |
| D.3 «con OD-2» y D.7 | V14 L2443, L2603 | el «con OD-2» lo satisface *R* |
| A-2 regla 4 y §7 | A2 L227, L301-304 | sustitución por la siguiente A-n |
| textos I62 materializados (C-20b) | AP 16.19; RD §13.1, §13.3, §13.4 (L303, L306, L366); DESC L23-L41; PRE `Fingerprint` e `Invalidators.FingerprintSha256`; RR2 `FingerprintChanged` | materialización con `MC_I62` nuevo (V14 L898-899) |
| I-61 | AP L698 | **sin cambio** (R10) |

Para D: cambian el descriptor (ruta de la huella y entorno de la receta, DESC L24, L30, L40) y su materialización; V14 §18 no cambia. Hace falta una
A-n porque la ruta `~/.codex/config.toml` y la receta forman parte de las superficies congeladas o materializadas, aunque el alcance es menor.

### 5.2 Materialidad (LC L81-90)

| M | B / C | D |
|---|---|---|
| M-01 | **sí**: la aceptación de cada instancia pasa a una regla | no: el Owner sigue aceptando el hash exacto |
| M-02 | **sí**: cambia el significado persistido de `Fingerprint.Sha256` e `Invalidators.FingerprintSha256` como clave de aceptación, y hacen falta campos nuevos (digests, blob de *R*) en `preflight/v1` y `relay-record/v2` | no (la forma no cambia; cambia la ruta que declara el descriptor) |
| M-03 | **sí**: una actualización deja de detener `codex-cli` | **sí**: ídem, por construcción |
| M-04 | **sí**: P-01 frente a la línea base deja de fallar en la clase; pasan a fallo cerrado R3 y R8 | no, si la medición confirma que la huella no cambia; si cambia, P-01 sin cambio |
| M-05 | **sí**: contratos consumidos (AP 16.18-16.20, README §13, esquemas) | **sí**: receta del descriptor |
| M-06 | **sí**: el descriptor declara *V* y R7 | **sí**: el descriptor declara `CODEX_HOME` |
| M-07 | **sí, por duda**: una clasificación de claves por adapter puede leerse como registro transversal (LC L78-79: «UNKNOWN en un disparador cuenta como activado») | no |
| M-08 | **sí, por duda** (ADR46 L95) | no |
| OWNER-RESERVED | **sí** (§4) | **sí** (autenticación y configuración persistente del host) |

Clasificación propuesta para la A-n: **MATERIAL POST-FREEZE AMENDMENT con consecuencia OWNER-RESERVED**. Autoridad: decisión previa del Owner
(OD-2-MAT) + Architect independiente + Coordinator (LC L221). Orden de actos del Freeze (FRZ L97-98): «→ decisión del Owner si la materia es
OWNER-RESERVED» y después «→ enmienda A-n según LIFECYCLE §6 (append-only, Applies-to, cláusula anterior y delta exacto)». Revisada sin la
decisión del Owner, la A-n quedaría `BLOCKED — OWNER DECISION` (LC L173).

### 5.3 Vehículo: A-4 separada, no dentro de A-3

| Criterio | Dentro de A-3 | A-4 separada |
|---|---|---|
| consecuencia OWNER-RESERVED | A-3 se detiene por su propia cláusula: «Si el Architect o el Coordinator identifican una consecuencia OWNER-RESERVED, **esta enmienda se detiene**» (A3 §8, cláusula de detención), y quedaría `BLOCKED — OWNER DECISION` (LC L173) | A-3 sigue con su autoridad (Architect + Coordinator); solo A-4 espera al Owner |
| diseño revisado | revierte la exclusión de fallo cerrado de A-3 (A3 regla 1, c; §3.6; Q-A3-10), corregida en los hallazgos F-02, F-03, C-04 y G-08 y protegida por los mutantes `M_FingerprintOnlyIsSuccessor`, `M_OD2Sticky` y `M_OD2NotExact` | A-3 no cambia; A-4 dice cómo compone con la regla 1 de A-3 |
| autoridades | mezcla Architect + Coordinator con Owner | separadas y explícitas |
| nombre fijado por el Coordinator | A-3 = «Runtime Successor Compatibility» (DEC L1100: «preparar SOLO») | sin conflicto |
| superficies | A-3 cambia §6 de V14; esto toca §7, §13, §18, D.3, D.7 y A-2 | separadas, con diff y guardas propios |
| secuencia | — | número siguiente sin huecos (LC L211): A-4 se publica después de A-3 (como candidata o acordada) |
| dependencia de valor | — | con D, A-3 pasa a aplicarse a `codex-cli` sin cambiar su texto; con B o C, A-4 tiene que sustituir la cláusula de huella de la regla 1, c de A-3 si se quiere herencia tras un cambio de huella de la clase |

**Recomendación de vehículo:** A-4 separada, «I-62 A-4 — Línea base de huella de `codex-cli`» (el título exacto lo fija el Coordinator). Se prepara
solo si el Owner responde B, C o D. Con A no hay A-4.

### 5.4 Orden de actos si el Owner elige B, C o D

1. El Owner responde OD-2-MAT (una línea).
2. El Coordinator ordena preparar la A-n siguiente (previsiblemente A-4; su número lo fija LC §6 al publicarse) con su alcance (DEC L1066: el Owner decide las OD reservadas; el acuerdo de la A-n es de Architect y
   Coordinator).
3. Antes del texto: la medición que la opción exige, autorizada aparte por el Owner (B: que `codex exec` respete la neutralización; D: que el
   `config.toml` dedicado no cambie con una actualización ni en `read-only`). C no necesita sonda, pero sí la clave durable por host (§3.3, 4).
4. A-4 se publica después de A-3. Revisión del Architect independiente y AGREED del Coordinator.
5. La materialización invalida `MC_I62`: se cierra de nuevo, se resiembra el fixture y se repiten los pilotos afectados (V14 L898-899). Cuándo, lo
   decide el Coordinator (precedente: ADR-0048 NO CHANGE BEFORE F6 PILOTS, DEC L916).
6. La primera aplicación necesita una OD-2 exacta de anclaje con su instantánea por clave custodiada (R1), que lee valores: solo con la autorización
   expresa del Owner que esa A-n necesite.

## 6. Efecto en A-3 (candidata) y en A-2 (AGREED)

- **A-3 no cambia.** Su §3.6 y su §8 siguen siendo correctos: la huella queda fuera de la relación de sucesor y OD-2 sigue siendo exacta. La respuesta
  a Q-A3-10 que se desprende de este análisis es **conservar la exclusión** y llevar cualquier cambio a OD-2-MAT y A-4. Opcionalmente, Q-A3-10 puede
  citar este paquete; eso cambia el texto de A-3, así que habría que repetir sus guardas.
- **A-2 no cambia** (blob `f1e1d6f0`). Con B o C, la sustitución de su regla 4 la haría A-4 (LC L216), nunca una edición.
- **F6 no cambia:** el camino sigue siendo `BLOQUE-CODEX-CONSUMO` (REQ L62) y, después de medir, una OD-2 exacta sobre la huella resultante (REQ L50).

## 7. Lo que no cambia mientras el Owner no decida

- OD-2 es el SHA-256 exacto de todo `config.toml` (DEC L890, L931; A2 L227).
- P-01 de I-61 (AP L698) y P-01/P-11 de I62 no cambian; `codex-cli` sigue en STOP con `6518EFAB…`.
- Ninguna huella se acepta por su delta (DEC L913) ni porque la actualización «parezca legítima» (DEC L967).
- El silencio no es decisión (V14 L977).

## 8. Límites de esta investigación

- No se leyó `config.toml`, ningún valor de configuración ni ninguna credencial. La clasificación usa solo nombres saneados (KN), la evidencia
  custodiada y la documentación pública del proveedor.
- No se ejecutó ningún binario. Si `codex exec` arranca `mcp_servers.node_repl` o ejecuta `notify`, y si respeta un override `enabled = false`, es
  UNKNOWN.
- Que la app o la CLI dejen intacto un `CODEX_HOME` dedicado es una hipótesis sin medir.
- La semántica de las variables `NODE_REPL_*`, `SKY_CUA_*` y `BROWSER_USE_*` no está documentada públicamente; solo se usa su nombre.
- A-3 se cita por sección y regla (sigla A3), nunca por línea.

## 9. Fuentes externas (documentación pública del proveedor; consultadas sin enviar datos locales)

- Referencia de configuración de Codex (definiciones de `notify`, `mcp_servers.<id>.command`, `.env`, `.enabled`, `model`, `sandbox_mode`,
  `projects.<path>.trust_level`, `windows.sandbox`, `shell_environment_policy.set`): https://learn.chatgpt.com/docs/config-file/config-reference
  (redirigida desde https://developers.openai.com/codex/config-reference).
- Configuración básica (precedencia: «CLI flags and `--config` overrides» primero; `~/.codex/config.toml` último):
  https://learn.chatgpt.com/docs/config-file/config-basic (redirigida desde https://developers.openai.com/codex/config-basic).
- Variables de entorno (`CODEX_HOME` como raíz de la configuración y la autenticación): https://learn.chatgpt.com/docs/config-file/environment-variables
- Repositorio público: https://github.com/openai/codex/blob/main/docs/config.md (remite a las páginas anteriores).
