# I-64 F4B — Decisión del Owner sobre OWNER-1 de la revisión independiente de PB-TC (intento 1)

**Fecha:** 2026-11-18. **Canal:** el chat de la sesión principal, con el diálogo de preguntas de la aplicación.

**Contexto:**
- La revisión independiente del código de PB-TC (intento 1, GREEN `4440f67d`) dio ACEPTABLE CON MENORES, 0/0/5.
- Su M-01 (OWNER-1) es un hueco del diseño del paquete v2.2 en «Ambos lados» con un campo mixto. No es una desviación del Worker.
- La sesión lo comprobó en el código antes de preguntar.

La pregunta y la respuesta se transcriben **literales**.

## La pregunta

> La revisión independiente encontró un caso raro que el plan de PB-TC no contemplaba. En «Ambos lados», si los dos lados tienen valores distintos, la caja aparece vacía (mixta). Ejemplo: lado A = 61,23456 y lado B = 60. Si escribes 61.235, que es lo que se ve del lado A, el lado A conserva su 61,23456 exacto y el B pasa a 61,235. Los dos lados no se igualan y la caja vuelve a quedar vacía. La diferencia numérica es menor de 0,0005, pero la caja no hace lo que esperas. Se evita escribiendo un número que se vea distinto, o el exacto completo (61.23456). Antes de la corrección, los dos lados quedaban en 61,235. Todo lo demás está bien hecho; el Worker cumplió el plan al pie de la letra. ¿Qué hago?

## Las opciones

- **«Registrar y arreglarlo luego (Recomendado)»:** Cierro PB-TC con este caso anotado como limitación conocida. El arreglo te lo propongo junto con las otras cajas que redondean, que ya iba a preguntarte antes de la tarea que separa el estado (PB-T1): es la misma regla. No gasta intento.
- **«Arreglarlo ya en PB-TC»:** Ajusto el plan (una caja que estaba mixta: lo que escribas vale para los dos lados), paso una comprobación corta y relanzo la cadena entera como intento 2 de 3.

## La respuesta del Owner

«Registrar y arreglarlo luego (Recomendado)».

## Alcance

- **PB-TC se cierra sobre el intento 1** si la verificación y los controles lo permiten.
- **OWNER-1 queda como limitación conocida declarada:**
  - en «Ambos lados», con una caja mixta, un número que se ve igual al exacto de un lado no iguala los dos lados;
  - el registro declarado `superseding-characterization.md` lo anota.
- **La propuesta de arreglo vuelve al Owner antes del contrato de PB-T1,** junto con el caso H-31 (F4B-E06, pregunta 4), porque es la misma regla `Keep`.
- **No cambia ningún archivo de `src/` ni de `tests/` del intento 1.**
