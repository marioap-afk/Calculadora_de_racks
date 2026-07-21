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
    /// <summary>Selective-rack commands + their draw/edit/payload helpers (frontal / lateral corte / planta), plus alias.</summary>
    public sealed class RackSelectivoCommands
    {
        [CommandMethod("RS")] public void AliasRackSelectivo() => RackSelectivo();        // RACKSELECTIVO

        /// <summary>Opens the selective-rack window; draws it after the modal windows close.</summary>
        [CommandMethod("RACKSELECTIVO")]
        public void RackSelectivo()
        {
            try
            {
                var window = new RackSelectiveWindow(canInsertInAutoCad: true);
                window.SetDimensionStyles(RackCommandSupport.ReadDimensionStyleNames(AcApplication.DocumentManager.MdiActiveDocument));
                AcApplication.ShowModalWindow(window);

                if (window.InsertRequested)
                {
                    DrawSelectiveView(window.InsertView, window.SystemToInsert, window.DesignToInsert, window.RackId, window.RackName);
                }
            }
            catch (System.Exception ex)
            {
                RackCommandSupport.Report(ex);
            }
        }

        internal static void EditSelective(Document document, ObjectId blockId, RackEmbedDocument embed)
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
            window.SetDimensionStyles(RackCommandSupport.ReadDimensionStyleNames(document)); // before LoadExisting so a saved style selects
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
            system.Name = name; // the "Colocar nombre de rack" annotation draws this
            // Base name for syncing the block-definition names across views (null = keep each view's descriptive default).
            var baseName = string.IsNullOrWhiteSpace(name) ? null : name.Trim();

            var blocks = RackCommandSupport.FindRackBlocks(document, id);
            var frontalBlocks = blocks.Where(b => !IsLateralView(b.Embed) && !RackCommandSupport.IsPlantaView(b.Embed)).ToList();
            var lateralBlocks = blocks.Where(b => IsLateralView(b.Embed)).OrderBy(b => b.Embed.Section).ToList();
            var plantaBlocks = blocks.Where(b => RackCommandSupport.IsPlantaView(b.Embed)).Select(b => b.BlockId).ToList();

            // The clicked block might not carry the GUID scan (defensive): make sure the selected one is handled.
            if (frontalBlocks.Count == 0 && lateralBlocks.All(b => b.BlockId != blockId) && !plantaBlocks.Contains(blockId)
                && !IsLateralView(embed) && !RackCommandSupport.IsPlantaView(embed))
            {
                frontalBlocks.Add((blockId, embed));
            }

            // The design JSON is identical for every view-block (only the envelope's view/section differ), so
            // serialize the full design ONCE — not once per frontal + corte + planta.
            var designJson = SerializeSelectiveDesign(design, id, name);

            // Each frontal block draws ONE fondo's face (its Section = fondo index; a legacy block with -1 = fondo 0).
            // Every loop below redraws with regen:false and the drawing regenerates ONCE at the end — a full
            // regeneration per view-block is pure waste on multi-view racks.
            var fondoCount = SelectiveDepthLayout.Count(system);

            // View-blocks whose fondo/corte no longer exists after a shrink: erased below instead of left as phantoms.
            // (User's choice: their peak geometry + GUID payload otherwise linger in every future Regen and
            // RACKEDITAR scan. A later re-grow re-inserts that fondo's frontal / that post's corte via the jig.)
            var staleViewBlocks = new System.Collections.Generic.List<ObjectId>();

            var updatedFrontal = 0;
            foreach (var fb in frontalBlocks)
            {
                var fondo = fb.Embed != null && fb.Embed.Section >= 0 ? fb.Embed.Section : 0;
                if (fondo >= fondoCount)
                {
                    staleViewBlocks.Add(fb.BlockId); // this fondo is gone — erase the phantom frontal
                    continue;
                }

                var fondoView = SelectiveDepthLayout.FondoSystemView(system, fondo);
                fondoView.Name = name;
                var payload = WrapSelectivePayload(designJson, id, name, RackEmbedDocument.ViewFrontal, fondo);
                var r = new SelectiveFrontalDrawService().RedrawInPlace(document, fb.BlockId, fondoView, payload, regen: false);
                if (r != null && r.Success)
                {
                    RackBlockRenamer.SyncName(document, fb.BlockId, FrontalName(baseName, fondo, fondoCount));
                    updatedFrontal++;
                }
            }

            // Redraw each existing lateral section in place with the section's new geometry (matched by section index).
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
                        staleViewBlocks.Add(lat.BlockId); // this section is gone — erase the phantom lateral (see note above)
                        continue;
                    }

                    var payload = WrapSelectivePayload(designJson, id, name, RackEmbedDocument.ViewLateral, corte.PostIndex);
                    var r = lateralService.RedrawInPlace(document, lat.BlockId, corte.Cabecera, payload, corte.Largueros, regen: false);
                    if (r != null && r.Success)
                    {
                        RackBlockRenamer.SyncName(document, lat.BlockId,
                            baseName == null ? null : baseName + " - lateral " + (corte.PostIndex + 1).ToString(CultureInfo.InvariantCulture));
                        updatedLateral++;
                    }
                }
            }

            // Redraw the planta block(s) in place (one block for the whole top view).
            var updatedPlanta = 0;
            foreach (var plantaId in plantaBlocks)
            {
                var payload = WrapSelectivePayload(designJson, id, name, RackEmbedDocument.ViewPlanta);
                var r = new SelectivePlantaDrawService().RedrawInPlace(document, plantaId, system, payload, regen: false);
                if (r != null && r.Success)
                {
                    RackBlockRenamer.SyncName(document, plantaId, baseName == null ? null : baseName + " - planta");
                    updatedPlanta++;
                }
            }

            // Erase the phantom view-blocks a shrink left behind (fondos/cortes that no longer exist) — but ONLY when
            // at least one view-block SURVIVES the edit. The rack's GUID + embedded design live on these blocks, so
            // erasing the last one would destroy the rack irrecoverably (no surviving block to RACKEDITAR). When nothing
            // survives, keep the phantoms — the rack stays editable and the user can insert a fresh view.
            var survivors = frontalBlocks.Count + lateralBlocks.Count + plantaBlocks.Count - staleViewBlocks.Count;
            var erasedPhantoms = 0;
            if (staleViewBlocks.Count > 0 && survivors > 0)
            {
                erasedPhantoms = RackCommandSupport.EraseViewBlocks(document, staleViewBlocks);
            }
            else if (staleViewBlocks.Count > 0)
            {
                editor.WriteMessage("\nRackCad: las vistas del sistema ya no corresponden al diseno encogido, pero son las unicas del rack: "
                    + "se conservan para no perderlo. Inserta una vista valida (RACKEDITAR) y borra las viejas a mano si lo deseas.");
            }

            if (updatedFrontal + updatedLateral + updatedPlanta + erasedPhantoms > 0)
            {
                document.Editor.Regen(); // ONE regeneration refreshes every redefined (and drops every erased) view-block
            }

            // "Insertar": after refreshing the existing views above, place a NEW linked view-block (same GUID) of the
            // requested view via the normal insertion path (jig). DrawSelectiveView writes its own outcome. "Actualizar"
            // (UpdateOnly) inserts nothing — the refresh above is the whole action.
            if (!window.UpdateOnly)
            {
                DrawSelectiveView(window.InsertView, system, design, id, name);
                return;
            }

            editor.WriteMessage(updatedFrontal + updatedLateral + updatedPlanta + erasedPhantoms > 0
                ? "\nRackCad: sistema actualizado; sus vistas se redibujaron (frontal x"
                    + updatedFrontal.ToString(CultureInfo.InvariantCulture) + ", lateral x"
                    + updatedLateral.ToString(CultureInfo.InvariantCulture) + ", planta x"
                    + updatedPlanta.ToString(CultureInfo.InvariantCulture) + ")."
                    + (erasedPhantoms > 0 ? " Vistas obsoletas retiradas: x" + erasedPhantoms.ToString(CultureInfo.InvariantCulture) + "." : string.Empty)
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
            => design == null ? null : WrapSelectivePayload(SerializeSelectiveDesign(design, id, name), id, name, view, section);

        /// <summary>The full design serialized once; every view-block carries this SAME JSON (see <see cref="WrapSelectivePayload"/>).</summary>
        private static string SerializeSelectiveDesign(SelectivePalletDesign design, string id, string name)
            => design == null ? null : new SelectivePalletDesignStore().Serialize(SelectivePalletDesignDocument.From(design, id, name));

        /// <summary>Wraps an ALREADY-serialized design in the per-view embed envelope — multi-view redraws reuse one
        /// design JSON instead of re-serializing the whole design per view-block.</summary>
        private static string WrapSelectivePayload(string designJson, string id, string name, string view, int section = -1)
        {
            if (string.IsNullOrEmpty(designJson))
            {
                return null;
            }

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
        internal static void DrawSelectiveView(string view, SelectiveRackSystem system, SelectivePalletDesign design, string id, string name)
        {
            var document = AcApplication.DocumentManager.MdiActiveDocument;

            if (document == null || system == null)
            {
                return;
            }

            system.Name = name; // the "Colocar nombre de rack" annotation draws this

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

            InsertSelectiveFrontal(document, system, design, id, name);
        }

        /// <summary>
        /// Inserts ONE frontal face, chosen by fondo number (a doble-profundidad rack has a frontal per fondo — each
        /// back-to-back side its own elevation). Single-fondo racks skip the prompt (fondo 0). The block carries the
        /// SAME rack id + full design (View=frontal, Section=fondo), so RACKEDITAR on it reopens the whole system and
        /// redraws every view.
        /// </summary>
        private static void InsertSelectiveFrontal(Document document, SelectiveRackSystem system, SelectivePalletDesign design, string id, string name)
        {
            if (document == null || system == null)
            {
                return;
            }

            var editor = document.Editor;
            var fondoCount = SelectiveDepthLayout.Count(system);

            var fondo = 0;
            if (fondoCount > 1)
            {
                // Sin acentos en los mensajes de linea de comandos (evita mojibake en consolas no-Unicode).
                var options = new PromptIntegerOptions("\nQue frontal insertar (numero de fondo)?")
                {
                    LowerLimit = 1,
                    UpperLimit = fondoCount,
                    DefaultValue = 1,
                    UseDefaultValue = true,
                    AllowNone = false
                };

                var pick = editor.GetInteger(options);
                if (pick.Status != PromptStatus.OK)
                {
                    return;
                }

                fondo = pick.Value - 1;
            }

            var fondoView = SelectiveDepthLayout.FondoSystemView(system, fondo);
            fondoView.Name = name;
            var payload = BuildSelectivePayload(design, id, name, RackEmbedDocument.ViewFrontal, fondo);
            var blockName = FrontalName(string.IsNullOrWhiteSpace(name) ? "Selectivo" : name.Trim(), fondo, fondoCount);
            var result = new SelectiveFrontalDrawService().DrawAndPlace(document, fondoView, payload, blockName);
            document.Editor.WriteMessage("\n" + DescribeSelective(result));
        }

        /// <summary>Block/definition name for a fondo's frontal: the base name, plus a "frente F{n}" suffix only when the rack has more than one fondo.</summary>
        private static string FrontalName(string baseName, int fondo, int fondoCount)
        {
            if (string.IsNullOrWhiteSpace(baseName))
            {
                return baseName;
            }

            return fondoCount > 1 ? baseName + " - frente F" + (fondo + 1).ToString(CultureInfo.InvariantCulture) : baseName;
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

            // Ask WHICH post's corte to insert (1-based, matching the frontal preview numbers). The lateral spans the
            // MASTER grid — a corner layout has more cortes than fondo 0's frentes — so bound the pick by the cortes.
            var postCount = 1;
            foreach (var c in cortes)
            {
                if (c.PostIndex + 1 > postCount) postCount = c.PostIndex + 1;
            }
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
            => RackCommandSupport.DescribePlacement(result, "el selectivo", "selectivo insertado");
    }
}
