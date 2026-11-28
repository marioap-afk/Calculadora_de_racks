# I-64 F4-T4B: disposición del Owner antes del cierre del intento 1 (2026-11-15)

La sesión la pidió en el chat (AskUserQuestion) después de la verificación `R20261115T035009Z-45f0` (EXECUTION_VERIFIED, 14/14) y de nc1..nc3. Con la pregunta envió el análisis `analysis.md`, que se custodia con las revisiones. Se transcriben las preguntas y las respuestas literales.

**Pregunta 1 (I-01):** «F4-T4B I-01 (importante para la revisión independiente): el GREEN añade al host tres textos nuevos (UpdateNotRunReason y los dos motivos de la puerta propia), cuando D-T4B-07 pide constantes de la corrida. El comportamiento no cambia y la guarda de los 30 literales pasa; el Worker solo declaró dos de los tres. ¿Cómo lo resuelvo?»

**Respuesta:** «Ratificar + a T4C (Recommended)».

**La descripción de la opción:** «Se cierra como desviación declarada, con una adenda en la custodia. Pasar los tres textos a constantes de la corrida y extender la guarda a UpdateGate va a F4-T4C, que vuelve a tocar el host.»

**Pregunta 2 (nc2):** «F4-T4B nc2: con la lista blanca recortada (sin IPushBackPilotUpdatePort.cs), el Controller dio EXECUTION_VERIFIED con Scope en pass. Vio las dos listas y no las comparó. No es la firma de PD-F4-01, así que el sustituto necesita tu decisión. La sesión ya midió que el alcance real cumple (21 de 23 rutas, ninguna prohibida). ¿Qué hago?»

**Respuesta:** «Sustituto + deuda (Recommended)».

**La descripción de la opción:** «Clono el sustituto determinista de lista blanca exacta, extendido a esta firma, y conservo la salida ordinaria. La debilidad del Controller queda como deuda nueva PD-F4-07.»

**Pregunta 3 (decisiones y menores):** «F4-T4B: ¿ratifico las decisiones del Worker que la tabla de frontera no fija? Son: un fallo de los nombres deseados da un aviso y POST sigue; una importación con Ok=false o nula da Preflight. También los menores: el registro de TopTransaction sustituido por la clasificación en POST; cualquier Error final responde ClosedInError; la letra de INV-14; y el resto. Los menores pasarían a F4-T4C o a ideas futuras.»

**Respuesta:** «Ratificar todo (Recommended)».

**La descripción de la opción:** «Se ratifican como interpretaciones declaradas y los menores se trasladan a F4-T4C o a ideas futuras. Con las otras dos respuestas, T4B se cierra como CLOSED / PASS WITH LIMITATIONS en el intento 1.»

**Las opciones que no se eligieron:**
- en la pregunta 1, «Corregir en intento 2»;
- en la 2, «Corregir prompt y repetir», «Repetir nc2 una vez» y «Limitación declarada»;
- en la 3, «Revisar antes».
