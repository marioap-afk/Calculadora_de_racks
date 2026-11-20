using System;
using RackCad.Application.Catalogs;
using RackCad.Application.Systems.PushBack.Pilot;

namespace RackCad.Application.Systems.PushBack.Draft
{
    /// <summary>
    /// I-64 F4B PB-T3a (D-PBT3-03) — the typed result of <see cref="PushBackFullDraftMaterializer.Materialize"/>:
    /// Admitted(state), OutOfPilot(reason) or Failed(reason). Nothing is ever thrown to the caller.
    /// </summary>
    public sealed class PushBackFullDraftMaterialization
    {
        private PushBackFullDraftMaterialization(PushBackPilotMaterializationKind kind, PushBackFullDraftState state, string reason)
        {
            Kind = kind;
            State = state;
            Reason = reason;
        }

        public PushBackPilotMaterializationKind Kind { get; }

        public bool IsAdmitted => Kind == PushBackPilotMaterializationKind.Admitted;

        /// <summary>The materialized state; null unless the result is Admitted.</summary>
        public PushBackFullDraftState State { get; }

        /// <summary>Why the result is not Admitted; null for Admitted.</summary>
        public string Reason { get; }

        public static PushBackFullDraftMaterialization Admitted(PushBackFullDraftState state)
            => new PushBackFullDraftMaterialization(PushBackPilotMaterializationKind.Admitted, state ?? throw new ArgumentNullException(nameof(state)), null);

        public static PushBackFullDraftMaterialization OutOfPilot(string reason)
            => new PushBackFullDraftMaterialization(PushBackPilotMaterializationKind.OutOfPilot, null, reason);

        public static PushBackFullDraftMaterialization Failed(string reason)
            => new PushBackFullDraftMaterialization(PushBackPilotMaterializationKind.Failed, null, reason);
    }

    /// <summary>I-64 F4B PB-T3a (D-PBT3-03) — materializes the FULL draft state from the raw text of an authored envelope.</summary>
    public static class PushBackFullDraftMaterializer
    {
        public static PushBackFullDraftMaterialization Materialize(string originText, RackCatalog catalog)
            => PushBackFullDraftMaterialization.Failed(PushBackPilotMaterializer.UnreadableOrFutureDiagnostic);
    }
}
