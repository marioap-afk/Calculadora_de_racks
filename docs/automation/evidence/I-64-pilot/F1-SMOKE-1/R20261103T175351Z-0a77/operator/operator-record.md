# I-64 Smoke-1 (REINTENTO) — Registro del operador, RunId R20261103T175351Z-0a77

Procedimiento: `docs/initiatives/I-64-f1-t3d-smoke-1-procedure.md`, con el orden de la autorización del Coordinator del 2026-11-03
(`authorization/coordinator-smoke1-retry-authorization.md`). Se llena durante la ejecución. Resultado por paso: `PASS`, `FAIL`,
`NOT_EXECUTED` (con motivo), `NOT_APPLICABLE` (solo por decisión del Coordinator) o `NOT_APPLICABLE_BY_DESIGN` (solo S1-06a..e, por
I-64-A-7 AGREED / ACTIVE). Sin rutas locales en este archivo. Las horas y los hashes los mide la sesión.

```text
RunId:                    R20261103T175351Z-0a77 (asignado por la autorización; R20261019T180407Z-1be6 = NOT_STARTED / SUPERSEDED)
Candidate SHA:            09f2f1c3c4e64491018ef1489ce4dd3030d1e4a8 (producto C1 b874705e; 4d79e910 = HISTORICAL_SMOKE1_STOP_CANDIDATE)
DLL SHA-256:              2923E7F2DF4F62594810DBBC33003D36E6E4382EDECFB7E43C4A6C7C01F5AE26
Inicio (UTC, SM-00a):     2026-11-03T20:00:36.589Z (acad.exe pid 41004, inicio 20:00:39.017Z)
Fin (UTC, SM-40):         QUIT ~20:28:35Z; acad.exe = 0 a las 2026-11-03T20:28:57Z
Operador:                 la SESIÓN con computer use, por decisión del Owner (desviación de T3D §1;
                          disposition/owner-decision-session-operator.md); bitácora: operator/session-operator-log.md
Host:                     recheck de I-52 del 2026-11-03 aceptado; I-52 retiene H10 desde la BASELINE hasta la POST y su recomprobación
AssemblyMap:              RackCad.Plugin = ABSENT, aceptado como estado de la BASELINE (origen de la retirada UNKNOWN; no fue una limpieza gobernada)
Entradas RackCad históricas: aceptadas como estado histórico (CoreUser, julio de 2026); capturadas antes y después (HOST-EXTRAS)
Actividad 2-3 nov:        UNKNOWN_HOST_ACTIVITY_PRE_BASELINE (sin atribuir)
PaletteKeyAcceptance:     ACCEPTED (Coordinator; solo estado de paleta o ventana modeless)
Carga única autorizada:   SI, solo para el DLL 2923E7F2...AE26; inspección del proceso tras el NETLOAD (SM-00d1)
I-64-A-7:                 AGREED / ACTIVE (Architect R20261011T202537Z-1990; activacion del Coordinator)
Dibujo Diagnostico:       I64-SMOKE1-FIXTURES-D.dxf (dibujo D), bloque I64_FX_PR12_DIAGNOSTICO; SHA-256 03e5606c1e311f700640125b892fe09bf408dd5642115ebcb433b57474ccf165; esperado: «Diagnóstico» / «No se pudo interpretar la definición del objeto seleccionado.»; nunca se guarda
Dibujo Sin identidad:     I64-SMOKE1-FIXTURES-D.dxf (dibujo D), bloque I64_FX_PR13_SIN_IDENTIDAD; mismo SHA-256; esperado: «Sin identidad» / «El objeto seleccionado no tiene identidad de rack.»; nunca se guarda
Sin cualquiera de los dos dibujos: S1-04 = INCOMPLETE y Smoke-1 no puede ser PASS
SECURELOAD_BASELINE_CURRENT: 1 (leído 2026-11-03T19:48:21Z; BASELINE MATCH; CanonicalSha256 9fbf0c32...a116)
Dibujos A, B, C:          nombres y SHA-256 en preflight/preflight-owner.txt (paso 5)
Orden de arranque:        SM-00a, SM-00b, SM-00d (carga), SM-00d1 (inspección por la sesión), SM-00e (abrir D), SM-00c (abrir C), SM-01...
```

| Paso | S1 / OV / INV | Resultado | Observado (textos literales del panel y de la línea de órdenes) | Valores (sesión, RackId, ms, DBMOD, entidades, filas) | Notas |
|---|---|---|---|---|---|
| SM-00a | arranque | PASS | AutoCAD 2025 abierto desde el acceso «AutoCAD 2025 - English»; pestaña Start → «New» → `Drawing1.dwg` | una sola acad.exe (pid 41004) | sin selector de perfil |
| SM-00b | perfil, SECURELOAD, PICKFIRST | PASS | `(getvar "CPROFILE")` → "<<Unnamed Profile>>"; `(getvar "SECURELOAD")` → 1; `(getvar "PICKFIRST")` → 1 | CPROFILE=<<Unnamed Profile>>; SECURELOAD=1; PICKFIRST=1 | coincide con la BASELINE |
| SM-00c | entidades de C | PASS | `(sslength (ssget "_X"))` → 17 | n=17 | igual que 184b |
| SM-00d | NETLOAD | PASS | «Security - Unsigned Executable File» (Always Load / Load Once / Do Not Load); ubicación = carpeta del Candidato; sin mensajes tras la carga | aviso de seguridad: sí; opción elegida: Load Once | el diálogo abrió en una carpeta ajena (historial); navegación por doble clic hasta `<worktree>\src\RackCad.Plugin\bin\Debug\net8.0-windows`; SHA en disco comprobado antes de Open |
| SM-00d1 | integridad del Candidato (autorización) | PASS | Inspect-LoadedRackCad: VERDICT = PASS (20:04:34Z) | ruta del DLL cargado=`<worktree>\src\RackCad.Plugin\bin\Debug\net8.0-windows\RackCad.Plugin.dll`; SHA-256=2923E7F2…AE26; módulos RackCad.Plugin=1 (más RackCad.Application.dll 66E88EB1… MATCH) | process/inspect-after-netload.txt |
| SM-00e | abrir el dibujo D (primera comprobacion de la autorizacion) | PASS | «Opening an AutoCAD 2013/LT 2013 DXF file.» / «**** System Variable Changed **** 1 of the monitored system variables has changed from the preferred value.» / dos rectángulos rotulados | abre limpio: sí; mensajes al abrir: aviso SysvarMonitor (MIRRTEXT del encabezado DXF; única vigilada guardada en el dibujo) | sin error, auditoría ni recuperación |
| SM-01 | S1-10 / esc. 1 y 10 (nunca abierto) | PASS | «Select objects: _ALL 17 found» … «SM-T ms=203»; ningún mensaje de RackCad | ms=203 | |
| SM-02 | S1-08 (lectura previa) | PASS | `(getvar "DBMOD")` → 0 | DBMOD=0 | |
| SM-03 | RACKPANEL / INV-02 | PASS | «RackCad: panel Workspace mostrado.»; paleta «RACKCAD», «Workspace», «Sesión activa», «El panel sigue el dibujo activo.», «Sesión 1»; casillas marcadas; «Ninguno» / «No hay objetos seleccionados.» | Sesión n=1 | C1: FLOTANTE DENTRO DE LA PANTALLA, arriba a la izquierda (~315×553 px físicos en ~83,83 a 1920×1080 ≈ 80,80,320,560 DIP); visible y usable; no se tocó hasta SM-17 |
| SM-04 | RACKPANEL repetido / INV-02 | PASS | misma paleta, sin segunda; «RackCad: panel Workspace mostrado.» repetido | | |
| SM-05 | instrumentación | PASS | «Instrumentación activada: 0 puntos de medida.», «Memoria del proceso: 1951.7 MB» | filas: 0 | |
| SM-06 | S1-04 un rack / esc. 3 | PASS | «Un rack» / «Rack seleccionado: 56ec2c3a-ff43-4468-8fb7-2648cf3ac54e» (secuencia en orden; antes, rack inferior → «Un rack» 54ef4061-d1f3-4191-9400-70fe08bafc0c) | RackId=56ec2c3a-ff43-4468-8fb7-2648cf3ac54e; primera lectura: drain ms=32.5; selection.read ms=32.2 | hubo exploración fuera de orden para localizar la copia (anotada en la bitácora) |
| SM-07 | S1-04 copia | PASS | sigue «Un rack», mismo RackId 56ec2c3a-… | | |
| SM-08 | S1-04 dos racks | PASS | «Selección actual: 2 racks y 0 otros objetos.» | | |
| SM-09 | S1-04 con otra entidad | PASS | «Selección actual: 2 racks y 1 otros objetos.» | | |
| SM-10a | S1-04 bloque sin datos de RackCad | PASS | «Selección actual: 0 racks y 1 otros objetos.» | | esperado: «Selección actual: 0 racks y 1 otros objetos.» |
| SM-10b | S1-04 sin identidad / INV-16 (dibujo D) | PASS | «Sesión 2»; «Sin identidad» / «El objeto seleccionado no tiene identidad de rack.»; sin RackId | | cambio de dibujo por el menú Window (la paleta tapaba la pestaña) |
| SM-11 | S1-04 diagnóstico / INV-17 (D-T3D-03, obligatorio; dibujo D) | PASS | «Diagnóstico» / «No se pudo interpretar la definición del objeto seleccionado.»; sin RackId | | vuelta a A antes de SM-12 |
| SM-12 | S1-09 / esc. 2 / INV-01 | PASS | 36 puntos antes y después de 60 s; sin «Regenerating model»; × y `RACKPANEL` → «RackCad: panel Workspace mostrado.» | última secuencia antes de la espera=35 selection.read 0.1 ms / 36 drain 0.1 ms; filas entre la espera y panel.hide=0 (37 panel.hide, 38 panel.open, 39 panel.show, 40 cache.inva…, 41 selection.read, 42 drain) | |
| SM-13 | S1-06f | **FAIL** | con `LINE` activo («LINE Specify first point:») la paleta siguió con «Leer el dibujo» y «Escribir en el dibujo» MARCADOS y sin «Hay un comando en curso.»; tras `Esc` siguen marcadas (nunca se desmarcaron) | | causa en código: `CommandWillStart` solo pone `Quiescent=false` sin avisar a la vista; repetido en SM-15 y SM-34 |
| S1-06a..e | INV-24 (A-7) | NOT_APPLICABLE_BY_DESIGN | | | por A-7; nunca PASS; obligacion vinculante en S2-16..S2-22 |
| SM-14a..e | evidencia complementaria (A-7) | PASS | con foco en «Medir en memoria»: `abc def`, `Ctrl+Z`, `Ctrl+C`/`Ctrl+V`, `Supr` (línea seleccionada), `Esc`, `Enter` → nada en AutoCAD (ni UNDO, COPYCLIP, PASTECLIP, ERASE) | | SUPPLEMENTARY_EVIDENCE; la barra espaciadora conmutó la casilla con foco (instrumentación desactivada; re-activada antes de SM-16); incidente del operador posterior (PASTECLIP de la herramienta, cancelado) en la bitácora |
| SM-14h | S1-06h | PASS | `(getvar "DBMOD")` → 16 | DBMOD=16 (bit 1 = 0; bit 16 = vista) | |
| SM-15 | S1-06g | PASS | foco en la paleta: `LINE` no llega a la línea de órdenes; en el dibujo: «LINE Specify first point:»; `Esc` | | |
| SM-16 | INV-33, ocultar y mostrar | PASS | 2 racks seleccionados; × ; cambio de selección; `RACKPANEL` → contexto actual «Ninguno» (RACKPANEL vacía la selección implícita) | filas con la paleta cerrada=0 (51 panel.hide → 52 panel.open); filas al mostrar=panel.open, panel.show, cache.inva…, selection.read ×1, drain ×1 | |
| SM-17 | acoplar, flotar, ocultación automática | PASS | acoplada izquierda, acoplada derecha, flotante (centro, dentro de la pantalla); ocultación automática: se pliega a la barra de título vertical, se despliega con clic; sin errores | filas panel.hide/panel.show/selection.read/drain por cada cambio (57–71) | HOST-TO-CONFIRM; el ratón sintético no despliega por paso; la sesión la dejó acoplada a la derecha |
| SM-18 | S1-08, S1-06h, OV-20 | PASS | `(getvar "DBMOD")` → 16 | DBMOD=16 (bit 1 = 0, igual que SM-02) | |
| SM-19 | S1-12 | NOT_APPLICABLE | PICKFIRST = 1 | | |
| SM-20 | S1-05 / esc. 9 | PASS | B: «Sesión 3», «Ninguno» | Sesión m=3; drain ms=(ver tabla de la paleta) | |
| SM-21 | S1-05 | PASS | «Un rack» / «Rack seleccionado: a0e7f2be-ef24-4311-8482-1730235f372f» | RackId B=a0e7f2be-ef24-4311-8482-1730235f372f | |
| SM-22 | S1-05 / INV-07, INV-22 | PASS | A: «Sesión 1», «Ninguno»; sin restos de B | | |
| SM-23 | S1-05 | PASS | B: «Sesión 3», «Ninguno» | | la selección implícita no sobrevive al cambio de dibujo |
| SM-24 | S1-05 / OV-15 | PASS | «Dibujo A.dwg:2 - Read Only» → «Sesión 4»; A original → «Sesión 1» | Sesión p=4 | no se mezclan |
| SM-25 | INV-22 | PASS | copia de solo lectura cerrada sin pregunta | | |
| SM-26 | sin documento | PASS | «Sin documento» / «Abre o crea un dibujo para trabajar con el panel.» / «Sin sesión activa»; casillas desmarcadas; «No hay un documento abierto.» | | A y C cerrados con «No»; autoguardado SAVETIME de C a %TEMP% observado (no es un guardado del dibujo); 1 ERROR contenido en RackLog al cerrar C (20:24:21Z, ver SM-39) [fe de erratas antes de la firma: antes decía «2 ERROR en esta fase»; el segundo es de SM-27] |
| SM-27 | S1-05 | PASS | C → «Sesión activa», «Sesión 7»; B → «Sesión activa», «Sesión 8» | | B se abrió desde la página Start: al pulsar la pestaña Start (con C abierto) RackLog registró el ERROR de 20:24:40Z (ver SM-39) y el panel siguió mostrando «Sesión 7» hasta abrir B |
| SM-28 | S1-01 / OV-01 | PASS | `LINE` de 3 segmentos y `CIRCLE` normales | | |
| SM-29 | S1-02 / OV-02 | PASS (parcial) | rueda, `ZOOM` `E`, `PAN` normales | | encuadre con botón central: NOT_EXECUTED (límite de la herramienta) |
| SM-30 | S1-03 / OV-03 | PASS | `MOVE`, `COPY`, `ERASE` (1 found cada una); `UNDO` `3` → «ERASE COPY MOVE» | | |
| SM-31 | S1-10 / esc. 10 (oculto) | PASS | «_ALL 21 found» … «SM-T ms=250» | ms=250 | 21 = 17 + 4 de SM-28 |
| SM-32 | S1-10 / esc. 10 (visible) | PASS | «RackCad: panel Workspace mostrado.»; «_ALL 21 found» … «SM-T ms=281» | ms=281 | |
| SM-33 | comandos clásicos / INV-25 | PASS | `RACKAYUDA` → «RackCad — comandos y atajos»; «Cerrar» | | |
| SM-34 | S1-07 / OV-17 | **FAIL** | `RACKEDITAR` → «Selecciona un rack para editar:» → «RackCad – Rack selectivo: editor avanzado (vista frontal)», «Selectivo 1»; el panel no interfiere, pero durante toda la orden muestra «Sesión activa», casillas marcadas y NINGÚN «Hay un comando en curso.» | | misma causa que SM-13 |
| SM-35 | S1-07 | PASS | «Cerrar» (nunca aceptar); la orden termina | DBMOD=0 | |
| SM-35n | S1-07 / INV-23 (D-T3D-06) | | | | SetModalRackCadActive no ejercitado; rama modal de INV-23 cubierta por Core |
| SM-36 | INV-25 | PASS | ningún «Error del panel» ni «Sesión degradada» | | |
| SM-37 | INV-25, INV-14 | PASS | ningún cuadro de excepción no controlada | | |
| SM-38 | INV-14 (D-T3D-04) | PASS (1)(2)(3)(5); (4) ver SM-39 | (1) sin aserción Debug visible; (2) sin excepción no controlada; (3) sin cierre inesperado; (4) RackLog sin fuga; (5) drenajes y lecturas completados | (4): RackLog sin mensajes de fuga de transacción; tiene 2 ERROR contenidos (SM-39) | |
| SM-39 | RackLog | **FAIL** | 2 entradas nuevas: [20:24:21.278Z] y [20:24:40.789Z] «ERROR AutoCadEventBridge.DocumentActivated: System.NullReferenceException: Object reference not set to an instance of an object.» en `RackCad.Plugin.Workspace.AutoCadEventBridge.NativeId(Document)` (AutoCadEventBridge.cs:251) ← lambda de `Hook` (línea 123) ← `Contain` (línea 279) | entradas nuevas=2 (ambas con «Workspace»); SHA-256 del archivo=1F9B1E6FF4466C98AB425EF5AB5590A79891F33D712F165E3D5C4733D82D8865 (1720 bytes) | 20:24:21Z = SM-26 (cerrar C, el último dibujo) y 20:24:40Z = SM-27 (clic en la pestaña Start con C abierto) [fe de erratas antes de la firma: antes decía «ambas durante SM-26»]: `DocumentActivated` llega con documento nulo (pestaña Start) y `NativeId` no lo admite; contenidas; en SM-27 el panel siguió mostrando «Sesión 7» en la página Start hasta abrir B; ninguna fuga; ningún «A workspace drain left a transaction open.» ni «The selection read left a transaction open.» |
| SM-40 | cierre sin guardar | PASS | `QUIT` → «Save changes to …Dibujo C.dwg?» → **No**; B cerró sin pregunta | acad=0 (20:28:57Z) | |
| SM-41 | dibujos sin cambios | PASS | SHA-256 iguales a los del preflight | A=90750e8b…; B=fb9b97f9…; C=ede7cdb3…; D=03e5606c…; biblioteca=B4CA2248… | fechas de modificación sin cambios |
| HOST-EXTRAS | AssemblyMap y CoreUser (autorización) | REPORT_REQUIRED (sin STOP) | AssemblyMap idéntico (2 valores, sin RackCad.Plugin, LastWrite 2026-11-02T19:54:39.812Z); CoreUser idéntico (28+28); clave de producto: NetLoadDialog (INFORMATIVE), «Dialogs\Select File» ADDED (MRU del diálogo Abrir) y «Recent File List» VALUES_CHANGED (MRU) | BEFORE sha=c858f955…1a6f; AFTER sha=4709100e…4514; Disposition=REPORT_REQUIRED (diff 88e27c92…) | ningún cambio de RackCad en AssemblyMap/Applications/Loaded; los dos REPORT son estado de interfaz de AutoCAD por abrir los dibujos |
| POST / DIFF | S1-11 / INV-21 | PASS | POST código 0; DIFF CLEAN | Disposition=CLEAN; código=0 (added 6, changed 17, removed 0; ALLOWED 23: 1 R-A-01 + 22 R-A-03; REPORT 0; FORBIDDEN 0; INVALID 0); POST CanonicalSha256=0a230e7e…7d16 | PaletteKeyAcceptance = ACCEPTED |
| I-52 | recomprobación tras el POST | PASS (sin deriva) | «sin deriva de lo registrado por I-52; no concedo ACK/RELEASE» … «libero mi retención sobre H10 respecto de tu ventana» | acad=0; RAM=0; TRUSTEDPATHS 19 / a9ba0fd1…; SECURELOAD 1; AssemblyMap sin RackCad.Plugin; ACL idénticas | disposition/i52-post-run-recheck.txt; P2 H10 sigue NOT_AUTHORIZED |

```text
Firma del Owner:          «Firmo el registro.» (por el chat de la sesión responsable)     Fecha (UTC): 2026-11-04T15:06:53Z
Alcance de la firma:      confirma el registro, no un PASS (revisión del Coordinator del 2026-11-03)
Disposición vigente:      NOT PASS (SM-13, SM-34, SM-39 = FAIL); ningún resultado cambia con la firma
Desviación de rol:        la SESIÓN operó AutoCAD (T3D §1 asigna la operación al Owner); ACCEPTED AS PROCEDURAL DEVIATION, se preserva
```
