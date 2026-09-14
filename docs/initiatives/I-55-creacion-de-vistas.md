---
schema: rackcad-initiative/v1
id: I-55
title: "Creacion de vistas de rack en todos los sistemas"
type: feature
status: claimed
branch: feature/creacion-de-vistas
base_branch: main
priority:
size:
depends_on: []
conflicts_with: []
context_packs: [autocad-plugin, ui-editors, persistence, architecture-kernel, system-selective, system-dynamic-flowbed]
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

# I-55 — Creacion de vistas de rack en todos los sistemas

> **Fase actual: G1 CERRADA — Discovery publicado** en [I-55-discovery.md](I-55-discovery.md). No hay Proposal y
> **no hay una sola linea de produccion escrita**. La sesion que abrio la iniciativa estaba autorizada **solo para
> G0 y G1**; la implementacion esta BLOQUEADA (seccion 12) y G2 requiere orden propia.
>
> ```text
> SUBSTANTIVE IMPLEMENTATION: BLOCKED
> Coordinator: NOT YET AGREED
> Architect:   NOT YET AGREED
> ```

```text
Initiative    = I-55
Owner ID      = NO FIJADO por la orden de apertura (seccion 2; Discovery §20, OQ-1)
Branch        = feature/creacion-de-vistas
Worktree      = ~/.codex/worktrees/feature-creacion-de-vistas
BASE_SHA      = ba497f14581d81e83a27514852d6ec082ff57635   (origin/main, merge de I-54)
CLAIM_SHA     = 24bb9cad77d945fecc00ec41f46ba0d0a7abb1de   (Claim-Id f96a4b1f-40ff-40d0-b3cf-e2cfad778cc7)
BOOTSTRAP_SHA = 5f774b459eb8b13732e4ae5c4e93a909874a10c5
DISCOVERY     = docs/initiatives/I-55-discovery.md          (su SHA se registra al abrir G2)
```

> **Apertura por autorizacion explicita sin fila previa** — caso (d) de [WORKFLOW](../WORKFLOW.md) seccion 2.
> La autorizacion es la **orden directa** recibida en la sesion de apertura («I-55 — G0/G1 solamente»). Esa
> autorizacion sustituye **unicamente** la preexistencia de la fila en ROADMAP: la fila durable y este contrato
> nacen en el bootstrap inmediatamente posterior al reclamo atomico. **No existe**
> `docs/automation/decisions/I-55.md` y el caso (d) **no lo exige**, asi que `decision_paths` queda vacio en vez
> de apuntar a un archivo inventado.

## 0. Gobierno de la iniciativa

```text
Owner → Coordinador I-55 ↔ Arquitecto I-55 → consenso → ejecutor
```

- El ejecutor **no cierra** decisiones de producto ni de arquitectura por su cuenta.
- **Ninguna implementacion sustantiva** antes de que Coordinador **y** Arquitecto declaren `AGREED` sobre la
  **misma** Proposal. La orden de apertura lo fija asi: «Proposal y consenso Coordinator ↔ Architect vienen
  despues».
- Las decisiones se versionaran en `docs/automation/decisions/I-55.md` **cuando existan**; hoy no existe
  ninguna.

## 1. Objetivo

La orden de apertura **no fija un resultado de producto**: fija el **objeto del Discovery**. Por eso este
contrato no enuncia todavia que debe poder hacer el usuario al terminar I-55; eso lo fija el consenso de G2.

El resultado verificable de la sesion autorizada es la **evidencia** de como se **crean hoy las vistas de un
rack** —la primera y las enlazadas— en **todos los sistemas**, publicada como `I-55-discovery.md` con la
matriz **sistema × ViewKind × variante** y evidencia `archivo:linea`, sobre la que Coordinador y Arquitecto
puedan converger antes de escribir codigo.

## 2. Problema

**Lo documentado hoy**, sin afirmarlo sobre el codigo (eso lo verifica G1):

- [ARCHITECTURE](../ARCHITECTURE.md) §4.1: «Las vistas adicionales solo se insertan desde un rack existente»,
  para evitar laterales o plantas sin diseno fuente.
- [ADR-0009](../adr/0009-identidad-guid-embebida-en-dwg.md): el GUID del rack se crea al insertar el rack, se
  conserva al actualizarlo y lo comparten sus vistas; `View` y `Section` describen la representacion; toda
  operacion que cree un rack logico independiente asigna identidad nueva.
- [ADR-0010](../adr/0010-actualizar-redibuja-insertar-liga-vistas.md): Actualizar redibuja en sitio e Insertar
  agrega una vista ligada; **cada editor expone solo las vistas que implementa**, con restricciones propias
  entre la insercion inicial y la edicion, y la cama de rodamiento conserva una sola vista lateral.

**Lo que la orden no fija.** La orden nombra la iniciativa y el objeto de su Discovery, pero **no** su titulo,
su rama ni un ID del Owner. El titulo y la rama de este contrato se nombran **por ese objeto** y no afirman
nada mas. Consta, como evidencia documental y no como correspondencia, que el contrato de I-50 (§7) declaro
fuera de su alcance tres IDs del Owner cuyo texto **no esta versionado**: «ID17 first-view freedom», «ID18
multi-view queue» e «ID19 multi-rack projection». **Si I-55 corresponde a alguno de ellos, a varios o a
ninguno no se afirma aqui**: es pregunta abierta para el Owner, via Coordinador, antes de G2.

## 3. Alcance

1. **G0 — reclamo y bootstrap**: reclamo atomico publicado, este contrato y la fila en ROADMAP.
2. **G1 — Discovery, solo documentacion**, sobre el codigo de `BASE_SHA` (la rama no difiere de el en `src/`,
   `tests/` ni `assets/`). Auditar por archivo y simbolo, en **todos los sistemas** que declare el registro:
   - **creacion**: insercion inicial de un rack y de cada una de sus vistas, y quien decide la primera vista;
   - **vistas enlazadas**: insercion de una vista ligada a un rack existente y actualizacion de las hermanas;
   - **identidad**: `RackId`, `View` y `Section`, y como se asignan, conservan o renuevan;
   - **`DimensionViewKind`**: la correspondencia entre la vista dibujada y su tipo de vista de cotas;
   - **`RequestDraw`**: la peticion de dibujo de cada editor y su contrato con el Plugin;
   - **materializacion**: del plan resuelto a la definicion de bloque y a la referencia colocada;
   - **authored / effective**: que valor viaja a una vista nueva;
   - **`DimensionViews`** (I-50, [ADR-0035](../adr/0035-visibilidad-de-cotas-por-tipo-de-vista.md));
   - **`CustomProperties`** (I-54, [ADR-0039](../adr/0039-custom-properties-persistencia-autoridad.md));
   - **`ProjectVariables`** (I-47/I-48, [ADR-0034](../adr/0034-project-variables-autoridad-drawing-level.md));
   - **BOM y listado**: consolidacion por identidad y conteo de vistas y copias;
   - **`RACKLAYOUT`** (y lo que comparta con `RACKRELLENAR`);
   - **I-51**: la duplicacion multiorigen y sus vistas hermanas;
   - **semantica de cancelacion**: que queda en el dibujo, en la sesion del editor y en la identidad cuando el
     usuario cancela en cada punto del flujo.

   Entregable: [`I-55-discovery.md`](I-55-discovery.md) con la matriz **sistema × ViewKind × variante** y
   evidencia `archivo:linea`.
3. **G2 — Proposal y consenso**: despues, con **orden propia**. **No autorizada** en esta sesion.
4. **Implementacion**: sus gates se definen tras el consenso de G2. **No esta autorizada.**

## 4. Fuera de alcance

- **Cualquier cambio de produccion en G0 y G1**: nada en `src/`, `tests/`, `assets/`, `eng/`, `deploy/`,
  `tools/` ni `.github/`.
- `docs/HANDOFF.md`: prohibido fuera de la integracion ([WORKFLOW](../WORKFLOW.md) seccion 2).
- **Reabrir decisiones aceptadas** —[ADR-0009](../adr/0009-identidad-guid-embebida-en-dwg.md),
  [ADR-0010](../adr/0010-actualizar-redibuja-insertar-liga-vistas.md),
  [ADR-0034](../adr/0034-project-variables-autoridad-drawing-level.md),
  [ADR-0035](../adr/0035-visibilidad-de-cotas-por-tipo-de-vista.md) y
  [ADR-0039](../adr/0039-custom-properties-persistencia-autoridad.md)— o la semantica de `RACKDUPLICAR` que
  entrego I-51. El Discovery las **describe**; cambiarlas exigiria ADR nuevo y decision del Owner.
- **Modificar `RACKLAYOUT` o `RACKRELLENAR`**: se auditan, no se tocan.
- **El territorio de las iniciativas activas**: I-49 (motor de expresiones), I-52 (`RACKMIRROR`) e I-53D (UI
  de cabeceras del Dinamico).
- Los hallazgos laterales se registran en el Discovery —y, al fijar el contrato, en
  [ideas-futuras.md](../ideas-futuras.md)—; **no se arreglan aqui**.

## 5. Contexto requerido

- [ARCHITECTURE.md](../ARCHITECTURE.md) §4.1 (identidad, vistas y round-trip).
- [ADR-0009](../adr/0009-identidad-guid-embebida-en-dwg.md), [ADR-0010](../adr/0010-actualizar-redibuja-insertar-liga-vistas.md),
  [ADR-0034](../adr/0034-project-variables-autoridad-drawing-level.md),
  [ADR-0035](../adr/0035-visibilidad-de-cotas-por-tipo-de-vista.md) y
  [ADR-0039](../adr/0039-custom-properties-persistencia-autoridad.md).
- Contratos de [I-50](I-50-cotas-independientes-por-vista.md), [I-51](I-51-rackduplicar-multiples-origenes.md) e
  [I-54](I-54-propiedades-personalizadas.md), y el de [I-11](I-11-persistencia-uniforme.md) (sobre y vista nueva).
- Contratos de I-49, I-52 e I-53D **en sus ramas remotas**, solo lectura y en el SHA exacto que se cite.
- [AGENTS.md](../../AGENTS.md): capas, persistencia versionada, pruebas y evidencia.
- Context Packs declarados en el frontmatter. Push Back y Cantilever no tienen pack propio: se leen desde el
  registro de sistemas y sus subarboles.

## 6. Dependencias

- **Sin dependencias pendientes**: I-47, I-48, I-50, I-51 e I-54 estan integradas.
- **Iniciativas paralelas a vigilar.** Al reclamar, `origin` tenia `main` (`ba497f1`), **I-49**
  (`architecture/motor-expresiones-parametricas` @ `5f969cc`, produccion en `Application/Expressions` y
  `Units`), **I-52** (`feature/rackmirror-espejo-semantico` @ `deb08cd`, solo documentacion) e **I-53D**
  (`feature/cabeceras-multidestino-dinamico` @ `a57bd50`, produccion en la ventana y el ensamblador del
  Dinamico). `conflicts_with` queda **vacio** hasta que G1 mida el cruce real de archivos: no se declara un
  estorbo sin evidencia, ni se omite uno que la tenga.
- **Cruce medido en G1** ([Discovery](I-55-discovery.md) §2 y §19): **0** archivos con I-49 y con I-52, y **2** con
  I-53D —`RackDynamicSystemWindow.xaml(.cs)`, archivo caliente que contiene la puerta de primera vista del
  Dinamico—. I-49 preve tocar en su G10 la ventana del Selectivo (`docs/ROADMAP.md:471`) e I-52 cruza en la
  **semantica** de materializacion (`MaterializationContext`). `conflicts_with` sigue **vacio** porque I-55 aun no
  tiene archivos de produccion: lo fija G2 re-midiendo este cruce.
- **Entrada del Owner**: `requires_owner_decision` pasa a `true` en G1 con evidencia: la orden no fija el resultado
  de producto (seccion 2) y el codigo no determina las preguntas OQ-1..OQ-7 del Discovery (§20), que se escalan
  **antes** de G2.

## 7. Archivos esperados

**Este contrato no fija la lista de archivos de produccion, y no debe fingir que si.** El mapa por archivo y
simbolo **es el entregable de G1**, y los archivos de produccion los fija el consenso de G2.

Lo que si se declara para esta sesion: solo `docs/initiatives/I-55-*.md` y la fila de I-55 en
`docs/ROADMAP.md`. **Una desviacion material frente a esto obliga a detenerse.**

## 8. Fases

| # | Fase | Entregable | Estado |
|---|---|---|---|
| G0 | Reclamo + bootstrap | Reclamo atomico publicado, contrato y fila en ROADMAP | **HECHA** |
| G1 | Discovery | `I-55-discovery.md`: matriz sistema × ViewKind × variante y respuestas de la seccion 3, punto 2 | **HECHA** — [I-55-discovery.md](I-55-discovery.md) |
| G2 | Proposal y consenso | Proposal, revision de Arquitecto y reconciliacion, cada una con orden propia | pendiente |
| G3+ | Implementacion, Candidato, validacion del Owner, integracion | Por definir **tras el consenso de G2**, no aqui | bloqueada |

Ninguna fase posterior arranca sin que la anterior tenga evidencia revisable.

## 9. Pruebas y builds

Se fijan **tras el consenso de G2**, cuando exista alcance de codigo. Lo que ya es exigible por norma y no
depende de G2: las **dos suites** en local sobre el Candidato, **CI verde sobre el SHA exacto**, y builds Debug
de UI y de Plugin (AGENTS.md, «Pruebas — definicion de terminado»). G0 y G1 no producen codigo: su evidencia
es documental.

## 10. Validacion manual

**Requerida** en la implementacion: crear o insertar vistas de un rack es comportamiento de dibujo, y solo el
Plugin toca AutoCAD (AGENTS.md, punto 5). Por eso `requires_plugin_build`, `requires_autocad` y
`requires_owner_validation` quedan en `true` desde el bootstrap: esa metadata es monotonica y solo puede anadir
obligaciones. El **checklist concreto se fija tras el consenso de G2**. G0 y G1 **no** la requieren: no tocan
producto.

## 11. Criterios de aceptacion

Preliminares; **los fija el consenso de G2**. Solo recogen doctrina aceptada que cualquier resultado debe
respetar mientras no la reemplace un ADR nuevo:

1. Las vistas de un mismo rack logico comparten su `RackId`; una operacion que cree un rack logico independiente
   asigna identidad nueva ([ADR-0009](../adr/0009-identidad-guid-embebida-en-dwg.md)).
2. Actualizar redibuja en sitio sin crear otro rack; insertar una vista la liga al rack existente
   ([ADR-0010](../adr/0010-actualizar-redibuja-insertar-liga-vistas.md)).
3. El BOM total y el listado no cuentan una vista ligada como un rack distinto
   ([ADR-0009](../adr/0009-identidad-guid-embebida-en-dwg.md), consecuencias).
4. Lo que ya viaja con el rack a una vista nueva —politica de cotas por tipo de vista, propiedades
   personalizadas y vinculos a variables de proyecto— sigue viajando, en los terminos de ADR-0035, ADR-0039 y
   ADR-0034.

## 12. Condiciones para detenerse

- **COMPUERTA DE ESTA SESION — G0 y G1, solo documentacion.** Ninguna linea de produccion; G2 y G3+ no se
  inician.
- **Implementacion bloqueada** hasta `Coordinator = AGREED` y `Architect = AGREED` sobre la **misma** Proposal.
- Si G1 encuentra **archivos productivos compartidos materiales con I-49, I-52 o I-53D**: reportarlo antes de
  continuar.
- Si el Discovery revela una **ambiguedad de producto** que el codigo y los contratos no resuelven: se escala
  al Owner antes de G2.
- Si revela una **decision arquitectonica material** (identidad, formato persistido, contrato de
  materializacion): se declara para **revision de Arquitecto antes de G2**.
- Si el alcance deriva hacia reabrir un ADR aceptado o hacia el territorio de una iniciativa activa:
  **detenerse** (seccion 4).
- `docs/HANDOFF.md` **no se toca**.

## 13. Estado versionado y entrega del Pull Request

Sin `docs/automation/state/I-55.yml`: la automatizacion esta **desactivada** (`automation.enabled: false`) y el
caso (d) no lo exige. [WORKFLOW](../WORKFLOW.md) seccion 8 pide para el bootstrap del caso (d) exactamente **fila
en ROADMAP mas contrato**. El estado vivo se deriva, como manda la seccion 2, de la existencia de
`origin/feature/creacion-de-vistas`.

No hay Pull Request. El merge automatico esta prohibido; la integracion es manual (WORKFLOW 4.5).

## 14. Evidencia final

**Reclamo atomico (G0).**

```text
Iniciativa = I-55 - Creacion de vistas de rack en todos los sistemas
Rama       = feature/creacion-de-vistas
Worktree   = ~/.codex/worktrees/feature-creacion-de-vistas
BASE_SHA   = ba497f14581d81e83a27514852d6ec082ff57635  (origin/main)
CLAIM_SHA  = 24bb9cad77d945fecc00ec41f46ba0d0a7abb1de  (commit vacio)
Claim-Id   = f96a4b1f-40ff-40d0-b3cf-e2cfad778cc7
```

Primer `git push -u origin feature/creacion-de-vistas` **aceptado sin force** (`* [new branch]`), con `origin`
sin ninguna referencia a I-55 en el preflight, repetido justo antes del reclamo. `main` **no fue modificada**.

**Bootstrap (G0).** `BOOTSTRAP_SHA = 5f774b459eb8b13732e4ae5c4e93a909874a10c5`: este contrato y la fila de I-55 en
`docs/ROADMAP.md`; solo documentacion; empujado sin force.

**Discovery (G1).** [I-55-discovery.md](I-55-discovery.md): matriz sistema × ViewKind × variante (§5), creacion y
primera vista (§6), vistas enlazadas (§7), contrato `RequestDraw` (§8), materializacion (§9), authored/effective,
`DimensionViews`, `CustomProperties` y `ProjectVariables` (§10-13), BOM y listado (§14), `RACKLAYOUT` (§15), I-51
(§16), cancelacion (§17), pruebas (§18), cruces (§19), preguntas abiertas OQ-1..OQ-7 y A-1..A-4 (§20) y hallazgos
H-01..H-12 (§21). Solo documentacion: el commit toca unicamente `docs/initiatives/I-55-*.md`; `ROADMAP` y `HANDOFF`
no se tocan en G1.

El resto de la evidencia se acumula al cerrar cada fase.
