using System;
using System.Linq;
using RackCad.Application.StructuralSections;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>
    /// F2 — la identidad no puede codificar AISC como única autoridad posible.
    ///
    /// El catálogo es NEUTRAL: si el prefijo del id estuviera incrustado, la única fuente que existe hoy sería
    /// la única que podría existir nunca, y dos publicadores que nombraran el mismo perfil colisionarían. La
    /// autoridad es ahora un dato explícito de <see cref="StructuralSectionSource.IdNamespace"/>.
    ///
    /// La condición que estas pruebas protegen por encima de todo: los 983 ids AISC **no cambian**.
    /// </summary>
    public class StructuralSectionIdNamespaceTests
    {
        [Theory]
        [InlineData(StructuralSectionFamily.W, "W12X26", "AISC-W-W12X26")]
        [InlineData(StructuralSectionFamily.Channel, "C10X15.3", "AISC-C-C10X15_3")]
        [InlineData(StructuralSectionFamily.Angle, "L4X4X1/4", "AISC-L-L4X4X1_4")]
        [InlineData(StructuralSectionFamily.HssRectangular, "HSS4X4X.250", "AISC-HSS-RECT-HSS4X4X_250")]
        public void TheAiscNamespaceReproducesExactlyTheIdsThatAlreadyExist(
            StructuralSectionFamily family,
            string edi,
            string expected)
        {
            Assert.Equal(
                expected,
                StructuralSectionId.Create(StructuralSectionSource.AiscIdNamespace, family, edi).Value);
        }

        [Fact]
        public void AiscShapesDeclaresTheAiscNamespace()
        {
            Assert.Equal("AISC", StructuralSectionSource.AiscIdNamespace);
        }

        [Fact]
        public void TwoSourcesWithTheSameFamilyAndDesignationProduceDifferentIds()
        {
            var first = StructuralSectionId.Create("AISC", StructuralSectionFamily.W, "W12X26");
            var second = StructuralSectionId.Create("OTRA", StructuralSectionFamily.W, "W12X26");

            Assert.NotEqual(first, second);
            Assert.Equal("AISC-W-W12X26", first.Value);
            Assert.Equal("OTRA-W-W12X26", second.Value);
        }

        [Fact]
        public void TwoSyntheticSourcesCanCoexistInOneCatalogWithoutColliding()
        {
            var catalog = StructuralSectionCatalog.Create(
                new[] { Section("AISC", "SRC-A"), Section("OTRA", "SRC-B") },
                new[] { Source("AISC", "SRC-A"), Source("OTRA", "SRC-B") });

            Assert.Equal(2, catalog.Count);
            Assert.True(catalog.TryGetById("AISC-W-W12X26", out _));
            Assert.True(catalog.TryGetById("OTRA-W-W12X26", out _));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData("aisc")]
        [InlineData("AI-SC")]
        [InlineData("AI_SC")]
        [InlineData("AI SC")]
        [InlineData("AISÇ")]
        public void AnInvalidNamespaceIsRefused(string idNamespace)
        {
            Assert.False(StructuralSectionId.IsValidNamespace(idNamespace));
            Assert.False(StructuralSectionId.TryCreate(idNamespace, StructuralSectionFamily.W, "W12X26", out _));
            Assert.Throws<ArgumentException>(
                () => StructuralSectionId.Create(idNamespace, StructuralSectionFamily.W, "W12X26"));
        }

        [Fact]
        public void ASourceWithAnInvalidNamespaceIsRefusedByTheCatalog()
        {
            var error = Assert.Throws<ArgumentException>(() => StructuralSectionCatalog.Create(
                new StructuralSectionDefinition[0],
                new[] { Source(idNamespace: "no-vale", sourceId: "SRC-A") }));

            Assert.Contains("namespace de id invalido", error.Message, StringComparison.Ordinal);
        }

        [Fact]
        public void TwoSourcesSharingANamespaceAreRefusedByTheCatalog()
        {
            var error = Assert.Throws<ArgumentException>(() => StructuralSectionCatalog.Create(
                new StructuralSectionDefinition[0],
                new[] { Source("AISC", "SRC-A"), Source("AISC", "SRC-B") }));

            Assert.Contains("lo declaran dos fuentes", error.Message, StringComparison.Ordinal);
        }

        [Fact]
        public void ExpectedSectionIdUsesTheAuthorityItIsGiven_NotADefault()
        {
            var identity = Section("OTRA", "SRC-B").Identity;

            Assert.Equal("OTRA-W-W12X26", identity.ExpectedSectionId("OTRA").Value);
            Assert.NotEqual(identity.SectionId, identity.ExpectedSectionId("AISC"));
        }

        [Fact]
        public void TheValidatorRebuildsTheIdThroughTheSourcesDeclaredAuthority()
        {
            // La sección dice ser de SRC-B (namespace OTRA) pero su id lleva el prefijo AISC: el validador,
            // que reconstruye con la autoridad de la FUENTE, lo detecta.
            var wrong = new StructuralSectionDefinition
            {
                Identity = new StructuralSectionIdentity
                {
                    SectionId = StructuralSectionId.Create("AISC", StructuralSectionFamily.W, "W12X26"),
                    Family = StructuralSectionFamily.W,
                    EdiDesignation = "W12X26",
                    ManualLabel = "W12X26",
                    SourceId = "SRC-B",
                    SourceRevision = "1"
                },
                WeightPerLength = 26,
                NativeUnitSystem = StructuralSectionUnitSystem.UsCustomary,
                Dimensions = new WSectionDimensions
                {
                    Depth = 12.2, FlangeWidth = 6.49, WebThickness = 0.23, FlangeThickness = 0.38
                },
                Properties = new StructuralSectionProperties { Area = 7.65 }
            };

            var catalog = StructuralSectionCatalog.Create(new[] { wrong }, new[] { Source("OTRA", "SRC-B") });
            var report = new StructuralSectionCatalogValidator().Validate(catalog);

            Assert.Single(report.WithCode(StructuralSectionCatalogValidator.CodeIdMismatch));
        }

        [Fact]
        public void ASectionWhoseSourceIsUnknownCannotBeCheckedAndIsReported()
        {
            var orphan = Section("AISC", "SRC-QUE-NO-EXISTE");
            var catalog = StructuralSectionCatalog.Create(new[] { orphan }, new[] { Source("AISC", "SRC-A") });
            var report = new StructuralSectionCatalogValidator().Validate(catalog);

            Assert.Single(report.WithCode(StructuralSectionCatalogValidator.CodeUnknownSource));
        }

        [Fact]
        public void TheSourcesFileCarriesTheNamespaceAndRoundTrips()
        {
            var text = StructuralSectionCsvWriter.WriteSources(new[] { Source("AISC", "SRC-A") });

            Assert.Contains(StructuralSectionCsvSchema.IdNamespace, text.Split('\n')[0], StringComparison.Ordinal);

            var table = StrictCsvTable.Parse(
                StructuralSectionCsvSchema.SourcesFile, text, StructuralSectionCsvSchema.SourcesColumns);
            var row = Assert.Single(table.Rows);

            Assert.Equal("AISC", row.RequiredText(StructuralSectionCsvSchema.IdNamespace));
        }

        private static StructuralSectionSource Source(string idNamespace, string sourceId) =>
            new StructuralSectionSource
            {
                SourceId = sourceId,
                Revision = "1",
                IdNamespace = idNamespace,
                Publisher = "fuente sintetica de prueba",
                SourceType = "synthetic",
                NativeUnitSystem = StructuralSectionUnitSystem.UsCustomary,
                Title = sourceId,
                Url = "https://example.invalid"
            };

        private static StructuralSectionDefinition Section(string idNamespace, string sourceId) =>
            new StructuralSectionDefinition
            {
                Identity = new StructuralSectionIdentity
                {
                    SectionId = StructuralSectionId.Create(idNamespace, StructuralSectionFamily.W, "W12X26"),
                    Family = StructuralSectionFamily.W,
                    EdiDesignation = "W12X26",
                    ManualLabel = "W12X26",
                    SourceId = sourceId,
                    SourceRevision = "1"
                },
                WeightPerLength = 26,
                NativeUnitSystem = StructuralSectionUnitSystem.UsCustomary,
                Dimensions = new WSectionDimensions
                {
                    Depth = 12.2, FlangeWidth = 6.49, WebThickness = 0.23, FlangeThickness = 0.38
                },
                Properties = new StructuralSectionProperties { Area = 7.65 }
            };
    }
}
