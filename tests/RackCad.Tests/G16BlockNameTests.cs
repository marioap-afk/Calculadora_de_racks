using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using RackCad.Application;
using RackCad.Application.Persistence;
using RackCad.Application.RackFrames;
using RackCad.Application.Systems.Selective;
using RackCad.Application.Systems.Shared;
using RackCad.Application.Views.Placement;
using RackCad.Domain.Systems.Shared;
using Xunit;
using static RackCad.Tests.DimensionViewScenarios;

namespace RackCad.Tests
{
    /// <summary>
    /// G16 corrective round (OV-ID19-01, second failure). AUTH-15 rejected a projected Selective lateral with
    /// <c>InvalidBlockName: nombre de bloque vacio</c>: the source rack had no name, so <c>LinkedLateral(null, post)</c> returned
    /// null, and — unlike the Dynamic and Push Back laterals, which fall back to a generated AUTH-11 name — the Selective lateral
    /// had no fallback in the shared authority. (The legacy insertion path only worked because it substituted the literal
    /// «Selectivo» itself, in the Plugin.) Envelope <c>Name</c> and block BaseName are distinct authorities.
    ///
    /// <para>
    /// Historical oracle, from <c>RackSelectivoCommands.InsertSelectiveLateralSection</c>: the base name is the rack name, or
    /// «Selectivo» when there is none, and the section name is <c>LinkedLateral(base, pick − 1)</c> where the prompt number
    /// <c>pick</c> is the PHYSICAL post number (the cut is found by <c>PostIndex == pick − 1</c>), so the suffix is the physical
    /// <c>PostIndex + 1</c> and never the position in the list of cuts.
    /// </para>
    /// </summary>
    public class G16BlockNameTests
    {
        public static IEnumerable<object[]> BlankNames() => new[]
        {
            new object[] { null },
            new object[] { "" },
            new object[] { "   " },
        };

        // ================================================================ Selective

        [Theory]
        [MemberData(nameof(BlankNames))]
        public void G16_C04_AnUnnamedSelectiveNamesEveryTargetView(string rackName)
        {
            foreach (var twoFondos in new[] { false, true })
            {
                var system = Selective(DimensionDetail.Standard, twoFondos, Catalog);
                foreach (var address in SelectiveTargets(system))
                {
                    var name = RackViewProductNames.Selective(system, address, rackName, Trimmed(rackName));

                    Assert.False(string.IsNullOrWhiteSpace(name), address + " (fondos: " + (twoFondos ? 2 : 1) + ")");
                    Assert.False(string.IsNullOrWhiteSpace(BlockNaming.SanitizeBlockName(name)));
                }
            }
        }

        [Fact]
        public void G16_C04_AnUnnamedSelectiveLateralFollowsTheHistoricalOracle()
        {
            var system = Selective(DimensionDetail.Standard, twoFondos: false, Catalog);

            foreach (var cut in new SelectiveLateralBuilder().Cortes(system, Catalog))
            {
                var name = RackViewProductNames.Selective(system, RackViewAddress.Post(cut.PostIndex), null, null);

                // The physical post number, exactly as the historical prompt used it.
                Assert.Equal("Selectivo - lateral " + (cut.PostIndex + 1), name);
            }
        }

        [Fact]
        public void G16_C04_TheLateralSuffixIsThePhysicalPostIndexNotTheOrdinalOfTheCut()
        {
            var system = Selective(DimensionDetail.Standard, twoFondos: true, Catalog);
            var posts = new SelectiveLateralBuilder().Cortes(system, Catalog).Select(cut => cut.PostIndex).ToList();

            Assert.True(posts.Count > 1);
            Assert.Equal(posts.Distinct().Count(), posts.Count);
            foreach (var post in posts)
            {
                var unnamed = RackViewProductNames.Selective(system, RackViewAddress.Post(post), null, null);
                var named = RackViewProductNames.Selective(system, RackViewAddress.Post(post), "Rack A", "Rack A");

                Assert.EndsWith("lateral " + (post + 1), unnamed);
                Assert.Equal("Rack A - lateral " + (post + 1), named);
            }

            // A post that is not a physical cut keeps its own number: the name never renumbers by position.
            var last = posts.Max();
            Assert.Equal("Selectivo - lateral " + (last + 1), RackViewProductNames.Selective(system, RackViewAddress.Post(last), null, null));
        }

        [Fact]
        public void G16_C04_ANamedSelectiveKeepsItsEstablishedLinkedNames()
        {
            var one = Selective(DimensionDetail.Standard, twoFondos: false, Catalog);
            var two = Selective(DimensionDetail.Standard, twoFondos: true, Catalog);

            Assert.Equal("Rack A - planta", RackViewProductNames.Selective(one, RackViewAddress.Whole(DimensionViewKind.Planta), "Rack A", "Rack A"));
            Assert.Equal("Rack A - lateral 1", RackViewProductNames.Selective(one, RackViewAddress.Post(0), "Rack A", "Rack A"));
            Assert.Equal("Rack A", RackViewProductNames.Selective(one, RackViewAddress.Fondo(0), "Rack A", "Rack A"));
            Assert.Equal("Rack A - frente F2", RackViewProductNames.Selective(two, RackViewAddress.Fondo(1), "Rack A", "Rack A"));
        }

        // ================================================================ every system, unnamed and named

        [Theory]
        [MemberData(nameof(BlankNames))]
        public void G16_C04_EveryUnnamedRackHasANonBlankBlockNameForEverySupportedTarget(string rackName)
        {
            var names = AllSystemNames(rackName, Trimmed(rackName));

            Assert.NotEmpty(names);
            Assert.All(names, entry => Assert.False(string.IsNullOrWhiteSpace(entry.Value), entry.Key));
            Assert.All(names, entry => Assert.False(string.IsNullOrWhiteSpace(BlockNaming.SanitizeBlockName(entry.Value)), entry.Key));
        }

        [Fact]
        public void G16_C04_EveryNamedRackHasANonBlankBlockNameForEverySupportedTarget()
        {
            var names = AllSystemNames("Rack A", "Rack A");

            Assert.All(names, entry => Assert.False(string.IsNullOrWhiteSpace(entry.Value), entry.Key));
        }

        [Fact]
        public void G16_C04_TheNamedRacksOfTheOtherSystemsKeepTheirHistoricalNames()
        {
            var dynamic = Dynamic(DimensionDetail.Standard, Catalog);

            Assert.Equal("Rack A - planta", RackViewProductNames.Dynamic(dynamic, RackViewAddress.Whole(DimensionViewKind.Planta), "Rack A", "Rack A"));
            Assert.Equal("Rack A - lateral 1", RackViewProductNames.Dynamic(dynamic, RackViewAddress.Post(0), "Rack A", "Rack A"));
            Assert.Equal("Rack A - frontal salida", RackViewProductNames.Dynamic(dynamic, RackViewAddress.FlowEnd(RackFlowEnd.Exit), "Rack A", "Rack A"));
            Assert.Equal(
                RackViewBaseName.CantileverGenerated(RackCad.Application.Systems.Cantilever.CantileverViewKind.Planta, -1, "Rack A"),
                RackViewProductNames.Cantilever(RackViewAddress.Whole(DimensionViewKind.Planta), "Rack A"));
        }

        // ================================================================ the rack name in RACKLISTA (C16-05)

        [Fact]
        public void G16_C05_AnUnnamedRackStaysSinNombreAndANamedRackKeepsItsNameAfterAProjectedView()
        {
            var unnamed = new RackEmbedDocument { Id = "R", Kind = "selective", Name = "", View = "planta", Section = -1, Design = "{}" };
            Assert.Equal("(sin nombre)", RackListBuilder.Build(new[] { unnamed }).Single().Name);
            // An unnamed rack is not projectable: no linked view (and no name) is ever added to it.
            Assert.Null(RackProjectionEnvelopeName.LogicalName(unnamed.Name, new[] { unnamed.Name }));

            var named = new RackEmbedDocument { Id = "N", Kind = "selective", Name = "Rack A", View = "planta", Section = -1, Design = "{}" };
            var projectedName = RackProjectionEnvelopeName.LogicalName(named.Name, new[] { named.Name });
            var projected = new RackEmbedDocument { Id = "N", Kind = "selective", Name = projectedName, View = "lateral", Section = 0, Design = "{}" };
            var after = RackListBuilder.Build(new[] { named, projected }).Single();

            Assert.Equal("Rack A", after.Name);
            Assert.Equal(named.Name, projected.Name);
        }

        // ================================================================ where the choice is made

        [Fact]
        public void G16_C04_ThePluginChoosesNamesOnlyThroughTheSharedAuthorityAndInventsNoString()
        {
            foreach (var file in new[] { "Views/RackViewBatchProducts.cs", "Views/RackProjectionKindSessions.cs" })
            {
                var code = Code("src/RackCad.Plugin/" + file);

                Assert.DoesNotContain("\"Selectivo\"", code);
                Assert.DoesNotContain("\"Selectivo lateral", code);
                Assert.Contains("RackViewProductNames.", code);
            }
        }

        [Fact]
        public void G16_C04_TheSharedAuthorityOwnsTheSelectiveLateralFallback()
        {
            var authority = Code("src/RackCad.Application/Systems/Shared/RackViewBaseName.cs");

            Assert.Contains("public static string SelectiveLateral(", authority);
        }

        // ================================================================ helpers

        private static string Trimmed(string rackName) => string.IsNullOrWhiteSpace(rackName) ? null : rackName.Trim();

        private static IEnumerable<RackViewAddress> SelectiveTargets(RackCad.Domain.Systems.Selective.SelectiveRackSystem system)
        {
            yield return RackViewAddress.Whole(DimensionViewKind.Planta);
            for (var fondo = 0; fondo < SelectiveDepthLayout.Count(system); fondo++) yield return RackViewAddress.Fondo(fondo);
            foreach (var cut in new SelectiveLateralBuilder().Cortes(system, Catalog)) yield return RackViewAddress.Post(cut.PostIndex);
        }

        private static IReadOnlyDictionary<string, string> AllSystemNames(string rackName, string baseName)
        {
            var names = new Dictionary<string, string>();
            var selective = Selective(DimensionDetail.Standard, twoFondos: true, Catalog);
            foreach (var address in SelectiveTargets(selective))
                names["selective " + address] = RackViewProductNames.Selective(selective, address, rackName, baseName);

            var dynamic = Dynamic(DimensionDetail.Standard, Catalog);
            foreach (var address in new[]
            {
                RackViewAddress.Whole(DimensionViewKind.Planta), RackViewAddress.FlowEnd(RackFlowEnd.Exit),
                RackViewAddress.FlowEnd(RackFlowEnd.Entrance), RackViewAddress.Post(0),
            })
                names["dynamic " + address] = RackViewProductNames.Dynamic(dynamic, address, rackName, baseName);

            var pushBack = PushBackSingleSided(DimensionDetail.Standard, Catalog);
            foreach (var address in new[]
            {
                RackViewAddress.Whole(DimensionViewKind.Planta),
                RackViewAddress.PushBackCut(RackPushBackEnd.EntradaSalida, RackPushBackSide.A),
                RackViewAddress.PushBackCut(RackPushBackEnd.Posterior, RackPushBackSide.A), RackViewAddress.Post(0),
            })
                names["pushback " + address] = RackViewProductNames.PushBack(pushBack, address, rackName, baseName);

            var header = new HardcodedStandardRackFrameService().CreateDefault();
            foreach (var address in new[] { RackViewAddress.Whole(DimensionViewKind.Planta), RackViewAddress.Whole(DimensionViewKind.Lateral) })
                names["header " + address] = RackViewProductNames.Header(Catalog, header, address, rackName);

            foreach (var address in new[]
            {
                RackViewAddress.Whole(DimensionViewKind.Planta), RackViewAddress.Whole(DimensionViewKind.Frontal), RackViewAddress.Station(0),
            })
                names["cantilever " + address] = RackViewProductNames.Cantilever(address, rackName);

            return names;
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
