using System;
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
    }
}
