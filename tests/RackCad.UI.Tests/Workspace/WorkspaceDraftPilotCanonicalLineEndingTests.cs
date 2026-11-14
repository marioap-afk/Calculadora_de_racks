using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using RackCad.Application.Catalogs;
using RackCad.Application.Persistence;
using RackCad.Application.Systems.PushBack;
using RackCad.Application.Systems.PushBack.Pilot;
using Xunit;

namespace RackCad.UI.Tests
{
    /// <summary>
    /// I-64 F4-T4A2-PREPARE-UI (INV-F4T4A2-15, the carry-over (5) of F4-E25): on Windows, where the indented JSON of the inner design carries
    /// CRLF, the document that the pilot persists WITHOUT an edit (Materialize, ComposeForWrite and BuildDesign with one catalog) is the inner
    /// design of the envelope byte for byte, with no normalization of the line endings. The Core tests normalize CRLF to LF because the Core
    /// job runs on Linux; this suite runs on Windows and so is the one place where that comparison can be exact. The eight canonical designs are
    /// named one by one: the fixture folder holds other, older designs too. The fixtures are read by path from the root of the repository.
    /// </summary>
    public class WorkspaceDraftPilotCanonicalLineEndingTests
    {
        private static readonly string[] Canonical =
        {
            "pb-01-base", "pb-02-modulos", "pb-03-fondos", "pb-04-blanco", "pb-05-topes", "pb-06-seguridad", "pb-07-combinado", "pb-08-redondeo",
        };

        public static IEnumerable<object[]> Designs() => Canonical.Select(name => new object[] { name });

        private static DirectoryInfo RepoRoot()
        {
            var dir = new DirectoryInfo(AppContext.BaseDirectory);
            while (dir != null && !File.Exists(Path.Combine(dir.FullName, "RackCad.sln"))) dir = dir.Parent;
            Assert.True(dir != null, "repo root (RackCad.sln) not found");
            return dir;
        }

        private static string Fixture(string name)
        {
            var path = Path.Combine(RepoRoot().FullName, "tests", "RackCad.Tests", "Fixtures", "I64", name + ".json");
            Assert.True(File.Exists(path), "missing fixture " + path);
            return File.ReadAllText(path);
        }

        [Fact]
        public void TheCorpusIsTheEightCanonicalDesignsOfTheCharacterizationByName()
        {
            Assert.Equal(8, Canonical.Length);
            Assert.Equal(8, Canonical.Distinct(StringComparer.Ordinal).Count());
            foreach (var name in Canonical) Assert.StartsWith("pb-0", name, StringComparison.Ordinal);
        }

        [Theory]
        [MemberData(nameof(Designs))]
        public void ThePersistedDocumentOfThePilotWithoutAnEditIsTheInnerDesignOfTheEnvelopeByteForByteWithoutNormalizingTheLineEndings(string fixture)
        {
            var text = Fixture(fixture);
            var catalog = JsonRackCatalogProvider.FromBaseDirectory().Load();
            var inner = new RackEmbedStore().Deserialize(text).Design;

            // The comparison is not vacuous on Windows: the inner design of the fixtures carries CRLF.
            Assert.Contains("\r\n", inner, StringComparison.Ordinal);

            var materialized = PushBackPilotMaterializer.Materialize(text, catalog);
            Assert.Equal(PushBackPilotMaterializationKind.Admitted, materialized.Kind);
            var composition = PushBackPilotMaterializer.ComposeForWrite(materialized.State, materialized.State, catalog);
            Assert.True(composition.IsComposed, composition.Reason);
            var persisted = new RackProjectStore().Serialize(
                RackProject.ForPushBack(new PushBackCompositeEditorAssembler(catalog).BuildDesign(composition.Editor, composition.Inputs)));

            Assert.Contains("\r\n", persisted, StringComparison.Ordinal);
            Assert.True(string.Equals(inner, persisted, StringComparison.Ordinal), fixture + ": the persisted document is not the inner design byte for byte");
        }
    }
}
