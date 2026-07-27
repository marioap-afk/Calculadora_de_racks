using System.Linq;
using RackCad.Domain.Systems.Dynamic;
using RackCad.Domain.Systems.PushBack;
using RackCad.Domain.Systems.Selective;
using RackCad.Domain.Systems.Shared;
using RackCad.UI;
using RackCad.UI.Systems.PushBack;
using Xunit;

namespace RackCad.UI.Tests
{
    /// <summary>
    /// PB-002 / PB-003 (I-32) — the desviador grid Push Back opens.
    ///
    /// PB-002: the grid offered the LAST post (and every interior one next to a taller front) a single level, because
    /// the window handed the shared dialog a per-FRONT list flagged as per-POST. Those cells WERE drawn but could not
    /// be switched off. The count now comes from the canonical adjacent-front rule.
    ///
    /// PB-003: Push Back's safety only ever lives at the low (entrance/exit) end — its authority collapses every side
    /// to Left — so the "Lado" selector was inert. It is gone for Push Back, and untouched everywhere else.
    /// </summary>
    public sealed class PushBackDesviadorGridTests
    {
        private static PushBackDesign TwoFronts(int front0Levels, int front1Levels)
        {
            var design = new PushBackDesign
            {
                Structure = new DynamicRackDesign
                {
                    Pallet = new PalletSpecification(42.0, 48.0, 60.0, 1000.0, "kg"),
                    PalletsDeep = 4,
                    LoadLevels = System.Math.Max(front0Levels, front1Levels),
                    FirstLevelHeight = 6.0,
                    BeamDepth = 4.0
                }
            };
            design.Structure.Fronts.Add(new DynamicRackFrontDesign { PalletCount = 1, LoadLevels = front0Levels, PalletsDeep = 4, DepthStartPosition = 1 });
            design.Structure.Fronts.Add(new DynamicRackFrontDesign { PalletCount = 1, LoadLevels = front1Levels, PalletsDeep = 4, DepthStartPosition = 1 });
            design.Fronts.Add(new PushBackFrontConfig());
            design.Fronts.Add(new PushBackFrontConfig());
            return design;
        }

        // ---- PB-002: the count the window hands the shared dialog ----

        [Fact]
        public void DesviadorLevelsPerPost_IsTheMaxOfAdjacentFronts_NotThePerFrontListIndexedByPost()
        {
            var perPost = StaTestRunner.Run(() =>
            {
                var w = new RackPushBackSystemWindow(canInsertInAutoCad: true);
                w.LoadExisting(TwoFronts(3, 1), "GUID-PB", "PB");
                return w.DesviadorLevelsPerPost().ToArray();
            });

            // 2 fronts (3 and 1 levels) => 3 posts. Before the fix the window produced [3, 1, 1]: the interior post
            // lost the taller neighbour's levels and the last post fell off the end of the per-front list.
            Assert.Equal(new[] { 3, 3, 1 }, perPost);
        }

        [Fact]
        public void DesviadorLevelsPerPost_CoversEveryPost_OfASingleFrontRack()
        {
            var perPost = StaTestRunner.Run(() =>
            {
                var w = new RackPushBackSystemWindow(canInsertInAutoCad: true);
                return w.DesviadorLevelsPerPost().ToArray();
            });

            Assert.Equal(new[] { 3, 3 }, perPost);   // a new rack: 1 front x 3 levels => 2 posts, both with 3
        }

        /// <summary>
        /// End to end through the shared grid: the columns it builds are the ones the window computed, and every cell
        /// of every column is present (togglable), which is the point — the user can now switch off what is drawn.
        /// </summary>
        [Fact]
        public void SharedGrid_BuildsOneFullColumnPerPost_WhenGivenThePerPostCount()
        {
            var r = StaTestRunner.Run(() =>
            {
                var w = new RackPushBackSystemWindow(canInsertInAutoCad: true);
                w.LoadExisting(TwoFronts(3, 1), "GUID-PB", "PB");
                var perPost = w.DesviadorLevelsPerPost();

                var grid = new SafetyDesviadorGridWindow(
                    "DESVIADOR_L_3_5", "Desviador",
                    system: null, catalog: null,
                    longitud: SelectiveSafetyDefaults.DesviadorLongitud,
                    firstHeight: SelectiveSafetyDefaults.DesviadorPrimerNivelAltura,
                    side: SafetySide.Left,
                    offCells: null,
                    fallbackPostCount: perPost.Count,
                    fallbackLevelsPerFrente: perPost,
                    fallbackLevelsArePerPost: true,
                    showSide: false);

                grid.Model.SetAll(true);
                return (levelCounts: grid.BuildResult().LevelCounts.ToArray(), selected: grid.Model.SelectedCount);
            });

            Assert.Equal(new[] { 3, 3, 1 }, r.levelCounts);
            Assert.Equal(7, r.selected);   // 3 + 3 + 1 present cells; before the fix the grid only had 3 + 1 + 1
        }

        // ---- PB-003: no "Lado" for Push Back, and the canonical side is kept ----

        [Fact]
        public void Desviador_HasNoSideSelector_ForPushBack_AndKeepsTheLowEndSide()
        {
            var r = StaTestRunner.Run(() =>
            {
                var pushBack = new SafetyDesviadorGridWindow(
                    "DESVIADOR_L_3_5", "Desviador", system: null, catalog: null,
                    longitud: SelectiveSafetyDefaults.DesviadorLongitud,
                    firstHeight: SelectiveSafetyDefaults.DesviadorPrimerNivelAltura,
                    side: SafetySide.Both, offCells: null,
                    fallbackPostCount: 2, fallbackLevelsPerFrente: new[] { 2, 2 },
                    fallbackLevelsArePerPost: true, showSide: false);

                var selective = new SafetyDesviadorGridWindow(
                    "DESVIADOR_L_3_5", "Desviador", system: null, catalog: null,
                    longitud: SelectiveSafetyDefaults.DesviadorLongitud,
                    firstHeight: SelectiveSafetyDefaults.DesviadorPrimerNivelAltura,
                    side: SafetySide.Both, offCells: null,
                    fallbackPostCount: 3, fallbackLevelsPerFrente: new[] { 2, 2 },
                    fallbackLevelsArePerPost: false);

                return (
                    pushBackHasSide: SafetyDialogTestSupport.HasSideSelector(pushBack),
                    pushBackSide: pushBack.BuildResult().Side,
                    selectiveHasSide: SafetyDialogTestSupport.HasSideSelector(selective),
                    selectiveSide: selective.BuildResult().Side);
            });

            Assert.False(r.pushBackHasSide);
            Assert.Equal(SafetySide.Left, r.pushBackSide);      // the low end, which is where the authority puts it anyway

            // The default path (Selectivo / Dinámico) is untouched: the selector is there and "Ambas" still wins.
            Assert.True(r.selectiveHasSide);
            Assert.Equal(SafetySide.Both, r.selectiveSide);
        }
    }
}
