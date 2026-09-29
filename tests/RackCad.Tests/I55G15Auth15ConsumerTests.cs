using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>
    /// G15 consume AUTH-15 (I-52-AUTH15, integrado en main) sin envolverla ni clonarla: la API integrada es visible, hay UN solo
    /// consumidor en el Plugin y el producto solo anade lo que AUTH-15 declara que no posee (lock, transaccion, commit,
    /// referencia, RackId, transformacion, orden, lote, Resolve, Prepare, seleccion, politica, Regen, purga, importacion).
    /// </summary>
    public class I55G15Auth15ConsumerTests
    {
        private static DirectoryInfo RepoRoot()
        {
            var dir = new DirectoryInfo(AppContext.BaseDirectory);
            while (dir != null && !File.Exists(Path.Combine(dir.FullName, "RackCad.sln"))) dir = dir.Parent;
            Assert.NotNull(dir);
            return dir;
        }

        private static string Plugin(params string[] relative)
            => File.ReadAllText(Path.Combine(new[] { RepoRoot().FullName, "src", "RackCad.Plugin" }.Concat(relative).ToArray()));

        [Fact]
        public void G15_AUTH15_LA_API_INTEGRADA_TIENE_DOS_SOBRECARGAS_POR_FAMILIA_Y_UN_RESULTADO_TIPADO()
        {
            var creator = Plugin("Systems", "Shared", "RackDefinitionCreator.cs");

            Assert.Equal(2, Regex.Matches(creator, @"internal static RackDefinitionCreationResult CreateInTransaction\(").Count);
            Assert.Matches(new Regex(@"CreateInTransaction\(\s*Database database,\s*Transaction transaction,\s*LateralHeaderDrawer drawer,\s*HeaderRunPlan plan,"), creator);
            Assert.Matches(new Regex(@"CreateInTransaction\(\s*Database database,\s*Transaction transaction,\s*CantileverViewPlan plan,"), creator);

            var result = Plugin("Systems", "Shared", "RackDefinitionCreationResult.cs");
            foreach (var value in new[] { "TransactionMismatch", "InvalidPlan", "InvalidBlockName", "MissingLibraryBlocks", "InvalidEnvelope", "EnvelopeWriteFailed", "WriteFailed" })
            {
                Assert.Contains(value, result);
            }
        }

        [Fact]
        public void G15_AUTH15_HAY_UN_SOLO_CONSUMIDOR_EN_EL_PLUGIN_Y_ES_EL_ALCANCE_DE_ESCRITURA_DE_G15()
        {
            var pluginDir = Path.Combine(RepoRoot().FullName, "src", "RackCad.Plugin");
            var consumers = Directory.GetFiles(pluginDir, "*.cs", SearchOption.AllDirectories)
                .Where(path => !path.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
                    .Any(segment => segment == "bin" || segment == "obj"))
                .Where(path => File.ReadAllText(path).Contains("RackDefinitionCreator.CreateInTransaction("))
                .Select(path => Path.GetFileName(path))
                .OrderBy(name => name, StringComparer.Ordinal)
                .ToArray();

            Assert.Equal(new[] { "RackProjectionWriteScope.cs" }, consumers);
        }

        [Fact]
        public void G15_AUTH15_NO_HAY_UN_SEGUNDO_CREADOR_DE_DEFINICION_ENVUELTO_POR_G15()
        {
            var scope = Plugin("Views", "RackProjectionWriteScope.cs");

            // G15 llama; no hereda, no envuelve, no reimplementa.
            Assert.DoesNotContain("class RackDefinitionCreator", scope);
            Assert.DoesNotContain("static RackDefinitionCreationResult", scope);
            Assert.DoesNotContain("BlockTableRecord(", scope);
            Assert.DoesNotContain("SymbolTable", scope);
        }

        [Fact]
        public void G15_AUTH15_EL_PRODUCTO_NO_TOCA_LA_AUTORIDAD_NI_LA_FUNDACION()
        {
            // El creador de AUTH-15 sigue sin lock, transaccion propia, commit, referencia ni importacion.
            var creator = string.Join("\n", Plugin("Systems", "Shared", "RackDefinitionCreator.cs")
                .Split('\n').Select(line => line.Contains("//") ? line.Substring(0, line.IndexOf("//", StringComparison.Ordinal)) : line));

            foreach (var forbidden in new[] { "LockDocument", "StartTransaction", "StartOpenCloseTransaction", ".Commit(", ".Abort(", "new BlockReference", "Regen(", "Purge", "BlockLibraryImporter" })
            {
                Assert.DoesNotContain(forbidden, creator);
            }
        }

        [Fact]
        public void G15_AUTH15_UN_FALLO_DEL_CREADOR_NO_SE_COMPENSA_EN_EL_PRODUCTO()
        {
            var scope = Plugin("Views", "RackProjectionWriteScope.cs");

            Assert.DoesNotContain("TryCleanupDefinition", scope);
            Assert.DoesNotContain("EraseUnreferencedDefinition", scope);
            Assert.DoesNotContain(".Erase(", scope);
        }
    }
}
