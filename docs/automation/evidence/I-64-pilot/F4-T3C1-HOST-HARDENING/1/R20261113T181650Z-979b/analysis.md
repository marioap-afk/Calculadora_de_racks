# Análisis de la sesión: revisiones de F4-T3C1-HOST-HARDENING, intento 1

**Entradas:**
- la revisión directa (`direct-review.md`): producto conforme, sin hallazgos;
- la revisión adversarial independiente (`independent-review.md`): ACEPTABLE CON MENORES, 0/0/6.

Ninguna de las dos encuentra:
- un escenario de producto que falle;
- una escritura del dibujo;
- una lectura nueva de la intención;
- una costura o reflexión para alcanzar los `catch` inalcanzables;
- un cambio que debilite una prueba o guarda existente.

La revisión independiente comprobó por su cuenta los 28 literales de H-07 y las 33 firmas de M-5, aplicando la regla de extracción sobre la base. Comprobó también que ninguna guarda nueva prohíbe para siempre algo que F4-T3C2 necesita según el §6.

## Clasificación de los menores de la revisión independiente

R-1 a R-6 son los hallazgos 1 a 6 del informe. El prefijo R los distingue de los menores M-1 a M-7 de F4-T3B2.

| Hallazgo | Naturaleza | ¿Cambia el comportamiento exigido? | Destino propuesto |
|---|---|---|---|
| R-1 | Hueco de prueba: ninguna prueba ni guarda discrimina la quita del prefijo duplicado en `WhyOf` (Busy). El producto es correcto. | No | F4-T3C2: prueba de texto exacto de `WhyOf` (Busy con motivo y sin él). T3C2 ya toca el aviso del panel. |
| R-2 | Hueco de prueba: la prueba de `_confirmUnread` pasa por `CancelDiscard` y no discrimina que `Ask` lo quite sin cancelar. El producto es correcto (`Ask` lo reasigna siempre). | No | F4-T3C2: prueba de dos peticiones seguidas sin cancelar. |
| R-3 | Hueco de prueba: el «solo en el estado Error» del host no está discriminado, porque `ErrorReason` ya es null fuera de Error por la semántica de `DraftSession`. El VM sí filtra y está probado. | No (latente hasta F4-T4) | F4-T4, que es quien pone borradores en Error. Si T3C2 toca el host, guarda por cuerpo. |
| R-4 | Documentación: las de `TakeLossNotices` y `TakeCloseNotices` siguen diciendo «leaves the queue empty», y las de `RequestDocumentDiscard` y `ConfirmDiscard` no nombran el desconocido. | No | F4-T3C2: documentación del puerto, que es la base del envoltorio del asesor. |
| R-5 | Declarativo: el ayudante `View(...)` del doble gana un `errorReason` opcional no declarado. Es inocuo, no cambia la respuesta por defecto ni ninguna aserción, y está dentro del alcance. | No | Se declara en este cierre. |
| R-6 | Declarativo: faltaba en la lista del Worker la limitación «el desconocido no lleva motivo en el valor: va al registro», que sí está en D-T3C1-08 y en el código. Además, el campo mezcla idiomas hasta R-C14: los motivos del host llegan en español y los del lector del Plugin en inglés. | No | Se declara en este cierre. La mezcla de idiomas la cierra F4-T3C2 (R-C14). |

**Observación de la revisión, sin hallazgo:** `LiteralViolations` fija el número exacto de apariciones de cada texto en español en el host y en el soporte. Una reutilización legítima en esos dos archivos exigirá un cambio declarado de la guarda. No afecta a R-C5..R-C14, porque la lógica de T3C2 va a clases nuevas y la de R-C14 al Plugin. Se anota para el paquete de T3C2.

## Propuesta

Si la verificación del Controller y nc1..nc3 pasan, F4-T3C1 se cierra en el intento 1 como **PASS WITH LIMITATIONS**, sin gastar el intento 2 en estos menores.
- Es el patrón de F4-T3A, F4-T3B1, F4-T3B2 y F4-T2C.
- Ninguno cambia el comportamiento exigido.
- Tres son huecos de prueba sobre código correcto y uno es de documentación. Caben en F4-T3C2, que sigue en la misma área.
- Los dos declarativos quedan declarados en el paquete de cierre.

**Riesgos de Smoke-2 de la revisión independiente:**
- desplegar juntos `Application`, `UI` y `Plugin`, porque cambia la firma de `PilotFieldView`;
- recrear la paleta con un veto en cola;
- la mezcla de idiomas en el campo hasta T3C2;
- la fila del veto al cerrar el documento tras un veto;
- los textos largos en la paleta estrecha.

Pasan a la lista de Smoke-2. El desconocido, la retención con `GetField` que lanza y `ErrorReason` no se pueden ejercitar en el smoke: solo los cubren las pruebas y las guardas.
