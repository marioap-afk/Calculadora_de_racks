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
        /// Resolves the effective design.
        ///
        /// <para>
        /// A null <paramref name="projectVariables"/> means a drawing with no register, which is valid legacy
        /// (C-1) and resolves normally for an unbound rack. For a BOUND one it is a broken reference like any
        /// other: the variable it names does not exist.
        /// </para>
        /// </summary>
        public SelectiveEffectiveResolution Resolve(
            SelectivePalletDesignDocument authored,
            ProjectVariablesDocument projectVariables)
        {
            if (authored == null)
            {
                throw new ArgumentNullException(nameof(authored));
            }

            var variables = Index(projectVariables);
            var effectiveClearance = authored.VerticalClearance;

            if (authored.PropertyValues != null)
            {
                foreach (var binding in authored.PropertyValues)
                {
                    var failure = TryResolveBinding(authored, binding.Key, binding.Value, variables, out var value);

                    if (failure != null)
                    {
                        return failure;
                    }

                    effectiveClearance = value;
                }
            }

            var design = authored.ToDomain();
            design.VerticalClearance = effectiveClearance;

            return SelectiveEffectiveResolution.Success(design);
        }

        /// <summary>
        /// Resolves one binding, or explains why it cannot be resolved. Every branch that fails names the
        /// rack, the property and the variable: an abort the user cannot locate is not usable, and the only
        /// repair path asks them to pick that exact rack.
        /// </summary>
        private static SelectiveEffectiveResolution TryResolveBinding(
            SelectivePalletDesignDocument authored,
            string propertyToken,
            SelectivePropertyValueDocument reference,
            IReadOnlyDictionary<VariableId, ProjectVariable> variables,
            out double value)
        {
            value = 0.0;

            var rack = Describe(authored);

            if (!PropertyId.TryParse(propertyToken, out var propertyId) ||
                !ProjectPropertyIds.IsKnown(propertyId))
            {
                return SelectiveEffectiveResolution.Failure(
                    SelectiveEffectiveOutcome.UnknownPropertyId,
                    propertyId,
                    rack + " declara un vínculo sobre una propiedad que esta versión no conoce ('" +
                    (propertyToken ?? "<null>") + "').",
                    reference?.VariableId);
            }

            if (reference == null ||
                !string.Equals(reference.Kind, SelectivePropertyValueDocument.ProjectVariableKind, StringComparison.Ordinal))
            {
                return SelectiveEffectiveResolution.Failure(
                    SelectiveEffectiveOutcome.UnknownReferenceKind,
                    propertyId,
                    rack + ", propiedad '" + propertyId + "': el vínculo es de una clase que esta versión no " +
                    "conoce ('" + (reference?.Kind ?? "<null>") + "').",
                    reference?.VariableId);
            }

            if (!VariableId.TryParse(reference.VariableId, out var variableId))
            {
                return SelectiveEffectiveResolution.Failure(
                    SelectiveEffectiveOutcome.MalformedReference,
                    propertyId,
                    rack + ", propiedad '" + propertyId + "': el vínculo apunta a un id de variable " +
                    "ilegible ('" + (reference.VariableId ?? "<null>") + "').",
                    reference.VariableId);
            }

            if (!variables.TryGetValue(variableId, out var variable))
            {
                return SelectiveEffectiveResolution.Failure(
                    SelectiveEffectiveOutcome.BrokenProjectVariableReference,
                    propertyId,
                    rack + ", propiedad '" + propertyId + "': la variable de proyecto '" + variableId +
                    "' no existe en este dibujo. No hay valor efectivo que aplicar.",
                    reference.VariableId);
            }

            value = variable.Definition.LiteralValue;
            return null;
        }

        /// <summary>The rack, named the way a user can find it: by id, and by name when it has one.</summary>
        private static string Describe(SelectivePalletDesignDocument authored)
            => string.IsNullOrWhiteSpace(authored.Name)
                ? "El rack " + authored.Id
                : "El rack " + authored.Id + " (" + authored.Name + ")";

        /// <summary>
        /// The register as a lookup. A null register is an EMPTY one — a drawing with zero variables is valid
        /// legacy, not a failure. What is a failure is a rack that references one of them.
        /// </summary>
        private static IReadOnlyDictionary<VariableId, ProjectVariable> Index(ProjectVariablesDocument document)
        {
            var index = new Dictionary<VariableId, ProjectVariable>();

            if (document == null)
            {
                return index;
            }

            foreach (var variable in document.ToProjectVariables())
            {
                index[variable.Id] = variable;
            }

            return index;
        }
    }
}
