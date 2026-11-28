# I-64 F4B PB-TC2 — Vuelta al Owner por PBTC2-C14 (parada prevista antes del RED)

**Fecha:** 2026-11-18, madrugada. **Preparada por** la sesión principal en modo autónomo, mientras el Owner dormía.

## Qué pasó

- **El Worker** de PB-TC2 (`R20261118T055535Z-38d3`, intento 1) hizo la medición previa que manda el contrato (D-PBTC2-07) y se **paró antes del RED**, como exige PBTC2-C14.
- **No hay commits:** la rama sigue en `eedb069c`, igual que el remoto, con el árbol limpio. El perfil del usuario está sin cambios (la prueba de limpieza antes y después es idéntica). Es una parada **prevista**, no un fallo.

## El dato que la activa (fila T2-3)

**El diseño medido:** `pb-06-seguridad` con una altura manual de cabecera de **150,00044** (`ManualHeaderHeightOverride`).

**Hoy** (pasar por el panel de avanzados la redondea a 150): el BOM tiene **una** línea de postes, `150 × 36`.

**Con la corrección** (se conserva 150,00044): el BOM tiene **dos** líneas de postes:
- `150 × 32`;
- `150,0004 × 4`, los cuatro postes de cabecera que llevan la altura manual.

En total, 15 líneas frente a 14. Por la letra del contrato, un número distinto de líneas del BOM es «material» (OB-06.4).

**Lo que no cambia:**
- ni piezas, ni niveles, ni coordenadas de más de 0,001: K1, K1o, K2 y K4 están vacías en las seis filas;
- la diferencia máxima es **0,00044"**.

**Las otras cinco filas:** el hueco, la longitud del módulo, la separación, el poste derivado y su refuerzo no cambian el número de líneas. Como mucho cambia una longitud en diezmilésimas.

**Lo importante:** eso mismo ya ocurre hoy **si no se toca el panel**. La medición «sin gesto» de la base da 150,00044 y el BOM partido. La corrección solo hace que deje de redondearse al pasar por la caja. Es decir, el BOM refleja siempre el valor guardado.

## La pregunta propuesta para el Owner

> PB-TC2 se paró, como estaba previsto, en la medición previa. Con una altura manual de cabecera con decimales (150,00044), la corrección hace que el BOM muestre los postes en dos líneas (32 de 150 y 4 de 150,0004) en vez de una (36 de 150). La diferencia es de 0,0004". Eso ya pasa hoy si no se toca el panel de avanzados; la corrección solo evita que pasar por la caja lo redondee. No cambian piezas, niveles ni el dibujo. ¿Qué hago?

**Opciones:**
- **(A) Seguir con la corrección (recomendada).** El BOM refleja siempre el valor guardado, como ya pasa sin tocar el panel. Se relanza el Worker de PB-TC2 desde la misma delegación, sin gastar intento, porque la parada fue de diseño, antes de cualquier código.
- **(B) Seguir, pero dejar fuera solo la altura manual de cabecera.** Esa caja seguiría redondeando al pasar por ella. Es una excepción a «ninguna caja manda», que PB-T1 tendría que declarar.
- **(C) Parar PB-TC2** y llevarlo todo a otra decisión.

## Lo que la sesión hace mientras tanto

- **Prepara el paquete de PB-T1,** solo documentos y sin emitir. PB-T1 depende del cierre de PB-TC2, así que sus líneas se volverán a medir sobre el GREEN de PB-TC2.
- **No relanza nada de PB-TC2** sin la respuesta.
