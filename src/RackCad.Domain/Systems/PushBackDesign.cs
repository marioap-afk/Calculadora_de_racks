using System.Collections.Generic;

namespace RackCad.Domain.Systems
{
    /// <summary>
    /// Editable inputs of a Push Back system. Push Back reuses the dynamic (pallet-flow) STRUCTURE — headers,
    /// separators, derived posts, infinite-adjust intermediate beams, fronts with different fondo counts and
    /// <c>DepthStartPosition</c>, fronts of different length sharing the same base structure — so the structural
    /// intent is COMPOSED as a <see cref="DynamicRackDesign"/> rather than restated. Push Back then adds its own
    /// LIFO behaviour: the entrance and exit share the LOW end, the HIGH (rear) end carries its OWN beam
    /// (<c>LARGUERO_ESCALON_TROQUEL_REDONDO</c>) whose PERALTE is editable PER FRONT AND LEVEL, and a rear
    /// pallet-stop tope guards the back. No calculated coordinates live here; Application resolves the model.
    /// </summary>
    public sealed class PushBackDesign
    {
        /// <summary>
        /// The shared structural intent, in the SAME vocabulary as the dynamic system (so its resolver, depth layout
        /// and header/separator/derived-post rules are reused, not duplicated). Never null. Its <c>SafetySelections</c>
        /// carry the entrance-side safety families that Push Back allows (every family EXCEPT entrance guides, which the
        /// resolver strips).
        /// </summary>
        public DynamicRackDesign Structure { get; set; } = new DynamicRackDesign();

        /// <summary>
        /// Per-front Push Back configuration, aligned BY INDEX with <see cref="DynamicRackDesign.Fronts"/> on
        /// <see cref="Structure"/>. A front not listed (or a level without a value) falls back to
        /// <see cref="LegacyHighEndBeamPeralte"/> and then to the explicit 3.5 default.
        /// </summary>
        public IList<PushBackFrontConfig> Fronts { get; } = new List<PushBackFrontConfig>();

        /// <summary>
        /// LEGACY rack-wide high-end beam PERALTE fallback. New designs store the peralte PER FRONT AND LEVEL in
        /// <see cref="Fronts"/>; this scalar is kept ONLY as a fallback for documents written before that refinement (and
        /// for a quick uniform default). &lt;= 0 resolves to <see cref="PushBackDefaults.HighEndBeamDefaultPeralte"/> (3.5).
        /// </summary>
        public double LegacyHighEndBeamPeralte { get; set; } = PushBackDefaults.HighEndBeamDefaultPeralte;

        /// <summary>Rear pallet-stop configuration: active by default, deactivable per cell (persists deactivations only).</summary>
        public PushBackRearTopeConfig RearTope { get; set; } = new PushBackRearTopeConfig();

        /// <summary>Convenience accessor for the composed pallet spec (the structural intent owns it).</summary>
        public PalletSpecification Pallet => Structure?.Pallet;

        /// <summary>The per-front config for <paramref name="frontIndex"/>, or null if none is stored.</summary>
        public PushBackFrontConfig FrontConfig(int frontIndex)
            => frontIndex >= 0 && frontIndex < Fronts.Count ? Fronts[frontIndex] : null;
    }

    /// <summary>
    /// Push-Back-specific editable intent for ONE front: the high-end (rear) beam PERALTE by load level (level 1 first).
    /// A missing/null/invalid entry falls back to the design's legacy rack-wide value and then to the explicit 3.5
    /// default. Aligned by index with the matching <see cref="DynamicRackFrontDesign"/> and its levels; it never adds a
    /// field to the dynamic types.
    /// </summary>
    public sealed class PushBackFrontConfig
    {
        /// <summary>High-end (rear) beam PERALTE by load level (index 0 = level 1). Null = inherit the fallback.</summary>
        public IList<double?> HighEndBeamPeraltes { get; } = new List<double?>();

        /// <summary>The stored peralte for <paramref name="level"/> (0-based), or null to inherit the fallback.</summary>
        public double? PeralteAt(int level)
            => level >= 0 && level < HighEndBeamPeraltes.Count ? HighEndBeamPeraltes[level] : null;

        public PushBackFrontConfig DeepCopy()
        {
            var copy = new PushBackFrontConfig();
            foreach (var peralte in HighEndBeamPeraltes)
            {
                copy.HighEndBeamPeraltes.Add(peralte);
            }

            return copy;
        }
    }
}
