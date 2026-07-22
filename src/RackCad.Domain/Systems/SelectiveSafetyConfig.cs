using System.Collections.Generic;
using System.Linq;

namespace RackCad.Domain.Systems
{
    // Per-family safety configuration subtypes (I-22, hallazgo E7). The God-data-class
    // SelectiveSafetySelection used to carry every family's fields inline with a DeepCopy that grew
    // field-by-field with each family. Each family now owns a sealed config with its OWN DeepCopy; the
    // selection composes them and delegates. Persistence maps each config with its own legacy fallback
    // (RackCad.Application.Persistence), and the selection keeps thin flat accessors so existing
    // consumers and their tests are unchanged. A new family = a new config + its mapping, not another
    // field in the God-class.

    /// <summary>TOPE (larguero tope) configuration: one shared central tope vs one per fondo, which fondo carries it,
    /// the block SAQUE, whether the frontal draws it, and the (frente, level) cells to skip.</summary>
    public sealed class SelectiveTopeConfig
    {
        /// <summary>One shared central tope (true) vs one per fondo of the central pair (false, guided by the selection Side).</summary>
        public bool Shared { get; set; } = true;

        /// <summary>Which fondo (0-based) carries the tope; &lt; 0 = the automatic central fondo.</summary>
        public int Fondo { get; set; } = -1;

        /// <summary>The block's SAQUE (stick-out) parameter, inches (&lt;= 0 → the domain default at resolve time).</summary>
        public double Saque { get; set; } = SelectiveSafetyDefaults.TopeSaque;

        /// <summary>Also draw the tope in the FRONTAL view (lateral + planta always draw it; the frontal is a toggle).</summary>
        public bool Frontal { get; set; }

        /// <summary>The (frente, level) cells with NO tope (default empty = a tope at every larguero).</summary>
        public IList<SelectiveGridCell> OffCells { get; } = new List<SelectiveGridCell>();

        /// <summary>True if a tope is drawn at (frente, level) — i.e. that cell is not in <see cref="OffCells"/>.</summary>
        public bool At(int frente, int level) => !SelectiveSafetyCells.Contains(OffCells, frente, level);

        public SelectiveTopeConfig DeepCopy()
        {
            var copy = new SelectiveTopeConfig { Shared = Shared, Fondo = Fondo, Saque = Saque, Frontal = Frontal };
            SelectiveSafetyCells.Copy(OffCells, copy.OffCells);
            return copy;
        }
    }

    /// <summary>DESVIADOR configuration: its two global dimensions and the disabled (post, load-level) cells. In an off
    /// cell, <see cref="SelectiveGridCell.Frente"/> stores the resolved post-column index (reusing the generic cell).</summary>
    public sealed class SelectiveDesviadorConfig
    {
        /// <summary>Dynamic LONGITUD (in). Invalid/legacy values fall back to the domain default in Application.</summary>
        public double Longitud { get; set; } = SelectiveSafetyDefaults.DesviadorLongitud;

        /// <summary>First load-level height above the first TROQUEL_LARGUERO (in). The first piece exists even when the
        /// floor pallet has no beam; invalid/legacy values fall back to the domain default in Application.</summary>
        public double PrimerNivelAltura { get; set; } = SelectiveSafetyDefaults.DesviadorPrimerNivelAltura;

        /// <summary>Disabled (post, load-level) cells; <see cref="SelectiveGridCell.Frente"/> holds the post-column index.</summary>
        public IList<SelectiveGridCell> OffCells { get; } = new List<SelectiveGridCell>();

        public bool At(int post, int level) => !SelectiveSafetyCells.Contains(OffCells, post, level);

        public SelectiveDesviadorConfig DeepCopy()
        {
            var copy = new SelectiveDesviadorConfig { Longitud = Longitud, PrimerNivelAltura = PrimerNivelAltura };
            SelectiveSafetyCells.Copy(OffCells, copy.OffCells);
            return copy;
        }
    }

    /// <summary>DEFENSA configuration: explicit per-post forklift-defense lengths. A zero length disables that post;
    /// missing posts use the dynamic-system defaults resolved by Application.</summary>
    public sealed class SelectiveDefensaConfig
    {
        public IList<SafetyPostDefense> Posts { get; } = new List<SafetyPostDefense>();

        public SelectiveDefensaConfig DeepCopy()
        {
            var copy = new SelectiveDefensaConfig();
            foreach (var post in Posts)
            {
                if (post != null)
                {
                    copy.Posts.Add(new SafetyPostDefense
                    {
                        PostIndex = post.PostIndex,
                        ExitLength = post.ExitLength,
                        EntranceLength = post.EntranceLength
                    });
                }
            }

            return copy;
        }
    }

    /// <summary>GUIA (entrance guide) configuration: disabled (frente, level) cells; empty means every available level.</summary>
    public sealed class SelectiveGuiaConfig
    {
        /// <summary>Zero-based frente/level cells without entrance guides. Missing/empty enables every cell.</summary>
        public IList<SelectiveGridCell> OffCells { get; } = new List<SelectiveGridCell>();

        public bool At(int frontIndex, int levelIndex)
            => frontIndex >= 0 && levelIndex >= 0 && !SelectiveSafetyCells.Contains(OffCells, frontIndex, levelIndex);

        public SelectiveGuiaConfig DeepCopy()
        {
            var copy = new SelectiveGuiaConfig();
            SelectiveSafetyCells.Copy(OffCells, copy.OffCells);
            return copy;
        }
    }

    /// <summary>PARRILLA (deck) configuration: which views draw it, the manual width/count, and the (frente, level)
    /// cells that carry one.</summary>
    public sealed class SelectiveParrillaConfig
    {
        /// <summary>Draw the deck in the FRONTAL view (seen edge-on, FRENTE = the frente width). Default true.</summary>
        public bool Frontal { get; set; } = true;

        /// <summary>Draw the deck in the LATERAL view (seen edge-on, FONDO = the depth). Default true.</summary>
        public bool Lateral { get; set; } = true;

        /// <summary>Manual deck width (FRENTE, inches); &lt;= 0 = one deck per tarima at the tarima's own frente.</summary>
        public double Frente { get; set; }

        /// <summary>Manual deck count PER LOAD ROW; &lt;= 0 = derived from the width (how many fit). Clamped to what fits.</summary>
        public int Cantidad { get; set; }

        /// <summary>The (frente, level) cells with NO deck (default empty = a deck at every load position).</summary>
        public IList<SelectiveGridCell> OffCells { get; } = new List<SelectiveGridCell>();

        /// <summary>True if a deck sits at (frente, level) — i.e. that cell is not in <see cref="OffCells"/>.</summary>
        public bool At(int frente, int level) => !SelectiveSafetyCells.Contains(OffCells, frente, level);

        public SelectiveParrillaConfig DeepCopy()
        {
            var copy = new SelectiveParrillaConfig { Frontal = Frontal, Lateral = Lateral, Frente = Frente, Cantidad = Cantidad };
            SelectiveSafetyCells.Copy(OffCells, copy.OffCells);
            return copy;
        }
    }

    /// <summary>Shared (frente, level) cell helpers for the safety configs — one deep-copy and one membership test so
    /// each config's DeepCopy/At reads identically and stays byte-for-byte equal to the pre-decomposition behavior.</summary>
    internal static class SelectiveSafetyCells
    {
        public static void Copy(IEnumerable<SelectiveGridCell> source, ICollection<SelectiveGridCell> target)
        {
            foreach (var cell in source)
            {
                if (cell != null)
                {
                    target.Add(new SelectiveGridCell { Frente = cell.Frente, Level = cell.Level });
                }
            }
        }

        public static bool Contains(IEnumerable<SelectiveGridCell> cells, int frente, int level)
            => cells.Any(cell => cell != null && cell.Frente == frente && cell.Level == level);
    }
}
