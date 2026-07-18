---
schema: rackcad-context-pack/v1
id: autocad-plugin
when_to_load: comandos, transacciones, jigs, bloques, Xrecords o DrawServices
required_docs:
  - docs/ARCHITECTURE.md
  - docs/guias/generacion-cabecera-lateral.md
optional_docs:
  - docs/guias/despliegue.md
  - docs/guias/validacion-manual-autocad.md
code_globs:
  - src/RackCad.Plugin/**/*.cs
usual_gates:
  - plugin-build
  - autocad
excludes:
  - reglas de geometría o BOM que puedan probarse en Application
---

# Context Pack: autocad-plugin

## Invariantes esenciales

- Plugin es el único proyecto que toca AutoCAD.
- DrawServices materializan planes ya resueltos.
- Las ediciones multivista redefinen por GUID y terminan con un solo `Regen`.
- Parámetros dinámicos se aplican case-insensitive y el patrón ARRAY evita trabajo repetido.
- Un fallo dependiente de bloques reales requiere evidencia del DWG del dueño, no un bloque inventado.
