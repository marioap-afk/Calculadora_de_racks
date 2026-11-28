# I-64 Smoke-1 — Guía del operador, RunId R20261013T011654Z-184b

Resumen operativo de `docs/initiatives/I-64-f1-t3d-smoke-1-procedure.md` (T3D, manda en caso de duda) con el orden de la autorización del
Coordinator. Cada paso se anota en `operator-record.md` (resultado, textos literales, valores). Ante cualquier STOP: no seguir, no
reparar, cerrar AutoCAD sin guardar y tomar el POST. **Nunca guardar** ningún dibujo de prueba (A, B, C ni D).

## Antes de AutoCAD (ya hecho por el preflight si terminó en «OK»)
acad = 0 → Candidato exacto → herramientas T3C → autoprueba 26/26 → SHA-256 de A, B, C, D y biblioteca → SECURELOAD leído (solo
lectura) → BASELINE con código 0. **No abrir AutoCAD hasta que la sesión I-64 confirme la BASELINE.**

## Arranque y carga
| Paso | Acción | Esperado / regla |
|---|---|---|
| SM-00a | Anotar la hora UTC. Abrir AutoCAD 2025 con el acceso habitual, sin opciones y sin elegir ni crear perfiles | `Drawing1`; una sola `acad.exe`. Selector de perfil u otra `acad.exe`: STOP |
| SM-00b | `(getvar "CPROFILE")`, `(getvar "SECURELOAD")`, `(getvar "PICKFIRST")` | CPROFILE = `<<Unnamed Profile>>`; SECURELOAD = el de la BASELINE; anotar PICKFIRST. Discrepancia: STOP |
| SM-00d | `NETLOAD` → elegir **exactamente** `src\RackCad.Plugin\bin\Debug\net8.0-windows\RackCad.Plugin.dll` del worktree de I-64 (nunca el del checkout principal) | si aparece el aviso de seguridad: **solo «cargar una vez»**. Nunca «cargar siempre» ni confiar en la ubicación; nada en TRUSTEDPATHS ni SECURELOAD. Si solo ofrece un cambio persistente: STOP-06 |
| SM-00e | Abrir `I64-SMOKE1-FIXTURES-D.dxf` (dibujo D) | abre limpio, con dos rectángulos rotulados «PR-13 SIN IDENTIDAD» y «PR-12 DIAGNOSTICO». Si no abre limpio: STOP (no reparar el DXF) |
| SM-00c | Abrir el dibujo C; `(sslength (ssget "_X"))` | anotar el número (> 0) |

## Fase A (C, panel nunca abierto)
| SM-01 | Medición del comando masivo (línea de AutoLISP de T3D §6, la única expresión permitida) | anotar `ms`; ningún mensaje de RackCad |

Línea exacta:
```text
(progn (setq t0 (getvar "MILLISECS")) (command "_.MOVE" "_ALL" "" "0,0" "1,0") (princ (strcat "\nSM-T ms=" (itoa (- (getvar "MILLISECS") t0)))) (princ))
```

## Fase B (A, y D solo para SM-10b y SM-11)
| Paso | Acción | Esperado |
|---|---|---|
| SM-02 | Abrir A; `(getvar "DBMOD")` | anotar; bit 1 = 0 |
| SM-03 | `RACKPANEL` | «RackCad: panel Workspace mostrado.»; paleta «RackCad», pestaña «Workspace»; «Sesión activa», «El panel sigue el dibujo activo.», «Sesión <n>», «Leer el dibujo» y «Escribir en el dibujo» marcados, «Selección» = «Ninguno» |
| SM-04 | `RACKPANEL` otra vez | misma paleta, sin segunda; mensaje repetido |
| SM-05 | Marcar «Medir en memoria (no escribe en el registro)» | «Instrumentación activada: <k> puntos de medida.», «Memoria del proceso: <x> MB»; anotar filas |
| SM-06 | Clic en un rack con identidad; esperar 2 s | «Un rack» / «Rack seleccionado: <RackId>»; filas `drain` y `selection.read` |
| SM-07 | Añadir la copia (mismo RackId) | sigue «Un rack», mismo RackId |
| SM-08 | Añadir un segundo rack (otro RackId) | «Selección actual: 2 racks y 0 otros objetos.» |
| SM-09 | Añadir una línea suelta | «Selección actual: 2 racks y 1 otros objetos.» |
| SM-10a | `Esc`; clic en el bloque sin datos de RackCad | «Selección actual: 0 racks y 1 otros objetos.» |
| SM-10b | Pestaña de **D**; esperar 2 s; `Esc`; clic en «PR-13 SIN IDENTIDAD» | «Sin identidad» / «El objeto seleccionado no tiene identidad de rack.»; ningún RackId |
| SM-11 | En D: `Esc`; clic en «PR-12 DIAGNOSTICO» | «Diagnóstico» / «No se pudo interpretar la definición del objeto seleccionado.»; ningún RackId |
| (vuelta) | Pestaña de **A**; esperar 2 s | contexto de A |
| SM-12 | `Esc`; 60 s sin tocar nada; cerrar la paleta con su botón; `RACKPANEL` | sin filas `drain`/`selection.read` entre la espera y `panel.hide`; sin «Regenerating model» |
| SM-13 | `LINE` sin marcar puntos | casillas desmarcadas; «Hay un comando en curso.»; `Esc`: se vuelven a marcar |
| SM-14a..e, h | Foco en «Medir en memoria» (con `Tab`): `abc def`; `Ctrl+Z`; `Ctrl+C`/`Ctrl+V`; `Supr` con objetos ya seleccionados; `Esc` y `Enter`; después `(getvar "DBMOD")` | nada llega a AutoCAD; sin UNDO, COPYCLIP, PASTECLIP ni borrado; DBMOD bit 1 sin cambios. Complementario (A-7); S1-06a..e = NOT_APPLICABLE_BY_DESIGN |
| SM-15 | Clic en la paleta y escribir `LINE`; después clic en el dibujo y `LINE` (`Esc`) | nada en la línea de órdenes con foco en la paleta; en el dibujo la orden se inicia |
| SM-16 | Seleccionar dos racks; cerrar la paleta; cambiar la selección; esperar; `RACKPANEL` | sin filas con la paleta cerrada; al mostrar: `panel.show`, `drain`, `selection.read` una vez; contexto actual |
| SM-17 | Acoplar izquierda, derecha, flotante; probar la ocultación automática | sin errores; anotar el plegado |
| SM-18 | `(getvar "DBMOD")` | bit 1 igual que en SM-02 |
| SM-19 | Solo si PICKFIRST = 0 | aviso «PICKFIRST está desactivado: …»; si no: NOT_APPLICABLE |

## Fase C (varios documentos)
SM-20 abrir B («Sesión <m>», m ≠ n, «Ninguno»); SM-21 clic en un rack de B («Un rack», RackId de B); SM-22 volver a A («Sesión <n>», sin
restos de B); SM-23 volver a B; SM-24 abrir A otra vez en **solo lectura** («Sesión <p>», p ≠ n; las copias no se mezclan); SM-25 cerrar
esa copia sin guardar; SM-26 cerrar todos sin guardar (también D y, al final, C) → «Sin documento» / «Abre o crea un dibujo para trabajar
con el panel.», «No hay un documento abierto.»; SM-27 reabrir C y B → «Sesión activa» con sesiones nuevas.

## Fase D (C, paleta visible)
SM-28 `LINE` (3 segmentos) y `CIRCLE`; SM-29 rueda, encuadre con botón central, `ZOOM` `E`, `PAN`; SM-30 `MOVE`, `COPY`, `ERASE` y
`UNDO` 3; SM-31 cerrar la paleta y medición masiva (anotar `ms`, «oculto»); SM-32 `RACKPANEL` y medición masiva (`ms`, «visible»);
SM-33 `RACKAYUDA` y cerrar su ventana.

## Fase E (B)
SM-34 `RACKEDITAR`, elegir el rack selectivo; con el editor abierto, el panel dice «Hay un comando en curso.»; SM-35 **Cancelar** (nunca
aceptar), esperar, `(getvar "DBMOD")`; SM-35n nota obligatoria (ya prellenada en el registro).

## Durante todo (SM-36..SM-38)
Anotar cualquier «Error del panel» o «Sesión degradada» (literal), cualquier cuadro de excepción no controlada, cualquier aserción Debug
visible o cierre inesperado. Una aserción Debug visible o una fuga de transacción: FAIL / STOP.

## Cierre
SM-40: cerrar todos los dibujos **sin guardar** («No») y salir de AutoCAD. Avisar a la sesión I-64: ella comprueba acad = 0, prepara el
POST y SM-39/SM-41. Capturas de pantalla: solo la paleta y la línea de órdenes, nunca la barra de título.
