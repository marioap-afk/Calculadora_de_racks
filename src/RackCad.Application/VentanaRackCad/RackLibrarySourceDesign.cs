using RackCad.Application.Diagnostics;
using RackCad.Application.Persistence;
using RackCad.Domain.Systems.Shared;

namespace RackCad.Application.VentanaRackCad
{
    /// <summary>
    /// T6 lib (I-64, sixth batch; UT-S10 and CM-S13): the design of the library a new Dinámico, Push Back or Cantilever of the window was
    /// opened from, as it travels with the write of «Insertar en el dibujo…» to the command that inserts it, so the drawing keeps the JSON
    /// fields of that file this build does not know, as the classic editor opened from the library keeps them (its <c>SourceProjectToInsert</c>,
    /// I-11 matrix B5). It is the JSON of the project of the file as the store writes it again: the wrapper with its unknown fields and its
    /// schema version (never downgraded) and, where the store keeps them, the unknown fields and the version of its payload (Push Back and
    /// Cantilever). Nothing deeper: the inner design of a Dinámico keeps no unknown fields of its own (the declared limit of I-11). A cama
    /// carries its <see cref="FlowBedDocument"/> instead (<see cref="CamaDoorWrite.SourceDesignJson"/>), and a Selectivo carries nothing,
    /// as in the classic editor: its block holds the selective document, not the project of the file.
    /// </summary>
    public static class RackLibrarySourceDesign
    {
        /// <summary>The JSON of the project a draft was opened from; null when it was not opened from the library.</summary>
        public static string Of(RackProject source) => source == null ? null : new RackProjectStore().Serialize(source);

        /// <summary>
        /// The project the command inserts with, as the classic editor hands it to the dispatch of «Nuevo» (the <c>SourceProject</c> of the
        /// request): the one read back from <paramref name="json"/> when it is a project of <paramref name="kind"/> this build reads; null
        /// otherwise, and the new rack is then written with no metadata of a source, as before. No JSON is no design of the library, and says
        /// nothing. A JSON that is not used (it does not read back, a newer build wrote it, or it is a project of another system) leaves a
        /// warning in the log of I-03 (<see cref="RackLog"/>) and the rack is still inserted: the command runs in the middle of the insertion
        /// and writes nothing on the command line, as the warning of an empty catalog (review of 39addd8f, point 1).
        /// </summary>
        public static RackProject Read(string json, RackSystemKind kind)
        {
            if (string.IsNullOrWhiteSpace(json)) return null;
            if (new RackProjectStore().TryDeserialize(json, out var project, out var incompatibleMajor))
            {
                if (project.Kind == kind) return project;
                Warn(kind, "es un proyecto de otro sistema (" + project.Kind + ")");
                return null;
            }

            Warn(kind, incompatibleMajor ? "lo escribió una versión más nueva" : "no se pudo leer");
            return null;
        }

        /// <summary>The context of the warning in the log of I-03.</summary>
        public const string LogContext = "Biblioteca de diseños: inserción de un rack nuevo de la ventana";

        private static void Warn(RackSystemKind kind, string why)
            => RackLog.Warning(LogContext, "El diseño de la biblioteca del rack nuevo (" + kind + ") " + why
                + ": se inserta sin los campos de su archivo que esta versión no conoce (I-11).");
    }
}
