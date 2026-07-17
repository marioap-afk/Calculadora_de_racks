using System.Globalization;
using System.Linq;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
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
                var resolver = new DynamicRackSystemResolver(catalog);
                var initialDesign = new DynamicRackDesign
                {
                    Pallet = pallet,
                    PalletsDeep = 8,
                    LoadLevels = DynamicRackDefaults.DefaultLoadLevels,
                    FirstLevelHeight = DynamicRackDefaults.DefaultFirstLevelHeight,
                    BeamDepth = DynamicRackDefaults.DefaultBeamDepth,
                    InOutBeamCatalogId = DynamicRackDefaults.InOutBeamCatalogId,
                    HeaderPostCatalogId = postId,
                    ManualHeaderHeightOverride = headerHeight
                };
                var system = resolver.Resolve(initialDesign).System;
                var design = resolver.Snapshot(
                    system,
                    DynamicRackDefaults.DefaultLoadLevels,
                    DynamicRackDefaults.DefaultFirstLevelHeight,
                    DynamicRackDefaults.DefaultBeamDepth,
                    postId);
                DrawDynamicView(
                    RackEmbedDocument.ViewLateral,
                    -1,
                    system,
                    design,
                    System.Guid.NewGuid().ToString(),
                    null);
            }
            catch (System.Exception ex)
            {
                Report(ex);
            }
        }

        /// <summary>Builds the requested linked view and runs its placement jig.</summary>
        private static void DrawDynamicView(
            string view,
            int section,
            DynamicRackSystem system,
            DynamicRackDesign design,
            string id,
            string rackName)
        {
            var document = AcApplication.DocumentManager.MdiActiveDocument;

            if (document == null || system == null)
            {
                return;
            }

            system.Name = rackName;

            if (string.Equals(view, RackEmbedDocument.ViewLateral, System.StringComparison.OrdinalIgnoreCase)
                && section < 0)
            {
                InsertDynamicLateralSection(document, system, design, id, rackName);
                return;
            }

            HeaderPlacementResult result;
            var payload = BuildDynamicPayload(design, id, rackName, view, section);
            if (string.Equals(view, RackEmbedDocument.ViewPlanta, System.StringComparison.OrdinalIgnoreCase))
            {
                result = new DynamicPlantaDrawService().DrawAndPlace(document, system, payload, rackName);
            }
            else if (string.Equals(view, RackEmbedDocument.ViewFrontal, System.StringComparison.OrdinalIgnoreCase))
            {
                result = new DynamicFrontalDrawService().DrawAndPlace(
                    document,
                    system,
                    DynamicEnd(section),
                    payload,
                    rackName);
            }
            else
            {
                result = new DynamicSystemDrawService().DrawAndPlace(
                    document,
                    system,
                    payload,
                    rackName,
                    section);
            }

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
            window.SetDimensionStyles(ReadDimensionStyleNames(document));
            window.LoadExisting(project.DynamicDesign, embed.Id, embed.Name);
            AcApplication.ShowModalWindow(window);

            if (!window.InsertRequested)
            {
                return;
            }

            var design = window.DesignToInsert;
            var system = window.SystemToInsert;
            var id = string.IsNullOrWhiteSpace(embed.Id) ? window.RackId : embed.Id;
            var name = string.IsNullOrWhiteSpace(window.RackName) ? embed.Name : window.RackName;
            var baseName = string.IsNullOrWhiteSpace(name) ? null : name.Trim();
            system.Name = name;
            var blocks = FindRackBlocks(document, id);
            if (blocks.Count == 0)
            {
                blocks.Add((blockId, embed));
            }

            var updatedLateral = 0;
            var updatedFrontal = 0;
            var updatedPlanta = 0;
            var staleViewBlocks = new System.Collections.Generic.List<ObjectId>();
            var catalog = LateralHeaderDrawService.LoadCatalog();
            var lateralCortes = new DynamicSystemLateralBuilder().Cortes(system, catalog);
            foreach (var viewBlock in blocks)
            {
                HeaderPlacementResult result;
                if (IsPlantaView(viewBlock.Embed))
                {
                    var payload = BuildDynamicPayload(design, id, name, RackEmbedDocument.ViewPlanta, -1);
                    result = new DynamicPlantaDrawService().RedrawInPlace(
                        document, viewBlock.BlockId, system, payload, regen: false);
                    if (result != null && result.Success)
                    {
                        RackBlockRenamer.SyncName(document, viewBlock.BlockId, baseName == null ? null : baseName + " - planta");
                        updatedPlanta++;
                    }
                }
                else if (IsDynamicFrontal(viewBlock.Embed))
                {
                    var end = DynamicEnd(viewBlock.Embed.Section);
                    var section = (int)end;
                    var payload = BuildDynamicPayload(design, id, name, RackEmbedDocument.ViewFrontal, section);
                    result = new DynamicFrontalDrawService().RedrawInPlace(
                        document, viewBlock.BlockId, system, end, payload, regen: false);
                    if (result != null && result.Success)
                    {
                        var suffix = end == DynamicRackEnd.Entrance ? " - frontal entrada" : " - frontal salida";
                        RackBlockRenamer.SyncName(document, viewBlock.BlockId, baseName == null ? null : baseName + suffix);
                        updatedFrontal++;
                    }
                }
                else
                {
                    // Legacy dynamic embeds did not carry View/Section; they become the first post's linked cut.
                    var postIndex = viewBlock.Embed != null && viewBlock.Embed.Section >= 0
                        ? viewBlock.Embed.Section
                        : 0;
                    var corte = lateralCortes.FirstOrDefault(item => item.PostIndex == postIndex);
                    if (corte == null)
                    {
                        staleViewBlocks.Add(viewBlock.BlockId);
                        continue;
                    }

                    var payload = BuildDynamicPayload(design, id, name, RackEmbedDocument.ViewLateral, postIndex);
                    result = new DynamicSystemDrawService().RedrawInPlace(
                        document,
                        viewBlock.BlockId,
                        system,
                        payload,
                        regen: false,
                        postIndex: postIndex);
                    if (result != null && result.Success)
                    {
                        RackBlockRenamer.SyncName(
                            document,
                            viewBlock.BlockId,
                            baseName == null
                                ? null
                                : baseName + " - lateral " + (postIndex + 1).ToString(CultureInfo.InvariantCulture));
                        updatedLateral++;
                    }
                }
            }

            var survivors = blocks.Count - staleViewBlocks.Count;
            var erasedPhantoms = staleViewBlocks.Count > 0 && survivors > 0
                ? EraseViewBlocks(document, staleViewBlocks)
                : 0;

            if (updatedLateral + updatedFrontal + updatedPlanta + erasedPhantoms > 0)
            {
                document.Editor.Regen();
            }

            if (!window.UpdateOnly)
            {
                DrawDynamicView(window.InsertView, window.InsertSection, system, design, id, name);
                return;
            }

            editor.WriteMessage(updatedLateral + updatedFrontal + updatedPlanta > 0
                ? "\nRackCad: sistema actualizado; vistas redibujadas (lateral x"
                    + updatedLateral.ToString(CultureInfo.InvariantCulture) + ", frontal x"
                    + updatedFrontal.ToString(CultureInfo.InvariantCulture) + ", planta x"
                    + updatedPlanta.ToString(CultureInfo.InvariantCulture) + ")."
                : "\nRackCad: no se pudo actualizar el sistema.");
        }

        /// <summary>Prompts for one transverse post and inserts its linked lateral section.</summary>
        private static void InsertDynamicLateralSection(
            Document document,
            DynamicRackSystem system,
            DynamicRackDesign design,
            string id,
            string name)
        {
            if (document == null || system == null)
            {
                return;
            }

            var editor = document.Editor;
            var cortes = new DynamicSystemLateralBuilder().Cortes(
                system,
                LateralHeaderDrawService.LoadCatalog());
            if (cortes.Count == 0)
            {
                editor.WriteMessage("\nRackCad: no hay cortes laterales que dibujar.");
                return;
            }

            var options = new PromptIntegerOptions("\nQue corte lateral insertar (numero de poste)?")
            {
                LowerLimit = 1,
                UpperLimit = cortes.Max(corte => corte.PostIndex) + 1,
                DefaultValue = 1,
                UseDefaultValue = true,
                AllowNone = false
            };
            var pick = editor.GetInteger(options);
            if (pick.Status != PromptStatus.OK)
            {
                return;
            }

            var corte = cortes.FirstOrDefault(item => item.PostIndex == pick.Value - 1);
            if (corte == null)
            {
                editor.WriteMessage("\nRackCad: el poste " + pick.Value.ToString(CultureInfo.InvariantCulture) + " no tiene corte lateral.");
                return;
            }

            var baseName = string.IsNullOrWhiteSpace(name) ? "Dinamico" : name.Trim();
            var sectionName = baseName + " - lateral " + pick.Value.ToString(CultureInfo.InvariantCulture);
            var payload = BuildDynamicPayload(
                design,
                id,
                name,
                RackEmbedDocument.ViewLateral,
                corte.PostIndex);
            var result = new DynamicSystemDrawService().DrawAndPlace(
                document,
                system,
                payload,
                sectionName,
                corte.PostIndex);
            editor.WriteMessage(result != null && result.Success
                ? "\nRackCad: corte lateral del poste " + pick.Value.ToString(CultureInfo.InvariantCulture) + " insertado y ligado al sistema."
                : "\nRackCad: no se pudo insertar el corte lateral. " + (result?.ErrorMessage ?? string.Empty));
        }

        /// <summary>Wraps a dynamic system in the uniform embed envelope; reuses the project store for the design JSON.</summary>
        private static string BuildDynamicPayload(
            DynamicRackDesign design,
            string id,
            string name,
            string view = RackEmbedDocument.ViewLateral,
            int section = -1)
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
                View = string.IsNullOrWhiteSpace(view) ? RackEmbedDocument.ViewLateral : view,
                Section = section,
                Design = designJson
            });
        }

        private static bool IsDynamicFrontal(RackEmbedDocument embed)
            => embed != null && string.Equals(
                embed.View,
                RackEmbedDocument.ViewFrontal,
                System.StringComparison.OrdinalIgnoreCase);

        private static DynamicRackEnd DynamicEnd(int section)
            => section == (int)DynamicRackEnd.Entrance ? DynamicRackEnd.Entrance : DynamicRackEnd.Exit;
    }
}
