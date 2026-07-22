namespace RackCad.Application.Systems
{
    /// <summary>
    /// How far the selective cell editor applies its values (the "Aplicar a:" choice): a single cell, a whole level
    /// (row), a whole frente (column) or every cell of the tramo. Extracted from <c>RackSelectiveWindow</c> (initiative
    /// I-20) so the scope logic is a pure, testable operation on <see cref="SelectiveEditorState"/>.
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
        All
    }
}
