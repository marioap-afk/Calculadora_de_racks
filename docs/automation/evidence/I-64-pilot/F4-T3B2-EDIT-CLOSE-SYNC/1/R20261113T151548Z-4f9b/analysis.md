# Análisis de la sesión: revisiones de F4-T3B2-EDIT-CLOSE-SYNC, intento 1

**Entradas:**
- la revisión directa (`direct-review.md`): producto conforme, con D-1 y D-2;
- la revisión adversarial independiente (`independent-review.md`): ACEPTABLE CON MENORES, 0/0/7.

Ninguna de las dos encuentra un defecto que cambie el comportamiento exigido en los caminos normales, una escritura del dibujo, una lectura de la intención ni un cambio no declarado de pruebas existentes.

## Clasificación de los menores

| Hallazgo | Naturaleza | Alcanzable hoy | Destino propuesto |
|---|---|---|---|
| M-1 | Letra de INV-F4T3B2-12: en el cierre limpio, una excepción del puerto no quita una confirmación pendiente de **otra** pestaña. Esa confirmación sigue siendo válida para su pestaña. | Solo con un puerto que lance en `DiscardRack` (el host no lanza) | F4-T3C (contención del panel) |
| M-2 | Texto: con solo inciertos, la confirmación del documento dice «de: .». Otros dos textos afirman lo que el VM no sabe cuando el puerto falla. | Sí, por la vía de reserva del host cuando falla un comparador | F4-T3C, junto al endurecimiento de `GetRacksAtRisk` (el mismo caso: desconocido frente a limpio) |
| M-3 | El `Present` del constructor del panel, con un veto en cola, llama a `IsSessionAlive` sin contención; la documentación de F3 de `browserSummary` dice «never when the panel is built». | Sí, al recrear la paleta con un veto en cola; en el Plugin es una búsqueda en memoria | F4-T3C (documentación y contención) y riesgo de Smoke-2 |
| M-4 | Precisión de las guardas: una llamada sin calificar tras `=>` o `?` cuenta como declaración | No; el código actual es correcto | F4-T3C (precisión de guardas, como los menores de T2C) |
| M-5 | Guardas sin autoprueba en negativo; la de la forma pública solo comprueba presencia | No | F4-T3C |
| M-6 | Dos guardas fijan para todo el panel (`KeyUp`, un solo `PreviewKeyDown=`, `IsDefault` e `IsCancel`) más de lo que pide la tarea | Afecta a F4-T3C (modal y foco) | F4-T3C: cambio declarado de esas guardas cuando lo necesite |
| M-7 | «Nunca `ContainFailure`» sin prueba ni guarda | No; el código actual es correcto | F4-T3C |
| D-1 | La guarda de las colas en el host fija cuatro archivos | Afecta a F4-T3C si nombra las colas en otro archivo | F4-T3C: cambio declarado |
| D-2 | `_launchKey` se conserva si el primer gesto no abre la caja | Inocuo | Smoke-2 (reserva de N-3) |

## Propuesta

Si la verificación del Controller y nc1..nc3 pasan, F4-T3B2 se cierra en el intento 1 como **PASS WITH LIMITATIONS**, sin gastar el intento 2 en estos menores.
- Es el patrón de F4-T3A, F4-T3B1 y F4-T2C.
- Ninguno cambia el comportamiento exigido.
- Los menores alcanzables (M-2 y M-3) caen dentro de lo que F4-T3C ya debe hacer: el endurecimiento del desconocido de `GetRacksAtRisk` y la integración del panel.

Los riesgos de Smoke-2 de la revisión independiente (foco tras «Cerrar», «Descartar» y «Cancelar»; la confirmación fuera de la vista; Enter y Space sobre los botones; el veto al cerrar un documento real; la recreación de la paleta con un veto en cola) pasan a la lista de Smoke-2.
