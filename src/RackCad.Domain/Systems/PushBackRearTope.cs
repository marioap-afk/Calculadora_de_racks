using System.Collections.Generic;

namespace RackCad.Domain.Systems
{
    /// <summary>
    /// Rear pallet-stop ("larguero tope") configuration for a Push Back system: one tope per front x load level at the
    /// HIGH (rear) end, ACTIVE BY DEFAULT. Only DEACTIVATIONS are stored (<see cref="OffCells"/>) so a complete positive
    /// list is never persisted — every (front, level) not listed is active. This reuses the canonical selective TOPE
    /// rule (the <c>OffCells</c> idiom and the SAQUE parameter); see <see cref="SelectiveTopeConfig"/>.
    /// </summary>
    public sealed class PushBackRearTopeConfig
    {
        /// <summary>The block SAQUE (stick-out) parameter, inches (&lt;= 0 -&gt; the domain default at resolve time).</summary>
        public double Saque { get; set; } = PushBackDefaults.RearTopeSaque;

        /// <summary>The (front, level) cells with NO rear tope (default empty = a tope at every front x level).</summary>
        public IList<SelectiveGridCell> OffCells { get; } = new List<SelectiveGridCell>();

        /// <summary>True if a rear tope is drawn at (<paramref name="front"/>, <paramref name="level"/>) — i.e. that cell
        /// is not deactivated. The default (empty <see cref="OffCells"/>) materializes every cell as active.</summary>
        public bool At(int front, int level) => !SelectiveSafetyCells.Contains(OffCells, front, level);

        /// <summary>Deactivate the rear tope at (<paramref name="front"/>, <paramref name="level"/>); no-op if already off.</summary>
        public void Disable(int front, int level)
        {
            if (At(front, level))
            {
                OffCells.Add(new SelectiveGridCell { Frente = front, Level = level });
            }
        }

        public PushBackRearTopeConfig DeepCopy()
        {
            var copy = new PushBackRearTopeConfig { Saque = Saque };
            SelectiveSafetyCells.Copy(OffCells, copy.OffCells);
            return copy;
        }
    }
}
