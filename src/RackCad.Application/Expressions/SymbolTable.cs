using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace RackCad.Application.Expressions
{
    /// <summary>
    /// The scopes of V6 P5.3. <see cref="Project"/>: one value per drawing. <see cref="Rack"/>: one value per rack, the
    /// scope of the <c>rack</c> entries and of the property consumers (I-63 D-04); a Project consumer that reads a Rack
    /// symbol is a scope violation (P25.4).
    /// </summary>
    public enum SymbolScope
    {
        Project = 1,
        Rack = 2,
    }

    public enum SymbolDefinitionKind
    {
        Literal = 1,
        Expression = 2,

        /// <summary>
        /// A leaf without a value in the table (I-63 D-16.3, the case P25.2 reserved): it has no expression and no edges,
        /// and whoever evaluates it supplies the value from outside the registry.
        /// </summary>
        Computed = 3,
    }

    /// <summary>
    /// A symbol's definition in the neutral model (P5.1): a literal <c>double</c>, a bound expression or a
    /// <see cref="SymbolDefinitionKind.Computed"/> leaf without a value. Reading the value of another case throws, with
    /// the same discipline as <c>VariableDefinition.LiteralValue</c>.
    /// </summary>
    public sealed class SymbolDefinition
    {
        private readonly double _literal;
        private readonly BoundExpression _expression;

        private SymbolDefinition(SymbolDefinitionKind kind, double literal, BoundExpression expression)
        {
            Kind = kind;
            _literal = literal;
            _expression = expression;
        }

        public SymbolDefinitionKind Kind { get; }

        public double LiteralValue
            => Kind == SymbolDefinitionKind.Literal
                ? _literal
                : throw new InvalidOperationException(
                    Kind == SymbolDefinitionKind.Computed
                        ? "This definition is a computed leaf, not a literal."
                        : "This definition is an expression, not a literal.");

        public BoundExpression Expression
            => Kind == SymbolDefinitionKind.Expression
                ? _expression
                : throw new InvalidOperationException(
                    Kind == SymbolDefinitionKind.Computed
                        ? "This definition is a computed leaf, not an expression."
                        : "This definition is a literal, not an expression.");

        /// <summary>A literal definition: every value of the engine is a finite double (P5.4).</summary>
        public static SymbolDefinition FromLiteral(double value)
        {
            if (double.IsNaN(value) || double.IsInfinity(value))
            {
                throw new ArgumentOutOfRangeException(nameof(value), value, "A literal definition must be a finite number.");
            }

            return new SymbolDefinition(SymbolDefinitionKind.Literal, value, null);
        }

        public static SymbolDefinition FromExpression(BoundExpression expression)
            => new SymbolDefinition(
                SymbolDefinitionKind.Expression,
                0,
                expression ?? throw new ArgumentNullException(nameof(expression)));

        /// <summary>A leaf without a value (I-63 D-16.3): no expression, no edges and no literal.</summary>
        public static SymbolDefinition FromComputed()
            => new SymbolDefinition(SymbolDefinitionKind.Computed, 0, null);
    }

    /// <summary>
    /// One symbol of a table: <c>{ SymbolId, SymbolScope, DisplayName, Definition }</c> (P5.1). There is deliberately NO
    /// <c>VariableType</c> and no type of <c>ProjectVariables</c>: the core names none of them, so ID23 can build its own
    /// table without that dependency (P26.1). The relation <c>SymbolId → VariableType</c> lives in the adapter (P8.9).
    ///
    /// <para>
    /// The display name is only for showing and for resolving while writing (P8.3). The core requires it to exist, not to
    /// be non-blank: that rule belongs to Project Variables, and the syntax of §4 can write any name, the empty one
    /// included (<c>{}</c>).
    /// </para>
    /// </summary>
    public sealed class SymbolEntry
    {
        public SymbolEntry(SymbolId id, SymbolScope scope, string displayName, SymbolDefinition definition)
        {
            if (scope != SymbolScope.Project && scope != SymbolScope.Rack)
            {
                throw new ArgumentOutOfRangeException(nameof(scope), scope, "Undeclared symbol scope.");
            }

            Id = id ?? throw new ArgumentNullException(nameof(id));
            Scope = scope;
            DisplayName = displayName ?? throw new ArgumentNullException(nameof(displayName));
            Definition = definition ?? throw new ArgumentNullException(nameof(definition));
        }

        public SymbolId Id { get; }

        public SymbolScope Scope { get; }

        public string DisplayName { get; }

        public SymbolDefinition Definition { get; }
    }

    /// <summary>
    /// The immutable symbol table of ONE snapshot (P6.1, P6.3).
    ///
    /// <para>
    /// Built only through the internal seam: from synthetic entries in the core and in tests (P6.5), and in G8 from the
    /// accredited <c>UsableProjectVariablesRegistry</c> by its adapter (P6.4). Plugin and UI cannot build one. A repeated
    /// identity is a programming error here: an ambiguous registry is <c>AmbiguousIdentity</c> BEFORE any table exists
    /// (P3.6).
    /// </para>
    /// <para>
    /// Name lookup is exact equality ignoring case ordinally, with no trimming, no Unicode normalization and no partial
    /// match (P7.2). Homonyms are legal and come back in the deterministic <see cref="SymbolId"/> order. The name indexes
    /// are PER NAMESPACE (I-63 D-16.4): the lookup of a name without namespace only sees <c>projectVariable</c> entries,
    /// a <c>rack</c> member only sees <c>rack</c> entries, and the <c>OperatorInName</c> patterns only come from
    /// <c>projectVariable</c> names.
    /// </para>
    /// </summary>
    public sealed class SymbolTable
    {
        private static readonly IReadOnlyList<SymbolEntry> NoEntries = new ReadOnlyCollection<SymbolEntry>(Array.Empty<SymbolEntry>());

        private readonly Dictionary<SymbolId, SymbolEntry> _byId;
        private readonly Dictionary<string, IReadOnlyList<SymbolEntry>> _byName;
        private readonly Dictionary<string, IReadOnlyList<SymbolEntry>> _rackByName;

        private SymbolTable(IReadOnlyList<SymbolEntry> entries)
        {
            Entries = entries;
            _byId = new Dictionary<SymbolId, SymbolEntry>();
            _byName = new Dictionary<string, IReadOnlyList<SymbolEntry>>(StringComparer.OrdinalIgnoreCase);
            _rackByName = new Dictionary<string, IReadOnlyList<SymbolEntry>>(StringComparer.OrdinalIgnoreCase);

            foreach (var entry in entries)
            {
                if (_byId.ContainsKey(entry.Id))
                {
                    throw new ArgumentException("The symbol " + entry.Id + " appears twice.", nameof(entries));
                }

                _byId.Add(entry.Id, entry);
            }

            foreach (var group in entries
                         .Where(entry => entry.Id.Namespace == SymbolNamespace.ProjectVariable)
                         .GroupBy(entry => entry.DisplayName, StringComparer.OrdinalIgnoreCase))
            {
                _byName.Add(group.Key, new ReadOnlyCollection<SymbolEntry>(group.ToList()));
            }

            foreach (var group in entries
                         .Where(entry => entry.Id.Namespace == SymbolNamespace.Rack)
                         .GroupBy(entry => entry.DisplayName, StringComparer.OrdinalIgnoreCase))
            {
                _rackByName.Add(group.Key, new ReadOnlyCollection<SymbolEntry>(group.ToList()));
            }

            HasRackEntries = _rackByName.Count > 0;

            OperatorNames = OperatorInNameDetector.PatternsOf(_byName);
        }

        public static SymbolTable Empty { get; } = new SymbolTable(NoEntries);

        /// <summary>Every entry, in the deterministic <see cref="SymbolId"/> order, whatever the input order was.</summary>
        public IReadOnlyList<SymbolEntry> Entries { get; }

        /// <summary>Whether the table holds at least one <c>rack</c> entry: the binder offers <c>Rack.member</c> only then (D-16.5).</summary>
        internal bool HasRackEntries { get; }

        /// <summary>The <c>projectVariable</c> display names that contain operators, split once for the <c>OperatorInName</c> check.</summary>
        internal IReadOnlyList<OperatorInNameDetector.NamePattern> OperatorNames { get; }

        internal static SymbolTable Create(IEnumerable<SymbolEntry> entries)
        {
            if (entries == null)
            {
                throw new ArgumentNullException(nameof(entries));
            }

            var list = entries.ToList();

            if (list.Any(entry => entry == null))
            {
                throw new ArgumentException("A symbol table cannot contain a null entry.", nameof(entries));
            }

            list.Sort((left, right) => left.Id.CompareTo(right.Id));
            return new SymbolTable(new ReadOnlyCollection<SymbolEntry>(list));
        }

        public bool TryGet(SymbolId id, out SymbolEntry entry)
        {
            entry = null;
            return id != null && _byId.TryGetValue(id, out entry);
        }

        /// <summary>
        /// Every <c>projectVariable</c> entry whose display name is exactly <paramref name="displayName"/>, ignoring case
        /// ordinally: the lookup of a name without namespace, which never sees <c>rack</c> entries (I-63 D-16.4).
        /// </summary>
        public IReadOnlyList<SymbolEntry> FindByDisplayName(string displayName)
            => displayName != null && _byName.TryGetValue(displayName, out var entries) ? entries : NoEntries;

        /// <summary>Every <c>rack</c> entry whose display name is exactly <paramref name="displayName"/>, ignoring case ordinally.</summary>
        internal IReadOnlyList<SymbolEntry> FindRackMember(string displayName)
            => displayName != null && _rackByName.TryGetValue(displayName, out var entries) ? entries : NoEntries;
    }
}
