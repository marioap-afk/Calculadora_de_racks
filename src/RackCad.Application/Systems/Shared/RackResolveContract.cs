using System;
using RackCad.Application.Persistence;

namespace RackCad.Application.Systems.Shared
{
    public enum RackResolveFailure
    {
        None,
        UnsupportedKind,
        UnreadableInput,
        Blocked,
        ResolutionFailure
    }

    /// <summary>A typed result from one per-kind resolver invocation. It carries diagnostics, never consumer policy.</summary>
    public readonly struct RackResolveResult<TResolved>
    {
        private RackResolveResult(
            string kind,
            bool isSuccess,
            TResolved resolved,
            RackResolveFailure failure,
            string code,
            string diagnostic)
        {
            Kind = kind;
            IsSuccess = isSuccess;
            Resolved = resolved;
            Failure = failure;
            Code = code;
            Diagnostic = diagnostic;
        }

        public string Kind { get; }
        public bool IsSuccess { get; }
        public TResolved Resolved { get; }
        public RackResolveFailure Failure { get; }
        public string Code { get; }
        public string Diagnostic { get; }

        public static RackResolveResult<TResolved> Success(string kind, TResolved resolved)
            => new RackResolveResult<TResolved>(kind, true, resolved, RackResolveFailure.None, "RESOLVED", null);

        public static RackResolveResult<TResolved> Unsupported(string kind, string diagnostic = null)
            => FailureResult(
                kind,
                RackResolveFailure.UnsupportedKind,
                "UNSUPPORTED_KIND",
                diagnostic);

        internal static RackResolveResult<TResolved> FailureResult(
            string kind,
            RackResolveFailure failure,
            string code,
            string diagnostic)
            => new RackResolveResult<TResolved>(kind, false, default, failure, code, diagnostic);
    }

    public interface IRackResolvePort<in TInput, TResolved>
    {
        string Kind { get; }
        RackResolveResult<TResolved> Resolve(TInput input);
    }

    /// <summary>
    /// Calls one existing per-kind resolver exactly once and normalizes only its outcome. The delegate remains the
    /// resolution authority; this adapter contains no geometry, fallback or representative-sibling algorithm.
    /// </summary>
    public sealed class RackResolveAdapter<TInput, TResolved> : IRackResolvePort<TInput, TResolved>
    {
        private readonly Func<TInput, TResolved> resolver;
        private readonly Func<TResolved, string> blockedDiagnostic;

        internal RackResolveAdapter(
            string kind,
            Func<TInput, TResolved> resolver,
            Func<TResolved, string> blockedDiagnostic = null)
        {
            if (string.IsNullOrWhiteSpace(kind)) throw new ArgumentException("A resolver adapter needs a kind.", nameof(kind));
            this.resolver = resolver ?? throw new ArgumentNullException(nameof(resolver));
            this.blockedDiagnostic = blockedDiagnostic;
            Kind = kind;
        }

        public string Kind { get; }

        public RackResolveResult<TResolved> Resolve(TInput input)
        {
            if (ReferenceEquals(input, null))
            {
                return RackResolveResult<TResolved>.FailureResult(
                    Kind,
                    RackResolveFailure.UnreadableInput,
                    "UNREADABLE_INPUT",
                    "The authored input is absent.");
            }

            try
            {
                var resolved = resolver(input);
                if (ReferenceEquals(resolved, null))
                {
                    return RackResolveResult<TResolved>.FailureResult(
                        Kind,
                        RackResolveFailure.ResolutionFailure,
                        "RESOLVER_RETURNED_NULL",
                        "The resolver did not produce a result.");
                }

                var blocked = blockedDiagnostic?.Invoke(resolved);
                return string.IsNullOrWhiteSpace(blocked)
                    ? RackResolveResult<TResolved>.Success(Kind, resolved)
                    : RackResolveResult<TResolved>.FailureResult(
                        Kind,
                        RackResolveFailure.Blocked,
                        "RESOLUTION_BLOCKED",
                        blocked);
            }
            catch (Exception ex)
            {
                return RackResolveResult<TResolved>.FailureResult(
                    Kind,
                    RackResolveFailure.ResolutionFailure,
                    "RESOLUTION_FAILED",
                    ex.Message);
            }
        }
    }

    /// <summary>Explicit per-kind composition points. Each caller supplies the existing typed resolver it owns.</summary>
    public static class RackResolvePorts
    {
        public static RackResolveAdapter<TInput, TResolved> Selective<TInput, TResolved>(
            Func<TInput, TResolved> resolver,
            Func<TResolved, string> blockedDiagnostic = null)
            => Create(RackEmbedDocument.KindSelective, resolver, blockedDiagnostic);

        public static RackResolveAdapter<TInput, TResolved> Dynamic<TInput, TResolved>(
            Func<TInput, TResolved> resolver,
            Func<TResolved, string> blockedDiagnostic = null)
            => Create(RackEmbedDocument.KindDynamic, resolver, blockedDiagnostic);

        public static RackResolveAdapter<TInput, TResolved> PushBack<TInput, TResolved>(
            Func<TInput, TResolved> resolver,
            Func<TResolved, string> blockedDiagnostic = null)
            => Create(RackEmbedDocument.KindPushBack, resolver, blockedDiagnostic);

        public static RackResolveAdapter<TInput, TResolved> Cantilever<TInput, TResolved>(
            Func<TInput, TResolved> resolver,
            Func<TResolved, string> blockedDiagnostic = null)
            => Create(RackEmbedDocument.KindCantilever, resolver, blockedDiagnostic);

        public static RackResolveAdapter<TInput, TResolved> Cabecera<TInput, TResolved>(
            Func<TInput, TResolved> resolver,
            Func<TResolved, string> blockedDiagnostic = null)
            => Create(RackEmbedDocument.KindCabecera, resolver, blockedDiagnostic);

        public static RackResolveAdapter<TInput, TResolved> Cama<TInput, TResolved>(
            Func<TInput, TResolved> resolver,
            Func<TResolved, string> blockedDiagnostic = null)
            => Create(RackEmbedDocument.KindCama, resolver, blockedDiagnostic);

        private static RackResolveAdapter<TInput, TResolved> Create<TInput, TResolved>(
            string kind,
            Func<TInput, TResolved> resolver,
            Func<TResolved, string> blockedDiagnostic)
            => new RackResolveAdapter<TInput, TResolved>(kind, resolver, blockedDiagnostic);
    }
}
