using System.Collections.Generic;

namespace RackCad.Domain.RackFrames
{
    public sealed class RackFrameConfiguration
    {
        public RackFrameConfiguration()
        {
            Horizontals = new List<FrameHorizontal>();
            BracingPanels = new List<BracingPanel>();
            Members = new List<FrameMember>();
            Exceptions = new List<FrameExceptionOverride>();
        }

        public string Name { get; set; }
        public string Units { get; set; }
        public double Height { get; set; }
        public double Depth { get; set; }

        /// <summary>
        /// Post peralte (in) as a design value. 0 = inherit the post profile's catalog width. Not visible in the
        /// LATERAL (the post is seen edge-on) but drives the PLANTA post/celosía/plate — and, when this frame is a
        /// selective per-post cabecera, the frontal too.
        /// </summary>
        public double PostPeralte { get; set; }

        // ---- Celosía / diagonal parameters (advanced editor; drive the lateral header builder) ----
        /// <summary>1-based troquel index where the first horizontal of the celosía sits.</summary>
        public int CelosiaStartTroquel { get; set; } = 3;

        /// <summary>The diagonal starts this many troqueles above the lower horizontal of its panel.</summary>
        public int DiagonalStartOffsetTroqueles { get; set; } = 2;

        /// <summary>The diagonal ends this many troqueles below the upper horizontal of its panel.</summary>
        public int DiagonalEndOffsetTroqueles { get; set; } = 2;

        /// <summary>
        /// Vertical separation (in troqueles) between the two parallel diagonals of a double-diagonal panel.
        /// The upper diagonal sits this many troqueles above the lower one, keeping the same slope.
        /// </summary>
        public int DiagonalDoubleSpacingTroqueles { get; set; } = 1;

        /// <summary>
        /// When a horizontal has quantity &gt; 1 (double horizontal), each extra travesaño sits this many
        /// troqueles above the previous one. Panel clears are measured from the first travesaño of a panel.
        /// </summary>
        public int HorizontalDoubleOffsetTroqueles { get; set; } = 1;

        /// <summary>Troquel pitch on the post (in). Troqueles repeat every PasoTroquel.</summary>
        public double PasoTroquel { get; set; } = 2.0;

        /// <summary>Standard vertical clear between consecutive standard horizontals (panel height, in).</summary>
        public double PanelClear { get; set; } = 44.0;

        public string StandardBaselineId { get; set; }
        public string StandardBaselineVersion { get; set; }
        public PostAssembly LeftPost { get; set; }
        public PostAssembly RightPost { get; set; }
        public BasePlatePlacement LeftBasePlate { get; set; }
        public BasePlatePlacement RightBasePlate { get; set; }
        public IList<FrameHorizontal> Horizontals { get; private set; }
        public IList<BracingPanel> BracingPanels { get; private set; }
        public IList<FrameMember> Members { get; private set; }
        public IList<FrameExceptionOverride> Exceptions { get; private set; }
    }
}
