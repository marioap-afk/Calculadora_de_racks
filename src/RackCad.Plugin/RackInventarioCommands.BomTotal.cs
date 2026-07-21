using System;
using System.Collections.Generic;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Runtime;
using RackCad.Application.Bom;
using RackCad.Application.Catalogs;
using RackCad.Application.Persistence;
using RackCad.Application.Systems;
using RackCad.Plugin.Headers;
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

                var catalog = LateralHeaderDrawService.LoadCatalog();
                var racks = new List<ConsolidatedRackBom>();
                foreach (var id in order)
                {
                    var aggregate = byRack[id];
                    if (aggregate.Copies <= 0)
                    {
                        continue; // a defined-but-unplaced rack (drawn then erased, not yet purged) isn't in the drawing
                    }

                    var bom = BuildRackBom(aggregate.Embed, catalog);
                    if (bom == null)
                    {
                        continue; // a foreign/unreadable rack is skipped, not fatal
                    }

                    racks.Add(new ConsolidatedRackBom
                    {
                        Name = string.IsNullOrWhiteSpace(aggregate.Embed.Name) ? "(sin nombre)" : aggregate.Embed.Name.Trim(),
                        Kind = KindLabel(aggregate.Embed.Kind),
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

        /// <summary>Rebuild ONE rack's bill of materials from its embedded design, dispatching on kind (mirrors RACKEDITAR).</summary>
        private static BillOfMaterials BuildRackBom(RackEmbedDocument embed, RackCatalog catalog)
        {
            try
            {
                switch (embed.Kind)
                {
                    case RackEmbedDocument.KindSelective:
                    {
                        var design = new SelectivePalletDesignStore().Deserialize(embed.Design)?.ToDomain();
                        if (design == null) return null;
                        var system = new SelectiveGeometryResolver().Resolve(design, catalog);
                        return SelectiveBomBuilder.Build(system, catalog);
                    }
                    case RackEmbedDocument.KindDynamic:
                    {
                        var project = new RackProjectStore().Deserialize(embed.Design);
                        var system = project?.DynamicDesign == null
                            ? project?.DynamicSystem
                            : new DynamicRackSystemResolver(catalog).Resolve(project.DynamicDesign).System;
                        return system == null ? null : SystemBomBuilder.Build(system, catalog);
                    }
                    case RackEmbedDocument.KindCabecera:
                    {
                        var header = new RackProjectStore().Deserialize(embed.Design)?.Header;
                        return header == null ? null : BomBuilder.Build(header, catalog);
                    }
                    case RackEmbedDocument.KindCama:
                    {
                        var config = new FlowBedConfigurationStore().Deserialize(embed.Design);
                        if (config == null) return null;
                        var instances = new FlowBedLateralBuilder().Build(config, catalog);
                        return FlowBedBomBuilder.Build(instances, catalog);
                    }
                    default:
                        return null;
                }
            }
            catch
            {
                return null;
            }
        }

        private static string KindLabel(string kind)
        {
            switch (kind)
            {
                case RackEmbedDocument.KindSelective: return "Selectivo";
                case RackEmbedDocument.KindDynamic: return "Dinámico";
                case RackEmbedDocument.KindCabecera: return "Cabecera";
                case RackEmbedDocument.KindCama: return "Cama";
                default: return kind ?? string.Empty;
            }
        }

        private sealed class RackAggregate
        {
            public RackEmbedDocument Embed { get; set; }
            public int Copies { get; set; }
        }
    }
}
