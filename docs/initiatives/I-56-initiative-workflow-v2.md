---
schema: rackcad-initiative/v1
id: I-56
title: "Initiative Workflow V2"
type: docs
status: integration-ready
branch: docs/initiative-workflow-v2
base_branch: main
priority:
size:
depends_on: []
conflicts_with: []
context_packs: [documentation-governance]
automation_state_path:
decision_paths: [docs/automation/decisions/I-56.md]
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

# I-56 — Initiative Workflow V2

> **Fase actual: MATERIALIZACION NORMATIVA Y CONFORMIDAD COMPLETAS; CANDIDATO Y CIERRE DOCUMENTAL
> LISTOS PARA INTEGRAR.** I-56 sigue siendo **solo documentacion y proceso** y continua gobernada por
> Workflow V1. La pausa de activacion esta **ACTIVE**; el merge normativo y las compuertas posteriores
> siguen pendientes, por lo que Workflow V2 **NO esta efectivo**.
>
> ```text
> I-56 IS DOCUMENTATION / PROCESS ONLY
> EVIDENCE AUDIT:            CLOSED
> PROPOSAL V4:               CONSENSUS REACHED
> DRY-RUN:                   PASS
> OWNER:                     APPROVED — PROPOSAL V4
> WORKFLOW V2:               NOT EFFECTIVE        (seccion 0.2)
> WORKFLOW_V2_EFFECTIVE_SHA: DOES NOT EXIST YET   (no se inventa)
> ```

```text
Initiative = I-56
Branch     = docs/initiative-workflow-v2
Worktree   = ~/.codex/worktrees/docs-initiative-workflow-v2
Baseline   = padre del commit de reclamo atomico (punta de origin/main al reclamar)
Claim-Id   = 82946e97-508a-4e0e-9b9c-09b9936be121
```

> **Sin hashes de commit en este contrato**: [AGENTS.md](../../AGENTS.md) y [WORKFLOW](../WORKFLOW.md) seccion 8
> los reservan a `docs/HANDOFF.md` seccion 12. La base, el reclamo y la punta previa a cada correccion se
> identifican por su SHA en los cuerpos de los commits de reclamo, de bootstrap y de correccion, que son la fuente.

> **Apertura por autorizacion explicita sin fila previa** — caso (d) de [WORKFLOW](../WORKFLOW.md) seccion 2. La
> autorizacion fue la **orden directa** recibida en la sesion de apertura («I-56 — G0 only»). Sustituye
> **unicamente** la preexistencia de la fila en ROADMAP: la fila durable y este contrato nacen en el bootstrap
> inmediatamente posterior al reclamo atomico. En aquel bootstrap no existia un registro de decisiones porque
> el caso (d) no lo exige. La aprobacion posterior de Proposal V4 ya se registra en
> `docs/automation/decisions/I-56.md`, y `decision_paths` apunta a ese canal durable.

## 0. Invariantes vinculantes

Registradas en **G0.1** por orden directa, que las declara **ya vinculantes**. No son resultado de la Evidence
Audit ni de la Proposal, y ninguna fase de I-56 las reabre sin orden expresa.

### 0.1 Invariante de transicion

```text
I-49, I-52 and I-55 are grandfathered.
Any initiative formally claimed before WORKFLOW_V2_EFFECTIVE_SHA is grandfathered.
A grandfathered initiative finishes under the workflow with which it was claimed.
I-56 must not rewrite active initiative contracts to apply V2 retroactively.
```

- Una iniciativa **grandfathered** termina bajo el workflow con el que se reclamo: la V2 **no** se le aplica a mitad
  de camino.
- **I-56 no reescribe** el contrato de ninguna iniciativa activa para aplicarle la V2 de forma retroactiva.
- Aplicacion literal de la segunda regla a I-56, no una regla nueva: I-56 se reclamo antes de que exista
  `WORKFLOW_V2_EFFECTIVE_SHA`, asi que **tambien queda grandfathered** y termina bajo el workflow con el que se
  reclamo.

### 0.2 Invariante de vigencia

```text
No normative Workflow V2 policy becomes effective until:

  Coordinator = AGREED
  Architect   = AGREED
      on the same version,
  and Owner   = APPROVED.

WORKFLOW_V2_EFFECTIVE_SHA does not exist yet.
```

- `WORKFLOW_V2_EFFECTIVE_SHA` **no existe todavia** y **no se inventa** en G0.1: ni un SHA ni un marcador con aspecto
  de SHA.
- Hasta que se cumplan las tres condiciones sobre la **misma** version, **ninguna** politica de la V2 es norma, aunque
  este redactada, versionada o acordada por una sola de las partes.
- La aprobacion del Owner es la **decision** que declara `requires_owner_decision: true` (seccion 6); no es una
  validacion de producto.

## 1. Objetivo

**Lo que fija la orden de apertura:**

- el ID, **I-56**;
- la naturaleza: **solo documentacion y proceso**; el objeto de la iniciativa no es el producto;
- la rama, `docs/initiative-workflow-v2`, de la que sale literalmente el nombre **Initiative Workflow V2**;
- la secuencia **G0 → Evidence Audit → Proposal**, y que las dos ultimas no empiezan en G0;
- la restriccion de G0: **todo cambio bajo `docs/**`**, y `docs/ORCHESTRATION.md` **no se crea en G0**.

**S-1 — CONFIRMADO en G0.1; ya no es un supuesto.** El alcance es el **workflow / proceso de iniciativas para las
iniciativas futuras**. Incluye el proceso que norman [WORKFLOW](../WORKFLOW.md), [AGENTS.md](../../AGENTS.md),
[AUTOMATION_PLAN](../AUTOMATION_PLAN.md), [ROADMAP](../ROADMAP.md) / [HANDOFF](../HANDOFF.md) y la documentacion
relacionada de iniciativas y de decisiones, segun corresponda. «Para las iniciativas futuras» se lee con la
transicion de la seccion 0.1: la V2 no se aplica retroactivamente a las iniciativas grandfathered. Que cambiara la V2
en cada documento, y en que sentido, **sigue sin afirmarse**: lo fija la Proposal sobre la evidencia de la Evidence
Audit, y nada entra en vigor sin la compuerta de la seccion 0.2.

**Lo que fija la orden de correccion (G0.1):** S-1 confirmado, `requires_owner_decision: true`,
`requires_owner_validation: false` y las dos invariantes de la seccion 0.

**Resultado verificable de la sesion autorizada (G0):** reclamo atomico publicado, este contrato y la fila en
ROADMAP. **De G0.1:** este contrato corregido (seccion 11). El resultado verificable **de la iniciativa** lo fija la
Proposal.

## 2. Problema

G0 y G0.1 **no auditan**: el problema y su evidencia son el entregable de la Evidence Audit. G0 solo registra el
hecho que la orden mando verificar:

- `docs/ORCHESTRATION.md` **no existe** en `origin/main`, ni en el arbol de las ramas activas al reclamar, ni en el
  historial de ninguna referencia. Cuatro documentos de `main` ya registran su ausencia y que WORKFLOW manda
  (contrato y estado de I-36D, estado de I-39A y contrato de I-43). **G0 y G0.1 no lo crean**; si la V2 lo necesita
  lo decide la Proposal.

## 3. Alcance

1. **G0 — reclamo y bootstrap**: reclamo atomico publicado, este contrato y la fila en ROADMAP.
2. **G0.1 — correccion del contrato de bootstrap**: S-1 confirmado, metadata del Owner y las dos invariantes de la
   seccion 0. Solo este archivo.
3. **Evidence Audit**: solo documentacion, con **orden propia** que fija su objeto y su metodo. **No autorizada** en
   G0 ni en G0.1.
4. **Proposal**: con **orden propia**. **No autorizada** en G0 ni en G0.1.
5. **Fases posteriores** (aplicar lo que fije la Proposal): se definen **despues** de la Proposal, no aqui, y ninguna
   pone en vigor una politica normativa sin la compuerta de la seccion 0.2.

## 4. Fuera de alcance

- **Cualquier cambio fuera de `docs/**`** en G0 y G0.1: nada en `src/`, `tests/`, `assets/`, `eng/`, `deploy/`,
  `tools/`, `.github/`, `AGENTS.md`, `CLAUDE.md` ni `README.md`.
- **Producto**: dibujo, BOM, GUID, persistencia, catalogos, DWG y pruebas. I-56 es solo documentacion y proceso en
  toda su vida, no solo en G0.
- **Cambiar en G0 o G0.1 una norma de proceso compartida**: WORKFLOW, AGENTS, AUTOMATION_PLAN, `TEMPLATE.md`, el
  README de iniciativas o los Context Packs. I-56 **se rige por el proceso vigente con el que se reclamo** (seccion
  0.1); su rama no cambia unilateralmente las reglas globales ([AUTOMATION_PLAN](../AUTOMATION_PLAN.md) seccion 2,
  «Modo normal»), y un cambio de proceso se documenta antes de aplicarse ([WORKFLOW](../WORKFLOW.md) seccion 8).
- **Aplicar la V2 retroactivamente**: reescribir el contrato de una iniciativa activa, o de cualquier iniciativa
  grandfathered, para aplicarle la V2 (seccion 0.1).
- **Poner en vigor una politica normativa de la V2, o fijar `WORKFLOW_V2_EFFECTIVE_SHA`**, fuera de la compuerta de
  la seccion 0.2.
- **Crear `docs/ORCHESTRATION.md` en G0 o G0.1.**
- **Iniciar la Evidence Audit o la Proposal.**
- `docs/HANDOFF.md`: prohibido fuera de la integracion ([WORKFLOW](../WORKFLOW.md) seccion 2).
- Filas, contratos y territorio de las iniciativas activas: I-49, I-52 e I-55.

## 5. Contexto requerido

- Los documentos que la orden mando verificar: [ROADMAP](../ROADMAP.md), [HANDOFF](../HANDOFF.md),
  [WORKFLOW](../WORKFLOW.md), [AGENTS.md](../../AGENTS.md) y [AUTOMATION_PLAN](../AUTOMATION_PLAN.md).
- [README de iniciativas](README.md), [TEMPLATE.md](TEMPLATE.md) y las decisiones versionadas de
  `docs/automation/decisions/`, segun corresponda.
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
  evidencia. `docs/adr/README.md` solo seria cruce si la Proposal creara un ADR, lo que no esta decidido. Las tres
  estan **grandfathered** (seccion 0.1).
- **Entrada del Owner** (fijada en G0.1):
  - `requires_owner_decision: true`. **Decision**: la aprobacion de politica del Owner (`Owner = APPROVED`) sobre la
    version de Workflow V2 en la que `Coordinator = AGREED` y `Architect = AGREED`. **Punto en que se necesita**:
    antes de que cualquier politica normativa de la V2 entre en vigor (seccion 0.2). Asi identifica la decision y su
    punto, como pide [AUTOMATION_PLAN](../AUTOMATION_PLAN.md) seccion 11. La decision ya esta aprobada y
    registrada en `docs/automation/decisions/I-56.md`; no activa por si sola Workflow V2.
  - `requires_owner_validation: false`. I-56 **no** requiere validacion del Owner en AutoCAD ni de producto: no cambia
    comportamiento de dibujo ([AGENTS.md](../../AGENTS.md), punto 5). Esa metadata es monotonica
    ([AUTOMATION_PLAN](../AUTOMATION_PLAN.md) seccion 11): `false` no cancela ninguna obligacion que venga de otro
    sitio, y la aprobacion de politica de arriba es una **decision**, no una validacion.

## 7. Archivos esperados

**G0**: solo `docs/initiatives/I-56-initiative-workflow-v2.md` (este contrato) y la fila de I-56 en
`docs/ROADMAP.md`. **G0.1**: solo este contrato; `docs/ROADMAP.md` no se toca. Los archivos de las fases posteriores
los fija la Proposal. **Una desviacion material frente a esto obliga a detenerse.**

## 8. Fases

| # | Fase | Entregable | Estado |
|---|---|---|---|
| G0 | Reclamo + bootstrap | Reclamo atomico publicado, contrato y fila en ROADMAP | **HECHA** |
| G0.1 | Correccion del contrato de bootstrap | S-1 confirmado, metadata del Owner e invariantes de la seccion 0 | **HECHA** |
| G1 | Evidence Audit | Auditoria aceptada y cerrada | **HECHA** |
| G2–G7 | Proposal y consenso | Proposal V4 exacta con Coordinator + Architect AGREED | **HECHA** |
| G8 | Dry-run historico | Tres casos PASS; ninguna captura material perdida | **HECHA** |
| G9 | Paquete y decision del Owner | 17 selecciones + aprobacion exacta de Proposal V4 | **HECHA** |
| — | Materializacion normativa | ADR, normas, controles e integracion segun Proposal V4 | **HECHA hasta cierre documental** — materializacion, controles, conformidad y Candidato completos; merge y compuertas posteriores pendientes |

Ninguna fase arranca sin que la anterior tenga evidencia revisable.

## 9. Pruebas y builds

I-56 no produce codigo, asi que su contenido no exige suites locales ni builds de UI o Plugin. El CI corre en todo
push, y un commit que solo toca documentacion **sigue siendo un SHA nuevo** que no hereda evidencia
([AGENTS.md](../../AGENTS.md), «Reutilizacion de evidencia»): cuando un punto del proceso exija CI verde, se exige
sobre **su** SHA exacto ([WORKFLOW](../WORKFLOW.md) seccion 4.5.4). Medido en G0: ninguna prueba lee
`docs/ROADMAP.md`, `docs/WORKFLOW.md`, `docs/AUTOMATION_PLAN.md`, `AGENTS.md` ni este contrato.

## 10. Validacion manual

**No aplica en AutoCAD ni de producto**: I-56 no cambia comportamiento de dibujo ([AGENTS.md](../../AGENTS.md),
punto 5), asi que `requires_plugin_build`, `requires_autocad` y `requires_owner_validation` quedan en `false`. Lo que
I-56 si exige del Owner es una **decision de politica**, `Owner = APPROVED` antes de que la V2 entre en vigor,
registrada como `requires_owner_decision: true` (secciones 0.2 y 6).

## 11. Criterios de aceptacion

**De G0**, verificables ya:

1. `origin/docs/initiative-workflow-v2` existe por un primer push aceptado **sin force** cuyo commit es el reclamo
   vacio con `Initiative-Id`, `Branch`, `Claim-Id` y `Co-Authored-By`.
2. Este contrato existe, creado desde `TEMPLATE.md`, con `status: claimed`.
3. La fila de I-56 existe en `docs/ROADMAP.md`, en la seccion de iniciativas transversales, con Estado `pendiente` y
   sin hashes.
4. El bootstrap solo toca `docs/**`; `docs/HANDOFF.md` intacto; `docs/ORCHESTRATION.md` no creado.

**De G0.1**, verificables ya:

1. S-1 consta como **confirmado**, no como supuesto (seccion 1).
2. El frontmatter declara `requires_owner_decision: true` y `requires_owner_validation: false`, y la seccion 6
   identifica la decision y el punto en que se necesita.
3. Las invariantes de transicion y de vigencia constan en la seccion 0, **sin ningun valor** para
   `WORKFLOW_V2_EFFECTIVE_SHA`.
4. El diff de G0.1 es **solo este contrato**: sin ROADMAP, HANDOFF, normas compartidas ni `docs/ORCHESTRATION.md`, y
   sin hashes de commit anadidos.

**De la iniciativa**: los fija la Proposal; ninguno se cumple poniendo en vigor una politica sin la compuerta de la
seccion 0.2.

## 12. Condiciones para detenerse

- Toda fase de materializacion posterior exige **orden propia**. La aprobacion del Owner no la inicia por si sola.
- Si una fase necesitara tocar algo **fuera de `docs/**`** (por ejemplo `AGENTS.md` o `CLAUDE.md`, que viven en la
  raiz), se declara en la Proposal y se detiene hasta que su orden lo autorice.
- Si una fase pretendiera aplicar la V2 a una iniciativa grandfathered, o reescribir el contrato de una iniciativa
  activa para aplicarsela: **detenerse** (seccion 0.1).
- Si una fase pretendiera poner en vigor una politica de la V2, o fijar `WORKFLOW_V2_EFFECTIVE_SHA`, sin
  `Coordinator = AGREED` y `Architect = AGREED` sobre la misma version y `Owner = APPROVED`: **detenerse**
  (seccion 0.2).
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

**Correccion de contrato (G0.1).** Commit solo de documentacion que toca unicamente este contrato, publicado sobre la
punta del bootstrap **sin rebase** —`origin/main` no avanzo desde la base, asi que [WORKFLOW](../WORKFLOW.md) seccion
4.2 no lo exige— y **sin force**; el historial de reclamo y bootstrap se conserva. La punta previa consta en el cuerpo
de su commit.

El resto de la evidencia se acumula al cerrar cada fase.
