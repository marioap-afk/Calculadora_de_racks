# Sonda medida de A4-1 — kit en borrador (NO EJECUTADO), actualizado a la corrección 2 de A-4

> **BORRADOR. No se ha ejecutado, no está congelado y no está autorizado.** Lo prepara la supervisión (plano a). Parte del borrador custodiado en
> `57877a7d` (`kits/FX-02/drafts/a4-probe-draft/`) y lo actualiza a la candidata A-4 **corrección 2** (`docs/initiatives/I-62-A-4.md`, blob
> `0d954376`; evidencia §105).
>
> Decisiones §62.4: «preparar la medición de A4-1 sin ejecutarla». Decisiones §61.4: «La sonda adicional del Controller no se gasta antes del acuerdo
> de A-4 y de `A4-SONDA-CONSUMO = A`». Nada de este directorio se ha lanzado y no existe `out/`. Sin las puertas de §2, `run_probe.sh probe` se niega
> a correr.

## 1. Qué mide

Mide la invocación medida adicional de **A4-1, regla 2** (A-4 `0d954376`, §3.2). Con la shell declarada, la celda del Controller de FX-02 tiene que
demostrar las operaciones que necesita la verificación. La corrección 2 añade a la regla las lecturas de Git de `Identity`, `Remote`, `CleanTree`
(estado e ignorados) y `Trailer` (historia) (A62-A4-O1). En el texto de la sonda son los pasos 9-12.

| Operación de A4-1, regla 2 | Paso | Criterio de `probe_tools.py compare` (propuesta; decide el Coordinator) |
|---|---|---|
| `git diff --name-only <base>..<head>` reproducible | 3 y 4 (dos ejecuciones) | las dos listas = la de referencia; `identical` = `yes` |
| lecturas de Git de `Identity` (AP 16.9 #5) | 9: `rev-parse --abbrev-ref HEAD`, `--show-toplevel`, `refs/remotes/origin/<rama>`; `merge-base --is-ancestor <base> <head>` | todos = referencia (ruta normalizada) |
| lecturas de Git de `Remote` (AP 16.9 #6) | 10: `ls-remote origin` de la rama y de `main`; `rev-parse refs/remotes/origin/main` | todos = referencia; referencia válida solo si `ls-remote` no cambió entre antes de la sonda, después y la comparación |
| lecturas de Git de `CleanTree`, estado e ignorados (AP 16.9 #8) | 11: `status --porcelain`, `status --porcelain --ignored`, `check-ignore -v --no-index <ruta>` y su código de salida | todos = referencia (líneas normalizadas) |
| lecturas de Git de `Trailer`, historia (AP 16.9 #11) | 12: `rev-list --reverse <base>..<head>` y, por commit, `log -1 --format=%(trailers:key=Co-Authored-By,valueonly)` | lista de commits y trailers = referencia |
| cálculo de blobs y hashes | 5 (`git hash-object`, `git rev-parse HEAD:<f>`) y 6 (`certutil -hashfile … SHA256`) | todos = referencia |
| lectura de las entradas JSON | 7 (contrato de T1: campos y longitudes de arrays) | todos = referencia |
| resolución por el mapa de cláusulas, o comprobación de su no disponibilidad declarada | 8 (`RESOLVE` o `DECLARED_UNAVAILABLE`; Q-A4-03) | todos = referencia |
| shell declarada `cmd.exe` | 1 (`ver`, `%ComSpec%`) y auditoría de los comandos de los eventos | `ComSpec` con `cmd.exe`; los intentos con PowerShell o con intérpretes se registran para la disposición |

Además se exige: salida del proceso 0 sin tope vencido; identidad igual antes y después; `HEAD` y árbol del clon sin cambios. Una clave pedida que
falte nunca casa, ni siquiera con una referencia vacía.

Veredictos propuestos:
- `ALL_OPERATIONS_DEMONSTRATED`: la elegibilidad para la planificación y la verificación (incluidos los negativos con invocación) la declara el
  Coordinator. La línea del Owner «no acepta su resultado».
- `NOT_DEMONSTRATED`: la celda no es elegible. La invocación **no se repite** bajo A4-1 (regla 4) y FX-02 sigue UNVERIFIED con esa causa. Solo una
  actualización observada (bloque A2-P2, A-2 §3.3 regla 3) u otra A-n abren otra medición.
- `REFERENCE_UNSTABLE` (nuevo): el origen cambió durante la medición, así que la referencia de `Remote` no se pudo fijar. Es un defecto de la
  referencia, no de la celda. Lo dispone el Coordinator y no hay reintento automático.

## 2. Puertas (todas, en este orden; `run_probe.sh probe` se niega a correr sin las comprobables)

1. **A-4 AGREED, incluida A4-1.** Requiere revisión formal acreditada y veredicto del Coordinator sobre la versión acordada (decisiones §61.2 y
   §62.2). Comprobación: `A4_AGREED_MARKER` literal en el archivo de decisiones.
2. **Disposición de aplicación del Coordinator de I-62.** Tiene que nombrar **la celda, la shell y el trío exacto** y **fijar el directorio de
   trabajo (`-C`) de la medición**: un clon con la historia que necesita `git diff <base>..<head>` (A4-1, regla 3, corrección 2; A62-A4-O2).
   Comprobaciones:
   - `DISPOSITION_MARKER` literal;
   - `DISPOSITION_DIR_MARKER` literal, y tiene que contener `PROBE_DIR` (sin distinguir mayúsculas ni el tipo de barra);
   - `CELL`, `SHELL_NAME` y `TRIO` iguales a las constantes del script (`gpt-6-luna/high`, `cmd.exe`, `3553cd6e…/26.1002.7124.0/6518EFAB…`). Si la
     disposición nombra otra cosa, el kit se rehace.
3. **`A4-SONDA-CONSUMO = A`** del Owner, en la misma solicitud que `A4-CLAUDE-FX02-CONSUMO` (decisiones §61.5 y §62.5; el silencio no es
   aprobación). Comprobación: la línea de `consumo-line.txt` aparece literal, como línea completa, en el archivo de decisiones. Es byte a byte la de
   A-4 §7 en `0d954376`, igual que en `27ffa26b`. Si la versión acordada cambia el texto, se sustituye antes de congelar.
4. **P-07: tope de uno en total para FX-02 en F6** (A4-1, regla 3, corrección 2). Se cuenta aparte: fuera de los lanzamientos de la ronda, del
   sumando «+ 2 sondas» y de los bloques de A2-P2. Comprobaciones:
   - el marcador `out/PROBE_LAUNCHED` se crea con `noclobber` justo antes de lanzar y nunca se borra; si existe, o si hay salidas `out/probe-*`, el
     script se niega;
   - **nuevo:** el script se niega también si `EVIDENCE_ROOT` (la carpeta `I-62-F6/OD-2/` de la evidencia real) ya tiene una medición
     `*a4-1-probe*`.

   La regla 4 solo abre otra medición con un bloque A2-P2 nuevo por una actualización nueva. Eso cambia el trío, así que exige otro kit y otra
   disposición. El script no puede ver copias del kit fuera de este directorio ni mediciones no custodiadas: eso lo comprueba la supervisión.
5. **Sin reintento.** El script no tiene bucle de reintento. Un fallo, un tope vencido o un lanzamiento incierto cuentan como el único lanzamiento.
6. **Identidad aceptada** (OD-2e = A, decisiones §59): huella `6518EFAB…`, estructura de 107 nombres saneados igual a la línea base, binario
   `3553cd6e…` (el único `codex.exe`) y app `26.1002.7124.0`.
   - Se comprueba antes de lanzar (no se lanza si difiere) y después.
   - Una diferencia es P-01, exige una OD-2 nueva (OD-2-MAT = A) y anula la medición (A4-1, regla 5).
   - Nunca se leen valores de `config.toml` ni se calculan digests por clave.
7. **Directorio: el que fija la disposición.**
   - Si es `D:\r62-fixture\A2`: solo tras S03 y antes de S04. El script se niega si A2 ya tiene directorio de proyecto de Claude («nunca `-C A2` con
     A2 abierta»).
   - Se niega también con `B`, `B2`, `B3`, `R`, `A6`, `arch`, `arch02`, `evidence-out` y cualquier `*.git`.
   - Requisitos del clon: `origin` = el origen del fixture, `core.autocrlf=false`, árbol limpio, y `BASE_SHA`, `HEAD_SHA` y los archivos en su historia.
   - **Nuevo:** el clon tiene que tener `refs/remotes/origin/<BRANCH>` y `refs/remotes/origin/main` (Identity y Remote).
   - **Nuevo:** entre la medición previa y `compare` no puede haber escrituras en el origen del fixture. El script registra `git ls-remote origin`
     antes y después de la sonda; si cambia, el veredicto es `REFERENCE_UNSTABLE`.
8. **Congelación tras el AGREED y antes de lanzar.** Se custodian con su SHA-256 en la evidencia de RackCad: `gates.env`, el texto de la sonda ya
   armado (`out/probe-prompt.txt` y su SHA-256), la declaración de shell, el esquema y los scripts. **El texto de `shell-declaration.txt` es el mismo
   que el bloque de transporte de la orden O4 pondrá en `Shell:`** (A4-1, regla 1). Si la disposición fija otro texto, se cambia aquí antes de
   congelar.

El script **no** puede comprobar, y queda a cargo de la supervisión:
- que los marcadores copiados en `gates.env` sean de verdad el registro del acuerdo, de la disposición y de la línea del Owner;
- los invalidadores que no observa: estado de autenticación, blobs de catálogo y de routing, e instancia del host.

## 3. Procedimiento (solo cuando se cumplan las puertas)

1. Copiar `gates.env.template` a `gates.env` y rellenarlo con lo registrado. No puede quedar ningún `<…>`.
2. Congelar (puerta 8): SHA-256 de todos los archivos del kit y de `gates.env`, en custodia.
3. `bash run_probe.sh probe`, una sola vez. El script:
   - arma el texto con `probe_tools.py build-prompt`;
   - mide la identidad, registra `git ls-remote origin` y crea el marcador;
   - lanza la receta 16.4 literal: `-C`, `-s read-only`, `-m gpt-6-luna`, `-c model_reasoning_effort="high"`, `--output-schema`, `-o`, `--json`,
     stdin cerrado y el `pwsh` del runtime primero en el `PATH`;
   - aplica un tope de 600 s y, si vence, `taskkill /T /F` del árbol;
   - vuelve a registrar `ls-remote` y a medir la identidad.
4. `bash run_probe.sh compare`. Calcula la referencia **después** de la sonda (así ningún valor esperado existe en disco mientras corre),
   comprueba la estabilidad de la referencia de `Remote` y emite `out/comparison.json`.
5. Del registro de sesión de Codex de **esta** invocación (`out/probe-new-sessions.txt`), la supervisión lee solo el modelo y el effort efectivos.
   Después sanea las salidas antes de custodiarlas: el `stderr` y los eventos pueden contener rutas con el nombre del usuario de Windows, que se
   sustituyen por `%USERPROFILE%` y `%LOCALAPPDATA%`. La custodia va en `EVIDENCE_ROOT/<RunId>-a4-1-probe/`, que activa la puerta 4 para cualquier
   intento posterior.
6. Resultado al Coordinator. Con la elegibilidad declarada, el bloque de transporte de la orden O4 va en S13 (`Shell`, `Cells`, `MeasurementRecord`
   según U-15). El registro de sesión de Codex de esta sonda entra en LB-1 de la tarjeta de A2.

## 4. Diferencias con `run_block.sh` (bloque A2-P2)

- **Una sola sonda.** Sin modo `version`: `codex --version` y `codex login status` son invocaciones que la línea de consumo no nombra. La versión de
  la CLI (`0.162.0-alpha.2`) se deduce del binario medido (evidencia §93).
- **Lo que añade este kit:**
  - las puertas de §2 y el tope de uno en total, que se comprueba también contra la evidencia;
  - el tope de 600 s de la receta (el bloque no lo aplicaba);
  - el texto con la shell declarada y 12 pasos;
  - el esquema con `commands` y `values` clave/valor;
  - la referencia y la comparación posteriores, con la estabilidad de `ls-remote`;
  - la lista de directorios no admitidos.
- **Lo que queda igual:**
  - `measure`: huella exacta, estructura saneada, binario, número de `codex.exe` y versión de la app; STOP con exit 3;
  - la línea de comando de `codex exec`;
  - el registro de `HEAD`, del estado del árbol y de los nombres de las sesiones nuevas.

## 5. Puntos abiertos para la congelación (no se deciden aquí)

- **Q-A4-03:** `CLAUSE_MAP_MODE` (`RESOLVE` o `DECLARED_UNAVAILABLE`) según lo que exija el contrato de VERIFY. Con `RESOLVE`, los pares (propuesta:
  uno presente en `Entries` y uno ausente).
- **Lista exacta de las lecturas de Git de VERIFY.** La regla 2 nombra las comprobaciones, no los comandos. Los pasos 9-12 son una propuesta mínima
  y fiel a AP 16.9 #5, #6, #8 y #11. Si el contrato del Controller o la orden O4 usan otras formas (por ejemplo, la comprobación de `RedSha`), se
  ajustan aquí antes de congelar.
- **Intérpretes.** Hay que decidir si la declaración de shell prohíbe también `py`, `python` y `node`, y tiene que coincidir con el texto de la
  orden O4. Riesgo conocido: en la sonda del bloque falló un intento con `py` (A-4, Anexo, punto 1).
- **`BASE_SHA` y `HEAD_SHA`.** Propuesta: `d30fb6a9…..ae25b596…`, un commit con trailer y 3 rutas. Alternativa: `1a4fc9c6…..ae25b596…`, dos
  commits (uno con `Co-Authored-By` y otro sin él) y 5 rutas, que ejerce `Trailer` en los dos sentidos. Comprobado con Git de lectura sobre el
  origen del fixture.
- **`IGNORE_PATH`.** Propuesta: una ruta del área transitoria (`artifacts/orchestration/…`). Con el clon en `c785def9` o posterior (el `.gitignore` de
  U-08, evidencia §106, commit `a39a1ada`) la ruta queda ignorada; en un clon anterior, la lectura es igual de reproducible pero no ejerce un
  caso ignorado.
- **Riesgos no medidos, que la sonda misma pondrá a prueba:**
  - `git ls-remote origin` contra el repositorio bare local bajo el sandbox de solo lectura;
  - `%(trailers…)` bajo `cmd.exe`. Lleva un solo `%`, así que no hay expansión de variable de `cmd`, pero no se ha medido.
  - La sonda 1 del bloque A2-P2 solo ejerció lecturas de Git sencillas.
- **Ruta de la evidencia.** Propuesta: `docs/automation/evidence/I-62-F6/OD-2/<RunId>-a4-1-probe/`, que es lo que lee la puerta 4.
- **Comprobación de la herramienta.** `build-prompt`, `reference` y `compare` se ensayaron sobre un clon temporal de lectura del origen del fixture
  (`fx/u1` = `c785def9`), con salidas sintéticas. Resultados:
  - una positiva: `ALL_OPERATIONS_DEMONSTRATED`;
  - tres negativas (falta la clave `status`, un trailer distinto, falta un trailer vacío): `NOT_DEMONSTRATED`;
  - `ls-remote` alterado: `REFERENCE_UNSTABLE`;
  - `ls-remote` alterado más un trailer distinto: `NOT_DEMONSTRATED`.

  En el ensayo no se invocó ningún modelo ni ningún binario de Codex y no se leyó `~/.codex`. El clon temporal se borró. `bash -n` y
  `py_compile` pasan.

## 6. Cambios frente al borrador de `57877a7d`

| Archivo | Cambio |
|---|---|
| `probe-prompt.template.txt` | Pasos 9-12 (Identity, Remote, CleanTree con estado e ignorados, Trailer con historia). Prohibición general de `git fetch`, `git pull` y escrituras. `id` de 1 a 12 |
| `probe-schema.json` | Mismas palabras de validación que antes (no se introduce ninguna que el bloque A2-P2 no haya ejercido). Solo se añaden `description` a la raíz y a `id` (pasos 1-12) |
| `gates.env.template` | Cabecera con la corrección 2. Nuevos `DISPOSITION_DIR_MARKER` (regla 3: la disposición fija el `-C`), `EVIDENCE_ROOT` (tope uno en total), `BRANCH` e `IGNORE_PATH`. Alternativa de `BASE_SHA`. Nota de igualdad de la línea de consumo con `0d954376` |
| `run_probe.sh` | `load_gates` con las variables nuevas. `check_gates`: directorio en la disposición, tope total contra `EVIDENCE_ROOT`, `IGNORE_PATH` relativa, refs `origin/<rama>` y `origin/main`. `probe`: `ls-remote origin` antes y después |
| `probe_tools.py` | `build-prompt` con `{{BRANCH}}`, `{{IGNORE_PATH}}` y `<commit>`. `reference`: pasos 9-12 y estabilidad de `ls-remote`. `compare`: OP5-OP8, claves ausentes que nunca casan (también en el paso 8: corrige un falso positivo posible con referencias vacías) y veredicto `REFERENCE_UNSTABLE` |
| `README.md` | Este texto |
| sin cambio | `shell-declaration.txt`, `clause-map-step.*.txt`, `consumo-line.txt` (igual byte a byte a A-4 §7 en `0d954376`), `cfg-fp.ps1`, `baseline-keynames-6518EFAB.json` |

## 7. Archivos (SHA-256 de este borrador)

| Archivo | Uso | SHA-256 |
|---|---|---|
| `run_probe.sh` | `measure`, `probe` y `compare`, con las puertas | `3ea41cabd5d6055d5d22675dcdd2f6a52d5f428c19862a0229f19f53d9d45cd3` |
| `probe_tools.py` | arma el texto, calcula la referencia después de la sonda y compara | `6efe3aa2bbf1356aed4924994ea4eb5ae2cee88b9d544c99f3dbd6403461e9f8` |
| `probe-prompt.template.txt` | texto de la sonda (pasos 1-12) | `63db9d10f362738d3a53ee9aa251a998708d3c59b25c274b3989a5d88465ac9c` |
| `shell-declaration.txt` | declaración de shell (la misma que irá en la orden O4) | `d8ae84d1bce74f50ba9c133c7241104ff5aa5624f5fa0bc258fefef416149a98` |
| `clause-map-step.RESOLVE.txt` | paso 8 con resolución por el mapa | `9361c3506c55b62d1a511d10577322ac0f2955bb6f09d33f68bd4308a0253b00` |
| `clause-map-step.DECLARED_UNAVAILABLE.txt` | paso 8 con la comprobación de la declaración | `cd648e174f9040dfb82a241bb210e93e23c8a772a9d4078bc7a66b2886c101b4` |
| `probe-schema.json` | esquema de la salida (`--output-schema`) | `e4c3a57dd5f46d0fffe10950dfb8534edeec81b3ffe2e0b6b3893f61595ad105` |
| `gates.env.template` | puertas y parámetros que se rellenan tras las autorizaciones | `2bd0e7fa614253c262bf0789a7abc5db73df09153b125e11af6126c2c9b2b990` |
| `consumo-line.txt` | línea literal `A4-SONDA-CONSUMO` de A-4 §7 (`0d954376`, igual en `27ffa26b`) | `3ea3ed9ef4a541fcf10fbcc4a3baa3e3b96f8c34d5394640989c415a2f1f82b8` |
| `cfg-fp.ps1` | huella de `config.toml` (SHA-256 y nombres saneados, nunca valores); copia exacta de la del bloque A2-P2 | `b1d833c0abc06fec6e27367b6df2715e7cdca321f0d6c6fbadb9f634e6a31bab` |
| `baseline-keynames-6518EFAB.json` | los 107 nombres saneados de la huella aceptada; copia exacta | `076637c550ff0b3109447ef00263ff087749f0aaa71f299d524e43b8cf82b70b` |
