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
                string.Equals(t.Id, "STD-3P", StringComparison.OrdinalIgnoreCase));

            Assert.NotNull(standard);
            Assert.Equal(new[] { 0.0, 44.0, 88.0, 132.0 }, standard.HorizontalElevations);
            Assert.Equal(BracingPattern.SingleDiagonal, standard.DefaultArrangement);
        }

        [Fact]
        public void Load_ParsesArrangementEnumFromString()
        {
            var templates = RackFrameTemplateProvider.FromBaseDirectory().Load();

            var tall = templates.FirstOrDefault(t =>
                string.Equals(t.Id, "TALL-4P", StringComparison.OrdinalIgnoreCase));

            Assert.NotNull(tall);
            Assert.Equal(BracingPattern.XBracing, tall.DefaultArrangement);
        }

        [Fact]
        public void Load_MissingFile_FallsBackToBuiltInTemplates()
        {
            var provider = new RackFrameTemplateProvider(Path.Combine(Path.GetTempPath(), "rackcad-no-templates-xyz"));

            var templates = provider.Load();

            Assert.Equal(RackFrameTemplateCatalog.All.Count, templates.Count);
            Assert.Contains(templates, t => string.Equals(t.Id, "STD-3P", StringComparison.OrdinalIgnoreCase));
        }

        [Fact]
        public void Load_CustomTemplatesFromDisk_AreUsedInsteadOfBuiltIn()
        {
            var directory = Path.Combine(Path.GetTempPath(), "rackcad-templates-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(directory);
            File.WriteAllText(Path.Combine(directory, RackFrameTemplateProvider.TemplatesFile),
                "[{\"id\":\"CUSTOM\",\"name\":\"Mi cabecera\",\"defaultHeight\":120,\"defaultDepth\":40,\"horizontalElevations\":[0,60,120],\"defaultArrangement\":\"DoubleDiagonal\"}]");

            try
            {
                var templates = new RackFrameTemplateProvider(directory).Load();

                Assert.Single(templates);
                Assert.Equal("Mi cabecera", templates[0].Name);
                Assert.Equal(BracingPattern.DoubleDiagonal, templates[0].DefaultArrangement);
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
