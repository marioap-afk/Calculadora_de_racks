# I-59 F3 — consumer-compatible Foundation integration

```text
Unit           = I-59
Workflow       = V2
Claim-Id       = 9d3b2b65-7d0b-4db0-9233-0b6d63f3d63a
Gate           = F3
F3_START_SHA   = 37a23a99e7d1e912acd690f5ecc101d9a5d9e26b
RED_SHA        = 4fece64e7efd953248eb430293591c13c21246c1
F3_CLOSURE_SHA = f87cad524b524305e42048452c5681b5d11b1a50
F4             = NOT OPEN
```

## Autoridad y preflight

Freeze inmutable aplicado:

```text
FREEZE_COMMIT_SHA = a7133b3f12cac8e4c763584c02530faa03ef29ae
FREEZE_PATH       = docs/initiatives/I-59-proposal-v3.md
FREEZE_BLOB_SHA   = 3237376922ae24a94995c06c1c3ec5f74bdbdd2f
A-n               = NONE
```

Preflight del 2026-09-24 en el worktree exclusivo
`C:\Users\alejandra-mendoza\.codex\worktrees\i-59-f1-characterization\Calculadora de racks`:

| Hecho | Resultado |
|---|---|
| fetch/prune/tags | PASS |
| rama / upstream | `architecture/shared-view-placement-block-facts` / `origin/architecture/shared-view-placement-block-facts` |
| HEAD inicial | coincide exactamente con `F3_START_SHA` |
| `origin/main` | `c75e7434a909d396c05e55c39a140ba53be9e98c`; no avanzo antes de modificar ni antes de publicar cierre |
| divergencia inicial | `origin/main...HEAD = 0/16` |
| arbol / stash / operaciones incompletas | limpio / vacio / ninguna |
| Claim-Id | coincide exactamente |
| Freeze commit/path/blob | coincide exactamente |
| A-n aplicable | NONE |

## Discovery delta y decision

El inventario cubrio los contratos integrados en la rama, las suites Shared, I-52
`origin/feature/rackmirror-espejo-semantico` e I-55 `origin/feature/creacion-de-vistas`:

| Frontera | Consumidores/productores observados | Disposicion F3 |
|---|---|---|
| AUTH-08 V1 | `RackTransformFacts`, `Transform2D`, suites Shared e I-52-compatible | conservar API y conducta; V2 coexiste sin reemplazo |
| preparacion V1 | `RackViewPreparationPorts`, `RackViewPreparationAdapter`, suites F5/F6/I-58 e I-55 `RackViewBatchProducts` | `Prepare` permanece intacto; `PrepareV2` ya era seam aditivo suficiente |
| extraction V1 | `RackBlockRequirementExtractors.HeaderRun/Cantilever`, importer y preparacion | conservar dedup V1; V2 usa directamente extractor contextual por instancia |
| query/import V1 | `AutoCadLibraryBlockQuery`, `BlockLibraryImporter`, `LibraryBlockAvailabilityFlow`, drawers/writers e I-55 placement | mantener query/import/final-query authority sin cambios |
| availability V2 | no habia implementacion productiva de `ILibraryPieceAvailabilityQuery` | faltaba un bridge neutral sobre `ILibraryBlockQuery` V1 |
| import V2 | `LibraryPieceAvailabilityFlowV2` ya recibe `ILibraryBlockImporter` V1 | ningun adapter adicional necesario |
| captura Plugin | `RackSourcePlacementCaptureAdapter.Capture` interno al assembly Plugin | ya disponible al futuro G14, sin clasificacion ni policy |

Decision: añadir solamente `LibraryPieceAvailabilityQueryV1Adapter` en Application. El bridge recibe
la disponibilidad de biblioteca como hecho explicito del caller; no la deduce de presencia en el dibujo.
`Found` conserva `Present`. `Missing` solo se convierte en `BlockMissing` cuando la evidencia de biblioteca
es `Ok`; con `Unknown` o `FileMissing` la presencia queda `Unknown`. No captura exceptions, no decide
warning/abort/place/retry y no crea una segunda autoridad.

No fue necesario añadir facade, registry, factory de kinds, policy, persistencia ni un cambio en I-52/I-55.

## RED ejecutable preservado

Fuente exacta:

```powershell
git show 4fece64e7efd953248eb430293591c13c21246c1:tests/RackCad.Tests/I59F3ConsumerIntegrationTests.cs
```

Comando:

```powershell
dotnet test tests/RackCad.Tests/RackCad.Tests.csproj `
  --filter FullyQualifiedName~I59F3ConsumerIntegrationTests `
  --logger "console;verbosity=minimal"
```

Resultado: **8 selected / 6 PASS / 2 FAIL / 0 SKIP**. Los dos fallos exigian el tipo concreto
`LibraryPieceAvailabilityQueryV1Adapter`, ausente antes de produccion. Las seis pruebas que ya pasaban
acreditaron que V1, preparacion, coexistencia V2, fixtures I-52/I-55 y captura Plugin no necesitaban
reescritura.

## Implementacion y alcance

| Archivo | Cambio |
|---|---|
| `src/RackCad.Application/Systems/Shared/LibraryPieceRequirementsV2.cs` | bridge neutral V1 query -> observaciones V2 con disponibilidad explicita |
| `tests/RackCad.Tests/I59F3ConsumerIntegrationTests.cs` | RED/GREEN conductual de CT59-15 y compatibilidad cruzada |

```text
SCHEMA DIFF           = NONE
PERSISTENCE MIGRATION = NONE
A-n                   = NONE
```

No se modificaron Plugin productivo, I-52, I-55, AUTH-15, RACKMIRROR, geometry/BOM, naming, DTO,
schema/store, FOUNDATIONS, HANDOFF, ROADMAP ni el Freeze.

## CT59-15 y compatibilidad

| Obligacion | Evidencia | Resultado |
|---|---|---|
| V1 source/API compatibility | los tests F3 compilan contra las firmas V1; Core Full | PASS |
| V1 behavior compatibility | tests F1/F3 + Shared F6 | PASS |
| AUTH-08 V1 unchanged | non-uniform sigue unsupported; `-pi` sigue historicamente negativo | PASS |
| AUTH-12 V1 unchanged | extractor conserva filtrado/dedup y equality por key | PASS |
| Preparation V1 unchanged | mismo `RackPreparedView`, payload y requirements V1 | PASS |
| Query/import/final-query authority | import precede a la unica query autoritativa final | PASS |
| V2 coexiste con V1 | mismo proceso y mismo port entregan resultado V1 y V2 aditivo | PASS |
| V2 preserva identidad | repeated key produce una key I/O y multiples PieceId/ViewAddress | PASS |
| V2 no reconstruye desde V1 | blank y repeated carriers sobreviven aunque V1 los omita/dedup | PASS |
| bridge availability neutral | library fact explicito gobierna si `Missing` puede ser `BlockMissing` | PASS |
| captura V2 disponible en Plugin | seam concreto copia Position/Scale/Normal/Origin y no clasifica | PASS |
| I-52-compatible | fixture V1 reflection/determinant/uniform scale | PASS |
| I-55 G8/G9/G12-compatible | V1 missing-after-import y orden import/query intactos | PASS |

## GREEN local sobre SHA de cierre

Arbol limpio, SDK resuelto `8.0.423`, HEAD exacto
`f87cad524b524305e42048452c5681b5d11b1a50`:

| Evidencia | Resultado |
|---|---|
| focal F3 | 8 selected / 8 PASS / 0 FAIL / 0 SKIP |
| F2+F3 + Shared View Foundation + I-58 seam | 162 selected / 162 PASS / 0 FAIL / 0 SKIP |
| Core Full | 11304 PASS / 0 FAIL / 0 SKIP |
| Build UI Debug | PASS; 0 errores / 0 advertencias |
| Build Plugin Debug | PASS; 0 errores / 2 `MSB3277` conocidos de referencias AutoCAD |
| UI Full local | N/A por LC-UI de `AGENTS.md`: F3 exige Core Full, no Full completo ni UI local. La clase UI del CI exact-SHA sigue siendo obligatoria y paso. |

## CI exact-SHA de cierre

```text
CI RUN     = 35964044990
event      = push
ref/branch = refs/heads/architecture/shared-view-placement-block-facts
head_sha   = f87cad524b524305e42048452c5681b5d11b1a50
result     = success
```

URL: https://github.com/marioap-afk/Calculadora_de_racks/actions/runs/35964044990

| Job requerido | Resultado |
|---|---|
| Tests (Domain + Application) | success |
| UI Tests (WPF controls, net8.0-windows) | success |
| Build UI (WPF, valida API de Application) | success |
| Build Plugin without AutoCAD | success |

Las anotaciones son deuda xUnit y avisos de runner preexistentes. La cobertura se omitio por cadencia
normal de rama no-Candidate. Ningun job requerido fallo.

## Estado publicado

```text
F3 = COMPLETE / PENDING COORDINATOR REVIEW
F4 = NOT OPEN
IMPLEMENTATION AUTHORIZATION = NO / F4 NOT AUTHORIZED
```
