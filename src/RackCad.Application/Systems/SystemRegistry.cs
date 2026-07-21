using System;
using System.Collections.Generic;
using RackCad.Domain.Systems;

namespace RackCad.Application.Systems
{
    /// <summary>
    /// Single source of truth for the set of rack systems the Application knows about (initiative I-08). It is built once
    /// from an explicit, ordered set of <see cref="SystemDescriptor"/>: it rejects duplicate kinds, keeps a stable
    /// enumeration order, resolves a descriptor by <see cref="RackSystemKind"/>, and fails explicitly for an unregistered
    /// kind. The <see cref="Default"/> instance (defined in the companion partial file) lists the five kinds that exist
    /// today, in <see cref="RackSystemKind"/> declaration order, each carrying the persistence operations the store
    /// delegates to. These generic mechanics use no reflection, no assembly scanning and no AutoCAD/WPF.
    /// </summary>
    public sealed partial class SystemRegistry
    {
        private readonly IReadOnlyList<SystemDescriptor> descriptors;
        private readonly Dictionary<RackSystemKind, SystemDescriptor> byKind;

        public SystemRegistry(IEnumerable<SystemDescriptor> descriptors)
        {
            if (descriptors == null)
            {
                throw new ArgumentNullException(nameof(descriptors));
            }

            // Copy defensively so the registry cannot change after construction, even if the caller mutates the source.
            var ordered = new List<SystemDescriptor>();
            var map = new Dictionary<RackSystemKind, SystemDescriptor>();
            foreach (var descriptor in descriptors)
            {
                if (descriptor == null)
                {
                    throw new ArgumentException("La lista de descriptores no puede contener nulos.", nameof(descriptors));
                }

                if (map.ContainsKey(descriptor.Kind))
                {
                    throw new ArgumentException(
                        "Descriptor de sistema duplicado para el tipo '" + descriptor.Kind + "'.", nameof(descriptors));
                }

                map.Add(descriptor.Kind, descriptor);
                ordered.Add(descriptor);
            }

            this.descriptors = ordered.AsReadOnly(); // ReadOnlyCollection: no cast-to-mutable path
            this.byKind = map;
        }

        /// <summary>The registered descriptors in their stable registration order.</summary>
        public IReadOnlyList<SystemDescriptor> Descriptors => descriptors;

        /// <summary>The descriptor for <paramref name="kind"/>; throws if that kind is not registered.</summary>
        public SystemDescriptor Get(RackSystemKind kind)
        {
            if (byKind.TryGetValue(kind, out var descriptor))
            {
                return descriptor;
            }

            throw new InvalidOperationException(
                "No hay un descriptor de sistema registrado para el tipo '" + kind + "'.");
        }

        /// <summary>Non-throwing lookup: true and the descriptor when <paramref name="kind"/> is registered.</summary>
        public bool TryGet(RackSystemKind kind, out SystemDescriptor descriptor)
            => byKind.TryGetValue(kind, out descriptor);
    }
}
