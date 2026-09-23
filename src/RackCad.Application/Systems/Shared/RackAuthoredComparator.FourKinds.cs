using System;
using System.Collections.Generic;
using System.Text.Json;
using RackCad.Application.Persistence;
using RackCad.Domain.RackFrames;
using RackCad.Domain.Systems.Dynamic;
using RackCad.Domain.Systems.PushBack;
using RackCad.Domain.Systems.Cantilever;

namespace RackCad.Application.Systems.Shared;

public static partial class RackAuthoredComparatorPorts
{
    public static IRackAuthoredComparatorPort<RackAuthoredInput, DynamicRackDesign> Dynamic()
        => new AuthoredComparator<DynamicRackSystemDocument, DynamicRackDesign>(RackEmbedDocument.KindDynamic,
            AuthoredCanonical.Dynamic, AuthoredTypedValues.Same, d => d.ToDesign());

    public static IRackAuthoredComparatorPort<RackAuthoredInput, PushBackDesign> PushBack()
        => new AuthoredComparator<PushBackDesignDocument, PushBackDesign>(RackEmbedDocument.KindPushBack,
            AuthoredCanonical.PushBack, AuthoredTypedValues.Same, d => d.ToDomain());

    public static IRackAuthoredComparatorPort<RackAuthoredInput, CantileverLineDesign> Cantilever()
        => new AuthoredComparator<CantileverLineDocument, CantileverLineDesign>(RackEmbedDocument.KindCantilever,
            AuthoredCanonical.Cantilever, AuthoredTypedValues.Same, d => AuthoredTypedValues.Copy(d.Line));

    public static IRackAuthoredComparatorPort<RackAuthoredInput, RackFrameConfiguration> Cabecera()
        => new AuthoredComparator<RackFrameProjectDocument, RackFrameConfiguration>(RackEmbedDocument.KindCabecera,
            AuthoredCanonical.Header, AuthoredTypedValues.Same, d => d.ToConfiguration());

    // Existing generic unsupported ports are deliberately unchanged. Only these four
    // concrete factories compose the private typed pipeline; it cannot cast arbitrary inputs or domains.
    private sealed class AuthoredComparator<TCanonical, TAuthored> : IRackAuthoredComparatorPort<RackAuthoredInput, TAuthored>
        where TCanonical : class
    {
        private readonly Func<TCanonical, TCanonical> normalize;
        private readonly Func<TCanonical, TCanonical, bool> same;
        private readonly Func<TCanonical, TAuthored> materialize;

        internal AuthoredComparator(string kind, Func<TCanonical, TCanonical> normalize,
            Func<TCanonical, TCanonical, bool> same, Func<TCanonical, TAuthored> materialize)
        {
            Kind = kind;
            this.normalize = normalize;
            this.same = same;
            this.materialize = materialize;
        }
        public string Kind { get; }

        public RackAuthoredComparisonResult<TAuthored> Compare(RackAuthoredInput input)
        {
            if (input == null || string.IsNullOrWhiteSpace(input.RackId) || !input.IsComplete ||
                string.IsNullOrWhiteSpace(input.CompletenessEvidence) || input.Siblings == null || input.Siblings.Count == 0)
                return RackAuthoredComparisonResult<TAuthored>.Unreadable("AUTH-13: incomplete sibling snapshot.");

            var sources = new HashSet<string>(StringComparer.Ordinal);
            var accredited = new List<(string Name, TCanonical Value)>();
            string error = null;
            foreach (var sibling in input.Siblings)
            {
                try
                {
                    if (sibling != null && !sources.Add(sibling.SourceIdentity))
                        throw AuthoredRawReader.Invalid("source", "duplicate identity");
                    var raw = AuthoredRawReader.Read(sibling, input.RackId, Kind);
                    var canonical = normalize(AuthoredRawReader.Decode<TCanonical>(raw.Payload));
                    accredited.Add((raw.Name, canonical));
                }
                catch (JsonException ex) { error ??= "AUTH-13 raw: " + ex.Message; }
            }
            if (error != null) return RackAuthoredComparisonResult<TAuthored>.Unreadable(error);

            // An anchor is used only after ALL sources passed the gate. It is never returned as authority.
            var anchor = accredited[0];
            for (int i = 1; i < accredited.Count; i++)
                if (!string.Equals(anchor.Name, accredited[i].Name, StringComparison.Ordinal) ||
                    !same(anchor.Value, accredited[i].Value))
                    return RackAuthoredComparisonResult<TAuthored>.Divergent("AUTH-13: distinct authored values.");

            // The accredited DTOs are private. Per-kind materializers construct fresh domains field by field,
            // after every mapper exclusion/repair has been either normalized by F-09 or rejected.
            return RackAuthoredComparisonResult<TAuthored>.Single(materialize(anchor.Value));
        }
    }
}
