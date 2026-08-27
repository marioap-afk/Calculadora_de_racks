using System.Linq;
using RackCad.Application.Catalogs;
using RackCad.Application.Systems.Dynamic;
using RackCad.Application.Systems.PushBack;
using RackCad.Domain.Systems.Dynamic;
using RackCad.Domain.Systems.PushBack;
using RackCad.Domain.Systems.Shared;
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

            // La GEOMETRIA con la que ese documento se guardo: es lo que hay que conservar.
            var before = new PushBackResolver(catalog).Resolve(design);
            var physical = before.Structure.Fronts[0].LoadBeamLevels
                .OrderBy(level => level.LevelNumber).First().ExitElevation;

            var state = new PushBackEditorState();
            var inputs = state.LoadFromDesign(design, new PushBackResolver(catalog));

            Assert.NotEmpty(state.Structure.Fronts);

            // I-42 (correccion aislada 3): este documento no trae marcador de datum, asi que la carga lo re-expresa
            // UNA vez sobre el troquel utilizable mas bajo. El NUMERO cambia —se midio la retícula real, no se resto
            // ninguna constante— y la GEOMETRIA no: es exactamente lo que el dueño pidio comprobar, el troquel y no
            // el contenido del cuadro. Lo que la prueba defiende sigue en pie: cargar NO es un diseño nuevo, asi que
            // el valor no cae al 4" del rack nuevo.
            Assert.All(
                state.Structure.Fronts,
                front => Assert.NotEqual(PushBackDefaults.DefaultFirstLevelHeight, front.FirstLevelHeight, 9));

            var after = new PushBackEditorDesignAssembler(catalog).Build(state, inputs).System;
            Assert.Equal(
                physical,
                after.Structure.Fronts[0].LoadBeamLevels.OrderBy(level => level.LevelNumber).First().ExitElevation,
                9);
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
