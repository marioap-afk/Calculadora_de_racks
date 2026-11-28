# Decisiones del Owner sobre el código de PB-T4c (I-64 F4B), intento 1

**Fecha:** 2026-11-21. Las tomó después de la revisión independiente del código (`independent-review-code.md`: ACEPTABLE CON MENORES, condicionado a I-01; B 0 / I 1 / M 7, más OWNER-01 a OWNER-05) y antes de la verificación del Controller.

**El objeto:** el Worker `R20261121T205731Z-41db`, con el RED `280c79beac61d31b03f86488c03686eef4f694bc` y el GREEN `7cc1076eae8159294a1498ab556851977223b64c`, sin reparación del RED, sobre la BASE `d5026014` (F4B-E54).

La sesión preguntó con AskUserQuestion, en una sola vuelta de dos preguntas. Este registro copia el texto exacto. La numeración F4B-E se asigna al custodiarlo.

## 1. Pregunta (encabezado «CT-11»)

**Pregunta, literal:**

> PB-T4c: el Worker terminó (CI del GREEN 4/4, totales exactos) y la revisión independiente da ACEPTABLE CON MENORES (0 bloqueantes, 1 importante, 7 menores), condicionado a esto. Una prueba nueva, CT-PBT4C-11, no sigue la receta del paquete: el paquete pide sustituir la «linea base resuelta» por la de otro fixture y comprobar que el borrador no queda sucio; la prueba sustituye otra referencia, el informe de módulos, por un objeto vacío. La línea esperada se cumple y el código de src/ no está en duda. Es la misma forma que CT-PBT4B-10, ya cerrada en PB-T4b. El Worker no lo declaró, y la revisión ve un indicio de que la receta literal podría no dar esa línea. ¿Cómo sigo?

**Opciones:**
1. **Ratificar como errata (Recomendado):** Se acepta la receta de la prueba como errata del paquete, solo este caso y sin cambio de código, igual que CT-PBT4B-10. Siguen la verificación (con tu decisión como entrada), los controles y el cierre en el intento 1.
2. **Intento 2 con la receta fijada:** Una versión nueva del paquete fija la receta y su línea esperada, se reemite el contrato y el Worker rehace la tarea en el intento 2 de 3. Cuesta varias horas.

**Respuesta del Owner, literal:** «Ratificar como errata (Recomendado)».

## 2. Pregunta (encabezado «Recarga»)

**Pregunta, literal:**

> PB-T4c: la revisión encontró un caso que el paquete no trata. Si el borrador está limpio pero marcado como posiblemente obsoleto (el dibujo cambió fuera), «Editar» recarga antes de editar, y «Editar todo» no: abre el editor sobre la base vieja. Al editar, el borrador pasa a sucio y obsoleto, y al aplicar sale el camino de «Conservar mi borrador», que avisa de lo que cambió fuera antes de escribir. No se escribe nada sin avisar, pero el usuario no recibe la recarga silenciosa que sí da «Editar». ¿Qué hago?

**Opciones:**
1. **Aceptarlo y corregir luego (Recomendado):** Se queda así en PB-T4c. El Smoke-PB lo mira con un cambio externo del dibujo, y que «Editar todo» recargue antes, como «Editar», queda como corrección para PB-T4d o una tarea posterior, que te vuelve con su lista.
2. **Corregirlo ahora:** Una versión del paquete en la que «Editar todo» recarga antes, y el Worker rehace PB-T4c en el intento 2 de 3. Cuesta varias horas.
3. **Es lo querido:** Se queda así sin anotar una corrección: editar sobre la base vieja y resolverlo al aplicar con «Conservar mi borrador» es el comportamiento que quieres.

**Respuesta del Owner, literal:** «Aceptarlo y corregir luego (Recomendado)».

## Lo que queda decidido

- **I-01 (CT-PBT4C-11), ratificada como errata del paquete,** solo para este caso.
  - La segunda receta de la prueba (`tests/RackCad.Tests/PushBackEditAllTests.cs:589-600`) sustituye `Editor.SideA.LastModuleReconciliation` por un objeto vacío, en lugar de la línea base resuelta por la de otro fixture (§5.3, fila 11). Es la misma forma que CT-PBT4B-10.
  - La línea esperada `sustituir-referencia estado=Applied isdirty=False` se cumple.
  - No cambia ningún archivo de `src/` ni de `tests/`, y PB-T4c sigue en el intento 1. La verificación del Controller la recibe como disposición de entrada.
- **«Editar todo» sobre un borrador Clean posiblemente obsoleto (OWNER-02): se acepta en PB-T4c.**
  - El Smoke-PB lo comprueba con un cambio externo del dibujo y un borrador Clean.
  - La corrección, que «Editar todo» recargue antes como «Editar», queda para PB-T4d o una tarea posterior. Esa tarea vuelve al Owner con su lista.
