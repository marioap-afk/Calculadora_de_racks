# ADR-0005: Perfiles estructurales unificados en secciones.csv por rol

- **Estado:** propuesto
- **Fecha:** 2026-07-19 (fecha de documentación retroactiva; no fecha de la decisión)
- **Decisores:** por confirmar durante la revisión formal; registro retroactivo redactado por Codex
- **Iniciativa relacionada:** I-07 — ADRs retroactivos (`docs/adr-retroactivos`)

## Contexto

Los postes, elementos de celosía, largueros y separadores son perfiles estructurales que comparten la
mayor parte de sus datos de catálogo. Mantener una tabla vigente por cada uso duplicaría el esquema y
permitiría que atributos comunes evolucionaran de forma divergente.

RackCad mantiene esos perfiles en `assets/catalogs/secciones.csv`. Cada fila conserva un `id` textual y
declara un `rol`; el proveedor proyecta la hoja unificada en las colecciones tipadas que consumen los
sistemas. El `id` se preserva en esa proyección. Campos de otros catálogos como `pieceId`, y campos de
perfil en defaults o plantillas, pueden referirse a esa misma identidad textual. El cargador no impone
por ello unicidad, cardinalidades ni claves foráneas de base de datos.

Este ADR documenta retroactivamente una decisión ya vigente. El estado `propuesto` deja pendiente la
revisión formal del registro. La evidencia preservada demuestra la unificación y el fallback de
compatibilidad, pero no permite fijar la fecha original ni reconstruir todas las alternativas evaluadas.

## Decisión

Mantener una sola fuente vigente para los perfiles estructurales: `secciones.csv`. Diferenciar el uso de
cada fila mediante `rol`, con los valores que procesa el proveedor actual: `POSTE`, `CELOSIA` o
`CELOSÍA`, `LARGUERO` y `SEPARADOR`.

`JsonRackCatalogProvider` divide las filas por rol en `PostProfiles`, `TrussProfiles`, `BeamProfiles` y
`SpacerProfiles`, preservando el `id` para las búsquedas y referencias de catálogo. `familia` y los demás
atributos describen la sección; no sustituyen a `id` ni a `rol`.

No reintroducir catálogos paralelos vigentes para postes, celosía, largueros o separadores mientras esta
decisión siga vigente. Los archivos separados de postes, celosía y largueros se leen únicamente como
fallback de compatibilidad cuando la fuente unificada no aporta filas; cuando `secciones.csv` aporta
filas, los archivos separados no son la fuente activa.

Esta decisión define el modelo unificado de perfiles. El medio general CSV Excel-first pertenece a
ADR-0004 y puede evolucionar separadamente del significado de `id` y `rol`.

## Alternativas consideradas

- **Un catálogo separado por cada uso estructural** — fue el esquema anterior para postes, celosía y
  largueros y se conserva solo como fallback de compatibilidad con el esquema anterior. Se descartó como
  fuente vigente al unificar los atributos comunes y distinguir el uso mediante `rol`.

La evidencia no demuestra que se evaluaran otras cardinalidades, reglas de unicidad o modelos
relacionales; este ADR no las atribuye a la decisión histórica.

## Consecuencias

- Positivas: una sola hoja mantiene los atributos comunes; los consumidores conservan colecciones
  tipadas sin duplicar las fuentes; las referencias siguen usando el mismo `id` después de la proyección
  por rol.
- Negativas / costos aceptados: el proveedor debe clasificar cada fila; un rol vacío o no reconocido no
  entra en las colecciones tipadas; la compatibilidad con catálogos separados exige mantener el fallback
  mientras el esquema anterior continúe soportado.

## Referencias

- [Catálogo unificado `secciones.csv`](../../assets/catalogs/secciones.csv)
- [Guía de catálogos y plantillas](../guias/catalogos-y-plantillas.md)
- [Modelo de datos de catálogos](../guias/modelo-de-datos.md)
- [`SeccionCatalogEntry` y entradas tipadas](../../src/RackCad.Application/Catalogs/CatalogEntries.cs)
- [`JsonRackCatalogProvider` — división por rol y fallback](../../src/RackCad.Application/Catalogs/JsonRackCatalogProvider.cs)
- [Pruebas del catálogo unificado](../../tests/RackCad.Tests/SeccionesCatalogTests.cs)
- [ADR-0004: Catálogos CSV Excel-first sin base de datos](0004-catalogos-csv-excel-first.md)
