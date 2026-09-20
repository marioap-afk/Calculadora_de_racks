using System;
using RackCad.Domain.Systems.Shared;

namespace RackCad.Application.Systems.Shared
{
    public enum RackViewSyntaxDisposition
    {
        Canonical,
        Canonicalizable,
        Coerced,
        Invalid
    }

    public readonly struct RackViewSyntax
    {
        public RackViewSyntax(string kind, string view, int section)
        {
            Kind = kind;
            View = view;
            Section = section;
        }

        public string Kind { get; }
        public string View { get; }
        public int Section { get; }

        public override string ToString() => (View ?? "null") + "/" + Section;
    }

    public readonly struct DecodedRackKind
    {
        internal DecodedRackKind(string originalKind, RackSystemKind systemKind, RackViewSyntaxDisposition disposition)
        {
            OriginalKind = originalKind;
            SystemKind = systemKind;
            Disposition = disposition;
        }

        public string OriginalKind { get; }
        public RackSystemKind SystemKind { get; }
        public RackViewSyntaxDisposition Disposition { get; }
        public bool IsValid => Disposition != RackViewSyntaxDisposition.Invalid;
    }

    public readonly struct DecodedRackView
    {
        internal DecodedRackView(
            RackViewSyntax originalSyntax,
            RackSystemKind systemKind,
            RackViewAddress address,
            bool hasAddress,
            RackViewSyntaxDisposition disposition,
            string coercionCode)
        {
            OriginalSyntax = originalSyntax;
            SystemKind = systemKind;
            Address = address;
            HasAddress = hasAddress;
            Disposition = disposition;
            CoercionCode = coercionCode;
        }

        public RackViewSyntax OriginalSyntax { get; }
        public RackSystemKind SystemKind { get; }
        public RackViewAddress Address { get; }
        public bool HasAddress { get; }
        public RackViewSyntaxDisposition Disposition { get; }
        public string CoercionCode { get; }
    }

    /// <summary>
    /// Total codec for persisted View/Section syntax. It reports observed coercion and never accepts, rejects, warns,
    /// prompts, erases or decides whether a consumer continues.
    /// </summary>
    public static class RackViewCodec
    {
        /// <summary>Recognizes only the three persisted view tokens. It does not trim or apply a kind fallback.</summary>
        public static bool TryDecodeViewKind(string view, out DimensionViewKind kind)
        {
            if (Token(view, "frontal", out _))
            {
                kind = DimensionViewKind.Frontal;
                return true;
            }

            if (Token(view, "lateral", out _))
            {
                kind = DimensionViewKind.Lateral;
                return true;
            }

            if (Token(view, "planta", out _))
            {
                kind = DimensionViewKind.Planta;
                return true;
            }

            kind = default;
            return false;
        }

        public static DecodedRackKind DecodeKind(string kind)
        {
            if (TryCanonicalKind(kind, out var systemKind, out var canonical))
            {
                return new DecodedRackKind(
                    kind,
                    systemKind,
                    canonical ? RackViewSyntaxDisposition.Canonical : RackViewSyntaxDisposition.Canonicalizable);
            }

            return new DecodedRackKind(kind, default, RackViewSyntaxDisposition.Invalid);
        }

        public static DecodedRackView Decode(string kind, string view, int section)
        {
            var syntax = new RackViewSyntax(kind, view, section);
            var decodedKind = DecodeKind(kind);
            if (!decodedKind.IsValid)
            {
                return Invalid(syntax);
            }

            var decoded = DecodeKnown(decodedKind.SystemKind, syntax);
            if (!decoded.HasAddress || decoded.Disposition == RackViewSyntaxDisposition.Coerced
                || decodedKind.Disposition == RackViewSyntaxDisposition.Canonical)
            {
                return decoded;
            }

            return Valid(
                syntax,
                decodedKind.SystemKind,
                decoded.Address,
                RackViewSyntaxDisposition.Canonicalizable,
                decoded.CoercionCode);
        }

        public static RackViewSyntax Encode(RackSystemKind systemKind, RackViewAddress address)
        {
            var kind = CanonicalKind(systemKind);
            switch (systemKind)
            {
                case RackSystemKind.SelectiveRack:
                    return EncodeSelective(kind, address);
                case RackSystemKind.PalletFlow:
                    return EncodeDynamic(kind, address);
                case RackSystemKind.PushBack:
                    return EncodePushBack(kind, address);
                case RackSystemKind.Cantilever:
                    return EncodeCantilever(kind, address);
                case RackSystemKind.Selective:
                    return EncodeWhole(kind, address, DimensionViewKind.Lateral, allowPlanta: true);
                case RackSystemKind.Cama:
                    if (address.Kind == DimensionViewKind.Lateral && address.Variant.Kind == RackViewVariantKind.Whole)
                    {
                        return new RackViewSyntax(kind, null, -1);
                    }

                    break;
            }

            throw new ArgumentException("Address is not supported by the rack kind.", nameof(address));
        }

        private static DecodedRackView DecodeKnown(RackSystemKind kind, RackViewSyntax syntax)
        {
            switch (kind)
            {
                case RackSystemKind.SelectiveRack:
                    return DecodeSelective(syntax, kind);
                case RackSystemKind.PalletFlow:
                    return DecodeDynamic(syntax, kind);
                case RackSystemKind.PushBack:
                    return DecodePushBack(syntax, kind);
                case RackSystemKind.Cantilever:
                    return DecodeCantilever(syntax, kind);
                case RackSystemKind.Selective:
                    return DecodeCabecera(syntax, kind);
                case RackSystemKind.Cama:
                    return DecodeCama(syntax, kind);
                default:
                    return Invalid(syntax);
            }
        }

        private static DecodedRackView DecodeSelective(RackViewSyntax syntax, RackSystemKind kind)
        {
            if (Token(syntax.View, "lateral", out var exact))
            {
                if (syntax.Section < 0) return Invalid(syntax);
                return Valid(syntax, kind, Address(DimensionViewKind.Lateral, RackViewVariant.Post(syntax.Section)),
                    exact ? RackViewSyntaxDisposition.Canonical : RackViewSyntaxDisposition.Canonicalizable);
            }

            if (Token(syntax.View, "planta", out exact))
            {
                var coerced = syntax.Section != -1;
                return Valid(syntax, kind, Address(DimensionViewKind.Planta, RackViewVariant.Whole()),
                    coerced ? RackViewSyntaxDisposition.Coerced
                        : exact ? RackViewSyntaxDisposition.Canonical : RackViewSyntaxDisposition.Canonicalizable,
                    coerced ? "WholeViewSectionIgnored" : null);
            }

            if (Token(syntax.View, "frontal", out exact))
            {
                if (syntax.Section < 0)
                {
                    return Valid(syntax, kind, Address(DimensionViewKind.Frontal, RackViewVariant.Fondo(0)),
                        RackViewSyntaxDisposition.Coerced, "SelectiveNegativeFondoToZero");
                }

                return Valid(syntax, kind, Address(DimensionViewKind.Frontal, RackViewVariant.Fondo(syntax.Section)),
                    exact ? RackViewSyntaxDisposition.Canonical : RackViewSyntaxDisposition.Canonicalizable);
            }

            var fondo = Math.Max(0, syntax.Section);
            var code = syntax.Section < 0
                ? "SelectiveDefaultFrontalAndFondoZero"
                : "SelectiveNonLateralNonPlantaToFrontal";
            return Valid(syntax, kind, Address(DimensionViewKind.Frontal, RackViewVariant.Fondo(fondo)),
                RackViewSyntaxDisposition.Coerced, code);
        }

        private static DecodedRackView DecodeDynamic(RackViewSyntax syntax, RackSystemKind kind)
        {
            if (Token(syntax.View, "planta", out var exact))
            {
                var coerced = syntax.Section != -1;
                return Valid(syntax, kind, Address(DimensionViewKind.Planta, RackViewVariant.Whole()),
                    coerced ? RackViewSyntaxDisposition.Coerced
                        : exact ? RackViewSyntaxDisposition.Canonical : RackViewSyntaxDisposition.Canonicalizable,
                    coerced ? "WholeViewSectionIgnored" : null);
            }

            if (Token(syntax.View, "frontal", out exact))
            {
                var end = syntax.Section == 1 ? RackFlowEnd.Entrance : RackFlowEnd.Exit;
                var coerced = syntax.Section != 0 && syntax.Section != 1;
                return Valid(syntax, kind, Address(DimensionViewKind.Frontal, RackViewVariant.ForFlowEnd(end)),
                    coerced ? RackViewSyntaxDisposition.Coerced
                        : exact ? RackViewSyntaxDisposition.Canonical : RackViewSyntaxDisposition.Canonicalizable,
                    coerced ? "DynamicNonEntranceToExit" : null);
            }

            var isLateral = Token(syntax.View, "lateral", out exact);
            var post = Math.Max(0, syntax.Section);
            if (isLateral)
            {
                return Valid(syntax, kind, Address(DimensionViewKind.Lateral, RackViewVariant.Post(post)),
                    syntax.Section < 0 ? RackViewSyntaxDisposition.Coerced
                        : exact ? RackViewSyntaxDisposition.Canonical : RackViewSyntaxDisposition.Canonicalizable,
                    syntax.Section < 0 ? "DynamicNegativePostToZero" : null);
            }

            var code = syntax.Section < 0 && string.IsNullOrEmpty(syntax.View)
                ? "DynamicDefaultLateralAndPostZero"
                : "DynamicNonFrontalNonPlantaToLateral";
            return Valid(syntax, kind, Address(DimensionViewKind.Lateral, RackViewVariant.Post(post)),
                RackViewSyntaxDisposition.Coerced, code);
        }

        private static DecodedRackView DecodePushBack(RackViewSyntax syntax, RackSystemKind kind)
        {
            if (Token(syntax.View, "planta", out var exact) && syntax.Section == -1)
            {
                return Valid(syntax, kind, Address(DimensionViewKind.Planta, RackViewVariant.Whole()),
                    exact ? RackViewSyntaxDisposition.Canonical : RackViewSyntaxDisposition.Canonicalizable);
            }

            if (Token(syntax.View, "lateral", out exact) && syntax.Section >= 0)
            {
                return Valid(syntax, kind, Address(DimensionViewKind.Lateral, RackViewVariant.Post(syntax.Section)),
                    exact ? RackViewSyntaxDisposition.Canonical : RackViewSyntaxDisposition.Canonicalizable);
            }

            if (Token(syntax.View, "frontal", out exact) && syntax.Section >= 0 && syntax.Section <= 3)
            {
                var end = syntax.Section % 2 == 0 ? RackPushBackEnd.EntradaSalida : RackPushBackEnd.Posterior;
                var side = syntax.Section >= 2 ? RackPushBackSide.B : RackPushBackSide.A;
                return Valid(syntax, kind, Address(DimensionViewKind.Frontal, RackViewVariant.PushBackCut(end, side)),
                    exact ? RackViewSyntaxDisposition.Canonical : RackViewSyntaxDisposition.Canonicalizable);
            }

            return Invalid(syntax);
        }

        private static DecodedRackView DecodeCantilever(RackViewSyntax syntax, RackSystemKind kind)
        {
            if (Token(syntax.View, "lateral", out var exact) && syntax.Section >= 0)
            {
                return Valid(syntax, kind, Address(DimensionViewKind.Lateral, RackViewVariant.Station(syntax.Section)),
                    exact ? RackViewSyntaxDisposition.Canonical : RackViewSyntaxDisposition.Canonicalizable);
            }

            if (Token(syntax.View, "frontal", out exact) && syntax.Section == -1)
            {
                return Valid(syntax, kind, Address(DimensionViewKind.Frontal, RackViewVariant.Whole()),
                    exact ? RackViewSyntaxDisposition.Canonical : RackViewSyntaxDisposition.Canonicalizable);
            }

            if (Token(syntax.View, "planta", out exact) && syntax.Section == -1)
            {
                return Valid(syntax, kind, Address(DimensionViewKind.Planta, RackViewVariant.Whole()),
                    exact ? RackViewSyntaxDisposition.Canonical : RackViewSyntaxDisposition.Canonicalizable);
            }

            return Invalid(syntax);
        }

        private static DecodedRackView DecodeCabecera(RackViewSyntax syntax, RackSystemKind kind)
        {
            if (Token(syntax.View, "planta", out var exact))
            {
                var coerced = syntax.Section != -1;
                return Valid(syntax, kind, Address(DimensionViewKind.Planta, RackViewVariant.Whole()),
                    coerced ? RackViewSyntaxDisposition.Coerced
                        : exact ? RackViewSyntaxDisposition.Canonical : RackViewSyntaxDisposition.Canonicalizable,
                    coerced ? "WholeViewSectionIgnored" : null);
            }

            if (Token(syntax.View, "lateral", out exact) && syntax.Section == -1)
            {
                return Valid(syntax, kind, Address(DimensionViewKind.Lateral, RackViewVariant.Whole()),
                    exact ? RackViewSyntaxDisposition.Canonical : RackViewSyntaxDisposition.Canonicalizable);
            }

            return Valid(syntax, kind, Address(DimensionViewKind.Lateral, RackViewVariant.Whole()),
                RackViewSyntaxDisposition.Coerced, "CabeceraNonPlantaToLateral");
        }

        private static DecodedRackView DecodeCama(RackViewSyntax syntax, RackSystemKind kind)
        {
            var address = Address(DimensionViewKind.Lateral, RackViewVariant.Whole());
            if (syntax.View == null && syntax.Section == -1)
            {
                return Valid(syntax, kind, address, RackViewSyntaxDisposition.Canonical);
            }

            if (Token(syntax.View, "lateral", out _) && syntax.Section == -1)
            {
                return Valid(syntax, kind, address, RackViewSyntaxDisposition.Canonicalizable);
            }

            return Valid(syntax, kind, address, RackViewSyntaxDisposition.Coerced, "CamaDescriptorIgnored");
        }

        private static RackViewSyntax EncodeSelective(string kind, RackViewAddress address)
        {
            if (address.Kind == DimensionViewKind.Frontal && address.Variant.Kind == RackViewVariantKind.Fondo)
                return new RackViewSyntax(kind, "frontal", address.Variant.Index);
            if (address.Kind == DimensionViewKind.Lateral && address.Variant.Kind == RackViewVariantKind.Post)
                return new RackViewSyntax(kind, "lateral", address.Variant.Index);
            return EncodeWhole(kind, address, DimensionViewKind.Planta, allowPlanta: true);
        }

        private static RackViewSyntax EncodeDynamic(string kind, RackViewAddress address)
        {
            if (address.Kind == DimensionViewKind.Lateral && address.Variant.Kind == RackViewVariantKind.Post)
                return new RackViewSyntax(kind, "lateral", address.Variant.Index);
            if (address.Kind == DimensionViewKind.Frontal && address.Variant.Kind == RackViewVariantKind.FlowEnd)
                return new RackViewSyntax(kind, "frontal", address.Variant.FlowEnd == RackFlowEnd.Entrance ? 1 : 0);
            return EncodeWhole(kind, address, DimensionViewKind.Planta, allowPlanta: true);
        }

        private static RackViewSyntax EncodePushBack(string kind, RackViewAddress address)
        {
            if (address.Kind == DimensionViewKind.Lateral && address.Variant.Kind == RackViewVariantKind.Post)
                return new RackViewSyntax(kind, "lateral", address.Variant.Index);
            if (address.Kind == DimensionViewKind.Frontal && address.Variant.Kind == RackViewVariantKind.PushBackCut)
            {
                var section = (address.Variant.PushBackSide == RackPushBackSide.B ? 2 : 0)
                    + (address.Variant.PushBackEnd == RackPushBackEnd.Posterior ? 1 : 0);
                return new RackViewSyntax(kind, "frontal", section);
            }

            return EncodeWhole(kind, address, DimensionViewKind.Planta, allowPlanta: true);
        }

        private static RackViewSyntax EncodeCantilever(string kind, RackViewAddress address)
        {
            if (address.Kind == DimensionViewKind.Lateral && address.Variant.Kind == RackViewVariantKind.Station)
                return new RackViewSyntax(kind, "lateral", address.Variant.Index);
            return EncodeWhole(kind, address, address.Kind, allowPlanta: true);
        }

        private static RackViewSyntax EncodeWhole(
            string kind, RackViewAddress address, DimensionViewKind expected, bool allowPlanta)
        {
            if (address.Variant.Kind != RackViewVariantKind.Whole
                || (address.Kind != expected && (!allowPlanta || address.Kind != DimensionViewKind.Planta)))
            {
                throw new ArgumentException("Address is not a whole view.", nameof(address));
            }

            return new RackViewSyntax(kind, address.Kind.ToString().ToLowerInvariant(), -1);
        }

        private static RackViewAddress Address(DimensionViewKind kind, RackViewVariant variant)
        {
            switch (variant.Kind)
            {
                case RackViewVariantKind.Whole: return RackViewAddress.Whole(kind);
                case RackViewVariantKind.Fondo: return RackViewAddress.Fondo(variant.Index);
                case RackViewVariantKind.Post: return RackViewAddress.Post(variant.Index);
                case RackViewVariantKind.FlowEnd: return RackViewAddress.FlowEnd(variant.FlowEnd);
                case RackViewVariantKind.PushBackCut:
                    return RackViewAddress.PushBackCut(variant.PushBackEnd, variant.PushBackSide);
                case RackViewVariantKind.Station: return RackViewAddress.Station(variant.Index);
                default: throw new ArgumentOutOfRangeException(nameof(variant));
            }
        }

        private static DecodedRackView Valid(
            RackViewSyntax syntax,
            RackSystemKind kind,
            RackViewAddress address,
            RackViewSyntaxDisposition disposition,
            string coercionCode = null) =>
            new DecodedRackView(syntax, kind, address, true, disposition, coercionCode);

        private static DecodedRackView Invalid(RackViewSyntax syntax) =>
            new DecodedRackView(syntax, default, default, false, RackViewSyntaxDisposition.Invalid, null);

        private static bool Token(string actual, string expected, out bool exact)
        {
            exact = string.Equals(actual, expected, StringComparison.Ordinal);
            return exact || string.Equals(actual, expected, StringComparison.OrdinalIgnoreCase);
        }

        private static bool TryCanonicalKind(string value, out RackSystemKind kind, out bool exact)
        {
            foreach (var candidate in new[]
            {
                RackSystemKind.SelectiveRack, RackSystemKind.PalletFlow, RackSystemKind.PushBack,
                RackSystemKind.Cantilever, RackSystemKind.Selective, RackSystemKind.Cama
            })
            {
                var token = CanonicalKind(candidate);
                if (string.Equals(value, token, StringComparison.OrdinalIgnoreCase))
                {
                    kind = candidate;
                    exact = string.Equals(value, token, StringComparison.Ordinal);
                    return true;
                }
            }

            kind = default;
            exact = false;
            return false;
        }

        private static string CanonicalKind(RackSystemKind kind)
        {
            switch (kind)
            {
                case RackSystemKind.SelectiveRack: return "selective";
                case RackSystemKind.PalletFlow: return "dynamic";
                case RackSystemKind.PushBack: return "pushback";
                case RackSystemKind.Cantilever: return "cantilever";
                case RackSystemKind.Selective: return "cabecera";
                case RackSystemKind.Cama: return "cama";
                default: throw new ArgumentOutOfRangeException(nameof(kind), kind, "Rack kind has no view codec.");
            }
        }
    }
}
