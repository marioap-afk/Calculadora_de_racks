#nullable enable
using System.Linq;
using RackCad.Application.Persistence;
using RackCad.Application.VentanaRackCad;
using RackCad.Domain.Systems.Dynamic;
using RackCad.Domain.Systems.Shared;
using RackCad.UI.Systems.Dynamic;
using Xunit;

namespace RackCad.UI.Tests
{
    /// <summary>
    /// I-64 A-14, line D (Dinámico): the draft of the RackCad window recomposes like the classic editor. For the same inputs with at most
    /// two decimals, the persisted design is the same, byte for byte, for a new rack and for a design opened from the drawing, with and
    /// without edits. The differences are declared and fixed here too (I-03 of the second review): the draft keeps the full precision of a
    /// value where the classic editor rounds it to two decimals through its boxes, it keeps the dimension style as read, and a post that the
    /// catalog does not have gives way to the default post, as in the classic editor, but said.
    /// </summary>
    public class DynamicDraftEditorEquivalenceTests
    {
        private static DynamicRackDesign Structure()
        {
            var design = new DynamicRackDesign
            {
                Pallet = new PalletSpecification(42.0, 48.0, 60.0, 1000.0, "kg"),
                PalletsDeep = 6,
                LoadLevels = 3,
                FirstLevelHeight = 6.0,
                BeamDepth = 4.0,
            };
            design.Fronts.Add(new DynamicRackFrontDesign { PalletCount = 2, LoadLevels = 3, PalletsDeep = 6, DepthStartPosition = 1 });
            design.Fronts.Add(new DynamicRackFrontDesign { PalletCount = 1, LoadLevels = 2, PalletsDeep = 4, DepthStartPosition = 3 });
            return design;
        }

        private static string Json(DynamicRackDesign design) => new RackProjectStore().Serialize(RackProject.ForDynamic(design));

        [Fact]
        public void ANewRackIsTheDesignOfTheClassicEditor()
        {
            StaTestRunner.Run(() =>
            {
                var window = new RackDynamicSystemWindow();
                try
                {
                    var classic = window.BuildDesignForTest(out var ok);
                    Assert.True(ok);

                    var draft = DynamicDraftEditor.CreateNew(window.Session.Catalog);

                    Assert.Null(draft.LastError);
                    Assert.Equal(Json(classic), Json(draft.Design));
                }
                finally { window.Close(); }
            });
        }

        [Fact]
        public void ADesignOpenedAndRecomposedIsTheDesignOfTheClassicEditor()
        {
            StaTestRunner.Run(() =>
            {
                var window = new RackDynamicSystemWindow();
                try
                {
                    window.LoadExisting(Structure(), "DIN-1", "DIN 1");
                    var classic = window.BuildDesignForTest(out var ok);
                    Assert.True(ok);

                    var draft = new DynamicDraftEditor(window.Session.Catalog);
                    draft.LoadFrom(Structure(), "DIN 1");
                    Assert.True(draft.Recompose(), draft.LastError);

                    Assert.Equal(Json(classic), Json(draft.Design));
                    Assert.Equal(2, draft.Matrix.Count);
                }
                finally { window.Close(); }
            });
        }

        [Fact]
        public void TheSameEditOfTheMatrixGivesTheSameDesignAsTheClassicEditor()
        {
            StaTestRunner.Run(() =>
            {
                var window = new RackDynamicSystemWindow();
                try
                {
                    window.LoadExisting(Structure(), "DIN-1", "DIN 1");
                    window.BuildDesignForTest(out _);

                    var draft = new DynamicDraftEditor(window.Session.Catalog);
                    draft.LoadFrom(Structure(), "DIN 1");
                    draft.Recompose();

                    // The same structural edit in both matrices (the window's through its own model): a third front copied from the
                    // selected one, and taller pallets in the SECOND front. Not in the first: on every recomposition the classic editor
                    // copies its field buffer into the selected cell (F1·N1), which the model edit did not touch (inventory, pain 4).
                    foreach (var matrix in new[] { window.MatrixForTest, draft.Matrix })
                    {
                        matrix.SetFrontCount(3);
                        foreach (var cell in matrix.Fronts[1].Cells) cell.PalletHeight = 72.0;
                    }

                    var classic = window.BuildDesignForTest(out var ok);
                    Assert.True(ok);
                    Assert.True(draft.Recompose(), draft.LastError);

                    Assert.Equal(Json(classic), Json(draft.Design));
                    Assert.Equal(3, draft.Design.Fronts.Count);
                }
                finally { window.Close(); }
            });
        }

        [Fact]
        public void AnInvalidInputIsRefusedWithTheMessageOfTheClassicEditorAndKeepsTheLastValidDesign()
        {
            StaTestRunner.Run(() =>
            {
                var window = new RackDynamicSystemWindow();
                try
                {
                    var draft = DynamicDraftEditor.CreateNew(window.Session.Catalog);
                    var valid = Json(draft.Design);

                    draft.PostPeralte = 0.0;
                    Assert.False(draft.Recompose());
                    Assert.Equal("El peralte de poste debe ser un número mayor que cero.", draft.LastError);
                    Assert.Equal(valid, Json(draft.Design));

                    draft.PostPeralte = 3.0;
                    draft.Matrix.Fronts[0].PalletsDeep = 1;
                    Assert.False(draft.Recompose());
                    Assert.Equal("Cada frente requiere al menos 2 fondos y una posición inicial >= 1.", draft.LastError);
                    Assert.Equal(valid, Json(draft.Design));
                }
                finally { window.Close(); }
            });
        }

        [Fact]
        public void ARecompositionThatFailsKeepsTheLastValidSystemAndNeverMutatesThePublishedOne()
        {
            // M-14 of the second review: the recomposition works on a copy of the system.
            var draft = new DynamicDraftEditor(UiSupport.LoadCatalogSafe());
            draft.LoadFrom(Structure(), "DIN 1");
            Assert.True(draft.Recompose(), draft.LastError);
            var system = draft.System;
            var separators = system.SeparatorCountOverride;

            draft.SeparatorCountOverride = 3;
            draft.Matrix.Fronts[0].PalletsDeep = 1;
            Assert.False(draft.Recompose());

            Assert.Same(system, draft.System);
            Assert.Equal(separators, draft.System.SeparatorCountOverride);

            draft.Matrix.Fronts[0].PalletsDeep = 6;
            Assert.True(draft.Recompose(), draft.LastError);
            Assert.NotSame(system, draft.System);
            Assert.Equal(separators, system.SeparatorCountOverride);
            Assert.Equal(3, draft.System.SeparatorCountOverride);
        }

        [Fact]
        public void TheSnapshotForAutoCadIsACopyThatLaterEditsDoNotReach()
        {
            // B-02 of the second review: the design goes through its JSON and the system is resolved again from that copy.
            var draft = new DynamicDraftEditor(UiSupport.LoadCatalogSafe());
            draft.LoadFrom(Structure(), " DIN 1 ");
            Assert.True(draft.Recompose(), draft.LastError);

            var snapshot = draft.SnapshotForWrite();

            Assert.NotSame(draft.Design, snapshot.Design);
            Assert.NotSame(draft.System, snapshot.System);
            Assert.Equal(Json(draft.Design), snapshot.DesignJson);
            Assert.Equal(snapshot.DesignJson, Json(snapshot.Design));
            Assert.Equal("DIN 1", snapshot.Name);
            Assert.Equal("DIN 1", snapshot.System.Name);
            Assert.Equal(draft.System.Modules.Count, snapshot.System.Modules.Count);
            Assert.Equal(draft.System.Fronts.Count, snapshot.System.Fronts.Count);
            Assert.Equal(draft.System.PostPeralte, snapshot.System.PostPeralte);
            Assert.Equal(draft.System.SeparatorCountOverride, snapshot.System.SeparatorCountOverride);

            draft.SeparatorCountOverride = 3;
            foreach (var cell in draft.Matrix.Fronts[0].Cells) cell.PalletWeight = 1300.0;
            Assert.True(draft.Recompose(), draft.LastError);

            Assert.Null(snapshot.System.SeparatorCountOverride);
            Assert.Null(snapshot.Design.SeparatorCountOverride);
            Assert.All(snapshot.Design.Fronts[0].Levels, level => Assert.Equal(1000.0, level.PalletWeight));
        }

        [Fact]
        public void TheDraftKeepsTheFullPrecisionWhereTheClassicEditorRoundsToTwoDecimals()
        {
            // Declared difference (I-03): 1200 mm of fondo are 47.2441 in; the classic editor writes 47.24 after its boxes are read again.
            StaTestRunner.Run(() =>
            {
                var window = new RackDynamicSystemWindow();
                try
                {
                    var metric = Structure();
                    metric.Pallet = new PalletSpecification(42.0, 47.2441, 60.0, 1000.555, "kg");
                    window.LoadExisting(metric, "DIN-1", "DIN 1");
                    var classic = window.BuildDesignForTest(out var ok);
                    Assert.True(ok);

                    var draft = new DynamicDraftEditor(window.Session.Catalog);
                    var same = Structure();
                    same.Pallet = new PalletSpecification(42.0, 47.2441, 60.0, 1000.555, "kg");
                    draft.LoadFrom(same, "DIN 1");
                    Assert.True(draft.Recompose(), draft.LastError);

                    Assert.Equal(47.24, classic.Pallet.Depth, 6);
                    Assert.Equal(47.2441, draft.Design.Pallet.Depth, 6);
                }
                finally { window.Close(); }
            });
        }

        [Fact]
        public void APostThatTheCatalogDoesNotHaveGivesWayToTheDefaultPostAndAKnownPostInAnotherSpellingIsThatPost()
        {
            var catalog = UiSupport.LoadCatalogSafe();
            var known = catalog.PostProfiles.First().Id;

            var unknown = Structure();
            unknown.HeaderPostCatalogId = "POSTE_X";
            var draft = new DynamicDraftEditor(catalog);
            draft.LoadFrom(unknown, "DIN 1");
            Assert.Equal(catalog.Defaults?.Post, draft.PostCatalogId);
            Assert.Equal("El poste «POSTE_X» no está en el catálogo: se usa «" + catalog.Defaults?.Post + "», como en el editor clásico.", draft.LoadNotice);
            Assert.True(draft.Recompose(), draft.LastError);
            Assert.Equal(catalog.Defaults?.Post, draft.Design.HeaderPostCatalogId);

            var spelled = Structure();
            spelled.HeaderPostCatalogId = known.ToLowerInvariant() == known ? known.ToUpperInvariant() : known.ToLowerInvariant();
            draft.LoadFrom(spelled, "DIN 1");
            Assert.Equal(known, draft.PostCatalogId);
            Assert.Null(draft.LoadNotice);
        }

        [Fact]
        public void TheDimensionStyleIsKeptAsRead()
        {
            // Declared difference (I-03): the window does not know the styles of the drawing; the drawing dimensions with its current
            // style when the named one is not there, as it does for the classic editor's null.
            var design = Structure();
            design.DimensionStyle = "ESTILO_QUE_NO_EXISTE";
            var draft = new DynamicDraftEditor(UiSupport.LoadCatalogSafe());
            draft.LoadFrom(design, "DIN 1");
            Assert.True(draft.Recompose(), draft.LastError);
            Assert.Equal("ESTILO_QUE_NO_EXISTE", draft.Design.DimensionStyle);
        }
    }
}
