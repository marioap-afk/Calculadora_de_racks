using RackCad.Domain.RackFrames;
using RackCad.Domain.Systems;

namespace RackCad.Application.Persistence
{
    /// <summary>
    /// A loaded project: a cabecera header, a dynamic system, a selective pallet-rack design, a flow bed (cama), or a
    /// larguero component. The <see cref="Kind"/> selects which payload is set. Members are already rebuilt by the store.
    /// </summary>
    public sealed class RackProject
    {
        private RackProject()
        {
        }

        public RackSystemKind Kind { get; private set; }
        public RackFrameConfiguration Header { get; private set; }
        public DynamicRackSystem DynamicSystem { get; private set; }

        /// <summary>The editable pallet-flow inputs that produced <see cref="DynamicSystem"/>.</summary>
        public DynamicRackDesign DynamicDesign { get; private set; }

        /// <summary>The selective pallet-rack design (its persisted document, with Id + Name); set when <see cref="Kind"/> is SelectiveRack.</summary>
        public SelectivePalletDesignDocument SelectiveRack { get; private set; }

        /// <summary>The flow bed (cama) configuration; set when <see cref="Kind"/> is Cama.</summary>
        public FlowBedConfiguration FlowBed { get; private set; }

        /// <summary>The larguero component; set when <see cref="Kind"/> is Larguero.</summary>
        public LargueroDesign Larguero { get; private set; }

        /// <summary>
        /// The persistence document this project was loaded from, kept so a re-save can carry forward JSON fields this
        /// build does not know about (wrapper- and payload-level <c>ExtensionData</c>, I-11 D3). Null for a project built
        /// in memory (a fresh save then writes only the known fields, exactly as before). It is a Persistence type, so it
        /// never leaks JSON metadata into the Domain. Set only by the store on load.
        /// </summary>
        internal RackProjectDocument SourceDocument { get; set; }

        /// <summary>
        /// Carry the persistence metadata (unknown JSON fields + non-downgraded schema version) of a previously LOADED
        /// <paramref name="source"/> onto THIS in-memory project, so a library re-save of an edited design preserves it
        /// (I-11 D3). The known model on this project is what gets written; only the source's underlying document (its
        /// <c>ExtensionData</c> and schema version) rides along. This is the seam the UI editors use: they build a fresh
        /// project from the edited model and attach the metadata of the project they opened. No-op if <paramref name="source"/>
        /// is null or was itself built in memory. Only the persistence document travels — no JSON metadata reaches the Domain.
        /// </summary>
        public RackProject WithSourceMetadataFrom(RackProject source)
        {
            SourceDocument = source?.SourceDocument;
            return this;
        }

        /// <summary>
        /// Attach the persistence metadata of a standalone source <paramref name="sourceFlowBed"/> document so a LIBRARY
        /// re-save of a cama opened from the DRAWING preserves its unknown fields and non-downgraded schema version, even
        /// though no source <see cref="RackProject"/> exists (I-11 D3). A Persistence-layer seam — the UI passes the
        /// <see cref="FlowBedDocument"/> it read from the embed, never hand-built JSON. Effective only for a cama project
        /// (the wrapper writes its FlowBed slot); no-op if <paramref name="sourceFlowBed"/> is null.
        /// </summary>
        public RackProject WithSourceFlowBed(FlowBedDocument sourceFlowBed)
        {
            if (sourceFlowBed != null)
            {
                SourceDocument = new RackProjectDocument { Kind = RackSystemKind.Cama, FlowBed = sourceFlowBed };
            }

            return this;
        }

        public static RackProject ForSelective(RackFrameConfiguration header)
        {
            return new RackProject { Kind = RackSystemKind.Selective, Header = header };
        }

        public static RackProject ForDynamic(DynamicRackSystem system)
        {
            var design = system == null ? null : DynamicRackSystemDocument.From(system).ToDesign();
            return new RackProject { Kind = RackSystemKind.PalletFlow, DynamicSystem = system, DynamicDesign = design };
        }

        public static RackProject ForDynamic(DynamicRackDesign design, DynamicRackSystem system = null)
        {
            var resolved = system ?? (design == null ? null : DynamicRackSystemDocument.From(design).ToDomain());
            return new RackProject { Kind = RackSystemKind.PalletFlow, DynamicSystem = resolved, DynamicDesign = design };
        }

        public static RackProject ForSelectiveRack(SelectivePalletDesignDocument design)
        {
            return new RackProject { Kind = RackSystemKind.SelectiveRack, SelectiveRack = design };
        }

        public static RackProject ForCama(FlowBedConfiguration flowBed)
        {
            return new RackProject { Kind = RackSystemKind.Cama, FlowBed = flowBed };
        }

        public static RackProject ForLarguero(LargueroDesign larguero)
        {
            return new RackProject { Kind = RackSystemKind.Larguero, Larguero = larguero };
        }
    }
}
