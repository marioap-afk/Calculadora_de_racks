using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using RackCad.Application.Persistence;
using RackCad.Domain.RackFrames;
using RackCad.Domain.Systems.Dynamic;
using RackCad.Domain.Systems.PushBack;
using RackCad.Domain.Systems.Cantilever;

namespace RackCad.Application.Systems.Shared
{
    // Explicit typed value operations for the four AUTH-13 trees. Schema is normalized before comparison.
    internal static class AuthoredTypedValues
    {
        internal static bool Same(DynamicRackSystemDocument a, DynamicRackSystemDocument b)
            => a is null ? b is null : b is not null
                && a.PalletFront == b.PalletFront
                && a.PalletDepth == b.PalletDepth
                && a.PalletHeight == b.PalletHeight
                && a.PalletWeight == b.PalletWeight
                && a.PalletWeightUnit == b.PalletWeightUnit
                && a.PalletsDeep == b.PalletsDeep
                && a.LoadLevels == b.LoadLevels
                && a.FirstLevelHeight == b.FirstLevelHeight
                && a.FirstLevelDatum == b.FirstLevelDatum
                && a.BeamDepth == b.BeamDepth
                && Sequence(a.IntermediateBeamDepths, b.IntermediateBeamDepths, (x, y) => x == y)
                && a.PalletTolerance == b.PalletTolerance
                && a.InOutBeamCatalogId == b.InOutBeamCatalogId
                && a.HeaderPostCatalogId == b.HeaderPostCatalogId
                && a.PostPeralte == b.PostPeralte
                && a.SeparatorCountOverride == b.SeparatorCountOverride
                && a.SeparatorSpacingOverride == b.SeparatorSpacingOverride
                && a.DerivedPostReinforced == b.DerivedPostReinforced
                && a.DerivedPostReinforcementHeight == b.DerivedPostReinforcementHeight
                && a.DerivedPostHeight == b.DerivedPostHeight
                && Sequence(a.HeaderLineOverrides, b.HeaderLineOverrides, Same)
                && Sequence(a.DerivedPostLineOverrides, b.DerivedPostLineOverrides, Same)
                && a.ManualHeaderHeightOverride == b.ManualHeaderHeightOverride
                && a.NumberFronts == b.NumberFronts
                && a.NumberLevels == b.NumberLevels
                && a.DrawRackName == b.DrawRackName
                && a.AnnotationScale == b.AnnotationScale
                && a.Dimensions == b.Dimensions
                && a.DimensionViews == b.DimensionViews
                && a.DimensionStyle == b.DimensionStyle
                && Sequence(a.SafetySelections, b.SafetySelections, Same)
                && Sequence(a.Fronts, b.Fronts, Same)
                && Sequence(a.Modules, b.Modules, Same)
                ;

        internal static bool Same(DynamicHeaderLineOverrideDocument a, DynamicHeaderLineOverrideDocument b)
            => a is null ? b is null : b is not null
                && a.PostIndex == b.PostIndex
                && a.ModuleId == b.ModuleId
                && Same(a.Header, b.Header)
                ;

        internal static bool Same(RackFrameProjectDocument a, RackFrameProjectDocument b)
            => a is null ? b is null : b is not null
                && a.SchemaVersion == b.SchemaVersion
                && a.Name == b.Name
                && a.Units == b.Units
                && a.Height == b.Height
                && a.Depth == b.Depth
                && a.PostPeralte == b.PostPeralte
                && a.CelosiaStartTroquel == b.CelosiaStartTroquel
                && a.DiagonalStartOffsetTroqueles == b.DiagonalStartOffsetTroqueles
                && a.DiagonalEndOffsetTroqueles == b.DiagonalEndOffsetTroqueles
                && a.DiagonalDoubleSpacingTroqueles == b.DiagonalDoubleSpacingTroqueles
                && a.HorizontalDoubleOffsetTroqueles == b.HorizontalDoubleOffsetTroqueles
                && a.PasoTroquel == b.PasoTroquel
                && a.PanelClear == b.PanelClear
                && a.StandardBaselineId == b.StandardBaselineId
                && a.StandardBaselineVersion == b.StandardBaselineVersion
                && Same(a.LeftPost, b.LeftPost)
                && Same(a.RightPost, b.RightPost)
                && Same(a.LeftBasePlate, b.LeftBasePlate)
                && Same(a.RightBasePlate, b.RightBasePlate)
                && Sequence(a.Horizontals, b.Horizontals, Same)
                && Sequence(a.Panels, b.Panels, Same)
                ;

        internal static bool Same(PostDocument a, PostDocument b)
            => a is null ? b is null : b is not null
                && a.PostCatalogId == b.PostCatalogId
                && a.Description == b.Description
                && a.HasReinforcement == b.HasReinforcement
                && a.ReinforcementCatalogId == b.ReinforcementCatalogId
                && a.ReinforcementHeight == b.ReinforcementHeight
                ;

        internal static bool Same(PlateDocument a, PlateDocument b)
            => a is null ? b is null : b is not null
                && a.PlateCatalogId == b.PlateCatalogId
                && a.Description == b.Description
                && a.ConnectionPointId == b.ConnectionPointId
                && a.PeralteOverride == b.PeralteOverride
                ;

        internal static bool Same(HorizontalDocument a, HorizontalDocument b)
            => a is null ? b is null : b is not null
                && a.Id == b.Id
                && a.Number == b.Number
                && a.Elevation == b.Elevation
                && a.ProfileId == b.ProfileId
                && a.Quantity == b.Quantity
                && a.MountingFace == b.MountingFace
                && a.State == b.State
                && a.Notes == b.Notes
                && a.IsStandard == b.IsStandard
                ;

        internal static bool Same(PanelDocument a, PanelDocument b)
            => a is null ? b is null : b is not null
                && a.PanelId == b.PanelId
                && a.Number == b.Number
                && a.LowerHorizontalId == b.LowerHorizontalId
                && a.UpperHorizontalId == b.UpperHorizontalId
                && a.Arrangement == b.Arrangement
                && a.MountingFace == b.MountingFace
                && a.DiagonalProfileId == b.DiagonalProfileId
                && a.DiagonalDirection == b.DiagonalDirection
                && a.StartConnectionPointId == b.StartConnectionPointId
                && a.EndConnectionPointId == b.EndConnectionPointId
                && a.IsStandard == b.IsStandard
                && a.IsException == b.IsException
                ;

        internal static bool Same(DynamicDerivedPostLineOverrideDocument a, DynamicDerivedPostLineOverrideDocument b)
            => a is null ? b is null : b is not null
                && a.PostIndex == b.PostIndex
                && a.Height == b.Height
                ;

        internal static bool Same(SafetySelectionDocument a, SafetySelectionDocument b)
            => a is null ? b is null : b is not null
                && a.ElementId == b.ElementId
                && a.Quantity == b.Quantity
                && a.Side == b.Side
                && Sequence(a.PostSides, b.PostSides, Same)
                && a.TopeShared == b.TopeShared
                && a.TopeSaque == b.TopeSaque
                && a.TopeFrontal == b.TopeFrontal
                && a.TopeFondo == b.TopeFondo
                && Sequence(a.TopeOffCells, b.TopeOffCells, Same)
                && a.DesviadorLongitud == b.DesviadorLongitud
                && a.DesviadorPrimerNivelAltura == b.DesviadorPrimerNivelAltura
                && Sequence(a.DesviadorOffCells, b.DesviadorOffCells, Same)
                && Sequence(a.DefensaPosts, b.DefensaPosts, Same)
                && Sequence(a.GuiaEntradaOffCells, b.GuiaEntradaOffCells, Same)
                && a.ParrillaFrontal == b.ParrillaFrontal
                && a.ParrillaLateral == b.ParrillaLateral
                && a.ParrillaFrente == b.ParrillaFrente
                && a.ParrillaCantidad == b.ParrillaCantidad
                && Sequence(a.ParrillaOffCells, b.ParrillaOffCells, Same)
                && a.BotaPlacement == b.BotaPlacement
                && Sequence(a.BotaPosts, b.BotaPosts, Same)
                && a.BotaBPlacement == b.BotaBPlacement
                && Sequence(a.BotaBPosts, b.BotaBPosts, Same)
                && a.BotaSidesDeclared == b.BotaSidesDeclared
                && a.BotaPieceId == b.BotaPieceId
                && a.BotaBPieceId == b.BotaBPieceId
                && a.AuthoredSide == b.AuthoredSide
                && Sequence(a.DerivedAisles, b.DerivedAisles, Same)
                ;

        internal static bool Same(PostSideDocument a, PostSideDocument b)
            => a is null ? b is null : b is not null
                && a.PostIndex == b.PostIndex
                && a.Side == b.Side
                ;

        internal static bool Same(GridCellDocument a, GridCellDocument b)
            => a is null ? b is null : b is not null
                && a.Frente == b.Frente
                && a.Level == b.Level
                ;

        internal static bool Same(PostDefenseDocument a, PostDefenseDocument b)
            => a is null ? b is null : b is not null
                && a.PostIndex == b.PostIndex
                && a.ExitLength == b.ExitLength
                && a.EntranceLength == b.EntranceLength
                && a.ExitAuto == b.ExitAuto
                && a.EntranceAuto == b.EntranceAuto
                ;

        internal static bool Same(BootPostDocument a, BootPostDocument b)
            => a is null ? b is null : b is not null
                && a.PostIndex == b.PostIndex
                && a.Placement == b.Placement
                ;

        internal static bool Same(DynamicRackFrontDocument a, DynamicRackFrontDocument b)
            => a is null ? b is null : b is not null
                && a.IsActive == b.IsActive
                && a.PalletCount == b.PalletCount
                && a.LoadLevels == b.LoadLevels
                && a.PalletsDeep == b.PalletsDeep
                && a.DepthStartPosition == b.DepthStartPosition
                && a.BeamLengthOverride == b.BeamLengthOverride
                && a.FirstLevelHeight == b.FirstLevelHeight
                && a.Bfr == b.Bfr
                && Sequence(a.IntermediateBeamDepths, b.IntermediateBeamDepths, (x, y) => x == y)
                && Sequence(a.Levels, b.Levels, Same)
                ;

        internal static bool Same(DynamicRackLevelDocument a, DynamicRackLevelDocument b)
            => a is null ? b is null : b is not null
                && a.PalletFront == b.PalletFront
                && a.PalletHeight == b.PalletHeight
                && a.PalletWeight == b.PalletWeight
                && a.ClearHeight == b.ClearHeight
                && a.InOutBeamCatalogId == b.InOutBeamCatalogId
                && a.InOutBeamDepth == b.InOutBeamDepth
                && a.BeamLengthOverride == b.BeamLengthOverride
                && a.IntermediateBeamCatalogId == b.IntermediateBeamCatalogId
                && a.IntermediateBeamDepth == b.IntermediateBeamDepth
                ;

        internal static bool Same(DynamicRackModuleDocument a, DynamicRackModuleDocument b)
            => a is null ? b is null : b is not null
                && a.ModuleId == b.ModuleId
                && a.Kind == b.Kind
                && a.Length == b.Length
                && a.IsCalculated == b.IsCalculated
                && a.IsManualOverride == b.IsManualOverride
                && a.UseCalculatedHeaderConfiguration == b.UseCalculatedHeaderConfiguration
                && a.Notes == b.Notes
                && Same(a.Header, b.Header)
                ;

        internal static bool Same(PushBackDesignDocument a, PushBackDesignDocument b)
            => a is null ? b is null : b is not null
                && a.SchemaVersion == b.SchemaVersion
                && Same(a.Structure, b.Structure)
                && Sequence(a.Fronts, b.Fronts, Same)
                && a.LegacyHighEndBeamPeralte == b.LegacyHighEndBeamPeralte
                && a.RearTopeSaque == b.RearTopeSaque
                && a.RearTopePieceId == b.RearTopePieceId
                && a.DefensePieceId == b.DefensePieceId
                && Sequence(a.RearTopeOffCells, b.RearTopeOffCells, Same)
                && Same(a.SideB, b.SideB)
                && Same(a.Composite, b.Composite)
                ;

        internal static bool Same(PushBackFrontDocument a, PushBackFrontDocument b)
            => a is null ? b is null : b is not null
                && Sequence(a.HighEndBeamPeraltes, b.HighEndBeamPeraltes, (x, y) => x == y)
                && a.DefaultPalletsDeep == b.DefaultPalletsDeep
                && Sequence(a.PalletsDeepOverrides, b.PalletsDeepOverrides, (x, y) => x == y)
                && Sequence(a.DrawPallets, b.DrawPallets, (x, y) => x == y)
                ;

        internal static bool Same(PushBackCellDocument a, PushBackCellDocument b)
            => a is null ? b is null : b is not null
                && a.Frente == b.Frente
                && a.Level == b.Level
                ;

        internal static bool Same(PushBackSideDocument a, PushBackSideDocument b)
            => a is null ? b is null : b is not null
                && a.IsPresent == b.IsPresent
                && a.LoadLevels == b.LoadLevels
                && a.FirstLevelHeight == b.FirstLevelHeight
                && a.LegacyHighEndBeamPeralte == b.LegacyHighEndBeamPeralte
                && Sequence(a.Fronts, b.Fronts, Same)
                && Sequence(a.FrontConfigs, b.FrontConfigs, Same)
                && a.RearTopeSaque == b.RearTopeSaque
                && a.RearTopePieceId == b.RearTopePieceId
                && a.DefensePieceId == b.DefensePieceId
                && Sequence(a.RearTopeOffCells, b.RearTopeOffCells, Same)
                ;

        internal static bool Same(PushBackCompositeDocument a, PushBackCompositeDocument b)
            => a is null ? b is null : b is not null
                && a.Gap == b.Gap
                && a.CentralSeparator == b.CentralSeparator
                && a.StructureOverrideA == b.StructureOverrideA
                && a.StructureOverrideB == b.StructureOverrideB
                && a.DefaultTopology == b.DefaultTopology
                && a.DefaultDirection == b.DefaultDirection
                && Sequence(a.Topologies, b.Topologies, Same)
                && Sequence(a.AbsentSlotsA, b.AbsentSlotsA, (x, y) => x == y)
                && Sequence(a.AbsentSlotsB, b.AbsentSlotsB, (x, y) => x == y)
                ;

        internal static bool Same(PushBackTopologyCellDocument a, PushBackTopologyCellDocument b)
            => a is null ? b is null : b is not null
                && a.Frente == b.Frente
                && a.Level == b.Level
                && a.Topology == b.Topology
                && a.Direction == b.Direction
                && a.CorridaDepth == b.CorridaDepth
                ;

        internal static bool Same(CantileverLineDocument a, CantileverLineDocument b)
            => a is null ? b is null : b is not null
                && a.SchemaVersion == b.SchemaVersion
                && Same(a.Line, b.Line)
                ;

        internal static CantileverLineDocument Copy(CantileverLineDocument a)
            => a is null ? null : new CantileverLineDocument
            {
                SchemaVersion = a.SchemaVersion,
                Line = Copy(a.Line),
            };

        internal static bool Same(CantileverLineDesign a, CantileverLineDesign b)
            => a is null ? b is null : b is not null
                && a.Id == b.Id
                && a.Name == b.Name
                && a.StationCount == b.StationCount
                && a.ColumnCentreSpacing == b.ColumnCentreSpacing
                && Same(a.StationTopology, b.StationTopology)
                && Same(a.DefaultArmTemplate, b.DefaultArmTemplate)
                && Sequence(a.ArmCellOverrides, b.ArmCellOverrides, Same)
                && Same(a.Bracing, b.Bracing)
                && Same(a.PlantaVisibility, b.PlantaVisibility)
                ;

        internal static CantileverLineDesign Copy(CantileverLineDesign a)
            => a is null ? null : new CantileverLineDesign
            {
                Id = a.Id,
                Name = a.Name,
                StationCount = a.StationCount,
                ColumnCentreSpacing = a.ColumnCentreSpacing,
                StationTopology = Copy(a.StationTopology),
                DefaultArmTemplate = Copy(a.DefaultArmTemplate),
                ArmCellOverrides = a.ArmCellOverrides?.Select(Copy).ToList(),
                Bracing = Copy(a.Bracing),
                PlantaVisibility = Copy(a.PlantaVisibility),
            };

        internal static bool Same(CantileverLineStationTopologyDesign a, CantileverLineStationTopologyDesign b)
            => a is null ? b is null : b is not null
                && a.FaceMode == b.FaceMode
                && a.SingleSide == b.SingleSide
                && Same(a.ColumnBaseTemplate, b.ColumnBaseTemplate)
                && a.LevelCount == b.LevelCount
                && a.FirstLevelPunchIndex == b.FirstLevelPunchIndex
                && a.RequestedClearHeight == b.RequestedClearHeight
                && a.TopClearFactor == b.TopClearFactor
                && Same(a.ColumnHeight, b.ColumnHeight)
                ;

        internal static CantileverLineStationTopologyDesign Copy(CantileverLineStationTopologyDesign a)
            => a is null ? null : new CantileverLineStationTopologyDesign
            {
                FaceMode = a.FaceMode,
                SingleSide = a.SingleSide,
                ColumnBaseTemplate = Copy(a.ColumnBaseTemplate),
                LevelCount = a.LevelCount,
                FirstLevelPunchIndex = a.FirstLevelPunchIndex,
                RequestedClearHeight = a.RequestedClearHeight,
                TopClearFactor = a.TopClearFactor,
                ColumnHeight = Copy(a.ColumnHeight),
            };

        internal static bool Same(CantileverStationColumnBaseTemplateDesign a, CantileverStationColumnBaseTemplateDesign b)
            => a is null ? b is null : b is not null
                && a.ColumnSectionId == b.ColumnSectionId
                && Same(a.ColumnBottomPlate, b.ColumnBottomPlate)
                && Same(a.Base, b.Base)
                && Same(a.Connection, b.Connection)
                && a.BaseFollowsColumn == b.BaseFollowsColumn
                ;

        internal static CantileverStationColumnBaseTemplateDesign Copy(CantileverStationColumnBaseTemplateDesign a)
            => a is null ? null : new CantileverStationColumnBaseTemplateDesign
            {
                ColumnSectionId = a.ColumnSectionId,
                ColumnBottomPlate = Copy(a.ColumnBottomPlate),
                Base = Copy(a.Base),
                Connection = Copy(a.Connection),
                BaseFollowsColumn = a.BaseFollowsColumn,
            };

        internal static bool Same(CantileverPlateDesign a, CantileverPlateDesign b)
            => a is null ? b is null : b is not null
                && a.Thickness == b.Thickness
                ;

        internal static CantileverPlateDesign Copy(CantileverPlateDesign a)
            => a is null ? null : new CantileverPlateDesign
            {
                Thickness = a.Thickness,
            };

        internal static bool Same(CantileverBaseDesign a, CantileverBaseDesign b)
            => a is null ? b is null : b is not null
                && a.SectionId == b.SectionId
                && a.Length == b.Length
                && Same(a.FrontPlate, b.FrontPlate)
                && Same(a.RearPlate, b.RearPlate)
                && Same(a.Gusset, b.Gusset)
                ;

        internal static CantileverBaseDesign Copy(CantileverBaseDesign a)
            => a is null ? null : new CantileverBaseDesign
            {
                SectionId = a.SectionId,
                Length = a.Length,
                FrontPlate = Copy(a.FrontPlate),
                RearPlate = Copy(a.RearPlate),
                Gusset = Copy(a.Gusset),
            };

        internal static bool Same(CantileverGussetDesign a, CantileverGussetDesign b)
            => a is null ? b is null : b is not null
                && a.Thickness == b.Thickness
                ;

        internal static CantileverGussetDesign Copy(CantileverGussetDesign a)
            => a is null ? null : new CantileverGussetDesign
            {
                Thickness = a.Thickness,
            };

        internal static bool Same(CantileverColumnBaseConnectionDesign a, CantileverColumnBaseConnectionDesign b)
            => a is null ? b is null : b is not null
                && a.Variant == b.Variant
                && Same(a.Punches, b.Punches)
                ;

        internal static CantileverColumnBaseConnectionDesign Copy(CantileverColumnBaseConnectionDesign a)
            => a is null ? null : new CantileverColumnBaseConnectionDesign
            {
                Variant = a.Variant,
                Punches = Copy(a.Punches),
            };

        internal static bool Same(CantileverPunchParameters a, CantileverPunchParameters b)
            => a is null ? b is null : b is not null
                && a.Diameter == b.Diameter
                && a.HorizontalEndOffset == b.HorizontalEndOffset
                && a.ConnectionPitch == b.ConnectionPitch
                && a.RearPlateVerticalEndOffset == b.RearPlateVerticalEndOffset
                && a.RegularColumnPitch == b.RegularColumnPitch
                && a.ConnectionPunchesAboveBase == b.ConnectionPunchesAboveBase
                && a.ColumnBottomPlatePitch == b.ColumnBottomPlatePitch
                && a.ColumnBottomPlateEndOffset == b.ColumnBottomPlateEndOffset
                && a.ColumnTopPunchOffset == b.ColumnTopPunchOffset
                ;

        internal static CantileverPunchParameters Copy(CantileverPunchParameters a)
            => a is null ? null : new CantileverPunchParameters
            {
                Diameter = a.Diameter,
                HorizontalEndOffset = a.HorizontalEndOffset,
                ConnectionPitch = a.ConnectionPitch,
                RearPlateVerticalEndOffset = a.RearPlateVerticalEndOffset,
                RegularColumnPitch = a.RegularColumnPitch,
                ConnectionPunchesAboveBase = a.ConnectionPunchesAboveBase,
                ColumnBottomPlatePitch = a.ColumnBottomPlatePitch,
                ColumnBottomPlateEndOffset = a.ColumnBottomPlateEndOffset,
                ColumnTopPunchOffset = a.ColumnTopPunchOffset,
            };

        internal static bool Same(CantileverStationColumnHeightDesign a, CantileverStationColumnHeightDesign b)
            => a is null ? b is null : b is not null
                && a.Mode == b.Mode
                && a.ManualHeight == b.ManualHeight
                ;

        internal static CantileverStationColumnHeightDesign Copy(CantileverStationColumnHeightDesign a)
            => a is null ? null : new CantileverStationColumnHeightDesign
            {
                Mode = a.Mode,
                ManualHeight = a.ManualHeight,
            };

        internal static bool Same(CantileverArmTemplateDesign a, CantileverArmTemplateDesign b)
            => a is null ? b is null : b is not null
                && Same(a.Body, b.Body)
                && Same(a.MountingPlate, b.MountingPlate)
                && Same(a.EndPlate, b.EndPlate)
                ;

        internal static CantileverArmTemplateDesign Copy(CantileverArmTemplateDesign a)
            => a is null ? null : new CantileverArmTemplateDesign
            {
                Body = Copy(a.Body),
                MountingPlate = Copy(a.MountingPlate),
                EndPlate = Copy(a.EndPlate),
            };

        internal static bool Same(CantileverArmBodyDesign a, CantileverArmBodyDesign b)
            => a is null ? b is null : b is not null
                && a.Arrangement == b.Arrangement
                && a.SectionId == b.SectionId
                && a.CutLength == b.CutLength
                && a.SlopeRisePer12 == b.SlopeRisePer12
                ;

        internal static CantileverArmBodyDesign Copy(CantileverArmBodyDesign a)
            => a is null ? null : new CantileverArmBodyDesign
            {
                Arrangement = a.Arrangement,
                SectionId = a.SectionId,
                CutLength = a.CutLength,
                SlopeRisePer12 = a.SlopeRisePer12,
            };

        internal static bool Same(CantileverArmMountingPlateTemplateDesign a, CantileverArmMountingPlateTemplateDesign b)
            => a is null ? b is null : b is not null
                && a.Thickness == b.Thickness
                && a.VerticalPunchCount == b.VerticalPunchCount
                && a.VerticalEndOffset == b.VerticalEndOffset
                ;

        internal static CantileverArmMountingPlateTemplateDesign Copy(CantileverArmMountingPlateTemplateDesign a)
            => a is null ? null : new CantileverArmMountingPlateTemplateDesign
            {
                Thickness = a.Thickness,
                VerticalPunchCount = a.VerticalPunchCount,
                VerticalEndOffset = a.VerticalEndOffset,
            };

        internal static bool Same(CantileverArmEndPlateDesign a, CantileverArmEndPlateDesign b)
            => a is null ? b is null : b is not null
                && a.Mode == b.Mode
                && a.Thickness == b.Thickness
                && a.ExtraStopHeight == b.ExtraStopHeight
                ;

        internal static CantileverArmEndPlateDesign Copy(CantileverArmEndPlateDesign a)
            => a is null ? null : new CantileverArmEndPlateDesign
            {
                Mode = a.Mode,
                Thickness = a.Thickness,
                ExtraStopHeight = a.ExtraStopHeight,
            };

        internal static bool Same(CantileverArmCellOverride a, CantileverArmCellOverride b)
            => a is null ? b is null : b is not null
                && a.StationIndex == b.StationIndex
                && a.LevelIndex == b.LevelIndex
                && a.Side == b.Side
                && Same(a.Arm, b.Arm)
                ;

        internal static CantileverArmCellOverride Copy(CantileverArmCellOverride a)
            => a is null ? null : new CantileverArmCellOverride
            {
                StationIndex = a.StationIndex,
                LevelIndex = a.LevelIndex,
                Side = a.Side,
                Arm = Copy(a.Arm),
            };

        internal static bool Same(CantileverBracingDesign a, CantileverBracingDesign b)
            => a is null ? b is null : b is not null
                && a.SeparatorSectionId == b.SeparatorSectionId
                && a.PanelCountMode == b.PanelCountMode
                && a.ManualPanelCount == b.ManualPanelCount
                && a.BracedPanelHeight == b.BracedPanelHeight
                && a.CentralEmptySpaceHeight == b.CentralEmptySpaceHeight
                && a.BraceKind == b.BraceKind
                && a.BraceSectionId == b.BraceSectionId
                && Same(a.ColdRolled, b.ColdRolled)
                && a.PanelLayoutMode == b.PanelLayoutMode
                && Sequence(a.AdvancedPanelSegments, b.AdvancedPanelSegments, Same)
                ;

        internal static CantileverBracingDesign Copy(CantileverBracingDesign a)
            => a is null ? null : new CantileverBracingDesign
            {
                SeparatorSectionId = a.SeparatorSectionId,
                PanelCountMode = a.PanelCountMode,
                ManualPanelCount = a.ManualPanelCount,
                BracedPanelHeight = a.BracedPanelHeight,
                CentralEmptySpaceHeight = a.CentralEmptySpaceHeight,
                BraceKind = a.BraceKind,
                BraceSectionId = a.BraceSectionId,
                ColdRolled = Copy(a.ColdRolled),
                PanelLayoutMode = a.PanelLayoutMode,
                AdvancedPanelSegments = a.AdvancedPanelSegments?.Select(Copy).ToList(),
            };

        internal static bool Same(CantileverColdRolledBraceDesign a, CantileverColdRolledBraceDesign b)
            => a is null ? b is null : b is not null
                && a.Diameter == b.Diameter
                ;

        internal static CantileverColdRolledBraceDesign Copy(CantileverColdRolledBraceDesign a)
            => a is null ? null : new CantileverColdRolledBraceDesign
            {
                Diameter = a.Diameter,
            };

        internal static bool Same(CantileverPanelSegmentDesign a, CantileverPanelSegmentDesign b)
            => a is null ? b is null : b is not null
                && a.StartElevation == b.StartElevation
                && a.EndElevation == b.EndElevation
                && a.BracingMode == b.BracingMode
                ;

        internal static CantileverPanelSegmentDesign Copy(CantileverPanelSegmentDesign a)
            => a is null ? null : new CantileverPanelSegmentDesign
            {
                StartElevation = a.StartElevation,
                EndElevation = a.EndElevation,
                BracingMode = a.BracingMode,
            };

        internal static bool Same(CantileverPlantaVisibilityDesign a, CantileverPlantaVisibilityDesign b)
            => a is null ? b is null : b is not null
                && a.ShowArms == b.ShowArms
                && a.ShowBraces == b.ShowBraces
                ;

        internal static CantileverPlantaVisibilityDesign Copy(CantileverPlantaVisibilityDesign a)
            => a is null ? null : new CantileverPlantaVisibilityDesign
            {
                ShowArms = a.ShowArms,
                ShowBraces = a.ShowBraces,
            };

        private static bool Sequence<T>(IReadOnlyList<T> a, IReadOnlyList<T> b, Func<T, T, bool> same)
        {
            if (a is null || b is null) return (a?.Count ?? 0) == (b?.Count ?? 0);
            if (a.Count != b.Count) return false;
            for (int i = 0; i < a.Count; i++) if (!same(a[i], b[i])) return false;
            return true;
        }
    }
}
