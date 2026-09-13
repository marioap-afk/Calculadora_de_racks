using RackCad.Domain.Systems.Selective;

namespace RackCad.Application.Systems.Selective
{
    /// <summary>
    /// The resolution a Selectivo cabecera batch reads (I-53, RR-01; contract §3.8, §3.9, §3.11): the resolved system the
    /// editor holds, its generation, and whether a recompute is still pending or deferred.
    /// <para>
    /// Application cannot see the editor's recompute gate, so the caller states it. PREPARE refuses a resolution that is not
    /// current — no system, or a recompute pending or deferred that could make it describe an older state — and the gesture
    /// ends before PREPARE, with no plan and no outcome. The generation goes into the batch signature, so a system rebuilt
    /// between PREPARE and MUTATE is detected even when its topology happens to coincide.
    /// </para>
    /// <para>
    /// For MUTATE the caller takes the resolution BEFORE it opens the batch's own deferred scope (RR-01 rule 4): inside that
    /// scope a recompute is deferred by design, so a resolution read there could never be current.
    /// </para>
    /// </summary>
    public sealed class SelectiveHeaderResolution
    {
        private SelectiveHeaderResolution(SelectiveRackSystem system, long generation, bool recomputePendingOrDeferred)
        {
            System = system;
            Generation = generation;
            RecomputePendingOrDeferred = recomputePendingOrDeferred;
        }

        /// <param name="system">The resolved system in force; null when there is none (the last build failed).</param>
        /// <param name="generation">Identity of that resolution: it must change every time the system is rebuilt.</param>
        /// <param name="recomputePendingOrDeferred">True while a recompute is pending or a deferred scope is open.</param>
        public static SelectiveHeaderResolution Of(SelectiveRackSystem system, long generation, bool recomputePendingOrDeferred)
            => new SelectiveHeaderResolution(system, generation, recomputePendingOrDeferred);

        public SelectiveRackSystem System { get; }

        public long Generation { get; }

        public bool RecomputePendingOrDeferred { get; }

        /// <summary>A resolved system exists and no recompute is pending or deferred: PREPARE may read it.</summary>
        public bool IsCurrent => System != null && !RecomputePendingOrDeferred;
    }
}
