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
        /// otherwise, and the new rack is then written with no metadata of a source, as before.
        /// </summary>
        public static RackProject Read(string json, RackSystemKind kind)
            => new RackProjectStore().TryDeserialize(json, out var project, out _) && project.Kind == kind ? project : null;
    }
}
