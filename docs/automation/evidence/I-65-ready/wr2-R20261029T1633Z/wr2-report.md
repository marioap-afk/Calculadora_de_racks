# I-65 W-R2: RAM Elements sobre `4a03b65b` (OV-I65-01, 02 y 04..07)

- **Corrida:** la de W-R1, `<corrida>` = `D:\r65-smoke\4a03b65b\wr1-R20261029T1633Z\`, el 2026-10-30.
- **Disposición:** «W-R1b Global Acceptance / W-R2 RAM Authorization» ([original](../../../decisions/I-65-owner-disposition-wr1b-acceptance-wr2-authorization.original.txt); decisiones §122).
- **Ventana de RAM:** de las 22:59:40Z (WR2-01) a las 23:16:28Z (WR2-08), unos 17 minutos de las 2.5 h de reserva.
- **Operación:** una única `RAMElements.exe` (pid 7584), manejada con Computer Use, y nueve scripts ejecutados una vez cada uno. Ningún modelo guardado y ningún análisis.

**Resultado:** las seis OV de W-R2 dan su oráculo. **Propuesta: W-R2 = PASS.** La matriz OV-I65-01..08 está en el punto 5.

## 1. Preflight (Owner §4)

| Punto | Resultado |
|---|---|
| 1. AutoCAD y RAM cerrados | `acad` = 0 y `ram` = 0 (también `accoreconsole`) |
| 2. Nadie necesita la pantalla | Inactividad de 693.3 s, con la app de Claude en primer plano, a las 22:59:33Z |
| 3. I-62 | Respuesta literal: «sin operación de pantalla» (solo shell, git y pruebas). I-64 e I-52 están inactivas, así que no se les pidió nada (Owner §4). Aviso de cierre enviado a I-62. |
| 4. RAM, API y bases | `RAMElements.exe` 26.00.01.15 `7dcb4855…`; `ramelements` 0.1.0, árbol `f71a017f…` (30 archivos); `W.sec` `dffd9e00…`, `HSS_SQR.sec` `992ddac9…`, `Steel.mat` `2a8b4239…`. Árboles: con el método de Python, `ccdc2346…`/`4cb742a5…`, iguales a §1.2; con `w2_hostsnap`, `e70bfff0…`/`b8e46d28…`, iguales a W2 y W4 (CD-I65-FC-69 (a)). |
| 5. Paquetes, inyecciones y scripts | Los 23 originales iguales a la lista custodiada de W-R1a; la herramienta `ramtool` (11 archivos), con manifiesto `4fbe3ea5…`; las seis herramientas del Coordinator, iguales a §1.3 |
| 6. Destinos | Ningún `ram-readback.json` ni `ram-dump.json` en las nueve carpetas |
| 7. `Config.ini` | Respaldado en `w2-pre\Config.ini.before` (`e51d5ae3…`) |
| 8. R-D2 | Volcados manuales `c4a98fb0…` y `12c89479…`. Scripts de volcado iguales a la plantilla de P2 salvo `dumpFolder` (1 línea). Especificaciones de REF-CANT-01 (`45cde765`, `2cec9656`, `3855c268`, hoja `a720c0d1`) y de OV-02 (hoja `6dc0d8da`, contenido `b15b9f9e…`; cargas `dcc1a039`/`d675d9f6…`), iguales en `4a03b65b`, `HEAD` y su origen |
| 9. Custodia (R-D3, R-D5) | `wr2-session.jsonl`: `session-start`, nueve `pre-run`/`post-run` y `session-end`. Un solo proceso de RAM; el contraste se hizo después del cierre. |
| 10. Checkouts | K1, con digest `d0105e96…`. Worktree (`3f28120a`) y checkout principal limpios; `git diff 4a03b65b..HEAD -- src deploy assets` vacío. |

## 2. Ejecuciones en RAM

En las nueve ejecuciones, la consola mostró `AnalysisInputFingerprint` igual al manifiesto y el SHA-256 del script se midió de nuevo justo antes de «Abrir». Cada caso usó un modelo nuevo y vacío y se descartó al terminar con «Nuevo» → «No».

| OV | Script | Consola | Lectura de vuelta o volcado | Modelo |
|---|---|---|---|---|
| OV-01 | `pkg-OV01` `e2d0b8cd` | `READBACK OK`, sin líneas ` - `, huella **H0** `73635936…d13ea` | `ram-readback.json` `ff9d770a`, `differences` vacío | «Elementos: 15» |
| OV-01 (volcado) | `dump-OV01-gen` `22400e22`, en el mismo modelo | `DUMP OK` | `ram-dump.json` `f5789d1c`: 8 nudos, 7 miembros, 2 casos, **0 combinaciones** | descartado |
| OV-02 | `pkg-OV02` `9a3d536d` | `READBACK OK`, huella `93849b42…7a72` (la del manifiesto) | `455a2b40`, `differences` vacío | «Elementos: 15» |
| OV-02 (volcado) | `dump-OV02-gen` `61925fce`, en el mismo modelo | `DUMP OK` | `e5cf0ead`: 8 nudos, 7 miembros, 2 casos, **0 combinaciones** | descartado |
| OV-04 | `pkg-OV04` `05ebc7fe`, sobre un modelo con el nudo 1 en (0, 0, 0) sin apoyos | `FAILED — DISCARD MODEL`, ` - gate: the model has 1 nodes` | `73af980d`, `values` vacíos | sin cambios: 1 nudo, «Elementos: 1», estado CM, sin REF1 |
| OV-05 | `inj-OV05` `4d0ef35a` | `FAILED — DISCARD MODEL`, ` - exception: ValueError: Section not found: "HSS_SQR INEXISTENTE-99"…` | `9ab45229`, `values` vacíos | parcial: «Elementos: 9» (8 nudos y 1 miembro), igual que en W2 |
| OV-06 | `inj-OV06` `897dbcd6` | `FAILED — DISCARD MODEL`, ` - member ARM.1.PositiveY: local axis 2 (0.0, 0.99933…, 0.03643…)` | `15ff2655` | «Elementos: 15» |
| OV-07a | `inj-OV07a` `3ff69cd4` | `FAILED — DISCARD MODEL`, ` - gate: package version is '0.1.0', expected '999.0.0'` | `0f397cd8`, `values` vacíos | vacío y sin «Modificado» |
| OV-07b | `inj-OV07b` `1bcfcbe3` | `FAILED — DISCARD MODEL`, ` - gate: core function missing: rackcad_nonexistent_function` | `10e549bb`, `values` vacíos | vacío y sin «Modificado» |

Los volcados de los modelos generados son byte a byte iguales a los de W2 (`f5789d1c`) y W4 (`e5cf0ead`). Las capturas `01`..`37` muestran la consola, la barra de estado, las rejillas, los diálogos y los descartes.

## 3. Después de cerrar RAM

- **Contraste** (`readback_dump_contrast.py`, R-D5 (b)): OV-01 **IGUAL** y OV-02 **IGUAL**, ambos con 0 hallazgos.
- **Comparador** (`ramtool` del Candidato, sin `--*-unread`, CMB-3):

  | Caso | Volcado manual | Resultado | Detalle |
  |---|---|---|---|
  | OV-01 | `c4a98fb0…` | **PASS (0)**, con `inputs.manualDumpSha256` = `c4a98fb0…` | |
  | OV-02 | `12c89479…` | **PASS (0)** | `LOADS` PASS, con 0 % de diferencia y 0 in de posición. **Q2 reproducida**: miembro 1, a 99.99999999999997 %, valor −337.2134…, igual que en W4 (§4.3). |
  | Control de OV-05 (`inj-OV05`) | `c4a98fb0…` | **REJECTED (2)** | «The package is a fault injection (unknown-section)…» |

  En los dos PASS:
  - **en PASS:** coordenadas, conectividad, ejes locales, sección, material, valores por omisión, liberaciones y apoyos, cargas, `COMBINATIONS` (0 en el generado y en el manual) y `CASE-CATEGORY`;
  - **NOT_EVALUATED:** propiedades de sección y de material (hoja D-09);
  - **INFORMATIVE:** categoría del caso manual (DL) y peso propio (0, 0, 0). El peso propio queda excluido.
- **Cadena de custodia** (WR2-13): **COHERENTE**.
  - Un único PID (7584) y los nueve `pre-run`/`post-run` emparejados, con la salida inexistente antes de cada ejecución.
  - OV-01 y OV-02: inicio 22:59:40Z ≤ lectura de vuelta ≤ volcado ≤ cierre 23:16:42Z.
  - Es evidencia operativa, no criptográfica (R-D3).

## 4. POST y estado del host

- **`w2_hostsnap -Phase post` frente al PRE:** solo cambian S-06 (`Config.ini` `e51d5ae3…` → `de651e69…`) y S-07 (un archivo nuevo por carpeta, por diseño, A65-FC-13). RAM, la API, las bases y el volcado de P2 siguen iguales. Los árboles por el método de Python también.
- **Carpetas y volcados:** las nueve carpetas conservan sus tres originales (iguales a la lista custodiada) y tienen solo el archivo nuevo previsto. Los volcados manuales siguen intactos.
- **`Config.ini`:** RAM lo reescribió al cerrar, con tres claves de estado de interfaz: `[Actions] accPythonConsole`, `[Actions] accQANew` y `[DefaultFolders] PythonScripts`. Se conserva (CD-I65-F4-73).
- **Checkouts:** K2 = K1 (`d0105e96…`). K1 → K2: solo un SIGNAL, el worktree de I-62, que avanzó limpio e igual a su remoto; `stop=false`.
- **AutoCAD, `ApplicationPlugins` y registro:** no se tocaron. Los residuos históricos del registro (W-R1b, CD-I65-FC-67) siguen documentados, no se eliminaron y el equipo no se declara idéntico a la BASELINE.

## 5. Matriz final OV-I65-01..08

| OV | Ventanas | Resultado | Evidencia |
|---|---|---|---|
| OV-I65-01 | W-R1a A + W-R2 | **PASS** | `Valid` H0 16/16 y 117/117 (§118); `READBACK OK` H0, `DUMP OK`, contraste IGUAL y comparador PASS (0) con `c4a98fb0` |
| OV-I65-02 | W-R1a A + W-R2 | **PASS** | `Valid` 16/16 y 6/6 (§118); `READBACK OK`, `DUMP OK`, IGUAL y comparador PASS (0) con `LOADS` y `12c89479`; Q2 reproducida |
| OV-I65-03 | W-R1a A | **PASS** | Casos (a)-(e) con su código y sin carpeta (§118, aceptada en §119) |
| OV-I65-04 | W-R1a A + W-R2 | **PASS** | `Valid` = H0 (§118); rechazo `gate` con el modelo intacto |
| OV-I65-05 | W-R1a D + W-R2 | **PASS** | `FAILED` con la excepción de la sección; parcial documentado; control REJECTED (2) |
| OV-I65-06 | W-R1a D + W-R2 | **PASS** | `FAILED` por el eje local 2 |
| OV-I65-07 | W-R1a D + W-R2 | **PASS** (07a y 07b) | Rechazo antes de escribir; modelo vacío |
| OV-I65-08 | W-R1a B | **PASS** | O8-01..13 y H2 = H1 (§118, aceptada en §119) |
| HB-01..05 (complemento) | W-R1b | **PASS** | Aceptado en §122, con la recuperación del registro de §121 |

**NOT_EVALUATED (separado de los PASS):**
- el punto de legado de OV-08 (WR1-13; no hubo DWG Cantilever anterior representativo);
- las filas de propiedades de sección y de material de OV-01 y OV-02: la API de RAM no las expone y se evalúan con la hoja firmada del ingeniero (D-09).

**INFORMATIVE, no son PASS:**
- DIF-01, el peso unitario de RAM frente a AISC (CD-I65-F4-47);
- la categoría del caso manual;
- el peso propio.

## 6. Incidencias (ninguna alteró un paquete, un oráculo ni el host)

1. **Monitor equivocado.** Entre las 23:00Z y las 23:03Z capturé el monitor equivocado («display 1798751497», solo escritorio). Por eso RAM parecía invisible: el registro de licencia era normal (Practitioner) y no hice ninguna acción sobre RAM. Lo resolví cambiando al monitor «BenQ GW2780».
2. **Preflight v1 incompleto.** No hizo las seis comprobaciones de herramientas, porque la variable `$tools` pisó al parámetro `[string]$Tools`. Es el mismo modo de fallo que el `$l`/`$L` de W-R1b. Lo repetí como `wr2-preflight-2`, que salió conforme. La v1 y su script se conservan.
3. **Tres clics fuera del botón ↑.** En el primer «Abrir», tres clics cayeron en el desplegable de ubicaciones recientes, sin efecto. Después usé el botón ↑ y el doble clic en cada carpeta.
4. **Nudo de prueba de OV-04.** Las dos primeras veces, «0» + Intro no creó la fila, porque 0 es el valor por omisión. Con doble clic en la celda X + Intro se creó el nudo 1 en (0, 0, 0). Es preparación del estado inicial, no un intento, como en W2.
5. **Hook de la sesión.** Bloqueó un comando de PowerShell por un patrón de expresión regular, así que el `pre-run` de OV-04 no llegó a ejecutarse en ese momento. Lo repetí antes de «Abrir». Ningún script se ejecutó sin su `pre-run`.
6. **Acciones de interfaz.** Usé «Limpiar» en la consola antes de cada caso y abrí la pestaña «Miembros» en OV-05 solo para documentar, como en W2.
7. **Lo que no se usó.** Sin `SetForegroundWindow`, sin portapapeles, sin segunda instancia y sin `open_application` con RAM abierto.
8. **Capturas.** Las del diálogo «Abrir» van recortadas y con el panel de navegación tapado (nombres de personas, de la organización y carpetas ajenas). Después del cierre no hice capturas, porque el escritorio muestra archivos personales.

**Consumo:** Codex 55/57 y `attempts` 7/11, sin consumo. Nueve ejecuciones del procedimiento, sin repeticiones.

## 7. Recomendación

**W-R2 = PASS:** se cumplen WR2-AC-01..05. Con W-R1a = PASS y W-R1b = PASS, la matriz OV-I65-01..08 queda completa en PASS, con los NOT_EVALUATED declarados arriba.

**Siguiente paso del Owner:** la **validación final** (ficha de la guía §7; el Coordinator no le atribuye firma). INTEGRATION = NO hasta una orden aparte. Me detengo.
