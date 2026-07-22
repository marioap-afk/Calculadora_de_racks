using System;
using System.IO;
using System.Linq;
using RackCad.Application.Persistence;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>
    /// I-03 (D2): the shared atomic write (temp + File.Replace/Move). A store save must not be able to
    /// leave a half-written file that destroys the previous good copy, and must not leave temp siblings.
    /// </summary>
    public class AtomicFileTests
    {
        private static string TempDir()
        {
            var dir = Path.Combine(Path.GetTempPath(), "rackcad-atomic-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(dir);
            return dir;
        }

        [Fact]
        public void WriteAllText_CreatesFile_WhenMissing()
        {
            var dir = TempDir();
            try
            {
                var path = Path.Combine(dir, "new.json");
                AtomicFile.WriteAllText(path, "hello-nuevo");
                Assert.True(File.Exists(path));
                Assert.Equal("hello-nuevo", File.ReadAllText(path));
            }
            finally
            {
                Directory.Delete(dir, recursive: true);
            }
        }

        [Fact]
        public void WriteAllText_Overwrites_WhenExists()
        {
            var dir = TempDir();
            try
            {
                var path = Path.Combine(dir, "existing.json");
                File.WriteAllText(path, "viejo-contenido");
                AtomicFile.WriteAllText(path, "contenido-nuevo");
                Assert.Equal("contenido-nuevo", File.ReadAllText(path));
            }
            finally
            {
                Directory.Delete(dir, recursive: true);
            }
        }

        [Fact]
        public void WriteAllText_LeavesNoTempSibling()
        {
            var dir = TempDir();
            try
            {
                var path = Path.Combine(dir, "clean.json");
                AtomicFile.WriteAllText(path, "a");
                AtomicFile.WriteAllText(path, "b"); // twice: create then replace

                var files = Directory.EnumerateFiles(dir).Select(Path.GetFileName).ToList();
                Assert.Equal(new[] { "clean.json" }, files);
            }
            finally
            {
                Directory.Delete(dir, recursive: true);
            }
        }

        [Fact]
        public void WriteAllText_ThrowsWhenDirectoryMissing_PreservingStorePrecondition()
        {
            // AtomicFile deliberately does NOT create the target directory: the stores that never created it
            // must keep failing the same way (a missing folder is a real error, not silently created).
            var missingDir = Path.Combine(Path.GetTempPath(), "rackcad-missing-" + Guid.NewGuid().ToString("N"));
            var path = Path.Combine(missingDir, "x.json");
            Assert.Throws<DirectoryNotFoundException>(() => AtomicFile.WriteAllText(path, "z"));
            Assert.False(Directory.Exists(missingDir));
        }
    }
}
