using System.Collections.Generic;
using RackCad.Domain.Systems;

namespace RackCad.Application.Systems
{
    /// <summary>
    /// The already-validated scalar/toggle inputs the selective editor reads from its WPF controls and hands to
    /// <see cref="SelectiveEditorState.BuildDesign"/>. Keeping this a plain data contract lets the design assembly stay
    /// pure and testable (initiative I-20): the window owns reading and validating the controls (and their Spanish error
    /// messages); the state owns turning the matrices + these inputs into a <see cref="SelectivePalletDesign"/>.
    /// </summary>
    public sealed class SelectiveDesignInputs
    {
        public string PostId { get; set; }
        public double PostPeralte { get; set; }
        public double PalletTolerance { get; set; }
        public double VerticalClearance { get; set; }
        public double FloorBeamRise { get; set; }

        /// <summary>The "Fondo de tarima" value read from its box, used ONLY as a last-resort fallback for fondo 0's
        /// depth when its saved slot has no positive depth (mirrors the editor's <c>fondo0.Depth &gt; 0 ? … : fondo</c>).</summary>
        public double Fondo { get; set; }

        public int DepthCount { get; set; }

        /// <summary>The working fondo's depth (read from its box with the editor's keep-previous fallback), committed into
        /// the selected fondo slot before fondo 0 is read.</summary>
        public double WorkingDepth { get; set; }

        /// <summary>The working fondo's cabecera-fondo override (0 = auto), committed alongside <see cref="WorkingDepth"/>.</summary>
        public double WorkingCabeceraOverride { get; set; }

        /// <summary>One separation per gap (read from the dynamic separator boxes), added to the design in order.</summary>
        public IReadOnlyList<double> Separators { get; set; } = new List<double>();

        public bool DrawBasePlate { get; set; }
        public bool NumberFronts { get; set; }
        public bool NumberLevels { get; set; }
        public bool DrawRackName { get; set; }
        public bool DrawPallets { get; set; }
        public double AnnotationScale { get; set; }
        public DimensionDetail Dimensions { get; set; }
        public string DimensionStyle { get; set; }

        /// <summary>The drawing-eligible safety selections, already filtered and deep-copied by the window (I-22 keeps
        /// safety ownership in the editor); the state adds them to the design verbatim.</summary>
        public IReadOnlyList<SelectiveSafetySelection> SafetySelections { get; set; } = new List<SelectiveSafetySelection>();
    }
}
