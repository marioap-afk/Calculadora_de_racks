using System;
using System.Collections.Generic;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.ApplicationServices;
using RackCad.Application.Views.Redraw;

namespace RackCad.Plugin.Systems.Shared
{
    internal static class SiblingRedrawTransaction
    {
        internal static RackSiblingMutationResult Mutate(Document document, IReadOnlyList<SiblingRedrawUnit> units)
        {
            if (document == null) return RackSiblingMutationResult.Discarded(null, "DOCUMENT_REQUIRED");
            if (units == null) return RackSiblingMutationResult.Discarded(null, "UNITS_REQUIRED");

            try
            {
                using (var transaction = document.Database.TransactionManager.StartTransaction())
                {
                    for (var index = 0; index < units.Count; index++)
                    {
                        var unit = units[index];
                        if (unit == null)
                        {
                            return RackSiblingMutationResult.Discarded(null, "NULL_UNIT");
                        }

#if DEBUG
                        SiblingRedrawDebugFaultInjection.ThrowIfRequested(index + 1);
#endif
                        var result = unit.Apply(transaction);
                        if (result == null || result.Kind == RackSiblingMutationKind.Discarded)
                        {
                            return result ?? RackSiblingMutationResult.Discarded(unit.DefinitionKey, "UNIT_RETURNED_NULL");
                        }
                    }

                    transaction.Commit();
                    return RackSiblingMutationResult.Committed();
                }
            }
            catch (Exception ex)
            {
                return RackSiblingMutationResult.Discarded(null, ex.Message);
            }
        }

        internal static void Post(Document document, IReadOnlyList<SiblingRedrawUnit> units)
        {
            if (units != null)
            {
                foreach (var unit in units)
                {
                    unit?.Post();
                }
            }

            document?.Editor.Regen();
        }
    }
}
