#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Nodes;

namespace RackCad.Tests
{
    /// <summary>
    /// MaterializationClose (AUTOMATION_PLAN 16.30; Proposal V14 §15; C-21): the close is the commit with the last change to the normative or template
    /// surfaces; a later change to any of them invalidates it, and a new close is required (for I-62, MC_I62: close again, reseed the fixture, repeat the
    /// affected pilots). The surfaces are the closed list of 16.13.
    /// </summary>
    public static class MaterializationClose
    {
        /// <summary>The surface paths changed after <paramref name="close"/> on the way to <paramref name="head"/> (empty = the close still holds).</summary>
        public static List<string> Invalidating(IGitHistory git, string close, string head)
        {
            return git.Range(close, head).SelectMany(git.ChangedPaths).Where(CompatibilityGuard.InSurfaces).Distinct(StringComparer.Ordinal)
                .OrderBy(p => p, StringComparer.Ordinal).ToList();
        }
    }

    /// <summary>
    /// The manual fallback of the orchestration (AUTOMATION_PLAN 16.29 and agent-execution README §18.7; Proposal V14 §20.9; C-37): every relay done by a
    /// human (the Owner or the Coordinator as transport) has an AUTONOMY_GAP record in <c>orchestration.autonomy_gaps[]</c> for its request; a manual
    /// relay without one makes the orchestration evidence invalid for criterion 15 (P-21). The transport audit of C-32 reads the same records.
    /// </summary>
    public static class AutonomyGaps
    {
        /// <summary>The logical requests with a manual relay that no custodied AUTONOMY_GAP record names (P-21).</summary>
        public static List<string> Missing(StatePoint point, IEnumerable<string> manuallyRelayedRequests)
        {
            var recorded = Y.L(point.State, "orchestration.autonomy_gaps").Select(r => StateTreeReader.Json(point.Tree, r))
                .Where(j => j != null && J.S(j, "Kind") == "AUTONOMY_GAP").Select(j => J.S(j, "LogicalReviewRequestId")).OfType<string>().ToHashSet(StringComparer.Ordinal);
            return manuallyRelayedRequests.Distinct(StringComparer.Ordinal).Where(r => !recorded.Contains(r)).ToList();
        }

        /// <summary>
        /// C-32 (V14 §20.9, FX-06): the Owner is the message bus of a sequence when some relay of it was done by the Owner, that is, some point custodies
        /// an AUTONOMY_GAP record with <c>RelayedBy</c> OWNER. Every manual relay has its record (P-21), so the audit reads only the custody; an unrecorded
        /// relay already invalidates the evidence (<see cref="Missing"/>).
        /// </summary>
        public static bool OwnerAsMessageBus(IEnumerable<StatePoint> points)
        {
            return points.Any(p => Y.L(p.State, "orchestration.autonomy_gaps").Select(r => StateTreeReader.Json(p.Tree, r))
                .Any(j => j != null && J.S(j, "Kind") == "AUTONOMY_GAP" && J.S(j, "RelayedBy") == "OWNER"));
        }

        /// <summary>An AUTONOMY_GAP record with the fields of README §18.7.</summary>
        public static JsonObject Record(string request, long round, string relayedBy, string medium, string artifactPath, string artifactBlob, string cause) => new JsonObject
        {
            ["Kind"] = "AUTONOMY_GAP", ["LogicalReviewRequestId"] = request, ["Round"] = round, ["RelayedBy"] = relayedBy, ["Medium"] = medium,
            ["Artifact"] = new JsonObject { ["Path"] = artifactPath, ["Blob"] = artifactBlob }, ["Cause"] = cause,
        };
    }
}
