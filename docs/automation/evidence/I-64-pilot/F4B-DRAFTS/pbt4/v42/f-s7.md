## 7. Riesgos y lecciones aplicadas

Los de la v1 (R-T4-01 a 15) y la v4 (R-T4-16 a 21), con lo que hacen la v4.1 y la v4.2:

| Riesgo | Qué hace la v4.2 |
|---|---|
| R-T4-01, la lista | Las listas, guardadas método a método, con su causa y su destino; la de PB-T4b, fijada por sha256 y aprobada por el Owner (470) |
| R-T4-08, los miembros por defecto | Solo `TryEditAll`, con sus dos reenvíos explícitos (CT-PBT4C-12 y 13) |
| R-T4-13, el modal y el abridor | Dos piezas; CT-PBT4C-08 fija el literal de la conversión (M-30) |
| R-T4-16, la doble lectura del piloto | Se mide en la precondición 6. En PB-T4a, la relectura en el origen repite las dos lecturas puras sobre el mismo escaneo, en `Draft/`, sin acceso nuevo al dibujo |
| R-T4-17, el coste de `IsDirty` | Sin cambio |
| R-T4-19, la duración de la suite de Core | Se mide en la precondición 6 de PB-T4b; si pasa de 15 min, vuelve a la sesión |
| R-T4-20, mover la validación numérica | `NumericFieldValidationTests` (UI) y CT-PBT4B-18; PBT4B-C20. El esqueleto de PB-T4b no toca `NumericFieldValidation.cs` (M-29) |
| R-T4-21, los riesgos del borrado real | Al Smoke-PB (§5.5) |
| R-T4-22, una guarda que fija una firma entera | EIG:139-141 fija la de `RackPilotIntentAdapter.Read`; `AtOrigin` lo evita |
| **R-T4-23, la retirada rompe por compilación** | Un `Skip` no evita un error de compilación. **El Owner la puso después de PB-T4c** (PB-T4d): PB-T4b no borra ni edita nada, y PB-T4d borra con registro, cuando ya no corre ninguna de esas pruebas (I-26) |
| R-T4-24, las copias en el RED | Partidas por su resultado en el RED, con una partición cerrada medida (§5.2) |
| R-T4-25, el léxico de las mediciones | Corregido otra vez en la v4.2 (`@"""…`); las listas no cambian |
| **R-T4-26. Una guarda que cuenta una llamada en todo un archivo** (B-08) | EIG:357 exige un solo `PushBackPilotIntentReader.Read(` en todo RIA, y la v4.1 lo medía solo en el cuerpo de `Read`. **La lección:** las guardas de los archivos tocados se recorren enteras, prueba a prueba (§4.1), no por selección; lo que no se pueda ver se declara con su parada |

