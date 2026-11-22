"""PB-T4d (paquete v5.2): las ediciones con registro de tests/ (owner-pbt4d-substitutes-decision.md; D-S12 a D-S14), como datos.

Cada edicion es (ruta, desde, hasta, nuevo, porque):
  - con `hasta` None, `desde` es un texto literal que tiene que estar exactamente una vez en el archivo (ya con las bajas) y se sustituye por `nuevo`;
  - con `hasta`, se sustituye el tramo de lineas que va de la linea que contiene `desde` (exactamente una) a la anterior a la que contiene
    `hasta` (la primera despues), por `nuevo`.
Los textos van con saltos de linea \n. Las aplica make_red_v52.py despues de las bajas, en este orden, y la pieza del RED lleva su texto literal.
Tambien estan aqui los tres Skip de D-S12 (SKIPS) y lo que se queda de las 138 sin tocar (KEPT).
"""
W = 'tests/RackCad.Tests/Workspace/'
T = 'tests/RackCad.Tests/'

SKIPS = [
    # (ruta, metodo, motivo literal ASCII)
    (T + 'PushBackEditAllFullCopyGuardTests.cs', 'CT_PBT4B_20_TheScriptOfSkipAndDuplicateGivesTheCopiesTheKitAndTheSkipsOfTheClosedList',
     'I-64 F4B PB-T4d: sustituida por su equivalente en PushBackRetirePilotFullCopyGuardTests (registro declarado superseding-characterization.md; decision del Owner owner-pbt4d-list-decisions.md, pregunta 1).'),
    (W + 'WorkspacePilotPrepareGuardTests.cs', 'TheFileOfThePlanIsNullableEnabledInTheNamespaceOfThePilotWithBracesAndDeclaresTheTypesOfThePackage',
     'I-64 F4B PB-T4d: sustituida por su equivalente en PushBackRetirePilotPlanGuardTests (registro declarado superseding-characterization.md; decision del Owner owner-pbt4d-list-decisions.md, pregunta 1).'),
    (W + 'WorkspacePilotPrepareGuardTests.cs', 'TheOrderOfPrepareIsTheOneOfThePackageAndNoOutcomeBeforeConflictLooksAtTheReread',
     'I-64 F4B PB-T4d: sustituida por su equivalente en PushBackRetirePilotPlanGuardTests (registro declarado superseding-characterization.md; decision del Owner owner-pbt4d-list-decisions.md, pregunta 1).'),
]

# Lo que se queda de las 138 y de sus ayudantes, sin tocar: compila y pasa sobre el modelo completo (medido en /d/r6h).
KEPT = [
    (T + 'PushBackFullWritePrepareTests.cs', m) for m in (
        'CT_PBT3B_01_PrepareFullWritesTheFieldsTheNameAndTheDesignOfTheDraftOverTheEnvelopeThatWasReread',
        'CT_PBT3B_02_TheFrontsRedrawEveryViewAndTheObsoleteLateralIsErasedWithTheOriginReanchored',
        'CT_PBT3B_03_AModuleLengthOfEachSideReachesTheOutgoingDesignAndNoViewIsErased',
        'CT_PBT3B_04_TheSideBIsComposedRejectedAsAFrontalOfAOneWayRackOrRemovedWithItsLaterals',
        'CT_PBT3B_10_Auth13RejectsTheWriteOfADesignWithANestedUnknownFieldWithoutAnyCallOfWriting',
        'CT_PBT3B_11_TheEnvelopeThatWasRereadIsCarriedOverAndTheDraftKeepsItsDesignAndItsName',
        'CT_PBT3B_15_ThePrecheckOfTheFullModelAndTheOrderOfPrepareFullAreTheOnesOfThePilot',
        'Auth13', 'Barrier', 'Campos', 'FilaEstilo', 'FilaF', 'FrontCase')
]

NOTE = '// I-64 F4B PB-T4d (owner-pbt4d-substitutes-decision.md): '

EDITS = [
    # ---- CT-PBT3B-05: se queda la parte del modelo completo; sale la del piloto (su Prepare y su instantanea se retiran)
    (T + 'PushBackFullWritePrepareTests.cs',
     '            // The pilot keeps the name of each envelope that was re-read.\n', '            Finish(\n',
     '            ' + NOTE + 'the part of the pilot (its Prepare, retired) goes; what the full model writes stays.\n\n',
     'CT-PBT3B-05 fija el nombre del modelo completo; la linea del piloto nombraba su Prepare, que se retira'),
    (T + 'PushBackFullWritePrepareTests.cs',
     '                    "nombre-en-blanco prepare=Ready A1=Origen B2=Origen C3=Origen",\n'
     '                    "piloto prepare=Ready A1=Origen B2=Otro-B C3=Otro-C",\n', None,
     '                    "nombre-en-blanco prepare=Ready A1=Origen B2=Origen C3=Origen",\n',
     'CT-PBT3B-05: sale la linea esperada del piloto'),
    # ---- CT-PBT3B-06: se queda la barrera sobre el modelo completo (i); salen (ii) y (iii), del piloto
    (T + 'PushBackFullWritePrepareTests.cs',
     '            // (ii) a draft of the PILOT over pb-01-base with the manual structure of A at 1: the pilot admits it, and now the barrier stops it.\n',
     '            const string blockedPrefix = "El diseño tiene un error que impide escribirlo: ";\n',
     '            ' + NOTE + '(ii) and (iii) were drafts of the PILOT (its Prepare and ComposeForWrite,\n'
     '            // retired); the barrier over the full model, (i), stays.\n\n',
     'CT-PBT3B-06: (ii) y (iii) son del piloto (ComposeForWrite y su Prepare); (i) fija la barrera del modelo completo'),
    (T + 'PushBackFullWritePrepareTests.cs',
     '                "celda-no-cabe prepare=Invalid estado-igual-al-primer-bloqueante-de-Evaluate=True",\n', '            Finish(log, expected);\n',
     '                "celda-no-cabe prepare=Invalid estado-igual-al-primer-bloqueante-de-Evaluate=True",\n'
     '            };\n',
     'CT-PBT3B-06: salen las lineas esperadas del piloto'),
    # ---- CT-PBT3B-09: se queda ComparePostFull; sale la comparacion del piloto (ComparePost y PilotUpdateSetup se retiran)
    (T + 'PushBackFullWriteRunTests.cs',
     '            // The pilot, with a plan without erasures, gives what it gives today in the same cases.\n', '            Finish(\n',
     '            ' + NOTE + 'the comparison of the pilot (ComparePost, retired) goes; ComparePostFull stays.\n\n',
     'CT-PBT3B-09 fija ComparePostFull; las dos lineas del piloto nombraban ComparePost, que se retira'),
    (T + 'PushBackFullWriteRunTests.cs',
     '                    "erased-keys=[B2]",\n'
     '                    "piloto-todo-igual <null>",\n'
     '                    "piloto-escala-distinta True",\n', None,
     '                    "erased-keys=[B2]",\n',
     'CT-PBT3B-09: salen las dos lineas esperadas del piloto'),
    # ---- E-3, E-5 y E-6 de D-1 (F4B-E29: «PB-T4 sustituira tres de esos equivalentes al quitar el piloto»): sin el lado del piloto
    (T + 'PushBackPilotUpdateEquivalenceGuardTests.cs',
     '            var build = Src.BodyOf(plan, @"private\\s+static\\s+PushBackPilotPrepareResult\\s+Build\\s*\\(");\n', None,
     '            ' + NOTE + 'the Build of the pilot is retired; what E-3 protected of BuildCore and of\n'
     '            // the full model stays.\n',
     'E-3: el Build del piloto se retira; lo de BuildCore y del modelo completo se queda'),
    (T + 'PushBackPilotUpdateEquivalenceGuardTests.cs',
     '            if (!Regex.IsMatch(build, @"\\bBuildCore\\s*\\([^;]*\\bfalse\\b")) yield return "the pilot does not call BuildCore without the capacity to erase";\n', None,
     '', 'E-3: sale la regla del Build del piloto'),
    (T + 'PushBackPilotUpdateEquivalenceGuardTests.cs',
     '            if (!build.Contains("input.CurrentDraftState.Inputs.Annotations.AnnotationScale", StringComparison.Ordinal)) yield return "the pilot does not take the scale of its current state";\n', None,
     '', 'E-3: sale la regla de la escala del piloto'),
    (T + 'PushBackPilotUpdateEquivalenceGuardTests.cs',
     '                ("Execute", @"public\\s+static\\s+DraftApplyOutcome<PushBackPilotDraftState>\\s+Execute\\s*\\("),\n', None,
     '                ' + NOTE + 'the Execute of the pilot is retired; ExecuteFull stays.\n',
     'E-5 y E-6: el Execute del piloto se retira; las reglas de M-01 sobre ExecuteFull y Execution<TState> se quedan'),
    # ---- La propiedad H-06 sobre la composicion que se queda: ForFullWrite solo la llama el planificador, tras la comparacion de la base
    (W + 'WorkspacePilotPrepareGuardTests.cs',
     '        var callers = product\n'
     '            .Where(f => !f.Path.EndsWith("/PushBackPilotMaterializer.cs", StringComparison.Ordinal)\n'
     '                        && Regex.IsMatch(f.Code, @"\\bPushBackPilotMaterializer\\s*\\.\\s*ComposeForWrite\\s*\\("))\n'
     '            .ToArray();\n'
     '        if (callers.Length != 1 || callers[0].Path != PlanPath) yield return "ComposeForWrite has callers other than the planner: " + string.Join(", ", callers.Select(c => c.Path));\n'
     '        foreach (var (_, code) in callers.Where(c => c.Path == PlanPath))\n'
     '        {\n'
     '            var compare = code.IndexOf(".CompareWith(", StringComparison.Ordinal);\n'
     '            var compose = code.IndexOf("PushBackPilotMaterializer.ComposeForWrite(", StringComparison.Ordinal);\n'
     '            if (compare < 0 || compose < compare) yield return "ComposeForWrite is called before the comparison of the base";\n'
     '            if (Regex.Matches(code, @"\\bPushBackPilotMaterializer\\s*\\.\\s*ComposeForWrite\\s*\\(").Count != 1) yield return "ComposeForWrite is called more than once";\n'
     '        }\n', None,
     '        ' + NOTE + 'ComposeForWrite is retired; H-06 holds for the composition that stays,\n'
     '        // PushBackFullComposition.ForFullWrite.\n'
     '        var callers = product\n'
     '            .Where(f => !f.Path.EndsWith("/PushBackFullComposition.cs", StringComparison.Ordinal)\n'
     '                        && Regex.IsMatch(f.Code, @"\\bPushBackFullComposition\\s*\\.\\s*ForFullWrite\\s*\\("))\n'
     '            .ToArray();\n'
     '        if (callers.Length != 1 || callers[0].Path != PlanPath) yield return "ForFullWrite has callers other than the planner: " + string.Join(", ", callers.Select(c => c.Path));\n'
     '        foreach (var (_, code) in callers.Where(c => c.Path == PlanPath))\n'
     '        {\n'
     '            var compare = code.IndexOf(".CompareWith(", StringComparison.Ordinal);\n'
     '            var compose = code.IndexOf("PushBackFullComposition.ForFullWrite(", StringComparison.Ordinal);\n'
     '            if (compare < 0 || compose < compare) yield return "ForFullWrite is called before the comparison of the base";\n'
     '            if (Regex.Matches(code, @"\\bPushBackFullComposition\\s*\\.\\s*ForFullWrite\\s*\\(").Count != 1) yield return "ForFullWrite is called more than once";\n'
     '        }\n',
     'H-06 (WPPG): ComposeForWrite se retira; la propiedad sigue sobre ForFullWrite, la composicion del modelo completo'),
    (W + 'WorkspacePilotPrepareGuardTests.cs',
     '        Assert.NotEmpty(AfterMatchViolations(product.Append(("src/RackCad.Application/Workspace/Other.cs", "var c = PushBackPilotMaterializer.ComposeForWrite(a, b, catalog);"))));\n', None,
     '        Assert.NotEmpty(AfterMatchViolations(product.Append(("src/RackCad.Application/Workspace/Other.cs", "var c = PushBackFullComposition.ForFullWrite(a, b, catalog);"))));\n',
     'H-06 (WPPG), su autoprueba: el llamador inyectado es de ForFullWrite'),
    (W + 'WorkspacePilotPrepareGuardTests.cs',
     '"var early = PushBackPilotMaterializer.ComposeForWrite(i, c, catalog); var comparison = input.StaleBase.CompareWith(reread.StaleBase);"', None,
     '"var early = PushBackFullComposition.ForFullWrite(i, c, catalog); var comparison = input.StaleBase.CompareWith(reread.StaleBase);"',
     'H-06 (WPPG), su autoprueba: la composicion inyectada antes de la comparacion es ForFullWrite'),
    # ---- PEG: la regla del adaptador estatico, la del adaptador completo (como su copia de PB-T4b); las seis autopruebas dejan de pasar en vacio
    (W + 'WorkspacePilotEditGuardTests.cs',
     '        if (Regex.IsMatch(host, @"private\\s+(?:readonly\\s+)?PushBackPilotFieldAdapter\\s+_\\w+")) yield return "the adapter of the pilot is a field of instance";\n'
     '        if (Src.Count(host, @"private\\s+static\\s+readonly\\s+PushBackPilotFieldAdapter\\s+\\w+") != 1) yield return "the adapter of the pilot is not the one static field";\n', None,
     '        if (Regex.IsMatch(host, @"private\\s+(?:readonly\\s+)?PushBackFullFieldAdapter\\s+_\\w+")) yield return "the adapter of the field is a field of instance";\n'
     '        if (Src.Count(host, @"private\\s+static\\s+readonly\\s+PushBackFullFieldAdapter\\s+\\w+") != 1) yield return "the adapter of the field is not the one static field";\n',
     'PEG, HostViolations: la regla del adaptador estatico pasa a la del adaptador completo, como en su copia (GUARD_EDITS de PB-T4b); las seis autopruebas de la lista vuelven a discriminar (paquete, seccion 4.2: «PB-T4d (C-7) las sustituye»)'),
    # ---- M-04 (D-S13): las dos guardas de PEG y el ayudante de su copia, con la firma del borrador completo
    (W + 'WorkspacePilotEditGuardTests.cs',
     '        var host = Host().Replace("private DraftSession<PushBackPilotDraftState>? FindPilotDraft", extra + "private DraftSession<PushBackPilotDraftState>? FindPilotDraft", StringComparison.Ordinal);\n', None,
     '        var host = Host().Replace("private DraftSession<PushBackFullDraftState>? FindPilotDraft", extra + "private DraftSession<PushBackFullDraftState>? FindPilotDraft", StringComparison.Ordinal);\n',
     'M-04 (PEG:591-598): el Replace busca FindPilotDraft con la firma del borrador completo'),
    (W + 'WorkspacePilotEditGuardTests.cs',
     '        foreach (var helper in new[] { @"private\\s+DraftSession<PushBackPilotDraftState>\\?\\s+FindPilotDraft\\b",', None,
     '        foreach (var helper in new[] { @"private\\s+DraftSession<PushBackFullDraftState>\\?\\s+FindPilotDraft\\b",',
     'M-04 (PEG:916-920, su ayudante WriteViolations, PEG:906): FindPilotDraft con la firma del borrador completo'),
    (W + 'WorkspacePilotEditGuardFull.cs',
     '        foreach (var helper in new[] { @"private\\s+DraftSession<PushBackPilotDraftState>\\?\\s+FindPilotDraft\\b",', None,
     '        foreach (var helper in new[] { @"private\\s+DraftSession<PushBackFullDraftState>\\?\\s+FindPilotDraft\\b",',
     'M-04 (WorkspacePilotEditGuardFull.cs:255-258): el ayudante de la copia, con la firma del borrador completo'),
    # ---- La prueba lenta (D-S14): cada 97.o toque y el ultimo
    (W + 'WorkspacePilotPrepareFull.cs',
     '        var failures = new List<PushBackPilotPrepareResult>();\n'
     '        for (var allowed = 0; allowed < total; allowed++)\n'
     '        {\n', None,
     '        // I-64 F4B PB-T4d (OWNER-05; owner-pbt4d-list-decisions.md, pregunta 3; D-S14): the loop is bounded to every 97th touch and the\n'
     '        // last one, instead of every touch (4 936 PREPARE, about 6.5 minutes); the failures of the composition (a) and of the builder (d)\n'
     '        // are among them, and the assertions do not change.\n'
     '        var failures = new List<PushBackPilotPrepareResult>();\n'
     '        foreach (var allowed in Enumerable.Range(0, total).Where(touch => touch % 97 == 0 || touch == total - 1))\n'
     '        {\n',
     'la prueba lenta (OWNER-05): su bucle se acota a cada 97.o toque del catalogo y al ultimo; las aserciones no cambian'),
    # ---- Ownership (el nucleo del borrador): se quedan las filas del estado de prueba (box); salen las del piloto
    (W + 'WorkspaceDraftPilotOwnershipTests.cs',
     '    public static IEnumerable<object[]> Kinds() => new[] { new object[] { "box" }, new object[] { "pilot" } };\n', None,
     '    ' + NOTE + 'the rows of the pilot go (its comparer is retired); the ones of the\n'
     '    // mutable test state, which test the ownership boundary of the draft core, stay.\n'
     '    public static IEnumerable<object[]> Kinds() => new[] { new object[] { "box" } };\n',
     'Ownership: el nucleo del borrador; se quedan las filas box de las ocho Theory'),
    (W + 'WorkspaceDraftPilotOwnershipTests.cs',
     '        if (kind == "box") scenario(new Ownership<DraftBox>(OwnershipKits.Box()));\n'
     '        else scenario(new Ownership<PushBackPilotDraftState>(OwnershipKits.Pilot()));\n', None,
     '        Assert.Equal("box", kind);\n'
     '        scenario(new Ownership<DraftBox>(OwnershipKits.Box()));\n',
     'Ownership: Both solo con el estado de prueba'),
    (W + 'WorkspaceDraftPilotOwnershipTests.cs',
     '    [InlineData("box", false)]\n'
     '    [InlineData("box", true)]\n'
     '    [InlineData("pilot", false)]\n'
     '    [InlineData("pilot", true)]\n', None,
     '    [InlineData("box", false)]\n'
     '    [InlineData("box", true)]\n',
     'Ownership: salen las dos filas del piloto de la Theory de POST'),
    # ---- StaleBase (Check now con el nucleo real): el borrador nace de la lectura completa del mismo escaneo
    (W + 'WorkspaceDraftPilotIntentStaleBaseTests.cs',
     '    private static DraftSession<PushBackPilotDraftState> UnknownAfterACommitThatThrew(PushBackPilotIntentResult first)\n'
     '    {\n'
     '        Assert.True(first.Read.IsAdmitted);\n'
     '        var begun = new DraftRegistry().Begin(DraftKit.S1, "R1", first.Read, PushBackPilotStateComparer.Instance, PushBackPilotStateComparer.Copy);\n'
     '        Assert.Equal(DraftBeginStatus.Created, begun.Status);\n'
     '        var draft = begun.Draft!;\n'
     '        Assert.True(draft.TryBeginApply().IsStarted);\n'
     '        draft.CompleteApply(DraftApplyOutcome<PushBackPilotDraftState>.CommitThrew("the commit threw"));\n', None,
     '    // I-64 F4B PB-T4d (owner-pbt4d-substitutes-decision.md): the draft is born from the read of the FULL model of the same scan (the\n'
     '    // comparer of the pilot is retired); the base that Check now compares is the same, and the rereads are still the ones of the reader.\n'
     '    private static DraftSession<PushBackFullDraftState> UnknownAfterACommitThatThrew(PushBackPilotIntentScan scan)\n'
     '    {\n'
     '        var first = PushBackFullDraftReader.Read(scan, PilotKit.Catalog);\n'
     '        Assert.True(first.Read.IsAdmitted);\n'
     '        var begun = new DraftRegistry().Begin(DraftKit.S1, "R1", first.Read, PushBackFullDraftStateComparer.Instance, PushBackFullDraftStateComparer.Copy);\n'
     '        Assert.Equal(DraftBeginStatus.Created, begun.Status);\n'
     '        var draft = begun.Draft!;\n'
     '        Assert.True(draft.TryBeginApply().IsStarted);\n'
     '        draft.CompleteApply(DraftApplyOutcome<PushBackFullDraftState>.CommitThrew("the commit threw"));\n',
     'StaleBase: el nucleo del borrador (Check now de un borrador Unknown); el borrador nace de la lectura completa'),
    (W + 'WorkspaceDraftPilotIntentStaleBaseTests.cs',
     'using RackCad.Application.Systems.PushBack.Pilot;\n', None,
     'using RackCad.Application.Systems.PushBack.Draft;\nusing RackCad.Application.Systems.PushBack.Pilot;\n',
     'StaleBase: la directiva del modelo completo'),
    # ---- Materialize (que se queda): los dos estados se comparan por el diseno que el producto arma con ellos
    (W + 'WorkspaceDraftPilotMaterializationTests.cs',
     '        Assert.True(Comparer.Equals(byHand, materialized));\n', None,
     '        ' + NOTE + 'the comparer of the pilot is retired; the two states are compared by\n'
     '        // the design that the product assembles from them.\n'
     '        Assert.Equal(\n'
     '            PilotKit.Persist(new PushBackCompositeEditorAssembler(PilotKit.Catalog).BuildDesign(byHand.Editor, byHand.Inputs)),\n'
     '            PilotKit.Persist(new PushBackCompositeEditorAssembler(PilotKit.Catalog).BuildDesign(materialized.Editor, materialized.Inputs)));\n',
     'Materialize se queda (lo usa el lector del modelo completo); su comparacion con la carga de la ventana pasa del comparador del piloto al diseno armado'),
]

# La llamada de los cuatro casos de StaleBase: el escaneo, no la lectura del piloto.
STALE_CALL = ('UnknownAfterACommitThatThrew(IntentKit.Read(IntentKit.TwoViews()))', 'UnknownAfterACommitThatThrew(IntentKit.TwoViews())', 4)
