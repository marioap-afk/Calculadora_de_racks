using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Systems.Dynamic;
using RackCad.Application.Systems.PushBack;
using RackCad.Application.Systems.Selective;
using RackCad.Domain.Systems.PushBack;
using RackCad.Domain.Systems.Selective;

namespace RackCad.Application.VentanaRackCad
{
    /// <summary>
    /// T5 pb, PB-S03: the safety of the draft for «Extremos y seguridad» of the window, on a COPY (<see cref="PushBackSafetyCopy"/>) that
    /// <see cref="PushBackSafetyEdit"/> edits as the classic dialog does and <see cref="ReplaceSafety"/> takes back; the caller recomposes.
    /// </summary>
    public sealed partial class PushBackDraftEditor
    {
        /// <summary>The posts the classic dialog of safety counts: max(2, fronts + 1).</summary>
        public int SafetyPostCount => Math.Max(2, Composite.Of(Composite.ActiveSide).Structure.Count + 1);

        /// <summary>
        /// The levels of the desviador grid at each post (PB-002, the rule of the drawing: the tallest front next to the post), of the tallest
        /// side when the rack has two (the classic grid shows the side being edited; the window shows every level a desviador can be drawn at).
        /// </summary>
        public IReadOnlyList<int> DesviadorLevelsPerPost()
        {
            var perSide = Sides().Select(side => DynamicFrontActivation.EffectiveLevelsPerPost(Side(side).Structure.EffectiveLevelCounts())).ToList();
            var posts = perSide.Count == 0 ? 0 : perSide.Max(l => l.Count);
            return Enumerable.Range(0, posts).Select(p => perSide.Max(l => p < l.Count ? l[p] : 0)).ToList();
        }

        /// <summary>A copy of the safety of the draft, with what the classic sections seed from (the resolved bota, the faces of each side).</summary>
        public PushBackSafetyCopy CopySafety()
        {
            var posts = SafetyPostCount;
            var structure = System?.Structure;
            var resolved = SelectiveSafetyFamilies.SelectedOfType(structure?.SafetySelections, Catalog.SafetyElements, SelectiveSafetyDefaults.BotaType);
            return new PushBackSafetyCopy(State.SafetySelections, Composite.Of(PushBackSide.A).DefensePieceId, Composite.Of(PushBackSide.B).DefensePieceId,
                IsTwoSided, posts, resolved,
                PushBackDefenseSides.FacesOf(structure, posts, PushBackSide.A), PushBackDefenseSides.FacesOf(structure, posts, PushBackSide.B));
        }

        /// <summary>
        /// The safety of the draft becomes the copy: the list, the type of defense of side A and, when the rack has a side B, the one of side B
        /// (the classic dialog does not touch side B of a rack of one way). Not recomposed.
        /// </summary>
        public void ReplaceSafety(PushBackSafetyCopy copy)
        {
            if (copy == null) throw new ArgumentNullException(nameof(copy));
            State.SafetySelections.Clear();
            State.SafetySelections.AddRange(copy.Safety.Select(s => s?.DeepCopy()));
            Composite.Of(PushBackSide.A).DefensePieceId = copy.DefensePieceA;
            if (copy.TwoSided && IsTwoSided) Composite.Of(PushBackSide.B).DefensePieceId = copy.DefensePieceB;
        }

        /// <summary>The rows of «Seguridad por poste» of the last valid recomposition, counted as the BOM.</summary>
        public IReadOnlyList<RackCadSafetyRow> SafetyRows() => PushBackSafetyRows.Build(System, Catalog, DesviadorLevelsPerPost());
    }
}
