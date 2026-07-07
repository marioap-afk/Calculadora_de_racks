using System;
using System.Linq;
using RackCad.Application.Catalogs;
using Xunit;

namespace RackCad.Tests
{
    public class BeamCatalogTests
    {
        private static RackCatalog Catalog => JsonRackCatalogProvider.FromBaseDirectory().Load();

        [Fact]
        public void Catalog_LoadsBeamProfiles_WithPeralteAndMensula()
        {
            var beams = Catalog.BeamProfiles;

            Assert.NotEmpty(beams);
            Assert.All(beams, beam => Assert.True(beam.Peralte > 0.0));
            Assert.All(beams, beam => Assert.False(string.IsNullOrWhiteSpace(beam.Mensula)));
        }

        [Fact]
        public void Catalog_LoadsMensulas()
        {
            Assert.NotEmpty(Catalog.Mensulas);
        }

        [Fact]
        public void Catalog_EveryBeamMensula_ResolvesInTheMensulaCatalog()
        {
            var catalog = Catalog;
            var mensulaIds = catalog.Mensulas
                .Select(mensula => mensula.Id)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            Assert.All(catalog.BeamProfiles, beam => Assert.Contains(beam.Mensula, mensulaIds));
        }
    }
}
