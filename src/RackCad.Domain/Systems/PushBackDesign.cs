namespace RackCad.Domain.Systems
{
    /// <summary>
    /// Editable inputs of a Push Back system. Push Back reuses the dynamic (pallet-flow) STRUCTURE — headers,
    /// separators, derived posts, infinite-adjust intermediate beams, fronts with different fondo counts and
    /// <c>DepthStartPosition</c>, fronts of different length sharing the same base structure — so the structural
    /// intent is COMPOSED as a <see cref="DynamicRackDesign"/> rather than restated. Push Back then adds its own
    /// LIFO behaviour: the entrance and exit share the LOW end, the HIGH (rear) end carries its own beam, and a rear
    /// pallet-stop tope guards the back. No calculated coordinates live here; Application resolves the model.
    /// </summary>
    public sealed class PushBackDesign
    {
        /// <summary>
        /// The shared structural intent, in the SAME vocabulary as the dynamic system (so its resolver, depth layout
        /// and header/separator/derived-post rules are reused, not duplicated). Never null. Its <c>SafetySelections</c>
        /// carry the entrance-side safety families that Push Back allows (every family EXCEPT entrance guides).
        /// </summary>
        public DynamicRackDesign Structure { get; set; } = new DynamicRackDesign();

        /// <summary>
        /// High-end (rear) load-beam PERALTE (in). &lt;= 0 resolves to <see cref="PushBackDefaults.HighEndBeamDefaultPeralte"/>
        /// (3.5) — an explicit rule, NOT "the first catalog peralte".
        /// </summary>
        public double HighEndBeamPeralte { get; set; } = PushBackDefaults.HighEndBeamDefaultPeralte;

        /// <summary>Rear pallet-stop configuration: active by default, deactivable per cell (persists deactivations only).</summary>
        public PushBackRearTopeConfig RearTope { get; set; } = new PushBackRearTopeConfig();

        /// <summary>Convenience accessor for the composed pallet spec (the structural intent owns it).</summary>
        public PalletSpecification Pallet => Structure?.Pallet;
    }
}
