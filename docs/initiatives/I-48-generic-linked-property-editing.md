---
schema: rackcad-initiative/v1
id: I-48
title: "Generic Linked Property Editing"
type: architecture
status: claimed
branch: architecture/generic-linked-property-editing
base_branch: main
priority:
size:
depends_on: [I-47]
conflicts_with: []
context_packs: [ui-editors, system-selective, architecture-kernel, persistence]
automation_state_path:
decision_paths: []
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

# Generic Linked Property Editing

> **Fase actual: RECLAMADA Y BOOTSTRAPEADA.** No hay Discovery hecho, no hay Proposal, no hay
> consenso y **no hay una sola linea de produccion escrita**. La implementacion esta BLOQUEADA por
> la compuerta de la seccion 12.

> **Apertura por autorizacion explicita del dueno sin fila previa** — caso (d) de
> [WORKFLOW](../WORKFLOW.md) seccion 2. Esa autorizacion sustituye **unicamente** la preexistencia de
> la fila en ROADMAP: la fila durable y este contrato nacen en el bootstrap inmediatamente posterior
> al reclamo atomico. La autorizacion es la **instruccion directa del dueno** que abrio la
> iniciativa; **no existe** `docs/automation/decisions/I-48.md` y la norma **no lo exige** para el
> caso (d), asi que `decision_paths` queda vacio en vez de apuntar a un archivo inventado.

## 1. Objetivo

Sustituir el **caso especial de `VerticalClearance`** por una **edicion vinculable reusable**.

I-47 (ID22A) fundo la autoridad de nivel dibujo y la ejercio sobre **una sola propiedad**:
`selective.verticalClearance`. Esa unica propiedad se sostiene hoy sobre superficie **especifica de
ella** —controles de vinculo propios, y el camino de datos que los acompana—. El resultado verificable
de I-48 es que **vincular una propiedad deje de ser un caso especial**: que exista un mecanismo de
edicion vinculable **reutilizable**, y que `selective.verticalClearance` pase a ser **un consumidor
mas** de ese mecanismo en vez de su unica implementacion.

El objetivo **no** es anadir capacidad nueva de calculo ni cambiar la semantica que I-47 fijo. Es
mover la frontera entre *lo que es generico* y *lo que es de una propiedad concreta*.

## 2. Problema

El vertical slice de I-47 fue deliberadamente **uno**. La consecuencia es que hoy no se sabe —no
esta demostrado— **que parte** del camino `authored/effective`, del preflight, del ejecutor de
mutacion, de los intents y de la superficie WPF es **generica** y que parte esta **atada a una
propiedad concreta**. Mientras eso no se separe:

- vincular una segunda propiedad cuesta lo que costo la primera, o no se sabe cuanto cuesta;
- el coste no se puede estimar sin auditar el wiring, porque no hay contrato de reuso;
- cada propiedad nueva es una oportunidad de divergir de las asimetrias de I-47.

La direccion de UX registrada en [ideas-futuras.md](../ideas-futuras.md) («Project Variables —
Excel-like Property Input UX») describe este mismo problema y esta marcada **«futura, sin numero y
sin reclamar»**. I-48 es el acto de planificacion formal que la **numera y la reclama** —y por eso
esa entrada queda superada por esta iniciativa—. Que quede claro lo que eso **no** significa: la
UX tipo Excel es **direccion NO NORMATIVA** (asi la marca [HANDOFF](../HANDOFF.md) seccion 4) y
**este contrato no la impone**. Que forma toma la edicion vinculable lo decide la Proposal.

## 3. Alcance

1. **Discovery**: caracterizar el wiring vigente de `selective.verticalClearance` de punta a punta,
   por archivo y simbolo, y separar lo generico de lo especifico. Identificar los hotspots que
   habria que tocar para hacer vinculable una segunda propiedad.
2. **Proposal**: proponer el mecanismo de edicion vinculable reusable, con su contrato de reuso.
   Rige **NO IMPLEMENTATION BEFORE CONSENSUS** (seccion 12).
3. **Implementacion**, solo tras el consenso: el mecanismo reusable, y la **migracion de
   `selective.verticalClearance`** a el sin cambio de comportamiento observable.
4. **Proof of generality**: demostrar la generalidad con **al menos una segunda propiedad real**.
   Candidata inicial: **`selective.palletTolerance`**. «Candidata inicial» significa que es el punto
   de partida, no que este fijada: si el Discovery muestra que otra propiedad real prueba mejor la
   generalidad, la Proposal puede sustituirla **argumentando por que**. Lo que **no** es negociable
   es que la prueba se haga con una propiedad **real** del producto, no con una de laboratorio.

Se conservan **sin cambio** las reglas que I-47 fijo y [ADR-0034](../adr/0034-project-variables-autoridad-drawing-level.md)
acepto, en particular las tres asimetrias: vincular **congela** el literal y desvincular
**materializa** el efectivo actual; un vinculo roto **no** se desvincula y solo se repara con aviso
explicito; y un registro presente e ilegible **nunca** se lee como vacio. I-48 puede cambiar **por
donde** se ejerce esa semantica; **no** puede cambiar la semantica.

## 4. Fuera de alcance

- **Formulas (ID22B)**: ni parser, ni AST, ni expresiones, ni grafo de dependencias, ni deteccion de
  ciclos, ni persistencia de expresiones. Que un diseno futuro admita `=Holgura General + 2` es
  **direccion, no permiso**.
- **Referencias rack a rack (ID21)**: referirse a propiedades de **otro** rack sigue sin existir.
- **Generalizar otros sistemas**: I-48 no lleva variables vinculables al Dinamico, a Push Back, a la
  Cama de rodamiento ni a Cantilever. El alcance de sistema es el que I-47 dejo.
- **Reescribir editores completos**: no es una migracion de ventanas, ni un rediseno del Editor
  Shell, ni una reforma de los editores ricos. Se toca lo que la edicion vinculable exige.
- Migracion de dibujos existentes, unificacion de `ClearHeight`, y cualquier cambio de la
  persistencia de ID22A que no sea consecuencia forzosa del mecanismo acordado.
- Los hallazgos laterales del Discovery se registran en [ideas-futuras.md](../ideas-futuras.md);
  **no se arreglan aqui**.

## 5. Contexto requerido

- [ADR-0034](../adr/0034-project-variables-autoridad-drawing-level.md) — autoridad drawing-level.
  **Aceptado, y por tanto inmutable en su contenido**.
- Contrato de [I-47](I-47-project-variables-foundation.md) y sus Proposals V1..V4 — que se
  implemento y, sobre todo, que se dejo expresamente fuera.
- [HANDOFF](../HANDOFF.md) seccion 1 (las tres asimetrias) y seccion 4 (backlog abierto y la
  direccion de UX marcada **no normativa**).
- [ideas-futuras.md](../ideas-futuras.md): la entrada de UX tipo Excel y la **deuda conocida** de
  `ProjectVariablesDocument.ToProjectVariables()`, que hardcodea `Length` e ignora el `Type`
  persistido — relevante en cuanto I-48 toque mas de un tipo, y **heredada, no causada** por I-48.
- Context Packs declarados en el frontmatter.
- [AGENTS.md](../../AGENTS.md), convencion 3 (los limites que cruza un cambio de propiedad).

## 6. Dependencias

- **I-47 integrada y cerrada** (merge `507921f`): es la base sobre la que se generaliza. Sin ella
  esta iniciativa no tiene objeto.
- **Entrada del dueno requerida**: el consenso de la seccion 12 y la aprobacion de la Proposal.
- No se declaran estorbos: al reclamar, **ninguna otra iniciativa estaba en curso** —`origin` solo
  tenia `main`—. Si se abriera otra sobre el editor Selectivo o sobre variables de proyecto, se
  serializa.

## 7. Archivos esperados

**Este contrato no fija todavia la lista de archivos, y no debe fingir que si.** Enumerar el wiring
por archivo y simbolo **es el entregable del Discovery** (seccion 3, punto 1), y el Discovery no se
ha hecho. Fijar aqui una lista seria inventarla.

Lo que si se declara, como area y como limite: la iniciativa espera tocar la **superficie WPF de
edicion vinculable** y su reuso, el **camino de intents/DTO** que hoy sirve a la propiedad unica, y
la **frontera de Application** donde se resuelve `authored/effective`. Espera **no** tocar la
persistencia del registro ni el ejecutor de mutacion salvo consecuencia forzosa del mecanismo
acordado. **Una desviacion material frente a esto obliga a detenerse** y a volver a la Proposal.

## 8. Fases

| # | Fase | Entregable | Estado |
|---|---|---|---|
| G0 | Reclamo + bootstrap | Reclamo atomico publicado, contrato y fila en ROADMAP | **HECHA** |
| G1 | Discovery | Informe del wiring vigente por archivo/simbolo y hotspots de la segunda propiedad | pendiente |
| G2 | Proposal | Mecanismo reusable y contrato de reuso, congelado en una version | pendiente |
| G3 | Consenso | `Coordinator=AGREED` y `Architect=AGREED` sobre la MISMA Proposal | pendiente — **compuerta** |
| G4+ | Implementacion | Por definir **en la Proposal**, no aqui | bloqueada por G3 |

Ninguna fase posterior arranca sin que la anterior tenga evidencia revisable.

## 9. Pruebas y builds

Se fijan **en la Proposal**, cuando exista alcance de codigo. Lo que ya es exigible por norma y no
depende de la Proposal: las **dos suites** en local sobre el Candidato, **CI verde sobre el SHA
exacto**, y builds Debug de UI y de Plugin (AGENTS.md, «Pruebas — definicion de terminado»). G0 y G1
no producen codigo: su evidencia es documental.

## 10. Validacion manual

**Requerida** si la implementacion cambia comportamiento de dibujo —y migrar la edicion de una
propiedad vinculable puede cambiarlo—. El **checklist concreto se fija en la Proposal**; aqui solo
consta la obligacion. G0 y G1 **no** la requieren: no tocan producto.

## 11. Criterios de aceptacion

1. `selective.verticalClearance` se edita **por el mecanismo generico**, no por superficie propia, y
   su comportamiento observable **no cambia**.
2. Una **segunda propiedad real** —candidata inicial `selective.palletTolerance`— es vinculable **a
   traves del mismo mecanismo**, y el trabajo que costo anadirla es **notoriamente menor** que
   reimplementar el camino.
3. Las tres asimetrias de I-47 siguen valiendo, verificadas, sobre **ambas** propiedades.
4. No aparecen formulas, ni referencias rack a rack, ni variables vinculables en otros sistemas.
5. Existe un **contrato de reuso explicito**: que debe aportar una propiedad para volverse
   vinculable.

## 12. Condiciones para detenerse

- **COMPUERTA PRINCIPAL — NO IMPLEMENTATION BEFORE CONSENSUS.** La implementacion queda **bloqueada**
  hasta que **Coordinator y Architect esten `AGREED` sobre la MISMA Proposal**, sin desacuerdos
  abiertos. Un hallazgo del Discovery, por concluyente que parezca, **no es una autorizacion**.
- Si el Discovery muestra que la generalizacion exige tocar la persistencia de ID22A o la semantica
  de ADR-0034: **detenerse**. Eso es ADR nuevo y decision del dueno, no un ajuste de alcance.
- Si `selective.palletTolerance` resulta no ser buena prueba de generalidad: **no sustituirla en
  silencio**; proponer el cambio en la Proposal, con argumento.
- Si el alcance empieza a parecerse a reescribir un editor: **detenerse** (seccion 4).
- Si aparece otra iniciativa activa sobre el editor Selectivo o sobre variables de proyecto:
  serializar.

## 13. Estado versionado y entrega del Pull Request

Sin `docs/automation/state/I-48.yml`: la automatizacion esta **desactivada** (`automation.enabled:
false`) y el precedente inmediato —I-47, tambien manual— tampoco lo llevo.
[WORKFLOW](../WORKFLOW.md) seccion 8 exige para el bootstrap del caso (d) exactamente **fila en
ROADMAP mas contrato**, y **no** un archivo de estado. El estado vivo se deriva, como manda la
seccion 2, de la existencia de `origin/architecture/generic-linked-property-editing`.

No hay Pull Request. El merge automatico esta prohibido; la integracion es manual (WORKFLOW 4.5).

## 14. Evidencia final

**Reclamo atomico (G0).**

```text
Iniciativa = I-48 - Generic Linked Property Editing
Rama       = architecture/generic-linked-property-editing
Worktree   = ~/.codex/worktrees/architecture-generic-linked-property-editing
BASE_SHA   = e8ed2bcc3ad32b9418be3e98d26f3fcbfeee5918  (origin/main)
CLAIM_SHA  = a1692738732d7c77f8f1875e91f24cd6c0f1a9bc  (commit vacio)
Claim-Id   = 098dc10f-0f57-4376-8e5a-6787e9966c25
```

Primer `git push -u origin architecture/generic-linked-property-editing` **aceptado sin force**
(`* [new branch]`), con `origin` sin ninguna otra referencia a I-48 en el preflight. `main` **no fue
modificada**.

El resto de la evidencia se acumula al cerrar cada fase.
