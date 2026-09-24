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

## Correccion del harness y conservacion durable del RED

El RED historico sigue siendo evidencia inmutable del commit
`cbe817e73c9dc731ce44ec530c35f1e8c37fdc2b`: **36 selected / 6 PASS / 30 FAIL / 0 SKIP**.
Esta seccion no reemplaza, reinterpreta ni oculta ese resultado. La fuente exacta del harness ejecutado
queda recuperable sin depender del tip:

```powershell
git show cbe817e73c9dc731ce44ec530c35f1e8c37fdc2b:tests/RackCad.Tests/I59F1CharacterizationTests.cs
```

Disposicion de findings:

| Finding | Correccion aplicada al harness activo | Obligacion futura |
|---|---|---|
| CR59-F1-01 | el caso `(2,3)` comprueba M11=2, M22=3, determinant=6 y `NonUniformScale`; se retiro `sqrt(6)` | V2 debe conservar X/Y/Z + `IsUniformScale=false`, sin scalar uniforme inventado |
| CR59-F1-02 | se retiraron `SharedPublicTypes`, scans de namespace y busquedas de cualquier property/enum | cada CT59 se compilara contra el carrier acordado al abrir F2; hasta entonces es RED documental |
| CR59-F1-03 | se retiraron nombres `UnitHalfTurn`, `UnitReflectionXY`, `UnitNegativeZ` y el mapping presentado como historia | Proposal V1 separa unit/uniform/reflection/half-turn/negative-Z; la tabla de roles es propuesta a revisar |

Matriz corregida del contrato actual:

| Area | Casos activos | Oraculo concreto |
|---|---|---|
| AUTH-08 supported | identity, rotation/translation/uniform, half-turn matrix, reflection matrix | `RackTransformFacts.Describe` / `RackTransformDescription` |
| AUTH-08 non-uniform | ejes 2/3, determinant 6, failure typed | `Transform2D` + `RackTransformFailure.NonUniformScale` |
| AUTH-08 angle | `-pi` actual queda `-pi` como gap medido | `RackTransformDescription.RotationRadians` |
| AUTH-08 tolerance | NaN, +Infinity, negativa | `RackTransformFailure.InvalidTolerance` |
| AUTH-12 extraction | blank omitido y key deduplicada V1 | `RackBlockRequirementExtractors.HeaderRun` |
| AUTH-12 trace source | PieceId/View/Role/key blank antes de extraction | `HeaderBlockInstance` |
| AUTH-12 requirement | blank rechazado; equality por key | `LibraryBlockRequirement` |
| AUTH-12 availability | Found/Missing y import antes de query final | contracts V1 concretos |

Corrida focal del harness corregido:

```powershell
dotnet test tests/RackCad.Tests/RackCad.Tests.csproj --filter `
  FullyQualifiedName~I59F1CharacterizationTests --no-restore -v:minimal
```

Resultado: **14 seleccionadas; 14 PASS; 0 FAIL; 0 SKIP**. La seleccion mayor que cero queda demostrada.
No se añadieron skips ni tests activos intencionalmente rojos.

La Proposal V1 completa vive en `docs/initiatives/I-59-proposal-v1.md`. Congela solo como propuesta,
`Frozen: NO`, los carriers, facts independientes, failure semantics, compatibility, invariantes y gates.
Los 30 fallos historicos no se declaran resueltos por este verde: demuestran gaps candidatos; los oraculos
defectuosos se sustituyen por CT59 verificables contra una API aun pendiente de acuerdo.

## Preflight de la sesion de correccion

Fecha: 2026-09-23, mismo worktree exclusivo.

| Hecho | Resultado |
|---|---|
| fetch/prune/tags | PASS |
| HEAD inicial | `cbe817e73c9dc731ce44ec530c35f1e8c37fdc2b` |
| upstream | `origin/architecture/shared-view-placement-block-facts`; `0/0` |
| arbol / stash / operaciones incompletas | limpio / vacio / ninguna |
| `origin/main` | `c75e7434a909d396c05e55c39a140ba53be9e98c`, igual a BASE_SHA; no avanzo |
| `origin/main...HEAD` | `0` detras / `4` delante |
| Claim-Id | verificado exactamente en claim, bootstrap y tip |

La evidencia Core Full exact-SHA y el CI propio de la punta publicada se leen despues del commit de
entrega, conforme a la regla de identidad. Si no quedan verdes, F1 closure no se presenta al Coordinator.

## Revision Architect V2 durable

Esta seccion conserva literalmente la disposicion minima emitida sobre V2; no añade conclusiones al
Architect ni reabre F1.

```text
Reviewed Proposal commit = ca54fca27cf639908d890e940e3d1d5b313780d3
Reviewed Proposal path   = docs/initiatives/I-59-proposal-v2.md
Reviewed Proposal blob   = d4949ac420fa66a2b542575a5ee9e7a0d48eaae0
Review-package tip       = 49ed90daa9264c37167657ec36ac8cb1280d1a0b
Role                     = SEPARATE SESSION
Reviewer and author same person = YES
GLOBAL VERDICT           = CHANGES REQUIRED
CR59-V1-01               = RESOLVED
CR59-V1-02               = RESOLVED
CR59-V1-03               = RESOLVED
AR59-V2-24               = REQUIRED
AR59-V2-25               = REQUIRED
OPTIONAL-01              = precision de finitud de Point3D/Vector3D
OPTIONAL-02              = actualizar contrato mutable I-59
Omitted Discovery expansion = NONE
Owner decision required  = NO
Architect                = CHANGES REQUIRED
```

AR59-V2-24 exige planificar y verificar la entrada factual AUTH-08/AUTH-12 de `docs/FOUNDATIONS.md`
antes de READY-04 y publicarla solo en el cierre documental. AR59-V2-25 exige una unica semantica de
reflection XY: primero clasificar signos por eje y, para inputs no degenerados, reflection si y solo si
los signos X/Y son opuestos, sin aplicar una segunda tolerancia al producto. OPTIONAL-01 corrige que
los carriers 3D no rechazan por constructor valores no finitos. OPTIONAL-02 mantiene el contrato mutable
en su superficie de estado, enlaces y coordinacion.
