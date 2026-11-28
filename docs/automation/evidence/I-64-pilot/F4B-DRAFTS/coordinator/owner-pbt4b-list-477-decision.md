# Decisión del Owner sobre PB-T4b (I-64 F4B): dos pruebas de PB-T4a fuera de la lista

**Fecha:** 2026-11-20, después de `owner-pbt4b-breaks-decision.md` (la lista de 475) y de `owner-pbt4a-cost-decision.md` (la optimización de la lectura en PB-T4b).

**Por qué se preguntó:** al construir el GREEN literal de PB-T4b para la v4.5 sobre el código real ya cerrado de PB-T4a (`faef7020`, custodia `78a7cef3`), el autor del paquete encontró dos pruebas de PB-T4a que caen y no están en la lista de 475:
- **CT-PBT4A-05** (`PushBackFullReadGuardTests.cs:53-74`) usa `new EditIntentFakeReader()`, que responde `FromPilot` sin lectura completa. Con el borrador completo de PB-T4b, `Begin` responde `NotAdmitted` y la línea de la prueba cambia;
- **CT-PBT4A-03** (`PushBackFullReadPathTests.cs:152-186`) busca el borrador con `Find<PushBackPilotDraftState>` (`PushBackFullReadSupport.cs:100`) y no lo encuentra.

La v4.4 no las vio porque entonces solo tenía la especificación de las CT de PB-T4a, no su código. La optimización de la lectura (D-S4: la lectura completa del `Primary`, diferida en RIA) no las rompe ni rompe nada más.

La sesión preguntó con AskUserQuestion. Este registro copia el texto exacto. La numeración F4B-E se asigna al custodiarlo.

## Pregunta (encabezado «Lista T4b»)

**Pregunta, literal:**

> Al construir PB-T4b sobre el código real ya cerrado de PB-T4a, caen dos pruebas de PB-T4a que no están en la lista de 475: CT-PBT4A-03 (la relectura con el origen) y CT-PBT4A-05 (la caída a la lectura de hoy con un lector sin la interfaz nueva). No las rompe la optimización de la lectura, sino el propio diseño que ya decidiste: el borrador del panel pasa a ser el completo, y esas dos pruebas usan ayudantes que buscan el borrador del piloto o leen sin la lectura completa. Para que pasaran, PB-T4b tendría que abrir un borrador del piloto, que es la traducción que prohibiste. La optimización en sí (una sola materialización por lectura) está diseñada y no rompe nada. ¿Cómo sigo?

**Opciones:**
1. **Añadirlas a la lista (Recomendado):** La lista de PB-T4b pasa de 475 a 477. Las dos se saltan y se sustituyen con registro por equivalentes escritos a mano que fijan lo mismo sobre el borrador completo (el mismo ayudante buscando el borrador completo, y un lector sin la interfaz que responde con la lectura completa). Si aparece otra fuera de la lista, te vuelve.
2. **No emitir PB-T4b:** PB-T4b se queda sin emitir hasta que decidas otra cosa; F4B se detiene antes de «Editar todo».

**Respuesta del Owner, literal:** «Añadirlas a la lista (Recomendado)».

## Lo que queda decidido

- **La lista de PB-T4b pasa de 475 a 477.** Se añaden CT-PBT4A-03 y CT-PBT4A-05, cada una con su `Skip` y un equivalente escrito a mano que fija lo mismo sobre el borrador completo.
- **Es la lista que vio el Owner** para la condición de emisión de `owner-pbt4-decisions.md`, pregunta 4. Si aparece otra prueba que falle fuera de ella, vuelve al Owner.
