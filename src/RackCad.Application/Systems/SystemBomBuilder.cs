using System.Linq;
using RackCad.Application.Bom;
using RackCad.Application.Catalogs;

namespace RackCad.Application.Systems
{
    /// <summary>
    /// Aggregates a whole dynamic system into one bill of materials by merging the BOM of every
    /// header module. With a symmetric system that means the shared header counted at both ends
    /// (quantities doubled). Separators and intermediate posts contribute nothing yet (they carry
    /// no profiled members in the current model); when they gain rails/rollers, they simply add to
    /// the same merge.
    /// </summary>
    public static class SystemBomBuilder
    {
        public static BillOfMaterials Build(ComposedDynamicRack composed, RackCatalog catalog)
        {
            if (composed == null)
            {
                return new BillOfMaterials(new System.Collections.Generic.List<BomLine>());
            }

            var headerBoms = composed.PlacedModules
                .Where(module => module.IsHeader)
                .Select(module => BomBuilder.Build(module.Header, catalog));

            return BomBuilder.Merge(headerBoms);
        }
    }
}
