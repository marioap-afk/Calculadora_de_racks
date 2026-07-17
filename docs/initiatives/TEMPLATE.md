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

## 13. Entrega del Pull Request

Titulo, estado draft, contenido minimo y revisores o decisiones requeridas. El merge automatico esta
prohibido.

Todo Pull Request gestionado por el ejecutor contiene exactamente un bloque de estado estructurado:

```yaml
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

`initiative`, `branch` y `claim_id` se copian del reclamo y no cambian. `state` y `gate` usan los
valores definidos en `docs/AUTOMATION_PLAN.md`; `attempts` es un entero no negativo y
`last_evidence_commit` es el SHA completo que respalda el estado. El bloque se actualiza en el mismo
Pull Request despues de cada fase, correccion de CI o cambio de gate. Nunca se abre un segundo Pull
Request para la iniciativa.

## 14. Evidencia final

Commits, archivos, pruebas, builds, checks, validaciones pendientes, intentos y confirmacion de que
`main` no fue modificada.
