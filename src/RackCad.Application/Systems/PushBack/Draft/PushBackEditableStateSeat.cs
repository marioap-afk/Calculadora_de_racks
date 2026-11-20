using RackCad.Application.Catalogs;
using RackCad.Domain.Systems.PushBack;

namespace RackCad.Application.Systems.PushBack.Draft
{
    /// <summary>
    /// I-64 F4B PB-T3a (D-PBT3-03) — the seat of the editable state: the recompute pass of the window and the sequence of
    /// its LoadExisting, as product code.
    /// </summary>
    public static class PushBackEditableStateSeat
    {
        public static PushBackEditorComputation Pass(
            PushBackEditableState state, PushBackEditorDesignAssembler assembler,
            PushBackCompositeEditorAssembler compositeAssembler, PushBackSide frontalSide)
            => null;

        public static PushBackEditableState LoadExisting(PushBackDesign design, string rackName, RackCatalog catalog)
            => new PushBackEditableState();
    }
}
