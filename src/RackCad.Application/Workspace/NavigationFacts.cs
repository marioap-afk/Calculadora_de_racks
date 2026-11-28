#nullable enable
using System.Collections.Generic;

namespace RackCad.Application.Workspace
{
    /// <summary>
    /// Neutral facts of ONE block definition as the host captured them (I-64 D-05). They are private to the panel and
    /// are not a reusable foundation: only the navigation authority reads them.
    /// </summary>
    public sealed record NavigationDefinitionFact(
        long DefinitionHandle,
        bool IsXref,
        bool IsInterpretable,
        string? EnvelopeId,
        string? ProbedId,
        string? Kind,
        string? View,
        int Section,
        string? Name,
        IReadOnlyList<long> ModelSpaceReferenceHandles);

    /// <summary>
    /// Read-only source of definition facts for one session (the Plugin implements it over the integrated envelope scan in
    /// F2-T2). It carries the session on every call (D-03) and writes nothing.
    /// </summary>
    public interface INavigationFactsPort
    {
        IReadOnlyList<NavigationDefinitionFact> ReadDefinitionFacts(SessionId session);
    }
}
