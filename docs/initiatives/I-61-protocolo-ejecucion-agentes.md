---
schema: rackcad-initiative/v2
id: I-61
title: Agent Execution, Model Routing & Prompting Protocol
type: architecture
status: integrated
workflow: V2
conceptual_initiative: I-61
delivery_unit: I-61
archetype: NEW ARCHITECTURE
materiality: [M-01, M-02, M-04, M-05, M-06, M-07, M-08]
branch: architecture/protocolo-ejecucion-agentes
base_branch: main
priority:
size:
depends_on: []
conflicts_with: []
hot_files: [docs/ROADMAP.md, docs/adr/README.md, src/RackCad.Plugin/RackCamaCommands.cs]
coordination_strategy: Ventana de escritura acordada con I-52 por su canal para ROADMAP y, en el commit de cierre (WORKFLOW §11.4), para el índice de ADR; número ADR-0046 notificado a I-52 al publicar el diseño; piloto en gate y commits propios (Discovery §19.2).
context_packs: [documentation-governance, delivery-validation, autocad-plugin, system-dynamic-flowbed, persistence]
consumes: [Rack Identity, Unknown-field Preservation in Persisted Envelopes, Custom Properties]
extends: []
introduces: [Agent Execution Protocol]
discovery_ref: docs/initiatives/I-61-discovery.md
freeze_ref: docs/initiatives/I-61-proposal-v9.md
freeze_delta_ref:
amendment_refs: []
ov_assignment_ref:
decision_refs: [docs/automation/decisions/I-61.md, docs/automation/decisions/I-61-owner-mandate.txt]
evidence_ref: docs/automation/evidence/I-61-evidence.md
automation_state_path: docs/automation/state/I-61.yml
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

# I-61 — Agent Execution, Model Routing & Prompting Protocol

> Contrato **manual** (`automation.enabled: false`): la iniciativa no se selecciona ni se reanuda por el ejecutor nocturno; eso impide la selección y la recurrencia
> automáticas, no el trabajo manual autorizado ([decisiones](../automation/decisions/I-61.md) §9). Desde G1-C, `requires_plugin_build`, `requires_autocad` y
> `requires_owner_validation` valen `true` porque el piloto cambia el comportamiento de dibujo ([AUTOMATION_PLAN](../AUTOMATION_PLAN.md) §11, regla monotónica; Discovery §19.3).

## 1. Identidad y agrupación

- Iniciativa conceptual y unidad de entrega: **I-61** (única). Workflow **V2, T4** ([WORKFLOW](../WORKFLOW.md) §11.3).
- Apertura: caso (d) de [WORKFLOW](../WORKFLOW.md) §2 por **mandato explícito del Owner** ([`I-61-owner-mandate.txt`](../automation/decisions/I-61-owner-mandate.txt), una sola copia).
- **Arquetipo: NEW ARCHITECTURE, confirmado por el Coordinator (Q-01).** La etiqueta FOUNDATION EVOLUTION del mandato se conserva como fuente y no se reescribe.
  Materialidad: M-01, M-02 (creador), M-04, M-05, M-06 y M-07 activados en el Discovery (§9); M-08 activada en el diseño (Proposal V9 §17: amplía la asignación de P-17, OWN-L); M-03 no. El delta del piloto es EXTENSION (Discovery §19.1).
- **Agrupación:** por sí solo, el piloto sería otra unidad (LIFECYCLE §2), pero el **Owner** fijó el alcance: lo incluye en esta iniciativa (mandato «PILOT») y en este recorrido hasta el Candidato,
  sin nuevos reclamos (decisiones §9.1, cláusulas 1-3). El Coordinator lo registra (C61-G1-07). Tiene gate y commits propios.
- Modifica el **proceso de desarrollo** de RackCad; el único cambio de producto es el piloto, intencionalmente pequeño.

## 2. Objetivo

Un protocolo estable para que un Coordinator delegue gates de implementación a un Execution Controller (Codex), que seleccione executor, model, effort, prompt profile y
transport, y delegue en workers Claude/Codex con entregas estructuradas, exact-SHA y condiciones de STOP, **sin mover** la autoridad arquitectónica o de producto fuera de
Coordinator/Architect/Owner (éxito 1..14 del mandato, «SUCCESS CRITERIA»).

## 3. Problema

Los seis problemas que declara el mandato («MOTIVATION»). La caracterización vive en el [Discovery](I-61-discovery.md), no en este contrato.

## 4. Alcance y no-objetivos

**Alcance A–G** (resumen por referencia; la fuente es el mandato, «DELIVERABLE A…G»; los `A/B/C` de «IMPLEMENTATION LEVEL» son **niveles de solución**):

| Entregable | Resumen (ver mandato) |
|---|---|
| A — Prompting standard | Guía breve y propia de RackCad basada en guía oficial vigente; registra URL, proveedor, fecha verificada y modelos/generación; no copia documentación externa |
| B — Model/Effort router | Política estable por **clase de tarea** (11 clases, 7 ejes) → Executor, Model, Effort, PromptProfile; `MODEL_ROUTING` estable separado de `MODEL_CATALOG` mutable y **no normativo**; effort semántico, escalado con `MODEL_ESCALATION_REASON` y enrutamiento hacia abajo |
| C — Prompt composition | Contrato base de delegación + perfil + delta de la tarea; prompts **autolocalizados**, sin mega-plantilla |
| D — Structured delegation | Esquema legible por máquina (JSON) con identidad, SHA base, rama/worktree, executor/model/effort/profile, autoridades, alcance, invariantes, criterios, pruebas, evidencia, STOP, `MaxReworkLoops`, ruta de la entrega |
| E — Structured handoff | Esquema legible por máquina de la **entrega de worker** (nombre elegido para no confundirla con `HANDOFF.md`); `WorkerStatus` `IMPLEMENTATION_COMPLETE`/`PARTIAL`/`BLOCKED`; **no otorga GATE PASS** |
| F — Transport hierarchy | 1 archivos + artefactos; 2 CLI/invocación directa; 3 conector/API/MCP; 4 Computer Use (fallback); 5 relevo manual |
| G — Execution Controller | Responsabilidades 1..12 (Discovery §20.4 las asigna a actores medidos); rework, STOP y propiedad exclusiva de escritura |

Además, del mandato: estado transitorio frente a versionado, métricas del piloto (las no disponibles no bloquean), piloto y nivel de implementación (preferir el mínimo; sin plataforma
grande antes de evidencia del piloto; sin automatización simulada).

**Jerarquía de autoridad (mandato):** el Worker declara `IMPLEMENTATION COMPLETE`; Codex, `EXECUTION VERIFIED` / `REWORK REQUIRED` / `BLOCKED`; **solo el Coordinator declara GATE PASS**;
solo el Workflow vigente declara Candidate / Closure / Integration. Codex **no** es el Master Orchestrator.

**No-objetivos del mandato:** ID20; BOM calculado; configuraciones/plantillas; UX de `RACKPROYECTAR`; Semantic Grips; backlog de producto ajeno; plataforma de orquestación grande antes
del piloto. **No convertir el piloto en un benchmark entre proveedores** (Coordinator, C61-G0-06, decisiones §2) y «No sustituir el piloto por un benchmark, un nuevo D0 o solo documentación»
(Owner, decisiones §9.1, cláusula 3).

**Restricciones de fase (no son no-objetivos del mandato; C61-G1-09):** en G0 y G1 rigieron las prohibiciones de tocar normas globales, `HANDOFF`, índice ADR, `src/`, `tests/`,
`assets/` o CI, y de scripts operativos, instalaciones, cambios de PATH/autenticación, workers nuevos o recurrencia. Desde la orden de continuidad rigen sus propias restricciones
([decisiones](../automation/decisions/I-61.md) §9). Desde la implementación solo se tocan las rutas justificadas por el Freeze.

**Piloto (Discovery §19; C61-G1-06):** corregir la asimetría **D-1a**: `RACKEDITAR` de Cama debe conservar el nombre lógico del sobre ante un nombre editado en blanco, como las otras
cinco familias. D-4 queda fuera.

Freeze, Freeze delta y A-n: **ninguno todavía**.

## 5. Fundaciones y evolución

```text
Consumes:   Rack Identity, Unknown-field Preservation in Persisted Envelopes, Custom Properties (piloto; sin cambiarlas; Discovery §19.1)
Extends:    ninguna fundación
Introduces: Agent Execution Protocol (reutilizable: M-06/M-07 y éxito 14); su entrada factual en FOUNDATIONS se redacta antes de READY-04 (LIFECYCLE §4.1) y se publica `STABLE` en el cierre
Normas que se modifican (no son fundaciones): PROMPT_TEMPLATES (sección nueva), AUTOMATION_PLAN (§16 y §2), WORKFLOW §10 (fila); base: artefactos V2 de ADR-0045 (Discovery §20.3)
```

## 6. Discovery, decisiones y Freeze

- Discovery: [I-61-discovery.md](I-61-discovery.md) (G1 + consolidación G1-C).
- Freeze / Freeze delta / A-n: Proposal V9 ([I-61-proposal-v9.md](I-61-proposal-v9.md)), acordada tras ocho rondas Coordinator ↔ Architect en SAME-SESSION ROLE ([r1](I-61-design-review-r1.md)…[r8](I-61-design-review-r8.md)); identidad y Freeze en [decisiones](../automation/decisions/I-61.md) §13. NEW ARCHITECTURE congela su Proposal autocontenida (LIFECYCLE §6). Sin A-n.
- Decisiones: [I-61.md](../automation/decisions/I-61.md); mandato: [I-61-owner-mandate.txt](../automation/decisions/I-61-owner-mandate.txt).

**Decisiones del Owner necesarias y dónde bloquean** (LC-17):

| Decisión | Bloquea |
|---|---|
| Aceptar o rechazar ADR-0046 (propuesto con el diseño) | `READY-03` y `FINAL_CANDIDATE_SHA`; no bloquea diseño, implementación ni piloto (C61-G1-04) **Decidido 2026-10-01: ACCEPTED** (decisiones §17) |
| Owner Validation del Candidato: comportamiento del piloto y aceptación del resultado frente a los criterios de éxito | la integración (es la validación ordinaria, no una decisión previa pendiente; Discovery §15 Q-04 y Q-06) **Ejecutada 2026-10-01: PASS** (decisiones §19) |
| DEV-G1C-01: conservar o eliminar la entrada `trusted` añadida a `~/.codex/config.toml` | `READY-03`, por aplicación literal (Proposal V9 §16.1; sustituye la anotación anterior «nada del flujo», decisiones §13) **Decidido 2026-10-01: eliminar; eliminada** (decisiones §17) |

Las preguntas sobre la intención de producto del piloto (Q-06, con la inferencia del Coordinator que el Discovery §15 registra), la agrupación (C61-G1-07) y el relevo con agentes externos (C61-G1-01) están **respondidas por cláusulas del Owner**
(mandato; decisiones §9.1).

## 7. Dependencias, archivos calientes y coordinación

- Dependencias integradas: ninguna declarada (la base contiene Workflow V2 efectivo).
- Coordinación con **I-52**: ambas escriben `docs/ROADMAP.md` y `docs/adr/README.md`. Toda escritura de esas superficies requiere una ventana acordada por el canal entre sesiones
  (precedente: decisiones §4). No se toca la rama, el worktree ni las filas de I-52.
- Archivos calientes: `docs/ROADMAP.md` (solo la fila propia), `docs/adr/README.md` (una fila para ADR-0046, en el commit de cierre, WORKFLOW §11.4) y `src/RackCad.Plugin/RackCamaCommands.cs` (piloto; WORKFLOW §7).
  `HANDOFF.md` solo se edita al integrar o cerrar.
- Las puntas observadas viven en la evidencia y en los cuerpos de commit.

## 8. Gates funcionales

Secuencia del ciclo (LIFECYCLE §§4-8): **Discovery → revisión del Coordinator → Proposal → rondas Coordinator ↔ Architect hasta AGREED → Consensus Freeze → gates funcionales →
READY → Candidato**. **El plan de gates, con sus resultados verificables, forma parte de la Proposal que se congela** (LIFECYCLE §6). Tras el Freeze solo se implementa, y cualquier
cambio a lo congelado va por A-n.

- **G0 — admisión documental (reclamo y bootstrap).** GATE PASS del Coordinator sobre `6f1ef981` (decisiones §7). Validación registrada en la evidencia §6: diff solo en `docs/` y CI de
  `push` 4/4 sobre el SHA exacto. No es un gate funcional (LIFECYCLE §7) y **no crea una exención general**: los gates funcionales y el Candidato siguen AGENTS.md «Pruebas — definición de
  terminado» y WORKFLOW §§4.5 y 11.5 sin excepción por clase de cambio.
- **G1 — Discovery**, con su consolidación G1-C: [I-61-discovery.md](I-61-discovery.md). Decisión del gate en el Discovery §20.5.
- **G2 — Protocolo materializado** y **G3 — Piloto real:** definidos por la Proposal congelada (§16.2); GATE PASS del Coordinator (decisiones §§14-15).
- **READY-01..09:** satisfechos en orden sobre el Candidato final (decisiones §§16-18; evidencia §16).

## 9. Owner Validation

- Asignación OV: la fija el Freeze.
- **Requiere AutoCAD y Owner Validation:** sí, porque el piloto cambia el comportamiento de dibujo (AGENTS.md punto 5; Discovery §19.3). Se ejerce sobre `FINAL_CANDIDATE_SHA`.
- **Veredicto (2026-10-01): APROBADA** — OV-I61-01..05 PASS y OV-I61-05 aceptada, declarados por el Owner sobre el Candidato final (decisiones §19; evidencia §16.4).

## 10. Evidencia y entrega

- Evidencia por unidad: [`docs/automation/evidence/I-61-evidence.md`](../automation/evidence/I-61-evidence.md)
- Estado transitorio: [`docs/automation/state/I-61.yml`](../automation/state/I-61.yml)
- Tag esperado: `integration/I-61`

Este contrato no copia SHAs, corridas ni conteos. La integración es manual, serializada y sin auto-merge, conforme a `WORKFLOW.md`.

## 11. Criterios de aceptación

Los 14 criterios de éxito del mandato («SUCCESS CRITERIA»), concretados por el Freeze. Completa ≠ integrada.

## 12. Condiciones para detenerse

Las del mandato («STOP CONDITIONS») más: contradicción material de fuentes o de autoridad (se cita y se devuelve al Coordinator); EXP-01 clase A abierta en los términos de LIFECYCLE
§4; intersección activa no coordinada; cualquier paso que requiera autenticación, instalación, invocación de pago, recurrencia o subdelegación sin autorización expresa.

**Materialidad UNKNOWN:** se rige por LIFECYCLE §§3-4. Una M en UNKNOWN obliga a investigarla (EXP-09) y cuenta como activada hasta resolverse; **no impide el Discovery** que debe
resolverla. (Corregido en G1-C: la versión anterior añadía una regla propia que no está en el lifecycle.)

## 13. Hallazgos fuera de alcance

- Regla `-text`/fin de línea para copias byte-exactas bajo `core.autocrlf=true`.
- 13 contratos V1 (más `TEMPLATE.md`) con `automation.enabled: true` sin ejecutor real; 35 estados obsoletos, 3 con YAML inválido y enums desviados; `.agent/` no ignorado; prosa «Workflow V2
  no efectivo» desactualizada (Discovery §18.3, EXP-01 F-02). No se corrigen en I-61.
- D-4: `EditCama` redibuja solo la definición elegida (Discovery §19).
- Observación pendiente de procedencia sobre la fila de I-57 en la rama de I-52 (decisiones §5).
- En el cierre, los hallazgos de esta sección (salvo el de I-52), los seguimientos del piloto y la autoverificación de la sesión principal quedan en
  [ideas-futuras](../ideas-futuras.md), sección de I-61.
