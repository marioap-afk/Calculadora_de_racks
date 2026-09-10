using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Runtime;
using RackCad.Application.Bom;
using RackCad.Application.Catalogs;
using RackCad.Application.Persistence;
using RackCad.Application.ProjectVariables;
using RackCad.Plugin.Drawing;
using RackCad.Plugin.KindHandlers;
using RackCad.UI;
using AcApplication = Autodesk.AutoCAD.ApplicationServices.Application;

namespace RackCad.Plugin
{
    /// <summary>
    /// RACKBOMTOTAL — the whole-drawing bill of materials. Scans every rack block (grouped by GUID like RACKLISTA),
    /// rebuilds each rack's BOM from its embedded design (per kind), and shows a per-rack breakdown + grand total.
    ///
    /// <para>
    /// I-47 G13 closed the three doors through which an apparently valid total used to come out. It no longer
    /// quotes the FIRST view of a rack — it proves every view says the same thing first; it reads the project
    /// variable register ONCE and hands the same snapshot to every handler, so the drawn value and the quoted
    /// value cannot come from different reads; and a placed definition it cannot classify aborts the total
    /// instead of being skipped, because without identity there is no way to show it is not part of a rack
    /// that IS being quoted.
    /// </para>
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

                List<RackEnvelopeScan> envelopes;
                ProjectVariablesReadResult registryRead;
                using (document.LockDocument())
                using (var transaction = document.Database.TransactionManager.StartTransaction())
                {
                    envelopes = RackBlockFinder.ScanEnvelopes(transaction, document.Database, includeReferenceCount: true);

                    // ONE read per command. Reading it per rack — or per view — would let two racks be quoted
                    // against two different registers within a single total.
                    registryRead = ProjectVariablesRegistry.Read(transaction, document.Database);
                    transaction.Commit();
                }

                if (registryRead.Outcome != ProjectVariablesReadOutcome.Absent &&
                    registryRead.Outcome != ProjectVariablesReadOutcome.Readable)
                {
                    // A register that EXISTS and cannot be read is not an empty one. Quoting against "empty"
                    // would silently price every bound rack at its frozen literal.
                    editor.WriteMessage("\nRackCad: no se genera el listado. " + registryRead.Error);
                    return;
                }

                var projectVariables = registryRead.Document;

                // Every view of every rack, projected purely (I-47 G8): what can be classified, what cannot, and
                // whether it is PLACED — which is measured from the block table record, never from the payload.
                var siblings = new Dictionary<string, List<ProjectVariableScanEntry>>(StringComparer.OrdinalIgnoreCase);
                var views = new Dictionary<string, RackEmbedDocument>(StringComparer.OrdinalIgnoreCase);
                var copies = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
                var order = new List<string>();

                foreach (var envelope in envelopes)
                {
                    var definitionId = envelope.DefinitionId.Handle.ToString();
                    var entry = ProjectVariableScanProjection.Project(
                        definitionId, envelope.Embed, envelope.DirectReferenceCount);

                    if (!entry.OuterEnvelopeInterpretable)
                    {
                        if (entry.DirectReferenceCount == 0)
                        {
                            // Defined but not placed: it is not in the drawing, so it cannot be missing from the
                            // total. This is the historic skip, now narrowed to the case where it is sound.
                            continue;
                        }

                        editor.WriteMessage(
                            "\n" + RackBomOutputGate.DescribeUnclassifiableDefinition(definitionId, envelope.BlockName));
                        return;
                    }

                    if (!siblings.TryGetValue(entry.RackId, out var group))
                    {
                        group = new List<ProjectVariableScanEntry>();
                        siblings[entry.RackId] = group;
                        copies[entry.RackId] = 0;
                        order.Add(entry.RackId);
                    }

                    group.Add(entry);
                    views[definitionId] = envelope.Embed;

                    // A rack copied N times shows N: the MAX direct-reference count across its view-blocks.
                    if (entry.DirectReferenceCount > copies[entry.RackId])
                    {
                        copies[entry.RackId] = entry.DirectReferenceCount;
                    }
                }

                if (order.Count == 0)
                {
                    editor.WriteMessage("\nRackCad: no hay racks en el dibujo para listar.");
                    return;
                }

                // Only PLACED racks matter (a defined-but-unplaced rack was drawn then erased, not yet purged).
                var placed = order.Where(rackId => copies[rackId] > 0).ToList();

                if (placed.Count == 0)
                {
                    editor.WriteMessage("\nRackCad: no hay racks en el dibujo para listar.");
                    return;
                }

                // Preflight: EVERY placed rack must resolve a handler BEFORE we load the catalog or build anything.
                // If ANY placed kind has no registered handler, abort the WHOLE command — a partial BOM must never be
                // shown. (A KNOWN handler whose payload turns out unreadable is still best-effort skipped below.)
                if (!KindHandlerDispatch.TryResolveAll(editor, placed.Select(rackId => siblings[rackId][0].Kind).ToList(), out var handlers))
                {
                    return;
                }

                var catalog = LateralHeaderDrawService.LoadCatalog();

                // The authority pass: a rack is quoted from ONE view only after every view was read and shown to
                // describe the same authored state. A rack without a single authority stays OUT of the listing,
                // with a visible warning — the historic policy for a rack this build cannot read.
                var approved = new List<ApprovedRack>();
                var skipped = new List<string>();

                for (var i = 0; i < placed.Count; i++)
                {
                    var rackId = placed[i];
                    var authority = BomAuthoredAuthority.Resolve(rackId, siblings[rackId]);
                    var name = siblings[rackId][0] == null ? null : views[siblings[rackId][0].DefinitionId]?.Name;

                    if (!authority.IsSuccess)
                    {
                        skipped.Add(RackBomOutputGate.DescribeNoAuthority(name, authority.Error));
                        continue;
                    }

                    approved.Add(new ApprovedRack
                    {
                        Handler = handlers[i],
                        Embed = views[authority.RepresentativeDefinitionId],
                        Copies = copies[rackId],
                    });
                }

                // I-42 (A1C/H11, contrato del dueño) — un diseño BLOQUEADO no entra al listado, y no se calla.
                //
                // El editor ya impide insertar, actualizar y cotizar un rack cuyo diagnostico es bloqueante; este
                // comando recorria los mismos diseños sin ese filtro y cotizaba, por ejemplo, un rack cuya cama pide
                // mas longitud de la que su estructura tiene. Se aborta el TOTAL —igual que cuando un kind no tiene
                // handler— porque un total al que le falta un rack no puede parecer completo, y se nombra cada rack
                // con el motivo que redacto su propia autoridad de diagnosticos.
                var blocked = new List<KeyValuePair<string, string>>();
                foreach (var rack in approved)
                {
                    var reason = rack.Handler.OutputBlockedReason(rack.Embed, catalog);
                    if (!string.IsNullOrWhiteSpace(reason))
                    {
                        blocked.Add(new KeyValuePair<string, string>(rack.Embed.Name, reason));
                    }
                }

                if (blocked.Count > 0)
                {
                    editor.WriteMessage("\n" + RackBomOutputGate.DescribeBlocked(blocked));
                    return;
                }

                foreach (var warning in skipped)
                {
                    editor.WriteMessage("\n" + warning);
                }

                var racks = new List<ConsolidatedRackBom>();
                foreach (var rack in approved)
                {
                    var result = BuildRackBom(rack.Handler, rack.Embed, catalog, projectVariables);

                    if (result.Outcome == BomBuildOutcome.BrokenProjectVariableReference)
                    {
                        // NOT a best-effort skip. The design is readable and the VARIABLE is what is missing, so a
                        // total without this rack would look complete and be wrong.
                        editor.WriteMessage("\n" + RackBomOutputGate.DescribeBrokenProjectVariableReference(
                            result.RackName, result.PropertyId, result.VariableId, result.Error));
                        return;
                    }

                    if (!result.IsSuccess)
                    {
                        // Payload ilegible: se salta, pero NUNCA en silencio — el listado que sale es mas corto y
                        // el usuario tiene que saber cual falta (I-42/H11).
                        editor.WriteMessage("\n" + RackBomOutputGate.DescribeUnreadable(rack.Embed.Name));
                        continue;
                    }

                    racks.Add(new ConsolidatedRackBom
                    {
                        Name = string.IsNullOrWhiteSpace(rack.Embed.Name) ? "(sin nombre)" : rack.Embed.Name.Trim(),
                        Kind = rack.Handler.BomLabel,
                        Copies = rack.Copies,
                        Bom = result.Bom
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

        /// <summary>
        /// Rebuild ONE rack's bill of materials via its already-resolved handler, TYPED (I-47 G13).
        ///
        /// <para>
        /// The blanket <c>catch</c> that used to wrap this is gone: it turned every expected state into
        /// "payload ilegible", including a rack whose design is perfectly readable and whose VARIABLE is
        /// missing. What remains is a last-resort guard for the genuinely unexpected, and it reports the state
        /// it actually knows — an unreadable payload — rather than pretending to know more.
        /// </para>
        /// </summary>
        private static BomBuildResult BuildRackBom(
            IRackKindHandler handler,
            RackEmbedDocument embed,
            RackCatalog catalog,
            ProjectVariablesDocument projectVariables)
        {
            try
            {
                return handler.BuildBom(embed, catalog, projectVariables);
            }
            catch (System.Exception ex)
            {
                RackCad.Application.Diagnostics.RackLog.Exception("Construir BOM de un rack (payload ilegible)", ex);
                return BomBuildResult.UnreadablePayload(embed.Id, embed.Name, ex.Message);
            }
        }

        /// <summary>One rack whose views were proven to agree, and the view it may be quoted from.</summary>
        private sealed class ApprovedRack
        {
            public IRackKindHandler Handler { get; set; }
            public RackEmbedDocument Embed { get; set; }
            public int Copies { get; set; }
        }
    }
}
