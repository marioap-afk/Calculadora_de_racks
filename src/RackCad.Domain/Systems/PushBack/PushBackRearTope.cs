using System;
using System.Collections.Generic;
using RackCad.Domain.Systems.Selective;

namespace RackCad.Domain.Systems.PushBack
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

        /// <summary>
        /// PB-005 (I-32) — the catalog TOPE variant to place. BLANK means "the system's own default", which is what
        /// every document written before this field existed carries, so a legacy rack keeps drawing the same piece.
        /// An id that is not a TOPE in the CURRENT catalog also falls back, so a stale or renamed row can never leave
        /// the rack with no stop at all. The single resolution rule lives in the Application builder.
        /// </summary>
        public string PieceId { get; set; }

        /// <summary>
        /// I-42 (ronda 7C, defecto del dueño) — «NINGUNO»: el valor EXPLICITO de <see cref="PieceId"/> que dice que
        /// este objetivo NO lleva tope posterior. Hasta ahora la unica forma de quitarlo era apagar celda por celda,
        /// y no habia manera de decirlo de una vez ni de leerlo despues: la ausencia se confundia con «todavia no lo
        /// he tocado». Es un valor persistido como cualquier otro id, asi que sobrevive a guardar y a RACKEDITAR.
        ///
        /// Los parentesis lo mantienen fuera del espacio de ids del catalogo, que no los usa.
        /// </summary>
        public const string NonePieceId = "(ninguno)";

        /// <summary>True cuando el usuario eligio «Ninguno» para este objetivo.</summary>
        public bool IsNone => !string.IsNullOrWhiteSpace(PieceId)
                              && string.Equals(PieceId.Trim(), NonePieceId, StringComparison.OrdinalIgnoreCase);

        /// <summary>The (front, level) cells with NO rear tope (default empty = a tope at every front x level).</summary>
        public IList<SelectiveGridCell> OffCells { get; } = new List<SelectiveGridCell>();

        /// <summary>True if a rear tope is drawn at (<paramref name="front"/>, <paramref name="level"/>) — i.e. that cell
        /// is not deactivated. The default (empty <see cref="OffCells"/>) materializes every cell as active.</summary>
        public bool At(int front, int level) => !SelectiveSafetyCells.Contains(OffCells, front, level);

        /// <summary>
        /// LA pregunta fisica: ¿se materializa un tope en esta celda? Es <see cref="At"/> mas la decision de
        /// objetivo <see cref="IsNone"/>, y es la que consumen el dibujo y el BOM —los dos, para que no puedan
        /// discrepar—. El editor sigue leyendo <see cref="At"/>: la mascara por celda es del usuario y «Ninguno» no
        /// la borra, asi que volver a elegir una pieza devuelve exactamente las celdas que habia.
        /// </summary>
        public bool Draws(int front, int level) => !IsNone && At(front, level);

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
            var copy = new PushBackRearTopeConfig { Saque = Saque, PieceId = PieceId };
            SelectiveSafetyCells.Copy(OffCells, copy.OffCells);
            return copy;
        }
    }
}
