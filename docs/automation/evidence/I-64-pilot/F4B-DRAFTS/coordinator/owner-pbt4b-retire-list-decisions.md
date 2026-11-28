# Decisiones del Owner sobre PB-T4b (I-64 F4B): cuándo se retira el piloto y la lista exacta

**Fecha:** 2026-11-20, después de las cuatro decisiones de `owner-pbt4-decisions.md`.

**Por qué se preguntó:** con las listas exactas de la v4.1 del paquete de PB-T4, la lista de PB-T4b superaba la cota que había visto el Owner, y cambiaban dos premisas de sus respuestas. Las dos preguntas las redactó la revisión independiente, en la §16 de `independent-review-v1.md`, y la sesión las hizo en una sola vuelta, con AskUserQuestion. Este registro copia el texto exacto de cada pregunta y la respuesta del Owner. La numeración F4B-E se asigna al custodiarlo.

## Pregunta 1 (encabezado «Retirada»)

**Pregunta, literal:**

> Al preparar PB-T4b con las listas exactas, dos de tus respuestas chocan en la misma subtarea: «Retirarlo» (el código del piloto) y «Saltar y duplicar» (las 428 pruebas del panel). Si el código del piloto se retira en PB-T4b, 55 de esas 428 dejan de compilar aunque estén saltadas, porque ellas o sus ayudantes usan lo retirado (por ejemplo, la clave de la escala del adaptador del piloto o el Prepare del piloto); y con ellas, 29 ayudantes. Habría que borrarlos con registro, y eso es una excepción a la regla de no editar pruebas ni ayudantes, que con «Saltar y duplicar» no hacía falta. Además, la retirada sustituye 139 pruebas, no unas 100: 67 de ellas porque dejan de compilar, sobre todo las que ejecutan el «Actualizar» del piloto. ¿Cuándo retiro el código del piloto?

**Opciones:**
1. **Retirar tras PB-T4c (Recomendado):** PB-T4b sustituye 470 pruebas, sin borrar ni editar ninguna. El código del piloto queda sin uso hasta una subtarea propia, PB-T4d, después de PB-T4c, cuando las copias ya estén en verde. PB-T4d sustituye las 139 y borra con registro las 55 originales saltadas y los 29 ayudantes, que para entonces ya no corren, y te vuelve con su lista antes de emitirse.
2. **Retirar en PB-T4b:** PB-T4b sustituye 609 pruebas (470 + 139) y borra con registro 55 de las 428 originales y 29 ayudantes, como excepción declarada a la regla de no editar pruebas ni ayudantes. No hace falta otra subtarea.
3. **Conservarlo sin uso:** No se retira, y sus pruebas siguen en verde. PB-T4b sustituye 470. Quedan anotados en ideas futuras el modelo paralelo, la segunda copia de la regla de la escala y la premisa de D-1 sin cumplir.

**Respuesta del Owner, literal:** «Retirar tras PB-T4c (Recomendado)».

## Pregunta 2 (encabezado «Lista T4b»)

**Pregunta, literal:**

> Con el recuento exacto, PB-T4b sustituye 470 pruebas (609 si eliges «Retirar en PB-T4b»), frente a las unas 465 que viste. Son las 428 del panel (375 copias que comprueba el guion y 53 equivalentes escritos a mano, con su registro, porque describen lo que PB-T4b cambia a propósito o dependen del piloto de una forma que la copia no puede cambiar), 15 guardas de texto (se dijeron unas 12) y 27 pruebas de la interfaz de «Conservar» (se dijeron unas 25). Tu condición de emisión pide que la lista emitida sea la que viste o una parte de ella, y 5 de estas no estaban contadas. ¿La tomo como la lista que viste?

**Opciones:**
1. **Sí, esa lista (Recomendado):** PB-T4b se emite con esa lista exacta, la guardada en el paquete, si se cumple el resto de tu condición de emisión. Una prueba que falle fuera de ella te sigue volviendo.
2. **Vuelve antes de emitir:** PB-T4b te vuelve con su contrato y su lista antes de emitirse.

**Respuesta del Owner, literal:** «Sí, esa lista (Recomendado)».

## Lo que queda decidido

- **El código del piloto NO se retira en PB-T4b.**
  - Queda sin uso hasta una subtarea propia, **PB-T4d**, que va después de PB-T4c.
  - PB-T4d sustituye las 139 y borra con registro las 55 originales saltadas y los 29 ayudantes, que para entonces ya no corren.
  - **PB-T4d vuelve al Owner con su lista antes de emitirse.** La autorización condicional de `owner-pbt4-decisions.md` (pregunta 4) no la cubre.
- **La lista exacta de PB-T4b son 470 pruebas,** y el Owner la toma como «la que vio»:
  - las 428 del panel: 375 copias mecánicas comprobadas por el guion y 53 equivalentes a mano, con su registro;
  - 15 guardas de texto;
  - 27 pruebas de la interfaz de «Conservar».

  Es la lista guardada en el paquete (`pbt4-substitution-lists.md`, sin las filas de la causa de la retirada, C-7). PB-T4b se emite con ella por la autorización condicional, si se cumple el resto de la condición. Una prueba que falle fuera de ella vuelve al Owner.
- **Sin cambios:** PB-T4a (lista de 1) y PB-T4c (lista de 2) siguen con la autorización condicional de `owner-pbt4-decisions.md`.
