# I-64 F4-T4C: disposición del Owner sobre el §9 del paquete v4 (2026-11-15)

La sesión la pidió en el chat (AskUserQuestion) después de la revisión independiente de la v1 (EMITIBLE TRAS CORRECCIONES, 0/12/16), de la re-revisión de la v2 (0/7/8), de la comprobación dirigida de la v3 (0/1/2) y de la v4 que aplica sus tres correcciones. Con la pregunta envió el paquete v4. Se transcriben las preguntas y las respuestas literales.

**Pregunta 1 (O-01):** «F4-T4C O-01: el «primer paso» de R-T4-05 (si el callback de ExecuteInCommandContextAsync es diferido, el paso único de Undo y el orden de los eventos) no está medido, y yo no opero AutoCAD. ¿Cómo se mide?»

**Respuesta:** «H0 con dispensas (Recommended)».

**La descripción de la opción:** «Sonda T4C-H0 antes del producto: un arnés Debug revisado en un worktree desechable, que operas tú sobre una copia del dibujo cerrada sin guardar, con su custodia y una tabla resultado → acción. Necesita tres dispensas: Smoke-2 sigue siendo la primera validación física, §12.1 sin build de hito y con el DLL del worktree desechable, y F4-E59.»

**Pregunta 2 (O-02):** «F4-T4C O-02: las pistas de AutoCAD no llevan rack, y tu orden exige un ámbito de mutación propia que «no pueda ocultar otro rack». ¿Qué vía?»

**Respuesta:** «Vía A + IC-09 (Recommended)».

**La descripción de la opción:** «Las pistas propias se registran aparte y solo invalidan cachés. Cierre con cota estructural, residual declarado (una escritura de un tercero sobre otro rack durante la ventana) y una IC-09 nueva. Respuestas preparadas según lo que mida H0: V2 o V1 por los *Cancel*, V3 por un cierre temprano, y «cerrar en Settle» (desviación de C-F4-04) solo si la admites; si no, A-n.»

**Pregunta 3 (O-03 y O-04):** «F4-T4C O-03 y O-04: el cableado guarda en el núcleo un ejecutor sin estado, y el escritor de T4B debe recibir el documento exacto y ser de un solo uso, lo que cambia un archivo y una guarda de T4B. ¿Qué combinación?»

**Respuesta:** «E-a + D-1 (Recommended)».

**La descripción de la opción:** «Un campo del núcleo con el ejecutor sin estado (no guarda documento, escritor ni corrida; tú decides si cuenta como autoridad persistente) y un escritor con constructor Document y marca de un solo uso. Cambio declarado de RackPilotUpdateWriter y de su guarda, protegido por C14, sin tocar MUTATE ni POST.»

**Pregunta 4 (O-05 a O-15):** «F4-T4C, el resto del §9 (O-05 a O-15): el aviso de C-F4-02 (ExecuteInCommandContextAsync en un solo archivo y solo para Actualizar, con el vaciado de PICKFIRST), el canal F-1, R-T4-07 con la cota del billete, la dispensa de UI local para Smoke-2, la división H0 + T4C1 + T4C2, el diagnóstico de T4A2, ClosedInError ampliado, el renombrado y la purga como POST, la definición de cancel, los avisos del gesto y aceptar PICKFIRST. ¿Acepto las recomendaciones?»

**Respuesta:** «Aceptar todas (Recommended)».

**La descripción de la opción:** «Se aceptan las recomendaciones de O-05 a O-15 tal como están en el §9 de la v4. Con las otras tres respuestas preparo la emisión de H0 (si la eliges), sin emitir T4C1 ni T4C2 hasta su resultado.»

**Las opciones que no se eligieron:**
- en la pregunta 1, «Sonda dentro de T4C1» y «Todo en Smoke-2»;
- en la 2, «Vía B (handle)» y «Revisar antes»;
- en la 3, «E-b + D-1» y «E-a + D-3»;
- en la 4, «Revisar una a una».
