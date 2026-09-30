using RackCad.Application.Systems.Shared;
using RackCad.Domain.Systems.Shared;

namespace RackCad.Application.Views.Policy
{
    public enum RackViewProductOperation
    {
        CreateFirst,
        InsertSibling,
        Batch,
        GroupProjection
    }

    /// <summary>Product exposure only. Physical existence remains owned by RackViewAvailability.</summary>
    public static class RackViewExposure
    {
        public static bool IsExposed(
            RackSystemKind systemKind,
            RackViewAddress address,
            RackViewProductOperation operation)
        {
            if (!IsSupportedAddress(systemKind, address))
            {
                return false;
            }

            // Flow Bed deliberately remains a single-view product. Its physical lateral exists, but the product
            // does not expose a sister, queue or group projection for it.
            return systemKind != RackSystemKind.Cama
                || operation == RackViewProductOperation.CreateFirst;
        }

        private static bool IsSupportedAddress(RackSystemKind systemKind, RackViewAddress address)
        {
            switch (systemKind)
            {
                case RackSystemKind.SelectiveRack:
                    return Is(address, DimensionViewKind.Frontal, RackViewVariantKind.Fondo)
                        || Is(address, DimensionViewKind.Lateral, RackViewVariantKind.Post)
                        || Is(address, DimensionViewKind.Planta, RackViewVariantKind.Whole);
                case RackSystemKind.PalletFlow:
                    return Is(address, DimensionViewKind.Frontal, RackViewVariantKind.FlowEnd)
                        || Is(address, DimensionViewKind.Lateral, RackViewVariantKind.Post)
                        || Is(address, DimensionViewKind.Planta, RackViewVariantKind.Whole);
                case RackSystemKind.PushBack:
                    return Is(address, DimensionViewKind.Frontal, RackViewVariantKind.PushBackCut)
                        || Is(address, DimensionViewKind.Lateral, RackViewVariantKind.Post)
                        || Is(address, DimensionViewKind.Planta, RackViewVariantKind.Whole);
                case RackSystemKind.Cantilever:
                    return Is(address, DimensionViewKind.Frontal, RackViewVariantKind.Whole)
                        || Is(address, DimensionViewKind.Lateral, RackViewVariantKind.Station)
                        || Is(address, DimensionViewKind.Planta, RackViewVariantKind.Whole);
                case RackSystemKind.Selective:
                    return Is(address, DimensionViewKind.Lateral, RackViewVariantKind.Whole)
                        || Is(address, DimensionViewKind.Planta, RackViewVariantKind.Whole);
                case RackSystemKind.Cama:
                    return Is(address, DimensionViewKind.Lateral, RackViewVariantKind.Whole);
                default:
                    return false;
            }
        }

        private static bool Is(
            RackViewAddress address,
            DimensionViewKind kind,
            RackViewVariantKind variant) =>
            address.Kind == kind && address.Variant.Kind == variant;
    }
}
