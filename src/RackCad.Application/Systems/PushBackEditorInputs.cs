using System.Collections.Generic;
using RackCad.Domain.Systems;

namespace RackCad.Application.Systems
{
    /// <summary>
    /// Rack-wide (non-per-front) inputs the Push Back editor needs to assemble a design but that are NOT owned by
    /// <see cref="PushBackEditorState"/> (whose authority is the transverse structure and the rear peralte/tope). The
    /// window reads these from the shared panels — pallet, fondos, post, annotations, safety — exactly as the dynamic
    /// window does; <see cref="PushBackEditorDesignAssembler"/> combines them with the state to build the design, and a
    /// load returns the set recovered from a persisted design so the window can repopulate those panels.
    /// </summary>
    public sealed class PushBackEditorInputs
    {
        public PalletSpecification Pallet { get; set; } = new PalletSpecification(42.0, 48.0, 60.0, 1000.0, "kg");
        public int PalletsDeep { get; set; } = DynamicRackDefaults.DefaultPalletsDeep;
        public string PostCatalogId { get; set; }
        public double PostPeralte { get; set; }
        public double PalletTolerance { get; set; } = DynamicRackDefaults.DefaultPalletTolerance;
        public double BeamDepth { get; set; } = DynamicRackDefaults.DefaultBeamDepth;
        public DynamicAnnotationOptions Annotations { get; set; } = new DynamicAnnotationOptions();

        /// <summary>Entrance-side safety selections. GUIA (entrance guides) are stripped at build; Push Back admits none.</summary>
        public IList<SelectiveSafetySelection> SafetySelections { get; } = new List<SelectiveSafetySelection>();

        /// <summary>The rack-wide inputs a brand-new Push Back design opens with (mirrors the dynamic new-design defaults).</summary>
        public static PushBackEditorInputs NewDesign() => new PushBackEditorInputs();
    }
}
