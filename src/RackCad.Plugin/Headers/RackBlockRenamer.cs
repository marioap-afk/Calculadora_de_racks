using System;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using RackCad.Application;

namespace RackCad.Plugin.Headers
{
    /// <summary>
    /// Best-effort sync of a rack view's block DEFINITION name to the current rack name, so renaming a rack updates the
    /// name shown in the block manager / INSERT / properties across ALL its views (frontal, lateral, planta) — not just
    /// the drawn annotation and the embedded payload. Block references point by object id, so renaming never breaks a
    /// reference. Cosmetic only: it no-ops when the name is blank, already matches, or the target is taken by another
    /// block, and never throws (a failed rename just leaves the old name).
    /// </summary>
    internal static class RackBlockRenamer
    {
        /// <summary>Rename <paramref name="blockId"/> to <paramref name="desiredRawName"/> (sanitized the same way the
        /// insert path names blocks). Callers pass the SAME per-view name the insert path would use, so insert and rename
        /// stay consistent by construction.</summary>
        public static void SyncName(Document document, ObjectId blockId, string desiredRawName)
        {
            if (document == null || blockId.IsNull || string.IsNullOrWhiteSpace(desiredRawName))
            {
                return;
            }

            try
            {
                var target = BlockNaming.SanitizeBlockName(desiredRawName);

                using (document.LockDocument())
                using (var tr = document.Database.TransactionManager.StartTransaction())
                {
                    if (!(tr.GetObject(blockId, OpenMode.ForRead) is BlockTableRecord def)
                        || def.IsErased || def.IsLayout
                        || string.Equals(def.Name, target, StringComparison.OrdinalIgnoreCase))
                    {
                        return; // nothing to do (missing, layout, or already named right)
                    }

                    var blockTable = (BlockTable)tr.GetObject(document.Database.BlockTableId, OpenMode.ForRead);
                    if (blockTable.Has(target))
                    {
                        return; // taken by another block — skip rather than uniquify (keep names predictable)
                    }

                    def.UpgradeOpen();
                    def.Name = target;
                    tr.Commit();
                }
            }
            catch
            {
                // The name is cosmetic; never let a rename failure break the edit flow.
            }
        }
    }
}
