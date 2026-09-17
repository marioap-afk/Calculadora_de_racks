---
schema: rackcad-context-pack/v1
id: documentation-governance
when_to_load: estructura documental, enlaces, ADRs, iniciativas o automatización
required_docs:
  - docs/WORKFLOW.md
  - docs/INITIATIVE_LIFECYCLE.md
  - docs/FOUNDATIONS.md
  - docs/ROADMAP.md
  - docs/AUTOMATION_PLAN.md
  - docs/initiatives/README.md
  - docs/initiatives/PROMPT_TEMPLATES.md
optional_docs:
  - docs/initiatives/I-06-auditoria-documental.md
code_globs:
  - '*.md'
  - docs/**/*.md
usual_gates:
  - owner-decision
  - owner-validation
excludes:
  - cambios de código salvo excepción explícita del dueño
---

# Context Pack: documentation-governance

## Invariantes esenciales

- Un documento vigente tiene un propósito; otros enlazan y no copian. El mapa de dueño único vive en
  `WORKFLOW.md` §10.
- WORKFLOW gobierna Git, integración, cadencia y transición; AGENTS pruebas/evidencia;
  INITIATIVE_LIFECYCLE diseño/revisión; la guía manual Owner Validation.
- HANDOFF contiene estado vivo; ROADMAP plan; ADRs aceptados decisiones; Freeze+A-n alcance congelado.
- FOUNDATIONS es un registro descriptivo subordinado a ADR/Freeze y código/pruebas.
- PROMPT_TEMPLATES es procedimental y subordinado; TEMPLATE describe el contrato mutable.
- Todo movimiento de ruta corrige referentes en la misma fase coherente.
- El estado del ejecutor vive en `docs/automation/state/`; las decisiones pueden vivir en
  `docs/automation/decisions/`.
- La evidencia V2 por unidad vive en `docs/automation/evidence/<unit>-evidence.md`; los hechos
  post-merge viven en el tag anotado de integración.
- Git y resultados verificables prevalecen; el Pull Request puede tener una copia opcional.
- No se requiere GitHub CLI para commit, push y estado versionado.
- Workflow V2 materializado no es efectivo hasta que exista `WORKFLOW_V2_EFFECTIVE_SHA`.
