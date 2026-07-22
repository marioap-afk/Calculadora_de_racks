using System.Collections.Generic;
using RackCad.Domain.Systems;

namespace RackCad.Application.Systems
{
    /// <summary>
    /// Editable transverse front of the dynamic editor: its structural fields (positions, levels, fondos, first-beam
    /// height) plus one <see cref="DynamicEditorCell"/> per level. Extracted verbatim from the window's private row so the
    /// grid can grow/clone/apply without WPF (I-21). Resolved fields (Bfr, BeamLength) are cached back from the resolver
    /// for display and mirror the previous inline row.
    /// </summary>
    public sealed class DynamicEditorFront
    {
        public int Index { get; set; }
        public int PalletCount { get; set; }
        public int LoadLevels { get; set; }
        public int PalletsDeep { get; set; }
        public int DepthStartPosition { get; set; } = 1;
        public double FirstLevelHeight { get; set; } = DynamicRackDefaults.DefaultFirstLevelHeight;
        public double Bfr { get; set; }
        public double BeamLength { get; set; }
        public List<DynamicEditorCell> Cells { get; } = new List<DynamicEditorCell>();

        /// <summary>Copy the editable structural front values from an edit buffer (was the window's ApplyFrontValues).</summary>
        public void Apply(DynamicEditorValues values)
        {
            PalletCount = values.PalletCount;
            LoadLevels = values.LoadLevels;
            PalletsDeep = values.PalletsDeep;
            DepthStartPosition = values.DepthStartPosition;
            FirstLevelHeight = values.FirstLevelHeight;
        }

        /// <summary>Grow the cell list so every load level owns a cell, cloning the last (or a default) as the window did.</summary>
        public void EnsureCellCount(int levelCount)
        {
            var target = levelCount < 1 ? 1 : levelCount;
            while (Cells.Count < target)
            {
                Cells.Add(Cells.Count > 0 ? Cells[Cells.Count - 1].Clone() : DynamicEditorCell.Default());
            }
        }

        public DynamicEditorFront Clone()
        {
            var clone = new DynamicEditorFront
            {
                Index = Index,
                PalletCount = PalletCount,
                LoadLevels = LoadLevels,
                PalletsDeep = PalletsDeep,
                DepthStartPosition = DepthStartPosition,
                FirstLevelHeight = FirstLevelHeight,
                Bfr = Bfr,
                BeamLength = BeamLength
            };
            foreach (var cell in Cells)
            {
                clone.Cells.Add(cell.Clone());
            }
            return clone;
        }

        /// <summary>The default single front the editor starts with (matches the window's initial row).</summary>
        public static DynamicEditorFront CreateDefault(int index)
            => new DynamicEditorFront
            {
                Index = index,
                PalletCount = DynamicRackDefaults.DefaultPalletsWide,
                LoadLevels = DynamicRackDefaults.DefaultLoadLevels,
                PalletsDeep = DynamicRackDefaults.DefaultPalletsDeep,
                DepthStartPosition = 1
            };
    }
}
