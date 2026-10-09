# Sonda medida de A4-1 — kit en borrador (NO EJECUTADO)

> **BORRADOR — no ejecutado, no congelado, no autorizado.** Preparación de la supervisión (plano a), clasificación de FX-02 (decisiones §61.3), §3 fila
> P7: «§61.4 prohíbe gastar la sonda, no prepararla; se congela tras el AGREED». Decisiones §61.4, literal: «La sonda adicional del Controller no se
> gasta antes del acuerdo de A-4 y de `A4-SONDA-CONSUMO = A`». Nada de este directorio se ha lanzado y no existe `out/`.

## 1. Qué mide

La invocación medida adicional de **A4-1, regla 2** (candidata A-4, `docs/initiatives/I-62-A-4.md`, blob `27ffa26b`, §3.2): con la shell declarada,
la celda del Controller de FX-02 tiene que demostrar las operaciones que necesita la verificación:

| Operación de A4-1, regla 2 | Paso de la sonda | Criterio de `probe_tools.py compare` (propuesta; decide el Coordinator) |
|---|---|---|
| `git diff --name-only <base>..<head>` reproducible | 3 y 4 (dos ejecuciones) | las dos listas = la de referencia y `identical` = `yes` |
| cálculo de blobs y hashes | 5 (`git hash-object`, `git rev-parse HEAD:<f>`) y 6 (`certutil -hashfile … SHA256`) | todos los valores = referencia |
| lectura de las entradas JSON | 7 (contrato de T1: campos y longitudes de arrays) | todos los valores = referencia |
| resolución por el mapa de cláusulas, o comprobación de su no disponibilidad declarada | 8 (modo `RESOLVE` o `DECLARED_UNAVAILABLE`; Q-A4-03) | todos los valores = referencia |
| shell declarada `cmd.exe` | 1 (`ver`, `%ComSpec%`) y auditoría de los comandos de los eventos | `ComSpec` con `cmd.exe`; los intentos con PowerShell o con intérpretes se registran para la disposición |

Además: salida del proceso 0 sin tope vencido, identidad igual antes y después y `HEAD` y árbol del clon sin cambios. Si algo falla, el veredicto
es `NOT_DEMONSTRATED`: la celda no es elegible, la invocación **no se repite** bajo A4-1 (regla 4) y FX-02 sigue UNVERIFIED con esa causa; solo otra
actualización observada (bloque A2-P2, A-2 §3.3 regla 3) u otra A-n abren otra medición. Con `ALL_OPERATIONS_DEMONSTRATED`, la elegibilidad para la
planificación y la verificación (incluidos los negativos con invocación) la declara el Coordinator; la línea del Owner «no acepta su resultado».

## 2. Puertas (todas, en este orden; `run_probe.sh probe` se niega a correr sin las comprobables)

1. **A-4 AGREED, incluida A4-1**: revisión formal acreditada y veredicto del Coordinator (decisiones §61.2; «A-4 no se declara AGREED sin revisión
   acreditada y veredicto del Coordinator»). Comprobación mecánica: `A4_AGREED_MARKER` literal en el archivo de decisiones.
2. **Disposición de aplicación del Coordinator de I-62** que nombre **la celda, la shell y el trío exacto** (A4-1, regla 3) **y el directorio** (A-4
   no lo fija; clasificación #46). Comprobación: `DISPOSITION_MARKER` literal; `CELL`, `SHELL_NAME` y `TRIO` iguales a las constantes del script
   (`gpt-6-luna/high`, `cmd.exe`, `3553cd6e…/26.1002.7124.0/6518EFAB…`); si la disposición nombra otra cosa, el kit se rehace.
3. **`A4-SONDA-CONSUMO = A`** del Owner, en la misma solicitud que `A4-CLAUDE-FX02-CONSUMO` (decisiones §61.5: «El silencio no se toma como
   aprobación»). Comprobación: la línea de `consumo-line.txt` aparece literal, como línea completa, en el archivo de decisiones. Hoy contiene la línea
   de A-4 §7 en el blob `27ffa26b`; si la versión acordada cambia el texto, se sustituye antes de congelar.
4. **P-07, tope 1**, contado aparte: fuera de los lanzamientos de la ronda, del sumando «+ 2 sondas» y de los bloques de A2-P2 (A4-1, regla 3;
   Q-A4-01). Comprobación: el marcador `out/PROBE_LAUNCHED` se crea con `noclobber` justo antes de lanzar y nunca se borra; si existe, o si hay
   salidas `out/probe-*`, el script se niega.
5. **Sin reintento**: el script no tiene bucle de reintento; un fallo, un tope vencido o un lanzamiento incierto cuentan como el único lanzamiento.
6. **Identidad aceptada**: huella `6518EFAB…`, estructura de 107 nombres saneados igual a la línea base, binario `3553cd6e…` (el único `codex.exe`),
   app `26.1002.7124.0` (OD-2e = A, decisiones §59). Se comprueba antes (sin lanzar si difiere) y después; una diferencia es P-01, exige una OD-2 nueva
   (OD-2-MAT = A) y anula la medición (A4-1, regla 5). Nunca se leen valores de `config.toml` ni se calculan digests por clave.
7. **Directorio**: el de la disposición. Si es `D:\r62-fixture\A2`, solo tras S03 y antes de S04: el script se niega si A2 ya tiene directorio de
   proyecto de Claude («nunca `-C A2` con A2 abierta»). Se niega también con `B`, `B2`, `B3`, `R`, `A6`, `arch`, `arch02`, `evidence-out` y cualquier
   `*.git`, y si el clon no tiene `origin` = el origen del fixture, `core.autocrlf=false`, árbol limpio y `BASE_SHA`/`HEAD_SHA`/archivos en su historia.
   Un clon con historia distinto de A2 desacopla la sonda de S02, a cambio de dejar sin medir el riesgo de `read-only` en A2 (README del kit §5;
   clasificación §2).
8. **Congelación tras el AGREED y antes de lanzar**: `gates.env`, el texto de la sonda ya armado (`out/probe-prompt.txt` y su SHA-256), la declaración
   de shell, el esquema y los scripts se custodian con su SHA-256 en la evidencia de RackCad. **El texto de `shell-declaration.txt` es el mismo que el
   bloque de transporte de la orden O4 pondrá en `Shell:`** (A4-1, regla 1: la invocación del rol declara la shell en su texto); si la disposición fija
   otro texto, se cambia aquí antes de congelar.

Lo que el script **no** puede comprobar y queda a cargo de la supervisión: que los marcadores copiados en `gates.env` sean de verdad el registro del
acuerdo, de la disposición y de la línea del Owner (no un texto cualquiera), y los invalidadores que no observa (estado de autenticación, blobs de
catálogo y de routing, instancia del host).

## 3. Procedimiento (solo cuando se cumplan las puertas)

1. Copiar `gates.env.template` a `gates.env` y rellenarlo con lo registrado (ningún `<…>` puede quedar).
2. Congelar (puerta 8): SHA-256 de todos los archivos del kit y de `gates.env`, en custodia.
3. `bash run_probe.sh probe` — una sola vez. Arma el texto con `probe_tools.py build-prompt`, mide la identidad, crea el marcador, lanza la receta 16.4
   literal (`-C`, `-s read-only`, `-m gpt-6-luna`, `-c model_reasoning_effort="high"`, `--output-schema`, `-o`, `--json`, stdin cerrado, el `pwsh`
   del runtime primero en el `PATH`) con tope de 600 s y `taskkill /T /F` del árbol si vence, y vuelve a medir.
4. `bash run_probe.sh compare` — calcula la referencia **después** de la sonda (para que ningún valor esperado exista en disco mientras corre) y emite
   `out/comparison.json`.
5. La supervisión lee del registro de sesión de Codex de **esta** invocación (`out/probe-new-sessions.txt`) solo el modelo y el effort efectivos, y
   sanea las salidas antes de custodiarlas (el `stderr` y los eventos pueden contener rutas con el nombre del usuario de Windows: se sustituyen por
   `%USERPROFILE%` y `%LOCALAPPDATA%`).
6. Resultado al Coordinator. Con elegibilidad declarada: bloque de transporte de la orden O4 en S13 (`Shell`, `Cells`, `MeasurementRecord` según U-15).
   El registro de sesión de Codex de esta sonda entra en LB-1 de la tarjeta de A2.

## 4. Diferencias con `run_block.sh` (bloque A2-P2)

- Una sola sonda; sin modo `version`: `codex --version` y `codex login status` son invocaciones que la línea de consumo no nombra. La versión de la CLI
  (`0.162.0-alpha.2`) se deduce del binario medido (evidencia §93).
- Puertas de §2 y marcador de tope 1; tope de 600 s de la receta (el bloque no lo aplicaba); texto con la shell declarada; esquema con `commands` y
  `values` clave/valor; referencia y comparación posteriores; lista de directorios no admitidos.
- Iguales: `measure` (huella exacta, estructura saneada, binario, número de `codex.exe`, versión de la app; STOP con exit 3), la línea de comando de
  `codex exec`, el registro de `HEAD`, del estado del árbol y de los nombres de las sesiones nuevas.

## 5. Puntos abiertos para la congelación (no se deciden aquí)

- **Q-A4-03**: `CLAUSE_MAP_MODE` (`RESOLVE` o `DECLARED_UNAVAILABLE`) según lo que exija el contrato de VERIFY; con `RESOLVE`, los pares (propuesta:
  uno presente en `Entries` y uno ausente).
- Si la declaración de shell prohíbe también intérpretes (`py`, `python`, `node`): tiene que coincidir con el texto de la orden O4. Riesgo conocido: en
  la sonda del bloque falló un intento con `py` (A-4, Anexo, punto 1).
- `BASE_SHA`/`HEAD_SHA` (propuesta: `d30fb6a9…..ae25b596…`, 3 rutas) y los archivos y campos de los pasos 5-7.
- Ruta de la evidencia (propuesta: `docs/automation/evidence/I-62-F6/OD-2/<RunId>-a4-1-probe/`).
- Comprobación de la herramienta: `build-prompt`, `reference` y `compare` se ensayaron sobre un clon temporal de lectura del origen del fixture, con
  una salida sintética (positiva y negativa), sin invocar ningún modelo ni ningún binario de Codex; el clon temporal se borró.

## 6. Archivos (SHA-256 del borrador)

| Archivo | Uso | SHA-256 |
|---|---|---|
| `run_probe.sh` | `measure` / `probe` / `compare`, con las puertas | `518e8b98d594ba60d4b236125fe919b4fbb081d49caf969423a34bb13a14a912` |
| `probe_tools.py` | arma el texto, calcula la referencia después de la sonda y compara | `88617e85a5b6ab9195c5d48fca403952328b7979261376c805d7bc3d8e8bfae4` |
| `probe-prompt.template.txt` | texto de la sonda (pasos 1-8) | `c85d6ab2a65bdeddb29113601f772ecd230784292d8ca033bd9e9066a8e85bd4` |
| `shell-declaration.txt` | declaración de shell (la misma que irá en la orden O4) | `d8ae84d1bce74f50ba9c133c7241104ff5aa5624f5fa0bc258fefef416149a98` |
| `clause-map-step.RESOLVE.txt` | paso 8 con resolución por el mapa | `9361c3506c55b62d1a511d10577322ac0f2955bb6f09d33f68bd4308a0253b00` |
| `clause-map-step.DECLARED_UNAVAILABLE.txt` | paso 8 con la comprobación de la declaración | `cd648e174f9040dfb82a241bb210e93e23c8a772a9d4078bc7a66b2886c101b4` |
| `probe-schema.json` | esquema de la salida (`--output-schema`) | `b5d1db358a67ff05f85a66baf7c76d0465ae2ee4a2fdce085c64836eef894bf7` |
| `gates.env.template` | puertas y parámetros que se rellenan tras las autorizaciones | `dfcabd17d01760bd4cbf279709de2a3c1ecef1488d7f9aeee4907f4cb6d0ea0f` |
| `consumo-line.txt` | línea literal `A4-SONDA-CONSUMO` de A-4 §7 (blob `27ffa26b`) | `3ea3ed9ef4a541fcf10fbcc4a3baa3e3b96f8c34d5394640989c415a2f1f82b8` |
| `cfg-fp.ps1` | huella de `config.toml` (SHA-256 y nombres saneados; nunca valores); copia exacta de la del bloque A2-P2 | `b1d833c0abc06fec6e27367b6df2715e7cdca321f0d6c6fbadb9f634e6a31bab` |
| `baseline-keynames-6518EFAB.json` | los 107 nombres saneados de la huella aceptada; copia exacta | `076637c550ff0b3109447ef00263ff087749f0aaa71f299d524e43b8cf82b70b` |
