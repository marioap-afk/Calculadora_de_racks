using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Catalogs;
using RackCad.Application.Systems;
using RackCad.Domain.Systems;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>
    /// Pure unit tests for <see cref="SelectiveEditorState"/> — the selective editor's state and operations extracted
    /// from <c>RackSelectiveWindow</c> (initiative I-20). They exercise the model, the matrix operations, apply-by-scope,
    /// resizing, the per-fondo transitions and <see cref="SelectiveEditorState.BuildDesign"/>, with a resolved-geometry
    /// equivalence between a state-built design and a hand-built one (the same drawing). Runs without WPF or AutoCAD.
    /// </summary>
    public class SelectiveEditorStateTests
    {
        private const string PostId = TestCatalogIds.Profiles.Posts.Standard;
        private const string BeamId = TestCatalogIds.Profiles.Beams.SelectiveThreeRivet;

        private static RackCatalog Catalog => JsonRackCatalogProvider.FromBaseDirectory().Load();

        private static SelectiveEditorState NewState(string beam = BeamId) => new SelectiveEditorState { DefaultBeamId = beam };

        /// <summary>A state opened like the editor's constructor: InitMatrix(bays, levels) + one fondo snapshot at depth.</summary>
        private static SelectiveEditorState Opened(int bays, int levels, double depth = 48.0)
        {
            var state = NewState();
            state.InitMatrix(bays, levels);
            state.FondoMatrices.Add(state.SnapshotWorking(depth, 0.0));
            state.SelectedFondo = 0;
            return state;
        }

        private static SelectiveDesignInputs Inputs(int depthCount = 1, double workingDepth = 48.0, double workingCabecera = 0.0,
            IReadOnlyList<double> separators = null)
            => new SelectiveDesignInputs
            {
                PostId = PostId,
                PostPeralte = 3.0,
                PalletTolerance = 4.0,
                VerticalClearance = 6.0,
                FloorBeamRise = 4.0,
                Fondo = 48.0,
                DepthCount = depthCount,
                WorkingDepth = workingDepth,
                WorkingCabeceraOverride = workingCabecera,
                Separators = separators ?? new List<double>(),
                DrawBasePlate = true,
                AnnotationScale = 1.0,
                Dimensions = DimensionDetail.None
            };

        // ---- SelectiveEditorCell model ----

        [Fact]
        public void NewCell_UsesDefaultBeamAndPeralte()
        {
            var cell = NewState("BEAM-X").NewCell();
            Assert.Equal("BEAM-X", cell.BeamId);
            Assert.Equal(SelectiveRackDefaults.DefaultBeamPeralte, cell.BeamPeralte);
            Assert.Equal(42.0, cell.Frente);
            Assert.Equal(60.0, cell.Alto);
            Assert.Equal(2, cell.PalletCount);
            Assert.False(cell.HasOverride);
        }

        [Fact]
        public void Cell_CloneIsIndependent_CopyFromCopiesEveryField()
        {
            var a = new SelectiveEditorCell { Frente = 40, Alto = 50, PalletCount = 3, BeamId = "B", BeamPeralte = 5, BeamLength = 100, Clear = 7 };
            var clone = a.Clone();
            clone.Frente = 999;
            Assert.Equal(40, a.Frente); // clone is independent

            var b = new SelectiveEditorCell();
            b.CopyFrom(a);
            Assert.Equal(40, b.Frente);
            Assert.Equal(50, b.Alto);
            Assert.Equal(3, b.PalletCount);
            Assert.Equal("B", b.BeamId);
            Assert.Equal(5, b.BeamPeralte);
            Assert.Equal(100, b.BeamLength);
            Assert.Equal(7, b.Clear);
            Assert.True(b.HasOverride);
        }

        // ---- InitMatrix ----

        [Fact]
        public void InitMatrix_SizesParallelListsAndResetsSelection()
        {
            var state = NewState();
            state.SelBay = 5; state.SelLevel = 9;
            state.InitMatrix(3, 4);

            Assert.Equal(3, state.Bays.Count);
            Assert.All(state.Bays, column => Assert.Equal(4, column.Count));
            Assert.Equal(3, state.FloorBeams.Count);
            Assert.Equal(3, state.BayHeights.Count);
            Assert.Equal(3, state.BaySegments.Count);
            Assert.All(state.FloorBeams, f => Assert.False(f));
            Assert.All(state.BayHeights, h => Assert.Null(h));
            Assert.Equal(0, state.SelBay);
            Assert.Equal(0, state.SelLevel);
        }

        // ---- Selection / TryGetSelected / ClampSelection ----

        [Fact]
        public void TryGetSelected_ReturnsTheSelectedCell_OrFalseOutOfRange()
        {
            var state = Opened(2, 3);
            state.SelBay = 1; state.SelLevel = 2;
            Assert.True(state.TryGetSelected(out var cell));
            Assert.Same(state.Bays[1][2], cell);

            state.SelBay = 9;
            Assert.False(state.TryGetSelected(out _));
        }

        [Fact]
        public void ClampSelection_KeepsSelectionInsideTheMatrix()
        {
            var state = Opened(2, 3);
            state.SelBay = 5; state.SelLevel = 5;
            state.ClampSelection();
            Assert.Equal(1, state.SelBay);   // last bay
            Assert.Equal(2, state.SelLevel); // last level
        }

        // ---- ApplyScope ----

        [Fact]
        public void ApplyScope_Cell_TouchesOnlyTheSelectedCell()
        {
            var state = Opened(3, 3);
            state.SelBay = 1; state.SelLevel = 1;
            var values = new SelectiveEditorCell { Frente = 99, Alto = 88, PalletCount = 4, BeamId = BeamId, BeamPeralte = 5 };

            var touched = state.ApplyScope(SelectiveApplyScope.Cell, values);

            Assert.Single(touched);
            Assert.Equal((1, 1), touched[0]);
            Assert.Equal(99, state.Bays[1][1].Frente);
            Assert.Equal(42, state.Bays[0][0].Frente); // untouched
        }

        [Fact]
        public void ApplyScope_Row_TouchesEveryFrenteAtTheSelectedLevel()
        {
            var state = Opened(3, 3);
            state.SelBay = 0; state.SelLevel = 2;
            var values = new SelectiveEditorCell { Frente = 99, Alto = 88, PalletCount = 4, BeamId = BeamId, BeamPeralte = 5 };

            var touched = state.ApplyScope(SelectiveApplyScope.Row, values);

            Assert.Equal(new[] { (0, 2), (1, 2), (2, 2) }, touched);
            Assert.All(state.Bays, column => Assert.Equal(99, column[2].Frente));
            Assert.All(state.Bays, column => Assert.Equal(42, column[0].Frente));
        }

        [Fact]
        public void ApplyScope_Column_TouchesEveryLevelOfTheSelectedFrente()
        {
            var state = Opened(3, 3);
            state.SelBay = 2; state.SelLevel = 0;
            var values = new SelectiveEditorCell { Frente = 99, Alto = 88, PalletCount = 4, BeamId = BeamId, BeamPeralte = 5 };

            var touched = state.ApplyScope(SelectiveApplyScope.Column, values);

            Assert.Equal(new[] { (2, 0), (2, 1), (2, 2) }, touched);
            Assert.All(state.Bays[2], cell => Assert.Equal(99, cell.Frente));
            Assert.Equal(42, state.Bays[0][0].Frente);
        }

        [Fact]
        public void ApplyScope_All_TouchesEveryCell_IncludingRaggedColumns()
        {
            var state = Opened(2, 3);
            state.Bays[1].RemoveAt(2); // ragged: bay 1 has 2 levels, bay 0 has 3
            var values = new SelectiveEditorCell { Frente = 99, Alto = 88, PalletCount = 4, BeamId = BeamId, BeamPeralte = 5 };

            var touched = state.ApplyScope(SelectiveApplyScope.All, values);

            Assert.Equal(5, touched.Count); // 3 + 2
            Assert.All(state.Bays.SelectMany(c => c), cell => Assert.Equal(99, cell.Frente));
        }

        [Fact]
        public void ApplyScope_Row_SkipsFrentesShorterThanTheSelectedLevel()
        {
            var state = Opened(2, 3);
            state.Bays[1].RemoveAt(2); // bay 1 has only levels 0,1
            state.SelBay = 0; state.SelLevel = 2;
            var values = new SelectiveEditorCell { Frente = 99, BeamId = BeamId, BeamPeralte = 4 };

            var touched = state.ApplyScope(SelectiveApplyScope.Row, values);

            Assert.Equal(new[] { (0, 2) }, touched); // bay 1 has no level 2
        }

        // ---- ResizeBays ----

        [Fact]
        public void ResizeBays_Grow_ClonesTheLastBayIndependently()
        {
            var state = Opened(1, 2);
            state.Bays[0][0].Frente = 77;
            state.FloorBeams[0] = true;

            state.ResizeBays(3);

            Assert.Equal(3, state.Bays.Count);
            Assert.Equal(77, state.Bays[2][0].Frente); // cloned from the last
            Assert.True(state.FloorBeams[2]);
            state.Bays[2][0].Frente = 5;
            Assert.Equal(77, state.Bays[0][0].Frente); // clone is independent
        }

        [Fact]
        public void ResizeBays_Shrink_DropsFromTheEnd_AndClampsSelection()
        {
            var state = Opened(4, 2);
            state.SelBay = 3;
            state.ResizeBays(2);
            Assert.Equal(2, state.Bays.Count);
            Assert.Equal(2, state.FloorBeams.Count);
            Assert.Equal(2, state.BaySegments.Count);
            Assert.Equal(1, state.SelBay); // clamped
        }

        // ---- AddLevel / RemoveLevel ----

        [Fact]
        public void AddLevel_ClonesTheTopLevel_RemoveLevel_GuardsTheLastOne()
        {
            var state = Opened(1, 1);
            state.Bays[0][0].Alto = 71;
            state.AddLevel(0);
            Assert.Equal(2, state.Bays[0].Count);
            Assert.Equal(71, state.Bays[0][1].Alto); // cloned

            Assert.True(state.RemoveLevel(0));
            Assert.Single(state.Bays[0]);
            Assert.False(state.CanRemoveLevel(0));
            Assert.False(state.RemoveLevel(0)); // refuses the last level (no change)
            Assert.Single(state.Bays[0]);
        }

        // ---- Per-fondo transitions ----

        [Fact]
        public void SaveAndLoadFondo_KeepEachFondosMatrixIsolated()
        {
            var state = Opened(2, 3);
            // Add a second fondo cloned from fondo 0 (the "Número de fondos" 1→2 flow).
            state.SaveWorkingToSelected(48.0, 0.0);
            state.FondoMatrices.Add(state.CloneAligned(state.FondoMatrices[0], state.FondoMatrices[0].Bays.Count, state.FondoMatrices[0]));

            // Edit fondo 1: switch to it, drop a level in bay 0.
            state.SaveWorkingToSelected(48.0, 0.0);
            state.SelectedFondo = 1;
            state.LoadFondo(1);
            state.RemoveLevel(0);
            state.SaveWorkingToSelected(40.0, 0.0); // fondo 1 shallower

            // Fondo 0 is untouched (still 3 levels, depth 48); fondo 1 has 2 levels, depth 40.
            Assert.Equal(3, state.FondoMatrices[0].Bays[0].Count);
            Assert.Equal(48.0, state.FondoMatrices[0].Depth);
            Assert.Equal(2, state.FondoMatrices[1].Bays[0].Count);
            Assert.Equal(40.0, state.FondoMatrices[1].Depth);
        }

        [Fact]
        public void SnapshotAndRestore_AreDeepCopies()
        {
            var state = Opened(2, 2);
            state.Bays[0][0].Frente = 33;
            var snap = state.SnapshotWorking(50.0, 2.0);

            state.Bays[0][0].Frente = 99; // mutate the working matrix AFTER snapshotting
            Assert.Equal(33, snap.Bays[0][0].Frente); // snapshot is independent
            Assert.Equal(50.0, snap.Depth);
            Assert.Equal(2.0, snap.CabeceraOverride);

            state.RestoreWorkingFrom(snap);
            Assert.Equal(33, state.Bays[0][0].Frente); // restored from the snapshot
            state.Bays[0][0].Frente = 1;
            Assert.Equal(33, snap.Bays[0][0].Frente); // restore also deep-copied (snap still intact)
        }

        [Fact]
        public void CloneAligned_GrowsToBayCount_SeedingNewBaysFromTheWidthSeed()
        {
            var seed = Opened(3, 2);
            seed.Bays[2][0].Frente = 55;
            var source = seed.SnapshotWorking(48.0, 0.0); // 3 bays

            var shorter = new SelectiveEditorFondoMatrix { Depth = 48.0 };
            shorter.Bays.Add(new List<SelectiveEditorCell> { new SelectiveEditorCell { Frente = 10 } });
            shorter.FloorBeams.Add(false); shorter.BayHeights.Add(null); shorter.BaySegments.Add(new List<SelectiveSegment>());

            var aligned = seed.CloneAligned(shorter, 3, source); // grow the 1-bay matrix to 3, seeding from source

            Assert.Equal(3, aligned.Bays.Count);
            Assert.Equal(10, aligned.Bays[0][0].Frente);  // its own first bay
            Assert.Equal(55, aligned.Bays[2][0].Frente);  // seeded from source's bay 2
        }

        [Fact]
        public void MaxFrenteCount_UsesLiveWorkingForSelected_AndSlotsForTheRest()
        {
            var state = Opened(2, 2);
            state.SaveWorkingToSelected(48.0, 0.0);
            state.FondoMatrices.Add(state.CloneAligned(state.FondoMatrices[0], 4, state.FondoMatrices[0])); // fondo 1 has 4 bays

            // Editing fondo 0 (2 bays live); fondo 1 slot has 4 → master is 4.
            Assert.Equal(4, state.MaxFrenteCount());

            // Switch to fondo 1 and grow the working matrix to 5 — the live copy wins over its stale slot.
            state.SelectedFondo = 1;
            state.LoadFondo(1);
            state.ResizeBays(5);
            Assert.Equal(5, state.MaxFrenteCount());
        }

        [Fact]
        public void SyncPostCabeceras_SizesToMasterPostsPreservingEntries()
        {
            var state = Opened(2, 2);
            var custom = new RackCad.Domain.RackFrames.RackFrameConfiguration();
            state.PostCabeceras.Add(custom);
            state.PostPeraltes.Add(7.0);

            state.SyncPostCabeceras(); // 2 frentes → 3 posts

            Assert.Equal(3, state.PostCabeceras.Count);
            Assert.Equal(3, state.PostPeraltes.Count);
            Assert.Same(custom, state.PostCabeceras[0]); // preserved
            Assert.Equal(7.0, state.PostPeraltes[0]);
            Assert.Null(state.PostCabeceras[2]);         // padded
            Assert.Equal(0.0, state.PostPeraltes[2]);
        }

        // ---- FondoMatrixFromDesignBays / BuildBayDesigns round-trip ----

        [Fact]
        public void FondoMatrixFromDesignBays_PadsEmptyFrentes_AndCounts()
        {
            var bays = new List<SelectiveBayDesign> { DesignBay(2), new SelectiveBayDesign() /* empty */, DesignBay(3) };
            var state = NewState();

            var m = state.FondoMatrixFromDesignBays(bays, out var padded);

            Assert.Equal(1, padded);
            Assert.Equal(3, m.Bays.Count);
            Assert.Single(m.Bays[1]); // padded with a default cell
        }

        [Fact]
        public void BuildBayDesigns_RoundTripsFromFondoMatrix()
        {
            var bays = new List<SelectiveBayDesign> { DesignBay(3, floor: true, frente: 40), DesignBay(2, frente: 44) };
            var state = NewState();
            var m = state.FondoMatrixFromDesignBays(bays, out _);

            var round = SelectiveEditorState.BuildBayDesigns(m);

            Assert.Equal(2, round.Count);
            Assert.True(round[0].FloorBeam);
            Assert.Equal(3, round[0].Levels.Count);
            Assert.Equal(40, round[0].Levels[0].Pallet.Frente);
            Assert.Equal(2, round[1].Levels.Count);
            Assert.Equal(44, round[1].Levels[0].Pallet.Frente);
        }

        // ---- BuildDesign: field-level + resolved-geometry equivalence ----

        [Fact]
        public void BuildDesign_SingleFondo_AssemblesTheExpectedDesign()
        {
            var state = Opened(2, 3);
            state.FloorBeams[0] = true;
            state.Bays[0][0].Frente = 40;

            var design = state.BuildDesign(Inputs(depthCount: 1, workingDepth: 48.0));

            Assert.NotNull(design);
            Assert.Equal(PostId, design.PostId);
            Assert.Equal(48.0, design.PalletDepth);
            Assert.Equal(1, design.DepthCount);
            Assert.Equal(2, design.Bays.Count);
            Assert.True(design.Bays[0].FloorBeam);
            Assert.Equal(3, design.Bays[0].Levels.Count);
            Assert.Equal(40, design.Bays[0].Levels[0].Pallet.Frente);
            Assert.True(design.DrawBasePlate);
            Assert.Single(design.CabeceraFondoOverrides); // one per fondo
            Assert.Equal(3, design.PostCabeceras.Count);  // synced to masterFrentes+1
        }

        [Fact]
        public void BuildDesign_ReturnsNull_WhenFondoZeroHasNoLevels()
        {
            var state = NewState();
            state.InitMatrix(1, 1);
            state.FondoMatrices.Add(state.SnapshotWorking(48.0, 0.0));
            state.Bays[0].Clear(); // simulate an empty working column
            state.SelectedFondo = 0;

            Assert.Null(state.BuildDesign(Inputs()));
        }

        [Fact]
        public void BuildDesign_MultiFondo_CarriesPerFondoLevelsDepthsAndSeparators()
        {
            // Two fondos: fondo 0 = 2×3, fondo 1 = 2×2 @ depth 40.
            var state = Opened(2, 3);
            state.SaveWorkingToSelected(48.0, 0.0);
            state.FondoMatrices.Add(state.CloneAligned(state.FondoMatrices[0], 2, state.FondoMatrices[0]));
            state.SelectedFondo = 1;
            state.LoadFondo(1);
            state.RemoveLevel(0); state.RemoveLevel(1);
            state.SaveWorkingToSelected(40.0, 0.0);
            state.SelectedFondo = 0;
            state.LoadFondo(0);

            var design = state.BuildDesign(Inputs(depthCount: 2, workingDepth: 48.0, separators: new List<double> { 8.0 }));

            Assert.Equal(2, design.DepthCount);
            Assert.Equal(48.0, design.PalletDepth);
            Assert.Single(design.ExtraFondoBays);
            Assert.Equal(2, design.ExtraFondoBays[0][0].Levels.Count); // fondo 1 kept its own 2 levels
            Assert.Equal(40.0, design.ExtraFondoDepths[0]);
            Assert.Equal(new[] { 8.0 }, design.SeparatorLengths);
            Assert.Equal(2, design.CabeceraFondoOverrides.Count); // one per fondo
        }

        [Fact]
        public void BuildDesign_ResolvesToTheSameGeometryAsAHandBuiltDesign()
        {
            // Build the same rack two ways and assert the resolved system matches: 2 frentes × 3 levels, floor beams,
            // per-post peralte override on post 0.
            var state = Opened(2, 3);
            state.FloorBeams[0] = true;
            state.FloorBeams[1] = true;
            state.PostPeraltes.Add(5.0); state.PostPeraltes.Add(0.0); state.PostPeraltes.Add(0.0);
            var built = state.BuildDesign(Inputs(depthCount: 1, workingDepth: 48.0));

            var hand = new SelectivePalletDesign
            {
                PostId = PostId, PostPeralte = 3.0, PalletTolerance = 4.0, VerticalClearance = 6.0,
                FloorBeamRise = 4.0, PalletDepth = 48.0, DepthCount = 1, DrawBasePlate = true
            };
            hand.Bays.Add(DesignBay(3, floor: true));
            hand.Bays.Add(DesignBay(3, floor: true));
            hand.PostPeraltes.Add(5.0); hand.PostPeraltes.Add(0.0); hand.PostPeraltes.Add(0.0);

            var catalog = Catalog;
            var a = new SelectiveGeometryResolver().Resolve(built, catalog);
            var b = new SelectiveGeometryResolver().Resolve(hand, catalog);

            Assert.Equal(b.Height, a.Height, 6);
            Assert.Equal(b.Bays.Count, a.Bays.Count);
            Assert.Equal(b.PostPeraltes, a.PostPeraltes);
            for (var i = 0; i < b.Bays.Count; i++)
            {
                Assert.Equal(b.Bays[i].BeamLength, a.Bays[i].BeamLength, 6);
                Assert.Equal(b.Bays[i].Levels.Select(l => l.Y), a.Bays[i].Levels.Select(l => l.Y));
            }
        }

        private static SelectiveBayDesign DesignBay(int levels, bool floor = false, double frente = 42.0, double alto = 60.0)
        {
            var bay = new SelectiveBayDesign { FloorBeam = floor };
            for (var l = 0; l < levels; l++)
            {
                bay.Levels.Add(new SelectiveCell { Pallet = new Tarima { Frente = frente, Alto = alto }, PalletCount = 2, BeamId = BeamId, BeamPeralte = 4.0 });
            }

            return bay;
        }
    }
}
