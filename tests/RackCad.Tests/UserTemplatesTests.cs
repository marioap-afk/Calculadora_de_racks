using System;
using System.IO;
using System.Linq;
using RackCad.Application.RackFrames;
using RackCad.Domain.RackFrames;
using Xunit;

namespace RackCad.Tests
{
    public class UserTemplatesTests
    {
        [Fact]
        public void FromConfiguration_CapturesPostPlateDiagonalProfileArrangementAndDimensions()
        {
            var template = RackFrameTemplateCatalog.FindStandardOrDefault();
            var config = new RackFrameConfigurationFactory().Build(template, CatalogIds.StandardPost, 132.0, 42.0);

            var result = RackFrameTemplateFactory.FromConfiguration(config, "USER-TEST", "Mi plantilla");

            // Id/Name come from the caller; dimensions and pieces are the inverse of the Build factory.
            Assert.Equal("USER-TEST", result.Id);
            Assert.Equal("Mi plantilla", result.Name);
            Assert.Equal(config.Height, result.DefaultHeight);
            Assert.Equal(config.Depth, result.DefaultDepth);
            Assert.Equal(config.LeftPost.PostCatalogId, result.Post);
            Assert.Equal(config.LeftBasePlate.PlateCatalogId, result.BasePlate);

            var firstStandardPanel = config.BracingPanels.First(panel => panel.IsStandard);
            Assert.Equal(firstStandardPanel.Arrangement, result.DefaultArrangement);
            Assert.Equal(firstStandardPanel.DiagonalProfileId, result.DiagonalProfile);
            Assert.Equal(firstStandardPanel.StartConnectionPointId, result.BraceStartConnectionPoint);
            Assert.Equal(firstStandardPanel.EndConnectionPointId, result.BraceEndConnectionPoint);

            Assert.NotEmpty(result.Horizontals);
            Assert.Equal(config.Horizontals[0].ProfileId, result.Horizontals[0].Profile);
            Assert.Equal(config.Horizontals[0].Quantity, result.Horizontals[0].Quantity);
        }

        [Fact]
        public void FromConfiguration_NullConfig_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => RackFrameTemplateFactory.FromConfiguration(null, "ID", "Name"));
        }

        [Fact]
        public void UserTemplateStore_SaveThenLoad_RoundTrips()
        {
            var directory = Path.Combine(Path.GetTempPath(), "rackcad-user-templates-" + Guid.NewGuid().ToString("N"));
            var path = Path.Combine(directory, UserTemplateStore.TemplatesFile);

            try
            {
                var store = new UserTemplateStore(path);
                var template = new RackFrameTemplate
                {
                    Id = "USER-1",
                    Name = "Cabecera personalizada",
                    DefaultHeight = 120.0,
                    DefaultDepth = 40.0,
                    DefaultArrangement = BracingPattern.XBracing,
                    DiagonalProfile = "DIAG",
                    Post = "POST",
                    BasePlate = "PLATE",
                    Horizontals = new[]
                    {
                        new TemplateHorizontal { Elevation = 0.0, Profile = "HZ", Quantity = 2 }
                    }
                };

                store.Save(template);

                var loaded = store.Load();

                Assert.Single(loaded);
                Assert.Equal("USER-1", loaded[0].Id);
                Assert.Equal("Cabecera personalizada", loaded[0].Name);
                Assert.Equal(120.0, loaded[0].DefaultHeight);
                Assert.Equal(40.0, loaded[0].DefaultDepth);
                // The enum survives the round-trip via JsonStringEnumConverter.
                Assert.Equal(BracingPattern.XBracing, loaded[0].DefaultArrangement);
                Assert.Equal("POST", loaded[0].Post);
                Assert.Equal(2, loaded[0].Horizontals[0].Quantity);
                Assert.Equal("HZ", loaded[0].Horizontals[0].Profile);
            }
            finally
            {
                if (Directory.Exists(directory))
                {
                    Directory.Delete(directory, recursive: true);
                }
            }
        }

        [Fact]
        public void UserTemplateStore_SaveSameId_UpsertsSingleEntry()
        {
            var directory = Path.Combine(Path.GetTempPath(), "rackcad-user-templates-" + Guid.NewGuid().ToString("N"));
            var path = Path.Combine(directory, UserTemplateStore.TemplatesFile);

            try
            {
                var store = new UserTemplateStore(path);
                store.Save(new RackFrameTemplate { Id = "USER-9", Name = "Primera" });
                store.Save(new RackFrameTemplate { Id = "USER-9", Name = "Segunda" });

                var loaded = store.Load();

                Assert.Single(loaded);
                Assert.Equal("Segunda", loaded[0].Name);

                // A different id adds a second entry (upsert only collapses matching ids).
                store.Save(new RackFrameTemplate { Id = "USER-10", Name = "Otra" });
                Assert.Equal(2, store.Load().Count);
            }
            finally
            {
                if (Directory.Exists(directory))
                {
                    Directory.Delete(directory, recursive: true);
                }
            }
        }

        [Fact]
        public void UserTemplateStore_LoadMissingFile_ReturnsEmpty()
        {
            var path = Path.Combine(
                Path.GetTempPath(), "rackcad-no-user-templates-" + Guid.NewGuid().ToString("N"), UserTemplateStore.TemplatesFile);

            var loaded = new UserTemplateStore(path).Load();

            Assert.Empty(loaded);
        }
    }
}
