# Decisión del Owner sobre PB-T4b (I-64 F4B): el coste de la lectura en la precondición 6

**Fecha:** 2026-11-21, después de `owner-pbt4b-list-478-decision.md` (la lista de 478).

**Por qué se preguntó:** el paquete v4.8 (`6ddc239d…`, §6.2.1) dice que si la sonda del coste de la lectura (`pbt4b-read-cost-probe.cs`) no cumple lo esperado, la emisión se detiene y vuelve a la sesión, que lo lleva al Owner. En la precondición 6 de la v4.8, paso 7, la primera corrida no cumplió un criterio («sin regresión») en un solo caso.

La sesión escribió la regla de una tercera corrida antes de medirla (`v48/session-cost-rerun-rule.md`): si la tercera cumplía todo, recomendaría darlo por cumplido. El registro está en `pbt4bcc/compile/compile-check.md` y en `pbt4b-precondition6.json`, campo `Cost`.

**Las tres corridas,** caso `pb-20-ab x4 vistas +200 racks ajenos`, `origen-es-Primary`. La tabla da la mediana en ms, con n = 200; el límite es la mediana de t4a más el mayor de 10 ms y el 30 %:

| Corrida | t4a | t4b | Límite | Resultado global |
|---|---|---|---|---|
| 1.ª (`/d/r4s`, TRX `6487a1c3…59f1`) | 65,2 | **98,2** | 84,8 | NO CUMPLE, solo por este caso |
| 2.ª (`/d/r4s`, TRX `ca1bea9c…8420`) | 46,5 | 47,2 | 60,4 | CUMPLE |
| 3.ª (`/d/r4t`, TRX `67a2bc8e…50f1`) | 65,4 (p95 157,2) | 60,1 | 85,0 | CUMPLE |

En la tercera corrida, un caso cumple muy justo: `pb-01-base x2`, `t4b Begin`, da 42,5 ms con un límite de 42,6. La carga de la máquina no se anotó en las dos primeras corridas.

La sesión preguntó con AskUserQuestion. Este registro copia el texto exacto. La numeración F4B-E se asigna al custodiarlo.

## Pregunta (encabezado «Coste T4b»)

**Pregunta, literal:**

> PB-T4b, el coste de la lectura (lo que debía optimizar PB-T4b, por tu decisión al cerrar PB-T4a): en tres corridas de la sonda, la lectura se materializa una sola vez en todos los casos y la relectura con el origen baja de +68…+128 ms a +36…+67 ms. Un solo caso falló en la primera corrida: un rack A/B con 4 vistas y 200 racks ajenos, cuando el origen es el rack principal, dio 98 ms con un límite de 85. En la segunda y en la tercera cumple (47 y 60 ms), y en la tercera también el tiempo de PB-T4a tuvo un pico. La regla la dejé escrita antes de la tercera corrida: si cumplía, te recomendaría darlo por cumplido. No sé qué cargaba la máquina en la primera corrida. Además, en la tercera un caso cumple muy justo (42,5 ms con límite 42,6). ¿Cómo sigo?

**Opciones:**
1. **Darlo por cumplido (Recomendado):** Dos de tres corridas cumplen todo y el fallo no se repite; parece ruido de la máquina. Se emite PB-T4b con las tres corridas custodiadas, y el coste real en AutoCAD queda para el Smoke-PB, como ya estaba previsto.
2. **Sonda dedicada antes:** Antes de emitir, una sonda solo de ese caso, con más corridas y alternando PB-T4a y PB-T4b para descartar una regresión real. Cuesta una o dos horas más de máquina.
3. **No emitir PB-T4b:** PB-T4b queda sin emitir hasta que decidas otra cosa; F4B se detiene antes de «Editar todo».

**Respuesta del Owner, literal:** «Darlo por cumplido (Recomendado)».

## Lo que queda decidido

- **El coste de la lectura de la precondición 6 de PB-T4b se da por cumplido,** con dos de tres corridas que cumplen todo. Las tres corridas y sus TRX, por sha256, van con la emisión.
- **El coste real en AutoCAD** queda para el Smoke-PB, como ya estaba previsto.
- **No cambia ningún criterio del paquete.** Si una medida posterior no cumple, vuelve a la sesión, que la lleva al Owner, como dice §6.2.1.

## Nota posterior de la sesión (no forma parte de la pregunta ni de la respuesta)

**Fecha:** 2026-11-21, después de la respuesta del Owner.

La comprobación acotada CA-T4b (`independent-review-v1.md`, §20.8 y M-07), que terminó después de la pregunta, señala que el argumento más fuerte estaba dentro de la propia primera corrida, y la pregunta no lo dio:
- en esa corrida, el mismo código de PB-T4b dio 67,5 ms en `cae-al-Primary`, en el bloque anterior, y 66,4 ms en `solo el completo`, en el siguiente;
- solo se aparta un bloque de unos 20 s;
- en el GREEN no hay ninguna causa que lo explique.

Es compatible con el ruido y apoya la opción elegida. Por eso la sesión no repite la pregunta: se lo informa al Owner al emitir.

También señala que una sonda dedicada, si alguna vez hace falta, debería intercalar t4a y t4b por muestra.
