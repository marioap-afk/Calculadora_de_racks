---
schema: rackcad-context-pack/v1
id: catalogs-data
when_to_load: CSV, JSON, FKs, perfiles, bloques, plantillas o validación de catálogos
required_docs:
  - docs/guias/catalogos-y-plantillas.md
  - docs/guias/modelo-de-datos.md
optional_docs:
  - docs/ARCHITECTURE.md
code_globs:
  - assets/catalogs/*
  - src/RackCad.Application/Catalogs/**/*.cs
  - src/RackCad.Application/RackFrames/CatalogIds.cs
usual_gates:
  - owner-validation
  - autocad
excludes:
  - blocks-library.dwg
  - catálogos copiados bajo bin u obj
---

# Context Pack: catalogs-data

## Invariantes esenciales

- Los CSV son la fuente editable y prevalecen sobre JSON cuando ambos existen.
- Encoding acepta UTF-8 y Windows-1252; la cache se invalida por firma de archivos.
- Cambiar una columna exige cambiar su propiedad C# y comprobar FKs/IDs.
- `blockName` debe coincidir exactamente con el bloque real del DWG.
- `blocks-library.dwg` pertenece al dueño y nunca se versiona ni se inventa.
