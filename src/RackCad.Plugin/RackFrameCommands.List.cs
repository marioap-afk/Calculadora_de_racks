using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using RackCad.Application.Persistence;
using RackCad.Plugin.Systems;
using RackCad.UI;
using AcApplication = Autodesk.AutoCAD.ApplicationServices.Application;

namespace RackCad.Plugin
{
    /// <summary>RACKLISTA: cross-type inventory of every rack in the drawing, with zoom-to on pick.</summary>
    public sealed partial class RackFrameCommands
    {
        /// <summary>
        /// Tabulates every rack in the drawing (name, type, views present, placed copies) by scanning all block
        /// definitions for embed envelopes, and zooms to the chosen rack's first model-space reference.
        /// </summary>
        [CommandMethod("RACKLISTA")]
        public void RackLista()
        {
            try
            {
                var document = AcApplication.DocumentManager.MdiActiveDocument;
                if (document == null)
                {
                    return;
                }

                var editor = document.Editor;

                // One pass over the block table gathers everything the window needs: the envelopes (grouped into
                // rows by RackListBuilder), each rack's view-block ids (for the zoom) and its placed-copy count.
                var store = new RackEmbedStore();
                var embeds = new List<RackEmbedDocument>();
                var blocksByRack = new Dictionary<string, List<(ObjectId BlockId, RackEmbedDocument Embed)>>(StringComparer.OrdinalIgnoreCase);
                var copiesByRack = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

                using (document.LockDocument())
                using (var transaction = document.Database.TransactionManager.StartTransaction())
                {
                    var blockTable = (BlockTable)transaction.GetObject(document.Database.BlockTableId, OpenMode.ForRead);
                    foreach (ObjectId id in blockTable)
                    {
                        var record = (BlockTableRecord)transaction.GetObject(id, OpenMode.ForRead);
                        if (record.IsLayout || record.IsAnonymous || record.IsFromExternalReference)
                        {
                            continue;
                        }

                        var json = RackBlockData.Read(transaction, id);
                        if (string.IsNullOrEmpty(json))
                        {
                            continue;
                        }

                        var embed = store.Deserialize(json);
                        if (embed == null || string.IsNullOrWhiteSpace(embed.Id) || string.IsNullOrWhiteSpace(embed.Kind))
                        {
                            continue;
                        }

                        embeds.Add(embed);
                        if (!blocksByRack.TryGetValue(embed.Id, out var blocks))
                        {
                            blocks = new List<(ObjectId, RackEmbedDocument)>();
                            blocksByRack[embed.Id] = blocks;
                            copiesByRack[embed.Id] = 0;
                        }

                        blocks.Add((id, embed));

                        // Copies = placed references of the rack's view-blocks. forceValidity stays FALSE: the true
                        // variant revalidates every reference and is expensive on large drawings.
                        copiesByRack[embed.Id] += record.GetBlockReferenceIds(directOnly: true, forceValidity: false).Count;
                    }

                    transaction.Commit();
                }

                var entries = RackListBuilder.Build(embeds);
                if (entries.Count == 0)
                {
                    editor.WriteMessage("\nRackCad: no hay racks en el dibujo.");
                    return;
                }

                var rows = entries
                    .Select(entry => new RackListRow(entry.Id, entry.Name, entry.KindLabel, entry.ViewsLabel,
                        copiesByRack.TryGetValue(entry.Id, out var copies) ? copies : 0))
                    .ToList();

                var window = new RackListWindow(rows);
                AcApplication.ShowModalWindow(window);

                if (window.SelectedEntry != null && blocksByRack.TryGetValue(window.SelectedEntry.Id, out var rackBlocks))
                {
                    ZoomToRack(document, rackBlocks);
                }
            }
            catch (System.Exception ex)
            {
                Report(ex);
            }
        }

        /// <summary>
        /// Zoom the current view to the rack's first model-space reference, preferring the frontal view-block.
        /// Extents can be stale/invalid on degenerate blocks, so failures fall back to a CLI message.
        /// </summary>
        private static void ZoomToRack(Document document, List<(ObjectId BlockId, RackEmbedDocument Embed)> blocks)
        {
            var editor = document.Editor;
            try
            {
                using (document.LockDocument())
                using (var transaction = document.Database.TransactionManager.StartTransaction())
                {
                    var reference = FindFirstModelSpaceReference(transaction, document.Database, blocks);
                    if (reference == null)
                    {
                        editor.WriteMessage("\nRackCad: el rack no tiene copias insertadas en el modelo.");
                        return;
                    }

                    var extents = reference.GeometricExtents; // throws on invalid geometry — caught below

                    // GetCurrentView hands back a non-resident clone: recentre it on the rack with ~10% margin and
                    // push it back. All racks draw in the XY plane, so the usual top-down plan view is assumed.
                    using (var view = editor.GetCurrentView())
                    {
                        view.CenterPoint = new Point2d(
                            (extents.MinPoint.X + extents.MaxPoint.X) / 2.0,
                            (extents.MinPoint.Y + extents.MaxPoint.Y) / 2.0);
                        view.Width = Math.Max((extents.MaxPoint.X - extents.MinPoint.X) * 1.10, 1.0);
                        view.Height = Math.Max((extents.MaxPoint.Y - extents.MinPoint.Y) * 1.10, 1.0);
                        editor.SetCurrentView(view);
                    }

                    transaction.Commit();
                }
            }
            catch (System.Exception ex)
            {
                editor.WriteMessage("\nRackCad: no se pudo hacer zoom al rack. " + ex.Message);
            }
        }

        /// <summary>First model-space reference among the rack's view-blocks, frontal first (scan order otherwise).</summary>
        private static BlockReference FindFirstModelSpaceReference(Transaction transaction, Database database, List<(ObjectId BlockId, RackEmbedDocument Embed)> blocks)
        {
            var modelSpaceId = SymbolUtilityServices.GetBlockModelSpaceId(database);
            var ordered = blocks.OrderBy(block =>
                string.Equals(block.Embed.View, RackEmbedDocument.ViewFrontal, StringComparison.OrdinalIgnoreCase) ? 0 : 1);

            foreach (var block in ordered)
            {
                var record = (BlockTableRecord)transaction.GetObject(block.BlockId, OpenMode.ForRead);
                foreach (ObjectId referenceId in record.GetBlockReferenceIds(directOnly: true, forceValidity: false))
                {
                    var reference = (BlockReference)transaction.GetObject(referenceId, OpenMode.ForRead);
                    if (reference.OwnerId == modelSpaceId)
                    {
                        return reference;
                    }
                }
            }

            return null;
        }
    }
}
