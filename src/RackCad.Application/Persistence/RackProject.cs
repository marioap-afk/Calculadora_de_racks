using RackCad.Domain.RackFrames;
using RackCad.Domain.Systems;

namespace RackCad.Application.Persistence
{
    /// <summary>
    /// A loaded project: either a selective header (a single <see cref="RackFrameConfiguration"/>)
    /// or a dynamic (pallet flow) system. Members are already rebuilt by the store.
    /// </summary>
    public sealed class RackProject
    {
        private RackProject()
        {
        }

        public RackSystemKind Kind { get; private set; }
        public RackFrameConfiguration Header { get; private set; }
        public DynamicRackSystem DynamicSystem { get; private set; }

        public static RackProject ForSelective(RackFrameConfiguration header)
        {
            return new RackProject { Kind = RackSystemKind.Selective, Header = header };
        }

        public static RackProject ForDynamic(DynamicRackSystem system)
        {
            return new RackProject { Kind = RackSystemKind.PalletFlow, DynamicSystem = system };
        }
    }
}
