using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Xunit;
using static RackCad.Tests.ExpressionSemanticTestSupport;
using Cmd = RackCad.Tests.StructuralAnalysisPluginCommandGuard;

namespace RackCad.Tests
{
    /// <summary>
    /// I-65 F4-T5 — the line command of D-13 (<c>RackRamElementsCommands.cs</c>) seen by the Core suite. The Plugin is never
    /// compiled here (ADR-0003), so the command is fixed by its TEXT: <see cref="Cmd"/> is a pure function that proves itself
    /// on synthetic code (a conforming sample and one variant per rule) before it sweeps the real file, one rule at a time.
    /// The distribution of the table through the project of the Plugin is read as XML.
    /// </summary>
    public class StructuralAnalysisPluginCommandGuardTests
    {
        /// <summary>The smallest text that obeys every rule of the guard; the variants below change ONE thing of it.</summary>
        private const string Anchor = "var path = editor.GetString(text);";

        private static readonly string Sample = (
"""
using System;
using System.Linq;
using System.Reflection;
using System.Text;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Runtime;

@@NS@@ RackCad.Plugin
{
    /// <summary>A comment may name Insunits or CsvStructuralSectionCatalogProvider without violating anything.</summary>
    public sealed class RackRamElementsCommands
    {
        private const string AdapterVersion = "1.0.0";

        [CommandMethod("RACKRAMPAQUETE")]
        public void RackRamPaquete()
        {
            try
            {
                if (!RackCommandSupport.PickRackBlock(document, "\nRack: ", out var embed, out _))
                {
                    return;
                }

                if (!string.Equals(embed.Kind, RackEmbedDocument.KindCantilever, StringComparison.OrdinalIgnoreCase))
                {
                    Show(StructuralExportOutcome.Unsupported);
                    return;
                }

                var station = editor.GetInteger(options);
                var path = editor.GetString(text);
                var mapPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "structural-analysis", "ram-elements", "section-material-map.json");
                var read = RamSectionMapReader.Read(File.ReadAllText(mapPath));

                if (read.Outcome != StructuralExportOutcome.Valid)
                {
                    Show(StructuralExportOutcome.Invalid, read.Diagnostics);
                    return;
                }

                if (!StructuralSectionCatalogAccess.TryLoad(editor, out var catalog))
                {
                    return;
                }

                var model = CantileverStructuralExport.Build(embed.Id, embed.Kind, station.Value, Encoding.UTF8.GetBytes(embed.Design), DecodeDesign, loadBytes, read.Table, catalog);

                if (model.Outcome != StructuralExportOutcome.Valid)
                {
                    return;
                }

                var package = RamBuildPackageFactory.CreateRamBuildPackage(model, new RamBuildPackageOptions(read.Table, AdapterVersion, folder, loadBytes));

                if (package.Outcome != StructuralExportOutcome.Valid)
                {
                    return;
                }

                var written = new StructuralPackagePublisher().Write(package.Package.PackageFolder, package.Package.Files);

                if (!written.Succeeded)
                {
                    return;
                }

                Show(package.Diagnostics, package.Package.AnalysisInputFingerprint, package.Package.Files.Select(file => file.Name), written.Code);
            }
            catch (System.Exception ex)
            {
                Show(StructuralExportOutcome.Unavailable, ex.Message);
            }
        }

        private static CantileverLineDesign DecodeDesign(byte[] bytes)
            => new RackProjectStore().Deserialize(Encoding.UTF8.GetString(bytes)).CantileverLineDesign;
    }
}
""").Replace("\r\n", "\n").Replace("@@NS@@", "name" + "space");

        // ================================================================ the guard proves itself on synthetic code

        [Fact]
        public void LaMuestraConformeNoTieneNingunaViolacion()
        {
            Assert.Empty(Cmd.Violations(Sample));
        }

        [Fact]
        public void LaGuardaIgnoraLosComentariosQueNombranLoProhibido()
        {
            Assert.Contains("Insunits or CsvStructuralSectionCatalogProvider", Sample, StringComparison.Ordinal);
            Assert.Empty(Cmd.Violations(Sample));
        }

        public static TheoryData<string, string, string> Variants() => new TheoryData<string, string, string>
        {
            { "C01", "namespace RackCad.Plugin\n{", "namespace RackCad.Plugin.Sub\n{" },
            { "C01", "public sealed class RackRamElementsCommands", "internal sealed class RackRamElementsCommands" },
            { "C01", "        private static CantileverLineDesign DecodeDesign", "        public sealed class Extra { }\n        private static CantileverLineDesign DecodeDesign" },
            { "C02", "[CommandMethod(\"RACKRAMPAQUETE\")]", "[CommandMethod(\"RACKRAMPAQUETE\")]\n        [CommandMethod(\"RRP\")]" },
            { "C02", "[CommandMethod(\"RACKRAMPAQUETE\")]", "[CommandMethod(\"RACKRAMPAQUETE\", CommandFlags.Modal)]" },
            { "C02", "[CommandMethod(\"RACKRAMPAQUETE\")]", "[CommandMethod(\"Grupo\", \"RACKRAMPAQUETE\", CommandFlags.Modal)]" },
            { "C02", "[CommandMethod(\"RACKRAMPAQUETE\")]", "[CommandMethod( \"RACKRAMPAQUETE\")]" },
            { "C02", "[CommandMethod(\"RACKRAMPAQUETE\")]", "[CommandMethod(\"rackrampaquete\")]" },
            { "C02", "        public void RackRamPaquete()", "        // RACKRAMPAQUETE\n        public void RackRamPaquete()" },
            { "C02", "        public void RackRamPaquete()", "        // [CommandMethod(\"X\")]\n        public void RackRamPaquete()" },
            { "C03", "RackCommandSupport.PickRackBlock(", "RackCommandSupport.PickOther(" },
            { "C03", "RackEmbedDocument.KindCantilever", "\"cantilever\"" },
            { "C03", "StringComparison.OrdinalIgnoreCase", "StringComparison.Ordinal" },
            { "C03", ".GetInteger(", ".GetDouble(" },
            { "C03", ".GetString(", ".GetKeywords(" },
            { "C04", "StructuralSectionCatalogAccess.TryLoad(", "StructuralSectionCatalogAccess.Open(" },
            { "C05", "RamSectionMapReader.Read(", "RamSectionMapReader.Parse(" },
            { "C05", "Assembly.GetExecutingAssembly().Location", "AppContext.BaseDirectory" },
            { "C05", "\"section-material-map.json\"", "\"other.json\"" },
            { "C05", "\"ram-elements\"", "\"elements\"" },
            { "C06", "CantileverStructuralExport.Build(", "CantileverStructuralExport.Create(" },
            { "C06", "Encoding.UTF8.GetBytes(embed.Design)", "Encoding.UTF8.GetBytes(embed.Name)" },
            { "C06", "new RackProjectStore().Deserialize(", "new RackEmbedStore().Deserialize(" },
            { "C06", ".CantileverLineDesign;", ".Design;" },
            { "C07", "RamBuildPackageFactory.CreateRamBuildPackage(", "RamBuildPackageFactory.CreateOther(" },
            { "C07", "Write(package.Package.PackageFolder,", "Write(folder," },
            { "C07", ", package.Package.Files);", ", loadBytes);" },
            { "C08", "Diagnostics", "Notes" },
            { "C08", ".AnalysisInputFingerprint", ".Length" },
            { "C08", "file => file.Name", "file => file.Length" },
            { "C09", "catch (System.Exception ex)", "catch (Exception ex)" },
            { "C09", "Show(StructuralExportOutcome.Unavailable, ex.Message);", "Show(StructuralExportOutcome.Invalid, ex.Message);" },
            { "C09", "if (!written.Succeeded)", "if (written == null)" },
            { "C09", "if (model.Outcome != StructuralExportOutcome.Valid)", "if (model == null)" },
            { "C09", "        private static CantileverLineDesign DecodeDesign", "        private void Quiet() { try { } catch { } }\n        private static CantileverLineDesign DecodeDesign" },
            { "C10", "read.Table, catalog", "profile, catalog" },
            { "C10", "read.Table, catalog", "new StructuralMaterialProfile(), catalog" },
            { "C10", "        private const string AdapterVersion = \"1.0.0\";", "        private const string AdapterVersion = \"1.0.0\";\n        private const string OtherVersion = \"2\";" },
            { "C10", "read.Table, AdapterVersion,", "read.Table, \"2.0.0\"," },
        };

        [Theory]
        [MemberData(nameof(Variants))]
        public void LaGuardaDetectaUnaVarianteQueRompeUnaRegla(string rule, string original, string replacement)
        {
            Assert.Contains(original, Sample, StringComparison.Ordinal);

            var violations = Cmd.Violations(Sample.Replace(original, replacement));

            Assert.True(violations.Any(violation => violation.StartsWith(rule + ":", StringComparison.Ordinal)), "la guarda no detecta la regla " + rule + " con: " + replacement);
        }

        public static TheoryData<string, string> Insertions() => new TheoryData<string, string>
        {
            { "C11", "document.CommandEnded += Handler;" },
            { "C11", "editor.SetImpliedSelection(ids);" },
            { "C11", "var palette = new PaletteSet(\"x\");" },
            { "C11", "System.Windows.MessageBox.Show(\"x\");" },
            { "C11", "new RackProjectWindow().Show();" },
            { "C11", "RackMenuCommands.Open();" },
            { "C11", "RackWorkspaceHost.Open();" },
            { "C11", "var shell = new Ribbon();" },
            { "C12", "var r = new CantileverLineResolver();" },
            { "C12", "var p = new CsvStructuralSectionCatalogProvider(directory);" },
            { "C12", "var t = typeof(StructuralSectionCatalogException);" },
            { "C12", "var u = database.Insunits;" },
            { "C12", "var s = new SiblingRedrawTransaction();" },
            { "C12", "RackDefinitionCreator.CreateInTransaction(a, b);" },
            { "C12", "var e = new RackEmbedDocument();" },
            { "C12", "RackEmbedComposer.Compose(a);" },
            { "C13", "var k = 25.4;" },
            { "C13", "var k = 645.16;" },
            { "C13", "var k = 0.45359237;" },
            { "C13", "var k = 9.80665;" },
            { "C13", "var k = \"[in]\";" },
            { "C13", "var k = \"[mm]\";" },
            { "C13", "var k = \"[ft]\";" },
            { "C13", "var k = Utilization;" },
            { "C13", "var k = \"member_design\";" },
            { "C14", "using (var tr = database.TransactionManager.StartTransaction()) { }" },
            { "C14", "var l = document.LockDocument();" },
            { "C14", "File.WriteAllText(a, b);" },
            { "C14", "File.Delete(a);" },
            { "C14", "Directory.CreateDirectory(a);" },
            { "C14", "Process.Start(a);" },
            { "C14", "editor.Regen();" },
        };

        [Theory]
        [MemberData(nameof(Insertions))]
        public void LaGuardaDetectaUnaSentenciaProhibidaInsertadaEnLaMuestra(string rule, string statement)
        {
            Assert.Contains(Anchor, Sample, StringComparison.Ordinal);

            var violations = Cmd.Violations(Sample.Replace(Anchor, statement + "\n                " + Anchor));

            Assert.True(violations.Any(violation => violation.StartsWith(rule + ":", StringComparison.Ordinal)), "la guarda no detecta la regla " + rule + " con: " + statement);
        }

        [Fact]
        public void CadaReglaDeLaGuardaTieneAlMenosUnaVarianteQueLaDemuestra()
        {
            var proven = Variants().Select(row => (string)row[0]).Concat(Insertions().Select(row => (string)row[0])).Distinct().OrderBy(id => id, StringComparer.Ordinal);

            Assert.Equal(Cmd.RuleIds.OrderBy(id => id, StringComparer.Ordinal), proven);
        }

        // ================================================================ the real file

        private static string RealCommand()
        {
            var path = Path.Combine(RepoRoot().FullName, Cmd.CommandPath.Replace('/', Path.DirectorySeparatorChar));

            Assert.True(File.Exists(path), "No existe el archivo del comando: " + Cmd.CommandPath);

            return File.ReadAllText(path);
        }

        [Fact]
        public void ElArchivoDelComandoNoTieneNingunaViolacion()
        {
            Assert.Empty(Cmd.Violations(RealCommand()));
        }

        public static TheoryData<string> Rules()
        {
            var data = new TheoryData<string>();

            foreach (var rule in Cmd.RuleIds)
            {
                data.Add(rule);
            }

            return data;
        }

        [Theory]
        [MemberData(nameof(Rules))]
        public void ElArchivoDelComandoCumpleLaRegla(string rule)
        {
            var violations = Cmd.Violations(RealCommand()).Where(violation => violation.StartsWith(rule + ":", StringComparison.Ordinal)).ToList();

            Assert.True(violations.Count == 0, "Violaciones de " + rule + ":\n" + string.Join("\n", violations));
        }

        [Fact]
        public void ElArchivoDelComandoTieneUnSoloRegistroYElNombreSoloEnElAtributo()
        {
            var text = RealCommand();

            Assert.Equal(1, text.Split("[CommandMethod(").Length - 1);
            Assert.Equal(1, text.Split(Cmd.CommandName).Length - 1);
            Assert.Contains("[CommandMethod(\"" + Cmd.CommandName + "\")]", text, StringComparison.Ordinal);
        }

        [Fact]
        public void LaRutaDelComandoEsLaUnicaRutaDeLaListaDePermisoDeG1()
        {
            Assert.Equal(new[] { Cmd.CommandPath }, StructuralAnalysisGuards.G1ExactPathAllowList);
        }

        // ================================================================ the table travels with the Plugin build, not with the bundle

        private static XElement[] TableContentElements()
        {
            var path = Path.Combine(RepoRoot().FullName, Cmd.ProjectPath.Replace('/', Path.DirectorySeparatorChar));
            var document = XDocument.Load(path);

            return document.Descendants("Content")
                .Where(element => ((string)element.Attribute("Include") ?? string.Empty).Contains("section-material-map.json", StringComparison.Ordinal))
                .ToArray();
        }

        [Fact]
        public void ElProyectoDelPluginGanaUnSoloElementoContentParaLaTabla()
        {
            var elements = TableContentElements();

            Assert.Single(elements);
            Assert.Equal(
                "../../assets/structural-analysis/ram-elements/section-material-map.json",
                ((string)elements[0].Attribute("Include")).Replace('\\', '/'));
            Assert.Equal("structural-analysis/ram-elements/section-material-map.json", (string)elements[0].Attribute("Link"));
            Assert.Equal("Always", (string)elements[0].Element("CopyToOutputDirectory"));
        }

        [Fact]
        public void ElElementoContentDeLaTablaYLaTablaExisten()
        {
            Assert.Single(TableContentElements());
            Assert.True(
                File.Exists(Path.Combine(RepoRoot().FullName, "assets", "structural-analysis", "ram-elements", "section-material-map.json")),
                "La tabla del producto no esta en assets/.");
        }

        [Fact]
        public void LaTablaNoViajaEnElPaqueteDelAutoloader_NiElDestinoDelPaqueteSeToca()
        {
            var root = RepoRoot().FullName;
            var project = File.ReadAllText(Path.Combine(root, "src", "RackCad.Plugin", "RackCad.Plugin.csproj"));
            var target = project.Substring(project.IndexOf("<Target Name=\"AssembleAutoloaderBundle\"", StringComparison.Ordinal));

            Assert.DoesNotContain("structural-analysis", target, StringComparison.Ordinal);
            Assert.DoesNotContain("section-material-map", target, StringComparison.Ordinal);

            foreach (var file in Directory.GetFiles(Path.Combine(root, "deploy"), "*", SearchOption.AllDirectories)
                         .Where(path => new[] { ".ps1", ".xml", ".json", ".md", ".template" }.Contains(Path.GetExtension(path).ToLowerInvariant())))
            {
                Assert.DoesNotContain("section-material-map", File.ReadAllText(file), StringComparison.Ordinal);
            }
        }
    }
}
