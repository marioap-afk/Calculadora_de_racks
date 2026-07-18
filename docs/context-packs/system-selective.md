---
schema: rackcad-context-pack/v1
id: system-selective
when_to_load: diseño, geometría, vistas, seguridad, cotas o BOM selectivo
required_docs:
  - docs/ARCHITECTURE.md
optional_docs:
  - docs/guias/catalogos-y-plantillas.md
code_globs:
  - src/RackCad.Domain/Systems/Selective*.cs
  - src/RackCad.Application/Systems/Selective*.cs
  - src/RackCad.UI/RackSelectiveWindow*
  - src/RackCad.UI/Safety*.cs
  - src/RackCad.Plugin/Systems/Selective*.cs
  - tests/RackCad.Tests/Selective*.cs
usual_gates:
  - plugin-build
  - autocad
excludes:
  - sistemas dinámico y flow-bed salvo contratos compartidos explícitos
---

# Context Pack: system-selective

## Invariantes esenciales

- Flujo: design -> resolver -> resolved -> builders.
- “Frente”, “fondo” y “tramo” no son intercambiables.
- `SelectiveDepthLayout` gobierna la rejilla de varios fondos.
- `SelectiveMedioFrente.Resolve` gobierna frontal, planta y BOM.
- Seguridad, cotas y parrilla consumen planes/helpers compartidos.
- Todo cambio de selección de seguridad cruza `DeepCopy`, DTO y round-trip.
