Decisión del Owner sobre la estructura de F4-T3C (2026-11-13, tras el cierre de F4-T3B2 y el mapeo previo del código real), por la herramienta de preguntas de la sesión. Pregunta y respuesta literales:

Pregunta: «F4-T3C, medido sobre el código real: unos 28 archivos (Application, UI, Plugin y pruebas), con 6 guardas de censo de comandos que cambian de forma declarada. Incluye: comando RACKPANELDESCARTAR, envoltorio del asesor con mensajes en la línea de comandos, endurecer GetRacksAtRisk (un fallo hoy parece «nada en riesgo»), try en las colas, motivos del host en español y el motivo del Error cruzando el puerto, y los menores M-1..M-7 de T3B2. T3B, con ~22, la dividiste. ¿Cómo la estructuro?»

Opciones ofrecidas:
- «Dos subtareas (Recommended)»: T3C1-HOST-HARDENING (sin Plugin): GetRacksAtRisk con desconocido explícito y sus consumidores en la UI, try en las colas, SessionDestroyed, motivos en español, motivo del Error, menores de T3B2. Luego T3C2-COMMAND-ADVISOR: RACKPANELDESCARTAR, envoltorio del asesor, aviso del panel con la vía del comando, censos R-C3, M-1 de T2B2B. Cada una 1/3, en orden C1 → C2.
- «Una sola tarea»: F4-T3C entera en un contrato (~28 archivos), attempts 1/3. Más riesgo de copia en la planificación y de una revisión con muchos hallazgos.

Respuesta: «Dos subtareas (Recommended)».
