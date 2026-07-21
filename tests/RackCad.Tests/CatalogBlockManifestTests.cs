using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Catalogs;
using RackCad.Application.Catalogs.Validation;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>
    /// The expected manifest for blocks-library.dwg (idea-futuras #15): built from the catalog, hashed,
    /// round-tripped as JSON, and diffed against a library's actual manifest. No DWG is ever touched.
    /// </summary>
    public class CatalogBlockManifestTests
    {
        [Fact]
        public void BuildExpected_GroupsBlocksByNameWithViewsAndDerivedParameters()
        {
            var catalog = new RackCatalog
            {
                Blocks = new List<BlockCatalogEntry>
                {
                    Block("POSTE", "FRONTAL", "POSTE_BN"),
                    Block("POSTE", "LATERAL", "POSTE_BN"),
                    Block("PLACA", "FRONTAL", "PLACA_BN")
                },
                ConnectionLayout = new List<ConnectionLayoutEntry>
                {
                    Layout("POSTE", "PERALTE", paramY: null),
                    Layout("POSTE", null, paramY: "PERALTE"),
                    Layout("PLACA", null, paramY: null)
                }
            };

            var manifest = CatalogBlockManifest.BuildExpected(catalog);

            Assert.Equal(CatalogBlockManifest.CurrentSchemaVersion, manifest.SchemaVersion);
            Assert.Equal(2, manifest.Blocks.Count);

            var poste = manifest.Blocks.Single(b => b.BlockName == "POSTE_BN");
            Assert.Equal(new[] { "POSTE" }, poste.Pieces);
            Assert.Equal(new[] { "FRONTAL", "LATERAL" }, poste.Views);
            Assert.Equal(new[] { "PERALTE" }, poste.Parameters);

            var placa = manifest.Blocks.Single(b => b.BlockName == "PLACA_BN");
            Assert.Empty(placa.Parameters);
        }

        [Fact]
        public void BuildExpected_SkipsBlankBlockNames()
        {
            var catalog = new RackCatalog
            {
                Blocks = new List<BlockCatalogEntry>
                {
                    Block("P1", "FRONTAL", "   "),
                    Block("P2", "FRONTAL", "P2_BN")
                }
            };

            var manifest = CatalogBlockManifest.BuildExpected(catalog);

            Assert.Single(manifest.Blocks);
            Assert.Equal("P2_BN", manifest.Blocks[0].BlockName);
        }

        [Fact]
        public void Fingerprint_IsStableForSameContentAndChangesWithBlocks()
        {
            var first = CatalogBlockManifest.BuildExpected(CatalogWith("A_BN", "B_BN"));
            var same = CatalogBlockManifest.BuildExpected(CatalogWith("A_BN", "B_BN"));
            var different = CatalogBlockManifest.BuildExpected(CatalogWith("A_BN", "C_BN"));

            Assert.False(string.IsNullOrWhiteSpace(first.Fingerprint));
            Assert.Equal(first.Fingerprint, same.Fingerprint);
            Assert.NotEqual(first.Fingerprint, different.Fingerprint);
        }

        [Fact]
        public void Json_RoundTripsBlocksAndFingerprint()
        {
            var manifest = CatalogBlockManifest.BuildExpected(CatalogWith("A_BN", "B_BN"));

            var restored = CatalogBlockManifest.FromJson(manifest.ToJson());

            Assert.NotNull(restored);
            Assert.Equal(manifest.SchemaVersion, restored.SchemaVersion);
            Assert.Equal(manifest.Fingerprint, restored.Fingerprint);
            Assert.Equal(manifest.Fingerprint, restored.ComputeFingerprint());
            Assert.Equal(
                manifest.Blocks.Select(b => b.BlockName),
                restored.Blocks.Select(b => b.BlockName));
        }

        [Fact]
        public void Compare_IdenticalManifests_HasNoIssues()
        {
            var expected = CatalogBlockManifest.BuildExpected(CatalogWith("A_BN", "B_BN"));
            var actual = CatalogBlockManifest.BuildExpected(CatalogWith("A_BN", "B_BN"));

            Assert.Empty(expected.Compare(actual));
        }

        [Fact]
        public void Compare_MissingBlockInLibrary_IsError()
        {
            var expected = CatalogBlockManifest.BuildExpected(CatalogWith("A_BN", "B_BN"));
            var actual = CatalogBlockManifest.BuildExpected(CatalogWith("A_BN"));

            var issue = Assert.Single(expected.Compare(actual));
            Assert.Equal("MANIFEST_MISSING_BLOCK", issue.Code);
            Assert.Equal(CatalogValidationSeverity.Error, issue.Severity);
            Assert.Equal("B_BN", issue.Location);
        }

        [Fact]
        public void Compare_ExtraBlockInLibrary_IsInfo()
        {
            var expected = CatalogBlockManifest.BuildExpected(CatalogWith("A_BN"));
            var actual = CatalogBlockManifest.BuildExpected(CatalogWith("A_BN", "EXTRA_BN"));

            var issue = Assert.Single(expected.Compare(actual));
            Assert.Equal("MANIFEST_EXTRA_BLOCK", issue.Code);
            Assert.Equal(CatalogValidationSeverity.Info, issue.Severity);
            Assert.Equal("EXTRA_BN", issue.Location);
        }

        [Fact]
        public void Compare_MissingExpectedParameter_IsWarning()
        {
            var expected = new CatalogBlockManifest
            {
                Blocks = new List<CatalogBlockManifestEntry>
                {
                    new CatalogBlockManifestEntry
                    {
                        BlockName = "POSTE_BN",
                        Parameters = new List<string> { "PERALTE" }
                    }
                }
            };
            var actual = new CatalogBlockManifest
            {
                Blocks = new List<CatalogBlockManifestEntry>
                {
                    new CatalogBlockManifestEntry { BlockName = "POSTE_BN", Parameters = new List<string>() }
                }
            };

            var issue = Assert.Single(expected.Compare(actual));
            Assert.Equal("MANIFEST_MISSING_PARAMETER", issue.Code);
            Assert.Equal(CatalogValidationSeverity.Warning, issue.Severity);
            Assert.Contains("PERALTE", issue.Message);
        }

        private static RackCatalog CatalogWith(params string[] blockNames)
        {
            return new RackCatalog
            {
                Blocks = blockNames
                    .Select((name, index) => Block("PIEZA_" + index, "FRONTAL", name))
                    .ToList()
            };
        }

        private static BlockCatalogEntry Block(string pieceId, string view, string blockName) =>
            new BlockCatalogEntry { PieceId = pieceId, View = view, BlockName = blockName };

        private static ConnectionLayoutEntry Layout(string pieceId, string paramX, string paramY) =>
            new ConnectionLayoutEntry { PieceId = pieceId, View = "FRONTAL", ParamX = paramX, ParamY = paramY };
    }
}
