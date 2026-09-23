---
schema: rackcad-initiative/v2
id: I-58
title: Shared View Foundation Authored Comparators
type: architecture
status: discovery
workflow: V2
conceptual_initiative: I-58
delivery_unit: I-58
archetype: FOUNDATION EVOLUTION
materiality: [M-02, M-03, M-05, M-06, M-08]
branch: architecture/shared-view-authored-comparators
base_branch: main
depends_on: [I-57]
conflicts_with: [I-55, I-52]
hot_files: [src/RackCad.Application/Systems/Shared/RackAuthoredComparator.cs]
coordination_strategy: Foundation integrada antes de consumo I-55 G12; sin cambios en ramas consumidoras.
context_packs: [architecture-kernel, persistence, documentation-governance]
consumes: [Shared View Foundation, Authored vs Effective]
extends: [AUTH-13]
introduces: []
discovery_ref: docs/initiatives/I-58-discovery.md
freeze_ref: docs/initiatives/I-58-freeze-draft.md
freeze_delta_ref:
amendment_refs: []
ov_assignment_ref: docs/initiatives/I-58-freeze-draft.md
decision_refs: [docs/automation/decisions/I-58.md]
evidence_ref: docs/automation/evidence/I-58-evidence.md
requires_ci: true
requires_plugin_build: true
requires_autocad: true
requires_owner_decision: true
requires_owner_validation: true
automation:
  enabled: false
  auto_merge: false
  max_attempts: 3
---

# I-58 — Shared View Foundation Authored Comparators

## Identidad, objetivo y agrupacion

Unidad propia V2 autorizada expresamente por el Owner sin fila previa (WORKFLOW §2(d)).
Los cuatro kinds comparten el mismo problema de autoridad authored entre hermanas y el punto AUTH-13.
FOUNDATION EVOLUTION propuesto; Coordinator confirma tras Discovery. La secuencia forzada exige
integrar esta fundacion antes de I-55 G12 (excepcion de separacion, lifecycle §2).

## Alcance autorizado

Esta ejecucion: D/F0 y preparacion F1 solamente. Discovery delta de Dynamic, PushBack, Cantilever y
Cabecera; matrices tipadas, schema/unknown, characterization, borrador completo y paquete Architect.
Sin produccion. Cama, AUTH-15, geometria/BOM, policy de producto y cambios de persistencia quedan fuera.
No tocar main, I-55, I-57, R3, Proposal V5 historica ni integration/I-57.

## Autoridades por referencia

WORKFLOW §§2,4.1,11; AGENTS (evidencia); INITIATIVE_LIFECYCLE §§3-7; FOUNDATIONS;
PROMPT_TEMPLATES A/C; ADR-0034 y ADR-0044; I-57 Proposal V5, R3 y evidencia F6.
Consumes: Shared View Foundation integrada; Authored vs Effective (limite).
Extends: AUTH-13, conservando ownership neutral de I-57. Introduces: ninguna autoridad transversal.

## Entregables y revision

Discovery, Freeze draft y paquete Architect se enlazan desde la evidencia canonica.
D/F0 produce material revisable, no declara consenso. F1 queda preparado, no abierto como gate funcional.
La entrada FOUNDATIONS futura solo se publicara al cierre con conformidad; no se modifica ahora.

Continuacion actual: AR58-V2-01 CORRECTION — DISCOVERY / CHARACTERIZATION / FREEZE V3 ONLY.
Misma rama/worktree/claim. CR58-01/02 RESOLVED y EXP-01 B confirmado SOLO I-58. No reabrir sin evidencia
materialmente nueva. M-03 ACTIVATED por perdida de fallback al materializar. Ver Discovery/Freeze V3
completos y diagnostico propio; ningun cambio de produccion, gate funcional ni cierre de iniciativa.

AR58-V2-01 = CORRECTION PROPOSED / ARCHITECT RE-REVIEW REQUIRED
Coordinator = REVIEW REQUIRED ON V3
Architect = CHANGES REQUIRED ON V2 / PENDING V3
Frozen = NO
F1 = NOT OPEN
I-55 G12 = NOT UNBLOCKED
IMPLEMENTATION AUTHORIZATION = NO
