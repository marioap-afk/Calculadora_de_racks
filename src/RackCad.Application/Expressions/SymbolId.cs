using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace RackCad.Application.Expressions
{
    /// <summary>
    /// The closed set of symbol namespaces (I-49, Proposal V6 P3.2 and P5.2; ADR-0040 D5).
    ///
    /// <para>
    /// Exactly ONE is active, <c>projectVariable</c>. <c>rack</c> and <c>project</c> are reserved CONCEPTUALLY for ID20
    /// and deliberately absent here: not registered, not resolvable, and no productive path creates them (P25.2). The
    /// grammar reserves them too: <c>Rack.Frentes</c> is namespace syntax that binds to <c>UnknownNamespace</c>.
    /// </para>
    /// </summary>
    public enum SymbolNamespace
    {
        ProjectVariable = 1,
    }

    /// <summary>
    /// The explicit two-way token table of <see cref="SymbolNamespace"/> and the key comparer each namespace fixes (P3.2,
    /// P17.10). Tokens are compared ordinally and never derived from the enum member name.
    /// </summary>
    public static class SymbolNamespaces
    {
        private static readonly IReadOnlyList<SymbolNamespace> DeclaredNamespaces =
            new ReadOnlyCollection<SymbolNamespace>(new[] { SymbolNamespace.ProjectVariable });

        public static IReadOnlyList<SymbolNamespace> All => DeclaredNamespaces;

        public static string Token(SymbolNamespace symbolNamespace)
        {
            switch (symbolNamespace)
            {
                case SymbolNamespace.ProjectVariable: return "projectVariable";
                default: throw new ArgumentOutOfRangeException(nameof(symbolNamespace), symbolNamespace, "Unregistered symbol namespace.");
            }
        }

        public static bool TryParseToken(string token, out SymbolNamespace symbolNamespace)
        {
            switch (token)
            {
                case "projectVariable":
                    symbolNamespace = SymbolNamespace.ProjectVariable;
                    return true;

                default:
                    symbolNamespace = default;
                    return false;
            }
        }

        /// <summary>
        /// The comparer of keys in a namespace. For <c>projectVariable</c> the key is the <c>VariableId</c> text with its
        /// current equality, ordinal ignoring case (P8.1): a GUID written by different writers in different cases is one
        /// identity.
        /// </summary>
        public static StringComparer KeyComparer(SymbolNamespace symbolNamespace)
        {
            switch (symbolNamespace)
            {
                case SymbolNamespace.ProjectVariable: return StringComparer.OrdinalIgnoreCase;
                default: throw new ArgumentOutOfRangeException(nameof(symbolNamespace), symbolNamespace, "Unregistered symbol namespace.");
            }
        }

        internal static bool IsRegistered(SymbolNamespace symbolNamespace) => symbolNamespace == SymbolNamespace.ProjectVariable;
    }

    /// <summary>
    /// The identity of a symbol: <c>(SymbolNamespace, Key)</c> (P3.2; ADR-0041 D5). A NAME is never an identity (P3.5):
    /// names only take part when writing and when showing.
    ///
    /// <para>
    /// For <c>projectVariable</c> the key is the <c>VariableId</c> text (Amendment A2 §3.1, §3.2). It is valid when it is not
    /// empty, equals its own <c>Trim()</c> and <c>Guid.TryParse</c> accepts it: exactly the texts a <c>VariableId</c> can
    /// have, in any layout —D, N, B, P, X and the compatibility ones. The parsed value only decides that: it is never
    /// kept, compared, hashed or used to disambiguate. The key is kept exactly as written and compared with the namespace
    /// comparer, so two layouts of one GUID are two identities and case is not a difference. Every valid key can be written
    /// back as text: the formatter writes it as <c>Q(key)</c> (A2 §3.6).
    /// </para>
    /// </summary>
    public sealed class SymbolId : IEquatable<SymbolId>, IComparable<SymbolId>
    {
        public SymbolId(SymbolNamespace symbolNamespace, string key)
        {
            if (!SymbolNamespaces.IsRegistered(symbolNamespace))
            {
                throw new ArgumentOutOfRangeException(nameof(symbolNamespace), symbolNamespace, "Unregistered symbol namespace.");
            }

            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            if (!IsValidProjectVariableKey(key))
            {
                throw new ArgumentException(
                    "A projectVariable key must be non-empty, equal to its own trim and a GUID in a layout the runtime reads.",
                    nameof(key));
            }

            Namespace = symbolNamespace;
            Key = key;
        }

        public SymbolNamespace Namespace { get; }

        /// <summary>The key exactly as it was written.</summary>
        public string Key { get; }

        public static SymbolId ProjectVariable(string key) => new SymbolId(SymbolNamespace.ProjectVariable, key);

        public bool Equals(SymbolId other)
            => other != null
               && Namespace == other.Namespace
               && SymbolNamespaces.KeyComparer(Namespace).Equals(Key, other.Key);

        public override bool Equals(object obj) => obj is SymbolId other && Equals(other);

        public override int GetHashCode()
            => unchecked(((int)Namespace * 397) ^ SymbolNamespaces.KeyComparer(Namespace).GetHashCode(Key));

        /// <summary>The deterministic order of P11.1: namespace token ordinally, then the key with its namespace comparer.</summary>
        public int CompareTo(SymbolId other)
        {
            if (other == null)
            {
                return 1;
            }

            var byNamespace = string.CompareOrdinal(SymbolNamespaces.Token(Namespace), SymbolNamespaces.Token(other.Namespace));

            return byNamespace != 0 ? byNamespace : SymbolNamespaces.KeyComparer(Namespace).Compare(Key, other.Key);
        }

        public override string ToString() => SymbolNamespaces.Token(Namespace) + ":" + Key;

        public static bool operator ==(SymbolId left, SymbolId right)
            => ReferenceEquals(left, right) || (left is object && left.Equals(right));

        public static bool operator !=(SymbolId left, SymbolId right) => !(left == right);

        /// <summary>
        /// The neutral validity of a <c>projectVariable</c> key (Amendment A2 §3.2), the one rule the constructor and the
        /// exact-key qualifier of the lexer share. Whether the text is a GUID is the only thing the parse decides.
        /// </summary>
        internal static bool IsValidProjectVariableKey(string key)
            => !string.IsNullOrEmpty(key)
               && string.Equals(key, key.Trim(), StringComparison.Ordinal)
               && Guid.TryParse(key, out _);
    }
}
