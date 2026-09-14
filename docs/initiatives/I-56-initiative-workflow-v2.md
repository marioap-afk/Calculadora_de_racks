---
schema: rackcad-initiative/v1
id: I-56
title: "Initiative Workflow V2"
type: docs
status: claimed
branch: docs/initiative-workflow-v2
base_branch: main
priority:
size:
depends_on: []
conflicts_with: []
context_packs: [documentation-governance]
automation_state_path:
decision_paths: []
requires_ci: true
requires_plugin_build: false
requires_autocad: false
requires_owner_decision:
requires_owner_validation:
automation:
  enabled: false
  auto_merge: false
  max_attempts: 3
---

# I-56 — Initiative Workflow V2

> **Fase actual: RECLAMADA Y BOOTSTRAPEADA (G0).** I-56 es **solo documentacion y proceso**. No hay Evidence
> Audit, no hay Proposal y **no se ha cambiado ninguna norma de proceso**. La sesion que abrio la iniciativa esta
> autorizada **solo para G0** (seccion 12).
>
> ```text
> I-56 IS DOCUMENTATION / PROCESS ONLY
> EVIDENCE AUDIT: NOT STARTED   (requiere orden propia)
> PROPOSAL:       NOT STARTED   (requiere orden propia)
> ```

```text
Initiative = I-56
Branch     = docs/initiative-workflow-v2
Worktree   = ~/.codex/worktrees/docs-initiative-workflow-v2
Baseline   = padre del commit de reclamo atomico (punta de origin/main al reclamar)
Claim-Id   = 82946e97-508a-4e0e-9b9c-09b9936be121
```

> **Sin hashes de commit en este contrato**: [AGENTS.md](../../AGENTS.md) y [WORKFLOW](../WORKFLOW.md) seccion 8
> los reservan a `docs/HANDOFF.md` seccion 12. La base y el reclamo se identifican por su SHA en los cuerpos de
> los commits de reclamo y de bootstrap, que son la fuente.

> **Apertura por autorizacion explicita sin fila previa** — caso (d) de [WORKFLOW](../WORKFLOW.md) seccion 2. La
> autorizacion es la **orden directa** recibida en la sesion de apertura («I-56 — G0 only»). Sustituye
> **unicamente** la preexistencia de la fila en ROADMAP: la fila durable y este contrato nacen en el bootstrap
> inmediatamente posterior al reclamo atomico. **No existe** `docs/automation/decisions/I-56.md` y el caso (d)
> **no lo exige**, asi que `decision_paths` queda vacio.

## 1. Objetivo

**Lo que fija la orden de apertura:**

- el ID, **I-56**;
- la naturaleza: **solo documentacion y proceso**; el objeto de la iniciativa no es el producto;
- la rama, `docs/initiative-workflow-v2`, de la que sale literalmente el nombre **Initiative Workflow V2**;
- la secuencia **G0 → Evidence Audit → Proposal**, y que las dos ultimas no empiezan en G0;
- la restriccion de G0: **todo cambio bajo `docs/**`**, y `docs/ORCHESTRATION.md` **no se crea en G0**.

**Supuesto explicito a confirmar (S-1).** El «workflow» del nombre es el **proceso de iniciativas** que hoy norman
los documentos que la orden mando verificar antes de escribir: [WORKFLOW](../WORKFLOW.md) (ramas, reclamo,
sesiones, integracion y limpieza), [AGENTS.md](../../AGENTS.md) (definicion de terminado y reutilizacion de
evidencia), [AUTOMATION_PLAN](../AUTOMATION_PLAN.md) (contratos, estado versionado y decisiones),
[ROADMAP](../ROADMAP.md) (plan y registro de cierre) y [HANDOFF](../HANDOFF.md) (estado vivo). **No se afirma** que
la V2 cambie uno en particular ni en que sentido: lo fija la Proposal sobre la evidencia de la Evidence Audit. Si
S-1 no corresponde con la intencion de la orden, se corrige **antes** de la Evidence Audit.

**Resultado verificable de la sesion autorizada (G0):** reclamo atomico publicado, este contrato y la fila en
ROADMAP. El resultado verificable **de la iniciativa** lo fija la Proposal.

## 2. Problema

G0 **no audita**: el problema y su evidencia son el entregable de la Evidence Audit. G0 solo registra el hecho que
la orden mando verificar:

- `docs/ORCHESTRATION.md` **no existe** en `origin/main`, ni en el arbol de las ramas activas al reclamar, ni en el
  historial de ninguna referencia. Cuatro documentos de `main` ya registran su ausencia y que WORKFLOW manda
  (contrato y estado de I-36D, estado de I-39A y contrato de I-43). **G0 no lo crea**; si la V2 lo necesita lo
  decide la Proposal.

## 3. Alcance

1. **G0 — reclamo y bootstrap**: reclamo atomico publicado, este contrato y la fila en ROADMAP.
2. **Evidence Audit**: solo documentacion, con **orden propia** que fija su objeto y su metodo. **No autorizada** en
   esta sesion.
3. **Proposal**: con **orden propia**. **No autorizada** en esta sesion.
4. **Fases posteriores** (aplicar lo que fije la Proposal): se definen **despues** de la Proposal, no aqui.

## 4. Fuera de alcance

- **Cualquier cambio fuera de `docs/**`** en G0: nada en `src/`, `tests/`, `assets/`, `eng/`, `deploy/`, `tools/`,
  `.github/`, `AGENTS.md`, `CLAUDE.md` ni `README.md`.
- **Producto**: dibujo, BOM, GUID, persistencia, catalogos, DWG y pruebas. I-56 es solo documentacion y proceso en
  toda su vida, no solo en G0.
- **Cambiar en G0 una norma de proceso**: WORKFLOW, AGENTS, AUTOMATION_PLAN, `TEMPLATE.md`, el README de
  iniciativas o los Context Packs. Mientras no se integre otra cosa, I-56 **se rige por el proceso vigente**: una
  rama no cambia unilateralmente las reglas globales, que se leen de `origin/main`
  ([AUTOMATION_PLAN](../AUTOMATION_PLAN.md) seccion 2, «Modo normal»), y un cambio de proceso se documenta antes de
  aplicarse ([WORKFLOW](../WORKFLOW.md) seccion 8).
- **Crear `docs/ORCHESTRATION.md` en G0.**
- **Iniciar la Evidence Audit o la Proposal.**
- `docs/HANDOFF.md`: prohibido fuera de la integracion ([WORKFLOW](../WORKFLOW.md) seccion 2).
- Filas, contratos y territorio de las iniciativas activas: I-49, I-52 e I-55.

## 5. Contexto requerido

- Los documentos que la orden mando verificar: [ROADMAP](../ROADMAP.md), [HANDOFF](../HANDOFF.md),
  [WORKFLOW](../WORKFLOW.md), [AGENTS.md](../../AGENTS.md) y [AUTOMATION_PLAN](../AUTOMATION_PLAN.md).
- [README de iniciativas](README.md) y [TEMPLATE.md](TEMPLATE.md).
- Context Pack declarado en el frontmatter:
  [documentation-governance](../context-packs/documentation-governance.md) (`when_to_load`: estructura documental,
  ADRs, iniciativas o automatizacion). La Evidence Audit puede ampliar la lista si mide que hace falta otro.
- Criterios de ADR: [adr/README.md](../adr/README.md).
- Contratos de las iniciativas activas **en sus ramas remotas**, solo lectura y en el SHA exacto que se cite.

## 6. Dependencias

- **Sin dependencias pendientes** declaradas.
- **Iniciativas activas al reclamar**: I-49 (`architecture/motor-expresiones-parametricas`), I-52
  (`feature/rackmirror-espejo-semantico`) e I-55 (`feature/creacion-de-vistas`); sus puntas constan en el cuerpo del
  commit de reclamo. **Cruce medido** (merge-base..punta) contra los archivos de proceso y los calientes: las tres
  tocan solo `docs/ROADMAP.md` —su propia fila, en la tabla de la Fase 6— y `docs/adr/README.md`. Ninguna toca
  WORKFLOW, AGENTS, AUTOMATION_PLAN, `TEMPLATE.md`, el README de iniciativas, los Context Packs, la guia de
  validacion manual, HANDOFF ni `.github/`. Por eso `conflicts_with` queda **vacio**: no se declara un estorbo sin
  evidencia. `docs/adr/README.md` solo seria cruce si la Proposal creara un ADR, lo que no esta decidido.
- **Entrada del dueno**: `requires_owner_decision` y `requires_owner_validation` quedan **vacios** hasta que la
  Proposal los fije, en vez de fijarse por analogia. Esa metadata es monotonica
  ([AUTOMATION_PLAN](../AUTOMATION_PLAN.md) seccion 11): dejarla vacia no exime de nada.

## 7. Archivos esperados

**G0**: solo `docs/initiatives/I-56-initiative-workflow-v2.md` (este contrato) y la fila de I-56 en
`docs/ROADMAP.md`. Los archivos de las fases posteriores los fija la Proposal. **Una desviacion material frente a
esto obliga a detenerse.**

## 8. Fases

| # | Fase | Entregable | Estado |
|---|---|---|---|
| G0 | Reclamo + bootstrap | Reclamo atomico publicado, contrato y fila en ROADMAP | **HECHA** |
| — | Evidence Audit | Lo fija su orden | pendiente — **no autorizada** |
| — | Proposal | Lo fija su orden | pendiente — **no autorizada** |
| — | Posteriores | Se definen tras la Proposal | bloqueadas |

Ninguna fase arranca sin que la anterior tenga evidencia revisable.

## 9. Pruebas y builds

I-56 no produce codigo, asi que su contenido no exige suites locales ni builds de UI o Plugin. El CI corre en todo
push, y un commit que solo toca documentacion **sigue siendo un SHA nuevo** que no hereda evidencia
([AGENTS.md](../../AGENTS.md), «Reutilizacion de evidencia»): cuando un punto del proceso exija CI verde, se exige
sobre **su** SHA exacto ([WORKFLOW](../WORKFLOW.md) seccion 4.5.4). Medido en G0: ninguna prueba lee
`docs/ROADMAP.md`, `docs/WORKFLOW.md`, `docs/AUTOMATION_PLAN.md`, `AGENTS.md` ni este contrato.

## 10. Validacion manual

**No aplica en AutoCAD**: I-56 no cambia comportamiento de dibujo ([AGENTS.md](../../AGENTS.md), punto 5), asi que
`requires_plugin_build` y `requires_autocad` quedan en `false`. Si el dueno debe confirmar el proceso V2 antes de
integrarlo lo fija la Proposal (`requires_owner_validation` vacio).

## 11. Criterios de aceptacion

**De G0**, verificables ya:

1. `origin/docs/initiative-workflow-v2` existe por un primer push aceptado **sin force** cuyo commit es el reclamo
   vacio con `Initiative-Id`, `Branch`, `Claim-Id` y `Co-Authored-By`.
2. Este contrato existe, creado desde `TEMPLATE.md`, con `status: claimed`.
3. La fila de I-56 existe en `docs/ROADMAP.md`, en la seccion de iniciativas transversales, con Estado `pendiente` y
   sin hashes.
4. El bootstrap solo toca `docs/**`; `docs/HANDOFF.md` intacto; `docs/ORCHESTRATION.md` no creado.

**De la iniciativa**: los fija la Proposal.

## 12. Condiciones para detenerse

- **COMPUERTA DE ESTA SESION — G0, solo documentacion.** Evidence Audit y Proposal no se inician.
- Toda fase posterior exige **orden propia**.
- Si una fase necesitara tocar algo **fuera de `docs/**`** (por ejemplo `AGENTS.md` o `CLAUDE.md`, que viven en la
  raiz), se declara en la Proposal y se detiene hasta que su orden lo autorice.
- Si S-1 no corresponde con la intencion de la orden: se corrige antes de la Evidence Audit.
- Si el alcance deriva hacia el territorio de una iniciativa activa o hacia reabrir un ADR aceptado: **detenerse**.
- `docs/HANDOFF.md` **no se toca**.

## 13. Estado versionado y entrega del Pull Request

Sin `docs/automation/state/I-56.yml`: la automatizacion esta **desactivada** (`automation.enabled: false`; no hay
ejecutor activo segun [AUTOMATION_PLAN](../AUTOMATION_PLAN.md)) y el caso (d) no lo exige.
[WORKFLOW](../WORKFLOW.md) seccion 8 pide para este bootstrap exactamente **fila en ROADMAP mas contrato**. El
estado vivo se deriva de la existencia de `origin/docs/initiative-workflow-v2`.

No hay Pull Request. El merge automatico esta prohibido; la integracion es manual ([WORKFLOW](../WORKFLOW.md)
seccion 4.5).

## 14. Evidencia final

**Reclamo atomico (G0).** Commit vacio con `Initiative-Id: I-56`, `Branch: docs/initiative-workflow-v2`,
`Claim-Id: 82946e97-508a-4e0e-9b9c-09b9936be121` y `Co-Authored-By`, sobre la punta de `origin/main` al reclamar;
primer `git push -u origin docs/initiative-workflow-v2` **aceptado sin force** (`* [new branch]`), con el preflight
repetido justo antes. `main` **no fue modificada**. Los SHAs de base y reclamo constan en el cuerpo del commit de
reclamo y en el del bootstrap.

El resto de la evidencia se acumula al cerrar cada fase.
