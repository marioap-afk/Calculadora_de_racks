using System;
using System.Collections.Generic;
using Autodesk.AutoCAD.DatabaseServices;

namespace RackCad.Plugin.Systems.Shared
{
    /// <summary>
    /// T6 at (I-64, sexta tanda; corrections of the review of c4985a2e, I-2): the state of «Actualizar todo» (RACKACTUALIZARTODO) while its
    /// ONE outer transaction is open. Two shared helpers of the redraw consult it, and only while it is active; outside the command nothing
    /// changes:
    /// <list type="bullet">
    /// <item><c>BlockLibraryImporter.EnsureForPlan</c> imports nothing inside the transaction (the importer must run «OUTSIDE the drawing
    /// transaction»): it records what the drawing lacks and the library has (<see cref="PendingImports"/>, imported after a rollback,
    /// OUTSIDE, before the set is written again) and what neither has (<see cref="DraftMissing"/>, a notice of the draft).</item>
    /// <item><c>LateralHeaderDrawer.PurgeUnreferenced</c>, meant to run «on the COMMITTED drawing», only collects its candidates
    /// (<see cref="TakePurge"/>); the command purges them once after the outer commit, in the same command (one «U»).</item>
    /// </list>
    /// </summary>
    internal static class RackUpdateAllScope
    {
        private static readonly HashSet<string> Pending = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        private static readonly HashSet<string> DraftPendingKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        private static readonly HashSet<string> DraftMissingKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        private static readonly HashSet<ObjectId> Purge = new HashSet<ObjectId>();

        /// <summary>The outer transaction of «Actualizar todo» is open.</summary>
        internal static bool IsActive { get; private set; }

        internal static void Enter()
        {
            IsActive = true;
            ClearPass();
        }

        /// <summary>Leaves the scope; what it collected is forgotten. Always called when the command ends.</summary>
        internal static void Exit()
        {
            IsActive = false;
            ClearPass();
        }

        /// <summary>A pass of the set was discarded (rollback): what it observed and collected belongs to nothing now.</summary>
        internal static void ClearPass()
        {
            Pending.Clear();
            DraftPendingKeys.Clear();
            DraftMissingKeys.Clear();
            Purge.Clear();
        }

        /// <summary>The write of the next draft starts: its own observations start empty.</summary>
        internal static void BeginDraft()
        {
            DraftPendingKeys.Clear();
            DraftMissingKeys.Clear();
        }

        /// <summary>What the importer found while the scope is active: <paramref name="toImport"/> the library has, <paramref name="missing"/> it does not.</summary>
        internal static void Observed(IEnumerable<string> toImport, IEnumerable<string> missing)
        {
            foreach (var key in toImport ?? Array.Empty<string>())
            {
                Pending.Add(key);
                DraftPendingKeys.Add(key);
            }

            foreach (var key in missing ?? Array.Empty<string>()) DraftMissingKeys.Add(key);
        }

        /// <summary>The blocks of the library the pass needs and the drawing does not have yet.</summary>
        internal static IReadOnlyCollection<string> PendingImports => new List<string>(Pending);

        /// <summary>The blocks of the library the current draft needs and the drawing does not have yet.</summary>
        internal static IReadOnlyCollection<string> DraftPending => new List<string>(DraftPendingKeys);

        /// <summary>The blocks the current draft needs that neither the drawing nor the library has: its pieces are left out, as in its «Actualizar».</summary>
        internal static IReadOnlyCollection<string> DraftMissing => new List<string>(DraftMissingKeys);

        /// <summary>The purge a helper asked for, deferred to after the outer commit.</summary>
        internal static void DeferPurge(IEnumerable<ObjectId> candidates)
        {
            if (candidates == null) return;
            foreach (var id in candidates) Purge.Add(id);
        }

        /// <summary>Takes the deferred purge (and forgets it).</summary>
        internal static IReadOnlyCollection<ObjectId> TakePurge()
        {
            var ids = new List<ObjectId>(Purge);
            Purge.Clear();
            return ids;
        }
    }
}
