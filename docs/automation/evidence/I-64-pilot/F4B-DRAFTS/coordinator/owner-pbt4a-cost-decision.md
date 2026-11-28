# Decisión del Owner sobre el coste de la doble lectura de PB-T4a (I-64 F4B, OWNER-02)

**Fecha:** 2026-11-20.

**Por qué se preguntó:**
- **El hallazgo:** la revisión independiente del código de PB-T4a lo señaló como OWNER-02: cada lectura del panel hace ahora el piloto y además el modelo completo. El paquete decía que el coste se mediría en la precondición 6 (R-T4-16), y no se midió.
- **La condición del Owner** (`owner-pbt4-decisions.md`, pregunta 4) manda volver por «un efecto material sin medir».
- **La medición:** antes de preguntar, la sesión hizo medir la parte pura de la lectura, sin AutoCAD, en copias exportadas de la base `5aaacce3` y del GREEN `faef7020`, con una prueba temporal que no entró en ningún commit (`read-cost/read-cost.md`, sus tablas y el sha256 de sus TRX).

**Medianas, aumento del GREEN sobre la base, en los fixtures mayores:**

| Lectura | Aumento |
|---|---|
| `Begin` | +40 a +80 ms |
| Relectura que cae al `Primary`, o con el origen en el `Primary` | +35 a +86 ms |
| Relectura con el origen encontrado | +80 a +175 ms (p95 hasta 257 ms) |

**De dónde sale el coste:** casi todo es la materialización completa (`PushBackEditableStateSeat.LoadExisting`, de 25 a 78 ms por llamada). La doble lectura del piloto añade solo de 2 a 10 ms. Con el origen encontrado se materializa dos veces y se descarta la del `Primary`. Todo pasa dentro del `LockDocument` y en cada una de las seis lecturas.

## Pregunta (encabezado «Coste T4a»)

**Pregunta, literal:**

> PB-T4a hace que el panel lea también el modelo completo en cada gesto. Medido sin AutoCAD sobre los dibujos de prueba más grandes, eso añade +40 a +80 ms a la lectura al empezar a editar, +35 a +86 ms a la relectura habitual y +80 a +175 ms cuando relee en una vista que no es la principal, porque entonces materializa el modelo dos veces y descarta una. Pasa dentro del bloqueo del documento. PB-T4a aún no usa esa lectura (la usará PB-T4b), y en el resto está verificada (14/14) y sin defectos. Tu condición pide volver por un efecto material. El coste real con dibujos reales solo lo da el Smoke-PB. ¿Qué hago?

**Opciones:**
1. **Cerrar y optimizar en PB-T4b (Recomendado):** PB-T4a cierra con el coste registrado como limitación. PB-T4b, que ya reescribe esas lecturas, añade a su diseño elegir la vista de origen antes de leer, para materializar una sola vez (quita el +80 a +175 ms del peor caso). Lo que quede, propio del modelo completo, se vuelve a medir en PB-T4b y en el Smoke-PB. Si la optimización rompe una prueba fuera de la lista, te vuelve.
2. **Cerrar y medir en Smoke-PB:** PB-T4a cierra con el coste registrado y no se optimiza nada hasta medirlo con dibujos reales en el Smoke-PB de PB-T5; entonces decides.
3. **Corregir PB-T4a ahora:** PB-T4a no cierra: pasa a su intento 2 para hacer la optimización ya, con su paquete, revisión y cadena. Es lo más limpio, pero retrasa PB-T4b varias horas.

**Respuesta del Owner, literal:** «Cerrar y optimizar en PB-T4b (Recomendado)».

## Lo que queda decidido

- **PB-T4a cierra** con el coste medido registrado como limitación conocida, en el paquete de cierre y en `docs/ideas-futuras.md`.
- **PB-T4b añade a su diseño** elegir la vista de origen antes de leer, para materializar el modelo completo **una sola vez** por lectura.
- **El coste que quede,** propio del modelo completo, se vuelve a medir con la misma prueba sobre el GREEN de PB-T4b y en el Smoke-PB.
- **Si la optimización rompe una prueba fuera de la lista de 475,** vuelve al Owner.
