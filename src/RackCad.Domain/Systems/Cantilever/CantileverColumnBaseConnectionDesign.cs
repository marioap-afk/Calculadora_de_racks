namespace RackCad.Domain.Systems.Cantilever
{
    /// <summary>
    /// Which approved column–base design a <see cref="CantileverColumnBaseDesign"/> is an instance of.
    ///
    /// A variant is not decoration: it fixes how each profile is TURNED, and therefore which face carries
    /// the holes. Two W sections bolted flange-to-flange and the same two bolted web-to-web are different
    /// products, not the same one drawn differently.
    ///
    /// Only one value exists in I-37A because only one design is approved. The enum exists anyway so that
    /// adding the second is a value plus a policy registration, not a reinterpretation of what the first
    /// meant.
    /// </summary>
    public enum CantileverColumnBaseVariantKind
    {
        /// <summary>
        /// The approved design: a W column whose section DEPTH runs along the base direction — so its
        /// flanges face across the run and their outer faces are the ones that receive the base — and a W
        /// base laid with its section depth VERTICAL.
        /// </summary>
        WFlangeConnected = 0
    }

    /// <summary>
    /// Editable intent of the CONNECTION between a base and a column: the variant it follows and the punch
    /// parameters both sides share.
    ///
    /// This type is the reason the sub-assembly has one authority and not two. The punch parameters are
    /// neither the column's nor the base's — they are the connection's — so neither piece can drift from
    /// the other by editing its own copy (ADR-0024, D3).
    /// </summary>
    public sealed class CantileverColumnBaseConnectionDesign
    {
        /// <summary>The approved design this sub-assembly follows.</summary>
        public CantileverColumnBaseVariantKind Variant { get; set; } = CantileverColumnBaseVariantKind.WFlangeConnected;

        /// <summary>The punch parameters shared by the rear plate, the column face and the column bottom plate.</summary>
        public CantileverPunchParameters Punches { get; set; } = new CantileverPunchParameters();

        public CantileverColumnBaseConnectionDesign DeepCopy() =>
            new CantileverColumnBaseConnectionDesign
            {
                Variant = Variant,
                Punches = Punches?.DeepCopy() ?? new CantileverPunchParameters()
            };
    }
}
