using System;
using System.Collections.Generic;
using RackCad.Application.Persistence;
using RackCad.Application.ProjectVariables;

namespace RackCad.Application.Systems.Shared
{
    public enum RackAuthoredComparisonOutcome
    {
        Single = 1,
        Divergent = 2,
        Unreadable = 3,
    }

    public sealed class RackAuthoredComparisonResult<TAuthored>
    {
        private RackAuthoredComparisonResult(
            RackAuthoredComparisonOutcome outcome,
            TAuthored authored,
            string diagnostic)
        {
            Outcome = outcome;
            Authored = authored;
            Diagnostic = diagnostic;
        }

        public RackAuthoredComparisonOutcome Outcome { get; }
        public TAuthored Authored { get; }
        public string Diagnostic { get; }

        internal static RackAuthoredComparisonResult<TAuthored> Single(TAuthored authored)
            => new RackAuthoredComparisonResult<TAuthored>(RackAuthoredComparisonOutcome.Single, authored, null);

        internal static RackAuthoredComparisonResult<TAuthored> Divergent(string diagnostic)
            => new RackAuthoredComparisonResult<TAuthored>(RackAuthoredComparisonOutcome.Divergent, default, diagnostic);

        internal static RackAuthoredComparisonResult<TAuthored> Unreadable(string diagnostic)
            => new RackAuthoredComparisonResult<TAuthored>(RackAuthoredComparisonOutcome.Unreadable, default, diagnostic);
    }

    public interface IRackAuthoredComparatorPort<in TInput, TAuthored>
    {
        string Kind { get; }
        RackAuthoredComparisonResult<TAuthored> Compare(TInput input);
    }

    public sealed class SelectiveAuthoredComparisonInput
    {
        public SelectiveAuthoredComparisonInput(
            string rackId,
            IReadOnlyList<ProjectVariableScanEntry> siblings)
        {
            RackId = rackId;
            Siblings = siblings;
        }

        public string RackId { get; }
        public IReadOnlyList<ProjectVariableScanEntry> Siblings { get; }
    }

    /// <summary>
    /// Per-kind authored comparators. Selective delegates to its existing include-by-default structural
    /// authority. Kinds without demonstrated authored equivalence fail closed; no sibling is selected.
    /// </summary>
    public static partial class RackAuthoredComparatorPorts
    {
        public static IRackAuthoredComparatorPort<SelectiveAuthoredComparisonInput, SelectivePalletDesignDocument>
            Selective() => new SelectiveComparator();

        public static IRackAuthoredComparatorPort<TInput, TAuthored> Dynamic<TInput, TAuthored>()
            => Unsupported<TInput, TAuthored>(RackEmbedDocument.KindDynamic);

        public static IRackAuthoredComparatorPort<TInput, TAuthored> PushBack<TInput, TAuthored>()
            => Unsupported<TInput, TAuthored>(RackEmbedDocument.KindPushBack);

        public static IRackAuthoredComparatorPort<TInput, TAuthored> Cantilever<TInput, TAuthored>()
            => Unsupported<TInput, TAuthored>(RackEmbedDocument.KindCantilever);

        public static IRackAuthoredComparatorPort<TInput, TAuthored> Cabecera<TInput, TAuthored>()
            => Unsupported<TInput, TAuthored>(RackEmbedDocument.KindCabecera);

        public static IRackAuthoredComparatorPort<TInput, TAuthored> Cama<TInput, TAuthored>()
            => Unsupported<TInput, TAuthored>(RackEmbedDocument.KindCama);

        private static IRackAuthoredComparatorPort<TInput, TAuthored> Unsupported<TInput, TAuthored>(string kind)
            => new UnsupportedComparator<TInput, TAuthored>(kind);

        private sealed class SelectiveComparator
            : IRackAuthoredComparatorPort<SelectiveAuthoredComparisonInput, SelectivePalletDesignDocument>
        {
            public string Kind => RackEmbedDocument.KindSelective;

            public RackAuthoredComparisonResult<SelectivePalletDesignDocument> Compare(
                SelectiveAuthoredComparisonInput input)
            {
                if (input == null)
                {
                    return RackAuthoredComparisonResult<SelectivePalletDesignDocument>.Unreadable(
                        "The Selective authored comparison input is absent.");
                }

                var existing = SelectiveAuthoredAuthority.Resolve(input.RackId, input.Siblings);
                switch (existing.Outcome)
                {
                    case AuthoredAuthorityOutcome.Single:
                        return RackAuthoredComparisonResult<SelectivePalletDesignDocument>.Single(existing.Authored);
                    case AuthoredAuthorityOutcome.Divergent:
                        return RackAuthoredComparisonResult<SelectivePalletDesignDocument>.Divergent(existing.Error);
                    default:
                        return RackAuthoredComparisonResult<SelectivePalletDesignDocument>.Unreadable(existing.Error);
                }
            }
        }

        private sealed class UnsupportedComparator<TInput, TAuthored>
            : IRackAuthoredComparatorPort<TInput, TAuthored>
        {
            internal UnsupportedComparator(string kind) => Kind = kind;

            public string Kind { get; }

            public RackAuthoredComparisonResult<TAuthored> Compare(TInput input)
                => RackAuthoredComparisonResult<TAuthored>.Unreadable(
                    "No authored comparator is demonstrated for kind '" + Kind + "'.");
        }
    }
}
