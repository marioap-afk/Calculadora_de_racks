using System.Linq;
using RackCad.Application.Catalogs;
using RackCad.Application.Persistence;
using RackCad.Application.RackFrames;
using RackCad.Domain.RackFrames;
using RackCad.Domain.Systems;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>
    /// Initiative I-17 (audit U4): every editor now deep-clones a <see cref="RackFrameConfiguration"/> through the SINGLE
    /// canonical <see cref="RackFrameProjectStore.DeepCopy"/> (the serialization store) instead of three separate copies
    /// (one hand-written, two through different stores). These tests fix the equivalence the unification depends on: the
    /// clone preserves the whole source of truth, is fully independent from the original, rebuilds the derived model, and
    /// matches BOTH previous serialization clones (the dynamic window's bare-store round-trip and the selective window's
    /// wrapper round-trip). The document's own fidelity (celosía/grid params, reinforcement, schema guards) already lives
    /// in <see cref="RackFrameProjectStoreTests"/>; this file is about the clone that consumes it.
    /// </summary>
    public class RackFrameConfigurationDeepCopyTests
    {
        /// <summary>A rich, deliberately non-default configuration: every persisted field carries a value that differs
        /// from the domain default, so a dropped field would change the serialized wire form and fail the equivalence.</summary>
        private static RackFrameConfiguration RichConfig()
        {
            var configuration = new RackFrameConfigurationFactory(JsonRackCatalogProvider.FromBaseDirectory().Load())
                .Build(RackFrameTemplateCatalog.Default, "POSTE_OMEGA_3X3", 132.0, 42.0);

            configuration.Name = "Cabecera I-17";
            configuration.PostPeralte = 3.25;
            configuration.CelosiaStartTroquel = 5;
            configuration.DiagonalStartOffsetTroqueles = 1;
            configuration.DiagonalEndOffsetTroqueles = 3;
            configuration.DiagonalDoubleSpacingTroqueles = 4;
            configuration.HorizontalDoubleOffsetTroqueles = 2;
            configuration.PasoTroquel = 3.0;
            configuration.PanelClear = 50.0;
            configuration.StandardBaselineId = "BASE-42";
            configuration.StandardBaselineVersion = "v9";

            configuration.LeftPost.HasReinforcement = true;
            configuration.LeftPost.ReinforcementCatalogId = "POSTE_OMEGA_3X3";
            configuration.LeftPost.ReinforcementHeight = 60.0;
            configuration.LeftBasePlate.PeralteOverride = 7.5;

            configuration.BracingPanels[0].Arrangement = BracingPattern.DoubleDiagonal;
            configuration.BracingPanels[1].Arrangement = BracingPattern.XBracing;
            configuration.BracingPanels[1].IsException = true;
            configuration.Horizontals[2].State = FrameComponentState.Manual;
            configuration.Horizontals[2].Notes = "ajuste manual";

            // Rebuild the derived model so the source has up-to-date Members/panel elevations, exactly as the editors
            // hold it when they clone.
            new BracingPanelMemberBuilder().RefreshPhysicalModel(configuration);
            return configuration;
        }

        private static string Wire(RackFrameConfiguration configuration)
            => new RackFrameProjectStore().Serialize(configuration);

        [Fact]
        public void DeepCopy_PreservesTheWholeSourceOfTruth()
        {
            var original = RichConfig();

            var clone = new RackFrameProjectStore().DeepCopy(original);

            // The store document IS the source of truth, so identical wire form == every persisted field preserved.
            Assert.Equal(Wire(original), Wire(clone));
        }

        [Fact]
        public void DeepCopy_PreservesRichNonDefaultStateFieldByField()
        {
            var original = RichConfig();

            var clone = new RackFrameProjectStore().DeepCopy(original);

            Assert.Equal("Cabecera I-17", clone.Name);
            Assert.Equal(132.0, clone.Height, 4);
            Assert.Equal(42.0, clone.Depth, 4);
            Assert.Equal(3.25, clone.PostPeralte, 4);
            Assert.Equal(5, clone.CelosiaStartTroquel);
            Assert.Equal(1, clone.DiagonalStartOffsetTroqueles);
            Assert.Equal(3, clone.DiagonalEndOffsetTroqueles);
            Assert.Equal(4, clone.DiagonalDoubleSpacingTroqueles);
            Assert.Equal(2, clone.HorizontalDoubleOffsetTroqueles);
            Assert.Equal(3.0, clone.PasoTroquel, 4);
            Assert.Equal(50.0, clone.PanelClear, 4);
            Assert.Equal("BASE-42", clone.StandardBaselineId);
            Assert.Equal("v9", clone.StandardBaselineVersion);
            Assert.True(clone.LeftPost.HasReinforcement);
            Assert.Equal("POSTE_OMEGA_3X3", clone.LeftPost.ReinforcementCatalogId);
            Assert.Equal(60.0, clone.LeftPost.ReinforcementHeight, 4);
            Assert.Equal(7.5, clone.LeftBasePlate.PeralteOverride);
            Assert.Equal(BracingPattern.DoubleDiagonal, clone.BracingPanels[0].Arrangement);
            Assert.Equal(BracingPattern.XBracing, clone.BracingPanels[1].Arrangement);
            Assert.True(clone.BracingPanels[1].IsException);
            Assert.Equal(FrameComponentState.Manual, clone.Horizontals[2].State);
            Assert.Equal("ajuste manual", clone.Horizontals[2].Notes);
        }

        [Fact]
        public void DeepCopy_ReturnsAFullyIndependentInstance()
        {
            var original = RichConfig();
            var originalWire = Wire(original);

            var clone = new RackFrameProjectStore().DeepCopy(original);
            Assert.NotSame(original, clone);
            Assert.NotSame(original.LeftPost, clone.LeftPost);
            Assert.NotSame(original.Horizontals[0], clone.Horizontals[0]);

            // Mutating the clone must not reach back into the original (no shared references anywhere in the graph).
            clone.Height = 999.0;
            clone.LeftPost.PostCatalogId = "MUTATED";
            clone.Horizontals.Clear();
            clone.BracingPanels[0].Arrangement = BracingPattern.NoBracing;

            Assert.Equal(originalWire, Wire(original));
        }

        [Fact]
        public void DeepCopy_RebuildsTheDerivedModelIndependently()
        {
            var original = RichConfig();

            var clone = new RackFrameProjectStore().DeepCopy(original);

            Assert.NotEmpty(clone.Members);
            Assert.All(clone.BracingPanels, panel => Assert.True(panel.EndElevation >= panel.StartElevation));
            // Derived collections are the clone's own, not aliases of the original's.
            Assert.NotSame(original.Members, clone.Members);
        }

        [Fact]
        public void DeepCopy_Null_ReturnsNull()
        {
            // Drop-in for the historical UI clone helpers, whose null-tolerance the editors relied on.
            Assert.Null(new RackFrameProjectStore().DeepCopy(null));
        }

        [Fact]
        public void DeepCopy_EqualsTheDynamicWindowsHistoricalStoreRoundTrip()
        {
            // The dynamic window used to clone with `store.Deserialize(store.Serialize(config))` inline; DeepCopy IS that.
            var original = RichConfig();
            var store = new RackFrameProjectStore();

            var viaDeepCopy = store.DeepCopy(original);
            var viaInlineRoundTrip = store.Deserialize(store.Serialize(original));

            Assert.Equal(Wire(viaInlineRoundTrip), Wire(viaDeepCopy));
        }

        [Fact]
        public void DeepCopy_IsIdempotentOnTheSourceOfTruth()
        {
            var original = RichConfig();
            var store = new RackFrameProjectStore();

            var once = store.DeepCopy(original);
            var twice = store.DeepCopy(once);

            Assert.Equal(Wire(once), Wire(twice));
        }

        [Fact]
        public void DeepCopy_MatchesTheSelectiveWindowsPreviousWrapperRoundTrip()
        {
            // Regression guard for the selective window's switch (RackSelectiveWindow.CloneCabecera): its old clone went
            // through the project WRAPPER — new RackProjectStore().Deserialize(Serialize(RackProject.ForSelective(h))).Header.
            // Both paths round-trip the same RackFrameProjectDocument, so the cloned cabecera must be identical.
            var original = RichConfig();

            var viaDeepCopy = new RackFrameProjectStore().DeepCopy(original);

            var wrapperStore = new RackProjectStore();
            var viaWrapper = wrapperStore
                .Deserialize(wrapperStore.Serialize(RackProject.ForSelective(original)))
                ?.Header;

            Assert.NotNull(viaWrapper);
            Assert.Equal(Wire(viaDeepCopy), Wire(viaWrapper));
        }
    }
}
