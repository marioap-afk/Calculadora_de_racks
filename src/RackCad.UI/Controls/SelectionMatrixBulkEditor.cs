using System;
using System.Collections.Generic;

namespace RackCad.UI.Controls
{
    /// <summary>DECLARED SURFACE ONLY (I-34, red step): the captions a dialog declares for its axes. Inert on purpose
    /// so the PB-007 regressions can be verified failing before the foundation exists.</summary>
    public sealed class SelectionMatrixScopeLabels
    {
        public static readonly SelectionMatrixScopeLabels ByFrente = new SelectionMatrixScopeLabels("Frente");

        public static readonly SelectionMatrixScopeLabels ByPoste = new SelectionMatrixScopeLabels("Poste");

        public SelectionMatrixScopeLabels(string columnAxis, string rowAxis = "Nivel", string allLabel = "Todo")
        {
        }

        public string ColumnAxis => null;

        public string RowAxis => null;

        public string AllLabel => null;

        public string For(SelectionMatrixScope scope) => null;
    }

    /// <summary>DECLARED SURFACE ONLY (I-34, red step): the bulk-edit state over a <see cref="SelectionMatrixModel"/>.
    /// Every member is inert so the regressions fail; the real behaviour lands in the foundation commit.</summary>
    public sealed class SelectionMatrixBulkEditor
    {
        public static readonly IReadOnlyList<SelectionMatrixScope> AllScopes = new[]
        {
            SelectionMatrixScope.Cell,
            SelectionMatrixScope.Row,
            SelectionMatrixScope.Column,
            SelectionMatrixScope.All
        };

        public SelectionMatrixBulkEditor(
            SelectionMatrixModel model,
            SelectionMatrixScopeLabels labels = null,
            IEnumerable<SelectionMatrixScope> scopes = null)
        {
            Model = model ?? throw new ArgumentNullException(nameof(model));
            Labels = labels ?? SelectionMatrixScopeLabels.ByFrente;
        }

        public SelectionMatrixModel Model { get; }

        public SelectionMatrixScopeLabels Labels { get; }

        public IReadOnlyList<SelectionMatrixScope> Scopes => Array.Empty<SelectionMatrixScope>();

        public SelectionMatrixCell? PrimaryCell => null;

        public bool HasPrimaryCell => false;

        public bool Supports(SelectionMatrixScope scope) => false;

        public string LabelFor(SelectionMatrixScope scope) => null;

        public bool TrySetPrimary(int column, int row) => false;

        public void ClearPrimary()
        {
        }

        public bool CanApply(SelectionMatrixScope scope) => false;

        public string DisabledReason(SelectionMatrixScope scope) => null;

        public IReadOnlyList<SelectionMatrixCell> Apply(SelectionMatrixScope scope, bool activate)
            => Array.Empty<SelectionMatrixCell>();
    }
}
