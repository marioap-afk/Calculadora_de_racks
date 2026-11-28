# I-64 Smoke-1 (SEGUNDO REINTENTO) — Registro del operador, RunId R20261104T194513Z-a578

Procedimiento: `docs/initiatives/I-64-f1-t3d-smoke-1-procedure.md` (T3D), con el orden de la autorización del Coordinator del 2026-11-04
(`authorization/coordinator-second-smoke1-retry-authorization.md`). Resultado por paso: `PASS`, `FAIL`, `NOT_EXECUTED` (con motivo),
`NOT_APPLICABLE` o `NOT_APPLICABLE_BY_DESIGN` (solo S1-06a..e, por I-64-A-7). Sin rutas locales en este archivo. Las horas y los hashes los
mide la sesión. Bitácora detallada: `operator/session-log.md`.

```text
RunId:                    R20261104T194513Z-a578 (0a77 = NOT PASS histórico; 1be6 = NOT_STARTED / SUPERSEDED)
Candidate SHA:            cc7503e3e4e56bff3a2fc9de7ca30a5322b57ea6 (producto 0e0d536b10350cc85e4a932a0d576b22fae22fa9;
                          09f2f1c3 = HISTORICAL_SMOKE1_NOT_PASS_CANDIDATE)
DLL SHA-256:              3C261B8F156E69192415E0084D0BC8774B3080ACE9D6CB4B9BB0C1760549B6A0
Inicio (UTC, SM-00a):     acad.exe pid 46292, inicio 2026-11-04T22:14:14.884Z (abierta por el Owner)
Fin (UTC):                STOP SM-STOP-02 a las 22:34:52Z; acad.exe = 0 y RAMElements.exe = 0 a las 22:36:54Z
Operador:                 el OWNER en SM-00a, SM-00b y SM-00d; la SESIÓN con computer use desde SM-00e por orden literal del Owner
                          («De ahí continúa tú tomando el control, no cuestiones mi orden»). DESVIACIÓN NUEVA de T3D §1 y de la
                          autorización («It must NOT operate AutoCAD instead of the Owner»); disposition/owner-order-session-operator.md
Causa del STOP:           la SESIÓN lanzó una segunda acad.exe (pid 9640, inicio 22:20:52.493Z) al llamar a open_application para
                          traer AutoCAD al frente; detectada a las 22:34:52Z. Error propio de la sesión (ver «Incidentes»)
HOST_RELEASE:             GRANTED BY OWNER / COORDINATOR, solo para I-64; I-52 no toca el host de la BASELINE al POST
PaletteKeyAcceptance:     ACCEPTED (Coordinator)
Carga única autorizada:   SI, solo para el DLL 3C261B8F...B6A0, no persistente; inspección del proceso tras el NETLOAD (SM-00d1)
SECURELOAD_BASELINE_CURRENT: 1 (leído por el Owner en solo lectura; BASELINE MATCH; CanonicalSha256 690d6926...f017)
AssemblyMap (BEFORE):     2 valores (AppManagerPlugin, FeaturedAppsPlugin), RackCad.Plugin ABSENT, LastWrite 2026-11-02T19:54:39.812Z
Rutas desaparecidas:      <carpeta-de-I-52> y <carpeta-local-de-I-65> = UNKNOWN_EXTERNAL_HOST_CHANGE, Attribution NONE, NON-BLOCKING; no tocadas
I-64-A-7:                 AGREED / ACTIVE
Dibujo D (PR-12, PR-13):  I64-SMOKE1-FIXTURES-D.dxf, SHA-256 03e5606c1e311f700640125b892fe09bf408dd5642115ebcb433b57474ccf165; nunca se guarda
Dibujos A, B, C:          nombres y SHA-256 en preflight/preflight-owner.txt (paso 5)
```

| Paso | S1 / OV / INV | Resultado | Observado (textos literales del panel y de la línea de órdenes) | Valores | Notas |
|---|---|---|---|---|---|
| SM-00a | arranque | PASS | AutoCAD 2025 abierto por el Owner; pestaña Start | una sola acad.exe (pid 46292) en ese momento | selector de perfil: no informado por el Owner |
| SM-00b | perfil, SECURELOAD, PICKFIRST | PASS | informado por el Owner: CPROFILE «<<Unnamed Profile>>», SECURELOAD 1, PICKFIRST 1 | CPROFILE=<<Unnamed Profile>>; SECURELOAD=1; PICKFIRST=1 | coincide con la BASELINE |
| SM-00d | NETLOAD | PASS (indirecto) | el Owner informó «cargado»; historial de la línea de órdenes: «Command: NETLOAD» y líneas «Command:» vacías (sin mensajes tras la carga) | opciones del aviso de seguridad y opción elegida: **NO INFORMADAS** | sin rastro persistente en el estado final: TRUSTEDPATHS y SECURELOAD iguales, ningún registro de carga (DIFF y HOST-EXTRAS); ver la limitación del POST |
| SM-00d1 | integridad del Candidato (autorización) | PASS | Inspect-LoadedRackCad: VERDICT = PASS (22:18:34Z) | DLL cargado=`<worktree>\src\RackCad.Plugin\bin\Debug\net8.0-windows\RackCad.Plugin.dll`; SHA-256=3C261B8F…B6A0; RackCad.Plugin=1 (más RackCad.Application.dll 12B8E237…27B3 MATCH) | process/inspect-after-netload.txt; acad = 1 en ese momento |
| SM-00e | abrir el dibujo D | PASS | «**** System Variable Changed **** 1 of the monitored system variables has changed from the preferred value. Use SYSVARMONITOR command to view changes.» / «AutoCAD menu utilities loaded.»; dos rectángulos «PR-13 SIN IDENTIDAD» y «PR-12 DIAGNOSTICO» | abre limpio | primera acción de la sesión como operadora; la herramienta no escribe en los diálogos de archivo (History, Look in) |
| SM-00c | entidades de C | PASS | `(sslength (ssget "_X"))` → 17 | n=17 | igual que 0a77 |
| SM-01 | S1-10 / esc. 1 y 10 (nunca abierto) | PASS | «Select objects: _ALL 17 found» … «SM-T ms=172»; ningún mensaje de RackCad | ms=172 | |
| SM-02 | S1-08 (lectura previa) | PASS | `(getvar "DBMOD")` → 0 | DBMOD=0 | |
| **SM-03** | RACKPANEL / INV-02 (**C1 crítico**) | **PASS** | «RackCad: panel Workspace mostrado.»; paleta «RACKCAD», «Workspace», «Sesión activa», «El panel sigue el dibujo activo.», «Sesión 1»; «Leer el dibujo» y «Escribir en el dibujo» marcados; «Ninguno» / «No hay objetos seleccionados.» | Sesión n=1 | FLOTANTE DENTRO DE LA PANTALLA, arriba a la izquierda (x 63–302, y 63–482 en la captura 1456×819); visible y usable; sin rescate; no se movió hasta SM-17 |
| SM-04 | RACKPANEL repetido / INV-02 | PASS | misma paleta, sin segunda; mensaje repetido | | |
| SM-05 | instrumentación | PASS | «Instrumentación activada: 0 puntos de medida.», «Memoria del proceso: 1947.7 MB» | filas: 0 | |
| SM-06 | S1-04 un rack / esc. 3 | PASS | «Un rack» / «Rack seleccionado: 56ec2c3a-ff43-4468-8fb7-2648cf3ac54e» | filas 5 selection.read 5.9 ms, 6 drain 5.9 ms | antes, clic de exploración fuera de orden: «Un rack» 54ef4061-d1f3-4191-9400-70fe08bafc0c (filas 1–2); alejamiento con la rueda (solo vista) |
| SM-07 | S1-04 copia | PASS | sigue «Un rack», 56ec2c3a-… | filas 7, 8 | |
| SM-08 | S1-04 dos racks | PASS | «Selección actual: 2 racks y 0 otros objetos.» | filas 9, 10 | |
| SM-09 | S1-04 con otra entidad | PASS | «Selección actual: 2 racks y 1 otros objetos.» | filas 11, 12 | |
| SM-10a | S1-04 bloque sin datos de RackCad | PASS | «Selección actual: 0 racks y 1 otros objetos.» | | |
| SM-10b | S1-04 sin identidad / INV-16 (D) | PASS | «Sesión 2»; «Sin identidad» / «El objeto seleccionado no tiene identidad de rack.»; sin RackId | | cambio de dibujo por el menú Window (la paleta tapa las pestañas) |
| SM-11 | S1-04 diagnóstico / INV-17 (D) | PASS | «Diagnóstico» / «No se pudo interpretar la definición del objeto seleccionado.»; sin RackId | | vuelta a A («Sesión 1») |
| SM-12 | S1-09 / esc. 2 / INV-01 | PASS | 30 puntos antes y después de 62 s; sin «Regenerating model»; cierre de la paleta y `RACKPANEL` → «RackCad: panel Workspace mostrado.» | última fila antes de la espera=30 drain 0.1 ms; filas entre la espera y panel.hide=0 (31 panel.hide, 32 panel.open, 33 panel.show, 34 cache.invalidate, 35 selection.read, 36 drain) | |
| **SM-13** | S1-06f (**C2 crítico**) | **PASS** | con `LINE` activo («LINE Specify first point:»): «Leer el dibujo» y «Escribir en el dibujo» DESMARCADOS y «Hay un comando en curso.»; tras `Esc` («*Cancel*») vuelven a marcarse y el texto desaparece | última fila antes=36; a ~2 s y ~7 s sigue en 36 (ninguna fila drain/selection.read durante la orden); después de la orden: 37 selection.read, 38 drain | corrige el FAIL de 0a77 |
| S1-06a..e | INV-24 (A-7) | NOT_APPLICABLE_BY_DESIGN | | | por A-7 |
| SM-14a..e | evidencia complementaria (A-7) | PASS | con foco en «Medir en memoria»: a b c espacio d e f, `Ctrl+Z`, `Ctrl+C`, `Ctrl+V`, `Supr` (línea preseleccionada), `Esc`, `Enter` → nada en AutoCAD (ni UNDO, COPYCLIP, PASTECLIP ni ERASE) | | el espacio conmutó la casilla («Instrumentación desactivada.»), como en 0a77; reactivada antes de SM-16 |
| SM-14h | S1-06h | PASS | `(getvar "DBMOD")` → 16 | DBMOD=16 (bit 1 = 0) | |
| SM-15 | S1-06g | PASS | foco en la paleta: `LINE` no llega a la línea de órdenes; en el dibujo: «LINE Specify first point:» y el panel muestra casillas desmarcadas y «Hay un comando en curso.»; `Esc` | | |
| SM-16 | INV-33, ocultar y mostrar | PASS | 2 racks; cierre de la paleta; cambio de selección con la paleta cerrada; 5 s; `RACKPANEL` → «Ninguno» / «No hay objetos seleccionados.» | filas con la paleta cerrada=0 (44 drain → 45 panel.hide); al mostrar: 46 panel.open, 47 panel.show, 48 cache.invalidate, 49 selection.read, 50 drain (una vez) | RACKPANEL vacía la selección implícita, como en 0a77 |
| **SM-17** | acoplar, flotar, ocultación automática (**C1 crítico**) | **PASS** | acoplada izquierda; acoplada derecha; flotante dentro de la pantalla (x 614–853, y 294–715 en la captura 1456×819); ocultación automática: se pliega a la franja vertical «RACKCAD», se despliega con clic, se desactiva; sin errores | puntos 54, 58, 62, 66 | la sesión la dejó acoplada a la derecha |
| SM-18 | S1-08, S1-06h, OV-20 | PASS | `(getvar "DBMOD")` → 16 | DBMOD=16 (bit 1 = 0, igual que SM-02) | |
| SM-19 | S1-12 | NOT_APPLICABLE | PICKFIRST = 1 | | |
| SM-20 | S1-05 / esc. 9 | PASS | B: «Sesión 3», «Ninguno» | m=3 | |
| SM-21 | S1-05 | PASS | «Un rack» / «Rack seleccionado: a0e7f2be-ef24-4311-8482-1730235f372f» | | |
| SM-22 | S1-05 / INV-07, INV-22 | PASS | A: «Sesión 1», «Ninguno»; sin restos de B | | |
| SM-23 | S1-05 | PASS | B: «Sesión 3», «Ninguno» | | |
| SM-24 | S1-05 / OV-15 | PASS | «Dibujo A.dwg:2 - Read Only» → «Sesión 4»; A original → «Sesión 1» | p=4 | |
| SM-25 | INV-22 | PASS | copia de solo lectura cerrada sin pregunta | | |
| **SM-26** | sin documento (**C2 crítico**) | **PASS** | «Sin documento» / «Abre o crea un dibujo para trabajar con el panel.» / «Sin sesión activa»; casillas desmarcadas; «No hay un documento abierto.»; «Ninguno» | RackLog 22:34:31Z y 22:34:52Z: sin entradas nuevas | B, D y Drawing1 sin pregunta; A y C con «No»; durante la pregunta de A el panel mostró «Hay un comando en curso.»; autoguardado SAVETIME de C a la carpeta temporal (no es un guardado del dibujo); ninguna NullReferenceException de `DocumentActivated` |
| **SM-27** | S1-05 (**C2 crítico**) | **NOT_EXECUTED** | | | STOP SM-STOP-02 antes de empezar el paso |
| SM-28 … SM-33 | S1-01, S1-02, S1-03, S1-10, INV-25 | NOT_EXECUTED | | | STOP |
| **SM-34** | S1-07 / OV-17 (**C2 crítico**) | **NOT_EXECUTED** | | | STOP |
| SM-35 | S1-07 | NOT_EXECUTED | | | STOP |
| SM-36 | INV-25 | PASS hasta el STOP | ningún «Error del panel» ni «Sesión degradada» | | observado por la sesión entre SM-00e y el STOP |
| SM-37 | INV-25, INV-14 | PASS hasta el STOP | ningún cuadro de excepción no controlada | | |
| SM-38 | INV-14 | PASS hasta el STOP | (1) sin aserción Debug visible; (2) sin excepción no controlada; (3) sin cierre inesperado; (4) RackLog sin fuga; (5) drenajes y lecturas completados | | |
| **SM-39** | RackLog (**crítico**) | **PASS hasta el STOP** | ninguna entrada nueva: la carpeta sigue con los cuatro archivos del BEFORE (último rackcad-20261103.log, 1720 bytes, 1F9B1E6F…8865); ningún rackcad-20261104.log | entradas nuevas=0; releído tras el POST (00:39Z): sin cambio, sin rackcad-20261105.log | sin NullReferenceException, sin fuga de transacción, sin ERROR; el caso de SM-27 (pestaña Start con C abierto) no se ejercitó |
| SM-40 | cierre sin guardar | PASS (tras el STOP) | pid 46292 cerrada con la X de su ventana (página Start, sin dibujos) y después la pid 9640 con su X; sin diálogos de guardar | acad=0 y RAM=0 a las 22:36:54Z | ningún dibujo guardado |
| SM-41 | dibujos sin cambios | PASS | SHA-256 iguales a los del preflight | A=90750e8b…4852; B=fb9b97f9…682b; C=ede7cdb3…a53c; D=03e5606c…f165; biblioteca=B4CA2248…60E8 | fechas de modificación sin cambio (2026-10-13) |
| HOST-EXTRAS | AssemblyMap, CoreUser, historial de NETLOAD | REPORT_REQUIRED mecánico (código 4); INFORMATIVE por la regla de 0a77 | AssemblyMap idéntico (2 valores, sin RackCad.Plugin, LastWrite 2026-11-02T19:54:39.812Z); CoreUser idéntico (57/57 entradas); clave del producto: «Dialogs\Select File» LASTWRITE_ONLY (→ 22:33:00.077Z) y «Recent File List» LASTWRITE_ONLY (→ 22:36:41.644Z), ambos con valores idénticos | BEFORE f15fa649…b34d; AFTER ccc8cd87…33c3; diff/host-extras-diff.json | ningún cambio de RackCad en AssemblyMap, Applications ni Loaded; solo fechas de escritura del MRU |
| POST / DIFF | S1-11 / INV-21 | CLEAN | POST código 0; DIFF CLEAN, código 0 | POST CanonicalSha256 d651ddf472ddbdcc65d7c45a34b2669fbb1e4698e7773b51586fe79a6a8ac7b2 (archivo be629a64…4bff); added 0, removed 1, changed 1; ALLOWED 2 (R-A-03: FixedProfile\Application Window\Position CHANGED; Variables\VIEWBACKSTATUS REMOVED); REPORT 0; FORBIDDEN 0; INVALID 0; diff 1edd3f56…d7aa | PaletteKeyAcceptance = ACCEPTED; ver la limitación del POST |
| I-52 | recomprobación tras el POST | PASS (sin deriva) | «SIN DERIVA de lo que I-52 registró. No concedo ACK/RELEASE y esto no autoriza P2 H10.» (00:43:29Z) | acad=0; RAM=0; acad 2A75996F… igual; TRUSTEDPATHS 19 / a9ba0fd1… igual; SECURELOAD 1; sin RackCad en Applications, InstalledApplications, Loaded, Applications del perfil, HKLM, AssemblyMap ni ApplicationPlugins; CoreUser RackCad con fechas de julio; P2 tip fb6b5648, 0 discrepancias; rutas <unidad-local>\ siguen sin existir (ACL y H10 no comparables) | disposition/i52-post-run-recheck.txt (literal); petición 00:43:04Z (msg 5112203c) |
| HKCU Applications | registro de carga (LastWrite) | INFORMATIVE propuesto (decide el Coordinator) | dato de I-52: LW de Applications 22:36:25.592Z, dentro de la ventana; lectura de la sesión: Applications\AcMr («AutoCAD Action Commands», LOADER en la carpeta de AutoCAD 2025) reescrita con Commands/Groups/MRU a las 22:36:25.596–.598Z, durante la salida de la pid 46292 | contenido idéntico (el DIFF T3C no tiene entradas en Applications; R-F-05 no se dispara); Loaded e InstalledApplications sin cambio de LW | host-extras/applications-lastwrite-session.txt; I-52 dice que I-64 ya lo clasificó ALLOWED: no es así, la sesión no lo había clasificado; sin medida de LW en 0a77 |

## Limitación del POST (observación de la sesión, para el Coordinator)
- La última instancia en salir fue la pid 9640, la que lanzó la sesión, sin RackCad, iniciada a las 22:20:52Z. La pid 46292 salió
  entre 22:36:22Z y 22:36:36Z; la 9640, entre 22:36:40Z y 22:36:54Z. «Recent File List» se escribió por última vez a las
  **22:36:41.644Z**, dentro de la salida de la 9640.
- La sesión abrió D, C, A y B en la 46292, pero «Recent File List» terminó con los valores exactos de la BEFORE. En 0a77, con una sola
  instancia, esa clave sí cambió de valores. Inferencia de la sesión (no medida): al salir, la 9640 reescribió el estado de interfaz que
  había leído al arrancar y tapó lo que la 46292 escribió al salir.
- Por eso el DIFF CLEAN y el HOST-EXTRAS prueban el **estado final** del host, sin deriva prohibida ni de carga. No prueban por sí solos
  qué escribió la 46292 **al salir** en valores que la 9640 también reescribe. Lo que la 46292 escribiera antes de las 22:20:52Z, como
  un cambio inmediato al cargar, lo habría leído la 9640 al arrancar y habría quedado en el POST.

## Incidentes del operador (sesión)
1. **Segunda acad.exe (causa del STOP).** Al tomar el control (~22:20:40Z), el primer clic falló porque el escritorio estaba al frente.
   La sesión llamó a `open_application("AutoCAD 2025 - English")` y la herramienta lanzó una instancia nueva (pid 9640, 22:20:52Z) en
   vez de enfocar la existente. La sesión no contó procesos entre SM-00d1 (22:18:34Z) y las 22:34:52Z. Toda la operación fue sobre la
   pid 46292, la única con RackCad y la paleta.
2. SM-06: clic de exploración fuera de orden (54ef4061-…) antes de la secuencia en orden; alejamiento con la rueda (solo vista).
3. SM-14a: la barra espaciadora conmutó la casilla con foco, como en 0a77.
4. SM-26: autoguardado SAVETIME de C en la carpeta temporal (.sv$), comportamiento de AutoCAD; los dibujos conservan su SHA (SM-41).

```text
Firma del Owner:          FIRMADO: «Firmo el registro.» (chat de la sesión, 2026-11-05 ~04:47Z; hora medida 04:47:56Z;
                          disposition/owner-signature.md). Firma el registro tal como está; no cambia la propuesta ni decide la disposición.
Propuesta de la sesión:   STOP (SM-STOP-02, causado por la sesión); sin PASS ni NOT PASS: SM-27 y SM-34 (críticos) NOT_EXECUTED
Desviación de rol:        NUEVA: la SESIÓN operó AutoCAD desde SM-00e por orden del Owner; la aceptación de 0a77 no se reutiliza
```
