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
        public void Build_StandardTemplateAtDefaultDimensions_MatchesLegacyStandard()
        {
            var template = RackFrameTemplateCatalog.FindById("STD-3P");

            var configuration = CreateFactory().Build(template, "POSTE_OMEGA_3X3", 132.0, 42.0);

            Assert.Equal(new[] { "H1", "H2", "H3", "H4" }, configuration.Horizontals.Select(h => h.Id));
            Assert.Equal(new[] { 0.0, 44.0, 88.0, 132.0 }, configuration.Horizontals.Select(h => h.Elevation));
            Assert.Equal(3, configuration.BracingPanels.Count);
            Assert.All(configuration.BracingPanels, p => Assert.Equal(BracingPattern.SingleDiagonal, p.Arrangement));
            Assert.Equal(132.0, configuration.Height);
            Assert.Equal(42.0, configuration.Depth);
        }

        [Fact]
        public void Build_ScalesHorizontalElevationsWithHeight()
        {
            var template = RackFrameTemplateCatalog.FindById("STD-3P");

            var configuration = CreateFactory().Build(template, "POSTE_OMEGA_3X3", 300.0, 48.0);

            Assert.Equal(new[] { 0.0, 100.0, 200.0, 300.0 }, configuration.Horizontals.Select(h => h.Elevation));
            Assert.Equal(300.0, configuration.Horizontals.Max(h => h.Elevation));
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
            var template = RackFrameTemplateCatalog.FindById("TALL-4P");

            var configuration = CreateFactory().Build(template, "POSTE_OMEGA_3X3", 180.0, 42.0);

            Assert.Equal(5, configuration.Horizontals.Count);
            Assert.Equal(4, configuration.BracingPanels.Count);
            Assert.All(configuration.BracingPanels, p => Assert.Equal(BracingPattern.XBracing, p.Arrangement));
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
        public void Build_DescendingOrDuplicateTemplateElevations_Throws()
        {
            var descending = new RackFrameTemplate
            {
                Id = "BAD-DESC",
                Name = "Mala (descendente)",
                DefaultHeight = 132.0,
                DefaultDepth = 42.0,
                Horizontals = Horizontals(0.0, 132.0, 44.0)
            };
            var duplicate = new RackFrameTemplate
            {
                Id = "BAD-DUP",
                Name = "Mala (duplicada)",
                DefaultHeight = 132.0,
                DefaultDepth = 42.0,
                Horizontals = Horizontals(0.0, 44.0, 44.0, 132.0)
            };

            Assert.Throws<System.ArgumentException>(() => CreateFactory().Build(descending, "POSTE_OMEGA_3X3", 132.0, 42.0));
            Assert.Throws<System.ArgumentException>(() => CreateFactory().Build(duplicate, "POSTE_OMEGA_3X3", 132.0, 42.0));
        }

        [Fact]
        public void Build_AllBuiltInTemplates_StillBuild()
        {
            foreach (var template in RackFrameTemplateCatalog.All)
            {
                var configuration = CreateFactory().Build(template, "POSTE_OMEGA_3X3", template.DefaultHeight, template.DefaultDepth);
                Assert.Equal(template.Horizontals.Count, configuration.Horizontals.Count);
            }
        }

        [Fact]
        public void Build_NullPost_FallsBackToDefaultPostId()
        {
            var configuration = CreateFactory().Build(RackFrameTemplateCatalog.Default, null, 132.0, 42.0);

            Assert.Equal(CatalogIds.StandardPost, configuration.LeftPost.PostCatalogId);
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

        private static System.Collections.Generic.List<TemplateHorizontal> Horizontals(params double[] elevations)
        {
            return elevations
                .Select(e => new TemplateHorizontal { Elevation = e, Profile = "HORIZONTAL_INTERMEDIA", Quantity = 1 })
                .ToList();
        }
    }
}
