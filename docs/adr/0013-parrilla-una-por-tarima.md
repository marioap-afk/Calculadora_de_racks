# ADR-0013: Parrilla una por tarima, contada en `SelectiveFrontalBuilder.ParrillaRow`

- **Estado:** propuesto
- **Fecha:** 2026-07-22 (fecha de documentación retroactiva; no es la fecha de la decisión original)
- **Decisores:** el dueño del repositorio acepta o rechaza este registro; solo él puede hacerlo. Redacción retroactiva bajo la iniciativa I-07. La evidencia conservada no identifica a los decisores de la decisión histórica original.
- **Iniciativa relacionada:** I-07 — ADRs retroactivos (`docs/adr-retroactivos`)

## Contexto

La parrilla (rejilla o deck de una tarima) del sistema selectivo aparece en tres lugares que deben
coincidir en un número: el dibujo la materializa, el BOM la cuenta y la UI la ofrece y previsualiza.
Si cada consumidor calculara por su cuenta cuántas parrillas hay y qué ancho tienen, el dibujo, el BOM
y la UI podrían divergir tras cualquier cambio de tarima, claro o nivel.

RackCad concentra ese cálculo en una función pura de Application, `SelectiveFrontalBuilder.ParrillaRow`,
consumida por la UI, las vistas y el BOM. La regla general de RackCad —cuando el dibujo, el BOM y la UI
deben concordar en un número, la aritmética vive en UNA función de Application— nombra precisamente esta
función como su ejemplo canónico.

Este ADR documenta retroactivamente una decisión ya vigente. El estado `propuesto` indica que el
registro espera revisión formal del propietario; no reabre la regla. La evidencia conservada demuestra
la regla y su punto único, pero no permite fijar de forma fiable la fecha original ni reconstruir todas
las alternativas evaluadas.

## Decisión

Contar una parrilla por tarima y mantener el cálculo de cantidad y ancho en la función pura
`SelectiveFrontalBuilder.ParrillaRow`, consumida por la UI, las vistas de dibujo y el BOM:

- por defecto, la cantidad es el número de tarimas de diseño del nivel —una parrilla por tarima—;
- un ancho manual (`overrideFrente`) o una cantidad manual (`overrideCount`) ajustan sobre esa base sin
  moverse a otro sitio de cálculo;
- el ancho por defecto proviene del frente de la tarima; sin tarima ni ancho manual no hay parrilla.

Ningún consumidor reimplementa la aritmética: el dibujo, el BOM y la UI llaman a `ParrillaRow`. Una regla
nueva sobre el conteo o el ancho de la parrilla entra en esa función, nunca en la UI ni en el BOM.

## Alternativas consideradas

- **Duplicar el cálculo de la parrilla en la UI y en el BOM** — descartada porque produce divergencia
  entre dibujo, BOM y UI en cuanto cambia una tarima, un claro o un nivel; es la clase de defecto que la
  regla "aritmética en un solo sitio" existe para evitar.

La evidencia conservada no permite reconstruir de forma fiable otras alternativas evaluadas cuando se
tomó la decisión.

## Consecuencias

- Positivas: dibujo, BOM y UI concuerdan por construcción; el ancho de acotado de la cantidad (ADR-0016)
  se apoya en el mismo punto único; una tarima de referencia visual puede excluirse del BOM sin alterar
  el conteo de parrillas reales.
- Negativas / costos aceptados: toda evolución del conteo o del ancho debe pasar por `ParrillaRow`; un
  cálculo colocado por descuido en la UI o el BOM reintroduce el riesgo de divergencia y debe devolverse
  a Application.

## Referencias

- [AGENTS.md — regla en un solo sitio (`SelectiveFrontalBuilder.ParrillaRow`)](../../AGENTS.md)
- [Arquitectura vigente — seguridad selectiva y conteo de parrilla](../ARCHITECTURE.md)
- [Context Pack: system-selective](../context-packs/system-selective.md)
- [`SelectiveFrontalBuilder.ParrillaRow` — cálculo puro de cantidad y ancho](../../src/RackCad.Application/Systems/SelectiveFrontalBuilder.cs)
- [Pruebas de colocación de parrilla](../../tests/RackCad.Tests/SelectiveParrillaPlacementTests.cs)
- [ADR-0016: Cantidad de parrilla acotada por la UI y por el builder](0016-cantidad-parrilla-acotada.md)
