using System;
using System.Globalization;
using System.Linq;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Runtime;
using RackCad.Application.Persistence;
using RackCad.Application.Systems;
using RackCad.Domain.Systems;
using RackCad.Plugin.Headers;
using RackCad.Plugin.Systems;
using RackCad.UI;
using RackCad.UI.Editor;
using AcApplication = Autodesk.AutoCAD.ApplicationServices.Application;

namespace RackCad.Plugin
{
    /// <summary>
    /// Push Back (I-18) system command + its draw/payload helpers, plus the RPB alias. It opens the pure Push Back editor,
    /// then draws whatever <see cref="PushBackInsertionRequest"/> the window's session produced, using ONLY the I-18a
    /// SystemPlan builders (via the three Push Back draw services). No geometry lives here. Multi-view edit (RACKEDITAR) and
    /// the kind handler are a later increment (4b).
    /// </summary>
    public sealed class RackPushBackCommands
    {
        [CommandMethod("RPB")]
        public void AliasRpb() => RackPushBack(); // RACKPUSHBACK

        /// <summary>Opens the Push Back editor and, if the user asked to insert, draws the requested view.</summary>
        [CommandMethod("RACKPUSHBACK")]
        public void RackPushBack()
        {
            var document = AcApplication.DocumentManager.MdiActiveDocument;
            if (document == null)
            {
                return;
            }

            try
            {
                var window = new RackPushBackSystemWindow(canInsertInAutoCad: true);
                AcApplication.ShowModalWindow(window);

                if (!(window.InsertionRequest is PushBackInsertionRequest request))
                {
                    return; // cancelled / closed: do NOT modify the DWG
                }

                // I-05: warn once if the drawing is not in inches, right before the first block is drawn.
                RackUnitsGuard.WarnIfNotInches(document);
                DrawPushBackView(
                    request.View,
                    request.Section,
                    request.System,
                    request.Design,
                    request.RackId,
                    request.RackName,
                    source: null,
                    innerSource: request.SourceProject);
            }
            catch (System.Exception ex)
            {
                RackCommandSupport.Report(ex);
            }
        }

        /// <summary>Builds the requested linked Push Back view and runs its placement jig. An unknown view/section fails
        /// visibly and draws nothing — it never falls back to lateral or to another frontal end.</summary>
        internal static void DrawPushBackView(
            string view,
            int section,
            PushBackSystem system,
            PushBackDesign design,
            string id,
            string rackName,
            RackEmbedDocument source = null,
            RackProject innerSource = null)
        {
            var document = AcApplication.DocumentManager.MdiActiveDocument;
            if (document == null || system == null)
            {
                return;
            }

            var editor = document.Editor;
            system.Name = rackName;

            // Lateral, unsectioned: prompt for a post and insert that corte.
            if (string.Equals(view, RackEmbedDocument.ViewLateral, StringComparison.OrdinalIgnoreCase) && section < 0)
            {
                InsertPushBackLateralSection(document, system, design, id, rackName, source, innerSource);
                return;
            }

            var payload = BuildPushBackPayload(design, id, rackName, view, section, source, innerSource);
            HeaderPlacementResult result;
            if (string.Equals(view, RackEmbedDocument.ViewPlanta, StringComparison.OrdinalIgnoreCase))
            {
                result = new PushBackPlantaDrawService().DrawAndPlace(document, system, payload, rackName);
            }
            else if (string.Equals(view, RackEmbedDocument.ViewFrontal, StringComparison.OrdinalIgnoreCase))
            {
                // An unknown frontal section is NOT silently coerced into another end.
                if (section != (int)PushBackFrontalEnd.EntradaSalida && section != (int)PushBackFrontalEnd.Posterior)
                {
                    editor.WriteMessage("\nRackCad: seccion frontal Push Back desconocida (" + section.ToString(CultureInfo.InvariantCulture) + "); no se dibuja.");
                    return;
                }

                var end = section == (int)PushBackFrontalEnd.Posterior ? PushBackFrontalEnd.Posterior : PushBackFrontalEnd.EntradaSalida;
                result = new PushBackFrontalDrawService().DrawAndPlace(document, system, end, payload, rackName);
            }
            else if (string.Equals(view, RackEmbedDocument.ViewLateral, StringComparison.OrdinalIgnoreCase))
            {
                // Lateral with a specific post (section = postIndex >= 0).
                result = new PushBackSystemDrawService().DrawAndPlace(document, system, payload, rackName, section);
            }
            else
            {
                editor.WriteMessage("\nRackCad: vista Push Back desconocida (" + (view ?? "null") + "); no se dibuja.");
                return;
            }

            editor.WriteMessage("\n" + RackCommandSupport.DescribePlacement(result, "el sistema Push Back", "sistema Push Back insertado"));
        }

        /// <summary>Prompts for one transverse post and inserts its linked lateral Push Back section.</summary>
        private static void InsertPushBackLateralSection(
            Document document,
            PushBackSystem system,
            PushBackDesign design,
            string id,
            string name,
            RackEmbedDocument source = null,
            RackProject innerSource = null)
        {
            if (document == null || system == null)
            {
                return;
            }

            var editor = document.Editor;
            var cortes = new PushBackSystemLateralBuilder().Cortes(system, LateralHeaderDrawService.LoadCatalog());
            if (cortes.Count == 0)
            {
                editor.WriteMessage("\nRackCad: no hay cortes laterales Push Back que dibujar.");
                return;
            }

            var options = new PromptIntegerOptions("\nQue corte lateral Push Back insertar (numero de poste)?")
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
                editor.WriteMessage("\nRackCad: el poste " + pick.Value.ToString(CultureInfo.InvariantCulture) + " no tiene corte lateral Push Back.");
                return;
            }

            var payload = BuildPushBackPayload(design, id, name, RackEmbedDocument.ViewLateral, corte.PostIndex, source, innerSource);
            // Pass the BASE name only; PushBackSystemDrawService.BlockName is the single authority for the "- lateral N" suffix.
            var result = new PushBackSystemDrawService().DrawAndPlace(document, system, payload, name, corte.PostIndex);
            editor.WriteMessage(result != null && result.Success
                ? "\nRackCad: corte lateral Push Back del poste " + pick.Value.ToString(CultureInfo.InvariantCulture) + " insertado y ligado al sistema."
                : "\nRackCad: no se pudo insertar el corte lateral Push Back. " + (result?.ErrorMessage ?? string.Empty));
        }

        /// <summary>Wraps a Push Back system in the uniform embed envelope; reuses the project store for the design JSON.</summary>
        internal static string BuildPushBackPayload(
            PushBackDesign design,
            string id,
            string name,
            string view = RackEmbedDocument.ViewLateral,
            int section = -1,
            RackEmbedDocument source = null,
            RackProject innerSource = null)
        {
            if (design == null)
            {
                return null;
            }

            // The inner Design of a Push Back block is itself a RackProjectDocument — a boundary INDEPENDENT of the envelope
            // (I-11). innerSource is the ALREADY-RESOLVED source project (null for a fresh one, or the library/initiating
            // project); WithSourceMetadataFrom preserves its unknown fields + non-downgraded version. Never KindDynamic.
            var designJson = new RackProjectStore().Serialize(RackProject.ForPushBack(design).WithSourceMetadataFrom(innerSource));
            var embed = RackEmbedComposer.Compose(
                source, RackEmbedDocument.KindPushBack, id, name,
                string.IsNullOrWhiteSpace(view) ? RackEmbedDocument.ViewLateral : view, section, designJson);
            return new RackEmbedStore().Serialize(embed);
        }
    }
}
