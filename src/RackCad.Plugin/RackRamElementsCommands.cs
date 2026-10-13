using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Runtime;
using RackCad.Application.Persistence;
using RackCad.Application.StructuralAnalysis;
using RackCad.Application.StructuralAnalysis.Providers.RamElements;
using RackCad.Application.Systems.Cantilever;
using RackCad.Domain.Systems.Cantilever;
using AcApplication = Autodesk.AutoCAD.ApplicationServices.Application;

namespace RackCad.Plugin
{
    /// <summary>
    /// The line command that exports ONE station of a Cantilever rack to the build package of the analysis program (I-65 D-13).
    ///
    /// <para>
    /// It is a thin command: it picks the rack with the shared helper, asks for the station, the load file and a NEW folder,
    /// reads the envelope ONCE and hands its design bytes to the neutral entry of the station, builds the package with the
    /// provider and publishes it into the folder the package itself declares. It draws nothing, edits no block, opens no window,
    /// runs no analysis and reads no result.
    /// </para>
    /// <para>
    /// It fails closed and visibly: a missing or unreadable table, an unavailable catalogue, a kind that is not Cantilever, a
    /// result that is not Valid or a failed publication write the outcome (Unavailable, Invalid or Unsupported) and its
    /// diagnostics on the command line, and no folder is left half written. The material profile comes ONLY from the table that
    /// the provider reader returns: while its rows are proposed, the command answers Unsupported and writes nothing.
    /// </para>
    /// </summary>
    public sealed class RackRamElementsCommands
    {
        /// <summary>The ONE version of this adapter; it is part of the fingerprint of the analysis input.</summary>
        private const string AdapterVersion = "1.0.0";

        [CommandMethod("RACKRAMPAQUETE")]
        public void RackRamPaquete()
        {
            var document = AcApplication.DocumentManager.MdiActiveDocument;

            if (document == null)
            {
                return;
            }

            var editor = document.Editor;

            try
            {
                Export(document, editor);
            }
            catch (System.Exception ex)
            {
                Show(editor, StructuralExportOutcome.Unavailable, ex.Message, null, null, null);
            }
        }

        private static void Export(Document document, Editor editor)
        {
            if (!RackCommandSupport.PickRackBlock(document, "\nSelecciona el rack Cantilever: ", out var embed, out _))
            {
                return;
            }

            if (embed == null || string.IsNullOrEmpty(embed.Design) || string.IsNullOrWhiteSpace(embed.Id))
            {
                Show(editor, StructuralExportOutcome.Invalid, "El bloque elegido no es un rack con diseno e identificador.", null, null, null);
                return;
            }

            if (!string.Equals(embed.Kind, RackEmbedDocument.KindCantilever, StringComparison.OrdinalIgnoreCase))
            {
                Show(editor, StructuralExportOutcome.Unsupported, "Solo se exporta un rack Cantilever; el elegido es de tipo " + embed.Kind + ".", null, null, null);
                return;
            }

            var station = editor.GetInteger(new PromptIntegerOptions("\nIndice de estacion (desde 0): ") { AllowNegative = false });

            if (station.Status != PromptStatus.OK)
            {
                return;
            }

            var loadsPath = Ask(editor, "\nArchivo de cargas: ");

            if (loadsPath == null)
            {
                return;
            }

            var folder = Ask(editor, "\nCarpeta NUEVA del paquete: ");

            if (folder == null)
            {
                return;
            }

            var mapPath = Path.Combine(
                Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? string.Empty,
                "structural-analysis",
                "ram-elements",
                "section-material-map.json");

            if (!File.Exists(mapPath))
            {
                Show(editor, StructuralExportOutcome.Unavailable, "Falta la tabla de secciones y materiales junto al Plugin: " + mapPath, null, null, null);
                return;
            }

            var read = RamSectionMapReader.Read(File.ReadAllText(mapPath, new UTF8Encoding(false)));

            if (read.Outcome != StructuralExportOutcome.Valid || read.Table == null)
            {
                Show(editor, read.Outcome, "La tabla de secciones y materiales no es utilizable.", read.Diagnostics, null, null);
                return;
            }

            if (!StructuralSectionCatalogAccess.TryLoad(editor, out var catalog))
            {
                Show(editor, StructuralExportOutcome.Unavailable, "El catalogo de secciones estructurales no esta disponible.", null, null, null);
                return;
            }

            var loadBytes = File.ReadAllBytes(loadsPath);

            var model = CantileverStructuralExport.Build(
                embed.Id, embed.Kind, station.Value, Encoding.UTF8.GetBytes(embed.Design), DecodeDesign, loadBytes, read.Table, catalog);

            if (model.Outcome != StructuralExportOutcome.Valid)
            {
                Show(editor, model.Outcome, "El modelo estructural de la estacion no es valido; no se escribio nada.", model.Diagnostics, null, null);
                return;
            }

            var package = RamBuildPackageFactory.CreateRamBuildPackage(
                model, new RamBuildPackageOptions(read.Table, AdapterVersion, folder, loadBytes));

            if (package.Outcome != StructuralExportOutcome.Valid || package.Package == null)
            {
                Show(editor, package.Outcome, "El paquete no se pudo construir; no se escribio nada.", package.Diagnostics, null, null);
                return;
            }

            var written = new StructuralPackagePublisher().Write(package.Package.PackageFolder, package.Package.Files);

            if (!written.Succeeded)
            {
                Show(
                    editor,
                    StructuralExportOutcome.Unavailable,
                    "La publicacion fallo (" + written.Code + ", " + written.Subject + "): " + written.Detail,
                    package.Diagnostics,
                    package.Package.AnalysisInputFingerprint,
                    null);
                return;
            }

            Show(
                editor,
                StructuralExportOutcome.Valid,
                "Paquete publicado en " + package.Package.PackageFolder,
                package.Diagnostics,
                package.Package.AnalysisInputFingerprint,
                package.Package.Files.Select(file => file.Name));
        }

        /// <summary>The design is decoded from the very bytes of the envelope, with the project store.</summary>
        private static CantileverLineDesign DecodeDesign(byte[] bytes)
            => new RackProjectStore().Deserialize(Encoding.UTF8.GetString(bytes)).CantileverLineDesign;

        /// <summary>The text the user typed, or null when the prompt was cancelled.</summary>
        private static string Ask(Editor editor, string prompt)
        {
            var answer = editor.GetString(new PromptStringOptions(prompt) { AllowSpaces = true });

            return answer.Status == PromptStatus.OK ? answer.StringResult.Trim().Trim('"') : null;
        }

        private static void Show(
            Editor editor,
            StructuralExportOutcome outcome,
            string note,
            IEnumerable<StructuralDiagnostic> diagnostics,
            string fingerprint,
            IEnumerable<string> files)
        {
            var lines = new List<string> { "RackCad paquete estructural: " + outcome + ". " + note };

            foreach (var diagnostic in diagnostics ?? Enumerable.Empty<StructuralDiagnostic>())
            {
                lines.Add("  " + diagnostic.Severity + " " + diagnostic.Code + " (" + diagnostic.Subject + ")");
            }

            if (fingerprint != null)
            {
                lines.Add("  Huella de entrada: " + fingerprint);
            }

            foreach (var name in files ?? Enumerable.Empty<string>())
            {
                lines.Add("  Archivo escrito: " + name);
            }

            editor.WriteMessage("\n" + string.Join("\n", lines) + "\n");
        }
    }
}
