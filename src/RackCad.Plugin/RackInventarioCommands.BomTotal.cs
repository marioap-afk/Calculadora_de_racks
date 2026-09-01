using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Runtime;
using RackCad.Application.Bom;
using RackCad.Application.Catalogs;
using RackCad.Application.Diagnostics;
using RackCad.Application.Persistence;
using RackCad.Plugin.Drawing;
using RackCad.Plugin.KindHandlers;
using RackCad.UI;
using AcApplication = Autodesk.AutoCAD.ApplicationServices.Application;

namespace RackCad.Plugin
{
    /// <summary>
    /// RACKBOMTOTAL — the whole-drawing bill of materials. Scans every rack block (grouped by GUID like RACKLISTA),
    /// rebuilds each rack's BOM from its embedded design (per kind), and shows a per-rack breakdown + grand total.
    /// </summary>
    public sealed partial class RackInventarioCommands
    {
        [CommandMethod("RB")]  public void AliasRackBomTotal() => RackBomTotal();          // RACKBOMTOTAL

        [CommandMethod("RACKBOMTOTAL")]
        public void RackBomTotal()
        {
            try
            {
                var document = AcApplication.DocumentManager.MdiActiveDocument;
                if (document == null)
                {
                    return;
                }

                var editor = document.Editor;
                // One representative embed per rack GUID (every view-block carries the same full design) + its placement
                // count = the MAX BlockReference count across the rack's view-blocks (a rack copied N times shows N).
                var byRack = new Dictionary<string, RackAggregate>(StringComparer.OrdinalIgnoreCase);
                var order = new List<string>();

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

                    var copies = envelope.DirectReferenceCount;
                    if (!byRack.TryGetValue(embed.Id, out var aggregate))
                    {
                        byRack[embed.Id] = new RackAggregate { Embed = embed, Copies = copies };
                        order.Add(embed.Id);
                    }
                    else if (copies > aggregate.Copies)
                    {
                        aggregate.Copies = copies;
                    }
                }

                if (byRack.Count == 0)
                {
                    editor.WriteMessage("\nRackCad: no hay racks en el dibujo para listar.");
                    return;
                }

                // Only PLACED racks matter (a defined-but-unplaced rack was drawn then erased, not yet purged).
                var placed = order.Select(id => byRack[id]).Where(aggregate => aggregate.Copies > 0).ToList();

                // Preflight: EVERY placed rack must resolve a handler BEFORE we load the catalog or build anything.
                // If ANY placed kind has no registered handler, abort the WHOLE command — a partial BOM must never be
                // shown. (A KNOWN handler whose payload turns out unreadable is still best-effort skipped below.)
                if (!KindHandlerDispatch.TryResolveAll(editor, placed.Select(aggregate => aggregate.Embed.Kind).ToList(), out var handlers))
                {
                    return;
                }

                var catalog = LateralHeaderDrawService.LoadCatalog();

                // I-42 (A1C/H11, contrato del dueño) — un diseño BLOQUEADO no entra al listado, y no se calla.
                //
                // El editor ya impide insertar, actualizar y cotizar un rack cuyo diagnostico es bloqueante; este
                // comando recorria los mismos diseños sin ese filtro y cotizaba, por ejemplo, un rack cuya cama pide
                // mas longitud de la que su estructura tiene. Se aborta el TOTAL —igual que cuando un kind no tiene
                // handler— porque un total al que le falta un rack no puede parecer completo, y se nombra cada rack
                // con el motivo que redacto su propia autoridad de diagnosticos.
                var blocked = new List<KeyValuePair<string, string>>();
                for (var i = 0; i < placed.Count; i++)
                {
                    var reason = handlers[i].OutputBlockedReason(placed[i].Embed, catalog);
                    if (!string.IsNullOrWhiteSpace(reason))
                    {
                        blocked.Add(new KeyValuePair<string, string>(placed[i].Embed.Name, reason));
                    }
                }

                if (blocked.Count > 0)
                {
                    editor.WriteMessage("\n" + RackBomOutputGate.DescribeBlocked(blocked));
                    return;
                }

                var racks = new List<ConsolidatedRackBom>();
                for (var i = 0; i < placed.Count; i++)
                {
                    var aggregate = placed[i];
                    var bom = BuildRackBom(handlers[i], aggregate.Embed, catalog);
                    if (bom == null)
                    {
                        // Payload ilegible: se salta, pero NUNCA en silencio — el listado que sale es mas corto y
                        // el usuario tiene que saber cual falta (I-42/H11).
                        editor.WriteMessage("\n" + RackBomOutputGate.DescribeUnreadable(aggregate.Embed.Name));
                        continue;
                    }

                    racks.Add(new ConsolidatedRackBom
                    {
                        Name = string.IsNullOrWhiteSpace(aggregate.Embed.Name) ? "(sin nombre)" : aggregate.Embed.Name.Trim(),
                        Kind = handlers[i].BomLabel,
                        Copies = aggregate.Copies,
                        Bom = bom
                    });
                }

                if (racks.Count == 0)
                {
                    editor.WriteMessage("\nRackCad: no se pudo interpretar ningun rack del dibujo.");
                    return;
                }

                var consolidated = ConsolidatedBomBuilder.Build(racks);
                AcApplication.ShowModalWindow(new RackConsolidatedBomWindow(consolidated));
            }
            catch (System.Exception ex)
            {
                RackCommandSupport.Report(ex);
            }
        }

        /// <summary>Rebuild ONE rack's bill of materials via its already-resolved handler (the caller reported an
        /// unrecognized kind before reaching here). Returns null for an unreadable/unusable payload, so the caller
        /// best-effort skips that rack.</summary>
        private static BillOfMaterials BuildRackBom(IRackKindHandler handler, RackEmbedDocument embed, RackCatalog catalog)
        {
            try
            {
                return handler.BuildBom(embed, catalog);
            }
            catch (System.Exception ex)
            {
                RackLog.Exception("Construir BOM de un rack (payload ilegible)", ex);
                return null;
            }
        }

        private sealed class RackAggregate
        {
            public RackEmbedDocument Embed { get; set; }
            public int Copies { get; set; }
        }
    }
}
