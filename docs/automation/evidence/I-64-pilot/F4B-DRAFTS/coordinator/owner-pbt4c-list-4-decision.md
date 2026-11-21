# Decisión del Owner sobre PB-T4c (I-64 F4B): una cuarta prueba fuera de la lista

**Fecha:** 2026-11-21. Después de `owner-pbt4c-list-3-decision.md`, que dejó la lista de PB-T4c en 3.

**Por qué se preguntó:** la precondición 6 de la v5.0, sobre copias exportadas del cierre de PB-T4b (`6adff90d`), corrió la suite completa de Core y los 17 filtros de UI. En el GREEN cae, en la UI, `PushBackWorkspaceApplyCloseTests.CT_PBT2B_06_ClosingAsksOnlyForWhatWasNotApplied`, que no está en la lista de 3.
- **Lo que fija:** su ventana 4 deja pendientes a la vez un cambio de módulo y cambios sin aplicar. La prueba exige que, al cerrar, mande primero la pregunta del módulo.
- **Por qué cae:** el GREEN cumple la decisión del Owner A-T2b-OWNER-01 (F4B-E38, «Corregir en PB-T4 (Recomendado)», `owner-pbt2b-close-decisions.md`). Con las dos cosas pendientes, la pregunta pasa a ser «Hay un cambio de módulo sin confirmar y cambios sin aplicar…».
- **El supuesto que falló:** el paquete daba por hecho que ninguna prueba fijaba ese caso, y esta sí lo fija.

En la misma corrida cayó también `WorkspaceModelBoundaryTests.ThereIsNoPerTabStateOrPerTabSubscriptions`, porque el nombre `IWorkspaceFullEditorHost` casa con su expresión. La resuelve la sesión sin tocar la lista: es un cambio de nombre (D-S9 en `session-pbt4c-design-decisions.md`).

Los mensajes completos están en `pbt4ccc/compile/out-redgreen-v50-r5c/`.

La sesión preguntó con AskUserQuestion. Este registro copia el texto exacto. La numeración F4B-E se asigna al custodiarlo.

## Pregunta (encabezado «Lista T4c»)

**Pregunta, literal:**

> PB-T4c: al correr la suite completa sobre el código nuevo cae otra prueba fuera de la lista de 3, CT-PBT2B-06 («al cerrar, solo pregunta por lo que no se aplicó»). Fija que, si hay a la vez un cambio de módulo sin confirmar y cambios sin aplicar, al cerrar la ventana manda primero la pregunta del módulo. Eso es justo lo que tú mandaste corregir en PB-T4 (A-T2b-OWNER-01, «Corregir en PB-T4»): ahora la pregunta dice «Hay un cambio de módulo sin confirmar y cambios sin aplicar…». El paquete suponía que ninguna prueba fijaba ese caso, y esta lo fija. (La otra prueba que cayó la resuelvo yo sin tocar la lista: es solo un cambio de nombre de una interfaz nueva.) ¿Cómo sigo?

**Opciones:**
1. **Añadirla a la lista (Recomendado):** La lista de PB-T4c pasa de 3 a 4. CT-PBT2B-06 se salta con registro y su equivalente es su copia literal con solo la línea de ese caso cambiada a la pregunta nueva. Si aparece otra fuera de la lista, te vuelve.
2. **Preguntar distinto:** Que la pregunta de las dos cosas solo salga cuando los cambios sin aplicar no vengan del propio módulo; así esa prueba sigue en verde. Reinterpreta tu decisión de PB-T2b. No lo recomiendo.
3. **No cambiar «Cerrar» ahora:** «Cerrar» no se corrige en PB-T4c y la prueba sigue como está. Va contra tu decisión de corregirlo en PB-T4; quedaría para otra tarea.

**Respuesta del Owner, literal:** «Añadirla a la lista (Recomendado)».

## Lo que queda decidido

- **La lista de PB-T4c pasa de 3 a 4:** PEG:155-163, CT-PBT1B-15, CT-PBT4B-20 y CT-PBT2B-06 (`PushBackWorkspaceApplyCloseTests`).
  - CT-PBT2B-06 se salta con registro.
  - Su equivalente es su copia literal en una clase de UI nueva. Solo cambia la línea de la ventana 4, a la pregunta nueva.
- **Es la lista que vio el Owner** para la condición de emisión de `owner-pbt4-decisions.md`, pregunta 4. Si aparece otra prueba que falle fuera de ella, vuelve al Owner.
