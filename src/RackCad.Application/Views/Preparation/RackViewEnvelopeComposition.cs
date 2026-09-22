using System;
using RackCad.Application.Persistence;
using RackCad.Application.Systems.Shared;
using RackCad.Domain.Systems.Shared;

namespace RackCad.Application.Views.Preparation
{
    public static class RackViewEnvelopeComposition
    {
        public static RackEmbedDocument Compose<TPayload>(
            RackEmbedDocument source,
            string rackId,
            string rackName,
            string serializedDesign,
            RackSystemKind systemKind,
            RackPreparedView<TPayload> prepared)
        {
            if (prepared == null) throw new ArgumentNullException(nameof(prepared));
            if (string.IsNullOrWhiteSpace(rackId)) throw new ArgumentException("A prepared view needs a rack identity.", nameof(rackId));

            var syntax = RackViewCodec.Encode(systemKind, prepared.Address);
            if (!string.Equals(syntax.Kind, prepared.Kind, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("The prepared view kind does not match its canonical address.");
            }

            return RackEmbedComposer.Compose(
                source,
                syntax.Kind,
                rackId,
                rackName,
                syntax.View,
                syntax.Section,
                serializedDesign);
        }
    }
}
