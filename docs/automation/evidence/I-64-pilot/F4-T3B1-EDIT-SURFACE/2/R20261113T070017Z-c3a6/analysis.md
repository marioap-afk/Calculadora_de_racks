# F4-T3B1-EDIT-SURFACE, intento 2 (corrección): análisis de las revisiones y disposición

**Planificación:** `R20261113T062027Z-4b5e`, sobre el contrato `gate-contract-attempt2.json`: fidelidad PASS a la primera en las ocho colecciones y A1-A8 8/8. Sin nc4 (solo en la primera delegación de la cadena).

**Entrega:**
- Worker `R20261113T062519Z-a807`: RED `40f5727c` y GREEN `1708e764`.
- CI del GREEN `43380342743`: 4/4.
- CI del RED `43380164657`: UI en rojo, con 20 de 352 en el filtro; Core en verde (2300/2300).

**Verificación del Controller:** ver la `controller-verification` de esta carpeta.

## Revisión directa de la sesión

**Producto conforme.** El bloqueante de teclado queda resuelto: sin `KeyDown` de burbuja, y solo Enter y Esc marcados en la caja editable. Los menores están corregidos. El informe está en `direct-review.md`.

## Re-revisión adversarial independiente

**Veredicto:** ACEPTABLE CON MENORES (0 bloqueantes, 0 importantes, 3 menores). El bloqueante y los menores 2, 3, 4, 5, 7 y 8 del intento 1, y la marca conservadora, quedan cerrados; el menor 6 se dispone a F4-T3C. El informe completo está en `independent-review.md`.

| Id | Hallazgo | Disposición |
|---|---|---|
| N-1 | El doble del puerto aplica el pendiente de un borrador no editable en `TabChange` (el host lo conservaría); la mitad de la prueba 6 que dice probar `Kept` no lo recorre, aunque otra prueba lo cubre en la UI | A F4-T3B2 (cambio declarado del doble y de la prueba) |
| N-2 | Space lo marca el propio `TextBox`; la prueba lo compara con un `TextBox` sin manejadores en lugar de afirmar «sin marcar» | Desviación declarada de la letra de INV-F4T3B1-04: correcta en sustancia |
| N-3 | Las repeticiones de una tecla mantenida sobre la caja en solo lectura se escriben una vez abierta la caja | A F4-T3B2, que toca el mismo código |

**Riesgo para Smoke-2:** las teclas que el `TextBox` no consume (Ctrl+Z o Ctrl+Y sin nada que deshacer, Ctrl+V vacío, teclas F, Alt o F10) pueden llegar a AutoCAD. El contrato del intento 2 ya deja esa comprobación (D-01) para Smoke-2.

## Disposición

Decisión de la sesión, delegada por la orden de autonomía del Owner (`delegated-f4t3b1-close-decision.md`): **cerrar** F4-T3B1 como PASS WITH LIMITATIONS sobre `1708e764`, tras nc1..nc3, con los menores N-1 y N-3 a F4-T3B2 y la desviación N-2 declarada.
