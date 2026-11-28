# Decisión del Owner sobre PB-T4b (I-64 F4B): la guarda de los textos del anfitrión, fuera de la lista

**Fecha:** 2026-11-20, después de `owner-pbt4b-list-477-decision.md` (la lista de 477).

**Por qué se preguntó:** la precondición 6 de la v4.7 (`b86ef9cb…`, sobre copias exportadas de `78a7cef3`) corrió la suite completa de Core. Con el GREEN literal cae `WorkspaceHostHardeningGuardTests.NoneOfThe30LiteralsInEnglishStaysInTheHostOrTheSupportAndTheFourIdentifiersAreKept`, que no está en la lista de 477. La guarda cuenta cuántas veces están exactamente los textos en español en `WorkspaceHostCore.cs` y `WorkspacePilotSupport.cs`, y el GREEN cambia tres cuentas:
- «La relectura no trajo resultado.»: de 2 a 0;
- «El borrador se está aplicando o ya no está registrado.»: de 5 a 6;
- «El rack no tiene escala de anotación.»: de 1 a 0.

Las tres salen del diseño ya decidido (D-PBT4-04 y D-PBT4-08, y la escala no anulable del modelo completo). Mantenerlas exigiría código muerto o textos cambiados solo para la guarda. El análisis medido está en `v47/analysis-outside-477.md`, punto 2.

En la misma vuelta cayeron otras dos cosas, que resolvió la sesión sin tocar la lista: la guarda de espacios de nombres (D-S6) y dos copias de WPP (D-S1, según D-PB-04 y D-PBT3-09 (e)). Las dos están en `session-pbt4b-design-decisions.md`.

La sesión preguntó con AskUserQuestion. Este registro copia el texto exacto. La numeración F4B-E se asigna al custodiarlo.

## Pregunta (encabezado «Guarda T4b»)

**Pregunta, literal:**

> PB-T4b: con el código nuevo cae una guarda que no está en la lista de 477. Esa guarda cuenta exactamente cuántas veces aparecen ciertos textos en español en el anfitrión, y tres cuentas cambian: «La relectura no trajo resultado.» 2→0, «El borrador se está aplicando o ya no está registrado.» 5→6 y «El rack no tiene escala de anotación.» 1→0. Las cambia el propio diseño que ya decidiste: las relecturas pasan a la lectura completa, «Conservar» presenta antes de conservar y la escala ya no puede faltar. No hay forma de mantener las cuentas sin dejar código muerto. (Lo demás de esta vuelta lo resolví yo sin tocar la lista, como con «Conservar»: el namespace de las copias y dos copias que ya fija D-PB-04, borrar vistas obsoletas.) ¿Cómo sigo?

**Opciones:**
1. **Añadirla a la lista (Recomendado):** La lista pasa a 478. La guarda se salta con registro y su equivalente escrito a mano lleva las cuentas nuevas (0, 6 y 0), cada una con su comentario «antes N», como hicieron las tareas anteriores. Si aparece otra prueba fuera de la lista, te vuelve.
2. **Mantener las cuentas:** El GREEN conserva las cuentas de hoy con ramas que nunca se ejecutan y textos cambiados solo para la guarda. No lo recomiendo: es código muerto y contradice D-PBT4-04 y D-PBT4-08.
3. **No emitir PB-T4b:** PB-T4b queda sin emitir hasta que decidas otra cosa; F4B se detiene antes de «Editar todo».

**Respuesta del Owner, literal:** «Añadirla a la lista (Recomendado)».

## Lo que queda decidido

- **La lista de PB-T4b pasa de 477 a 478.** Se añade `WorkspaceHostHardeningGuardTests.NoneOfThe30LiteralsInEnglishStaysInTheHostOrTheSupportAndTheFourIdentifiersAreKept`, en C-5, con su `Skip`. Su equivalente escrito a mano (`WorkspaceHostHardeningGuardFull`) lleva la misma tabla con tres cuentas nuevas, 0, 6 y 0, cada una con su comentario `// before N`.
- **Es la lista que vio el Owner** para la condición de emisión de `owner-pbt4-decisions.md`, pregunta 4. Si aparece otra prueba que falle fuera de ella, vuelve al Owner.
