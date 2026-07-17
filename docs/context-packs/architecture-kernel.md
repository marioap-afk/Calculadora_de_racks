---
schema: rackcad-context-pack/v1
id: architecture-kernel
when_to_load: contratos compartidos, capas, registros, namespaces o sistemas nuevos
required_docs:
  - docs/ARCHITECTURE.md
  - docs/adr/README.md
optional_docs:
  - docs/auditoria-arquitectura-2026-07.md
code_globs:
  - src/RackCad.Domain/**/*.cs
  - src/RackCad.Application/**/*.cs
excludes:
  - cambios de producto no declarados por la iniciativa
---

# Context Pack: architecture-kernel

## Invariantes esenciales

- Dependencias: Domain <- Application <- UI <- Plugin.
- AutoCAD solo se referencia desde Plugin.
- Diseño editable, geometría resuelta y adapter de plataforma son responsabilidades distintas.
- Un registro objetivo elimina switches compartidos; no autoriza introducir abstracciones antes de
  la iniciativa correspondiente.
- Las decisiones aceptadas se consultan en el índice ADR y no se reabren sin un ADR nuevo.

Leer solo los subárboles de código afectados por el contrato; los globs son un mapa, no alcance.
