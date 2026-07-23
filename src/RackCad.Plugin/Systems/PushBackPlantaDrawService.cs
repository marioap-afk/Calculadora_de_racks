using System.Globalization;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using RackCad.Application.Systems;
using RackCad.Domain.Systems;
using RackCad.Plugin.Headers;

namespace RackCad.Plugin.Systems
{
    /// <summary>
    /// AutoCAD orchestration for the linked TOP view of a Push Back system: a thin adapter over <see cref="ViewBlockDraw"/>
    /// that draws the plan the Application <see cref="PushBackSystemPlantaBuilder"/> produced. No geometry is recomputed here.
    /// </summary>
    public sealed class PushBackPlantaDrawService
    {
        private readonly PushBackSystemPlantaBuilder builder = new PushBackSystemPlantaBuilder();
        private readonly LateralHeaderDrawer drawer = new LateralHeaderDrawer();

        public HeaderPlacementResult DrawAndPlace(
            Document document,
            PushBackSystem system,
            string payloadJson = null,
            string rackName = null)
            => ViewBlockDraw.DrawAndPlace(
                document,
                system != null,
                "No hay sistema Push Back para dibujar.",
                drawer,
                catalog => builder.BuildPlan(system, catalog),
                () => BlockName(system, rackName),
                payloadJson);

        public HeaderPlacementResult RedrawInPlace(
            Document document,
            ObjectId blockId,
            PushBackSystem system,
            string payloadJson,
            bool regen = true)
            => ViewBlockDraw.RedrawInPlace(
                document,
                blockId,
                system != null && !blockId.IsNull,
                "No hay sistema Push Back para actualizar.",
                drawer,
                catalog => builder.BuildPlan(system, catalog),
                payloadJson,
                regen);

        internal static string BlockName(PushBackSystem system, string rackName)
        {
            if (!string.IsNullOrWhiteSpace(rackName))
            {
                return rackName.Trim() + " - planta";
            }

            return string.Format(CultureInfo.InvariantCulture, "Push Back planta - {0} frentes", system.Fronts?.Count ?? 0);
        }
    }
}
