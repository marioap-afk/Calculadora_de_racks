using System;
using System.Collections.Generic;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using RackCad.Application.Headers;
using RackCad.Application.Systems;
using RackCad.Domain.RackFrames;
using RackCad.Plugin.Systems;

namespace RackCad.Plugin.Headers
{
    /// <summary>
    /// Draws the PLANTA (top-down) view of a cabecera — the post footprints, plates and the collapsed celosía — as a
    /// single block, jig-placed, with the rack payload embedded on the block DEFINITION so every copy shares it and
    /// RACKEDITAR can reopen it. Mirrors <c>SelectiveFrontalDrawService</c> (loose instances → plan → block).
    /// </summary>
    public sealed class PlantaHeaderDrawService
    {
        private readonly PlantaHeaderLayoutBuilder builder = new PlantaHeaderLayoutBuilder();
        private readonly LateralHeaderDrawer drawer = new LateralHeaderDrawer();

        public HeaderPlacementResult DrawAndPlace(Document document, RackFrameConfiguration config, string payloadJson = null, string rackName = null)
        {
            if (document == null)
            {
                return HeaderPlacementResult.Failure("No hay un dibujo activo en AutoCAD.");
            }

            if (config == null)
            {
                return HeaderPlacementResult.Failure("No hay configuracion para dibujar.");
            }

            try
            {
                var catalog = LateralHeaderDrawService.LoadCatalog();
                var plan = new DynamicSystemPlan(new List<HeaderGroup>(), builder.Build(config, catalog));
                var block = CreateBlock(document, plan, BlockName(rackName), payloadJson);
                return new LateralHeaderDrawService().PlaceAndReport(document, catalog, block);
            }
            catch (Exception ex)
            {
                return HeaderPlacementResult.Failure(ex.Message);
            }
        }

        /// <summary>Redraw an existing planta block DEFINITION in place (all copies update on regen).</summary>
        public HeaderPlacementResult RedrawInPlace(Document document, ObjectId blockId, RackFrameConfiguration config, string payloadJson)
        {
            if (document == null)
            {
                return HeaderPlacementResult.Failure("No hay un dibujo activo en AutoCAD.");
            }

            if (config == null || blockId.IsNull)
            {
                return HeaderPlacementResult.Failure("No hay cabecera para actualizar.");
            }

            try
            {
                var catalog = LateralHeaderDrawService.LoadCatalog();
                var plan = new DynamicSystemPlan(new List<HeaderGroup>(), builder.Build(config, catalog));
                var database = document.Database;

                LateralHeaderDrawOutcome outcome;
                using (document.LockDocument())
                {
                    BlockLibraryImporter.EnsureForPlan(database, plan);

                    using (var transaction = database.TransactionManager.StartTransaction())
                    {
                        outcome = drawer.RedefineSystemBlock(database, transaction, blockId, plan);
                        RackBlockData.Write(transaction, blockId, payloadJson);
                        transaction.Commit();
                    }

                    document.Editor.Regen();
                }

                return new HeaderPlacementResult(true, true, null, LateralHeaderDrawService.DescribeMissing(catalog, outcome), outcome);
            }
            catch (Exception ex)
            {
                return HeaderPlacementResult.Failure(ex.Message);
            }
        }

        private LateralHeaderBlockResult CreateBlock(Document document, DynamicSystemPlan plan, string blockName, string payloadJson)
        {
            var database = document.Database;

            using (document.LockDocument())
            {
                BlockLibraryImporter.EnsureForPlan(database, plan);

                using (var transaction = database.TransactionManager.StartTransaction())
                {
                    var result = drawer.CreateSystemBlock(database, transaction, plan, blockName);

                    if (!string.IsNullOrEmpty(payloadJson))
                    {
                        RackBlockData.Write(transaction, result.DefinitionId, payloadJson);
                    }

                    transaction.Commit();
                    return result;
                }
            }
        }

        private static string BlockName(string rackName)
            => string.IsNullOrWhiteSpace(rackName) ? "Cabecera planta" : rackName.Trim() + " - planta";
    }
}
