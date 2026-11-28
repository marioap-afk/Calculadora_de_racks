#nullable enable
using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Workspace;

namespace RackCad.Tests;

/// <summary>I-64 F2-T1-NAVIGATION-CORE: simulated facts port. It records every read and the session it was asked for.</summary>
internal sealed class FakeNavigationFacts : INavigationFactsPort
{
    private readonly Dictionary<SessionId, List<NavigationDefinitionFact>> _bySession = new();

    public int Reads { get; private set; }

    public List<SessionId> ReadSessions { get; } = new();

    public List<NavigationDefinitionFact> For(SessionId session)
    {
        if (!_bySession.TryGetValue(session, out var list))
            _bySession[session] = list = new List<NavigationDefinitionFact>();
        return list;
    }

    public FakeNavigationFacts With(SessionId session, params NavigationDefinitionFact[] facts)
    {
        For(session).AddRange(facts);
        return this;
    }

    public IReadOnlyList<NavigationDefinitionFact> ReadDefinitionFacts(SessionId session)
    {
        Reads++;
        ReadSessions.Add(session);
        return For(session).ToList();
    }
}

/// <summary>I-64 F2-T1-NAVIGATION-CORE: simulated effects port. Any call is recorded; a rejected action must leave it empty.</summary>
internal sealed class RecordingNavigationEffects : INavigationEffects
{
    public List<string> Calls { get; } = new();

    public void SelectReferences(SessionId session, IReadOnlyList<long> handles) =>
        Calls.Add("select:" + session.Value + ":" + string.Join(",", handles));

    public void ViewReference(SessionId session, long handle) => Calls.Add("view:" + session.Value + ":" + handle);

    public void CenterOnReference(SessionId session, long handle) => Calls.Add("center:" + session.Value + ":" + handle);
}

internal static class NavFact
{
    /// <summary>An interpretable, non-xref member definition of a selective rack.</summary>
    public static NavigationDefinitionFact Member(
        long definition, string? envelopeId, string view, int section, params long[] references) =>
        new(definition, false, true, envelopeId, null, "selective", view, section, "Rack " + definition, references);

    public static NavigationDefinitionFact Raw(
        long definition,
        bool isXref,
        bool isInterpretable,
        string? envelopeId,
        string? probedId,
        string? kind,
        string? view,
        int section,
        params long[] references) =>
        new(definition, isXref, isInterpretable, envelopeId, probedId, kind, view, section, "Rack " + definition, references);

    public static long[] Handles(NavigationTargetSet set) => set.Targets.Select(t => t.ReferenceHandle).ToArray();
}
