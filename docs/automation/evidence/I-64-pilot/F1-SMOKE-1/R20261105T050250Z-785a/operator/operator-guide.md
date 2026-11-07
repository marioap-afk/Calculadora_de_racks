# I-64 Smoke-1 — Guía del operador, RunId R20261105T050250Z-785a (Candidato cc7503e3 reutilizado, DLL 3C261B8F…B6A0)

Es un resumen operativo; **T3D manda en caso de duda** (`docs/initiatives/I-64-f1-t3d-smoke-1-procedure.md`). Lo completan dos
documentos literales en `authorization/` y en `F1-SMOKE-1-RETRY-PREP/host-recheck-after-a578/`:
- «I-64 — COORDINATOR SMOKE-1 RETRY AUTHORIZATION» (785a);
- «I-64 — COORDINATOR OPERATOR DISPOSITION».

**Operador:** «AutoCAD operated by implementation session under Coordinator OPERATOR_EXCEPTION for this occurrence.»
- Es una excepción explícita del Coordinator, anterior a la ejecución, para esta ocurrencia sola. No es una desviación de T3D.
- La sesión también ejecuta el preflight, HOST-EXTRAS, la BASELINE, las inspecciones de solo lectura, la POST y los diffs.
- No puede:
  - cambiar las reglas de aceptación;
  - saltarse pasos obligatorios;
  - hacer cambios exploratorios en el host;
  - tocar seguridad, perfil ni registro;
  - declarar PASS por su cuenta.

**Reglas de la guarda de procesos** (ACCEPTED y BINDING;
`F1-SMOKE-1-RETRY-PREP/host-recheck-after-a578/process-guard/process-guard-procedure.md`, revisión 2):
1. `open_application` solo en PG-1a, con acad = 0 recién contado. Con AutoCAD abierto, nunca.
2. Si un clic falla o hay otra ventana delante: captura, `kit\Test-AcadProcessGuard.ps1 -Mode Check -Checkpoint FOCUS-<SM>`, y clic
   en la propia ventana de AutoCAD. Si no se ve, STOP y vuelta al Coordinator.
3. Antes de cada paso, en `operator/session-log.md`: hora UTC, acción exacta y resultado. Un `computer_batch` solo agrupa las
   acciones de UN paso.
4. Solo las acciones de T3D, sin exploración. Una desviación necesaria se para y vuelve al Coordinator.
5. Capturas solo de la paleta y de la línea de órdenes, con `kit\Save-ScreenRegion.ps1`, en `operator/screens/<SM>-<que>.png`.
6. Dibujos solo con `OPEN` en la misma instancia, navegando el diálogo con clics. Nunca con doble clic desde el Explorador.
7. Sin saltos de línea dentro del texto escrito en AutoCAD; Enter aparte.
8. Aviso de seguridad del NETLOAD: **solo «Load Once»**. Se anotan las opciones mostradas y la elegida.
9. **Nunca guardar** A, B, C ni D.

**Puntos de guarda** (en `process/`):

| PG | Cuándo | Orden |
|---|---|---|
| PG-0 | preflight y BASELINE | `kit\owner-preflight-baseline.ps1` (acad = 0 y RAM = 0 en los pasos 1, 6, 6b y 7) |
| PG-0b | antes de la BASELINE | aviso a «I - 52» e «I - 62» |
| PG-1a | justo antes de abrir AutoCAD | acad = 0; una sola llamada a `open_application("AutoCAD 2025 - English")` |
| PG-1 | AutoCAD abierto, antes de SM-00b | `Test-AcadProcessGuard.ps1 -Mode Register` |
| PG-1w | justo después | `Test-AcadProcessGuard.ps1 -Mode Watch` en segundo plano. Salida 1 o 2 = STOP inmediato |
| PG-2 | SM-00d1, tras el NETLOAD | `Inspect-LoadedRackCad.ps1` ligada a la PID |
| PG-3 | antes de SM-01 y de SM-03; tras SM-13 y SM-17; antes de SM-20; tras SM-26, SM-27 y SM-35; antes de SM-36; tras cada cambio de foco | `-Mode Check -Checkpoint <SM>` |
| PG-4 | antes de SM-40 | `-Mode Check -Checkpoint PRE-CLOSE` |
| PG-5 | tras SM-40 | `-Mode Closed` |
| PG-6/7 | POST | `kit\owner-post.ps1` (exige `closed.ok` y una vigilancia sin STOP); después, `watch-stop.flag` |

## Arranque y carga (T3D §5)

| Paso | Acción (sesión) | Esperado / regla |
|---|---|---|
| SM-00a | Hora UTC; PG-1a; abrir AutoCAD sin opciones y sin elegir ni crear perfiles; si abre en Start, «New»; PG-1 y PG-1w | `Drawing1`; una sola `acad.exe`. Un selector de perfil u otra `acad.exe`: STOP |
| SM-00b | `(getvar "CPROFILE")`, `(getvar "SECURELOAD")` y `(getvar "PICKFIRST")` | CPROFILE = `<<Unnamed Profile>>`; SECURELOAD = el de la BASELINE; anotar PICKFIRST. Discrepancia: STOP |
| SM-00d | `NETLOAD` → `src\RackCad.Plugin\bin\Debug\net8.0-windows\RackCad.Plugin.dll` del worktree de I-64 | aviso de seguridad: solo «Load Once». Si solo ofrece un cambio persistente: STOP-06 |
| SM-00d1 | PG-2, sin tocar AutoCAD | PASS: un solo `RackCad.Plugin.dll` en AUTHORIZED_AUTOCAD_PID, en la carpeta del Candidato, con `3C261B8F…B6A0`. STOP: cerrar sin guardar y POST |
| SM-00e | Abrir `I64-SMOKE1-FIXTURES-D.dxf` (D) | dos rectángulos «PR-13 SIN IDENTIDAD» y «PR-12 DIAGNOSTICO». Si no abre limpio: STOP |
| SM-00c | Abrir C; `(sslength (ssget "_X"))` | anotar el número (> 0) |

## Fase A (C, panel nunca abierto)

PG-3 antes de SM-01. SM-01: la medición del comando masivo con la línea exacta de T3D §6; anotar `ms`; ningún mensaje de RackCad.

```text
(progn (setq t0 (getvar "MILLISECS")) (command "_.MOVE" "_ALL" "" "0,0" "1,0") (princ (strcat "\nSM-T ms=" (itoa (- (getvar "MILLISECS") t0)))) (princ))
```

## Fase B (A; D solo para SM-10b y SM-11)

| Paso | Acción (sesión) | Esperado |
|---|---|---|
| SM-02 | Abrir A; `(getvar "DBMOD")` | anotar; bit 1 = 0 |
| **SM-03** (C1) | PG-3; `RACKPANEL` | paleta acoplada, o flotante **dentro de la pantalla**, visible y usable; «RackCad: panel Workspace mostrado.»; «Sesión activa», «El panel sigue el dibujo activo.», «Sesión <n>», «Leer el dibujo» y «Escribir en el dibujo» marcados, «Selección» = «Ninguno». No mover ni acoplar antes de SM-17. Si no se ve o no se puede usar: STOP, sin rescate. Captura |
| SM-04 | `RACKPANEL` otra vez | misma paleta, sin segunda; mensaje repetido |
| SM-05 | Marcar «Medir en memoria (no escribe en el registro)» | «Instrumentación activada: <k> puntos de medida.» y «Memoria del proceso: <x> MB» |
| SM-06 | Clic en un rack con identidad; esperar 2 s | «Un rack» / «Rack seleccionado: <RackId>»; filas `drain` y `selection.read` |
| SM-07 | Añadir la copia (mismo RackId) | sigue «Un rack», con el mismo RackId |
| SM-08 | Añadir un segundo rack (otro RackId) | «Selección actual: 2 racks y 0 otros objetos.» |
| SM-09 | Añadir una línea suelta | «Selección actual: 2 racks y 1 otros objetos.» |
| SM-10a | `Esc`; clic en el bloque sin datos de RackCad | «Selección actual: 0 racks y 1 otros objetos.» |
| SM-10b | Pestaña de D; esperar 2 s; `Esc`; clic en «PR-13 SIN IDENTIDAD» | «Sin identidad» / «El objeto seleccionado no tiene identidad de rack.»; ningún RackId |
| SM-11 | En D: `Esc`; clic en «PR-12 DIAGNOSTICO» | «Diagnóstico» / «No se pudo interpretar la definición del objeto seleccionado.»; ningún RackId |
| (vuelta) | Pestaña de A; esperar 2 s | contexto de A |
| SM-12 | `Esc`; anotar la última fila; 60 s sin tocar nada; cerrar la paleta con su botón; `RACKPANEL` | sin filas `drain` ni `selection.read` entre la espera y `panel.hide`; sin «Regenerating model» |
| **SM-13** (C2) | Anotar la última fila; `LINE` sin marcar puntos; mirar el panel; unos 5 s; anotar la última fila; `Esc`; PG-3 | con `LINE`: casillas **desmarcadas** y «**Hay un comando en curso.**»; ninguna fila `drain` ni `selection.read` nueva mientras dura la orden. Tras `Esc`: vuelven a marcarse y el texto desaparece. Captura |
| SM-14a..e, h | Foco en «Medir en memoria» con `Tab`; `abc def`; `Ctrl+Z`; `Ctrl+C` y `Ctrl+V`; `Supr` con objetos ya seleccionados; `Esc` y `Enter`; después `(getvar "DBMOD")` | nada llega a AutoCAD; DBMOD bit 1 sin cambios. Es complementario (A-7); S1-06a..e = NOT_APPLICABLE_BY_DESIGN. La barra espaciadora conmuta la casilla con foco |
| SM-15 | Clic en la paleta y `LINE`; después clic en el dibujo y `LINE` (`Esc`) | nada en la línea de órdenes con el foco en la paleta; en el dibujo la orden se inicia |
| SM-16 | Seleccionar dos racks; cerrar la paleta; cambiar la selección; esperar; `RACKPANEL` | sin filas con la paleta cerrada; al mostrarla, `panel.show`, `drain` y `selection.read` una sola vez; contexto actual |
| **SM-17** (C1) | Acoplar a la izquierda y a la derecha; dejar flotante; ocultación automática; PG-3 | sin errores; anotar dónde queda la paleta flotante y cómo se pliega. Captura |
| SM-18 | `(getvar "DBMOD")` | bit 1 igual que en SM-02 |
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
| **SM-26** (C2) | Cerrar todos sin guardar menos C; después cerrar C sin guardar (queda Start); anotar la hora; PG-3 | «**Sin documento**» / «Abre o crea un dibujo para trabajar con el panel.», acciones desmarcadas, «No hay un documento abierto.»; RackLog sin NullReferenceException de `DocumentActivated`. Captura |
| **SM-27** (C2) | (a) Desde Start, abrir C y anotar la sesión. (b) Con C abierto, pestaña **Start**. (c) Desde Start, abrir B. (d) Pestaña de C. PG-3 | (a) «Sesión activa», sesión nueva. (b) «**Sin documento**», sin la sesión anterior de C y sin error en RackLog. (c) Sesión nueva de B. (d) La misma sesión de C que en (a). Captura en (b) |

## Fase D (C, paleta visible)

- SM-28: `LINE` (3 segmentos) y `CIRCLE`.
- SM-29: rueda, encuadre con el botón central (no ejecutable con la herramienta: se anota), `ZOOM` `E` y `PAN`.
- SM-30: `MOVE`, `COPY`, `ERASE` y `UNDO` 3.
- SM-31: cerrar la paleta y medición masiva (`ms`, «oculto»).
- SM-32: `RACKPANEL` y medición masiva (`ms`, «visible»).
- SM-33: `RACKAYUDA` y cerrar su ventana.

## Fase E (B)

| Paso | Acción (sesión) | Esperado |
|---|---|---|
| **SM-34** (C2) | En B: `RACKEDITAR`, elegir el rack selectivo; con el editor abierto, mirar el panel; sin aceptar | el editor funciona y el panel no interfiere; mientras dura la orden, casillas desmarcadas y «**Hay un comando en curso.**»; sin filas `drain` ni `selection.read` nuevas. Captura |
| SM-35 | **Cancelar** el editor (nunca aceptar); esperar; `(getvar "DBMOD")`; PG-3 | la orden termina; anotar DBMOD |
| SM-35n | Nota obligatoria de T3D | — |

## Durante todo (SM-36..SM-38) y cierre

- **SM-36..SM-38**, durante toda la secuencia: «Error del panel» o «Sesión degradada» (literal), un cuadro de excepción no controlada, una
  aserción Debug visible o un cierre inesperado. Una aserción Debug visible o una fuga de transacción: FAIL / STOP. PG-3 antes de SM-36.
- **SM-40:** PG-4; cerrar todos los dibujos **sin guardar** («No») y salir de AutoCAD con normalidad; PG-5.
- **Después del cierre:**
  - **SM-39:** RackLog, ligado a la ventana de la PID por `acad-watch.log`;
  - **SM-41:** SHA-256 de A, B, C, D y de la biblioteca;
  - `kit\owner-post.ps1`: HOST-EXTRAS AFTER y POST;
  - `watch-stop.flag`;
  - DIFF de T3C (PaletteKeyAcceptance = ACCEPTED) y comparación de HOST-EXTRAS;
  - recheck de I-52;
  - el registro del operador completo, que se presenta al Owner para que lo firme.
