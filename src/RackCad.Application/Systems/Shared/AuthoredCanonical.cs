using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Persistence;
using RackCad.Domain.RackFrames;
using RackCad.Domain.Systems.Dynamic;
using RackCad.Domain.Systems.PushBack;
using RackCad.Domain.Systems.Cantilever;
using RackCad.Domain.Systems.Selective;
using RackCad.Domain.Systems.Shared;

namespace RackCad.Application.Systems.Shared;

// Only documented legacy defaults and F-08 exclusions. No domain->document round trip is an equality authority.
internal static class AuthoredCanonical
{
    private static void Require(bool condition, string path)
    {
        if (!condition) throw AuthoredRawReader.Invalid(path, "unaccredited or lossy authored value");
    }
    private static IEnumerable<T> Items<T>(List<T> list) => list ?? Enumerable.Empty<T>();
    private static void Positive(double? value, string path) => Require(!value.HasValue || value > 0, path);
    private static void Nonnegative(double? value, string path) => Require(!value.HasValue || value >= 0, path);
    private static void NonblankString(string value, string path)
        => Require(value == null || !string.IsNullOrWhiteSpace(value), path);
    private static void StableString(string value, string path)
        => Require(value == null || (!string.IsNullOrWhiteSpace(value) && value == value.Trim()), path);

    internal static DynamicRackSystemDocument Dynamic(DynamicRackSystemDocument d)
    {
        Require(d != null && d.Fronts != null && d.Fronts.Count > 0, "Dynamic.Fronts");
        Nonnegative(d.PostPeralte, "Dynamic.PostPeralte");
        Positive(d.AnnotationScale, "Dynamic.AnnotationScale");
        NonblankString(d.PalletWeightUnit, "Dynamic.PalletWeightUnit");
        NonblankString(d.InOutBeamCatalogId, "Dynamic.InOutBeamCatalogId");
        Require(!d.Dimensions.HasValue || Enum.IsDefined((DimensionDetail)d.Dimensions.Value), "Dynamic.Dimensions");
        d.PalletWeightUnit ??= "kg";
        d.InOutBeamCatalogId ??= DynamicRackDefaults.InOutBeamCatalogId;
        d.LoadLevels ??= DynamicRackDefaults.DefaultLoadLevels;
        d.FirstLevelHeight ??= DynamicRackDefaults.DefaultFirstLevelHeight;
        d.BeamDepth ??= DynamicRackDefaults.LegacyDefaultBeamDepth;
        d.PalletTolerance ??= DynamicRackDefaults.DefaultPalletTolerance;
        d.PostPeralte ??= 0;
        d.AnnotationScale ??= 1;
        d.NumberFronts ??= false;
        d.NumberLevels ??= false;
        d.DrawRackName ??= false;
        d.Dimensions ??= (int)DimensionDetail.None;
        foreach (var n in Items(d.IntermediateBeamDepths)) Positive(n, "Dynamic.IntermediateBeamDepths");
        foreach (var f in d.Fronts) Front(f, d.LoadLevels.Value, d.PalletsDeep);
        foreach (var m in Items(d.Modules))
        {
            Require(m != null, "Dynamic.Modules");
            m.UseCalculatedHeaderConfiguration ??= m.Header == null;
            Require(m.UseCalculatedHeaderConfiguration.Value || m.Header != null, "Dynamic.Module.Header provenance");
            if (m.Header != null) Header(m.Header);
            bool isHeader = m.Kind is DynamicRackModuleKind.HeaderStart or DynamicRackModuleKind.HeaderIntermediate or DynamicRackModuleKind.HeaderEnd;
            // Raw validation of the entire header has already completed, even on the excluded path.
            if (isHeader && m.UseCalculatedHeaderConfiguration.Value && d.PostPeralte > 0) m.Header = null;
        }
        foreach (var h in Items(d.HeaderLineOverrides))
        {
            Require(h != null && h.Header != null && !string.IsNullOrWhiteSpace(h.ModuleId), "Dynamic.HeaderLineOverrides");
            Header(h.Header);
        }
        foreach (var h in Items(d.DerivedPostLineOverrides))
            Require(h != null && h.Height > 0, "Dynamic.DerivedPostLineOverrides");
        foreach (var s in Items(d.SafetySelections)) Safety(s);
        return d;
    }

    private static void Front(DynamicRackFrontDocument f, int levels, int deep)
    {
        Require(f != null && f.PalletCount > 0, "Dynamic.Front.PalletCount");
        Require(!f.LoadLevels.HasValue || f.LoadLevels > 0, "Dynamic.Front.LoadLevels");
        Require(!f.PalletsDeep.HasValue || f.PalletsDeep >= 2, "Dynamic.Front.PalletsDeep");
        Require(!f.DepthStartPosition.HasValue || f.DepthStartPosition > 0, "Dynamic.Front.DepthStartPosition");
        f.IsActive ??= true;
        f.LoadLevels ??= Math.Max(1, levels);
        f.PalletsDeep ??= Math.Max(2, deep);
        f.DepthStartPosition ??= 1;
        f.Bfr = null;
        foreach (var n in Items(f.IntermediateBeamDepths)) Positive(n, "Dynamic.Front.IntermediateBeamDepths");
    }

    internal static RackFrameProjectDocument Header(RackFrameProjectDocument h)
    {
        Require(h != null, "Header");
        var defaults = new RackFrameConfiguration();
        // No physical model is created. Header presence is not collapsed into a default header.
        NonblankString(h.Units, "Header.Units");
        h.SchemaVersion = "1.0";
        h.Units ??= "in";
        h.PostPeralte ??= 0;
        h.CelosiaStartTroquel ??= defaults.CelosiaStartTroquel;
        h.DiagonalStartOffsetTroqueles ??= defaults.DiagonalStartOffsetTroqueles;
        h.DiagonalEndOffsetTroqueles ??= defaults.DiagonalEndOffsetTroqueles;
        h.DiagonalDoubleSpacingTroqueles ??= defaults.DiagonalDoubleSpacingTroqueles;
        h.HorizontalDoubleOffsetTroqueles ??= defaults.HorizontalDoubleOffsetTroqueles;
        h.PasoTroquel ??= defaults.PasoTroquel;
        h.PanelClear ??= defaults.PanelClear;
        return h;
    }

    private static void Side(int? side, string path) => Require(!side.HasValue || Enum.IsDefined((SafetySide)side.Value), path);
    private static void Placement(int? side, string path) => Require(!side.HasValue || Enum.IsDefined((BootPlacement)side.Value), path);
    private static void Cells(List<GridCellDocument> cells)
    {
        foreach (var c in Items(cells)) Require(c != null && c.Frente >= 0 && c.Level >= 0, "Safety.OffCells");
    }
    private static void Safety(SafetySelectionDocument s)
    {
        Require(s != null && !string.IsNullOrWhiteSpace(s.ElementId), "Safety.ElementId");
        Require(s.DerivedAisles == null || s.DerivedAisles.Count == 0, "Safety.DerivedAisles");
        Side(s.Side, "Safety.Side"); Side(s.AuthoredSide, "Safety.AuthoredSide");
        s.Side = s.AuthoredSide ?? s.Side ?? (int)SafetySide.Both;
        s.AuthoredSide = s.Side;
        foreach (var p in Items(s.PostSides))
        {
            Require(p != null && p.PostIndex >= 0, "Safety.PostSides");
            Side(p.Side, "Safety.PostSides.Side"); p.Side ??= (int)SafetySide.Both;
        }
        Placement(s.BotaPlacement, "Safety.BotaPlacement"); Placement(s.BotaBPlacement, "Safety.BotaBPlacement");
        foreach (var p in Items(s.BotaPosts).Concat(Items(s.BotaBPosts)))
        {
            Require(p != null && p.PostIndex >= 0, "Safety.BotaPosts"); Placement(p.Placement, "Safety.BotaPosts.Placement");
        }
        Positive(s.TopeSaque, "Safety.TopeSaque"); Positive(s.DesviadorLongitud, "Safety.DesviadorLongitud");
        Positive(s.DesviadorPrimerNivelAltura, "Safety.DesviadorPrimerNivelAltura");
        s.TopeShared ??= true; s.TopeSaque ??= SelectiveSafetyDefaults.TopeSaque;
        s.TopeFrontal ??= false; s.TopeFondo ??= -1;
        s.DesviadorLongitud ??= SelectiveSafetyDefaults.DesviadorLongitud;
        s.DesviadorPrimerNivelAltura ??= SelectiveSafetyDefaults.DesviadorPrimerNivelAltura;
        s.ParrillaFrontal ??= true; s.ParrillaLateral ??= true;
        s.ParrillaFrente ??= 0; s.ParrillaCantidad ??= 0; s.BotaSidesDeclared ??= false;
        foreach (var d in Items(s.DefensaPosts))
        {
            Require(d != null && d.PostIndex >= 0, "Safety.DefensaPosts");
            Nonnegative(d.ExitLength, "Safety.ExitLength"); Nonnegative(d.EntranceLength, "Safety.EntranceLength");
            d.ExitLength ??= 0; d.EntranceLength ??= 0; d.ExitAuto ??= false; d.EntranceAuto ??= false;
        }
        Cells(s.TopeOffCells); Cells(s.DesviadorOffCells); Cells(s.GuiaEntradaOffCells); Cells(s.ParrillaOffCells);
    }

    internal static PushBackDesignDocument PushBack(PushBackDesignDocument d)
    {
        Require(d != null, "PushBack");
        d.SchemaVersion = "1.0";
        Dynamic(d.Structure);
        Positive(d.LegacyHighEndBeamPeralte, "PushBack.LegacyHighEndBeamPeralte");
        Positive(d.RearTopeSaque, "PushBack.RearTopeSaque");
        d.LegacyHighEndBeamPeralte ??= PushBackDefaults.HighEndBeamDefaultPeralte;
        d.RearTopeSaque ??= PushBackDefaults.RearTopeSaque;
        StableString(d.RearTopePieceId, "PushBack.RearTopePieceId"); StableString(d.DefensePieceId, "PushBack.DefensePieceId");
        for (int i = 0; i < (d.Fronts?.Count ?? 0); i++)
            PushFront(d.Fronts[i], i < d.Structure.Fronts.Count ? d.Structure.Fronts[i].LoadLevels : null);
        // The top-level list is an optional override vector. The existing writer omits the entire
        // vector when no entry has content; retain every position as soon as any entry has content.
        if (d.Fronts != null && !d.Fronts.Any(f => (f.HighEndBeamPeraltes?.Count ?? 0) > 0 ||
            f.DefaultPalletsDeep.HasValue || (f.PalletsDeepOverrides?.Any(n => n.HasValue) ?? false) ||
            (f.DrawPallets?.Any(n => n == true) ?? false))) d.Fronts = null;
        if (d.SideB != null)
        {
            var b = d.SideB;
            b.IsPresent ??= true; b.LoadLevels ??= DynamicRackDefaults.DefaultLoadLevels;
            b.FirstLevelHeight ??= PushBackDefaults.DefaultFirstLevelHeight;
            Positive(b.LegacyHighEndBeamPeralte, "SideB.LegacyHighEndBeamPeralte");
            Positive(b.RearTopeSaque, "SideB.RearTopeSaque");
            b.LegacyHighEndBeamPeralte ??= PushBackDefaults.HighEndBeamDefaultPeralte;
            b.RearTopeSaque ??= PushBackDefaults.RearTopeSaque;
            StableString(b.RearTopePieceId, "SideB.RearTopePieceId"); StableString(b.DefensePieceId, "SideB.DefensePieceId");
            foreach (var f in Items(b.Fronts)) if (f != null) Front(f, b.LoadLevels.Value, DynamicRackDefaults.DefaultPalletsDeep);
            for (int i = 0; i < (b.FrontConfigs?.Count ?? 0); i++)
                if (b.FrontConfigs[i] != null)
                    PushFront(b.FrontConfigs[i], i < (b.Fronts?.Count ?? 0) ? b.Fronts[i]?.LoadLevels : null);
            if (b.FrontConfigs != null && b.FrontConfigs.All(f => f == null)) b.FrontConfigs = null;
        }
        if (d.Composite != null)
        {
            var c = d.Composite;
            Nonnegative(c.Gap, "Composite.Gap"); c.Gap ??= 0; c.CentralSeparator ??= false;
            c.DefaultTopology = Known<PushBackCellTopology>(c.DefaultTopology, PushBackCellTopology.Encontradas);
            c.DefaultDirection = Known<PushBackRunDirection>(c.DefaultDirection, PushBackRunDirection.AToB);
            foreach (var t in Items(c.Topologies))
            {
                t.Topology = Known(t.Topology, Enum.Parse<PushBackCellTopology>(c.DefaultTopology));
                t.Direction = Known(t.Direction, Enum.Parse<PushBackRunDirection>(c.DefaultDirection));
            }
        }
        return d;
    }
    private static string Known<T>(string value, T fallback) where T : struct, Enum
    {
        if (value == null) return fallback.ToString();
        Require(Enum.TryParse<T>(value, true, out var parsed) && Enum.IsDefined(parsed), "PushBack.enum");
        return parsed.ToString();
    }
    private static void PushFront(PushBackFrontDocument f, int? levels)
    {
        Require(f != null, "PushBack.Fronts");
        // F-09: retain every in-range position; only trailing inheritance outside a known level range is absent.
        if (levels.HasValue)
        {
            TrimUnusedTail(f.HighEndBeamPeraltes, levels.Value, n => !n.HasValue);
            TrimUnusedTail(f.PalletsDeepOverrides, levels.Value, n => !n.HasValue);
            TrimUnusedTail(f.DrawPallets, levels.Value, n => n != true);
        }
        if (f.PalletsDeepOverrides != null && f.PalletsDeepOverrides.All(n => !n.HasValue)) f.PalletsDeepOverrides = null;
        if (f.DrawPallets != null)
        {
            if (!f.DrawPallets.Any(n => n == true)) f.DrawPallets = null;
            else for (int i = 0; i < f.DrawPallets.Count; i++) f.DrawPallets[i] ??= false;
        }
    }

    private static void TrimUnusedTail<T>(List<T> values, int count, Func<T, bool> absent)
    {
        if (values == null) return;
        while (values.Count > count && absent(values[values.Count - 1])) values.RemoveAt(values.Count - 1);
    }

    internal static CantileverLineDocument Cantilever(CantileverLineDocument d)
    {
        Require(d?.Line != null && d.Line.Id != Guid.Empty, "Cantilever.Line.Id");
        d.SchemaVersion = "1.0";
        var l = d.Line;
        Require(l.StationTopology != null && l.DefaultArmTemplate != null && l.Bracing != null && l.PlantaVisibility != null, "Cantilever.structural null");
        var t = l.StationTopology;
        Require(t.ColumnHeight != null && t.ColumnBaseTemplate != null, "Cantilever.StationTopology");
        var cb = t.ColumnBaseTemplate;
        Require(cb.ColumnBottomPlate != null && cb.Base != null && cb.Connection?.Punches != null, "Cantilever.ColumnBaseTemplate");
        Require(cb.Base.FrontPlate != null && cb.Base.RearPlate != null && cb.Base.Gusset != null, "Cantilever.Base");
        cb.Connection.Punches.ColumnBottomPlateEndOffset = null;
        cb.Connection.Punches.ColumnTopPunchOffset = null;
        Arm(l.DefaultArmTemplate);
        foreach (var a in Items(l.ArmCellOverrides)) if (a.Arm != null) Arm(a.Arm);
        Require(l.Bracing.ColdRolled != null, "Cantilever.Bracing.ColdRolled");
        l.ArmCellOverrides ??= new List<CantileverArmCellOverride>();
        l.Bracing.AdvancedPanelSegments ??= new List<CantileverPanelSegmentDesign>();
        return d;
    }
    private static void Arm(CantileverArmTemplateDesign a)
        => Require(a.Body != null && a.MountingPlate != null && a.EndPlate != null, "Cantilever.Arm");
}
