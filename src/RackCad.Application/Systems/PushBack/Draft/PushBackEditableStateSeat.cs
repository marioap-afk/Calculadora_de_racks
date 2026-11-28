using System;
using RackCad.Application.Catalogs;
using RackCad.Domain.Systems.PushBack;

namespace RackCad.Application.Systems.PushBack.Draft
{
    /// <summary>
    /// I-64 F4B PB-T3a (D-PBT3-03) — the seat of the editable state: the recompute pass of the window and the sequence of
    /// its LoadExisting, as product code. Until the window uses it (PB-T4, A-T3-9) the same sequence lives in two places,
    /// and only CT-PBT3A-01 ties them.
    /// </summary>
    public static class PushBackEditableStateSeat
    {
        /// <summary>
        /// The adoption of a recompute pass as the window runs it, adapted to the state: build the design from the editor and
        /// the inputs, and, when the computation is valid, accept it into side A, commit the consumption of the dormant tail,
        /// clear the module commit and restore the advanced parameters when a standard restore landed. An exception of the
        /// assembly gives <c>null</c>, as the catch of the window does.
        /// </summary>
        public static PushBackEditorComputation Pass(
            PushBackEditableState state, PushBackEditorDesignAssembler assembler,
            PushBackCompositeEditorAssembler compositeAssembler, PushBackSide frontalSide)
        {
            PushBackEditorComputation computation;
            try { computation = assembler.BuildFrom(compositeAssembler.BuildDesign(state.Editor, state.BuildInputs(assembler)), frontalSide); }
            catch (Exception) { return null; }
            if (!computation.IsValid) return computation;
            assembler.AcceptComputation(state.Editor.SideA, computation);
            state.Editor.CommitDormantTailConsumption();
            var standardRestoreLanded = state.Editor.SideA.ModuleCommit?.StandardRestoreRequested == true;
            state.Editor.SideA.ClearModuleCommit();
            if (standardRestoreLanded) state.Editor.Active.RestoreAdvancedRackParameters(state.Advanced);
            _ = state.Editor.SideA.ModuleSession;
            return computation;
        }

        /// <summary>
        /// The state the window would hold after opening that rack: the new window (LoadNew, pass, default safety, pass),
        /// then the design (LoadDesign, pass). Always with the frontal side A.
        /// </summary>
        public static PushBackEditableState LoadExisting(PushBackDesign design, string rackName, RackCatalog catalog)
        {
            if (design == null) throw new ArgumentNullException(nameof(design));
            var state = new PushBackEditableState();
            var assembler = new PushBackEditorDesignAssembler(catalog);
            var compositeAssembler = new PushBackCompositeEditorAssembler(catalog);
            state.LoadNew(catalog);
            Pass(state, assembler, compositeAssembler, PushBackSide.A);
            state.SeedDefaultSafety(catalog);
            Pass(state, assembler, compositeAssembler, PushBackSide.A);
            state.LoadDesign(design, rackName, assembler.Resolver);
            Pass(state, assembler, compositeAssembler, PushBackSide.A);
            return state;
        }
    }
}
