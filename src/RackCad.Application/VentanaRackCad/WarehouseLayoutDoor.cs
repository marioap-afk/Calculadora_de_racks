using System;
using System.Collections.Generic;
using RackCad.Application.Geometry;
using RackCad.Application.Workspace;

namespace RackCad.Application.VentanaRackCad
{
    /// <summary>T5 al (I-64, prototype «almacen»): the two tabs of the page «Almacén»: «Rejilla» (RACKLAYOUT) and «Rellenar un área» (RACKRELLENAR).</summary>
    public enum WarehouseLayoutMode
    {
        Grid,
        Fill,
    }

    /// <summary>
    /// T5 al: what the user typed in the page «Almacén», as typed (an invalid value is kept so the page can say why). One set for the two
    /// tabs (question 11 of the prototype: the aisles and the back-to-back are the same in both classic windows). Remembered per user,
    /// never in the drawing (question 6); <see cref="Defaults"/> are the literals of the two classic windows (inventory §2).
    /// </summary>
    public sealed record WarehouseLayoutValues(
        string Rows,
        string Cols,
        string AisleRows,
        string AisleCols,
        bool BackToBack,
        string BackGap,
        bool Independent,
        string BuildingX,
        string BuildingY,
        string Layer,
        string WallClearance,
        string ColumnClearance,
        bool TryRotated)
    {
        /// <summary>The values of RackWarehouseLayoutWindow and RackWarehouseFillWindow when they open.</summary>
        public static WarehouseLayoutValues Defaults { get; } = new(
            "4", "1", "96", "96", false, "6", false, string.Empty, string.Empty, WarehouseLayoutRules.DefaultSiteLayer, "0", "4", true);
    }

    /// <summary>
    /// T5 al: the window asks AutoCAD to read, for the page «Almacén», the planta of ONE rack (the seed) and the site of a layer
    /// (RACKALMACENLEER, no undo marker). <paramref name="PreferredSeedHandle"/> is the planta the page read before, kept when nothing is
    /// selected in AutoCAD (question 3: the seed is the planta selected in AutoCAD).
    /// </summary>
    public sealed record WarehouseReadRequest(long RequestId, SessionId Session, string SeedRackId, string Layer, string PreferredSeedHandle = null);

    /// <summary>
    /// T5 al: the planta of the seed as AutoCAD measured it, plain values: the reference, its definition, the insertion point, the footprint
    /// (the bounding box of the reference, with its dimensions and texts, ADR-0035), the offset origin → corner, its own transform, the hash
    /// of its design and whether its kind is known. The same measure RACKLAYOUT and RACKRELLENAR make (<c>SeedOf</c> in the Plugin).
    /// </summary>
    public sealed record WarehouseSeedFacts(
        string RackId,
        string Name,
        string Kind,
        string ReferenceHandle,
        string DefinitionHandle,
        double OriginX,
        double OriginY,
        double FootprintX,
        double FootprintY,
        double OffsetX,
        double OffsetY,
        double Rotation,
        double ScaleX,
        double ScaleY,
        double ScaleZ,
        string DesignHash,
        bool HasEmbeddedDesign,
        bool KindKnown,
        bool WasSelected);

    /// <summary>T5 al: one obstacle of the layer of the site (a column, a block…), its bounding box without clearance.</summary>
    public sealed record WarehouseObstacleFact(double X, double Y, double Width, double Depth, string Label);

    /// <summary>
    /// T5 al: the site read from a layer, as RACKRELLENAR reads it: the largest CLOSED polyline is the outline (arcs sampled in 16 chords),
    /// the circles, polylines, ellipses and blocks are obstacles by their bounding box, and what is almost as big as the outline is ignored
    /// (<see cref="IgnoredLarge"/>). <see cref="Problem"/> says why there is no outline, with how to fix it; null when there is one.
    /// </summary>
    public sealed record WarehouseSiteFacts(
        string Layer,
        string Problem,
        IReadOnlyList<Point2D> Boundary,
        IReadOnlyList<WarehouseObstacleFact> Obstacles,
        int IgnoredLarge)
    {
        public bool HasOutline => Problem == null && Boundary != null && Boundary.Count >= 3;
    }

    /// <summary>
    /// T5 al: what AutoCAD read for the page «Almacén», for the request of that number: the seed (or why there is none, with the count of
    /// its plantas, question 7), the site of the layer asked and the layers of the drawing (question 8). <see cref="Failure"/> says why
    /// nothing could be read at all. <paramref name="CurrentLayer"/> is the current layer of the drawing (CLAYER), where AutoCAD places the
    /// new references (correction 2 of the review: on the layer of the site they would be read as obstacles next time).
    /// </summary>
    public sealed record WarehouseReading(
        long RequestId,
        string Failure,
        string SeedRackId,
        string SeedName,
        int PlantaCount,
        WarehouseSeedFacts Seed,
        string SeedProblem,
        WarehouseSiteFacts Site,
        IReadOnlyList<string> Layers,
        string CurrentLayer = null)
    {
        public bool IsRead => Failure == null;

        public static WarehouseReading Failed(long requestId, string seedRackId, string failure) => new(
            requestId, string.IsNullOrWhiteSpace(failure) ? "No se pudo leer el dibujo." : failure.Trim(), seedRackId, null, 0, null, null, null,
            Array.Empty<string>());
    }

    /// <summary>
    /// T5 al: the window asks AutoCAD to place the grid or the fill (RACKALMACENCOLOCAR, ONE command, one «U»). The values are a copy taken
    /// at the request; <paramref name="Fingerprint"/> is the one of the reading the page showed: the command reads the planta (and the site,
    /// to fill) again and places nothing if they changed (question 5). <paramref name="PlaceAnyway"/>: the user answered «Colocar de todos
    /// modos» to a grid that does not fit its building.
    /// </summary>
    public sealed record WarehousePlaceRequest(
        long RequestId,
        SessionId Session,
        WarehouseLayoutMode Mode,
        string SeedRackId,
        string SeedHandle,
        string Layer,
        string Fingerprint,
        WarehouseLayoutValues Values,
        bool PlaceAnyway);

    /// <summary>
    /// T5 al: what AutoCAD answered to «Colocar»: whether it placed, what it said, how many racks, and the reading of the drawing made in the
    /// same command (after placing, or the new one when the drawing changed), so the page follows without a second command (F-2 of the
    /// smoke of the fourth batch: no read is asked inside the answer of a write).
    /// </summary>
    public sealed record WarehousePlaced(long RequestId, bool Placed, string Message, int Count, WarehouseReading Reading);

    /// <summary>
    /// T5 al: the door of the RackCad window to the warehouse layout (page «Almacén»): a read with no undo marker (RACKALMACENLEER) and a
    /// write (RACKALMACENCOLOCAR), each numbered, with «Dejar de esperar» (<see cref="IRackCadRackDoorPort.Abandon"/>) and the rules of the
    /// door to a rack: only without a command in progress, in the drawing of the request, the focus back to the drawing.
    /// </summary>
    public interface IRackCadWarehousePort : IRackCadRackDoorPort
    {
        RackCadClassicResult RequestWarehouseRead(WarehouseReadRequest request);

        RackCadClassicResult RequestWarehousePlace(WarehousePlaceRequest request);

        event Action<WarehouseReading> WarehouseRead;

        event Action<WarehousePlaced> WarehousePlaced;
    }
}
