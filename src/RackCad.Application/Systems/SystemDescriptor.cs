using System;
using RackCad.Application.Persistence;
using RackCad.Application.RackFrames;
using RackCad.Domain.Systems;

namespace RackCad.Application.Systems
{
    /// <summary>
    /// The Application-layer facts and operations about one <see cref="RackSystemKind"/> that the persistence store, the
    /// design library and validation consult through a single registry instead of scattered per-kind switches and the
    /// parallel design-kind enum (initiative I-08). Carries the canonical identity, the exact visible library label,
    /// the grammatical validation noun, and the strongly-typed operations <see cref="RackProjectStore"/> delegates to
    /// (write payload, build, is-usable). All operations work with <see cref="RackProject"/> and
    /// <see cref="RackProjectDocument"/>, which are Application types. No AutoCAD/WPF; not a Plugin handler; no drawing /
    /// BOM / editor / Xrecord concerns. A descriptor may be built label-only (identity + label) for callers that do not
    /// need persistence — its operation members then throw (see <see cref="SupportsPersistence"/>).
    /// </summary>
    public sealed class SystemDescriptor
    {
        private readonly string validationNoun;
        private readonly Func<RackProject, RackProjectDocument, bool> writePayload;
        private readonly Func<RackProjectDocument, BracingPanelMemberBuilder, RackProject> build;
        private readonly Func<RackProject, bool> isUsable;

        /// <summary>Identity + visible label only. Does not carry persistence operations (see <see cref="SupportsPersistence"/>).</summary>
        public SystemDescriptor(RackSystemKind kind, string libraryLabel)
        {
            if (string.IsNullOrWhiteSpace(libraryLabel))
            {
                throw new ArgumentException("La etiqueta de biblioteca no puede estar vacia.", nameof(libraryLabel));
            }

            Kind = kind;
            LibraryLabel = libraryLabel;
        }

        /// <summary>
        /// Identity + label + the strongly-typed Application operations the store delegates to: writing this kind's
        /// payload into a document (false when the project carries no such payload), building the project back from a
        /// document (rebuilding derived members via the supplied <see cref="BracingPanelMemberBuilder"/>), the "usable"
        /// predicate, and the grammatical validation noun used in degenerate errors.
        /// </summary>
        public SystemDescriptor(
            RackSystemKind kind,
            string libraryLabel,
            string validationNoun,
            Func<RackProject, RackProjectDocument, bool> writePayload,
            Func<RackProjectDocument, BracingPanelMemberBuilder, RackProject> build,
            Func<RackProject, bool> isUsable)
            : this(kind, libraryLabel)
        {
            if (string.IsNullOrWhiteSpace(validationNoun))
            {
                throw new ArgumentException("El sustantivo de validacion no puede estar vacio.", nameof(validationNoun));
            }

            this.validationNoun = validationNoun;
            this.writePayload = writePayload ?? throw new ArgumentNullException(nameof(writePayload));
            this.build = build ?? throw new ArgumentNullException(nameof(build));
            this.isUsable = isUsable ?? throw new ArgumentNullException(nameof(isUsable));
            SupportsPersistence = true;
        }

        /// <summary>The canonical system kind this descriptor represents.</summary>
        public RackSystemKind Kind { get; }

        /// <summary>
        /// The exact, user-visible label shown for this kind in the design library's "Tipo" column. It must reproduce the
        /// current strings verbatim (frozen by the F1 characterization); it is distinct from <see cref="ValidationNoun"/>
        /// and from the Plugin label set.
        /// </summary>
        public string LibraryLabel { get; }

        /// <summary>True when this descriptor carries the persistence operations (built with the full constructor).</summary>
        public bool SupportsPersistence { get; }

        /// <summary>
        /// The grammatical noun WITH article used in degenerate validation errors ("el sistema dinámico", "la cabecera"…),
        /// distinct from <see cref="LibraryLabel"/>.
        /// </summary>
        public string ValidationNoun
        {
            get
            {
                EnsurePersistence();
                return validationNoun;
            }
        }

        /// <summary>Writes this kind's payload into <paramref name="document"/>; returns false when the project has no such payload.</summary>
        public bool TryWritePayload(RackProject project, RackProjectDocument document)
        {
            EnsurePersistence();
            return writePayload(project, document);
        }

        /// <summary>Builds the project from <paramref name="document"/>, rebuilding derived members with <paramref name="memberBuilder"/>.</summary>
        public RackProject Build(RackProjectDocument document, BracingPanelMemberBuilder memberBuilder)
        {
            EnsurePersistence();
            return build(document, memberBuilder);
        }

        /// <summary>The current per-kind "usable" predicate applied to a built project.</summary>
        public bool IsUsable(RackProject project)
        {
            EnsurePersistence();
            return isUsable(project);
        }

        private void EnsurePersistence()
        {
            if (!SupportsPersistence)
            {
                throw new InvalidOperationException("El descriptor de '" + Kind + "' no define operaciones de persistencia.");
            }
        }
    }
}
