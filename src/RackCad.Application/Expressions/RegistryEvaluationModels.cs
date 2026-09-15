using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace RackCad.Application.Expressions
{
    /// <summary>An immutable representative chain whose tail is shared, so prepending one dependency is O(1).</summary>
    internal sealed class CauseChain : IReadOnlyList<SymbolId>
    {
        private static readonly IReadOnlyList<SymbolId> EmptyTail = Array.Empty<SymbolId>();

        private readonly SymbolId _head;
        private readonly IReadOnlyList<SymbolId> _tail;

        private CauseChain(SymbolId head, IReadOnlyList<SymbolId> tail)
        {
            _head = head;
            _tail = tail;
            Count = checked(1 + tail.Count);
        }

        public int Count { get; }

        public SymbolId this[int index]
        {
            get
            {
                if (index < 0 || index >= Count)
                {
                    throw new ArgumentOutOfRangeException(nameof(index));
                }

                return index == 0 ? _head : _tail[index - 1];
            }
        }

        internal static IReadOnlyList<SymbolId> Single(SymbolId head) => new CauseChain(head, EmptyTail);

        internal static IReadOnlyList<SymbolId> Prepend(SymbolId head, IReadOnlyList<SymbolId> tail)
            => new CauseChain(head, tail ?? throw new ArgumentNullException(nameof(tail)));

        public IEnumerator<SymbolId> GetEnumerator()
        {
            yield return _head;
            foreach (var item in _tail)
            {
                yield return item;
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }

    /// <summary>The stable data of one invalid function call.</summary>
    public sealed class InvalidArgumentSignature : IEquatable<InvalidArgumentSignature>, IComparable<InvalidArgumentSignature>
    {
        public InvalidArgumentSignature(string token, int argumentCount)
        {
            Token = token ?? throw new ArgumentNullException(nameof(token));
            ArgumentCount = argumentCount;
        }

        public string Token { get; }

        public int ArgumentCount { get; }

        public int CompareTo(InvalidArgumentSignature other)
        {
            if (other == null)
            {
                return 1;
            }

            var byToken = string.CompareOrdinal(Token, other.Token);
            return byToken != 0 ? byToken : ArgumentCount.CompareTo(other.ArgumentCount);
        }

        public bool Equals(InvalidArgumentSignature other) => other != null && CompareTo(other) == 0;

        public override bool Equals(object obj) => obj is InvalidArgumentSignature other && Equals(other);

        public override int GetHashCode() => HashCode.Combine(StringComparer.Ordinal.GetHashCode(Token), ArgumentCount);
    }

    /// <summary>
    /// A structural cause root. Owner roots aggregate their stable data; a cycle root has members and no privileged owner.
    /// </summary>
    public sealed class RootSignature : IEquatable<RootSignature>, IComparable<RootSignature>
    {
        private static readonly IReadOnlyList<SymbolId> NoSymbols = ReadOnly(Array.Empty<SymbolId>());
        private static readonly IReadOnlyList<InvalidArgumentSignature> NoInvalidArguments =
            new ReadOnlyCollection<InvalidArgumentSignature>(Array.Empty<InvalidArgumentSignature>());

        private RootSignature(
            SymbolId owner,
            ExpressionDiagnosticCode code,
            IReadOnlyList<SymbolId> missingSymbols,
            IReadOnlyList<InvalidArgumentSignature> invalidArguments,
            IReadOnlyList<SymbolId> members)
        {
            Owner = owner;
            Code = code;
            MissingSymbols = missingSymbols;
            InvalidArguments = invalidArguments;
            Members = members;
        }

        public SymbolId Owner { get; }

        public ExpressionDiagnosticCode Code { get; }

        public IReadOnlyList<SymbolId> MissingSymbols { get; }

        public IReadOnlyList<InvalidArgumentSignature> InvalidArguments { get; }

        public IReadOnlyList<SymbolId> Members { get; }

        public string FunctionToken => InvalidArguments.Count == 1 ? InvalidArguments[0].Token : null;

        public int? ArgumentCount => InvalidArguments.Count == 1 ? InvalidArguments[0].ArgumentCount : (int?)null;

        internal static RootSignature Broken(SymbolId owner, IEnumerable<SymbolId> missing)
            => new RootSignature(owner, ExpressionDiagnosticCode.BrokenReference, ReadOnly(missing.OrderBy(id => id)), NoInvalidArguments, NoSymbols);

        internal static RootSignature Invalid(SymbolId owner, IEnumerable<InvalidArgumentSignature> invalid)
            => new RootSignature(
                owner,
                ExpressionDiagnosticCode.InvalidArguments,
                NoSymbols,
                new ReadOnlyCollection<InvalidArgumentSignature>(invalid.OrderBy(item => item).ToList()),
                NoSymbols);

        internal static RootSignature Intrinsic(SymbolId owner, ExpressionDiagnosticCode code)
            => new RootSignature(owner, code, NoSymbols, NoInvalidArguments, NoSymbols);

        internal static RootSignature Cycle(IReadOnlyList<SymbolId> members)
            => new RootSignature(null, ExpressionDiagnosticCode.Cycle, NoSymbols, NoInvalidArguments, members);

        public int CompareTo(RootSignature other)
        {
            if (other == null)
            {
                return 1;
            }

            var byCode = ((int)Code).CompareTo((int)other.Code);
            if (byCode != 0)
            {
                return byCode;
            }

            var identity = Owner ?? Members.FirstOrDefault();
            var otherIdentity = other.Owner ?? other.Members.FirstOrDefault();
            var byIdentity = CompareIds(identity, otherIdentity);
            if (byIdentity != 0)
            {
                return byIdentity;
            }

            var byMissing = CompareLists(MissingSymbols, other.MissingSymbols, CompareIds);
            if (byMissing != 0)
            {
                return byMissing;
            }

            var byInvalid = CompareLists(InvalidArguments, other.InvalidArguments, (left, right) => left.CompareTo(right));
            return byInvalid != 0 ? byInvalid : CompareLists(Members, other.Members, CompareIds);
        }

        public bool Equals(RootSignature other) => other != null && CompareTo(other) == 0;

        public override bool Equals(object obj) => obj is RootSignature other && Equals(other);

        public override int GetHashCode()
        {
            var hash = new HashCode();
            hash.Add((int)Code);
            hash.Add(Owner);
            foreach (var item in MissingSymbols)
            {
                hash.Add(item);
            }

            foreach (var item in InvalidArguments)
            {
                hash.Add(item);
            }

            foreach (var item in Members)
            {
                hash.Add(item);
            }

            return hash.ToHashCode();
        }

        private static int CompareIds(SymbolId left, SymbolId right)
            => ReferenceEquals(left, right) ? 0 : left == null ? -1 : left.CompareTo(right);

        private static int CompareLists<T>(IReadOnlyList<T> left, IReadOnlyList<T> right, Func<T, T, int> compare)
        {
            var count = Math.Min(left.Count, right.Count);
            for (var index = 0; index < count; index++)
            {
                var result = compare(left[index], right[index]);
                if (result != 0)
                {
                    return result;
                }
            }

            return left.Count.CompareTo(right.Count);
        }

        private static IReadOnlyList<SymbolId> ReadOnly(IEnumerable<SymbolId> values)
            => new ReadOnlyCollection<SymbolId>(values.ToList());
    }

    /// <summary>A registry-level semantic diagnostic, including graph cause data unavailable to the tree evaluator.</summary>
    public sealed class RegistryDiagnostic
    {
        private static readonly IReadOnlyList<SymbolId> NoSymbols =
            new ReadOnlyCollection<SymbolId>(Array.Empty<SymbolId>());

        private RegistryDiagnostic(
            ExpressionDiagnosticCode code,
            SymbolId cause,
            IReadOnlyList<SymbolId> relatedSymbols,
            IReadOnlyList<SymbolId> chain,
            RootSignature root,
            string functionToken,
            int? argumentCount)
        {
            Code = code;
            Cause = cause;
            RelatedSymbols = relatedSymbols;
            Chain = chain;
            Root = root;
            FunctionToken = functionToken;
            ArgumentCount = argumentCount;
        }

        public ExpressionDiagnosticCode Code { get; }

        public SymbolId Cause { get; }

        public IReadOnlyList<SymbolId> RelatedSymbols { get; }

        public IReadOnlyList<SymbolId> Chain { get; }

        public RootSignature Root { get; }

        public string FunctionToken { get; }

        public int? ArgumentCount { get; }

        internal static RegistryDiagnostic Broken(SymbolId missing)
            => new RegistryDiagnostic(ExpressionDiagnosticCode.BrokenReference, null, ReadOnly(new[] { missing }), NoSymbols, null, null, null);

        internal static RegistryDiagnostic Cycle(IReadOnlyList<SymbolId> members)
            => new RegistryDiagnostic(ExpressionDiagnosticCode.Cycle, null, members, NoSymbols, null, null, null);

        internal static RegistryDiagnostic DependencyFailed(SymbolId cause, IReadOnlyList<SymbolId> chain, RootSignature root)
            => new RegistryDiagnostic(ExpressionDiagnosticCode.DependencyFailed, cause, NoSymbols, chain, root, null, null);

        internal static RegistryDiagnostic Invalid(InvalidArgumentSignature invalid)
            => new RegistryDiagnostic(
                ExpressionDiagnosticCode.InvalidArguments,
                null,
                NoSymbols,
                NoSymbols,
                null,
                invalid.Token,
                invalid.ArgumentCount);

        internal static RegistryDiagnostic Intrinsic(ExpressionDiagnosticCode code)
            => new RegistryDiagnostic(code, null, NoSymbols, NoSymbols, null, null, null);

        internal static IReadOnlyList<RegistryDiagnostic> Ordered(IEnumerable<RegistryDiagnostic> diagnostics)
            => new ReadOnlyCollection<RegistryDiagnostic>(diagnostics.OrderBy(item => item, Comparer<RegistryDiagnostic>.Create(Compare)).ToList());

        private static int Compare(RegistryDiagnostic left, RegistryDiagnostic right)
        {
            var byCode = ((int)left.Code).CompareTo((int)right.Code);
            if (byCode != 0)
            {
                return byCode;
            }

            switch (left.Code)
            {
                case ExpressionDiagnosticCode.BrokenReference:
                    return left.RelatedSymbols[0].CompareTo(right.RelatedSymbols[0]);
                case ExpressionDiagnosticCode.DependencyFailed:
                    return left.Cause.CompareTo(right.Cause);
                case ExpressionDiagnosticCode.InvalidArguments:
                    var byToken = string.CompareOrdinal(left.FunctionToken, right.FunctionToken);
                    return byToken != 0 ? byToken : left.ArgumentCount.Value.CompareTo(right.ArgumentCount.Value);
                default:
                    return 0;
            }
        }

        private static IReadOnlyList<SymbolId> ReadOnly(IEnumerable<SymbolId> values)
            => new ReadOnlyCollection<SymbolId>(values.ToList());
    }

    /// <summary>The immutable result of one symbol in a registry snapshot.</summary>
    public sealed class RegistrySymbolResult
    {
        private readonly double _value;

        internal RegistrySymbolResult(
            bool succeeded,
            double value,
            IReadOnlyList<RegistryDiagnostic> diagnostics,
            IReadOnlyList<RootSignature> rootCauses)
        {
            Succeeded = succeeded;
            _value = value;
            Diagnostics = diagnostics;
            RootCauses = rootCauses;
        }

        public bool Succeeded { get; }

        public EvaluationOutcome Outcome => Succeeded ? EvaluationOutcome.Success : EvaluationOutcome.Failed;

        public double Value => Succeeded ? _value : throw new InvalidOperationException("A failed registry symbol has no value.");

        public IReadOnlyList<RegistryDiagnostic> Diagnostics { get; }

        public IReadOnlyList<RootSignature> RootCauses { get; }
    }
}
