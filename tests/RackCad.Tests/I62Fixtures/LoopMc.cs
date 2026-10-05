#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static RackCad.Tests.CustodyMc;
using static RackCad.Tests.OrchestrationSamples;
using static RackCad.Tests.StateV2Samples;

namespace RackCad.Tests
{
    /// <summary>
    /// The F.8 loop of the Architect written as real durable points of a disposable repository, shared by the C-29 and C-31 reproducible controls:
    /// real commits for the reviewed versions X v1 and X v2, for the authorization entries the materialized bindings cite, and for the custody SHAs.
    /// Test data and helpers only.
    /// </summary>
    public static class LoopMc
    {
        private const string Auth1 = "a1a1a1a1a1a1a1a1a1a1a1a1a1a1a1a1a1a1a1a1";
        private const string Auth2 = "a2a2a2a2a2a2a2a2a2a2a2a2a2a2a2a2a2a2a2a2";

        /// <summary>
        /// Writes the F.8 points on the holder's clone and pushes them. Returns the points read back from Git and the synthetic F.8 points (with the
        /// AuthorizationRef of each materialized binding patched), so that a caller can build further points with the same substitutions.
        /// </summary>
        public static (List<(StatePoint Point, string Commit)> Points, List<StatePoint> F8) WriteF8(CustodyRepo r)
        {
            var h = r.Holder;
            r.Subst[Sha] = r.Main0;
            r.Subst[DigitSha] = r.Main0;
            r.Subst[Sha3] = r.Commit(h, "T-01 RED", (RedFile, "rojo\n"));
            r.Subst[Sha2] = r.Commit(h, "T-01 GREEN", (GreenFile, "verde\n"));

            // The AuthorizationRef of each materialized binding cites the commit where its authorization was already custodied (never the custody point).
            var f8 = F8();
            foreach (var p in f8)
            {
                var tree = (InMemoryStateTree)p.Tree;
                foreach (var (path, bytes) in tree.Files.ToList().Where(f => f.Key.Contains("/review/B2026", StringComparison.Ordinal)))
                {
                    var token = path.Contains("-b001", StringComparison.Ordinal) ? Auth1 : Auth2;
                    tree.Put(path, Encoding.UTF8.GetString(bytes).Replace("\"Commit\": \"" + Sha + "\"", "\"Commit\": \"" + token + "\"", StringComparison.Ordinal));
                }
            }

            var points = new List<(StatePoint Point, string Commit)>();
            void Write(int i)
            {
                var k = r.WritePoint(h, f8[i], "I-99: F.8 punto " + i, push: false);
                points.Add((r.Read(h, k), k));
            }

            Write(0);
            ((InMemoryStateTree)f8[1].Tree).TryRead(OrchestrationSamples.Decisions, out var rla);
            r.G.Write(h, OrchestrationSamples.Decisions, Encoding.UTF8.GetString(rla));
            r.Subst[Auth1] = r.G.CommitAll(h, "I-99: ReviewLoopAuthorization RLA-1");
            r.G.Write(h, X, "# I-99 propuesta v1\n");
            r.Subst[C1] = r.G.CommitAll(h, "I-99: X v1");
            r.Subst[B1] = r.Git(h, "rev-parse", r.Subst[C1] + ":" + X);
            for (var i = 1; i <= 6; i++)
            {
                Write(i);
            }

            r.Subst[Auth2] = points[^1].Commit;
            r.G.Write(h, X, "# I-99 propuesta v2 (corrige LIN-1 y LIN-2)\n");
            r.Subst[C2] = r.G.CommitAll(h, "I-99: X v2");
            r.Subst[B2] = r.Git(h, "rev-parse", r.Subst[C2] + ":" + X);
            for (var i = 7; i < f8.Count; i++)
            {
                Write(i);
            }

            r.Git(h, "push", "-q", "origin", "HEAD");
            return (points, f8);
        }
    }
}
