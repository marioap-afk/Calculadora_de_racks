using System.Globalization;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using RackCad.Application.Systems;
using RackCad.Domain.Systems;
using RackCad.Plugin.Headers;

namespace RackCad.Plugin.Systems
{
    /// <summary>
    /// AutoCAD-side orchestration for a Push Back LATERAL view: a thin adapter over <see cref="ViewBlockDraw"/> that draws
    /// the plan the Application <see cref="PushBackSystemLateralBuilder"/> already produced (the full lateral, or one corte
    /// when <c>postIndex &gt;= 0</c>). No slope, bed, tope, larguero or safety formula lives here — the SystemPlan is consumed
    /// verbatim and the regen stays inside <see cref="ViewBlockDraw"/>.
    /// </summary>
    public sealed class PushBackSystemDrawService
    {
        private readonly PushBackSystemLateralBuilder builder = new PushBackSystemLateralBuilder();
        private readonly LateralHeaderDrawer drawer = new LateralHeaderDrawer();

        /// <summary>Draws + places the Push Back lateral block, embedding <paramref name="payloadJson"/> on the definition so
        /// every copy shares it. A null system surfaces the visible error, exactly like the dynamic service.</summary>
        public HeaderPlacementResult DrawAndPlace(
            Document document,
            PushBackSystem system,
            string payloadJson = null,
            string rackName = null,
            int postIndex = -1)
            => ViewBlockDraw.DrawAndPlace(
                document,
                system != null,
                "No hay sistema Push Back para dibujar.",
                drawer,
                catalog => postIndex >= 0 ? builder.Build(system, catalog, postIndex) : builder.Build(system, catalog),
                () => BlockName(system, rackName, postIndex),
                payloadJson);

        /// <summary>Redraw an existing Push Back lateral block DEFINITION in place; every copy updates on regen.</summary>
        public HeaderPlacementResult RedrawInPlace(
            Document document,
            ObjectId blockId,
            PushBackSystem system,
            string payloadJson,
            bool regen = true,
            int postIndex = -1)
            => ViewBlockDraw.RedrawInPlace(
                document,
                blockId,
                system != null && !blockId.IsNull,
                "No hay sistema Push Back para actualizar.",
                drawer,
                catalog => postIndex >= 0 ? builder.Build(system, catalog, postIndex) : builder.Build(system, catalog),
                payloadJson,
                regen);

        /// <summary>
        /// The SINGLE authority for a Push Back lateral block name: with a name → "&lt;name&gt; - lateral N" (N = postIndex + 1);
        /// without a name → a stable descriptive name. Callers pass the BASE rack name (never a pre-sectioned one), so the
        /// "- lateral N" suffix is added EXACTLY ONCE, here.
        /// </summary>
        internal static string BlockName(PushBackSystem system, string rackName, int postIndex)
        {
            var section = postIndex >= 0
                ? " - lateral " + (postIndex + 1).ToString(CultureInfo.InvariantCulture)
                : string.Empty;
            if (!string.IsNullOrWhiteSpace(rackName))
            {
                return rackName.Trim() + section;
            }

            return string.Format(CultureInfo.InvariantCulture, "Push Back{0} - {1} frentes", section, system.Fronts?.Count ?? 0);
        }
    }
}
