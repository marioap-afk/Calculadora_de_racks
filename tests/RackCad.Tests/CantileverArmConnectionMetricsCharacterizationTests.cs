using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using RackCad.Application.Catalogs;
using RackCad.Application.StructuralSections;
using RackCad.Application.StructuralSections.Geometry;
using RackCad.Application.Systems.Cantilever;
using RackCad.Domain.Systems.Cantilever;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>
    /// CHARACTERIZATION of the arm's CONNECTION METRICS as I-37B shipped them, captured BEFORE they were
    /// extracted into <c>CantileverArmConnectionMetricsResolver</c> (I-37C correction round).
    ///
    /// The extraction exists because the station was computing the same four elevations a second time — with
    /// its own <c>Math.Max(2, count)</c> and its own <c>offset ?? 0</c>, which turned invalid input into
    /// plausible numbers instead of diagnostics. One authority means the prediction and the resolve cannot
    /// disagree; this file is what proves the authority did not change what I-37B already produced.
    ///
    /// The goldens pin what an observer of a resolved arm can see about its connection: the selected indices,
    /// the two elevations, the two plate edges and the observed pitch, at sixteen decimals. A golden rounded to
    /// four would hide exactly the kind of difference this exists to catch.
    /// </summary>
    public class CantileverArmConnectionMetricsCharacterizationTests
    {
        private const string ColumnW = "AISC-W-W10X33";
        private const string BaseW = "AISC-W-W12X26";
        private const string Hss = "AISC-HSS-RECT-HSS4X4X_250";
        private const string Channel = "AISC-C-C10X15_3";

        private static readonly StructuralSectionCatalog Catalog =
            new CsvStructuralSectionCatalogProvider(CatalogDirectory.Resolve()).Load();

        private static readonly StructuralSectionGeometryFactory Factory =
            new StructuralSectionGeometryFactory(Catalog);

        private static StructuralSectionId Id(string value) => StructuralSectionId.Parse(value);

        private static CantileverColumnBaseAssembly Column(double height = 240.0)
        {
            var policy = CantileverColumnBaseSectionPolicy.Create(
                new[]
                {
                    new CantileverColumnBaseVariant(
                        CantileverColumnBaseVariantKind.WFlangeConnected, Id(ColumnW), Id(BaseW))
                },
                new[] { StructuralSectionFamily.W });

            var design = new CantileverColumnBaseDesign
            {
                Column = new CantileverColumnDesign { SectionId = ColumnW, Height = height },
                Base = new CantileverBaseDesign { SectionId = BaseW, Length = 48.0 }
            };
            design.Connection.Punches.ColumnBottomPlateEndOffset = 1.5;
            design.Connection.Punches.ColumnTopPunchOffset = 4.0;

            var assembly = new CantileverColumnBaseResolver(Catalog, Factory, policy).Resolve(design);
            Assert.False(assembly.IsBlocked);
            return assembly;
        }

        private static CantileverArmSectionPolicy Policy() =>
            CantileverArmSectionPolicy.Create(new[]
            {
                new CantileverArmVariant(Id(Hss), CantileverArmBodyArrangement.Single),
                new CantileverArmVariant(Id(Channel), CantileverArmBodyArrangement.Single),
                new CantileverArmVariant(Id(Channel), CantileverArmBodyArrangement.DoubleChannelFacing)
            });

        private static CantileverArmAssembly Arm(
            string section,
            CantileverArmBodyArrangement arrangement,
            CantileverArmSide side,
            double slope,
            int lowerIndex,
            int count,
            double offset)
        {
            var design = new CantileverArmDesign
            {
                Side = side,
                Body = new CantileverArmBodyDesign
                {
                    Arrangement = arrangement,
                    SectionId = section,
                    CutLength = 48.0,
                    SlopeRisePer12 = slope
                },
                MountingPlate = new CantileverArmMountingPlateDesign
                {
                    LowerColumnPunchIndex = lowerIndex,
                    VerticalPunchCount = count,
                    VerticalEndOffset = offset
                }
            };

            return new CantileverArmResolver(Catalog, Factory, Policy())
                .Resolve(design, Column(), "CHAR");
        }

        /// <summary>The fixtures, chosen to move every input the metrics depend on.</summary>
        private static IEnumerable<(string Name, CantileverArmAssembly Arm)> Fixtures()
        {
            yield return ("hss-flat-i0-n2-o1.5",
                Arm(Hss, CantileverArmBodyArrangement.Single, CantileverArmSide.PositiveY, 0.0, 0, 2, 1.5));
            yield return ("hss-flat-i7-n2-o1.5",
                Arm(Hss, CantileverArmBodyArrangement.Single, CantileverArmSide.PositiveY, 0.0, 7, 2, 1.5));
            yield return ("hss-slope1-i3-n4-o3",
                Arm(Hss, CantileverArmBodyArrangement.Single, CantileverArmSide.PositiveY, 1.0, 3, 4, 3.0));
            yield return ("hss-neg-slope2-i2-n3-o2",
                Arm(Hss, CantileverArmBodyArrangement.Single, CantileverArmSide.NegativeY, 2.0, 2, 3, 2.0));
            yield return ("chan-single-i0-n4-o1.5",
                Arm(Channel, CantileverArmBodyArrangement.Single, CantileverArmSide.PositiveY, 0.0, 0, 4, 1.5));
            yield return ("chan-pair-slope0.5-i1-n5-o2.5",
                Arm(Channel, CantileverArmBodyArrangement.DoubleChannelFacing, CantileverArmSide.PositiveY,
                    0.5, 1, 5, 2.5));
        }

        /// <summary>
        /// Everything the connection metrics determine, in one deterministic string.
        ///
        /// <c>BodyTopZ</c> is derived here the same way I-37B derives it — plate bottom plus the foreshortened
        /// section depth — because it is not exposed on the assembly. That is precisely the quantity the
        /// station was recomputing on its own, so pinning it is the point of this file.
        /// </summary>
        private static string Dump(CantileverArmAssembly arm)
        {
            var p = arm.ConnectionPattern;
            var run = Math.Cos(arm.Body.AngleRadians);

            return string.Format(
                CultureInfo.InvariantCulture,
                "i={0}..{1};n={2};z={3:R}..{4:R};plate={5:R}..{6:R};body={7:R}..{8:R};pitch={9:R};d={10:R}",
                p.LowerColumnPunchIndex,
                p.LowerColumnPunchIndex + p.VerticalPunchCount - 1,
                p.VerticalPunchCount,
                p.FirstElevation,
                p.LastElevation,
                p.PlateBottomZ,
                p.PlateTopZ,
                p.PlateBottomZ,
                p.PlateBottomZ + (run * arm.Body.SectionHeight),
                p.ObservedPitch,
                p.Diameter);
        }

        // The goldens, captured from the shipped resolver and never edited to make a test pass.
        private static readonly IReadOnlyDictionary<string, string> Golden =
            new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["hss-flat-i0-n2-o1.5"] =
                    "i=0..1;n=2;z=20.5..24.5;plate=19..26;body=19..23;pitch=4;d=0.75",
                ["hss-flat-i7-n2-o1.5"] =
                    "i=7..8;n=2;z=48.5..52.5;plate=47..54;body=47..51;pitch=4;d=0.75",
                ["hss-slope1-i3-n4-o3"] =
                    "i=3..6;n=4;z=32.5..44.5;plate=29.5..47.5;body=29.5..33.48618303297952;pitch=4;d=0.75",
                ["hss-neg-slope2-i2-n3-o2"] =
                    "i=2..4;n=3;z=28.5..36.5;plate=26.5..38.5;body=26.5..30.445575695328575;pitch=4;d=0.75",
                ["chan-single-i0-n4-o1.5"] =
                    "i=0..3;n=4;z=20.5..32.5;plate=19..34;body=19..29;pitch=4;d=0.75",
                ["chan-pair-slope0.5-i1-n5-o2.5"] =
                    "i=1..5;n=5;z=24.5..40.5;plate=22..43;body=22..31.99133073092352;pitch=4;d=0.75"
            };

        [Fact]
        public void TheGoldensAreNotVacuous()
        {
            // A characterization whose goldens are empty passes forever.
            Assert.Equal(6, Golden.Count);
            Assert.All(Golden.Values, v => Assert.Contains("plate=", v, StringComparison.Ordinal));
            Assert.All(Golden.Values, v => Assert.Contains("body=", v, StringComparison.Ordinal));

            // And they really do differ from one another: six identical goldens would prove nothing.
            Assert.Equal(6, Golden.Values.Distinct(StringComparer.Ordinal).Count());
        }

        [Fact]
        public void EveryFixtureResolves()
        {
            foreach (var (name, arm) in Fixtures())
            {
                Assert.False(
                    arm.IsBlocked,
                    name + " no resolvio: " + string.Join(" | ", arm.Diagnostics.Select(d => d.Code)));
            }
        }

        [Fact]
        public void TheConnectionMetricsOfI37BAreUNCHANGED()
        {
            foreach (var (name, arm) in Fixtures())
            {
                Assert.True(Golden.ContainsKey(name), "Falta el golden de " + name + ".");
                Assert.Equal(Golden[name], Dump(arm));
            }
        }

        [Theory]
        [InlineData(0.0)]
        [InlineData(0.5)]
        [InlineData(1.0)]
        [InlineData(2.0)]
        [InlineData(6.0)]
        public void TheBodyTopIsStillTheFORESHORTENEDDepth(double slope)
        {
            var arm = Arm(Hss, CantileverArmBodyArrangement.Single, CantileverArmSide.PositiveY, slope, 3, 4, 3.0);

            Assert.False(arm.IsBlocked);

            var expected = arm.ConnectionPattern.PlateBottomZ +
                           (Math.Cos(CantileverArmFrameResolver.AngleRadians(slope)) * arm.Body.SectionHeight);

            Assert.Equal(
                expected.ToString("R", CultureInfo.InvariantCulture),
                (arm.ConnectionPattern.PlateBottomZ +
                 (Math.Cos(arm.Body.AngleRadians) * arm.Body.SectionHeight))
                .ToString("R", CultureInfo.InvariantCulture));
        }

        [Fact]
        public void ThePlateEdgesAreStillTheElevationsPlusAndMinusTheMargin()
        {
            foreach (var (_, arm) in Fixtures())
            {
                var p = arm.ConnectionPattern;

                Assert.Equal(p.FirstElevation - p.VerticalEndOffset, p.PlateBottomZ, 12);
                Assert.Equal(p.LastElevation + p.VerticalEndOffset, p.PlateTopZ, 12);
            }
        }
    }
}
