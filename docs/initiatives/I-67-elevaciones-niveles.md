---
schema: rackcad-initiative/v2
id: I-67
title: Elevaciones de niveles en Dinamico y Push Back (H58)
type: fix
status: claimed
workflow: V2
conceptual_initiative: I-67
delivery_unit: I-67
archetype: FOUNDATION EVOLUTION
materiality: [M-03, M-05]
branch: fix/elevaciones-niveles
base_branch: main
priority: high
size: S
depends_on: []
conflicts_with: []
hot_files: []
coordination_strategy: unidad unica; se comprueba la interseccion con las ramas activas en cada reentrada (DC-07)
context_packs: [system-dynamic-flowbed, persistence]
consumes: [RackFirstLevelDatum, autoridad vertical de Push Back]
extends: []
introduces: []
discovery_ref: docs/automation/evidence/I-67-evidence.md
freeze_ref: docs/initiatives/I-67-proposal-v2.md
freeze_delta_ref:
amendment_refs: []
ov_assignment_ref: docs/initiatives/I-67-proposal-v2.md
decision_refs: [docs/automation/decisions/I-67.md]
evidence_ref: docs/automation/evidence/I-67-evidence.md
automation_state_path:
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

# I-67 — Elevaciones de niveles en Dinámico y Push Back (H58)

Workflow: V2 (reclamo posterior a `WORKFLOW_V2_EFFECTIVE_SHA`, base que lo contiene, sin pausa activa: T4).
Unit: `I-67`. Rama: `fix/elevaciones-niveles`. Autorización: orden directa del Owner (WORKFLOW §2, caso d).

## 1. Identidad y agrupación

Iniciativa conceptual y unidad de entrega únicas: un defecto observable, un resultado verificable. Arquetipo inicial
**EXTENSION**, fijado por el Owner como punto de partida; Discovery lo eleva a **FOUNDATION EVOLUTION** por M-03 y M-05 (LIFECYCLE §3).

## 2. Objetivo

Que cada intervalo entre niveles consecutivos quede en el primer troquel real que satisface o supera su separación
requerida, medido desde la elevación **real resuelta** del nivel anterior.

## 3. Problema

H58, reportado por el Owner en Dinámico: separaciones obtenidas 44" / 46" / 44", esperadas 46" / 46" / 46". El
diagnóstico histórico (`DynamicLoadBeamGeometry.ResolveLevels`, `Math.Round`, acumulación teórica) es una hipótesis
hasta que Discovery lo confirme contra `origin/main`.

## 4. Alcance y no-objetivos

La autoridad de alcance, invariantes, no-objetivos y matriz OV es el [Freeze](I-67-proposal-v2.md). No-objetivos fijados por
el Owner: rediseño general de Dinámico o Push Back, representación de tarimas, opciones nuevas de UI, reglas nuevas de
pendiente, Dynamic Rebuild Robustness, cambios estructurales/RAM, Workspace/ID30, migraciones de persistencia no
justificadas y refactors oportunistas.

## 5. Fundaciones y evolución

```text
Consumes: RackFirstLevelDatum (ADR-0031); autoridad vertical de Push Back (ADR-0031 §9)
Extends: ninguno
Introduces: ninguno
```

## 6. Discovery, decisiones y Freeze

- Discovery: [evidencia](../automation/evidence/I-67-evidence.md)
- Freeze: [I-67-proposal-v2.md](I-67-proposal-v2.md)
- Freeze delta: no aplica
- A-n: ninguna
- Decisiones: [decisions/I-67.md](../automation/decisions/I-67.md)

## 7. Dependencias, archivos calientes y coordinación

Sin dependencias previas. Se comprueba en cada reentrada que ninguna rama activa toque las autoridades de niveles.

## 8. Gates funcionales

Se fijan en el Freeze. Orden del Owner: F0 preflight/caracterización/Freeze, F1 elevaciones y regresión Dinámico, F2
impacto Push Back, F3 persistencia, edición, vistas y cotas, F4 regresión final y conformidad; consolidables si Discovery
demuestra que el cambio es menor.

## 9. Owner Validation

- Asignación OV: [Freeze](I-67-proposal-v2.md)
- Requiere AutoCAD: sí, cambia comportamiento de dibujo.
- Requiere Owner Validation: sí, cambia comportamiento de dibujo.

## 10. Evidencia y entrega

- Evidencia por unidad: [`docs/automation/evidence/I-67-evidence.md`](../automation/evidence/I-67-evidence.md)
- Tag esperado: `integration/I-67`

La integración es manual, serializada y sin auto-merge conforme a `WORKFLOW.md`.
