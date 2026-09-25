# I-55 — G14: ID19 pure group placement and projection

```text
Gate            = G14 (ID19 PURE CONTRACT)
Estado          = COMPLETE
Alcance         = contrato puro en Application; sin comando, sin AUTH-15, sin edicion de Foundation
PRE_REBASE_I55_SHA = afc4a864bd26514e74b1c88d47686298f26df70e
POST_REBASE_BASE   = 016bf46715cec45e644f88a22ef091b311a2bef1   (origin/main con I-59 integrada)
G14_START_SHA      = 7a4b7d776e4a09ce65baabe048ef427f1dc3390f
G14_PRODUCT_SHA    = a3c66db496ca4312c8ed21efc44ff9780e66cf14
G14_FINAL_SHA      = el commit de cierre que publica este recibo
Archivo de preservacion = archive/i-55-pre-i59-integration-afc4a86 (anotado, empujado)
```

## 1. Recibo de I-59 verificado antes de consumir

| Requisito | Observado |
|---|---|
| `integration/I-59` existe y es anotado | si (`git cat-file -t` = `tag`) |
| Objeto del tag | `a3ee77b9d9eda033bdcbafb0f9849d3a3df5b548` |
| Destino pelado | `016bf46715cec45e644f88a22ef091b311a2bef1` = `origin/main` |
| `FINAL_CANDIDATE_SHA` | `d905572e9d3faf7bedcb6c22abe51c36da6e11eb` (I-59: versiona A-1 para Owner Validation) |
| `CLOSURE_SHA` | `b5e25582f376770e4bab476a97a311da9b152c79` |
| `MERGE_SHA` | `016bf46715cec45e644f88a22ef091b311a2bef1` |
| CI post-merge | run `36071758876`, `success`, `headSha = 016bf467` |
| Cobertura del Candidate | run `36030056756`, `success`, `headSha = 885b4069` (commit `I-59: declara Candidate tras READY-09`, ancestro de `main`) |
| `integration/I-57`, `integration/I-58` | presentes y anotados |

La unica diferencia observada frente al enunciado de la orden es el `headSha` del dispatch de cobertura: mide el Candidate
declarado (`885b4069`), no el commit posterior de documentacion `d905572e`. El identificador de run y su resultado coinciden.

## 2. Reconciliacion de I-55 sobre I-59

- Rebase de los 44 commits de I-55 sobre `origin/main` (`016bf467`). `git range-diff` empareja los 44: solo cambian los dos
  commits que tocan la fila de ROADMAP, resueltos de forma semantica conservando la fila de I-59 y la de I-55 sin duplicarla.
- Resultado: `7a4b7d776e4a09ce65baabe048ef427f1dc3390f`, arbol limpio.
- El resultado contiene el trabajo historico hasta G12, AUTH-13 de I-58 y AUTH-08 V2 y AUTH-12 V2 de I-59.
- Linea base tras el rebase, antes de escribir G14: Core `11501` pruebas, 0 fallos.

## 3. Consumo de Foundation (sin sustitutos locales)

| Autoridad | Consumo en G14 | Evidencia |
|---|---|---|
| AUTH-08 V2 (`RackSourceTransformFactsV2`) | `RackProjectionSourceTransform.Accept` recibe el `RackSourceTransformFactsResult` del clasificador y conserva la **misma instancia** de hechos; la parte lineal se compone con `Transform2D` compartido | `I55G14FoundationConsumerTests.AUTH08_V2_FACTS_TRAVEL_INTO_THE_PRODUCT_WITHOUT_BEING_REBUILT` |
| AUTH-12 V2 (`LibraryPieceRequirement`, `LibraryAvailability`, `LibraryBlockPresence`) | La compuerta estructural lee los hechos tal cual y los copia al diagnostico (`PieceId`, `ViewAddress`, `Role`, `LibraryKey`, `KeyState`, `Availability`, `Presence`) | `RackGroupPlacementPlanTests` (Z, AA, AB, AC, AD) y `I55G14FoundationConsumerTests.AUTH12_V2_FACTS_TRAVEL_INTO_THE_DIAGNOSTIC_WITHOUT_BEING_REBUILT` |
| AUTH-05 (`RackViewFrame`) | `RackViewFrameSemantics` lee ejes, origen, tramo, centro y desplazamiento por variante; nunca deduce nada de la envolvente dibujada | `RackGroupFrameTests` |
| AUTH-07 (`RackPhysicalSelection`) | CLASSIFY y GROUP trabajan sobre los hechos de seleccion | `RackGroupPlacementPlanTests` |
| AUTH-13 (I-58) | `RackSiblingAuthoredGate` traduce `RackAuthoredComparisonOutcome` a politica | `RackSiblingAuthoredGateTests` |
| AUTH-09 / AUTH-10 | El plan las consume por puerto, una sola vez por `RackId` (Resolve) y una vez por vista destino (Prepare) | `RackGroupPlacementPlanTests` (M, AD) |

**Guardas de no duplicacion:** ningun archivo de `src/RackCad.Application/Views` contiene `Math.Atan2`, `NormalizePi`,
`new RackSourcePlacementSnapshot`, `RackScaleSign.ZeroWithinTolerance`, `new LibraryPieceRequirement(`,
`LibraryPieceAvailabilityFlowV2.Observe`, `RackHeaderPieceRequirementExtractorV2`, `ILibraryPieceAvailabilityQuery` ni
`blocks-library`.

**Ediciones de Foundation en G14: NINGUNA.** `git diff` frente a `origin/main` no toca `Systems/Shared`, `Drawing` ni
esquemas; los archivos de I-59 llegan por el rebase, no por el diff de G14.

## 4. Contrato entregado

| Pieza | Responsabilidad |
|---|---|
| `RackProjectionPipeline` | CLASSIFY → GROUP → AUTHORITY GATES → REPRESENTATIVE → RESOLVE → EDIT PREFLIGHT → AVAILABLE → FRAMES → VALIDATE → PLANS, con fallo cerrado antes de cualquier punto |
| `RackGroupPlacementPlan` | Grupos por `RackId`, direcciones destino, vistas aceptadas, proyeccion ortografica, avisos y `Place(B, T)` |
| `CommonTransform2D` | Una transformacion rigida por operacion: `Translation(-B).Then(Rotation(alpha)).Then(Translation(T))`, `det = +1`, escala 1 |
| `SourceGroupFrame`, `TargetGroupFrame` | Marcos semanticos; `phi_t = 0` y `alpha = 0` congelados (OD-6.b A, OD-6.c A) |
| `RackRigidPlacementPolicy` | Misma clase: el layout de anclas se traslada; `rho = phi_r + alpha` |
| `RackOrthographicPlacementPolicy` | Planta contra elevaciones con **Relative Frame Window** (OD-7.e A); jamas mayoria |
| `RackProjectionClassMapping` | Mapeos congelados; frontal ↔ lateral no expuesto |
| `RackSiblingAuthoredGate` | Compuerta authored sobre AUTH-13, independiente de propiedades |
| `RackProjectionRemedySelector` | Una fila por situacion de la Proposal V5 §3.8 |
| `RackProjectionDiagnostic`, `RackProjectionWarning` | Hechos de Foundation preservados; el producto solo anade remedio |

## 5. Decisiones del Owner respetadas

OD-1..OD-8 y M-01 tal como estan registradas: `phi_t = 0`; `alpha = 0`; misma clase conserva la variante de la fuente;
clase distinta toma la canonica; tipos fuente mezclados, varias definiciones por `RackId` y familias mezcladas fallan
cerrado; superposicion avisa; **OD-7.e = A (Relative Frame Window)** con el aviso de cercania al limite.

## 6. Matriz de compuerta estructural (AUTH-12 V2)

| Caso | Resultado |
|---|---|
| `Required` valido y `Present` | pasa |
| `Required` con clave en blanco | `RequiredKeyMissing`, falla antes de puntos |
| `Required` con `BlockMissing` | `RequiredBlockMissing` |
| `Required` con `FileMissing` | `RequiredLibraryMissing` |
| `Required` con `Unknown` | `RequiredUnknownAvailability` |
| `OptionalVisual` ausente | aviso, no falla |
| `NotApplicable` | ignorado |
| `UnknownSourceRole` | falla cerrado |

## 7. Evidencia

| Suite | Resultado |
|---|---|
| RED de G14 (antes de implementar) | 87 fallos / 11 verdes sobre las 98 pruebas nuevas |
| GREEN focal de G14 | 98 / 98 |
| Core completa | 11599 pruebas, 0 fallos, 0 omitidas |
| UI completa | 1616 pruebas, 0 fallos, 17 omitidas (las mismas de la base: G14 no anade pruebas de UI) |
| Build UI Debug | 0 errores |
| Build Plugin Debug | 0 errores (solo las dos advertencias MSB3277 conocidas) |
| CI de `push` | run `36076491560` sobre `a3c66db4`: `success` en los cuatro trabajos |

Clases nuevas: `CommonTransform2DTests`, `RackGroupFrameTests`, `RackRigidPlacementPolicyTests`,
`RackOrthographicPlacementPolicyTests`, `RackGroupPlacementPlanTests`, `RackSiblingAuthoredGateTests`,
`RackProjectionRemedyTests`, `RackPlacedGeometryInvariantTests`, `RackViewCountInvariantTests`,
`RackViewAuthoredEquivalenceTests`, `RackViewPartialRackContractTests`, `RackSiblingGateIndependenceGuardTests`,
`I55G14FoundationConsumerTests` y el soporte `G14ProjectionTestSupport`.

## 8. I-52 y AUTH-15

| Dato | Observado |
|---|---|
| Rama | `feature/rackmirror-espejo-semantico` |
| Punta | `5f9af17ba86973609f374ee99ead6e7128dc0115` (`I-52: publica hito R2 build-side de CT-DA`) |
| AUTH-15 implementada | no: su propio registro declara que no hay codigo de produccion de AUTH-15 |
| AUTH-15 integrada en `main` | no: `main` no contiene `CreateInTransaction` ni entrada AUTH-15 en `docs/FOUNDATIONS.md` |
| Solape con G14 | ninguno: G14 no materializa, no crea definiciones y no abre transacciones |

**G15 sigue BLOCKED BY AUTH-15.** G14 no lo espera y no implementa una autoridad sustituta.

## 9. Fuera de alcance en G14

`RACKPROYECTAR`, `RPY`, `RackProyectarCommands`, cualquier materializacion, Owner Validation (OV-ID19), Flow Bed,
modificacion de AUTH-08 o AUTH-12, migracion de DTO, store o esquema.
