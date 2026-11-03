using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using RackCad.Application.StructuralAnalysis;
using Xunit;
using W = RackCad.Application.StructuralAnalysis.StructuralPackageWriteCodes;

namespace RackCad.Tests
{
    /// <summary>
    /// I-65 F4-T1 — A65-F3-09: the race of the destination. The writer checks that the destination does not exist and, later, renames the
    /// temporary folder onto it. If something creates the destination in between, the rename fails (as the real system does, which never
    /// overwrites); the result must be a failure, the folder that appeared must be left exactly as it was, and no temporary folder may remain.
    /// The race is injected through the file-system seam: the checks of existence answer "no" and the destination appears right before the rename.
    /// </summary>
    public class StructuralAnalysisRamElementsHardeningRaceTests
    {
        private static readonly string Parent = Path.Combine(Path.GetTempPath(), "rackcad-hardening-race");

        private static string Destination => Path.Combine(Parent, "package");

        private static List<StructuralPackageFile> Files() => new List<StructuralPackageFile>
        {
            new StructuralPackageFile("manifest.json", new byte[] { 1 }),
            new StructuralPackageFile("script.txt", new byte[] { 2 }),
            new StructuralPackageFile("loads.json", new byte[] { 3, 0, 255 }),
        };

        /// <summary>An in-memory file system whose destination appears between the checks of the writer and the rename.</summary>
        private sealed class RacingFileSystem : IStructuralPackageFileSystem
        {
            private readonly HashSet<string> _directories = new HashSet<string>(StringComparer.Ordinal) { Parent };
            private readonly Dictionary<string, byte[]> _files = new Dictionary<string, byte[]>(StringComparer.Ordinal);
            private readonly bool _appearsAsFile;

            public RacingFileSystem(bool appearsAsFile)
            {
                _appearsAsFile = appearsAsFile;
            }

            public bool RaceInjected { get; private set; }

            public bool FailCleanup { get; set; }

            public IReadOnlyCollection<string> Directories => _directories;

            public IReadOnlyDictionary<string, byte[]> AllFiles => _files;

            public bool DirectoryExists(string path) => _directories.Contains(path);

            public bool FileExists(string path) => _files.ContainsKey(path);

            public void CreateDirectory(string path) => _directories.Add(path);

            public void CreateFile(string path, byte[] content)
            {
                if (_files.ContainsKey(path))
                {
                    throw new IOException("exists");
                }

                _files.Add(path, content);
            }

            public void MoveDirectory(string source, string destination)
            {
                if (!RaceInjected)
                {
                    RaceInjected = true;

                    if (_appearsAsFile)
                    {
                        _files.Add(destination, new byte[] { 9 });
                    }
                    else
                    {
                        _directories.Add(destination);
                        _files.Add(Path.Combine(destination, "keep.txt"), new byte[] { 9 });
                    }
                }

                if (_directories.Contains(destination) || _files.ContainsKey(destination) || !_directories.Contains(source))
                {
                    throw new IOException("cannot move");
                }

                _directories.Remove(source);
                _directories.Add(destination);

                foreach (var pair in _files.Where(f => f.Key.StartsWith(source + Path.DirectorySeparatorChar, StringComparison.Ordinal)).ToList())
                {
                    _files.Remove(pair.Key);
                    _files.Add(destination + pair.Key.Substring(source.Length), pair.Value);
                }
            }

            public void DeleteDirectory(string path)
            {
                if (FailCleanup)
                {
                    throw new IOException("cleanup");
                }

                _directories.Remove(path);

                foreach (var key in _files.Keys.Where(k => k.StartsWith(path + Path.DirectorySeparatorChar, StringComparison.Ordinal)).ToList())
                {
                    _files.Remove(key);
                }
            }
        }

        [Fact]
        public void SiElDestinoApareceComoCarpetaEntreLaComprobacionYElRenombradoElResultadoEsUnFalloYLoPreexistenteQuedaIntacto()
        {
            var fake = new RacingFileSystem(appearsAsFile: false);

            var result = new StructuralPackagePublisher(fake).Write(Destination, Files());

            Assert.True(fake.RaceInjected, "La carrera no se produjo.");
            Assert.False(result.Succeeded);
            Assert.Equal(W.PublishFailed, result.Code);
            Assert.False(result.CleanupFailed);
            Assert.Equal(new byte[] { 9 }, fake.AllFiles[Path.Combine(Destination, "keep.txt")]);
            Assert.Single(fake.AllFiles);
        }

        [Fact]
        public void SiElDestinoApareceComoArchivoEntreLaComprobacionYElRenombradoElResultadoEsUnFalloYLoPreexistenteQuedaIntacto()
        {
            var fake = new RacingFileSystem(appearsAsFile: true);

            var result = new StructuralPackagePublisher(fake).Write(Destination, Files());

            Assert.True(fake.RaceInjected, "La carrera no se produjo.");
            Assert.False(result.Succeeded);
            Assert.Equal(W.PublishFailed, result.Code);
            Assert.Equal(new byte[] { 9 }, fake.AllFiles[Destination]);
            Assert.Single(fake.AllFiles);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void TrasLaCarreraNoQuedaCarpetaTemporalNiArchivosSueltos(bool appearsAsFile)
        {
            var fake = new RacingFileSystem(appearsAsFile);

            new StructuralPackagePublisher(fake).Write(Destination, Files());

            Assert.Equal(new[] { Parent }, fake.Directories.Where(d => d != Destination).ToArray());
            Assert.DoesNotContain(fake.AllFiles.Keys, k => k.Contains(".tmp-", StringComparison.Ordinal));
        }

        [Fact]
        public void SiAdemasFallaLaLimpiezaSeDiceYLoPreexistenteSigueIntacto()
        {
            var fake = new RacingFileSystem(appearsAsFile: false) { FailCleanup = true };

            var result = new StructuralPackagePublisher(fake).Write(Destination, Files());

            Assert.False(result.Succeeded);
            Assert.Equal(W.PublishFailed, result.Code);
            Assert.True(result.CleanupFailed);
            Assert.Equal(new byte[] { 9 }, fake.AllFiles[Path.Combine(Destination, "keep.txt")]);
        }

        // ---------------------------------------------------------------- the real disk

        private sealed class RacingDiskFileSystem : IStructuralPackageFileSystem
        {
            private readonly StructuralPackageDiskFileSystem _disk = new StructuralPackageDiskFileSystem();

            public bool DirectoryExists(string path) => _disk.DirectoryExists(path);

            public bool FileExists(string path) => _disk.FileExists(path);

            public void CreateDirectory(string path) => _disk.CreateDirectory(path);

            public void CreateFile(string path, byte[] content) => _disk.CreateFile(path, content);

            public void MoveDirectory(string source, string destination)
            {
                Directory.CreateDirectory(destination);
                File.WriteAllText(Path.Combine(destination, "keep.txt"), "keep");
                _disk.MoveDirectory(source, destination);
            }

            public void DeleteDirectory(string path) => _disk.DeleteDirectory(path);
        }

        [Fact]
        public void EnDiscoSiElDestinoApareceAntesDelRenombradoFallaConservaLoPreexistenteYNoDejaTemporales()
        {
            var parent = Path.Combine(Path.GetTempPath(), "rackcad-hardening-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(parent);

            try
            {
                var destination = Path.Combine(parent, "package");

                var result = new StructuralPackagePublisher(new RacingDiskFileSystem()).Write(destination, Files());

                Assert.False(result.Succeeded);
                Assert.Equal(W.PublishFailed, result.Code);
                Assert.False(result.CleanupFailed);
                Assert.Equal(new[] { destination }, Directory.GetFileSystemEntries(parent));
                Assert.Equal(new[] { Path.Combine(destination, "keep.txt") }, Directory.GetFileSystemEntries(destination));
                Assert.Equal("keep", File.ReadAllText(Path.Combine(destination, "keep.txt")));
            }
            finally
            {
                Directory.Delete(parent, true);
            }
        }
    }
}
