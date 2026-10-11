using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using RackCad.Application.StructuralAnalysis;
using RackCad.Application.StructuralAnalysis.Providers.RamElements;
using Xunit;
using Xunit.Abstractions;
using G = RackCad.Tests.StructuralAnalysisRamElementsPackageTemplateGuard;
using S = RackCad.Tests.StructuralAnalysisRamElementsDumpSupport;

namespace RackCad.Tests
{
    /// <summary>
    /// The static guard of the dump script (I-65 D-11, A65-V3-01, CD-I65-F4-13). It READS the text of the script and nothing runs it. It never
    /// names the module of the analysis program nor the interpreter: the name of the module is read from the import line of the build
    /// template (<see cref="StructuralAnalysisRamElementsHardeningScans.ToolName"/>) and the patterns are composed at run time. The guard is
    /// proven on the real script and on synthetic variants that must fail.
    /// </summary>
    internal static class StructuralAnalysisRamElementsDumpTemplateGuard
    {
        private static readonly Regex Import = new Regex(@"^\s*(?:import|from)\s", RegexOptions.CultureInvariant);

        private static readonly Regex Forbidden = new Regex(
            @"\b(?:os|sys|subprocess|threading|exec|eval|compile|SystemExit|exit|quit|importlib|ctypes|socket|save|save_as|new|analyze|"
            + @"model_new|model_save|model_save_as|results|member_design|design_members|design_run|design_update|LoadCategory)\b|__import__",
            RegexOptions.CultureInvariant);

        // a call that creates, changes or deletes something in the model, or any assignment to an attribute
        private static readonly Regex WriteCalls = new Regex(
            @"\.(?:add\w*|delete\w*|remove\w*|clear\w*|insert\w*|set_\w+|enable_\w+|import_\w+|update\w*)\s*\(", RegexOptions.CultureInvariant);

        private static readonly Regex AttributeAssignment = new Regex(
            @"\.[A-Za-z_]\w*\s*(?:[-+*/%]|//|\*\*)?=(?!=)", RegexOptions.CultureInvariant);

        private static readonly Regex Attribute = new Regex(@"\.([A-Za-z_]\w*)", RegexOptions.CultureInvariant);

        // what the dump reads (executed in P1 or read from the installed package) and the plain script code it needs
        public static readonly HashSet<string> AllowedNames = new HashSet<string>(StringComparer.Ordinal)
        {
            // the package, executed in P1
            "model", "units", "current", "set", "nodes", "members", "load_condition", "id", "description", "get_coordinates", "restraints",
            "get_local_axes", "end_releases", "section", "material", "rotation", "cardinal_point", "rigid_end_offset_i", "rigid_end_offset_j",
            "rotational_stiffness_i", "rotational_stiffness_j", "axial_behavior", "get_stiffness_factors", "point_load_count", "get_point_load",
            "direction", "value", "distance", "is_percentage", "category", "self_weight", "Units", "LB_IN", "dx", "dy", "dz", "rx", "ry", "rz",
            "i_m33", "i_m22", "i_v2", "i_v3", "j_m33", "j_m22", "j_v2", "j_v3", "torsion", "axial", "_p4d", "engine",
            // the package, read from the installed source and not executed in P1
            "node_i", "node_j", "is_combination", "terms", "case", "factor", "name",
            // plain script code
            "loads", "dumps", "isfinite", "__name__", "startswith", "append", "extend", "write", "close", "__getattribute__",
        };

        /// <summary>The keys the dump must write, by block.</summary>
        public static readonly IReadOnlyDictionary<string, string[]> RequiredKeys = new Dictionary<string, string[]>(StringComparer.Ordinal)
        {
            ["file"] = new[] { "schema", "status", "adapterVersion", "initialUnits", "readUnits", "unitsRestored", "nodes", "members", "loadCases", "combinations", "errors" },
            ["nodes"] = new[] { "id", "description", "coordinates", "restraints" },
            ["members"] = new[]
            {
                "id", "description", "nodeI", "nodeJ", "section", "material", "rotation", "l1", "l2", "l3", "cardinalPoint", "offsetI", "offsetJ",
                "stiffnessFactors", "axialBehavior", "springI", "springJ", "releases",
            },
            ["cases"] = new[] { "id", "category", "selfWeight", "memberLoads", "member", "loads", "direction", "value", "distance", "isPercentage" },
            ["combinations"] = new[] { "id", "terms", "case", "factor" },
        };

        /// <summary>The violations of the rules of the script; empty when the script obeys them all.</summary>
        public static IReadOnlyList<string> Violations(string script)
        {
            var violations = new List<string>();
            var tool = StructuralAnalysisRamElementsHardeningScans.ToolName();
            var code = G.StripComments(script);
            var skeleton = G.StripStrings(code);
            var functions = Functions(code);

            // rule 1: imports
            var allowedImports = new[]
            {
                new Regex(@"^import json$", RegexOptions.CultureInvariant),
                new Regex(@"^import math$", RegexOptions.CultureInvariant),
                new Regex("^import " + Regex.Escape(tool) + @"(?: as \w+)?$", RegexOptions.CultureInvariant),
                new Regex("^from " + Regex.Escape(tool) + @"(?:\.\w+)* import \w+(?:, \w+)*$", RegexOptions.CultureInvariant),
            };

            foreach (var line in skeleton.Split('\n').Where(l => Import.IsMatch(l)).Select(l => l.Trim()))
            {
                if (!allowedImports.Any(pattern => pattern.IsMatch(line)))
                {
                    violations.Add("import no admitido: " + line);
                }
            }

            // read only: no forbidden word, no call that writes, no assignment to an attribute
            foreach (Match match in Forbidden.Matches(skeleton))
            {
                violations.Add("palabra prohibida: " + match.Value);
            }

            foreach (Match match in WriteCalls.Matches(skeleton))
            {
                violations.Add("llamada que escribe en el modelo: " + match.Value);
            }

            foreach (Match match in AttributeAssignment.Matches(skeleton))
            {
                violations.Add("asignacion a un atributo: " + match.Value);
            }

            // ONE open, exclusive mode, the dump file of the embedded folder
            var opens = Regex.Matches(skeleton, @"\bopen\s*\(").Count;

            if (opens != 1)
            {
                violations.Add("se esperaba un unico open, hay " + opens);
            }

            if (!code.Contains(@"out = open(data[""dumpFolder""] + ""\\"" + DUMP_NAME, ""x"", encoding=""utf-8"", newline=""\n"")", StringComparison.Ordinal))
            {
                violations.Add("el open no es el del volcado en modo exclusivo dentro de la carpeta embebida");
            }

            if (!code.Contains(@"DUMP_NAME = ""ram-dump.json""", StringComparison.Ordinal))
            {
                violations.Add("falta el nombre del archivo del volcado");
            }

            if (!code.Contains(@"FAILED = ""DUMP FAILED""", StringComparison.Ordinal) || !code.Contains(@"PASSED = ""DUMP OK""", StringComparison.Ordinal))
            {
                violations.Add("los textos de estado no son DUMP OK y un estado de fallo");
            }

            // the pair of units: exactly two units.set, the first one only when the initial pair is another one, the second in finally
            var unitsSets = Regex.Matches(skeleton, @"\.units\.set\s*\(").Count;

            if (unitsSets != 2)
            {
                violations.Add("se esperaban dos units.set, hay " + unitsSets);
            }

            if (!functions.TryGetValue("read_model", out var read))
            {
                violations.Add("falta read_model");
            }
            else
            {
                var order = new[]
                {
                    "changed = False", "try:", "if initial != rm.Units.LB_IN:", "changed = True", "m.units.set(rm.Units.LB_IN)",
                    "result[\"readUnits\"]", "read_nodes(m)", "read_members(members)", "read_cases(m, members, result)",
                    "finally:", "if changed:", "m.units.set(initial)", "result[\"unitsRestored\"] = True",
                };
                var last = -1;

                foreach (var token in order)
                {
                    var index = read.IndexOf(token, StringComparison.Ordinal);

                    if (index < 0)
                    {
                        violations.Add("read_model no contiene " + token);
                    }
                    else if (index <= last)
                    {
                        violations.Add("read_model tiene " + token + " fuera de orden");
                    }
                    else
                    {
                        last = index;
                    }
                }

                if (Regex.Matches(read, @"\.units\.set\s*\(").Count != 2)
                {
                    violations.Add("las unidades se fijan fuera de read_model");
                }
            }

            foreach (var pair in functions.Where(f => f.Key != "read_model"))
            {
                if (Regex.IsMatch(pair.Value, @"\.units\.set\s*\("))
                {
                    violations.Add("units.set fuera de read_model, en " + pair.Key);
                }
            }

            // the gate of capability before the model, the model before the open, the open before the reading, the status after
            if (!functions.TryGetValue("main", out var main))
            {
                violations.Add("falta main");
            }
            else
            {
                var order = new[]
                {
                    "capability_failures(data)", "rm.model()", "m.units.current", "payload_failures(data)", "open(", "read_model(m, initial, result)",
                    "persist_dump(out, result)", "print(result[\"status\"])",
                };
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

                if (Regex.Matches(main, @"read_model\(").Count != 1)
                {
                    violations.Add("main debe leer el modelo una sola vez");
                }

                if (!main.Contains("errors.extend([\"gate: \" + item for item in gate])", StringComparison.Ordinal))
                {
                    violations.Add("los rechazos de las puertas no llevan el prefijo gate");
                }

                if (main.Contains("out.write(", StringComparison.Ordinal) || main.Contains("json.dumps(", StringComparison.Ordinal))
                {
                    violations.Add("main serializa o escribe el archivo fuera del manejo de errores");
                }
            }

            // the capability gate checks the version and the core functions, as the build script does
            if (!functions.TryGetValue("capability_failures", out var capability)
                || !capability.Contains("__version__", StringComparison.Ordinal)
                || !capability.Contains("capabilities[\"coreFunctions\"]", StringComparison.Ordinal)
                || !capability.Contains("object.__getattribute__(get_core(), \"_module\")", StringComparison.Ordinal)
                || !capability.Contains("if name not in present:", StringComparison.Ordinal))
            {
                violations.Add("la puerta de capacidad no comprueba la version y las funciones del nucleo");
            }

            // the serialisation and the writing are inside the error handling, with a minimal record on failure and the file always closed
            if (!functions.TryGetValue("persist_dump", out var persist))
            {
                violations.Add("falta persist_dump");
            }
            else
            {
                var steps = new[] { "try:", "json.dumps(result", "out.write(text", "except Exception as exc:", "finally:", "out.close()" };
                var positions = steps.Select(step => persist.IndexOf(step, StringComparison.Ordinal)).ToList();

                if (positions.Contains(-1) || !positions.SequenceEqual(positions.OrderBy(position => position)))
                {
                    violations.Add("la serializacion, la escritura y el cierre del volcado no estan protegidos");
                }

                if (!persist.Contains("allow_nan=False", StringComparison.Ordinal))
                {
                    violations.Add("el volcado admite valores no finitos");
                }
            }

            // the contents: every key of the contract
            foreach (var pair in RequiredKeys)
            {
                foreach (var key in pair.Value.Where(key => !code.Contains("\"" + key + "\"", StringComparison.Ordinal)))
                {
                    violations.Add("falta la clave " + pair.Key + "." + key);
                }
            }

            // ASCII only
            if (script.Any(c => c > 0x7F))
            {
                violations.Add("el script no es solo ASCII");
            }

            // every attribute and call is in the list
            foreach (Match match in Attribute.Matches(skeleton))
            {
                if (!AllowedNames.Contains(match.Groups[1].Value))
                {
                    violations.Add("llamada o atributo fuera de la lista: ." + match.Groups[1].Value);
                }
            }

            return violations;
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
    }

    /// <summary>I-65 F4-T3 — D-11 and A65-V3-01: the fixed text of the dump script and its static guard, proven on the real script and on variants that must fail.</summary>
    public class StructuralAnalysisRamElementsDumpTemplateTests
    {
        private readonly ITestOutputHelper _output;

        public StructuralAnalysisRamElementsDumpTemplateTests(ITestOutputHelper output)
        {
            _output = output;
        }

        private static string Replace(string script, string from, string to)
        {
            var changed = script.Replace(from, to, StringComparison.Ordinal);

            Assert.NotEqual(script, changed);
            return changed;
        }

        // ---------------------------------------------------------------- the real text

        [Fact]
        public void LaPlantillaTieneSaltosLfElMarcadorUnaVezYEsSoloAscii()
        {
            var text = RamDumpScriptTemplate.Text;

            Assert.NotEmpty(text);
            Assert.DoesNotContain("\r", text);
            Assert.Equal(1, Regex.Matches(text, Regex.Escape(RamDumpScriptTemplate.PayloadMarker)).Count);
            Assert.Contains("PAYLOAD = \"@@PAYLOAD@@\"", text);
            Assert.EndsWith("\n", text);
            Assert.All(text, c => Assert.True(c <= 0x7E, "Caracter fuera de ASCII: " + (int)c));
        }

        [Fact]
        public void LaPlantillaSinRenderizarCumpleLaGuarda()
        {
            Assert.Empty(StructuralAnalysisRamElementsDumpTemplateGuard.Violations(RamDumpScriptTemplate.Text));
        }

        [Fact]
        public void ElScriptRenderizadoCumpleLaGuardaYSoloDifiereDeLaPlantillaEnLaLineaDeLaCargaUtil()
        {
            var script = S.Script();

            Assert.Empty(StructuralAnalysisRamElementsDumpTemplateGuard.Violations(script));

            var template = RamDumpScriptTemplate.Text.Split('\n');
            var rendered = script.Split('\n');

            Assert.Equal(template.Length, rendered.Length);

            var different = Enumerable.Range(0, template.Length).Where(i => template[i] != rendered[i]).ToList();

            Assert.Single(different);
            Assert.StartsWith(RamDumpScriptTemplate.PayloadLinePrefix, rendered[different[0]]);
            Assert.DoesNotContain(RamDumpScriptTemplate.PayloadMarker, script);
        }

        [Fact]
        public void LosImportsSonSoloElModuloDelProveedorSuGetCoreJsonYMath()
        {
            var tool = StructuralAnalysisRamElementsHardeningScans.ToolName();
            var imports = S.Script().Split('\n').Where(l => l.StartsWith("import ", StringComparison.Ordinal) || l.StartsWith("from ", StringComparison.Ordinal)).ToList();

            Assert.Equal(
                new[] { "import json", "import math", "import " + tool + " as rm", "from " + tool + "._p4d.engine import get_core" },
                imports);
        }

        [Fact]
        public void LosTextosDeEstadoSonDumpOkOUnEstadoDeFallo()
        {
            var lines = S.Script().Split('\n');

            Assert.Equal("DUMP OK", RamDumpScriptTemplate.StatusOk);
            Assert.Equal("DUMP FAILED", RamDumpScriptTemplate.StatusFailed);
            Assert.Contains("PASSED = \"DUMP OK\"", lines);
            Assert.Contains("FAILED = \"DUMP FAILED\"", lines);
        }

        [Fact]
        public void ElScriptAbreUnSoloArchivoEnModoExclusivoDentroDeLaCarpetaEmbebida()
        {
            var script = S.Script();

            Assert.Equal("ram-dump.json", RamDumpScriptTemplate.DumpFileName);
            Assert.Equal(1, Regex.Matches(script, @"\bopen\(").Count);
            Assert.Contains(@"out = open(data[""dumpFolder""] + ""\\"" + DUMP_NAME, ""x"", encoding=""utf-8"", newline=""\n"")", script);
            Assert.Contains(@"DUMP_NAME = ""ram-dump.json""", script);
        }

        [Fact]
        public void LaPuertaDeCapacidadVaAntesDelModeloYDeLaAperturaDelArchivo()
        {
            var script = S.Script();
            var main = script.Substring(script.IndexOf("def main():", StringComparison.Ordinal));
            var tokens = new[] { "capability_failures(data)", "rm.model()", "m.units.current", "payload_failures(data)", "open(", "read_model(m, initial, result)" };
            var indexes = tokens.Select(t => main.IndexOf(t, StringComparison.Ordinal)).ToList();

            Assert.DoesNotContain(-1, indexes);
            Assert.Equal(indexes.OrderBy(i => i).ToList(), indexes);
        }

        [Fact]
        public void LasUnidadesSoloCambianSiElParInicialNoEsLbInYSeRestauranEnFinally()
        {
            var script = S.Script();
            var read = script.Substring(script.IndexOf("def read_model(", StringComparison.Ordinal));
            var conditional = read.IndexOf("        if initial != rm.Units.LB_IN:\n            changed = True\n            m.units.set(rm.Units.LB_IN)\n", StringComparison.Ordinal);
            var finallyBlock = read.IndexOf("    finally:\n        if changed:\n            m.units.set(initial)\n            result[\"unitsRestored\"] = True\n", StringComparison.Ordinal);

            Assert.True(conditional > 0, "El cambio a LB_IN no es condicional al par inicial.");
            Assert.True(finallyBlock > conditional, "La restauracion no esta en finally despues del cambio.");
            Assert.Equal(2, Regex.Matches(script, @"\.units\.set\(").Count);
            Assert.Contains("result[\"initialUnits\"] = initial.name", script);
            Assert.Contains("result[\"readUnits\"] = m.units.current.name", script);
        }

        [Fact]
        public void ElScriptNoEscribeEnElModeloNiAnalizaNiGuardaNiUsaOtroModulo()
        {
            var skeleton = G.StripStrings(G.StripComments(S.Script()));

            Assert.DoesNotMatch(@"\.(?:add\w*|delete\w*|remove\w*|set_\w+|enable_\w+|update\w*)\s*\(", skeleton);
            Assert.DoesNotMatch(@"\b(?:os|sys|subprocess|threading|exec|eval|save|save_as|new|analyze|exit|quit)\b", skeleton);
            Assert.DoesNotMatch(@"\.[A-Za-z_]\w*\s*(?:[-+*/%]|//|\*\*)?=(?!=)", skeleton);
        }

        [Theory]
        [InlineData("file")]
        [InlineData("nodes")]
        [InlineData("members")]
        [InlineData("cases")]
        [InlineData("combinations")]
        public void ElScriptVuelcaLosCamposContractualesDeCadaBloque(string block)
        {
            var code = G.StripComments(S.Script());

            foreach (var key in StructuralAnalysisRamElementsDumpTemplateGuard.RequiredKeys[block])
            {
                Assert.Contains("\"" + key + "\"", code);
            }
        }

        [Fact]
        public void ElScriptNoTieneNingunaLlamadaFueraDeLaLista()
        {
            var skeleton = G.StripStrings(G.StripComments(S.Script()));
            var names = Regex.Matches(skeleton, @"\.([A-Za-z_]\w*)").Select(m => m.Groups[1].Value).Distinct().ToList();

            Assert.NotEmpty(names);
            Assert.All(names, name => Assert.Contains(name, StructuralAnalysisRamElementsDumpTemplateGuard.AllowedNames));
        }

        [Fact]
        public void UnaCargaUtilConComillasBarrasYAlmohadillaNoRompeLaGuardaNiElLiteral()
        {
            var payload = "{\"a\":\"say \\\"x\\\" # not a comment \\\\ end\"}";
            var script = RamDumpScriptTemplate.Render(payload);

            Assert.Equal(payload, StructuralAnalysisRamElementsPackageInputs.ExtractPayload(script));
            Assert.Empty(StructuralAnalysisRamElementsDumpTemplateGuard.Violations(script));
        }

        [Fact]
        public void RenderRechazaUnaCargaUtilNulaConSaltosOFueraDeAscii()
        {
            Assert.Throws<ArgumentNullException>(() => RamDumpScriptTemplate.Render(null));
            Assert.Throws<ArgumentException>(() => RamDumpScriptTemplate.Render("{\"a\":\n1}"));
            Assert.Throws<ArgumentException>(() => RamDumpScriptTemplate.Render("{\"a\":\"\u00e9\"}"));
        }

        [Fact]
        public void ElScriptDeUnaCarpetaFijaTieneUnSha256EstableQueSeRegistra()
        {
            var first = Hash(S.Script());
            var second = Hash(S.Script());

            _output.WriteLine("DUMP-SCRIPT-SHA256 " + first + " folder=" + S.Folder + " version=" + S.Version);
            Assert.Matches("^[0-9a-f]{64}$", first);
            Assert.Equal(first, second);
            Assert.NotEqual(first, Hash(S.Script(folder: "C:\\RackCadDumps\\Other-Folder")));
            Assert.NotEqual(first, Hash(S.Script(version: "2.0.0-fixture")));
        }

        private static string Hash(string script) =>
            Convert.ToHexString(SHA256.HashData(new UTF8Encoding(false).GetBytes(script))).ToLowerInvariant();

        // ---------------------------------------------------------------- the guard proves itself: each variant must fail

        private static readonly Dictionary<string, Func<string, string>> Mutations = BuildMutations();

        public static IEnumerable<object[]> Variants() => Mutations.Keys.Select(name => new object[] { name });

        private static Dictionary<string, Func<string, string>> BuildMutations()
        {
            var all = new Dictionary<string, Func<string, string>>(StringComparer.Ordinal);
            const string readNodes = "def read_nodes(m):\n    nodes = []\n";
            const string setUnits = "            m.units.set(rm.Units.LB_IN)\n";

            all["con un add de nudo"] = s => Replace(s, readNodes, readNodes + "    m.nodes.add(0.0, 0.0, 0.0)\n");
            all["con un add de miembro"] = s => Replace(s, readNodes, readNodes + "    m.members.add(1, 2, section=\"x\")\n");
            all["con un add de caso de carga"] = s => Replace(s, readNodes, readNodes + "    m.load_condition.add_load_case(\"X\", \"x\", \"OTHER\")\n");
            all["con un add de carga puntual"] = s => Replace(s, readNodes, readNodes + "    m.members[1].add_point_load(\"X\", None, 1.0)\n");
            all["con un setter de miembro"] = s => Replace(s, readNodes, readNodes + "    m.members[1].set_section(\"x\")\n");
            all["con un setter por asignacion"] = s => Replace(s, readNodes, readNodes + "    m.members[1].rotation = 0.0\n");
            all["con un setter por asignacion aumentada"] = s => Replace(s, readNodes, readNodes + "    m.members[1].rotation += 1.0\n");
            all["con un setter de apoyos"] = s => Replace(s, readNodes, readNodes + "    m.nodes[1].restraints = None\n");
            all["con un delete"] = s => Replace(s, readNodes, readNodes + "    m.nodes.delete(1)\n");
            all["con analyze"] = s => Replace(s, readNodes, readNodes + "    m.analyze()\n");
            all["con save"] = s => Replace(s, readNodes, readNodes + "    m.save()\n");
            all["con save_as"] = s => Replace(s, readNodes, readNodes + "    m.save_as(\"x\")\n");
            all["con new"] = s => Replace(s, readNodes, readNodes + "    m.new()\n");
            all["con un units.set incondicional"] = s => Replace(s, "        if initial != rm.Units.LB_IN:\n            changed = True\n" + setUnits, "        changed = True\n        m.units.set(rm.Units.LB_IN)\n");
            all["con un tercer units.set"] = s => Replace(s, readNodes, readNodes + "    m.units.set(rm.Units.LB_IN)\n");
            all["sin restauracion en finally"] = s => Replace(s, "            m.units.set(initial)\n", string.Empty);
            all["con la restauracion fuera de finally"] = s => Replace(s, "    finally:\n        if changed:\n            m.units.set(initial)\n", "    except Exception:\n        raise\n    m.units.set(initial)\n");
            all["con la restauracion incondicional"] = s => Replace(s, "        if changed:\n            m.units.set(initial)\n", "        m.units.set(initial)\n");
            all["sin registrar la restauracion"] = s => Replace(s, "            result[\"unitsRestored\"] = True\n", string.Empty);
            all["con otro open"] = s => Replace(s, readNodes, readNodes + "    other = open(\"other.txt\", \"w\")\n");
            all["con apertura no exclusiva"] = s => Replace(s, "\"x\", encoding=\"utf-8\"", "\"w\", encoding=\"utf-8\"");
            all["con otro nombre de archivo"] = s => Replace(s, "DUMP_NAME = \"ram-dump.json\"", "DUMP_NAME = \"other.json\"");
            all["con import os"] = s => Replace(s, "import math\n", "import math\nimport os\n");
            all["con import sys"] = s => Replace(s, "import math\n", "import math\nimport sys\n");
            all["con import subprocess"] = s => Replace(s, "import math\n", "import math\nimport subprocess\n");
            all["con exec"] = s => Replace(s, readNodes, readNodes + "    exec(\"pass\")\n");
            all["con eval"] = s => Replace(s, readNodes, readNodes + "    eval(\"1\")\n");
            all["sin puerta de capacidad"] = s => Replace(s, "        gate = capability_failures(data)\n", "        gate = []\n");
            all["sin comprobar la version"] = s => Replace(s, "getattr(rm, \"__version__\", None)", "None");
            all["sin comprobar las funciones del nucleo"] = s => Replace(s, "for name in capabilities[\"coreFunctions\"]:", "for name in []:");
            all["con la lectura antes de abrir el archivo"] = s => Replace(s, "        out = open(", "        read_model(m, initial, result)\n        out = open(");
            all["con la lectura antes de la puerta"] = s => Replace(s, "        gate = capability_failures(data)\n", "        read_model(m, initial, result)\n        gate = capability_failures(data)\n");
            all["con una llamada fuera de la lista"] = s => Replace(s, readNodes, readNodes + "    m.nodes.find_nearest(0.0, 0.0, 0.0)\n");
            all["con una letra fuera de ASCII"] = s => Replace(s, "# RackCad dump script", "# RackCad dump scr\u00edpt");
            all["con otro estado de exito"] = s => Replace(s, "PASSED = \"DUMP OK\"", "PASSED = \"OK\"");
            all["con otro estado de fallo"] = s => Replace(s, "FAILED = \"DUMP FAILED\"", "FAILED = \"FAILED\"");
            all["sin proteger la escritura del volcado"] = s => Replace(s, "            out.write(text + \"\\n\")\n", "            pass\n");
            all["sin cerrar el archivo"] = s => Replace(s, "        out.close()\n", "        pass\n");
            all["sin la lista de miembros del volcado"] = s => Replace(s, "\"memberLoads\"", "\"loadsOfMembers\"");
            all["sin los terminos de las combinaciones"] = s => Replace(s, "\"terms\"", "\"parts\"");

            return all;
        }

        [Theory]
        [MemberData(nameof(Variants))]
        public void LaGuardaRechazaCadaVarianteSinteticaDelScript(string name)
        {
            var violations = StructuralAnalysisRamElementsDumpTemplateGuard.Violations(Mutations[name](S.Script()));

            Assert.True(violations.Count > 0, "La guarda dejo pasar la variante: " + name);
        }

        [Fact]
        public void LaGuardaTambienRechazaLasVariantesSobreLaPlantillaSinRenderizar()
        {
            var template = RamDumpScriptTemplate.Text;

            Assert.NotEmpty(StructuralAnalysisRamElementsDumpTemplateGuard.Violations(Replace(template, "        gate = capability_failures(data)\n", "        gate = []\n")));
            Assert.NotEmpty(StructuralAnalysisRamElementsDumpTemplateGuard.Violations(Replace(template, "def read_nodes(m):\n    nodes = []\n", "def read_nodes(m):\n    nodes = []\n    m.analyze()\n")));
            Assert.NotEmpty(StructuralAnalysisRamElementsDumpTemplateGuard.Violations(Replace(template, "            m.units.set(initial)\n", string.Empty)));
        }

        [Fact]
        public void UnComentarioOUnMensajeConPalabrasProhibidasNoEsCodigo()
        {
            var script = S.Script();
            var withComment = Replace(script, "import json\n", "import json\n# it never calls save_as or analyze, and it does not import os\n");
            var withMessage = Replace(script, "\"the dump folder is missing\"", "\"the dump folder is missing (no save, no analyze, no new)\"");

            Assert.Empty(StructuralAnalysisRamElementsDumpTemplateGuard.Violations(withComment));
            Assert.Empty(StructuralAnalysisRamElementsDumpTemplateGuard.Violations(withMessage));
        }
    }
}
