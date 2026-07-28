using System;
using System.Linq;
using RackCad.Application.StructuralSections;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>
    /// The neutral core, exercised with SYNTHETIC sections only. Nothing here depends on a real AISC id, so
    /// these tests keep working when the source is revised — the distributed catalog has its own suite.
    /// </summary>
    public class StructuralSectionModelTests
    {
        // ---- Normalization and identity ---------------------------------------------------------------

        [Theory]
        [InlineData("W12X26", "W12X26")]
        [InlineData("C10X15.3", "C10X15_3")]
        [InlineData("L4X4X1/4", "L4X4X1_4")]
        [InlineData("L12X12X1-3/8", "L12X12X1_3_8")]
        [InlineData("HSS4X4X.250", "HSS4X4X_250")]
        [InlineData("  hss4x4x.250 ", "HSS4X4X_250")]
        public void Normalizer_UppercasesStripsSpacesAndMapsPunctuationToUnderscore(string input, string expected)
        {
            Assert.Equal(expected, StructuralSectionDesignationNormalizer.Normalize(input));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData("W12*26")]
        [InlineData("W12X26°")]
        public void Normalizer_RefusesEmptyOrUnexpectedCharacters(string input)
        {
            Assert.False(StructuralSectionDesignationNormalizer.TryNormalize(input, out _));
            Assert.Throws<ArgumentException>(() => StructuralSectionDesignationNormalizer.Normalize(input));
        }

        [Theory]
        [InlineData(StructuralSectionFamily.W, "W12X26", "AISC-W-W12X26")]
        [InlineData(StructuralSectionFamily.Channel, "C10X15.3", "AISC-C-C10X15_3")]
        [InlineData(StructuralSectionFamily.Angle, "L4X4X1/4", "AISC-L-L4X4X1_4")]
        [InlineData(StructuralSectionFamily.HssRectangular, "HSS4X4X.250", "AISC-HSS-RECT-HSS4X4X_250")]
        public void SectionId_FollowsTheDocumentedPolicy(
            StructuralSectionFamily family,
            string edi,
            string expected)
        {
            Assert.Equal(expected, StructuralSectionId.Create(family, edi).Value);
        }

        [Fact]
        public void SectionId_NeverCarriesTheSourceRevision()
        {
            // ADR-0021: the revision lives in its own field so a section survives v17 with the same id.
            var section = Section(StructuralSectionFamily.W, "W12X26", "W12X26", 26);

            Assert.DoesNotContain("16", section.SectionId.Value, StringComparison.Ordinal);
            Assert.Equal("16.0", section.Identity.SourceRevision);
        }

        [Fact]
        public void SectionId_RejectsTextOutsideItsCharset()
        {
            Assert.False(StructuralSectionId.TryParse("AISC-W-W12/26", out _));
            Assert.False(StructuralSectionId.TryParse("aisc-w-w12x26", out _));
            Assert.True(StructuralSectionId.TryParse("AISC-W-W12X26", out _));
        }

        [Fact]
        public void DefaultSectionId_IsEmptyAndNeverNull()
        {
            var id = default(StructuralSectionId);

            Assert.True(id.IsEmpty);
            Assert.Equal(string.Empty, id.Value);
        }

        // ---- Catalog lookups ---------------------------------------------------------------------------

        [Fact]
        public void GetById_ResolvesEvenWhenTheSectionIsDisabled()
        {
            // The round-trip guarantee: a design saved before the section was withdrawn must still load.
            var disabled = Disable(Section(StructuralSectionFamily.W, "W12X26", "W12X26", 26));
            var catalog = Catalog(disabled);

            Assert.True(catalog.TryGetById(disabled.SectionId, out var found));
            Assert.False(found.IsEnabled);
        }

        [Fact]
        public void ByFamily_HidesDisabledSectionsUnlessAskedExplicitly()
        {
            var enabled = Section(StructuralSectionFamily.W, "W12X26", "W12X26", 26);
            var disabled = Disable(Section(StructuralSectionFamily.W, "W12X30", "W12X30", 30));
            var catalog = Catalog(enabled, disabled);

            Assert.Single(catalog.ByFamily(StructuralSectionFamily.W));
            Assert.Equal(2, catalog.ByFamily(StructuralSectionFamily.W, includeDisabled: true).Count);
            Assert.Single(catalog.Enabled);
        }

        [Fact]
        public void ByFamily_FiltersToTheRequestedFamilyOnly()
        {
            var catalog = Catalog(
                Section(StructuralSectionFamily.W, "W12X26", "W12X26", 26),
                Section(StructuralSectionFamily.Channel, "C10X15.3", "C10X15.3", 15.3),
                Section(StructuralSectionFamily.Angle, "L4X4X1/4", "L4X4X1/4", 6.6));

            Assert.Single(catalog.ByFamily(StructuralSectionFamily.W));
            Assert.Single(catalog.ByFamily(StructuralSectionFamily.Channel));
            Assert.Empty(catalog.ByFamily(StructuralSectionFamily.HssRectangular));
        }

        [Fact]
        public void LookupByEdi_IgnoresPunctuationAndCase()
        {
            var catalog = Catalog(
                Section(StructuralSectionFamily.HssRectangular, "HSS4X4X.250", "HSS4X4X1/4", 12.21));

            Assert.True(catalog.TryGetByEdiDesignation("hss4x4x.250", out var byEdi));
            Assert.True(catalog.TryGetByDesignation("HSS4X4X1/4", out var byLabel));
            Assert.Equal(byEdi.SectionId, byLabel.SectionId);
        }

        [Fact]
        public void Search_MatchesEitherPublishedDesignation()
        {
            var catalog = Catalog(
                Section(StructuralSectionFamily.HssRectangular, "HSS4X4X.250", "HSS4X4X1/4", 12.21),
                Section(StructuralSectionFamily.HssRectangular, "HSS4X4X.375", "HSS4X4X3/8", 17.27));

            Assert.Equal(2, catalog.Search("hss4x4").Count);
            Assert.Single(catalog.Search("HSS4X4X3/8"));
            Assert.Empty(catalog.Search("W12"));
        }

        [Fact]
        public void AmbiguousDesignation_IsRejectedInsteadOfGuessed()
        {
            // Two DIFFERENT sections whose visible labels normalize to the same text: the lookup must refuse.
            var first = Section(StructuralSectionFamily.W, "W12X26", "AMBIGUA", 26);
            var second = Section(StructuralSectionFamily.W, "W12X30", "AMBIGUA", 30);
            var catalog = Catalog(first, second);

            Assert.False(catalog.TryGetByDesignation("AMBIGUA", out _));
            Assert.Equal(2, catalog.FindByDesignation("AMBIGUA").Count);
        }

        [Fact]
        public void DuplicateId_IsRefusedAtConstruction()
        {
            var section = Section(StructuralSectionFamily.W, "W12X26", "W12X26", 26);

            var error = Assert.Throws<ArgumentException>(() => Catalog(section, section));
            Assert.Contains("duplicado", error.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void EmptyCatalog_AnswersEverythingWithoutThrowing()
        {
            Assert.Empty(StructuralSectionCatalog.Empty.All);
            Assert.Empty(StructuralSectionCatalog.Empty.ByFamily(StructuralSectionFamily.W));
            Assert.False(StructuralSectionCatalog.Empty.TryGetByDesignation("W12X26", out _));
        }

        // ---- Optional properties ------------------------------------------------------------------------

        [Fact]
        public void MissingProperties_StayNull_NeverZero()
        {
            var section = Section(StructuralSectionFamily.W, "W12X26", "W12X26", 26);

            Assert.Null(section.Properties.Cw);
            Assert.Null(section.Properties.HssTorsionalConstant);
            Assert.Null(section.MaterialGrade);
        }

        [Fact]
        public void SquareHss_IsACaseOfRectangular_NotAFifthFamily()
        {
            var square = new HssRectangularSectionDimensions
            {
                OverallDepth = 4, OverallWidth = 4, NominalThickness = 0.25, DesignThickness = 0.233
            };
            var rectangular = new HssRectangularSectionDimensions
            {
                OverallDepth = 6, OverallWidth = 4, NominalThickness = 0.25, DesignThickness = 0.233
            };

            Assert.True(square.IsSquare);
            Assert.False(rectangular.IsSquare);
            Assert.Equal(StructuralSectionFamily.HssRectangular, square.Family);
        }

        [Fact]
        public void NominalAndDesignThickness_AreDistinctData()
        {
            var hss = new HssRectangularSectionDimensions
            {
                OverallDepth = 4, OverallWidth = 4, NominalThickness = 0.25, DesignThickness = 0.233
            };

            Assert.NotEqual(hss.NominalThickness, hss.DesignThickness);
        }

        // ---- Units and weight ---------------------------------------------------------------------------

        [Fact]
        public void PoundsPerFootToKilogramsPerMeter_UsesTheExactDefinition()
        {
            Assert.Equal(0.45359237d / 0.3048d, StructuralSectionUnits.PoundsPerFootToKilogramsPerMeter, 12);
            Assert.Equal(41.6685904, StructuralSectionUnits.ToKilogramsPerMeter(28), 6);
        }

        [Fact]
        public void NativeWeightIsPreserved_AndTheEquivalentIsComputed()
        {
            var section = Section(StructuralSectionFamily.W, "W12X26", "W12X26", 26);

            Assert.Equal(26, section.WeightPerLength);
            Assert.Equal(26, StructuralSectionUnits.WeightPerLengthInPoundsPerFoot(section));
            Assert.Equal(38.692, StructuralSectionUnits.WeightPerLengthInKilogramsPerMeter(section), 3);
        }

        [Fact]
        public void WeightForLength_UsesWeightPerLength_NotTheTextOfTheDesignation()
        {
            // The label says 26 and the tabulated weight says 99: the calculation must follow the DATA.
            var section = Section(StructuralSectionFamily.W, "W12X26", "W12X26", 99);

            Assert.Equal(99 * 10, StructuralSectionUnits.WeightInPounds(section, 120), 9);
        }

        [Fact]
        public void WeightForLength_ConvertsToKilogramsConsistently()
        {
            var section = Section(StructuralSectionFamily.W, "W12X26", "W12X26", 26);
            var pounds = StructuralSectionUnits.WeightInPounds(section, 144);
            var kilograms = StructuralSectionUnits.WeightInKilograms(section, 144);

            Assert.Equal(26 * 12, pounds, 9);
            Assert.Equal(pounds * 0.45359237, kilograms, 9);
            Assert.Equal(pounds, StructuralSectionUnits.Weight(section, 144), 9);
        }

        [Fact]
        public void WeightForLength_RefusesNonFiniteOrNegativeLengths()
        {
            var section = Section(StructuralSectionFamily.W, "W12X26", "W12X26", 26);

            Assert.Throws<ArgumentOutOfRangeException>(() => StructuralSectionUnits.WeightInPounds(section, -1));
            Assert.Throws<ArgumentOutOfRangeException>(
                () => StructuralSectionUnits.WeightInPounds(section, double.NaN));
        }

        // ---- Presentation --------------------------------------------------------------------------------

        [Fact]
        public void Formatter_RendersTheOwnersExampleExactly()
        {
            // The owner fixed this string. W12X28 is not an AISC v16.0 shape — it is the FORMAT that is being
            // pinned here, which is why the section is synthetic.
            var section = Section(StructuralSectionFamily.W, "W12X28", "W12X28", 28);

            Assert.Equal(
                "W12X28 — 28 lb/ft (41.7 kg/m)",
                StructuralSectionLabelFormatter.FormatWeightWithDesignation(section));
        }

        [Fact]
        public void Formatter_PutsTheNativeUnitFirst_AlsoForAMetricSource()
        {
            var metric = new StructuralSectionDefinition
            {
                Identity = new StructuralSectionIdentity
                {
                    SectionId = StructuralSectionId.Create(StructuralSectionFamily.W, "W310X42"),
                    Family = StructuralSectionFamily.W,
                    EdiDesignation = "W310X42",
                    ManualLabel = "W310X42",
                    SourceId = "OTRA-FUENTE",
                    SourceRevision = "1"
                },
                WeightPerLength = 41.7,
                NativeUnitSystem = StructuralSectionUnitSystem.Metric,
                Dimensions = new WSectionDimensions { Depth = 310, FlangeWidth = 165, WebThickness = 6, FlangeThickness = 10 },
                Properties = new StructuralSectionProperties { Area = 5320 }
            };

            Assert.Equal(
                "W310X42 — 41.7 kg/m (28 lb/ft)",
                StructuralSectionLabelFormatter.FormatWeightWithDesignation(metric));
        }

        [Fact]
        public void Formatter_KeepsTheTabulatedNativeNumberAsPublished()
        {
            var section = Section(StructuralSectionFamily.Channel, "C10X15.3", "C10X15.3", 15.3);

            Assert.Equal("15.3 lb/ft", StructuralSectionLabelFormatter.FormatNativeWeight(section));
            Assert.Equal("22.8 kg/m", StructuralSectionLabelFormatter.FormatEquivalentWeight(section));
        }

        // ---- Helpers -------------------------------------------------------------------------------------

        internal static StructuralSectionCatalog Catalog(params StructuralSectionDefinition[] sections) =>
            StructuralSectionCatalog.Create(sections, new[] { AiscSource });

        internal static readonly StructuralSectionSource AiscSource = new StructuralSectionSource
        {
            SourceId = StructuralSectionSource.AiscShapesId,
            Revision = "16.0",
            Publisher = "American Institute of Steel Construction",
            SourceType = "official technical database",
            NativeUnitSystem = StructuralSectionUnitSystem.UsCustomary,
            Title = "AISC Shapes Database v16.0",
            Url = "https://example.invalid"
        };

        internal static StructuralSectionDefinition Section(
            StructuralSectionFamily family,
            string edi,
            string label,
            double weightPerLength)
        {
            return new StructuralSectionDefinition
            {
                Identity = new StructuralSectionIdentity
                {
                    SectionId = StructuralSectionId.Create(family, edi),
                    Family = family,
                    EdiDesignation = edi,
                    ManualLabel = label,
                    SourceId = StructuralSectionSource.AiscShapesId,
                    SourceRevision = "16.0"
                },
                WeightPerLength = weightPerLength,
                NativeUnitSystem = StructuralSectionUnitSystem.UsCustomary,
                Dimensions = DimensionsFor(family),
                Properties = new StructuralSectionProperties { Area = 7.65, Ix = 204, Iy = 17.3 },
                IsEnabled = true
            };
        }

        internal static StructuralSectionDefinition Disable(StructuralSectionDefinition section) =>
            new StructuralSectionDefinition
            {
                Identity = section.Identity,
                WeightPerLength = section.WeightPerLength,
                NativeUnitSystem = section.NativeUnitSystem,
                Dimensions = section.Dimensions,
                Properties = section.Properties,
                IsEnabled = false,
                StatusNotes = "retirada por el dueño"
            };

        internal static IStructuralSectionDimensions DimensionsFor(StructuralSectionFamily family)
        {
            switch (family)
            {
                case StructuralSectionFamily.W:
                    return new WSectionDimensions
                    {
                        Depth = 12.2, FlangeWidth = 6.49, WebThickness = 0.23, FlangeThickness = 0.38
                    };
                case StructuralSectionFamily.HssRectangular:
                    return new HssRectangularSectionDimensions
                    {
                        OverallDepth = 4, OverallWidth = 4, FlatDepth = 3.1, FlatWidth = 3.1,
                        NominalThickness = 0.25, DesignThickness = 0.233
                    };
                case StructuralSectionFamily.Channel:
                    return new ChannelSectionDimensions
                    {
                        Depth = 10, FlangeWidth = 2.6, WebThickness = 0.24, FlangeThickness = 0.436
                    };
                case StructuralSectionFamily.Angle:
                    return new AngleSectionDimensions { ShortLeg = 4, LongLeg = 4, Thickness = 0.25 };
                default:
                    throw new ArgumentOutOfRangeException(nameof(family));
            }
        }
    }
}
