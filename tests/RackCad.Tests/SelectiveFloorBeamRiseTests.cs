using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Catalogs;
using RackCad.Application.Persistence;
using RackCad.Application.Systems.Selective;
using RackCad.Domain.Systems.Selective;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>
    /// I-43 Gate 4 / ID14: the "elevacion de larguero a piso" becomes an authority of <c>(FondoIndex, FrontIndex)</c>
    /// with a fallback to the run-wide value. These tests pin the rule (<c>override ?? global</c>, where <c>0.0</c> is
    /// an override and null is inheritance), the geometry it produces, the multi-fondo write over the shared
    /// <c>TargetFondos</c>, the destructive resize, and the additive persistence. Pure: no WPF, no AutoCAD.
    /// </summary>
    public class SelectiveFloorBeamRiseTests
    {
        private const string PostId = TestCatalogIds.Profiles.Posts.Standard;
        private const string BeamId = TestCatalogIds.Profiles.Beams.SelectiveThreeRivet;

        private static RackCatalog Catalog => JsonRackCatalogProvider.FromBaseDirectory().Load();

        private static SelectiveBayDesign Bay(bool floor = true, double? rise = null, int levels = 2)
        {
            var bay = new SelectiveBayDesign { FloorBeam = floor, FloorBeamRiseOverride = rise };
            for (var l = 0; l < levels; l++)
            {
                bay.Levels.Add(new SelectiveCell
                {
                    Pallet = new Tarima { Frente = 42.0, Alto = 48.0 },
                    PalletCount = 2,
                    BeamId = BeamId,
                    BeamPeralte = 4.0
                });
            }

            return bay;
        }

        /// <summary>A design whose fondos hold the given bays; <paramref name="globalRise"/> is the run-wide default.</summary>
        private static SelectivePalletDesign Design(double globalRise, params SelectiveBayDesign[][] fondos)
        {
            var design = new SelectivePalletDesign
            {
                PostId = PostId,
                PostPeralte = 3.0,
                PalletTolerance = 4.0,
                VerticalClearance = 6.0,
                FloorBeamRise = globalRise,
                PalletDepth = 48.0,
                DepthCount = fondos.Length
            };

            foreach (var bay in fondos[0]) design.Bays.Add(bay);
            for (var k = 1; k < fondos.Length; k++)
            {
                design.ExtraFondoBays.Add(fondos[k].ToList());
                design.ExtraFondoDepths.Add(0.0);
            }

            return design;
        }

        private static SelectiveRackSystem Resolve(SelectivePalletDesign design)
            => new SelectiveGeometryResolver().Resolve(design, Catalog);

        /// <summary>The Y of the FLOOR larguero of a frente — the value the rise moves.</summary>
        private static double FloorY(SelectiveRackSystem system, int fondo, int front)
            => SelectiveDepthLayout.BaysOfFondo(system, fondo)[front].Levels.Min(level => level.Y);

        // ---- The rule: override ?? global, and 0.0 is an override ----

        [Fact]
        public void ADesignWithoutOverrides_ProducesExactlyTheGlobalGeometry()
        {
            var legacy = Design(4.0, new[] { Bay(), Bay() });
            var explicitly = Design(4.0, new[] { Bay(rise: 4.0), Bay(rise: 4.0) });

            var a = Resolve(legacy);
            var b = Resolve(explicitly);

            Assert.Equal(FloorY(b, 0, 0), FloorY(a, 0, 0), 6);
            Assert.Equal(FloorY(b, 0, 1), FloorY(a, 0, 1), 6);
        }

        [Fact]
        public void TwoFrentesOfTheSameFondo_CanHaveDifferentFloorLargueroYs()
        {
            var system = Resolve(Design(4.0, new[] { Bay(rise: 4.0), Bay(rise: 12.0) }));

            Assert.NotEqual(FloorY(system, 0, 0), FloorY(system, 0, 1));
            Assert.True(FloorY(system, 0, 1) > FloorY(system, 0, 0));
        }

        [Fact]
        public void TheSameFrontIndexInDifferentFondos_CanHaveDifferentRises()
        {
            var system = Resolve(Design(4.0,
                new[] { Bay(rise: 4.0), Bay() },
                new[] { Bay(rise: 16.0), Bay() }));

            Assert.NotEqual(FloorY(system, 0, 0), FloorY(system, 1, 0));
            Assert.Equal(FloorY(system, 0, 1), FloorY(system, 1, 1), 6); // both inherit the global
        }

        [Fact]
        public void AnExplicitZero_IsNotTheSameAsInheriting()
        {
            var inherits = Resolve(Design(8.0, new[] { Bay() }));
            var zero = Resolve(Design(8.0, new[] { Bay(rise: 0.0) }));

            Assert.NotEqual(FloorY(inherits, 0, 0), FloorY(zero, 0, 0));
            Assert.True(FloorY(zero, 0, 0) < FloorY(inherits, 0, 0));
        }

        [Fact]
        public void ChangingTheGlobal_MovesTheInheritingFrentes_AndNeverTheOverriddenOnes()
        {
            var before = Resolve(Design(4.0, new[] { Bay(), Bay(rise: 12.0) }));
            var after = Resolve(Design(20.0, new[] { Bay(), Bay(rise: 12.0) }));

            Assert.NotEqual(FloorY(before, 0, 0), FloorY(after, 0, 0)); // inherits: it moved
            Assert.Equal(FloorY(before, 0, 1), FloorY(after, 0, 1), 6); // explicit: untouched
        }

        [Fact]
        public void TheRiseSnapsUpToTheTroquelPitch_ExactlyAsBefore()
        {
            // 5" is not a multiple of the 2" pitch: it must round up to 6, landing on the same Y as an explicit 6.
            var five = Resolve(Design(4.0, new[] { Bay(rise: 5.0) }));
            var six = Resolve(Design(4.0, new[] { Bay(rise: 6.0) }));

            Assert.Equal(FloorY(six, 0, 0), FloorY(five, 0, 0), 6);
        }

        [Fact]
        public void WithoutFloorBeam_TheOverrideChangesNoGeometry_ButIsKept()
        {
            var off = Resolve(Design(4.0, new[] { Bay(floor: false, rise: 30.0) }));
            var plain = Resolve(Design(4.0, new[] { Bay(floor: false) }));

            Assert.Equal(FloorY(plain, 0, 0), FloorY(off, 0, 0), 6);

            // And the value survives the design round trip, so re-checking "Piso" brings it back.
            var design = Design(4.0, new[] { Bay(floor: false, rise: 30.0) });
            Assert.Equal(30.0, RoundTrip(design, out _).Bays[0].FloorBeamRiseOverride);
        }

        // ---- The editor state: multi-fondo write over TargetFondos ----

        private static SelectiveEditorState StateWith(params int[] fondoFrentes)
        {
            var state = new SelectiveEditorState { DefaultBeamId = BeamId };
            foreach (var frentes in fondoFrentes)
            {
                state.InitMatrix(frentes, 2);
                for (var b = 0; b < frentes; b++) state.FloorBeams[b] = true;
                state.FondoMatrices.Add(state.SnapshotWorking(48.0, 0.0));
            }

            state.SelectedFondo = 0;
            state.LoadFondo(0);
            state.SetTargetFondos(new[] { 0 });
            return state;
        }

        private static double?[] RisesOf(SelectiveEditorState state, int fondo, int frentes)
            => Enumerable.Range(0, frentes).Select(front => state.FloorBeamRiseOverrideAt(fondo, front)).ToArray();

        [Fact]
        public void Front_WritesThatFrenteInEveryTargetFondo_AndOnlyIt()
        {
            var state = StateWith(3, 3, 3, 3);
            state.SetTargetFondos(new[] { 1, 3 });

            var result = state.ApplyFloorBeamRiseToTargets(SelectiveFrontApplyScope.Front, 1, 14.0);

            Assert.Equal(new[] { (1, 1), (3, 1) }, result.Applied);
            Assert.Equal(new double?[] { null, null, null }, RisesOf(state, 0, 3)); // untouched
            Assert.Equal(new double?[] { null, 14.0, null }, RisesOf(state, 1, 3));
            Assert.Equal(new double?[] { null, null, null }, RisesOf(state, 2, 3));
            Assert.Equal(new double?[] { null, 14.0, null }, RisesOf(state, 3, 3));
        }

        [Fact]
        public void All_WritesEveryFrenteOfEveryTargetFondo()
        {
            var state = StateWith(2, 2, 2);
            state.SetTargetFondos(new[] { 0, 2 });

            var result = state.ApplyFloorBeamRiseToTargets(SelectiveFrontApplyScope.All, 0, 9.0);

            Assert.Equal(new[] { (0, 0), (0, 1), (2, 0), (2, 1) }, result.Applied);
            Assert.Equal(new double?[] { 9.0, 9.0 }, RisesOf(state, 0, 2));
            Assert.Equal(new double?[] { null, null }, RisesOf(state, 1, 2));
            Assert.Equal(new double?[] { 9.0, 9.0 }, RisesOf(state, 2, 2));
        }

        [Fact]
        public void ATargetFondoWithoutThatFrente_IsOmittedWithoutBlockingTheOthers()
        {
            var state = StateWith(3, 1, 3); // fondo 1 has a single frente
            state.SetTargetFondos(new[] { 0, 1, 2 });

            var result = state.ApplyFloorBeamRiseToTargets(SelectiveFrontApplyScope.Front, 2, 11.0);

            Assert.Equal(new[] { (0, 2), (2, 2) }, result.Applied);
            Assert.Equal(new[] { 1 }, result.OmittedFondos);
            Assert.Contains("no tiene ese frente", result.Describe(restore: false));
            Assert.Equal(new double?[] { null }, RisesOf(state, 1, 1)); // nothing padded, nothing clamped
        }

        [Fact]
        public void TheWriteReachesTheLiveMatrixOfTheActiveFondo_AndTheStoredOnesOfTheRest()
        {
            var state = StateWith(2, 2, 2);
            state.SetTargetFondos(new[] { 0, 2 });

            state.ApplyFloorBeamRiseToTargets(SelectiveFrontApplyScope.Front, 0, 13.0);

            Assert.Equal(13.0, state.FloorBeamRiseOverrides[0]);                        // live working matrix (fondo 0)
            Assert.Equal(13.0, state.FondoMatrices[2].FloorBeamRiseOverrides[0]);       // stored matrix (fondo 2)
            Assert.Null(state.FondoMatrices[1].FloorBeamRiseOverrides[0]);

            state.SaveWorkingToSelected(48.0, 0.0);
            Assert.Equal(13.0, state.FondoMatrices[0].FloorBeamRiseOverrides[0]);       // and it survives the commit
        }

        [Fact]
        public void RestoringWritesNull_OnlyOnTheResolvedTargets_AndTheGlobalGovernsAgain()
        {
            var state = StateWith(2, 2);
            state.SetTargetFondos(new[] { 0, 1 });
            state.ApplyFloorBeamRiseToTargets(SelectiveFrontApplyScope.Front, 0, 18.0);

            state.SetTargetFondos(new[] { 1 });
            var result = state.ApplyFloorBeamRiseToTargets(SelectiveFrontApplyScope.Front, 0, null);

            Assert.Contains("Restablecida", result.Describe(restore: true));
            Assert.Equal(18.0, state.FloorBeamRiseOverrideAt(0, 0)); // still overridden
            Assert.Null(state.FloorBeamRiseOverrideAt(1, 0));        // back to inheriting

            // And the restored frente now follows the global, while the other keeps its own value.
            var design = state.BuildDesign(Inputs(state, globalRise: 22.0));
            var system = Resolve(design);
            Assert.NotEqual(FloorY(system, 0, 0), FloorY(system, 1, 0));
        }

        [Fact]
        public void AnExplicitZeroSurvivesTheEditorState_AndIsNotConfusedWithARestore()
        {
            var state = StateWith(2, 2);
            state.SetTargetFondos(new[] { 0, 1 });

            state.ApplyFloorBeamRiseToTargets(SelectiveFrontApplyScope.Front, 0, 0.0);

            Assert.Equal(0.0, state.FloorBeamRiseOverrideAt(0, 0));
            Assert.Equal(0.0, state.FloorBeamRiseOverrideAt(1, 0));
            Assert.True(state.FloorBeamRiseOverrideAt(0, 0).HasValue); // a value, not an inheritance
        }

        // ---- Resize: destructive, no resurrection ----

        [Fact]
        public void ShrinkingDropsTheOverridesOfTheFrentesRemoved_AndRegrowingDoesNotReviveThem()
        {
            var state = StateWith(3);
            state.ApplyFloorBeamRiseToTargets(SelectiveFrontApplyScope.All, 0, 17.0);
            Assert.Equal(new double?[] { 17.0, 17.0, 17.0 }, RisesOf(state, 0, 3));

            state.ResizeBays(1);
            Assert.Single(state.FloorBeamRiseOverrides);

            state.ResizeBays(3);
            // A new frente clones the CURRENT last one, exactly like FloorBeams/BayHeights do — it does not restore
            // what the shrink deleted; here both happen to be 17 because the survivor carried it.
            Assert.Equal(new double?[] { 17.0, 17.0, 17.0 }, RisesOf(state, 0, 3));

            // With a survivor that inherits, the regrown frentes inherit too: nothing is resurrected.
            var other = StateWith(3);
            other.ApplyFloorBeamRiseToTargets(SelectiveFrontApplyScope.Front, 2, 19.0);
            other.ResizeBays(1);
            other.ResizeBays(3);
            Assert.Equal(new double?[] { null, null, null }, RisesOf(other, 0, 3));
        }

        [Fact]
        public void TheOverridesTravelThroughSnapshotRestoreAndClone()
        {
            var state = StateWith(2, 2);
            state.ApplyFloorBeamRiseToTargets(SelectiveFrontApplyScope.Front, 1, 21.0);

            var snapshot = state.SnapshotWorking(48.0, 0.0);
            Assert.Equal(new double?[] { null, 21.0 }, snapshot.FloorBeamRiseOverrides.ToArray());

            state.RestoreWorkingFrom(snapshot);
            Assert.Equal(new double?[] { null, 21.0 }, state.FloorBeamRiseOverrides.ToArray());

            var aligned = state.CloneAligned(snapshot, 3, snapshot);
            Assert.Equal(new double?[] { null, 21.0, null }, aligned.FloorBeamRiseOverrides.ToArray());
        }

        [Fact]
        public void ASnapshotTakenBeforeThisFieldExisted_LoadsAsInheriting()
        {
            var state = StateWith(2);
            var legacy = state.SnapshotWorking(48.0, 0.0);
            legacy.FloorBeamRiseOverrides.Clear(); // a stored matrix from before ID14

            state.RestoreWorkingFrom(legacy);

            Assert.Equal(new double?[] { null, null }, state.FloorBeamRiseOverrides.ToArray());
        }

        // ---- Persistence ----

        [Fact]
        public void RoundTrip_PreservesNullZeroAndPositiveValues()
        {
            var design = Design(4.0, new[] { Bay(), Bay(rise: 0.0), Bay(rise: 15.5) });

            var restored = RoundTrip(design, out var id);

            Assert.Equal("GUID-RISE", id);
            Assert.Null(restored.Bays[0].FloorBeamRiseOverride);
            Assert.Equal(0.0, restored.Bays[1].FloorBeamRiseOverride);
            Assert.Equal(15.5, restored.Bays[2].FloorBeamRiseOverride);
            Assert.Equal(4.0, restored.FloorBeamRise); // the global default is untouched
        }

        [Fact]
        public void ALegacyDocumentWithoutTheField_InheritsTheGlobalEverywhere()
        {
            var design = Design(7.0, new[] { Bay(), Bay() });
            var document = SelectivePalletDesignDocument.From(design, "GUID-LEG-RISE", "Legacy");
            foreach (var bay in document.Bays) bay.FloorBeamRiseOverride = null; // the pre-ID14 shape
            var store = new SelectivePalletDesignStore();

            var restored = store.Deserialize(store.Serialize(document)).ToDomain();
            var system = Resolve(restored);

            Assert.All(restored.Bays, bay => Assert.Null(bay.FloorBeamRiseOverride));
            Assert.Equal(FloorY(Resolve(Design(7.0, new[] { Bay(), Bay() })), 0, 0), FloorY(system, 0, 0), 6);
        }

        [Fact]
        public void SaveAndLoad_KeepsExactlyTheResolvedFloorYs()
        {
            var design = Design(4.0,
                new[] { Bay(rise: 0.0), Bay(rise: 12.0), Bay() },
                new[] { Bay(rise: 26.0), Bay(), Bay(rise: 6.0) });

            var before = Resolve(design);
            var after = Resolve(RoundTrip(design, out _));

            for (var fondo = 0; fondo < 2; fondo++)
            {
                for (var front = 0; front < 3; front++)
                {
                    Assert.Equal(FloorY(before, fondo, front), FloorY(after, fondo, front), 6);
                }
            }
        }

        [Fact]
        public void TheRiseDoesNotDisturbTheCabecerasOfGate4()
        {
            var design = Design(4.0, new[] { Bay(rise: 24.0), Bay() }, new[] { Bay(), Bay() });
            var factory = new RackCad.Application.RackFrames.RackFrameConfigurationFactory(Catalog);
            var custom = factory.Build(RackCad.Application.RackFrames.RackFrameTemplateCatalog.FindStandardOrDefault(), PostId, 300.0, 42.0);
            design.ExtraFondoPostCabeceras.Add(new List<RackCad.Domain.RackFrames.RackFrameConfiguration> { null, custom });

            var system = Resolve(design);

            Assert.Equal(300.0, SelectiveCabeceraAuthority.EffectiveCustomAt(system, 1, 1).Height, 4);
            Assert.Equal(42.0, SelectiveCabeceraAuthority.EffectiveCustomAt(system, 1, 1).Depth, 4);
            Assert.NotEqual(FloorY(system, 0, 0), FloorY(system, 0, 1)); // and the rise still applies
        }

        private static SelectiveDesignInputs Inputs(SelectiveEditorState state, double globalRise)
            => new SelectiveDesignInputs
            {
                PostId = PostId,
                PostPeralte = 3.0,
                PalletTolerance = 4.0,
                VerticalClearance = 6.0,
                FloorBeamRise = globalRise,
                Fondo = 48.0,
                DepthCount = state.FondoMatrices.Count,
                WorkingDepth = 48.0,
                WorkingCabeceraOverride = 0.0,
                Separators = new List<double>()
            };

        private static SelectivePalletDesign RoundTrip(SelectivePalletDesign design, out string id)
        {
            var store = new SelectivePalletDesignStore();
            var document = store.Deserialize(store.Serialize(SelectivePalletDesignDocument.From(design, "GUID-RISE", "Rise")));
            id = document.Id;
            return document.ToDomain();
        }
    }
}
