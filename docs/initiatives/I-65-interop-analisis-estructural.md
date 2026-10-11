---
schema: rackcad-initiative/v2
id: I-65
title: Structural Analysis Interop Foundation + RAM Elements Adapter
type: architecture
status: f1-closed
workflow: V2
conceptual_initiative: I-65
delivery_unit: I-65
archetype: NEW ARCHITECTURE
materiality: [M-07]
branch: architecture/interop-analisis-estructural
base_branch: main
priority: HIGH
size:
depends_on: []
conflicts_with: []
hot_files: [docs/ROADMAP.md]
coordination_strategy: Bootstrap con acuse de ventana de ROADMAP de las sesiones paralelas (I-52, I-62, I-64); frontera de propiedad con ID30 (I-64) e I-62 en el mandato y en DC-07.
context_packs: [architecture-kernel, catalogs-data, system-selective]
consumes: [Rack Identity (ADR-0009), catalogo neutral de secciones (ADR-0020..0023), colocacion Cantilever (ADR-0022/0024/0025), LengthUnits (ADR-0005, ADR-0040 D3), ADR-0017]
extends: [StructuralSectionUnits (ADR-0021 §10), bit a bit]
introduces: [Structural Analysis Model neutral, frontera de unidades de analisis, frontera de proveedores de analisis, supuestos explicitos, canal de cargas explicitas, huellas de fuente y de entrada]
discovery_ref: docs/initiatives/I-65-discovery.md
freeze_ref: docs/initiatives/I-65-proposal-v3.1.md
freeze_delta_ref: null
amendment_refs: []
ov_assignment_ref: docs/initiatives/I-65-proposal-v3.1.md#5
decision_refs: [docs/automation/decisions/I-65.md, docs/automation/decisions/I-65-owner-mandate.original.txt, docs/automation/decisions/I-65-owner-decision-p1.original.txt, docs/automation/decisions/I-65-owner-decision-freeze-f1.original.txt, docs/adr/0050-interoperabilidad-analisis-estructural.md]
evidence_ref: docs/automation/evidence/I-65-evidence.md
automation_state_path: docs/automation/state/I-65.yml
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

# I-65 — Structural Analysis Interop Foundation + RAM Elements Adapter

> Contrato **manual** (`automation.enabled: false`): el ejecutor nocturno no selecciona ni reanuda esta iniciativa.
> `requires_plugin_build`, `requires_autocad` y `requires_owner_validation` son `true` por el Freeze (Proposal V3.1 §11, D-13,
> OV-I65-01, A65-V1-19). `requires_owner_decision` sigue **vacío a propósito**: el Freeze no lo fija; no es `false` y no concede
> exenciones ([AUTOMATION_PLAN](../AUTOMATION_PLAN.md) §11, regla monotónica). `priority: HIGH` procede del mandato («Priority: HIGH /
> PARALLEL PRODUCT TRACK»); `size` no tiene fuente y queda vacío.

## 1. Identidad y agrupación

- Número técnico **I-65**; en el mandato figura como `<NEXT_FREE_INITIATIVE_ID>`. La resolución consta en las
  [decisiones](../automation/decisions/I-65.md) y el preflight en la [evidencia](../automation/evidence/I-65-evidence.md).
- Iniciativa conceptual y unidad de entrega: **I-65** (única, por ahora). Discovery decide si se parte en unidades
  ([INITIATIVE_LIFECYCLE](../INITIATIVE_LIFECYCLE.md) §2).
- Apertura: caso (d) de [WORKFLOW](../WORKFLOW.md) §2, por **mandato explícito del Owner**
  ([`I-65-owner-mandate.original.txt`](../automation/decisions/I-65-owner-mandate.original.txt), copia única y byte-idéntica; no se
  reescribe aquí).
- Workflow **V2**. La clasificación de transición se determina por el reclamo ([WORKFLOW](../WORKFLOW.md) §11.3) y se registra en la
  evidencia.
- **Arquetipo: NEW ARCHITECTURE** (mandato). Disparador previsto: **M-07** (contrato arquitectónico transversal nuevo: modelo
  estructural neutral y frontera de adaptadores de análisis). DC-09 lo confirma o lo corrige; ante duda rige el superior.

## 2. Objetivo

Una fundación **neutral de análisis estructural** derivada de RackCad y el **primer adaptador hacia RAM Elements**, que transfiera un
modelo de una rebanada vertical pequeña por un mecanismo **soportado** y que sea estructuralmente equivalente, bajo el contrato de mapeo
congelado, a un modelo de referencia hecho a mano en RAM Elements (mandato, «GOAL», «EQUIVALENCE TARGET» y «SUCCESS CRITERIA» 1-14).

## 3. Problema

La validación estructural de cargas está diferida a RAM Elements por decisión del Owner
([ADR-0017](../adr/0017-validacion-cargas-diferida-ram-elements.md)), pero hoy no existe ningún camino reproducible de RackCad hacia
RAM Elements. El problema y la intención los declara el Owner en el mandato («CONTEXT», «CORE PRINCIPLE»); la caracterización es
trabajo de Discovery (§6).

## 4. Alcance y no-objetivos

**Alcance** (resumen por referencia; la fuente es el mandato): caracterizar las superficies reales de integración de la versión
instalada de RAM Elements; un modelo estructural neutral sin identidades de proveedor; una frontera única de unidades; mapeos explícitos
de sección y material; idealización analítica distinta de la geometría gráfica; supuestos estructurales explícitos (conexiones, apoyos,
cargas); una rebanada vertical real; un adaptador por mecanismo soportado; equivalencia frente a una referencia manual; provenance y
detección de resultados obsoletos; validación temprana del Owner/ingeniería.

**No-objetivos** (mandato, «OUT OF SCOPE»): todos los sistemas en V1; motor de diseño RMI/AISC completo; automatización sísmica;
losa, cimentación y anclas; optimización o selección automática de miembros; mezzanine completo; UI final del Workspace (ID30); grips
semánticos; análisis en vivo; sobrescritura automática de archivos de RAM; ingeniería inversa de formatos propietarios; resultados de RAM
como autoridad de persistencia de RackCad. Tampoco absorbe **I-38** (ingeniería estructural de Cantilever: verificación y
dimensionado) ni reabre [ADR-0017](../adr/0017-validacion-cargas-diferida-ram-elements.md): RackCad no valida cargas por sí mismo.

Alcance congelado: [Proposal V3.1](I-65-proposal-v3.1.md) (Frozen: yes); Freeze delta y A-n: ninguno.

## 5. Fundaciones y evolución

La fuente es la [Proposal V3.1](I-65-proposal-v3.1.md) §7 (congelada); el front matter la resume.
Los candidatos de intake se verificaron con DC-08 en la Discovery.

## 6. Discovery, decisiones y Freeze

- Discovery: [I-65-discovery.md](I-65-discovery.md) (con adendas §13 y §14).
- Freeze: [I-65-proposal-v3.1.md](I-65-proposal-v3.1.md) (Frozen: yes); Freeze delta: no aplica; A-n: ninguna.
- ADR: [ADR-0050](../adr/0050-interoperabilidad-analisis-estructural.md) (aceptado).
- Decisiones: [I-65.md](../automation/decisions/I-65.md); mandato: [I-65-owner-mandate.original.txt](../automation/decisions/I-65-owner-mandate.original.txt).

Discovery responde la «FIRST DISCOVERY QUESTION» del mandato con fuentes oficiales fechadas y con el entorno instalado, y cubre DC-01..09
con EXP-01..09 evaluadas y sus negativos razonados. La Proposal cubre las obligaciones del mandato (modelo, unidades, secciones,
materiales, idealización, conexiones, apoyos, cargas, errores, provenance, pruebas, equivalencia y validación del Owner).

## 7. Dependencias, archivos calientes y coordinación

- Sin dependencias de iniciativas abiertas. Fundaciones candidatas integradas: §5.
- **ID30 / I-64** posee Workspace/PaletteSet, UX WPF, selección y eventos de AutoCAD y la sesión de RACKEDITAR. **I-62** posee la
  infraestructura de agentes y proceso. I-65 no toca ninguna de esas superficies ni consume su código no integrado. Si aparece una
  autoridad compartida de Producto/Dominio: STOP y aviso al Master Orchestrator antes de escribir.
- Archivos calientes: `docs/ROADMAP.md` (solo la fila propia, en los momentos de [WORKFLOW](../WORKFLOW.md) §2). `HANDOFF.md` y el
  índice de ADR se editan únicamente al integrar/cerrar. DC-07 fija el resto.

## 8. Gates funcionales

G0 (reclamo y bootstrap) y F0 (Discovery, sonda P1, Freeze) cumplidos. El plan de gates vigente es el de la Proposal V3.1 §6; F1 está
autorizada por el Owner (decisiones §9), se ejecutó en dos tareas (decisiones §10) y está **cerrada** (decisiones §16); los gates
siguientes esperan la decisión del Owner.

## 9. Owner Validation

- Asignación OV: [Proposal V3.1](I-65-proposal-v3.1.md) §5 (OV-I65-01..08). El hito temprano del mandato («OWNER / ENGINEERING
  VALIDATION») es OV-I65-01 en F4, antes de ampliar cobertura.
- Requiere AutoCAD y Owner Validation: **sí** (Freeze). RAM Elements: solo en las OV de F4/F5 y READY, por el ingeniero.

## 10. Evidencia y entrega

- Evidencia por unidad: [`docs/automation/evidence/I-65-evidence.md`](../automation/evidence/I-65-evidence.md)
- Estado transitorio: [`docs/automation/state/I-65.yml`](../automation/state/I-65.yml)
- Tag esperado: `integration/I-65`

Este contrato no copia SHAs, corridas ni conteos. La integración es manual, serializada y sin auto-merge, conforme a `WORKFLOW.md`. La
ejecución delegada tras el Freeze sigue [AUTOMATION_PLAN](../AUTOMATION_PLAN.md) §16 (protocolo integrado de I-61).

## 11. Criterios de aceptación

Los catorce criterios de éxito del mandato («SUCCESS CRITERIA»), concretados por el Freeze. Completa ≠ integrada.

## 12. Condiciones para detenerse

Las del mandato más: contradicción material de fuentes o de autoridad; documentación oficial que contradice el comportamiento instalado
(se registran ambas y se falla cerrado); EXP-01 clase A abierta; M UNKNOWN sin resolver antes del Freeze; intersección activa no
coordinada; autoridad compartida con ID30 o I-62 (Master Orchestrator); consumo de código no integrado; cualquier paso que exija
autenticación, licencias, instalación, descargas, cambios de configuración o ejecutar RAM Elements sin autorización del Owner; cualquier
escritura sobre archivos de ingeniería existentes del usuario.

## 13. Hallazgos fuera de alcance

Ninguno todavía.
