using System.Globalization;
using System.Linq;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Runtime;
using RackCad.Application.Persistence;
using RackCad.Application.Systems;
using RackCad.Domain.Systems;
using RackCad.Plugin.Headers;
using RackCad.Plugin.Systems;
using RackCad.UI;
using AcApplication = Autodesk.AutoCAD.ApplicationServices.Application;

namespace RackCad.Plugin
{
    /// <summary>Selective-rack commands + their draw/edit/payload helpers (frontal / lateral corte / planta).</summary>
    public sealed partial class RackFrameCommands
    {
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

        /// <summary>Duplicate a selective rack as an independent copy (new GUID/name), drawn in the clicked view.</summary>
        private static void DuplicateSelective(Document document, RackEmbedDocument embed, string newId, string newName)
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

            var design = saved?.ToDomain();
            if (design == null)
            {
                editor.WriteMessage("\nRackCad: datos de selectivo invalidos.");
                return;
            }

            var system = new SelectiveGeometryResolver().Resolve(design, LateralHeaderDrawService.LoadCatalog());
            // Draw a NEW block through the normal insertion path with the fresh id/name (DrawSelectiveView writes the
            // outcome). The copy is independent: RACKEDITAR on it finds only itself by the new GUID.
            DrawSelectiveView(embed.View, system, design, newId, newName);
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

            // Editing the SYSTEM redraws BOTH views. A rack is one frontal block + N lateral section blocks, all sharing
            // this GUID; find them and redefine each in place (every copy updates). Id comes from the embed (stable); the
            // client name may have been edited in the window.
            var design = window.DesignToInsert;
            var system = window.SystemToInsert;
            var id = string.IsNullOrEmpty(embed.Id) ? window.RackId : embed.Id;
            var name = string.IsNullOrWhiteSpace(window.RackName) ? embed.Name : window.RackName;

            var blocks = FindRackBlocks(document, id);
            var frontalBlocks = blocks.Where(b => !IsLateralView(b.Embed) && !IsPlantaView(b.Embed)).Select(b => b.BlockId).ToList();
            var lateralBlocks = blocks.Where(b => IsLateralView(b.Embed)).OrderBy(b => b.Embed.Section).ToList();
            var plantaBlocks = blocks.Where(b => IsPlantaView(b.Embed)).Select(b => b.BlockId).ToList();

            // The clicked block might not carry the GUID scan (defensive): make sure the selected one is handled.
            if (frontalBlocks.Count == 0 && lateralBlocks.All(b => b.BlockId != blockId) && !plantaBlocks.Contains(blockId)
                && !IsLateralView(embed) && !IsPlantaView(embed))
            {
                frontalBlocks.Add(blockId);
            }

            var updatedFrontal = 0;
            foreach (var frontalId in frontalBlocks)
            {
                var payload = BuildSelectivePayload(design, id, name, RackEmbedDocument.ViewFrontal);
                var r = new SelectiveFrontalDrawService().RedrawInPlace(document, frontalId, system, payload);
                if (r != null && r.Success)
                {
                    updatedFrontal++;
                }
            }

            // Redraw each existing lateral section in place with the section's new geometry (matched by section
            // index). Regen ONCE after the loop — a full drawing regeneration per corte is pure waste.
            var updatedLateral = 0;
            if (lateralBlocks.Count > 0)
            {
                var cortes = new SelectiveLateralBuilder().Cortes(system, LateralHeaderDrawService.LoadCatalog());
                var lateralService = new LateralHeaderDrawService();
                foreach (var lat in lateralBlocks)
                {
                    var corte = cortes.FirstOrDefault(c => c.PostIndex == lat.Embed.Section);
                    if (corte == null)
                    {
                        continue; // the design shrank: this section no longer exists
                    }

                    var payload = BuildSelectivePayload(design, id, name, RackEmbedDocument.ViewLateral, corte.PostIndex);
                    var r = lateralService.RedrawInPlace(document, lat.BlockId, corte.Cabecera, payload, corte.Largueros, regen: false);
                    if (r != null && r.Success)
                    {
                        updatedLateral++;
                    }
                }

                if (updatedLateral > 0)
                {
                    document.Editor.Regen();
                }
            }

            // Redraw the planta block(s) in place (one block for the whole top view).
            var updatedPlanta = 0;
            foreach (var plantaId in plantaBlocks)
            {
                var payload = BuildSelectivePayload(design, id, name, RackEmbedDocument.ViewPlanta);
                var r = new SelectivePlantaDrawService().RedrawInPlace(document, plantaId, system, payload);
                if (r != null && r.Success)
                {
                    updatedPlanta++;
                }
            }

            // If the user asked to insert a lateral, ask which corte and add it (tied to this GUID); the existing
            // views were already redrawn above.
            if (window.InsertView == RackEmbedDocument.ViewLateral)
            {
                InsertSelectiveLateralSection(document, system, design, id, name);
                return;
            }

            // The planta is one block for the whole top view; insert it if requested and none exists yet.
            if (window.InsertView == RackEmbedDocument.ViewPlanta && plantaBlocks.Count == 0)
            {
                var payload = BuildSelectivePayload(design, id, name, RackEmbedDocument.ViewPlanta);
                var inserted = new SelectivePlantaDrawService().DrawAndPlace(document, system, payload, name);
                editor.WriteMessage(inserted != null && inserted.Success
                    ? "\nRackCad: vista planta insertada y ligada al sistema; RACKEDITAR redibuja todas las vistas."
                    : "\nRackCad: no se pudo insertar la planta. " + (inserted?.ErrorMessage ?? string.Empty));
                return;
            }

            editor.WriteMessage(updatedFrontal + updatedLateral + updatedPlanta > 0
                ? "\nRackCad: sistema actualizado; sus vistas se redibujaron (frontal x"
                    + updatedFrontal.ToString(CultureInfo.InvariantCulture) + ", lateral x"
                    + updatedLateral.ToString(CultureInfo.InvariantCulture) + ", planta x"
                    + updatedPlanta.ToString(CultureInfo.InvariantCulture) + ")."
                : "\nRackCad: no se pudo actualizar el rack.");
        }

        /// <summary>True when a view-block draws the LATERAL view (so it is a section of the system, not the frontal).</summary>
        private static bool IsLateralView(RackEmbedDocument embed) =>
            embed != null && string.Equals(embed.View, RackEmbedDocument.ViewLateral, System.StringComparison.OrdinalIgnoreCase);

        /// <summary>
        /// Wraps a selective design in the uniform embed envelope (kind + id + name + view + section + design JSON).
        /// Every view-block of a rack (the one frontal + each lateral section) carries the SAME full design and Id, so
        /// RACKEDITAR on any of them reopens the whole system; <paramref name="section"/> tags which lateral section.
        /// </summary>
        private static string BuildSelectivePayload(SelectivePalletDesign design, string id, string name, string view, int section = -1)
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
                Section = section,
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
                InsertSelectiveLateralSection(document, system, design, id, name);
                return;
            }

            if (view == RackEmbedDocument.ViewPlanta)
            {
                var plantaPayload = BuildSelectivePayload(design, id, name, RackEmbedDocument.ViewPlanta);
                var plantaResult = new SelectivePlantaDrawService().DrawAndPlace(document, system, plantaPayload, name);
                document.Editor.WriteMessage("\n" + DescribeSelective(plantaResult));
                return;
            }

            var payload = BuildSelectivePayload(design, id, name, RackEmbedDocument.ViewFrontal);
            var result = new SelectiveFrontalDrawService().DrawAndPlace(document, system, payload, name);
            document.Editor.WriteMessage("\n" + DescribeSelective(result));
        }

        /// <summary>
        /// Inserts ONE lateral "corte" (cross-section), chosen by post number, and jig-places it. The section carries
        /// the SAME rack id + full design (View=lateral, Section=i), so it is tied to the system: RACKEDITAR on it
        /// reopens the whole selective and redraws BOTH views. It is its own block (movable independently), but a view
        /// OF the system, not a loose cabecera. Called after inserting the frontal (via RACKEDITAR) so it links to it.
        /// </summary>
        private static void InsertSelectiveLateralSection(Document document, SelectiveRackSystem system, SelectivePalletDesign design, string id, string name)
        {
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

            // Ask WHICH post's corte to insert (1-based, matching the frontal preview numbers).
            var postCount = system.Bays.Count + 1;
            // Sin acentos: los mensajes de línea de comandos de AutoCAD evitan acentos en todo el plugin
            // (riesgo de mojibake en consolas no-Unicode); solo la UI WPF los lleva.
            var options = new PromptIntegerOptions("\nQue corte lateral insertar (numero de poste)?")
            {
                LowerLimit = 1,
                UpperLimit = postCount,
                DefaultValue = 1,
                UseDefaultValue = true,
                AllowNone = false
            };

            var pick = editor.GetInteger(options);
            if (pick.Status != PromptStatus.OK)
            {
                return;
            }

            var corte = cortes.FirstOrDefault(c => c.PostIndex == pick.Value - 1);
            if (corte == null)
            {
                editor.WriteMessage("\nRackCad: el poste " + pick.Value.ToString(CultureInfo.InvariantCulture) + " no tiene corte lateral.");
                return;
            }

            var baseName = string.IsNullOrWhiteSpace(name) ? "Selectivo" : name.Trim();
            var sectionName = baseName + " - lateral " + pick.Value.ToString(CultureInfo.InvariantCulture);
            var payload = BuildSelectivePayload(design, id, name, RackEmbedDocument.ViewLateral, corte.PostIndex);

            var result = new LateralHeaderDrawService().DrawAndPlace(document, corte.Cabecera, payload, sectionName, corte.Largueros);
            editor.WriteMessage(result != null && result.Success
                ? "\nRackCad: corte lateral del poste " + pick.Value.ToString(CultureInfo.InvariantCulture) + " insertado y ligado al sistema."
                : "\nRackCad: no se pudo insertar el corte lateral. " + (result?.ErrorMessage ?? string.Empty));
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
    }
}
