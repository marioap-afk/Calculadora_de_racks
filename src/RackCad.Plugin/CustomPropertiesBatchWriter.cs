using System.Collections.Generic;
using Autodesk.AutoCAD.ApplicationServices;
using RackCad.Application.CustomProperties;
using RackCad.Application.Persistence;
using RackCad.Application.VentanaRackCad;

namespace RackCad.Plugin
{
    /// <summary>
    /// Writes a whole draft of the custom properties of the project (I-64 A-14, cuarta tanda, va; VA-06), as the Project write of
    /// <see cref="CustomPropertiesExecutor"/> does with one intent: one transaction re-reads the entry, requires it to be the one the page
    /// read, applies the draft (<see cref="CustomPropertiesBatch.Apply"/>), asks the write guard and writes once. Any refusal leaves without
    /// confirming. Only an Xrecord: no regeneration.
    /// </summary>
    internal static class CustomPropertiesBatchWriter
    {
        /// <summary>Null when written; otherwise why nothing was written.</summary>
        internal static string WriteProject(Document document, string fingerprint, IReadOnlyList<CustomPropertyDraftEntry> desired)
        {
            var database = document.Database;
            var store = new CustomPropertiesStore();

            using (document.LockDocument())
            using (var transaction = database.TransactionManager.StartTransaction())
            {
                var fresh = store.Read(CustomPropertiesData.Read(transaction, database));
                if (!string.Equals(VariablesDoorRules.PropertiesFingerprint(fresh), fingerprint, System.StringComparison.Ordinal))
                    return "las propiedades del proyecto cambiaron en el dibujo desde que se leyeron.";
                if (!fresh.CanWrite) return fresh.Error ?? "las propiedades del proyecto no se pueden escribir.";

                var applied = CustomPropertiesBatch.Apply(fresh, desired);
                if (!applied.Succeeded) return applied.Error;
                if (!CustomPropertiesWriteGuard.CanOverwrite(fresh, out var refusal)) return refusal;

                CustomPropertiesData.Write(transaction, database, store.Serialize(applied.Document));
                transaction.Commit();
                return null;
            }
        }
    }
}
