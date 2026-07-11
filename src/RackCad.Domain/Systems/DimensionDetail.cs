namespace RackCad.Domain.Systems
{
    /// <summary>
    /// How much automatic dimensioning a view draws (chosen from a combo in the editor and saved with the rack).
    /// Each higher level is a superset of the one below.
    /// </summary>
    public enum DimensionDetail
    {
        /// <summary>No dimensions.</summary>
        None = 0,

        /// <summary>Only the framing dimensions: overall height/width (frontal), overall depth, etc.</summary>
        Minimal = 1,

        /// <summary>Minimal + the internal breakdown (per-frente widths, level separations, per-fondo depths).</summary>
        Standard = 2,

        /// <summary>Standard + every individual measure (each level's elevation from the floor, every tramo/gap).</summary>
        Detailed = 3
    }
}
