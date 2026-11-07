# I-64 Smoke-1 (TERCER REINTENTO) — Registro del operador, RunId R20261105T050250Z-785a

AutoCAD operated by implementation session under Coordinator OPERATOR_EXCEPTION for this occurrence.

**Procedimiento:**
- T3D: `docs/initiatives/I-64-f1-t3d-smoke-1-procedure.md`.
- Orden de la autorización del Coordinator del 2026-11-05 (`authorization/coordinator-smoke1-retry-authorization-785a.md`).
- Disposiciones durante la ocurrencia: `disposition/coordinator-disposition-sm07.md` y `disposition/coordinator-disposition-sm14h.md`.

**Resultados por paso:**
- `PASS`, `FAIL` o `NOT MET`;
- `NOT_EXECUTED`, con su motivo;
- `NOT_APPLICABLE`;
- `NOT_APPLICABLE_BY_DESIGN`, solo para S1-06a..e, por I-64-A-7;
- `COMPROMISED`, solo para SM-18, por la disposición.

**Reglas del registro:**
- Sin rutas locales en este archivo.
- Las horas y los hashes los mide la sesión.
- La bitácora contemporánea está en `operator/session-log.md`.

```text
RunId:                    R20261105T050250Z-785a (a578 = STOP CONFIRMED histórico; 0a77 = NOT PASS histórico)
Candidate SHA:            cc7503e3e4e56bff3a2fc9de7ca30a5322b57ea6, reutilizado SIN recompilar. Punta de la rama: b5ea02f5 (custodia);
                          delta cc7503e3..HEAD: src 0, tests 0, proyecto 0 (preflight paso 2)
DLL SHA-256:              3C261B8F156E69192415E0084D0BC8774B3080ACE9D6CB4B9BB0C1760549B6A0
                          (ProductVersion 1.0.0+cc7503e3e4e56bff3a2fc9de7ca30a5322b57ea6)
AUTHORIZED_AUTOCAD_PID:   78512. Inicio 2026-11-05T17:25:16.8462970Z; acad.exe sha256 2A75996F…6422 (R25.0.171.0.0).
                          La abrió la sesión con UNA sola llamada a open_application (process/authorized-autocad-pid.json)
Fin:                      salida normal de la PID 78512 a las 19:47:20Z (acad-watch.log, tras PRE-CLOSE);
                          PG-5 Closed PASS 19:47:32Z; acad.exe = 0 y RAMElements.exe = 0
Guarda de procesos:       REGISTER PASS (17:25:24Z); 20 CHECK PASS de 17:31Z a 19:45Z; PRE-CLOSE PASS (19:45:51Z); CLOSED PASS.
                          Vigilancia continua de 1 s (17:25:43Z a 19:49:30Z): una sola acad.exe todo el tiempo, sin STOP; código 0
Operador:                 AutoCAD operated by implementation session under Coordinator OPERATOR_EXCEPTION for this occurrence.
                          La sesión ejecutó también el preflight, HOST-EXTRAS, BASELINE, POST, DIFF y las lecturas de solo lectura
HOST_RELEASE:             GRANTED FOR I-64 / 785a; I-52 solo mantiene su compromiso de no tocar el host y hace el recheck
PaletteKeyAcceptance:     ACCEPTED (Coordinator)
NETLOAD:                  una sola carga no persistente con «Load Once», solo del DLL autorizado (SM-00d); inspección ligada a la PID
                          tras la carga (SM-00d1) y otra tras cada disposición: las tres VERDICT = PASS
SECURELOAD_BASELINE_CURRENT: 1 (reg query de solo lectura de la sesión). BASELINE MATCH: CanonicalSha256 980ce80f…23dd
                          (archivo 17e81015…1e92)
Antes de la BASELINE:     OWNER_AUTOCAD_ACTIVITY_PRE_BASELINE (~16:00 a 16:57Z); escritura de Applications a las 16:57:18Z =
                          NON-BLOCKING (Coordinator); no se limpió nada
AssemblyMap (BEFORE):     2 valores (AppManagerPlugin, FeaturedAppsPlugin), RackCad.Plugin ABSENT, LastWrite 2026-11-02T19:54:39.812Z
Dibujo D (PR-12, PR-13):  I64-SMOKE1-FIXTURES-D.dxf, SHA-256 03e5606c…f165; nunca se guarda
Dibujos A, B, C:          SHA-256 en preflight/preflight-owner.txt (paso 5) y logs/sm41.txt
```

| Paso | S1 / OV / INV | Resultado | Observado (textos literales del panel y de la línea de órdenes) | Valores | Notas |
|---|---|---|---|---|---|
| SM-00a | arranque | PASS | AutoCAD 2025 abierto por la sesión. Página Start, sin selector de perfil. New → Drawing1 | una sola acad.exe (PID 78512) | PG-1a: acad = 0 antes; PG-1 Register |
| SM-00b | perfil, SECURELOAD, PICKFIRST | PASS | `(getvar "CPROFILE")` → «<<Unnamed Profile>>»; `(getvar "SECURELOAD")` → 1; `(getvar "PICKFIRST")` → 1 | CPROFILE=<<Unnamed Profile>>; SECURELOAD=1; PICKFIRST=1 | coincide con la BASELINE |
| SM-00d | NETLOAD | PASS | diálogo «Choose .NET Assembly» en la carpeta bin del Candidato; aviso «Security - Unsigned Executable File» con las opciones «Always Load», «Load Once» y «Do Not Load» | opción elegida: **Load Once** | la carga terminó sin mensaje de error |
| SM-00d1 | integridad del Candidato | PASS | Inspect-LoadedRackCad ligado a la PID: VERDICT = PASS (17:28:21Z) | un solo RackCad.Plugin.dll, en `<candidate-bin>`, SHA-256 3C261B8F…B6A0; RackCad.Application 12B8E237 MATCH | también PASS a las 18:48:38Z y 19:17:00Z (RackCad.UI 363AE9FE MATCH) |
| SM-00e | abrir el dibujo D | PASS | «PR-13 SIN IDENTIDAD» y «PR-12 DIAGNOSTICO»; aviso de SYSVARMONITOR; «AutoCAD menu utilities loaded.» | abre limpio | lugar History del diálogo y tipo DXF |
| SM-00c | entidades de C | PASS | `(sslength (ssget "_X"))` → 17 | n=17 | |
| SM-01 | S1-10 / esc. 1 y 10 (nunca abierto) | PASS | línea exacta de T3D §6 → «SM-T ms=297»; ningún mensaje de RackCad | ms=297 | |
| SM-02 | S1-08 (lectura previa) | PASS | `(getvar "DBMOD")` → 0 | DBMOD=0 | |
| **SM-03** | RACKPANEL / INV-02 (**C1 crítico**) | **PASS** | Línea de órdenes: «RackCad: panel Workspace mostrado.». Paleta «RackCad», pestaña «Workspace»: «Sesión activa», «El panel sigue el dibujo activo.», «Sesión 1». «Leer el dibujo» y «Escribir en el dibujo» marcados. «Ninguno» / «No hay objetos seleccionados.» | n=1 | FLOTANTE DENTRO DE LA PANTALLA, arriba a la izquierda (x 60–306, y 60–486 en el marco 1456×819); visible y usable; sin rescate; capturas SM-03-* |
| SM-04 | RACKPANEL repetido | PASS | misma paleta, sin segunda; mensaje repetido | | |
| SM-05 | instrumentación | PASS | «Instrumentación activada: 0 puntos de medida.», «Memoria del proceso: 1940.6 MB» | 0 filas | |
| SM-06 (primera ejecución) | S1-04 un rack | PASS (tal como se observó) | «Un rack» / «Rack seleccionado: 54ef4061-d1f3-4191-9400-70fe08bafc0c» | filas 1 selection.read 62.6 ms, 2 drain 63.3 ms | se eligió el rack de la pareja equivocada para SM-07: OPERATOR_SELECTION_INCIDENT (incidente 1) |
| SM-06 (repetido) | S1-04 | PASS — RECOVERY REPEAT AUTHORIZED BY COORDINATOR | `Esc` → «Ninguno»; clic en el rack de arriba a la izquierda → «Un rack» / «Rack seleccionado: 56ec2c3a-ff43-4468-8fb7-2648cf3ac54e» | filas 7 selection.read 5.1 ms, 8 drain 5.1 ms | motivo: wrong fixture member selected for the SM-07 sequence |
| SM-07 | S1-04 copia | PASS | sigue «Un rack», 56ec2c3a-… | filas 9, 10 | evaluado con normalidad tras la recuperación |
| SM-08 | S1-04 dos racks | PASS | «Selección actual: 2 racks y 0 otros objetos.» | fila 11 selection.read 6.3 ms (3) | el rack de abajo (54ef4061) como el de otro RackId |
| SM-09 | S1-04 con otra entidad | PASS | «Selección actual: 2 racks y 1 otros objetos.» | 14 puntos | |
| SM-10a | S1-04 bloque sin datos de RackCad | PASS | «Selección actual: 0 racks y 1 otros objetos.» | 18 puntos | `Esc`; acercamiento con la rueda (solo vista) para apuntar al bloque, que es diminuto |
| SM-10b | S1-04 sin identidad / INV-16 (D) | PASS | «Sesión 2»; «Sin identidad» / «El objeto seleccionado no tiene identidad de rack.»; sin RackId | 22 puntos | cambio de dibujo por el menú Window (la paleta tapa las pestañas) |
| SM-11 | S1-04 diagnóstico / INV-17 (D) | PASS | «Diagnóstico» / «No se pudo interpretar la definición del objeto seleccionado.»; sin RackId | 26 puntos | vuelta a A: «Sesión 1», sin restos de D |
| SM-12 | S1-09 / esc. 2 / INV-01 | PASS | 28 puntos antes y después de 62 s; sin «Regenerating model». Cierre de la paleta con su botón y `RACKPANEL` → «RackCad: panel Workspace mostrado.» | última fila antes de la espera = 28 drain 0.1 ms; filas entre la espera y panel.hide = 0 (29 panel.hide, 30 panel.open, 31 panel.show, 32 cache.invalidate, 33 selection.read, 34 drain) | la tabla se desplaza arrastrando la barra de la paleta, como en a578 |
| **SM-13** | S1-06f (**C2 crítico**) | **PASS** | Con `LINE` activo («LINE Specify first point:»): «Leer el dibujo» y «Escribir en el dibujo» DESMARCADOS y «Hay un comando en curso.». Tras `Esc` («*Cancel*») vuelven a marcarse y el texto desaparece | última fila antes = 34; a ~2 s y ~30 s sigue en 34 (ninguna fila drain/selection.read durante la orden); después: 35 cache.invalidate, 36 drain | capturas SM-13-* con la orden activa; en a578, después de la orden: selection.read y drain |
| S1-06a..e | INV-24 (A-7) | NOT_APPLICABLE_BY_DESIGN | | | por A-7 |
| SM-14a..e | evidencia complementaria (A-7) | PASS | Línea suelta preseleccionada; foco en «Medir en memoria» con `Tab`. Se enviaron `abc def`, `Ctrl+Z`, `Ctrl+C`, `Ctrl+V`, `Supr`, `Esc` y `Enter` → nada en AutoCAD (ni UNDO, COPYCLIP, PASTECLIP ni ERASE); la línea siguió existiendo y seleccionada | 38 puntos, sin filas nuevas | la casilla NO se conmutó (en 0a77 y a578 sí): la herramienta envía el texto como caracteres, no como la tecla barra espaciadora (incidente 4) |
| **SM-14h** | S1-06h | **NOT MET** / OPERATOR_INPUT_INCIDENT | Un clic en la línea de órdenes solo activó la ventana, y `Ctrl+V` lanzó «_pasteclip» / «PASTECLIP Specify insertion point:». `Esc` → «*Cancel*», sin insertar nada. `(getvar "DBMOD")`, escrito con el teclado → 17 | DBMOD=17 (bit 1 = 1; a las 18:51:06Z era 16) | **no es un FAIL del producto** salvo que se demuestre que la escritura la hizo RackCad (disposición); capturas INCIDENTE-SM-14h-*; incidente 2 |
| SM-15 | S1-06g | PASS | Foco en la paleta: «LINE» no llega a la línea de órdenes. En el dibujo: clic en un punto vacío y `Esc`; «LINE» escrito → entrada dinámica; `Enter` → «LINE Specify first point:», con casillas desmarcadas y «Hay un comando en curso.»; `Esc` | 41 → 43 puntos | |
| SM-16 | INV-33, ocultar y mostrar | PASS | 2 racks: «Selección actual: 2 racks y 0 otros objetos.». Cierre de la paleta; `Esc` y otro rack con la paleta cerrada; 5 s; `RACKPANEL` → «Ninguno» / «No hay objetos seleccionados.» | filas con la paleta cerrada = 0 (47 drain → 48 panel.hide); al mostrar: 49 panel.open, 50 panel.show, 51 cache.invalidate, 52 selection.read, 53 drain (una vez) | RACKPANEL vacía la selección implícita, como en 0a77 y a578 |
| **SM-17** | acoplar, flotar, ocultación automática (**C1 crítico**) | **PASS** | Acoplada a la izquierda, acoplada a la derecha y flotante dentro de la pantalla (x 634–873, y 295–716 en el marco). Ocultación automática: se pliega a la franja vertical «RACKCAD» (x≈866), se despliega con un clic y se desactiva. Sin errores | puntos 57, 61, 65, 65, 69 | la sesión la dejó acoplada a la derecha, como a578; capturas SM-17-flotante, SM-17-plegada y SM-17-derecha-* |
| **SM-18** | S1-08, S1-06h, OV-20 | **COMPROMISED_BY_PRIOR_OPERATOR_INCIDENT** | `(getvar "DBMOD")` → 17 | DBMOD=17 (bit 1 = 1 desde el incidente de SM-14h) | conclusión DBMOD: **INCONCLUSIVE FOR DRAWING-WRITE ASSERTION**; no es un PASS |
| SM-19 | S1-12 | NOT_APPLICABLE | PICKFIRST = 1 | | |
| SM-20 | S1-05 / esc. 9 | PASS | B: «Sesión 3», «Ninguno» | m=3 | OPEN escrito; doble clic en Dibujo B |
| SM-21 | S1-05 | PASS | «Un rack» / «Rack seleccionado: a0e7f2be-ef24-4311-8482-1730235f372f» | | |
| SM-22 | S1-05 / INV-07, INV-22 | PASS | A: «Sesión 1», «Ninguno»; sin restos de B | | |
| SM-23 | S1-05 | PASS | B: «Sesión 3», «Ninguno» | | |
| SM-24 | S1-05 / OV-15 | PASS | «Dibujo A.dwg:2 - Read Only» → «Sesión 4». Pestaña del A original → «Sesión 1»; copia → «Sesión 4» | p=4 | Open ▾ → «Open Read-Only»; la herramienta no deja escribir en el campo de nombre: Dibujo A se eligió con un clic en la lista |
| SM-25 | INV-22 | PASS | copia de solo lectura cerrada con la X de su pestaña, sin pregunta | | |
| **SM-26** | sin documento (**C2 crítico**) | **PASS** | «Sin documento» / «Abre o crea un dibujo para trabajar con el panel.» / «Sin sesión activa». «Leer el dibujo» y «Escribir en el dibujo» desmarcados. «No hay un documento abierto.». «Ninguno» | RackLog a las 19:29, 19:31:06 y 19:31:49Z: igual que la BEFORE, sin archivo nuevo | B, D y Drawing1 («Sesión 6») sin pregunta; A y C con «No» (durante cada pregunta: «Hay un comando en curso.»); C activo = «Sesión 5»; ninguna NullReferenceException de `DocumentActivated`; captura SM-26-palette |
| **SM-27** | S1-05 (**C2 crítico**) | **PASS** | (a) C desde Recent de Start → «Sesión 7» (nueva). (b) Pestaña Start → «Sin documento» / «Sin sesión activa», sin la sesión de C. (c) B desde Recent → «Sesión 8» (nueva). (d) Pestaña de C → «Sesión 7» | RackLog a las 19:32:59Z: sin error | captura SM-27b-palette (solo la paleta: la página Start muestra nombres de proyectos ajenos) |
| SM-28 | S1-01, OV-01 | PASS | `LINE`, tres segmentos, y `CIRCLE`: se dibujan con normalidad; la paleta queda abierta | | órdenes escritas con el teclado |
| SM-29 | S1-02, OV-02 | PASS (arrastre central NO_EJECUTABLE) | rueda hacia arriba y hacia abajo; `ZOOM` `E`; `PAN` en tiempo real con el botón izquierdo y `Esc`: funcionan, y la paleta no parpadea ni roba el foco | | la herramienta no puede arrastrar con el botón central |
| SM-30 | S1-03, OV-03 | PASS | `MOVE`, `COPY` («Copy mode = Multiple») y `ERASE` sobre lo dibujado; `UNDO` 3 → «ERASE COPY MOVE» | | UNDO deshace órdenes de AutoCAD |
| SM-31 | S1-10 / esc. 10 (oculto) | PASS | Paleta cerrada (X de la barra acoplada). Línea exacta de T3D §6 → «_ALL 21 found» … «SM-T ms=375» | ms=375 | 21 = 17 + 4 de SM-28; incidente 3 (portapapeles interno de la herramienta, sin ejecutar) |
| SM-32 | S1-10 / esc. 10 (visible) | PASS | `RACKPANEL` → «RackCad: panel Workspace mostrado.» (vuelve acoplada a la derecha); misma línea → «SM-T ms=406» | ms=406 | escrita en 13 trozos como teclas |
| SM-33 | comandos clásicos / INV-25 | PASS | `RACKAYUDA` → «RackCad — comandos y atajos»; «Cerrar» | | mientras está abierta: «Hay un comando en curso.» |
| (requisito de SM-35) | — | B LIMPIO | `(getvar "DBMOD")` en B antes de SM-34 → 0 | DBMOD=0 | B abierto de nuevo en SM-27c; desde entonces solo cambios de pestaña |
| **SM-34** | S1-07 / OV-17 (**C2 crítico**) | **PASS** | `RACKEDITAR` → «Selecciona un rack para editar:» → «RackCad – Rack selectivo: editor avanzado (vista frontal)», «Nombre (Rack A)» = «Selectivo 1». El editor funciona y el panel no interfiere. Durante la orden: casillas DESMARCADAS y «**Hay un comando en curso.**» | 135 puntos durante toda la orden (sin filas drain ni selection.read nuevas) | capturas SM-34-*; corrige el FAIL de 0a77 |
| SM-35 | S1-07 | PASS (**autoritativo en B**) | «Cerrar» en el editor, sin aceptar nunca: la orden termina. El panel vuelve a «Sesión activa», «Sesión 8», casillas marcadas | DBMOD=0 (antes 0): bit 1 sin cambio | |
| SM-35n | nota obligatoria de T3D | — | S1-07 valida la convivencia y la sincronización alrededor del flujo real de RACKEDITAR. La conexión de host de `SetModalRackCadActive` desde los comandos clásicos no se ejercita: la rama modal de INV-23 la cubre la evidencia Core con fuente simulada. En F1 no se reabren los comandos clásicos | | |
| SM-36 | INV-25 | PASS | ningún «Error del panel» ni «Sesión degradada» en toda la secuencia | | |
| SM-37 | INV-25, INV-14 | PASS | ningún cuadro de excepción no controlada | | |
| SM-38 | INV-14 | PASS | (1) sin aserción Debug visible; (2) sin excepción no controlada; (3) sin cierre inesperado; (4) RackLog sin fuga; (5) drenajes y lecturas completados | | |
| **SM-39** | RackLog (**crítico**) | **PASS** | Ventana de la PID 17:25:16Z a 19:47:20Z: la carpeta sigue con los cuatro archivos de la BEFORE (último rackcad-20261103.log, 1720 bytes, 1F9B1E6F…8865); ningún rackcad-20261105.log | entradas nuevas = 0 (lecturas a las 19:29, 19:31, 19:32 y 19:48Z) | sin NullReferenceException de DocumentActivated, sin fuga de transacción, sin ERROR; logs/racklog-checks.txt |
| SM-40 | cierre sin guardar | PASS | PG-4 PRE-CLOSE; B sin pregunta; C con «No»; X de la ventana desde Start → salida normal, sin diálogos | PG-5 Closed PASS 19:47:32Z | ningún dibujo guardado |
| SM-41 | dibujos sin cambios | PASS | SHA-256 iguales a los del preflight; ningún .bak, .sv$ ni .dwl; ningún archivo escrito en las carpetas | A=90750e8b…4852; B=fb9b97f9…682b; C=ede7cdb3…a53c; D=03e5606c…f165; biblioteca=B4CA2248…60E8 | logs/sm41.txt |
| HOST-EXTRAS | AssemblyMap, CoreUser, clave del producto | REPORT_REQUIRED mecánico (código 4); INFORMATIVE propuesto (decide el Coordinator) | AssemblyMap idéntico (2 valores, sin RackCad.Plugin, LastWrite 2026-11-02T19:54:39.812Z); CoreUser idéntico (29 y 28 entradas). Clave del producto: «Dialogs\Select File» LASTWRITE_ONLY (→ 19:27:26.148Z, el diálogo OPEN de SM-24) y «Recent File List» VALUES_CHANGED (→ 19:47:15.455Z, al salir) | BEFORE f9499f0f…0c79; AFTER E8C8D14C…3606; comparación 149F2F04… | la lista de recientes cambia de valores porque la sesión abrió A, B, C y D con una sola instancia, como en 0a77; ningún cambio de RackCad en AssemblyMap, Applications ni Loaded |
| POST / DIFF | S1-11 / INV-21 | CLEAN | POST código 0; DIFF CLEAN, código 0 | POST CanonicalSha256 310f6ac9…8916 (archivo 97737343…2e35); added 0, removed 0, changed 1; ALLOWED 1 (R-A-03: Dialogs\OpenSaveAnavDialogs\InitialDirectory CHANGED); REPORT 0; FORBIDDEN 0; INVALID 0; diff BB5DC356… | PaletteKeyAcceptance = ACCEPTED; una sola instancia de principio a fin, así que esta vez el POST sí refleja la salida de la PID operada |
| HKCU Applications | registro de carga (LastWrite) | INFORMATIVE propuesto (decide el Coordinator) | LastWrite de Applications 19:47:16.440Z: lo reescribe AcMr («AutoCAD Action Commands», LOADER en la carpeta de AutoCAD 2025) con Commands, Groups y MRU (19:47:16.454 a .458Z) durante la salida de la PID 78512; mismo patrón que a578 | Loaded e InstalledApplications sin cambio de LastWrite; ninguna entrada RackCad; el DIFF de T3C no tiene entradas en Applications | host-extras/applications-lastwrite-session.txt; no se limpia nada |
| I-52 | recomprobación tras el POST | PASS (sin deriva) | «SIN DERIVA de lo que I-52 registró. Solo cambió la última escritura de HKCU Applications. No concedo ACK/RELEASE; no autoriza P2 H10.» (19:51:33Z) | acad=0; RAM=0; acad 2A75996F… igual; TRUSTEDPATHS 19 / a9ba0fd1… igual; SECURELOAD 1; Applications: mismo conjunto de claves, LW 19:47:16.440Z (antes 16:57:18.875Z); Loaded e InstalledApplications sin cambio; sin RackCad en Applications del perfil, HKLM, AssemblyMap ni ApplicationPlugins; CoreUser sin cambio; P2 tip fb6b5648, 0 discrepancias | disposition/i52-post-run-recheck.txt (literal; dos rutas locales redactadas); petición ~19:51Z (msg 41693629). I-52 no cubre RackLog, el DIFF/HOST-EXTRAS de T3C ni la clave del producto |

## Incidentes y notas del operador (sesión)
1. **SM-07, OPERATOR_SELECTION_INCIDENT.**
   - En la primera ejecución de SM-06 la sesión eligió el rack 54ef4061, que no tiene copia.
   - Pausa y petición: `disposition/deviation-request-sm07.md`.
   - El Coordinator dio OPTION A: Esc y SM-06 repetido sobre 56ec2c3a; después SM-07 y SM-08 con normalidad.
   - Antes de seguir se confirmó: guarda PASS, DLL igual, ningún guardado y DBMOD 16, solo vista (`disposition/no-save-check-sm07.txt`).
   - La primera ejecución de SM-06 se conserva tal como se registró.
2. **SM-14h, OPERATOR_INPUT_INCIDENT.**
   - `Ctrl+V` se envió cuando la línea de órdenes todavía no tenía el foco de texto, y AutoCAD inició PASTECLIP.
   - Se canceló con `Esc` sin insertar nada. DBMOD pasó de 16 a 17.
   - Pausa: `disposition/incident-sm14h-pasteclip.md`.
   - El Coordinator dio OPTION A:
     - SM-14h = NOT MET;
     - SM-18 = COMPROMISED / INCONCLUSIVE;
     - resultado máximo de la ocurrencia: NOT PASS;
     - sin `Ctrl+V` en adelante.
   - A no se reparó, no se deshizo ni se guardó.
3. **SM-31, el portapapeles de la herramienta.**
   - Para la línea larga de medición, la acción de escribir respondió «Typed (via clipboard)»: la herramienta usó por su cuenta su vía rápida del portapapeles.
   - Eso va contra la letra de la restricción (sin `Ctrl+V`).
   - El texto quedó en la entrada de la línea de órdenes, completo y **sin ejecutar**: sin `Enter`, sin PASTECLIP, sin ningún cambio en el dibujo.
   - Se vació con `Esc` («…*Cancel*») y se volvió a escribir en 13 trozos cortos que la herramienta envió como teclas, comprobando el texto antes de `Enter`.
   - Lo mismo en SM-32. Lo clasifica el Coordinator.
4. **SM-14a.** La barra espaciadora no conmutó la casilla con foco, a diferencia de 0a77 y a578, por cómo envía la herramienta el texto. La expectativa de SM-14a..e, que nada llegue a AutoCAD, se cumplió.
5. **Autoguardados SAVETIME** de AutoCAD en la carpeta temporal (`.sv$`):
   - C a las 17:42:36Z y A a las 17:46:36Z, durante la pausa de SM-07;
   - A a las 19:04:32Z, justo después de cancelar PASTECLIP;
   - C (sesión 7) durante la fase D.
   - Es el mismo comportamiento que en 0a77 y a578: no es un guardado del dibujo. Los cinco SHA no cambian (SM-41) y los `.sv$` desaparecieron con el cierre normal.
6. **Acciones de solo vista** necesarias para los pasos: rueda para alejar (preparación de SM-07), para acercar al bloque diminuto (SM-10a) y la de SM-29.
7. **Diálogo de archivos.** La herramienta no deja escribir en el campo de nombre (SM-24); el dibujo se eligió con un clic en la lista.
8. **Anotaciones del diario fuera de hora:**
   - dos anotaciones se escribieron unos 40 s tarde porque un filtro de la herramienta bloqueó la orden de registro (notas a las 17:29:17Z y 17:36:03Z);
   - dos fe de erratas: 18:55:45Z (título de D) y 19:17:17Z (hora de recepción de la disposición).
9. **Fin de la ocurrencia.** Al cerrarse AutoCAD quedó a la vista una ventana del Explorador de archivos del usuario; la sesión no la tocó ni la capturó.

```text
Firma del Owner:          FIRMADO: «Firmo el registro.» (chat de la sesión; hora medida 2026-11-05T20:41:06Z;
                          disposition/owner-signature.md). Firma el registro tal como está; confirma el registro, no un PASS
                          (única edición posterior: este bloque de firma)
Propuesta de la sesión:   NOT PASS (máximo fijado por la disposición de SM-14h: SM-14h NOT MET y la aserción de escritura de SM-18
                          COMPROMISED / INCONCLUSIVE). Las comprobaciones críticas C1 y C2 dan PASS: SM-03, SM-13, SM-17, SM-26,
                          SM-27, SM-34, SM-35 (autoritativo en B) y SM-39; guarda de procesos, POST y DIFF CLEAN. La sesión no
                          declara PASS
Desviación de rol:        ninguna nueva: operador = sesión por OPERATOR_EXCEPTION, autorizada antes de la ejecución y solo para esta
                          ocurrencia
```
