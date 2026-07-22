using System.Collections.Generic;
using System.Linq;
using RackCad.Domain.Systems;

namespace RackCad.Application.Systems
{
    /// <summary>
    /// Pure safety-selection rules of the dynamic editor, extracted from the window (I-21). The predicate decides whether a
    /// selection actually draws (so the toolbar count and the persisted design agree), and the copy keeps only drawable,
    /// element-bound selections as deep copies — the same filter the window applied before snapshotting a design.
    /// </summary>
    public static class DynamicEditorSafety
    {
        /// <summary>True when a selection places at least one physical piece (rack-wide, a face, or a per-post side).</summary>
        public static bool Draws(SelectiveSafetySelection selection)
            => selection != null
               && (selection.Quantity > 0
                   || selection.Side != SafetySide.None
                   || selection.PostSides.Any(post => post != null && post.Side != SafetySide.None));

        /// <summary>Replace <paramref name="target"/> with deep copies of the drawable, element-bound selections in
        /// <paramref name="source"/> (was the window's ReplaceSafetySelections). Empty or non-drawing selections drop out.</summary>
        public static void CopyDrawable(
            ICollection<SelectiveSafetySelection> target,
            IEnumerable<SelectiveSafetySelection> source)
        {
            target.Clear();
            foreach (var safety in source ?? Enumerable.Empty<SelectiveSafetySelection>())
            {
                if (Draws(safety) && !string.IsNullOrWhiteSpace(safety.ElementId))
                {
                    target.Add(safety.DeepCopy());
                }
            }
        }
    }
}
