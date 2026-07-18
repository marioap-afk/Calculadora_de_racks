---
schema: rackcad-context-pack/v1
id: ui-editors
when_to_load: WPF, ViewModels, controles, previews, selección o Editor Shell
required_docs:
  - docs/ARCHITECTURE.md
optional_docs:
  - docs/guias/validacion-manual-autocad.md
code_globs:
  - src/RackCad.UI/**/*
usual_gates:
  - owner-validation
  - autocad
excludes:
  - API de AutoCAD
  - geometría duplicada en code-behind
---

# Context Pack: ui-editors

## Invariantes esenciales

- UI no referencia AutoCAD.
- Preview, BOM y dibujo consumen reglas de Application.
- Los grids dinámicos pueden construirse en código; los diálogos estáticos usan XAML.
- No se reconstruyen controles completos por cada clic.
- El Editor Shell y los controles compartidos son arquitectura objetivo hasta que sus iniciativas se
  integren; no se asumen como existentes.
