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
    /// RACKBOMLEER is the read of the window, after a click of the user: a command with no undo marker and no history, which takes the
    /// request that waits in the door (<see cref="RackCadRackDoorPort"/>), says that it took it (<c>Started</c>: from then on what runs is the
    /// read itself, not a change of the drawing) and answers with its number. It reads the drawing ONCE, as RACKBOMTOTAL does — every rack
    /// block with its count of direct references and the register of project variables in one transaction — and hands the facts to the pure
    /// rules (<see cref="RackBomReadingRules.Compose"/>), with the handlers of the kinds. Without a request, or for a request of another
    /// drawing, it reads nothing for the window.
    /// </para>
    /// <para>
    /// RACKLISTA, RL, RACKBOMTOTAL and RB open that page (BM-11) instead of their classic windows, as RACKCAD opens the window. They already
    /// run in the context of a command, so they read the drawing INSIDE it, with the same read and no undo marker, and hand the reading to
    /// the page: no second command is sent (correction 4 of the review: no command leaves the window without a gesture of the user). The
    /// commands keep the selection made before them (<c>UsePickSet</c>, <c>Redraw</c>). The classic flows stay in their files, not
    /// registered, until their retirement (F9).
    /// </para>
    /// </summary>
    public sealed partial class RackInventarioCommands
    {
        private const string OtherDrawingText = "La orden corrió en otro dibujo que el de la ventana: no se leyó nada. Vuelve a ese dibujo y pulsa «Calcular».";

        // The read of the drawing for the page keeps no undo step and keeps the selection made before it.
        private const CommandFlags ReadFlags = CommandFlags.NoUndoMarker | CommandFlags.UsePickSet | CommandFlags.Redraw;

        [CommandMethod("RL", ReadFlags)]  public void AliasRackLista() => RackLista();                // RACKLISTA

        [CommandMethod("RB", ReadFlags)]  public void AliasRackBomTotal() => RackBomTotal();          // RACKBOMTOTAL

        /// <summary>BM-11: the list of the racks of the drawing is the tab «Racks del dibujo» of the page of the window, read now.</summary>
        [CommandMethod("RACKLISTA", ReadFlags)]
        public void RackLista() => OpenBomPage(RackBomTab.Racks);

        /// <summary>BM-11: the BOM of the whole drawing is the tab «BOM total» of the page of the window, read now.</summary>
        [CommandMethod("RACKBOMTOTAL", ReadFlags)]
        public void RackBomTotal() => OpenBomPage(RackBomTab.Total);

        /// <summary>The read of the BOM of the drawing for the page of the window (BM-01): no undo marker, no history, the selection kept.</summary>
        [CommandMethod("RACKBOMLEER", ReadFlags | CommandFlags.NoHistory)]
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

            // The window counts what runs from here to the answer as its own read, never as a change of the drawing (correction 1).
            door?.Started(pending.RequestId);

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

        // Shows the window, reads the drawing inside THIS command (the same read as RACKBOMLEER) and hands the reading to the page.
        private static void OpenBomPage(RackBomTab tab)
        {
            try
            {
                var document = AcApplication.DocumentManager.MdiActiveDocument;
                if (document == null) return;

                Workspace.WorkspacePanelCommand.ShowRackCadWindow();
                var door = RackCadRackDoorPort.Current;
                if (door == null)
                {
                    document.Editor.WriteMessage("\nRackCad: no se pudo abrir la página «BOM y lista de racks» de la ventana.");
                    return;
                }

                var path = document.IsNamedDrawing ? document.Name : null;
                var name = RackBomFiles.NameOf(document.Name);
                RackBomReading reading;
                try
                {
                    reading = ReadBomOfDrawing(document, 0, path, name);
                }
                catch (System.Exception ex)
                {
                    RackLog.Exception("RackInventarioCommands.OpenBomPage", ex);
                    reading = RackBomReading.Failed(0, path, name, "No se pudo leer el dibujo: " + ex.Message);
                }

                door.AskBomPage(tab, reading);
                document.Editor.WriteMessage("\n" + RackBomReadingRules.CommandLineText(reading));
            }
            catch (System.Exception ex)
            {
                RackCommandSupport.Report(ex);
            }
        }

        /// <summary>
        /// ONE read of the drawing for the page: every rack block with its count of direct references (one pass over the block table) and
        /// the register of project variables, in one transaction, then the pure rules with the handler of each kind. The catalog is loaded
        /// once, BEFORE the rules: a catalog that cannot be loaded is a reading that failed with its reason, never an unknown kind
        /// (correction 6 of the review).
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

            RackCatalog catalog;
            try
            {
                catalog = LateralHeaderDrawService.LoadCatalog();
            }
            catch (System.Exception ex)
            {
                RackLog.Exception("RackInventarioCommands.ReadBomOfDrawing (catálogo)", ex);
                return RackBomReading.Failed(requestId, path, name, "No hay BOM: no se pudo cargar el catálogo de RackCad. " + ex.Message);
            }

            if (catalog == null) return RackBomReading.Failed(requestId, path, name, "No hay BOM: no se pudo cargar el catálogo de RackCad.");

            var kinds = new Dictionary<string, IRackBomKind>(StringComparer.Ordinal);
            return RackBomReadingRules.Compose(requestId, path, name, facts, registry, kind =>
            {
                if (kind == null) return null;
                if (kinds.TryGetValue(kind, out var known)) return known;

                // The same resolution as the preflight of RACKBOMTOTAL (ordinal); a kind with no handler is said by the rules.
                IRackBomKind resolved = KindHandlerRegistry.Default.TryGet(kind, out var handler) ? new HandlerKind(handler, catalog) : null;
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
