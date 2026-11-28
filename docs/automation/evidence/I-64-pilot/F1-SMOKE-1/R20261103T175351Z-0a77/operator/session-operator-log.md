# R20261103T175351Z-0a77 — Bitácora de la sesión operadora (decisión del Owner; disposition/owner-decision-session-operator.md)

Observaciones transcritas de las capturas de pantalla de computer use. Sin rutas locales salvo `<worktree>`.

- SM-00a 2026-11-03T20:00:36.589Z: AutoCAD 2025 abierto con el acceso del menú Inicio «AutoCAD 2025 - English» (computer use
  open_application), sin opciones ni selector de perfil. Una sola acad.exe (pid 41004, inicio 20:00:39.017Z). AutoCAD abrió en la
  pestaña «Start» (comportamiento habitual del host); la sesión pulsó «New» → `Drawing1.dwg` (plantilla por defecto). Línea de órdenes:
  «Regenerating model.» / «AutoCAD menu utilities loaded.*Cancel*». PASS.
- SM-00b: `(getvar "CPROFILE")` → "<<Unnamed Profile>>"; `(getvar "SECURELOAD")` → 1; `(getvar "PICKFIRST")` → 1. Coincide con la
  BASELINE. PASS. (La herramienta escribió las expresiones por el portapapeles.)
- SM-00d ~20:03Z: NETLOAD. El diálogo «Choose .NET Assembly» abrió en una carpeta ajena al Candidato (historial del diálogo: una
  carpeta `bin-Debug-net8.0-windows` de artefactos de I-65); no se eligió nada allí. La herramienta no permite escribir en este diálogo,
  así que la sesión navegó con «subir nivel» y doble clic hasta `<worktree>\src\RackCad.Plugin\bin\Debug\net8.0-windows` y seleccionó
  `RackCad.Plugin.dll` (SHA en disco 2923E7F2…AE26 comprobado a las 20:04:14Z). Aviso «Security - Unsigned Executable File»
  (Always Load / Load Once / Do Not Load; ubicación = carpeta del Candidato): **Load Once**. Línea de órdenes sin mensajes tras la carga.
  PASS.
- SM-00d1 2026-11-03T20:04:34Z: Inspect-LoadedRackCad → VERDICT = PASS. acad = 1 (pid 41004); módulos RackCad = 2
  (RackCad.Plugin.dll 2923E7F2…AE26 MATCH; RackCad.Application.dll 66E88EB1… MATCH), ambos en la carpeta del Candidato.
  process/inspect-after-netload.txt.
- SM-00e ~20:06Z: Ctrl+O → «Select File» (abrió en una carpeta de artefactos de I-65; nada elegido allí); tipo «DXF (*.dxf)»; navegación
  hasta `<worktree>\artifacts\orchestration\I-64\F1-SMOKE-1-PREP\fixtures\I64-SMOKE1-FIXTURES-D.dxf`; Open. Abre limpio: dos
  rectángulos «PR-13 SIN IDENTIDAD» y «PR-12 DIAGNOSTICO» y el rótulo superior. Historial (F2): «Opening an AutoCAD 2013/LT 2013 DXF
  file.» / «Regenerating model.» / «**** System Variable Changed **** 1 of the monitored system variables has changed from the preferred
  value. Use SYSVARMONITOR command to view changes.» / «AutoCAD menu utilities loaded.». Nota: la lista SysvarMonitor de la BASELINE es
  CMDDIA, FILEDIA, HIGHLIGHT, MIRRTEXT(0), PICKADD, PICKAUTO, PICKFIRST, SDI, SELECTIONPREVIEW; la única guardada en el dibujo es
  MIRRTEXT, así que el aviso procede del encabezado del DXF (no se ejecutó SYSVARMONITOR: fuera del procedimiento). Sin error,
  auditoría ni recuperación. PASS.
- SM-00c ~20:07Z: Ctrl+O, tipo «Drawing (*.dwg)», `<dibujos>\Dibujo C.dwg` → «Opening an AutoCAD 2018 format file.» / «**** No System
  Variable Changed ****» / «Autodesk DWG. This file is a TrustedDWG…». `(sslength (ssget "_X"))` → 17. PASS (n=17, igual que 184b).
- SM-01 ~20:08Z (C, panel nunca abierto): línea exacta de T3D §6 → «Select objects: _ALL 17 found» … «SM-T ms=203». Ningún mensaje de
  RackCad. PASS (ms=203).
- SM-02 ~20:09Z: `<dibujos>\Dibujo A.dwg` abierto («Autodesk DWG. This file is a TrustedDWG…»); `(getvar "DBMOD")` → 0. PASS.
- SM-03 ~20:09Z (C1, comprobación crítica): `RACKPANEL` → «RackCad: panel Workspace mostrado.». Paleta «RACKCAD» (título vertical,
  icono RackCad), contenido «Workspace»: **FLOTANTE, DENTRO DE LA PANTALLA**, arriba a la izquierda; en la captura (1456×819) ocupa
  x 63–302, y 63–482 ≈ 315×553 px físicos en (83,83) a 1920×1080, coherente con InitializeFloatingPosition(80,80,320,560). Visible y
  usable. Textos: «Sesión activa», «El panel sigue el dibujo activo.», «Sesión 1»; «Leer el dibujo» y «Escribir en el dibujo»
  marcados; «Selección» = «Ninguno» / «No hay objetos seleccionados.»; «Medir en memoria (no escribe en el registro)» sin marcar,
  «Instrumentación desactivada.». No se movió ni se acopló. PASS. (La paleta tapa la parte izquierda de la cinta y de la barra de
  pestañas; el cambio de dibujo se hizo con el menú Window para no moverla.)
- SM-04: `RACKPANEL` otra vez → misma paleta, sin segunda; mensaje repetido. PASS.
- SM-05: marcada la casilla → «Instrumentación activada: 0 puntos de medida.», «Memoria del proceso: 1951.7 MB». PASS.
- SM-06 (primera ejecución): el primer clic solo dio foco al dibujo; clic en el poste del rack inferior → «Un rack» / «Rack
  seleccionado: 54ef4061-d1f3-4191-9400-70fe08bafc0c»; filas 1 selection.read 32.2 ms 1954.6 MB 1 / 2 drain 32.5 ms 1954.6 MB 2.
  Para encontrar la copia la sesión alejó la vista con la rueda (5+3 pasos): la pestaña pasó a «Dibujo A*» (cambio de vista; DBMOD bit 1
  se comprueba en SM-18). Exploración fuera de orden (anotada): rack inferior + superior izquierdo → «2 racks y 0 otros objetos»;
  Esc; inferior + superior derecho → «2 racks y 0 otros objetos»; + superior izquierdo (3 objetos) → «2 racks y 0 otros objetos» ⇒ la
  pareja rack/copia (mismo RackId) son los dos racks superiores; el inferior tiene otro RackId.
- SM-06..SM-09 (secuencia en orden, tras Esc): clic en el rack superior izquierdo → «Un rack» / «Rack seleccionado:
  56ec2c3a-ff43-4468-8fb7-2648cf3ac54e» (SM-06 PASS); + copia (superior derecho) → sigue «Un rack», mismo RackId (SM-07 PASS); + rack
  inferior → «Selección actual: 2 racks y 0 otros objetos.» (SM-08 PASS); + línea suelta → «Selección actual: 2 racks y 1 otros
  objetos.» (SM-09 PASS).
- SM-10a: Esc; clic en el bloque pequeño (marcador triangular) → «Selección actual: 0 racks y 1 otros objetos.». PASS.
- SM-10b: menú Window → «2 I64-SMOKE1-FIXTURES-D.dxf» → panel «Sesión 2», «Ninguno»; Esc; clic en «PR-13 SIN IDENTIDAD» → «Sin
  identidad» / «El objeto seleccionado no tiene identidad de rack.»; ningún RackId. PASS.
- SM-11: Esc; clic en «PR-12 DIAGNOSTICO» → «Diagnóstico» / «No se pudo interpretar la definición del objeto seleccionado.»; ningún
  RackId. PASS.
- (vuelta) menú Window → «4 Dibujo A.dwg» → «Sesión 1», «Ninguno». Historial: «**** No System Variable Changed ****».
- SM-12 ~20:13–20:15Z: Esc; 36 puntos de medida antes y después de 60 s sin tocar nada; ningún «Regenerating model»; cerrada la paleta
  con su botón ×; `RACKPANEL` → «RackCad: panel Workspace mostrado.», misma posición. Filas 35 selection.read 0.1 ms / 36 drain 0.1 ms
  (antes de la espera) → 37 panel.hide 0.0 ms → 38 panel.open → 39 panel.show → 40 cache.inva… 0.1 ms → 41 selection.read 0.1 ms →
  42 drain 0.2 ms 4. Ninguna fila drain/selection.read entre la espera y panel.hide. PASS.
- SM-13 ~20:15–20:16Z: **FAIL**. `LINE` en la línea de órdenes («LINE Specify first point:»), sin marcar puntos; tras >10 s (con
  movimientos del ratón sobre el dibujo) la paleta seguía mostrando «Sesión activa», «Leer el dibujo» y «Escribir en el dibujo»
  MARCADOS y ningún «Hay un comando en curso.»; ninguna fila nueva (42). `Esc` → «Specify first point: *Cancel*»; filas 43–44
  (cache.inva…/drain); las casillas siguieron marcadas (nunca se desmarcaron). Repetido en SM-15 (LINE desde el dibujo): mismo
  resultado. Lectura de código (solo lectura, sin cambios): `EventBridgeCore.OnSourceEvent` ante `CommandWillStart` solo pone
  `state.Quiescent = false` y retorna sin `SyncIdle` ni aviso; la vista se refresca solo con `WorkspaceController.NotifyStateChanged`
  (OpenPanel/ShowPanel/HidePanel, cambio de sesión activa, OnDrained). Por eso el bloqueo «CommandActive» (WorkspaceHostPolicy) nunca
  llega a pintarse mientras dura la orden. T3D §9: «Cualquier FAIL deja Smoke-1 en NOT PASS»; no es STOP → se continúa.
- SM-14a..e, h ~20:16–20:17Z: línea suelta seleccionada (panel «0 racks y 1 otros objetos.»); clic en el rótulo «Acciones» y `Tab` →
  foco (recuadro) en «Medir en memoria». Teclas una a una: `a b c ␠ d e f` → nada en la línea de órdenes; la BARRA ESPACIADORA
  conmutó la casilla con foco → «Instrumentación desactivada.» (efecto interno de la paleta; re-activada antes de SM-16). `Ctrl+Z`,
  `Ctrl+C`, `Ctrl+V`, `Supr` (con la línea seleccionada), `Esc`, `Enter` → ninguna orden en AutoCAD (ni UNDO, COPYCLIP, PASTECLIP,
  ERASE, ni repetición de LINE); la línea sigue seleccionada y presente. PASS (complementario A-7; S1-06a..e = NOT_APPLICABLE_BY_DESIGN).
  **Incidente del operador (no de la paleta):** al escribir después `(getvar "DBMOD")` en la línea de órdenes, la herramienta de
  computer use usó su vía de portapapeles y AutoCAD recibió `Ctrl+V` como acelerador → «Command: _pasteclip» / «Specify insertion
  point:»; la sesión pulsó `Esc` → «*Cancel*», nada insertado. Historial F2: ningún UNDO/COPYCLIP/PASTECLIP/ERASE mientras la paleta
  tenía el foco; el único `_pasteclip` coincide con la acción de escritura de la sesión. Desde entonces la sesión escribe sin salto de
  línea y pulsa `Enter` aparte. SM-14h: `(getvar "DBMOD")` → 16 (bit 16 = vista, por la rueda; bit 1 = 0, igual que SM-02). PASS.
- SM-15: foco en la paleta (clic en «Acciones») y teclas `L I N E` → nada en la línea de órdenes; clic en zona vacía del dibujo (abre una
  ventana de selección), `Esc`, teclas `L I N E` `Enter` → «LINE Specify first point:»; `Esc`. PASS.
- SM-16 ~20:18–20:19Z: re-marcada «Medir en memoria» (las filas anteriores se conservan, 48 puntos); seleccionados dos racks (inferior y
  superior izquierdo) → «2 racks y 0 otros objetos», 50 puntos; cerrada la paleta (×); clic en vacío + `Esc` + clic en la línea
  (cambio de selección); espera ~6 s; `RACKPANEL` → «RackCad: panel Workspace mostrado.»; contexto actual «Ninguno» (la orden
  RACKPANEL vacía la selección implícita). Filas: 49 selection.read 2.7 ms 2 / 50 drain → 51 panel.hide → (nada con la paleta
  cerrada) → 52 panel.open → 53 panel.show → 54 cache.inva… → 55 selection.read 0.1 ms 0 → 56 drain 0.1 ms 4 (una vez). PASS.
- SM-17 ~20:19–20:21Z: arrastre al borde izquierdo → ACOPLADA IZQUIERDA (filas 57 panel.hide, 58 panel.show, 59 selection.read,
  60 drain); arrastre al borde derecho → ACOPLADA DERECHA (61–64 igual); arrastre al centro → FLOTANTE (x 636–873, y 258–676 en la
  captura; dentro de la pantalla). Ocultación automática (botón de la barra de título) → se pliega a la barra de título vertical
  («RACKCAD», franja en x≈866); pasar el ratón sintético no la despliega; clic en la franja → se despliega; desactivada → queda
  flotante expandida. Filas 65–71 panel.hide/panel.show/selection.read/drain. Sin errores. PASS. La sesión la dejó acoplada a la
  derecha para el resto.
- SM-18: `(getvar "DBMOD")` → 16; bit 1 = 0, igual que SM-02. PASS.
- SM-19: PICKFIRST = 1 → NOT_APPLICABLE.
- SM-20 ~20:22Z: `<dibujos>\Dibujo B.dwg` abierto → «Sesión 3» (≠ 1), «Ninguno». PASS.
- SM-21: clic en el rack superior de B → «Un rack» / «Rack seleccionado: a0e7f2be-ef24-4311-8482-1730235f372f». PASS.
- SM-22: pestaña A → «Sesión 1», «Ninguno»; sin restos de B. PASS.
- SM-23: pestaña B → «Sesión 3», «Ninguno» (la selección implícita no sobrevive al cambio de dibujo). PASS.
- SM-24: Ctrl+O → Dibujo A → «Open Read-Only» → «Dibujo A.dwg:2 - Read Only» → «Sesión 4» (≠ 1); pestaña A original → «Sesión 1»;
  pestaña de solo lectura → «Sesión 4». No se mezclan. PASS.
- SM-25: cerrada la copia de solo lectura (×), sin pregunta. PASS.
- SM-26 ~20:24–20:25Z: cerrados B (sin pregunta), A («Save changes to …Dibujo A.dwg?» → **No**), D (sin pregunta), Drawing1 (sin
  pregunta, «Sesión 6» al activarla) y al final C («Save changes to …Dibujo C.dwg?» → **No**). Observación: con C activo la línea de
  órdenes mostró «Automatic save to %TEMP%\Dibujo C_1_29790_60be929d.sv$ ...» (autoguardado SAVETIME de
  AutoCAD en %TEMP%, no un guardado del dibujo; SM-41 comprueba los SHA de A, B, C y D). Solo la pestaña Start: panel «Sin documento» /
  «Abre o crea un dibujo para trabajar con el panel.» / «Sin sesión activa»; «Leer el dibujo» y «Escribir en el dibujo» desmarcados;
  «No hay un documento abierto.». PASS.
- SM-27: desde Start → Recent: Dibujo C → «Sesión activa», «Sesión 7»; Dibujo B → «Sesión activa», «Sesión 8». PASS.
- SM-28 (C, paleta visible acoplada a la derecha): `LINE` de 3 segmentos y `CIRCLE` sin incidencias. PASS.
- SM-29: rueda (3 arriba, 3 abajo), `ZOOM` `E`, `PAN` (arrastre con botón izquierdo) y `Esc`. El encuadre con botón central:
  NOT_EXECUTED (la herramienta no puede arrastrar con el botón central). Resto PASS.
- SM-30: `MOVE` (1 found), `COPY` (1 found), `ERASE` (1 found), `UNDO` `3` → «ERASE COPY MOVE». PASS.
- SM-31: paleta cerrada (× de la barra acoplada, visible al pasar el ratón); línea exacta de T3D §6 → «_ALL 21 found» … «SM-T ms=250»
  (oculto). PASS.
- SM-32: `RACKPANEL` → «RackCad: panel Workspace mostrado.» (vuelve acoplada a la derecha); misma línea → «_ALL 21 found» … «SM-T
  ms=281» (visible). PASS. (21 = 17 originales de C + 3 segmentos + 1 círculo de SM-28.)
- SM-33: `RACKAYUDA` → ventana «RackCad — comandos y atajos»; «Cerrar». PASS.
- SM-34 ~20:27Z: pestaña B; `RACKEDITAR` → «Selecciona un rack para editar:»; clic en el rack superior → «RackCad – Rack selectivo: editor
  avanzado (vista frontal)», «Nombre (Rack A)» = «Selectivo 1». El panel no interfiere con el editor, PERO durante toda la orden (prompt
  y editor abierto, >4 s) la paleta mostró «Sesión activa», casillas MARCADAS y **ningún «Hay un comando en curso.»**. **FAIL**
  (misma causa que SM-13).
- SM-35: «Cerrar» (nunca aceptar) → el editor se cierra; RACKEDITAR termina; espera 3 s; `(getvar "DBMOD")` → 0. PASS.
- SM-36..SM-38: durante toda la ocurrencia ningún «Error del panel», ninguna «Sesión degradada», ningún cuadro de excepción no
  controlada, ninguna aserción Debug visible, ningún cierre inesperado. PASS.
- SM-40 ~20:28:35Z: `QUIT` → B cerró sin pregunta; «Save changes to …Dibujo C.dwg?» → **No**; AutoCAD se cerró. acad.exe = 0 y RAM = 0 a
  las 2026-11-03T20:28:57Z. PASS.
- HOST-EXTRAS AFTER + POST (kit/owner-post.ps1, ejecutado por la sesión): HOST-EXTRAS AFTER código 0, sha256 4709100e…4514;
  POST T3C código 0, CanonicalSha256 0a230e7eb2cb5637749a5c00b9156c828d4aa236c6aa67007dcabd46fa6b7d16 (2241 líneas, 399 claves, 1772
  valores, 21 archivos, 3 plugins, env 0, ilegibles 0).
- DIFF T3C (PaletteKeyAcceptance = ACCEPTED): DISPOSITION=CLEAN código 0; added 6, removed 0, changed 17; ALLOWED 23 (1 R-A-01 RackLog,
  22 R-A-03 actividad normal de AutoCAD); REPORT 0; FORBIDDEN 0; INVALID 0. diff/profile-diff.json (24985d2d…).
- HOST-EXTRAS DIFF: REPORT_REQUIRED (código 4). AssemblyMap idéntico (Exists, 2 valores, RackCad.Plugin ausente, LastWrite
  2026-11-02T19:54:39.812Z antes y después). CoreUser 56 → 56 entradas sin cambios. Clave de producto 22 → 23: (a) NetLoadDialog
  VALUES_CHANGED → INFORMATIVE (MRU reordenado: el DLL del Candidato pasa a MRU0; InitialDirectory = carpeta del Candidato); (b)
  `Profiles\<PROFILE>\Dialogs\Select File` ADDED → REPORT_REQUIRED (pasa a contener un valor con «rackcad»: MRU del diálogo Abrir con
  la ruta del dibujo D en el worktree); (c) `Recent File List` VALUES_CHANGED → REPORT_REQUIRED (MRU de archivos: entra la ruta del
  dibujo D, sale una entrada antigua). Ninguna entrada nueva o cambiada de RackCad en AssemblyMap, Applications ni Loaded.
  diff/host-extras-diff.json (88e27c92…). Nota de privacidad: los JSON de HOST-EXTRAS guardan los datos crudos del registro (incluye
  rutas locales); son locales (artifacts/ en .gitignore) y no se citan en este registro.
- SM-39: rackcad-20261103.log (1720 bytes, sha256 1F9B1E6F…8865): 2 entradas nuevas desde SM-00a, ambas durante SM-26:
  [20:24:21.278Z] y [20:24:40.789Z] «ERROR AutoCadEventBridge.DocumentActivated: System.NullReferenceException: Object reference not
  set to an instance of an object.» en `RackCad.Plugin.Workspace.AutoCadEventBridge.NativeId(Document)` (AutoCadEventBridge.cs:251)
  ← lambda de `Hook` para DocumentActivated (línea 123) ← `Contain` (línea 279). Explicación (lectura de código, sin cambios): el
  manejador hace `NativeId(e.Document)` = `e.Document.UnmanagedObject` sin comprobar nulo; al cerrar dibujos AutoCAD activa la pestaña
  Start (sin documento) y entrega `e.Document == null`. `Contain` registra la excepción; no llegó a AutoCAD ni cambió la paleta (mostró
  «Sin documento» correcto). Ningún «A workspace drain left a transaction open.» ni «The selection read left a transaction open.».
  Criterio T3D SM-39 «ninguna con `Workspace`»: **FAIL**.
- SM-41: SHA-256 iguales al preflight: A 90750e8b…, B fb9b97f9…, C ede7cdb3…, D 03e5606c…, biblioteca B4CA2248…; fechas de modificación
  sin cambios. PASS.
- I-52: aviso de devolución del host enviado (message_id 48ac2dd1-4f5a-4cae-a3f1-1493af3a94e7); recomprobación pendiente.
- I-52 recomprobación recibida (2026-11-03T20:35:19Z): sin deriva; libera su retención sobre H10 respecto de esta ventana; no concede ACK/RELEASE; P2 H10 NOT_AUTHORIZED. disposition/i52-post-run-recheck.txt.
- FE DE ERRATAS (2026-11-03, antes de la firma del Owner; por correlación con las marcas de tiempo de las capturas): de las dos
  entradas de RackLog, la de 20:24:21.278Z es de SM-26 (cerrar C, el último dibujo; capturas 20:24:17Z aviso de guardar → 20:24:23Z solo
  Start) y la de 20:24:40.789Z es de SM-27 (clic en la pestaña Start con C reabierto; capturas 20:24:36Z → 20:24:43Z, panel aún
  «Sesión 7»). Las líneas anteriores de esta bitácora que dicen «ambas durante SM-26» quedan corregidas por esta. Ningún resultado cambia.
- Revisión del Coordinator recibida: NOT PASS; ocurrencia válida; desviación de rol ACEPTADA COMO DESVIACIÓN DE PROCEDIMIENTO; C1 PASS
  (authorization/coordinator-smoke1-retry-review.md). Análisis de fallos: disposition/failure-analysis.md.
- Firma del Owner recibida por el chat: «Firmo el registro.» (2026-11-04T15:06:53Z, hora medida por la sesión). Anotada en
  operator/operator-record.md. La firma confirma el registro, no un PASS; NOT PASS y la desviación de rol se conservan; ningún
  resultado SM cambia.
- Observación fuera de la ocurrencia: a las 2026-11-04T15:06Z hay un acad.exe en ejecución (inicio 2026-11-03T21:31:43Z, lanzado
  desde el Explorador de Windows), posterior al fin de la ocurrencia (acad = 0 a las 20:28:57Z) y a la recomprobación de I-52. La
  sesión I-64 no lo inició ni lo tocó; no forma parte de esta evidencia.
- Custodia de la ocurrencia tras la firma: copia sin rutas locales en docs/automation/evidence/I-64-pilot/F1-SMOKE-1/ con
  custody-manifest.json (SHA-256 originales). Desde aquí estos archivos transitorios no se modifican.
