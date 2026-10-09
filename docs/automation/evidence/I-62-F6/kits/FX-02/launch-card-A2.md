# Tarjeta de lanzamiento del Owner — Principal A2 de FX-U1 (topología A)

> **Preparación; no lanzado.** La sesión de supervisión completa la parte 1 antes de entregar esta tarjeta. El Owner solo hace la parte 2. Del archivo, a
> la sesión A2 llega **únicamente** el bloque «Texto inicial» de la parte 2. Frontera: **F-HL-A2** (HUMAN_LAUNCH_REQUIRED). Autoridad: V14 D.3 (Principal
> abierto por el Owner; tope de sesiones de la ronda A), D.6 (entradas y aislamiento; aplicación a A2: OQ-32 del README, U-26), AP 16.15 (perfil
> PRINCIPAL_COORDINATION: nivel Frontera, effort `Long-horizon`), routing §8, dec. §52 (variante Claude; `xhigh`), dec. §54 («Bus de mensajes»),
> decisiones §55 (U-02(b): medición y sus invalidadores; U-03: `-C` = `D:\r62-fixture\A2`), §57 (A-2 AGREED), §58 (B3 en `D:\r62-fixture\B3` desde la
> instantánea del QH2; bloque A2-P2 en `D:\r62-fixture\A` y `D:\r62-fixture\arch`; OD-2-MAT = A), §59 (OD-2e = A sobre la huella `6518EFAB…`), §60
> (FX-04a / C-25a PASS sobre B3, sin B4; F6-OBS-03), §61 (A-4 en revisión formal; la sonda adicional del Controller no se gasta antes de A-4 AGREED y
> `A4-SONDA-CONSUMO = A`; orden: decisión sobre A-4 → sonda autorizada del Controller → lanzamiento de A2) y la práctica de los «continúa» (precedente
> de R, ev. §71; actor y alcance frente a dec. §50: OQ-16 del README, pendiente: U-06). Acción 4 de la tarjeta de la mañana (`../README.md`).
>
> **Copia de staging (fx02-prep, 2026-10-09).** Actualizada con decisiones §57-§61; no se entrega. El texto inicial no cambia.

## Parte 1 — antes de entregar la tarjeta (la sesión de supervisión; lista de comprobación)

| # | Comprobación | Esperado | Registro |
|---|---|---|---|
| 0 | Origen del fixture y FX-04a antes de cualquier escritura de FX-02 en el origen y de abrir A2 (README §1) | FX-04a / C-25a = PASS sobre B3, sin B4 (decisiones §60.1). OQ-33 (CD-26, U-66) **resuelta por hecho**: B3 corrió en el clon `D:\r62-fixture\B3` cuyo `origin` era la instantánea congelada `D:\r62-fixture\fx04a-qh2-origin.git` (decisiones §58.1: «Usa solo el clon B3 preparado (origen congelado en QH2)»; evidencia §90-§91), y ninguna B3 ni B4 futura leerá `fx/u1` (decisiones §60.1: «Sin repetir B3 ni abrir B4»). Registro de esa resolución por el Coordinator en `decisions/I-62.md` (clasificación G2: solo confirmación). Si no consta: **no** se publica la orden ni se entrega esta tarjeta | `prelaunch-A2.json` (referencias a decisiones §58.1 y §60.1 y al registro de CD-26) |
| 1 | Orden FX-U1-O4 publicada en `fx/u1` (`origin` y `github`) con todos sus marcadores de posición resueltos o retirados por el Coordinator | commit de la orden = punta de `fx/u1`; corrida de CI registrada | `prelaunch-A2.json` |
| 2 | `main` del fixture | = `fbe25347799e5b801ef708454212335537448bd1` (si no: designación T22, no T16) | ídem |
| 3 | Tope de sesiones de Principal de la ronda A (P-07) | disposición del Coordinator de I-62 sobre OQ-07 (CD-08, U-14) registrada en `decisions/I-62.md`; A2 no excede el tope. U-14 cambia un presupuesto: si R cuenta, A2 sería la 3.ª y no puede abrirse sin otra A-n y una autorización de consumo del Owner por encima de OD-5 | ídem |
| 4 | Clon `D:\r62-fixture\A2` | `git clone --no-local` desde `D:\r62-fixture\fixture-origin.git`, rama `fx/u1`, en la **punta vigente** tras el punto 1; árbol limpio; `core.autocrlf=false`; autor y committer `fixture <fixture@example.invalid>` locales al clon; remotos `origin` (origen del fixture) y `github` (`marioap-afk/rackcad-i62-fixture`), y ningún otro | ídem (`HEAD`, remotos, `git status --porcelain` vacío) |
| 5 | Sin rastros de sesiones previas en esa carpeta | ningún directorio de proyecto de Claude para `D:\r62-fixture\A2`; memoria de proyecto inexistente. El bloque A2-P2 (S11) corrió en `A` y `arch`, no en `A2` (decisiones §58.2). Si la disposición de aplicación de A4-1 fija `-C D:\r62-fixture\A2` para la sonda (F-A4-PROBE), los puntos 4 y 5 se comprueban después de esa sonda, cuyo registro de sesión de Codex es de la supervisión (LB-1). Ninguna sonda corre con `-C` en `A2` con A2 abierta | ídem |
| 6 | Entradas automáticas (D.6), con ruta y SHA-256 | `CLAUDE.md` global ausente (o su hash); `settings.json` y gancho con su hash y la salida del gancho; plugins habilitados; `AGENTS.md` y `CLAUDE.md` del clon; contexto que inyecta la app con cobertura declarada. Ninguna con hechos de FX-U1, del oráculo o de RackCad | ídem |
| 7 | Área transitoria y árbol limpio | lo que disponga el Coordinator de I-62 sobre OQ-02 (CD-02, U-08) aplicado en el clon y enumerado como dependencia técnica | ídem |
| 8 | Huella de `codex-cli` y vigencia de la medición | identidad aceptada por OD-2e = A (decisiones §59): SHA-256 de `~/.codex/config.toml` = `6518EFAB0BC0C0C2C2C5DCCD3A3D3646DFDC9F15B222FD6B744CBC84B857DB32` (107 nombres de claves saneados, sin valores ni digests por clave); binario `%LOCALAPPDATA%\OpenAI\Codex\bin\9691020b546a15b2\codex.exe` = `3553cd6e7df5a093d8cb8301cd8088a57e0971aba71ddbe0e67f7f44a15cdf68`, el único `codex.exe`; app `26.1002.7124.0`; CLI `codex-cli 0.162.0-alpha.2` (evidencia §93-§94). Abrir A2 no debe cambiarlos. Los invalidadores de decisiones §55 U-02(b) (BinaryHash, AppVersion, huella, estado de autenticación, modelo, effort, blobs de catálogo y routing, instancia del host) iguales a los de la medición en la aceptación (2026-10-09T04:28:31Z) y a los de la sonda medida de A4-1 (F-A4-PROBE), hecha antes de esta parte 1 con todas las operaciones de A4-1, regla 2, demostradas: entonces no hay segunda medición. **Medición de A4-1 pendiente:** A-4 no acordada y `A4-SONDA-CONSUMO` no dada (decisiones §61.4). Si cambia alguno, la observación queda obsoleta y se registra: P-01 y una OD-2 nueva (OD-2-MAT = A); sin celda medida vigente no se autoriza el transporte | ídem |
| 9 | Fuera del clon y del texto inicial | ningún oráculo, esperado, disposición de negativo, transcripción de A, R, B1, B2 o B3, ni texto de V14 | ídem |
| 10 | Modo de permisos | el que el Owner elija en la parte 2, paso 1 (decisión suya; la supervisión no recomienda ninguno). La supervisión registra el modo efectivo (`get_session` de A2) en `prelaunch-A2.json` | ídem |
| 11 | Lista B (abajo): entradas privadas prohibidas para A2, enumeradas con su ruta | cada ruta de la lista B registrada; ubicación y SHA-256 de los archivos sellados de FX-02 (`negatives.md`, `supervision-checks.md`) y de los oráculos, fuera del clon y de toda ruta de la lista A | ídem (`ForbiddenInputs`) |
| 12 | Lista A (abajo): operaciones que A2 necesita fuera del clon, enumeradas | cada ruta y cada ejecutable de la lista A registrados (con SHA-256 los binarios), para contrastar después la auditoría | ídem |
| 13 | Directorio `-C` de `codex-cli` | `D:\r62-fixture\A2` para el Controller (decisiones §55, U-03); es el único directorio de trabajo del Controller en el bloque de autorización de transporte. El de la sonda de A4-1 lo fija su disposición de aplicación (A-4 no lo fija). El del Architect de FX-02 lo fija CD-27 / OQ-34 del README (U-68; propuesta `arch02`), y con A4-2 su medición la fija la A-n | ídem |

**Lista A — lo que A2 tiene que poder hacer** (hechos de la orden y del procedimiento, sin recomendar un modo de permisos):

| # | Operación | Fuente |
|---|---|---|
| LA-1 | leer, escribir y ejecutar dentro del clon `D:\r62-fixture\A2` y en el área transitoria donde la fije CD-02 | orden FX-U1-O4; RAE §2 |
| LA-2 | `git fetch`, `git ls-remote` y `git push` a `origin` (`D:\r62-fixture\fixture-origin.git`, fuera del clon) y a `github` (repositorio del fixture), también desde sus subagentes | orden, puntos 2, 3, 13, 14 y 16; AP 16.4 (Salida); RAE §3.1 |
| LA-3 | SHA-256 de `%USERPROFILE%\.codex\config.toml` y lista ordenada de los nombres de sus secciones y claves, sin valores, antes y después de cada invocación de `codex-cli` | orden, punto 4; AP 16.4 paso 1; RAE §3.1, §3.4 |
| LA-4 | ejecutar `%LOCALAPPDATA%\OpenAI\Codex\bin\<etiqueta autorizada>\codex.exe exec -C D:\r62-fixture\A2 -s read-only …` (Controller: decisiones §55, U-03; el `-C` del Architect de FX-02, según CD-27 / OQ-34 del README), con el `pwsh` del runtime de Codex (`%USERPROFILE%\.cache\codex-runtimes\…`) en el `PATH` del proceso hijo, y el SHA-256 del binario | AP 16.4 («Receta de Codex»); RAE §5; bloque de autorización de transporte; decisiones §55 (U-03) |
| LA-5 | leer el registro de sesión de Codex de **cada invocación propia** en `%USERPROFILE%\.codex\sessions\`, localizado por el `thread_id` de su evento `thread.started` (modelo y effort efectivos) | RAE §5; AP 16.9 #12 (`Routing` con `required`) |
| LA-6 | `gh run list` y `gh run view` (y `--log-failed`) de lectura sobre el repositorio del fixture | RAE §7; AP 16.9 #9 |
| LA-7 | listado de procesos desde PowerShell (`Get-CimInstance Win32_Process`) y `taskkill /PID <pid> /T /F` del árbol de un proceso propio que siga vivo al vencer el tope | AP 16.4 («Procesos»); RAE §3.2, §5 |
| LA-8 | `get_session("self")` para cada preflight propio | AP 16.15; orden, punto 1 |
| LA-9 | lanzar subagentes de su sesión (Worker; Reviewer si se mantiene) con una llamada cuya finalización se notifica | AP 16.4; orden, punto 6 |
| LA-10 | `dotnet` dentro del clon, si el Worker ejecuta pruebas locales | `AGENTS.md` del fixture («Comandos canonicos»); `worker-reviewer-contracts.md` §2.3 |

Pendiente de A-4 (no es una operación autorizada hoy): si A4-1 se aplica, las invocaciones de `codex-cli` declaran en su texto la shell que nombre el
bloque de autorización (propuesta de A-4: `cmd.exe`), sin otro cambio de la receta; si A4-2 se aplica, el ejecutable de `claude-cli` y el directorio
del Architect de FX-02 (U-68) entran en esta lista con su ruta y su SHA-256 (y la descarga del TRX, `gh run download`, si U-07 se dispone así).

**Lista B — lo que A2 no puede leer ni usar** (V14 D.6, «Privadas prohibidas»; la misma clase de entradas que dec. §52-§53 excluyó para B y B2; D.6
está escrita para B en FX-04a: si y cómo se aplica a A2 en FX-02 es OQ-32 del README, U-26):

| # | Entrada | Fuente |
|---|---|---|
| LB-1 | transcripciones y sesiones de otras sesiones: `%USERPROFILE%\.claude\projects\*` de otros directorios y `%USERPROFILE%\.codex\sessions\*` que no sean las de sus propias invocaciones (LA-5), incluidos los registros de las dos sondas del bloque A2-P2 (en `D:\r62-fixture\A` y `D:\r62-fixture\arch`) y el de la futura sonda de A4-1 (directorio según su disposición de aplicación); toda herramienta de sesiones distinta de `get_session("self")` | V14 D.6; precedente: auditoría de B (ev. §72) |
| LB-2 | los worktrees de RackCad (el de I-62 bajo `%USERPROFILE%\.codex\worktrees\` y el principal, `D:\Documentos\…`) y `D:\IDs` | V14 D.6 |
| LB-3 | `D:\r62-fixture\evidence-out\` (archivos sellados de FX-02, oráculos, kits de otras líneas) | lista de la supervisión por analogía con V14 D.6 «Privadas prohibidas» (OQ-32); ubicación: README (cabecera) |
| LB-4 | los demás clones del fixture (`A`, `R`, `B`, `B2`, `B3`, `arch`, `A6` y, si la sonda de A4-1 usa un clon propio, ese clon) y sus áreas transitorias, y la instantánea `D:\r62-fixture\fx04a-qh2-origin.git` | lista de la supervisión por analogía con V14 D.6 «Privadas prohibidas» (OQ-32); instantánea: README §0 y OQ-33; `B3`: decisiones §58.1 |
| LB-5 | memoria de otras sesiones | V14 D.6 |

## Parte 2 — lo que hace el Owner

1. En la app de Claude, abre una **sesión nueva** con la carpeta `D:\r62-fixture\A2`. Antes del primer mensaje: modelo **`claude-opus-5-5`**, effort
   **`xhigh`** y el **modo de permisos** que prefieras (decisión tuya; la supervisión lo registra). No cambies ni el modelo, ni el
   effort, ni el modo de permisos durante la sesión.
2. Pega como **único** primer mensaje el bloque siguiente, sin añadir nada.

**Texto inicial** (lo único que se pega; sin hechos de la unidad):

```text
Eres el Principal de la unidad FX-U1 de este repositorio. Sigue AGENTS.md y la orden FX-U1-O4 de docs/automation/decisions/FX-U1.md.
```

3. **No pegues nunca:** valores esperados, oráculos, resultados de otras sesiones, transcripciones, hechos reconstruidos, textos de RackCad o de I-62,
   identificadores de sesiones reales, salidas de Codex o de subagentes, ni explicaciones sobre qué debería pasar.
4. **Mensajes posteriores permitidos:** solo `continúa`, y solo cuando la sesión de supervisión te avise de que el Coordinator ya publicó la decisión que
   A2 espera (designación, autorización de transporte, aceptación de bindings u otra de la orden). Un `continúa` va sin información sustantiva del
   protocolo (la expresión es de dec. §50: «sin información sustantiva del protocolo»). Práctica vigente: dec. §54, «Bus de mensajes»: «sin usar al Owner
   para transportar prompts, salidas de roles, resultados de revisión ni valores del oráculo»; precedente de R, ev. §71; su actor y su alcance frente a
   dec. §50 están pendientes: OQ-16 del README (U-06 a/b; decisiones §61.5: no se piden mensajes manuales salvo cuando lo exija el contrato vigente). La
   supervisión registra cada uno con su instante. Dentro de la ventana (entre los eventos E4 y E5) no se publica ninguna decisión que esperar.
5. **Si A2 te pregunta algo** (incluida una pregunta con opciones): no contestes ni elijas una opción. Avisa a la supervisión. Si hace falta una decisión,
   la publica el Coordinator en `FX-U1.md` y después tú escribes `continúa`. Precedente: una respuesta del Owner dentro de la sesión A («mantener
   `bdc8e4d`») acabó siendo una decisión no autorizada dentro del sistema bajo prueba (ev. §69, dec. §51).
6. **Si la app pide aprobar un permiso de herramienta:** aprobar o denegar es decisión tuya; la supervisión registra cada petición con su instante, la
   operación y tu respuesta. Hechos para esa decisión (no recomiendan ningún modo de permisos):
   - las operaciones que A2 necesita son las de la **lista A** (parte 1); denegar una deja a A2 sin esa comprobación: según el caso, STOP en una Salida
     o una Entrada, o una comprobación en `not_run`; además, una denegación registrada en los eventos o la transcripción hace fallar `Denials`
     (AP 16.9 #14: «permisos o credenciales → STOP (S-06)»);
   - las de la **lista B** (parte 1) son entradas prohibidas: si consta una lectura de una de ellas, la supervisión la registra y propone su
     clasificación al Coordinator (V14 D.6; OQ-32 del README); la supervisión las busca en la auditoría posterior (parte 3);
   - cada respuesta tuya es una interacción del Owner dentro del sistema bajo prueba (precedente: ev. §69); si cuenta como AUTONOMY_GAP en FX-02 lo
     decide CD-13 (OQ-16 del README).
7. **No cierres la sesión** hasta que la supervisión te lo diga.

## Parte 3 — eventos de compleción

Los observa la sesión de supervisión en el origen del fixture, no el Owner. Los valores esperados de cada evento están solo en `supervision-checks.md`
(archivo de supervisión); esta tarjeta solo nombra los eventos.

| Evento | Señal observable | Qué sigue |
|---|---|---|
| E1 | commit nuevo de A2 en `fx/u1` con su observación y su propuesta, y A2 en espera | validación (S07), designación del Coordinator (S08) y `continúa` |
| E1' | A2 se detiene sin publicar nada | la supervisión registra la causa; nadie corrige el effort sin instrucción |
| E2 | primer commit de estado de A2 | validador y siguiente frontera (transporte: S13; S11 y S12 hechas, bloque A2-P2 y OD-2e, y la sonda de A4-1 hecha antes de abrir A2) |
| E3 | commit de A2 con la propuesta de binding del Controller, y A2 en espera | validación y aceptación del Coordinator (S16) y `continúa` |
| E4 | commit de estado que abre la ventana | desde aquí la supervisión **no** escribe en `fx/u1` hasta el cierre de la ventana |
| E5 | commit de estado que cierra la ventana | validador, CI y revisiones fuera de la ventana según la orden |
| E6 | fin según la orden y `isRunning` = false en dos observaciones de la supervisión, sin actividad posterior | terminación de A2 acreditada (V14 §9.1); el Owner puede cerrar la sesión |

Un STOP de A2 se registra tal cual; no se reanuda con un mensaje del Owner, sino con una decisión del Coordinator publicada en `FX-U1.md`. Dentro de la
ventana (E4-E5) el Coordinator no puede publicar: lo que hace A2 entonces lo fija la orden (`{IN_WINDOW_STOP_RULE}`; CD-23 / OQ-29 del README, U-24).

**Auditoría de aislamiento (después de E6; la hace la supervisión, no el Owner).** La supervisión revisa las llamadas a herramientas de la transcripción
de A2 (incluidas las de sus subagentes) y los registros de sesión de Codex de sus invocaciones, y contrasta cada ruta leída o ejecutada fuera del clon
con las listas A y B de la parte 1, con cobertura declarada (V14 D.6, «Auditoría»: «el registro no garantiza capturar lecturas internas del runtime»;
precedente: auditoría de B, ev. §72). La clasificación que se derive la propone la supervisión al Coordinator (V14 D.6, tabla de hallazgos; D.4); si y
cómo se aplica D.6 a A2 en FX-02 es OQ-32 del README (U-26); los criterios están en `supervision-checks.md` §8 (archivo de supervisión).
