using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using RackCad.Application.StructuralAnalysis;
using RackCad.Application.StructuralSections;
using RackCad.Application.Units;
using Xunit;
using static RackCad.Tests.ExpressionSemanticTestSupport;

namespace RackCad.Tests
{
    /// <summary>
    /// I-65 F1-T1 — INV-02, INV-03 and INV-16: typed quantities, the closed force table with its single conversion to
    /// pounds-force, the unique owners of the pound mass and of standard gravity, and the bit-for-bit identity of
    /// <c>StructuralSectionUnits</c>.
    /// </summary>
    public class StructuralAnalysisUnitTests
    {
        private const string SectionUnitsFile = "src/RackCad.Application/StructuralSections/StructuralSectionUnits.cs";

        /// <summary>SHA-256 of StructuralSectionUnits.cs (LF line endings) with ONLY its literal replaced by the MassUnits member.</summary>
        private const string SectionUnitsExpectedSha256 = "ac181811ebf160f5af861931c20c5c9f352c49edc8ed4d27e999cf899d2a7d0b";

        private static readonly Type[] QuantityTypes =
        {
            typeof(StructuralLength), typeof(StructuralForce), typeof(StructuralArea), typeof(StructuralMomentOfInertia),
        };

        private static long Bits(double value) => BitConverter.DoubleToInt64Bits(value);

        private static void AssertRelative(double expected, double actual, double tolerance = 1e-12)
        {
            var scale = Math.Max(Math.Abs(expected), double.Epsilon);
            Assert.True(Math.Abs(expected - actual) / scale <= tolerance, "esperado " + expected.ToString("R") + ", obtenido " + actual.ToString("R"));
        }

        // ================================================================ the owners

        [Fact]
        public void MassUnits_DeclaraLaLibraExacta()
        {
            Assert.Equal(Bits(0.45359237d), Bits(MassUnits.KilogramsPerPound));
        }

        [Fact]
        public void ForceUnits_DeclaraLaGravedadEstandarYDerivaLaLibraFuerzaPorMultiplicacion()
        {
            Assert.Equal(Bits(9.80665d), Bits(ForceUnits.StandardGravity));
            Assert.Equal(Bits(MassUnits.KilogramsPerPound * ForceUnits.StandardGravity), Bits(ForceUnits.NewtonsPerPoundForce));
            AssertRelative(4.4482216152605d, ForceUnits.NewtonsPerPoundForce);
        }

        [Fact]
        public void ForceUnits_LaTablaEsCerradaYSusSimbolosSonOrdinales()
        {
            var expected = new[]
            {
                (ForceUnit.Newton, "N"), (ForceUnit.Kilonewton, "kN"), (ForceUnit.PoundForce, "lbf"),
                (ForceUnit.Kip, "kip"), (ForceUnit.KilogramForce, "kgf"),
            };

            Assert.Equal(expected.Select(e => e.Item1), ForceUnits.Units);

            foreach (var (unit, token) in expected)
            {
                Assert.Equal(token, ForceUnits.Token(unit));
                Assert.True(ForceUnits.TryParseToken(token, out var parsed));
                Assert.Equal(unit, parsed);
            }
        }

        [Theory]
        [InlineData("lb")]
        [InlineData("LBF")]
        [InlineData("kips")]
        [InlineData("n")]
        [InlineData("KN")]
        [InlineData("Kip")]
        [InlineData("KGF")]
        [InlineData("")]
        [InlineData(" N")]
        [InlineData(null)]
        public void ForceUnits_UnSimboloFueraDeLaTablaNoEsUnaUnidad(string token)
        {
            Assert.False(ForceUnits.TryParseToken(token, out _));
        }

        // ================================================================ INV-03 — force

        [Theory]
        [InlineData(1.0)]
        [InlineData(2.5)]
        [InlineData(1234.5678)]
        public void ForceUnits_ElKipEsMilLibrasFuerzaConUnaSolaMultiplicacionExacta(double kips)
        {
            Assert.Equal(Bits(kips * 1000d), Bits(ForceUnits.ToPoundsForce(kips, ForceUnit.Kip)));
            Assert.Equal(Bits(kips), Bits(ForceUnits.FromPoundsForce(kips * 1000d, ForceUnit.Kip)));
            Assert.Equal(2500d, ForceUnits.ToPoundsForce(2.5, ForceUnit.Kip));
        }

        [Theory]
        [InlineData(1.0)]
        [InlineData(7.25)]
        [InlineData(98765.4321)]
        public void ForceUnits_LibraFuerzaYNewtonUsanElMismoOperadorYElMismoFactor(double value)
        {
            Assert.Equal(Bits(value), Bits(ForceUnits.ToPoundsForce(value, ForceUnit.PoundForce)));
            Assert.Equal(Bits(value * ForceUnits.NewtonsPerPoundForce), Bits(ForceUnits.FromPoundsForce(value, ForceUnit.Newton)));
            Assert.Equal(Bits(value / ForceUnits.NewtonsPerPoundForce), Bits(ForceUnits.ToPoundsForce(value, ForceUnit.Newton)));
        }

        [Fact]
        public void ForceUnits_ElKilonewtonEsMilNewtons()
        {
            Assert.Equal(Bits(ForceUnits.ToPoundsForce(3000d, ForceUnit.Newton)), Bits(ForceUnits.ToPoundsForce(3d, ForceUnit.Kilonewton)));
            Assert.Equal(Bits(ForceUnits.FromPoundsForce(10d, ForceUnit.Newton) / 1000d), Bits(ForceUnits.FromPoundsForce(10d, ForceUnit.Kilonewton)));
        }

        [Fact]
        public void ForceUnits_ElKilogramoFuerzaEsUnKilogramoPorLaGravedadEstandar()
        {
            AssertRelative(1d / 0.45359237d, ForceUnits.ToPoundsForce(1d, ForceUnit.KilogramForce));
            AssertRelative(9.80665d, ForceUnits.FromPoundsForce(ForceUnits.ToPoundsForce(1d, ForceUnit.KilogramForce), ForceUnit.Newton));
            AssertRelative(
                ForceUnits.ToPoundsForce(9.80665d, ForceUnit.Newton),
                ForceUnits.ToPoundsForce(1d, ForceUnit.KilogramForce));
        }

        [Theory]
        [InlineData(ForceUnit.Newton, 1e-6)]
        [InlineData(ForceUnit.Newton, 0.5)]
        [InlineData(ForceUnit.Kilonewton, 123.456)]
        [InlineData(ForceUnit.PoundForce, 1e6)]
        [InlineData(ForceUnit.Kip, 0.3)]
        [InlineData(ForceUnit.Kip, 987.654)]
        [InlineData(ForceUnit.KilogramForce, 1)]
        [InlineData(ForceUnit.KilogramForce, 4321.0987)]
        public void ForceUnits_IdaYVueltaConTolerancia_1e12(ForceUnit unit, double value)
        {
            AssertRelative(value, ForceUnits.FromPoundsForce(ForceUnits.ToPoundsForce(value, unit), unit));
            AssertRelative(value, StructuralForce.From(value, unit).In(unit));
        }

        [Fact]
        public void StructuralForce_SoloSeCreaConUnidadYGuardaLibrasFuerza()
        {
            Assert.Equal(12.5, StructuralForce.FromPoundsForce(12.5).PoundsForce);
            Assert.Equal(Bits(ForceUnits.ToPoundsForce(4d, ForceUnit.Kilonewton)), Bits(StructuralForce.From(4d, ForceUnit.Kilonewton).PoundsForce));
            Assert.Equal(StructuralForce.FromPoundsForce(2500d), StructuralForce.From(2.5, ForceUnit.Kip));
            Assert.NotEqual(StructuralForce.FromPoundsForce(1d), StructuralForce.FromPoundsForce(2d));
        }

        // ================================================================ INV-03 — length

        [Theory]
        [InlineData(25.4)]
        [InlineData(1000.0)]
        [InlineData(1234.5678)]
        public void StructuralLength_LosMilimetrosPasanPorLaAutoridadDeLongitud(double millimeters)
        {
            Assert.Equal(
                Bits(LengthUnits.Authority.ToInches(millimeters, LengthUnit.Millimeter)),
                Bits(StructuralLength.FromMillimeters(millimeters).Inches));
            Assert.Equal(
                Bits(LengthUnits.Authority.ToInches(millimeters, LengthUnit.Millimeter)),
                Bits(StructuralLength.From(millimeters, LengthUnit.Millimeter).Inches));
        }

        [Fact]
        public void StructuralLength_LasEscalasExactasSeConservan()
        {
            Assert.Equal(1.0, StructuralLength.FromMillimeters(25.4).Inches);
            Assert.Equal(24.0, StructuralLength.From(2d, LengthUnit.Foot).Inches);
            Assert.Equal(7.5, StructuralLength.FromInches(7.5).Inches);
            Assert.Equal(StructuralLength.FromInches(0d), StructuralLength.Zero);
        }

        [Theory]
        [InlineData(0.001)]
        [InlineData(1.0)]
        [InlineData(97.3125)]
        [InlineData(12345.678)]
        public void StructuralLength_IdaYVueltaPulgadaMilimetroConTolerancia_1e12(double inches)
        {
            var millimeters = inches * LengthUnits.MillimetersPerInch;

            AssertRelative(inches, StructuralLength.FromMillimeters(millimeters).Inches);
        }

        // ================================================================ INV-02 — typed quantities

        [Fact]
        public void LasCantidadesNoTienenConstructorPublicoNiConversionDesdeDouble()
        {
            foreach (var type in QuantityTypes)
            {
                Assert.Empty(type.GetConstructors(BindingFlags.Public | BindingFlags.Instance));

                var conversions = type
                    .GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static)
                    .Where(m => m.Name == "op_Implicit" || m.Name == "op_Explicit")
                    .ToList();

                Assert.Empty(conversions);
                Assert.Empty(typeof(StructuralLength).Assembly.GetTypes()
                    .Where(t => t.Namespace == "RackCad.Application.StructuralAnalysis")
                    .SelectMany(t => t.GetMethods(BindingFlags.Public | BindingFlags.Static))
                    .Where(m => (m.Name == "op_Implicit" || m.Name == "op_Explicit") && m.ReturnType == type));
            }
        }

        [Fact]
        public void LasFabricasDeCantidadesNombranSuUnidad()
        {
            foreach (var type in QuantityTypes)
            {
                var factories = type.GetMethods(BindingFlags.Public | BindingFlags.Static)
                    .Where(m => m.ReturnType == type && m.GetParameters().Length >= 1 && m.GetParameters()[0].ParameterType == typeof(double))
                    .ToList();

                Assert.NotEmpty(factories);
                Assert.All(factories, factory => Assert.StartsWith("From", factory.Name, StringComparison.Ordinal));
            }
        }

        [Theory]
        [InlineData(double.NaN)]
        [InlineData(double.PositiveInfinity)]
        [InlineData(double.NegativeInfinity)]
        public void LasFabricasRechazanMagnitudesNoFinitas(double value)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => StructuralLength.FromInches(value));
            Assert.Throws<ArgumentOutOfRangeException>(() => StructuralLength.FromMillimeters(value));
            Assert.Throws<ArgumentOutOfRangeException>(() => StructuralForce.FromPoundsForce(value));
            Assert.Throws<ArgumentOutOfRangeException>(() => StructuralForce.From(value, ForceUnit.Newton));
            Assert.Throws<ArgumentOutOfRangeException>(() => StructuralArea.FromSquareInches(value));
            Assert.Throws<ArgumentOutOfRangeException>(() => StructuralMomentOfInertia.FromInchesToTheFourth(value));
        }

        [Fact]
        public void AreaEInerciaGuardanPulgadasSinConversionYSeComparanPorValor()
        {
            Assert.Equal(7.65, StructuralArea.FromSquareInches(7.65).SquareInches);
            Assert.Equal(204d, StructuralMomentOfInertia.FromInchesToTheFourth(204d).InchesToTheFourth);
            Assert.Equal(StructuralArea.FromSquareInches(3d), StructuralArea.FromSquareInches(3d));
            Assert.Equal(StructuralArea.FromSquareInches(3d).GetHashCode(), StructuralArea.FromSquareInches(3d).GetHashCode());
            Assert.True(StructuralMomentOfInertia.FromInchesToTheFourth(1d) != StructuralMomentOfInertia.FromInchesToTheFourth(2d));
        }

        // ================================================================ INV-16 — StructuralSectionUnits, bit for bit

        [Fact]
        public void StructuralSectionUnits_ElFactorDeLibraPorPieEsIdenticoBitABit()
        {
            Assert.Equal(Bits(0.45359237d / 0.3048d), Bits(StructuralSectionUnits.PoundsPerFootToKilogramsPerMeter));
        }

        [Theory]
        [InlineData(0.0)]
        [InlineData(1.0)]
        [InlineData(26.0)]
        [InlineData(13.37)]
        [InlineData(1234.5678)]
        public void StructuralSectionUnits_CadaConversionEsIdenticaBitABitAlValorAnterior(double value)
        {
            const double previousFactor = 0.45359237d / 0.3048d;

            Assert.Equal(Bits(value * previousFactor), Bits(StructuralSectionUnits.ToKilogramsPerMeter(value)));
            Assert.Equal(Bits(value / previousFactor), Bits(StructuralSectionUnits.ToPoundsPerFoot(value)));
            Assert.Equal(Bits(value * 25.4), Bits(StructuralSectionUnits.ToMillimeters(value)));
            Assert.Equal(Bits(value * 645.16), Bits(StructuralSectionUnits.ToSquareMillimeters(value)));
        }

        [Theory]
        [InlineData(StructuralSectionUnitSystem.UsCustomary)]
        [InlineData(StructuralSectionUnitSystem.Metric)]
        public void StructuralSectionUnits_LosPesosSonIdenticosBitABitAlValorAnterior(StructuralSectionUnitSystem system)
        {
            const double previousFactor = 0.45359237d / 0.3048d;
            const double length = 143.75;

            var section = new StructuralSectionDefinition
            {
                Identity = new StructuralSectionIdentity
                {
                    SectionId = StructuralSectionId.Parse("AISC-W-W12X26"),
                    Family = StructuralSectionFamily.W,
                    EdiDesignation = "W12X26",
                    ManualLabel = "W12X26",
                },
                WeightPerLength = 26.0,
                NativeUnitSystem = system,
            };

            var perFootInKilograms = system == StructuralSectionUnitSystem.UsCustomary ? 26.0 * previousFactor : 26.0;
            var perFootInPounds = system == StructuralSectionUnitSystem.UsCustomary ? 26.0 : 26.0 / previousFactor;

            Assert.Equal(Bits(perFootInKilograms), Bits(StructuralSectionUnits.WeightPerLengthInKilogramsPerMeter(section)));
            Assert.Equal(Bits(perFootInPounds), Bits(StructuralSectionUnits.WeightPerLengthInPoundsPerFoot(section)));
            Assert.Equal(Bits(perFootInKilograms * (length * 25.4 / 1000d)), Bits(StructuralSectionUnits.WeightInKilograms(section, length)));
            Assert.Equal(Bits(perFootInPounds * (length / 12d)), Bits(StructuralSectionUnits.WeightInPounds(section, length)));
        }

        [Fact]
        public void StructuralSectionUnits_LaApiPublicaNoCambia()
        {
            var actual = typeof(StructuralSectionUnits)
                .GetMembers(BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                .Select(member => member.MemberType + " " + member.Name)
                .OrderBy(text => text, StringComparer.Ordinal)
                .ToList();

            var expected = new[]
            {
                "Field InchesToMillimeters", "Field PoundsPerFootToKilogramsPerMeter", "Field SquareInchesToSquareMillimeters",
                "Method ToKilogramsPerMeter", "Method ToMillimeters", "Method ToPoundsPerFoot", "Method ToSquareMillimeters",
                "Method Weight", "Method WeightInKilograms", "Method WeightInPounds", "Method WeightPerLengthInKilogramsPerMeter",
                "Method WeightPerLengthInPoundsPerFoot",
            };

            Assert.Equal(expected.OrderBy(text => text, StringComparer.Ordinal), actual);
            Assert.True(typeof(StructuralSectionUnits).IsAbstract && typeof(StructuralSectionUnits).IsSealed, "debe seguir siendo una clase estática");
            Assert.Equal(typeof(double), typeof(StructuralSectionUnits).GetField(nameof(StructuralSectionUnits.PoundsPerFootToKilogramsPerMeter)).FieldType);
        }

        [Fact]
        public void StructuralSectionUnits_ConsumeMassUnitsEnLugarDeSuPropioLiteral()
        {
            var code = StructuralAnalysisGuards.Code(File.ReadAllText(Path.Combine(RepoRoot().FullName, SectionUnitsFile)));

            Assert.Contains("MassUnits.KilogramsPerPound", code, StringComparison.Ordinal);
            Assert.DoesNotContain("0.45359237", code, StringComparison.Ordinal);
        }

        [Fact]
        public void StructuralSectionUnits_EsElArchivoOriginalSalvoElReemplazoDelLiteral()
        {
            var text = File.ReadAllText(Path.Combine(RepoRoot().FullName, SectionUnitsFile)).Replace("\r\n", "\n", StringComparison.Ordinal);
            var hash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(text))).ToLowerInvariant();

            Assert.Equal(SectionUnitsExpectedSha256, hash);
        }

        // ================================================================ the quantities own no literal of the owners

        [Fact]
        public void ElProductoDeLaCapaNeutralConsumeLosPropietariosYNoDuplicaSusConstantes()
        {
            var forceCode = StructuralAnalysisGuards.Code(File.ReadAllText(Path.Combine(RepoRoot().FullName, "src/RackCad.Application/Units/ForceUnits.cs")));
            var massCode = StructuralAnalysisGuards.Code(File.ReadAllText(Path.Combine(RepoRoot().FullName, "src/RackCad.Application/Units/MassUnits.cs")));

            Assert.Contains("MassUnits.KilogramsPerPound", forceCode, StringComparison.Ordinal);
            Assert.DoesNotContain("0.45359237", forceCode, StringComparison.Ordinal);
            Assert.DoesNotContain("9.80665", massCode, StringComparison.Ordinal);
        }
    }
}
