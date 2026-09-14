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

> **Fase actual: G2 — Proposal V1 publicada sin consenso** ([Proposal V1](I-55-proposal-v1.md) @ `d1918ab`), con mapa de
> implementacion, ADR-0042 propuesto y paquetes de revision para Coordinador y Arquitecto. No hay una sola linea de
> produccion escrita; la implementacion esta **BLOQUEADA** (seccion 12).
>
> ```text
> SUBSTANTIVE IMPLEMENTATION: BLOCKED
> Coordinator: REVIEW REQUIRED
> Architect:   REVIEW REQUIRED
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
4. **G2 — Proposal y consenso**: Proposal V1 (NOT CONSENSUS), ADR propuesto si corresponde, mapa de implementacion,
   diseno de pruebas y de validacion del Owner, y paquetes de revision para Coordinador y Arquitecto. Autorizado como
   documentacion; el consenso lo emiten Coordinador y Arquitecto.
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
| G2 | Proposal y consenso | [Proposal V1](I-55-proposal-v1.md), ADR-0042 propuesto, [mapa de implementacion](I-55-implementation-map-v1.md), paquetes de revision ([Coordinador](I-55-coordinator-review-package-v1.md), [Arquitecto](I-55-architect-review-package-v1.md)); veredictos de Coordinador y Arquitecto | **Proposal V1 publicada (`d1918ab`); pendiente de veredictos** |
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
