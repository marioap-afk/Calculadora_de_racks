# I-67 — Evidencia de la unidad (elevaciones de niveles, H58)

Unit / Initiative / Workflow: `I-67` / I-67 / V2. Claim-Id `9e3c2538-2eca-4ad9-b62f-48862f784004`.
Reclamo `b38f2e7f`, base `origin/main` `a140eb3a9f055518f0ffa5a3f927ff1c0e1c1e60`.
Clasificación **T4**: reclamo posterior a `WORKFLOW_V2_EFFECTIVE_SHA` (`8a021fb6`), base que lo contiene, pausa terminada.

Cada afirmación se etiqueta `MEASURED`, `RECONSTRUCTED`, `INFERENCE` o `UNKNOWN` (LIFECYCLE §4).

## 1. Reproducción de H58

**Clasificación: REPRODUCED.** MEASURED con `tests/RackCad.Tests/LevelSpacingH58ReproductionTests.cs`, sobre un árbol cuyo
`src/` es idéntico al de `a140eb3a` (commit de RED; `git diff --name-only a140eb3a HEAD -- src` vacío), SDK 8.0.423.

| Caso (tarima 33", C6 6", holgura 6", requisito 45", primer nivel 6", 4 niveles) | Obtenido | Esperado |
|---|---|---|
| Dinámico, `DynamicRackSystemResolver` y catálogo real | 44 / 46 / 44 | 46 / 46 / 46 |
| Push Back, `PushBackResolver` y catálogo real | 44 / 46 / 44 | 46 / 46 / 46 |
| Primeros niveles 0, 2.5 … 12, 6 niveles | intervalos de 44" | ≥ 45" |

Selección: 3 pruebas, 3 fallos por aserción. El Push Back comparte el defecto.

## 2. Discovery Core

**DC-01 Comportamiento.** MEASURED. `DynamicLoadBeamGeometry.ResolveLevels` (`src/RackCad.Application/Systems/Dynamic/`) acumula una elevación
**teórica** (`rawExit += InOutBeamDepth + Pallet.Height + ClearHeight`) y ajusta cada nivel al troquel **más cercano**
(`SnapToNearestTroquel`, `Math.Round … AwayFromZero`). La retícula de 2" con base `TROQUEL_LARGUERO` = 0.5" convierte un requisito impar en
44/46 alternados: el redondeo cae hacia abajo y la siguiente suma no ve la elevación real. La entrada se calcula igual
(`rawExit + slope`), de modo que la subida también alterna (6"/8" con 4 fondos: la nominal es 7.4375").

**DC-02 Dueños.** MEASURED.

| Regla | Dueño | Gobierno |
|---|---|---|
| Elevación de salida y entrada por nivel | `DynamicLoadBeamGeometry.ResolveLevels` | ningún ADR fija el redondeo de niveles superiores (búsqueda en `docs/adr`) |
| Desde dónde se mide el primer nivel | `RackFirstLevelDatum` | ADR-0031, I-42 |
| Retícula | `DynamicRackSystemResolver` (punto `TROQUEL_LARGUERO` FRONTAL + `TroquelPaso`) | código |
| Larguero posterior Push Back | `PushBackElevations.ChooseHighTroquel` | ADR-0031 §9 (el bajo es el ancla) |
| Requisito de intervalo | la misma suma de `ResolveLevels` sobre `DynamicRackLevel` | código |

Precedente de producto: `SelectiveGeometryResolver.SnapUp` ya es «el troquel más pequeño ≥ valor» con épsilon `1e-9` y acumula desde la `y` resuelta.

**DC-03 Persistencia.** MEASURED. `DynamicRackSystemDocument` y `PushBackDesignDocument` guardan solo entradas authored (`FirstLevelHeight`, `FirstLevelDatum`,
niveles por celda). `LoadBeamLevels` no se serializa: el único uso en el documento es su conteo (`DynamicRackSystemDocument.cs:89`). El XML-doc del DTO lo declara:
«Positions and physical members are always rebuilt on load». No hay marcador que distinga un documento resuelto con la regla anterior.

**DC-04 Camino.** MEASURED. Entrada de editor o `Design` → `DynamicRackSystemResolver.Resolve` → `DynamicRackLevelGeometry.Resolve` → `ResolveLevels` →
`front.LoadBeamLevels` (y `system.LoadBeamLevels` por proyección) → builders y BOM. El Push Back entra por `PushBackResolver` / `PushBackCompositeResolver`, que reutilizan el resolver dinámico.

**DC-05 Consumidores.** MEASURED por búsqueda de `LoadBeamLevels`, `ExitElevation`, `EntranceElevation` y `ResolveLevels` en `src/`.

- Una sola llamada de producción a `ResolveLevels` (`DynamicRackSystemResolver.cs:104`). La sobrecarga por `PalletSpecification` solo tiene llamadores de prueba.
- Ninguna otra suma teórica de niveles fuera de `ResolveLevels` y del `Calculate` de reserva de `DynamicHeaderHeightCalculator`, que solo corre sin frentes.
- Vistas y cotas consumen los niveles resueltos: `DynamicLoadBeamGeometry.Placements`, `DynamicViewDecorations` (cadena de separaciones), `DynamicSystemFrontalBuilder`, `DynamicSafety*`, `DynamicFlowBedGeometry`, `DynamicEntranceGuidePlan`, `DynamicEndBeamIdentity`.
- Push Back: `PushBackPlacements` (caja negra sobre `Placements`), `PushBackElevations` (bajo = salida; alto derivado), `RackLevelElevations` para cotas y vistas, `PushBackRuns`, `PushBackMirror` y `PushBackDiverterPlan`.
- Altura: `DynamicHeaderHeightCalculator.CalculateResolved` y `PushBackHeaderHeight.Requirement` usan la entrada del último nivel.
- Rutas que re-resuelven desde el diseño: `RACKEDITAR` (ventanas), BOM (`DynamicKindHandler` / `PushBackKindHandler`), proyección (`RackProjectionKindSessions`).
- Rutas que no re-resuelven: abrir o guardar (el Plugin no registra eventos de documento; `PluginInitializer` no los usa) y duplicación (sin referencias a resolvers).

**DC-06 Pruebas.** MEASURED. `ALegacyDocument_KeepsItsPhysicalGeometry` y su gemela dinámica fijan el **primer** larguero. `Resolver_SnapsBothInOutBeamMatesToTheNearestPostTroquel`
exige estar en retícula. Hueco declarado: ninguna prueba fijaba intervalos entre niveles; ninguna medía separaciones de cota.

**DC-07 Paralelas.** MEASURED. `git diff --name-only <merge-base> <rama> -- src tests` filtrado por `Dynamic|PushBack|Troquel|LevelDatum` es vacío en
`architecture/workspace-persistente-rackcad`, `architecture/portabilidad-coordinador-principal`, `fix/placedclone-copia-anotaciones` y `feature/rackmirror-espejo-semantico`.
Ningún archivo caliente de WORKFLOW §7 interviene.

**DC-08 Fundaciones.** MEASURED. Consume `RackFirstLevelDatum` (ADR-0031) y la autoridad vertical de Push Back (ADR-0031 §9). FOUNDATIONS no registra la resolución de niveles. No se extiende ni se introduce fundación.

**DC-09 Materialidad.**

| M | Resultado | Evidencia |
|---|---|---|
| M-01 | no activado | el dueño de la regla no cambia y no aparece otra autoridad |
| M-02 | no activado | sin DTO ni significado persistido nuevos |
| **M-03** | **activado** | un documento existente adopta la regla al re-resolver; es decisión de esta unidad, no un efecto colateral |
| M-04 | no activado | no cambia qué falla ni cómo |
| **M-05** | **activado** | la salida de `ResolveLevels` la consumen Dinámico y Push Back |
| M-06, M-07 | no activados | sin puntos de extensión ni mecanismo genérico |
| M-08 | no activado | ADR-0031 limita «ningún archivo se mueve» a la introducción del datum, y el primer nivel no cambia |

**Arquetipo propuesto: FOUNDATION EVOLUTION** (M-03, M-05). Eleva el EXTENSION inicial y obliga a una revisión adversarial del Architect.
**ADR: no requerido**: ninguno de los cuatro criterios de `docs/adr/README.md` aplica (regla local de una función, barata de revertir, sin formato ni contrato nuevos).

## 3. Expansiones

| EXP | Resultado | Razón |
|---|---|---|
| EXP-01 | negativo | código, ADR y FOUNDATIONS no se contradicen sobre niveles superiores |
| EXP-02 | negativo | una sola autoridad |
| EXP-03 | negativo | la persistencia se localiza por completo en DC-03 |
| EXP-04 | cubierto por DC-05 | consumidores en un segundo sistema inventariados |
| EXP-05 | negativo | cada invariante tiene prueba o modo de probarla |
| EXP-06 | negativo | no hay fallo nuevo |
| EXP-07 | negativo | ninguna rama toca la autoridad |
| **EXP-08** | **positivo, acotado** | la altura manual (`ManualHeaderHeightOverride`) no se valida contra el último larguero. Es deuda previa; subir niveles puede hacerla visible. Se registra en `docs/ideas-futuras.md` y no se corrige aquí |
| EXP-09 | negativo | ninguna M quedó UNKNOWN |

## 4. Revisión del Coordinator

SAME-SESSION ROLE. DC-01..09 completos. Pregunta expresa de LIFECYCLE §4 (qué expansión debió activarse): ninguna adicional. Agrupación: unidad única (un problema, una autoridad). Sin discrepancia clase A.

Delegación de la implementación: **ninguna** (AUTOMATION_PLAN §16 exige orden expresa y no se adopta por estar escrito). El cambio cabe en una función y sus pruebas; no hay piezas independientes que paralelizar. Se delega solo la revisión del Architect, en un contexto limpio.
