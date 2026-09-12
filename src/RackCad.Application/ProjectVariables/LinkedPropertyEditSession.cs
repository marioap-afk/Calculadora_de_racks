using System;
using System.Collections.Generic;
using System.Globalization;

namespace RackCad.Application.ProjectVariables
{
    /// <summary>What the user has typed that is not committed yet.</summary>
    public enum LinkedPropertyDraftKind
    {
        /// <summary>Nothing pending: what is shown IS the committed state.</summary>
        Committed = 1,

        /// <summary>A number that parses, not committed yet.</summary>
        DraftLiteral = 2,

        /// <summary>A reference QUERY (it starts with '='). It filters; it resolves nothing.</summary>
        DraftReferenceQuery = 3,

        /// <summary>Text that is neither a number nor a reference query.</summary>
        InvalidDraft = 4,
    }

    /// <summary>How a pending draft may be resolved at a write boundary.</summary>
    public enum LinkedPropertyStageOutcome
    {
        /// <summary>Nothing pending. The boundary proceeds and nothing is applied.</summary>
        Clean = 1,

        /// <summary>A draft that does NOT change the source and is ready to apply.</summary>
        Ready = 2,

        /// <summary>The boundary must BLOCK: the draft is invalid, or it would change the source silently.</summary>
        Blocked = 3,
    }

    /// <summary>The verdict of <see cref="LinkedPropertyEditSession.TryStage"/>.</summary>
    public sealed class LinkedPropertyStageResult
    {
        private LinkedPropertyStageResult(LinkedPropertyStageOutcome outcome, string error)
        {
            Outcome = outcome;
            Error = error;
        }

        public LinkedPropertyStageOutcome Outcome { get; }

        /// <summary>Why the boundary is blocked. Null otherwise.</summary>
        public string Error { get; }

        public bool CanProceed => Outcome != LinkedPropertyStageOutcome.Blocked;

        internal static LinkedPropertyStageResult Clean { get; } =
            new LinkedPropertyStageResult(LinkedPropertyStageOutcome.Clean, null);

        internal static LinkedPropertyStageResult Ready { get; } =
            new LinkedPropertyStageResult(LinkedPropertyStageOutcome.Ready, null);

        internal static LinkedPropertyStageResult Blocked(string error)
            => new LinkedPropertyStageResult(LinkedPropertyStageOutcome.Blocked, error);
    }

    /// <summary>
    /// The editing semantics of ONE linked property, with no WPF in sight (I-48 G4C).
    ///
    /// <para>
    /// Every rule the reviews argued about lives here, which is the point: Enter, Escape, LostFocus, the
    /// bare <c>=</c>, the refusal to auto-select a single candidate, and the 20.13 freeze are all decidable
    /// from text plus a committed state. Putting them in a WPF control would make them unprovable without a
    /// message pump, and a rule nobody can exercise is a rule that drifts.
    /// </para>
    /// <para>
    /// The two asymmetries it exists to enforce:
    /// </para>
    /// <list type="bullet">
    /// <item><b><c>LostFocus</c> never changes <see cref="LinkedPropertySource"/></b> (V3-R06). It may commit a
    /// literal over a literal, because that keeps the source; it can never create, replace or remove a
    /// reference.</item>
    /// <item><b>A reference is only ever committed by explicit selection</b> (V3-R08). Typing filters. Enter
    /// with nothing selected resolves nothing — not by name, not "the only one", not the best match.</item>
    /// </list>
    /// <para>
    /// It validates the SHAPE of a literal (a finite number) and not its domain range. The range belongs to
    /// whoever assembles the design and is still checked there, so this stays reusable by a property whose
    /// admissible values are not a clearance's.
    /// </para>
    /// <para>
    /// It never consults the register. The options arrive already accredited and type-compatible from
    /// Application, so the session cannot resolve an identity, cannot judge compatibility, and cannot turn a
    /// name into a <see cref="VariableId"/>.
    /// </para>
    /// </summary>
    public sealed class LinkedPropertyEditSession
    {
        private readonly IReadOnlyList<LinkedPropertyOption> _options;
        private LinkedPropertyEditState _committed;
        private string _text;
        private bool _dirty;
        private double _stagedLiteral;
        private bool _staged;

        /// <param name="committed">The state the property starts in.</param>
        /// <param name="options">
        /// The variables this property may be bound to, ALREADY accredited and type-compatible by Application.
        /// The session filters them by text; it never decides what is compatible and never resolves a name.
        /// </param>
        public LinkedPropertyEditSession(
            LinkedPropertyEditState committed, IReadOnlyList<LinkedPropertyOption> options)
        {
            _committed = committed ?? throw new ArgumentNullException(nameof(committed));
            _options = options ?? new LinkedPropertyOption[0];
            _text = DisplayOf(committed);
        }

        /// <summary>The last COMMITTED state. A draft never shows through here.</summary>
        public LinkedPropertyEditState Committed => _committed;

        /// <summary>The text as it stands, committed or not.</summary>
        public string Text => _text;

        public LinkedPropertyDraftKind Draft
        {
            get
            {
                if (!_dirty)
                {
                    return LinkedPropertyDraftKind.Committed;
                }

                if (IsQuery(_text))
                {
                    return LinkedPropertyDraftKind.DraftReferenceQuery;
                }

                return TryParseLiteral(_text, out _)
                    ? LinkedPropertyDraftKind.DraftLiteral
                    : LinkedPropertyDraftKind.InvalidDraft;
            }
        }

        /// <summary>
        /// There is a human edit that has not been resolved. Retyping the SAME visible value counts: the user
        /// expressed an intention, and the inherited pending contract (I-43, O-43-02) respects it.
        /// </summary>
        public bool IsDirty => _dirty;

        /// <summary>
        /// The pending draft would change <see cref="LinkedPropertySource"/>. It decides what
        /// <c>LostFocus</c> and a generic write boundary may do.
        /// </summary>
        public bool DraftChangesSource
        {
            get
            {
                switch (Draft)
                {
                    case LinkedPropertyDraftKind.DraftReferenceQuery:
                        // It would create or replace a reference, whatever the committed source is.
                        return true;

                    case LinkedPropertyDraftKind.DraftLiteral:
                        // Over a literal it is the same source; over a reference it would unlink.
                        return _committed.IsReference;

                    default:
                        return false;
                }
            }
        }

        /// <summary>
        /// The options the current query filters to, in the order Application gave them. Outside a query it is
        /// the whole compatible set, so a surface can open the list without typing first.
        /// </summary>
        public IReadOnlyList<LinkedPropertyOption> Candidates
        {
            get
            {
                if (!IsQuerying)
                {
                    return _options;
                }

                var query = _text.Substring(1).Trim();

                if (query.Length == 0)
                {
                    return _options;
                }

                var matches = new List<LinkedPropertyOption>();

                foreach (var option in _options)
                {
                    // The text FILTERS. It is compared against the label because that is what a person is
                    // typing; the match never becomes a resolution, because committing needs TrySelect.
                    if ((option.Name ?? string.Empty).IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        matches.Add(option);
                    }
                }

                return matches;
            }
        }

        /// <summary>The reference query is open, so a surface should be offering candidates.</summary>
        public bool IsQuerying => Draft == LinkedPropertyDraftKind.DraftReferenceQuery;

        /// <summary>
        /// The value IN FORCE for the committed state: the literal itself, or the referenced variable's value.
        ///
        /// <para>
        /// It FAILS rather than falling back. A reference whose variable is not among the options has no
        /// effective value, and answering the frozen literal instead would let a rack be drawn with a number
        /// that is not the one governing it — the exact silent substitution the whole initiative forbids.
        /// </para>
        /// </summary>
        public bool TryGetEffectiveValue(out double value)
        {
            if (!_committed.IsReference)
            {
                value = _committed.CommittedLiteral;
                return true;
            }

            var id = _committed.Source.VariableId;

            foreach (var option in _options)
            {
                if (option.VariableId.Equals(id))
                {
                    value = option.LiteralValue;
                    return true;
                }
            }

            value = 0.0;
            return false;
        }

        /// <summary>The user typed. NOTHING is committed and no source changes.</summary>
        public void Type(string text)
        {
            _text = text ?? string.Empty;
            _dirty = true;
            _staged = false;
        }

        /// <summary>
        /// Enter. Commits a valid literal draft — including <c>Reference → Literal</c>, the one transition that
        /// needs an explicit gesture. Returns false and keeps the draft pending otherwise.
        /// </summary>
        public bool TryCommitByEnter(out string error)
        {
            error = null;

            switch (Draft)
            {
                case LinkedPropertyDraftKind.Committed:
                    return true;

                case LinkedPropertyDraftKind.DraftLiteral:
                    // Enter is the explicit gesture, so it may change the source: this is the ONLY path from a
                    // reference back to a literal, and it deliberately needs no prior Unlink button.
                    TryParseLiteral(_text, out var value);
                    CommitState(LinkedPropertyEditState.Literal(value));
                    return true;

                case LinkedPropertyDraftKind.DraftReferenceQuery:
                    error = "Elige una variable de la lista: el texto solo filtra, no la resuelve.";
                    return false;

                default:
                    error = "'" + _text + "' no es un numero valido.";
                    return false;
            }
        }

        /// <summary>
        /// The COMPOSITE control lost focus. Commits only when the source does not change; a pending draft over
        /// a reference stays pending instead of silently unlinking.
        /// </summary>
        public bool TryCommitByLostFocus(out string error)
        {
            error = null;

            if (Draft == LinkedPropertyDraftKind.Committed)
            {
                return true;
            }

            if (DraftChangesSource)
            {
                // The general rule: LostFocus may commit a value only if it does NOT change the source.
                error = SourceChangeMessage;
                return false;
            }

            return TryCommitByEnter(out error);
        }

        /// <summary>
        /// Explicit selection of an option, by identity. The ONLY way a reference is ever committed.
        /// Freezes <see cref="LinkedPropertyEditState.CommittedLiteral"/> — the last literal COMMITTED, never
        /// the last text typed, which is what makes the 20.13 rule a single rule.
        /// </summary>
        public bool TrySelect(VariableId variableId, out string error)
        {
            error = null;

            if (!Offers(variableId))
            {
                // Application decided what is compatible; the session does not re-decide it, but it will not
                // accept an identity nobody offered either.
                error = "La variable '" + variableId + "' no esta entre las que esta propiedad admite.";
                return false;
            }

            if (_committed.IsReference && _committed.Source.VariableId.Equals(variableId))
            {
                // Reference(X) -> Reference(X) is an idempotent NO-OP: the frozen literal is not rewritten and
                // the committed state is not replaced.
                Resolve();
                return true;
            }

            CommitState(LinkedPropertyEditState.Reference(_committed.CommittedLiteral, variableId));
            return true;
        }

        /// <summary>Escape: back to the last committed state, exactly. Zero semantic mutation.</summary>
        public void Cancel() => Resolve();

        /// <summary>Stage for a generic write boundary (C4). Validates; mutates nothing.</summary>
        public LinkedPropertyStageResult TryStage()
        {
            _staged = false;

            switch (Draft)
            {
                case LinkedPropertyDraftKind.Committed:
                    return LinkedPropertyStageResult.Clean;

                case LinkedPropertyDraftKind.DraftLiteral when !DraftChangesSource:
                    TryParseLiteral(_text, out _stagedLiteral);
                    _staged = true;
                    return LinkedPropertyStageResult.Ready;

                case LinkedPropertyDraftKind.DraftLiteral:
                case LinkedPropertyDraftKind.DraftReferenceQuery:
                    // A generic boundary must never turn a source change into a commit on its own (V3-R07).
                    return LinkedPropertyStageResult.Blocked(SourceChangeMessage);

                default:
                    return LinkedPropertyStageResult.Blocked("'" + _text + "' no es un numero valido.");
            }
        }

        /// <summary>Applies what <see cref="TryStage"/> accepted. Does nothing when there was nothing staged.</summary>
        public void ApplyStaged()
        {
            if (!_staged)
            {
                return;
            }

            _staged = false;
            CommitState(LinkedPropertyEditState.Literal(_stagedLiteral));
        }

        /// <summary>
        /// The text a committed state is shown as: a number, or <c>=Nombre</c>. A reference to a variable that
        /// is not on offer falls back to its IDENTITY rather than inventing a name or showing a gap — it cannot
        /// happen in production, because opening already requires the rack to resolve.
        /// </summary>
        public string DisplayOf(LinkedPropertyEditState state)
        {
            if (state == null)
            {
                return string.Empty;
            }

            if (!state.IsReference)
            {
                return state.CommittedLiteral.ToString("0.###", CultureInfo.InvariantCulture);
            }

            var id = state.Source.VariableId;

            foreach (var option in _options)
            {
                if (option.VariableId.Equals(id) && !string.IsNullOrWhiteSpace(option.Name))
                {
                    return "=" + option.Name;
                }
            }

            return "=" + id;
        }

        private const string SourceChangeMessage =
            "Confirma el cambio de fuente con Enter o cancelalo con Escape.";

        private bool Offers(VariableId variableId)
        {
            foreach (var option in _options)
            {
                if (option.VariableId.Equals(variableId))
                {
                    return true;
                }
            }

            return false;
        }

        private void CommitState(LinkedPropertyEditState state)
        {
            _committed = state;
            Resolve();
        }

        /// <summary>The edit is RESOLVED: the text goes back to describing the committed state.</summary>
        private void Resolve()
        {
            _dirty = false;
            _staged = false;
            _text = DisplayOf(_committed);
        }

        private static bool IsQuery(string text)
            => text != null && text.TrimStart().StartsWith("=", StringComparison.Ordinal);

        /// <summary>
        /// A literal is a finite number in the invariant culture. It deliberately does NOT judge the range: a
        /// clearance's admissible values are not every property's, and that rule already lives where the design
        /// is assembled.
        /// </summary>
        private static bool TryParseLiteral(string text, out double value)
            => double.TryParse(
                   text, NumberStyles.Float, CultureInfo.InvariantCulture, out value) &&
               !double.IsNaN(value) &&
               !double.IsInfinity(value);
    }
}
