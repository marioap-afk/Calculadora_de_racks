using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Domain.Systems;

namespace RackCad.UI
{
    /// <summary>
    /// Owner decision (2026-07-24) — the Push Back rear tope is configured EXCLUSIVELY from Seguridad, alongside the rest
    /// of the safety elements. Instead of growing a second tope UI, this adapter REUSES the shared
    /// <see cref="SafetyTopeGridWindow"/>: it PROJECTS a <see cref="PushBackRearTopeConfig"/> onto the dialog's inputs on
    /// open, and RECOVERS the SAQUE and the per-cell deactivations from its result on accept.
    ///
    /// Pure translation with no WPF work of its own, so the whole round trip is testable without showing a window. Push
    /// Back has a single rear-stop family (one per frente × nivel at the high end), so the dialog's per-fondo / side /
    /// shared / frontal knobs have no Push Back meaning: they are pinned on open and IGNORED on accept, which is what
    /// keeps a future change to those knobs from leaking into the Push Back design.
    /// </summary>
    internal static class PushBackRearTopeDialogAdapter
    {
        /// <summary>Label the shared grid dialog shows for the Push Back rear stop.</summary>
        public const string Label = "Tope posterior (extremo alto)";

        /// <summary>The dialog's SAQUE input: the configured value when set, else the domain default.</summary>
        public static double Saque(PushBackRearTopeConfig config)
            => config != null && config.Saque > 0.0 ? config.Saque : PushBackDefaults.RearTopeSaque;

        /// <summary>
        /// The deactivated cells projected onto the dialog's grid. Both sides mean "on = drawn" and persist only the
        /// deactivations, so the projection is a COPY — never an inversion, and never a shared list.
        /// </summary>
        public static IReadOnlyList<SelectiveGridCell> OffCells(PushBackRearTopeConfig config)
            => config?.OffCells == null
                ? new List<SelectiveGridCell>()
                : config.OffCells.Select(cell => new SelectiveGridCell { Frente = cell.Frente, Level = cell.Level }).ToList();

        /// <summary>Levels per frente for the dialog's jagged grid; never empty (the dialog needs at least one column).</summary>
        /// <param name="allowBlankFronts">
        /// I-33, opt-in: honour a ZERO as "this frente has no cells" (a front EN BLANCO) instead of flooring it to one.
        /// Default false keeps the historical flooring for any caller that has no notion of blank fronts.
        /// </param>
        public static IReadOnlyList<int> LevelsPerFrente(IEnumerable<int> levels, bool allowBlankFronts = false)
        {
            var result = (levels ?? Enumerable.Empty<int>())
                .Select(count => allowBlankFronts ? Math.Max(0, count) : Math.Max(1, count))
                .ToList();
            if (result.Count == 0)
            {
                result.Add(Math.Max(1, DynamicRackDefaults.DefaultLoadLevels));
            }

            return result;
        }

        /// <summary>
        /// Recover the dialog's result into the config: SAQUE and the per-cell deactivations, and nothing else. A null
        /// result (the user cancelled) leaves the config untouched.
        /// </summary>
        public static void Apply(SafetyTopeGridWindow.TopeResult result, PushBackRearTopeConfig config)
        {
            if (result == null || config == null)
            {
                return;
            }

            config.Saque = result.Saque > 0.0 ? result.Saque : PushBackDefaults.RearTopeSaque;
            config.OffCells.Clear();
            foreach (var cell in result.OffCells ?? new List<SelectiveGridCell>())
            {
                config.OffCells.Add(new SelectiveGridCell { Frente = cell.Frente, Level = cell.Level });
            }
        }

    }
}
