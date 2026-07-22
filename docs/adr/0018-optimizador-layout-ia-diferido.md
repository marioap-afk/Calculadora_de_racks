# ADR-0018: Optimizador de layout con IA diferido; `RACKLAYOUT` es el motor determinista vigente

- **Estado:** aceptado
- **Fecha:** 2026-07-22 (documentación retroactiva y aceptación; no es la fecha de la decisión original)
- **Decisores:** Mario Pérez, dueño del repositorio (aceptó el registro el 2026-07-22). Redacción retroactiva bajo la iniciativa I-07. El diferimiento consta como decisión del dueño (ver Referencias); la evidencia conservada no fija su fecha original.
- **Iniciativa relacionada:** I-07 — ADRs retroactivos (`docs/adr-retroactivos`)

## Contexto

El layout de almacén de RackCad coloca y rellena racks en planta. La versión vigente lo hace de forma
determinista: `WarehouseGridPlanner`, `WarehouseFitChecker` y `WarehouseAutoFill` viven en Application, y
`RACKLAYOUT`/`RACKRELLENAR` son sus adapters en el Plugin; el auto-relleno maximiza el conteo sobre una
rejilla anclada. La documentación de trabajo futuro contempla un optimizador de layout con IA + reglas que
puntúe por beneficio/costo, lea el sitio (muros, columnas) y decida la rejilla; ese optimizador es una
meta futura, no el motor actual.

Confundir el motor determinista vigente con el optimizador futuro llevaría a re-proponer como faltante algo
que ya existe, o a atribuir al comando actual capacidades que no tiene. Esta decisión delimita ambos.

Este ADR documenta retroactivamente una decisión ya vigente. El estado `propuesto` se refiere a este
registro, que aún no ha sido revisado formalmente por el dueño; **no reabre el diferimiento del
optimizador con IA**, que sigue vigente. La evidencia conservada demuestra el motor determinista actual y
que el optimizador es trabajo futuro, pero no permite fijar de forma fiable la fecha original de la
decisión.

## Decisión

Mantener como motor vigente de layout de almacén la colocación y el relleno **deterministas**
(`WarehouseGridPlanner`/`WarehouseFitChecker`/`WarehouseAutoFill` en Application; `RACKLAYOUT` y
`RACKRELLENAR` como adapters del Plugin), que consumen huellas resueltas de planta y maximizan el conteo.

Diferir el **optimizador de layout con IA + reglas** (puntuación por beneficio/costo, lectura del sitio y
decisión de la rejilla) como trabajo futuro. `RACKLAYOUT` no es ese optimizador y no debe presentarse como
tal; el motor determinista es su prerrequisito, no su sustituto.

## Alternativas consideradas

- **Tratar `RACKLAYOUT` como el optimizador (o construir el optimizador con IA dentro del alcance actual)**
  — descartada: el motor vigente es determinista por diseño y el optimizador con IA es una meta futura
  diferida por el dueño. La evidencia conservada no documenta un estudio comparativo formal más allá de esa
  delimitación, y este ADR no lo reconstruye.

## Consecuencias

- Positivas: se distingue con claridad el motor determinista vigente del optimizador futuro; no se
  re-propone como faltante la colocación/relleno que ya existe; el optimizador puede construirse después
  sobre el mismo materializador de rejilla.
- Negativas / costos aceptados: el layout vigente maximiza conteo y no beneficio/costo, y no lee el sitio
  (muros/columnas); esas capacidades quedan pendientes del trabajo futuro.

## Referencias

- [Arquitectura vigente — layout de almacén determinista, no el optimizador IA futuro](../ARCHITECTURE.md)
- [Ideas futuras — layout de almacén (v1 hecho) y optimizador con IA pendiente](../ideas-futuras.md)
- [Historial de HANDOFF preservado — optimizador IA entre lo que no se toma sin confirmar](../archivo/transicion-2026-07/handoff-historial-2026-07.md)
- [HANDOFF §7 — decisiones vigentes pendientes de I-07](../HANDOFF.md)
- [`WarehouseGridPlanner` — planificación determinista de la rejilla](../../src/RackCad.Application/Layout/WarehouseGridPlanner.cs)
- [ADR-0017: Validación estructural de cargas diferida a RAM Elements](0017-validacion-cargas-diferida-ram-elements.md)

## Notas posteriores

- **2026-07-22 — Aceptado por Mario Pérez**, dueño del repositorio («Sí, apruebo»). La aceptación recae sobre este registro tal como está en el candidato `600b22e`; no atribuye fecha ni decisores históricos ausentes y conserva las limitaciones documentadas. Decisión versionada en [`docs/automation/decisions/I-07.md`](../automation/decisions/I-07.md).
