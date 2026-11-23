---
schema: rackcad-initiative/v2
id: I-69
title: Selectivo Component Catalog Expansion
type: feature
status: discovery
workflow: V2
conceptual_initiative: I-69
delivery_unit: I-69
archetype: EXTENSION
materiality: []
branch: feature/selectivo-catalogo-componentes
base_branch: main
priority: high
size: S
depends_on: []
conflicts_with: [I-64]
hot_files: [docs/ROADMAP.md, assets/catalogs/, src/RackCad.UI/Systems/Selective/RackSelectiveWindow.xaml.cs, src/RackCad.Domain/Systems/Selective/SelectivePalletDesign.cs]
coordination_strategy: Bootstrap limitado a fila propia de ROADMAP; Discovery de solo lectura; resolver intersecciones de producto antes de implementar.
context_packs: [catalogs-data, persistence, system-selective, ui-editors, delivery-validation]
consumes: []
extends: []
introduces: []
discovery_ref: docs/automation/evidence/I-69-evidence.md
freeze_ref:
freeze_delta_ref:
amendment_refs: []
ov_assignment_ref:
decision_refs: [docs/automation/decisions/I-69.md]
evidence_ref: docs/automation/evidence/I-69-evidence.md
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

# I-69 — Selectivo Component Catalog Expansion

Workflow V2. Unidad conceptual y de entrega unica, autorizada por el Owner (WORKFLOW §2,
caso d). Prioridad HIGH / SMALL PRODUCT IMPROVEMENT.

## 1. Objetivo y alcance

Caracterizar el dropdown de largueros por celda/nivel del Selectivo y preparar nuevas opciones,
renombres visibles y los elementos Guarda trasera y Entrepaño caída de agua. El
[mandato literal](../automation/decisions/I-69-owner-mandate.original.txt) fija los 30 puntos
de la primera entrega y sus limites.

**IMPLEMENTATION AUTHORIZATION = NO.** Solo reclamo, bootstrap y Discovery; sin Freeze.

## 2. Agrupacion y arquetipo

Agrupacion provisional: opciones de componentes del mismo editor. Discovery confirmara la
autoridad comun y los gates necesarios. EXTENSION es el arquetipo inicial pedido; materiality
vacio es provisional, no una conclusion negativa. M-01..08 siguen UNKNOWN hasta DC-09.
Identidad basada en descripcion, acoplamiento material o schema/migracion obligan a STOP
antes del Freeze y a evaluar FOUNDATION EVOLUTION. Rige LIFECYCLE §3 ante UNKNOWN.

## 3. No-objetivos

Sin larguero por lado, duplicacion de Selectivo, rediseño del editor/niveles, live-edit,
Workspace nuevo, ID23, rediseño de consolidacion BOM, RAM, expresiones nuevas ni plataforma
general de catalogos. I-68/XlsxWriter no se modifica.

## 4. Consumes / Extends / Introduces

UNKNOWN en bootstrap; se completan tras DC-08. Las listas vacias no afirman ausencia.

## 5. Coordinacion

ROADMAP solo recibe la fila propia en bootstrap; no se edita HANDOFF. Ramas activas y sus
puntas viven en evidencia. I-64 comparte SelectiveEditorOpenTests: no se escribe esa prueba
durante Discovery. Las intersecciones se revisan antes de toda escritura de producto.
I-25 (guardas traseras) es backlog antecedente; no se reclama, cierra ni modifica.

## 6. Plan inicial y routing

F0 Discovery/compatibilidad/propuesta; F1 largueros; F2 seguridad; F3 regresion;
F4 Owner Validation/preparacion de cierre; READY segun LIFECYCLE. Solo F0 esta autorizado
hasta la primera devolucion al Owner, sin congelar. Consolidar gates si la evidencia lo permite.

Routing I-61 previsto: caracterizacion Balanced; identidad Balanced/Deep si ambigua;
adiciones Routine/Balanced; regresion Balanced; revision independiente segun LIFECYCLE.
No se declaran celdas elegibles ni ejecucion delegada verificada sin medir el entorno.

## 7. Evidencia y Owner Validation

Smoke acotado: listado, renombres, seleccion/reapertura, las dos seguridades y su comportamiento
acordado. Matriz T1..T15 del mandato, por asignar en propuesta. Evidencia:
[I-69-evidence.md](../automation/evidence/I-69-evidence.md).
Integracion manual; tag esperado integration/I-69. Sin Candidato ni autorizacion de merge.
