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
using RackCad.Domain.RackFrames;
using RackCad.Domain.Systems;
using RackCad.Plugin.Headers;
using RackCad.Plugin.Systems;
using RackCad.UI;
using AcApplication = Autodesk.AutoCAD.ApplicationServices.Application;

namespace RackCad.Plugin
{
    /// <summary>Cabecera (lateral header) commands + their draw/edit/payload helpers.</summary>
    public sealed partial class RackFrameCommands
    {
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
        /// Quick lateral header straight from the command line: prompts for post, depth and height, then
        /// builds the header and lets the user place it. Same no-UI style as QUICKCAMA.
        /// </summary>
        [CommandMethod("QUICKCABECERA")]
        public void QuickCabecera()
        {
            var document = AcApplication.DocumentManager.MdiActiveDocument;

            if (document == null)
            {
                return;
            }

            var editor = document.Editor;

            try
            {
                var catalog = LateralHeaderDrawService.LoadCatalog();

                var postId = PromptPostId(editor, catalog);
                if (postId == null)
                {
                    return;
                }

                // Prompt defaults come from defaults.json (the user-editable standard recipe), not literals.
                var depthOptions = new PromptDistanceOptions("\nFondo (in)")
                {
                    DefaultValue = SelectiveRackDefaults.DefaultPalletDepth,
                    UseDefaultValue = true,
                    AllowNegative = false,
                    AllowZero = false
                };
                var depthResult = editor.GetDistance(depthOptions);
                if (depthResult.Status != PromptStatus.OK)
                {
                    return;
                }

                var heightOptions = new PromptDistanceOptions("\nAlto (in)")
                {
                    DefaultValue = catalog?.Defaults?.DefaultHeaderHeight > 0.0 ? catalog.Defaults.DefaultHeaderHeight : 132.0,
                    UseDefaultValue = true,
                    AllowNegative = false,
                    AllowZero = false
                };
                var heightResult = editor.GetDistance(heightOptions);
                if (heightResult.Status != PromptStatus.OK)
                {
                    return;
                }

                var configuration = new DynamicRackSystemBuilder(catalog)
                    .BuildHeaderConfiguration(RackFrameTemplateCatalog.Default, postId, heightResult.Value, depthResult.Value);

                DrawAndPlace(configuration);
            }
            catch (System.Exception ex)
            {
                Report(ex);
            }
        }

        /// <summary>
        /// Prompts for the post: uses the only one if there is a single post in the catalog, otherwise prints a
        /// numbered list and asks for the index. Returns the post id, or null if the user cancelled.
        /// </summary>
        private static string PromptPostId(Editor editor, RackCatalog catalog)
        {
            var posts = catalog.PostProfiles
                .Where(p => p != null && !string.IsNullOrWhiteSpace(p.Id))
                .GroupBy(p => p.Id, System.StringComparer.OrdinalIgnoreCase)
                .Select(g => g.First())
                .ToList();

            if (posts.Count == 0)
            {
                // Empty catalog: fall back to the defaults.json post first, then the built-in standard.
                return !string.IsNullOrWhiteSpace(catalog?.Defaults?.Post) ? catalog.Defaults.Post : CatalogIds.StandardPost;
            }

            if (posts.Count == 1)
            {
                editor.WriteMessage("\nPoste: " + posts[0].Label);
                return posts[0].Id;
            }

            for (var i = 0; i < posts.Count; i++)
            {
                editor.WriteMessage(string.Format(CultureInfo.InvariantCulture, "\n  {0}: {1}", i + 1, posts[i].Label));
            }

            var options = new PromptIntegerOptions("\nPoste #")
            {
                DefaultValue = 1,
                UseDefaultValue = true,
                LowerLimit = 1,
                UpperLimit = posts.Count
            };
            var result = editor.GetInteger(options);
            if (result.Status != PromptStatus.OK)
            {
                return null;
            }

            return posts[result.Value - 1].Id;
        }

        /// <summary>Builds the header block and runs the placement jig, then reports the outcome.</summary>
        private static void DrawAndPlace(RackFrameConfiguration configuration)
        {
            var document = AcApplication.DocumentManager.MdiActiveDocument;

            if (document == null || configuration == null)
            {
                return;
            }

            var payload = BuildCabeceraPayload(configuration, System.Guid.NewGuid().ToString(), configuration.Name);
            var result = new LateralHeaderDrawService().DrawAndPlace(document, configuration, payload, configuration.Name);
            document.Editor.WriteMessage("\n" + Describe(result));
        }

        /// <summary>Wraps a cabecera (RackFrameConfiguration) in the uniform embed envelope; reuses the project store.
        /// <paramref name="view"/> tags which view this block draws (lateral default, or planta) so a cabecera can have
        /// several view-blocks sharing its id — the same multi-view round-trip as the selective.</summary>
        private static string BuildCabeceraPayload(RackFrameConfiguration configuration, string id, string name, string view = null)
        {
            if (configuration == null)
            {
                return null;
            }

            var designJson = new RackProjectStore().Serialize(RackProject.ForSelective(configuration));
            return new RackEmbedStore().Serialize(new RackEmbedDocument
            {
                Kind = RackEmbedDocument.KindCabecera,
                Id = id,
                Name = name,
                View = string.IsNullOrWhiteSpace(view) ? RackEmbedDocument.ViewLateral : view,
                Design = designJson
            });
        }

        /// <summary>Duplicate a cabecera as an independent copy (new GUID/name), drawn in the clicked view (lateral/planta).</summary>
        private static void DuplicateCabecera(Document document, RackEmbedDocument embed, string newId, string newName)
        {
            var editor = document.Editor;

            RackProject project;
            try
            {
                project = new RackProjectStore().Deserialize(embed.Design);
            }
            catch (System.Exception ex)
            {
                editor.WriteMessage("\nRackCad: no se pudieron leer los datos de la cabecera. " + ex.Message);
                return;
            }

            if (project?.Header == null)
            {
                editor.WriteMessage("\nRackCad: datos de cabecera invalidos.");
                return;
            }

            var config = project.Header;
            config.Name = newName;

            HeaderPlacementResult result;
            if (IsPlantaView(embed))
            {
                result = new PlantaHeaderDrawService().DrawAndPlace(
                    document, config, BuildCabeceraPayload(config, newId, newName, RackEmbedDocument.ViewPlanta), newName);
            }
            else
            {
                result = new LateralHeaderDrawService().DrawAndPlace(
                    document, config, BuildCabeceraPayload(config, newId, newName, RackEmbedDocument.ViewLateral), newName);
            }

            editor.WriteMessage(result != null && result.Success
                ? "\nRackCad: cabecera duplicada como copia independiente ('" + newName + "')."
                : "\nRackCad: no se pudo duplicar la cabecera. " + (result?.ErrorMessage ?? string.Empty));
        }

        private static void EditCabecera(Document document, ObjectId blockId, RackEmbedDocument embed)
        {
            var editor = document.Editor;

            RackProject project;
            try
            {
                project = new RackProjectStore().Deserialize(embed.Design);
            }
            catch (System.Exception ex)
            {
                editor.WriteMessage("\nRackCad: no se pudieron leer los datos de la cabecera. " + ex.Message);
                return;
            }

            if (project?.Header == null)
            {
                editor.WriteMessage("\nRackCad: datos de cabecera invalidos.");
                return;
            }

            var window = new RackFrameConfiguratorWindow(project.Header, canInsertInAutoCad: true) { IsEditingExisting = true };
            AcApplication.ShowModalWindow(window);

            if (!window.InsertRequested)
            {
                return;
            }

            // Editing the cabecera redraws BOTH its views (lateral + planta), found by the shared GUID — the same
            // multi-view round-trip as the selective. The planta is a separate block that links to this cabecera.
            var config = window.Configuration;
            var id = string.IsNullOrEmpty(embed.Id) ? System.Guid.NewGuid().ToString() : embed.Id;
            var name = string.IsNullOrWhiteSpace(config?.Name) ? embed.Name : config.Name;
            var baseName = string.IsNullOrWhiteSpace(name) ? null : name.Trim();

            var blocks = FindRackBlocks(document, id);
            var lateralBlocks = blocks.Where(b => !IsPlantaView(b.Embed)).Select(b => b.BlockId).ToList();
            var plantaBlocks = blocks.Where(b => IsPlantaView(b.Embed)).Select(b => b.BlockId).ToList();

            // Make sure the clicked block is handled even if the GUID scan missed it.
            if (lateralBlocks.Count == 0 && !plantaBlocks.Contains(blockId) && !IsPlantaView(embed))
            {
                lateralBlocks.Add(blockId);
            }

            var updated = 0;
            foreach (var lateralId in lateralBlocks)
            {
                var r = new LateralHeaderDrawService().RedrawInPlace(
                    document, lateralId, config, BuildCabeceraPayload(config, id, name, RackEmbedDocument.ViewLateral));
                if (r != null && r.Success)
                {
                    RackBlockRenamer.SyncName(document, lateralId, baseName);
                    updated++;
                }
            }

            foreach (var plantaId in plantaBlocks)
            {
                var r = new PlantaHeaderDrawService().RedrawInPlace(
                    document, plantaId, config, BuildCabeceraPayload(config, id, name, RackEmbedDocument.ViewPlanta));
                if (r != null && r.Success)
                {
                    RackBlockRenamer.SyncName(document, plantaId, baseName == null ? null : baseName + " - planta");
                    updated++;
                }
            }

            // "Insertar": after refreshing the existing views above, place a NEW linked view-block (same GUID) of the
            // requested view via the jig. "Actualizar" (UpdateOnly) inserts nothing.
            if (!window.UpdateOnly)
            {
                if (window.InsertView == RackEmbedDocument.ViewPlanta)
                {
                    var payload = BuildCabeceraPayload(config, id, name, RackEmbedDocument.ViewPlanta);
                    var inserted = new PlantaHeaderDrawService().DrawAndPlace(document, config, payload, name);
                    editor.WriteMessage(inserted != null && inserted.Success
                        ? "\nRackCad: vista planta insertada y ligada a la cabecera; RACKEDITAR sobre cualquier vista edita ambas."
                        : "\nRackCad: no se pudo insertar la planta. " + (inserted?.ErrorMessage ?? string.Empty));
                }
                else
                {
                    var payload = BuildCabeceraPayload(config, id, name, RackEmbedDocument.ViewLateral);
                    var inserted = new LateralHeaderDrawService().DrawAndPlace(document, config, payload, name);
                    editor.WriteMessage(inserted != null && inserted.Success
                        ? "\nRackCad: cabecera lateral insertada y ligada al mismo rack (mismo GUID)."
                        : "\nRackCad: no se pudo insertar la cabecera. " + (inserted?.ErrorMessage ?? string.Empty));
                }

                return;
            }

            editor.WriteMessage(updated > 0
                ? "\nRackCad: cabecera actualizada; sus vistas (lateral/planta) se redibujaron."
                : "\nRackCad: no se pudo actualizar la cabecera.");
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
    }
}
