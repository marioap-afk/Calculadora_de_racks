using System;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Runtime;
using RackCad.Application.Diagnostics;
using RackCad.Application.Persistence;
using RackCad.Application.VentanaRackCad;
using RackCad.Domain.Systems.Dynamic;
using AcApplication = Autodesk.AutoCAD.ApplicationServices.Application;

namespace RackCad.Plugin.VentanaRackCad
{
    /// <summary>
    /// The door of the RackCad window to one rack (I-64 A-14, line D). The palette never reads or writes the drawing itself: it hands
    /// the request to AutoCAD as a command, only when there is an active document and no command is in progress, and the command answers
    /// through the events of the port. The read runs RACKDINAMICOABRIR, a command without an undo marker; the write runs
    /// RACKDINAMICOAPLICAR, which redraws the rack with the very drawing half of RACKEDITAR (<c>ApplyDynamicEdit</c>), update only.
    /// </summary>
    public sealed class RackCadRackDoorPort : IRackCadRackDoorPort
    {
        internal const string ReadCommand = "RACKDINAMICOABRIR";
        internal const string WriteCommand = "RACKDINAMICOAPLICAR";

        internal sealed record PendingWrite(string RackId, string Name, DynamicRackDesign Design, DynamicRackSystem System);

        /// <summary>The door of the palette of the process; the commands answer through it.</summary>
        internal static RackCadRackDoorPort Current { get; private set; }

        private static PendingWrite _pendingWrite;

        public RackCadRackDoorPort()
        {
            Current = this;
        }

        public event Action<RackDesignRead> DesignRead;

        public event Action<RackDesignWritten> DesignWritten;

        public RackCadClassicResult RequestDesign(string rackId)
        {
            if (string.IsNullOrWhiteSpace(rackId) || rackId.IndexOf(' ') >= 0)
                return RackCadClassicResult.Without(RackCadClassicStatus.Failed, "El rack no tiene una identidad válida.");
            return Send(ReadCommand + " " + rackId + " ");
        }

        public RackCadClassicResult RequestDynamicUpdate(string rackId, string name, DynamicRackDesign design, DynamicRackSystem system)
        {
            if (string.IsNullOrWhiteSpace(rackId) || design == null || system == null)
                return RackCadClassicResult.Without(RackCadClassicStatus.Failed, "No hay un borrador válido que escribir.");

            var started = Send(WriteCommand + " ");
            if (started.IsStarted) _pendingWrite = new PendingWrite(rackId, name, design, system);
            return started;
        }

        internal static PendingWrite TakePendingWrite()
        {
            var pending = _pendingWrite;
            _pendingWrite = null;
            return pending;
        }

        internal void AnswerRead(RackDesignRead read) => Raise(() => DesignRead?.Invoke(read));

        internal void AnswerWrite(RackDesignWritten written) => Raise(() => DesignWritten?.Invoke(written));

        private static RackCadClassicResult Send(string text)
        {
            try
            {
                var document = AcApplication.DocumentManager.MdiActiveDocument;
                if (document == null)
                    return RackCadClassicResult.Without(RackCadClassicStatus.NoDocument, "No hay un dibujo activo.");
                if (!string.IsNullOrEmpty(document.CommandInProgress) || !document.Editor.IsQuiescent)
                    return RackCadClassicResult.Without(RackCadClassicStatus.Busy, "Hay un comando en curso.");

                document.SendStringToExecute(text, true, false, false);
                return RackCadClassicResult.Started();
            }
            catch (System.Exception ex)
            {
                RackLog.Exception("RackCadRackDoorPort.Send", ex);
                return RackCadClassicResult.Without(RackCadClassicStatus.Failed, ex.Message);
            }
        }

        // A subscriber of the window that fails is logged and never reaches the command or AutoCAD.
        private static void Raise(Action raise)
        {
            try
            {
                raise();
            }
            catch (System.Exception ex)
            {
                RackLog.Exception("RackCadRackDoorPort.Raise", ex);
            }
        }
    }

    /// <summary>
    /// The two commands of the door of the RackCad window (I-64 A-14, line D). They are typed by the window, not by the user: the id of the
    /// rack goes on the command line. RACKDINAMICOABRIR only reads (no undo marker, no history); RACKDINAMICOAPLICAR writes with the drawing
    /// half of RACKEDITAR, so one «U» undoes the whole update.
    /// </summary>
    public sealed class RackCadRackDoorCommands
    {
        [CommandMethod("RACKDINAMICOABRIR", CommandFlags.NoUndoMarker | CommandFlags.NoHistory)]
        public void ReadDesign()
        {
            var door = RackCadRackDoorPort.Current;
            var document = AcApplication.DocumentManager.MdiActiveDocument;
            if (document == null) return;

            string rackId = null;
            try
            {
                var options = new PromptStringOptions("\nRack") { AllowSpaces = false };
                var answer = document.Editor.GetString(options);
                if (answer.Status != PromptStatus.OK) return;
                rackId = answer.StringResult?.Trim();

                var blocks = RackCommandSupport.FindRackBlocks(document, rackId);
                if (blocks.Count == 0)
                {
                    door?.AnswerRead(new RackDesignRead(rackId, false, null, null, null, "El rack no está en el dibujo."));
                    return;
                }

                var embed = blocks[0].Embed;
                if (embed == null || string.IsNullOrEmpty(embed.Design))
                {
                    door?.AnswerRead(new RackDesignRead(rackId, false, embed?.Kind, embed?.Name, null, "El rack no tiene datos editables."));
                    return;
                }

                door?.AnswerRead(new RackDesignRead(rackId, true, embed.Kind, embed.Name, embed.Design, null));
            }
            catch (System.Exception ex)
            {
                RackLog.Exception("RackCadRackDoorCommands.ReadDesign", ex);
                door?.AnswerRead(new RackDesignRead(rackId, false, null, null, null, ex.Message));
            }
        }

        [CommandMethod("RACKDINAMICOAPLICAR")]
        public void WriteDesign()
        {
            var door = RackCadRackDoorPort.Current;
            var pending = RackCadRackDoorPort.TakePendingWrite();
            var document = AcApplication.DocumentManager.MdiActiveDocument;
            if (document == null || pending == null) return;

            try
            {
                var blocks = RackCommandSupport.FindRackBlocks(document, pending.RackId);
                if (blocks.Count == 0)
                {
                    door?.AnswerWrite(new RackDesignWritten(pending.RackId, false, "El rack ya no está en el dibujo: no se escribió nada."));
                    return;
                }

                var (blockId, embed) = blocks[0];
                var project = new RackProjectStore().Deserialize(embed.Design);
                if (project?.DynamicDesign == null)
                {
                    door?.AnswerWrite(new RackDesignWritten(pending.RackId, false, "El rack del dibujo no es un Dinámico legible: no se escribió nada."));
                    return;
                }

                RackDinamicoCommands.ApplyDynamicEdit(
                    document, blockId, embed, project, new RackCad.UI.Systems.Dynamic.DynamicDraftOutcome(pending.RackId, pending.Name, pending.Design, pending.System));
                door?.AnswerWrite(new RackDesignWritten(pending.RackId, true, "AutoCAD aplicó el borrador con la ruta de RACKEDITAR; la línea de órdenes dice qué vistas se redibujaron."));
            }
            catch (System.Exception ex)
            {
                RackCommandSupport.Report(ex);
                door?.AnswerWrite(new RackDesignWritten(pending.RackId, false, "No se pudo escribir: " + ex.Message));
            }
        }
    }
}
