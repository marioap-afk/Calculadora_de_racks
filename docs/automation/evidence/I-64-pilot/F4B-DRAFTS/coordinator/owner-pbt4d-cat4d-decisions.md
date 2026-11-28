# Decisiones del Owner sobre PB-T4d (I-64 F4B) tras la comprobación acotada CA-T4d

**Fecha:** 2026-11-22. Tras la comprobación acotada de la v5.2 (`independent-review-pbt4d-check.md`, sha256 `439837f9…dced3`: EMITIBLE TRAS CORRECCIONES, B 0 / I 2 / M 7).

**Por qué se preguntó:**
- **I-01:** la lista borra las pruebas n.º 48 (`TheSystemIsRebuiltFromTheDesignTextOfAMemberOfThePlan`) y n.º 49 (`TheSystemRebuiltForAMemberCarriesTheRackNameOfItsEnvelope`) de WPUR. No prueban la corrida del piloto, sino `PushBackPilotSystemRebuild`, que se queda: lo llaman `DesiredNamesOf` y el escritor real de «Actualizar» (`RackPilotUpdateWriter.cs:131`).
  - Son las únicas que cubren `PushBackPilotSystemRebuild.cs:32` y 6 ramas.
  - Por la regla de `owner-pbt4d-run-coverage-decision.md`, vuelve al Owner.
- **I-02:** el criterio de cobertura de la precondición 6 solo miraba las 436 líneas de WPUR en dos archivos, y no veía lo que pierden las demás bajas en el resto del código que se queda.

La sesión preguntó con AskUserQuestion, en una sola vuelta de dos preguntas. Este registro copia el texto exacto. La numeración F4B-E se asigna al custodiarlo.

## 1. Pregunta (encabezado «48 y 49»)

**Pregunta, literal:**

> PB-T4d: la comprobación acotada da EMITIBLE TRAS CORRECCIONES (0 bloqueantes, 2 importantes, 7 menores). El importante que es tuyo: dos pruebas de la lista que se borran (las n.º 48 y 49 de la corrida genérica) en realidad no prueban el piloto, sino la reconstrucción del rack desde el texto del diseño (PushBackPilotSystemRebuild), que se queda y que usa el «Actualizar» real. Son las únicas que la cubren: al borrarlas, una línea y 6 ramas de código que se queda quedarían sin prueba, justo lo que pediste evitar. ¿Qué hago?

**Opciones:**
1. **Sustituirlas (Recomendado):** Pasan a equivalentes sobre el modelo completo, igual que las otras 27: serían 29 sustitutas. No queda sin prueba esa reconstrucción.
2. **Borrarlas y declararlo:** Se borran con registro y se declara esa línea y esas 6 ramas como pérdida de prueba.

**Respuesta del Owner, literal:** «Sustituirlas (Recomendado)».

## 2. Pregunta (encabezado «Huecos nuevos»)

**Pregunta, literal:**

> PB-T4d: el otro importante es del método. La medida de cobertura que pediste solo miraba los dos archivos de la corrida genérica, y por eso no vio el caso anterior. La corrijo para que mire todo el código que se queda, comparando la suite entera antes y después de la retirada. Puede encontrar algún hueco más (ya hay un indicio pequeño en el panel compuesto, que probablemente cubre la UI). Para no volver a preguntarte por cada uno: ¿qué hago con los huecos que encuentre esa medida ampliada?

**Opciones:**
1. **Sustituirlos siempre (Recomendado):** Todo camino que se queda y pierda su única prueba recibe una sustituta sobre el modelo completo, con la misma regla, sin preguntarte otra vez. Solo te vuelvo si una sustituta no se puede escribir, o si salen más de 10 nuevas.
2. **Preguntarme cada vez:** Cada hueco nuevo que salga te vuelve antes de seguir, como hasta ahora.

**Respuesta del Owner, literal:** «Sustituirlos siempre (Recomendado)».

## Lo que queda decidido

- **Las pruebas n.º 48 y n.º 49 de WPUR** se sustituyen por equivalentes sobre el modelo completo: las sustitutas de WPUR pasan a 29.
- **La cobertura de la precondición 6 se amplía a todo el código de `src/` que se queda.** Compara la suite entera de la base con la del GREEN, línea a línea y rama por rama.
- **Regla permanente para PB-T4d:** todo camino de código que se queda y pierda su única prueba recibe una sustituta sobre el modelo completo, sin volver al Owner.
  - **Vuelve al Owner solo si** una sustituta no se puede escribir, o si salen más de 10 sustitutas nuevas sobre las 29.
  - **Lo que cubra la UI** se mide con la cobertura de sus filtros antes de darlo por perdido.
