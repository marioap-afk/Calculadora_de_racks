using System;
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
    /// assembly scanning. Null handlers, blank keys and duplicate keys are rejected at construction, so a wiring
    /// mistake fails fast (in the build / a debug session), never silently at runtime. Lookups never throw: a
    /// missing handler is surfaced by the CALLER (RACKEDITAR prints the visible "tipo de rack no reconocido"
    /// message), preserving the historical behaviour.
    ///
    /// This is a PLUGIN registry for AutoCAD operations; it is NOT the Application-layer <c>SystemRegistry</c>
    /// (persistence, validation, library, keyed by <c>RackSystemKind</c>). The two live in different layers with
    /// different vocabularies and are deliberately kept separate.
    /// </summary>
    internal sealed class KindHandlerRegistry
    {
        // Ordinal map mirrors the C# `switch (embed.Kind)` the consumers used (case-sensitive). The ignore-case
        // map preserves the ONE case-insensitive consumer (RackEnvelopeRestamp compared with OrdinalIgnoreCase).
        private readonly Dictionary<string, IRackKindHandler> _ordinal;
        private readonly Dictionary<string, IRackKindHandler> _ignoreCase;
        private readonly IReadOnlyList<IRackKindHandler> _handlers;

        public KindHandlerRegistry(IEnumerable<IRackKindHandler> handlers)
        {
            if (handlers == null)
            {
                throw new ArgumentNullException(nameof(handlers));
            }

            _ordinal = new Dictionary<string, IRackKindHandler>(StringComparer.Ordinal);
            _ignoreCase = new Dictionary<string, IRackKindHandler>(StringComparer.OrdinalIgnoreCase);
            var ordered = new List<IRackKindHandler>();

            foreach (var handler in handlers)
            {
                if (handler == null)
                {
                    throw new ArgumentException("A kind handler cannot be null.", nameof(handlers));
                }

                if (string.IsNullOrWhiteSpace(handler.Kind))
                {
                    throw new ArgumentException("A kind handler must declare a non-empty Kind.", nameof(handlers));
                }

                if (_ordinal.ContainsKey(handler.Kind) || _ignoreCase.ContainsKey(handler.Kind))
                {
                    throw new ArgumentException($"Duplicate kind handler for '{handler.Kind}'.", nameof(handlers));
                }

                _ordinal.Add(handler.Kind, handler);
                _ignoreCase.Add(handler.Kind, handler);
                ordered.Add(handler);
            }

            _handlers = ordered;
        }

        /// <summary>The registered handlers in declaration order (for inventory / verification).</summary>
        public IReadOnlyList<IRackKindHandler> Handlers => _handlers;

        /// <summary>Case-sensitive lookup, mirroring the consumers' <c>switch (embed.Kind)</c>. Never throws.</summary>
        public bool TryGet(string kind, out IRackKindHandler handler)
        {
            if (kind == null)
            {
                handler = null;
                return false;
            }

            return _ordinal.TryGetValue(kind, out handler);
        }

        /// <summary>Case-INSENSITIVE lookup for the one consumer that compared kinds with
        /// <see cref="StringComparison.OrdinalIgnoreCase"/> (<see cref="RackEnvelopeRestamp"/>). Never throws.</summary>
        public bool TryGetIgnoreCase(string kind, out IRackKindHandler handler)
        {
            if (kind == null)
            {
                handler = null;
                return false;
            }

            return _ignoreCase.TryGetValue(kind, out handler);
        }

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
