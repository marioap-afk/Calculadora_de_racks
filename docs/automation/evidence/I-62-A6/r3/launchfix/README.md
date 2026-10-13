# I-62 — Corrección del lanzamiento del siguiente kit de revisión (tras la ronda 2 de A-6)

Alcance: corregir el procedimiento de lanzamiento del **siguiente** kit y dar una comprobación previa no destructiva de que `launch.py`
puede escribir `run.json` en el entorno en que se va a lanzar. No se ha ejecutado `claude.exe`, Codex ni ningún CLI de modelos; no se ha
tocado el kit congelado de la ronda 2 (solo se leyó y se copiaron cuatro archivos a directorios desechables, con SHA-256 comprobado antes y
después); no se ha escrito en ningún worktree de git; sin red; no se han leído `auth.json`, credenciales ni `config.toml`.

## 1. Qué falló en la ronda 2 (hechos)

- Revisión formal de A-6, ronda 2, RunId `R20261012T195511Z-e5c3`. La sesión principal lanzó el lanzador congelado como
  `python -X utf8 -B kit/launch.py` en lugar de la orden documentada `python -B kit/launch.py` (README del kit, pasos 4-5).
- `claude.exe` terminó (registro del lanzamiento: inicio 03:21:36Z, fin 03:48:35Z, `rc=1`). Después, `launch.py` L267 ejecutó
  `subprocess.run(["tasklist", "/FI", "PID eq %d" % p.pid, "/NH"], capture_output=True, text=True)`. En modo UTF-8, `text=True` decodificó
  como UTF-8 la salida de `tasklist`, que estaba en la página de la consola (cp850). El hilo lector lanzó
  `UnicodeDecodeError: 'utf-8' codec can't decode byte 0xe0 in position 9` (0xE0 es la «Ó» de «INFORMACIÓN:» en cp850), `q.stdout` quedó en
  `None`, la L268 (`str(p.pid) not in q.stdout`) lanzó `TypeError` y `launch.py` salió con 1 **sin escribir** `launch/run.json` (que se escribe
  en la L284). El auditor dio `NOT_ACCREDITED` por un solo motivo: «falta run.json».
- Medido ahora en este host (sección 5): el byte 0xE0 solo aparece cuando la página de códigos de la consola es la 850, que es la del entorno
  de la herramienta Bash, el mismo en que se ejecutaron estas pruebas. En el entorno de la herramienta PowerShell (consola en 65001),
  `tasklist` escribe UTF-8 válido y el mismo fragmento con `-X utf8` **no** falla (observación complementaria S-2). El fallo necesitó las dos
  cosas: el flag añadido y la consola en cp850.
- Con la orden documentada (`-B` solo), `text=True` decodifica con cp1252 y este mensaje concreto pasa (salen caracteres mal decodificados,
  pero sin error). Funciona por los bytes de este mensaje, no porque el código sea robusto: cp1252 no define 0x81, 0x8D, 0x8F, 0x90 ni 0x9D.
  En cp850 esos bytes son «ü», «ì», «Å», «É» y «Ø». En UTF-8 son el segundo byte de «Á», «Í», «Ï», «Ð» y «Ý». Un mensaje con alguno de esos
  caracteres también rompería el código de la ronda 2 sin flags añadidos. La corrección R elimina esa dependencia.

## 2. Procedimiento corregido de lanzamiento (siguiente kit)

La única orden admitida es la congelada, **sin ningún flag añadido** (es la constante `FROZEN_COMMAND` de la plantilla, la que comprueba la
guarda G). Se ejecuta desde la raíz del kit:

```text
python -B kit/launch.py
```

Pasos, en este orden. Todos desde la raíz del kit y en la **misma** herramienta o shell, con el mismo entorno, que se usará para lanzar: la
página de códigos de la consola decide los bytes de `tasklist`.

1. **Sonda de registro (obligatoria antes de lanzar).** Se espera código 0, `RecordProbe.Ok = true` y `FrozenCommandGuard.Ok = true`. Con
   cualquier otro código no se lanza: se informa de `RecordProbe.Problems` o de `FrozenCommandGuard.Problems`.

   ```text
   python -B kit/launch.py --record-probe
   ```

2. **Dry run.** El código es siempre 2. Se exige `FrozenCommandGuard.Ok = true`, `RecordProbeOk = true` y `BlockingChecks = []`, más las
   condiciones del kit (la comprobación 11 en Ok y `AuthorizationSections` = la disposición del Coordinator; 4 y 5 no se ejecutan).

   ```text
   python -B kit/launch.py --dry-run
   ```

3. **Lanzamiento, una sola vez, con la orden congelada del bloque de arriba.** El lanzador vuelve a evaluar G y la sonda P **antes** de la
   preflight. Si alguna falla, se niega con código 2 sin escribir nada en el run.

Reglas:
- Copiar la orden literalmente. Nada de `-X …`, `-I`, `-E` ni `-u`, ni otras opciones, y sin `PYTHONUTF8=1` en el entorno.
- Si la guarda G rechaza la orden, no se ejecuta nada y no se escribe nada: no se carga `transport_gate.py` ni `owner_gate.py`, no hay preflight
  y no se crea `<run>/launch` (casos iv-g e iv-h). Se corrige la orden y se repite. El rechazo no consume el lanzamiento único.

## 3. Qué hace cada corrección

`adapt_launch_r3.py` deriva `launch.r3-template.py` del `launch.py` de la ronda 2 mediante 12 sustituciones exactas. Antes comprueba que el
SHA-256 de entrada es `f2f58cec…` (el del manifiesto custodiado). Cada sustitución debe coincidir exactamente una vez; si no, se detiene sin
escribir nada. El diff tiene +339 y −28 líneas.

| Corrección | Sustituciones | Qué hace |
|---|---|---|
| **G** guarda de la orden congelada | `G1-imports-y-guarda`, `G2-preflight-json` | Se evalúa al importar, **antes** de cargar `transport_gate.py` y `owner_gate.py` y de cualquier preflight. Se niega (código 2, sin escribir nada) salvo que se cumpla todo esto: `sys.orig_argv[1:] = ["-B", <script>, *modo]`, con `<script>` normalizado (realpath + normcase de la ruta dada, que debe resolver a **este** archivo; se aceptan `kit/launch.py`, `kit\launch.py` y la ruta absoluta); el modo es ninguno, `--dry-run` o `--record-probe` (cualquier otro argumento, o dos modos, se rechaza); `sys.flags.dont_write_bytecode == 1`, `utf8_mode == 0` e `isolated == 0`; `sys._xoptions` vacío. Lo observado (argv sin el perfil, todos los `sys.flags`, las opciones `-X`, el intérprete, la versión y las codificaciones: preferida por locale, stdout y páginas de consola, OEM y ANSI) va a `preflight.json` como `FrozenCommandGuard`, fuera de `Checks`, y a la salida de `--dry-run` y `--record-probe`. `FROZEN_COMMAND = "python -B kit/launch.py"` es la misma cadena que este README. La regla de argv es necesaria porque en este host `PYTHONDONTWRITEBYTECODE=1` hace que `dont_write_bytecode` valga 1 aunque falte `-B` (caso iv-c). La regla de flags cubre `PYTHONUTF8=1` con la orden exacta (caso iv-d). |
| **R** registro posterior a prueba de fallos | `R1`…`R7` | `tasklist` se captura en **bytes**, sin decodificar, y el PID se busca en bytes con límites de dígito (`(?<![0-9])PID(?![0-9])`). La ronda 2 buscaba una subcadena en texto. Nuevo campo `TasklistQuery` (`ReturnCode`, `StdoutBytes`, `NonAsciiBytes`). Cada paso posterior (`ProcessGoneAfter`, `Stdout`, `ResultMessage`, `TransportFailure`, `Transcript`, `CloneAfter`) va aparte: su excepción queda en `PostRunErrors` (paso, clase y mensaje) y los demás siguen. Un `threading.excepthook` temporal también registra las excepciones de hilos, como la del lector de `subprocess` de la ronda 2. `run_and_record` escribe el registro **siempre** en un `finally` una vez entra en el lanzamiento, con los campos reunidos. Si el hijo no arranca, el registro lleva `LaunchError`. `write_record` usa JSON estricto y, si un valor no fuera serializable, lo anota y escribe con `repr`. `git()` pasa a decodificar con `encoding="utf-8", errors="replace"`, independiente del modo del intérprete, y admite `repo=` (por defecto, el clon). Los campos de `run.json` de la ronda 2 conservan nombre y forma (`base_record` es el diccionario de la ronda 2, movido). Cambia el código de salida: el lanzamiento real devuelve 0 solo si el hijo salió con 0, hay mensaje `result`, `PostRunErrors` está vacío y no hay `LaunchError`. |
| **P** `--record-probe` | `R7-…-P2-record_probe`, `P1-main-record-probe` | Ejerce el **mismo** código que escribe `run.json` (`base_record` → `run_and_record` → `spawn_and_wait` → `post_run` → `write_record`), sin preflight, con un hijo sustituto inocuo: este intérprete con `-B -I -c`, que lee stdin, imprime una línea JSON `type=result` y sale con 0. Trabaja en `tempfile.mkdtemp()` (`%TEMP%\r62-record-probe-*`) y comprueba que el temporal no está dentro del run, del clon ni del kit. Escribe `record-probe.json` (nunca `run.json` ni `<run>/launch`) y lo valida: los 27 campos de `RUN_JSON_FIELDS` presentes y serializables como JSON estricto, `PostRunErrors = []`, sin `LaunchError` ni `StdinError`, `tasklist` consultado y tratado, `ProcessGoneAfter = true`, salida 0, una línea `result` con el stdin entregado, transcripción sustituta con su SHA-256 y `CloneAfter` con sus dos campos. Después borra el temporal y comprueba que `<run>/launch` no apareció y que el listado del kit no cambió. No ejecuta `claude.exe` y no lee `~/.claude/projects`. Su `git` (paso `CloneAfter`) apunta a una ruta **inexistente** del temporal: en este host `%USERPROFILE%` es un repositorio git, y un directorio cualquiera de `%TEMP%` lo descubriría; con la ruta inexistente, `git -C` falla sin buscar hacia arriba. El alias de la Microsoft Store (`sys.executable`) no se puede abrir para leer (OSError 22), así que el SHA-256 del intérprete se toma del binario de `sys.base_prefix`. Código 0 si el registro funciona; 1 si no, con el motivo en `Problems`. |
| **D** dry-run y lanzamiento real | `D0-docstring-uso`, `D1-sonda-antes-de-la-preflight` | `--dry-run` ejecuta la sonda y su salida añade `FrozenCommandGuard`, `RecordProbe` y `RecordProbeOk`; las claves de la ronda 2 no cambian. El lanzamiento real ejecuta la sonda **antes** de la preflight y se niega (código 2) si falla. En el lanzamiento real, `preflight.json` lleva `RecordProbe`. La docstring documenta G, R, P, D y el nuevo uso. |

Compatibilidad, revisada sobre el `post-review.py` de la ronda 2:
- El auditor lee de `run.json` claves que no han cambiado. Las nuevas (`PostRunErrors`, `TasklistQuery`, `LaunchError`) no las mira.
- En `preflight.json`, el auditor exige que todas las entradas de `Checks` tengan `Ok = true`. Por eso G y P van **fuera** de `Checks`.
- La sonda nunca escribe en `<run>/launch`, cuyo conjunto de archivos el auditor restringe.
- Que el auditor del siguiente kit exija `PostRunErrors == []` y `FrozenCommandGuard.Ok` queda como propuesta: no está hecho.

La plantilla conserva a propósito las constantes de la ronda 2 (`CLONE`, `RUN`, `SESSION_ID`, el directorio del proyecto, el binario y los
topes), para que el diff muestre solo las correcciones. El adaptador del siguiente kit, como `prep/adapt_launch.py`, debe sustituirlas y
mantener la disposición `kit/launch.py` que supone `FROZEN_COMMAND`.

## 4. Archivos (en este directorio)

| Archivo | SHA-256 | Qué es |
|---|---|---|
| `adapt_launch_r3.py` | `3c2fa7cae1a0e7d5f61b5f206b784cb60836af3baf0b08435ba9ce1715100491` | Adaptador: 12 sustituciones exactas y únicas. Escribe la plantilla y el diff. |
| `launch.r3-template.py` | `4534c63be73d12c45bad2a1ce927b5a57bff06e3345a3e7b8d56ca4ad7d149f0` | Lanzador corregido (plantilla para el siguiente kit), 603 líneas. |
| `launch.r3-template.diff` | `b514f0d70f4d2e96fc624f7f8a76538dcc273fd33f1d4ae854bb25390789686e` | Unified diff frente al `launch.py` de la ronda 2 (+339 / −28). |
| `test_launchfix.py` | `88c0ba493f4bacde722b3650c365f582aa0e9676985201e006733310229caa5c` | Pruebas. Se ejecutan con `python -B test_launchfix.py`. |
| `test-results.json` | cambia en cada ejecución | Resultados saneados, con los hechos del host y el detalle de cada caso. |

Directorios de prueba desechables: `D:\r62-a6r3\launchfix\case-*` y `adapt-rerun`. Cada copia de prueba lleva `kit/launch.py` (la plantilla,
con las sustituciones de prueba que diga el caso) junto a `transport_gate.py`, `owner_gate.py` y `kit-params.json`, copiados del kit
congelado.

## 5. Resultados de las pruebas

Ejecución: `python -B test_launchfix.py` desde la herramienta Bash. Se cumplen **28 de 28** casos (`test-results.json`, `Summary`).

Hechos del host, medidos sin cambiar ninguna página de códigos:

| Hecho | Valor |
|---|---|
| Windows | Windows 11 Pro 10.0.26300 (`ver`: 10.0.26300.9550), es-ES |
| Consola (entorno Bash de las pruebas) | `chcp` → 850; `[Console]::OutputEncoding` (pwsh hijo) = 850 / ibm850; GetConsoleOutputCP = 850, GetConsoleCP = 850; OEMCP = 850; ACP = 1252 |
| Consola (entorno PowerShell, complementario) | `[Console]::OutputEncoding` = 65001 (utf-8); GetConsoleOutputCP = 65001 |
| Python | 3.13.14 (Microsoft Store, paquete 3.13.3824.0); `python` del PATH = alias `%USERPROFILE%\AppData\Local\Microsoft\WindowsApps\python.exe` (no legible, OSError 22); binario de `sys.base_prefix\python.exe` con SHA-256 `ad169f4cb4bfb78c7a5c030a4529c19d6643276778e33994c93e145b6191c3ec` |
| Entorno Python | `PYTHONDONTWRITEBYTECODE=1` (por eso `dont_write_bytecode = 1` incluso sin `-B`); `PYTHONUTF8` no definido; locale preferido cp1252 |
| `tasklist /FI "PID eq 99999999" /NH` (entorno Bash) | primeros bytes `494e464f524d414349e04e3a…` = «INFORMACI\xE0N:», no es UTF-8 válido (cp850) |

| Caso | Orden | Esperado | Observado | ¿Se cumple? |
|---|---|---|---|---|
| i-a | `python -B kit/launch.py --record-probe` | código 0, sonda y guarda Ok, sin escrituras | código 0; `RecordProbe.Ok` y guarda Ok; `PostRunErrors = []`; `TasklistQuery` = rc 0, 89 bytes, no ASCII; temporal borrado; copia sin cambios | sí |
| i-b | `python -B launch.r3-template.py --record-probe` (desde `kit/`) | ídem | ídem | sí |
| i-c | `python -B kit\launch.py --record-probe` | ídem | ídem | sí |
| i-d | `python -B D:\…\case-i\kit\launch.py --record-probe` | ídem | ídem | sí |
| ii-a | `python -X utf8 -B kit/launch.py --record-probe` (la orden de la ronda 2) | rechazo de la guarda, código 2, nada escrito | código 2; problemas: opciones `['-X']` ≠ `['-B']`, `utf8_mode = 1`, opción `-X utf8`; sin sonda; copia sin cambios; sin `__pycache__` ni temporal | sí |
| ii-b | ídem, copia con la guarda desactivada | registro escrito sin error (bytes) | código 0; guarda informada (Ok = false, Enforced = false); `RecordProbe.Ok = true`; `PostRunErrors = []`; `tasklist` con bytes no ASCII tratado | sí |
| ii-c | ídem, guarda desactivada + L267-268 originales inyectadas | se escribe el registro y se captura la clase, sin abortar | código 1 sin traza fatal; registro escrito; `PostRunErrors`: `UnicodeDecodeError` (hilo `_readerthread`, «byte 0xe0 in position 9») y `TypeError` (paso `ProcessGoneAfter`); solo faltan `TasklistQuery` y `ProcessGoneAfter`; temporal borrado | sí |
| ii-d | `python -B kit/launch.py --record-probe` (copia de ii-c) | sin error con `-B` solo | código 0; `PostRunErrors = []` | sí |
| ii-e | `python -B kit/launch.py --record-probe` (copia: hijo inexistente) | registro escrito con `LaunchError` | código 1; registro escrito; `LaunchError.Class = FileNotFoundError`; temporal borrado | sí |
| ii-f | `python -B kit/launch.py --record-probe` (copia: tope 2 s, hijo que duerme) | terminate y registro completo | código 1; registro con los 27 campos y `TerminateAt`; `PostRunErrors = []`; «terminado por el invocador»; `ProcessGoneAfter = true` | sí |
| iii-a | `python -X utf8 -B snippet_round2.py 99999999` (L267-268 originales) | `UnicodeDecodeError` → `q.stdout` None → `TypeError` | hilo: `UnicodeDecodeError` «'utf-8' codec can't decode byte 0xe0 in position 9»; `q.stdout is None`; `TypeError` «argument of type 'NoneType' is not iterable» | sí |
| iii-b | `python -B snippet_round2.py 99999999` | funciona | sin errores; `q.stdout` es str; `ProcessGoneAfter = true` | sí |
| iv-a | `python -B -X utf8 kit/launch.py --record-probe` | rechazo (código 2), nada escrito | código 2; `utf8_mode = 1`, opción `-X utf8`, argv distinto | sí |
| iv-b | `python -I -B kit/launch.py --record-probe` | ídem | código 2; `isolated = 1`, opciones `['-I']` | sí |
| iv-c | `python kit/launch.py --record-probe` (sin `-B`) | ídem | código 2; lo detecta la regla de argv (`dont_write_bytecode` = 1 por `PYTHONDONTWRITEBYTECODE`) | sí |
| iv-d | `PYTHONUTF8=1 python -B kit/launch.py --record-probe` | ídem | código 2; solo la regla de flags: `utf8_mode = 1` | sí |
| iv-e | `python -B kit/launch.py --record-probe --dry-run` | ídem | código 2; argumentos no admitidos | sí |
| iv-f | `python -B kit/launch.py --bogus` | ídem | código 2; argumentos no admitidos | sí |
| iv-g | `python -X utf8 -B kit/launch.py --record-probe` (`transport_gate.py` con marcador al importarse) | rechazo sin cargar `transport_gate.py` | código 2; sin marcador | sí |
| iv-h | `python -B kit/launch.py --record-probe` (misma copia, control) | con la orden congelada sí se carga, después de G | código 0; marcador presente | sí |
| v | `python -B kit/launch.py --dry-run` (preflight sustituida, run y clon desechables) | informa de G y P; código 2 | código 2; `FrozenCommandGuard.Ok`, `RecordProbe.Ok` y `RecordProbeOk` = true; `<run>/launch` no creado; copia sin cambios | sí |
| vi-a | `python -X utf8 -B kit/launch.py` (guarda desactivada, fallo inyectado, preflight sustituida, binario inexistente) | la sonda falla y se niega antes de la preflight | código 2; «la sonda de registro falló»; `PostRunErrors`: `UnicodeDecodeError` y `TypeError`; sin `Checks`; `<run>/launch` no creado | sí |
| vi-b | `python -B kit/launch.py` (misma copia) | la sonda pasa y se llega a la preflight | código 2; «preflight fallida» (la preflight sustituida); `<run>/launch` no creado | sí |
| vii-a | `python -B adapt_launch_r3.py <launch.py r2> <dir>` | mismos bytes, 12 sustituciones exactas | código 0; 12 sustituciones, todas con 1 coincidencia; plantilla y diff idénticos | sí |
| vii-b | SHA-256 del kit congelado y listado de `D:\r62-arch-a6r2-run\launch` | intactos | SHA-256 = manifiesto antes y después; listado idéntico (la sonda lo vio existir y no lo tocó) | sí |
| vii-c | `FROZEN_COMMAND` frente a los README | misma orden | `python -B kit/launch.py`; literal en el README de la ronda 2; bloque exacto en este README | sí |
| vii-d | restos | sin `__pycache__` y sin temporales de la sonda | 0 y 0 | sí |
| vii-e | saneamiento de este directorio | sin perfil, cuenta, forma 8.3 ni correos | ninguna fuga | sí |

Observaciones complementarias, hechas a mano desde la herramienta PowerShell (consola en 65001). No forman parte de `test_launchfix.py`
porque reproducirlas exigiría cambiar la página de códigos:

| Caso | Orden | Observado |
|---|---|---|
| S-1 | `tasklist /FI "PID eq 99999999" /NH` capturado en bytes | `INFORMACI\xC3\x93N…`: UTF-8 válido |
| S-2 | `python -X utf8 -B snippet_round2.py 99999999` | sin error (`q.stdout` es str; `ProcessGoneAfter = true`): con consola 65001, el fallo de la ronda 2 no se reproduce |
| S-3 | `python -B kit/launch.py --record-probe` (case-i) | código 0; `RecordProbe.Ok = true`; `TasklistQuery` = rc 0, 91 bytes, no ASCII; `ConsoleOutputCP = 65001` |

## 6. Lo que no queda demostrado

- **El hijo sustituto no es `claude.exe`.** La sonda demuestra que el código común de lanzamiento y registro escribe un registro completo en
  este entorno con un proceso de Python de una línea. No demuestra nada de la ejecución real: ni el tamaño de stdout (en la ronda 2,
  `stdout.jsonl` tuvo 1 976 462 bytes), ni la duración (unos 27 minutos), ni el tamaño y el hash de la transcripción, ni el contenido
  `stream-json` ni el `result` real, ni el refresco OAuth.
- **El final completo del lanzamiento real no se ha ejecutado:** la preflight real con el binario real, `--version`, `auth status`, la espera de
  120 s y `run.json` en `<run>/launch` con `Executable` = `claude.exe`. En las pruebas v y vi, la preflight está sustituida y el binario es
  inexistente. El dry-run con la preflight real (integridad del kit con el nuevo `launch.py` en el manifiesto, clon, compuertas) queda para el
  kit siguiente.
- **`CloneAfter` en la sonda** ejerce el subprocess de `git` y la decodificación contra una ruta inexistente, no los valores. El `git status`
  del clon real sigue como en la ronda 2: puede refrescar el índice del clon, igual que antes.
- **Rutas no ejercidas:** el `kill` posterior a un `terminate` que no termina (`KillAt`), la interrupción con Ctrl+C (`KeyboardInterrupt`:
  el `finally` escribe el registro y la excepción se propaga) y un fallo al escribir el propio archivo de registro (disco o permisos).
- **Equivalencia de entorno.** La sonda solo prueba el entorno en que corre. Si se lanza desde otra herramienta u otra página de códigos, la
  comprobación previa hay que repetirla allí. El lanzamiento real la repite por dentro, en el mismo proceso, justo antes de la preflight, y
  eso cubre el entorno del lanzamiento.
- **Límites de la guarda.** No verifica la identidad del intérprete (`python` del PATH): solo lo registra. Rechaza por argv, por tres flags
  y por las opciones `-X`. Otros modos inducidos por el entorno (`PYTHONDEVMODE`, `PYTHONWARNINGS`, `PYTHONIOENCODING`…) no se rechazan, solo
  quedan en `Flags` o fuera.
- **Riesgo residual fuera de esta orden.** `transport_gate.cli_version` y `transport_gate.auth_status` (preflight 4 y 5) usan `text=True` sin
  codificación. Con la orden congelada decodifican en cp1252, y una salida de `claude.exe` con bytes que cp1252 no define (por ejemplo, el
  UTF-8 de «Á» o «Í» en un nombre de organización) lanzaría el mismo tipo de error en el hilo lector. Ocurriría **antes** de escribir nada
  (en la preflight), así que no se perdería `run.json`, pero el lanzamiento abortaría. No se ha corregido aquí, porque `transport_gate.py`
  es otro archivo del kit, fuera de esta orden.
- **Solo Windows**: `tasklist` y las páginas de códigos vía `ctypes`.
