# I-64 F4-T4C: decisión del Owner sobre C12 de la sonda T4C-H0 (2026-11-15)

La sesión la pidió en el chat (AskUserQuestion) después de la sonda T4C-H0 (F4-E63, custodia `44fe029d`). Se transcriben la pregunta y la respuesta literales.

**Pregunta:** «F4-T4C-H0 dio C12 en la fila 3. AutoCAD entra en reposo (EnteringQuiescentState) dos veces entre la vuelta de ExecuteInCommandContextAsync y su callback, en las 5 sondas. Con la regla de D-T4C-02 (el billete «en vuelo» caduca con la primera entrada en reposo si el callback no empezó), cada Actualizar caducaría antes de correr. También se midió que la llamada se completa siempre (5/5). ¿Qué regla de cota adopto para el billete?»

**Respuesta:** «Completar o comando ajeno (Recommended)».

**La descripción de la opción:** «El billete se libera con la compleción de ExecutionResult (OnCompleted, que H0 vio siempre). Como respaldo caduca con el primer CommandWillStart de la sesión de un comando que no sea EXECUTEFUNCTION si el callback no empezó, y con el cierre del documento o la retirada de la sesión. Las entradas en reposo dejan de contar.»

**Las opciones que no se eligieron:**
- «Contar desde el comando interno»: la caducidad por reposo solo cuenta después del `CommandWillStart` y el `CommandEnded` de `EXECUTEFUNCTION`;
- «A-n: comando registrado»: abandonar `ExecuteInCommandContextAsync`.
