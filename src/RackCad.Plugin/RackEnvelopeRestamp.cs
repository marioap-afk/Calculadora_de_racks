using System;
using RackCad.Application.Persistence;
using RackCad.Plugin.KindHandlers;

namespace RackCad.Plugin
{
    /// <summary>
    /// Shared Plugin helper (I-09 F4): re-stamp a rack payload for an INDEPENDENT copy — a fresh GUID + the copy's
    /// name, including the kind-specific inner design identity. Extracted verbatim from the former RackFrameCommands
    /// partial so RACKDUPLICAR and RACKLAYOUT share the one implementation. Behavior is unchanged.
    /// </summary>
    internal static class RackEnvelopeRestamp
    {
        /// <summary>Copy a rack payload with a FRESH GUID and the copy's name so it is an independent rack. The
        /// KIND-SPECIFIC design inside is re-stamped too (selective: Id+Name; cabecera: Header.Name) — otherwise the
        /// first RACKEDITAR on the copy would show and silently write back the ORIGINAL's name (its editor loads the
        /// name from the inner design). The caller guarantees <paramref name="payload"/> deserializes.</summary>
        public static string RestampEnvelope(string payload, string copyName)
        {
            var store = new RackEmbedStore();
            var embed = store.Deserialize(payload);
            embed.Id = System.Guid.NewGuid().ToString();
            embed.Name = copyName;
            embed.Design = RestampDesign(embed.Kind, embed.Design, embed.Id, copyName);
            return store.Serialize(embed);
        }

        /// <summary>Re-stamp the identity the kind-specific design carries, dispatching by kind via the kind-handler
        /// registry. Dynamic and cama designs hold no display identity of their own (their editors take the
        /// envelope's name), so their handlers are no-ops. The lookup is case-INSENSITIVE, as this consumer always
        /// was. Best effort: a kind with no handler, or an unreadable inner design, is returned untouched — the
        /// envelope-only restamp still applies.</summary>
        private static string RestampDesign(string kind, string designJson, string newId, string copyName)
        {
            if (string.IsNullOrEmpty(designJson))
            {
                return designJson;
            }

            if (!KindHandlerRegistry.Default.TryGetIgnoreCase(kind, out var handler))
            {
                return designJson;
            }

            try
            {
                return handler.RestampDesign(designJson, newId, copyName);
            }
            catch
            {
                // Best effort: keep the original design JSON; the copy still gets its own GUID/envelope name.
            }

            return designJson;
        }
    }
}
