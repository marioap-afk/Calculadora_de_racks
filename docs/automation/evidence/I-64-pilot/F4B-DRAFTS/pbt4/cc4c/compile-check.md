# PB-T4c: la precondición 6 del paquete de PB-T4 v5.1

**Base:** el cierre de PB-T4b, `6adff90d856026a7a51067c1e9d7248f3d81641c` (F4B-E53). Cada corrida, en una copia exportada nueva (`git archive` del checkout principal; nada más en él), fuera de todo worktree. SDK de usuario (`<datos-locales-del-usuario>\Microsoft\dotnet\dotnet.exe`), con `--no-incremental -nodeReuse:false -p:UseSharedCompilation=false`; pruebas en Debug y el Plugin en Release con `-o`; cero `warning CS`; la suite **completa** de Core y, en UI, **solo los filtros** del contrato; `build-server shutdown` al final. Ninguna corrida toca el worktree, y el checkout principal queda limpio en todas (`MAINCO []`).

**El resumen:** `pbt4c-precondition6.json` (Schema `rackcad-pbt4c-precondition6/v1`, con las claves de la v5.0 y `Rehearsal`, `Control`, `Census` y `ClassResults`), sha256 `29179a12ef8623f4c276f943a725ece6f3ffd84464f16705e1feda757efbce1a`, que escribe `p6_json_pbt4c_v51.py` (`0b10d851…b6a9`) desde los registros de abajo. **El de la v5.0,** como historia: `pbt4c-precondition6-v50.json` (`0cb67aee2b2d925d68235bb6444566b4222205a6cf97f472b3851fd0795b2f51`), con su registro, `compile-check-v50.md` (`bca9fde88b3a4ac881860769260780354a4440a48a94c9690b8c08f7acaefc44`).

**Los nombres de las salidas de la v5.1 son cortos** (`<copia>-rg/`, `-en/`, `-id/`, `-cc/`, `v51-census/` y `v51-clases/`; los filtros de UI, por su número `NN`): con el prefijo `docs/automation/evidence/I-64-pilot/F4B-DRAFTS/pbt4/cc4c/`, la ruta más larga de las 146 tiene 91 caracteres (el límite es 156). **Cada corrida trae aquí la tabla de resultados de cada clase** (Passed, Failed y Skipped), de Core y de UI, también las de la v5.0 cuyas salidas la custodia solo lleva por sha256 (`/d/r5c` y `/d/r5g`).

## Las corridas que valen (v5.1)

**Las piezas** (sha256 del texto con `\n`; las mismas en las cuatro corridas que las usan, por la cabecera de cada registro):

| Pieza | sha256 |
|---|---|
| `green_pbt4c_v51.py` | `a8a3afa2a0a71674ccd7eafc1f4e52eea183fef7cbf8bc5fd319be9dfb639215` |
| `pbt4c-green-v51.txt` | `081ef77e66a390cf3637f15851b5fe05a5cdf07f6e0065928fc5c841b250dc16` |
| `pbt4c-skeleton-v51.txt` | `f8840c67fd41c3bbdabf125d9b80941e3f3aedf5e610681c8ce15f111e7b392e` |
| `pbt4c_skips_v50.py` | `a59a3aedc982bb15312a04eb5ddbe92afa2a868a1177e7c8b4c3f49332970f7f` |
| `make_equivalents_pbt4c_v50.py` | `d81fcb41bed684b8ac35949841e3a7fba2277120bdf3e882d0267f8176e4a896` |
| `apply_red_pbt4c_v51.py` | `03dbb6a76d1e50c7c1935926727e4a5172f44c30a3cc6bc11ddac51197bd5102` |
| `apply_green_pbt4c_v51.py` | `8b9445455b60521598a113e14e870fe5218d3b1b4b142308fbac934016d9bd49` |
| `check_skips_pbt4c_v50.py` | `71521e52abdd7f371fb4b067d2b4400bb508c2d1d6a246ca83df4bb39127ee22` |
| `pbt4c-substitution-list-4.tsv` | `9bd4c17a0eda837c92a7a0b7b3562d4ab17f1608baf3ffa8efaf560d8b1f82de` |
| `pbt4c-rehearsal-textct.cs` | `ede5d822d30e8d5dc022f124a2a42f4e7cb562f53de7862be11b2d7b0b0e13ba` |
| `pbt4c-probe-editall.cs` | `0d668ed9b58d9f0d933cfdbda1e06bf7a7a9d5ad78fed661a88c7e1a9ac5285b` |
| `pbt4c-probe-close-ui.cs` | `bc9a0fb348b63aa9f639d66ccf301210af3bb2b63f799348e8808d58290649b5` |
| `p6_check_pbt4c_v50b.py` | `bb2c983b53121020df80e63655743e2cee943c318f25653362bcb8561f8d9486` |
| `rehearsal_check_v51.py` | `772f35eebc2589bfdd3d64adef7183b6109044c4d3ac8bf7f1cadc2054b20428` |
| `filter_overlap_pbt4c_v50b.py` | `d5ebc82f7cba6754872f5a24d0237b2d3010bf2f3b05dec2c52f0ec856265306` |
| `classes_v51.py` | `48e093c372ba5f98899327986020727ee164b901d87348e523787a97072edf74` |
| `trx_message_v50.py` | `6faf8fc9052c1a7185e1c5f431f40272883f66f678830a25645cb6c3f1fc646e` |
| `ui_filters_pbt4c_v50b.txt` | `19182fbc10012c160453df237ad2657568affdc586e2f7ce9cad8feccdef7815` |
| `pbt4b-contract-dump-v50.txt` | `8feddf4f40505fe740dd6cd410d6b00d38d66b10145e7c6126224c9f8e5db449` |
| `p6_pbt4c_v51.sh` | `83477d65dcf3f9461494025d950462f15308ceb9538f571222f9a2527d3d753b` |

### `/d/r5j`: los pasos 2 a 6, el RED exacto y el GREEN literal, sin las CT

Registro: `driver-r5j-rg.txt` (`1fa471d0e68095b1182c4a70b7da51ee51f9daa1b11008284fcfbe12c8045da4`) y `r5j-rg/`.

- red-core rc=0 errores=0 avisos-CS=0
- red-ui rc=0 errores=0 avisos-CS=0
- red-plugin rc=0 errores=0 avisos-CS=0
- green-core rc=0 errores=0 avisos-CS=0
- green-ui rc=0 errores=0 avisos-CS=0
- green-plugin rc=0 errores=0 avisos-CS=0
- **Los `Skip`** (`check_skips_pbt4c_v50.py`): check_skips PB-T4c: PASA (4 archivos, 4 Skip) (`r5j-rg/check-skips.txt`).
- **Core, RED:** **19 608 = 19 147 `Passed` + 0 `Failed` + 461 `Skipped`** (TRX `f8eac38a…41a9`, 736 s); core red: PASA (`r5j-rg/check-core-red.txt`).
- **Core, GREEN:** **19 608 = 19 147 `Passed` + 0 `Failed` + 461 `Skipped`** (TRX `a0d26d57…6667`, 622 s); core green: PASA (`r5j-rg/check-core-green.txt`).
- **UI:** ui red: PASA (otras Failed 0); ui green: PASA (otras Failed 0) (`r5j-rg/check-ui-red.txt` y `check-ui-green.txt`).
- **Los filtros** (`filter_overlap_pbt4c_v50b.py`, D-S8): filtros: PASA (`r5j-rg/filtros.txt`).
- **D = 0, K = 4 y E = 4.**

**Cada filtro de UI** (`NN`, por `r5j-rg/ui-filtros-r.txt` y `ui-filtros-g.txt`; resultados, Passed, Failed y Skipped):

| NN | Filtro | RED | GREEN |
|---|---|---|---|
| 01 | `FullyQualifiedName~RackCad.UI.Tests.PushBackFullDraftWindowParityTests` | 1 = 1 + 0 + 0 | 1 = 1 + 0 + 0 |
| 02 | `FullyQualifiedName~RackCad.UI.Tests.Workspace` | 672 = 644 + 0 + 28 | 672 = 644 + 0 + 28 |
| 03 | `FullyQualifiedName~RackCad.UI.Tests.PushBackWindowOnStateTests` | 2 = 2 + 0 + 0 | 2 = 2 + 0 + 0 |
| 04 | `FullyQualifiedName~RackCad.UI.Tests.PushBackClassicCharacterization` | 55 = 55 + 0 + 0 | 55 = 55 + 0 + 0 |
| 05 | `FullyQualifiedName~RackCad.UI.Tests.PushBackFullEditorCharacterization` | 18 = 16 + 0 + 2 | 18 = 16 + 0 + 2 |
| 06 | `FullyQualifiedName~RackCad.UI.Tests.PushBackStateNet` | 14 = 14 + 0 + 0 | 14 = 14 + 0 + 0 |
| 07 | `FullyQualifiedName~RackCad.UI.Tests.PushBackDeclaredCorrection` | 14 = 14 + 0 + 0 | 14 = 14 + 0 + 0 |
| 08 | `FullyQualifiedName~RackCad.UI.Tests.Cantilever` | 67 = 67 + 0 + 0 | 67 = 67 + 0 + 0 |
| 09 | `FullyQualifiedName~RackCad.UI.Tests.PushBackWorkspaceModeTests` | 3 = 3 + 0 + 0 | 3 = 3 + 0 + 0 |
| 10 | `FullyQualifiedName~RackCad.UI.Tests.PushBackWorkspaceApplyCloseTests` | 4 = 3 + 0 + 1 | 4 = 3 + 0 + 1 |
| 11 | `FullyQualifiedName~RackCad.UI.Tests.PushBackFieldValidationConcordanceTests` | 1 = 1 + 0 + 0 | 1 = 1 + 0 + 0 |
| 12 | `FullyQualifiedName~RackCad.UI.Tests.PushBackWorkspaceCarryoverTests` | 3 = 3 + 0 + 0 | 3 = 3 + 0 + 0 |
| 13 | `FullyQualifiedName~RackCad.UI.Tests.PushBackFullPanelFieldTests` | 3 = 3 + 0 + 0 | 3 = 3 + 0 + 0 |
| 14 | `FullyQualifiedName~RackCad.UI.Tests.NumericFieldValidationTests` | 13 = 13 + 0 + 0 | 13 = 13 + 0 + 0 |
| 15 | `FullyQualifiedName~RackCad.UI.Tests.NumericFieldTests` | 10 = 10 + 0 + 0 | 10 = 10 + 0 + 0 |
| 16 | `FullyQualifiedName~RackCad.UI.Tests.FullPanelCopyWorkspacePilotReconciliationGreenTests` | 30 = 30 + 0 + 0 | 30 = 30 + 0 + 0 |
| 17 | `FullyQualifiedName~RackCad.UI.Tests.FullPanelCopyWorkspacePilotReconciliationViewGreenTests` | 6 = 6 + 0 + 0 | 6 = 6 + 0 + 0 |
| 18 | `FullyQualifiedName~RackCad.UI.Tests.PushBackEditAllApplyCloseTests` | 1 = 0 + 1 + 0 | 1 = 1 + 0 + 0 |

**El resultado de cada clase que corre en `/d/r5j`** (la suite completa de Core y los 18 filtros de UI, en cada rama; `v51-clases/r5j.txt`, sha256 `ac7a6beb…74f1`; Passed / Failed / Skipped):

| Clase | RED (P / F / S) | GREEN (P / F / S) |
|---|---|---|
| `RackCad.Tests.AgentExecutionProtocolTests` | 17 / 0 / 0 | 17 / 0 / 0 |
| `RackCad.Tests.AiscWorkbookVerificationTests` | 11 / 0 / 0 | 11 / 0 / 0 |
| `RackCad.Tests.AtomicFileTests` | 4 / 0 / 0 | 4 / 0 / 0 |
| `RackCad.Tests.BasePlatePeraltePersistenceTests` | 2 / 0 / 0 | 2 / 0 / 0 |
| `RackCad.Tests.BeamCatalogTests` | 3 / 0 / 0 | 3 / 0 / 0 |
| `RackCad.Tests.BlankFrontBoundaryTests` | 15 / 0 / 0 | 15 / 0 / 0 |
| `RackCad.Tests.BlankFrontSafetyTests` | 9 / 0 / 0 | 9 / 0 / 0 |
| `RackCad.Tests.BlankFrontTests` | 28 / 0 / 0 | 28 / 0 / 0 |
| `RackCad.Tests.BlankIdPredicateScopeGuardTests` | 2 / 0 / 0 | 2 / 0 / 0 |
| `RackCad.Tests.BlockNamingTests` | 8 / 0 / 0 | 8 / 0 / 0 |
| `RackCad.Tests.BomBuilderTests` | 6 / 0 / 0 | 6 / 0 / 0 |
| `RackCad.Tests.BomExcelCharacterizationTests` | 5 / 0 / 0 | 5 / 0 / 0 |
| `RackCad.Tests.BomExcelPivotTests` | 14 / 0 / 0 | 14 / 0 / 0 |
| `RackCad.Tests.BomExcelSmokeFixtures` | 1 / 0 / 0 | 1 / 0 / 0 |
| `RackCad.Tests.BomExcelSystemMatrixTests` | 15 / 0 / 0 | 15 / 0 / 0 |
| `RackCad.Tests.BracingPanelMemberBuilderTests` | 11 / 0 / 0 | 11 / 0 / 0 |
| `RackCad.Tests.CallerOwnedFacadeGuardTests` | 81 / 0 / 0 | 81 / 0 / 0 |
| `RackCad.Tests.CallerOwnedTransactionGuardTests` | 15 / 0 / 0 | 15 / 0 / 0 |
| `RackCad.Tests.CantileverAdapterSectionViewTests` | 12 / 0 / 0 | 12 / 0 / 0 |
| `RackCad.Tests.CantileverAdvancedPanelLayoutTests` | 27 / 0 / 0 | 27 / 0 / 0 |
| `RackCad.Tests.CantileverArmConnectionMetricsCharacterizationTests` | 9 / 0 / 0 | 9 / 0 / 0 |
| `RackCad.Tests.CantileverArmDefaultsAndFrontalTests` | 14 / 0 / 0 | 14 / 0 / 0 |
| `RackCad.Tests.CantileverArmTests` | 117 / 0 / 0 | 117 / 0 / 0 |
| `RackCad.Tests.CantileverBraceRepresentationTests` | 32 / 0 / 0 | 32 / 0 / 0 |
| `RackCad.Tests.CantileverColumnBaseFollowTests` | 15 / 0 / 0 | 15 / 0 / 0 |
| `RackCad.Tests.CantileverColumnBaseTests` | 84 / 0 / 0 | 84 / 0 / 0 |
| `RackCad.Tests.CantileverDoubleStationLayoutTests` | 8 / 0 / 0 | 8 / 0 / 0 |
| `RackCad.Tests.CantileverLateralViewTests` | 12 / 0 / 0 | 12 / 0 / 0 |
| `RackCad.Tests.CantileverLineEditorAssemblerTests` | 10 / 0 / 0 | 10 / 0 / 0 |
| `RackCad.Tests.CantileverLineTests` | 73 / 0 / 0 | 73 / 0 / 0 |
| `RackCad.Tests.CantileverPersistenceAndViewTests` | 28 / 0 / 0 | 28 / 0 / 0 |
| `RackCad.Tests.CantileverPlantaViewTests` | 8 / 0 / 0 | 8 / 0 / 0 |
| `RackCad.Tests.CantileverPluginSourceGuardTests` | 17 / 0 / 0 | 17 / 0 / 0 |
| `RackCad.Tests.CantileverPunchRepresentationTests` | 14 / 0 / 0 | 14 / 0 / 0 |
| `RackCad.Tests.CantileverRegularPunchGridCharacterizationTests` | 16 / 0 / 0 | 16 / 0 / 0 |
| `RackCad.Tests.CantileverRetiredMarginTests` | 11 / 0 / 0 | 11 / 0 / 0 |
| `RackCad.Tests.CantileverRoundThreeCorrectionTests` | 27 / 0 / 0 | 27 / 0 / 0 |
| `RackCad.Tests.CantileverRoundTwoCharacterizationTests` | 12 / 0 / 0 | 12 / 0 / 0 |
| `RackCad.Tests.CantileverRoundTwoSourceGuardTests` | 38 / 0 / 0 | 38 / 0 / 0 |
| `RackCad.Tests.CantileverSeparatorWebTests` | 6 / 0 / 0 | 6 / 0 / 0 |
| `RackCad.Tests.CantileverSourceGuardTests` | 90 / 0 / 0 | 90 / 0 / 0 |
| `RackCad.Tests.CantileverStationCorrectionTests` | 78 / 0 / 0 | 78 / 0 / 0 |
| `RackCad.Tests.CantileverStationTests` | 81 / 0 / 0 | 81 / 0 / 0 |
| `RackCad.Tests.CantileverVerticalDatumTests` | 15 / 0 / 0 | 15 / 0 / 0 |
| `RackCad.Tests.CantileverVisualRoleTests` | 15 / 0 / 0 | 15 / 0 / 0 |
| `RackCad.Tests.CatalogBackedStandardServiceTests` | 4 / 0 / 0 | 4 / 0 / 0 |
| `RackCad.Tests.CatalogBlockManifestTests` | 13 / 0 / 0 | 13 / 0 / 0 |
| `RackCad.Tests.CatalogBlockParametersTests` | 8 / 0 / 0 | 8 / 0 / 0 |
| `RackCad.Tests.CatalogCanonicalIdsTests` | 1 / 0 / 0 | 1 / 0 / 0 |
| `RackCad.Tests.CatalogDirectoryTests` | 2 / 0 / 0 | 2 / 0 / 0 |
| `RackCad.Tests.CatalogManifestGuardTests` | 3 / 0 / 0 | 3 / 0 / 0 |
| `RackCad.Tests.CatalogStandardConsistencyTests` | 5 / 0 / 0 | 5 / 0 / 0 |
| `RackCad.Tests.CatalogValidatorTests` | 22 / 0 / 0 | 22 / 0 / 0 |
| `RackCad.Tests.CommonTransform2DTests` | 6 / 0 / 0 | 6 / 0 / 0 |
| `RackCad.Tests.ComputedParametersPopulationGuardTests` | 3 / 0 / 0 | 3 / 0 / 0 |
| `RackCad.Tests.ComputedParametersPopulationTests` | 51 / 0 / 0 | 51 / 0 / 0 |
| `RackCad.Tests.ComputedParametersRackMetricIdsTests` | 10 / 0 / 0 | 10 / 0 / 0 |
| `RackCad.Tests.ComputedParametersRackMetricProviderPurityTests` | 3 / 0 / 0 | 3 / 0 / 0 |
| `RackCad.Tests.ComputedParametersRackMetricRequestTests` | 14 / 0 / 0 | 14 / 0 / 0 |
| `RackCad.Tests.ComputedParametersSummaryContextTests` | 3 / 0 / 0 | 3 / 0 / 0 |
| `RackCad.Tests.ComputedParametersSummaryGuardTests` | 2 / 0 / 0 | 2 / 0 / 0 |
| `RackCad.Tests.ComputedParametersSummaryPerformanceTests` | 6 / 0 / 0 | 6 / 0 / 0 |
| `RackCad.Tests.ComputedParametersSummaryTests` | 18 / 0 / 0 | 18 / 0 / 0 |
| `RackCad.Tests.ComputedParametersSymbolsBindingTests` | 22 / 0 / 0 | 22 / 0 / 0 |
| `RackCad.Tests.ComputedParametersSymbolsConsumersTests` | 4 / 0 / 0 | 4 / 0 / 0 |
| `RackCad.Tests.ComputedParametersSymbolsContextTests` | 14 / 0 / 0 | 14 / 0 / 0 |
| `RackCad.Tests.ComputedParametersSymbolsFormattingTests` | 12 / 0 / 0 | 12 / 0 / 0 |
| `RackCad.Tests.ComputedParametersSymbolsIdentityTests` | 33 / 0 / 0 | 33 / 0 / 0 |
| `RackCad.Tests.ComputedParametersSymbolsPersistenceTests` | 4 / 0 / 0 | 4 / 0 / 0 |
| `RackCad.Tests.ConsolidatedBomBuilderTests` | 2 / 0 / 0 | 2 / 0 / 0 |
| `RackCad.Tests.ConsolidatedBomXlsxDetailTests` | 13 / 0 / 0 | 13 / 0 / 0 |
| `RackCad.Tests.CsvCatalogReaderTests` | 9 / 0 / 0 | 9 / 0 / 0 |
| `RackCad.Tests.CsvLexerTests` | 10 / 0 / 0 | 10 / 0 / 0 |
| `RackCad.Tests.CustomPropertiesAuthorityTests` | 87 / 0 / 0 | 87 / 0 / 0 |
| `RackCad.Tests.CustomPropertiesCommandGuardTests` | 48 / 0 / 0 | 48 / 0 / 0 |
| `RackCad.Tests.CustomPropertiesCommitTests` | 35 / 0 / 0 | 35 / 0 / 0 |
| `RackCad.Tests.CustomPropertiesEdgeGuardTests` | 52 / 0 / 0 | 52 / 0 / 0 |
| `RackCad.Tests.CustomPropertiesEnvelopeCharacterizationTests` | 103 / 0 / 0 | 103 / 0 / 0 |
| `RackCad.Tests.CustomPropertiesEnvelopeGuardTests` | 22 / 0 / 0 | 22 / 0 / 0 |
| `RackCad.Tests.CustomPropertiesEnvelopeTests` | 150 / 0 / 0 | 150 / 0 / 0 |
| `RackCad.Tests.CustomPropertiesMutationTests` | 17 / 0 / 0 | 17 / 0 / 0 |
| `RackCad.Tests.CustomPropertiesProjectTests` | 32 / 0 / 0 | 32 / 0 / 0 |
| `RackCad.Tests.CustomPropertiesStoreTests` | 228 / 0 / 0 | 228 / 0 / 0 |
| `RackCad.Tests.CustomPropertiesWorkspaceTests` | 21 / 0 / 0 | 21 / 0 / 0 |
| `RackCad.Tests.DebugFaultInjectionGuardTests` | 1 / 0 / 0 | 1 / 0 / 0 |
| `RackCad.Tests.DiagnosticsNegativeTests` | 3 / 0 / 0 | 3 / 0 / 0 |
| `RackCad.Tests.DimensionViewPolicyTests` | 297 / 0 / 0 | 297 / 0 / 0 |
| `RackCad.Tests.DimensionViewsCopySitesTests` | 24 / 0 / 0 | 24 / 0 / 0 |
| `RackCad.Tests.DimensionViewsLegacyJsonTests` | 3 / 0 / 0 | 3 / 0 / 0 |
| `RackCad.Tests.DimensionViewsNoneWinsTests` | 36 / 0 / 0 | 36 / 0 / 0 |
| `RackCad.Tests.DimensionViewsPhysicalAndBomInvarianceTests` | 72 / 0 / 0 | 72 / 0 / 0 |
| `RackCad.Tests.DimensionViewsRestampTests` | 11 / 0 / 0 | 11 / 0 / 0 |
| `RackCad.Tests.DrawServicePlanBaselineTests` | 8 / 0 / 0 | 8 / 0 / 0 |
| `RackCad.Tests.DrawingUnitsAdvisoryTests` | 6 / 0 / 0 | 6 / 0 / 0 |
| `RackCad.Tests.DynamicDepthGeometryTests` | 6 / 0 / 0 | 6 / 0 / 0 |
| `RackCad.Tests.DynamicDimensionLegacyCharacterizationTests` | 5 / 0 / 0 | 5 / 0 / 0 |
| `RackCad.Tests.DynamicDimensionViewsDocumentTests` | 9 / 0 / 0 | 9 / 0 / 0 |
| `RackCad.Tests.DynamicDimensionViewsEndToEndTests` | 10 / 0 / 0 | 10 / 0 / 0 |
| `RackCad.Tests.DynamicDimensionViewsTests` | 66 / 0 / 0 | 66 / 0 / 0 |
| `RackCad.Tests.DynamicEditorCellTests` | 12 / 0 / 0 | 12 / 0 / 0 |
| `RackCad.Tests.DynamicEditorDesignAssemblerTests` | 8 / 0 / 0 | 8 / 0 / 0 |
| `RackCad.Tests.DynamicEditorSafetyTests` | 7 / 0 / 0 | 7 / 0 / 0 |
| `RackCad.Tests.DynamicEntranceGuidePlanTests` | 2 / 0 / 0 | 2 / 0 / 0 |
| `RackCad.Tests.DynamicFlowBedLateralBuilderTests` | 4 / 0 / 0 | 4 / 0 / 0 |
| `RackCad.Tests.DynamicForkliftDefensePlanTests` | 2 / 0 / 0 | 2 / 0 / 0 |
| `RackCad.Tests.DynamicFrontGeometryTests` | 4 / 0 / 0 | 4 / 0 / 0 |
| `RackCad.Tests.DynamicFrontMatrixTests` | 20 / 0 / 0 | 20 / 0 / 0 |
| `RackCad.Tests.DynamicHeaderBatchLengthTests` | 6 / 0 / 0 | 6 / 0 / 0 |
| `RackCad.Tests.DynamicHeaderBatchMutateTests` | 11 / 0 / 0 | 11 / 0 / 0 |
| `RackCad.Tests.DynamicHeaderBatchPersistenceTests` | 4 / 0 / 0 | 4 / 0 / 0 |
| `RackCad.Tests.DynamicHeaderBatchPrepareTests` | 18 / 0 / 0 | 18 / 0 / 0 |
| `RackCad.Tests.DynamicHeaderBatchReconciliationTests` | 4 / 0 / 0 | 4 / 0 / 0 |
| `RackCad.Tests.DynamicHeaderHeightCalculatorTests` | 7 / 0 / 0 | 7 / 0 / 0 |
| `RackCad.Tests.DynamicLoadBeamGeometryTests` | 4 / 0 / 0 | 4 / 0 / 0 |
| `RackCad.Tests.DynamicModuleTargetsTests` | 18 / 0 / 0 | 18 / 0 / 0 |
| `RackCad.Tests.DynamicNullOverrideGoldenTests` | 1 / 0 / 0 | 1 / 0 / 0 |
| `RackCad.Tests.DynamicPostLevelsTests` | 3 / 0 / 0 | 3 / 0 / 0 |
| `RackCad.Tests.DynamicRackSystemBuilderTests` | 21 / 0 / 0 | 21 / 0 / 0 |
| `RackCad.Tests.DynamicRackSystemResolverTests` | 11 / 0 / 0 | 11 / 0 / 0 |
| `RackCad.Tests.DynamicSafetyDefaultsTests` | 2 / 0 / 0 | 2 / 0 / 0 |
| `RackCad.Tests.DynamicSystemLateralBuilderTests` | 18 / 0 / 0 | 18 / 0 / 0 |
| `RackCad.Tests.DynamicSystemMultiViewBuilderTests` | 12 / 0 / 0 | 12 / 0 / 0 |
| `RackCad.Tests.ElevationOverrideSourceGuardTests` | 3 / 0 / 0 | 3 / 0 / 0 |
| `RackCad.Tests.ExpressionBinderTests` | 65 / 0 / 0 | 65 / 0 / 0 |
| `RackCad.Tests.ExpressionBoundLimitsTests` | 16 / 0 / 0 | 16 / 0 / 0 |
| `RackCad.Tests.ExpressionCanonicalShapeTests` | 15 / 0 / 0 | 15 / 0 / 0 |
| `RackCad.Tests.ExpressionCoreGuardTests` | 6 / 0 / 0 | 6 / 0 / 0 |
| `RackCad.Tests.ExpressionDiagnosticCatalogTests` | 2 / 0 / 0 | 2 / 0 / 0 |
| `RackCad.Tests.ExpressionEvaluatorTests` | 15 / 0 / 0 | 15 / 0 / 0 |
| `RackCad.Tests.ExpressionFormatterTests` | 54 / 0 / 0 | 54 / 0 / 0 |
| `RackCad.Tests.ExpressionFunctionRegistryTests` | 34 / 0 / 0 | 34 / 0 / 0 |
| `RackCad.Tests.ExpressionParserGuardTests` | 38 / 0 / 0 | 38 / 0 / 0 |
| `RackCad.Tests.ExpressionRoundTripTests` | 12 / 0 / 0 | 12 / 0 / 0 |
| `RackCad.Tests.ExpressionSymbolModelTests` | 44 / 0 / 0 | 44 / 0 / 0 |
| `RackCad.Tests.ExpressionSyntaxDiagnosticsTests` | 223 / 0 / 0 | 223 / 0 / 0 |
| `RackCad.Tests.ExpressionSyntaxGrammarTests` | 122 / 0 / 0 | 122 / 0 / 0 |
| `RackCad.Tests.ExpressionTokenTablesTests` | 3 / 0 / 0 | 3 / 0 / 0 |
| `RackCad.Tests.FirstViewFreedomGuardTests` | 5 / 0 / 0 | 5 / 0 / 0 |
| `RackCad.Tests.FlowBedBomBuilderTests` | 2 / 0 / 0 | 2 / 0 / 0 |
| `RackCad.Tests.FlowBedCatalogTests` | 2 / 0 / 0 | 2 / 0 / 0 |
| `RackCad.Tests.FlowBedConfigurationStoreTests` | 2 / 0 / 0 | 2 / 0 / 0 |
| `RackCad.Tests.FlowBedLateralBuilderTests` | 9 / 0 / 0 | 9 / 0 / 0 |
| `RackCad.Tests.FrameModelValidatorTests` | 10 / 0 / 0 | 10 / 0 / 0 |
| `RackCad.Tests.FullPanelCopyPushBackFullReadGuardRedTests` | 1 / 0 / 0 | 1 / 0 / 0 |
| `RackCad.Tests.FullPanelCopyPushBackFullReadPathRedTests` | 1 / 0 / 0 | 1 / 0 / 0 |
| `RackCad.Tests.FullPanelCopyPushBackWritePathGuardRedTests` | 1 / 0 / 0 | 1 / 0 / 0 |
| `RackCad.Tests.FullPanelCopyWorkspaceAssociativeBookkeepingGreenTests` | 21 / 0 / 0 | 21 / 0 / 0 |
| `RackCad.Tests.FullPanelCopyWorkspaceAssociativeBookkeepingRedTests` | 1 / 0 / 0 | 1 / 0 / 0 |
| `RackCad.Tests.FullPanelCopyWorkspaceCommandAdvisorGreenTests` | 70 / 0 / 0 | 70 / 0 / 0 |
| `RackCad.Tests.FullPanelCopyWorkspaceCommandAdvisorRedTests` | 6 / 0 / 0 | 6 / 0 / 0 |
| `RackCad.Tests.FullPanelCopyWorkspaceEditIntentGuardRedTests` | 13 / 0 / 0 | 13 / 0 / 0 |
| `RackCad.Tests.FullPanelCopyWorkspaceEditIntentHostGreenTests` | 57 / 0 / 0 | 57 / 0 / 0 |
| `RackCad.Tests.FullPanelCopyWorkspaceEditIntentHostRedTests` | 24 / 0 / 0 | 24 / 0 / 0 |
| `RackCad.Tests.FullPanelCopyWorkspaceHostHardeningGuardRedTests` | 1 / 0 / 0 | 1 / 0 / 0 |
| `RackCad.Tests.FullPanelCopyWorkspaceLifecycleGuardRedTests` | 2 / 0 / 0 | 2 / 0 / 0 |
| `RackCad.Tests.FullPanelCopyWorkspaceLifecycleHostGreenTests` | 69 / 0 / 0 | 69 / 0 / 0 |
| `RackCad.Tests.FullPanelCopyWorkspaceLifecycleHostRedTests` | 24 / 0 / 0 | 24 / 0 / 0 |
| `RackCad.Tests.FullPanelCopyWorkspacePilotEditGuardGreenTests` | 17 / 0 / 0 | 17 / 0 / 0 |
| `RackCad.Tests.FullPanelCopyWorkspacePilotEditGuardRedTests` | 6 / 0 / 0 | 6 / 0 / 0 |
| `RackCad.Tests.FullPanelCopyWorkspacePilotEditHostGreenTests` | 17 / 0 / 0 | 17 / 0 / 0 |
| `RackCad.Tests.FullPanelCopyWorkspacePilotEditHostRedTests` | 59 / 0 / 0 | 59 / 0 / 0 |
| `RackCad.Tests.FullPanelCopyWorkspacePilotPrepareGreenTests` | 47 / 0 / 0 | 47 / 0 / 0 |
| `RackCad.Tests.FullPanelCopyWorkspacePilotPrepareRedTests` | 44 / 0 / 0 | 44 / 0 / 0 |
| `RackCad.Tests.FullPanelCopyWorkspacePilotReconciliationGuardGreenTests` | 1 / 0 / 0 | 1 / 0 / 0 |
| `RackCad.Tests.FullPanelCopyWorkspacePilotReconciliationHostGreenTests` | 5 / 0 / 0 | 5 / 0 / 0 |
| `RackCad.Tests.FullPanelCopyWorkspacePilotReconciliationHostRedTests` | 23 / 0 / 0 | 23 / 0 / 0 |
| `RackCad.Tests.FullPanelCopyWorkspacePilotUpdateGuardRedTests` | 1 / 0 / 0 | 1 / 0 / 0 |
| `RackCad.Tests.FullPanelCopyWorkspacePilotUpdateHostGreenTests` | 18 / 0 / 0 | 18 / 0 / 0 |
| `RackCad.Tests.FullPanelCopyWorkspacePilotUpdateHostRedTests` | 29 / 0 / 0 | 29 / 0 / 0 |
| `RackCad.Tests.FullPanelCopyWorkspacePilotUpdateIntegrationGuardRedTests` | 2 / 0 / 0 | 2 / 0 / 0 |
| `RackCad.Tests.FullPanelCopyWorkspacePilotUpdateRequestGreenTests` | 1 / 0 / 0 | 1 / 0 / 0 |
| `RackCad.Tests.FullPanelCopyWorkspacePilotUpdateRequestRedTests` | 53 / 0 / 0 | 53 / 0 / 0 |
| `RackCad.Tests.G10EditorContractTests` | 22 / 0 / 0 | 22 / 0 / 0 |
| `RackCad.Tests.G10LegacyEditorCharacterizationTests` | 8 / 0 / 0 | 8 / 0 / 0 |
| `RackCad.Tests.G10VariablesContractTests` | 9 / 0 / 0 | 9 / 0 / 0 |
| `RackCad.Tests.G11PerformanceMeasurementTests` | 3 / 0 / 0 | 3 / 0 / 0 |
| `RackCad.Tests.G11RealChainCandidateTests` | 2 / 0 / 0 | 2 / 0 / 0 |
| `RackCad.Tests.G12A1CreateExpressionHotfixTests` | 8 / 0 / 0 | 8 / 0 / 0 |
| `RackCad.Tests.G16BlockNameTests` | 14 / 0 / 0 | 14 / 0 / 0 |
| `RackCad.Tests.G16ElevationProjectionTests` | 98 / 0 / 0 | 98 / 0 / 0 |
| `RackCad.Tests.G16FirstViewCorrectiveTests` | 61 / 0 / 0 | 61 / 0 / 0 |
| `RackCad.Tests.G16OrientationAuthorityTests` | 256 / 0 / 0 | 256 / 0 / 0 |
| `RackCad.Tests.G16OrientationGuardTests` | 5 / 0 / 0 | 5 / 0 / 0 |
| `RackCad.Tests.G16ProjectedEnvelopeTests` | 42 / 0 / 0 | 42 / 0 / 0 |
| `RackCad.Tests.G16ProjectedOrientationTests` | 48 / 0 / 0 | 48 / 0 / 0 |
| `RackCad.Tests.G16UnnamedRackTests` | 28 / 0 / 0 | 28 / 0 / 0 |
| `RackCad.Tests.G7DependencyGraphContractTests` | 8 / 0 / 0 | 8 / 0 / 0 |
| `RackCad.Tests.G7RegistryEvaluationContractTests` | 12 / 0 / 0 | 12 / 0 / 0 |
| `RackCad.Tests.G8LegacyPersistenceCharacterizationTests` | 9 / 0 / 0 | 9 / 0 / 0 |
| `RackCad.Tests.G8PersistenceIntegrationContractTests` | 53 / 0 / 0 | 53 / 0 / 0 |
| `RackCad.Tests.G9A3ContractMatrixTests` | 44 / 0 / 0 | 44 / 0 / 0 |
| `RackCad.Tests.G9ConformanceFixTests` | 6 / 0 / 0 | 6 / 0 / 0 |
| `RackCad.Tests.G9LegacyMutationCharacterizationTests` | 10 / 0 / 0 | 10 / 0 / 0 |
| `RackCad.Tests.G9MutationPropagationContractTests` | 23 / 0 / 0 | 23 / 0 / 0 |
| `RackCad.Tests.GeometryMateSolverTests` | 3 / 0 / 0 | 3 / 0 / 0 |
| `RackCad.Tests.GeometryPrimitivesTests` | 34 / 0 / 0 | 34 / 0 / 0 |
| `RackCad.Tests.HardcodedStandardRackFrameServiceTests` | 3 / 0 / 0 | 3 / 0 / 0 |
| `RackCad.Tests.HeaderBatchContractTests` | 31 / 0 / 0 | 31 / 0 / 0 |
| `RackCad.Tests.HeaderConfigurationSnapshotTests` | 19 / 0 / 0 | 19 / 0 / 0 |
| `RackCad.Tests.HeaderInstanceGrouperTests` | 6 / 0 / 0 | 6 / 0 / 0 |
| `RackCad.Tests.I55CantileverPlantaVisibilityPropagationTests` | 1 / 0 / 0 | 1 / 0 / 0 |
| `RackCad.Tests.I55DeferredProductFixCharacterizationTests` | 3 / 0 / 0 | 3 / 0 / 0 |
| `RackCad.Tests.I55G14FoundationConsumerTests` | 5 / 0 / 0 | 5 / 0 / 0 |
| `RackCad.Tests.I55G15Auth15ConsumerTests` | 5 / 0 / 0 | 5 / 0 / 0 |
| `RackCad.Tests.I58Conf58SchemaWhitespaceTests` | 580 / 0 / 0 | 580 / 0 / 0 |
| `RackCad.Tests.I58F1CharacterizationTests` | 1272 / 0 / 0 | 1272 / 0 / 0 |
| `RackCad.Tests.I58F1OracleChecks` | 640 / 0 / 0 | 640 / 0 / 0 |
| `RackCad.Tests.I58F2BoundaryTests` | 26 / 0 / 0 | 26 / 0 / 0 |
| `RackCad.Tests.I58F2MaterializationTests` | 3 / 0 / 0 | 3 / 0 / 0 |
| `RackCad.Tests.I58F2ReaderTests` | 408 / 0 / 0 | 408 / 0 / 0 |
| `RackCad.Tests.I58F3SeamTests` | 56 / 0 / 0 | 56 / 0 / 0 |
| `RackCad.Tests.I59F1CharacterizationTests` | 14 / 0 / 0 | 14 / 0 / 0 |
| `RackCad.Tests.I59F2BoundaryGuardTests` | 2 / 0 / 0 | 2 / 0 / 0 |
| `RackCad.Tests.I59F2ContractTests` | 59 / 0 / 0 | 59 / 0 / 0 |
| `RackCad.Tests.I59F3ConsumerIntegrationTests` | 8 / 0 / 0 | 8 / 0 / 0 |
| `RackCad.Tests.I59F3CorrectionTests` | 13 / 0 / 0 | 13 / 0 / 0 |
| `RackCad.Tests.I59F4ConformanceTests` | 4 / 0 / 0 | 4 / 0 / 0 |
| `RackCad.Tests.I60BatchCreationNamingTests` | 25 / 0 / 0 | 25 / 0 / 0 |
| `RackCad.Tests.I60RackLogicalNameTests` | 35 / 0 / 0 | 35 / 0 / 0 |
| `RackCad.Tests.I60RackNameWiringGuardTests` | 22 / 0 / 0 | 22 / 0 / 0 |
| `RackCad.Tests.I61CamaEditWiringGuardTests` | 10 / 0 / 0 | 10 / 0 / 0 |
| `RackCad.Tests.I61EditedRackNameTests` | 6 / 0 / 0 | 6 / 0 / 0 |
| `RackCad.Tests.InsertBlankIdFlowCharacterizationTests` | 7 / 0 / 0 | 7 / 0 / 0 |
| `RackCad.Tests.InsertRedrawLayerAccessCharacterizationTests` | 5 / 0 / 0 | 5 / 0 / 0 |
| `RackCad.Tests.JsonRackCatalogProviderTests` | 6 / 0 / 0 | 6 / 0 / 0 |
| `RackCad.Tests.KindDispatchTests` | 21 / 0 / 0 | 21 / 0 / 0 |
| `RackCad.Tests.KindHandlerGuardSourceTests` | 2 / 0 / 0 | 2 / 0 / 0 |
| `RackCad.Tests.LargueroBomBuilderTests` | 3 / 0 / 0 | 3 / 0 / 0 |
| `RackCad.Tests.LateralHeaderLayoutBuilderTests` | 18 / 0 / 0 | 18 / 0 / 0 |
| `RackCad.Tests.LateralHeaderParametersFactoryTests` | 4 / 0 / 0 | 4 / 0 / 0 |
| `RackCad.Tests.LegacyViewPayloadCompositionCharacterizationTests` | 8 / 0 / 0 | 8 / 0 / 0 |
| `RackCad.Tests.LengthUnitsAuthorityGuardTests` | 25 / 0 / 0 | 25 / 0 / 0 |
| `RackCad.Tests.LengthUnitsTests` | 5 / 0 / 0 | 5 / 0 / 0 |
| `RackCad.Tests.LevelElevationResolutionTests` | 1095 / 0 / 0 | 1095 / 0 / 0 |
| `RackCad.Tests.LevelSpacingH58ReproductionTests` | 3 / 0 / 0 | 3 / 0 / 0 |
| `RackCad.Tests.LinkedPropertyEditSessionTests` | 31 / 0 / 0 | 31 / 0 / 0 |
| `RackCad.Tests.LinkedPropertyFoundationTests` | 21 / 0 / 0 | 21 / 0 / 0 |
| `RackCad.Tests.LinkedPropertyKernelTests` | 31 / 0 / 0 | 31 / 0 / 0 |
| `RackCad.Tests.LinkedPropertyReconcilerTests` | 16 / 0 / 0 | 16 / 0 / 0 |
| `RackCad.Tests.LinkedPropertyViewPayloadTests` | 6 / 0 / 0 | 6 / 0 / 0 |
| `RackCad.Tests.LocalizedNumberParserTests` | 9 / 0 / 0 | 9 / 0 / 0 |
| `RackCad.Tests.MainMenuStructuralSectionAccessGuardTests` | 28 / 0 / 0 | 28 / 0 / 0 |
| `RackCad.Tests.MultiPropertyRealProofTests` | 30 / 0 / 0 | 30 / 0 / 0 |
| `RackCad.Tests.NamespaceFolderGuardTests` | 7 / 0 / 0 | 7 / 0 / 0 |
| `RackCad.Tests.NoProductPlannerGuardTests` | 3 / 0 / 0 | 3 / 0 / 0 |
| `RackCad.Tests.OrphanFirstJigTransactionGuardTests` | 2 / 0 / 0 | 2 / 0 / 0 |
| `RackCad.Tests.PalletToleranceRealProofTests` | 17 / 0 / 0 | 17 / 0 / 0 |
| `RackCad.Tests.PeralteListTests` | 4 / 0 / 0 | 4 / 0 / 0 |
| `RackCad.Tests.PersistedVariableTypeGrammarTests` | 36 / 0 / 0 | 36 / 0 / 0 |
| `RackCad.Tests.PersistenceEmbedInnerDesignTests` | 4 / 0 / 0 | 4 / 0 / 0 |
| `RackCad.Tests.PersistenceInnerPreflightTests` | 9 / 0 / 0 | 9 / 0 / 0 |
| `RackCad.Tests.PersistenceInnerSourceTests` | 13 / 0 / 0 | 13 / 0 / 0 |
| `RackCad.Tests.PersistenceLibraryTransportTests` | 4 / 0 / 0 | 4 / 0 / 0 |
| `RackCad.Tests.PersistenceReopenPreservationTests` | 22 / 0 / 0 | 22 / 0 / 0 |
| `RackCad.Tests.PersistenceUniformityTests` | 15 / 0 / 0 | 15 / 0 / 0 |
| `RackCad.Tests.PersistenceValidationTests` | 5 / 0 / 0 | 5 / 0 / 0 |
| `RackCad.Tests.PersistenceVersioningTests` | 7 / 0 / 0 | 7 / 0 / 0 |
| `RackCad.Tests.PlantaHeaderLayoutBuilderTests` | 6 / 0 / 0 | 6 / 0 / 0 |
| `RackCad.Tests.ProductPolicyIndependenceGuardTests` | 2 / 0 / 0 | 2 / 0 / 0 |
| `RackCad.Tests.ProjectVariableDiscoveryTests` | 39 / 0 / 0 | 39 / 0 / 0 |
| `RackCad.Tests.ProjectVariableMutationExecutorTests` | 28 / 0 / 0 | 28 / 0 / 0 |
| `RackCad.Tests.ProjectVariableMutationPreflightTests` | 46 / 0 / 0 | 46 / 0 / 0 |
| `RackCad.Tests.ProjectVariableScanProjectionTests` | 17 / 0 / 0 | 17 / 0 / 0 |
| `RackCad.Tests.ProjectVariablesConformanceTests` | 10 / 0 / 0 | 10 / 0 / 0 |
| `RackCad.Tests.ProjectVariablesDocumentTests` | 27 / 0 / 0 | 27 / 0 / 0 |
| `RackCad.Tests.ProjectVariablesModelTests` | 34 / 0 / 0 | 34 / 0 / 0 |
| `RackCad.Tests.ProjectVariablesRegistryAccessTests` | 14 / 0 / 0 | 14 / 0 / 0 |
| `RackCad.Tests.ProjectVariablesWorkspaceTests` | 46 / 0 / 0 | 46 / 0 / 0 |
| `RackCad.Tests.ProjectionAvailabilityPolicyTests` | 9 / 0 / 0 | 9 / 0 / 0 |
| `RackCad.Tests.ProjectionCodecPolicyTests` | 6 / 0 / 0 | 6 / 0 / 0 |
| `RackCad.Tests.PushBackA1BStorageTests` | 6 / 0 / 0 | 6 / 0 / 0 |
| `RackCad.Tests.PushBackA1PersistenceTests` | 5 / 0 / 0 | 5 / 0 / 0 |
| `RackCad.Tests.PushBackA1StructureTests` | 4 / 0 / 0 | 4 / 0 / 0 |
| `RackCad.Tests.PushBackAdvancedRackParametersTests` | 30 / 0 / 0 | 30 / 0 / 0 |
| `RackCad.Tests.PushBackBedAnchorTests` | 43 / 0 / 0 | 43 / 0 / 0 |
| `RackCad.Tests.PushBackBedAsymmetryTests` | 38 / 0 / 0 | 38 / 0 / 0 |
| `RackCad.Tests.PushBackBedInOutMateTests` | 41 / 0 / 0 | 41 / 0 / 0 |
| `RackCad.Tests.PushBackBedSlopeTests` | 3 / 0 / 0 | 3 / 0 / 0 |
| `RackCad.Tests.PushBackBedTangencyTests` | 4 / 0 / 0 | 4 / 0 / 0 |
| `RackCad.Tests.PushBackBlankFrontAuthorityTests` | 9 / 0 / 0 | 9 / 0 / 0 |
| `RackCad.Tests.PushBackBlankSideBDeclarationTests` | 9 / 0 / 0 | 9 / 0 / 0 |
| `RackCad.Tests.PushBackBlankSlotIdentityTests` | 11 / 0 / 0 | 11 / 0 / 0 |
| `RackCad.Tests.PushBackBlankStorageCoverageTests` | 12 / 0 / 0 | 12 / 0 / 0 |
| `RackCad.Tests.PushBackBomCommandGuardTests` | 4 / 0 / 0 | 4 / 0 / 0 |
| `RackCad.Tests.PushBackBomTests` | 5 / 0 / 0 | 5 / 0 / 0 |
| `RackCad.Tests.PushBackBootIdentityTests` | 32 / 0 / 0 | 32 / 0 / 0 |
| `RackCad.Tests.PushBackBootLowFaceTests` | 40 / 0 / 0 | 40 / 0 / 0 |
| `RackCad.Tests.PushBackBootSemanticsTests` | 84 / 0 / 0 | 84 / 0 / 0 |
| `RackCad.Tests.PushBackBootSidesTests` | 45 / 0 / 0 | 45 / 0 / 0 |
| `RackCad.Tests.PushBackBootTypeTests` | 28 / 0 / 0 | 28 / 0 / 0 |
| `RackCad.Tests.PushBackCellConfigurationCharacterizationTests` | 7 / 0 / 0 | 7 / 0 / 0 |
| `RackCad.Tests.PushBackCellConfigurationDeliveryTests` | 17 / 0 / 0 | 17 / 0 / 0 |
| `RackCad.Tests.PushBackCellDepthTests` | 37 / 0 / 0 | 37 / 0 / 0 |
| `RackCad.Tests.PushBackCellPalletTests` | 25 / 0 / 0 | 25 / 0 / 0 |
| `RackCad.Tests.PushBackClassicCharacterizationPluginGuardTests` | 10 / 0 / 0 | 10 / 0 / 0 |
| `RackCad.Tests.PushBackCompositeAuditTests` | 19 / 0 / 0 | 19 / 0 / 0 |
| `RackCad.Tests.PushBackCompositeBedTests` | 14 / 0 / 0 | 14 / 0 / 0 |
| `RackCad.Tests.PushBackCompositeBomTests` | 10 / 0 / 0 | 10 / 0 / 0 |
| `RackCad.Tests.PushBackCompositeCharacterizationTests` | 6 / 0 / 0 | 6 / 0 / 0 |
| `RackCad.Tests.PushBackCompositeCorrectionTests` | 37 / 0 / 0 | 37 / 0 / 0 |
| `RackCad.Tests.PushBackCompositeCorridaAuthorityTests` | 19 / 0 / 0 | 19 / 0 / 0 |
| `RackCad.Tests.PushBackCompositeEditingContractTests` | 18 / 0 / 0 | 18 / 0 / 0 |
| `RackCad.Tests.PushBackCompositeEditorTests` | 15 / 0 / 0 | 15 / 0 / 0 |
| `RackCad.Tests.PushBackCompositeFrontalTests` | 8 / 0 / 0 | 8 / 0 / 0 |
| `RackCad.Tests.PushBackCompositeHeightTests` | 7 / 0 / 0 | 7 / 0 / 0 |
| `RackCad.Tests.PushBackCompositeLifecycleTests` | 8 / 0 / 0 | 8 / 0 / 0 |
| `RackCad.Tests.PushBackCompositeModelTests` | 13 / 0 / 0 | 13 / 0 / 0 |
| `RackCad.Tests.PushBackCompositeModuleSequenceTests` | 9 / 0 / 0 | 9 / 0 / 0 |
| `RackCad.Tests.PushBackCompositeMultiFrontTests` | 28 / 0 / 0 | 28 / 0 / 0 |
| `RackCad.Tests.PushBackCompositeRecomputeContainmentTests` | 2 / 0 / 0 | 2 / 0 / 0 |
| `RackCad.Tests.PushBackCompositeRoundTripTests` | 13 / 0 / 0 | 13 / 0 / 0 |
| `RackCad.Tests.PushBackCompositeSafetyOwnershipTests` | 9 / 0 / 0 | 9 / 0 / 0 |
| `RackCad.Tests.PushBackCompositeSafetyTests` | 10 / 0 / 0 | 10 / 0 / 0 |
| `RackCad.Tests.PushBackCompositeStructureTests` | 15 / 0 / 0 | 15 / 0 / 0 |
| `RackCad.Tests.PushBackCompositeSymmetryTests` | 10 / 0 / 0 | 10 / 0 / 0 |
| `RackCad.Tests.PushBackCompositeTopeTests` | 14 / 0 / 0 | 14 / 0 / 0 |
| `RackCad.Tests.PushBackCompositeViewTests` | 20 / 0 / 0 | 20 / 0 / 0 |
| `RackCad.Tests.PushBackCoreTests` | 21 / 0 / 0 | 21 / 0 / 0 |
| `RackCad.Tests.PushBackCorrectionTests` | 4 / 0 / 0 | 4 / 0 / 0 |
| `RackCad.Tests.PushBackCorridaAnchorTests` | 15 / 0 / 0 | 15 / 0 / 0 |
| `RackCad.Tests.PushBackCorridaDualValidityTests` | 11 / 0 / 0 | 11 / 0 / 0 |
| `RackCad.Tests.PushBackCorridaSpanStorageTests` | 14 / 0 / 0 | 14 / 0 / 0 |
| `RackCad.Tests.PushBackCutSupportTests` | 23 / 0 / 0 | 23 / 0 / 0 |
| `RackCad.Tests.PushBackDefensePerPostTests` | 15 / 0 / 0 | 15 / 0 / 0 |
| `RackCad.Tests.PushBackDefensePlanTests` | 14 / 0 / 0 | 14 / 0 / 0 |
| `RackCad.Tests.PushBackDefenseResizeTests` | 5 / 0 / 0 | 5 / 0 / 0 |
| `RackCad.Tests.PushBackDefenseSideIdentityTests` | 15 / 0 / 0 | 15 / 0 / 0 |
| `RackCad.Tests.PushBackDerivedPostAndLineTests` | 21 / 0 / 0 | 21 / 0 / 0 |
| `RackCad.Tests.PushBackDesviadorCellKeyTests` | 7 / 0 / 0 | 7 / 0 / 0 |
| `RackCad.Tests.PushBackDimensionLegacyCharacterizationTests` | 8 / 0 / 0 | 8 / 0 / 0 |
| `RackCad.Tests.PushBackDimensionViewsTests` | 47 / 0 / 0 | 47 / 0 / 0 |
| `RackCad.Tests.PushBackDiverterLowAuthorityTests` | 13 / 0 / 0 | 13 / 0 / 0 |
| `RackCad.Tests.PushBackDiverterPlanTests` | 28 / 0 / 0 | 28 / 0 / 0 |
| `RackCad.Tests.PushBackDormantSideBFirstLevelTests` | 7 / 0 / 0 | 7 / 0 / 0 |
| `RackCad.Tests.PushBackDraftPortSharedReferenceGuardTests` | 1 / 0 / 0 | 1 / 0 / 0 |
| `RackCad.Tests.PushBackDraftStatePortTests` | 3 / 0 / 0 | 3 / 0 / 0 |
| `RackCad.Tests.PushBackDraftWriteGuardTests` | 2 / 0 / 1 | 2 / 0 / 1 |
| `RackCad.Tests.PushBackEditAllFullCopyGuardTests` | 1 / 0 / 0 | 1 / 0 / 0 |
| `RackCad.Tests.PushBackEditAllNameWritersGuardTests` | 1 / 0 / 0 | 1 / 0 / 0 |
| `RackCad.Tests.PushBackEditAllPortEquivalenceGuardTests` | 1 / 0 / 0 | 1 / 0 / 0 |
| `RackCad.Tests.PushBackEditableStateCopyTests` | 7 / 0 / 0 | 7 / 0 / 0 |
| `RackCad.Tests.PushBackEditableStateLoadTests` | 5 / 0 / 0 | 5 / 0 / 0 |
| `RackCad.Tests.PushBackEditableStateOwnershipTests` | 1 / 0 / 0 | 1 / 0 / 0 |
| `RackCad.Tests.PushBackEditorCorrectionTests` | 17 / 0 / 0 | 17 / 0 / 0 |
| `RackCad.Tests.PushBackEditorDefaultsTests` | 5 / 0 / 0 | 5 / 0 / 0 |
| `RackCad.Tests.PushBackEditorOutputGateTests` | 1 / 0 / 0 | 1 / 0 / 0 |
| `RackCad.Tests.PushBackEditorStateTests` | 27 / 0 / 0 | 27 / 0 / 0 |
| `RackCad.Tests.PushBackElevationAuthorityTests` | 19 / 0 / 0 | 19 / 0 / 0 |
| `RackCad.Tests.PushBackElevationConsumerTests` | 14 / 0 / 0 | 14 / 0 / 0 |
| `RackCad.Tests.PushBackEndBeamIdentityTests` | 9 / 0 / 0 | 9 / 0 / 0 |
| `RackCad.Tests.PushBackEndToEndChainTests` | 2 / 0 / 0 | 2 / 0 / 0 |
| `RackCad.Tests.PushBackFirstLevelDatumTransportTests` | 29 / 0 / 0 | 29 / 0 / 0 |
| `RackCad.Tests.PushBackForkliftDefenseTests` | 12 / 0 / 0 | 12 / 0 / 0 |
| `RackCad.Tests.PushBackFrontalHeaderOwnershipTests` | 9 / 0 / 0 | 9 / 0 / 0 |
| `RackCad.Tests.PushBackFrontalMatchesLateralTests` | 16 / 0 / 0 | 16 / 0 / 0 |
| `RackCad.Tests.PushBackFrontalRearCutTests` | 7 / 0 / 0 | 7 / 0 / 0 |
| `RackCad.Tests.PushBackFullDraftClassifierTests` | 2 / 0 / 0 | 2 / 0 / 0 |
| `RackCad.Tests.PushBackFullDraftCompositionTests` | 3 / 0 / 0 | 3 / 0 / 0 |
| `RackCad.Tests.PushBackFullDraftLoadTests` | 4 / 0 / 0 | 4 / 0 / 0 |
| `RackCad.Tests.PushBackFullDraftViewTests` | 1 / 0 / 0 | 1 / 0 / 0 |
| `RackCad.Tests.PushBackFullEditorCharacterizationDimensionStyleLossTests` | 1 / 0 / 0 | 1 / 0 / 0 |
| `RackCad.Tests.PushBackFullEditorCharacterizationLateralCutTests` | 1 / 0 / 0 | 1 / 0 / 0 |
| `RackCad.Tests.PushBackFullEditorCharacterizationRoundingEffectTests` | 1 / 0 / 0 | 1 / 0 / 0 |
| `RackCad.Tests.PushBackFullPanelCensusGuardTests` | 1 / 0 / 0 | 1 / 0 / 0 |
| `RackCad.Tests.PushBackFullPanelGuardTests` | 4 / 0 / 0 | 4 / 0 / 0 |
| `RackCad.Tests.PushBackFullPanelTests` | 11 / 0 / 0 | 11 / 0 / 0 |
| `RackCad.Tests.PushBackFullReadGuardTests` | 1 / 0 / 2 | 1 / 0 / 2 |
| `RackCad.Tests.PushBackFullReadPathTests` | 4 / 0 / 1 | 4 / 0 / 1 |
| `RackCad.Tests.PushBackFullWritePrepareTests` | 9 / 0 / 0 | 9 / 0 / 0 |
| `RackCad.Tests.PushBackFullWriteRunTests` | 4 / 0 / 0 | 4 / 0 / 0 |
| `RackCad.Tests.PushBackGeneralLateralActiveEnvelopeTests` | 4 / 0 / 0 | 4 / 0 / 0 |
| `RackCad.Tests.PushBackGeneralLateralHeaderZoneTests` | 9 / 0 / 0 | 9 / 0 / 0 |
| `RackCad.Tests.PushBackGoldenTests` | 1 / 0 / 0 | 1 / 0 / 0 |
| `RackCad.Tests.PushBackHeaderApplyTests` | 30 / 0 / 0 | 30 / 0 / 0 |
| `RackCad.Tests.PushBackHeaderDepthHeightTests` | 9 / 0 / 0 | 9 / 0 / 0 |
| `RackCad.Tests.PushBackHeaderEnvelopeTests` | 10 / 0 / 0 | 10 / 0 / 0 |
| `RackCad.Tests.PushBackHeaderHeightAuthorityTests` | 32 / 0 / 0 | 32 / 0 / 0 |
| `RackCad.Tests.PushBackHeaderHeightTests` | 3 / 0 / 0 | 3 / 0 / 0 |
| `RackCad.Tests.PushBackHighEndHandTests` | 15 / 0 / 0 | 15 / 0 / 0 |
| `RackCad.Tests.PushBackHighTieBreakTests` | 16 / 0 / 0 | 16 / 0 / 0 |
| `RackCad.Tests.PushBackIntermediateBeamPeralteAuthorityTests` | 6 / 0 / 0 | 6 / 0 / 0 |
| `RackCad.Tests.PushBackIntermediateBeamPeralteFixTests` | 4 / 0 / 0 | 4 / 0 / 0 |
| `RackCad.Tests.PushBackIntermediateBeamPeralteTests` | 12 / 0 / 0 | 12 / 0 / 0 |
| `RackCad.Tests.PushBackLateralGuardDefaultTests` | 11 / 0 / 0 | 11 / 0 / 0 |
| `RackCad.Tests.PushBackLateralProtectorEndsTests` | 7 / 0 / 0 | 7 / 0 / 0 |
| `RackCad.Tests.PushBackLengthCoherenceTests` | 2 / 0 / 0 | 2 / 0 / 0 |
| `RackCad.Tests.PushBackModuleAdoptionTests` | 20 / 0 / 0 | 20 / 0 / 0 |
| `RackCad.Tests.PushBackModuleEditorCharacterizationTests` | 15 / 0 / 0 | 15 / 0 / 0 |
| `RackCad.Tests.PushBackNoParrillaPersistenceTests` | 1 / 0 / 0 | 1 / 0 / 0 |
| `RackCad.Tests.PushBackOutputGateTests` | 8 / 0 / 0 | 8 / 0 / 0 |
| `RackCad.Tests.PushBackOwnerRejectionRound1Tests` | 10 / 0 / 0 | 10 / 0 / 0 |
| `RackCad.Tests.PushBackPalletProjectionTests` | 17 / 0 / 0 | 17 / 0 / 0 |
| `RackCad.Tests.PushBackPanelDraftGuardTests` | 1 / 0 / 0 | 1 / 0 / 0 |
| `RackCad.Tests.PushBackPartialCompositeTests` | 14 / 0 / 0 | 14 / 0 / 0 |
| `RackCad.Tests.PushBackPhysicalBomAndSafetyTests` | 26 / 0 / 0 | 26 / 0 / 0 |
| `RackCad.Tests.PushBackPhysicalHeaderHeightTests` | 19 / 0 / 0 | 19 / 0 / 0 |
| `RackCad.Tests.PushBackPilotUpdateEquivalenceGuardTests` | 6 / 0 / 0 | 6 / 0 / 0 |
| `RackCad.Tests.PushBackPlanTests` | 9 / 0 / 0 | 9 / 0 / 0 |
| `RackCad.Tests.PushBackPlantBedBeamIdentityTests` | 8 / 0 / 0 | 8 / 0 / 0 |
| `RackCad.Tests.PushBackPluginSourceGuardTests` | 11 / 0 / 0 | 11 / 0 / 0 |
| `RackCad.Tests.PushBackPreviewSafetyParityTests` | 9 / 0 / 0 | 9 / 0 / 0 |
| `RackCad.Tests.PushBackRearTopeAnchorTests` | 7 / 0 / 0 | 7 / 0 / 0 |
| `RackCad.Tests.PushBackRearTopeContactPointTests` | 9 / 0 / 0 | 9 / 0 / 0 |
| `RackCad.Tests.PushBackRearTopeNoneAndFarEndAutoTests` | 9 / 0 / 0 | 9 / 0 / 0 |
| `RackCad.Tests.PushBackRearTopePieceTests` | 9 / 0 / 0 | 9 / 0 / 0 |
| `RackCad.Tests.PushBackRearTopeRunAuthorityTests` | 27 / 0 / 0 | 27 / 0 / 0 |
| `RackCad.Tests.PushBackRegistrationTests` | 7 / 0 / 0 | 7 / 0 / 0 |
| `RackCad.Tests.PushBackReinforcedPostBeamTests` | 7 / 0 / 0 | 7 / 0 / 0 |
| `RackCad.Tests.PushBackRoundTripSourceGuardTests` | 24 / 0 / 0 | 24 / 0 / 0 |
| `RackCad.Tests.PushBackRunFrameTests` | 51 / 0 / 0 | 51 / 0 / 0 |
| `RackCad.Tests.PushBackSafetyBomTests` | 5 / 0 / 0 | 5 / 0 / 0 |
| `RackCad.Tests.PushBackSafetyPerPostTests` | 10 / 0 / 0 | 10 / 0 / 0 |
| `RackCad.Tests.PushBackSharedCellGridTests` | 12 / 0 / 0 | 12 / 0 / 0 |
| `RackCad.Tests.PushBackSharedGridCellDemandTests` | 10 / 0 / 0 | 10 / 0 / 0 |
| `RackCad.Tests.PushBackSharedReferenceGuardTests` | 1 / 0 / 1 | 1 / 0 / 1 |
| `RackCad.Tests.PushBackSharedTransverseGridTests` | 10 / 0 / 0 | 10 / 0 / 0 |
| `RackCad.Tests.PushBackSideIndependentHeightTests` | 35 / 0 / 0 | 35 / 0 / 0 |
| `RackCad.Tests.PushBackTarimaPlacementTests` | 10 / 0 / 0 | 10 / 0 / 0 |
| `RackCad.Tests.PushBackTopeAnchorPerViewTests` | 7 / 0 / 0 | 7 / 0 / 0 |
| `RackCad.Tests.PushBackTopeOriginMateTests` | 7 / 0 / 0 | 7 / 0 / 0 |
| `RackCad.Tests.PushBackTopePlantaAndDiverterTests` | 18 / 0 / 0 | 18 / 0 / 0 |
| `RackCad.Tests.PushBackTopeSurfaceTests` | 14 / 0 / 0 | 14 / 0 / 0 |
| `RackCad.Tests.PushBackVerticalAuthorityTests` | 19 / 0 / 0 | 19 / 0 / 0 |
| `RackCad.Tests.PushBackViewEraseGuardTests` | 4 / 0 / 0 | 4 / 0 / 0 |
| `RackCad.Tests.PushBackViewHygieneTests` | 30 / 0 / 0 | 30 / 0 / 0 |
| `RackCad.Tests.PushBackViewTests` | 6 / 0 / 0 | 6 / 0 / 0 |
| `RackCad.Tests.PushBackWholeLateralEnvelopeTests` | 9 / 0 / 0 | 9 / 0 / 0 |
| `RackCad.Tests.PushBackWindowOnStateSourceGuardTests` | 1 / 0 / 0 | 1 / 0 / 0 |
| `RackCad.Tests.PushBackWindowSharedReferenceGuardTests` | 1 / 0 / 0 | 1 / 0 / 0 |
| `RackCad.Tests.PushBackWorkspaceModeSourceGuardTests` | 1 / 0 / 0 | 1 / 0 / 0 |
| `RackCad.Tests.PushBackWorkspaceSharedReferenceGuardTests` | 1 / 0 / 0 | 1 / 0 / 0 |
| `RackCad.Tests.PushBackWritePathGuardTests` | 1 / 0 / 1 | 1 / 0 / 1 |
| `RackCad.Tests.PushBackWritePathPluginGuardTests` | 1 / 0 / 0 | 1 / 0 / 0 |
| `RackCad.Tests.RackCountInvariantCharacterizationTests` | 3 / 0 / 0 | 3 / 0 / 0 |
| `RackCad.Tests.RackDefinitionCreatorGuardTests` | 26 / 0 / 0 | 26 / 0 / 0 |
| `RackCad.Tests.RackDesignLibraryTests` | 9 / 0 / 0 | 9 / 0 / 0 |
| `RackCad.Tests.RackDuplicationPlanTests` | 31 / 0 / 0 | 31 / 0 / 0 |
| `RackCad.Tests.RackEditPreflightCharacterizationTests` | 5 / 0 / 0 | 5 / 0 / 0 |
| `RackCad.Tests.RackEmbedDocumentTests` | 7 / 0 / 0 | 7 / 0 / 0 |
| `RackCad.Tests.RackEnvelopeIdProbeTests` | 5 / 0 / 0 | 5 / 0 / 0 |
| `RackCad.Tests.RackFirstLevelDatumTests` | 26 / 0 / 0 | 26 / 0 / 0 |
| `RackCad.Tests.RackFrameConfigurationDeepCopyTests` | 11 / 0 / 0 | 11 / 0 / 0 |
| `RackCad.Tests.RackFrameConfigurationFactoryTests` | 11 / 0 / 0 | 11 / 0 / 0 |
| `RackCad.Tests.RackFrameProjectStoreTests` | 11 / 0 / 0 | 11 / 0 / 0 |
| `RackCad.Tests.RackFrameTemplateProviderTests` | 5 / 0 / 0 | 5 / 0 / 0 |
| `RackCad.Tests.RackGroupFrameTests` | 7 / 0 / 0 | 7 / 0 / 0 |
| `RackCad.Tests.RackGroupPlacementPlanTests` | 24 / 0 / 0 | 24 / 0 / 0 |
| `RackCad.Tests.RackIdLifecycleTests` | 4 / 0 / 0 | 4 / 0 / 0 |
| `RackCad.Tests.RackLevelElevationsTests` | 15 / 0 / 0 | 15 / 0 / 0 |
| `RackCad.Tests.RackListBuilderTests` | 10 / 0 / 0 | 10 / 0 / 0 |
| `RackCad.Tests.RackLogTests` | 8 / 0 / 0 | 8 / 0 / 0 |
| `RackCad.Tests.RackModuleEditSessionTests` | 25 / 0 / 0 | 25 / 0 / 0 |
| `RackCad.Tests.RackOrthographicPlacementPolicyTests` | 15 / 0 / 0 | 15 / 0 / 0 |
| `RackCad.Tests.RackPlacedGeometryInvariantTests` | 6 / 0 / 0 | 6 / 0 / 0 |
| `RackCad.Tests.RackProductAuthoredGateTests` | 5 / 0 / 0 | 5 / 0 / 0 |
| `RackCad.Tests.RackProductPrepareTests` | 5 / 0 / 0 | 5 / 0 / 0 |
| `RackCad.Tests.RackProjectStorePerKindTests` | 3 / 0 / 0 | 3 / 0 / 0 |
| `RackCad.Tests.RackProjectStoreTests` | 18 / 0 / 0 | 18 / 0 / 0 |
| `RackCad.Tests.RackProjectionCommandGuardTests` | 34 / 0 / 0 | 34 / 0 / 0 |
| `RackCad.Tests.RackProjectionCommandRunTests` | 49 / 0 / 0 | 49 / 0 / 0 |
| `RackCad.Tests.RackProjectionMaterializationRunTests` | 9 / 0 / 0 | 9 / 0 / 0 |
| `RackCad.Tests.RackProjectionRemedyTests` | 10 / 0 / 0 | 10 / 0 / 0 |
| `RackCad.Tests.RackRigidPlacementPolicyTests` | 11 / 0 / 0 | 11 / 0 / 0 |
| `RackCad.Tests.RackSiblingAuthoredGateTests` | 4 / 0 / 0 | 4 / 0 / 0 |
| `RackCad.Tests.RackSiblingCustomPropertiesGateTests` | 2 / 0 / 0 | 2 / 0 / 0 |
| `RackCad.Tests.RackSiblingGateIndependenceGuardTests` | 3 / 0 / 0 | 3 / 0 / 0 |
| `RackCad.Tests.RackSiblingInsertRunTests` | 11 / 0 / 0 | 11 / 0 / 0 |
| `RackCad.Tests.RackSiblingMembershipTests` | 12 / 0 / 0 | 12 / 0 / 0 |
| `RackCad.Tests.RackSiblingRedrawPlanTests` | 2 / 0 / 0 | 2 / 0 / 0 |
| `RackCad.Tests.RackSiblingRedrawRunTests` | 6 / 0 / 0 | 6 / 0 / 0 |
| `RackCad.Tests.RackSiblingRedrawUnitsTests` | 2 / 0 / 0 | 2 / 0 / 0 |
| `RackCad.Tests.RackSingleViewPlacementTests` | 18 / 0 / 0 | 18 / 0 / 0 |
| `RackCad.Tests.RackTransformFactsTests` | 9 / 0 / 0 | 9 / 0 / 0 |
| `RackCad.Tests.RackUnitsGuardSourceTests` | 18 / 0 / 0 | 18 / 0 / 0 |
| `RackCad.Tests.RackViewAuthoredEquivalenceTests` | 2 / 0 / 0 | 2 / 0 / 0 |
| `RackCad.Tests.RackViewBatchDriverGuardTests` | 4 / 0 / 0 | 4 / 0 / 0 |
| `RackCad.Tests.RackViewBatchPlanTests` | 16 / 0 / 0 | 16 / 0 / 0 |
| `RackCad.Tests.RackViewCountInvariantTests` | 3 / 0 / 0 | 3 / 0 / 0 |
| `RackCad.Tests.RackViewExposureTests` | 7 / 0 / 0 | 7 / 0 / 0 |
| `RackCad.Tests.RackViewFrameTests` | 10 / 0 / 0 | 10 / 0 / 0 |
| `RackCad.Tests.RackViewPartialRackContractTests` | 2 / 0 / 0 | 2 / 0 / 0 |
| `RackCad.Tests.RackViewPlacementGuardTests` | 4 / 0 / 0 | 4 / 0 / 0 |
| `RackCad.Tests.RackViewPreparationSystemTests` | 6 / 0 / 0 | 6 / 0 / 0 |
| `RackCad.Tests.RedefinedViewReferenceUndoTouchGuardTests` | 4 / 0 / 0 | 4 / 0 / 0 |
| `RackCad.Tests.RegistryCommitAccreditationTests` | 12 / 0 / 0 | 12 / 0 / 0 |
| `RackCad.Tests.SafetySelectionDocumentsTests` | 10 / 0 / 0 | 10 / 0 / 0 |
| `RackCad.Tests.SeccionesCatalogTests` | 3 / 0 / 0 | 3 / 0 / 0 |
| `RackCad.Tests.SelectiveAuthoredBindingPresenceTests` | 15 / 0 / 0 | 15 / 0 / 0 |
| `RackCad.Tests.SelectiveAuthoredBindingTests` | 21 / 0 / 0 | 21 / 0 / 0 |
| `RackCad.Tests.SelectiveAuthoredCarrierAdoptionTests` | 12 / 0 / 0 | 12 / 0 / 0 |
| `RackCad.Tests.SelectiveBindingIntentTests` | 22 / 0 / 0 | 22 / 0 / 0 |
| `RackCad.Tests.SelectiveBomAuthorityTests` | 31 / 0 / 0 | 31 / 0 / 0 |
| `RackCad.Tests.SelectiveBomBuilderTests` | 7 / 0 / 0 | 7 / 0 / 0 |
| `RackCad.Tests.SelectiveCabeceraHeightReviewTests` | 9 / 0 / 0 | 9 / 0 / 0 |
| `RackCad.Tests.SelectiveCabeceraViewPurityTests` | 10 / 0 / 0 | 10 / 0 / 0 |
| `RackCad.Tests.SelectiveDepthTests` | 11 / 0 / 0 | 11 / 0 / 0 |
| `RackCad.Tests.SelectiveDesviadorTests` | 30 / 0 / 0 | 30 / 0 / 0 |
| `RackCad.Tests.SelectiveDimensionLegacyCharacterizationTests` | 6 / 0 / 0 | 6 / 0 / 0 |
| `RackCad.Tests.SelectiveDimensionViewsAuthorityTests` | 12 / 0 / 0 | 12 / 0 / 0 |
| `RackCad.Tests.SelectiveDimensionViewsDocumentTests` | 10 / 0 / 0 | 10 / 0 / 0 |
| `RackCad.Tests.SelectiveDimensionViewsEndToEndTests` | 18 / 0 / 0 | 18 / 0 / 0 |
| `RackCad.Tests.SelectiveDimensionViewsTests` | 139 / 0 / 0 | 139 / 0 / 0 |
| `RackCad.Tests.SelectiveDimensionsTests` | 13 / 0 / 0 | 13 / 0 / 0 |
| `RackCad.Tests.SelectiveDuplicationFailClosedTests` | 24 / 0 / 0 | 24 / 0 / 0 |
| `RackCad.Tests.SelectiveEditorOpenTests` | 33 / 0 / 0 | 33 / 0 / 0 |
| `RackCad.Tests.SelectiveEditorStateTests` | 29 / 0 / 0 | 29 / 0 / 0 |
| `RackCad.Tests.SelectiveEffectiveDesignResolverTests` | 20 / 0 / 0 | 20 / 0 / 0 |
| `RackCad.Tests.SelectiveFloorBeamRiseMaterializationTests` | 9 / 0 / 0 | 9 / 0 / 0 |
| `RackCad.Tests.SelectiveFloorBeamRiseTests` | 24 / 0 / 0 | 24 / 0 / 0 |
| `RackCad.Tests.SelectiveFondoCabeceraTests` | 33 / 0 / 0 | 33 / 0 / 0 |
| `RackCad.Tests.SelectiveFrontalBuilderTests` | 25 / 0 / 0 | 25 / 0 / 0 |
| `RackCad.Tests.SelectiveGate8ATests` | 21 / 0 / 0 | 21 / 0 / 0 |
| `RackCad.Tests.SelectiveGeometryResolverTests` | 22 / 0 / 0 | 22 / 0 / 0 |
| `RackCad.Tests.SelectiveHeaderBatchMutateTests` | 17 / 0 / 0 | 17 / 0 / 0 |
| `RackCad.Tests.SelectiveHeaderBatchNormalizationTests` | 8 / 0 / 0 | 8 / 0 / 0 |
| `RackCad.Tests.SelectiveHeaderBatchPersistenceTests` | 4 / 0 / 0 | 4 / 0 / 0 |
| `RackCad.Tests.SelectiveHeaderBatchPrepareTests` | 15 / 0 / 0 | 15 / 0 / 0 |
| `RackCad.Tests.SelectiveLateralBuilderTests` | 13 / 0 / 0 | 13 / 0 / 0 |
| `RackCad.Tests.SelectiveLibraryExportTests` | 21 / 0 / 0 | 21 / 0 / 0 |
| `RackCad.Tests.SelectiveMedioFrenteTests` | 14 / 0 / 0 | 14 / 0 / 0 |
| `RackCad.Tests.SelectiveMultiSelectionTests` | 43 / 0 / 0 | 43 / 0 / 0 |
| `RackCad.Tests.SelectivePalletDesignDocumentTests` | 13 / 0 / 0 | 13 / 0 / 0 |
| `RackCad.Tests.SelectiveParrillaPlacementTests` | 3 / 0 / 0 | 3 / 0 / 0 |
| `RackCad.Tests.SelectivePerFondoTests` | 30 / 0 / 0 | 30 / 0 / 0 |
| `RackCad.Tests.SelectivePlantaPlanTests` | 6 / 0 / 0 | 6 / 0 / 0 |
| `RackCad.Tests.SelectivePostGeometryTests` | 3 / 0 / 0 | 3 / 0 / 0 |
| `RackCad.Tests.SelectivePostTargetsTests` | 8 / 0 / 0 | 8 / 0 / 0 |
| `RackCad.Tests.SelectiveSafetyConfigTests` | 12 / 0 / 0 | 12 / 0 / 0 |
| `RackCad.Tests.SelectiveSafetyEquivalenceTests` | 7 / 0 / 0 | 7 / 0 / 0 |
| `RackCad.Tests.SelectiveSafetyGridTests` | 5 / 0 / 0 | 5 / 0 / 0 |
| `RackCad.Tests.SelectiveSafetyTests` | 49 / 0 / 0 | 49 / 0 / 0 |
| `RackCad.Tests.SelectiveSeparadorPlacementTests` | 1 / 0 / 0 | 1 / 0 / 0 |
| `RackCad.Tests.SelectiveTargetFondosIntegrationTests` | 13 / 0 / 0 | 13 / 0 / 0 |
| `RackCad.Tests.SelectiveTargetPreferenceTests` | 17 / 0 / 0 | 17 / 0 / 0 |
| `RackCad.Tests.SelectiveTargetResolverTests` | 34 / 0 / 0 | 34 / 0 / 0 |
| `RackCad.Tests.SelectiveTarimaPlacementTests` | 3 / 0 / 0 | 3 / 0 / 0 |
| `RackCad.Tests.SelectiveTopeFamilyIsolationGuardTests` | 3 / 0 / 0 | 3 / 0 / 0 |
| `RackCad.Tests.SelectiveTopeMultiFondoCharacterizationTests` | 46 / 0 / 0 | 46 / 0 / 0 |
| `RackCad.Tests.SelectiveTopePlacementTests` | 4 / 0 / 0 | 4 / 0 / 0 |
| `RackCad.Tests.SelectiveTopePlanFrontalTests` | 5 / 0 / 0 | 5 / 0 / 0 |
| `RackCad.Tests.SelectiveTopeSideMultiFondoContractTests` | 32 / 0 / 0 | 32 / 0 / 0 |
| `RackCad.Tests.SelectiveTopeSidePersistenceTests` | 19 / 0 / 0 | 19 / 0 / 0 |
| `RackCad.Tests.SelectiveTopeSideTests` | 20 / 0 / 0 | 20 / 0 / 0 |
| `RackCad.Tests.SelectiveTwentyBaysEquivalenceTests` | 4 / 0 / 0 | 4 / 0 / 0 |
| `RackCad.Tests.SeparatorLevelCalculatorTests` | 14 / 0 / 0 | 14 / 0 / 0 |
| `RackCad.Tests.SharedPhysicalFactsTests` | 5 / 0 / 0 | 5 / 0 / 0 |
| `RackCad.Tests.SharedViewCodecTests` | 4 / 0 / 0 | 4 / 0 / 0 |
| `RackCad.Tests.SharedViewFoundationCt04V2CharacterizationTests` | 7 / 0 / 0 | 7 / 0 / 0 |
| `RackCad.Tests.SharedViewFoundationF1CharacterizationTests` | 26 / 0 / 0 | 26 / 0 / 0 |
| `RackCad.Tests.SharedViewFoundationF5Tests` | 20 / 0 / 0 | 20 / 0 / 0 |
| `RackCad.Tests.SharedViewFoundationF6Tests` | 18 / 0 / 0 | 18 / 0 / 0 |
| `RackCad.Tests.ShippedCatalogIntegrityTests` | 5 / 0 / 0 | 5 / 0 / 0 |
| `RackCad.Tests.ShippedStructuralSectionCatalogTests` | 29 / 0 / 0 | 29 / 0 / 0 |
| `RackCad.Tests.SiblingRedrawSeamGuardTests` | 2 / 0 / 0 | 2 / 0 / 0 |
| `RackCad.Tests.SingleScanInsertGuardTests` | 5 / 0 / 0 | 5 / 0 / 0 |
| `RackCad.Tests.StructuralAnalysisCanonicalizerTests` | 12 / 0 / 0 | 12 / 0 / 0 |
| `RackCad.Tests.StructuralAnalysisCantileverBoundaryTests` | 30 / 0 / 0 | 30 / 0 / 0 |
| `RackCad.Tests.StructuralAnalysisCantileverGoldenTests` | 14 / 0 / 0 | 14 / 0 / 0 |
| `RackCad.Tests.StructuralAnalysisCantileverIdealisationTests` | 36 / 0 / 0 | 36 / 0 / 0 |
| `RackCad.Tests.StructuralAnalysisComparisonHardeningDumpReaderTests` | 14 / 0 / 0 | 14 / 0 / 0 |
| `RackCad.Tests.StructuralAnalysisComparisonHardeningIncompleteTests` | 24 / 0 / 0 | 24 / 0 / 0 |
| `RackCad.Tests.StructuralAnalysisComparisonHardeningMetricTests` | 31 / 0 / 0 | 31 / 0 / 0 |
| `RackCad.Tests.StructuralAnalysisComparisonHardeningReadbackTests` | 68 / 0 / 0 | 68 / 0 / 0 |
| `RackCad.Tests.StructuralAnalysisComparisonHardeningRehearsalTests` | 18 / 0 / 0 | 18 / 0 / 0 |
| `RackCad.Tests.StructuralAnalysisComparisonHardeningToolTests` | 36 / 0 / 0 | 36 / 0 / 0 |
| `RackCad.Tests.StructuralAnalysisComparisonRowTests` | 23 / 0 / 0 | 23 / 0 / 0 |
| `RackCad.Tests.StructuralAnalysisComparisonToolTests` | 28 / 0 / 0 | 28 / 0 / 0 |
| `RackCad.Tests.StructuralAnalysisComparisonVerdictTests` | 30 / 0 / 0 | 30 / 0 / 0 |
| `RackCad.Tests.StructuralAnalysisFingerprintTests` | 10 / 0 / 0 | 10 / 0 / 0 |
| `RackCad.Tests.StructuralAnalysisLoadFileReaderTests` | 51 / 0 / 0 | 51 / 0 / 0 |
| `RackCad.Tests.StructuralAnalysisMaterialProfileTests` | 11 / 0 / 0 | 11 / 0 / 0 |
| `RackCad.Tests.StructuralAnalysisModelBuilderTests` | 23 / 0 / 0 | 23 / 0 / 0 |
| `RackCad.Tests.StructuralAnalysisModelTests` | 10 / 0 / 0 | 10 / 0 / 0 |
| `RackCad.Tests.StructuralAnalysisOutcomeTests` | 12 / 0 / 0 | 12 / 0 / 0 |
| `RackCad.Tests.StructuralAnalysisPluginCommandGuardTests` | 112 / 0 / 0 | 112 / 0 / 0 |
| `RackCad.Tests.StructuralAnalysisPluginCommandProductTableTests` | 4 / 0 / 0 | 4 / 0 / 0 |
| `RackCad.Tests.StructuralAnalysisRamElementsAssumptionsTests` | 5 / 0 / 0 | 5 / 0 / 0 |
| `RackCad.Tests.StructuralAnalysisRamElementsAxesTests` | 96 / 0 / 0 | 96 / 0 / 0 |
| `RackCad.Tests.StructuralAnalysisRamElementsDumpPayloadTests` | 26 / 0 / 0 | 26 / 0 / 0 |
| `RackCad.Tests.StructuralAnalysisRamElementsDumpReaderTests` | 106 / 0 / 0 | 106 / 0 / 0 |
| `RackCad.Tests.StructuralAnalysisRamElementsDumpRoundTripTests` | 6 / 0 / 0 | 6 / 0 / 0 |
| `RackCad.Tests.StructuralAnalysisRamElementsDumpSourceTests` | 23 / 0 / 0 | 23 / 0 / 0 |
| `RackCad.Tests.StructuralAnalysisRamElementsDumpTemplateTests` | 60 / 0 / 0 | 60 / 0 / 0 |
| `RackCad.Tests.StructuralAnalysisRamElementsDumpTransferReaderTests` | 73 / 0 / 0 | 73 / 0 / 0 |
| `RackCad.Tests.StructuralAnalysisRamElementsGuardTests` | 24 / 0 / 0 | 24 / 0 / 0 |
| `RackCad.Tests.StructuralAnalysisRamElementsHardeningGateTests` | 46 / 0 / 0 | 46 / 0 / 0 |
| `RackCad.Tests.StructuralAnalysisRamElementsHardeningGuardTests` | 28 / 0 / 0 | 28 / 0 / 0 |
| `RackCad.Tests.StructuralAnalysisRamElementsHardeningPackageTests` | 45 / 0 / 0 | 45 / 0 / 0 |
| `RackCad.Tests.StructuralAnalysisRamElementsHardeningRaceTests` | 6 / 0 / 0 | 6 / 0 / 0 |
| `RackCad.Tests.StructuralAnalysisRamElementsHardeningReadBackTests` | 29 / 0 / 0 | 29 / 0 / 0 |
| `RackCad.Tests.StructuralAnalysisRamElementsMapReaderTests` | 111 / 0 / 0 | 111 / 0 / 0 |
| `RackCad.Tests.StructuralAnalysisRamElementsPackageContentTests` | 14 / 0 / 0 | 14 / 0 / 0 |
| `RackCad.Tests.StructuralAnalysisRamElementsPackageGoldenTests` | 5 / 0 / 0 | 5 / 0 / 0 |
| `RackCad.Tests.StructuralAnalysisRamElementsPackageManifestTests` | 31 / 0 / 0 | 31 / 0 / 0 |
| `RackCad.Tests.StructuralAnalysisRamElementsPackagePayloadTests` | 26 / 0 / 0 | 26 / 0 / 0 |
| `RackCad.Tests.StructuralAnalysisRamElementsPackageTemplateTests` | 51 / 0 / 0 | 51 / 0 / 0 |
| `RackCad.Tests.StructuralAnalysisRamElementsPackageValidationTests` | 63 / 0 / 0 | 63 / 0 / 0 |
| `RackCad.Tests.StructuralAnalysisRamElementsPackageWriterTests` | 51 / 0 / 0 | 51 / 0 / 0 |
| `RackCad.Tests.StructuralAnalysisRamElementsProjectionTests` | 28 / 0 / 0 | 28 / 0 / 0 |
| `RackCad.Tests.StructuralAnalysisRamElementsTranslationTests` | 33 / 0 / 0 | 33 / 0 / 0 |
| `RackCad.Tests.StructuralAnalysisSectionMapVerifiedTests` | 6 / 0 / 0 | 6 / 0 / 0 |
| `RackCad.Tests.StructuralAnalysisSourceEntryHardeningTests` | 37 / 0 / 0 | 37 / 0 / 0 |
| `RackCad.Tests.StructuralAnalysisSourceEntryTests` | 29 / 0 / 0 | 29 / 0 / 0 |
| `RackCad.Tests.StructuralAnalysisSourceGuardTests` | 86 / 0 / 0 | 86 / 0 / 0 |
| `RackCad.Tests.StructuralAnalysisUnitTests` | 56 / 0 / 0 | 56 / 0 / 0 |
| `RackCad.Tests.StructuralSectionGeometrySentinelTests` | 12 / 0 / 0 | 12 / 0 / 0 |
| `RackCad.Tests.StructuralSectionGeometryTests` | 18 / 0 / 0 | 18 / 0 / 0 |
| `RackCad.Tests.StructuralSectionIdNamespaceTests` | 21 / 0 / 0 | 21 / 0 / 0 |
| `RackCad.Tests.StructuralSectionImporterTests` | 20 / 0 / 0 | 20 / 0 / 0 |
| `RackCad.Tests.StructuralSectionLoadValidationTests` | 15 / 0 / 0 | 15 / 0 / 0 |
| `RackCad.Tests.StructuralSectionModelTests` | 37 / 0 / 0 | 37 / 0 / 0 |
| `RackCad.Tests.StructuralSectionPluginSourceGuardTests` | 45 / 0 / 0 | 45 / 0 / 0 |
| `RackCad.Tests.StructuralSectionProjectionTests` | 38 / 0 / 0 | 38 / 0 / 0 |
| `RackCad.Tests.StructuralSectionPublishTransactionTests` | 9 / 0 / 0 | 9 / 0 / 0 |
| `RackCad.Tests.StructuralSectionSFamilyTests` | 21 / 0 / 0 | 21 / 0 / 0 |
| `RackCad.Tests.StructuralSectionSearchTests` | 16 / 0 / 0 | 16 / 0 / 0 |
| `RackCad.Tests.StructuralSectionStrictReaderTests` | 25 / 0 / 0 | 25 / 0 / 0 |
| `RackCad.Tests.StructuralSectionValidatorTests` | 55 / 0 / 0 | 55 / 0 / 0 |
| `RackCad.Tests.StructuralSectionWireframeIntegrityTests` | 47 / 0 / 0 | 47 / 0 / 0 |
| `RackCad.Tests.SystemBomBuilderTests` | 16 / 0 / 0 | 16 / 0 / 0 |
| `RackCad.Tests.SystemKindPersistenceCharacterizationTests` | 31 / 0 / 0 | 31 / 0 / 0 |
| `RackCad.Tests.SystemRegistryTests` | 13 / 0 / 0 | 13 / 0 / 0 |
| `RackCad.Tests.UserSettingsStoreTests` | 4 / 0 / 0 | 4 / 0 / 0 |
| `RackCad.Tests.UserTemplateStoreTests` | 4 / 0 / 0 | 4 / 0 / 0 |
| `RackCad.Tests.UserTemplatesTests` | 5 / 0 / 0 | 5 / 0 / 0 |
| `RackCad.Tests.WarehouseAutoFillTests` | 13 / 0 / 0 | 13 / 0 / 0 |
| `RackCad.Tests.WarehouseFitCheckerTests` | 10 / 0 / 0 | 10 / 0 / 0 |
| `RackCad.Tests.WarehouseGridPlannerTests` | 19 / 0 / 0 | 19 / 0 / 0 |
| `RackCad.Tests.WorkspaceAssociativeBookkeepingTests` | 0 / 0 / 10 | 0 / 0 / 10 |
| `RackCad.Tests.WorkspaceBridgeAuthorizeTests` | 7 / 0 / 0 | 7 / 0 / 0 |
| `RackCad.Tests.WorkspaceBridgeCommandStateTests` | 20 / 0 / 0 | 20 / 0 / 0 |
| `RackCad.Tests.WorkspaceBridgeDegradedModeTests` | 11 / 0 / 0 | 11 / 0 / 0 |
| `RackCad.Tests.WorkspaceBridgeFacadeSourceGuardTests` | 18 / 0 / 0 | 18 / 0 / 0 |
| `RackCad.Tests.WorkspaceBridgeHintAndDrainTests` | 28 / 0 / 0 | 28 / 0 / 0 |
| `RackCad.Tests.WorkspaceBridgeNoActiveDocumentTests` | 11 / 0 / 0 | 11 / 0 / 0 |
| `RackCad.Tests.WorkspaceBridgeObjectSubjectGuardTests` | 20 / 0 / 0 | 20 / 0 / 0 |
| `RackCad.Tests.WorkspaceBridgeReconciliationTests` | 17 / 0 / 0 | 17 / 0 / 0 |
| `RackCad.Tests.WorkspaceBridgeSourceGuardTests` | 52 / 0 / 0 | 52 / 0 / 0 |
| `RackCad.Tests.WorkspaceBridgeSubscriptionLifecycleTests` | 17 / 0 / 0 | 17 / 0 / 0 |
| `RackCad.Tests.WorkspaceBrowserBoundaryTests` | 26 / 0 / 0 | 26 / 0 / 0 |
| `RackCad.Tests.WorkspaceBrowserHostGuardTests` | 19 / 0 / 0 | 19 / 0 / 0 |
| `RackCad.Tests.WorkspaceBrowserSessionRegistryTests` | 12 / 0 / 0 | 12 / 0 / 0 |
| `RackCad.Tests.WorkspaceBrowserSessionTests` | 44 / 0 / 0 | 44 / 0 / 0 |
| `RackCad.Tests.WorkspaceBrowserSummaryTests` | 31 / 0 / 0 | 31 / 0 / 0 |
| `RackCad.Tests.WorkspaceCommandAdvisorGuardTests` | 82 / 0 / 0 | 82 / 0 / 0 |
| `RackCad.Tests.WorkspaceCommandAdvisorTests` | 0 / 0 / 54 | 0 / 0 / 54 |
| `RackCad.Tests.WorkspaceDraftApplyTests` | 61 / 0 / 0 | 61 / 0 / 0 |
| `RackCad.Tests.WorkspaceDraftBoundaryTests` | 152 / 0 / 0 | 152 / 0 / 0 |
| `RackCad.Tests.WorkspaceDraftPilotIntentGuardTests` | 17 / 0 / 0 | 17 / 0 / 0 |
| `RackCad.Tests.WorkspaceDraftPilotIntentReadTests` | 82 / 0 / 0 | 82 / 0 / 0 |
| `RackCad.Tests.WorkspaceDraftPilotIntentStaleBaseTests` | 29 / 0 / 0 | 29 / 0 / 0 |
| `RackCad.Tests.WorkspaceDraftPilotMaterializationTests` | 78 / 0 / 0 | 78 / 0 / 0 |
| `RackCad.Tests.WorkspaceDraftPilotOwnershipTests` | 31 / 0 / 0 | 31 / 0 / 0 |
| `RackCad.Tests.WorkspaceDraftPilotStateTests` | 65 / 0 / 0 | 65 / 0 / 0 |
| `RackCad.Tests.WorkspaceDraftRegistryContainmentTests` | 23 / 0 / 0 | 23 / 0 / 0 |
| `RackCad.Tests.WorkspaceDraftRegistryTests` | 36 / 0 / 0 | 36 / 0 / 0 |
| `RackCad.Tests.WorkspaceDraftSessionTests` | 99 / 0 / 0 | 99 / 0 / 0 |
| `RackCad.Tests.WorkspaceDraftStaleBaseTests` | 50 / 0 / 0 | 50 / 0 / 0 |
| `RackCad.Tests.WorkspaceEditIntentGuardTests` | 145 / 0 / 7 | 145 / 0 / 7 |
| `RackCad.Tests.WorkspaceEditIntentHostTests` | 0 / 0 / 62 | 0 / 0 / 62 |
| `RackCad.Tests.WorkspaceFullCopyGuardTests` | 0 / 0 / 1 | 0 / 0 / 1 |
| `RackCad.Tests.WorkspaceHintDrainTests` | 11 / 0 / 0 | 11 / 0 / 0 |
| `RackCad.Tests.WorkspaceHostAuthorizationTests` | 13 / 0 / 0 | 13 / 0 / 0 |
| `RackCad.Tests.WorkspaceHostC2SourceGuardTests` | 15 / 0 / 0 | 15 / 0 / 0 |
| `RackCad.Tests.WorkspaceHostDrainTests` | 42 / 0 / 0 | 42 / 0 / 0 |
| `RackCad.Tests.WorkspaceHostHardeningGuardTests` | 99 / 0 / 1 | 99 / 0 / 1 |
| `RackCad.Tests.WorkspaceHostLifecycleTests` | 35 / 0 / 0 | 35 / 0 / 0 |
| `RackCad.Tests.WorkspaceHostPolicyTests` | 37 / 0 / 0 | 37 / 0 / 0 |
| `RackCad.Tests.WorkspaceHostPortGuardTests` | 33 / 0 / 0 | 33 / 0 / 0 |
| `RackCad.Tests.WorkspaceHostSourceGuardTests` | 45 / 0 / 0 | 45 / 0 / 0 |
| `RackCad.Tests.WorkspaceIdleSchedulerTests` | 48 / 0 / 0 | 48 / 0 / 0 |
| `RackCad.Tests.WorkspaceIdleSchedulingGuardTests` | 124 / 0 / 0 | 124 / 0 / 0 |
| `RackCad.Tests.WorkspaceInstrumentationTests` | 16 / 0 / 0 | 16 / 0 / 0 |
| `RackCad.Tests.WorkspaceLifecycleGuardTests` | 57 / 0 / 2 | 57 / 0 / 2 |
| `RackCad.Tests.WorkspaceLifecycleHostTests` | 0 / 0 / 69 | 0 / 0 / 69 |
| `RackCad.Tests.WorkspaceModelBoundaryTests` | 19 / 0 / 0 | 19 / 0 / 0 |
| `RackCad.Tests.WorkspaceNavigationActionPolicyTests` | 20 / 0 / 0 | 20 / 0 / 0 |
| `RackCad.Tests.WorkspaceNavigationAuthorityTests` | 10 / 0 / 0 | 10 / 0 / 0 |
| `RackCad.Tests.WorkspaceNavigationCacheTests` | 15 / 0 / 0 | 15 / 0 / 0 |
| `RackCad.Tests.WorkspaceNavigationEchoTests` | 18 / 0 / 0 | 18 / 0 / 0 |
| `RackCad.Tests.WorkspaceNavigationHostTests` | 67 / 0 / 0 | 67 / 0 / 0 |
| `RackCad.Tests.WorkspaceNavigationPrivacyGuardTests` | 11 / 0 / 0 | 11 / 0 / 0 |
| `RackCad.Tests.WorkspaceNavigationTargetsTests` | 23 / 0 / 0 | 23 / 0 / 0 |
| `RackCad.Tests.WorkspaceOwnMutationTests` | 18 / 0 / 0 | 18 / 0 / 0 |
| `RackCad.Tests.WorkspacePaletteHostSourceGuardTests` | 45 / 0 / 0 | 45 / 0 / 0 |
| `RackCad.Tests.WorkspacePilotCloseSyncGuardTests` | 119 / 0 / 0 | 119 / 0 / 0 |
| `RackCad.Tests.WorkspacePilotEditGuardTests` | 121 / 0 / 6 | 121 / 0 / 6 |
| `RackCad.Tests.WorkspacePilotEditHostTests` | 0 / 0 / 59 | 0 / 0 / 59 |
| `RackCad.Tests.WorkspacePilotPrepareGuardTests` | 22 / 0 / 3 | 22 / 0 / 3 |
| `RackCad.Tests.WorkspacePilotPrepareTests` | 0 / 0 / 67 | 0 / 0 / 67 |
| `RackCad.Tests.WorkspacePilotReconciliationGuardTests` | 25 / 0 / 1 | 25 / 0 / 1 |
| `RackCad.Tests.WorkspacePilotReconciliationHostTests` | 0 / 0 / 28 | 0 / 0 / 28 |
| `RackCad.Tests.WorkspacePilotSurfaceGuardTests` | 43 / 0 / 0 | 43 / 0 / 0 |
| `RackCad.Tests.WorkspacePilotUpdateGuardTests` | 85 / 0 / 2 | 85 / 0 / 2 |
| `RackCad.Tests.WorkspacePilotUpdateHostRunTests` | 27 / 0 / 0 | 27 / 0 / 0 |
| `RackCad.Tests.WorkspacePilotUpdateHostTests` | 0 / 0 / 35 | 0 / 0 / 35 |
| `RackCad.Tests.WorkspacePilotUpdateIntegrationGuardTests` | 121 / 0 / 4 | 121 / 0 / 4 |
| `RackCad.Tests.WorkspacePilotUpdateRequestTests` | 0 / 0 / 44 | 0 / 0 / 44 |
| `RackCad.Tests.WorkspacePilotUpdateRunTests` | 57 / 0 / 0 | 57 / 0 / 0 |
| `RackCad.Tests.WorkspaceSelectionContextTests` | 13 / 0 / 0 | 13 / 0 / 0 |
| `RackCad.Tests.WorkspaceSelectionMappingTests` | 27 / 0 / 0 | 27 / 0 / 0 |
| `RackCad.Tests.WorkspaceSessionTests` | 8 / 0 / 0 | 8 / 0 / 0 |
| `RackCad.Tests.WorkspaceSourceGuardExceptionTests` | 88 / 0 / 0 | 88 / 0 / 0 |
| `RackCad.Tests.XlsxWriterTests` | 12 / 0 / 0 | 12 / 0 / 0 |
| `RackCad.UI.Tests.CantileverAdvancedPanelWindowTests` | 10 / 0 / 0 | 10 / 0 / 0 |
| `RackCad.UI.Tests.CantileverComponentEditorTests` | 33 / 0 / 0 | 33 / 0 / 0 |
| `RackCad.UI.Tests.CantileverEditorWindowTests` | 24 / 0 / 0 | 24 / 0 / 0 |
| `RackCad.UI.Tests.FullPanelCopyWorkspacePilotReconciliationGreenTests` | 30 / 0 / 0 | 30 / 0 / 0 |
| `RackCad.UI.Tests.FullPanelCopyWorkspacePilotReconciliationViewGreenTests` | 6 / 0 / 0 | 6 / 0 / 0 |
| `RackCad.UI.Tests.NumericFieldTests` | 10 / 0 / 0 | 10 / 0 / 0 |
| `RackCad.UI.Tests.NumericFieldValidationTests` | 13 / 0 / 0 | 13 / 0 / 0 |
| `RackCad.UI.Tests.PushBackClassicCharacterizationCloseTests` | 21 / 0 / 0 | 21 / 0 / 0 |
| `RackCad.UI.Tests.PushBackClassicCharacterizationIntentTests` | 12 / 0 / 0 | 12 / 0 / 0 |
| `RackCad.UI.Tests.PushBackClassicCharacterizationKeyboardTests` | 4 / 0 / 0 | 4 / 0 / 0 |
| `RackCad.UI.Tests.PushBackClassicCharacterizationOwnerTests` | 4 / 0 / 0 | 4 / 0 / 0 |
| `RackCad.UI.Tests.PushBackClassicCharacterizationResourceTests` | 2 / 0 / 0 | 2 / 0 / 0 |
| `RackCad.UI.Tests.PushBackClassicCharacterizationSourceGuardTests` | 12 / 0 / 0 | 12 / 0 / 0 |
| `RackCad.UI.Tests.PushBackDeclaredCorrection2MixedBoxTests` | 1 / 0 / 0 | 1 / 0 / 0 |
| `RackCad.UI.Tests.PushBackDeclaredCorrection2RoundingTests` | 3 / 0 / 0 | 3 / 0 / 0 |
| `RackCad.UI.Tests.PushBackDeclaredCorrectionDimensionStyleTests` | 2 / 0 / 0 | 2 / 0 / 0 |
| `RackCad.UI.Tests.PushBackDeclaredCorrectionPilotParityTests` | 2 / 0 / 0 | 2 / 0 / 0 |
| `RackCad.UI.Tests.PushBackDeclaredCorrectionRoundingTests` | 3 / 0 / 0 | 3 / 0 / 0 |
| `RackCad.UI.Tests.PushBackDeclaredCorrectionSideBNoticeTests` | 2 / 0 / 0 | 2 / 0 / 0 |
| `RackCad.UI.Tests.PushBackDeclaredCorrectionStandardRestoreTests` | 1 / 0 / 0 | 1 / 0 / 0 |
| `RackCad.UI.Tests.PushBackEditAllApplyCloseTests` | 0 / 1 / 0 | 1 / 0 / 0 |
| `RackCad.UI.Tests.PushBackFieldValidationConcordanceTests` | 1 / 0 / 0 | 1 / 0 / 0 |
| `RackCad.UI.Tests.PushBackFullDraftWindowParityTests` | 1 / 0 / 0 | 1 / 0 / 0 |
| `RackCad.UI.Tests.PushBackFullEditorCharacterizationCatalogTests` | 1 / 0 / 0 | 1 / 0 / 0 |
| `RackCad.UI.Tests.PushBackFullEditorCharacterizationCompositeTests` | 4 / 0 / 0 | 4 / 0 / 0 |
| `RackCad.UI.Tests.PushBackFullEditorCharacterizationModuleTests` | 2 / 0 / 1 | 2 / 0 / 1 |
| `RackCad.UI.Tests.PushBackFullEditorCharacterizationRoundingLossTests` | 1 / 0 / 1 | 1 / 0 / 1 |
| `RackCad.UI.Tests.PushBackFullEditorCharacterizationStateTests` | 3 / 0 / 0 | 3 / 0 / 0 |
| `RackCad.UI.Tests.PushBackFullEditorCharacterizationSubdialogTests` | 5 / 0 / 0 | 5 / 0 / 0 |
| `RackCad.UI.Tests.PushBackFullPanelFieldTests` | 3 / 0 / 0 | 3 / 0 / 0 |
| `RackCad.UI.Tests.PushBackStateNetActionsTests` | 2 / 0 / 0 | 2 / 0 / 0 |
| `RackCad.UI.Tests.PushBackStateNetCellAndFrontTests` | 4 / 0 / 0 | 4 / 0 / 0 |
| `RackCad.UI.Tests.PushBackStateNetCompositeTests` | 3 / 0 / 0 | 3 / 0 / 0 |
| `RackCad.UI.Tests.PushBackStateNetFingerprintTests` | 1 / 0 / 0 | 1 / 0 / 0 |
| `RackCad.UI.Tests.PushBackStateNetModuleAndSafetyTests` | 2 / 0 / 0 | 2 / 0 / 0 |
| `RackCad.UI.Tests.PushBackStateNetRackControlsTests` | 2 / 0 / 0 | 2 / 0 / 0 |
| `RackCad.UI.Tests.PushBackWindowOnStateTests` | 2 / 0 / 0 | 2 / 0 / 0 |
| `RackCad.UI.Tests.PushBackWorkspaceApplyCloseTests` | 3 / 0 / 1 | 3 / 0 / 1 |
| `RackCad.UI.Tests.PushBackWorkspaceCarryoverTests` | 3 / 0 / 0 | 3 / 0 / 0 |
| `RackCad.UI.Tests.PushBackWorkspaceModeTests` | 3 / 0 / 0 | 3 / 0 / 0 |
| `RackCad.UI.Tests.WorkspaceBrowserScenarioHarnessTests` | 5 / 0 / 0 | 5 / 0 / 0 |
| `RackCad.UI.Tests.WorkspaceBrowserViewModelTests` | 49 / 0 / 0 | 49 / 0 / 0 |
| `RackCad.UI.Tests.WorkspaceBrowserViewTests` | 13 / 0 / 0 | 13 / 0 / 0 |
| `RackCad.UI.Tests.WorkspaceDraftPilotCanonicalLineEndingTests` | 9 / 0 / 0 | 9 / 0 / 0 |
| `RackCad.UI.Tests.WorkspaceDraftPilotValidationParityTests` | 9 / 0 / 0 | 9 / 0 / 0 |
| `RackCad.UI.Tests.WorkspaceDraftPilotWindowLoadCharacterizationTests` | 3 / 0 / 1 | 3 / 0 / 1 |
| `RackCad.UI.Tests.WorkspaceNavigationViewModelTests` | 73 / 0 / 0 | 73 / 0 / 0 |
| `RackCad.UI.Tests.WorkspaceNavigationViewTests` | 18 / 0 / 0 | 18 / 0 / 0 |
| `RackCad.UI.Tests.WorkspacePanelContextTests` | 15 / 0 / 0 | 15 / 0 / 0 |
| `RackCad.UI.Tests.WorkspacePanelErrorTests` | 11 / 0 / 0 | 11 / 0 / 0 |
| `RackCad.UI.Tests.WorkspacePanelInstrumentationTests` | 10 / 0 / 0 | 10 / 0 / 0 |
| `RackCad.UI.Tests.WorkspacePanelNoWorkTests` | 8 / 0 / 0 | 8 / 0 / 0 |
| `RackCad.UI.Tests.WorkspacePanelStateTests` | 12 / 0 / 0 | 12 / 0 / 0 |
| `RackCad.UI.Tests.WorkspacePanelViewTests` | 8 / 0 / 0 | 8 / 0 / 0 |
| `RackCad.UI.Tests.WorkspacePilotCloseSyncTests` | 78 / 0 / 0 | 78 / 0 / 0 |
| `RackCad.UI.Tests.WorkspacePilotCloseSyncViewTests` | 17 / 0 / 0 | 17 / 0 / 0 |
| `RackCad.UI.Tests.WorkspacePilotFieldViewModelTests` | 160 / 0 / 0 | 160 / 0 / 0 |
| `RackCad.UI.Tests.WorkspacePilotFieldViewTests` | 43 / 0 / 0 | 43 / 0 / 0 |
| `RackCad.UI.Tests.WorkspacePilotReconciliationTests` | 100 / 0 / 21 | 100 / 0 / 21 |
| `RackCad.UI.Tests.WorkspacePilotReconciliationViewTests` | 3 / 0 / 6 | 3 / 0 / 6 |
| **Total (764 clases)** | **20032 / 1 / 492** | **20033 / 0 / 492** |

TRX leidas (38): RED core-red.trx f8eac38a2f77; RED r-ui-01.trx ce67b1bd56a8; RED r-ui-02.trx e8bfb01c6483; RED r-ui-03.trx 86c10d63f2b9; RED r-ui-04.trx 0c088cce2b2c; RED r-ui-05.trx d6075a139f98; RED r-ui-06.trx a709f117a567; RED r-ui-07.trx 9932fb23471c; RED r-ui-08.trx 81e1500c58a7; RED r-ui-09.trx ff1426bfc5a3; RED r-ui-10.trx abcb92dd3c68; RED r-ui-11.trx f148c7c4e456; RED r-ui-12.trx 57f1f4403081; RED r-ui-13.trx ad0a80885fa3; RED r-ui-14.trx 2d109c9839c5; RED r-ui-15.trx 6b0af16ee4a0; RED r-ui-16.trx c8a7b0cc5050; RED r-ui-17.trx b742a315b950; RED r-ui-18.trx b869d117851a; GREEN core-green.trx a0d26d57abeb; GREEN g-ui-01.trx 145a821058f0; GREEN g-ui-02.trx da57cc62091b; GREEN g-ui-03.trx 4878cf7ae313; GREEN g-ui-04.trx 58ee0020d988; GREEN g-ui-05.trx 0423401e409d; GREEN g-ui-06.trx 5c6c2cfcf38e; GREEN g-ui-07.trx cc72ef3f51eb; GREEN g-ui-08.trx d04ca8c3306d; GREEN g-ui-09.trx 55ddae7becb2; GREEN g-ui-10.trx a9e123d40e63; GREEN g-ui-11.trx cdb7b879cded; GREEN g-ui-12.trx 02748c253285; GREEN g-ui-13.trx 882a620296da; GREEN g-ui-14.trx 1ded113559b0; GREEN g-ui-15.trx 3be2e1094f1e; GREEN g-ui-16.trx e12a22e51924; GREEN g-ui-17.trx 8fb115e3be7c; GREEN g-ui-18.trx d55a11d860f5

### `/d/r5i`: el paso 8 (el ensayo de las CT de texto) y las sondas del paso 7

Registro: `driver-r5i-en.txt` (`d3f0635ee26e5fca01761c9b20174be6a8f82fe506df05b07bffe52f77ffb34f`) y `r5i-en/`. El ensayo (`pbt4c-rehearsal-textct.cs`, como `tests/RackCad.Tests/TmpPbt4cTextCt.cs`) sobre el RED y, tras el GREEN, otra vez con las dos sondas.

- en-red-core rc=0 errores=0 avisos-CS=0
- en-green-core rc=0 errores=0 avisos-CS=0
- en-green-ui rc=0 errores=0 avisos-CS=0
- **El ensayo, RED:** ensayo red: PASA; **GREEN:** ensayo green: PASA (`r5i-en/ensayo-red.txt` y `ensayo-green.txt`). En el RED, CT-01, 08 y 13 caen por su `Assert.Equal` (la primera línea del StackTrace, en `TmpPbt4cTextCt.cs`) y CT-07 y CT-15 pasan; en el GREEN, las cinco pasan.
- **Las sondas:** las líneas de Core (`r5i-en/sonda-core-lineas.txt`, `cbbd8457…a59f`) y de UI (`sonda-ui-lineas.txt`, `3ee13f60…85ce`), **iguales por sha256 a las de `/d/r5f`** (la v5.0).

**El resultado de cada clase que corre en `/d/r5i`** (el ensayo en el RED; el ensayo y las dos sondas en el GREEN (las sondas acaban en `Assert.Fail` con sus líneas: sus dos `Failed` son las sondas, no una caída); `v51-clases/r5i.txt`, sha256 `044bd5bf…2233`; Passed / Failed / Skipped):

| Clase | RED (P / F / S) | GREEN (P / F / S) |
|---|---|---|
| `RackCad.Tests.TmpPbt4cEditAllProbe` | — | 0 / 1 / 0 |
| `RackCad.Tests.TmpPbt4cTextCt01` | 0 / 1 / 0 | 1 / 0 / 0 |
| `RackCad.Tests.TmpPbt4cTextCt07` | 1 / 0 / 0 | 1 / 0 / 0 |
| `RackCad.Tests.TmpPbt4cTextCt08` | 0 / 1 / 0 | 1 / 0 / 0 |
| `RackCad.Tests.TmpPbt4cTextCt13` | 0 / 1 / 0 | 1 / 0 / 0 |
| `RackCad.Tests.TmpPbt4cTextCt15` | 1 / 0 / 0 | 1 / 0 / 0 |
| `RackCad.UI.Tests.TmpPbt4cCloseProbe` | — | 0 / 1 / 0 |
| **Total (7 clases)** | **2 / 3 / 0** | **5 / 2 / 0** |

TRX leidas (4): RED en-red.trx de055654cadd; GREEN en-green.trx d8ba0a0db1f1; GREEN sonda-core.trx 236a6a8ae4d3; GREEN sonda-ui.trx 9be9f7ef93f7

### El censo de C13 (paso 8, por texto; sin copia)

`census_c13_v51.py` (`83a88e38…43cc`) por objetos git de `6adff90d` y las piezas de la v5.1: **PASA** (`v51-census/census-c13.txt`, `be049da4…2519`).

- src/RackCad.Application/Workspace/IWorkspacePilotEditPort.cs: nuevas=1 esperadas=1 igual=True quitadas=[] | esqueleto nuevas=1 dentro-de-la-lista=True quitadas=[]
- src/RackCad.Application/Workspace/WorkspaceEditIntent.cs: nuevas=21 esperadas=21 igual=True quitadas=[] | esqueleto nuevas=21 dentro-de-la-lista=True quitadas=[]
- src/RackCad.Application/Workspace/WorkspaceHostCore.cs: nuevas=8 esperadas=8 igual=True quitadas=[] | esqueleto nuevas=1 dentro-de-la-lista=True quitadas=[]
- src/RackCad.Application/Workspace/WorkspacePilotSupport.cs: nuevas=1 esperadas=1 igual=True quitadas=[] | esqueleto nuevas=0 dentro-de-la-lista=True quitadas=[]
- src/RackCad.Plugin/Workspace/WorkspaceController.cs: nuevas=6 esperadas=6 igual=True quitadas=[] | esqueleto nuevas=0 dentro-de-la-lista=True quitadas=[]
- src/RackCad.UI/Workspace/WorkspacePilotFieldViewModel.cs: nuevas=7 esperadas=7 igual=True quitadas=[] | esqueleto nuevas=2 dentro-de-la-lista=True quitadas=[]
- src/RackCad.UI/Workspace/WorkspacePanelView.xaml: nuevas=1 esperadas=1 igual=True quitadas=[] | esqueleto nuevas=0 dentro-de-la-lista=True quitadas=[]
- src/RackCad.UI/Workspace/WorkspacePanelView.xaml.cs: nuevas=1 esperadas=1 igual=True quitadas=[] | esqueleto nuevas=0 dentro-de-la-lista=True quitadas=[]
- src/RackCad.UI/Systems/PushBack/RackPushBackSystemWindow.xaml.cs: nuevas=1 esperadas=1 igual=True quitadas=[] | esqueleto nuevas=0 dentro-de-la-lista=True quitadas=[]
- src/RackCad.Application/Workspace/IWorkspaceFullEditHost.cs: nuevas=2 esperadas=2 igual=True quitadas=[] | esqueleto nuevas=2 dentro-de-la-lista=True quitadas=[]
- src/RackCad.Application/Systems/PushBack/Draft/PushBackFullDraftLaunch.cs: nuevas=3 esperadas=3 igual=True quitadas=[] | esqueleto nuevas=2 dentro-de-la-lista=True quitadas=[]
- src/RackCad.Application/Systems/PushBack/Draft/PushBackFullDraftStatePort.cs: nuevas=5 esperadas=5 igual=True quitadas=[] | esqueleto nuevas=5 dentro-de-la-lista=True quitadas=[]
- src/RackCad.Plugin/Views/RackWorkspaceEditorWindow.cs: nuevas=2 esperadas=2 igual=True quitadas=[] | esqueleto nuevas=2 dentro-de-la-lista=True quitadas=[]
- nombres de RackCad.Application.Workspace: violaciones=[]
- autoprueba-constante=falla
- autoprueba-sobrecarga=falla
- autoprueba-propiedad=falla
- autoprueba-miembro-de-enum=falla
- autoprueba-tipo-anidado=falla
- autoprueba-x:Name=falla
- autoprueba-quitada=falla
- Lo que no ve: la visibilidad y la firma (solo el nombre y su cuenta), los miembros generados por el compilador, las declaraciones locales y las lambdas; la reflexion sobre el ensamblado la mide la suite completa de Core.
- censo C13: PASA

### `/d/r5k`: el coste de `IsDirty` (paso 7)

Registro: `driver-r5k-id.txt` (`6122f1afeb1016512c10d874f463b41fb4cc91caa347ed6d1909c2e1f1d54e9c`) y `r5k-id/`. La sonda de PB-T4b (`pbt4bcc/compile/TmpIsDirtyCostProbe.cs`, sin tocarla) sobre el GREEN de la v5.1, en Release y sola en la máquina:

- pb-01-base | IsDirty limpio | mediana=3.3779 ms | p95=5.3195 ms | n=1000
- pb-01-base | IsDirty sucio | mediana=3.1584 ms | p95=4.4443 ms | n=1000
- pb-07-combinado | IsDirty limpio | mediana=4.1369 ms | p95=5.9035 ms | n=1000
- pb-07-combinado | IsDirty sucio | mediana=4.1363 ms | p95=5.5366 ms | n=1000
- pb-20-ab | IsDirty limpio | mediana=9.7677 ms | p95=12.3808 ms | n=1000
- pb-20-ab | IsDirty sucio | mediana=9.6555 ms | p95=12.5184 ms | n=1000

**El resultado de cada clase que corre en `/d/r5k`** (la sonda del coste, en Release (acaba en `Assert.Fail` con sus medidas); `v51-clases/r5k.txt`, sha256 `2fa9e261…f3a6`; Passed / Failed / Skipped):

| Clase | GREEN (P / F / S) |
|---|---|
| `RackCad.Tests.TmpIsDirtyCostProbe` | 0 / 1 / 0 |
| **Total (1 clases)** | **0 / 1 / 0** |

TRX leidas (1): GREEN isdirty.trx 1adff95131ee

### `/d/r5l`: el control del ensayo sobre las piezas de la v5.0

Registro: `driver-r5l-cc.txt` (`f685599ecdef5462d8f03846f941b58587f63f237644b319635396228bc2103d`) y `r5l-cc/`. El mismo ensayo sobre `apply_red_pbt4c_v50.py` y `apply_green_pbt4c_v50.py` (con `#nullable enable` en los dos archivos de `Draft/`): **RED, VE I-01; GREEN, VE I-01** (`r5l-cc/control-red.txt` y `control-green.txt`). CT-15 cae en las dos ramas, con su primera diferencia en la línea de la pureza; las otras cuatro, como con la v5.1.

- cc-red-core rc=0 errores=0 avisos-CS=0
- test cc-red.trx trx-sha256=54578149c1d4984b89b9eec2c992d6597a608911e10228bab6eedd5edfed12d1 :: filas 5 {'Failed': 4, 'Passed': 1}; inicio 2026-11-21T13:05:47.8735032-06:00; fin 2026-11-21T13:05:47.9358546-06:00
- cc-green-core rc=0 errores=0 avisos-CS=0
- test cc-green.trx trx-sha256=356956f9054d9cf2ebcf2f102ee65f7ed673204f637f3becde677d195a905dc6 :: filas 5 {'Failed': 1, 'Passed': 4}; inicio 2026-11-21T13:06:43.2430533-06:00; fin 2026-11-21T13:06:43.3500323-06:00

**El resultado de cada clase que corre en `/d/r5l`** (el ensayo sobre las piezas de la v5.0, en cada rama; `v51-clases/r5l.txt`, sha256 `2fbbb8e8…a78f`; Passed / Failed / Skipped):

| Clase | RED (P / F / S) | GREEN (P / F / S) |
|---|---|---|
| `RackCad.Tests.TmpPbt4cTextCt01` | 0 / 1 / 0 | 1 / 0 / 0 |
| `RackCad.Tests.TmpPbt4cTextCt07` | 1 / 0 / 0 | 1 / 0 / 0 |
| `RackCad.Tests.TmpPbt4cTextCt08` | 0 / 1 / 0 | 1 / 0 / 0 |
| `RackCad.Tests.TmpPbt4cTextCt13` | 0 / 1 / 0 | 1 / 0 / 0 |
| `RackCad.Tests.TmpPbt4cTextCt15` | 0 / 1 / 0 | 0 / 1 / 0 |
| **Total (5 clases)** | **1 / 4 / 0** | **4 / 1 / 0** |

TRX leidas (2): RED cc-red.trx 54578149c1d4; GREEN cc-green.trx 356956f9054d

## La historia: la v5.0 y la vuelta parada

**No valen para la emisión de la v5.1:** las piezas que midieron no son las de la v5.1. Se custodian como historia.

| Copia | Qué midió | Registro |
|---|---|---|
| `/d/r5a` | La sonda de la lista: solo los dos `Skip` de la lista de 2, sin GREEN. Core 19 605 = 19 144 + 1 + 460; cae CT-PBT4B-20 → `owner-pbt4c-list-3-decision.md` | `pbt4/v50/probe-skips-r5a-driver.txt` y `pbt4/v50/out-probe-skips-r5a/` |
| `/d/r5b` | La compilación del RED y del GREEN con `IWorkspaceFullEditorHost`: 0 errores y 0 `warning CS` | `driver-compile-r5b.txt` |
| `/d/r5c` | La precondición 6 con la lista de 3: cae `WorkspaceModelBoundaryTests` (por «Editor») en las dos ramas y, en el GREEN, CT-PBT2B-06 → D-S9 y `owner-pbt4c-list-4-decision.md` | `driver-redgreen-r5c.txt` y `out-redgreen-v50-r5c/` |
| `/d/r5d` | La sonda de CT-03, 04, 05 y 12 con las piezas de la primera vuelta | `driver-probes-r5d.txt` |
| `/d/r5e` | La medida de D-S9: el RED y el GREEN con `IWorkspaceFullEditHost`; Core 19 608 = 19 147 + 0 + 461 en las dos ramas | `pbt4/v50/variant-rename/driver-redgreen-r5e.txt` |
| `/d/r5f` | v5.0: las sondas del paso 7 (las mismas líneas que `/d/r5i`) | `driver-probes-v50b-r5f.txt` y `out-probes-v50b-r5f/` |
| `/d/r5g` | v5.0: los pasos 2 a 6 con las piezas de la v5.0 (con `#nullable enable` en `Draft/`: CA-T4c I-01, que no vio porque no corre las CT). Core 19 608 = 19 147 + 0 + 461 en las dos ramas | `driver-redgreen-v50b-r5g.txt` y `out-redgreen-v50b-r5g/` |
| `/d/r5h` | v5.0: el coste de `IsDirty` | `driver-isdirty-v50-r5h.txt` y `out-isdirty-v50-r5h/` |

**Las salidas de `/d/r5c` y `/d/r5g` que la custodia solo lleva por sha256** (su ruta, con el prefijo de `cc4c/`, pasa de 156 caracteres): `out-redgreen-v50-r5c/green-ui-RackCad_UI_Tests_FullPanelCopyWorkspacePilotReconciliationGreenTests-outcomes.txt` (168), `out-redgreen-v50-r5c/green-ui-RackCad_UI_Tests_FullPanelCopyWorkspacePilotReconciliationGreenTests.log` (159), `out-redgreen-v50-r5c/green-ui-RackCad_UI_Tests_FullPanelCopyWorkspacePilotReconciliationViewGreenTests-outcomes.txt` (172), `out-redgreen-v50-r5c/green-ui-RackCad_UI_Tests_FullPanelCopyWorkspacePilotReconciliationViewGreenTests.log` (163), `out-redgreen-v50-r5c/red-ui-RackCad_UI_Tests_FullPanelCopyWorkspacePilotReconciliationGreenTests-outcomes.txt` (166), `out-redgreen-v50-r5c/red-ui-RackCad_UI_Tests_FullPanelCopyWorkspacePilotReconciliationGreenTests.log` (157), `out-redgreen-v50-r5c/red-ui-RackCad_UI_Tests_FullPanelCopyWorkspacePilotReconciliationViewGreenTests-outcomes.txt` (170), `out-redgreen-v50-r5c/red-ui-RackCad_UI_Tests_FullPanelCopyWorkspacePilotReconciliationViewGreenTests.log` (161), `out-redgreen-v50b-r5g/green-ui-RackCad_UI_Tests_FullPanelCopyWorkspacePilotReconciliationGreenTests-outcomes.txt` (169), `out-redgreen-v50b-r5g/green-ui-RackCad_UI_Tests_FullPanelCopyWorkspacePilotReconciliationGreenTests.log` (160), `out-redgreen-v50b-r5g/green-ui-RackCad_UI_Tests_FullPanelCopyWorkspacePilotReconciliationViewGreenTests-outcomes.txt` (173), `out-redgreen-v50b-r5g/green-ui-RackCad_UI_Tests_FullPanelCopyWorkspacePilotReconciliationViewGreenTests.log` (164), `out-redgreen-v50b-r5g/green-ui-RackCad_UI_Tests_PushBackFieldValidationConcordanceTests-outcomes.txt` (157), `out-redgreen-v50b-r5g/red-ui-RackCad_UI_Tests_FullPanelCopyWorkspacePilotReconciliationGreenTests-outcomes.txt` (167), `out-redgreen-v50b-r5g/red-ui-RackCad_UI_Tests_FullPanelCopyWorkspacePilotReconciliationGreenTests.log` (158), `out-redgreen-v50b-r5g/red-ui-RackCad_UI_Tests_FullPanelCopyWorkspacePilotReconciliationViewGreenTests-outcomes.txt` (171), `out-redgreen-v50b-r5g/red-ui-RackCad_UI_Tests_FullPanelCopyWorkspacePilotReconciliationViewGreenTests.log` (162). Su resultado queda aquí, en la tabla de cada filtro y en la de cada clase de su corrida.

**El resultado de cada filtro de `/d/r5c`** (la primera línea de cada `*-outcomes.txt` de `out-redgreen-v50-r5c/`):

| Registro | sha256 | Resultados | Passed | Failed | Skipped |
|---|---|---|---|---|---|
| `core-green-outcomes.txt` | `9bf24792…69f2` | 19608 | 19146 | 1 | 461 |
| `core-red-outcomes.txt` | `9e82f45a…73f7` | 19608 | 19146 | 1 | 461 |
| `green-ui-RackCad_UI_Tests_Cantilever-outcomes.txt` | `ecb7c081…7bba` | 67 | 67 | 0 | 0 |
| `green-ui-RackCad_UI_Tests_FullPanelCopyWorkspacePilotReconciliationGreenTests-outcomes.txt` | `dd1e9cdb…ea49` | 30 | 30 | 0 | 0 |
| `green-ui-RackCad_UI_Tests_FullPanelCopyWorkspacePilotReconciliationViewGreenTests-outcomes.txt` | `a0c664b1…cc3a` | 6 | 6 | 0 | 0 |
| `green-ui-RackCad_UI_Tests_NumericFieldTests-outcomes.txt` | `a9b25477…90c8` | 10 | 10 | 0 | 0 |
| `green-ui-RackCad_UI_Tests_NumericFieldValidationTests-outcomes.txt` | `37511a7b…2ad9` | 13 | 13 | 0 | 0 |
| `green-ui-RackCad_UI_Tests_PushBackClassicCharacterization-outcomes.txt` | `5ee0c93e…af51` | 55 | 55 | 0 | 0 |
| `green-ui-RackCad_UI_Tests_PushBackDeclaredCorrection-outcomes.txt` | `443c6e80…09d9` | 14 | 14 | 0 | 0 |
| `green-ui-RackCad_UI_Tests_PushBackFieldValidationConcordanceTests-outcomes.txt` | `fc6dc59b…039a` | 1 | 1 | 0 | 0 |
| `green-ui-RackCad_UI_Tests_PushBackFullDraftWindowParityTests-outcomes.txt` | `86914ce1…7a63` | 1 | 1 | 0 | 0 |
| `green-ui-RackCad_UI_Tests_PushBackFullEditorCharacterization-outcomes.txt` | `13f23631…c154` | 18 | 16 | 0 | 2 |
| `green-ui-RackCad_UI_Tests_PushBackFullPanelFieldTests-outcomes.txt` | `e23206b5…44a3` | 3 | 3 | 0 | 0 |
| `green-ui-RackCad_UI_Tests_PushBackStateNet-outcomes.txt` | `0d777217…ae17` | 14 | 14 | 0 | 0 |
| `green-ui-RackCad_UI_Tests_PushBackWindowOnStateTests-outcomes.txt` | `f0a00e72…b87a` | 2 | 2 | 0 | 0 |
| `green-ui-RackCad_UI_Tests_PushBackWorkspaceApplyCloseTests-outcomes.txt` | `21d88d45…258c` | 4 | 3 | 1 | 0 |
| `green-ui-RackCad_UI_Tests_PushBackWorkspaceCarryoverTests-outcomes.txt` | `dd74fd32…f157` | 3 | 3 | 0 | 0 |
| `green-ui-RackCad_UI_Tests_PushBackWorkspaceModeTests-outcomes.txt` | `edf32e8d…c3e2` | 3 | 3 | 0 | 0 |
| `green-ui-RackCad_UI_Tests_Workspace-outcomes.txt` | `664271b7…8457` | 672 | 644 | 0 | 28 |
| `red-ui-RackCad_UI_Tests_Cantilever-outcomes.txt` | `9372d22a…c622` | 67 | 67 | 0 | 0 |
| `red-ui-RackCad_UI_Tests_FullPanelCopyWorkspacePilotReconciliationGreenTests-outcomes.txt` | `d2d87717…8d34` | 30 | 30 | 0 | 0 |
| `red-ui-RackCad_UI_Tests_FullPanelCopyWorkspacePilotReconciliationViewGreenTests-outcomes.txt` | `5172244a…bbe2` | 6 | 6 | 0 | 0 |
| `red-ui-RackCad_UI_Tests_NumericFieldTests-outcomes.txt` | `dc4823c4…a36d` | 10 | 10 | 0 | 0 |
| `red-ui-RackCad_UI_Tests_NumericFieldValidationTests-outcomes.txt` | `87c0356e…5f4d` | 13 | 13 | 0 | 0 |
| `red-ui-RackCad_UI_Tests_PushBackClassicCharacterization-outcomes.txt` | `70d08121…77c9` | 55 | 55 | 0 | 0 |
| `red-ui-RackCad_UI_Tests_PushBackDeclaredCorrection-outcomes.txt` | `486b34a0…2ee0` | 14 | 14 | 0 | 0 |
| `red-ui-RackCad_UI_Tests_PushBackFieldValidationConcordanceTests-outcomes.txt` | `4de5a115…30c5` | 1 | 1 | 0 | 0 |
| `red-ui-RackCad_UI_Tests_PushBackFullDraftWindowParityTests-outcomes.txt` | `331f33cd…efd5` | 1 | 1 | 0 | 0 |
| `red-ui-RackCad_UI_Tests_PushBackFullEditorCharacterization-outcomes.txt` | `2f5fd122…4677` | 18 | 16 | 0 | 2 |
| `red-ui-RackCad_UI_Tests_PushBackFullPanelFieldTests-outcomes.txt` | `ecee59e3…ec67` | 3 | 3 | 0 | 0 |
| `red-ui-RackCad_UI_Tests_PushBackStateNet-outcomes.txt` | `f73a46da…6775` | 14 | 14 | 0 | 0 |
| `red-ui-RackCad_UI_Tests_PushBackWindowOnStateTests-outcomes.txt` | `c707b9f6…d68a` | 2 | 2 | 0 | 0 |
| `red-ui-RackCad_UI_Tests_PushBackWorkspaceApplyCloseTests-outcomes.txt` | `39652332…99f4` | 4 | 4 | 0 | 0 |
| `red-ui-RackCad_UI_Tests_PushBackWorkspaceCarryoverTests-outcomes.txt` | `c3729f0a…e48d` | 3 | 3 | 0 | 0 |
| `red-ui-RackCad_UI_Tests_PushBackWorkspaceModeTests-outcomes.txt` | `283316de…e61f` | 3 | 3 | 0 | 0 |
| `red-ui-RackCad_UI_Tests_Workspace-outcomes.txt` | `cea4c713…9c8f` | 672 | 644 | 0 | 28 |

**El resultado de cada clase que corre en `/d/r5c`** (la suite completa de Core y los filtros de UI de la v5.0, en cada rama (las TRX, en la copia exportada); `v51-clases/r5c.txt`, sha256 `a260ba07…e3fd`; Passed / Failed / Skipped):

| Clase | RED (P / F / S) | GREEN (P / F / S) |
|---|---|---|
| `RackCad.Tests.AgentExecutionProtocolTests` | 17 / 0 / 0 | 17 / 0 / 0 |
| `RackCad.Tests.AiscWorkbookVerificationTests` | 11 / 0 / 0 | 11 / 0 / 0 |
| `RackCad.Tests.AtomicFileTests` | 4 / 0 / 0 | 4 / 0 / 0 |
| `RackCad.Tests.BasePlatePeraltePersistenceTests` | 2 / 0 / 0 | 2 / 0 / 0 |
| `RackCad.Tests.BeamCatalogTests` | 3 / 0 / 0 | 3 / 0 / 0 |
| `RackCad.Tests.BlankFrontBoundaryTests` | 15 / 0 / 0 | 15 / 0 / 0 |
| `RackCad.Tests.BlankFrontSafetyTests` | 9 / 0 / 0 | 9 / 0 / 0 |
| `RackCad.Tests.BlankFrontTests` | 28 / 0 / 0 | 28 / 0 / 0 |
| `RackCad.Tests.BlankIdPredicateScopeGuardTests` | 2 / 0 / 0 | 2 / 0 / 0 |
| `RackCad.Tests.BlockNamingTests` | 8 / 0 / 0 | 8 / 0 / 0 |
| `RackCad.Tests.BomBuilderTests` | 6 / 0 / 0 | 6 / 0 / 0 |
| `RackCad.Tests.BomExcelCharacterizationTests` | 5 / 0 / 0 | 5 / 0 / 0 |
| `RackCad.Tests.BomExcelPivotTests` | 14 / 0 / 0 | 14 / 0 / 0 |
| `RackCad.Tests.BomExcelSmokeFixtures` | 1 / 0 / 0 | 1 / 0 / 0 |
| `RackCad.Tests.BomExcelSystemMatrixTests` | 15 / 0 / 0 | 15 / 0 / 0 |
| `RackCad.Tests.BracingPanelMemberBuilderTests` | 11 / 0 / 0 | 11 / 0 / 0 |
| `RackCad.Tests.CallerOwnedFacadeGuardTests` | 81 / 0 / 0 | 81 / 0 / 0 |
| `RackCad.Tests.CallerOwnedTransactionGuardTests` | 15 / 0 / 0 | 15 / 0 / 0 |
| `RackCad.Tests.CantileverAdapterSectionViewTests` | 12 / 0 / 0 | 12 / 0 / 0 |
| `RackCad.Tests.CantileverAdvancedPanelLayoutTests` | 27 / 0 / 0 | 27 / 0 / 0 |
| `RackCad.Tests.CantileverArmConnectionMetricsCharacterizationTests` | 9 / 0 / 0 | 9 / 0 / 0 |
| `RackCad.Tests.CantileverArmDefaultsAndFrontalTests` | 14 / 0 / 0 | 14 / 0 / 0 |
| `RackCad.Tests.CantileverArmTests` | 117 / 0 / 0 | 117 / 0 / 0 |
| `RackCad.Tests.CantileverBraceRepresentationTests` | 32 / 0 / 0 | 32 / 0 / 0 |
| `RackCad.Tests.CantileverColumnBaseFollowTests` | 15 / 0 / 0 | 15 / 0 / 0 |
| `RackCad.Tests.CantileverColumnBaseTests` | 84 / 0 / 0 | 84 / 0 / 0 |
| `RackCad.Tests.CantileverDoubleStationLayoutTests` | 8 / 0 / 0 | 8 / 0 / 0 |
| `RackCad.Tests.CantileverLateralViewTests` | 12 / 0 / 0 | 12 / 0 / 0 |
| `RackCad.Tests.CantileverLineEditorAssemblerTests` | 10 / 0 / 0 | 10 / 0 / 0 |
| `RackCad.Tests.CantileverLineTests` | 73 / 0 / 0 | 73 / 0 / 0 |
| `RackCad.Tests.CantileverPersistenceAndViewTests` | 28 / 0 / 0 | 28 / 0 / 0 |
| `RackCad.Tests.CantileverPlantaViewTests` | 8 / 0 / 0 | 8 / 0 / 0 |
| `RackCad.Tests.CantileverPluginSourceGuardTests` | 17 / 0 / 0 | 17 / 0 / 0 |
| `RackCad.Tests.CantileverPunchRepresentationTests` | 14 / 0 / 0 | 14 / 0 / 0 |
| `RackCad.Tests.CantileverRegularPunchGridCharacterizationTests` | 16 / 0 / 0 | 16 / 0 / 0 |
| `RackCad.Tests.CantileverRetiredMarginTests` | 11 / 0 / 0 | 11 / 0 / 0 |
| `RackCad.Tests.CantileverRoundThreeCorrectionTests` | 27 / 0 / 0 | 27 / 0 / 0 |
| `RackCad.Tests.CantileverRoundTwoCharacterizationTests` | 12 / 0 / 0 | 12 / 0 / 0 |
| `RackCad.Tests.CantileverRoundTwoSourceGuardTests` | 38 / 0 / 0 | 38 / 0 / 0 |
| `RackCad.Tests.CantileverSeparatorWebTests` | 6 / 0 / 0 | 6 / 0 / 0 |
| `RackCad.Tests.CantileverSourceGuardTests` | 90 / 0 / 0 | 90 / 0 / 0 |
| `RackCad.Tests.CantileverStationCorrectionTests` | 78 / 0 / 0 | 78 / 0 / 0 |
| `RackCad.Tests.CantileverStationTests` | 81 / 0 / 0 | 81 / 0 / 0 |
| `RackCad.Tests.CantileverVerticalDatumTests` | 15 / 0 / 0 | 15 / 0 / 0 |
| `RackCad.Tests.CantileverVisualRoleTests` | 15 / 0 / 0 | 15 / 0 / 0 |
| `RackCad.Tests.CatalogBackedStandardServiceTests` | 4 / 0 / 0 | 4 / 0 / 0 |
| `RackCad.Tests.CatalogBlockManifestTests` | 13 / 0 / 0 | 13 / 0 / 0 |
| `RackCad.Tests.CatalogBlockParametersTests` | 8 / 0 / 0 | 8 / 0 / 0 |
| `RackCad.Tests.CatalogCanonicalIdsTests` | 1 / 0 / 0 | 1 / 0 / 0 |
| `RackCad.Tests.CatalogDirectoryTests` | 2 / 0 / 0 | 2 / 0 / 0 |
| `RackCad.Tests.CatalogManifestGuardTests` | 3 / 0 / 0 | 3 / 0 / 0 |
| `RackCad.Tests.CatalogStandardConsistencyTests` | 5 / 0 / 0 | 5 / 0 / 0 |
| `RackCad.Tests.CatalogValidatorTests` | 22 / 0 / 0 | 22 / 0 / 0 |
| `RackCad.Tests.CommonTransform2DTests` | 6 / 0 / 0 | 6 / 0 / 0 |
| `RackCad.Tests.ComputedParametersPopulationGuardTests` | 3 / 0 / 0 | 3 / 0 / 0 |
| `RackCad.Tests.ComputedParametersPopulationTests` | 51 / 0 / 0 | 51 / 0 / 0 |
| `RackCad.Tests.ComputedParametersRackMetricIdsTests` | 10 / 0 / 0 | 10 / 0 / 0 |
| `RackCad.Tests.ComputedParametersRackMetricProviderPurityTests` | 3 / 0 / 0 | 3 / 0 / 0 |
| `RackCad.Tests.ComputedParametersRackMetricRequestTests` | 14 / 0 / 0 | 14 / 0 / 0 |
| `RackCad.Tests.ComputedParametersSummaryContextTests` | 3 / 0 / 0 | 3 / 0 / 0 |
| `RackCad.Tests.ComputedParametersSummaryGuardTests` | 2 / 0 / 0 | 2 / 0 / 0 |
| `RackCad.Tests.ComputedParametersSummaryPerformanceTests` | 6 / 0 / 0 | 6 / 0 / 0 |
| `RackCad.Tests.ComputedParametersSummaryTests` | 18 / 0 / 0 | 18 / 0 / 0 |
| `RackCad.Tests.ComputedParametersSymbolsBindingTests` | 22 / 0 / 0 | 22 / 0 / 0 |
| `RackCad.Tests.ComputedParametersSymbolsConsumersTests` | 4 / 0 / 0 | 4 / 0 / 0 |
| `RackCad.Tests.ComputedParametersSymbolsContextTests` | 14 / 0 / 0 | 14 / 0 / 0 |
| `RackCad.Tests.ComputedParametersSymbolsFormattingTests` | 12 / 0 / 0 | 12 / 0 / 0 |
| `RackCad.Tests.ComputedParametersSymbolsIdentityTests` | 33 / 0 / 0 | 33 / 0 / 0 |
| `RackCad.Tests.ComputedParametersSymbolsPersistenceTests` | 4 / 0 / 0 | 4 / 0 / 0 |
| `RackCad.Tests.ConsolidatedBomBuilderTests` | 2 / 0 / 0 | 2 / 0 / 0 |
| `RackCad.Tests.ConsolidatedBomXlsxDetailTests` | 13 / 0 / 0 | 13 / 0 / 0 |
| `RackCad.Tests.CsvCatalogReaderTests` | 9 / 0 / 0 | 9 / 0 / 0 |
| `RackCad.Tests.CsvLexerTests` | 10 / 0 / 0 | 10 / 0 / 0 |
| `RackCad.Tests.CustomPropertiesAuthorityTests` | 87 / 0 / 0 | 87 / 0 / 0 |
| `RackCad.Tests.CustomPropertiesCommandGuardTests` | 48 / 0 / 0 | 48 / 0 / 0 |
| `RackCad.Tests.CustomPropertiesCommitTests` | 35 / 0 / 0 | 35 / 0 / 0 |
| `RackCad.Tests.CustomPropertiesEdgeGuardTests` | 52 / 0 / 0 | 52 / 0 / 0 |
| `RackCad.Tests.CustomPropertiesEnvelopeCharacterizationTests` | 103 / 0 / 0 | 103 / 0 / 0 |
| `RackCad.Tests.CustomPropertiesEnvelopeGuardTests` | 22 / 0 / 0 | 22 / 0 / 0 |
| `RackCad.Tests.CustomPropertiesEnvelopeTests` | 150 / 0 / 0 | 150 / 0 / 0 |
| `RackCad.Tests.CustomPropertiesMutationTests` | 17 / 0 / 0 | 17 / 0 / 0 |
| `RackCad.Tests.CustomPropertiesProjectTests` | 32 / 0 / 0 | 32 / 0 / 0 |
| `RackCad.Tests.CustomPropertiesStoreTests` | 228 / 0 / 0 | 228 / 0 / 0 |
| `RackCad.Tests.CustomPropertiesWorkspaceTests` | 21 / 0 / 0 | 21 / 0 / 0 |
| `RackCad.Tests.DebugFaultInjectionGuardTests` | 1 / 0 / 0 | 1 / 0 / 0 |
| `RackCad.Tests.DiagnosticsNegativeTests` | 3 / 0 / 0 | 3 / 0 / 0 |
| `RackCad.Tests.DimensionViewPolicyTests` | 297 / 0 / 0 | 297 / 0 / 0 |
| `RackCad.Tests.DimensionViewsCopySitesTests` | 24 / 0 / 0 | 24 / 0 / 0 |
| `RackCad.Tests.DimensionViewsLegacyJsonTests` | 3 / 0 / 0 | 3 / 0 / 0 |
| `RackCad.Tests.DimensionViewsNoneWinsTests` | 36 / 0 / 0 | 36 / 0 / 0 |
| `RackCad.Tests.DimensionViewsPhysicalAndBomInvarianceTests` | 72 / 0 / 0 | 72 / 0 / 0 |
| `RackCad.Tests.DimensionViewsRestampTests` | 11 / 0 / 0 | 11 / 0 / 0 |
| `RackCad.Tests.DrawServicePlanBaselineTests` | 8 / 0 / 0 | 8 / 0 / 0 |
| `RackCad.Tests.DrawingUnitsAdvisoryTests` | 6 / 0 / 0 | 6 / 0 / 0 |
| `RackCad.Tests.DynamicDepthGeometryTests` | 6 / 0 / 0 | 6 / 0 / 0 |
| `RackCad.Tests.DynamicDimensionLegacyCharacterizationTests` | 5 / 0 / 0 | 5 / 0 / 0 |
| `RackCad.Tests.DynamicDimensionViewsDocumentTests` | 9 / 0 / 0 | 9 / 0 / 0 |
| `RackCad.Tests.DynamicDimensionViewsEndToEndTests` | 10 / 0 / 0 | 10 / 0 / 0 |
| `RackCad.Tests.DynamicDimensionViewsTests` | 66 / 0 / 0 | 66 / 0 / 0 |
| `RackCad.Tests.DynamicEditorCellTests` | 12 / 0 / 0 | 12 / 0 / 0 |
| `RackCad.Tests.DynamicEditorDesignAssemblerTests` | 8 / 0 / 0 | 8 / 0 / 0 |
| `RackCad.Tests.DynamicEditorSafetyTests` | 7 / 0 / 0 | 7 / 0 / 0 |
| `RackCad.Tests.DynamicEntranceGuidePlanTests` | 2 / 0 / 0 | 2 / 0 / 0 |
| `RackCad.Tests.DynamicFlowBedLateralBuilderTests` | 4 / 0 / 0 | 4 / 0 / 0 |
| `RackCad.Tests.DynamicForkliftDefensePlanTests` | 2 / 0 / 0 | 2 / 0 / 0 |
| `RackCad.Tests.DynamicFrontGeometryTests` | 4 / 0 / 0 | 4 / 0 / 0 |
| `RackCad.Tests.DynamicFrontMatrixTests` | 20 / 0 / 0 | 20 / 0 / 0 |
| `RackCad.Tests.DynamicHeaderBatchLengthTests` | 6 / 0 / 0 | 6 / 0 / 0 |
| `RackCad.Tests.DynamicHeaderBatchMutateTests` | 11 / 0 / 0 | 11 / 0 / 0 |
| `RackCad.Tests.DynamicHeaderBatchPersistenceTests` | 4 / 0 / 0 | 4 / 0 / 0 |
| `RackCad.Tests.DynamicHeaderBatchPrepareTests` | 18 / 0 / 0 | 18 / 0 / 0 |
| `RackCad.Tests.DynamicHeaderBatchReconciliationTests` | 4 / 0 / 0 | 4 / 0 / 0 |
| `RackCad.Tests.DynamicHeaderHeightCalculatorTests` | 7 / 0 / 0 | 7 / 0 / 0 |
| `RackCad.Tests.DynamicLoadBeamGeometryTests` | 4 / 0 / 0 | 4 / 0 / 0 |
| `RackCad.Tests.DynamicModuleTargetsTests` | 18 / 0 / 0 | 18 / 0 / 0 |
| `RackCad.Tests.DynamicNullOverrideGoldenTests` | 1 / 0 / 0 | 1 / 0 / 0 |
| `RackCad.Tests.DynamicPostLevelsTests` | 3 / 0 / 0 | 3 / 0 / 0 |
| `RackCad.Tests.DynamicRackSystemBuilderTests` | 21 / 0 / 0 | 21 / 0 / 0 |
| `RackCad.Tests.DynamicRackSystemResolverTests` | 11 / 0 / 0 | 11 / 0 / 0 |
| `RackCad.Tests.DynamicSafetyDefaultsTests` | 2 / 0 / 0 | 2 / 0 / 0 |
| `RackCad.Tests.DynamicSystemLateralBuilderTests` | 18 / 0 / 0 | 18 / 0 / 0 |
| `RackCad.Tests.DynamicSystemMultiViewBuilderTests` | 12 / 0 / 0 | 12 / 0 / 0 |
| `RackCad.Tests.ElevationOverrideSourceGuardTests` | 3 / 0 / 0 | 3 / 0 / 0 |
| `RackCad.Tests.ExpressionBinderTests` | 65 / 0 / 0 | 65 / 0 / 0 |
| `RackCad.Tests.ExpressionBoundLimitsTests` | 16 / 0 / 0 | 16 / 0 / 0 |
| `RackCad.Tests.ExpressionCanonicalShapeTests` | 15 / 0 / 0 | 15 / 0 / 0 |
| `RackCad.Tests.ExpressionCoreGuardTests` | 6 / 0 / 0 | 6 / 0 / 0 |
| `RackCad.Tests.ExpressionDiagnosticCatalogTests` | 2 / 0 / 0 | 2 / 0 / 0 |
| `RackCad.Tests.ExpressionEvaluatorTests` | 15 / 0 / 0 | 15 / 0 / 0 |
| `RackCad.Tests.ExpressionFormatterTests` | 54 / 0 / 0 | 54 / 0 / 0 |
| `RackCad.Tests.ExpressionFunctionRegistryTests` | 34 / 0 / 0 | 34 / 0 / 0 |
| `RackCad.Tests.ExpressionParserGuardTests` | 38 / 0 / 0 | 38 / 0 / 0 |
| `RackCad.Tests.ExpressionRoundTripTests` | 12 / 0 / 0 | 12 / 0 / 0 |
| `RackCad.Tests.ExpressionSymbolModelTests` | 44 / 0 / 0 | 44 / 0 / 0 |
| `RackCad.Tests.ExpressionSyntaxDiagnosticsTests` | 223 / 0 / 0 | 223 / 0 / 0 |
| `RackCad.Tests.ExpressionSyntaxGrammarTests` | 122 / 0 / 0 | 122 / 0 / 0 |
| `RackCad.Tests.ExpressionTokenTablesTests` | 3 / 0 / 0 | 3 / 0 / 0 |
| `RackCad.Tests.FirstViewFreedomGuardTests` | 5 / 0 / 0 | 5 / 0 / 0 |
| `RackCad.Tests.FlowBedBomBuilderTests` | 2 / 0 / 0 | 2 / 0 / 0 |
| `RackCad.Tests.FlowBedCatalogTests` | 2 / 0 / 0 | 2 / 0 / 0 |
| `RackCad.Tests.FlowBedConfigurationStoreTests` | 2 / 0 / 0 | 2 / 0 / 0 |
| `RackCad.Tests.FlowBedLateralBuilderTests` | 9 / 0 / 0 | 9 / 0 / 0 |
| `RackCad.Tests.FrameModelValidatorTests` | 10 / 0 / 0 | 10 / 0 / 0 |
| `RackCad.Tests.FullPanelCopyPushBackFullReadGuardRedTests` | 1 / 0 / 0 | 1 / 0 / 0 |
| `RackCad.Tests.FullPanelCopyPushBackFullReadPathRedTests` | 1 / 0 / 0 | 1 / 0 / 0 |
| `RackCad.Tests.FullPanelCopyPushBackWritePathGuardRedTests` | 1 / 0 / 0 | 1 / 0 / 0 |
| `RackCad.Tests.FullPanelCopyWorkspaceAssociativeBookkeepingGreenTests` | 21 / 0 / 0 | 21 / 0 / 0 |
| `RackCad.Tests.FullPanelCopyWorkspaceAssociativeBookkeepingRedTests` | 1 / 0 / 0 | 1 / 0 / 0 |
| `RackCad.Tests.FullPanelCopyWorkspaceCommandAdvisorGreenTests` | 70 / 0 / 0 | 70 / 0 / 0 |
| `RackCad.Tests.FullPanelCopyWorkspaceCommandAdvisorRedTests` | 6 / 0 / 0 | 6 / 0 / 0 |
| `RackCad.Tests.FullPanelCopyWorkspaceEditIntentGuardRedTests` | 13 / 0 / 0 | 13 / 0 / 0 |
| `RackCad.Tests.FullPanelCopyWorkspaceEditIntentHostGreenTests` | 57 / 0 / 0 | 57 / 0 / 0 |
| `RackCad.Tests.FullPanelCopyWorkspaceEditIntentHostRedTests` | 24 / 0 / 0 | 24 / 0 / 0 |
| `RackCad.Tests.FullPanelCopyWorkspaceHostHardeningGuardRedTests` | 1 / 0 / 0 | 1 / 0 / 0 |
| `RackCad.Tests.FullPanelCopyWorkspaceLifecycleGuardRedTests` | 2 / 0 / 0 | 2 / 0 / 0 |
| `RackCad.Tests.FullPanelCopyWorkspaceLifecycleHostGreenTests` | 69 / 0 / 0 | 69 / 0 / 0 |
| `RackCad.Tests.FullPanelCopyWorkspaceLifecycleHostRedTests` | 24 / 0 / 0 | 24 / 0 / 0 |
| `RackCad.Tests.FullPanelCopyWorkspacePilotEditGuardGreenTests` | 17 / 0 / 0 | 17 / 0 / 0 |
| `RackCad.Tests.FullPanelCopyWorkspacePilotEditGuardRedTests` | 6 / 0 / 0 | 6 / 0 / 0 |
| `RackCad.Tests.FullPanelCopyWorkspacePilotEditHostGreenTests` | 17 / 0 / 0 | 17 / 0 / 0 |
| `RackCad.Tests.FullPanelCopyWorkspacePilotEditHostRedTests` | 59 / 0 / 0 | 59 / 0 / 0 |
| `RackCad.Tests.FullPanelCopyWorkspacePilotPrepareGreenTests` | 47 / 0 / 0 | 47 / 0 / 0 |
| `RackCad.Tests.FullPanelCopyWorkspacePilotPrepareRedTests` | 44 / 0 / 0 | 44 / 0 / 0 |
| `RackCad.Tests.FullPanelCopyWorkspacePilotReconciliationGuardGreenTests` | 1 / 0 / 0 | 1 / 0 / 0 |
| `RackCad.Tests.FullPanelCopyWorkspacePilotReconciliationHostGreenTests` | 5 / 0 / 0 | 5 / 0 / 0 |
| `RackCad.Tests.FullPanelCopyWorkspacePilotReconciliationHostRedTests` | 23 / 0 / 0 | 23 / 0 / 0 |
| `RackCad.Tests.FullPanelCopyWorkspacePilotUpdateGuardRedTests` | 1 / 0 / 0 | 1 / 0 / 0 |
| `RackCad.Tests.FullPanelCopyWorkspacePilotUpdateHostGreenTests` | 18 / 0 / 0 | 18 / 0 / 0 |
| `RackCad.Tests.FullPanelCopyWorkspacePilotUpdateHostRedTests` | 29 / 0 / 0 | 29 / 0 / 0 |
| `RackCad.Tests.FullPanelCopyWorkspacePilotUpdateIntegrationGuardRedTests` | 2 / 0 / 0 | 2 / 0 / 0 |
| `RackCad.Tests.FullPanelCopyWorkspacePilotUpdateRequestGreenTests` | 1 / 0 / 0 | 1 / 0 / 0 |
| `RackCad.Tests.FullPanelCopyWorkspacePilotUpdateRequestRedTests` | 53 / 0 / 0 | 53 / 0 / 0 |
| `RackCad.Tests.G10EditorContractTests` | 22 / 0 / 0 | 22 / 0 / 0 |
| `RackCad.Tests.G10LegacyEditorCharacterizationTests` | 8 / 0 / 0 | 8 / 0 / 0 |
| `RackCad.Tests.G10VariablesContractTests` | 9 / 0 / 0 | 9 / 0 / 0 |
| `RackCad.Tests.G11PerformanceMeasurementTests` | 3 / 0 / 0 | 3 / 0 / 0 |
| `RackCad.Tests.G11RealChainCandidateTests` | 2 / 0 / 0 | 2 / 0 / 0 |
| `RackCad.Tests.G12A1CreateExpressionHotfixTests` | 8 / 0 / 0 | 8 / 0 / 0 |
| `RackCad.Tests.G16BlockNameTests` | 14 / 0 / 0 | 14 / 0 / 0 |
| `RackCad.Tests.G16ElevationProjectionTests` | 98 / 0 / 0 | 98 / 0 / 0 |
| `RackCad.Tests.G16FirstViewCorrectiveTests` | 61 / 0 / 0 | 61 / 0 / 0 |
| `RackCad.Tests.G16OrientationAuthorityTests` | 256 / 0 / 0 | 256 / 0 / 0 |
| `RackCad.Tests.G16OrientationGuardTests` | 5 / 0 / 0 | 5 / 0 / 0 |
| `RackCad.Tests.G16ProjectedEnvelopeTests` | 42 / 0 / 0 | 42 / 0 / 0 |
| `RackCad.Tests.G16ProjectedOrientationTests` | 48 / 0 / 0 | 48 / 0 / 0 |
| `RackCad.Tests.G16UnnamedRackTests` | 28 / 0 / 0 | 28 / 0 / 0 |
| `RackCad.Tests.G7DependencyGraphContractTests` | 8 / 0 / 0 | 8 / 0 / 0 |
| `RackCad.Tests.G7RegistryEvaluationContractTests` | 12 / 0 / 0 | 12 / 0 / 0 |
| `RackCad.Tests.G8LegacyPersistenceCharacterizationTests` | 9 / 0 / 0 | 9 / 0 / 0 |
| `RackCad.Tests.G8PersistenceIntegrationContractTests` | 53 / 0 / 0 | 53 / 0 / 0 |
| `RackCad.Tests.G9A3ContractMatrixTests` | 44 / 0 / 0 | 44 / 0 / 0 |
| `RackCad.Tests.G9ConformanceFixTests` | 6 / 0 / 0 | 6 / 0 / 0 |
| `RackCad.Tests.G9LegacyMutationCharacterizationTests` | 10 / 0 / 0 | 10 / 0 / 0 |
| `RackCad.Tests.G9MutationPropagationContractTests` | 23 / 0 / 0 | 23 / 0 / 0 |
| `RackCad.Tests.GeometryMateSolverTests` | 3 / 0 / 0 | 3 / 0 / 0 |
| `RackCad.Tests.GeometryPrimitivesTests` | 34 / 0 / 0 | 34 / 0 / 0 |
| `RackCad.Tests.HardcodedStandardRackFrameServiceTests` | 3 / 0 / 0 | 3 / 0 / 0 |
| `RackCad.Tests.HeaderBatchContractTests` | 31 / 0 / 0 | 31 / 0 / 0 |
| `RackCad.Tests.HeaderConfigurationSnapshotTests` | 19 / 0 / 0 | 19 / 0 / 0 |
| `RackCad.Tests.HeaderInstanceGrouperTests` | 6 / 0 / 0 | 6 / 0 / 0 |
| `RackCad.Tests.I55CantileverPlantaVisibilityPropagationTests` | 1 / 0 / 0 | 1 / 0 / 0 |
| `RackCad.Tests.I55DeferredProductFixCharacterizationTests` | 3 / 0 / 0 | 3 / 0 / 0 |
| `RackCad.Tests.I55G14FoundationConsumerTests` | 5 / 0 / 0 | 5 / 0 / 0 |
| `RackCad.Tests.I55G15Auth15ConsumerTests` | 5 / 0 / 0 | 5 / 0 / 0 |
| `RackCad.Tests.I58Conf58SchemaWhitespaceTests` | 580 / 0 / 0 | 580 / 0 / 0 |
| `RackCad.Tests.I58F1CharacterizationTests` | 1272 / 0 / 0 | 1272 / 0 / 0 |
| `RackCad.Tests.I58F1OracleChecks` | 640 / 0 / 0 | 640 / 0 / 0 |
| `RackCad.Tests.I58F2BoundaryTests` | 26 / 0 / 0 | 26 / 0 / 0 |
| `RackCad.Tests.I58F2MaterializationTests` | 3 / 0 / 0 | 3 / 0 / 0 |
| `RackCad.Tests.I58F2ReaderTests` | 408 / 0 / 0 | 408 / 0 / 0 |
| `RackCad.Tests.I58F3SeamTests` | 56 / 0 / 0 | 56 / 0 / 0 |
| `RackCad.Tests.I59F1CharacterizationTests` | 14 / 0 / 0 | 14 / 0 / 0 |
| `RackCad.Tests.I59F2BoundaryGuardTests` | 2 / 0 / 0 | 2 / 0 / 0 |
| `RackCad.Tests.I59F2ContractTests` | 59 / 0 / 0 | 59 / 0 / 0 |
| `RackCad.Tests.I59F3ConsumerIntegrationTests` | 8 / 0 / 0 | 8 / 0 / 0 |
| `RackCad.Tests.I59F3CorrectionTests` | 13 / 0 / 0 | 13 / 0 / 0 |
| `RackCad.Tests.I59F4ConformanceTests` | 4 / 0 / 0 | 4 / 0 / 0 |
| `RackCad.Tests.I60BatchCreationNamingTests` | 25 / 0 / 0 | 25 / 0 / 0 |
| `RackCad.Tests.I60RackLogicalNameTests` | 35 / 0 / 0 | 35 / 0 / 0 |
| `RackCad.Tests.I60RackNameWiringGuardTests` | 22 / 0 / 0 | 22 / 0 / 0 |
| `RackCad.Tests.I61CamaEditWiringGuardTests` | 10 / 0 / 0 | 10 / 0 / 0 |
| `RackCad.Tests.I61EditedRackNameTests` | 6 / 0 / 0 | 6 / 0 / 0 |
| `RackCad.Tests.InsertBlankIdFlowCharacterizationTests` | 7 / 0 / 0 | 7 / 0 / 0 |
| `RackCad.Tests.InsertRedrawLayerAccessCharacterizationTests` | 5 / 0 / 0 | 5 / 0 / 0 |
| `RackCad.Tests.JsonRackCatalogProviderTests` | 6 / 0 / 0 | 6 / 0 / 0 |
| `RackCad.Tests.KindDispatchTests` | 21 / 0 / 0 | 21 / 0 / 0 |
| `RackCad.Tests.KindHandlerGuardSourceTests` | 2 / 0 / 0 | 2 / 0 / 0 |
| `RackCad.Tests.LargueroBomBuilderTests` | 3 / 0 / 0 | 3 / 0 / 0 |
| `RackCad.Tests.LateralHeaderLayoutBuilderTests` | 18 / 0 / 0 | 18 / 0 / 0 |
| `RackCad.Tests.LateralHeaderParametersFactoryTests` | 4 / 0 / 0 | 4 / 0 / 0 |
| `RackCad.Tests.LegacyViewPayloadCompositionCharacterizationTests` | 8 / 0 / 0 | 8 / 0 / 0 |
| `RackCad.Tests.LengthUnitsAuthorityGuardTests` | 25 / 0 / 0 | 25 / 0 / 0 |
| `RackCad.Tests.LengthUnitsTests` | 5 / 0 / 0 | 5 / 0 / 0 |
| `RackCad.Tests.LevelElevationResolutionTests` | 1095 / 0 / 0 | 1095 / 0 / 0 |
| `RackCad.Tests.LevelSpacingH58ReproductionTests` | 3 / 0 / 0 | 3 / 0 / 0 |
| `RackCad.Tests.LinkedPropertyEditSessionTests` | 31 / 0 / 0 | 31 / 0 / 0 |
| `RackCad.Tests.LinkedPropertyFoundationTests` | 21 / 0 / 0 | 21 / 0 / 0 |
| `RackCad.Tests.LinkedPropertyKernelTests` | 31 / 0 / 0 | 31 / 0 / 0 |
| `RackCad.Tests.LinkedPropertyReconcilerTests` | 16 / 0 / 0 | 16 / 0 / 0 |
| `RackCad.Tests.LinkedPropertyViewPayloadTests` | 6 / 0 / 0 | 6 / 0 / 0 |
| `RackCad.Tests.LocalizedNumberParserTests` | 9 / 0 / 0 | 9 / 0 / 0 |
| `RackCad.Tests.MainMenuStructuralSectionAccessGuardTests` | 28 / 0 / 0 | 28 / 0 / 0 |
| `RackCad.Tests.MultiPropertyRealProofTests` | 30 / 0 / 0 | 30 / 0 / 0 |
| `RackCad.Tests.NamespaceFolderGuardTests` | 7 / 0 / 0 | 7 / 0 / 0 |
| `RackCad.Tests.NoProductPlannerGuardTests` | 3 / 0 / 0 | 3 / 0 / 0 |
| `RackCad.Tests.OrphanFirstJigTransactionGuardTests` | 2 / 0 / 0 | 2 / 0 / 0 |
| `RackCad.Tests.PalletToleranceRealProofTests` | 17 / 0 / 0 | 17 / 0 / 0 |
| `RackCad.Tests.PeralteListTests` | 4 / 0 / 0 | 4 / 0 / 0 |
| `RackCad.Tests.PersistedVariableTypeGrammarTests` | 36 / 0 / 0 | 36 / 0 / 0 |
| `RackCad.Tests.PersistenceEmbedInnerDesignTests` | 4 / 0 / 0 | 4 / 0 / 0 |
| `RackCad.Tests.PersistenceInnerPreflightTests` | 9 / 0 / 0 | 9 / 0 / 0 |
| `RackCad.Tests.PersistenceInnerSourceTests` | 13 / 0 / 0 | 13 / 0 / 0 |
| `RackCad.Tests.PersistenceLibraryTransportTests` | 4 / 0 / 0 | 4 / 0 / 0 |
| `RackCad.Tests.PersistenceReopenPreservationTests` | 22 / 0 / 0 | 22 / 0 / 0 |
| `RackCad.Tests.PersistenceUniformityTests` | 15 / 0 / 0 | 15 / 0 / 0 |
| `RackCad.Tests.PersistenceValidationTests` | 5 / 0 / 0 | 5 / 0 / 0 |
| `RackCad.Tests.PersistenceVersioningTests` | 7 / 0 / 0 | 7 / 0 / 0 |
| `RackCad.Tests.PlantaHeaderLayoutBuilderTests` | 6 / 0 / 0 | 6 / 0 / 0 |
| `RackCad.Tests.ProductPolicyIndependenceGuardTests` | 2 / 0 / 0 | 2 / 0 / 0 |
| `RackCad.Tests.ProjectVariableDiscoveryTests` | 39 / 0 / 0 | 39 / 0 / 0 |
| `RackCad.Tests.ProjectVariableMutationExecutorTests` | 28 / 0 / 0 | 28 / 0 / 0 |
| `RackCad.Tests.ProjectVariableMutationPreflightTests` | 46 / 0 / 0 | 46 / 0 / 0 |
| `RackCad.Tests.ProjectVariableScanProjectionTests` | 17 / 0 / 0 | 17 / 0 / 0 |
| `RackCad.Tests.ProjectVariablesConformanceTests` | 10 / 0 / 0 | 10 / 0 / 0 |
| `RackCad.Tests.ProjectVariablesDocumentTests` | 27 / 0 / 0 | 27 / 0 / 0 |
| `RackCad.Tests.ProjectVariablesModelTests` | 34 / 0 / 0 | 34 / 0 / 0 |
| `RackCad.Tests.ProjectVariablesRegistryAccessTests` | 14 / 0 / 0 | 14 / 0 / 0 |
| `RackCad.Tests.ProjectVariablesWorkspaceTests` | 46 / 0 / 0 | 46 / 0 / 0 |
| `RackCad.Tests.ProjectionAvailabilityPolicyTests` | 9 / 0 / 0 | 9 / 0 / 0 |
| `RackCad.Tests.ProjectionCodecPolicyTests` | 6 / 0 / 0 | 6 / 0 / 0 |
| `RackCad.Tests.PushBackA1BStorageTests` | 6 / 0 / 0 | 6 / 0 / 0 |
| `RackCad.Tests.PushBackA1PersistenceTests` | 5 / 0 / 0 | 5 / 0 / 0 |
| `RackCad.Tests.PushBackA1StructureTests` | 4 / 0 / 0 | 4 / 0 / 0 |
| `RackCad.Tests.PushBackAdvancedRackParametersTests` | 30 / 0 / 0 | 30 / 0 / 0 |
| `RackCad.Tests.PushBackBedAnchorTests` | 43 / 0 / 0 | 43 / 0 / 0 |
| `RackCad.Tests.PushBackBedAsymmetryTests` | 38 / 0 / 0 | 38 / 0 / 0 |
| `RackCad.Tests.PushBackBedInOutMateTests` | 41 / 0 / 0 | 41 / 0 / 0 |
| `RackCad.Tests.PushBackBedSlopeTests` | 3 / 0 / 0 | 3 / 0 / 0 |
| `RackCad.Tests.PushBackBedTangencyTests` | 4 / 0 / 0 | 4 / 0 / 0 |
| `RackCad.Tests.PushBackBlankFrontAuthorityTests` | 9 / 0 / 0 | 9 / 0 / 0 |
| `RackCad.Tests.PushBackBlankSideBDeclarationTests` | 9 / 0 / 0 | 9 / 0 / 0 |
| `RackCad.Tests.PushBackBlankSlotIdentityTests` | 11 / 0 / 0 | 11 / 0 / 0 |
| `RackCad.Tests.PushBackBlankStorageCoverageTests` | 12 / 0 / 0 | 12 / 0 / 0 |
| `RackCad.Tests.PushBackBomCommandGuardTests` | 4 / 0 / 0 | 4 / 0 / 0 |
| `RackCad.Tests.PushBackBomTests` | 5 / 0 / 0 | 5 / 0 / 0 |
| `RackCad.Tests.PushBackBootIdentityTests` | 32 / 0 / 0 | 32 / 0 / 0 |
| `RackCad.Tests.PushBackBootLowFaceTests` | 40 / 0 / 0 | 40 / 0 / 0 |
| `RackCad.Tests.PushBackBootSemanticsTests` | 84 / 0 / 0 | 84 / 0 / 0 |
| `RackCad.Tests.PushBackBootSidesTests` | 45 / 0 / 0 | 45 / 0 / 0 |
| `RackCad.Tests.PushBackBootTypeTests` | 28 / 0 / 0 | 28 / 0 / 0 |
| `RackCad.Tests.PushBackCellConfigurationCharacterizationTests` | 7 / 0 / 0 | 7 / 0 / 0 |
| `RackCad.Tests.PushBackCellConfigurationDeliveryTests` | 17 / 0 / 0 | 17 / 0 / 0 |
| `RackCad.Tests.PushBackCellDepthTests` | 37 / 0 / 0 | 37 / 0 / 0 |
| `RackCad.Tests.PushBackCellPalletTests` | 25 / 0 / 0 | 25 / 0 / 0 |
| `RackCad.Tests.PushBackClassicCharacterizationPluginGuardTests` | 10 / 0 / 0 | 10 / 0 / 0 |
| `RackCad.Tests.PushBackCompositeAuditTests` | 19 / 0 / 0 | 19 / 0 / 0 |
| `RackCad.Tests.PushBackCompositeBedTests` | 14 / 0 / 0 | 14 / 0 / 0 |
| `RackCad.Tests.PushBackCompositeBomTests` | 10 / 0 / 0 | 10 / 0 / 0 |
| `RackCad.Tests.PushBackCompositeCharacterizationTests` | 6 / 0 / 0 | 6 / 0 / 0 |
| `RackCad.Tests.PushBackCompositeCorrectionTests` | 37 / 0 / 0 | 37 / 0 / 0 |
| `RackCad.Tests.PushBackCompositeCorridaAuthorityTests` | 19 / 0 / 0 | 19 / 0 / 0 |
| `RackCad.Tests.PushBackCompositeEditingContractTests` | 18 / 0 / 0 | 18 / 0 / 0 |
| `RackCad.Tests.PushBackCompositeEditorTests` | 15 / 0 / 0 | 15 / 0 / 0 |
| `RackCad.Tests.PushBackCompositeFrontalTests` | 8 / 0 / 0 | 8 / 0 / 0 |
| `RackCad.Tests.PushBackCompositeHeightTests` | 7 / 0 / 0 | 7 / 0 / 0 |
| `RackCad.Tests.PushBackCompositeLifecycleTests` | 8 / 0 / 0 | 8 / 0 / 0 |
| `RackCad.Tests.PushBackCompositeModelTests` | 13 / 0 / 0 | 13 / 0 / 0 |
| `RackCad.Tests.PushBackCompositeModuleSequenceTests` | 9 / 0 / 0 | 9 / 0 / 0 |
| `RackCad.Tests.PushBackCompositeMultiFrontTests` | 28 / 0 / 0 | 28 / 0 / 0 |
| `RackCad.Tests.PushBackCompositeRecomputeContainmentTests` | 2 / 0 / 0 | 2 / 0 / 0 |
| `RackCad.Tests.PushBackCompositeRoundTripTests` | 13 / 0 / 0 | 13 / 0 / 0 |
| `RackCad.Tests.PushBackCompositeSafetyOwnershipTests` | 9 / 0 / 0 | 9 / 0 / 0 |
| `RackCad.Tests.PushBackCompositeSafetyTests` | 10 / 0 / 0 | 10 / 0 / 0 |
| `RackCad.Tests.PushBackCompositeStructureTests` | 15 / 0 / 0 | 15 / 0 / 0 |
| `RackCad.Tests.PushBackCompositeSymmetryTests` | 10 / 0 / 0 | 10 / 0 / 0 |
| `RackCad.Tests.PushBackCompositeTopeTests` | 14 / 0 / 0 | 14 / 0 / 0 |
| `RackCad.Tests.PushBackCompositeViewTests` | 20 / 0 / 0 | 20 / 0 / 0 |
| `RackCad.Tests.PushBackCoreTests` | 21 / 0 / 0 | 21 / 0 / 0 |
| `RackCad.Tests.PushBackCorrectionTests` | 4 / 0 / 0 | 4 / 0 / 0 |
| `RackCad.Tests.PushBackCorridaAnchorTests` | 15 / 0 / 0 | 15 / 0 / 0 |
| `RackCad.Tests.PushBackCorridaDualValidityTests` | 11 / 0 / 0 | 11 / 0 / 0 |
| `RackCad.Tests.PushBackCorridaSpanStorageTests` | 14 / 0 / 0 | 14 / 0 / 0 |
| `RackCad.Tests.PushBackCutSupportTests` | 23 / 0 / 0 | 23 / 0 / 0 |
| `RackCad.Tests.PushBackDefensePerPostTests` | 15 / 0 / 0 | 15 / 0 / 0 |
| `RackCad.Tests.PushBackDefensePlanTests` | 14 / 0 / 0 | 14 / 0 / 0 |
| `RackCad.Tests.PushBackDefenseResizeTests` | 5 / 0 / 0 | 5 / 0 / 0 |
| `RackCad.Tests.PushBackDefenseSideIdentityTests` | 15 / 0 / 0 | 15 / 0 / 0 |
| `RackCad.Tests.PushBackDerivedPostAndLineTests` | 21 / 0 / 0 | 21 / 0 / 0 |
| `RackCad.Tests.PushBackDesviadorCellKeyTests` | 7 / 0 / 0 | 7 / 0 / 0 |
| `RackCad.Tests.PushBackDimensionLegacyCharacterizationTests` | 8 / 0 / 0 | 8 / 0 / 0 |
| `RackCad.Tests.PushBackDimensionViewsTests` | 47 / 0 / 0 | 47 / 0 / 0 |
| `RackCad.Tests.PushBackDiverterLowAuthorityTests` | 13 / 0 / 0 | 13 / 0 / 0 |
| `RackCad.Tests.PushBackDiverterPlanTests` | 28 / 0 / 0 | 28 / 0 / 0 |
| `RackCad.Tests.PushBackDormantSideBFirstLevelTests` | 7 / 0 / 0 | 7 / 0 / 0 |
| `RackCad.Tests.PushBackDraftPortSharedReferenceGuardTests` | 1 / 0 / 0 | 1 / 0 / 0 |
| `RackCad.Tests.PushBackDraftStatePortTests` | 3 / 0 / 0 | 3 / 0 / 0 |
| `RackCad.Tests.PushBackDraftWriteGuardTests` | 2 / 0 / 1 | 2 / 0 / 1 |
| `RackCad.Tests.PushBackEditAllFullCopyGuardTests` | 1 / 0 / 0 | 1 / 0 / 0 |
| `RackCad.Tests.PushBackEditAllNameWritersGuardTests` | 1 / 0 / 0 | 1 / 0 / 0 |
| `RackCad.Tests.PushBackEditAllPortEquivalenceGuardTests` | 1 / 0 / 0 | 1 / 0 / 0 |
| `RackCad.Tests.PushBackEditableStateCopyTests` | 7 / 0 / 0 | 7 / 0 / 0 |
| `RackCad.Tests.PushBackEditableStateLoadTests` | 5 / 0 / 0 | 5 / 0 / 0 |
| `RackCad.Tests.PushBackEditableStateOwnershipTests` | 1 / 0 / 0 | 1 / 0 / 0 |
| `RackCad.Tests.PushBackEditorCorrectionTests` | 17 / 0 / 0 | 17 / 0 / 0 |
| `RackCad.Tests.PushBackEditorDefaultsTests` | 5 / 0 / 0 | 5 / 0 / 0 |
| `RackCad.Tests.PushBackEditorOutputGateTests` | 1 / 0 / 0 | 1 / 0 / 0 |
| `RackCad.Tests.PushBackEditorStateTests` | 27 / 0 / 0 | 27 / 0 / 0 |
| `RackCad.Tests.PushBackElevationAuthorityTests` | 19 / 0 / 0 | 19 / 0 / 0 |
| `RackCad.Tests.PushBackElevationConsumerTests` | 14 / 0 / 0 | 14 / 0 / 0 |
| `RackCad.Tests.PushBackEndBeamIdentityTests` | 9 / 0 / 0 | 9 / 0 / 0 |
| `RackCad.Tests.PushBackEndToEndChainTests` | 2 / 0 / 0 | 2 / 0 / 0 |
| `RackCad.Tests.PushBackFirstLevelDatumTransportTests` | 29 / 0 / 0 | 29 / 0 / 0 |
| `RackCad.Tests.PushBackForkliftDefenseTests` | 12 / 0 / 0 | 12 / 0 / 0 |
| `RackCad.Tests.PushBackFrontalHeaderOwnershipTests` | 9 / 0 / 0 | 9 / 0 / 0 |
| `RackCad.Tests.PushBackFrontalMatchesLateralTests` | 16 / 0 / 0 | 16 / 0 / 0 |
| `RackCad.Tests.PushBackFrontalRearCutTests` | 7 / 0 / 0 | 7 / 0 / 0 |
| `RackCad.Tests.PushBackFullDraftClassifierTests` | 2 / 0 / 0 | 2 / 0 / 0 |
| `RackCad.Tests.PushBackFullDraftCompositionTests` | 3 / 0 / 0 | 3 / 0 / 0 |
| `RackCad.Tests.PushBackFullDraftLoadTests` | 4 / 0 / 0 | 4 / 0 / 0 |
| `RackCad.Tests.PushBackFullDraftViewTests` | 1 / 0 / 0 | 1 / 0 / 0 |
| `RackCad.Tests.PushBackFullEditorCharacterizationDimensionStyleLossTests` | 1 / 0 / 0 | 1 / 0 / 0 |
| `RackCad.Tests.PushBackFullEditorCharacterizationLateralCutTests` | 1 / 0 / 0 | 1 / 0 / 0 |
| `RackCad.Tests.PushBackFullEditorCharacterizationRoundingEffectTests` | 1 / 0 / 0 | 1 / 0 / 0 |
| `RackCad.Tests.PushBackFullPanelCensusGuardTests` | 1 / 0 / 0 | 1 / 0 / 0 |
| `RackCad.Tests.PushBackFullPanelGuardTests` | 4 / 0 / 0 | 4 / 0 / 0 |
| `RackCad.Tests.PushBackFullPanelTests` | 11 / 0 / 0 | 11 / 0 / 0 |
| `RackCad.Tests.PushBackFullReadGuardTests` | 1 / 0 / 2 | 1 / 0 / 2 |
| `RackCad.Tests.PushBackFullReadPathTests` | 4 / 0 / 1 | 4 / 0 / 1 |
| `RackCad.Tests.PushBackFullWritePrepareTests` | 9 / 0 / 0 | 9 / 0 / 0 |
| `RackCad.Tests.PushBackFullWriteRunTests` | 4 / 0 / 0 | 4 / 0 / 0 |
| `RackCad.Tests.PushBackGeneralLateralActiveEnvelopeTests` | 4 / 0 / 0 | 4 / 0 / 0 |
| `RackCad.Tests.PushBackGeneralLateralHeaderZoneTests` | 9 / 0 / 0 | 9 / 0 / 0 |
| `RackCad.Tests.PushBackGoldenTests` | 1 / 0 / 0 | 1 / 0 / 0 |
| `RackCad.Tests.PushBackHeaderApplyTests` | 30 / 0 / 0 | 30 / 0 / 0 |
| `RackCad.Tests.PushBackHeaderDepthHeightTests` | 9 / 0 / 0 | 9 / 0 / 0 |
| `RackCad.Tests.PushBackHeaderEnvelopeTests` | 10 / 0 / 0 | 10 / 0 / 0 |
| `RackCad.Tests.PushBackHeaderHeightAuthorityTests` | 32 / 0 / 0 | 32 / 0 / 0 |
| `RackCad.Tests.PushBackHeaderHeightTests` | 3 / 0 / 0 | 3 / 0 / 0 |
| `RackCad.Tests.PushBackHighEndHandTests` | 15 / 0 / 0 | 15 / 0 / 0 |
| `RackCad.Tests.PushBackHighTieBreakTests` | 16 / 0 / 0 | 16 / 0 / 0 |
| `RackCad.Tests.PushBackIntermediateBeamPeralteAuthorityTests` | 6 / 0 / 0 | 6 / 0 / 0 |
| `RackCad.Tests.PushBackIntermediateBeamPeralteFixTests` | 4 / 0 / 0 | 4 / 0 / 0 |
| `RackCad.Tests.PushBackIntermediateBeamPeralteTests` | 12 / 0 / 0 | 12 / 0 / 0 |
| `RackCad.Tests.PushBackLateralGuardDefaultTests` | 11 / 0 / 0 | 11 / 0 / 0 |
| `RackCad.Tests.PushBackLateralProtectorEndsTests` | 7 / 0 / 0 | 7 / 0 / 0 |
| `RackCad.Tests.PushBackLengthCoherenceTests` | 2 / 0 / 0 | 2 / 0 / 0 |
| `RackCad.Tests.PushBackModuleAdoptionTests` | 20 / 0 / 0 | 20 / 0 / 0 |
| `RackCad.Tests.PushBackModuleEditorCharacterizationTests` | 15 / 0 / 0 | 15 / 0 / 0 |
| `RackCad.Tests.PushBackNoParrillaPersistenceTests` | 1 / 0 / 0 | 1 / 0 / 0 |
| `RackCad.Tests.PushBackOutputGateTests` | 8 / 0 / 0 | 8 / 0 / 0 |
| `RackCad.Tests.PushBackOwnerRejectionRound1Tests` | 10 / 0 / 0 | 10 / 0 / 0 |
| `RackCad.Tests.PushBackPalletProjectionTests` | 17 / 0 / 0 | 17 / 0 / 0 |
| `RackCad.Tests.PushBackPanelDraftGuardTests` | 1 / 0 / 0 | 1 / 0 / 0 |
| `RackCad.Tests.PushBackPartialCompositeTests` | 14 / 0 / 0 | 14 / 0 / 0 |
| `RackCad.Tests.PushBackPhysicalBomAndSafetyTests` | 26 / 0 / 0 | 26 / 0 / 0 |
| `RackCad.Tests.PushBackPhysicalHeaderHeightTests` | 19 / 0 / 0 | 19 / 0 / 0 |
| `RackCad.Tests.PushBackPilotUpdateEquivalenceGuardTests` | 6 / 0 / 0 | 6 / 0 / 0 |
| `RackCad.Tests.PushBackPlanTests` | 9 / 0 / 0 | 9 / 0 / 0 |
| `RackCad.Tests.PushBackPlantBedBeamIdentityTests` | 8 / 0 / 0 | 8 / 0 / 0 |
| `RackCad.Tests.PushBackPluginSourceGuardTests` | 11 / 0 / 0 | 11 / 0 / 0 |
| `RackCad.Tests.PushBackPreviewSafetyParityTests` | 9 / 0 / 0 | 9 / 0 / 0 |
| `RackCad.Tests.PushBackRearTopeAnchorTests` | 7 / 0 / 0 | 7 / 0 / 0 |
| `RackCad.Tests.PushBackRearTopeContactPointTests` | 9 / 0 / 0 | 9 / 0 / 0 |
| `RackCad.Tests.PushBackRearTopeNoneAndFarEndAutoTests` | 9 / 0 / 0 | 9 / 0 / 0 |
| `RackCad.Tests.PushBackRearTopePieceTests` | 9 / 0 / 0 | 9 / 0 / 0 |
| `RackCad.Tests.PushBackRearTopeRunAuthorityTests` | 27 / 0 / 0 | 27 / 0 / 0 |
| `RackCad.Tests.PushBackRegistrationTests` | 7 / 0 / 0 | 7 / 0 / 0 |
| `RackCad.Tests.PushBackReinforcedPostBeamTests` | 7 / 0 / 0 | 7 / 0 / 0 |
| `RackCad.Tests.PushBackRoundTripSourceGuardTests` | 24 / 0 / 0 | 24 / 0 / 0 |
| `RackCad.Tests.PushBackRunFrameTests` | 51 / 0 / 0 | 51 / 0 / 0 |
| `RackCad.Tests.PushBackSafetyBomTests` | 5 / 0 / 0 | 5 / 0 / 0 |
| `RackCad.Tests.PushBackSafetyPerPostTests` | 10 / 0 / 0 | 10 / 0 / 0 |
| `RackCad.Tests.PushBackSharedCellGridTests` | 12 / 0 / 0 | 12 / 0 / 0 |
| `RackCad.Tests.PushBackSharedGridCellDemandTests` | 10 / 0 / 0 | 10 / 0 / 0 |
| `RackCad.Tests.PushBackSharedReferenceGuardTests` | 1 / 0 / 1 | 1 / 0 / 1 |
| `RackCad.Tests.PushBackSharedTransverseGridTests` | 10 / 0 / 0 | 10 / 0 / 0 |
| `RackCad.Tests.PushBackSideIndependentHeightTests` | 35 / 0 / 0 | 35 / 0 / 0 |
| `RackCad.Tests.PushBackTarimaPlacementTests` | 10 / 0 / 0 | 10 / 0 / 0 |
| `RackCad.Tests.PushBackTopeAnchorPerViewTests` | 7 / 0 / 0 | 7 / 0 / 0 |
| `RackCad.Tests.PushBackTopeOriginMateTests` | 7 / 0 / 0 | 7 / 0 / 0 |
| `RackCad.Tests.PushBackTopePlantaAndDiverterTests` | 18 / 0 / 0 | 18 / 0 / 0 |
| `RackCad.Tests.PushBackTopeSurfaceTests` | 14 / 0 / 0 | 14 / 0 / 0 |
| `RackCad.Tests.PushBackVerticalAuthorityTests` | 19 / 0 / 0 | 19 / 0 / 0 |
| `RackCad.Tests.PushBackViewEraseGuardTests` | 4 / 0 / 0 | 4 / 0 / 0 |
| `RackCad.Tests.PushBackViewHygieneTests` | 30 / 0 / 0 | 30 / 0 / 0 |
| `RackCad.Tests.PushBackViewTests` | 6 / 0 / 0 | 6 / 0 / 0 |
| `RackCad.Tests.PushBackWholeLateralEnvelopeTests` | 9 / 0 / 0 | 9 / 0 / 0 |
| `RackCad.Tests.PushBackWindowOnStateSourceGuardTests` | 1 / 0 / 0 | 1 / 0 / 0 |
| `RackCad.Tests.PushBackWindowSharedReferenceGuardTests` | 1 / 0 / 0 | 1 / 0 / 0 |
| `RackCad.Tests.PushBackWorkspaceModeSourceGuardTests` | 1 / 0 / 0 | 1 / 0 / 0 |
| `RackCad.Tests.PushBackWorkspaceSharedReferenceGuardTests` | 1 / 0 / 0 | 1 / 0 / 0 |
| `RackCad.Tests.PushBackWritePathGuardTests` | 1 / 0 / 1 | 1 / 0 / 1 |
| `RackCad.Tests.PushBackWritePathPluginGuardTests` | 1 / 0 / 0 | 1 / 0 / 0 |
| `RackCad.Tests.RackCountInvariantCharacterizationTests` | 3 / 0 / 0 | 3 / 0 / 0 |
| `RackCad.Tests.RackDefinitionCreatorGuardTests` | 26 / 0 / 0 | 26 / 0 / 0 |
| `RackCad.Tests.RackDesignLibraryTests` | 9 / 0 / 0 | 9 / 0 / 0 |
| `RackCad.Tests.RackDuplicationPlanTests` | 31 / 0 / 0 | 31 / 0 / 0 |
| `RackCad.Tests.RackEditPreflightCharacterizationTests` | 5 / 0 / 0 | 5 / 0 / 0 |
| `RackCad.Tests.RackEmbedDocumentTests` | 7 / 0 / 0 | 7 / 0 / 0 |
| `RackCad.Tests.RackEnvelopeIdProbeTests` | 5 / 0 / 0 | 5 / 0 / 0 |
| `RackCad.Tests.RackFirstLevelDatumTests` | 26 / 0 / 0 | 26 / 0 / 0 |
| `RackCad.Tests.RackFrameConfigurationDeepCopyTests` | 11 / 0 / 0 | 11 / 0 / 0 |
| `RackCad.Tests.RackFrameConfigurationFactoryTests` | 11 / 0 / 0 | 11 / 0 / 0 |
| `RackCad.Tests.RackFrameProjectStoreTests` | 11 / 0 / 0 | 11 / 0 / 0 |
| `RackCad.Tests.RackFrameTemplateProviderTests` | 5 / 0 / 0 | 5 / 0 / 0 |
| `RackCad.Tests.RackGroupFrameTests` | 7 / 0 / 0 | 7 / 0 / 0 |
| `RackCad.Tests.RackGroupPlacementPlanTests` | 24 / 0 / 0 | 24 / 0 / 0 |
| `RackCad.Tests.RackIdLifecycleTests` | 4 / 0 / 0 | 4 / 0 / 0 |
| `RackCad.Tests.RackLevelElevationsTests` | 15 / 0 / 0 | 15 / 0 / 0 |
| `RackCad.Tests.RackListBuilderTests` | 10 / 0 / 0 | 10 / 0 / 0 |
| `RackCad.Tests.RackLogTests` | 8 / 0 / 0 | 8 / 0 / 0 |
| `RackCad.Tests.RackModuleEditSessionTests` | 25 / 0 / 0 | 25 / 0 / 0 |
| `RackCad.Tests.RackOrthographicPlacementPolicyTests` | 15 / 0 / 0 | 15 / 0 / 0 |
| `RackCad.Tests.RackPlacedGeometryInvariantTests` | 6 / 0 / 0 | 6 / 0 / 0 |
| `RackCad.Tests.RackProductAuthoredGateTests` | 5 / 0 / 0 | 5 / 0 / 0 |
| `RackCad.Tests.RackProductPrepareTests` | 5 / 0 / 0 | 5 / 0 / 0 |
| `RackCad.Tests.RackProjectStorePerKindTests` | 3 / 0 / 0 | 3 / 0 / 0 |
| `RackCad.Tests.RackProjectStoreTests` | 18 / 0 / 0 | 18 / 0 / 0 |
| `RackCad.Tests.RackProjectionCommandGuardTests` | 34 / 0 / 0 | 34 / 0 / 0 |
| `RackCad.Tests.RackProjectionCommandRunTests` | 49 / 0 / 0 | 49 / 0 / 0 |
| `RackCad.Tests.RackProjectionMaterializationRunTests` | 9 / 0 / 0 | 9 / 0 / 0 |
| `RackCad.Tests.RackProjectionRemedyTests` | 10 / 0 / 0 | 10 / 0 / 0 |
| `RackCad.Tests.RackRigidPlacementPolicyTests` | 11 / 0 / 0 | 11 / 0 / 0 |
| `RackCad.Tests.RackSiblingAuthoredGateTests` | 4 / 0 / 0 | 4 / 0 / 0 |
| `RackCad.Tests.RackSiblingCustomPropertiesGateTests` | 2 / 0 / 0 | 2 / 0 / 0 |
| `RackCad.Tests.RackSiblingGateIndependenceGuardTests` | 3 / 0 / 0 | 3 / 0 / 0 |
| `RackCad.Tests.RackSiblingInsertRunTests` | 11 / 0 / 0 | 11 / 0 / 0 |
| `RackCad.Tests.RackSiblingMembershipTests` | 12 / 0 / 0 | 12 / 0 / 0 |
| `RackCad.Tests.RackSiblingRedrawPlanTests` | 2 / 0 / 0 | 2 / 0 / 0 |
| `RackCad.Tests.RackSiblingRedrawRunTests` | 6 / 0 / 0 | 6 / 0 / 0 |
| `RackCad.Tests.RackSiblingRedrawUnitsTests` | 2 / 0 / 0 | 2 / 0 / 0 |
| `RackCad.Tests.RackSingleViewPlacementTests` | 18 / 0 / 0 | 18 / 0 / 0 |
| `RackCad.Tests.RackTransformFactsTests` | 9 / 0 / 0 | 9 / 0 / 0 |
| `RackCad.Tests.RackUnitsGuardSourceTests` | 18 / 0 / 0 | 18 / 0 / 0 |
| `RackCad.Tests.RackViewAuthoredEquivalenceTests` | 2 / 0 / 0 | 2 / 0 / 0 |
| `RackCad.Tests.RackViewBatchDriverGuardTests` | 4 / 0 / 0 | 4 / 0 / 0 |
| `RackCad.Tests.RackViewBatchPlanTests` | 16 / 0 / 0 | 16 / 0 / 0 |
| `RackCad.Tests.RackViewCountInvariantTests` | 3 / 0 / 0 | 3 / 0 / 0 |
| `RackCad.Tests.RackViewExposureTests` | 7 / 0 / 0 | 7 / 0 / 0 |
| `RackCad.Tests.RackViewFrameTests` | 10 / 0 / 0 | 10 / 0 / 0 |
| `RackCad.Tests.RackViewPartialRackContractTests` | 2 / 0 / 0 | 2 / 0 / 0 |
| `RackCad.Tests.RackViewPlacementGuardTests` | 4 / 0 / 0 | 4 / 0 / 0 |
| `RackCad.Tests.RackViewPreparationSystemTests` | 6 / 0 / 0 | 6 / 0 / 0 |
| `RackCad.Tests.RedefinedViewReferenceUndoTouchGuardTests` | 4 / 0 / 0 | 4 / 0 / 0 |
| `RackCad.Tests.RegistryCommitAccreditationTests` | 12 / 0 / 0 | 12 / 0 / 0 |
| `RackCad.Tests.SafetySelectionDocumentsTests` | 10 / 0 / 0 | 10 / 0 / 0 |
| `RackCad.Tests.SeccionesCatalogTests` | 3 / 0 / 0 | 3 / 0 / 0 |
| `RackCad.Tests.SelectiveAuthoredBindingPresenceTests` | 15 / 0 / 0 | 15 / 0 / 0 |
| `RackCad.Tests.SelectiveAuthoredBindingTests` | 21 / 0 / 0 | 21 / 0 / 0 |
| `RackCad.Tests.SelectiveAuthoredCarrierAdoptionTests` | 12 / 0 / 0 | 12 / 0 / 0 |
| `RackCad.Tests.SelectiveBindingIntentTests` | 22 / 0 / 0 | 22 / 0 / 0 |
| `RackCad.Tests.SelectiveBomAuthorityTests` | 31 / 0 / 0 | 31 / 0 / 0 |
| `RackCad.Tests.SelectiveBomBuilderTests` | 7 / 0 / 0 | 7 / 0 / 0 |
| `RackCad.Tests.SelectiveCabeceraHeightReviewTests` | 9 / 0 / 0 | 9 / 0 / 0 |
| `RackCad.Tests.SelectiveCabeceraViewPurityTests` | 10 / 0 / 0 | 10 / 0 / 0 |
| `RackCad.Tests.SelectiveDepthTests` | 11 / 0 / 0 | 11 / 0 / 0 |
| `RackCad.Tests.SelectiveDesviadorTests` | 30 / 0 / 0 | 30 / 0 / 0 |
| `RackCad.Tests.SelectiveDimensionLegacyCharacterizationTests` | 6 / 0 / 0 | 6 / 0 / 0 |
| `RackCad.Tests.SelectiveDimensionViewsAuthorityTests` | 12 / 0 / 0 | 12 / 0 / 0 |
| `RackCad.Tests.SelectiveDimensionViewsDocumentTests` | 10 / 0 / 0 | 10 / 0 / 0 |
| `RackCad.Tests.SelectiveDimensionViewsEndToEndTests` | 18 / 0 / 0 | 18 / 0 / 0 |
| `RackCad.Tests.SelectiveDimensionViewsTests` | 139 / 0 / 0 | 139 / 0 / 0 |
| `RackCad.Tests.SelectiveDimensionsTests` | 13 / 0 / 0 | 13 / 0 / 0 |
| `RackCad.Tests.SelectiveDuplicationFailClosedTests` | 24 / 0 / 0 | 24 / 0 / 0 |
| `RackCad.Tests.SelectiveEditorOpenTests` | 33 / 0 / 0 | 33 / 0 / 0 |
| `RackCad.Tests.SelectiveEditorStateTests` | 29 / 0 / 0 | 29 / 0 / 0 |
| `RackCad.Tests.SelectiveEffectiveDesignResolverTests` | 20 / 0 / 0 | 20 / 0 / 0 |
| `RackCad.Tests.SelectiveFloorBeamRiseMaterializationTests` | 9 / 0 / 0 | 9 / 0 / 0 |
| `RackCad.Tests.SelectiveFloorBeamRiseTests` | 24 / 0 / 0 | 24 / 0 / 0 |
| `RackCad.Tests.SelectiveFondoCabeceraTests` | 33 / 0 / 0 | 33 / 0 / 0 |
| `RackCad.Tests.SelectiveFrontalBuilderTests` | 25 / 0 / 0 | 25 / 0 / 0 |
| `RackCad.Tests.SelectiveGate8ATests` | 21 / 0 / 0 | 21 / 0 / 0 |
| `RackCad.Tests.SelectiveGeometryResolverTests` | 22 / 0 / 0 | 22 / 0 / 0 |
| `RackCad.Tests.SelectiveHeaderBatchMutateTests` | 17 / 0 / 0 | 17 / 0 / 0 |
| `RackCad.Tests.SelectiveHeaderBatchNormalizationTests` | 8 / 0 / 0 | 8 / 0 / 0 |
| `RackCad.Tests.SelectiveHeaderBatchPersistenceTests` | 4 / 0 / 0 | 4 / 0 / 0 |
| `RackCad.Tests.SelectiveHeaderBatchPrepareTests` | 15 / 0 / 0 | 15 / 0 / 0 |
| `RackCad.Tests.SelectiveLateralBuilderTests` | 13 / 0 / 0 | 13 / 0 / 0 |
| `RackCad.Tests.SelectiveLibraryExportTests` | 21 / 0 / 0 | 21 / 0 / 0 |
| `RackCad.Tests.SelectiveMedioFrenteTests` | 14 / 0 / 0 | 14 / 0 / 0 |
| `RackCad.Tests.SelectiveMultiSelectionTests` | 43 / 0 / 0 | 43 / 0 / 0 |
| `RackCad.Tests.SelectivePalletDesignDocumentTests` | 13 / 0 / 0 | 13 / 0 / 0 |
| `RackCad.Tests.SelectiveParrillaPlacementTests` | 3 / 0 / 0 | 3 / 0 / 0 |
| `RackCad.Tests.SelectivePerFondoTests` | 30 / 0 / 0 | 30 / 0 / 0 |
| `RackCad.Tests.SelectivePlantaPlanTests` | 6 / 0 / 0 | 6 / 0 / 0 |
| `RackCad.Tests.SelectivePostGeometryTests` | 3 / 0 / 0 | 3 / 0 / 0 |
| `RackCad.Tests.SelectivePostTargetsTests` | 8 / 0 / 0 | 8 / 0 / 0 |
| `RackCad.Tests.SelectiveSafetyConfigTests` | 12 / 0 / 0 | 12 / 0 / 0 |
| `RackCad.Tests.SelectiveSafetyEquivalenceTests` | 7 / 0 / 0 | 7 / 0 / 0 |
| `RackCad.Tests.SelectiveSafetyGridTests` | 5 / 0 / 0 | 5 / 0 / 0 |
| `RackCad.Tests.SelectiveSafetyTests` | 49 / 0 / 0 | 49 / 0 / 0 |
| `RackCad.Tests.SelectiveSeparadorPlacementTests` | 1 / 0 / 0 | 1 / 0 / 0 |
| `RackCad.Tests.SelectiveTargetFondosIntegrationTests` | 13 / 0 / 0 | 13 / 0 / 0 |
| `RackCad.Tests.SelectiveTargetPreferenceTests` | 17 / 0 / 0 | 17 / 0 / 0 |
| `RackCad.Tests.SelectiveTargetResolverTests` | 34 / 0 / 0 | 34 / 0 / 0 |
| `RackCad.Tests.SelectiveTarimaPlacementTests` | 3 / 0 / 0 | 3 / 0 / 0 |
| `RackCad.Tests.SelectiveTopeFamilyIsolationGuardTests` | 3 / 0 / 0 | 3 / 0 / 0 |
| `RackCad.Tests.SelectiveTopeMultiFondoCharacterizationTests` | 46 / 0 / 0 | 46 / 0 / 0 |
| `RackCad.Tests.SelectiveTopePlacementTests` | 4 / 0 / 0 | 4 / 0 / 0 |
| `RackCad.Tests.SelectiveTopePlanFrontalTests` | 5 / 0 / 0 | 5 / 0 / 0 |
| `RackCad.Tests.SelectiveTopeSideMultiFondoContractTests` | 32 / 0 / 0 | 32 / 0 / 0 |
| `RackCad.Tests.SelectiveTopeSidePersistenceTests` | 19 / 0 / 0 | 19 / 0 / 0 |
| `RackCad.Tests.SelectiveTopeSideTests` | 20 / 0 / 0 | 20 / 0 / 0 |
| `RackCad.Tests.SelectiveTwentyBaysEquivalenceTests` | 4 / 0 / 0 | 4 / 0 / 0 |
| `RackCad.Tests.SeparatorLevelCalculatorTests` | 14 / 0 / 0 | 14 / 0 / 0 |
| `RackCad.Tests.SharedPhysicalFactsTests` | 5 / 0 / 0 | 5 / 0 / 0 |
| `RackCad.Tests.SharedViewCodecTests` | 4 / 0 / 0 | 4 / 0 / 0 |
| `RackCad.Tests.SharedViewFoundationCt04V2CharacterizationTests` | 7 / 0 / 0 | 7 / 0 / 0 |
| `RackCad.Tests.SharedViewFoundationF1CharacterizationTests` | 26 / 0 / 0 | 26 / 0 / 0 |
| `RackCad.Tests.SharedViewFoundationF5Tests` | 20 / 0 / 0 | 20 / 0 / 0 |
| `RackCad.Tests.SharedViewFoundationF6Tests` | 18 / 0 / 0 | 18 / 0 / 0 |
| `RackCad.Tests.ShippedCatalogIntegrityTests` | 5 / 0 / 0 | 5 / 0 / 0 |
| `RackCad.Tests.ShippedStructuralSectionCatalogTests` | 29 / 0 / 0 | 29 / 0 / 0 |
| `RackCad.Tests.SiblingRedrawSeamGuardTests` | 2 / 0 / 0 | 2 / 0 / 0 |
| `RackCad.Tests.SingleScanInsertGuardTests` | 5 / 0 / 0 | 5 / 0 / 0 |
| `RackCad.Tests.StructuralAnalysisCanonicalizerTests` | 12 / 0 / 0 | 12 / 0 / 0 |
| `RackCad.Tests.StructuralAnalysisCantileverBoundaryTests` | 30 / 0 / 0 | 30 / 0 / 0 |
| `RackCad.Tests.StructuralAnalysisCantileverGoldenTests` | 14 / 0 / 0 | 14 / 0 / 0 |
| `RackCad.Tests.StructuralAnalysisCantileverIdealisationTests` | 36 / 0 / 0 | 36 / 0 / 0 |
| `RackCad.Tests.StructuralAnalysisComparisonHardeningDumpReaderTests` | 14 / 0 / 0 | 14 / 0 / 0 |
| `RackCad.Tests.StructuralAnalysisComparisonHardeningIncompleteTests` | 24 / 0 / 0 | 24 / 0 / 0 |
| `RackCad.Tests.StructuralAnalysisComparisonHardeningMetricTests` | 31 / 0 / 0 | 31 / 0 / 0 |
| `RackCad.Tests.StructuralAnalysisComparisonHardeningReadbackTests` | 68 / 0 / 0 | 68 / 0 / 0 |
| `RackCad.Tests.StructuralAnalysisComparisonHardeningRehearsalTests` | 18 / 0 / 0 | 18 / 0 / 0 |
| `RackCad.Tests.StructuralAnalysisComparisonHardeningToolTests` | 36 / 0 / 0 | 36 / 0 / 0 |
| `RackCad.Tests.StructuralAnalysisComparisonRowTests` | 23 / 0 / 0 | 23 / 0 / 0 |
| `RackCad.Tests.StructuralAnalysisComparisonToolTests` | 28 / 0 / 0 | 28 / 0 / 0 |
| `RackCad.Tests.StructuralAnalysisComparisonVerdictTests` | 30 / 0 / 0 | 30 / 0 / 0 |
| `RackCad.Tests.StructuralAnalysisFingerprintTests` | 10 / 0 / 0 | 10 / 0 / 0 |
| `RackCad.Tests.StructuralAnalysisLoadFileReaderTests` | 51 / 0 / 0 | 51 / 0 / 0 |
| `RackCad.Tests.StructuralAnalysisMaterialProfileTests` | 11 / 0 / 0 | 11 / 0 / 0 |
| `RackCad.Tests.StructuralAnalysisModelBuilderTests` | 23 / 0 / 0 | 23 / 0 / 0 |
| `RackCad.Tests.StructuralAnalysisModelTests` | 10 / 0 / 0 | 10 / 0 / 0 |
| `RackCad.Tests.StructuralAnalysisOutcomeTests` | 12 / 0 / 0 | 12 / 0 / 0 |
| `RackCad.Tests.StructuralAnalysisPluginCommandGuardTests` | 112 / 0 / 0 | 112 / 0 / 0 |
| `RackCad.Tests.StructuralAnalysisPluginCommandProductTableTests` | 4 / 0 / 0 | 4 / 0 / 0 |
| `RackCad.Tests.StructuralAnalysisRamElementsAssumptionsTests` | 5 / 0 / 0 | 5 / 0 / 0 |
| `RackCad.Tests.StructuralAnalysisRamElementsAxesTests` | 96 / 0 / 0 | 96 / 0 / 0 |
| `RackCad.Tests.StructuralAnalysisRamElementsDumpPayloadTests` | 26 / 0 / 0 | 26 / 0 / 0 |
| `RackCad.Tests.StructuralAnalysisRamElementsDumpReaderTests` | 106 / 0 / 0 | 106 / 0 / 0 |
| `RackCad.Tests.StructuralAnalysisRamElementsDumpRoundTripTests` | 6 / 0 / 0 | 6 / 0 / 0 |
| `RackCad.Tests.StructuralAnalysisRamElementsDumpSourceTests` | 23 / 0 / 0 | 23 / 0 / 0 |
| `RackCad.Tests.StructuralAnalysisRamElementsDumpTemplateTests` | 60 / 0 / 0 | 60 / 0 / 0 |
| `RackCad.Tests.StructuralAnalysisRamElementsDumpTransferReaderTests` | 73 / 0 / 0 | 73 / 0 / 0 |
| `RackCad.Tests.StructuralAnalysisRamElementsGuardTests` | 24 / 0 / 0 | 24 / 0 / 0 |
| `RackCad.Tests.StructuralAnalysisRamElementsHardeningGateTests` | 46 / 0 / 0 | 46 / 0 / 0 |
| `RackCad.Tests.StructuralAnalysisRamElementsHardeningGuardTests` | 28 / 0 / 0 | 28 / 0 / 0 |
| `RackCad.Tests.StructuralAnalysisRamElementsHardeningPackageTests` | 45 / 0 / 0 | 45 / 0 / 0 |
| `RackCad.Tests.StructuralAnalysisRamElementsHardeningRaceTests` | 6 / 0 / 0 | 6 / 0 / 0 |
| `RackCad.Tests.StructuralAnalysisRamElementsHardeningReadBackTests` | 29 / 0 / 0 | 29 / 0 / 0 |
| `RackCad.Tests.StructuralAnalysisRamElementsMapReaderTests` | 111 / 0 / 0 | 111 / 0 / 0 |
| `RackCad.Tests.StructuralAnalysisRamElementsPackageContentTests` | 14 / 0 / 0 | 14 / 0 / 0 |
| `RackCad.Tests.StructuralAnalysisRamElementsPackageGoldenTests` | 5 / 0 / 0 | 5 / 0 / 0 |
| `RackCad.Tests.StructuralAnalysisRamElementsPackageManifestTests` | 31 / 0 / 0 | 31 / 0 / 0 |
| `RackCad.Tests.StructuralAnalysisRamElementsPackagePayloadTests` | 26 / 0 / 0 | 26 / 0 / 0 |
| `RackCad.Tests.StructuralAnalysisRamElementsPackageTemplateTests` | 51 / 0 / 0 | 51 / 0 / 0 |
| `RackCad.Tests.StructuralAnalysisRamElementsPackageValidationTests` | 63 / 0 / 0 | 63 / 0 / 0 |
| `RackCad.Tests.StructuralAnalysisRamElementsPackageWriterTests` | 51 / 0 / 0 | 51 / 0 / 0 |
| `RackCad.Tests.StructuralAnalysisRamElementsProjectionTests` | 28 / 0 / 0 | 28 / 0 / 0 |
| `RackCad.Tests.StructuralAnalysisRamElementsTranslationTests` | 33 / 0 / 0 | 33 / 0 / 0 |
| `RackCad.Tests.StructuralAnalysisSectionMapVerifiedTests` | 6 / 0 / 0 | 6 / 0 / 0 |
| `RackCad.Tests.StructuralAnalysisSourceEntryHardeningTests` | 37 / 0 / 0 | 37 / 0 / 0 |
| `RackCad.Tests.StructuralAnalysisSourceEntryTests` | 29 / 0 / 0 | 29 / 0 / 0 |
| `RackCad.Tests.StructuralAnalysisSourceGuardTests` | 86 / 0 / 0 | 86 / 0 / 0 |
| `RackCad.Tests.StructuralAnalysisUnitTests` | 56 / 0 / 0 | 56 / 0 / 0 |
| `RackCad.Tests.StructuralSectionGeometrySentinelTests` | 12 / 0 / 0 | 12 / 0 / 0 |
| `RackCad.Tests.StructuralSectionGeometryTests` | 18 / 0 / 0 | 18 / 0 / 0 |
| `RackCad.Tests.StructuralSectionIdNamespaceTests` | 21 / 0 / 0 | 21 / 0 / 0 |
| `RackCad.Tests.StructuralSectionImporterTests` | 20 / 0 / 0 | 20 / 0 / 0 |
| `RackCad.Tests.StructuralSectionLoadValidationTests` | 15 / 0 / 0 | 15 / 0 / 0 |
| `RackCad.Tests.StructuralSectionModelTests` | 37 / 0 / 0 | 37 / 0 / 0 |
| `RackCad.Tests.StructuralSectionPluginSourceGuardTests` | 45 / 0 / 0 | 45 / 0 / 0 |
| `RackCad.Tests.StructuralSectionProjectionTests` | 38 / 0 / 0 | 38 / 0 / 0 |
| `RackCad.Tests.StructuralSectionPublishTransactionTests` | 9 / 0 / 0 | 9 / 0 / 0 |
| `RackCad.Tests.StructuralSectionSFamilyTests` | 21 / 0 / 0 | 21 / 0 / 0 |
| `RackCad.Tests.StructuralSectionSearchTests` | 16 / 0 / 0 | 16 / 0 / 0 |
| `RackCad.Tests.StructuralSectionStrictReaderTests` | 25 / 0 / 0 | 25 / 0 / 0 |
| `RackCad.Tests.StructuralSectionValidatorTests` | 55 / 0 / 0 | 55 / 0 / 0 |
| `RackCad.Tests.StructuralSectionWireframeIntegrityTests` | 47 / 0 / 0 | 47 / 0 / 0 |
| `RackCad.Tests.SystemBomBuilderTests` | 16 / 0 / 0 | 16 / 0 / 0 |
| `RackCad.Tests.SystemKindPersistenceCharacterizationTests` | 31 / 0 / 0 | 31 / 0 / 0 |
| `RackCad.Tests.SystemRegistryTests` | 13 / 0 / 0 | 13 / 0 / 0 |
| `RackCad.Tests.UserSettingsStoreTests` | 4 / 0 / 0 | 4 / 0 / 0 |
| `RackCad.Tests.UserTemplateStoreTests` | 4 / 0 / 0 | 4 / 0 / 0 |
| `RackCad.Tests.UserTemplatesTests` | 5 / 0 / 0 | 5 / 0 / 0 |
| `RackCad.Tests.WarehouseAutoFillTests` | 13 / 0 / 0 | 13 / 0 / 0 |
| `RackCad.Tests.WarehouseFitCheckerTests` | 10 / 0 / 0 | 10 / 0 / 0 |
| `RackCad.Tests.WarehouseGridPlannerTests` | 19 / 0 / 0 | 19 / 0 / 0 |
| `RackCad.Tests.WorkspaceAssociativeBookkeepingTests` | 0 / 0 / 10 | 0 / 0 / 10 |
| `RackCad.Tests.WorkspaceBridgeAuthorizeTests` | 7 / 0 / 0 | 7 / 0 / 0 |
| `RackCad.Tests.WorkspaceBridgeCommandStateTests` | 20 / 0 / 0 | 20 / 0 / 0 |
| `RackCad.Tests.WorkspaceBridgeDegradedModeTests` | 11 / 0 / 0 | 11 / 0 / 0 |
| `RackCad.Tests.WorkspaceBridgeFacadeSourceGuardTests` | 18 / 0 / 0 | 18 / 0 / 0 |
| `RackCad.Tests.WorkspaceBridgeHintAndDrainTests` | 28 / 0 / 0 | 28 / 0 / 0 |
| `RackCad.Tests.WorkspaceBridgeNoActiveDocumentTests` | 11 / 0 / 0 | 11 / 0 / 0 |
| `RackCad.Tests.WorkspaceBridgeObjectSubjectGuardTests` | 20 / 0 / 0 | 20 / 0 / 0 |
| `RackCad.Tests.WorkspaceBridgeReconciliationTests` | 17 / 0 / 0 | 17 / 0 / 0 |
| `RackCad.Tests.WorkspaceBridgeSourceGuardTests` | 52 / 0 / 0 | 52 / 0 / 0 |
| `RackCad.Tests.WorkspaceBridgeSubscriptionLifecycleTests` | 17 / 0 / 0 | 17 / 0 / 0 |
| `RackCad.Tests.WorkspaceBrowserBoundaryTests` | 26 / 0 / 0 | 26 / 0 / 0 |
| `RackCad.Tests.WorkspaceBrowserHostGuardTests` | 19 / 0 / 0 | 19 / 0 / 0 |
| `RackCad.Tests.WorkspaceBrowserSessionRegistryTests` | 12 / 0 / 0 | 12 / 0 / 0 |
| `RackCad.Tests.WorkspaceBrowserSessionTests` | 44 / 0 / 0 | 44 / 0 / 0 |
| `RackCad.Tests.WorkspaceBrowserSummaryTests` | 31 / 0 / 0 | 31 / 0 / 0 |
| `RackCad.Tests.WorkspaceCommandAdvisorGuardTests` | 82 / 0 / 0 | 82 / 0 / 0 |
| `RackCad.Tests.WorkspaceCommandAdvisorTests` | 0 / 0 / 54 | 0 / 0 / 54 |
| `RackCad.Tests.WorkspaceDraftApplyTests` | 61 / 0 / 0 | 61 / 0 / 0 |
| `RackCad.Tests.WorkspaceDraftBoundaryTests` | 152 / 0 / 0 | 152 / 0 / 0 |
| `RackCad.Tests.WorkspaceDraftPilotIntentGuardTests` | 17 / 0 / 0 | 17 / 0 / 0 |
| `RackCad.Tests.WorkspaceDraftPilotIntentReadTests` | 82 / 0 / 0 | 82 / 0 / 0 |
| `RackCad.Tests.WorkspaceDraftPilotIntentStaleBaseTests` | 29 / 0 / 0 | 29 / 0 / 0 |
| `RackCad.Tests.WorkspaceDraftPilotMaterializationTests` | 78 / 0 / 0 | 78 / 0 / 0 |
| `RackCad.Tests.WorkspaceDraftPilotOwnershipTests` | 31 / 0 / 0 | 31 / 0 / 0 |
| `RackCad.Tests.WorkspaceDraftPilotStateTests` | 65 / 0 / 0 | 65 / 0 / 0 |
| `RackCad.Tests.WorkspaceDraftRegistryContainmentTests` | 23 / 0 / 0 | 23 / 0 / 0 |
| `RackCad.Tests.WorkspaceDraftRegistryTests` | 36 / 0 / 0 | 36 / 0 / 0 |
| `RackCad.Tests.WorkspaceDraftSessionTests` | 99 / 0 / 0 | 99 / 0 / 0 |
| `RackCad.Tests.WorkspaceDraftStaleBaseTests` | 50 / 0 / 0 | 50 / 0 / 0 |
| `RackCad.Tests.WorkspaceEditIntentGuardTests` | 145 / 0 / 7 | 145 / 0 / 7 |
| `RackCad.Tests.WorkspaceEditIntentHostTests` | 0 / 0 / 62 | 0 / 0 / 62 |
| `RackCad.Tests.WorkspaceFullCopyGuardTests` | 0 / 0 / 1 | 0 / 0 / 1 |
| `RackCad.Tests.WorkspaceHintDrainTests` | 11 / 0 / 0 | 11 / 0 / 0 |
| `RackCad.Tests.WorkspaceHostAuthorizationTests` | 13 / 0 / 0 | 13 / 0 / 0 |
| `RackCad.Tests.WorkspaceHostC2SourceGuardTests` | 15 / 0 / 0 | 15 / 0 / 0 |
| `RackCad.Tests.WorkspaceHostDrainTests` | 42 / 0 / 0 | 42 / 0 / 0 |
| `RackCad.Tests.WorkspaceHostHardeningGuardTests` | 99 / 0 / 1 | 99 / 0 / 1 |
| `RackCad.Tests.WorkspaceHostLifecycleTests` | 35 / 0 / 0 | 35 / 0 / 0 |
| `RackCad.Tests.WorkspaceHostPolicyTests` | 37 / 0 / 0 | 37 / 0 / 0 |
| `RackCad.Tests.WorkspaceHostPortGuardTests` | 33 / 0 / 0 | 33 / 0 / 0 |
| `RackCad.Tests.WorkspaceHostSourceGuardTests` | 45 / 0 / 0 | 45 / 0 / 0 |
| `RackCad.Tests.WorkspaceIdleSchedulerTests` | 48 / 0 / 0 | 48 / 0 / 0 |
| `RackCad.Tests.WorkspaceIdleSchedulingGuardTests` | 124 / 0 / 0 | 124 / 0 / 0 |
| `RackCad.Tests.WorkspaceInstrumentationTests` | 16 / 0 / 0 | 16 / 0 / 0 |
| `RackCad.Tests.WorkspaceLifecycleGuardTests` | 57 / 0 / 2 | 57 / 0 / 2 |
| `RackCad.Tests.WorkspaceLifecycleHostTests` | 0 / 0 / 69 | 0 / 0 / 69 |
| `RackCad.Tests.WorkspaceModelBoundaryTests` | 18 / 1 / 0 | 18 / 1 / 0 |
| `RackCad.Tests.WorkspaceNavigationActionPolicyTests` | 20 / 0 / 0 | 20 / 0 / 0 |
| `RackCad.Tests.WorkspaceNavigationAuthorityTests` | 10 / 0 / 0 | 10 / 0 / 0 |
| `RackCad.Tests.WorkspaceNavigationCacheTests` | 15 / 0 / 0 | 15 / 0 / 0 |
| `RackCad.Tests.WorkspaceNavigationEchoTests` | 18 / 0 / 0 | 18 / 0 / 0 |
| `RackCad.Tests.WorkspaceNavigationHostTests` | 67 / 0 / 0 | 67 / 0 / 0 |
| `RackCad.Tests.WorkspaceNavigationPrivacyGuardTests` | 11 / 0 / 0 | 11 / 0 / 0 |
| `RackCad.Tests.WorkspaceNavigationTargetsTests` | 23 / 0 / 0 | 23 / 0 / 0 |
| `RackCad.Tests.WorkspaceOwnMutationTests` | 18 / 0 / 0 | 18 / 0 / 0 |
| `RackCad.Tests.WorkspacePaletteHostSourceGuardTests` | 45 / 0 / 0 | 45 / 0 / 0 |
| `RackCad.Tests.WorkspacePilotCloseSyncGuardTests` | 119 / 0 / 0 | 119 / 0 / 0 |
| `RackCad.Tests.WorkspacePilotEditGuardTests` | 121 / 0 / 6 | 121 / 0 / 6 |
| `RackCad.Tests.WorkspacePilotEditHostTests` | 0 / 0 / 59 | 0 / 0 / 59 |
| `RackCad.Tests.WorkspacePilotPrepareGuardTests` | 22 / 0 / 3 | 22 / 0 / 3 |
| `RackCad.Tests.WorkspacePilotPrepareTests` | 0 / 0 / 67 | 0 / 0 / 67 |
| `RackCad.Tests.WorkspacePilotReconciliationGuardTests` | 25 / 0 / 1 | 25 / 0 / 1 |
| `RackCad.Tests.WorkspacePilotReconciliationHostTests` | 0 / 0 / 28 | 0 / 0 / 28 |
| `RackCad.Tests.WorkspacePilotSurfaceGuardTests` | 43 / 0 / 0 | 43 / 0 / 0 |
| `RackCad.Tests.WorkspacePilotUpdateGuardTests` | 85 / 0 / 2 | 85 / 0 / 2 |
| `RackCad.Tests.WorkspacePilotUpdateHostRunTests` | 27 / 0 / 0 | 27 / 0 / 0 |
| `RackCad.Tests.WorkspacePilotUpdateHostTests` | 0 / 0 / 35 | 0 / 0 / 35 |
| `RackCad.Tests.WorkspacePilotUpdateIntegrationGuardTests` | 121 / 0 / 4 | 121 / 0 / 4 |
| `RackCad.Tests.WorkspacePilotUpdateRequestTests` | 0 / 0 / 44 | 0 / 0 / 44 |
| `RackCad.Tests.WorkspacePilotUpdateRunTests` | 57 / 0 / 0 | 57 / 0 / 0 |
| `RackCad.Tests.WorkspaceSelectionContextTests` | 13 / 0 / 0 | 13 / 0 / 0 |
| `RackCad.Tests.WorkspaceSelectionMappingTests` | 27 / 0 / 0 | 27 / 0 / 0 |
| `RackCad.Tests.WorkspaceSessionTests` | 8 / 0 / 0 | 8 / 0 / 0 |
| `RackCad.Tests.WorkspaceSourceGuardExceptionTests` | 88 / 0 / 0 | 88 / 0 / 0 |
| `RackCad.Tests.XlsxWriterTests` | 12 / 0 / 0 | 12 / 0 / 0 |
| `RackCad.UI.Tests.CantileverAdvancedPanelWindowTests` | 10 / 0 / 0 | 10 / 0 / 0 |
| `RackCad.UI.Tests.CantileverComponentEditorTests` | 33 / 0 / 0 | 33 / 0 / 0 |
| `RackCad.UI.Tests.CantileverEditorWindowTests` | 24 / 0 / 0 | 24 / 0 / 0 |
| `RackCad.UI.Tests.FullPanelCopyWorkspacePilotReconciliationGreenTests` | 30 / 0 / 0 | 30 / 0 / 0 |
| `RackCad.UI.Tests.FullPanelCopyWorkspacePilotReconciliationViewGreenTests` | 6 / 0 / 0 | 6 / 0 / 0 |
| `RackCad.UI.Tests.NumericFieldTests` | 10 / 0 / 0 | 10 / 0 / 0 |
| `RackCad.UI.Tests.NumericFieldValidationTests` | 13 / 0 / 0 | 13 / 0 / 0 |
| `RackCad.UI.Tests.PushBackClassicCharacterizationCloseTests` | 21 / 0 / 0 | 21 / 0 / 0 |
| `RackCad.UI.Tests.PushBackClassicCharacterizationIntentTests` | 12 / 0 / 0 | 12 / 0 / 0 |
| `RackCad.UI.Tests.PushBackClassicCharacterizationKeyboardTests` | 4 / 0 / 0 | 4 / 0 / 0 |
| `RackCad.UI.Tests.PushBackClassicCharacterizationOwnerTests` | 4 / 0 / 0 | 4 / 0 / 0 |
| `RackCad.UI.Tests.PushBackClassicCharacterizationResourceTests` | 2 / 0 / 0 | 2 / 0 / 0 |
| `RackCad.UI.Tests.PushBackClassicCharacterizationSourceGuardTests` | 12 / 0 / 0 | 12 / 0 / 0 |
| `RackCad.UI.Tests.PushBackDeclaredCorrection2MixedBoxTests` | 1 / 0 / 0 | 1 / 0 / 0 |
| `RackCad.UI.Tests.PushBackDeclaredCorrection2RoundingTests` | 3 / 0 / 0 | 3 / 0 / 0 |
| `RackCad.UI.Tests.PushBackDeclaredCorrectionDimensionStyleTests` | 2 / 0 / 0 | 2 / 0 / 0 |
| `RackCad.UI.Tests.PushBackDeclaredCorrectionPilotParityTests` | 2 / 0 / 0 | 2 / 0 / 0 |
| `RackCad.UI.Tests.PushBackDeclaredCorrectionRoundingTests` | 3 / 0 / 0 | 3 / 0 / 0 |
| `RackCad.UI.Tests.PushBackDeclaredCorrectionSideBNoticeTests` | 2 / 0 / 0 | 2 / 0 / 0 |
| `RackCad.UI.Tests.PushBackDeclaredCorrectionStandardRestoreTests` | 1 / 0 / 0 | 1 / 0 / 0 |
| `RackCad.UI.Tests.PushBackFieldValidationConcordanceTests` | 1 / 0 / 0 | 1 / 0 / 0 |
| `RackCad.UI.Tests.PushBackFullDraftWindowParityTests` | 1 / 0 / 0 | 1 / 0 / 0 |
| `RackCad.UI.Tests.PushBackFullEditorCharacterizationCatalogTests` | 1 / 0 / 0 | 1 / 0 / 0 |
| `RackCad.UI.Tests.PushBackFullEditorCharacterizationCompositeTests` | 4 / 0 / 0 | 4 / 0 / 0 |
| `RackCad.UI.Tests.PushBackFullEditorCharacterizationModuleTests` | 2 / 0 / 1 | 2 / 0 / 1 |
| `RackCad.UI.Tests.PushBackFullEditorCharacterizationRoundingLossTests` | 1 / 0 / 1 | 1 / 0 / 1 |
| `RackCad.UI.Tests.PushBackFullEditorCharacterizationStateTests` | 3 / 0 / 0 | 3 / 0 / 0 |
| `RackCad.UI.Tests.PushBackFullEditorCharacterizationSubdialogTests` | 5 / 0 / 0 | 5 / 0 / 0 |
| `RackCad.UI.Tests.PushBackFullPanelFieldTests` | 3 / 0 / 0 | 3 / 0 / 0 |
| `RackCad.UI.Tests.PushBackStateNetActionsTests` | 2 / 0 / 0 | 2 / 0 / 0 |
| `RackCad.UI.Tests.PushBackStateNetCellAndFrontTests` | 4 / 0 / 0 | 4 / 0 / 0 |
| `RackCad.UI.Tests.PushBackStateNetCompositeTests` | 3 / 0 / 0 | 3 / 0 / 0 |
| `RackCad.UI.Tests.PushBackStateNetFingerprintTests` | 1 / 0 / 0 | 1 / 0 / 0 |
| `RackCad.UI.Tests.PushBackStateNetModuleAndSafetyTests` | 2 / 0 / 0 | 2 / 0 / 0 |
| `RackCad.UI.Tests.PushBackStateNetRackControlsTests` | 2 / 0 / 0 | 2 / 0 / 0 |
| `RackCad.UI.Tests.PushBackWindowOnStateTests` | 2 / 0 / 0 | 2 / 0 / 0 |
| `RackCad.UI.Tests.PushBackWorkspaceApplyCloseTests` | 4 / 0 / 0 | 3 / 1 / 0 |
| `RackCad.UI.Tests.PushBackWorkspaceCarryoverTests` | 3 / 0 / 0 | 3 / 0 / 0 |
| `RackCad.UI.Tests.PushBackWorkspaceModeTests` | 3 / 0 / 0 | 3 / 0 / 0 |
| `RackCad.UI.Tests.WorkspaceBrowserScenarioHarnessTests` | 5 / 0 / 0 | 5 / 0 / 0 |
| `RackCad.UI.Tests.WorkspaceBrowserViewModelTests` | 49 / 0 / 0 | 49 / 0 / 0 |
| `RackCad.UI.Tests.WorkspaceBrowserViewTests` | 13 / 0 / 0 | 13 / 0 / 0 |
| `RackCad.UI.Tests.WorkspaceDraftPilotCanonicalLineEndingTests` | 9 / 0 / 0 | 9 / 0 / 0 |
| `RackCad.UI.Tests.WorkspaceDraftPilotValidationParityTests` | 9 / 0 / 0 | 9 / 0 / 0 |
| `RackCad.UI.Tests.WorkspaceDraftPilotWindowLoadCharacterizationTests` | 3 / 0 / 1 | 3 / 0 / 1 |
| `RackCad.UI.Tests.WorkspaceNavigationViewModelTests` | 73 / 0 / 0 | 73 / 0 / 0 |
| `RackCad.UI.Tests.WorkspaceNavigationViewTests` | 18 / 0 / 0 | 18 / 0 / 0 |
| `RackCad.UI.Tests.WorkspacePanelContextTests` | 15 / 0 / 0 | 15 / 0 / 0 |
| `RackCad.UI.Tests.WorkspacePanelErrorTests` | 11 / 0 / 0 | 11 / 0 / 0 |
| `RackCad.UI.Tests.WorkspacePanelInstrumentationTests` | 10 / 0 / 0 | 10 / 0 / 0 |
| `RackCad.UI.Tests.WorkspacePanelNoWorkTests` | 8 / 0 / 0 | 8 / 0 / 0 |
| `RackCad.UI.Tests.WorkspacePanelStateTests` | 12 / 0 / 0 | 12 / 0 / 0 |
| `RackCad.UI.Tests.WorkspacePanelViewTests` | 8 / 0 / 0 | 8 / 0 / 0 |
| `RackCad.UI.Tests.WorkspacePilotCloseSyncTests` | 78 / 0 / 0 | 78 / 0 / 0 |
| `RackCad.UI.Tests.WorkspacePilotCloseSyncViewTests` | 17 / 0 / 0 | 17 / 0 / 0 |
| `RackCad.UI.Tests.WorkspacePilotFieldViewModelTests` | 160 / 0 / 0 | 160 / 0 / 0 |
| `RackCad.UI.Tests.WorkspacePilotFieldViewTests` | 43 / 0 / 0 | 43 / 0 / 0 |
| `RackCad.UI.Tests.WorkspacePilotReconciliationTests` | 100 / 0 / 21 | 100 / 0 / 21 |
| `RackCad.UI.Tests.WorkspacePilotReconciliationViewTests` | 3 / 0 / 6 | 3 / 0 / 6 |
| **Total (763 clases)** | **20032 / 1 / 491** | **20031 / 2 / 491** |

TRX leidas (36): RED core-red.trx 4bba1f6d7ffe; RED red-ui-RackCad_UI_Tests_Cantilever.trx 02aa95170393; RED red-ui-RackCad_UI_Tests_FullPanelCopyWorkspacePilotReconciliationGreenTests.trx 0e80dda02381; RED red-ui-RackCad_UI_Tests_FullPanelCopyWorkspacePilotReconciliationViewGreenTests.trx fe7c77b60d92; RED red-ui-RackCad_UI_Tests_NumericFieldTests.trx dfb393934dff; RED red-ui-RackCad_UI_Tests_NumericFieldValidationTests.trx 43390b79fa9a; RED red-ui-RackCad_UI_Tests_PushBackClassicCharacterization.trx dd8d4e6c47d8; RED red-ui-RackCad_UI_Tests_PushBackDeclaredCorrection.trx b524322049f9; RED red-ui-RackCad_UI_Tests_PushBackFieldValidationConcordanceTests.trx 60856b2065ab; RED red-ui-RackCad_UI_Tests_PushBackFullDraftWindowParityTests.trx eda975394a20; RED red-ui-RackCad_UI_Tests_PushBackFullEditorCharacterization.trx 4534ceb4d950; RED red-ui-RackCad_UI_Tests_PushBackFullPanelFieldTests.trx 024fa59814e7; RED red-ui-RackCad_UI_Tests_PushBackStateNet.trx 3e8e5d82f064; RED red-ui-RackCad_UI_Tests_PushBackWindowOnStateTests.trx e41bf59250f5; RED red-ui-RackCad_UI_Tests_PushBackWorkspaceApplyCloseTests.trx 8231f3c2dd1b; RED red-ui-RackCad_UI_Tests_PushBackWorkspaceCarryoverTests.trx 2ff9ffb1d41b; RED red-ui-RackCad_UI_Tests_PushBackWorkspaceModeTests.trx 47211dd2147f; RED red-ui-RackCad_UI_Tests_Workspace.trx 5fc3f3a1f810; GREEN core-green.trx 8501d96738dd; GREEN green-ui-RackCad_UI_Tests_Cantilever.trx f664229fb15c; GREEN green-ui-RackCad_UI_Tests_FullPanelCopyWorkspacePilotReconciliationGreenTests.trx 88fb326a082b; GREEN green-ui-RackCad_UI_Tests_FullPanelCopyWorkspacePilotReconciliationViewGreenTests.trx f619737a18ea; GREEN green-ui-RackCad_UI_Tests_NumericFieldTests.trx 5991e3d29772; GREEN green-ui-RackCad_UI_Tests_NumericFieldValidationTests.trx 803932138309; GREEN green-ui-RackCad_UI_Tests_PushBackClassicCharacterization.trx 07cd856369a4; GREEN green-ui-RackCad_UI_Tests_PushBackDeclaredCorrection.trx 3eebdea74133; GREEN green-ui-RackCad_UI_Tests_PushBackFieldValidationConcordanceTests.trx e07ead567532; GREEN green-ui-RackCad_UI_Tests_PushBackFullDraftWindowParityTests.trx bb21057d775e; GREEN green-ui-RackCad_UI_Tests_PushBackFullEditorCharacterization.trx b9c9629d28c1; GREEN green-ui-RackCad_UI_Tests_PushBackFullPanelFieldTests.trx bea1bf1b6c87; GREEN green-ui-RackCad_UI_Tests_PushBackStateNet.trx eabea3a7f4f5; GREEN green-ui-RackCad_UI_Tests_PushBackWindowOnStateTests.trx a61e95fc8540; GREEN green-ui-RackCad_UI_Tests_PushBackWorkspaceApplyCloseTests.trx e016a2e364f4; GREEN green-ui-RackCad_UI_Tests_PushBackWorkspaceCarryoverTests.trx 38d9439e27c2; GREEN green-ui-RackCad_UI_Tests_PushBackWorkspaceModeTests.trx 5172437a0b4b; GREEN green-ui-RackCad_UI_Tests_Workspace.trx 65979028e61e

**El resultado de cada filtro de `/d/r5g`** (la primera línea de cada `*-outcomes.txt` de `out-redgreen-v50b-r5g/`):

| Registro | sha256 | Resultados | Passed | Failed | Skipped |
|---|---|---|---|---|---|
| `core-green-outcomes.txt` | `6cd9dd2c…a200` | 19608 | 19147 | 0 | 461 |
| `core-red-outcomes.txt` | `c210a2a8…abdc` | 19608 | 19147 | 0 | 461 |
| `green-ui-RackCad_UI_Tests_Cantilever-outcomes.txt` | `3fb57f5c…4f45` | 67 | 67 | 0 | 0 |
| `green-ui-RackCad_UI_Tests_FullPanelCopyWorkspacePilotReconciliationGreenTests-outcomes.txt` | `0cd4e6f5…ee6e` | 30 | 30 | 0 | 0 |
| `green-ui-RackCad_UI_Tests_FullPanelCopyWorkspacePilotReconciliationViewGreenTests-outcomes.txt` | `c51a2dc3…a0eb` | 6 | 6 | 0 | 0 |
| `green-ui-RackCad_UI_Tests_NumericFieldTests-outcomes.txt` | `97070151…8942` | 10 | 10 | 0 | 0 |
| `green-ui-RackCad_UI_Tests_NumericFieldValidationTests-outcomes.txt` | `05833645…ab86` | 13 | 13 | 0 | 0 |
| `green-ui-RackCad_UI_Tests_PushBackClassicCharacterization-outcomes.txt` | `5b987766…e6b2` | 55 | 55 | 0 | 0 |
| `green-ui-RackCad_UI_Tests_PushBackDeclaredCorrection-outcomes.txt` | `bf63c5d7…3534` | 14 | 14 | 0 | 0 |
| `green-ui-RackCad_UI_Tests_PushBackEditAllApplyCloseTests-outcomes.txt` | `f244feda…96b4` | 1 | 1 | 0 | 0 |
| `green-ui-RackCad_UI_Tests_PushBackFieldValidationConcordanceTests-outcomes.txt` | `7f244710…a6b0` | 1 | 1 | 0 | 0 |
| `green-ui-RackCad_UI_Tests_PushBackFullDraftWindowParityTests-outcomes.txt` | `a87b0c9e…6ce4` | 1 | 1 | 0 | 0 |
| `green-ui-RackCad_UI_Tests_PushBackFullEditorCharacterization-outcomes.txt` | `a12ded78…37d5` | 18 | 16 | 0 | 2 |
| `green-ui-RackCad_UI_Tests_PushBackFullPanelFieldTests-outcomes.txt` | `45b40b73…d087` | 3 | 3 | 0 | 0 |
| `green-ui-RackCad_UI_Tests_PushBackStateNet-outcomes.txt` | `09f13412…4b3b` | 14 | 14 | 0 | 0 |
| `green-ui-RackCad_UI_Tests_PushBackWindowOnStateTests-outcomes.txt` | `c7a8ddc0…f1bb` | 2 | 2 | 0 | 0 |
| `green-ui-RackCad_UI_Tests_PushBackWorkspaceApplyCloseTests-outcomes.txt` | `113e1710…cd6b` | 4 | 3 | 0 | 1 |
| `green-ui-RackCad_UI_Tests_PushBackWorkspaceCarryoverTests-outcomes.txt` | `a8294981…930a` | 3 | 3 | 0 | 0 |
| `green-ui-RackCad_UI_Tests_PushBackWorkspaceModeTests-outcomes.txt` | `8a30a8a1…b745` | 3 | 3 | 0 | 0 |
| `green-ui-RackCad_UI_Tests_Workspace-outcomes.txt` | `8aed0f64…0d6b` | 672 | 644 | 0 | 28 |
| `red-ui-RackCad_UI_Tests_Cantilever-outcomes.txt` | `4dae0009…603c` | 67 | 67 | 0 | 0 |
| `red-ui-RackCad_UI_Tests_FullPanelCopyWorkspacePilotReconciliationGreenTests-outcomes.txt` | `6e5fcae6…06f2` | 30 | 30 | 0 | 0 |
| `red-ui-RackCad_UI_Tests_FullPanelCopyWorkspacePilotReconciliationViewGreenTests-outcomes.txt` | `d6c2e050…f3ce` | 6 | 6 | 0 | 0 |
| `red-ui-RackCad_UI_Tests_NumericFieldTests-outcomes.txt` | `22f16303…6b9b` | 10 | 10 | 0 | 0 |
| `red-ui-RackCad_UI_Tests_NumericFieldValidationTests-outcomes.txt` | `ce8d63a4…8ddb` | 13 | 13 | 0 | 0 |
| `red-ui-RackCad_UI_Tests_PushBackClassicCharacterization-outcomes.txt` | `b7820efc…aa3b` | 55 | 55 | 0 | 0 |
| `red-ui-RackCad_UI_Tests_PushBackDeclaredCorrection-outcomes.txt` | `b948c50c…69be` | 14 | 14 | 0 | 0 |
| `red-ui-RackCad_UI_Tests_PushBackEditAllApplyCloseTests-outcomes.txt` | `56e77ed8…ee94` | 1 | 0 | 1 | 0 |
| `red-ui-RackCad_UI_Tests_PushBackFieldValidationConcordanceTests-outcomes.txt` | `5e3c0524…57f4` | 1 | 1 | 0 | 0 |
| `red-ui-RackCad_UI_Tests_PushBackFullDraftWindowParityTests-outcomes.txt` | `a552e966…c344` | 1 | 1 | 0 | 0 |
| `red-ui-RackCad_UI_Tests_PushBackFullEditorCharacterization-outcomes.txt` | `37ada7da…93e5` | 18 | 16 | 0 | 2 |
| `red-ui-RackCad_UI_Tests_PushBackFullPanelFieldTests-outcomes.txt` | `7300ff84…8038` | 3 | 3 | 0 | 0 |
| `red-ui-RackCad_UI_Tests_PushBackStateNet-outcomes.txt` | `b663a8a2…e52e` | 14 | 14 | 0 | 0 |
| `red-ui-RackCad_UI_Tests_PushBackWindowOnStateTests-outcomes.txt` | `21fec1b1…0af7` | 2 | 2 | 0 | 0 |
| `red-ui-RackCad_UI_Tests_PushBackWorkspaceApplyCloseTests-outcomes.txt` | `4b445a51…2d95` | 4 | 3 | 0 | 1 |
| `red-ui-RackCad_UI_Tests_PushBackWorkspaceCarryoverTests-outcomes.txt` | `55f992de…ddda` | 3 | 3 | 0 | 0 |
| `red-ui-RackCad_UI_Tests_PushBackWorkspaceModeTests-outcomes.txt` | `9852d5f7…ee74` | 3 | 3 | 0 | 0 |
| `red-ui-RackCad_UI_Tests_Workspace-outcomes.txt` | `8d5b2102…aaf2` | 672 | 644 | 0 | 28 |

**El resultado de cada clase que corre en `/d/r5g`** (la suite completa de Core y los filtros de UI de la v5.0, en cada rama (las TRX, en la copia exportada); `v51-clases/r5g.txt`, sha256 `b3eb3988…2d95`; Passed / Failed / Skipped):

| Clase | RED (P / F / S) | GREEN (P / F / S) |
|---|---|---|
| `RackCad.Tests.AgentExecutionProtocolTests` | 17 / 0 / 0 | 17 / 0 / 0 |
| `RackCad.Tests.AiscWorkbookVerificationTests` | 11 / 0 / 0 | 11 / 0 / 0 |
| `RackCad.Tests.AtomicFileTests` | 4 / 0 / 0 | 4 / 0 / 0 |
| `RackCad.Tests.BasePlatePeraltePersistenceTests` | 2 / 0 / 0 | 2 / 0 / 0 |
| `RackCad.Tests.BeamCatalogTests` | 3 / 0 / 0 | 3 / 0 / 0 |
| `RackCad.Tests.BlankFrontBoundaryTests` | 15 / 0 / 0 | 15 / 0 / 0 |
| `RackCad.Tests.BlankFrontSafetyTests` | 9 / 0 / 0 | 9 / 0 / 0 |
| `RackCad.Tests.BlankFrontTests` | 28 / 0 / 0 | 28 / 0 / 0 |
| `RackCad.Tests.BlankIdPredicateScopeGuardTests` | 2 / 0 / 0 | 2 / 0 / 0 |
| `RackCad.Tests.BlockNamingTests` | 8 / 0 / 0 | 8 / 0 / 0 |
| `RackCad.Tests.BomBuilderTests` | 6 / 0 / 0 | 6 / 0 / 0 |
| `RackCad.Tests.BomExcelCharacterizationTests` | 5 / 0 / 0 | 5 / 0 / 0 |
| `RackCad.Tests.BomExcelPivotTests` | 14 / 0 / 0 | 14 / 0 / 0 |
| `RackCad.Tests.BomExcelSmokeFixtures` | 1 / 0 / 0 | 1 / 0 / 0 |
| `RackCad.Tests.BomExcelSystemMatrixTests` | 15 / 0 / 0 | 15 / 0 / 0 |
| `RackCad.Tests.BracingPanelMemberBuilderTests` | 11 / 0 / 0 | 11 / 0 / 0 |
| `RackCad.Tests.CallerOwnedFacadeGuardTests` | 81 / 0 / 0 | 81 / 0 / 0 |
| `RackCad.Tests.CallerOwnedTransactionGuardTests` | 15 / 0 / 0 | 15 / 0 / 0 |
| `RackCad.Tests.CantileverAdapterSectionViewTests` | 12 / 0 / 0 | 12 / 0 / 0 |
| `RackCad.Tests.CantileverAdvancedPanelLayoutTests` | 27 / 0 / 0 | 27 / 0 / 0 |
| `RackCad.Tests.CantileverArmConnectionMetricsCharacterizationTests` | 9 / 0 / 0 | 9 / 0 / 0 |
| `RackCad.Tests.CantileverArmDefaultsAndFrontalTests` | 14 / 0 / 0 | 14 / 0 / 0 |
| `RackCad.Tests.CantileverArmTests` | 117 / 0 / 0 | 117 / 0 / 0 |
| `RackCad.Tests.CantileverBraceRepresentationTests` | 32 / 0 / 0 | 32 / 0 / 0 |
| `RackCad.Tests.CantileverColumnBaseFollowTests` | 15 / 0 / 0 | 15 / 0 / 0 |
| `RackCad.Tests.CantileverColumnBaseTests` | 84 / 0 / 0 | 84 / 0 / 0 |
| `RackCad.Tests.CantileverDoubleStationLayoutTests` | 8 / 0 / 0 | 8 / 0 / 0 |
| `RackCad.Tests.CantileverLateralViewTests` | 12 / 0 / 0 | 12 / 0 / 0 |
| `RackCad.Tests.CantileverLineEditorAssemblerTests` | 10 / 0 / 0 | 10 / 0 / 0 |
| `RackCad.Tests.CantileverLineTests` | 73 / 0 / 0 | 73 / 0 / 0 |
| `RackCad.Tests.CantileverPersistenceAndViewTests` | 28 / 0 / 0 | 28 / 0 / 0 |
| `RackCad.Tests.CantileverPlantaViewTests` | 8 / 0 / 0 | 8 / 0 / 0 |
| `RackCad.Tests.CantileverPluginSourceGuardTests` | 17 / 0 / 0 | 17 / 0 / 0 |
| `RackCad.Tests.CantileverPunchRepresentationTests` | 14 / 0 / 0 | 14 / 0 / 0 |
| `RackCad.Tests.CantileverRegularPunchGridCharacterizationTests` | 16 / 0 / 0 | 16 / 0 / 0 |
| `RackCad.Tests.CantileverRetiredMarginTests` | 11 / 0 / 0 | 11 / 0 / 0 |
| `RackCad.Tests.CantileverRoundThreeCorrectionTests` | 27 / 0 / 0 | 27 / 0 / 0 |
| `RackCad.Tests.CantileverRoundTwoCharacterizationTests` | 12 / 0 / 0 | 12 / 0 / 0 |
| `RackCad.Tests.CantileverRoundTwoSourceGuardTests` | 38 / 0 / 0 | 38 / 0 / 0 |
| `RackCad.Tests.CantileverSeparatorWebTests` | 6 / 0 / 0 | 6 / 0 / 0 |
| `RackCad.Tests.CantileverSourceGuardTests` | 90 / 0 / 0 | 90 / 0 / 0 |
| `RackCad.Tests.CantileverStationCorrectionTests` | 78 / 0 / 0 | 78 / 0 / 0 |
| `RackCad.Tests.CantileverStationTests` | 81 / 0 / 0 | 81 / 0 / 0 |
| `RackCad.Tests.CantileverVerticalDatumTests` | 15 / 0 / 0 | 15 / 0 / 0 |
| `RackCad.Tests.CantileverVisualRoleTests` | 15 / 0 / 0 | 15 / 0 / 0 |
| `RackCad.Tests.CatalogBackedStandardServiceTests` | 4 / 0 / 0 | 4 / 0 / 0 |
| `RackCad.Tests.CatalogBlockManifestTests` | 13 / 0 / 0 | 13 / 0 / 0 |
| `RackCad.Tests.CatalogBlockParametersTests` | 8 / 0 / 0 | 8 / 0 / 0 |
| `RackCad.Tests.CatalogCanonicalIdsTests` | 1 / 0 / 0 | 1 / 0 / 0 |
| `RackCad.Tests.CatalogDirectoryTests` | 2 / 0 / 0 | 2 / 0 / 0 |
| `RackCad.Tests.CatalogManifestGuardTests` | 3 / 0 / 0 | 3 / 0 / 0 |
| `RackCad.Tests.CatalogStandardConsistencyTests` | 5 / 0 / 0 | 5 / 0 / 0 |
| `RackCad.Tests.CatalogValidatorTests` | 22 / 0 / 0 | 22 / 0 / 0 |
| `RackCad.Tests.CommonTransform2DTests` | 6 / 0 / 0 | 6 / 0 / 0 |
| `RackCad.Tests.ComputedParametersPopulationGuardTests` | 3 / 0 / 0 | 3 / 0 / 0 |
| `RackCad.Tests.ComputedParametersPopulationTests` | 51 / 0 / 0 | 51 / 0 / 0 |
| `RackCad.Tests.ComputedParametersRackMetricIdsTests` | 10 / 0 / 0 | 10 / 0 / 0 |
| `RackCad.Tests.ComputedParametersRackMetricProviderPurityTests` | 3 / 0 / 0 | 3 / 0 / 0 |
| `RackCad.Tests.ComputedParametersRackMetricRequestTests` | 14 / 0 / 0 | 14 / 0 / 0 |
| `RackCad.Tests.ComputedParametersSummaryContextTests` | 3 / 0 / 0 | 3 / 0 / 0 |
| `RackCad.Tests.ComputedParametersSummaryGuardTests` | 2 / 0 / 0 | 2 / 0 / 0 |
| `RackCad.Tests.ComputedParametersSummaryPerformanceTests` | 6 / 0 / 0 | 6 / 0 / 0 |
| `RackCad.Tests.ComputedParametersSummaryTests` | 18 / 0 / 0 | 18 / 0 / 0 |
| `RackCad.Tests.ComputedParametersSymbolsBindingTests` | 22 / 0 / 0 | 22 / 0 / 0 |
| `RackCad.Tests.ComputedParametersSymbolsConsumersTests` | 4 / 0 / 0 | 4 / 0 / 0 |
| `RackCad.Tests.ComputedParametersSymbolsContextTests` | 14 / 0 / 0 | 14 / 0 / 0 |
| `RackCad.Tests.ComputedParametersSymbolsFormattingTests` | 12 / 0 / 0 | 12 / 0 / 0 |
| `RackCad.Tests.ComputedParametersSymbolsIdentityTests` | 33 / 0 / 0 | 33 / 0 / 0 |
| `RackCad.Tests.ComputedParametersSymbolsPersistenceTests` | 4 / 0 / 0 | 4 / 0 / 0 |
| `RackCad.Tests.ConsolidatedBomBuilderTests` | 2 / 0 / 0 | 2 / 0 / 0 |
| `RackCad.Tests.ConsolidatedBomXlsxDetailTests` | 13 / 0 / 0 | 13 / 0 / 0 |
| `RackCad.Tests.CsvCatalogReaderTests` | 9 / 0 / 0 | 9 / 0 / 0 |
| `RackCad.Tests.CsvLexerTests` | 10 / 0 / 0 | 10 / 0 / 0 |
| `RackCad.Tests.CustomPropertiesAuthorityTests` | 87 / 0 / 0 | 87 / 0 / 0 |
| `RackCad.Tests.CustomPropertiesCommandGuardTests` | 48 / 0 / 0 | 48 / 0 / 0 |
| `RackCad.Tests.CustomPropertiesCommitTests` | 35 / 0 / 0 | 35 / 0 / 0 |
| `RackCad.Tests.CustomPropertiesEdgeGuardTests` | 52 / 0 / 0 | 52 / 0 / 0 |
| `RackCad.Tests.CustomPropertiesEnvelopeCharacterizationTests` | 103 / 0 / 0 | 103 / 0 / 0 |
| `RackCad.Tests.CustomPropertiesEnvelopeGuardTests` | 22 / 0 / 0 | 22 / 0 / 0 |
| `RackCad.Tests.CustomPropertiesEnvelopeTests` | 150 / 0 / 0 | 150 / 0 / 0 |
| `RackCad.Tests.CustomPropertiesMutationTests` | 17 / 0 / 0 | 17 / 0 / 0 |
| `RackCad.Tests.CustomPropertiesProjectTests` | 32 / 0 / 0 | 32 / 0 / 0 |
| `RackCad.Tests.CustomPropertiesStoreTests` | 228 / 0 / 0 | 228 / 0 / 0 |
| `RackCad.Tests.CustomPropertiesWorkspaceTests` | 21 / 0 / 0 | 21 / 0 / 0 |
| `RackCad.Tests.DebugFaultInjectionGuardTests` | 1 / 0 / 0 | 1 / 0 / 0 |
| `RackCad.Tests.DiagnosticsNegativeTests` | 3 / 0 / 0 | 3 / 0 / 0 |
| `RackCad.Tests.DimensionViewPolicyTests` | 297 / 0 / 0 | 297 / 0 / 0 |
| `RackCad.Tests.DimensionViewsCopySitesTests` | 24 / 0 / 0 | 24 / 0 / 0 |
| `RackCad.Tests.DimensionViewsLegacyJsonTests` | 3 / 0 / 0 | 3 / 0 / 0 |
| `RackCad.Tests.DimensionViewsNoneWinsTests` | 36 / 0 / 0 | 36 / 0 / 0 |
| `RackCad.Tests.DimensionViewsPhysicalAndBomInvarianceTests` | 72 / 0 / 0 | 72 / 0 / 0 |
| `RackCad.Tests.DimensionViewsRestampTests` | 11 / 0 / 0 | 11 / 0 / 0 |
| `RackCad.Tests.DrawServicePlanBaselineTests` | 8 / 0 / 0 | 8 / 0 / 0 |
| `RackCad.Tests.DrawingUnitsAdvisoryTests` | 6 / 0 / 0 | 6 / 0 / 0 |
| `RackCad.Tests.DynamicDepthGeometryTests` | 6 / 0 / 0 | 6 / 0 / 0 |
| `RackCad.Tests.DynamicDimensionLegacyCharacterizationTests` | 5 / 0 / 0 | 5 / 0 / 0 |
| `RackCad.Tests.DynamicDimensionViewsDocumentTests` | 9 / 0 / 0 | 9 / 0 / 0 |
| `RackCad.Tests.DynamicDimensionViewsEndToEndTests` | 10 / 0 / 0 | 10 / 0 / 0 |
| `RackCad.Tests.DynamicDimensionViewsTests` | 66 / 0 / 0 | 66 / 0 / 0 |
| `RackCad.Tests.DynamicEditorCellTests` | 12 / 0 / 0 | 12 / 0 / 0 |
| `RackCad.Tests.DynamicEditorDesignAssemblerTests` | 8 / 0 / 0 | 8 / 0 / 0 |
| `RackCad.Tests.DynamicEditorSafetyTests` | 7 / 0 / 0 | 7 / 0 / 0 |
| `RackCad.Tests.DynamicEntranceGuidePlanTests` | 2 / 0 / 0 | 2 / 0 / 0 |
| `RackCad.Tests.DynamicFlowBedLateralBuilderTests` | 4 / 0 / 0 | 4 / 0 / 0 |
| `RackCad.Tests.DynamicForkliftDefensePlanTests` | 2 / 0 / 0 | 2 / 0 / 0 |
| `RackCad.Tests.DynamicFrontGeometryTests` | 4 / 0 / 0 | 4 / 0 / 0 |
| `RackCad.Tests.DynamicFrontMatrixTests` | 20 / 0 / 0 | 20 / 0 / 0 |
| `RackCad.Tests.DynamicHeaderBatchLengthTests` | 6 / 0 / 0 | 6 / 0 / 0 |
| `RackCad.Tests.DynamicHeaderBatchMutateTests` | 11 / 0 / 0 | 11 / 0 / 0 |
| `RackCad.Tests.DynamicHeaderBatchPersistenceTests` | 4 / 0 / 0 | 4 / 0 / 0 |
| `RackCad.Tests.DynamicHeaderBatchPrepareTests` | 18 / 0 / 0 | 18 / 0 / 0 |
| `RackCad.Tests.DynamicHeaderBatchReconciliationTests` | 4 / 0 / 0 | 4 / 0 / 0 |
| `RackCad.Tests.DynamicHeaderHeightCalculatorTests` | 7 / 0 / 0 | 7 / 0 / 0 |
| `RackCad.Tests.DynamicLoadBeamGeometryTests` | 4 / 0 / 0 | 4 / 0 / 0 |
| `RackCad.Tests.DynamicModuleTargetsTests` | 18 / 0 / 0 | 18 / 0 / 0 |
| `RackCad.Tests.DynamicNullOverrideGoldenTests` | 1 / 0 / 0 | 1 / 0 / 0 |
| `RackCad.Tests.DynamicPostLevelsTests` | 3 / 0 / 0 | 3 / 0 / 0 |
| `RackCad.Tests.DynamicRackSystemBuilderTests` | 21 / 0 / 0 | 21 / 0 / 0 |
| `RackCad.Tests.DynamicRackSystemResolverTests` | 11 / 0 / 0 | 11 / 0 / 0 |
| `RackCad.Tests.DynamicSafetyDefaultsTests` | 2 / 0 / 0 | 2 / 0 / 0 |
| `RackCad.Tests.DynamicSystemLateralBuilderTests` | 18 / 0 / 0 | 18 / 0 / 0 |
| `RackCad.Tests.DynamicSystemMultiViewBuilderTests` | 12 / 0 / 0 | 12 / 0 / 0 |
| `RackCad.Tests.ElevationOverrideSourceGuardTests` | 3 / 0 / 0 | 3 / 0 / 0 |
| `RackCad.Tests.ExpressionBinderTests` | 65 / 0 / 0 | 65 / 0 / 0 |
| `RackCad.Tests.ExpressionBoundLimitsTests` | 16 / 0 / 0 | 16 / 0 / 0 |
| `RackCad.Tests.ExpressionCanonicalShapeTests` | 15 / 0 / 0 | 15 / 0 / 0 |
| `RackCad.Tests.ExpressionCoreGuardTests` | 6 / 0 / 0 | 6 / 0 / 0 |
| `RackCad.Tests.ExpressionDiagnosticCatalogTests` | 2 / 0 / 0 | 2 / 0 / 0 |
| `RackCad.Tests.ExpressionEvaluatorTests` | 15 / 0 / 0 | 15 / 0 / 0 |
| `RackCad.Tests.ExpressionFormatterTests` | 54 / 0 / 0 | 54 / 0 / 0 |
| `RackCad.Tests.ExpressionFunctionRegistryTests` | 34 / 0 / 0 | 34 / 0 / 0 |
| `RackCad.Tests.ExpressionParserGuardTests` | 38 / 0 / 0 | 38 / 0 / 0 |
| `RackCad.Tests.ExpressionRoundTripTests` | 12 / 0 / 0 | 12 / 0 / 0 |
| `RackCad.Tests.ExpressionSymbolModelTests` | 44 / 0 / 0 | 44 / 0 / 0 |
| `RackCad.Tests.ExpressionSyntaxDiagnosticsTests` | 223 / 0 / 0 | 223 / 0 / 0 |
| `RackCad.Tests.ExpressionSyntaxGrammarTests` | 122 / 0 / 0 | 122 / 0 / 0 |
| `RackCad.Tests.ExpressionTokenTablesTests` | 3 / 0 / 0 | 3 / 0 / 0 |
| `RackCad.Tests.FirstViewFreedomGuardTests` | 5 / 0 / 0 | 5 / 0 / 0 |
| `RackCad.Tests.FlowBedBomBuilderTests` | 2 / 0 / 0 | 2 / 0 / 0 |
| `RackCad.Tests.FlowBedCatalogTests` | 2 / 0 / 0 | 2 / 0 / 0 |
| `RackCad.Tests.FlowBedConfigurationStoreTests` | 2 / 0 / 0 | 2 / 0 / 0 |
| `RackCad.Tests.FlowBedLateralBuilderTests` | 9 / 0 / 0 | 9 / 0 / 0 |
| `RackCad.Tests.FrameModelValidatorTests` | 10 / 0 / 0 | 10 / 0 / 0 |
| `RackCad.Tests.FullPanelCopyPushBackFullReadGuardRedTests` | 1 / 0 / 0 | 1 / 0 / 0 |
| `RackCad.Tests.FullPanelCopyPushBackFullReadPathRedTests` | 1 / 0 / 0 | 1 / 0 / 0 |
| `RackCad.Tests.FullPanelCopyPushBackWritePathGuardRedTests` | 1 / 0 / 0 | 1 / 0 / 0 |
| `RackCad.Tests.FullPanelCopyWorkspaceAssociativeBookkeepingGreenTests` | 21 / 0 / 0 | 21 / 0 / 0 |
| `RackCad.Tests.FullPanelCopyWorkspaceAssociativeBookkeepingRedTests` | 1 / 0 / 0 | 1 / 0 / 0 |
| `RackCad.Tests.FullPanelCopyWorkspaceCommandAdvisorGreenTests` | 70 / 0 / 0 | 70 / 0 / 0 |
| `RackCad.Tests.FullPanelCopyWorkspaceCommandAdvisorRedTests` | 6 / 0 / 0 | 6 / 0 / 0 |
| `RackCad.Tests.FullPanelCopyWorkspaceEditIntentGuardRedTests` | 13 / 0 / 0 | 13 / 0 / 0 |
| `RackCad.Tests.FullPanelCopyWorkspaceEditIntentHostGreenTests` | 57 / 0 / 0 | 57 / 0 / 0 |
| `RackCad.Tests.FullPanelCopyWorkspaceEditIntentHostRedTests` | 24 / 0 / 0 | 24 / 0 / 0 |
| `RackCad.Tests.FullPanelCopyWorkspaceHostHardeningGuardRedTests` | 1 / 0 / 0 | 1 / 0 / 0 |
| `RackCad.Tests.FullPanelCopyWorkspaceLifecycleGuardRedTests` | 2 / 0 / 0 | 2 / 0 / 0 |
| `RackCad.Tests.FullPanelCopyWorkspaceLifecycleHostGreenTests` | 69 / 0 / 0 | 69 / 0 / 0 |
| `RackCad.Tests.FullPanelCopyWorkspaceLifecycleHostRedTests` | 24 / 0 / 0 | 24 / 0 / 0 |
| `RackCad.Tests.FullPanelCopyWorkspacePilotEditGuardGreenTests` | 17 / 0 / 0 | 17 / 0 / 0 |
| `RackCad.Tests.FullPanelCopyWorkspacePilotEditGuardRedTests` | 6 / 0 / 0 | 6 / 0 / 0 |
| `RackCad.Tests.FullPanelCopyWorkspacePilotEditHostGreenTests` | 17 / 0 / 0 | 17 / 0 / 0 |
| `RackCad.Tests.FullPanelCopyWorkspacePilotEditHostRedTests` | 59 / 0 / 0 | 59 / 0 / 0 |
| `RackCad.Tests.FullPanelCopyWorkspacePilotPrepareGreenTests` | 47 / 0 / 0 | 47 / 0 / 0 |
| `RackCad.Tests.FullPanelCopyWorkspacePilotPrepareRedTests` | 44 / 0 / 0 | 44 / 0 / 0 |
| `RackCad.Tests.FullPanelCopyWorkspacePilotReconciliationGuardGreenTests` | 1 / 0 / 0 | 1 / 0 / 0 |
| `RackCad.Tests.FullPanelCopyWorkspacePilotReconciliationHostGreenTests` | 5 / 0 / 0 | 5 / 0 / 0 |
| `RackCad.Tests.FullPanelCopyWorkspacePilotReconciliationHostRedTests` | 23 / 0 / 0 | 23 / 0 / 0 |
| `RackCad.Tests.FullPanelCopyWorkspacePilotUpdateGuardRedTests` | 1 / 0 / 0 | 1 / 0 / 0 |
| `RackCad.Tests.FullPanelCopyWorkspacePilotUpdateHostGreenTests` | 18 / 0 / 0 | 18 / 0 / 0 |
| `RackCad.Tests.FullPanelCopyWorkspacePilotUpdateHostRedTests` | 29 / 0 / 0 | 29 / 0 / 0 |
| `RackCad.Tests.FullPanelCopyWorkspacePilotUpdateIntegrationGuardRedTests` | 2 / 0 / 0 | 2 / 0 / 0 |
| `RackCad.Tests.FullPanelCopyWorkspacePilotUpdateRequestGreenTests` | 1 / 0 / 0 | 1 / 0 / 0 |
| `RackCad.Tests.FullPanelCopyWorkspacePilotUpdateRequestRedTests` | 53 / 0 / 0 | 53 / 0 / 0 |
| `RackCad.Tests.G10EditorContractTests` | 22 / 0 / 0 | 22 / 0 / 0 |
| `RackCad.Tests.G10LegacyEditorCharacterizationTests` | 8 / 0 / 0 | 8 / 0 / 0 |
| `RackCad.Tests.G10VariablesContractTests` | 9 / 0 / 0 | 9 / 0 / 0 |
| `RackCad.Tests.G11PerformanceMeasurementTests` | 3 / 0 / 0 | 3 / 0 / 0 |
| `RackCad.Tests.G11RealChainCandidateTests` | 2 / 0 / 0 | 2 / 0 / 0 |
| `RackCad.Tests.G12A1CreateExpressionHotfixTests` | 8 / 0 / 0 | 8 / 0 / 0 |
| `RackCad.Tests.G16BlockNameTests` | 14 / 0 / 0 | 14 / 0 / 0 |
| `RackCad.Tests.G16ElevationProjectionTests` | 98 / 0 / 0 | 98 / 0 / 0 |
| `RackCad.Tests.G16FirstViewCorrectiveTests` | 61 / 0 / 0 | 61 / 0 / 0 |
| `RackCad.Tests.G16OrientationAuthorityTests` | 256 / 0 / 0 | 256 / 0 / 0 |
| `RackCad.Tests.G16OrientationGuardTests` | 5 / 0 / 0 | 5 / 0 / 0 |
| `RackCad.Tests.G16ProjectedEnvelopeTests` | 42 / 0 / 0 | 42 / 0 / 0 |
| `RackCad.Tests.G16ProjectedOrientationTests` | 48 / 0 / 0 | 48 / 0 / 0 |
| `RackCad.Tests.G16UnnamedRackTests` | 28 / 0 / 0 | 28 / 0 / 0 |
| `RackCad.Tests.G7DependencyGraphContractTests` | 8 / 0 / 0 | 8 / 0 / 0 |
| `RackCad.Tests.G7RegistryEvaluationContractTests` | 12 / 0 / 0 | 12 / 0 / 0 |
| `RackCad.Tests.G8LegacyPersistenceCharacterizationTests` | 9 / 0 / 0 | 9 / 0 / 0 |
| `RackCad.Tests.G8PersistenceIntegrationContractTests` | 53 / 0 / 0 | 53 / 0 / 0 |
| `RackCad.Tests.G9A3ContractMatrixTests` | 44 / 0 / 0 | 44 / 0 / 0 |
| `RackCad.Tests.G9ConformanceFixTests` | 6 / 0 / 0 | 6 / 0 / 0 |
| `RackCad.Tests.G9LegacyMutationCharacterizationTests` | 10 / 0 / 0 | 10 / 0 / 0 |
| `RackCad.Tests.G9MutationPropagationContractTests` | 23 / 0 / 0 | 23 / 0 / 0 |
| `RackCad.Tests.GeometryMateSolverTests` | 3 / 0 / 0 | 3 / 0 / 0 |
| `RackCad.Tests.GeometryPrimitivesTests` | 34 / 0 / 0 | 34 / 0 / 0 |
| `RackCad.Tests.HardcodedStandardRackFrameServiceTests` | 3 / 0 / 0 | 3 / 0 / 0 |
| `RackCad.Tests.HeaderBatchContractTests` | 31 / 0 / 0 | 31 / 0 / 0 |
| `RackCad.Tests.HeaderConfigurationSnapshotTests` | 19 / 0 / 0 | 19 / 0 / 0 |
| `RackCad.Tests.HeaderInstanceGrouperTests` | 6 / 0 / 0 | 6 / 0 / 0 |
| `RackCad.Tests.I55CantileverPlantaVisibilityPropagationTests` | 1 / 0 / 0 | 1 / 0 / 0 |
| `RackCad.Tests.I55DeferredProductFixCharacterizationTests` | 3 / 0 / 0 | 3 / 0 / 0 |
| `RackCad.Tests.I55G14FoundationConsumerTests` | 5 / 0 / 0 | 5 / 0 / 0 |
| `RackCad.Tests.I55G15Auth15ConsumerTests` | 5 / 0 / 0 | 5 / 0 / 0 |
| `RackCad.Tests.I58Conf58SchemaWhitespaceTests` | 580 / 0 / 0 | 580 / 0 / 0 |
| `RackCad.Tests.I58F1CharacterizationTests` | 1272 / 0 / 0 | 1272 / 0 / 0 |
| `RackCad.Tests.I58F1OracleChecks` | 640 / 0 / 0 | 640 / 0 / 0 |
| `RackCad.Tests.I58F2BoundaryTests` | 26 / 0 / 0 | 26 / 0 / 0 |
| `RackCad.Tests.I58F2MaterializationTests` | 3 / 0 / 0 | 3 / 0 / 0 |
| `RackCad.Tests.I58F2ReaderTests` | 408 / 0 / 0 | 408 / 0 / 0 |
| `RackCad.Tests.I58F3SeamTests` | 56 / 0 / 0 | 56 / 0 / 0 |
| `RackCad.Tests.I59F1CharacterizationTests` | 14 / 0 / 0 | 14 / 0 / 0 |
| `RackCad.Tests.I59F2BoundaryGuardTests` | 2 / 0 / 0 | 2 / 0 / 0 |
| `RackCad.Tests.I59F2ContractTests` | 59 / 0 / 0 | 59 / 0 / 0 |
| `RackCad.Tests.I59F3ConsumerIntegrationTests` | 8 / 0 / 0 | 8 / 0 / 0 |
| `RackCad.Tests.I59F3CorrectionTests` | 13 / 0 / 0 | 13 / 0 / 0 |
| `RackCad.Tests.I59F4ConformanceTests` | 4 / 0 / 0 | 4 / 0 / 0 |
| `RackCad.Tests.I60BatchCreationNamingTests` | 25 / 0 / 0 | 25 / 0 / 0 |
| `RackCad.Tests.I60RackLogicalNameTests` | 35 / 0 / 0 | 35 / 0 / 0 |
| `RackCad.Tests.I60RackNameWiringGuardTests` | 22 / 0 / 0 | 22 / 0 / 0 |
| `RackCad.Tests.I61CamaEditWiringGuardTests` | 10 / 0 / 0 | 10 / 0 / 0 |
| `RackCad.Tests.I61EditedRackNameTests` | 6 / 0 / 0 | 6 / 0 / 0 |
| `RackCad.Tests.InsertBlankIdFlowCharacterizationTests` | 7 / 0 / 0 | 7 / 0 / 0 |
| `RackCad.Tests.InsertRedrawLayerAccessCharacterizationTests` | 5 / 0 / 0 | 5 / 0 / 0 |
| `RackCad.Tests.JsonRackCatalogProviderTests` | 6 / 0 / 0 | 6 / 0 / 0 |
| `RackCad.Tests.KindDispatchTests` | 21 / 0 / 0 | 21 / 0 / 0 |
| `RackCad.Tests.KindHandlerGuardSourceTests` | 2 / 0 / 0 | 2 / 0 / 0 |
| `RackCad.Tests.LargueroBomBuilderTests` | 3 / 0 / 0 | 3 / 0 / 0 |
| `RackCad.Tests.LateralHeaderLayoutBuilderTests` | 18 / 0 / 0 | 18 / 0 / 0 |
| `RackCad.Tests.LateralHeaderParametersFactoryTests` | 4 / 0 / 0 | 4 / 0 / 0 |
| `RackCad.Tests.LegacyViewPayloadCompositionCharacterizationTests` | 8 / 0 / 0 | 8 / 0 / 0 |
| `RackCad.Tests.LengthUnitsAuthorityGuardTests` | 25 / 0 / 0 | 25 / 0 / 0 |
| `RackCad.Tests.LengthUnitsTests` | 5 / 0 / 0 | 5 / 0 / 0 |
| `RackCad.Tests.LevelElevationResolutionTests` | 1095 / 0 / 0 | 1095 / 0 / 0 |
| `RackCad.Tests.LevelSpacingH58ReproductionTests` | 3 / 0 / 0 | 3 / 0 / 0 |
| `RackCad.Tests.LinkedPropertyEditSessionTests` | 31 / 0 / 0 | 31 / 0 / 0 |
| `RackCad.Tests.LinkedPropertyFoundationTests` | 21 / 0 / 0 | 21 / 0 / 0 |
| `RackCad.Tests.LinkedPropertyKernelTests` | 31 / 0 / 0 | 31 / 0 / 0 |
| `RackCad.Tests.LinkedPropertyReconcilerTests` | 16 / 0 / 0 | 16 / 0 / 0 |
| `RackCad.Tests.LinkedPropertyViewPayloadTests` | 6 / 0 / 0 | 6 / 0 / 0 |
| `RackCad.Tests.LocalizedNumberParserTests` | 9 / 0 / 0 | 9 / 0 / 0 |
| `RackCad.Tests.MainMenuStructuralSectionAccessGuardTests` | 28 / 0 / 0 | 28 / 0 / 0 |
| `RackCad.Tests.MultiPropertyRealProofTests` | 30 / 0 / 0 | 30 / 0 / 0 |
| `RackCad.Tests.NamespaceFolderGuardTests` | 7 / 0 / 0 | 7 / 0 / 0 |
| `RackCad.Tests.NoProductPlannerGuardTests` | 3 / 0 / 0 | 3 / 0 / 0 |
| `RackCad.Tests.OrphanFirstJigTransactionGuardTests` | 2 / 0 / 0 | 2 / 0 / 0 |
| `RackCad.Tests.PalletToleranceRealProofTests` | 17 / 0 / 0 | 17 / 0 / 0 |
| `RackCad.Tests.PeralteListTests` | 4 / 0 / 0 | 4 / 0 / 0 |
| `RackCad.Tests.PersistedVariableTypeGrammarTests` | 36 / 0 / 0 | 36 / 0 / 0 |
| `RackCad.Tests.PersistenceEmbedInnerDesignTests` | 4 / 0 / 0 | 4 / 0 / 0 |
| `RackCad.Tests.PersistenceInnerPreflightTests` | 9 / 0 / 0 | 9 / 0 / 0 |
| `RackCad.Tests.PersistenceInnerSourceTests` | 13 / 0 / 0 | 13 / 0 / 0 |
| `RackCad.Tests.PersistenceLibraryTransportTests` | 4 / 0 / 0 | 4 / 0 / 0 |
| `RackCad.Tests.PersistenceReopenPreservationTests` | 22 / 0 / 0 | 22 / 0 / 0 |
| `RackCad.Tests.PersistenceUniformityTests` | 15 / 0 / 0 | 15 / 0 / 0 |
| `RackCad.Tests.PersistenceValidationTests` | 5 / 0 / 0 | 5 / 0 / 0 |
| `RackCad.Tests.PersistenceVersioningTests` | 7 / 0 / 0 | 7 / 0 / 0 |
| `RackCad.Tests.PlantaHeaderLayoutBuilderTests` | 6 / 0 / 0 | 6 / 0 / 0 |
| `RackCad.Tests.ProductPolicyIndependenceGuardTests` | 2 / 0 / 0 | 2 / 0 / 0 |
| `RackCad.Tests.ProjectVariableDiscoveryTests` | 39 / 0 / 0 | 39 / 0 / 0 |
| `RackCad.Tests.ProjectVariableMutationExecutorTests` | 28 / 0 / 0 | 28 / 0 / 0 |
| `RackCad.Tests.ProjectVariableMutationPreflightTests` | 46 / 0 / 0 | 46 / 0 / 0 |
| `RackCad.Tests.ProjectVariableScanProjectionTests` | 17 / 0 / 0 | 17 / 0 / 0 |
| `RackCad.Tests.ProjectVariablesConformanceTests` | 10 / 0 / 0 | 10 / 0 / 0 |
| `RackCad.Tests.ProjectVariablesDocumentTests` | 27 / 0 / 0 | 27 / 0 / 0 |
| `RackCad.Tests.ProjectVariablesModelTests` | 34 / 0 / 0 | 34 / 0 / 0 |
| `RackCad.Tests.ProjectVariablesRegistryAccessTests` | 14 / 0 / 0 | 14 / 0 / 0 |
| `RackCad.Tests.ProjectVariablesWorkspaceTests` | 46 / 0 / 0 | 46 / 0 / 0 |
| `RackCad.Tests.ProjectionAvailabilityPolicyTests` | 9 / 0 / 0 | 9 / 0 / 0 |
| `RackCad.Tests.ProjectionCodecPolicyTests` | 6 / 0 / 0 | 6 / 0 / 0 |
| `RackCad.Tests.PushBackA1BStorageTests` | 6 / 0 / 0 | 6 / 0 / 0 |
| `RackCad.Tests.PushBackA1PersistenceTests` | 5 / 0 / 0 | 5 / 0 / 0 |
| `RackCad.Tests.PushBackA1StructureTests` | 4 / 0 / 0 | 4 / 0 / 0 |
| `RackCad.Tests.PushBackAdvancedRackParametersTests` | 30 / 0 / 0 | 30 / 0 / 0 |
| `RackCad.Tests.PushBackBedAnchorTests` | 43 / 0 / 0 | 43 / 0 / 0 |
| `RackCad.Tests.PushBackBedAsymmetryTests` | 38 / 0 / 0 | 38 / 0 / 0 |
| `RackCad.Tests.PushBackBedInOutMateTests` | 41 / 0 / 0 | 41 / 0 / 0 |
| `RackCad.Tests.PushBackBedSlopeTests` | 3 / 0 / 0 | 3 / 0 / 0 |
| `RackCad.Tests.PushBackBedTangencyTests` | 4 / 0 / 0 | 4 / 0 / 0 |
| `RackCad.Tests.PushBackBlankFrontAuthorityTests` | 9 / 0 / 0 | 9 / 0 / 0 |
| `RackCad.Tests.PushBackBlankSideBDeclarationTests` | 9 / 0 / 0 | 9 / 0 / 0 |
| `RackCad.Tests.PushBackBlankSlotIdentityTests` | 11 / 0 / 0 | 11 / 0 / 0 |
| `RackCad.Tests.PushBackBlankStorageCoverageTests` | 12 / 0 / 0 | 12 / 0 / 0 |
| `RackCad.Tests.PushBackBomCommandGuardTests` | 4 / 0 / 0 | 4 / 0 / 0 |
| `RackCad.Tests.PushBackBomTests` | 5 / 0 / 0 | 5 / 0 / 0 |
| `RackCad.Tests.PushBackBootIdentityTests` | 32 / 0 / 0 | 32 / 0 / 0 |
| `RackCad.Tests.PushBackBootLowFaceTests` | 40 / 0 / 0 | 40 / 0 / 0 |
| `RackCad.Tests.PushBackBootSemanticsTests` | 84 / 0 / 0 | 84 / 0 / 0 |
| `RackCad.Tests.PushBackBootSidesTests` | 45 / 0 / 0 | 45 / 0 / 0 |
| `RackCad.Tests.PushBackBootTypeTests` | 28 / 0 / 0 | 28 / 0 / 0 |
| `RackCad.Tests.PushBackCellConfigurationCharacterizationTests` | 7 / 0 / 0 | 7 / 0 / 0 |
| `RackCad.Tests.PushBackCellConfigurationDeliveryTests` | 17 / 0 / 0 | 17 / 0 / 0 |
| `RackCad.Tests.PushBackCellDepthTests` | 37 / 0 / 0 | 37 / 0 / 0 |
| `RackCad.Tests.PushBackCellPalletTests` | 25 / 0 / 0 | 25 / 0 / 0 |
| `RackCad.Tests.PushBackClassicCharacterizationPluginGuardTests` | 10 / 0 / 0 | 10 / 0 / 0 |
| `RackCad.Tests.PushBackCompositeAuditTests` | 19 / 0 / 0 | 19 / 0 / 0 |
| `RackCad.Tests.PushBackCompositeBedTests` | 14 / 0 / 0 | 14 / 0 / 0 |
| `RackCad.Tests.PushBackCompositeBomTests` | 10 / 0 / 0 | 10 / 0 / 0 |
| `RackCad.Tests.PushBackCompositeCharacterizationTests` | 6 / 0 / 0 | 6 / 0 / 0 |
| `RackCad.Tests.PushBackCompositeCorrectionTests` | 37 / 0 / 0 | 37 / 0 / 0 |
| `RackCad.Tests.PushBackCompositeCorridaAuthorityTests` | 19 / 0 / 0 | 19 / 0 / 0 |
| `RackCad.Tests.PushBackCompositeEditingContractTests` | 18 / 0 / 0 | 18 / 0 / 0 |
| `RackCad.Tests.PushBackCompositeEditorTests` | 15 / 0 / 0 | 15 / 0 / 0 |
| `RackCad.Tests.PushBackCompositeFrontalTests` | 8 / 0 / 0 | 8 / 0 / 0 |
| `RackCad.Tests.PushBackCompositeHeightTests` | 7 / 0 / 0 | 7 / 0 / 0 |
| `RackCad.Tests.PushBackCompositeLifecycleTests` | 8 / 0 / 0 | 8 / 0 / 0 |
| `RackCad.Tests.PushBackCompositeModelTests` | 13 / 0 / 0 | 13 / 0 / 0 |
| `RackCad.Tests.PushBackCompositeModuleSequenceTests` | 9 / 0 / 0 | 9 / 0 / 0 |
| `RackCad.Tests.PushBackCompositeMultiFrontTests` | 28 / 0 / 0 | 28 / 0 / 0 |
| `RackCad.Tests.PushBackCompositeRecomputeContainmentTests` | 2 / 0 / 0 | 2 / 0 / 0 |
| `RackCad.Tests.PushBackCompositeRoundTripTests` | 13 / 0 / 0 | 13 / 0 / 0 |
| `RackCad.Tests.PushBackCompositeSafetyOwnershipTests` | 9 / 0 / 0 | 9 / 0 / 0 |
| `RackCad.Tests.PushBackCompositeSafetyTests` | 10 / 0 / 0 | 10 / 0 / 0 |
| `RackCad.Tests.PushBackCompositeStructureTests` | 15 / 0 / 0 | 15 / 0 / 0 |
| `RackCad.Tests.PushBackCompositeSymmetryTests` | 10 / 0 / 0 | 10 / 0 / 0 |
| `RackCad.Tests.PushBackCompositeTopeTests` | 14 / 0 / 0 | 14 / 0 / 0 |
| `RackCad.Tests.PushBackCompositeViewTests` | 20 / 0 / 0 | 20 / 0 / 0 |
| `RackCad.Tests.PushBackCoreTests` | 21 / 0 / 0 | 21 / 0 / 0 |
| `RackCad.Tests.PushBackCorrectionTests` | 4 / 0 / 0 | 4 / 0 / 0 |
| `RackCad.Tests.PushBackCorridaAnchorTests` | 15 / 0 / 0 | 15 / 0 / 0 |
| `RackCad.Tests.PushBackCorridaDualValidityTests` | 11 / 0 / 0 | 11 / 0 / 0 |
| `RackCad.Tests.PushBackCorridaSpanStorageTests` | 14 / 0 / 0 | 14 / 0 / 0 |
| `RackCad.Tests.PushBackCutSupportTests` | 23 / 0 / 0 | 23 / 0 / 0 |
| `RackCad.Tests.PushBackDefensePerPostTests` | 15 / 0 / 0 | 15 / 0 / 0 |
| `RackCad.Tests.PushBackDefensePlanTests` | 14 / 0 / 0 | 14 / 0 / 0 |
| `RackCad.Tests.PushBackDefenseResizeTests` | 5 / 0 / 0 | 5 / 0 / 0 |
| `RackCad.Tests.PushBackDefenseSideIdentityTests` | 15 / 0 / 0 | 15 / 0 / 0 |
| `RackCad.Tests.PushBackDerivedPostAndLineTests` | 21 / 0 / 0 | 21 / 0 / 0 |
| `RackCad.Tests.PushBackDesviadorCellKeyTests` | 7 / 0 / 0 | 7 / 0 / 0 |
| `RackCad.Tests.PushBackDimensionLegacyCharacterizationTests` | 8 / 0 / 0 | 8 / 0 / 0 |
| `RackCad.Tests.PushBackDimensionViewsTests` | 47 / 0 / 0 | 47 / 0 / 0 |
| `RackCad.Tests.PushBackDiverterLowAuthorityTests` | 13 / 0 / 0 | 13 / 0 / 0 |
| `RackCad.Tests.PushBackDiverterPlanTests` | 28 / 0 / 0 | 28 / 0 / 0 |
| `RackCad.Tests.PushBackDormantSideBFirstLevelTests` | 7 / 0 / 0 | 7 / 0 / 0 |
| `RackCad.Tests.PushBackDraftPortSharedReferenceGuardTests` | 1 / 0 / 0 | 1 / 0 / 0 |
| `RackCad.Tests.PushBackDraftStatePortTests` | 3 / 0 / 0 | 3 / 0 / 0 |
| `RackCad.Tests.PushBackDraftWriteGuardTests` | 2 / 0 / 1 | 2 / 0 / 1 |
| `RackCad.Tests.PushBackEditAllFullCopyGuardTests` | 1 / 0 / 0 | 1 / 0 / 0 |
| `RackCad.Tests.PushBackEditAllNameWritersGuardTests` | 1 / 0 / 0 | 1 / 0 / 0 |
| `RackCad.Tests.PushBackEditAllPortEquivalenceGuardTests` | 1 / 0 / 0 | 1 / 0 / 0 |
| `RackCad.Tests.PushBackEditableStateCopyTests` | 7 / 0 / 0 | 7 / 0 / 0 |
| `RackCad.Tests.PushBackEditableStateLoadTests` | 5 / 0 / 0 | 5 / 0 / 0 |
| `RackCad.Tests.PushBackEditableStateOwnershipTests` | 1 / 0 / 0 | 1 / 0 / 0 |
| `RackCad.Tests.PushBackEditorCorrectionTests` | 17 / 0 / 0 | 17 / 0 / 0 |
| `RackCad.Tests.PushBackEditorDefaultsTests` | 5 / 0 / 0 | 5 / 0 / 0 |
| `RackCad.Tests.PushBackEditorOutputGateTests` | 1 / 0 / 0 | 1 / 0 / 0 |
| `RackCad.Tests.PushBackEditorStateTests` | 27 / 0 / 0 | 27 / 0 / 0 |
| `RackCad.Tests.PushBackElevationAuthorityTests` | 19 / 0 / 0 | 19 / 0 / 0 |
| `RackCad.Tests.PushBackElevationConsumerTests` | 14 / 0 / 0 | 14 / 0 / 0 |
| `RackCad.Tests.PushBackEndBeamIdentityTests` | 9 / 0 / 0 | 9 / 0 / 0 |
| `RackCad.Tests.PushBackEndToEndChainTests` | 2 / 0 / 0 | 2 / 0 / 0 |
| `RackCad.Tests.PushBackFirstLevelDatumTransportTests` | 29 / 0 / 0 | 29 / 0 / 0 |
| `RackCad.Tests.PushBackForkliftDefenseTests` | 12 / 0 / 0 | 12 / 0 / 0 |
| `RackCad.Tests.PushBackFrontalHeaderOwnershipTests` | 9 / 0 / 0 | 9 / 0 / 0 |
| `RackCad.Tests.PushBackFrontalMatchesLateralTests` | 16 / 0 / 0 | 16 / 0 / 0 |
| `RackCad.Tests.PushBackFrontalRearCutTests` | 7 / 0 / 0 | 7 / 0 / 0 |
| `RackCad.Tests.PushBackFullDraftClassifierTests` | 2 / 0 / 0 | 2 / 0 / 0 |
| `RackCad.Tests.PushBackFullDraftCompositionTests` | 3 / 0 / 0 | 3 / 0 / 0 |
| `RackCad.Tests.PushBackFullDraftLoadTests` | 4 / 0 / 0 | 4 / 0 / 0 |
| `RackCad.Tests.PushBackFullDraftViewTests` | 1 / 0 / 0 | 1 / 0 / 0 |
| `RackCad.Tests.PushBackFullEditorCharacterizationDimensionStyleLossTests` | 1 / 0 / 0 | 1 / 0 / 0 |
| `RackCad.Tests.PushBackFullEditorCharacterizationLateralCutTests` | 1 / 0 / 0 | 1 / 0 / 0 |
| `RackCad.Tests.PushBackFullEditorCharacterizationRoundingEffectTests` | 1 / 0 / 0 | 1 / 0 / 0 |
| `RackCad.Tests.PushBackFullPanelCensusGuardTests` | 1 / 0 / 0 | 1 / 0 / 0 |
| `RackCad.Tests.PushBackFullPanelGuardTests` | 4 / 0 / 0 | 4 / 0 / 0 |
| `RackCad.Tests.PushBackFullPanelTests` | 11 / 0 / 0 | 11 / 0 / 0 |
| `RackCad.Tests.PushBackFullReadGuardTests` | 1 / 0 / 2 | 1 / 0 / 2 |
| `RackCad.Tests.PushBackFullReadPathTests` | 4 / 0 / 1 | 4 / 0 / 1 |
| `RackCad.Tests.PushBackFullWritePrepareTests` | 9 / 0 / 0 | 9 / 0 / 0 |
| `RackCad.Tests.PushBackFullWriteRunTests` | 4 / 0 / 0 | 4 / 0 / 0 |
| `RackCad.Tests.PushBackGeneralLateralActiveEnvelopeTests` | 4 / 0 / 0 | 4 / 0 / 0 |
| `RackCad.Tests.PushBackGeneralLateralHeaderZoneTests` | 9 / 0 / 0 | 9 / 0 / 0 |
| `RackCad.Tests.PushBackGoldenTests` | 1 / 0 / 0 | 1 / 0 / 0 |
| `RackCad.Tests.PushBackHeaderApplyTests` | 30 / 0 / 0 | 30 / 0 / 0 |
| `RackCad.Tests.PushBackHeaderDepthHeightTests` | 9 / 0 / 0 | 9 / 0 / 0 |
| `RackCad.Tests.PushBackHeaderEnvelopeTests` | 10 / 0 / 0 | 10 / 0 / 0 |
| `RackCad.Tests.PushBackHeaderHeightAuthorityTests` | 32 / 0 / 0 | 32 / 0 / 0 |
| `RackCad.Tests.PushBackHeaderHeightTests` | 3 / 0 / 0 | 3 / 0 / 0 |
| `RackCad.Tests.PushBackHighEndHandTests` | 15 / 0 / 0 | 15 / 0 / 0 |
| `RackCad.Tests.PushBackHighTieBreakTests` | 16 / 0 / 0 | 16 / 0 / 0 |
| `RackCad.Tests.PushBackIntermediateBeamPeralteAuthorityTests` | 6 / 0 / 0 | 6 / 0 / 0 |
| `RackCad.Tests.PushBackIntermediateBeamPeralteFixTests` | 4 / 0 / 0 | 4 / 0 / 0 |
| `RackCad.Tests.PushBackIntermediateBeamPeralteTests` | 12 / 0 / 0 | 12 / 0 / 0 |
| `RackCad.Tests.PushBackLateralGuardDefaultTests` | 11 / 0 / 0 | 11 / 0 / 0 |
| `RackCad.Tests.PushBackLateralProtectorEndsTests` | 7 / 0 / 0 | 7 / 0 / 0 |
| `RackCad.Tests.PushBackLengthCoherenceTests` | 2 / 0 / 0 | 2 / 0 / 0 |
| `RackCad.Tests.PushBackModuleAdoptionTests` | 20 / 0 / 0 | 20 / 0 / 0 |
| `RackCad.Tests.PushBackModuleEditorCharacterizationTests` | 15 / 0 / 0 | 15 / 0 / 0 |
| `RackCad.Tests.PushBackNoParrillaPersistenceTests` | 1 / 0 / 0 | 1 / 0 / 0 |
| `RackCad.Tests.PushBackOutputGateTests` | 8 / 0 / 0 | 8 / 0 / 0 |
| `RackCad.Tests.PushBackOwnerRejectionRound1Tests` | 10 / 0 / 0 | 10 / 0 / 0 |
| `RackCad.Tests.PushBackPalletProjectionTests` | 17 / 0 / 0 | 17 / 0 / 0 |
| `RackCad.Tests.PushBackPanelDraftGuardTests` | 1 / 0 / 0 | 1 / 0 / 0 |
| `RackCad.Tests.PushBackPartialCompositeTests` | 14 / 0 / 0 | 14 / 0 / 0 |
| `RackCad.Tests.PushBackPhysicalBomAndSafetyTests` | 26 / 0 / 0 | 26 / 0 / 0 |
| `RackCad.Tests.PushBackPhysicalHeaderHeightTests` | 19 / 0 / 0 | 19 / 0 / 0 |
| `RackCad.Tests.PushBackPilotUpdateEquivalenceGuardTests` | 6 / 0 / 0 | 6 / 0 / 0 |
| `RackCad.Tests.PushBackPlanTests` | 9 / 0 / 0 | 9 / 0 / 0 |
| `RackCad.Tests.PushBackPlantBedBeamIdentityTests` | 8 / 0 / 0 | 8 / 0 / 0 |
| `RackCad.Tests.PushBackPluginSourceGuardTests` | 11 / 0 / 0 | 11 / 0 / 0 |
| `RackCad.Tests.PushBackPreviewSafetyParityTests` | 9 / 0 / 0 | 9 / 0 / 0 |
| `RackCad.Tests.PushBackRearTopeAnchorTests` | 7 / 0 / 0 | 7 / 0 / 0 |
| `RackCad.Tests.PushBackRearTopeContactPointTests` | 9 / 0 / 0 | 9 / 0 / 0 |
| `RackCad.Tests.PushBackRearTopeNoneAndFarEndAutoTests` | 9 / 0 / 0 | 9 / 0 / 0 |
| `RackCad.Tests.PushBackRearTopePieceTests` | 9 / 0 / 0 | 9 / 0 / 0 |
| `RackCad.Tests.PushBackRearTopeRunAuthorityTests` | 27 / 0 / 0 | 27 / 0 / 0 |
| `RackCad.Tests.PushBackRegistrationTests` | 7 / 0 / 0 | 7 / 0 / 0 |
| `RackCad.Tests.PushBackReinforcedPostBeamTests` | 7 / 0 / 0 | 7 / 0 / 0 |
| `RackCad.Tests.PushBackRoundTripSourceGuardTests` | 24 / 0 / 0 | 24 / 0 / 0 |
| `RackCad.Tests.PushBackRunFrameTests` | 51 / 0 / 0 | 51 / 0 / 0 |
| `RackCad.Tests.PushBackSafetyBomTests` | 5 / 0 / 0 | 5 / 0 / 0 |
| `RackCad.Tests.PushBackSafetyPerPostTests` | 10 / 0 / 0 | 10 / 0 / 0 |
| `RackCad.Tests.PushBackSharedCellGridTests` | 12 / 0 / 0 | 12 / 0 / 0 |
| `RackCad.Tests.PushBackSharedGridCellDemandTests` | 10 / 0 / 0 | 10 / 0 / 0 |
| `RackCad.Tests.PushBackSharedReferenceGuardTests` | 1 / 0 / 1 | 1 / 0 / 1 |
| `RackCad.Tests.PushBackSharedTransverseGridTests` | 10 / 0 / 0 | 10 / 0 / 0 |
| `RackCad.Tests.PushBackSideIndependentHeightTests` | 35 / 0 / 0 | 35 / 0 / 0 |
| `RackCad.Tests.PushBackTarimaPlacementTests` | 10 / 0 / 0 | 10 / 0 / 0 |
| `RackCad.Tests.PushBackTopeAnchorPerViewTests` | 7 / 0 / 0 | 7 / 0 / 0 |
| `RackCad.Tests.PushBackTopeOriginMateTests` | 7 / 0 / 0 | 7 / 0 / 0 |
| `RackCad.Tests.PushBackTopePlantaAndDiverterTests` | 18 / 0 / 0 | 18 / 0 / 0 |
| `RackCad.Tests.PushBackTopeSurfaceTests` | 14 / 0 / 0 | 14 / 0 / 0 |
| `RackCad.Tests.PushBackVerticalAuthorityTests` | 19 / 0 / 0 | 19 / 0 / 0 |
| `RackCad.Tests.PushBackViewEraseGuardTests` | 4 / 0 / 0 | 4 / 0 / 0 |
| `RackCad.Tests.PushBackViewHygieneTests` | 30 / 0 / 0 | 30 / 0 / 0 |
| `RackCad.Tests.PushBackViewTests` | 6 / 0 / 0 | 6 / 0 / 0 |
| `RackCad.Tests.PushBackWholeLateralEnvelopeTests` | 9 / 0 / 0 | 9 / 0 / 0 |
| `RackCad.Tests.PushBackWindowOnStateSourceGuardTests` | 1 / 0 / 0 | 1 / 0 / 0 |
| `RackCad.Tests.PushBackWindowSharedReferenceGuardTests` | 1 / 0 / 0 | 1 / 0 / 0 |
| `RackCad.Tests.PushBackWorkspaceModeSourceGuardTests` | 1 / 0 / 0 | 1 / 0 / 0 |
| `RackCad.Tests.PushBackWorkspaceSharedReferenceGuardTests` | 1 / 0 / 0 | 1 / 0 / 0 |
| `RackCad.Tests.PushBackWritePathGuardTests` | 1 / 0 / 1 | 1 / 0 / 1 |
| `RackCad.Tests.PushBackWritePathPluginGuardTests` | 1 / 0 / 0 | 1 / 0 / 0 |
| `RackCad.Tests.RackCountInvariantCharacterizationTests` | 3 / 0 / 0 | 3 / 0 / 0 |
| `RackCad.Tests.RackDefinitionCreatorGuardTests` | 26 / 0 / 0 | 26 / 0 / 0 |
| `RackCad.Tests.RackDesignLibraryTests` | 9 / 0 / 0 | 9 / 0 / 0 |
| `RackCad.Tests.RackDuplicationPlanTests` | 31 / 0 / 0 | 31 / 0 / 0 |
| `RackCad.Tests.RackEditPreflightCharacterizationTests` | 5 / 0 / 0 | 5 / 0 / 0 |
| `RackCad.Tests.RackEmbedDocumentTests` | 7 / 0 / 0 | 7 / 0 / 0 |
| `RackCad.Tests.RackEnvelopeIdProbeTests` | 5 / 0 / 0 | 5 / 0 / 0 |
| `RackCad.Tests.RackFirstLevelDatumTests` | 26 / 0 / 0 | 26 / 0 / 0 |
| `RackCad.Tests.RackFrameConfigurationDeepCopyTests` | 11 / 0 / 0 | 11 / 0 / 0 |
| `RackCad.Tests.RackFrameConfigurationFactoryTests` | 11 / 0 / 0 | 11 / 0 / 0 |
| `RackCad.Tests.RackFrameProjectStoreTests` | 11 / 0 / 0 | 11 / 0 / 0 |
| `RackCad.Tests.RackFrameTemplateProviderTests` | 5 / 0 / 0 | 5 / 0 / 0 |
| `RackCad.Tests.RackGroupFrameTests` | 7 / 0 / 0 | 7 / 0 / 0 |
| `RackCad.Tests.RackGroupPlacementPlanTests` | 24 / 0 / 0 | 24 / 0 / 0 |
| `RackCad.Tests.RackIdLifecycleTests` | 4 / 0 / 0 | 4 / 0 / 0 |
| `RackCad.Tests.RackLevelElevationsTests` | 15 / 0 / 0 | 15 / 0 / 0 |
| `RackCad.Tests.RackListBuilderTests` | 10 / 0 / 0 | 10 / 0 / 0 |
| `RackCad.Tests.RackLogTests` | 8 / 0 / 0 | 8 / 0 / 0 |
| `RackCad.Tests.RackModuleEditSessionTests` | 25 / 0 / 0 | 25 / 0 / 0 |
| `RackCad.Tests.RackOrthographicPlacementPolicyTests` | 15 / 0 / 0 | 15 / 0 / 0 |
| `RackCad.Tests.RackPlacedGeometryInvariantTests` | 6 / 0 / 0 | 6 / 0 / 0 |
| `RackCad.Tests.RackProductAuthoredGateTests` | 5 / 0 / 0 | 5 / 0 / 0 |
| `RackCad.Tests.RackProductPrepareTests` | 5 / 0 / 0 | 5 / 0 / 0 |
| `RackCad.Tests.RackProjectStorePerKindTests` | 3 / 0 / 0 | 3 / 0 / 0 |
| `RackCad.Tests.RackProjectStoreTests` | 18 / 0 / 0 | 18 / 0 / 0 |
| `RackCad.Tests.RackProjectionCommandGuardTests` | 34 / 0 / 0 | 34 / 0 / 0 |
| `RackCad.Tests.RackProjectionCommandRunTests` | 49 / 0 / 0 | 49 / 0 / 0 |
| `RackCad.Tests.RackProjectionMaterializationRunTests` | 9 / 0 / 0 | 9 / 0 / 0 |
| `RackCad.Tests.RackProjectionRemedyTests` | 10 / 0 / 0 | 10 / 0 / 0 |
| `RackCad.Tests.RackRigidPlacementPolicyTests` | 11 / 0 / 0 | 11 / 0 / 0 |
| `RackCad.Tests.RackSiblingAuthoredGateTests` | 4 / 0 / 0 | 4 / 0 / 0 |
| `RackCad.Tests.RackSiblingCustomPropertiesGateTests` | 2 / 0 / 0 | 2 / 0 / 0 |
| `RackCad.Tests.RackSiblingGateIndependenceGuardTests` | 3 / 0 / 0 | 3 / 0 / 0 |
| `RackCad.Tests.RackSiblingInsertRunTests` | 11 / 0 / 0 | 11 / 0 / 0 |
| `RackCad.Tests.RackSiblingMembershipTests` | 12 / 0 / 0 | 12 / 0 / 0 |
| `RackCad.Tests.RackSiblingRedrawPlanTests` | 2 / 0 / 0 | 2 / 0 / 0 |
| `RackCad.Tests.RackSiblingRedrawRunTests` | 6 / 0 / 0 | 6 / 0 / 0 |
| `RackCad.Tests.RackSiblingRedrawUnitsTests` | 2 / 0 / 0 | 2 / 0 / 0 |
| `RackCad.Tests.RackSingleViewPlacementTests` | 18 / 0 / 0 | 18 / 0 / 0 |
| `RackCad.Tests.RackTransformFactsTests` | 9 / 0 / 0 | 9 / 0 / 0 |
| `RackCad.Tests.RackUnitsGuardSourceTests` | 18 / 0 / 0 | 18 / 0 / 0 |
| `RackCad.Tests.RackViewAuthoredEquivalenceTests` | 2 / 0 / 0 | 2 / 0 / 0 |
| `RackCad.Tests.RackViewBatchDriverGuardTests` | 4 / 0 / 0 | 4 / 0 / 0 |
| `RackCad.Tests.RackViewBatchPlanTests` | 16 / 0 / 0 | 16 / 0 / 0 |
| `RackCad.Tests.RackViewCountInvariantTests` | 3 / 0 / 0 | 3 / 0 / 0 |
| `RackCad.Tests.RackViewExposureTests` | 7 / 0 / 0 | 7 / 0 / 0 |
| `RackCad.Tests.RackViewFrameTests` | 10 / 0 / 0 | 10 / 0 / 0 |
| `RackCad.Tests.RackViewPartialRackContractTests` | 2 / 0 / 0 | 2 / 0 / 0 |
| `RackCad.Tests.RackViewPlacementGuardTests` | 4 / 0 / 0 | 4 / 0 / 0 |
| `RackCad.Tests.RackViewPreparationSystemTests` | 6 / 0 / 0 | 6 / 0 / 0 |
| `RackCad.Tests.RedefinedViewReferenceUndoTouchGuardTests` | 4 / 0 / 0 | 4 / 0 / 0 |
| `RackCad.Tests.RegistryCommitAccreditationTests` | 12 / 0 / 0 | 12 / 0 / 0 |
| `RackCad.Tests.SafetySelectionDocumentsTests` | 10 / 0 / 0 | 10 / 0 / 0 |
| `RackCad.Tests.SeccionesCatalogTests` | 3 / 0 / 0 | 3 / 0 / 0 |
| `RackCad.Tests.SelectiveAuthoredBindingPresenceTests` | 15 / 0 / 0 | 15 / 0 / 0 |
| `RackCad.Tests.SelectiveAuthoredBindingTests` | 21 / 0 / 0 | 21 / 0 / 0 |
| `RackCad.Tests.SelectiveAuthoredCarrierAdoptionTests` | 12 / 0 / 0 | 12 / 0 / 0 |
| `RackCad.Tests.SelectiveBindingIntentTests` | 22 / 0 / 0 | 22 / 0 / 0 |
| `RackCad.Tests.SelectiveBomAuthorityTests` | 31 / 0 / 0 | 31 / 0 / 0 |
| `RackCad.Tests.SelectiveBomBuilderTests` | 7 / 0 / 0 | 7 / 0 / 0 |
| `RackCad.Tests.SelectiveCabeceraHeightReviewTests` | 9 / 0 / 0 | 9 / 0 / 0 |
| `RackCad.Tests.SelectiveCabeceraViewPurityTests` | 10 / 0 / 0 | 10 / 0 / 0 |
| `RackCad.Tests.SelectiveDepthTests` | 11 / 0 / 0 | 11 / 0 / 0 |
| `RackCad.Tests.SelectiveDesviadorTests` | 30 / 0 / 0 | 30 / 0 / 0 |
| `RackCad.Tests.SelectiveDimensionLegacyCharacterizationTests` | 6 / 0 / 0 | 6 / 0 / 0 |
| `RackCad.Tests.SelectiveDimensionViewsAuthorityTests` | 12 / 0 / 0 | 12 / 0 / 0 |
| `RackCad.Tests.SelectiveDimensionViewsDocumentTests` | 10 / 0 / 0 | 10 / 0 / 0 |
| `RackCad.Tests.SelectiveDimensionViewsEndToEndTests` | 18 / 0 / 0 | 18 / 0 / 0 |
| `RackCad.Tests.SelectiveDimensionViewsTests` | 139 / 0 / 0 | 139 / 0 / 0 |
| `RackCad.Tests.SelectiveDimensionsTests` | 13 / 0 / 0 | 13 / 0 / 0 |
| `RackCad.Tests.SelectiveDuplicationFailClosedTests` | 24 / 0 / 0 | 24 / 0 / 0 |
| `RackCad.Tests.SelectiveEditorOpenTests` | 33 / 0 / 0 | 33 / 0 / 0 |
| `RackCad.Tests.SelectiveEditorStateTests` | 29 / 0 / 0 | 29 / 0 / 0 |
| `RackCad.Tests.SelectiveEffectiveDesignResolverTests` | 20 / 0 / 0 | 20 / 0 / 0 |
| `RackCad.Tests.SelectiveFloorBeamRiseMaterializationTests` | 9 / 0 / 0 | 9 / 0 / 0 |
| `RackCad.Tests.SelectiveFloorBeamRiseTests` | 24 / 0 / 0 | 24 / 0 / 0 |
| `RackCad.Tests.SelectiveFondoCabeceraTests` | 33 / 0 / 0 | 33 / 0 / 0 |
| `RackCad.Tests.SelectiveFrontalBuilderTests` | 25 / 0 / 0 | 25 / 0 / 0 |
| `RackCad.Tests.SelectiveGate8ATests` | 21 / 0 / 0 | 21 / 0 / 0 |
| `RackCad.Tests.SelectiveGeometryResolverTests` | 22 / 0 / 0 | 22 / 0 / 0 |
| `RackCad.Tests.SelectiveHeaderBatchMutateTests` | 17 / 0 / 0 | 17 / 0 / 0 |
| `RackCad.Tests.SelectiveHeaderBatchNormalizationTests` | 8 / 0 / 0 | 8 / 0 / 0 |
| `RackCad.Tests.SelectiveHeaderBatchPersistenceTests` | 4 / 0 / 0 | 4 / 0 / 0 |
| `RackCad.Tests.SelectiveHeaderBatchPrepareTests` | 15 / 0 / 0 | 15 / 0 / 0 |
| `RackCad.Tests.SelectiveLateralBuilderTests` | 13 / 0 / 0 | 13 / 0 / 0 |
| `RackCad.Tests.SelectiveLibraryExportTests` | 21 / 0 / 0 | 21 / 0 / 0 |
| `RackCad.Tests.SelectiveMedioFrenteTests` | 14 / 0 / 0 | 14 / 0 / 0 |
| `RackCad.Tests.SelectiveMultiSelectionTests` | 43 / 0 / 0 | 43 / 0 / 0 |
| `RackCad.Tests.SelectivePalletDesignDocumentTests` | 13 / 0 / 0 | 13 / 0 / 0 |
| `RackCad.Tests.SelectiveParrillaPlacementTests` | 3 / 0 / 0 | 3 / 0 / 0 |
| `RackCad.Tests.SelectivePerFondoTests` | 30 / 0 / 0 | 30 / 0 / 0 |
| `RackCad.Tests.SelectivePlantaPlanTests` | 6 / 0 / 0 | 6 / 0 / 0 |
| `RackCad.Tests.SelectivePostGeometryTests` | 3 / 0 / 0 | 3 / 0 / 0 |
| `RackCad.Tests.SelectivePostTargetsTests` | 8 / 0 / 0 | 8 / 0 / 0 |
| `RackCad.Tests.SelectiveSafetyConfigTests` | 12 / 0 / 0 | 12 / 0 / 0 |
| `RackCad.Tests.SelectiveSafetyEquivalenceTests` | 7 / 0 / 0 | 7 / 0 / 0 |
| `RackCad.Tests.SelectiveSafetyGridTests` | 5 / 0 / 0 | 5 / 0 / 0 |
| `RackCad.Tests.SelectiveSafetyTests` | 49 / 0 / 0 | 49 / 0 / 0 |
| `RackCad.Tests.SelectiveSeparadorPlacementTests` | 1 / 0 / 0 | 1 / 0 / 0 |
| `RackCad.Tests.SelectiveTargetFondosIntegrationTests` | 13 / 0 / 0 | 13 / 0 / 0 |
| `RackCad.Tests.SelectiveTargetPreferenceTests` | 17 / 0 / 0 | 17 / 0 / 0 |
| `RackCad.Tests.SelectiveTargetResolverTests` | 34 / 0 / 0 | 34 / 0 / 0 |
| `RackCad.Tests.SelectiveTarimaPlacementTests` | 3 / 0 / 0 | 3 / 0 / 0 |
| `RackCad.Tests.SelectiveTopeFamilyIsolationGuardTests` | 3 / 0 / 0 | 3 / 0 / 0 |
| `RackCad.Tests.SelectiveTopeMultiFondoCharacterizationTests` | 46 / 0 / 0 | 46 / 0 / 0 |
| `RackCad.Tests.SelectiveTopePlacementTests` | 4 / 0 / 0 | 4 / 0 / 0 |
| `RackCad.Tests.SelectiveTopePlanFrontalTests` | 5 / 0 / 0 | 5 / 0 / 0 |
| `RackCad.Tests.SelectiveTopeSideMultiFondoContractTests` | 32 / 0 / 0 | 32 / 0 / 0 |
| `RackCad.Tests.SelectiveTopeSidePersistenceTests` | 19 / 0 / 0 | 19 / 0 / 0 |
| `RackCad.Tests.SelectiveTopeSideTests` | 20 / 0 / 0 | 20 / 0 / 0 |
| `RackCad.Tests.SelectiveTwentyBaysEquivalenceTests` | 4 / 0 / 0 | 4 / 0 / 0 |
| `RackCad.Tests.SeparatorLevelCalculatorTests` | 14 / 0 / 0 | 14 / 0 / 0 |
| `RackCad.Tests.SharedPhysicalFactsTests` | 5 / 0 / 0 | 5 / 0 / 0 |
| `RackCad.Tests.SharedViewCodecTests` | 4 / 0 / 0 | 4 / 0 / 0 |
| `RackCad.Tests.SharedViewFoundationCt04V2CharacterizationTests` | 7 / 0 / 0 | 7 / 0 / 0 |
| `RackCad.Tests.SharedViewFoundationF1CharacterizationTests` | 26 / 0 / 0 | 26 / 0 / 0 |
| `RackCad.Tests.SharedViewFoundationF5Tests` | 20 / 0 / 0 | 20 / 0 / 0 |
| `RackCad.Tests.SharedViewFoundationF6Tests` | 18 / 0 / 0 | 18 / 0 / 0 |
| `RackCad.Tests.ShippedCatalogIntegrityTests` | 5 / 0 / 0 | 5 / 0 / 0 |
| `RackCad.Tests.ShippedStructuralSectionCatalogTests` | 29 / 0 / 0 | 29 / 0 / 0 |
| `RackCad.Tests.SiblingRedrawSeamGuardTests` | 2 / 0 / 0 | 2 / 0 / 0 |
| `RackCad.Tests.SingleScanInsertGuardTests` | 5 / 0 / 0 | 5 / 0 / 0 |
| `RackCad.Tests.StructuralAnalysisCanonicalizerTests` | 12 / 0 / 0 | 12 / 0 / 0 |
| `RackCad.Tests.StructuralAnalysisCantileverBoundaryTests` | 30 / 0 / 0 | 30 / 0 / 0 |
| `RackCad.Tests.StructuralAnalysisCantileverGoldenTests` | 14 / 0 / 0 | 14 / 0 / 0 |
| `RackCad.Tests.StructuralAnalysisCantileverIdealisationTests` | 36 / 0 / 0 | 36 / 0 / 0 |
| `RackCad.Tests.StructuralAnalysisComparisonHardeningDumpReaderTests` | 14 / 0 / 0 | 14 / 0 / 0 |
| `RackCad.Tests.StructuralAnalysisComparisonHardeningIncompleteTests` | 24 / 0 / 0 | 24 / 0 / 0 |
| `RackCad.Tests.StructuralAnalysisComparisonHardeningMetricTests` | 31 / 0 / 0 | 31 / 0 / 0 |
| `RackCad.Tests.StructuralAnalysisComparisonHardeningReadbackTests` | 68 / 0 / 0 | 68 / 0 / 0 |
| `RackCad.Tests.StructuralAnalysisComparisonHardeningRehearsalTests` | 18 / 0 / 0 | 18 / 0 / 0 |
| `RackCad.Tests.StructuralAnalysisComparisonHardeningToolTests` | 36 / 0 / 0 | 36 / 0 / 0 |
| `RackCad.Tests.StructuralAnalysisComparisonRowTests` | 23 / 0 / 0 | 23 / 0 / 0 |
| `RackCad.Tests.StructuralAnalysisComparisonToolTests` | 28 / 0 / 0 | 28 / 0 / 0 |
| `RackCad.Tests.StructuralAnalysisComparisonVerdictTests` | 30 / 0 / 0 | 30 / 0 / 0 |
| `RackCad.Tests.StructuralAnalysisFingerprintTests` | 10 / 0 / 0 | 10 / 0 / 0 |
| `RackCad.Tests.StructuralAnalysisLoadFileReaderTests` | 51 / 0 / 0 | 51 / 0 / 0 |
| `RackCad.Tests.StructuralAnalysisMaterialProfileTests` | 11 / 0 / 0 | 11 / 0 / 0 |
| `RackCad.Tests.StructuralAnalysisModelBuilderTests` | 23 / 0 / 0 | 23 / 0 / 0 |
| `RackCad.Tests.StructuralAnalysisModelTests` | 10 / 0 / 0 | 10 / 0 / 0 |
| `RackCad.Tests.StructuralAnalysisOutcomeTests` | 12 / 0 / 0 | 12 / 0 / 0 |
| `RackCad.Tests.StructuralAnalysisPluginCommandGuardTests` | 112 / 0 / 0 | 112 / 0 / 0 |
| `RackCad.Tests.StructuralAnalysisPluginCommandProductTableTests` | 4 / 0 / 0 | 4 / 0 / 0 |
| `RackCad.Tests.StructuralAnalysisRamElementsAssumptionsTests` | 5 / 0 / 0 | 5 / 0 / 0 |
| `RackCad.Tests.StructuralAnalysisRamElementsAxesTests` | 96 / 0 / 0 | 96 / 0 / 0 |
| `RackCad.Tests.StructuralAnalysisRamElementsDumpPayloadTests` | 26 / 0 / 0 | 26 / 0 / 0 |
| `RackCad.Tests.StructuralAnalysisRamElementsDumpReaderTests` | 106 / 0 / 0 | 106 / 0 / 0 |
| `RackCad.Tests.StructuralAnalysisRamElementsDumpRoundTripTests` | 6 / 0 / 0 | 6 / 0 / 0 |
| `RackCad.Tests.StructuralAnalysisRamElementsDumpSourceTests` | 23 / 0 / 0 | 23 / 0 / 0 |
| `RackCad.Tests.StructuralAnalysisRamElementsDumpTemplateTests` | 60 / 0 / 0 | 60 / 0 / 0 |
| `RackCad.Tests.StructuralAnalysisRamElementsDumpTransferReaderTests` | 73 / 0 / 0 | 73 / 0 / 0 |
| `RackCad.Tests.StructuralAnalysisRamElementsGuardTests` | 24 / 0 / 0 | 24 / 0 / 0 |
| `RackCad.Tests.StructuralAnalysisRamElementsHardeningGateTests` | 46 / 0 / 0 | 46 / 0 / 0 |
| `RackCad.Tests.StructuralAnalysisRamElementsHardeningGuardTests` | 28 / 0 / 0 | 28 / 0 / 0 |
| `RackCad.Tests.StructuralAnalysisRamElementsHardeningPackageTests` | 45 / 0 / 0 | 45 / 0 / 0 |
| `RackCad.Tests.StructuralAnalysisRamElementsHardeningRaceTests` | 6 / 0 / 0 | 6 / 0 / 0 |
| `RackCad.Tests.StructuralAnalysisRamElementsHardeningReadBackTests` | 29 / 0 / 0 | 29 / 0 / 0 |
| `RackCad.Tests.StructuralAnalysisRamElementsMapReaderTests` | 111 / 0 / 0 | 111 / 0 / 0 |
| `RackCad.Tests.StructuralAnalysisRamElementsPackageContentTests` | 14 / 0 / 0 | 14 / 0 / 0 |
| `RackCad.Tests.StructuralAnalysisRamElementsPackageGoldenTests` | 5 / 0 / 0 | 5 / 0 / 0 |
| `RackCad.Tests.StructuralAnalysisRamElementsPackageManifestTests` | 31 / 0 / 0 | 31 / 0 / 0 |
| `RackCad.Tests.StructuralAnalysisRamElementsPackagePayloadTests` | 26 / 0 / 0 | 26 / 0 / 0 |
| `RackCad.Tests.StructuralAnalysisRamElementsPackageTemplateTests` | 51 / 0 / 0 | 51 / 0 / 0 |
| `RackCad.Tests.StructuralAnalysisRamElementsPackageValidationTests` | 63 / 0 / 0 | 63 / 0 / 0 |
| `RackCad.Tests.StructuralAnalysisRamElementsPackageWriterTests` | 51 / 0 / 0 | 51 / 0 / 0 |
| `RackCad.Tests.StructuralAnalysisRamElementsProjectionTests` | 28 / 0 / 0 | 28 / 0 / 0 |
| `RackCad.Tests.StructuralAnalysisRamElementsTranslationTests` | 33 / 0 / 0 | 33 / 0 / 0 |
| `RackCad.Tests.StructuralAnalysisSectionMapVerifiedTests` | 6 / 0 / 0 | 6 / 0 / 0 |
| `RackCad.Tests.StructuralAnalysisSourceEntryHardeningTests` | 37 / 0 / 0 | 37 / 0 / 0 |
| `RackCad.Tests.StructuralAnalysisSourceEntryTests` | 29 / 0 / 0 | 29 / 0 / 0 |
| `RackCad.Tests.StructuralAnalysisSourceGuardTests` | 86 / 0 / 0 | 86 / 0 / 0 |
| `RackCad.Tests.StructuralAnalysisUnitTests` | 56 / 0 / 0 | 56 / 0 / 0 |
| `RackCad.Tests.StructuralSectionGeometrySentinelTests` | 12 / 0 / 0 | 12 / 0 / 0 |
| `RackCad.Tests.StructuralSectionGeometryTests` | 18 / 0 / 0 | 18 / 0 / 0 |
| `RackCad.Tests.StructuralSectionIdNamespaceTests` | 21 / 0 / 0 | 21 / 0 / 0 |
| `RackCad.Tests.StructuralSectionImporterTests` | 20 / 0 / 0 | 20 / 0 / 0 |
| `RackCad.Tests.StructuralSectionLoadValidationTests` | 15 / 0 / 0 | 15 / 0 / 0 |
| `RackCad.Tests.StructuralSectionModelTests` | 37 / 0 / 0 | 37 / 0 / 0 |
| `RackCad.Tests.StructuralSectionPluginSourceGuardTests` | 45 / 0 / 0 | 45 / 0 / 0 |
| `RackCad.Tests.StructuralSectionProjectionTests` | 38 / 0 / 0 | 38 / 0 / 0 |
| `RackCad.Tests.StructuralSectionPublishTransactionTests` | 9 / 0 / 0 | 9 / 0 / 0 |
| `RackCad.Tests.StructuralSectionSFamilyTests` | 21 / 0 / 0 | 21 / 0 / 0 |
| `RackCad.Tests.StructuralSectionSearchTests` | 16 / 0 / 0 | 16 / 0 / 0 |
| `RackCad.Tests.StructuralSectionStrictReaderTests` | 25 / 0 / 0 | 25 / 0 / 0 |
| `RackCad.Tests.StructuralSectionValidatorTests` | 55 / 0 / 0 | 55 / 0 / 0 |
| `RackCad.Tests.StructuralSectionWireframeIntegrityTests` | 47 / 0 / 0 | 47 / 0 / 0 |
| `RackCad.Tests.SystemBomBuilderTests` | 16 / 0 / 0 | 16 / 0 / 0 |
| `RackCad.Tests.SystemKindPersistenceCharacterizationTests` | 31 / 0 / 0 | 31 / 0 / 0 |
| `RackCad.Tests.SystemRegistryTests` | 13 / 0 / 0 | 13 / 0 / 0 |
| `RackCad.Tests.UserSettingsStoreTests` | 4 / 0 / 0 | 4 / 0 / 0 |
| `RackCad.Tests.UserTemplateStoreTests` | 4 / 0 / 0 | 4 / 0 / 0 |
| `RackCad.Tests.UserTemplatesTests` | 5 / 0 / 0 | 5 / 0 / 0 |
| `RackCad.Tests.WarehouseAutoFillTests` | 13 / 0 / 0 | 13 / 0 / 0 |
| `RackCad.Tests.WarehouseFitCheckerTests` | 10 / 0 / 0 | 10 / 0 / 0 |
| `RackCad.Tests.WarehouseGridPlannerTests` | 19 / 0 / 0 | 19 / 0 / 0 |
| `RackCad.Tests.WorkspaceAssociativeBookkeepingTests` | 0 / 0 / 10 | 0 / 0 / 10 |
| `RackCad.Tests.WorkspaceBridgeAuthorizeTests` | 7 / 0 / 0 | 7 / 0 / 0 |
| `RackCad.Tests.WorkspaceBridgeCommandStateTests` | 20 / 0 / 0 | 20 / 0 / 0 |
| `RackCad.Tests.WorkspaceBridgeDegradedModeTests` | 11 / 0 / 0 | 11 / 0 / 0 |
| `RackCad.Tests.WorkspaceBridgeFacadeSourceGuardTests` | 18 / 0 / 0 | 18 / 0 / 0 |
| `RackCad.Tests.WorkspaceBridgeHintAndDrainTests` | 28 / 0 / 0 | 28 / 0 / 0 |
| `RackCad.Tests.WorkspaceBridgeNoActiveDocumentTests` | 11 / 0 / 0 | 11 / 0 / 0 |
| `RackCad.Tests.WorkspaceBridgeObjectSubjectGuardTests` | 20 / 0 / 0 | 20 / 0 / 0 |
| `RackCad.Tests.WorkspaceBridgeReconciliationTests` | 17 / 0 / 0 | 17 / 0 / 0 |
| `RackCad.Tests.WorkspaceBridgeSourceGuardTests` | 52 / 0 / 0 | 52 / 0 / 0 |
| `RackCad.Tests.WorkspaceBridgeSubscriptionLifecycleTests` | 17 / 0 / 0 | 17 / 0 / 0 |
| `RackCad.Tests.WorkspaceBrowserBoundaryTests` | 26 / 0 / 0 | 26 / 0 / 0 |
| `RackCad.Tests.WorkspaceBrowserHostGuardTests` | 19 / 0 / 0 | 19 / 0 / 0 |
| `RackCad.Tests.WorkspaceBrowserSessionRegistryTests` | 12 / 0 / 0 | 12 / 0 / 0 |
| `RackCad.Tests.WorkspaceBrowserSessionTests` | 44 / 0 / 0 | 44 / 0 / 0 |
| `RackCad.Tests.WorkspaceBrowserSummaryTests` | 31 / 0 / 0 | 31 / 0 / 0 |
| `RackCad.Tests.WorkspaceCommandAdvisorGuardTests` | 82 / 0 / 0 | 82 / 0 / 0 |
| `RackCad.Tests.WorkspaceCommandAdvisorTests` | 0 / 0 / 54 | 0 / 0 / 54 |
| `RackCad.Tests.WorkspaceDraftApplyTests` | 61 / 0 / 0 | 61 / 0 / 0 |
| `RackCad.Tests.WorkspaceDraftBoundaryTests` | 152 / 0 / 0 | 152 / 0 / 0 |
| `RackCad.Tests.WorkspaceDraftPilotIntentGuardTests` | 17 / 0 / 0 | 17 / 0 / 0 |
| `RackCad.Tests.WorkspaceDraftPilotIntentReadTests` | 82 / 0 / 0 | 82 / 0 / 0 |
| `RackCad.Tests.WorkspaceDraftPilotIntentStaleBaseTests` | 29 / 0 / 0 | 29 / 0 / 0 |
| `RackCad.Tests.WorkspaceDraftPilotMaterializationTests` | 78 / 0 / 0 | 78 / 0 / 0 |
| `RackCad.Tests.WorkspaceDraftPilotOwnershipTests` | 31 / 0 / 0 | 31 / 0 / 0 |
| `RackCad.Tests.WorkspaceDraftPilotStateTests` | 65 / 0 / 0 | 65 / 0 / 0 |
| `RackCad.Tests.WorkspaceDraftRegistryContainmentTests` | 23 / 0 / 0 | 23 / 0 / 0 |
| `RackCad.Tests.WorkspaceDraftRegistryTests` | 36 / 0 / 0 | 36 / 0 / 0 |
| `RackCad.Tests.WorkspaceDraftSessionTests` | 99 / 0 / 0 | 99 / 0 / 0 |
| `RackCad.Tests.WorkspaceDraftStaleBaseTests` | 50 / 0 / 0 | 50 / 0 / 0 |
| `RackCad.Tests.WorkspaceEditIntentGuardTests` | 145 / 0 / 7 | 145 / 0 / 7 |
| `RackCad.Tests.WorkspaceEditIntentHostTests` | 0 / 0 / 62 | 0 / 0 / 62 |
| `RackCad.Tests.WorkspaceFullCopyGuardTests` | 0 / 0 / 1 | 0 / 0 / 1 |
| `RackCad.Tests.WorkspaceHintDrainTests` | 11 / 0 / 0 | 11 / 0 / 0 |
| `RackCad.Tests.WorkspaceHostAuthorizationTests` | 13 / 0 / 0 | 13 / 0 / 0 |
| `RackCad.Tests.WorkspaceHostC2SourceGuardTests` | 15 / 0 / 0 | 15 / 0 / 0 |
| `RackCad.Tests.WorkspaceHostDrainTests` | 42 / 0 / 0 | 42 / 0 / 0 |
| `RackCad.Tests.WorkspaceHostHardeningGuardTests` | 99 / 0 / 1 | 99 / 0 / 1 |
| `RackCad.Tests.WorkspaceHostLifecycleTests` | 35 / 0 / 0 | 35 / 0 / 0 |
| `RackCad.Tests.WorkspaceHostPolicyTests` | 37 / 0 / 0 | 37 / 0 / 0 |
| `RackCad.Tests.WorkspaceHostPortGuardTests` | 33 / 0 / 0 | 33 / 0 / 0 |
| `RackCad.Tests.WorkspaceHostSourceGuardTests` | 45 / 0 / 0 | 45 / 0 / 0 |
| `RackCad.Tests.WorkspaceIdleSchedulerTests` | 48 / 0 / 0 | 48 / 0 / 0 |
| `RackCad.Tests.WorkspaceIdleSchedulingGuardTests` | 124 / 0 / 0 | 124 / 0 / 0 |
| `RackCad.Tests.WorkspaceInstrumentationTests` | 16 / 0 / 0 | 16 / 0 / 0 |
| `RackCad.Tests.WorkspaceLifecycleGuardTests` | 57 / 0 / 2 | 57 / 0 / 2 |
| `RackCad.Tests.WorkspaceLifecycleHostTests` | 0 / 0 / 69 | 0 / 0 / 69 |
| `RackCad.Tests.WorkspaceModelBoundaryTests` | 19 / 0 / 0 | 19 / 0 / 0 |
| `RackCad.Tests.WorkspaceNavigationActionPolicyTests` | 20 / 0 / 0 | 20 / 0 / 0 |
| `RackCad.Tests.WorkspaceNavigationAuthorityTests` | 10 / 0 / 0 | 10 / 0 / 0 |
| `RackCad.Tests.WorkspaceNavigationCacheTests` | 15 / 0 / 0 | 15 / 0 / 0 |
| `RackCad.Tests.WorkspaceNavigationEchoTests` | 18 / 0 / 0 | 18 / 0 / 0 |
| `RackCad.Tests.WorkspaceNavigationHostTests` | 67 / 0 / 0 | 67 / 0 / 0 |
| `RackCad.Tests.WorkspaceNavigationPrivacyGuardTests` | 11 / 0 / 0 | 11 / 0 / 0 |
| `RackCad.Tests.WorkspaceNavigationTargetsTests` | 23 / 0 / 0 | 23 / 0 / 0 |
| `RackCad.Tests.WorkspaceOwnMutationTests` | 18 / 0 / 0 | 18 / 0 / 0 |
| `RackCad.Tests.WorkspacePaletteHostSourceGuardTests` | 45 / 0 / 0 | 45 / 0 / 0 |
| `RackCad.Tests.WorkspacePilotCloseSyncGuardTests` | 119 / 0 / 0 | 119 / 0 / 0 |
| `RackCad.Tests.WorkspacePilotEditGuardTests` | 121 / 0 / 6 | 121 / 0 / 6 |
| `RackCad.Tests.WorkspacePilotEditHostTests` | 0 / 0 / 59 | 0 / 0 / 59 |
| `RackCad.Tests.WorkspacePilotPrepareGuardTests` | 22 / 0 / 3 | 22 / 0 / 3 |
| `RackCad.Tests.WorkspacePilotPrepareTests` | 0 / 0 / 67 | 0 / 0 / 67 |
| `RackCad.Tests.WorkspacePilotReconciliationGuardTests` | 25 / 0 / 1 | 25 / 0 / 1 |
| `RackCad.Tests.WorkspacePilotReconciliationHostTests` | 0 / 0 / 28 | 0 / 0 / 28 |
| `RackCad.Tests.WorkspacePilotSurfaceGuardTests` | 43 / 0 / 0 | 43 / 0 / 0 |
| `RackCad.Tests.WorkspacePilotUpdateGuardTests` | 85 / 0 / 2 | 85 / 0 / 2 |
| `RackCad.Tests.WorkspacePilotUpdateHostRunTests` | 27 / 0 / 0 | 27 / 0 / 0 |
| `RackCad.Tests.WorkspacePilotUpdateHostTests` | 0 / 0 / 35 | 0 / 0 / 35 |
| `RackCad.Tests.WorkspacePilotUpdateIntegrationGuardTests` | 121 / 0 / 4 | 121 / 0 / 4 |
| `RackCad.Tests.WorkspacePilotUpdateRequestTests` | 0 / 0 / 44 | 0 / 0 / 44 |
| `RackCad.Tests.WorkspacePilotUpdateRunTests` | 57 / 0 / 0 | 57 / 0 / 0 |
| `RackCad.Tests.WorkspaceSelectionContextTests` | 13 / 0 / 0 | 13 / 0 / 0 |
| `RackCad.Tests.WorkspaceSelectionMappingTests` | 27 / 0 / 0 | 27 / 0 / 0 |
| `RackCad.Tests.WorkspaceSessionTests` | 8 / 0 / 0 | 8 / 0 / 0 |
| `RackCad.Tests.WorkspaceSourceGuardExceptionTests` | 88 / 0 / 0 | 88 / 0 / 0 |
| `RackCad.Tests.XlsxWriterTests` | 12 / 0 / 0 | 12 / 0 / 0 |
| `RackCad.UI.Tests.CantileverAdvancedPanelWindowTests` | 10 / 0 / 0 | 10 / 0 / 0 |
| `RackCad.UI.Tests.CantileverComponentEditorTests` | 33 / 0 / 0 | 33 / 0 / 0 |
| `RackCad.UI.Tests.CantileverEditorWindowTests` | 24 / 0 / 0 | 24 / 0 / 0 |
| `RackCad.UI.Tests.FullPanelCopyWorkspacePilotReconciliationGreenTests` | 30 / 0 / 0 | 30 / 0 / 0 |
| `RackCad.UI.Tests.FullPanelCopyWorkspacePilotReconciliationViewGreenTests` | 6 / 0 / 0 | 6 / 0 / 0 |
| `RackCad.UI.Tests.NumericFieldTests` | 10 / 0 / 0 | 10 / 0 / 0 |
| `RackCad.UI.Tests.NumericFieldValidationTests` | 13 / 0 / 0 | 13 / 0 / 0 |
| `RackCad.UI.Tests.PushBackClassicCharacterizationCloseTests` | 21 / 0 / 0 | 21 / 0 / 0 |
| `RackCad.UI.Tests.PushBackClassicCharacterizationIntentTests` | 12 / 0 / 0 | 12 / 0 / 0 |
| `RackCad.UI.Tests.PushBackClassicCharacterizationKeyboardTests` | 4 / 0 / 0 | 4 / 0 / 0 |
| `RackCad.UI.Tests.PushBackClassicCharacterizationOwnerTests` | 4 / 0 / 0 | 4 / 0 / 0 |
| `RackCad.UI.Tests.PushBackClassicCharacterizationResourceTests` | 2 / 0 / 0 | 2 / 0 / 0 |
| `RackCad.UI.Tests.PushBackClassicCharacterizationSourceGuardTests` | 12 / 0 / 0 | 12 / 0 / 0 |
| `RackCad.UI.Tests.PushBackDeclaredCorrection2MixedBoxTests` | 1 / 0 / 0 | 1 / 0 / 0 |
| `RackCad.UI.Tests.PushBackDeclaredCorrection2RoundingTests` | 3 / 0 / 0 | 3 / 0 / 0 |
| `RackCad.UI.Tests.PushBackDeclaredCorrectionDimensionStyleTests` | 2 / 0 / 0 | 2 / 0 / 0 |
| `RackCad.UI.Tests.PushBackDeclaredCorrectionPilotParityTests` | 2 / 0 / 0 | 2 / 0 / 0 |
| `RackCad.UI.Tests.PushBackDeclaredCorrectionRoundingTests` | 3 / 0 / 0 | 3 / 0 / 0 |
| `RackCad.UI.Tests.PushBackDeclaredCorrectionSideBNoticeTests` | 2 / 0 / 0 | 2 / 0 / 0 |
| `RackCad.UI.Tests.PushBackDeclaredCorrectionStandardRestoreTests` | 1 / 0 / 0 | 1 / 0 / 0 |
| `RackCad.UI.Tests.PushBackEditAllApplyCloseTests` | 0 / 1 / 0 | 1 / 0 / 0 |
| `RackCad.UI.Tests.PushBackFieldValidationConcordanceTests` | 1 / 0 / 0 | 1 / 0 / 0 |
| `RackCad.UI.Tests.PushBackFullDraftWindowParityTests` | 1 / 0 / 0 | 1 / 0 / 0 |
| `RackCad.UI.Tests.PushBackFullEditorCharacterizationCatalogTests` | 1 / 0 / 0 | 1 / 0 / 0 |
| `RackCad.UI.Tests.PushBackFullEditorCharacterizationCompositeTests` | 4 / 0 / 0 | 4 / 0 / 0 |
| `RackCad.UI.Tests.PushBackFullEditorCharacterizationModuleTests` | 2 / 0 / 1 | 2 / 0 / 1 |
| `RackCad.UI.Tests.PushBackFullEditorCharacterizationRoundingLossTests` | 1 / 0 / 1 | 1 / 0 / 1 |
| `RackCad.UI.Tests.PushBackFullEditorCharacterizationStateTests` | 3 / 0 / 0 | 3 / 0 / 0 |
| `RackCad.UI.Tests.PushBackFullEditorCharacterizationSubdialogTests` | 5 / 0 / 0 | 5 / 0 / 0 |
| `RackCad.UI.Tests.PushBackFullPanelFieldTests` | 3 / 0 / 0 | 3 / 0 / 0 |
| `RackCad.UI.Tests.PushBackStateNetActionsTests` | 2 / 0 / 0 | 2 / 0 / 0 |
| `RackCad.UI.Tests.PushBackStateNetCellAndFrontTests` | 4 / 0 / 0 | 4 / 0 / 0 |
| `RackCad.UI.Tests.PushBackStateNetCompositeTests` | 3 / 0 / 0 | 3 / 0 / 0 |
| `RackCad.UI.Tests.PushBackStateNetFingerprintTests` | 1 / 0 / 0 | 1 / 0 / 0 |
| `RackCad.UI.Tests.PushBackStateNetModuleAndSafetyTests` | 2 / 0 / 0 | 2 / 0 / 0 |
| `RackCad.UI.Tests.PushBackStateNetRackControlsTests` | 2 / 0 / 0 | 2 / 0 / 0 |
| `RackCad.UI.Tests.PushBackWindowOnStateTests` | 2 / 0 / 0 | 2 / 0 / 0 |
| `RackCad.UI.Tests.PushBackWorkspaceApplyCloseTests` | 3 / 0 / 1 | 3 / 0 / 1 |
| `RackCad.UI.Tests.PushBackWorkspaceCarryoverTests` | 3 / 0 / 0 | 3 / 0 / 0 |
| `RackCad.UI.Tests.PushBackWorkspaceModeTests` | 3 / 0 / 0 | 3 / 0 / 0 |
| `RackCad.UI.Tests.WorkspaceBrowserScenarioHarnessTests` | 5 / 0 / 0 | 5 / 0 / 0 |
| `RackCad.UI.Tests.WorkspaceBrowserViewModelTests` | 49 / 0 / 0 | 49 / 0 / 0 |
| `RackCad.UI.Tests.WorkspaceBrowserViewTests` | 13 / 0 / 0 | 13 / 0 / 0 |
| `RackCad.UI.Tests.WorkspaceDraftPilotCanonicalLineEndingTests` | 9 / 0 / 0 | 9 / 0 / 0 |
| `RackCad.UI.Tests.WorkspaceDraftPilotValidationParityTests` | 9 / 0 / 0 | 9 / 0 / 0 |
| `RackCad.UI.Tests.WorkspaceDraftPilotWindowLoadCharacterizationTests` | 3 / 0 / 1 | 3 / 0 / 1 |
| `RackCad.UI.Tests.WorkspaceNavigationViewModelTests` | 73 / 0 / 0 | 73 / 0 / 0 |
| `RackCad.UI.Tests.WorkspaceNavigationViewTests` | 18 / 0 / 0 | 18 / 0 / 0 |
| `RackCad.UI.Tests.WorkspacePanelContextTests` | 15 / 0 / 0 | 15 / 0 / 0 |
| `RackCad.UI.Tests.WorkspacePanelErrorTests` | 11 / 0 / 0 | 11 / 0 / 0 |
| `RackCad.UI.Tests.WorkspacePanelInstrumentationTests` | 10 / 0 / 0 | 10 / 0 / 0 |
| `RackCad.UI.Tests.WorkspacePanelNoWorkTests` | 8 / 0 / 0 | 8 / 0 / 0 |
| `RackCad.UI.Tests.WorkspacePanelStateTests` | 12 / 0 / 0 | 12 / 0 / 0 |
| `RackCad.UI.Tests.WorkspacePanelViewTests` | 8 / 0 / 0 | 8 / 0 / 0 |
| `RackCad.UI.Tests.WorkspacePilotCloseSyncTests` | 78 / 0 / 0 | 78 / 0 / 0 |
| `RackCad.UI.Tests.WorkspacePilotCloseSyncViewTests` | 17 / 0 / 0 | 17 / 0 / 0 |
| `RackCad.UI.Tests.WorkspacePilotFieldViewModelTests` | 160 / 0 / 0 | 160 / 0 / 0 |
| `RackCad.UI.Tests.WorkspacePilotFieldViewTests` | 43 / 0 / 0 | 43 / 0 / 0 |
| `RackCad.UI.Tests.WorkspacePilotReconciliationTests` | 100 / 0 / 21 | 100 / 0 / 21 |
| `RackCad.UI.Tests.WorkspacePilotReconciliationViewTests` | 3 / 0 / 6 | 3 / 0 / 6 |
| **Total (764 clases)** | **20032 / 1 / 492** | **20033 / 0 / 492** |

TRX leidas (38): RED core-red.trx 46aeb97ba224; RED red-ui-RackCad_UI_Tests_Cantilever.trx 25c9dbbda722; RED red-ui-RackCad_UI_Tests_FullPanelCopyWorkspacePilotReconciliationGreenTests.trx 418e683b0995; RED red-ui-RackCad_UI_Tests_FullPanelCopyWorkspacePilotReconciliationViewGreenTests.trx 2229c9629ccd; RED red-ui-RackCad_UI_Tests_NumericFieldTests.trx b582c7b8ebd8; RED red-ui-RackCad_UI_Tests_NumericFieldValidationTests.trx bb4b85a18bc6; RED red-ui-RackCad_UI_Tests_PushBackClassicCharacterization.trx 07a694e3d366; RED red-ui-RackCad_UI_Tests_PushBackDeclaredCorrection.trx 5a3b78ba7b56; RED red-ui-RackCad_UI_Tests_PushBackEditAllApplyCloseTests.trx 1d2774971942; RED red-ui-RackCad_UI_Tests_PushBackFieldValidationConcordanceTests.trx 53348d3132cc; RED red-ui-RackCad_UI_Tests_PushBackFullDraftWindowParityTests.trx 4294623b555a; RED red-ui-RackCad_UI_Tests_PushBackFullEditorCharacterization.trx 28b8893ec8a0; RED red-ui-RackCad_UI_Tests_PushBackFullPanelFieldTests.trx b54d6f294ddd; RED red-ui-RackCad_UI_Tests_PushBackStateNet.trx fa0b3d7b700c; RED red-ui-RackCad_UI_Tests_PushBackWindowOnStateTests.trx 252a88f8e5df; RED red-ui-RackCad_UI_Tests_PushBackWorkspaceApplyCloseTests.trx c433ae15eaba; RED red-ui-RackCad_UI_Tests_PushBackWorkspaceCarryoverTests.trx 3bb6c382381b; RED red-ui-RackCad_UI_Tests_PushBackWorkspaceModeTests.trx ac26fd9d9516; RED red-ui-RackCad_UI_Tests_Workspace.trx c517ebfdd28d; GREEN core-green.trx c6df1250a69b; GREEN green-ui-RackCad_UI_Tests_Cantilever.trx ac1903bae592; GREEN green-ui-RackCad_UI_Tests_FullPanelCopyWorkspacePilotReconciliationGreenTests.trx a274a481ee70; GREEN green-ui-RackCad_UI_Tests_FullPanelCopyWorkspacePilotReconciliationViewGreenTests.trx d69cc56643d9; GREEN green-ui-RackCad_UI_Tests_NumericFieldTests.trx e1551917af80; GREEN green-ui-RackCad_UI_Tests_NumericFieldValidationTests.trx 7499b844d3da; GREEN green-ui-RackCad_UI_Tests_PushBackClassicCharacterization.trx 1cf0843555e3; GREEN green-ui-RackCad_UI_Tests_PushBackDeclaredCorrection.trx 66f283a20705; GREEN green-ui-RackCad_UI_Tests_PushBackEditAllApplyCloseTests.trx 474beeb0a2ae; GREEN green-ui-RackCad_UI_Tests_PushBackFieldValidationConcordanceTests.trx 76b6eb9a46de; GREEN green-ui-RackCad_UI_Tests_PushBackFullDraftWindowParityTests.trx 2f29947c3ef2; GREEN green-ui-RackCad_UI_Tests_PushBackFullEditorCharacterization.trx e578979f4242; GREEN green-ui-RackCad_UI_Tests_PushBackFullPanelFieldTests.trx 2dd37044dc61; GREEN green-ui-RackCad_UI_Tests_PushBackStateNet.trx ceb878ecfe9a; GREEN green-ui-RackCad_UI_Tests_PushBackWindowOnStateTests.trx 29d5ae486ff1; GREEN green-ui-RackCad_UI_Tests_PushBackWorkspaceApplyCloseTests.trx 2a76d8503382; GREEN green-ui-RackCad_UI_Tests_PushBackWorkspaceCarryoverTests.trx 737ff0da861e; GREEN green-ui-RackCad_UI_Tests_PushBackWorkspaceModeTests.trx b44b3eb178a7; GREEN green-ui-RackCad_UI_Tests_Workspace.trx eb745ff31272

