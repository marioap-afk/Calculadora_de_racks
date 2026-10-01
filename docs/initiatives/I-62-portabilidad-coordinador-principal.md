---
schema: rackcad-initiative/v2
id: I-62
title: Principal Coordinator Portability & Provider-Agnostic Role Binding
type: architecture
status: discovery-f0
workflow: V2
conceptual_initiative: I-62
delivery_unit: I-62
archetype: NEW ARCHITECTURE
materiality: [UNKNOWN]
branch: architecture/portabilidad-coordinador-principal
base_branch: main
priority:
size:
depends_on: []
conflicts_with: []
hot_files: [docs/ROADMAP.md]
coordination_strategy: Bootstrap con acuses de ventana de ROADMAP de I-52, I-63 e I-64 (ID30) (ver decisiones); DC-07 fija el resto en Discovery.
context_packs: [documentation-governance]
consumes: [UNKNOWN]
extends: [UNKNOWN]
introduces: [UNKNOWN]
discovery_ref: docs/initiatives/I-62-discovery.md
freeze_ref:
freeze_delta_ref:
amendment_refs: []
ov_assignment_ref:
decision_refs: [docs/automation/decisions/I-62.md, docs/automation/decisions/I-62-owner-mandate.txt]
evidence_ref: docs/automation/evidence/I-62-evidence.md
automation_state_path: docs/automation/state/I-62.yml
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

# I-62 — Principal Coordinator Portability & Provider-Agnostic Role Binding

> Contrato **manual** (`automation.enabled: false`): el ejecutor nocturno no selecciona ni reanuda esta iniciativa. Los campos
> `requires_*` que no tienen valor están **pendientes** y **no conceden exenciones**. Build, AutoCAD, Owner Validation y decisiones del Owner
> se rigen por las autoridades de cada fase (`AGENTS.md`, [WORKFLOW](../WORKFLOW.md), [AUTOMATION_PLAN](../AUTOMATION_PLAN.md) §11) y por la
> asignación que fije el Freeze.

## 1. Identidad y agrupación

- Iniciativa conceptual y unidad de entrega: **I-62** (única). Workflow **V2, T4**: reclamo posterior a `WORKFLOW_V2_EFFECTIVE_SHA`, sin
  pausa ([WORKFLOW](../WORKFLOW.md) §11.3).
- Apertura: caso (d) de [WORKFLOW](../WORKFLOW.md) §2, por **autorización explícita del Owner**, que además reserva el número I-62 para esta
  iniciativa ([decisiones](../automation/decisions/I-62.md) §2).
- Mandato: [`I-62-owner-mandate.txt`](../automation/decisions/I-62-owner-mandate.txt). Es una sola copia; aquí no se reescribe.
- **Arquetipo: NEW ARCHITECTURE provisional**, aceptado por el Coordinator como clasificación conservadora mientras M-07 siga UNKNOWN
  (C62-F0-01, [decisiones](../automation/decisions/I-62.md) §7; [INITIATIVE_LIFECYCLE](../INITIATIVE_LIFECYCLE.md) §3). Solo gobierna la
  profundidad de Discovery y revisión: no amplía alcance, no obliga a Level B y no autoriza plataforma ni gasto. El mandato conserva su
  etiqueta FOUNDATION EVOLUTION. **Propuesta del Discovery** (pendiente de revisión): FOUNDATION EVOLUTION, con la restricción de diseño de
  [Discovery](I-62-discovery.md) §9.3. La confirma el Coordinator; una bajada antes del Freeze sigue LIFECYCLE §3.
- Evoluciona el **Agent Execution Protocol** de I-61, que sigue integrado, intacto y como protocolo vigente hasta que I-62 se integre. Es un
  cambio de **proceso de desarrollo**; no cambia funcionalidad de producto.

## 2. Objetivo

Quitar del protocolo de I-61 el acoplamiento innecesario entre roles y proveedor o sesión. Una iniciativa debe poder asignar los roles de
ejecución a distintos proveedores, modelos o runtimes según capacidad **verificada**, no por supuestos fijos («Claude = Principal», «Codex =
Reviewer»). El protocolo debe seguir siendo agnóstico de proveedor. La fuente es el mandato, secciones «GOAL» y «SUCCESS CRITERIA» (1..14).

## 3. Problema

Según el mandato, el protocolo integrado asigna implícitamente roles a proveedores y depende de memoria privada para reanudar. Además, no
acredita la configuración de la sesión principal ni el entorno de ejecución antes del trabajo delegado. El inventario inicial de acoplamientos
y huecos es un antecedente fuera del repositorio ([evidencia](../automation/evidence/I-62-evidence.md) §2). La caracterización verificada en
la base es trabajo de Discovery (§6).

## 4. Alcance y no-objetivos

**Alcance**, resumido por referencia (la fuente son las secciones del mandato con el nombre indicado):

| Área del mandato | Resumen |
|---|---|
| PRIMARY ROLES / ROLE BINDING | Roles estables independientes del proveedor (PRINCIPAL_COORDINATOR, ARCHITECT, EXECUTION_CONTROLLER, WORKER, REVIEWER; otros solo si Discovery prueba la necesidad). El binding se resuelve en runtime con hechos observables, fail-closed si una capacidad requerida no se acredita |
| PRINCIPAL SESSION SELF-VERIFICATION / CAPABILITY MODEL | Perfil versionado del Principal con estados MATCH / ABOVE_REQUIRED / BELOW_REQUIRED / UNKNOWN. Reutiliza los niveles y el effort semántico de I-61. Los nombres de modelo no son normativos |
| EXECUTION ENVIRONMENT PREFLIGHT | Contrato de preflight agnóstico (Git, runtimes vinculados, autenticación sin secretos, herramientas, directorios), sin PASS simulado |
| PROVIDER ADAPTER BOUNDARY / PROMPT PORTABILITY / SECURITY | El núcleo conserva roles, contratos, exact-SHA, STOP, presupuestos, custodia y evidencia. Ejecutables, sintaxis, modelos, flags, introspección y renderizado de prompts viven en adapters o catálogos. Ningún secreto entra al repositorio |
| MODEL ROUTING / INDEPENDENCE BY RISK / TRANSPORT | Routing portable entre proveedores. Independencia según riesgo (los nombres finales los fija el Freeze). Se conserva la jerarquía de transporte |
| CONTEXT PORTABILITY / PORTABILITY TEST | Un Principal distinto reanuda solo con estado versionado, verificado con una prueba real A→B (y B→A si es viable) |
| CUSTODY / ORPHAN RECOVERY / RETRY BUDGET | Custodia explícita, recuperación determinista y fail-closed, y presupuestos que sobreviven al cambio de proveedor |
| I-61 BACKLOG / LEVEL B / EVIDENCE FROM PRODUCT INITIATIVES | Clasificar el backlog de I-61 (IN SCOPE / DEFER / OBSOLETE / SEPARATE UNIT). Level B solo si la evidencia lo justifica; nada de plataforma de agentes. Recoger evidencia sin cambiar el protocolo de otras iniciativas a mitad de camino |
| VALIDATION | Topologías A (Claude Principal + rol externo Codex) y B (Codex Principal + rol externo Claude), sin fingir paridad. Lo que no se pueda demostrar se registra UNSUPPORTED / UNVERIFIED |

**Autoridad:** I-62 se implementa con I-61 tal como está integrado. No se usa conducta de I-62 sin aprobar para implementar I-62, y cada
fricción se registra (mandato, «I-61 EXECUTION»). Solo el Coordinator declara GATE PASS. Coordinator + Architect acuerdan el Freeze.

**No-objetivos** (mandato, «OUT OF SCOPE» y «PARALLELISM WITH PRODUCT»):
- cambiar funcionalidad, UI o lógica de producto;
- reemplazar al Master Orchestrator o quitar autoridad al Coordinator;
- bucles autónomos sin límite, compras o suscripciones dinámicas, gestión de credenciales o un scheduler en la nube arbitrario;
- fijar modelos actuales de forma permanente;
- auto-merge fuera de la autoridad de Workflow;
- reescribir la historia de I-61.

Además, **durante G0**: no se tocan normas globales, HANDOFF, FOUNDATIONS, esquemas vigentes, índice ADR, `src/`, `tests/`, `assets/`, CI,
configuración de agentes ni superficies de I-52, I-63 o I-64.

Freeze, Freeze delta y A-n: **ninguno**.

## 5. Fundaciones y evolución

```text
Consumes: UNKNOWN (Discovery DC-08; candidata: Agent Execution Protocol de I-61, solo tras comprobación en la base)
Extends: UNKNOWN (el mandato propone extender el Agent Execution Protocol)
Introduces: UNKNOWN (condicionado a M-07 y EXP-09)
```

Ninguna entrada se afirma sin la comprobación DC-08 de [INITIATIVE_LIFECYCLE](../INITIATIVE_LIFECYCLE.md) §4.1.

**Propuesta del Discovery** (DC-08, [Discovery](I-62-discovery.md) §8; pendiente de revisión del Coordinator; el frontmatter no cambia hasta
entonces):
- Consumes: Workflow V2 e INITIATIVE_LIFECYCLE;
- Extends: Agent Execution Protocol (AUTOMATION_PLAN §16, ADR-0046, Freeze de I-61 §15);
- Introduces: ninguna fundación nueva, condicionado a la restricción de diseño de §9.3.

## 6. Discovery, decisiones y Freeze

- Discovery: [I-62-discovery.md](I-62-discovery.md), tramo Discovery de F0 entregado para revisión del Coordinator (C62-F0-01). **No está
  completo**: EXP-05 y EXP-06 están propuestas y sin autorizar, y falta la revisión del Coordinator.
- Freeze / Freeze delta / A-n: ninguno.
- Decisiones: [I-62.md](../automation/decisions/I-62.md). Mandato: [I-62-owner-mandate.txt](../automation/decisions/I-62-owner-mandate.txt).

**F0 del mandato** (texto de G0, conservado): Discovery delta de I-61 (DC-01..09 sobre la base vigente) con evidencia de uso real. Incluye
EXP-09 (en particular M-07), DC-07 frente a I-52, I-63, I-64 y futuras normas, la acreditación de capacidades por runtime, el triage del
backlog de I-61, la evaluación de Level B, el plan de las dos topologías, el paquete del Architect y la Proposal hacia el Freeze. La
secuencia F1–F7 del mandato («FUNCTIONAL GATES») queda como propuesta hasta el Freeze. F5 no se implementa si el Freeze concluye que el
Level A sigue siendo preferible. C62-F0-01 autorizó solo el tramo Discovery: diseño, Proposal, revisión del Architect y Freeze **no** están
aprobados.

## 7. Dependencias, archivos calientes y coordinación

- Dependencias integradas: I-61 (Agent Execution Protocol) y Workflow V2 efectivo.
- Conflictos: ninguna iniciativa conflictiva declarada. Coordinación **textual** de `docs/ROADMAP.md` con I-52, I-63 e I-64 (ID30): el bootstrap se
  hizo dentro de una ventana acordada por acuse ([decisiones](../automation/decisions/I-62.md) §5).
- Archivos calientes: `docs/ROADMAP.md` (solo la fila de I-62 en Engineering Productivity). `HANDOFF.md` solo se edita al integrar o cerrar.
- Las puntas observadas se registran en la evidencia y en los cuerpos de commit.

## 8. Gates funcionales

G0 (este bootstrap) incluye reclamo, contrato, fila, estado, decisiones, mandato y evidencia. Los gates posteriores se definen en el Freeze
según [INITIATIVE_LIFECYCLE](../INITIATIVE_LIFECYCLE.md) §7; aquí no se prometen.

## 9. Owner Validation

- Asignación OV: pendiente del Freeze.
- Requiere AutoCAD: pendiente. No hay disparador mientras no cambie el comportamiento de dibujo; el mandato excluye cambios de producto
  salvo un fixture mínimo aprobado.
- Requiere Owner Validation: pendiente. La fija el Freeze conforme a la autoridad vigente; no se decide N/A por anticipado.

## 10. Evidencia y entrega

- Evidencia por unidad: [`docs/automation/evidence/I-62-evidence.md`](../automation/evidence/I-62-evidence.md)
- Estado transitorio: [`docs/automation/state/I-62.yml`](../automation/state/I-62.yml)
- Tag esperado: `integration/I-62`

Este contrato no copia SHAs, corridas ni conteos. La integración es manual, serializada y sin auto-merge, conforme a `WORKFLOW.md`.

## 11. Criterios de aceptación

Los 14 criterios del mandato («SUCCESS CRITERIA»), concretados por el Freeze. Completa ≠ integrada.

## 12. Condiciones para detenerse

Las del mandato (fail-closed ante una capacidad requerida no acreditada) más:
- contradicción material de fuentes o de autoridad: se cita y se devuelve al Coordinator;
- EXP-01 clase A abierta, o un disparador M UNKNOWN sin resolver al cerrar Discovery;
- intersección activa no coordinada;
- cualquier paso que requiera autenticación, instalación, invocación de pago, cambio de configuración, recurrencia o delegación sin
  autorización expresa;
- un STOP P-01 pendiente o un cambio de `config.toml` sin disposición acreditada antes de una delegación.

## 13. Hallazgos fuera de alcance

Ninguno registrado en G0.
