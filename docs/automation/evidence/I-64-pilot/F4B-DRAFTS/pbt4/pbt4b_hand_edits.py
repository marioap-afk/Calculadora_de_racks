"""PB-T4b (paquete v4.4; D-S1 y la decisión del Owner sobre las cinco roturas): el texto literal de los equivalentes escritos a mano.

Cada equivalente es su prueba original con, en este orden:
1. en las 9 clases de C-0, el mapa cerrado M (copy_map_defs.py), como las copias mecánicas;
2. las sustituciones uniformes H que su fila nombra (abajo, cerradas);
3. sus ediciones literales: (texto viejo, texto nuevo, veces), cada una sobre el texto ya transformado, y cada una tiene que casar exactamente las
   veces que dice (si no, el generador para).
Una prueba «a mano» sin H ni ediciones es su copia mapeada: el mapa basta, y se mide en la precondición 6 como las demás.
Las guardas (C-5 y las cinco nuevas) no llevan M: su copia parcial lleva todos los miembros que no son pruebas de su clase (con las ediciones de
ayudante de HELPER_EDITS) y solo sus pruebas de la lista, con sus ediciones. Las 27 de la interfaz son copias sin cambios.
"""

# ---------------------------------------------------------------- H: sustituciones uniformes de los equivalentes a mano (cerradas)
H = {
    'H-01': ('PushBackPilotUpdatePlanner.Prepare(', 'FullHandKit.Prepare('),
    'H-02': ('PushBackPilotPrepareInput.From(', 'PushBackFullPrepareInput.From('),
    'H-03': ('PushBackPilotMaterializer.ComposeForWrite(', 'PushBackFullComposition.ForFullWrite('),
    'H-04': ('EditIntentReadResult.FromPilot(', 'FullHandKit.ReadOfPilot('),
}

WPP = 'tests/RackCad.Tests/Workspace/WorkspacePilotPrepareTests.cs'
EIH = 'tests/RackCad.Tests/Workspace/WorkspaceEditIntentHostTests.cs'
WPRH = 'tests/RackCad.Tests/Workspace/WorkspacePilotReconciliationHostTests.cs'

_SEEN_WPP = 'rig.Reader.Answer = rack => FullHandKit.Seen(Changed(rack), presented.KeepWitness!);\n'

# ---------------------------------------------------------------- C-0: los 60 a mano (53 de la v4.3 y los 7 de D-S1) con algo más que M
# (archivo, método) -> (lista de H, lista de ediciones). Las que no están aquí no llevan más que M.
HAND = {
    # --- WPP, R-C3: el primer TryKeepMine sin testigo presenta (D-PBT4-08)
    (WPP, 'EachOperationReadsOnceAndSetsTheMarkOfReadInProgressAroundTheReadAndResetsItAfterwards'): ([], [
        ('        Assert.Equal(new[] { "Confirmed", "Kept", "Reloaded" }, results);\n',
         '        // I-64 F4B PB-T4b (D-PBT4-08): «Conservar» sin testigo presenta y no cambia el borrador.\n'
         '        Assert.Equal(new[] { "Confirmed", "Presented", "Reloaded" }, results);\n', 1),
    ]),
    # --- WPP, R-retirado: KeepPilotEditsOn -> KeepOn, en dos tiempos
    (WPP, 'KeepAfterAChangeOfTheDrawingStaysOnTheNewVersionWithTheScaleAndThePendingTextOfTheDraft'): ([], [
        ('        var expectedCurrent = PushBackPilotMaterializer.KeepPilotEditsOn(\n'
         '            PilotKit.Admitted(IntentKit.Variant("pb-02-modulos", RackA)), draft.CurrentDraftState);\n'
         '\n'
         '        var outcome = rig.Core.TryKeepPilotEdit(rig.Request());\n'
         '\n'
         '        Assert.Equal(EditIntentStatus.Kept, outcome.Status);\n'
         '        Assert.Equal(EditWitnessStatus.NotSupplied, outcome.Witness);\n',
         '        var expectedCurrent = PushBackFullComposition.KeepOn(Changed(RackA).FullRead!.Read.InitialState!, draft.CurrentDraftState);\n'
         '\n'
         '        // I-64 F4B PB-T4b (D-PBT4-08): the first call presents and changes nothing; the second, with the witness of that read, keeps.\n'
         '        var presented = rig.Core.TryKeepPilotEdit(rig.Request());\n'
         '        Assert.Equal(EditIntentStatus.Presented, presented.Status);\n'
         '        Assert.True(draft.IsPossiblyStale);\n'
         '        ' + _SEEN_WPP +
         '        var outcome = rig.Core.TryKeepPilotEdit(rig.Request(witness: presented.KeepWitness));\n'
         '\n'
         '        Assert.Equal(EditIntentStatus.Kept, outcome.Status);\n'
         '        Assert.Equal(EditWitnessStatus.Unchanged, outcome.Witness);\n', 1),
    ]),
    # --- WPP, R-retirado: el PREPARE completo
    (WPP, 'PendingTextComesBeforeEverythingButNotApplicableAndBeforeTheReread'): (['H-01', 'H-02'], []),
    (WPP, 'ACleanDraftHasNothingToApplyAndTheRereadIsNotLookedAt'): (['H-01', 'H-02'], []),
    (WPP, 'InvalidD_ABuilderThatCannotBuildTheOutgoingSystemIsInvalidWithTheTypedErrorAndNothingIsWritten'): (['H-01', 'H-02'], []),
    (WPP, 'PrepareIsTotalAMissingPieceIsInvalidAndNeverAnException'): (['H-01', 'H-02'], []),
    (WPP, 'AMissingCatalogIsAnsweredAfterTheConflictAndTheAdmissionAndNotBeforeThem'): (['H-01', 'H-02'], []),
    (WPP, 'TheOutgoingDesignAndEnvelopeOfEachViewComeFromTheRereadEnvelopeAndNeverFromTheDraft'): (['H-03'], []),
    # --- WPP, R-C4: el contenido que el piloto no edita sí lo edita el modelo completo
    (WPP, 'InvalidA_ACurrentStateThatDoesNotComposeIsInvalidWithTheReasonOfTheCompositionAndOverTheCarryOversOfTheMaterializer'): ([], [
        ('        // Content that the pilot does not edit.\n'
         '        var content = FullPrepareKit.DraftOf(read);\n'
         '        FullPrepareKit.SetScale(content, 2.25);\n'
         '        content.CurrentDraftState.Editable.PalletTolerance += 1.0;\n'
         '        var rejected = FullPrepareKit.Prepare(content, read);\n'
         '        Assert.Equal(PushBackPilotPrepareKind.Invalid, rejected.Kind);\n'
         '        Assert.Equal(PushBackPilotMaterializer.NonPilotContentReason, rejected.Reason);\n',
         '        // I-64 F4B PB-T4b (D-PBT4-04): content that the pilot did not edit is edited by the full model, and it composes.\n'
         '        var content = FullPrepareKit.DraftOf(read);\n'
         '        FullPrepareKit.SetScale(content, 2.25);\n'
         '        content.CurrentDraftState.Editable.PalletTolerance += 1.0;\n'
         '        var accepted = FullPrepareKit.Prepare(content, read);\n'
         '        Assert.True(accepted.Kind == PushBackPilotPrepareKind.Ready, accepted.Reason);\n'
         '        Assert.NotNull(accepted.Plan);\n', 1),
    ]),
    # --- WPP, R-estado: el estado completo se construye con su editable
    (WPP, 'InvalidA_AnOriginThatNoLongerMaterializesIsInvalidWithTheReasonOfTheMaterializerBeforeAnythingElse'): ([], [
        ('        var state = PilotKit.State("pb-01-base");\n', '        var editable = FullHandKit.FullState("pb-01-base").Editable;\n', 1),
        ('new PushBackFullDraftState(state.Editor, state.Inputs, origin)', 'new PushBackFullDraftState(editable, origin)', 1),
    ]),
    # --- WPP, D-S1: las cuatro copias que no podían pasar
    (WPP, 'KeepAppliesToConflictAndToDirtyMarkedStaleApplyingWaitsAndTheOthersAreNotNeededAllWithoutReading'): ([], [
        ('                Assert.Equal(EditIntentStatus.Kept, outcome.Status);\n'
         '                Assert.Single(rig.Reader.Calls);\n',
         '                // I-64 F4B PB-T4b (D-S1, D-PBT4-08): the first call presents; the second, with the witness of that read, keeps.\n'
         '                Assert.Equal(EditIntentStatus.Presented, outcome.Status);\n'
         '                Assert.Single(rig.Reader.Calls);\n'
         '                rig.Reader.Answer = rack => FullHandKit.Seen(Changed(rack), outcome.KeepWitness!);\n'
         '                Assert.Equal(EditIntentStatus.Kept, rig.Core.TryKeepPilotEdit(rig.Request(witness: outcome.KeepWitness)).Status);\n'
         '                Assert.Equal(2, rig.Reader.Calls.Count);\n', 1),
    ]),
    (WPP, 'KeepAConflictLeavesItAndTheDraftIsDirtyOnTheNewVersion'): ([], [
        ('        var outcome = rig.Core.TryKeepPilotEdit(rig.Request());\n'
         '\n'
         '        Assert.Equal(EditIntentStatus.Kept, outcome.Status);\n',
         '        // I-64 F4B PB-T4b (D-S1, D-PBT4-08): the first call presents and leaves the conflict; the second, with the witness, keeps.\n'
         '        var presented = rig.Core.TryKeepPilotEdit(rig.Request());\n'
         '        Assert.Equal(EditIntentStatus.Presented, presented.Status);\n'
         '        Assert.Equal(DraftStatus.Conflict, draft.Status);\n'
         '        ' + _SEEN_WPP +
         '        var outcome = rig.Core.TryKeepPilotEdit(rig.Request(witness: presented.KeepWitness));\n'
         '\n'
         '        Assert.Equal(EditIntentStatus.Kept, outcome.Status);\n', 1),
    ]),
    (WPP, 'AKeepThatIsKeptEndsTheMarkAndOneThatIsNotNeededOrNotAdmittedDoesNot'): ([], [
        ('            rig.Reader.Answer = Changed;\n'
         '            Assert.Equal(EditIntentStatus.Kept, rig.Core.TryKeepPilotEdit(rig.Request()).Status);\n',
         '            rig.Reader.Answer = Changed;\n'
         '            // I-64 F4B PB-T4b (D-S1, D-PBT4-08): the presentation does not end the mark; the keep with the witness does.\n'
         '            var presented = rig.Core.TryKeepPilotEdit(rig.Request());\n'
         '            Assert.Equal(EditIntentStatus.Presented, presented.Status);\n'
         '            ' + _SEEN_WPP +
         '            Assert.Equal(EditIntentStatus.Kept, rig.Core.TryKeepPilotEdit(rig.Request(witness: presented.KeepWitness)).Status);\n', 1),
    ]),
    (WPP, 'TheNewOutcomesAreValuesWithTheStatesOfThePackageAndTheDefaultIsUnavailable'): ([], [
        ('        // Kept is the one new state of the intention, after the ten that stay as an ordered prefix; Reloaded is reused by the discard.\n'
         '        var intent = Enum.GetNames<EditIntentStatus>();\n'
         '        Assert.Equal(11, intent.Length);\n'
         '        Assert.Equal("Kept", intent[10]);\n',
         '        // Kept comes after the ten that stay as an ordered prefix; Reloaded is reused by the discard. I-64 F4B PB-T4b (D-S1, D-PBT4-08):\n'
         '        // Presented, the presentation of the keep, comes after Kept, with the eleven as an ordered prefix.\n'
         '        var intent = Enum.GetNames<EditIntentStatus>();\n'
         '        Assert.Equal(12, intent.Length);\n'
         '        Assert.Equal("Kept", intent[10]);\n'
         '        Assert.Equal("Presented", intent[11]);\n', 1),
        ('        Assert.Equal(EditWitnessStatus.Changed, EditIntentOutcome.Kept(EditWitnessStatus.Changed).Witness);\n',
         '        Assert.Equal(EditWitnessStatus.Changed, EditIntentOutcome.Kept(EditWitnessStatus.Changed).Witness);\n'
         '        Assert.Empty(EditIntentOutcome.Kept(EditWitnessStatus.Changed).PresentationLines);\n'
         '        Assert.Null(EditIntentOutcome.Kept(EditWitnessStatus.Changed).KeepWitness);\n'
         '        var keepWitness = new EditIntentWitness("A1", "texto");\n'
         '        var presented = EditIntentOutcome.Presented(new[] { "Cambiado fuera: el nombre." }, keepWitness, EditWitnessStatus.NotSupplied);\n'
         '        Assert.Equal(EditIntentStatus.Presented, presented.Status);\n'
         '        Assert.Equal(new[] { "Cambiado fuera: el nombre." }, presented.PresentationLines.ToArray());\n'
         '        Assert.Same(keepWitness, presented.KeepWitness);\n'
         '        Assert.Null(presented.Reason);\n', 1),
    ]),
    # --- EIH, R-C9
    (EIH, 'CheckNowUsesTheBaseOfARereadThatIsNotAdmittedToo'): (['H-04'], []),
    # --- WPRH, R-C9 (y R-C2,R-C9)
    (WPRH, 'ConfirmOfARackThatIsNoLongerEditableWithTheSameBaseIsConfirmedWithTheDiagnosticAndTheHint'): (['H-04'], []),
    (WPRH, 'ConfirmOfARackThatIsNoLongerEditableWithAnotherBaseIsAConflictWithBothThings'): (['H-04'], []),
    (WPRH, 'AnExceptionInsideTheComparisonOfTheGeneralCheckIsFailedReportedAndChangesNothing'): (['H-04'], []),
    (WPRH, 'CheckNowWithARereadThatIsNotAdmittedAndMatchesTheBaseIsRecoveredAndCarriesTheDiagnosticAndTheHint'): (['H-04'], []),
    (WPRH, 'CheckNowWithARereadThatIsNotAdmittedAndDiffersFromTheBaseIsAConflictAndCarriesTheDiagnosticAndTheHint'): (['H-04'], []),
    (WPRH, 'TheDiagnosticNeverDecidesTheClassificationTheSameBaseGivesTheSameAnswerAndTheSameDraftWithOrWithoutAdmission'): (['H-04'], []),
    (WPRH, 'AFailureOfTheDiagnosticGivesNullAndNeverChangesTheClassificationNorTurnsAChangedDraftIntoFailed'): ([], [
        ('EditIntentReadResult.FromPilot(WithoutRead(sameBase))', 'FullHandKit.ReadWithoutFullRead(sameBase)', 1),
        ('EditIntentReadResult.FromPilot(WithoutRead(anotherBase))', 'FullHandKit.ReadWithoutFullRead(anotherBase)', 1),
    ]),
    (WPRH, 'TheDiagnosticOfCheckNowReadsTheDrawingExactlyOnceAndWritesNothingAndANotAdmittedRereadStillReadsOnce'): (['H-04'], []),
    # --- WPRH, D-S1: las tres copias que no podían pasar
    (WPRH, 'ConfirmWithAChangeOfTheScaleAndOfAnotherFieldIsAConflictAndKeepAfterwardsKeepsTheScaleOfTheDraftOnTheNewVersion'): ([], [
        ('        var keep = rig.Core.TryKeepPilotEdit(rig.Request());\n'
         '\n'
         '        Assert.Equal(EditIntentStatus.Kept, keep.Status);\n',
         '        // I-64 F4B PB-T4b (D-S1, D-PBT4-08): the first call presents and leaves the conflict; the second, with the witness, keeps.\n'
         '        var presented = rig.Core.TryKeepPilotEdit(rig.Request());\n'
         '        Assert.Equal(EditIntentStatus.Presented, presented.Status);\n'
         '        Assert.Equal(DraftStatus.Conflict, draft.Status);\n'
         '        rig.Reader.Answer = rack => FullHandKit.Seen(OriginChanged(rack, (ScaleFrom, ScaleTo), (ToleranceFrom, ToleranceTo)), presented.KeepWitness!);\n'
         '        var keep = rig.Core.TryKeepPilotEdit(new EditIntentRequest(rig.Session, RackA, null, presented.KeepWitness));\n'
         '\n'
         '        Assert.Equal(EditIntentStatus.Kept, keep.Status);\n', 1),
    ]),
    (WPRH, 'AnInvalidPendingTextDoesNotBlockTheThreeOperationsConfirmAndKeepLeaveItExactlyAsTypedAndDiscardEmptiesIt'): ([], [
        ('        Assert.Equal(EditIntentStatus.Kept, keepRig.Core.TryKeepPilotEdit(keepRig.Request()).Status);\n',
         '        // I-64 F4B PB-T4b (D-S1, D-PBT4-08): the first call presents; the second, with the witness of that read, keeps.\n'
         '        var presented = keepRig.Core.TryKeepPilotEdit(keepRig.Request());\n'
         '        Assert.Equal(EditIntentStatus.Presented, presented.Status);\n'
         '        Assert.Equal(InvalidText, keepDraft.PendingText["field"]);\n'
         '        keepRig.Reader.Answer = rack => FullHandKit.Seen(OriginChanged(rack, (ToleranceFrom, ToleranceTo)), presented.KeepWitness!);\n'
         '        Assert.Equal(EditIntentStatus.Kept, keepRig.Core.TryKeepPilotEdit(new EditIntentRequest(keepRig.Session, RackA, null, presented.KeepWitness)).Status);\n', 1),
    ]),
    (WPRH, 'TheThreeOperationsStillReadTheDrawingExactlyOnceAndTheGeneralCheckStillComparesWithTheBaseOfTheDraft'): ([], [
        ('        Assert.Equal(EditIntentStatus.Kept, rig.Core.TryKeepPilotEdit(rig.Request()).Status);\n'
         '        Assert.Single(rig.Reader.Calls);\n',
         '        // I-64 F4B PB-T4b (D-S1, D-PBT4-08): each of the two calls of the keep reads the drawing exactly once.\n'
         '        var presented = rig.Core.TryKeepPilotEdit(rig.Request());\n'
         '        Assert.Equal(EditIntentStatus.Presented, presented.Status);\n'
         '        Assert.Single(rig.Reader.Calls);\n'
         '        rig.Reader.Calls.Clear();\n'
         '        rig.Reader.Answer = rack => FullHandKit.Seen(OriginChanged(rack, (ToleranceFrom, ToleranceTo)), presented.KeepWitness!);\n'
         '        Assert.Equal(EditIntentStatus.Kept, rig.Core.TryKeepPilotEdit(new EditIntentRequest(rig.Session, RackA, null, presented.KeepWitness)).Status);\n'
         '        Assert.Single(rig.Reader.Calls);\n', 1),
    ]),
}

# Las siete de D-S1 (las que la v4.3 daba por copias mecánicas), en el orden del mapa de la copia.
DS1 = [
    (WPP, 'KeepAppliesToConflictAndToDirtyMarkedStaleApplyingWaitsAndTheOthersAreNotNeededAllWithoutReading'),
    (WPP, 'KeepAConflictLeavesItAndTheDraftIsDirtyOnTheNewVersion'),
    (WPP, 'AKeepThatIsKeptEndsTheMarkAndOneThatIsNotNeededOrNotAdmittedDoesNot'),
    (WPP, 'TheNewOutcomesAreValuesWithTheStatesOfThePackageAndTheDefaultIsUnavailable'),
    (WPRH, 'ConfirmWithAChangeOfTheScaleAndOfAnotherFieldIsAConflictAndKeepAfterwardsKeepsTheScaleOfTheDraftOnTheNewVersion'),
    (WPRH, 'AnInvalidPendingTextDoesNotBlockTheThreeOperationsConfirmAndKeepLeaveItExactlyAsTypedAndDiscardEmptiesIt'),
    (WPRH, 'TheThreeOperationsStillReadTheDrawingExactlyOnceAndTheGeneralCheckStillComparesWithTheBaseOfTheDraft'),
]

# ---------------------------------------------------------------- las guardas: C-5 (sin CT-PBT4A-06, que sustituye CT-PBT4B-13) y las cinco nuevas
WPG = 'tests/RackCad.Tests/PushBackWritePathGuardTests.cs'
IG = 'tests/RackCad.Tests/Workspace/WorkspacePilotUpdateIntegrationGuardTests.cs'
EIG = 'tests/RackCad.Tests/Workspace/WorkspaceEditIntentGuardTests.cs'
PEG = 'tests/RackCad.Tests/Workspace/WorkspacePilotEditGuardTests.cs'
RG = 'tests/RackCad.Tests/Workspace/WorkspacePilotReconciliationGuardTests.cs'
WLG = 'tests/RackCad.Tests/Workspace/WorkspaceLifecycleGuardTests.cs'
WPUG = 'tests/RackCad.Tests/Workspace/WorkspacePilotUpdateGuardTests.cs'

# Ediciones de los miembros que no son pruebas (los ayudantes), en la copia parcial de la guarda: el modelo completo donde la guarda nombraba
# el piloto. Son las únicas diferencias de esos miembros con el original.
HELPER_EDITS = {
    WPG: [
        ('            => relative == PlanPath || relative == RunPath || relative.StartsWith(DraftFolder, StringComparison.Ordinal);\n',
         '            => relative == PlanPath || relative == RunPath || relative == HostPath || relative.StartsWith(DraftFolder, StringComparison.Ordinal);\n', 1),
        ('found.All(argument => argument == "PushBackPilotDraftState")', 'found.All(argument => argument == "PushBackFullDraftState")', 1),
    ],
    IG: [
        ('        var runCallers = product.Where(f => Regex.IsMatch(f.Code, @"PushBackPilotUpdateRun\\.Execute\\s*\\(")).Select(f => f.Path).ToArray();\n'
         '        if (!runCallers.SequenceEqual(new[] { CorePath })) yield return "PushBackPilotUpdateRun.Execute is named in " + string.Join(", ", runCallers);\n',
         '        var runCallers = product.Where(f => Regex.IsMatch(f.Code, @"PushBackPilotUpdateRun\\.ExecuteFull\\s*\\(")).Select(f => f.Path).ToArray();\n'
         '        if (!runCallers.SequenceEqual(new[] { CorePath })) yield return "PushBackPilotUpdateRun.ExecuteFull is named in " + string.Join(", ", runCallers);\n', 1),
    ],
    EIG: [
        ('    private static readonly Regex BaseOrReadUse = new(@"\\.StaleBase\\b|\\.InitialState\\b|\\b(?:pilot|result)\\.Read\\b|\\bpilot\\s*[?!]?\\s*\\.\\s*Read\\b|\\.Entries\\b");\n',
         '    private static readonly Regex BaseOrReadUse = new(@"\\.StaleBase\\b|\\.InitialState\\b|\\b(?:full|result)\\.Read\\b|\\bfull\\s*[?!]?\\s*\\.\\s*Read\\b|\\.Entries\\b");\n', 1),
        ('    private const string AdmissionDiagnosticPattern = @"\\bpilot\\.Read\\.IsAdmitted\\s*\\?\\s*null\\s*:\\s*pilot\\.Read\\.Admission\\.Diagnostic\\b";\n',
         '    private const string AdmissionDiagnosticPattern = @"\\bfull\\.Read\\.IsAdmitted\\s*\\?\\s*null\\s*:\\s*full\\.Read\\.Admission\\.Diagnostic\\b";\n', 1),
        ('    private const string AnotherDiagnosticExpression = "\\nvar again = pilot.Read.IsAdmitted ? null : pilot.Read.Admission.Diagnostic;";\n',
         '    private const string AnotherDiagnosticExpression = "\\nvar again = full.Read.IsAdmitted ? null : full.Read.Admission.Diagnostic;";\n', 1),
        ('    private static readonly Regex PilotReadAssignment = new(@"var\\s+pilot\\s*=\\s*read\\.PilotRead\\s*;");\n',
         '    private static readonly Regex PilotReadAssignment = new(@"var\\s+pilot\\s*=\\s*read\\.PilotRead\\s*;");\n'
         '\n'
         '    // I-64 F4B PB-T4b: the full read of a result has ONE name in the host core, "full", assigned by one statement per operation that reads.\n'
         '    private static readonly Regex FullReadAssignment = new(@"var\\s+full\\s*=\\s*read\\.FullRead\\s*;");\n', 1),
    ],
    PEG: [
        ('        if (Regex.IsMatch(host, @"private\\s+(?:readonly\\s+)?PushBackPilotFieldAdapter\\s+_\\w+")) yield return "the adapter of the pilot is a field of instance";\n'
         '        if (Src.Count(host, @"private\\s+static\\s+readonly\\s+PushBackPilotFieldAdapter\\s+\\w+") != 1) yield return "the adapter of the pilot is not the one static field";\n',
         '        if (Regex.IsMatch(host, @"private\\s+(?:readonly\\s+)?PushBackFullFieldAdapter\\s+_\\w+")) yield return "the adapter of the field is a field of instance";\n'
         '        if (Src.Count(host, @"private\\s+static\\s+readonly\\s+PushBackFullFieldAdapter\\s+\\w+") != 1) yield return "the adapter of the field is not the one static field";\n', 1),
    ],
    RG: [],
    WLG: [],
    WPUG: [],
}

# (archivo, método) -> ediciones de la prueba en su copia (las que no están aquí, sin cambios). Los nombres del método no cambian.
GUARD_EDITS = {
    # CT-PBT3B-13 sobre el modelo completo: WHC puede nombrar la escritura completa y el anfitrión solo encuentra el borrador completo
    (WPG, 'CT_PBT3B_13_NoFileOutsideThePlanTheRunAndTheModelNamesTheFullWriteAndTheHostFindsOnlyTheDraftOfThePilot'): [
        ('            log.Add("host Find<PushBackPilotDraftState>-es-el-unico-Find<=" + OnlyThePilotDraftIsFound(host) + " busquedas>0=" + (FindArguments(host).Count > 0));\n',
         '            log.Add("host Find<PushBackFullDraftState>-es-el-unico-Find<=" + OnlyThePilotDraftIsFound(host) + " busquedas>0=" + (FindArguments(host).Count > 0));\n', 1),
        ('                "host Find<PushBackPilotDraftState>-es-el-unico-Find<=True busquedas>0=True",\n',
         '                "host Find<PushBackFullDraftState>-es-el-unico-Find<=True busquedas>0=True",\n', 1),
        ('            foreach (var destination in new[] { "WorkspaceHostCore.cs", "WorkspacePilotSupport.cs" })\n',
         '            // I-64 F4B PB-T4b: the host core runs the full write, so the self-tests inject the names in the support of the panel only.\n'
         '            foreach (var destination in new[] { "WorkspacePilotSupport.cs" })\n', 1),
        ('            log.Add("autoprueba-Find<PushBackFullDraftState>-en-WorkspaceHostCore.cs="\n'
         '                + (OnlyThePilotDraftIsFound(host + "\\n_drafts.Find<PushBackFullDraftState>(session, rackId);\\n") ? "pasa" : "falla"));\n'
         '            log.Add("control-Find<PushBackPilotDraftState>-en-WorkspaceHostCore.cs="\n'
         '                + (OnlyThePilotDraftIsFound(host + "\\n_drafts.Find<PushBackPilotDraftState>(session, rackId);\\n") ? "pasa" : "falla"));\n'
         '            expected.Add("autoprueba-Find<PushBackFullDraftState>-en-WorkspaceHostCore.cs=falla");\n'
         '            expected.Add("control-Find<PushBackPilotDraftState>-en-WorkspaceHostCore.cs=pasa");\n',
         '            log.Add("autoprueba-Find<PushBackPilotDraftState>-en-WorkspaceHostCore.cs="\n'
         '                + (OnlyThePilotDraftIsFound(host + "\\n_drafts.Find<PushBackPilotDraftState>(session, rackId);\\n") ? "pasa" : "falla"));\n'
         '            log.Add("control-Find<PushBackFullDraftState>-en-WorkspaceHostCore.cs="\n'
         '                + (OnlyThePilotDraftIsFound(host + "\\n_drafts.Find<PushBackFullDraftState>(session, rackId);\\n") ? "pasa" : "falla"));\n'
         '            expected.Add("autoprueba-Find<PushBackPilotDraftState>-en-WorkspaceHostCore.cs=falla");\n'
         '            expected.Add("control-Find<PushBackFullDraftState>-en-WorkspaceHostCore.cs=pasa");\n', 1),
    ],
    # G-4 con la corrida completa, y la autoprueba que IG:510-517 ya no discrimina (§17.2 de la revisión)
    (IG, 'G4OnlyTheExecuteOfTheRunOfTheCoreCallsTheUpdateAndOnlyTheCoreCallsTheRunOfT4B'): [
        ('        Assert.Empty(G4Violations(Product()));\n',
         '        Assert.Empty(G4Violations(Product()));\n'
         '        // I-64 F4B PB-T4b (the self-test of IG:510-517 over the full run): a caller of the run outside the core, a caller of the update\n'
         '        // outside the core and a second call in the core trip the guard.\n'
         '        Assert.NotEmpty(G4Violations(Append(Product(), HostRunPath, "var o = PushBackPilotUpdateRun.ExecuteFull(draft, request, port);")));\n'
         '        Assert.NotEmpty(G4Violations(Append(Product(), ControllerPath, "var u = _host.TryUpdatePilot(request, port);")));\n'
         '        Assert.NotEmpty(G4Violations(Append(Product(), AdapterPath, "var u = core.TryUpdatePilot(request, port);")));\n'
         '        Assert.NotEmpty(G4Violations(With(Product(), CorePath, code => code.Replace("public EditUpdateOutcome TryUpdatePilot(", '
         '"public EditUpdateOutcome Other() { return TryUpdatePilot(null!, null!); }\\n        public EditUpdateOutcome TryUpdatePilot(", StringComparison.Ordinal))));\n', 1),
    ],
    (IG, 'G4TripsOnAGroupOfMethodsOfTheUpdateOutsideTheCoreAndInsideItAndNotOnTheNameInsideAString'): [],
    # EIG: la base y la lectura del resultado completo
    (EIG, 'TheBaseOfAResultOfThePilotOnlyReachesBeginCheckNowOrDiscardAndReloadInTheHostCoreAndTheTwoOperationsCountTheirOwnUses'): [
        ('        Assert.Equal(1, Src.Count(intent, @"\\bpilot\\.Read\\b"));\n'
         '        Assert.Matches(@"\\.Begin\\s*\\(\\s*[^;]*?\\bpilot\\.Read\\b[^;]*?\\)\\s*;", intent);\n',
         '        Assert.Equal(1, Src.Count(intent, @"\\bfull\\.Read\\b"));\n'
         '        Assert.Matches(@"\\.Begin\\s*\\(\\s*[^;]*?\\bfull\\.Read\\b[^;]*?\\)\\s*;", intent);\n', 1),
        ('        Assert.Equal(2, Src.Count(check, @"\\bpilot\\.Read\\b"));\n'
         '        Assert.Equal(0, Src.Count(WithoutTheAllowedConsumers(check), @"\\bpilot\\.Read\\b"));\n',
         '        Assert.Equal(2, Src.Count(check, @"\\bfull\\.Read\\b"));\n'
         '        Assert.Equal(0, Src.Count(WithoutTheAllowedConsumers(check), @"\\bfull\\.Read\\b"));\n', 1),
    ],
    (EIG, 'ThePerOperationCountsOfTheBaseAndOfTheReadInTheThreeBodiesOfTheReconciliationAreTheirOwnConsumer'): [
        ('        Assert.Equal(2, Src.Count(confirm, @"\\bpilot\\.Read\\b"));\n'
         '        Assert.Equal(0, Src.Count(WithoutTheAllowedConsumers(confirm), @"\\bpilot\\.Read\\b"));\n',
         '        Assert.Equal(2, Src.Count(confirm, @"\\bfull\\.Read\\b"));\n'
         '        Assert.Equal(0, Src.Count(WithoutTheAllowedConsumers(confirm), @"\\bfull\\.Read\\b"));\n', 1),
        ('        Assert.Equal(1, Src.Count(keep, @"\\bpilot\\.Read\\b"));\n'
         '        Assert.Matches(@"\\.KeepMineOnCurrentVersion\\s*\\(\\s*pilot\\.Read\\s*,\\s*PushBackPilotMaterializer\\.KeepPilotEditsOn\\s*,\\s*confirmed\\s*:\\s*true\\s*\\)", keep);\n',
         '        Assert.Equal(1, Src.Count(keep, @"\\bfull\\.Read\\b"));\n'
         '        Assert.Matches(@"\\.KeepMineOnCurrentVersion\\s*\\(\\s*full\\.Read\\s*,\\s*PushBackFullComposition\\.KeepOn\\s*,\\s*confirmed\\s*:\\s*true\\s*\\)", keep);\n', 1),
        ('        Assert.Equal(1, Src.Count(discard, @"\\bpilot\\.Read\\b"));\n'
         '        Assert.Matches(@"\\.DiscardAndReload\\s*\\(\\s*pilot\\.Read\\s*\\)", discard);\n',
         '        Assert.Equal(1, Src.Count(discard, @"\\bfull\\.Read\\b"));\n'
         '        Assert.Matches(@"\\.DiscardAndReload\\s*\\(\\s*full\\.Read\\s*\\)", discard);\n', 1),
    ],
    (EIG, 'TheResultOfThePilotIsOnlyEverNamedPilotInTheHostCoreSoNoAliasEscapesTheGuardOfTheUseOfTheBase'): [
        ('        var withoutTheAssignments = PilotReadAssignment.Replace(core, string.Empty);\n'
         '\n'
         '        Assert.DoesNotMatch(new Regex(@"\\.PilotRead\\b"), withoutTheAssignments);\n'
         '        // One assignment per operation that reads: the intention, the check, the reload, the general check, the keep and the discard.\n'
         '        Assert.Equal(6, PilotReadAssignment.Matches(core).Count);\n',
         '        var withoutTheAssignments = FullReadAssignment.Replace(PilotReadAssignment.Replace(core, string.Empty), string.Empty);\n'
         '\n'
         '        // I-64 F4B PB-T4b: the full read is only ever named "full", once per operation that reads (the intention, the check, the reload,\n'
         '        // the general check, the keep and the discard), and the read of the pilot only for the witness of the preview (four).\n'
         '        Assert.DoesNotMatch(new Regex(@"\\.PilotRead\\b"), withoutTheAssignments);\n'
         '        Assert.DoesNotMatch(new Regex(@"\\.FullRead\\b"), withoutTheAssignments);\n'
         '        Assert.Equal(6, FullReadAssignment.Matches(core).Count);\n'
         '        Assert.Equal(4, PilotReadAssignment.Matches(core).Count);\n', 1),
    ],
    (EIG, 'TheThreeBodiesOfTheReconciliationFollowTheStepsOfTheCheckAndTheReloadWithTheApplicabilityBeforeTheOneRead'): [
        ('        // The keep is called with the function of the pilot and the confirmation that the gesture already is.\n'
         '        Assert.Contains("PushBackPilotMaterializer.KeepPilotEditsOn", keep);\n',
         '        // The keep is called with the function of the full model and the confirmation of the second "Si" with the witness.\n'
         '        Assert.Contains("PushBackFullComposition.KeepOn", keep);\n', 1),
    ],
    (EIG, 'TheBaseGuardAllowsTheConsumersOfTheObligation'): [
        ('    [InlineData("draft.DiscardAndReload(pilot.Read);")]\n', '    [InlineData("draft.DiscardAndReload(full.Read);")]\n', 1),
        ('    [InlineData("draft.DiscardAndReload(reload.Read, pilot.StaleBase);")]\n', '    [InlineData("draft.DiscardAndReload(reload.Read, full.StaleBase);")]\n', 1),
        ('    [InlineData("var check = draft.CheckNow(pilot.StaleBase);")]\n', '    [InlineData("var check = draft.CheckNow(full.StaleBase);")]\n', 1),
        ('    [InlineData("var begun = _drafts.Begin(session, rack, pilot.Read, comparer, copy);")]\n',
         '    [InlineData("var begun = _drafts.Begin(session, rack, full.Read, comparer, copy);")]\n', 1),
        ('    [InlineData("var confirmed = draft.ConfirmBase(pilot.StaleBase);")]\n', '    [InlineData("var confirmed = draft.ConfirmBase(full.StaleBase);")]\n', 1),
        ('    [InlineData("var kept = draft.KeepMineOnCurrentVersion(pilot.Read, PushBackPilotMaterializer.KeepPilotEditsOn, confirmed: true);")]\n',
         '    [InlineData("var kept = draft.KeepMineOnCurrentVersion(full.Read, PushBackFullComposition.KeepOn, confirmed: true);")]\n', 1),
        ('    [InlineData("return EditConfirmOutcome.Confirmed(pilot.Read.IsAdmitted ? null : pilot.Read.Admission.Diagnostic);")]\n',
         '    [InlineData("return EditConfirmOutcome.Confirmed(full.Read.IsAdmitted ? null : full.Read.Admission.Diagnostic);")]\n', 1),
    ],
    (EIG, 'TheExpressionOfTheDiagnosticOfTheAdmissionIsInTheWholeHostCoreExactlyTwiceOnceInTheGeneralCheckAndOnceInCheckNow'): [],
    (EIG, 'TheGuardOfTheExpressionOfTheDiagnosticSeesAThirdOneInTheCoreASecondOneInCheckNowAndOneInKeep'): [
        ('        Assert.Matches(BaseOrReadUse, WithoutTheAllowedConsumers(core + "\\nvar d = pilot.Read.Admission.Diagnostic;"));\n',
         '        Assert.Matches(BaseOrReadUse, WithoutTheAllowedConsumers(core + "\\nvar d = full.Read.Admission.Diagnostic;"));\n', 1),
    ],
    # PEG: el adaptador del campo es el del modelo completo
    (PEG, 'TheHostImplementsThePortExplicitlyAndNeverReadsOutsideTheThreeReadsAndChangesOnlyThroughNotePilotEdit'): [],
    (PEG, 'TheHostGuardTripsOnAPublicOperationAnOrderOfBasesOrAnAdapterThatIsAFieldOfInstance'): [
        ('    [InlineData("private static readonly PushBackPilotFieldAdapter PilotFieldAdapter = new();", "private readonly PushBackPilotFieldAdapter _adapter = new();")]\n',
         '    [InlineData("private static readonly PushBackFullFieldAdapter PilotFieldAdapter = new();", "private readonly PushBackFullFieldAdapter _adapter = new();")]\n', 1),
    ],
    (PEG, 'TheFlightGuardTripsWhenTheFlightIsCheckedBeforeTheGateThatAuthorizesInTheThreeReconciliations'): [],
    (PEG, 'TheHostGuardAllowsWhatTheNextTaskWouldAddAnUpdateOperationThatWrites'): [
        ('"private DraftSession<PushBackPilotDraftState>? FindPilotDraft"', '"private DraftSession<PushBackFullDraftState>? FindPilotDraft"', 2),
    ],
    (PEG, 'TheTrapGuardTripsOnEachNameAndEachWordThatTheNeighbouringGuardsForbid'): [],
    # RG:109-123: con D-S2 su texto sigue valiendo
    (RG, 'TheGuardOfWhoReadsSeesAKeepInTheRequestACheckInThePresentationAnyOfTheFourInTheBrowserAndANameof'): [],
    # Las tres nuevas de los borradores del panel y de «Actualizar»
    (WLG, 'TheMarkingOnlyTouchesTheDraftsOfThePilotOfTheSessionAndIsOnlyAHint'): [
        ('        Assert.Contains("_drafts.Find<PushBackPilotDraftState>(session, rackId)", body);\n',
         '        Assert.Contains("_drafts.Find<PushBackFullDraftState>(session, rackId)", body);\n', 1),
    ],
    (WLG, 'TheReloadIsAMethodOfItsOwnWithTheGateAndTheEightStepsOfTheTableAndOneReadThatOnlyReachesDiscardAndReload'): [
        ('            "_drafts.Find<PushBackPilotDraftState>(",\n', '            "_drafts.Find<PushBackFullDraftState>(",\n', 1),
        ('            ".DiscardAndReload(pilot.Read);");\n', '            ".DiscardAndReload(full.Read);");\n', 1),
        ('        Assert.Equal(1, Src.Count(body, @"\\bpilot\\.Read\\b"));\n'
         '        Assert.Matches(@"\\.DiscardAndReload\\s*\\(\\s*pilot\\.Read\\s*\\)\\s*;", body);\n',
         '        Assert.Equal(1, Src.Count(body, @"\\bfull\\.Read\\b"));\n'
         '        Assert.Matches(@"\\.DiscardAndReload\\s*\\(\\s*full\\.Read\\s*\\)\\s*;", body);\n', 1),
    ],
    (WPUG, 'TheBodyOfTheUpdateFollowsTheOrderOfThePackageAndEndsTheMarkOfKnownCleanAfterTheClose'): [
        ('            "_drafts.Find<PushBackPilotDraftState>(",\n', '            "_drafts.Find<PushBackFullDraftState>(",\n', 1),
        ('            "PushBackPilotUpdateRun.Execute(",\n', '            "PushBackPilotUpdateRun.ExecuteFull(",\n', 1),
        ('        Assert.Equal(1, Src.Count(body, @"PushBackPilotUpdateRun\\.Execute\\s*\\("));\n',
         '        Assert.Equal(1, Src.Count(body, @"PushBackPilotUpdateRun\\.ExecuteFull\\s*\\("));\n', 1),
    ],
}
