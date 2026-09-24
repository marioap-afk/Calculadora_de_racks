# I-59 F1 — Characterization AUTH-08 / AUTH-12

```text
Unit       = I-59
Workflow   = V2
Claim-Id   = 9d3b2b65-7d0b-4db0-9233-0b6d63f3d63a
Scope      = F1 CHARACTERIZATION / RED ONLY
Production = UNCHANGED
Plugin     = UNCHANGED
Schema     = UNCHANGED
Persistence= UNCHANGED
F2         = NOT OPEN
```

## Preflight local

Fecha: 2026-09-23. Worktree exclusivo:
`C:\Users\alejandra-mendoza\.codex\worktrees\i-59-f1-characterization\Calculadora de racks`.

| Hecho | Resultado |
|---|---|
| `git fetch origin --prune --tags` | PASS |
| Rama | `architecture/shared-view-placement-block-facts` |
| Upstream | `origin/architecture/shared-view-placement-block-facts` |
| HEAD inicial | `3d4c1dc5aba4d228e2b0a1fb8643803f08dfa55f` |
| Arbol inicial | limpio |
| Stash | vacio |
| Operaciones Git incompletas | ninguna |
| `origin/main...HEAD` | `0` detras / `3` delante |
| Claim original | `ec6ccc459532c71e0ed85e0b410f6a0c6ab7f25f` |
| Claim-Id | presente en claim, bootstrap y tip; coincide exactamente |

Baseline previo a F1:

| Suite | Resultado |
|---|---|
| Core Full | 11221 PASS / 0 FAIL / 0 SKIP; 2 m 55 s |
| UI Full | 1581 PASS / 0 FAIL / 17 SKIP; 1598 total; 14 m 51 s |

El primer intento Core con `--no-restore` fallo porque el worktree nuevo no tenia
`obj/project.assets.json`; la invocacion canonica sin ese flag restauro y produjo el baseline verde.
Las advertencias xUnit preexistentes coinciden con la deuda declarada por el repositorio.

## Matriz ejecutable

Archivo: `tests/RackCad.Tests/I59F1CharacterizationTests.cs`.

### AUTH-08

| Grupo | Casos | Estado actual |
|---|---|---|
| transform | identidad; rotacion + traslacion + escala uniforme; media vuelta; reflexion; no uniforme | 4 PASS / 1 RED |
| angle | `-pi` canonico debe resultar `+pi` | 1 RED |
| scale | Unit; UnitHalfTurn; UnitReflectionXY; UnitNegativeZ; NonUnitUniform; NonUniform | 6 RED |
| normal / origin / position | Position XYZ; escala por eje; Normal; NormalIsWorldZ; Origin XYZ | 5 RED |

La matriz conserva `Transform2D` como autoridad matematica 2D. El RED no decide aceptacion de
proyeccion: exige que los hechos de fuente sigan observables antes de que un consumidor aplique policy.

### AUTH-12

| Grupo | Casos | Estado actual |
|---|---|---|
| role | Required; OptionalVisual; NotApplicable | 3 RED |
| requirement traceability | PieceId; ViewAddress/View; Role; LibraryKey/Key | Key PASS; 3 RED |
| repeated key | dos piezas/vistas con la misma clave no colapsan su trazabilidad | 1 RED |
| blank required key | el hecho estructural se conserva | 1 RED |
| source-role mapping | Beam; Pallet; Annotation; Dimension | 4 RED |
| availability | Ok; FileMissing; Unknown | 3 RED |
| failure-cause traceability | block absent; library unavailable; unknown | block absent PASS; 2 RED |

## RED esperado y observado

Comando:

```powershell
dotnet test tests/RackCad.Tests/RackCad.Tests.csproj --no-restore `
  --filter FullyQualifiedName~I59F1CharacterizationTests -v:minimal
```

Resultado: **36 seleccionadas; 6 PASS; 30 FAIL; 0 SKIP**. La seleccion mayor que cero queda
demostrada. Los rojos observados corresponden a los huecos esperados:

- AUTH-08 rechaza `NonUniformScale`, devuelve `-pi`, solo expone traslacion 2D y escala uniforme,
  y no transporta normal, Z, origen ni clasificacion de signos;
- AUTH-12 reduce el requirement a `Key`, descarta claves vacias, deduplica por clave, no conserva
  pieza/vista/rol y solo distingue `Found`/`Missing`.

Control focal de la autoridad vigente:

```powershell
dotnet test tests/RackCad.Tests/RackCad.Tests.csproj --no-restore `
  --filter "FullyQualifiedName~RackTransformFactsTests|FullyQualifiedName~SharedViewFoundationF6Tests" -v:minimal
```

Resultado: **27 seleccionadas; 27 PASS; 0 FAIL; 0 SKIP**.

No se ejecuto GREEN y no se cambio produccion porque `IMPLEMENTATION AUTHORIZATION = NO`.

## Discrepancia material con Proposed Freeze

La rama no contiene Proposed Freeze, Freeze acordado, identidad de blob/commit ni veredictos
`Coordinator = AGREED` y `Architect = AGREED` para I-59. El unico artefacto I-59 bajo
`docs/initiatives/` es el contrato bootstrap, cuyo estado aun dice `D/F0` y cuya compuerta mantiene
`IMPLEMENTATION AUTHORIZATION = NO`.

Esto impide afirmar conformidad de nombres o forma publica contra un Freeze y prohibe abrir F2. La
orden directa de esta sesion autoriza solamente la caracterizacion/RED pedida; no sustituye el
Consensus Freeze requerido por el contrato y `INITIATIVE_LIFECYCLE.md` seccion 6.

El requisito de origen no es especulativo: el consumidor actual I-55 en
`origin/feature/creacion-de-vistas` (`afc4a864bd26514e74b1c88d47686298f26df70e`) exige para G14
`Origin != 0`, Position con Z, rotacion no nula, escala `(+1,+1,+1)` y `(-1,-1,+1)`, Normal +Z y
mutaciones que fallen si se ignora Origin. Ese hallazgo justifica incluir origin en la matriz neutral,
pero I-55 no se modifica ni se convierte en autoridad de I-59.
