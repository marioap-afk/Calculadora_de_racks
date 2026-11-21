
    /// <summary>
    /// A result of the read of the pilot built by hand, so a test can give it any base and any admission (D-S3: the type of the read stays). It is
    /// registered with its full twin, which has the same base, the same admission and, when admitted, the full state of pb-01-base: the reread
    /// that PREPARE and the drafts of this kit see is the one the test asked for.
    /// </summary>
    public static PushBackPilotIntentResult Reread(DraftStaleBase staleBase, PushBackPilotDraftState? state = null, string? notAdmitted = null)
    {
        var pilot = new PushBackPilotIntentResult(
            staleBase,
            notAdmitted == null
                ? DraftIntentRead<PushBackPilotDraftState>.Admitted(staleBase, state ?? PilotKit.State("pb-01-base"))
                : DraftIntentRead<PushBackPilotDraftState>.NotAdmitted(notAdmitted),
            "A1",
            null,
            notAdmitted == null ? null : PushBackPilotIntentReader.ClassicEditorHint,
            PushBackPilotPreviewWitnessStatus.NotSupplied);
        FullHandKit.Twin(
            pilot,
            new PushBackFullIntentResult(
                staleBase,
                notAdmitted == null
                    ? DraftIntentRead<PushBackFullDraftState>.Admitted(staleBase, FullHandKit.FullState("pb-01-base"))
                    : DraftIntentRead<PushBackFullDraftState>.NotAdmitted(notAdmitted),
                "A1",
                null!,
                notAdmitted == null ? null! : PushBackPilotIntentReader.ClassicEditorHint));
        return pilot;
    }

    /// <summary>A full draft begun with the full read of the base of <paramref name="read"/>.</summary>
    public static DraftSession<PushBackFullDraftState> DraftOf(PushBackPilotIntentResult read, string rack = Rack, SessionId? session = null)
    {
        var begun = new DraftRegistry().Begin(
            session ?? DraftKit.S1, rack, FullHandKit.FullOf(read)!.Read, PushBackFullDraftStateComparer.Instance, PushBackFullDraftStateComparer.Copy);
        Assert.Equal(DraftBeginStatus.Created, begun.Status);
        return begun.Draft!;
    }

    /// <summary>A full draft whose base is the given one and whose initial state is materialized from the given origin text.</summary>
    public static DraftSession<PushBackFullDraftState> DraftOf(DraftStaleBase staleBase, string originText)
    {
        var begun = new DraftRegistry().Begin(
            DraftKit.S1, Rack, DraftIntentRead<PushBackFullDraftState>.Admitted(staleBase, FullHandKit.FullOfText(originText)),
            PushBackFullDraftStateComparer.Instance, PushBackFullDraftStateComparer.Copy);
        Assert.Equal(DraftBeginStatus.Created, begun.Status);
        return begun.Draft!;
    }

    public static void SetScale(DraftSession<PushBackFullDraftState> draft, double scale) =>
        draft.CurrentDraftState.Editable.AnnotationScale = scale;

    /// <summary>The full PREPARE of the draft, over the full read of the reread that the test hands in (null stays null).</summary>
    public static PushBackPilotPrepareResult Prepare(DraftSession<PushBackFullDraftState> draft, PushBackPilotIntentResult? reread) =>
        PushBackPilotUpdatePlanner.PrepareFull(PushBackFullPrepareInput.From(draft), FullHandKit.FullOf(reread)!, PilotKit.Catalog);
}

/// <summary>
/// I-64 F4B PB-T4b (D-S1): what the equivalents written by hand need beyond the kit. The full read of a read of the pilot, over the same base;
/// the read of a test with the status of the witness of the presentation, as the reader of the Plugin evaluates it; the full PREPARE over a read
/// of the pilot; and a read whose full read cannot be opened.
/// </summary>
internal static class FullHandKit
{
    private static readonly System.Runtime.CompilerServices.ConditionalWeakTable<PushBackPilotIntentResult, PushBackFullIntentResult> Twins = new();

    /// <summary>The full state of a fixture of the kit, materialized by the full model.</summary>
    public static PushBackFullDraftState FullState(string fixture) => FullOfText(PilotKit.Text(fixture));

    /// <summary>The full state materialized from an envelope text; a text that the full model does not admit fails by assertion.</summary>
    public static PushBackFullDraftState FullOfText(string originText)
    {
        var materialized = PushBackFullDraftMaterializer.Materialize(originText, PilotKit.Catalog);
        Assert.True(materialized.IsAdmitted, materialized.Reason);
        return materialized.State;
    }

    /// <summary>Registers the full twin of a read of the pilot built by hand.</summary>
    public static void Twin(PushBackPilotIntentResult pilot, PushBackFullIntentResult full) => Twins.AddOrUpdate(pilot, full);

    /// <summary>
    /// The full read of the SAME base as a read of the pilot: the twin of a read built by hand, or else the decision of PushBackFullDraftReader
    /// over that base (what the pilot refuses, except the two-sided refusal, stays refused with its diagnostic; anything else is the
    /// materialization of the full model from the text of the origin in the base).
    /// </summary>
    public static PushBackFullIntentResult? FullOf(PushBackPilotIntentResult? pilot)
    {
        if (pilot == null) return null;
        if (Twins.TryGetValue(pilot, out var twin)) return twin;
        var twoSided = !pilot.Read.IsAdmitted
                       && string.Equals(pilot.Read.Admission.Diagnostic, PushBackPilotIntentReader.OutOfPilotDiagnostic, StringComparison.Ordinal)
                       && string.Equals(pilot.Detail, PushBackPilotMaterializer.CompositeOutOfPilotReason, StringComparison.Ordinal);
        if (!pilot.Read.IsAdmitted && !twoSided)
        {
            return new PushBackFullIntentResult(
                pilot.StaleBase,
                DraftIntentRead<PushBackFullDraftState>.NotAdmitted(pilot.Read.Admission.Diagnostic ?? PushBackPilotIntentReader.MaterializeDiagnostic),
                pilot.OriginKey,
                pilot.Detail!,
                pilot.ClassicEditorHint!);
        }

        var origin = pilot.StaleBase.Entries.TryGetValue(pilot.OriginKey, out var entry) ? entry.EnvelopeText : null;
        var full = PushBackFullDraftMaterializer.Materialize(origin ?? string.Empty, PilotKit.Catalog);
        return full.IsAdmitted
            ? new PushBackFullIntentResult(pilot.StaleBase, DraftIntentRead<PushBackFullDraftState>.Admitted(pilot.StaleBase, full.State), pilot.OriginKey, null!, null!)
            : new PushBackFullIntentResult(
                pilot.StaleBase,
                DraftIntentRead<PushBackFullDraftState>.NotAdmitted(PushBackPilotIntentReader.MaterializeDiagnostic),
                pilot.OriginKey,
                full.Reason,
                PushBackPilotIntentReader.ClassicEditorHint);
    }

    /// <summary>The read of the pilot and its full read over the same base, as the reader of the Plugin hands them over (FromReads).</summary>
    public static EditIntentReadResult ReadOfPilot(PushBackPilotIntentResult pilot) =>
        EditIntentReadResult.FromReads(pilot, FullOf(pilot)!, PilotKit.Catalog);

    /// <summary>The full PREPARE over the full read of a read of the pilot (null stays null).</summary>
    public static PushBackPilotPrepareResult Prepare(PushBackFullPrepareInput input, PushBackPilotIntentResult? reread, RackCatalog? catalog) =>
        PushBackPilotUpdatePlanner.PrepareFull(input, FullOf(reread)!, catalog!);

    /// <summary>
    /// The same read, with the status of the witness of the preview evaluated against <paramref name="witness"/> as the reader of the pilot does on
    /// the scan: Unchanged with the key of the origin and its raw text, Changed otherwise.
    /// </summary>
    public static EditIntentReadResult Seen(EditIntentReadResult read, EditIntentWitness witness)
    {
        var pilot = read.PilotRead!;
        var origin = pilot.StaleBase.Entries.TryGetValue(pilot.OriginKey, out var entry) ? entry.EnvelopeText : null;
        var status = witness != null
                     && string.Equals(witness.Key, pilot.OriginKey, StringComparison.Ordinal)
                     && witness.Text != null
                     && string.Equals(witness.Text, origin, StringComparison.Ordinal)
            ? PushBackPilotPreviewWitnessStatus.Unchanged
            : PushBackPilotPreviewWitnessStatus.Changed;
        var seen = new PushBackPilotIntentResult(pilot.StaleBase, pilot.Read, pilot.OriginKey, pilot.Detail, pilot.ClassicEditorHint, status);
        return EditIntentReadResult.FromReads(seen, read.FullRead!, read.Catalog!);
    }

    /// <summary>A read whose full read keeps its real base and has no read, so the expression of its diagnostic throws; built without its constructor.</summary>
    public static EditIntentReadResult ReadWithoutFullRead(DraftStaleBase staleBase)
    {
        var broken = (PushBackFullIntentResult)System.Runtime.CompilerServices.RuntimeHelpers.GetUninitializedObject(typeof(PushBackFullIntentResult));
        var field = typeof(PushBackFullIntentResult).GetField("<StaleBase>k__BackingField", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
        Assert.NotNull(field);
        field!.SetValue(broken, staleBase);
        Assert.Throws<NullReferenceException>(() => broken.Read.IsAdmitted);
        return EditIntentReadResult.FromReads(FullEditIntentKit.Admitted(FullEditIntentKit.RackA).PilotRead!, broken, PilotKit.Catalog);
    }
}
