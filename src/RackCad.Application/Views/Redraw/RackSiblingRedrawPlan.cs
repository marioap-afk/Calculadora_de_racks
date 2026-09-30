using System;
using System.Collections.Generic;

namespace RackCad.Application.Views.Redraw
{
    public enum RackSiblingRedrawDisposition
    {
        MutateNow,
        DeferToFirstPlacement,
        NoMutation
    }

    public sealed class RackSiblingPreparedUnit<TUnit>
    {
        public RackSiblingPreparedUnit(RackSiblingMember member, TUnit unit)
        {
            Member = member ?? throw new ArgumentNullException(nameof(member));
            Unit = unit;
        }

        public RackSiblingMember Member { get; }
        public TUnit Unit { get; }
    }

    public sealed class RackSiblingRedrawPlan<TUnit>
    {
        internal RackSiblingRedrawPlan(
            IReadOnlyList<RackSiblingPreparedUnit<TUnit>> redraw,
            IReadOnlyList<RackSiblingPreparedUnit<TUnit>> erase,
            IReadOnlyList<RackSiblingMember> readOnly,
            IReadOnlyList<RackSiblingMember> blocking,
            IReadOnlyList<RackSiblingMember> survivors,
            RackSiblingRedrawDisposition disposition)
        {
            RedrawUnits = redraw;
            EraseUnits = erase;
            ReadOnly = readOnly;
            Blocking = blocking;
            Survivors = survivors;
            Disposition = disposition;
        }

        public IReadOnlyList<RackSiblingPreparedUnit<TUnit>> RedrawUnits { get; }
        public IReadOnlyList<RackSiblingPreparedUnit<TUnit>> EraseUnits { get; }
        public IReadOnlyList<RackSiblingMember> ReadOnly { get; }
        public IReadOnlyList<RackSiblingMember> Blocking { get; }
        public IReadOnlyList<RackSiblingMember> Survivors { get; }
        public RackSiblingRedrawDisposition Disposition { get; }

        public static RackSiblingRedrawPlan<TUnit> Create(
            RackSiblingMembershipSnapshot membership,
            IReadOnlyList<RackSiblingPreparedUnit<TUnit>> prepared)
        {
            if (membership == null) throw new ArgumentNullException(nameof(membership));
            if (prepared == null) throw new ArgumentNullException(nameof(prepared));

            var units = new Dictionary<RackSiblingMember, RackSiblingPreparedUnit<TUnit>>();
            foreach (var item in prepared)
            {
                if (item == null || !units.TryAdd(item.Member, item))
                {
                    throw new ArgumentException("Prepared units must be non-null and unique per member.", nameof(prepared));
                }
            }

            var redraw = new List<RackSiblingPreparedUnit<TUnit>>();
            var erase = new List<RackSiblingPreparedUnit<TUnit>>();
            var readOnly = new List<RackSiblingMember>();
            var blocking = new List<RackSiblingMember>();
            var survivors = new List<RackSiblingMember>();
            foreach (var member in membership.Members)
            {
                switch (member.Kind)
                {
                    case RackSiblingMembershipKind.Redraw:
                        redraw.Add(RequiredUnit(units, member));
                        if (member.IsSurvivor) survivors.Add(member);
                        break;
                    case RackSiblingMembershipKind.Erase:
                        erase.Add(RequiredUnit(units, member));
                        break;
                    case RackSiblingMembershipKind.ReadOnly:
                        readOnly.Add(member);
                        break;
                    case RackSiblingMembershipKind.BlockingUnreadable:
                        blocking.Add(member);
                        break;
                }
            }

            var disposition = blocking.Count != 0 || redraw.Count == 0 && erase.Count == 0
                ? RackSiblingRedrawDisposition.NoMutation
                : survivors.Count == 0 && erase.Count != 0
                    ? RackSiblingRedrawDisposition.DeferToFirstPlacement
                    : RackSiblingRedrawDisposition.MutateNow;
            return new RackSiblingRedrawPlan<TUnit>(redraw, erase, readOnly, blocking, survivors, disposition);
        }

        private static RackSiblingPreparedUnit<TUnit> RequiredUnit(
            IReadOnlyDictionary<RackSiblingMember, RackSiblingPreparedUnit<TUnit>> units,
            RackSiblingMember member)
        {
            if (!units.TryGetValue(member, out var unit))
            {
                throw new ArgumentException("Every REDRAW and ERASE member must be prepared: " + member.Fact.DefinitionKey);
            }

            return unit;
        }
    }
}
