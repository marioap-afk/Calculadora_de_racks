using System.Collections.Generic;
using System.Linq;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Runtime;
using RackCad.Application.Diagnostics;
using RackCad.Application.Persistence;
using RackCad.Application.VentanaRackCad;
using RackCad.Plugin.VentanaRackCad;

namespace RackCad.Plugin
{
    /// <summary>
    /// T6 rl (answer of the Owner, 2026-11-27: «La ventana, ya»): RACKVARIABLES, RVA, RACKPROPIEDADES and RPR open their page in the RackCad
    /// window, as RL and RB open «BOM y lista de racks» and RLY and RR open «Almacén». The command the user typed is the gesture: it shows
    /// the window, asks it to show what it opens (during the call, with the gestures the window already has: the first read of «Variables»,
    /// «Editar aquí» of the rack), runs INSIDE itself the read the window handed to the door meanwhile, and tells the window when it
    /// answered (it then shows the tab asked). No command leaves the window. The classic windows stay in the classic menu (RACKMENU).
    /// </summary>
    internal static class RackCadWindowShortcuts
    {
        /// <summary>The commands only read for the window: no undo marker, and the selection made before them is kept (it designates the rack).</summary>
        internal const CommandFlags WindowFlags = CommandFlags.NoUndoMarker | CommandFlags.UsePickSet | CommandFlags.Redraw;

        /// <summary>Shows the window, asks it, runs the read it asked inside this command and says what happened in the command line.</summary>
        internal static void Open(Document document, RackCadShortcutAsk ask)
        {
            Workspace.WorkspacePanelCommand.ShowRackCadWindow();
            var door = RackCadRackDoorPort.Current;
            if (door == null)
            {
                document.Editor.WriteMessage("\nRackCad: " + RackCadShortcutRules.NotOpenedText);
                return;
            }

            var reads = RackCadRackDoorPort.CollectInlineReads(document, () => door.AskShortcut(ask));
            foreach (var read in reads) RunRead(read);
            if (ask.Taken) door.SayShortcutDone(ask);
            document.Editor.WriteMessage("\n" + RackCadShortcutRules.CommandLineText(ask));
        }

        // The read of the window, run here: the very command method AutoCAD would run, which takes the request that waits in the door.
        private static void RunRead(string command)
        {
            switch (command)
            {
                case "RACKVARIABLESLEER": new RackCadVariablesDoorCommands().ReadVariables(); break;
                case "RACKSELECTIVOABRIR": new RackCadRackDoorCommands().ReadSelectiveDesign(); break;
                case "RACKDINAMICOABRIR": new RackCadRackDoorCommands().ReadDesign(); break;
                case "RACKPUSHBACKABRIR": new RackCadPushBackDoorCommands().ReadPushBack(); break;
                case "RACKCANTILEVERABRIR": new RackCadCantileverDoorCommands().ReadCantilever(); break;
                case "RACKCAMAABRIR": new RackCadCamaDoorCommands().ReadCama(); break;
            }
        }

        /// <summary>The ids of the racks selected before the command (the selection of RLY and RR), each once.</summary>
        internal static IReadOnlyList<string> SelectedRackIds(Document document) =>
            RackLayoutCommands.SelectedRacks(document).Select(r => r.RackId).ToList();

        /// <summary>The id of the rack of a reference picked at the prompt, by the data of its definition (as RACKLAYOUT reads it); null when it is no rack.</summary>
        internal static string RackIdOf(Document document, ObjectId referenceId)
        {
            try
            {
                var json = InDocumentTransaction.Run(document, transaction =>
                {
                    var reference = (BlockReference)transaction.GetObject(referenceId, OpenMode.ForRead);
                    return Systems.Shared.RackBlockData.Read(transaction, reference.BlockTableRecord);
                });
                var embed = new RackEmbedStore().Deserialize(json);
                return string.IsNullOrWhiteSpace(embed?.Id) ? null : embed.Id.Trim();
            }
            catch (System.Exception ex)
            {
                RackLog.Exception("RackCadWindowShortcuts.RackIdOf", ex);
                return null;
            }
        }
    }
}
