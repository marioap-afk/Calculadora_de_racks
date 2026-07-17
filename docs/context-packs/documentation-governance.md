---
schema: rackcad-context-pack/v1
id: documentation-governance
when_to_load: estructura documental, enlaces, ADRs, iniciativas o automatización
required_docs:
  - docs/WORKFLOW.md
  - docs/ROADMAP.md
  - docs/AUTOMATION_PLAN.md
  - docs/initiatives/README.md
optional_docs:
  - docs/initiatives/I-06-auditoria-documental.md
code_globs:
  - '*.md'
  - docs/**/*.md
excludes:
  - cambios de código salvo excepción explícita del dueño
---

# Context Pack: documentation-governance

## Invariantes esenciales

- Un documento vigente tiene un propósito; otros enlazan y no copian.
- HANDOFF contiene estado vivo; ROADMAP plan; WORKFLOW proceso; ADRs decisiones.
- Todo movimiento de ruta corrige referentes en la misma fase coherente.
- El estado del ejecutor vive en `docs/automation/state/`; las decisiones pueden vivir en
  `docs/automation/decisions/`.
- Git y resultados verificables prevalecen; el Pull Request puede tener una copia opcional.
- No se requiere GitHub CLI para commit, push y estado versionado.
