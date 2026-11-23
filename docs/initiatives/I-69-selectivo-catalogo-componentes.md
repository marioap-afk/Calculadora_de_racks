---
schema: rackcad-initiative/v2
id: I-69
title: Selectivo Component Catalog Expansion
type: feature
status: discovery
workflow: V2
conceptual_initiative: I-69
delivery_unit: I-69
archetype: FOUNDATION EVOLUTION
materiality: [M-02, M-03, M-05, M-06]
branch: feature/selectivo-catalogo-componentes
base_branch: main
priority: high
size: S
depends_on: []
conflicts_with: [I-64]
hot_files: [docs/ROADMAP.md, assets/catalogs/, src/RackCad.UI/Systems/Selective/RackSelectiveWindow.xaml.cs, src/RackCad.Domain/Systems/Selective/SelectivePalletDesign.cs]
coordination_strategy: Bootstrap limitado a fila propia de ROADMAP; Discovery de solo lectura; resolver intersecciones de producto antes de implementar.
context_packs: [catalogs-data, persistence, system-selective, ui-editors, delivery-validation]
consumes: [Catalogos CSV y compatibilidad legacy, Rack Identity, View Identity, Authored vs Effective, Unknown-field Preservation, Copia central de seguridad, Agent Execution Protocol]
extends: [Registros de BeamProfiles y SafetyElements y mappings compatibles pendientes de alcance]
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

Agrupacion por mandato del Owner: opciones de componentes del editor, con autoridades de
catalogo e identidad compartidas. EXTENSION fue el arquetipo inicial pedido. Discovery
descarta descripcion como identidad, pero encuentra consumidores compartidos, efecto del
orden sobre el default y riesgo de una segunda PARRILLA.

Arquetipo operativo FOUNDATION EVOLUTION **provisional**: M-02/03/05/06 siguen UNKNOWN y
cuentan activados conforme a LIFECYCLE §3, hasta EXP-09. Esto no afirma una migracion/schema
necesarios. El camino de datos compatibles + seguridad B podria permitir EXTENSION antes
del Freeze, con evidencia y decision de Coordinator/Architect. STOP antes del Freeze.

## 3. No-objetivos

Sin larguero por lado, duplicacion de Selectivo, rediseño del editor/niveles, live-edit,
Workspace nuevo, ID23, rediseño de consolidacion BOM, RAM, expresiones nuevas ni plataforma
general de catalogos. I-68/XlsxWriter no se modifica.

## 4. Consumes / Extends / Introduces

Consumes: catalogos CSV/compatibilidad legacy (ADR-0007/0020), identidad/vistas,
authored/effective, preservacion de desconocidos en limites declarados, copia central de
seguridad (ADR-0014) e I-61 para coordinacion. Extends previsto: filas y mappings compatibles.
Introduces: ninguna foundation/schema/migracion decidida. DC-08 en evidencia identifica
fuentes, simbolos y pruebas; no se inventa una entrada de FOUNDATIONS.

[Proposal V1](I-69-proposal-v1.md) contiene decisiones Owner pendientes, invariantes
propuestos, matriz T1..T15 y smoke. Frozen: NO; no es un contrato acordado.

## 5. Coordinacion

ROADMAP solo recibe la fila propia en bootstrap; no se edita HANDOFF. Ramas activas y sus
puntas viven en evidencia. I-64 comparte SelectiveEditorOpenTests: no se escribe esa prueba
durante Discovery. Las intersecciones se revisan antes de toda escritura de producto.
I-25 (guardas traseras) es backlog antecedente; no se reclama, cierra ni modifica.

## 6. Plan inicial y routing

F0 Discovery/compatibilidad/propuesta; F1 largueros; F2 seguridad; F3 regresion;
F4 preparacion verificable de OV, cerrada antes de READY; READY/Candidato y despues
Owner Validation/cierre segun LIFECYCLE. Solo F0 esta autorizado
hasta la primera devolucion al Owner, sin congelar. Consolidar gates si la evidencia lo permite.

Routing I-61 previsto: caracterizacion Balanced; identidad Balanced/Deep si ambigua;
adiciones Routine/Balanced; regresion Balanced; revision independiente segun LIFECYCLE.
No se declaran celdas elegibles ni ejecucion delegada verificada sin medir el entorno.

## 7. Evidencia y Owner Validation

Smoke acotado: listado, renombres, seleccion/reapertura, las dos seguridades y su comportamiento
acordado. Matriz T1..T15 en Proposal V1 §5; oraculos finales pendientes de O-01..06. Evidencia:
[I-69-evidence.md](../automation/evidence/I-69-evidence.md).
Integracion manual; tag esperado integration/I-69. Sin Candidato ni autorizacion de merge.

## 8. Reparto Owner / Claude y relevo inicial

Decision Owner posterior a Proposal V1: [D-09](../automation/decisions/I-69.md).
El Owner completa los datos Excel/CSV, incluidos largueros y etiquetas; Claude
recibe la implementacion de software y pruebas que se acuerden. O-01/O-02 no
bloquean la recepcion del worktree ni la preparacion tecnica, aunque los datos
reales deban integrarse y validarse antes de entregar producto.

Se conserva la misma iniciativa/rama/worktree; una sola sesion opera en el
worktree a la vez, tambien al recibir ediciones del Owner.
[Prompt inicial para Claude](../automation/prompts/I-69-claude-initial.md).
El primer encargo permite relevo, preflight, delta y documentos propios.
Producto sigue NO autorizado; seguridad/Freeze aun requieren decisiones concretas.
