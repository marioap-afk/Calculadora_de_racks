"""PB-T4d (paquete v5.4): las ediciones con registro de tests/ (owner-pbt4d-substitutes-decision.md; D-S12 a D-S14; y, desde la v5.3, CA-T4d
M-01 y M-03 con la regla de owner-pbt4d-cat4d-decisions.md), como datos.

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
    # ---- CT-PBT3B-06: se queda la barrera sobre el modelo completo (i); sale (ii), del piloto; (iii), el corpus, pasa al modelo completo
    #      (CA-T4d M-01, con la regla de owner-pbt4d-cat4d-decisions.md, pregunta 2)
    (T + 'PushBackFullWritePrepareTests.cs',
     '            // (ii) a draft of the PILOT over pb-01-base with the manual structure of A at 1: the pilot admits it, and now the barrier stops it.\n',
     '            const string blockedPrefix = "El diseño tiene un error que impide escribirlo: ";\n',
     '            ' + NOTE + '(ii) was a draft of the PILOT (its Prepare and ComposeForWrite, retired)\n'
     '            // and goes; the barrier over the full model, (i), stays. (owner-pbt4d-cat4d-decisions.md, CA-T4d M-01): (iii), the designs of the\n'
     '            // corpus, was a draft of the pilot too, and passes now as drafts of the FULL model through PrepareFull, the assembler and the barrier.\n'
     '\n'
     '            // (iii) the designs of the corpus that the full model admits are not blocked: the same edit gives Ready.\n'
     '            var fixtures = new[]\n'
     '            {\n'
     '                "pb-01-base", "pb-02-modulos", "pb-03-fondos", "pb-04-blanco", "pb-05-topes", "pb-06-seguridad", "pb-07-combinado", "pb-08-redondeo",\n'
     '                "pb-10-sin-datum", "pb-11-sin-i41", "pb-12-poste-nulo", "pb-13-sideb-no-presente", "pb-14-peralte-heredado", "pb-15-sin-composite",\n'
     '                "pb-31-v2-modulos",\n'
     '            };\n'
     '            foreach (var fixture in fixtures)\n'
     '            {\n'
     '                Paso(log, () =>\n'
     '                {\n'
     '                    var read = FullWriteKit.Read(IntentKit.Scan(Rack, "A1", IntentKit.Member("A1", PrepareKit.Frontal(fixture))));\n'
     '                    var draft = FullWriteKit.DraftOf(read);\n'
     '                    var scale = fixture == "pb-02-modulos" ? 2.5 : 2.0;\n'
     '                    FullWriteKit.Edit(draft, state => state.Editable.AnnotationScale = scale);\n'
     '                    var result = PrepareFull(log, draft, read);\n'
     '                    log.Add(fixture + " admitido=" + read.Read.IsAdmitted + " prepare=" + Describe(result));\n'
     '                });\n'
     '            }\n'
     '\n',
     'CT-PBT3B-06: (ii) es del piloto (ComposeForWrite y su Prepare); (i) fija la barrera del modelo completo; (iii), el corpus por el '
     'ensamblador y la barrera, pasa a borradores del modelo completo por PrepareFull (CA-T4d M-01)'),
    (T + 'PushBackFullWritePrepareTests.cs',
     '                "celda-no-cabe prepare=Invalid estado-igual-al-primer-bloqueante-de-Evaluate=True",\n', '            Finish(log, expected);\n',
     '                "celda-no-cabe prepare=Invalid estado-igual-al-primer-bloqueante-de-Evaluate=True",\n'
     '            };\n'
     '            expected.AddRange(fixtures.Select(fixture => fixture + " admitido=True prepare=Ready"));\n',
     'CT-PBT3B-06: sale la linea esperada de (ii), del piloto; las de (iii), el corpus, se quedan (CA-T4d M-01)'),
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
    # ---- E-3 y la rama muerta de BuildCore (owner-pbt4d-deadcode-27-decisions.md, pregunta 1): salen las reglas de allowErase y de la vista no
    # disponible; la de los borrados, condicionados a una lateral obsoleta, se queda con un texto que vale en el RED y en el GREEN
    (T + 'PushBackPilotUpdateEquivalenceGuardTests.cs',
     '            if (!core.Contains("PushBackViewDispositionKind.ObsoleteLateral && allowErase", StringComparison.Ordinal)) yield return "the erasures are not conditioned to an obsolete lateral and to a model that admits erasing";\n'
     '            // The pilot keeps its text for a view that is not available.\n'
     '            if (!core.Contains("ViewUnavailablePrefix + entry.Key + ViewUnavailableSuffix", StringComparison.Ordinal)) yield return "the pilot does not keep ViewUnavailablePrefix";\n'
     '            if (!Regex.IsMatch(full, @"\\bBuildCore\\s*\\([^;]*\\btrue\\b")) yield return "the full model does not call BuildCore with the capacity to erase";\n', None,
     '            // I-64 F4B PB-T4d (owner-pbt4d-deadcode-27-decisions.md, pregunta 1): the mode of the pilot that never erases (allowErase) is retired\n'
     '            // with the text of BuildCore for a view that is not available; the erasures stay conditioned to an obsolete lateral.\n'
     '            if (!core.Contains("disposition == PushBackViewDispositionKind.ObsoleteLateral", StringComparison.Ordinal)) yield return "the erasures are not conditioned to an obsolete lateral";\n',
     'E-3 y la rama muerta de BuildCore: salen las reglas de allowErase y de ViewUnavailable; los borrados siguen condicionados a una lateral obsoleta'),
    (T + 'PushBackPilotUpdateEquivalenceGuardTests.cs',
     '            Assert.Contains("PushBackViewDispositionKind.ObsoleteLateral && allowErase", plan);\n'
     '            Assert.Contains("ViewUnavailablePrefix + entry.Key + ViewUnavailableSuffix", plan);\n', None,
     '            Assert.Contains("disposition == PushBackViewDispositionKind.ObsoleteLateral", plan);\n',
     'E-3 y la rama muerta de BuildCore: las dos comprobaciones directas del texto, con el de los borrados que se queda'),
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

# Las 29 de WPUR que se sustituyen sobre el modelo completo en PushBackRetirePilotRunTests (owner-pbt4d-run-coverage-decision.md: las 21;
# owner-pbt4d-deadcode-27-decisions.md, pregunta 2: y la 06, 07, 16, 19, 29 y 42; owner-pbt4d-cat4d-decisions.md, pregunta 1: y la 48 y la 49),
# con su numero en el orden del original y como se copian (v53/make_run_eq_v53.py); y su ayudante StatusAfter, que va copiado con el mapa. De
# las otras 20 pruebas de WPUR, 19 se borran con registro y una se queda (TheDesiredNamesOfACompositeRack...).
WPUR_SUBST = {
    'ThereIsNoOriginWhenNoMutableMemberHasTheTextOfTheOriginOfTheDraft': ('06', 'mapa'),
    'SeveralMembersWithTheTextOfTheOriginAreAPreflightWithTheirOwnReasonAndNotTheMisleadingOneOfTheReader': ('07', 'mapa'),
    'ADifferenceOfTheBaseInPrepareIsAConflictWithItsDifferencesAndNothingIsImported': ('08', 'mapa'),
    'AViewThatIsNotAvailableInTheOutgoingSystemIsAPreflightAndNothingIsImported':
        ('10', 'a mano: el Invalid de PREPARE dentro de la corrida es la frontal B de un rack sin lado B (owner-pbt4d-deadcode-27-decisions.md, pregunta 1)'),
    'ACleanDraftIsNothingToApplyAndAConflictDraftIsNotApplicableAndAPendingTextIsPendingTextEvenWithoutTheSlot': ('11', 'mapa'),
    'WithoutSurvivorsNothingIsImportedAndTheRunStops': ('12', 'mapa'),
    'AnImportThatFailsIsAPreflightWithItsCauseAndNoScopeOfMutationIsStarted': ('15', 'mapa'),
    'AForeignScopeOpenIsRejectedAsAValueAndNothingIsRereadWrittenOrReleased': ('16', 'mapa'),
    'ADocumentThatChangedBetweenPrepareAndMutateIsAPreflightWithoutWrites': ('19', 'mapa'),
    'ABlockThatIsNotTheOneThatWasPreparedIsAPreflightAndNothingIsWritten': ('20', 'mapa'),
    'TheSurvivorsAndTheLockedLayersAreCheckedAgainInMutateAndNothingIsWritten': ('21', 'mapa'),
    'AbortThatFailsAfterAUnitStartedIsCommitThrewWithAbortFailedAndNoRollbackIsClaimed': ('23', 'mapa'),
    'AReleaseThatFailsWithoutSealAndWithoutAUnitStartedIsNotAppliedFailureAndNotAnUncertainOutcome': ('24', 'mapa'),
    'ASealThatReturnsAndAReleaseThatThrowsGoesToPostAndTheReleaseIsAWarning': ('26', 'mapa'),
    'APostWithoutValuesOrWithoutItsRereadIsAppliedPostRereadFailed': ('28', 'mapa'),
    'AForeignScopeOpenAtPostIsAppliedPostRereadFailedWithItsCauseAndTheWarnings': ('29', 'mapa'),
    'AComparisonThatThrowsAfterTheSealIsAppliedPostRereadFailed': ('30', 'mapa'),
    'AnOuterReleaseThatThrowsAfterPostTurnsAppliedIntoAppliedWithWarningsAndKeepsTheOtherWarnings': ('31', 'mapa'),
    'AnOuterReleaseThatThrowsOutsidePostAddsItsCauseToTheClassificationThatWasAlreadyDecided': ('32', 'mapa'),
    'AnOuterReleaseThatThrowsOnAConflictIsDroppedBecauseAConflictCarriesNoCause': ('33', 'mapa'),
    'AConflictOrAPreflightOfMutateStaysWhatItIsEvenWhenTheReleaseOfTheScopeOfMutateThrows': ('34', 'mapa'),
    'ASealThatThrewKeepsBeingCommitThrewBecauseOfTheSealEvenIfTheAbortThrowsToo': ('35', 'mapa'),
    'TheRunNeverThrowsForMissingArgumentsOrForAPortThatAnswersNothing': ('37', 'mapa'),
    'ANameThatDiffersAfterTheRenameIsAWarningAndACaseOnlyDifferenceIsNot': ('40', 'mapa'),
    'TheMissingPiecesAndAFailedRegenerationAreWarningsAndTheStateStaysClean': ('41', 'mapa'),
    'ThePostComparisonDoesNotNormalizeAnything': ('42', 'mapa'),
    'ThePostComparisonFailsForAnotherSetOfKeysAnotherEnvelopeAndAnotherScale':
        ('43', 'mapa y, a mano, la lectura y la comparacion de POST del modelo completo (FullWriteKit.Read y ComparePostFull)'),
    'TheSystemIsRebuiltFromTheDesignTextOfAMemberOfThePlan': ('48', 'mapa'),
    'TheSystemRebuiltForAMemberCarriesTheRackNameOfItsEnvelope': ('49', 'mapa y, a mano, solo el comentario del lector'),
    'StatusAfter': ('ayudante', 'mapa'),
}
# Las dos de CA-T4d I-01 (owner-pbt4d-cat4d-decisions.md, pregunta 1): no prueban la corrida, sino la reconstruccion del sistema de un miembro
# (PushBackPilotSystemRebuild, que se queda: la usan DesiredNamesOf y el escritor real de Actualizar), sobre el plan del PREPARE completo.
WPUR_REBUILD = ('TheSystemIsRebuiltFromTheDesignTextOfAMemberOfThePlan', 'TheSystemRebuiltForAMemberCarriesTheRackNameOfItsEnvelope')

# La llamada de los cuatro casos de StaleBase: el escaneo, no la lectura del piloto.
STALE_CALL = ('UnknownAfterACommitThatThrew(IntentKit.Read(IntentKit.TwoViews()))', 'UnknownAfterACommitThatThrew(IntentKit.TwoViews())', 4)

# ---- CA-T4d M-03 (la sesion: no queda codigo muerto en tests/, como pide el alcance B): los ayudantes que la retirada deja sin uso.
# Medido en el arbol del RED (IDE0051/IDE0052 en una copia, ide_dead_v53.py; y por texto, v53/dead_text_v53.py), en vueltas hasta que no sale
# ninguno nuevo. Cada uno es (ruta, nombre, porque, n.o de declaracion o None, como): `quitar` lo quita make_red_v53.py despues de las
# ediciones (v53/unused_v53.py: su declaracion, sus ///, sus atributos y el // pegado encima); `edicion` lo quita la edicion de WPUR_TAIL.
NOTE_M03 = '// I-64 F4B PB-T4d (CA-T4d M-03): '
WPUR_TAIL = (W + 'WorkspacePilotUpdateRunTests.cs',
             '    private static readonly string[] HappyCalls =\n',
             '    // M-06 (d): the names of the views of a composite rack carry the side of each cut. The pilot refuses a composite rack (its reader says it is\n',
             '    ' + NOTE_M03 + 'the tests of the run of the pilot go (29 of them have their substitute over the full model in\n'
             '    // PushBackRetirePilotRunTests), and with them their helpers and the titles of their sections; what stays is the rule of the names of a\n'
             '    // composite rack, over a plan built by hand.\n\n',
             'WPUR: HappyCalls, Boom, Cause y NamedScan quedan sin uso, y los titulos de sus secciones y el comentario de la n.o 42 sin prueba (CA-T4d M-03)')
EDITS.append(WPUR_TAIL)
# ---- Recomprobacion 1 de CA-T4d, M-12 (a): solo comentarios de WPUR (el resumen de la clase y los comentarios de la 48 y la 49, que se quedan
#      sin su prueba). Despues de las bajas y de WPUR_TAIL.
NOTE_R1 = '// I-64 F4B PB-T4d (recomprobacion 1 de CA-T4d, M-12): '
EDITS.append((W + 'WorkspacePilotUpdateRunTests.cs',
              '// I-64 F4-T4B-MUTATE-POST (INV-F4T4B-01, 02, 03, 05, 06, 10, 16): the pure and TOTAL run of the update of the pilot, with the real draft\n'
              '// session, the real planner and the real reader, and a double of the port that simulates the drawing. The classification is the table of\n'
              '// D-13 by frontier, with the three rows of the abort, the row of the outer release, the precedence and its two declared exceptions.\n', None,
              '// I-64 F4-T4B-MUTATE-POST (INV-F4T4B-01, 02, 03, 05, 06, 10, 16) tested here the run of the update of the pilot, which PB-T4d retires.\n'
              + NOTE_R1 + 'what stays is the rule of the names of the views of a composite rack (DesiredNamesOf), over a plan\n'
              '// built by hand; the run itself is tested over the full model in PushBackRetirePilotRunTests.\n',
              'WPUR: el resumen de la clase describia la corrida del piloto, que se retira (recomprobacion 1 de CA-T4d, M-12 a; solo comentarios)'))
EDITS.append((W + 'WorkspacePilotUpdateRunTests.cs',
              '    }\n\n'
              '    // The system of the outgoing design, rebuilt apart from the text of a member of the plan with the same resolver as PREPARE.\n\n'
              '    // I-64 F4-T4-HOST-C1 (D-C1-01, INV-C1-01, test 1 of D-C1-09): the system that the writer redraws carries the rack name of the envelope of ITS\n'
              '    // member, raw, because the label of the name of the views is drawn only from it. Everything runs over the text kits of the plan: the members\n'
              '    // are only deserialized, nothing is drawn.\n'
              '}\n', None,
              '    }\n'
              '}\n',
              'WPUR: los comentarios de la n.o 48 y la n.o 49, que se quedaban sin su prueba (las dos van sustituidas en PushBackRetirePilotRunTests; '
              'recomprobacion 1 de CA-T4d, M-12 a; solo comentarios)'))
UNUSED = [
    (W + 'WorkspacePilotUpdateRunTests.cs', 'HappyCalls', 'lo usaban las pruebas de la corrida del piloto que se borran', None, 'edicion'),
    (W + 'WorkspacePilotUpdateRunTests.cs', 'Boom', 'lo usaban las pruebas de la corrida del piloto que se borran (su copia va en PushBackRetirePilotRunTests)', None, 'edicion'),
    (W + 'WorkspacePilotUpdateRunTests.cs', 'Cause', 'lo usaban las pruebas de la corrida del piloto que se borran (su copia va en PushBackRetirePilotRunTests)', None, 'edicion'),
    (W + 'WorkspacePilotUpdateRunTests.cs', 'NamedScan', 'lo usaban la n.o 40 y otras que se borran (su copia va en PushBackRetirePilotRunTests)', None, 'edicion'),
    (W + 'WorkspacePilotPrepareTests.cs', 'AssertHeld', 'lo usaban las pruebas de ConfirmBase que se borran', None, 'quitar'),
    (W + 'WorkspacePilotPrepareTests.cs', 'Held', 'el record de AssertHeld', None, 'quitar'),
    (W + 'WorkspacePilotPrepareTests.cs', 'YetAnotherBase', 'lo usaba una prueba de ConfirmBase que se borra', None, 'quitar'),
    (W + 'WorkspacePilotPrepareTests.cs', 'WithExtra', 'lo usaba una prueba del plan que se borra', None, 'quitar'),
    (W + 'WorkspacePilotPrepareTests.cs', 'DraftHeld', 'el record del Hold del piloto, que se borra', None, 'quitar'),
    (W + 'WorkspacePilotReconciliationHostTests.cs', 'Hold', 'lo usaban las pruebas de la reconciliacion del piloto que se borran', None, 'quitar'),
    (W + 'WorkspacePilotUpdateGuardTests.cs', 'ComposeForWriteViolations', 'lo usaban las dos guardas de ComposeForWrite que se borran', None, 'quitar'),
    (W + 'WorkspacePilotUpdateGuardTests.cs', 'MaterializerPath', 'lo usaba ComposeForWriteViolations', None, 'quitar'),
    (W + 'WorkspaceDraftPilotMaterializationTests.cs', 'ScalePath', 'lo usaban las pruebas de la materializacion del piloto que se borran', None, 'quitar'),
    (W + 'WorkspaceDraftPilotMaterializationTests.cs', 'Variant', 'lo usaban las pruebas de la materializacion del piloto que se borran', None, 'quitar'),
    (W + 'WorkspaceDraftPilotMaterializationTests.cs', 'CanonicalCorpus', 'el MemberData de una Theory que se borra', None, 'quitar'),
    (W + 'WorkspaceDraftPilotMaterializationTests.cs', 'LegacyCorpus', 'el MemberData de una Theory que se borra', None, 'quitar'),
    (W + 'WorkspacePilotEditHostTests.cs', 'TextCorpus', 'lo usaba una prueba del campo del piloto que se borra', None, 'quitar'),
    (W + 'WorkspaceDraftPilotStateTests.cs', 'IsContainer', 'el de la clase de pruebas (el n.o 2 del archivo): lo usaban las pruebas del comparador que se borran', 2, 'quitar'),
    (W + 'WorkspaceDraftPilotStateTests.cs', 'ScaleCases', 'lo usaban las pruebas del adaptador del campo, que se retira', None, 'quitar'),
    (W + 'WorkspaceDraftPilotStateTests.cs', 'Cultures', 'el MemberData de las pruebas del adaptador del campo, que se retira', None, 'quitar'),
    (W + 'WorkspaceDraftPilotStateTests.cs', 'PilotRich', 'el estado rico de las pruebas del comparador, que se retira', None, 'quitar'),
    ('tests/RackCad.UI.Tests/Workspace/WorkspaceDraftPilotValidationParityTests.cs', 'InCulture', 'lo usaban las pruebas del adaptador del campo que se borran', None, 'quitar'),
    ('tests/RackCad.UI.Tests/Workspace/WorkspaceDraftPilotValidationParityTests.cs', 'Texts', 'lo usaban las pruebas del adaptador del campo que se borran', None, 'quitar'),
    ('tests/RackCad.UI.Tests/Workspace/WorkspaceDraftPilotValidationParityTests.cs', 'Cultures', 'el MemberData de las pruebas que se borran', None, 'quitar'),
    ('tests/RackCad.UI.Tests/Workspace/WorkspaceDraftPilotWindowLoadCharacterizationTests.cs', 'DesignNumber', 'lo usaba la Theory saltada desde PB-TC que se borra (SUS-01)', None, 'quitar'),
    ('tests/RackCad.UI.Tests/Workspace/WorkspaceDraftPilotWindowLoadCharacterizationTests.cs', 'Diff', 'lo usaba la Theory saltada desde PB-TC que se borra (SUS-01)', None, 'quitar'),
    ('tests/RackCad.UI.Tests/Workspace/WorkspaceDraftPilotWindowLoadCharacterizationTests.cs', 'Fixture', 'lo usaba la Theory saltada desde PB-TC que se borra (SUS-01)', None, 'quitar'),
    ('tests/RackCad.UI.Tests/Workspace/WorkspaceDraftPilotWindowLoadCharacterizationTests.cs', 'LoadInWindow', 'lo usaba la Theory saltada desde PB-TC que se borra (SUS-01)', None, 'quitar'),
    # la segunda vuelta: lo que solo usaban los de la primera
    (W + 'WorkspaceDraftPilotStateTests.cs', 'Mutator', 'lo usaban el IsContainer de la clase de pruebas y las pruebas del comparador que se borran', None, 'quitar'),
    (W + 'WorkspacePilotReconciliationHostTests.cs', 'Held', 'el record del Hold que se quita', None, 'quitar'),
    ('tests/RackCad.UI.Tests/Workspace/WorkspaceDraftPilotWindowLoadCharacterizationTests.cs', 'WindowResult', 'el resultado de LoadInWindow, que se quita', None, 'quitar'),
    ('tests/RackCad.UI.Tests/Workspace/WorkspaceDraftPilotWindowLoadCharacterizationTests.cs', 'Persist', 'lo usaba LoadInWindow, que se quita', None, 'quitar'),
]
# Las clases que la retirada deja sin ninguna prueba: se borra el archivo entero (bloque DELETE de la pieza del RED).
DELETE_FILES = [
    ('tests/RackCad.UI.Tests/PushBackFieldValidationConcordanceTests.cs', 'su unica prueba (CT-PBT2B-10) se borra: la clase queda vacia'),
    ('tests/RackCad.UI.Tests/PushBackDeclaredCorrectionPilotParityTests.cs', 'sus dos pruebas (CT-PBTC-06 y CT-PBTC-10) se borran: la clase queda vacia'),
    ('tests/RackCad.UI.Tests/Workspace/WorkspaceDraftPilotCanonicalLineEndingTests.cs',
     'sus pruebas se borran: la clase queda con solo ayudantes sin uso (Canonical, Designs, RepoRoot y Fixture)'),
]
