#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Persistence;
using RackCad.Application.ProjectVariables;
using RackCad.Application.Systems.Selective;
using RackCad.Application.VentanaRackCad;
using RackCad.Domain.Systems.Selective;
using Xunit;

namespace RackCad.Tests;

// I-64 A-14, line D, Selectivo E1-A: the door of the RackCad window to a Selectivo. The read decides what RACKEDITAR decides before its
// window exists (a broken reference blocks it, with no fallback); the write is planned again on the rack and the register read at write
// time, with the linked states the window opened, and then goes through the «Actualizar» drawing half of RACKEDITAR. The AutoCAD commands
// cannot run here, so their wiring is fixed on the source; the rules are pure and tested by behaviour.
public class SelectiveRackDoorTests
{
    private const string RackId = "GUID-SEL-VENTANA";
    private const string VarId = "8a1d4e77-2c93-4b60-8f15-6e0b93a7c221";
    private static readonly PropertyId Clearance = ProjectPropertyIds.SelectiveVerticalClearance;
    private static readonly PropertyId Tolerance = ProjectPropertyIds.SelectivePalletTolerance;

    private static SelectivePalletDesign Design(double clearance)
    {
        var design = new SelectivePalletDesign
        {
            PostId = "POSTE",
            PostPeralte = 3.0,
            PalletTolerance = 4.0,
            VerticalClearance = clearance,
            FloorBeamRise = 4.0,
            PalletDepth = 48.0,
            DepthCount = 1,
            DrawBasePlate = true,
        };
        var bay = new SelectiveBayDesign { FloorBeam = true };
        for (var level = 0; level < 2; level++)
        {
            bay.Levels.Add(new SelectiveCell { Pallet = new Tarima { Frente = 42.0, Alto = 60.0 }, PalletCount = 2, BeamId = "LARGUERO", BeamPeralte = 4.0 });
        }

        design.Bays.Add(bay);
        return design;
    }

    private static string Authored(bool bound, double literal = 6.0)
    {
        var document = SelectivePalletDesignDocument.From(Design(literal), RackId, "Selectivo A");
        if (bound)
        {
            document.PropertyValues = new Dictionary<string, SelectivePropertyValueDocument>
            {
                [ProjectPropertyIds.SelectiveVerticalClearanceToken] = SelectivePropertyValueDocument.ToProjectVariable(VarId),
            };
            document.SchemaVersion = SelectivePalletDesignDocument.PromotedSchemaVersion;
        }

        return new SelectivePalletDesignStore().Serialize(document);
    }

    private static ProjectVariablesReadResult Register(double value)
    {
        var document = ProjectVariablesDocument.CreateNew();
        document.Variables = new List<ProjectVariableDocument>
        {
            new ProjectVariableDocument
            {
                VariableId = VarId,
                Name = "Holgura estándar",
                Type = VariableType.Length.ToString(),
                Definition = new ProjectVariableDefinitionDocument { Kind = "literal", Value = value },
            },
        };
        return ProjectVariablesReadResult.Readable(document);
    }

    private static SelectiveDoorWrite WriteOf(SelectiveDesignRead read, Func<SelectivePalletDesign, SelectivePalletDesign>? edit = null)
    {
        var design = read.EffectiveDesign()!;
        design = edit?.Invoke(design) ?? design;
        var finals = read.LinkedPropertyStates!.ToDictionary(
            pair => pair.Key,
            pair => pair.Value.Source.Kind == LinkedPropertySourceKind.Literal
                ? LinkedPropertyEditState.Literal(pair.Key == Clearance ? design.VerticalClearance : design.PalletTolerance)
                : pair.Value);
        return new SelectiveDoorWrite(7, default, RackId, "Selectivo A", design, new SelectiveRackSystem(), finals);
    }

    // ------------------------------------------------------------------ RACKSELECTIVOABRIR

    [Fact]
    public void AnOpenedSelectivoCarriesTheEffectiveDesignAndTheStatesThatRackEditarGivesItsWindow()
    {
        var json = Authored(bound: false);
        var read = RackDoorRules.AnswerForSelectiveOpen(5, RackId, RackEmbedDocument.KindSelective, json, ProjectVariablesReadResult.Absent());

        var open = SelectiveEditorOpen.Resolve(new SelectivePalletDesignStore().Deserialize(json), ProjectVariablesReadResult.Absent());
        Assert.True(read.Opens, read.Reason);
        Assert.Equal((5L, RackId, RackEmbedDocument.KindSelective, "Selectivo A"), (read.RequestId, read.RackId, read.Kind, read.Name));
        Assert.Null(read.Reason);
        Assert.Equal(
            new SelectivePalletDesignStore().Serialize(SelectivePalletDesignDocument.From(open.Design, RackId, "Selectivo A")),
            read.EffectiveDesignJson);
        Assert.Equal(open.LinkedPropertyStates.OrderBy(p => p.Key.Value), read.LinkedPropertyStates!.OrderBy(p => p.Key.Value));
        Assert.False(read.IsLinked(Clearance));
        Assert.False(read.IsLinked(Tolerance));
        Assert.NotSame(read.EffectiveDesign(), read.EffectiveDesign());
    }

    [Fact]
    public void ABoundHolguraOpensWithTheValueOfItsVariableItsFrozenLiteralAndTheNameToShow()
    {
        var read = RackDoorRules.AnswerForSelectiveOpen(5, RackId, RackEmbedDocument.KindSelective, Authored(bound: true), Register(10.0));

        Assert.True(read.Opens, read.Reason);
        Assert.Equal(10.0, read.EffectiveDesign()!.VerticalClearance);
        Assert.True(read.IsLinked(Clearance));
        Assert.True(read.LinkedPropertyStates![Clearance].IsReference);
        Assert.Equal(6.0, read.LinkedPropertyStates[Clearance].CommittedLiteral);
        Assert.Equal("Holgura estándar", read.LinkedVariableNames![Clearance]);
        Assert.False(read.IsLinked(Tolerance));
    }

    [Fact]
    public void ABrokenVariableReferenceBlocksTheOpeningExactlyAsItBlocksRackEditar()
    {
        var json = Authored(bound: true);
        var register = ProjectVariablesReadResult.Absent(); // the variable the rack names is not in the drawing
        var read = RackDoorRules.AnswerForSelectiveOpen(5, RackId, RackEmbedDocument.KindSelective, json, register);

        var rackEditar = SelectiveEditorOpen.Resolve(new SelectivePalletDesignStore().Deserialize(json), register);
        Assert.False(rackEditar.IsOpen);
        Assert.False(read.Opens);
        Assert.Equal(rackEditar.Error, read.Reason);
        Assert.Null(read.EffectiveDesignJson);
        Assert.Null(read.EffectiveDesign());

        var unreadable = RackDoorRules.AnswerForSelectiveOpen(5, RackId, RackEmbedDocument.KindSelective, Authored(bound: false),
            ProjectVariablesReadResult.Unreadable("registro dañado"));
        Assert.False(unreadable.Opens);
        Assert.Equal(SelectiveEditorOpen.Resolve(new SelectivePalletDesignStore().Deserialize(Authored(bound: false)),
            ProjectVariablesReadResult.Unreadable("registro dañado")).Error, unreadable.Reason);
    }

    [Fact]
    public void AnotherKindOrADesignThatCannotBeReadDoesNotOpen()
    {
        var dynamic = RackDoorRules.AnswerForSelectiveOpen(5, RackId, RackEmbedDocument.KindDynamic, Authored(bound: false), ProjectVariablesReadResult.Absent());
        Assert.False(dynamic.Opens);
        Assert.Equal("El rack no es un Selectivo: este editor no lo abre.", dynamic.Reason);

        var broken = RackDoorRules.AnswerForSelectiveOpen(5, RackId, RackEmbedDocument.KindSelective, "{ no es json", ProjectVariablesReadResult.Absent());
        Assert.False(broken.Opens);
        Assert.StartsWith("No se pudieron leer los datos del rack.", broken.Reason);
    }

    // ------------------------------------------------------------------ RACKSELECTIVOAPLICAR

    [Fact]
    public void AWriteWithTheOpenedStatesIsPlannedOnTheRackReadAgainAndKeepsTheBinding()
    {
        var json = Authored(bound: true);
        var read = RackDoorRules.AnswerForSelectiveOpen(5, RackId, RackEmbedDocument.KindSelective, json, Register(10.0));
        var write = WriteOf(read, design => { design.PalletTolerance = 3.0; return design; });

        var plan = RackDoorRules.PlanSelectiveApply(RackEmbedDocument.KindSelective, json, Register(10.0), write);

        Assert.True(plan.CanWrite, plan.Reason);
        Assert.NotNull(plan.Saved);
        var reconciled = LinkedPropertyReconciler.Reconcile(plan.Saved!, write.Design, write.LinkedPropertyFinalStates, Register(10.0), RackId, "Selectivo A");
        Assert.True(reconciled.IsSuccess, reconciled.Error);
        Assert.True(reconciled.Authored.TryGetBinding(Clearance, out _));
        Assert.Equal(6.0, reconciled.Authored.VerticalClearance); // the frozen literal survives the write
        Assert.Equal(10.0, reconciled.Effective.VerticalClearance);
        Assert.Equal(3.0, reconciled.Authored.PalletTolerance);
    }

    [Fact]
    public void AWriteIsRefusedWhenTheBindingOfAHolguraChangedInTheDrawingSinceItOpened()
    {
        // Opened unbound; meanwhile RACKEDITAR bound the clearance. Writing the opened literal state would silently unlink it.
        var read = RackDoorRules.AnswerForSelectiveOpen(5, RackId, RackEmbedDocument.KindSelective, Authored(bound: false), Register(10.0));
        var plan = RackDoorRules.PlanSelectiveApply(RackEmbedDocument.KindSelective, Authored(bound: true), Register(10.0), WriteOf(read));

        Assert.False(plan.CanWrite);
        Assert.Null(plan.Saved);
        // S-24 (M-06 of the review of the entrega 1): the text says what the window can do; «Descartar» goes back to what it opened.
        Assert.Equal("El vínculo de la holgura vertical cambió en el dibujo desde que se abrió el rack aquí: no se escribió nada. "
                     + "Cierra el editor (los cambios de la ventana se pierden) y vuelve a abrirlo con «Editar aquí».", plan.Reason);

        // And the other way round: opened bound, unlinked in the drawing since.
        var bound = RackDoorRules.AnswerForSelectiveOpen(5, RackId, RackEmbedDocument.KindSelective, Authored(bound: true), Register(10.0));
        Assert.False(RackDoorRules.PlanSelectiveApply(RackEmbedDocument.KindSelective, Authored(bound: false), Register(10.0), WriteOf(bound)).CanWrite);
    }

    [Fact]
    public void AWriteIsRefusedWhenTheVariableThatGovernsAHolguraHasAnotherValueNow()
    {
        var json = Authored(bound: true);
        var read = RackDoorRules.AnswerForSelectiveOpen(5, RackId, RackEmbedDocument.KindSelective, json, Register(10.0));

        var plan = RackDoorRules.PlanSelectiveApply(RackEmbedDocument.KindSelective, json, Register(12.0), WriteOf(read));

        Assert.False(plan.CanWrite);
        Assert.Equal("El valor de lo que gobierna la holgura vertical cambió desde que se abrió el rack aquí: no se escribió nada. "
                     + "Cierra el editor (los cambios de la ventana se pierden) y vuelve a abrirlo con «Editar aquí».", plan.Reason);
    }

    // S-24 (M-11 of the review of the entrega 1): when both holguras changed, the refusal names both, in a fixed order.
    [Fact]
    public void ARefusalNamesBothHolgurasWhenBothChanged()
    {
        const string Reopen = " Cierra el editor (los cambios de la ventana se pierden) y vuelve a abrirlo con «Editar aquí».";

        // Opened with both holguras of the rack; meanwhile the drawing bound both.
        var read = RackDoorRules.AnswerForSelectiveOpen(5, RackId, RackEmbedDocument.KindSelective, Authored(bound: false), Register(10.0));
        var rebound = RackDoorRules.PlanSelectiveApply(RackEmbedDocument.KindSelective, AuthoredBoth(), Register(10.0), WriteOf(read));
        Assert.False(rebound.CanWrite);
        Assert.Equal("Los vínculos de la holgura vertical y de la tolerancia horizontal cambiaron en el dibujo desde que se abrió el rack aquí: "
                     + "no se escribió nada." + Reopen, rebound.Reason);

        // Opened with both bound to the variable; the variable has another value now.
        var both = RackDoorRules.AnswerForSelectiveOpen(5, RackId, RackEmbedDocument.KindSelective, AuthoredBoth(), Register(10.0));
        Assert.True(both.Opens, both.Reason);
        Assert.True(both.IsLinked(Clearance) && both.IsLinked(Tolerance));
        var revalued = RackDoorRules.PlanSelectiveApply(RackEmbedDocument.KindSelective, AuthoredBoth(), Register(12.0), WriteOf(both));
        Assert.False(revalued.CanWrite);
        Assert.Equal("Los valores de lo que gobierna la holgura vertical y la tolerancia horizontal cambiaron desde que se abrió el rack aquí: "
                     + "no se escribió nada." + Reopen, revalued.Reason);

        // The same register: it writes.
        Assert.True(RackDoorRules.PlanSelectiveApply(RackEmbedDocument.KindSelective, AuthoredBoth(), Register(10.0), WriteOf(both)).CanWrite);
    }

    // Both holguras governed by the project variable.
    private static string AuthoredBoth()
    {
        var document = SelectivePalletDesignDocument.From(Design(6.0), RackId, "Selectivo A");
        document.PropertyValues = new Dictionary<string, SelectivePropertyValueDocument>
        {
            [ProjectPropertyIds.SelectiveVerticalClearanceToken] = SelectivePropertyValueDocument.ToProjectVariable(VarId),
            [ProjectPropertyIds.SelectivePalletToleranceToken] = SelectivePropertyValueDocument.ToProjectVariable(VarId),
        };
        document.SchemaVersion = SelectivePalletDesignDocument.PromotedSchemaVersion;
        return new SelectivePalletDesignStore().Serialize(document);
    }

    [Fact]
    public void AWriteIsRefusedWhenTheRackWouldNotOpenNowOrIsNotAReadableSelectivo()
    {
        var json = Authored(bound: true);
        var read = RackDoorRules.AnswerForSelectiveOpen(5, RackId, RackEmbedDocument.KindSelective, json, Register(10.0));

        var deleted = RackDoorRules.PlanSelectiveApply(RackEmbedDocument.KindSelective, json, ProjectVariablesReadResult.Absent(), WriteOf(read));
        Assert.False(deleted.CanWrite);
        Assert.Equal(SelectiveEditorOpen.Resolve(new SelectivePalletDesignStore().Deserialize(json), ProjectVariablesReadResult.Absent()).Error
                     + " No se escribió nada.", deleted.Reason);

        Assert.Equal("El rack del dibujo no es un Selectivo legible: no se escribió nada.",
            RackDoorRules.PlanSelectiveApply(RackEmbedDocument.KindDynamic, json, Register(10.0), WriteOf(read)).Reason);
        Assert.Equal("El rack del dibujo no es un Selectivo legible: no se escribió nada.",
            RackDoorRules.PlanSelectiveApply(RackEmbedDocument.KindSelective, "{", Register(10.0), WriteOf(read)).Reason);
    }

    // ------------------------------------------------------------------ the wiring (AutoCAD cannot run here)

    private const string Commands = "src/RackCad.Plugin/VentanaRackCad/RackCadRackDoorCommands.cs";
    private const string Port = "src/RackCad.Plugin/VentanaRackCad/RackCadRackDoorPort.cs";

    [Fact]
    public void TheReadOfASelectivoIsACommandWithoutUndoThatDecidesWithTheRuleOnTheRegisterOfRackEditar()
    {
        var code = WorkspaceHostSource.Code(Commands);
        var read = Method(code, "public void ReadSelectiveDesign()");

        Assert.Contains("[CommandMethod(\"RACKSELECTIVOABRIR\", CommandFlags.NoUndoMarker | CommandFlags.NoHistory)]", code, StringComparison.Ordinal);
        Assert.Contains("RackCadRackDoorPort.TakePendingSelectiveRead()", read, StringComparison.Ordinal);
        Assert.Contains("pending.Document != RackCadRackDoorPort.KeyOf(document)", read, StringComparison.Ordinal);
        Assert.True(At(read, "RackSelectivoCommands.ReadProjectVariables(document, out _)") < At(read, "RackDoorRules.AnswerForSelectiveOpen("));

        // S-27 (M-10 of the review of the entrega 1): a rack that opens carries the dimension styles of the drawing, read as RACKEDITAR reads them.
        Assert.Contains("if (answer.Opens) answer = answer with { DimensionStyles = RackCommandSupport.ReadDimensionStyleNames(document) };", read, StringComparison.Ordinal);
        Assert.True(At(read, "RackDoorRules.AnswerForSelectiveOpen(") < At(read, "RackCommandSupport.ReadDimensionStyleNames(document)"));
        Assert.True(At(read, "RackCommandSupport.ReadDimensionStyleNames(document)") < At(read, "door?.AnswerSelectiveRead(answer);"));
    }

    [Fact]
    public void AReadCarriesTheDimensionStylesOnlyWhenTheCommandAddsThem()
    {
        // The new field is the last one and optional: the callers that do not send it keep working.
        var read = RackDoorRules.AnswerForSelectiveOpen(5, RackId, RackEmbedDocument.KindSelective, Authored(bound: false), ProjectVariablesReadResult.Absent());
        Assert.Null(read.DimensionStyles);
        Assert.Null(SelectiveDesignRead.Refused(5, RackId, null, null, "x").DimensionStyles);
        Assert.Equal(new[] { "ISO-25" }, (read with { DimensionStyles = new[] { "ISO-25" } }).DimensionStyles);
    }

    [Fact]
    public void TheWriteOfASelectivoPlansAgainThenUsesTheDrawingHalfOfRackEditarAndAnswersWithTheRule()
    {
        var code = WorkspaceHostSource.Code(Commands);
        var write = Method(code, "public void WriteSelectiveDesign()");

        Assert.Contains("[CommandMethod(\"RACKSELECTIVOAPLICAR\")]", code, StringComparison.Ordinal);
        var order = new[]
        {
            "RackCadRackDoorPort.TakePendingSelectiveWrite()",
            "door?.Started(pending.RequestId)",
            "pending.Document != RackCadRackDoorPort.KeyOf(document)",
            "RackCommandSupport.FindRackBlocks(document, pending.RackId)",
            "RackSelectivoCommands.ReadProjectVariables(document, out _)",
            "RackDoorRules.PlanSelectiveApply(",
            "RackSelectivoCommands.ApplySelectiveEdit(",
            "RackDoorRules.AnswerForApply(",
        };
        var at = -1;
        foreach (var step in order)
        {
            var next = write.IndexOf(step, at + 1, StringComparison.Ordinal);
            Assert.True(next > at, "out of order or missing: " + step);
            at = next;
        }

        Assert.DoesNotMatch(new System.Text.RegularExpressions.Regex(@"new\s+RackDesignWritten\s*\([^;]*,\s*true\s*,"), write);
    }

    [Fact]
    public void RackEditarAndTheWindowShareTheExtractedActualizarHalf()
    {
        var code = WorkspaceHostSource.Code("src/RackCad.Plugin/RackSelectivoCommands.cs");
        var edit = Method(code, "internal static void EditSelective(");
        var half = Method(code, "internal static (int Redrawn, int Erased) ApplySelectiveEdit(");

        // RACKEDITAR: the update goes through the extracted half, with the authored document and the register it read before opening.
        Assert.Contains("ApplySelectiveEdit(document, blockId, embed, saved, registry, window);", edit, StringComparison.Ordinal);
        // The half reconciles the final states of the editor and serializes the authored document once for every sibling.
        Assert.Contains("LinkedPropertyReconciler.Reconcile(", half, StringComparison.Ordinal);
        Assert.Contains("saved, design, window.LinkedPropertyFinalStates, registry, id, name", half, StringComparison.Ordinal);
        Assert.Equal(1, WorkspaceHostSource.Count(half, @"SelectivePalletDesignStore\(\)\.Serialize"));
        Assert.Contains("return (updatedFrontal + updatedLateral + updatedPlanta, erasedPhantoms);", half, StringComparison.Ordinal);
        Assert.Contains("return (0, 0);", half, StringComparison.Ordinal);
    }

    [Fact]
    public void TheDoorKeepsTheSelectivoRequestsInTheirOwnSlotsAndAbandonsThem()
    {
        var port = WorkspaceHostSource.Code(Port);

        Assert.Contains("SelectiveReadCommand = \"RACKSELECTIVOABRIR\"", port, StringComparison.Ordinal);
        Assert.Contains("SelectiveWriteCommand = \"RACKSELECTIVOAPLICAR\"", port, StringComparison.Ordinal);
        Assert.Contains("class RackCadRackDoorPort : IRackCadSelectiveDoorPort", port, StringComparison.Ordinal);
        var abandon = Method(port, "public void Abandon(long requestId)");
        Assert.Contains("_pendingSelectiveRead = null", abandon, StringComparison.Ordinal);
        Assert.Contains("_pendingSelectiveWrite = null", abandon, StringComparison.Ordinal);
    }

    private static int At(string text, string what)
    {
        var at = text.IndexOf(what, StringComparison.Ordinal);
        Assert.True(at >= 0, "missing: " + what);
        return at;
    }

    // The body of a method, by matching braces from its signature.
    private static string Method(string code, string signature)
    {
        var at = At(code, signature);
        var open = code.IndexOf('{', at);
        var depth = 0;
        for (var i = open; i < code.Length; i++)
        {
            if (code[i] == '{') depth++;
            else if (code[i] == '}' && --depth == 0) return code.Substring(open, i - open + 1);
        }

        Assert.Fail("no body for " + signature);
        return string.Empty;
    }
}
