using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using RackCad.Application.Persistence;
using RackCad.UI;
using AcApplication = Autodesk.AutoCAD.ApplicationServices.Application;

namespace RackCad.Plugin
{
    /// <summary>RACKLISTA / RACKBOMTOTAL: cross-type inventory of every rack in the drawing, with zoom-to on pick; plus their aliases.</summary>
    public sealed partial class RackInventarioCommands
    {
        [CommandMethod("RL")]  public void AliasRackLista() => RackLista();                // RACKLISTA

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
                var embeds = new List<RackEmbedDocument>();
                var blocksByRack = new Dictionary<string, List<(ObjectId BlockId, RackEmbedDocument Embed)>>(StringComparer.OrdinalIgnoreCase);
                var copiesByRack = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

                List<RackEnvelopeScan> envelopes;
                using (document.LockDocument())
                using (var transaction = document.Database.TransactionManager.StartTransaction())
                {
                    envelopes = RackBlockFinder.ScanEnvelopes(transaction, document.Database, includeReferenceCount: true);
                    transaction.Commit();
                }

                foreach (var envelope in envelopes)
                {
                    var embed = envelope.Embed;
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

                    blocks.Add((envelope.DefinitionId, embed));

                    // Copies = PHYSICAL placements of the rack: the MAX reference count across its view-blocks,
                    // the same aggregation RACKBOMTOTAL uses. Summing instead counted every view as a copy (a rack
                    // with frontal + 2 cortes + planta showed "4" here while the BOM said 1). forceValidity stays
                    // FALSE: the true variant revalidates every reference and is expensive on large drawings.
                    copiesByRack[embed.Id] = Math.Max(copiesByRack[embed.Id], envelope.DirectReferenceCount);
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
                RackCommandSupport.Report(ex);
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
                    var reference = RackBlockFinder.FindFirstModelSpaceReference(transaction, document.Database, blocks);
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
    }
}
