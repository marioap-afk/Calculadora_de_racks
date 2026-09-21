using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using RackCad.Application.Systems.PushBack;
using RackCad.Application.Systems.Shared;
using RackCad.Domain.Systems.PushBack;
using RackCad.Plugin.Drawing;
using RackCad.Plugin.Systems.Shared;

namespace RackCad.Plugin.Systems.PushBack
{
    /// <summary>
    /// AutoCAD orchestration for one Push Back FRONTAL cut: a thin adapter over <see cref="ViewBlockDraw"/> that draws the
    /// plan the Application <see cref="PushBackSystemFrontalBuilder"/> produced for the requested <see cref="PushBackFrontalEnd"/>
    /// (the low entrance/exit cut, or the rear posterior cut). No geometry is recomputed here.
    /// </summary>
    public sealed class PushBackFrontalDrawService
    {
        private readonly PushBackSystemFrontalBuilder builder = new PushBackSystemFrontalBuilder();
        private readonly LateralHeaderDrawer drawer = new LateralHeaderDrawer();

        public HeaderPlacementResult DrawAndPlace(
            Document document,
            PushBackSystem system,
            PushBackFrontalEnd end,
            string payloadJson = null,
            string rackName = null,
            PushBackSide side = PushBackSide.A)
            => ViewBlockDraw.DrawAndPlace(
                document,
                system != null,
                "No hay sistema Push Back para dibujar.",
                drawer,
                catalog => builder.BuildPlan(system, catalog, end, side),
                () => BlockName(system, rackName, end, side),
                payloadJson);

        public HeaderPlacementResult RedrawInPlace(
            Document document,
            ObjectId blockId,
            PushBackSystem system,
            PushBackFrontalEnd end,
            string payloadJson,
            bool regen = true,
            PushBackSide side = PushBackSide.A)
            => ViewBlockDraw.RedrawInPlace(
                document,
                blockId,
                system != null && !blockId.IsNull,
                "No hay sistema Push Back para actualizar.",
                drawer,
                catalog => builder.BuildPlan(system, catalog, end, side),
                payloadJson,
                regen);

        /// <summary>
        /// I-42: el nombre del bloque lleva el LADO cuando el rack tiene dos, para que los cuatro cortes frontales de
        /// un mismo rack no colisionen en el mismo nombre. Un rack de un solo sentido conserva el nombre historico.
        /// </summary>
        internal static string BlockName(
            PushBackSystem system, string rackName, PushBackFrontalEnd end, PushBackSide side = PushBackSide.A)
            => RackViewBaseName.PushBackFrontal(
                system,
                rackName,
                end == PushBackFrontalEnd.Posterior
                    ? RackPushBackEnd.Posterior
                    : RackPushBackEnd.EntradaSalida,
                side == PushBackSide.B ? RackPushBackSide.B : RackPushBackSide.A);
    }
}
