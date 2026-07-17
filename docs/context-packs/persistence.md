---
schema: rackcad-context-pack/v1
id: persistence
when_to_load: DTO, stores, schemas, identidad, round-trip o compatibilidad legacy
required_docs:
  - docs/ARCHITECTURE.md
optional_docs:
  - docs/modelo-de-datos.md
code_globs:
  - src/RackCad.Application/Persistence/**/*.cs
  - tests/RackCad.Tests/**/*DocumentTests.cs
  - tests/RackCad.Tests/**/*StoreTests.cs
excludes:
  - cambios de formato sin fallback y prueba legacy
---

# Context Pack: persistence

## Invariantes esenciales

- Se persiste el diseño; la geometría resuelta se reconstruye.
- Todo campo nuevo de un DTO compatible es nullable y define fallback legacy.
- `SchemaGuard` protege contra versiones futuras no soportadas.
- Cada cambio de contrato lleva round-trip y escenario legacy.
- El GUID embebido identifica el rack; el nombre visible no es identidad estable.
- Un campo nuevo de `SelectiveSafetySelection` también se copia en `DeepCopy`.
