using System;
using System.IO;
using System.Linq;
using System.Reflection;
using RackCad.Application.StructuralSections;
using RackCad.Application.Systems.Dynamic;
using RackCad.Application.Systems.Selective;
using RackCad.Application.Units;
using Xunit;
using static RackCad.Tests.ExpressionSemanticTestSupport;

namespace RackCad.Tests
{
    /// <summary>
    /// I-49 G6 — la autoridad neutral de unidades de longitud (V6 P9; ADR-0040 D3).
    ///
    /// <para>
    /// Un único componente de Application declara la tabla cerrada de unidades, sus tokens y SOLO las conversiones
    /// genéricas milímetros↔pulgadas y pies↔pulgadas. <c>StructuralSectionUnits</c> declara sus dos constantes como alias
    /// de ella, con los mismos bits. Las constantes de dominio que valen 12 —el redondeo del Selectivo, el pie comercial
    /// del Dinámico y la notación por 12 del Cantilever— se quedan locales: una constante pertenece a la autoridad por su
    /// papel de conversión, no por su valor (P9.6, T-V4-10).
    /// </para>
    /// </summary>
    public class LengthUnitsTests
    {
        [Fact]
        public void LA_AUTORIDAD_DECLARA_SOLO_LAS_DOS_CONVERSIONES_GENERICAS()
        {
            AssertBits(25.4, LengthUnits.MillimetersPerInch);
            AssertBits(12.0, LengthUnits.InchesPerFoot);

            Assert.Equal(
                new[] { "InchesPerFoot", "MillimetersPerInch" },
                typeof(LengthUnits)
                    .GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static)
                    .Where(field => field.IsLiteral)
                    .Select(field => field.Name)
                    .OrderBy(name => name, StringComparer.Ordinal));
        }

        [Fact]
        public void LOS_TOKENS_DE_UNIDAD_SON_UNA_TABLA_CERRADA_EN_LOS_DOS_SENTIDOS()
        {
            var units = LengthUnits.Authority;

            Assert.Same(units, LengthUnits.Authority);
            Assert.Equal(new[] { LengthUnit.Millimeter, LengthUnit.Inch, LengthUnit.Foot }, units.Units);
            Assert.Equal(new[] { 1, 2, 3 }, Enum.GetValues<LengthUnit>().Select(value => (int)value));

            foreach (var (token, unit) in new[] { ("mm", LengthUnit.Millimeter), ("in", LengthUnit.Inch), ("ft", LengthUnit.Foot) })
            {
                Assert.Equal(token, units.Token(unit));
                Assert.True(units.TryParseToken(token, out var parsed), token);
                Assert.Equal(unit, parsed);
            }

            foreach (var token in new[] { "MM", "In", "FT", "cm", "m", "inch", "\"", "'", string.Empty, " mm", "mm " })
            {
                Assert.False(units.TryParseToken(token, out _), "«" + token + "» no es un token de unidad");
            }

            Assert.False(units.TryParseToken(null, out _));
            Assert.Throws<ArgumentOutOfRangeException>(() => units.Token((LengthUnit)0));
        }

        /// <summary>P9.3: la operación está fijada —multiplicar por 12, dividir entre 25.4— para ser determinista bit a bit.</summary>
        [Fact]
        public void LAS_CONVERSIONES_A_PULGADAS_SON_EXACTAS_BIT_A_BIT()
        {
            var units = LengthUnits.Authority;

            AssertBits(4, units.ToInches(4, LengthUnit.Inch));
            AssertBits(240, units.ToInches(20, LengthUnit.Foot));
            AssertBits(100 / 25.4, units.ToInches(100, LengthUnit.Millimeter));
            AssertBits(1, units.ToInches(25.4, LengthUnit.Millimeter));
            AssertBits(6, units.ToInches(0.5, LengthUnit.Foot));

            Assert.Throws<ArgumentOutOfRangeException>(() => units.ToInches(1, (LengthUnit)0));
        }

        [Fact]
        public void LOS_ALIAS_DE_STRUCTURALSECTIONUNITS_VALEN_LO_MISMO_QUE_LA_AUTORIDAD()
        {
            AssertBits(LengthUnits.MillimetersPerInch, StructuralSectionUnits.InchesToMillimeters);

            var inchesPerFoot = typeof(StructuralSectionUnits).GetField("InchesPerFoot", BindingFlags.NonPublic | BindingFlags.Static);
            Assert.NotNull(inchesPerFoot);
            Assert.True(inchesPerFoot.IsLiteral);
            AssertBits(LengthUnits.InchesPerFoot, (double)inchesPerFoot.GetRawConstantValue());

            // La API y el comportamiento de StructuralSectionUnits no cambian.
            AssertBits(25.4, StructuralSectionUnits.ToMillimeters(1));
        }

        [Fact]
        public void LAS_CONSTANTES_DE_DOMINIO_QUE_VALEN_DOCE_SIGUEN_LOCALES_Y_SIN_CAMBIO()
        {
            AssertBits(12.0, SelectiveGeometryResolver.FootInches);
            AssertBits(12.0, DynamicHeaderHeightCalculator.CommercialFoot);

            var sistemas = Path.Combine(RepoRoot().FullName, "src", "RackCad.Application", "Systems");
            var cantilever = File.ReadAllText(Path.Combine(sistemas, "Cantilever", "CantileverArmFrameResolver.cs"));
            Assert.Contains("slopeRisePer12 / 12.0", cantilever, StringComparison.Ordinal);

            foreach (var archivo in new[]
                     {
                         Path.Combine(sistemas, "Selective", "SelectiveGeometryResolver.cs"),
                         Path.Combine(sistemas, "Dynamic", "DynamicHeaderHeightCalculator.cs"),
                         Path.Combine(sistemas, "Cantilever", "CantileverArmFrameResolver.cs"),
                     })
            {
                Assert.DoesNotContain("LengthUnits", File.ReadAllText(archivo), StringComparison.Ordinal);
            }
        }
    }
}
