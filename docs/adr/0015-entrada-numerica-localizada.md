# ADR-0015: Entrada numérica localizada sin separador de miles

- **Estado:** aceptado
- **Fecha:** 2026-07-22 (documentación retroactiva y aceptación; no es la fecha de la decisión original)
- **Decisores:** Mario Pérez, dueño del repositorio (aceptó el registro el 2026-07-22). Redacción retroactiva bajo la iniciativa I-07. La evidencia conservada no identifica a los decisores de la decisión histórica original.
- **Iniciativa relacionada:** I-07 — ADRs retroactivos (`docs/adr-retroactivos`)

## Contexto

Los usuarios de RackCad capturan medidas en configuraciones regionales que difieren en el separador
decimal: unos escriben `12.5` y otros `12,5`. Interpretar la coma o el punto como separador de miles
transformaría valores ambiguos —`1,234` podría leerse como `1234` o como `1.234`— y produciría medidas
silenciosamente equivocadas. La captura numérica ocurre en muchas ventanas, de modo que la regla de
parseo debe vivir en un solo lugar para no divergir entre diálogos.

RackCad concentra el parseo en una función pura de Application, `LocalizedNumberParser`, y lo consume
desde el control común de UI `NumericField` a través de `NumericFieldValidation`. El parser admite el
punto y la coma como separador decimal y prohíbe los agrupadores de miles.

Este ADR documenta retroactivamente una decisión ya vigente. El estado `propuesto` indica que el registro
espera revisión formal del propietario; no reabre la regla. La evidencia conservada demuestra el parseo y
su punto único, pero no permite fijar de forma fiable la fecha original ni reconstruir todas las
alternativas evaluadas.

## Decisión

Parsear la entrada numérica con una regla localizada única (`LocalizedNumberParser`), consumida por el
control común `NumericField`/`NumericFieldValidation`:

- se acepta el punto o la coma como separador decimal;
- se prohíben los separadores de miles (agrupadores): el estilo de parseo no permite agrupación;
- una sola coma sin punto es, sin ambigüedad, separador decimal en la entrada de RackCad, no de miles;
- no se reinterpretan valores ambiguos para "adivinar" agrupación.

La validación de vacío (opcional → "auto") y el rechazo por rango viven en el mismo lugar puro, para no
reimplementarse por ventana.

## Alternativas consideradas

- **Aceptar separadores de miles según la cultura del sistema** — descartada porque vuelve ambiguos
  valores como `1,234` y puede transformar la medida capturada sin que el usuario lo note.
- **Fijar una sola cultura (solo punto o solo coma)** — descartada porque obligaría a una parte de los
  usuarios a cambiar su forma natural de escribir decimales; la regla admite ambos separadores decimales.

La evidencia conservada no permite reconstruir de forma fiable otras alternativas evaluadas cuando se
tomó la decisión.

## Consecuencias

- Positivas: la captura acepta el separador decimal natural del usuario sin transformar valores; la regla
  y sus mensajes viven en un lugar probado; los diálogos no divergen en el parseo.
- Negativas / costos aceptados: no se admite el separador de miles como comodidad de captura; la entrada
  que dependa de agrupación se rechaza como no numérica.

## Referencias

- [`LocalizedNumberParser` — parseo decimal sin agrupación](../../src/RackCad.Application/Formatting/LocalizedNumberParser.cs)
- [`NumericFieldValidation` — regla única de validación numérica](../../src/RackCad.UI/Controls/NumericFieldValidation.cs)
- [Control común `NumericField`](../../src/RackCad.UI/Controls/NumericField.cs)
- [Context Pack: ui-editors](../context-packs/ui-editors.md)
- [Pruebas del parser localizado](../../tests/RackCad.Tests/LocalizedNumberParserTests.cs)

## Notas posteriores

- **2026-07-22 — Aceptado por Mario Pérez**, dueño del repositorio («Sí, apruebo»). La aceptación recae sobre este registro tal como está en el candidato `600b22e`; no atribuye fecha ni decisores históricos ausentes y conserva las limitaciones documentadas. Decisión versionada en [`docs/automation/decisions/I-07.md`](../automation/decisions/I-07.md).
