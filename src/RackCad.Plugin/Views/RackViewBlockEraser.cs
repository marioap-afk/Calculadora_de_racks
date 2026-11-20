#nullable enable
using System;
using System.Collections.Generic;
using Autodesk.AutoCAD.DatabaseServices;

namespace RackCad.Plugin.Views
{
    internal static class RackViewBlockEraser
    {
        internal static IReadOnlyCollection<ObjectId> EraseInTransaction(Transaction transaction, ObjectId definitionId)
        {
            throw new NotSupportedException();
        }
    }
}
