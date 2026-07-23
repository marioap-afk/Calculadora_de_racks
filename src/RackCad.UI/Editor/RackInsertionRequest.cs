using RackCad.Application.Persistence;
using RackCad.Domain.RackFrames;
using RackCad.Domain.Systems;

namespace RackCad.UI.Editor
{
    /// <summary>
    /// The editor→host insertion/update contract, tipado por <see cref="RackSystemKind"/> (initiative I-15). It replaces
    /// the ~19 per-system payload properties that <see cref="RackMainMenuWindow"/> used to carry: an editor module now
    /// produces ONE request that the menu exposes and the Plugin host consumes, dispatching by <see cref="Kind"/> to the
    /// same per-system draw call. Each subtype carries EXACTLY the values the host passed before (frozen by the F-series
    /// characterization + the menu→Plugin wiring), so the drawn result and the I-11 source-metadata transport are
    /// unchanged. Pure UI data: no WPF, no AutoCAD, no drawing logic — the Plugin owns the dispatch.
    /// </summary>
    public abstract class RackInsertionRequest
    {
        private protected RackInsertionRequest()
        {
        }

        /// <summary>The canonical system kind this request draws (used by the Plugin host to dispatch).</summary>
        public abstract RackSystemKind Kind { get; }
    }

    /// <summary>
    /// Insert a standalone cabecera (<see cref="RackSystemKind.Selective"/>). Mirrors the old
    /// <c>RackCabeceraCommands.DrawAndPlace(ConfigurationToInsert, ConfigurationSourceProjectToInsert)</c>.
    /// </summary>
    public sealed class HeaderInsertionRequest : RackInsertionRequest
    {
        public HeaderInsertionRequest(RackFrameConfiguration configuration, RackProject sourceProject)
        {
            // No null-guard on the payload: a module only builds a request when the editor asked to insert (so it is set),
            // and the Plugin's Draw* already returns early on a null payload — matching the old "null → no draw" behavior.
            Configuration = configuration;
            SourceProject = sourceProject; // null for a brand-new header; the library carries the loaded project (I-11)
        }

        public override RackSystemKind Kind => RackSystemKind.Selective;

        public RackFrameConfiguration Configuration { get; }

        /// <summary>The loaded library project (its unknown JSON fields + schema version) so the new embed preserves them
        /// (I-11). Null for a brand-new header, where the downstream <c>WithSourceMetadataFrom</c> is a no-op.</summary>
        public RackProject SourceProject { get; }
    }

    /// <summary>
    /// Insert a dynamic (pallet-flow) view (<see cref="RackSystemKind.PalletFlow"/>). Mirrors the old
    /// <c>RackDinamicoCommands.DrawDynamicView(View, Section, System, Design, RackId, RackName, source: null,
    /// innerSource: DynamicSourceProjectToInsert)</c>.
    /// </summary>
    public sealed class DynamicInsertionRequest : RackInsertionRequest
    {
        public DynamicInsertionRequest(
            DynamicRackSystem system, DynamicRackDesign design, string rackId, string rackName, string view, int section,
            RackProject sourceProject)
        {
            System = system; // see HeaderInsertionRequest: the Plugin's Draw* guards null, preserving "null → no draw"
            Design = design;
            RackId = rackId;
            RackName = rackName;
            View = view;
            Section = section;
            SourceProject = sourceProject; // library wrapper metadata to carry into the embed (I-11); null for a new design
        }

        public override RackSystemKind Kind => RackSystemKind.PalletFlow;

        public DynamicRackSystem System { get; }

        public DynamicRackDesign Design { get; }

        public string RackId { get; }

        public string RackName { get; }

        public string View { get; }

        public int Section { get; }

        public RackProject SourceProject { get; }
    }

    /// <summary>
    /// Insert a flow bed / cama (<see cref="RackSystemKind.Cama"/>). Mirrors the old
    /// <c>RackCamaCommands.DrawAndPlaceBed(FlowBedToInsert, BuildCamaPayload(FlowBedToInsert, RackId, RackName, null,
    /// FlowBedSourceDocumentToInsert), RackName)</c>.
    /// </summary>
    public sealed class FlowBedInsertionRequest : RackInsertionRequest
    {
        public FlowBedInsertionRequest(FlowBedConfiguration flowBed, string rackId, string rackName, FlowBedDocument sourceDocument)
        {
            FlowBed = flowBed; // see HeaderInsertionRequest: the Plugin's Draw* guards null, preserving "null → no draw"
            RackId = rackId;
            RackName = rackName;
            SourceDocument = sourceDocument; // source FlowBed document (unknown fields + version) to carry into the embed (I-11)
        }

        public override RackSystemKind Kind => RackSystemKind.Cama;

        public FlowBedConfiguration FlowBed { get; }

        public string RackId { get; }

        public string RackName { get; }

        public FlowBedDocument SourceDocument { get; }
    }

    /// <summary>
    /// Insert a Push Back view (<see cref="RackSystemKind.PushBack"/>), initiative I-18. Carries exactly the values the
    /// Push Back editor produces (the resolved system + its editable design + the identity + the normalized view/section +
    /// the I-11 source-metadata project); a null <see cref="View"/> with <see cref="Section"/> = -1 is the in-place update.
    /// Pure UI data — no drawing logic; the Plugin host (a later increment) dispatches by <see cref="Kind"/>.
    /// </summary>
    public sealed class PushBackInsertionRequest : RackInsertionRequest
    {
        public PushBackInsertionRequest(
            PushBackSystem system, PushBackDesign design, string rackId, string rackName, string view, int section,
            RackProject sourceProject)
        {
            System = system; // see HeaderInsertionRequest: the Plugin's Draw* guards null, preserving "null → no draw"
            Design = design;
            RackId = rackId;
            RackName = rackName;
            View = view;
            Section = section;
            SourceProject = sourceProject; // library wrapper metadata to carry into the embed (I-11); null for a new design
        }

        public override RackSystemKind Kind => RackSystemKind.PushBack;

        public PushBackSystem System { get; }

        public PushBackDesign Design { get; }

        public string RackId { get; }

        public string RackName { get; }

        /// <summary>The requested view, or null on an update (in-place redraw of every existing view).</summary>
        public string View { get; }

        /// <summary>The requested section (post index for lateral, (int)PushBackFrontalEnd for frontal, -1 for planta/update).</summary>
        public int Section { get; }

        public RackProject SourceProject { get; }
    }

    /// <summary>
    /// Insert a selective-rack view (<see cref="RackSystemKind.SelectiveRack"/>). Mirrors the old
    /// <c>RackSelectivoCommands.DrawSelectiveView(View, System, Design, RackId, RackName)</c>.
    /// </summary>
    public sealed class SelectiveInsertionRequest : RackInsertionRequest
    {
        public SelectiveInsertionRequest(
            SelectiveRackSystem system, SelectivePalletDesign design, string rackId, string rackName, string view)
        {
            System = system; // see HeaderInsertionRequest: the Plugin's Draw* guards null, preserving "null → no draw"
            Design = design;
            RackId = rackId;
            RackName = rackName;
            View = view;
        }

        public override RackSystemKind Kind => RackSystemKind.SelectiveRack;

        public SelectiveRackSystem System { get; }

        public SelectivePalletDesign Design { get; }

        public string RackId { get; }

        public string RackName { get; }

        public string View { get; }
    }
}
