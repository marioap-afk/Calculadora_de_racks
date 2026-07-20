# ADR-0003: AutoCAD solo en RackCad.Plugin

- **Estado:** propuesto
- **Fecha:** 2026-07-19 (fecha de documentación retroactiva; no fecha de la decisión)
- **Decisores:** por confirmar durante la revisión formal; registro retroactivo redactado por Codex
- **Iniciativa relacionada:** I-07 — ADRs retroactivos (`docs/adr-retroactivos`)

## Contexto

RackCad se distribuye como un plugin de AutoCAD, pero sus diseños, reglas geométricas, planes de
vista y listas de materiales también deben poder calcularse y probarse fuera de AutoCAD. La solución
vigente separa `RackCad.Domain`, `RackCad.Application`, `RackCad.UI` y `RackCad.Plugin`, con las
dependencias dirigidas hacia Domain. UI usa WPF, pero no referencia la API de AutoCAD.

Application construye modelos resueltos y planes puros. Plugin recibe esos resultados y realiza la
integración concreta: comandos, acceso al documento DWG, transacciones, selección, jigs, importación
de bloques, Xrecords y materialización mediante drawers y DrawServices. Por tanto, confinar AutoCAD
en Plugin no significa trasladar allí la lógica geométrica o de BOM.

Este ADR documenta retroactivamente una decisión ya vigente. El estado `propuesto` indica que el
registro espera revisión formal del propietario; no solicita volver a elegir la arquitectura. La
evidencia conservada registra la separación y su motivo de testeabilidad, pero no permite fijar de
forma fiable la fecha original ni reconstruir todas las alternativas evaluadas entonces.

## Decisión

Mantener `RackCad.Plugin` como el único proyecto de la solución vigente que referencia y usa la API
de AutoCAD:

- `RackCad.Domain` conserva el modelo de dominio sin dependencias de plataforma;
- `RackCad.Application` conserva geometría, resolvers, planes de vista, BOM, catálogos y los
  contratos o modelos serializables de persistencia; todo ello permanece puro y testeable;
- `RackCad.UI` puede usar WPF para captura, edición y preview, pero no tipos ni referencias de
  AutoCAD;
- `RackCad.Plugin` adapta UI y Application a comandos, documentos DWG, transacciones, bloques,
  drawers, jigs y persistencia embebida de AutoCAD.

Los builders de Application determinan qué debe dibujarse; los adaptadores de Plugin determinan cómo
materializar ese plan con la API de AutoCAD. Una regla de geometría o BOM que pueda expresarse sin la
plataforma no se implementa en Plugin.

El alcance es la dependencia de RackCad hacia AutoCAD en la arquitectura actual. No prohíbe crear en
el futuro otros adaptadores de plataforma mediante una decisión explícita, siempre que no introduzcan
tipos de AutoCAD en Domain, Application o UI ni inviertan la dirección de dependencias.

## Alternativas consideradas

La evidencia conservada no permite reconstruir de forma fiable las alternativas evaluadas cuando se
tomó la decisión. En particular, no se presenta la posible creación futura de otros adaptadores como una
alternativa histórica: sería una propuesta distinta que deberá respetar el núcleo independiente de
AutoCAD.

## Consecuencias

- Positivas: geometría y BOM se prueban sin AutoCAD; UI no queda acoplada a sus DLL; la integración
  con el DWG queda localizada; los DrawServices reciben planes ya resueltos.
- Negativas / costos aceptados: Plugin necesita código de adaptación y traducción entre planes puros
  y entidades de AutoCAD; una regla colocada accidentalmente en Plugin puede quedar fuera de la suite
  independiente y debe devolverse a Application.

## Referencias

- [AGENTS.md — mapa y convenciones arquitectónicas](../../AGENTS.md)
- [Arquitectura vigente — principios, capas y adaptación AutoCAD](../ARCHITECTURE.md)
- [Context Pack: architecture-kernel](../context-packs/architecture-kernel.md)
- [Context Pack: autocad-plugin](../context-packs/autocad-plugin.md)
- [Guía de generación de cabecera — separación pura y AutoCAD](../guias/generacion-cabecera-lateral.md)
