using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using RackCad.Application.Persistence;
using RackCad.Application.Systems.Shared;
using RackCad.Application.Views.Placement;
using RackCad.Domain.Systems.Shared;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>
    /// G16 corrective round, C16-05 (Coordinator decision). A logical rack whose Name is null, empty or whitespace is NOT projectable:
    /// the whole RACKPROYECTAR operation is refused before any point, import or write, with no synthetic rack name. ID19 adds a linked
    /// view of the SAME rack; it must not turn «(sin nombre)» into «Rack» or any other persisted name. AUTH-15 still requires a
    /// nonblank <c>RackEmbedDocument.Name</c>, so the requirement is enforced before materialization.
    /// </summary>
    public class G16UnnamedRackTests
    {
        private const string UnnamedCode = "UnnamedRackNotProjectable";

        public static IEnumerable<object[]> BlankNames() => new[]
        {
            new object[] { null },
            new object[] { "" },
            new object[] { "   " },
        };

        public static IEnumerable<object[]> KindsAndBlankNames()
        {
            foreach (var kind in new[]
            {
                RackSystemKind.SelectiveRack, RackSystemKind.PalletFlow, RackSystemKind.PushBack,
                RackSystemKind.Cantilever, RackSystemKind.Selective,
            })
            {
                foreach (var name in new string[] { null, "", "   " })
                {
                    yield return new object[] { kind, name };
                }
            }
        }

        // ================================================================ A. the unnamed rack is refused before points

        [Theory]
        [MemberData(nameof(BlankNames))]
        public void G16_C05_AnUnnamedSelectiveIsRefusedBeforeAnyPointImportOrWrite(string name)
        {
            var scenario = new G15.Scenario(RackSystemKind.SelectiveRack, DimensionViewKind.Lateral);
            scenario.AddRack(G14.RackA, "REF-1", 0, 0, name: name);
            var port = scenario.NewPort(G15.Points());

            var result = RackProjectionCommandRun.Execute(port);

            Assert.Equal(RackProjectionCommandStatus.Blocked, result.Status);
            Assert.Contains(result.PlanResult.Diagnostics, d => d.Code.ToString() == UnnamedCode && d.RackId == G14.RackA);
            AssertNothingHappened(port, scenario);
        }

        // ================================================================ B. fail whole, all offenders

        [Fact]
        public void G16_C05_ANamedAndAnUnnamedRackFailTheWholeOperationAndEveryOffenderIsReported()
        {
            var scenario = new G15.Scenario(RackSystemKind.SelectiveRack, DimensionViewKind.Planta);
            scenario.AddRack(G14.RackA, "REF-1", 0, 0, name: "Rack A");
            scenario.AddRack(G14.RackB, "REF-2", 0, 100, name: "");
            scenario.AddRack(G14.RackC, "REF-3", 0, 200, name: "Rack C");
            var port = scenario.NewPort(G15.Points());

            var result = RackProjectionCommandRun.Execute(port);

            Assert.Equal(RackProjectionCommandStatus.Blocked, result.Status);
            var offenders = result.PlanResult.Diagnostics.Where(d => d.Code.ToString() == UnnamedCode).Select(d => d.RackId).ToList();
            Assert.Equal(new[] { G14.RackB }, offenders);
            AssertNothingHappened(port, scenario);
            Assert.Contains(port.Printed, line => line.Contains(G14.RackB));
        }

        [Fact]
        public void G16_C05_SeveralUnnamedRacksAreAllListed()
        {
            var scenario = new G15.Scenario(RackSystemKind.SelectiveRack, DimensionViewKind.Planta);
            scenario.AddRack(G14.RackA, "REF-1", 0, 0, name: null);
            scenario.AddRack(G14.RackB, "REF-2", 0, 100, name: "Rack B");
            scenario.AddRack(G14.RackC, "REF-3", 0, 200, name: "  ");

            var result = RackProjectionCommandRun.Execute(scenario.NewPort(G15.Points()));

            var offenders = result.PlanResult.Diagnostics.Where(d => d.Code.ToString() == UnnamedCode).Select(d => d.RackId).OrderBy(x => x).ToList();
            Assert.Equal(new[] { G14.RackA, G14.RackC }.OrderBy(x => x).ToList(), offenders);
        }

        // ================================================================ C. a named rack is still projected

        [Fact]
        public void G16_C05_ANamedSelectiveStillProjectsPlantaToLateral()
        {
            var scenario = new G15.Scenario(RackSystemKind.SelectiveRack, DimensionViewKind.Lateral);
            scenario.AddRack(G14.RackA, "REF-1", 0, 0, name: "Rack A");
            var port = scenario.NewPort(G15.Points());

            var result = RackProjectionCommandRun.Execute(port);

            Assert.True(result.IsCompleted, string.Join(" | ", port.Printed));
            Assert.Equal(1, port.Scopes.Commits);
            Assert.Equal(G14.RackA, Assert.Single(port.Scopes.Created).RackId);
        }

        // ================================================================ D. nothing is repaired, renamed or invented

        [Fact]
        public void G16_C05_TheUnnamedSourceEnvelopeIsUnchangedAfterTheRefusal()
        {
            var scenario = new G15.Scenario(RackSystemKind.SelectiveRack, DimensionViewKind.Planta);
            scenario.AddRack(G14.RackA, "REF-1", 0, 0, name: "");
            var before = scenario.Definitions.Select(d => d.RawPayload).ToList();

            RackProjectionCommandRun.Execute(scenario.NewPort(G15.Points()));

            Assert.Equal(before, scenario.Definitions.Select(d => d.RawPayload).ToList());
            Assert.All(scenario.Definitions, d => Assert.Equal(string.Empty, new RackEmbedStore().Deserialize(d.RawPayload).Name));
        }

        [Fact]
        public void G16_C05_NoSyntheticNameIsEverInvented()
        {
            Assert.Null(RackProjectionEnvelopeName.LogicalName("", new[] { null, " " }));
            Assert.Null(RackProjectionEnvelopeName.LogicalName(null, null));

            var unnamed = new RackEmbedDocument { Id = "R", Kind = "selective", Name = "", View = "planta", Section = -1, Design = "{}" };
            var same = RackProjectionEnvelopeName.WithLogicalName(unnamed, new[] { "" });

            Assert.Same(unnamed, same);
            Assert.True(string.IsNullOrWhiteSpace(same.Name));
        }

        [Fact]
        public void G16_C05_ABlankViewOfANamedRackKeepsTheNameAnotherViewOfThatRackCarries()
        {
            var blank = new RackEmbedDocument { Id = "R", Kind = "selective", Name = "", View = "planta", Section = -1, Design = "{}" };

            var composed = RackProjectionEnvelopeName.WithLogicalName(blank, new[] { "", "Rack A" });

            Assert.Equal("Rack A", composed.Name);
            Assert.Equal(string.Empty, blank.Name);
        }

        // ================================================================ cross-system matrix

        [Theory]
        [MemberData(nameof(KindsAndBlankNames))]
        public void G16_C05_EverySupportedSystemRefusesAnUnnamedRackBeforePointsImportAndWrites(RackSystemKind kind, string name)
        {
            var scenario = new G15.Scenario(kind, DimensionViewKind.Planta);
            scenario.AddRack(G14.RackA, "REF-1", 0, 0, name: name);
            var port = scenario.NewPort(G15.Points());

            var result = RackProjectionCommandRun.Execute(port);

            Assert.Equal(RackProjectionCommandStatus.Blocked, result.Status);
            Assert.Contains(result.PlanResult.Diagnostics, d => d.Code.ToString() == UnnamedCode);
            AssertNothingHappened(port, scenario);
        }

        [Theory]
        [InlineData(RackSystemKind.SelectiveRack)]
        [InlineData(RackSystemKind.PalletFlow)]
        [InlineData(RackSystemKind.PushBack)]
        [InlineData(RackSystemKind.Cantilever)]
        [InlineData(RackSystemKind.Selective)]
        public void G16_C05_EverySupportedSystemStillProjectsANamedRack(RackSystemKind kind)
        {
            var scenario = new G15.Scenario(kind, DimensionViewKind.Planta);
            scenario.AddRack(G14.RackA, "REF-1", 0, 0, name: "Rack A");

            var result = RackProjectionCommandRun.Execute(scenario.NewPort(G15.Points()));

            Assert.True(result.IsCompleted);
        }

        // ================================================================ where the policy lives

        [Fact]
        public void G16_C05_ThePolicyIsProductPolicyInThePurePlanNotInTheFoundationNorInAuth15()
        {
            var pipeline = Code("src/RackCad.Application/Views/Placement/RackProjectionPipeline.cs");
            var creator = Code("src/RackCad.Plugin/Systems/Shared/RackDefinitionCreator.cs");

            Assert.Contains("RackProjectionFailureCode.UnnamedRackNotProjectable", pipeline);
            Assert.True(
                pipeline.IndexOf("UnnamedRackNotProjectable", StringComparison.Ordinal)
                < pipeline.IndexOf("request.Services.Authored(", StringComparison.Ordinal),
                "the refusal must come before any authority is consulted");
            Assert.Contains("string.IsNullOrWhiteSpace(envelope.Name)", creator);
        }

        [Fact]
        public void G16_C05_NoOtherLiteralOrNamingAuthorityFillsTheRackName()
        {
            var sessions = Code("src/RackCad.Plugin/Views/RackProjectionKindSessions.cs");
            var plan = Code("src/RackCad.Application/Persistence/RackDuplicationPlan.cs");
            var name = Code("src/RackCad.Application/Views/Placement/RackProjectionEnvelopeName.cs");

            Assert.DoesNotContain("FallbackName", sessions);
            Assert.DoesNotContain("internal static string FallbackName", plan);
            Assert.DoesNotContain("FallbackName", name);
            Assert.DoesNotContain("\"Rack\"", name);
        }

        // ================================================================ helpers

        private static void AssertNothingHappened(G15.Port port, G15.Scenario scenario)
        {
            Assert.DoesNotContain("pick-base", port.Events);
            Assert.DoesNotContain("pick-target", port.Events);
            Assert.DoesNotContain("before-write", port.Events);
            Assert.DoesNotContain("import", port.Events);
            Assert.Equal(0, port.Scopes.Begun);
            Assert.Empty(port.Scopes.Created);
            Assert.Empty(port.Scopes.Placed);
            Assert.Equal(0, port.Scopes.Commits);
            Assert.Empty(scenario.Services.ResolveCalls);
            Assert.Empty(scenario.Services.PrepareCalls);
        }

        private static string Code(string relative)
        {
            var dir = new DirectoryInfo(AppContext.BaseDirectory);
            while (dir != null && !File.Exists(Path.Combine(dir.FullName, "RackCad.sln"))) dir = dir.Parent;
            Assert.NotNull(dir);
            return string.Join("\n", File.ReadAllLines(Path.Combine(dir.FullName, relative.Replace('/', Path.DirectorySeparatorChar)))
                .Select(line => line.Contains("//") ? line.Substring(0, line.IndexOf("//", StringComparison.Ordinal)) : line));
        }
    }
}
