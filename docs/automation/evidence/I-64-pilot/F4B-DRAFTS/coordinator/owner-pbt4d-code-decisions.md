# Decisiones del Owner sobre el código de PB-T4d (I-64 F4B), intento 1

**Fecha:** 2026-11-22. Las tomó después de la revisión independiente del código (`independent-review-code.md`: ACEPTABLE CON MENORES; B 0 / I 0 / M 3, más OWNER-01 a OWNER-03, informativos) y antes de la verificación del Controller.

**El objeto:** el Worker `R20261122T173325Z-c9e1`, con el RED `aa65ecfde87bfe274e9ba518f63dfe94c9e9f61a` y el GREEN `7db6fd0c4e4a27ec05d56cce0daaf0883940a4e2`, sin reparación del RED, sobre la BASE `399c9bec` (F4B-E56).

La sesión preguntó con AskUserQuestion, en una sola vuelta de tres preguntas. Este registro copia el texto exacto. La numeración F4B-E se asigna al custodiarlo.

## 1. Pregunta (encabezado «M-02 (CT-01)»)

**Pregunta, literal:**

> PB-T4d: la revisión independiente del código da ACEPTABLE CON MENORES (0/0/3). El RED y el GREEN son el texto literal del paquete, y la CI cuadra al número. El menor M-02 choca con la letra del contrato: la invariante INV-PBT4D-02 dice que, en el RED, «las autopruebas de CT-PBT4D-01 y 02 fallan sobre el texto inyectado», y en el RED 2 de las 14 autopruebas de CT-01 no discriminan. Son las dos que prueban que un archivo retirado ya no existe: en el RED ese archivo aún existe, así que inyectarlo no cambia nada. En el GREEN discriminan las 14, y el criterio principal del RED se cumple (CT-01 falla por aserción). Es el diseño literal de la clase del paquete; el Worker no podía cambiarlo. ¿Qué hago?

**Opciones:**
1. **Ratificar como límite del diseño (Recomendado):** Lo declaro como errata del contrato y límite del diseño literal de la CT en una disposición para la verificación del Controller y en el cierre, sin cambiar código. Sigo con la verificación en el intento 1.
2. **Corregir la CT:** Se abre una corrección (intento 2 de 3) para reescribir esas dos autopruebas de CT-01 y repetir la cadena RED/GREEN. Cuesta otra ronda de Controller, Worker y CI.

**Respuesta del Owner, literal:** «Ratificar como límite del diseño (Recomendado)».

## 2. Pregunta (encabezado «Smoke-PB»)

**Pregunta, literal:**

> OWNER-02 de la revisión: hay tres caminos de BuildCore cuyo texto cambia, sin cambio de comportamiento. En Core están cubiertos, pero una pasada en AutoCAD los vería en un dibujo real: (1) un plan que borra un lateral obsoleto; (2) un borrador con el nombre en blanco, que escribe el del sobre de origen en todas las vistas; (3) el rechazo de la frontal B de un rack sin lado B. ¿Los sumo al Smoke-PB de PB-T5?

**Opciones:**
1. **Sí, sumarlos (Recomendado):** Se añaden a la lista del Smoke-PB, junto a la pasada de «Editar todo» → «Actualizar» que ya estaba prevista.
2. **No sumarlos:** Basta con la cobertura de Core; el Smoke-PB se queda como estaba.

**Respuesta del Owner, literal:** «Sí, sumarlos (Recomendado)».

## 3. Pregunta (encabezado «Limpieza»)

**Pregunta, literal:**

> OWNER-03 de la revisión (menor M-03): algunos nombres de pruebas y comentarios siguen describiendo como presente el código retirado, por ejemplo «...AndThePilotKeepsItsComparison». No afecta a lo que se prueba, pero renombrar una prueba cambia su identidad y los filtros. ¿Qué hago con esa limpieza?

**Opciones:**
1. **Apuntarla en ideas futuras (Recomendado):** La registro en docs/ideas-futuras.md como limpieza opcional. No se hace nada en PB-T4d ni en PB-T5.
2. **Tarea propia tras PB-T5:** Una tarea pequeña propia, después de PB-T5, que renombre esas pruebas y corrija los comentarios.
3. **No hacer nada:** Se queda como está, sin registrarla.

**Respuesta del Owner, literal:** «Apuntarla en ideas futuras (Recomendado)».

## Lo que queda decidido

- **M-02 (CT-PBT4D-01), ratificado como límite del diseño literal de la CT y errata del contrato,** solo para este caso.
  - INV-PBT4D-02 dice que, en el RED, «las autopruebas de CT-PBT4D-01 y 02 fallan sobre el texto inyectado». En el RED, las dos autopruebas de archivo de CT-01 (`tests/RackCad.Tests/PushBackRetirePilotGuardTests.cs:111-115`) no discriminan, porque el archivo que inyectan ya existe. Las otras 12 de CT-01 y todas las de CT-02 sí discriminan en el RED, y en el GREEN discriminan las 14 de CT-01.
  - El criterio del RED se cumple: CT-PBT4D-01 cae por aserción, con Failed == Selected.
  - No cambia ningún archivo de `src/` ni de `tests/`, y PB-T4d sigue en el intento 1. La verificación del Controller la recibe como disposición de entrada.
- **El Smoke-PB de PB-T5 suma los tres caminos de `BuildCore` de OWNER-02:** un plan que borra un lateral obsoleto; un borrador con el nombre en blanco, que escribe el del sobre de origen en todas las vistas; y el rechazo de la frontal B de un rack sin lado B. Van junto a la pasada de «Editar todo» → «Actualizar» ya prevista.
- **La limpieza de M-03 (OWNER-03) se apunta en `docs/ideas-futuras.md`** como limpieza opcional. No se hace nada en PB-T4d ni en PB-T5.
- **OWNER-01** (la CI de la BASE `399c9bec`, run `45043090710`, en rojo por el cuelgue de la prueba lenta que PB-T4d acota) es informativo: el cierre lo registra como causa conocida, resuelta en esta cadena.
