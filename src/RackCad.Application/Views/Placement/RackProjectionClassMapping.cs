using System.Collections.Generic;
using System.Linq;
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
    /// Closed class-pair policy. Same class is Rigid; planta against elevations is Orthographic (Owner decision OD-7.a A). G16
    /// C16-07: the Owner revoked the «frontal against lateral is not exposed» clause; the two elevations are Orthographic on Height,
    /// the only physical axis both AUTH-05 frames represent (Frontal = Run x Height, Lateral = Depth x Height). Whether a rack offers
    /// the target class at all is a fact of its system (<see cref="OffersClass"/>), not of this table.
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

            if (sourceKind == DimensionViewKind.Frontal && targetKind == DimensionViewKind.Lateral
                || sourceKind == DimensionViewKind.Lateral && targetKind == DimensionViewKind.Frontal)
            {
                conservedAxis = RackPhysicalAxis.Height;
                return true;
            }

            conservedAxis = RackPhysicalAxis.Run;
            return false;
        }

        /// <summary>
        /// G16 C16-07: whether the rack offers any view of the target class (its resolved, physically available addresses). A cross-class
        /// pair towards a class the system does not have (a cabecera has no frontal) is a genuinely unsupported combination.
        /// </summary>
        public static bool OffersClass(IEnumerable<RackViewAddress> availableTargetAddresses, DimensionViewKind targetKind)
            => availableTargetAddresses != null && availableTargetAddresses.Any(address => address.Kind == targetKind);

        public static RackProjectionFamily FamilyOf(RackSystemKind systemKind)
            => systemKind == RackSystemKind.Cantilever
                ? RackProjectionFamily.Cantilever
                : RackProjectionFamily.Rack;
    }
}
