using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Bom;
using RackCad.Application.Catalogs;
using RackCad.Domain.Systems;

namespace RackCad.Application.Systems
{
    /// <summary>
    /// Aggregates a whole dynamic system into one bill of materials by merging the BOM of every
    /// header module (each with its own associated configuration). Separators and intermediate
    /// posts contribute nothing yet (no profiled members); when they gain rails/rollers they simply
    /// add to the same merge.
    /// </summary>
    public static class SystemBomBuilder
    {
        public static BillOfMaterials Build(DynamicRackSystem system, RackCatalog catalog)
        {
            if (system == null)
            {
                return new BillOfMaterials(new List<BomLine>());
            }

            var headerBoms = system.Modules
                .Where(module => module.IsHeader && module.AssociatedFrameConfiguration != null)
                .Select(module => BomBuilder.Build(module.AssociatedFrameConfiguration, catalog));

            return BomBuilder.Merge(headerBoms);
        }
    }
}
