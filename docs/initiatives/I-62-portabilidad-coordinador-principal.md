---
schema: rackcad-initiative/v2
id: I-62
title: Principal Coordinator Portability & Provider-Agnostic Role Binding
type: architecture
status: design
workflow: V2
conceptual_initiative: I-62
delivery_unit: I-62
archetype: NEW ARCHITECTURE
materiality: [M-01, M-02, M-04, M-05, M-06, M-07, M-08]
branch: architecture/portabilidad-coordinador-principal
base_branch: main
priority:
size:
depends_on: []
conflicts_with: []
hot_files: [docs/ROADMAP.md]
coordination_strategy: Bootstrap con acuses de ventana de ROADMAP de I-52, I-63 e I-64 (ID30) (ver decisiones); DC-07 fija el resto en Discovery.
context_packs: [documentation-governance]
consumes: []
extends: [Agent Execution Protocol]
introduces: []
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
- **Arquetipo: NEW ARCHITECTURE**, **confirmado por el Coordinator para el diseño** (C62-F0-09 §1, [decisiones](../automation/decisions/I-62.md)
  §10; [INITIATIVE_LIFECYCLE](../INITIATIVE_LIFECYCLE.md) §3):
  - activados M-01 (creador), M-02 (modificación y creador), M-04, M-05, M-06, M-07 y M-08; M-03 no;
  - el Architect puede cuestionarlo; su acuerdo no se afirma;
  - la clasificación no autoriza motor, framework, scheduler, servicio de locks ni plataforma.

  Antes la clasificación era provisional (C62-F0-01, decisiones §7) y se mantuvo así en F0-R1 (R62-F0-01). El mandato conserva la etiqueta
  FOUNDATION EVOLUTION como fuente histórica de la intención.
- **Agrupación:** una iniciativa conceptual y **una** unidad de entrega. Los pilotos F6 validan la misma entrega (C62-F0-09 §2).
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
Consumes: ninguna entrada de FOUNDATIONS (decisión del Coordinator C62-F0-09 §1; Discovery §8)
Extends: Agent Execution Protocol (AUTOMATION_PLAN §16, ADR-0046, Freeze de I-61 §15; C62-F0-09 §1)
Introduces: ninguna entrada propia (propuesta de la Proposal V13, Anexo B.1: los contratos nuevos forman parte de la entrada extendida; pendiente de revisión)
```

Ninguna entrada se afirma sin la comprobación DC-08 de [INITIATIVE_LIFECYCLE](../INITIATIVE_LIFECYCLE.md) §4.1. Las autoridades de proceso
(WORKFLOW, LIFECYCLE) se obedecen, pero no son fundaciones. La entrada de FOUNDATIONS se redacta como borrador factual en la evidencia (F7) y se
**publica en el commit de cierre documental** (WORKFLOW §11.4); no antes ni como `STABLE` anticipado.

## 6. Discovery, decisiones y Freeze

- Discovery: [I-62-discovery.md](I-62-discovery.md), ronda R1, **aceptada por el Coordinator como base suficiente para el diseño**
  (C62-F0-08). Las decisiones sobre sus asuntos abiertos están en Discovery §21 (C62-F0-09/10). No es F0 GATE PASS.
- Proposal vigente para revisión: [I-62-proposal-v13.md](I-62-proposal-v13.md), `Frozen: NO`. Es la corrección final del residuo de A62-V11-01: la
  fidelidad de una premisa sigue sus dependencias normativas (`NormativeDependencyClosure`), y la evidencia de fidelidad describe la representación entregada
  al revisor, con la compactación como evidencia de runtime ([disposición del Owner y del Coordinator](I-62-architect-review-v12-disposition.md)). Conserva
  todos los cierres de V12 y anteriores, incluido A62-V10-03. Paquete del Architect: [I-62-architect-package-v13.md](I-62-architect-package-v13.md).
  Revisión formal final de V13: **CHANGES REQUIRED** ([registro](I-62-architect-review-v13.md)), con el residuo de A62-V11-01 abierto (terminalidad
  incondicional del documento completo) y los cierres anteriores confirmados; pendiente de la disposición del Owner y del Coordinator.
- Versiones anteriores, conservadas sin cambios:
  - [V1](I-62-proposal-v1.md) … [V5](I-62-proposal-v5.md) y sus paquetes: CHANGES REQUIRED del Coordinator (registros
    [v1](I-62-coordinator-review-v1.md) … [v5](I-62-coordinator-review-v5.md));
  - [V6](I-62-proposal-v6.md) y [V7](I-62-proposal-v7.md), con sus paquetes: CHANGES REQUIRED del Architect (registros
    [V6](I-62-architect-review-v6.md) y [V7](I-62-architect-review-v7.md));
  - [V8](I-62-proposal-v8.md) y su [paquete](I-62-architect-package-v8.md): histórica; no se envió al Architect (requisito R62-AUTO);
  - [V9](I-62-proposal-v9.md) y su [paquete](I-62-architect-package-v9.md): revisión del Architect por invocación acotada del Owner, BLOCKED — OWNER
    DECISION ([registro](I-62-architect-review-v9.md)), **no acreditada como revisión formal limpia**; sus hallazgos, aceptados para corrección;
  - [V10](I-62-proposal-v10.md) y su [paquete](I-62-architect-package-v10.md): revisión formal limpia, CHANGES REQUIRED ([registro](I-62-architect-review-v10.md)),
    válida con una excepción por hallazgo ([disposición](I-62-architect-review-v10-disposition.md));
  - [V11](I-62-proposal-v11.md) y su [paquete](I-62-architect-package-v11.md): fidelidad de los insumos y correcciones de A62-V10-01, -03 y -04; revisión
    formal limpia con la fidelidad probada, CHANGES REQUIRED y acreditada ([registro](I-62-architect-review-v11.md),
    [disposición](I-62-architect-review-v11-disposition.md)); su registro de evidencia quedó corregido por la auditoría de lo visible por el revisor, y
    sigue acreditada;
  - [V12](I-62-proposal-v12.md) y su [paquete](I-62-architect-package-v12.md): corrección de A62-V10-03 (residuo) y A62-V11-01; revisión formal limpia,
    CHANGES REQUIRED, con A62-V10-03 CLOSED y el residuo de A62-V11-01 aceptado ([registro](I-62-architect-review-v12.md),
    [disposición](I-62-architect-review-v12-disposition.md)).
- Punto del Owner abierto: **OD-6** (predicado de independencia de LIFECYCLE; Proposal V13 §11.4), con dos alternativas delimitadas y la recomendación del
  Coordinator registrada como tal. Sin decisión del Owner no hay acuerdo ni Freeze.
- Freeze / Freeze delta / A-n: ninguno.
- Decisiones: [I-62.md](../automation/decisions/I-62.md). Mandato: [I-62-owner-mandate.txt](../automation/decisions/I-62-owner-mandate.txt).

**F0 del mandato** (texto de G0, conservado): Discovery delta de I-61 (DC-01..09 sobre la base vigente) con evidencia de uso real. Incluye
EXP-09 (en particular M-07), DC-07 frente a I-52, I-63, I-64 y futuras normas, la acreditación de capacidades por runtime, el triage del
backlog de I-61, la evaluación de Level B, el plan de las dos topologías, el paquete del Architect y la Proposal hacia el Freeze. La
secuencia F1–F7 del mandato («FUNCTIONAL GATES») queda como propuesta hasta el Freeze. F5 no se implementa si el Freeze concluye que el
Level A sigue siendo preferible.

Autorizaciones de F0:
- C62-F0-01: el tramo Discovery;
- C62-F0-11: el diseño y la Proposal V1, **sin implementación**.

La revisión del Architect, el acuerdo y el Freeze **no** están hechos ni aprobados.

## 7. Dependencias, archivos calientes y coordinación

- Dependencias integradas: I-61 (Agent Execution Protocol) y Workflow V2 efectivo.
- Conflictos: ninguna iniciativa conflictiva declarada. Coordinación **textual** de `docs/ROADMAP.md` con I-52, I-63 e I-64 (ID30): el bootstrap se
  hizo dentro de una ventana acordada por acuse ([decisiones](../automation/decisions/I-62.md) §5).
- Archivos calientes: `docs/ROADMAP.md` (solo la fila de I-62 en Engineering Productivity). `HANDOFF.md` solo se edita al integrar o cerrar.
- Las puntas observadas se registran en la evidencia y en los cuerpos de commit.

## 8. Gates funcionales

G0 (este bootstrap) incluye reclamo, contrato, fila, estado, decisiones, mandato y evidencia. Los gates posteriores se definen en el Freeze
según [INITIATIVE_LIFECYCLE](../INITIATIVE_LIFECYCLE.md) §7; aquí no se prometen. Plan **propuesto** (no aprobado): [Proposal V13](I-62-proposal-v13.md) §17.

## 9. Owner Validation

- Asignación OV: pendiente del Freeze. Matriz **propuesta** OV-I62-01..06 (preparación, ensayo y ejecución final sobre FINAL_CANDIDATE_SHA) y decisiones del Owner OD-1..OD-7 con su momento de bloqueo: [Proposal V13](I-62-proposal-v13.md) §18.
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
