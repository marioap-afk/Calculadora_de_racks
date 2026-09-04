using System;
using System.Globalization;
using System.Collections.Generic;
using System.Linq;

namespace RackCad.Application.Systems.Selective
{
    /// <summary>
    /// The ARBITRARY set of fondos an operation is aimed at (I-43) — "aplica esto a los fondos 1 y 3", with no
    /// requirement that they be adjacent, that they start at 0, or that fondo 0 be among them.
    /// <para>
    /// It is a set of INTENTIONS, not of facts: it stores what the caller asked for, normalized to distinct ascending
    /// indices so the resolution order never depends on the order the UI happened to click them. Whether each fondo
    /// EXISTS is a question about a <see cref="SelectiveTopology"/>, and only <see cref="SelectiveTargetResolver"/>
    /// answers it — which is why an out-of-range fondo is kept here and reported as an omission there, instead of
    /// disappearing silently at construction.
    /// </para>
    /// <para>
    /// Nothing here is persisted. A target set lives for exactly one operation: it is built, resolved into a plan,
    /// applied, and dropped. No <c>*Document</c> carries it and no DWG stores it.
    /// </para>
    /// </summary>
    public sealed class SelectiveFondoTargets
    {
        private readonly int[] fondos;

        private SelectiveFondoTargets(int[] normalized) => fondos = normalized;

        /// <summary>The requested fondos, distinct and ascending. May contain indices no topology has.</summary>
        public IReadOnlyList<int> Fondos => fondos;

        public int Count => fondos.Length;

        public bool IsEmpty => fondos.Length == 0;

        public bool Contains(int fondoIndex) => Array.IndexOf(fondos, fondoIndex) >= 0;

        /// <summary>No fondo at all. Every scope resolves to zero targets against it — including <c>Selected</c>.</summary>
        public static SelectiveFondoTargets None { get; } = new SelectiveFondoTargets(Array.Empty<int>());

        /// <summary>An arbitrary set, e.g. <c>Of(1, 3)</c>. Duplicates collapse; order is irrelevant.</summary>
        public static SelectiveFondoTargets Of(params int[] fondoIndices)
            => Of((IEnumerable<int>)fondoIndices);

        /// <summary>An arbitrary set from any sequence. Null is an empty set, not an error.</summary>
        public static SelectiveFondoTargets Of(IEnumerable<int> fondoIndices)
            => new SelectiveFondoTargets((fondoIndices ?? Enumerable.Empty<int>()).Distinct().OrderBy(index => index).ToArray());

        /// <summary>Just one fondo — the shape every operation of the Selectivo has today (the fondo being edited).</summary>
        public static SelectiveFondoTargets Single(int fondoIndex) => new SelectiveFondoTargets(new[] { fondoIndex });

        /// <summary>Every fondo the topology actually has, ascending. The explicit way to say "todo el rack".</summary>
        public static SelectiveFondoTargets All(SelectiveTopology topology)
            => topology == null || topology.FondoCount == 0
                ? None
                : new SelectiveFondoTargets(Enumerable.Range(0, topology.FondoCount).ToArray());

        /// <summary>
        /// Parse the compact subset notation the editor offers: <c>1+3</c>, <c>2-4</c>, <c>1,3-4</c>, <c>todos</c>.
        /// Separators are <c>,</c>, <c>+</c>, <c>;</c> or spaces; a range is <c>a-b</c> in either direction.
        /// <para>
        /// Input is ONE-BASED because that is what the editor shows ("Fondo 1"); the result is zero-based like every
        /// index in the model. An out-of-range or unreadable fondo is an ERROR, not a silent drop: the user typed
        /// something specific, and quietly narrowing an operation is exactly what this initiative is meant to prevent.
        /// </para>
        /// </summary>
        public static bool TryParse(string text, int fondoCount, out SelectiveFondoTargets targets, out string error)
        {
            targets = None;
            error = null;
            var trimmed = (text ?? string.Empty).Trim();
            if (trimmed.Length == 0)
            {
                error = "Indica al menos un fondo.";
                return false;
            }

            if (trimmed.Equals("todos", StringComparison.OrdinalIgnoreCase)
                || trimmed.Equals("todas", StringComparison.OrdinalIgnoreCase)
                || trimmed.Equals("all", StringComparison.OrdinalIgnoreCase)
                || trimmed == "*")
            {
                if (fondoCount <= 0)
                {
                    error = "No hay fondos.";
                    return false;
                }

                targets = new SelectiveFondoTargets(Enumerable.Range(0, fondoCount).ToArray());
                return true;
            }

            var found = new List<int>();
            foreach (var token in trimmed.Split(new[] { ',', '+', ';', ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries))
            {
                var range = token.Split(new[] { '-', '–' }, StringSplitOptions.RemoveEmptyEntries);
                if (range.Length > 2)
                {
                    error = "Rango inválido: " + token;
                    return false;
                }

                if (!TryFondo(range[0], fondoCount, out var first, out error)) return false;
                if (range.Length == 1)
                {
                    found.Add(first);
                    continue;
                }

                if (!TryFondo(range[1], fondoCount, out var last, out error)) return false;
                for (var index = Math.Min(first, last); index <= Math.Max(first, last); index++) found.Add(index);
            }

            if (found.Count == 0)
            {
                error = "Indica al menos un fondo.";
                return false;
            }

            targets = Of(found);
            return true;
        }

        /// <summary>One one-based fondo number to a zero-based index, refusing anything the rack does not have.</summary>
        private static bool TryFondo(string token, int fondoCount, out int index, out string error)
        {
            index = 0;
            error = null;
            if (!int.TryParse(token.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var oneBased))
            {
                error = "No es un número de fondo: " + token.Trim();
                return false;
            }

            if (oneBased < 1 || oneBased > fondoCount)
            {
                error = string.Format(
                    CultureInfo.CurrentCulture, "El fondo {0} no existe (hay {1}).", oneBased, Math.Max(0, fondoCount));
                return false;
            }

            index = oneBased - 1;
            return true;
        }
    }
}
