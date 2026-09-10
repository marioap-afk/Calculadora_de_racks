using RackCad.Application.Persistence;
using RackCad.Application.ProjectVariables;
using RackCad.Domain.Systems.Selective;

namespace RackCad.Application.Systems.Selective
{
    /// <summary>Whether the rack editor may open at all.</summary>
    public enum SelectiveEditorOpenOutcome
    {
        Open = 1,

        /// <summary>It may not, and the reason is visible. There is no design, and there is no fallback.</summary>
        Blocked = 2,
    }

    /// <summary>
    /// What the editor needs to know about the vertical clearance, and NOTHING else.
    ///
    /// <para>
    /// It carries no variable id, no reference, no register. The editor's whole question is "is this value
    /// mine to change, and what is it right now" — giving it the id as well would invite it to resolve, and
    /// resolution has exactly one home (<see cref="SelectiveEffectiveDesignResolver"/>).
    /// </para>
    /// </summary>
    public sealed class VerticalClearanceBindingState
    {
        private VerticalClearanceBindingState(bool isBound, double effectiveValue, string boundVariableName)
        {
            IsBound = isBound;
            EffectiveValue = effectiveValue;
            BoundVariableName = boundVariableName;
        }

        /// <summary>True when a project variable governs the property, so the editor is not its author.</summary>
        public bool IsBound { get; }

        /// <summary>The value in force — the variable's when bound, the authored literal when not.</summary>
        public double EffectiveValue { get; }

        /// <summary>
        /// The governing variable's name, for DISPLAY only (I-47 G17). Null when unbound. The editor never
        /// addresses anything by it — a name is text the user edits, and two variables may share one.
        /// </summary>
        public string BoundVariableName { get; }

        public static VerticalClearanceBindingState Of(
            bool isBound, double effectiveValue, string boundVariableName = null)
            => new VerticalClearanceBindingState(isBound, effectiveValue, boundVariableName);
    }

    /// <summary>The answer: open with this state, or do not open and say why.</summary>
    public sealed class SelectiveEditorOpenResult
    {
        private SelectiveEditorOpenResult(
            SelectiveEditorOpenOutcome outcome,
            SelectivePalletDesign design,
            VerticalClearanceBindingState verticalClearance,
            string error)
        {
            Outcome = outcome;
            Design = design;
            VerticalClearance = verticalClearance;
            Error = error;
        }

        public SelectiveEditorOpenOutcome Outcome { get; }

        /// <summary>The EFFECTIVE design the editor works on. Null when blocked — there is no partial one.</summary>
        public SelectivePalletDesign Design { get; }

        /// <summary>Null when blocked, for the same reason.</summary>
        public VerticalClearanceBindingState VerticalClearance { get; }

        /// <summary>The visible reason. Null when open.</summary>
        public string Error { get; }

        public bool IsOpen => Outcome == SelectiveEditorOpenOutcome.Open;

        public static SelectiveEditorOpenResult Open(
            SelectivePalletDesign design, VerticalClearanceBindingState verticalClearance)
            => new SelectiveEditorOpenResult(SelectiveEditorOpenOutcome.Open, design, verticalClearance, null);

        public static SelectiveEditorOpenResult Blocked(string error)
            => new SelectiveEditorOpenResult(SelectiveEditorOpenOutcome.Blocked, null, null, error);
    }

    /// <summary>
    /// What <c>RACKEDITAR</c> has to decide before a selective rack's editor exists (I-47 G12).
    ///
    /// <para>
    /// The editor works on the EFFECTIVE design and the document that gets persisted is still the AUTHORED
    /// one. Both halves are the same sentence: if the editor wrote the effective value into the literal, the
    /// frozen snapshot — the only thing a later repair or unlink has to go back to — would disappear on the
    /// first save, with nothing failing. So this hands the editor a design it may edit freely, plus the fact
    /// that one of its fields is not its own, and the save keeps going through the G10 carrier.
    /// </para>
    /// <para>
    /// The asymmetry that cannot be symmetrised: <b>opening may refuse</b>. A broken reference, a kind from
    /// the future, an unreadable id, or a register that is PRESENT and unreadable are not resolved by falling
    /// back to the frozen literal — falling back would change the geometry in silence, which is the failure
    /// this contract exists to prevent. And an unreadable register blocks even a rack that is not bound:
    /// reading it as empty is how every variable in the drawing would be destroyed on the next write.
    /// </para>
    /// <para>
    /// Repair is not here. It is an explicit, warned operation on the register, and an editor that silently
    /// fixed what it opened would make the warning unnecessary — and the damage invisible.
    /// </para>
    /// </summary>
    public static class SelectiveEditorOpen
    {
        private static readonly SelectiveEffectiveDesignResolver Resolver = new SelectiveEffectiveDesignResolver();

        /// <summary>
        /// The governing variable's name, for the editor to SHOW. Looking a name up is not resolving: the
        /// value already came from the one resolver, and nothing here decides anything by this string.
        /// </summary>
        private static string BoundName(
            SelectivePalletDesignDocument authored, ProjectVariablesDocument registry)
        {
            if (registry == null ||
                !authored.TryGetBinding(ProjectPropertyIds.SelectiveVerticalClearance, out var variableId))
            {
                return null;
            }

            foreach (var variable in registry.ToProjectVariables())
            {
                if (variable.Id.Equals(variableId))
                {
                    return variable.Name;
                }
            }

            return null;
        }

        public static SelectiveEditorOpenResult Resolve(
            SelectivePalletDesignDocument authored, ProjectVariablesReadResult registry)
        {
            if (authored == null)
            {
                return SelectiveEditorOpenResult.Blocked("No se pudieron leer los datos del rack.");
            }

            if (registry == null)
            {
                return SelectiveEditorOpenResult.Blocked(
                    "No se consultó el registro de variables de proyecto de este dibujo, así que no se puede " +
                    "saber qué valor gobierna al rack. El editor no abre.");
            }

            switch (registry.Outcome)
            {
                case ProjectVariablesReadOutcome.Absent:
                case ProjectVariablesReadOutcome.Readable:
                    break;

                default:
                    // PresentButUnreadable / IncompatibleMajor. Blocking here is what keeps "unreadable" from
                    // becoming "empty" — including for a rack that is not bound at all.
                    return SelectiveEditorOpenResult.Blocked(registry.Error);
            }

            var resolution = Resolver.Resolve(authored, registry.Document);

            if (!resolution.IsSuccess)
            {
                return SelectiveEditorOpenResult.Blocked(resolution.Error);
            }

            // PRESENCE, not interpretability: by this point an entry that could not be interpreted has already
            // blocked, so the two questions agree — and asking the presence one keeps the doctrine intact.
            var bound = authored.HasBindingEntry(ProjectPropertyIds.SelectiveVerticalClearance);

            return SelectiveEditorOpenResult.Open(
                resolution.Design,
                VerticalClearanceBindingState.Of(
                    bound, resolution.Design.VerticalClearance, bound ? BoundName(authored, registry.Document) : null));
        }
    }
}
