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
    /// system of the write (<c>PushBackDoorRules.Named</c>): this file never writes a name. From the preflight to the end the body is the one
    /// of <c>EditPushBack</c>, with four declared differences; <c>PushBackWindowDrawingMirrorGuardTests</c> ties the two (review of 24adfee7).
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
                    return (0, 0);
                }
            }

            // Inner-project preflight (I-11): an incompatible-MAJOR or wrong-kind inner design aborts the whole edit before
            // any geometry change. This MUST run before the first RedrawInPlace.
            var preflight = RackCommandSupport.PreflightInnerSources(blocks, RackSystemKind.PushBack, project);
            if (preflight.Aborted)
            {
                editor.WriteMessage("\nRackCad: " + preflight.ErrorMessage);
                return (0, 0);
            }

            // View-descriptor preflight: validate EVERY envelope's (view, section) up front. A corrupt descriptor is NEVER
            // silently coerced into another view — it aborts before geometry is touched.
            foreach (var viewBlock in blocks)
            {
                if (!IsValidPushBackDescriptor(viewBlock.Embed))
                {
                    editor.WriteMessage("\nRackCad: una vista ligada de este rack tiene un descriptor de vista invalido (posible corrupcion). No se modifico ningun bloque.");
                    return (0, 0);
                }
            }

            // I-05: a NEW linked view is inserted only in the "!UpdateOnly" branch; warn once if the drawing is not in
            // inches BEFORE any block is (re)drawn. A pure update must NOT warn.
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
            var staleViewBlocks = new System.Collections.Generic.List<ObjectId>();
            var catalog = LateralHeaderDrawService.LoadCatalog();
            var lateralCortes = new PushBackSystemLateralBuilder().Cortes(system, catalog);
            var availabilityFacts = new PushBackViewAvailabilityFacts(
                lateralCortes.Select(item => item.PostIndex), system.IsComposite);
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
                    // T6 b1 (point 6 of the review of dc0ada4c): a frontal cut of side B of a rack that is no longer two-way is stale, as a
                    // lateral of a post that no longer exists; it is never redrawn with the geometry of side A under the descriptor of B.
                    if (RackViewAvailability.Evaluate(decoded, availabilityFacts).Status != RackViewAvailabilityStatus.Available)
                    {
                        staleViewBlocks.Add(viewBlock.BlockId);
                        continue;
                    }

                    var end = decoded.Address.Variant.PushBackEnd == RackPushBackEnd.Posterior
                        ? PushBackFrontalEnd.Posterior
                        : PushBackFrontalEnd.EntradaSalida;
                    var side = decoded.Address.Variant.PushBackSide == RackPushBackSide.B
                        ? PushBackSide.B
                        : PushBackSide.A;
                    // I-42: la seccion se RE-ESCRIBE tal cual venia (corte + lado), no como el ordinal del corte:
                    // en un rack compuesto perder el lado mandaria los dos cortes de B al descriptor de A.
                    var payload = RackPushBackCommands.BuildPushBackPayload(design, id, name, RackEmbedDocument.ViewFrontal, viewBlock.Embed.Section, viewBlock.Embed, preflight.ResolvedByBlock[viewBlock.BlockId]);
                    result = new PushBackFrontalDrawService().RedrawInPlace(document, viewBlock.BlockId, system, end, payload, regen: false, side: side);
                    if (result != null && result.Success)
                    {
                        RackBlockRenamer.SyncName(
                            document,
                            viewBlock.BlockId,
                            RackViewBaseName.LinkedPushBackCut(
                                baseName,
                                end == PushBackFrontalEnd.Posterior
                                    ? RackPushBackEnd.Posterior
                                    : RackPushBackEnd.EntradaSalida,
                                side == PushBackSide.B ? RackPushBackSide.B : RackPushBackSide.A,
                                system != null && system.IsComposite));
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
                        RackBlockRenamer.SyncName(
                            document,
                            viewBlock.BlockId,
                            RackViewBaseName.LinkedLateral(baseName, postIndex));
                        updatedLateral++;
                    }
                }
            }

            // Stale lateral cuts: erase ONLY when some survive (never delete a rack's last remaining link).
            var survivors = blocks.Count - staleViewBlocks.Count;
            var erasedPhantoms = staleViewBlocks.Count > 0 && survivors > 0
                ? RackCommandSupport.EraseViewBlocks(document, staleViewBlocks)
                : 0;
            // T6 b1: with «Insertar» the obsolete views of a rack with no survivor wait for the first new view (RackInsertRedrawOutcome).
            if (staleViewBlocks.Count > 0 && survivors == 0 && updateOnly)
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

            if (!updateOnly)
            {
                // T6 b1 (I-02 of the review of B-1): the queue places the new views only when every view that remains was redrawn.
                var batch = RackViewBatchExecution.Run(document, batchProducts, batchRequest,
                    _ => RackCad.Application.Views.Batch.RackInsertRedrawOutcome.Of(
                        blocks.Count, staleViewBlocks.Count, updatedLateral + updatedFrontal + updatedPlanta, erasedPhantoms));
                if (RackCad.Application.Views.Batch.RackInsertRedrawOutcome.ErasesStaleAfterPlacement(blocks.Count, staleViewBlocks.Count))
                {
                    erasedPhantoms = batch.Report.PlacedCount > 0 ? RackCommandSupport.EraseViewBlocks(document, staleViewBlocks) : 0;
                    editor.WriteMessage("\nRackCad: " + RackCad.Application.Views.Batch.RackInsertRedrawOutcome.StaleAfterPlacementText(
                        staleViewBlocks.Count, erasedPhantoms, batch.Report.PlacedCount));
                }

                return (updatedLateral + updatedFrontal + updatedPlanta, erasedPhantoms);
            }

            editor.WriteMessage(changedInPlace
                ? "\nRackCad: sistema Push Back actualizado desde la ventana RackCad; vistas redibujadas (lateral x"
                    + updatedLateral.ToString(CultureInfo.InvariantCulture) + ", frontal entrada/salida x"
                    + updatedFrontalEntrada.ToString(CultureInfo.InvariantCulture) + ", frontal posterior x"
                    + updatedFrontalPosterior.ToString(CultureInfo.InvariantCulture) + ", planta x"
                    + updatedPlanta.ToString(CultureInfo.InvariantCulture) + "; cortes obsoletos eliminados x"
                    + erasedPhantoms.ToString(CultureInfo.InvariantCulture) + ")."
                : "\nRackCad: no se pudo actualizar el sistema Push Back.");
            return (updatedLateral + updatedFrontal + updatedPlanta, erasedPhantoms);
        }

        /// <summary>The same check of the descriptor of a view as <c>RackPushBackCommands.IsValidPushBackDescriptor</c> (a guard ties the two).</summary>
        private static bool IsValidPushBackDescriptor(RackEmbedDocument embed)
        {
            var decoded = RackCommandSupport.DecodeView(embed);
            return decoded.HasAddress && decoded.SystemKind == RackSystemKind.PushBack;
        }
    }
}
