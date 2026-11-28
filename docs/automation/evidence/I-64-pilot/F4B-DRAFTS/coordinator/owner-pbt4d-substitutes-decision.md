# Decisión del Owner sobre PB-T4d (I-64 F4B): qué sustituye a las 138 pruebas de la lista

**Fecha:** 2026-11-22. Después de `owner-pbt4d-list-decisions.md`, cuya pregunta 1 eligió el alcance B.

**Por qué se preguntó:** al escribir la v5.2, el autor del paquete midió si cada baja de B hace falta. Partió de cero bajas en una copia exportada de `6c2820ba` (`/d/r6h`) y quitó solo lo que la compilación pedía (`v52/need-report-r6h.txt`).
- **Lo que hace falta:** de las 235 bajas de B, 201 para compilar y 34 no. Ninguna cae fuera de B.
- **Pruebas que protegen código que se queda:** muchas de las 110 de las 138 que hacen falta usan el piloto como vehículo:
  - WPUR, 48 pruebas: la corrida genérica que sigue usando `ExecuteFull`;
  - Ownership, 9: el núcleo del borrador;
  - StaleBase, 4;
  - CT-PBT3B-05, 06 y 09: la escritura completa.
- **Pruebas que no hace falta quitar:** 7 CT de PB-T3b pasan sobre el modelo completo, con sus 6 ayudantes.
- **Lo que no fijaba el paquete:** qué sustituye a las 138.
- **Una corrección de la sesión:** en la pregunta anterior le había dicho al Owner que las 138 «se sustituyen con registro». La sesión lo corrige en esta pregunta.

La sesión preguntó con AskUserQuestion. Este registro copia el texto exacto. La numeración F4B-E se asigna al custodiarlo.

## Pregunta (encabezado «Las 138»)

**Pregunta, literal:**

> PB-T4d: tengo que corregir algo que te dije. En la pregunta anterior escribí que las 138 pruebas «se sustituyen con registro», pero el paquete nunca definió sus sustitutas, y la medida (compilando sin el piloto) muestra que muchas de ellas, además del piloto, prueban código que se queda: por ejemplo 48 que usan el piloto para ejercitar la corrida genérica de escritura, 9 que prueban el núcleo del borrador y 3 de la escritura completa. Otras 7 ni siquiera hace falta quitarlas (pasan sobre el modelo completo) y algunas pasan en vacío. Borrarlas todas sin sustituta dejaría sin red código que se queda. ¿Qué sustituye a esas 138?

**Opciones:**
1. **Sustitutas donde hace falta (Recomendado):** Se quedan las 7 que pasan. Lo que también prueba código que se queda conserva esa parte, como equivalente escrito a mano o edición con registro (unas 15 a 25 piezas). Lo que solo fija el piloto, o pasa en vacío, se borra con registro; eso incluye las 48 de la corrida genérica, que también prueban las CT de PB-T3b sobre el modelo completo.
2. **Y además copias completas:** Lo mismo, y además copias sobre el modelo completo de las que usan el piloto como vehículo (las 48 de la corrida genérica, las del núcleo…), como un «Saltar y duplicar» propio de PB-T4d. Más red, bastante más trabajo.
3. **Borrarlas todas:** Se borran las 235 con registro, y el registro declara por grupo qué cubre ya el modelo completo y qué se pierde. Es lo más barato, pero deja partes sin prueba.

**Respuesta del Owner, literal:** «Sustitutas donde hace falta (Recomendado)».

## Nota de la sesión: una afirmación de la opción 1 sin comprobar

La opción 1 dice que la corrida genérica de las 48 de WPUR «también prueban las CT de PB-T3b sobre el modelo completo». Cuando la sesión escribió la pregunta, eso no estaba medido.

La sesión encarga comprobarlo antes de escribir la v5.2, en dos pasos:
1. **Qué ejercitan las 48:** qué caminos de la corrida genérica, con `ExecuteFull` y sus estados.
2. **Si los cubren las CT de PB-T3b u otras pruebas que se quedan.**

**Si alguno no queda cubierto:**
- ese camino pasa a ser una parte «que también prueba código que se queda», y entra en las sustitutas de la opción 1;
- si eso cambia materialmente lo que el Owner eligió, vuelve al Owner.

## Lo que queda decidido

- **Las 138 de la lista de B y sus ayudantes** se tratan así:
  - **se quedan,** sin quitarlas, las 7 CT de PB-T3b que pasan sobre el modelo completo y sus 6 ayudantes;
  - **lo que también prueba código que se queda conserva esa parte,** como equivalente escrito a mano o como edición con registro;
  - **lo que solo fija lo retirado, o pasa en vacío,** se borra con registro.
- **La lista de PB-T4d** sigue siendo una parte del alcance B, que eligió el Owner (`owner-pbt4d-list-decisions.md`), con las sustitutas de esta decisión.
