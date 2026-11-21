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

        /// <summary>INERT in the RED of PB-T4b: no line.</summary>
        public static PushBackKeepMinePresentation Of(PushBackConflictClassification classification) =>
            new PushBackKeepMinePresentation(Array.Empty<string>(), null, null);

        /// <summary>INERT in the RED of PB-T4b: no line.</summary>
        public static PushBackKeepMinePresentation Read(DraftSession<PushBackFullDraftState> draft, PushBackFullIntentResult read, RackCatalog catalog) =>
            new PushBackKeepMinePresentation(Array.Empty<string>(), null, null);
    }
}
