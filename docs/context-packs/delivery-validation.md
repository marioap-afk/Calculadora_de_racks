---
schema: rackcad-context-pack/v1
id: delivery-validation
when_to_load: build, CI, versiones, bundle, NETLOAD o validación manual
required_docs:
  - AGENTS.md
  - docs/03-guia-desarrollo-y-validacion.md
  - docs/despliegue.md
optional_docs:
  - README.md
code_globs:
  - .github/workflows/*
  - deploy/**/*
  - '*.sln'
  - '**/*.csproj'
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

La guía 03 es la fuente temporal del checklist hasta que I-06 cree
`docs/guias/validacion-manual-autocad.md` en su fase de migración.
