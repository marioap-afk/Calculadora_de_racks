using System;
using RackCad.Application.Diagnostics;
using RackCad.Application.Persistence;
using RackCad.Plugin.KindHandlers;

namespace RackCad.Plugin
{
    /// <summary>
    /// Shared Plugin helper (I-09 F4): re-stamp a rack payload for an INDEPENDENT copy — a fresh GUID + the copy's
    /// name, including the kind-specific inner design identity. RACKDUPLICAR and RACKLAYOUT share the one
    /// implementation. A kind with NO registered handler throws rather than silently producing a copy with a
    /// possibly-inconsistent inner identity (both commands gate on the handler first, for a clean visible message).
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
        /// registry (case-INSENSITIVE, as this consumer always was). Dynamic and cama designs hold no display
        /// identity of their own, so their handlers are no-ops. A kind with NO handler THROWS: an independent copy
        /// must never be produced with a possibly-inconsistent inner identity (RACKDUPLICAR/RACKLAYOUT gate on the
        /// handler first, so this is a defense-in-depth invariant). A readable design whose store round-trip fails
        /// is best-effort left untouched — the envelope-only restamp still applies.</summary>
        private static string RestampDesign(string kind, string designJson, string newId, string copyName)
        {
            if (string.IsNullOrEmpty(designJson))
            {
                return designJson;
            }

            if (!KindHandlerRegistry.Default.TryGetIgnoreCase(kind, out var handler))
            {
                throw new InvalidOperationException(KindDispatchMessages.NotRecognized(kind));
            }

            try
            {
                return handler.RestampDesign(designJson, newId, copyName);
            }
            catch (Exception ex)
            {
                // Best effort for a readable design whose store round-trip fails: keep the original JSON; the copy
                // still gets its own GUID/envelope name. (Distinct from the missing-handler case above, which throws.)
                RackLog.Exception("Re-estampar diseño interior de copia", ex);
            }

            return designJson;
        }
    }
}
