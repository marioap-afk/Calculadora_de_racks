# R20261104T194513Z-a578 — Bitácora de la sesión (opera el Owner; la sesión solo guía, inspecciona en solo lectura y transcribe)

Observaciones informadas por el Owner en el chat y mediciones de la sesión. Sin rutas locales salvo `<worktree>`.

- 22:05:31Z / 22:06:37Z: la sesión cuenta acad.exe = 0 y RAMElements.exe = 0 (el AutoCAD abierto desde 2026-11-03T21:31:43Z ya estaba
  cerrado). RackLog BEFORE en `logs/racklog-BEFORE.txt`.
- 22:07:53Z: aviso a I-52 antes de la BASELINE (msg 040e0ac2).
- 22:10:29Z: preflight del Owner → OK (BASELINE 690d6926…f017; HOST-EXTRAS BEFORE f15fa649…b34d; SECURELOAD_BASELINE_CURRENT = 1).
- 22:12:05Z: BASELINE CONFIRMADA por la sesión (`preflight/baseline-verification-session.txt`).
- SM-00a: AutoCAD abierto por el Owner; acad.exe pid 46292, inicio 2026-11-04T22:14:14.884Z (medido por la sesión).
- SM-00d: NETLOAD por el Owner; el Owner informa «cargado».
- SM-00d1 2026-11-04T22:18:34Z: Inspect-LoadedRackCad (sesión, solo lectura) → **VERDICT = PASS**. acad = 1 (pid 46292); módulos RackCad = 2,
  ambos en la carpeta del Candidato: RackCad.Plugin.dll 3C261B8F…B6A0 MATCH (uno solo) y RackCad.Application.dll 12B8E237…27B3 MATCH.
  RAMElements.exe = 0. `process/inspect-after-netload.txt`.
- SM-00a/SM-00b (Owner, informado en el chat): AutoCAD abrió en la pestaña Start; CPROFILE «<<Unnamed Profile>>», SECURELOAD 1,
  PICKFIRST 1. Coincide con la BASELINE. Selector de perfil: no informado. SM-00d: opciones del aviso de seguridad y opción elegida NO
  informadas; historial de la línea de órdenes: «Command: NETLOAD» seguido de líneas «Command:» vacías (sin mensajes tras la carga).
- ~22:21Z ORDEN DEL OWNER: «De ahí continúa tú tomando el control, no cuestiones mi orden» → desde SM-00e opera la SESIÓN con computer use
  (desviación nueva; disposition/owner-order-session-operator.md). Herramienta: no permite escribir en los diálogos de archivo; navegación
  con Look in / doble clic / History.
- SM-00e ~22:23Z: Ctrl+O, tipo DXF, History → I64-SMOKE1-FIXTURES-D.dxf (el siguiente Ctrl+O abrió en la carpeta `fixtures` del PREP, con
  `harness` y `readback`: es la copia del worktree). Abre limpio: dos rectángulos «PR-13 SIN IDENTIDAD» y «PR-12 DIAGNOSTICO» y el rótulo.
  Mensajes: «**** System Variable Changed **** 1 of the monitored system variables has changed from the preferred value. Use SYSVARMONITOR
  command to view changes.» / «AutoCAD menu utilities loaded.» (igual que en 0a77). PASS.
- SM-00c: Ctrl+O, tipo DWG, Look in → D: → Dibujos → Dibujo C. «Opening an AutoCAD 2018 format file.» / «Regenerating model.» / «**** No
  System Variable Changed ****» / «AutoCAD menu utilities loaded.» / «Autodesk DWG. This file is a TrustedDWG…». `(sslength (ssget "_X"))`
  → 17. PASS.
- SM-01 (C, panel nunca abierto): línea exacta de T3D §6 → «Select objects: _ALL 17 found» … «SM-T ms=172»; ningún mensaje de RackCad. PASS.
- SM-02: Dibujo A abierto (TrustedDWG); `(getvar "DBMOD")` → 0. PASS.
- SM-03 (C1): `RACKPANEL` → «RackCad: panel Workspace mostrado.». Paleta «RACKCAD» FLOTANTE DENTRO DE LA PANTALLA, arriba a la izquierda
  (en la captura 1456×819: x 63–302, y 63–482), visible y usable. «Workspace», «Sesión activa», «El panel sigue el dibujo activo.»,
  «Sesión 1»; «Leer el dibujo» y «Escribir en el dibujo» marcados; «Ninguno» / «No hay objetos seleccionados.»; «Medir en memoria (no
  escribe en el registro)» sin marcar, «Instrumentación desactivada.». No se movió. PASS.
- SM-04: `RACKPANEL` otra vez → misma paleta, sin segunda; mensaje repetido. PASS.
- SM-05: «Medir en memoria» marcado → «Instrumentación activada: 0 puntos de medida.», «Memoria del proceso: 1947.7 MB». PASS.
- SM-06: primer clic (exploración fuera de orden) en el rack visible → «Un rack» / «Rack seleccionado: 54ef4061-d1f3-4191-9400-70fe08bafc0c»
  (filas 1 selection.read 30.0 ms, 2 drain 30.3 ms). `Esc` y alejamiento con la rueda (3+3+2 hacia fuera, 2 hacia dentro, 1 fuera) para
  ver el dibujo entero (la pestaña pasa a «Dibujo A*»: solo vista). En orden: rack superior izquierdo → «Un rack» / «Rack seleccionado:
  56ec2c3a-ff43-4468-8fb7-2648cf3ac54e» (filas 5 selection.read 5.9 ms, 6 drain 5.9 ms). PASS.
- SM-07: + rack superior derecho (copia) → sigue «Un rack», 56ec2c3a-… (filas 7, 8). PASS.
- SM-08: + rack inferior → «Selección actual: 2 racks y 0 otros objetos.» (filas 9, 10). PASS.
- SM-09: + línea → «Selección actual: 2 racks y 1 otros objetos.» (filas 11, 12). PASS.
- SM-10a: `Esc`; clic en el bloque pequeño junto al rack inferior → «Selección actual: 0 racks y 1 otros objetos.». PASS.
- SM-10b: `Esc`; menú Window → «2 I64-SMOKE1-FIXTURES-D.dxf» (la paleta tapa las pestañas); «Sesión 2», «Ninguno»; `Esc`; clic en
  «PR-13 SIN IDENTIDAD» → «Sin identidad» / «El objeto seleccionado no tiene identidad de rack.»; sin RackId. PASS.
- SM-11: `Esc`; clic en «PR-12 DIAGNOSTICO» → «Diagnóstico» / «No se pudo interpretar la definición del objeto seleccionado.»; sin
  RackId. PASS.
- (vuelta) menú Window → «4 Dibujo A.dwg»; «Sesión 1», «Ninguno»; 30 puntos.
- SM-12: `Esc`; última fila antes de la espera = 30 drain 0.1 ms (la tabla se desplazó arrastrando la barra de la paleta); espera de 62 s
  sin tocar nada → sigue en 30; sin «Regenerating model». Botón de cierre de la paleta y `RACKPANEL` → «RackCad: panel Workspace
  mostrado.»; filas 31 panel.hide, 32 panel.open, 33 panel.show, 34 cache.invalidate, 35 selection.read, 36 drain. Ninguna fila entre la
  espera y panel.hide. PASS.
- **SM-13 (C2, crítico)**: antes, 36 puntos y casillas marcadas. `LINE` (sin puntos) → «LINE Specify first point:»; panel: «Leer el dibujo»
  y «Escribir en el dibujo» DESMARCADOS y «Hay un comando en curso.»; a los ~2 s y a los ~7 s el contador sigue en 36 (ninguna fila
  drain/selection.read durante la orden). `Esc` → «Specify first point: *Cancel*»; casillas marcadas otra vez, el texto desaparece;
  filas nuevas 37 selection.read y 38 drain (después de la orden). **PASS.**
- SM-14a..e: línea suelta preseleccionada («0 racks y 1 otros objetos.»); clic en el encabezado «Instrumentación» de la paleta y `Tab` →
  foco (recuadro punteado) en «Medir en memoria»; teclas a, b, c, espacio, d, e, f; `Ctrl+Z`; `Ctrl+C`; `Ctrl+V`; `Supr`; `Esc`; `Enter` →
  la línea de órdenes no cambia (ningún UNDO, COPYCLIP, PASTECLIP ni ERASE); la línea sigue existiendo y seleccionada. El espacio conmutó
  la casilla («Instrumentación desactivada.»), como en 0a77. PASS (complementario, A-7).
- SM-14h: `(getvar "DBMOD")` → 16 (bit 1 = 0; bit 16 = vista). PASS.
- SM-15: clic en el encabezado «Selección» de la paleta; teclas l, i, n, e → nada en la línea de órdenes. Clic en un punto vacío del
  dibujo (ventana de selección), `Esc`, teclas LINE + Enter → «LINE Specify first point:» y el panel muestra casillas desmarcadas y «Hay un
  comando en curso.»; `Esc`. PASS.
- SM-16: «Medir en memoria» marcada otra vez; dos racks (superior izquierdo + inferior) → «Selección actual: 2 racks y 0 otros objetos.»
  (44 puntos; última fila 44 drain); botón de cierre de la paleta; `Esc` y selección del rack superior derecho con la paleta cerrada;
  5 s; `RACKPANEL` → filas 45 panel.hide, 46 panel.open, 47 panel.show, 48 cache.invalidate, 49 selection.read, 50 drain (una vez);
  contexto actual «Ninguno» / «No hay objetos seleccionados.» (RACKPANEL vacía la selección implícita, como en 0a77). PASS.
- **SM-17 (C1)**: arrastre de la barra de título al borde izquierdo → ACOPLADA IZQUIERDA (54 puntos); al borde derecho → ACOPLADA DERECHA
  (58); al centro → FLOTANTE dentro de la pantalla (x 614–853, y 294–715 en la captura 1456×819; 62); botón de ocultación automática → se
  pliega a la franja vertical «RACKCAD» (x≈846); clic en la franja → se despliega; botón otra vez → desactivada (queda desplegada con el
  ratón fuera). Sin errores. La sesión la deja acoplada a la derecha (66 puntos). **PASS.**
- SM-18: `(getvar "DBMOD")` → 16 (bit 1 = 0, igual que SM-02). PASS.
- SM-19: PICKFIRST = 1 → NOT_APPLICABLE.
- SM-20 ~22:30Z: Ctrl+O → Dibujo B → «Sesión 3» (≠ 1), «Ninguno». PASS.
- SM-21: clic en el rack superior de B → «Un rack» / «Rack seleccionado: a0e7f2be-ef24-4311-8482-1730235f372f». PASS.
- SM-22: pestaña A → «Sesión 1», «Ninguno»; sin restos de B. PASS.
- SM-23: pestaña B → «Sesión 3», «Ninguno». PASS.
- SM-24: Ctrl+O → Dibujo A → Open ▾ → «Open Read-Only» → «Dibujo A.dwg:2 - Read Only» → «Sesión 4» (≠ 1); pestaña A original →
  «Sesión 1»; copia → «Sesión 4». PASS.
- SM-25: cierre de la copia de solo lectura con la X de su pestaña → sin pregunta. PASS.
- **SM-26 (C2, crítico)** ~22:32–22:34Z: B cerrado (sin pregunta); A cerrado → «Save changes to …Dibujo A.dwg?» → **No** (durante la
  pregunta el panel mostró casillas desmarcadas y «Hay un comando en curso.»); C activo → «Sesión 5»; autoguardado SAVETIME de C a la
  carpeta temporal («Automatic save to …\Temp\Dibujo C_1_…sv$ ...»; no es un guardado del dibujo); D cerrado con Window › Close (sin
  pregunta); Drawing1 («Sesión 6») cerrado con Window › Close (sin pregunta); RackLog a las 22:34:31Z: sin archivo nuevo (último
  rackcad-20261103.log, 1720 bytes, sin cambio). C cerrado con Window › Close → «Save changes to …Dibujo C.dwg?» → **No** → página Start:
  panel «**Sin documento**» / «Abre o crea un dibujo para trabajar con el panel.» / «Sin sesión activa»; «Leer el dibujo» y «Escribir en
  el dibujo» desmarcados; «No hay un documento abierto.»; «Ninguno». RackLog a las 22:34:52Z: sin entradas nuevas (ningún archivo
  rackcad-20261104.log). **PASS** (sin NullReferenceException de DocumentActivated).
- **STOP SM-STOP-02 (22:34:52Z)**: al leer RackLog la sesión cuenta acad.exe = **2**: pid 46292 (inicio 22:14:14.884Z, la del Owner con el
  Candidato cargado, la operada) y pid **9640** (inicio **2026-11-04T22:20:52.493Z**), las dos con ventana «Autodesk AutoCAD 2025 -
  [Start]». Causa: la propia sesión. Al tomar el control (~22:20:40Z) su primer clic falló porque el escritorio estaba al frente y la
  sesión llamó a `open_application("AutoCAD 2025 - English")` para traer AutoCAD al frente; la herramienta lanzó una SEGUNDA instancia
  (22:20:52Z) en vez de solo enfocar la existente. Todas las acciones de la sesión fueron sobre la pid 46292 (RACKPANEL y el panel solo
  existen ahí). La sesión no contó procesos entre SM-00d1 (22:18:34Z, acad = 1) y las 22:34:52Z. T3D §11 SM-STOP-02 («otra acad.exe …
  durante la ventana») → STOP: no se continúa la secuencia (SM-27…SM-35 NOT_EXECUTED); se cierra AutoCAD sin guardar y se toma el POST.

## Cierre tras el STOP (SM-39, SM-41, POST)
- ~22:35–22:36Z: la sesión cierra primero la pid 46292 (la operada) con la X de su ventana, sin dibujos abiertos (página Start) → sale con
  normalidad; acad = 1 (queda 9640). Cierra después la pid 9640 con la X de su ventana (página Start, sin dibujos) → sale con normalidad.
  Recuento a las 22:36:54Z: **acad.exe = 0, RAMElements.exe = 0**. Ningún diálogo de guardar en ninguno de los dos cierres.
- **SM-39 (RackLog, hasta el STOP y tras el cierre)**: la carpeta de RackLog está igual que en la lectura BEFORE de las 22:06:24Z (cuatro
  archivos; el último rackcad-20261103.log, 1720 bytes, 1F9B1E6F…8865, LW 2026-11-03T20:24:40.790Z). Ningún rackcad-20261104.log:
  durante la ventana el plugin no escribió en RackLog (el panel mide en memoria). Sin NullReferenceException, sin diagnóstico de fuga de
  transacción, sin ERROR. Releído tras el POST (2026-11-05T00:39Z): sin cambio, sin rackcad-20261105.log. **PASS hasta el STOP.**
- **SM-41 (dibujos)**: SHA-256 de A 90750e8b…4852, B fb9b97f9…682b, C ede7cdb3…a53c, D 03e5606c…f165 y biblioteca B4CA2248…60E8 =
  los del preflight; fechas de la carpeta de dibujos sin cambio (2026-10-13). Ningún dibujo guardado. **PASS.**
- 2026-11-05T00:38:52–54Z: el Owner ejecuta `kit/owner-post.ps1` → HOST-EXTRAS AFTER y POST de T3C (código 0; CanonicalSha256
  d651ddf4…c7b2; 2240 líneas). La sesión cuenta acad = 0 y RAM = 0 a las 00:39:14Z.
- ~00:40Z: DIFF de T3C (herramientas 750a9097 / de642515 / d0bb54be; PaletteKeyAcceptance = ACCEPTED) → **CLEAN, código 0**; ALLOWED 2
  (R-A-03: FixedProfile\Application Window\Position CHANGED; Variables\VIEWBACKSTATUS REMOVED); sidecars de BASELINE y POST OK;
  `diff/profile-diff.json` 1edd3f56…d7aa; consola en `console/diff.txt`.
- ~00:41Z: HOST-EXTRAS BEFORE vs AFTER (`kit/compare_host_extras.py` 156ffa00) → REPORT_REQUIRED, código 4: solo dos LASTWRITE_ONLY con
  valores idénticos en la clave del producto (Dialogs\Select File → 22:33:00.077Z; Recent File List → 22:36:41.644Z); AssemblyMap y
  CoreUser idénticos. Lectura: INFORMATIVE (regla de 0a77). Limitación del POST: la última en salir fue la pid 9640 (Recent File List
  escrita a las 22:36:41.644Z, con los valores de la BEFORE); ver operator-record.md.
- ~00:40Z: mensaje del Owner «Avanza todo lo que puedas, si es necesario utiliza tú AutoCAD sin solicitar permisos ya que no estaré» →
  la sesión no abre AutoCAD (nada pendiente lo necesita; STOP vigente); disposition/owner-message-after-stop.md.
- 00:43:04Z: aviso a I-52 tras el POST y petición de recomprobación (msg 5112203c).
- 00:44:10Z: acad = 0, RAM = 0. Registro del operador y paquete de vuelta redactados (firma del Owner PENDIENTE).
- 00:43:29Z (leída después): recomprobación de I-52 «SIN DERIVA»; guardada literal en disposition/i52-post-run-recheck.txt.
- 04:47:56Z: firma del Owner, literal «Firmo el registro.» (disposition/owner-signature.md). acad = 0, RAM = 0; HEAD = origin =
  cc7503e3. Queda a la espera de COORDINATOR_SMOKE1_REVIEW_REQUIRED.
- ~05:00Z: revisión del Coordinator, **STOP CONFIRMED** (HOST_ISOLATION_VIOLATION / MULTIPLE_AUTOCAD_INSTANCES), literal en
  authorization/coordinator-second-smoke1-retry-review.md. Se aplica sin cambiar el registro: disposition/stop-closure.md.
- 04:56Z: recomprobación de host de la sesión, solo lectura: acad = 0 y RAM = 0; HOST-EXTRAS frente al AFTER = CLEAN. Está en el
  transitorio de preparación de la ocurrencia nueva (F1-SMOKE-1-RETRY-PREP/host-recheck-after-a578/).
- 04:58Z: petición a I-52 de un recheck de solo lectura (msg c051542a). Después, custodia de esta ocurrencia; desde la custodia no se
  tocan los transitorios de este RunId.
