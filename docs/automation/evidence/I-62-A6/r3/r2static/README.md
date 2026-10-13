# Banco de la ronda 3 de A-6 (I-62): formato real de los eventos de Codex y comprobaciones (v)-(viii) en el punto de uso

Este paquete solo contiene pruebas y resultados. No redacta ni edita el texto de la enmienda. No se invocó ningún modelo ni
ninguna CLI de modelo (Codex, claude), no se ejecutó nada bajo `%USERPROFILE%\.codex` ni bajo `%USERPROFILE%\.cache\codex-runtimes`,
no hubo red y no se leyeron `auth.json`, credenciales ni `config.toml`. El worktree de I-62 solo se leyó (archivos y
`git --no-replace-objects -C <wt> cat-file blob`). Todo lo escrito está en `D:\r62-a6r3\pou\` (clones desechables) y en esta carpeta.
Las órdenes se ejecutaron en secuencia.

## 1. Entorno exacto

| Elemento | Valor |
|---|---|
| Sistema | Windows 11 Pro 10.0.26300 (`Windows-11-10.0.26300-SP0`), NTFS |
| Git declarado | `%USERPROFILE%\AppData\Local\Programs\Git\cmd\git.exe` (el primero del PATH; es el lanzador de Git para Windows), `git version 2.54.0.windows.1`, SHA-256 `81ef35ae005ca9318018d18e3327578ce939fb99feaad6b2d7c8ab15f3de8db5`, 46 480 bytes |
| Binario real detrás del lanzador (informativo) | `%USERPROFILE%\AppData\Local\Programs\Git\mingw64\bin\git.exe`, misma versión, SHA-256 `cab4c4eea1d869cf9f7be73868dc9a90ad2df1b1b673e5f8c8714a576c25ea96`, 4 422 544 bytes |
| Intérprete declarado (el de la ronda 2) | ruta `%USERPROFILE%\AppData\Local\Microsoft\WindowsApps\python.exe` (alias de ejecución de aplicación, etiqueta de reanálisis `0x8000001B`; es lo que resuelve `python` en el PATH), destino `C:\Program Files\WindowsApps\PythonSoftwareFoundation.Python.3.13_3.13.3824.0_x64__qbz5n2kfra8p0\python3.13.exe`, `Python 3.13.14`, SHA-256 del destino `ad169f4cb4bfb78c7a5c030a4529c19d6643276778e33994c93e145b6191c3ec` (171 744 bytes), **igual** al `Interpreter.ExecutableSha256` de la ronda 2 (`r1r4/inputs-t1/request.json`) |
| Shell de (iii) | pwsh 7 del PATH: `C:\Program Files\WindowsApps\Microsoft.PowerShell_7.6.6.0_x64__8wekyb3d8bbwe\pwsh.exe`, 7.6.6, FullLanguage, SHA-256 `bfb46af89433268872ddb43d1ca7a3f433452ee91ed356a9786940f90118e285`. **No** es el pwsh del runtime de Codex (`362a356c…`, Parte A) |
| Variables heredadas relevantes | `NoDefaultCurrentDirectoryInExePath=1`; `PATHEXT=.COM;.EXE;.BAT;.CMD;.VBS;.VBE;.JS;.JSE;.WSF;.WSH;.MSC;.CPL`; **`GIT_ASKPASS=` (vacía), `GIT_EDITOR=true`, `GIT_TERMINAL_PROMPT=0`**, que vienen del host del banco (la sesión de Claude Code) |
| Instrumento | `bmin_facts.py`, blob `9a829b0eeb1210238052b1feac06cfa8f7964aed`, SHA-256 `b03c3a19cef608386afd7c2e0618b884bfd7b7f6913e95d8ed70272d517a0200`, 64 764 bytes (igual que en la ronda 2) |
| Fixture | `D:/r62-fixture/fixture-origin.git`, rama `fx/u1` (solo como origen de `git clone`) |

Hechos del host medidos de paso: el `python.exe` del paquete (bajo `C:\Program Files\WindowsApps\…`) no se puede ejecutar
por su ruta (`Acceso denegado`), solo a través del alias; y el alias no se puede leer como archivo (`Errno 22`), así que su SHA-256 solo se
establece leyendo el destino del punto de reanálisis.

## 2. Cómo reproducir

```powershell
python -I -B part_a_events.py   # Parte A: análisis estático de los 9 registros -> part-a-events.json
python -I -B run_pou.py         # Parte B: clones, P1, registro de la configuración y casos -> part-b-pou.json
python -I -B summarize.py       # tablas de este README; --hashes lista el SHA-256 de cada archivo
```

`pou_check.py` es el script de comprobación de la supervisión; se puede usar solo:

```powershell
python -I pou_check.py --snapshot-config --repo <T> --decl decl.json > registered-config.json   # en la escritura de preparación
python -I pou_check.py --repo <T> --decl decl.json --registered-config registered-config.json --out result.json
```

Se ejecuta en el mismo entorno con el que la sesión lanzará la invocación (o con `--launch-env-json`). Sale con 0 solo si
`Launch` es true, con 1 si es false y con 2 si hay un error interno. Cualquier salida distinta de 0, o un resultado ausente o
ilegible, significa «no lanzar». `benchlib.py` tiene las utilidades comunes (saneado, JSON canónico, instantáneas de la ronda 2).

Los resultados son JSON canónico ASCII (claves ordenadas, separadores `,` `:`, terminado en LF) y pasan por un saneado: el
prefijo de USERPROFILE se escribe `%USERPROFILE%`, el nombre de la cuenta y su forma corta 8.3 se escriben `<usuario>`, y las
direcciones de correo se escriben `<email>`. `write_result` se niega a escribir si después del saneado queda alguna de esas
formas. Los resultados crudos de cada caso (sin sanear) quedan en `D:\r62-a6r3\pou\cases\<caso>.meta\`.

## 3. Parte A: formato de los registros de eventos `--json` de Codex (estático, solo lectura)

Fuente: los 9 `*events.jsonl` custodiados en `docs/automation/evidence/I-62-F6/OD-2/*/` y `OD-2b-PROBE/*/` del worktree de I-62
(ya saneados por quien los custodió). Evidencia: `part-a-events.json` (por registro: tipos de evento, ítems, completitud,
campos de `command_execution`, tamaños, terminadores, marcas de stderr con extracto, marcas de truncado, `exit_code`/`status`,
y la versión leída de los archivos vecinos con el nombre del archivo fuente).

### 3.1 Por registro

| Registro | CLI / binario / app | Eventos | command_execution | Shell | Máx. bytes de `aggregated_output` | CRLF / LF sueltos | Ítems con texto de stderr | Completo |
|---|---|---|---|---|---|---|---|---|
| `OD-2b-PROBE/R20261006T150704Z-od2b/probe1` | 0.160.0 / `37762753` / — | thread.started 1, turn.started 1, item.started 9, item.completed 11, turn.completed 1 | 9 | pwsh del runtime 9 | 2 984 | 50 / 51 | 8 | sí |
| `OD-2b-PROBE/R20261006T150704Z-od2b/probe2` | 0.160.0 / `37762753` / — | 1, 1, 6, 8, 1 | 6 | pwsh del runtime 6 | 217 | 2 / 14 | 1 | sí |
| `OD-2/R20261007T013200Z-od2d-probe/probe1` | 0.160.1 / `3b8f6e33` / 26.930.7945.0 | 1, 1, 8, 9, 1 | 8 | pwsh del runtime 8 | 248 | 10 / 3 | 7 | sí |
| `OD-2/R20261007T013200Z-od2d-probe/probe2` | 0.160.1 / `3b8f6e33` / 26.930.7945.0 | 1, 1, 6, 8, 1 | 6 | pwsh del runtime 6 | 218 | 8 / 7 | 1 | sí |
| `OD-2/R20261009T041427Z-a2p2-block/probe1` | 0.162.0-alpha.2 / `3553cd6e` / 26.1002.7124.0 | 1, 1, 19, 21, 1 | 19 | cmd.exe 16, alias `WindowsApps\pwsh.exe` 3 | 4 152 | 14 / 75 | 4 | sí |
| `OD-2/R20261009T041427Z-a2p2-block/probe2` | 0.162.0-alpha.2 / `3553cd6e` / 26.1002.7124.0 | 1, 1, 6, 7, 1 | 6 | alias `WindowsApps\pwsh.exe` 6 | 390 | 0 / 6 | 0 | sí |
| `OD-2/R20261010T000542Z-a4-1-probe/probe` | — / `3553cd6e` / 26.1002.7124.0 | 1, 1, 11, 12, 1 | 11 | cmd.exe 11 | 11 672 | 151 / 767 | 5 | sí |
| `OD-2/R20261011T050156Z-a2p2-v4-controller/probe` | — / **`d13914ce`** / **26.1007.2314.0** | 1, 1, 4, 5, 1 | 4 | pwsh del runtime 4 | **59 805** | 307 / 1 430 | 4 | sí |
| `OD-2/R20261011T102825Z-a2p2-v5-controller/probe` | — / **`d13914ce`** / **26.1007.2314.0** | 1, 1, 42, 45, 1 | 42 | pwsh del runtime 42 | 29 675 | 266 / 56 | 42 | sí |

Versiones: `version.txt`/`pre-version.txt` (CLI), `measurements.log` (`bin=` y `app=`), `result.json` y `README.md` de cada
carpeta. Para las dos sondas del Controller del 2026-10-11 el binario es `d13914ced6c7…` y la app 26.1007.2314.0, como se
esperaba; su `runtime-pwsh.sha256` es `362a356ce7f0940e…`. Su versión de CLI no se registró (la observación pasiva de
`R20261011T034757Z-s68-passive` la deja en `NotObserved`). La sonda A4-1 no tiene `version.txt`: su binario es el mismo
`3553cd6e…` de la versión 0.162.0-alpha.2.

### 3.2 Lo que muestran los datos

1. **Estructura y completitud.** Los 9 registros tienen exactamente un `thread.started`, un `turn.started` y un
   `turn.completed` (con `usage`), ningún `turn.failed` ni `error`, ningún error de JSON, y empiezan por `thread.started` y
   terminan en `turn.completed`. Los 111 `item.started` tienen su `item.completed` con el mismo `id`, después. Solo
   `command_execution` tiene `item.started`; los `agent_message` (15) solo aparecen como `item.completed`. Los únicos tipos
   de ítem son `command_execution` y `agent_message`.
2. **Campos de `command_execution`:** `id`, `type`, `command`, `aggregated_output`, `exit_code`, `status`. **No hay campo
   separado de stdout ni de stderr.** En `item.started`, `aggregated_output` = `""`, `exit_code` = null y `status` =
   `in_progress` (111/111), y `command` es igual al del `item.completed` (111/111).
3. **`command` es siempre una sola cadena** (111/111, ninguna lista) que incluye la shell: `"<ruta de la shell>" -Command '<script>'`
   (o `/c` con cmd.exe, o `-NoProfile -Command`), con comillas al estilo POSIX (`'"'"'`). La ruta de la shell aparece con las
   barras invertidas duplicadas en los 111 casos, mientras que la misma ruta impresa por el proceso (`(Get-Process -Id $PID).Path`)
   sale con barras simples. **No es la línea de órdenes ejecutada:** en los 9 fallos de creación de proceso de la versión
   0.162.0-alpha.2, el texto de Codex muestra la línea real, y en los 9 difiere de `command`: lleva delante
   `try { [Console]::OutputEncoding=[System.Text.Encoding]::UTF8 } catch {}` y un salto de línea, y usa comillas dobles.
4. **Shells.** pwsh del runtime (`%USERPROFILE%\.cache\codex-runtimes\codex-primary-runtime\dependencies\native\powershell\pwsh.exe`):
   75 órdenes. cmd.exe (`C:\WINDOWS\system32\cmd.exe`): 27. Alias `%USERPROFILE%\AppData\Local\Microsoft\WindowsApps\pwsh.exe`:
   9, y las 9 fallaron al crearse (`exit_code` −1, `status` `failed`, «CreateProcessAsUserW failed: 5 (Acceso denegado.)»,
   `si_flags=256` = `STARTF_USESTDHANDLES`, `creation_flags=0x8080400`). En esos 9, `aggregated_output` no es salida de ningún
   proceso: es el mensaje de error de Codex.
5. **Tamaños** (bytes UTF-8 del texto custodiado): 111 ítems; mínimo 0, mediana 355, P90 4 929, **máximo 59 805**
   (v4 `item_2`). Reparto: 0 → 5; 1-1 023 → 79; 1 K-4 K → 14; 4 K-16 K → 10; 16 K-64 K → 3; ≥ 64 KiB → 0.
6. **stdout y stderr llegan mezclados** en `aggregated_output`, sin delimitador y en un orden coherente con el de ejecución. 72 de 111
   ítems contienen texto que su programa escribe en el flujo de error: `profile.ps1: Cannot dot-source this command because
   it was defined in a different language mode…` (pwsh del runtime sin `-NoProfile`, 63 ítems), registros de error de
   PowerShell (`InvalidOperation:` + `Line |`), `fatal:` de git, `Traceback` de Python, «no se reconoce como un comando interno
   o externo» de cmd y `FINDSTR:`. En 63 ítems el texto de error va **primero**, y en 57 de ellos le sigue la salida normal
   del programa. En 4 ítems (A4-1, cmd.exe) aparece salida → `fatal:` de git → salida (por ejemplo, `STEP4` y después
   `fatal: ambiguous argument …` entre los marcadores de `echo`). Con el pwsh del runtime, las 10 órdenes con `-NoProfile`
   (od2d y od2b, sonda 2) no tienen la línea de `profile.ps1`, y 63 de las 65 sin `-NoProfile` sí la tienen (las 42 de v5,
   receta sin `-NoProfile` y ConstrainedLanguage, todas). Las 2 excepciones son `git branch --show-current` en od2d y od2b
   (0.160.x): su salida es solo `fx/u1` + LF. Los datos no dicen por qué (puede que esa orden no pasara por el perfil o por pwsh).
7. **Terminadores de línea:** en total 808 CRLF, 2 409 LF sueltos y 0 CR sueltos. Por ítem: solo CRLF 55, solo LF 22,
   mezclados 29, sin salto 5. La mezcla ocurre dentro de un mismo ítem: con cmd.exe, las líneas de `echo` llevan CRLF y la salida
   de git LF (A4-1 `item_3`); con el pwsh del runtime, el `fatal:` de git llega con LF y el `Traceback` de Python con CRLF (v5
   `item_35`), y la salida de `git log` con LF queda entre líneas de pwsh con CRLF (v5 `item_9`). Codex no normaliza los
   terminadores en lo que se ve.
8. **Codificación:** `aggregated_output` es una cadena JSON (Unicode), no bytes. 8 ítems contienen U+FFFD (299 en total): los
   bytes que no eran UTF-8 válido se sustituyeron y no se pueden recuperar (por ejemplo, en v4/v5 la `ñ` de «Añadir» llega
   como U+FFFD; allí el pwsh del runtime está en ConstrainedLanguage). 17 ítems tienen caracteres no ASCII. No hay ESC ni NUL.
9. **Truncado:** ninguna marca (`truncat`, `[...`, `… (N`, `omitted`, `Total output lines`, `…`) en los 111 ítems. 10 ítems no
   terminan en salto de línea: los 9 fallos de creación (texto de Codex) y A4-1 `item_9` (`type` de un TRX que termina sin salto,
   `exit_code` 0). En los 4 ítems cuya orden imprime marcadores `===X===`, todos los marcadores aparecen y en orden, también en
   el mayor (59 805 bytes). No hay señal de corte hasta ~60 KB; por encima no hay datos.
10. **`exit_code` y `status`:** 0 → 85, 1 → 13, −1 → 9, 128 → 1, 255 → 2, 256 → 1. `status` es `completed` exactamente cuando
    `exit_code` = 0 (85) y `failed` en los demás (26). −1 significa que Codex no creó el proceso.
11. **Custodia:** los registros están saneados (rutas de usuario sustituidas), así que el texto custodiado ya no es el original
    allí donde aparecía la ruta; las direcciones de correo de un tráiler siguen en el registro custodiado.
12. **Python desde el pwsh del runtime:** 24 órdenes de v5 invocan `python` (21 con salida 0; las 3 con 1 terminan en un
    `Traceback` del propio script). Ningún registro contiene un tubo entre dos programas nativos (`git … | python …`).

### 3.3 Conclusión de la Parte A (solo con estos datos)

**¿Se pueden recuperar los bytes exactos del stdout de una orden?** En general, **no**:

- `aggregated_output` mezcla stdout y stderr sin delimitador, y la shell del runtime añade su propio texto de error (`profile.ps1`)
  al principio de cada orden lanzada sin `-NoProfile`. Desde el registro no se puede saber qué parte vino de stdout.
- Es texto ya decodificado: los bytes no UTF-8 se pierden (U+FFFD).
- `command` no es la línea de órdenes ejecutada (prefijo y comillas distintos en la versión en la que se pudo comparar), así que el
  argv tampoco se recupera exactamente desde `command`.
- Las copias custodiadas están saneadas; para bytes exactos haría falta el registro original.

**Condiciones en las que el texto coincidiría con el stdout** (necesarias según los datos, no suficientes): que ni el programa
ni la shell escriban nada en stderr (con el pwsh del runtime eso exige `-NoProfile` o un perfil que cargue; en v5 las 42
órdenes empiezan con el error del perfil); que el stdout sea UTF-8 válido (lo más seguro, ASCII); que el tamaño esté dentro de
lo observado sin corte (≤ 59 805 bytes); y que los terminadores sean los que escribe el productor (lo observado: Codex no los
toca). Aun así, la ausencia de stderr no se puede demostrar desde el propio registro: tendría que garantizarla el programa o
verificarse con una suma de control que viaje en el propio stdout.

**Qué sigue sin saberse sin una ejecución real del instrumento:** el tamaño máximo que Codex entrega sin cortar (por encima de
59 805 bytes no hay datos); si stderr puede entrar en medio de una línea de stdout (solo se ve intercalado por líneas, y el
análisis no lo detectaría de forma fiable); si stdout y stderr comparten el mismo tubo (solo se ve `STARTF_USESTDHANDLES`);
los terminadores y los bytes del stdout de un programa nativo en un tubo nativo → nativo dentro del pwsh del runtime
(`git cat-file blob … | python -I - …`), porque ningún registro contiene ese tubo y la versión del pwsh del runtime no consta
(solo su SHA-256 `362a356c…`); y si la versión 26.1007.2314.0 sigue anteponiendo el prefijo de `[Console]::OutputEncoding`
(en ConstrainedLanguage ese prefijo falla en silencio dentro de su `try`).

## 4. Parte B: comprobaciones (i)-(viii) en el punto de uso sobre clones desechables del fixture

### 4.1 Qué hace `pou_check.py`

Entrada: `--repo` (el `-C` de la invocación), la declaración (`decl.json`: blob fijado; git, intérprete y shell con ruta,
versión exacta de `--version` y SHA-256; nombre con el que la invocación los llama) y la instantánea de configuración
registrada en la escritura de preparación. El entorno de lanzamiento es el del propio proceso. Las llamadas a git de la
supervisión usan el **git declarado por ruta completa**, sin ninguna variable `GIT_*` (así un `GIT_DIR` plantado no desvía las
propias comprobaciones). Ningún archivo del árbol de trabajo se ejecuta nunca.

| Paso | Cómo se establece | Falla cerrado cuando |
|---|---|---|
| (i) | `git -C T for-each-ref refs/replace/` vacío y salida 0 (lógica P2 de la ronda 2) | git no se puede ejecutar |
| (ii) | en el common dir (`rev-parse --git-common-dir`): no existen `info/grafts` ni `objects/info/alternates`; `rev-parse --is-shallow-repository` = `false` | `rev-parse` falla |
| (iii) | por la shell declarada (pwsh 7 del PATH), cwd = T y el entorno de lanzamiento sin tocar: `& '<git>' --no-replace-objects cat-file -t B` = `blob` y `& '<git>' --no-replace-objects cat-file blob B \| & '<git>' hash-object --no-filters --stdin` = B. Git va por ruta completa: nada se resuelve desde el cwd | falta la shell, el blob o git |
| (iv) | `git fsck --full --no-dangling --no-progress` sale con 0 y la instantánea de refs, estado, árbol y git dir es igual antes y después | excepción |
| (v) | para git y para el intérprete: la ruta existe; el SHA-256 de sus bytes es el declarado (si la ruta es un alias de ejecución de aplicación, se lee el destino con `FSCTL_GET_REPARSE_POINT` y se hashea el destino); `<ruta> --version` (fuera de T, sin `GIT_*`) da exactamente la versión declarada; y la shell declarada, con cwd = T y el entorno de lanzamiento, resuelve el nombre de la invocación (`git`, `python`) a esa misma ruta (`Get-Command -CommandType Application`, que no ejecuta nada) | falta un campo de la declaración, el alias no se puede leer, la ruta es un directorio u otro tipo de punto de reanálisis, o la orden de versión falla |
| (vi) | ningún nombre del entorno de lanzamiento empieza por `GIT_` (sin distinguir mayúsculas) | — |
| (vii) | `.git/config` (common dir) y `config.worktree` (git dir): misma existencia y mismo SHA-256 que la instantánea registrada; ni claves `include.*`/`includeif.*` (`git config --file … --no-includes --list -z`) ni secciones `[include`/`[includeIf` en el texto | falta la instantánea registrada o la configuración no se puede analizar |
| (viii) | en la raíz de T no hay ninguna entrada cuyo nombre sea `<raíz><ext>`, con raíz ∈ {nombre de la invocación, nombre de archivo declarado y nombre del destino del alias: `git`, `python`, `python3.13`} y ext ∈ PATHEXT del entorno ∪ PATHEXT por defecto de Windows ∪ {`.ps1`}, sin distinguir mayúsculas | excepción |

Todos los pasos se ejecutan siempre (no hay cortocircuito) para que el resultado diga todo lo que falla. `Launch` = todos
`PASS`. Un paso que no se puede establecer queda `UNESTABLISHED` y cuenta como fallido.

### 4.2 Preparación

- Instrumento: `git --no-replace-objects -C <wt> cat-file blob 9a829b0e…` (SHA-1 de blob recalculado = B, SHA-256 `b03c3a19…`,
  64 764 bytes, igual que en la ronda 2).
- Cada caso usa un clon nuevo: `git clone -q --no-hardlinks -c core.autocrlf=false -b fx/u1 D:/r62-fixture/fixture-origin.git
  D:\r62-a6r3\pou\cases\<caso>`, P1 como en la ronda 2 (`hash-object -w --no-filters --stdin` con los bytes del worktree; id = B
  y la única diferencia es el objeto suelto nuevo) e inmediatamente después la instantánea «registrada» de la configuración
  (`pou_check.py --snapshot-config`). La condición se planta después del registro. B1 repite la línea base sobre el clon de B0.
- Un clon auxiliar (`D:\r62-a6r3\pou\side\a1`, también con P1) sirve de destino benigno de `GIT_DIR` y de los alternates.
- Entorno de lanzamiento base: el del proceso del banco **sin** ninguna variable `GIT_*`.
- Los archivos plantados en (viii) son vacíos (`.exe`) o texto inofensivo que, si alguien lo ejecutara, escribiría un marcador en
  `D:\r62-a6r3\pou\markers\` (`.cmd`, `.bat`, `.ps1`). Después de cada caso se comprueba que no existe ningún marcador y que la
  instantánea completa de T (refs, HEAD, índice, estado, árbol y git dir) es igual antes y después de `pou_check.py`.

### 4.3 Casos

Evidencia: `part-b-pou.json` (entorno, declaración base, instantánea registrada de B0, P1 de cada clon, y por caso: lo
plantado, rc, `Launch`, pasos fallidos y sin establecer, el detalle del paso esperado, el SHA-256 del resultado crudo, la
comprobación de cero escrituras y los marcadores). En los 27 clones nuevos P1 se cumplió (id = B, solo el objeto suelto nuevo).
En los 28 casos, la instantánea de T es igual antes y después de `pou_check.py` y no se creó ningún marcador.

| Caso | Condición plantada | Esperado | Observado (rc; `Launch`; pasos fallidos) | Detalle del paso esperado | ¿Se cumple? |
|---|---|---|---|---|---|
| B0 | ninguna (clon nuevo, P1, configuración registrada, entorno sin `GIT_*`) | `Launch` true | 0; true; ninguno | (i)-(viii) PASS; (iii) `blob` y `9a829b0e…` | sí |
| v-a | SHA-256 declarado del intérprete erróneo (último dígito) | false en (v) | 1; false; (v) | Interpreter: `Sha256` FAIL (medido `ad169f4c…`); el resto PASS | sí |
| v-b | versión declarada del intérprete `Python 3.13.13` | false en (v) | 1; false; (v) | Interpreter: `Version` FAIL (medido `Python 3.13.14`) | sí |
| v-b2 (extra) | versión declarada de git `git version 2.53.0.windows.1` | false en (v) | 1; false; (v) | Git: `Version` FAIL | sí |
| v-c | SHA-256 declarado de git erróneo | false en (v) | 1; false; (v) | Git: `Sha256` FAIL (medido `81ef35ae…`) | sí |
| v-d | ruta declarada del intérprete inexistente (`D:\r62-a6r3\pou\absent\python.exe`) | false en (v) | 1; false; (v) | Interpreter: `Path` FAIL, `ResolvedByShell` FAIL (la shell resuelve el alias real), `Sha256` y `Version` sin establecer | sí |
| v-d2 (extra) | ruta declarada de git inexistente | false en (v) | 1; false; (i), (ii), (iii), (iv), (v), (vii) | Git: `Path` FAIL; (i)-(iv) y (vii) **sin establecer** porque usan el git declarado | sí (fallo cerrado; pasos extra) |
| v-e (extra) | git declarado = `mingw64\bin\git.exe` con su SHA-256 y versión correctos | false en (v) | 1; false; (v) | Git: `Sha256`, `Version` y `Path` PASS, pero `ResolvedByShell` FAIL: la shell resuelve `Git\cmd\git.exe` | sí |
| vi-0 (extra) | entorno heredado del banco sin depurar | false en (vi) | 1; false; (vi) | `GIT_ASKPASS`, `GIT_EDITOR`, `GIT_TERMINAL_PROMPT` | sí |
| vi-a | `GIT_DIR` = `.git` de un clon auxiliar que también tiene el objeto | false en (vi) | 1; false; (vi) | `GIT_DIR`; **(iii) pasa** con `GIT_DIR` apuntando a otro repositorio | sí |
| vi-b | `GIT_ALTERNATE_OBJECT_DIRECTORIES` = objetos del clon auxiliar | false en (vi) | 1; false; (vi) | `GIT_ALTERNATE_OBJECT_DIRECTORIES`; (ii) no lo ve (solo mira archivos) | sí |
| vi-c | `GIT_CONFIG_PARAMETERS='bench.planted'='1'` | false en (vi) | 1; false; (vi) | `GIT_CONFIG_PARAMETERS` | sí |
| vi-d | `GIT_REPLACE_REF_BASE=refs/bench-replace/` | false en (vi) | 1; false; (vi) | `GIT_REPLACE_REF_BASE` | sí |
| vii-a | `[include] path = <archivo benigno>` añadido a `.git/config` | false en (vii) | 1; false; (vii) | `LocalEqualsRegistered` FAIL y `NoIncludeOrIncludeIf` FAIL; añadido `include.path=…` | sí |
| vii-b | `[includeIf "gitdir:D:/r62-a6r3/"] path = …` | false en (vii) | 1; false; (vii) | las dos FAIL; añadido `includeif.gitdir:D:/r62-a6r3/.path=…` | sí |
| vii-c | `git config --local core.useReplaceRefs false` | false en (vii) | 1; false; (vii) | `LocalEqualsRegistered` FAIL; añadido `core.usereplacerefs=false` | sí |
| vii-d | `extensions.worktreeConfig=true` y `git config --worktree bench.planted 1` | false en (vii) | 1; false; (vii) | `LocalEqualsRegistered` FAIL y `WorktreeEqualsRegistered` FAIL (`config.worktree` no existía al registrar) | sí |
| vii-e (extra) | `config.worktree` escrito sin `extensions.worktreeConfig` (git lo ignora) | false en (vii) | 1; false; (vii) | solo `WorktreeEqualsRegistered` FAIL (falla cerrado aunque git no lo lea) | sí |
| viii-a | `git.exe` vacío en la raíz | false en (viii) | 1; false; (viii) | coincide `git.exe` | sí |
| viii-b | `git.cmd` (texto con marcador) | false en (viii) | 1; false; (viii) | coincide `git.cmd`; sin marcador | sí |
| viii-c | `git.bat` (texto con marcador) | false en (viii) | 1; false; (viii) | coincide `git.bat`; sin marcador | sí |
| viii-d | `python.exe` vacío | false en (viii) | 1; false; (viii) | coincide `python.exe` | sí |
| viii-e | `python3.13.exe` vacío (nombre exacto del intérprete: destino del alias) | false en (viii) | 1; false; (viii) | coincide `python3.13.exe` | sí |
| viii-f (extra) | `Git.PS1` (mayúsculas y `.ps1`; texto con marcador) | false en (viii) | 1; false; (viii) | coincide `Git.PS1`; sin marcador | sí |
| FC-a | ruta declarada de la shell inexistente | false en (iii) | 1; false; (iii), (v) | (iii) **sin establecer** («declared shell not found»); (v) `ResolvedByShell` sin establecer | sí (paso extra (v)) |
| FC-b | declaración del intérprete sin `Sha256` | false en (v) | 1; false; (v) | Interpreter: `Declaration` sin establecer («declaration lacks Sha256») | sí |
| FC-c | sin instantánea registrada de la configuración | false en (vii) | 1; false; (vii) | (vii) **sin establecer** («no registered config snapshot») | sí |
| B1 | repetición de B0 al final, sobre el mismo clon | `Launch` true | 0; true; ninguno | igual que B0; cero escrituras | sí |

Resumen: 28/28 casos se cumplen. Solo v-d2 y FC-a fallan además en otros pasos, y en los dos casos es el fallo cerrado
esperado: sin git declarado utilizable no se puede establecer ningún paso que use git, y sin shell declarada no se puede
resolver nada con ella. Cada comprobación tardó entre 2 y 12 s.

### 4.4 Lo que enseñan los casos

1. **(vi) es necesario además de (i)-(iv).** Con `GIT_DIR` hacia otro repositorio que tiene el objeto (vi-a), (iii) da `blob`
   y el id fijado: el rehash no dice de qué repositorio salieron los bytes (las comprobaciones (i), (ii), (iv) y (vii) de la
   supervisión pasan porque se ejecutan sin `GIT_*`, sobre T). Con `GIT_ALTERNATE_OBJECT_DIRECTORIES` (vi-b), (ii)
   no ve nada porque solo mira `objects/info/alternates`. Solo (vi) lo detecta.
2. **El propio host puede llevar `GIT_*`.** La sesión de Claude Code que ejecutó el banco hereda `GIT_ASKPASS`, `GIT_EDITOR` y
   `GIT_TERMINAL_PROMPT`. Una supervisión que lance desde un entorno así sin depurarlo falla siempre en (vi) (vi-0); hay que
   construir el entorno de lanzamiento quitando todo `GIT_*`.
3. **«Coincidir en ruta» exige mirar qué resuelve la shell.** v-e declara el binario real de git con su SHA-256 y su versión
   correctos y pasa esas dos pruebas; solo la resolución por la shell revela que la invocación ejecutaría otro archivo (el
   lanzador `Git\cmd\git.exe`).
4. **Un alias de ejecución de aplicación no se puede hashear leyendo la ruta.** El SHA-256 del intérprete de la ronda 2 solo se
   reproduce leyendo el destino del alias (`python3.13.exe`). Por eso el nombre exacto del intérprete para (viii) es
   `python3.13.exe`, distinto de `python.exe` (viii-e frente a viii-d).
5. **(vii) por SHA-256 del archivo** detecta cualquier cambio de la configuración local o de worktree, incluso uno que git
   ignora (vii-e). La comprobación separada de `include`/`includeIf` dice además por qué.
6. **Ningún archivo plantado se ejecutó**: la supervisión llama a git por ruta completa y la resolución es `Get-Command`, que no
   ejecuta. Los marcadores de `git.cmd`, `git.bat` y `Git.PS1` nunca aparecieron.

### 4.5 Comprobaciones que no se pueden establecer y cómo falla cerrado el script

- Falta un campo de la declaración (ruta, versión, SHA-256, nombre de invocación): ese ejecutable queda `UNESTABLISHED` → (v) falla (FC-b).
- La ruta declarada no existe: `Path` FAIL; SHA-256 y versión `UNESTABLISHED` (v-d). Si es la de git, (i)-(iv) y (vii) también
  quedan `UNESTABLISHED`, porque la supervisión no usa ningún otro git (v-d2).
- La ruta es un directorio, un punto de reanálisis de otro tipo, o un alias cuyo destino no se puede leer: `UNESTABLISHED` (sin caso propio).
- La orden `--version` falla o se agota (120 s): `Version` `UNESTABLISHED` (sin caso propio).
- La shell declarada no existe o `Get-Command` falla: (iii) y `ResolvedByShell` `UNESTABLISHED` (FC-a).
- Falta la instantánea registrada, o la configuración no se puede analizar con `git config --file`: (vii) `UNESTABLISHED` (FC-c).
- `PATHEXT` ausente o más corto que el de Windows: (viii) usa la unión con la lista por defecto, nunca una lista más estrecha.
- Cualquier excepción dentro de un paso lo deja `UNESTABLISHED`; una excepción fuera de los pasos sale con código 2 y sin
  veredicto. Solo rc 0 con un resultado legible y `Launch` true permite lanzar.

## 5. Lo que no queda demostrado

- **El pwsh del runtime de Codex.** (iii) y la resolución por la shell de (v) se hicieron con pwsh 7.6.6 del PATH
  (FullLanguage, SHA-256 `bfb46af8…`), no con el pwsh del runtime (`362a356c…`), que en v5 corre en **ConstrainedLanguage**.
  En ese modo, `[Console]::Out.Write(...)`, que usa la resolución de `pou_check.py`, es una llamada a un método que el modo
  bloquea; la comprobación tendría que reescribirse o fallaría cerrada. Tampoco se probó qué hace el tubo nativo → nativo de
  ese pwsh con los bytes del instrumento.
- **El entorno real de la invocación.** (vi) mira el entorno con el que la sesión lanzaría Codex, no el que Codex pasa a su shell
  ni a los procesos hijos. Si Codex añade, quita o reescribe variables (`GIT_*`, `PATH`, `PATHEXT`), este banco no lo ve. El
  propio host del banco (la sesión de Claude Code) hereda `GIT_ASKPASS`, `GIT_EDITOR` y `GIT_TERMINAL_PROMPT`: sin depurar, (vi)
  falla (caso vi-0).
- **El sandbox.** Codex crea los procesos con `CreateProcessAsUserW` (token restringido). En la versión 0.162.0-alpha.2 no pudo
  lanzar el alias `WindowsApps\pwsh.exe` (error 5, Parte A). El intérprete declarado aquí también es un alias de ejecución de
  aplicación (`WindowsApps\python.exe` → `python3.13.exe`). En v5 sí se lanzó `python` desde el pwsh del runtime (24 órdenes),
  pero no consta qué `python` resolvió ni con qué SHA-256. No se probó el lanzamiento real bajo el sandbox.
- **Lo que fija el SHA-256 declarado.** El git declarado es el lanzador `Git\cmd\git.exe` (46 480 bytes), que a su vez ejecuta
  `mingw64\bin\git.exe`; el SHA-256 no cubre ese binario real ni sus DLL (se registra aparte, informativo). En Python, el SHA-256
  cubre `python3.13.exe`, no sus DLL ni la biblioteca estándar.
- **TOCTOU.** La comprobación es una foto inmediatamente anterior al lanzamiento. Un escritor que actúe entre la comprobación y
  el lanzamiento no se ve; el banco es secuencial y no tiene un escritor concurrente sobre T.
- **Configuración global y del sistema.** (vii) cubre la local y la del worktree, como pide la regla. `%USERPROFILE%\.gitconfig`,
  el `etc\gitconfig` de Git para Windows y `GIT_CONFIG_GLOBAL`/`GIT_CONFIG_SYSTEM` también influyen en git (incluidos `include`
  y `core.useReplaceRefs`) y no se comprueban.
- **(viii) solo mira la raíz.** No se comprobó la carga de DLL desde el directorio actual, ni la búsqueda de CreateProcess desde el
  directorio actual del **padre** (la que importa cuando un proceso hijo lanza `git` por nombre). Tampoco se cambió
  `NoDefaultCurrentDirectoryInExePath` (heredada = 1; la ronda 2 ya midió su efecto con cmd.exe).
- **Ninguna invocación VERIFY real**, ni Controller ni Worker. Un solo host, NTFS, git 2.54.0.windows.1, pwsh 7.6.6 y Python 3.13.14.
- **La Parte A se basa en 9 registros saneados** de 4 binarios de Codex y 111 órdenes. La ausencia de truncado, de stderr dentro
  de una línea o de normalización de terminadores solo vale para lo observado.
- Durante la Parte B corría a la vez en el host otra sesión de banco (`a6r3-guards.py self-test` sobre el worktree de I-62). No
  escribe en `D:\r62-a6r3\pou\`, y las instantáneas de cada caso no muestran cambios ajenos, pero el banco no la controla.

## 6. Hallazgos inesperados

- El host del banco (la sesión de Claude Code) inyecta `GIT_ASKPASS` (vacía), `GIT_EDITOR=true` y `GIT_TERMINAL_PROMPT=0`.
  Sin depurar el entorno, (vi) bloquea siempre el lanzamiento (vi-0).
- En Codex 0.162.0-alpha.2, `command` no es la línea ejecutada: la real lleva el prefijo de `[Console]::OutputEncoding` y otras
  comillas. En todas las versiones, la ruta de la shell aparece en `command` con las barras invertidas duplicadas.
- Dos órdenes del pwsh del runtime sin `-NoProfile` no muestran el error del perfil (salida `fx/u1` + LF).
- `aux` es un nombre de dispositivo reservado: `git clone` a `D:\r62-a6r3\pou\aux\a1` falla con «Invalid argument». El banco usa
  `side\` (incidente del banco, sin efecto en los resultados; la primera ejecución abortó antes del primer caso).
- El `python.exe` del paquete no se ejecuta por su ruta y el alias no se lee como archivo: el SHA-256 del intérprete solo se
  establece por el destino del punto de reanálisis.
- Durante la Parte B corría en el host otra sesión de banco (`a6r3-guards.py self-test`), ajena a este paquete.

## 7. Archivos (SHA-256)

El SHA-256 de este README se da fuera de él. Los JSON llevan la hora de generación, así que una nueva ejecución cambia su
SHA-256. Worktree de I-62 leído en `HEAD` = `9cd074aa5668a59ef0030cb494f97ca2ca6333f5`. `part-b-pou.json` registra el SHA-256 de
`pou_check.py` con el que se ejecutaron los casos (`c3490925…`, igual al de abajo).

| SHA-256 | Archivo |
|---|---|
| `60e8362a62bb5d7fe7329893a7e96e96149147434a2e161e87a506b8bc097443` | `benchlib.py` |
| `c10d46e125e0dac7cc20fea3214e0b02bc5b178ac5f91def5804734656e48ca2` | `part-a-events.json` |
| `86dc98a94fc06d1c572b95fdb4bba2b8af5e0290ad3b3adc7b7a51c6a5ce9657` | `part-b-pou.json` |
| `1ce463252e4742d0d8eeb34c9c1f6e75630796313af31fb6b8f1ad41a0caf5d7` | `part_a_events.py` |
| `c3490925c63d24747952663470725487a6e45b90e2128e122910d3e49d81db7d` | `pou_check.py` |
| `29134abde79f459ee0aebaff423b6128b155cbcf353acba47da696ae25467e73` | `run_pou.py` |
| `56f83ed0eaafa90433fea868799e979b674fae776961434d691c9db6169f24e5` | `summarize.py` |
