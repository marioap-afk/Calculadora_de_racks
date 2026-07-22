# ADR-0016: Cantidad de parrilla acotada por la UI y por el builder

- **Estado:** propuesto
- **Fecha:** 2026-07-22 (fecha de documentación retroactiva; no es la fecha de la decisión original)
- **Decisores:** el dueño del repositorio acepta o rechaza este registro; solo él puede hacerlo. Redacción retroactiva bajo la iniciativa I-07. La evidencia conservada no identifica a los decisores de la decisión histórica original.
- **Iniciativa relacionada:** I-07 — ADRs retroactivos (`docs/adr-retroactivos`)

## Contexto

El usuario puede forzar cuántas parrillas colocar en un claro. Un número mayor de las que caben dibujaría
parrillas fuera del marco o con holgura negativa, un resultado que nunca es correcto. Además, aunque la
cantidad fuera válida al capturarla, el claro puede estrecharse después (al cambiar postes o niveles), y
la cantidad guardada dejaría de caber.

RackCad acota la cantidad en dos puntos complementarios: la UI del selectivo guía la captura mostrando
cuántas parrillas caben y valida la entrada, y el builder puro `SelectiveFrontalBuilder.ParrillaRow`
acota la cantidad final a las que caben. El conteo base —una parrilla por tarima— y el ajuste viven en la
misma función pura (ADR-0013).

Este ADR documenta retroactivamente una decisión ya vigente. El estado `propuesto` indica que el registro
espera revisión formal del propietario; no reabre la regla. La evidencia conservada demuestra el acotado
en el builder y la guía de la UI, pero no permite fijar de forma fiable la fecha original ni reconstruir
todas las alternativas evaluadas.

## Decisión

Acotar la cantidad de parrilla en ambos extremos:

- **La UI rechaza/limita** una cantidad que no cabe en el claro: el diálogo de parrilla del selectivo
  muestra cuántas caben y la captura numérica valida la entrada;
- **El builder acota** como garantía final: `SelectiveFrontalBuilder.ParrillaRow` limita la cantidad a
  las que caben en el claro (mínimo entre la cantidad pedida y las que entran).

Así, un claro estrechado después de capturar la cantidad degrada de forma segura —se dibujan solo las que
caben— y nunca se materializan parrillas fuera del marco ni con holgura negativa. La garantía del builder
es independiente de la validación de la UI: aunque un flujo omitiera la validación, el dibujo sigue
acotado.

## Alternativas consideradas

- **Confiar solo en la validación de la UI** — descartada porque un cambio posterior del claro, o un flujo
  que no pase por la UI, dejaría la cantidad guardada dibujando fuera del marco.
- **Confiar solo en el acotado del builder** — descartada como única defensa porque la UI debe además dar
  retroalimentación al capturar (cuántas caben) en lugar de aceptar en silencio un valor que se recortará.

La evidencia conservada no permite reconstruir de forma fiable otras alternativas evaluadas cuando se
tomó la decisión.

## Consecuencias

- Positivas: el dibujo nunca excede el marco por una cantidad forzada; el diseño degrada de forma segura
  cuando el claro cambia; la UI informa al usuario en el momento de capturar.
- Negativas / costos aceptados: la cantidad efectiva puede ser menor que la pedida tras estrechar el
  claro; la regla de "cuántas caben" debe mantenerse coherente entre la guía de la UI y el acotado del
  builder (ambos derivan del mismo ajuste de `ParrillaRow`).

## Referencias

- [`SelectiveFrontalBuilder.ParrillaRow` — acotado de la cantidad a las que caben](../../src/RackCad.Application/Systems/SelectiveFrontalBuilder.cs)
- [Diálogo de parrilla del selectivo (guía de captura)](../../src/RackCad.UI/SafetyParrillaGridWindow.cs)
- [`NumericFieldValidation` — rechazo por rango en la captura](../../src/RackCad.UI/Controls/NumericFieldValidation.cs)
- [Context Pack: system-selective](../context-packs/system-selective.md)
- [Pruebas de colocación de parrilla](../../tests/RackCad.Tests/SelectiveParrillaPlacementTests.cs)
- [ADR-0013: Parrilla una por tarima, contada en `SelectiveFrontalBuilder.ParrillaRow`](0013-parrilla-una-por-tarima.md)
- [ADR-0015: Entrada numérica localizada sin separador de miles](0015-entrada-numerica-localizada.md)
