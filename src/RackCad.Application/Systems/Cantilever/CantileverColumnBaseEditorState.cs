using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Application.StructuralSections;
using RackCad.Domain.Systems.Cantilever;

namespace RackCad.Application.Systems.Cantilever
{
    /// <summary>
    /// The editable state of the column–base configurator, and the ONE place the «la base sigue a la columna»
    /// rule lives.
    ///
    /// Motivo 4 del rechazo de la ronda 1 fue que la base no seguía a la columna. The fix is a rule, not a
    /// convenience: a Cantilever is normally built from ONE profile, so choosing the column should choose the
    /// base — until the user says otherwise, and from then on their choice is theirs.
    ///
    /// <para><b>Following is INTENT, never a comparison.</b> Whether the base follows is stored
    /// (<see cref="CantileverStationColumnBaseTemplateDesign.BaseFollowsColumn"/>) and never derived from «the
    /// two ids happen to be equal»: two sections can coincide by accident, and a user who deliberately picked
    /// the same base would silently lose their decision the first time they changed the column. It is persisted
    /// for the same reason — reopening a line must not change the rule under it.</para>
    ///
    /// <para>It edits a COPY. The window hands the copy back only when the user accepts, so cancelling cannot
    /// mutate the design it was opened from.</para>
    /// </summary>
    public sealed class CantileverColumnBaseEditorState
    {
        private readonly ICantileverColumnBaseSectionPolicy _policy;
        private readonly List<CantileverDiagnostic> _diagnostics = new List<CantileverDiagnostic>();

        /// <summary>
        /// Opens the editor on a COPY of <paramref name="template"/>.
        ///
        /// A null template is a brand-new column–base: it starts following, because a new design has no base the
        /// user chose on purpose.
        /// </summary>
        public CantileverColumnBaseEditorState(
            CantileverStationColumnBaseTemplateDesign template,
            ICantileverColumnBaseSectionPolicy policy)
        {
            _policy = policy ?? throw new ArgumentNullException(nameof(policy));
            Template = template?.DeepCopy() ?? new CantileverStationColumnBaseTemplateDesign();
        }

        /// <summary>The working copy. Never the caller's instance.</summary>
        public CantileverStationColumnBaseTemplateDesign Template { get; }

        /// <summary>Whether a change of column still drags the base with it.</summary>
        public bool BaseFollowsColumn => Template.BaseFollowsColumn;

        /// <summary>What the last operation had to say. Cleared at the start of each one.</summary>
        public IReadOnlyList<CantileverDiagnostic> Diagnostics => _diagnostics;

        public string ColumnSectionId => Template.ColumnSectionId;

        public string BaseSectionId => Template.Base?.SectionId;

        /// <summary>
        /// The user chose a column.
        ///
        /// With following ON the base moves with it — unless that section cannot BE a base, and then nothing is
        /// normalized and the reason is reported: silently leaving the base behind would look like the rule had
        /// been forgotten, and silently writing an ineligible base would produce a line that cannot resolve.
        /// </summary>
        public void SelectColumn(string sectionId)
        {
            _diagnostics.Clear();
            Template.ColumnSectionId = sectionId;

            if (!Template.BaseFollowsColumn)
            {
                return; // the user's base is the user's
            }

            if (!IsEligibleAsBase(sectionId))
            {
                _diagnostics.Add(CantileverDiagnostic.Warning(
                    CantileverDiagnostics.ColumnBaseSectionNotEligible,
                    "La seccion '" + (sectionId ?? "(ninguna)") + "' no es elegible como base, asi que la base " +
                    "no se cambio. Elige una base a mano o cambia la columna."));

                return;
            }

            EnsureBase().SectionId = sectionId;
        }

        /// <summary>
        /// The user chose a base by hand. From now on the base is theirs and a column change leaves it alone.
        /// </summary>
        public void SelectBase(string sectionId)
        {
            _diagnostics.Clear();
            EnsureBase().SectionId = sectionId;
            Template.BaseFollowsColumn = false;
        }

        /// <summary>
        /// «Usar la misma sección que la columna»: turns following back on AND applies it now.
        ///
        /// Turning it on without applying it would leave the two disagreeing until the next column change, which
        /// is exactly the surprise the button exists to remove.
        /// </summary>
        public void UseSameSectionAsColumn()
        {
            _diagnostics.Clear();

            if (!IsEligibleAsBase(Template.ColumnSectionId))
            {
                _diagnostics.Add(CantileverDiagnostic.Warning(
                    CantileverDiagnostics.ColumnBaseSectionNotEligible,
                    "La columna actual no es elegible como base, asi que no se copio."));

                return;
            }

            Template.BaseFollowsColumn = true;
            EnsureBase().SectionId = Template.ColumnSectionId;
        }

        /// <summary>Whether a section may be used as a base, asked to the PRODUCT policy and to nobody else.</summary>
        public bool IsEligibleAsBase(string sectionId)
        {
            if (string.IsNullOrWhiteSpace(sectionId) ||
                !StructuralSectionId.TryParse(sectionId, out var id))
            {
                return false;
            }

            return _policy.TryGetVariant(id, id, out _);
        }

        /// <summary>The template to hand back on «Aceptar». A copy again, so the editor keeps editing its own.</summary>
        public CantileverStationColumnBaseTemplateDesign Accept() => Template.DeepCopy();

        private CantileverBaseDesign EnsureBase() => Template.Base ??= new CantileverBaseDesign();

        /// <summary>A compact, deterministic line for the card of the main window.</summary>
        public string Summary()
        {
            var column = Designation(Template.ColumnSectionId) ?? "(sin columna)";
            var baseSection = Designation(Template.Base?.SectionId) ?? "(sin base)";

            return column + " · base " + baseSection + " · " +
                   (Template.BaseFollowsColumn ? "sigue a la columna" : "base manual");
        }

        /// <summary>
        /// The short designation of an id, for a card that must stay compact.
        ///
        /// It is a DISPLAY conversion of a text the design already holds, not a lookup: it does not consult the
        /// catalogue and it never turns a designation back into an id.
        /// </summary>
        internal static string Designation(string sectionId)
        {
            if (string.IsNullOrWhiteSpace(sectionId))
            {
                return null;
            }

            var parts = sectionId.Split('-');
            return parts.Length >= 3 ? string.Join("-", parts.Skip(2)) : sectionId;
        }
    }
}
