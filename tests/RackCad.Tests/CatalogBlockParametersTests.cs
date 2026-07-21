using System.Linq;
using RackCad.Application.Catalogs;
using RackCad.Application.Catalogs.Validation;
using RackCad.Application.Headers;
using RackCad.Application.Systems;
using RackCad.Domain.Systems;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>
    /// Defect 2: the manifest must expect the REAL dynamic parameters Application applies to each block —
    /// LONGITUD of the rail / posts / separators, PERALTE, ALTURA of the pallet — from a single shared source
    /// (<see cref="CatalogBlockParameters"/>), and NEVER on unrelated blocks. Parameter names are the same
    /// domain constants the producers use, so the two cannot drift; a builder guard cross-checks that.
    /// </summary>
    public class CatalogBlockParametersTests
    {
        private const string Longitud = SelectiveRackDefaults.LengthParam;   // "LONGITUD"
        private const string Peralte = SelectiveRackDefaults.PeralteParam;   // "PERALTE"
        private const string Altura = SelectiveRackDefaults.PalletAltoParam; // "ALTURA"

        private static RackCatalog Catalog => JsonRackCatalogProvider.FromBaseDirectory().Load();

        [Fact]
        public void ExpectedParameters_Rail_RequiresLongitud()
        {
            Assert.Contains(Longitud, CatalogBlockParameters.ExpectedParameters(Catalog, TestCatalogIds.FlowBed.Rail, "LATERAL"));
        }

        [Fact]
        public void ExpectedParameters_Post_IsViewExact()
        {
            var post = TestCatalogIds.Profiles.Posts.Standard;

            var frontal = CatalogBlockParameters.ExpectedParameters(Catalog, post, "FRONTAL");
            Assert.Contains(Longitud, frontal);
            Assert.Contains(Peralte, frontal);

            var lateral = CatalogBlockParameters.ExpectedParameters(Catalog, post, "LATERAL");
            Assert.Contains(Longitud, lateral);
            Assert.DoesNotContain(Peralte, lateral); // the LATERAL post block has no PERALTE grip

            var planta = CatalogBlockParameters.ExpectedParameters(Catalog, post, "PLANTA");
            Assert.Contains(Peralte, planta);
            Assert.DoesNotContain(Longitud, planta);
        }

        [Fact]
        public void ExpectedParameters_Separator_RequiresLongitud_InFrontalAndPlantaOnly()
        {
            var separator = TestCatalogIds.Profiles.Spacers.Header;

            Assert.Contains(Longitud, CatalogBlockParameters.ExpectedParameters(Catalog, separator, "FRONTAL"));
            Assert.Contains(Longitud, CatalogBlockParameters.ExpectedParameters(Catalog, separator, "PLANTA"));
            // No production builder writes the separator LONGITUD in a LATERAL block.
            Assert.DoesNotContain(Longitud, CatalogBlockParameters.ExpectedParameters(Catalog, separator, "LATERAL"));
        }

        [Fact]
        public void ExpectedParameters_Beam_RequiresLongitudAndPeralte()
        {
            var beam = CatalogBlockParameters.ExpectedParameters(Catalog, TestCatalogIds.Profiles.Beams.SelectiveThreeRivet, "FRONTAL");
            Assert.Contains(Longitud, beam);
            Assert.Contains(Peralte, beam);
        }

        [Fact]
        public void ExpectedParameters_Pallet_RequiresLongitudAndAltura()
        {
            var pallet = CatalogBlockParameters.ExpectedParameters(Catalog, TestCatalogIds.BlockOnlyPieces.Pallet, "FRONTAL");
            Assert.Contains(Longitud, pallet);
            Assert.Contains(Altura, pallet);
        }

        [Fact]
        public void ExpectedParameters_UnrelatedBlocks_DoNotRequireLongitud()
        {
            // A base plate carries only PERALTE; a ménsula is a fixed block with no dynamic parameter.
            Assert.DoesNotContain(Longitud, CatalogBlockParameters.ExpectedParameters(Catalog, TestCatalogIds.BasePlates.Standard, "FRONTAL"));
            Assert.Contains(Peralte, CatalogBlockParameters.ExpectedParameters(Catalog, TestCatalogIds.BasePlates.Standard, "FRONTAL"));
            Assert.Empty(CatalogBlockParameters.ExpectedParameters(Catalog, TestCatalogIds.Mensulas.ThreeRivet, "FRONTAL"));
        }

        [Fact]
        public void Manifest_RailBlock_RequiresLongitud_MensulaBlockDoesNot()
        {
            var manifest = CatalogBlockManifest.BuildExpected(Catalog);

            var railBlock = manifest.Blocks.Single(b =>
                b.Pieces.Contains(TestCatalogIds.FlowBed.Rail) && b.Views.Contains("LATERAL"));
            Assert.Contains(Longitud, railBlock.Parameters);

            var mensulaBlock = manifest.Blocks.First(b => b.Pieces.Contains(TestCatalogIds.Mensulas.ThreeRivet));
            Assert.DoesNotContain(Longitud, mensulaBlock.Parameters);
        }

        [Fact]
        public void Manifest_ExpectsEveryParameterTheRailBuilderActuallyApplies()
        {
            // Anti-divergence guard: run the REAL builder and prove the manifest expects every dynamic
            // parameter it writes to the rail block. If a producer starts applying a new grip, this fails.
            var catalog = Catalog;
            var config = new FlowBedConfiguration
            {
                BedType = FlowBedType.Pushback,
                LaneDepth = 100.0,
                PalletDepth = 40.0,
                RollerId = TestCatalogIds.FlowBed.Roller1Point9
            };

            var rail = new FlowBedLateralBuilder().Build(config, catalog).Single(i => i.Role == HeaderBlockRole.Rail);
            Assert.Contains(Longitud, rail.DynamicParameters.Keys); // the builder really applies LONGITUD

            var expected = CatalogBlockParameters.ExpectedParameters(catalog, TestCatalogIds.FlowBed.Rail, "LATERAL");
            foreach (var applied in rail.DynamicParameters.Keys)
            {
                Assert.Contains(applied, expected);
            }
        }
    }
}
