using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using RackCad.Application.Catalogs;
using RackCad.Application.Geometry;
using RackCad.Application.StructuralSections;
using RackCad.Application.StructuralSections.Geometry;
using Xunit;
using Xunit.Abstractions;

namespace RackCad.Tests
{
    /// <summary>
    /// I-36D: the 28 American Standard beams (AISC Type <c>S</c>, locally IPS) and the visual authority that
    /// ADR-0023 introduces for them.
    ///
    /// The suite is split along the line the ADR draws. What AISC owns — the 28 rows, their identity, their
    /// dimensions, their area, their weight and their properties — is asserted against the SHIPPED catalog,
    /// because those are copied values and a regression there means data was lost. What RackCad owns — the
    /// taper, the visual fillet, the tip and the warning — is asserted as a CONVENTION: constant across the
    /// 28, never tuned per designation, and always declared.
    ///
    /// The area residual is checked as a BAND, not as a target. It must stay inside the envelope the contract
    /// measured; it must never be closed by adjusting the rule.
    /// </summary>
    public class StructuralSectionSFamilyTests
    {
        private readonly ITestOutputHelper _output;

        public StructuralSectionSFamilyTests(ITestOutputHelper output) => _output = output;

        /// <summary>The sentinel the contract names. The dot of the designation normalizes to '_' (ADR-0021).</summary>
        private const string SentinelId = "AISC-S-S10X25_4";

        private const int ExpectedS = 28;
        private const int ExpectedTotal = 1011;

        private static StructuralSectionCatalog Catalog() =>
            new CsvStructuralSectionCatalogProvider(CatalogDirectory.Resolve()).Load();

        private static StructuralSectionGeometryFactory Factory() =>
            new StructuralSectionGeometryFactory(Catalog());

        private static IReadOnlyList<StructuralSectionDefinition> SSections() =>
            Catalog().ByFamily(StructuralSectionFamily.S).ToArray();

        // ---- What AISC owns ------------------------------------------------------------------------------

        [Fact]
        public void TheCatalogCarriesExactlyTwentyEightS_AndTotalsOneThousandAndEleven()
        {
            var catalog = Catalog();

            Assert.Equal(ExpectedS, catalog.ByFamily(StructuralSectionFamily.S).Count);
            Assert.Equal(ExpectedTotal, catalog.Count);
        }

        [Fact]
        public void TheSentinelResolvesAndKeepsTheWorkbookValues()
        {
            var catalog = Catalog();

            Assert.True(catalog.TryGetById(StructuralSectionId.Parse(SentinelId), out var section));
            Assert.Equal(StructuralSectionFamily.S, section.Family);
            Assert.Equal("S10X25.4", section.Identity.EdiDesignation);
            Assert.Equal("S10X25.4", section.Identity.ManualLabel);
            Assert.Equal(25.4, section.WeightPerLength, 6);

            var dimensions = Assert.IsType<SSectionDimensions>(section.Dimensions);
            Assert.Equal(10.0, dimensions.Depth.Value, 6);
            Assert.Equal(4.66, dimensions.FlangeWidth.Value, 6);
            Assert.Equal(0.311, dimensions.WebThickness.Value, 6);
            Assert.Equal(0.491, dimensions.FlangeThickness.Value, 6);
            Assert.Equal(1.13, dimensions.KDesign.Value, 6);
            Assert.Equal(7.45, section.Properties.Area.Value, 6);
        }

        /// <summary>
        /// The EDI designation and the Manual label stay SEPARATE fields (decision 5), even where they agree.
        /// </summary>
        [Fact]
        public void EdiAndManualLabelAreSeparateFields()
        {
            foreach (var section in SSections())
            {
                Assert.False(string.IsNullOrEmpty(section.Identity.EdiDesignation));
                Assert.False(string.IsNullOrEmpty(section.Identity.ManualLabel));
            }
        }

        /// <summary>S must never be presented as W: its dimensions are its own type (decision 6).</summary>
        [Fact]
        public void EverySUsesItsOwnDimensionType_NeverTheWOne()
        {
            foreach (var section in SSections())
            {
                Assert.IsType<SSectionDimensions>(section.Dimensions);
                Assert.IsNotType<WSectionDimensions>(section.Dimensions);
                Assert.StartsWith("AISC-S-", section.SectionId.Value, StringComparison.Ordinal);
            }
        }

        /// <summary>AISC reserves its special notes to W, M, WT and MT; the 28 S rows leave T_F empty.</summary>
        [Fact]
        public void SourceSpecialNoteIsNullForEveryS()
        {
            Assert.All(SSections(), section => Assert.Null(section.SourceSpecialNote));
        }

        /// <summary>
        /// The resistant block AISC publishes complete for S. Asserted as PRESENT, not as a value: the point
        /// is that no property was dropped on the way in.
        /// </summary>
        [Fact]
        public void TheWholeResistantBlockSurvivesTheImport()
        {
            foreach (var section in SSections())
            {
                var p = section.Properties;
                var label = section.Identity.ManualLabel;

                foreach (var pair in new (string Name, double? Value)[]
                {
                    ("A", p.Area), ("Ix", p.Ix), ("Zx", p.Zx), ("Sx", p.Sx), ("rx", p.Rx),
                    ("Iy", p.Iy), ("Zy", p.Zy), ("Sy", p.Sy), ("ry", p.Ry), ("J", p.J),
                    ("Cw", p.Cw), ("Wno", p.Wno), ("Sw1", p.Sw1), ("Qf", p.Qf), ("Qw", p.Qw),
                    ("rts", p.Rts), ("ho", p.Ho), ("PA", p.PA), ("PB", p.PB), ("PC", p.PC), ("PD", p.PD)
                })
                {
                    Assert.True(pair.Value.HasValue, label + " perdio la propiedad " + pair.Name);
                }
            }
        }

        /// <summary>k1 and WGo are empty in all 28 rows. Null, never zero: a zero would be a false measurement.</summary>
        [Fact]
        public void TheAbsentColumnsAreNullAndNotZero()
        {
            foreach (var section in SSections())
            {
                var dimensions = (SSectionDimensions)section.Dimensions;

                Assert.Null(dimensions.K1);
                Assert.Null(dimensions.WorkableGageOuter);
            }
        }

        [Fact]
        public void TheCsvRoundTripsWithoutLosingAValue()
        {
            foreach (var original in SSections())
            {
                var cells = StructuralSectionCsvSerializer.ToCells(original);
                var columns = StructuralSectionCsvSchema.ColumnsFor(StructuralSectionFamily.S);

                Assert.Equal(columns.Length, cells.Length);

                var table = StrictCsvTable.Parse(
                    StructuralSectionCsvSchema.SFile,
                    string.Join(",", columns) + "\n" + string.Join(",", cells) + "\n",
                    columns);

                var restored = StructuralSectionCsvSerializer.FromRow(
                    StructuralSectionFamily.S, table.Rows.Single());

                Assert.Equal(original.SectionId.Value, restored.SectionId.Value);
                Assert.Equal(original.WeightPerLength, restored.WeightPerLength, 9);
                Assert.Equal(original.Properties.Area.Value, restored.Properties.Area.Value, 9);

                var before = (SSectionDimensions)original.Dimensions;
                var after = (SSectionDimensions)restored.Dimensions;

                Assert.Equal(before.Depth, after.Depth);
                Assert.Equal(before.FlangeWidth, after.FlangeWidth);
                Assert.Equal(before.WebThickness, after.WebThickness);
                Assert.Equal(before.FlangeThickness, after.FlangeThickness);
                Assert.Equal(before.KDesign, after.KDesign);
                Assert.Equal(before.KDetailing, after.KDetailing);
                Assert.Equal(before.DistanceBetweenFilletToes, after.DistanceBetweenFilletToes);
                Assert.Equal(before.WorkableGageInner, after.WorkableGageInner);
                Assert.Null(after.K1);
                Assert.Null(after.WorkableGageOuter);
            }
        }

        /// <summary>The S file must not grow a slope or radius column: those are NOT catalog data.</summary>
        [Fact]
        public void TheSchemaCarriesNoSlopeAndNoRadiusColumn()
        {
            var columns = StructuralSectionCsvSchema.ColumnsFor(StructuralSectionFamily.S);

            foreach (var forbidden in new[]
                     { "slope", "taper", "pitch", "inclination", "radius", "fillet", "r", "tanAlpha" })
            {
                Assert.DoesNotContain(columns, c => string.Equals(c, forbidden, StringComparison.OrdinalIgnoreCase));
            }

            // T_F is dropped on purpose: it would be empty in all 28 rows.
            Assert.DoesNotContain("T_F", columns);
        }

        // ---- What RackCad owns ---------------------------------------------------------------------------

        [Fact]
        public void EverySGeometryDeclaresVisualDerivedAuthority_AtBothDetailLevels()
        {
            var factory = Factory();

            foreach (var section in SSections())
            {
                foreach (var detail in new[] { SectionDetailLevel.Simplified, SectionDetailLevel.Tabulated })
                {
                    var geometry = factory.Get(section, detail);

                    Assert.Equal(SectionGeometryAuthority.VisualDerived, geometry.Authority);
                    Assert.True(geometry.IsVisualDerived);
                    Assert.Contains(
                        geometry.Diagnostics,
                        d => d.Code == SectionGeometryDiagnostics.VisualConventionApplied);
                }
            }
        }

        [Fact]
        public void TabulatedDetailReportsTabulatedDerivedFidelity_AndNothingDegrades()
        {
            var factory = Factory();

            foreach (var section in SSections())
            {
                Assert.Equal(
                    SectionFidelity.TabulatedDerived,
                    factory.Get(section, SectionDetailLevel.Tabulated).Fidelity);

                Assert.Equal(
                    SectionFidelity.Simplified,
                    factory.Get(section, SectionDetailLevel.Simplified).Fidelity);
            }
        }

        /// <summary>
        /// No designation gets an exception: all 28 derive the visual fillet, so none falls back to square
        /// corners. This is the check the contract makes a stopping condition.
        /// </summary>
        [Fact]
        public void NoDesignationNeedsAnException()
        {
            var factory = Factory();

            foreach (var section in SSections())
            {
                var geometry = factory.Get(section, SectionDetailLevel.Tabulated);

                Assert.False(geometry.IsDegraded, section.Identity.ManualLabel + " degrado.");
                Assert.DoesNotContain(
                    geometry.Diagnostics,
                    d => d.Code == SectionGeometryDiagnostics.FilletNotDerivable ||
                         d.Code == SectionGeometryDiagnostics.FilletDoesNotFit);
            }
        }

        [Fact]
        public void BoundsAreExactlyFlangeWidthByDepth_AndTheCentroidSitsOnTheOrigin()
        {
            var factory = Factory();

            foreach (var section in SSections())
            {
                var dimensions = (SSectionDimensions)section.Dimensions;

                foreach (var detail in new[] { SectionDetailLevel.Simplified, SectionDetailLevel.Tabulated })
                {
                    var geometry = factory.Get(section, detail);

                    Assert.Equal(dimensions.FlangeWidth.Value, geometry.Bounds.Width, 9);
                    Assert.Equal(dimensions.Depth.Value, geometry.Bounds.Height, 9);

                    // Doubly symmetric by construction, so this is exact, not approximate.
                    Assert.True(
                        geometry.GeometricCentroidResidual < 1e-9,
                        section.Identity.ManualLabel + " " + detail + " residuo " +
                        geometry.GeometricCentroidResidual.ToString("G17", CultureInfo.InvariantCulture));

                    Assert.Equal(SectionOriginBasis.Symmetry, geometry.OriginBasis);
                }
            }
        }

        [Fact]
        public void TheFlangeIsActuallyTapered_AndThickerAtTheRoot()
        {
            var factory = Factory();

            foreach (var section in SSections())
            {
                var d = (SSectionDimensions)section.Dimensions;
                var overhang = (d.FlangeWidth.Value - d.WebThickness.Value) / 2.0;
                var half = SSectionGeometryProbe.FlangeSlope * overhang / 2.0;

                // The convention itself: root thicker than tip by exactly s·a, and tf the mean of the two.
                var root = d.FlangeThickness.Value + half;
                var tip = d.FlangeThickness.Value - half;

                Assert.True(tip > 0.0, section.Identity.ManualLabel + " tendria punta de espesor nulo.");
                Assert.True(root > tip);
                Assert.Equal(
                    SSectionGeometryProbe.FlangeSlope * overhang, root - tip, 9);
                Assert.Equal(d.FlangeThickness.Value, (root + tip) / 2.0, 9);

                // And it reaches the drawing: the contour is NOT a parallel-flange I.
                var geometry = factory.Get(section, SectionDetailLevel.Simplified);
                var tipY = geometry.OuterContour.Flatten(1e-6)
                    .Where(p => Math.Abs(Math.Abs(p.X) - (d.FlangeWidth.Value / 2.0)) < 1e-9 && p.Y > 0.0)
                    .Min(p => p.Y);

                Assert.Equal((d.Depth.Value / 2.0) - tip, tipY, 6);
            }
        }

        /// <summary>
        /// The area residual, as a BAND. It is diagnostic: the rule is never tuned to close it, and this test
        /// exists to catch someone doing exactly that — a suspiciously perfect area would fail the lower bound.
        /// </summary>
        [Fact]
        public void TheAreaResidualStaysInsideTheMeasuredEnvelope()
        {
            var factory = Factory();
            var errors = new List<double>();

            foreach (var section in SSections())
            {
                var geometry = factory.Get(section, SectionDetailLevel.Tabulated);
                var error = (geometry.Area - section.Properties.Area.Value) /
                            section.Properties.Area.Value * 100.0;

                errors.Add(error);
                _output.WriteLine(section.Identity.ManualLabel + "  " +
                                  error.ToString("+0.000;-0.000", CultureInfo.InvariantCulture) + " %");

                // Always positive: four fillets added, tip rounding not modelled.
                Assert.True(error > 0.0, section.Identity.ManualLabel + " dio error no positivo.");
                Assert.True(error < 3.0, section.Identity.ManualLabel + " supero el 3 %.");
            }

            Assert.Equal(ExpectedS, errors.Count);
            Assert.InRange(errors.Min(), 0.20, 0.30);
            Assert.InRange(errors.Max(), 2.50, 2.65);
            Assert.InRange(errors.Average(), 1.00, 1.25);
        }

        /// <summary>
        /// The rule degenerates into ADR-0022's at zero slope: <c>r = kdes − tf</c>. Verified on the algebra
        /// rather than asserted in prose, because this is what keeps S from forking the geometric model.
        /// </summary>
        [Fact]
        public void TheRuleDegeneratesIntoTheWRuleAtZeroSlope()
        {
            foreach (var slope in new[] { 0.0 })
            {
                const double tf = 0.491;
                const double kdes = 1.13;
                const double overhang = 2.1745;

                var root = tf + (slope * overhang / 2.0);
                var hypotenuse = Math.Sqrt(1.0 + (slope * slope));
                var radius = (kdes - root) * (hypotenuse + slope);

                Assert.Equal(kdes - tf, radius, 12);
            }
        }

        /// <summary>The visual fillet is tangent to BOTH the web face and the sloped face. Geometry, not faith.</summary>
        [Fact]
        public void TheVisualFilletIsTangentToTheWebAndToTheSlopedFace()
        {
            var s = SSectionGeometryProbe.FlangeSlope;
            var hypotenuse = Math.Sqrt(1.0 + (s * s));

            foreach (var section in SSections())
            {
                var d = (SSectionDimensions)section.Dimensions;
                var overhang = (d.FlangeWidth.Value - d.WebThickness.Value) / 2.0;
                var root = d.FlangeThickness.Value + (s * overhang / 2.0);
                var delta = d.KDesign.Value - root;
                var radius = delta * (hypotenuse + s);

                Assert.True(delta > 0.0, section.Identity.ManualLabel + " tendria delta no positivo.");

                var halfWeb = d.WebThickness.Value / 2.0;
                var halfDepth = d.Depth.Value / 2.0;
                var centreX = halfWeb + radius;
                var centreY = halfDepth - d.KDesign.Value;

                // Tangent to the web face x = halfWeb: the horizontal distance IS the radius.
                Assert.Equal(radius, centreX - halfWeb, 9);

                // Tangent to the sloped face y = (halfDepth - root) + s·(x - halfWeb): point-line distance.
                var distance = Math.Abs((s * centreX) - centreY + (halfDepth - root) - (s * halfWeb)) /
                               hypotenuse;

                Assert.Equal(radius, distance, 9);

                // And the touch point falls strictly inside the free overhang.
                var tangentOffset = radius * (1.0 - (s / hypotenuse));
                Assert.InRange(tangentOffset, 1e-9, overhang - 1e-9);
            }
        }

        // ---- The warning ---------------------------------------------------------------------------------

        /// <summary>
        /// A visually derived geometry without its warning cannot be constructed. This is the guard that makes
        /// the warning non-optional at the TYPE level, so no future path can quietly drop it.
        /// </summary>
        [Fact]
        public void AVisualDerivedGeometryWithoutItsWarningIsRejected()
        {
            var contour = ClosedContour2D.FromPolygon(new[]
            {
                new Point2D(-1.0, -1.0), new Point2D(1.0, -1.0),
                new Point2D(1.0, 1.0), new Point2D(-1.0, 1.0)
            });

            var error = Assert.Throws<ArgumentException>(() => StructuralSectionGeometry.Create(
                StructuralSectionId.Parse(SentinelId),
                StructuralSectionFamily.S,
                SectionDetailLevel.Simplified,
                SectionFidelity.Simplified,
                contour,
                authority: SectionGeometryAuthority.VisualDerived));

            Assert.Contains(SectionGeometryDiagnostics.VisualConventionApplied, error.Message);
        }

        // ---- The .gitattributes contract -----------------------------------------------------------------

        /// <summary>
        /// Every generated catalog file must be declared <c>-text</c> in <c>.gitattributes</c>.
        ///
        /// The repository runs with <c>core.autocrlf=true</c>. Without the attribute, a fresh clone rewrites
        /// the line endings of a generated file and its ON-DISK SHA-256 stops matching the one the manifest
        /// declares — the manifest would accuse the data of being corrupt when nothing was touched. I-36D hit
        /// exactly this when it added a fifth family file, so the rule gets a guard instead of a comment.
        /// </summary>
        [Fact]
        public void EveryGeneratedCatalogFileIsDeclaredBinaryInGitAttributes()
        {
            var root = new System.IO.DirectoryInfo(AppContext.BaseDirectory);

            while (root != null && !System.IO.File.Exists(System.IO.Path.Combine(root.FullName, "RackCad.sln")))
            {
                root = root.Parent;
            }

            Assert.True(root != null, "No se encontro la raiz del repositorio.");

            var attributes = System.IO.File.ReadAllText(
                System.IO.Path.Combine(root.FullName, ".gitattributes"));

            foreach (var fileName in StructuralSectionCsvSchema.AllFiles()
                         .Concat(new[] { StructuralSectionCsvSchema.ManifestFile }))
            {
                var expected = "assets/catalogs/" + fileName;

                // Comment lines mention the files too; only a REAL rule counts.
                var rule = attributes
                    .Split('\n')
                    .Select(l => l.Trim())
                    .FirstOrDefault(l =>
                        !l.StartsWith("#", StringComparison.Ordinal) &&
                        l.Contains(expected, StringComparison.Ordinal));

                Assert.True(
                    rule != null,
                    ".gitattributes no declara '" + expected + "': un clon nuevo le cambiaria los saltos de " +
                    "linea y su SHA-256 dejaria de coincidir con el manifiesto.");

                Assert.Contains("-text", rule, StringComparison.Ordinal);
            }
        }

        // ---- The other families are untouched ------------------------------------------------------------

        [Fact]
        public void EveryOtherFamilyStaysTabulatedConstrained()
        {
            var factory = Factory();

            foreach (var section in factory.Catalog.All.Where(s => s.Family != StructuralSectionFamily.S))
            {
                foreach (var detail in new[] { SectionDetailLevel.Simplified, SectionDetailLevel.Tabulated })
                {
                    var geometry = factory.Get(section, detail);

                    Assert.Equal(SectionGeometryAuthority.TabulatedConstrained, geometry.Authority);
                    Assert.False(geometry.IsVisualDerived);
                    Assert.DoesNotContain(
                        geometry.Diagnostics,
                        d => d.Code == SectionGeometryDiagnostics.VisualConventionApplied);
                }
            }
        }

        /// <summary>The plan carries the authority so the renderer never has to look at the family.</summary>
        [Fact]
        public void ThePlanCarriesTheAuthorityAndTheSignatureNoticesIt()
        {
            var factory = Factory();
            var s = factory.Catalog.ByFamily(StructuralSectionFamily.S).First();
            var w = factory.Catalog.ByFamily(StructuralSectionFamily.W).First();

            var sPlan = Plan(factory, s);
            var wPlan = Plan(factory, w);

            Assert.Equal(SectionGeometryAuthority.VisualDerived, sPlan.Authority);
            Assert.True(sPlan.IsVisualDerived);
            Assert.Equal(SectionGeometryAuthority.TabulatedConstrained, wPlan.Authority);
            Assert.False(wPlan.IsVisualDerived);

            Assert.Contains(SectionGeometryAuthority.VisualDerived.ToString(), sPlan.Signature());
            Assert.Contains(
                SectionGeometryAuthority.TabulatedConstrained.ToString(), wPlan.Signature());
        }

        private static StructuralSectionRepresentationPlan Plan(
            StructuralSectionGeometryFactory factory, StructuralSectionDefinition section)
        {
            var geometry = factory.Get(section, SectionDetailLevel.Tabulated);
            var instance = PrismaticSectionInstance.Create(section.SectionId, 24.0, null, 0.0, false);

            return StructuralSectionPlanBuilder.Build(geometry, instance, new SectionRepresentationOptions
            {
                Viewpoint = SectionViewpoint.Standard(SectionViewKind.CrossSection),
                Mode = SectionRepresentationMode.Wireframe,
                Detail = SectionDetailLevel.Tabulated
            });
        }
    }

    /// <summary>Mirrors the builder's convention so the tests can assert against it without reflection.</summary>
    internal static class SSectionGeometryProbe
    {
        public const double FlangeSlope = 1.0 / 6.0;
    }
}
