#nullable enable
using System;
using System.Collections.Generic;

namespace RackCad.Application.Systems.PushBack.Pilot
{
    /// <summary>I-64 F4-T2A (D-10, INV-26, D-T2-03) — comparator and deep copier of the pilot state. RED skeleton: inert.</summary>
    public sealed class PushBackPilotStateComparer : IEqualityComparer<PushBackPilotDraftState>
    {
        public static PushBackPilotStateComparer Instance { get; } = new PushBackPilotStateComparer();

        public bool Equals(PushBackPilotDraftState? x, PushBackPilotDraftState? y) => false;

        public int GetHashCode(PushBackPilotDraftState obj) => 0;

        public static PushBackPilotDraftState Copy(PushBackPilotDraftState state) => state;
    }
}
