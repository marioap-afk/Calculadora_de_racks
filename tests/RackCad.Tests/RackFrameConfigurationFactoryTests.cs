using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Catalogs;
using RackCad.Application.RackFrames;
using RackCad.Domain.RackFrames;
using Xunit;

namespace RackCad.Tests
{
    public class RackFrameConfigurationFactoryTests
    {
        private static RackFrameConfigurationFactory CreateFactory(RackCatalog catalog = null)
        {
            return new RackFrameConfigurationFactory(catalog ?? new RackCatalog());
        }

        [Fact]
        public void Build_StandardTemplateAtDefaultDimensions_UsesParametricCelosiaWithClosings()
        {
            var template = RackFrameTemplateCatalog.FindById(TestCatalogIds.Templates.Standard);

            var configuration = CreateFactory().Build(template, "POSTE_OMEGA_3X3", 132.0, 42.0);

            // First travesaño at troquel 3 (4"), panels of 44", then two closing travesaños (110, 128).
            Assert.Equal(new[] { "H1", "H2", "H3", "H4", "H5" }, configuration.Horizontals.Select(h => h.Id));
            Assert.Equal(new[] { 4.0, 48.0, 92.0, 110.0, 128.0 }, configuration.Horizontals.Select(h => h.Elevation));
            Assert.Equal(4, configuration.BracingPanels.Count);
            Assert.Equal(2, configuration.BracingPanels.Count(p => p.Arrangement == BracingPattern.SingleDiagonal));
            Assert.Equal(2, configuration.BracingPanels.Count(p => p.Arrangement == BracingPattern.NoBracing));
            Assert.Equal(132.0, configuration.Height);
            Assert.Equal(42.0, configuration.Depth);
        }

        [Fact]
        public void Build_FirstTravesanoAtTroquel_PanelsEvery44_RegardlessOfHeight()
        {
            var template = RackFrameTemplateCatalog.FindById(TestCatalogIds.Templates.Standard);

            var configuration = CreateFactory().Build(template, "POSTE_OMEGA_3X3", 300.0, 48.0);
            var elevations = configuration.Horizontals.Select(h => h.Elevation).OrderBy(e => e).ToList();

            Assert.Equal(4.0, elevations[0], 4);           // first travesaño on the start troquel
            Assert.Equal(44.0, elevations[1] - elevations[0], 4); // standard panels are 44" apart
            Assert.True(elevations.Last() < 300.0);        // closings clear the post top
            Assert.Equal(48.0, configuration.Depth);
        }

        [Fact]
        public void Build_AppliesChosenPostToBothSides_WithCatalogDescription()
        {
            var catalog = new RackCatalog
            {
                PostProfiles = new List<ProfileCatalogEntry>
                {
                    new ProfileCatalogEntry { Id = "POSTE_REFORZADO", Description = "Poste reforzado 4x4" }
                }
            };

            var configuration = CreateFactory(catalog)
                .Build(RackFrameTemplateCatalog.Default, "POSTE_REFORZADO", 132.0, 42.0);

            Assert.Equal("POSTE_REFORZADO", configuration.LeftPost.PostCatalogId);
            Assert.Equal("POSTE_REFORZADO", configuration.RightPost.PostCatalogId);
            Assert.Equal("Poste reforzado 4x4", configuration.LeftPost.Description);
        }

        [Fact]
        public void Build_TallTemplate_ProducesFiveHorizontalsAndFourXPanels()
        {
            var template = RackFrameTemplateCatalog.FindById(TestCatalogIds.Templates.Tall);

            // 184 = 4 (start troquel) + 4*44 (four 44" panels) + 4 (remate): the top horizontal lands at 180, so the
            // five travesaños are evenly spaced with no closing.
            var configuration = CreateFactory().Build(template, "POSTE_OMEGA_3X3", 184.0, 42.0);

            Assert.Equal(new[] { 4.0, 48.0, 92.0, 136.0, 180.0 }, configuration.Horizontals.Select(h => h.Elevation));
            Assert.Equal(4, configuration.BracingPanels.Count);
            Assert.All(configuration.BracingPanels, p => Assert.Equal(BracingPattern.XBracing, p.Arrangement));
        }

        [Fact]
        public void Build_TopHorizontalLandsAtHeightMinusRemate_SoBuiltHeightEqualsRequested()
        {
            var template = RackFrameTemplateCatalog.FindById(TestCatalogIds.Templates.Standard);

            // Requesting 240 used to build a 242" frame (the top horizontal drifted a troquel high). Now the top
            // horizontal lands at 240 - PostTopRemate = 236, so the built height is exactly 240.
            var configuration = CreateFactory().Build(template, "POSTE_OMEGA_3X3", 240.0, 48.0);

            var top = configuration.Horizontals.Max(h => h.Elevation);
            Assert.Equal(240.0 - RackFrameConfigurationFactory.PostTopRemate, top, 4);
            Assert.Equal(240.0, top + RackFrameConfigurationFactory.PostTopRemate, 4);
            Assert.Equal(240.0, configuration.Height);
        }

        [Theory]
        [InlineData(0.0, 42.0)]
        [InlineData(132.0, 0.0)]
        [InlineData(-10.0, 42.0)]
        public void Build_RejectsNonPositiveDimensions(double height, double depth)
        {
            Assert.ThrowsAny<ArgumentException>(() =>
                CreateFactory().Build(RackFrameTemplateCatalog.Default, "POSTE_OMEGA_3X3", height, depth));
        }

        [Fact]
        public void Build_AllBuiltInTemplates_StillBuild()
        {
            foreach (var template in RackFrameTemplateCatalog.All)
            {
                var configuration = CreateFactory().Build(template, "POSTE_OMEGA_3X3", template.DefaultHeight, template.DefaultDepth);

                // Horizontals are now computed parametrically (start troquel + 44" panels + closings), so the
                // count is no longer the template's; just assert a usable celosía was produced.
                Assert.True(configuration.Horizontals.Count >= 2);
                Assert.True(configuration.BracingPanels.Count >= 1);
            }
        }

        [Fact]
        public void Build_NullPost_FallsBackToDefaultPostId()
        {
            var configuration = CreateFactory().Build(RackFrameTemplateCatalog.Default, null, 132.0, 42.0);

            Assert.Equal(TestCatalogIds.Profiles.Posts.Standard, configuration.LeftPost.PostCatalogId);
        }

        [Fact]
        public void TemplateCatalog_ExposesSeveralTemplatesIncludingDefault()
        {
            Assert.True(RackFrameTemplateCatalog.All.Count >= 2);
            Assert.NotNull(RackFrameTemplateCatalog.Default);
            Assert.All(RackFrameTemplateCatalog.All, t =>
            {
                Assert.NotNull(t.Horizontals);
                Assert.Equal(0.0, t.Horizontals.First().Elevation);
                Assert.Equal(t.DefaultHeight, t.Horizontals.Last().Elevation);
                Assert.All(t.Horizontals, h => Assert.False(string.IsNullOrWhiteSpace(h.Profile)));
            });
        }

    }
}
