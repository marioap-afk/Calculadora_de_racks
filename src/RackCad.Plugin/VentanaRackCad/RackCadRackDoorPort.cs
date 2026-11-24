using System;
using System.Collections.Generic;
using RackCad.Application.Diagnostics;
using RackCad.Application.Systems.Shared;
using RackCad.Application.VentanaRackCad;
using RackCad.Domain.Systems.Dynamic;
using AcApplication = Autodesk.AutoCAD.ApplicationServices.Application;
using AcDocument = Autodesk.AutoCAD.ApplicationServices.Document;

namespace RackCad.Plugin.VentanaRackCad
{
    /// <summary>
    /// The door of the RackCad window to one rack (I-64 A-14, line D). The palette never reads or writes the drawing itself: it keeps the
    /// request (its number, the rack, the drawing that is active and, for a write, the copies of the draft) and hands its command to
    /// AutoCAD, only when there is an active document and no command is in progress, then gives the focus back to the drawing. The command
    /// takes the request when it runs (<see cref="RackCadRackDoorCommands"/>) and answers through the events of the port with the number of
    /// the request. A newer request replaces an older one that never ran, and a request the window abandoned is dropped, so a late
    /// command does nothing. This file holds no AutoCAD transaction and no lock; the commands do their work in their own command context.
    /// </summary>
    public sealed class RackCadRackDoorPort : IRackCadRackDoorPort
    {
        internal const string ReadCommand = "RACKDINAMICOABRIR";
        internal const string WriteCommand = "RACKDINAMICOAPLICAR";
        internal const string InsertCommand = "RACKDINAMICOINSERTAR";

        /// <summary>A read that waits for its command: the number, the rack and the drawing it is for.</summary>
        internal sealed record PendingRead(long RequestId, string RackId, long Document);

        /// <summary>A write that waits for its command: the number, the copies of the draft and the drawing it is for.</summary>
        internal sealed record PendingWrite(long RequestId, string RackId, string Name, DynamicRackDesign Design, DynamicRackSystem System, long Document,
            IReadOnlyList<RackViewAddress> Views = null);

        /// <summary>The door of the palette of the process; the commands answer through it.</summary>
        internal static RackCadRackDoorPort Current { get; private set; }

        private static PendingRead _pendingRead;
        private static PendingWrite _pendingWrite;
        private static PendingWrite _pendingInsert;

        public RackCadRackDoorPort()
        {
            Current = this;
        }

        public event Action<RackDesignRead> DesignRead;

        public event Action<RackDesignWritten> DesignWritten;

        public RackCadClassicResult RequestDesign(RackDoorRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.RackId))
                return RackCadClassicResult.Without(RackCadClassicStatus.Failed, "El rack no tiene una identidad válida.");

            return Send(ReadCommand + " ", document => _pendingRead = new PendingRead(request.RequestId, request.RackId.Trim(), KeyOf(document)), () => _pendingRead = null);
        }

        public RackCadClassicResult RequestDynamicUpdate(RackDoorWrite write)
        {
            if (write == null || string.IsNullOrWhiteSpace(write.RackId) || write.Design == null || write.System == null)
                return RackCadClassicResult.Without(RackCadClassicStatus.Failed, "No hay un borrador válido que escribir.");

            // «Insertar vista…»: its own command, which writes the draft in the views that exist and then inserts the new ones.
            if (write.Inserts)
            {
                return Send(
                    InsertCommand + " ",
                    document => _pendingInsert = new PendingWrite(write.RequestId, write.RackId.Trim(), write.Name, write.Design, write.System, KeyOf(document),
                        new List<RackViewAddress>(write.InsertViews)),
                    () => _pendingInsert = null);
            }

            return Send(
                WriteCommand + " ",
                document => _pendingWrite = new PendingWrite(write.RequestId, write.RackId.Trim(), write.Name, write.Design, write.System, KeyOf(document)),
                () => _pendingWrite = null);
        }

        public void Abandon(long requestId)
        {
            if (_pendingRead != null && _pendingRead.RequestId == requestId) _pendingRead = null;
            if (_pendingWrite != null && _pendingWrite.RequestId == requestId) _pendingWrite = null;
            if (_pendingInsert != null && _pendingInsert.RequestId == requestId) _pendingInsert = null;
        }

        /// <summary>The read that waits, taken once by its command; null when there is none.</summary>
        internal static PendingRead TakePendingRead()
        {
            var pending = _pendingRead;
            _pendingRead = null;
            return pending;
        }

        /// <summary>The write that waits, taken once by its command; null when there is none.</summary>
        internal static PendingWrite TakePendingWrite()
        {
            var pending = _pendingWrite;
            _pendingWrite = null;
            return pending;
        }

        /// <summary>The write with views to insert that waits, taken once by its command; null when there is none.</summary>
        internal static PendingWrite TakePendingInsert()
        {
            var pending = _pendingInsert;
            _pendingInsert = null;
            return pending;
        }

        /// <summary>The identity of a document while it is open: the address of its native object.</summary>
        internal static long KeyOf(AcDocument document) => document == null ? 0L : document.UnmanagedObject.ToInt64();

        internal void AnswerRead(RackDesignRead read) => Raise(() => DesignRead?.Invoke(read));

        internal void AnswerWrite(RackDesignWritten written) => Raise(() => DesignWritten?.Invoke(written));

        // The request is kept before its command is handed over (AutoCAD may run it at once) and dropped if handing it over fails.
        private static RackCadClassicResult Send(string text, Action<AcDocument> keep, Action drop)
        {
            try
            {
                var document = AcApplication.DocumentManager.MdiActiveDocument;
                if (document == null)
                    return RackCadClassicResult.Without(RackCadClassicStatus.NoDocument, "No hay un dibujo activo.");
                if (!string.IsNullOrEmpty(document.CommandInProgress) || !document.Editor.IsQuiescent)
                    return RackCadClassicResult.Without(RackCadClassicStatus.Busy, "Hay un comando en curso.");

                keep(document);
                try
                {
                    document.SendStringToExecute(text, true, false, false);
                }
                catch
                {
                    drop();
                    throw;
                }

                RackCadClassicCommandPort.ReturnFocusToDrawing(document);
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
}
