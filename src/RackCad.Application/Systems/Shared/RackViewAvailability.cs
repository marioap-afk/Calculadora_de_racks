using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Domain.Systems.Shared;

namespace RackCad.Application.Systems.Shared
{
    public enum RackViewAvailabilityStatus
    {
        Available,
        VariantNotPresent,
        SystemDoesNotSupportKind,
        Unavailable
    }

    public readonly struct RackViewAvailabilityResult
    {
        internal RackViewAvailabilityResult(RackViewAvailabilityStatus status, string code)
        {
            Status = status;
            Code = code;
        }

        public RackViewAvailabilityStatus Status { get; }
        public string Code { get; }
    }

    public abstract class RackViewAvailabilityFacts
    {
        protected RackViewAvailabilityFacts(RackSystemKind systemKind) => SystemKind = systemKind;
        public RackSystemKind SystemKind { get; }
        internal abstract bool Contains(RackViewAddress address);
    }

    public sealed class CabeceraViewAvailabilityFacts : RackViewAvailabilityFacts
    {
        public CabeceraViewAvailabilityFacts() : base(RackSystemKind.Selective) { }
        internal override bool Contains(RackViewAddress address) =>
            address.Variant.Kind == RackViewVariantKind.Whole
            && (address.Kind == DimensionViewKind.Lateral || address.Kind == DimensionViewKind.Planta);
    }

    public sealed class CamaViewAvailabilityFacts : RackViewAvailabilityFacts
    {
        public CamaViewAvailabilityFacts() : base(RackSystemKind.Cama) { }
        internal override bool Contains(RackViewAddress address) =>
            address.Variant.Kind == RackViewVariantKind.Whole && address.Kind == DimensionViewKind.Lateral;
    }

    public sealed class SelectiveViewAvailabilityFacts : RackViewAvailabilityFacts
    {
        private readonly HashSet<int> posts;

        public SelectiveViewAvailabilityFacts(int fondoCount, IEnumerable<int> postIndexes)
            : base(RackSystemKind.SelectiveRack)
        {
            FondoCount = Math.Max(0, fondoCount);
            posts = new HashSet<int>(postIndexes ?? Enumerable.Empty<int>());
        }

        public int FondoCount { get; }

        internal override bool Contains(RackViewAddress address)
        {
            switch (address.Variant.Kind)
            {
                case RackViewVariantKind.Whole:
                    return address.Kind == DimensionViewKind.Planta;
                case RackViewVariantKind.Fondo:
                    return address.Variant.Index < FondoCount;
                case RackViewVariantKind.Post:
                    return posts.Contains(address.Variant.Index);
                default:
                    return false;
            }
        }
    }

    public sealed class DynamicViewAvailabilityFacts : RackViewAvailabilityFacts
    {
        private readonly HashSet<int> posts;

        public DynamicViewAvailabilityFacts(IEnumerable<int> postIndexes) : base(RackSystemKind.PalletFlow)
        {
            posts = new HashSet<int>(postIndexes ?? Enumerable.Empty<int>());
        }

        internal override bool Contains(RackViewAddress address) =>
            address.Variant.Kind == RackViewVariantKind.Whole && address.Kind == DimensionViewKind.Planta
            || address.Variant.Kind == RackViewVariantKind.FlowEnd
            || address.Variant.Kind == RackViewVariantKind.Post && posts.Contains(address.Variant.Index);
    }

    public sealed class PushBackViewAvailabilityFacts : RackViewAvailabilityFacts
    {
        private readonly HashSet<int> posts;
        private readonly bool isComposite;

        public PushBackViewAvailabilityFacts(IEnumerable<int> postIndexes, bool isComposite)
            : base(RackSystemKind.PushBack)
        {
            posts = new HashSet<int>(postIndexes ?? Enumerable.Empty<int>());
            this.isComposite = isComposite;
        }

        internal override bool Contains(RackViewAddress address)
        {
            if (address.Variant.Kind == RackViewVariantKind.Whole)
                return address.Kind == DimensionViewKind.Planta;
            if (address.Variant.Kind == RackViewVariantKind.Post)
                return posts.Contains(address.Variant.Index);
            if (address.Variant.Kind == RackViewVariantKind.PushBackCut)
                return address.Variant.PushBackSide == RackPushBackSide.A || isComposite;
            return false;
        }
    }

    public sealed class CantileverViewAvailabilityFacts : RackViewAvailabilityFacts
    {
        public CantileverViewAvailabilityFacts(int stationCount) : base(RackSystemKind.Cantilever)
        {
            StationCount = Math.Max(0, stationCount);
        }

        public int StationCount { get; }

        internal override bool Contains(RackViewAddress address) =>
            address.Variant.Kind == RackViewVariantKind.Whole
            || address.Variant.Kind == RackViewVariantKind.Station
               && address.Variant.Index < StationCount;
    }

    /// <summary>Evaluates physical facts only. It never selects a fallback address or consumer action.</summary>
    public static class RackViewAvailability
    {
        public static RackViewAvailabilityResult Evaluate(
            DecodedRackView decoded, RackViewAvailabilityFacts facts)
        {
            if (!decoded.HasAddress || facts == null)
                return Result(RackViewAvailabilityStatus.Unavailable, "VIEW_ADDRESS_UNAVAILABLE");
            if (decoded.SystemKind != facts.SystemKind)
                return Result(RackViewAvailabilityStatus.SystemDoesNotSupportKind, "VIEW_KIND_NOT_SUPPORTED");
            return facts.Contains(decoded.Address)
                ? Result(RackViewAvailabilityStatus.Available, null)
                : Result(RackViewAvailabilityStatus.VariantNotPresent, "VIEW_VARIANT_NOT_PRESENT");
        }

        private static RackViewAvailabilityResult Result(RackViewAvailabilityStatus status, string code) =>
            new RackViewAvailabilityResult(status, code);
    }
}
