using System;
using System.Collections.Generic;
using RackCad.Application.RackFrames;
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
        /// La custom de <c>(fondoIndex, postIndex)</c> que SIRVE como marco, o null. Es una LECTURA PURA: comprueba
        /// que existe y que tiene altura, y devuelve la receta ALMACENADA tal cual — sin imponer el <c>Depth</c> del
        /// fondo, sin refrescar el modelo fisico y sin copiarla.
        /// <para>
        /// Preguntar no puede mutar. Quien necesite la configuracion EFECTIVA —con el <c>Depth</c> del fondo ya
        /// impuesto— usa <see cref="EffectiveCustomAt"/>; quien solo necesita decidir si hay una custom, o mostrarla
        /// como receta, usa esta (I-43, gate 8.6F).
        /// </para>
        /// <para>
        /// Una receta con <c>Height &lt;= 0</c> no es un marco utilizable y se rechaza, como ya hacia cada consumidor
        /// por su cuenta.
        /// </para>
        /// </summary>
        public static RackFrameConfiguration UsableCustomAt(SelectiveRackSystem system, int fondoIndex, int postIndex)
        {
            var raw = CustomAt(system, fondoIndex, postIndex);
            return raw != null && raw.Height > 0.0 ? raw : null;
        }

        /// <summary>
        /// The custom that is usable as a FULL override of that post's cabecera, or null — with ONE exception, the
        /// depth.
        /// <para>
        /// A custom cabecera overrides the height and the whole frame recipe, but it does NOT own its depth: the
        /// Selectivo already has an explicit per-fondo authority for that (<c>CabeceraFondoOverride</c>, else
        /// <c>tarima − 6"</c>, in <see cref="SelectiveDepthLayout.CabeceraDepthOfFondo"/>). Letting a stored
        /// configuration carry its own depth would make it a SECOND authority, and the two would disagree the moment
        /// the same configuration is applied to fondos of different depth — which is exactly what the multi-fondo
        /// apply of I-43 does. So the fondo's depth is imposed here, at the single point every consumer reads through,
        /// instead of being repaired later on load: the drawing is then the same before and after a save/load, and
        /// changing a fondo's <c>CabeceraFondoOverride</c> moves that fondo's cabeceras and no others.
        /// </para>
        /// <para>
        /// The configuration is normalized in place rather than copied, so identity is preserved for callers that
        /// compare it, and the write is idempotent. This is sound because a configuration belongs to exactly ONE
        /// <c>(fondo, post)</c> — the multi-fondo apply hands every target its own deep copy for this very reason.
        /// A stored configuration with <c>Height &lt;= 0</c> is not a usable frame and is refused, as every consumer
        /// already did on its own.
        /// </para>
        /// </summary>
        public static RackFrameConfiguration EffectiveCustomAt(SelectiveRackSystem system, int fondoIndex, int postIndex)
        {
            var custom = UsableCustomAt(system, fondoIndex, postIndex);
            if (custom == null) return null;

            // SIGUE imponiendo el Depth sobre la receta almacenada, a proposito: purificarlo es ARQ-43-10, un
            // follow-up DIFERIDO fuera de I-43. Lo unico que cambia aqui es que la comprobacion de usabilidad se
            // delega en la consulta pura, para que no haya dos redacciones de la misma regla.
            ImposeFondoDepth(custom, SelectiveDepthLayout.CabeceraDepthOfFondo(system, fondoIndex));
            return custom;
        }

        /// <summary>
        /// Make a custom cabecera carry the depth its FONDO dictates. This is the one function that governs the rule,
        /// used both when a configuration is written to a fondo and whenever one is read back, so there is no path on
        /// which the two could disagree.
        /// <para>
        /// Changing the depth also refreshes the DERIVED physical model. The members (travesaños, diagonales) are
        /// computed from the frame and its depth, so a configuration whose depth moved while its members did not is
        /// internally inconsistent — and it would be counted one way in the live session and another after a reload
        /// regenerated them. Refreshing here is what makes the BOM identical before and after a save/load.
        /// </para>
        /// </summary>
        public static void ImposeFondoDepth(RackFrameConfiguration custom, double depth)
        {
            if (custom == null || depth <= 0.0 || Math.Abs(custom.Depth - depth) <= DepthTolerance) return;

            custom.Depth = depth;
            new BracingPanelMemberBuilder().RefreshPhysicalModel(custom);
        }

        /// <summary>Inches below which two depths are the same value (they come from the same arithmetic).</summary>
        private const double DepthTolerance = 1e-6;

        /// <summary>
        /// Si ese poste de ese fondo dibuja una cabecera personalizada en vez de una derivada. PURA: pasa por
        /// <see cref="UsableCustomAt"/>, no por <see cref="EffectiveCustomAt"/>, porque preguntar si algo existe no
        /// puede imponer el <c>Depth</c> del fondo sobre la receta almacenada (I-43, gate 8.6F).
        /// </summary>
        public static bool HasCustomAt(SelectiveRackSystem system, int fondoIndex, int postIndex)
            => UsableCustomAt(system, fondoIndex, postIndex) != null;

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
