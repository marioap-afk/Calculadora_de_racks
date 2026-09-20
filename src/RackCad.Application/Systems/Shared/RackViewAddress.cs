using System;
using System.Globalization;

namespace RackCad.Application.Systems.Shared
{
    public enum RackViewVariantKind
    {
        Whole,
        Fondo,
        Post,
        FlowEnd,
        PushBackCut,
        Station
    }

    public enum RackFlowEnd
    {
        Exit,
        Entrance
    }

    public enum RackPushBackEnd
    {
        EntradaSalida,
        Posterior
    }

    public enum RackPushBackSide
    {
        A,
        B
    }

    /// <summary>
    /// Discriminated variant of one semantic rack view. Only the named factories create values, so a fondo cannot be
    /// confused with a post or station even though all three use zero-based integers.
    /// </summary>
    public readonly struct RackViewVariant : IEquatable<RackViewVariant>
    {
        private RackViewVariant(
            RackViewVariantKind kind,
            int index,
            RackFlowEnd flowEnd,
            RackPushBackEnd pushBackEnd,
            RackPushBackSide pushBackSide)
        {
            Kind = kind;
            Index = index;
            FlowEnd = flowEnd;
            PushBackEnd = pushBackEnd;
            PushBackSide = pushBackSide;
        }

        public RackViewVariantKind Kind { get; }
        public int Index { get; }
        public RackFlowEnd FlowEnd { get; }
        public RackPushBackEnd PushBackEnd { get; }
        public RackPushBackSide PushBackSide { get; }

        public static RackViewVariant Whole() => new RackViewVariant(RackViewVariantKind.Whole, -1, default, default, default);
        public static RackViewVariant Fondo(int index) => Indexed(RackViewVariantKind.Fondo, index);
        public static RackViewVariant Post(int index) => Indexed(RackViewVariantKind.Post, index);
        public static RackViewVariant Station(int index) => Indexed(RackViewVariantKind.Station, index);
        public static RackViewVariant ForFlowEnd(RackFlowEnd end) =>
            new RackViewVariant(RackViewVariantKind.FlowEnd, -1, end, default, default);
        public static RackViewVariant PushBackCut(RackPushBackEnd end, RackPushBackSide side) =>
            new RackViewVariant(RackViewVariantKind.PushBackCut, -1, default, end, side);

        private static RackViewVariant Indexed(RackViewVariantKind kind, int index)
        {
            if (index < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            return new RackViewVariant(kind, index, default, default, default);
        }

        public bool Equals(RackViewVariant other) =>
            Kind == other.Kind && Index == other.Index && FlowEnd == other.FlowEnd
            && PushBackEnd == other.PushBackEnd && PushBackSide == other.PushBackSide;

        public override bool Equals(object obj) => obj is RackViewVariant other && Equals(other);
        public override int GetHashCode() => HashCode.Combine(Kind, Index, FlowEnd, PushBackEnd, PushBackSide);

        public override string ToString()
        {
            switch (Kind)
            {
                case RackViewVariantKind.Whole:
                    return "Whole";
                case RackViewVariantKind.Fondo:
                    return "Fondo(" + Index.ToString(CultureInfo.InvariantCulture) + ")";
                case RackViewVariantKind.Post:
                    return "Post(" + Index.ToString(CultureInfo.InvariantCulture) + ")";
                case RackViewVariantKind.FlowEnd:
                    return "FlowEnd(" + FlowEnd + ")";
                case RackViewVariantKind.PushBackCut:
                    return "PushBackCut(" + PushBackEnd + "," + PushBackSide + ")";
                case RackViewVariantKind.Station:
                    return "Station(" + Index.ToString(CultureInfo.InvariantCulture) + ")";
                default:
                    throw new InvalidOperationException("Rack view variant is not defined.");
            }
        }
    }

    /// <summary>A semantic view address. It contains no geometry, UI index or consumer decision.</summary>
    public readonly struct RackViewAddress : IEquatable<RackViewAddress>
    {
        private RackViewAddress(DimensionViewKind kind, RackViewVariant variant)
        {
            var valid = variant.Kind == RackViewVariantKind.Whole
                || kind == DimensionViewKind.Frontal && (variant.Kind == RackViewVariantKind.Fondo
                    || variant.Kind == RackViewVariantKind.FlowEnd
                    || variant.Kind == RackViewVariantKind.PushBackCut)
                || kind == DimensionViewKind.Lateral && (variant.Kind == RackViewVariantKind.Post
                    || variant.Kind == RackViewVariantKind.Station);
            if (!valid)
            {
                throw new ArgumentException("View kind and variant do not form a semantic rack address.", nameof(variant));
            }

            Kind = kind;
            Variant = variant;
        }

        public DimensionViewKind Kind { get; }
        public RackViewVariant Variant { get; }

        public static RackViewAddress Whole(DimensionViewKind kind) => new RackViewAddress(kind, RackViewVariant.Whole());
        public static RackViewAddress Fondo(int index) => new RackViewAddress(DimensionViewKind.Frontal, RackViewVariant.Fondo(index));
        public static RackViewAddress Post(int index) => new RackViewAddress(DimensionViewKind.Lateral, RackViewVariant.Post(index));
        public static RackViewAddress FlowEnd(RackFlowEnd end) =>
            new RackViewAddress(DimensionViewKind.Frontal, RackViewVariant.ForFlowEnd(end));
        public static RackViewAddress PushBackCut(RackPushBackEnd end, RackPushBackSide side) =>
            new RackViewAddress(DimensionViewKind.Frontal, RackViewVariant.PushBackCut(end, side));
        public static RackViewAddress Station(int index) =>
            new RackViewAddress(DimensionViewKind.Lateral, RackViewVariant.Station(index));

        public bool Equals(RackViewAddress other) => Kind == other.Kind && Variant.Equals(other.Variant);
        public override bool Equals(object obj) => obj is RackViewAddress other && Equals(other);
        public override int GetHashCode() => HashCode.Combine(Kind, Variant);
        public override string ToString() => Kind + "/" + Variant;
    }
}
