# I-23 — Inventario de namespaces por sistema (corregido)

> Regenerado sobre el arbol YA corregido. Cubre **los seis proyectos**: Domain, Application
> (incluida Persistence), UI, Plugin y los **dos** proyectos de pruebas. Sustituye al inventario
> de la primera ronda, que solo cubria los archivos movidos y omitia UI y pruebas.


Columnas: **Tipo** (de primer nivel), **Propietario** (el sistema que lo posee, o `transversal`),
**Destino** (el namespace en que vive hoy) y **Consumido por** (proyectos que lo nombran, medido
por referencia textual en `src` y `tests`, excluyendo su propio archivo).


Consumir un contrato de otro sistema **no** cambia al propietario: componer entre sistemas es
legal (ROADMAP, principio 5).


## Domain — 40 archivos, 65 tipos


### `RackCad.Domain.RackFrames`

| Tipo | Propietario | Destino | Consumido por |
|---|---|---|---|
| `BasePlatePlacement` | RackFrames | `RackCad.Domain.RackFrames` | Application, Domain, Tests, UI |
| `BracingDiagonalGeometry` | RackFrames | `RackCad.Domain.RackFrames` | Application, Tests |
| `DoubleDiagonalElevations` | RackFrames | `RackCad.Domain.RackFrames` | _(sin consumidores)_ |
| `BracingPanel` | RackFrames | `RackCad.Domain.RackFrames` | Application, Domain, Tests, UI |
| `BracingPattern` | RackFrames | `RackCad.Domain.RackFrames` | Application, Domain, Tests, UI, UI.Tests |
| `DiagonalDirection` | RackFrames | `RackCad.Domain.RackFrames` | Application, Domain, Tests, UI |
| `ExceptionType` | RackFrames | `RackCad.Domain.RackFrames` | Application, Domain, Tests, UI |
| `FrameComponentState` | RackFrames | `RackCad.Domain.RackFrames` | Application, Domain, Tests, UI |
| `FrameExceptionOverride` | RackFrames | `RackCad.Domain.RackFrames` | Application, Domain, Tests, UI |
| `FrameHorizontal` | RackFrames | `RackCad.Domain.RackFrames` | Application, Domain, Tests, UI |
| `FrameMember` | RackFrames | `RackCad.Domain.RackFrames` | Application, Domain, Tests, UI |
| `FrameMemberEnd` | RackFrames | `RackCad.Domain.RackFrames` | Application, Domain, Tests, UI |
| `FrameMemberEndRole` | RackFrames | `RackCad.Domain.RackFrames` | Application, Domain |
| `FrameMemberOrigin` | RackFrames | `RackCad.Domain.RackFrames` | Application, Domain |
| `FrameMemberType` | RackFrames | `RackCad.Domain.RackFrames` | Application, Domain, Tests, UI |
| `FrameSide` | RackFrames | `RackCad.Domain.RackFrames` | Application, Domain, Tests, UI |
| `PostAssembly` | RackFrames | `RackCad.Domain.RackFrames` | Application, Domain, Tests, UI |
| `PostSide` | RackFrames | `RackCad.Domain.RackFrames` | Application, Domain, Tests |
| `RackFrameConfiguration` | RackFrames | `RackCad.Domain.RackFrames` | Application, Domain, Plugin, Tests, UI, UI.Tests |

### `RackCad.Domain.Systems.Dynamic`

| Tipo | Propietario | Destino | Consumido por |
|---|---|---|---|
| `DynamicLoadBeamLevel` | Dynamic | `RackCad.Domain.Systems.Dynamic` | Application, Domain, Tests, UI |
| `DynamicRackDefaults` | Dynamic | `RackCad.Domain.Systems.Dynamic` | Application, Domain, Plugin, Tests, UI, UI.Tests |
| `DynamicRackDesign` | Dynamic | `RackCad.Domain.Systems.Dynamic` | Application, Domain, Plugin, Tests, UI, UI.Tests |
| `DynamicRackModuleDesign` | Dynamic | `RackCad.Domain.Systems.Dynamic` | Application, Tests |
| `DynamicRackFrontDesign` | Dynamic | `RackCad.Domain.Systems.Dynamic` | Application, Domain, Tests, UI, UI.Tests |
| `DynamicRackFront` | Dynamic | `RackCad.Domain.Systems.Dynamic` | Application, Domain, Tests, UI |
| `DynamicRackLevelDesign` | Dynamic | `RackCad.Domain.Systems.Dynamic` | Application, Tests |
| `DynamicRackLevel` | Dynamic | `RackCad.Domain.Systems.Dynamic` | Application, Tests |
| `DynamicRackEnd` | Dynamic | `RackCad.Domain.Systems.Dynamic` | Application, Plugin, Tests, UI, UI.Tests |
| `DynamicRackModule` | Dynamic | `RackCad.Domain.Systems.Dynamic` | Application, Domain, Tests, UI |
| `DynamicRackModuleKind` | Dynamic | `RackCad.Domain.Systems.Dynamic` | Application, Domain, Tests, UI |
| `DynamicRackSystem` | Dynamic | `RackCad.Domain.Systems.Dynamic` | Application, Domain, Plugin, Tests, UI, UI.Tests |

### `RackCad.Domain.Systems.FlowBed`

| Tipo | Propietario | Destino | Consumido por |
|---|---|---|---|
| `FlowBedConfiguration` | FlowBed | `RackCad.Domain.Systems.FlowBed` | Application, Plugin, Tests, UI, UI.Tests |
| `FlowBedDefaults` | FlowBed | `RackCad.Domain.Systems.FlowBed` | Application, Domain, Plugin, Tests, UI, UI.Tests |
| `FlowBedType` | FlowBed | `RackCad.Domain.Systems.FlowBed` | Application, Domain, Plugin, Tests, UI, UI.Tests |

### `RackCad.Domain.Systems.Larguero`

| Tipo | Propietario | Destino | Consumido por |
|---|---|---|---|
| `LargueroDesign` | Larguero | `RackCad.Domain.Systems.Larguero` | Application, Tests, UI, UI.Tests |

### `RackCad.Domain.Systems.PushBack`

| Tipo | Propietario | Destino | Consumido por |
|---|---|---|---|
| `PushBackDefaults` | PushBack | `RackCad.Domain.Systems.PushBack` | Application, Domain, Tests, UI, UI.Tests |
| `PushBackDesign` | PushBack | `RackCad.Domain.Systems.PushBack` | Application, Plugin, Tests, UI, UI.Tests |
| `PushBackFrontConfig` | PushBack | `RackCad.Domain.Systems.PushBack` | Application, Tests, UI.Tests |
| `PushBackRearTopeConfig` | PushBack | `RackCad.Domain.Systems.PushBack` | Application, Domain, Tests, UI, UI.Tests |
| `PushBackSystem` | PushBack | `RackCad.Domain.Systems.PushBack` | Application, Plugin, Tests, UI, UI.Tests |
| `PushBackResolvedFront` | PushBack | `RackCad.Domain.Systems.PushBack` | Application |

### `RackCad.Domain.Systems.Selective`

| Tipo | Propietario | Destino | Consumido por |
|---|---|---|---|
| `SelectivePalletDesign` | Selective | `RackCad.Domain.Systems.Selective` | Application, Plugin, Tests, UI, UI.Tests |
| `SafetySide` | Selective | `RackCad.Domain.Systems.Selective` | Application, Tests, UI, UI.Tests |
| `SelectiveSafetySelection` | Selective | `RackCad.Domain.Systems.Selective` | Application, Domain, Tests, UI, UI.Tests |
| `SafetyPostSide` | Selective | `RackCad.Domain.Systems.Selective` | Application, Tests, UI, UI.Tests |
| `SafetyPostDefense` | Selective | `RackCad.Domain.Systems.Selective` | Application, Domain, Tests, UI, UI.Tests |
| `SelectiveGridCell` | Selective | `RackCad.Domain.Systems.Selective` | Application, Domain, Tests, UI, UI.Tests |
| `SelectiveBayDesign` | Selective | `RackCad.Domain.Systems.Selective` | Application, Tests, UI, UI.Tests |
| `SelectiveSegment` | Selective | `RackCad.Domain.Systems.Selective` | Application, Domain, Tests, UI, UI.Tests |
| `SelectiveCell` | Selective | `RackCad.Domain.Systems.Selective` | Application, Tests, UI.Tests |
| `Tarima` | Selective | `RackCad.Domain.Systems.Selective` | Application, Tests, UI, UI.Tests |
| `SelectiveRackDefaults` | Selective | `RackCad.Domain.Systems.Selective` | Application, Domain, Plugin, Tests, UI |
| `SelectiveSafetyDefaults` | Selective | `RackCad.Domain.Systems.Selective` | Application, Domain, Tests, UI, UI.Tests |
| `SelectiveRackSystem` | Selective | `RackCad.Domain.Systems.Selective` | Application, Plugin, Tests, UI, UI.Tests |
| `SelectiveBay` | Selective | `RackCad.Domain.Systems.Selective` | Application, Tests |
| `SelectiveLevel` | Selective | `RackCad.Domain.Systems.Selective` | Application, Tests |
| `SelectiveTopeConfig` | Selective | `RackCad.Domain.Systems.Selective` | Application, Domain, Tests |
| `SelectiveDesviadorConfig` | Selective | `RackCad.Domain.Systems.Selective` | Application, Domain, Tests |
| `SelectiveDefensaConfig` | Selective | `RackCad.Domain.Systems.Selective` | Application, Domain, Tests |
| `SelectiveGuiaConfig` | Selective | `RackCad.Domain.Systems.Selective` | Application, Domain, Tests |
| `SelectiveParrillaConfig` | Selective | `RackCad.Domain.Systems.Selective` | Application, Domain, Tests |
| `SelectiveSafetyCells` | Selective | `RackCad.Domain.Systems.Selective` | Domain |

### `RackCad.Domain.Systems.Shared`

| Tipo | Propietario | Destino | Consumido por |
|---|---|---|---|
| `DimensionDetail` | Shared | `RackCad.Domain.Systems.Shared` | Application, Domain, Tests, UI, UI.Tests |
| `PalletSpecification` | Shared | `RackCad.Domain.Systems.Shared` | Application, Domain, Plugin, Tests, UI, UI.Tests |
| `RackSystemKind` | Shared | `RackCad.Domain.Systems.Shared` | Application, Domain, Plugin, Tests, UI, UI.Tests |

## Application — 181 archivos, 274 tipos


### `RackCad.Application`

| Tipo | Propietario | Destino | Consumido por |
|---|---|---|---|
| `BlockNaming` | raiz (transversal) | `RackCad.Application` | Plugin, Tests |

### `RackCad.Application.Bom`

| Tipo | Propietario | Destino | Consumido por |
|---|---|---|---|
| `BomLine` | Bom | `RackCad.Application.Bom` | Application, Tests, UI |
| `BomComponent` | Bom | `RackCad.Application.Bom` | Application, Tests |
| `BillOfMaterials` | Bom | `RackCad.Application.Bom` | Application, Plugin, Tests, UI |
| `BomBuilder` | Bom | `RackCad.Application.Bom` | Application, Plugin, Tests, UI |
| `BomCsvExporter` | Bom | `RackCad.Application.Bom` | Application, Tests, UI |
| `BomXlsxExporter` | Bom | `RackCad.Application.Bom` | Application, Tests, UI |
| `ConsolidatedRackBom` | Bom | `RackCad.Application.Bom` | Plugin, Tests, UI |
| `ConsolidatedBom` | Bom | `RackCad.Application.Bom` | Application, UI |
| `ConsolidatedBomBuilder` | Bom | `RackCad.Application.Bom` | Plugin, Tests |
| `ConsolidatedBomCsvExporter` | Bom | `RackCad.Application.Bom` | Application, Tests, UI |
| `ConsolidatedBomXlsxExporter` | Bom | `RackCad.Application.Bom` | UI |
| `XlsxCell` | Bom | `RackCad.Application.Bom` | Application, Tests |
| `XlsxSheet` | Bom | `RackCad.Application.Bom` | Application, Tests |
| `XlsxWriter` | Bom | `RackCad.Application.Bom` | Application, Tests |

### `RackCad.Application.Catalogs`

| Tipo | Propietario | Destino | Consumido por |
|---|---|---|---|
| `BlockLibraryLocator` | Catalogs | `RackCad.Application.Catalogs` | Plugin, UI |
| `CatalogBlockParameters` | Catalogs | `RackCad.Application.Catalogs` | Application, Tests |
| `CatalogDirectory` | Catalogs | `RackCad.Application.Catalogs` | Application, Tests |
| `CatalogEntryBase` | Catalogs | `RackCad.Application.Catalogs` | Application, Tests, UI, UI.Tests |
| `ProfileCatalogEntry` | Catalogs | `RackCad.Application.Catalogs` | Application, Tests |
| `SeccionCatalogEntry` | Catalogs | `RackCad.Application.Catalogs` | Application, Tests |
| `FlowBedComponentCatalogEntry` | Catalogs | `RackCad.Application.Catalogs` | Application, Plugin, UI |
| `BeamProfileCatalogEntry` | Catalogs | `RackCad.Application.Catalogs` | Application, Tests, UI |
| `MensulaCatalogEntry` | Catalogs | `RackCad.Application.Catalogs` | Application, Tests |
| `SafetyElementCatalogEntry` | Catalogs | `RackCad.Application.Catalogs` | Application, UI, UI.Tests |
| `BasePlateCatalogEntry` | Catalogs | `RackCad.Application.Catalogs` | Application, Tests |
| `ConnectionPointCatalogEntry` | Catalogs | `RackCad.Application.Catalogs` | Application, Tests |
| `ConnectionLayoutEntry` | Catalogs | `RackCad.Application.Catalogs` | Application, Tests |
| `ViewCatalogEntry` | Catalogs | `RackCad.Application.Catalogs` | Application, Tests |
| `BlockCatalogEntry` | Catalogs | `RackCad.Application.Catalogs` | Application, Tests |
| `RackCatalog` | Catalogs | `RackCad.Application.Catalogs` | Application, Plugin, Tests, UI, UI.Tests |
| `CatalogLookup` | Catalogs | `RackCad.Application.Catalogs` | Application, Tests, UI |
| `CsvCatalogReader` | Catalogs | `RackCad.Application.Catalogs` | Application, Tests |
| `IRackCatalogProvider` | Catalogs | `RackCad.Application.Catalogs` | Application |
| `JsonRackCatalogProvider` | Catalogs | `RackCad.Application.Catalogs` | Application, Plugin, Tests, UI, UI.Tests |
| `PeralteList` | Catalogs | `RackCad.Application.Catalogs` | Application, Tests, UI |
| `RackCatalogExtensions` | Catalogs | `RackCad.Application.Catalogs` | _(sin consumidores)_ |
| `RackDefaults` | Catalogs | `RackCad.Application.Catalogs` | Application, Tests |
| `SeccionRole` | Catalogs | `RackCad.Application.Catalogs` | Application |
| `SeccionRoles` | Catalogs | `RackCad.Application.Catalogs` | Application, Tests |

### `RackCad.Application.Catalogs.Validation`

| Tipo | Propietario | Destino | Consumido por |
|---|---|---|---|
| `CatalogBlockManifestEntry` | Validation | `RackCad.Application.Catalogs.Validation` | Tests |
| `CatalogBlockManifest` | Validation | `RackCad.Application.Catalogs.Validation` | Application, Tests |
| `CatalogValidationSeverity` | Validation | `RackCad.Application.Catalogs.Validation` | Application, Tests |
| `CatalogValidationCategory` | Validation | `RackCad.Application.Catalogs.Validation` | Application, Tests |
| `CatalogValidationIssue` | Validation | `RackCad.Application.Catalogs.Validation` | Application |
| `CatalogValidationReport` | Validation | `RackCad.Application.Catalogs.Validation` | Application |
| `CatalogValidator` | Validation | `RackCad.Application.Catalogs.Validation` | Tests |

### `RackCad.Application.Diagnostics`

| Tipo | Propietario | Destino | Consumido por |
|---|---|---|---|
| `CorruptFile` | Diagnostics | `RackCad.Application.Diagnostics` | Application, Tests |
| `RackDiagnosticsLog` | Diagnostics | `RackCad.Application.Diagnostics` | Application, Tests |
| `RackLog` | Diagnostics | `RackCad.Application.Diagnostics` | Application, Plugin, Tests |
| `RackLogFormatter` | Diagnostics | `RackCad.Application.Diagnostics` | Application, Tests |

### `RackCad.Application.Drawing`

| Tipo | Propietario | Destino | Consumido por |
|---|---|---|---|
| `DrawingUnits` | Drawing | `RackCad.Application.Drawing` | Plugin, Tests |
| `DrawingUnitsAdvisory` | Drawing | `RackCad.Application.Drawing` | Plugin, Tests |
| `HeaderBlockRole` | Drawing | `RackCad.Application.Drawing` | Application, Plugin, Tests, UI, UI.Tests |
| `HeaderBlockInstance` | Drawing | `RackCad.Application.Drawing` | Application, Plugin, Tests, UI, UI.Tests |
| `HeaderInstanceGrouper` | Drawing | `RackCad.Application.Drawing` | Application, Plugin, Tests |
| `HeaderRunPlan` | Drawing | `RackCad.Application.Drawing` | Application, Plugin, Tests, UI, UI.Tests |
| `HeaderPlacement` | Drawing | `RackCad.Application.Drawing` | Application |
| `HeaderGroup` | Drawing | `RackCad.Application.Drawing` | Application, Plugin, Tests |
| `LateralHeaderLayout` | Drawing | `RackCad.Application.Drawing` | Application, Plugin, Tests |

### `RackCad.Application.Formatting`

| Tipo | Propietario | Destino | Consumido por |
|---|---|---|---|
| `LocalizedNumberParser` | Formatting | `RackCad.Application.Formatting` | Tests, UI |

### `RackCad.Application.Geometry`

| Tipo | Propietario | Destino | Consumido por |
|---|---|---|---|
| `BasePlatePlacement2D` | Geometry | `RackCad.Application.Geometry` | _(sin consumidores)_ |
| `FramePlacementResolver` | Geometry | `RackCad.Application.Geometry` | Tests |
| `Point2D` | Geometry | `RackCad.Application.Geometry` | Application, Plugin, Tests, UI |
| `Placement2D` | Geometry | `RackCad.Application.Geometry` | Application |
| `MateSolver` | Geometry | `RackCad.Application.Geometry` | Application, Tests |

### `RackCad.Application.Layout`

| Tipo | Propietario | Destino | Consumido por |
|---|---|---|---|
| `PolygonGeometry` | Layout | `RackCad.Application.Layout` | Application, Tests |
| `WarehouseAutoFillResult` | Layout | `RackCad.Application.Layout` | Plugin |
| `WarehouseAutoFill` | Layout | `RackCad.Application.Layout` | Plugin, Tests |
| `FitViolationKind` | Layout | `RackCad.Application.Layout` | Application, Tests |
| `FitViolation` | Layout | `RackCad.Application.Layout` | _(sin consumidores)_ |
| `WarehouseFitResult` | Layout | `RackCad.Application.Layout` | _(sin consumidores)_ |
| `WarehouseFitChecker` | Layout | `RackCad.Application.Layout` | Application, Plugin, Tests |
| `WarehouseCell` | Layout | `RackCad.Application.Layout` | Application, Plugin |
| `RowPairing` | Layout | `RackCad.Application.Layout` | Application, Plugin, Tests |
| `RackOrientation` | Layout | `RackCad.Application.Layout` | Application, Plugin, Tests |
| `WarehouseGridPlan` | Layout | `RackCad.Application.Layout` | Application, Plugin, Tests |
| `WarehouseGridPlanner` | Layout | `RackCad.Application.Layout` | Application, Plugin, Tests |
| `SiteObstacle` | Layout | `RackCad.Application.Layout` | Application, Plugin, Tests |
| `WarehouseSite` | Layout | `RackCad.Application.Layout` | Application, Plugin, Tests |

### `RackCad.Application.Persistence`

| Tipo | Propietario | Destino | Consumido por |
|---|---|---|---|
| `AtomicFile` | Persistence | `RackCad.Application.Persistence` | Application, Tests |
| `DynamicRackSystemDocument` | Persistence | `RackCad.Application.Persistence` | Application, Tests |
| `DynamicRackFrontDocument` | Persistence | `RackCad.Application.Persistence` | _(sin consumidores)_ |
| `DynamicRackLevelDocument` | Persistence | `RackCad.Application.Persistence` | _(sin consumidores)_ |
| `DynamicRackModuleDocument` | Persistence | `RackCad.Application.Persistence` | _(sin consumidores)_ |
| `FlowBedConfigurationStore` | Persistence | `RackCad.Application.Persistence` | Application, Plugin, Tests |
| `FlowBedDocument` | Persistence | `RackCad.Application.Persistence` | Application, Plugin, Tests, UI, UI.Tests |
| `InnerSourceOutcome` | Persistence | `RackCad.Application.Persistence` | Application, Plugin, Tests |
| `InnerSourceResolution` | Persistence | `RackCad.Application.Persistence` | Application |
| `InnerSourcePreflightResult` | Persistence | `RackCad.Application.Persistence` | Application |
| `KindDispatch` | Persistence | `RackCad.Application.Persistence` | Application, Plugin, Tests |
| `KindDispatchMessages` | Persistence | `RackCad.Application.Persistence` | Plugin, Tests |
| `LargueroDocument` | Persistence | `RackCad.Application.Persistence` | Application, Tests |
| `PushBackDesignDocument` | Persistence | `RackCad.Application.Persistence` | Application, Tests |
| `PushBackFrontDocument` | Persistence | `RackCad.Application.Persistence` | _(sin consumidores)_ |
| `PushBackCellDocument` | Persistence | `RackCad.Application.Persistence` | _(sin consumidores)_ |
| `RackDesignLibraryEntry` | Persistence | `RackCad.Application.Persistence` | UI, UI.Tests |
| `RackDesignLibrary` | Persistence | `RackCad.Application.Persistence` | Tests, UI |
| `RackDesignValidation` | Persistence | `RackCad.Application.Persistence` | Application, Plugin, Tests |
| `RackEmbedComposer` | Persistence | `RackCad.Application.Persistence` | Plugin, Tests |
| `RackEmbedDocument` | Persistence | `RackCad.Application.Persistence` | Application, Plugin, Tests, UI, UI.Tests |
| `RackEmbedStore` | Persistence | `RackCad.Application.Persistence` | Plugin, Tests |
| `RackFrameProjectDocument` | Persistence | `RackCad.Application.Persistence` | Application, Tests, UI |
| `PostDocument` | Persistence | `RackCad.Application.Persistence` | _(sin consumidores)_ |
| `PlateDocument` | Persistence | `RackCad.Application.Persistence` | Tests |
| `HorizontalDocument` | Persistence | `RackCad.Application.Persistence` | _(sin consumidores)_ |
| `PanelDocument` | Persistence | `RackCad.Application.Persistence` | _(sin consumidores)_ |
| `RackFrameProjectStore` | Persistence | `RackCad.Application.Persistence` | Application, Tests, UI |
| `RackListEntry` | Persistence | `RackCad.Application.Persistence` | UI |
| `RackListBuilder` | Persistence | `RackCad.Application.Persistence` | Plugin, Tests |
| `RackProject` | Persistence | `RackCad.Application.Persistence` | Application, Plugin, Tests, UI, UI.Tests |
| `RackProjectDocument` | Persistence | `RackCad.Application.Persistence` | Application, Plugin, Tests, UI |
| `RackProjectStore` | Persistence | `RackCad.Application.Persistence` | Application, Plugin, Tests, UI, UI.Tests |
| `SafetyDocumentMapping` | Persistence | `RackCad.Application.Persistence` | Application |
| `TopeSelectionDocument` | Persistence | `RackCad.Application.Persistence` | Application, Tests |
| `DesviadorSelectionDocument` | Persistence | `RackCad.Application.Persistence` | Application, Tests |
| `DefensaSelectionDocument` | Persistence | `RackCad.Application.Persistence` | Application, Tests |
| `GuiaSelectionDocument` | Persistence | `RackCad.Application.Persistence` | Application, Tests |
| `ParrillaSelectionDocument` | Persistence | `RackCad.Application.Persistence` | Application, Tests |
| `SchemaGuard` | Persistence | `RackCad.Application.Persistence` | Application, Tests |
| `SchemaVersionPolicy` | Persistence | `RackCad.Application.Persistence` | Application, Plugin, Tests |
| `SelectivePalletDesignDocument` | Persistence | `RackCad.Application.Persistence` | Application, Plugin, Tests, UI, UI.Tests |
| `SelectiveBayDocument` | Persistence | `RackCad.Application.Persistence` | Tests |
| `SelectiveSegmentDocument` | Persistence | `RackCad.Application.Persistence` | _(sin consumidores)_ |
| `SafetySelectionDocument` | Persistence | `RackCad.Application.Persistence` | Application, Tests |
| `GridCellDocument` | Persistence | `RackCad.Application.Persistence` | Application |
| `PostSideDocument` | Persistence | `RackCad.Application.Persistence` | _(sin consumidores)_ |
| `PostDefenseDocument` | Persistence | `RackCad.Application.Persistence` | Application, Tests |
| `SelectiveCellDocument` | Persistence | `RackCad.Application.Persistence` | _(sin consumidores)_ |
| `SelectivePalletDesignStore` | Persistence | `RackCad.Application.Persistence` | Application, Plugin, Tests |

### `RackCad.Application.RackFrames`

| Tipo | Propietario | Destino | Consumido por |
|---|---|---|---|
| `BracingPanelMemberBuilder` | RackFrames | `RackCad.Application.RackFrames` | Application, Tests, UI, UI.Tests |
| `CatalogIds` | RackFrames | `RackCad.Application.RackFrames` | Application, Plugin, Tests |
| `FrameModelValidator` | RackFrames | `RackCad.Application.RackFrames` | Tests, UI |
| `HardcodedStandardRackFrameService` | RackFrames | `RackCad.Application.RackFrames` | Plugin, Tests, UI, UI.Tests |
| `LateralHeaderLayoutBuilder` | RackFrames | `RackCad.Application.RackFrames` | Application, Plugin, Tests |
| `LateralHeaderParameters` | RackFrames | `RackCad.Application.RackFrames` | Application |
| `LateralHeaderParametersFactory` | RackFrames | `RackCad.Application.RackFrames` | Application, Plugin, Tests |
| `PlantaHeaderLayoutBuilder` | RackFrames | `RackCad.Application.RackFrames` | Application, Plugin, Tests |
| `RackFrameConfigurationFactory` | RackFrames | `RackCad.Application.RackFrames` | Application, Tests, UI |
| `TemplateHorizontal` | RackFrames | `RackCad.Application.RackFrames` | Application, Tests |
| `RackFrameTemplate` | RackFrames | `RackCad.Application.RackFrames` | Application, Tests, UI |
| `RackFrameTemplateCatalog` | RackFrames | `RackCad.Application.RackFrames` | Application, Plugin, Tests, UI, UI.Tests |
| `RackFrameTemplateFactory` | RackFrames | `RackCad.Application.RackFrames` | Tests, UI |
| `RackFrameTemplateProvider` | RackFrames | `RackCad.Application.RackFrames` | Application, Tests, UI |
| `UserTemplateStore` | RackFrames | `RackCad.Application.RackFrames` | Tests, UI |

### `RackCad.Application.Settings`

| Tipo | Propietario | Destino | Consumido por |
|---|---|---|---|
| `UserSettings` | Settings | `RackCad.Application.Settings` | Application, Tests, UI |
| `UserSettingsStore` | Settings | `RackCad.Application.Settings` | Application, Tests, UI |

### `RackCad.Application.Systems.Dynamic`

| Tipo | Propietario | Destino | Consumido por |
|---|---|---|---|
| `DynamicAnnotationOptions` | Dynamic | `RackCad.Application.Systems.Dynamic` | Application, Tests, UI, UI.Tests |
| `DynamicDepthRange` | Dynamic | `RackCad.Application.Systems.Dynamic` | Application |
| `DynamicDepthLayout` | Dynamic | `RackCad.Application.Systems.Dynamic` | Application, UI |
| `DynamicDepthGeometry` | Dynamic | `RackCad.Application.Systems.Dynamic` | Application, Tests, UI |
| `DynamicDerivedPostPlacement` | Dynamic | `RackCad.Application.Systems.Dynamic` | _(sin consumidores)_ |
| `DynamicDerivedPostGeometry` | Dynamic | `RackCad.Application.Systems.Dynamic` | Application, UI |
| `DynamicEditorCell` | Dynamic | `RackCad.Application.Systems.Dynamic` | Application, Tests, UI |
| `DynamicEditorDesignAssembler` | Dynamic | `RackCad.Application.Systems.Dynamic` | Application, Tests, UI, UI.Tests |
| `DynamicEditorFront` | Dynamic | `RackCad.Application.Systems.Dynamic` | Application |
| `DynamicEditorSafety` | Dynamic | `RackCad.Application.Systems.Dynamic` | Application, Tests, UI |
| `DynamicEditorValues` | Dynamic | `RackCad.Application.Systems.Dynamic` | Application, Tests, UI |
| `DynamicEntranceGuidePlan` | Dynamic | `RackCad.Application.Systems.Dynamic` | Application, Tests |
| `DynamicEntranceGuidePlacement` | Dynamic | `RackCad.Application.Systems.Dynamic` | _(sin consumidores)_ |
| `DynamicFlowBedAxis` | Dynamic | `RackCad.Application.Systems.Dynamic` | Application |
| `DynamicFlowBedGeometry` | Dynamic | `RackCad.Application.Systems.Dynamic` | Application, Tests |
| `DynamicFlowBedLateralBuilder` | Dynamic | `RackCad.Application.Systems.Dynamic` | Application, Tests |
| `DynamicForkliftDefensePlan` | Dynamic | `RackCad.Application.Systems.Dynamic` | Application, Tests, UI, UI.Tests |
| `DynamicForkliftDefenseSetting` | Dynamic | `RackCad.Application.Systems.Dynamic` | _(sin consumidores)_ |
| `DynamicFrontActivation` | Dynamic | `RackCad.Application.Systems.Dynamic` | Application, Domain, Tests, UI, UI.Tests |
| `DynamicFrontLayout` | Dynamic | `RackCad.Application.Systems.Dynamic` | Application |
| `DynamicFrontGeometry` | Dynamic | `RackCad.Application.Systems.Dynamic` | Application, Tests, UI |
| `DynamicFrontMatrix` | Dynamic | `RackCad.Application.Systems.Dynamic` | Application, Tests, UI, UI.Tests |
| `DynamicHeaderHeightResult` | Dynamic | `RackCad.Application.Systems.Dynamic` | Application |
| `DynamicHeaderHeightCalculator` | Dynamic | `RackCad.Application.Systems.Dynamic` | Application, Tests, UI |
| `DynamicIntermediateBeamSupport` | Dynamic | `RackCad.Application.Systems.Dynamic` | _(sin consumidores)_ |
| `DynamicIntermediateBeamGeometry` | Dynamic | `RackCad.Application.Systems.Dynamic` | Application, Tests |
| `DynamicIntermediateBeamLateralBuilder` | Dynamic | `RackCad.Application.Systems.Dynamic` | Application |
| `DynamicLateralCorte` | Dynamic | `RackCad.Application.Systems.Dynamic` | Application |
| `DynamicLoadBeamPlacement` | Dynamic | `RackCad.Application.Systems.Dynamic` | Application |
| `DynamicLoadBeamGeometry` | Dynamic | `RackCad.Application.Systems.Dynamic` | Application, Tests, UI |
| `DynamicRackCellScope` | Dynamic | `RackCad.Application.Systems.Dynamic` | Application, Tests, UI |
| `DynamicRackCellAddress` | Dynamic | `RackCad.Application.Systems.Dynamic` | Application, Tests |
| `DynamicRackCellScopeResolver` | Dynamic | `RackCad.Application.Systems.Dynamic` | Application, Tests |
| `DynamicRackLevelGeometry` | Dynamic | `RackCad.Application.Systems.Dynamic` | Application, Tests, UI |
| `DynamicRackSystemBuilder` | Dynamic | `RackCad.Application.Systems.Dynamic` | Application, Plugin, Tests, UI, UI.Tests |
| `DynamicRackResolution` | Dynamic | `RackCad.Application.Systems.Dynamic` | _(sin consumidores)_ |
| `DynamicRackSystemResolver` | Dynamic | `RackCad.Application.Systems.Dynamic` | Application, Plugin, Tests, UI, UI.Tests |
| `DynamicSafetyDefaults` | Dynamic | `RackCad.Application.Systems.Dynamic` | Application, Tests, UI |
| `DynamicLateralGuardPlan` | Dynamic | `RackCad.Application.Systems.Dynamic` | Application, Tests |
| `DynamicSafetyLateralBuilder` | Dynamic | `RackCad.Application.Systems.Dynamic` | Application |
| `DynamicSafetyMultiViewBuilder` | Dynamic | `RackCad.Application.Systems.Dynamic` | Application |
| `DynamicSeparatorGeometry` | Dynamic | `RackCad.Application.Systems.Dynamic` | Application, UI |
| `DynamicSystemFrontalBuilder` | Dynamic | `RackCad.Application.Systems.Dynamic` | Application, Plugin, Tests, UI, UI.Tests |
| `DynamicSystemLateralBuilder` | Dynamic | `RackCad.Application.Systems.Dynamic` | Application, Plugin, Tests, UI.Tests |
| `DynamicSystemPlantaBuilder` | Dynamic | `RackCad.Application.Systems.Dynamic` | Application, Plugin, Tests, UI.Tests |
| `DynamicFrontalPreviewGeometry` | Dynamic | `RackCad.Application.Systems.Dynamic` | _(sin consumidores)_ |
| `DynamicLateralPreviewGeometry` | Dynamic | `RackCad.Application.Systems.Dynamic` | _(sin consumidores)_ |
| `DynamicSystemPreviewGeometry` | Dynamic | `RackCad.Application.Systems.Dynamic` | Tests, UI |
| `DynamicViewDecorations` | Dynamic | `RackCad.Application.Systems.Dynamic` | Application |
| `SystemBomBuilder` | Dynamic | `RackCad.Application.Systems.Dynamic` | Application, Plugin, Tests, UI |

### `RackCad.Application.Systems.FlowBed`

| Tipo | Propietario | Destino | Consumido por |
|---|---|---|---|
| `FlowBedBomBuilder` | FlowBed | `RackCad.Application.Systems.FlowBed` | Plugin, Tests, UI |
| `FlowBedLateralBuilder` | FlowBed | `RackCad.Application.Systems.FlowBed` | Application, Plugin, Tests, UI |

### `RackCad.Application.Systems.Larguero`

| Tipo | Propietario | Destino | Consumido por |
|---|---|---|---|
| `LargueroBomBuilder` | Larguero | `RackCad.Application.Systems.Larguero` | Application, Tests, UI |

### `RackCad.Application.Systems.PushBack`

| Tipo | Propietario | Destino | Consumido por |
|---|---|---|---|
| `PushBackAdvancedRackParameters` | PushBack | `RackCad.Application.Systems.PushBack` | Application, UI |
| `PushBackBedRotation` | PushBack | `RackCad.Application.Systems.PushBack` | Application, Tests |
| `PushBackBedSlope` | PushBack | `RackCad.Application.Systems.PushBack` | Application, Tests |
| `PushBackBomBuilder` | PushBack | `RackCad.Application.Systems.PushBack` | Application, Plugin, Tests, UI.Tests |
| `PushBackEditorCell` | PushBack | `RackCad.Application.Systems.PushBack` | Application, Tests |
| `PushBackEditorComputation` | PushBack | `RackCad.Application.Systems.PushBack` | Application, Tests, UI |
| `PushBackEditorDesignAssembler` | PushBack | `RackCad.Application.Systems.PushBack` | Application, Tests, UI |
| `PushBackEditorFront` | PushBack | `RackCad.Application.Systems.PushBack` | Application |
| `PushBackEditorInputs` | PushBack | `RackCad.Application.Systems.PushBack` | Application, Tests, UI |
| `PushBackEditorState` | PushBack | `RackCad.Application.Systems.PushBack` | Application, Tests, UI, UI.Tests |
| `PushBackEditorState` | PushBack | `RackCad.Application.Systems.PushBack` | Application, Tests, UI, UI.Tests |
| `PushBackEditorSnapshot` | PushBack | `RackCad.Application.Systems.PushBack` | _(sin consumidores)_ |
| `PushBackEditorValues` | PushBack | `RackCad.Application.Systems.PushBack` | Application, Tests, UI |
| `PushBackCellElevation` | PushBack | `RackCad.Application.Systems.PushBack` | _(sin consumidores)_ |
| `PushBackElevations` | PushBack | `RackCad.Application.Systems.PushBack` | Application, Tests |
| `PushBackFlowBedAxis` | PushBack | `RackCad.Application.Systems.PushBack` | Application, Tests |
| `PushBackFlowBedGeometry` | PushBack | `RackCad.Application.Systems.PushBack` | Application, Tests |
| `PushBackFlowBedLateralBuilder` | PushBack | `RackCad.Application.Systems.PushBack` | Application, Tests |
| `PushBackHighEndBeamGeometry` | PushBack | `RackCad.Application.Systems.PushBack` | Application, Tests |
| `PushBackIntermediateBeamLateralBuilder` | PushBack | `RackCad.Application.Systems.PushBack` | Application |
| `PushBackLoadBeamGeometry` | PushBack | `RackCad.Application.Systems.PushBack` | Application, Tests |
| `PushBackPlanComposer` | PushBack | `RackCad.Application.Systems.PushBack` | Application |
| `PushBackRearTopeBuilder` | PushBack | `RackCad.Application.Systems.PushBack` | Application, Tests, UI, UI.Tests |
| `PushBackResolver` | PushBack | `RackCad.Application.Systems.PushBack` | Application, Plugin, Tests |
| `PushBackSafetyAuthority` | PushBack | `RackCad.Application.Systems.PushBack` | Application, Tests, UI, UI.Tests |
| `PushBackFrontalEnd` | PushBack | `RackCad.Application.Systems.PushBack` | Application, Plugin, Tests, UI, UI.Tests |
| `PushBackSystemFrontalBuilder` | PushBack | `RackCad.Application.Systems.PushBack` | Application, Plugin, Tests, UI.Tests |
| `PushBackSystemLateralBuilder` | PushBack | `RackCad.Application.Systems.PushBack` | Application, Plugin, Tests |
| `PushBackSystemPlantaBuilder` | PushBack | `RackCad.Application.Systems.PushBack` | Application, Plugin, Tests |
| `PushBackTroquelGrid` | PushBack | `RackCad.Application.Systems.PushBack` | Application, Tests |

### `RackCad.Application.Systems.Selective`

| Tipo | Propietario | Destino | Consumido por |
|---|---|---|---|
| `SelectiveAnnotations` | Selective | `RackCad.Application.Systems.Selective` | Application |
| `SelectiveApplyScope` | Selective | `RackCad.Application.Systems.Selective` | Application, Tests, UI |
| `SelectiveBomBuilder` | Selective | `RackCad.Application.Systems.Selective` | Application, Plugin, Tests, UI, UI.Tests |
| `SelectiveDepthLayout` | Selective | `RackCad.Application.Systems.Selective` | Application, Plugin, Tests, UI, UI.Tests |
| `SelectiveDesignInputs` | Selective | `RackCad.Application.Systems.Selective` | Application, Tests, UI |
| `SelectiveDesviadorDrawing` | Selective | `RackCad.Application.Systems.Selective` | Application |
| `SelectiveDesviadorPlan` | Selective | `RackCad.Application.Systems.Selective` | Application, Tests, UI |
| `SelectiveDimensions` | Selective | `RackCad.Application.Systems.Selective` | Application |
| `SelectiveEditorCell` | Selective | `RackCad.Application.Systems.Selective` | Application, Tests, UI |
| `SelectiveEditorFondoMatrix` | Selective | `RackCad.Application.Systems.Selective` | Application, Tests, UI |
| `SelectiveEditorState` | Selective | `RackCad.Application.Systems.Selective` | Application, Tests, UI, UI.Tests |
| `SelectiveFrontalBuilder` | Selective | `RackCad.Application.Systems.Selective` | Application, Plugin, Tests, UI, UI.Tests |
| `SelectiveGeometryResolver` | Selective | `RackCad.Application.Systems.Selective` | Application, Domain, Plugin, Tests, UI, UI.Tests |
| `SelectiveLateralBuilder` | Selective | `RackCad.Application.Systems.Selective` | Application, Plugin, Tests, UI.Tests |
| `SelectiveCorte` | Selective | `RackCad.Application.Systems.Selective` | _(sin consumidores)_ |
| `SelectiveMedioFrente` | Selective | `RackCad.Application.Systems.Selective` | Application, Domain |
| `SelectiveParrillaPlacement` | Selective | `RackCad.Application.Systems.Selective` | Application, Tests |
| `SelectiveParrillaPlan` | Selective | `RackCad.Application.Systems.Selective` | Application, Tests, UI, UI.Tests |
| `SelectivePlantaBuilder` | Selective | `RackCad.Application.Systems.Selective` | Application, Plugin, Tests, UI.Tests |
| `SelectivePostGeometry` | Selective | `RackCad.Application.Systems.Selective` | Application, Tests, UI |
| `SelectivePostLayout` | Selective | `RackCad.Application.Systems.Selective` | Application |
| `SafetyEndCopy` | Selective | `RackCad.Application.Systems.Selective` | Application |
| `SelectiveSafetyEnds` | Selective | `RackCad.Application.Systems.Selective` | Application, Tests, UI.Tests |
| `SelectiveSafetyFamilies` | Selective | `RackCad.Application.Systems.Selective` | Application, Tests, UI, UI.Tests |
| `SelectiveSafetyGrid` | Selective | `RackCad.Application.Systems.Selective` | Application, Tests, UI, UI.Tests |
| `SelectiveSafetyPlacement` | Selective | `RackCad.Application.Systems.Selective` | Application, Tests |
| `SelectiveSeparadorPlacement` | Selective | `RackCad.Application.Systems.Selective` | Application, Tests |
| `SelectiveSeparadorPlan` | Selective | `RackCad.Application.Systems.Selective` | Application |
| `SelectiveTarimaPlacement` | Selective | `RackCad.Application.Systems.Selective` | Application, Tests |
| `SelectiveTopePlacement` | Selective | `RackCad.Application.Systems.Selective` | Application, Tests |
| `SelectiveTopePlan` | Selective | `RackCad.Application.Systems.Selective` | Application, Tests |

### `RackCad.Application.Systems.Shared`

| Tipo | Propietario | Destino | Consumido por |
|---|---|---|---|
| `RackFrontLevelElevations` | Shared | `RackCad.Application.Systems.Shared` | Application, Tests |
| `RackLevelElevations` | Shared | `RackCad.Application.Systems.Shared` | Application, Tests |
| `RackLevelElevationsExtensions` | Shared | `RackCad.Application.Systems.Shared` | _(sin consumidores)_ |
| `RackModuleDescriptor` | Shared | `RackCad.Application.Systems.Shared` | Application, Tests, UI, UI.Tests |
| `RackModuleCommit` | Shared | `RackCad.Application.Systems.Shared` | Application |
| `RackModuleEditSession` | Shared | `RackCad.Application.Systems.Shared` | Application, Tests |
| `RackModuleReconciliationResult` | Shared | `RackCad.Application.Systems.Shared` | Application, Tests |
| `RackModuleReconciliation` | Shared | `RackCad.Application.Systems.Shared` | Application, Tests |
| `SafetyDormantCells` | Shared | `RackCad.Application.Systems.Shared` | Tests, UI |
| `SeparatorLevelCalculator` | Shared | `RackCad.Application.Systems.Shared` | Application, Tests |
| `SystemDescriptor` | Shared | `RackCad.Application.Systems.Shared` | Application, Tests |
| `SystemRegistry` | Shared | `RackCad.Application.Systems.Shared` | Application, Plugin, Tests, UI |
| `SystemRegistry` | Shared | `RackCad.Application.Systems.Shared` | Application, Plugin, Tests, UI |

## UI — 75 archivos, 105 tipos


### `RackCad.UI`

| Tipo | Propietario | Destino | Consumido por |
|---|---|---|---|
| `CatalogOption` | raiz (transversal) | `RackCad.UI` | UI, UI.Tests |
| `EnumDisplayConverter` | raiz (transversal) | `RackCad.UI` | _(sin consumidores)_ |
| `ObservableObject` | raiz (transversal) | `RackCad.UI` | UI |
| `PreviewCanvasPainter` | raiz (transversal) | `RackCad.UI` | UI, UI.Tests |
| `RackBomWindow` | raiz (transversal) | `RackCad.UI` | Application, UI |
| `RackCommandHelpWindow` | raiz (transversal) | `RackCad.UI` | Plugin |
| `RackCommandInfo` | raiz (transversal) | `RackCad.UI` | _(sin consumidores)_ |
| `RackCommandReference` | raiz (transversal) | `RackCad.UI` | UI |
| `RackConsolidatedBomWindow` | raiz (transversal) | `RackCad.UI` | Plugin |
| `RackDesignLibraryWindow` | raiz (transversal) | `RackCad.UI` | UI |
| `RackListRow` | raiz (transversal) | `RackCad.UI` | Plugin, UI |
| `RackListWindow` | raiz (transversal) | `RackCad.UI` | Plugin |
| `RackMainMenuWindow` | raiz (transversal) | `RackCad.UI` | Plugin, UI, UI.Tests |
| `RackWarehouseFillWindow` | raiz (transversal) | `RackCad.UI` | Plugin |
| `RackWarehouseLayoutWindow` | raiz (transversal) | `RackCad.UI` | Plugin, Tests |
| `SafetyDefensaGridWindow` | raiz (transversal) | `RackCad.UI` | UI, UI.Tests |
| `SafetyDesviadorGridWindow` | raiz (transversal) | `RackCad.UI` | UI, UI.Tests |
| `SafetyGuiaEntradaGridWindow` | raiz (transversal) | `RackCad.UI` | UI, UI.Tests |
| `SafetyParrillaGridWindow` | raiz (transversal) | `RackCad.UI` | UI, UI.Tests |
| `SafetyTopeGridWindow` | raiz (transversal) | `RackCad.UI` | UI, UI.Tests |
| `SelectiveSafetyWindow` | raiz (transversal) | `RackCad.UI` | UI, UI.Tests |
| `SafetyPerPostWindow` | raiz (transversal) | `RackCad.UI` | _(sin consumidores)_ |
| `UiSupport` | raiz (transversal) | `RackCad.UI` | Plugin, UI, UI.Tests |

### `RackCad.UI.Controls`

| Tipo | Propietario | Destino | Consumido por |
|---|---|---|---|
| `CatalogCombo` | Controls | `RackCad.UI.Controls` | UI, UI.Tests |
| `CatalogComboSelection` | Controls | `RackCad.UI.Controls` | UI, UI.Tests |
| `NumericField` | Controls | `RackCad.UI.Controls` | UI, UI.Tests |
| `NumericFieldStatus` | Controls | `RackCad.UI.Controls` | UI, UI.Tests |
| `NumericFieldValidationResult` | Controls | `RackCad.UI.Controls` | UI |
| `NumericFieldValidation` | Controls | `RackCad.UI.Controls` | UI, UI.Tests |
| `PreviewCanvas` | Controls | `RackCad.UI.Controls` | UI, UI.Tests |
| `PreviewPalette` | Controls | `RackCad.UI.Controls` | UI, UI.Tests |
| `PreviewProjection` | Controls | `RackCad.UI.Controls` | UI, UI.Tests |
| `DialogActionBar` | Controls | `RackCad.UI.Controls` | UI.Tests |
| `RackDialogWindow` | Controls | `RackCad.UI.Controls` | UI.Tests |
| `SelectionMatrix` | Controls | `RackCad.UI.Controls` | Tests, UI, UI.Tests |
| `SelectionMatrixBulkBar` | Controls | `RackCad.UI.Controls` | UI, UI.Tests |
| `SelectionMatrixScopeLabels` | Controls | `RackCad.UI.Controls` | UI, UI.Tests |
| `SelectionMatrixBulkEditor` | Controls | `RackCad.UI.Controls` | UI, UI.Tests |
| `SelectionMatrixCell` | Controls | `RackCad.UI.Controls` | UI, UI.Tests |
| `SelectionMatrixCellChangedEventArgs` | Controls | `RackCad.UI.Controls` | UI |
| `SelectionMatrixModel` | Controls | `RackCad.UI.Controls` | UI, UI.Tests |
| `SelectionMatrixScope` | Controls | `RackCad.UI.Controls` | UI, UI.Tests |
| `SelectionMatrixScopeAppliedEventArgs` | Controls | `RackCad.UI.Controls` | UI, UI.Tests |

### `RackCad.UI.Editor`

| Tipo | Propietario | Destino | Consumido por |
|---|---|---|---|
| `DispatcherRecomputeScheduler` | Editor | `RackCad.UI.Editor` | UI |
| `EditorModuleRegistry` | Editor | `RackCad.UI.Editor` | UI, UI.Tests |
| `SelectiveEditorModule` | Editor | `RackCad.UI.Editor` | UI |
| `DynamicEditorModule` | Editor | `RackCad.UI.Editor` | UI, UI.Tests |
| `PushBackEditorModule` | Editor | `RackCad.UI.Editor` | UI, UI.Tests |
| `HeaderEditorModule` | Editor | `RackCad.UI.Editor` | UI |
| `FlowBedEditorModule` | Editor | `RackCad.UI.Editor` | UI |
| `LargueroEditorModule` | Editor | `RackCad.UI.Editor` | UI |
| `IRackEditorModule` | Editor | `RackCad.UI.Editor` | UI, UI.Tests |
| `IRecomputeScheduler` | Editor | `RackCad.UI.Editor` | UI, UI.Tests |
| `RackEditorIdentity` | Editor | `RackCad.UI.Editor` | UI, UI.Tests |
| `RackEditorLaunchContext` | Editor | `RackCad.UI.Editor` | UI, UI.Tests |
| `RackInsertionContext` | Editor | `RackCad.UI.Editor` | UI.Tests |
| `RackEditorSession` | Editor | `RackCad.UI.Editor` | UI, UI.Tests |
| `RackInsertionRequest` | Editor | `RackCad.UI.Editor` | UI, UI.Tests |
| `HeaderInsertionRequest` | Editor | `RackCad.UI.Editor` | Plugin, UI, UI.Tests |
| `DynamicInsertionRequest` | Editor | `RackCad.UI.Editor` | Plugin, UI, UI.Tests |
| `FlowBedInsertionRequest` | Editor | `RackCad.UI.Editor` | Plugin, UI, UI.Tests |
| `PushBackInsertionRequest` | Editor | `RackCad.UI.Editor` | Plugin, Tests, UI, UI.Tests |
| `SelectiveInsertionRequest` | Editor | `RackCad.UI.Editor` | Plugin, UI, UI.Tests |
| `RecomputeDebouncer` | Editor | `RackCad.UI.Editor` | UI, UI.Tests |
| `RecomputeGate` | Editor | `RackCad.UI.Editor` | UI, UI.Tests |

### `RackCad.UI.Preview`

| Tipo | Propietario | Destino | Consumido por |
|---|---|---|---|
| `EditorPreviewPalette` | Preview | `RackCad.UI.Preview` | UI |
| `EditorPreviewParts` | Preview | `RackCad.UI.Preview` | UI |
| `EditorPreviewSurface` | Preview | `RackCad.UI.Preview` | UI, UI.Tests |

### `RackCad.UI.RackFrames`

| Tipo | Propietario | Destino | Consumido por |
|---|---|---|---|
| `BracingSegmentEditorRow` | RackFrames | `RackCad.UI.RackFrames` | UI |
| `ConfiguratorNavigationItem` | RackFrames | `RackCad.UI.RackFrames` | UI |
| `FrameExceptionEditorRow` | RackFrames | `RackCad.UI.RackFrames` | UI |
| `FrameExceptionGroup` | RackFrames | `RackCad.UI.RackFrames` | UI |
| `HorizontalEditorRow` | RackFrames | `RackCad.UI.RackFrames` | UI |
| `RackFrameConfiguratorLayoutSettings` | RackFrames | `RackCad.UI.RackFrames` | UI |
| `RackFrameConfiguratorLayoutStore` | RackFrames | `RackCad.UI.RackFrames` | UI |
| `RackFrameConfiguratorViewModel` | RackFrames | `RackCad.UI.RackFrames` | Tests, UI, UI.Tests |
| `RackFrameConfiguratorWindow` | RackFrames | `RackCad.UI.RackFrames` | Application, Plugin, Tests, UI, UI.Tests |
| `RackFrameEngineeringPreviewLayout` | RackFrames | `RackCad.UI.RackFrames` | UI |
| `RackFrameEngineeringPreviewSegment` | RackFrames | `RackCad.UI.RackFrames` | UI |
| `RackFrameEngineeringPreviewPost` | RackFrames | `RackCad.UI.RackFrames` | UI |
| `RackFrameEngineeringPreviewPlate` | RackFrames | `RackCad.UI.RackFrames` | UI |

### `RackCad.UI.Shell`

| Tipo | Propietario | Destino | Consumido por |
|---|---|---|---|
| `EditorAction` | Shell | `RackCad.UI.Shell` | UI.Tests |
| `EditorActions` | Shell | `RackCad.UI.Shell` | UI, UI.Tests |
| `EditorActionBar` | Shell | `RackCad.UI.Shell` | UI, UI.Tests |
| `EditorStatusSeverity` | Shell | `RackCad.UI.Shell` | UI, UI.Tests |
| `EditorStatusMessage` | Shell | `RackCad.UI.Shell` | UI, UI.Tests |
| `EditorStatusPalette` | Shell | `RackCad.UI.Shell` | UI |
| `EditorStatusPresenter` | Shell | `RackCad.UI.Shell` | UI, UI.Tests |
| `RackEditorVisualShell` | Shell | `RackCad.UI.Shell` | UI.Tests |
| `ShellResources` | Shell | `RackCad.UI.Shell` | UI, UI.Tests |
| `ShellSlotVisibilityConverter` | Shell | `RackCad.UI.Shell` | UI.Tests |

### `RackCad.UI.Systems.Dynamic`

| Tipo | Propietario | Destino | Consumido por |
|---|---|---|---|
| `RackDynamicSystemWindow` | Dynamic | `RackCad.UI.Systems.Dynamic` | Plugin, Tests, UI, UI.Tests |

### `RackCad.UI.Systems.FlowBed`

| Tipo | Propietario | Destino | Consumido por |
|---|---|---|---|
| `RackFlowBedWindow` | FlowBed | `RackCad.UI.Systems.FlowBed` | Plugin, UI, UI.Tests |

### `RackCad.UI.Systems.Larguero`

| Tipo | Propietario | Destino | Consumido por |
|---|---|---|---|
| `RackLargueroWindow` | Larguero | `RackCad.UI.Systems.Larguero` | UI, UI.Tests |

### `RackCad.UI.Systems.PushBack`

| Tipo | Propietario | Destino | Consumido por |
|---|---|---|---|
| `PushBackMatrixCard` | PushBack | `RackCad.UI.Systems.PushBack` | UI |
| `PushBackMatrixCardModel` | PushBack | `RackCad.UI.Systems.PushBack` | UI, UI.Tests |
| `PushBackPreviewKind` | PushBack | `RackCad.UI.Systems.PushBack` | _(sin consumidores)_ |
| `PushBackPreviewPrimitive` | PushBack | `RackCad.UI.Systems.PushBack` | _(sin consumidores)_ |
| `PushBackPreviewModel` | PushBack | `RackCad.UI.Systems.PushBack` | UI, UI.Tests |
| `PushBackPreviewRenderer` | PushBack | `RackCad.UI.Systems.PushBack` | UI, UI.Tests |
| `PushBackRearTopeDialogAdapter` | PushBack | `RackCad.UI.Systems.PushBack` | UI, UI.Tests |
| `PushBackRearTopeSection` | PushBack | `RackCad.UI.Systems.PushBack` | UI, UI.Tests |
| `RackPushBackSystemWindow` | PushBack | `RackCad.UI.Systems.PushBack` | Plugin, Tests, UI, UI.Tests |

### `RackCad.UI.Systems.Selective`

| Tipo | Propietario | Destino | Consumido por |
|---|---|---|---|
| `RackSelectiveWindow` | Selective | `RackCad.UI.Systems.Selective` | Application, Plugin, Tests, UI, UI.Tests |
| `SelectiveSegmentsWindow` | Selective | `RackCad.UI.Systems.Selective` | UI |

## Plugin — 49 archivos, 52 tipos


### `RackCad.Plugin`

| Tipo | Propietario | Destino | Consumido por |
|---|---|---|---|
| `InDocumentTransaction` | raiz (transversal) | `RackCad.Plugin` | Plugin |
| `LayerHelper` | raiz (transversal) | `RackCad.Plugin` | Plugin |
| `PluginInitializer` | raiz (transversal) | `RackCad.Plugin` | _(sin consumidores)_ |
| `RackAyudaCommands` | raiz (transversal) | `RackCad.Plugin` | _(sin consumidores)_ |
| `RackBlockFinder` | raiz (transversal) | `RackCad.Plugin` | Application, Plugin |
| `RackEnvelopeScan` | raiz (transversal) | `RackCad.Plugin` | Plugin |
| `RackCabeceraCommands` | raiz (transversal) | `RackCad.Plugin` | Plugin, Tests, UI |
| `RackCamaCommands` | raiz (transversal) | `RackCad.Plugin` | Plugin, Tests, UI |
| `RackCatalogLoader` | raiz (transversal) | `RackCad.Plugin` | Plugin |
| `RackCloner` | raiz (transversal) | `RackCad.Plugin` | Plugin |
| `RackCommandSupport` | raiz (transversal) | `RackCad.Plugin` | Plugin, Tests |
| `InnerSourcePreflight` | raiz (transversal) | `RackCad.Plugin` | _(sin consumidores)_ |
| `RackDinamicoCommands` | raiz (transversal) | `RackCad.Plugin` | Plugin, Tests, UI |
| `RackDuplicarCommands` | raiz (transversal) | `RackCad.Plugin` | Tests |
| `RackEnvelopeRestamp` | raiz (transversal) | `RackCad.Plugin` | Plugin, Tests |
| `RackInventarioCommands` | raiz (transversal) | `RackCad.Plugin` | Plugin, Tests |
| `RackInventarioCommands` | raiz (transversal) | `RackCad.Plugin` | Plugin, Tests |
| `RackLayoutCommands` | raiz (transversal) | `RackCad.Plugin` | Plugin, Tests |
| `RackLayoutCommands` | raiz (transversal) | `RackCad.Plugin` | Plugin, Tests |
| `RackMenuCommands` | raiz (transversal) | `RackCad.Plugin` | Tests |
| `RackPushBackCommands` | raiz (transversal) | `RackCad.Plugin` | Plugin, Tests |
| `RackSelectivoCommands` | raiz (transversal) | `RackCad.Plugin` | Plugin, Tests, UI |
| `RackUnitsGuard` | raiz (transversal) | `RackCad.Plugin` | Application, Plugin, Tests |

### `RackCad.Plugin.Drawing`

| Tipo | Propietario | Destino | Consumido por |
|---|---|---|---|
| `BlockLibraryImporter` | Drawing | `RackCad.Plugin.Drawing` | Plugin |
| `BlockPlacement` | Drawing | `RackCad.Plugin.Drawing` | Plugin |
| `LateralHeaderBlockResult` | Drawing | `RackCad.Plugin.Drawing` | Plugin |
| `LateralHeaderDrawOutcome` | Drawing | `RackCad.Plugin.Drawing` | Plugin |
| `LateralHeaderDrawService` | Drawing | `RackCad.Plugin.Drawing` | Plugin |
| `HeaderPlacementResult` | Drawing | `RackCad.Plugin.Drawing` | Plugin |
| `LateralHeaderDrawer` | Drawing | `RackCad.Plugin.Drawing` | Plugin |
| `PlantaHeaderDrawService` | Drawing | `RackCad.Plugin.Drawing` | Plugin, Tests |
| `RackBlockRenamer` | Drawing | `RackCad.Plugin.Drawing` | Plugin, Tests |

### `RackCad.Plugin.KindHandlers`

| Tipo | Propietario | Destino | Consumido por |
|---|---|---|---|
| `CabeceraKindHandler` | KindHandlers | `RackCad.Plugin.KindHandlers` | Plugin, Tests |
| `CamaKindHandler` | KindHandlers | `RackCad.Plugin.KindHandlers` | Plugin, Tests |
| `DynamicKindHandler` | KindHandlers | `RackCad.Plugin.KindHandlers` | Plugin, Tests |
| `IRackKindHandler` | KindHandlers | `RackCad.Plugin.KindHandlers` | Application, Plugin, Tests |
| `KindHandlerDispatch` | KindHandlers | `RackCad.Plugin.KindHandlers` | Plugin, Tests |
| `KindHandlerRegistry` | KindHandlers | `RackCad.Plugin.KindHandlers` | Application, Plugin, Tests, UI |
| `PushBackKindHandler` | KindHandlers | `RackCad.Plugin.KindHandlers` | Plugin, Tests |
| `SelectiveKindHandler` | KindHandlers | `RackCad.Plugin.KindHandlers` | Plugin, Tests |

### `RackCad.Plugin.Systems.Dynamic`

| Tipo | Propietario | Destino | Consumido por |
|---|---|---|---|
| `DynamicFrontalDrawService` | Dynamic | `RackCad.Plugin.Systems.Dynamic` | Plugin, Tests |
| `DynamicPlantaDrawService` | Dynamic | `RackCad.Plugin.Systems.Dynamic` | Plugin, Tests |
| `DynamicSystemDrawService` | Dynamic | `RackCad.Plugin.Systems.Dynamic` | Plugin, Tests |

### `RackCad.Plugin.Systems.FlowBed`

| Tipo | Propietario | Destino | Consumido por |
|---|---|---|---|
| `FlowBedDrawService` | FlowBed | `RackCad.Plugin.Systems.FlowBed` | Plugin, Tests |

### `RackCad.Plugin.Systems.PushBack`

| Tipo | Propietario | Destino | Consumido por |
|---|---|---|---|
| `PushBackFrontalDrawService` | PushBack | `RackCad.Plugin.Systems.PushBack` | Plugin, Tests |
| `PushBackPlantaDrawService` | PushBack | `RackCad.Plugin.Systems.PushBack` | Plugin, Tests |
| `PushBackSystemDrawService` | PushBack | `RackCad.Plugin.Systems.PushBack` | Plugin, Tests |

### `RackCad.Plugin.Systems.Selective`

| Tipo | Propietario | Destino | Consumido por |
|---|---|---|---|
| `SelectiveFrontalDrawService` | Selective | `RackCad.Plugin.Systems.Selective` | Plugin |
| `SelectivePlantaDrawService` | Selective | `RackCad.Plugin.Systems.Selective` | Plugin |

### `RackCad.Plugin.Systems.Shared`

| Tipo | Propietario | Destino | Consumido por |
|---|---|---|---|
| `RackBlockData` | Shared | `RackCad.Plugin.Systems.Shared` | Plugin |
| `SystemBlockWriter` | Shared | `RackCad.Plugin.Systems.Shared` | Plugin |
| `ViewBlockDraw` | Shared | `RackCad.Plugin.Systems.Shared` | Plugin, Tests |

## Tests — 159 archivos, 160 tipos


### `RackCad.Tests`

| Tipo | Propietario | Destino | Consumido por |
|---|---|---|---|
| `AtomicFileTests` | transversal | `RackCad.Tests` | _(sin consumidores)_ |
| `BasePlatePeraltePersistenceTests` | transversal | `RackCad.Tests` | _(sin consumidores)_ |
| `BeamCatalogTests` | transversal | `RackCad.Tests` | _(sin consumidores)_ |
| `BlankFrontBoundaryTests` | Dynamic+PushBack+Selective | `RackCad.Tests` | _(sin consumidores)_ |
| `BlankFrontSafetyTests` | Dynamic+Selective | `RackCad.Tests` | _(sin consumidores)_ |
| `BlankFrontTests` | Dynamic+PushBack+Selective | `RackCad.Tests` | _(sin consumidores)_ |
| `BlockNamingTests` | transversal | `RackCad.Tests` | _(sin consumidores)_ |
| `BomBuilderTests` | transversal | `RackCad.Tests` | _(sin consumidores)_ |
| `BracingPanelMemberBuilderTests` | transversal | `RackCad.Tests` | _(sin consumidores)_ |
| `CatalogBackedStandardServiceTests` | transversal | `RackCad.Tests` | _(sin consumidores)_ |
| `CatalogBlockManifestTests` | transversal | `RackCad.Tests` | Tests |
| `CatalogBlockParametersTests` | FlowBed+Selective | `RackCad.Tests` | _(sin consumidores)_ |
| `CatalogCanonicalIdsTests` | Dynamic+FlowBed+Selective | `RackCad.Tests` | _(sin consumidores)_ |
| `CatalogDirectoryTests` | transversal | `RackCad.Tests` | _(sin consumidores)_ |
| `CatalogManifestGuardTests` | Dynamic+FlowBed+Selective | `RackCad.Tests` | _(sin consumidores)_ |
| `CatalogStandardConsistencyTests` | transversal | `RackCad.Tests` | _(sin consumidores)_ |
| `CatalogValidatorTests` | transversal | `RackCad.Tests` | _(sin consumidores)_ |
| `ConsolidatedBomBuilderTests` | transversal | `RackCad.Tests` | _(sin consumidores)_ |
| `CsvCatalogReaderTests` | transversal | `RackCad.Tests` | _(sin consumidores)_ |
| `DiagnosticsNegativeTests` | transversal | `RackCad.Tests` | _(sin consumidores)_ |
| `DrawServicePlanBaselineTests` | Dynamic+FlowBed+Selective | `RackCad.Tests` | _(sin consumidores)_ |
| `DrawingUnitsAdvisoryTests` | transversal | `RackCad.Tests` | _(sin consumidores)_ |
| `DynamicDepthGeometryTests` | Dynamic+Selective | `RackCad.Tests` | _(sin consumidores)_ |
| `DynamicEditorCellTests` | Dynamic | `RackCad.Tests` | _(sin consumidores)_ |
| `DynamicEditorDesignAssemblerTests` | Dynamic+Selective | `RackCad.Tests` | _(sin consumidores)_ |
| `DynamicEditorSafetyTests` | Dynamic+Selective | `RackCad.Tests` | _(sin consumidores)_ |
| `DynamicEntranceGuidePlanTests` | Dynamic+Selective | `RackCad.Tests` | _(sin consumidores)_ |
| `DynamicFlowBedLateralBuilderTests` | Dynamic | `RackCad.Tests` | _(sin consumidores)_ |
| `DynamicForkliftDefensePlanTests` | Dynamic+Selective | `RackCad.Tests` | _(sin consumidores)_ |
| `DynamicFrontGeometryTests` | Dynamic | `RackCad.Tests` | Tests |
| `DynamicFrontMatrixTests` | Dynamic | `RackCad.Tests` | _(sin consumidores)_ |
| `DynamicHeaderHeightCalculatorTests` | Dynamic | `RackCad.Tests` | _(sin consumidores)_ |
| `DynamicLoadBeamGeometryTests` | Dynamic+Selective | `RackCad.Tests` | _(sin consumidores)_ |
| `DynamicNullOverrideGoldenTests` | Dynamic | `RackCad.Tests` | Tests |
| `DynamicPostLevelsTests` | Dynamic | `RackCad.Tests` | _(sin consumidores)_ |
| `DynamicRackSystemBuilderTests` | Dynamic | `RackCad.Tests` | _(sin consumidores)_ |
| `DynamicRackSystemResolverTests` | Dynamic+Selective | `RackCad.Tests` | _(sin consumidores)_ |
| `DynamicSafetyDefaultsTests` | Dynamic+Selective | `RackCad.Tests` | _(sin consumidores)_ |
| `DynamicSystemLateralBuilderTests` | Dynamic+Selective | `RackCad.Tests` | _(sin consumidores)_ |
| `DynamicSystemMultiViewBuilderTests` | Dynamic+Selective | `RackCad.Tests` | _(sin consumidores)_ |
| `ElevationOverrideSourceGuardTests` | PushBack | `RackCad.Tests` | _(sin consumidores)_ |
| `FlowBedBomBuilderTests` | FlowBed | `RackCad.Tests` | _(sin consumidores)_ |
| `FlowBedCatalogTests` | transversal | `RackCad.Tests` | _(sin consumidores)_ |
| `FlowBedConfigurationStoreTests` | FlowBed | `RackCad.Tests` | _(sin consumidores)_ |
| `FlowBedLateralBuilderTests` | FlowBed | `RackCad.Tests` | _(sin consumidores)_ |
| `FrameModelValidatorTests` | transversal | `RackCad.Tests` | _(sin consumidores)_ |
| `GeometryMateSolverTests` | transversal | `RackCad.Tests` | _(sin consumidores)_ |
| `HardcodedStandardRackFrameServiceTests` | transversal | `RackCad.Tests` | _(sin consumidores)_ |
| `HeaderInstanceGrouperTests` | transversal | `RackCad.Tests` | _(sin consumidores)_ |
| `JsonRackCatalogProviderTests` | transversal | `RackCad.Tests` | _(sin consumidores)_ |
| `KindDispatchTests` | transversal | `RackCad.Tests` | Tests |
| `KindHandlerGuardSourceTests` | transversal | `RackCad.Tests` | Tests |
| `LargueroBomBuilderTests` | Larguero | `RackCad.Tests` | _(sin consumidores)_ |
| `LateralHeaderLayoutBuilderTests` | transversal | `RackCad.Tests` | _(sin consumidores)_ |
| `LateralHeaderParametersFactoryTests` | transversal | `RackCad.Tests` | _(sin consumidores)_ |
| `LocalizedNumberParserTests` | transversal | `RackCad.Tests` | _(sin consumidores)_ |
| `NamespaceFolderGuardTests` | transversal | `RackCad.Tests` | UI.Tests |
| `PeralteListTests` | transversal | `RackCad.Tests` | _(sin consumidores)_ |
| `PersistenceEmbedInnerDesignTests` | Dynamic | `RackCad.Tests` | _(sin consumidores)_ |
| `PersistenceInnerPreflightTests` | Dynamic+FlowBed | `RackCad.Tests` | _(sin consumidores)_ |
| `PersistenceInnerSourceTests` | FlowBed | `RackCad.Tests` | _(sin consumidores)_ |
| `PersistenceLibraryTransportTests` | Dynamic+FlowBed | `RackCad.Tests` | _(sin consumidores)_ |
| `PersistenceReopenPreservationTests` | Dynamic+FlowBed+Larguero+Selective | `RackCad.Tests` | _(sin consumidores)_ |
| `PersistenceUniformityTests` | FlowBed+Larguero | `RackCad.Tests` | _(sin consumidores)_ |
| `PersistenceValidationTests` | transversal | `RackCad.Tests` | _(sin consumidores)_ |
| `PersistenceVersioningTests` | FlowBed+Larguero | `RackCad.Tests` | _(sin consumidores)_ |
| `PlantaHeaderLayoutBuilderTests` | transversal | `RackCad.Tests` | _(sin consumidores)_ |
| `PushBackAdvancedRackParametersTests` | Dynamic+PushBack | `RackCad.Tests` | _(sin consumidores)_ |
| `PushBackBedAnchorTests` | Dynamic+FlowBed+PushBack+Selective | `RackCad.Tests` | Tests |
| `PushBackBedAsymmetryTests` | Dynamic+FlowBed+PushBack+Selective | `RackCad.Tests` | Tests |
| `PushBackBedInOutMateTests` | Dynamic+FlowBed+PushBack+Selective | `RackCad.Tests` | _(sin consumidores)_ |
| `PushBackBedSlopeTests` | Dynamic+FlowBed+PushBack+Selective | `RackCad.Tests` | _(sin consumidores)_ |
| `PushBackBedTangencyTests` | Dynamic+PushBack | `RackCad.Tests` | _(sin consumidores)_ |
| `PushBackBomTests` | Dynamic+PushBack | `RackCad.Tests` | _(sin consumidores)_ |
| `PushBackCoreTests` | Dynamic+PushBack+Selective | `RackCad.Tests` | _(sin consumidores)_ |
| `PushBackCorrectionTests` | Dynamic+PushBack+Selective | `RackCad.Tests` | _(sin consumidores)_ |
| `PushBackDefenseResizeTests` | Dynamic+PushBack+Selective | `RackCad.Tests` | _(sin consumidores)_ |
| `PushBackDesviadorCellKeyTests` | Dynamic+PushBack+Selective | `RackCad.Tests` | _(sin consumidores)_ |
| `PushBackEditorCorrectionTests` | Dynamic+PushBack+Selective | `RackCad.Tests` | _(sin consumidores)_ |
| `PushBackEditorDefaultsTests` | Dynamic+PushBack | `RackCad.Tests` | _(sin consumidores)_ |
| `PushBackEditorStateTests` | Dynamic+PushBack+Selective | `RackCad.Tests` | _(sin consumidores)_ |
| `PushBackElevationAuthorityTests` | Dynamic+FlowBed+PushBack+Selective | `RackCad.Tests` | _(sin consumidores)_ |
| `PushBackElevationConsumerTests` | Dynamic+PushBack+Selective | `RackCad.Tests` | _(sin consumidores)_ |
| `PushBackEndToEndChainTests` | Dynamic+PushBack | `RackCad.Tests` | _(sin consumidores)_ |
| `PushBackForkliftDefenseTests` | Dynamic+PushBack+Selective | `RackCad.Tests` | _(sin consumidores)_ |
| `PushBackGoldenTests` | Dynamic+PushBack+Selective | `RackCad.Tests` | Tests |
| `PushBackHeaderHeightTests` | Dynamic+PushBack | `RackCad.Tests` | _(sin consumidores)_ |
| `PushBackLateralGuardDefaultTests` | Dynamic+PushBack+Selective | `RackCad.Tests` | _(sin consumidores)_ |
| `PushBackLateralProtectorEndsTests` | Dynamic+PushBack+Selective | `RackCad.Tests` | _(sin consumidores)_ |
| `PushBackLegacyTieBreakTests` | Dynamic+FlowBed+PushBack | `RackCad.Tests` | _(sin consumidores)_ |
| `PushBackLengthCoherenceTests` | Dynamic+PushBack+Selective | `RackCad.Tests` | _(sin consumidores)_ |
| `PushBackModuleAdoptionTests` | Dynamic+PushBack | `RackCad.Tests` | _(sin consumidores)_ |
| `PushBackModuleEditorCharacterizationTests` | Dynamic+PushBack | `RackCad.Tests` | _(sin consumidores)_ |
| `PushBackNoParrillaPersistenceTests` | Dynamic+PushBack+Selective | `RackCad.Tests` | _(sin consumidores)_ |
| `PushBackOwnerRejectionRound1Tests` | Dynamic+PushBack+Selective | `RackCad.Tests` | _(sin consumidores)_ |
| `PushBackPlanTests` | Dynamic+PushBack+Selective | `RackCad.Tests` | _(sin consumidores)_ |
| `PushBackPluginSourceGuardTests` | Dynamic+PushBack | `RackCad.Tests` | Tests |
| `PushBackRearTopeAnchorTests` | Dynamic+PushBack+Selective | `RackCad.Tests` | Tests |
| `PushBackRearTopePieceTests` | Dynamic+PushBack | `RackCad.Tests` | _(sin consumidores)_ |
| `PushBackRegistrationTests` | Dynamic+PushBack | `RackCad.Tests` | _(sin consumidores)_ |
| `PushBackRoundTripSourceGuardTests` | Dynamic+PushBack+Selective | `RackCad.Tests` | _(sin consumidores)_ |
| `PushBackSafetyBomTests` | Dynamic+PushBack+Selective | `RackCad.Tests` | _(sin consumidores)_ |
| `PushBackSafetyPerPostTests` | Dynamic+PushBack+Selective | `RackCad.Tests` | _(sin consumidores)_ |
| `PushBackTopeAnchorPerViewTests` | Dynamic+PushBack+Selective | `RackCad.Tests` | _(sin consumidores)_ |
| `PushBackTopeOriginMateTests` | Dynamic+PushBack+Selective | `RackCad.Tests` | _(sin consumidores)_ |
| `PushBackViewTests` | Dynamic+PushBack | `RackCad.Tests` | _(sin consumidores)_ |
| `PushBackWholeLateralEnvelopeTests` | Dynamic+PushBack+Selective | `RackCad.Tests` | _(sin consumidores)_ |
| `RackDesignLibraryTests` | Dynamic+FlowBed+Larguero+Selective | `RackCad.Tests` | _(sin consumidores)_ |
| `RackEmbedDocumentTests` | transversal | `RackCad.Tests` | _(sin consumidores)_ |
| `RackFrameConfigurationDeepCopyTests` | Selective | `RackCad.Tests` | _(sin consumidores)_ |
| `RackFrameConfigurationFactoryTests` | transversal | `RackCad.Tests` | _(sin consumidores)_ |
| `RackFrameProjectStoreTests` | transversal | `RackCad.Tests` | Tests |
| `RackFrameTemplateProviderTests` | transversal | `RackCad.Tests` | _(sin consumidores)_ |
| `RackLevelElevationsTests` | transversal | `RackCad.Tests` | _(sin consumidores)_ |
| `RackListBuilderTests` | transversal | `RackCad.Tests` | _(sin consumidores)_ |
| `RackLogCollection` | transversal | `RackCad.Tests` | _(sin consumidores)_ |
| `LogCapture` | transversal | `RackCad.Tests` | Tests |
| `RackLogTests` | transversal | `RackCad.Tests` | _(sin consumidores)_ |
| `RackModuleEditSessionTests` | Dynamic+PushBack | `RackCad.Tests` | _(sin consumidores)_ |
| `RackProjectStorePerKindTests` | FlowBed+Larguero+Selective | `RackCad.Tests` | Tests |
| `RackProjectStoreTests` | Dynamic+Selective | `RackCad.Tests` | Tests |
| `RackUnitsGuardSourceTests` | transversal | `RackCad.Tests` | Tests |
| `SafetySelectionDocumentsTests` | Selective | `RackCad.Tests` | _(sin consumidores)_ |
| `SeccionesCatalogTests` | transversal | `RackCad.Tests` | _(sin consumidores)_ |
| `SelectiveBomBuilderTests` | Larguero+Selective | `RackCad.Tests` | _(sin consumidores)_ |
| `SelectiveDepthTests` | Selective | `RackCad.Tests` | _(sin consumidores)_ |
| `SelectiveDesviadorTests` | Selective | `RackCad.Tests` | _(sin consumidores)_ |
| `SelectiveDimensionsTests` | Selective | `RackCad.Tests` | _(sin consumidores)_ |
| `SelectiveEditorStateTests` | Selective | `RackCad.Tests` | _(sin consumidores)_ |
| `SelectiveFrontalBuilderTests` | Selective | `RackCad.Tests` | _(sin consumidores)_ |
| `SelectiveGeometryResolverTests` | Selective | `RackCad.Tests` | _(sin consumidores)_ |
| `SelectiveLateralBuilderTests` | Selective | `RackCad.Tests` | _(sin consumidores)_ |
| `SelectiveMedioFrenteTests` | Selective | `RackCad.Tests` | _(sin consumidores)_ |
| `SelectivePalletDesignDocumentTests` | Selective | `RackCad.Tests` | _(sin consumidores)_ |
| `SelectiveParrillaPlacementTests` | Selective | `RackCad.Tests` | _(sin consumidores)_ |
| `SelectivePerFondoTests` | Selective | `RackCad.Tests` | _(sin consumidores)_ |
| `SelectivePlantaPlanTests` | Selective | `RackCad.Tests` | _(sin consumidores)_ |
| `SelectivePostGeometryTests` | Selective | `RackCad.Tests` | _(sin consumidores)_ |
| `SelectiveSafetyConfigTests` | Selective | `RackCad.Tests` | _(sin consumidores)_ |
| `SelectiveSafetyEquivalenceTests` | Selective | `RackCad.Tests` | _(sin consumidores)_ |
| `SelectiveSafetyGridTests` | Selective | `RackCad.Tests` | _(sin consumidores)_ |
| `SelectiveSafetyTests` | Selective | `RackCad.Tests` | _(sin consumidores)_ |
| `SelectiveSeparadorPlacementTests` | Dynamic+Selective | `RackCad.Tests` | _(sin consumidores)_ |
| `SelectiveTarimaPlacementTests` | Selective | `RackCad.Tests` | _(sin consumidores)_ |
| `SelectiveTopePlacementTests` | Selective | `RackCad.Tests` | _(sin consumidores)_ |
| `SelectiveTopePlanFrontalTests` | Selective | `RackCad.Tests` | _(sin consumidores)_ |
| `SelectiveTwentyBaysEquivalenceTests` | Selective | `RackCad.Tests` | _(sin consumidores)_ |
| `SeparatorLevelCalculatorTests` | transversal | `RackCad.Tests` | _(sin consumidores)_ |
| `ShippedCatalogIntegrityTests` | transversal | `RackCad.Tests` | Tests |
| `SystemBomBuilderTests` | Dynamic+Selective | `RackCad.Tests` | _(sin consumidores)_ |
| `SystemKindPersistenceCharacterizationTests` | Dynamic+FlowBed+Larguero+Selective | `RackCad.Tests` | Tests |
| `SystemRegistryTests` | transversal | `RackCad.Tests` | _(sin consumidores)_ |
| `TestCatalogIds` | transversal | `RackCad.Tests` | Tests, UI.Tests |
| `UserSettingsStoreTests` | transversal | `RackCad.Tests` | _(sin consumidores)_ |
| `UserTemplateStoreTests` | transversal | `RackCad.Tests` | _(sin consumidores)_ |
| `UserTemplatesTests` | transversal | `RackCad.Tests` | _(sin consumidores)_ |
| `WarehouseAutoFillTests` | transversal | `RackCad.Tests` | _(sin consumidores)_ |
| `WarehouseFitCheckerTests` | transversal | `RackCad.Tests` | _(sin consumidores)_ |
| `WarehouseGridPlannerTests` | transversal | `RackCad.Tests` | _(sin consumidores)_ |
| `XlsxWriterTests` | transversal | `RackCad.Tests` | _(sin consumidores)_ |

## UI.Tests — 61 archivos, 62 tipos


### `RackCad.UI.Tests`

| Tipo | Propietario | Destino | Consumido por |
|---|---|---|---|
| `BlankFrontDesviadorHandoffTests` | Dynamic+PushBack+Selective | `RackCad.UI.Tests` | _(sin consumidores)_ |
| `BlankFrontEditorTests` | Dynamic+PushBack | `RackCad.UI.Tests` | _(sin consumidores)_ |
| `BlankFrontSafetyGridTests` | Dynamic+PushBack+Selective | `RackCad.UI.Tests` | _(sin consumidores)_ |
| `CatalogComboSelectionTests` | transversal | `RackCad.UI.Tests` | _(sin consumidores)_ |
| `CatalogComboTests` | transversal | `RackCad.UI.Tests` | _(sin consumidores)_ |
| `DynamicEditorWindowTests` | Dynamic+Selective | `RackCad.UI.Tests` | UI.Tests |
| `DynamicPreviewExtractionEquivalenceTests` | Dynamic | `RackCad.UI.Tests` | _(sin consumidores)_ |
| `DynamicPreviewRendererCharacterizationTests` | Dynamic | `RackCad.UI.Tests` | _(sin consumidores)_ |
| `DynamicShellMigrationTests` | Dynamic | `RackCad.UI.Tests` | _(sin consumidores)_ |
| `EditorModuleRegistryTests` | FlowBed+Larguero | `RackCad.UI.Tests` | _(sin consumidores)_ |
| `EditorShellAdoptionTests` | Dynamic+FlowBed+Selective | `RackCad.UI.Tests` | _(sin consumidores)_ |
| `EditorVisualShellTests` | transversal | `RackCad.UI.Tests` | _(sin consumidores)_ |
| `EditorWindowTestSupport` | transversal | `RackCad.UI.Tests` | UI.Tests |
| `FlowBedEditorWindowTests` | FlowBed | `RackCad.UI.Tests` | _(sin consumidores)_ |
| `NumericFieldTests` | transversal | `RackCad.UI.Tests` | _(sin consumidores)_ |
| `NumericFieldValidationTests` | transversal | `RackCad.UI.Tests` | _(sin consumidores)_ |
| `PreviewCanvasTests` | transversal | `RackCad.UI.Tests` | _(sin consumidores)_ |
| `PreviewPaletteTests` | transversal | `RackCad.UI.Tests` | _(sin consumidores)_ |
| `PreviewProjectionTests` | transversal | `RackCad.UI.Tests` | _(sin consumidores)_ |
| `PushBackAdvancedRackParametersWindowTests` | PushBack | `RackCad.UI.Tests` | _(sin consumidores)_ |
| `PushBackDefensaDialogTests` | Dynamic+Selective | `RackCad.UI.Tests` | _(sin consumidores)_ |
| `PushBackDesviadorGridTests` | Dynamic+PushBack+Selective | `RackCad.UI.Tests` | _(sin consumidores)_ |
| `PushBackEditorLayoutTests` | Dynamic+PushBack+Selective | `RackCad.UI.Tests` | _(sin consumidores)_ |
| `PushBackEditorModuleTests` | Dynamic+FlowBed+Larguero+PushBack | `RackCad.UI.Tests` | _(sin consumidores)_ |
| `PushBackEditorWindowTests` | Dynamic+PushBack+Selective | `RackCad.UI.Tests` | _(sin consumidores)_ |
| `PushBackInsertionRequestTests` | PushBack | `RackCad.UI.Tests` | _(sin consumidores)_ |
| `PushBackMatrixAndPreviewTests` | Dynamic+PushBack+Selective | `RackCad.UI.Tests` | _(sin consumidores)_ |
| `PushBackModuleEditorWindowTests` | PushBack | `RackCad.UI.Tests` | _(sin consumidores)_ |
| `PushBackPalletScopeTests` | Dynamic+PushBack+Selective | `RackCad.UI.Tests` | _(sin consumidores)_ |
| `PushBackRichPreviewTests` | Dynamic+PushBack+Selective | `RackCad.UI.Tests` | _(sin consumidores)_ |
| `PushBackPreviewProbe` | Dynamic+PushBack+Selective | `RackCad.UI.Tests` | _(sin consumidores)_ |
| `PushBackSafetyFlowTests` | PushBack+Selective | `RackCad.UI.Tests` | _(sin consumidores)_ |
| `PushBackSafetyPerPostFlowTests` | PushBack+Selective | `RackCad.UI.Tests` | _(sin consumidores)_ |
| `PushBackShellAdoptionTests` | PushBack | `RackCad.UI.Tests` | _(sin consumidores)_ |
| `PushBackSidebarCompositionTests` | PushBack | `RackCad.UI.Tests` | _(sin consumidores)_ |
| `PushBackTopeDialogOptionsTests` | PushBack+Selective | `RackCad.UI.Tests` | _(sin consumidores)_ |
| `PushBackTopeOnlyInSafetyTests` | PushBack+Selective | `RackCad.UI.Tests` | _(sin consumidores)_ |
| `PushBackTopeSectionInSafetyTests` | PushBack+Selective | `RackCad.UI.Tests` | _(sin consumidores)_ |
| `RackDialogWindowTests` | transversal | `RackCad.UI.Tests` | _(sin consumidores)_ |
| `RackEditorIdentityTests` | transversal | `RackCad.UI.Tests` | _(sin consumidores)_ |
| `RackEditorSessionTests` | transversal | `RackCad.UI.Tests` | _(sin consumidores)_ |
| `RackFrameConfiguratorViewModelTests` | transversal | `RackCad.UI.Tests` | _(sin consumidores)_ |
| `RackInsertionRequestTests` | FlowBed | `RackCad.UI.Tests` | _(sin consumidores)_ |
| `RackMainMenuPushBackTests` | transversal | `RackCad.UI.Tests` | _(sin consumidores)_ |
| `RecomputeDebouncerTests` | transversal | `RackCad.UI.Tests` | _(sin consumidores)_ |
| `RecomputeGateTests` | transversal | `RackCad.UI.Tests` | _(sin consumidores)_ |
| `SafetyDialogTestSupport` | transversal | `RackCad.UI.Tests` | UI.Tests |
| `SafetyGridAdoptionTests` | Selective | `RackCad.UI.Tests` | _(sin consumidores)_ |
| `SafetyGridBulkAdoptionTests` | PushBack+Selective | `RackCad.UI.Tests` | _(sin consumidores)_ |
| `SafetyParrillaBulkAdoptionTests` | Selective | `RackCad.UI.Tests` | _(sin consumidores)_ |
| `SelectionMatrixAbsentCellTests` | transversal | `RackCad.UI.Tests` | _(sin consumidores)_ |
| `SelectionMatrixBulkEditTests` | transversal | `RackCad.UI.Tests` | _(sin consumidores)_ |
| `SelectionMatrixBulkEditorStaTests` | transversal | `RackCad.UI.Tests` | _(sin consumidores)_ |
| `SelectionMatrixModelTests` | transversal | `RackCad.UI.Tests` | _(sin consumidores)_ |
| `SelectionMatrixScopeGuardTests` | transversal | `RackCad.UI.Tests` | _(sin consumidores)_ |
| `SelectionMatrixTests` | transversal | `RackCad.UI.Tests` | _(sin consumidores)_ |
| `SelectiveEditorStateAdoptionTests` | Selective | `RackCad.UI.Tests` | UI.Tests |
| `SelectiveEditorWindowTests` | Selective | `RackCad.UI.Tests` | UI.Tests |
| `SelectiveShellMigrationTests` | Dynamic+Selective | `RackCad.UI.Tests` | _(sin consumidores)_ |
| `StaTestRunner` | transversal | `RackCad.UI.Tests` | UI.Tests |
| `StaTestRunnerTests` | transversal | `RackCad.UI.Tests` | _(sin consumidores)_ |
| `UiSystemBoundaryGuardTests` | Dynamic+FlowBed+Larguero+PushBack+Selective | `RackCad.UI.Tests` | _(sin consumidores)_ |

## Resumen por proyecto y frontera

| Proyecto | Frontera (namespace) | Archivos | Tipos |
|---|---|---|---|
| Domain | `RackCad.Domain.RackFrames` | 18 | 19 |
| Domain | `RackCad.Domain.Systems.Dynamic` | 7 | 12 |
| Domain | `RackCad.Domain.Systems.FlowBed` | 3 | 3 |
| Domain | `RackCad.Domain.Systems.Larguero` | 1 | 1 |
| Domain | `RackCad.Domain.Systems.PushBack` | 4 | 6 |
| Domain | `RackCad.Domain.Systems.Selective` | 4 | 21 |
| Domain | `RackCad.Domain.Systems.Shared` | 3 | 3 |
| Application | `RackCad.Application` | 1 | 1 |
| Application | `RackCad.Application.Bom` | 8 | 14 |
| Application | `RackCad.Application.Catalogs` | 12 | 25 |
| Application | `RackCad.Application.Catalogs.Validation` | 4 | 7 |
| Application | `RackCad.Application.Diagnostics` | 4 | 4 |
| Application | `RackCad.Application.Drawing` | 5 | 9 |
| Application | `RackCad.Application.Formatting` | 1 | 1 |
| Application | `RackCad.Application.Geometry` | 3 | 5 |
| Application | `RackCad.Application.Layout` | 5 | 14 |
| Application | `RackCad.Application.Persistence` | 23 | 50 |
| Application | `RackCad.Application.RackFrames` | 14 | 15 |
| Application | `RackCad.Application.Settings` | 1 | 2 |
| Application | `RackCad.Application.Systems.Dynamic` | 34 | 50 |
| Application | `RackCad.Application.Systems.FlowBed` | 2 | 2 |
| Application | `RackCad.Application.Systems.Larguero` | 1 | 1 |
| Application | `RackCad.Application.Systems.PushBack` | 26 | 30 |
| Application | `RackCad.Application.Systems.Selective` | 28 | 31 |
| Application | `RackCad.Application.Systems.Shared` | 9 | 13 |
| UI | `RackCad.UI` | 21 | 23 |
| UI | `RackCad.UI.Controls` | 13 | 20 |
| UI | `RackCad.UI.Editor` | 11 | 22 |
| UI | `RackCad.UI.Preview` | 3 | 3 |
| UI | `RackCad.UI.RackFrames` | 9 | 13 |
| UI | `RackCad.UI.Shell` | 7 | 10 |
| UI | `RackCad.UI.Systems.Dynamic` | 1 | 1 |
| UI | `RackCad.UI.Systems.FlowBed` | 1 | 1 |
| UI | `RackCad.UI.Systems.Larguero` | 1 | 1 |
| UI | `RackCad.UI.Systems.PushBack` | 6 | 9 |
| UI | `RackCad.UI.Systems.Selective` | 2 | 2 |
| Plugin | `RackCad.Plugin` | 21 | 23 |
| Plugin | `RackCad.Plugin.Drawing` | 8 | 9 |
| Plugin | `RackCad.Plugin.KindHandlers` | 8 | 8 |
| Plugin | `RackCad.Plugin.Systems.Dynamic` | 3 | 3 |
| Plugin | `RackCad.Plugin.Systems.FlowBed` | 1 | 1 |
| Plugin | `RackCad.Plugin.Systems.PushBack` | 3 | 3 |
| Plugin | `RackCad.Plugin.Systems.Selective` | 2 | 2 |
| Plugin | `RackCad.Plugin.Systems.Shared` | 3 | 3 |
| Tests | `RackCad.Tests` | 159 | 160 |
| UI.Tests | `RackCad.UI.Tests` | 61 | 62 |
| **Total** | | **565** | **718** |

## Por que las pruebas conservan su namespace de ensamblado

Medido sobre este mismo arbol, por numero de sistemas que cada archivo de prueba toca:

| Sistemas que toca | Archivos de prueba |
|---|---|
| ninguno (transversal) | 76 |
| 1 | 52 |
| 2 | 44 |
| 3 | 38 |
| 4 | 9 |
| 5 | 1 |
| **Total** | **220** |

**92 de 220 archivos de prueba (42%) tocan MAS DE UN sistema.** Asignarles un namespace propietario seria arbitrario justo en el 42% de los casos, que es lo contrario de la regla que I-23 establece: el propietario tiene que ser inequivoco. Por eso los dos proyectos de prueba conservan un unico namespace de ensamblado, como **excepcion explicita y comprobable** (`NamespaceFolderGuardTests.TestProjects_KeepExactlyOneAssemblyRootNamespace`).

