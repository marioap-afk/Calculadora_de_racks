# F4-T2B1-INTENT-READ, intento 1: análisis de las revisiones y corrección

**Entrega:** Worker `R20261112T081315Z-4a75`, RED `04717913`, GREEN `23792b2d`; CI `43184046954` 4/4.
**Verificación del Controller** `R20261112T084307Z-16bc`: `EXECUTION_VERIFIED`, 14/14, sin hallazgos.

## Revisión directa de la sesión

Sin defectos importantes. El lector sigue la tabla de las 14 reglas; las constantes coinciden con el paquete; la entrada copia sus colecciones y comprueba las precondiciones; las guardas nuevas tienen pruebas de inyección. Una observación: la regla 5 del paquete, leída al pie de la letra («o el origen no quedó clasificado Redraw ni Erase»), haría inalcanzable la regla 6 para un origen xref con el Id correcto; el Worker deja pasar ReadOnly a la regla 6, que es la lectura coherente con la existencia de la regla 6.

## Revisión adversarial independiente

**Veredicto:** NO ACEPTABLE, solo por cobertura de pruebas; ningún defecto importante de producto.

| Id | Severidad | Hallazgo | Tipo |
|---|---|---|---|
| I-1 | IMPORTANTE | La regla 13 no tiene prueba. Es alcanzable: el canónico de AUTH-13 no valida lo que `RackProjectStore` sí valida (por ejemplo, una altura de tarima 0), así que AUTH-13 da Single y `Materialize` da Failed con `UnreadableOrFutureDiagnostic`. Sin la prueba, quitar la regla 13 no hace fallar nada y rompe INV-17 | Pruebas |
| I-2 | IMPORTANTE | Ninguna prueba ejercita el `catch` de `Read` para una excepción del comparador. Un `Design` con un surrogate escapado suelto pasa la deserialización tolerante y hace lanzar `InvalidOperationException` al lector estricto, que solo captura `JsonException` | Pruebas |
| M-1 | MENOR | `Rule5ReadsTheFactAndNotTheDeserializedEnvelope` no discrimina; falta el caso inverso (hecho de `Rack`, sobre con otro Id → `UnreadableDiagnostic` con el detalle del comparador) | Pruebas |
| M-2 | MENOR | La condición `EnvelopeInterpretable` de la regla 3 no tiene prueba (hecho no interpretable con texto válido) | Pruebas |
| M-3 | MENOR | El testigo da `Unchanged` con el origen ausente de los hechos si su texto está en `RawEnvelopes`; D-T2B-05 dice «origen ausente → Changed» | Producto |
| M-4 | MENOR | La guarda de hilos es más débil que su precedente (`WorkspaceDraftBoundaryTests.cs:259`: `Task\.`, `ValueTask`) | Pruebas |
| M-5 | MENOR | `RawEnvelopes` expone el `Dictionary` interno: un llamador puede convertirlo y mutar una entrada declarada inmutable | Producto |
| M-6 | MENOR | La regla 5 se interpreta distinto de su letra sin registrarlo; las comprobaciones defensivas inalcanzables no se marcan | Registro |
| M-7 | MENOR | La regla 9 da `Detail = ""` con un Kind null; la documentación dice null cuando no hay detalle | Producto |
| M-8 | MENOR | Falta la prueba «Design crudo distinto» de D-T2B-08; probablemente inalcanzable vía `Read`, porque `RawDesign` sale del mismo texto | Registro |
| M-9 | MENOR | `Rule9ComesBeforeRule10` prueba en realidad «9 antes de AUTH-13» (par 9,11); el par (9,10) es imposible con `PushBack()` | Pruebas |
| M-10 | MENOR | `m.Embed!.Design!` afirma no-nulo sobre un valor que puede ser null; una aserción de `StaleBaseTests` se presenta como discriminante y pasaría contra el esqueleto del RED | Calidad |

## Disposición

La verificación 14/14 es necesaria pero no suficiente: la revisión independiente es obligatoria (F4-E02) y su veredicto impide cerrar. La sesión decide, por la delegación nocturna del Owner:
- **Intento de corrección 2** de F4-T2B1 (`attempts` 2 de 3), con un contrato del intento 2 que solo cambia y añade invariantes.
- **RED de la corrección**, sobre el producto del GREEN `23792b2d`: las pruebas de I-1, I-2, M-1, M-2, M-4, M-9 y M-10 (pueden pasar ya: cubren comportamiento correcto que no estaba fijado) y las de M-3, M-5 y M-7, que fallan por aserción contra el producto actual.
- **GREEN de la corrección:** solo producto (M-3, M-5, M-7 y el `!` de M-10); ningún archivo de `tests/`.
- **M-6:** se registra la lectura de la regla 5 (Id distinto, o clasificación NotMember o Blocking; un ReadOnly con el Id correcto pasa a la regla 6) y las comprobaciones defensivas se marcan como tales en el código.
- **M-8:** se registra como limitación aceptada; no se exige una prueba inalcanzable.
- **CorrectionOf** = esta verificación, con la clase `Tests` (el hallazgo es de cobertura de las pruebas que el contrato exige); este archivo es el análisis.
