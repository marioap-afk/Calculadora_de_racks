# I-64 Smoke-1 — Guía del operador, RunId R20261107T171052Z-6862 (Candidato cc7503e3 reutilizado, DLL 3C261B8F…B6A0)

Es un resumen operativo; **T3D manda en caso de duda** (`docs/initiatives/I-64-f1-t3d-smoke-1-procedure.md`). La autorización está literal en
`authorization/coordinator-full-smoke1-authorization-6862.md`.

**Operador:** implementation session, por excepción explícita del Coordinator para 6862, con una excepción dentro: **OWNER_MANUAL_GESTURE_EXCEPTION — SM-29 only**.
- La sesión ejecuta también preflight, BASELINE, HOST-EXTRAS, inspecciones de procesos y módulos, POST, comparación de T3C y capturas.
- No puede cambiar las reglas de aceptación, saltarse pasos obligatorios, hacer cambios exploratorios en el host, tocar seguridad, perfil ni registro, ni declarar PASS.

**Reglas de la guarda de procesos** (BINDING; `F1-SMOKE-1-RETRY-PREP/host-recheck-after-a578/process-guard/process-guard-procedure.md`):
1. `open_application` solo en PG-1a, con acad = 0 recién contado. Con AutoCAD abierto, nunca.
2. Si un clic falla o hay otra ventana delante: captura, `Check FOCUS-<SM>` y clic en la propia ventana de AutoCAD. Si no se ve: STOP y vuelta al Coordinator.
3. Antes de cada paso, en `operator/session-log.md`: hora UTC, acción y resultado. Un `computer_batch` solo agrupa las acciones de UN paso.
4. Solo las acciones de T3D y las de esta guía autorizadas. Una desviación necesaria se para y vuelve al Coordinator.
5. Dibujos solo con `OPEN` en la misma instancia, navegando el diálogo con clics, o desde Recent en Start donde lo pide SM-27.
6. Aviso de seguridad del NETLOAD: **solo «Load Once»**. Se anotan las opciones mostradas y la elegida.
7. **Nunca guardar** A, B, C ni D.

**Capturas:**
- **Stage A, justo antes de SM-00a:** monitor guardado de AutoCAD, ventana de Claude en otro monitor, regiones, mecanismo y obstrucción sintética (`preflight/stage-a-*`).
- **Stage B, en SM-03 antes de SM-04:** captura de la paleta real; título y cabecera completos, estado, controles de lectura y escritura, contenido inferior, línea de órdenes y ninguna ventana ajena. Si falla: **PAUSA**; se corrige la disposición de ventanas sin tocar el dibujo y no se sigue hasta tener una captura limpia.
- **Cada captura crítica se abre y se revisa justo después de tomarla.** Una obstrucción se corrige sin alterar el dibujo y se repite la captura; las dos se conservan.
- Solo paleta y línea de órdenes, con `kit\Save-ScreenRegion.ps1`, en `operator/screens/<SM>-<que>.png`; nunca la barra de título de AutoCAD. En Start, solo la paleta (la página muestra proyectos de terceros).

**Entrada de teclado:**
- Línea de órdenes: **sin `Ctrl+V` y sin aceleradores**. Escritura directa en trozos de ≤16 caracteres; cada resultado debe decir «Typed N char(s)»; si dice «via clipboard», no se pulsa `Enter`, se vacía con `Esc` y se anota. Texto comprobado antes de `Enter`.
- Tras tener el foco la paleta: clic en la línea de órdenes, se escribe el primer trozo, se comprueba que aparece y solo entonces se sigue.
- `Ctrl+V` solo en SM-14c, una vez, con el foco visible y anotado dentro de la casilla.

**Puntos de guarda** (en `process/`):

| PG | Cuándo | Orden |
|---|---|---|
| PG-0 | preflight y BASELINE | `kit\owner-preflight-baseline.ps1` (acad = 0 y RAM = 0 en los pasos 1, 6, 6b y 7; SECURELOAD fresco y lectura independiente de la BASELINE) |
| PG-0b | antes de la BASELINE | aviso a «I - 52» e «I - 62» |
| PG-1a | justo antes de abrir AutoCAD | acad = 0; Stage A; una sola llamada a `open_application("AutoCAD 2025 - English")` |
| PG-1 | AutoCAD abierto, antes de SM-00b | `Test-AcadProcessGuard.ps1 -Mode Register` |
| PG-1w | justo después | `-Mode Watch` en segundo plano. Salida 1 o 2 = STOP inmediato |
| PG-2 | SM-00d1, tras el NETLOAD | `Inspect-LoadedRackCad.ps1` ligada a la PID |
| PG-3 | antes de SM-01 y de SM-03; tras SM-13 y SM-17; antes de SM-20; tras SM-26, SM-27 y SM-35; antes de SM-36; tras cada cambio de foco | `-Mode Check -Checkpoint <SM>` |
| PG-4 | antes de SM-40 | `-Mode Check -Checkpoint PRE-CLOSE` |
| PG-5 | tras SM-40 | `-Mode Closed` |
| PG-6/7 | POST | `kit\owner-post.ps1` (exige `closed.ok` y una vigilancia sin STOP); después, `watch-stop.flag` |

## DBMOD del dibujo A (bit 1 = dibujo modificado; bit 16 y otros = vista o estado)

Lecturas de solo lectura con `(getvar "DBMOD")`, escritas con el teclado; son puntos de guarda y no cambian T3D:

| Id | Momento | Origen |
|---|---|---|
| A-1 = SM-02 | A recién abierto | T3D y Coordinator |
| A-2 | tras alejar la vista, antes de SM-06 | adicional |
| A-3 | tras SM-12 | adicional |
| A-4 | tras SM-13 y la preselección de la línea, antes de SM-14a | Coordinator |
| A-5 | después de SM-14e | Coordinator |
| A-6 | inmediatamente antes de SM-14h | Coordinator |
| **SM-14h** | la aserción de T3D | T3D |
| A-7 | inmediatamente después de SM-14h | Coordinator |
| A-8 | tras SM-15 | adicional |
| A-9 | tras SM-16 | adicional |
| A-10 | antes de SM-18, tras SM-17 | Coordinator |
| **SM-18** | la aserción autoritativa | T3D |
| A-11 | después de SM-18 | Coordinator |

**Regla:** si el bit 1 se activa antes de SM-18: **STOP**. No se repara, no se deshace y no se reinicia DBMOD. Se cierra sin guardar y se toma la POST (T3D §11). Si coincide con un paso prescrito de T3D sin desviación del operador, también se detiene y la causa la clasifica el Coordinator.

## Arranque y carga (T3D §5)

| Paso | Acción (sesión) | Esperado / regla |
|---|---|---|
| SM-00a | Hora UTC; PG-1a; abrir AutoCAD sin opciones y sin elegir ni crear perfiles; si abre en Start, «New»; PG-1 y PG-1w; comprobar que abre en el monitor principal | `Drawing1`; una sola `acad.exe`. Un selector de perfil u otra `acad.exe`: STOP |
| SM-00b | `(getvar "CPROFILE")`, `(getvar "SECURELOAD")` y `(getvar "PICKFIRST")` | CPROFILE = `<<Unnamed Profile>>`; SECURELOAD = el de la BASELINE; anotar PICKFIRST. Discrepancia: STOP |
| SM-00d | `NETLOAD` → `src\RackCad.Plugin\bin\Debug\net8.0-windows\RackCad.Plugin.dll` del worktree de I-64 | aviso de seguridad: solo «Load Once». Si solo ofrece un cambio persistente: STOP-06 |
| SM-00d1 | PG-2, sin tocar AutoCAD | PASS: un solo `RackCad.Plugin.dll` en AUTHORIZED_AUTOCAD_PID, en la carpeta del Candidato, con `3C261B8F…B6A0`. STOP: cerrar sin guardar y POST |
| SM-00e | Abrir `I64-SMOKE1-FIXTURES-D.dxf` (D) | dos rectángulos «PR-13 SIN IDENTIDAD» y «PR-12 DIAGNOSTICO». Si no abre limpio: STOP |
| SM-00c | Abrir C; `(sslength (ssget "_X"))` | anotar el número (> 0) |

## Fase A (C, panel nunca abierto)

PG-3 antes de SM-01. SM-01: la medición del comando masivo con la línea exacta de T3D §6, en trozos; anotar `ms`; ningún mensaje de RackCad.

```text
(progn (setq t0 (getvar "MILLISECS")) (command "_.MOVE" "_ALL" "" "0,0" "1,0") (princ (strcat "\nSM-T ms=" (itoa (- (getvar "MILLISECS") t0)))) (princ))
```

## Fase B (A; D solo para SM-10b y SM-11)

| Paso | Acción (sesión) | Esperado |
|---|---|---|
| SM-02 | Abrir A; `(getvar "DBMOD")` (A-1) | anotar; bit 1 = 0 |
| **SM-03** (C1) | PG-3; `RACKPANEL`; **Stage B** | paleta acoplada, o flotante **dentro de la pantalla**, visible y usable; «RackCad: panel Workspace mostrado.»; «Sesión activa», «El panel sigue el dibujo activo.», «Sesión <n>», «Leer el dibujo» y «Escribir en el dibujo» marcados, «Selección» = «Ninguno». No mover ni acoplar antes de SM-17, salvo la corrección de ventanas de Stage B si hiciera falta. Si no se ve o no se puede usar: STOP, sin rescate |
| SM-04 | `RACKPANEL` otra vez | misma paleta, sin segunda; mensaje repetido |
| SM-05 | Marcar «Medir en memoria (no escribe en el registro)» | «Instrumentación activada: <k> puntos de medida.» y «Memoria del proceso: <x> MB» |
| (preparación) | Rueda para alejar en A hasta ver los tres racks (solo vista); `(getvar "DBMOD")` (A-2) | bit 1 sin cambios |
| SM-06 | Clic en el rack de **arriba a la izquierda** (`56ec2c3a…`); esperar 2 s | «Un rack» / «Rack seleccionado: <RackId>»; filas `drain` y `selection.read` |
| SM-07 | Añadir su copia, el rack de **arriba a la derecha** (mismo RackId) | sigue «Un rack», con el mismo RackId |
| SM-08 | Añadir el rack de **abajo** (`54ef4061…`) | «Selección actual: 2 racks y 0 otros objetos.» |
| SM-09 | Añadir la línea suelta | «Selección actual: 2 racks y 1 otros objetos.» |
| SM-10a | `Esc`; rueda para acercar al bloque diminuto (solo vista); clic en el bloque sin datos de RackCad | «Selección actual: 0 racks y 1 otros objetos.» |
| SM-10b | Dibujo D (menú Window si la paleta tapa las pestañas); esperar 2 s; `Esc`; clic en el borde derecho de «PR-13 SIN IDENTIDAD» | «Sin identidad» / «El objeto seleccionado no tiene identidad de rack.»; ningún RackId |
| SM-11 | En D: `Esc`; clic en el borde derecho de «PR-12 DIAGNOSTICO» | «Diagnóstico» / «No se pudo interpretar la definición del objeto seleccionado.»; ningún RackId |
| (vuelta) | Volver a A; esperar 2 s | contexto de A |
| SM-12 | `Esc`; anotar la última fila; 60 s sin tocar nada; cerrar la paleta con su botón; `RACKPANEL`; `(getvar "DBMOD")` (A-3) | sin filas `drain` ni `selection.read` entre la espera y `panel.hide`; sin «Regenerating model» |
| **SM-13** (C2) | Anotar la última fila; `LINE` sin marcar puntos; mirar el panel; captura; unos 5 s; anotar la última fila; `Esc`; PG-3 | con `LINE`: casillas **desmarcadas** y «**Hay un comando en curso.**»; ninguna fila `drain` ni `selection.read` nueva mientras dura la orden. Tras `Esc`: vuelven a marcarse y el texto desaparece |
| (preparación) | Clic en la línea suelta (preselección de SM-14d); `(getvar "DBMOD")` (A-4) | bit 1 sin cambios |
| SM-14a | Clic en un encabezado de la paleta y `Tab`; recuadro de foco **visible** en «Medir en memoria» y anotado. Teclas reales una a una: `a b c espacio d e f` | ningún carácter en la línea de órdenes; la barra espaciadora conmuta la casilla (se anota) |
| SM-14b | `Ctrl+Z` | sin `UNDO`; el dibujo no cambia |
| SM-14c | Foco visible en la casilla, **captura y anotación**; `Ctrl+C`; `Ctrl+V` **una vez**; revisar al momento | sin `COPYCLIP` ni `PASTECLIP`. Si AutoCAD inicia PASTECLIP, la línea de órdenes reacciona, DBMOD cambia o el foco es ambiguo: **STOP inmediato, sin continuación de recuperación** |
| SM-14d | `Supr` con el objeto ya seleccionado | no se borra |
| SM-14e | `Esc` y `Enter` | no se cancela ni se repite ninguna orden |
| SM-14h | Clic en la línea de órdenes, primer trozo comprobado; A-5; A-6; `(getvar "DBMOD")`; A-7 | bit 1 sin cambios (S1-06h) |
| SM-15 | Clic en la casilla de instrumentación (la vuelve a marcar) y teclas reales `L I N E`; después clic en un punto vacío del dibujo, `Esc`, `LINE`, `Enter` y `Esc`; `(getvar "DBMOD")` (A-8) | nada en la línea de órdenes con el foco en la paleta; en el dibujo la orden se inicia. Si la casilla no quedó marcada, se marca antes de SM-16 y se anota |
| SM-16 | Seleccionar dos racks; cerrar la paleta; cambiar la selección; esperar; `RACKPANEL`; `(getvar "DBMOD")` (A-9) | sin filas con la paleta cerrada; al mostrarla, `panel.show`, `drain` y `selection.read` una sola vez; contexto actual |
| **SM-17** (C1) | Acoplar a la izquierda y a la derecha; dejar flotante; ocultación automática plegar y desplegar; acoplar a la derecha para los pasos siguientes; PG-3. Captura y revisión de cada estado | sin errores; anotar dónde queda la paleta flotante y cómo se pliega |
| SM-18 | A-10; `(getvar "DBMOD")`; A-11 | bit 1 igual que en SM-02 (S1-08, S1-06h, OV-20) |
| SM-19 | Solo si PICKFIRST = 0 | aviso «PICKFIRST está desactivado: …»; si no, NOT_APPLICABLE |

## Fase C (varios documentos)

PG-3 antes de SM-20.

| Paso | Acción (sesión) | Esperado |
|---|---|---|
| SM-20 | Abrir B | «Sesión <m>», con m ≠ n, y «Ninguno» |
| SM-21 | Clic en un rack de B | «Un rack», con el RackId de B |
| SM-22 | Volver a A | «Sesión <n>», sin restos de B |
| SM-23 | Volver a B | «Sesión <m>» |
| SM-24 | Abrir A otra vez en solo lectura | «Sesión <p>», con p ≠ n; las copias no se mezclan |
| SM-25 | Cerrar esa copia sin guardar | sin pregunta |
| **SM-26** (C2) | Cerrar todos sin guardar menos C; después cerrar C sin guardar (queda Start); anotar la hora; PG-3. Captura y revisión | «**Sin documento**» / «Abre o crea un dibujo para trabajar con el panel.», acciones desmarcadas, «No hay un documento abierto.»; RackLog sin NullReferenceException de `DocumentActivated` |
| **SM-27** (C2) | (a) Desde Start, abrir C y anotar la sesión. (b) Con C abierto, pestaña **Start**; captura y revisión. (c) Desde Start, abrir B. (d) Pestaña de C. PG-3 | (a) «Sesión activa», sesión nueva. (b) «**Sin documento**», sin la sesión anterior de C y sin error en RackLog. (c) Sesión nueva de B. (d) La misma sesión de C que en (a) |

## Fase D (C, paleta visible)

- SM-28: `LINE` (3 segmentos) y `CIRCLE`.
- **SM-29 (S1-02, OV-02):**
  1. La sesión hace la rueda (zoom) y lee `(getvar "VIEWCTR")` = V0, con el foco en AutoCAD.
  2. **OWNER_MANUAL_GESTURE_EXCEPTION — SM-29 only.** La sesión pide en el chat el arrastre. El Owner lo lee en el monitor secundario sin hacer clic en el chat, lleva el ratón físico al área de dibujo, arrastra con el botón central una vez y suelta. No hace ninguna otra acción en AutoCAD. La sesión no envía entradas mientras tanto.
  3. La sesión escribe `(getvar "VIEWCTR")` = V1, sin clic previo si es posible.
  4. V1 ≠ V0 confirma el encuadre. Captura de la paleta y de la línea de órdenes, revisadas al momento; observación del Owner, literal.
  5. La sesión retoma con `ZOOM` `E` y `PAN`.
  - S1-02 no se da por PASS por otras acciones de navegación. Si V1 = V0 o no se puede hacer: S1-02 no se marca PASS, pausa y vuelta al Coordinator.
  - `MBUTTONPAN` no está en la autorización: no se lee.
- SM-30: `MOVE`, `COPY`, `ERASE` y `UNDO` 3.
- SM-31: cerrar la paleta y medición masiva (`ms`, «oculto»).
- SM-32: `RACKPANEL` y medición masiva (`ms`, «visible»).
- SM-33: `RACKAYUDA` y cerrar su ventana.

## Fase E (B)

| Paso | Acción (sesión) | Esperado |
|---|---|---|
| (requisito) | Pestaña de B; `(getvar "DBMOD")` | anotar; bit 1 = 0 antes de SM-34 |
| **SM-34** (C2) | En B: `RACKEDITAR`, elegir el rack selectivo; con el editor abierto, mirar el panel; sin aceptar. Captura y revisión | el editor funciona y el panel no interfiere; mientras dura la orden, casillas desmarcadas y «**Hay un comando en curso.**»; sin filas `drain` ni `selection.read` nuevas |
| SM-35 | **Cancelar** el editor (nunca aceptar); esperar; `(getvar "DBMOD")`; PG-3 | la orden termina; bit 1 sin cambios |
| SM-35n | Nota obligatoria de T3D | — |

## Durante todo (SM-36..SM-38) y cierre

- **SM-36..SM-38**, durante toda la secuencia: «Error del panel» o «Sesión degradada» (literal), un cuadro de excepción no controlada, una aserción Debug visible o un cierre inesperado. Una aserción Debug visible o una fuga de transacción: FAIL / STOP. PG-3 antes de SM-36.
- **SM-40:** PG-4; cerrar todos los dibujos **sin guardar** («No») y salir de AutoCAD con normalidad; PG-5.
- **Después del cierre:**
  - **SM-39:** RackLog, ligado a la ventana de la PID por `acad-watch.log`;
  - **SM-41:** SHA-256 de A, B, C, D y de la biblioteca;
  - `kit\owner-post.ps1`: HOST-EXTRAS AFTER y POST;
  - `watch-stop.flag`;
  - DIFF de T3C y comparación de HOST-EXTRAS. **PaletteKeyAcceptance:** en la autorización de 6862 no aparece de forma literal. Se pasa `NOT_RECORDED`, el valor que T3D §8 prevé si el acuse no lo dice, y se informa;
  - recheck de I-52;
  - el registro del operador completo, que se presenta al Owner para que lo firme.
