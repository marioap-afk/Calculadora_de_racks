using System;
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
using RackCad.UI.Editor;
using AcApplication = Autodesk.AutoCAD.ApplicationServices.Application;

namespace RackCad.Plugin
{
    /// <summary>
    /// Push Back (I-18) system command + its draw/edit/payload helpers, plus the RPB alias. It opens the pure Push Back
    /// editor, then draws whatever <see cref="PushBackInsertionRequest"/> the window's session produced, using ONLY the
    /// I-18a SystemPlan builders (via the three Push Back draw services). No geometry lives here. The RACKEDITAR
    /// multi-view round-trip is <see cref="EditPushBack"/> below; <c>PushBackKindHandler</c> only forwards to it.
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

        /// <summary>
        /// RACKEDITAR round-trip for a Push Back rack: reopen the editor with the picked block's data, then redraw EVERY
        /// linked view in place (and optionally insert one more). The single Push Back edit implementation — the kind
        /// handler only forwards here. It reuses the window/session as the sole recompute authority (never re-resolves a
        /// system the window already produced), preserves the picked GUID (never mints a new one on an edit), and runs the
        /// full preflight (envelope kind+GUID, inner-project I-11, view descriptor) BEFORE touching any geometry.
        /// </summary>
        internal static void EditPushBack(Document document, ObjectId blockId, RackEmbedDocument embed)
        {
            var editor = document.Editor;

            RackProject project;
            try
            {
                project = new RackProjectStore().Deserialize(embed.Design);
            }
            catch (System.Exception ex)
            {
                editor.WriteMessage("\nRackCad: no se pudieron leer los datos del sistema Push Back. " + ex.Message);
                return;
            }

            if (project?.PushBackDesign == null)
            {
                editor.WriteMessage("\nRackCad: datos de sistema Push Back invalidos.");
                return;
            }

            var window = new RackPushBackSystemWindow(canInsertInAutoCad: true);
            window.LoadExisting(project.PushBackDesign, embed.Id, embed.Name, project);
            AcApplication.ShowModalWindow(window);

            if (!window.InsertRequested)
            {
                return;
            }

            // Use ONLY what the window/session produced; do NOT re-resolve a system the window already gave us.
            var design = window.DesignToInsert;
            var system = window.SystemToInsert;
            if (design == null || system == null)
            {
                editor.WriteMessage("\nRackCad: la ventana Push Back no entrego un sistema valido; no se modifico nada.");
                return;
            }

            // Identity: keep the picked envelope's GUID; the window GUID is only a fallback. NEVER mint a new GUID on edit.
            var id = string.IsNullOrWhiteSpace(embed.Id) ? window.RackId : embed.Id;
            var name = string.IsNullOrWhiteSpace(window.RackName) ? embed.Name : window.RackName;
            var baseName = string.IsNullOrWhiteSpace(name) ? null : name.Trim();
            system.Name = name;

            var blocks = RackCommandSupport.FindRackBlocks(document, id);
            if (blocks.Count == 0)
            {
                blocks.Add((blockId, embed));
            }

            // --- PREFLIGHT: everything below runs BEFORE the first RedrawInPlace / rename / erase / regen ---

            // Envelope preflight: every linked view must be a non-null Push Back envelope of THIS same GUID. A different
            // kind, a foreign GUID or a corrupt envelope aborts the WHOLE edit with a visible message (no partial update).
            foreach (var viewBlock in blocks)
            {
                if (viewBlock.Embed == null
                    || !string.Equals(viewBlock.Embed.Kind, RackEmbedDocument.KindPushBack, StringComparison.OrdinalIgnoreCase)
                    || !string.Equals(viewBlock.Embed.Id, id, StringComparison.OrdinalIgnoreCase))
                {
                    editor.WriteMessage("\nRackCad: una vista ligada de este rack no es Push Back o pertenece a otro sistema (posible corrupcion). No se modifico ningun bloque.");
                    return;
                }
            }

            // Inner-project preflight (I-11): an incompatible-MAJOR or wrong-kind inner design aborts the whole edit before
            // any geometry change. This MUST run before the first RedrawInPlace.
            var preflight = RackCommandSupport.PreflightInnerSources(blocks, RackSystemKind.PushBack, project);
            if (preflight.Aborted)
            {
                editor.WriteMessage("\nRackCad: " + preflight.ErrorMessage);
                return;
            }

            // View-descriptor preflight: validate EVERY envelope's (view, section) up front. A corrupt descriptor is NEVER
            // silently coerced into another view — it aborts before geometry is touched.
            foreach (var viewBlock in blocks)
            {
                if (!IsValidPushBackDescriptor(viewBlock.Embed))
                {
                    editor.WriteMessage("\nRackCad: una vista ligada de este rack tiene un descriptor de vista invalido (posible corrupcion). No se modifico ningun bloque.");
                    return;
                }
            }

            // I-05: a NEW linked view is inserted only in the "!UpdateOnly" branch; warn once if the drawing is not in
            // inches BEFORE any block is (re)drawn. A pure update must NOT warn.
            if (!window.UpdateOnly)
            {
                RackUnitsGuard.WarnIfNotInches(document);
            }

            // --- Multiview redraw: catalog + lateral cuts computed ONCE, geometry never recomputed here ---
            var updatedLateral = 0;
            var updatedFrontalEntrada = 0;
            var updatedFrontalPosterior = 0;
            var updatedPlanta = 0;
            var staleViewBlocks = new System.Collections.Generic.List<ObjectId>();
            var catalog = LateralHeaderDrawService.LoadCatalog();
            var lateralCortes = new PushBackSystemLateralBuilder().Cortes(system, catalog);
            foreach (var viewBlock in blocks)
            {
                HeaderPlacementResult result;
                if (RackCommandSupport.IsPlantaView(viewBlock.Embed))
                {
                    var payload = BuildPushBackPayload(design, id, name, RackEmbedDocument.ViewPlanta, -1, viewBlock.Embed, preflight.ResolvedByBlock[viewBlock.BlockId]);
                    result = new PushBackPlantaDrawService().RedrawInPlace(document, viewBlock.BlockId, system, payload, regen: false);
                    if (result != null && result.Success)
                    {
                        RackBlockRenamer.SyncName(document, viewBlock.BlockId, baseName == null ? null : baseName + " - planta");
                        updatedPlanta++;
                    }
                }
                else if (IsPushBackFrontal(viewBlock.Embed))
                {
                    var end = viewBlock.Embed.Section == (int)PushBackFrontalEnd.Posterior ? PushBackFrontalEnd.Posterior : PushBackFrontalEnd.EntradaSalida;
                    var payload = BuildPushBackPayload(design, id, name, RackEmbedDocument.ViewFrontal, (int)end, viewBlock.Embed, preflight.ResolvedByBlock[viewBlock.BlockId]);
                    result = new PushBackFrontalDrawService().RedrawInPlace(document, viewBlock.BlockId, system, end, payload, regen: false);
                    if (result != null && result.Success)
                    {
                        var suffix = end == PushBackFrontalEnd.Posterior ? " - frontal posterior" : " - frontal entrada-salida";
                        RackBlockRenamer.SyncName(document, viewBlock.BlockId, baseName == null ? null : baseName + suffix);
                        if (end == PushBackFrontalEnd.Posterior)
                        {
                            updatedFrontalPosterior++;
                        }
                        else
                        {
                            updatedFrontalEntrada++;
                        }
                    }
                }
                else
                {
                    // Lateral: the descriptor preflight guarantees View == ViewLateral && Section >= 0. Redraw the cut for
                    // THIS post index; a post that no longer exists becomes stale (never redraw it at another index).
                    var postIndex = viewBlock.Embed.Section;
                    var corte = lateralCortes.FirstOrDefault(item => item.PostIndex == postIndex);
                    if (corte == null)
                    {
                        staleViewBlocks.Add(viewBlock.BlockId);
                        continue;
                    }

                    var payload = BuildPushBackPayload(design, id, name, RackEmbedDocument.ViewLateral, postIndex, viewBlock.Embed, preflight.ResolvedByBlock[viewBlock.BlockId]);
                    result = new PushBackSystemDrawService().RedrawInPlace(document, viewBlock.BlockId, system, payload, regen: false, postIndex: postIndex);
                    if (result != null && result.Success)
                    {
                        RackBlockRenamer.SyncName(
                            document,
                            viewBlock.BlockId,
                            baseName == null ? null : baseName + " - lateral " + (postIndex + 1).ToString(CultureInfo.InvariantCulture));
                        updatedLateral++;
                    }
                }
            }

            // Stale lateral cuts: erase ONLY when some survive (never delete a rack's last remaining link).
            var survivors = blocks.Count - staleViewBlocks.Count;
            var erasedPhantoms = staleViewBlocks.Count > 0 && survivors > 0
                ? RackCommandSupport.EraseViewBlocks(document, staleViewBlocks)
                : 0;
            if (staleViewBlocks.Count > 0 && survivors == 0)
            {
                editor.WriteMessage("\nRackCad: todas las vistas Push Back quedaron obsoletas; no se elimino el ultimo vinculo del rack.");
            }

            // SINGLE authority for "the drawing changed in place": erasing a stale cut IS a change, so an edit that only
            // dropped obsolete cuts still regens AND still reports success with the full counts. The regen and the final
            // message must never disagree. Exactly one explicit batch regen (each RedrawInPlace used regen:false); a NEW
            // view inserted below regens through ViewBlockDraw.DrawAndPlace, so no extra manual regen wraps it.
            var updatedFrontal = updatedFrontalEntrada + updatedFrontalPosterior;
            var changedInPlace = updatedLateral + updatedFrontal + updatedPlanta + erasedPhantoms > 0;
            if (changedInPlace)
            {
                document.Editor.Regen();
            }

            if (!window.UpdateOnly)
            {
                // The NEW view inherits the picked (initiating) envelope AND the inner wrapper (I-11): same GUID, current
                // name, unknown envelope metadata + unknown/non-degraded inner-project version.
                DrawPushBackView(window.InsertView, window.InsertSection, system, design, id, name, source: embed, innerSource: project);
                return;
            }

            editor.WriteMessage(changedInPlace
                ? "\nRackCad: sistema Push Back actualizado; vistas redibujadas (lateral x"
                    + updatedLateral.ToString(CultureInfo.InvariantCulture) + ", frontal entrada/salida x"
                    + updatedFrontalEntrada.ToString(CultureInfo.InvariantCulture) + ", frontal posterior x"
                    + updatedFrontalPosterior.ToString(CultureInfo.InvariantCulture) + ", planta x"
                    + updatedPlanta.ToString(CultureInfo.InvariantCulture) + "; cortes obsoletos eliminados x"
                    + erasedPhantoms.ToString(CultureInfo.InvariantCulture) + ")."
                : "\nRackCad: no se pudo actualizar el sistema Push Back.");
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
            // Pass the BASE name only; PushBackSystemDrawService.BlockName is the single authority for the lateral-section suffix.
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
            // project); WithSourceMetadataFrom preserves its unknown fields + non-downgraded version. Never the dynamic kind.
            var designJson = new RackProjectStore().Serialize(RackProject.ForPushBack(design).WithSourceMetadataFrom(innerSource));
            var embed = RackEmbedComposer.Compose(
                source, RackEmbedDocument.KindPushBack, id, name,
                string.IsNullOrWhiteSpace(view) ? RackEmbedDocument.ViewLateral : view, section, designJson);
            return new RackEmbedStore().Serialize(embed);
        }

        private static bool IsPushBackFrontal(RackEmbedDocument embed)
            => embed != null && string.Equals(embed.View, RackEmbedDocument.ViewFrontal, StringComparison.OrdinalIgnoreCase);

        /// <summary>True when the envelope carries a well-formed Push Back view descriptor: planta (section -1), frontal
        /// (section exactly EntradaSalida/Posterior) or lateral (section >= 0, a post index). Anything else is corrupt and
        /// aborts the edit — it is never coerced into another view.</summary>
        private static bool IsValidPushBackDescriptor(RackEmbedDocument embed)
        {
            if (embed == null)
            {
                return false;
            }

            if (string.Equals(embed.View, RackEmbedDocument.ViewPlanta, StringComparison.OrdinalIgnoreCase))
            {
                return embed.Section == -1;
            }

            if (string.Equals(embed.View, RackEmbedDocument.ViewFrontal, StringComparison.OrdinalIgnoreCase))
            {
                return embed.Section == (int)PushBackFrontalEnd.EntradaSalida
                    || embed.Section == (int)PushBackFrontalEnd.Posterior;
            }

            if (string.Equals(embed.View, RackEmbedDocument.ViewLateral, StringComparison.OrdinalIgnoreCase))
            {
                return embed.Section >= 0;
            }

            return false;
        }
    }
}
