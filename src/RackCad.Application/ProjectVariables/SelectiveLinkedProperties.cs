using System;
using System.Collections.Generic;
using RackCad.Application.Persistence;
using RackCad.Domain.Systems.Selective;

namespace RackCad.Application.ProjectVariables
{
    /// <summary>
    /// What a Selective property must declare to be bindable (I-48 G4A, Proposal V8 R-02/V6-R02).
    ///
    /// <para>
    /// It is deliberately NOT a bag of behaviour. It declares identity and the type it EXPECTS of a variable,
    /// and nothing else: no WPF label, no AutoCAD, no parsing, no formula, no arbitrary business callback. A
    /// descriptor that accepted a general delegate would BE the service locator the design says it is not.
    /// </para>
    /// <para>
    /// The four accessors are MECHANICAL: each moves exactly one value. They decide nothing — not whether a
    /// reference exists, not which <see cref="VariableId"/> wins, not compatibility, not repair, not
    /// Link/Unlink and not any fallback. Every one of those decisions belongs to the kernel, and that split is
    /// what keeps this type from becoming a bag of behaviour (I-48 G4B).
    /// </para>
    /// </summary>
    internal sealed class SelectiveLinkedPropertyDescriptor
    {
        internal SelectiveLinkedPropertyDescriptor(
            PropertyId propertyId,
            VariableType variableType,
            Func<SelectivePalletDesignDocument, double> readAuthored,
            Action<SelectivePalletDesignDocument, double> writeAuthored,
            Func<SelectivePalletDesign, double> readEffective,
            Action<SelectivePalletDesign, double> writeEffective)
        {
            if (propertyId.IsEmpty)
            {
                throw new ArgumentException(
                    "Un descriptor de propiedad vinculable necesita un PropertyId.", nameof(propertyId));
            }

            PropertyId = propertyId;
            VariableType = variableType;
            _readAuthored = readAuthored ?? throw new ArgumentNullException(nameof(readAuthored));
            _writeAuthored = writeAuthored ?? throw new ArgumentNullException(nameof(writeAuthored));
            _readEffective = readEffective ?? throw new ArgumentNullException(nameof(readEffective));
            _writeEffective = writeEffective ?? throw new ArgumentNullException(nameof(writeEffective));
        }

        private readonly Func<SelectivePalletDesignDocument, double> _readAuthored;
        private readonly Action<SelectivePalletDesignDocument, double> _writeAuthored;
        private readonly Func<SelectivePalletDesign, double> _readEffective;
        private readonly Action<SelectivePalletDesign, double> _writeEffective;

        /// <summary>The persisted identity of the property. Compared Ordinal, like <see cref="ProjectVariables.PropertyId"/>.</summary>
        internal PropertyId PropertyId { get; }

        /// <summary>The type this property REQUIRES of the variable that governs it.</summary>
        internal VariableType VariableType { get; }

        /// <summary>The stored literal, verbatim. While a binding exists this is the FROZEN value.</summary>
        internal double ReadAuthored(SelectivePalletDesignDocument authored) => _readAuthored(authored);

        /// <summary>Writes the stored literal. Freezing and materialising decide WHEN; this only moves the value.</summary>
        internal void WriteAuthored(SelectivePalletDesignDocument authored, double value) => _writeAuthored(authored, value);

        /// <summary>The effective field of the domain design this property governs.</summary>
        internal double ReadEffective(SelectivePalletDesign design) => _readEffective(design);

        /// <summary>Writes the effective field. The resolver decides WHAT; this only puts it in the right place.</summary>
        internal void WriteEffective(SelectivePalletDesign design, double value) => _writeEffective(design, value);
    }

    /// <summary>
    /// A descriptor set whose <see cref="PropertyId"/> keys are proven unique before anything is inspected.
    ///
    /// <para>
    /// Uniqueness is a PRECONDITION, not a lookup policy: with a duplicated key the lookup would have to pick,
    /// and picking would be first-wins or last-wins — exactly the arbitrary resolution this initiative removes
    /// one level up for <see cref="VariableId"/>. Comparison is Ordinal and case-sensitive: no normalization,
    /// no case folding, no aliases (ADR-0034 §2, and the asymmetry documented in <see cref="PropertyId"/>).
    /// </para>
    /// <para>
    /// The same invariant governs the closed production catalogue and any synthetic set a kernel test builds.
    /// Accepting a set as a parameter is a pure-function seam, not runtime extensibility: production always
    /// passes <see cref="SelectiveLinkedProperties.All"/>.
    /// </para>
    /// </summary>
    internal sealed class LinkedPropertyDescriptorSet
    {
        private readonly Dictionary<PropertyId, SelectiveLinkedPropertyDescriptor> _byProperty;

        private LinkedPropertyDescriptorSet(Dictionary<PropertyId, SelectiveLinkedPropertyDescriptor> byProperty)
        {
            _byProperty = byProperty;
        }

        internal int Count => _byProperty.Count;

        /// <summary>
        /// Builds the set, or explains why it is not a valid configuration. Returns false — it does not throw —
        /// so a caller can report the reason instead of crashing, and so an invalid synthetic fixture fails
        /// loudly in the test that built it.
        /// </summary>
        internal static bool TryCreate(
            IEnumerable<SelectiveLinkedPropertyDescriptor> descriptors,
            out LinkedPropertyDescriptorSet set,
            out string error)
        {
            set = null;
            error = null;

            if (descriptors == null)
            {
                error = "No se entrego un conjunto de descriptores de propiedad vinculable.";
                return false;
            }

            var byProperty = new Dictionary<PropertyId, SelectiveLinkedPropertyDescriptor>();

            foreach (var descriptor in descriptors)
            {
                if (descriptor == null)
                {
                    error = "El conjunto de descriptores contiene una entrada vacia.";
                    return false;
                }

                if (byProperty.ContainsKey(descriptor.PropertyId))
                {
                    error = "El conjunto de descriptores declara dos veces la propiedad '" +
                            descriptor.PropertyId + "'. Un PropertyId duplicado no es configuracion valida.";
                    return false;
                }

                byProperty.Add(descriptor.PropertyId, descriptor);
            }

            set = new LinkedPropertyDescriptorSet(byProperty);
            return true;
        }

        /// <summary>The descriptor for a property, when this set knows it. A miss is what makes a token UNKNOWN.</summary>
        internal bool TryGetDescriptor(PropertyId propertyId, out SelectiveLinkedPropertyDescriptor descriptor)
            => _byProperty.TryGetValue(propertyId, out descriptor);

        internal bool Contains(PropertyId propertyId) => _byProperty.ContainsKey(propertyId);

        /// <summary>
        /// The descriptors, ordered Ordinal by <see cref="PropertyId"/>. Deterministic on purpose: anything a
        /// caller derives from the catalogue — a scan, a diagnostic, a plan — must not depend on the
        /// enumeration order of a Dictionary.
        /// </summary>
        internal IReadOnlyList<SelectiveLinkedPropertyDescriptor> Ordered()
        {
            var ordered = new List<SelectiveLinkedPropertyDescriptor>(_byProperty.Values);
            ordered.Sort(static (left, right) =>
                string.Compare(left.PropertyId.Value, right.PropertyId.Value, StringComparison.Ordinal));
            return ordered;
        }
    }

    /// <summary>
    /// The CLOSED, static catalogue of Selective properties this build can bind.
    ///
    /// <para>
    /// Closed and static is the whole guarantee: no dependency injection, no service lookup, no runtime
    /// registration, no configuration or CSV, no reflection discovery, no mutable registry. Passing a
    /// descriptor — or this set — as a value to a pure helper is not any of those; asking a container for one
    /// would be.
    /// </para>
    /// <para>
    /// It holds exactly ONE property until the proof gate adds the second. That is not a placeholder: the
    /// guard that keeps a second property from being resolved, materialised or frozen incorrectly is the
    /// catalogue itself, so it is the LAST thing that grows.
    /// </para>
    /// </summary>
    internal static class SelectiveLinkedProperties
    {
        private static readonly LinkedPropertyDescriptorSet Catalogue = BuildCatalogue();

        /// <summary>The production authority. Every productive entry point passes THIS set and no other.</summary>
        internal static LinkedPropertyDescriptorSet All => Catalogue;

        /// <summary>True when the production catalogue declares <paramref name="propertyId"/> as bindable.</summary>
        internal static bool IsKnown(PropertyId propertyId) => Catalogue.Contains(propertyId);

        private static LinkedPropertyDescriptorSet BuildCatalogue()
        {
            var descriptors = new[]
            {
                // Mechanical, one field each. A reviewer must be able to SEE that these move a value and
                // decide nothing: no existence check, no compatibility, no fallback, no repair.
                new SelectiveLinkedPropertyDescriptor(
                    ProjectPropertyIds.SelectiveVerticalClearance,
                    VariableType.Length,
                    authored => authored.VerticalClearance,
                    (authored, value) => authored.VerticalClearance = value,
                    design => design.VerticalClearance,
                    (design, value) => design.VerticalClearance = value),
            };

            if (!TryCreate(descriptors, out var set, out var error))
            {
                // Unreachable with a literal, unique table. Reaching it means the catalogue itself is
                // malformed, which is an invariant violation and not something to resolve silently.
                throw new InvalidOperationException(
                    "El catalogo de propiedades vinculables del Selectivo es invalido: " + error);
            }

            return set;
        }

        private static bool TryCreate(
            IEnumerable<SelectiveLinkedPropertyDescriptor> descriptors,
            out LinkedPropertyDescriptorSet set,
            out string error)
            => LinkedPropertyDescriptorSet.TryCreate(descriptors, out set, out error);
    }
}
