# I-19 — Evidencia del validador de catálogos

Evidencia reproducible del validador de catálogos con severidades y del manifiesto esperado de
`blocks-library.dwg`. Sin AutoCAD (trabajo puro de `RackCad.Application` + pruebas).

## Cómo reproducir

Desde el worktree de la iniciativa, con el SDK de usuario (`%LOCALAPPDATA%\Microsoft\dotnet\dotnet.exe`):

```powershell
dotnet build src/RackCad.Application/RackCad.Application.csproj -v:minimal   # 0 errores, 0 advertencias
dotnet test  tests/RackCad.Tests/RackCad.Tests.csproj -v:minimal            # suite completa verde

# Sólo las pruebas de I-19:
dotnet test tests/RackCad.Tests/RackCad.Tests.csproj `
  --filter "FullyQualifiedName~CatalogValidatorTests|FullyQualifiedName~CatalogBlockManifestTests|FullyQualifiedName~ShippedCatalogIntegrityTests"
```

## Resultado de la suite (2026-07-21)

- `dotnet build` de `RackCad.Application`: **0 errores, 0 advertencias**.
- `dotnet test` (suite completa): **822 pruebas, 822 superadas, 0 fallidas, 0 omitidas** (31 nuevas de I-19
  sobre la línea base de 791 en `de72287`).

## Diagnóstico del validador sobre el catálogo REAL distribuido

Salida verbatim de `CatalogValidator.Validate(JsonRackCatalogProvider.FromBaseDirectory()).Format()`
(fijada por `ShippedCatalogIntegrityTests`):

```
Validación de catálogo: 1 error(es), 2 advertencia(s), 0 informativa(s).
- [ERROR][DuplicateId] DUPLICATE_ID — TROQUEL_TOPE: El id aparece 2 veces en Puntos de conexión; los lookups sólo ven la primera fila.
- [WARNING][InvalidReference] UNRESOLVED_BLOCK_PIECE — TARIMA_GENERICA @ FRONTAL: El bloque referencia la pieza 'TARIMA_GENERICA', que no está en ningún catálogo de piezas.
- [WARNING][InvalidReference] UNRESOLVED_BLOCK_PIECE — TARIMA_GENERICA @ LATERAL: El bloque referencia la pieza 'TARIMA_GENERICA', que no está en ningún catálogo de piezas.
```

Manifiesto esperado de `blocks-library.dwg` construido desde el catálogo:
**90 bloques**, huella `540d623b68a864629d715e0c1722aaf5d894a8cd754f983479c89d174e5d17e4`.

Lectura:

- El **error** `TROQUEL_TOPE` es un hallazgo pre-existente del catálogo distribuido (ver `docs/ideas-futuras.md`);
  I-19 lo REPORTA, no lo corrige (fuera de alcance). La prueba de integridad fija este estado conocido.
- Las **advertencias** de `TARIMA_GENERICA` son esperadas: es un bloque genérico visual sin fila de catálogo.
- El resto de categorías (referencias colgantes, vistas faltantes, filas descartadas por rol) están en **cero**
  sobre el catálogo real.

## Matriz categoría → severidad → prueba (positiva y negativa)

| Categoría (contrato) | Código | Severidad | Prueba negativa | Prueba positiva |
|---|---|---|---|---|
| IDs duplicados | `DUPLICATE_ID` | Error | `Validate_DuplicateIdWithinList_IsError` | `Validate_CleanCatalog_HasNoIssuesAndIsValid` |
| IDs duplicados (hoja secciones) | `DUPLICATE_SECCION_ID` | Error | `Validate_DuplicateIdAcrossRolesInSecciones_IsError` | `Validate_RecognizedRoles_AreNotDiscarded` |
| Referencias inválidas (ménsula) | `INVALID_MENSULA_REF` | Error | `Validate_BeamPointingAtMissingMensula_IsError` | (clean) |
| Referencias inválidas (punto conexión) | `INVALID_CONNECTION_POINT_REF` | Error | `Validate_LayoutPointingAtMissingConnectionPoint_IsError` | (clean) |
| Referencias inválidas (pieza) | `INVALID_LAYOUT_PIECE_REF` | Error | `Validate_LayoutPointingAtMissingPiece_IsError` | (clean) |
| Relaciones repetidas (layout) | `DUPLICATE_LAYOUT_KEY` | Advertencia | `Validate_DuplicateLayoutKey_IsWarning` | (clean) |
| Relaciones repetidas (bloque) | `DUPLICATE_BLOCK_KEY` | Advertencia | `Validate_DuplicateBlockKey_IsWarning` | (clean) |
| Bloques/vistas faltantes (nombre) | `MISSING_BLOCK_NAME` | Error | `Validate_BlockWithoutName_IsError` | (clean) |
| Bloques/vistas faltantes (vista bloque) | `MISSING_BLOCK_VIEW` | Error | `Validate_BlockReferencingUndefinedView_IsError` | (clean) |
| Bloques/vistas faltantes (vista layout) | `MISSING_LAYOUT_VIEW` | Error | `Validate_LayoutReferencingUndefinedView_IsError` | (clean) |
| Pieza genérica sin catálogo | `UNRESOLVED_BLOCK_PIECE` | Advertencia | `Validate_BlockWithPieceInNoCatalog_IsWarningNotError` | (clean) |
| Filas descartadas por rol | `DISCARDED_SECCION_ROW` | Advertencia | `Validate_RowWithUnknownRol_IsDiscardedWarning`; `ValidateDirectory_SurfacesDiscardedRowFromDisk` | `Validate_RecognizedRoles_AreNotDiscarded` |
| Manifiesto (bloque faltante) | `MANIFEST_MISSING_BLOCK` | Error | `Compare_MissingBlockInLibrary_IsError`; `Validate_WithLibraryManifest_FoldsManifestMismatchIntoReport` | `Compare_IdenticalManifests_HasNoIssues` |
| Manifiesto (bloque extra) | `MANIFEST_EXTRA_BLOCK` | Informativa | `Compare_ExtraBlockInLibrary_IsInfo` | `Compare_IdenticalManifests_HasNoIssues` |
| Manifiesto (parámetro faltante) | `MANIFEST_MISSING_PARAMETER` | Advertencia | `Compare_MissingExpectedParameter_IsWarning` | `Compare_IdenticalManifests_HasNoIssues` |
| Modo estricto (despliegue) | — | — | `Validate_DuplicateLayoutKey_IsWarning` (strict fatal) | `Validate_CleanCatalog_HasNoIssuesAndIsValid` |

## Integridad del catálogo y estado de `blocks-library.dwg`

- **Catálogos NO modificados:** `git diff --stat origin/main -- assets/catalogs` está **vacío**.
- **`blocks-library.dwg` NO modificado:** no está versionado (pertenece al Owner, AGENTS.md §Seguridad), no hay
  ningún `.dwg` rastreado ni en disco dentro del worktree, y el validador/manifiesto **nunca** abren un DWG. El
  manifiesto es sólo un modelo de datos comparado contra un manifiesto real provisto aparte (paso de Plugin fuera
  de alcance).
- **Golden del reparto por rol:** `ShippedSecciones_SplitByRoleIsUnchanged` confirma que extraer `SeccionRoles`
  del proveedor no cambió el split (postes=1, celosías=1, largueros=3, separadores=1).
