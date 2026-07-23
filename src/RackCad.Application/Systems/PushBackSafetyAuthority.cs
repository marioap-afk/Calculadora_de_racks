using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Catalogs;
using RackCad.Domain.Systems;

namespace RackCad.Application.Systems
{
    /// <summary>
    /// The single, pure authority for Push Back safety selections. Push Back admits every applicable safety family EXCEPT
    /// entrance GUIDES, and only at the LOW (entrance/exit) end — never the rear. This one implementation is shared by
    /// <see cref="PushBackResolver"/> and <see cref="PushBackEditorDesignAssembler.AuthorizedSafety"/>, so there are never
    /// two divergent copies of the low-end restriction: it drops GUIA, deep-copies each surviving selection, and normalizes
    /// it to the low end (Side = Left, per-post side overrides cleared, the rear defensa length zeroed). The input
    /// collection and its selections are never mutated.
    /// </summary>
    public sealed class PushBackSafetyAuthority
    {
        private readonly RackCatalog catalog;

        public PushBackSafetyAuthority(RackCatalog catalog)
        {
            this.catalog = catalog ?? new RackCatalog();
        }

        /// <summary>True when the selection's catalog element is an entrance guide (type GUIA) — never admitted by Push Back.</summary>
        public bool IsEntranceGuide(SelectiveSafetySelection selection)
        {
            if (selection == null || string.IsNullOrWhiteSpace(selection.ElementId))
            {
                return false;
            }

            var element = catalog?.SafetyElements?.FirstOrDefault(entry => entry != null
                && string.Equals(entry.Id, selection.ElementId, StringComparison.OrdinalIgnoreCase));
            return element != null && SelectiveSafetyDefaults.IsType(element.Type, SelectiveSafetyDefaults.GuiaType);
        }

        /// <summary>
        /// The authorized, low-end-only safety set: deep copies of the GUIA-free selections, each restricted to the low
        /// end. Independent of the source (the input collection and its selections are never mutated).
        /// </summary>
        public IReadOnlyList<SelectiveSafetySelection> Authorize(IEnumerable<SelectiveSafetySelection> source)
        {
            var result = new List<SelectiveSafetySelection>();
            foreach (var selection in source ?? Enumerable.Empty<SelectiveSafetySelection>())
            {
                if (selection == null || IsEntranceGuide(selection))
                {
                    continue;
                }

                var copy = selection.DeepCopy();
                RestrictToLowEnd(copy);
                result.Add(copy);
            }

            return result;
        }

        /// <summary>
        /// Restrict a safety selection to the LOW (entrance/exit) end only: a two-ended or rear (Right) side collapses to
        /// Left (the exit end); per-post side overrides are cleared so every post uses the low side; a forklift defense
        /// keeps only its exit length (the rear entrance length is zeroed). Mutates the passed COPY, never the source.
        /// </summary>
        public static void RestrictToLowEnd(SelectiveSafetySelection selection)
        {
            if (selection == null)
            {
                return;
            }

            if (selection.Side == SafetySide.Both || selection.Side == SafetySide.Right)
            {
                selection.Side = SafetySide.Left;
            }

            selection.PostSides.Clear();
            foreach (var post in selection.DefensaPosts)
            {
                if (post != null)
                {
                    post.EntranceLength = 0.0;
                }
            }
        }
    }
}
