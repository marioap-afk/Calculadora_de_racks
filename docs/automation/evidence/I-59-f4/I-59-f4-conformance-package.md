# I-59 F4 — paquete de conformidad y readiness

## Identidad y limites

```text
Unit                  = I-59
Workflow              = V2
Claim-Id              = 9d3b2b65-7d0b-4db0-9233-0b6d63f3d63a
F4_START_SHA           = 058061b12e22464897363aa191e2470ed07c1271
FREEZE_COMMIT_SHA      = a7133b3f12cac8e4c763584c02530faa03ef29ae
FREEZE_PATH            = docs/initiatives/I-59-proposal-v3.md
FREEZE_BLOB_SHA        = 3237376922ae24a94995c06c1c3ec5f74bdbdd2f
A-n                    = NONE
F1 CLOSURE             = ACCEPTED
F2 CLOSURE             = ACCEPTED
F3 CLOSURE             = ACCEPTED / aef8bd2c9d9a74ca1d1a1fd03624aa3207ee12f1
READY                  = NOT OPEN
FINAL_CANDIDATE_SHA    = DOES NOT EXIST
```

Este paquete prepara la revision posterior de READY/conformance. No emite READY-06, no declara
Candidate, no integra y no desbloquea I-55.

## Consumidores paralelos revalidados

| Consumidor | Tip remoto observado tras fetch | Interseccion material AUTH-08/AUTH-12 |
|---|---|---|
| I-52 `feature/rackmirror-espejo-semantico` | `5f9af17ba86973609f374ee99ead6e7128dc0115` | NONE: el diff actual contra main se limita a `docs/automation`, documentos de iniciativa/ADR y `eng/research/I52Ctda`/validacion; no toca `src/`, tests ni Shared View Foundation |
| I-55 `feature/creacion-de-vistas` | `afc4a864bd26514e74b1c88d47686298f26df70e` | NONE sobre la autoridad I-59: G14 sigue abierto y declara consumo posterior desde `main`; su producto vigente no contiene los simbolos V2 de I-59 |

## CT59-16 — revision factual de scope

Diff revisado: `a7133b3f12cac8e4c763584c02530faa03ef29ae..F4 product tip`.

| Clase | Archivos/cambio | Veredicto |
|---|---|---|
| Foundation Application neutral | `LibraryPieceRequirementsV2.cs`, `RackSourceTransformFactsV2.cs`, seam aditivo en `RackViewPreparation.cs` | permitido por Freeze |
| adapters Plugin neutrales | captura AutoCAD, observer externo, cache compartido y refactor mecanico del importer | permitido; sin consumer policy |
| tests I-59 | F2/F3 y `I59F4ConformanceTests` | permitido |
| evidencia/estado I-59 | solo `docs/automation/evidence/I-59-*` y contrato mutable I-59 | permitido |
| schema / persisted DTO / store / serializer | ningun archivo | NONE |
| I-55 product | ningun archivo de su rama/producto | NONE |
| I-52 product | ningun archivo de su rama/producto | NONE |
| BOM / product geometry | ningun archivo | NONE |
| resolve / authored authority | ningun archivo | NONE |
| naming policy | ningun archivo | NONE |

```text
Schema diff             = NONE
Persistence migration   = NONE
Persisted DTO diff      = NONE
Store/serializer diff   = NONE
I-55 product diff       = NONE
I-52 product diff       = NONE
BOM/product geometry    = NONE
Resolve/authored diff   = NONE
Naming policy diff      = NONE
```

La guarda `I59F4ConformanceTests.CT59_16_*` complementa esta revision factual; no la sustituye.

## Matriz cross-consumer CT59-01..17

| Frozen clause / CT | Production symbol | Protecting test | Consumer | Verdict |
|---|---|---|---|---|
| CT59-01 boundary Plugin/Application | `RackSourcePlacementCaptureAdapter`, `RackSourcePlacementInput`, `RackSourceTransformClassifier` | `I59F2BoundaryGuardTests`, `I59F4ConformanceTests` | I-55 G14; I-52 | PASS |
| CT59-02 Position/Origin XYZ separados | `RackSourcePlacementSnapshot.ReferencePosition/DefinitionOrigin` | `I59F2ContractTests.CT59_01_02_*` | I-55 G14 | PASS |
| CT59-03 angulo canonico | `RackSourceTransformFactsV2.RotationRadians` | `CT59_03_ROTATION_IS_CANONICAL` | I-55 G14; I-52 | PASS |
| CT59-04 ejes/signos/non-uniform | `ScaleX/Y/Z`, `SignX/Y/Z` | `CT59_04_05_*`, `CT59_04_06_*` | I-55 G14 | PASS |
| CT59-05 determinant/reflection exactos; no compuesto | `ReferenceBasisDeterminantXY`, `ReferenceBasisIsReflectionXY` | `CT59_04_05_*`, `CT59_05_*` | I-55 G14; I-52 | PASS |
| CT59-06 facts independientes | `IsUniformScale`, `IsUnitScale`, `HasPlanarHalfTurnSign`, `IsNegativeZ` | `CT59_06_*` | I-55 G14 | PASS |
| CT59-07 Normal/world-Z/tolerance | `Normal`, `NormalIsWorldZ`, `Tolerance` | `CT59_07_*` | I-55 G14 | PASS |
| CT59-08 failure semantics | `RackSourceTransformOutcome` | `CT59_08_*` | I-55 G14; I-52 | PASS |
| CT59-09 identidad por instancia | `LibraryPieceRequirement` | `CT59_09_*` | I-55 G14/G15 | PASS |
| CT59-10 address tipada | `PrepareV2`, `RackHeaderPieceRequirementExtractorV2` | `CT59_10_*` | I-55 G14/G15 | PASS |
| CT59-11 mapping total | `RackHeaderBlockRequirementRoleClassifier` | `CT59_11_*` | I-55 G14/G15 | PASS |
| CT59-12 blank Required | `RequirementKeyState.KeyMissing` | `CT59_12_*` | I-55 G14/G15 | PASS |
| CT59-13 availability/presence independientes | `LibraryAvailability`, `LibraryBlockPresence`, `ExternalLibraryAvailabilityFacts` | `CT59_13_*`, `I59F3CorrectionTests` | I-55 G14/G15 | PASS |
| CT59-14 query/import y reattachment | `LibraryPieceAvailabilityFlowV2` | `CT59_12_13_14_*` | I-55 G14/G15 | PASS |
| CT59-15 compatibilidad V1/cross-consumer | APIs V1 + V2 aditivas | `I59F3ConsumerIntegrationTests` | I-52; I-55 G8/G9/G12 | PASS |
| CT59-16 scope/schema/persistence | diff factual + guarda F4 | `I59F4ConformanceTests.CT59_16_*` | repo completo | PASS |
| CT59-17 draft factual de diez campos | draft F4 | `I59F4ConformanceTests.CT59_17_*` | cierre documental posterior | PASS; re-verificacion Candidate pendiente |

## I-55 G14 foundation readiness

| I-55 G14 requirement | I-59 authority/seam | Exact symbol/API | Available after integration? | Local reconstruction needed? |
|---|---|---|---|---|
| Position XYZ | snapshot AUTH-08 | `RackSourceTransformFactsV2.ReferencePosition` | YES | NO |
| DefinitionOrigin XYZ | snapshot AUTH-08 | `RackSourceTransformFactsV2.DefinitionOrigin` | YES | NO |
| source rotation canonical | classifier AUTH-08 | `RotationRadians` | YES | NO |
| Scale X/Y/Z + signs | facts AUTH-08 | `ScaleX/Y/Z`, `SignX/Y/Z` | YES | NO |
| determinant/reflection | facts AUTH-08 | `ReferenceBasisDeterminantXY`, `ReferenceBasisIsReflectionXY` | YES | NO |
| uniform/non-uniform | facts AUTH-08 | `IsUniformScale` | YES | NO |
| unit/non-unit | facts AUTH-08 | `IsUnitScale` | YES | NO |
| half-turn | facts AUTH-08 | `HasPlanarHalfTurnSign` | YES | NO |
| negative-Z | facts AUTH-08 | `IsNegativeZ` | YES | NO |
| Normal | snapshot AUTH-08 | `Normal` | YES | NO |
| NormalIsWorldZ | facts AUTH-08 | `NormalIsWorldZ` | YES | NO |
| tolerance inputs | classifier input/output | `RackSourceTransformTolerance`, `Tolerance` | YES | NO |
| PieceId | carrier AUTH-12 | `LibraryPieceRequirement.PieceId` | YES | NO |
| RackViewAddress | preparation context AUTH-12 | `ViewAddress`, `PrepareV2` | YES | NO |
| RequirementRole | total classifier AUTH-12 | `RequirementRole`, `RackHeaderBlockRequirementRoleClassifier` | YES | NO |
| LibraryKey / KeyMissing | carrier AUTH-12 | `LibraryKey`, `RequirementKeyState` | YES | NO |
| external FileMissing/Unknown | external observer AUTH-12 | `LibraryAvailability` | YES | NO |
| external Present/BlockMissing | external observer AUTH-12 | `LibraryBlockPresence`, `AutoCadExternalLibraryBlockQuery` | YES | NO |
| V1 final drawing availability | V1 active-drawing query | `LibraryBlockAvailabilityFlow`, `AutoCadLibraryBlockQuery` | YES | NO |
| neutral capture seam Plugin | AutoCAD adapter | `RackSourcePlacementCaptureAdapter.Capture` | YES | NO |

```text
I-55 G14 FOUNDATION READINESS = READY TO CONSUME AFTER I-59 INTEGRATION
LOCAL RECONSTRUCTION REQUIRED BY I-55 = NO
I-55 G14 = NOT UNBLOCKED
```

Rigid, Orthographic, Relative Frame Window, accept/reject, remedy, anchor/placement, mensajes y
transacciones permanecen policy de I-55 y no aparecen en Foundation.

## CT59-17 — identidad del draft

Ruta: `docs/automation/evidence/I-59-f4/FOUNDATIONS-auth08-auth12-draft.md`.

El draft contiene exactamente los diez campos vigentes y describe solo codigo existente. Su blob se
registra en la evidencia posterior al cierre.

```text
F4 verification basis = F4 product/closure SHA
Final Candidate re-verification = REQUIRED before publication
```

## Owner Validation propuesta para el futuro Candidate

ADR-0044 exige Owner Validation cuando cambia una ruta de dibujo, y la guia §§6–7.2 advierte que suites
verdes no cubren DWG, transaccion o bloques reales. El refactor comparte el cache activo del importador,
por lo que la asignacion correcta deja de ser N/A.

| OV-id | Escenario | Resultado esperado |
|---|---|---|
| OV-I59-01 | dibujo sin una definicion; key presente en `blocks-library.dwg`; ejecutar una ruta real que importe | se importa una vez, la query final V1 ve `Found` y el dibujo continua sin regresion visible |
| OV-I59-02 | key ausente del dibujo y tambien de una biblioteca consultable | no crash; no se fabrica import/presencia; query final V1 conserva `Missing` |
| OV-I59-03 | ruta/archivo de biblioteca ausente | no crash; availability externa `FileMissing`, block presence `Unknown`; conducta historica de placement/report permanece bajo policy del consumidor |
| OV-I59-04 | dos ejecuciones consecutivas con la misma biblioteca real, ejercitando el cache compartido | mismo resultado visible e import/final query; sin stale data ni regresion por extraer el cache |

Asignacion: I-59, a ejecutar una sola vez sobre el futuro `FINAL_CANDIDATE_SHA` con AutoCAD y biblioteca
identificados por version/ruta/SHA-256. No se ejecuta en F4 y no se sustituye con OV de I-55 ni ID19.

## Desviaciones y decisiones

```text
DEVIATIONS          = NONE
MATERIAL DECISIONS  = NONE
A-n                 = NONE
READY-06            = NOT EMITTED
READY               = NOT OPEN
```
