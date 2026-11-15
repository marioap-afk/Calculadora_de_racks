# I-64 F4-T1B: disposición del Owner antes del cierre del intento 1 (2026-11-14)

La sesión la pidió en el chat (AskUserQuestion), después de la verificación `R20261114T233845Z-3df7` (EXECUTION_VERIFIED, 14/14) y de nc1..nc3. Llevaba el análisis `analysis.md`, que se custodia con las revisiones. Se transcriben las preguntas y las respuestas literales.

**Pregunta 1 (I-01):** «F4-T1B I-01: la entrega del Worker no declara como «ancla» unas 11 aserciones sin mutante propio, y dos de las declaradas son inexactas. Todas están en métodos que algún mutante de la tabla detecta. Leída al pie de la letra, la parada F4T1B-C04 (3.er caso) se activa; el Controller no la activó (14/14). ¿Cómo la resuelvo?»

**Respuesta:** «Adenda y cerrar (Recommended)». La descripción de la opción: «Cierro el intento 1 con una adenda de la sesión en la custodia: la lista completa de anclas medida por la revisión independiente. Tú declaras que C04 (3) no se activa porque toda aserción va en un método detectado.»

**Pregunta 2 (I-02):** «F4-T1B I-02: la suite de UI existente abre el configurador real y reescribe la fecha de su archivo de disposición en el perfil real (mismo contenido). Mi prompt exigía suites completas en local, y el Worker no se paró. Las pruebas de T1B no escriben. ¿Qué disposición?»

**Respuesta:** «Ratificar + regla (Recommended)». La descripción de la opción: «Ratificas que T1B no escribe, se registra como desviación de la delegación causada por el prompt, y queda la regla: nunca suites completas de UI en local con contención de perfil, solo en la CI. La escritura previa de la suite queda como deuda aparte.»

**Pregunta 3 (I-03):** «F4-T1B I-03: una sonda temporal de medición del Worker disparó «Insertar varias…» con entradas válidas sobre una ventana sin mostrar. Lanzó antes de abrir el diálogo, no escribió nada y no llegó a ningún commit. Contradice la letra de INV-F4T1B-06. ¿Qué disposición?»

**Respuesta:** «Ratificar contenida (Recommended)». La descripción de la opción: «Se acepta como desviación contenida y declarada. Los prompts futuros prohíben las sondas con entradas válidas en caminos con diálogo o escritura.»
