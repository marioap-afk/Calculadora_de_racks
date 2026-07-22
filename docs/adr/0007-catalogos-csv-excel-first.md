# ADR-0007: Catálogos CSV Excel-first sin base de datos

- **Estado:** aceptado
- **Fecha:** 2026-07-22 (documentación retroactiva y aceptación; no es la fecha de la decisión original)
- **Decisores:** Mario Pérez, dueño del repositorio (aceptó el registro el 2026-07-22). Redacción retroactiva bajo la iniciativa I-07. La evidencia conservada no identifica a los decisores de la decisión histórica original.
- **Iniciativa relacionada:** I-07 — ADRs retroactivos (`docs/adr-retroactivos`)

## Contexto

RackCad distribuye bajo `assets/catalogs/` catálogos tabulares de piezas, bloques, vistas y reglas que
el usuario mantiene como archivos CSV. La documentación y el cargador describen este flujo como
Excel-first: Excel es una herramienta de edición esperada, mientras RackCad consume los archivos
guardados sin depender de Excel durante la ejecución.

La misma carpeta contiene estructuras anidadas en JSON, como defaults y plantillas de cabecera. El
proveedor de catálogos está detrás de `IRackCatalogProvider`; para los catálogos que admiten una
representación tabular, un CSV presente tiene precedencia sobre su JSON hermano. Por tanto, la
existencia de JSON no convierte a esos documentos anidados en catálogos CSV ni contradice la fuente
tabular vigente.

Este ADR documenta retroactivamente una decisión ya vigente. El estado `propuesto` solo deja pendiente
la revisión formal del registro. La evidencia conservada confirma el flujo Excel-first y que no existe
una base de datos como fuente de verdad actual, pero no permite fijar una fecha original de la decisión.

## Decisión

Mantener como archivos CSV versionados los catálogos tabulares editables que RackCad distribuye bajo
`assets/catalogs/`. Esos CSV son la fuente vigente para sus tablas; no existe una base de datos como
fuente de verdad de esos catálogos.

Conservar el contrato de carga respaldado por el flujo real de edición:

- el delimitador es la coma y la primera fila no vacía contiene los encabezados;
- los campos pueden ir entre comillas y una comilla interna se representa duplicada;
- se acepta UTF-8, con o sin BOM, y el lector aplica su fallback Latin-1 cuando la decodificación
  UTF-8 no resulta válida;
- la caché se invalida mediante una firma de los archivos CSV y JSON del directorio basada en nombre,
  tamaño y fecha de modificación;
- cuando un catálogo tabular tiene un CSV y un JSON hermanos, el CSV tiene precedencia.

Excel no es una dependencia de ejecución. Esta decisión tampoco obliga a convertir en CSV los datos
anidados que actualmente pertenecen a JSON, ni impide sustituir el almacenamiento en el futuro mediante
otro ADR si aparece un requisito que lo justifique.

## Alternativas consideradas

- **SQLite o una base de datos local** — documentos iniciales la consideraron junto con JSON y CSV. La
  decisión vigente cerró esa opción para el alcance actual en favor de catálogos CSV editables; la
  documentación conserva una migración de almacenamiento solo como posibilidad futura condicionada a
  nuevos requisitos.
- **JSON como formato principal de todas las tablas editables** — el cargador conserva compatibilidad
  con JSON para catálogos de arreglo, pero la fuente tabular vigente da precedencia al CSV. JSON sigue
  siendo apropiado para defaults y plantillas anidadas.

La evidencia no permite reconstruir comparaciones históricas más detalladas ni atribuir otros criterios
a quienes tomaron la decisión.

## Consecuencias

- Positivas: los catálogos tabulares pueden mantenerse con Excel y revisarse como texto versionado; un
  archivo guardado se recarga al cambiar su firma; la aplicación no requiere un servicio ni motor de
  base de datos.
- Negativas / costos aceptados: el lector debe tolerar variaciones de codificación derivadas del flujo
  de edición tabular y las reglas de quoting; las relaciones entre archivos son referencias textuales y
  no restricciones validadas por un motor relacional; una migración futura requiere una decisión y
  adaptación explícitas.

## Referencias

- [AGENTS.md — catálogos y fuentes de verdad](../../AGENTS.md)
- [HANDOFF — decisiones vigentes](../HANDOFF.md)
- [Guía de catálogos y plantillas](../guias/catalogos-y-plantillas.md)
- [Modelo de datos de catálogos](../guias/modelo-de-datos.md)
- [`CsvCatalogReader`](../../src/RackCad.Application/Catalogs/CsvCatalogReader.cs)
- [`JsonRackCatalogProvider`](../../src/RackCad.Application/Catalogs/JsonRackCatalogProvider.cs)
- [Pruebas del lector CSV](../../tests/RackCad.Tests/CsvCatalogReaderTests.cs)
- [Arquitectura inicial — opciones de almacenamiento preservadas](../archivo/mvp-inicial/arquitectura-autocad-racks.md)
- [ADR-0008: Perfiles estructurales unificados en secciones.csv por rol](0008-secciones-unificadas-por-rol.md)

## Notas posteriores

- **2026-07-22 — Aceptado por Mario Pérez**, dueño del repositorio («Sí, apruebo»). La aceptación recae sobre este registro tal como está en el candidato `600b22e`; no atribuye fecha ni decisores históricos ausentes y conserva las limitaciones documentadas. Decisión versionada en [`docs/automation/decisions/I-07.md`](../automation/decisions/I-07.md).
