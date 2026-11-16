# I-64 F4: diagnóstico en AutoCAD tras F4-T4-HOST-C1 (2026-11-16, ~07:03Z a 07:50Z)

**Contexto:** orden del Owner «Avanza lo más que puedas…» (`F4-DRAFTS/coordinator/owner-f4-advance-2026-11-16.md`). La sesión operó AutoCAD solo para diagnosticar. **No se ejecutó ningún paso de Smoke-2.**

**Montaje:**
- copia desechable `<copias-de-diagnostico>\chk-1.dwg` del original de D1 (R006), cerrada SIN guardar. El SHA-256 de la copia y del original sigue igual: `7A868624…47A5`;
- AutoCAD lanzado con la copia como argumento;
- `NETLOAD` de `<carpeta-del-DLL-de-C1>\RackCad.Plugin.dll` («Cargar una vez»). La lista de módulos de `acad.exe` da esa ruta con SHA-256 `7CD9E5D7…2BDF` (el DLL de C1, versión `e67fb511`);
- sin variables `RACKCAD_DEBUG_*`;
- I-52 recibió el aviso de inicio;
- el registro de RackCad del día no se creó (ninguna línea ERROR) y no hay eventos de error de `acad.exe`.

## 1. Los arreglos de C1, en AutoCAD

Rack A = «PB G», con la planta y la lateral. La selección se hizo por LISP (`ssget` y `sssetfirst`). La pestaña «PB G» aparece en el navegador.

| Comprobación | Antes de C1 (corrida 1 de Smoke-2) | Con el DLL de C1 |
|---|---|---|
| «Editar» en un borrador limpio, sin ninguna orden de por medio | `Limpio (posiblemente obsoleto)` en menos de 1 s | `Listo para editar`, estable a los 5 s |
| «Comprobar ahora» | `Limpio` un instante y luego la marca | `Limpio` + «Sin cambios: el dibujo coincide con la base del borrador.», estable a los 6 s |
| Retroceso como primera tecla en la caja | no borraba | borra a la primera: caja vacía, «Modificado», «Escribe un valor.» |
| «Actualizar», escala 1 → 2 | sin el rótulo «PB G» (F4-E72) | rótulos «PB G» por vista (conteo de TEXT/MTEXT con «PB G» en el BTR): planta 1 → 1, lateral 1 → 1; el rótulo se ve en la lateral, con el tamaño nuevo |
| Estado tras «Actualizar» | `Limpio (posiblemente obsoleto)` | `Limpio` + «Actualizado: el cambio se aplicó al dibujo.», estable a los 5 s; pestaña sin `*`; selección vaciada (PICKFIRST, O-15) |

Después de una orden de AutoCAD (LISP, ZOOM, `U`), el borrador pasa a `Limpio (posiblemente obsoleto)`, que es lo esperado (FRZ:237-238; el anexo, §3).

## 2. F4-S2-03: ¿existía antes de I-64?

- **Panel**, escala 2 → 4 y luego `U`: la orden deshace `EXECUTEFUNCTION`, pero el rótulo sigue grande hasta un `REGEN`. `REDO` funciona: «Everything has been redone». Se reproduce S2-03.
- **`RACKEDITAR` clásico**, escala 4 → 2 con el «Actualizar» de la ventana clásica: «vistas redibujadas (lateral x1, …, planta x1…)». Luego `U` deshace `RACKEDITAR`, y **el rótulo sigue con el tamaño de la escala 2**. `REDO` funciona. Tras `U` y `REGEN`, el rótulo vuelve al tamaño de la escala 4: la base de datos estaba revertida y la pantalla no se había refrescado.

**Conclusión: S2-03 es un comportamiento PREEXISTENTE del redibujo clásico**, porque `RACKEDITAR` hace lo mismo. No es una regresión de I-64. El panel hereda la mecánica de redibujo de I-55 (OV-RED): `Editor.Regen()` al final de la orden, que `U` no repite.

## 3. Una corrección sin eventos: tocar las inserciones en el mismo paso de Undo

- **Experimento** (con LISP, sobre el `RACKEDITAR` clásico):
  - `UNDO BE` → `RACKEDITAR`, escala 4 → 2 con «Actualizar» → `entmod(entget(...))` con los MISMOS datos sobre las dos inserciones de primer nivel («PB G - lateral 1» y «PB G - planta») → `UNDO E` → `U`.
  - **Resultado:** la pantalla se refresca sola, sin `REGEN`: el rótulo vuelve al tamaño de la escala 4. Después, `REDO` («Everything has been redone») vuelve a refrescar al tamaño de la escala 2.
- **Control**, sin el `entmod`: `UNDO BE` → `RACKEDITAR`, escala 2 → 4 → `UNDO E` → `U`.
  - **Resultado:** la pantalla **no** se refresca: el rótulo sigue grande.
  - El refresco lo causa la reescritura de las inserciones de primer nivel dentro del mismo paso.

**Consecuencia para el diseño:**
- **Vía (W):** dentro de la MISMA transacción del Actualizar (y del redibujo clásico), abrir para escritura cada inserción de primer nivel de cada definición de vista redefinida, de modo que quede registrada en el Undo.
- **Lo que hace:** `U` y `REDO` de ese paso refrescan la pantalla.
- **Lo que no toca:** ningún evento, el drenaje y la cláusula «y solo estas» (FRZ:228-232), además de la reentrada estructural.
- **El Undo sigue siendo un solo paso:** un Actualizar del panel es un único paso de UNDO (FRZ:542).

**Pendiente de medir con un build de diagnóstico:** el equivalente .NET (abrir para escritura y `RecordGraphicsModified(true)`, u otra modificación registrada) se tiene que comportar como `entmod`. Lo mismo, en la ruta del piloto (`EXECUTEFUNCTION`).

## 4. La vía (W) en .NET, medida con un build de diagnóstico (~08:15Z a 08:35Z)

- **Build desechable:**
  - worktree `<worktree-diag3>` (detached `e67fb511`, sin commits ni rama), con UNA línea añadida en `src/RackCad.Plugin/Drawing/LateralHeaderDrawer.cs`, dentro del bucle que ya abría para escritura cada referencia de primer nivel y llamaba a `RecordGraphicsModified(true)` (`:167-171`): `reference.Position = reference.Position;`;
  - salida `<build-diag3>`, compilada con 0 advertencias y 0 errores. El Plugin cargado es `<build-diag3>\RackCad.Plugin.dll`, con SHA-256 `E2DD7BB8…2F0B`;
  - copia desechable `chk-2.dwg`, cerrada SIN guardar (el SHA-256 no cambia).
- **Por qué no bastaba lo que ya había:** el mapa (`F4-DRAFTS/c2/map.md`) muestra que `RedefineSystemBlock` YA abría las referencias para escritura y llamaba a `RecordGraphicsModified(true)`, y S2-03 ocurría igual. Abrir para escritura sin modificar nada, más `RecordGraphicsModified`, no deja ningún registro de Undo de la referencia. El `entmod` de LISP y el setter de `Position` sí lo dejan.

**Resultados:**

| Ruta | `U` | `REDO` | Después |
|---|---|---|---|
| Panel, escala 1 → 4 (1 secuencia) | `EXECUTEFUNCTION` deshecho, y **la pantalla vuelve sola** al rótulo pequeño, sin `REGEN` | «Everything has been redone», y **la pantalla vuelve sola** al rótulo grande | «Comprobar ahora» da `Limpio` + «Sin cambios…»; rótulos `PB G` planta=1 y lateral=1 |
| `RACKEDITAR` clásico (4 secuencias) | deshecho en las 4. **La pantalla vuelve sola** en las 3 en que se capturó (1.ª, 2.ª y 4.ª); en la 3.ª no se capturó la pantalla | «Everything has been redone» en 3 de 4 (2.ª, 3.ª y 4.ª), con el refresco capturado en la 2.ª. En la 1.ª, «Nothing to redo» | — |

- **El estado del panel justo después del «Actualizar» con el build W** (captura ~25 s después del clic; el Actualizar tarda ~14 s): `Limpio` + «Actualizado: el cambio se aplicó al dibujo.», sin la marca, y la pestaña sin `*`. Es el mismo resultado que con C1. Solo hay una captura, sin seguimiento a los 5 s.
- **Lo que hubo entre `U` y `REDO` en el clásico:**
  - 1.ª secuencia, la que falló: un movimiento del ratón sobre el dibujo y dos capturas, unos 15 s;
  - 2.ª: nada, unos 6 s;
  - 3.ª: dos movimientos del ratón sobre el dibujo y un clic en la línea de órdenes, unos 9 s;
  - 4.ª: un movimiento del ratón y una captura, unos 7 s.
  - **Diferencias de la 1.ª:** fue el primer `RACKEDITAR` de esa sesión de AutoCAD, justo después de un «Comprobar ahora» y de un LISP de lectura. Su designación falló una vez («Nothing Selected.») antes de la buena.
- **Observación del «Nothing to redo»** (1 de 4 `REDO` en el clásico): no se reprodujo al repetir la secuencia. Hipótesis sin confirmar: una relectura del panel en el drenaje, que vacía la pila de Rehacer (O-18, ya registrado y fuera de T4C). Sin el arreglo, con el DLL de C1, hay 1 de 1 `REDO` correcto en el clásico: la muestra no basta para atribuirlo al arreglo ni para descartarlo.
- **Sin errores:** el registro de RackCad del día no existe y no hay eventos de error de `acad.exe`. AutoCAD se cerró y no queda ninguna `acad.exe`. I-52 recibió el aviso de cierre.

**Conclusión:** la vía (W) corrige F4-S2-03 en el panel y en `RACKEDITAR` clásico de Push Back, con una línea en la primitiva compartida y sin tocar el Freeze.
- **A qué afecta:** a los redibujos que usan `SystemBlockWriter` → `RedefineSystemBlock`. Solo se midió en Push Back.
- **A qué no:** Cantilever redefine con su propio bucle (`CantileverViewMaterializer.cs:83-87`), así que no queda cubierto (revisión independiente, I-1).
