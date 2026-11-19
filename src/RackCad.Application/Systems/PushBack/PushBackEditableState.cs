using System.Collections.Generic;
using RackCad.Application.Catalogs;
using RackCad.Application.Systems.Dynamic;
using RackCad.Application.Systems.Shared;
using RackCad.Domain.Systems.Dynamic;
using RackCad.Domain.Systems.PushBack;
using RackCad.Domain.Systems.Selective;
using RackCad.Domain.Systems.Shared;

namespace RackCad.Application.Systems.PushBack
{
    /// <summary>Whether a design is a rack whose editable state survives a round trip without losing anything.</summary>
    public enum PushBackEditableSupport
    {
        /// <summary>Every part of the design has a place in the editable state.</summary>
        Supported = 0,

        /// <summary>A one-way rack whose structure keeps modules or lines of a parked composite tail: the tail has no
        /// place in a one-way state, so it is outside the lossless guarantee.</summary>
        DormantCompositeTail = 1
    }

    /// <summary>
    /// The editable state of a Push Back rack outside any window: a GROUPING of the state that already exists, never a
    /// parallel model. It holds the composite editor state (both sides, the composite intent, the parked tail and the
    /// module authority of each side) plus the rack-wide values the editor window used to keep in its own fields and
    /// controls. SKELETON (I-64 F4B PB-T1b, the RED): every member has its initial value and every method is inert.
    /// </summary>
    public sealed class PushBackEditableState
    {
        public PushBackEditableState()
        {
            Editor = new PushBackCompositeEditorState(new PushBackEditorState(), new PushBackEditorState());
            Pallet = PushBackEditorInputs.NewDesign().Pallet;
            PalletsDeep = DynamicRackDefaults.DefaultPalletsDeep;
            PostPeralte = PushBackEditorInputs.NewDesign().PostPeralte;
            PalletTolerance = DynamicRackDefaults.DefaultPalletTolerance;
            BeamDepth = DynamicRackDefaults.DefaultBeamDepth;
            AnnotationScale = 1.0;
            FirstLevelDatum = (int)RackFirstLevelDatumMode.LowestUsablePunch;
            Dimensions = DimensionDetail.None;
            Advanced = PushBackEditorInputs.NewDesign();
        }

        /// <summary>The composite editor state: both sides, the composite intent, the parked tail and the selection.</summary>
        public PushBackCompositeEditorState Editor { get; }

        /// <summary>The general pallet; it also carries the weight unit the editor shows.</summary>
        public PalletSpecification Pallet { get; set; }

        public int PalletsDeep { get; set; }
        public string PostCatalogId { get; set; }
        public double PostPeralte { get; set; }
        public double PalletTolerance { get; set; }
        public double BeamDepth { get; set; }
        public double AnnotationScale { get; set; }

        /// <summary>Where «Alto 1er nivel» is measured from; transported, never invented.</summary>
        public int? FirstLevelDatum { get; set; }

        public bool NumberFronts { get; set; }
        public bool NumberLevels { get; set; }
        public bool DrawRackName { get; set; }
        public DimensionDetail Dimensions { get; set; }

        /// <summary>The named dimension style the loaded design carried.</summary>
        public string DimensionStyle { get; set; }

        /// <summary>The per-view dimension policy the loaded design carried; <c>null</c> is the legacy one.</summary>
        public DimensionViewVisibility? LoadedDimensionViews { get; set; }

        /// <summary>Whether the user changed one of the three view checks since the last load.</summary>
        public bool DimensionViewsTouched { get; set; }

        public bool DimensionsFrontal { get; set; }
        public bool DimensionsLateral { get; set; }
        public bool DimensionsPlanta { get; set; }

        /// <summary>The carrier of the six advanced rack-wide scopes (only those six are meaningful).</summary>
        public PushBackEditorInputs Advanced { get; }

        public List<SelectiveSafetySelection> SafetySelections { get; } = new List<SelectiveSafetySelection>();

        /// <summary>The logical name of the rack (the identity is the GUID, which lives elsewhere).</summary>
        public string Name { get; set; }

        // ---- Loads (skeleton) ----------------------------------------------------------------------------------------

        /// <summary>The editor half of a new rack. Skeleton: inert.</summary>
        public PushBackEditorInputs LoadNewEditor() => PushBackEditorInputs.NewDesign();

        /// <summary>The editor half of loading a design. Skeleton: inert.</summary>
        public PushBackEditorInputs LoadDesignEditor(PushBackDesign design, PushBackResolver resolver) => PushBackEditorInputs.NewDesign();

        /// <summary>The editor half of «Restaurar». Skeleton: inert.</summary>
        public PushBackEditorInputs RestoreEditor(PushBackDesign lastValidDesign, PushBackResolver resolver) => PushBackEditorInputs.NewDesign();

        /// <summary>Adopt the rack-wide values a load recovered. Skeleton: does nothing.</summary>
        public void AdoptCarriers(PushBackEditorInputs inputs, RackCatalog catalog)
        {
        }

        /// <summary>Seed the safety selections a new rack opens with. Skeleton: does nothing.</summary>
        public void SeedDefaultSafety(RackCatalog catalog)
        {
        }

        /// <summary>A new rack, for a host that is not the editor window. Skeleton: inert.</summary>
        public PushBackEditorInputs LoadNew(RackCatalog catalog) => PushBackEditorInputs.NewDesign();

        /// <summary>A design, for a host that is not the editor window. Skeleton: inert.</summary>
        public PushBackEditorInputs LoadDesign(PushBackDesign design, string rackName, PushBackResolver resolver) => PushBackEditorInputs.NewDesign();

        /// <summary>«Restaurar» from the last valid design, for a host that is not the editor window. Skeleton: inert.</summary>
        public PushBackEditorInputs Restore(PushBackDesign lastValidDesign, PushBackResolver resolver) => PushBackEditorInputs.NewDesign();

        /// <summary>The design of side A alone. Skeleton: the same design.</summary>
        public static PushBackDesign SideAOnly(PushBackDesign design) => design;

        // ---- Output (skeleton) ---------------------------------------------------------------------------------------

        /// <summary>The inputs of a recompute. Skeleton: the inputs of a new design.</summary>
        public PushBackEditorInputs BuildInputs(PushBackEditorDesignAssembler assembler) => PushBackEditorInputs.NewDesign();

        // ---- Explicit rules (skeleton) -------------------------------------------------------------------------------

        /// <summary>The post the editor shows for a loaded id. Skeleton: the requested id.</summary>
        public static string ResolvePostCatalogId(RackCatalog catalog, string requested) => requested;

        /// <summary>The weight unit the editor shows after seeding. Skeleton: the requested unit.</summary>
        public static string ResolveWeightUnit(string current, string requested) => requested;

        /// <summary>Whether the design is inside the lossless guarantee. Skeleton: always supported.</summary>
        public static PushBackEditableSupport SupportOf(PushBackDesign design) => PushBackEditableSupport.Supported;
    }
}
