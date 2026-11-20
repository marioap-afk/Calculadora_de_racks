using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Catalogs;
using RackCad.Application.Persistence;

namespace RackCad.Application.Systems.PushBack.Draft
{
    /// <summary>The group of a changed leaf (D-PBT3-07).</summary>
    public enum PushBackChangeGroup { Name = 0, Rack = 1, Annotations = 2, Safety = 3, Fronts = 4, Modules = 5, SideB = 6, Composite = 7, OutsideEditor = 8 }

    /// <summary>Who changed a leaf with respect to the base.</summary>
    public enum PushBackChangeClass { ChangedExternally = 0, ChangedByUser = 1, Overlapping = 2 }

    /// <summary>One leaf that differs from the base in the current rack, in the draft or in both.</summary>
    public sealed class PushBackChangeEntry
    {
        internal PushBackChangeEntry(
            PushBackDocumentLevel level, string path, PushBackChangeGroup group,
            PushBackChangeClass changeClass, bool isStructural, string baseValue, string externalValue, string draftValue)
        {
            Level = level;
            Path = path;
            Group = group;
            Class = changeClass;
            IsStructural = isStructural;
            BaseValue = baseValue;
            ExternalValue = externalValue;
            DraftValue = draftValue;
        }

        public PushBackDocumentLevel Level { get; }

        /// <summary>The path <c>$.A.B[3].C</c>, with the real indexes.</summary>
        public string Path { get; }

        public PushBackChangeGroup Group { get; }

        public PushBackChangeClass Class { get; }

        public bool IsStructural { get; }

        /// <summary>The JSON text of the leaf, or the marker of an absent one.</summary>
        public string BaseValue { get; }

        public string ExternalValue { get; }

        public string DraftValue { get; }
    }

    /// <summary>The classification of the differences of a draft and of the current rack with respect to the base.</summary>
    public sealed class PushBackConflictClassification
    {
        internal PushBackConflictClassification(
            IEnumerable<PushBackChangeEntry> entries, IEnumerable<string> viewsToErase, IEnumerable<string> viewsRejected, bool allViewsObsolete)
        {
            if (entries == null) throw new ArgumentNullException(nameof(entries));
            if (viewsToErase == null) throw new ArgumentNullException(nameof(viewsToErase));
            if (viewsRejected == null) throw new ArgumentNullException(nameof(viewsRejected));
            Entries = entries
                .OrderBy(entry => entry.Level)
                .ThenBy(entry => entry.Path, StringComparer.Ordinal)
                .ToList()
                .AsReadOnly();
            ChangedExternally = OfClass(PushBackChangeClass.ChangedExternally);
            ChangedByUser = OfClass(PushBackChangeClass.ChangedByUser);
            Overlapping = OfClass(PushBackChangeClass.Overlapping);
            Structural = Entries.Where(entry => entry.IsStructural).ToList().AsReadOnly();
            ViewsToErase = viewsToErase.OrderBy(key => key, StringComparer.Ordinal).ToList().AsReadOnly();
            ViewsRejected = viewsRejected.OrderBy(key => key, StringComparer.Ordinal).ToList().AsReadOnly();
            AllViewsObsolete = allViewsObsolete;
        }

        public IReadOnlyList<PushBackChangeEntry> Entries { get; }

        public IReadOnlyList<PushBackChangeEntry> ChangedExternally { get; }

        public IReadOnlyList<PushBackChangeEntry> ChangedByUser { get; }

        public IReadOnlyList<PushBackChangeEntry> Overlapping { get; }

        public IReadOnlyList<PushBackChangeEntry> Structural { get; }

        public IReadOnlyList<string> ViewsToErase { get; }

        public IReadOnlyList<string> ViewsRejected { get; }

        public bool AllViewsObsolete { get; }

        private IReadOnlyList<PushBackChangeEntry> OfClass(PushBackChangeClass changeClass)
            => Entries.Where(entry => entry.Class == changeClass).ToList().AsReadOnly();
    }

    /// <summary>I-64 F4B PB-T3a (D-PBT3-07) — the classifier of the conflicts of D-PB-05, not connected.</summary>
    public static class PushBackConflictClassifier
    {
        public static PushBackConflictClassification Classify(
            PushBackFullDraftState baseState, PushBackFullDraftState currentRack, PushBackFullDraftState draft,
            IEnumerable<KeyValuePair<string, RackEmbedDocument>> currentMembers, RackCatalog catalog)
            => new PushBackConflictClassification(
                Array.Empty<PushBackChangeEntry>(), Array.Empty<string>(), Array.Empty<string>(), false);
    }
}
