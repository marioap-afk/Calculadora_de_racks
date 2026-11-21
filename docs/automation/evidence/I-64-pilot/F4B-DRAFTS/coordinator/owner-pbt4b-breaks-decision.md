# Decisión del Owner sobre PB-T4b (I-64 F4B): las cinco roturas fuera de la lista

**Fecha:** 2026-11-20, después de `owner-pbt4-decisions.md` y `owner-pbt4b-retire-list-decisions.md`.

**Por qué se preguntó:** al construir el GREEN literal de PB-T4b sobre el de PB-T4a, el autor del paquete encontró cinco guardas que no están en la lista de 470 y que caen con el diseño ya decidido. La revisión independiente las confirmó una a una (la §17 de `independent-review-v1.md`): las cinco pasan en la base y caen en el GREEN. Ninguna se evita sin código muerto, y dos ni siquiera así. La lista habría pasado de la que vio el Owner, así que la sesión preguntó, con AskUserQuestion. Este registro copia el texto exacto que se le mostró y su respuesta. La numeración F4B-E se asigna al custodiarlo.

## Lo que se le dijo antes de la pregunta, sin preguntar (texto exacto del chat)

> Antes, dos cosas de PB-T4b que **no** necesitan tu decisión, porque no cambian la lista de 470 que aprobaste:
> - **Siete de las 375 copias por guion no pueden pasar,** porque fijan justo lo que PB-T4b cambia a propósito: el estado nuevo de «Conservar» y que sin testigo ahora se presentan los cambios. Pasan a equivalentes escritos a mano, con su registro. El reparto queda en 368 copias y 60 a mano, quizá alguna más al ejecutarlas, siempre dentro de las 470.
> - **«Conservar mi borrador» pide dos «Sí».** El primero muestra los cambios por grupos y el segundo conserva. Así no se rompen dos guardas del panel que no están en la lista, y no se escribe nada hasta que confirmas.

Son D-S1 y D-S2 de `session-pbt4b-design-decisions.md`.

## La pregunta (encabezado «Roturas T4b»)

**Pregunta, literal:**

> Al escribir PB-T4b sobre PB-T4a, cinco pruebas de guarda que no están en la lista de 470 caen por el propio diseño que ya decidiste: el borrador del panel pasa al modelo completo y «Actualizar» usa la escritura completa. Dos fijan que el diagnóstico de admisión sale de la lectura del piloto; dos, que el anfitrión marca y recarga borradores del piloto; y una, que «Actualizar» llama a la corrida del piloto. Esta última ya estaba en la lista de la retirada (PB-T4d), pero cae antes. Ninguna se puede evitar sin añadir código que nunca se ejecuta, y dos ni siquiera así. ¿Cómo sigo?

**Opciones:**
1. **Añadirlas a la lista (Recomendado):** La lista de PB-T4b pasa de 470 a 475. Las cinco se saltan y se sustituyen con registro por equivalentes que fijan lo mismo sobre el modelo completo. La quinta sale de la lista de PB-T4d, que pasa de 139 a 138, y el mínimo del filtro de su clase baja de 86 a 85. Si la comprobación sobre el código compilado encuentra otra fuera de la lista, te vuelve, como ahora.
2. **Mantener código muerto:** Se añaden líneas que nunca se ejecutan o no sirven para nada, para conservar la letra de tres de las cinco. Las otras dos se sustituyen igual, y la lista pasa de 470 a 472. Ese código queda en el producto hasta PB-T4d y hace que esas guardas sigan pasando sin proteger lo que protegían.

**Respuesta del Owner, literal:** «Añadirlas a la lista (Recomendado)».

## Lo que queda decidido

- **La lista de PB-T4b pasa de 470 a 475 pruebas.** Se añaden cinco:
  - EIG:871 y EIG:883 (el diagnóstico de admisión del piloto en WHC);
  - `WorkspaceLifecycleGuardTests`:627 y :642 (el marcado y la recarga de borradores del piloto);
  - `WorkspacePilotUpdateGuardTests`:823 («Actualizar» con la corrida del piloto).

  Las cinco se saltan y se sustituyen con registro por equivalentes que fijan lo mismo sobre el modelo completo. Es la lista que vio el Owner para la condición de emisión de `owner-pbt4-decisions.md`, pregunta 4.
- **La lista de PB-T4d pasa de 139 a 138,** porque la quinta sale de ella.
- **El `MinSelected` de `WorkspacePilotUpdateGuardTests` baja de 86 a 85.**
- **La regla de siempre:** si la comprobación sobre el código compilado (la precondición 6) o el GREEN del Worker encuentran otra prueba que falle fuera de la lista, vuelve al Owner.
