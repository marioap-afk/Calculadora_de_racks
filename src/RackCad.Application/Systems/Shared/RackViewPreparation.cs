using System;
using RackCad.Application.Persistence;

namespace RackCad.Application.Systems.Shared
{
    public enum RackViewPreparationFailure
    {
        None,
        UnsupportedAddress,
        UnreadableResolvedInput,
        FrameUnavailable,
        BuilderFailure
    }

    /// <summary>
    /// Final typed prepared-view carrier. The payload remains the existing per-kind plan type and requirements
    /// are extracted from that payload, never from the generated view name.
    /// </summary>
    public sealed class RackPreparedView<TPayload>
    {
        internal RackPreparedView(
            string kind,
            RackViewAddress address,
            RackViewFrame frame,
            string baseName,
            System.Collections.Generic.IReadOnlyList<LibraryBlockRequirement> blockRequirements,
            TPayload payload)
        {
            Kind = kind;
            Address = address;
            Frame = frame;
            BaseName = baseName;
            BlockRequirements = blockRequirements;
            Payload = payload;
        }

        public string Kind { get; }
        public RackViewAddress Address { get; }
        public RackViewFrame Frame { get; }
        public string BaseName { get; }
        public System.Collections.Generic.IReadOnlyList<LibraryBlockRequirement> BlockRequirements { get; }
        public TPayload Payload { get; }
    }

    public readonly struct RackViewPreparationResult<TPayload>
    {
        private RackViewPreparationResult(
            bool isSuccess,
            RackPreparedView<TPayload> prepared,
            RackViewPreparationFailure failure,
            string code,
            string diagnostic)
        {
            IsSuccess = isSuccess;
            Prepared = prepared;
            Failure = failure;
            Code = code;
            Diagnostic = diagnostic;
        }

        public bool IsSuccess { get; }
        public RackPreparedView<TPayload> Prepared { get; }
        public RackViewPreparationFailure Failure { get; }
        public string Code { get; }
        public string Diagnostic { get; }

        internal static RackViewPreparationResult<TPayload> Success(RackPreparedView<TPayload> prepared)
            => new RackViewPreparationResult<TPayload>(true, prepared, RackViewPreparationFailure.None, "PREPARED", null);

        internal static RackViewPreparationResult<TPayload> Failed(
            RackViewPreparationFailure failure,
            string code,
            string diagnostic)
            => new RackViewPreparationResult<TPayload>(false, null, failure, code, diagnostic);
    }

    public interface IRackViewPreparationPort<in TResolved, TPayload>
    {
        string Kind { get; }

        RackViewPreparationResult<TPayload> Prepare(
            TResolved resolved,
            RackViewAddress address,
            RackViewFrameResult frame,
            string baseName);
    }

    /// <summary>
    /// Invokes one existing per-kind builder exactly once and carries its typed plan unchanged. It neither interprets
    /// plan geometry nor materializes, imports, opens transactions or chooses a consumer fallback.
    /// </summary>
    public sealed class RackViewPreparationAdapter<TResolved, TPayload>
        : IRackViewPreparationPort<TResolved, TPayload>
    {
        private readonly Func<TResolved, RackViewAddress, TPayload> builder;
        private readonly Func<RackViewAddress, bool> supports;
        private readonly IRackBlockRequirementExtractor<TPayload> requirements;

        internal RackViewPreparationAdapter(
            string kind,
            Func<TResolved, RackViewAddress, TPayload> builder,
            Func<RackViewAddress, bool> supports,
            IRackBlockRequirementExtractor<TPayload> requirements)
        {
            if (string.IsNullOrWhiteSpace(kind)) throw new ArgumentException("A preparation adapter needs a kind.", nameof(kind));
            this.builder = builder ?? throw new ArgumentNullException(nameof(builder));
            this.supports = supports ?? throw new ArgumentNullException(nameof(supports));
            this.requirements = requirements ?? throw new ArgumentNullException(nameof(requirements));
            Kind = kind;
        }

        public string Kind { get; }

        public RackViewPreparationResult<TPayload> Prepare(
            TResolved resolved,
            RackViewAddress address,
            RackViewFrameResult frame,
            string baseName)
        {
            if (ReferenceEquals(resolved, null))
            {
                return RackViewPreparationResult<TPayload>.Failed(
                    RackViewPreparationFailure.UnreadableResolvedInput,
                    "UNREADABLE_RESOLVED_INPUT",
                    "The resolved input is absent.");
            }

            if (!supports(address))
            {
                return RackViewPreparationResult<TPayload>.Failed(
                    RackViewPreparationFailure.UnsupportedAddress,
                    "UNSUPPORTED_ADDRESS",
                    address.ToString());
            }

            if (!frame.IsAvailable)
            {
                return RackViewPreparationResult<TPayload>.Failed(
                    RackViewPreparationFailure.FrameUnavailable,
                    "FRAME_" + frame.Failure.ToString().ToUpperInvariant(),
                    frame.Failure.ToString());
            }

            try
            {
                var payload = builder(resolved, address);
                if (ReferenceEquals(payload, null))
                {
                    return RackViewPreparationResult<TPayload>.Failed(
                        RackViewPreparationFailure.BuilderFailure,
                        "BUILDER_RETURNED_NULL",
                        "The builder did not produce a typed plan.");
                }

                var blockRequirements = requirements.Extract(payload);
                return RackViewPreparationResult<TPayload>.Success(
                    new RackPreparedView<TPayload>(Kind, address, frame.Frame, baseName, blockRequirements, payload));
            }
            catch (Exception ex)
            {
                return RackViewPreparationResult<TPayload>.Failed(
                    RackViewPreparationFailure.BuilderFailure,
                    "BUILDER_FAILED",
                    ex.Message);
            }
        }
    }

    /// <summary>Explicit per-kind composition points; each receives the existing typed builder and address support.</summary>
    public static class RackViewPreparationPorts
    {
        public static RackViewPreparationAdapter<TResolved, TPayload> Selective<TResolved, TPayload>(
            Func<TResolved, RackViewAddress, TPayload> builder,
            Func<RackViewAddress, bool> supports,
            IRackBlockRequirementExtractor<TPayload> requirements)
            => Create(RackEmbedDocument.KindSelective, builder, supports, requirements);

        public static RackViewPreparationAdapter<TResolved, TPayload> Dynamic<TResolved, TPayload>(
            Func<TResolved, RackViewAddress, TPayload> builder,
            Func<RackViewAddress, bool> supports,
            IRackBlockRequirementExtractor<TPayload> requirements)
            => Create(RackEmbedDocument.KindDynamic, builder, supports, requirements);

        public static RackViewPreparationAdapter<TResolved, TPayload> PushBack<TResolved, TPayload>(
            Func<TResolved, RackViewAddress, TPayload> builder,
            Func<RackViewAddress, bool> supports,
            IRackBlockRequirementExtractor<TPayload> requirements)
            => Create(RackEmbedDocument.KindPushBack, builder, supports, requirements);

        public static RackViewPreparationAdapter<TResolved, TPayload> Cantilever<TResolved, TPayload>(
            Func<TResolved, RackViewAddress, TPayload> builder,
            Func<RackViewAddress, bool> supports,
            IRackBlockRequirementExtractor<TPayload> requirements)
            => Create(RackEmbedDocument.KindCantilever, builder, supports, requirements);

        public static RackViewPreparationAdapter<TResolved, TPayload> Cabecera<TResolved, TPayload>(
            Func<TResolved, RackViewAddress, TPayload> builder,
            Func<RackViewAddress, bool> supports,
            IRackBlockRequirementExtractor<TPayload> requirements)
            => Create(RackEmbedDocument.KindCabecera, builder, supports, requirements);

        public static RackViewPreparationAdapter<TResolved, TPayload> Cama<TResolved, TPayload>(
            Func<TResolved, RackViewAddress, TPayload> builder,
            Func<RackViewAddress, bool> supports,
            IRackBlockRequirementExtractor<TPayload> requirements)
            => Create(RackEmbedDocument.KindCama, builder, supports, requirements);

        private static RackViewPreparationAdapter<TResolved, TPayload> Create<TResolved, TPayload>(
            string kind,
            Func<TResolved, RackViewAddress, TPayload> builder,
            Func<RackViewAddress, bool> supports,
            IRackBlockRequirementExtractor<TPayload> requirements)
            => new RackViewPreparationAdapter<TResolved, TPayload>(kind, builder, supports, requirements);
    }
}
