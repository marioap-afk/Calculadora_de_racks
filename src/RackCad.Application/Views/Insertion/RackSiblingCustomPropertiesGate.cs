using System;
using System.Collections.Generic;
using RackCad.Application.CustomProperties;
using RackCad.Application.Persistence;
using RackCad.Application.Views.Redraw;

namespace RackCad.Application.Views.Insertion
{
    /// <summary>Product gate over exactly the mutable set already classified by sibling membership.</summary>
    public static class RackSiblingCustomPropertiesGate
    {
        public static RackInsertGateResult Evaluate(
            RackSiblingMembershipSnapshot membership,
            IReadOnlyDictionary<string, CustomPropertiesReadResult> collections)
        {
            if (membership == null) throw new ArgumentNullException(nameof(membership));
            if (collections == null) throw new ArgumentNullException(nameof(collections));

            string canonical = null;
            foreach (var member in membership.CustomPropertiesGateMembers)
            {
                if (!collections.TryGetValue(member.Fact.DefinitionKey, out var collection) || collection == null)
                    return RackInsertGateResult.Reject("CUSTOM_PROPERTIES_UNREADABLE: " + member.Fact.DefinitionKey);
                if (!collection.CanWrite)
                    return RackInsertGateResult.Reject("CUSTOM_PROPERTIES_READ_ONLY: " + member.Fact.DefinitionKey);

                var current = CustomPropertiesCanonicalForm.Of(collection);
                if (canonical == null) canonical = current;
                else if (!string.Equals(canonical, current, StringComparison.Ordinal))
                    return RackInsertGateResult.Reject(
                        "CUSTOM_PROPERTIES_DIVERGENT: usa RACKPROPIEDADES para unificar las vistas antes de insertar otra.");
            }

            return RackInsertGateResult.Proceed();
        }
    }
}
