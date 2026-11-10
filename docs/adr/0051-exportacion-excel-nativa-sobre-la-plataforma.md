# ADR-0051: Exportación Excel nativa (Table y PivotTable) sobre la plataforma, sin biblioteca externa

- **Estado:** propuesto
- **Fecha:** 2026-11-09
- **Decisores:** Owner del proyecto (pendiente de aceptación, decisión D-0). Coordinator de I-68 (redacción).
- **Iniciativa relacionada:** I-68 — `feature/bom-excel-analisis-nativo` ([contrato](../initiatives/I-68-bom-excel-analisis-nativo.md), [Proposal V4](../initiatives/I-68-proposal-v4.md), [evidencia](../automation/evidence/I-68-evidence.md)).
- **Consume sin reabrir:** [ADR-0012](0012-producto-sin-dependencias-nuget.md).

## Contexto

El BOM consolidado debe exportarse con una tabla y una tabla dinámica nativas de Excel. RackCad no tiene biblioteca Excel: `XlsxWriter` escribe OOXML a mano con la plataforma y hoy no soporta ninguna de las dos. ADR-0012 permite mantener una capacidad sobre la plataforma cuando el costo sea aceptado, y aclara que no obliga a reimplementar cualquier biblioteca. AutoCAD 2025 trae en su carpeta ClosedXML 0.102.3 y Open XML SDK 2.16, por lo que estas alternativas volverán a proponerse.

## Decisión

Extender `XlsxWriter` para declarar una tabla en una hoja y una tabla dinámica sobre ella, sin paquetes en el código de producto.

La tabla dinámica se escribe con **definición mínima**: la caché y los campos no listan valores, Excel los reconstruye al abrir (`refreshOnLoad`), y la hoja lleva las celdas ya calculadas para que el resumen se vea sin refrescar.

Se fijan los nombres públicos `RackCadBOM` (tabla), `ResumenBOM` (tabla dinámica) y `Resumen` (hoja).

Generar el archivo no requiere Excel ni automatización.

## Alternativas consideradas

- **ClosedXML** — excepción a ADR-0012, varias DLL nuevas y posible choque con la copia que AutoCAD carga; en su carpeta faltan dependencias (ExcelNumberFormat, XLParser, Irony). Capacidad de tabla dinámica no medida.
- **Open XML SDK** — excepción a ADR-0012; AutoCAD trae 2.16 y la 3.x es un salto mayor. La autoría de la tabla dinámica sigue siendo manual, aunque tipada.
- **Excel COM / Interop** — exige Excel instalado en cada equipo y durante las pruebas.
- **Definición completa con listas de valores** — medido: Excel no abre el archivo ante enteros sin `containsInteger` o variantes de mayúsculas listadas aparte.

## Consecuencias

- Positivas: sin dependencias ni riesgo de versiones en el proceso de AutoCAD; salida determinista; no replica las reglas de agrupación de Excel en el XML.
- Negativas / costos aceptados: RackCad posee el XML de la tabla dinámica. Su validez real solo se comprueba del todo con una prueba local en Excel, fuera de CI; medido solo en Excel 16.0 build 20527. El refresco de apertura es asíncrono (medido con Excel oculto): mientras no ocurre, la tabla dinámica no tiene valores en sus filtros, y Actualizar la recupera.
- Revisar esta decisión si aparecen más consumidores o una versión de Excel rechaza la definición mínima.

## Referencias

- [Evidencia I-68](../automation/evidence/I-68-evidence.md) §§4-5
- [ADR-0012](0012-producto-sin-dependencias-nuget.md)
- [`XlsxWriter`](../../src/RackCad.Application/Bom/XlsxWriter.cs)
