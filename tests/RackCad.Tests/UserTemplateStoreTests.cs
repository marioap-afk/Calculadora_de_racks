using System;
using System.IO;
using System.Linq;
using RackCad.Application.RackFrames;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>
    /// I-03 (D2): the per-user template library. A missing file is empty (normal); a corrupt file is
    /// quarantined to <c>.bad</c> and logged instead of silently discarded, and the save is atomic.
    /// <see cref="UserTemplateStore"/> already takes its path in the constructor, so no seam is needed.
    /// The corrupt-file test redirects the log sink (RackLog collection) so logging never hits %APPDATA%.
    /// </summary>
    [Collection("RackLog")]
    public class UserTemplateStoreTests
    {
        private static string TempPath()
            => Path.Combine(Path.GetTempPath(), "rackcad-templates-" + Guid.NewGuid().ToString("N") + ".json");

        [Fact]
        public void Load_MissingFile_ReturnsEmpty_WithoutQuarantine()
        {
            var path = TempPath();
            var store = new UserTemplateStore(path);

            Assert.Empty(store.Load());
            Assert.False(File.Exists(path + ".bad"));
        }

        [Fact]
        public void Load_CorruptFile_QuarantinesToBad_AndReturnsEmpty()
        {
            var path = TempPath();
            File.WriteAllText(path, "}} definitely not a template array {{");
            using var cap = LogCapture.Begin(); // the corrupt load logs; keep it off the real %AppData%
            try
            {
                var store = new UserTemplateStore(path);

                Assert.Empty(store.Load());
                Assert.False(File.Exists(path));         // moved aside, not silently overwritten
                Assert.True(File.Exists(path + ".bad")); // preserved for diagnosis
                Assert.Contains("not a template array", File.ReadAllText(path + ".bad"));
                Assert.Contains("UserTemplateStore load", cap.Text); // and the discard was recorded, not silent
            }
            finally
            {
                foreach (var p in new[] { path, path + ".bad" })
                {
                    if (File.Exists(p)) File.Delete(p);
                }
            }
        }

        [Fact]
        public void SaveThenLoad_RoundTripsTemplate()
        {
            var path = TempPath();
            try
            {
                var store = new UserTemplateStore(path);
                store.Save(new RackFrameTemplate { Id = "T-1", Name = "Prueba", DefaultHeight = 120, DefaultDepth = 42 });

                var loaded = store.Load();
                Assert.Single(loaded);
                Assert.Equal("T-1", loaded[0].Id);
                Assert.Equal("Prueba", loaded[0].Name);
            }
            finally
            {
                if (File.Exists(path)) File.Delete(path);
            }
        }

        [Fact]
        public void Save_IsAtomic_NoTempSiblingLeft()
        {
            var dir = Path.Combine(Path.GetTempPath(), "rackcad-templates-dir-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(dir);
            try
            {
                var path = Path.Combine(dir, "user-templates.json");
                var store = new UserTemplateStore(path);
                store.Save(new RackFrameTemplate { Id = "T-1", Name = "A" });
                store.Save(new RackFrameTemplate { Id = "T-2", Name = "B" });

                var files = Directory.EnumerateFiles(dir).Select(Path.GetFileName).ToList();
                Assert.Equal(new[] { "user-templates.json" }, files);
            }
            finally
            {
                Directory.Delete(dir, recursive: true);
            }
        }
    }
}
