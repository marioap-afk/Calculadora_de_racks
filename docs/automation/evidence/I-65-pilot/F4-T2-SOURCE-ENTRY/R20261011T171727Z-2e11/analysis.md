# I-65 F4-T2-SOURCE-ENTRY — análisis del Coordinator tras el STOP del Worker (intento 1)

Coordinator I-65, 2026-10-11. Cadena `F4-T2-SOURCE-ENTRY`; delegación `R20261011T171258Z-689c` (Attempt 1); trabajo
`R20261011T171727Z-2e11`: `WorkerStatus` `BLOCKED`, `Disposition` `STOP`, `TriggeredStopConditions` C-09 y C-01; **sin commits** (`CurrentSha`
= `BaseSha` = `cd3fb69b`, árbol limpio, sin push).

## 1. Causa raíz

El contrato fijaba la entrada en `src/RackCad.Application/Systems/Cantilever/CantileverStructuralExport.cs` y le pedía deserializar los bytes
del diseño con el almacén de proyecto. Una guarda de arquitectura existente, `CantileverSourceGuardTests.TheStationHasNoPersistenceAndNoView`
(I-37C), prohíbe las subcadenas `RackProject`, `Document`, `Xrecord`, `ViewBuilder`, `Preview` y `RackSystemKind` en todo archivo de
`Systems/Cantilever`: la estación no depende de la persistencia ni de la vista. El defecto es del **contrato** (lo emitió el Coordinator sin
cruzarlo con esa guarda), no del Worker, que se detuvo correctamente sin debilitar la guarda ni salir del alcance.

## 2. Opciones

1. Eximir el archivo de la guarda por ruta exacta: debilita una frontera de arquitectura vigente. **Descartada.**
2. Mover la entrada fuera de `Systems/Cantilever`: lo neutral no puede referenciar `Systems` (G2) y el proveedor tampoco; la persistencia no
   debe conocer el análisis estructural. **Descartada.**
3. **Inyectar el decodificador:** la entrada recibe, junto con los bytes, una función que los decodifica a `CantileverLineDesign`; quien llama
   la aporta (el comando del Plugin, con el almacén de proyecto; las pruebas, igual). La entrada aplica esa función **a los mismos bytes** que
   entrega intactos a `StructuralScope`, así que A65-F1-08 (una sola lectura; los bytes hasheados son los deserializados) se mantiene, y la
   estación sigue sin persistencia. Es una concreción de la firma, no un cambio del Freeze.

## 3. Decisión (CD-I65-F4-11)

- Opción 3. El contrato de F4-T2 se **reemite** con esa concreción: la entrada recibe `rackId`, el kind del sobre, el índice de estación, los
  bytes del diseño, una función de decodificación de esos bytes, los bytes del archivo de cargas, el perfil y el catálogo; las excepciones de
  la función son `Unavailable`; el archivo de la entrada no contiene ninguna de las subcadenas de la guarda (constante local para el kind
  `cantilever`); todas las guardas de Cantilever siguen en verde **sin cambios**.
- **Corrección** de la cadena con `attempts` 1 → 2 (README §9: STOP resuelto por el Coordinator con un cambio del trabajo), `CorrectionOf`
  = este análisis; `ChainRedSha` `null` y `ChainRedFiles` vacío (no hubo RED); `ChainBaseSha` `cd3fb69b`.
- **DEV-I65-F4-04 (desviación de protocolo declarada):** la sesión aplica el STOP declarado por el Worker **sin** verificación del Controller,
  porque no hay entrega verificable (ningún commit; `CurrentSha` = `BaseSha` y árbol limpio, comprobados mecánicamente) y el tope de Codex del
  Owner queda ajustado (28: con esta decisión, F4 usa exactamente 28 si no hay más correcciones).
- El resto de la delegación (excepciones como `Unavailable`, rechazos del constructor, bloqueo de línea, lista de permiso de familias, cota de
  la re-ortogonalización, inventario de piezas) no cambia. Las otras dos colisiones que encontró el Worker (`Document` y `Interval`) se evitan
  dentro del alcance, como él mismo indicó.
