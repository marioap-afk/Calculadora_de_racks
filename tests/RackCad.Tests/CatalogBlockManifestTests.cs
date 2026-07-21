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
            var actual = Sealed(new CatalogBlockManifest
            {
                Blocks = new List<CatalogBlockManifestEntry>
                {
                    new CatalogBlockManifestEntry { BlockName = "POSTE_BN", Parameters = new List<string>() }
                }
            });

            var issue = Assert.Single(expected.Compare(actual));
            Assert.Equal("MANIFEST_MISSING_PARAMETER", issue.Code);
            Assert.Equal(CatalogValidationSeverity.Warning, issue.Severity);
            Assert.Contains("PERALTE", issue.Message);
        }

        // ---- Defect 1: params are exact per pieceId+view+blockName, never a piece's other-view params -------

        [Fact]
        public void BuildExpected_LayoutParams_ApplyOnlyToTheSamePieceAndView()
        {
            // The piece uses PERALTE in FRONTAL and PLANTA but NOT in LATERAL, and each view is a DISTINCT block.
            var catalog = new RackCatalog
            {
                Blocks = new List<BlockCatalogEntry>
                {
                    Block("CUSTOM", "FRONTAL", "CUSTOM_FRONTAL"),
                    Block("CUSTOM", "LATERAL", "CUSTOM_LATERAL"),
                    Block("CUSTOM", "PLANTA", "CUSTOM_PLANTA")
                },
                ConnectionLayout = new List<ConnectionLayoutEntry>
                {
                    new ConnectionLayoutEntry { PieceId = "CUSTOM", View = "FRONTAL", ParamX = "PERALTE" },
                    new ConnectionLayoutEntry { PieceId = "CUSTOM", View = "PLANTA", ParamY = "PERALTE" },
                    new ConnectionLayoutEntry { PieceId = "CUSTOM", View = "LATERAL" } // no param in LATERAL
                }
            };

            var manifest = CatalogBlockManifest.BuildExpected(catalog);

            Assert.Equal(new[] { "PERALTE" }, manifest.Blocks.Single(b => b.BlockName == "CUSTOM_FRONTAL").Parameters);
            Assert.Equal(new[] { "PERALTE" }, manifest.Blocks.Single(b => b.BlockName == "CUSTOM_PLANTA").Parameters);
            Assert.Empty(manifest.Blocks.Single(b => b.BlockName == "CUSTOM_LATERAL").Parameters);
        }

        // ---- Defect 3: version + fingerprint integrity in Compare -------------------------------------------

        [Fact]
        public void Compare_ValidLibraryManifest_HasNoIssues()
        {
            var expected = CatalogBlockManifest.BuildExpected(CatalogWith("A_BN", "B_BN"));
            var actual = CatalogBlockManifest.FromJson(expected.ToJson()); // a faithfully persisted library

            Assert.Empty(expected.Compare(actual));
        }

        [Fact]
        public void Compare_IncompatibleSchemaVersion_IsErrorAndAborts()
        {
            var expected = CatalogBlockManifest.BuildExpected(CatalogWith("A_BN", "B_BN"));
            var actual = CatalogBlockManifest.BuildExpected(CatalogWith("A_BN")); // also MISSING B_BN
            actual.SchemaVersion = CatalogBlockManifest.CurrentSchemaVersion + 1;
            actual.Fingerprint = actual.ComputeFingerprint();

            var issues = expected.Compare(actual);

            var issue = Assert.Single(issues); // aborts: the missing-block diff is NOT reported for a schema we cannot read
            Assert.Equal("MANIFEST_SCHEMA_INCOMPATIBLE", issue.Code);
            Assert.Equal(CatalogValidationSeverity.Error, issue.Severity);
        }

        [Fact]
        public void Compare_MissingFingerprint_IsError()
        {
            var expected = CatalogBlockManifest.BuildExpected(CatalogWith("A_BN"));
            var actual = CatalogBlockManifest.BuildExpected(CatalogWith("A_BN"));
            actual.Fingerprint = null;

            Assert.Contains(expected.Compare(actual), i => i.Code == "MANIFEST_FINGERPRINT_MISMATCH"
                && i.Severity == CatalogValidationSeverity.Error);
        }

        [Fact]
        public void Compare_TamperedJson_FingerprintMismatchIsError()
        {
            var expected = CatalogBlockManifest.BuildExpected(CatalogWith("A_BN", "B_BN"));

            // Hand-edit the persisted JSON: drop a block but keep the old fingerprint (a truncated/tampered file).
            var json = expected.ToJson().Replace("\"B_BN\"", "\"B_BN_TAMPERED\"");
            var actual = CatalogBlockManifest.FromJson(json);

            Assert.Contains(expected.Compare(actual), i => i.Code == "MANIFEST_FINGERPRINT_MISMATCH"
                && i.Severity == CatalogValidationSeverity.Error);
        }

        private static CatalogBlockManifest Sealed(CatalogBlockManifest manifest)
        {
            manifest.Fingerprint = manifest.ComputeFingerprint();
            return manifest;
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
