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

        /// <summary>The selective pallet-rack design (its persisted document, with Id + Name); set when <see cref="Kind"/> is SelectiveRack.</summary>
        public SelectivePalletDesignDocument SelectiveRack { get; private set; }

        /// <summary>The flow bed (cama) configuration; set when <see cref="Kind"/> is Cama.</summary>
        public FlowBedConfiguration FlowBed { get; private set; }

        /// <summary>The larguero component; set when <see cref="Kind"/> is Larguero.</summary>
        public LargueroDesign Larguero { get; private set; }

        public static RackProject ForSelective(RackFrameConfiguration header)
        {
            return new RackProject { Kind = RackSystemKind.Selective, Header = header };
        }

        public static RackProject ForDynamic(DynamicRackSystem system)
        {
            return new RackProject { Kind = RackSystemKind.PalletFlow, DynamicSystem = system };
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
