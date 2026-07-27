using System.Linq;
using RackCad.Application.Catalogs;
using RackCad.Application.Systems;
using RackCad.Domain.Systems;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>
    /// PB-012 (I-32) — a BRAND-NEW Push Back rack opens with "Alto 1er nivel" = 4", not the dynamic system's 6".
    /// The rule is Push Back's own (<see cref="PushBackDefaults.DefaultFirstLevelHeight"/>): the shared dynamic
    /// constant stays at 6" and the dynamic editor keeps opening at 6".
    ///
    /// The default must reach ONLY a new design. A persisted rack carries its own first-level height and reloading it
    /// must never be re-defaulted — that would silently rewrite a saved drawing.
    /// </summary>
    public class PushBackEditorDefaultsTests
    {
        private static RackCatalog Catalog => JsonRackCatalogProvider.FromBaseDirectory().Load();

        [Fact]
        public void NewRack_OpensWithFirstLevelHeightFour()
        {
            var state = new PushBackEditorState();
            state.LoadNew();

            Assert.NotEmpty(state.Structure.Fronts);
            Assert.All(state.Structure.Fronts, front => Assert.Equal(4.0, front.FirstLevelHeight, 9));
        }

        [Fact]
        public void NewRack_CarriesFirstLevelHeightFour_IntoTheBuiltDesign()
        {
            var state = new PushBackEditorState();
            var inputs = state.LoadNew();

            var design = new PushBackEditorDesignAssembler(Catalog).BuildDesign(state, inputs);
            Assert.NotEmpty(design.Structure.Fronts);
            Assert.All(design.Structure.Fronts, front => Assert.Equal(4.0, front.FirstLevelHeight ?? -1.0, 9));
        }

        [Fact]
        public void AddedFronts_InheritTheNewRacksFourInches()
        {
            var state = new PushBackEditorState();
            state.LoadNew();
            state.SetFrontCount(3);

            Assert.Equal(3, state.Structure.Fronts.Count);
            Assert.All(state.Structure.Fronts, front => Assert.Equal(4.0, front.FirstLevelHeight, 9));
        }

        /// <summary>
        /// The guard that matters: a SAVED rack keeps the height it was saved with. Loading is not a new design.
        /// </summary>
        [Fact]
        public void LoadedDesign_KeepsItsOwnFirstLevelHeight_NotTheNewRackDefault()
        {
            var catalog = Catalog;
            var design = new PushBackDesign
            {
                Structure = new DynamicRackDesign
                {
                    Pallet = new PalletSpecification(42.0, 48.0, 60.0, 1000.0, "kg"),
                    PalletsDeep = 4,
                    LoadLevels = 2,
                    FirstLevelHeight = 9.0,
                    BeamDepth = 4.0
                }
            };

            var state = new PushBackEditorState();
            state.LoadFromDesign(design, new PushBackResolver(catalog));

            Assert.NotEmpty(state.Structure.Fronts);
            Assert.All(state.Structure.Fronts, front => Assert.Equal(9.0, front.FirstLevelHeight, 9));
        }

        // ---- Isolation: the dynamic system keeps its own default ----

        [Fact]
        public void DynamicDefault_StaysAtSix()
        {
            Assert.Equal(6.0, DynamicRackDefaults.DefaultFirstLevelHeight, 9);

            var dynamicMatrix = new DynamicFrontMatrix();
            Assert.NotEmpty(dynamicMatrix.Fronts);
            Assert.All(dynamicMatrix.Fronts, front => Assert.Equal(6.0, front.FirstLevelHeight, 9));
        }
    }
}
