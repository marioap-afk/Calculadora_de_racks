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
    /// CHARACTERIZATION of the column's regular punch region, written BEFORE it was extracted into
    /// <see cref="CantileverColumnRegularPunchGrid"/> (I-37C).
    ///
    /// It exists for one purpose: to prove the extraction was MECHANICAL. The goldens below were captured
    /// from I-37A as it shipped in <c>0610adb</c>, and they pin the elevations, the transverse rows, the
    /// diameters, the piece ids and the count — everything an observer of <c>ColumnRegularPunches</c> can
    /// see. If the extraction changed any number, these fail, and the contract of I-37C says that obliges a
    /// stop rather than an adjustment.
    ///
    /// The fixtures deliberately include a NON-DYADIC pitch. The pre-extraction loop ACCUMULATED
    /// (<c>z += pitch</c>) while the grid authority MULTIPLIES (<c>first + i × pitch</c>); for 4.0 those
    /// agree bit for bit, and for 3.7 they need not. A characterization that only used the default pitch
    /// would have proved nothing about the one case where the two forms can diverge.
    /// </summary>
    public class CantileverRegularPunchGridCharacterizationTests
    {
        private const string ColumnW = "AISC-W-W10X33";
        private const string BaseW = "AISC-W-W12X26";

        private static readonly StructuralSectionCatalog Catalog =
            new CsvStructuralSectionCatalogProvider(CatalogDirectory.Resolve()).Load();

        private static readonly StructuralSectionGeometryFactory Factory =
            new StructuralSectionGeometryFactory(Catalog);

        private static StructuralSectionId Id(string value) => StructuralSectionId.Parse(value);

        private static CantileverColumnBaseSectionPolicy Policy() =>
            CantileverColumnBaseSectionPolicy.Create(
                new[]
                {
                    new CantileverColumnBaseVariant(
                        CantileverColumnBaseVariantKind.WFlangeConnected, Id(ColumnW), Id(BaseW))
                },
                new[] { StructuralSectionFamily.W });

        private static CantileverColumnBaseAssembly Resolve(
            double height, double regularPitch = 4.0, double topOffset = 4.0, double baseLength = 48.0)
        {
            var design = new CantileverColumnBaseDesign
            {
                Column = new CantileverColumnDesign { SectionId = ColumnW, Height = height },
                Base = new CantileverBaseDesign { SectionId = BaseW, Length = baseLength }
            };
            design.Connection.Punches.ColumnBottomPlateEndOffset = 1.5;
            design.Connection.Punches.ColumnTopPunchOffset = topOffset;
            design.Connection.Punches.RegularColumnPitch = regularPitch;

            return new CantileverColumnBaseResolver(Catalog, Factory, Policy()).Resolve(design);
        }

        /// <summary>
        /// Everything an observer of the regular punches can see, in one deterministic string.
        ///
        /// Sixteen decimals on purpose: a golden rounded to four would let a floating-point difference through
        /// unnoticed, which is exactly the difference this test exists to catch.
        /// </summary>
        private static string Dump(IReadOnlyList<CantileverPunchPlan> punches) =>
            punches.Count + ";" + string.Join("|", punches.Select(p => string.Format(
                CultureInfo.InvariantCulture,
                "{0}~{1}~{2:R}~{3:R}~{4:R}~{5:R}~{6:R}~{7}",
                p.Id.Value,
                p.Surface,
                p.Datum.U,
                p.Datum.V,
                p.Datum.Diameter,
                p.Centre.X,
                p.Centre.Z,
                p.Datum.Axis)));

        // The goldens. Captured from the shipped resolver, and never edited to make a test pass.
        //
        // MOVIDO A PROPOSITO en la correccion de columna/base de I-37D (ronda 2). El dueno retiro
        // `ColumnTopPunchOffset` por ser un parametro sin utilidad de producto: lo que limita el ultimo agujero
        // no es un margen que alguien escribe, es si el AGUJERO ENTERO cabe bajo el extremo fisico de la
        // columna. Con el margen de 4 in el techo caia en z = 92 y el ultimo troquel quedaba en 88.5; con la
        // regla del radio el techo cae en 92.625 y entra tambien el de 92.5.
        //
        // Golden anterior: 36 troqueles, 18 elevaciones por fila, ultima 88.5.
        // Golden nuevo:    38 troqueles, 19 elevaciones por fila, ultima 92.5.
        // Causa fisica:    el agujero de 92.5 mide 0.75 y termina en 92.875 <= 96. Siempre cupo; el margen lo
        //                  excluia sin razon de producto.
        // MOVIDO OTRA VEZ en la correccion de columna y base, y por una causa fisica declarada: el dueno
        // corrigio la distancia de la orilla al centro del troquel de 1.5 a 1.0 in, y precisó que se mide
        // «desde el exterior de la placa hacia el centro de la columna». Las dos filas pasan de x = ±2.48
        // —(7.96/2) − 1.5, acotadas desde la COLUMNA— a x = ±2.245 —(6.49/2) − 1.0, acotadas desde la placa
        // posterior, cuyo ancho es el del patin de la base—.
        //
        // El numero de troqueles y sus elevaciones NO cambian: el datum horizontal no toca el vertical.
        private const string Golden96 =
            "38;CANT-CB-PCH-REG-1~ColumnFace~-2.245~20.5~0.75~-2.245~20.5~AlongY|CANT-CB-PCH-REG-2~ColumnFace~-2.245~24.5~0.75~-2.245~24.5~AlongY|CANT-CB-PCH-REG-3" +
            "~ColumnFace~-2.245~28.5~0.75~-2.245~28.5~AlongY|CANT-CB-PCH-REG-4~ColumnFace~-2.245~32.5~0.75~-2.245~32.5~AlongY|CANT-CB-PCH-REG-5~ColumnFace~-2.245~3" +
            "6.5~0.75~-2.245~36.5~AlongY|CANT-CB-PCH-REG-6~ColumnFace~-2.245~40.5~0.75~-2.245~40.5~AlongY|CANT-CB-PCH-REG-7~ColumnFace~-2.245~44.5~0.75~-2.245~44.5" +
            "~AlongY|CANT-CB-PCH-REG-8~ColumnFace~-2.245~48.5~0.75~-2.245~48.5~AlongY|CANT-CB-PCH-REG-9~ColumnFace~-2.245~52.5~0.75~-2.245~52.5~AlongY|CANT-CB-PCH-" +
            "REG-10~ColumnFace~-2.245~56.5~0.75~-2.245~56.5~AlongY|CANT-CB-PCH-REG-11~ColumnFace~-2.245~60.5~0.75~-2.245~60.5~AlongY|CANT-CB-PCH-REG-12~ColumnFace~" +
            "-2.245~64.5~0.75~-2.245~64.5~AlongY|CANT-CB-PCH-REG-13~ColumnFace~-2.245~68.5~0.75~-2.245~68.5~AlongY|CANT-CB-PCH-REG-14~ColumnFace~-2.245~72.5~0.75~-" +
            "2.245~72.5~AlongY|CANT-CB-PCH-REG-15~ColumnFace~-2.245~76.5~0.75~-2.245~76.5~AlongY|CANT-CB-PCH-REG-16~ColumnFace~-2.245~80.5~0.75~-2.245~80.5~AlongY|" +
            "CANT-CB-PCH-REG-17~ColumnFace~-2.245~84.5~0.75~-2.245~84.5~AlongY|CANT-CB-PCH-REG-18~ColumnFace~-2.245~88.5~0.75~-2.245~88.5~AlongY|CANT-CB-PCH-REG-19" +
            "~ColumnFace~-2.245~92.5~0.75~-2.245~92.5~AlongY|CANT-CB-PCH-REG-20~ColumnFace~2.245~20.5~0.75~2.245~20.5~AlongY|CANT-CB-PCH-REG-21~ColumnFace~2.245~24" +
            ".5~0.75~2.245~24.5~AlongY|CANT-CB-PCH-REG-22~ColumnFace~2.245~28.5~0.75~2.245~28.5~AlongY|CANT-CB-PCH-REG-23~ColumnFace~2.245~32.5~0.75~2.245~32.5~Alo" +
            "ngY|CANT-CB-PCH-REG-24~ColumnFace~2.245~36.5~0.75~2.245~36.5~AlongY|CANT-CB-PCH-REG-25~ColumnFace~2.245~40.5~0.75~2.245~40.5~AlongY|CANT-CB-PCH-REG-26" +
            "~ColumnFace~2.245~44.5~0.75~2.245~44.5~AlongY|CANT-CB-PCH-REG-27~ColumnFace~2.245~48.5~0.75~2.245~48.5~AlongY|CANT-CB-PCH-REG-28~ColumnFace~2.245~52.5" +
            "~0.75~2.245~52.5~AlongY|CANT-CB-PCH-REG-29~ColumnFace~2.245~56.5~0.75~2.245~56.5~AlongY|CANT-CB-PCH-REG-30~ColumnFace~2.245~60.5~0.75~2.245~60.5~Along" +
            "Y|CANT-CB-PCH-REG-31~ColumnFace~2.245~64.5~0.75~2.245~64.5~AlongY|CANT-CB-PCH-REG-32~ColumnFace~2.245~68.5~0.75~2.245~68.5~AlongY|CANT-CB-PCH-REG-33~C" +
            "olumnFace~2.245~72.5~0.75~2.245~72.5~AlongY|CANT-CB-PCH-REG-34~ColumnFace~2.245~76.5~0.75~2.245~76.5~AlongY|CANT-CB-PCH-REG-35~ColumnFace~2.245~80.5~0" +
            ".75~2.245~80.5~AlongY|CANT-CB-PCH-REG-36~ColumnFace~2.245~84.5~0.75~2.245~84.5~AlongY|CANT-CB-PCH-REG-37~ColumnFace~2.245~88.5~0.75~2.245~88.5~AlongY|" +
            "CANT-CB-PCH-REG-38~ColumnFace~2.245~92.5~0.75~2.245~92.5~AlongY";

        [Fact]
        public void TheGoldenDumpIsNotVacuous()
        {
            // A characterization whose golden is empty passes forever. This is what stops that.
            Assert.StartsWith("38;", Golden96, StringComparison.Ordinal);
            Assert.Equal(37, Golden96.Count(c => c == '|'));
            Assert.Contains("~20.5~", Golden96, StringComparison.Ordinal);
            Assert.Contains("~92.5~", Golden96, StringComparison.Ordinal);
            Assert.Contains("CANT-CB-PCH-REG-1~", Golden96, StringComparison.Ordinal);
        }

        [Fact]
        public void TheRegularPunchesOfTheShippedFixtureAreUNCHANGED()
        {
            var assembly = Resolve(96.0);

            Assert.False(assembly.IsBlocked);
            Assert.Equal(Golden96, Dump(assembly.ColumnRegularPunches));
        }

        [Theory]
        [InlineData(40.0)]
        [InlineData(60.0)]
        [InlineData(96.0)]
        [InlineData(144.0)]
        [InlineData(240.0)]
        public void TheElevationsStillStartOnePITCHAboveTheLastConnectionPunch(double height)
        {
            var assembly = Resolve(height);
            var elevations = assembly.ColumnRegularPunches
                .Select(p => p.Datum.V).Distinct().OrderBy(v => v).ToList();

            Assert.NotEmpty(elevations);
            Assert.Equal(
                assembly.Pattern.LastConnectionElevation + assembly.Pattern.Parameters.RegularColumnPitch,
                elevations[0],
                12);
        }

        [Theory]
        [InlineData(40.0)]
        [InlineData(96.0)]
        [InlineData(240.0)]
        public void TheLastElevationStillRespectsTheTopOffset(double height)
        {
            var assembly = Resolve(height);
            var elevations = assembly.ColumnRegularPunches
                .Select(p => p.Datum.V).Distinct().OrderBy(v => v).ToList();

            var ceiling = height - assembly.Pattern.Parameters.ColumnTopPunchOffset;

            Assert.True(elevations[elevations.Count - 1] <= ceiling + 1e-9);
            // And the NEXT one would not have fitted: the generation is not stopping early.
            Assert.True(
                elevations[elevations.Count - 1] + assembly.Pattern.Parameters.RegularColumnPitch >
                ceiling + 1e-9);
        }

        [Theory]
        [InlineData(3.7)]
        [InlineData(4.0)]
        [InlineData(5.25)]
        [InlineData(6.0)]
        public void ANonDyadicPitchStillProducesTheSAMESequence(double pitch)
        {
            // The one case where "accumulate" and "multiply" can differ. Both forms are computed here and
            // compared bit for bit, so the extraction cannot hide a drift behind a rounded assertion.
            var assembly = Resolve(240.0, regularPitch: pitch);

            Assert.False(assembly.IsBlocked);

            var actual = assembly.ColumnRegularPunches
                .Select(p => p.Datum.V).Distinct().OrderBy(v => v).ToList();

            var first = assembly.Pattern.LastConnectionElevation + pitch;
            var ceiling = 240.0 - assembly.Pattern.Parameters.ColumnTopPunchOffset;

            var accumulated = new List<double>();
            for (var z = first; z <= ceiling + 1e-9; z += pitch)
            {
                accumulated.Add(z);
            }

            Assert.Equal(accumulated.Count, actual.Count);
            for (var i = 0; i < accumulated.Count; i++)
            {
                Assert.Equal(accumulated[i].ToString("R", CultureInfo.InvariantCulture),
                    actual[i].ToString("R", CultureInfo.InvariantCulture));
            }
        }

        [Fact]
        public void AColumnTooShortForAnyRegularPunchStillWarnsAndReturnsNone()
        {
            // The other end of the behaviour: the warning, not an exception and not an empty success.
            var assembly = Resolve(20.0);

            Assert.Empty(assembly.ColumnRegularPunches);
            Assert.Contains(
                assembly.Diagnostics,
                d => d.Code == CantileverDiagnostics.NoRegularPunchFits &&
                     d.Severity == CantileverDiagnosticSeverity.Warning);
        }

        [Fact]
        public void TheAssemblySignatureOfTheShippedFixtureIsUNCHANGED()
        {
            // The coarsest net, and the one that would catch a change anywhere else in the sub-assembly.
            var assembly = Resolve(96.0);

            Assert.Equal(
                "col=AISC-W-W10X33@96;base=AISC-W-W12X26@48;pattern=rows=-2.245,2.245;d=0.75;base=0..12.2;inside=5;above=3;top=19;z=2.5|4.5|6.5|8.5|10.5|12.5|14.5|16.5;plates=0.25,0.25,0.25,0.25;gusset=6.8x6.8;punches=16,16,38,8;env=-3.98,-9.73,0..3.98,48.5,96.25",
                assembly.Signature());
        }
    }
}
