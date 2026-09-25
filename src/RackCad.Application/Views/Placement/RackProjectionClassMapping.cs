using RackCad.Application.Systems.Shared;
using RackCad.Domain.Systems.Shared;

namespace RackCad.Application.Views.Placement
{
    public enum RackProjectionMode
    {
        Rigid,
        Orthographic
    }

    public enum RackProjectionFamily
    {
        Rack,
        Cantilever
    }

    /// <summary>
    /// Frozen class mapping (Owner decision OD-7.a A). Same class is Rigid; planta against elevations is
    /// Orthographic; frontal against lateral is not exposed.
    /// </summary>
    public static class RackProjectionClassMapping
    {
        public static bool TryMap(
            DimensionViewKind sourceKind,
            DimensionViewKind targetKind,
            out RackProjectionMode mode,
            out RackPhysicalAxis conservedAxis)
        {
            if (sourceKind == targetKind)
            {
                mode = RackProjectionMode.Rigid;
                conservedAxis = sourceKind == DimensionViewKind.Lateral
                    ? RackPhysicalAxis.Depth
                    : RackPhysicalAxis.Run;
                return true;
            }

            mode = RackProjectionMode.Orthographic;

            if (sourceKind == DimensionViewKind.Planta && targetKind == DimensionViewKind.Frontal
                || sourceKind == DimensionViewKind.Frontal && targetKind == DimensionViewKind.Planta)
            {
                conservedAxis = RackPhysicalAxis.Run;
                return true;
            }

            if (sourceKind == DimensionViewKind.Planta && targetKind == DimensionViewKind.Lateral
                || sourceKind == DimensionViewKind.Lateral && targetKind == DimensionViewKind.Planta)
            {
                conservedAxis = RackPhysicalAxis.Depth;
                return true;
            }

            conservedAxis = RackPhysicalAxis.Run;
            return false;
        }

        public static RackProjectionFamily FamilyOf(RackSystemKind systemKind)
            => systemKind == RackSystemKind.Cantilever
                ? RackProjectionFamily.Cantilever
                : RackProjectionFamily.Rack;
    }
}
