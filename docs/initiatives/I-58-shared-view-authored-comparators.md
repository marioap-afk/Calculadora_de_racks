---
schema: rackcad-initiative/v2
id: I-58
title: Shared View Foundation Authored Comparators
type: architecture
status: in-progress
workflow: V2
conceptual_initiative: I-58
delivery_unit: I-58
archetype: FOUNDATION EVOLUTION
materiality: [M-02, M-03, M-04, M-05, M-06, M-08]
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

Esta ejecucion: F2 — IMPLEMENT AUTH-13 FOR FOUR KINDS / RED -> GREEN.
F1 = COMPLETE / COORDINATOR AGREED, incluido F1-CR-01 RESOLVED. Se autorizan unicamente los
comparators Dynamic, PushBack, Cantilever y Cabecera bajo Consensus Freeze V3, con carrier raw,
lectura estricta local, igualdad tipada y materializacion de dominio profunda.
F3 no abierto. Sin Candidate, integracion ni desbloqueo I-55 G12. No se modifican Domain,
DTOs, stores, resolvers, UI, Plugin, main, I-55, I-52, I-57, R3 ni los anexos congelados.

## Autoridades por referencia

WORKFLOW §§2,4.1,11; AGENTS (evidencia); INITIATIVE_LIFECYCLE §§3-7; FOUNDATIONS;
PROMPT_TEMPLATES A/C; ADR-0034 y ADR-0044; I-57 Proposal V5, R3 y evidencia F6.
Consumes: Shared View Foundation integrada; Authored vs Effective (limite).
Extends: AUTH-13, conservando ownership neutral de I-57. Introduces: ninguna autoridad transversal.

## Entregables y revision

Consensus Freeze V3 y anexos permanecen intactos. Los acuerdos V3 y la resolucion Architect de AR58-V2-01
son el estado de entrada comunicado por el Owner; esta ejecucion no emite nuevos veredictos de diseño.
CR58-01/02 RESOLVED; EXP-01 CLASS B FOR I-58 ONLY / CONFIRMED; M-03 ACTIVATED.

F1 tiene [evidencia propia](../automation/evidence/I-58-f1/README.md), con manifiestos CT/MM,
carriers TEST-ONLY, oraculos tipados, control de sabotajes, RED por kind y PASS vacuos separados.
F1 esta acordado por Coordinator. F2 tiene [evidencia propia](../automation/evidence/I-58-f2/README.md);
los resultados se atribuyen a su identidad exacta y F3 requiere una revision posterior de Coordinator.
La entrada FOUNDATIONS futura solo se publicara al cierre con conformidad; no se modifica ahora.

Coordinator = AGREED ON V3 / F1 AGREED
Architect = AGREED ON V3
AR58-V2-01 = RESOLVED BY ARCHITECT
Frozen = YES
F1 = COMPLETE / COORDINATOR AGREED
F2 = OPEN / IMPLEMENTATION AUTHORIZED
F3 = NOT OPEN
I-55 G12 = NOT UNBLOCKED
IMPLEMENTATION AUTHORIZATION = YES — F2 ONLY
