---
schema: rackcad-initiative/v2
id: I-61
title: Agent Execution, Model Routing & Prompting Protocol
type: architecture
status: discovery-g1
workflow: V2
conceptual_initiative: I-61
delivery_unit: I-61
archetype: NEW ARCHITECTURE
materiality: [UNKNOWN]
branch: architecture/protocolo-ejecucion-agentes
base_branch: main
priority:
size:
depends_on: []
conflicts_with: []
hot_files: [docs/ROADMAP.md]
coordination_strategy: Bootstrap con acuse de ventana de ROADMAP de I-52 (ver decisiones); DC-07 fija el resto en Discovery.
context_packs: [documentation-governance]
consumes: [UNKNOWN]
extends: [UNKNOWN]
introduces: [UNKNOWN]
discovery_ref: docs/initiatives/I-61-discovery.md
freeze_ref:
freeze_delta_ref:
amendment_refs: []
ov_assignment_ref:
decision_refs: [docs/automation/decisions/I-61.md, docs/automation/decisions/I-61-owner-mandate.txt]
evidence_ref: docs/automation/evidence/I-61-evidence.md
automation_state_path: docs/automation/state/I-61.yml
requires_ci: true
requires_plugin_build: false
requires_autocad: false
requires_owner_decision: true
requires_owner_validation: false
automation:
  enabled: false
  auto_merge: false
  max_attempts: 3
---

# I-61 — Agent Execution, Model Routing & Prompting Protocol

> Contrato **manual** (`automation.enabled: false`): esta iniciativa no se selecciona ni se reanuda por el ejecutor nocturno. Los valores
> `requires_* : false` **solo significan que este contrato no añade un gate por esa metadata**; **no conceden exenciones**. Si el piloto
> (o cualquier cambio posterior) cambia comportamiento de dibujo, `AGENTS.md` y `WORKFLOW.md` §4.5.3 exigen Owner Validation/AutoCAD por la
> naturaleza del cambio, y el build del Plugin se exige donde aplique ([AUTOMATION_PLAN](../AUTOMATION_PLAN.md) §11, regla monotónica).

## 1. Identidad y agrupación

- Iniciativa conceptual y unidad de entrega: **I-61** (única). Workflow **V2, T4** (reclamo posterior a `WORKFLOW_V2_EFFECTIVE_SHA`, sin pausa;
  [WORKFLOW](../WORKFLOW.md) §11.3).
- Apertura: caso (d) de [WORKFLOW](../WORKFLOW.md) §2 por **mandato explícito del Owner**
  ([`I-61-owner-mandate.txt`](../automation/decisions/I-61-owner-mandate.txt), una sola copia; no se reescribe aquí).
- **Arquetipo: NEW ARCHITECTURE, provisional.** El mandato etiqueta FOUNDATION EVOLUTION; el Coordinator fijó la clasificación conservadora
  provisional mientras **M-07** siga UNKNOWN (C61-D0-04; [INITIATIVE_LIFECYCLE](../INITIATIVE_LIFECYCLE.md) §3: ante duda, el arquetipo superior).
  Materialidad final, `Consumes/Extends/Introduces` y agrupación las decide el Coordinator sobre el [Discovery G1](I-61-discovery.md); hasta entonces el frontmatter
  conserva `UNKNOWN`. **Propuesta del Executor (no decisión):** I-61 **crea y modifica** (EXP-09 resuelta); M-01, M-02, M-04, M-06 y M-07 activados, M-03 UNKNOWN solo
  para el piloto, M-05 a reevaluar y M-08 no activado; arquetipo **NEW ARCHITECTURE**; el proceso no consume ni extiende ninguna entrada de FOUNDATIONS (DC-08).
- Modifica el **proceso de desarrollo** de RackCad; **no** la funcionalidad de producto salvo el piloto, que debe seguir siendo intencionalmente pequeño.

## 2. Objetivo

Un protocolo estable para que un Coordinator delegue gates de implementación a un Execution Controller (Codex), que seleccione executor, model,
effort, prompt profile y transport, y delegue en workers Claude/Codex con handoffs estructurados, exact-SHA y condiciones de STOP, **sin mover**
la autoridad arquitectónica o de producto fuera de Coordinator/Architect/Owner (éxito 1..14 en el mandato, «SUCCESS CRITERIA»).

## 3. Problema

Seis problemas declarados por el Owner en el mandato («MOTIVATION»): modelos potentes para trabajo rutinario; selección informal de modelo y effort;
traspaso manual de prompts; relevo humano repetido; Computer Use frágil como bus; handoffs no legibles por máquina ni atribuibles por SHA exacto.
La caracterización en el repositorio es trabajo de Discovery (§6), no de este contrato.

## 4. Alcance y no-objetivos

**Alcance A–G** (resumen por referencia; la fuente es el mandato, secciones «DELIVERABLE A…G»; los nombres `A/B/C` de «IMPLEMENTATION LEVEL»
son **niveles de solución**, otra cosa):

| Entregable | Resumen (ver mandato) |
|---|---|
| A — Prompting standard | Guía breve y propia de RackCad basada en guía oficial vigente de los proveedores; registra URL, proveedor, fecha verificada y modelos/generación; no copia documentación externa; las reglas normativas viven en el repositorio |
| B — Model/Effort router | Política estable por **clase de tarea** (11 clases mínimas, 7 ejes de evaluación) que selecciona Executor, Model, Effort y PromptProfile; `MODEL_ROUTING` (principios estables) separado de `MODEL_CATALOG` (modelos vigentes, **no normativo**, actualizable sin tocar Workflow); política de effort semántica, escalado con `MODEL_ESCALATION_REASON` y *down-routing* |
| C — Prompt composition | Base Delegation Contract + Model Prompt Profile + Task Delta; prompts **autolocalizados**, sin mega-plantilla ni repetir WORKFLOW/lifecycle/contrato/Freeze |
| D — Structured delegation | Esquema legible por máquina (JSON preferido) con identidad, SHA base, rama/worktree esperados, executor/model/effort/profile, autoridades, alcance de escritura permitido/prohibido, invariantes, criterios, pruebas, evidencia, STOP, `MaxReworkLoops`, ruta de handoff |
| E — Structured handoff | Esquema legible por máquina con SHAs, rama/worktree, archivos cambiados, pruebas y resultados, evidencia, hallazgos, desviaciones, `WorkerStatus` (`IMPLEMENTATION_COMPLETE`/`PARTIAL`/`BLOCKED`); **no otorga GATE PASS** |
| F — Transport hierarchy | Orden: 1 filesystem + artefactos estructurados; 2 CLI/invocación directa; 3 connector/API/MCP estable; 4 Computer Use (fallback); 5 relevo manual; el protocolo no depende del transporte |
| G — Execution Controller | Responsabilidades 1..12 de Codex (recibir contrato, clasificar, rutar, delegar, verificar de forma independiente, clasificar `VERIFIED`/`REWORK REQUIRED`/`BLOCKED`, devolver al Coordinator); política de rework (`MAX_REWORK_LOOPS = 3`, «misma clase de fallo»), STOP y propiedad exclusiva de escritura |

Además del A–G, el mandato fija: **estado transitorio vs versionado** (auditar convenciones del repositorio antes de adoptar cualquier ruta;
la evidencia persistente sigue en las ubicaciones V2), **métricas del piloto** (las no disponibles no bloquean), **piloto** y **nivel de
implementación** (Discovery decide A/B/C; preferir lo mínimo; sin plataforma grande antes de evidencia del piloto; sin automatización simulada).

**Jerarquía de autoridad (mandato):** el Worker puede declarar `IMPLEMENTATION COMPLETE`; Codex puede declarar `EXECUTION VERIFIED` /
`EXECUTION REWORK REQUIRED` / `EXECUTION BLOCKED`; **solo el Coordinator declara GATE PASS**; solo el Workflow vigente declara Candidate / Closure /
Integration. Codex **no** es el Master Orchestrator. Este contrato no crea política normativa nueva: **referencia sobre repetición**.

**No-objetivos / exclusiones** (mandato «NO PRODUCT SCOPE CREEP» y órdenes del Coordinator): ID20; BOM calculado; configuraciones/plantillas;
UX de `RACKPROYECTAR`; Semantic Grips; backlog de producto ajeno; plataforma de orquestación grande antes del piloto; cambios en normas globales,
`HANDOFF`, índice ADR, `src/`, `tests/`, `assets/` o CI **durante G0**; scripts operativos, instalaciones, cambios de PATH/autenticación, workers nuevos
o recurrencia; benchmark entre proveedores.

**Piloto (pendiente de caracterización):** candidato preferido **Cama / `RACKEDITAR` → preservar el `Name` lógico en editar/actualizar/guardar/
reabrir** (hallazgo residual reportado en `I-60-evidence.md` §7). **No** se corrige en G0. Si el estado real del repositorio lo muestra ya
corregido o inadecuado, Discovery elige otro Extension pequeño y lo justifica. Una prueba de transporte de solo lectura no sustituye al piloto.

Freeze, Freeze delta y A-n: **ninguno** (no existe Freeze; los esquemas y el «catálogo» históricos son borradores/transcripción no reverificada,
no autoridades integradas).

## 5. Fundaciones y evolución

```text
Consumes: UNKNOWN en el frontmatter; Discovery G1 (DC-08) propone «ninguna entrada de FOUNDATIONS» para el proceso; pendiente de confirmación
Extends: UNKNOWN; Discovery G1 propone «ninguna»
Introduces: UNKNOWN; Discovery G1 propone mecanismos nuevos (delegación, handoff, enrutamiento, catálogo, perfiles), condicionado a Q-01 (M-07)
```

Ninguna entrada se afirma sin la comprobación DC-08 de [INITIATIVE_LIFECYCLE](../INITIATIVE_LIFECYCLE.md) §4.1. El piloto, si consume Rack Identity / View Identity /
Unknown-field Preservation / Custom Properties, lo verifica en su propio Discovery delta.

## 6. Discovery, decisiones y Freeze

- Discovery: [I-61-discovery.md](I-61-discovery.md) (G1; DC-01..09 y EXP evaluadas; **no** es Proposal ni Freeze).
- Freeze / Freeze delta / A-n: ninguno.
- Decisiones: [I-61.md](../automation/decisions/I-61.md); mandato: [I-61-owner-mandate.txt](../automation/decisions/I-61-owner-mandate.txt).

**Plan de Discovery** ([INITIATIVE_LIFECYCLE](../INITIATIVE_LIFECYCLE.md) §4; ejecutado en G1 en [I-61-discovery.md](I-61-discovery.md), a revisar por el Coordinator): DC-01..09 sobre la base vigente, con:

- **DC-07:** archivos calientes e intersecciones activas (ROADMAP, HANDOFF, índice ADR frente a I-52 y futuras normas).
- **DC-09 / EXP-09:** M-01..M-08 (en particular **M-07**) y la distinción crear/modificar; decide arquetipo y agrupación.
- **EXP-01:** separar, cláusula por cláusula, lo condicional, lo histórico y lo materialmente contradictorio (p. ej. prosa que sigue diciendo «V2 no
  efectivo» frente a hechos de Git); no se pre-clasifica como clase B (C61-D0-05).
- **EXP-02:** solo si se demuestra una ambigüedad real de dueño de dominio para «ejecución de agentes / enrutamiento de modelos»; no se infiere
  por faltar un rótulo en `WORKFLOW` §10 (C61-D0-03).
- Auditoría de convenciones para el **estado transitorio** (ubicación ignorada por Git, sin adoptar `.agent/` por analogía) y de los puntos de
  contacto con `AUTOMATION_PLAN` (§§8–9: cambiar de modelo **no reinicia** intentos), `PROMPT_TEMPLATES` y `AGENTS.md`.
- Caracterización del piloto (Cama/`RACKEDITAR`/`Name`) y de los transportes realmente disponibles/probados (instalación ≠ autenticación ≠
  invocación probada), y verificación **en fuentes oficiales vigentes** de modelos y controles de effort para el catálogo (Deliverable A/B).
- Decisión A/B/C del nivel de solución con la opción mínima que demuestre el protocolo.
- Revisión obligatoria del **Architect** (mandato): autoridad invertida, Codex como Master, worker auto-aprobado, regreso de la duplicación de
  prompts, sobre-complejidad del router, catálogo obsoleto, Computer Use, bucles sin límite, pérdida de exact-SHA, carreras, contaminación de Git
  por artefactos transitorios y métricas convertidas en burocracia.

## 7. Dependencias, archivos calientes y coordinación

- Dependencias integradas: ninguna declarada (la base contiene Workflow V2 efectivo).
- Conflictos: sin iniciativa conflictiva declarada. Coordinación **textual** con **I-52**: ambas escriben `docs/ROADMAP.md`; el bootstrap se
  hizo dentro de una ventana acordada por acuse (decisiones §4). Ninguna edición de la rama, worktree ni filas de I-52.
- Archivos calientes: `docs/ROADMAP.md` (solo la fila propia de I-61 en Engineering Productivity). `HANDOFF.md` se edita únicamente al integrar/cerrar.
- Las puntas observadas viven en la evidencia y en los cuerpos de commit.

## 8. Gates funcionales

Secuencia del ciclo ([INITIATIVE_LIFECYCLE](../INITIATIVE_LIFECYCLE.md) §§4-7): **Discovery → revisión del Coordinator → diseño/Proposal → revisión del Architect → Consensus Freeze → gates funcionales de
implementación y piloto → READY → Candidato**. Discovery y revisión **preceden** al Freeze; los gates funcionales se definen tras él y no se prometen aquí.

- **G0 — admisión documental (reclamo y bootstrap):** reclamo, contrato, fila, estado, decisiones y evidencia. Decisión del Coordinator: GATE PASS sobre el SHA revisado (ver decisiones §7).
  *Validación registrada* en la [evidencia](../automation/evidence/I-61-evidence.md) §6: comprobación mecánica de que el diff solo toca `docs/` y CI de `push` 4/4 `success` sobre el SHA exacto
  (el CI ejecuta ambas suites y builds sobre ese SHA). G0 **no** es un gate funcional ([INITIATIVE_LIFECYCLE](../INITIATIVE_LIFECYCLE.md) §7: no entrega un resultado conductual de producto) y **no crea una
  exención general** por ser documental: los gates funcionales posteriores y el Candidato siguen [AGENTS.md](../../AGENTS.md) «Pruebas — definición de terminado» y [WORKFLOW](../WORKFLOW.md) §§4.5 y 11.5 sin excepción por clase de cambio.
- **G1 — Discovery (este trabajo):** [I-61-discovery.md](I-61-discovery.md). No implementa; devuelve al Coordinator para decidir el paso de diseño. No declara G1 GATE PASS.

## 9. Owner Validation

- Asignación OV: pendiente del Freeze.
- Requiere AutoCAD: no por metadata; **sí** si el piloto cambia comportamiento de dibujo (disparador vigente: AGENTS.md punto 5 y WORKFLOW §4.5.3).
- Requiere Owner Validation: igual criterio; `false` no la cancela ([AUTOMATION_PLAN](../AUTOMATION_PLAN.md) §11).

## 10. Evidencia y entrega

- Evidencia por unidad: [`docs/automation/evidence/I-61-evidence.md`](../automation/evidence/I-61-evidence.md)
- Estado transitorio: [`docs/automation/state/I-61.yml`](../automation/state/I-61.yml)
- Tag esperado: `integration/I-61`

Este contrato no copia SHAs, corridas ni conteos. La integración es manual, serializada y sin auto-merge, conforme a `WORKFLOW.md`.

## 11. Criterios de aceptación

Los 14 criterios de éxito del mandato («SUCCESS CRITERIA»), concretados por el Freeze. Completa ≠ integrada.

## 12. Condiciones para detenerse

Las del mandato («STOP CONDITIONS») más: contradicción material de fuentes o de autoridad (se cita y se devuelve al Coordinator); EXP-01 clase A
abierta **antes del Freeze o de cualquier gate de implementación**; intersección activa no coordinada; cualquier paso que requiera autenticación, instalación,
invocación de pago, recurrencia o subdelegación sin autorización expresa.

**Materialidad UNKNOWN (precisión de G1):** una M en UNKNOWN **obliga a investigarla** (EXP-09) y cuenta como activada hasta resolverse, pero **no impide el Discovery** que debe
resolverla. Sí impide el Freeze, la Proposal congelable y cualquier implementación mientras siga sin resolver; la UNKNOWN de M-07/Q-01 y de M-03 (piloto) se eleva al Coordinator.

## 13. Hallazgos fuera de alcance

- Regla `-text`/fin de línea para copias byte-exactas de fuentes del Owner bajo `core.autocrlf=true` (tema de configuración; no se toca en G0).
- 14 contratos V1 con `automation.enabled: true` sin ejecutor real, `.agent/` no ignorado por Git y prosa «Workflow V2 no efectivo» desactualizada: deuda preexistente registrada en el
  [Discovery](I-61-discovery.md) (EXP-08/EXP-01); no se corrige en G1.
- Observación pendiente de procedencia sobre la fila de I-57 en la rama de I-52 (decisiones §5).
