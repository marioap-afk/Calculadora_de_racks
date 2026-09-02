namespace RackCad.Application.Systems.Selective
{
    /// <summary>
    /// How far the selective cell editor applies its values (the "Aplicar a:" choice): a single cell, a whole level
    /// (row), a whole frente (column) or every cell of the tramo. Extracted from <c>RackSelectiveWindow</c> (initiative
    /// I-20) so the scope logic is a pure, testable operation on <see cref="SelectiveEditorState"/>.
    /// <para>
    /// I-43 keeps the four original members —and their meaning— untouched, and adds <see cref="Selected"/> so the five
    /// scopes match one-to-one the grammar Dinamico/Push Back already offer (<c>DynamicRackCellScope</c>:
    /// Cell/Selected/Level/Front/All). The names differ because the Selectivo named its axes first and the editor's
    /// buttons say "Nivel"/"Frente": <see cref="Row"/> IS the Level scope and <see cref="Column"/> IS the Front scope.
    /// A second enum with the other spelling would be a second vocabulary for one grammar, so there is none.
    /// </para>
    /// <para>
    /// This enum addresses the INNER (frente x nivel) axes only. The fondo axis is never one of these members: it is
    /// the independent set of target fondos the caller hands to <see cref="SelectiveTargetResolver"/>, which is what
    /// makes the contract a PRODUCT (<c>fondos objetivo x alcance interno</c>) instead of a fourth scope value.
    /// </para>
    /// </summary>
    public enum SelectiveApplyScope
    {
        /// <summary>Only the selected cell (selected frente × selected level).</summary>
        Cell,

        /// <summary>Every frente at the selected level (the selected row).</summary>
        Row,

        /// <summary>Every level of the selected frente (the selected column).</summary>
        Column,

        /// <summary>Every cell of the tramo (all frentes × all levels).</summary>
        All,

        /// <summary>
        /// Exactly the positions the caller marks on the VISIBLE matrix — the scope that needs no anchor because
        /// every coordinate is explicit. A selection is a set of <see cref="SelectiveMatrixPosition"/>
        /// (frente x nivel) and carries NO fondo of its own: like every other scope, it is projected onto each target
        /// fondo, so the fondo axis keeps meaning the same thing for all five. I-43 only founds the CONTRACT:
        /// <see cref="SelectiveTargetResolver"/> resolves it, and <see cref="SelectiveEditorState.ApplyScope"/> (the
        /// single-fondo legacy path, which has no multi-selection) REFUSES it out loud instead of quietly applying to
        /// nothing. No editor produces it yet.
        /// </summary>
        Selected
    }
}
