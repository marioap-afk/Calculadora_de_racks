# I-57 F1 — Evidencia de caracterizacion

```text
Gate             = F1 / CHARACTERIZATION ONLY
F1 start SHA     = c641476abca9d6a4370bd222961a0f997fbf5c73
Fixture manifest = tests/RackCad.Tests/Fixtures/I57/f1-characterizations.json
Production diff  = NONE
F1 result        = COMPLETE
F2               = NOT OPEN
```

## Resultado por CT

| CT | Fuente de produccion observada | Expected fijado | Fail-closed fijado | Evidencia ejecutable/reutilizada |
|---|---|---|---|---|
| CT-04 | `RackEmbedDocument`, `RackEmbedStore` y comandos por kind | seis tokens de kind, tres view tokens, preservacion literal de `View/Section`, default `Section=-1` | null/blank/junk/future incompatible no adquiere direccion valida | clase F1 + `RackEmbedDocumentTests` + caracterizaciones legacy de dimensiones |
| CT-05 | builders/layouts por familia y `CantileverViewPlan.Bounds` | matriz explicita de once familias/vistas con ejes, origen, `[K_min,K_max]`, centro, extremo, offset y bounds separados | endpoint ambiguo deja el frame no disponible para F3 | fixture JSON + clase F1 + suites geometricas existentes |
| CT-16 | `RackDuplicationPlan.Build` | filtro fisico, deduplicacion y orden estable conservan grouping, COPY identity, nombre y conteo | payload desconocido/ilegible falla el plan completo | clase F1 + `RackDuplicationPlanTests` + `SelectiveDuplicationFailClosedTests` |
| CT-RES | `SelectiveKindHandler` y resolvers por kind | handler Selective invoca una resolucion efectiva y delega al resolver vigente | resolucion fallida no produce plan/BOM | guarda Plugin F1 + suites de resolvers y handlers |
| CT-PLAN | `HeaderRunPlan`, builders y `CantileverViewPlan` | payload tipado conserva instancias/curvas y geometria del builder | builder ausente o payload incongruente bloquea preparacion | clase F1 + `DrawServicePlanBaselineTests` + Cantilever |
| CT-NAME | `BlockNaming` y helpers `BlockName`/`SuggestName` | cadena base exacta antes de colision; `.` permanece valida | productor no caracterizado permanece legacy | clase F1 + `BlockNamingTests` + guardas Plugin |
| CT-SCAN | `ProjectVariableScanProjection` | definition id y direct-reference count sobreviven; foreign y unreadable no se confunden | no inventa RackId; unreadable no equivale a ausencia | clase F1 + `ProjectVariableScanProjectionTests` |
| CT-GEO | `Transform2D` | matriz, giro, espejo, traslacion, escala y determinante observables | escala cero/negativa lanza; policy/tolerancia queda fuera | clase F1 + `GeometryPrimitivesTests` |
| CT-BLK | `HeaderBlockInstance.BlockName`, `BlockLibraryImporter`, `SystemBlockWriter`, `LateralHeaderDrawer` | BLK-IDENTITY-01..03 y BLK-AVAILABILITY-01..03; query final posterior a import cuando aplica | MISSING preliminar no es sticky; import fallido sigue MISSING; no-import usa query final | seis casos focales F1 + guardas Plugin + baselines de plan |
| CT-AUTH | `SelectiveAuthoredAuthority` | Single/Divergent/Unreadable; comparator Selective include-by-default | divergent/unreadable devuelve null y nunca elige hermana | clase F1 + suites de autoridad Selective |

## Separaciones demostradas

1. `PreparedViewPlan.BaseName` y cada requirement key ocupan espacios distintos: importer/query solo ven
   `HeaderBlockInstance.BlockName`; sanitizacion y suffix solo actuan sobre la definicion generada.
2. El flujo con import observa de nuevo el target despues de `Ensure/import`; el resultado preliminar nunca se devuelve
   como availability final. Sin permiso de import, el primer query es final.
3. `Resolve` y `Plan` siguen delegando a resolvers/builders actuales; las pruebas no introducen algoritmo ni payload
   universal.
4. El snapshot fisico AutoCAD queda en Plugin. Las pruebas Core solo ejercitan datos puros y usan guardas textuales para
   seams Plugin que el proyecto de tests no puede cargar.
5. No se detecto contradiccion material con Proposal V4 o R3. Proposal V5 no es necesaria por F1.

CT-GEO deja una frontera explicita para F4: el factory `Scale` rechaza factores cero o negativos, mientras el
constructor matricial publico permite matrices no uniformes y degeneradas. Esto no contradice V4; confirma por que el
adapter futuro debe devolver un resultado tipado no soportado en vez de atribuir escala uniforme a toda matriz.

## Characterization Blockers

| Blocker de Proposal V4 | Resultado F1 |
|---|---|
| matriz codec CT-04 | CLOSED: entradas y disposiciones quedan enumeradas; una combinacion sin evidencia sigue Invalid/STOP |
| convenciones de extremos CT-05 | CLOSED: once filas fijan eje, origen, tramo, centro, extremo, variante, bounds y fuente |
| comparators no Selectivos | CLOSED para F1: ausencia actual queda `Unreadable`; F6 debe aportar adapter por kind antes de habilitarlo |
| inventario de productores legacy | CLOSED: AUTH-01..14 censadas; AUTH-15 excluida |

`Open Material = NONE`. `Open Minor = NONE`. No se crea una CR nueva ni se requiere Proposal V5.

## Corridas

| Nivel | Resultado local |
|---|---|
| T0 focal F1 | 26 passed, 0 failed, 0 skipped; seleccion mayor que cero |
| T1/final Core | 7,266 passed, 0 failed, 0 skipped |
| UI Full | 1,568 passed, 0 failed, 17 skipped, 1,585 total |
| Debug UI | success, 0 errors, 0 warnings |
| Debug Plugin | success, 0 errors; solo los 2 `MSB3277` conocidos de referencias AutoCAD |

La primera ejecucion T0 observo un fallo real (25/26): `Transform2D.Scale(0)` arroja `ArgumentException`. La
expectativa se corrigio al tipo exacto observado y la repeticion quedo 26/26. Una invocacion inicial de UI con
`--no-restore` no ejecuto pruebas porque faltaba `project.assets.json`; tras restaurar, UI Full quedo verde con los
conteos anteriores. El recibo de CI de `push` se conserva por run/SHA en el reporte de sesion: agregarlo aqui crearia
otro SHA y haria obsoleto el recibo exacto que pretende registrar.
