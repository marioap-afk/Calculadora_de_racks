#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Application;
using RackCad.Application.Systems.PushBack.Pilot;
using RackCad.Application.Views.Redraw;
using RackCad.Application.Workspace.Drafts;
using Xunit;

namespace RackCad.Tests;

/// <summary>
/// I-64 F4-T4B: the double of the port of the update of the pilot. It simulates a drawing of the members of ONE rack (a key and the raw
/// text of its envelope), logs every call of the run in order, and can be told to throw at any call, to answer something else, or to run
/// a hook, which is how a test makes the drawing change between PREPARE and MUTATE. Writes are STAGED: the seal applies them to the drawing
/// that POST then reads, and releasing the scope of MUTATE without a seal throws them away, like an abort.
/// </summary>
internal sealed class PilotUpdateScopeDouble : IPushBackPilotUpdatePort
{
    private readonly List<(string Key, string Text)> _drawing;
    private readonly List<(string Key, string Text)> _staged = new();

    public PilotUpdateScopeDouble(params IntentItem[] members)
    {
        _drawing = members.Select(m => (m.Fact.DefinitionKey, m.Text!)).ToList();
    }

    /// <summary>Every call, in order: Begin, ReadForPrepare, PrepareUnits, BeginMutation, Reread, WriteUnit:key, Seal, Dispose:mutation, Post, Dispose:scope.</summary>
    public List<string> Calls { get; } = new();

    public PushBackPilotUpdateRequest? Request { get; private set; }

    public PushBackPilotPostRequest? PostRequest { get; private set; }

    public PushBackPilotUpdatePlan? Plan { get; private set; }

    /// <summary>What each WriteUnit received, in order.</summary>
    public List<(string Key, string Text)> Written { get; } = new();

    /// <summary>The state of the drawing now: the key and the raw text of each member.</summary>
    public IReadOnlyList<(string Key, string Text)> Drawing => _drawing;

    public bool DocumentMatches { get; set; } = true;

    public bool DocumentMatchesAtReread { get; set; } = true;

    public bool ReturnNullScope { get; set; }

    /// <summary>Throws the exception at the named call (see <see cref="Calls"/>); the call is logged first.</summary>
    public Dictionary<string, Exception> ThrowAt { get; } = new();

    /// <summary>Runs before the named call does anything: how a test changes the drawing or the draft between two steps.</summary>
    public Dictionary<string, Action> HookAt { get; } = new();

    public IntentItem[]? PrepareDrawing { get; set; }

    public IntentItem[]? RereadDrawing { get; set; }

    public IReadOnlyCollection<string> LockedAtPrepare { get; set; } = Array.Empty<string>();

    public IReadOnlyCollection<string> LockedAtReread { get; set; } = Array.Empty<string>();

    public bool NullCatalog { get; set; }

    public PushBackPilotImportResult Import { get; set; } = PushBackPilotImportResult.Succeeded();

    public string? Rejection { get; set; }

    public bool NullMutationStart { get; set; }

    public Func<string, bool> PreparedBlockMatches { get; set; } = _ => true;

    public Dictionary<string, IReadOnlyList<string>> MissingPieces { get; } = new(StringComparer.Ordinal);

    public bool NullUnitResult { get; set; }

    public bool ForeignAtPost { get; set; }

    public string? RegenFailure { get; set; }

    public bool NullPostValues { get; set; }

    public bool NullPostScan { get; set; }

    /// <summary>The name that a definition has after POST, from its key and the name that was asked for; by default the sanitized request.</summary>
    public Func<string, string, string>? NameAfterPost { get; set; }

    /// <summary>Replaces the whole answer of POST.</summary>
    public Func<PushBackPilotPostRequest, PushBackPilotPostValues?>? PostOverride { get; set; }

    /// <summary>Changes the text of a member as the drawing would show it after the seal (the facts of POST are built from the drawing).</summary>
    public Func<string, string, string>? TextAfterSeal { get; set; }

    /// <summary>The calls of a run that ends in POST without any failure.</summary>
    public static string[] Steps() => new[]
    {
        "Begin", "ReadForPrepare", "PrepareUnits", "BeginMutation", "Reread", "WriteUnit:A1", "WriteUnit:B2", "WriteUnit:C3", "Seal",
        "Dispose:mutation", "Post", "Dispose:scope",
    };

    public static RackSiblingScanFact FactOf(string key, string text, int layoutReferences = 1)
    {
        var item = IntentKit.Member(key, text);
        var f = item.Fact;
        return new RackSiblingScanFact(f.DefinitionKey, f.View, false, f.IsXrefDependent, f.EnvelopeInterpretable, f.RackId, f.ProbeId, layoutReferences, f.NestedReferenceCount, false);
    }

    public IPushBackPilotUpdateScope Begin(PushBackPilotUpdateRequest request)
    {
        Request = request;
        Enter("Begin");
        return ReturnNullScope ? null! : new Scope(this);
    }

    private void Enter(string call)
    {
        Calls.Add(call);
        if (HookAt.TryGetValue(call, out var hook)) hook();
        if (ThrowAt.TryGetValue(call, out var ex)) throw ex;
    }

    private PushBackPilotScanValues Values(IntentItem[]? replacement, bool matches, IReadOnlyCollection<string> locked, bool withCatalog, bool withMatches)
    {
        var items = replacement ?? _drawing.Select(m => IntentKit.Member(m.Key, m.Text)).ToArray();
        var raw = items.Where(i => i.Text != null).ToDictionary(i => i.Fact.DefinitionKey, i => i.Text!, StringComparer.Ordinal);
        Dictionary<string, bool>? prepared = null;
        if (withMatches && Plan != null) prepared = Plan.Members.ToDictionary(m => m.Key, m => PreparedBlockMatches(m.Key), StringComparer.Ordinal);
        return new PushBackPilotScanValues(
            matches,
            items.Select(i => i.Fact).ToList(),
            raw,
            locked,
            withCatalog && !NullCatalog ? PilotKit.Catalog : null,
            prepared);
    }

    private sealed class Scope : IPushBackPilotUpdateScope
    {
        private readonly PilotUpdateScopeDouble _owner;

        public Scope(PilotUpdateScopeDouble owner) => _owner = owner;

        public PushBackPilotScanValues ReadForPrepare()
        {
            _owner.Enter("ReadForPrepare");
            return _owner.Values(_owner.PrepareDrawing, _owner.DocumentMatches, _owner.LockedAtPrepare, withCatalog: true, withMatches: false);
        }

        public PushBackPilotImportResult PrepareUnits(PushBackPilotUpdatePlan plan)
        {
            _owner.Plan = plan;
            _owner.Enter("PrepareUnits");
            return _owner.Import;
        }

        public PushBackPilotMutationStart BeginMutation()
        {
            _owner.Enter("BeginMutation");
            if (_owner.NullMutationStart) return null!;
            if (_owner.Rejection != null) return PushBackPilotMutationStart.Rejected(_owner.Rejection);
            return PushBackPilotMutationStart.Started(new MutationScope(_owner));
        }

        public PushBackPilotPostValues Post(PushBackPilotPostRequest request)
        {
            _owner.PostRequest = request;
            _owner.Enter("Post");
            if (_owner.NullPostValues) return null!;
            if (_owner.PostOverride != null) return _owner.PostOverride(request)!;
            if (_owner.ForeignAtPost) return new PushBackPilotPostValues(true, null, null, null);

            var names = new Dictionary<string, string>(StringComparer.Ordinal);
            foreach (var member in _owner._drawing)
            {
                request.DesiredNames.TryGetValue(member.Key, out var desired);
                names[member.Key] = desired == null
                    ? "Unrelated name"
                    : _owner.NameAfterPost != null ? _owner.NameAfterPost(member.Key, desired) : BlockNaming.SanitizeBlockName(desired);
            }

            var scan = _owner.NullPostScan ? null : _owner.Values(null, true, Array.Empty<string>(), withCatalog: false, withMatches: false);
            return new PushBackPilotPostValues(false, scan, names, _owner.RegenFailure);
        }

        public void Dispose() => _owner.EnterDispose("Dispose:scope");
    }

    private void EnterDispose(string call)
    {
        Calls.Add(call);
        if (HookAt.TryGetValue(call, out var hook)) hook();
        if (ThrowAt.TryGetValue(call, out var ex)) throw ex;
    }

    private sealed class MutationScope : IPushBackPilotMutationScope
    {
        private readonly PilotUpdateScopeDouble _owner;

        public MutationScope(PilotUpdateScopeDouble owner) => _owner = owner;

        public PushBackPilotScanValues Reread()
        {
            _owner.Enter("Reread");
            return _owner.Values(_owner.RereadDrawing, _owner.DocumentMatchesAtReread, _owner.LockedAtReread, withCatalog: false, withMatches: true);
        }

        public PushBackPilotUnitResult WriteUnit(string memberKey, string envelopeText)
        {
            _owner.Enter("WriteUnit:" + memberKey);
            _owner.Written.Add((memberKey, envelopeText));
            _owner._staged.Add((memberKey, envelopeText));
            if (_owner.NullUnitResult) return null!;
            return new PushBackPilotUnitResult(_owner.MissingPieces.TryGetValue(memberKey, out var missing) ? missing : Array.Empty<string>());
        }

        public void Seal()
        {
            _owner.Enter("Seal");
            foreach (var (key, text) in _owner._staged)
            {
                var index = _owner._drawing.FindIndex(m => m.Key == key);
                _owner._drawing[index] = (key, _owner.TextAfterSeal != null ? _owner.TextAfterSeal(key, text) : text);
            }

            _owner._staged.Clear();
        }

        public void Dispose()
        {
            // Without a seal this is the abort: whatever the units staged is thrown away.
            _owner._staged.Clear();
            _owner.EnterDispose("Dispose:mutation");
        }
    }
}
