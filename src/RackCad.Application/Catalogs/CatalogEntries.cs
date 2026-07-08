using System.Collections.Generic;
using System.Text.Json.Serialization;
using RackCad.Application.RackFrames;

namespace RackCad.Application.Catalogs
{
    /// <summary>
    /// Common fields shared by every catalog piece. Beyond the typed fields, the open
    /// <see cref="Properties"/> bag holds any future key/value without changing the model, so the
    /// catalog is extensible: new attributes can be added in JSON and round-trip untouched.
    /// </summary>
    public abstract class CatalogEntryBase
    {
        public string Id { get; set; }

        /// <summary>User-facing name shown in the UI (falls back to description, then id).</summary>
        public string DisplayName { get; set; }

        public string Description { get; set; }

        // ---- Commercial / quoting ----
        public string Material { get; set; }
        public string PartNumber { get; set; }
        public string Manufacturer { get; set; }
        public string Finish { get; set; }
        public double UnitCost { get; set; }
        public string Currency { get; set; }

        /// <summary>Unit the cost/weight refer to, e.g. "m", "ft", "pieza".</summary>
        public string CostUnit { get; set; }

        /// <summary>Open bag for arbitrary future properties (extensibility seam).</summary>
        public Dictionary<string, string> Properties { get; set; } = new Dictionary<string, string>();

        /// <summary>Best human label: display name, else description, else id.</summary>
        [JsonIgnore]
        public string Label =>
            !string.IsNullOrWhiteSpace(DisplayName) ? DisplayName
            : !string.IsNullOrWhiteSpace(Description) ? Description
            : Id;
    }

    /// <summary>
    /// Structural profile used by posts, horizontals and diagonals. Dimensions are in
    /// <see cref="Units"/> (defaults to inches).
    /// </summary>
    public sealed class ProfileCatalogEntry : CatalogEntryBase
    {
        public string Family { get; set; }
        public double Width { get; set; }
        public double Depth { get; set; }
        public double Thickness { get; set; }
        public string Units { get; set; }

        // ---- Engineering ----
        public string Gauge { get; set; }
        public double WeightPerMeter { get; set; }
    }

    /// <summary>
    /// A fixed component of a roller bed ("cama de rodamiento"): rail, roller, brake or stop. They are
    /// unique pieces that do not vary; <see cref="Role"/> says which one. The assembly rule (how many
    /// rollers, the pitch, brakes every N) lives in code, not here. <see cref="CapacityKg"/> is reserved
    /// for the future capacity-driven roller count.
    /// </summary>
    public sealed class FlowBedComponentCatalogEntry : CatalogEntryBase
    {
        /// <summary>RIEL, RODILLO, FRENO or TOPE.</summary>
        public string Role { get; set; }
        public double Diameter { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }

        /// <summary>Fixed length (in); empty for the rail, whose length is parametric (= lane depth).</summary>
        public double Length { get; set; }
        public string Units { get; set; }
        public string Gauge { get; set; }

        /// <summary>Load capacity (kg) of a roller; reserved for the future capacity-based count.</summary>
        public double CapacityKg { get; set; }
        public double WeightEach { get; set; }
    }

    /// <summary>
    /// A load beam ("larguero"/"viga") type = one dynamic block. Both LENGTH and PERALTE are the block's
    /// parameters (LONGITUD / PERALTE grips), not stored per row; <see cref="Peraltes"/> only DECLARES the
    /// values the PERALTE grip accepts. Carries a fixed end connector (<see cref="Mensula"/>, FK to
    /// <see cref="MensulaCatalogEntry"/>). Per-peralte weight/part will live in a companion table at BOM time.
    /// </summary>
    public sealed class BeamProfileCatalogEntry : CatalogEntryBase
    {
        public string Family { get; set; }

        /// <summary>Allowed values for the block's PERALTE parameter, e.g. "3;3.5;4;4.5;5".</summary>
        public string Peraltes { get; set; }
        public double Width { get; set; }
        public double Thickness { get; set; }
        public string Units { get; set; }
        public string Gauge { get; set; }

        /// <summary>Fixed end connector for this beam (FK to a ménsula id).</summary>
        public string Mensula { get; set; }

        /// <summary>Base weight per unit length (per-peralte weight comes later, at BOM time).</summary>
        public double WeightPerMeter { get; set; }
    }

    /// <summary>End connector ("ménsula") of a beam: a fixed part counted in the BOM (two per beam).</summary>
    public sealed class MensulaCatalogEntry : CatalogEntryBase
    {
        public string Type { get; set; }
        public double Height { get; set; }
        public string Units { get; set; }
        public string Gauge { get; set; }
        public double WeightEach { get; set; }
    }

    /// <summary>Base plate that anchors a post to the floor.</summary>
    public sealed class BasePlateCatalogEntry : CatalogEntryBase
    {
        public double Width { get; set; }
        public double Length { get; set; }
        public double Thickness { get; set; }
        public string Units { get; set; }
        public double WeightEach { get; set; }

        // ---- Standard peralte, derived from the mating post ----
        /// <summary>Base term of the plate PERALTE: <c>peralte = PeralteBase + PeraltePorPeraltePoste * postPeralte</c>.</summary>
        public double PeralteBase { get; set; }

        /// <summary>Slope: plate PERALTE gained per unit of post peralte (1 = "post + base"; 0 = a fixed peralte).</summary>
        public double PeraltePorPeraltePoste { get; set; }

        /// <summary>The standard plate PERALTE for a post of <paramref name="postPeralte"/> (the advanced editor may override this).</summary>
        public double StandardPeralte(double postPeralte) => PeralteBase + PeraltePorPeraltePoste * postPeralte;
    }

    /// <summary>
    /// Definition of a named connection point (punch/troquel/anchor): just what it IS (<c>Id</c>,
    /// <c>Role</c>). WHERE it sits on a piece is not here — a piece may carry several and the 2D
    /// position depends on the view, so positions live in <see cref="ConnectionLayoutEntry"/>.
    /// </summary>
    public sealed class ConnectionPointCatalogEntry : CatalogEntryBase
    {
        public string Role { get; set; }
    }

    /// <summary>
    /// Placement of a connection point ON a piece, IN a view (a row of the normalized layout table).
    /// Key = <see cref="PieceId"/> + <see cref="ConnectionPointId"/> + <see cref="View"/>; a piece can
    /// have many. The mate solver reads <see cref="LocalX"/>/<see cref="LocalY"/> from here.
    /// </summary>
    public sealed class ConnectionLayoutEntry : CatalogEntryBase
    {
        /// <summary>Piece that owns this connection point (FK to a plate/profile/etc.).</summary>
        public string PieceId { get; set; }

        /// <summary>Which connection point (FK to <see cref="ConnectionPointCatalogEntry.Id"/>).</summary>
        public string ConnectionPointId { get; set; }

        /// <summary>View this 2D position applies to (FK to <see cref="ViewCatalogEntry.Id"/>).</summary>
        public string View { get; set; }

        public double LocalX { get; set; }
        public double LocalY { get; set; }

        /// <summary>
        /// Slope: X moves this much per unit of the parameter named in <see cref="Param"/> (Y stays fixed).
        /// So <c>X = LocalX + LocalXPorParam * value(Param)</c>. 0 = a fixed point (the default for every
        /// existing/centered mate). Lets a point that slides when a block parameter changes (e.g. the post's
        /// larguero troquel moving with PERALTE) be captured as data instead of a per-value row.
        /// </summary>
        public double LocalXPorParam { get; set; }

        /// <summary>Name of the block parameter that drives the X slope (e.g. "PERALTE"); empty when fixed.</summary>
        public string Param { get; set; }
    }

    /// <summary>
    /// A drawing view a piece can be represented in (frontal, lateral, planta, ...). Just a lookup:
    /// <c>Id</c> is the code referenced by <see cref="BlockCatalogEntry.View"/>, <c>DisplayName</c> the
    /// label.
    /// </summary>
    public sealed class ViewCatalogEntry : CatalogEntryBase
    {
    }

    /// <summary>
    /// One AutoCAD block for a piece in a specific view (a row of the normalized blocks table, so a
    /// piece can have many: <see cref="PieceId"/> + <see cref="View"/> are the key). This is the ONLY
    /// place a block name lives — the piece catalogs describe what a part IS; this table describes how
    /// to draw it per view (block, layer, color, scale, rotation).
    /// </summary>
    public sealed class BlockCatalogEntry : CatalogEntryBase
    {
        /// <summary>Id of the piece this block represents (FK to a profile/plate/connection point).</summary>
        public string PieceId { get; set; }

        /// <summary>View code this block draws (FK to <see cref="ViewCatalogEntry.Id"/>).</summary>
        public string View { get; set; }

        /// <summary>AutoCAD block name to insert.</summary>
        public string BlockName { get; set; }

        public string Layer { get; set; }
        public int Color { get; set; }
        public double Scale { get; set; } = 1.0;
        public double Rotation { get; set; }
    }

    /// <summary>
    /// Aggregate of every catalog the configurator consumes. Lists are never
    /// null so callers can enumerate without guarding.
    /// </summary>
    public sealed class RackCatalog
    {
        /// <summary>Post profiles. Reinforcements are not a separate part — they are posts, drawn from here too.</summary>
        public IReadOnlyList<ProfileCatalogEntry> PostProfiles { get; set; } = new List<ProfileCatalogEntry>();

        /// <summary>
        /// Celosía/truss profiles. Horizontals and diagonals are not different parts — both are truss
        /// members drawn from this single catalog (a profile can be used either way).
        /// </summary>
        public IReadOnlyList<ProfileCatalogEntry> TrussProfiles { get; set; } = new List<ProfileCatalogEntry>();
        public IReadOnlyList<BasePlateCatalogEntry> BasePlates { get; set; } = new List<BasePlateCatalogEntry>();

        /// <summary>Roller-bed components (rail, roller, brake, stop) for flow / pushback beds.</summary>
        public IReadOnlyList<FlowBedComponentCatalogEntry> FlowBedProfiles { get; set; } = new List<FlowBedComponentCatalogEntry>();

        /// <summary>Load-beam ("larguero") SKUs and their end connectors ("ménsulas").</summary>
        public IReadOnlyList<BeamProfileCatalogEntry> BeamProfiles { get; set; } = new List<BeamProfileCatalogEntry>();
        public IReadOnlyList<MensulaCatalogEntry> Mensulas { get; set; } = new List<MensulaCatalogEntry>();
        public IReadOnlyList<ConnectionPointCatalogEntry> ConnectionPoints { get; set; } = new List<ConnectionPointCatalogEntry>();
        public IReadOnlyList<ConnectionLayoutEntry> ConnectionLayout { get; set; } = new List<ConnectionLayoutEntry>();
        public IReadOnlyList<ViewCatalogEntry> Views { get; set; } = new List<ViewCatalogEntry>();
        public IReadOnlyList<BlockCatalogEntry> Blocks { get; set; } = new List<BlockCatalogEntry>();
        public RackDefaults Defaults { get; set; } = new RackDefaults();
    }
}
