using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Catalogs;
using RackCad.Domain.Systems;

namespace RackCad.Application.Systems
{
    /// <summary>
    /// The single, pure authority for Push Back safety selections. Push Back admits every applicable safety family EXCEPT
    /// entrance GUIDES (GUIA) and walk grids (PARRILLA), and only at the LOW (entrance/exit) end — never the rear. This one
    /// implementation is shared by <see cref="PushBackResolver"/> and <see cref="PushBackEditorDesignAssembler.AuthorizedSafety"/>,
    /// so there are never two divergent copies of the restriction: it drops the unsupported families, deep-copies each
    /// surviving selection, and normalizes it to the low end (Side = Left, per-post side overrides cleared, the rear defensa
    /// length zeroed). Because every consumer (resolver, system, plans, drawing, BOM) reads ONLY <see cref="Authorize"/>'s
    /// output, an unsupported selection persisted by an OLDER document is read back without error but never reaches a plan,
    /// a view or the BOM (PB-VAL-06): it is stripped at build, exactly as GUIA already was — no destructive migration. The
    /// input collection and its selections are never mutated.
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
            => IsFamily(selection, SelectiveSafetyDefaults.GuiaType);

        /// <summary>True when the selection's catalog element is a walk grid (type PARRILLA) — never admitted by Push Back (PB-VAL-06).</summary>
        public bool IsRearGrid(SelectiveSafetySelection selection)
            => IsFamily(selection, SelectiveSafetyDefaults.ParrillaType);

        /// <summary>True when the selection's catalog element is of the TOPE family. Owner decision (2026-07-24): in Push Back
        /// the rear stop belongs to the HIGH end and is owned by the rear-tope config (SAQUE + per-cell deactivations),
        /// so it must never travel as ordinary low-end SAFETY — that would give one physical piece two authorities.</summary>
        public bool IsRearStop(SelectiveSafetySelection selection)
            => IsFamily(selection, SelectiveSafetyDefaults.TopeType);

        /// <summary>The canonical Push Back exclusion: a GUIA (entrance guide), a PARRILLA (walk grid) or a TOPE (owned by
        /// the rear-tope config) is never admitted as safety, on either end. Every downstream consumer reads only
        /// <see cref="Authorize"/>'s output, so this one predicate is the single authority for what Push Back refuses.</summary>
        public bool IsUnsupported(SelectiveSafetySelection selection)
            => IsEntranceGuide(selection) || IsRearGrid(selection) || IsRearStop(selection);

        private bool IsFamily(SelectiveSafetySelection selection, string type)
        {
            if (selection == null || string.IsNullOrWhiteSpace(selection.ElementId))
            {
                return false;
            }

            var element = catalog?.SafetyElements?.FirstOrDefault(entry => entry != null
                && string.Equals(entry.Id, selection.ElementId, StringComparison.OrdinalIgnoreCase));
            return element != null && SelectiveSafetyDefaults.IsType(element.Type, type);
        }

        /// <summary>
        /// The authorized, low-end-only safety set: deep copies of the GUIA/PARRILLA-free selections, each restricted to the
        /// low end. Independent of the source (the input collection and its selections are never mutated).
        /// </summary>
        public IReadOnlyList<SelectiveSafetySelection> Authorize(IEnumerable<SelectiveSafetySelection> source)
        {
            var result = new List<SelectiveSafetySelection>();
            foreach (var selection in source ?? Enumerable.Empty<SelectiveSafetySelection>())
            {
                if (selection == null || IsUnsupported(selection))
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
        /// The safety a BRAND-NEW Push Back system opens with (PB-VAL-04: it used to open with none, so a new rack drew no
        /// safety at all). It is the SAME catalog-driven authority the dynamic editor seeds a new rack from
        /// (<see cref="DynamicSafetyDefaults.Build"/>) run through <see cref="Authorize"/> — so the GUIA the dynamic default
        /// set includes is dropped and every surviving family is restricted to the LOW end. No hard-coded family list, and
        /// the shared defaults are never mutated (<see cref="DynamicSafetyDefaults.Build"/> mints fresh selections and
        /// <see cref="Authorize"/> deep-copies them again).
        /// </summary>
        public IReadOnlyList<SelectiveSafetySelection> Defaults() => Authorize(DynamicSafetyDefaults.Build(catalog));

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

            // PB-009 (I-32): mark the selection itself, so the ADAPTIVE defaults of every family stop reaching the far
            // end. Zeroing the stored records was not enough — a brand-new rack carries NO records at all, so the
            // forklift defence fell straight through to the 12/36 default and drew a rear piece in lateral, planta and
            // the BOM, and the lateral guard's adaptive rule put a guard on the last post's far face.
            //
            // It is a DEFAULT, not a prohibition: an end the user explicitly set is honoured, which is why the stored
            // entrance lengths are no longer wiped. What IS cleared is the AUTO flag of that end — an automatic far end
            // has no meaning here and would resolve back to 12/36.
            selection.LowEndOnly = true;

            // PB-002 (I-32): the desviador grid Push Back shows has one column per POST, so its off-cells are keyed by
            // post. Marking the selection is what makes the frontal, the planta and the BOM read the same cell the
            // lateral does — before this they collapsed the last two columns with a Math.Min onto the last front.
            selection.DesviadorCellsAreByPost = true;
            foreach (var post in selection.DefensaPosts)
            {
                if (post != null)
                {
                    post.EntranceAuto = false;
                }
            }
        }
    }
}
