using System;
using System.Collections.Generic;
using System.Linq;

namespace RackCad.UI.VentanaRackCad
{
    /// <summary>
    /// T5 cm, point 1 of the review: what the prune of a closed drawing lost is said ONCE. Before, each system said it with its own
    /// <c>Say</c>, and the prune that ran after it overwrote it: a drawing closed with a cama and a Dinámico with changes named only the
    /// Dinámico. Now every prune of a system (Cabecera, Cama, Variables, Dinámico and Selectivo today) gives the names of what it forgets
    /// with changes to <see cref="LostOnPrune"/>, and <see cref="PruneDeadSessionsAndSayLost"/> (what <c>Present</c> calls) says them all
    /// in one line when <c>PruneDeadSessions</c> ends, whatever way it ends.
    ///
    /// <para><b>The entry point for the systems that come</b> (the other parts of the fifth batch): add the prune of the system to
    /// <c>PruneDeadSessions</c> as the others (one line), and in it call <see cref="LostOnPrune"/> with the name of each draft with work
    /// that it forgets, the same name the veto of the close gives it (D-08). Never call <c>Say</c> from a prune.</para>
    /// </summary>
    public sealed partial class RackCadWindowViewModel
    {
        private readonly List<string> _lostOnPrune = new List<string>();

        /// <summary>A prune of a system forgot <paramref name="name"/> with changes not written: said once, with the others, at the end.</summary>
        private void LostOnPrune(string name)
        {
            if (!string.IsNullOrWhiteSpace(name)) _lostOnPrune.Add(name.Trim());
        }

        /// <summary>The prunes of every system, then ONE line with everything they lost (sorted as the veto of the close names them).</summary>
        private void PruneDeadSessionsAndSayLost()
        {
            _lostOnPrune.Clear();
            try
            {
                PruneDeadSessions();
            }
            finally
            {
                var lost = _lostOnPrune.OrderBy(n => n, StringComparer.CurrentCultureIgnoreCase).ToList();
                _lostOnPrune.Clear();
                if (lost.Count > 0)
                    Say("Se cerró el dibujo de " + string.Join(", ", lost) + " con cambios sin escribir en la ventana: esos cambios se perdieron.", failed: true);
            }
        }
    }
}
