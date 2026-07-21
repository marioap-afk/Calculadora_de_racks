using System;
using System.Globalization;
using Autodesk.AutoCAD.DatabaseServices;
using RackCad.Application;
using RackCad.Plugin.Systems;

namespace RackCad.Plugin
{
    /// <summary>
    /// Shared Plugin helper (I-09): clone a rack's block DEFINITION for an INDEPENDENT copy. Extracted from the
    /// CloneDefinition that lived in the former <c>RackFrameCommands</c> partial (used by RACKDUPLICAR and by RACKLAYOUT's
    /// independent copies). LINKED copies do NOT go through here — they reference the existing definition directly,
    /// so edit-one-edits-all still holds.
    ///
    /// The copy-name uniqueness policy is this helper's own (append " (1)", " (2)", …). It is intentionally NOT
    /// unified with <see cref="RackCad.Plugin.Headers.LateralHeaderDrawer"/>'s distinct "_1", "_2" policy: the two
    /// stay separate.
    /// </summary>
    internal static class RackCloner
    {
        /// <summary>
        /// Duplicate a rack's block DEFINITION for an INDEPENDENT copy: a fresh named BlockTableRecord holding clones of
        /// the source's entities (nested ARRAY defs are shared, not duplicated) and the restamped payload (new GUID +
        /// name). When <paramref name="labelFrom"/>/<paramref name="labelTo"/> are given, the drawn rack-name annotation
        /// (a DBText equal to the source's name) is renamed so the copy isn't visually labeled as the original.
        /// Returns the new definition id.
        /// </summary>
        public static ObjectId CloneDefinition(Database database, Transaction transaction, ObjectId sourceDefId, string copyName, string payload, string labelFrom = null, string labelTo = null)
        {
            var blockTable = (BlockTable)transaction.GetObject(database.BlockTableId, OpenMode.ForWrite);
            var source = (BlockTableRecord)transaction.GetObject(sourceDefId, OpenMode.ForRead);

            var clone = new BlockTableRecord { Name = UniqueBlockName(blockTable, copyName), Origin = source.Origin };
            var cloneId = blockTable.Add(clone);
            transaction.AddNewlyCreatedDBObject(clone, true);

            var entityIds = new ObjectIdCollection();
            foreach (ObjectId id in source)
            {
                entityIds.Add(id);
            }

            if (entityIds.Count > 0)
            {
                // Clone the source's entities into the fresh definition. The nested (ARRAY) defs they reference are NOT
                // in the clone set, so the copies point at the SAME nested defs — identical geometry, no duplication.
                var mapping = new IdMapping();
                database.DeepCloneObjects(entityIds, cloneId, mapping, false);
            }

            // Rename the drawn rack-name annotation (if any): the clone carries the ORIGINAL's DBText label.
            if (!string.IsNullOrWhiteSpace(labelFrom) && !string.IsNullOrWhiteSpace(labelTo))
            {
                foreach (ObjectId id in clone)
                {
                    if (transaction.GetObject(id, OpenMode.ForRead) is DBText text &&
                        string.Equals(text.TextString?.Trim(), labelFrom.Trim(), StringComparison.Ordinal))
                    {
                        text.UpgradeOpen();
                        text.TextString = labelTo;
                    }
                }
            }

            RackBlockData.Write(transaction, cloneId, payload); // replace the cloned (old) payload → independent rack
            return cloneId;
        }

        /// <summary>A block name not yet in the table; if taken, append " (1)", " (2)", … so no other block is renamed.</summary>
        private static string UniqueBlockName(BlockTable blockTable, string baseName)
        {
            var name = BlockNaming.SanitizeBlockName(baseName);
            if (!blockTable.Has(name))
            {
                return name;
            }

            for (var suffix = 1; ; suffix++)
            {
                var candidate = name + " (" + suffix.ToString(CultureInfo.InvariantCulture) + ")";
                if (!blockTable.Has(candidate))
                {
                    return candidate;
                }
            }
        }
    }
}
