using System;
using System.Collections.Generic;

namespace RackCad.UI.Controls
{
    /// <summary>
    /// The captions a dialog DECLARES for its own axes (I-34). The column axis is a "Frente" in the tope and guía
    /// grids and a "Poste" in the desviador one; deriving that from the system would reintroduce exactly the defect
    /// I-33 §6.5 had to undo (a capability inferred from an unrelated input instead of declared). Nothing here knows
    /// what a rack is.
    /// </summary>
    public sealed class SelectionMatrixScopeLabels
    {
        /// <summary>The usual grid: columns are frentes.</summary>
        public static readonly SelectionMatrixScopeLabels ByFrente = new SelectionMatrixScopeLabels("Frente");

        /// <summary>The desviador grid: columns are POSTS (N fronts ⇒ N+1 posts).</summary>
        public static readonly SelectionMatrixScopeLabels ByPoste = new SelectionMatrixScopeLabels("Poste");

        public SelectionMatrixScopeLabels(string columnAxis, string rowAxis = "Nivel", string allLabel = "Todo")
        {
            ColumnAxis = Clean(columnAxis, "Columna");
            RowAxis = Clean(rowAxis, "Nivel");
            AllLabel = Clean(allLabel, "Todo");
        }

        /// <summary>What one column IS here: "Frente" or "Poste".</summary>
        public string ColumnAxis { get; }

        /// <summary>What one row IS here: "Nivel" in every current grid.</summary>
        public string RowAxis { get; }

        /// <summary>The caption of <see cref="SelectionMatrixScope.All"/>.</summary>
        public string AllLabel { get; }

        /// <summary>The caption of a scope button: "Celda", the row axis, the column axis, or the all label.</summary>
        public string For(SelectionMatrixScope scope)
        {
            switch (scope)
            {
                case SelectionMatrixScope.Cell: return "Celda";
                case SelectionMatrixScope.Row: return RowAxis;
                case SelectionMatrixScope.Column: return ColumnAxis;
                default: return AllLabel;
            }
        }

        private static string Clean(string value, string fallback)
            => string.IsNullOrWhiteSpace(value) ? fallback : value.Trim();
    }

    /// <summary>
    /// The reusable, PURE state behind a bulk safety edit (I-34 / PB-007): a primary cell, the scopes the adopting
    /// dialog declares it offers, their captions, and whether each one can be applied right now. No WPF, no AutoCAD, no
    /// <c>RackSystemKind</c> — it only knows a <see cref="SelectionMatrixModel"/>.
    /// <para>
    /// The PRIMARY CELL is deliberately TRANSIENT: it anchors <see cref="SelectionMatrixScope.Row"/> and
    /// <see cref="SelectionMatrixScope.Column"/> while the dialog is open and is never persisted. What the safety
    /// grids persist is unchanged — the set of OFF cells — and this type sits upstream of that translation, so the
    /// dormant cells <c>SafetyDormantCells</c> preserves are untouched.
    /// </para>
    /// </summary>
    public sealed class SelectionMatrixBulkEditor
    {
        /// <summary>Every scope, in the order the dialogs show them. The default capability set.</summary>
        public static readonly IReadOnlyList<SelectionMatrixScope> AllScopes = new[]
        {
            SelectionMatrixScope.Cell,
            SelectionMatrixScope.Row,
            SelectionMatrixScope.Column,
            SelectionMatrixScope.All
        };

        private readonly List<SelectionMatrixScope> scopes;
        private SelectionMatrixCell primary;
        private bool hasPrimary;

        /// <param name="model">The grid this editor applies to.</param>
        /// <param name="labels">The captions the ADOPTER declares. Null → the neutral "Frente" set.</param>
        /// <param name="scopes">The scopes the adopter offers. Null → <see cref="AllScopes"/>. A scope not declared is
        /// not supported: <see cref="CanApply"/> is false and <see cref="Apply"/> is a no-op.</param>
        public SelectionMatrixBulkEditor(
            SelectionMatrixModel model,
            SelectionMatrixScopeLabels labels = null,
            IEnumerable<SelectionMatrixScope> scopes = null)
        {
            Model = model ?? throw new ArgumentNullException(nameof(model));
            Labels = labels ?? SelectionMatrixScopeLabels.ByFrente;

            this.scopes = new List<SelectionMatrixScope>();
            foreach (var scope in scopes ?? AllScopes)
            {
                if (!this.scopes.Contains(scope))
                {
                    this.scopes.Add(scope);
                }
            }
        }

        public SelectionMatrixModel Model { get; }

        public SelectionMatrixScopeLabels Labels { get; }

        /// <summary>The scopes this dialog declared, deduplicated, in the order given.</summary>
        public IReadOnlyList<SelectionMatrixScope> Scopes => scopes;

        /// <summary>The anchor of the row/column scopes, or null when none is set. NEVER persisted.</summary>
        public SelectionMatrixCell? PrimaryCell => hasPrimary ? primary : (SelectionMatrixCell?)null;

        public bool HasPrimaryCell => hasPrimary;

        public bool Supports(SelectionMatrixScope scope) => scopes.Contains(scope);

        /// <summary>The caption of a scope button.</summary>
        public string LabelFor(SelectionMatrixScope scope) => Labels.For(scope);

        /// <summary>
        /// Sets the primary cell, but ONLY over a cell that exists: an absent cell (a jagged column's empty slot, or a
        /// blank front's whole column) and an out-of-range coordinate are REFUSED and leave the previous primary
        /// untouched — a rejected click must not silently move the anchor.
        /// </summary>
        /// <returns>True when the primary moved to (column, row).</returns>
        public bool TrySetPrimary(int column, int row)
        {
            if (column < 0 || column >= Model.Columns || row < 0 || row >= Model.Rows || Model.IsAbsent(column, row))
            {
                return false;
            }

            primary = new SelectionMatrixCell(column, row);
            hasPrimary = true;
            return true;
        }

        public void ClearPrimary()
        {
            hasPrimary = false;
            primary = default;
        }

        /// <summary>Whether the scope can be applied right now. False ⇒ the control disables it and shows
        /// <see cref="DisabledReason"/>; a scope is never offered as enabled-but-inert.</summary>
        public bool CanApply(SelectionMatrixScope scope) => DisabledReason(scope) == null;

        /// <summary>Why the scope cannot be applied, or NULL when it can. This is the tooltip text.</summary>
        public string DisabledReason(SelectionMatrixScope scope)
        {
            if (!Supports(scope))
            {
                return "Este alcance no está disponible en esta rejilla.";
            }

            if (Model.CellCount == 0)
            {
                return "La rejilla no tiene ninguna celda.";
            }

            if (scope != SelectionMatrixScope.All && !hasPrimary)
            {
                return "Selecciona primero una celda para usar este alcance.";
            }

            return null;
        }

        /// <summary>
        /// Applies <paramref name="activate"/> (true = Activar, false = Desactivar) to every PRESENT cell in the scope.
        /// Absent cells and cells already in that state are left alone and never reported. Raises exactly ONE
        /// aggregated <see cref="SelectionMatrixModel.ScopeApplied"/>, or none at all when nothing changed.
        /// </summary>
        /// <returns>The cells that changed; empty when the scope is unavailable or the edit is a no-op.</returns>
        public IReadOnlyList<SelectionMatrixCell> Apply(SelectionMatrixScope scope, bool activate)
        {
            if (!CanApply(scope))
            {
                return Array.Empty<SelectionMatrixCell>();
            }

            return Model.ApplyScope(scope, primary.Column, primary.Row, activate);
        }
    }
}
