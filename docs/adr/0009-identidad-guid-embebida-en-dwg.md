# ADR-0009: Identidad de rack mediante GUID embebido en el DWG

- **Estado:** aceptado
- **Fecha:** 2026-07-22 (documentación retroactiva y aceptación; no es la fecha de la decisión original)
- **Decisores:** Mario Pérez, dueño del repositorio (aceptó el registro el 2026-07-22). Redacción retroactiva bajo la iniciativa I-07. La evidencia conservada no identifica a los decisores de la decisión histórica original.
- **Iniciativa relacionada:** I-07 — ADRs retroactivos (`docs/adr-retroactivos`)

## Contexto

Un rack lógico puede aparecer en el dibujo mediante una o varias vistas. Cada vista se materializa
como una definición de bloque y puede tener una o más referencias insertadas. Los nombres visibles y
los nombres de las definiciones ayudan al usuario, pero pueden cambiar y no identifican de forma
estable al rack. Los identificadores internos de AutoCAD pertenecen a objetos del DWG y tampoco son
la identidad de dominio.

RackCad persiste en cada definición de vista un sobre `RackEmbedDocument`. El sobre distingue tipo de
rack, vista y sección, y contiene `Id`, `Name` y el diseño serializado. `RackEmbedStore` lo convierte a
JSON y `RackBlockData` lo guarda, dividido en valores de texto, en un Xrecord del diccionario de
extensión de la definición. Las definiciones de vistas relacionadas repiten el diseño necesario para
el round-trip y comparten el mismo `Id`.

Este ADR documenta retroactivamente una decisión ya vigente. El estado `propuesto` solo deja pendiente
la revisión formal del registro; no reabre la elección técnica. La historia conservada confirma que
el nombre no se consideró una identidad estable, pero no permite fijar de forma fiable la fecha
original ni todas las alternativas analizadas.

## Decisión

Asignar a cada rack lógico un GUID estable y persistirlo como `RackEmbedDocument.Id` dentro del DWG.
El GUID se crea al insertar inicialmente el rack, se conserva al actualizarlo y se comparte entre las
definiciones de bloque que representan vistas o secciones del mismo rack.

Mantener separados estos conceptos:

- `Id` es la identidad lógica estable del rack;
- `Name` es el nombre visible y puede cambiar sin crear otro rack;
- el nombre de una definición de bloque identifica esa definición en AutoCAD, no al rack lógico;
- `View` y `Section` describen qué representación contiene una definición;
- una `BlockReference` es una colocación de una definición y puede compartirla con otras copias;
- un `ObjectId` de AutoCAD identifica un objeto dentro de una base de datos, no sustituye al GUID.

Para localizar representaciones relacionadas, Plugin lee los sobres de las definiciones y compara su
`Id`. Agregar una vista admitida conserva ese GUID. Una operación que cree un rack lógico independiente
debe asignar una identidad nueva; las vistas ligadas conservan la identidad existente. El mecanismo no
implica que todos los tipos de rack ofrezcan las mismas vistas.

## Alternativas consideradas

- **Usar el nombre visible o el nombre del bloque como identidad primaria** — descartada según la
  evidencia conservada porque el nombre es editable y no constituye una clave estable.

La evidencia disponible no permite reconstruir de forma fiable otras alternativas evaluadas cuando
se tomó la decisión.

## Consecuencias

- Positivas: las vistas pueden localizarse y redibujarse como un solo rack; cambiar el nombre no rompe
  la relación; el diseño y su identidad viajan dentro del DWG; el BOM total y el listado consolidan
  representaciones por `RackEmbedDocument.Id` para no tratarlas como racks distintos.
- Negativas / costos aceptados: cada definición debe conservar un sobre coherente; localizar todas las
  vistas requiere recorrer definiciones y leer sus Xrecords; toda operación que cree un rack lógico
  independiente debe generar una identidad nueva de forma explícita.

## Referencias

- [Arquitectura vigente — identidad, vistas y round-trip](../ARCHITECTURE.md)
- [Context Pack: persistence](../context-packs/persistence.md)
- [Historial preservado — identidad y round-trip](../archivo/transicion-2026-07/01-estado-actual-mvp.md)
- [`RackEmbedDocument` y `RackEmbedStore`](../../src/RackCad.Application/Persistence/RackEmbedDocument.cs)
- [`RackBlockData`](../../src/RackCad.Plugin/Systems/RackBlockData.cs)
- [Consolidación del BOM total por identidad](../../src/RackCad.Plugin/RackInventarioCommands.BomTotal.cs)
- [`RackListBuilder` — agrupación del listado por identidad](../../src/RackCad.Application/Persistence/RackListBuilder.cs)
- [Pruebas del listado por rack lógico](../../tests/RackCad.Tests/RackListBuilderTests.cs)
- [ADR-0010: Actualizar redibuja e Insertar agrega una vista ligada](0010-actualizar-redibuja-insertar-liga-vistas.md)

## Notas posteriores

- **2026-07-22 — Aceptado por Mario Pérez**, dueño del repositorio («Sí, apruebo»). La aceptación recae sobre este registro tal como está en el candidato `600b22e`; no atribuye fecha ni decisores históricos ausentes y conserva las limitaciones documentadas. Decisión versionada en [`docs/automation/decisions/I-07.md`](../automation/decisions/I-07.md).
