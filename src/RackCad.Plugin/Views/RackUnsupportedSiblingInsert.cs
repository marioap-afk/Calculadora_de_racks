using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using RackCad.Application.Persistence;
using RackCad.Application.Systems.Shared;
using RackCad.Application.Views.Insertion;

namespace RackCad.Plugin.Views
{
    internal sealed class RackAuthorizedSiblingBatch
    {
        internal RackAuthorizedSiblingBatch(
            RackAuthoredInput authoredInput,
            System.Collections.Generic.IReadOnlyList<(ObjectId BlockId, RackEmbedDocument Embed)> blocks)
        {
            AuthoredInput = authoredInput;
            Blocks = blocks;
        }

        internal RackAuthoredInput AuthoredInput { get; }
        internal System.Collections.Generic.IReadOnlyList<(ObjectId BlockId, RackEmbedDocument Embed)> Blocks { get; }
    }

    /// <summary>I-55 consumer of the demonstrated I-58 AUTH-13 comparators.</summary>
    internal static class RackUnsupportedSiblingInsert
    {
        internal static bool TryAuthorize(Document document, ObjectId selected, RackEmbedDocument source, string rackId)
        {
            if (!TryAuthorize(document, selected, source, rackId, out var authorized)) return false;
            var comparison = CompareWithFoundationAuthority(source?.Kind, authorized.AuthoredInput);
            if (comparison.outcome == RackAuthoredComparisonOutcome.Single) return true;
            document.Editor.WriteMessage("\nRackCad: no se inserto ninguna vista: "
                + comparison.outcome + ": " + comparison.diagnostic);
            return false;
        }

        internal static bool TryAuthorize(
            Document document, ObjectId selected, RackEmbedDocument source, string rackId,
            out RackAuthorizedSiblingBatch authorized)
        {
            authorized = null;
            var originalIdentityIsAttributable = string.Equals(source?.Kind, RackEmbedDocument.KindCabecera,
                System.StringComparison.OrdinalIgnoreCase) && !string.IsNullOrEmpty(source?.Id);
            var snapshot = RackSiblingScan.Capture(document, selected, rackId, source?.Id,
                originalIdentityIsAttributable, _ => false);
            var properties = RackSiblingCustomPropertiesGate.Evaluate(snapshot.Membership, snapshot.Properties);
            if (!properties.Accepted)
            {
                document.Editor.WriteMessage("\nRackCad: no se inserto ninguna vista: " + properties.Diagnostic);
                return false;
            }

            var siblings = new System.Collections.Generic.List<RackAuthoredSibling>();
            var store = new RackEmbedStore();
            foreach (var member in snapshot.Membership.AuthoredGateMembers)
            {
                if (!snapshot.Envelopes.TryGetValue(member.Fact.DefinitionKey, out var envelope))
                {
                    document.Editor.WriteMessage("\nRackCad: no se inserto ninguna vista: AUTH-13: unreadable sibling envelope.");
                    return false;
                }
                siblings.Add(new RackAuthoredSibling(
                    member.Fact.DefinitionKey,
                    envelope.Kind,
                    store.Serialize(envelope),
                    envelope.Design));
            }
            var complete = true;
            foreach (var member in snapshot.Membership.Members)
                if (member.Kind == RackCad.Application.Views.Redraw.RackSiblingMembershipKind.BlockingUnreadable)
                    complete = false;
            var blocks = new System.Collections.Generic.List<(ObjectId BlockId, RackEmbedDocument Embed)>();
            foreach (var member in snapshot.Membership.Members)
                if (snapshot.Definitions.TryGetValue(member.Fact.DefinitionKey, out var definition)
                    && snapshot.Envelopes.TryGetValue(member.Fact.DefinitionKey, out var envelope))
                    blocks.Add((definition, envelope));
            if (blocks.Count == 0 && source != null) blocks.Add((selected, source));
            authorized = new RackAuthorizedSiblingBatch(
                new RackAuthoredInput(rackId, siblings, complete, "RackSiblingScan/Capture"), blocks);
            return true;
        }

        private static (RackAuthoredComparisonOutcome outcome, string diagnostic) CompareWithFoundationAuthority(
            string kind, RackAuthoredInput input)
        {
            switch (kind)
            {
                case RackEmbedDocument.KindDynamic:
                    return Outcome(RackAuthoredComparatorPorts.Dynamic().Compare(input));
                case RackEmbedDocument.KindPushBack:
                    return Outcome(RackAuthoredComparatorPorts.PushBack().Compare(input));
                case RackEmbedDocument.KindCantilever:
                    return Outcome(RackAuthoredComparatorPorts.Cantilever().Compare(input));
                case RackEmbedDocument.KindCabecera:
                    return Outcome(RackAuthoredComparatorPorts.Cabecera().Compare(input));
                default:
                    return (RackAuthoredComparisonOutcome.Unreadable,
                        "AUTH-13: tipo sin comparador demostrado.");
            }
        }

        private static (RackAuthoredComparisonOutcome outcome, string diagnostic) Outcome<T>(
            RackAuthoredComparisonResult<T> result) => (result.Outcome, result.Diagnostic);
    }
}
