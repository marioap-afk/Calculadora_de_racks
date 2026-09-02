using System.Collections.Generic;
using RackCad.Domain.RackFrames;
using RackCad.Domain.Systems.Selective;

namespace RackCad.Application.Systems.Selective
{
    /// <summary>
    /// The ONE place that answers "which custom cabecera is in force at (fondo, poste)" (I-43).
    /// <para>
    /// Before this, lateral, planta, BOM and the per-fondo view each re-derived the same rule inline — always as
    /// <c>k == 0 &amp;&amp; i &lt; PostCabeceras.Count</c>, which is why a custom cabecera could only ever exist on
    /// fondo 0. Four copies of a fallback are four chances for them to disagree, so the rule now lives here and the
    /// builders ask instead of interpreting.
    /// </para>
    /// <para>
    /// The storage is deliberately split: <see cref="SelectiveRackSystem.PostCabeceras"/> IS fondo 0's row, and
    /// <see cref="SelectiveRackSystem.ExtraFondoPostCabeceras"/> holds fondos 1..N. That keeps the legacy shape with its
    /// legacy meaning — a document written before I-43 carries only the fondo 0 row, so its other fondos stay standard
    /// and its drawing does not change — while making the new axis purely additive.
    /// </para>
    /// </summary>
    public static class SelectiveCabeceraAuthority
    {
        /// <summary>
        /// The custom cabecera stored at <c>(fondoIndex, postIndex)</c>, or null when there is none. A missing row, a
        /// short row and a null entry are all "none": the caller derives the standard cabecera for that fondo and post.
        /// </summary>
        public static RackFrameConfiguration CustomAt(SelectiveRackSystem system, int fondoIndex, int postIndex)
            => system == null ? null : At(system.PostCabeceras, system.ExtraFondoPostCabeceras, fondoIndex, postIndex);

        /// <summary>The same lookup on a DESIGN, for the resolver and the persistence boundary.</summary>
        public static RackFrameConfiguration CustomAt(SelectivePalletDesign design, int fondoIndex, int postIndex)
            => design == null ? null : At(design.PostCabeceras, design.ExtraFondoPostCabeceras, fondoIndex, postIndex);

        /// <summary>
        /// The custom that is usable as a FULL override of that post's cabecera, or null.
        /// <para>
        /// A custom cabecera overrides everything for its post — height, depth and the whole frame — but only when it
        /// actually carries a height. A stored configuration with <c>Height &lt;= 0</c> is not a usable frame, and
        /// every consumer already refused it; keeping that check here means none of them has to remember it.
        /// </para>
        /// </summary>
        public static RackFrameConfiguration EffectiveCustomAt(SelectiveRackSystem system, int fondoIndex, int postIndex)
        {
            var custom = CustomAt(system, fondoIndex, postIndex);
            return custom != null && custom.Height > 0.0 ? custom : null;
        }

        /// <summary>Whether that post of that fondo draws a customized cabecera rather than a derived one.</summary>
        public static bool HasCustomAt(SelectiveRackSystem system, int fondoIndex, int postIndex)
            => EffectiveCustomAt(system, fondoIndex, postIndex) != null;

        private static RackFrameConfiguration At(
            IList<RackFrameConfiguration> fondoZero,
            IList<IList<RackFrameConfiguration>> extras,
            int fondoIndex,
            int postIndex)
        {
            if (postIndex < 0) return null;
            if (fondoIndex == 0)
            {
                return fondoZero != null && postIndex < fondoZero.Count ? fondoZero[postIndex] : null;
            }

            if (fondoIndex < 0 || extras == null || fondoIndex - 1 >= extras.Count) return null;
            var row = extras[fondoIndex - 1];
            return row != null && postIndex < row.Count ? row[postIndex] : null;
        }
    }
}
