# I-59 F2 — neutral model + extraction/classification

```text
Unit          = I-59
Workflow      = V2
Claim-Id      = 9d3b2b65-7d0b-4db0-9233-0b6d63f3d63a
Gate          = F2
F2_START_SHA  = a7133b3f12cac8e4c763584c02530faa03ef29ae
RED_SHA       = f020d1d33c51f45561233cf80dcadd36f39efd0c
F2_CLOSURE_SHA= df2f8c4360865ea2f2b677f2f44f3c29fb06f12d
F3            = NOT OPEN
```

## Autoridad y preflight

Freeze aplicado sin modificaciones:

```text
FREEZE_COMMIT_SHA = a7133b3f12cac8e4c763584c02530faa03ef29ae
FREEZE_PATH       = docs/initiatives/I-59-proposal-v3.md
FREEZE_BLOB_SHA   = 3237376922ae24a94995c06c1c3ec5f74bdbdd2f
```

Preflight del 2026-09-23 en el worktree exclusivo
`C:\Users\alejandra-mendoza\.codex\worktrees\i-59-f1-characterization\Calculadora de racks`:

| Hecho | Resultado |
|---|---|
| fetch/prune/tags | PASS |
| rama / upstream | `architecture/shared-view-placement-block-facts` / `origin/architecture/shared-view-placement-block-facts` |
| HEAD inicial | coincide exactamente con `F2_START_SHA` |
| `origin/main` | `c75e7434a909d396c05e55c39a140ba53be9e98c`; no avanzo antes de modificar ni antes del cierre |
| divergencia inicial | `origin/main...HEAD = 0/12` |
| arbol / stash / operaciones incompletas | limpio / vacio / ninguna |
| Claim-Id | coincide exactamente |
| Freeze path/blob | coincide exactamente |
| A-n aplicable | NONE |

La comprobacion anterior al cierre mantuvo `origin/main` en el mismo SHA y dio
`origin/main...HEAD = 0/15`.

## RED ejecutable preservado

El commit de tests previo a produccion es `f020d1d33c51f45561233cf80dcadd36f39efd0c`, hijo directo del
Freeze. La fuente exacta se recupera con:

```powershell
git show f020d1d33c51f45561233cf80dcadd36f39efd0c:tests/RackCad.Tests/I59F2ContractTests.cs
```

Comando exacto:

```powershell
dotnet test tests/RackCad.Tests/RackCad.Tests.csproj `
  --filter FullyQualifiedName~I59F2ContractTests --no-restore -v:minimal
```

Resultado exacto: **57 selected / 0 PASS / 57 FAIL / 0 SKIP**. Los fallos eran los esperados: no
existian los contratos V2 concretos, el clasificador, el extractor contextual, el flujo availability ni
`PrepareV2`. No se usaron scans de namespace ni busquedas ciegas como oraculo conductual.

Al llegar a GREEN se corrigio un defecto mecanico del harness: la base usada por `DispatchProxy` estaba
declarada `sealed`. Tambien se ampliaron las fronteras de todos los doubles crudos, eje Z/normal y la
guarda concreta de ausencia de `Transform2D`. Estas correcciones no reescriben el commit ni el resultado
RED; el SHA anterior conserva su fuente y ejecucion exactas.

## Implementacion productiva F2

| Archivo | Responsabilidad F2 |
|---|---|
| `src/RackCad.Application/Systems/Shared/RackSourceTransformFactsV2.cs` | input crudo, snapshot finito, tolerancias, facts independientes y outcomes AUTH-08 |
| `src/RackCad.Application/Systems/Shared/LibraryPieceRequirementsV2.cs` | role total, requirement por instancia, proyeccion I/O, availability neutral y reattachment AUTH-12 |
| `src/RackCad.Application/Systems/Shared/RackViewPreparation.cs` | `PrepareV2` aditivo que entrega la address tipada ya validada al extractor contextual |
| `src/RackCad.Plugin/Drawing/RackSourcePlacementCaptureAdapter.cs` | copia Position/Rotation/Scale/Normal/DefinitionOrigin de AutoCAD a primitives, sin policy |

No se modificaron comandos ni producto I-55, I-52, builders, BOM, Resolve, naming, DTO, schema, store,
FOUNDATIONS, HANDOFF, ROADMAP o el Freeze.

## CT59-01..14

Tests activos: `tests/RackCad.Tests/I59F2ContractTests.cs` y
`tests/RackCad.Tests/I59F2BoundaryGuardTests.cs`.

| CT | Oraculo concreto | Resultado |
|---|---|---|
| CT59-01 | raw input conductual + referencia de assembly Application sin Autodesk + adapter Plugin de copia explicita | PASS |
| CT59-02 | `CT59_01_02_RAW_INPUT_PUBLISHES_FINITE_XYZ_NORMAL_SCALES_AND_DISTINCT_ORIGIN` | PASS |
| CT59-03 | theory `CT59_03_ROTATION_IS_CANONICAL`, incluidos `-3pi/-pi/pi/3pi` | PASS |
| CT59-04 | ejes/signos por tolerancia y non-uniform sin scalar | PASS |
| CT59-05 | determinant crudo, reflection por signos, frontera `-.010001/+.010001` y carrier concreto sin `Transform2D` | PASS |
| CT59-06 | unit/uniform/reflection/half-turn/negative-Z independientes; cada eje degenerado | PASS |
| CT59-07 | Normal intacta, WorldZ con tolerancia explicita y frontera degenerada | PASS |
| CT59-08 | cada double crudo no finito, zero/degenerate e invalid tolerance | PASS |
| CT59-09 | requirements conservan instancia; repeated key proyecta una key sin colapsar carriers | PASS |
| CT59-10 | `PrepareV2` inyecta exactamente `RackPreparedView.Address`; metadata contradictoria no manda | PASS |
| CT59-11 | theory exhaustiva de los 16 roles y `(HeaderBlockRole)999 -> UnknownSourceRole` sin parcial | PASS |
| CT59-12 | null/blank/whitespace Required permanece `KeyMissing` y no llega a I/O | PASS |
| CT59-13 | ejes `LibraryAvailability` y `LibraryBlockPresence` conservan sus estados independientes | PASS |
| CT59-14 | import separado precede query final; observacion se reatacha a cada requirement original | PASS |

La guarda Plugin es complementaria: no sustituye los tests conductuales sobre primitives y clasificacion.

## GREEN y compatibilidad

Todas las corridas de cierre de esta seccion se hicieron con arbol limpio, SDK resuelto `8.0.423` y
HEAD exacto `df2f8c4360865ea2f2b677f2f44f3c29fb06f12d`.

| Evidencia local | Resultado |
|---|---|
| focal `I59F2ContractTests|I59F2BoundaryGuardTests` | 61 selected / 61 PASS / 0 FAIL / 0 SKIP |
| relevante I-59 F1/F2 + Shared F1/F5/F6 + I-58 seam + Header guards | 226 selected / 226 PASS / 0 FAIL / 0 SKIP |
| Core Full | 11296 PASS / 0 FAIL / 0 SKIP |
| Build UI Debug | PASS; 0 errores / 0 advertencias |
| Build Plugin Debug | PASS; 0 errores / 2 `MSB3277` conocidos de referencias AutoCAD |
| UI Full local | N/A para F2 por LC-UI de `AGENTS.md`: un cierre de gate V2 exige Core Full; UI Full local solo si el gate exige Full. F2 no lo exige. La clase UI CI sigue siendo obligatoria y paso abajo. |

La compatibilidad V1 queda observada por las suites Shared relevantes y Core Full: no cambio la firma o
semantica de `RackTransformFacts.Describe`, `IRackBlockRequirementExtractor<TPayload>`, el flujo V1 ni
`RackViewPreparationAdapter.Prepare`. V2 usa carriers, extractor contextual, resultado y metodo aditivos.

Un primer Core Full sobre `69885830f83c4d2773e8ff2dc52c10b4b9ee1891` descubrio una colision de
taxonomia con la guarda cerrada Header de I-53: 11295 PASS / 1 FAIL. Los tipos nuevos se renombraron con
prefijo `RackHeader...`, sin cambiar semantica congelada ni ampliar las raices historicas. Ese SHA no se
usa como cierre; todas las evidencias validas se repitieron sobre `F2_CLOSURE_SHA`.

## Schema, persistence y alcance

```text
SCHEMA DIFF           = NONE
PERSISTENCE MIGRATION = NONE
A-n                   = NONE
```

La lista de archivos entre Freeze y cierre contiene exclusivamente los cuatro archivos productivos y
los dos archivos de test enumerados arriba.

## CI exact-SHA

```text
CI RUN     = 35961622932
event      = push
ref/branch = refs/heads/architecture/shared-view-placement-block-facts
head_sha   = df2f8c4360865ea2f2b677f2f44f3c29fb06f12d
result     = success
```

URL: https://github.com/marioap-afk/Calculadora_de_racks/actions/runs/35961622932

| Job requerido | Resultado |
|---|---|
| Tests (Domain + Application) | success |
| UI Tests (WPF controls, net8.0-windows) | success |
| Build UI (WPF, valida API de Application) | success |
| Build Plugin without AutoCAD | success |

Las anotaciones corresponden a deuda xUnit y avisos de runner preexistentes; ninguno de los cuatro jobs
fallo. La cobertura fue omitida por la cadencia normal de rama no-Candidate y no forma parte de F2.

## Estado publicado

```text
F2 = COMPLETE / PENDING COORDINATOR REVIEW
F3 = NOT OPEN
```
