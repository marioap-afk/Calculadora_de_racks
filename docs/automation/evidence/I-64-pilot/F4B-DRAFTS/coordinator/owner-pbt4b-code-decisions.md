# Decisiones del Owner sobre el código de PB-T4b (I-64 F4B), intento 1

**Fecha:** 2026-11-21. Las tomó después de la revisión independiente del código (`independent-review-code.md`: ACEPTABLE CON MENORES, condicionado a I-01; B 0 / I 1 / M 8, más OWNER-01 a OWNER-04) y antes de la verificación del Controller.

**El objeto:** el Worker `R20261121T085458Z-5ef5`:
- el RED `c1e4f4585217abc8c85d90b499a07bb93c84be2c`;
- la reparación del RED `9b2d18937d3aa03b4cbeaf0546cd0e6b44a4f6fe`;
- el GREEN `c065080d3fd1dd8bb1937b98b81e9a0c1714777b`.

Todo sobre la BASE `8f783df5` (F4B-E52).

La sesión preguntó con AskUserQuestion, en una sola vuelta de tres preguntas. Este registro copia el texto exacto. La numeración F4B-E se asigna al custodiarlo.

## 1. Pregunta (encabezado «CT-20 (5)»)

**Pregunta, literal:**

> PB-T4b: el Worker terminó y la revisión independiente da ACEPTABLE CON MENORES (0 bloqueantes, 1 importante, 8 menores), condicionado a esta decisión. Una de las comprobaciones de la prueba CT-20 dice que ninguna de las 21 copias nombra ciertos tipos del piloto. Tal cual no se puede cumplir: dos copias de guardas, cuyo texto ya estaba fijado por hash en el paquete, nombran esos tipos dentro de cadenas de texto (lo que inyectan sus autopruebas y una expresión de búsqueda), nunca como código. El Worker, en vez de parar, la limitó a las 9 copias donde sí se cumple y lo declaró. No cambia ningún código de src/. Como con c-16 de PB-T0 y CT-02 de PB-T1a, la decisión era tuya. ¿Cómo sigo?

**Opciones:**
1. **Ratificar como errata (Recomendado):** Se acepta como errata del paquete: la comprobación solo puede valer sobre esas 9 copias. Solo este caso, sin cambio de código. Siguen la verificación (con tu decisión como entrada), los controles y el cierre en el intento 1.
2. **Intento 2 con el paquete corregido:** Se reformula la comprobación en una versión nueva del paquete, se reemite el contrato y el Worker rehace la tarea en el intento 2 de 3. Cuesta varias horas.

**Respuesta del Owner, literal:** «Ratificar como errata (Recomendado)».

## 2. Pregunta (encabezado «Vacíos»)

**Pregunta, literal:**

> PB-T4b: la revisión encontró dos guardas existentes del piloto (fuera de la lista de 478) que en el código nuevo pasan «en vacío»: siguen en verde, pero ya no comprueban nada, porque buscan un método del borrador del piloto que el panel ya no usa. Una de las copias hereda uno de esos vacíos. No fallan, así que tu regla de «una prueba que falle fuera de la lista» no las cubre. PB-T4d, que retira el código del piloto, ya te vuelve con su lista antes de emitirse. ¿Qué hago con ellas?

**Opciones:**
1. **Llevarlas a PB-T4d (Recomendado):** Se registran ahora como vacíos declarados en el cierre de PB-T4b y entran en la lista de PB-T4d, que las sustituye o las retira junto con el código del piloto. Esa lista te vuelve antes de emitirse.
2. **Corregirlas antes de PB-T4c:** Una tarea de corrección propia, antes de «Editar todo», que reescriba esas guardas para el modelo completo. Cuesta una tarea más con su cadena.
3. **Solo registrarlas:** Se dejan como límite declarado, sin llevarlas a ninguna lista.

**Respuesta del Owner, literal:** «Llevarlas a PB-T4d (Recomendado)».

## 3. Pregunta (encabezado «Nombre»)

**Pregunta, literal:**

> PB-T4b: con «Conservar mi borrador» y después «Actualizar», si alguien renombró el rack en el dibujo fuera del panel, el rack se queda con el nombre del borrador, aunque tú no hayas tocado el nombre. Es lo que decidiste en D-PB-05 (gana el diseño editable completo, y el nombre es editable), y la presentación lo avisa antes del segundo «Sí» como «Cambiado fuera: el nombre.». Lo que no dice es que conservar devuelve el nombre del borrador. ¿Lo das por bueno?

**Opciones:**
1. **Es lo esperado (Recomendado):** Se queda así. El Smoke-PB lo comprueba en AutoCAD, y en la etapa de la interfaz se puede mejorar el texto para que diga qué valor gana (queda anotado en ideas futuras).
2. **Que el texto lo diga ya:** Se cambia ahora el texto de la presentación para que avise de que se conserva el nombre del borrador. Necesita una versión del paquete y una corrección antes de cerrar.

**Respuesta del Owner, literal:** «Es lo esperado (Recomendado)».

## Lo que queda decidido

- **I-01 (CT-PBT4B-20, comprobación (5)), ratificada como errata del paquete,** solo para este caso.
  - La comprobación vale sobre las 9 clases de C-0 (`tests/RackCad.Tests/WorkspaceFullCopyGuardTests.cs:630-632`).
  - Tal cual daría `False` por cinco apariciones en literales de cadena de dos copias de guarda custodiadas: `PushBackWritePathGuardFull.cs` y `WorkspacePilotEditGuardFull.cs`. Una de ellas, `WorkspacePilotEditGuardFull.cs:255`, es la expresión de un ayudante vivo, no un texto inyectado (M-05 de la revisión).
  - No cambia ningún archivo de `src/` ni de `tests/`, y PB-T4b sigue en el intento 1.
  - La verificación del Controller la recibe como disposición de entrada.
- **Los vacíos de M-04 de la revisión van a la lista de PB-T4d:**
  - `WorkspacePilotEditGuardTests.TheReadGuardAllowsAnotherReadThatALaterTaskWouldAddWithItsOwnMark`;
  - `WorkspacePilotEditGuardTests.TheOperationsOfThePortAndTheDecoratorNameNoApiOfWritingTheDrawing`;
  - el vacío que hereda su copia en `WorkspacePilotEditGuardFull.cs:255-258`.

  El cierre de PB-T4b los registra como vacíos declarados. La lista de PB-T4d vuelve al Owner antes de emitirse, como ya estaba decidido.
- **El nombre tras «Conservar» y «Actualizar» es el comportamiento esperado** (D-PB-05, paso 4).
  - El Smoke-PB lo comprueba en AutoCAD.
  - La mejora del texto de la presentación, para que diga qué valor gana, queda anotada en `docs/ideas-futuras.md` para la etapa de la interfaz.
