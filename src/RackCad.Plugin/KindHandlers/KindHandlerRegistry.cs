using System.Collections.Generic;
using RackCad.Application.Persistence;

namespace RackCad.Plugin.KindHandlers
{
    /// <summary>
    /// Explicit, immutable registry mapping the persisted <see cref="RackEmbedDocument.Kind"/> string to the Plugin
    /// operations for that rack type (edit, BOM, restamp). It replaces the per-consumer <c>switch (embed.Kind)</c>
    /// / <c>if</c> chains that RACKEDITAR, RACKBOMTOTAL and the copy restamp each repeated (auditoría E2).
    ///
    /// Handlers are listed BY HAND in <see cref="Default"/>, in a stable, documented order — no reflection, no
    /// assembly scanning. The registration invariants (no null handler, no blank key, no duplicate key) and the
    /// lookups live in the pure, unit-tested <see cref="KindDispatch{T}"/>, so a wiring mistake fails fast (in a
    /// test / the build) and <see cref="Handlers"/> is genuinely read-only (no cast-to-mutable path). Lookups never
    /// throw: a missing handler is surfaced by the CALLER via <see cref="KindHandlerDispatch"/>, which prints the
    /// historic "tipo de rack no reconocido" message, so no operation continues silently with an unknown kind.
    ///
    /// This is a PLUGIN registry for AutoCAD operations; it is NOT the Application-layer <c>SystemRegistry</c>
    /// (persistence, validation, library, keyed by <c>RackSystemKind</c>). The two live in different layers with
    /// different vocabularies and are deliberately kept separate.
    /// </summary>
    internal sealed class KindHandlerRegistry
    {
        private readonly KindDispatch<IRackKindHandler> _dispatch;

        public KindHandlerRegistry(IEnumerable<IRackKindHandler> handlers)
            => _dispatch = new KindDispatch<IRackKindHandler>(handlers, handler => handler.Kind);

        /// <summary>The registered handlers in declaration order (for inventory / verification). Read-only.</summary>
        public IReadOnlyList<IRackKindHandler> Handlers => _dispatch.Items;

        /// <summary>Case-sensitive lookup, mirroring the consumers' <c>switch (embed.Kind)</c>. Never throws.</summary>
        public bool TryGet(string kind, out IRackKindHandler handler) => _dispatch.TryGet(kind, out handler);

        /// <summary>Case-INSENSITIVE lookup for the one consumer that compared kinds with OrdinalIgnoreCase
        /// (<see cref="RackEnvelopeRestamp"/>). Never throws.</summary>
        public bool TryGetIgnoreCase(string kind, out IRackKindHandler handler) => _dispatch.TryGetIgnoreCase(kind, out handler);

        /// <summary>
        /// The single, explicit composition root: the four embedded kinds in canonical order (the same order the
        /// former RACKEDITAR / RACKBOMTOTAL switches used). <c>Larguero</c> has no embed discriminator and no draw
        /// block, so it is deliberately ABSENT — an unknown kind resolves to no handler.
        /// </summary>
        public static KindHandlerRegistry Default { get; } = new KindHandlerRegistry(new IRackKindHandler[]
        {
            new SelectiveKindHandler(),
            new DynamicKindHandler(),
            new CabeceraKindHandler(),
            new CamaKindHandler(),
        });
    }
}
