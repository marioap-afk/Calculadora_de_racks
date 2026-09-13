using RackCad.Domain.Systems.Dynamic;

namespace RackCad.Application.Systems.Dynamic
{
    /// <summary>
    /// The runtime state of I-53 in the Dinamico editor (contract §7.5, §7.6): the rebuild GENERATION, the remembered source
    /// address and the module targets. The editor owns one instance; nothing here is persisted or remembered between sessions.
    /// <para>
    /// The generation is what makes positional ids safe. Every rebuild (<see cref="DynamicRackRebuild"/>) advances it and drops
    /// the remembered source and an explicit target set. A recomposition without rebuild leaves all three as they are, because
    /// ids and kinds travel through the resolver's snapshot.
    /// </para>
    /// </summary>
    public sealed class DynamicHeaderBatchState
    {
        public long Generation { get; private set; }

        /// <summary>The address «Tomar como origen» remembered; null when there is none.</summary>
        public DynamicHeaderSource Source { get; private set; }

        public DynamicModuleTargets Targets { get; } = new DynamicModuleTargets();

        /// <summary>
        /// «Tomar como origen»: remember the ADDRESS of <paramref name="moduleId"/>, signed with the sequence of
        /// <paramref name="system"/> at the current generation. No configuration is captured.
        /// </summary>
        public DynamicHeaderSource RememberSource(DynamicRackSystem system, string moduleId)
        {
            Source = DynamicHeaderSource.Of(system, Generation, moduleId);
            return Source;
        }

        public void ForgetSource() => Source = null;

        /// <summary>A rebuild replaced the sequence: advance the generation and drop what named the previous one.</summary>
        internal void NoteRebuild(out bool sourceInvalidated, out bool explicitTargetsInvalidated)
        {
            Generation++;
            sourceInvalidated = Source != null;
            Source = null;
            explicitTargetsInvalidated = Targets.Invalidate();
        }
    }
}
