Pregunta de la sesión al Owner (AskUserQuestion, 2026-11-13), tras la revisión independiente del paquete v1 de F4-T3:

F4-T3: la revisión del paquete v1 dio EMITIBLE TRAS CORRECCIONES (0/9/12). Al incorporar sus correcciones, la parte del host crece: compuerta de reentrada en la recarga, defensa en ejecución contra lecturas disparadas desde avisos, cola de avisos de veto para el panel, invariante del texto vivo y los arrastres de T2B2B y T2C. La revisión advierte de que T3A puede evaluarse como Long-horizon, lo que dispara la parada C07, y pide avisarte de la división antes de emitir. Cada subtarea empieza en 1 de 3 intentos (F4-E09), así que dividir multiplica la capacidad total de intentos de T3. ¿Cómo divido F4-T3?

Opciones ofrecidas:
- Tres subtareas (Recomendado): T3A-EDIT-PORT: puerto de edición y su semántica, ámbito del modal, compuertas de reentrada y de avisos, cola de avisos de veto y arrastres de T2B2B y T2C. T3B-EDIT-UI: la UI del panel. T3C-CLOSE-DISCARD: RACKPANELDESCARTAR, mensajes del veto y de la pérdida en la línea de comandos y envoltorio del asesor (M-1). Orden A, B, C. Capacidad x3 (tres veces 1/3).
- Dos subtareas: T3A-EDIT-HOST con todo lo del host, incluidos el comando y los mensajes, y T3B-EDIT-UI con la UI. Capacidad x2, pero T3A queda grande y con riesgo de Long-horizon o de un Worker que no termine.
- Una sola tarea: F4-T3 completa en un único contrato (1/3), como dice la orden. Es el mayor riesgo de Long-horizon y de fallo del Worker.

Respuesta del Owner: Tres subtareas (Recomendado)
