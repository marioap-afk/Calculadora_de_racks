# I-59 F3 — correccion CR59-F3-01

```text
Unit                  = I-59
Workflow              = V2
Claim-Id              = 9d3b2b65-7d0b-4db0-9233-0b6d63f3d63a
Gate                   = F3 corrective round
PREVIOUS_F3_CLOSURE    = f87cad524b524305e42048452c5681b5d11b1a50
PREVIOUS_PUBLICATION   = ff10af0a0744bdf2eb4378633e0143a6005308fa
CORRECTION_RED_SHA     = ed1b4f5704e2c79c59676f52eece98956496eeda
NEW_F3_CLOSURE_SHA     = aef8bd2c9d9a74ca1d1a1fd03624aa3207ee12f1
CR59-F3-01             = RESOLVED
F4                     = NOT OPEN
```

La evidencia anterior
[`I-59-f3-consumer-compatible-integration.md`](I-59-f3-consumer-compatible-integration.md)
se conserva sin reescritura como ronda historica. Esa ronda descubrio el seam, pero su conclusion de
que `ILibraryBlockQuery` podia alimentar presencia de biblioteca V2 fue rechazada por CR59-F3-01 y no
se usa como cierre aceptable.

## Autoridad y preflight

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
| HEAD inicial | `ff10af0a0744bdf2eb4378633e0143a6005308fa` exacto |
| rama / upstream | `architecture/shared-view-placement-block-facts` / `origin/architecture/shared-view-placement-block-facts` |
| `origin/main` | `c75e7434a909d396c05e55c39a140ba53be9e98c`; sin avance antes de modificar, cerrar o publicar |
| divergencia inicial | `origin/main...HEAD = 0/19` |
| arbol / stash / operaciones incompletas | limpio / vacio / ninguna |
| Claim-Id | coincide exactamente |
| Freeze commit/path/blob | coincide exactamente |
| A-n | NONE |

## Discovery delta: inventario semantico

No se encontro contradiccion con P-20/P-21 del Freeze. Esas clausulas distinguen availability de la
biblioteca externa y presencia de la key en esa biblioteca; no autorizan deducir ninguna desde el
dibujo activo.

| Hecho | Boundary y fuente real | Semantica preservada |
|---|---|---|
| presencia en dibujo activo | `AutoCadLibraryBlockQuery` consulta `BlockTable.Has` sobre el `Database` activo | solo V1 `Found/Missing`; no acredita la biblioteca externa |
| disponibilidad de biblioteca externa | `BlockLibraryLocator.ResolvePath` + existencia/apertura de `blocks-library.dwg` | `Ok`, `FileMissing` o `Unknown` |
| presencia de bloque en biblioteca externa | `AutoCadExternalLibraryBlockQuery` consulta `sourceTable.Has` en el side database del DWG externo | `Present`/`BlockMissing` solo con biblioteca `Ok` y observacion independiente; de otro modo `Unknown` |
| resultado de importacion | `BlockLibraryImporter` clona definiciones encontradas y devuelve `LibraryBlockImportResult` | intento/conteo de importacion; no sustituye ninguna observacion |
| presencia final en dibujo activo | query V1 ejecutada por `LibraryBlockAvailabilityFlow` despues del import | autoridad final V1 para placement historico |

El flujo V1 permanece `import -> final active-drawing query`. El flujo V2 mantiene query/import
separados, pero su query concreta de Plugin observa directamente la biblioteca externa. Ambos accesos
reutilizan la autoridad unica `BlockLibraryLocator` y `BlockLibraryDatabaseCache`; no hay un segundo
path, parser o cache.

## RED correctivo preservado

Fuente exacta:

```powershell
git show ed1b4f5704e2c79c59676f52eece98956496eeda:tests/RackCad.Tests/I59F3CorrectionTests.cs
git show ed1b4f5704e2c79c59676f52eece98956496eeda:tests/RackCad.Tests/I59F3ConsumerIntegrationTests.cs
```

Comando:

```powershell
dotnet test tests/RackCad.Tests/RackCad.Tests.csproj `
  --filter "FullyQualifiedName~I59F3ConsumerIntegrationTests|FullyQualifiedName~I59F3CorrectionTests" `
  --logger "console;verbosity=minimal" --no-restore
```

Resultado: **21 selected / 8 PASS / 13 FAIL / 0 SKIP**. Fallaron exclusivamente las obligaciones
nuevas: ausencia del adapter invalido, clasificador de hechos externos, observer Plugin independiente
y cache compartido. Los casos impiden estos falsos positivos:

- drawing `Found` + library `FileMissing` no implica external `Present`;
- drawing `Found` + library `Unknown` no implica external `Present`;
- drawing `Missing` + library `Ok` sin observacion externa no implica `BlockMissing`;
- `FileMissing` o library `Unknown` deja external block presence en `Unknown`;
- `Present`/`BlockMissing` requieren observacion independiente sobre una biblioteca consultable.

## Correccion productiva

| Archivo | Cambio |
|---|---|
| `src/RackCad.Application/Systems/Shared/LibraryPieceRequirementsV2.cs` | elimina `LibraryPieceAvailabilityQueryV1Adapter`; añade observacion externa neutral y clasificador que rechaza combinaciones incoherentes |
| `src/RackCad.Plugin/Drawing/AutoCadExternalLibraryBlockQuery.cs` | observa path, apertura y `sourceTable.Has` del DWG externo sin consultar el dibujo activo ni decidir policy |
| `src/RackCad.Plugin/Drawing/BlockLibraryDatabaseCache.cs` | extrae el cache/parser existente para compartirlo entre observer e importer |
| `src/RackCad.Plugin/Drawing/BlockLibraryImporter.cs` | usa el cache compartido; clonacion, resultado y orden V1 permanecen iguales |
| `tests/RackCad.Tests/I59F3CorrectionTests.cs` | RED/GREEN conductual y guardas del boundary correctivo |
| `tests/RackCad.Tests/I59F3ConsumerIntegrationTests.cs` | reemplaza el falso bridge por query externa independiente y preserva compatibilidad CT59-15 |

No se introdujo warning/abort/place/retry, facade universal, registry, policy, cambio en I-52/I-55,
schema ni persistencia.

## CT59-15 repetida

| Obligacion | Resultado |
|---|---|
| V1 source/API compatibility | PASS |
| V1 behavior compatibility | PASS |
| AUTH-08 V1 unchanged | PASS |
| AUTH-12 V1 unchanged | PASS |
| Preparation V1 unchanged | PASS |
| V1 import -> final drawing query authority | PASS |
| V2 coexiste aditivamente con V1 | PASS |
| V2 preserva identidad pieza/vista con keys repetidas | PASS |
| V2 no reconstruye carriers desde V1 | PASS |
| library availability y library block presence permanecen ejes neutrales | PASS |
| captura V2 disponible en Plugin sin consumer policy | PASS |
| I-52-compatible | PASS |
| I-55 G8/G9/G12-compatible | PASS |
| external library fact is not inferred from active drawing presence | **PASS** |

## GREEN local sobre el nuevo cierre

Arbol limpio, SDK resuelto `8.0.423`, HEAD exacto
`aef8bd2c9d9a74ca1d1a1fd03624aa3207ee12f1`:

| Evidencia | Resultado |
|---|---|
| focal correctivo + F3 completo | 21 selected / 21 PASS / 0 FAIL / 0 SKIP |
| F2+F3 relevante | 82 selected / 82 PASS / 0 FAIL / 0 SKIP |
| Shared View Foundation relevante | 153 selected / 153 PASS / 0 FAIL / 0 SKIP |
| I-52 + I-55 G8/G9/G12 focales | 2 selected / 2 PASS / 0 FAIL / 0 SKIP |
| Core Full | 11317 PASS / 0 FAIL / 0 SKIP |
| Build UI Debug | PASS; 0 errores / 0 advertencias |
| Build Plugin Debug | PASS; 0 errores / 2 `MSB3277` conocidos de referencias AutoCAD |
| UI Full local | N/A por LC-UI: F3 exige Core Full, no Full completo ni UI local. UI Full del CI exact-SHA fue obligatoria y paso. |

```text
SCHEMA DIFF           = NONE
PERSISTENCE MIGRATION = NONE
A-n                   = NONE
```

## CI exact-SHA del cierre

```text
CI RUN     = 35967787328
event      = push
ref/branch = refs/heads/architecture/shared-view-placement-block-facts
head_sha   = aef8bd2c9d9a74ca1d1a1fd03624aa3207ee12f1
result     = success
```

URL: https://github.com/marioap-afk/Calculadora_de_racks/actions/runs/35967787328

| Job requerido | Resultado |
|---|---|
| Tests (Domain + Application) | success |
| UI Tests (WPF controls, net8.0-windows) | success |
| Build UI (WPF, valida API de Application) | success |
| Build Plugin without AutoCAD | success |

Las anotaciones de Node 20 pertenecen al runner. Cobertura se omitio por la cadencia normal de rama
no-Candidate. Ningun job requerido fallo.

## Estado presentado

```text
CR59-F3-01 = RESOLVED
F3 = COMPLETE / PENDING COORDINATOR REVIEW
F4 = NOT OPEN
IMPLEMENTATION AUTHORIZATION = NO / F4 NOT AUTHORIZED
```
