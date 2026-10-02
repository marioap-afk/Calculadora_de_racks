---
schema: rackcad-initiative/v2
id: I-63
title: Computed Parameters & Project Summary Foundation
type: architecture
status: g3-implementing
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
discovery_ref: docs/initiatives/I-63-discovery.md
freeze_ref: docs/initiatives/I-63-proposal-v3.md
freeze_delta_ref:
amendment_refs: [docs/initiatives/I-63-proposal-v3-amendment-a1-verificacion.md, docs/initiatives/I-63-proposal-v3-amendment-a2-gate-verification-sequencing.md, docs/initiatives/I-63-proposal-v3-amendment-a3-inv22-diagnostico.md]
ov_assignment_ref:
decision_refs: [docs/automation/decisions/I-63.md, docs/automation/decisions/I-63-owner-mandate.original.txt]
evidence_ref: docs/automation/evidence/I-63-evidence.md
automation_state_path: docs/automation/state/I-63.yml
requires_ci: true
requires_plugin_build: true
requires_autocad: false
requires_owner_decision:
requires_owner_validation: false
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

- Discovery: [I-63-discovery.md](I-63-discovery.md), ronda **R2**, **aceptada** por el Coordinator (CD-I63-F0-R2-04). Aplica las precisiones
  localizadas R63-R1-01 y R63-R1-02 (CD-I63-F0-R2-01 y CD-I63-F0-R2-02) sobre R1, que a su vez corrigió R0 (CD-I63-F0-R1-01) y
  completó las EXP de CD-I63-F0-R1-02. No es Proposal ni Freeze. La hizo directamente la sesión principal responsable, sin
  participantes de IA. Las rondas R0 y R1 se conservan en Git. Sus §24, §25 y §26 registran las decisiones posteriores del Owner
  (P-14, P-01..P-05 y el cierre de P-05).
- Proposal vigente: [I-63-proposal-v3.md](I-63-proposal-v3.md) (Frozen: NO), con el paquete de re-revisión
  [I-63-architect-package-v3.md](I-63-architect-package-v3.md). V1 ([I-63-proposal-v1.md](I-63-proposal-v1.md)) y V2
  ([I-63-proposal-v2.md](I-63-proposal-v2.md)) son históricas: el Architect dio CHANGES REQUIRED en R1 y R2.
- **Freeze:** la Proposal V3 congelada ([I-63-proposal-v3.md](I-63-proposal-v3.md), `Frozen: YES`), por consenso Coordinator + Architect R3.
- **A-n:** [A-1](I-63-proposal-v3-amendment-a1-verificacion.md), solo del Coordinator: precisiones de verificación O-PV3-1..3.
- Decisiones: [I-63.md](../automation/decisions/I-63.md); mandato: [I-63-owner-mandate.original.txt](../automation/decisions/I-63-owner-mandate.original.txt).

Discovery debe responder las catorce preguntas del mandato («DISCOVERY REQUIRED») sobre DC-01..09, con EXP-01..09 evaluadas y sus
negativos razonados. **EXP-09 es expansión requerida**: ¿se crea una autoridad o mecanismo transversal (M-07) o solo se extienden
puntos integrados (M-01..06/M-08)? La Proposal/Freeze cubre las obligaciones de «PROPOSAL / FREEZE MUST DEFINE» del mandato y además
delimita CustomProperty, ProjectVariable y ComputedParameter con referencia a
[ADR-0039](../adr/0039-custom-properties-persistencia-autoridad.md) D-16.

**Tareas de F0** ([INITIATIVE_LIFECYCLE](../INITIATIVE_LIFECYCLE.md) §4). Las cuatro primeras se ejecutaron en F0-DISCOVERY,
directamente por la sesión principal; las dos últimas siguen pendientes:

| Tarea | Perfil o actor | Salida |
|---|---|---|
| Inventario de autoridades, símbolos, fases y fuentes de conteo (DC-01..06; preguntas 1-12 y 14 del mandato) | DOCUMENTATION | `docs/initiatives/I-63-discovery.md` |
| Matriz sistema × métrica candidata, con evidencia por celda y sin pruebas nuevas | DOCUMENTATION | mismo archivo |
| Hotspots e intersecciones (DC-07); fundaciones verificadas contra fuente, código y pruebas (DC-08), incluida la identidad del Freeze final de I-49 sobre A3-R2 | sesión responsable | Discovery y evidencia |
| DC-09 y EXP-01..09 con negativos razonados; EXP-09 requerida | sesión responsable; revisión del Coordinator | Discovery |
| Pruebas de caracterización nuevas, solo si una pregunta las exige | CHARACTERIZATION, con contrato aprobado aparte | — |
| Revisión del Architect sobre la Proposal | ARCHITECTURE_REVIEW; modo de revisión declarado | revisión versionada |

C-F0-RED queda registrado en las [decisiones](../automation/decisions/I-63.md) como limitación no corregida del protocolo
([AUTOMATION_PLAN](../AUTOMATION_PLAN.md) §16.8-16.9). No bloquea el Discovery directo y no crea ninguna exención para delegaciones.

## 7. Dependencias, archivos calientes y coordinación

- Dependencia integrada: **I-49** (Expression Engine), cuyo ADR reserva `Rack.*`, `Project.*` y el ámbito `Rack` para ID20. Las demás
  fundaciones candidatas (§5) están integradas y se verifican en DC-08.
- **I-64 — ID30, Persistent RackCad Workspace**, está activa en paralelo. Frontera del mandato, que el contrato de I-64 recoge igual:
  ID20 posee la semántica de métricas, los providers y la semántica de agregación; ID30 posee el inventario runtime de navegación y la
  representación de UI/sesión. No se consume código no integrado de I-64 ni se copia su diseño.
- **Decisión del Owner sobre P-14** (2026-10-01, posterior a R2; [Discovery](I-63-discovery.md) §24, [decisiones](../automation/decisions/I-63.md) §6):
  - no se crea ni se exige en esta iniciativa un contrato común de inventario o enumeración con I-64;
  - I-63 implementa su propio mecanismo para sus métricas, e I-64 el suyo para el Workspace;
  - la duplicación queda aceptada conscientemente y diferida a una futura iniciativa arquitectónica separada;
  - la decisión prevalece, para I-63, sobre `MASTER-I63-I64-01`, registrada por I-64;
  - antes, la regla era «si aparece una fundación común: STOP y Master Orchestrator». Para el mecanismo propio de I-63 ya no aplica;
    crear una fundación compartida queda fuera de esta iniciativa.

**Mapa preliminar de DC-07** (las puntas observadas viven en la [evidencia](../automation/evidence/I-63-evidence.md)):

| Iniciativa activa | Rama | Cruce textual conocido | Cruce funcional |
|---|---|---|---|
| I-64 (ID30) | `architecture/workspace-persistente-rackcad` | Su fila de ROADMAP también va tras I-60: conflicto textual de filas adyacentes para quien integre segundo | Código no inspeccionado (su rama solo tiene documentos). El Discovery identificó un candidato a contrato compartido (CD-I63-F0-R1-03). **El Owner decidió no exigirlo:** mecanismos independientes, duplicación diferida y consultas al Master superadas sin enviar. La Proposal de I-64 (V2) sigue aplicando `MASTER-I63-I64-01`, declara abierto el conflicto y hace depender su F6 de un *snapshot* de I-63. I-63 se lo avisó; resolverlo corresponde a su Coordinator y al Owner ([Discovery](I-63-discovery.md) §9.4, §9.5, §24) |
| I-62 (portabilidad del Principal) | `architecture/portabilidad-coordinador-principal` | Fila en otra tabla de ROADMAP (Engineering Productivity, tras I-61) | Ninguno de producto: su rama solo tiene documentos de proceso (Discovery y Proposal incluidos) |
| I-52 (RACKMIRROR) | `feature/rackmirror-espejo-semantico` | Edita ROADMAP (su fila y la de I-57), HANDOFF y el índice de ADR; su rebase futuro sobre main tocará ROADMAP | Sin cambios propios en `src/` ni `tests/` (diff de tres puntos; solo `eng/` y `docs/`); su contenido no se inspeccionó entero ([Discovery](I-63-discovery.md) §9.3) |

- La escritura de `docs/ROADMAP.md` se coordina por acuse (decisiones y evidencia). El DC-07 vigente está en el [Discovery](I-63-discovery.md) §9.
- Archivos calientes: `docs/ROADMAP.md` (solo la fila propia, en los momentos de [WORKFLOW](../WORKFLOW.md) §2). `HANDOFF.md` y el índice
  de ADR se editan únicamente al integrar/cerrar.

## 8. Gates funcionales

G0: reclamo y bootstrap, **PASS** (CD-I63-G0-10). F0-DISCOVERY: R0 con CHANGES REQUIRED (CD-I63-F0-R1-01); R1 con CHANGES REQUIRED
localizado (CD-I63-F0-R2-01); R2 **aceptada** (CD-I63-F0-R2-04), sin GATE PASS de F0 (Discovery + Freeze). P-14 quedó resuelta por
el Owner, sin contrato común (§7). El Owner también resolvió P-01..P-05 ([Discovery](I-63-discovery.md) §25 y §26). Con la orden PV1, F0-DISCOVERY queda cerrado para pasar al diseño. Architect R1 (V1) y R2
(V2) = CHANGES REQUIRED. **Architect R3 (V3) = AGREED** sobre `fb3c8788` / blob `e4a94eff`. **FREEZE** por orden del Coordinator, con la
A-1 solo del Coordinator. **F0 = PASS.** G1 autorizado bajo I-61; tras el STOP S-04 (evidencia §23), corrección 1 autorizada por el Coordinator (evidencia §24), con su planificación repetida tras el STOP P-03 (evidencia §25-§26) y su verificación detenida en STOP S-12 (evidencia §27); corrección 2 autorizada (evidencia §28), con su planificación detenida dos veces en STOP P-03 de la aceptación y resuelta sin cambio del trabajo (evidencia §29-§32); verificación `EXECUTION_VERIFIED` sobre `4a6c2d88` con nc1-nc3 conformes (evidencia §33). **G1 PASS** del Coordinator (evidencia §34); G2 autorizado con la A-2 solo del Coordinator (evidencia §36) y verificado `EXECUTION_VERIFIED` sobre `844dabb6` con nc1, nc3 y nc4 conformes y nc2 no superado (evidencia §37). **G2 PASS** del Coordinator (evidencia §38); G3 autorizado como ejecución escalonada T1 (RED) → A-3 → T2 (GREEN) (evidencia §40); RED de G3-T1 `637dce7e` acreditado y A-3 materializada con el resultado de INV-22 (evidencia §41); condición de la A-3 corregida por el Coordinator y G3-T2 autorizado (evidencia §42-§43); G2, G3 y G4 no autorizados. El plan provisional del mandato (F0 Discovery y Freeze; F1..F6; READY) **no está congelado** y se
revisa contra [INITIATIVE_LIFECYCLE](../INITIATIVE_LIFECYCLE.md) §7 antes del Freeze: ningún gate solo de capa, DTO o modelo sin
resultado observable verificable. La conformidad final se referencia desde READY-06.

## 9. Owner Validation

- **Owner Validation: NOT APPLICABLE para I-63 V1** (orden de Freeze). No hay UI, comandos, cambios de dibujo ni integración host.
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
abierta; M UNKNOWN sin resolver antes del Freeze; intersección activa no coordinada; crear o exigir una fundación común con ID30 (fuera de esta iniciativa por decisión del Owner sobre P-14);
consumo de código no integrado; cualquier paso que exija autenticación, instalación, cambios de configuración o participantes no
autorizados por un contrato aprobado.

## 13. Hallazgos fuera de alcance

- Varios documentos normativos conservan encabezados que dicen «Workflow V2 no efectivo», aunque el registro durable de activación
  existe; se observan sin corregir (no-touch de G0).
