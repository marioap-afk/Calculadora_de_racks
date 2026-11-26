using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.Runtime;
using RackCad.Application.Bom;
using RackCad.Application.Catalogs;
using RackCad.Application.Diagnostics;
using RackCad.Application.Persistence;
using RackCad.Application.ProjectVariables;
using RackCad.Application.VentanaRackCad;
using RackCad.Plugin.Drawing;
using RackCad.Plugin.KindHandlers;
using RackCad.Plugin.VentanaRackCad;
using AcApplication = Autodesk.AutoCAD.ApplicationServices.Application;

namespace RackCad.Plugin
{
    /// <summary>
    /// T4 bom (I-64 A-14, line D): the page «BOM y lista de racks» of the RackCad window replaces the three classic modal windows
    /// (RackListWindow, RackConsolidatedBomWindow and the BOM of a rack).
    ///
    /// <para>
    /// RACKBOMLEER is the read of the window: a command with no undo marker and no history, which takes the request that waits in the door
    /// (<see cref="RackCadRackDoorPort"/>) and answers with its number. It reads the drawing ONCE, as RACKBOMTOTAL does — every rack block
    /// with its count of direct references and the register of project variables in one transaction — and hands the facts to the pure
    /// rules (<see cref="RackBomReadingRules.Compose"/>), with the handlers of the kinds. Without a request, or for a request of another
    /// drawing, it reads nothing for the window.
    /// </para>
    /// <para>
    /// RACKLISTA, RL, RACKBOMTOTAL and RB open that page (BM-11) instead of their classic windows, as RACKCAD opens the window; the page
    /// reads the drawing as soon as AutoCAD is free (after this command ends). The classic flows stay in their files, not registered, until
    /// their retirement (F9).
    /// </para>
    /// </summary>
    public sealed partial class RackInventarioCommands
    {
        private const string OtherDrawingText = "La orden corrió en otro dibujo que el de la ventana: no se leyó nada. Vuelve a ese dibujo y pulsa «Calcular de nuevo».";

        [CommandMethod("RL")]  public void AliasRackLista() => RackLista();                // RACKLISTA

        [CommandMethod("RB")]  public void AliasRackBomTotal() => RackBomTotal();          // RACKBOMTOTAL

        /// <summary>BM-11: the list of the racks of the drawing is the tab «Racks del dibujo» of the page of the window.</summary>
        [CommandMethod("RACKLISTA")]
        public void RackLista() => OpenBomPage(RackBomTab.Racks);

        /// <summary>BM-11: the BOM of the whole drawing is the tab «BOM total» of the page of the window.</summary>
        [CommandMethod("RACKBOMTOTAL")]
        public void RackBomTotal() => OpenBomPage(RackBomTab.Total);

        /// <summary>The read of the BOM of the drawing for the page of the window (BM-01): no undo marker, no history.</summary>
        [CommandMethod("RACKBOMLEER", CommandFlags.NoUndoMarker | CommandFlags.NoHistory)]
        public void RackBomLeer()
        {
            var door = RackCadRackDoorPort.Current;
            var document = AcApplication.DocumentManager.MdiActiveDocument;
            if (document == null) return;

            var pending = RackCadRackDoorPort.TakePendingBomRead();
            if (pending == null)
            {
                document.Editor.WriteMessage("\nRACKBOMLEER la usa la ventana RackCad: no hay ninguna lectura pendiente.");
                return;
            }

            string path = null;
            string name = null;
            try
            {
                path = document.IsNamedDrawing ? document.Name : null;
                name = RackBomFiles.NameOf(document.Name);
                if (pending.Document != RackCadRackDoorPort.KeyOf(document))
                {
                    door?.AnswerBomRead(RackBomReading.Failed(pending.RequestId, path, name, OtherDrawingText));
                    return;
                }

                var reading = ReadBomOfDrawing(document, pending.RequestId, path, name);
                door?.AnswerBomRead(reading);
                document.Editor.WriteMessage("\n" + RackBomReadingRules.CommandLineText(reading));
            }
            catch (System.Exception ex)
            {
                RackLog.Exception("RackInventarioCommands.RackBomLeer", ex);
                door?.AnswerBomRead(RackBomReading.Failed(pending.RequestId, path, name, "No se pudo leer el dibujo: " + ex.Message));
            }
        }

        // Shows the window and asks it for the page; the window reads the drawing when AutoCAD is free (the door does not hand a command over
        // while this one runs).
        private static void OpenBomPage(RackBomTab tab)
        {
            try
            {
                Workspace.WorkspacePanelCommand.ShowRackCadWindow();
                var door = RackCadRackDoorPort.Current;
                var editor = AcApplication.DocumentManager.MdiActiveDocument?.Editor;
                if (door == null)
                {
                    editor?.WriteMessage("\nRackCad: no se pudo abrir la página «BOM y lista de racks» de la ventana.");
                    return;
                }

                door.AskBomPage(tab);
                editor?.WriteMessage("\nRackCad: «BOM y lista de racks» está en la ventana RackCad; el dibujo se lee al terminar esta orden.");
            }
            catch (System.Exception ex)
            {
                RackCommandSupport.Report(ex);
            }
        }

        /// <summary>
        /// ONE read of the drawing for the page: every rack block with its count of direct references (one pass over the block table) and
        /// the register of project variables, in one transaction, then the pure rules with the handler of each kind. The catalog is loaded
        /// once, only when a kind needs it.
        /// </summary>
        internal static RackBomReading ReadBomOfDrawing(Document document, long requestId, string path, string name)
        {
            List<RackEnvelopeScan> envelopes;
            ProjectVariablesReadResult registry;
            using (document.LockDocument())
            using (var transaction = document.Database.TransactionManager.StartTransaction())
            {
                envelopes = RackBlockFinder.ScanEnvelopes(transaction, document.Database, includeReferenceCount: true);

                // ONE read per reading: two racks are never quoted against two different registers.
                registry = ProjectVariablesRegistry.Read(transaction, document.Database);
                transaction.Commit();
            }

            var facts = envelopes
                .Select(envelope => new RackBomViewFact(envelope.DefinitionId.Handle.ToString(), envelope.BlockName, envelope.Embed, envelope.DirectReferenceCount))
                .ToList();

            RackCatalog catalog = null;
            var kinds = new Dictionary<string, IRackBomKind>(StringComparer.Ordinal);
            return RackBomReadingRules.Compose(requestId, path, name, facts, registry, kind =>
            {
                if (kind == null) return null;
                if (kinds.TryGetValue(kind, out var known)) return known;

                // The same resolution as the preflight of RACKBOMTOTAL (ordinal); a kind with no handler is said by the rules.
                IRackBomKind resolved = null;
                if (KindHandlerRegistry.Default.TryGet(kind, out var handler))
                {
                    catalog ??= LateralHeaderDrawService.LoadCatalog();
                    resolved = new HandlerKind(handler, catalog);
                }

                kinds[kind] = resolved;
                return resolved;
            });
        }

        // The handler of a kind, as the rules see it: its label, the gate of I-42 and its BOM, with the catalog of the reading.
        private sealed class HandlerKind : IRackBomKind
        {
            private readonly IRackKindHandler _handler;
            private readonly RackCatalog _catalog;

            public HandlerKind(IRackKindHandler handler, RackCatalog catalog)
            {
                _handler = handler;
                _catalog = catalog;
            }

            public string Label => _handler.BomLabel;

            public string BlockedReason(RackEmbedDocument embed) => _handler.OutputBlockedReason(embed, _catalog);

            public BomBuildResult Build(RackEmbedDocument embed, ProjectVariablesDocument variables) => _handler.BuildBom(embed, _catalog, variables);
        }
    }
}
