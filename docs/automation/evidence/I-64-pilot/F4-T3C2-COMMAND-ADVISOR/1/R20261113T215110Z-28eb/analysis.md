# Análisis de la sesión: revisiones de F4-T3C2-COMMAND-ADVISOR, intento 1

**Entradas:**
- la revisión directa (`direct-review.md`): producto conforme, sin hallazgos;
- la revisión adversarial independiente (`independent-review.md`): ACEPTABLE CON MENORES, 0/0/8.

Ninguna de las dos encuentra:
- un escenario de producto que falle;
- una escritura del dibujo;
- una lectura de la intención o algo de F4-T4;
- un cambio no declarado de pruebas existentes;
- una prohibición debilitada.

Los ocho menores son de precisión de guardas de texto, de autopruebas, de nombres y de documentación.

## Clasificación (M-1 a M-8 del informe independiente; el prefijo «C2-» los distingue de otras listas)

| Hallazgo | Naturaleza | ¿El producto es correcto? | Destino propuesto |
|---|---|---|---|
| C2-M-1 | La guarda de `GetKeywords` no fija que verdadero exija `PromptStatus.OK` **y** `"Si"`: pasaría un mapeo invertido | Sí (`WorkspacePanelCommand.cs:69`) | F4-T4: reforzar la guarda (expresión del `return` y autopruebas con `"No"` y con `\|\|`). Smoke-2 comprueba Intro, Esc y Sí con el host real |
| C2-M-2 | La guarda del cuerpo de `RackPanelDiscard` solo exige `.Authorize(`: pasaría un resultado ignorado o invertido | Sí (`WorkspacePanelCommand.cs:45-47`) | F4-T4: fijar `Authorize(new WorkspaceRequest(s)) == RequestDecision.Accepted` y los dos primeros argumentos de `Run`, con su autoprueba |
| C2-M-3 | Siete guardas nuevas sin autoprueba en negativo; una mira `SessionDestroyed` en lugar de `RememberLoss` | Sí. Las pruebas de comportamiento cubren las reglas | F4-T4: autopruebas y el cuerpo correcto |
| C2-M-4 | La guarda de los dos miembros de la interfaz del asesor congela «dos sentencias» para siempre | Sí. Es lo que pidió D-T3C2-10 para esta subtarea | F4-T4: cambio declarado previsible si añade un miembro; acotar su nombre a F4-T3C2 |
| C2-M-5 | Nombres de prueba y comentarios de guardas existentes que ya no describen lo que comprueban (dos comandos, tres excepciones, el envoltorio) | Sí | F4-T4 o F7: renombrar y documentar de forma declarada |
| C2-M-6 | La documentación «the one writer of the command line of the panel» exagera: `RackPanel` y `RackCommandSupport.Report` también escriben | Sí. G-12 se cumple | F4-T4: matizar la frase |
| C2-M-7 | La documentación R-4 del VM es imprecisa cuando la lectura del riesgo lanza (el aviso es el de `FailGesture`) | Sí | F4-T4: precisar la frase |
| C2-M-8 | Una teoría con tres casos idénticos (no, Intro y cancelar), que sobredeclara lo que prueba | Sí | F4-T4: un `Fact`. Intro y Esc son de Smoke-2 |

## Propuesta

Si la verificación del Controller y nc1..nc3 pasan, F4-T3C2 se cierra en el intento 1 como **PASS WITH LIMITATIONS**, sin gastar el intento 2 en estos menores.
- Es el patrón de F4-T3A, F4-T3B1, F4-T3B2 y F4-T3C1.
- La autorización del Owner y del Coordinator incluye la custodia y el cierre sin más aprobación para la ejecución ordinaria, y no reabre por menores arrastrados (como con F4-T3C1).
- Ninguno cambia el comportamiento exigido.
- Con T3C2, **F4-T3 queda completa** en el nivel de subtareas, así que los menores pasan a **F4-T4** como cambios declarados de su paquete.
  - C2-M-1 y C2-M-2 son los primeros: refuerzan la protección estática de «explicit discard only» y del destino inequívoco, hasta Smoke-2.

**Riesgos de Smoke-2 de la revisión independiente:**
- `GetKeywords` (Intro, Esc, Sí) con el host real;
- el foco con la paleta visible;
- la escritura en `BeginDocumentClose` y en `DocumentActivated`;
- el cambio de documento durante la pregunta;
- el panel tras un descarte por comando.

Se suman a la lista de Smoke-2.
