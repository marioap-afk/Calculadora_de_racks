using System.Globalization;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using RackCad.Application.Systems;
using RackCad.Domain.Systems;
using RackCad.Plugin.Headers;

namespace RackCad.Plugin.Systems
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
            string rackName = null)
            => ViewBlockDraw.DrawAndPlace(
                document,
                system != null,
                "No hay sistema Push Back para dibujar.",
                drawer,
                catalog => builder.BuildPlan(system, catalog, end),
                () => BlockName(system, rackName, end),
                payloadJson);

        public HeaderPlacementResult RedrawInPlace(
            Document document,
            ObjectId blockId,
            PushBackSystem system,
            PushBackFrontalEnd end,
            string payloadJson,
            bool regen = true)
            => ViewBlockDraw.RedrawInPlace(
                document,
                blockId,
                system != null && !blockId.IsNull,
                "No hay sistema Push Back para actualizar.",
                drawer,
                catalog => builder.BuildPlan(system, catalog, end),
                payloadJson,
                regen);

        internal static string BlockName(PushBackSystem system, string rackName, PushBackFrontalEnd end)
        {
            var suffix = end == PushBackFrontalEnd.Posterior ? "frontal posterior" : "frontal entrada-salida";
            if (!string.IsNullOrWhiteSpace(rackName))
            {
                return rackName.Trim() + " - " + suffix;
            }

            return string.Format(CultureInfo.InvariantCulture, "Push Back {0} - {1} frentes", suffix, system.Fronts?.Count ?? 0);
        }
    }
}
