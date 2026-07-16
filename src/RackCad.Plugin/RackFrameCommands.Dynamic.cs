using System.Globalization;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Runtime;
using RackCad.Application.Catalogs;
using RackCad.Application.Persistence;
using RackCad.Application.RackFrames;
using RackCad.Application.Systems;
using RackCad.Domain.Systems;
using RackCad.Plugin.Headers;
using RackCad.Plugin.Systems;
using RackCad.UI;
using AcApplication = Autodesk.AutoCAD.ApplicationServices.Application;

namespace RackCad.Plugin
{
    /// <summary>Dynamic (pallet flow) system commands + their draw/edit/payload helpers.</summary>
    public sealed partial class RackFrameCommands
    {
        /// <summary>
        /// Draws the current lateral representation of a dynamic (pallet flow) system as one block placed with the
        /// mouse. Editable inputs are embedded separately from resolved geometry for deterministic reopen/edit.
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
                // Demo command: pallet/depth stay illustrative, but post + header height honor defaults.json.
                var catalog = LateralHeaderDrawService.LoadCatalog();
                var pallet = new PalletSpecification(42.0, 48.0, 60.0, 1000.0, "kg");
                var postId = !string.IsNullOrWhiteSpace(catalog?.Defaults?.Post) ? catalog.Defaults.Post : CatalogIds.StandardPost;
                var headerHeight = catalog?.Defaults?.DefaultHeaderHeight > 0.0 ? catalog.Defaults.DefaultHeaderHeight : 132.0;
                var system = new DynamicRackSystemBuilder(catalog).BuildDefault(
                    pallet,
                    palletsDeep: 8,
                    headerTemplate: RackFrameTemplateCatalog.Default,
                    headerPostCatalogId: postId,
                    headerHeight: headerHeight);
                system.ManualHeaderHeightOverride = headerHeight;

                var design = new DynamicRackSystemResolver(catalog).Snapshot(
                    system,
                    DynamicRackDefaults.DefaultLoadLevels,
                    DynamicRackDefaults.DefaultFirstLevelHeight,
                    DynamicRackDefaults.DefaultBeamDepth,
                    postId);
                var payload = BuildDynamicPayload(design, System.Guid.NewGuid().ToString(), null);
                var result = new DynamicSystemDrawService().DrawAndPlace(document, system, payload);
                document.Editor.WriteMessage("\n" + DescribeSystem(result));
            }
            catch (System.Exception ex)
            {
                Report(ex);
            }
        }

        /// <summary>Builds the dynamic-system block and runs the placement jig, then reports the outcome.</summary>
        private static void DrawAndPlaceSystem(DynamicRackSystem system, string payloadJson, string rackName)
        {
            var document = AcApplication.DocumentManager.MdiActiveDocument;

            if (document == null || system == null)
            {
                return;
            }

            var result = new DynamicSystemDrawService().DrawAndPlace(document, system, payloadJson, rackName);
            document.Editor.WriteMessage("\n" + DescribeSystem(result));
        }

        /// <summary>Summary for a dynamic-system insert (shared shape in <see cref="DescribePlacement"/>).</summary>
        private static string DescribeSystem(HeaderPlacementResult result)
            => DescribePlacement(result, "el sistema", "sistema insertado");

        private static void EditDynamic(Document document, ObjectId blockId, RackEmbedDocument embed)
        {
            var editor = document.Editor;

            RackProject project;
            try
            {
                project = new RackProjectStore().Deserialize(embed.Design);
            }
            catch (System.Exception ex)
            {
                editor.WriteMessage("\nRackCad: no se pudieron leer los datos del sistema. " + ex.Message);
                return;
            }

            if (project?.DynamicDesign == null)
            {
                editor.WriteMessage("\nRackCad: datos de sistema dinamico invalidos.");
                return;
            }

            var window = new RackDynamicSystemWindow(canInsertInAutoCad: true);
            window.LoadExisting(project.DynamicDesign, embed.Id, embed.Name);
            AcApplication.ShowModalWindow(window);

            if (!window.InsertRequested)
            {
                return;
            }

            var result = new DynamicSystemDrawService().RedrawInPlace(
                document, blockId, window.SystemToInsert,
                BuildDynamicPayload(window.DesignToInsert, window.RackId, window.RackName));

            if (result != null && result.Success)
            {
                RackBlockRenamer.SyncName(document, blockId, string.IsNullOrWhiteSpace(window.RackName) ? null : window.RackName.Trim());
            }

            editor.WriteMessage(result != null && result.Success
                ? "\nRackCad: sistema actualizado; todas sus copias reflejan el cambio."
                : "\nRackCad: no se pudo actualizar el sistema. " + (result?.ErrorMessage ?? string.Empty));
        }

        /// <summary>Wraps a dynamic system in the uniform embed envelope; reuses the project store for the design JSON.</summary>
        private static string BuildDynamicPayload(DynamicRackDesign design, string id, string name)
        {
            if (design == null)
            {
                return null;
            }

            var designJson = new RackProjectStore().Serialize(RackProject.ForDynamic(design));
            return new RackEmbedStore().Serialize(new RackEmbedDocument
            {
                Kind = RackEmbedDocument.KindDynamic,
                Id = id,
                Name = name,
                Design = designJson
            });
        }
    }
}
