using System;
using RackCad.Domain.Systems;

namespace RackCad.Application.Systems
{
    /// <summary>
    /// The Application-layer facts about one <see cref="RackSystemKind"/> that the persistence store, the design library
    /// and validation will consult through a single registry instead of the scattered per-kind switches and the parallel
    /// RackDesignKind enum (initiative I-08). This F2 shape carries only the canonical kind identity and the exact visible
    /// library label; later phases EXTEND this sealed type with the persistence payload selector, the "usable" predicate
    /// and the library name accessor as each consumer migrates. It is pure Application data: no AutoCAD, no WPF, and no
    /// drawing / BOM / editor / Xrecord concerns. It is NOT a Plugin handler.
    /// </summary>
    public sealed class SystemDescriptor
    {
        public SystemDescriptor(RackSystemKind kind, string libraryLabel)
        {
            if (string.IsNullOrWhiteSpace(libraryLabel))
            {
                throw new ArgumentException("La etiqueta de biblioteca no puede estar vacia.", nameof(libraryLabel));
            }

            Kind = kind;
            LibraryLabel = libraryLabel;
        }

        /// <summary>The canonical system kind this descriptor represents.</summary>
        public RackSystemKind Kind { get; }

        /// <summary>
        /// The exact, user-visible label shown for this kind in the design library's "Tipo" column. It must reproduce the
        /// current strings verbatim (frozen by the F1 characterization); do not fold in the validation error nouns or the
        /// Plugin label set — those are distinct and belong to later phases.
        /// </summary>
        public string LibraryLabel { get; }
    }
}
