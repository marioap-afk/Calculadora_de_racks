using System;
using RackCad.Application.Systems.Shared;
using RackCad.Domain.Systems.Dynamic;

namespace RackCad.Application.Systems.Dynamic
{
    /// <summary>
    /// The remembered SOURCE of a Dinamico distribution (I-53, ID6; contract §3.3, §7.4, §7.6): an address and the signature of
    /// the sequence it was taken on. «Tomar como origen» remembers this, never a configuration.
    /// <para>
    /// There is no hidden clipboard. The configuration is captured when the batch is PREPARED, from the value the address
    /// designates at that moment, so an edit of the source made afterwards is what gets applied. The signature keeps the
    /// address honest: ids are positional and a rebuild hands them to new modules, so a source taken on another sequence or
    /// generation is refused as stale and never re-aimed at the module that inherited its id.
    /// </para>
    /// <para>
    /// An immutable runtime value: not persisted, not part of the design and not kept between sessions.
    /// </para>
    /// </summary>
    public sealed class DynamicHeaderSource
    {
        private DynamicHeaderSource(DynamicHeaderAddress address, HeaderBatchSignature signature)
        {
            Address = address;
            Signature = signature;
        }

        /// <summary>
        /// The address of <paramref name="moduleId"/>, signed with the sequence of <paramref name="system"/> at
        /// <paramref name="generation"/>. Whether it designates a usable custom cabecera is decided when the batch is prepared.
        /// </summary>
        public static DynamicHeaderSource Of(DynamicRackSystem system, long generation, string moduleId)
        {
            if (system == null)
            {
                throw new ArgumentNullException(nameof(system));
            }

            return new DynamicHeaderSource(new DynamicHeaderAddress(moduleId), DynamicHeaderBatch.SequenceSignature(system, generation));
        }

        public DynamicHeaderAddress Address { get; }

        /// <summary>The signature of the sequence the address was taken on (<see cref="DynamicHeaderBatch.SequenceSignature"/>).</summary>
        public HeaderBatchSignature Signature { get; }
    }
}
