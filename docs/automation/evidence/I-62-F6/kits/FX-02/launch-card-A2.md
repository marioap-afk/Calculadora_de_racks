# Tarjeta de lanzamiento del Owner — Principal A2 de FX-U1 (topología A)

> **Preparación; no lanzado.** La sesión de supervisión completa la parte 1 antes de entregar esta tarjeta. El Owner solo hace la parte 2. Del archivo, a
> la sesión A2 llega **únicamente** el bloque «Texto inicial» de la parte 2. Frontera: **F-HL-A2** (HUMAN_LAUNCH_REQUIRED). Autoridad: V14 D.3 (Principal
> abierto por el Owner; tope de sesiones de la ronda A), D.6 (entradas y aislamiento), AP 16.15 (perfil PRINCIPAL_COORDINATION: nivel Frontera, effort
> `Long-horizon`), routing §8, dec. §50-§52 (mensajes al Principal; variante Claude; `xhigh`).

## Parte 1 — antes de entregar la tarjeta (la sesión de supervisión; lista de comprobación)

| # | Comprobación | Esperado | Registro |
|---|---|---|---|
| 0 | FX-04a cerrada antes de cualquier escritura de FX-02 en el origen y de abrir A2 (frontera F-UP-FX04A; README §1) | B2 lanzada y terminada, SHA-256 de su respuesta durable en RackCad, comparación con el oráculo v2 hecha y clasificación del Coordinator registrada; o disposición explícita del Coordinator de I-62 que libere el origen. Si no: **no** se publica la orden ni se entrega esta tarjeta | `prelaunch-A2.json` (referencia a la clasificación o a la disposición) |
| 1 | Orden FX-U1-O4 publicada en `fx/u1` (`origin` y `github`) con todos sus marcadores de posición resueltos o retirados por el Coordinator | commit de la orden = punta de `fx/u1`; corrida de CI registrada | `prelaunch-A2.json` |
| 2 | `main` del fixture | = `fbe25347799e5b801ef708454212335537448bd1` (si no: designación T22, no T16) | ídem |
| 3 | Tope de sesiones de Principal de la ronda A (P-07) | disposición del Coordinator de I-62 sobre OQ-07 (CD-08) registrada en `decisions/I-62.md`; A2 no excede el tope | ídem |
| 4 | Clon `D:\r62-fixture\A2` | `git clone --no-local` desde `D:\r62-fixture\fixture-origin.git`, rama `fx/u1`, en la **punta vigente** tras el punto 1; árbol limpio; `core.autocrlf=false`; autor y committer `fixture <fixture@example.invalid>` locales al clon; remotos `origin` (origen del fixture) y `github` (`marioap-afk/rackcad-i62-fixture`), y ningún otro | ídem (`HEAD`, remotos, `git status --porcelain` vacío) |
| 5 | Sin rastros de sesiones previas en esa carpeta | ningún directorio de proyecto de Claude para `D:\r62-fixture\A2`; memoria de proyecto inexistente | ídem |
| 6 | Entradas automáticas (D.6), con ruta y SHA-256 | `CLAUDE.md` global ausente (o su hash); `settings.json` y gancho con su hash y la salida del gancho; plugins habilitados; `AGENTS.md` y `CLAUDE.md` del clon; contexto que inyecta la app con cobertura declarada. Ninguna con hechos de FX-U1, del oráculo o de RackCad | ídem |
| 7 | Área transitoria y árbol limpio | lo que disponga el Coordinator de I-62 sobre OQ-02 (CD-02) aplicado en el clon y enumerado como dependencia técnica | ídem |
| 8 | Huella de `codex-cli` | SHA-256 de `~/.codex/config.toml` y binario registrados (solo hash y nombres de claves saneados); abrir A2 no debe cambiarlos | ídem |
| 9 | Fuera del clon y del texto inicial | ningún oráculo, esperado, disposición de negativo, transcripción de A, R, B1 o B2, ni texto de V14 | ídem |
| 10 | Modo de permisos | el que el Owner elija en la parte 2, paso 1 (decisión suya; la supervisión no recomienda ninguno). La supervisión registra el modo efectivo (`get_session` de A2) en `prelaunch-A2.json` | ídem |

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
   A2 espera (designación, autorización de transporte, aceptación de bindings u otra de la orden). Un `continúa` no transporta información del protocolo
   (dec. §50); la supervisión registra cada uno con su instante.
5. **Si A2 te pregunta algo** (incluida una pregunta con opciones): no contestes ni elijas una opción. Avisa a la supervisión. Si hace falta una decisión,
   la publica el Coordinator en `FX-U1.md` y después tú escribes `continúa`. Precedente: una respuesta del Owner dentro de la sesión A («mantener
   `bdc8e4d`») acabó siendo una decisión no autorizada dentro del sistema bajo prueba (ev. §69, dec. §51).
6. **Si la app pide aprobar permisos de herramientas:** apruébalos solo para el clon
   `D:\r62-fixture\A2`; la supervisión registra cada aprobación como interacción del Owner.
7. **No cierres la sesión** hasta que la supervisión te lo diga.

## Parte 3 — eventos de compleción

Los observa la sesión de supervisión en el origen del fixture, no el Owner. Los valores esperados de cada evento están solo en `supervision-checks.md`
(archivo de supervisión); esta tarjeta solo nombra los eventos.

| Evento | Señal observable | Qué sigue |
|---|---|---|
| E1 | commit nuevo de A2 en `fx/u1` con su observación y su propuesta, y A2 en espera | validación (S07), designación del Coordinator (S08) y `continúa` |
| E1' | A2 se detiene sin publicar nada | la supervisión registra la causa; nadie corrige el effort sin instrucción |
| E2 | primer commit de estado de A2 | validador y siguiente frontera (transporte, S11-S13) |
| E3 | commit de A2 con la propuesta de binding del Controller, y A2 en espera | validación y aceptación del Coordinator (S16) y `continúa` |
| E4 | commit de estado que abre la ventana | desde aquí la supervisión **no** escribe en `fx/u1` hasta el cierre de la ventana |
| E5 | commit de estado que cierra la ventana | validador, CI y revisiones fuera de la ventana según la orden |
| E6 | fin según la orden y `isRunning` = false en dos observaciones de la supervisión, sin actividad posterior | terminación de A2 acreditada (V14 §9.1); el Owner puede cerrar la sesión |

Un STOP de A2 se registra tal cual; no se reanuda con un mensaje del Owner, sino con una decisión del Coordinator publicada en `FX-U1.md`.
