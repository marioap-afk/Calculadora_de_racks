---
schema: rackcad-initiative/v2
id:
title:
type:
status:
workflow: V2
conceptual_initiative:
delivery_unit:
archetype:
materiality: []
branch:
base_branch: main
priority:
size:
depends_on: []
conflicts_with: []
hot_files: []
coordination_strategy:
context_packs: []
consumes: []
extends: []
introduces: []
discovery_ref:
freeze_ref:
freeze_delta_ref:
amendment_refs: []
ov_assignment_ref:
decision_refs: []
evidence_ref:
automation_state_path:
requires_ci: true
requires_plugin_build:
requires_autocad:
requires_owner_decision:
requires_owner_validation:
automation:
  enabled: true
  auto_merge: false
  max_attempts: 3
---

# Titulo de la unidad

> Esta plantilla es para reclamos V2 posteriores a la activacion. Los contratos V1 y grandfathered
> conservan su schema y su forma: no se reescriben para adoptar estos campos. La clasificacion real se
> determina por Claim-Id y `WORKFLOW.md` §11, no por elegir manualmente el valor de `workflow`.

## 1. Identidad y agrupacion

Indicar iniciativa conceptual, unidad de entrega, arquetipo y disparadores `M-nn`. Explicar por que la
agrupacion satisface el contrato conjuntivo de `INITIATIVE_LIFECYCLE.md` §2. El contrato es mutable y
no contiene puntas vivas, hashes cambiantes ni conteos de pruebas.

## 2. Objetivo

Resultado verificable que debe producir esta unidad.

## 3. Problema

Problema actual y evidencia que justifica resolverlo. Enlazar Discovery; no copiarlo.

## 4. Alcance y no-objetivos

Resumir el alcance autorizado y enlazar Freeze, Freeze delta y A-n aplicables. El artefacto congelado
es la autoridad de alcance, invariantes, no-objetivos y matriz OV.

## 5. Fundaciones y evolucion

```text
Consumes:
Extends:
Introduces:
```

Para cada entrada, enlazar `docs/FOUNDATIONS.md` y registrar la comprobacion DC-08 en Discovery. Un
contrato V1 sin estos campos significa `UNKNOWN`, no ausencia de evolucion.

## 6. Discovery, decisiones y Freeze

- Discovery: `<ruta>`
- Freeze: `<ruta>`
- Freeze delta: `<ruta o no aplica>`
- A-n: `<rutas o ninguna>`
- Decisiones del Owner: `<rutas o ninguna>`

La semantica de Discovery, Architect, Freeze/A-n y conformidad vive en
`docs/INITIATIVE_LIFECYCLE.md`.

## 7. Dependencias, archivos calientes y coordinacion

Enumerar dependencias integradas, conflictos activos, archivos calientes y estrategia concreta para
serializar o dividir el trabajo. Las puntas observadas se registran en el informe/cuerpo de commit, no
en este contrato.

## 8. Gates funcionales

Secuencia de gates acotados con comportamiento y evidencia esperada. No existe commit ceremonial
`-CLOSE` obligatorio por gate. La composicion Full y las clases de evidencia se referencian desde
`AGENTS.md`; no se copian aqui.

## 9. Owner Validation

- Asignacion OV: `<Freeze / Freeze delta / A-n>`
- Requiere AutoCAD: `<si/no, con disparador>`
- Requiere Owner Validation: `<si/no, con disparador>`

La matriz OV vive en Freeze/delta/A-n. Este contrato solo enlaza su asignacion; no mantiene una copia
mutable. El procedimiento y la identidad del DLL viven en
`docs/guias/validacion-manual-autocad.md`.

## 10. Evidencia y entrega

- Evidencia por unidad: `docs/automation/evidence/<unit>-evidence.md`
- Estado transitorio del ejecutor: `docs/automation/state/<unit>.yml`
- Tag esperado: `integration/<unit>`

El archivo de evidencia conserva clasificacion, identidad de Freeze, rondas de Candidato,
conformidad, Owner Validation y metricas. Este contrato no copia SHAs, corridas ni conteos. La
integracion es manual, serializada y sin auto-merge conforme a `WORKFLOW.md`.

## 11. Criterios de aceptacion

Condiciones observables para considerar completa la implementacion, sin confundirla con integrada.

## 12. Condiciones para detenerse

Decisiones, dependencias, conflictos, discrepancias EXP-01, fallos o ampliaciones de alcance que
obligan a detener el gate.

## 13. Hallazgos fuera de alcance

Lista local que se transfiere a `docs/ideas-futuras.md` en el cierre concentrado.
