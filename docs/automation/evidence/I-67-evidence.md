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
(`SnapToNearestTroquel`, `Math.Round … AwayFromZero`). La retícula de 2" convierte un requisito impar en
44/46 alternados (la base `localY` de `TROQUEL_LARGUERO` en FRONTAL es 0.6053", corregido en la ronda 1 del Architect, R-04): el redondeo cae hacia abajo y la siguiente suma no ve la elevación real. La entrada se calcula igual
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
| M-08 | no activado | ADR-0031 gobierna el primer nivel y, con la decisión D-PEND del Owner, ni su salida ni su entrada cambian (la premisa de V1 era falsa: Architect R-03) |

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

## 5. RED ampliado

MEASURED sobre `a59a6db7` (sin cambios en `src/`): T3, T4, T5, T6, T7, T8, T10, T11, T13 y T14 (degenerados y elevaciones altas) fallan por aserción. T2 exacto, T9, la cota inferior de T12, T14 sin retícula y T15 pasan: son caracterización o guarda y exigen una violación temporal tras el GREEN.

## 6. Cuantificación de las opciones de entrada

MEASURED por réplica aritmética de las fórmulas con la base real 0.6053" ([script](I-67-quantify-entrance-options.py)); no ejecuta producción. Matriz: primer nivel 4, 6, 7.5, 8 y 12"; tarima 30..72"; 2..6 niveles; 2..8 fondos de 48"; C6 y holgura 6".

| Racks | Casos | Opción «más cercano a la nominal» cambia la cabecera | Opción «subida del nivel 1» cambia la cabecera | Entrada del nivel 1 se mueve (nominal / nivel 1) |
|---|---|---|---|---|
| Sin el defecto (requisito par) | 3,850 | 198 (5.1%) | 0 | 1,210 (31.4%) / 0 |
| Con el defecto (requisito impar) | 3,675 | 984 (26.8%) | 887 (24.1%) | 1,155 (31.4%) / 0 |

Error medio de la subida frente a la nominal, primer nivel 6": histórico 0.708" (máx. 1.438"), «más cercano» 0.545" (máx. 0.938"), «subida del nivel 1» 0.723" (máx. 1.438").

Caso H58 (6", tarima 33", 4 niveles, 4 fondos, nominal 7.4375"): salidas 6.6053 / 52.6053 / 98.6053 / 144.6053 en ambas; subidas 8/8/8/8 («más cercano») o 6/6/6/6 («nivel 1»), frente a 6/8/6/8 hoy. La cabecera automática pasa de 168" a 180" en ambas.

Caso sin defecto (12", tarima 60", 2 niveles, 4 fondos): salidas iguales; la cabecera pasa de 120" a 132" solo con «más cercano».

Con la opción elegida por el Owner («subida del nivel 1»), sobre 36,720 casos (primer nivel 3..12", tarima 30..72" en medias pulgadas, 2..7 niveles, 2..10 fondos): la cabecera sube en 7,910, no baja en ninguno y queda igual en 28,810; ninguna salida ni entrada baja; la mayor subida de una salida es 10".

## 7. Revisión del Architect, ronda 1

Sobre `887141fc`, Proposal blob `fed80351`. Modo SAME-SESSION ROLE (subagente de lectura con contexto limpio, mismo modelo que el autor). Veredicto: **BLOCKED — OWNER DECISION**. Hallazgos REQUIRED R-01..R-06 en [decisiones](../decisions/I-67.md) §5. Sus afirmaciones de hecho se comprobaron contra el código: base 0.6053 (`connection-layout.csv:19`), caso 120"→132", `RACKPROYECTAR` sin redibujo de hermanas y UI Dinámica sin `FirstLevelDatum`.

## 8. Identidad del Freeze

Proposal V3, `docs/initiatives/I-67-proposal-v3.md`. Acuerdo: commit `34faf9037d3c32897e30a5e168067de9bb38e339`, blob `73758d78d40ca82fd8722339b81824c3d2ad6cde`. Freeze: commit `abc5da87ae0642d9c365dab8503d67c8ff5381bc`, blob `ba266476f80d71e9a291cd8614f2a0986b8553a6`, trailer `Freeze: I-67 docs/initiatives/I-67-proposal-v3.md`. A-n: ninguna.

## 9. Ciclo RED → GREEN de G1

MEASURED con el SDK 8.0.423 y `FullyQualifiedName~LevelElevationResolutionTests|FullyQualifiedName~LevelSpacingH58ReproductionTests` (TRX). Las corridas previas al commit de producto son diagnóstico sobre árbol sucio y no acreditan el SHA.

| Corrida | Producto | Resultado |
|---|---|---|
| RED (V2) | sin arreglo | 1411 ejecutadas, 915 pasan, 496 fallan por aserción |
| GREEN focal | con arreglo | 1098 de 1098 |
| Core completo, árbol de trabajo | con arreglo | 15,490 de 15,490; ninguna prueba existente requirió cambio |

La matriz de T12 se limita a las retículas con base menor que medio paso, las del catálogo (Proposal V3 §4). Fuera de ellas un punto medio exacto bajo la base puede redondearse hacia abajo; la medición lo encontró solo con base 1.25" y paso 0.5", la salvedad que ya había señalado el Architect en la ronda 2.

Violaciones temporales del Freeze, cada una restaurada y verificada por SHA-256 del archivo:

| Violación | Prueba | Fallan / ejecutadas |
|---|---|---|
| Quitar el épsilon | T2 ruido | 1 / 3 |
| Subir un troquel el posterior en un nivel | T9 | 3 / 13 |
| `Floor` en lugar de `Ceiling` | T12 cota inferior | 221 / 360 |
| Quitar el piso de un paso | T14 degenerados | 2 / 2 |
| Medir la cabecera con la entrada del primer nivel | T15 | 3 / 24 |
| Entrada al troquel más cercano a la nominal | T16 | 4 / 98 |
| Regla nueva en una sola sobrecarga | T17 | 1 / 1 |
| Omitir la migración de datum | T12 por `LoadFromDesign` | 1 / 1 |
