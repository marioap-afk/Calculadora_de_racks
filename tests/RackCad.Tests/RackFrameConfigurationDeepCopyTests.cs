using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
    /// canonical <see cref="RackFrameProjectStore.DeepCopy"/> instead of three separate copies (one hand-written, two
    /// through different stores). These tests fix the equivalence the unification depends on and, crucially, cover the
    /// state the persistence document does NOT carry: the derived model (rebuilt on load) and the runtime-only
    /// <see cref="RackFrameConfiguration.Exceptions"/> (re-attached by DeepCopy). Wire equality alone is deliberately
    /// insufficient here — the wire form omits Exceptions and the derived model — so the whole object graph is compared
    /// field by field, and a reflection guard forces any FUTURE property to be classified. The document's own fidelity
    /// (celosía/grid params, reinforcement, schema guards) lives in <see cref="RackFrameProjectStoreTests"/>.
    /// </summary>
    public class RackFrameConfigurationDeepCopyTests
    {
        /// <summary>A rich, deliberately non-default configuration: every persisted field carries a non-default value,
        /// the derived model is built, AND there are non-empty, non-default override Exceptions (runtime-only state the
        /// document does not persist).</summary>
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

            // Build the derived model (Members, per-panel members, panel elevations) so the source is exactly what an
            // editor holds when it clones.
            new BracingPanelMemberBuilder().RefreshPhysicalModel(configuration);

            // Runtime-only override audit trail: NOT persisted by RackFrameProjectDocument and NOT rebuilt on load.
            configuration.Exceptions.Add(new FrameExceptionOverride
            {
                ExceptionType = ExceptionType.PatternChange,
                TargetId = configuration.BracingPanels[1].PanelId,
                StandardValue = "SingleDiagonal",
                OverrideValue = "XBracing",
                Reason = "refuerzo por carga"
            });
            configuration.Exceptions.Add(new FrameExceptionOverride
            {
                ExceptionType = ExceptionType.ProfileChange,
                TargetId = configuration.Horizontals[2].Id,
                StandardValue = "PERFIL_A",
                OverrideValue = "PERFIL_B",
                Reason = "cambio de perfil manual"
            });
            return configuration;
        }

        private static string Wire(RackFrameConfiguration configuration)
            => new RackFrameProjectStore().Serialize(configuration);

        // ---- Persisted model (the store document's own projection) ----

        [Fact]
        public void DeepCopy_PreservesThePersistedModel_ByWireEquality()
        {
            // The store document IS the persisted source of truth, so identical wire form == every PERSISTED field
            // preserved. This is necessary but NOT sufficient (the wire omits Exceptions and the derived model), so the
            // tests below compare those explicitly rather than leaning on Serialize==Serialize.
            var original = RichConfig();

            var clone = new RackFrameProjectStore().DeepCopy(original);

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

        // ---- Runtime-only Exceptions (omitted by the document, re-attached by DeepCopy) ----

        [Fact]
        public void DeepCopy_PreservesEveryException_AsIndependentDeepCopies()
        {
            var original = RichConfig();
            Assert.NotEmpty(original.Exceptions); // guards a vacuous test

            var clone = new RackFrameProjectStore().DeepCopy(original);

            Assert.Equal(original.Exceptions.Count, clone.Exceptions.Count);
            Assert.NotSame(original.Exceptions, clone.Exceptions); // the list itself is not shared
            for (var i = 0; i < original.Exceptions.Count; i++)
            {
                var expected = original.Exceptions[i];
                var actual = clone.Exceptions[i];
                Assert.NotSame(expected, actual); // deep copy, not a shared reference
                Assert.Equal(expected.ExceptionType, actual.ExceptionType);
                Assert.Equal(expected.TargetId, actual.TargetId);
                Assert.Equal(expected.StandardValue, actual.StandardValue);
                Assert.Equal(expected.OverrideValue, actual.OverrideValue);
                Assert.Equal(expected.Reason, actual.Reason);
            }

            // Mutating the clone's overrides must not reach back into the original.
            clone.Exceptions[0].Reason = "MUTATED";
            var survivingReason = original.Exceptions[0].Reason;
            clone.Exceptions.Clear();
            Assert.Equal(2, original.Exceptions.Count);
            Assert.NotEqual("MUTATED", survivingReason);
        }

        [Fact]
        public void DeepCopy_WithNoExceptions_ClonesAnEmptyOverrideList()
        {
            var original = RichConfig();
            original.Exceptions.Clear();

            var clone = new RackFrameProjectStore().DeepCopy(original);

            Assert.Empty(clone.Exceptions);
        }

        // ---- Derived model (rebuilt on load) — compared member by member, including membership ----

        [Fact]
        public void DeepCopy_ReproducesTheDerivedModel_MemberForMember_IncludingPanelMembership()
        {
            var original = RichConfig();
            Assert.NotEmpty(original.Members); // guards a vacuous test

            var clone = new RackFrameProjectStore().DeepCopy(original);

            // Top-level members: same count, every field equal, no shared references.
            AssertMembersEqual(original.Members, clone.Members);
            Assert.NotSame(original.Members, clone.Members);

            // Per-panel membership: each panel's own member list is rebuilt identically (and independently), and the
            // rebuilt panel elevations match.
            Assert.Equal(original.BracingPanels.Count, clone.BracingPanels.Count);
            for (var i = 0; i < original.BracingPanels.Count; i++)
            {
                var op = original.BracingPanels[i];
                var cp = clone.BracingPanels[i];
                Assert.Equal(op.StartElevation, cp.StartElevation, 6);
                Assert.Equal(op.EndElevation, cp.EndElevation, 6);
                Assert.NotSame(op.Members, cp.Members);
                AssertMembersEqual(op.Members, cp.Members);
            }
        }

        private static void AssertMembersEqual(IList<FrameMember> expected, IList<FrameMember> actual)
        {
            Assert.Equal(expected.Count, actual.Count);
            for (var i = 0; i < expected.Count; i++)
            {
                var e = expected[i];
                var a = actual[i];
                Assert.NotSame(e, a);
                Assert.Equal(e.SourcePanelId, a.SourcePanelId);          // membership (panel id)
                Assert.Equal(e.SourcePanelIndex, a.SourcePanelIndex);    // membership (panel index)
                Assert.Equal(e.MemberType, a.MemberType);                // type
                Assert.Equal(e.CatalogId, a.CatalogId);                  // profile / catalog
                Assert.Equal(e.ProfileId, a.ProfileId);
                Assert.Equal(e.Quantity, a.Quantity);                    // quantity
                Assert.Equal(e.MountingFace, a.MountingFace);            // face
                Assert.Equal(e.Origin, a.Origin);                        // origin (standard/manual/exception)
                Assert.Equal(e.Length, a.Length, 6);                     // length
                Assert.Equal(e.Angle, a.Angle, 6);                       // angle
                Assert.Equal(e.IsStandard, a.IsStandard);
                AssertEndEqual(e.Start, a.Start);                        // ends: elevations + connection points
                AssertEndEqual(e.End, a.End);
            }
        }

        private static void AssertEndEqual(FrameMemberEnd expected, FrameMemberEnd actual)
        {
            if (expected == null)
            {
                Assert.Null(actual);
                return;
            }

            Assert.NotNull(actual);
            Assert.NotSame(expected, actual);
            Assert.Equal(expected.Role, actual.Role);
            Assert.Equal(expected.PostSide, actual.PostSide);
            Assert.Equal(expected.HorizontalPositionRatio, actual.HorizontalPositionRatio, 6);
            Assert.Equal(expected.Elevation, actual.Elevation, 6);
            Assert.Equal(expected.ConnectionPointId, actual.ConnectionPointId);
        }

        // ---- Whole-graph independence, null, idempotence, and equivalence with the two previous serialization clones ----

        [Fact]
        public void DeepCopy_ReturnsAFullyIndependentInstance()
        {
            var original = RichConfig();
            var originalWire = Wire(original);
            var originalExceptionCount = original.Exceptions.Count;

            var clone = new RackFrameProjectStore().DeepCopy(original);
            Assert.NotSame(original, clone);
            Assert.NotSame(original.LeftPost, clone.LeftPost);
            Assert.NotSame(original.Horizontals[0], clone.Horizontals[0]);

            // Mutating the clone must not reach the original anywhere in the graph.
            clone.Height = 999.0;
            clone.LeftPost.PostCatalogId = "MUTATED";
            clone.Horizontals.Clear();
            clone.BracingPanels[0].Arrangement = BracingPattern.NoBracing;
            clone.Exceptions.Add(new FrameExceptionOverride { Reason = "extra" });

            Assert.Equal(originalWire, Wire(original));
            Assert.Equal(originalExceptionCount, original.Exceptions.Count);
        }

        [Fact]
        public void DeepCopy_Null_ReturnsNull()
        {
            // Drop-in for the historical UI clone helpers, whose null-tolerance the editors relied on.
            Assert.Null(new RackFrameProjectStore().DeepCopy(null));
        }

        [Fact]
        public void DeepCopy_EqualsTheDynamicWindowsHistoricalStoreRoundTrip_ForThePersistedModel()
        {
            // The dynamic window used to clone with `store.Deserialize(store.Serialize(config))` inline; DeepCopy still
            // does exactly that for the persisted+derived model (and additionally carries the runtime Exceptions).
            var original = RichConfig();
            var store = new RackFrameProjectStore();

            var viaDeepCopy = store.DeepCopy(original);
            var viaInlineRoundTrip = store.Deserialize(store.Serialize(original));

            Assert.Equal(Wire(viaInlineRoundTrip), Wire(viaDeepCopy));
        }

        [Fact]
        public void DeepCopy_IsIdempotent_OnThePersistedModelAndOnExceptions()
        {
            var original = RichConfig();
            var store = new RackFrameProjectStore();

            var once = store.DeepCopy(original);
            var twice = store.DeepCopy(once);

            Assert.Equal(Wire(once), Wire(twice));
            Assert.Equal(once.Exceptions.Count, twice.Exceptions.Count); // exceptions survive repeated cloning
        }

        [Fact]
        public void DeepCopy_MatchesTheSelectiveWindowsPreviousWrapperRoundTrip_ForThePersistedModel()
        {
            // Regression guard for the selective window's switch (RackSelectiveWindow.CloneCabecera): its old clone went
            // through the project WRAPPER — new RackProjectStore().Deserialize(Serialize(RackProject.ForSelective(h))).Header.
            // Both paths round-trip the same RackFrameProjectDocument, so the cloned cabecera's persisted model is
            // identical. (DeepCopy additionally preserves the runtime Exceptions the wrapper path dropped — an
            // improvement, not a regression; those overrides do not drive geometry or BOM.)
            var original = RichConfig();

            var viaDeepCopy = new RackFrameProjectStore().DeepCopy(original);

            var wrapperStore = new RackProjectStore();
            var viaWrapper = wrapperStore
                .Deserialize(wrapperStore.Serialize(RackProject.ForSelective(original)))
                ?.Header;

            Assert.NotNull(viaWrapper);
            Assert.Equal(Wire(viaDeepCopy), Wire(viaWrapper));
        }

        // ---- Guard: every RackFrameConfiguration property must be classified ----

        [Fact]
        public void EveryRackFrameConfigurationProperty_IsClassifiedAsPersistedDerivedOrRuntimePreserved()
        {
            // A future RackFrameConfiguration property must be explicitly classified so it cannot silently vanish from
            // the canonical clone:
            //   - PERSISTED         => RackFrameProjectDocument carries it through Serialize/Deserialize;
            //   - DERIVED           => BracingPanelMemberBuilder.RefreshPhysicalModel rebuilds it on load;
            //   - RUNTIME-PRESERVED => the document omits it and it is NOT derived, so RackFrameProjectStore.DeepCopy
            //                          must re-attach it (as it does for Exceptions) AND a preservation test must cover it.
            var persisted = new HashSet<string>
            {
                nameof(RackFrameConfiguration.Name),
                nameof(RackFrameConfiguration.Units),
                nameof(RackFrameConfiguration.Height),
                nameof(RackFrameConfiguration.Depth),
                nameof(RackFrameConfiguration.PostPeralte),
                nameof(RackFrameConfiguration.CelosiaStartTroquel),
                nameof(RackFrameConfiguration.DiagonalStartOffsetTroqueles),
                nameof(RackFrameConfiguration.DiagonalEndOffsetTroqueles),
                nameof(RackFrameConfiguration.DiagonalDoubleSpacingTroqueles),
                nameof(RackFrameConfiguration.HorizontalDoubleOffsetTroqueles),
                nameof(RackFrameConfiguration.PasoTroquel),
                nameof(RackFrameConfiguration.PanelClear),
                nameof(RackFrameConfiguration.StandardBaselineId),
                nameof(RackFrameConfiguration.StandardBaselineVersion),
                nameof(RackFrameConfiguration.LeftPost),
                nameof(RackFrameConfiguration.RightPost),
                nameof(RackFrameConfiguration.LeftBasePlate),
                nameof(RackFrameConfiguration.RightBasePlate),
                nameof(RackFrameConfiguration.Horizontals),
                nameof(RackFrameConfiguration.BracingPanels),
            };
            var derived = new HashSet<string> { nameof(RackFrameConfiguration.Members) };
            var runtimePreserved = new HashSet<string> { nameof(RackFrameConfiguration.Exceptions) };

            var classified = new HashSet<string>(persisted);
            classified.UnionWith(derived);
            classified.UnionWith(runtimePreserved);

            var actual = typeof(RackFrameConfiguration)
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Select(property => property.Name)
                .ToList();

            var unclassified = actual.Where(name => !classified.Contains(name)).ToList();
            Assert.True(
                unclassified.Count == 0,
                "Unclassified RackFrameConfiguration properties (classify each as persisted/derived/runtime-preserved; " +
                "wire runtime-preserved ones into RackFrameProjectStore.DeepCopy and add a preservation test): " +
                string.Join(", ", unclassified));

            // Keep the classification honest: no stale names that no longer exist on the type.
            var actualSet = new HashSet<string>(actual);
            var stale = classified.Where(name => !actualSet.Contains(name)).ToList();
            Assert.True(
                stale.Count == 0,
                "Stale classification entries no longer on RackFrameConfiguration: " + string.Join(", ", stale));
        }
    }
}
