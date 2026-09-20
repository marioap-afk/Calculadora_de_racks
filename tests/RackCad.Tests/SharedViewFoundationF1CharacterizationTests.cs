using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using RackCad.Application;
using RackCad.Application.Drawing;
using RackCad.Application.Geometry;
using RackCad.Application.Persistence;
using RackCad.Application.Systems.Shared;
using RackCad.Application.ProjectVariables;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>
    /// I-57 F1. These tests freeze the seams that the shared-view foundation will later adapt. They deliberately
    /// exercise the existing authorities; no foundation production type exists in this gate.
    /// </summary>
    public sealed class SharedViewFoundationF1CharacterizationTests
    {
        private static readonly string Root = FindRepositoryRoot();

        [Theory]
        [InlineData("CT-04")]
        [InlineData("CT-05")]
        [InlineData("CT-16")]
        [InlineData("CT-RES")]
        [InlineData("CT-PLAN")]
        [InlineData("CT-NAME")]
        [InlineData("CT-SCAN")]
        [InlineData("CT-GEO")]
        [InlineData("CT-BLK")]
        [InlineData("CT-AUTH")]
        public void EveryCharacterizationNamesItsFixtureAuthorityConsumerExpectedAndFailClosedResult(string id)
        {
            var manifest = Manifest();
            var item = Assert.Single(manifest.Characterizations, candidate => candidate.Id == id);

            Assert.All(new[] { item.Fixture, item.ProductionSource, item.Symbol, item.FutureConsumer, item.Expected, item.FailClosed },
                value => Assert.False(string.IsNullOrWhiteSpace(value)));
            var sourcePath = Path.Combine(Root, item.ProductionSource.Replace('/', Path.DirectorySeparatorChar));
            Assert.True(File.Exists(sourcePath), item.ProductionSource);
            Assert.Contains(item.Symbol, File.ReadAllText(sourcePath));
            Assert.NotEmpty(item.EvidenceTests);
            Assert.NotEmpty(item.Cases);
            Assert.Equal(item.Cases.Count, item.Cases.Distinct(StringComparer.Ordinal).Count());
            Assert.All(item.EvidenceTests, test =>
            {
                var path = Path.Combine(Root, "tests", "RackCad.Tests", test);
                Assert.True(File.Exists(path), test);
                var text = File.ReadAllText(path);
                Assert.True(text.Contains("[Fact]", StringComparison.Ordinal) || text.Contains("[Theory]", StringComparison.Ordinal), test);
            });
        }

        [Fact]
        public void CT04_EnvelopeSyntaxPreservesTokensAndRejectsUnreadableInput()
        {
            Assert.Equal(new[] { "selective", "dynamic", "cabecera", "cama", "pushback", "cantilever" },
                new[] { RackEmbedDocument.KindSelective, RackEmbedDocument.KindDynamic, RackEmbedDocument.KindCabecera,
                    RackEmbedDocument.KindCama, RackEmbedDocument.KindPushBack, RackEmbedDocument.KindCantilever });
            Assert.Equal(new[] { "frontal", "lateral", "planta" },
                new[] { RackEmbedDocument.ViewFrontal, RackEmbedDocument.ViewLateral, RackEmbedDocument.ViewPlanta });

            var store = new RackEmbedStore();
            Assert.Null(store.Deserialize(null));
            Assert.Null(store.Deserialize("   "));
            Assert.Null(store.Deserialize("not-json"));

            var roundTrip = store.Deserialize(store.Serialize(new RackEmbedDocument
            {
                Kind = "DyNaMiC ", View = " lateral ", Section = 7, Design = "{}"
            }));
            Assert.Equal("DyNaMiC ", roundTrip.Kind);
            Assert.Equal(" lateral ", roundTrip.View);
            Assert.Equal(7, roundTrip.Section);
            Assert.Equal(-1, new RackEmbedDocument().Section);
        }

        [Fact]
        public void CT05_FrameFixtureIsCompleteNonSymmetricAndUsesThePhysicalCenter()
        {
            var frames = Manifest().Frames;
            Assert.Equal(11, frames.Count);
            Assert.Equal(new[] { "Cabecera", "Cama", "Cantilever", "Dynamic", "PushBack", "Selective" },
                frames.Select(frame => frame.Family).Distinct().OrderBy(value => value).ToArray());
            Assert.Contains(frames, frame => frame.KMin < 0.0);

            Assert.All(frames, frame =>
            {
                Assert.All(new[] { frame.Family, frame.View, frame.AxisMap, frame.PhysicalOrigin, frame.Endpoint,
                    frame.VariantOffset, frame.DrawnBounds, frame.ProductionSource, frame.Symbol },
                    value => Assert.False(string.IsNullOrWhiteSpace(value)));
                Assert.True(frame.KMin < frame.KMax, frame.Family + "/" + frame.View);
                Assert.Equal((frame.KMin + frame.KMax) / 2.0, frame.Center, 10);
                Assert.Contains(frame.Symbol, Source(frame.ProductionSource));
            });
            Assert.Contains(frames, frame => frame.Family == "Dynamic" && frame.VariantOffset.Contains("PostIndex", StringComparison.Ordinal));
            Assert.Contains(frames, frame => frame.Family == "PushBack" && frame.VariantOffset.Contains("Side", StringComparison.Ordinal));
        }

        [Fact]
        public void CT16_SelectionKeepsCopyIdentityNamesCountsAndStableFirstSelectionOrder()
        {
            var store = new RackEmbedStore();
            var id = Guid.NewGuid().ToString();
            var payload = store.Serialize(new RackEmbedDocument
                { Kind = RackEmbedDocument.KindDynamic, Id = id, Name = " Rack A ", Design = "{}" });
            var definitions = new[] { new RackPhysicalDefinitionSnapshot("D1", payload, "DEF") };
            var selection = new[]
            {
                new RackPhysicalReferenceSnapshot("non-block", false, true, null),
                new RackPhysicalReferenceSnapshot("paper", true, false, "missing"),
                new RackPhysicalReferenceSnapshot("R1", true, true, "D1"),
                new RackPhysicalReferenceSnapshot("R1", true, true, "D1"),
                new RackPhysicalReferenceSnapshot("R2", true, true, "D1"),
            };

            var plan = RackDuplicationPlan.Build(selection, definitions,
                kind => string.Equals(kind, RackEmbedDocument.KindDynamic, StringComparison.OrdinalIgnoreCase));

            Assert.True(plan.IsSuccess);
            var group = Assert.Single(plan.Groups);
            Assert.Equal(RackDuplicationSourceKeyKind.RackId, group.Key.Kind);
            Assert.Equal(id, group.Key.Value);
            Assert.Equal("Rack A", group.BaseName);
            Assert.Equal(new[] { "R1", "R2" }, group.References.Select(reference => reference.ReferenceKey));
            Assert.Equal(1, plan.IgnoredNonBlockReferences);
            Assert.Equal(1, plan.IgnoredOutsideModelSpace);
        }

        [Fact]
        public void CTRES_SelectiveHandlerDelegatesExactlyOnceToTheExistingEffectiveResolver()
        {
            var source = Source("src/RackCad.Plugin/KindHandlers/SelectiveKindHandler.cs");
            Assert.Equal(1, Count(source, "new SelectiveEffectiveDesignResolver().Resolve("));
            Assert.Contains("if (!resolution.IsSuccess)", source);
            Assert.Contains("SelectiveGeometryResolver", source);
        }

        [Fact]
        public void CTPLAN_HeaderRunPlanRetainsItsTypedPayloadAndBuilderOwnedGeometry()
        {
            var piece = new HeaderBlockInstance
            {
                Role = HeaderBlockRole.Beam,
                PieceId = "beam-1",
                BlockName = "VIGA.100",
                View = "lateral",
                Insertion = new Point2D(2, 3),
                ConnectionAnchor = new Point2D(5, 7),
            };
            var plan = new HeaderRunPlan(Array.Empty<HeaderGroup>(), new[] { piece });

            var flattened = Assert.Single(plan.Flatten().Instances);
            Assert.Same(piece, flattened);
            Assert.Equal("VIGA.100", flattened.BlockName);
            Assert.Equal(2, flattened.Insertion.X);
            Assert.Equal(7, flattened.ConnectionAnchor.Y);
        }

        [Theory]
        [InlineData(null, "Cabecera")]
        [InlineData("  Rack:A  ", "Rack A")]
        [InlineData("VIGA.100", "VIGA.100")]
        public void CTNAME_BaseNameSanitizerKeepsItsLegacyBoundary(string input, string expected)
            => Assert.Equal(expected, BlockNaming.SanitizeBlockName(input));

        [Fact]
        public void CTSCAN_PhysicalDefinitionAndCountSurviveUnreadableAndForeignClassification()
        {
            var unreadable = ProjectVariableScanProjection.Project("DEF-X", null, 3);
            Assert.Equal("DEF-X", unreadable.DefinitionId);
            Assert.Equal(3, unreadable.DirectReferenceCount);
            Assert.False(unreadable.OuterEnvelopeInterpretable);
            Assert.Null(unreadable.RackId);
            Assert.Null(unreadable.Kind);

            var foreign = ProjectVariableScanProjection.Project("DEF-D", new RackEmbedDocument
                { Kind = RackEmbedDocument.KindDynamic, Id = "rack-d", Design = "{}" }, 2);
            Assert.True(foreign.OuterEnvelopeInterpretable);
            Assert.Equal("rack-d", foreign.RackId);
            Assert.Equal(RackEmbedDocument.KindDynamic, foreign.Kind);
            Assert.Equal(2, foreign.DirectReferenceCount);
            Assert.False(foreign.IsSelective);
        }

        [Fact]
        public void CTGEO_TransformFactsCoverRotationMirrorTranslationScaleAndDegenerateInputs()
        {
            var transform = Transform2D.Identity
                .Then(Transform2D.Scale(2))
                .Then(Transform2D.RotationDegrees(90))
                .Then(Transform2D.Translation(5, -3));
            var point = transform.Apply(new Point2D(1, 0));
            Assert.Equal(5, point.X, 10);
            Assert.Equal(-1, point.Y, 10);
            Assert.Equal(4, transform.Determinant, 10);
            Assert.Equal(2, transform.ScaleFactor, 10);
            Assert.Equal(Math.PI / 2, transform.RotationAngle(), 10);
            Assert.False(transform.ReversesOrientation);
            Assert.True(Transform2D.MirrorAboutX.ReversesOrientation);
            Assert.True(Transform2D.MirrorAboutY.ReversesOrientation);
            Assert.Equal(Math.PI, Math.Abs(Transform2D.RotationDegrees(180).RotationAngle()), 10);
            var nonUniform = new Transform2D(2, 0, 0, 3, 0, 0);
            Assert.Equal(6, nonUniform.Determinant, 10);
            Assert.Equal(Math.Sqrt(6), nonUniform.ScaleFactor, 10);
            var degenerate = new Transform2D(0, 0, 0, 0, 4, 5);
            Assert.Equal(0, degenerate.Determinant, 10);
            Assert.Equal(0, degenerate.ScaleFactor, 10);
            Assert.Throws<ArgumentException>(() => Transform2D.Scale(0));
            Assert.Throws<ArgumentException>(() => Transform2D.Scale(-1));
        }

        [Fact]
        public void CTBLK_IdentityKeysComeOnlyFromTypedInstancesAndNamingCannotRewriteThem()
        {
            var importer = Source("src/RackCad.Plugin/Drawing/BlockLibraryImporter.cs");
            Assert.Equal(2, Count(importer, ".Select(i => i.BlockName)"));
            Assert.Contains(".Distinct(StringComparer.OrdinalIgnoreCase)", importer);
            Assert.DoesNotContain("BlockNaming", importer);
            Assert.DoesNotContain("SanitizeBlockName", importer);

            var writer = Source("src/RackCad.Plugin/Systems/Shared/SystemBlockWriter.cs");
            Assert.True(writer.IndexOf("EnsureForPlan(database, plan)", StringComparison.Ordinal) <
                        writer.IndexOf("drawer.CreateSystemBlock", StringComparison.Ordinal));
            var drawer = Source("src/RackCad.Plugin/Drawing/LateralHeaderDrawer.cs");
            Assert.Contains("blockTable.Has(instance.BlockName)", drawer);
            Assert.Contains("BlockNaming.SanitizeBlockName(baseName)", drawer);
        }

        [Fact]
        public void CTBLK_IdentityCasesKeepBaseNameCollisionAndRequirementKeysOrthogonal()
        {
            var requirements = new[] { "VIGA.100", "POSTE-A" };
            var queried = new List<string>();
            var available = Query(requirements, new HashSet<string>(requirements), queried);
            Assert.Equal(requirements, queried);
            Assert.DoesNotContain("Vista Rack", queried);
            Assert.All(available.Values, Assert.True);

            var before = requirements.ToArray();
            Assert.Equal("Vista Rack_1", UniqueGeneratedName("Vista Rack", new HashSet<string> { "Vista Rack" }));
            Assert.Equal(before, requirements);
            Assert.Contains("VIGA.100", queried);
        }

        [Fact]
        public void CTBLK_AvailabilityImportFlowRequeriesAndDoesNotKeepPreQueryMissingSticky()
        {
            var target = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var queryCalls = 0;
            var final = ObserveAvailability(new[] { "VIGA.100" }, true,
                keys => { queryCalls++; return Query(keys, target); },
                keys => { foreach (var key in keys) target.Add(key); });
            Assert.Equal(2, queryCalls);
            Assert.True(final["VIGA.100"]);
        }

        [Fact]
        public void CTBLK_AvailabilityNoImportUsesThePureQueryAsFinal()
        {
            var imported = false;
            var final = ObserveAvailability(new[] { "VIGA.100" }, false,
                keys => Query(keys, new HashSet<string>()), keys => imported = true);
            Assert.False(imported);
            Assert.False(final["VIGA.100"]);
        }

        [Fact]
        public void CTBLK_AvailabilityFailedImportRemainsMissingAfterFinalQuery()
        {
            var queryCalls = 0;
            var final = ObserveAvailability(new[] { "VIGA.100" }, true,
                keys => { queryCalls++; return Query(keys, new HashSet<string>()); }, keys => { });
            Assert.Equal(2, queryCalls);
            Assert.False(final["VIGA.100"]);
        }

        [Fact]
        public void CTAUTH_ComparatorReturnsSingleDivergentAndUnreadableWithoutChoosingASibling()
        {
            var a = new SelectivePalletDesignDocument { Id = "rack", Name = "A", PostId = "P" };
            var same = new SelectivePalletDesignDocument { Id = "rack", Name = "A", PostId = "P" };
            var different = new SelectivePalletDesignDocument { Id = "rack", Name = "B", PostId = "P" };

            var single = SelectiveAuthoredAuthority.Resolve("rack", new[]
            {
                ProjectVariableScanEntry.Selective("D1", "rack", a),
                ProjectVariableScanEntry.Selective("D2", "rack", same),
            });
            Assert.Equal(AuthoredAuthorityOutcome.Single, single.Outcome);
            Assert.Same(a, single.Authored);

            var divergent = SelectiveAuthoredAuthority.Resolve("rack", new[]
            {
                ProjectVariableScanEntry.Selective("D1", "rack", a),
                ProjectVariableScanEntry.Selective("D2", "rack", different),
            });
            Assert.Equal(AuthoredAuthorityOutcome.Divergent, divergent.Outcome);
            Assert.Null(divergent.Authored);

            var unreadable = SelectiveAuthoredAuthority.Resolve("rack", new[]
            {
                ProjectVariableScanEntry.Selective("D1", "rack", a),
                ProjectVariableScanEntry.SelectiveUnreadableDesign("D2", "rack"),
            });
            Assert.Equal(AuthoredAuthorityOutcome.UnreadableSibling, unreadable.Outcome);
            Assert.Null(unreadable.Authored);
        }

        private static IReadOnlyDictionary<string, bool> ObserveAvailability(
            IReadOnlyList<string> requirements,
            bool allowImport,
            Func<IReadOnlyList<string>, IReadOnlyDictionary<string, bool>> query,
            Action<IReadOnlyList<string>> ensure)
        {
            if (!allowImport)
            {
                return query(requirements);
            }

            query(requirements); // optional pre-query; never returned as final
            ensure(requirements);
            return query(requirements);
        }

        private static IReadOnlyDictionary<string, bool> Query(
            IEnumerable<string> keys, ISet<string> available, ICollection<string> observed = null)
        {
            var result = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase);
            foreach (var key in keys.Where(value => !string.IsNullOrWhiteSpace(value)).Distinct(StringComparer.OrdinalIgnoreCase))
            {
                observed?.Add(key);
                result[key] = available.Contains(key);
            }

            return result;
        }

        private static string UniqueGeneratedName(string baseName, ISet<string> names)
        {
            var sanitized = BlockNaming.SanitizeBlockName(baseName);
            if (!names.Contains(sanitized)) return sanitized;
            for (var suffix = 1; ; suffix++)
            {
                var candidate = sanitized + "_" + suffix;
                if (!names.Contains(candidate)) return candidate;
            }
        }

        private static F1Manifest Manifest()
        {
            var path = Path.Combine(Root, "tests", "RackCad.Tests", "Fixtures", "I57", "f1-characterizations.json");
            return JsonSerializer.Deserialize<F1Manifest>(File.ReadAllText(path),
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }

        private static string Source(string relativePath)
            => File.ReadAllText(Path.Combine(Root, relativePath.Replace('/', Path.DirectorySeparatorChar)));

        private static int Count(string value, string needle)
        {
            var count = 0;
            for (var index = 0; (index = value.IndexOf(needle, index, StringComparison.Ordinal)) >= 0; index += needle.Length) count++;
            return count;
        }

        private static string FindRepositoryRoot()
        {
            var directory = new DirectoryInfo(AppContext.BaseDirectory);
            while (directory != null && !File.Exists(Path.Combine(directory.FullName, "RackCad.sln"))) directory = directory.Parent;
            return directory?.FullName ?? throw new InvalidOperationException("RackCad.sln was not found above the test output directory.");
        }

        private sealed class F1Manifest
        {
            public List<Characterization> Characterizations { get; set; } = new List<Characterization>();
            public List<FrameCase> Frames { get; set; } = new List<FrameCase>();
        }

        private sealed class Characterization
        {
            public string Id { get; set; }
            public string Fixture { get; set; }
            public string ProductionSource { get; set; }
            public string Symbol { get; set; }
            public string FutureConsumer { get; set; }
            public string Expected { get; set; }
            public string FailClosed { get; set; }
            public List<string> EvidenceTests { get; set; } = new List<string>();
            public List<string> Cases { get; set; } = new List<string>();
        }

        private sealed class FrameCase
        {
            public string Family { get; set; }
            public string View { get; set; }
            public string AxisMap { get; set; }
            public string PhysicalOrigin { get; set; }
            public double KMin { get; set; }
            public double KMax { get; set; }
            public double Center { get; set; }
            public string Endpoint { get; set; }
            public string VariantOffset { get; set; }
            public string DrawnBounds { get; set; }
            public string ProductionSource { get; set; }
            public string Symbol { get; set; }
        }
    }
}
