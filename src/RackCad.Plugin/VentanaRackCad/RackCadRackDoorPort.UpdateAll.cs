using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Application.ProjectVariables;
using RackCad.Application.Systems.Shared;
using RackCad.Application.VentanaRackCad;

namespace RackCad.Plugin.VentanaRackCad
{
    /// <summary>
    /// T6 at (I-64, sexta tanda): the door of «Actualizar todo» (RACKACTUALIZARTODO), with the rules of the other doors: the request waits
    /// BEFORE its command is handed over, it is kept with the drawing that was active, «Dejar de esperar» drops it, and the request that
    /// waits keeps its own copies of the list of drafts (and of what each draft carries in a list).
    /// </summary>
    public sealed partial class RackCadRackDoorPort : IRackCadUpdateAllDoorPort
    {
        internal const string UpdateAllCommand = "RACKACTUALIZARTODO";

        /// <summary>«Actualizar todo» that waits for its command: the request (copies of every draft) and the drawing it is for.</summary>
        internal sealed record PendingUpdateAll(UpdateAllWrite Write, long Document)
        {
            public long RequestId => Write.RequestId;
        }

        private static PendingUpdateAll _pendingUpdateAll;

        public event Action<UpdateAllWritten> UpdateAllWritten;

        public RackCadClassicResult RequestUpdateAll(UpdateAllWrite write)
        {
            if (write?.Items == null || write.Items.Count == 0)
                return RackCadClassicResult.Without(RackCadClassicStatus.Failed, UpdateAllText.NothingToWrite);
            if (write.Items.Any(i => i == null || !i.IsWellFormed || (i.System != UpdateAllSystem.Variables && string.IsNullOrWhiteSpace(i.RackId))))
                return RackCadClassicResult.Without(RackCadClassicStatus.Failed, "La petición de «Actualizar todo» no es válida.");

            return Send(UpdateAllCommand + " ",
                document => _pendingUpdateAll = new PendingUpdateAll(write with { Items = write.Items.Select(Kept).ToList() }, KeyOf(document)),
                () => _pendingUpdateAll = null);
        }

        // An item as it waits: the identity trimmed, as every door keeps it, and its own copy of the final linked states of a Selectivo.
        private static UpdateAllItem Kept(UpdateAllItem item)
        {
            var rackId = item.RackId?.Trim();
            return item with
            {
                RackId = rackId,
                Dynamic = item.Dynamic == null ? null : item.Dynamic with { RackId = item.Dynamic.RackId.Trim() },
                Selective = item.Selective == null ? null : item.Selective with
                {
                    RackId = item.Selective.RackId.Trim(),
                    LinkedPropertyFinalStates = new Dictionary<PropertyId, LinkedPropertyEditState>(
                        item.Selective.LinkedPropertyFinalStates ?? new Dictionary<PropertyId, LinkedPropertyEditState>()),
                },
                PushBack = item.PushBack == null ? null : item.PushBack with { RackId = item.PushBack.RackId.Trim() },
                Cantilever = item.Cantilever == null ? null : item.Cantilever with { RackId = item.Cantilever.RackId.Trim() },
                Cama = item.Cama == null ? null : item.Cama with { RackId = item.Cama.RackId.Trim() },
                Cabecera = item.Cabecera == null ? null : item.Cabecera with { RackId = item.Cabecera.RackId.Trim() },
            };
        }

        // One line of Abandon: «Dejar de esperar» forgets «Actualizar todo» if it waits.
        private static void AbandonUpdateAll(long requestId)
        {
            if (_pendingUpdateAll != null && _pendingUpdateAll.RequestId == requestId) _pendingUpdateAll = null;
        }

        /// <summary>«Actualizar todo» that waits, taken once by its command; null when there is none.</summary>
        internal static PendingUpdateAll TakePendingUpdateAll()
        {
            var pending = _pendingUpdateAll;
            _pendingUpdateAll = null;
            return pending;
        }

        internal void AnswerUpdateAll(UpdateAllWritten written) => Raise(() => UpdateAllWritten?.Invoke(written));
    }
}
