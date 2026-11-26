#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using RackCad.Application.Persistence;
using RackCad.Application.Settings;
using RackCad.Application.Workspace;

namespace RackCad.Application.VentanaRackCad
{
    /// <summary>
    /// The window asks AutoCAD to open ONE design of the library in its classic editor (I-64 A-14, fourth batch, part «ut», UT-03: the
    /// systems that are not edited in the window yet). The request has its number, the session the window showed and the file chosen.
    /// </summary>
    public sealed record RackCadLibraryDesignRequest(long RequestId, SessionId Session, string Path);

    /// <summary>
    /// The door of the RackCad window for its utilities (part «ut»): a command of the list «Órdenes y atajos» (UT-10) and a design of the
    /// library in its classic editor (UT-03). The host hands the command to AutoCAD only when there is an active document and no command
    /// is in progress, and gives the focus back to the drawing, like <see cref="IRackCadClassicCommandPort"/>; it reads and writes nothing
    /// of the drawing by itself. A port of the host that does not implement it leaves both gestures out of the window.
    /// </summary>
    public interface IRackCadUtilityCommandPort
    {
        /// <summary>Runs a command of the help list: only one of <see cref="RackCadHelpCommands.Runnable"/>.</summary>
        RackCadClassicResult RunHelpCommand(string command);

        /// <summary>Keeps the request and hands <see cref="RackCadLibraryDoorRules.Command"/> to AutoCAD, which opens the design in its editor.</summary>
        RackCadClassicResult OpenLibraryDesign(RackCadLibraryDesignRequest request);
    }

    /// <summary>
    /// The commands that «Ejecutar» of the page «Órdenes y atajos» may hand to AutoCAD (UT-10): the commands of the help list except RACKCAD,
    /// which is the window itself. The doors of the window (RACKNUEVO, RACKBIBLIOTECA, RACKBIBLIOTECAABRIR, RACKPANEL and the RACK…ABRIR /
    /// APLICAR / INSERTAR / NUEVO of the editors) are not here: the window uses them. The help list of the UI is checked against this one by
    /// a test, so the two never drift.
    /// </summary>
    public static class RackCadHelpCommands
    {
        public static IReadOnlyList<string> Runnable { get; } = new[]
        {
            "RACKMENU", "RACKSELECTIVO", "RACKCABECERA", "QUICKCABECERA", "RACKSISTEMADINAMICO", "QUICKCAMA", "RACKCANTILEVER",
            "RACKPUSHBACK", "RACKSECCION",
            "RACKEDITAR", "RACKDUPLICAR", "RACKPROPIEDADES", "RACKPROYECTAR", "RACKVARIABLES",
            "RACKLAYOUT", "RACKRELLENAR",
            "RACKLISTA", "RACKBOMTOTAL",
            "RACKAYUDA",
        };

        /// <summary>The text the host sends for a command of the list (the command and a space, which ends the word); null for any other text.</summary>
        public static string? TextOf(string? command)
        {
            var name = command?.Trim();
            if (string.IsNullOrEmpty(name)) return null;
            return Runnable.Contains(name, StringComparer.Ordinal) ? name + " " : null;
        }
    }

    /// <summary>The rules of the door that opens a design of the library in its classic editor (UT-03), pure so they have tests in Core.</summary>
    public static class RackCadLibraryDoorRules
    {
        /// <summary>The command of the door: it takes the request the window kept and asks nothing on the command line.</summary>
        public const string Command = "RACKBIBLIOTECAABRIR";

        /// <summary>Why the window cannot ask for this design; null when it can (a number, and a full path of a design file).</summary>
        public static string? RefusalOf(RackCadLibraryDesignRequest? request)
        {
            if (request == null || request.RequestId <= 0) return "La petición no es válida.";
            if (string.IsNullOrWhiteSpace(request.Path) || !System.IO.Path.IsPathRooted(request.Path)
                || !request.Path.EndsWith(RackProjectStore.FileExtension, StringComparison.OrdinalIgnoreCase))
                return "El diseño no es un archivo de la biblioteca (" + RackProjectStore.FileExtension + ").";
            return null;
        }

        /// <summary>
        /// What the command says when it does not open anything: typed by hand with no request of the window, a request kept for another
        /// drawing (the request is for the drawing that was active when it was made), or a file that no longer exists. Null: it opens.
        /// </summary>
        public static string? RefusalWhenRun(bool hasRequest, bool sameDrawing, bool fileExists)
        {
            if (!hasRequest) return Command + " la usa la ventana RackCad para abrir un diseño de su biblioteca; no hay ninguno pedido. Usa «Biblioteca de diseños» de la ventana, o RACKBIBLIOTECA.";
            if (!sameDrawing) return "El diseño se pidió para otro dibujo: no se abrió nada. Vuelve a pedirlo desde la ventana.";
            if (!fileExists) return "El archivo del diseño ya no está en la carpeta: no se abrió nada. Vuelve a leer la carpeta en la ventana.";
            return null;
        }
    }

    /// <summary>The name of the file of a design saved from the window (UT-04), with the rule of the classic save (<c>PromptSaveToLibrary</c>).</summary>
    public static class RackCadLibraryFileName
    {
        /// <summary>
        /// The base of the file (without its extension) of a design saved with that name: the name trimmed, every character that Windows
        /// does not admit in a file name as «_». Null for an empty name: a design without a name is not saved.
        /// </summary>
        public static string? BaseOf(string? name)
        {
            if (string.IsNullOrWhiteSpace(name)) return null;
            return string.Join("_", name.Trim().Split(System.IO.Path.GetInvalidFileNameChars()));
        }

        /// <summary>The file name (with the extension) of a design saved with that name; null for an empty name.</summary>
        public static string? FileOf(string? name)
        {
            var baseName = BaseOf(name);
            return baseName == null ? null : baseName + RackProjectStore.FileExtension;
        }

        /// <summary>True when the file name changed characters of the name (the page says so).</summary>
        public static bool IsCleaned(string? name)
        {
            var baseName = BaseOf(name);
            return baseName != null && !string.Equals(baseName, name!.Trim(), StringComparison.Ordinal);
        }

        /// <summary>
        /// «Cambiar el nombre» of a name whose file exists: the first «name (n)», from 2, whose file does not exist in that folder.
        /// </summary>
        public static string NextFreeName(string folder, string name, Func<string, bool>? exists = null)
        {
            exists ??= File.Exists;
            var trimmed = (name ?? string.Empty).Trim();
            for (var n = 2; n < 1000; n++)
            {
                var candidate = trimmed + " (" + n + ")";
                var file = FileOf(candidate);
                if (file != null && !exists(System.IO.Path.Combine(folder ?? string.Empty, file))) return candidate;
            }

            return trimmed + " (" + DateTime.Now.ToString("yyyyMMddHHmmss", System.Globalization.CultureInfo.InvariantCulture) + ")";
        }
    }

    /// <summary>«Abrir RackCad al iniciar AutoCAD» (UT-01): the one rule the plugin reads when it loads.</summary>
    public static class RackCadStartupRule
    {
        /// <summary>True when the window opens with the first drawing of the session: only when the user turned it on (off by default).</summary>
        public static bool OpensWindow(UserSettings? settings) => settings != null && settings.OpenRackCadOnStartup;
    }
}
