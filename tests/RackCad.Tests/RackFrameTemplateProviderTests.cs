using System;
using System.IO;
using System.Linq;
using RackCad.Application.RackFrames;
using RackCad.Domain.RackFrames;
using Xunit;

namespace RackCad.Tests
{
    public class RackFrameTemplateProviderTests
    {
        [Fact]
        public void Load_ShippedTemplates_ParsesTemplatesIncludingStandard()
        {
            var templates = RackFrameTemplateProvider.FromBaseDirectory().Load();

            Assert.NotEmpty(templates);

            var standard = templates.FirstOrDefault(t =>
                string.Equals(t.Id, TestCatalogIds.Templates.Standard, StringComparison.OrdinalIgnoreCase));

            Assert.NotNull(standard);
            Assert.Equal(new[] { 0.0, 44.0, 88.0, 132.0 }, standard.Horizontals.Select(h => h.Elevation));
            // Horizontals are all the unified celosía/truss profile now.
            Assert.All(standard.Horizontals, h =>
                Assert.Equal(TestCatalogIds.Profiles.Truss.Standard, h.Profile));
            Assert.Equal(2, standard.Horizontals.First().Quantity);
            Assert.Equal(TestCatalogIds.Profiles.Posts.Standard, standard.Post);
            Assert.Equal(TestCatalogIds.Profiles.Truss.Standard, standard.DiagonalProfile);
            Assert.Equal(BracingPattern.SingleDiagonal, standard.DefaultArrangement);
        }

        [Fact]
        public void Load_ParsesArrangementEnumFromString()
        {
            var templates = RackFrameTemplateProvider.FromBaseDirectory().Load();

            var tall = templates.FirstOrDefault(t =>
                string.Equals(t.Id, TestCatalogIds.Templates.Tall, StringComparison.OrdinalIgnoreCase));

            Assert.NotNull(tall);
            Assert.Equal(BracingPattern.XBracing, tall.DefaultArrangement);
        }

        [Fact]
        public void Load_MissingFile_FallsBackToBuiltInTemplates()
        {
            var provider = new RackFrameTemplateProvider(Path.Combine(Path.GetTempPath(), "rackcad-no-templates-xyz"));

            var templates = provider.Load();

            Assert.Equal(RackFrameTemplateCatalog.All.Count, templates.Count);
            Assert.Contains(templates, t => string.Equals(
                t.Id,
                TestCatalogIds.Templates.Standard,
                StringComparison.OrdinalIgnoreCase));
        }

        [Fact]
        public void Load_CustomTemplatesFromDisk_AreUsedInsteadOfBuiltIn()
        {
            var directory = Path.Combine(Path.GetTempPath(), "rackcad-templates-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(directory);
            File.WriteAllText(Path.Combine(directory, RackFrameTemplateProvider.TemplatesFile),
                "[{\"id\":\"CUSTOM\",\"name\":\"Mi cabecera\",\"defaultHeight\":120,\"defaultDepth\":40," +
                "\"horizontals\":[{\"elevation\":0,\"profile\":\"HORIZONTAL_INFERIOR\",\"quantity\":2},{\"elevation\":60,\"profile\":\"HORIZONTAL_INTERMEDIA\",\"quantity\":1},{\"elevation\":120,\"profile\":\"HORIZONTAL_SUPERIOR\",\"quantity\":1}]," +
                "\"defaultArrangement\":\"DoubleDiagonal\",\"post\":\"POSTE_OMEGA_3X3\"}]");

            try
            {
                var templates = new RackFrameTemplateProvider(directory).Load();

                Assert.Single(templates);
                Assert.Equal("Mi cabecera", templates[0].Name);
                Assert.Equal(BracingPattern.DoubleDiagonal, templates[0].DefaultArrangement);
                Assert.Equal(3, templates[0].Horizontals.Count);
                Assert.Equal("HORIZONTAL_INFERIOR", templates[0].Horizontals[0].Profile);
            }
            finally
            {
                Directory.Delete(directory, recursive: true);
            }
        }

        [Fact]
        public void Load_InvalidJson_ThrowsDescriptiveError()
        {
            var directory = Path.Combine(Path.GetTempPath(), "rackcad-bad-templates-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(directory);
            File.WriteAllText(Path.Combine(directory, RackFrameTemplateProvider.TemplatesFile), "[{ not json");

            try
            {
                var error = Assert.Throws<InvalidOperationException>(() => new RackFrameTemplateProvider(directory).Load());
                Assert.Contains(RackFrameTemplateProvider.TemplatesFile, error.Message);
            }
            finally
            {
                Directory.Delete(directory, recursive: true);
            }
        }
    }
}
