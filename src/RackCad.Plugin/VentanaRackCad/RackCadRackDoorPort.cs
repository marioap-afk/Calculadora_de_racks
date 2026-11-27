using System;
using System.Collections.Generic;
using RackCad.Application.Diagnostics;
using RackCad.Application.ProjectVariables;
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
    /// The Selectivo has its own two commands (Selectivo E1-A), with the same rules, and the two of its second delivery: «Insertar vista…»
    /// (RACKSELECTIVOINSERTAR) and the new Selectivo of the window (RACKSELECTIVONUEVO), each in its own slot.
    /// </summary>
    public sealed partial class RackCadRackDoorPort : IRackCadSelectiveDoorPort // T4 cb, va, bom: partial (RackCadRackDoorPort.Cabecera.cs, .Variables.cs, .Bom.cs)
    {
        internal const string ReadCommand = "RACKDINAMICOABRIR";
        internal const string WriteCommand = "RACKDINAMICOAPLICAR";
        internal const string InsertCommand = "RACKDINAMICOINSERTAR";
        internal const string NewCommand = "RACKDINAMICONUEVO";
        internal const string SelectiveReadCommand = "RACKSELECTIVOABRIR";
        internal const string SelectiveWriteCommand = "RACKSELECTIVOAPLICAR";
        internal const string SelectiveInsertCommand = "RACKSELECTIVOINSERTAR";
        internal const string SelectiveNewCommand = "RACKSELECTIVONUEVO";

        /// <summary>A read that waits for its command: the number, the rack and the drawing it is for.</summary>
        internal sealed record PendingRead(long RequestId, string RackId, long Document);

        /// <summary>A write that waits for its command: the number, the copies of the draft and the drawing it is for.</summary>
        internal sealed record PendingWrite(long RequestId, string RackId, string Name, DynamicRackDesign Design, DynamicRackSystem System, long Document,
            IReadOnlyList<RackViewAddress> Views = null, RackPropertiesWrite Properties = null); // T4 va: Properties (VA-07)

        /// <summary>A write of a Selectivo that waits for its command: the request (copies of the draft and its final linked states) and the drawing.</summary>
        internal sealed record PendingSelectiveWrite(SelectiveDoorWrite Write, long Document)
        {
            public long RequestId => Write.RequestId;

            public string RackId => Write.RackId;
        }

        /// <summary>The door of the palette of the process; the commands answer through it.</summary>
        internal static RackCadRackDoorPort Current { get; private set; }

        private static PendingRead _pendingRead;
        private static PendingWrite _pendingWrite;
        private static PendingWrite _pendingInsert;
        private static PendingWrite _pendingNew;
        private static PendingRead _pendingSelectiveRead;
        private static PendingSelectiveWrite _pendingSelectiveWrite;
        private static PendingSelectiveWrite _pendingSelectiveInsert;
        private static PendingSelectiveWrite _pendingSelectiveNew;

        public RackCadRackDoorPort()
        {
            Current = this;
        }

        public event Action<RackDesignRead> DesignRead;

        public event Action<RackDesignWritten> DesignWritten;

        public event Action<long> RequestStarted;

        public event Action<SelectiveDesignRead> SelectiveRead;

        public RackCadClassicResult RequestSelectiveDesign(RackDoorRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.RackId))
                return RackCadClassicResult.Without(RackCadClassicStatus.Failed, "El rack no tiene una identidad válida.");

            return Send(SelectiveReadCommand + " ",
                document => _pendingSelectiveRead = new PendingRead(request.RequestId, request.RackId.Trim(), KeyOf(document)),
                () => _pendingSelectiveRead = null);
        }

        public RackCadClassicResult RequestSelectiveUpdate(SelectiveDoorWrite write)
        {
            if (write == null || string.IsNullOrWhiteSpace(write.RackId) || write.Design == null || write.System == null)
                return RackCadClassicResult.Without(RackCadClassicStatus.Failed, "No hay un borrador válido que escribir.");

            // Selectivo E2: a new Selectivo of the window goes first to its own command, which inserts it with the views chosen, never
            // without views or identity (the rule of the door); «Insertar vista…» has its own command too.
            if (write.NewRack)
            {
                var refusal = SelectiveDoorInsertion.RefusalOf(write);
                if (refusal != null) return RackCadClassicResult.Without(RackCadClassicStatus.Failed, refusal);
                return Send(
                    SelectiveNewCommand + " ",
                    document => _pendingSelectiveNew = PendingCopy(write, document),
                    () => _pendingSelectiveNew = null);
            }

            var insert = write.Inserts;
            return Send(
                (insert ? SelectiveInsertCommand : SelectiveWriteCommand) + " ",
                document => SetSelectivePending(insert, PendingCopy(write, document)),
                () => SetSelectivePending(insert, null));
        }

        // The request as it waits: the identity trimmed, and its own copies of the final linked states and of the views to insert.
        private static PendingSelectiveWrite PendingCopy(SelectiveDoorWrite write, AcDocument document) => new PendingSelectiveWrite(
            write with
            {
                RackId = write.RackId.Trim(),
                LinkedPropertyFinalStates = new Dictionary<PropertyId, LinkedPropertyEditState>(
                    write.LinkedPropertyFinalStates ?? new Dictionary<PropertyId, LinkedPropertyEditState>()),
                InsertViews = write.Inserts ? new List<RackViewAddress>(write.InsertViews) : null,
            },
            KeyOf(document));

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

            // A new Dinámico of the window: its own command, which inserts the rack with the views chosen.
            if (write.NewRack)
            {
                if (!write.Inserts) return RackCadClassicResult.Without(RackCadClassicStatus.Failed, "Elige al menos una vista para insertar el rack.");
                return Send(
                    NewCommand + " ",
                    document => _pendingNew = new PendingWrite(write.RequestId, write.RackId.Trim(), write.Name, write.Design, write.System, KeyOf(document),
                        new List<RackViewAddress>(write.InsertViews)),
                    () => _pendingNew = null);
            }

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
                document => _pendingWrite = new PendingWrite(write.RequestId, write.RackId.Trim(), write.Name, write.Design, write.System, KeyOf(document),
                    Properties: write.Properties), // T4 va
                () => _pendingWrite = null);
        }

        public void Abandon(long requestId)
        {
            if (_pendingRead != null && _pendingRead.RequestId == requestId) _pendingRead = null;
            if (_pendingWrite != null && _pendingWrite.RequestId == requestId) _pendingWrite = null;
            if (_pendingInsert != null && _pendingInsert.RequestId == requestId) _pendingInsert = null;
            if (_pendingNew != null && _pendingNew.RequestId == requestId) _pendingNew = null;
            if (_pendingSelectiveRead != null && _pendingSelectiveRead.RequestId == requestId) _pendingSelectiveRead = null;
            if (_pendingSelectiveWrite != null && _pendingSelectiveWrite.RequestId == requestId) _pendingSelectiveWrite = null;
            if (_pendingSelectiveInsert != null && _pendingSelectiveInsert.RequestId == requestId) _pendingSelectiveInsert = null;
            if (_pendingSelectiveNew != null && _pendingSelectiveNew.RequestId == requestId) _pendingSelectiveNew = null;
            AbandonCabecera(requestId); // T4 cb
            AbandonVariables(requestId); // T4 va: the page «Variables» (RackCadRackDoorPort.Variables.cs)
            AbandonBomRead(requestId); // T4 bom
            AbandonWarehouse(requestId); // T5 al: the page «Almacén» (RackCadRackDoorPort.Warehouse.cs)
            AbandonCama(requestId); // T5 cm: the door of a cama (RackCadRackDoorPort.Cama.cs)
            AbandonCantilever(requestId); // T5 ct: the door of a Cantilever line (RackCadRackDoorPort.Cantilever.cs)
            AbandonPushBack(requestId); // T5 pb: the door of a Push Back (RackCadRackDoorPort.PushBack.cs)
            AbandonLarguero(requestId); // T6 lg: the page «Larguero» (RackCadRackDoorPort.Larguero.cs)
        }

        /// <summary>The read of a Selectivo that waits, taken once by its command; null when there is none.</summary>
        internal static PendingRead TakePendingSelectiveRead()
        {
            var pending = _pendingSelectiveRead;
            _pendingSelectiveRead = null;
            return pending;
        }

        /// <summary>The write of a Selectivo that waits, taken once by its command; null when there is none.</summary>
        internal static PendingSelectiveWrite TakePendingSelectiveWrite()
        {
            var pending = _pendingSelectiveWrite;
            _pendingSelectiveWrite = null;
            return pending;
        }

        /// <summary>The «Insertar vista…» of a Selectivo that waits (RACKSELECTIVOINSERTAR), taken once by its command; null when there is none.</summary>
        internal static PendingSelectiveWrite TakePendingSelectiveInsert()
        {
            var pending = _pendingSelectiveInsert;
            _pendingSelectiveInsert = null;
            return pending;
        }

        /// <summary>The new Selectivo of the window that waits for its insertion (RACKSELECTIVONUEVO), taken once by its command; null when there is none.</summary>
        internal static PendingSelectiveWrite TakePendingSelectiveNew()
        {
            var pending = _pendingSelectiveNew;
            _pendingSelectiveNew = null;
            return pending;
        }

        private static void SetSelectivePending(bool insert, PendingSelectiveWrite pending)
        {
            if (insert) _pendingSelectiveInsert = pending;
            else _pendingSelectiveWrite = pending;
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

        /// <summary>The new rack that waits for its insertion, taken once by its command; null when there is none.</summary>
        internal static PendingWrite TakePendingNew()
        {
            var pending = _pendingNew;
            _pendingNew = null;
            return pending;
        }

        /// <summary>The identity of a document while it is open: the address of its native object.</summary>
        internal static long KeyOf(AcDocument document) => document == null ? 0L : document.UnmanagedObject.ToInt64();

        internal void AnswerRead(RackDesignRead read) => Raise(() => DesignRead?.Invoke(read));

        internal void AnswerSelectiveRead(SelectiveDesignRead read) => Raise(() => SelectiveRead?.Invoke(read));

        internal void AnswerWrite(RackDesignWritten written) => Raise(() => DesignWritten?.Invoke(written));

        internal void Started(long requestId) => Raise(() => RequestStarted?.Invoke(requestId));

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
