# I-64 F4B — Decisión del Owner sobre la parada PBT3A-C02 de PB-T3a (intento 1): renombrar ForWrite, dentro de F4B-E42

**Fecha:** 2026-11-20. **Canal:** el chat de la sesión principal, con el diálogo de preguntas de la aplicación. La pregunta y la respuesta son literales.

**La parada:** el Worker `R20261120T001738Z-7d92`, sobre la delegación `R20261120T001442Z-903c`, paró con PBT3A-C02 antes del commit RED, sin commits ni push. HEAD sigue en `18e4b783` y el árbol está limpio.
- **La prueba existente:** `WorkspacePilotUpdateIntegrationGuardTests.G9NoFileOfThePluginOrOfApplicationThatNamesThePilotUsesAWriteApiOrALockOutsideTheFourExceptionsWithTheirExactCounts` falla sobre el esqueleto.
- **La causa:** el esqueleto exacto del paquete v4.4 hace que `PushBackFullComposition.cs` nombre el piloto, porque usa `PushBackPilotMaterializer.NonPilotContentReason`, y que declare `ForWrite`, el nombre cerrado por D-PBT3-05. La guarda G9 trata `\bForWrite\b` como una API de escritura y la prohíbe en los archivos que nombran el piloto, salvo cuatro excepciones fijas.
- **Es un defecto del paquete,** no del Worker. El Worker dejó parches reutilizables: `wip-red-tree.patch`, `rr/skeleton.patch` y `rr/green-wip.patch`.

## Pregunta

> El Worker de PB-T3a paró antes del RED (PBT3A-C02): el paquete tiene un defecto, no el Worker. El esqueleto exacto que fija el paquete hace que el archivo nuevo PushBackFullComposition.cs nombre el piloto (usa PushBackPilotMaterializer.NonPilotContentReason) y declare el método ForWrite, cuyo nombre cerró el paquete (D-PBT3-05). Pero la guarda existente G9 prohíbe que un archivo que nombra el piloto use 'ForWrite' (lo trata como API de escritura de AutoCAD), salvo 4 excepciones fijas. No hay commits; el árbol está limpio y el Worker dejó parches reutilizables (RED y GREEN en borrador). Arreglarlo cambia el contrato, así que sería una corrección con reemisión, en el intento 2 de 3. ¿Cómo lo resuelvo?

**Opciones:**
- **«Renombrar ForWrite (Recomendado)»:** Una v4.5 cambia solo el nombre del método cerrado (por ejemplo, ForFullWrite) en PB-T3a y lo que de él dependa en PB-T3b. La guarda G9 no se toca. La v4.5 pasa su recomprobación, reemito PB-T3a y hago la corrección en el intento 2 reutilizando los parches del Worker.
- **«Añadir una excepción a G9»:** Se añade el archivo nuevo como quinta excepción de la guarda, con su recuento exacto. Cambia una prueba existente de seguridad y la afloja un poco. También pasa por una v4.5, la reemisión y el intento 2.
- **«No nombrar el piloto»:** El archivo nuevo deja de usar la constante del piloto y lleva su propio texto, duplicado. Cambia el esqueleto y duplica un literal. También pasa por una v4.5, la reemisión y el intento 2.

**Respuesta del Owner:** «Renombrar ForWrite (Recomendado)».

## Lo que implica

- **La v4.5 del paquete de PB-T3:**
  - renombra el miembro cerrado `ForWrite` de D-PBT3-05 y todo lo que dependa de él en PB-T3a y PB-T3b;
  - comprueba los archivos nuevos de PB-T3 frente a todas las guardas existentes, para que no haya otra colisión;
  - no toca G9 ni ninguna prueba existente.
- **La comprobación acotada de la v4.5:** antes de reemitir.
- **La reemisión del contrato de F4B-PBT3A-FULL-WRITE-MODEL,** con la v4.5, y **la corrección en el intento 2 de 3**, con el análisis de la sesión. La delegación nueva puede reutilizar los parches del Worker del intento 1, con el nombre cambiado.
- **Lo demás no cambia:** el diseño y D-1.
