# ADR-0008: Parámetros dinámicos mediante definiciones compartidas con patrón ARRAY

- **Estado:** propuesto
- **Fecha:** 2026-07-19 (fecha de documentación retroactiva; no fecha de la decisión)
- **Decisores:** por confirmar durante la revisión formal; registro retroactivo redactado por Codex
- **Iniciativa relacionada:** I-07 — ADRs retroactivos (`docs/adr-retroactivos`)

## Contexto

AutoCAD evalúa los parámetros dinámicos de cada referencia a la que se asignan. La evidencia
preservada identifica la asignación repetida de esos parámetros por referencia como un cuello de
botella al insertar muchas piezas iguales. El costo se repite aunque las piezas compartan bloque,
transformación y valores dinámicos.

RackCad denomina patrón ARRAY a un plan que agrupa piezas geométricamente intercambiables. Application
produce `HeaderGroup`: una pieza configurada como prototipo y una lista de colocaciones. Plugin crea
una definición de bloque anidada para el grupo, aplica allí los parámetros al prototipo y agrega
referencias de esa definición en las posiciones requeridas. El término no designa al comando
interactivo `ARRAY` de AutoCAD.

Este ADR documenta retroactivamente una decisión de rendimiento ya vigente. El estado `propuesto`
solo deja pendiente la revisión formal del registro. La historia conserva el problema general y la
solución adoptada, pero no aporta métricas comparables ni una fecha original que deban atribuirse a
la decisión.

## Decisión

Usar definiciones anidadas compartidas para las piezas repetidas que tengan equivalencia exacta en
todos los datos que afecten la geometría, orientación, parámetros dinámicos y punto de colocación.

Application agrupa las repeticiones en un plan puro. Plugin materializa una sola copia configurada en
la definición anidada y coloca tantas referencias de esa definición como ubicaciones indique el plan.
Así, los parámetros dinámicos se fijan una vez por definición compartida en lugar de repetirse en cada
colocación equivalente.

Las instancias sin equivalencia exacta permanecen independientes. Agrupar no puede cambiar la
geometría: aplanar el plan debe reproducir las instancias originales.

Esta decisión es una estrategia interna de planes y definiciones compartidas; no prescribe el comando
interactivo `ARRAY`, no exige agrupar piezas distintas y no depende de la identidad GUID del rack.

## Alternativas consideradas

- **Fijar parámetros dinámicos en cada referencia repetida** — descartada porque la evidencia
  preservada la identifica como el trabajo repetitivo que dominaba el costo de inserción.

La evidencia conservada no permite reconstruir de forma fiable otras alternativas evaluadas cuando
se tomó la decisión. El comando interactivo `ARRAY` no se registra como alternativa histórica porque
no es el mecanismo implementado por RackCad.

## Consecuencias

- Positivas: las piezas idénticas reutilizan una configuración; disminuyen las evaluaciones repetidas
  de parámetros; Application puede verificar la equivalencia del plan sin AutoCAD.
- Negativas / costos aceptados: el dibujo contiene definiciones anidadas adicionales; la firma de
  agrupación debe incluir todo dato que afecte la geometría, orientación, parámetros dinámicos o
  colocación.

## Referencias

- [AGENTS.md — rendimiento en inserción](../../AGENTS.md)
- [Arquitectura vigente — UI y adaptación AutoCAD](../ARCHITECTURE.md)
- [Context Pack: autocad-plugin](../context-packs/autocad-plugin.md)
- [Guía de generación — patrón ARRAY](../guias/generacion-cabecera-lateral.md)
- [`HeaderInstanceGrouper`](../../src/RackCad.Application/Systems/HeaderInstanceGrouper.cs)
- [`LateralHeaderDrawer`](../../src/RackCad.Plugin/Headers/LateralHeaderDrawer.cs)
- [Pruebas de equivalencia del agrupador](../../tests/RackCad.Tests/HeaderInstanceGrouperTests.cs)
- [ADR-0003: AutoCAD solo en RackCad.Plugin](0003-autocad-solo-en-plugin.md)
