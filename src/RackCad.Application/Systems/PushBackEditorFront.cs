using System.Collections.Generic;

namespace RackCad.Application.Systems
{
    /// <summary>
    /// The Push-Back-specific companion of one editable <see cref="DynamicEditorFront"/>: one <see cref="PushBackEditorCell"/>
    /// per load level, holding the rear beam peralte and the rear-tope flag. It stays parallel to the matrix front by index;
    /// <see cref="DynamicFrontMatrix"/> remains the authority for the front's structure (positions, levels, fondos, ...).
    /// Growing clones the last cell exactly like <see cref="DynamicEditorFront.EnsureCellCount"/>; shrinking removes only the
    /// trailing cells a level reduction left behind, conserving the surviving intersection.
    /// </summary>
    public sealed class PushBackEditorFront
    {
        public List<PushBackEditorCell> Cells { get; } = new List<PushBackEditorCell>();

        /// <summary>Grow the cell list so every load level owns a cell (cloning the last, or a default), like the matrix.</summary>
        public void EnsureCellCount(int levelCount)
        {
            var target = levelCount < 1 ? 1 : levelCount;
            while (Cells.Count < target)
            {
                Cells.Add(Cells.Count > 0 ? Cells[Cells.Count - 1].Clone() : PushBackEditorCell.Default());
            }
        }

        /// <summary>Drop the trailing cells so the count matches <paramref name="levelCount"/> (shrink); never below one.</summary>
        public void TrimToLevelCount(int levelCount)
        {
            var target = levelCount < 1 ? 1 : levelCount;
            if (Cells.Count > target)
            {
                Cells.RemoveRange(target, Cells.Count - target);
            }
        }

        public PushBackEditorFront Clone()
        {
            var clone = new PushBackEditorFront();
            foreach (var cell in Cells)
            {
                clone.Cells.Add(cell.Clone());
            }

            return clone;
        }

        /// <summary>A default front with <paramref name="levelCount"/> cells at the Push Back defaults (peralte 3.5, active tope).</summary>
        public static PushBackEditorFront CreateDefault(int levelCount)
        {
            var front = new PushBackEditorFront();
            front.EnsureCellCount(levelCount);
            return front;
        }
    }
}
