using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using RackCad.Application.Catalogs;
using RackCad.Application.StructuralSections;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>
    /// The catalog RackCad actually distributes, read exactly as the plugin reads it: the seven files copied
    /// next to the binaries.
    ///
    /// This is the only suite that depends on real AISC ids, on purpose — everything else uses synthetic
    /// fixtures so a future revision of the source cannot break the whole test base. What it proves is that
    /// the 983 shipped rows load strictly, validate clean, and match what the manifest claims about them.
    /// </summary>
    public class ShippedStructuralSectionCatalogTests
    {
        private const int ExpectedW = 289;
        private const int ExpectedHssRectangular = 525;
        private const int ExpectedChannel = 32;
        private const int ExpectedAngle = 137;
        private const int ExpectedTotal = ExpectedW + ExpectedHssRectangular + ExpectedChannel + ExpectedAngle;

        private static CsvStructuralSectionCatalogProvider Provider() =>
            new CsvStructuralSectionCatalogProvider(CatalogDirectory.Resolve());

        private static StructuralSectionCatalog Catalog() => Provider().Load();

        [Fact]
        public void EverySevenFilesAreShippedNextToTheBinaries()
        {
            var directory = CatalogDirectory.Resolve();

            foreach (var fileName in StructuralSectionCsvSchema.AllFiles()
                .Concat(new[] { StructuralSectionCsvSchema.ManifestFile }))
            {
                Assert.True(File.Exists(Path.Combine(directory, fileName)), fileName);
            }
        }

        [Fact]
        public void TheWholeCatalogLoadsStrictly_WithoutASingleTolerantFallback()
        {
            // A strict load either produces every row or throws naming file, line and column. Reaching this
            // assertion at all is the point.
            Assert.Equal(ExpectedTotal, Catalog().Count);
        }

        [Fact]
        public void CountsPerFamilyAreTheOnesTheOwnerValidated()
        {
            var counts = Catalog().CountsByFamily();

            Assert.Equal(ExpectedW, counts[StructuralSectionFamily.W]);
            Assert.Equal(ExpectedHssRectangular, counts[StructuralSectionFamily.HssRectangular]);
            Assert.Equal(ExpectedChannel, counts[StructuralSectionFamily.Channel]);
            Assert.Equal(ExpectedAngle, counts[StructuralSectionFamily.Angle]);
        }

        [Fact]
        public void TheValidatorFindsNoErrorInTheDistributedCatalog()
        {
            var provider = Provider();
            var report = new StructuralSectionCatalogValidator()
                .Validate(provider.Load(), provider.ReadManifest(), provider.ComputeSha256);

            Assert.True(report.IsValid(strict: true), report.Format());
        }

        [Fact]
        public void ManifestCountsAndHashesMatchTheShippedFiles()
        {
            var provider = Provider();
            var manifest = provider.ReadManifest();

            Assert.Equal(ExpectedTotal, manifest.TotalCount);
            Assert.Equal(0, manifest.RejectedSelectedRows);

            foreach (var file in manifest.Files)
            {
                Assert.Equal(provider.ComputeSha256(file.Name), file.Sha256);
            }
        }

        [Fact]
        public void TheManifestDoesNotHashItself()
        {
            Assert.DoesNotContain(
                Provider().ReadManifest().Files,
                file => string.Equals(file.Name, StructuralSectionCsvSchema.ManifestFile, StringComparison.Ordinal));
        }

        [Fact]
        public void EverySectionDeclaresTheSameSourceAndRevision()
        {
            var catalog = Catalog();

            Assert.All(catalog.All, section =>
            {
                Assert.Equal(StructuralSectionSource.AiscShapesId, section.Identity.SourceId);
                Assert.Equal("16.0", section.Identity.SourceRevision);
                Assert.Equal(StructuralSectionUnitSystem.UsCustomary, section.NativeUnitSystem);
            });

            var source = Assert.Single(catalog.Sources);
            Assert.Equal(StructuralSectionSource.AiscShapesId, source.SourceId);
            Assert.Equal("16.0", source.Revision);
            Assert.Equal(StructuralSectionUnitSystem.UsCustomary, source.NativeUnitSystem);
        }

        [Fact]
        public void NoSectionHasAnEmptyIdOrAnIdThatContradictsItsDesignation()
        {
            Assert.All(Catalog().All, section =>
            {
                Assert.False(section.SectionId.IsEmpty);
                Assert.Equal(section.Identity.ExpectedSectionId, section.SectionId);
            });
        }

        [Fact]
        public void NoIdCarriesTheSourceRevision()
        {
            Assert.All(Catalog().All, section =>
                Assert.DoesNotContain("V16", section.SectionId.Value, StringComparison.Ordinal));
        }

        [Fact]
        public void NoRequiredMagnitudeIsZero_WhichIsHowAFailedParseWouldLook()
        {
            Assert.All(Catalog().All, section =>
            {
                Assert.True(section.WeightPerLength > 0, section.SectionId.Value);
                Assert.NotNull(section.Properties.Area);
                Assert.True(section.Properties.Area.Value > 0, section.SectionId.Value);
            });
        }

        [Fact]
        public void MaterialGradeIsNeverInferredFromTheAiscCatalog()
        {
            Assert.All(Catalog().All, section => Assert.Null(section.MaterialGrade));
        }

        [Fact]
        public void EverySectionIsEnabledByDefault_AndTheStatusOverlayHoldsOnlyExceptions()
        {
            var provider = Provider();

            Assert.Empty(provider.ReadStatusOverrides());
            Assert.All(provider.Load().All, section => Assert.True(section.IsEnabled));
        }

        [Fact]
        public void HssKeepsNominalAndDesignThicknessAsDistinctData()
        {
            var hss = Catalog().ByFamily(StructuralSectionFamily.HssRectangular)
                .Select(section => (HssRectangularSectionDimensions)section.Dimensions)
                .ToArray();

            Assert.All(hss, dimensions =>
            {
                Assert.NotNull(dimensions.NominalThickness);
                Assert.NotNull(dimensions.DesignThickness);
                Assert.True(dimensions.DesignThickness.Value <= dimensions.NominalThickness.Value);
            });

            // Not every shape distinguishes them (a 1 in. wall of some products is tabulated with tdes = tnom),
            // but the vast majority does, which is what makes keeping both worthwhile.
            Assert.True(hss.Count(d => d.DesignThickness.Value < d.NominalThickness.Value) > hss.Length / 2);
        }

        [Fact]
        public void SquareHssIsCountedInsideTheRectangularFamily()
        {
            var square = Catalog().ByFamily(StructuralSectionFamily.HssRectangular)
                .Count(section => ((HssRectangularSectionDimensions)section.Dimensions).IsSquare);

            Assert.Equal(126, square);
        }

        [Fact]
        public void PropertiesThatDoNotApplyToAFamilyAreNull_NeverZero()
        {
            var catalog = Catalog();

            Assert.All(catalog.ByFamily(StructuralSectionFamily.HssRectangular), section =>
            {
                Assert.Null(section.Properties.Cw);
                Assert.NotNull(section.Properties.HssTorsionalConstant);
            });

            Assert.All(catalog.ByFamily(StructuralSectionFamily.W), section =>
            {
                Assert.Null(section.Properties.HssTorsionalConstant);
                Assert.Null(section.Properties.Iz);
                Assert.NotNull(section.Properties.Cw);
            });

            Assert.All(catalog.ByFamily(StructuralSectionFamily.Angle), section =>
            {
                Assert.NotNull(section.Properties.Iz);
                Assert.NotNull(section.Properties.TanAlpha);
            });
        }

        [Fact]
        public void EqualLegAnglesTabulateZeroForZb_AndTheValidatorAcceptsIt()
        {
            // zB is 0 for every equal-leg angle because point B lies on the z axis. It is a REAL value, so the
            // "a zero means a lost value" rule must not apply to positions.
            var withZeroZb = Catalog().ByFamily(StructuralSectionFamily.Angle)
                .Count(section => section.Properties.ZB == 0);

            Assert.Equal(61, withZeroZb);
        }

        [Fact]
        public void LookupsResolveByIdByEdiAndByVisibleDesignation()
        {
            var catalog = Catalog();

            Assert.True(catalog.TryGetById("AISC-HSS-RECT-HSS4X4X_250", out var byId));
            Assert.True(catalog.TryGetByEdiDesignation("HSS4X4X.250", out var byEdi));
            Assert.True(catalog.TryGetByDesignation("HSS4X4X1/4", out var byLabel));

            Assert.Equal(byId.SectionId, byEdi.SectionId);
            Assert.Equal(byId.SectionId, byLabel.SectionId);
            Assert.Equal("HSS4X4X1/4", byId.DisplayName);
        }

        [Fact]
        public void TheHssIdFollowsTheEdiDesignation_NotTheManualLabel()
        {
            // ADR-0021 §6. Documented deliberately: the id of HSS4X4X1/4 is built from its EDI form
            // (HSS4X4X.250) because the rule is the same for the four families, without exception.
            var catalog = Catalog();

            Assert.True(catalog.TryGetByDesignation("HSS4X4X1/4", out var section));
            Assert.Equal("AISC-HSS-RECT-HSS4X4X_250", section.SectionId.Value);
            Assert.False(catalog.TryGetById("AISC-HSS-RECT-HSS4X4X1_4", out _));
        }

        [Fact]
        public void WeightLabelsRenderNativeFirstWithTheComputedEquivalent()
        {
            var catalog = Catalog();

            Assert.True(catalog.TryGetById("AISC-W-W12X26", out var w));
            Assert.Equal("W12X26 — 26 lb/ft (38.7 kg/m)",
                StructuralSectionLabelFormatter.FormatWeightWithDesignation(w));

            Assert.True(catalog.TryGetById("AISC-C-C10X15_3", out var channel));
            Assert.Equal("C10X15.3 — 15.3 lb/ft (22.8 kg/m)",
                StructuralSectionLabelFormatter.FormatWeightWithDesignation(channel));
        }

        // ---- Sentinels ------------------------------------------------------------------------------------
        //
        // Two per family, read BY HAND from the official workbook (SHA-256 82D0CEB9…3496, sheet
        // "Database v16.0"). Each test names the source row so the value can be checked in a minute; the same
        // cells are transcribed in
        // docs/automation/evidence/I-36A-catalogo-secciones-estructurales.md.

        [Fact]
        public void Sentinel_W44X408_MatchesTheWorkbookRow2()
        {
            var section = Get("AISC-W-W44X408");
            var dimensions = (WSectionDimensions)section.Dimensions;

            Assert.Equal("W44X408", section.Identity.EdiDesignation);
            Assert.Equal(408, section.WeightPerLength);
            Assert.Equal(120, section.Properties.Area);
            Assert.Equal(44.8, dimensions.Depth);
            Assert.Equal(16.1, dimensions.FlangeWidth);
            Assert.Equal(1.22, dimensions.WebThickness);
            Assert.Equal(2.17, dimensions.FlangeThickness);
            Assert.Equal(2.96, dimensions.KDesign);
            Assert.Equal(3.375, dimensions.KDetailing);
            Assert.Equal(38700, section.Properties.Ix);
            Assert.Equal(691000, section.Properties.Cw);
            Assert.True(section.SourceSpecialNote);  // tf = 2.17 in. > 2 in.
        }

        [Fact]
        public void Sentinel_W12X26_MatchesTheWorkbookRow245()
        {
            var section = Get("AISC-W-W12X26");
            var dimensions = (WSectionDimensions)section.Dimensions;

            Assert.Equal(26, section.WeightPerLength);
            Assert.Equal(7.65, section.Properties.Area);
            Assert.Equal(12.2, dimensions.Depth);
            Assert.Equal(6.49, dimensions.FlangeWidth);
            Assert.Equal(0.23, dimensions.WebThickness);
            Assert.Equal(0.38, dimensions.FlangeThickness);
            Assert.Equal(204, section.Properties.Ix);
            Assert.Equal(17.3, section.Properties.Iy);
            Assert.False(section.SourceSpecialNote);
        }

        [Fact]
        public void Sentinel_HSS34X10X1_MatchesTheWorkbookRow1536()
        {
            var section = Get("AISC-HSS-RECT-HSS34X10X1");
            var dimensions = (HssRectangularSectionDimensions)section.Dimensions;

            Assert.Equal(277.07, section.WeightPerLength);
            Assert.Equal(76.2, section.Properties.Area);
            Assert.Equal(34, dimensions.OverallDepth);
            Assert.Equal(10, dimensions.OverallWidth);
            Assert.Equal(31.2, dimensions.FlatDepth);
            Assert.Equal(7.21, dimensions.FlatWidth);
            Assert.Equal(1, dimensions.NominalThickness);
            Assert.Equal(0.93, dimensions.DesignThickness);
            Assert.Equal(555, section.Properties.HssTorsionalConstant);
            Assert.False(dimensions.IsSquare);
        }

        [Fact]
        public void Sentinel_HSS4X4X1_4_MatchesTheWorkbookRow1983()
        {
            var section = Get("AISC-HSS-RECT-HSS4X4X_250");
            var dimensions = (HssRectangularSectionDimensions)section.Dimensions;

            Assert.Equal("HSS4X4X1/4", section.Identity.ManualLabel);
            Assert.Equal("HSS4X4X.250", section.Identity.EdiDesignation);
            Assert.Equal(12.21, section.WeightPerLength);
            Assert.Equal(3.37, section.Properties.Area);
            Assert.Equal(4, dimensions.OverallDepth);
            Assert.Equal(4, dimensions.OverallWidth);
            Assert.Equal(0.25, dimensions.NominalThickness);
            Assert.Equal(0.233, dimensions.DesignThickness);
            Assert.True(dimensions.IsSquare);
        }

        [Fact]
        public void Sentinel_C15X50_MatchesTheWorkbookRow357()
        {
            var section = Get("AISC-C-C15X50");
            var dimensions = (ChannelSectionDimensions)section.Dimensions;

            Assert.Equal(50, section.WeightPerLength);
            Assert.Equal(14.7, section.Properties.Area);
            Assert.Equal(15, dimensions.Depth);
            Assert.Equal(3.72, dimensions.FlangeWidth);
            Assert.Equal(0.716, dimensions.WebThickness);
            Assert.Equal(0.65, dimensions.FlangeThickness);
            Assert.Equal(0.799, dimensions.CentroidX);
            Assert.Equal(0.583, dimensions.ShearCenterX);
            Assert.Equal(404, section.Properties.Ix);
        }

        [Fact]
        public void Sentinel_C10X15_3_MatchesTheWorkbookRow366()
        {
            var section = Get("AISC-C-C10X15_3");
            var dimensions = (ChannelSectionDimensions)section.Dimensions;

            Assert.Equal("C10X15.3", section.Identity.ManualLabel);
            Assert.Equal(15.3, section.WeightPerLength);
            Assert.Equal(4.48, section.Properties.Area);
            Assert.Equal(10, dimensions.Depth);
            Assert.Equal(2.6, dimensions.FlangeWidth);
            Assert.Equal(0.634, dimensions.CentroidX);
            Assert.Equal(0.884, section.Properties.FlexuralConstantH);
        }

        [Fact]
        public void Sentinel_L12X12X1_3_8_MatchesTheWorkbookRow429()
        {
            var section = Get("AISC-L-L12X12X1_3_8");
            var dimensions = (AngleSectionDimensions)section.Dimensions;

            Assert.Equal("L12X12X1-3/8", section.Identity.ManualLabel);
            Assert.Equal(105, section.WeightPerLength);
            Assert.Equal(31.1, section.Properties.Area);
            Assert.Equal(12, dimensions.ShortLeg);
            Assert.Equal(12, dimensions.LongLeg);
            Assert.Equal(1.38, dimensions.Thickness);
            Assert.Equal(3.5, dimensions.CentroidX);
            Assert.Equal(3.5, dimensions.CentroidY);
            Assert.Equal(165, section.Properties.Iz);
            Assert.True(dimensions.IsEqualLeg);
        }

        [Fact]
        public void Sentinel_L4X4X1_4_MatchesTheWorkbookRow509()
        {
            var section = Get("AISC-L-L4X4X1_4");
            var dimensions = (AngleSectionDimensions)section.Dimensions;

            Assert.Equal(6.6, section.WeightPerLength);
            Assert.Equal(1.93, section.Properties.Area);
            Assert.Equal(4, dimensions.ShortLeg);
            Assert.Equal(4, dimensions.LongLeg);
            Assert.Equal(0.25, dimensions.Thickness);
            Assert.Equal(1.19, section.Properties.Iz);
            Assert.Equal(0.0438, section.Properties.J.Value, 6);
            Assert.Equal(0.0505, section.Properties.Cw.Value, 6);
            Assert.Equal(0, section.Properties.ZB);
        }

        [Fact]
        public void UnequalAngles_PutTheShortLegInDAndTheLongOneInB()
        {
            // AISC labels L8X6X1 by its LONG leg first, while the columns are d = short and b = long. Getting
            // this backwards would silently mirror every unequal angle drawn in I-36B.
            var section = Get("AISC-L-L8X6X1");
            var dimensions = (AngleSectionDimensions)section.Dimensions;

            Assert.Equal(6, dimensions.ShortLeg);
            Assert.Equal(8, dimensions.LongLeg);
            Assert.False(dimensions.IsEqualLeg);
        }

        private static StructuralSectionDefinition Get(string id)
        {
            Assert.True(Catalog().TryGetById(id, out var section), id);
            return section;
        }
    }
}
