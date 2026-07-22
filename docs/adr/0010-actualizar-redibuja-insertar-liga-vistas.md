# ADR-0010: Actualizar redibuja e Insertar agrega una vista ligada

- **Estado:** aceptado
- **Fecha:** 2026-07-22 (documentación retroactiva y aceptación; no es la fecha de la decisión original)
- **Decisores:** Mario Pérez, dueño del repositorio (aceptó el registro el 2026-07-22). Redacción retroactiva bajo la iniciativa I-07. La evidencia conservada no identifica a los decisores de la decisión histórica original.
- **Iniciativa relacionada:** I-07 — ADRs retroactivos (`docs/adr-retroactivos`)

## Contexto

RackCad separa el rack lógico de sus representaciones gráficas. Una vista o sección se guarda en una
definición de bloque; una referencia insertada coloca esa definición en el dibujo. En los sistemas
multivista, varias definiciones comparten el GUID del rack y se reconstruyen desde el mismo diseño.
Editar una vista como si fuera un rack independiente rompería esa relación.

Desde `RACKEDITAR`, el selectivo puede actualizar sus representaciones existentes y agregar las vistas
frontal, lateral o planta que admite; el dinámico puede actualizar las suyas y agregar laterales por
sección, frontales de salida o entrada y planta; cabecera puede actualizar y agregar lateral o planta.
Cada uno aplica restricciones propias entre la inserción inicial y la edición. La cama de rodamiento
FlowBed conserva una sola vista lateral: su editor actualiza la definición seleccionada, pero no ofrece
inserción de vistas adicionales.

Este ADR registra retroactivamente una semántica ya vigente. Su estado `propuesto` significa que el
documento espera revisión formal del propietario, no que se esté reconsiderando el comportamiento.
La evidencia histórica describe la convención y las vistas disponibles, pero no permite reconstruir
de forma fiable la fecha original ni todas las alternativas evaluadas.

## Decisión

Aplicar estas operaciones cuando un rack existente se abre mediante `RACKEDITAR`:

- **Actualizar** reconstruye el diseño y redefine en sitio las definiciones de las representaciones
  existentes que correspondan al flujo implementado por el editor. Conserva el GUID, no crea otro rack
  lógico y no inserta una vista adicional. Las referencias que comparten una definición reflejan su
  redefinición.
- **Insertar una vista** crea una representación adicional ligada al rack existente y conserva su
  identidad. Cada editor puede sincronizar previamente las representaciones existentes según su flujo
  implementado. La nueva definición registra el mismo GUID e inserta una referencia mediante el flujo
  de colocación.

Una vista adicional solo se inserta desde un rack ya existente, para que disponga de diseño e
identidad fuente. `View` y, cuando aplica, `Section` distinguen la representación sin convertirla en
otro rack. La definición de bloque contiene la geometría y el sobre de esa representación; la
`BlockReference` únicamente la coloca en el dibujo.

Esta decisión gobierna el flujo de edición. No cambia la inserción inicial que crea un rack nuevo ni
promete capacidades multivista para todos los editores. En particular, FlowBed solo redibuja su vista
lateral existente; selectivo, dinámico y cabecera exponen únicamente las vistas que implementa cada
sistema.

## Alternativas consideradas

La evidencia conservada no permite reconstruir de forma fiable las alternativas evaluadas cuando se
tomó la decisión. Las posibles vistas futuras de un sistema no se presentan aquí como alternativas
históricas ni quedan autorizadas por este ADR.

## Consecuencias

- Positivas: una edición mantiene coherentes las representaciones del mismo rack; agregar una vista no
  crea una identidad accidental; las copias de una definición se actualizan sin recolocarse; una vista
  adicional queda asociada a la identidad del rack existente.
- Negativas / costos aceptados: Plugin debe localizar por GUID y redibujar cada definición aplicable;
  cada editor debe declarar qué vistas admite y mantener explícita la diferencia entre actualizar e
  insertar.

## Referencias

- [Arquitectura vigente — identidad, vistas y round-trip](../ARCHITECTURE.md)
- [Context Pack: ui-editors](../context-packs/ui-editors.md)
- [Historial preservado — identidad y convención de botones](../archivo/transicion-2026-07/01-estado-actual-mvp.md)
- [`RackBlockFinder` — búsqueda de vistas por GUID (`ScanEnvelopes`)](../../src/RackCad.Plugin/RackBlockFinder.cs)
- [Edición del selectivo](../../src/RackCad.Plugin/RackSelectivoCommands.cs)
- [Edición del dinámico](../../src/RackCad.Plugin/RackDinamicoCommands.cs)
- [Edición de cabecera](../../src/RackCad.Plugin/RackCabeceraCommands.cs)
- [Edición de la cama de rodamiento (FlowBed)](../../src/RackCad.Plugin/RackCamaCommands.cs)
- [ADR-0009: Identidad de rack mediante GUID embebido en el DWG](0009-identidad-guid-embebida-en-dwg.md)

## Notas posteriores

- **2026-07-22 — Aceptado por Mario Pérez**, dueño del repositorio («Sí, apruebo»). La aceptación recae sobre este registro tal como está en el candidato `600b22e`; no atribuye fecha ni decisores históricos ausentes y conserva las limitaciones documentadas. Decisión versionada en [`docs/automation/decisions/I-07.md`](../automation/decisions/I-07.md).
