# I-64 Smoke-1 — Guía del operador, RunId R20261104T194513Z-a578 (SEGUNDO REINTENTO, Candidato cc7503e3, DLL 3C261B8F…B6A0)

Resumen operativo de `docs/initiatives/I-64-f1-t3d-smoke-1-procedure.md` (T3D, manda en caso de duda) con el orden de la autorización del
Coordinator del 2026-11-04 (`authorization/coordinator-second-smoke1-retry-authorization.md`).

**Roles (T3D §1, sin desviación):** el **Owner** opera AutoCAD (todas las teclas, clics y órdenes). La sesión I-64 **no toca AutoCAD**: guía,
inspecciona en solo lectura (procesos, DLL cargado, RackLog), mide horas y hashes, transcribe y evalúa cada paso. Si la sesión operara
AutoCAD: STOP y vuelta al Coordinator.

El Owner informa por el chat lo que ve (textos literales del panel y de la línea de órdenes); las capturas de pantalla, solo de la paleta
y de la línea de órdenes, nunca de la barra de título. Ante cualquier STOP: no seguir, no reparar, cerrar AutoCAD sin guardar y tomar el
POST. **Nunca guardar** ningún dibujo de prueba (A, B, C ni D).

## Antes de AutoCAD (lo hace `kit\owner-preflight-baseline.ps1`; termina en «OK»)
acad = 0 y RAM Elements = 0 → Candidato exacto → herramientas T3C → autoprueba 26/26 → SHA-256 de A, B, C, D y biblioteca → SECURELOAD
leído por el Owner en solo lectura (= SECURELOAD_BASELINE_CURRENT) → **HOST-EXTRAS BEFORE** (AssemblyMap, CoreUser e historial de NETLOAD,
solo lectura) → BASELINE de T3C con código 0 (vuelve a leer SECURELOAD; si no coincide, STOP antes de AutoCAD). **No abrir AutoCAD hasta
que la sesión I-64 confirme la BASELINE.**

## Arranque y carga
| Paso | Acción del Owner | Esperado / regla |
|---|---|---|
| SM-00a | Abrir AutoCAD 2025 con el acceso habitual, sin opciones y sin elegir ni crear perfiles; si abre en Start, «New» | `Drawing1`; una sola `acad.exe`. Selector de perfil u otra `acad.exe`: STOP |
| SM-00b | `(getvar "CPROFILE")`, `(getvar "SECURELOAD")`, `(getvar "PICKFIRST")` | CPROFILE = `<<Unnamed Profile>>`; SECURELOAD = el de la BASELINE; anotar PICKFIRST. Discrepancia: STOP |
| SM-00d | `NETLOAD` → elegir **exactamente** `src\RackCad.Plugin\bin\Debug\net8.0-windows\RackCad.Plugin.dll` del worktree de I-64 (el diálogo puede abrir en otra carpeta: no elegir nada allí) | aviso de seguridad: **solo «Load Once»**. Nunca «Always Load» ni confiar en la ubicación; nada en TRUSTEDPATHS ni SECURELOAD. Si solo ofrece un cambio persistente: STOP-06 |
| **SM-00d1** | **Decir a la sesión «cargado» y esperar sin tocar AutoCAD** | la sesión inspecciona el proceso (solo lectura) y responde **PASS** (un solo `RackCad.Plugin.dll`, carpeta del Candidato, SHA-256 `3C261B8F…B6A0`) o **STOP**. Con STOP: no seguir; cerrar AutoCAD sin guardar; POST |
| SM-00e | Abrir `I64-SMOKE1-FIXTURES-D.dxf` (dibujo D) | abre limpio, con dos rectángulos «PR-13 SIN IDENTIDAD» y «PR-12 DIAGNOSTICO». Si no abre limpio: STOP |
| SM-00c | Abrir el dibujo C; `(sslength (ssget "_X"))` | anotar el número (> 0) |

## Fase A (C, panel nunca abierto)
SM-01: medición del comando masivo con la línea exacta de T3D §6 (la única expresión permitida); anotar `ms`; ningún mensaje de RackCad.
```text
(progn (setq t0 (getvar "MILLISECS")) (command "_.MOVE" "_ALL" "" "0,0" "1,0") (princ (strcat "\nSM-T ms=" (itoa (- (getvar "MILLISECS") t0)))) (princ))
```

## Fase B (A, y D solo para SM-10b y SM-11)
| Paso | Acción del Owner | Esperado |
|---|---|---|
| SM-02 | Abrir A; `(getvar "DBMOD")` | anotar; bit 1 = 0 |
| **SM-03** | `RACKPANEL` — **comprobación crítica de C1** | paleta acoplada, o flotante y **dentro de la pantalla**, visible y usable; decir dónde. «RackCad: panel Workspace mostrado.»; «Sesión activa», «El panel sigue el dibujo activo.», «Sesión <n>», «Leer el dibujo» y «Escribir en el dibujo» **marcados**, «Selección» = «Ninguno». **No mover ni acoplar la paleta antes de SM-17.** Si no se ve o no se puede usar: **STOP** |
| SM-04 | `RACKPANEL` otra vez | misma paleta, sin segunda; mensaje repetido |
| SM-05 | Marcar «Medir en memoria (no escribe en el registro)» | «Instrumentación activada: <k> puntos de medida.», «Memoria del proceso: <x> MB» |
| SM-06 | Clic en un rack con identidad; esperar 2 s | «Un rack» / «Rack seleccionado: <RackId>»; filas `drain` y `selection.read` |
| SM-07 | Añadir la copia (mismo RackId) | sigue «Un rack», mismo RackId |
| SM-08 | Añadir un segundo rack (otro RackId) | «Selección actual: 2 racks y 0 otros objetos.» |
| SM-09 | Añadir una línea suelta | «Selección actual: 2 racks y 1 otros objetos.» |
| SM-10a | `Esc`; clic en el bloque sin datos de RackCad | «Selección actual: 0 racks y 1 otros objetos.» |
| SM-10b | Pestaña de **D**; esperar 2 s; `Esc`; clic en «PR-13 SIN IDENTIDAD» | «Sin identidad» / «El objeto seleccionado no tiene identidad de rack.»; ningún RackId |
| SM-11 | En D: `Esc`; clic en «PR-12 DIAGNOSTICO» | «Diagnóstico» / «No se pudo interpretar la definición del objeto seleccionado.»; ningún RackId |
| (vuelta) | Pestaña de **A**; esperar 2 s | contexto de A |
| SM-12 | `Esc`; anotar el número de la última fila de la tabla; 60 s sin tocar nada; cerrar la paleta con su botón; `RACKPANEL` | sin filas `drain`/`selection.read` entre la espera y `panel.hide`; sin «Regenerating model» |
| **SM-13** | **Comprobación crítica de C2.** Anotar el número de la última fila de la tabla; escribir `LINE` y **no** marcar ningún punto; mirar el panel; esperar unos 5 s; anotar la última fila; después `Esc` | con `LINE` activo: «Leer el dibujo» y «Escribir en el dibujo» **desmarcados**; «**Hay un comando en curso.**»; **ninguna fila nueva `drain` ni `selection.read` mientras dura la orden**. Tras `Esc`: vuelven a marcarse y el texto desaparece |
| SM-14a..e, h | Foco en «Medir en memoria» (con `Tab`): `abc def`; `Ctrl+Z`; `Ctrl+C`/`Ctrl+V`; `Supr` con objetos ya seleccionados; `Esc` y `Enter`; después `(getvar "DBMOD")` | nada llega a AutoCAD; sin UNDO, COPYCLIP, PASTECLIP ni borrado; DBMOD bit 1 sin cambios. Complementario (A-7); S1-06a..e = NOT_APPLICABLE_BY_DESIGN. Ojo: la barra espaciadora conmuta la casilla con foco |
| SM-15 | Clic en la paleta y escribir `LINE`; después clic en el dibujo y `LINE` (`Esc`) | nada en la línea de órdenes con foco en la paleta; en el dibujo la orden se inicia (y el panel muestra «Hay un comando en curso.») |
| SM-16 | Seleccionar dos racks; cerrar la paleta; cambiar la selección; esperar; `RACKPANEL` | sin filas con la paleta cerrada; al mostrar: `panel.show`, `drain`, `selection.read` una vez; contexto actual |
| **SM-17** | **Comprobación crítica de C1.** Acoplar a la izquierda, a la derecha, dejar flotante; probar la ocultación automática | sin errores; decir dónde queda la paleta flotante y cómo se pliega |
| SM-18 | `(getvar "DBMOD")` | bit 1 igual que en SM-02 |
| SM-19 | Solo si PICKFIRST = 0 | aviso «PICKFIRST está desactivado: …»; si no: NOT_APPLICABLE |

## Fase C (varios documentos)
| Paso | Acción del Owner | Esperado |
|---|---|---|
| SM-20 | Abrir B | «Sesión <m>», m ≠ n, «Ninguno» |
| SM-21 | Clic en un rack de B | «Un rack», RackId de B |
| SM-22 | Volver a A | «Sesión <n>», sin restos de B |
| SM-23 | Volver a B | «Sesión <m>» |
| SM-24 | Abrir A otra vez en **solo lectura** | «Sesión <p>», p ≠ n; las copias no se mezclan |
| SM-25 | Cerrar esa copia sin guardar | sin pregunta |
| **SM-26** | **Comprobación crítica de C2.** Cerrar todos sin guardar menos C; después cerrar C sin guardar (queda la página Start, ningún dibujo); decir la hora aproximada | «**Sin documento**» / «Abre o crea un dibujo para trabajar con el panel.», acciones desmarcadas, «No hay un documento abierto.»; la sesión comprueba RackLog: **ninguna NullReferenceException de `DocumentActivated`** |
| **SM-27** | **Comprobación crítica de C2.** (a) Desde Start, abrir C → anotar la sesión. (b) Con C abierto, pulsar la pestaña **Start** y mirar el panel. (c) Desde Start, abrir B. (d) Pulsar la pestaña de C | (a) «Sesión activa», sesión nueva; (b) con Start activa: «**Sin documento**», **sin la sesión anterior de C** y sin error en RackLog; (c) «Sesión activa», sesión nueva de B; (d) vuelve **la misma sesión de C** de (a) |

## Fase D (C, paleta visible)
SM-28 `LINE` (3 segmentos) y `CIRCLE`; SM-29 rueda, encuadre con botón central, `ZOOM` `E`, `PAN`; SM-30 `MOVE`, `COPY`, `ERASE` y
`UNDO` 3; SM-31 cerrar la paleta y medición masiva (anotar `ms`, «oculto»); SM-32 `RACKPANEL` y medición masiva (`ms`, «visible»);
SM-33 `RACKAYUDA` y cerrar su ventana.

## Fase E (B)
| Paso | Acción del Owner | Esperado |
|---|---|---|
| **SM-34** | **Comprobación crítica de C2.** En B: `RACKEDITAR`, elegir el rack selectivo; con el editor abierto, mirar el panel; usar el editor con normalidad (sin aceptar) | el editor funciona con normalidad y el panel no interfiere; mientras dura la orden: «Leer el dibujo» y «Escribir en el dibujo» desmarcados y «**Hay un comando en curso.**»; sin filas `drain`/`selection.read` nuevas durante la orden |
| SM-35 | **Cancelar / Cerrar** el editor (nunca aceptar), esperar, `(getvar "DBMOD")` | la orden termina; anotar DBMOD |

## Durante todo (SM-36..SM-38)
Decir a la sesión cualquier «Error del panel» o «Sesión degradada» (literal), cualquier cuadro de excepción no controlada, cualquier aserción
Debug visible o cierre inesperado. Una aserción Debug visible o una fuga de transacción: FAIL / STOP.

## Cierre
SM-40: cerrar todos los dibujos **sin guardar** («No») y salir de AutoCAD con normalidad. Avisar a la sesión: ella comprueba acad = 0, lee
RackLog (SM-39) y los SHA-256 de los dibujos (SM-41). Después el Owner ejecuta `kit\owner-post.ps1` (HOST-EXTRAS AFTER y POST de T3C) y
avisa; la sesión hace el DIFF de T3C con PaletteKeyAcceptance = ACCEPTED, compara HOST-EXTRAS y pide a I-52 su recomprobación. Al final
el Owner firma el registro.
