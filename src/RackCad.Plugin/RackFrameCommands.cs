using System.Globalization;
using System.Linq;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
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
                    if (menu.ConfigurationToInsert != null)
                    {
                        DrawAndPlace(menu.ConfigurationToInsert);
                    }
                    else if (menu.DynamicSystemToInsert != null)
                    {
                        DrawAndPlaceSystem(menu.DynamicSystemToInsert, BuildDynamicPayload(menu.DynamicSystemToInsert, menu.DynamicRackId, menu.DynamicRackName), menu.DynamicRackName);
                    }
                    else if (menu.FlowBedToInsert != null)
                    {
                        DrawAndPlaceBed(menu.FlowBedToInsert, BuildCamaPayload(menu.FlowBedToInsert, menu.FlowBedRackId, menu.FlowBedRackName), menu.FlowBedRackName);
                    }
                    else if (menu.SelectiveSystemToInsert != null)
                    {
                        DrawSelectiveView(menu.SelectiveView, menu.SelectiveSystemToInsert, menu.SelectiveDesignToInsert, menu.SelectiveRackId, menu.SelectiveRackName);
                    }
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

                var depthOptions = new PromptDistanceOptions("\nFondo (in)")
                {
                    DefaultValue = 48.0,
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
                    DefaultValue = 132.0,
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
                return CatalogIds.StandardPost;
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

                var payload = BuildDynamicPayload(system, System.Guid.NewGuid().ToString(), null);
                var result = new DynamicSystemDrawService().DrawAndPlace(document, system, payload);
                document.Editor.WriteMessage("\n" + Describe(result));
            }
            catch (System.Exception ex)
            {
                Report(ex);
            }
        }

        /// <summary>
        /// Draws one roller bed ("cama de rodamiento") in the lateral view as a block placed with the mouse.
        /// Prompts for the bed type, roller, lane depth and (for dynamic beds) pallet depth. Pushback beds
        /// omit the brakes.
        /// </summary>
        [CommandMethod("QUICKCAMA")]
        public void QuickCama()
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

                var typeOptions = new PromptKeywordOptions("\nTipo de cama") { AllowNone = true };
                typeOptions.Keywords.Add("Dinamica");
                typeOptions.Keywords.Add("Pushback");
                typeOptions.Keywords.Default = "Dinamica";
                var typeResult = editor.GetKeywords(typeOptions);
                if (typeResult.Status != PromptStatus.OK && typeResult.Status != PromptStatus.None)
                {
                    return;
                }

                var bedType = typeResult.StringResult == "Pushback" ? FlowBedType.Pushback : FlowBedType.Dynamic;

                var rollerId = PromptRollerId(editor, catalog);
                if (rollerId == null)
                {
                    return;
                }

                var depthOptions = new PromptDistanceOptions("\nFondo de cama (in)")
                {
                    DefaultValue = 96.0,
                    UseDefaultValue = true,
                    AllowNegative = false,
                    AllowZero = false
                };
                var depthResult = editor.GetDistance(depthOptions);
                if (depthResult.Status != PromptStatus.OK)
                {
                    return;
                }

                var palletDepth = 0.0;
                if (bedType == FlowBedType.Dynamic)
                {
                    var palletOptions = new PromptDistanceOptions("\nFondo de tarima (in)")
                    {
                        DefaultValue = 48.0,
                        UseDefaultValue = true,
                        AllowNegative = false,
                        AllowZero = false
                    };
                    var palletResult = editor.GetDistance(palletOptions);
                    if (palletResult.Status != PromptStatus.OK)
                    {
                        return;
                    }

                    palletDepth = palletResult.Value;
                }

                var config = new FlowBedConfiguration
                {
                    BedType = bedType,
                    LaneDepth = depthResult.Value,
                    PalletDepth = palletDepth,
                    RollerId = rollerId
                };

                var payload = BuildCamaPayload(config, System.Guid.NewGuid().ToString(), null);
                var result = new FlowBedDrawService().DrawAndPlace(document, config, payload);
                editor.WriteMessage("\n" + DescribeBed(result));
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

            var payload = BuildCabeceraPayload(configuration, System.Guid.NewGuid().ToString(), configuration.Name);
            var result = new LateralHeaderDrawService().DrawAndPlace(document, configuration, payload, configuration.Name);
            document.Editor.WriteMessage("\n" + Describe(result));
        }

        /// <summary>Wraps a cabecera (RackFrameConfiguration) in the uniform embed envelope; reuses the project store.</summary>
        private static string BuildCabeceraPayload(RackFrameConfiguration configuration, string id, string name)
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
                Design = designJson
            });
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

            var window = new RackFrameConfiguratorWindow(project.Header, canInsertInAutoCad: true);
            AcApplication.ShowModalWindow(window);

            if (!window.InsertRequested)
            {
                return;
            }

            var config = window.Configuration;
            var result = new LateralHeaderDrawService().RedrawInPlace(
                document, blockId, config, BuildCabeceraPayload(config, embed.Id, config?.Name));

            editor.WriteMessage(result != null && result.Success
                ? "\nRackCad: cabecera actualizada; todas sus copias reflejan el cambio."
                : "\nRackCad: no se pudo actualizar la cabecera. " + (result?.ErrorMessage ?? string.Empty));
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

        /// <summary>
        /// Prompts for the roller from the catalog (role RODILLO): auto-picks a single one, otherwise prints a
        /// numbered list and asks for the index. Returns the roller id, or null if the user cancelled.
        /// </summary>
        private static string PromptRollerId(Editor editor, RackCatalog catalog)
        {
            var rollers = (catalog?.FlowBedProfiles ?? System.Array.Empty<FlowBedComponentCatalogEntry>())
                .Where(c => c != null && !string.IsNullOrWhiteSpace(c.Id)
                    && string.Equals(c.Role, "RODILLO", System.StringComparison.OrdinalIgnoreCase))
                .GroupBy(c => c.Id, System.StringComparer.OrdinalIgnoreCase)
                .Select(g => g.First())
                .OrderBy(c => c.Diameter)
                .ToList();

            if (rollers.Count == 0)
            {
                return FlowBedDefaults.RollerId;
            }

            if (rollers.Count == 1)
            {
                editor.WriteMessage("\nRodillo: " + rollers[0].Label);
                return rollers[0].Id;
            }

            for (var i = 0; i < rollers.Count; i++)
            {
                editor.WriteMessage(string.Format(CultureInfo.InvariantCulture, "\n  {0}: {1}", i + 1, rollers[i].Label));
            }

            var options = new PromptIntegerOptions("\nRodillo #")
            {
                DefaultValue = 1,
                UseDefaultValue = true,
                LowerLimit = 1,
                UpperLimit = rollers.Count
            };
            var result = editor.GetInteger(options);
            if (result.Status != PromptStatus.OK)
            {
                return null;
            }

            return rollers[result.Value - 1].Id;
        }

        /// <summary>Opens the selective-rack window; draws it after the modal windows close.</summary>
        [CommandMethod("RACKSELECTIVO")]
        public void RackSelectivo()
        {
            try
            {
                var window = new RackSelectiveWindow(canInsertInAutoCad: true);
                AcApplication.ShowModalWindow(window);

                if (window.InsertRequested)
                {
                    DrawSelectiveView(window.InsertView, window.SystemToInsert, window.DesignToInsert, window.RackId, window.RackName);
                }
            }
            catch (System.Exception ex)
            {
                Report(ex);
            }
        }

        /// <summary>Select an already-drawn selective rack, reopen its editor with all its data, and redraw it.</summary>
        [CommandMethod("RACKEDITAR")]
        public void RackEditar()
        {
            try
            {
                var document = AcApplication.DocumentManager.MdiActiveDocument;
                if (document == null)
                {
                    return;
                }

                var editor = document.Editor;
                var options = new PromptEntityOptions("\nSelecciona un rack para editar: ");
                options.SetRejectMessage("\nEse objeto no es un rack.");
                options.AddAllowedClass(typeof(BlockReference), exactMatch: false);

                var selection = editor.GetEntity(options);
                if (selection.Status != PromptStatus.OK)
                {
                    return;
                }

                // Read the payload from the block DEFINITION (shared by every copy), found via the selected reference.
                ObjectId blockId;
                string json;
                using (document.LockDocument())
                using (var transaction = document.Database.TransactionManager.StartTransaction())
                {
                    var reference = (BlockReference)transaction.GetObject(selection.ObjectId, OpenMode.ForRead);
                    blockId = reference.BlockTableRecord;
                    json = RackBlockData.Read(transaction, blockId);
                    transaction.Commit();
                }

                var embed = new RackEmbedStore().Deserialize(json);
                if (embed == null || string.IsNullOrEmpty(embed.Design))
                {
                    editor.WriteMessage("\nRackCad: ese bloque no tiene datos de rack editables.");
                    return;
                }

                // Dispatch by rack type — the same round-trip serves selective, dynamic (and later cabecera/cama).
                switch (embed.Kind)
                {
                    case RackEmbedDocument.KindSelective:
                        EditSelective(document, blockId, embed);
                        break;
                    case RackEmbedDocument.KindDynamic:
                        EditDynamic(document, blockId, embed);
                        break;
                    case RackEmbedDocument.KindCabecera:
                        EditCabecera(document, blockId, embed);
                        break;
                    case RackEmbedDocument.KindCama:
                        EditCama(document, blockId, embed);
                        break;
                    default:
                        editor.WriteMessage("\nRackCad: tipo de rack no reconocido (" + embed.Kind + ").");
                        break;
                }
            }
            catch (System.Exception ex)
            {
                Report(ex);
            }
        }

        private static void EditSelective(Document document, ObjectId blockId, RackEmbedDocument embed)
        {
            var editor = document.Editor;

            SelectivePalletDesignDocument saved;
            try
            {
                saved = new SelectivePalletDesignStore().Deserialize(embed.Design);
            }
            catch (System.Exception ex)
            {
                editor.WriteMessage("\nRackCad: no se pudieron leer los datos del rack. " + ex.Message);
                return;
            }

            var window = new RackSelectiveWindow(canInsertInAutoCad: true);
            window.LoadExisting(saved);
            AcApplication.ShowModalWindow(window);

            if (!window.InsertRequested)
            {
                return;
            }

            // The frontal selective is one block; redefine it in place (its copies all update). The lateral cortes
            // are INDEPENDENT cabecera blocks, so they are edited on their own via the cabecera round-trip.
            var payload = BuildSelectivePayload(window.DesignToInsert, window.RackId, window.RackName, RackEmbedDocument.ViewFrontal);
            var result = new SelectiveFrontalDrawService().RedrawInPlace(document, blockId, window.SystemToInsert, payload);

            editor.WriteMessage(result != null && result.Success
                ? "\nRackCad: rack actualizado; todas sus copias reflejan el cambio."
                : "\nRackCad: no se pudo actualizar el rack. " + (result?.ErrorMessage ?? string.Empty));
        }

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

            if (project?.DynamicSystem == null)
            {
                editor.WriteMessage("\nRackCad: datos de sistema dinamico invalidos.");
                return;
            }

            var window = new RackDynamicSystemWindow(canInsertInAutoCad: true);
            window.LoadExisting(project.DynamicSystem, embed.Id, embed.Name);
            AcApplication.ShowModalWindow(window);

            if (!window.InsertRequested)
            {
                return;
            }

            var result = new DynamicSystemDrawService().RedrawInPlace(
                document, blockId, window.SystemToInsert,
                BuildDynamicPayload(window.SystemToInsert, window.RackId, window.RackName));

            editor.WriteMessage(result != null && result.Success
                ? "\nRackCad: sistema actualizado; todas sus copias reflejan el cambio."
                : "\nRackCad: no se pudo actualizar el sistema. " + (result?.ErrorMessage ?? string.Empty));
        }

        /// <summary>Wraps a selective design in the uniform embed envelope (kind + id + name + view + design JSON).</summary>
        private static string BuildSelectivePayload(SelectivePalletDesign design, string id, string name, string view)
        {
            if (design == null)
            {
                return null;
            }

            var designJson = new SelectivePalletDesignStore().Serialize(SelectivePalletDesignDocument.From(design, id, name));
            return new RackEmbedStore().Serialize(new RackEmbedDocument
            {
                Kind = RackEmbedDocument.KindSelective,
                Id = id,
                Name = name,
                View = string.IsNullOrWhiteSpace(view) ? RackEmbedDocument.ViewFrontal : view,
                Design = designJson
            });
        }

        /// <summary>Wraps a dynamic system in the uniform embed envelope; reuses the project store for the design JSON.</summary>
        private static string BuildDynamicPayload(DynamicRackSystem system, string id, string name)
        {
            if (system == null)
            {
                return null;
            }

            var designJson = new RackProjectStore().Serialize(RackProject.ForDynamic(system));
            return new RackEmbedStore().Serialize(new RackEmbedDocument
            {
                Kind = RackEmbedDocument.KindDynamic,
                Id = id,
                Name = name,
                Design = designJson
            });
        }

        /// <summary>Draws the selective in the requested VIEW: frontal = one selective block; lateral = one cabecera "corte" per post.</summary>
        private static void DrawSelectiveView(string view, SelectiveRackSystem system, SelectivePalletDesign design, string id, string name)
        {
            var document = AcApplication.DocumentManager.MdiActiveDocument;

            if (document == null || system == null)
            {
                return;
            }

            if (view == RackEmbedDocument.ViewLateral)
            {
                DrawSelectiveCortes(system, name);
                return;
            }

            var payload = BuildSelectivePayload(design, id, name, RackEmbedDocument.ViewFrontal);
            var result = new SelectiveFrontalDrawService().DrawAndPlace(document, system, payload, name);
            document.Editor.WriteMessage("\n" + DescribeSelective(result));
        }

        /// <summary>
        /// Draws the selective's LATERAL view as INDEPENDENT cortes: one cabecera block per post, laid out at the
        /// frontal post Xs from a single base point. Each corte is its own cabecera (own GUID) → RACKEDITAR edits
        /// that corte alone (via the cabecera round-trip), not the whole rack.
        /// </summary>
        private static void DrawSelectiveCortes(SelectiveRackSystem system, string name)
        {
            var document = AcApplication.DocumentManager.MdiActiveDocument;
            if (document == null || system == null)
            {
                return;
            }

            var editor = document.Editor;
            var catalog = LateralHeaderDrawService.LoadCatalog();

            var cortes = new SelectiveLateralBuilder().Cortes(system, catalog);
            if (cortes.Count == 0)
            {
                editor.WriteMessage("\nRackCad: no hay cortes laterales que dibujar.");
                return;
            }

            var pick = editor.GetPoint("\nPunto base de los cortes laterales: ");
            if (pick.Status != PromptStatus.OK)
            {
                return;
            }

            var basePoint = pick.Value;
            var service = new LateralHeaderDrawService();
            var baseName = string.IsNullOrWhiteSpace(name) ? "Selectivo" : name.Trim();
            var drawn = 0;

            foreach (var corte in cortes)
            {
                var corteName = baseName + " - corte " + (corte.PostIndex + 1).ToString(CultureInfo.InvariantCulture);
                var payload = BuildCabeceraPayload(corte.Cabecera, System.Guid.NewGuid().ToString(), corteName);
                var insertion = new Point3d(basePoint.X + corte.X, basePoint.Y, basePoint.Z);

                var result = service.DrawAt(document, corte.Cabecera, insertion, payload, corteName);
                if (result != null && result.Success)
                {
                    drawn++;
                }
            }

            editor.WriteMessage("\nRackCad: " + drawn.ToString(CultureInfo.InvariantCulture)
                + " corte(s) lateral(es) insertado(s); cada uno se edita por separado con RACKEDITAR.");
        }

        private static string DescribeSelective(HeaderPlacementResult result)
        {
            if (!result.Success)
            {
                return "RackCad: no se pudo dibujar el selectivo. " + result.ErrorMessage;
            }

            if (!result.Placed)
            {
                return "RackCad: bloque '" + result.BlockName + "' creado, pero la insercion se cancelo.";
            }

            var summary = string.Format(
                CultureInfo.InvariantCulture,
                "RackCad: selectivo insertado como bloque '{0}'. {1} piezas.",
                result.BlockName,
                result.Outcome.InsertedCount);

            if (result.HasMissingBlocks)
            {
                summary += "\nBloques no definidos en el dibujo (omitidos): " + string.Join(", ", result.MissingBlocks);
            }

            return summary;
        }

        /// <summary>Builds the roller-bed block and runs the placement jig, then reports the outcome.</summary>
        private static void DrawAndPlaceBed(FlowBedConfiguration config, string payloadJson, string rackName)
        {
            var document = AcApplication.DocumentManager.MdiActiveDocument;

            if (document == null || config == null)
            {
                return;
            }

            var result = new FlowBedDrawService().DrawAndPlace(document, config, payloadJson, rackName);
            document.Editor.WriteMessage("\n" + DescribeBed(result));
        }

        /// <summary>Wraps a cama (FlowBedConfiguration) in the uniform embed envelope.</summary>
        private static string BuildCamaPayload(FlowBedConfiguration config, string id, string name)
        {
            if (config == null)
            {
                return null;
            }

            var designJson = new FlowBedConfigurationStore().Serialize(config);
            return new RackEmbedStore().Serialize(new RackEmbedDocument
            {
                Kind = RackEmbedDocument.KindCama,
                Id = id,
                Name = name,
                Design = designJson
            });
        }

        private static void EditCama(Document document, ObjectId blockId, RackEmbedDocument embed)
        {
            var editor = document.Editor;

            var config = new FlowBedConfigurationStore().Deserialize(embed.Design);
            if (config == null)
            {
                editor.WriteMessage("\nRackCad: datos de cama invalidos.");
                return;
            }

            var window = new RackFlowBedWindow(canInsertInAutoCad: true);
            window.LoadExisting(config, embed.Id, embed.Name);
            AcApplication.ShowModalWindow(window);

            if (!window.InsertRequested)
            {
                return;
            }

            var result = new FlowBedDrawService().RedrawInPlace(
                document, blockId, window.FlowBedToInsert,
                BuildCamaPayload(window.FlowBedToInsert, window.RackId, window.RackName));

            editor.WriteMessage(result != null && result.Success
                ? "\nRackCad: cama actualizada; todas sus copias reflejan el cambio."
                : "\nRackCad: no se pudo actualizar la cama. " + (result?.ErrorMessage ?? string.Empty));
        }

        private static string DescribeBed(HeaderPlacementResult result)
        {
            if (!result.Success)
            {
                return "RackCad: no se pudo dibujar la cama de rodamiento. " + result.ErrorMessage;
            }

            if (!result.Placed)
            {
                return "RackCad: bloque '" + result.BlockName + "' creado, pero la insercion se cancelo.";
            }

            var summary = string.Format(
                CultureInfo.InvariantCulture,
                "RackCad: cama insertada como bloque '{0}'. {1} piezas.",
                result.BlockName,
                result.Outcome.InsertedCount);

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
