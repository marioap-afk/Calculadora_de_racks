---
schema: rackcad-initiative/v2
id: I-64
title: Persistent RackCad Workspace
type: architecture
status: f0-proposal-v3
workflow: V2
conceptual_initiative: I-64
delivery_unit: I-64
archetype: NEW ARCHITECTURE
materiality: [M-01, M-04, M-05, M-06, M-07, M-08]
branch: architecture/workspace-persistente-rackcad
base_branch: main
priority: HIGH
size:
depends_on: []
conflicts_with: []
hot_files: [docs/ROADMAP.md]
coordination_strategy: Bootstrap con acuse de ventana de ROADMAP de los escritores pertinentes (ver evidencia); intersecciones con I-52 e I-63 y archivos calientes de producto en DC-07.
context_packs: [ui-editors, autocad-plugin, architecture-kernel]
consumes: [UNKNOWN]
extends: [UNKNOWN]
introduces: [UNKNOWN]
discovery_ref: docs/initiatives/I-64-discovery.md
freeze_ref:
freeze_delta_ref:
amendment_refs: []
ov_assignment_ref:
decision_refs: [docs/initiatives/I-64-owner-brief.txt, docs/initiatives/I-64-coordinator-confirmation-d0-r3.txt]
evidence_ref: docs/automation/evidence/I-64-evidence.md
automation_state_path:
requires_ci: true
requires_plugin_build: true
requires_autocad: true
requires_owner_decision:
requires_owner_validation: true
automation:
  enabled: false
  auto_merge: false
  max_attempts: 3
---

# I-64 — Persistent RackCad Workspace (ID30)

> Contrato **manual** (`automation.enabled: false`): el ejecutor nocturno no selecciona ni reanuda esta iniciativa.
> `requires_plugin_build`, `requires_autocad` y `requires_owner_validation` salen del brief («OWNER SMOKE EARLY», «OWNER
> VALIDATION»): la workspace vive dentro de AutoCAD y el Owner la valida con el DLL del worktree. `requires_owner_decision` queda
> **vacío a propósito** (por determinar). Ningún campo vacío concede exenciones ([AUTOMATION_PLAN](../AUTOMATION_PLAN.md) §11).
> `automation_state_path` queda vacío hasta que se cree el estado antes de la primera delegación (AUTOMATION_PLAN 16.8).

## 1. Identidad y agrupación

- **ID funcional ID30** («Modeless Contextual Editor»), que **absorbe funcionalmente ID3** (sesión y edición multi-rack); número
  técnico **I-64**. ID30 no es el Initiative-Id I-30, ni ID3 el I-03.
- Iniciativa conceptual y unidad de entrega: **I-64** (única, por ahora). La partición en unidades la decide Discovery
  ([INITIATIVE_LIFECYCLE](../INITIATIVE_LIFECYCLE.md) §2).
- Apertura: caso (d) de [WORKFLOW](../WORKFLOW.md) §2, por **brief explícito del Owner**
  ([`I-64-owner-brief.txt`](I-64-owner-brief.txt), copia única y byte-idéntica; no se reescribe aquí). En el brief el número
  figura como `<NEXT_FREE_INITIATIVE_ID>`; la asignación a I-64 consta en la confirmación directa del Coordinator
  ([`I-64-coordinator-confirmation-d0-r3.txt`](I-64-coordinator-confirmation-d0-r3.txt), CD-ID30-NUM-01).
- Workflow **V2**; la clasificación de transición se determina por el reclamo ([WORKFLOW](../WORKFLOW.md) §11.3) y se registra en
  la evidencia.
- **Arquetipo: NEW ARCHITECTURE**, fijado por el brief. Materialidad confirmada por el Coordinator: M-04..M-07 y M-08, este último de
  forma conservadora (CD-I64-D1-01); y M-01, que el Coordinator exigió activar (C64-D1-04) y quedó así en el Discovery D1-R1 aceptado. Para
  la Proposal V2 el Coordinator aceptó **M-02 = NOT ACTIVATED** y **M-03 = NOT ACTIVATED**, con las condiciones de su §14 (sin estampado,
  `Compose` por vista e importación sin efecto persistido; comandos clásicos observacionalmente invariantes). Si una condición se rompe, el
  disparador se reabre.

## 2. Objetivo

Que RackCad pase de ventanas modales o temporales a una **workspace persistente y modeless** dentro de AutoCAD, con sincronización
bidireccional de contexto y selección, sesiones de borrador por rack y **Actualizar como única frontera de commit** (brief, «GOAL»,
«PRODUCT DECISION ALREADY MADE» y «SUCCESS CRITERIA» 1-12). Nombres, host y estructura concretos salen de Discovery/Freeze.

## 3. Problema

La necesidad y la decisión de producto las declara el Owner en el brief. La caracterización en el repositorio (arquitectura de
ventanas, ciclo de vida de `RACKEDITAR`, eventos, inventario, stale-state) es trabajo de Discovery (§6); este contrato no la adelanta.

## 4. Alcance y no-objetivos

**Alcance** (resumen por referencia; la fuente es el brief, «FUNCTIONAL SCOPE» 1-12):

- workspace modeless persistente, evaluando seriamente PaletteSet; conciencia del documento activo;
- selección de AutoCAD → RackId lógico → workspace, y navegación de RackCad → seleccionar / ver / centrar en AutoCAD;
- navegador e inventario ligero de racks por dibujo, con invalidación y refresco bajo demanda;
- pestañas tipo navegador (preview, fijadas, cierre sin modificar el dibujo, marca dirty, carga diferida);
- `DraftSession` por RackId con base comprometida, borrador, dirty y stale/conflict, sin perder borradores al cambiar de rack;
- integración progresiva de los seis editores, sin seis reescrituras paralelas, conservando `RACKEDITAR` durante la migración salvo
  cambio explícito del Freeze;
- base de multiselección como contexto «Selection (N)», sin abrir N pestañas;
- cierre de ID3 Foundation, clasificado COMPLETE o PARTIAL con residual exacto.

**Decisiones de producto ya tomadas por el Owner** (brief): **NO LIVE MUTATION**; cambiar de rack o pestaña no aplica cambios; un
borrador dirty nunca se reemplaza en silencio; el DWG/authored persistido es la autoridad comprometida y la workspace es estado
transitorio; los documentos se aíslan; smoke temprano del Owner antes de expandir los editores.

**No-objetivos** (brief, «OUT OF SCOPE» y «PREPARES BUT DOES NOT IMPLEMENT»): mutación en vivo; semantic grips; parámetros
calculados ID20 (son de I-63); BOM calculado ID23; ID28/29; optimizador de layout; semántica nueva de expresiones; cajetín o sistema
de dibujo; rediseño mayor de RACKPROYECTAR; restauración entre arranques salvo trivial; Apply global por lotes salvo Freeze explícito;
reescritura de toda la arquitectura de producto. La workspace prepara superficies de alojamiento para esas funciones sin
implementarlas.

Freeze, Freeze delta y A-n: **ninguno** todavía.

## 5. Fundaciones y evolución

```text
Consumes: UNKNOWN — candidatos del brief: Rack Identity, View Identity, Authored vs Effective, Project Variables / Expressions,
          Linked Properties, Custom Properties, DimensionViews, Shared View Foundation, Auto Rack Naming
Extends: UNKNOWN — candidato preliminar: hosting y ciclo de vida de los editores y sus adaptadores de comando
Introduces: UNKNOWN — candidatos preliminares: workspace modeless en memoria, sesión por documento, puente central de eventos de
            AutoCAD, inventario runtime de navegación y sesiones de borrador con control de obsolescencia
```

Son candidatos de intake, no afirmaciones. La [Proposal V3](I-64-proposal-v3.md) concreta qué consume y qué introduce (§§2-8 y anexo A);
la lista se fija en el Freeze. DC-08 del Discovery (§11) comprobó en la base de F0 la presencia de los símbolos y pruebas de
las nueve fundaciones del brief; el consumo concreto se fija en el Freeze. Cada entrada se vuelve a verificar con DC-08 en la base vigente
([INITIATIVE_LIFECYCLE](../INITIATIVE_LIFECYCLE.md) §4.1), contra fuente, código y pruebas. Que una fundación no tenga entrada en
[FOUNDATIONS](../FOUNDATIONS.md) (p. ej., Auto Rack Naming) no prueba que no exista. La workspace **no** se convierte en autoridad
persistente ni crea una segunda autoridad de persistencia (brief, «KEY ARCHITECTURAL BOUNDARY» y «STALE / EXTERNAL CHANGE»).

## 6. Discovery, decisiones y Freeze

- Discovery: [I-64-discovery.md](I-64-discovery.md), versión D1-R1. **Aceptado por el Coordinator, que cerró el Discovery** (evidencia
  §13). Responde las dieciocho preguntas del brief («DISCOVERY REQUIRED») sobre DC-01..09, con EXP-01..09 evaluadas y sus negativos
  razonados; su §17 detalla la disposición de C64-D1-01..07.
- Decisión del Master: `MASTER-I63-I64-01` (frontera con I-63 y reglas del smoke), registrada en la evidencia §13; **sustituida
  parcialmente** por `MASTER-I63-I64-02` (evidencia §15), que retira la fundación compartida con I-63 y conserva las reglas del smoke.
- Proposal V1: [I-64-proposal-v1.md](I-64-proposal-v1.md), con su [paquete](I-64-architect-package-v1.md). Veredicto del Architect
  (sesión separada): **CHANGES REQUIRED**, A64-PV1-01..22, aceptado por el Coordinator (evidencia §14).
- Proposal V2: [I-64-proposal-v2.md](I-64-proposal-v2.md) (`Frozen: NO`, autocontenida), que dispone A64-PV1-01..22 con las precisiones
  PV2-01..22 del Coordinator, con su [paquete](I-64-architect-package-v2.md). Queda como registro histórico; no llegó a re-revisarse.
- Proposal V3: [I-64-proposal-v3.md](I-64-proposal-v3.md) (`Frozen: NO`, autocontenida), que conserva la disposición de A64-PV1-01..22 e
  incorpora `MASTER-I63-I64-02` y el plan de gates confirmado. Paquete de re-revisión: [I-64-architect-package-v3.md](I-64-architect-package-v3.md).
  **Re-revisión pendiente** por el mismo Architect y por el Coordinator; NEW ARCHITECTURE exige rondas hasta acuerdo sobre la misma
  versión ([INITIATIVE_LIFECYCLE](../INITIATIVE_LIFECYCLE.md) §5).
- Freeze / Freeze delta / A-n: ninguno.
- Fuentes: [brief](I-64-owner-brief.txt) y [confirmación D0-R3](I-64-coordinator-confirmation-d0-r3.txt).

## 7. Dependencias, archivos calientes y coordinación

- Sin dependencia de producto no integrada; las fundaciones candidatas (§5) están integradas y se verifican en DC-08.
- **I-63 (ID20)** corre en paralelo. Frontera fijada por el mandato de ID20 y su contrato: ID20 posee la semántica de métricas, los
  providers y la agregación; esta unidad posee el inventario runtime de navegación y la representación de UI/sesión. No se consume
  código no integrado de I-63. Una fundación común es STOP al Master Orchestrator. Ambos Discovery registran un **candidato** a
  fundación común en la enumeración lógica de racks. El Master lo decidió en `MASTER-I63-I64-01`: I-63 es autor inicial de un snapshot
  lógico neutral y mínimo por RackId; I-64 posee el índice runtime. En la Proposal V2 solo F6 (navegador del documento) lo consume, y
  solo integrado en `origin/main`; si no lo está al cerrar F1..F5, STOP al Master. **Conflicto abierto:** I-63 registró después la decisión
  del Owner P-14 (sin contrato común; prevalece para I-63), así que no será autor de ese snapshot. Qué rige para I-64 lo deciden su
  Coordinator y el Owner (Proposal V2 §15; evidencia §14). **Resuelto por `MASTER-I63-I64-02`** (evidencia §15; Proposal V3 §19): se
  retiran la fundación compartida y la autoría de I-63; I-64 captura e indexa por su cuenta sobre autoridades integradas, sin declararlo
  fundación; ningún gate de I-64 depende de I-63, e I-63 no consume código no integrado de I-64.
- **I-52 (RACKMIRROR)** recorre en su producto planificado el ciclo `RACKEDITAR` → Actualizar → redibujo; la intersección se evalúa en
  DC-07 (EXP-07 si toca la misma autoridad o contrato). No se toca su rama, worktree, paquete de host ni política de confianza. Su
  respuesta sobre la convivencia del host y las reglas del smoke del Master constan en la evidencia §13 y en la Proposal V3 §12.1.
- Archivos calientes: `docs/ROADMAP.md` (solo la fila propia, en los momentos de [WORKFLOW](../WORKFLOW.md) §2). Los de producto
  (editores grandes de `src/RackCad.UI/Systems/`, `src/RackCad.Plugin/*Commands*.cs`) se inventarían en DC-07. `HANDOFF.md` y el
  índice de ADR se editan únicamente al integrar/cerrar.

## 8. Gates funcionales

D0: reclamo y este bootstrap (tarea de F0, no gate funcional). La [Proposal V3](I-64-proposal-v3.md) §11 recoge el plan confirmado por el
Coordinator: F1 → Smoke-1; F2 Context Navigation → Smoke-NAV; F3 Browser Session; F4 con Smoke-2 dentro de su cierre; F5-A y F5-B; F6
Document-wide Inventory, Selección (N) e ID3, sin depender de I-63; F7; y READY, revisados contra
[INITIATIVE_LIFECYCLE](../INITIATIVE_LIFECYCLE.md) §7. **No está congelado.** La conformidad final se referencia desde READY-06.

## 9. Owner Validation

- Asignación OV: propuesta en la [Proposal V3](I-64-proposal-v3.md) §12 (OV-01..OV-26, todas a la unidad I-64); se fija en el Freeze.
- Requiere AutoCAD: **sí** (workspace y sincronización dentro de AutoCAD; smoke temprano tras F1).
- Requiere Owner Validation: **sí** (brief). El procedimiento y la identidad del DLL viven en
  [validacion-manual-autocad.md](../guias/validacion-manual-autocad.md).

## 10. Evidencia y entrega

- Evidencia por unidad: [`docs/automation/evidence/I-64-evidence.md`](../automation/evidence/I-64-evidence.md)
- Estado transitorio: pendiente (`docs/automation/state/I-64.yml`, antes de la primera delegación)
- Tag esperado: `integration/I-64`

Este contrato no copia SHAs, corridas ni conteos. La integración es manual, serializada y sin auto-merge, conforme a `WORKFLOW.md`.
Los gates de implementación usan la ejecución delegada de [AUTOMATION_PLAN](../AUTOMATION_PLAN.md) §16 bajo contratos de gate
aprobados (brief, «I-61 EXECUTION PROTOCOL»); la fricción de routing se registra como evidencia para I-62.

## 11. Criterios de aceptación

Los doce criterios de éxito del brief («SUCCESS CRITERIA»), concretados por el Freeze. Completa ≠ integrada.

## 12. Condiciones para detenerse

Las del brief más: contradicción material de fuentes o de autoridad (se citan ambas y se devuelve al Coordinator); EXP-01 clase A
abierta; M UNKNOWN sin resolver antes del Freeze; intersección activa no coordinada; fundación común con I-63 (Master
Orchestrator); consumo de código no integrado; cualquier paso que exija ejecutar AutoCAD, autenticación, instalación o cambios de
configuración no autorizados por un contrato aprobado. **IMPLEMENTATION AUTHORIZATION = NO** hasta Coordinator = AGREED y
Architect = AGREED sobre el mismo Freeze.

## 13. Hallazgos fuera de alcance

- Varios documentos normativos conservan encabezados que dicen «Workflow V2 no efectivo», aunque el registro durable de activación
  existe; se observan sin corregir (no-touch de D0).
