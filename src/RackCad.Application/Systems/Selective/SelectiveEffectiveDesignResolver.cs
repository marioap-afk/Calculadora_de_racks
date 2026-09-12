using System;
using System.Collections.Generic;
using RackCad.Application.Persistence;
using RackCad.Application.ProjectVariables;
using RackCad.Domain.Systems.Selective;

namespace RackCad.Application.Systems.Selective
{
    /// <summary>
    /// THE single point where a project-variable reference becomes a number (I-47 D-04-bis, C4-4).
    ///
    /// <para>
    /// Before this existed, the BOM re-resolved the design on its own from the AUTHORED document, with no
    /// access to the register — so a bound rack would be DRAWN with the variable and QUOTED with the frozen
    /// literal, two numbers for one property and nothing failing. The repository convention already covers
    /// this case: when the drawing, the BOM and the UI have to agree on a number, the rule lives in ONE
    /// function of Application. This is that function.
    /// </para>
    /// <para>
    /// It is PURE: two documents in, one domain design out. No AutoCAD, no I/O, no catalog. That is what
    /// makes the whole semantics of binding, breakage and precedence verifiable in the Core suite.
    /// </para>
    /// <para>
    /// The property that makes it work is that NOBODY ELSE resolves. Geometry, BOM and preview receive a
    /// design that is already effective and cannot tell whether a value came from a literal or a variable —
    /// so they cannot disagree. The entry point moves; it does not get duplicated. <c>ToDomain()</c> keeps
    /// existing and is, exactly, the unbound case.
    /// </para>
    /// </summary>
    public sealed class SelectiveEffectiveDesignResolver
    {
        /// <summary>
        /// Resolves against a register document. Kept as the productive facade so existing call sites read the
        /// same, but it no longer resolves anything itself: a null document is the legacy "this drawing has no
        /// register" case (C-1) and is mapped EXPLICITLY, here and only here, to an ABSENT read — the one input
        /// for which an empty register is the right answer.
        /// </summary>
        public SelectiveEffectiveResolution Resolve(
            SelectivePalletDesignDocument authored,
            ProjectVariablesDocument projectVariables)
            => ResolveAccredited(
                authored,
                projectVariables == null
                    ? ProjectVariablesReadResult.Absent()
                    : ProjectVariablesReadResult.Readable(projectVariables));

        /// <summary>
        /// Resolves against a READ, so the accreditation belongs to THAT read. An unreadable register, an
        /// incompatible major and an ambiguous identity all fail here, before a single field is written.
        /// </summary>
        public SelectiveEffectiveResolution ResolveAccredited(
            SelectivePalletDesignDocument authored, ProjectVariablesReadResult read)
        {
            if (authored == null)
            {
                throw new ArgumentNullException(nameof(authored));
            }

            var accreditation = UsableProjectVariablesRegistry.Accredit(read);

            return accreditation.IsUsable
                ? ResolveAgainst(authored, accreditation.Registry)
                : SelectiveEffectiveResolution.Failure(
                    SelectiveEffectiveOutcome.BrokenProjectVariableReference,
                    default,
                    Describe(authored) + ": " + accreditation.Error,
                    null);
        }

        /// <summary>
        /// THE resolution, now GENERIC (I-48 G4B).
        ///
        /// <para>
        /// Before this gate every binding wrote into the same local and the result landed on one hardcoded
        /// field, so no <c>PropertyId</c> could ever resolve anywhere else — what made that safe was the guard
        /// that admitted a single property, not the loop. Now each HEALTHY binding writes through ITS OWN
        /// descriptor, which is what lets two properties resolve independently.
        /// </para>
        /// <para>
        /// Two properties it must have, and both are verifiable: the outcome does NOT depend on the enumeration
        /// order of the persisted map, and resolving one property does NOT touch another's field.
        /// </para>
        /// <para>
        /// Nothing degrades. Missing target, unknown property, malformed reference and incompatible type all
        /// ABORT: there is no fall back to the stored literal, because a literal frozen when the binding was
        /// created is not the value in force.
        /// </para>
        /// <para>
        /// Fields are written only AFTER every binding is proven resolvable. Writing as the scan went would
        /// leave a half-applied design behind on the first failure.
        /// </para>
        /// </summary>
        internal SelectiveEffectiveResolution ResolveAgainst(
            SelectivePalletDesignDocument authored, UsableProjectVariablesRegistry registry)
            => ResolveWith(authored, registry, SelectiveLinkedProperties.All);

        /// <summary>
        /// The same resolution over an EXPLICIT descriptor set. Pure-function seam, not runtime extensibility
        /// (V4-R02/V4-R04): it is internal, it is never populated from configuration, and production has exactly
        /// one caller, which passes <see cref="SelectiveLinkedProperties.All"/>.
        ///
        /// <para>
        /// It exists because genericity is not observable through a catalogue of one. With a single registered
        /// property, "each binding writes through its own descriptor" and "every binding writes the vertical
        /// clearance" produce identical results — the field-level hardcode the reviews found would pass every
        /// test. Two synthetic properties over REAL fields separate the two, without registering a second
        /// productive property.
        /// </para>
        /// </summary>
        internal SelectiveEffectiveResolution ResolveWith(
            SelectivePalletDesignDocument authored,
            UsableProjectVariablesRegistry registry,
            LinkedPropertyDescriptorSet descriptors)
        {
            if (authored == null)
            {
                throw new ArgumentNullException(nameof(authored));
            }

            if (registry == null)
            {
                throw new ArgumentNullException(nameof(registry));
            }

            if (descriptors == null)
            {
                throw new ArgumentNullException(nameof(descriptors));
            }

            var inspections = SelectiveLinkedPropertyKernel.InspectBindings(authored, descriptors, registry);
            var rack = Describe(authored);

            foreach (var inspection in inspections)
            {
                if (!inspection.IsHealthy)
                {
                    return SelectiveEffectiveResolution.Failure(
                        OutcomeOf(inspection),
                        inspection.PropertyId,
                        rack + ", " + inspection.Detail,
                        inspection.RawVariableId);
                }
            }

            var design = authored.ToDomain();

            foreach (var inspection in inspections)
            {
                if (!descriptors.TryGetDescriptor(inspection.PropertyId, out var descriptor))
                {
                    // Unreachable: HEALTHY means the descriptor was found. Reaching it is an invariant
                    // violation, not a state to reinterpret.
                    return SelectiveEffectiveResolution.Failure(
                        SelectiveEffectiveOutcome.UnknownPropertyId,
                        inspection.PropertyId,
                        rack + ": la propiedad '" + inspection.PropertyId + "' resolvio sin descriptor.",
                        inspection.RawVariableId);
                }

                descriptor.WriteEffective(design, inspection.Target.LiteralValue);
            }

            return SelectiveEffectiveResolution.Success(design);
        }

        /// <summary>Maps an inspection failure onto the outcome vocabulary this resolver already published.</summary>
        private static SelectiveEffectiveOutcome OutcomeOf(BindingInspection inspection)
        {
            switch (inspection.Outcome)
            {
                case BindingInspectionOutcome.FatalUnknownProperty:
                    return SelectiveEffectiveOutcome.UnknownPropertyId;

                case BindingInspectionOutcome.FatalMalformedReference:
                    return inspection.Malformed == MalformedReferenceReason.UnknownKind
                        ? SelectiveEffectiveOutcome.UnknownReferenceKind
                        : SelectiveEffectiveOutcome.MalformedReference;

                default:
                    // Missing target and incompatible target are both "this reference yields no value".
                    return SelectiveEffectiveOutcome.BrokenProjectVariableReference;
            }
        }


        /// <summary>The rack, named the way a user can find it: by id, and by name when it has one.</summary>
        private static string Describe(SelectivePalletDesignDocument authored)
            => string.IsNullOrWhiteSpace(authored.Name)
                ? "El rack " + authored.Id
                : "El rack " + authored.Id + " (" + authored.Name + ")";
    }
}
