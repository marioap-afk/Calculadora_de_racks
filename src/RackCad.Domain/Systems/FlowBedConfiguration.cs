namespace RackCad.Domain.Systems
{
    /// <summary>
    /// Inputs for a single roller bed ("cama de rodamiento") in the lateral view: the bed type, the lane
    /// depth (rail length), the pallet depth (drives brake spacing) and the chosen roller. The assembly
    /// rule (rollers from the tope at the pitch, brakes every pallet depth) lives in the builder.
    /// </summary>
    public sealed class FlowBedConfiguration
    {
        public FlowBedType BedType { get; set; } = FlowBedType.Dynamic;

        /// <summary>Rail length = lane depth / fondo de cama (in).</summary>
        public double LaneDepth { get; set; }

        /// <summary>Pallet depth (in); a dynamic bed places a brake about every pallet depth + 1".</summary>
        public double PalletDepth { get; set; }

        /// <summary>Catalog id of the roller to use (its diameter drives the minimum pitch).</summary>
        public string RollerId { get; set; } = FlowBedDefaults.RollerId;

        /// <summary>Custom roller pitch (in); null/&lt;=0 = the minimum allowed by the roller diameter.</summary>
        public double? RollerPitchOverride { get; set; }
    }
}
