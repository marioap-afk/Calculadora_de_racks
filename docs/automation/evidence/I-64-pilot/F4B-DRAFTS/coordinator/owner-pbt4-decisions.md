# Decisiones del Owner sobre PB-T4 (I-64 F4B): el piloto, la presentación, las pruebas del panel y la emisión

**Fecha:** 2026-11-20. La sesión principal hizo las cuatro preguntas en una sola vuelta, con AskUserQuestion. Este registro copia el texto exacto de cada pregunta y la respuesta del Owner. La numeración F4B-E se asigna al custodiarlo, con la emisión de PB-T4a.

**Antes de preguntar:**
- el paquete de PB-T4 v4 pasó su revisión independiente adversarial: la v1, la §13 sobre la v2, la §14 sobre la v3 y la §15 sobre la v4;
- la §15 dio LISTO TRAS CORRECCIONES DE TEXTO, 0/3/4, y sus correcciones literales de Q1, Q3 y Q4 ya estaban aplicadas en el texto preguntado;
- los números son recuentos por texto sobre el cierre de PB-T3c (`7f0d9aa3`, F4B-E48).

## Pregunta 1 (encabezado «Piloto»)

**Pregunta, literal:**

> PB-T4 pasa el borrador de Push Back del panel del Workspace al modelo completo de PB-T3, sin convertir un borrador en otro, como pediste. Al hacerlo, muchas pruebas existentes dejan de describir el panel. Contadas por texto, y según lo que elijas en la pregunta sobre las 428 pruebas del panel, son unas 465 (si se saltan y se duplican) o unas 280 (si se edita una línea del kit). Cada una se sustituye con registro por otra que conserva todo lo que protegía.
>
> Queda además código del «piloto» (el editor reducido de F4) que el panel ya no usará: su comparador, su adaptador de la escala (con su propia copia de la regla, que pediste dejar en una sola), su «Conservar» y su Prepare, Precheck y Execute. PB-T1 lo dejó como un modelo paralelo interino que PB-T4 debía retirar, y tu decisión D-1 contaba con quitarlo. El lector del piloto se queda en cualquier caso, porque el modelo completo lo usa para el rechazo AUTH-13. ¿Qué hago con ese código?

**Opciones:**
1. **Retirarlo (Recomendado):** Se retira lo que solo usaba el panel. Se sustituyen además unas 99 pruebas propias (35 guardas de texto y 64 de comportamiento) y tres equivalentes de D-1. Queda una sola regla de la escala; del modelo paralelo solo queda la carga, dentro del lector, porque el modelo completo la usa.
2. **Conservarlo sin uso:** No se toca, y esas pruebas siguen en verde. Quedan, anotados en ideas futuras, el modelo paralelo, la segunda copia de la regla y la premisa de D-1 sin cumplir.
3. **Dos modelos hasta F5-A:** El campo de escala del panel sigue con el piloto, y «Editar todo» exige actualizar o descartar ese borrador antes de abrir uno completo. Son dos códigos y dos «Conservar mi borrador». No se puede medir cuántas pruebas toca sin diseñarlo.

**Respuesta del Owner, literal:** «Retirarlo (Recomendado)».

## Pregunta 2 (encabezado «Presentación»)

**Pregunta, literal:**

> Antes de aceptar «Conservar mi borrador», pediste mostrar lo bastante para distinguir los campos («fields») cambiados fuera, los cambiados por el usuario, los cambiados en los dos sitios, los cambios estructurales (módulos, frentes, lado B) y lo que pasa con las vistas, sin un diff JSON. El clasificador de PB-T3 da cada valor cambiado con su grupo (nombre, rack, anotaciones, seguridad, frentes, módulos, lado B, parte compuesta, fuera del editor), y también los valores derivados que cambian solos, como las posiciones al añadir un frente. ¿Qué se muestra encima del Sí/No del panel, que mide unos 320 px?

**Opciones:**
1. **Por grupos (Recomendado):** Una línea por clase de cambio con los grupos afectados, por ejemplo «Cambiado fuera: el nombre y los frentes.», más una línea de lo estructural y otra de las vistas. Son 9 nombres fijos y cabe en el panel. Dice qué zonas cambiaron, no qué campo.
2. **Por campos:** Lo mismo, con el nombre en español de cada campo cambiado. Es la letra de tu decisión. Necesita una tabla de nombres para del orden de un centenar de campos del editor, y enseña también los valores derivados.
3. **Por campos con valores:** Como «Por campos», y tras cada campo el valor de antes, el de fuera y el tuyo. Es la más larga.

**Respuesta del Owner, literal:** «Por grupos (Recomendado)».

## Pregunta 3 (encabezado «Pruebas F4»)

**Pregunta, literal:**

> Para pasar el panel al modelo completo, 428 pruebas del panel de F4 (en 9 clases) empiezan sus borradores con una lectura del piloto que les da un kit de pruebas compartido. Unas 242 fallarían de verdad, porque ellas o sus ayudantes usan el borrador o la lectura del piloto; las otras 186 seguirían bien si el kit trajera también el modelo completo. ¿Cómo las trato?

**Opciones:**
1. **Saltar y duplicar (Recomendado):** Como en D-1, las 428 se saltan y se copian con un kit nuevo, y un guion comprueba que cada copia solo cambia el kit. La lista es cerrada, no hay implementación oculta y ninguna prueba se debilita. No hace falta ninguna excepción a la regla de no editar pruebas ni ayudantes. Son 428 copias.
2. **Editar una línea:** Se edita una línea del kit compartido, como excepción declarada a esa regla. Las 186 siguen tal cual, sobre el camino real, y solo las 242 se saltan y se sustituyen con registro. Cumple lo mismo que la anterior con 186 copias menos, pero rompe la regla y se aparta de D-1.
3. **Implementación de referencia:** La sesión escribe una implementación para medir la lista exacta. Si el Worker no la recibe, cualquier diferencia suya para el trabajo; si la recibe, cambian los papeles de la sesión y del Worker.

**Respuesta del Owner, literal:** «Saltar y duplicar (Recomendado)».

## Pregunta 4 (encabezado «Emisión»)

**Pregunta, literal:**

> PB-T4 va en tres subtareas, en este orden: PB-T4a, el panel lee también el modelo completo, sin cambiar su borrador; PB-T4b, el panel pasa al modelo completo, con el nuevo «Conservar mi borrador»; PB-T4c, «Editar todo».
>
> Ninguna autorización tuya cubre PB-T4. Las pruebas que se sustituyen con registro, contadas por texto, son: 1 en PB-T4a; en PB-T4b, unas 465 si se saltan y se duplican o unas 280 si se edita una línea del kit, más unas 100 si retiras el código del piloto; y 2 en PB-T4c (la lista de operaciones del puerto y el censo de archivos de tu decisión D2 sobre el nombre, que pasa de 14 a 15 sin cambiar nada más). La lista de cada una queda guardada en el paquete. ¿Emito cada subtarea tras el cierre de la anterior, sin volver a ti, si su comprobación acotada no deja nada bloqueante ni importante, la remedición no cambia nada material y la lista que se emite es esa o una parte de ella?

**Opciones:**
1. **Sí, con condiciones (Recomendado):** Vuelve a ti por una decisión nueva, un control con una firma no prevista, un intento 3, un efecto material sin medir o una prueba que falle fuera de la lista. Si eso ocurre en mitad de una subtarea, el Worker para y te vuelve con ella.
2. **Solo PB-T4a:** PB-T4a se emite así, y PB-T4b y PB-T4c vuelven a ti antes de emitirse.
3. **Cada subtarea me vuelve:** Cada una vuelve a ti, con su lista, antes de emitirse.

**Respuesta del Owner, literal:** «Sí, con condiciones (Recomendado)».

## Lo que queda decidido

- **El piloto se retira:** se retira lo que solo usaba el panel, y queda la carga dentro del lector, que usa el modelo completo para AUTH-13. Sus pruebas propias y E-3, E-5 y E-6 de D-1 se sustituyen con registro (PB-T4b).
- **«Conservar mi borrador» presenta los cambios por grupos.**
- **Las 428 pruebas del panel de F4** que leen del kit del piloto se saltan y se duplican con un kit nuevo, como en D-1, con un guion que compruebe que cada copia solo cambia el kit. No hay excepción a la regla de no editar pruebas ni ayudantes.
- **La emisión condicional de PB-T4a, PB-T4b y PB-T4c,** en ese orden y cada una tras el cierre de la anterior. La condición es que la comprobación acotada dé 0 bloqueantes y 0 importantes sin resolver, que la remedición no cambie nada material y que la lista emitida sea la que vio el Owner o una parte de ella. Vuelve al Owner por una decisión nueva, un control con una firma no prevista, un intento 3, un efecto material sin medir o una prueba que falle fuera de la lista; si eso pasa en mitad de una subtarea, el Worker para.
