# Decisiones del Owner sobre PB-T4d (I-64 F4B): la lista de la retirada del piloto y su emisión

**Fecha:** 2026-11-22. Es la vuelta al Owner que pedía `owner-pbt4b-retire-list-decisions.md`, pregunta 1: «PB-T4d vuelve al Owner con su lista antes de emitirse».

**El contexto:** la emisión condicional de F4B-E49, pregunta 4, cubría PB-T4a, PB-T4b y PB-T4c, y terminó con el cierre de PB-T4c (`6c2820ba`, F4B-E55). La lista de PB-T4d se midió:
- **por texto,** sobre `6c2820ba` (`v52/remeasure_pbt4d_v52-out.txt`, `v52/pbt4d-remeasure-v52.tsv`);
- **compilando y ejecutando,** en copias exportadas:
  - la variante A, en `/d/r6d`;
  - la variante B, en `/d/r6e`;
  - la prueba lenta, en `/d/r6f`;

  con los registros en `pbt4dcc/compile/`.

El traspaso del redactor está en `v52/handoff-pbt4d.md`.

La sesión preguntó con AskUserQuestion, en una sola vuelta de cuatro preguntas. Este registro copia el texto exacto. La numeración F4B-E se asigna al custodiarlo.

## 1. Pregunta (encabezado «Lista T4d»)

**Pregunta, literal:**

> PB-T4d (retirar el código del piloto), su lista medida en copias exportadas sobre el cierre de PB-T4c. Se sustituyen con registro las 138 pruebas que fijan el piloto por su letra; se borran con registro las 55 originales ya saltadas y sus 29 ayudantes, y una prueba de UI saltada desde PB-TC. Al medir salen dos alcances: A quita lo previsto y lo justo para compilar (8 bajas más por compilación y 4 pruebas que caen al ejecutar); B quita además lo que queda sin uso (12 bajas por compilación, 3 que caen, compila sin ningún aviso y no deja código muerto, pero edita una copia de PB-T4b). En los dos casos, lo que cae al ejecutar (la copia de la guarda de «Saltar y duplicar» y dos guardas que exigen la forma del piloto) se sustituye con registro, y las dos guardas que pasaban en vacío se reescriben para el borrador completo, como decidiste. ¿Qué lista emito?

**Opciones:**
1. **Alcance B (Recomendado):** Retirar el piloto y lo que queda sin uso, con esa lista. Es la retirada completa: no queda código muerto y compila sin avisos.
2. **Alcance A:** Retirar solo lo previsto y lo justo para compilar. Quedan algunas piezas del piloto sin uso, para otra tarea.
3. **No emitir PB-T4d:** El código del piloto se queda sin uso por ahora; PB-T5 sigue sin la retirada.

**Respuesta del Owner, literal:** «Alcance B (Recomendado)».

## 2. Pregunta (encabezado «Recarga»)

**Pregunta, literal:**

> PB-T4d: el arreglo que dejaste para después en PB-T4c, que «Editar todo» recargue antes un borrador limpio posiblemente obsoleto, como ya hace «Editar». Está medido: toca una función del panel o del anfitrión, se fija con una prueba nueva y no cambia ninguna línea esperada existente. ¿Dónde va?

**Opciones:**
1. **Tarea propia después (Recomendado):** Una tarea pequeña, después de PB-T4d. Cambia un comportamiento visible y no es retirada, así que no mezcla las dos cosas en un mismo contrato.
2. **Dentro de PB-T4d:** Va en el mismo contrato que la retirada. Ahorra una cadena, pero junta un cambio visible con la retirada.

**Respuesta del Owner, literal:** «Tarea propia después (Recomendado)».

## 3. Pregunta (encabezado «Prueba lenta»)

**Pregunta, literal:**

> PB-T4d: la prueba lenta que señaló la revisión (una copia de PB-T4b que tarda unos 6,5 minutos sola) se queda tras la retirada. La causa es simple: repite una preparación completa por cada uno de los casi 5 000 toques al catálogo. En la CI roza el detector de cuelgues de 5 minutos. ¿Qué hago?

**Opciones:**
1. **Acotarla en PB-T4d (Recomendado):** Su bucle se reduce a unos puntos que incluyan el fallo del constructor, con registro, en PB-T4d, que ya reescribe esa familia de copias. Pasa de unos 6,5 minutos a unos segundos y deja de rozar el límite.
2. **Dejarla como está:** Se queda igual; sigue el riesgo de que la CI la corte un día por tiempo.
3. **Subir el límite de la CI:** Se sube el detector de cuelgues de la CI en otra tarea. La prueba sigue lenta.

**Respuesta del Owner, literal:** «Acotarla en PB-T4d (Recomendado)».

## 4. Pregunta (encabezado «Emisión T4d»)

**Pregunta, literal:**

> PB-T4d: dijiste que PB-T4d te vuelve con su lista antes de emitirse; esta es esa vuelta. Una vez decidida la lista, ¿puedo emitir PB-T4d sin volver a preguntarte, con las mismas condiciones que PB-T4a, T4b y T4c?

**Opciones:**
1. **Sí, con condiciones (Recomendado):** Se emite si la comprobación acotada da 0 bloqueantes y 0 importantes, la remedición no cambia nada material y la lista es la que elegiste o una parte. Te vuelve por una decisión nueva, una prueba que falle fuera de la lista, un efecto sin medir o un intento 3.
2. **Vuelve antes de emitir:** Preparo el contrato y la comprobación acotada, y te vuelvo con todo antes de emitir.

**Respuesta del Owner, literal:** «Sí, con condiciones (Recomendado)».

## Lo que queda decidido

- **PB-T4d emite el alcance B:** D-PBT4-10, más lo que queda sin uso (`PushBackPilotPrepareInput`, `PushBackPilotComposition` y el `ComparePost` del piloto), con la lista medida:
  - las 138 sustituidas con registro;
  - las 55 originales y sus 29 ayudantes, borrados con registro;
  - la `Theory` de UI de SUS-01, borrada con registro;
  - las 12 bajas por compilación de la variante B;
  - las 3 que caen al ejecutar, sustituidas con registro: el equivalente de CT-PBT4B-20 de PB-T4c y las dos guardas de WPPG;
  - los vacíos de M-04, reescritos para el borrador completo.

  La lista exacta, fila a fila, la fija el paquete (v5.2 o siguiente) con su archivo y su sha256, y se mide otra vez en su precondición 6.
- **La recarga de «Editar todo»** (OWNER-02 de PB-T4c) va a una tarea propia, pequeña, después de PB-T4d. No entra en PB-T4d.
- **La prueba lenta** (`FullPanelCopyWorkspacePilotPrepareGreenTests.InvalidD_ABuilderThatCannotBuild…`, OWNER-05 de PB-T4c) se acota en PB-T4d, con registro: su bucle se reduce a unos puntos que incluyan el fallo del constructor.
- **La emisión de PB-T4d es condicional,** como la de PB-T4a, T4b y T4c. Se emite sin volver al Owner si se cumplen tres condiciones:
  - la comprobación acotada da 0 bloqueantes y 0 importantes;
  - la remedición no cambia nada material;
  - la lista es la elegida, o una parte.

  Vuelve al Owner por una decisión nueva, una prueba que falle fuera de la lista, un efecto material sin medir o un intento 3.
