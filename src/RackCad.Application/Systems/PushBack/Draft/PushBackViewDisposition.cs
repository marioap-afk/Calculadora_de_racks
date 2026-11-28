using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Catalogs;
using RackCad.Application.Persistence;
using RackCad.Application.Systems.Shared;
using RackCad.Domain.Systems.PushBack;
using RackCad.Domain.Systems.Shared;

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

    /// <summary>
    /// I-64 F4B PB-T3a (D-PBT3-06) — the disposition of the views of a rack after an edition, as the classic editor decides
    /// it (the lateral cuts of the new system and the pure availability): a view that is still available is redrawn; a lateral
    /// with no cut in the new system is obsolete; the frontal of side B of a one-way rack is rejected; a view with no address
    /// is rejected too. The edition never creates a view.
    /// </summary>
    public static class PushBackViewDispositions
    {
        public static PushBackViewDispositionPlan Of(
            IEnumerable<KeyValuePair<string, RackEmbedDocument>> members, PushBackSystem outgoing, RackCatalog catalog)
        {
            if (members == null) throw new ArgumentNullException(nameof(members));
            if (outgoing == null) throw new ArgumentNullException(nameof(outgoing));

            // The facts are computed ONCE.
            var cuts = new PushBackSystemLateralBuilder().Cortes(outgoing, catalog);
            var facts = new PushBackViewAvailabilityFacts(cuts.Select(cut => cut.PostIndex), outgoing.IsComposite);

            var result = new List<KeyValuePair<string, PushBackViewDispositionKind>>();
            foreach (var pair in members)
            {
                var envelope = pair.Value;
                var decoded = RackViewCodec.Decode(envelope?.Kind, envelope?.View, envelope?.Section ?? -1);
                PushBackViewDispositionKind kind;
                if (!decoded.HasAddress) kind = PushBackViewDispositionKind.NoAddress;
                else if (RackViewAvailability.Evaluate(decoded, facts).Status == RackViewAvailabilityStatus.Available) kind = PushBackViewDispositionKind.Redraw;
                else if (decoded.Address.Kind == DimensionViewKind.Lateral) kind = PushBackViewDispositionKind.ObsoleteLateral;
                else if (decoded.Address.Kind == DimensionViewKind.Frontal
                         && decoded.Address.Variant.Kind == RackViewVariantKind.PushBackCut
                         && decoded.Address.Variant.PushBackSide == RackPushBackSide.B) kind = PushBackViewDispositionKind.FrontalBOfOneWay;
                else kind = PushBackViewDispositionKind.NoAddress;
                result.Add(new KeyValuePair<string, PushBackViewDispositionKind>(pair.Key, kind));
            }

            return new PushBackViewDispositionPlan(result);
        }
    }
}
