using System.Collections.Generic;
using RackCad.Application.Systems.Dynamic;
using RackCad.Application.Systems.Shared;
using RackCad.Domain.Systems.Dynamic;
using RackCad.Domain.Systems.Selective;
using RackCad.Domain.Systems.Shared;

namespace RackCad.Application.Systems.PushBack
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

        /// <summary>
        /// I-42 — desde donde se mide «Alto 1er nivel» en ESTE rack. Un rack NUEVO nace con el datum del producto
        /// —el troquel utilizable mas bajo—; uno CARGADO conserva el que traiga su documento, que para todo archivo
        /// anterior es la lectura historica. Asi ninguna geometria existente se mueve y toda geometria nueva usa el
        /// cero real.
        /// </summary>
        public int? FirstLevelDatum { get; set; } = (int)RackFirstLevelDatumMode.LowestUsablePunch;
        public double BeamDepth { get; set; } = DynamicRackDefaults.DefaultBeamDepth;
        public DynamicAnnotationOptions Annotations { get; set; } = new DynamicAnnotationOptions();

        // ---- Advanced RACK-WIDE parameters (I-35, Owner round 2) --------------------------------------------------
        // These are TRANSPORT, not a second authority: same names, same types and same nullability as the properties
        // of DynamicRackDesign/DynamicRackSystem that already own them, assigned across with no transformation. The
        // rule of each one lives where it always did — the resolver, the separator geometry, the lateral builder and
        // the BOM — and Push Back only carries the user's intent from its panel to the shared structure it composes.
        // They are parameters of the RACK, never properties of a Separator module.

        /// <summary>Manual cabecera height. Null = the height derived from the load inputs (the standing calculation).</summary>
        public double? ManualHeaderHeightOverride { get; set; }

        /// <summary>Whether the derived post carries its reinforcement. False removes ONLY the reinforcement: the
        /// derived post itself is a structural consequence of two consecutive separators and always exists.</summary>
        public bool DerivedPostReinforced { get; set; } = true;

        /// <summary>Reinforcement length. Null = full height of the derived post; a value = partial, from the base up.</summary>
        public double? DerivedPostReinforcementHeight { get; set; }

        /// <summary>
        /// I-40 (Owner): ALTURA del poste derivado. Vacio = la altura de la cabecera, que es lo que el poste
        /// derivado heredaba y sigue heredando cuando nadie la fija — un rack antiguo se comporta exactamente igual.
        /// Es hermana de <see cref="DerivedPostReinforcementHeight"/> y vive en el mismo sitio: el poste derivado es
        /// del RACK, no de una cabecera.
        /// </summary>
        public double? DerivedPostHeight { get; set; }


        /// <summary>Separator count per cabecera. Null = the standard calculation.</summary>
        public int? SeparatorCountOverride { get; set; }

        /// <summary>Separator spacing. Null = the standard calculation. Independent of the count.</summary>
        public double? SeparatorSpacingOverride { get; set; }

        /// <summary>Entrance-side safety selections. GUIA (entrance guides) are stripped at build; Push Back admits none.</summary>
        public IList<SelectiveSafetySelection> SafetySelections { get; } = new List<SelectiveSafetySelection>();

        /// <summary>The rack-wide inputs a brand-new Push Back design opens with (mirrors the dynamic new-design defaults).</summary>
        public static PushBackEditorInputs NewDesign() => new PushBackEditorInputs();
    }
}
