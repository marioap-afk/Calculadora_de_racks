using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using RackCad.Application.StructuralAnalysis;
using Xunit;
using C = RackCad.Tests.StructuralAnalysisCantileverFixtures;
using I = RackCad.Tests.StructuralAnalysisRamElementsPackageInputs;
using W = RackCad.Application.StructuralAnalysis.StructuralPackageWriteCodes;

namespace RackCad.Tests
{
    /// <summary>
    /// I-65 F3-T2 — D-08 and INV-05: the neutral writer. It publishes a NEW folder with ONE rename of a sibling temporary folder, never
    /// overwrites, and whatever fails (the writing of any file, the rename) leaves neither the destination nor the temporary folder.
    /// The failures are injected through the file-system seam; one test also proves that the harness catches a writer that is not atomic.
    /// </summary>
    public class StructuralAnalysisRamElementsPackageWriterTests
    {
        private sealed class InjectedFailure : Exception
        {
            public InjectedFailure(string message)
                : base(message)
            {
            }
        }

        /// <summary>An in-memory file system that records every operation and fails the ones the test asks for.</summary>
        private sealed class FakeFileSystem : IStructuralPackageFileSystem
        {
            public readonly HashSet<string> Directories = new HashSet<string>(StringComparer.Ordinal);
            public readonly Dictionary<string, byte[]> Files = new Dictionary<string, byte[]>(StringComparer.Ordinal);
            public readonly List<string> Operations = new List<string>();
            public Func<string, string, bool> Fail = (operation, path) => false;

            public bool DirectoryExists(string path) => Directories.Contains(path);

            public bool FileExists(string path) => Files.ContainsKey(path);

            public void CreateDirectory(string path)
            {
                Directories.Add(path);
                Record("CreateDirectory", path);
            }

            public void CreateFile(string path, byte[] content)
            {
                Operations.Add("CreateFile " + path);

                if (Files.ContainsKey(path))
                {
                    throw new IOException("exists");
                }

                Files.Add(path, content);
                Record("CreateFile", path, false);
            }

            public void MoveDirectory(string source, string destination)
            {
                Record("MoveDirectory", destination);

                if (Directories.Contains(destination) || !Directories.Contains(source))
                {
                    throw new IOException("cannot move");
                }

                Directories.Remove(source);
                Directories.Add(destination);

                foreach (var pair in Files.Where(f => IsUnder(f.Key, source)).ToList())
                {
                    Files.Remove(pair.Key);
                    Files.Add(destination + pair.Key.Substring(source.Length), pair.Value);
                }
            }

            public void DeleteDirectory(string path)
            {
                Record("DeleteDirectory", path);
                Directories.Remove(path);

                foreach (var key in Files.Keys.Where(k => IsUnder(k, path)).ToList())
                {
                    Files.Remove(key);
                }
            }

            public bool Holds(string folder) =>
                Directories.Contains(folder) || Files.Keys.Any(k => IsUnder(k, folder));

            private void Record(string operation, string path, bool log = true)
            {
                if (log)
                {
                    Operations.Add(operation + " " + path);
                }


                if (Fail(operation, path))
                {
                    throw new InjectedFailure(operation);
                }
            }

            private static bool IsUnder(string path, string folder) =>
                path.StartsWith(folder + Path.DirectorySeparatorChar, StringComparison.Ordinal);
        }

        private static readonly string Parent = Path.Combine(Path.GetTempPath(), "rackcad-package-writer-tests");

        private static string Destination(string name = "package") => Path.Combine(Parent, name);

        private static FakeFileSystem Fake()
        {
            var fake = new FakeFileSystem();
            fake.Directories.Add(Parent);
            return fake;
        }

        private static List<StructuralPackageFile> Files() => new List<StructuralPackageFile>
        {
            new StructuralPackageFile("manifest.json", Encoding.UTF8.GetBytes("{}\n")),
            new StructuralPackageFile("rackcad-ram-build.py", Encoding.ASCII.GetBytes("pass\n")),
            new StructuralPackageFile("structural-loads.json", new byte[] { 1, 2, 3, 0, 255 }),
        };

        /// <summary>What a failed write left behind, in the language of the harness: nothing at the destination and nothing as a sibling.</summary>
        private static List<string> Leftovers(FakeFileSystem fake, string destination, bool destinationAllowed = false)
        {
            var left = new List<string>();

            if (!destinationAllowed && fake.Holds(destination))
            {
                left.Add("queda la carpeta de destino");
            }

            left.AddRange(fake.Directories.Where(d => d != Parent && d != destination).Select(d => "queda la carpeta " + d));
            left.AddRange(fake.Files.Keys.Where(f => !f.StartsWith(destination + Path.DirectorySeparatorChar, StringComparison.Ordinal)).Select(f => "queda el archivo " + f));
            return left;
        }

        // ---------------------------------------------------------------- the write that works

        [Fact]
        public void EscribeLaCarpetaNuevaConSusArchivosConUnUnicoRenombrado()
        {
            var fake = Fake();
            var result = new StructuralPackagePublisher(fake).Write(Destination(), Files());

            Assert.True(result.Succeeded);
            Assert.Null(result.Code);
            Assert.True(fake.Directories.Contains(Destination()));
            Assert.Equal(3, fake.Files.Count);
            Assert.Equal(1, fake.Operations.Count(o => o.StartsWith("MoveDirectory ", StringComparison.Ordinal)));
            Assert.Empty(Leftovers(fake, Destination(), destinationAllowed: true));
        }

        [Fact]
        public void LosArchivosSeEscribenEnUnaCarpetaTemporalHermanaYElRenombradoVieneAlFinal()
        {
            var fake = Fake();
            new StructuralPackagePublisher(fake).Write(Destination(), Files());

            var created = fake.Operations.Where(o => o.StartsWith("CreateFile ", StringComparison.Ordinal)).Select(o => o.Substring("CreateFile ".Length)).ToList();
            var temporary = Path.GetDirectoryName(created[0]);

            Assert.Equal(3, created.Count);
            Assert.All(created, path => Assert.Equal(temporary, Path.GetDirectoryName(path)));
            Assert.Equal(Parent, Path.GetDirectoryName(temporary));
            Assert.NotEqual(Destination(), temporary);
            Assert.StartsWith("package", Path.GetFileName(temporary));
            Assert.StartsWith("MoveDirectory", fake.Operations.Last());
            Assert.DoesNotContain(fake.Operations, o => o.StartsWith("CreateFile " + Destination() + Path.DirectorySeparatorChar, StringComparison.Ordinal));
        }

        [Fact]
        public void ElContenidoPublicadoEsElDeLosBytesDados()
        {
            var fake = Fake();
            new StructuralPackagePublisher(fake).Write(Destination(), Files());

            Assert.Equal(new byte[] { 1, 2, 3, 0, 255 }, fake.Files[Path.Combine(Destination(), "structural-loads.json")]);
            Assert.Equal(Encoding.ASCII.GetBytes("pass\n"), fake.Files[Path.Combine(Destination(), "rackcad-ram-build.py")]);
        }

        // ---------------------------------------------------------------- failure injection: every write and the rename

        public static IEnumerable<object[]> FailurePoints()
        {
            yield return new object[] { "CreateDirectory", null, W.WriteFailed };
            yield return new object[] { "CreateFile", "manifest.json", W.WriteFailed };
            yield return new object[] { "CreateFile", "rackcad-ram-build.py", W.WriteFailed };
            yield return new object[] { "CreateFile", "structural-loads.json", W.WriteFailed };
            yield return new object[] { "MoveDirectory", null, W.PublishFailed };
        }

        [Theory]
        [MemberData(nameof(FailurePoints))]
        public void UnFalloEnCualquierEscrituraOEnElRenombradoNoDejaDestinoNiTemporal(string operation, string fileName, string code)
        {
            var fake = Fake();
            fake.Fail = (op, path) => op == operation && (fileName == null || Path.GetFileName(path) == fileName);

            var result = new StructuralPackagePublisher(fake).Write(Destination(), Files());

            Assert.False(result.Succeeded);
            Assert.Equal(code, result.Code);
            Assert.False(result.CleanupFailed);
            Assert.Contains("DeleteDirectory", string.Join("\n", fake.Operations));
            Assert.Empty(Leftovers(fake, Destination()));
        }

        [Fact]
        public void SiLaLimpiezaTambienFallaSeDiceYElResultadoSigueSiendoUnFallo()
        {
            var fake = Fake();
            fake.Fail = (op, path) => op == "MoveDirectory" || op == "DeleteDirectory";

            var result = new StructuralPackagePublisher(fake).Write(Destination(), Files());

            Assert.False(result.Succeeded);
            Assert.Equal(W.PublishFailed, result.Code);
            Assert.True(result.CleanupFailed);
            Assert.False(fake.Holds(Destination()));
        }

        [Fact]
        public void UnFalloConservaSuMensajeYElDestinoComoAsunto()
        {
            var fake = Fake();
            fake.Fail = (op, path) => op == "CreateFile" && Path.GetFileName(path) == "rackcad-ram-build.py";

            var result = new StructuralPackagePublisher(fake).Write(Destination(), Files());

            Assert.Equal("CreateFile", result.Detail);
            Assert.Equal(Destination(), result.Subject);
        }

        /// <summary>A writer that is NOT atomic: it writes straight into the destination and does not clean up. The harness must catch it.</summary>
        private static StructuralPackageWriteResult NaiveWrite(IStructuralPackageFileSystem fileSystem, string destination, IEnumerable<StructuralPackageFile> files)
        {
            fileSystem.CreateDirectory(destination);

            foreach (var file in files)
            {
                fileSystem.CreateFile(Path.Combine(destination, file.Name), file.GetContent());
            }

            return null;
        }

        [Fact]
        public void UnEscritorNoAtomicoDejaCarpetaYElArnesLoDetecta()
        {
            var fake = Fake();
            fake.Fail = (op, path) => op == "CreateFile" && Path.GetFileName(path) == "rackcad-ram-build.py";

            Assert.Throws<InjectedFailure>(() => NaiveWrite(fake, Destination(), Files()));
            Assert.NotEmpty(Leftovers(fake, Destination()));
        }

        [Fact]
        public void UnEscritorQueNoLimpiaLaTemporalTambienLoDetectaElArnes()
        {
            var fake = Fake();
            fake.Fail = (op, path) => op == "MoveDirectory";
            var temporary = Destination() + ".tmp";

            fake.CreateDirectory(temporary);
            fake.CreateFile(Path.Combine(temporary, "manifest.json"), new byte[] { 1 });

            Assert.NotEmpty(Leftovers(fake, Destination()));
        }

        // ---------------------------------------------------------------- the refusals: nothing is touched

        [Fact]
        public void UnDestinoQueYaExisteSeRechazaSinTocarNadaYNuncaSeSobrescribe()
        {
            var fake = Fake();
            fake.Directories.Add(Destination());
            fake.Files.Add(Path.Combine(Destination(), "keep.txt"), new byte[] { 9 });

            var result = new StructuralPackagePublisher(fake).Write(Destination(), Files());

            Assert.False(result.Succeeded);
            Assert.Equal(W.DestinationExists, result.Code);
            Assert.Empty(fake.Operations);
            Assert.Equal(new byte[] { 9 }, fake.Files[Path.Combine(Destination(), "keep.txt")]);
            Assert.Single(fake.Files);
        }

        [Fact]
        public void UnDestinoQueEsUnArchivoSeRechazaSinTocarNada()
        {
            var fake = Fake();
            fake.Files.Add(Destination(), new byte[] { 9 });

            var result = new StructuralPackagePublisher(fake).Write(Destination(), Files());

            Assert.Equal(W.DestinationExists, result.Code);
            Assert.Empty(fake.Operations);
        }

        [Fact]
        public void SiElPadreNoExisteSeRechazaSinCrearNada()
        {
            var fake = new FakeFileSystem();

            var result = new StructuralPackagePublisher(fake).Write(Destination(), Files());

            Assert.Equal(W.ParentMissing, result.Code);
            Assert.Empty(fake.Operations);
            Assert.Empty(fake.Directories);
        }

        [Theory]
        [InlineData("a/b")]
        [InlineData("a\\b")]
        [InlineData("..")]
        [InlineData(".")]
        [InlineData("../x")]
        [InlineData("..\\x")]
        [InlineData("a..b")]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("x ")]
        [InlineData(" x")]
        [InlineData("x.")]
        [InlineData("C:x")]
        [InlineData("a:b")]
        [InlineData("a|b")]
        [InlineData("a?b")]
        [InlineData("a*b")]
        [InlineData("a<b")]
        [InlineData("a\"b")]
        [InlineData("a\tb")]
        [InlineData("/abs")]
        [InlineData(null)]
        public void UnNombreDeArchivoQueNoEsSimpleSeRechazaSinTocarNada(string name)
        {
            var fake = Fake();
            var files = new List<StructuralPackageFile> { new StructuralPackageFile("ok.txt", new byte[] { 1 }), new StructuralPackageFile(name, new byte[] { 2 }) };

            var result = new StructuralPackagePublisher(fake).Write(Destination(), files);

            Assert.False(result.Succeeded);
            Assert.Equal(W.FileNameInvalid, result.Code);
            Assert.Empty(fake.Operations);
        }

        [Theory]
        [InlineData("a.txt", "a.txt")]
        [InlineData("a.txt", "A.TXT")]
        public void UnNombreDuplicadoSeRechazaSinTocarNadaTambienSiSoloCambianLasMayusculas(string first, string second)
        {
            var fake = Fake();
            var files = new List<StructuralPackageFile> { new StructuralPackageFile(first, new byte[] { 1 }), new StructuralPackageFile(second, new byte[] { 2 }) };

            var result = new StructuralPackagePublisher(fake).Write(Destination(), files);

            Assert.Equal(W.FileNameDuplicate, result.Code);
            Assert.Empty(fake.Operations);
        }

        [Fact]
        public void SinArchivosOConContenidoAusenteSeRechazaSinTocarNada()
        {
            var fake = Fake();

            Assert.Equal(W.NoFiles, new StructuralPackagePublisher(fake).Write(Destination(), new StructuralPackageFile[0]).Code);
            Assert.Equal(
                W.ContentMissing,
                new StructuralPackagePublisher(fake).Write(Destination(), new[] { new StructuralPackageFile("a.txt", null) }).Code);
            Assert.Empty(fake.Operations);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void UnDestinoEnBlancoSeRechazaSinTocarNada(string destination)
        {
            var fake = Fake();

            Assert.Equal(W.DestinationInvalid, new StructuralPackagePublisher(fake).Write(destination, Files()).Code);
            Assert.Empty(fake.Operations);
        }

        [Fact]
        public void UnDestinoConSeparadorFinalOSinNombreSeRechazaSinTocarNada()
        {
            var fake = Fake();

            Assert.Equal(W.DestinationInvalid, new StructuralPackagePublisher(fake).Write(Destination() + Path.DirectorySeparatorChar, Files()).Code);
            Assert.Equal(W.DestinationInvalid, new StructuralPackagePublisher(fake).Write(Path.GetPathRoot(Path.GetTempPath()), Files()).Code);
            Assert.Empty(fake.Operations);
        }

        [Fact]
        public void LosArgumentosNulosSonUnErrorDeProgramacion()
        {
            Assert.Throws<ArgumentNullException>(() => new StructuralPackagePublisher(null));
            Assert.Throws<ArgumentNullException>(() => new StructuralPackagePublisher(Fake()).Write(Destination(), null));
        }

        // ---------------------------------------------------------------- the real disk

        private static string TempFolder()
        {
            var folder = Path.Combine(Path.GetTempPath(), "rackcad-package-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(folder);
            return folder;
        }

        [Fact]
        public void EnDiscoPublicaLaCarpetaConLosBytesExactosYNoDejaTemporales()
        {
            var parent = TempFolder();

            try
            {
                var destination = Path.Combine(parent, "REF-CANT-01");
                var result = new StructuralPackagePublisher().Write(destination, Files());

                Assert.True(result.Succeeded, result.Code + " " + result.Detail);
                Assert.Equal(new[] { destination }, Directory.GetFileSystemEntries(parent));
                Assert.Equal(
                    new[] { "manifest.json", "rackcad-ram-build.py", "structural-loads.json" },
                    Directory.GetFiles(destination).Select(Path.GetFileName).OrderBy(n => n, StringComparer.Ordinal));
                Assert.Equal(new byte[] { 1, 2, 3, 0, 255 }, File.ReadAllBytes(Path.Combine(destination, "structural-loads.json")));
            }
            finally
            {
                Directory.Delete(parent, true);
            }
        }

        [Fact]
        public void EnDiscoUnDestinoExistenteQuedaIntactoYNoSeCreaNadaMas()
        {
            var parent = TempFolder();

            try
            {
                var destination = Path.Combine(parent, "exists");
                Directory.CreateDirectory(destination);
                File.WriteAllText(Path.Combine(destination, "keep.txt"), "keep");

                var result = new StructuralPackagePublisher().Write(destination, Files());

                Assert.Equal(W.DestinationExists, result.Code);
                Assert.Equal(new[] { destination }, Directory.GetFileSystemEntries(parent));
                Assert.Equal(new[] { Path.Combine(destination, "keep.txt") }, Directory.GetFileSystemEntries(destination));
                Assert.Equal("keep", File.ReadAllText(Path.Combine(destination, "keep.txt")));
            }
            finally
            {
                Directory.Delete(parent, true);
            }
        }

        [Fact]
        public void EnDiscoUnPadreInexistenteNoSeCreaYNoSeEscribeNada()
        {
            var parent = Path.Combine(Path.GetTempPath(), "rackcad-missing-" + Guid.NewGuid().ToString("N"));
            var result = new StructuralPackagePublisher().Write(Path.Combine(parent, "package"), Files());

            Assert.Equal(W.ParentMissing, result.Code);
            Assert.False(Directory.Exists(parent));
        }

        [Fact]
        public void EnDiscoUnArchivoQueYaExisteNoSePisa()
        {
            var fake = new StructuralPackageDiskFileSystem();
            var parent = TempFolder();

            try
            {
                var path = Path.Combine(parent, "f.bin");
                fake.CreateFile(path, new byte[] { 1 });

                Assert.ThrowsAny<IOException>(() => fake.CreateFile(path, new byte[] { 2 }));
                Assert.Equal(new byte[] { 1 }, File.ReadAllBytes(path));
            }
            finally
            {
                Directory.Delete(parent, true);
            }
        }

        // ---------------------------------------------------------------- neutral

        [Fact]
        public void ElEscritorYSusTiposNoNombranAlProveedor()
        {
            var path = Path.Combine(C.RepoRoot(), "src", "RackCad.Application", "StructuralAnalysis", "StructuralPackageWriter.cs");
            var code = StructuralAnalysisGuards.Code(File.ReadAllText(path));

            Assert.False(Regex.IsMatch(code, @"RamElements|RAM Elements|\bRam[A-Z]|\bProviders\b|\bSystems\b|manifest|\.py\b", RegexOptions.IgnoreCase), "El escritor nombra al proveedor o al contenido del paquete.");
            Assert.Equal("RackCad.Application.StructuralAnalysis", typeof(StructuralPackagePublisher).Namespace);
        }

        [Fact]
        public void ElEscritorPublicaUnPaqueteDelProveedorSinSaberQueLoEs()
        {
            var package = I.Package();
            var fake = Fake();
            var result = new StructuralPackagePublisher(fake).Write(Destination("ref"), package.Files);

            Assert.True(result.Succeeded);
            Assert.Equal(
                package.Files.Select(f => f.Name).OrderBy(n => n, StringComparer.Ordinal),
                fake.Files.Keys.Select(Path.GetFileName).OrderBy(n => n, StringComparer.Ordinal));
        }
    }
}
