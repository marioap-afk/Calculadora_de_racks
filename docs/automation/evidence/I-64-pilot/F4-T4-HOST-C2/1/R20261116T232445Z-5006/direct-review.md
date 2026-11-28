# F4-T4-HOST-C2: revisión directa de la sesión

**Objeto:** `git diff dcf3396f 9abc1320` (RED `fe165a63`, GREEN `9abc1320`; WorkRunId `R20261116T125319Z-3ea9`).

**Veredicto:** ACEPTABLE CON MENORES. **0 B / 0 I / 3 m.**

## Alcance

`git diff --name-only dcf3396f 9abc1320` da 2 rutas, exactamente las 2 del AllowedWriteScope: `src/RackCad.Plugin/Drawing/LateralHeaderDrawer.cs` (M) y `tests/RackCad.Tests/RedefinedViewReferenceUndoTouchGuardTests.cs` (A). Ninguna cae bajo el ForbiddenWriteScope y `Drawing/Cantilever/` no cambia. El RED (`dcf3396f..fe165a63`) no toca `src/` (diff vacío) y el GREEN (`fe165a63..9abc1320`) no toca `tests/`.

## Producto, frente a D-C2-01 e INV-C2-01

`git diff fe165a63 9abc1320 -- src` son exactamente dos líneas añadidas en el bucle `foreach (ObjectId referenceId in systemDef.GetBlockReferenceIds(directOnly: true, forceValidity: true))` de `RedefineSystemBlock`, entre `var reference = (BlockReference)tr.GetObject(referenceId, OpenMode.ForWrite);` y `reference.RecordGraphicsModified(true);`, con 16 espacios:
- el comentario, byte a byte el de D-C2-01;
- `reference.Position = reference.Position;`.

Ninguna otra línea cambia. No hay `Regen`, evento, suscripción, pista ni tratamiento de `U`/`UNDO`/`REDO`/`MREDO` (INV-C2-02).

## La guarda nueva, frente a D-C2-07 e INV-C2-05

`RedefinedViewReferenceUndoTouchGuardTests`: clase pública sellada con exactamente 4 `[Fact]`. Lee el código sin comentarios con `I55ProductCharacterizationTestSupport.Code` y recorta el método con `Body` (desde su `{` de apertura), sin cambiar el soporte.

| Caso | D-C2-07 | Aplicado | Discrimina |
|---|---|---|---|
| 1 | dentro del bucle de `GetBlockReferenceIds(directOnly: true`, tras la apertura `ForWrite` de la misma variable | regex con grupos `id` y `ref`; entre la apertura del bucle y la sentencia, `[^}]*?` (sin llave de cierre, así que dentro del mismo cuerpo) | falla si la sentencia falta, está fuera del bucle, antes de la apertura o sobre otra variable |
| 2 | exactamente una vez en el cuerpo | `Assert.Single(TouchStatement.Matches(body))`, con retroreferencia `\1` (misma variable a ambos lados) | falla con 0 o con 2 |
| 3 | autoprueba: quitarla | quita la sentencia, comprueba que el texto cambió y que el caso 1 la rechaza | sí, sobre el texto real |
| 4 | autoprueba: moverla fuera del bucle | la quita e inserta tras la `{` del método; comprueba el cambio y el rechazo | sí; en el RED solo inserta |

No fija `RecordGraphicsModified(true)` ni el orden respecto de él. Contención (INV-C2-06): solo lee archivos del repositorio; no escribe, ni carga el Plugin o AutoCAD.

## RED y CI

- **CI del RED** `43971230759` (`fe165a63`): Core en failure, UI en success, build-ui en success y build-plugin omitido. En el TRX de Core fallan solo 3 casos de la clase nueva, los tres por aserción (`Assert.NotEqual`, el mensaje del caso 1 y `Assert.Single` con la colección vacía); fuera de la clase, ningún fallo (19064 de 19067).
- **CI del GREEN** `43972738540` (`9abc1320`): 4/4 en success. La clase nueva da 4 de 4; Core Workspace 3482 de 3482; Core completo 19067 de 19067; UI Workspace 694 de 694.
- **Locales del Worker:** RED `red0..red28`, `redu1` y `redu2`; GREEN `g0..g28`, `gu1`, `gu2` y `gfull` (19067 de 19067); Release `h0..h28` y `h18b`.

## Menores

- **m-1 (el RED de la autoprueba «quitarla»):** en el RED falla por aserción (`Assert.NotEqual`: no hay nada que quitar). El paquete dice que las autopruebas «pueden» pasar en el RED, no que deban. Es un caso que exige la sentencia y falla por aserción: conforme a INV-C2-05. Se declara.
- **m-2 (h18 en Release):** el Worker ejecutó `SingleScanInsertGuardTests` en Release desde el DLL de una carpeta bajo `artifacts/` y dio 5 fallos de `DirectoryNotFoundException`. La prueba localiza la raíz del repositorio subiendo un número fijo de directorios desde el binario, así que el fallo es ajeno al producto. Se repitió por el proyecto con `-c Release` (`h18b`, 5 de 5) y la CI la pasa. Se declara la corrida y su sustitución.
- **m-3 (Core Workspace 3482 frente al mínimo 3468):** el contrato fija un mínimo (`MinSelected`), no un número exacto. La CI del GREEN de C1 (`43801607010`) ya seleccionó 3482 con ese filtro (paquete de cierre de C1, §2), igual que ahora. El 3468 lo midió la sesión al preparar el paquete, contando en el TRX los resultados por `testId`: así, las filas de teoría que comparten id cuentan una sola vez. Es un error menor del paquete (§4, D-C2-07: «CI del GREEN de C1»): el mínimo queda 14 por debajo de lo seleccionado, así que la comprobación de `MinSelected` no detectaría la pérdida de hasta 14 casos. Aquí no se perdió ninguno: las 3482 pasan en el RED y en el GREEN, igual que en C1.

## Incidente de la cesión (declarado por el Worker)

El equipo entró en espera moderna a las 13:06:51Z y en suspensión a las 16:23Z, pese al keep-awake por `SetThreadExecutionState(SYSTEM_REQUIRED)`, y despertó a las 22:31Z. El Worker se cortó (sin progreso durante 600 s) entre la validación local del RED y su commit, con la prueba sin seguimiento y sin commits. La sesión inspeccionó el estado (git, la carpeta de la corrida y los procesos) y reanudó el MISMO Worker por mensaje, con el mismo WorkRunId. El Worker descartó las corridas previas al corte y las repitió después del commit RED. La entrada del relevo tiene 0 participantes. La sesión pasó a mantener también la pantalla (`DISPLAY_REQUIRED`) y pidió el keep-awake de la app.

## Lo que no se puede verificar sin AutoCAD

Que `U` y `REDO` refresquen la pantalla (H-06, Smoke-2). R-1 (los demás sistemas) va a la OV final de I-64, por la decisión del Owner. R-4 (Cantilever) queda declarado.
