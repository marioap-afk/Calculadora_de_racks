using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Catalogs;
using RackCad.Application.Persistence;
using RackCad.Domain.Systems.PushBack;

namespace RackCad.Application.Systems.PushBack.Draft
{
    /// <summary>What the edition does to ONE view of the rack (D-PBT3-06).</summary>
    public enum PushBackViewDispositionKind { Redraw = 0, ObsoleteLateral = 1, FrontalBOfOneWay = 2, NoAddress = 3 }

    /// <summary>The disposition of every view of the rack, by definition key (ordinal).</summary>
    public sealed class PushBackViewDispositionPlan
    {
        internal PushBackViewDispositionPlan(IEnumerable<KeyValuePair<string, PushBackViewDispositionKind>> members)
        {
            if (members == null) throw new ArgumentNullException(nameof(members));
            Members = members.OrderBy(pair => pair.Key, StringComparer.Ordinal).ToList().AsReadOnly();
            Redraw = KeysOf(PushBackViewDispositionKind.Redraw);
            ObsoleteLaterals = KeysOf(PushBackViewDispositionKind.ObsoleteLateral);
            FrontalBOfOneWay = KeysOf(PushBackViewDispositionKind.FrontalBOfOneWay);
            NoAddress = KeysOf(PushBackViewDispositionKind.NoAddress);
            AllObsolete = Members.Count > 0 && Members.All(pair => pair.Value == PushBackViewDispositionKind.ObsoleteLateral);
        }

        public IReadOnlyList<KeyValuePair<string, PushBackViewDispositionKind>> Members { get; }

        public IReadOnlyList<string> Redraw { get; }

        public IReadOnlyList<string> ObsoleteLaterals { get; }

        public IReadOnlyList<string> FrontalBOfOneWay { get; }

        public IReadOnlyList<string> NoAddress { get; }

        /// <summary>There are members and every one of them is an obsolete lateral.</summary>
        public bool AllObsolete { get; }

        private IReadOnlyList<string> KeysOf(PushBackViewDispositionKind kind)
            => Members.Where(pair => pair.Value == kind).Select(pair => pair.Key).ToList().AsReadOnly();
    }

    /// <summary>I-64 F4B PB-T3a (D-PBT3-06) — the disposition of the views of a rack after an edition.</summary>
    public static class PushBackViewDispositions
    {
        public static PushBackViewDispositionPlan Of(
            IEnumerable<KeyValuePair<string, RackEmbedDocument>> members, PushBackSystem outgoing, RackCatalog catalog)
            => new PushBackViewDispositionPlan(
                members.Select(pair => new KeyValuePair<string, PushBackViewDispositionKind>(pair.Key, PushBackViewDispositionKind.Redraw)));
    }
}
