#nullable enable
namespace RackCad.Application.Systems.PushBack.Pilot
{
    /// <summary>
    /// I-64 F4B PB-T3b (D-PBT3-10) — the capacity of the scope of MUTATE to take away one view of the rack that the update leaves
    /// obsolete, by the key of its definition. It is a separate interface so the port of the update does not change: a scope that does
    /// not implement it cannot take views away, and the run refuses a plan that asks for it before it reads or writes anything.
    /// </summary>
    public interface IPushBackPilotViewEraser
    {
        /// <summary>Takes away the view of that key. It is called between the start of MUTATE and its seal, once per key of the plan.</summary>
        PushBackPilotUnitResult EraseUnit(string memberKey);
    }
}
