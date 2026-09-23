using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Domain.RackFrames;
using RackCad.Domain.Systems.Dynamic;
using RackCad.Domain.Systems.PushBack;
using RackCad.Domain.Systems.Cantilever;
using RackCad.Domain.Systems.Selective;
using RackCad.Domain.Systems.Shared;
using Xunit;
namespace RackCad.Tests;
// Explicit domain assertions, separate from authored equality. No reflection or serialization oracle.
internal static class I58F1DomainOracle
{
    public static void Values(DynamicRackDesign e, DynamicRackDesign a)
    {
        if(e is null){Assert.Null(a);return;}
        Assert.NotNull(a);
        Values(e.Pallet, a.Pallet);
        Assert.Equal(e.PalletsDeep, a.PalletsDeep);
        Assert.Equal(e.LoadLevels, a.LoadLevels);
        Assert.Equal(e.FirstLevelDatum, a.FirstLevelDatum);
        Assert.Equal(e.FirstLevelHeight, a.FirstLevelHeight);
        Assert.Equal(e.BeamDepth, a.BeamDepth);
        Items(e.IntermediateBeamDepths, a.IntermediateBeamDepths, (x,y)=>Assert.Equal(x,y));
        Assert.Equal(e.PalletTolerance, a.PalletTolerance);
        Assert.Equal(e.InOutBeamCatalogId, a.InOutBeamCatalogId);
        Assert.Equal(e.HeaderPostCatalogId, a.HeaderPostCatalogId);
        Assert.Equal(e.PostPeralte, a.PostPeralte);
        Assert.Equal(e.SeparatorCountOverride, a.SeparatorCountOverride);
        Assert.Equal(e.SeparatorSpacingOverride, a.SeparatorSpacingOverride);
        Assert.Equal(e.DerivedPostReinforced, a.DerivedPostReinforced);
        Assert.Equal(e.DerivedPostReinforcementHeight, a.DerivedPostReinforcementHeight);
        Assert.Equal(e.DerivedPostHeight, a.DerivedPostHeight);
        Items(e.HeaderLineOverrides, a.HeaderLineOverrides, Values);
        Items(e.DerivedPostLineOverrides, a.DerivedPostLineOverrides, Values);
        Assert.Equal(e.ManualHeaderHeightOverride, a.ManualHeaderHeightOverride);
        Assert.Equal(e.NumberFronts, a.NumberFronts);
        Assert.Equal(e.NumberLevels, a.NumberLevels);
        Assert.Equal(e.DrawRackName, a.DrawRackName);
        Assert.Equal(e.AnnotationScale, a.AnnotationScale);
        Assert.Equal(e.Dimensions, a.Dimensions);
        Assert.Equal(e.DimensionViews, a.DimensionViews);
        Assert.Equal(e.DimensionStyle, a.DimensionStyle);
        Items(e.SafetySelections, a.SafetySelections, Values);
        Items(e.Fronts, a.Fronts, Values);
        Items(e.Modules, a.Modules, Values);
    }
    public static void Separate(DynamicRackDesign e, DynamicRackDesign a)
    {
        if(e is null){Assert.Null(a);return;}
        Assert.NotNull(a);
        Assert.NotSame(e,a);
        Separate(e.Pallet,a.Pallet);
        Assert.NotSame(e.IntermediateBeamDepths,a.IntermediateBeamDepths);
        Assert.NotSame(e.HeaderLineOverrides,a.HeaderLineOverrides);
        Items(e.HeaderLineOverrides,a.HeaderLineOverrides,Separate);
        Assert.NotSame(e.DerivedPostLineOverrides,a.DerivedPostLineOverrides);
        Items(e.DerivedPostLineOverrides,a.DerivedPostLineOverrides,Separate);
        Assert.NotSame(e.SafetySelections,a.SafetySelections);
        Items(e.SafetySelections,a.SafetySelections,Separate);
        Assert.NotSame(e.Fronts,a.Fronts);
        Items(e.Fronts,a.Fronts,Separate);
        Assert.NotSame(e.Modules,a.Modules);
        Items(e.Modules,a.Modules,Separate);
    }
    public static void Mutate(DynamicRackDesign a)
    {
        if(a is null)return;
        Mutate(a.Pallet);
        a.PalletsDeep=a.PalletsDeep + 123;
        a.LoadLevels=a.LoadLevels + 123;
        a.FirstLevelDatum=(a.FirstLevelDatum ?? 0) + 123;
        a.FirstLevelHeight=a.FirstLevelHeight + 123;
        a.BeamDepth=a.BeamDepth + 123;
        a.IntermediateBeamDepths.Clear();
        a.PalletTolerance=a.PalletTolerance + 123;
        a.InOutBeamCatalogId="mutated by isolation oracle";
        a.HeaderPostCatalogId="mutated by isolation oracle";
        a.PostPeralte=a.PostPeralte + 123;
        a.SeparatorCountOverride=(a.SeparatorCountOverride ?? 0) + 123;
        a.SeparatorSpacingOverride=(a.SeparatorSpacingOverride ?? 0) + 123;
        a.DerivedPostReinforced=!a.DerivedPostReinforced;
        a.DerivedPostReinforcementHeight=(a.DerivedPostReinforcementHeight ?? 0) + 123;
        a.DerivedPostHeight=(a.DerivedPostHeight ?? 0) + 123;
        foreach(var item in a.HeaderLineOverrides)Mutate(item);
        a.HeaderLineOverrides.Clear();
        foreach(var item in a.DerivedPostLineOverrides)Mutate(item);
        a.DerivedPostLineOverrides.Clear();
        a.ManualHeaderHeightOverride=(a.ManualHeaderHeightOverride ?? 0) + 123;
        a.NumberFronts=!a.NumberFronts;
        a.NumberLevels=!a.NumberLevels;
        a.DrawRackName=!a.DrawRackName;
        a.AnnotationScale=a.AnnotationScale + 123;
        a.Dimensions=(DimensionDetail)99;
        a.DimensionViews=(DimensionViewVisibility)99;
        a.DimensionStyle="mutated by isolation oracle";
        foreach(var item in a.SafetySelections)Mutate(item);
        a.SafetySelections.Clear();
        foreach(var item in a.Fronts)Mutate(item);
        a.Fronts.Clear();
        foreach(var item in a.Modules)Mutate(item);
        a.Modules.Clear();
    }
    public static void Values(PalletSpecification e, PalletSpecification a)
    {
        if(e is null){Assert.Null(a);return;}
        Assert.NotNull(a);
        Assert.Equal(e.Front, a.Front);
        Assert.Equal(e.Depth, a.Depth);
        Assert.Equal(e.Height, a.Height);
        Assert.Equal(e.Weight, a.Weight);
        Assert.Equal(e.WeightUnit, a.WeightUnit);
    }
    public static void Separate(PalletSpecification e, PalletSpecification a)
    {
        if(e is null){Assert.Null(a);return;}
        Assert.NotNull(a);
        Assert.NotSame(e,a);
    }
    public static void Mutate(PalletSpecification a)
    {
        if(a is null)return;
        a.Front=a.Front + 123;
        a.Depth=a.Depth + 123;
        a.Height=a.Height + 123;
        a.Weight=a.Weight + 123;
        a.WeightUnit="mutated by isolation oracle";
    }
    public static void Values(DynamicHeaderLineOverride e, DynamicHeaderLineOverride a)
    {
        if(e is null){Assert.Null(a);return;}
        Assert.NotNull(a);
        Assert.Equal(e.PostIndex, a.PostIndex);
        Assert.Equal(e.ModuleId, a.ModuleId);
        Values(e.Header, a.Header);
    }
    public static void Separate(DynamicHeaderLineOverride e, DynamicHeaderLineOverride a)
    {
        if(e is null){Assert.Null(a);return;}
        Assert.NotNull(a);
        Assert.NotSame(e,a);
        Separate(e.Header,a.Header);
    }
    public static void Mutate(DynamicHeaderLineOverride a)
    {
        if(a is null)return;
        a.PostIndex=a.PostIndex + 123;
        a.ModuleId="mutated by isolation oracle";
        Mutate(a.Header);
    }
    public static void Values(RackFrameConfiguration e, RackFrameConfiguration a)
    {
        if(e is null){Assert.Null(a);return;}
        Assert.NotNull(a);
        Assert.Equal(e.Name, a.Name);
        Assert.Equal(e.Units, a.Units);
        Assert.Equal(e.Height, a.Height);
        Assert.Equal(e.Depth, a.Depth);
        Assert.Equal(e.PostPeralte, a.PostPeralte);
        Assert.Equal(e.CelosiaStartTroquel, a.CelosiaStartTroquel);
        Assert.Equal(e.DiagonalStartOffsetTroqueles, a.DiagonalStartOffsetTroqueles);
        Assert.Equal(e.DiagonalEndOffsetTroqueles, a.DiagonalEndOffsetTroqueles);
        Assert.Equal(e.DiagonalDoubleSpacingTroqueles, a.DiagonalDoubleSpacingTroqueles);
        Assert.Equal(e.HorizontalDoubleOffsetTroqueles, a.HorizontalDoubleOffsetTroqueles);
        Assert.Equal(e.PasoTroquel, a.PasoTroquel);
        Assert.Equal(e.PanelClear, a.PanelClear);
        Assert.Equal(e.StandardBaselineId, a.StandardBaselineId);
        Assert.Equal(e.StandardBaselineVersion, a.StandardBaselineVersion);
        Values(e.LeftPost, a.LeftPost);
        Values(e.RightPost, a.RightPost);
        Values(e.LeftBasePlate, a.LeftBasePlate);
        Values(e.RightBasePlate, a.RightBasePlate);
        Items(e.Horizontals, a.Horizontals, Values);
        Items(e.BracingPanels, a.BracingPanels, Values);
    }
    public static void Separate(RackFrameConfiguration e, RackFrameConfiguration a)
    {
        if(e is null){Assert.Null(a);return;}
        Assert.NotNull(a);
        Assert.NotSame(e,a);
        Separate(e.LeftPost,a.LeftPost);
        Separate(e.RightPost,a.RightPost);
        Separate(e.LeftBasePlate,a.LeftBasePlate);
        Separate(e.RightBasePlate,a.RightBasePlate);
        Assert.NotSame(e.Horizontals,a.Horizontals);
        Items(e.Horizontals,a.Horizontals,Separate);
        Assert.NotSame(e.BracingPanels,a.BracingPanels);
        Items(e.BracingPanels,a.BracingPanels,Separate);
    }
    public static void Mutate(RackFrameConfiguration a)
    {
        if(a is null)return;
        a.Name="mutated by isolation oracle";
        a.Units="mutated by isolation oracle";
        a.Height=a.Height + 123;
        a.Depth=a.Depth + 123;
        a.PostPeralte=a.PostPeralte + 123;
        a.CelosiaStartTroquel=a.CelosiaStartTroquel + 123;
        a.DiagonalStartOffsetTroqueles=a.DiagonalStartOffsetTroqueles + 123;
        a.DiagonalEndOffsetTroqueles=a.DiagonalEndOffsetTroqueles + 123;
        a.DiagonalDoubleSpacingTroqueles=a.DiagonalDoubleSpacingTroqueles + 123;
        a.HorizontalDoubleOffsetTroqueles=a.HorizontalDoubleOffsetTroqueles + 123;
        a.PasoTroquel=a.PasoTroquel + 123;
        a.PanelClear=a.PanelClear + 123;
        a.StandardBaselineId="mutated by isolation oracle";
        a.StandardBaselineVersion="mutated by isolation oracle";
        Mutate(a.LeftPost);
        Mutate(a.RightPost);
        Mutate(a.LeftBasePlate);
        Mutate(a.RightBasePlate);
        foreach(var item in a.Horizontals)Mutate(item);
        a.Horizontals.Clear();
        foreach(var item in a.BracingPanels)Mutate(item);
        a.BracingPanels.Clear();
    }
    public static void Values(PostAssembly e, PostAssembly a)
    {
        if(e is null){Assert.Null(a);return;}
        Assert.NotNull(a);
        Assert.Equal(e.Side, a.Side);
        Assert.Equal(e.PostCatalogId, a.PostCatalogId);
        Assert.Equal(e.Description, a.Description);
        Assert.Equal(e.HasReinforcement, a.HasReinforcement);
        Assert.Equal(e.ReinforcementCatalogId, a.ReinforcementCatalogId);
        Assert.Equal(e.ReinforcementHeight, a.ReinforcementHeight);
    }
    public static void Separate(PostAssembly e, PostAssembly a)
    {
        if(e is null){Assert.Null(a);return;}
        Assert.NotNull(a);
        Assert.NotSame(e,a);
    }
    public static void Mutate(PostAssembly a)
    {
        if(a is null)return;
        a.Side=(PostSide)99;
        a.PostCatalogId="mutated by isolation oracle";
        a.Description="mutated by isolation oracle";
        a.HasReinforcement=!a.HasReinforcement;
        a.ReinforcementCatalogId="mutated by isolation oracle";
        a.ReinforcementHeight=a.ReinforcementHeight + 123;
    }
    public static void Values(BasePlatePlacement e, BasePlatePlacement a)
    {
        if(e is null){Assert.Null(a);return;}
        Assert.NotNull(a);
        Assert.Equal(e.PostSide, a.PostSide);
        Assert.Equal(e.PlateCatalogId, a.PlateCatalogId);
        Assert.Equal(e.Description, a.Description);
        Assert.Equal(e.ConnectionPointId, a.ConnectionPointId);
        Assert.Equal(e.PeralteOverride, a.PeralteOverride);
    }
    public static void Separate(BasePlatePlacement e, BasePlatePlacement a)
    {
        if(e is null){Assert.Null(a);return;}
        Assert.NotNull(a);
        Assert.NotSame(e,a);
    }
    public static void Mutate(BasePlatePlacement a)
    {
        if(a is null)return;
        a.PostSide=(PostSide)99;
        a.PlateCatalogId="mutated by isolation oracle";
        a.Description="mutated by isolation oracle";
        a.ConnectionPointId="mutated by isolation oracle";
        a.PeralteOverride=(a.PeralteOverride ?? 0) + 123;
    }
    public static void Values(FrameHorizontal e, FrameHorizontal a)
    {
        if(e is null){Assert.Null(a);return;}
        Assert.NotNull(a);
        Assert.Equal(e.Id, a.Id);
        Assert.Equal(e.Number, a.Number);
        Assert.Equal(e.Elevation, a.Elevation);
        Assert.Equal(e.ProfileId, a.ProfileId);
        Assert.Equal(e.Quantity, a.Quantity);
        Assert.Equal(e.MountingFace, a.MountingFace);
        Assert.Equal(e.State, a.State);
        Assert.Equal(e.Notes, a.Notes);
        Assert.Equal(e.IsStandard, a.IsStandard);
    }
    public static void Separate(FrameHorizontal e, FrameHorizontal a)
    {
        if(e is null){Assert.Null(a);return;}
        Assert.NotNull(a);
        Assert.NotSame(e,a);
    }
    public static void Mutate(FrameHorizontal a)
    {
        if(a is null)return;
        a.Id="mutated by isolation oracle";
        a.Number=a.Number + 123;
        a.Elevation=a.Elevation + 123;
        a.ProfileId="mutated by isolation oracle";
        a.Quantity=a.Quantity + 123;
        a.MountingFace=(FrameSide)99;
        a.State=(FrameComponentState)99;
        a.Notes="mutated by isolation oracle";
        a.IsStandard=!a.IsStandard;
    }
    public static void Values(BracingPanel e, BracingPanel a)
    {
        if(e is null){Assert.Null(a);return;}
        Assert.NotNull(a);
        Assert.Equal(e.PanelId, a.PanelId);
        Assert.Equal(e.Number, a.Number);
        Assert.Equal(e.Index, a.Index);
        Assert.Equal(e.LowerHorizontalId, a.LowerHorizontalId);
        Assert.Equal(e.UpperHorizontalId, a.UpperHorizontalId);
        Assert.Equal(e.Arrangement, a.Arrangement);
        Assert.Equal(e.MountingFace, a.MountingFace);
        Assert.Equal(e.DiagonalProfileId, a.DiagonalProfileId);
        Assert.Equal(e.DiagonalDirection, a.DiagonalDirection);
        Assert.Equal(e.StartConnectionPointId, a.StartConnectionPointId);
        Assert.Equal(e.EndConnectionPointId, a.EndConnectionPointId);
        Assert.Equal(e.IsStandard, a.IsStandard);
        Assert.Equal(e.IsException, a.IsException);
    }
    public static void Separate(BracingPanel e, BracingPanel a)
    {
        if(e is null){Assert.Null(a);return;}
        Assert.NotNull(a);
        Assert.NotSame(e,a);
    }
    public static void Mutate(BracingPanel a)
    {
        if(a is null)return;
        a.PanelId="mutated by isolation oracle";
        a.Number=a.Number + 123;
        a.Index=a.Index + 123;
        a.LowerHorizontalId="mutated by isolation oracle";
        a.UpperHorizontalId="mutated by isolation oracle";
        a.Arrangement=(BracingPattern)99;
        a.MountingFace=(FrameSide)99;
        a.DiagonalProfileId="mutated by isolation oracle";
        a.DiagonalDirection=(DiagonalDirection)99;
        a.StartConnectionPointId="mutated by isolation oracle";
        a.EndConnectionPointId="mutated by isolation oracle";
        a.IsStandard=!a.IsStandard;
        a.IsException=!a.IsException;
    }
    public static void Values(DynamicDerivedPostLineOverride e, DynamicDerivedPostLineOverride a)
    {
        if(e is null){Assert.Null(a);return;}
        Assert.NotNull(a);
        Assert.Equal(e.PostIndex, a.PostIndex);
        Assert.Equal(e.Height, a.Height);
    }
    public static void Separate(DynamicDerivedPostLineOverride e, DynamicDerivedPostLineOverride a)
    {
        if(e is null){Assert.Null(a);return;}
        Assert.NotNull(a);
        Assert.NotSame(e,a);
    }
    public static void Mutate(DynamicDerivedPostLineOverride a)
    {
        if(a is null)return;
        a.PostIndex=a.PostIndex + 123;
        a.Height=a.Height + 123;
    }
    public static void Values(SelectiveSafetySelection e, SelectiveSafetySelection a)
    {
        if(e is null){Assert.Null(a);return;}
        Assert.NotNull(a);
        Assert.Equal(e.ElementId, a.ElementId);
        Assert.Equal(e.Quantity, a.Quantity);
        Values(e.BotaB, a.BotaB);
        Assert.Equal(e.BootSidesDeclared, a.BootSidesDeclared);
        Items(e.PostSides, a.PostSides, Values);
        Values(e.Tope, a.Tope);
        Values(e.Desviador, a.Desviador);
        Values(e.Defensa, a.Defensa);
        Values(e.Guia, a.Guia);
        Values(e.Parrilla, a.Parrilla);
        Values(e.Bota, a.Bota);
        Assert.Equal(e.AuthoredSide ?? e.Side, a.AuthoredSide ?? a.Side);
        Assert.Equal(a.AuthoredSide ?? a.Side, a.Side);
        Assert.Empty(a.DerivedAisles);
    }
    public static void Separate(SelectiveSafetySelection e, SelectiveSafetySelection a)
    {
        if(e is null){Assert.Null(a);return;}
        Assert.NotNull(a);
        Assert.NotSame(e,a);
        Separate(e.BotaB,a.BotaB);
        Assert.NotSame(e.PostSides,a.PostSides);
        Items(e.PostSides,a.PostSides,Separate);
        Separate(e.Tope,a.Tope);
        Separate(e.Desviador,a.Desviador);
        Separate(e.Defensa,a.Defensa);
        Separate(e.Guia,a.Guia);
        Separate(e.Parrilla,a.Parrilla);
        Separate(e.Bota,a.Bota);
    }
    public static void Mutate(SelectiveSafetySelection a)
    {
        if(a is null)return;
        a.ElementId="mutated by isolation oracle";
        a.Quantity=a.Quantity + 123;
        Mutate(a.BotaB);
        a.BootSidesDeclared=!a.BootSidesDeclared;
        foreach(var item in a.PostSides)Mutate(item);
        a.PostSides.Clear();
        Mutate(a.Tope);
        Mutate(a.Desviador);
        Mutate(a.Defensa);
        Mutate(a.Guia);
        Mutate(a.Parrilla);
        Mutate(a.Bota);
    }
    public static void Values(SelectiveBotaConfig e, SelectiveBotaConfig a)
    {
        if(e is null){Assert.Null(a);return;}
        Assert.NotNull(a);
        Assert.Equal(e.PieceId, a.PieceId);
        Assert.Equal(e.Placement, a.Placement);
        Items(e.Posts, a.Posts, Values);
    }
    public static void Separate(SelectiveBotaConfig e, SelectiveBotaConfig a)
    {
        if(e is null){Assert.Null(a);return;}
        Assert.NotNull(a);
        Assert.NotSame(e,a);
        Assert.NotSame(e.Posts,a.Posts);
        Items(e.Posts,a.Posts,Separate);
    }
    public static void Mutate(SelectiveBotaConfig a)
    {
        if(a is null)return;
        a.PieceId="mutated by isolation oracle";
        a.Placement=(BootPlacement)99;
        foreach(var item in a.Posts)Mutate(item);
        a.Posts.Clear();
    }
    public static void Values(BootPostPlacement e, BootPostPlacement a)
    {
        if(e is null){Assert.Null(a);return;}
        Assert.NotNull(a);
        Assert.Equal(e.PostIndex, a.PostIndex);
        Assert.Equal(e.Placement, a.Placement);
    }
    public static void Separate(BootPostPlacement e, BootPostPlacement a)
    {
        if(e is null){Assert.Null(a);return;}
        Assert.NotNull(a);
        Assert.NotSame(e,a);
    }
    public static void Mutate(BootPostPlacement a)
    {
        if(a is null)return;
        a.PostIndex=a.PostIndex + 123;
        a.Placement=(BootPlacement)99;
    }
    public static void Values(SafetyPostSide e, SafetyPostSide a)
    {
        if(e is null){Assert.Null(a);return;}
        Assert.NotNull(a);
        Assert.Equal(e.PostIndex, a.PostIndex);
        Assert.Equal(e.Side, a.Side);
    }
    public static void Separate(SafetyPostSide e, SafetyPostSide a)
    {
        if(e is null){Assert.Null(a);return;}
        Assert.NotNull(a);
        Assert.NotSame(e,a);
    }
    public static void Mutate(SafetyPostSide a)
    {
        if(a is null)return;
        a.PostIndex=a.PostIndex + 123;
        a.Side=(SafetySide)99;
    }
    public static void Values(SelectiveTopeConfig e, SelectiveTopeConfig a)
    {
        if(e is null){Assert.Null(a);return;}
        Assert.NotNull(a);
        Assert.Equal(e.Shared, a.Shared);
        Assert.Equal(e.Fondo, a.Fondo);
        Assert.Equal(e.Saque, a.Saque);
        Assert.Equal(e.Frontal, a.Frontal);
        Items(e.OffCells, a.OffCells, Values);
    }
    public static void Separate(SelectiveTopeConfig e, SelectiveTopeConfig a)
    {
        if(e is null){Assert.Null(a);return;}
        Assert.NotNull(a);
        Assert.NotSame(e,a);
        Assert.NotSame(e.OffCells,a.OffCells);
        Items(e.OffCells,a.OffCells,Separate);
    }
    public static void Mutate(SelectiveTopeConfig a)
    {
        if(a is null)return;
        a.Shared=!a.Shared;
        a.Fondo=a.Fondo + 123;
        a.Saque=a.Saque + 123;
        a.Frontal=!a.Frontal;
        foreach(var item in a.OffCells)Mutate(item);
        a.OffCells.Clear();
    }
    public static void Values(SelectiveGridCell e, SelectiveGridCell a)
    {
        if(e is null){Assert.Null(a);return;}
        Assert.NotNull(a);
        Assert.Equal(e.Frente, a.Frente);
        Assert.Equal(e.Level, a.Level);
    }
    public static void Separate(SelectiveGridCell e, SelectiveGridCell a)
    {
        if(e is null){Assert.Null(a);return;}
        Assert.NotNull(a);
        Assert.NotSame(e,a);
    }
    public static void Mutate(SelectiveGridCell a)
    {
        if(a is null)return;
        a.Frente=a.Frente + 123;
        a.Level=a.Level + 123;
    }
    public static void Values(SelectiveDesviadorConfig e, SelectiveDesviadorConfig a)
    {
        if(e is null){Assert.Null(a);return;}
        Assert.NotNull(a);
        Assert.Equal(e.Longitud, a.Longitud);
        Assert.Equal(e.PrimerNivelAltura, a.PrimerNivelAltura);
        Items(e.OffCells, a.OffCells, Values);
    }
    public static void Separate(SelectiveDesviadorConfig e, SelectiveDesviadorConfig a)
    {
        if(e is null){Assert.Null(a);return;}
        Assert.NotNull(a);
        Assert.NotSame(e,a);
        Assert.NotSame(e.OffCells,a.OffCells);
        Items(e.OffCells,a.OffCells,Separate);
    }
    public static void Mutate(SelectiveDesviadorConfig a)
    {
        if(a is null)return;
        a.Longitud=a.Longitud + 123;
        a.PrimerNivelAltura=a.PrimerNivelAltura + 123;
        foreach(var item in a.OffCells)Mutate(item);
        a.OffCells.Clear();
    }
    public static void Values(SelectiveDefensaConfig e, SelectiveDefensaConfig a)
    {
        if(e is null){Assert.Null(a);return;}
        Assert.NotNull(a);
        Items(e.Posts, a.Posts, Values);
    }
    public static void Separate(SelectiveDefensaConfig e, SelectiveDefensaConfig a)
    {
        if(e is null){Assert.Null(a);return;}
        Assert.NotNull(a);
        Assert.NotSame(e,a);
        Assert.NotSame(e.Posts,a.Posts);
        Items(e.Posts,a.Posts,Separate);
    }
    public static void Mutate(SelectiveDefensaConfig a)
    {
        if(a is null)return;
        foreach(var item in a.Posts)Mutate(item);
        a.Posts.Clear();
    }
    public static void Values(SafetyPostDefense e, SafetyPostDefense a)
    {
        if(e is null){Assert.Null(a);return;}
        Assert.NotNull(a);
        Assert.Equal(e.PostIndex, a.PostIndex);
        Assert.Equal(e.ExitLength, a.ExitLength);
        Assert.Equal(e.EntranceLength, a.EntranceLength);
        Assert.Equal(e.ExitAuto, a.ExitAuto);
        Assert.Equal(e.EntranceAuto, a.EntranceAuto);
    }
    public static void Separate(SafetyPostDefense e, SafetyPostDefense a)
    {
        if(e is null){Assert.Null(a);return;}
        Assert.NotNull(a);
        Assert.NotSame(e,a);
    }
    public static void Mutate(SafetyPostDefense a)
    {
        if(a is null)return;
        a.PostIndex=a.PostIndex + 123;
        a.ExitLength=a.ExitLength + 123;
        a.EntranceLength=a.EntranceLength + 123;
        a.ExitAuto=!a.ExitAuto;
        a.EntranceAuto=!a.EntranceAuto;
    }
    public static void Values(SelectiveGuiaConfig e, SelectiveGuiaConfig a)
    {
        if(e is null){Assert.Null(a);return;}
        Assert.NotNull(a);
        Items(e.OffCells, a.OffCells, Values);
    }
    public static void Separate(SelectiveGuiaConfig e, SelectiveGuiaConfig a)
    {
        if(e is null){Assert.Null(a);return;}
        Assert.NotNull(a);
        Assert.NotSame(e,a);
        Assert.NotSame(e.OffCells,a.OffCells);
        Items(e.OffCells,a.OffCells,Separate);
    }
    public static void Mutate(SelectiveGuiaConfig a)
    {
        if(a is null)return;
        foreach(var item in a.OffCells)Mutate(item);
        a.OffCells.Clear();
    }
    public static void Values(SelectiveParrillaConfig e, SelectiveParrillaConfig a)
    {
        if(e is null){Assert.Null(a);return;}
        Assert.NotNull(a);
        Assert.Equal(e.Frontal, a.Frontal);
        Assert.Equal(e.Lateral, a.Lateral);
        Assert.Equal(e.Frente, a.Frente);
        Assert.Equal(e.Cantidad, a.Cantidad);
        Items(e.OffCells, a.OffCells, Values);
    }
    public static void Separate(SelectiveParrillaConfig e, SelectiveParrillaConfig a)
    {
        if(e is null){Assert.Null(a);return;}
        Assert.NotNull(a);
        Assert.NotSame(e,a);
        Assert.NotSame(e.OffCells,a.OffCells);
        Items(e.OffCells,a.OffCells,Separate);
    }
    public static void Mutate(SelectiveParrillaConfig a)
    {
        if(a is null)return;
        a.Frontal=!a.Frontal;
        a.Lateral=!a.Lateral;
        a.Frente=a.Frente + 123;
        a.Cantidad=a.Cantidad + 123;
        foreach(var item in a.OffCells)Mutate(item);
        a.OffCells.Clear();
    }
    public static void Values(DynamicRackFrontDesign e, DynamicRackFrontDesign a)
    {
        if(e is null){Assert.Null(a);return;}
        Assert.NotNull(a);
        Assert.Equal(e.IsActive, a.IsActive);
        Assert.Equal(e.PalletCount, a.PalletCount);
        Assert.Equal(e.LoadLevels, a.LoadLevels);
        Assert.Equal(e.PalletsDeep, a.PalletsDeep);
        Assert.Equal(e.DepthStartPosition, a.DepthStartPosition);
        Assert.Equal(e.BeamLengthOverride, a.BeamLengthOverride);
        Assert.Equal(e.FirstLevelHeight, a.FirstLevelHeight);
        Items(e.IntermediateBeamDepths, a.IntermediateBeamDepths, (x,y)=>Assert.Equal(x,y));
        Items(e.Levels, a.Levels, Values);
    }
    public static void Separate(DynamicRackFrontDesign e, DynamicRackFrontDesign a)
    {
        if(e is null){Assert.Null(a);return;}
        Assert.NotNull(a);
        Assert.NotSame(e,a);
        Assert.NotSame(e.IntermediateBeamDepths,a.IntermediateBeamDepths);
        Assert.NotSame(e.Levels,a.Levels);
        Items(e.Levels,a.Levels,Separate);
    }
    public static void Mutate(DynamicRackFrontDesign a)
    {
        if(a is null)return;
        a.IsActive=!a.IsActive;
        a.PalletCount=a.PalletCount + 123;
        a.LoadLevels=(a.LoadLevels ?? 0) + 123;
        a.PalletsDeep=(a.PalletsDeep ?? 0) + 123;
        a.DepthStartPosition=(a.DepthStartPosition ?? 0) + 123;
        a.BeamLengthOverride=(a.BeamLengthOverride ?? 0) + 123;
        a.FirstLevelHeight=(a.FirstLevelHeight ?? 0) + 123;
        a.IntermediateBeamDepths.Clear();
        foreach(var item in a.Levels)Mutate(item);
        a.Levels.Clear();
    }
    public static void Values(DynamicRackLevelDesign e, DynamicRackLevelDesign a)
    {
        if(e is null){Assert.Null(a);return;}
        Assert.NotNull(a);
        Assert.Equal(e.PalletFront, a.PalletFront);
        Assert.Equal(e.PalletHeight, a.PalletHeight);
        Assert.Equal(e.PalletWeight, a.PalletWeight);
        Assert.Equal(e.ClearHeight, a.ClearHeight);
        Assert.Equal(e.InOutBeamCatalogId, a.InOutBeamCatalogId);
        Assert.Equal(e.InOutBeamDepth, a.InOutBeamDepth);
        Assert.Equal(e.BeamLengthOverride, a.BeamLengthOverride);
        Assert.Equal(e.IntermediateBeamCatalogId, a.IntermediateBeamCatalogId);
        Assert.Equal(e.IntermediateBeamDepth, a.IntermediateBeamDepth);
    }
    public static void Separate(DynamicRackLevelDesign e, DynamicRackLevelDesign a)
    {
        if(e is null){Assert.Null(a);return;}
        Assert.NotNull(a);
        Assert.NotSame(e,a);
    }
    public static void Mutate(DynamicRackLevelDesign a)
    {
        if(a is null)return;
        a.PalletFront=(a.PalletFront ?? 0) + 123;
        a.PalletHeight=(a.PalletHeight ?? 0) + 123;
        a.PalletWeight=(a.PalletWeight ?? 0) + 123;
        a.ClearHeight=(a.ClearHeight ?? 0) + 123;
        a.InOutBeamCatalogId="mutated by isolation oracle";
        a.InOutBeamDepth=(a.InOutBeamDepth ?? 0) + 123;
        a.BeamLengthOverride=(a.BeamLengthOverride ?? 0) + 123;
        a.IntermediateBeamCatalogId="mutated by isolation oracle";
        a.IntermediateBeamDepth=(a.IntermediateBeamDepth ?? 0) + 123;
    }
    public static void Values(DynamicRackModuleDesign e, DynamicRackModuleDesign a)
    {
        if(e is null){Assert.Null(a);return;}
        Assert.NotNull(a);
        Assert.Equal(e.ModuleId, a.ModuleId);
        Assert.Equal(e.Kind, a.Kind);
        Assert.Equal(e.Length, a.Length);
        Assert.Equal(e.IsCalculated, a.IsCalculated);
        Assert.Equal(e.IsManualOverride, a.IsManualOverride);
        Assert.Equal(e.UseCalculatedHeaderConfiguration, a.UseCalculatedHeaderConfiguration);
        Values(e.HeaderConfiguration, a.HeaderConfiguration);
        Assert.Equal(e.Notes, a.Notes);
    }
    public static void Separate(DynamicRackModuleDesign e, DynamicRackModuleDesign a)
    {
        if(e is null){Assert.Null(a);return;}
        Assert.NotNull(a);
        Assert.NotSame(e,a);
        Separate(e.HeaderConfiguration,a.HeaderConfiguration);
    }
    public static void Mutate(DynamicRackModuleDesign a)
    {
        if(a is null)return;
        a.ModuleId="mutated by isolation oracle";
        a.Kind=(DynamicRackModuleKind)99;
        a.Length=a.Length + 123;
        a.IsCalculated=!a.IsCalculated;
        a.IsManualOverride=!a.IsManualOverride;
        a.UseCalculatedHeaderConfiguration=!a.UseCalculatedHeaderConfiguration;
        Mutate(a.HeaderConfiguration);
        a.Notes="mutated by isolation oracle";
    }
    public static void Values(PushBackDesign e, PushBackDesign a)
    {
        if(e is null){Assert.Null(a);return;}
        Assert.NotNull(a);
        Values(e.Structure, a.Structure);
        Items(e.Fronts, a.Fronts, Values);
        Assert.Equal(e.LegacyHighEndBeamPeralte, a.LegacyHighEndBeamPeralte);
        Values(e.RearTope, a.RearTope);
        Assert.Equal(e.DefensePieceId, a.DefensePieceId);
        Values(e.SideB, a.SideB);
        Values(e.Composite, a.Composite);
    }
    public static void Separate(PushBackDesign e, PushBackDesign a)
    {
        if(e is null){Assert.Null(a);return;}
        Assert.NotNull(a);
        Assert.NotSame(e,a);
        Separate(e.Structure,a.Structure);
        Assert.NotSame(e.Fronts,a.Fronts);
        Items(e.Fronts,a.Fronts,Separate);
        Separate(e.RearTope,a.RearTope);
        Separate(e.SideB,a.SideB);
        Separate(e.Composite,a.Composite);
    }
    public static void Mutate(PushBackDesign a)
    {
        if(a is null)return;
        Mutate(a.Structure);
        foreach(var item in a.Fronts)Mutate(item);
        a.Fronts.Clear();
        a.LegacyHighEndBeamPeralte=a.LegacyHighEndBeamPeralte + 123;
        Mutate(a.RearTope);
        a.DefensePieceId="mutated by isolation oracle";
        Mutate(a.SideB);
        Mutate(a.Composite);
    }
    public static void Values(PushBackFrontConfig e, PushBackFrontConfig a)
    {
        if(e is null){Assert.Null(a);return;}
        Assert.NotNull(a);
        Items(e.HighEndBeamPeraltes, a.HighEndBeamPeraltes, (x,y)=>Assert.Equal(x,y));
        Assert.Equal(e.DefaultPalletsDeep, a.DefaultPalletsDeep);
        Items(e.PalletsDeepOverrides, a.PalletsDeepOverrides, (x,y)=>Assert.Equal(x,y));
        Items(e.DrawPallets, a.DrawPallets, (x,y)=>Assert.Equal(x,y));
    }
    public static void Separate(PushBackFrontConfig e, PushBackFrontConfig a)
    {
        if(e is null){Assert.Null(a);return;}
        Assert.NotNull(a);
        Assert.NotSame(e,a);
        Assert.NotSame(e.HighEndBeamPeraltes,a.HighEndBeamPeraltes);
        Assert.NotSame(e.PalletsDeepOverrides,a.PalletsDeepOverrides);
        Assert.NotSame(e.DrawPallets,a.DrawPallets);
    }
    public static void Mutate(PushBackFrontConfig a)
    {
        if(a is null)return;
        a.HighEndBeamPeraltes.Clear();
        a.DefaultPalletsDeep=(a.DefaultPalletsDeep ?? 0) + 123;
        a.PalletsDeepOverrides.Clear();
        a.DrawPallets.Clear();
    }
    public static void Values(PushBackRearTopeConfig e, PushBackRearTopeConfig a)
    {
        if(e is null){Assert.Null(a);return;}
        Assert.NotNull(a);
        Assert.Equal(e.Saque, a.Saque);
        Assert.Equal(e.PieceId, a.PieceId);
        Items(e.OffCells, a.OffCells, Values);
    }
    public static void Separate(PushBackRearTopeConfig e, PushBackRearTopeConfig a)
    {
        if(e is null){Assert.Null(a);return;}
        Assert.NotNull(a);
        Assert.NotSame(e,a);
        Assert.NotSame(e.OffCells,a.OffCells);
        Items(e.OffCells,a.OffCells,Separate);
    }
    public static void Mutate(PushBackRearTopeConfig a)
    {
        if(a is null)return;
        a.Saque=a.Saque + 123;
        a.PieceId="mutated by isolation oracle";
        foreach(var item in a.OffCells)Mutate(item);
        a.OffCells.Clear();
    }
    public static void Values(PushBackSideDesign e, PushBackSideDesign a)
    {
        if(e is null){Assert.Null(a);return;}
        Assert.NotNull(a);
        Assert.Equal(e.IsPresent, a.IsPresent);
        Assert.Equal(e.LoadLevels, a.LoadLevels);
        Assert.Equal(e.FirstLevelHeight, a.FirstLevelHeight);
        Assert.Equal(e.LegacyHighEndBeamPeralte, a.LegacyHighEndBeamPeralte);
        Items(e.Fronts, a.Fronts, Values);
        Items(e.FrontConfigs, a.FrontConfigs, Values);
        Values(e.RearTope, a.RearTope);
        Assert.Equal(e.DefensePieceId, a.DefensePieceId);
    }
    public static void Separate(PushBackSideDesign e, PushBackSideDesign a)
    {
        if(e is null){Assert.Null(a);return;}
        Assert.NotNull(a);
        Assert.NotSame(e,a);
        Assert.NotSame(e.Fronts,a.Fronts);
        Items(e.Fronts,a.Fronts,Separate);
        Assert.NotSame(e.FrontConfigs,a.FrontConfigs);
        Items(e.FrontConfigs,a.FrontConfigs,Separate);
        Separate(e.RearTope,a.RearTope);
    }
    public static void Mutate(PushBackSideDesign a)
    {
        if(a is null)return;
        a.IsPresent=!a.IsPresent;
        a.LoadLevels=a.LoadLevels + 123;
        a.FirstLevelHeight=a.FirstLevelHeight + 123;
        a.LegacyHighEndBeamPeralte=a.LegacyHighEndBeamPeralte + 123;
        foreach(var item in a.Fronts)Mutate(item);
        a.Fronts.Clear();
        foreach(var item in a.FrontConfigs)Mutate(item);
        a.FrontConfigs.Clear();
        Mutate(a.RearTope);
        a.DefensePieceId="mutated by isolation oracle";
    }
    public static void Values(PushBackCompositeDesign e, PushBackCompositeDesign a)
    {
        if(e is null){Assert.Null(a);return;}
        Assert.NotNull(a);
        Assert.Equal(e.Gap, a.Gap);
        Assert.Equal(e.CentralSeparator, a.CentralSeparator);
        Assert.Equal(e.StructureOverrideA, a.StructureOverrideA);
        Assert.Equal(e.StructureOverrideB, a.StructureOverrideB);
        Items(e.AbsentSlotsA, a.AbsentSlotsA, (x,y)=>Assert.Equal(x,y));
        Items(e.AbsentSlotsB, a.AbsentSlotsB, (x,y)=>Assert.Equal(x,y));
        Items(e.Topologies, a.Topologies, Values);
        Assert.Equal(e.DefaultTopology, a.DefaultTopology);
        Assert.Equal(e.DefaultDirection, a.DefaultDirection);
    }
    public static void Separate(PushBackCompositeDesign e, PushBackCompositeDesign a)
    {
        if(e is null){Assert.Null(a);return;}
        Assert.NotNull(a);
        Assert.NotSame(e,a);
        Assert.NotSame(e.AbsentSlotsA,a.AbsentSlotsA);
        Assert.NotSame(e.AbsentSlotsB,a.AbsentSlotsB);
        Assert.NotSame(e.Topologies,a.Topologies);
        Items(e.Topologies,a.Topologies,Separate);
    }
    public static void Mutate(PushBackCompositeDesign a)
    {
        if(a is null)return;
        a.Gap=a.Gap + 123;
        a.CentralSeparator=!a.CentralSeparator;
        a.StructureOverrideA=(a.StructureOverrideA ?? 0) + 123;
        a.StructureOverrideB=(a.StructureOverrideB ?? 0) + 123;
        a.AbsentSlotsA.Clear();
        a.AbsentSlotsB.Clear();
        foreach(var item in a.Topologies)Mutate(item);
        a.Topologies.Clear();
        a.DefaultTopology=(PushBackCellTopology)99;
        a.DefaultDirection=(PushBackRunDirection)99;
    }
    public static void Values(PushBackTopologyCell e, PushBackTopologyCell a)
    {
        if(e is null){Assert.Null(a);return;}
        Assert.NotNull(a);
        Assert.Equal(e.Frente, a.Frente);
        Assert.Equal(e.Level, a.Level);
        Assert.Equal(e.Topology, a.Topology);
        Assert.Equal(e.Direction, a.Direction);
        Assert.Equal(e.CorridaDepth, a.CorridaDepth);
    }
    public static void Separate(PushBackTopologyCell e, PushBackTopologyCell a)
    {
        if(e is null){Assert.Null(a);return;}
        Assert.NotNull(a);
        Assert.NotSame(e,a);
    }
    public static void Mutate(PushBackTopologyCell a)
    {
        if(a is null)return;
        a.Frente=a.Frente + 123;
        a.Level=a.Level + 123;
        a.Topology=(PushBackCellTopology)99;
        a.Direction=(PushBackRunDirection)99;
        a.CorridaDepth=(a.CorridaDepth ?? 0) + 123;
    }
    public static void Values(CantileverLineDesign e, CantileverLineDesign a)
    {
        if(e is null){Assert.Null(a);return;}
        Assert.NotNull(a);
        Assert.Equal(e.Id, a.Id);
        Assert.Equal(e.Name, a.Name);
        Assert.Equal(e.StationCount, a.StationCount);
        Assert.Equal(e.ColumnCentreSpacing, a.ColumnCentreSpacing);
        Values(e.StationTopology, a.StationTopology);
        Values(e.DefaultArmTemplate, a.DefaultArmTemplate);
        Items(e.ArmCellOverrides, a.ArmCellOverrides, Values);
        Values(e.Bracing, a.Bracing);
        Values(e.PlantaVisibility, a.PlantaVisibility);
    }
    public static void Separate(CantileverLineDesign e, CantileverLineDesign a)
    {
        if(e is null){Assert.Null(a);return;}
        Assert.NotNull(a);
        Assert.NotSame(e,a);
        Separate(e.StationTopology,a.StationTopology);
        Separate(e.DefaultArmTemplate,a.DefaultArmTemplate);
        Assert.NotSame(e.ArmCellOverrides,a.ArmCellOverrides);
        Items(e.ArmCellOverrides,a.ArmCellOverrides,Separate);
        Separate(e.Bracing,a.Bracing);
        Separate(e.PlantaVisibility,a.PlantaVisibility);
    }
    public static void Mutate(CantileverLineDesign a)
    {
        if(a is null)return;
        a.Id=Guid.Empty;
        a.Name="mutated by isolation oracle";
        a.StationCount=a.StationCount + 123;
        a.ColumnCentreSpacing=a.ColumnCentreSpacing + 123;
        Mutate(a.StationTopology);
        Mutate(a.DefaultArmTemplate);
        foreach(var item in a.ArmCellOverrides)Mutate(item);
        a.ArmCellOverrides.Clear();
        Mutate(a.Bracing);
        Mutate(a.PlantaVisibility);
    }
    public static void Values(CantileverLineStationTopologyDesign e, CantileverLineStationTopologyDesign a)
    {
        if(e is null){Assert.Null(a);return;}
        Assert.NotNull(a);
        Assert.Equal(e.FaceMode, a.FaceMode);
        Assert.Equal(e.SingleSide, a.SingleSide);
        Values(e.ColumnBaseTemplate, a.ColumnBaseTemplate);
        Assert.Equal(e.LevelCount, a.LevelCount);
        Assert.Equal(e.FirstLevelPunchIndex, a.FirstLevelPunchIndex);
        Assert.Equal(e.RequestedClearHeight, a.RequestedClearHeight);
        Assert.Equal(e.TopClearFactor, a.TopClearFactor);
        Values(e.ColumnHeight, a.ColumnHeight);
    }
    public static void Separate(CantileverLineStationTopologyDesign e, CantileverLineStationTopologyDesign a)
    {
        if(e is null){Assert.Null(a);return;}
        Assert.NotNull(a);
        Assert.NotSame(e,a);
        Separate(e.ColumnBaseTemplate,a.ColumnBaseTemplate);
        Separate(e.ColumnHeight,a.ColumnHeight);
    }
    public static void Mutate(CantileverLineStationTopologyDesign a)
    {
        if(a is null)return;
        a.FaceMode=(CantileverStationFaceMode)99;
        a.SingleSide=(CantileverArmSide)99;
        Mutate(a.ColumnBaseTemplate);
        a.LevelCount=a.LevelCount + 123;
        a.FirstLevelPunchIndex=a.FirstLevelPunchIndex + 123;
        a.RequestedClearHeight=a.RequestedClearHeight + 123;
        a.TopClearFactor=a.TopClearFactor + 123;
        Mutate(a.ColumnHeight);
    }
    public static void Values(CantileverStationColumnBaseTemplateDesign e, CantileverStationColumnBaseTemplateDesign a)
    {
        if(e is null){Assert.Null(a);return;}
        Assert.NotNull(a);
        Assert.Equal(e.ColumnSectionId, a.ColumnSectionId);
        Values(e.ColumnBottomPlate, a.ColumnBottomPlate);
        Values(e.Base, a.Base);
        Values(e.Connection, a.Connection);
        Assert.Equal(e.BaseFollowsColumn, a.BaseFollowsColumn);
    }
    public static void Separate(CantileverStationColumnBaseTemplateDesign e, CantileverStationColumnBaseTemplateDesign a)
    {
        if(e is null){Assert.Null(a);return;}
        Assert.NotNull(a);
        Assert.NotSame(e,a);
        Separate(e.ColumnBottomPlate,a.ColumnBottomPlate);
        Separate(e.Base,a.Base);
        Separate(e.Connection,a.Connection);
    }
    public static void Mutate(CantileverStationColumnBaseTemplateDesign a)
    {
        if(a is null)return;
        a.ColumnSectionId="mutated by isolation oracle";
        Mutate(a.ColumnBottomPlate);
        Mutate(a.Base);
        Mutate(a.Connection);
        a.BaseFollowsColumn=!a.BaseFollowsColumn;
    }
    public static void Values(CantileverPlateDesign e, CantileverPlateDesign a)
    {
        if(e is null){Assert.Null(a);return;}
        Assert.NotNull(a);
        Assert.Equal(e.Thickness, a.Thickness);
    }
    public static void Separate(CantileverPlateDesign e, CantileverPlateDesign a)
    {
        if(e is null){Assert.Null(a);return;}
        Assert.NotNull(a);
        Assert.NotSame(e,a);
    }
    public static void Mutate(CantileverPlateDesign a)
    {
        if(a is null)return;
        a.Thickness=a.Thickness + 123;
    }
    public static void Values(CantileverBaseDesign e, CantileverBaseDesign a)
    {
        if(e is null){Assert.Null(a);return;}
        Assert.NotNull(a);
        Assert.Equal(e.SectionId, a.SectionId);
        Assert.Equal(e.Length, a.Length);
        Values(e.FrontPlate, a.FrontPlate);
        Values(e.RearPlate, a.RearPlate);
        Values(e.Gusset, a.Gusset);
    }
    public static void Separate(CantileverBaseDesign e, CantileverBaseDesign a)
    {
        if(e is null){Assert.Null(a);return;}
        Assert.NotNull(a);
        Assert.NotSame(e,a);
        Separate(e.FrontPlate,a.FrontPlate);
        Separate(e.RearPlate,a.RearPlate);
        Separate(e.Gusset,a.Gusset);
    }
    public static void Mutate(CantileverBaseDesign a)
    {
        if(a is null)return;
        a.SectionId="mutated by isolation oracle";
        a.Length=a.Length + 123;
        Mutate(a.FrontPlate);
        Mutate(a.RearPlate);
        Mutate(a.Gusset);
    }
    public static void Values(CantileverGussetDesign e, CantileverGussetDesign a)
    {
        if(e is null){Assert.Null(a);return;}
        Assert.NotNull(a);
        Assert.Equal(e.Thickness, a.Thickness);
    }
    public static void Separate(CantileverGussetDesign e, CantileverGussetDesign a)
    {
        if(e is null){Assert.Null(a);return;}
        Assert.NotNull(a);
        Assert.NotSame(e,a);
    }
    public static void Mutate(CantileverGussetDesign a)
    {
        if(a is null)return;
        a.Thickness=a.Thickness + 123;
    }
    public static void Values(CantileverColumnBaseConnectionDesign e, CantileverColumnBaseConnectionDesign a)
    {
        if(e is null){Assert.Null(a);return;}
        Assert.NotNull(a);
        Assert.Equal(e.Variant, a.Variant);
        Values(e.Punches, a.Punches);
    }
    public static void Separate(CantileverColumnBaseConnectionDesign e, CantileverColumnBaseConnectionDesign a)
    {
        if(e is null){Assert.Null(a);return;}
        Assert.NotNull(a);
        Assert.NotSame(e,a);
        Separate(e.Punches,a.Punches);
    }
    public static void Mutate(CantileverColumnBaseConnectionDesign a)
    {
        if(a is null)return;
        a.Variant=(CantileverColumnBaseVariantKind)99;
        Mutate(a.Punches);
    }
    public static void Values(CantileverPunchParameters e, CantileverPunchParameters a)
    {
        if(e is null){Assert.Null(a);return;}
        Assert.NotNull(a);
        Assert.Equal(e.Diameter, a.Diameter);
        Assert.Equal(e.HorizontalEndOffset, a.HorizontalEndOffset);
        Assert.Equal(e.ConnectionPitch, a.ConnectionPitch);
        Assert.Equal(e.RearPlateVerticalEndOffset, a.RearPlateVerticalEndOffset);
        Assert.Equal(e.RegularColumnPitch, a.RegularColumnPitch);
        Assert.Equal(e.ConnectionPunchesAboveBase, a.ConnectionPunchesAboveBase);
        Assert.Equal(e.ColumnBottomPlatePitch, a.ColumnBottomPlatePitch);
        Assert.Null(a.ColumnBottomPlateEndOffset);
        Assert.Null(a.ColumnTopPunchOffset);
    }
    public static void Separate(CantileverPunchParameters e, CantileverPunchParameters a)
    {
        if(e is null){Assert.Null(a);return;}
        Assert.NotNull(a);
        Assert.NotSame(e,a);
    }
    public static void Mutate(CantileverPunchParameters a)
    {
        if(a is null)return;
        a.Diameter=a.Diameter + 123;
        a.HorizontalEndOffset=a.HorizontalEndOffset + 123;
        a.ConnectionPitch=a.ConnectionPitch + 123;
        a.RearPlateVerticalEndOffset=a.RearPlateVerticalEndOffset + 123;
        a.RegularColumnPitch=a.RegularColumnPitch + 123;
        a.ConnectionPunchesAboveBase=a.ConnectionPunchesAboveBase + 123;
        a.ColumnBottomPlatePitch=a.ColumnBottomPlatePitch + 123;
    }
    public static void Values(CantileverStationColumnHeightDesign e, CantileverStationColumnHeightDesign a)
    {
        if(e is null){Assert.Null(a);return;}
        Assert.NotNull(a);
        Assert.Equal(e.Mode, a.Mode);
        Assert.Equal(e.ManualHeight, a.ManualHeight);
    }
    public static void Separate(CantileverStationColumnHeightDesign e, CantileverStationColumnHeightDesign a)
    {
        if(e is null){Assert.Null(a);return;}
        Assert.NotNull(a);
        Assert.NotSame(e,a);
    }
    public static void Mutate(CantileverStationColumnHeightDesign a)
    {
        if(a is null)return;
        a.Mode=(CantileverStationColumnHeightMode)99;
        a.ManualHeight=(a.ManualHeight ?? 0) + 123;
    }
    public static void Values(CantileverArmTemplateDesign e, CantileverArmTemplateDesign a)
    {
        if(e is null){Assert.Null(a);return;}
        Assert.NotNull(a);
        Values(e.Body, a.Body);
        Values(e.MountingPlate, a.MountingPlate);
        Values(e.EndPlate, a.EndPlate);
    }
    public static void Separate(CantileverArmTemplateDesign e, CantileverArmTemplateDesign a)
    {
        if(e is null){Assert.Null(a);return;}
        Assert.NotNull(a);
        Assert.NotSame(e,a);
        Separate(e.Body,a.Body);
        Separate(e.MountingPlate,a.MountingPlate);
        Separate(e.EndPlate,a.EndPlate);
    }
    public static void Mutate(CantileverArmTemplateDesign a)
    {
        if(a is null)return;
        Mutate(a.Body);
        Mutate(a.MountingPlate);
        Mutate(a.EndPlate);
    }
    public static void Values(CantileverArmBodyDesign e, CantileverArmBodyDesign a)
    {
        if(e is null){Assert.Null(a);return;}
        Assert.NotNull(a);
        Assert.Equal(e.Arrangement, a.Arrangement);
        Assert.Equal(e.SectionId, a.SectionId);
        Assert.Equal(e.CutLength, a.CutLength);
        Assert.Equal(e.SlopeRisePer12, a.SlopeRisePer12);
    }
    public static void Separate(CantileverArmBodyDesign e, CantileverArmBodyDesign a)
    {
        if(e is null){Assert.Null(a);return;}
        Assert.NotNull(a);
        Assert.NotSame(e,a);
    }
    public static void Mutate(CantileverArmBodyDesign a)
    {
        if(a is null)return;
        a.Arrangement=(CantileverArmBodyArrangement)99;
        a.SectionId="mutated by isolation oracle";
        a.CutLength=a.CutLength + 123;
        a.SlopeRisePer12=a.SlopeRisePer12 + 123;
    }
    public static void Values(CantileverArmMountingPlateTemplateDesign e, CantileverArmMountingPlateTemplateDesign a)
    {
        if(e is null){Assert.Null(a);return;}
        Assert.NotNull(a);
        Assert.Equal(e.Thickness, a.Thickness);
        Assert.Equal(e.VerticalPunchCount, a.VerticalPunchCount);
        Assert.Equal(e.VerticalEndOffset, a.VerticalEndOffset);
    }
    public static void Separate(CantileverArmMountingPlateTemplateDesign e, CantileverArmMountingPlateTemplateDesign a)
    {
        if(e is null){Assert.Null(a);return;}
        Assert.NotNull(a);
        Assert.NotSame(e,a);
    }
    public static void Mutate(CantileverArmMountingPlateTemplateDesign a)
    {
        if(a is null)return;
        a.Thickness=a.Thickness + 123;
        a.VerticalPunchCount=a.VerticalPunchCount + 123;
        a.VerticalEndOffset=(a.VerticalEndOffset ?? 0) + 123;
    }
    public static void Values(CantileverArmEndPlateDesign e, CantileverArmEndPlateDesign a)
    {
        if(e is null){Assert.Null(a);return;}
        Assert.NotNull(a);
        Assert.Equal(e.Mode, a.Mode);
        Assert.Equal(e.Thickness, a.Thickness);
        Assert.Equal(e.ExtraStopHeight, a.ExtraStopHeight);
    }
    public static void Separate(CantileverArmEndPlateDesign e, CantileverArmEndPlateDesign a)
    {
        if(e is null){Assert.Null(a);return;}
        Assert.NotNull(a);
        Assert.NotSame(e,a);
    }
    public static void Mutate(CantileverArmEndPlateDesign a)
    {
        if(a is null)return;
        a.Mode=(CantileverArmEndPlateMode)99;
        a.Thickness=a.Thickness + 123;
        a.ExtraStopHeight=a.ExtraStopHeight + 123;
    }
    public static void Values(CantileverArmCellOverride e, CantileverArmCellOverride a)
    {
        if(e is null){Assert.Null(a);return;}
        Assert.NotNull(a);
        Assert.Equal(e.StationIndex, a.StationIndex);
        Assert.Equal(e.LevelIndex, a.LevelIndex);
        Assert.Equal(e.Side, a.Side);
        Values(e.Arm, a.Arm);
    }
    public static void Separate(CantileverArmCellOverride e, CantileverArmCellOverride a)
    {
        if(e is null){Assert.Null(a);return;}
        Assert.NotNull(a);
        Assert.NotSame(e,a);
        Separate(e.Arm,a.Arm);
    }
    public static void Mutate(CantileverArmCellOverride a)
    {
        if(a is null)return;
        a.StationIndex=a.StationIndex + 123;
        a.LevelIndex=a.LevelIndex + 123;
        a.Side=(CantileverArmSide)99;
        Mutate(a.Arm);
    }
    public static void Values(CantileverBracingDesign e, CantileverBracingDesign a)
    {
        if(e is null){Assert.Null(a);return;}
        Assert.NotNull(a);
        Assert.Equal(e.SeparatorSectionId, a.SeparatorSectionId);
        Assert.Equal(e.PanelCountMode, a.PanelCountMode);
        Assert.Equal(e.ManualPanelCount, a.ManualPanelCount);
        Assert.Equal(e.BracedPanelHeight, a.BracedPanelHeight);
        Assert.Equal(e.CentralEmptySpaceHeight, a.CentralEmptySpaceHeight);
        Assert.Equal(e.BraceKind, a.BraceKind);
        Assert.Equal(e.BraceSectionId, a.BraceSectionId);
        Values(e.ColdRolled, a.ColdRolled);
        Assert.Equal(e.PanelLayoutMode, a.PanelLayoutMode);
        Items(e.AdvancedPanelSegments, a.AdvancedPanelSegments, Values);
    }
    public static void Separate(CantileverBracingDesign e, CantileverBracingDesign a)
    {
        if(e is null){Assert.Null(a);return;}
        Assert.NotNull(a);
        Assert.NotSame(e,a);
        Separate(e.ColdRolled,a.ColdRolled);
        Assert.NotSame(e.AdvancedPanelSegments,a.AdvancedPanelSegments);
        Items(e.AdvancedPanelSegments,a.AdvancedPanelSegments,Separate);
    }
    public static void Mutate(CantileverBracingDesign a)
    {
        if(a is null)return;
        a.SeparatorSectionId="mutated by isolation oracle";
        a.PanelCountMode=(CantileverBracedPanelCountMode)99;
        a.ManualPanelCount=(a.ManualPanelCount ?? 0) + 123;
        a.BracedPanelHeight=a.BracedPanelHeight + 123;
        a.CentralEmptySpaceHeight=a.CentralEmptySpaceHeight + 123;
        a.BraceKind=(CantileverBraceBodyKind)99;
        a.BraceSectionId="mutated by isolation oracle";
        Mutate(a.ColdRolled);
        a.PanelLayoutMode=(CantileverPanelLayoutMode)99;
        foreach(var item in a.AdvancedPanelSegments)Mutate(item);
        a.AdvancedPanelSegments.Clear();
    }
    public static void Values(CantileverColdRolledBraceDesign e, CantileverColdRolledBraceDesign a)
    {
        if(e is null){Assert.Null(a);return;}
        Assert.NotNull(a);
        Assert.Equal(e.Diameter, a.Diameter);
    }
    public static void Separate(CantileverColdRolledBraceDesign e, CantileverColdRolledBraceDesign a)
    {
        if(e is null){Assert.Null(a);return;}
        Assert.NotNull(a);
        Assert.NotSame(e,a);
    }
    public static void Mutate(CantileverColdRolledBraceDesign a)
    {
        if(a is null)return;
        a.Diameter=a.Diameter + 123;
    }
    public static void Values(CantileverPanelSegmentDesign e, CantileverPanelSegmentDesign a)
    {
        if(e is null){Assert.Null(a);return;}
        Assert.NotNull(a);
        Assert.Equal(e.StartElevation, a.StartElevation);
        Assert.Equal(e.EndElevation, a.EndElevation);
        Assert.Equal(e.BracingMode, a.BracingMode);
    }
    public static void Separate(CantileverPanelSegmentDesign e, CantileverPanelSegmentDesign a)
    {
        if(e is null){Assert.Null(a);return;}
        Assert.NotNull(a);
        Assert.NotSame(e,a);
    }
    public static void Mutate(CantileverPanelSegmentDesign a)
    {
        if(a is null)return;
        a.StartElevation=a.StartElevation + 123;
        a.EndElevation=a.EndElevation + 123;
        a.BracingMode=(CantileverPanelBracingMode)99;
    }
    public static void Values(CantileverPlantaVisibilityDesign e, CantileverPlantaVisibilityDesign a)
    {
        if(e is null){Assert.Null(a);return;}
        Assert.NotNull(a);
        Assert.Equal(e.ShowArms, a.ShowArms);
        Assert.Equal(e.ShowBraces, a.ShowBraces);
    }
    public static void Separate(CantileverPlantaVisibilityDesign e, CantileverPlantaVisibilityDesign a)
    {
        if(e is null){Assert.Null(a);return;}
        Assert.NotNull(a);
        Assert.NotSame(e,a);
    }
    public static void Mutate(CantileverPlantaVisibilityDesign a)
    {
        if(a is null)return;
        a.ShowArms=!a.ShowArms;
        a.ShowBraces=!a.ShowBraces;
    }
    private static void Items<T>(IEnumerable<T> e,IEnumerable<T> a,Action<T,T> check)
    {
        if(e is null){Assert.Null(a);return;}
        Assert.NotNull(a);var left=e.ToArray();var right=a.ToArray();Assert.Equal(left.Length,right.Length);
        for(int i=0;i<left.Length;i++)check(left[i],right[i]);
    }
}
