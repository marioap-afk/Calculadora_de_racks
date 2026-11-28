# I-64 Smoke-1 — Guarda de procesos revisada para la ocurrencia nueva (PROPUESTA, revisión 2)

- **Origen:** «I-64 — COORDINATOR SMOKE-1 REVIEW» de `R20261104T194513Z-a578`: STOP CONFIRMED, HOST_ISOLATION_VIOLATION /
  MULTIPLE_AUTOCAD_INSTANCES.
- **Revisión 2 (2026-11-05 ~15:50Z):** adaptada a «I-64 — COORDINATOR OPERATOR DISPOSITION». Hay OPERATOR_EXCEPTION = AUTHORIZED para
  UNA ocurrencia, y el operador de AutoCAD es la sesión de implementación (`../coordinator-operator-disposition.md`).
  - La revisión 1 decía que la sesión no usaba computer use. Esa regla se sustituye por las reglas de operación de la sección 4.
- **Estado:** propuesta. Entra en el kit de la ocurrencia (`<run>/kit/`) solo con la autorización del Coordinator.
- No cambia T3D: T3D §1 queda igual en la historia.

## 1. Qué falló en a578 y qué cierra cada medida

| Fallo en a578 | Medida |
|---|---|
| Con AutoCAD abierto, el primer clic falló y la sesión llamó a `open_application`, que lanzó una segunda `acad.exe` (22:20:52Z) | `open_application` se usa **una sola vez**: para abrir AutoCAD con acad = 0 justo antes (PG-1a). Después, **nunca**. Ante un clic fallido o un cambio de foco: captura, guarda `Check` y foco con un clic en la ventana de AutoCAD (sección 4) |
| No se contaron procesos de 22:18:34Z a 22:34:52Z (16 min) | Vigilancia **continua** cada 1 s desde PG-1 hasta después de la POST, más los puntos de control |
| La inspección y los conteos iban por nombre de proceso | Toda comprobación sensible a procesos se liga a **AUTHORIZED_AUTOCAD_PID**: PID, ticks UTC de inicio, ruta y SHA-256 del ejecutable |
| No se pudo atribuir la POST, el RackLog ni el registro | Esa evidencia solo cuenta si la bitácora de vigilancia demuestra una sola instancia, la autorizada, en toda la ventana, y si la POST se toma con `closed.ok` |

## 2. Herramientas (borradores de solo lectura, en `process-guard/`)

| Archivo | SHA-256 | Autoprueba sin AutoCAD |
|---|---|---|
| `Test-AcadProcessGuard.ps1` (modos `Register`, `Check`, `Closed` y `Watch`) | `736463BD3EF607A5C61530ACCBE385E9C799BD9A1FDF72107CEF1B76CB76E3A4` | `selftest-result.txt`: **20/20 PASS** |
| `Inspect-LoadedRackCad.ps1` (SM-00d1 ligada a la PID) | `86985EC90F4591806391F70257DE2B2CCBD0CFC096EB34B33117DF63E413E5AB` | `selftest-inspect-result.txt`: **4/4 PASS**, caminos de STOP de la ligadura |

- Las autopruebas usan un proceso sustituto, `i64probe.exe`, copia de `PING.EXE`. Lo lanzan y lo detienen ellas mismas por PID; ningún
  `acad.exe`.
- **Cubierto:**
  - registro con 0 procesos, con un SHA distinto y repetido;
  - punto de control con una instancia, con dos y con la PID desaparecida;
  - vigilancia que detecta otra PID;
  - salida sin PRE-CLOSE;
  - cierre normal con PRE-CLOSE y `Closed`;
  - instancia nueva tras el cierre;
  - fin con `watch-stop.flag`;
  - inspección sin registro, sin `RackCad.Plugin.dll` en la PID, con dos procesos y con otra instancia del mismo nombre.
- **No cubierto sin AutoCAD:** el camino PASS de la inspección de módulos. Ese código es el del kit de a578, que dio PASS en el host en
  SM-00d1 (22:18:34Z).
- **Archivos en `<run>/process/`:**
  - `authorized-autocad-pid.json` y su `.sha256`;
  - `guard-log.txt`;
  - `acad-watch.log`;
  - `pre-close.ok`, `closed.ok` y `watch-stop.flag`;
  - `inspect-after-netload.txt`.
- **Errores que encontró la autoprueba**, corregidos antes de proponer:
  - PowerShell desenrolla un arreglo vacío o de un elemento devuelto por una función; se cuenta con `@(...)`.
  - `ConvertFrom-Json` convierte la fecha ISO en `DateTime`; la identidad se compara por ticks UTC.

## 3. Secuencia

Opera la sesión. Los scripts de preflight/BASELINE y de POST también los ejecuta la sesión: el Owner eligió «Ejecútalos tú». Es un
punto que autoriza el Coordinator (paquete, sección 7).

| PG | Momento | Comprobación | Si falla |
|---|---|---|---|
| PG-0 | Preflight y BASELINE (pasos 1, 6, 6b y 7 del preflight) | acad = 0 y RAMElements = 0 | no se toma la BASELINE |
| PG-0b | Antes de la BASELINE | aviso a las sesiones vivas de la máquina (hoy «I - 52» e «I - 62»): ninguna lanza `acad.exe` ni toca el host hasta la POST y su recomprobación | — |
| PG-1a | Justo antes de abrir AutoCAD | acad = 0 otra vez; después, **una** llamada a `open_application("AutoCAD 2025 - English")`, la única de la ocurrencia | si acad ≠ 0, no se abre |
| PG-1 | En cuanto AutoCAD arranca, antes de SM-00b | `Register`: exactamente 1 `acad.exe`; registra AUTHORIZED_AUTOCAD_PID con PID, ruta, SHA-256 (`2A75996F…6422`) e inicio | STOP (SM-STOP-02); cerrar sin guardar |
| PG-1w | Justo después de PG-1 | `Watch` en segundo plano cada 1 s, hasta `watch-stop.flag` | salida 1 (otra instancia) o 2 (salida sin PRE-CLOSE) = **STOP inmediato** |
| PG-2 | SM-00d1, tras el NETLOAD | inspección ligada a la PID: un solo `RackCad.Plugin.dll` **en esa PID**, en el bin del Candidato, con `3C261B8F…B6A0`; los demás `RackCad*` MATCH | STOP |
| PG-3 | Puntos críticos: antes de SM-01, antes de SM-03, tras SM-13, tras SM-17, antes de SM-20, tras SM-26, tras SM-27, tras SM-35 y antes de SM-36; además, **tras cada cambio de foco o clic fallido** (sección 4) | `Check <SM>`: acad = 1 y es AUTHORIZED_AUTOCAD_PID | STOP inmediato |
| PG-4 | Antes de SM-40 | `Check PRE-CLOSE`: la autorizada es la única; se escribe `pre-close.ok`. Después, cierre normal sin guardar | STOP |
| PG-5 | Tras SM-40 | `Closed`: acad = 0 y la PID ya no existe; se escribe `closed.ok`. La vigilancia anota la salida normal y sigue | STOP |
| PG-6 | Script de POST | además de acad = 0: exige `closed.ok` y una bitácora de vigilancia sin STOP. Después, HOST-EXTRAS AFTER y POST | no se toma la POST |
| PG-7 | Tras la POST | `watch-stop.flag`; la vigilancia termina con 0. `acad-watch.log` demuestra una sola PID, la autorizada, de PG-1 a PG-5, y 0 procesos hasta el fin | — |

## 4. Reglas de operación de la sesión (OPERATOR_EXCEPTION)

1. **Una sola apertura.** `open_application` solo en PG-1a, con acad = 0 recién comprobado. Con AutoCAD abierto no se llama nunca, ni
   para enfocar.
2. **Foco.**
   - Si un clic falla o otra ventana está delante: captura de pantalla y `Check`.
   - Después, foco con un clic en una zona visible de la ventana de AutoCAD (área de dibujo o línea de órdenes).
   - Si AutoCAD no está visible, no se busca otra vía: se para y se vuelve al Coordinator.
3. **Registro contemporáneo.** Cada paso SM se anota en `operator/session-log.md` antes de pasar al siguiente: hora UTC, acción
   exacta y resultado observado. Ninguna acción sin documentar ni en lote: un `computer_batch` solo agrupa las acciones de UN paso
   escrito, y se anota.
4. **Sin exploración.** Solo las acciones de T3D. No se repite un paso fuera de orden ni se prueba nada adicional, salvo lo que T3D
   permite de forma explícita. Una desviación necesaria se para y vuelve al Coordinator antes de seguir.
5. **Capturas.**
   - Las que pide T3D (§6 y §10), solo de la paleta y de la línea de órdenes; nunca la barra de título, el Explorador ni nombres de
     proyectos.
   - La captura de computer use no queda en disco, así que la región se guarda en PNG con `Save-ScreenRegion.ps1` (SHA-256
     `61983D32CE96DFFE54BF26629E63511BEC4A3C93E507B895AC9BB38AEB576CDF`). Probado con una región de 4×4 píxeles, después borrada; no
     sobrescribe.
   - Las coordenadas son píxeles físicos, convertidos desde la captura de computer use con su factor de escala.
   - Se guardan en `<run>/operator/screens/<SM>-<que>.png`, con su `.sha256`, y se anotan en `session-log.md`.
   - El repositorio es público: se revisan antes de custodiarlas.
6. **Dibujos.** Solo con `OPEN` dentro de la misma instancia, navegando los diálogos con clics, porque la herramienta no escribe en
   los diálogos de archivo. Nunca con doble clic desde el Explorador.
7. **Escritura en la línea de órdenes.** Sin saltos de línea dentro del texto, para no provocar un PASTECLIP. Enter aparte.
8. **Aviso de seguridad del NETLOAD (SM-00d).** «Load Once» («Cargar una vez»); nunca «Always Load». Se anotan las opciones mostradas
   y la elegida.
9. **Cierre abrupto, excepción no controlada o selector de perfil:** STOP (SM-STOP-02 o SM-STOP-05).

## 5. Ligadura de la evidencia
- **SM-00d1:** su salida incluye la PID y la hora de inicio de la instancia autorizada.
- **RackLog (SM-39) y estado de host (HOST-EXTRAS, POST, DIFF):** se atribuyen a la ocurrencia solo si `acad-watch.log` demuestra una
  sola instancia, la autorizada, en toda la ventana, y si `closed.ok` es anterior a la captura AFTER.
- **Observaciones visuales:** las anota la sesión, con las capturas de la regla 5, sobre la ventana de AUTHORIZED_AUTOCAD_PID.
- **Registro del operador:** dice de forma literal: «AutoCAD operated by implementation session under Coordinator OPERATOR_EXCEPTION
  for this occurrence.» Nunca «Owner-operated».

## 6. Cambios del kit (solo tras la autorización)
- **`owner-preflight-baseline.ps1`** (lo ejecuta la sesión):
  - RunId, Candidato y SHA nuevos;
  - el paso 2 pasa de «HEAD = Candidato» a «HEAD = `b5ea02f5d193d80e4b921b82bbeb7f87a0c8be0d` exacto», con
    `git diff --name-only cc7503e3 HEAD` limitado a `docs/automation/`, 0 archivos de producto distintos de `0e0d536b` y DLL =
    `3C261B8F…B6A0`;
  - SECURELOAD sigue con `reg query` de solo lectura, ahora ejecutado por la sesión.
- **`owner-post.ps1`** (lo ejecuta la sesión): la exigencia de PG-6.
- **`Inspect-LoadedRackCad.ps1`:** la versión ligada a la PID.
- **`Test-AcadProcessGuard.ps1`** y **`Save-ScreenRegion.ps1`:** nuevos.
- **`operator-guide.md`:** operador = la sesión bajo OPERATOR_EXCEPTION; puntos de guarda; reglas de la sección 4; comprobaciones
  críticas del cierre de C2.
- **Sin cambios:** HOST-EXTRAS, `compare_host_extras.py` y la orden exacta de T3C con las herramientas `750a9097`, `de642515` y
  `d0bb54be`.

## 7. Límites conocidos
- Un sondeo de 1 s podría no ver un proceso que viva menos de 1 s. Lo compensan los puntos de control y el conteo final.
- La vigilancia corre como proceso de la sesión. Si muere sin la línea «WATCH fin», se informa, y quedan los puntos de control. No se
  declara una ventana limpia sin la bitácora completa.
- Las autopruebas no ejercitan `acad.exe` real. La primera ejecución real es PG-1 de la ocurrencia autorizada.
