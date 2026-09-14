using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Persistence;

namespace RackCad.Application.CustomProperties
{
    /// <summary>What a window may offer over a collection (I-54 D-18.3).</summary>
    public enum CustomPropertiesWorkspaceState
    {
        /// <summary>Rows by id and every operation: Single in Rack, Absent or Readable in Project.</summary>
        Editable = 1,

        /// <summary>Read-only from the read itself, with its reason: no operation is offered that would fail on commit.</summary>
        ReadOnly = 2,

        /// <summary>The views of the rack disagree: per-view summaries and, only when available, a unify chosen by the user.</summary>
        Divergent = 3,
    }

    public enum CustomPropertiesScope
    {
        Project = 1,

        Rack = 2,
    }

    /// <summary>Why a workspace is read-only: a rack result of D-09.8 or a non-writable container of D-13.</summary>
    public enum CustomPropertiesReadOnlyReason
    {
        XrefRejected = 1,

        NoIdentity = 2,

        IndeterminateMembership = 3,

        MixedKind = 4,

        UnknownKind = 5,

        CustomPropertiesReadOnly = 6,

        PresentButUnreadable = 7,

        AmbiguousIdentity = 8,

        IncompatibleMajor = 9,

        DepthLimitExceeded = 10,
    }

    /// <summary>One editable property, as a window lists it. Every operation travels by <see cref="Id"/>, never by name.</summary>
    public sealed class CustomPropertiesRow
    {
        internal CustomPropertiesRow(CustomPropertyId id, string name, string value, bool nombreRepetido)
        {
            Id = id;
            Name = name;
            Value = value;
            NombreRepetido = nombreRepetido;
        }

        public CustomPropertyId Id { get; }

        public string Name { get; }

        public string Value { get; }

        /// <summary>Another entry has the same name, compared in NFC ignoring case (D-05.2). A warning, not an error.</summary>
        public bool NombreRepetido { get; }
    }

    /// <summary>One <c>Nombre — Valor</c> line of a view summary.</summary>
    public sealed class CustomPropertiesSummaryEntry
    {
        internal CustomPropertiesSummaryEntry(string name, string value)
        {
            Name = name;
            Value = value;
        }

        public string Name { get; }

        public string Value { get; }
    }

    /// <summary>
    /// What one view holds, labelled with that view (D-10.3): block, handle, view, section, the state of its collection and,
    /// when readable, its entries. Never presented as the rack's values.
    /// </summary>
    public sealed class CustomPropertiesViewSummary
    {
        internal CustomPropertiesViewSummary(RackCustomPropertiesMember member, bool isUnifySourceAvailable)
        {
            Handle = member.Handle;
            BlockName = member.BlockName;
            View = member.View;
            Section = member.Section;
            State = member.Collection.Outcome;
            Error = member.Collection.Error;
            Entries = member.Collection.Outcome == CustomPropertiesReadOutcome.Readable
                ? member.Collection.Document.Entries.Select(entry => new CustomPropertiesSummaryEntry(entry.Name, entry.Value)).ToList()
                : new List<CustomPropertiesSummaryEntry>();
            IsUnifySourceAvailable = isUnifySourceAvailable;
        }

        public string Handle { get; }

        public string BlockName { get; }

        public string View { get; }

        public int Section { get; }

        public CustomPropertiesReadOutcome State { get; }

        /// <summary>The store's reason when the collection of this view is not writable.</summary>
        public string Error { get; }

        public IReadOnlyList<CustomPropertiesSummaryEntry> Entries { get; }

        /// <summary>This view may be chosen as the source of a safe unify (D-09.10).</summary>
        public bool IsUnifySourceAvailable { get; }
    }

    /// <summary>
    /// The pure workspace of custom properties, agnostic to the scope (I-54 D-18.3 / ADR-0039 §8). It holds no UI type and
    /// is never an authority: a window presents it and returns intents by id, which Application revalidates on a fresh
    /// read (D-22).
    /// </summary>
    public sealed class CustomPropertiesWorkspace
    {
        private static readonly IReadOnlyList<CustomPropertiesRow> NoRows = Array.Empty<CustomPropertiesRow>();

        private static readonly IReadOnlyList<CustomPropertiesViewSummary> NoSummaries = Array.Empty<CustomPropertiesViewSummary>();

        private static readonly IReadOnlyList<RackCustomPropertiesDefinition> NoDefinitions = Array.Empty<RackCustomPropertiesDefinition>();

        private CustomPropertiesWorkspace(
            CustomPropertiesWorkspaceState state,
            CustomPropertiesScope scope,
            string scopeLabel,
            CustomPropertiesReadOnlyReason? readOnlyReason,
            string diagnostic,
            IReadOnlyList<CustomPropertiesRow> rows,
            IReadOnlyList<CustomPropertiesViewSummary> viewSummaries,
            IReadOnlyList<RackCustomPropertiesDefinition> uninterpretableDefinitions)
        {
            State = state;
            Scope = scope;
            ScopeLabel = scopeLabel;
            ReadOnlyReason = readOnlyReason;
            Diagnostic = diagnostic;
            Rows = rows ?? NoRows;
            ViewSummaries = viewSummaries ?? NoSummaries;
            UninterpretableDefinitions = uninterpretableDefinitions ?? NoDefinitions;
        }

        public CustomPropertiesWorkspaceState State { get; }

        public CustomPropertiesScope Scope { get; }

        public string ScopeLabel { get; }

        /// <summary>Why nothing may be written; null when the state is not <see cref="CustomPropertiesWorkspaceState.ReadOnly"/>.</summary>
        public CustomPropertiesReadOnlyReason? ReadOnlyReason { get; }

        public string Diagnostic { get; }

        /// <summary>Rows by id, ONLY when the state is <see cref="CustomPropertiesWorkspaceState.Editable"/>.</summary>
        public IReadOnlyList<CustomPropertiesRow> Rows { get; }

        /// <summary>Per-view summaries of a rack that is not Single, whenever its views are known.</summary>
        public IReadOnlyList<CustomPropertiesViewSummary> ViewSummaries { get; }

        /// <summary>The definitions behind an indeterminate membership: block, handle and whether each is placed.</summary>
        public IReadOnlyList<RackCustomPropertiesDefinition> UninterpretableDefinitions { get; }

        /// <summary>A Divergent rack in which at least one view may be the source of a safe unify.</summary>
        public bool UnifyAvailable => ViewSummaries.Any(summary => summary.IsUnifySourceAvailable);

        public static CustomPropertiesWorkspace ForProject(CustomPropertiesReadResult collection)
        {
            if (collection == null)
            {
                throw new ArgumentNullException(nameof(collection));
            }

            if (collection.CanWrite)
            {
                return new CustomPropertiesWorkspace(
                    CustomPropertiesWorkspaceState.Editable, CustomPropertiesScope.Project, ProjectLabel, null, null, RowsOf(collection), null, null);
            }

            return new CustomPropertiesWorkspace(
                CustomPropertiesWorkspaceState.ReadOnly,
                CustomPropertiesScope.Project,
                ProjectLabel,
                ContainerReason(collection.Outcome),
                collection.Error,
                null,
                null,
                null);
        }

        public static CustomPropertiesWorkspace ForRack(RackCustomPropertiesAuthorityResult authority)
        {
            if (authority == null)
            {
                throw new ArgumentNullException(nameof(authority));
            }

            var label = string.IsNullOrWhiteSpace(authority.RackName) ? "Rack" : "Rack «" + authority.RackName + "»";

            switch (authority.Outcome)
            {
                case RackCustomPropertiesAuthorityOutcome.Single:
                    return new CustomPropertiesWorkspace(
                        CustomPropertiesWorkspaceState.Editable, CustomPropertiesScope.Rack, label, null, null, RowsOf(authority.Collection), null, null);

                case RackCustomPropertiesAuthorityOutcome.Divergent:
                    var available = new HashSet<string>(
                        authority.UnifyOptions.Where(option => option.IsAvailable).Select(option => option.SourceHandle), StringComparer.Ordinal);

                    return new CustomPropertiesWorkspace(
                        CustomPropertiesWorkspaceState.Divergent,
                        CustomPropertiesScope.Rack,
                        label,
                        null,
                        authority.Error,
                        null,
                        authority.Members.Select(member => new CustomPropertiesViewSummary(member, available.Contains(member.Handle))).ToList(),
                        null);

                default:
                    return new CustomPropertiesWorkspace(
                        CustomPropertiesWorkspaceState.ReadOnly,
                        CustomPropertiesScope.Rack,
                        label,
                        RackReason(authority.Outcome),
                        authority.Error,
                        null,
                        authority.Members.Select(member => new CustomPropertiesViewSummary(member, false)).ToList(),
                        authority.UninterpretableDefinitions);
            }
        }

        private const string ProjectLabel = "Proyecto";

        private static IReadOnlyList<CustomPropertiesRow> RowsOf(CustomPropertiesReadResult collection)
        {
            var repeated = new HashSet<CustomPropertyId>(collection.RepeatedNameEntryIds);

            return collection.Document.Entries
                .Select(entry =>
                {
                    CustomPropertyId.TryParse(entry.Id, out var id);
                    return new CustomPropertiesRow(id, entry.Name, entry.Value, repeated.Contains(id));
                })
                .ToList();
        }

        private static CustomPropertiesReadOnlyReason RackReason(RackCustomPropertiesAuthorityOutcome outcome)
        {
            switch (outcome)
            {
                case RackCustomPropertiesAuthorityOutcome.XrefRejected:
                    return CustomPropertiesReadOnlyReason.XrefRejected;
                case RackCustomPropertiesAuthorityOutcome.NoIdentity:
                    return CustomPropertiesReadOnlyReason.NoIdentity;
                case RackCustomPropertiesAuthorityOutcome.IndeterminateMembership:
                    return CustomPropertiesReadOnlyReason.IndeterminateMembership;
                case RackCustomPropertiesAuthorityOutcome.MixedKind:
                    return CustomPropertiesReadOnlyReason.MixedKind;
                case RackCustomPropertiesAuthorityOutcome.UnknownKind:
                    return CustomPropertiesReadOnlyReason.UnknownKind;
                case RackCustomPropertiesAuthorityOutcome.CustomPropertiesReadOnly:
                    return CustomPropertiesReadOnlyReason.CustomPropertiesReadOnly;
                default:
                    throw new InvalidOperationException("Un rack " + outcome + " no es de solo lectura.");
            }
        }

        private static CustomPropertiesReadOnlyReason ContainerReason(CustomPropertiesReadOutcome outcome)
        {
            switch (outcome)
            {
                case CustomPropertiesReadOutcome.AmbiguousIdentity:
                    return CustomPropertiesReadOnlyReason.AmbiguousIdentity;
                case CustomPropertiesReadOutcome.IncompatibleMajor:
                    return CustomPropertiesReadOnlyReason.IncompatibleMajor;
                case CustomPropertiesReadOutcome.DepthLimitExceeded:
                    return CustomPropertiesReadOnlyReason.DepthLimitExceeded;
                default:
                    return CustomPropertiesReadOnlyReason.PresentButUnreadable;
            }
        }
    }
}
