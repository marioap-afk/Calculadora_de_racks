using System.Text.Json;
using System.Text.Json.Nodes;
using RackCad.Application.Catalogs;
using RackCad.Application.RackFrames;
using RackCad.Application.Systems;
using RackCad.Domain.RackFrames;
using RackCad.Domain.Systems;
using RackCad.Application.Persistence;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>
    /// I-11 (changes-required, 4ª ronda, item 1): a redraw must NEVER overwrite an incompatible or foreign inner design
    /// with a fresh one. <see cref="RackProjectStore.ResolveInnerSource"/> returns a DISCRIMINATED result and
    /// <see cref="RackProjectStore.PreflightInnerSources"/> resolves ALL linked views up front, aborting with no resolved
    /// sources on the first blocking outcome — so the Plugin edit touches no block (no partial update). These pure tests
    /// lock that rule; the Plugin/WPF preflight wiring is exercised by the AutoCAD matrix (S7).
    /// </summary>
    public class PersistenceInnerPreflightTests
    {
        private static RackCatalog Catalog => JsonRackCatalogProvider.FromBaseDirectory().Load();

        private static RackProject DynamicProject()
            => RackProject.ForDynamic(new DynamicRackSystemBuilder(Catalog).BuildDefault(
                new PalletSpecification(42.0, 48.0, 60.0, 1000.0, "kg"),
                palletsDeep: 4,
                headerTemplate: RackFrameTemplateCatalog.Default,
                headerPostCatalogId: "POSTE_OMEGA_3X3",
                headerHeight: 132.0));

        private static RackProject CabeceraProject()
            => RackProject.ForSelective(new HardcodedStandardRackFrameService(Catalog).CreateDefault());

        private static string DynamicWrapperJson(string schemaVersion = null, string unknownKey = null)
        {
            var node = JsonNode.Parse(new RackProjectStore().Serialize(DynamicProject())).AsObject();
            if (schemaVersion != null) node["SchemaVersion"] = schemaVersion;
            if (unknownKey != null) node[unknownKey] = "x";
            return node.ToJsonString();
        }

        private static string CamaWrapperJson()
            => new RackProjectStore().Serialize(RackProject.ForCama(
                new FlowBedConfiguration { LaneDepth = 96.0, RollerId = "R" }));

        // --- ResolveInnerSource: the discriminated outcomes ---

        [Fact]
        public void Resolve_ValidSameMajor_ReturnsSuccessWithOwnProject()
        {
            var resolution = new RackProjectStore().ResolveInnerSource(
                DynamicWrapperJson("2.5", "futureInner"), RackSystemKind.PalletFlow, CabeceraProject());

            Assert.Equal(InnerSourceOutcome.Success, resolution.Outcome);
            Assert.NotNull(resolution.Project);
            Assert.Equal(RackSystemKind.PalletFlow, resolution.Project.Kind);
        }

        [Fact]
        public void Resolve_FutureMajor_ReturnsIncompatibleMajor_WithNoProject()
        {
            var resolution = new RackProjectStore().ResolveInnerSource(
                DynamicWrapperJson("3.0"), RackSystemKind.PalletFlow, DynamicProject());

            Assert.Equal(InnerSourceOutcome.IncompatibleMajor, resolution.Outcome);
            Assert.Null(resolution.Project); // an incompatible inner design can NEVER become a fresh payload
            Assert.True(resolution.IsBlocking);
        }

        [Fact]
        public void Resolve_WrongKind_ReturnsWrongKind_WithNoProject()
        {
            // A cama wrapper where a dynamic (PalletFlow) inner design was expected: corruption/foreign — blocking.
            var resolution = new RackProjectStore().ResolveInnerSource(
                CamaWrapperJson(), RackSystemKind.PalletFlow, DynamicProject());

            Assert.Equal(InnerSourceOutcome.WrongKind, resolution.Outcome);
            Assert.Null(resolution.Project);
            Assert.True(resolution.IsBlocking);
        }

        [Fact]
        public void Resolve_BenignFailure_FallsBackToInitiating()
        {
            var initiating = DynamicProject();
            var resolution = new RackProjectStore().ResolveInnerSource("{ not json", RackSystemKind.PalletFlow, initiating);

            Assert.Equal(InnerSourceOutcome.BenignFallback, resolution.Outcome);
            Assert.Same(initiating, resolution.Project);
            Assert.False(resolution.IsBlocking);
        }

        // --- PreflightInnerSources: abort BEFORE resolving/serializing any view ---

        [Fact]
        public void Preflight_OneIncompatibleAmongCompatible_AbortsWithNoResolvedSources()
        {
            var designs = new[] { DynamicWrapperJson("2.2", "u"), DynamicWrapperJson("3.0") };

            var result = new RackProjectStore().PreflightInnerSources(designs, RackSystemKind.PalletFlow, DynamicProject());

            Assert.True(result.Aborted);
            Assert.Equal(InnerSourceOutcome.IncompatibleMajor, result.BlockingOutcome);
            Assert.Empty(result.ResolvedSources); // nothing resolved => the caller serializes/redraws NOTHING
        }

        [Fact]
        public void Preflight_OneWrongKindAmongCompatible_Aborts()
        {
            var designs = new[] { DynamicWrapperJson("2.0"), CamaWrapperJson() };

            var result = new RackProjectStore().PreflightInnerSources(designs, RackSystemKind.PalletFlow, DynamicProject());

            Assert.True(result.Aborted);
            Assert.Equal(InnerSourceOutcome.WrongKind, result.BlockingOutcome);
            Assert.Empty(result.ResolvedSources);
        }

        [Fact]
        public void Preflight_AllCompatible_ResolvesEachToItsOwnProject()
        {
            var designs = new[] { DynamicWrapperJson("2.1", "a"), DynamicWrapperJson("2.3", "b") };

            var result = new RackProjectStore().PreflightInnerSources(designs, RackSystemKind.PalletFlow, DynamicProject());

            Assert.False(result.Aborted);
            Assert.Equal(2, result.ResolvedSources.Count);
            Assert.All(result.ResolvedSources, p => Assert.Equal(RackSystemKind.PalletFlow, p.Kind));
        }

        [Fact]
        public void Preflight_BenignView_ResolvesToInitiating()
        {
            var initiating = DynamicProject();
            var result = new RackProjectStore().PreflightInnerSources(new[] { "{}" }, RackSystemKind.PalletFlow, initiating);

            Assert.False(result.Aborted);
            Assert.Same(initiating, Assert.Single(result.ResolvedSources));
        }

        [Fact]
        public void FreshInsert_NoSource_WritesCurrentVersions()
        {
            // An explicit fresh insert (innerSource = null) is still allowed and stamps the current versions.
            var store = new RackProjectStore();
            using var doc = JsonDocument.Parse(store.Serialize(
                RackProject.ForCama(new FlowBedConfiguration { LaneDepth = 96.0, RollerId = "R" }).WithSourceMetadataFrom(null)));
            Assert.Equal("2.0", doc.RootElement.GetProperty("SchemaVersion").GetString());
        }
    }
}
