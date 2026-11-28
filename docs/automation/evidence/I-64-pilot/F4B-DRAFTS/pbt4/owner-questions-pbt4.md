> **Redacción para el repo público:** sin rutas locales.

# I-64 F4B — PB-T4: las cuatro preguntas al Owner (para AskUserQuestion)

Son cuatro preguntas para una sola vuelta. Cada una se entiende sola, y la recomendada va primero. Las etiquetas tienen como mucho 5 palabras, sin contar «(Recomendado)».

**Los números** son recuentos por texto en `7f0d9aa3`, el cierre de PB-T3c (paquete de PB-T4 v4, §8.2 y D-PBT4-11; `pbt4-measurements.md` R-29 a R-33). Pueden contar de más o de menos. La lista que se emite es, como mucho, la que el Owner ve aquí.

---

## Q1 — header: «Piloto»

**Pregunta:**
> PB-T4 pasa el borrador de Push Back del panel del Workspace al modelo completo de PB-T3, sin convertir un borrador en otro, como pediste. Al hacerlo, muchas pruebas existentes dejan de describir el panel. Según lo que elijas en la pregunta del procedimiento, son unas 461 (si se saltan y se duplican) o unas 275 (si se edita una línea del kit). Cada una se sustituye con registro por otra que conserva todo lo que protegía.
>
> Queda además código del «piloto» (el editor reducido de F4) que el panel ya no usará: su comparador, su adaptador de la escala (con su propia copia de la regla, que pediste dejar en una sola), su «Conservar» y su Prepare, Precheck y Execute. PB-T1 lo dejó como un modelo paralelo interino que PB-T4 debía retirar, y tu decisión D-1 contaba con quitarlo. El lector del piloto se queda en cualquier caso, porque el modelo completo lo usa para el rechazo AUTH-13. ¿Qué hago con ese código?

**Opciones:**
1. **«Retirarlo (Recomendado)»:** se retira lo que solo usaba el panel. Se sustituyen además unas 99 pruebas propias (35 guardas de texto y 64 de comportamiento) y tres equivalentes de D-1. Queda una sola regla de la escala y ningún modelo paralelo; la carga sigue dentro del lector.
2. **«Conservarlo sin uso»:** no se toca, y esas pruebas siguen en verde. Quedan, anotados en ideas futuras, el modelo paralelo, la segunda copia de la regla y la premisa de D-1 sin cumplir.
3. **«Dos modelos hasta F5-A»:** el campo de escala del panel sigue con el piloto, y «Editar todo» exige actualizar o descartar ese borrador antes de abrir uno completo. Son dos códigos y dos «Conservar mi borrador». No se puede medir cuántas pruebas toca sin diseñarlo.

---

## Q2 — header: «Presentación»

**Pregunta:**
> Antes de aceptar «Conservar mi borrador», pediste mostrar lo bastante para distinguir los campos («fields») cambiados fuera, los cambiados por el usuario, los cambiados en los dos sitios, los cambios estructurales (módulos, frentes, lado B) y lo que pasa con las vistas, sin un diff JSON. El clasificador de PB-T3 da cada valor cambiado con su grupo (nombre, rack, anotaciones, seguridad, frentes, módulos, lado B, parte compuesta, fuera del editor). También da los valores derivados que cambian solos, como posiciones al añadir un frente. ¿Qué se muestra encima del Sí/No del panel, que mide unos 320 px?

**Opciones:**
1. **«Por grupos (Recomendado)»:** una línea por clase con los grupos afectados, por ejemplo «Cambiado fuera: el nombre y los frentes.», más una de lo estructural y otra de las vistas. Son 9 nombres fijos y cabe en el panel. Dice qué zonas cambiaron, no qué campo.
2. **«Por campos»:** lo mismo, con el nombre en español de cada campo cambiado. Es la letra de tu decisión. Necesita una tabla de nombres para del orden de un centenar de campos del editor, y enseña también los valores derivados.
3. **«Por campos con valores»:** como «Por campos», con el valor de antes, el de fuera y el tuyo tras cada campo. Es la más larga.

---

## Q3 — header: «Emisión»

**Pregunta:**
> PB-T4 va en tres subtareas, en este orden:
> - PB-T4a: el panel lee también el modelo completo, sin cambiar su borrador;
> - PB-T4b: el panel pasa al modelo completo, con el nuevo «Conservar mi borrador»;
> - PB-T4c: «Editar todo».
>
> Ninguna autorización tuya cubre PB-T4. Las pruebas que se sustituyen con registro son, como mucho: 1 en PB-T4a; unas 461 o unas 275 en PB-T4b, según la pregunta del procedimiento; y 2 en PB-T4c. ¿Emito cada subtarea tras el cierre de la anterior, sin volver a ti, si su comprobación acotada no deja nada bloqueante ni importante, la remedición no cambia nada material y la lista emitida es la que viste o una parte de ella?

**Opciones:**
1. **«Sí, con condiciones (Recomendado)»:** vuelve a ti por una decisión nueva, un control con una firma no prevista, un intento 3, un efecto material sin medir o una prueba que falle fuera de la lista. Si eso ocurre en mitad de una subtarea, el Worker para y te vuelve con ella.
2. **«Solo PB-T4a»:** PB-T4a se emite así, y PB-T4b y PB-T4c vuelven a ti antes de emitirse.
3. **«Cada subtarea me vuelve»:** cada una vuelve a ti, con su lista, antes de emitirse.

---

## Q4 — header: «Procedimiento»

**Pregunta:**
> Para pasar el panel al modelo completo, 428 pruebas del panel de F4 (en 9 clases) empiezan sus borradores con una lectura del piloto que les da un kit de pruebas compartido. Unas 242 fallarían de verdad, porque sus propios ayudantes buscan el borrador del piloto; las otras 186 seguirían bien si el kit trajera también el modelo completo. ¿Cómo las trato?

**Opciones:**
1. **«Saltar y duplicar (Recomendado)»:** como en D-1, las 428 se saltan y se copian con un kit nuevo, y un guion comprueba que cada copia solo cambia el kit. La lista es cerrada, no hay implementación oculta y ninguna prueba se debilita. No hace falta ninguna excepción a la regla de no editar pruebas ni ayudantes. Son 428 copias.
2. **«Editar una línea»:** se edita una línea del kit compartido, como excepción declarada a esa regla. Las 186 siguen tal cual, sobre el camino real, y solo las 242 se saltan y se sustituyen con registro. Cumple lo mismo que la anterior con 186 copias menos, pero rompe la regla y se aparta de D-1.
3. **«Implementación de referencia»:** la sesión escribe una para medir la lista exacta. Si el Worker no la recibe, cualquier diferencia suya para el trabajo; si la recibe, cambian los papeles de la sesión y del Worker.
