using System.Globalization;
using System.Linq;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Runtime;
using RackCad.Application.Catalogs;
using RackCad.Application.Persistence;
using RackCad.Domain.Systems;
using RackCad.Plugin.Headers;
using RackCad.Plugin.Systems;
using RackCad.UI;
using AcApplication = Autodesk.AutoCAD.ApplicationServices.Application;

namespace RackCad.Plugin
{
    /// <summary>Cama de rodamiento (roller flow bed) commands + their draw/edit/payload helpers, plus alias.</summary>
    public sealed class RackCamaCommands
    {
        [CommandMethod("QCM")] public void AliasQuickCama() => QuickCama();                // QUICKCAMA

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
                RackCommandSupport.Report(ex);
            }
        }

        /// <summary>
        /// Prompts for the roller from the catalog (role RODILLO): auto-picks a single one, otherwise prints a
        /// numbered list and asks for the index. Returns the roller id, or null if the user cancelled.
        /// </summary>
        private static string PromptRollerId(Editor editor, RackCatalog catalog)
        {
            var rollers = (catalog?.FlowBedProfiles ?? System.Array.Empty<FlowBedComponentCatalogEntry>())
                .Where(c => c != null && !string.IsNullOrWhiteSpace(c.Id)
                    && string.Equals(c.Role, FlowBedDefaults.RollerRole, System.StringComparison.OrdinalIgnoreCase))
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

        /// <summary>Builds the roller-bed block and runs the placement jig, then reports the outcome.</summary>
        internal static void DrawAndPlaceBed(FlowBedConfiguration config, string payloadJson, string rackName)
        {
            var document = AcApplication.DocumentManager.MdiActiveDocument;

            if (document == null || config == null)
            {
                return;
            }

            var result = new FlowBedDrawService().DrawAndPlace(document, config, payloadJson, rackName);
            document.Editor.WriteMessage("\n" + DescribeBed(result));
        }

        /// <summary>Wraps a cama (FlowBedConfiguration) in the uniform embed envelope (fresh insert: no source metadata).</summary>
        internal static string BuildCamaPayload(FlowBedConfiguration config, string id, string name)
            => BuildCamaPayload(config, id, name, null, null);

        /// <summary>
        /// Wraps a cama in the uniform embed envelope, carrying forward the unknown JSON fields of a source envelope and
        /// source FlowBed document so an edit does not drop metadata a newer build wrote (I-11 D3). Unknown keys are disjoint
        /// from the known fields the editor rewrites, so the edited values always win and the extra keys ride along.
        /// </summary>
        internal static string BuildCamaPayload(
            FlowBedConfiguration config, string id, string name, RackEmbedDocument sourceEmbed, FlowBedDocument sourceDesign)
        {
            if (config == null)
            {
                return null;
            }

            var designDocument = FlowBedDocument.FromDomain(config);
            designDocument.SchemaVersion = SchemaVersionPolicy.ResolveWriteVersion(
                sourceDesign?.SchemaVersion, FlowBedDocument.CurrentSchemaVersion);
            designDocument.ExtensionData = sourceDesign?.ExtensionData;

            var designJson = new FlowBedConfigurationStore().SerializeDocument(designDocument);
            var embed = RackEmbedComposer.Compose(
                sourceEmbed, RackEmbedDocument.KindCama, id, name, view: null, section: -1, design: designJson);
            return new RackEmbedStore().Serialize(embed);
        }

        internal static void EditCama(Document document, ObjectId blockId, RackEmbedDocument embed)
        {
            var editor = document.Editor;

            // Read the whole source document (with its ExtensionData) so the re-save can preserve unknown FlowBed fields.
            var sourceDesign = new FlowBedConfigurationStore().DeserializeDocument(embed.Design);
            var config = sourceDesign?.ToDomain();
            if (config == null || !RackDesignValidation.IsUsableFlowBed(config))
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
                BuildCamaPayload(window.FlowBedToInsert, window.RackId, window.RackName, embed, sourceDesign));

            if (result != null && result.Success)
            {
                RackBlockRenamer.SyncName(document, blockId, string.IsNullOrWhiteSpace(window.RackName) ? null : window.RackName.Trim());
            }

            editor.WriteMessage(result != null && result.Success
                ? "\nRackCad: cama actualizada; todas sus copias reflejan el cambio."
                : "\nRackCad: no se pudo actualizar la cama. " + (result?.ErrorMessage ?? string.Empty));
        }

        private static string DescribeBed(HeaderPlacementResult result)
            => RackCommandSupport.DescribePlacement(result, "la cama de rodamiento", "cama insertada");
    }
}
