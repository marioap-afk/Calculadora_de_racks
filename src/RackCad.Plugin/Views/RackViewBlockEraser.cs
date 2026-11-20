#nullable enable
using System;
using System.Collections.Generic;
using Autodesk.AutoCAD.DatabaseServices;

namespace RackCad.Plugin.Views
{
    /// <summary>
    /// I-64 F4B PB-T3c (D-PBT3-11) — erases ONE view-block definition together with its direct references, inside a scope that the CALLER
    /// owns: it takes no lock of the document, opens no scope of its own, confirms nothing and tidies nothing up afterwards. It does what the
    /// classic erasure does for one definition, in the same order, and it returns the definitions that the erased one nested, so the caller
    /// can deal with them once its own scope is confirmed.
    /// </summary>
    internal static class RackViewBlockEraser
    {
        internal static IReadOnlyCollection<ObjectId> EraseInTransaction(Transaction transaction, ObjectId definitionId)
        {
            // The classic erasure skips a layout in silence; here the caller aborts as a whole, so it is an error.
            if (!(transaction.GetObject(definitionId, OpenMode.ForRead) is BlockTableRecord definition) || definition.IsLayout)
                throw new InvalidOperationException("La definición " + definitionId + " no es una vista que se pueda borrar.");

            // The definitions that this one nests are captured BEFORE it is erased: erasing the owner only dereferences them.
            var nested = new HashSet<ObjectId>();
            foreach (ObjectId childId in definition)
            {
                if (transaction.GetObject(childId, OpenMode.ForRead) is BlockReference child && !child.BlockTableRecord.IsNull)
                    nested.Add(child.BlockTableRecord);
            }

            // Every direct reference goes first, also the ones that sit inside other definitions, and then the owner.
            foreach (ObjectId referenceId in definition.GetBlockReferenceIds(directOnly: true, forceValidity: true))
            {
                if (transaction.GetObject(referenceId, OpenMode.ForWrite) is BlockReference reference) reference.Erase();
            }

            definition.UpgradeOpen();
            definition.Erase();
            return nested;
        }
    }
}
