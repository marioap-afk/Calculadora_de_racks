using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using RackCad.Application.Catalogs;
using RackCad.Application.Catalogs.Validation;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>
    /// Positive and negative coverage for every I-19 category and severity: duplicate ids, invalid
    /// references/relations, missing blocks/views, and rows discarded by an unknown rol. The manifest
    /// category lives in <see cref="CatalogBlockManifestTests"/>; the shipped-catalog baseline in
    /// <see cref="ShippedCatalogIntegrityTests"/>.
    /// </summary>
    public class CatalogValidatorTests
    {
        // ---- Positive: a clean catalog produces no issues -----------------------------------------------

        [Fact]
        public void Validate_CleanCatalog_HasNoIssuesAndIsValid()
        {
            var report = CatalogValidator.Validate(CleanCatalog());

            Assert.Empty(report.Issues);
            Assert.True(report.IsValid());
            Assert.True(report.IsValid(strict: true));
            Assert.Equal(CatalogValidationSeverity.Info, report.MaxSeverity);
            Assert.Equal("Catálogo válido: sin incidencias.", report.Format());
        }

        [Fact]
        public void Validate_NullCatalog_ReportsError()
        {
            var report = CatalogValidator.Validate((RackCatalog)null);

            Assert.False(report.IsValid());
            Assert.Contains(report.Issues, issue => issue.Code == "NULL_CATALOG");
        }

        // ---- Category 1: duplicate ids ------------------------------------------------------------------

        [Fact]
        public void Validate_DuplicateIdWithinList_IsError()
        {
            var catalog = CleanCatalog();
            catalog.ConnectionPoints = new List<ConnectionPointCatalogEntry> { Cp("CP1"), Cp("cp1") };

            var report = CatalogValidator.Validate(catalog);

            var issue = Assert.Single(report.WithCode("DUPLICATE_ID"));
            Assert.Equal(CatalogValidationSeverity.Error, issue.Severity);
            Assert.Equal(CatalogValidationCategory.DuplicateId, issue.Category);
            Assert.Equal("CP1", issue.Location);
            Assert.False(report.IsValid());
        }

        [Fact]
        public void Validate_DuplicateIdAcrossRolesInSecciones_IsError()
        {
            var rows = new List<SeccionCatalogEntry>
            {
                Seccion("POSTE", "SHARED"),
                Seccion("LARGUERO", "SHARED")
            };

            var report = CatalogValidator.Validate(CleanCatalog(), rows);

            var issue = Assert.Single(report.WithCode("DUPLICATE_SECCION_ID"));
            Assert.Equal(CatalogValidationSeverity.Error, issue.Severity);
            Assert.Equal("SHARED", issue.Location);
            Assert.Contains("POSTE", issue.Message);
            Assert.Contains("LARGUERO", issue.Message);
        }

        // ---- Category 2: invalid references / relations -------------------------------------------------

        [Fact]
        public void Validate_BeamPointingAtMissingMensula_IsError()
        {
            var catalog = CleanCatalog();
            catalog.BeamProfiles = new List<BeamProfileCatalogEntry> { Beam("B1", "MENSULA_INEXISTENTE") };

            var report = CatalogValidator.Validate(catalog);

            var issue = Assert.Single(report.WithCode("INVALID_MENSULA_REF"));
            Assert.Equal(CatalogValidationSeverity.Error, issue.Severity);
            Assert.Equal(CatalogValidationCategory.InvalidReference, issue.Category);
        }

        [Fact]
        public void Validate_LayoutPointingAtMissingConnectionPoint_IsError()
        {
            var catalog = CleanCatalog();
            catalog.ConnectionLayout = new List<ConnectionLayoutEntry> { Layout("P1", "CP_INEXISTENTE", "FRONTAL") };

            var report = CatalogValidator.Validate(catalog);

            Assert.Single(report.WithCode("INVALID_CONNECTION_POINT_REF"));
            Assert.All(report.WithCode("INVALID_CONNECTION_POINT_REF"), i => Assert.Equal(CatalogValidationSeverity.Error, i.Severity));
        }

        [Fact]
        public void Validate_LayoutPointingAtMissingPiece_IsError()
        {
            var catalog = CleanCatalog();
            catalog.ConnectionLayout = new List<ConnectionLayoutEntry> { Layout("PIEZA_INEXISTENTE", "CP1", "FRONTAL") };

            var report = CatalogValidator.Validate(catalog);

            Assert.Single(report.WithCode("INVALID_LAYOUT_PIECE_REF"));
        }

        [Fact]
        public void Validate_DuplicateLayoutKey_IsWarning()
        {
            var catalog = CleanCatalog();
            catalog.ConnectionLayout = new List<ConnectionLayoutEntry>
            {
                Layout("P1", "CP1", "FRONTAL"),
                Layout("P1", "CP1", "FRONTAL")
            };

            var report = CatalogValidator.Validate(catalog);

            var issue = Assert.Single(report.WithCode("DUPLICATE_LAYOUT_KEY"));
            Assert.Equal(CatalogValidationSeverity.Warning, issue.Severity);
            Assert.True(report.IsValid());              // warning alone is not fatal...
            Assert.False(report.IsValid(strict: true)); // ...unless strict.
        }

        [Fact]
        public void Validate_DuplicateBlockKey_IsWarning()
        {
            var catalog = CleanCatalog();
            catalog.Blocks = new List<BlockCatalogEntry>
            {
                Block("P1", "FRONTAL", "P1_FRONTAL"),
                Block("P1", "FRONTAL", "P1_FRONTAL_BIS")
            };

            var report = CatalogValidator.Validate(catalog);

            var issue = Assert.Single(report.WithCode("DUPLICATE_BLOCK_KEY"));
            Assert.Equal(CatalogValidationSeverity.Warning, issue.Severity);
        }

        // ---- Category 2 (defect 4): empty mandatory ConnectionLayout fields -----------------------------

        [Fact]
        public void Validate_LayoutWithEmptyPieceId_IsError()
        {
            var catalog = CleanCatalog();
            catalog.ConnectionLayout = new List<ConnectionLayoutEntry> { Layout("", "CP1", "FRONTAL") };

            var report = CatalogValidator.Validate(catalog);

            var issue = Assert.Single(report.WithCode("EMPTY_LAYOUT_FIELD"));
            Assert.Equal(CatalogValidationSeverity.Error, issue.Severity);
            Assert.Contains("PieceId", issue.Message);
            // An empty field is not a dangling reference.
            Assert.Empty(report.WithCode("INVALID_LAYOUT_PIECE_REF"));
        }

        [Fact]
        public void Validate_LayoutWithEmptyConnectionPointId_IsError()
        {
            var catalog = CleanCatalog();
            catalog.ConnectionLayout = new List<ConnectionLayoutEntry> { Layout("P1", "", "FRONTAL") };

            var report = CatalogValidator.Validate(catalog);

            var issue = Assert.Single(report.WithCode("EMPTY_LAYOUT_FIELD"));
            Assert.Equal(CatalogValidationSeverity.Error, issue.Severity);
            Assert.Contains("ConnectionPointId", issue.Message);
        }

        [Fact]
        public void Validate_LayoutWithEmptyView_IsError()
        {
            var catalog = CleanCatalog();
            catalog.ConnectionLayout = new List<ConnectionLayoutEntry> { Layout("P1", "CP1", "") };

            var report = CatalogValidator.Validate(catalog);

            var issue = Assert.Single(report.WithCode("EMPTY_LAYOUT_FIELD"));
            Assert.Equal(CatalogValidationSeverity.Error, issue.Severity);
            Assert.Contains("View", issue.Message);
            Assert.Empty(report.WithCode("MISSING_LAYOUT_VIEW"));
        }

        [Fact]
        public void Validate_LayoutWithAllMandatoryFields_HasNoEmptyFieldError()
        {
            var report = CatalogValidator.Validate(CleanCatalog());

            Assert.Empty(report.WithCode("EMPTY_LAYOUT_FIELD"));
        }

        // ---- Category 3: missing blocks or views --------------------------------------------------------

        [Fact]
        public void Validate_BlockWithoutName_IsError()
        {
            var catalog = CleanCatalog();
            catalog.Blocks = new List<BlockCatalogEntry> { Block("P1", "FRONTAL", "  ") };

            var report = CatalogValidator.Validate(catalog);

            var issue = Assert.Single(report.WithCode("MISSING_BLOCK_NAME"));
            Assert.Equal(CatalogValidationSeverity.Error, issue.Severity);
            Assert.Equal(CatalogValidationCategory.MissingBlockOrView, issue.Category);
        }

        [Fact]
        public void Validate_BlockReferencingUndefinedView_IsError()
        {
            var catalog = CleanCatalog();
            catalog.Blocks = new List<BlockCatalogEntry> { Block("P1", "PLANTA", "P1_PLANTA") };

            var report = CatalogValidator.Validate(catalog);

            var issue = Assert.Single(report.WithCode("MISSING_BLOCK_VIEW"));
            Assert.Equal(CatalogValidationSeverity.Error, issue.Severity);
            Assert.Contains("PLANTA", issue.Message);
        }

        [Fact]
        public void Validate_LayoutReferencingUndefinedView_IsError()
        {
            var catalog = CleanCatalog();
            catalog.ConnectionLayout = new List<ConnectionLayoutEntry> { Layout("P1", "CP1", "PLANTA") };

            var report = CatalogValidator.Validate(catalog);

            Assert.Single(report.WithCode("MISSING_LAYOUT_VIEW"));
        }

        [Fact]
        public void Validate_BlockWithPieceInNoCatalog_IsWarningNotError()
        {
            var catalog = CleanCatalog();
            // A generic library-only block (like TARIMA_GENERICA): drawn, but no commercial catalog row.
            catalog.Blocks = new List<BlockCatalogEntry>
            {
                Block("P1", "FRONTAL", "P1_FRONTAL"),
                Block("TARIMA_GENERICA", "FRONTAL", "TARIMA_GENERICA")
            };

            var report = CatalogValidator.Validate(catalog);

            var issue = Assert.Single(report.WithCode("UNRESOLVED_BLOCK_PIECE"));
            Assert.Equal(CatalogValidationSeverity.Warning, issue.Severity);
            Assert.True(report.IsValid()); // generic blocks do not fail the catalog.
        }

        // ---- Category 4: rows discarded by rol ----------------------------------------------------------

        [Fact]
        public void Validate_RowWithUnknownRol_IsDiscardedWarning()
        {
            var rows = new List<SeccionCatalogEntry>
            {
                Seccion("POSTE", "P1"),
                Seccion("RARO", "X1"),
                Seccion("", "SIN_ROL")
            };

            var report = CatalogValidator.Validate(CleanCatalog(), rows);

            var discarded = report.WithCode("DISCARDED_SECCION_ROW").ToList();
            Assert.Equal(2, discarded.Count);
            Assert.All(discarded, i => Assert.Equal(CatalogValidationSeverity.Warning, i.Severity));
            Assert.All(discarded, i => Assert.Equal(CatalogValidationCategory.DiscardedRow, i.Category));
            Assert.Contains(discarded, i => i.Location == "X1");
            Assert.Contains(discarded, i => i.Location == "SIN_ROL");
        }

        [Fact]
        public void Validate_RecognizedRoles_AreNotDiscarded()
        {
            var rows = new List<SeccionCatalogEntry>
            {
                Seccion("POSTE", "P1"),
                Seccion("celosía", "T1"),   // accent + lower-case tolerated
                Seccion("LARGUERO", "B1"),
                Seccion("SEPARADOR", "S1")
            };

            var report = CatalogValidator.Validate(CleanCatalog(), rows);

            Assert.Empty(report.WithCode("DISCARDED_SECCION_ROW"));
        }

        // ---- End-to-end through the provider seam (LoadSeccionRows) -------------------------------------

        [Fact]
        public void ValidateDirectory_SurfacesDiscardedRowFromDisk()
        {
            var directory = Path.Combine(Path.GetTempPath(), "RackCadValidator_" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(directory);
            try
            {
                File.WriteAllText(
                    Path.Combine(directory, "secciones.csv"),
                    "rol,id,displayName\nPOSTE,P1,Poste uno\nRARO,X1,Rol desconocido\n");

                var report = CatalogValidator.ValidateDirectory(directory);

                var issue = Assert.Single(report.WithCode("DISCARDED_SECCION_ROW"));
                Assert.Equal("X1", issue.Location);
            }
            finally
            {
                Directory.Delete(directory, true);
            }
        }

        // ---- Category 5 folded into the single report (manifest) ----------------------------------------

        [Fact]
        public void Validate_WithLibraryManifest_FoldsManifestMismatchIntoReport()
        {
            var catalog = CleanCatalog();
            catalog.Blocks = new List<BlockCatalogEntry> { Block("P1", "FRONTAL", "P1_FRONTAL") };

            // A valid (fingerprinted) library that simply does NOT contain the expected block.
            var library = new CatalogBlockManifest();
            library.Fingerprint = library.ComputeFingerprint();

            var report = CatalogValidator.Validate(catalog, rawSecciones: null, libraryManifest: library);

            var issue = Assert.Single(report.WithCode("MANIFEST_MISSING_BLOCK"));
            Assert.Equal(CatalogValidationSeverity.Error, issue.Severity);
            Assert.Equal(CatalogValidationCategory.Manifest, issue.Category);
            Assert.Equal("P1_FRONTAL", issue.Location);
        }

        // ---- Report shape -------------------------------------------------------------------------------

        [Fact]
        public void Report_Format_ListsCountsAndIssues()
        {
            var catalog = CleanCatalog();
            catalog.ConnectionPoints = new List<ConnectionPointCatalogEntry> { Cp("CP1"), Cp("CP1") };

            var text = CatalogValidator.Validate(catalog).Format();

            Assert.Contains("1 error(es)", text);
            Assert.Contains("DUPLICATE_ID", text);
        }

        // ---- Fixtures -----------------------------------------------------------------------------------

        private static RackCatalog CleanCatalog()
        {
            return new RackCatalog
            {
                Views = new List<ViewCatalogEntry> { View("FRONTAL"), View("LATERAL") },
                Mensulas = new List<MensulaCatalogEntry> { Mensula("M1") },
                BeamProfiles = new List<BeamProfileCatalogEntry> { Beam("B1", "M1") },
                PostProfiles = new List<ProfileCatalogEntry> { Profile("P1") },
                ConnectionPoints = new List<ConnectionPointCatalogEntry> { Cp("CP1") },
                ConnectionLayout = new List<ConnectionLayoutEntry> { Layout("P1", "CP1", "FRONTAL") },
                Blocks = new List<BlockCatalogEntry> { Block("P1", "FRONTAL", "P1_FRONTAL") }
            };
        }

        private static ViewCatalogEntry View(string id) => new ViewCatalogEntry { Id = id };

        private static MensulaCatalogEntry Mensula(string id) => new MensulaCatalogEntry { Id = id };

        private static BeamProfileCatalogEntry Beam(string id, string mensula) =>
            new BeamProfileCatalogEntry { Id = id, Mensula = mensula };

        private static ProfileCatalogEntry Profile(string id) => new ProfileCatalogEntry { Id = id };

        private static ConnectionPointCatalogEntry Cp(string id) => new ConnectionPointCatalogEntry { Id = id };

        private static ConnectionLayoutEntry Layout(string pieceId, string connectionPointId, string view) =>
            new ConnectionLayoutEntry { PieceId = pieceId, ConnectionPointId = connectionPointId, View = view };

        private static BlockCatalogEntry Block(string pieceId, string view, string blockName) =>
            new BlockCatalogEntry { PieceId = pieceId, View = view, BlockName = blockName };

        private static SeccionCatalogEntry Seccion(string rol, string id) =>
            new SeccionCatalogEntry { Rol = rol, Id = id };
    }
}
