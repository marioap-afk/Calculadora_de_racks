---
schema: rackcad-initiative/v1
id: I-51
title: "RACKDUPLICAR con multiples origenes"
type: feature
status: claimed
branch: feature/rackduplicar-multiples-origenes
base_branch: main
priority:
size:
depends_on: []
conflicts_with: []
context_packs: [autocad-plugin, persistence, architecture-kernel]
automation_state_path:
decision_paths: []
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

# RACKDUPLICAR con multiples origenes

> **Fase actual: G1 (Discovery) HECHO; G2 BLOQUEADO.** El informe
> [I-51-discovery.md](I-51-discovery.md) declara una **decision arquitectonica material** —revision de
> Arquitecto antes de G2— y siete decisiones de producto para el dueno. No hay contrato de autoridad
> (G2) y **no hay una sola linea de produccion escrita**. La sesion que abrio la iniciativa estaba
> autorizada **solo para G0 y G1**; la implementacion esta BLOQUEADA (seccion 12).

> **Apertura por autorizacion explicita del dueno sin fila previa** — caso (d) de
> [WORKFLOW](../WORKFLOW.md) seccion 2. Esa autorizacion sustituye **unicamente** la preexistencia de
> la fila en ROADMAP: la fila durable y este contrato nacen en el bootstrap inmediatamente posterior
> al reclamo atomico. La autorizacion es la **instruccion directa del dueno** que abrio la
> iniciativa; **no existe** `docs/automation/decisions/I-51.md` y la norma **no lo exige** para el
> caso (d), asi que `decision_paths` queda vacio en vez de apuntar a un archivo inventado.

## 1. Objetivo

Que `RACKDUPLICAR` pueda duplicar **varios origenes en una sola operacion**: una seleccion fisica de
varias referencias de rack —de uno o de varios racks, y de una o de varias vistas de un mismo rack—
hacia uno o varios puntos de destino, conservando lo que define al comando: **cada copia es un rack
independiente** del original.

El dueno fijo en la apertura dos exigencias que el diseno debe cumplir y que G1 debe poder sostener
sobre el codigo real:

1. **Identidad por rack origen, no por vista.** Todas las vistas hermanas copiadas de un mismo rack
   origen `A` comparten **UN** `RackId` nuevo, no uno por vista. Como se reparte esa regla entre
   **N racks × M puntos de destino** es pregunta del Discovery (seccion 3), no una respuesta de este
   contrato.
2. **Seleccion agrupada sin perdida.** La seleccion fisica multiple se **agrupa y deduplica por
   `RackId` sin perder las vistas realmente seleccionadas**.

## 2. Problema

El comportamiento **documentado** del comando es de un solo origen:

- [`docs/guias/despliegue.md`](../guias/despliegue.md), fila `RACKDUPLICAR`: copia **independiente**
  (GUID nuevo), al estilo `COPY` —punto base y puntos de destino—, **copia multiple por defecto**
  con `Unica` para una sola, nombre «… - copia N», y **«Duplica la vista clicada»**.
- [`docs/ideas-futuras.md`](../ideas-futuras.md), «Gestion de racks» #5: **decision del 2026-07-09**
  —duplicar **solo la vista clicada**, guardando el sistema completo en el embed, es el comportamiento
  deseado; no se duplican todas las vistas—.

Duplicar varios racks, o varias vistas de un mismo rack, exige hoy **una invocacion por origen**. Lo
que eso implica para la identidad de las vistas de una misma copia **no se afirma aqui**: es
hipotesis a verificar en G1 contra el codigo, junto con si la decision del 2026-07-09 describe
todavia lo que el codigo hace y si debe seguir gobernando una seleccion multiple.

## 3. Alcance

1. **G0 — reclamo y bootstrap**: reclamo atomico publicado, este contrato y la fila en ROADMAP.
2. **G1 — Discovery, solo documentacion.** Caracterizar con archivo, linea y evidencia:
   `RackDuplicarCommands`, la seleccion actual, las vistas hermanas, `RackBlockFinder`, clon y
   restamp, `RackId` exterior e interior, JSON authored, bindings de I-47/I-48, posicion de insercion
   y de referencia, rotacion/escala/capa, UCS→WCS, transacciones, modo Multiple/Unica y reutilizacion
   **real** de `RACKLAYOUT`. Y responder especificamente:
   - el flujo exacto actual seleccion → clon → restamp → traslado → commit;
   - si una vista seleccionada duplica historicamente **solo esa vista** o **todas sus hermanas**;
   - como representar una seleccion fisica multiple agrupada/deduplicada por `RackId` sin perder las
     vistas realmente seleccionadas;
   - como lograr que todas las hermanas copiadas de `A` compartan **UN** `RackId` nuevo;
   - si hace falta un mapa `OldRackId → NewRackId`;
   - la atomicidad recomendada y su alcance;
   - **N racks × M puntos de destino**;
   - los riesgos con copias enlazadas y con multiples referencias del mismo `RackId`;
   - los conflictos de archivos con **I-49** e **I-50**;
   - las piezas pequenas reutilizables, **sin crear un framework general**.
3. **G2 — contrato** de seleccion, asignacion de identidad, atomicidad y transformacion. Si la
   autoridad o la asignacion de `RackId`, las referencias o la transformacion multi-rack resultan una
   **decision arquitectonica material**, se declara explicitamente para **revision de Arquitecto
   antes de G2** (instruccion del dueno).
4. **Implementacion**: sus gates se definen en G2. **No esta autorizada.**

## 4. Fuera de alcance

- **Cualquier cambio de produccion en G0 y G1**: nada en `src/`, `tests/`, `assets/`, `eng/`,
  `deploy/` ni `.github/`.
- **Cambiar la semantica de independencia**: la copia de `RACKDUPLICAR` sigue siendo un rack
  independiente, y el `COPY` de AutoCAD —que conserva la misma definicion ([ARCHITECTURE](../ARCHITECTURE.md)
  §4.1)— y las copias enlazadas siguen compartiendo identidad con su origen. I-51 amplia **cuantos
  origenes** entran; no redefine que es una copia.
- **Modificar `RACKLAYOUT` o `RACKRELLENAR`**: su reutilizacion se caracteriza, no se cambia.
- **La semantica de Project Variables** ([ADR-0034](../adr/0034-project-variables-autoridad-drawing-level.md),
  I-47) y de la **edicion vinculable** (I-48): la duplicacion ya conserva el `VariableId` (decision
  C-4 de [I-47](../automation/decisions/I-47.md)) y eso no se reabre.
- **Territorio de iniciativas paralelas**: cotas por vista (I-50) y lo que el contrato de I-50 atribuye
  a I-49 (`LinkedPropertyEditor`, Expression Engine, Project Variables).
- Migracion de dibujos, BOM, geometria de sistemas y cualquier **framework general** de seleccion o de
  transformacion.
- Los hallazgos laterales del Discovery se registran en [ideas-futuras.md](../ideas-futuras.md);
  **no se arreglan aqui**.

## 5. Contexto requerido

- [ideas-futuras.md](../ideas-futuras.md), «Gestion de racks» #5 (decision del 2026-07-09, y su nota
  de usar el comando como base del layout de almacen, #4).
- [`docs/guias/despliegue.md`](../guias/despliegue.md), tabla de comandos.
- [ARCHITECTURE.md](../ARCHITECTURE.md), identidad del rack (GUID, copias de definicion,
  `RACKDUPLICAR`).
- Contrato de [I-10](I-10-kind-handlers.md) (restamp de copias independientes por handler, consumido
  por `RACKDUPLICAR` y `RACKLAYOUT`) y de [I-11](I-11-persistencia-uniforme.md) (duplicacion con
  campos desconocidos preservados).
- Decisiones de [I-47](../automation/decisions/I-47.md) (C-4, C2-7), ADR-0034 y la Proposal V8 de
  [I-48](I-48-proposal-v8.md).
- [AGENTS.md](../../AGENTS.md), convencion 6 (regen **unico** en ediciones multi-vista).
- Context Packs declarados en el frontmatter.

## 6. Dependencias

- **Sin dependencias pendientes**: todo lo que el comando consume hoy esta integrado.
- **Iniciativas paralelas a vigilar.** Al reclamar, `origin` tenia `main` y
  `feature/cotas-independientes-por-vista` (**I-50**, en curso). **I-49** no tenia rama remota; la
  cita el contrato de I-50 como paralela. `conflicts_with` queda **vacio** hasta que G1 mida el cruce
  real de archivos: no se declara un estorbo sin evidencia, ni se omite uno que la tenga.
- **Entrada del dueno**: puede hacer falta si G1 encuentra ambiguedad de producto —p. ej. si la
  decision del 2026-07-09 sigue gobernando una seleccion multiple—. Por eso
  `requires_owner_decision` queda **vacio** hasta G1, en vez de fijarse por analogia.

## 7. Archivos esperados

**Este contrato no fija todavia la lista de archivos, y no debe fingir que si.** El mapa por archivo
y simbolo **es el entregable de G1**.

Lo que si se declara, como area y como limite: el comando `RACKDUPLICAR` del Plugin y sus ayudantes
de busqueda, clon y restamp; y, si el Discovery lo justifica, una pieza **pura** en Application. Se
espera **no** tocar persistencia, BOM, geometria de sistemas, editores ni `RACKLAYOUT`. **Una
desviacion material frente a esto obliga a detenerse** y a volver a G2.

## 8. Fases

| # | Fase | Entregable | Estado |
|---|---|---|---|
| G0 | Reclamo + bootstrap | Reclamo atomico publicado, contrato y fila en ROADMAP | **HECHA** |
| G1 | Discovery | [I-51-discovery.md](I-51-discovery.md): informe por archivo/linea y respuestas de la seccion 3, punto 2 | **HECHA** — declara decision arquitectonica material (AM-1 a AM-3; AM-4 condicionada a I-50) |
| G2 | Contrato | Seleccion, identidad, atomicidad y transformacion; revision de Arquitecto si hay decision arquitectonica material | pendiente — **bloqueada** hasta la revision de Arquitecto y las decisiones PD-1 a PD-7 del dueno |
| G3+ | Implementacion, Candidato, validacion del dueno, integracion | Por definir **en G2**, no aqui | bloqueada |

Ninguna fase posterior arranca sin que la anterior tenga evidencia revisable.

## 9. Pruebas y builds

Se fijan **en G2**, cuando exista alcance de codigo. Lo que ya es exigible por norma y no depende de
G2: las **dos suites** en local sobre el Candidato, **CI verde sobre el SHA exacto**, y builds Debug
de UI y de Plugin (AGENTS.md, «Pruebas — definicion de terminado»). G0 y G1 no producen codigo: su
evidencia es documental.

## 10. Validacion manual

**Requerida** en la implementacion: `RACKDUPLICAR` es un comando interactivo de AutoCAD y cambiar que
origenes acepta cambia comportamiento de dibujo (AGENTS.md, punto 5). El **checklist concreto se fija
en G2**. G0 y G1 **no** la requieren: no tocan producto.

## 11. Criterios de aceptacion

Preliminares; **los fija G2**. Solo recogen lo que el dueno enuncio y lo que el comando ya garantiza:

1. Una sola invocacion de `RACKDUPLICAR` acepta **varios origenes**.
2. Las vistas hermanas copiadas de un mismo rack origen comparten **un unico `RackId` nuevo**, y
   ninguna copia comparte `RackId` con su origen.
3. La agrupacion y la deduplicacion por `RackId` **no pierden** ninguna vista realmente seleccionada.
4. El original y sus copias enlazadas quedan **intactos**; editar una copia no toca al original.
5. Se conserva lo que `RACKDUPLICAR` ya garantiza: el `VariableId` de los bindings (C-4 de I-47) y los
   campos desconocidos del documento (I-11).

## 12. Condiciones para detenerse

- **COMPUERTA DE ESTA SESION — G0 y G1 solamente.** Ninguna linea de produccion. G3+ queda bloqueado
  hasta cerrar G2.
- Si la autoridad o la asignacion de `RackId`, las referencias o la transformacion multi-rack exigen
  una **decision arquitectonica material**: **declararla explicitamente para revision de Arquitecto
  antes de G2**.
- Si hay **ambiguedad de producto** —p. ej. vista seleccionada frente a todas sus hermanas, contra la
  decision del 2026-07-09—: la decide el dueno, no el Discovery.
- Si G1 encuentra **archivos productivos compartidos materiales con I-49 o I-50**: reportarlo antes de
  continuar.
- Si el cambio exige tocar el formato persistido o la semantica de identidad del rack: **detenerse**;
  eso es ADR y decision del dueno.
- Si el alcance deriva hacia rediseñar `RACKLAYOUT` o hacia un framework general: **detenerse**
  (seccion 4).

## 13. Estado versionado y entrega del Pull Request

Sin `docs/automation/state/I-51.yml`: la automatizacion esta **desactivada** (`automation.enabled:
false`) y los precedentes inmediatos —I-47 e I-48, tambien manuales— tampoco lo llevaron.
[WORKFLOW](../WORKFLOW.md) seccion 8 exige para el bootstrap del caso (d) exactamente **fila en
ROADMAP mas contrato**, y **no** un archivo de estado. El estado vivo se deriva, como manda la
seccion 2, de la existencia de `origin/feature/rackduplicar-multiples-origenes`.

No hay Pull Request. El merge automatico esta prohibido; la integracion es manual (WORKFLOW 4.5).

## 14. Evidencia final

**Reclamo atomico (G0).**

```text
Iniciativa = I-51 - RACKDUPLICAR con multiples origenes
Rama       = feature/rackduplicar-multiples-origenes
Worktree   = ~/.codex/worktrees/feature-rackduplicar-multiples-origenes
BASE_SHA   = a4d88f18a1f42263d366c44dc05dd18a6786f152  (origin/main)
CLAIM_SHA  = 3ffd2ca21778b29bd5ccac2e6d171971769ad8ca  (commit vacio)
Claim-Id   = ad4b9e47-63c0-48c3-9406-9ae2db8a121a
```

Primer `git push -u origin feature/rackduplicar-multiples-origenes` **aceptado sin force**
(`* [new branch]`), con `origin` sin ninguna otra referencia a I-51 en el preflight. `main` **no fue
modificada**.

El resto de la evidencia se acumula al cerrar cada fase.
