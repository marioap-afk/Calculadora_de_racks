---
schema: rackcad-context-pack/v1
id: delivery-validation
when_to_load: build, CI, versiones, bundle, NETLOAD o validación manual
required_docs:
  - AGENTS.md
  - docs/guias/validacion-manual-autocad.md
  - docs/guias/despliegue.md
optional_docs:
  - README.md
code_globs:
  - .github/workflows/*
  - deploy/**/*
  - '*.sln'
  - '**/*.csproj'
usual_gates:
  - ci
  - plugin-build
  - autocad
excludes:
  - declarar AutoCAD validado sin confirmación del dueño
---

# Context Pack: delivery-validation

## Invariantes esenciales

- La suite Domain/Application no requiere AutoCAD.
- UI y Plugin se validan en Debug cuando el contrato lo exige.
- AutoCAD abierto puede bloquear los DLL del directorio de salida.
- Los MSB3277 conocidos no equivalen a errores propios.
- Push de rama no significa integración; el merge es una sesión manual separada.

La guía de validación separa evidencia automatizada de la confirmación de comportamiento dentro de
AutoCAD y exige identificar el DLL exacto del worktree.
