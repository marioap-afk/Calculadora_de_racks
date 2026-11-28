using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using RackCad.Application.Catalogs;
using RackCad.Application.Persistence;
using RackCad.Application.Systems.PushBack.Pilot;
using RackCad.Application.Workspace.Drafts;

namespace RackCad.Application.Systems.PushBack.Draft
{
    /// <summary>
    /// I-64 F4B PB-T4b (D-PBT4-08, D-PBT4-09; OB-05.3) — what "keep my draft" shows above its Yes and No: the changes BY GROUPS (the decision of
    /// the Owner, "Por grupos"), one line per class of change, the structural one and the ones of the views, in that order. Pure: it reads no
    /// drawing and changes nothing. <see cref="Of"/> turns a classification into lines; <see cref="Read"/> classifies a draft against the reread
    /// of its rack with the classifier of the full model and keeps the text of the envelope of the origin of that reread, which is the text of
    /// the witness that the confirmation hands back.
    /// </summary>
    public sealed class PushBackKeepMinePresentation
    {
        private PushBackKeepMinePresentation(IReadOnlyList<string> lines, string originText, string diagnostic)
        {
            Lines = lines;
            OriginText = originText;
            Diagnostic = diagnostic;
        }

        /// <summary>The lines, in the order of the table of D-PBT4-09, each with its final point.</summary>
        public IReadOnlyList<string> Lines { get; }

        /// <summary>True when there is no line.</summary>
        public bool IsEmpty => Lines.Count == 0;

        /// <summary>The raw text of the envelope of the origin of the classified reread; null for <see cref="Of"/> and without admission.</summary>
        public string OriginText { get; }

        /// <summary>The diagnostic of a reread that is not admitted, which is then not classified; null otherwise.</summary>
        public string Diagnostic { get; }

        /// <summary>True when the reread was admitted and classified.</summary>
        public bool IsAdmitted => Diagnostic == null;

        /// <summary>The lines of a classification: each group once, in the order of <see cref="PushBackChangeGroup"/>, joined with commas and a last " y ".</summary>
        public static PushBackKeepMinePresentation Of(PushBackConflictClassification classification)
        {
            if (classification == null) throw new ArgumentNullException(nameof(classification));

            var lines = new List<string>();
            AddGroups(lines, "Cambiado fuera: ", classification.ChangedExternally);
            AddGroups(lines, "Cambiado por ti: ", classification.ChangedByUser);
            AddGroups(lines, "Cambiado en los dos: ", classification.Overlapping);
            AddGroups(lines, "Cambios estructurales: ", classification.Structural);

            var erased = classification.ViewsToErase.Count;
            if (erased == 1) lines.Add("Vistas: se borrará 1 lateral obsoleto.");
            else if (erased > 1) lines.Add("Vistas: se borrarán " + erased.ToString(CultureInfo.InvariantCulture) + " laterales obsoletos.");

            var rejected = classification.ViewsRejected.Count;
            if (rejected == 1) lines.Add("Vistas: 1 no se puede conservar.");
            else if (rejected > 1) lines.Add("Vistas: " + rejected.ToString(CultureInfo.InvariantCulture) + " no se pueden conservar.");

            if (classification.AllViewsObsolete) lines.Add("Todas las vistas quedarían obsoletas.");
            return new PushBackKeepMinePresentation(lines.AsReadOnly(), null, null);
        }

        /// <summary>
        /// Classifies the draft against the reread of its rack: the base is the initial state of the draft, the current rack is the state of the
        /// reread, the draft is its current state, and the members are the mutable envelopes of the base of the reread. A reread that is not
        /// admitted is not classified: the presentation carries its diagnostic and no line. It changes neither the draft nor the reread.
        /// </summary>
        public static PushBackKeepMinePresentation Read(DraftSession<PushBackFullDraftState> draft, PushBackFullIntentResult read, RackCatalog catalog)
        {
            if (draft == null) throw new ArgumentNullException(nameof(draft));
            if (read == null) throw new ArgumentNullException(nameof(read));
            if (!read.Read.IsAdmitted)
            {
                return new PushBackKeepMinePresentation(
                    Array.Empty<string>(), null, read.Read.Admission.Diagnostic ?? PushBackPilotIntentReader.MaterializeDiagnostic);
            }

            var current = read.Read.InitialState;
            var envelopes = new RackEmbedStore();
            var members = new List<KeyValuePair<string, RackEmbedDocument>>();
            foreach (var entry in read.StaleBase.Entries.Values
                         .Where(e => e.Membership == DraftMembership.Mutable && e.EnvelopeText != null)
                         .OrderBy(e => e.Key, StringComparer.Ordinal))
            {
                var embed = envelopes.Deserialize(entry.EnvelopeText);
                if (embed != null) members.Add(new KeyValuePair<string, RackEmbedDocument>(entry.Key, embed));
            }

            var classification = PushBackConflictClassifier.Classify(draft.InitialDraftState, current, draft.CurrentDraftState, members, catalog);
            return new PushBackKeepMinePresentation(Of(classification).Lines, current.Origin, null);
        }

        private static void AddGroups(List<string> lines, string prefix, IReadOnlyList<PushBackChangeEntry> entries)
        {
            var groups = entries.Select(entry => entry.Group).Distinct().OrderBy(group => (int)group).Select(TextOf).ToList();
            if (groups.Count == 0) return;
            lines.Add(prefix + Join(groups) + ".");
        }

        private static string Join(IReadOnlyList<string> items)
            => items.Count == 1 ? items[0] : string.Join(", ", items.Take(items.Count - 1)) + " y " + items[items.Count - 1];

        private static string TextOf(PushBackChangeGroup group)
        {
            switch (group)
            {
                case PushBackChangeGroup.Name: return "el nombre";
                case PushBackChangeGroup.Rack: return "el rack";
                case PushBackChangeGroup.Annotations: return "las anotaciones";
                case PushBackChangeGroup.Safety: return "la seguridad";
                case PushBackChangeGroup.Fronts: return "los frentes";
                case PushBackChangeGroup.Modules: return "los módulos";
                case PushBackChangeGroup.SideB: return "el lado B";
                case PushBackChangeGroup.Composite: return "la parte compuesta";
                default: return "fuera del editor";
            }
        }
    }
}
