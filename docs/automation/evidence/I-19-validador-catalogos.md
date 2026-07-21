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
- `dotnet test` (suite completa): **839 pruebas, 839 superadas, 0 fallidas, 0 omitidas** (48 nuevas de I-19
  sobre la línea base de 791 en `de72287`; la primera ronda dejó 822, la ronda de correcciones sube a 839).

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
**90 bloques**, huella `0352c75e8c258769a5abbcc1ec92dc881f55bab29acefbe5055f7cb2926e174a`.

> La huella cambió respecto de la primera ronda (`540d623b…`) porque el manifiesto ahora exige los
> parámetros dinámicos reales por bloque (LONGITUD, PERALTE, ALTURA, SAQUE, FRENTE/FONDO), no sólo los
> `paramX`/`paramY` del layout. El número de bloques (90) no cambia.

### Parámetros esperados por bloque (muestra sobre el catálogo real)

| Pieza | FRONTAL | LATERAL | PLANTA |
|---|---|---|---|
| Riel (`RIEL_DE_CINTA_CALIBRE_12`) | — | `LONGITUD` | — |
| Poste (`POSTE_OMEGA_…`) | `LONGITUD`,`PERALTE` | `LONGITUD` | `PERALTE` |
| Separador (`SEPARADOR_…`) | — | `LONGITUD` | `LONGITUD` |
| Larguero (`LARGUERO_ESCALON_CAL14_3_REMACHES`) | `LONGITUD`,`PERALTE` | — | `LONGITUD`,`PERALTE` |
| Placa base (`PLACA_BASE_…`) | `PERALTE` | `PERALTE` | `PERALTE` |
| Tarima (`TARIMA_GENERICA`) | `LONGITUD`,`ALTURA` | `LONGITUD`,`ALTURA` | — |
| Ménsula (`MENSULA_3_REMACHES_CAL_10`) | ∅ | ∅ | ∅ |

El poste exige `PERALTE` en FRONTAL/PLANTA pero **no** en LATERAL (exactitud por vista); la ménsula y la
placa **no** exigen `LONGITUD` (bloques ajenos). Verificado en `CatalogBlockParametersTests`.

## Correcciones de la revisión (ronda 2)

| Defecto | Corrección | Prueba |
|---|---|---|
| 1. Exactitud de parámetros | El manifiesto deriva los parámetros por **uso exacto** de bloque (pieceId+view+blockName), no por PieceId global; los `paramX`/`paramY` sólo aplican a la misma pieza y vista | `CatalogBlockManifestTests.BuildExpected_LayoutParams_ApplyOnlyToTheSamePieceAndView`; `CatalogBlockParametersTests.ExpectedParameters_Post_IsViewExact` |
| 2. Parámetros dinámicos reales | Fuente compartida `CatalogBlockParameters` con nombres de los constantes de dominio (`SelectiveRackDefaults`/`SelectiveSafetyDefaults`) que los productores también usan; incluye `LONGITUD` del riel/postes/separadores, `PERALTE`, `ALTURA`, `SAQUE`, `FRENTE`/`FONDO` | `CatalogBlockParametersTests.*` (incl. `Manifest_ExpectsEveryParameterTheRailBuilderActuallyApplies`, guardia contra divergencia) |
| 3. Versión y huella | `Compare` aborta ante esquema incompatible (`MANIFEST_SCHEMA_INCOMPATIBLE`) y marca huella ausente/alterada (`MANIFEST_FINGERPRINT_MISMATCH`) | `CatalogBlockManifestTests.Compare_IncompatibleSchemaVersion_IsErrorAndAborts` / `_MissingFingerprint_IsError` / `_TamperedJson_…` / `_ValidLibraryManifest_HasNoIssues` |
| 4. Campos obligatorios vacíos | `ConnectionLayoutEntry` con `PieceId`/`ConnectionPointId`/`View` vacío = error (`EMPTY_LAYOUT_FIELD`); el bloque genérico sin pieza sigue siendo advertencia (`UNRESOLVED_BLOCK_PIECE`) | `CatalogValidatorTests.Validate_LayoutWithEmpty{PieceId,ConnectionPointId,View}_IsError` + `…AllMandatoryFields_HasNoEmptyFieldError` |

Consolidación de nombres (defecto 2): los literales `"LONGITUD"`/`"PERALTE"` dispersos en
`LateralHeaderParameters`, `DynamicSystemLateralBuilder` y `FlowBedLateralBuilder`, y los nombres `SAQUE`/
`FRENTE`/`FONDO` de `SelectiveSafetyPlacement`, ahora referencian las constantes de dominio. Comportamiento
idéntico (la suite completa lo respalda).

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
