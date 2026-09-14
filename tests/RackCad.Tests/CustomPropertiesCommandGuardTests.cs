using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Xunit;
using static RackCad.Tests.CustomPropertiesTestKit;

namespace RackCad.Tests
{
    /// <summary>
    /// I-54 G7 — T-GRD-08 del lado Core (Proposal V5 D-18.1, D-18.2, D-18.7 y D-19.3): el censo de comandos, clasificado por
    /// NOMBRE y no por un numero, y la forma del comando `RACKPROPIEDADES`.
    ///
    /// <para>
    /// La decision del Owner (G7A) fija los dos nombres: `RACKPROPIEDADES` con el alias `RPR`, un solo comportamiento para
    /// Rack y Proyecto y ningun otro alias ni comando por alcance. Ninguna suite carga el Plugin (ADR-0003), asi que el
    /// comando se fija por su forma: pide, elige, lee y escribe por el borde de G6, abre la ventana y pasa el intent; no
    /// decide autoridad, kind, unificacion, igualdad canonica ni frescura. La interaccion fisica es de G9.
    /// </para>
    /// <para>
    /// El censo de ventanas y el de la ayuda viven en las pruebas de UI, donde se ven los tipos (WindowCensusGuardTests y
    /// CustomPropertiesHelpCensusTests). Cada guarda se demuestra en rojo aqui mismo contra una variante del archivo real.
    /// </para>
    /// </summary>
    public class CustomPropertiesCommandGuardTests
    {
        internal const string CommandPath = "src/RackCad.Plugin/RackPropiedadesCommands.cs";

        /// <summary>El prompt de D-18.1, con la palabra clave del Proyecto.</summary>
        internal const string Prompt = "Selecciona un rack o [Proyecto]";

        /// <summary>
        /// El censo de APERTURA de G7, medido en `5c16ae8` (G7A rebasado sobre `origin/main` `104ef3a`): 33 registros en 15
        /// archivos, sin `RACKPROPIEDADES` ni `RPR`.
        /// </summary>
        internal static readonly string[] OpeningCommands =
        {
            "QCB", "QCM", "QUICKCABECERA", "QUICKCAMA", "RA", "RACKAYUDA", "RACKBOMTOTAL", "RACKCABECERA", "RACKCAD",
            "RACKCANTILEVER", "RACKDUPLICAR", "RACKEDITAR", "RACKLAYOUT", "RACKLISTA", "RACKPUSHBACK", "RACKRELLENAR",
            "RACKSECCION", "RACKSELECTIVO", "RACKSISTEMADINAMICO", "RACKVARIABLES", "RB", "RCB", "RCT", "RD", "RED", "RK",
            "RL", "RLY", "RPB", "RR", "RS", "RSD", "RVA",
        };

        /// <summary>Lo que G7 anade, EXACTAMENTE: el comando decidido por el Owner y su unico alias.</summary>
        internal static readonly string[] G7Commands = { "RACKPROPIEDADES", "RPR" };

        // ================================================================ censo

        [Fact]
        public void TGrd08_ElCensoDeApertura_SonTreintaYTresNombresDistintos()
        {
            Assert.Equal(33, OpeningCommands.Length);
            Assert.Equal(OpeningCommands.Length, OpeningCommands.Distinct(StringComparer.Ordinal).Count());
            Assert.Empty(OpeningCommands.Intersect(G7Commands, StringComparer.Ordinal));
        }

        [Fact]
        public void TGrd08_ElCensoDeComandosEsElDeAperturaMasRackPropiedadesYRpr()
        {
            Assert.Empty(CensusViolations(PluginCommandNames()));
        }

        [Fact]
        public void TGrd08_LosDosNombresNuevosSeRegistranSoloEnElArchivoDelComando()
        {
            var owners = PluginCommandRegistrations()
                .Where(registration => G7Commands.Contains(registration.Name, StringComparer.Ordinal))
                .Select(registration => registration.Path + ":" + registration.Name)
                .OrderBy(entry => entry, StringComparer.Ordinal)
                .ToList();

            Assert.Equal(new[] { CommandPath + ":RACKPROPIEDADES", CommandPath + ":RPR" }, owners);
        }

        public static TheoryData<string, string[]> CensusMutations() => new TheoryData<string, string[]>
        {
            { "falta el alias", OpeningCommands.Append("RACKPROPIEDADES").ToArray() },
            { "falta el comando", OpeningCommands.Append("RPR").ToArray() },
            { "un alias de mas", OpeningCommands.Concat(G7Commands).Append("RPRO").ToArray() },
            { "un comando por alcance", OpeningCommands.Concat(G7Commands).Append("RACKPROPIEDADESPROYECTO").ToArray() },
            { "el alias con otra grafia", OpeningCommands.Append("RACKPROPIEDADES").Append("rpr").ToArray() },
            { "el comando traducido", OpeningCommands.Append("RACKPROPERTIES").Append("RPR").ToArray() },
            { "el alias registrado dos veces", OpeningCommands.Concat(G7Commands).Append("RPR").ToArray() },
            { "un comando de apertura desaparecido", OpeningCommands.Where(name => name != "RVA").Concat(G7Commands).ToArray() },
            { "solo el numero cuadra", OpeningCommands.Where(name => name != "RVA").Concat(G7Commands).Append("RPRO").ToArray() },
        };

        [Theory]
        [MemberData(nameof(CensusMutations))]
        public void TGrd08_ElCensoDetectaCualquierDesviacionDeNombre(string caso, string[] names)
        {
            Assert.True(CensusViolations(names).Count > 0, "el censo no detecta: " + caso);
        }

        // ================================================================ forma del comando

        [Fact]
        public void TGrd08_ElComandoPideEligeYDelegaEnElBorde_SinDecidir()
        {
            Assert.Empty(CommandViolations(CommandText()));
        }

        public static TheoryData<string, string, string> CommandMutations() => new TheoryData<string, string, string>
        {
            { "un alias de mas", "[CommandMethod(\"RPR\")]", "[CommandMethod(\"RPR\")] [CommandMethod(\"RPRO\")]" },
            { "el alias con otra grafia", "[CommandMethod(\"RPR\")]", "[CommandMethod(\"rpr\")]" },
            { "el alias con su propio comportamiento", "=> RackPropiedades();", "=> RackPropiedadesProyecto();" },
            { "un comando por alcance", "[CommandMethod(\"RACKPROPIEDADES\")]", "[CommandMethod(\"RACKPROPIEDADESPROYECTO\")]" },
            { "decide con la autoridad", "var read = CustomPropertiesExecutor.ReadRack(document, definitionId);", "var read = CustomPropertiesExecutor.ReadRack(document, definitionId); RackCustomPropertiesAuthority.Evaluate(null, null, null);" },
            { "confirma en Application sin el borde", "var preflight = CustomPropertiesPreflight.ForRack(read.Authority, intent);", "var preflight = CustomPropertiesPreflight.ForRack(read.Authority, intent); CustomPropertiesCommit.ForRack(null, null, null, null, null);" },
            { "escribe el bloque", "var result = CustomPropertiesExecutor.ExecuteRack(document, read.Selection, displayed, intent);", "var result = CustomPropertiesExecutor.ExecuteRack(document, read.Selection, displayed, intent); RackBlockData.Write(null, ObjectId.Null, null);" },
            { "lee el NOD con el store", "var execution = CustomPropertiesExecutor.ExecuteProject(document, window.Intent);", "new CustomPropertiesStore().Read(CustomPropertiesData.Read(null, null)); var execution = CustomPropertiesExecutor.ExecuteProject(document, window.Intent);" },
            { "aplica la mutacion", "var execution = CustomPropertiesExecutor.ExecuteProject(document, window.Intent);", "CustomPropertiesMutations.Apply(read.Collection, window.Intent); var execution = CustomPropertiesExecutor.ExecuteProject(document, window.Intent);" },
            { "abre la referencia para escribir", "OpenMode.ForRead", "OpenMode.ForWrite" },
            { "abre otra transaccion", "EditRack(document, definitionId);", "document.Database.TransactionManager.StartTransaction(); EditRack(document, definitionId);" },
            { "lee el payload al elegir", "EditRack(document, definitionId);", "RackCommandSupport.PickRackBlock(document, null, out _, out _); EditRack(document, definitionId);" },
            { "escanea el dibujo", "EditRack(document, definitionId);", "RackBlockFinder.ScanEnvelopes(null, null, true); EditRack(document, definitionId);" },
            { "mira la autoridad", ": RackCustomPropertiesDisplayedState.Capture(read.Authority);", ": read.Authority.Outcome == 0 ? null : RackCustomPropertiesDisplayedState.Capture(read.Authority);" },
            { "compara formas canonicas", "var preflight = CustomPropertiesPreflight.ForRackUnify(read.Authority, intent);", "var preflight = CustomPropertiesPreflight.ForRackUnify(read.Authority, intent); CustomPropertiesCanonicalForm.Of(null);" },
            { "decide la unificacion", "var preflight = CustomPropertiesPreflight.ForRackUnify(read.Authority, intent);", "var preflight = CustomPropertiesPreflight.ForRackUnify(read.Authority, intent); var ok = read.Workspace.ViewSummaries[0].IsUnifySourceAvailable;" },
            { "decide el kind", "EditRack(document, definitionId);", "KindHandlerRegistry.Default.TryGetIgnoreCase(null, out _); EditRack(document, definitionId);" },
            { "compone el sobre en el comando", "var result = CustomPropertiesExecutor.ExecuteRack(document, read.Selection, displayed, intent);", "var result = CustomPropertiesExecutor.ExecuteRack(document, read.Selection, displayed, intent); RackEmbedComposer.WithCustomProperties(null, null);" },
            { "salta el preflight de la edicion", "var preflight = CustomPropertiesPreflight.ForRack(read.Authority, intent);", "var preflight = CustomPropertiesPreflight.ForRackUnify(read.Authority, null);" },
            { "salta el preflight de la unificacion", "var preflight = CustomPropertiesPreflight.ForRackUnify(read.Authority, intent);", "var preflight = CustomPropertiesPreflight.ForRack(read.Authority, null);" },
            { "escribe sin el borde", "CustomPropertiesExecutor.ExecuteRackUnify(", "CustomPropertiesExecutorBis.ExecuteRackUnify(" },
            { "otra interaccion", Prompt, "Selecciona un rack" },
            { "otra palabra clave", "ProjectKeyword = \"Proyecto\"", "ProjectKeyword = \"Project\"" },
            { "usa MessageBox", "EditProject(document);", "System.Windows.MessageBox.Show(null); EditProject(document);" },
            { "regenera", "EditRack(document, definitionId);", "document.Editor.Regen(); EditRack(document, definitionId);" },
            { "captura otra familia", "catch (System.Exception ex)", "catch (Autodesk.AutoCAD.Runtime.Exception ex)" },
            { "calla el error", "RackCommandSupport.Report(ex);", "ex.ToString();" },
            { "no usa la ventana", "new RackCustomPropertiesWindow(read.Workspace, displayed, status)", "new RackProjectVariablesWindow(null)" },
            { "conoce Project Variables", "EditProject(document);", "ProjectVariablesRegistry.Read(null, null); EditProject(document);" },
        };

        [Theory]
        [MemberData(nameof(CommandMutations))]
        public void TGrd08_LaGuardaDetectaUnComandoQueSeSaleDelContrato(string caso, string original, string replacement)
        {
            var text = CommandText();

            Assert.Contains(original, text);
            Assert.True(CommandViolations(text.Replace(original, replacement)).Count > 0, "la guarda no detecta: " + caso);
        }

        // ================================================================ utilidades

        internal static string CommandText()
            => File.ReadAllText(Path.Combine(RepoRoot().FullName, CommandPath.Replace('/', Path.DirectorySeparatorChar))).Replace("\r\n", "\n");

        /// <summary>Cada registro <c>[CommandMethod("NOMBRE")]</c> del Plugin, con su archivo, fuera de lo generado.</summary>
        internal static IReadOnlyList<(string Path, string Name)> PluginCommandRegistrations()
        {
            var root = RepoRoot().FullName;

            return Directory.GetFiles(Path.Combine(root, "src", "RackCad.Plugin"), "*.cs", SearchOption.AllDirectories)
                .Where(path => !path.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
                    .Any(segment => segment == "bin" || segment == "obj"))
                .SelectMany(path => Regex.Matches(File.ReadAllText(path), @"\[CommandMethod\(""(?<name>[^""]*)""")
                    .Select(match => (Path.GetRelativePath(root, path).Replace('\\', '/'), match.Groups["name"].Value)))
                .OrderBy(registration => registration.Item1, StringComparer.Ordinal)
                .ThenBy(registration => registration.Item2, StringComparer.Ordinal)
                .ToList();
        }

        internal static IReadOnlyList<string> PluginCommandNames()
            => PluginCommandRegistrations().Select(registration => registration.Name).ToList();

        /// <summary>
        /// Lo que separa un censo de la apertura mas los dos nombres de G7, comparando NOMBRES con mayusculas exactas. Vacia si
        /// cada nombre esperado aparece exactamente una vez y no aparece ningun otro: un numero que cuadra con un nombre de mas
        /// y otro de menos no pasa.
        /// </summary>
        internal static IReadOnlyList<string> CensusViolations(IEnumerable<string> names)
        {
            var violations = new List<string>();
            var expected = OpeningCommands.Concat(G7Commands).ToList();
            var found = names.ToList();

            foreach (var group in found.GroupBy(name => name, StringComparer.Ordinal).Where(group => group.Count() > 1))
            {
                violations.Add("registrado " + group.Count() + " veces: " + group.Key);
            }

            foreach (var missing in expected.Except(found, StringComparer.Ordinal))
            {
                violations.Add("falta: " + missing);
            }

            foreach (var extra in found.Except(expected, StringComparer.Ordinal))
            {
                violations.Add("sin clasificar en el censo de G7: " + extra);
            }

            return violations;
        }

        /// <summary>
        /// Lo que el comando hace mal. Vacia si cumple D-18.1 y D-18.2 con la decision del Owner:
        /// <list type="bullet">
        /// <item>exactamente dos registros, `RACKPROPIEDADES` y `RPR`, y el alias es una llamada al MISMO metodo;</item>
        /// <item>el prompt «Selecciona un rack o [Proyecto]» con la palabra clave Proyecto;</item>
        /// <item>la eleccion resuelve la referencia a su definicion en una lectura, sin tocar el payload;</item>
        /// <item>la lectura y la escritura de las dos ramas pasan por <c>CustomPropertiesExecutor</c>, y el Rack por el preflight
        /// de Application antes de escribir;</item>
        /// <item>la ventana es <c>RackCustomPropertiesWindow</c> y el error sale por <c>RackCommandSupport.Report</c>;</item>
        /// <item>nada de autoridad, commit, store, NOD, bloques, barrido, kind, formas canonicas, opciones de unificacion,
        /// Project Variables, transacciones propias de escritura, regeneracion ni MessageBox.</item>
        /// </list>
        /// </summary>
        internal static IReadOnlyList<string> CommandViolations(string text)
        {
            var violations = new List<string>();
            var code = PluginSourceCode.Mask(text);
            var literals = PluginSourceCode.StringLiterals(text);

            var registrations = Regex.Matches(text, @"\[CommandMethod\(""(?<name>[^""]*)""").Select(match => match.Groups["name"].Value).ToList();

            if (!registrations.OrderBy(name => name, StringComparer.Ordinal).SequenceEqual(new[] { "RACKPROPIEDADES", "RPR" }, StringComparer.Ordinal))
            {
                violations.Add("los registros no son exactamente RACKPROPIEDADES y RPR: " + string.Join(", ", registrations));
            }

            if (!Regex.IsMatch(text, @"\[CommandMethod\(""RACKPROPIEDADES""\)\]\s*public\s+void\s+RackPropiedades\s*\(\s*\)\s*\{"))
            {
                violations.Add("RACKPROPIEDADES no es el metodo publico RackPropiedades()");
            }

            if (!Regex.IsMatch(text, @"\[CommandMethod\(""RPR""\)\]\s*public\s+void\s+\w+\s*\(\s*\)\s*=>\s*RackPropiedades\s*\(\s*\)\s*;"))
            {
                violations.Add("RPR no es una llamada al mismo RackPropiedades()");
            }

            if (!literals.Any(literal => literal.Contains(Prompt, StringComparison.Ordinal)))
            {
                violations.Add("falta la interaccion «" + Prompt + "»");
            }

            if (!Regex.IsMatch(text, @"\bProjectKeyword\s*=\s*""Proyecto""\s*;") || !Regex.IsMatch(code, @"\bGetEntity\s*\("))
            {
                violations.Add("la palabra clave del Proyecto no es Proyecto sobre la eleccion de entidad");
            }

            foreach (var required in new[]
                     {
                         @"\bCustomPropertiesExecutor\s*\.\s*ReadProject\s*\(",
                         @"\bCustomPropertiesExecutor\s*\.\s*ExecuteProject\s*\(",
                         @"\bCustomPropertiesExecutor\s*\.\s*ReadRack\s*\(",
                         @"\bCustomPropertiesExecutor\s*\.\s*ExecuteRack\s*\(",
                         @"\bCustomPropertiesExecutor\s*\.\s*ExecuteRackUnify\s*\(",
                         @"\bCustomPropertiesPreflight\s*\.\s*ForRack\s*\(",
                         @"\bCustomPropertiesPreflight\s*\.\s*ForRackUnify\s*\(",
                         @"\bRackCustomPropertiesDisplayedState\s*\.\s*Capture\s*\(",
                         @"\bnew\s+RackCustomPropertiesWindow\s*\(",
                         @"\bShowModalWindow\s*\(",
                         @"\bRackCommandSupport\s*\.\s*Report\s*\(\s*ex\s*\)",
                         @"\.\s*BlockTableRecord\b",
                     })
            {
                if (!Regex.IsMatch(code, required))
                {
                    violations.Add("no hace lo que el contrato pide: " + required);
                }
            }

            foreach (var forbidden in new[]
                     {
                         "RackCustomPropertiesAuthority", "CustomPropertiesCommit", "CustomPropertiesStore", "CustomPropertiesData",
                         "CustomPropertiesMutations", "CustomPropertiesWriteGuard", "CustomPropertiesCanonicalForm", "RackEmbedStore",
                         "RackEmbedDocument", "RackEmbedComposer", "RackBlockData", "RackBlockFinder", "ScanEnvelopes", "FindRackBlocks",
                         "PickRackBlock", "StartTransaction", "LockDocument", "Commit", "UpgradeOpen", "ForWrite", "KindHandlerRegistry",
                         "TryGetIgnoreCase", "CanonicalForm", "UnifyOptions", "IsUnifySourceAvailable", "Regen", "ApplyRegen",
                         "RedefineSystemBlock", "EnsureForPlan", "PurgeUnreferenced", "PurgeAfterCommit", "MessageBox",
                         "EditorDiscardPrompt",
                     })
            {
                if (Regex.IsMatch(code, @"\b" + forbidden + @"\b"))
                {
                    violations.Add("el comando nombra " + forbidden);
                }
            }

            if (Regex.IsMatch(code, @"ProjectVariable"))
            {
                violations.Add("el comando conoce Project Variables");
            }

            if (Regex.IsMatch(code, @"\bAuthority\s*\.\s*\w+"))
            {
                violations.Add("el comando mira dentro de la autoridad en lugar de pasarla");
            }

            if (Regex.Matches(code, @"\bInDocumentTransaction\s*\.\s*Run\s*\(").Count != 1)
            {
                violations.Add("la eleccion no es UNA lectura de la referencia");
            }

            if (Regex.Matches(code, @"\bnew\s+RackCustomPropertiesWindow\s*\(").Count != 2)
            {
                violations.Add("Rack y Proyecto no abren la misma ventana, una vez por rama");
            }

            foreach (Match match in Regex.Matches(code, @"\bcatch\b\s*(?<clause>\([^)]*\))?"))
            {
                if (!Regex.IsMatch(match.Groups["clause"].Value, @"^\(\s*System\s*\.\s*Exception\s+ex\s*\)$"))
                {
                    violations.Add("captura distinta de System.Exception: catch " + match.Groups["clause"].Value);
                }
            }

            return violations;
        }
    }
}
