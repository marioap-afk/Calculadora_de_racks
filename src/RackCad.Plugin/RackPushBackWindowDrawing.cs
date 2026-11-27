using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using RackCad.Application.Persistence;
using RackCad.Application.Systems.PushBack;
using RackCad.Application.Systems.Shared;
using RackCad.Domain.Systems.PushBack;
using RackCad.Domain.Systems.Shared;
using RackCad.Plugin.Drawing;
using RackCad.Plugin.Systems.PushBack;
using RackCad.Plugin.Systems.Shared;
using RackCad.Plugin.Views;

namespace RackCad.Plugin
{
    /// <summary>
    /// T5 pb (I-64, fifth batch): the drawing half of «Actualizar» and of «Insertar» of RACKEDITAR for a Push Back, with the draft of the
    /// RackCad window as its input instead of the classic window. It is the same sequence as the half of <c>RackPushBackCommands.EditPushBack</c>
    /// after its window closes, step by step and with the same services: the identity of the picked envelope, the views of the rack (the
    /// authorized batch of AUTH-13 when views are inserted), the preflight of every envelope (kind and identity), of every inner project
    /// (I-11) and of every view descriptor BEFORE any geometry; then every linked view redrawn in place without a regeneration of its own, renamed,
    /// the stale lateral cuts erased when some survive, ONE regeneration, and the new views placed by the batch of ID18.
    ///
    /// <para>
    /// Declared (PB-S06): the half is repeated here, not extracted from <c>EditPushBack</c>, because about twenty source guards of F4B fix
    /// the text of that method; the two are joined when the classic editor is retired (end of I-64). The name of the views comes with the
    /// system of the write (<c>PushBackDoorRules.Named</c>): this file never writes a name.
    /// </para>
    /// </summary>
    internal static class RackPushBackWindowDrawing
    {
        /// <summary>
        /// Redraws every view of the rack with the draft and then, with <paramref name="insertViews"/>, places those views as linked copies.
        /// It answers how many views it redrew and how many stale cuts it erased: zero and zero when it stopped before any geometry.
        /// </summary>
        internal static (int Redrawn, int Erased) Apply(
            Document document, ObjectId blockId, RackEmbedDocument embed, RackProject project, PushBackDesign design, PushBackSystem system,
            string rackId, IReadOnlyList<RackViewAddress> insertViews)
        {
            var editor = document.Editor;
            var updateOnly = insertViews == null || insertViews.Count == 0;
            if (design == null || system == null)
            {
                editor.WriteMessage("\nRackCad: la ventana RackCad no entrego un sistema Push Back valido; no se modifico nada.");
                return (0, 0);
            }

            // Identity: keep the picked envelope's GUID; the one of the request is only a fallback. NEVER mint a new GUID on an edit.
            var id = string.IsNullOrWhiteSpace(embed.Id) ? rackId : embed.Id;
            var name = system.Name;
            var baseName = string.IsNullOrWhiteSpace(name) ? null : name.Trim();
            RackAuthorizedSiblingBatch authorized = null;
            if (!updateOnly)
            {
                if (!RackUnsupportedSiblingInsert.TryAuthorize(document, blockId, embed, id, out authorized)) return (0, 0);
            }

            var blocks = !updateOnly ? authorized.Blocks.ToList() : RackCommandSupport.FindRackBlocks(document, id);
            if (blocks.Count == 0)
            {
                blocks.Add((blockId, embed));
            }

            // --- PREFLIGHT: everything below runs BEFORE the first redraw in place, rename, erase or regeneration ---
            foreach (var viewBlock in blocks)
            {
                if (viewBlock.Embed == null
                    || !string.Equals(viewBlock.Embed.Kind, RackEmbedDocument.KindPushBack, StringComparison.OrdinalIgnoreCase)
                    || !string.Equals(viewBlock.Embed.Id, id, StringComparison.OrdinalIgnoreCase))
                {
                    editor.WriteMessage("\nRackCad: una vista ligada de este rack no es Push Back o pertenece a otro sistema (posible corrupcion). No se modifico ningun bloque.");
                    return (0, 0);
                }
            }

            var preflight = RackCommandSupport.PreflightInnerSources(blocks, RackSystemKind.PushBack, project);
            if (preflight.Aborted)
            {
                editor.WriteMessage("\nRackCad: " + preflight.ErrorMessage);
                return (0, 0);
            }

            foreach (var viewBlock in blocks)
            {
                var descriptor = RackCommandSupport.DecodeView(viewBlock.Embed);
                if (!descriptor.HasAddress || descriptor.SystemKind != RackSystemKind.PushBack)
                {
                    editor.WriteMessage("\nRackCad: una vista ligada de este rack tiene un descriptor de vista invalido (posible corrupcion). No se modifico ningun bloque.");
                    return (0, 0);
                }
            }

            if (!updateOnly)
            {
                RackUnitsGuard.WarnIfNotInches(document);
            }

            RackViewBatchProductSession<PushBackDesign, PushBackSystem, RackCad.Application.Drawing.HeaderRunPlan> batchProducts = null;
            RackCad.Application.Views.Batch.RackViewBatchRequest batchRequest = null;
            if (!updateOnly)
            {
                batchProducts = RackViewBatchProducts.PushBack(document, system, design, id, name, embed, project, authorized.AuthoredInput);
                batchRequest = RackViewBatchProductSession<PushBackDesign, PushBackSystem,
                    RackCad.Application.Drawing.HeaderRunPlan>.Request(
                        RackCad.Application.Views.Preparation.RackProductSourceKind.ExistingRack,
                        RackSystemKind.PushBack, id, insertViews);
                if (!batchProducts.PrepareAll(batchRequest, out var batchDiagnostic))
                {
                    editor.WriteMessage("\nRackCad: no se preparo la cola ID18; no se modifico ningun bloque. " + batchDiagnostic);
                    return (0, 0);
                }
            }

            // --- Multiview redraw: catalog + lateral cuts computed ONCE, geometry never recomputed here ---
            var updatedLateral = 0;
            var updatedFrontalEntrada = 0;
            var updatedFrontalPosterior = 0;
            var updatedPlanta = 0;
            var staleViewBlocks = new List<ObjectId>();
            var catalog = LateralHeaderDrawService.LoadCatalog();
            var lateralCortes = new PushBackSystemLateralBuilder().Cortes(system, catalog);
            var availabilityFacts = new PushBackViewAvailabilityFacts(lateralCortes.Select(item => item.PostIndex), system.IsComposite);
            foreach (var viewBlock in blocks)
            {
                HeaderPlacementResult result;
                var decoded = RackCommandSupport.DecodeView(viewBlock.Embed);
                if (decoded.Address.Kind == DimensionViewKind.Planta)
                {
                    var payload = RackPushBackCommands.BuildPushBackPayload(design, id, name, RackEmbedDocument.ViewPlanta, -1, viewBlock.Embed, preflight.ResolvedByBlock[viewBlock.BlockId]);
                    result = new PushBackPlantaDrawService().RedrawInPlace(document, viewBlock.BlockId, system, payload, regen: false);
                    if (result != null && result.Success)
                    {
                        RackBlockRenamer.SyncName(document, viewBlock.BlockId, RackViewBaseName.LinkedPlanta(baseName));
                        updatedPlanta++;
                    }
                }
                else if (decoded.Address.Kind == DimensionViewKind.Frontal)
                {
                    var end = decoded.Address.Variant.PushBackEnd == RackPushBackEnd.Posterior ? PushBackFrontalEnd.Posterior : PushBackFrontalEnd.EntradaSalida;
                    var side = decoded.Address.Variant.PushBackSide == RackPushBackSide.B ? PushBackSide.B : PushBackSide.A;
                    // I-42: the section is written again as it came (cut + side), never as the ordinal of the cut.
                    var payload = RackPushBackCommands.BuildPushBackPayload(design, id, name, RackEmbedDocument.ViewFrontal, viewBlock.Embed.Section, viewBlock.Embed, preflight.ResolvedByBlock[viewBlock.BlockId]);
                    result = new PushBackFrontalDrawService().RedrawInPlace(document, viewBlock.BlockId, system, end, payload, regen: false, side: side);
                    if (result != null && result.Success)
                    {
                        RackBlockRenamer.SyncName(
                            document,
                            viewBlock.BlockId,
                            RackViewBaseName.LinkedPushBackCut(
                                baseName,
                                end == PushBackFrontalEnd.Posterior ? RackPushBackEnd.Posterior : RackPushBackEnd.EntradaSalida,
                                side == PushBackSide.B ? RackPushBackSide.B : RackPushBackSide.A,
                                system.IsComposite));
                        if (end == PushBackFrontalEnd.Posterior) updatedFrontalPosterior++;
                        else updatedFrontalEntrada++;
                    }
                }
                else
                {
                    // Lateral: a post that no longer exists becomes stale (never redraw it at another index).
                    var postIndex = decoded.Address.Variant.Index;
                    var availability = RackViewAvailability.Evaluate(decoded, availabilityFacts);
                    var corte = availability.Status == RackViewAvailabilityStatus.Available
                        ? lateralCortes.FirstOrDefault(item => item.PostIndex == postIndex)
                        : null;
                    if (corte == null)
                    {
                        staleViewBlocks.Add(viewBlock.BlockId);
                        continue;
                    }

                    var payload = RackPushBackCommands.BuildPushBackPayload(design, id, name, RackEmbedDocument.ViewLateral, postIndex, viewBlock.Embed, preflight.ResolvedByBlock[viewBlock.BlockId]);
                    result = new PushBackSystemDrawService().RedrawInPlace(document, viewBlock.BlockId, system, payload, regen: false, postIndex: postIndex);
                    if (result != null && result.Success)
                    {
                        RackBlockRenamer.SyncName(document, viewBlock.BlockId, RackViewBaseName.LinkedLateral(baseName, postIndex));
                        updatedLateral++;
                    }
                }
            }

            // Stale lateral cuts: erase ONLY when some survive (never delete a rack's last remaining link).
            var survivors = blocks.Count - staleViewBlocks.Count;
            var erased = staleViewBlocks.Count > 0 && survivors > 0 ? RackCommandSupport.EraseViewBlocks(document, staleViewBlocks) : 0;
            if (staleViewBlocks.Count > 0 && survivors == 0)
            {
                editor.WriteMessage("\nRackCad: todas las vistas Push Back quedaron obsoletas; no se elimino el ultimo vinculo del rack.");
            }

            var redrawn = updatedLateral + updatedFrontalEntrada + updatedFrontalPosterior + updatedPlanta;
            if (redrawn + erased > 0)
            {
                document.Editor.Regen();
            }

            if (!updateOnly)
            {
                RackViewBatchExecution.Run(document, batchProducts, batchRequest,
                    _ => RackCad.Application.Views.Batch.RackViewBatchRedrawResult.Applied());
                return (redrawn, erased);
            }

            editor.WriteMessage(redrawn + erased > 0
                ? "\nRackCad: sistema Push Back actualizado desde la ventana RackCad; vistas redibujadas (lateral x"
                    + updatedLateral.ToString(CultureInfo.InvariantCulture) + ", frontal entrada/salida x"
                    + updatedFrontalEntrada.ToString(CultureInfo.InvariantCulture) + ", frontal posterior x"
                    + updatedFrontalPosterior.ToString(CultureInfo.InvariantCulture) + ", planta x"
                    + updatedPlanta.ToString(CultureInfo.InvariantCulture) + "; cortes obsoletos eliminados x"
                    + erased.ToString(CultureInfo.InvariantCulture) + ")."
                : "\nRackCad: no se pudo actualizar el sistema Push Back.");
            return (redrawn, erased);
        }
    }
}
