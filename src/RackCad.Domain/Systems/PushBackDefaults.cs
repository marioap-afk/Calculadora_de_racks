namespace RackCad.Domain.Systems
{
    /// <summary>
    /// Domain constants for the Push Back rack system. Push Back reuses the dynamic (pallet-flow) STRUCTURE but
    /// works LIFO: the entrance and the exit share the LOW (left) end. The low end carries the same complete IN/OUT
    /// beam as the dynamic system (<see cref="DynamicRackDefaults.InOutBeamCatalogId"/>); the HIGH (right, rear) end
    /// carries <see cref="HighEndBeamCatalogId"/>. The slope, the 2" troquel snap and the header/separator/derived-post
    /// structure are identical to the dynamic system and are reused, not restated, by the resolver.
    /// </summary>
    public static class PushBackDefaults
    {
        /// <summary>High-end (rear) load beam id. The low end reuses <see cref="DynamicRackDefaults.InOutBeamCatalogId"/>.</summary>
        public const string HighEndBeamCatalogId = "LARGUERO_ESCALON_TROQUEL_REDONDO";

        /// <summary>View the high-end beam block and its bed-contact points use (same as the dynamic lateral beams).</summary>
        public const string HighEndBeamView = "LATERAL";

        /// <summary>
        /// Explicit default PERALTE (in) for the high-end beam. This MUST be a rule, NOT "the first catalog peralte":
        /// the catalog lists 3;3.5;4;4.5;5;5.5;6 (first = 3), but Push Back's high-end beam defaults to 3.5.
        /// </summary>
        public const double HighEndBeamDefaultPeralte = 3.5;

        /// <summary>
        /// Lateral bed-contact points on the high-end beam. Unlike the dynamic IN/OUT beam (which the bed mates via
        /// TROQUEL_CAMA), the high-end beam exposes INICIO_IZQUIERDO/INICIO_DERECHO — the bed line stays tangent to
        /// these, exactly like the dynamic intermediate beam (its bracket is not snapped to a post hole).
        /// </summary>
        public const string HighEndBeamLeftBedMatePoint = "INICIO_IZQUIERDO";
        public const string HighEndBeamRightBedMatePoint = "INICIO_DERECHO";

        /// <summary>Default rear-tope stick-out (SAQUE, in). Reuses the selective TOPE default so the rule lives in one place.</summary>
        public const double RearTopeSaque = SelectiveSafetyDefaults.TopeSaque;
    }
}
