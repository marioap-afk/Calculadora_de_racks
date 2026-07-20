using Autodesk.AutoCAD.Colors;
using Autodesk.AutoCAD.DatabaseServices;

namespace RackCad.Plugin
{
    /// <summary>
    /// Shared Plugin helper: ensure a named layer exists with a given ACI color. Extracted (I-09) from the two
    /// byte-identical <c>EnsureLayer</c> copies that lived in <see cref="RackFrameCommands"/> (RACKLAYOUT /
    /// RACKRELLENAR) and in the lateral header drawer, so the layer-creation policy lives in ONE place. Behavior is
    /// unchanged: an existing layer is returned as-is; otherwise a fresh <see cref="LayerTableRecord"/> is created
    /// with <see cref="Color.FromColorIndex"/> (ByAci) and its id returned.
    /// </summary>
    internal static class LayerHelper
    {
        /// <summary>Ensure a named layer exists with the given ACI color; returns its id (existing or freshly created).</summary>
        public static ObjectId EnsureLayer(Database database, Transaction transaction, string name, short aci)
        {
            var layerTable = (LayerTable)transaction.GetObject(database.LayerTableId, OpenMode.ForRead);
            if (layerTable.Has(name))
            {
                return layerTable[name];
            }

            layerTable.UpgradeOpen();
            var record = new LayerTableRecord { Name = name, Color = Color.FromColorIndex(ColorMethod.ByAci, aci) };
            var id = layerTable.Add(record);
            transaction.AddNewlyCreatedDBObject(record, true);
            return id;
        }
    }
}
