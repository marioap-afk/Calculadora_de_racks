using System;

namespace RackCad.Application.Systems.Dynamic
{
    /// <summary>
    /// The address of a cabecera of the Dinamico in a header reuse or batch distribution (I-53, ID6 + ID7; contract §7.1):
    /// its <see cref="ModuleId"/>, and nothing else.
    /// <para>
    /// A module of the Dinamico is ONE entry of the rack's longitudinal sequence, shared by every front and every post
    /// (Owner, I-35), so the destination is the module and never <c>(PostIndex, ModuleId)</c>, which is Push Back's line
    /// address (I-40) and is not reused here. Module ids are positional and a rebuild hands them to new modules, so an address
    /// never travels between gestures on its own: a request carries it with the signature of the sequence it was taken on
    /// (<see cref="DynamicHeaderSource"/>, <see cref="DynamicModuleTargets"/>).
    /// </para>
    /// <para>
    /// Equality is ordinal on the id. There is no ordering here: the order of a batch is the modules' <c>Index</c>.
    /// </para>
    /// </summary>
    public readonly struct DynamicHeaderAddress : IEquatable<DynamicHeaderAddress>
    {
        private readonly string moduleId;

        public DynamicHeaderAddress(string moduleId)
        {
            this.moduleId = moduleId;
        }

        /// <summary>The module id as given; never null.</summary>
        public string ModuleId => moduleId ?? string.Empty;

        public bool Equals(DynamicHeaderAddress other) => string.Equals(ModuleId, other.ModuleId, StringComparison.Ordinal);

        public override bool Equals(object obj) => obj is DynamicHeaderAddress other && Equals(other);

        public override int GetHashCode() => StringComparer.Ordinal.GetHashCode(ModuleId);

        public static bool operator ==(DynamicHeaderAddress left, DynamicHeaderAddress right) => left.Equals(right);

        public static bool operator !=(DynamicHeaderAddress left, DynamicHeaderAddress right) => !left.Equals(right);

        public override string ToString() => ModuleId;
    }
}
