using System;
using RackCad.Application.Persistence;
using RackCad.Plugin.KindHandlers;

namespace RackCad.Plugin
{
    /// <summary>
    /// Shared Plugin helper (I-09 F4): re-stamp a rack payload for an INDEPENDENT copy — a fresh GUID + the copy's
    /// name, including the kind-specific inner design identity. RACKDUPLICAR and RACKLAYOUT share the one
    /// implementation. A kind with NO registered handler throws rather than silently producing a copy with a
    /// possibly-inconsistent inner identity (both commands gate on the handler first, for a clean visible message).
    ///
    /// <para>
    /// I-47 G14 — <b>una duplicación es indivisible</b>. Este helper tenía un <c>catch</c> que, ante un
    /// re-estampado fallido, devolvía el JSON ORIGINAL: la copia salía con un RackId nuevo en el sobre y la
    /// identidad vieja dentro. Dos racks con identidad semántica mezclada, y el defecto solo se ve la primera
    /// vez que alguien abre la copia y guarda — momento en el que ya no hay forma de saber cuál era cuál. Con
    /// vínculos de proyecto encima, esa copia arrastra además un binding cuyo dueño ya no es quien dice ser.
    /// </para>
    /// <para>
    /// Así que el fallo es un VALOR y el llamador tiene que mirarlo ANTES de materializar la copia. No hay
    /// mejor esfuerzo: o sale entera, o no sale ninguna.
    /// </para>
    /// <para>
    /// I-51 G4 — <b>la identidad nueva puede venir del llamador</b>. Duplicar varias vistas de un mismo rack
    /// exige que todas nazcan con el MISMO id, y este helper no ve el lote: por eso la entrada con
    /// <see cref="Guid"/> es la ÚNICA implementación, y la firma histórica de dos argumentos solo delega en ella
    /// con un GUID nuevo, de modo que sus consumidores no cambian.
    /// </para>
    /// </summary>
    internal static class RackEnvelopeRestamp
    {
        /// <summary>Copy a rack payload with a FRESH GUID and the copy's name so it is an independent rack. The
        /// historic entry point: it only generates the identity and delegates to the single implementation.</summary>
        public static RestampResult RestampEnvelope(string payload, string copyName)
            => RestampEnvelope(payload, copyName, Guid.NewGuid());

        /// <summary>
        /// Copy a rack payload with the identity <paramref name="newId"/> and the copy's name so it is an independent
        /// rack. The KIND-SPECIFIC design inside is re-stamped too (selective: Id+Name; cabecera: Header.Name) —
        /// otherwise the first RACKEDITAR on the copy would show and silently write back the ORIGINAL's name (its
        /// editor loads the name from the inner design). The caller must check the result BEFORE it writes anything
        /// (I-47 G14).
        ///
        /// <para>
        /// The identity is converted to text ONCE and that very text goes to the envelope and to the inner restamp,
        /// so both halves always agree (a kind that parses the id, like Cantilever, would otherwise be free to
        /// disagree). An empty identity, one equal to the source's own id, and an envelope that cannot be read are
        /// failures: none of them can produce an independent copy.
        /// </para>
        /// </summary>
        public static RestampResult RestampEnvelope(string payload, string copyName, Guid newId)
        {
            if (newId == Guid.Empty)
            {
                return RestampResult.Failure("La copia necesita una identidad nueva y recibio un GUID vacio.");
            }

            var store = new RackEmbedStore();
            var embed = store.Deserialize(payload);

            if (embed == null)
            {
                // The drawing-wide scan tolerates an envelope it cannot read; an independent copy cannot be built on one.
                return RestampResult.Failure("Los datos del rack de origen no se pueden leer: no se crea la copia.");
            }

            var newIdText = newId.ToString();

            if (string.Equals(embed.Id, newIdText, StringComparison.OrdinalIgnoreCase))
            {
                return RestampResult.Failure("La identidad nueva coincide con la del rack de origen: no se crea la copia.");
            }

            embed.Id = newIdText;
            embed.Name = copyName;

            var design = RestampDesign(embed.Kind, embed.Design, newIdText, copyName);

            if (!design.IsSuccess)
            {
                // The envelope was already given a new identity in memory; it never reaches the drawing.
                return design;
            }

            embed.Design = design.DesignJson;
            return RestampResult.Success(store.Serialize(embed));
        }

        /// <summary>Re-stamp the identity the kind-specific design carries, dispatching by kind via the kind-handler
        /// registry (case-INSENSITIVE, as this consumer always was). Dynamic and cama designs hold no display
        /// identity of their own, so their handlers are no-ops. A kind with NO handler THROWS: an independent copy
        /// must never be produced with a possibly-inconsistent inner identity (RACKDUPLICAR/RACKLAYOUT gate on the
        /// handler first, so this is a defense-in-depth invariant). A kind that CANNOT re-stamp its design FAILS
        /// (I-47 G14) — the envelope-only copy that used to come out of here is exactly the mixed identity being
        /// prevented.</summary>
        private static RestampResult RestampDesign(string kind, string designJson, string newId, string copyName)
        {
            if (string.IsNullOrEmpty(designJson))
            {
                // Nothing to re-stamp is not a failure: a kind may legitimately carry no inner design.
                return RestampResult.Success(designJson);
            }

            if (!KindHandlerRegistry.Default.TryGetIgnoreCase(kind, out var handler))
            {
                throw new InvalidOperationException(KindDispatchMessages.NotRecognized(kind));
            }

            return handler.RestampDesign(designJson, newId, copyName);
        }
    }
}
