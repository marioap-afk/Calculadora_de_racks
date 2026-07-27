using System;
using System.Globalization;
using RackCad.Application.Systems.Dynamic;
using RackCad.Domain.Systems.Dynamic;

namespace RackCad.Application.Systems.PushBack
{
    /// <summary>
    /// The four advanced RACK-WIDE scopes of the Push Back editor (I-35, Owner round 2): manual cabecera height,
    /// derived-post reinforcement with its optional length, separator count and separator spacing.
    /// <para>
    /// This type owns NO value and NO geometry. The values live where they always did —
    /// <see cref="DynamicRackSystem.ManualHeaderHeightOverride"/>, <see cref="DynamicRackSystem.DerivedPostReinforced"/>,
    /// <see cref="DynamicRackSystem.DerivedPostReinforcementHeight"/>, <see cref="DynamicRackSystem.SeparatorCountOverride"/>
    /// and <see cref="DynamicRackSystem.SeparatorSpacingOverride"/>— and the rules that consume them live in the
    /// resolver, <c>DynamicSeparatorGeometry</c>, the lateral builder and the BOM. What lives here is only the
    /// admissibility check and the assignment, so Push Back can refuse an impossible value with a visible message
    /// instead of drawing a rack nobody asked for.
    /// </para>
    /// <para>
    /// They are parameters of the RACK: they are NOT properties of a <c>Separator</c> module and they belong to their
    /// own section of the panel, not to "Módulo seleccionado".
    /// </para>
    /// </summary>
    public static class PushBackAdvancedRackParameters
    {
        /// <summary>
        /// Admissibility of the scopes that do not depend on the resolved geometry. Empty means "the standing
        /// calculation", which is always admissible; a captured value must be usable.
        /// </summary>
        public static void Validate(PushBackEditorInputs inputs)
        {
            if (inputs == null)
            {
                return;
            }

            if (inputs.ManualHeaderHeightOverride.HasValue && inputs.ManualHeaderHeightOverride.Value <= 0.0)
            {
                throw new InvalidOperationException(
                    "La altura manual de cabecera debe ser mayor que cero. Deja el campo vacío para usar la altura calculada.");
            }

            // Only the reinforcement's OWN admissibility here: it is meaningless when there is no reinforcement, so a
            // leftover value cannot block a rack that does not draw it.
            if (inputs.DerivedPostReinforced
                && inputs.DerivedPostReinforcementHeight.HasValue
                && inputs.DerivedPostReinforcementHeight.Value <= 0.0)
            {
                throw new InvalidOperationException(
                    "La altura del refuerzo debe ser mayor que cero. Deja el campo vacío para reforzar toda la altura del poste derivado.");
            }

            if (inputs.SeparatorCountOverride.HasValue && inputs.SeparatorCountOverride.Value <= 0)
            {
                throw new InvalidOperationException(
                    "La cantidad de separadores debe ser un entero mayor que cero. Deja el campo vacío para el cálculo automático.");
            }

            if (inputs.SeparatorSpacingOverride.HasValue && inputs.SeparatorSpacingOverride.Value <= 0.0)
            {
                throw new InvalidOperationException(
                    "La separación entre separadores debe ser mayor que cero. Deja el campo vacío para el cálculo automático.");
            }
        }

        /// <summary>
        /// The reinforcement cannot be taller than the post it reinforces. Checked against the EFFECTIVE cabecera
        /// height — the physical height the derived post resolves to, manual override included.
        /// <para>
        /// A recompute that shrinks the post (fewer levels, a lower manual height, different geometry) turns a
        /// previously valid length invalid, and that BLOCKS with a visible error: nothing is clamped and nothing is
        /// restored behind the user's back, because silently shortening a reinforcement would ship a rack whose
        /// drawing does not match what was asked for (Owner, I-35).
        /// </para>
        /// </summary>
        public static void ValidateReinforcementAgainstPost(PushBackEditorInputs inputs, double resolvedPostHeight)
        {
            if (inputs == null
                || !inputs.DerivedPostReinforced
                || !inputs.DerivedPostReinforcementHeight.HasValue
                || resolvedPostHeight <= 0.0)
            {
                return;
            }

            var requested = inputs.DerivedPostReinforcementHeight.Value;
            if (requested > resolvedPostHeight)
            {
                throw new InvalidOperationException(string.Format(
                    CultureInfo.CurrentCulture,
                    "La altura del refuerzo ({0:0.##}\") supera la altura del poste derivado ({1:0.##}\"). "
                    + "Reduce el refuerzo, deja el campo vacío para reforzar toda la altura, o aumenta la altura de la cabecera.",
                    requested,
                    resolvedPostHeight));
            }
        }

        /// <summary>
        /// Hand the four scopes to the structure that owns them. Pure assignment: same names, same nullability, no
        /// transformation — the moment a value lands here it is governed by the shared rules, not by Push Back.
        /// </summary>
        public static void ApplyTo(DynamicRackSystem system, PushBackEditorInputs inputs)
        {
            if (system == null || inputs == null)
            {
                return;
            }

            system.ManualHeaderHeightOverride = inputs.ManualHeaderHeightOverride;
            system.DerivedPostReinforced = inputs.DerivedPostReinforced;
            system.DerivedPostReinforcementHeight = inputs.DerivedPostReinforced
                ? inputs.DerivedPostReinforcementHeight
                : null;   // no reinforcement, no length: the structure must not carry a dead measurement
            system.SeparatorCountOverride = inputs.SeparatorCountOverride;
            system.SeparatorSpacingOverride = inputs.SeparatorSpacingOverride;
        }

        /// <summary>Read the four scopes back from a persisted structure, so reopening a design repopulates the panel.</summary>
        public static void ReadFrom(DynamicRackDesign design, PushBackEditorInputs inputs)
        {
            if (design == null || inputs == null)
            {
                return;
            }

            inputs.ManualHeaderHeightOverride = design.ManualHeaderHeightOverride;
            inputs.DerivedPostReinforced = design.DerivedPostReinforced;
            inputs.DerivedPostReinforcementHeight = design.DerivedPostReinforcementHeight;
            inputs.SeparatorCountOverride = design.SeparatorCountOverride;
            inputs.SeparatorSpacingOverride = design.SeparatorSpacingOverride;
        }

        /// <summary>
        /// Back to the standing calculation and the defaults: no manual height, the derived post REINFORCED (its
        /// historical default) at full height, and both separator scopes automatic.
        /// </summary>
        public static void Reset(PushBackEditorInputs inputs)
        {
            if (inputs == null)
            {
                return;
            }

            inputs.ManualHeaderHeightOverride = null;
            inputs.DerivedPostReinforced = true;
            inputs.DerivedPostReinforcementHeight = null;
            inputs.SeparatorCountOverride = null;
            inputs.SeparatorSpacingOverride = null;
        }
    }
}
