---
schema: rackcad-initiative/v1
id:
title:
type:
status:
branch:
base_branch: main
priority:
size:
depends_on: []
conflicts_with: []
context_packs: []
automation_state_path:
decision_paths: []
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

# Titulo de la iniciativa

## 1. Objetivo

Resultado verificable que debe producir la iniciativa.

## 2. Problema

Problema actual y evidencia que justifica resolverlo.

## 3. Alcance

Cambios autorizados por el ROADMAP, sin ampliaciones laterales.

## 4. Fuera de alcance

Cambios relacionados que esta iniciativa no debe realizar.

## 5. Contexto requerido

Documentos, Context Packs, ADRs y codigo que deben leerse antes de editar.

## 6. Dependencias

Dependencias que deben estar integradas, conflictos que deben permanecer inactivos y entradas del
dueno que deben existir.

## 7. Archivos esperados

Archivos o areas que se espera crear, modificar o mover. Una desviacion material exige detenerse.

## 8. Fases

Secuencia de trabajo acotada. Cada fase debe terminar con evidencia revisable.

## 9. Pruebas y builds

Comandos automatizados, CI y builds locales requeridos.

## 10. Validacion manual

Checklist del dueno, incluido AutoCAD cuando corresponda. Especificar `no aplica` si no se requiere.

## 11. Criterios de aceptacion

Condiciones observables para considerar completa la implementacion, sin confundirla con integrada.

## 12. Condiciones para detenerse

Decisiones, dependencias, conflictos, fallos o expansiones de alcance que obligan a pausar.

## 13. Estado versionado y entrega del Pull Request

Ruta de `docs/automation/state/<initiative>.yml`, titulo y numero del Pull Request existente, estado
draft conocido y decisiones requeridas. El merge automatico esta prohibido. La incapacidad de
actualizar la descripcion del Pull Request no bloquea commit y push cuando el estado versionado queda
publicado.

El archivo de estado canonico contiene:

```yaml
schema: rackcad-automation-state/v1
automation_state:
  initiative:
  branch:
  claim_id:
  current_phase:
  state:
  gate:
  attempts:
  next_action:
  last_evidence_commit:
```

`initiative`, `branch` y `claim_id` se copian del reclamo y no cambian. `state` usa `claimed`,
`implementing`, `validating`, `ci-failed`, `waiting`, `review-ready`, `integration-ready` o
`completed`. `gate` usa `none`, `owner-decision`, `owner-validation`, `autocad`, `plugin-build`,
`ci`, `dependency`, `conflict`, `permissions` o `scope`.

El archivo es estado transitorio: Git y los resultados verificables prevalecen si lo contradicen.
`current_phase` apunta a la siguiente fase pendiente o a la actualmente detenida; `attempts` solo
aumenta al intentar corregir un fallo; `last_evidence_commit` es el SHA completo que respalda el
estado. El ejecutor actualiza el archivo al terminar cada ejecucion. El Pull Request puede contener
una copia opcional del bloque; no se requiere GitHub CLI para leerla o escribirla y su falta de
actualizacion no invalida el estado publicado. Nunca se abre un segundo Pull Request para la
iniciativa. `completed` no significa integrada: la integracion sigue siendo manual.

Las decisiones del dueno pueden llegar por `docs/automation/decisions/<initiative>.md`. Antes de
resolver un gate se verifica que la decision provenga de la rama remota, cubra todos los IDs exigidos
y autorice el alcance siguiente.

## 14. Evidencia final

Commits, archivos, pruebas, builds, checks, validaciones pendientes, intentos y confirmacion de que
`main` no fue modificada.
