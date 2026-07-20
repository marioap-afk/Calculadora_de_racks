# ADR-0009: Código de producto sin dependencias NuGet

- **Estado:** propuesto
- **Fecha:** 2026-07-19 (fecha de documentación retroactiva; no fecha de la decisión)
- **Decisores:** por confirmar durante la revisión formal; registro retroactivo redactado por Codex
- **Iniciativa relacionada:** I-07 — ADRs retroactivos (`docs/adr-retroactivos`)

## Contexto

RackCad está compuesto por los cuatro proyectos bajo `src/`: `RackCad.Domain`, `RackCad.Application`,
`RackCad.UI` y `RackCad.Plugin`. Las dependencias externas de esos proyectos afectan la restauración,
el despliegue y la carga del producto en el entorno de AutoCAD.

Los cuatro proyectos de producto no declaran actualmente `PackageReference`. El proyecto
`RackCad.Tests` sí usa paquetes para el SDK de pruebas y xUnit, pero no forma parte de la aplicación
distribuida. Como consecuencia visible de la política, la exportación XLSX construye directamente un
paquete OOXML mediante APIs de la plataforma, sin agregar una biblioteca externa al producto.

Este ADR registra retroactivamente una política ya vigente. El estado `propuesto` solo indica que el
registro espera revisión formal; no reabre la política ni adopta por anticipado una excepción. La
evidencia preservada respalda la simplificación del despliegue, pero no permite reconstruir de forma
fiable la fecha original ni todas las alternativas consideradas.

## Decisión

Mantener sin paquetes NuGet a los proyectos que forman el producto distribuido:

- `src/RackCad.Domain/RackCad.Domain.csproj`;
- `src/RackCad.Application/RackCad.Application.csproj`;
- `src/RackCad.UI/RackCad.UI.csproj`;
- `src/RackCad.Plugin/RackCad.Plugin.csproj`.

Los proyectos de pruebas quedan fuera de esta restricción y pueden declarar los paquetes necesarios
para ejecutar la infraestructura de validación.

No agregar una dependencia NuGet al código de producto sin acuerdo explícito del propietario y un ADR
que documente la excepción, su alcance, impacto de despliegue y criterio de mantenimiento. Esta regla
no prohíbe NuGet para siempre: define el valor por defecto y el proceso para cambiarlo mediante una
decisión arquitectónica expresa.

Cuando una capacidad pueda implementarse razonablemente con la plataforma y el costo sea aceptado,
puede mantenerse dentro del producto sin dependencia externa, como ocurre con el escritor OOXML. Este
ejemplo es una consecuencia de la política, no una obligación de reimplementar cualquier biblioteca.

## Alternativas consideradas

La evidencia conservada no permite reconstruir de forma fiable las alternativas evaluadas cuando se
adoptó la política. En particular, no se presenta aquí ninguna dependencia futura concreta como si
hubiera sido evaluada o aprobada.

## Consecuencias

- Positivas: disminuyen las dependencias externas que deben restaurarse, desplegarse y mantenerse
  compatibles; cada excepción recibe revisión arquitectónica visible.
- Negativas / costos aceptados: algunas capacidades requieren implementación y pruebas propias, como
  el escritor OOXML; una biblioteca externa potencialmente útil no puede incorporarse como cambio
  incidental y necesita acuerdo y ADR.

## Referencias

- [AGENTS.md — política de dependencias](../../AGENTS.md)
- [Arquitectura vigente — producto sin paquetes NuGet](../ARCHITECTURE.md)
- [Solución RackCad](../../RackCad.sln)
- [Proyecto Domain](../../src/RackCad.Domain/RackCad.Domain.csproj)
- [Proyecto Application](../../src/RackCad.Application/RackCad.Application.csproj)
- [Proyecto UI](../../src/RackCad.UI/RackCad.UI.csproj)
- [Proyecto Plugin](../../src/RackCad.Plugin/RackCad.Plugin.csproj)
- [Proyecto de pruebas y sus paquetes](../../tests/RackCad.Tests/RackCad.Tests.csproj)
- [`XlsxWriter` sin dependencia externa](../../src/RackCad.Application/Bom/XlsxWriter.cs)
