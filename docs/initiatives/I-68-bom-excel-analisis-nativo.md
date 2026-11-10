---
schema: rackcad-initiative/v2
id: I-68
title: BOM Excel Native Analysis (ID72) — Cantidad total, tabla y tabla dinámica
type: feature
status: in-progress
workflow: V2
conceptual_initiative: I-68
delivery_unit: I-68
archetype: FOUNDATION EVOLUTION
materiality: [M-04, M-06]
branch: feature/bom-excel-analisis-nativo
base_branch: main
priority: high
size:
depends_on: []
conflicts_with: []
hot_files: []
coordination_strategy: unidad unica; se comprueba la interseccion con las ramas activas en cada reentrada (DC-07)
context_packs: []
consumes: [BillOfMaterials, ConsolidatedBom, ConsolidatedBomBuilder, XlsxWriter]
extends: []
introduces: [capacidad de Table y PivotTable nativas en XlsxWriter]
discovery_ref: docs/automation/evidence/I-68-evidence.md
freeze_ref: docs/initiatives/I-68-proposal-v6.md
freeze_delta_ref:
amendment_refs: []
ov_assignment_ref: docs/initiatives/I-68-proposal-v6.md
decision_refs: [docs/automation/decisions/I-68.md]
evidence_ref: docs/automation/evidence/I-68-evidence.md
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

# I-68 — BOM Excel Native Analysis (ID72)

Status: **FREEZE REGISTRADO; en implementacion**. El estado vivo se deriva de la existencia de `origin/feature/bom-excel-analisis-nativo`.

Workflow: V2 (reclamo posterior a `WORKFLOW_V2_EFFECTIVE_SHA`, base que lo contiene, sin pausa activa: T4).
Unit: `I-68`. Rama: `feature/bom-excel-analisis-nativo`. Autorización: orden directa del Owner (WORKFLOW §2, caso d).

## 1. Identidad y agrupación

Iniciativa conceptual y unidad de entrega únicas. Arquetipo inicial **EXTENSION**, fijado por el Owner como punto de partida;
Discovery lo eleva a **FOUNDATION EVOLUTION** por M-04 y M-06 (LIFECYCLE §3), aceptado por el Owner en D-0.

## 2. Objetivo

Que el BOM exportado a Excel pueda analizarse de inmediato, sin añadir columnas, crear fórmulas, convertir rangos a tabla ni
construir una tabla dinámica a mano.

## 3. Alcance y no-objetivos

La autoridad funcional es la [especificación literal del Owner](I-68-especificacion-id72.md). Limitada a RACKBOMTOTAL: la exportación individual queda fuera del alcance (D-2). El alcance congelado, los invariantes
y la matriz OV se fijarán en el Freeze. No-objetivos fijados por el Owner: ID23 Calculated BOM Items, costos, precios, proveedores,
fórmulas BOM, Expression Engine, RAM Elements, reglas nuevas de BOM, consolidación física nueva y partidas nuevas.

## 4. Fundaciones y evolución

```text
Consumes: BillOfMaterials, ConsolidatedBom, ConsolidatedBomBuilder, XlsxWriter
Extends: ninguno
Introduces: capacidad de Table y PivotTable nativas en XlsxWriter (punto de extensión, M-06)
```

## 5. Discovery, decisiones y Freeze

- Discovery: [evidencia](../automation/evidence/I-68-evidence.md)
- Freeze: [Proposal V6](I-68-proposal-v6.md), congelada
- ADR: [0051](../adr/0051-exportacion-excel-nativa-sobre-la-plataforma.md), aceptado
- Decisiones: [decisions/I-68.md](../automation/decisions/I-68.md)

## 6. Dependencias, archivos calientes y coordinación

Sin dependencias previas. Se comprueba en cada reentrada que ninguna rama activa toque las autoridades de exportación BOM.

## 7. Owner Validation

- Requiere Owner Validation: sí, la salida es directamente visible para el usuario.
- Se valida en Excel real con un XLSX generado por RACKBOMTOTAL; asignación en la [propuesta](I-68-proposal-v6.md) §12.

## 8. Evidencia y entrega

- Evidencia por unidad: [`docs/automation/evidence/I-68-evidence.md`](../automation/evidence/I-68-evidence.md)
- Tag esperado: `integration/I-68`
