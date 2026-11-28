# I-64 Smoke-1 — Registro del operador, RunId R20261107T171052Z-6862

AutoCAD operated by implementation session under Coordinator exception for this occurrence, except the SM-29 middle-button drag (OWNER_MANUAL_GESTURE_EXCEPTION — SM-29 only).

**Procedimiento:**
- T3D: `docs/initiatives/I-64-f1-t3d-smoke-1-procedure.md`.
- Autorización: `authorization/coordinator-full-smoke1-authorization-6862.md`.
- Pausa durante la ocurrencia: `disposition/deviation-request-sm29.md` y `disposition/disposition-sm29-option-a.md`.

**Resultados por paso:** `PASS`, `FAIL`, `NOT_EXECUTED` (con motivo), `NOT_APPLICABLE` y `NOT_APPLICABLE_BY_DESIGN` (solo S1-06a..e, por I-64-A-7).

**Reglas del registro:** sin rutas locales en este archivo; las horas y los hashes los mide la sesión; la bitácora contemporánea está en `operator/session-log.md`.

```text
RunId:                    R20261107T171052Z-6862 (785a = NOT PASS / CLOSED histórico)
Candidate SHA:            cc7503e3e4e56bff3a2fc9de7ca30a5322b57ea6, reutilizado SIN recompilar. Punta de la rama: 1917bc19 (custodia);
                          delta cc7503e3..HEAD: src 0, tests 0, proyecto 0, fuera de docs/automation 0 (preflight paso 2)
DLL SHA-256:              3C261B8F156E69192415E0084D0BC8774B3080ACE9D6CB4B9BB0C1760549B6A0
                          (ProductVersion 1.0.0+cc7503e3e4e56bff3a2fc9de7ca30a5322b57ea6)
AUTHORIZED_AUTOCAD_PID:   67260. Inicio 2026-11-07T18:05:46.9023864Z; acad.exe sha256 2A75996F…6422 (R25.0.171.0.0).
                          La abrió la sesión con UNA sola llamada a open_application (process/authorized-autocad-pid.json)
Fin:                      salida normal de la PID 67260 a las 19:05:02Z (acad-watch.log, tras PRE-CLOSE);
                          PG-5 Closed PASS 19:05:16Z; acad.exe = 0 y RAMElements.exe = 0
Guarda de procesos:       REGISTER PASS 18:06:01Z; puntos de control PASS en cada punto crítico y tras cada cambio de foco
                          (recuento en disposition/return-package.md); PRE-CLOSE PASS 19:04:21Z; CLOSED PASS.
                          Vigilancia continua de 1 s, 18:06:12Z a 19:06:25Z: una sola acad.exe, sin STOP; código 0.
                          La única coincidencia con «STOP» en la bitácora es el texto de su última línea, «WATCH fin por watch-stop.flag»
Operador:                 implementation session; el Owner hizo solo el gesto de SM-29 (ver el incidente 1)
HOST_RELEASE:             GRANTED FOR I-64 / 6862; I-52 no da ACK/RELEASE
PaletteKeyAcceptance:     NOT_RECORDED: la autorización no lo nombra (T3D §8). Sin efecto: el DIFF no tiene ninguna entrada de la clave de la paleta
NETLOAD:                  una sola carga no persistente con «Load Once», solo del DLL autorizado (SM-00d); inspección ligada a la PID
                          tras la carga (SM-00d1) y otra en la pausa de SM-29: las dos VERDICT = PASS
SECURELOAD_BASELINE_CURRENT: 1 (reg query de solo lectura en el preflight, ejecutado por la sesión). BASELINE lo lee por su cuenta: MATCH.
BASELINE:                 CanonicalSha256 411b97fd…76c2 (archivo E5215177…7E51)
POST:                     CanonicalSha256 353087a9…c523 (archivo 6485685B…C8AC); código 0
Antes de la BASELINE:     actividad de AutoCAD del Owner (PID 16088 y una apertura breve), clasificada en la autorización como estado de host previo
AssemblyMap (BEFORE):     2 valores (AppManagerPlugin, FeaturedAppsPlugin), RackCad.Plugin ABSENT, LastWrite 2026-11-02T19:54:39.812Z
Dibujos:                  SHA-256 en preflight/preflight-owner.txt (paso 5) y logs/sm41.txt; D (PR-12, PR-13) = I64-SMOKE1-FIXTURES-D.dxf
Stage A:                  preflight/stage-a-* (encontró ChatGPT en el monitor principal; corregido por el Owner) y stage-a2-* (PASS, 18:05:19Z)
Stage B:                  PASS en SM-03 (captura real de la paleta, abierta al momento)
```

## Lecturas de DBMOD en el dibujo A (bit 1 = dibujo modificado; 16 = vista)

| Id | Momento | DBMOD | Bit 1 |
|---|---|---|---|
| A-1 = SM-02 | A recién abierto | 0 | 0 |
| A-2 | tras alejar con la rueda, antes de SM-06 | 16 | 0 |
| A-3 | tras SM-12 | 16 | 0 |
| A-4 | tras SM-13 y la preselección, antes de SM-14a | 16 | 0 |
| A-5 | después de SM-14e | 16 | 0 |
| A-6 | inmediatamente antes de SM-14h | 16 | 0 |
| **SM-14h** | aserción de T3D | **16** | **0** |
| A-7 | inmediatamente después de SM-14h | 16 | 0 |
| A-8 | tras SM-15 | 16 | 0 |
| A-9 | tras SM-16 | 16 | 0 |
| A-10 | antes de SM-18, tras SM-17 | 16 | 0 |
| **SM-18** | aserción autoritativa | **16** | **0** |
| A-11 | después de SM-18 | 16 | 0 |

## Resultados

| Paso | S1 / OV / INV | Resultado | Observado (textos literales del panel y de la línea de órdenes) | Valores | Notas |
|---|---|---|---|---|---|
| SM-00a | arranque | PASS | AutoCAD 2025 abierto por la sesión en el monitor principal. Página Start, sin selector de perfil ni administrador de recuperación. New → Drawing1 | una sola acad.exe (PID 67260) | PG-1a: acad = 0 antes; PG-1 Register |
| SM-00b | perfil, SECURELOAD, PICKFIRST | PASS | «<<Unnamed Profile>>»; 1; 1 | CPROFILE=<<Unnamed Profile>>; SECURELOAD=1; PICKFIRST=1 | coincide con la BASELINE |
| SM-00d | NETLOAD | PASS | diálogo «Choose .NET Assembly» en la carpeta bin del Candidato; aviso «Security - Unsigned Executable File» con «Always Load», «Load Once» y «Do Not Load» | elegida: **Load Once** | sin mensaje de error |
| SM-00d1 | integridad del Candidato | PASS | Inspect-LoadedRackCad ligado a la PID: VERDICT = PASS (18:08:15Z) | un solo RackCad.Plugin.dll, en `<candidate-bin>`, 3C261B8F…B6A0; Application 12B8E237 MATCH | también PASS a las 18:47:08Z (UI 363AE9FE MATCH) |
| SM-00e | abrir D | PASS | «PR-13 SIN IDENTIDAD» y «PR-12 DIAGNOSTICO»; aviso de SYSVARMONITOR; «AutoCAD menu utilities loaded.» | abre limpio | |
| SM-00c | entidades de C | PASS | `(sslength (ssget "_X"))` → 17 | n=17 | |
| SM-01 | S1-10 / esc. 1 y 10 (nunca abierto) | PASS | línea exacta de T3D §6 → «SM-T ms=188»; ningún mensaje de RackCad | ms=188 | |
| SM-02 | S1-08 (lectura previa) | PASS | `(getvar "DBMOD")` → 0 | DBMOD=0 | A-1 |
| **SM-03** | S1-01, INV-02 (**C1 crítico**) | **PASS** | «RackCad: panel Workspace mostrado.». Paleta «RackCad», pestaña «Workspace»: «Sesión activa», «El panel sigue el dibujo activo.», «Sesión 1». «Leer el dibujo» y «Escribir en el dibujo» marcados. «Ninguno» / «No hay objetos seleccionados.» | n=1 | FLOTANTE DENTRO DE LA PANTALLA (marco x 62–302, y 62–484), visible y usable, sin rescate. **Stage B PASS.** Capturas SM-03-palette (60D5AC99), SM-03-cmdline (1C44A77E) |
| SM-04 | INV-02 | PASS | misma paleta, sin segunda; mensaje repetido | | |
| SM-05 | instrumentación | PASS | «Instrumentación activada: 0 puntos de medida.», «Memoria del proceso: 1931.3 MB» | 0 filas | |
| SM-06 | S1-04 | PASS | «Un rack» / «Rack seleccionado: 56ec2c3a-ff43-4468-8fb7-2648cf3ac54e» | filas 1 selection.read 27.6 ms, 2 drain 27.8 ms | rack correcto a la primera; solo vista: rueda para alejar |
| SM-07 | S1-04 copia | PASS | sigue «Un rack», mismo RackId | filas 3, 4 | |
| SM-08 | S1-04, OV-14 | PASS | «Selección actual: 2 racks y 0 otros objetos.» | filas 5, 6 | rack de abajo 54ef4061 |
| SM-09 | S1-04, OV-14 | PASS | «Selección actual: 2 racks y 1 otros objetos.» | filas 7, 8 | |
| SM-10a | S1-04 | PASS | «Selección actual: 0 racks y 1 otros objetos.» | 12 puntos | rueda para acercar (solo vista) |
| SM-10b | S1-04, INV-16 (D) | PASS | «Sesión 2»; «Sin identidad» / «El objeto seleccionado no tiene identidad de rack.»; sin RackId | 16 puntos | |
| SM-11 | S1-04, INV-17 (D) | PASS | «Diagnóstico» / «No se pudo interpretar la definición del objeto seleccionado.»; sin RackId | 20 puntos | vuelta a A: «Sesión 1», sin restos de D |
| SM-12 | S1-09, esc. 2, INV-01 | PASS | 22 puntos antes y después de unos 62 s; sin «Regenerating model». Cierre y `RACKPANEL` → «RackCad: panel Workspace mostrado.» | última fila 22 drain 0.1 ms; filas entre la espera y panel.hide = 0 (23 panel.hide, 24 panel.open, 25 panel.show, 26 cache.invalidate, 27 selection.read, 28 drain) | capturas SM-12-* |
| **SM-13** | S1-06f (**C2 crítico**) | **PASS** | Con «LINE Specify first point:»: casillas DESMARCADAS y «Hay un comando en curso.». Tras `Esc` («*Cancel*») vuelven a marcarse y el texto desaparece | 28 puntos durante unos 15 s, sin filas `drain` ni `selection.read`; después 29 cache.invalidate, 30 drain | capturas SM-13-palette (C2F1E2F6), SM-13-cmdline (CB3C36A9) |
| S1-06a..e | INV-24 (A-7) | NOT_APPLICABLE_BY_DESIGN | | | por A-7 |
| SM-14a | complementaria (A-7) | PASS | Recuadro de foco visible en «Medir en memoria»; teclas reales `a b c espacio d e f`: nada en la línea de órdenes; la barra espaciadora desmarcó la casilla | filas ocultas | captura SM-14a-foco-* |
| SM-14b | complementaria | PASS | `Ctrl+Z`: sin UNDO; el dibujo no cambia | | |
| SM-14c | complementaria | PASS | Foco visible y anotado (SM-14c-foco-*, abiertas antes de enviar). `Ctrl+C`. `Ctrl+V` **una vez**: la línea de órdenes no reacciona; sin COPYCLIP ni PASTECLIP | | SM-14c-tras-* |
| SM-14d | complementaria | PASS | `Supr`: la línea suelta sigue existiendo y seleccionada | | |
| SM-14e | complementaria | PASS | `Esc` y `Enter`: no se cancela ni se repite ninguna orden | | |
| **SM-14h** | S1-06h | **PASS** | `(getvar "DBMOD")` → 16 | DBMOD=16; bit 1 = 0 | capturas SM-14h-cmdline (0618C785), SM-14h-palette (8E1FBC6D); A-5, A-6, A-7 |
| SM-15 | S1-06g | PASS | Foco en la paleta: teclas reales `L I N E` no llegan a la línea de órdenes. En el dibujo: clic en un punto vacío y `Esc`; LINE y `Enter` → «LINE Specify first point:», casillas desmarcadas; `Esc` | 38 puntos | el clic en la casilla la volvió a marcar y las filas se conservaron; coincidió un autoguardado SAVETIME (incidente 3) |
| SM-16 | INV-33 | PASS | «Selección actual: 2 racks y 0 otros objetos.». Paleta cerrada; `Esc` y otro rack; 5 s; `RACKPANEL` → «Ninguno» | filas con la paleta cerrada = 0 (42 drain → 43 panel.hide); al mostrar: 44 panel.open, 45 panel.show, 46 cache.invalidate, 47 selection.read, 48 drain (una vez) | RACKPANEL vacía la selección implícita, como en 785a; captura SM-16-filas |
| **SM-17** | D-01 (**C1 crítico**) | **PASS** | Acoplada a la izquierda, acoplada a la derecha y flotante dentro de la pantalla (marco x 645–885, y 360–780). Ocultación automática: se pliega a la franja «RACKCAD» (x≈876), se despliega con un clic y se desactiva. Sin errores | 52, 56, 60, 64 puntos | capturas SM-17-izquierda, -derecha, -flotante, -plegada, -desplegada, -derecha-final, todas abiertas y limpias |
| **SM-18** | S1-08, S1-06h, OV-20 | **PASS** | `(getvar "DBMOD")` → 16 | DBMOD=16; **bit 1 = 0, igual que en SM-02** | capturas SM-18-cmdline (DE705695), SM-18-palette (6BF706D9); A-10, A-11 |
| SM-19 | S1-12 | NOT_APPLICABLE | PICKFIRST = 1 | | |
| SM-20 | S1-05, esc. 9 | PASS | B: «Sesión 3», «Ninguno» | m=3 | |
| SM-21 | S1-05 | PASS | «Un rack» / «Rack seleccionado: a0e7f2be-ef24-4311-8482-1730235f372f» | | |
| SM-22 | S1-05, INV-07, INV-22 | PASS | A: «Sesión 1», «Ninguno»; sin restos de B | | |
| SM-23 | S1-05 | PASS | B: «Sesión 3», «Ninguno» | | |
| SM-24 | S1-05, OV-15 | PASS | «Dibujo A.dwg:2 - Read Only» → «Sesión 4». Rack en la copia → «Un rack», 54ef4061-…. A original → «Sesión 1», «Ninguno». Copia → «Sesión 4» | p=4 | |
| SM-25 | INV-22 | PASS | copia cerrada sin pregunta; B activo | | |
| **SM-26** | sin documento (**C2 crítico**) | **PASS** | «Sin documento» / «Abre o crea un dibujo para trabajar con el panel.» / «Sin sesión activa». Casillas desmarcadas. «No hay un documento abierto.». «Ninguno» | 96 puntos; RackLog igual que BEFORE | **captura autoritativa SM-26-palette (22109588)**, abierta y limpia; A y C con «No» |
| **SM-27** | S1-05 (**C2 crítico**) | **PASS** | (a) C desde Recent → «Sesión 7». (b) Pestaña Start → «Sin documento» / «Sin sesión activa», sin la sesión de C. (c) B desde Recent → «Sesión 8». (d) Pestaña de C → «Sesión 7» | RackLog 18:34:07Z sin error | **captura autoritativa SM-27b-palette (FEED48C2)**, abierta y limpia |
| SM-28 | S1-01, OV-01 | PASS | `LINE` y `CIRCLE`: se dibujan con normalidad; la paleta queda abierta | | |
| **SM-29** | S1-02, OV-02 | **PASS propuesto; lo decide el Coordinator** | Rueda, `ZOOM` `E` y `PAN`: funcionan (la sesión). **Arrastre central físico del Owner, repetición limpia:** V2 (3596.49 1229.31 0.0), VIEWSIZE 1649.97 → V3 (4467.18 1450.59 0.0), VIEWSIZE 1649.97: encuadre sin zoom; historial sin líneas nuevas; 117 puntos, sin filas. Owner: «La vista siguió al ratón», «Paleta sin parpadeo» | ver el incidente 1 | OWNER_MANUAL_GESTURE_EXCEPTION; capturas SM-29-* |
| SM-30 | S1-03, OV-03 | PASS | `MOVE`, `COPY` y `ERASE`; `UNDO` 3 → «ERASE COPY MOVE» | | |
| SM-31 | S1-10, esc. 10 (oculto) | PASS | paleta cerrada; línea de T3D §6 → «SM-T ms=234» | ms=234 | 13 trozos, ninguno por portapapeles |
| SM-32 | S1-10, esc. 10 (visible) | PASS | `RACKPANEL`, misma línea → «SM-T ms=234» | ms=234 | |
| SM-33 | INV-25 | PASS | `RACKAYUDA` → «RackCad — comandos y atajos»; «Cerrar» | | |
| (requisito de SM-35) | — | B LIMPIO | `(getvar "DBMOD")` en B antes de SM-34 → 0 | DBMOD=0 | |
| **SM-34** | S1-07, OV-17 (**C2 crítico**) | **PASS** | `RACKEDITAR` → editor «Rack selectivo», «Selectivo 1». Durante la orden: casillas DESMARCADAS y «**Hay un comando en curso.**». El panel no interfiere | 142 puntos toda la orden | **captura autoritativa SM-34-palette (BBA9F621)**, abierta y limpia; SM-34-cmdline (06A16451) tapada en parte por el editor, con el prompt legible |
| SM-35 | S1-07 | PASS (**autoritativo en B**) | «Cerrar», sin aceptar. Panel: «Sesión activa», «Sesión 8», casillas marcadas | DBMOD=0 (antes 0): bit 1 sin cambio | capturas SM-35-* |
| SM-35n | nota obligatoria de T3D | — | S1-07 valida la convivencia y la sincronización alrededor del flujo real de RACKEDITAR. La conexión de host de `SetModalRackCadActive` desde los comandos clásicos no se ejercita: la rama modal de INV-23 la cubre la evidencia Core con fuente simulada. En F1 no se reabren los comandos clásicos | | |
| SM-36 | INV-25 | PASS | ningún «Error del panel» ni «Sesión degradada» | | |
| SM-37 | INV-25, INV-14 | PASS | ningún cuadro de excepción no controlada | | |
| SM-38 | INV-14 | PASS | (1) sin aserción Debug visible; (2) sin excepción no controlada; (3) sin cierre inesperado; (4) RackLog sin fuga; (5) drenajes y lecturas completados | | |
| **SM-39** | RackLog (**crítico**) | **PASS** | Ventana de la PID 18:05:46Z a 19:05:02Z: los cuatro archivos iguales que BEFORE (último rackcad-20261103.log, 1F9B1E6F…8865); ningún archivo nuevo | entradas nuevas = 0 | sin NullReferenceException de DocumentActivated, sin fuga de transacción, sin ERROR; logs/racklog-checks.txt |
| SM-40 | cierre sin guardar | PASS | PG-4; B sin pregunta; C con «No»; X de la ventana desde Start → salida normal, sin diálogos | PG-5 Closed PASS 19:05:16Z | ningún dibujo guardado |
| SM-41 | dibujos sin cambios | PASS | SHA-256 iguales a los del preflight; ningún .bak, .sv$ ni .dwl; ningún archivo escrito en las carpetas | A=90750E8B…4852; B=FB9B97F9…682B; C=EDE7CDB3…A53C; D=03E5606C…F165; biblioteca=B4CA2248…60E8 | logs/sm41.txt |
| HOST-EXTRAS | AssemblyMap, CoreUser, clave del producto | REPORT_REQUIRED mecánico (código 4); INFORMATIVE propuesto (decide el Coordinator) | AssemblyMap y CoreUser idénticos (29 y 28 entradas). Clave del producto: «Dialogs\Select File» LASTWRITE_ONLY (→ 18:30:54.241Z, el diálogo OPEN de SM-24) y «Recent File List» VALUES_CHANGED (→ 19:04:59.266Z, al salir) | BEFORE 9A0899E8…F049; AFTER 3A3CC659…4C03; comparación 14765962…1C26D | mismo tipo que en 785a |
| POST / DIFF | S1-11, INV-21 | **CLEAN** | POST código 0; DIFF CLEAN, código 0 | added 1, removed 0, changed 2; ALLOWED 3 (R-A-03: Application Window\Position; OpenSaveAnavDialogs\InitialDirectory; Variables\VIEWBACKSTATUS); REPORT 0; FORBIDDEN 0; INVALID 0; diff B2400A71…C1E8 | PaletteKeyAcceptance = NOT_RECORDED, sin efecto |
| HKCU Applications | registro de carga | INFORMATIVE propuesto (decide el Coordinator) | LastWrite 19:04:59.935Z, durante la salida | Loaded e InstalledApplications sin cambio de LastWrite; ninguna entrada RackCad; el DIFF no tiene entradas en Applications | host-extras/after-read-session.txt; no se limpia nada |
| I-52 | recomprobación tras la POST | PASS (sin deriva en lo que registra) | Lectura de 19:07:21Z: acad = 0, RAM = 0; perfil, acad.exe, TRUSTEDPATHS y SECURELOAD iguales; Applications LW 19:04:59.935Z, mismas 7 claves; AssemblyMap sin RackCad.Plugin; P2 sin diferencias; «Operaciones de host de I-52 durante la ventana: ninguna.» | acad 2A75996F… igual; TRUSTEDPATHS 19 / a9ba0fd1… igual; SECURELOAD 1 | disposition/i52-post-run-recheck.txt (literal, con rutas redactadas; leído de su transcripción, con procedencia). I-52 señala una diferencia con sus notas en BlockPreviewUser; los HOST-EXTRAS muestran dos claves distintas, sin cambio y anteriores a la ventana |

## Incidentes y notas del operador

1. **SM-29, OWNER_GESTURE_INCIDENT (clasificación propuesta).** Detalle en `disposition/deviation-request-sm29.md`.
   - Primera petición sin efecto: 130 s de espera.
   - Al terminar el turno, ChatGPT apareció en el monitor principal; el Owner lo minimizó.
   - Primer gesto: V0 (3344.54 1493.04 0.0) → V1 (3079.37 1205.26 0.0), pero con clics izquierdos, `Esc` y rueda («Clic izquierdo», «Tecla Esc», «Giré la rueda»). Owner, literal: «hecho, la vista siguió al ratón y la paleta no parpadeó». No aísla el encuadre.
   - Durante la pausa, el Owner hizo otro arrastre sin petición de la sesión («Sí, lo hice yo»): V1 → V2 sin cambio de escala visible, sin `VIEWSIZE` de referencia en V1. Se conserva como parte del incidente; no se usa como prueba.
   - Disposición: el Owner escribió «A» en el chat. **La sesión no recibió texto literal del Coordinator; pide confirmarlo en la revisión** (`disposition/disposition-sm29-option-a.md`).
   - Repetición limpia: V2 → V3 con `VIEWSIZE` igual. El Owner no marcó «Tuve que repetirlo». Foco: antes de V3 la sesión hizo clic en la línea de órdenes, porque el Owner había usado el chat; que la paleta no robó el foco es observación del Owner y del historial sin líneas nuevas.
2. **Stage A.** El primer Stage A encontró ChatGPT maximizado en el monitor principal. AutoCAD no se abrió hasta que el Owner lo quitó y Stage A se repitió (40 min después; la BASELINE no se repitió, y el DIFF no muestra deriva ajena).
3. **Autoguardados SAVETIME** de AutoCAD en la carpeta temporal (`.sv$`), de A durante SM-15 y de C más tarde: no son un guardado del dibujo; desaparecieron con el cierre normal; no se usan como evidencia (SM-41). Las capturas de la línea de órdenes que podían mostrar la ruta se evitaron o se recortaron. Los `.sv$` previos a la ocurrencia no se tocaron.
4. **Capa transparente de la app de Claude** mientras la herramienta tiene el control: ventana a pantalla completa, transparente al ratón, en el monitor principal. Deja un leve tinte rosado en los bordes de las capturas; no tapa texto. La comprobación previa de `cap.ps1` la detectó en SM-03 y se ajustó para bloquear solo ventanas opacas. La lectura de la capa como indicador de la app es una interpretación de la sesión.
5. **Revisión de capturas.** Las de SM-03, SM-12, SM-13, SM-14, SM-16, SM-17 (paleta), SM-18, SM-26, SM-27b, SM-29-despues, SM-29-repeticion, SM-34 y SM-35 se abrieron al tomarlas. Cinco de apoyo se abrieron más tarde, antes de este registro: SM-17-izquierda-cmdline, SM-17-derecha-cmdline, SM-17-derecha-final-cmdline, SM-29-antes-palette y SM-29-antes-cmdline. Todas están limpias.
6. **Comprobación antes de `Enter`.** Las expresiones que dan un resultado (`DBMOD`, `VIEWCTR`, `VIEWSIZE`, `sslength`, la línea de medición) se comprobaron antes de `Enter`, salvo el segundo trozo de A-2, cuya imagen se revisó justo después de enviarlo. Órdenes cortas sin expresión (`OPEN`, `RACKPANEL` en SM-04 y SM-32, `LINE`, `CIRCLE`, `ZOOM`, `PAN`, `MOVE`, `COPY`, `ERASE`, `UNDO`, `RACKAYUDA`) se enviaron en el mismo lote que su `Enter`, con «Typed N char(s)» sin portapapeles. Ninguna dio un resultado inesperado.
7. **`_ALL N found` de SM-31 y SM-32.** La línea queda cortada por el borde de la línea de órdenes; no se afirma el número.
8. **Registro con retraso.** Dos órdenes de registro las bloqueó un filtro de la herramienta por el texto; se reescribieron, y una entrada entró unos 40 s tarde (anotado en el diario). El intento de 18:5x no llegó a registrar el Check de esa llamada; se repitió a las 18:54:52Z.
9. **Lecturas no autorizadas no hechas.** `MBUTTONPAN` no se leyó: la autorización no la nombra. `VIEWSIZE` se leyó solo tras la opción A.

```text
Firma del Owner:          FIRMADO: «Firmo el registro.» (chat de la sesión; hora medida 2026-11-07T19:16:29Z;
                          disposition/owner-signature.md). Firma el registro tal como está; confirma el registro, no un PASS
                          (única edición posterior: este bloque de firma)
Propuesta de la sesión:   ver disposition/return-package.md. La sesión no declara PASS
```
