using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Catalogs;
using RackCad.Application.RackFrames;
using RackCad.Domain.RackFrames;
using Xunit;

namespace RackCad.Tests
{
    public class FrameModelValidatorTests
    {
        private const double Tolerance = 0.01;

        private static RackFrameConfiguration StandardConfig()
        {
            return new RackFrameConfigurationFactory(JsonRackCatalogProvider.FromBaseDirectory().Load())
                .Build(RackFrameTemplateCatalog.Default, "POSTE_OMEGA_3X3", 132.0, 42.0);
        }

        private static RackCatalog ShippedCatalog()
        {
            return JsonRackCatalogProvider.FromBaseDirectory().Load();
        }

        [Theory]
        [InlineData(44.005, true)]
        [InlineData(44.02, false)]
        public void CollidesWithExisting_UsesTolerance(double candidate, bool expected)
        {
            var existing = new[] { 0.0, 44.0, 88.0, 132.0 };

            Assert.Equal(expected, FrameModelValidator.CollidesWithExisting(existing, candidate, Tolerance));
        }

        [Fact]
        public void Validate_CleanStandardConfig_HasNoWarnings()
        {
            var warnings = FrameModelValidator.Validate(StandardConfig(), ShippedCatalog(), Tolerance);

            Assert.Empty(warnings);
        }

        [Fact]
        public void Validate_NearEqualElevations_WarnsAndFlagsZeroHeightPanel()
        {
            var config = StandardConfig();
            // Collapse H2 onto H1 (both at 0) -> panel P1 becomes zero-height.
            config.Horizontals.First(h => h.Id == "H2").Elevation = 0.004;

            var warnings = FrameModelValidator.Validate(config, ShippedCatalog(), Tolerance);

            Assert.Contains(warnings, w => w.Contains("muy cercana"));
            Assert.Contains(warnings, w => w.Contains("altura insuficiente"));
        }

        [Fact]
        public void Validate_NonZeroBaseElevation_Warns()
        {
            var config = StandardConfig();
            config.Horizontals.First(h => h.Id == "H1").Elevation = 10.0;

            var warnings = FrameModelValidator.Validate(config, ShippedCatalog(), Tolerance);

            Assert.Contains(warnings, w => w.Contains("inferior deberia estar en 0"));
        }

        [Fact]
        public void Validate_UnknownCatalogId_Warns()
        {
            var config = StandardConfig();
            config.LeftPost.PostCatalogId = "POSTE_OMEGA_3x4_TYPO";

            var warnings = FrameModelValidator.Validate(config, ShippedCatalog(), Tolerance);

            Assert.Contains(warnings, w => w.Contains("desconocida en catalogo") && w.Contains("POSTE_OMEGA_3x4_TYPO"));
        }

        [Fact]
        public void Validate_EmptyCatalog_DoesNotProduceUnknownIdWarnings()
        {
            var config = StandardConfig();
            config.LeftPost.PostCatalogId = "CUALQUIER_COSA";

            var warnings = FrameModelValidator.Validate(config, new RackCatalog(), Tolerance);

            Assert.DoesNotContain(warnings, w => w.Contains("desconocida en catalogo"));
        }

        [Fact]
        public void Validate_UnknownReinforcementId_WarnsOnlyWhenReinforced()
        {
            var config = StandardConfig();
            config.LeftPost.HasReinforcement = true;
            config.LeftPost.ReinforcementCatalogId = "REFUERZO_INEXISTENTE";

            var warnings = FrameModelValidator.Validate(config, ShippedCatalog(), Tolerance);
            Assert.Contains(warnings, w => w.Contains("REFUERZO_INEXISTENTE"));

            // Same id but reinforcement disabled -> not validated.
            config.LeftPost.HasReinforcement = false;
            var noWarnings = FrameModelValidator.Validate(config, ShippedCatalog(), Tolerance);
            Assert.DoesNotContain(noWarnings, w => w.Contains("REFUERZO_INEXISTENTE"));
        }

        [Fact]
        public void Validate_NullConfiguration_ReturnsEmpty()
        {
            Assert.Empty(FrameModelValidator.Validate(null, ShippedCatalog(), Tolerance));
        }
    }
}
