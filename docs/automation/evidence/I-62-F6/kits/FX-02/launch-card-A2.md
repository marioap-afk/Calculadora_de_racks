# Tarjeta de lanzamiento del Owner — Principal A2 de FX-U1 (topología A)

> **Preparación; no lanzado.** La sesión de supervisión completa la parte 1 antes de entregar esta tarjeta. El Owner solo hace la parte 2. Del archivo, a
> la sesión A2 llega **únicamente** el bloque «Texto inicial» de la parte 2. Frontera: **F-HL-A2** (HUMAN_LAUNCH_REQUIRED). Autoridad: V14 D.3 (Principal
> abierto por el Owner; tope de sesiones de la ronda A), D.6 (entradas y aislamiento; aplicación a A2: OQ-32 del README), AP 16.15 (perfil PRINCIPAL_COORDINATION: nivel Frontera, effort
> `Long-horizon`), routing §8, dec. §52 (variante Claude; `xhigh`), dec. §54 («Bus de mensajes»), decisiones §55 (U-01c: origen liberado para la
> preparación; U-02(b): medición y sus invalidadores; U-03: `-C` = `D:\r62-fixture\A2`; U-04: tope de sondas agotado, A-2 antes de medir) y la
> práctica de los «continúa» (precedente de R, ev. §71; actor y alcance frente a dec. §50: OQ-16 del README, pendiente: U-06). Acción 4 de la tarjeta
> de la mañana (`../README.md`).

## Parte 1 — antes de entregar la tarjeta (la sesión de supervisión; lista de comprobación)

| # | Comprobación | Esperado | Registro |
|---|---|---|---|
| 0 | Origen del fixture y FX-04a antes de cualquier escritura de FX-02 en el origen y de abrir A2 (README §1) | origen liberado para la preparación (decisiones §55, U-01c; FX-04a sigue OPEN / UNVERIFIED) **y** disposición del Coordinator sobre OQ-33 (CD-26: B3 desde un clon cuyo `origin` sea la instantánea `D:\r62-fixture\fx04a-qh2-origin.git`, o ninguna escritura en `fx/u1` antes de B3) registrada en `decisions/I-62.md` y respetada. Si no: **no** se publica la orden ni se entrega esta tarjeta | `prelaunch-A2.json` (referencias a decisiones §55 y a la disposición de CD-26) |
| 1 | Orden FX-U1-O4 publicada en `fx/u1` (`origin` y `github`) con todos sus marcadores de posición resueltos o retirados por el Coordinator | commit de la orden = punta de `fx/u1`; corrida de CI registrada | `prelaunch-A2.json` |
| 2 | `main` del fixture | = `fbe25347799e5b801ef708454212335537448bd1` (si no: designación T22, no T16) | ídem |
| 3 | Tope de sesiones de Principal de la ronda A (P-07) | disposición del Coordinator de I-62 sobre OQ-07 (CD-08) registrada en `decisions/I-62.md`; A2 no excede el tope | ídem |
| 4 | Clon `D:\r62-fixture\A2` | `git clone --no-local` desde `D:\r62-fixture\fixture-origin.git`, rama `fx/u1`, en la **punta vigente** tras el punto 1; árbol limpio; `core.autocrlf=false`; autor y committer `fixture <fixture@example.invalid>` locales al clon; remotos `origin` (origen del fixture) y `github` (`marioap-afk/rackcad-i62-fixture`), y ningún otro | ídem (`HEAD`, remotos, `git status --porcelain` vacío) |
| 5 | Sin rastros de sesiones previas en esa carpeta | ningún directorio de proyecto de Claude para `D:\r62-fixture\A2`; memoria de proyecto inexistente. Esta parte 1 se completa después de S11 y S12 (F-HL-A2 y S04 dependen de ellas; README §2), así que, si la sonda de S11 corrió en `A2` (propuesta de OQ-35 / CD-28 del README), los puntos 4 y 5 se comprueban después de esa sonda, cuyo registro de sesión de Codex es de la supervisión (LB-1). Ninguna sonda corre con `-C` en `A2` con A2 abierta | ídem |
| 6 | Entradas automáticas (D.6), con ruta y SHA-256 | `CLAUDE.md` global ausente (o su hash); `settings.json` y gancho con su hash y la salida del gancho; plugins habilitados; `AGENTS.md` y `CLAUDE.md` del clon; contexto que inyecta la app con cobertura declarada. Ninguna con hechos de FX-U1, del oráculo o de RackCad | ídem |
| 7 | Área transitoria y árbol limpio | lo que disponga el Coordinator de I-62 sobre OQ-02 (CD-02) aplicado en el clon y enumerado como dependencia técnica | ídem |
| 8 | Huella de `codex-cli` y vigencia de la medición | SHA-256 de `~/.codex/config.toml` y binario registrados (solo hash y nombres de claves saneados); abrir A2 no debe cambiarlos. Los invalidadores de decisiones §55 U-02(b) (BinaryHash, AppVersion, huella, estado de autenticación, modelo, effort, blobs de catálogo y routing, instancia del host) iguales a los de la medición de la celda del Controller en S11 (directorio según OQ-35 / CD-28 del README; propuesta: `A2`) y a los aceptados en OD-2d (S12), las dos hechas antes de esta parte 1: entonces no hay segunda medición. Si cambia alguno, la observación queda obsoleta y se registra (sin celda medida vigente no se autoriza el transporte) | ídem |
| 9 | Fuera del clon y del texto inicial | ningún oráculo, esperado, disposición de negativo, transcripción de A, R, B1 o B2, ni texto de V14 | ídem |
| 10 | Modo de permisos | el que el Owner elija en la parte 2, paso 1 (decisión suya; la supervisión no recomienda ninguno). La supervisión registra el modo efectivo (`get_session` de A2) en `prelaunch-A2.json` | ídem |
| 11 | Lista B (abajo): entradas privadas prohibidas para A2, enumeradas con su ruta | cada ruta de la lista B registrada; ubicación y SHA-256 de los archivos sellados de FX-02 (`negatives.md`, `supervision-checks.md`) y de los oráculos, fuera del clon y de toda ruta de la lista A | ídem (`ForbiddenInputs`) |
| 12 | Lista A (abajo): operaciones que A2 necesita fuera del clon, enumeradas | cada ruta y cada ejecutable de la lista A registrados (con SHA-256 los binarios), para contrastar después la auditoría | ídem |
| 13 | Directorio `-C` de `codex-cli` | `D:\r62-fixture\A2` (decisiones §55, U-03); con la propuesta de OQ-35 (CD-28) del README, también el de la sonda de S11 (U-03 no fija el directorio de las sondas); es el único directorio de trabajo del Controller en el bloque de autorización de transporte (el del Architect de FX-02 lo fija CD-27 / OQ-34 del README) | ídem |

**Lista A — lo que A2 tiene que poder hacer** (hechos de la orden y del procedimiento; no es una recomendación de modo de permisos):

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

**Lista B — lo que A2 no puede leer ni usar** (V14 D.6, «Privadas prohibidas»; la misma clase de entradas que dec. §52-§53 excluyó para B y B2; D.6
está escrita para B en FX-04a: si y cómo se aplica a A2 en FX-02 es OQ-32 del README):

| # | Entrada | Fuente |
|---|---|---|
| LB-1 | transcripciones y sesiones de otras sesiones: `%USERPROFILE%\.claude\projects\*` de otros directorios y `%USERPROFILE%\.codex\sessions\*` que no sean las de sus propias invocaciones (LA-5), incluido el registro de la sonda de la supervisión de S11 (con `-C D:\r62-fixture\A2` si OQ-35 / CD-28 del README adopta la propuesta); toda herramienta de sesiones distinta de `get_session("self")` | V14 D.6; precedente: auditoría de B (ev. §72) |
| LB-2 | los worktrees de RackCad (el de I-62 bajo `%USERPROFILE%\.codex\worktrees\` y el principal, `D:\Documentos\…`) y `D:\IDs` | V14 D.6 |
| LB-3 | `D:\r62-fixture\evidence-out\` (archivos sellados de FX-02, oráculos, kits de otras líneas) | lista de la supervisión por analogía con V14 D.6 «Privadas prohibidas» (OQ-32); ubicación: README (cabecera) |
| LB-4 | los demás clones del fixture (`A`, `R`, `B`, `B2`, `arch`, `A6`) y sus áreas transitorias, y la instantánea `D:\r62-fixture\fx04a-qh2-origin.git` | lista de la supervisión por analogía con V14 D.6 «Privadas prohibidas» (OQ-32); instantánea: README §0 y OQ-33 |
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
   dec. §50 están pendientes: OQ-16 del README. La supervisión registra cada uno con su instante. Dentro de la ventana (entre los eventos E4 y E5) no
   se publica ninguna decisión que esperar.
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
| E2 | primer commit de estado de A2 | validador y siguiente frontera (transporte: S13; S11 y S12 ya están hechas antes de abrir A2) |
| E3 | commit de A2 con la propuesta de binding del Controller, y A2 en espera | validación y aceptación del Coordinator (S16) y `continúa` |
| E4 | commit de estado que abre la ventana | desde aquí la supervisión **no** escribe en `fx/u1` hasta el cierre de la ventana |
| E5 | commit de estado que cierra la ventana | validador, CI y revisiones fuera de la ventana según la orden |
| E6 | fin según la orden y `isRunning` = false en dos observaciones de la supervisión, sin actividad posterior | terminación de A2 acreditada (V14 §9.1); el Owner puede cerrar la sesión |

Un STOP de A2 se registra tal cual; no se reanuda con un mensaje del Owner, sino con una decisión del Coordinator publicada en `FX-U1.md`. Dentro de la
ventana (E4-E5) el Coordinator no puede publicar: lo que hace A2 entonces lo fija la orden (`{IN_WINDOW_STOP_RULE}`; CD-23 / OQ-29 del README).

**Auditoría de aislamiento (después de E6; la hace la supervisión, no el Owner).** La supervisión revisa las llamadas a herramientas de la transcripción
de A2 (incluidas las de sus subagentes) y los registros de sesión de Codex de sus invocaciones, y contrasta cada ruta leída o ejecutada fuera del clon
con las listas A y B de la parte 1, con cobertura declarada (V14 D.6, «Auditoría»: «el registro no garantiza capturar lecturas internas del runtime»;
precedente: auditoría de B, ev. §72). La clasificación que se derive la propone la supervisión al Coordinator (V14 D.6, tabla de hallazgos; D.4); si y
cómo se aplica D.6 a A2 en FX-02 es OQ-32 del README; los criterios están en `supervision-checks.md` §8 (archivo de supervisión).
