# I-23 — Inventario de namespaces por sistema

> Generado sobre la base `b43b5d1` **antes** de mover ningun archivo. Es la fotografia
> que la revision del refactor usa para comprobar que nada se perdio ni cambio de dueño.


Cada fila es un tipo de primer nivel: su archivo de origen, su destino, y **los proyectos
que realmente lo consumen** (medidos por referencia textual en `src` y `tests`, excluyendo su
propio archivo). Un consumidor en otro sistema NO mueve el tipo: la composicion entre sistemas
es legal (ROADMAP, principio 5).


## `RackCad.Application.Drawing`

| Tipo | Archivo origen | Archivo destino | Consumido por |
|---|---|---|---|
| `HeaderBlockRole` | `src/RackCad.Application/Headers/HeaderBlockInstance.cs` | `src/RackCad.Application/Drawing/HeaderBlockInstance.cs` | Application, Plugin, Tests, UI, UI.Tests |
| `HeaderBlockInstance` | `src/RackCad.Application/Headers/HeaderBlockInstance.cs` | `src/RackCad.Application/Drawing/HeaderBlockInstance.cs` | Application, Plugin, Tests, UI, UI.Tests |
| `LateralHeaderLayout` | `src/RackCad.Application/Headers/LateralHeaderLayout.cs` | `src/RackCad.Application/Drawing/LateralHeaderLayout.cs` | Application, Plugin, Tests |
| `DynamicSystemPlan` -> **`HeaderRunPlan`** | `src/RackCad.Application/Systems/DynamicSystemPlan.cs` | `src/RackCad.Application/Drawing/HeaderRunPlan.cs` | Application, Plugin, Tests, UI, UI.Tests |
| `HeaderPlacement` | `src/RackCad.Application/Systems/DynamicSystemPlan.cs` | `src/RackCad.Application/Drawing/HeaderRunPlan.cs` | Application |
| `HeaderGroup` | `src/RackCad.Application/Systems/DynamicSystemPlan.cs` | `src/RackCad.Application/Drawing/HeaderRunPlan.cs` | Application, Plugin, Tests |
| `HeaderInstanceGrouper` | `src/RackCad.Application/Systems/HeaderInstanceGrouper.cs` | `src/RackCad.Application/Drawing/HeaderInstanceGrouper.cs` | Application, Plugin, Tests |

## `RackCad.Application.RackFrames`

| Tipo | Archivo origen | Archivo destino | Consumido por |
|---|---|---|---|
| `LateralHeaderLayoutBuilder` | `src/RackCad.Application/Headers/LateralHeaderLayoutBuilder.cs` | `src/RackCad.Application/RackFrames/LateralHeaderLayoutBuilder.cs` | Application, Plugin, Tests |
| `LateralHeaderParameters` | `src/RackCad.Application/Headers/LateralHeaderParameters.cs` | `src/RackCad.Application/RackFrames/LateralHeaderParameters.cs` | Application |
| `LateralHeaderParametersFactory` | `src/RackCad.Application/Headers/LateralHeaderParametersFactory.cs` | `src/RackCad.Application/RackFrames/LateralHeaderParametersFactory.cs` | Application, Plugin, Tests |
| `PlantaHeaderLayoutBuilder` | `src/RackCad.Application/Headers/PlantaHeaderLayoutBuilder.cs` | `src/RackCad.Application/RackFrames/PlantaHeaderLayoutBuilder.cs` | Application, Plugin, Tests |

## `RackCad.Application.Systems.Dynamic`

| Tipo | Archivo origen | Archivo destino | Consumido por |
|---|---|---|---|
| `DynamicAnnotationOptions` | `src/RackCad.Application/Systems/DynamicAnnotationOptions.cs` | `src/RackCad.Application/Systems/Dynamic/DynamicAnnotationOptions.cs` | Application, Tests, UI, UI.Tests |
| `DynamicDepthRange` | `src/RackCad.Application/Systems/DynamicDepthGeometry.cs` | `src/RackCad.Application/Systems/Dynamic/DynamicDepthGeometry.cs` | Application |
| `DynamicDepthLayout` | `src/RackCad.Application/Systems/DynamicDepthGeometry.cs` | `src/RackCad.Application/Systems/Dynamic/DynamicDepthGeometry.cs` | Application, UI |
| `DynamicDepthGeometry` | `src/RackCad.Application/Systems/DynamicDepthGeometry.cs` | `src/RackCad.Application/Systems/Dynamic/DynamicDepthGeometry.cs` | Application, Tests, UI |
| `DynamicDerivedPostPlacement` | `src/RackCad.Application/Systems/DynamicDerivedPostGeometry.cs` | `src/RackCad.Application/Systems/Dynamic/DynamicDerivedPostGeometry.cs` | _(sin consumidores)_ |
| `DynamicDerivedPostGeometry` | `src/RackCad.Application/Systems/DynamicDerivedPostGeometry.cs` | `src/RackCad.Application/Systems/Dynamic/DynamicDerivedPostGeometry.cs` | Application, UI |
| `DynamicEditorCell` | `src/RackCad.Application/Systems/DynamicEditorCell.cs` | `src/RackCad.Application/Systems/Dynamic/DynamicEditorCell.cs` | Application, Tests, UI |
| `DynamicEditorDesignAssembler` | `src/RackCad.Application/Systems/DynamicEditorDesignAssembler.cs` | `src/RackCad.Application/Systems/Dynamic/DynamicEditorDesignAssembler.cs` | Application, Tests, UI, UI.Tests |
| `DynamicEditorFront` | `src/RackCad.Application/Systems/DynamicEditorFront.cs` | `src/RackCad.Application/Systems/Dynamic/DynamicEditorFront.cs` | Application |
| `DynamicEditorSafety` | `src/RackCad.Application/Systems/DynamicEditorSafety.cs` | `src/RackCad.Application/Systems/Dynamic/DynamicEditorSafety.cs` | Application, Tests, UI |
| `DynamicEditorValues` | `src/RackCad.Application/Systems/DynamicEditorValues.cs` | `src/RackCad.Application/Systems/Dynamic/DynamicEditorValues.cs` | Application, Tests, UI |
| `DynamicEntranceGuidePlan` | `src/RackCad.Application/Systems/DynamicEntranceGuidePlan.cs` | `src/RackCad.Application/Systems/Dynamic/DynamicEntranceGuidePlan.cs` | Application, Tests |
| `DynamicEntranceGuidePlacement` | `src/RackCad.Application/Systems/DynamicEntranceGuidePlan.cs` | `src/RackCad.Application/Systems/Dynamic/DynamicEntranceGuidePlan.cs` | _(sin consumidores)_ |
| `DynamicFlowBedAxis` | `src/RackCad.Application/Systems/DynamicFlowBedGeometry.cs` | `src/RackCad.Application/Systems/Dynamic/DynamicFlowBedGeometry.cs` | Application |
| `DynamicFlowBedGeometry` | `src/RackCad.Application/Systems/DynamicFlowBedGeometry.cs` | `src/RackCad.Application/Systems/Dynamic/DynamicFlowBedGeometry.cs` | Application, Tests |
| `DynamicFlowBedLateralBuilder` | `src/RackCad.Application/Systems/DynamicFlowBedLateralBuilder.cs` | `src/RackCad.Application/Systems/Dynamic/DynamicFlowBedLateralBuilder.cs` | Application, Tests |
| `DynamicForkliftDefensePlan` | `src/RackCad.Application/Systems/DynamicForkliftDefensePlan.cs` | `src/RackCad.Application/Systems/Dynamic/DynamicForkliftDefensePlan.cs` | Application, Tests, UI, UI.Tests |
| `DynamicForkliftDefenseSetting` | `src/RackCad.Application/Systems/DynamicForkliftDefensePlan.cs` | `src/RackCad.Application/Systems/Dynamic/DynamicForkliftDefensePlan.cs` | _(sin consumidores)_ |
| `DynamicFrontActivation` | `src/RackCad.Application/Systems/DynamicFrontActivation.cs` | `src/RackCad.Application/Systems/Dynamic/DynamicFrontActivation.cs` | Application, Domain, Tests, UI, UI.Tests |
| `DynamicFrontLayout` | `src/RackCad.Application/Systems/DynamicFrontGeometry.cs` | `src/RackCad.Application/Systems/Dynamic/DynamicFrontGeometry.cs` | Application |
| `DynamicFrontGeometry` | `src/RackCad.Application/Systems/DynamicFrontGeometry.cs` | `src/RackCad.Application/Systems/Dynamic/DynamicFrontGeometry.cs` | Application, Tests, UI |
| `DynamicFrontMatrix` | `src/RackCad.Application/Systems/DynamicFrontMatrix.cs` | `src/RackCad.Application/Systems/Dynamic/DynamicFrontMatrix.cs` | Application, Tests, UI, UI.Tests |
| `DynamicHeaderHeightResult` | `src/RackCad.Application/Systems/DynamicHeaderHeightCalculator.cs` | `src/RackCad.Application/Systems/Dynamic/DynamicHeaderHeightCalculator.cs` | Application |
| `DynamicHeaderHeightCalculator` | `src/RackCad.Application/Systems/DynamicHeaderHeightCalculator.cs` | `src/RackCad.Application/Systems/Dynamic/DynamicHeaderHeightCalculator.cs` | Application, Tests, UI |
| `DynamicIntermediateBeamSupport` | `src/RackCad.Application/Systems/DynamicIntermediateBeamGeometry.cs` | `src/RackCad.Application/Systems/Dynamic/DynamicIntermediateBeamGeometry.cs` | _(sin consumidores)_ |
| `DynamicIntermediateBeamGeometry` | `src/RackCad.Application/Systems/DynamicIntermediateBeamGeometry.cs` | `src/RackCad.Application/Systems/Dynamic/DynamicIntermediateBeamGeometry.cs` | Application, Tests |
| `DynamicIntermediateBeamLateralBuilder` | `src/RackCad.Application/Systems/DynamicIntermediateBeamLateralBuilder.cs` | `src/RackCad.Application/Systems/Dynamic/DynamicIntermediateBeamLateralBuilder.cs` | Application |
| `DynamicLateralCorte` | `src/RackCad.Application/Systems/DynamicLateralCorte.cs` | `src/RackCad.Application/Systems/Dynamic/DynamicLateralCorte.cs` | Application |
| `DynamicLoadBeamPlacement` | `src/RackCad.Application/Systems/DynamicLoadBeamGeometry.cs` | `src/RackCad.Application/Systems/Dynamic/DynamicLoadBeamGeometry.cs` | Application |
| `DynamicLoadBeamGeometry` | `src/RackCad.Application/Systems/DynamicLoadBeamGeometry.cs` | `src/RackCad.Application/Systems/Dynamic/DynamicLoadBeamGeometry.cs` | Application, Tests, UI |
| `DynamicRackCellScope` | `src/RackCad.Application/Systems/DynamicRackCellScope.cs` | `src/RackCad.Application/Systems/Dynamic/DynamicRackCellScope.cs` | Application, Tests, UI |
| `DynamicRackCellAddress` | `src/RackCad.Application/Systems/DynamicRackCellScope.cs` | `src/RackCad.Application/Systems/Dynamic/DynamicRackCellScope.cs` | Application, Tests |
| `DynamicRackCellScopeResolver` | `src/RackCad.Application/Systems/DynamicRackCellScope.cs` | `src/RackCad.Application/Systems/Dynamic/DynamicRackCellScope.cs` | Application, Tests |
| `DynamicRackLevelGeometry` | `src/RackCad.Application/Systems/DynamicRackLevelGeometry.cs` | `src/RackCad.Application/Systems/Dynamic/DynamicRackLevelGeometry.cs` | Application, Tests, UI |
| `DynamicRackSystemBuilder` | `src/RackCad.Application/Systems/DynamicRackSystemBuilder.cs` | `src/RackCad.Application/Systems/Dynamic/DynamicRackSystemBuilder.cs` | Application, Plugin, Tests, UI, UI.Tests |
| `DynamicRackResolution` | `src/RackCad.Application/Systems/DynamicRackSystemResolver.cs` | `src/RackCad.Application/Systems/Dynamic/DynamicRackSystemResolver.cs` | _(sin consumidores)_ |
| `DynamicRackSystemResolver` | `src/RackCad.Application/Systems/DynamicRackSystemResolver.cs` | `src/RackCad.Application/Systems/Dynamic/DynamicRackSystemResolver.cs` | Application, Plugin, Tests, UI, UI.Tests |
| `DynamicSafetyDefaults` | `src/RackCad.Application/Systems/DynamicSafetyDefaults.cs` | `src/RackCad.Application/Systems/Dynamic/DynamicSafetyDefaults.cs` | Application, Tests, UI |
| `DynamicLateralGuardPlan` | `src/RackCad.Application/Systems/DynamicSafetyDefaults.cs` | `src/RackCad.Application/Systems/Dynamic/DynamicSafetyDefaults.cs` | Application, Tests |
| `DynamicSafetyLateralBuilder` | `src/RackCad.Application/Systems/DynamicSafetyLateralBuilder.cs` | `src/RackCad.Application/Systems/Dynamic/DynamicSafetyLateralBuilder.cs` | Application |
| `DynamicSafetyMultiViewBuilder` | `src/RackCad.Application/Systems/DynamicSafetyMultiViewBuilder.cs` | `src/RackCad.Application/Systems/Dynamic/DynamicSafetyMultiViewBuilder.cs` | Application |
| `DynamicSeparatorGeometry` | `src/RackCad.Application/Systems/DynamicSeparatorGeometry.cs` | `src/RackCad.Application/Systems/Dynamic/DynamicSeparatorGeometry.cs` | Application, UI |
| `DynamicSystemFrontalBuilder` | `src/RackCad.Application/Systems/DynamicSystemFrontalBuilder.cs` | `src/RackCad.Application/Systems/Dynamic/DynamicSystemFrontalBuilder.cs` | Application, Plugin, Tests, UI, UI.Tests |
| `DynamicSystemLateralBuilder` | `src/RackCad.Application/Systems/DynamicSystemLateralBuilder.cs` | `src/RackCad.Application/Systems/Dynamic/DynamicSystemLateralBuilder.cs` | Application, Plugin, Tests, UI.Tests |
| `DynamicSystemPlantaBuilder` | `src/RackCad.Application/Systems/DynamicSystemPlantaBuilder.cs` | `src/RackCad.Application/Systems/Dynamic/DynamicSystemPlantaBuilder.cs` | Application, Plugin, Tests, UI.Tests |
| `DynamicFrontalPreviewGeometry` | `src/RackCad.Application/Systems/DynamicSystemPreviewGeometry.cs` | `src/RackCad.Application/Systems/Dynamic/DynamicSystemPreviewGeometry.cs` | _(sin consumidores)_ |
| `DynamicLateralPreviewGeometry` | `src/RackCad.Application/Systems/DynamicSystemPreviewGeometry.cs` | `src/RackCad.Application/Systems/Dynamic/DynamicSystemPreviewGeometry.cs` | _(sin consumidores)_ |
| `DynamicSystemPreviewGeometry` | `src/RackCad.Application/Systems/DynamicSystemPreviewGeometry.cs` | `src/RackCad.Application/Systems/Dynamic/DynamicSystemPreviewGeometry.cs` | Tests, UI |
| `DynamicViewDecorations` | `src/RackCad.Application/Systems/DynamicViewDecorations.cs` | `src/RackCad.Application/Systems/Dynamic/DynamicViewDecorations.cs` | Application |
| `SystemBomBuilder` | `src/RackCad.Application/Systems/SystemBomBuilder.cs` | `src/RackCad.Application/Systems/Dynamic/SystemBomBuilder.cs` | Application, Plugin, Tests, UI |

## `RackCad.Application.Systems.FlowBed`

| Tipo | Archivo origen | Archivo destino | Consumido por |
|---|---|---|---|
| `FlowBedBomBuilder` | `src/RackCad.Application/Systems/FlowBedBomBuilder.cs` | `src/RackCad.Application/Systems/FlowBed/FlowBedBomBuilder.cs` | Plugin, Tests, UI |
| `FlowBedLateralBuilder` | `src/RackCad.Application/Systems/FlowBedLateralBuilder.cs` | `src/RackCad.Application/Systems/FlowBed/FlowBedLateralBuilder.cs` | Application, Plugin, Tests, UI |

## `RackCad.Application.Systems.Larguero`

| Tipo | Archivo origen | Archivo destino | Consumido por |
|---|---|---|---|
| `LargueroBomBuilder` | `src/RackCad.Application/Systems/LargueroBomBuilder.cs` | `src/RackCad.Application/Systems/Larguero/LargueroBomBuilder.cs` | Application, Tests, UI |

## `RackCad.Application.Systems.PushBack`

| Tipo | Archivo origen | Archivo destino | Consumido por |
|---|---|---|---|
| `PushBackAdvancedRackParameters` | `src/RackCad.Application/Systems/PushBackAdvancedRackParameters.cs` | `src/RackCad.Application/Systems/PushBack/PushBackAdvancedRackParameters.cs` | Application, UI |
| `PushBackBedRotation` | `src/RackCad.Application/Systems/PushBackBedRotation.cs` | `src/RackCad.Application/Systems/PushBack/PushBackBedRotation.cs` | Application, Tests |
| `PushBackBedSlope` | `src/RackCad.Application/Systems/PushBackBedSlope.cs` | `src/RackCad.Application/Systems/PushBack/PushBackBedSlope.cs` | Application, Tests |
| `PushBackBomBuilder` | `src/RackCad.Application/Systems/PushBackBomBuilder.cs` | `src/RackCad.Application/Systems/PushBack/PushBackBomBuilder.cs` | Application, Plugin, Tests, UI.Tests |
| `PushBackEditorCell` | `src/RackCad.Application/Systems/PushBackEditorCell.cs` | `src/RackCad.Application/Systems/PushBack/PushBackEditorCell.cs` | Application, Tests |
| `PushBackEditorComputation` | `src/RackCad.Application/Systems/PushBackEditorComputation.cs` | `src/RackCad.Application/Systems/PushBack/PushBackEditorComputation.cs` | Application, Tests, UI |
| `PushBackEditorDesignAssembler` | `src/RackCad.Application/Systems/PushBackEditorDesignAssembler.cs` | `src/RackCad.Application/Systems/PushBack/PushBackEditorDesignAssembler.cs` | Application, Tests, UI |
| `PushBackEditorFront` | `src/RackCad.Application/Systems/PushBackEditorFront.cs` | `src/RackCad.Application/Systems/PushBack/PushBackEditorFront.cs` | Application |
| `PushBackEditorInputs` | `src/RackCad.Application/Systems/PushBackEditorInputs.cs` | `src/RackCad.Application/Systems/PushBack/PushBackEditorInputs.cs` | Application, Tests, UI |
| `PushBackEditorState` | `src/RackCad.Application/Systems/PushBackEditorState.Load.cs` | `src/RackCad.Application/Systems/PushBack/PushBackEditorState.Load.cs` | Application, Tests, UI, UI.Tests |
| `PushBackEditorState` | `src/RackCad.Application/Systems/PushBackEditorState.cs` | `src/RackCad.Application/Systems/PushBack/PushBackEditorState.cs` | Application, Tests, UI, UI.Tests |
| `PushBackEditorSnapshot` | `src/RackCad.Application/Systems/PushBackEditorState.cs` | `src/RackCad.Application/Systems/PushBack/PushBackEditorState.cs` | _(sin consumidores)_ |
| `PushBackEditorValues` | `src/RackCad.Application/Systems/PushBackEditorValues.cs` | `src/RackCad.Application/Systems/PushBack/PushBackEditorValues.cs` | Application, Tests, UI |
| `PushBackCellElevation` | `src/RackCad.Application/Systems/PushBackElevations.cs` | `src/RackCad.Application/Systems/PushBack/PushBackElevations.cs` | _(sin consumidores)_ |
| `PushBackElevations` | `src/RackCad.Application/Systems/PushBackElevations.cs` | `src/RackCad.Application/Systems/PushBack/PushBackElevations.cs` | Application, Tests |
| `PushBackFlowBedAxis` | `src/RackCad.Application/Systems/PushBackFlowBedGeometry.cs` | `src/RackCad.Application/Systems/PushBack/PushBackFlowBedGeometry.cs` | Application, Tests |
| `PushBackFlowBedGeometry` | `src/RackCad.Application/Systems/PushBackFlowBedGeometry.cs` | `src/RackCad.Application/Systems/PushBack/PushBackFlowBedGeometry.cs` | Application, Tests |
| `PushBackFlowBedLateralBuilder` | `src/RackCad.Application/Systems/PushBackFlowBedLateralBuilder.cs` | `src/RackCad.Application/Systems/PushBack/PushBackFlowBedLateralBuilder.cs` | Application, Tests |
| `PushBackHighEndBeamGeometry` | `src/RackCad.Application/Systems/PushBackHighEndBeamGeometry.cs` | `src/RackCad.Application/Systems/PushBack/PushBackHighEndBeamGeometry.cs` | Application, Tests |
| `PushBackIntermediateBeamLateralBuilder` | `src/RackCad.Application/Systems/PushBackIntermediateBeamLateralBuilder.cs` | `src/RackCad.Application/Systems/PushBack/PushBackIntermediateBeamLateralBuilder.cs` | Application |
| `PushBackLoadBeamGeometry` | `src/RackCad.Application/Systems/PushBackLoadBeamGeometry.cs` | `src/RackCad.Application/Systems/PushBack/PushBackLoadBeamGeometry.cs` | Application, Tests |
| `PushBackPlanComposer` | `src/RackCad.Application/Systems/PushBackPlanComposer.cs` | `src/RackCad.Application/Systems/PushBack/PushBackPlanComposer.cs` | Application |
| `PushBackRearTopeBuilder` | `src/RackCad.Application/Systems/PushBackRearTopeBuilder.cs` | `src/RackCad.Application/Systems/PushBack/PushBackRearTopeBuilder.cs` | Application, Tests, UI, UI.Tests |
| `PushBackResolver` | `src/RackCad.Application/Systems/PushBackResolver.cs` | `src/RackCad.Application/Systems/PushBack/PushBackResolver.cs` | Application, Plugin, Tests |
| `PushBackSafetyAuthority` | `src/RackCad.Application/Systems/PushBackSafetyAuthority.cs` | `src/RackCad.Application/Systems/PushBack/PushBackSafetyAuthority.cs` | Application, Tests, UI, UI.Tests |
| `PushBackFrontalEnd` | `src/RackCad.Application/Systems/PushBackSystemFrontalBuilder.cs` | `src/RackCad.Application/Systems/PushBack/PushBackSystemFrontalBuilder.cs` | Application, Plugin, Tests, UI, UI.Tests |
| `PushBackSystemFrontalBuilder` | `src/RackCad.Application/Systems/PushBackSystemFrontalBuilder.cs` | `src/RackCad.Application/Systems/PushBack/PushBackSystemFrontalBuilder.cs` | Application, Plugin, Tests, UI.Tests |
| `PushBackSystemLateralBuilder` | `src/RackCad.Application/Systems/PushBackSystemLateralBuilder.cs` | `src/RackCad.Application/Systems/PushBack/PushBackSystemLateralBuilder.cs` | Application, Plugin, Tests |
| `PushBackSystemPlantaBuilder` | `src/RackCad.Application/Systems/PushBackSystemPlantaBuilder.cs` | `src/RackCad.Application/Systems/PushBack/PushBackSystemPlantaBuilder.cs` | Application, Plugin, Tests |
| `PushBackTroquelGrid` | `src/RackCad.Application/Systems/PushBackTroquelGrid.cs` | `src/RackCad.Application/Systems/PushBack/PushBackTroquelGrid.cs` | Application, Tests |

## `RackCad.Application.Systems.Selective`

| Tipo | Archivo origen | Archivo destino | Consumido por |
|---|---|---|---|
| `SelectiveAnnotations` | `src/RackCad.Application/Systems/SelectiveAnnotations.cs` | `src/RackCad.Application/Systems/Selective/SelectiveAnnotations.cs` | Application |
| `SelectiveApplyScope` | `src/RackCad.Application/Systems/SelectiveApplyScope.cs` | `src/RackCad.Application/Systems/Selective/SelectiveApplyScope.cs` | Application, Tests, UI |
| `SelectiveBomBuilder` | `src/RackCad.Application/Systems/SelectiveBomBuilder.cs` | `src/RackCad.Application/Systems/Selective/SelectiveBomBuilder.cs` | Application, Plugin, Tests, UI, UI.Tests |
| `SelectiveDepthLayout` | `src/RackCad.Application/Systems/SelectiveDepthLayout.cs` | `src/RackCad.Application/Systems/Selective/SelectiveDepthLayout.cs` | Application, Plugin, Tests, UI, UI.Tests |
| `SelectiveDesignInputs` | `src/RackCad.Application/Systems/SelectiveDesignInputs.cs` | `src/RackCad.Application/Systems/Selective/SelectiveDesignInputs.cs` | Application, Tests, UI |
| `SelectiveDesviadorDrawing` | `src/RackCad.Application/Systems/SelectiveDesviadorDrawing.cs` | `src/RackCad.Application/Systems/Selective/SelectiveDesviadorDrawing.cs` | Application |
| `SelectiveDesviadorPlan` | `src/RackCad.Application/Systems/SelectiveDesviadorPlan.cs` | `src/RackCad.Application/Systems/Selective/SelectiveDesviadorPlan.cs` | Application, Tests, UI |
| `SelectiveDimensions` | `src/RackCad.Application/Systems/SelectiveDimensions.cs` | `src/RackCad.Application/Systems/Selective/SelectiveDimensions.cs` | Application |
| `SelectiveEditorCell` | `src/RackCad.Application/Systems/SelectiveEditorCell.cs` | `src/RackCad.Application/Systems/Selective/SelectiveEditorCell.cs` | Application, Tests, UI |
| `SelectiveEditorFondoMatrix` | `src/RackCad.Application/Systems/SelectiveEditorFondoMatrix.cs` | `src/RackCad.Application/Systems/Selective/SelectiveEditorFondoMatrix.cs` | Application, Tests, UI |
| `SelectiveEditorState` | `src/RackCad.Application/Systems/SelectiveEditorState.cs` | `src/RackCad.Application/Systems/Selective/SelectiveEditorState.cs` | Application, Tests, UI, UI.Tests |
| `SelectiveFrontalBuilder` | `src/RackCad.Application/Systems/SelectiveFrontalBuilder.cs` | `src/RackCad.Application/Systems/Selective/SelectiveFrontalBuilder.cs` | Application, Plugin, Tests, UI, UI.Tests |
| `SelectiveGeometryResolver` | `src/RackCad.Application/Systems/SelectiveGeometryResolver.cs` | `src/RackCad.Application/Systems/Selective/SelectiveGeometryResolver.cs` | Application, Domain, Plugin, Tests, UI, UI.Tests |
| `SelectiveLateralBuilder` | `src/RackCad.Application/Systems/SelectiveLateralBuilder.cs` | `src/RackCad.Application/Systems/Selective/SelectiveLateralBuilder.cs` | Application, Plugin, Tests, UI.Tests |
| `SelectiveCorte` | `src/RackCad.Application/Systems/SelectiveLateralBuilder.cs` | `src/RackCad.Application/Systems/Selective/SelectiveLateralBuilder.cs` | _(sin consumidores)_ |
| `SelectiveMedioFrente` | `src/RackCad.Application/Systems/SelectiveMedioFrente.cs` | `src/RackCad.Application/Systems/Selective/SelectiveMedioFrente.cs` | Application, Domain |
| `SelectiveParrillaPlacement` | `src/RackCad.Application/Systems/SelectiveParrillaPlacement.cs` | `src/RackCad.Application/Systems/Selective/SelectiveParrillaPlacement.cs` | Application, Tests |
| `SelectiveParrillaPlan` | `src/RackCad.Application/Systems/SelectiveParrillaPlan.cs` | `src/RackCad.Application/Systems/Selective/SelectiveParrillaPlan.cs` | Application, Tests, UI, UI.Tests |
| `SelectivePlantaBuilder` | `src/RackCad.Application/Systems/SelectivePlantaBuilder.cs` | `src/RackCad.Application/Systems/Selective/SelectivePlantaBuilder.cs` | Application, Plugin, Tests, UI.Tests |
| `SelectivePostGeometry` | `src/RackCad.Application/Systems/SelectivePostGeometry.cs` | `src/RackCad.Application/Systems/Selective/SelectivePostGeometry.cs` | Application, Tests, UI |
| `SelectivePostLayout` | `src/RackCad.Application/Systems/SelectivePostGeometry.cs` | `src/RackCad.Application/Systems/Selective/SelectivePostGeometry.cs` | Application |
| `SafetyEndCopy` | `src/RackCad.Application/Systems/SelectiveSafetyEnds.cs` | `src/RackCad.Application/Systems/Selective/SelectiveSafetyEnds.cs` | Application |
| `SelectiveSafetyEnds` | `src/RackCad.Application/Systems/SelectiveSafetyEnds.cs` | `src/RackCad.Application/Systems/Selective/SelectiveSafetyEnds.cs` | Application, Tests, UI.Tests |
| `SelectiveSafetyFamilies` | `src/RackCad.Application/Systems/SelectiveSafetyFamilies.cs` | `src/RackCad.Application/Systems/Selective/SelectiveSafetyFamilies.cs` | Application, Tests, UI, UI.Tests |
| `SelectiveSafetyGrid` | `src/RackCad.Application/Systems/SelectiveSafetyGrid.cs` | `src/RackCad.Application/Systems/Selective/SelectiveSafetyGrid.cs` | Application, Tests, UI, UI.Tests |
| `SelectiveSafetyPlacement` | `src/RackCad.Application/Systems/SelectiveSafetyPlacement.cs` | `src/RackCad.Application/Systems/Selective/SelectiveSafetyPlacement.cs` | Application, Tests |
| `SelectiveSeparadorPlacement` | `src/RackCad.Application/Systems/SelectiveSeparadorPlacement.cs` | `src/RackCad.Application/Systems/Selective/SelectiveSeparadorPlacement.cs` | Application, Tests |
| `SelectiveSeparadorPlan` | `src/RackCad.Application/Systems/SelectiveSeparadorPlan.cs` | `src/RackCad.Application/Systems/Selective/SelectiveSeparadorPlan.cs` | Application |
| `SelectiveTarimaPlacement` | `src/RackCad.Application/Systems/SelectiveTarimaPlacement.cs` | `src/RackCad.Application/Systems/Selective/SelectiveTarimaPlacement.cs` | Application, Tests |
| `SelectiveTopePlacement` | `src/RackCad.Application/Systems/SelectiveTopePlacement.cs` | `src/RackCad.Application/Systems/Selective/SelectiveTopePlacement.cs` | Application, Tests |
| `SelectiveTopePlan` | `src/RackCad.Application/Systems/SelectiveTopePlan.cs` | `src/RackCad.Application/Systems/Selective/SelectiveTopePlan.cs` | Application, Tests |

## `RackCad.Application.Systems.Shared`

| Tipo | Archivo origen | Archivo destino | Consumido por |
|---|---|---|---|
| `RackFrontLevelElevations` | `src/RackCad.Application/Systems/RackLevelElevations.cs` | `src/RackCad.Application/Systems/Shared/RackLevelElevations.cs` | Application, Tests |
| `RackLevelElevations` | `src/RackCad.Application/Systems/RackLevelElevations.cs` | `src/RackCad.Application/Systems/Shared/RackLevelElevations.cs` | Application, Tests |
| `RackLevelElevationsExtensions` | `src/RackCad.Application/Systems/RackLevelElevations.cs` | `src/RackCad.Application/Systems/Shared/RackLevelElevations.cs` | _(sin consumidores)_ |
| `RackModuleDescriptor` | `src/RackCad.Application/Systems/RackModuleDescriptor.cs` | `src/RackCad.Application/Systems/Shared/RackModuleDescriptor.cs` | Application, Tests, UI, UI.Tests |
| `RackModuleCommit` | `src/RackCad.Application/Systems/RackModuleEditSession.cs` | `src/RackCad.Application/Systems/Shared/RackModuleEditSession.cs` | Application |
| `RackModuleEditSession` | `src/RackCad.Application/Systems/RackModuleEditSession.cs` | `src/RackCad.Application/Systems/Shared/RackModuleEditSession.cs` | Application, Tests |
| `RackModuleReconciliationResult` | `src/RackCad.Application/Systems/RackModuleReconciliation.cs` | `src/RackCad.Application/Systems/Shared/RackModuleReconciliation.cs` | Application, Tests |
| `RackModuleReconciliation` | `src/RackCad.Application/Systems/RackModuleReconciliation.cs` | `src/RackCad.Application/Systems/Shared/RackModuleReconciliation.cs` | Application, Tests |
| `SafetyDormantCells` | `src/RackCad.Application/Systems/SafetyDormantCells.cs` | `src/RackCad.Application/Systems/Shared/SafetyDormantCells.cs` | Tests, UI |
| `SeparatorLevelCalculator` | `src/RackCad.Application/Systems/SeparatorLevelCalculator.cs` | `src/RackCad.Application/Systems/Shared/SeparatorLevelCalculator.cs` | Application, Tests |
| `SystemDescriptor` | `src/RackCad.Application/Systems/SystemDescriptor.cs` | `src/RackCad.Application/Systems/Shared/SystemDescriptor.cs` | Application, Tests |
| `SystemRegistry` | `src/RackCad.Application/Systems/SystemRegistry.Default.cs` | `src/RackCad.Application/Systems/Shared/SystemRegistry.Default.cs` | Application, Plugin, Tests, UI |
| `SystemRegistry` | `src/RackCad.Application/Systems/SystemRegistry.cs` | `src/RackCad.Application/Systems/Shared/SystemRegistry.cs` | Application, Plugin, Tests, UI |

## `RackCad.Domain.Systems.Dynamic`

| Tipo | Archivo origen | Archivo destino | Consumido por |
|---|---|---|---|
| `DynamicLoadBeamLevel` | `src/RackCad.Domain/Systems/DynamicLoadBeamLevel.cs` | `src/RackCad.Domain/Systems/Dynamic/DynamicLoadBeamLevel.cs` | Application, Domain, Tests, UI |
| `DynamicRackDefaults` | `src/RackCad.Domain/Systems/DynamicRackDefaults.cs` | `src/RackCad.Domain/Systems/Dynamic/DynamicRackDefaults.cs` | Application, Domain, Plugin, Tests, UI, UI.Tests |
| `DynamicRackDesign` | `src/RackCad.Domain/Systems/DynamicRackDesign.cs` | `src/RackCad.Domain/Systems/Dynamic/DynamicRackDesign.cs` | Application, Domain, Plugin, Tests, UI, UI.Tests |
| `DynamicRackModuleDesign` | `src/RackCad.Domain/Systems/DynamicRackDesign.cs` | `src/RackCad.Domain/Systems/Dynamic/DynamicRackDesign.cs` | Application, Tests |
| `DynamicRackFrontDesign` | `src/RackCad.Domain/Systems/DynamicRackFront.cs` | `src/RackCad.Domain/Systems/Dynamic/DynamicRackFront.cs` | Application, Domain, Tests, UI, UI.Tests |
| `DynamicRackFront` | `src/RackCad.Domain/Systems/DynamicRackFront.cs` | `src/RackCad.Domain/Systems/Dynamic/DynamicRackFront.cs` | Application, Domain, Tests, UI |
| `DynamicRackLevelDesign` | `src/RackCad.Domain/Systems/DynamicRackFront.cs` | `src/RackCad.Domain/Systems/Dynamic/DynamicRackFront.cs` | Application, Tests |
| `DynamicRackLevel` | `src/RackCad.Domain/Systems/DynamicRackFront.cs` | `src/RackCad.Domain/Systems/Dynamic/DynamicRackFront.cs` | Application, Tests |
| `DynamicRackEnd` | `src/RackCad.Domain/Systems/DynamicRackFront.cs` | `src/RackCad.Domain/Systems/Dynamic/DynamicRackFront.cs` | Application, Plugin, Tests, UI, UI.Tests |
| `DynamicRackModule` | `src/RackCad.Domain/Systems/DynamicRackModule.cs` | `src/RackCad.Domain/Systems/Dynamic/DynamicRackModule.cs` | Application, Domain, Tests, UI |
| `DynamicRackModuleKind` | `src/RackCad.Domain/Systems/DynamicRackModuleKind.cs` | `src/RackCad.Domain/Systems/Dynamic/DynamicRackModuleKind.cs` | Application, Domain, Tests, UI |
| `DynamicRackSystem` | `src/RackCad.Domain/Systems/DynamicRackSystem.cs` | `src/RackCad.Domain/Systems/Dynamic/DynamicRackSystem.cs` | Application, Domain, Plugin, Tests, UI, UI.Tests |

## `RackCad.Domain.Systems.FlowBed`

| Tipo | Archivo origen | Archivo destino | Consumido por |
|---|---|---|---|
| `FlowBedConfiguration` | `src/RackCad.Domain/Systems/FlowBedConfiguration.cs` | `src/RackCad.Domain/Systems/FlowBed/FlowBedConfiguration.cs` | Application, Plugin, Tests, UI, UI.Tests |
| `FlowBedDefaults` | `src/RackCad.Domain/Systems/FlowBedDefaults.cs` | `src/RackCad.Domain/Systems/FlowBed/FlowBedDefaults.cs` | Application, Domain, Plugin, Tests, UI, UI.Tests |
| `FlowBedType` | `src/RackCad.Domain/Systems/FlowBedType.cs` | `src/RackCad.Domain/Systems/FlowBed/FlowBedType.cs` | Application, Domain, Plugin, Tests, UI, UI.Tests |

## `RackCad.Domain.Systems.Larguero`

| Tipo | Archivo origen | Archivo destino | Consumido por |
|---|---|---|---|
| `LargueroDesign` | `src/RackCad.Domain/Systems/LargueroDesign.cs` | `src/RackCad.Domain/Systems/Larguero/LargueroDesign.cs` | Application, Tests, UI, UI.Tests |

## `RackCad.Domain.Systems.PushBack`

| Tipo | Archivo origen | Archivo destino | Consumido por |
|---|---|---|---|
| `PushBackDefaults` | `src/RackCad.Domain/Systems/PushBackDefaults.cs` | `src/RackCad.Domain/Systems/PushBack/PushBackDefaults.cs` | Application, Domain, Tests, UI, UI.Tests |
| `PushBackDesign` | `src/RackCad.Domain/Systems/PushBackDesign.cs` | `src/RackCad.Domain/Systems/PushBack/PushBackDesign.cs` | Application, Plugin, Tests, UI, UI.Tests |
| `PushBackFrontConfig` | `src/RackCad.Domain/Systems/PushBackDesign.cs` | `src/RackCad.Domain/Systems/PushBack/PushBackDesign.cs` | Application, Tests, UI.Tests |
| `PushBackRearTopeConfig` | `src/RackCad.Domain/Systems/PushBackRearTope.cs` | `src/RackCad.Domain/Systems/PushBack/PushBackRearTope.cs` | Application, Domain, Tests, UI, UI.Tests |
| `PushBackSystem` | `src/RackCad.Domain/Systems/PushBackSystem.cs` | `src/RackCad.Domain/Systems/PushBack/PushBackSystem.cs` | Application, Plugin, Tests, UI, UI.Tests |
| `PushBackResolvedFront` | `src/RackCad.Domain/Systems/PushBackSystem.cs` | `src/RackCad.Domain/Systems/PushBack/PushBackSystem.cs` | Application |

## `RackCad.Domain.Systems.Selective`

| Tipo | Archivo origen | Archivo destino | Consumido por |
|---|---|---|---|
| `SelectivePalletDesign` | `src/RackCad.Domain/Systems/SelectivePalletDesign.cs` | `src/RackCad.Domain/Systems/Selective/SelectivePalletDesign.cs` | Application, Plugin, Tests, UI, UI.Tests |
| `SafetySide` | `src/RackCad.Domain/Systems/SelectivePalletDesign.cs` | `src/RackCad.Domain/Systems/Selective/SelectivePalletDesign.cs` | Application, Tests, UI, UI.Tests |
| `SelectiveSafetySelection` | `src/RackCad.Domain/Systems/SelectivePalletDesign.cs` | `src/RackCad.Domain/Systems/Selective/SelectivePalletDesign.cs` | Application, Domain, Tests, UI, UI.Tests |
| `SafetyPostSide` | `src/RackCad.Domain/Systems/SelectivePalletDesign.cs` | `src/RackCad.Domain/Systems/Selective/SelectivePalletDesign.cs` | Application, Tests, UI, UI.Tests |
| `SafetyPostDefense` | `src/RackCad.Domain/Systems/SelectivePalletDesign.cs` | `src/RackCad.Domain/Systems/Selective/SelectivePalletDesign.cs` | Application, Domain, Tests, UI, UI.Tests |
| `SelectiveGridCell` | `src/RackCad.Domain/Systems/SelectivePalletDesign.cs` | `src/RackCad.Domain/Systems/Selective/SelectivePalletDesign.cs` | Application, Domain, Tests, UI, UI.Tests |
| `SelectiveBayDesign` | `src/RackCad.Domain/Systems/SelectivePalletDesign.cs` | `src/RackCad.Domain/Systems/Selective/SelectivePalletDesign.cs` | Application, Tests, UI, UI.Tests |
| `SelectiveSegment` | `src/RackCad.Domain/Systems/SelectivePalletDesign.cs` | `src/RackCad.Domain/Systems/Selective/SelectivePalletDesign.cs` | Application, Domain, Tests, UI, UI.Tests |
| `SelectiveCell` | `src/RackCad.Domain/Systems/SelectivePalletDesign.cs` | `src/RackCad.Domain/Systems/Selective/SelectivePalletDesign.cs` | Application, Tests, UI.Tests |
| `Tarima` | `src/RackCad.Domain/Systems/SelectivePalletDesign.cs` | `src/RackCad.Domain/Systems/Selective/SelectivePalletDesign.cs` | Application, Tests, UI, UI.Tests |
| `SelectiveRackDefaults` | `src/RackCad.Domain/Systems/SelectiveRackDefaults.cs` | `src/RackCad.Domain/Systems/Selective/SelectiveRackDefaults.cs` | Application, Domain, Plugin, Tests, UI |
| `SelectiveSafetyDefaults` | `src/RackCad.Domain/Systems/SelectiveRackDefaults.cs` | `src/RackCad.Domain/Systems/Selective/SelectiveRackDefaults.cs` | Application, Domain, Tests, UI, UI.Tests |
| `SelectiveRackSystem` | `src/RackCad.Domain/Systems/SelectiveRackSystem.cs` | `src/RackCad.Domain/Systems/Selective/SelectiveRackSystem.cs` | Application, Plugin, Tests, UI, UI.Tests |
| `SelectiveBay` | `src/RackCad.Domain/Systems/SelectiveRackSystem.cs` | `src/RackCad.Domain/Systems/Selective/SelectiveRackSystem.cs` | Application, Tests |
| `SelectiveLevel` | `src/RackCad.Domain/Systems/SelectiveRackSystem.cs` | `src/RackCad.Domain/Systems/Selective/SelectiveRackSystem.cs` | Application, Tests |
| `SelectiveTopeConfig` | `src/RackCad.Domain/Systems/SelectiveSafetyConfig.cs` | `src/RackCad.Domain/Systems/Selective/SelectiveSafetyConfig.cs` | Application, Domain, Tests |
| `SelectiveDesviadorConfig` | `src/RackCad.Domain/Systems/SelectiveSafetyConfig.cs` | `src/RackCad.Domain/Systems/Selective/SelectiveSafetyConfig.cs` | Application, Domain, Tests |
| `SelectiveDefensaConfig` | `src/RackCad.Domain/Systems/SelectiveSafetyConfig.cs` | `src/RackCad.Domain/Systems/Selective/SelectiveSafetyConfig.cs` | Application, Domain, Tests |
| `SelectiveGuiaConfig` | `src/RackCad.Domain/Systems/SelectiveSafetyConfig.cs` | `src/RackCad.Domain/Systems/Selective/SelectiveSafetyConfig.cs` | Application, Domain, Tests |
| `SelectiveParrillaConfig` | `src/RackCad.Domain/Systems/SelectiveSafetyConfig.cs` | `src/RackCad.Domain/Systems/Selective/SelectiveSafetyConfig.cs` | Application, Domain, Tests |
| `SelectiveSafetyCells` | `src/RackCad.Domain/Systems/SelectiveSafetyConfig.cs` | `src/RackCad.Domain/Systems/Selective/SelectiveSafetyConfig.cs` | Domain |

## `RackCad.Domain.Systems.Shared`

| Tipo | Archivo origen | Archivo destino | Consumido por |
|---|---|---|---|
| `DimensionDetail` | `src/RackCad.Domain/Systems/DimensionDetail.cs` | `src/RackCad.Domain/Systems/Shared/DimensionDetail.cs` | Application, Domain, Tests, UI, UI.Tests |
| `PalletSpecification` | `src/RackCad.Domain/Systems/PalletSpecification.cs` | `src/RackCad.Domain/Systems/Shared/PalletSpecification.cs` | Application, Domain, Plugin, Tests, UI, UI.Tests |
| `RackSystemKind` | `src/RackCad.Domain/Systems/RackSystemKind.cs` | `src/RackCad.Domain/Systems/Shared/RackSystemKind.cs` | Application, Domain, Plugin, Tests, UI, UI.Tests |

## `RackCad.Plugin.Systems.Dynamic`

| Tipo | Archivo origen | Archivo destino | Consumido por |
|---|---|---|---|
| `DynamicFrontalDrawService` | `src/RackCad.Plugin/Systems/DynamicFrontalDrawService.cs` | `src/RackCad.Plugin/Systems/Dynamic/DynamicFrontalDrawService.cs` | Plugin, Tests |
| `DynamicPlantaDrawService` | `src/RackCad.Plugin/Systems/DynamicPlantaDrawService.cs` | `src/RackCad.Plugin/Systems/Dynamic/DynamicPlantaDrawService.cs` | Plugin, Tests |
| `DynamicSystemDrawService` | `src/RackCad.Plugin/Systems/DynamicSystemDrawService.cs` | `src/RackCad.Plugin/Systems/Dynamic/DynamicSystemDrawService.cs` | Plugin, Tests |

## `RackCad.Plugin.Systems.FlowBed`

| Tipo | Archivo origen | Archivo destino | Consumido por |
|---|---|---|---|
| `FlowBedDrawService` | `src/RackCad.Plugin/Systems/FlowBedDrawService.cs` | `src/RackCad.Plugin/Systems/FlowBed/FlowBedDrawService.cs` | Plugin, Tests |

## `RackCad.Plugin.Systems.PushBack`

| Tipo | Archivo origen | Archivo destino | Consumido por |
|---|---|---|---|
| `PushBackFrontalDrawService` | `src/RackCad.Plugin/Systems/PushBackFrontalDrawService.cs` | `src/RackCad.Plugin/Systems/PushBack/PushBackFrontalDrawService.cs` | Plugin, Tests |
| `PushBackPlantaDrawService` | `src/RackCad.Plugin/Systems/PushBackPlantaDrawService.cs` | `src/RackCad.Plugin/Systems/PushBack/PushBackPlantaDrawService.cs` | Plugin, Tests |
| `PushBackSystemDrawService` | `src/RackCad.Plugin/Systems/PushBackSystemDrawService.cs` | `src/RackCad.Plugin/Systems/PushBack/PushBackSystemDrawService.cs` | Plugin, Tests |

## `RackCad.Plugin.Systems.Selective`

| Tipo | Archivo origen | Archivo destino | Consumido por |
|---|---|---|---|
| `SelectiveFrontalDrawService` | `src/RackCad.Plugin/Systems/SelectiveFrontalDrawService.cs` | `src/RackCad.Plugin/Systems/Selective/SelectiveFrontalDrawService.cs` | Plugin |
| `SelectivePlantaDrawService` | `src/RackCad.Plugin/Systems/SelectivePlantaDrawService.cs` | `src/RackCad.Plugin/Systems/Selective/SelectivePlantaDrawService.cs` | Plugin |

## `RackCad.Plugin.Systems.Shared`

| Tipo | Archivo origen | Archivo destino | Consumido por |
|---|---|---|---|
| `RackBlockData` | `src/RackCad.Plugin/Systems/RackBlockData.cs` | `src/RackCad.Plugin/Systems/Shared/RackBlockData.cs` | Plugin |
| `SystemBlockWriter` | `src/RackCad.Plugin/Systems/SystemBlockWriter.cs` | `src/RackCad.Plugin/Systems/Shared/SystemBlockWriter.cs` | Plugin |
| `ViewBlockDraw` | `src/RackCad.Plugin/Systems/ViewBlockDraw.cs` | `src/RackCad.Plugin/Systems/Shared/ViewBlockDraw.cs` | Plugin, Tests |

## Resumen

| Namespace destino | Archivos | Tipos de primer nivel |
|---|---|---|
| `RackCad.Application.Drawing` | 4 | 7 |
| `RackCad.Application.RackFrames` | 4 | 4 |
| `RackCad.Application.Systems.Dynamic` | 34 | 50 |
| `RackCad.Application.Systems.FlowBed` | 2 | 2 |
| `RackCad.Application.Systems.Larguero` | 1 | 1 |
| `RackCad.Application.Systems.PushBack` | 26 | 30 |
| `RackCad.Application.Systems.Selective` | 28 | 31 |
| `RackCad.Application.Systems.Shared` | 9 | 13 |
| `RackCad.Domain.Systems.Dynamic` | 7 | 12 |
| `RackCad.Domain.Systems.FlowBed` | 3 | 3 |
| `RackCad.Domain.Systems.Larguero` | 1 | 1 |
| `RackCad.Domain.Systems.PushBack` | 4 | 6 |
| `RackCad.Domain.Systems.Selective` | 4 | 21 |
| `RackCad.Domain.Systems.Shared` | 3 | 3 |
| `RackCad.Plugin.Systems.Dynamic` | 3 | 3 |
| `RackCad.Plugin.Systems.FlowBed` | 1 | 1 |
| `RackCad.Plugin.Systems.PushBack` | 3 | 3 |
| `RackCad.Plugin.Systems.Selective` | 2 | 2 |
| `RackCad.Plugin.Systems.Shared` | 3 | 3 |
| **Total** | **142** | **196** |

## Renombre fosil autorizado

| Antes | Despues | Motivo |
|---|---|---|
| `RackCad.Application.Systems.DynamicSystemPlan` | `RackCad.Application.Drawing.HeaderRunPlan` | No es el plan del sistema dinamico: es el plan de corridas de cabecera y lo consumen los cuatro sistemas. El nombre `SystemPlan` que anota el ROADMAP es ambiguo en el arbol actual (colisiona con `SystemBomBuilder`, `SystemDescriptor`, `SystemRegistry` y `SystemBlockWriter`, que si son por sistema). |

Sus dos tipos acompañantes, `HeaderGroup` y `HeaderPlacement`, viajan con el en el mismo archivo y **conservan su nombre**.

