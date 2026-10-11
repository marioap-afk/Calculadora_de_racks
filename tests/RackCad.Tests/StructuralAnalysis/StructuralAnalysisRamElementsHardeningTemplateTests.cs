using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using RackCad.Application.StructuralAnalysis.Providers.RamElements;
using Xunit;
using G = RackCad.Tests.StructuralAnalysisRamElementsPackageTemplateGuard;
using I = RackCad.Tests.StructuralAnalysisRamElementsPackageInputs;
using S = RackCad.Tests.StructuralAnalysisRamElementsHardeningScripts;

namespace RackCad.Tests
{
    /// <summary>The helpers of the I-65 F4-T1 hardening tests over the TEXT of the build script. Nothing here runs the script.</summary>
    internal static class StructuralAnalysisRamElementsHardeningScripts
    {
        /// <summary>The line of main() that adds the checks of the payload to the list of gate failures.</summary>
        public const string PayloadGateLine = "        gate = gate + payload_failures(data)\n";

        /// <summary>The generated script of REF-CANT-01 with the fixture table and the fixed test folder.</summary>
        public static string Real() => I.Text(I.Package(), RamBuildPackage.ScriptFileName);

        /// <summary>A top-level function of the script, from its signature to the next top-level line; the test fails when it is absent.</summary>
        public static string Function(string script, string name)
        {
            var code = G.StripComments(script);
            var match = Regex.Match(code, @"(?ms)^def " + Regex.Escape(name) + @"\(.*?(?=^\S|\z)");

            Assert.True(match.Success, "Falta la funcion " + name + " en el script.");
            return match.Value;
        }

        public static string Replace(string script, string from, string to)
        {
            var changed = script.Replace(from, to, StringComparison.Ordinal);

            Assert.NotEqual(script, changed);
            return changed;
        }
    }

    /// <summary>
    /// I-65 F4-T1 — A65-F3-11 and T4-21: the THIRD gate. The checks of the payload are evaluated with the capability gate and the empty-model
    /// gate, BEFORE the read-back file is opened and before any write (units.set included); every rejection of any gate goes to
    /// <c>differences</c> with the prefix <c>gate: </c>; write_model checks nothing of the payload. Everything is proven on the TEXT of the
    /// script and on synthetic variants that the guard must reject.
    /// </summary>
    public class StructuralAnalysisRamElementsHardeningGateTests
    {
        private static readonly string[] PayloadChecks =
        {
            "data[\"units\"] != \"LB_IN\"",
            "not field(spec, \"section\")",
            "not field(spec, \"material\")",
            "field(spec, \"nodeI\") not in node_ids",
            "field(spec, \"nodeJ\") not in node_ids",
            "field(spec, \"member\") not in member_ids",
            "field(spec, \"direction\") != \"GLOBAL_Y\"",
            "len(flags) != 6",
            "isinstance(flag, bool)",
            "repeated_ids(\"node\", node_ids)",
            "repeated_ids(\"member\", member_ids)",
            "repeated_ids(\"load\", load_ids)",
        };

        [Fact]
        public void LaPlantillaDefineLaFuncionDeLaTerceraPuertaYSusAuxiliares()
        {
            var text = RamBuildScriptTemplate.Text;

            Assert.Matches(@"(?m)^def payload_failures\(data\):$", text);
            Assert.Matches(@"(?m)^def repeated_ids\(kind, ids\):$", text);
            Assert.Matches(@"(?m)^def field\(spec, key\):$", text);
        }

        [Fact]
        public void MainEvaluaLasTresPuertasAntesDeAbrirElArchivoYDeCualquierEscritura()
        {
            var main = S.Function(S.Real(), "main");
            var tokens = new[] { "capability_failures(data)", "rm.model()", "empty_model_failures(m", "payload_failures(data)", "open(", "write_model(m, data)" };
            var indexes = tokens.Select(t => main.IndexOf(t, StringComparison.Ordinal)).ToList();

            Assert.DoesNotContain(-1, indexes);
            Assert.Equal(indexes.OrderBy(i => i).ToList(), indexes);
        }

        [Fact]
        public void LaPuertaDeLaCargaUtilSeEvaluaSiempreQueLaCargaUtilSeHayaDecodificadoNoSoloSiPasanLasOtras()
        {
            var main = S.Function(S.Real(), "main");

            Assert.Matches(@"(?m)^        gate = gate \+ payload_failures\(data\)$", main);
        }

        [Fact]
        public void TodoRechazoDeCualquierPuertaVaADifferencesConElPrefijoGateYElEstadoSigueSiendoElDeFallo()
        {
            var script = S.Real();
            var main = S.Function(script, "main");

            Assert.Contains("differences.extend([\"gate: \" + item for item in gate])", main);
            Assert.Equal(1, Regex.Matches(script, Regex.Escape("\"gate: \"")).Count);
            Assert.DoesNotContain("gate: ", S.Function(script, "payload_failures"));
            Assert.Contains("result[\"status\"] = PASSED", main);
            Assert.Contains("FAILED = \"FAILED \\u2014 DISCARD MODEL\"", script);
        }

        [Fact]
        public void SiAlgunaPuertaFallaElModeloNoRecibeNingunaLlamadaDeEscritura()
        {
            var main = S.Function(S.Real(), "main");

            Assert.Matches(
                @"(?s)        if gate:\n            differences\.extend\(\[""gate: "" \+ item for item in gate\]\)\n        else:\n            nodes, members = write_model\(m, data\)\n",
                main);
        }

        [Theory]
        [InlineData(".units.set(")]
        [InlineData(".nodes.add(")]
        [InlineData(".members.add(")]
        [InlineData(".add_load_case(")]
        [InlineData(".add_point_load(")]
        public void CadaLlamadaDeEscrituraApareceUnaSolaVezYDentroDeWriteModel(string call)
        {
            var script = S.Real();
            var code = G.StripComments(script);

            Assert.Equal(1, Regex.Matches(code, Regex.Escape(call)).Count);
            Assert.Contains(call, S.Function(script, "write_model"));
            Assert.DoesNotContain(call, S.Function(script, "main"));
            Assert.DoesNotContain(call, S.Function(script, "payload_failures"));
        }

        [Theory]
        [InlineData("raise ")]
        [InlineData("!= \"GLOBAL_Y\"")]
        [InlineData("data[\"units\"]")]
        [InlineData("not spec[\"section\"]")]
        [InlineData("not spec[\"material\"]")]
        public void WriteModelYaNoComprobaNadaDeLaCargaUtil(string fragment)
        {
            Assert.DoesNotContain(fragment, S.Function(S.Real(), "write_model"));
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        [InlineData(4)]
        [InlineData(5)]
        [InlineData(6)]
        [InlineData(7)]
        [InlineData(8)]
        [InlineData(9)]
        [InlineData(10)]
        [InlineData(11)]
        public void LaTerceraPuertaHaceCadaComprobacionDeLaCargaUtil(int index)
        {
            Assert.Contains(PayloadChecks[index], S.Function(S.Real(), "payload_failures"));
        }

        [Fact]
        public void LaPuertaDetectaLosIdsRepetidosConUnConjuntoDeLosYaVistos()
        {
            Assert.Contains("if item in seen:", S.Function(S.Real(), "repeated_ids"));
        }

        // ---------------------------------------------------------------- the guard proves the third gate: each variant must fail

        private static readonly Dictionary<string, Func<string, string>> Mutations = BuildMutations();

        public static IEnumerable<object[]> Variants() => Mutations.Keys.Select(name => new object[] { name });

        private static Dictionary<string, Func<string, string>> BuildMutations()
        {
            var all = new Dictionary<string, Func<string, string>>(StringComparer.Ordinal);

            all["sin llamar a la tercera puerta"] = s => S.Replace(s, S.PayloadGateLine, string.Empty);
            all["la tercera puerta despues de abrir el archivo"] = s =>
                S.Replace(S.Replace(s, S.PayloadGateLine, string.Empty), "        if gate:\n", S.PayloadGateLine + "        if gate:\n");
            all["la tercera puerta solo si pasan las otras"] = s =>
                S.Replace(s, S.PayloadGateLine, "        if not gate:\n            gate = gate + payload_failures(data)\n");
            all["sin el prefijo gate"] = s => S.Replace(s, "\"gate: \" + item", "item");
            all["con una comprobacion de la carga util dentro de write_model"] = s => S.Replace(
                s,
                "    m.units.set(rm.Units.LB_IN)\n",
                "    m.units.set(rm.Units.LB_IN)\n    if not data[\"members\"][0][\"section\"]:\n        raise ValueError(\"empty section\")\n");
            all["sin detectar ids repetidos"] = s => S.Replace(s, "if item in seen:", "if False:");

            foreach (var check in PayloadChecks)
            {
                var captured = check;
                all["sin la comprobacion " + captured] = s => S.Replace(s, captured, "False");
            }

            return all;
        }

        [Theory]
        [MemberData(nameof(Variants))]
        public void LaGuardaRechazaCadaVarianteSinteticaDeLaTerceraPuertaYAceptaElScriptReal(string name)
        {
            var script = S.Real();

            Assert.Empty(G.Violations(script));
            Assert.NotEmpty(G.Violations(Mutations[name](script)));
        }
    }

    /// <summary>
    /// I-65 F4-T1 — A65-F3-01, A65-F3-02 and A65-F3-04 in the read-back: the turn is compared modulo 360 with the relative tolerance of rule 9;
    /// the description is written but not compared; the serialisation and the writing of the read-back file are inside the error handling,
    /// with a minimal fallback record, the file always closed and the status always printed.
    /// </summary>
    public class StructuralAnalysisRamElementsHardeningReadBackTests
    {
        [Theory]
        [InlineData("difference = actual - expected")]
        [InlineData("math.isfinite(difference)")]
        [InlineData("math.fmod(difference, 360.0)")]
        [InlineData("wrapped > 180.0")]
        [InlineData("wrapped -= 360.0")]
        [InlineData("wrapped <= -180.0")]
        [InlineData("wrapped += 360.0")]
        [InlineData("abs(wrapped) <= tolerance * max(1.0, abs(expected))")]
        public void ElGiroSeCompararModulo360ConLaDiferenciaEnvueltaAlIntervaloDeMenos180A180(string fragment)
        {
            Assert.Contains(fragment, S.Function(S.Real(), "angle_near"));
        }

        [Fact]
        public void LaLecturaDeVueltaComparaElGiroConLaFuncionModulo360YLaToleranciaRelativa()
        {
            var back = S.Function(S.Real(), "read_back");

            Assert.Contains("angle_near(member.rotation, spec[\"rotation\"], relative)", back);
            Assert.DoesNotMatch(@"(?<!angle_)near\(member\.rotation", back);
            Assert.Contains("relative = tolerances[\"relative\"]", back);
        }

        [Fact]
        public void ElGiroLeidoSeRegistraTalCualEnValues()
        {
            var back = S.Function(S.Real(), "read_back");

            Assert.Contains("\"rotation\": member.rotation,", back);
        }

        [Fact]
        public void LaLecturaDeVueltaNoComparaLaDescripcion()
        {
            var back = S.Function(S.Real(), "read_back");

            Assert.DoesNotContain("member.description ==", back);
            Assert.DoesNotContain(": description", back);
        }

        [Fact]
        public void LaDescripcionSigueEscribiendoseConElIdNeutralYSuValorLeidoQuedaEnValues()
        {
            var script = S.Real();
            var call = S.Function(script, "write_model");

            Assert.Contains("            description=spec[\"id\"],\n", call);
            Assert.Contains("\"description\": member.description,", S.Function(script, "read_back"));
        }

        [Fact]
        public void LaSerializacionYLaEscrituraDeLaLecturaDeVueltaEstanDentroDelManejoDeErrores()
        {
            var persist = S.Function(S.Real(), "persist_readback");
            var order = new[] { "try:", "json.dumps(result", "out.write(text", "except Exception as exc:", "finally:", "out.close()" };
            var indexes = order.Select(t => persist.IndexOf(t, StringComparison.Ordinal)).ToList();

            Assert.DoesNotContain(-1, indexes);
            Assert.Equal(indexes.OrderBy(i => i).ToList(), indexes);
        }

        [Theory]
        [InlineData("\"schema\": result[\"schema\"],")]
        [InlineData("\"status\": FAILED,")]
        [InlineData("\"analysisInputFingerprint\": result[\"analysisInputFingerprint\"],")]
        [InlineData("\"error\": error,")]
        public void ElRespaldoMinimoLlevaElEstadoDeFalloLaHuellaDeEntradaYElError(string fragment)
        {
            Assert.Contains(fragment, S.Function(S.Real(), "persist_readback"));
        }

        [Fact]
        public void ElRespaldoNoLlevaLosValoresLeidos()
        {
            var persist = S.Function(S.Real(), "persist_readback");

            Assert.DoesNotContain("\"values\"", persist);
            Assert.DoesNotContain("result[\"values\"]", persist);
        }

        [Fact]
        public void ElArchivoSeCierraSiempreEnFinallyYElEstadoSeImprimeSiempre()
        {
            var main = S.Function(S.Real(), "main");

            Assert.Contains("    finally:\n        out.close()\n", S.Function(S.Real(), "persist_readback"));
            Assert.Matches(@"(?s)    try:\n        persist_readback\(out, result\)\n    except Exception as exc:\n        result\[""status""\] = FAILED\n", main);
            Assert.True(
                main.IndexOf("    print(result[\"status\"])\n", StringComparison.Ordinal) > main.IndexOf("persist_readback(out, result)", StringComparison.Ordinal),
                "El estado se imprime despues de la escritura de la lectura de vuelta, fuera de cualquier condicion.");
            Assert.DoesNotContain("out.write(", main);
            Assert.DoesNotContain("json.dumps(", main);
        }

        // ---------------------------------------------------------------- the guard proves the read-back: each variant must fail

        private static readonly Dictionary<string, Func<string, string>> Mutations = BuildMutations();

        public static IEnumerable<object[]> Variants() => Mutations.Keys.Select(name => new object[] { name });

        private static Dictionary<string, Func<string, string>> BuildMutations()
        {
            var all = new Dictionary<string, Func<string, string>>(StringComparer.Ordinal);

            all["el giro con la comparacion sin modulo"] = s => S.Replace(s, "angle_near(member.rotation", "near(member.rotation");
            all["el giro sin fmod"] = s => S.Replace(s, "math.fmod(difference, 360.0)", "difference");
            all["el giro sin comprobar que es finito"] = s => S.Replace(s, "math.isfinite(difference)", "True");
            all["con la comparacion de la descripcion"] = s => S.Replace(
                s,
                "        check(differences, label + \": releases\", not any(flags))\n",
                "        check(differences, label + \": releases\", not any(flags))\n        check(differences, label + \": description\", member.description == spec[\"id\"])\n");
            all["dumps fuera del manejo de errores"] = s => S.Replace(
                s,
                "    try:\n        try:\n            text = json.dumps(result, indent=2, sort_keys=True)\n",
                "    text = json.dumps(result, indent=2, sort_keys=True)\n    try:\n        try:\n");
            all["sin cerrar en finally"] = s => S.Replace(s, "    finally:\n        out.close()\n", "    out.close()\n");
            all["sin respaldo minimo"] = s => S.Replace(s, "\"error\": error,", string.Empty);
            all["respaldo con los valores"] = s => S.Replace(s, "\"error\": error,", "\"error\": error,\n                \"values\": result[\"values\"],");
            all["sin imprimir el estado"] = s => S.Replace(s, "    print(result[\"status\"])\n", string.Empty);
            all["escribiendo el archivo desde main"] = s => S.Replace(s, "    try:\n        persist_readback(out, result)\n", "    out.write(\"x\")\n    try:\n        persist_readback(out, result)\n");

            return all;
        }

        [Theory]
        [MemberData(nameof(Variants))]
        public void LaGuardaRechazaCadaVarianteSinteticaDeLaLecturaDeVueltaYAceptaElScriptReal(string name)
        {
            var script = S.Real();

            Assert.Empty(G.Violations(script));
            Assert.NotEmpty(G.Violations(Mutations[name](script)));
        }
    }
}
