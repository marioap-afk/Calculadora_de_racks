---
schema: rackcad-initiative/v2
id: I-63
title: Computed Parameters & Project Summary Foundation
type: architecture
status: bootstrap-g0
workflow: V2
conceptual_initiative: I-63
delivery_unit: I-63
archetype: NEW ARCHITECTURE
materiality: [UNKNOWN]
branch: architecture/parametros-calculados-resumen-proyecto
base_branch: main
priority: HIGH
size:
depends_on: [I-49]
conflicts_with: [I-64]
hot_files: [docs/ROADMAP.md]
coordination_strategy: Bootstrap con acuse de ventana de ROADMAP de los escritores pertinentes (ver decisiones); frontera con ID30 y resto de intersecciones en DC-07.
context_packs: [documentation-governance]
consumes: [UNKNOWN]
extends: [UNKNOWN]
introduces: [UNKNOWN]
discovery_ref:
freeze_ref:
freeze_delta_ref:
amendment_refs: []
ov_assignment_ref:
decision_refs: [docs/automation/decisions/I-63.md, docs/automation/decisions/I-63-owner-mandate.original.txt]
evidence_ref: docs/automation/evidence/I-63-evidence.md
automation_state_path: docs/automation/state/I-63.yml
requires_ci: true
requires_plugin_build:
requires_autocad:
requires_owner_decision:
requires_owner_validation:
automation:
  enabled: false
  auto_merge: false
  max_attempts: 3
---

# I-63 — Computed Parameters & Project Summary Foundation (ID20)

> Contrato **manual** (`automation.enabled: false`): el ejecutor nocturno no selecciona ni reanuda esta iniciativa. Los campos
> `requires_plugin_build`, `requires_autocad`, `requires_owner_decision` y `requires_owner_validation` quedan **vacíos a propósito**:
> están **por determinar** según el workflow, no son `false`. Que estén vacíos no concede exenciones: si un gate cambia comportamiento
> visible o de dibujo, `AGENTS.md` y la guía manual activan Owner Validation/AutoCAD por la naturaleza del cambio
> ([AUTOMATION_PLAN](../AUTOMATION_PLAN.md) §11, regla monotónica).

## 1. Identidad y agrupación

- **ID funcional ID20** («Resumen paramétrico de racks y proyecto»); número técnico **I-63**. ID20 **no** es el Initiative-Id I-20.
- Iniciativa conceptual y unidad de entrega: **I-63** (única, por ahora). La agrupación o partición en unidades la decide Discovery
  ([INITIATIVE_LIFECYCLE](../INITIATIVE_LIFECYCLE.md) §2).
- Apertura: caso (d) de [WORKFLOW](../WORKFLOW.md) §2, por **mandato explícito del Owner**
  ([`I-63-owner-mandate.original.txt`](../automation/decisions/I-63-owner-mandate.original.txt), copia única y byte-idéntica; no se
  reescribe aquí). En el mandato, el número figura como `<NEXT_FREE_INITIATIVE_ID>`; la resolución a I-63 consta en las
  [decisiones](../automation/decisions/I-63.md).
- Workflow **V2**. La clasificación de transición se determina por el reclamo ([WORKFLOW](../WORKFLOW.md) §11.3) y se registra en la evidencia.
- **Arquetipo: NEW ARCHITECTURE, provisional.** El mandato dice FOUNDATION EVOLUTION. El Coordinator elevó la clasificación como
  medida preventiva mientras **M-07** siga UNKNOWN ([INITIATIVE_LIFECYCLE](../INITIATIVE_LIFECYCLE.md) §3: ante duda, el arquetipo
  superior). La elevación no amplía el producto ni autoriza un framework universal, un segundo engine, un inventario runtime ni UI.
  EXP-09 la resuelve en Discovery; un descenso posterior sigue el procedimiento normal.

## 2. Objetivo

Una capa común de **parámetros calculados de solo lectura** (`Rack.*`, `Project.*`) y un **modelo neutral de resumen de proyecto**, de
modo que UI, BOM, reportes y futuros consumidores obtengan cada métrica de **una sola autoridad**, sin recalcularla por su cuenta
(mandato, «GOAL» y «SUCCESS CRITERIA» 1-10). Nombres concretos y métricas finales salen de Discovery/Freeze.

## 3. Problema

El problema y la intención de producto los declara el Owner en el mandato («PRODUCT INTENT», «KEY CONCEPTS», «METRIC AUTHORITY»).
La caracterización en el repositorio es trabajo de Discovery (§6); este contrato no la adelanta.

## 4. Alcance y no-objetivos

**Alcance** (resumen por referencia; la fuente es el mandato):

- separar ComputedParameter (solo lectura, derivado por RackCad) de ProjectVariable (editable) y de CustomProperty (metadata);
- cada métrica con una fuente única y explícita;
- conteo de racks lógicos por RackId, nunca por referencias o vistas; legacy sin identidad tratado según la fundación vigente;
- matriz por sistema con estados Supported / NotApplicable / Unavailable / NeedsContract, sin equivalencias falsas;
- símbolos built-in integrados en el motor de I-49 por su punto oficial de extensión, sin segundo engine y sin recursión oculta;
- modelo neutral de ProjectSummary consumible después por ID30, reportes, RACKLISTA, BOM calculado y diagnósticos;
- vía estable para ID23 y provenance suficiente para ID28/ID29, sin implementar esos productos.

**No-objetivos** (mandato, «OUT OF SCOPE» y órdenes del Coordinator): UI de Workspace; `ProjectSummaryWindow`; BOM calculado (ID23);
referencias de racks (ID21); configuraciones y plantillas (ID25/26); UI de ID28/29; Engineering Rules; fórmulas de Custom Properties;
editores nuevos; UI de paleta de AutoCAD; cambios geométricos; reglas de producto nuevas; un segundo expression engine; un inventario
de racks competidor con el de ID30.

Freeze, Freeze delta y A-n: **ninguno** todavía.

## 5. Fundaciones y evolución

```text
Consumes: UNKNOWN — candidatos preliminares: Rack Identity, View Identity, Authored vs Effective, Project Variables,
          Auto Rack Naming, Shared View Foundation
Extends: UNKNOWN — candidato preliminar: punto oficial de símbolos/contextos del Expression Engine (I-49)
Introduces: UNKNOWN — candidatos preliminares: semántica y provider de ComputedParameter; agregación y ProjectSummary neutral
```

Son candidatos de intake, no afirmaciones: cada entrada se verifica con DC-08 en la base vigente
([INITIATIVE_LIFECYCLE](../INITIATIVE_LIFECYCLE.md) §4.1), contra fuente, código y pruebas. Que una fundación no tenga entrada en
[FOUNDATIONS](../FOUNDATIONS.md) no prueba que no exista. Para el Expression Engine, la base aceptada es la que registra
[ADR-0043](../adr/0043-motor-expresiones-parametricas-causas-multiples-y-recuperacion-segura.md) («Base exacta y frontera de decisión»).

## 6. Discovery, decisiones y Freeze

- Discovery: pendiente; **no iniciado** en G0. La preparación de F0 está entregada al Coordinator y no aprobada; la resume el plan
  de abajo.
- Freeze / Freeze delta / A-n: ninguno.
- Decisiones: [I-63.md](../automation/decisions/I-63.md); mandato: [I-63-owner-mandate.original.txt](../automation/decisions/I-63-owner-mandate.original.txt).

Discovery debe responder las catorce preguntas del mandato («DISCOVERY REQUIRED») sobre DC-01..09, con EXP-01..09 evaluadas y sus
negativos razonados. **EXP-09 es expansión requerida**: ¿se crea una autoridad o mecanismo transversal (M-07) o solo se extienden
puntos integrados (M-01..06/M-08)? La Proposal/Freeze cubre las obligaciones de «PROPOSAL / FREEZE MUST DEFINE» del mandato y además
delimita CustomProperty, ProjectVariable y ComputedParameter con referencia a
[ADR-0039](../adr/0039-custom-properties-persistencia-autoridad.md) D-16.

**Plan de F0** (propuesto y no aprobado; [INITIATIVE_LIFECYCLE](../INITIATIVE_LIFECYCLE.md) §4):

| Tarea | Perfil o actor | Salida |
|---|---|---|
| Inventario de autoridades, símbolos, fases y fuentes de conteo (DC-01..06; preguntas 1-12 y 14 del mandato) | DOCUMENTATION | `docs/initiatives/I-63-discovery.md` |
| Matriz sistema × métrica candidata, con evidencia por celda y sin pruebas nuevas | DOCUMENTATION | mismo archivo |
| Hotspots e intersecciones (DC-07); fundaciones verificadas contra fuente, código y pruebas (DC-08), incluida la identidad del Freeze final de I-49 sobre A3-R2 | sesión responsable | Discovery y evidencia |
| DC-09 y EXP-01..09 con negativos razonados; EXP-09 requerida | sesión responsable; revisión del Coordinator | Discovery |
| Pruebas de caracterización nuevas, solo si una pregunta las exige | CHARACTERIZATION, con contrato aprobado aparte | — |
| Revisión del Architect sobre la Proposal | ARCHITECTURE_REVIEW; modo de revisión declarado | revisión versionada |

La delegación de estas tareas bajo [AUTOMATION_PLAN](../AUTOMATION_PLAN.md) §16 depende del conflicto C-F0-RED, pendiente de
[decisión](../automation/decisions/I-63.md) del Coordinator.

## 7. Dependencias, archivos calientes y coordinación

- Dependencia integrada: **I-49** (Expression Engine), cuyo ADR reserva `Rack.*`, `Project.*` y el ámbito `Rack` para ID20. Las demás
  fundaciones candidatas (§5) están integradas y se verifican en DC-08.
- **I-64 — ID30, Persistent RackCad Workspace**, está activa en paralelo. Frontera del mandato, que el contrato de I-64 recoge igual:
  ID20 posee la semántica de métricas, los providers y la semántica de agregación; ID30 posee el inventario runtime de navegación y la
  representación de UI/sesión. No se consume código no integrado de I-64 ni se copia su diseño. Si aparece una fundación común: STOP y
  Master Orchestrator.

**Mapa preliminar de DC-07** (las puntas observadas viven en la [evidencia](../automation/evidence/I-63-evidence.md)):

| Iniciativa activa | Rama | Cruce textual conocido | Cruce funcional |
|---|---|---|---|
| I-64 (ID30) | `architecture/workspace-persistente-rackcad` | Su fila de ROADMAP también va tras I-60: conflicto textual de filas adyacentes para quien integre segundo | **No inspeccionado.** Riesgo declarado por el mandato: inventario y enumeración de racks lógicos |
| I-62 (portabilidad del Principal) | `architecture/portabilidad-coordinador-principal` | Fila en otra tabla de ROADMAP (Engineering Productivity, tras I-61) | **No inspeccionado.** Por su objeto (proceso y protocolo), no se espera cruce de producto; sin verificar |
| I-52 (RACKMIRROR) | `feature/rackmirror-espejo-semantico` | Edita ROADMAP (su fila y la de I-57), HANDOFF y el índice de ADR; su rebase futuro sobre main tocará ROADMAP | **No inspeccionado** |

- La escritura de `docs/ROADMAP.md` se coordina por acuse (decisiones y evidencia). DC-07 mide el resto en F0.
- Archivos calientes: `docs/ROADMAP.md` (solo la fila propia, en los momentos de [WORKFLOW](../WORKFLOW.md) §2). `HANDOFF.md` y el índice
  de ADR se editan únicamente al integrar/cerrar.

## 8. Gates funcionales

G0: reclamo y este bootstrap. El plan provisional del mandato (F0 Discovery y Freeze; F1..F6; READY) **no está congelado** y se
revisa contra [INITIATIVE_LIFECYCLE](../INITIATIVE_LIFECYCLE.md) §7 antes del Freeze: ningún gate solo de capa, DTO o modelo sin
resultado observable verificable. La conformidad final se referencia desde READY-06.

## 9. Owner Validation

- Asignación OV: pendiente del Freeze.
- Requiere AutoCAD / Owner Validation: **por determinar** según el workflow. El mandato prevé que pueda no aplicar si no se añade UI ni
  comportamiento visible de AutoCAD, y que se planifique si se añade un comando o un resumen visible. No se decide por anticipado ni se
  añade UI para forzarla.

## 10. Evidencia y entrega

- Evidencia por unidad: [`docs/automation/evidence/I-63-evidence.md`](../automation/evidence/I-63-evidence.md)
- Estado transitorio: [`docs/automation/state/I-63.yml`](../automation/state/I-63.yml)
- Tag esperado: `integration/I-63`

Este contrato no copia SHAs, corridas ni conteos. La integración es manual, serializada y sin auto-merge, conforme a `WORKFLOW.md`.
La ejecución delegada, si se usa, sigue [AUTOMATION_PLAN](../AUTOMATION_PLAN.md) §16 bajo contratos de gate aprobados.

## 11. Criterios de aceptación

Los diez criterios de éxito del mandato («SUCCESS CRITERIA»), concretados por el Freeze. Completa ≠ integrada.

## 12. Condiciones para detenerse

Las del mandato más: contradicción material de fuentes o de autoridad (se citan ambas y se devuelve al Coordinator); EXP-01 clase A
abierta; M UNKNOWN sin resolver antes del Freeze; intersección activa no coordinada; fundación común con ID30 (Master Orchestrator);
consumo de código no integrado; cualquier paso que exija autenticación, instalación, cambios de configuración o participantes no
autorizados por un contrato aprobado.

## 13. Hallazgos fuera de alcance

- Varios documentos normativos conservan encabezados que dicen «Workflow V2 no efectivo», aunque el registro durable de activación
  existe; se observan sin corregir (no-touch de G0).
