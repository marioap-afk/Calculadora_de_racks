# Disposición para la verificación de F4B-PBT4-EDIT-ALL-WINDOW: la segunda receta de CT-PBT4C-11

**Autoridad:** la decisión del Owner del 2026-11-21, «Ratificar como errata (Recomendado)». Es la pregunta 1, con su respuesta literal, del registro `owner-pbt4c-code-decisions.md`, que la sesión custodia con el cierre de PB-T4c. La pregunta se hizo después de la revisión independiente del código (I-01) y antes de esta verificación.

**El punto:**
- **Lo que pide el paquete:** el §5.3 de la v5.1, fila 11, pide para la condición 7 «la línea base resuelta del estado aplicado, sustituida por la de otro fixture».
- **Lo que hace la prueba:** `tests/RackCad.Tests/PushBackEditAllTests.cs`, en :589-600 de `7cc1076e`, sustituye `Editor.SideA.LastModuleReconciliation` por un objeto vacío creado con `RuntimeHelpers.GetUninitializedObject`. Es la misma forma que CT-PBT4B-10 de PB-T4b.
- **La línea esperada,** `sustituir-referencia estado=Applied isdirty=False`, no cambia.

**Lo que ratifica el Owner, y nada más:**
1. **La errata:** la segunda receta de CT-PBT4C-11, tal como está en la prueba, se acepta como errata del paquete, solo para este intento.
2. **Sin cambios de código:** no se cambia ningún archivo de `src/` ni de `tests/`.

**Lo que no cambia:**
- **El resto de CT-PBT4C-11** lo compruebas tú mismo, como siempre: sus demás recetas, sus líneas esperadas y su resultado en el RED y en el CurrentSha.
- **Cualquier otra desviación** de la entrega respecto del contrato se juzga con su letra. Esta disposición no la cubre.
- **La evidencia:** cita en tu Evidence lo que midas, y nombra esta disposición en la comprobación a la que afecte.
- **El alcance:** esta disposición no relaja ninguna otra comprobación.
