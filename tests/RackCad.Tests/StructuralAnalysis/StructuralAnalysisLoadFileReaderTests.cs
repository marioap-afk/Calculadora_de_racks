using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using RackCad.Application.StructuralAnalysis;
using RackCad.Application.Units;
using Xunit;
using F = RackCad.Tests.StructuralAnalysisFixtures;

namespace RackCad.Tests
{
    /// <summary>
    /// I-65 F1-T1 — INV-15, D-10b and CD-I65-F1-05: the strict reader of <c>rackcad-structural-loads/v1</c>. A magnitude
    /// without a unit is never accepted, every deviation from the exact shape is Invalid and the file enters the
    /// fingerprint through the SHA-256 of its bytes.
    /// </summary>
    public class StructuralAnalysisLoadFileReaderTests
    {
        public static IEnumerable<object[]> InvalidFiles()
        {
            // unit: absent, unknown, ambiguous
            yield return Case("unit ausente", F.File(F.Load(unit: null)), StructuralDiagnosticCodes.LoadsUnitMissing);
            yield return Case("unit lb ambigua", F.File(F.Load(unit: "\"lb\"")), StructuralDiagnosticCodes.LoadsUnitUnknown);
            yield return Case("unit LBF por mayusculas", F.File(F.Load(unit: "\"LBF\"")), StructuralDiagnosticCodes.LoadsUnitUnknown);
            yield return Case("unit kips", F.File(F.Load(unit: "\"kips\"")), StructuralDiagnosticCodes.LoadsUnitUnknown);
            yield return Case("unit n minuscula", F.File(F.Load(unit: "\"n\"")), StructuralDiagnosticCodes.LoadsUnitUnknown);
            yield return Case("unit vacia", F.File(F.Load(unit: "\"\"")), StructuralDiagnosticCodes.LoadsUnitUnknown);
            yield return Case("unit numerica", F.File(F.Load(unit: "5")), StructuralDiagnosticCodes.LoadsWrongType);

            // position
            yield return Case("fraccion mayor que 1", F.File(F.Load(position: "1.0001")), StructuralDiagnosticCodes.LoadsPositionRange);
            yield return Case("fraccion negativa", F.File(F.Load(position: "-0.01")), StructuralDiagnosticCodes.LoadsPositionRange);
            yield return Case("fraccion como texto", F.File(F.Load(position: "\"0.5\"")), StructuralDiagnosticCodes.LoadsWrongType);
            yield return Case("positionBasis distinto", F.File(F.Load(basis: "\"absoluteDistance\"")), StructuralDiagnosticCodes.LoadsPositionBasis);

            // direction and magnitude
            yield return Case("direccion +Z", F.File(F.Load(direction: "\"+Z\"")), StructuralDiagnosticCodes.LoadsDirection);
            yield return Case("direccion -Y", F.File(F.Load(direction: "\"-Y\"")), StructuralDiagnosticCodes.LoadsDirection);
            yield return Case("magnitud cero", F.File(F.Load(magnitude: "0")), StructuralDiagnosticCodes.LoadsMagnitude);
            yield return Case("magnitud negativa", F.File(F.Load(magnitude: "-5")), StructuralDiagnosticCodes.LoadsMagnitude);
            yield return Case("magnitud no finita", F.File(F.Load(magnitude: "1e999")), StructuralDiagnosticCodes.LoadsMagnitude);
            yield return Case("magnitud como texto", F.File(F.Load(magnitude: "\"10\"")), StructuralDiagnosticCodes.LoadsWrongType);

            // properties and types
            yield return Case("propiedad desconocida en la carga", F.File(F.Load(extra: "\"note\":1")), StructuralDiagnosticCodes.LoadsUnknownProperty);
            yield return Case("propiedad desconocida en la raiz", F.File(new[] { F.Load() }, extra: "\"extra\":true"), StructuralDiagnosticCodes.LoadsUnknownProperty);
            yield return Case("propiedad desconocida en target", F.File(F.Load(target: "{\"level\":1,\"side\":\"PositiveY\",\"deck\":2}")), StructuralDiagnosticCodes.LoadsUnknownProperty);
            yield return Case("propiedad duplicada", F.File(F.Load(extra: "\"unit\":\"kN\"")), StructuralDiagnosticCodes.LoadsDuplicateProperty);
            yield return Case("propiedad duplicada en la raiz", F.File(new[] { F.Load() }, extra: "\"schema\":\"x\""), StructuralDiagnosticCodes.LoadsDuplicateProperty);
            yield return Case("propiedad faltante", F.File(F.Load(direction: null)), StructuralDiagnosticCodes.LoadsMissingProperty);
            yield return Case("id como numero", F.File(F.Load(id: "7")), StructuralDiagnosticCodes.LoadsWrongType);
            yield return Case("loads no es lista", "{\"schema\":\"rackcad-structural-loads/v1\",\"loads\":{}}", StructuralDiagnosticCodes.LoadsWrongType);

            // list, ids and schema
            yield return Case("lista vacia", F.File(new string[0]), StructuralDiagnosticCodes.LoadsEmpty);
            yield return Case("id vacio", F.File(F.Load(id: "\"\"")), StructuralDiagnosticCodes.LoadsIdEmpty);
            yield return Case("id repetido", F.File(F.Load(), F.Load(position: "0.1")), StructuralDiagnosticCodes.LoadsIdDuplicate);
            yield return Case("schema distinto", F.File(new[] { F.Load() }, schema: "\"rackcad-structural-loads/v2\""), StructuralDiagnosticCodes.LoadsSchemaMismatch);
            yield return Case("schema sin version", F.File(new[] { F.Load() }, schema: "\"rackcad-structural-loads\""), StructuralDiagnosticCodes.LoadsSchemaMismatch);

            // target
            yield return Case("nivel cero", F.File(F.Load(level: "0")), StructuralDiagnosticCodes.LoadsTargetInvalid);
            yield return Case("lado desconocido", F.File(F.Load(side: "\"Z\"")), StructuralDiagnosticCodes.LoadsTargetInvalid);
            yield return Case("lado por mayusculas", F.File(F.Load(side: "\"positiveY\"")), StructuralDiagnosticCodes.LoadsTargetInvalid);
            yield return Case("nivel como texto", F.File(F.Load(level: "\"1\"")), StructuralDiagnosticCodes.LoadsWrongType);

            // not JSON at all
            yield return Case("json truncado", "{\"schema\":", StructuralDiagnosticCodes.LoadsJsonMalformed);
            yield return Case("coma final", "{\"schema\":\"rackcad-structural-loads/v1\",\"loads\":[],}", StructuralDiagnosticCodes.LoadsJsonMalformed);
            yield return Case("comentario", "{/*x*/\"schema\":\"rackcad-structural-loads/v1\",\"loads\":[]}", StructuralDiagnosticCodes.LoadsJsonMalformed);
            yield return Case("vacio", string.Empty, StructuralDiagnosticCodes.LoadsJsonMalformed);
            yield return Case("raiz no es objeto", "[]", StructuralDiagnosticCodes.LoadsWrongType);
        }

        private static object[] Case(string name, string json, string code) => new object[] { name, json, code };

        [Theory]
        [MemberData(nameof(InvalidFiles))]
        public void CadaDesviacionDeLaFormaExactaEsInvalidYNoDejaArchivo(string name, string json, string expectedCode)
        {
            var result = F.Read(json);

            Assert.True(StructuralExportOutcome.Invalid == result.Outcome, name + ": se esperaba Invalid");
            Assert.Null(result.LoadFile);
            Assert.True(result.Diagnostics.Any(d => d.Code == expectedCode), name + ": se esperaba " + expectedCode + ", hay " + string.Join(",", result.Diagnostics.Select(d => d.Code)));
            Assert.All(result.Diagnostics, d => Assert.True(d.IsBlocking && d.ImpliedOutcome == StructuralExportOutcome.Invalid));
            Assert.All(result.Diagnostics, d => Assert.False(string.IsNullOrWhiteSpace(d.Subject)));
        }

        [Fact]
        public void UnArchivoNuloOSinBytesEsInvalid()
        {
            Assert.Equal(StructuralExportOutcome.Invalid, StructuralLoadFileReader.Read(null).Outcome);
            Assert.Equal(StructuralExportOutcome.Invalid, StructuralLoadFileReader.Read(new byte[0]).Outcome);
        }

        [Fact]
        public void UnArchivoValidoSeLeeConSusCargasEnOrdenYConvertidasALibrasFuerza()
        {
            var result = F.Read(F.File(
                F.Load(id: "\"A\"", level: "1", side: "\"PositiveY\"", position: "0", magnitude: "100", unit: "\"N\""),
                F.Load(id: "\"B\"", level: "2", side: "\"NegativeY\"", position: "1", magnitude: "2.5", unit: "\"kip\""),
                F.Load(id: "\"C\"", level: "3", position: "0.125", magnitude: "7", unit: "\"kgf\"")));

            Assert.Equal(StructuralExportOutcome.Valid, result.Outcome);
            Assert.Empty(result.Diagnostics);
            Assert.NotNull(result.LoadFile);
            Assert.Equal(new[] { "A", "B", "C" }, result.LoadFile.Loads.Select(l => l.Id));
            Assert.Equal(new[] { 1, 2, 3 }, result.LoadFile.Loads.Select(l => l.Level));
            Assert.Equal(StructuralLoadSide.NegativeY, result.LoadFile.Loads[1].Side);
            Assert.Equal(new[] { 0.0, 1.0, 0.125 }, result.LoadFile.Loads.Select(l => l.PositionFraction));
            Assert.Equal(
                BitConverter.DoubleToInt64Bits(ForceUnits.ToPoundsForce(100, ForceUnit.Newton)),
                BitConverter.DoubleToInt64Bits(result.LoadFile.Loads[0].Magnitude.PoundsForce));
            Assert.Equal(2500.0, result.LoadFile.Loads[1].Magnitude.PoundsForce);
            Assert.Equal(StructuralForce.From(7, ForceUnit.KilogramForce), result.LoadFile.Loads[2].Magnitude);
        }

        [Theory]
        [InlineData("N", 1.0)]
        [InlineData("kN", 1.0)]
        [InlineData("lbf", 1.0)]
        [InlineData("kip", 1.0)]
        [InlineData("kgf", 1.0)]
        public void LasCincoUnidadesDeLaTablaSeAceptan(string unit, double magnitude)
        {
            var result = F.Read(F.File(F.Load(magnitude: magnitude.ToString("R", System.Globalization.CultureInfo.InvariantCulture), unit: "\"" + unit + "\"")));

            Assert.Equal(StructuralExportOutcome.Valid, result.Outcome);
            Assert.True(ForceUnits.TryParseToken(unit, out var parsed));
            Assert.Equal(StructuralForce.From(magnitude, parsed), result.LoadFile.Loads.Single().Magnitude);
        }

        [Fact]
        public void ElHashDelArchivoEsElSha256DeSusBytesExactos()
        {
            var bytes = F.Bytes(F.File(F.Load(), F.Load(id: "\"L2\"")));
            var result = StructuralLoadFileReader.Read(bytes);
            var expected = Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();

            Assert.Equal(expected, result.LoadFile.Sha256);
            Assert.Matches("^[0-9a-f]{64}$", result.LoadFile.Sha256);
            Assert.Equal(StructuralFingerprints.Sha256Hex(bytes), result.LoadFile.Sha256);
        }

        [Fact]
        public void DosArchivosConElMismoContenidoLogicoPeroDistintosBytesTienenDistintoHash()
        {
            var compact = F.Bytes(F.File(F.Load()));
            var spaced = F.Bytes(F.File(F.Load()).Replace(",", ", ", StringComparison.Ordinal));

            Assert.NotEqual(StructuralLoadFileReader.Read(compact).LoadFile.Sha256, StructuralLoadFileReader.Read(spaced).LoadFile.Sha256);
        }

        [Fact]
        public void UnBomUtf8SeToleraYElHashCubreLosBytesOriginalesIncluidoElBom()
        {
            var json = F.Bytes(F.File(F.Load()));
            var withBom = new byte[] { 0xEF, 0xBB, 0xBF }.Concat(json).ToArray();
            var result = StructuralLoadFileReader.Read(withBom);

            Assert.Equal(StructuralExportOutcome.Valid, result.Outcome);
            Assert.Equal(StructuralFingerprints.Sha256Hex(withBom), result.LoadFile.Sha256);
            Assert.NotEqual(StructuralFingerprints.Sha256Hex(json), result.LoadFile.Sha256);
        }

        [Fact]
        public void LosHallazgosDeVariasCargasSeAcumulanSinDetenerseEnElPrimero()
        {
            var result = F.Read(F.File(
                F.Load(id: "\"A\"", unit: null),
                F.Load(id: "\"B\"", position: "2"),
                F.Load(id: "\"C\"", direction: "\"+Z\"")));

            var codes = result.Diagnostics.Select(d => d.Code).ToList();

            Assert.Equal(StructuralExportOutcome.Invalid, result.Outcome);
            Assert.Contains(StructuralDiagnosticCodes.LoadsUnitMissing, codes);
            Assert.Contains(StructuralDiagnosticCodes.LoadsPositionRange, codes);
            Assert.Contains(StructuralDiagnosticCodes.LoadsDirection, codes);
        }

        [Fact]
        public void LaLecturaEsEstrictaNoCoercionaNiAdivina()
        {
            // The schema id is exactly the contract's, and a number is never accepted as text or the reverse.
            Assert.Equal("rackcad-structural-loads/v1", StructuralLoadFileReader.SchemaId);
            Assert.Equal(StructuralExportOutcome.Invalid, F.Read(F.File(F.Load(level: "1.5"))).Outcome);
            Assert.Equal(StructuralExportOutcome.Invalid, F.Read(F.File(F.Load(magnitude: "true"))).Outcome);
            Assert.Equal(StructuralExportOutcome.Invalid, F.Read(F.File(F.Load(magnitude: "null"))).Outcome);
            Assert.Equal(StructuralExportOutcome.Valid, F.Read(F.File(F.Load(position: "5e-1"))).Outcome);
        }
    }
}
