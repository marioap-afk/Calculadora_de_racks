Decisión del Owner sobre la parada del Worker del intento 1 de F4-T4A1-PREPARE-CORE (2026-11-14), por la herramienta de preguntas de la sesión. Pregunta, opciones y respuesta literales:

Pregunta: «F4-T4A1: el Worker se detuvo con BLOCKED antes del RED (paradas C01/C02, sin commits). Dos pruebas existentes fuera del alcance tendrían que cambiar y mi paquete no las mapeó: WorkspaceDraftRegistryTests.cs:135, que solo admite CheckNow con una StaleBase sola, y CustomPropertiesEnvelopeTests.cs:708, que fija 10 llamadas a Compose y pasarían a 11. El Worker dejó la implementación completa en un parche: Core 18371/18373 (los 2 fallos son esas pruebas), UI 2112 sin fallos y el Plugin compila. ¿Cómo sigo?»

Opciones:
- «Corrección, intento 2 (Recommended)»: Contrato del intento 2 igual al del 1 más esas 2 pruebas en el alcance, con sus cambios declarados (ConfirmBase exento como CheckNow; el censo de 10 a 11). Además, el diagnóstico de admisión de TryConfirmBase queda declarado en la guarda del uso de la base, sin el rodeo del nombre de variable. Planificación y Worker nuevos, y el Worker puede partir del parche del intento 1. attempts pasa a 2 de 3; el intento 1 se conserva como historia.
- «Reemisión sin gastar intento»: Tratar el STOP como defecto de entrada de la sesión: reemitir el contrato del intento 1 con el alcance corregido, con nueva planificación y nuevo Worker, y attempts sigue en 1 de 3. Los precedentes de no consumir son de planificación y verificación (F4-E14, F4-E25); nunca se ha aplicado a un Worker.

Respuesta: «Corrección, intento 2 (Recommended)».
