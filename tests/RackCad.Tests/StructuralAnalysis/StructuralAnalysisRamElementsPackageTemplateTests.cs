using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using RackCad.Application.StructuralAnalysis;
using RackCad.Application.StructuralAnalysis.Providers.RamElements;
using Xunit;
using I = RackCad.Tests.StructuralAnalysisRamElementsPackageInputs;

namespace RackCad.Tests
{
    /// <summary>
    /// The static guard of the build script (I-65 INV-10, D-09 rules 1, 3-10, CD-I65-F3-08). It is the ONLY test file that names the module
    /// of the analysis program and the interpreter (INV-14, CD-I65-F3-05 b). It READS the text of the script: nothing here runs it, and
    /// nothing reads the installation of the analysis program. The guard is proven on the real script and on synthetic variants that must
    /// fail: a gate removed, a save, an analysis, another open, the category as an enum member, a member without material.
    /// </summary>
    internal static class StructuralAnalysisRamElementsPackageTemplateGuard
    {
        private static readonly Regex Import = new Regex(@"^\s*(?:import|from)\s", RegexOptions.CultureInvariant);

        private static readonly Regex[] AllowedImports =
        {
            new Regex(@"^import json$", RegexOptions.CultureInvariant),
            new Regex(@"^import math$", RegexOptions.CultureInvariant),
            new Regex(@"^import ramelements(?: as \w+)?$", RegexOptions.CultureInvariant),
            new Regex(@"^from ramelements(?:\.\w+)* import \w+(?:, \w+)*$", RegexOptions.CultureInvariant),
        };

        private static readonly Regex Forbidden = new Regex(
            @"\b(?:os|sys|subprocess|threading|exec|eval|compile|SystemExit|exit|quit|importlib|ctypes|socket|"
            + @"save|save_as|new|analyze|model_new|model_save|model_save_as|results|member_design|design_members|design_run|LoadCategory)\b|__import__",
            RegexOptions.CultureInvariant);

        private static readonly Regex Setters = new Regex(
            @"\.(?:rigid_end_offset_[ij]|rotational_stiffness_[ij]|axial_behavior|end_releases|restraints)\s*=(?!=)"
            + @"|\.(?:set_stiffness_factors|set_releases|set_spring|set_rigid_end_offset|set_rotational_stiffness|set_axial_behavior)\s*\(",
            RegexOptions.CultureInvariant);

        private static readonly Regex Attribute = new Regex(@"\.([A-Za-z_]\w*)", RegexOptions.CultureInvariant);

        // Section 7 of the F3 document (executed in P1 or read from the installed package) and the plain Python the script needs.
        public static readonly HashSet<string> AllowedNames = new HashSet<string>(StringComparer.Ordinal)
        {
            // the package
            "model", "units", "set", "current", "nodes", "members", "load_condition", "add", "add_load_case", "add_point_load",
            "point_load_count", "get_point_load", "get_local_axes", "get_coordinates", "get_stiffness_factors", "restraints", "id",
            "description", "section", "material", "rotation", "cardinal_point", "rigid_end_offset_i", "rigid_end_offset_j",
            "rotational_stiffness_i", "rotational_stiffness_j", "axial_behavior", "end_releases", "i_m33", "i_m22", "i_v2", "i_v3",
            "j_m33", "j_m22", "j_v2", "j_v3", "torsion", "axial", "direction", "value", "distance", "is_percentage", "category",
            "self_weight", "dx", "dy", "dz", "rx", "ry", "rz", "Units", "LB_IN", "Restraints", "CardinalPoint", "CENTROID",
            "LoadDirection", "GLOBAL_Y", "_p4d", "engine",
            // plain Python
            "loads", "dumps", "isfinite", "__name__", "startswith", "append", "extend", "write", "close", "__getattribute__",
        };

        /// <summary>The violations of the rules of the script; empty when the script obeys them all.</summary>
        public static IReadOnlyList<string> Violations(string script)
        {
            var violations = new List<string>();
            var code = StripComments(script);
            var skeleton = StripStrings(code);
            var functions = Functions(code);

            // rule 1: imports
            foreach (var line in skeleton.Split('\n').Where(l => Import.IsMatch(l)).Select(l => l.Trim()))
            {
                if (!AllowedImports.Any(pattern => pattern.IsMatch(line)))
                {
                    violations.Add("import no admitido: " + line);
                }
            }

            // rules 4, 10 and the owner restrictions: forbidden words, no analysis, no store
            foreach (Match match in Forbidden.Matches(skeleton))
            {
                violations.Add("palabra prohibida: " + match.Value);
            }

            foreach (Match match in Setters.Matches(skeleton))
            {
                violations.Add("setter prohibido: " + match.Value);
            }

            // rule 4: ONE open, exclusive mode, the read-back file of the embedded folder
            var opens = Regex.Matches(skeleton, @"\bopen\s*\(").Count;

            if (opens != 1)
            {
                violations.Add("se esperaba un unico open, hay " + opens);
            }

            if (!Regex.IsMatch(code, @"out = open\(data\[""packageFolder""\] \+ ""\\\\"" \+ READBACK_NAME, ""x"", encoding=""utf-8"", newline=""\\n""\)"))
            {
                violations.Add("el open no es el de ram-readback.json en modo exclusivo dentro de la carpeta embebida");
            }

            if (!code.Contains("READBACK_NAME = \"ram-readback.json\"", StringComparison.Ordinal))
            {
                violations.Add("falta el nombre del archivo de lectura de vuelta");
            }

            // rule 8: both gates, in order, before the open and before the first write
            if (!functions.TryGetValue("main", out var main))
            {
                violations.Add("falta main");
            }
            else
            {
                var order = new[] { "capability_failures(data)", "rm.model()", "empty_model_failures(m", "open(", "write_model(m, data)" };
                var last = -1;

                foreach (var token in order)
                {
                    var index = main.IndexOf(token, StringComparison.Ordinal);

                    if (index < 0)
                    {
                        violations.Add("main no contiene " + token);
                    }
                    else if (index <= last)
                    {
                        violations.Add("main llama " + token + " fuera de orden");
                    }
                    else
                    {
                        last = index;
                    }
                }

                if (Regex.Matches(main, @"write_model\(").Count != 1)
                {
                    violations.Add("main debe escribir el modelo una sola vez");
                }
            }

            if (!functions.TryGetValue("capability_failures", out var capability)
                || !capability.Contains("__version__", StringComparison.Ordinal)
                || !capability.Contains("capabilities[\"coreFunctions\"]", StringComparison.Ordinal)
                || !capability.Contains("object.__getattribute__(get_core(), \"_module\")", StringComparison.Ordinal))
            {
                violations.Add("la puerta de capacidad no comprueba la version y las funciones del nucleo");
            }

            if (!functions.TryGetValue("empty_model_failures", out var empty)
                || !empty.Contains("len(m.nodes) != 0", StringComparison.Ordinal)
                || !empty.Contains("len(m.members) != 0", StringComparison.Ordinal)
                || !empty.Contains("len(cases) > 1", StringComparison.Ordinal)
                || !empty.Contains("case_id in cases", StringComparison.Ordinal))
            {
                violations.Add("la puerta de modelo vacio no comprueba nudos, miembros y casos");
            }

            // the writes live in write_model and nowhere else; the pair of units is the first of them
            var writeTokens = new[] { ".units.set(", ".nodes.add(", ".members.add(", ".add_load_case(", ".add_point_load(", ".cardinal_point =" };

            foreach (var pair in functions.Where(f => f.Key != "write_model"))
            {
                foreach (var token in writeTokens.Where(t => pair.Value.Contains(t, StringComparison.Ordinal)))
                {
                    violations.Add("escritura " + token + " fuera de write_model, en " + pair.Key);
                }
            }

            if (!functions.TryGetValue("write_model", out var write))
            {
                violations.Add("falta write_model");
            }
            else
            {
                var units = write.IndexOf("m.units.set(rm.Units.LB_IN)", StringComparison.Ordinal);

                if (units < 0)
                {
                    violations.Add("no fija LB_IN con units.set");
                }
                else if (writeTokens.Skip(1).Any(t => write.IndexOf(t, StringComparison.Ordinal) is var i && i >= 0 && i < units))
                {
                    violations.Add("escribe antes de fijar LB_IN");
                }

                if (!write.Contains("data[\"units\"] != \"LB_IN\"", StringComparison.Ordinal))
                {
                    violations.Add("no comprueba el par de unidades de la carga util");
                }

                if (!write.Contains("member.cardinal_point = rm.CardinalPoint.CENTROID", StringComparison.Ordinal))
                {
                    violations.Add("no fija el punto cardinal CENTROID");
                }

                if (!Regex.IsMatch(write, @"rm\.Restraints\(f\[0\], f\[1\], f\[2\], f\[3\], f\[4\], f\[5\]\)"))
                {
                    violations.Add("los apoyos no se construyen con Restraints y sus seis banderas");
                }

                // rule 5 and the category as text
                if (!Regex.IsMatch(write, @"\.add_load_case\(case_id, CASE_DESCRIPTION, data\[""loadCase""\]\[""category""\]\)"))
                {
                    violations.Add("la categoria del caso no es el texto de la carga util");
                }

                // members.add always with an explicit material
                var call = Call(write, "m.members.add(");

                if (call == null
                    || !Regex.IsMatch(call, @"section=spec\[""section""\]")
                    || !Regex.IsMatch(call, @"material=spec\[""material""\]")
                    || !Regex.IsMatch(call, @"rotation=spec\[""rotation""\]")
                    || !Regex.IsMatch(call, @"description=spec\[""id""\]"))
                {
                    violations.Add("members.add sin seccion, material, giro y descripcion explicitos");
                }

                if (!write.Contains("not spec[\"section\"] or not spec[\"material\"]", StringComparison.Ordinal))
                {
                    violations.Add("no rechaza una seccion o un material vacios");
                }

                var load = Call(write, ".add_point_load(");

                if (load == null || !load.Contains("rm.LoadDirection.GLOBAL_Y", StringComparison.Ordinal) || !load.Contains("is_percentage=True", StringComparison.Ordinal))
                {
                    violations.Add("la carga puntual no es GLOBAL_Y en porcentaje");
                }
            }

            // rule 7 and 9: the read-back compares what it must, with the measured defaults
            if (!functions.TryGetValue("read_back", out var back))
            {
                violations.Add("falta read_back");
            }
            else
            {
                foreach (var required in new[]
                {
                    "int(member.cardinal_point) == 0",
                    "offset_i == (0.0, 0.0, 0.0)",
                    "offset_j == (0.0, 0.0, 0.0)",
                    "factors == (1.0, 1.0, 1.0, 1.0)",
                    "int(member.axial_behavior) == 0",
                    "member.rotational_stiffness_i == 0 and member.rotational_stiffness_j == 0",
                    "not any(flags)",
                    "tuple(reference.self_weight) == (0.0, 0.0, 0.0)",
                    "flags == list(spec[\"restraints\"])",
                    "member.section == spec[\"section\"]",
                    "member.material == spec[\"material\"]",
                    "m.units.current == rm.Units.LB_IN",
                })
                {
                    if (!back.Contains(required, StringComparison.Ordinal))
                    {
                        violations.Add("la lectura de vuelta no comprueba: " + required);
                    }
                }
            }

            // rule 9: the tolerances
            if (!code.Contains("abs(actual - expected) <= tolerance * max(1.0, abs(expected))", StringComparison.Ordinal)
                || !code.Contains("abs(actual - expected) <= tolerance", StringComparison.Ordinal))
            {
                violations.Add("faltan las dos formas de tolerancia de D-09 regla 9");
            }

            // the failed state, written with the escape of the interpreter and nothing outside ASCII
            if (!code.Contains("FAILED = \"FAILED \\u2014 DISCARD MODEL\"", StringComparison.Ordinal))
            {
                violations.Add("el estado de fallo no es FAILED con la secuencia de escape de la raya");
            }

            if (script.Any(c => c > 0x7F))
            {
                violations.Add("el script no es solo ASCII");
            }

            // CD-I65-F3-08: every attribute and call is in the list
            foreach (Match match in Attribute.Matches(skeleton))
            {
                if (!AllowedNames.Contains(match.Groups[1].Value))
                {
                    violations.Add("llamada o atributo fuera de la lista: ." + match.Groups[1].Value);
                }
            }

            return violations;
        }

        /// <summary>The text of a Python call, from its name to its matching closing parenthesis; <c>null</c> when it is absent.</summary>
        private static string Call(string text, string name)
        {
            var start = text.IndexOf(name, StringComparison.Ordinal);

            if (start < 0)
            {
                return null;
            }

            var depth = 0;

            for (var index = start + name.Length - 1; index < text.Length; index++)
            {
                if (text[index] == '(')
                {
                    depth++;
                }
                else if (text[index] == ')' && --depth == 0)
                {
                    return text.Substring(start, index - start + 1);
                }
            }

            return null;
        }

        /// <summary>Each top-level function, by name, with its signature and body.</summary>
        private static Dictionary<string, string> Functions(string code)
        {
            var functions = new Dictionary<string, string>(StringComparer.Ordinal);

            foreach (Match match in Regex.Matches(code, @"(?ms)^def (?<name>\w+)\(.*?(?=^\S|\z)"))
            {
                functions[match.Groups["name"].Value] = match.Value;
            }

            return functions;
        }

        /// <summary>The script without its comments: a hash inside a string literal is not a comment.</summary>
        public static string StripComments(string script)
        {
            var result = new StringBuilder();

            foreach (var line in script.Split('\n'))
            {
                var inString = false;
                var cut = line.Length;

                for (var index = 0; index < line.Length; index++)
                {
                    var character = line[index];

                    if (inString)
                    {
                        if (character == '\\')
                        {
                            index++;
                        }
                        else if (character == '"')
                        {
                            inString = false;
                        }
                    }
                    else if (character == '"')
                    {
                        inString = true;
                    }
                    else if (character == '#')
                    {
                        cut = index;
                        break;
                    }
                }

                result.Append(line.Substring(0, cut).TrimEnd()).Append('\n');
            }

            return result.ToString();
        }

        /// <summary>The script with the content of every string literal emptied, so that words inside messages are not read as code.</summary>
        public static string StripStrings(string code)
        {
            var result = new StringBuilder();
            var inString = false;

            for (var index = 0; index < code.Length; index++)
            {
                var character = code[index];

                if (inString)
                {
                    if (character == '\\')
                    {
                        index++;
                    }
                    else if (character == '"')
                    {
                        inString = false;
                        result.Append('"');
                    }
                    else if (character == '\n')
                    {
                        inString = false;
                        result.Append('\n');
                    }
                }
                else
                {
                    result.Append(character);
                    inString = character == '"';
                }
            }

            return result.ToString();
        }
    }

    /// <summary>I-65 F3-T2 — INV-10: the fixed text of the build script and its static guard, proven on the real script and on variants that must fail.</summary>
    public class StructuralAnalysisRamElementsPackageTemplateTests
    {
        private static string RefScript() => I.Text(I.Package(), RamBuildPackage.ScriptFileName);

        private static string Replace(string script, string from, string to)
        {
            var changed = script.Replace(from, to, StringComparison.Ordinal);

            Assert.NotEqual(script, changed);
            return changed;
        }

        // ---------------------------------------------------------------- the real text

        [Fact]
        public void LaPlantillaTieneSaltosLfElMarcadorUnaVezYNingunaLlaveDeModelo()
        {
            var text = RamBuildScriptTemplate.Text;

            Assert.DoesNotContain("\r", text);
            Assert.Equal(1, Regex.Matches(text, Regex.Escape(RamBuildScriptTemplate.PayloadMarker)).Count);
            Assert.Contains("PAYLOAD = \"@@PAYLOAD@@\"", text);
            Assert.EndsWith("\n", text);
        }

        [Fact]
        public void LaPlantillaSinRenderizarCumpleLaGuarda()
        {
            Assert.Empty(StructuralAnalysisRamElementsPackageTemplateGuard.Violations(RamBuildScriptTemplate.Text));
        }

        [Fact]
        public void ElScriptGeneradoDeRefCant01CumpleLaGuarda()
        {
            Assert.Empty(StructuralAnalysisRamElementsPackageTemplateGuard.Violations(RefScript()));
        }

        [Fact]
        public void ElScriptGeneradoSoloDifiereDeLaPlantillaEnLaLineaDeLaCargaUtil()
        {
            var template = RamBuildScriptTemplate.Text.Split('\n');
            var script = RefScript().Split('\n');

            Assert.Equal(template.Length, script.Length);

            var different = Enumerable.Range(0, template.Length).Where(i => template[i] != script[i]).ToList();

            Assert.Single(different);
            Assert.StartsWith(RamBuildScriptTemplate.PayloadLinePrefix, script[different[0]]);
        }

        [Fact]
        public void UnaCargaUtilConComillasBarrasYAlmohadillaNoRompeLaGuardaNiElLiteral()
        {
            var payload = "{\"a\":\"say \\\"x\\\" # not a comment \\\\ end\"}";
            var script = RamBuildScriptTemplate.Render(payload);

            Assert.Equal(payload, I.ExtractPayload(script));
            Assert.Empty(StructuralAnalysisRamElementsPackageTemplateGuard.Violations(script));
        }

        [Fact]
        public void LosImportsSonSoloElModuloDelProveedorJsonYMath()
        {
            var imports = RefScript().Split('\n').Where(l => l.StartsWith("import ", StringComparison.Ordinal) || l.StartsWith("from ", StringComparison.Ordinal)).ToList();

            Assert.Equal(
                new[]
                {
                    "import json",
                    "import math",
                    "import ramelements as rm",
                    "from ramelements._p4d.engine import get_core",
                },
                imports);
        }

        [Fact]
        public void ElEstadoDeFalloSeEscribeConLaSecuenciaDeEscapeDeLaRayaYSuValorEsElPedido()
        {
            var script = RefScript();
            var line = script.Split('\n').Single(l => l.StartsWith("FAILED = ", StringComparison.Ordinal));

            Assert.Equal("FAILED = \"FAILED \\u2014 DISCARD MODEL\"", line);
            Assert.DoesNotContain("\u2014", script);
            Assert.Equal("FAILED \u2014 DISCARD MODEL", Regex.Unescape(line.Substring("FAILED = \"".Length).TrimEnd('"')));
        }

        [Fact]
        public void ElScriptEscribeElEstadoLaHuellaLasDiferenciasYLosValoresLeidosTambienAnteUnFallo()
        {
            var script = RefScript();
            var failure = script.LastIndexOf("except Exception as exc:", StringComparison.Ordinal);
            var write = script.IndexOf("out.write(text", StringComparison.Ordinal);
            var close = script.IndexOf("out.close()", StringComparison.Ordinal);

            Assert.True(failure > 0 && write > failure && close > write, "El archivo se escribe despues del bloque de excepcion.");

            foreach (var key in new[] { "\"status\"", "\"analysisInputFingerprint\"", "\"differences\"", "\"values\"", "result[\"status\"] = PASSED", "print(result[\"status\"])" })
            {
                Assert.Contains(key, script);
            }
        }

        [Fact]
        public void LaPuertaYLaEscrituraSiguenElOrdenDeD09()
        {
            var script = RefScript();
            var main = script.Substring(script.IndexOf("def main():", StringComparison.Ordinal));
            var tokens = new[] { "capability_failures(data)", "rm.model()", "empty_model_failures(m", "open(", "write_model(m, data)", "read_back(m, data" };
            var indexes = tokens.Select(t => main.IndexOf(t, StringComparison.Ordinal)).ToList();

            Assert.DoesNotContain(-1, indexes);
            Assert.Equal(indexes.OrderBy(i => i).ToList(), indexes);
        }

        [Fact]
        public void ElScriptNoTieneNingunaLlamadaFueraDeLaListaDeLaSeccion7()
        {
            var skeleton = StructuralAnalysisRamElementsPackageTemplateGuard.StripStrings(StructuralAnalysisRamElementsPackageTemplateGuard.StripComments(RefScript()));
            var names = Regex.Matches(skeleton, @"\.([A-Za-z_]\w*)").Select(m => m.Groups[1].Value).Distinct().ToList();

            Assert.NotEmpty(names);
            Assert.All(names, name => Assert.Contains(name, StructuralAnalysisRamElementsPackageTemplateGuard.AllowedNames));
        }

        // ---------------------------------------------------------------- the guard proves itself: each variant must fail

        private static readonly Dictionary<string, Func<string, string>> Mutations = BuildMutations();

        public static IEnumerable<object[]> Variants() => Mutations.Keys.Select(name => new object[] { name });

        private static Dictionary<string, Func<string, string>> BuildMutations()
        {
            var all = new Dictionary<string, Func<string, string>>(StringComparer.Ordinal);

            all["sin puerta de capacidad"] = s => Replace(s, "        gate = capability_failures(data)\n", "        gate = []\n");
            all["sin comprobar la version"] = s => Replace(s, "getattr(rm, \"__version__\", None)", "None");
            all["sin comprobar las funciones del nucleo"] = s => Replace(s, "for name in capabilities[\"coreFunctions\"]:", "for name in []:");
            all["sin puerta de modelo vacio"] = s => Replace(s, "            gate = empty_model_failures(m, data[\"loadCase\"][\"id\"])\n", "            gate = []\n");
            all["sin mirar los nudos del modelo"] = s => Replace(s, "len(m.nodes) != 0", "False");
            all["con save_as"] = s => Replace(s, "    m.units.set(rm.Units.LB_IN)\n", "    m.units.set(rm.Units.LB_IN)\n    m.save_as(\"x\")\n");
            all["con save"] = s => Replace(s, "    m.units.set(rm.Units.LB_IN)\n", "    m.units.set(rm.Units.LB_IN)\n    m.save()\n");
            all["con analyze"] = s => Replace(s, "    return nodes, members\n", "    m.analyze()\n    return nodes, members\n");
            all["con otro open"] = s => Replace(s, "    m.units.set(rm.Units.LB_IN)\n", "    m.units.set(rm.Units.LB_IN)\n    other = open(\"other.txt\", \"w\")\n");
            all["con la categoria como miembro del enum"] = s => Replace(s, "CASE_DESCRIPTION, data[\"loadCase\"][\"category\"])", "CASE_DESCRIPTION, rm.LoadCategory.OTHER)");
            all["sin material explicito"] = s => Replace(s, "            material=spec[\"material\"],\n", string.Empty);
            all["sin seccion explicita"] = s => Replace(s, "            section=spec[\"section\"],\n", string.Empty);
            all["apertura no exclusiva"] = s => Replace(s, "\"x\", encoding=\"utf-8\"", "\"w\", encoding=\"utf-8\"");
            all["escritura antes de abrir el archivo"] = s => Replace(s, "        out = open(", "        write_model(m, data)\n        out = open(");
            all["con import os"] = s => Replace(s, "import math\n", "import math\nimport os\n");
            all["con import sys"] = s => Replace(s, "import math\n", "import math\nimport sys\n");
            all["con exec"] = s => Replace(s, "    m.units.set(rm.Units.LB_IN)\n", "    m.units.set(rm.Units.LB_IN)\n    exec(\"pass\")\n");
            all["con setter de offsets"] = s => Replace(s, "        member.cardinal_point = rm.CardinalPoint.CENTROID\n", "        member.cardinal_point = rm.CardinalPoint.CENTROID\n        member.rigid_end_offset_i = (0.0, 0.0, 0.0)\n");
            all["con setter de resortes"] = s => Replace(s, "        member.cardinal_point = rm.CardinalPoint.CENTROID\n", "        member.cardinal_point = rm.CardinalPoint.CENTROID\n        member.rotational_stiffness_i = 1.0\n");
            all["con setter de liberaciones"] = s => Replace(s, "        member.cardinal_point = rm.CardinalPoint.CENTROID\n", "        member.cardinal_point = rm.CardinalPoint.CENTROID\n        member.set_releases(True)\n");
            all["sin fijar el punto cardinal"] = s => Replace(s, "        member.cardinal_point = rm.CardinalPoint.CENTROID\n", string.Empty);
            all["sin fijar LB_IN"] = s => Replace(s, "    m.units.set(rm.Units.LB_IN)\n", string.Empty);
            all["con la raya sin escapar"] = s => Replace(s, "\\u2014", "\u2014");
            all["con otro estado de fallo"] = s => Replace(s, "FAILED \\u2014 DISCARD MODEL", "FAILED");
            all["con una llamada fuera de la lista"] = s => Replace(s, "    return nodes, members\n", "    m.nodes.add_bulk([])\n    return nodes, members\n");
            all["con la carga en valor absoluto"] = s => Replace(s, "is_percentage=True", "is_percentage=False");
            all["con otra direccion de carga"] = s => Replace(s, "rm.LoadDirection.GLOBAL_Y, spec", "rm.LoadDirection.GLOBAL_X, spec");
            all["sin comparar los apoyos"] = s => Replace(s, "flags == list(spec[\"restraints\"])", "True");
            all["sin comprobar las liberaciones"] = s => Replace(s, "not any(flags)", "True");
            all["con una tolerancia distinta"] = s => Replace(s, "abs(actual - expected) <= tolerance * max(1.0, abs(expected))", "abs(actual - expected) <= 1.0");
            all["con un nombre de archivo de lectura de vuelta distinto"] = s => Replace(s, "READBACK_NAME = \"ram-readback.json\"", "READBACK_NAME = \"other.json\"");

            return all;
        }

        [Theory]
        [MemberData(nameof(Variants))]
        public void LaGuardaRechazaCadaVarianteSinteticaDelScript(string name)
        {
            var mutate = Mutations[name];
            var violations = StructuralAnalysisRamElementsPackageTemplateGuard.Violations(mutate(RefScript()));

            Assert.True(violations.Count > 0, "La guarda dejo pasar la variante: " + name);
        }

        [Fact]
        public void LaGuardaTambienRechazaLasVariantesSobreLaPlantillaSinRenderizar()
        {
            var template = RamBuildScriptTemplate.Text;

            Assert.NotEmpty(StructuralAnalysisRamElementsPackageTemplateGuard.Violations(Replace(template, "        gate = capability_failures(data)\n", "        gate = []\n")));
            Assert.NotEmpty(StructuralAnalysisRamElementsPackageTemplateGuard.Violations(Replace(template, "    m.units.set(rm.Units.LB_IN)\n", "    m.units.set(rm.Units.LB_IN)\n    m.analyze()\n")));
            Assert.NotEmpty(StructuralAnalysisRamElementsPackageTemplateGuard.Violations(Replace(template, "            material=spec[\"material\"],\n", string.Empty)));
        }

        [Fact]
        public void UnComentarioOUnMensajeConPalabrasProhibidasNoEsCodigo()
        {
            var script = RefScript();
            var withComment = Replace(script, "import json\n", "import json\n# it never calls save_as or analyze, and it does not import os\n");
            var withMessage = Replace(script, "\"the model has \"", "\"the model has (no save, no analyze, no new) \"");

            Assert.Empty(StructuralAnalysisRamElementsPackageTemplateGuard.Violations(withComment));
            Assert.Empty(StructuralAnalysisRamElementsPackageTemplateGuard.Violations(withMessage));
        }

        // ---------------------------------------------------------------- the scanners prove themselves

        [Theory]
        [InlineData("x = 1  # os", "x = 1")]
        [InlineData("x = \"a # b\"  # c", "x = \"a # b\"")]
        [InlineData("x = \"a \\\" # b\"", "x = \"a \\\" # b\"")]
        [InlineData("# only a comment", "")]
        public void StripCommentsQuitaElComentarioPeroNoLaAlmohadillaDentroDeUnaCadena(string line, string expected)
        {
            Assert.Equal(expected, StructuralAnalysisRamElementsPackageTemplateGuard.StripComments(line).TrimEnd('\n'));
        }

        [Theory]
        [InlineData("x = \"os save\"", "x = \"\"")]
        [InlineData("x = \"a \\\" b\" + y", "x = \"\" + y")]
        [InlineData("x = f(1)", "x = f(1)")]
        public void StripStringsVaciaElContenidoDeLasCadenas(string code, string expected)
        {
            Assert.Equal(expected, StructuralAnalysisRamElementsPackageTemplateGuard.StripStrings(code));
        }

        [Fact]
        public void ElModuloYElInterpreteSoloSeNombranEnEsteArchivo()
        {
            // INV-14 exempts exactly this file (CD-I65-F3-05 b): the other package tests do not name the tool or the interpreter.
            var folder = System.IO.Path.Combine(StructuralAnalysisCantileverFixtures.RepoRoot(), "tests", "RackCad.Tests", "StructuralAnalysis");

            foreach (var path in System.IO.Directory.GetFiles(folder, "StructuralAnalysisRamElementsPackage*.cs"))
            {
                if (System.IO.Path.GetFileName(path) == "StructuralAnalysisRamElementsPackageTemplateTests.cs")
                {
                    continue;
                }

                var text = System.IO.File.ReadAllText(path);
                var code = StructuralAnalysisGuards.Code(text);

                var name = System.IO.Path.GetFileName(path) + " nombra la herramienta o el interprete";

                Assert.False(Regex.IsMatch(code, "ramelements"), name);
                Assert.False(Regex.IsMatch(code, "python|bentley", RegexOptions.IgnoreCase), name);
            }
        }
    }
}
