using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Bom;
using RackCad.Application.Catalogs;
using RackCad.Domain.Systems;

namespace RackCad.Application.Systems
{
    /// <summary>
    /// Aggregates a whole dynamic system into a COMPONENT bill of materials: every header module is a <b>cabecera</b>
    /// component (identical frames collapse, via <see cref="BomBuilder.Components"/>), each expanding to its pieces.
    /// Separators and intermediate posts contribute nothing yet (no profiled members); the future integrated bed will
    /// be added as one component per physical bed without exposing its internal drawing recipe in this BOM.
    /// </summary>
    public static class SystemBomBuilder
    {
        public static BillOfMaterials Build(DynamicRackSystem system, RackCatalog catalog)
        {
            if (system == null)
            {
                return new BillOfMaterials(new List<BomComponent>());
            }

            var cabeceras = system.Modules
                .Where(module => module.IsHeader && module.AssociatedFrameConfiguration != null)
                .Select(module => module.AssociatedFrameConfiguration);

            return new BillOfMaterials(BomBuilder.Components(cabeceras, catalog));
        }
    }
}
