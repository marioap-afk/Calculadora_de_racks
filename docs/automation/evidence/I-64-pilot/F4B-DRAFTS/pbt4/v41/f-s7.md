## 7. Riesgos y lecciones aplicadas

Los de la v1 (R-T4-01 a 15) y la v4 (R-T4-16 a 21), con lo que hace la v4.1:

| Riesgo | Qué hace la v4.1 |
|---|---|
| R-T4-01, la lista | Las listas, guardadas método a método, con su causa y su destino (§8.3). Firmas por método y ayudantes, a un nivel, y, para la retirada, por compilación transitiva |
| R-T4-08, los miembros por defecto | Solo `TryEditAll`, con sus dos reenvíos explícitos (CT-PBT4C-12 y 13) |
| R-T4-13, el modal y el abridor | Dos piezas; CT-PBT4C-08 fija el literal de la conversión (M-30) |
| R-T4-16, la doble lectura del piloto | Se mide en la precondición 6. En PB-T4a, la relectura en el origen repite las dos lecturas puras sobre el mismo escaneo, sin acceso nuevo al dibujo |
| R-T4-17, el coste de `IsDirty` | Sin cambio |
| R-T4-19, la duración de la suite de Core | Se mide en la precondición 6 de PB-T4b; si pasa de 15 min, vuelve a la sesión |
| R-T4-20, mover la validación numérica | `NumericFieldValidationTests` (UI) y CT-PBT4B-18; PBT4B-C20. El esqueleto de PB-T4b no toca `NumericFieldValidation.cs` (M-29) |
| R-T4-21, los riesgos del borrado real | Al Smoke-PB (§5.5) |
| **R-T4-22. Una guarda que fija una firma entera** (V41-06) | EIG:139-141 fija la de `RackPilotIntentAdapter.Read`. **La lección:** antes de añadir un parámetro, opcional o no, se buscan las guardas que fijan la firma completa; aquí, la relectura pura `AtOrigin` lo evita |
| **R-T4-23. La retirada rompe por compilación, no solo por texto** | Un `Skip` no evita un error de compilación: lo que nombra lo retirado, directa o transitivamente, deja de compilar. Medido con `retire_compile_pbt4.py`; vuelve al Owner (§8.3) |
| **R-T4-24. Las copias de «Saltar y duplicar» en el RED** | Su resultado es mixto en el RED; el punto abierto de §5.2 |
| **R-T4-25. El léxico de las mediciones** | Los guiones de la v4 cortaban mal los métodos con llaves dentro de cadenas. `lists_core.py` vacía cadenas, caracteres y comentarios antes de contar llaves, y los cuerpos de expresión acaban en el `;` de nivel cero. Por eso cambian algunos números de la v4 (§8.3) |

