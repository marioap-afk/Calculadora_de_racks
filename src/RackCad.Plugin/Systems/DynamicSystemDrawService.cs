using System;
using System.Globalization;
using Autodesk.AutoCAD.ApplicationServices;
using RackCad.Application.Systems;
using RackCad.Domain.Systems;
using RackCad.Plugin.Headers;

namespace RackCad.Plugin.Systems
{
    /// <summary>
    /// AutoCAD-side orchestration for drawing a whole dynamic (pallet flow) system: builds the pure plan
    /// (headers along the run + separators per level) and reuses the header placer to turn it into a single
    /// block the user drops with the mouse. The geometry stays in the Application layer.
    /// </summary>
    public sealed class DynamicSystemDrawService
    {
        private readonly DynamicSystemLateralBuilder builder = new DynamicSystemLateralBuilder();

        public HeaderPlacementResult DrawAndPlace(Document document, DynamicRackSystem system)
        {
            if (document == null)
            {
                return HeaderPlacementResult.Failure("No hay un dibujo activo en AutoCAD.");
            }

            if (system == null)
            {
                return HeaderPlacementResult.Failure("No hay sistema para dibujar.");
            }

            try
            {
                var catalog = LateralHeaderDrawService.LoadCatalog();
                var layout = builder.Build(system, catalog);
                var blockName = string.Format(
                    CultureInfo.InvariantCulture,
                    "Sistema dinamico - {0} fondos - L{1:0.##}",
                    system.PalletsDeep,
                    system.TotalLength);

                return new LateralHeaderDrawService().PlaceLayout(document, catalog, layout, blockName);
            }
            catch (Exception ex)
            {
                return HeaderPlacementResult.Failure(ex.Message);
            }
        }
    }
}
