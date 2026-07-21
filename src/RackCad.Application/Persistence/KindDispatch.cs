using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace RackCad.Application.Persistence
{
    /// <summary>
    /// Immutable, explicit lookup of items keyed by a <see cref="RackEmbedDocument.Kind"/> string, with no
    /// reflection. Rejects null items, blank keys and duplicate keys (including case-variant duplicates) at
    /// construction, so a wiring mistake fails fast — in a test or the build — instead of silently at runtime.
    /// Exposes a case-sensitive lookup (mirroring the C# <c>switch (kind)</c> the Plugin consumers used) and a
    /// case-insensitive one (for the copy restamp path).
    ///
    /// Pure and unit-testable WITHOUT AutoCAD (ADR-0003): the Plugin's <c>KindHandlerRegistry</c> is a thin wrapper
    /// of this over <c>IRackKindHandler</c>, so the registration invariants and the missing-key behaviour are
    /// covered here rather than only by a mechanical inventory of the Plugin.
    /// </summary>
    public sealed class KindDispatch<T> where T : class
    {
        // Ordinal mirrors the consumers' `switch (kind)` (case-sensitive); the ignore-case view serves the one
        // consumer that compared kinds with OrdinalIgnoreCase (the restamp).
        private readonly Dictionary<string, T> _ordinal;
        private readonly Dictionary<string, T> _ignoreCase;
        private readonly ReadOnlyCollection<T> _items;

        public KindDispatch(IEnumerable<T> items, Func<T, string> keyOf)
        {
            if (items == null)
            {
                throw new ArgumentNullException(nameof(items));
            }

            if (keyOf == null)
            {
                throw new ArgumentNullException(nameof(keyOf));
            }

            _ordinal = new Dictionary<string, T>(StringComparer.Ordinal);
            _ignoreCase = new Dictionary<string, T>(StringComparer.OrdinalIgnoreCase);
            var ordered = new List<T>();

            foreach (var item in items)
            {
                if (item == null)
                {
                    throw new ArgumentException("A kind-keyed item cannot be null.", nameof(items));
                }

                var key = keyOf(item);
                if (string.IsNullOrWhiteSpace(key))
                {
                    throw new ArgumentException("A kind-keyed item must declare a non-empty kind.", nameof(items));
                }

                // Checking the case-insensitive map rejects both exact and case-variant duplicates (a case-variant
                // duplicate would make TryGetIgnoreCase ambiguous).
                if (_ignoreCase.ContainsKey(key))
                {
                    throw new ArgumentException($"Duplicate kind '{key}'.", nameof(items));
                }

                _ordinal.Add(key, item);
                _ignoreCase.Add(key, item);
                ordered.Add(item);
            }

            _items = new ReadOnlyCollection<T>(ordered);
        }

        /// <summary>The registered items in declaration order. Read-only: there is no cast-to-mutable path.</summary>
        public IReadOnlyList<T> Items => _items;

        /// <summary>Case-sensitive lookup (ordinal), mirroring the consumers' <c>switch (kind)</c>. Never throws.</summary>
        public bool TryGet(string kind, out T item)
        {
            if (kind == null)
            {
                item = null;
                return false;
            }

            return _ordinal.TryGetValue(kind, out item);
        }

        /// <summary>Case-INSENSITIVE lookup (for the copy restamp path). Never throws.</summary>
        public bool TryGetIgnoreCase(string kind, out T item)
        {
            if (kind == null)
            {
                item = null;
                return false;
            }

            return _ignoreCase.TryGetValue(kind, out item);
        }
    }

    /// <summary>
    /// Single source of the historic visible message for an envelope kind with no registered handler, shared by
    /// RACKEDITAR, RACKBOMTOTAL and the copy restamp so the wording never drifts. The AutoCAD editor line prefix
    /// (<c>"\n"</c>) is added at the call site, exactly as every other Plugin message is.
    /// </summary>
    public static class KindDispatchMessages
    {
        public static string NotRecognized(string kind) => "RackCad: tipo de rack no reconocido (" + kind + ").";
    }
}
