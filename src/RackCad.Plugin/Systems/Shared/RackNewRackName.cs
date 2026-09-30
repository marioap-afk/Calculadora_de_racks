using System.Collections.Generic;
using System.Linq;
using Autodesk.AutoCAD.ApplicationServices;
using RackCad.Application.Persistence;
using RackCad.Domain.Systems.Shared;

namespace RackCad.Plugin.Systems.Shared
{
    /// <summary>
    /// I-60: the logical name of a NEW rack, applied only on the creation paths of a new logical rack (never on an edit, a
    /// duplication, a layout, a report or a projection). The rule is the pure <see cref="RackLogicalNameAllocator"/>; this
    /// adapter only reads the logical names already in the drawing. It writes nothing.
    /// </summary>
    internal static class RackNewRackName
    {
        /// <summary>The requested name when it is assigned; otherwise the next automatic name of the family in this drawing.</summary>
        internal static string Resolve(Document document, RackSystemKind kind, string requestedName)
        {
            if (!RackLogicalNameAllocator.IsUnassigned(kind, requestedName))
            {
                return requestedName;
            }

            return RackLogicalNameAllocator.Next(kind, LogicalNames(document));
        }

        /// <summary>Every logical name the drawing's rack envelopes carry (placed or not). A read-only scan.</summary>
        private static IReadOnlyList<string> LogicalNames(Document document)
        {
            if (document == null)
            {
                return new string[0];
            }

            using (document.LockDocument())
            using (var transaction = document.Database.TransactionManager.StartTransaction())
            {
                var names = RackBlockFinder.ScanEnvelopes(transaction, document.Database, includeReferenceCount: false)
                    .Select(envelope => envelope.Embed?.Name)
                    .Where(name => !string.IsNullOrWhiteSpace(name))
                    .ToList();
                transaction.Commit();
                return names;
            }
        }
    }
}
