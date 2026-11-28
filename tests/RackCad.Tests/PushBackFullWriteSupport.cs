using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Application;
using RackCad.Application.Catalogs;
using RackCad.Application.Systems.PushBack.Draft;
using RackCad.Application.Systems.PushBack.Pilot;
using RackCad.Application.Views.Redraw;
using RackCad.Application.Workspace.Drafts;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>
    /// I-64 F4B PB-T3b — the double of the port of the update WITH the capacity to erase views (INV-PBT3B-06, CT-PBT3B-07 to 09). It is a
    /// copy of <c>PilotUpdateScopeDouble</c>, which is not edited: its scope of MUTATE implements <see cref="IPushBackPilotViewEraser"/>,
    /// it logs <c>EraseUnit:&lt;key&gt;</c> and applies the erasure at the seal, like the writes, and its re-read of MUTATE answers
    /// <c>PreparedBlockMatches</c> for EVERY key of the plan, the ones of <c>Members</c> and the ones of <c>Erasures</c>, as the writer of
    /// PB-T3c will do. It simulates a drawing of the members of ONE rack (a key and the raw text of its envelope).
    /// </summary>
    internal sealed class PilotEraseScopeDouble : IPushBackPilotUpdatePort
    {
        private readonly List<(string Key, string Text)> _drawing;
        private readonly List<(string Key, string Text)> _staged = new List<(string Key, string Text)>();
        private readonly List<string> _stagedErasures = new List<string>();

        public PilotEraseScopeDouble(params IntentItem[] members)
        {
            _drawing = members.Select(m => (m.Fact.DefinitionKey, m.Text)).ToList();
        }

        /// <summary>Every call, in order: Begin, ReadForPrepare, PrepareUnits, BeginMutation, Reread, WriteUnit:key, EraseUnit:key, Seal, Dispose:mutation, Post, Dispose:scope.</summary>
        public List<string> Calls { get; } = new List<string>();

        public PushBackPilotUpdateRequest Request { get; private set; }

        public PushBackPilotPostRequest PostRequest { get; private set; }

        public PushBackPilotUpdatePlan Plan { get; private set; }

        /// <summary>What each WriteUnit received, in order.</summary>
        public List<(string Key, string Text)> Written { get; } = new List<(string Key, string Text)>();

        /// <summary>The keys that each EraseUnit received, in order.</summary>
        public List<string> Erased { get; } = new List<string>();

        /// <summary>The state of the drawing now: the key and the raw text of each member.</summary>
        public IReadOnlyList<(string Key, string Text)> Drawing => _drawing;

        public bool DocumentMatches { get; set; } = true;

        public IReadOnlyCollection<string> LockedAtPrepare { get; set; } = Array.Empty<string>();

        public IReadOnlyCollection<string> LockedAtReread { get; set; } = Array.Empty<string>();

        public IntentItem[] RereadDrawing { get; set; }

        /// <summary>Throws the exception at the named call (see <see cref="Calls"/>); the call is logged first.</summary>
        public Dictionary<string, Exception> ThrowAt { get; } = new Dictionary<string, Exception>();

        /// <summary>Runs before the named call does anything: how a test changes the drawing between two steps.</summary>
        public Dictionary<string, Action> HookAt { get; } = new Dictionary<string, Action>();

        public IPushBackPilotUpdateScope Begin(PushBackPilotUpdateRequest request)
        {
            Request = request;
            Enter("Begin");
            return new Scope(this);
        }

        private void Enter(string call)
        {
            Calls.Add(call);
            if (HookAt.TryGetValue(call, out var hook)) hook();
            if (ThrowAt.TryGetValue(call, out var error)) throw error;
        }

        private PushBackPilotScanValues Values(IntentItem[] replacement, bool matches, IReadOnlyCollection<string> locked, bool withCatalog, bool withMatches)
        {
            var items = replacement ?? _drawing.Select(m => IntentKit.Member(m.Key, m.Text)).ToArray();
            var raw = items.Where(i => i.Text != null).ToDictionary(i => i.Fact.DefinitionKey, i => i.Text, StringComparer.Ordinal);
            Dictionary<string, bool> prepared = null;
            if (withMatches && Plan != null)
            {
                // PreparedBlockMatches for EVERY key of the plan: the ones that are redrawn and the ones that are erased.
                prepared = Plan.Members.Select(m => m.Key).Concat(Plan.Erasures).ToDictionary(key => key, key => true, StringComparer.Ordinal);
            }

            return new PushBackPilotScanValues(matches, items.Select(i => i.Fact).ToList(), raw, locked, withCatalog ? PilotKit.Catalog : null, prepared);
        }

        private sealed class Scope : IPushBackPilotUpdateScope
        {
            private readonly PilotEraseScopeDouble _owner;

            public Scope(PilotEraseScopeDouble owner) => _owner = owner;

            public PushBackPilotScanValues ReadForPrepare()
            {
                _owner.Enter("ReadForPrepare");
                return _owner.Values(null, _owner.DocumentMatches, _owner.LockedAtPrepare, withCatalog: true, withMatches: false);
            }

            public PushBackPilotImportResult PrepareUnits(PushBackPilotUpdatePlan plan)
            {
                _owner.Plan = plan;
                _owner.Enter("PrepareUnits");
                return PushBackPilotImportResult.Succeeded();
            }

            public PushBackPilotMutationStart BeginMutation()
            {
                _owner.Enter("BeginMutation");
                return PushBackPilotMutationStart.Started(new MutationScope(_owner));
            }

            public PushBackPilotPostValues Post(PushBackPilotPostRequest request)
            {
                _owner.PostRequest = request;
                _owner.Enter("Post");
                var names = new Dictionary<string, string>(StringComparer.Ordinal);
                foreach (var member in _owner._drawing)
                {
                    request.DesiredNames.TryGetValue(member.Key, out var desired);
                    names[member.Key] = desired == null ? "Unrelated name" : BlockNaming.SanitizeBlockName(desired);
                }

                var scan = _owner.Values(null, true, Array.Empty<string>(), withCatalog: false, withMatches: false);
                return new PushBackPilotPostValues(false, scan, names, null);
            }

            public void Dispose() => _owner.EnterDispose("Dispose:scope");
        }

        private void EnterDispose(string call)
        {
            Calls.Add(call);
            if (HookAt.TryGetValue(call, out var hook)) hook();
            if (ThrowAt.TryGetValue(call, out var error)) throw error;
        }

        private sealed class MutationScope : IPushBackPilotMutationScope, IPushBackPilotViewEraser
        {
            private readonly PilotEraseScopeDouble _owner;

            public MutationScope(PilotEraseScopeDouble owner) => _owner = owner;

            public PushBackPilotScanValues Reread()
            {
                _owner.Enter("Reread");
                return _owner.Values(_owner.RereadDrawing, true, _owner.LockedAtReread, withCatalog: false, withMatches: true);
            }

            public PushBackPilotUnitResult WriteUnit(string memberKey, string envelopeText)
            {
                _owner.Enter("WriteUnit:" + memberKey);
                _owner.Written.Add((memberKey, envelopeText));
                _owner._staged.Add((memberKey, envelopeText));
                return new PushBackPilotUnitResult(Array.Empty<string>());
            }

            public PushBackPilotUnitResult EraseUnit(string memberKey)
            {
                _owner.Enter("EraseUnit:" + memberKey);
                _owner.Erased.Add(memberKey);
                _owner._stagedErasures.Add(memberKey);
                return new PushBackPilotUnitResult(Array.Empty<string>());
            }

            public void Seal()
            {
                _owner.Enter("Seal");
                foreach (var (key, text) in _owner._staged)
                {
                    var index = _owner._drawing.FindIndex(m => m.Key == key);
                    _owner._drawing[index] = (key, text);
                }

                foreach (var key in _owner._stagedErasures) _owner._drawing.RemoveAll(m => m.Key == key);
                _owner._staged.Clear();
                _owner._stagedErasures.Clear();
            }

            public void Dispose()
            {
                // Without a seal this is the abort: whatever the units staged is thrown away.
                _owner._staged.Clear();
                _owner._stagedErasures.Clear();
                _owner.EnterDispose("Dispose:mutation");
            }
        }
    }

    /// <summary>
    /// I-64 F4B PB-T3b — the builders shared by the classes of the write of the full model: the read, the draft, the gestures and the
    /// snapshot of PREPARE. They only run the existing API; every call to the new one goes through the helper of the class that makes it.
    /// </summary>
    internal static class FullWriteKit
    {
        public static RackCatalog Catalog => PilotKit.Catalog;

        public static PushBackFullIntentResult Read(PushBackPilotIntentScan scan) => PushBackFullDraftReader.Read(scan, Catalog);

        /// <summary>A draft of the full model born from an admitted read, as the host does with the pilot (WHC:347-352).</summary>
        public static DraftSession<PushBackFullDraftState> DraftOf(DraftIntentRead<PushBackFullDraftState> read)
        {
            var begun = new DraftRegistry().Begin(
                DraftKit.S1, IntentKit.Rack, read, PushBackFullDraftStateComparer.Instance, PushBackFullDraftStateComparer.Copy);
            Assert.Equal(DraftBeginStatus.Created, begun.Status);
            return begun.Draft;
        }

        public static DraftSession<PushBackFullDraftState> DraftOf(PushBackFullIntentResult read)
        {
            Assert.True(read.Read.IsAdmitted, "the read of the recipe is not admitted: " + read.Read.Admission.Diagnostic);
            return DraftOf(read.Read);
        }

        /// <summary>One edit of the current state: a COPY is edited and handed to the draft, which keeps a new copy (C-1 of F4B-E33).</summary>
        public static void Edit(DraftSession<PushBackFullDraftState> draft, Action<PushBackFullDraftState> edit)
        {
            var copy = PushBackFullDraftStateComparer.Copy(draft.CurrentDraftState);
            edit(copy);
            Assert.Equal(DraftEditStatus.Updated, draft.SetCurrentDraftState(copy).Status);
        }

        /// <summary>The members of a scan as the items that the double of the port draws.</summary>
        public static IntentItem[] ItemsOf(PushBackPilotIntentScan scan)
            => scan.Facts.Select(f => new IntentItem(f, scan.RawEnvelopes[f.DefinitionKey])).ToArray();
    }
}
