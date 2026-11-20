using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Catalogs;
using RackCad.Application.Systems.PushBack.Draft;
using RackCad.Application.Systems.PushBack.Pilot;
using RackCad.Application.Workspace;
using RackCad.Application.Workspace.Drafts;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>
    /// I-64 F4B PB-T4a — the kit of the reads of the full model (D-PBT4-02, D-PBT4-03). The lectures of the pilot and of the full
    /// model over ONE scan of <c>IntentKit</c>, joined in the result of the read (<c>FromReads</c>), and a reader that offers the
    /// read at the origin of an open draft and records what it receives. The existing kits are used, never edited.
    /// </summary>
    internal static class PushBackFullReadKit
    {
        /// <summary>The five rereads of an open draft, in the order of the package (D-PBT4-03).</summary>
        public static readonly string[] Operations = { "CheckNow", "Reload", "ConfirmBase", "KeepMine", "DiscardAndReload" };

        /// <summary>The two pure reads over the same scan, joined by <c>FromReads</c>, as the adapter of the Plugin joins them.</summary>
        public static EditIntentReadResult Reads(PushBackPilotIntentScan scan)
        {
            var catalog = PilotKit.Catalog;
            return EditIntentReadResult.FromReads(PushBackPilotIntentReader.Read(scan, catalog), PushBackFullDraftReader.Read(scan, catalog), catalog);
        }

        /// <summary>The answer of the readers of these tests: the admitted read of two views of the rack, through <c>FromReads</c>.</summary>
        public static EditIntentReadResult Admitted(string rackId) => Reads(EditIntentKit.TwoViews(rackId));

        /// <summary>The same base in the two reads: the entries, key by key, and the text of the variables.</summary>
        public static bool SameBase(DraftStaleBase a, DraftStaleBase b)
        {
            if (a == null || b == null) return ReferenceEquals(a, b);
            if (a.Entries.Count != b.Entries.Count) return false;
            foreach (var pair in a.Entries)
            {
                if (!b.Entries.TryGetValue(pair.Key, out var other) || other != pair.Value) return false;
            }

            return string.Equals(a.VariableRegistryText, b.VariableRegistryText, StringComparison.Ordinal);
        }
    }

    /// <summary>
    /// A reader of the edit intention that offers the read at the origin of an open draft. It records the kind of every call and the
    /// text of the origin it receives, and it answers through <c>FromReads</c> like the adapter of the Plugin.
    /// </summary>
    internal sealed class PushBackOriginReaderDouble : IWorkspaceEditIntentReader, IWorkspaceEditIntentOriginReader
    {
        private readonly EditIntentFakeReader _inner = new EditIntentFakeReader { Answer = PushBackFullReadKit.Admitted };

        /// <summary>The kind of each call: <c>ReadIntent</c> or <c>ReadIntentAtOrigin</c>.</summary>
        public List<string> Kinds { get; } = new List<string>();

        /// <summary>The text of the origin of each call; null for <c>ReadIntent</c>.</summary>
        public List<string> Origins { get; } = new List<string>();

        public EditIntentReadResult ReadIntent(SessionId session, string rackId, long? chosenReference, EditIntentWitness witness)
        {
            Kinds.Add("ReadIntent");
            Origins.Add(null);
            return _inner.ReadIntent(session, rackId, chosenReference, witness);
        }

        public EditIntentReadResult ReadIntentAtOrigin(SessionId session, string rackId, string originText, EditIntentWitness witness)
        {
            Kinds.Add("ReadIntentAtOrigin");
            Origins.Add(originText);
            return _inner.ReadIntent(session, rackId, null, witness);
        }
    }

    /// <summary>
    /// The host of the workspace over a simulated bridge, with a reader, and the five rereads of an open draft driven through the
    /// public operations of the core (the recipe of <c>WorkspaceEditIntentHostTests</c>, which is private to it). No AutoCAD.
    /// </summary>
    internal sealed class PushBackFullReadRig
    {
        public PushBackFullReadRig(IWorkspaceEditIntentReader reader)
        {
            Host = new WorkspaceHostRig().WithDocument(100);
            Host.Host.OpenPanel();
            Session = Host.ActiveSession;
            Host.Idle();
            Host.Reader.Reads.Clear();
            Host.Host.SetEditIntentReader(reader);
        }

        public WorkspaceHostRig Host { get; }

        public WorkspaceHostCore Core => Host.Host;

        public SessionId Session { get; }

        public EditIntentRequest Request() => new EditIntentRequest(Session, EditIntentKit.RackA, null, null);

        public DraftSession<PushBackPilotDraftState> Draft() => Core.OpenDrafts.Find<PushBackPilotDraftState>(Session, EditIntentKit.RackA);

        /// <summary>The first read: <c>Begin</c> of the intention, which opens the draft of the rack.</summary>
        public DraftSession<PushBackPilotDraftState> Begin()
        {
            var outcome = Core.TryBeginPilotEdit(Request());
            Assert.Equal(EditIntentStatus.Admitted, outcome.Status);
            return Draft();
        }

        /// <summary>The state that the operation needs, and then the operation: ONE reread of the drawing.</summary>
        public void Run(string operation, DraftSession<PushBackPilotDraftState> draft)
        {
            switch (operation)
            {
                case "CheckNow":
                    Assert.True(draft.TryBeginApply().IsStarted);
                    draft.CompleteApply(DraftApplyOutcome<PushBackPilotDraftState>.CommitThrew("the commit threw"));
                    Core.TryCheckPilotEdit(Request());
                    break;
                case "Reload":
                    draft.MarkPossiblyStale();
                    Core.TryReloadPilotEdit(Request());
                    break;
                case "ConfirmBase":
                    Core.TryConfirmPilotBase(Request());
                    break;
                case "KeepMine":
                    draft.SetPendingText("field", "1");
                    draft.MarkPossiblyStale();
                    Core.TryKeepPilotEdit(Request());
                    break;
                case "DiscardAndReload":
                    Core.TryDiscardAndReloadPilotEdit(Request());
                    break;
                default:
                    throw new ArgumentException("unknown operation " + operation, nameof(operation));
            }
        }
    }
}
