using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>
    /// I-64 F4B PB-T4a — the GUARDS of the path of the full read (CT-PBT4A-05, 06 and 08). They are GUARDS: they pass on the skeleton
    /// and on the implementation, and they discriminate through their negative self-tests, run over text injected in memory (never
    /// written).
    ///
    /// <para>
    /// CT-PBT4A-05: a reader without the new interface is read as today. CT-PBT4A-06: the census of the types of the full model,
    /// which replaces CT-PBT3A-12 and admits the adapter of the Plugin. CT-PBT4A-08: G-L1 of D2 (CT-PBT2A-04) and the purity of
    /// the folder (CT-PBT3A-14), over the new file of the folder, <c>PushBackOriginByText.cs</c>.
    /// </para>
    /// </summary>
    public sealed class FullPanelCopyPushBackFullReadGuardRedTests
    {

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

        // ------------------------------------------------------------------------------------------------- CT-05

        [Fact]
        public void CT_PBT4A_05_WithAReaderWithoutTheNewInterfaceTheFiveRereadsUseReadIntentAsToday()
        {
            var log = new List<string>();
            var expected = new List<string>();
            foreach (var operation in PushBackFullReadKit.Operations)
            {
                expected.Add(operation + " lector=ReadIntent");
                // I-64 F4B PB-T4b (owner-pbt4b-list-477-decision.md): a reader without the new interface that answers with the full read too
                // (FromReads), as the reader of the Plugin does; the draft of the panel is the full one, so the rig finds the full draft.
                var reader = new FullEditIntentFakeReader();
                var rig = new FullReadRig(reader);
                var draft = Paso(log, () => rig.Begin());
                if (draft == null) continue;

                // The reading is armed right after its last gesture: the calls of the opening are not counted.
                var before = reader.Calls.Count;
                Paso(log, () => rig.Run(operation, draft));
                var reads = reader.Calls.Count - before;
                log.Add(operation + (reads == 1 ? " lector=ReadIntent" : " lector=" + reads + " lecturas"));
            }

            Assert.Equal(string.Join("\n", expected), string.Join("\n", log));
        }
    }
}
