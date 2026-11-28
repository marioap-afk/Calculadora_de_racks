using System;
using System.Collections.Generic;
using RackCad.Application.CustomProperties;
using RackCad.Application.ProjectVariables;
using RackCad.Application.VentanaRackCad;

namespace RackCad.Plugin.VentanaRackCad
{
    /// <summary>
    /// The door of the page «Variables» of the RackCad window (I-64 A-14, cuarta tanda, va), with the rules of the door to a rack: the
    /// request waits with its number and the drawing that is active, its command (RACKVARIABLESLEER or RACKVARIABLESAPLICAR,
    /// <see cref="RackCadVariablesDoorCommands"/>) takes it once, and «Dejar de esperar» drops it. The write waits with its own copies.
    /// </summary>
    public sealed partial class RackCadRackDoorPort : IRackCadVariablesDoorPort
    {
        internal const string VariablesReadCommand = "RACKVARIABLESLEER";
        internal const string VariablesWriteCommand = "RACKVARIABLESAPLICAR";

        /// <summary>A read of the page that waits for its command: its number and the drawing it is for.</summary>
        internal sealed record PendingVariablesRead(long RequestId, long Document);

        /// <summary>A write of the page that waits for its command: the request (its own copies) and the drawing it is for.</summary>
        internal sealed record PendingVariablesWrite(VariablesDoorWrite Write, long Document)
        {
            public long RequestId => Write.RequestId;
        }

        private static PendingVariablesRead _pendingVariablesRead;
        private static PendingVariablesWrite _pendingVariablesWrite;

        public event Action<VariablesDoorRead> VariablesRead;

        public event Action<VariablesDoorWritten> VariablesWritten;

        public RackCadClassicResult RequestVariables(VariablesDoorRequest request)
        {
            if (request == null) return RackCadClassicResult.Without(RackCadClassicStatus.Failed, "No hay ninguna lectura que pedir.");
            return Send(VariablesReadCommand + " ",
                document => _pendingVariablesRead = new PendingVariablesRead(request.RequestId, KeyOf(document)),
                () => _pendingVariablesRead = null);
        }

        public RackCadClassicResult RequestVariablesUpdate(VariablesDoorWrite write)
        {
            if (write?.Batch == null) return RackCadClassicResult.Without(RackCadClassicStatus.Failed, "No hay un borrador que escribir.");
            var copy = write with
            {
                Batch = new ProjectVariablesBatchRequest(
                    new List<ProjectVariableDraftRow>(write.Batch.Rows ?? Array.Empty<ProjectVariableDraftRow>()),
                    new List<string>(write.Batch.RepairRacks ?? Array.Empty<string>())),
                ProjectProperties = write.ProjectProperties == null ? null : new List<CustomPropertyDraftEntry>(write.ProjectProperties),
            };
            return Send(VariablesWriteCommand + " ",
                document => _pendingVariablesWrite = new PendingVariablesWrite(copy, KeyOf(document)),
                () => _pendingVariablesWrite = null);
        }

        /// <summary>The read of the page that waits, taken once by its command; null when there is none.</summary>
        internal static PendingVariablesRead TakePendingVariablesRead()
        {
            var pending = _pendingVariablesRead;
            _pendingVariablesRead = null;
            return pending;
        }

        /// <summary>The write of the page that waits, taken once by its command; null when there is none.</summary>
        internal static PendingVariablesWrite TakePendingVariablesWrite()
        {
            var pending = _pendingVariablesWrite;
            _pendingVariablesWrite = null;
            return pending;
        }

        private static void AbandonVariables(long requestId)
        {
            if (_pendingVariablesRead != null && _pendingVariablesRead.RequestId == requestId) _pendingVariablesRead = null;
            if (_pendingVariablesWrite != null && _pendingVariablesWrite.RequestId == requestId) _pendingVariablesWrite = null;
        }

        internal void AnswerVariablesRead(VariablesDoorRead read) => Raise(() => VariablesRead?.Invoke(read));

        internal void AnswerVariablesWrite(VariablesDoorWritten written) => Raise(() => VariablesWritten?.Invoke(written));
    }
}
