using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using RackCad.Application.Persistence;
using RackCad.Application.Systems.PushBack.Draft;
using RackCad.Application.Systems.PushBack.Pilot;
using RackCad.Application.Workspace;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>
    /// I-64 F4B PB-T4a — the path of the full read (CT-PBT4A-01 to 04 and 07): the result of the read carries the full model and the
    /// catalog next to the one of the pilot (<c>FromReads</c>), the five rereads of an open draft go to the origin of the draft by its
    /// text when the reader offers it (A-T3-8), the choice by text and the reread at it are pure, and the Plugin does it over the same
    /// scan. No AutoCAD: the doubles of the host, the pure readers and the text of the Plugin, read as text. A step that throws becomes
    /// an <c>EXCEPCION</c> line and never escapes: the CT fails by its one assertion.
    /// </summary>
    public sealed class FullPanelCopyPushBackFullReadPathRedTests
    {

        // ---------------------------------------------------------------------------------------------- helpers

        /// <summary>The «EXCEPCION» rule: a step that throws becomes a line, and the expected lines never contain one.</summary>
        private static T Paso<T>(List<string> log, Func<T> step)
        {
            try
            {
                return step();
            }
            catch (Exception error)
            {
                log.Add("EXCEPCION " + error.GetType().Name + ": " + error.Message);
                return default(T);
            }
        }

        private static void Paso(List<string> log, Action step) => Paso<int>(log, () => { step(); return 0; });

        private static int Count(string text, string pattern) => Regex.Matches(text, pattern).Count;

        // ------------------------------------------------------------------------------------------------- CT-03

        [Fact]
        public void CT_PBT4A_03_TheFiveRereadsOfAnOpenDraftReadAtTheOriginOfTheDraftAndBeginReadsAsToday()
        {
            var log = new List<string>();

            var opening = new PushBackOriginReaderDouble();
            // I-64 F4B PB-T4b (owner-pbt4b-list-477-decision.md): the draft of the panel is the full one, so the rig finds the full draft.
            Paso(log, () => new FullReadRig(opening).Begin());
            log.Add("Begin lector=" + (opening.Kinds.Count == 1 ? opening.Kinds[0] : opening.Kinds.Count + " lecturas"));

            var expected = new List<string> { "Begin lector=ReadIntent" };
            foreach (var operation in PushBackFullReadKit.Operations)
            {
                expected.Add(operation + " lector=ReadIntentAtOrigin origen-del-borrador=True");
                var reader = new PushBackOriginReaderDouble();
                var rig = new FullReadRig(reader);
                var draft = Paso(log, () => rig.Begin());
                if (draft == null) continue;

                // The reading is armed right after its last gesture: the origin of the draft is taken before the reread.
                var origin = draft.CurrentDraftState.Origin;
                var before = reader.Kinds.Count;
                Paso(log, () => rig.Run(operation, draft));
                var kinds = reader.Kinds.Skip(before).ToList();
                if (kinds.Count != 1)
                {
                    log.Add(operation + " lector=" + kinds.Count + " lecturas");
                    continue;
                }

                var received = reader.Origins[before];
                log.Add(operation + " lector=" + kinds[0] + (received == null ? string.Empty : " origen-del-borrador=" + string.Equals(received, origin, StringComparison.Ordinal)));
            }

            Assert.Equal(string.Join("\n", expected), string.Join("\n", log));
        }
    }
}
