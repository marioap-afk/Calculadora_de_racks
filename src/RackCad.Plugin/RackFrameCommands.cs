using System.Globalization;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.Runtime;
using RackCad.Application.RackFrames;
using RackCad.Application.Systems;
using RackCad.Domain.RackFrames;
using RackCad.Domain.Systems;
using RackCad.Plugin.Headers;
using RackCad.Plugin.Systems;
using RackCad.UI;
using AcApplication = Autodesk.AutoCAD.ApplicationServices.Application;

namespace RackCad.Plugin
{
    public sealed class RackFrameCommands
    {
        /// <summary>Main entry point: opens the menu where the user picks what to design.</summary>
        [CommandMethod("RACKCAD")]
        public void RackCad()
        {
            try
            {
                var menu = new RackMainMenuWindow(canInsertInAutoCad: true);
                AcApplication.ShowModalWindow(menu);

                if (menu.InsertRequested)
                {
                    DrawAndPlace(menu.ConfigurationToInsert);
                }
            }
            catch (System.Exception ex)
            {
                Report(ex);
            }
        }

        /// <summary>Shortcut straight to the header configurator module.</summary>
        [CommandMethod("RACKCABECERA")]
        public void RackCabecera()
        {
            try
            {
                var configuration = new HardcodedStandardRackFrameService().CreateDefault();
                var window = new RackFrameConfiguratorWindow(configuration, canInsertInAutoCad: true);
                AcApplication.ShowModalWindow(window);

                if (window.InsertRequested)
                {
                    DrawAndPlace(window.Configuration);
                }
            }
            catch (System.Exception ex)
            {
                Report(ex);
            }
        }

        /// <summary>
        /// Draws the standard lateral header straight into the drawing (creates the block and lets the user
        /// place it). Quick path without the configurator; the editor's "Insertar en AutoCAD" button draws
        /// the edited header instead.
        /// </summary>
        [CommandMethod("RACKCABECERALATERAL")]
        public void RackCabeceraLateral()
        {
            try
            {
                DrawAndPlace(new HardcodedStandardRackFrameService().CreateDefault());
            }
            catch (System.Exception ex)
            {
                Report(ex);
            }
        }

        /// <summary>
        /// Draws a preliminary dynamic (pallet flow) system: headers along the run + separators per level,
        /// as one block placed with the mouse. Uses default pallet/height for now (header-height logic TBD).
        /// </summary>
        [CommandMethod("RACKSISTEMADINAMICO")]
        public void RackSistemaDinamico()
        {
            var document = AcApplication.DocumentManager.MdiActiveDocument;

            if (document == null)
            {
                return;
            }

            try
            {
                var catalog = LateralHeaderDrawService.LoadCatalog();
                var pallet = new PalletSpecification(42.0, 48.0, 60.0, 1000.0, "kg");
                var system = new DynamicRackSystemBuilder(catalog).BuildDefault(
                    pallet,
                    palletsDeep: 8,
                    headerTemplate: RackFrameTemplateCatalog.Default,
                    headerPostCatalogId: CatalogIds.StandardPost,
                    headerHeight: 132.0);

                var result = new DynamicSystemDrawService().DrawAndPlace(document, system);
                document.Editor.WriteMessage("\n" + Describe(result));
            }
            catch (System.Exception ex)
            {
                Report(ex);
            }
        }

        /// <summary>Builds the header block and runs the placement jig, then reports the outcome.</summary>
        private static void DrawAndPlace(RackFrameConfiguration configuration)
        {
            var document = AcApplication.DocumentManager.MdiActiveDocument;

            if (document == null || configuration == null)
            {
                return;
            }

            var result = new LateralHeaderDrawService().DrawAndPlace(document, configuration);
            document.Editor.WriteMessage("\n" + Describe(result));
        }

        private static string Describe(HeaderPlacementResult result)
        {
            if (!result.Success)
            {
                return "RackCad: no se pudo dibujar la cabecera lateral. " + result.ErrorMessage;
            }

            if (!result.Placed)
            {
                return "RackCad: bloque '" + result.BlockName + "' creado, pero la insercion se cancelo.";
            }

            var outcome = result.Outcome;
            var summary = string.Format(
                CultureInfo.InvariantCulture,
                "RackCad: cabecera insertada como bloque '{0}'. {1} piezas ({2} horizontales, {3} diagonales).",
                result.BlockName,
                outcome.InsertedCount,
                outcome.Layout.HorizontalCount,
                outcome.Layout.DiagonalCount);

            if (result.HasMissingBlocks)
            {
                summary += "\nBloques no definidos en el dibujo (omitidos): " + string.Join(", ", result.MissingBlocks);
            }

            return summary;
        }

        private static void Report(System.Exception ex)
        {
            var document = AcApplication.DocumentManager.MdiActiveDocument;
            document?.Editor.WriteMessage("\nRackCad error: " + ex.Message);
        }
    }
}
