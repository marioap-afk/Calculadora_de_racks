using System;
using System.Linq;
using RackCad.Application.Systems.Shared;
using Xunit;
using static RackCad.Tests.I58F1Fixtures;
using static RackCad.Tests.I58F1Oracles;
using O = RackCad.Application.Systems.Shared.RackAuthoredComparisonOutcome;
namespace RackCad.Tests;

public sealed class I58F2BoundaryTests
{
    [Theory]
    [InlineData("HighEndBeamPeraltes", "[null,4]", "[null,4,null]")]
    [InlineData("PalletsDeepOverrides", "[null,4]", "[null,4,null]")]
    [InlineData("DrawPallets", "[false,true]", "[false,true,null]")]
    public void F09_trailing_inherit_outside_levels_has_no_new_intent(string field, string before, string after)
    {
        string a = Set(Set(Rich("pushback"), "PushBack.Structure.Fronts.0.LoadLevels", "2"), "PushBack.Fronts.0." + field, before);
        string b = Set(a, "PushBack.Fronts.0." + field, after);
        var r = RackAuthoredComparatorPorts.PushBack().Compare(ProductInput(Input(Sibling("pushback", a), Sibling("pushback", b, "B")), "pushback"));
        Assert.Equal(O.Single, r.Outcome);
        Assert.Equal(2, field switch { "HighEndBeamPeraltes" => r.Authored.Fronts[0].HighEndBeamPeraltes.Count,
            "PalletsDeepOverrides" => r.Authored.Fronts[0].PalletsDeepOverrides.Count, _ => r.Authored.Fronts[0].DrawPallets.Count });
    }

    [Theory]
    [InlineData("PalletWeightUnit")]
    [InlineData("InOutBeamCatalogId")]
    public void Authored_nonblank_strings_are_not_trimmed_or_rejected(string field)
    {
        string raw = Set(Rich("dynamic"), "DynamicSystem." + field, "\" authored with spaces \"");
        var r = RackAuthoredComparatorPorts.Dynamic().Compare(ProductInput(Input(Sibling("dynamic", raw)), "dynamic"));
        Assert.Equal(O.Single, r.Outcome);
        Assert.Equal(" authored with spaces ", field == "PalletWeightUnit" ? r.Authored.Pallet.WeightUnit : r.Authored.InOutBeamCatalogId);
    }
    [Fact]
    public void Header_units_preserve_nonblank_ordinal_text()
    {
        string raw = Set(Rich("cabecera"), "Header.Units", "\" in \"");
        var r = RackAuthoredComparatorPorts.Cabecera().Compare(ProductInput(Input(Sibling("cabecera", raw)), "cabecera"));
        Assert.Equal(O.Single, r.Outcome);
        Assert.Equal(" in ", r.Authored.Units);
    }
    [Theory]
    [InlineData("PushBack.LegacyHighEndBeamPeralte", "0")]
    [InlineData("PushBack.RearTopeSaque", "0")]
    [InlineData("PushBack.SideB.LegacyHighEndBeamPeralte", "0")]
    [InlineData("PushBack.SideB.RearTopeSaque", "0")]
    public void Writer_loss_is_rejected_before_accreditation(string path, string value)
    {
        string raw = Set(Rich("pushback"), path, value);
        var r = RackAuthoredComparatorPorts.PushBack().Compare(ProductInput(Input(Sibling("pushback", raw)), "pushback"));
        Assert.Equal(O.Unreadable, r.Outcome);
        Assert.Null(r.Authored);
    }
    [Fact]
    public void Optional_front_overrides_with_no_content_follow_the_writer_absence_rule()
    {
        string a = Set(Rich("pushback"), "PushBack.Fronts", "null");
        string b = Set(a, "PushBack.Fronts", "[{}]");
        Assert.Null(Store.Deserialize(b).PushBackDesign.Fronts[0].DefaultPalletsDeep);
        string reopened = Store.Serialize(Store.Deserialize(b));
        var r = RackAuthoredComparatorPorts.PushBack().Compare(ProductInput(Input(Sibling("pushback", a), Sibling("pushback", b, "B"), Sibling("pushback", reopened, "C")), "pushback"));
        Assert.Equal(O.Single, r.Outcome);
        Assert.Empty(r.Authored.Fronts);
    }
    [Theory]
    [InlineData("dynamic", "DynamicSystem.PalletDepth", "\"48\"")]
    [InlineData("dynamic", "DynamicSystem.PalletsDeep", "1.5")]
    [InlineData("dynamic", "DynamicSystem.Modules.0.IsCalculated", "1")]
    [InlineData("dynamic", "DynamicSystem.FirstLevelHeight", "{}")]
    [InlineData("dynamic", "DynamicSystem.Fronts", "null")]
    [InlineData("dynamic", "DynamicSystem.Modules", "{}")]
    [InlineData("dynamic", "DynamicSystem.PalletDepth", "1e999")]
    [InlineData("dynamic", "DynamicSystem.SafetySelections.0.PostSides.0.PostIndex", "-1")]
    [InlineData("pushback", "PushBack.SideB.FrontConfigs", "true")]
    [InlineData("pushback", "PushBack.ExtensionData", "{\"Future\":1}")]
    [InlineData("cantilever", "Cantilever.Line.StationTopology", "null")]
    [InlineData("cantilever", "Cantilever.Line.Bracing.AdvancedPanelSegments.0.Height", "12")]
    [InlineData("cabecera", "Header.Panels", "[null]")]
    [InlineData("cabecera", "Header.Panels.0.Arrangement", "\"Future\"")]
    public void Strict_shape_rejects_lossy_members_before_mapping(string kind, string path, string value)
    {
        string raw = Set(Rich(kind), path, value);
        var input = ProductInput(Input(Sibling(kind, raw)), kind);
        var actual = kind switch {
            "dynamic" => RackAuthoredComparatorPorts.Dynamic().Compare(input).Outcome,
            "pushback" => RackAuthoredComparatorPorts.PushBack().Compare(input).Outcome,
            "cantilever" => RackAuthoredComparatorPorts.Cantilever().Compare(input).Outcome,
            _ => RackAuthoredComparatorPorts.Cabecera().Compare(input).Outcome };
        Assert.Equal(O.Unreadable, actual);
    }
    [Fact]
    public void Cantilever_null_arm_override_stays_null_without_inventing_a_template()
    {
        string raw = Set(Rich("cantilever"), "Cantilever.Line.ArmCellOverrides.0.Arm", "null");
        var r = RackAuthoredComparatorPorts.Cantilever().Compare(ProductInput(Input(Sibling("cantilever", raw)), "cantilever"));
        Assert.Equal(O.Single, r.Outcome);
        Assert.Null(r.Authored.ArmCellOverrides[0].Arm);
    }
}
