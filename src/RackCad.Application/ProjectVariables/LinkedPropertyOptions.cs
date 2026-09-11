using System;
using System.Collections.Generic;
using System.Globalization;

namespace RackCad.Application.ProjectVariables
{
    /// <summary>
    /// One variable a property may be bound to, as a surface should PRESENT it (I-48 G4C, Proposal V2 R-06).
    ///
    /// <para>
    /// Three data with three different jobs, and mixing them up is the whole risk:
    /// <see cref="VariableId"/> is IDENTITY and the only thing a selection carries;
    /// <see cref="Name"/> is a label a person reads and text FILTERS against; <see cref="LiteralValue"/> is
    /// shown so a decision is informed. Nothing here ever supports the reverse lookup name → id.
    /// </para>
    /// <para>
    /// <see cref="Disambiguator"/> is MANDATORY and not decoration. Duplicate names are allowed, and two
    /// homonyms can even hold the same value, so neither the name nor the name plus the value is enough for a
    /// person to tell the options apart. Without a visible fragment of the identity, a list of two
    /// "Holgura General = 10" entries asks the user to pick blind.
    /// </para>
    /// </summary>
    public sealed class LinkedPropertyOption
    {
        public LinkedPropertyOption(VariableId variableId, string name, VariableType variableType, double literalValue)
        {
            if (variableId.IsEmpty)
            {
                throw new ArgumentException("Una opcion necesita un VariableId.", nameof(variableId));
            }

            VariableId = variableId;
            Name = name;
            VariableType = variableType;
            LiteralValue = literalValue;
        }

        /// <summary>The authority. A selection travels by this and by nothing else.</summary>
        public VariableId VariableId { get; }

        /// <summary>The label. NOT unique, and never identity.</summary>
        public string Name { get; }

        public VariableType VariableType { get; }

        public double LiteralValue { get; }

        /// <summary>
        /// A short, stable fragment of the identity, so two homonyms are distinguishable on screen. It is a
        /// PREFIX of the id: an abbreviation of the authority rather than a second name.
        /// </summary>
        public string Disambiguator
        {
            get
            {
                var value = VariableId.Value ?? string.Empty;
                return value.Length <= 8 ? value : value.Substring(0, 8);
            }
        }

        /// <summary>What a list shows: the name, the value, and the identity fragment that tells homonyms apart.</summary>
        public string DisplayText
            => (string.IsNullOrWhiteSpace(Name) ? "(sin nombre)" : Name)
               + " = " + LiteralValue.ToString("0.###", CultureInfo.InvariantCulture)
               + "  [" + Disambiguator + "]";

        public override string ToString() => DisplayText;
    }

    /// <summary>
    /// The options one linked property may be bound to, from an ACCREDITED register (I-48 G4C).
    ///
    /// <para>
    /// It reads the same accredited authority a resolution reads, so the list a user picks from and the value
    /// the drawing takes can never come from two different readings of the register. Before G4C this walked
    /// the raw document and parsed the persisted type with <c>Enum.TryParse</c> — which also accepts the
    /// numeric form and comma lists, so it offered entries the store's own grammar rejects.
    /// </para>
    /// <para>
    /// Compatibility is decided HERE, against the descriptor's <see cref="VariableType"/>, and never in a
    /// surface. A variable of a type the property cannot take is not offered at all: offering it would invite a
    /// binding that resolves to a number meaning something else.
    /// </para>
    /// </summary>
    public static class LinkedPropertyOptions
    {
        /// <summary>
        /// The compatible options for one property, ordered by identity so the list is reproducible. INTERNAL:
        /// it takes the accredited authority, which no surface may hold.
        /// </summary>
        internal static IReadOnlyList<LinkedPropertyOption> For(
            SelectiveLinkedPropertyDescriptor descriptor, UsableProjectVariablesRegistry registry)
        {
            if (descriptor == null)
            {
                throw new ArgumentNullException(nameof(descriptor));
            }

            if (registry == null)
            {
                throw new ArgumentNullException(nameof(registry));
            }

            var options = new List<LinkedPropertyOption>();

            foreach (var target in registry.Targets())
            {
                // Compatibility is the descriptor's requirement against the target's accredited type. Nothing
                // is re-parsed here: the type already survived the store's grammar and the accreditation.
                if (target.VariableType != descriptor.VariableType)
                {
                    continue;
                }

                options.Add(new LinkedPropertyOption(
                    target.VariableId, target.Name, target.VariableType, target.LiteralValue));
            }

            return options;
        }

        /// <summary>
        /// PRODUCTION entry point: accredits the read, then projects. A register that is not usable offers NO
        /// options and says why — it never degrades to an empty list, because "no variables" and "the register
        /// cannot be read" are different answers and only one of them invites creating the first variable.
        /// </summary>
        public static LinkedPropertyOptionsResult ForProperty(
            PropertyId propertyId, Persistence.ProjectVariablesReadResult read)
        {
            if (!SelectiveLinkedProperties.All.TryGetDescriptor(propertyId, out var descriptor))
            {
                return LinkedPropertyOptionsResult.Failed(
                    "La propiedad '" + propertyId + "' no es vinculable en esta version.");
            }

            var accreditation = UsableProjectVariablesRegistry.Accredit(read);

            return accreditation.IsUsable
                ? LinkedPropertyOptionsResult.Usable(For(descriptor, accreditation.Registry))
                : LinkedPropertyOptionsResult.Failed(accreditation.Error);
        }
    }

    /// <summary>The options, or the visible reason there are none.</summary>
    public sealed class LinkedPropertyOptionsResult
    {
        private LinkedPropertyOptionsResult(
            bool isUsable, IReadOnlyList<LinkedPropertyOption> options, string error)
        {
            IsUsable = isUsable;
            Options = options;
            Error = error;
        }

        public bool IsUsable { get; }

        public IReadOnlyList<LinkedPropertyOption> Options { get; }

        public string Error { get; }

        internal static LinkedPropertyOptionsResult Usable(IReadOnlyList<LinkedPropertyOption> options)
            => new LinkedPropertyOptionsResult(true, options, null);

        internal static LinkedPropertyOptionsResult Failed(string error)
            => new LinkedPropertyOptionsResult(false, new LinkedPropertyOption[0], error);
    }
}
