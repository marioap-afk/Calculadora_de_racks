using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace RackCad.Tests
{
    /// <summary>
    /// I-65 F4-T5 — the guard over the TEXT of the line command of D-13 (<c>src/RackCad.Plugin/RackRamElementsCommands.cs</c>).
    ///
    /// <para>
    /// The Plugin references AutoCAD, so the Core suite never compiles or loads it (ADR-0003): like the other Plugin guards,
    /// this one reads the file as text, masked with <see cref="PluginSourceCode"/> (comments and the content of literals blanked,
    /// delimiters kept) so that documenting a rule never violates it. It is a pure function over the text, so it proves itself on
    /// synthetic code before it sweeps the real file. A text guard shows what the command does not say; the behaviour of the
    /// pieces the command composes lives in the F1-F3 tests and in the manual validation of the command.
    /// </para>
    /// <para>
    /// Each violation starts with the id of the rule it breaks, so a test can ask for one rule at a time.
    /// </para>
    /// </summary>
    internal static class StructuralAnalysisPluginCommandGuard
    {
        internal const string CommandPath = "src/RackCad.Plugin/RackRamElementsCommands.cs";

        internal const string ProjectPath = "src/RackCad.Plugin/RackCad.Plugin.csproj";

        internal const string CommandName = "RACKRAMPAQUETE";

        /// <summary>The rules, in order: each one is observed by <see cref="Violations"/>.</summary>
        internal static readonly string[] RuleIds =
        {
            "C01", "C02", "C03", "C04", "C05", "C06", "C07", "C08", "C09", "C10", "C11", "C12", "C13", "C14",
            "C15", "C16", "C17", "C18",
        };

        /// <summary>The index of the brace that closes the one opened at <paramref name="open"/>, or -1.</summary>
        private static int MatchingBrace(string code, int open)
        {
            var depth = 0;

            for (var index = open; index < code.Length; index++)
            {
                if (code[index] == '{')
                {
                    depth++;
                }
                else if (code[index] == '}')
                {
                    depth--;

                    if (depth == 0)
                    {
                        return index;
                    }
                }
            }

            return -1;
        }

        private static readonly RegexOptions Options = RegexOptions.CultureInvariant;

        /// <summary>Events, windows, palettes, menus and the Workspace or ID30 code (D-13, INV-12).</summary>
        private static readonly Regex UserInterfaceTokens = new Regex(
            @"\+=|\bSetImpliedSelection\b|\bPaletteSet\b|System\s*\.\s*Windows|\bMessageBox\b|\b\w*Window\b|\bShowModal\w*|\bShowDialog\b"
            + @"|\bDispatcher\b|\bUserControl\b",
            Options);

        private static readonly Regex NamedAreaTokens = new Regex(@"workspace|ribbon|menu|\bid30\b", Options | RegexOptions.IgnoreCase);

        /// <summary>What the command must never name: the resolver, the catalogue loader, the units reader, the envelope builders.</summary>
        private static readonly Regex ForbiddenSymbols = new Regex(
            @"\bCantileverLineResolver\b|\bCsvStructuralSectionCatalogProvider\b|\bStructuralSectionCatalogException\b|Insunits"
            + @"|\bSiblingRedrawTransaction\b|\bRackDefinitionCreator\s*\.\s*CreateInTransaction\b|\bnew\s+RackEmbedDocument\b"
            + @"|\bRackEmbedComposer\b",
            Options);

        /// <summary>The conversion literals and unit tags (G4, INV-16) and the design vocabulary (INV-11).</summary>
        private static readonly Regex ConversionAndDesignTokens = new Regex(
            @"(?<![0-9.])(?:25\.40*|645\.160*|0\.453592370*|9\.806650*|4\.44822161526050*|0\.30480*)(?![0-9])"
            + @"|\[(?:in|mm|ft)\]"
            + @"|member_design|MemberDesign|Utilization|InteractionRatio|DesignRatio|DemandCapacity|CodeCheck|UnityCheck",
            Options);

        /// <summary>The command draws nothing, edits no block, starts no process and writes no file by itself.</summary>
        private static readonly Regex SideEffectTokens = new Regex(
            @"\bStartTransaction\b|\bTransactionManager\b|\bLockDocument\b|\bUpgradeOpen\b|\bForWrite\b|\bRegen\b"
            + @"|\bProcess\b|\bProcessStartInfo\b"
            + @"|\bFile\s*\.\s*(?:Write\w*|Append\w*|Delete|Move|Copy|Create|Open\w*)\b"
            + @"|\bDirectory\s*\.\s*(?:Create\w*|Delete|Move)\b",
            Options);

        private static readonly Regex ProfileConstruction = new Regex(
            @"\bStructuralMaterialProfile\b|\bStructuralMaterialRow\b|\bIStructuralMaterialProfile\b|\bnew\s+\w*Profile\w*\s*[({]",
            Options);

        internal static IReadOnlyList<string> Violations(string source)
        {
            var text = source.Replace("\r\n", "\n");
            var code = PluginSourceCode.Mask(text);
            var literals = PluginSourceCode.StringLiterals(text);
            var scanned = code + "\n" + string.Join("\n", literals);
            var found = new List<string>();

            void Require(string rule, string pattern, string what, int exactly = -1)
            {
                var count = Regex.Matches(code, pattern, Options).Count;

                if (exactly >= 0 ? count != exactly : count == 0)
                {
                    found.Add(rule + ": " + what);
                }
            }

            void Forbid(string rule, Regex pattern, string text0, string what)
            {
                foreach (Match match in pattern.Matches(text0))
                {
                    found.Add(rule + ": " + what + " '" + match.Value.Trim() + "'");
                }
            }

            // ---- C01 shape of the file: namespace in block, one public class
            if (!Regex.IsMatch(code, @"(?m)^namespace\s+RackCad\.Plugin\s*\n\s*\{", Options))
            {
                found.Add("C01: el namespace no es RackCad.Plugin en bloque");
            }

            if (Regex.IsMatch(code, @"(?m)^namespace\s+[\w.]+\s*;", Options))
            {
                found.Add("C01: namespace de ambito de archivo");
            }

            if (Regex.Matches(code, @"\bclass\s+\w+", Options).Count != 1
                || Regex.Matches(code, @"\bpublic\s+(?:sealed\s+|static\s+|partial\s+)*class\s+\w+", Options).Count != 1)
            {
                found.Add("C01: no hay exactamente UNA clase y es publica");
            }

            // ---- C02 the registration: one attribute, canonical, the name only inside it
            if (!Regex.IsMatch(text, @"\[CommandMethod\(""" + CommandName + @"""\)\]", Options))
            {
                found.Add("C02: falta el registro canonico del comando");
            }

            if (text.Split("[CommandMethod(").Length - 1 != 1 || Regex.Matches(code, @"\[CommandMethod\(", Options).Count != 1)
            {
                found.Add("C02: la subcadena [CommandMethod( no aparece exactamente una vez (el conteo del censo es bruto)");
            }

            if (Regex.Matches(text, CommandName, Options).Count != 1)
            {
                found.Add("C02: el nombre del comando aparece fuera del atributo (o mas de una vez)");
            }

            // ---- C03 selection and prompts
            Require("C03", @"\bRackCommandSupport\s*\.\s*PickRackBlock\s*\(", "no selecciona con el ayudante existente", 1);
            Require("C03", @"\bRackEmbedDocument\s*\.\s*KindCantilever\b", "no compara con el kind cantilever del sobre");
            Require("C03", @"\bStringComparison\s*\.\s*OrdinalIgnoreCase\b", "el kind se compara distinguiendo mayusculas");
            Require("C03", @"\beditor\s*\.\s*GetInteger\s*\(", "no pide el indice de estacion");
            Require("C03", @"\beditor\s*\.\s*GetString\s*\(", "no pide las rutas del archivo de cargas y de la carpeta");

            // ---- C04 the catalogue, through the only entry the Plugin has
            Require("C04", @"\bStructuralSectionCatalogAccess\s*\.\s*TryLoad\s*\(", "no carga el catalogo con la unica entrada", 1);

            // ---- C05 the table, next to the Plugin assembly
            Require("C05", @"\bRamSectionMapReader\s*\.\s*Read\s*\(", "no lee la tabla con su lector", 1);
            Require("C05", @"\bAssembly\b[^;]*?\.\s*Location\b", "la tabla no se busca junto a Assembly.Location");

            if (Regex.IsMatch(scanned, @"\bAppContext\b|\bBaseDirectory\b", Options))
            {
                found.Add("C05: la tabla se busca desde AppContext o BaseDirectory");
            }

            foreach (var part in new[] { "structural-analysis", "ram-elements", "section-material-map.json" })
            {
                if (!literals.Contains(part))
                {
                    found.Add("C05: falta el segmento de ruta de la tabla '" + part + "'");
                }
            }

            // ---- C06 the entry of the station: the bytes of the design and the decoder with the project store
            Require("C06", @"\bCantileverStructuralExport\s*\.\s*Build\s*\(", "no llama a la entrada de la estacion", 1);
            Require("C06", @"\bEncoding\s*\.\s*UTF8\s*\.\s*GetBytes\s*\(\s*\w+\s*\.\s*Design\s*\)", "no pasa los bytes del Design del sobre");
            Require("C06", @"\bEncoding\s*\.\s*UTF8\s*\.\s*GetString\s*\(", "el decodificador no vuelve los bytes texto");
            Require("C06", @"\bnew\s+RackProjectStore\s*\(\s*\)\s*\.\s*Deserialize\s*\(", "el decodificador no usa el almacen de proyecto");
            Require("C06", @"\.\s*CantileverLineDesign\b", "el decodificador no devuelve la linea Cantilever");

            // ---- C07 the package and its publication into the folder the package declares
            Require("C07", @"\bRamBuildPackageFactory\s*\.\s*CreateRamBuildPackage\s*\(", "no construye el paquete con la fabrica");
            Require(
                "C07",
                @"\bnew\s+StructuralPackagePublisher\s*\(\s*\)\s*\.\s*Write\s*\(\s*[\w.]*\bPackageFolder\s*,\s*[\w.]*\bFiles\s*\)",
                "no publica con la carpeta y los archivos del paquete",
                1);
            Require("C07", @"\.\s*Write\s*\(", "hay mas de una escritura de paquete", 1);

            // ---- C08 what the command reports
            Require("C08", @"\.\s*Diagnostics\b", "no lista los diagnosticos");
            Require("C08", @"\.\s*Code\b", "no muestra el codigo de los diagnosticos");
            Require("C08", @"\.\s*AnalysisInputFingerprint\b", "no muestra la huella de entrada");
            Require("C08", @"\.\s*Files\b[^;]*\.\s*Name\b", "no lista los archivos escritos");

            // ---- C09 fail closed and visible
            var catches = Regex.Matches(code, @"\bcatch\b\s*(?<clause>\([^)]*\))?", Options);

            if (catches.Count == 0)
            {
                found.Add("C09: no captura ninguna excepcion");
            }

            foreach (Match match in catches)
            {
                if (!Regex.IsMatch(match.Groups["clause"].Value, @"^\(\s*System\s*\.\s*Exception\s+ex\s*\)$", Options))
                {
                    found.Add("C09: captura distinta de System.Exception: catch " + match.Groups["clause"].Value);
                }
            }

            if (!Regex.IsMatch(code, @"catch\s*\(\s*System\s*\.\s*Exception\s+ex\s*\)\s*\{[^{}]*StructuralExportOutcome\s*\.\s*Unavailable", Options))
            {
                found.Add("C09: la excepcion no se informa como Unavailable");
            }

            foreach (var outcome in new[] { "Unavailable", "Invalid", "Unsupported" })
            {
                Require("C09", @"\bStructuralExportOutcome\s*\.\s*" + outcome + @"\b", "no informa el resultado " + outcome);
            }

            if (Regex.Matches(code, @"\bOutcome\s*!=\s*StructuralExportOutcome\s*\.\s*Valid\b", Options).Count < 3)
            {
                found.Add("C09: no se comprueba el resultado Valid de la tabla, del modelo y del paquete");
            }

            Require("C09", @"\.\s*Succeeded\b", "no comprueba el exito de la publicacion");

            // ---- C10 the material profile comes ONLY from the table the reader returns; one version constant
            Require(
                "C10",
                @"\bCantileverStructuralExport\s*\.\s*Build\s*\([^;]*,\s*[\w.]+\s*\.\s*Table\s*,\s*\w+\s*\)\s*;",
                "el perfil de material no es la tabla leida con su lector");
            Forbid("C10", ProfileConstruction, code, "el comando construye o nombra un perfil");

            if (Regex.Matches(code, @"\bconst\s+string\s+\w*Version\w*\s*=", Options).Count != 1
                || !Regex.IsMatch(code, @"\bconst\s+string\s+AdapterVersion\s*=", Options))
            {
                found.Add("C10: la version del adaptador no es una constante unica llamada AdapterVersion");
            }

            Require(
                "C10",
                @"\bnew\s+RamBuildPackageOptions\s*\(\s*[\w.]+\s*,\s*AdapterVersion\s*,",
                "las opciones del paquete no usan la constante AdapterVersion");

            // ---- C11 no events, windows, palettes, menus, Workspace or ID30 code
            Forbid("C11", UserInterfaceTokens, scanned, "interfaz o evento prohibido");
            Forbid("C11", NamedAreaTokens, scanned, "area prohibida");

            // ---- C12 no resolver, loader, units reader, sibling redraw, creator or envelope builder
            Forbid("C12", ForbiddenSymbols, scanned, "simbolo prohibido");

            // ---- C13 no conversion literal, unit tag or design vocabulary
            Forbid("C13", ConversionAndDesignTokens, scanned, "literal de conversion, unidad o vocabulario de diseno");

            // ---- C14 no drawing write, process or file mutation
            Forbid("C14", SideEffectTokens, scanned, "efecto prohibido");

            // ---- C15 one publication, after the three checks of the table, the model and the package
            var publications = Regex.Matches(code, @"\bnew\s+StructuralPackagePublisher\s*\(\s*\)\s*\.\s*Write\s*\(", Options);
            var checks = Regex.Matches(code, @"\b(?<x>\w+)\s*\.\s*Outcome\s*!=\s*StructuralExportOutcome\s*\.\s*Valid\b", Options);

            if (publications.Count != 1)
            {
                found.Add("C15: no hay exactamente UNA llamada de publicacion");
            }

            if (checks.Cast<Match>().Select(check => check.Groups["x"].Value).Distinct(StringComparer.Ordinal).Count() < 3)
            {
                found.Add("C15: faltan las tres comprobaciones != Valid (tabla, modelo y paquete)");
            }
            else if (publications.Count == 1 && checks.Cast<Match>().Any(check => check.Index > publications[0].Index))
            {
                found.Add("C15: la publicacion aparece antes de una comprobacion != Valid");
            }

            // ---- C16 every non-Valid block reports the outcome and its diagnostics, then returns before the publication
            var blocks = Regex.Matches(
                code,
                @"\bif\s*\(\s*(?<x>\w+)\s*\.\s*Outcome\s*!=\s*StructuralExportOutcome\s*\.\s*Valid\b[^()]*\)\s*\{",
                Options);

            if (blocks.Count < 3)
            {
                found.Add("C16: faltan los tres bloques if (x.Outcome != Valid) { }");
            }

            foreach (Match block in blocks)
            {
                var subject = block.Groups["x"].Value;
                var open = block.Index + block.Length - 1;
                var close = MatchingBrace(code, open);

                if (close < 0)
                {
                    found.Add("C16: bloque de " + subject + " sin cierre");
                    continue;
                }

                var body = code.Substring(open + 1, close - open - 1).Trim();

                if (!Regex.IsMatch(
                        body,
                        @"\bShow\s*\([^;]*\b" + subject + @"\s*\.\s*Outcome\b[^;]*\b" + subject + @"\s*\.\s*Diagnostics\b",
                        Options))
                {
                    found.Add("C16: el bloque de " + subject + " no informa Outcome y Diagnostics con el ayudante visible");
                }

                if (!Regex.IsMatch(body, @"\breturn\s*;$", Options))
                {
                    found.Add("C16: el bloque de " + subject + " no termina con return;");
                }
            }

            // ---- C17 the visible helper writes, with editor.WriteMessage, a text that includes the outcome
            var helpers = Regex.Matches(code, @"\bvoid\s+Show\s*\((?<params>[^)]*)\)\s*\{", Options);

            if (helpers.Count != 1)
            {
                found.Add("C17: no hay exactamente UN ayudante visible Show");
            }
            else
            {
                var helper = helpers[0];
                var outcomeParameter = Regex.Match(helper.Groups["params"].Value, @"\bStructuralExportOutcome\s+(?<o>\w+)", Options);
                var helperOpen = helper.Index + helper.Length - 1;
                var helperClose = MatchingBrace(code, helperOpen);
                var helperBody = helperClose < 0 ? string.Empty : code.Substring(helperOpen + 1, helperClose - helperOpen - 1);
                var write = Regex.Match(helperBody, @"\beditor\s*\.\s*WriteMessage\s*\((?<arg>[^;]*)\)\s*;", Options);

                if (!outcomeParameter.Success || !write.Success)
                {
                    found.Add("C17: el ayudante visible no recibe el resultado o no escribe con editor.WriteMessage");
                }
                else
                {
                    var names = new List<string> { outcomeParameter.Groups["o"].Value };

                    foreach (Match local in Regex.Matches(helperBody, @"\b(?:var|[\w<>]+)\s+(?<v>\w+)\s*=\s*(?<init>[^;]*);", Options))
                    {
                        if (names.Any(name => Regex.IsMatch(local.Groups["init"].Value, @"\b" + name + @"\b", Options)))
                        {
                            names.Add(local.Groups["v"].Value);
                        }
                    }

                    if (!names.Any(name => Regex.IsMatch(write.Groups["arg"].Value, @"\b" + name + @"\b", Options)))
                    {
                        found.Add("C17: el texto que escribe el ayudante visible no incluye el resultado");
                    }
                }
            }

            // ---- C18 only the publisher writes files or folders
            Forbid(
                "C18",
                new Regex(
                    @"\bFile\s*\.\s*(?:Write\w*|Create\w*|AppendAll\w*|Copy|Move)\b|\bDirectory\s*\.\s*Create\w*|\bFileStream\b|\bStreamWriter\b",
                    Options),
                scanned,
                "escritura de archivos o carpetas fuera del publicador");

            return found;
        }
    }
}
