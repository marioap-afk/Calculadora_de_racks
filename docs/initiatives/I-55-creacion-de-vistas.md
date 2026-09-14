---
schema: rackcad-initiative/v1
id: I-55
title: "View Placement & Projection (ID17 + ID18 + ID19)"
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
decision_paths: [docs/automation/decisions/I-55.md]
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

# I-55 — View Placement & Projection

> **Fase actual: G2E — Proposal V4 en revision** ([Proposal V4](I-55-proposal-v4.md) @ `fe70d7a`).
>
> - **Proposal V3** (`8e35a51`): el Coordinador decidio CQ-01 (redibujo atomico de hermanas en Insertar) → Proposal V4.
> - **Proposal V4:** Coordinator = `REVIEW REQUIRED` ([paquete](I-55-coordinator-review-package-v4.md)); Architect formal = `PENDING`
>   ([paquete para un Arquitecto independiente](I-55-architect-review-package-v4.md)); Owner = `PENDING`.
> - **Open Material:** M-01 (decision del Owner pendiente) y adopcion de X-1..X-8 por I-52 (Proposal V4 §17.2.1).
>
> No hay una sola linea de produccion escrita; la implementacion esta **BLOQUEADA** (seccion 12).
>
> ```text
> SUBSTANTIVE IMPLEMENTATION: BLOCKED
> Coordinator: REVIEW REQUIRED
> Architect:   PENDING (formal, independiente)
> Owner:       PENDING
> Consensus:   NOT REACHED
> ```

```text
Initiative     = I-55 — View Placement & Projection
Owner IDs      = ID17 FIRST-VIEW FREEDOM · ID18 MULTI-VIEW QUEUE / BATCH · ID19 MULTI-RACK PROJECTION
Branch         = feature/creacion-de-vistas   (nombre del bootstrap; se conserva por decision CD-05)
Worktree       = ~/.codex/worktrees/feature-creacion-de-vistas
BASE_SHA       = ba497f14581d81e83a27514852d6ec082ff57635   (base original: merge de I-54)
CURRENT_BASE   = dad4e77f4f267b9fa248ecb0c8bfd8a74bbab093   (merge de I-53D; rebase al abrir G1.1)

                 original (sobre ba497f1)                    rebasado (sobre dad4e77)
CLAIM_SHA      = 24bb9cad77d945fecc00ec41f46ba0d0a7abb1de   6976262c6376fbda0c5b68afa2c3d3bdb95e8dc6
BOOTSTRAP_SHA  = 5f774b459eb8b13732e4ae5c4e93a909874a10c5   5b5c4f6624bbd2d58d8419de071493d3735a6230
DISCOVERY_SHA  = cf207b95020dbeb5a5164184b83dddb6658278e5   2d0f9bf03e70b8411ef4104b06609c542c5d71ff
G1_CLOSE_SHA   = 96f6444f12b609e4ac009c8975b63cdc2c993338   717a38d2cc5b809cbcaab1e2748ae150070a85bf

Claim-Id       = f96a4b1f-40ff-40d0-b3cf-e2cfad778cc7
Archivo        = tag archive/i-55-creacion-de-vistas-pre-rebase-96f6444 (punta previa al rebase)
Decisiones     = docs/automation/decisions/I-55.md
```

> **Correccion G1.1 (2026-09-13).** G0 y G1 afirmaron que la orden de apertura no fijaba un resultado de producto ni
> un ID del Owner. **Esa afirmacion era incorrecta y queda retirada.** El Coordinador detecto la contradiccion y la
> resolvio con caracter vinculante (decisiones CD-01..CD-09 en
> [`docs/automation/decisions/I-55.md`](../automation/decisions/I-55.md)). La evidencia tecnica del
> [Discovery](I-55-discovery.md) no cambia; cambia su lectura de producto.

## 0. Gobierno de la iniciativa

```text
Owner → Coordinador I-55 ↔ Arquitecto I-55 → consenso congelado → implementador
```

- El ejecutor **no sustituye** al Coordinador ni al Arquitecto, **no declara** sus veredictos y **no declara**
  consenso si ambos no lo emitieron sobre la **misma** version.
- **NO SUBSTANTIVE IMPLEMENTATION BEFORE `Coordinator = AGREED` AND `Architect = AGREED` ON THE SAME PLAN VERSION.**
- Hasta entonces se admite todo trabajo documental, de analisis, caracterizacion, diseno y planificacion que no cambie
  comportamiento productivo.

## 1. Objetivo

I-55 entrega **tres capacidades de producto** que forman **una sola iniciativa** porque comparten **una sola
infraestructura: preparar una representacion de vista antes de materializarla en AutoCAD**.

- **ID17 — FIRST-VIEW FREEDOM.** Crear un rack comenzando por **cualquier vista que ese sistema realmente soporte**.
  Ejemplo valido: crear un Selectivo desde **Planta** y despues agregar Frontal y Lateral **sin perder `RackId`, datos
  authored ni coherencia**.
- **ID18 — MULTI-VIEW QUEUE / BATCH.** Elegir **varias vistas de UN mismo rack en un flujo** y colocarlas
  consecutivamente: Frontal → colocar → Lateral → colocar → Planta → colocar. **Todas comparten el mismo `RackId`**.
- **ID19 — MULTI-RACK PROJECTION.** Seleccionar **varios racks ya existentes**, pedir **una misma clase de vista**,
  generarla para todos y **preservar su layout relativo mediante UNA transformacion comun**. **Cada rack conserva SU
  `RackId`**: **no** es `RACKDUPLICAR`, **no** hay `NewRackId` y **no** es un re-estampado de copia independiente.

**Resultado verificable** (tras consenso e implementacion): las tres capacidades en todos los sistemas vigentes que
dibujan vistas, sobre una frontera de **preparacion de vistas** verificable sin AutoCAD, con los invariantes de
identidad, authored, metadatos y conteo de la seccion 11.

## 2. Problema (evidencia de G1)

El [Discovery](I-55-discovery.md) mide la linea base sobre el codigo:

- **Primera vista restringida** en Selectivo (solo frontal), Dinamico (solo lateral) y Cabecera (solo lateral); libre
  en Push Back y Cantilever (Discovery §6.2). **ID17** elimina esa restriccion por sistema, sin exigir uniformidad.
- **Una vista por gesto**: toda ventana cierra tras una insercion; no hay cola ni lote (Discovery §6.3). **ID18** la
  sustituye por un flujo de varias vistas del mismo rack.
- **No existe proyeccion multi-rack**: las rutas de copia existentes (`RACKDUPLICAR`, `RACKLAYOUT` independiente) crean
  identidad nueva o solo replican la planta (Discovery §15-16). **ID19** crea vistas **de los mismos racks**.
- **Riesgos de base** que la foundation debe cerrar: la vista enlazada copia el sobre de la vista elegida sin comparar
  hermanas (Discovery §7, §12); la variante se elige a veces en la ventana y a veces en el Plugin (§5); y **H-01**
  demuestra el peligro de usar un indice de UI como contrato de `Section` (§21).

## 3. Alcance

1. **G0 — reclamo y bootstrap**: HECHO.
2. **G1 — Discovery**: HECHO; su evidencia sigue vigente (§2.1 del Discovery registra la re-medicion tras el rebase).
3. **G1.1 — correccion contractual**: framing, Owner IDs, objetivo y alcance; reclasificacion de OQ-1..OQ-7; fila
   propia de ROADMAP; rebase sobre `dad4e77` y reconciliacion de la evidencia del Dinamico. Solo documentacion.
4. **G2 — Proposal y consenso**: Proposal V1 (NOT CONSENSUS; Coordinator = CHANGES REQUIRED), en G2B Proposal V2
   (NOT CONSENSUS; Coordinator = CHANGES REQUIRED → V3), en G2D Proposal V3 (NOT CONSENSUS; decision CQ-01 → V4) y, en G2E, Proposal V4
   (NOT CONSENSUS), ADR propuesto si corresponde, mapa de implementacion, diseno de pruebas y de validacion del Owner, y
   paquetes de revision para Coordinador y Arquitecto. Autorizado como documentacion; el consenso lo emiten Coordinador y
   Arquitecto.
5. **Implementacion** de ID17, ID18 e ID19 sobre la foundation acordada, por los gates que fije el consenso.
   **BLOQUEADA.**

## 4. Fuera de alcance

- **Produccion antes del consenso**: nada en `src/`, `tests/`, `assets/`, `eng/`, `deploy/`, `tools/` ni `.github/`.
- `docs/HANDOFF.md` fuera de la integracion ([WORKFLOW](../WORKFLOW.md) §2).
- **Corregir H-01..H-12 incidentalmente** (CD-07). Si uno bloquea directamente el contrato, se aisla como prerrequisito
  justificado antes de tocar produccion.
- **Unicidad incidental de `(RackId, View, Section)`** (OQ-4): se preserva el comportamiento legacy.
- **Geometria nueva**: los builders por sistema siguen siendo la autoridad geometrica.
- **Drive-In**: no existe como sistema productivo; no se inventa.
- **Cambiar la semantica** de `RACKDUPLICAR` (I-51), `RACKLAYOUT` o `RACKRELLENAR`.
- **Reabrir ADR aceptados** ([ADR-0009](../adr/0009-identidad-guid-embebida-en-dwg.md),
  [ADR-0010](../adr/0010-actualizar-redibuja-insertar-liga-vistas.md),
  [ADR-0034](../adr/0034-project-variables-autoridad-drawing-level.md),
  [ADR-0035](../adr/0035-visibilidad-de-cotas-por-tipo-de-vista.md),
  [ADR-0039](../adr/0039-custom-properties-persistencia-autoridad.md)) sin un ADR nuevo.
- **Territorio de iniciativas activas**: I-49 (motor de expresiones) e I-52 (`RACKMIRROR`). I-55 debe ser compatible
  con ambas **sin depender** de ellas.

## 5. Contexto requerido

- [ARCHITECTURE.md](../ARCHITECTURE.md) §4.1; ADR-0009, ADR-0010, ADR-0034, ADR-0035 y ADR-0039.
- [Discovery](I-55-discovery.md) y [decisiones de I-55](../automation/decisions/I-55.md).
- Contratos de [I-50](I-50-cotas-independientes-por-vista.md), [I-51](I-51-rackduplicar-multiples-origenes.md),
  [I-54](I-54-propiedades-personalizadas.md), [I-11](I-11-persistencia-uniforme.md) e
  [I-53D](I-53D-cabeceras-multidestino-dinamico.md) (integrada en `dad4e77`).
- I-49 e I-52 **en sus ramas remotas**, solo lectura y en el SHA exacto que se cite.
- [AGENTS.md](../../AGENTS.md) e [I-45](I-45-test-validation-workflow.md) (evidencia por clase y por SHA exacto).
- Context Packs del frontmatter; Push Back y Cantilever se leen desde el registro de sistemas y sus subarboles.

## 6. Dependencias

- **Sin dependencias pendientes**: I-47, I-48, I-50, I-51, I-53D (integrada en `dad4e77`) e I-54 estan en `main`.
- **Paralelas**: I-49 y I-52; su estado y su cruce se re-miden antes de cada gate material (Discovery §2.1).
- **I-53D** ya forma parte de la base: la ventana del Dinamico que I-55 modificara es la de I-53D, re-auditada en G1.1.
- `conflicts_with` queda **vacio**: I-55 aun no tiene archivos de produccion. Lo fija el consenso de G2 con el cruce
  re-medido.
- **Entrada del Owner**: `requires_owner_decision: true`. El Owner fijo el producto en G1.1; las decisiones de
  experiencia de usuario que la Proposal no pueda cerrar con evidencia se formulan como decisiones concretas (opcion A,
  opcion B, compromiso, recomendacion y consecuencia).

## 7. Archivos esperados

**Esta sesion (solo documentacion)**: `docs/initiatives/I-55-*.md`, `docs/automation/decisions/I-55.md`, un ADR
**propuesto** en `docs/adr/` con su linea en `docs/adr/README.md` si corresponde, y la fila propia de I-55 en
`docs/ROADMAP.md` (correccion ordenada en G1.1).

**Produccion**: la fija el consenso de G2; el mapa previsto por archivo y simbolo vive en la Proposal y en el mapa de
implementacion, ambos **sin consenso**. Una desviacion material frente a lo acordado obliga a detenerse.

## 8. Fases

| # | Fase | Entregable | Estado |
|---|---|---|---|
| G0 | Reclamo + bootstrap | Reclamo atomico publicado, contrato y fila en ROADMAP | **HECHA** |
| G1 | Discovery | [I-55-discovery.md](I-55-discovery.md): matriz sistema × ViewKind × variante | **HECHA** |
| G1.1 | Correccion contractual | Framing ID17 + ID18 + ID19, OQ reclasificadas, rebase y reconciliacion | **HECHA** (commit de esta correccion) |
| G2 | Proposal y consenso | [Proposal V1](I-55-proposal-v1.md), [V2](I-55-proposal-v2.md) y [V3](I-55-proposal-v3.md) (historicas) con sus mapas y paquetes; en G2E, [Proposal V4](I-55-proposal-v4.md), [mapa V4](I-55-implementation-map-v4.md), ADR-0042 complementario y paquetes V4 ([Coordinador](I-55-coordinator-review-package-v4.md), [Arquitecto independiente](I-55-architect-review-package-v4.md)); veredictos de Coordinador y Arquitecto | **V1: Coordinator = CHANGES REQUIRED. V2 (`f84f303`): Coordinator = CHANGES REQUIRED → V3; Architect formal = PENDING ([revision tecnica adversarial](I-55-architect-review-v2.md)). V3 (`8e35a51`): decision CQ-01 → V4. V4 (`fe70d7a`): Coordinator = REVIEW REQUIRED; Architect formal = PENDING; M-01 abierta** |
| G3+ | Implementacion, Candidato, validacion del Owner, integracion | Gates del mapa de implementacion **tras el consenso** | **bloqueada** |

## 9. Pruebas y builds

Se fijan en el consenso de G2. Ya es exigible por norma: las **dos suites** en local sobre el Candidato, **CI verde
sobre el SHA exacto** y builds Debug de UI y de Plugin (AGENTS.md, «Pruebas — definicion de terminado»). La Proposal
maximiza la logica **pura** verificable sin AutoCAD y minimiza las guardas de texto (I-45). G0, G1, G1.1 y G2 son
documentales: su evidencia es el CI de cada commit.

## 10. Validacion manual

**Requerida** en la implementacion: crear, encolar y proyectar vistas cambia el comportamiento de dibujo y solo el Plugin
toca AutoCAD (AGENTS.md, punto 5). El checklist de AutoCAD 2025 (OV-ID17, OV-ID18, OV-ID19 y metadatos) se **disena**
en G2 y se **ejecuta** solo sobre el Candidato. G0..G2 no la requieren.

## 11. Criterios de aceptacion

**Preliminares; los congela el consenso de G2.** Recogen lo vinculante de G1.1 y la doctrina aceptada:

1. **Identidad.** Una vista hermana conserva el `RackId` de su rack; ID18 crea todas sus vistas con **un** `RackId`;
   ID19 **conserva** los `RackId` originales y **no** crea identidad nueva (ADR-0009, CD-03, CD-04).
2. **Authored/effective.** Nunca se reconstruye authored desde geometria efectiva; `ProjectVariableReference`, las
   expresiones futuras de I-49, `DimensionViews`, `CustomProperties`, `ExtensionData` y `SchemaVersion` sobreviven.
3. **Sin divergencia nueva.** I-55 no crea autoridad authored divergente entre hermanas (OQ-5).
4. **Conteo.** Agregar vistas **nunca** agrega racks: una vista hermana no suma un rack; un lote F + L + P de un rack
   nuevo es **un** rack; ID19 sobre N `RackId` son N racks antes y despues; `RACKLISTA` puede mostrar mas vistas pero
   no multiplica racks ni cambia copias por vistas nuevas; el BOM no puede elegir un authored divergente creado por I-55.
5. **Racks parciales.** Un rack existente puede recibir cualquier vista hermana soportada, incluido uno que hoy solo
   tenga Planta (OQ-7).
6. **Geometria.** Ninguna geometria nueva: los builders existentes producen toda vista.
7. **Legado.** Los flujos de una sola vista, `RACKLAYOUT`, `RACKDUPLICAR`, el BOM, `RACKLISTA`, Project Variables,
   Custom Properties y las cotas de I-50 no regresan.

## 12. Condiciones para detenerse

- **Implementacion bloqueada** hasta `Coordinator = AGREED` y `Architect = AGREED` sobre la **misma** Proposal.
- **M-01 abierta**: sin consenso posible sobre la Proposal V4 hasta resolverla (CR-02). Sin la reconciliacion obligatoria con I-52 de las
  autoridades compartidas no hay Consensus Freeze (Proposal V4 §17.2).
- Contradiccion material no resoluble con evidencia; decision del Owner no contestada por las decisiones vigentes;
  necesidad de veredicto del Arquitecto; conflicto Git o semantico ambiguo; fallo que invalide evidencia previa.
- Cruce material con archivos de produccion de I-49 o I-52 al fijar los archivos de G2: reportarlo antes de continuar.
- `docs/HANDOFF.md` **no se toca**.

## 13. Estado versionado y entrega del Pull Request

Sin `docs/automation/state/I-55.yml` (`automation.enabled: false`). Las decisiones viven en
[`docs/automation/decisions/I-55.md`](../automation/decisions/I-55.md). El estado vivo se deriva de
`origin/feature/creacion-de-vistas`. No hay Pull Request; la integracion es manual (WORKFLOW §4.5).

## 14. Evidencia final

**Reclamo atomico (G0).** Primer `git push -u origin feature/creacion-de-vistas` aceptado sin force (`* [new
branch]`) sobre `ba497f1`, con `origin` sin referencias a I-55. `main` no fue modificada.

**Bootstrap (G0).** Este contrato y la fila de I-55 en `docs/ROADMAP.md`; solo documentacion.

**Discovery (G1).** [I-55-discovery.md](I-55-discovery.md): matriz sistema × ViewKind × variante (§5), creacion y
primera vista (§6), vistas enlazadas (§7), `RequestDraw` (§8), materializacion (§9), authored/effective,
`DimensionViews`, `CustomProperties` y `ProjectVariables` (§10-13), BOM y listado (§14), `RACKLAYOUT` (§15), I-51
(§16), cancelacion (§17), pruebas (§18), cruces (§19), preguntas (§20) y hallazgos H-01..H-12 (§21).

**G1.1.** `origin/main` avanzo a `dad4e77` (merge de I-53D, CI de `push` success) y la rama se rebaso al abrir la sesion
(WORKFLOW §4.2), sin conflictos: `range-diff` con reclamo, Discovery y cierre de G1 identicos y el bootstrap cambiado
solo en contexto de ROADMAP; publicacion con `--force-with-lease` sobre `96f6444`. La ventana del Dinamico se re-audito
sobre la nueva base (Discovery §2.1). Este commit corrige el framing, reclasifica OQ-1..OQ-7 y crea el registro de
decisiones. Solo documentacion.

**G2 — Proposal V1 (sin consenso).** `d1918ab9a44ce7ae391aeacc10ff6686c8807a61`: Proposal V1, mapa de implementacion V1,
ADR-0042 propuesto (sucesor propuesto de ADR-0010), fila del indice de ADR y seccion G2 del registro de decisiones. CI de
`push` 34814797798: success en sus cuatro trabajos (Tests Domain + Application, UI Tests, Build UI, Build Plugin without
AutoCAD). Antes del commit: re-fetch con `main` en `dad4e77`; I-49 publico ADR-0041 en su rama durante la redaccion, asi
que el ADR de I-55 es 0042; revision adversarial propia y con un agente de solo lectura (Proposal §23). Los paquetes de
revision citan ese SHA exacto. Solo documentacion; HANDOFF y ROADMAP sin tocar.

**G2B — Proposal V2 (sin consenso).** El Coordinador emitio `CHANGES REQUIRED` sobre la Proposal V1 (CR-01..CR-12 en el
registro de decisiones). `f84f303adc132a7d72ebc3e2c5cf3d7bf9faf6e1` publica la Proposal V2, el mapa de implementacion
V2, ADR-0042 revisado y todavia propuesto, la fila del indice de ADR y la seccion G2B del registro. CI de `push`
34863498499: success en sus cuatro trabajos. Antes del commit se hizo el preflight y el re-fetch: `main` en `dad4e77`
sin avance; I-49 avanzo a `75f1862` e I-52 a `2275f21` (Proposal V11, con la reconciliacion obligatoria con I-55
antes de cualquier freeze). La revision adversarial tuvo tres pasadas (Proposal V2 §23). Los paquetes de revision V2
citan ese SHA exacto. Proposal V1, mapa V1 y paquetes V1 quedan como registro historico. Solo documentacion; HANDOFF
y ROADMAP sin tocar.

**G2C — revisiones sobre V2.** El Coordinador concluyo su revision tecnica sin blocker nuevo, pero no declaro
`AGREED`: M-01 y X-1..X-8 siguen abiertas. Recomienda al Owner las opciones A, sin decision del Owner registrada. La
revision del Arquitecto, asignada por la orden a esta sesion y hecha con revisores independientes de solo lectura, se
registra en [I-55-architect-review-v2.md](I-55-architect-review-v2.md):
- veredicto `CHANGES REQUIRED — PROPOSAL V3`, con MEDIUM AR2-01..AR2-06 y LOW AR2-07..AR2-20;
- X-1..X-8 AGREED;
- A-8 = complemento con nota posterior fechada en ADR-0010.

Solo documentacion; HANDOFF y ROADMAP sin tocar. **Corregido en G2D:** el Coordinador reclasifica esa revision como revision tecnica
adversarial; el veredicto formal del Arquitecto queda `PENDING`.

**G2D — Proposal V3 (sin consenso).** Estado de V2: Coordinator = `CHANGES REQUIRED → V3`, Architect formal = `PENDING`.
`8e35a51058033c2876c1935afd3f3a94931748ae` publica la Proposal V3, el mapa V3, ADR-0042 revisado (propuesto y complementario de ADR-0010, que no se
modifica), la fila del indice de ADR, la seccion G2D del registro y la nota de reclasificacion de la revision de G2C. CI de `push`
34881178359: success en sus cuatro trabajos. Preflight: `main` en `dad4e77` sin avance; antes del commit, I-49 avanzo a `f6f0991`
(Amendment A3), I-52 a `dd45b0f` (Proposal V13, releida para X-1..X-8, sin MATERIAL CONFLICT) e I-56 a `0d66df2` (Evidence Audit). La
revision adversarial de 20 puntos, con pase de verificacion, no dejo BLOCKER ni HIGH abiertos (Proposal V3 §23). Los paquetes V3 citan
ese SHA exacto; el del Arquitecto es para un Arquitecto independiente. Sin decisiones del Owner registradas. Proposal V1 y V2, sus mapas y
sus paquetes quedan como registro historico. Solo documentacion; HANDOFF y ROADMAP sin tocar.

**G2E — Proposal V4 (sin consenso).** El Coordinador decidio CQ-01 sobre V3: el redibujo de las hermanas existentes durante Insertar pasa
a PREPARE → una MUTATE → POST, y las vistas nuevas siguen colocandose una a una. `fe70d7a76c77f0c0eec01f242924a7a50a2adc4c` publica la Proposal V4
(reconciliacion acotada de CQ-01), el mapa V4 (G9 dividido en G9a y G9b), ADR-0042 revisado (propuesto y complementario), la fila del
indice de ADR y la seccion G2E del registro. CI de `push` 34889818061: success en sus cuatro trabajos. Preflight: `main` en `dad4e77` sin
avance; I-52 avanzo a `e59bc89` (Proposal V14, releida para X-1..X-8 y CQ-01, sin MATERIAL CONFLICT); antes del commit, I-49 a `c8cfee2`
(A3-R1) e I-56 a `18401da` (Proposal V1 de Workflow V2, que no se aplica a I-55). La revision adversarial de los ataques de la orden, con
pase de verificacion, no dejo BLOCKER ni HIGH abiertos (Proposal V4 §23). Los paquetes V4 citan ese SHA exacto. Sin decisiones del Owner
registradas. Proposals V1, V2 y V3, sus mapas y sus paquetes quedan como registro historico. Solo documentacion; HANDOFF y ROADMAP sin
tocar.
