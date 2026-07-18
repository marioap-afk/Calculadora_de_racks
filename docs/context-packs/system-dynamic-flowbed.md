---
schema: rackcad-context-pack/v1
id: system-dynamic-flowbed
when_to_load: dinámico, camas, largueros IN/OUT, seguridad o BOM asociado
required_docs:
  - docs/ARCHITECTURE.md
optional_docs:
  - docs/guias/catalogos-y-plantillas.md
code_globs:
  - src/RackCad.Domain/Systems/Dynamic*.cs
  - src/RackCad.Domain/Systems/FlowBed*.cs
  - src/RackCad.Application/Systems/Dynamic*.cs
  - src/RackCad.Application/Systems/FlowBed*.cs
  - src/RackCad.UI/RackDynamicSystemWindow*
  - src/RackCad.UI/RackFlowBedWindow*
  - src/RackCad.Plugin/Systems/Dynamic*.cs
  - src/RackCad.Plugin/Systems/FlowBed*.cs
  - tests/RackCad.Tests/Dynamic*.cs
  - tests/RackCad.Tests/FlowBed*.cs
usual_gates:
  - plugin-build
  - autocad
excludes:
  - reabrir ADR-0002 sin un ADR sustituto
---

# Context Pack: system-dynamic-flowbed

## Invariantes esenciales

- `DynamicRackDesign` se persiste y el resolver materializa `DynamicRackSystem`.
- Alturas de poste dependen de frentes adyacentes, no de un máximo global indiscriminado.
- La cama dinámica compone `FlowBedLateralBuilder`; no copia su geometría.
- Laterales por poste conservan `Section` y todas las vistas comparten GUID.
- Seguridad y BOM cuentan piezas físicas una vez aunque tengan varias proyecciones.
