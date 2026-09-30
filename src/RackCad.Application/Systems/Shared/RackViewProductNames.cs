using RackCad.Application.Catalogs;
using RackCad.Application.Systems.Cantilever;
using RackCad.Application.Systems.Selective;
using RackCad.Domain.RackFrames;
using RackCad.Domain.Systems.Dynamic;
using RackCad.Domain.Systems.PushBack;
using RackCad.Domain.Systems.Selective;
using RackCad.Domain.Systems.Shared;

namespace RackCad.Application.Systems.Shared
{
    /// <summary>
    /// Which AUTH-11 base name each product composition asks for, per system and view address. It only CHOOSES among the
    /// <see cref="RackViewBaseName"/> functions (the naming authority); it invents no string. It was extracted from the
    /// Plugin's product compositions so that the rule "every prepared view has a usable block name" can be proved without AutoCAD.
    /// </summary>
    public static class RackViewProductNames
    {
        public static string Selective(
            SelectiveRackSystem resolved, RackViewAddress address, string rackName, string baseName)
            => address.Kind == DimensionViewKind.Planta
                ? RackViewBaseName.SelectivePlanta(resolved, rackName)
                : address.Kind == DimensionViewKind.Lateral
                    ? RackViewBaseName.LinkedLateral(baseName, address.Variant.Index)
                        ?? RackViewBaseName.SelectiveLateral(rackName, address.Variant.Index)
                    : RackViewBaseName.LinkedSelectiveFrontal(baseName, address.Variant.Index,
                        SelectiveDepthLayout.Count(resolved)) ?? RackViewBaseName.SelectiveFrontal(resolved, rackName);

        public static string Header(
            RackCatalog catalog, RackFrameConfiguration resolved, RackViewAddress address, string rackName)
            => address.Kind == DimensionViewKind.Planta
                ? RackViewBaseName.CabeceraPlanta(rackName)
                : RackViewBaseName.CabeceraLateral(catalog, resolved, rackName);

        public static string Dynamic(DynamicRackSystem system, RackViewAddress address, string name, string baseName)
            => address.Kind == DimensionViewKind.Planta ? RackViewBaseName.DynamicPlanta(system, name)
                : address.Kind == DimensionViewKind.Frontal ? RackViewBaseName.DynamicFrontal(system, name, address.Variant.FlowEnd)
                : RackViewBaseName.LinkedLateral(baseName, address.Variant.Index)
                    ?? RackViewBaseName.DynamicLateral(system, name);

        public static string PushBack(PushBackSystem system, RackViewAddress address, string name, string baseName)
            => address.Kind == DimensionViewKind.Planta ? RackViewBaseName.PushBackPlanta(system, name)
                : address.Kind == DimensionViewKind.Frontal
                    ? RackViewBaseName.PushBackFrontal(system, name, address.Variant.PushBackEnd, address.Variant.PushBackSide)
                    : RackViewBaseName.LinkedLateral(baseName, address.Variant.Index)
                        ?? RackViewBaseName.PushBackLateral(system, name, address.Variant.Index);

        public static string Cantilever(RackViewAddress address, string rackName)
            => RackViewBaseName.CantileverGenerated(
                address.Kind == DimensionViewKind.Planta ? CantileverViewKind.Planta
                    : address.Kind == DimensionViewKind.Lateral ? CantileverViewKind.Lateral
                    : CantileverViewKind.Frontal,
                address.Variant.Kind == RackViewVariantKind.Station ? address.Variant.Index : -1,
                rackName);
    }
}
