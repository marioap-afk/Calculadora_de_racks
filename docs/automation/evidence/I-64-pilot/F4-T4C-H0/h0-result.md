# I-64 F4-T4C-H0: resultado de la sonda de host

**Qué es:** la sonda de diagnóstico de D-T4C-16 (O-01, «H0 con dispensas»). No es una validación del producto. Smoke-2 sigue siendo la primera validación física, y nada de este resultado cuenta como evidencia de cumplimiento.

## Datos de la sesión (medidos por la sesión)

| Dato | Valor |
|---|---|
| Ventana | `2026-11-15T12:32:39Z` a `12:45` UTC, aproximadamente (`session-start-utc.txt` y `session-end-utc.txt`) |
| Host | AutoCAD 2025, `AcCoreMgd` 25.0.171, una sola `acad.exe` (0 antes y 0 después) |
| Arnés | Parche `h0-harness.patch` sobre `b15072ff`, en un worktree desechable y sin commit; build 2 revisado (APTO PARA OPERAR) |
| DLL cargados (comprobados por `harness.attach` y `T0.assembly`) | `RackCad.Plugin.dll` `611808426BAA9A02…`, `RackCad.Application.dll` `85027566…`, `RackCad.Domain.dll` `0033C322…` y `RackCad.UI.dll` `13778C81…`, los cuatro en `<salida>`, `folders=1` |
| Inyecciones | Ninguna: las tres variables ausentes en el proceso, en el usuario y en la máquina |
| Bundle y Autoloader | Sin bundle en `ApplicationPlugins` y sin restos en `Applications`, `Loaded` ni `AssemblyMap` (comprobado antes en lectura) |
| Biblioteca de bloques | SHA-256 `B4CA2248…` (la de `settings.json`) |
| Dibujo | Creado por la sesión con RACKPUSHBACK y RACKEDITAR: un rack «H0 sonda» con dos vistas enlazadas, lateral (`100E`) y planta (`2BDC`). Original SHA-256 `07ac27c3…`, igual antes y después. La sonda corrió sobre `h0-copia.dwg`, que se cerró **sin guardar** |
| Carga del DLL | `NETLOAD` con «Load Once». No se cambió ni el perfil, ni `TRUSTEDPATHS`, ni `SECURELOAD` |
| Registro | `rackcad-20261115.log` (UTC). Lleva 2192 líneas `[H0]`, `#1` a `#2192`, consecutivas y sin huecos:<br>- de `#1` a `#6`, en los primeros 1440 bytes: las trazas de bloqueo de RACKEDITAR al crear el dibujo;<br>- de `#7` a `#2192`, desde RACKPANEL: la sonda |
| Restos | Ningún autoguardado (`.sv$`) ni volcado nuevo de esta sesión |

## Las corridas

| Sonda | Qué | Resultado de la corrida |
|---|---|---|
| P1 | Contexto vacío, sin selección | Terminada; sin escritura |
| P2 | Contexto vacío, con el rack seleccionado | Terminada; **la selección se pierde** («Ninguno») |
| P3 | Actualizar con el borrador limpio | `NothingToApply`, con el bloqueo exterior tomado y liberado, sin escritura |
| P4 | Actualizar con la escala de anotación de 1 a 2 | **`Applied`**: dos `WriteUnit`, `Seal`, POST completo y sin avisos |
| P5 | Repetición de P4, tras recargar | **`Applied`**, con el mismo patrón |

## La tabla de D-T4C-16, fila por fila

| # | Medida | Resultado medido | Acción de la tabla |
|---|---|---|---|
| 1 | Momento del callback | **Diferido** en las 5 sondas. La llamada vuelve con `ExecutionResult.IsCompleted=False` y el callback corre después: P1 a los 49,6 ms, P2 a los 61,6, P3 a los 80,6, P4 a los 74,4 y P5 a los 145,8. La UI pinta 1 o 2 fotogramas mientras tanto (`ui.frames returnedAwaitingCallback=1..2`) | Ninguna. F-1 tolera los dos casos. **[H]→[M]: diferido** |
| 2 | ¿Se completa siempre? | **Sí**, en las 5. `OnCompleted` llega en el hilo de la UI y la continuación corre (`continuation.ran IsCompleted=True`) | Ninguna |
| 3 | ¿Llega una entrada en reposo **antes** del callback? | **Sí, en las 5.** Llegan dos `EnteringQuiescentState` entre `call.returned` y el `CommandWillStart` de `EXECUTEFUNCTION` | **C12.** Con la regla de D-T4C-02 («caduca con la primera entrada en reposo de la sesión si el callback no empezó»), el billete caducaría en **cada** Actualizar. Hay que rehacer la regla de la cota, por ejemplo contando desde el comando interno, o una A-n. **Vuelve al Owner antes de emitir** |
| 4 | ¿Los `*Cancel*` producen `CommandCancelled` o `CommandEnded` antes de `EXECUTEFUNCTION`? | **No.** En la línea de comandos salen dos `*Cancel*` por sonda, pero el puente solo recibe los dos `EnteringQuiescentState` de la fila 3, sin pista de marcado | Ninguna (V1 y V2 de O-02 no hacen falta) |
| 5 | Orden de los eventos | `CommandWillStart(EXECUTEFUNCTION)` → el callback (con el bloqueo exterior) → los `Object*` de la importación y de MUTATE (en el callback) → `Seal` → POST (purga, `SyncName`, regen y relectura, en el callback) → salida del callback → `CommandEnded(EXECUTEFUNCTION)` → `EnteringQuiescentState`. **`F4P:60` se cumple:** `CommandEnded` llega después de la relectura de POST. No hay comandos anidados | Ninguna: es lo que prevé D-T4C-09 |
| 6 | Un solo paso de Undo, Undo y Redo | **Un solo paso:** la lista de Deshacer muestra **una** entrada «Executefunction» por Actualizar. Un `U` deshace «EXECUTEFUNCTION» entero: «Comprobar ahora» da Conflicto en `100E` y en `2BDC`, y la recarga muestra la escala 1. Un `REDO` **inmediato** lo rehace entero: «Everything has been redone» y «Comprobar ahora» da «Sin cambios» | Ninguna. Smoke-2 lo confirma con el producto. Ver la observación O-H0-2 |
| 7 | Paso de Undo vacío de un `EXECUTEFUNCTION` sin escritura | **Existe.** P1, P2 y P3 dejan cada una una entrada «Executefunction» en Deshacer | Se declara para Smoke-2 y se informa al Owner |
| 8 | Estado al entrar en el callback | `MdiActiveDocument` = el admitido (`activeIsAdmitted=True`), `TopTransaction` nula, `CommandInProgress=EXECUTEFUNCTION`, `quiescent=False` | Ninguna |
| 9 | Reentrada de los bloqueos anidados | Funciona: `CaptureDrawing` (dos veces) y `SyncName` (dos claves) toman y liberan su bloqueo bajo el exterior (`lock.nested.*.held/released`, `lockAll=Write`) | Ninguna |
| 10 | Contexto de sincronización y continuación | Al pedir: `DispatcherSynchronizationContext`. La continuación corre en el hilo de la UI (`thread=1`, `onPanelThread=True`) | Ninguna |
| 11 | ¿Bombea WPF dentro del callback? | **No.** Las sondas `Background` y `Normal` corren **después** de salir del callback, en P3, P4 y P5 (`row11.*-ran … AFTER`), con `inCallback=0` fotogramas | Ninguna |
| 12 | `Debug.Assert` del drenaje | **No salta.** `drain.assert-precheck top=null` en todos los drenajes | Ninguna |

## Observaciones que no son filas

- **O-H0-1. La mutación propia hoy.** Tras cada Actualizar, el drenaje lleva `[InvalidateNavigationCaches, MarkDraftsPossiblyStale]`, y el campo queda «Limpio (posiblemente obsoleto)». Es la línea base sin el ámbito de T4C, y confirma la necesidad de D-T4C-09. `U` y `REDO`, que son comandos ajenos, también marcan, como deben.
- **O-H0-2. Las lecturas del panel vacían la pila de Rehacer.** «Comprobar ahora» y «Editar» dejan entradas «Group of commands» en Deshacer, y hacer un «Comprobar ahora» entre `U` y `REDO` da «Nothing to redo». Para Smoke-2: probar Redo inmediatamente después de Undo, y declarar este comportamiento de las lecturas del panel, que no es de T4C.
- **O-H0-3. La selección implícita.** `EXECUTEFUNCTION` vacía PICKFIRST (P2): es lo que se aceptó en O-15. Una selección del rack abre su pestaña en el navegador, y la pestaña se mantiene tras el vaciado.
- **O-H0-4. Un hecho del arnés.** `ExecuteInCommandContextAsync` devuelve `DocumentCollection.ExecutionResult` (`IsCompleted`, `OnCompleted` y `GetResult`, este último vacío), no un `Task`. **La guarda de D-T4C-04,** con su «una sola `ContinueWith`», hay que reescribirla para `OnCompleted` antes de T4C1.
- **O-H0-5. Lo que no se hizo:** el paso opcional 10 (sin fotogramas de WPF). Las filas 1 y 11 no lo necesitan: la fila 11 se decide con las dos sondas `row11.*`.

## Desviaciones del procedimiento (para el Owner)

- **El operador fue la sesión, no el Owner.** A la pregunta por el dibujo, el Owner respondió: «Crealo tú controlando autocad, no pidas autorización». La sesión creó el dibujo y operó también la sonda, siguiendo la guía, con control del escritorio. Todas las observaciones son de la sesión, con capturas de pantalla e historial de comandos. D-T4C-16 decía «el Owner opera».
- **El RELEASE de I-52** se pidió por el canal entre sesiones antes de abrir AutoCAD, pero no hubo respuesta: la sesión de I-52 estaba inactiva, sin ninguna `acad.exe` abierta y sin solapamiento posible. Se operó por la instrucción del Owner de no pedir autorización.
- **El dibujo** no es un dibujo real del Owner: lo creó la sesión con el producto clásico (RACKPUSHBACK y RACKEDITAR) sobre una plantilla vacía. Va en pulgadas por defecto de RackCad, con el aviso de INSUNITS, que no bloquea.

## Lo que vuelve al Owner

- **La fila 3 da C12.** La cota del billete «en vuelo» por la primera entrada en reposo (D-T4C-02 y R-01 de la revisión de la v1) **no sirve**: AutoCAD entra en reposo dos veces entre la vuelta de la llamada y el callback. Hace falta decidir una regla nueva antes de emitir T4C1. Las opciones las propone la sesión con el paquete corregido.
- **Hechos para el paquete:**
  - el callback es diferido;
  - un Actualizar es un solo paso de Undo, con Undo y Redo completos;
  - existe el paso vacío;
  - los bloqueos anidados funcionan;
  - no hay bombeo;
  - `ExecutionResult` no es un `Task`.
