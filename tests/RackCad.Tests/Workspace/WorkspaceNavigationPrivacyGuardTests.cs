#nullable enable
using System;
using System.Linq;
using System.Text.RegularExpressions;
using Xunit;
using Src = RackCad.Tests.WorkspaceHostSource;

namespace RackCad.Tests;

// I-64 F2-T1-NAVIGATION-CORE: INV-36 / D-05. NavigationTargets and its neutral facts are private to the panel: no metrics,
// ProjectSummary, BOM, TotalRacks, aggregation, list or command outside the panel may consume them, and no I-63 dependency.
public class WorkspaceNavigationPrivacyGuardTests
{
    private static readonly string[] NavigationNames =
    {
        "NavigationTargets", "NavigationTargetSet", "NavigationTarget", "NavigationDefinitionFact", "INavigationFactsPort",
        "NavigationDiagnostic", "NavigationDiagnosticKind", "NavigationViewOrder", "NavigationRevalidation",
        "NavigationRevalidationVerdict", "NavigationActionPolicy", "NavigationActionPlan", "NavigationActionDecision",
        "NavigationActionKind", "NavigationActionVerdict", "INavigationEffects",
    };

    private static bool IsPanelPath(string path) =>
        path.StartsWith("src/RackCad.Application/Workspace/", StringComparison.Ordinal)
        || path.StartsWith("src/RackCad.Plugin/Workspace/", StringComparison.Ordinal)
        || path.StartsWith("src/RackCad.UI/Workspace/", StringComparison.Ordinal);

    private static Regex AnyNavigationName() =>
        new(@"\b(?:" + string.Join("|", NavigationNames.Select(Regex.Escape)) + @")\b");

    [Fact]
    public void NoFileOutsideThePanelFoldersConsumesTheNavigationAuthorityOrItsFacts()
    {
        var pattern = AnyNavigationName();

        var offenders = Src.AllProduct()
            .Where(f => !IsPanelPath(f.Path) && pattern.IsMatch(f.Code))
            .Select(f => f.Path)
            .ToArray();

        Assert.Empty(offenders);
        Assert.True(Src.AllProduct().Count(f => IsPanelPath(f.Path)) > 0);
    }

    [Theory]
    [InlineData("StructuralAnalysis")]
    [InlineData("ComputedParameter")]
    [InlineData("ProjectSummary")]
    [InlineData("TotalRacks")]
    [InlineData("Bom")]
    public void NoConsumerOfThatAreaMentionsTheNavigationAuthority(string area)
    {
        var consumers = Src.AllProduct()
            .Where(f => !IsPanelPath(f.Path)
                        && (f.Path.IndexOf(area, StringComparison.OrdinalIgnoreCase) >= 0
                            || f.Code.IndexOf(area, StringComparison.OrdinalIgnoreCase) >= 0))
            .ToArray();
        var pattern = AnyNavigationName();

        Assert.NotEmpty(consumers);
        Assert.All(consumers, f => Assert.DoesNotMatch(pattern, f.Code));
    }

    [Fact]
    public void TheStructuralAnalysisFolderHasNoReferenceToTheNavigationAuthority()
    {
        var files = Src.Files("src/RackCad.Application/StructuralAnalysis");
        var pattern = AnyNavigationName();

        Assert.NotEmpty(files);
        Assert.All(files, f => Assert.DoesNotMatch(pattern, f.Code));
    }

    [Fact]
    public void NoCommandOutsideThePanelMentionsTheAuthority()
    {
        var commands = Src.AllProduct()
            .Where(f => f.Path.StartsWith("src/RackCad.Plugin/", StringComparison.Ordinal) && !IsPanelPath(f.Path))
            .ToArray();
        var pattern = AnyNavigationName();

        Assert.NotEmpty(commands);
        Assert.All(commands, f => Assert.DoesNotMatch(pattern, f.Code));
    }

    [Fact]
    public void TheNavigationFilesHaveNoMetricProjectSummaryAggregationOrFoundationVocabulary()
    {
        var folder = "src/RackCad.Application/Workspace/";
        var forbidden = new Regex(
            @"ComputedParameter|Metric|ProjectSummary|TotalRacks|Aggregat|Provider|Bom|Foundation|I63|I-63",
            RegexOptions.IgnoreCase);
        var files = new[] { "NavigationFacts.cs", "NavigationTargetSet.cs", "NavigationTargets.cs", "NavigationActions.cs" };

        foreach (var file in files) Assert.DoesNotMatch(forbidden, Src.Code(folder + file));
    }

    [Fact]
    public void TheNavigationFilesHaveNoDependencyOnStructuralAnalysisOrOnTheClassicEditors()
    {
        var folder = "src/RackCad.Application/Workspace/";
        var forbidden = new Regex(@"StructuralAnalysis|RackSiblingMembership|RackBlockFinder|RackListBuilder|RackEmbedStore|Autodesk\.|RackCad\.Plugin|RackCad\.UI");
        var files = new[] { "NavigationFacts.cs", "NavigationTargetSet.cs", "NavigationTargets.cs", "NavigationActions.cs" };

        foreach (var file in files) Assert.DoesNotMatch(forbidden, Src.Code(folder + file));
    }

    [Fact]
    public void NoNavigationTypeIsAPublicSharedApiOutsideTheWorkspaceNamespace()
    {
        var types = typeof(RackCad.Application.Workspace.NavigationTargets).Assembly.GetTypes()
            .Where(t => NavigationNames.Contains(t.Name))
            .ToArray();

        Assert.Equal(NavigationNames.Length, types.Length);
        Assert.All(types, t => Assert.Equal("RackCad.Application.Workspace", t.Namespace));
    }
}
