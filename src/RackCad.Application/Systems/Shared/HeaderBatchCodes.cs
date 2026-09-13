namespace RackCad.Application.Systems.Shared
{
    /// <summary>
    /// Why a destination that the system's taxonomy CAN address takes no copy in a header reuse or batch distribution
    /// (I-53, ID6 + ID7; contract §3.5, ADR-0037). An omission never invalidates the other destinations: nothing is
    /// clamped to a neighbour, nothing is created and nothing pruned is resurrected.
    /// <para>
    /// The set is CLOSED. A reason that is not one of these three is a change of the contract, not a new member.
    /// </para>
    /// </summary>
    public enum HeaderOmissionReason
    {
        /// <summary>The taxonomy expresses the address, but the current topology has no such position.</summary>
        AbsentInScope = 1,

        /// <summary>The destination exists and is well addressed, but it is not drawn.</summary>
        NotPhysicallyPresent = 2,

        /// <summary>The destination is the address of the source itself.</summary>
        IsSource = 3,
    }

    /// <summary>
    /// Why a whole header reuse or batch distribution is refused: zero mutation, scalars included (I-53, contract §3.6,
    /// ADR-0037). The set is CLOSED and there is no generic or catch-all code.
    /// <para>
    /// The numeric values are stable identities, not an ordering. Which code wins when a request has several defects is
    /// the precedence of contract §3.7, and each consuming system applies it; this type does not.
    /// </para>
    /// </summary>
    public enum HeaderRejectionCode
    {
        /// <summary>The request or the plan was captured against another topology or resolution.</summary>
        StaleTargets = 1,

        /// <summary>The source address no longer designates a header in the current topology.</summary>
        SourceNotFound = 2,

        /// <summary>The source address resolves, but the source is not eligible or cannot be captured.</summary>
        SourceUnusable = 3,

        /// <summary>The user's intent has no destinations.</summary>
        NoTargets = 4,

        /// <summary>A destination address does not belong to the universe of the taxonomy.</summary>
        MalformedTarget = 5,

        /// <summary>Every resolved destination was omitted.</summary>
        NoApplicableTargets = 6,

        /// <summary>An applicable destination is invalid, or its normalized copy is not usable.</summary>
        DestinationInvalid = 7,
    }

    /// <summary>
    /// Severity of a non-blocking notice of a prepared plan (contract §3.4, <c>Informativo</c> | <c>Severo</c>). A plan
    /// requires confirmation if and only if it carries at least one <see cref="Severe"/> notice.
    /// </summary>
    public enum HeaderWarningSeverity
    {
        /// <summary>Informs; never asks for confirmation.</summary>
        Informative = 1,

        /// <summary>Requires the user's confirmation before anything is applied.</summary>
        Severe = 2,
    }
}
