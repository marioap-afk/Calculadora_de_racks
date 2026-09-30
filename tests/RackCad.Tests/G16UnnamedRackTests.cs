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
    /// G16, legacy unnamed racks (Owner product decision). The C16-05 refusal (<c>UnnamedRackNotProjectable</c>, candidate
    /// <c>3f06994a</c>) was REVOKED: a logical rack whose <c>RackEmbedDocument.Name</c> is null, empty or whitespace is a valid,
    /// existing product state («(sin nombre)» in RACKLISTA and RACKBOMTOTAL; identity is RackId + Kind) and RACKPROYECTAR MUST project
    /// it. The projected sibling keeps the same RackId and stays unnamed: no synthetic «Rack», «Selectivo» or «Sin nombre» is ever
    /// persisted as its logical Name. AUTH-15 accepts that envelope since I-52-AUTH15-C1 (integrated). The physical block BaseName
    /// (AUTH-11) is a separate authority and is never blank (C16-04, see <see cref="G16BlockNameTests"/>).
    /// </summary>
    public class G16UnnamedRackTests
    {
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

        // ================================================================ an unnamed rack is a valid ID19 source

        [Theory]
        [MemberData(nameof(BlankNames))]
        public void G16_UNNAMED_AnUnnamedSelectiveProjectsPlantaToLateral(string name)
        {
            var scenario = new G15.Scenario(RackSystemKind.SelectiveRack, DimensionViewKind.Lateral);
            scenario.AddRack(G14.RackA, "REF-1", 0, 0, name: name);
            var port = scenario.NewPort(G15.Points());

            var result = RackProjectionCommandRun.Execute(port);

            Assert.True(result.IsCompleted, string.Join(" | ", port.Printed));
            Assert.Equal(1, port.Scopes.Commits);
            Assert.Equal(G14.RackA, Assert.Single(port.Scopes.Created).RackId);
            Assert.Single(port.Scopes.Placed);
        }

        [Fact]
        public void G16_UNNAMED_NamedAndUnnamedRacksProjectTogetherInOneOperation()
        {
            var scenario = new G15.Scenario(RackSystemKind.SelectiveRack, DimensionViewKind.Planta);
            scenario.AddRack(G14.RackA, "REF-1", 0, 0, name: "Rack A");
            scenario.AddRack(G14.RackB, "REF-2", 0, 100, name: "");
            scenario.AddRack(G14.RackC, "REF-3", 0, 200, name: null);
            var port = scenario.NewPort(G15.Points());

            var result = RackProjectionCommandRun.Execute(port);

            Assert.True(result.IsCompleted, string.Join(" | ", port.Printed));
            Assert.Equal(
                new[] { G14.RackA, G14.RackB, G14.RackC }.OrderBy(x => x),
                port.Scopes.Created.Select(view => view.RackId).OrderBy(x => x));
            Assert.Equal(1, port.Scopes.Commits);
        }

        [Theory]
        [MemberData(nameof(KindsAndBlankNames))]
        public void G16_UNNAMED_EverySupportedSystemProjectsAnUnnamedRack(RackSystemKind kind, string name)
        {
            var scenario = new G15.Scenario(kind, DimensionViewKind.Planta);
            scenario.AddRack(G14.RackA, "REF-1", 0, 0, name: name);

            var result = RackProjectionCommandRun.Execute(scenario.NewPort(G15.Points()));

            Assert.True(result.IsCompleted);
            Assert.DoesNotContain(result.PlanResult.Diagnostics, d => d.Code.ToString().IndexOf("Unnamed", StringComparison.Ordinal) >= 0);
        }

        [Fact]
        public void G16_UNNAMED_TheSourceEnvelopeIsUnchangedByTheProjection()
        {
            var scenario = new G15.Scenario(RackSystemKind.SelectiveRack, DimensionViewKind.Planta);
            scenario.AddRack(G14.RackA, "REF-1", 0, 0, name: "");
            var before = scenario.Definitions.Select(d => d.RawPayload).ToList();

            RackProjectionCommandRun.Execute(scenario.NewPort(G15.Points()));

            Assert.Equal(before, scenario.Definitions.Select(d => d.RawPayload).ToList());
            Assert.All(scenario.Definitions, d => Assert.Equal(string.Empty, new RackEmbedStore().Deserialize(d.RawPayload).Name));
        }

        // ================================================================ no synthetic logical name, anywhere

        [Fact]
        public void G16_UNNAMED_NoSyntheticNameIsEverInvented()
        {
            Assert.Null(RackProjectionEnvelopeName.LogicalName("", new[] { null, " " }));
            Assert.Null(RackProjectionEnvelopeName.LogicalName(null, null));

            var unnamed = new RackEmbedDocument { Id = "R", Kind = "selective", Name = "", View = "planta", Section = -1, Design = "{}" };
            var same = RackProjectionEnvelopeName.WithLogicalName(unnamed, new[] { "" });

            Assert.Same(unnamed, same);
            Assert.True(string.IsNullOrWhiteSpace(same.Name));
        }

        [Fact]
        public void G16_UNNAMED_ABlankViewOfANamedRackTakesTheNameAnotherViewOfThatRackCarries()
        {
            var blank = new RackEmbedDocument { Id = "R", Kind = "selective", Name = "", View = "planta", Section = -1, Design = "{}" };

            var composed = RackProjectionEnvelopeName.WithLogicalName(blank, new[] { "", "Rack A" });

            Assert.Equal("Rack A", composed.Name);
            Assert.Equal(string.Empty, blank.Name);
        }

        // ================================================================ RACKLISTA and BOM grouping are unchanged

        [Theory]
        [MemberData(nameof(BlankNames))]
        public void G16_UNNAMED_RackListaShowsSinNombreBeforeAndAfterAProjectedSiblingAndTheRackCountIsUnchanged(string name)
        {
            var source = new RackEmbedDocument { Id = "R", Kind = "selective", Name = name, View = "planta", Section = -1, Design = "{}" };
            var other = new RackEmbedDocument { Id = "N", Kind = "selective", Name = "Rack N", View = "planta", Section = -1, Design = "{}" };
            var before = RackListBuilder.Build(new[] { source, other });

            // The projected sibling is composed exactly as the ID19 session composes it (same RackId, the rack's own name state).
            var composed = RackProjectionEnvelopeName.WithLogicalName(source, new[] { source.Name });
            var projected = RackEmbedComposer.Compose(composed, composed.Kind, composed.Id, composed.Name, "lateral", 0, composed.Design);
            var after = RackListBuilder.Build(new[] { source, other, projected });

            Assert.Equal(before.Count, after.Count);
            Assert.Equal("(sin nombre)", before.Single(entry => entry.Id == "R").Name);
            Assert.Equal("(sin nombre)", after.Single(entry => entry.Id == "R").Name);
            Assert.Equal("Rack N", after.Single(entry => entry.Id == "N").Name);
            Assert.True(string.IsNullOrWhiteSpace(projected.Name));
            Assert.Equal("R", projected.Id);
        }

        [Fact]
        public void G16_UNNAMED_BomTotalGroupsRacksByRackIdNotByName()
        {
            var bom = Code("src/RackCad.Plugin/RackInventarioCommands.BomTotal.cs");

            Assert.Contains("siblings[entry.RackId] = group;", bom);
            Assert.Contains("string.IsNullOrWhiteSpace(rack.Embed.Name) ? \"(sin nombre)\"", bom);
        }

        // ================================================================ the revoked policy is gone

        [Fact]
        public void G16_UNNAMED_TheRevokedUnnamedRackPolicyNoLongerExists()
        {
            Assert.DoesNotContain("UnnamedRackNotProjectable", Enum.GetNames(typeof(RackProjectionFailureCode)));
            Assert.Null(typeof(RackProjectionServices).GetProperty("LogicalName"));

            foreach (var file in new[]
            {
                "src/RackCad.Application/Views/Placement/RackProjectionPipeline.cs",
                "src/RackCad.Application/Views/Placement/RackProjectionRemedy.cs",
                "src/RackCad.Plugin/Views/RackProjectionKindSessions.cs",
                "src/RackCad.Plugin/Views/RackProjectionSnapshotReader.cs",
            })
            {
                Assert.DoesNotContain("UnnamedRackNotProjectable", Code(file));
                Assert.DoesNotContain("Services.LogicalName", Code(file));
            }
        }

        [Fact]
        public void G16_UNNAMED_NoNamingAuthorityFillsTheLogicalRackName()
        {
            var sessions = Code("src/RackCad.Plugin/Views/RackProjectionKindSessions.cs");
            var plan = Code("src/RackCad.Application/Persistence/RackDuplicationPlan.cs");
            var name = Code("src/RackCad.Application/Views/Placement/RackProjectionEnvelopeName.cs");

            Assert.DoesNotContain("FallbackName", sessions);
            Assert.DoesNotContain("internal static string FallbackName", plan);
            Assert.DoesNotContain("FallbackName", name);
            Assert.DoesNotContain("\"Rack\"", name);
            Assert.DoesNotContain("\"Sin nombre\"", name);
            Assert.DoesNotContain("\"Selectivo\"", name);
        }

        // ================================================================ helpers

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
