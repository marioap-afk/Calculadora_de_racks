using System;
using System.Linq;
using RackCad.Application.Catalogs;
using RackCad.Application.Catalogs.Validation;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>
    /// Runs the I-19 validator against the ACTUAL shipped catalog folder and pins its state, so a future edit
    /// that introduces a new duplicate, a dangling FK, a missing view or a silently dropped row fails the
    /// build. It also proves the <see cref="SeccionRoles"/> extraction did not change how secciones.csv splits.
    ///
    /// Known baseline (2026-07, I-19): the distributed catalog has exactly ONE error and TWO warnings —
    ///   • DUPLICATE_ID @ TROQUEL_TOPE: connection-points.csv ships the id twice (roles Poste and FlowBed);
    ///     the FlowBed row is shadowed by FindConnectionPoint's FirstOrDefault. A pre-existing finding, out of
    ///     I-19 scope to fix (editing a hot append-only catalog + role-lookup behavior). See ideas-futuras.md.
    ///   • UNRESOLVED_BLOCK_PIECE @ TARIMA_GENERICA (FRONTAL + LATERAL): a generic library-only block with no
    ///     commercial catalog row — expected and harmless.
    /// </summary>
    public class ShippedCatalogIntegrityTests
    {
        [Fact]
        public void ShippedCatalog_ValidatorReportsOnlyTheKnownBaseline()
        {
            var report = CatalogValidator.Validate(JsonRackCatalogProvider.FromBaseDirectory());

            // Exactly one error: the known TROQUEL_TOPE duplicate connection-point id.
            var error = Assert.Single(report.Errors);
            Assert.Equal("DUPLICATE_ID", error.Code);
            Assert.Equal("TROQUEL_TOPE", error.Location);

            // Exactly two warnings: the generic TARIMA_GENERICA block in its two views.
            Assert.Equal(2, report.WarningCount);
            Assert.All(report.Warnings, warning => Assert.Equal("UNRESOLVED_BLOCK_PIECE", warning.Code));
            Assert.All(report.Warnings, warning => Assert.Contains("TARIMA_GENERICA", warning.Location));

            Assert.Equal(0, report.InfoCount);
            Assert.Equal(3, report.Issues.Count);
            Assert.False(report.IsValid(), report.Format());
        }

        [Fact]
        public void ShippedCatalog_HasNoDanglingReferencesOrMissingViews()
        {
            var report = CatalogValidator.Validate(JsonRackCatalogProvider.FromBaseDirectory());

            foreach (var code in new[]
            {
                "INVALID_MENSULA_REF",
                "INVALID_CONNECTION_POINT_REF",
                "INVALID_LAYOUT_PIECE_REF",
                "MISSING_BLOCK_NAME",
                "MISSING_BLOCK_VIEW",
                "MISSING_LAYOUT_VIEW",
                "DUPLICATE_BLOCK_KEY",
                "DUPLICATE_LAYOUT_KEY",
                "DUPLICATE_SECCION_ID",
                "DISCARDED_SECCION_ROW"
            })
            {
                Assert.True(!report.WithCode(code).Any(), code + " inesperado:\n" + report.Format());
            }
        }

        [Fact]
        public void ShippedSecciones_SplitByRoleIsUnchanged()
        {
            // Golden: the SeccionRoles extraction must load exactly the same typed lists as before.
            var catalog = JsonRackCatalogProvider.FromBaseDirectory().Load();

            Assert.Single(catalog.PostProfiles);
            Assert.Single(catalog.TrussProfiles);
            Assert.Equal(3, catalog.BeamProfiles.Count);
            Assert.Single(catalog.SpacerProfiles);
        }

        [Fact]
        public void ShippedSecciones_RawRowsAreLoadedAndNoneDiscarded()
        {
            var rows = JsonRackCatalogProvider.FromBaseDirectory().LoadSeccionRows();

            Assert.Equal(6, rows.Count);
            Assert.All(rows, row => Assert.True(SeccionRoles.IsRecognized(row.Rol), "rol no reconocido: " + row.Rol));
        }

        [Fact]
        public void ShippedCatalog_ExpectedManifestIsSelfConsistent()
        {
            var catalog = JsonRackCatalogProvider.FromBaseDirectory().Load();

            var manifest = CatalogBlockManifest.BuildExpected(catalog);

            var distinctBlockNames = catalog.Blocks
                .Select(block => block.BlockName)
                .Where(name => !string.IsNullOrWhiteSpace(name))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Count();

            Assert.False(string.IsNullOrWhiteSpace(manifest.Fingerprint));
            Assert.Equal(distinctBlockNames, manifest.Blocks.Count);
            Assert.Empty(manifest.Compare(manifest)); // a library that matches the catalog has no findings.
        }
    }
}
