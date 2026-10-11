using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using RackCad.Application.StructuralAnalysis;
using RackCad.Application.StructuralAnalysis.Providers.RamElements;
using Xunit;
using Codes = RackCad.Application.StructuralAnalysis.Providers.RamElements.RamDumpDiagnosticCodes;
using S = RackCad.Tests.StructuralAnalysisRamElementsDumpSupport;

namespace RackCad.Tests
{
    /// <summary>
    /// I-65 F4-T3 — D-11, D-04 and A65-F4P-02: the strict reader of <c>rackcad-ram-dump/v1</c>. It receives TEXT (no I/O), rejects what is not
    /// exactly the contract (unknown, duplicate or wrongly typed properties, a failure status, a pair of units other than LB_IN) and
    /// produces the neutral readback: neutral frame, typed inches and pounds-force, loads as the absolute distance from node I.
    /// </summary>
    public class StructuralAnalysisRamElementsDumpReaderTests
    {
        private const double Tolerance = 1e-9;

        private static StructuralModelReadback ReadValid(JsonObject dump)
        {
            var result = S.Read(dump);

            Assert.True(
                result.Outcome == StructuralExportOutcome.Valid && result.Readback != null,
                "Se esperaba Valid; resultado " + result.Outcome + ": " + S.Codes(result));
            return result.Readback;
        }

        private static JsonObject WithCombination()
        {
            var dump = S.Valid();
            ((JsonArray)dump["combinations"]).Add(S.Combination("COMB1", ("REF1", 1.5)));
            return dump;
        }

        // ---------------------------------------------------------------- the valid reading

        [Fact]
        public void UnVolcadoValidoDaElModeloLeidoSinDiagnosticosYConLosDatosDelVolcado()
        {
            var result = S.Read(S.Valid());

            Assert.Equal(StructuralExportOutcome.Valid, result.Outcome);
            Assert.Empty(result.Diagnostics);

            var readback = result.Readback;

            Assert.Equal(S.Version, readback.AdapterVersion);
            Assert.Equal("LB_IN", readback.InitialUnitPair);
            Assert.False(readback.UnitsRestored);
            Assert.Equal(new[] { "11", "12" }, readback.Nodes.Select(n => n.Id).ToArray());
            Assert.Equal(new[] { "N1", "N2" }, readback.Nodes.Select(n => n.Description).ToArray());

            var member = Assert.Single(readback.Members);

            Assert.Equal("21", member.Id);
            Assert.Equal("M1", member.Description);
            Assert.Equal("11", member.NodeIId);
            Assert.Equal("12", member.NodeJId);
            Assert.Equal("W 12X26", member.SectionName);
            Assert.Equal("A992 Gr50", member.MaterialName);
            Assert.Equal(0, member.TwistDegrees);
            Assert.Equal(0, member.CardinalPoint);
            Assert.Equal(StructuralReadbackAxialBehavior.Normal, member.AxialBehavior);
            Assert.Equal(new[] { 1.0, 1.0, 1.0, 1.0 }, member.StiffnessFactors.ToArray());
            Assert.Equal(10, member.Releases.Count);
            Assert.DoesNotContain(true, member.Releases);
            Assert.Equal(0, member.RotationalSpringI);
            Assert.Equal(0, member.RotationalSpringJ);

            var loadCase = Assert.Single(readback.LoadCases);

            Assert.Equal("REF1", loadCase.Id);
            Assert.Equal("OTHER", loadCase.Category);
            Assert.Empty(readback.Combinations);
        }

        [Fact]
        public void LasCombinacionesSeLeenConSusTerminos()
        {
            var combination = Assert.Single(ReadValid(WithCombination()).Combinations);

            Assert.Equal("COMB1", combination.Id);

            var term = Assert.Single(combination.Terms);

            Assert.Equal("REF1", term.CaseId);
            Assert.Equal(1.5, term.Factor);
        }

        [Fact]
        public void UnVolcadoSinCargasNiCombinacionesNiMiembrosEsValido()
        {
            var dump = S.Valid();
            dump["members"] = new JsonArray();
            dump["loadCases"] = new JsonArray(S.Case("REF1", "OTHER"));

            var readback = ReadValid(dump);

            Assert.Empty(readback.Members);
            Assert.Empty(Assert.Single(readback.LoadCases).Loads);
        }

        [Fact]
        public void LaLecturaConservaElOrdenDeLosNudosLosMiembrosYLasCargas()
        {
            var dump = S.Valid();
            ((JsonArray)dump["nodes"]).Add(S.Node(13, "N3", 5, 0, 0, false, false, false, false, false, false));
            ((JsonArray)dump["members"]).Add(S.Member(22, "M2", 11, 13, new[] { 1.0, 0.0, 0.0 }, new[] { 0.0, 1.0, 0.0 }, new[] { 0.0, 0.0, 1.0 }));
            dump["loadCases"] = new JsonArray(
                S.Case(
                    "REF1",
                    "OTHER",
                    S.MemberLoads(21, S.Load("Y", -1, 10, true), S.Load("Y", -2, 20, true)),
                    S.MemberLoads(22, S.Load("Y", -3, 50, true))));

            var readback = ReadValid(dump);
            var loads = readback.LoadCases[0].Loads;

            Assert.Equal(new[] { "11", "12", "13" }, readback.Nodes.Select(n => n.Id).ToArray());
            Assert.Equal(new[] { "21", "22" }, readback.Members.Select(m => m.Id).ToArray());
            Assert.Equal(new[] { "21", "21", "22" }, loads.Select(l => l.MemberId).ToArray());
            Assert.Equal(new[] { -1.0, -2.0, -3.0 }, loads.Select(l => l.Value.PoundsForce).ToArray());
        }

        // ---------------------------------------------------------------- the neutral frame (inverse of D-04)

        [Fact]
        public void LosPuntosVuelvenAlMarcoNeutralConXIgualXYIgualMenosZYZIgualY()
        {
            var dump = S.Valid();
            ((JsonArray)dump["nodes"]).Add(S.Node(13, "N3", 1, 2, 3, false, false, false, false, false, false));

            var position = ReadValid(dump).Nodes.Single(n => n.Id == "13").Position;

            Assert.Equal(1, position.X.Inches, 12);
            Assert.Equal(-3, position.Y.Inches, 12);
            Assert.Equal(2, position.Z.Inches, 12);
        }

        [Theory]
        [InlineData(0, 0)]
        [InlineData(1, 2)]
        [InlineData(2, 1)]
        [InlineData(3, 3)]
        [InlineData(4, 5)]
        [InlineData(5, 4)]
        public void LasBanderasDeApoyoVuelvenAlOrdenNeutralTxTyTzRxRyRz(int programIndex, int neutralIndex)
        {
            // program order dx dy dz rx ry rz; neutral order Tx Ty Tz Rx Ry Rz: Tx = dx, Ty = dz, Tz = dy, Rx = rx, Ry = rz, Rz = ry
            var flags = new bool[6];
            flags[programIndex] = true;

            var dump = S.Valid();
            ((JsonArray)dump["nodes"]).Add(S.Node(13, "N3", 0, 0, 0, flags));

            var neutral = ReadValid(dump).Nodes.Single(n => n.Id == "13").SupportFlags;

            Assert.Equal(6, neutral.Count);
            Assert.Equal(Enumerable.Range(0, 6).Select(i => i == neutralIndex).ToArray(), neutral.ToArray());
        }

        [Fact]
        public void LosEjesLocalesVuelvenAlMarcoNeutralComoVectores()
        {
            var member = Assert.Single(ReadValid(S.Valid()).Members);

            AssertDirection(0, 0, 1, member.Axis1);
            AssertDirection(-1, 0, 0, member.Axis2);
            AssertDirection(0, -1, 0, member.Axis3);
        }

        [Fact]
        public void LosOffsetsYElPesoPropioVuelvenAlMarcoNeutralYSeTipan()
        {
            var dump = S.Valid();
            var member = dump["members"].ObjectAt(0);
            member["offsetI"] = S.Numbers(1, 2, 3);
            member["offsetJ"] = S.Numbers(-4, 5, -6);
            dump["loadCases"].ObjectAt(0)["selfWeight"] = S.Numbers(0, -1, 0);

            var readback = ReadValid(dump);
            var read = readback.Members[0];

            Assert.Equal(1, read.OffsetI.X.Inches, 12);
            Assert.Equal(-3, read.OffsetI.Y.Inches, 12);
            Assert.Equal(2, read.OffsetI.Z.Inches, 12);
            Assert.Equal(-4, read.OffsetJ.X.Inches, 12);
            Assert.Equal(6, read.OffsetJ.Y.Inches, 12);
            Assert.Equal(5, read.OffsetJ.Z.Inches, 12);
            AssertDirection(0, 0, -1, readback.LoadCases[0].SelfWeight);
        }

        [Fact]
        public void LosValoresQueNoCambianDeMarcoSeConservan()
        {
            var dump = S.Valid();
            var member = dump["members"].ObjectAt(0);
            member["rotation"] = -90.0;
            member["cardinalPoint"] = 5;
            member["stiffnessFactors"] = S.Numbers(0.9, 0.8, 0.7, 0.6);
            member["axialBehavior"] = 3;
            member["springI"] = 1.25;
            member["springJ"] = 2.5;
            member["releases"] = S.Bools(true, false, true, false, true, false, true, false, true, false);

            var read = ReadValid(dump).Members[0];

            Assert.Equal(-90.0, read.TwistDegrees);
            Assert.Equal(5, read.CardinalPoint);
            Assert.Equal(new[] { 0.9, 0.8, 0.7, 0.6 }, read.StiffnessFactors.ToArray());
            Assert.Equal(StructuralReadbackAxialBehavior.Cable, read.AxialBehavior);
            Assert.Equal(1.25, read.RotationalSpringI);
            Assert.Equal(2.5, read.RotationalSpringJ);
            Assert.Equal(new[] { true, false, true, false, true, false, true, false, true, false }, read.Releases.ToArray());
        }

        [Theory]
        [InlineData("X", StructuralReadbackLoadAxis.GlobalX, 7.0)]
        [InlineData("Y", StructuralReadbackLoadAxis.GlobalZ, 7.0)]
        [InlineData("Z", StructuralReadbackLoadAxis.GlobalY, -7.0)]
        [InlineData("1", StructuralReadbackLoadAxis.Local1, 7.0)]
        [InlineData("2", StructuralReadbackLoadAxis.Local2, 7.0)]
        [InlineData("3", StructuralReadbackLoadAxis.Local3, 7.0)]
        public void LaDireccionDeLaCargaVuelveAlMarcoNeutralConSuSigno(string direction, StructuralReadbackLoadAxis axis, double value)
        {
            var dump = S.Valid();
            dump["loadCases"] = new JsonArray(S.Case("REF1", "OTHER", S.MemberLoads(21, S.Load(direction, 7, 25, true))));

            var load = Assert.Single(ReadValid(dump).LoadCases[0].Loads);

            Assert.Equal(axis, load.Axis);
            Assert.Equal(value, load.Value.PoundsForce, 12);
            Assert.Equal("21", load.MemberId);
        }

        [Fact]
        public void UnaDireccionDeCargaDesconocidaEsInvalida()
        {
            var dump = S.Valid();
            dump["loadCases"] = new JsonArray(S.Case("REF1", "OTHER", S.MemberLoads(21, S.Load("W", 7, 25, true))));

            var result = S.Read(dump);

            Assert.Equal(StructuralExportOutcome.Invalid, result.Outcome);
            Assert.Null(result.Readback);
            Assert.Contains(result.Diagnostics, d => d.Code == Codes.DumpLoadDirection);
        }

        // ---------------------------------------------------------------- the position of the load: the absolute distance from node I

        [Theory]
        [InlineData(25.0, 25.0)]
        [InlineData(0.0, 0.0)]
        [InlineData(100.0, 100.0)]
        [InlineData(33.5, 33.5)]
        public void ElPorcentajeSeConvierteEnDistanciaAbsolutaPorLaLongitudDelMiembro(double percent, double expectedInches)
        {
            var dump = S.Valid();
            dump["loadCases"] = new JsonArray(S.Case("REF1", "OTHER", S.MemberLoads(21, S.Load("Y", -1, percent, true))));

            var load = Assert.Single(ReadValid(dump).LoadCases[0].Loads);

            Assert.Equal(expectedInches, load.DistanceFromNodeI.Inches, 9);
        }

        [Fact]
        public void LaLongitudDelMiembroSalePorLaDistanciaEntreSusNudosEnCualquierDireccion()
        {
            var dump = S.Valid();
            dump["nodes"] = new JsonArray(
                S.Node(11, "N1", 0, 0, 0, false, false, false, false, false, false),
                S.Node(12, "N2", 30, 40, 0, false, false, false, false, false, false));
            dump["loadCases"] = new JsonArray(S.Case("REF1", "OTHER", S.MemberLoads(21, S.Load("Y", -1, 10, true))));

            var load = Assert.Single(ReadValid(dump).LoadCases[0].Loads);

            Assert.Equal(5.0, load.DistanceFromNodeI.Inches, 9);
        }

        [Fact]
        public void UnaDistanciaQueNoEsPorcentajeSeConservaTalCualEnPulgadas()
        {
            var dump = S.Valid();
            dump["loadCases"] = new JsonArray(S.Case("REF1", "OTHER", S.MemberLoads(21, S.Load("Y", -1, 40, false))));

            var load = Assert.Single(ReadValid(dump).LoadCases[0].Loads);

            Assert.Equal(40.0, load.DistanceFromNodeI.Inches, 9);
        }

        [Fact]
        public void UnPorcentajeEnUnMiembroSinLongitudEsInvalido()
        {
            var dump = S.Valid();
            dump["nodes"] = new JsonArray(
                S.Node(11, "N1", 0, 0, 0, false, false, false, false, false, false),
                S.Node(12, "N2", 0, 0, 0, false, false, false, false, false, false));

            var result = S.Read(dump);

            Assert.Equal(StructuralExportOutcome.Invalid, result.Outcome);
            Assert.Null(result.Readback);
            Assert.Contains(result.Diagnostics, d => d.Code == Codes.DumpMemberGeometry);
        }

        // ---------------------------------------------------------------- a failure status is a rejection

        [Fact]
        public void UnVolcadoConEstadoDeFalloSeRechazaConSuPropioCodigo()
        {
            var dump = S.Valid();
            dump["status"] = RamDumpScriptTemplate.StatusFailed;
            dump["errors"] = new JsonArray("gate: package version is '0.0.1', expected '0.1.0'");

            var result = S.Read(dump);

            Assert.NotEqual(StructuralExportOutcome.Valid, result.Outcome);
            Assert.Null(result.Readback);
            Assert.Contains(result.Diagnostics, d => d.Code == Codes.DumpFailed && d.IsBlocking);
        }

        [Fact]
        public void ElRespaldoMinimoDeUnFalloTambienSeRechazaConElCodigoDeFallo()
        {
            var backup = new JsonObject
            {
                ["schema"] = RamDumpReader.Schema,
                ["status"] = RamDumpScriptTemplate.StatusFailed,
                ["adapterVersion"] = S.Version,
                ["errors"] = new JsonArray("dump: ValueError: Out of range float values are not JSON compliant"),
            };

            var result = RamDumpReader.Read(backup.ToJsonString());

            Assert.NotEqual(StructuralExportOutcome.Valid, result.Outcome);
            Assert.Null(result.Readback);
            Assert.Contains(result.Diagnostics, d => d.Code == Codes.DumpFailed);
        }

        [Theory]
        [InlineData("DUMP OK ")]
        [InlineData("dump ok")]
        [InlineData("OK")]
        [InlineData("")]
        public void CualquierEstadoQueNoSeaExactamenteDumpOkSeRechazaComoFallo(string status)
        {
            var dump = S.Valid();
            dump["status"] = status;

            var result = S.Read(dump);

            Assert.NotEqual(StructuralExportOutcome.Valid, result.Outcome);
            Assert.Null(result.Readback);
            Assert.Contains(result.Diagnostics, d => d.Code == Codes.DumpFailed);
        }

        // ---------------------------------------------------------------- the pair of units (D-11)

        [Theory]
        [InlineData("KIP_IN")]
        [InlineData("N_MM")]
        [InlineData("lb-in")]
        [InlineData("")]
        public void UnVolcadoNoLeidoEnLbInEsInvalidoConUnCodigoQueNombraElParDeUnidades(string pair)
        {
            var dump = S.Valid();
            dump["readUnits"] = pair;

            var result = S.Read(dump);

            Assert.Equal(StructuralExportOutcome.Invalid, result.Outcome);
            Assert.Null(result.Readback);
            Assert.Contains(result.Diagnostics, d => d.Code == Codes.DumpUnitPair && d.IsBlocking);
            Assert.Contains("UNIT-PAIR", Codes.DumpUnitPair, StringComparison.Ordinal);
        }

        [Fact]
        public void UnParInicialDistintoDeLbInRestauradoSeAceptaYSeRegistra()
        {
            var dump = S.Valid();
            dump["initialUnits"] = "KIP_IN";
            dump["unitsRestored"] = true;

            var readback = ReadValid(dump);

            Assert.Equal("KIP_IN", readback.InitialUnitPair);
            Assert.True(readback.UnitsRestored);
        }

        [Fact]
        public void UnParInicialDistintoQueNoSeRestauroEsInvalido()
        {
            var dump = S.Valid();
            dump["initialUnits"] = "KIP_IN";
            dump["unitsRestored"] = false;

            var result = S.Read(dump);

            Assert.Equal(StructuralExportOutcome.Invalid, result.Outcome);
            Assert.Null(result.Readback);
            Assert.Contains(result.Diagnostics, d => d.Code == Codes.DumpUnitsInconsistent);
        }

        [Fact]
        public void UnaRestauracionDeUnParQueNuncaSeCambioEsInvalida()
        {
            var dump = S.Valid();
            dump["unitsRestored"] = true;

            var result = S.Read(dump);

            Assert.Equal(StructuralExportOutcome.Invalid, result.Outcome);
            Assert.Contains(result.Diagnostics, d => d.Code == Codes.DumpUnitsInconsistent);
        }

        // ---------------------------------------------------------------- strict: every variant is Invalid with its own code

        private static readonly Dictionary<string, (Func<JsonObject, string> Mutate, string Code)> Mutations = BuildMutations();

        public static IEnumerable<object[]> Variants() => Mutations.Keys.Select(name => new object[] { name });

        private static (Func<JsonObject, string> Mutate, string Code) Json(Action<JsonObject> change, string code) =>
            (dump => { change(dump); return dump.ToJsonString(); }, code);

        private static (Func<JsonObject, string> Mutate, string Code) Text(Func<string, string> change, string code) =>
            (dump => change(dump.ToJsonString()), code);

        private static JsonObject NodeOf(JsonObject dump) => dump["nodes"].ObjectAt(0);

        private static JsonObject MemberOf(JsonObject dump) => dump["members"].ObjectAt(0);

        private static JsonObject CaseOf(JsonObject dump) => dump["loadCases"].ObjectAt(0);

        private static JsonObject LoadOf(JsonObject dump) => CaseOf(dump)["memberLoads"].ObjectAt(0)["loads"].ObjectAt(0);

        private static JsonObject CombinationOf(JsonObject dump) => dump["combinations"].ObjectAt(0);

        private static Dictionary<string, (Func<JsonObject, string> Mutate, string Code)> BuildMutations()
        {
            var all = new Dictionary<string, (Func<JsonObject, string>, string)>(StringComparer.Ordinal);

            // unknown properties
            all["propiedad desconocida en la raiz"] = Json(d => d["extra"] = 1, Codes.DumpUnknownProperty);
            all["propiedad desconocida en un nudo"] = Json(d => NodeOf(d)["extra"] = 1, Codes.DumpUnknownProperty);
            all["propiedad desconocida en un miembro"] = Json(d => MemberOf(d)["extra"] = 1, Codes.DumpUnknownProperty);
            all["propiedad desconocida en un caso"] = Json(d => CaseOf(d)["extra"] = 1, Codes.DumpUnknownProperty);
            all["propiedad desconocida en las cargas de un miembro"] = Json(d => CaseOf(d)["memberLoads"].ObjectAt(0)["extra"] = 1, Codes.DumpUnknownProperty);
            all["propiedad desconocida en una carga"] = Json(d => LoadOf(d)["extra"] = 1, Codes.DumpUnknownProperty);
            all["propiedad desconocida en una combinacion"] = Json(d => { d["combinations"] = new JsonArray(S.Combination("C", ("REF1", 1))); CombinationOf(d)["extra"] = 1; }, Codes.DumpUnknownProperty);
            all["propiedad desconocida en un termino"] = Json(d => { d["combinations"] = new JsonArray(S.Combination("C", ("REF1", 1))); CombinationOf(d)["terms"].ObjectAt(0)["extra"] = 1; }, Codes.DumpUnknownProperty);

            // duplicate properties (seen in the text, which a document model hides)
            all["propiedad duplicada en la raiz"] = Text(t => "{\"schema\":\"x\"," + t.Substring(1), Codes.DumpDuplicateProperty);
            all["propiedad duplicada en un nudo"] = Text(t => t.Replace("\"id\":11", "\"id\":11,\"id\":11", StringComparison.Ordinal), Codes.DumpDuplicateProperty);
            all["propiedad duplicada en una carga"] = Text(t => t.Replace("\"value\":-1000", "\"value\":-1000,\"value\":-1000", StringComparison.Ordinal), Codes.DumpDuplicateProperty);

            // missing properties
            all["sin nudos"] = Json(d => d.Remove("nodes"), Codes.DumpMissingProperty);
            all["sin combinaciones"] = Json(d => d.Remove("combinations"), Codes.DumpMissingProperty);
            all["sin errores"] = Json(d => d.Remove("errors"), Codes.DumpMissingProperty);
            all["sin par de unidades de lectura"] = Json(d => d.Remove("readUnits"), Codes.DumpMissingProperty);
            all["sin apoyos en un nudo"] = Json(d => NodeOf(d).Remove("restraints"), Codes.DumpMissingProperty);
            all["sin eje local en un miembro"] = Json(d => MemberOf(d).Remove("l2"), Codes.DumpMissingProperty);
            all["sin liberaciones en un miembro"] = Json(d => MemberOf(d).Remove("releases"), Codes.DumpMissingProperty);
            all["sin valor en una carga"] = Json(d => LoadOf(d).Remove("value"), Codes.DumpMissingProperty);
            all["sin bandera de porcentaje en una carga"] = Json(d => LoadOf(d).Remove("isPercentage"), Codes.DumpMissingProperty);

            // the schema
            all["otro esquema"] = Json(d => d["schema"] = "rackcad-ram-dump/v2", Codes.DumpSchemaMismatch);

            // wrong types
            all["esquema numerico"] = Json(d => d["schema"] = 1, Codes.DumpWrongType);
            all["estado numerico"] = Json(d => d["status"] = 1, Codes.DumpWrongType);
            all["nudos como objeto"] = Json(d => d["nodes"] = new JsonObject(), Codes.DumpWrongType);
            all["identificador de nudo como texto"] = Json(d => NodeOf(d)["id"] = "11", Codes.DumpWrongType);
            all["descripcion de nudo nula"] = Json(d => NodeOf(d)["description"] = null, Codes.DumpWrongType);
            all["coordenada como texto"] = Json(d => NodeOf(d)["coordinates"] = new JsonArray("a", 0, 0), Codes.DumpWrongType);
            all["apoyo como numero"] = Json(d => NodeOf(d)["restraints"] = new JsonArray(1, 1, 1, 1, 1, 1), Codes.DumpWrongType);
            all["punto cardinal como texto"] = Json(d => MemberOf(d)["cardinalPoint"] = "0", Codes.DumpWrongType);
            all["seccion numerica"] = Json(d => MemberOf(d)["section"] = 5, Codes.DumpWrongType);
            all["liberacion como numero"] = Json(d => MemberOf(d)["releases"] = new JsonArray(0, 0, 0, 0, 0, 0, 0, 0, 0, 0), Codes.DumpWrongType);
            all["direccion de carga numerica"] = Json(d => LoadOf(d)["direction"] = 1, Codes.DumpWrongType);
            all["bandera de porcentaje numerica"] = Json(d => LoadOf(d)["isPercentage"] = 1, Codes.DumpWrongType);
            all["valor de carga como texto"] = Json(d => LoadOf(d)["value"] = "-1000", Codes.DumpWrongType);
            all["unidades restauradas como texto"] = Json(d => d["unitsRestored"] = "false", Codes.DumpWrongType);
            all["raiz que es una lista"] = Text(t => "[" + t + "]", Codes.DumpWrongType);

            // invalid values
            all["identificador de nudo fraccionario"] = Json(d => NodeOf(d)["id"] = 1.5, Codes.DumpValueInvalid);
            all["identificador de nudo negativo"] = Json(d => NodeOf(d)["id"] = -1, Codes.DumpValueInvalid);
            all["dos coordenadas"] = Json(d => NodeOf(d)["coordinates"] = S.Numbers(0, 0), Codes.DumpValueInvalid);
            all["cinco apoyos"] = Json(d => NodeOf(d)["restraints"] = S.Bools(true, true, true, true, true), Codes.DumpValueInvalid);
            all["nueve liberaciones"] = Json(d => MemberOf(d)["releases"] = S.Bools(new bool[9]), Codes.DumpValueInvalid);
            all["tres factores de rigidez"] = Json(d => MemberOf(d)["stiffnessFactors"] = S.Numbers(1, 1, 1), Codes.DumpValueInvalid);
            all["punto cardinal fuera de rango"] = Json(d => MemberOf(d)["cardinalPoint"] = 10, Codes.DumpValueInvalid);
            all["comportamiento axial fuera de rango"] = Json(d => MemberOf(d)["axialBehavior"] = 4, Codes.DumpValueInvalid);
            all["seccion vacia"] = Json(d => MemberOf(d)["section"] = string.Empty, Codes.DumpValueInvalid);
            all["numero no finito"] = Text(t => t.Replace("\"distance\":25", "\"distance\":1e999", StringComparison.Ordinal), Codes.DumpValueInvalid);

            // identity and references
            all["nudo repetido"] = Json(d => ((JsonArray)d["nodes"]).Add(S.Node(11, "N3", 1, 1, 1, false, false, false, false, false, false)), Codes.DumpIdDuplicate);
            all["miembro repetido"] = Json(d => ((JsonArray)d["members"]).Add(S.Member(21, "M2", 11, 12, new[] { 0.0, 1.0, 0.0 }, new[] { -1.0, 0.0, 0.0 }, new[] { 0.0, 0.0, 1.0 })), Codes.DumpIdDuplicate);
            all["caso repetido"] = Json(d => ((JsonArray)d["loadCases"]).Add(S.Case("REF1", "OTHER")), Codes.DumpIdDuplicate);
            all["combinacion con el id de un caso"] = Json(d => d["combinations"] = new JsonArray(S.Combination("REF1", ("REF1", 1))), Codes.DumpIdDuplicate);
            all["miembro con un nudo I que no existe"] = Json(d => MemberOf(d)["nodeI"] = 99, Codes.DumpReference);
            all["miembro con un nudo J que no existe"] = Json(d => MemberOf(d)["nodeJ"] = 99, Codes.DumpReference);
            all["cargas de un miembro que no existe"] = Json(d => CaseOf(d)["memberLoads"].ObjectAt(0)["member"] = 99, Codes.DumpReference);
            all["termino de un caso que no existe"] = Json(d => d["combinations"] = new JsonArray(S.Combination("C", ("NOPE", 1))), Codes.DumpReference);

            // malformed text
            all["texto vacio"] = Text(t => string.Empty, Codes.DumpJsonMalformed);
            all["solo espacios"] = Text(t => "   ", Codes.DumpJsonMalformed);
            all["texto que no es json"] = Text(t => "this is not json", Codes.DumpJsonMalformed);
            all["json truncado"] = Text(t => t.Substring(0, t.Length / 2), Codes.DumpJsonMalformed);
            all["dos valores en la raiz"] = Text(t => t + t, Codes.DumpJsonMalformed);
            all["comentario dentro del json"] = Text(t => "{/*c*/" + t.Substring(1), Codes.DumpJsonMalformed);
            all["coma final"] = Text(t => t.Substring(0, t.Length - 1) + ",}", Codes.DumpJsonMalformed);

            return all.ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        }

        [Theory]
        [MemberData(nameof(Variants))]
        public void ElLectorEstrictoRechazaCadaVarianteConSuPropioCodigoYNuncaDaUnModelo(string name)
        {
            var (mutate, code) = Mutations[name];
            var result = RamDumpReader.Read(mutate(S.Valid()));

            Assert.NotEqual(StructuralExportOutcome.Valid, result.Outcome);
            Assert.Null(result.Readback);
            Assert.Contains(result.Diagnostics, d => d.Code == code && d.IsBlocking);
        }

        [Fact]
        public void ElLectorInformaTodosLosHallazgosYNoSoloElPrimero()
        {
            var dump = S.Valid();
            dump["extra"] = 1;
            NodeOf(dump)["id"] = "11";
            MemberOf(dump).Remove("l2");

            var result = S.Read(dump);

            Assert.Equal(StructuralExportOutcome.Invalid, result.Outcome);
            Assert.Contains(result.Diagnostics, d => d.Code == Codes.DumpUnknownProperty);
            Assert.Contains(result.Diagnostics, d => d.Code == Codes.DumpWrongType);
            Assert.Contains(result.Diagnostics, d => d.Code == Codes.DumpMissingProperty);
        }

        [Fact]
        public void ElLectorAceptaUnaMarcaDeOrdenDeBytesAlPrincipio()
        {
            var result = RamDumpReader.Read("﻿" + S.Valid().ToJsonString());

            Assert.Equal(StructuralExportOutcome.Valid, result.Outcome);
            Assert.NotNull(result.Readback);
        }

        [Fact]
        public void ElLectorNoLanzaConEntradasNulasOHostiles()
        {
            foreach (var text in new[] { null, string.Empty, "null", "{}", "[]", "\"x\"", "1", "{\"schema\":", new string('[', 5000) })
            {
                var result = RamDumpReader.Read(text);

                Assert.NotEqual(StructuralExportOutcome.Valid, result.Outcome);
                Assert.Null(result.Readback);
                Assert.NotEmpty(result.Diagnostics);
            }
        }

        [Fact]
        public void ElResultadoNoValidoNuncaTrasladaUnModeloParcial()
        {
            var dump = S.Valid();
            MemberOf(dump)["nodeI"] = 99;

            var result = S.Read(dump);

            Assert.NotEqual(StructuralExportOutcome.Valid, result.Outcome);
            Assert.Null(result.Readback);
            Assert.All(result.Diagnostics.Where(d => d.IsBlocking), d => Assert.NotEqual(StructuralExportOutcome.Valid, d.ImpliedOutcome));
        }

        private static void AssertDirection(double x, double y, double z, StructuralDirection actual)
        {
            Assert.InRange(Math.Abs(actual.X - x), 0, Tolerance);
            Assert.InRange(Math.Abs(actual.Y - y), 0, Tolerance);
            Assert.InRange(Math.Abs(actual.Z - z), 0, Tolerance);
        }
    }
}
