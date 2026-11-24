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
    /// I-64 A-14, line D (Dinámico): the draft of the RackCad window recomposes EXACTLY like the classic editor. For the same inputs
    /// the persisted design is the same, byte for byte, for a new rack and for a design opened from the drawing, with and without
    /// edits. It is the one guarantee that the new editor writes what the classic one would.
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
    }
}
