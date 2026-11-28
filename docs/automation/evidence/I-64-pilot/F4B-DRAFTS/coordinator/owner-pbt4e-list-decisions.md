# Decisiones del Owner sobre la lista de la tarea de la recarga de «Editar todo» (I-64 F4B; nombre provisional PB-T4e)

**Fecha:** 2026-11-22. Las tomó después del cierre de PB-T4d (F4B-E59, `160e019d`) y del análisis de la lista (`pbt4e/list-proposal.md`, leído por objetos git en `160e019d`), y antes de escribir el paquete de la tarea.

**El origen:** OWNER-02 de la revisión del código de PB-T4c, que el Owner aceptó con «Aceptarlo y corregir luego» (`owner-pbt4c-code-decisions.md`, pregunta 2), y la pregunta 2 de `owner-pbt4d-list-decisions.md`, «Tarea propia después».

La sesión preguntó con AskUserQuestion, en una sola vuelta de cuatro preguntas. Este registro copia el texto exacto. La numeración F4B-E se asigna al custodiarlo.

## 1. Pregunta (encabezado «Alcance»)

**Pregunta, literal:**

> «Editar todo» no vuelve a leer el dibujo cuando cambió fuera del panel y el borrador no tiene cambios tuyos; «Editar» sí lo hace, sin avisar. ¿Qué arreglo preparo?

**Opciones:**
1. **«Solo la recarga (Recomendado)»:** Antes de abrir la ventana se vuelve a leer el dibujo, sin aviso, igual que con «Editar». Con cambios tuyos, todo sigue como hoy. Toca una función, y ninguna prueba existente cambia.
2. **«La recarga con un aviso»:** Lo mismo, y el campo dice «Borrador recargado desde el dibujo.». «Editar» no avisa, así que los dos botones dejan de comportarse igual.
3. **«Recarga y comprobar cambios»:** Además, si el borrador tiene cambios tuyos y el dibujo cambió fuera, se compara antes de abrir: el conflicto sale antes de editar y no al «Actualizar». Va más allá de «Editar»: otro camino, textos nuevos y más pruebas.

**Respuesta del Owner, literal:** «Solo la recarga (Recomendado)».

## 2. Pregunta (encabezado «Si falla»)

**Pregunta, literal:**

> Si no se puede volver a leer el dibujo (el rack ya no está, hay un comando en curso, o el rack ya no es editable aquí), ¿qué hace «Editar todo»?

**Opciones:**
1. **«No abrir y avisar (Recomendado)»:** La ventana no se abre, y el campo muestra lo mismo que «Editar»: el motivo, la pista de RACKEDITAR y «No se recargó: el borrador se conserva tal como estaba.». Abrir sobre una base que ya se sabe vieja es justo lo que se corrige, y si el rack ya no está, «Actualizar» tampoco podría escribir.
2. **«Abrir sobre la base de antes»:** La ventana se abre como hoy, con un aviso de que no se pudo recargar. El cambio de fuera se detecta al «Actualizar», como hoy.

**Respuesta del Owner, literal:** «No abrir y avisar (Recomendado)».

## 3. Pregunta (encabezado «Orden»)

**Pregunta, literal:**

> ¿Cuándo va esta tarea respecto a PB-T5, que es la CI exacta, el Smoke-PB con tu firma y el cierre de F4B?

**Opciones:**
1. **«Antes de PB-T5 (Recomendado)»:** Una cadena pequeña, con su intento 1 de 3, cerrada antes de PB-T5, y el Smoke-PB ya mira el comportamiento corregido. Es lo que decidiste en PB-T4d («Tarea propia después»): no mezcla un cambio visible con el cierre, y si falla no retiene el cierre.
2. **«Fundirla con PB-T5»:** Ahorra una cadena, pero PB-T5 llevaría un cambio de código junto a su CI, su Smoke-PB y el cierre. Un fallo de la corrección pararía el cierre.
3. **«Después de F4B»:** El Smoke-PB mira el comportamiento de hoy, como ya estaba previsto, y la corrección queda anotada para más adelante.

**Respuesta del Owner, literal:** «Antes de PB-T5 (Recomendado)».

## 4. Pregunta (encabezado «Emisión»)

**Pregunta, literal:**

> Una vez decidida la lista, ¿puedo emitir esta tarea sin volver a preguntarte, con las mismas condiciones que PB-T4a a PB-T4d?

**Opciones:**
1. **«Sí, con condiciones (Recomendado)»:** Se emite si la comprobación acotada da 0 bloqueantes y 0 importantes, si la remedición no cambia nada material y si la lista es la elegida (aquí, ninguna prueba existente sustituida). Te vuelve por una decisión nueva, por una prueba que falle fuera de la lista, por un efecto sin medir o por un intento 3.
2. **«Vuelve antes de emitir»:** Preparo el paquete, el contrato y la comprobación acotada, y te vuelvo con todo antes de emitir.

**Respuesta del Owner, literal:** «Sí, con condiciones (Recomendado)».

## Lo que queda decidido

- **El alcance:** solo la recarga. Antes de abrir la ventana de «Editar todo», un borrador Clean marcado como posiblemente obsoleto se recarga desde el dibujo, sin aviso, igual que con «Editar». Un borrador con cambios del usuario (Dirty) o sin la marca sigue como hoy.
- **Si la recarga falla:** la ventana no se abre, y el campo muestra lo mismo que «Editar»: el motivo, la pista de RACKEDITAR y «No se recargó: el borrador se conserva tal como estaba.».
- **El orden:** una tarea propia, con su cadena y su intento 1 de 3, cerrada antes de PB-T5. El Smoke-PB de PB-T5 mira el comportamiento corregido.
- **La emisión:** condicional, como PB-T4a a PB-T4d. Se emite si la comprobación acotada da 0 bloqueantes y 0 importantes sin resolver, si la remedición no cambia nada material y si la lista es la elegida (ninguna prueba existente sustituida). Vuelve al Owner por una decisión nueva, por una prueba que falle fuera de la lista, por un efecto sin medir o por un intento 3.
- **Lo que no se preguntó, porque es de diseño y lo propone la sesión:** dónde va la recarga (en el anfitrión, la opción A de la propuesta) y si se reutiliza `NotBegun` o se crea un valor nuevo de `EditAllStatus`. Son decisiones de la sesión, que el paquete registra como tales.
