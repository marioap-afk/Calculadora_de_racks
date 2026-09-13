using System;

namespace RackCad.Application.CustomProperties
{
    /// <summary>What the user asked to do to a collection of custom properties (I-54 D-10.1).</summary>
    public enum CustomPropertiesIntentKind
    {
        Create = 1,

        Rename = 2,

        ChangeValue = 3,

        /// <summary>Removes one entry. Removing the last one empties the collection; the container itself stays.</summary>
        Delete = 4,
    }

    /// <summary>
    /// One request, addressed by IDENTITY (I-54 D-05.3, D-10 / ADR-0039 §1).
    ///
    /// <para>
    /// The name never addresses anything: it is the one field a user edits freely, so an operation routed by name would
    /// move the moment somebody renamed an entry. That is why every factory that acts on an existing entry takes a
    /// <see cref="CustomPropertyId"/>, and refuses an empty one.
    /// </para>
    /// <para>
    /// <see cref="Create"/> carries no id on purpose: minting one is the mutation's job, so no caller can invent an identity
    /// the collection never agreed to.
    /// </para>
    /// </summary>
    public sealed class CustomPropertiesIntent
    {
        private CustomPropertiesIntent(CustomPropertiesIntentKind kind, CustomPropertyId id, string name, string value)
        {
            Kind = kind;
            Id = id;
            Name = name;
            Value = value;
        }

        public CustomPropertiesIntentKind Kind { get; }

        /// <summary>The entry the operation is about. Empty for <see cref="Create"/>.</summary>
        public CustomPropertyId Id { get; }

        /// <summary>The requested name, for <see cref="Create"/> and <see cref="Rename"/>. Validated by the mutation, not here.</summary>
        public string Name { get; }

        /// <summary>The requested value, for <see cref="Create"/> and <see cref="ChangeValue"/>. Validated by the mutation, not here.</summary>
        public string Value { get; }

        public static CustomPropertiesIntent Create(string name, string value)
            => new CustomPropertiesIntent(CustomPropertiesIntentKind.Create, default, name, value);

        public static CustomPropertiesIntent Rename(CustomPropertyId id, string name)
            => new CustomPropertiesIntent(CustomPropertiesIntentKind.Rename, RequireId(id), name, null);

        public static CustomPropertiesIntent ChangeValue(CustomPropertyId id, string value)
            => new CustomPropertiesIntent(CustomPropertiesIntentKind.ChangeValue, RequireId(id), null, value);

        public static CustomPropertiesIntent Delete(CustomPropertyId id)
            => new CustomPropertiesIntent(CustomPropertiesIntentKind.Delete, RequireId(id), null, null);

        private static CustomPropertyId RequireId(CustomPropertyId id)
        {
            if (id.IsEmpty)
            {
                throw new ArgumentException(
                    "Una operación sobre una propiedad personalizada existente exige su id.", nameof(id));
            }

            return id;
        }
    }
}
