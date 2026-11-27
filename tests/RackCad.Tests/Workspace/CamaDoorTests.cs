#nullable enable
using System;
using System.Linq;
using System.Text.RegularExpressions;
using RackCad.Application.Persistence;
using RackCad.Application.VentanaRackCad;
using RackCad.Application.Workspace;
using RackCad.Domain.Systems.FlowBed;
using Xunit;

namespace RackCad.Tests;

/// <summary>
/// I-64, quinta tanda, part cm: the door of a cama de rodamiento. The rules are pure and decided here; the commands of AutoCAD
/// (RACKCAMAABRIR, RACKCAMAAPLICAR, RACKCAMANUEVO) cannot run here, so their wiring is fixed by their source, as the doors of Dinámico,
/// Selectivo and the cabecera.
/// </summary>
public class CamaDoorTests
{
    private const string Commands = "src/RackCad.Plugin/VentanaRackCad/RackCadCamaDoorCommands.cs";
    private const string Port = "src/RackCad.Plugin/VentanaRackCad/RackCadRackDoorPort.Cama.cs";
    private const string MainPort = "src/RackCad.Plugin/VentanaRackCad/RackCadRackDoorPort.cs";
    private const string Half = "src/RackCad.Plugin/RackCamaCommands.Door.cs";
    private const string Classic = "src/RackCad.Plugin/RackCamaCommands.cs";

    private static FlowBedConfiguration Bed(double lane = 96.0, double? pitch = null) => new()
    {
        BedType = FlowBedType.Dynamic,
        LaneDepth = lane,
        PalletDepth = 48.0,
        RollerId = FlowBedDefaults.RollerId,
        RollerPitchOverride = pitch,
    };

    private static string Design(FlowBedConfiguration bed) => new FlowBedConfigurationStore().Serialize(bed);

    // ------------------------------------------------------------------ the rules

    [Fact]
    public void ACamaOpensWithItsBedAndTheNameOfTheEnvelopeAndKeepsItsUnknownFields()
    {
        var json = Design(Bed(120.0, 4.0)).TrimEnd('}') + ",\"Futuro\":7}";
        var read = CamaDoorRules.AnswerForOpen(3, "CAMA-1", RackEmbedDocument.KindCama, "Cama 1", json);

        Assert.True(read.Opens, read.Reason);
        Assert.Equal((3L, "CAMA-1", "Cama 1"), (read.RequestId, read.RackId, read.Name));
        var bed = read.Bed()!;
        Assert.Equal((120.0, 4.0), (bed.LaneDepth, bed.RollerPitchOverride));
        Assert.NotSame(bed, read.Bed()); // every read is a new object
        Assert.True(read.Document()!.ExtensionData!.ContainsKey("Futuro")); // I-11: a save to the library keeps it
    }

    [Fact]
    public void WhatIsNotAReadableCamaDoesNotOpenAndSaysWhy()
    {
        var other = CamaDoorRules.AnswerForOpen(1, "X", RackEmbedDocument.KindDynamic, "D", Design(Bed()));
        Assert.False(other.Opens);
        Assert.Equal("El rack no es una cama de rodamiento: este editor no la abre.", other.Reason);
        Assert.Null(other.Bed());

        Assert.Equal("Datos de cama inválidos.", CamaDoorRules.AnswerForOpen(1, "X", RackEmbedDocument.KindCama, "C", "no es json").Reason);
        Assert.Equal("Datos de cama inválidos.", CamaDoorRules.AnswerForOpen(1, "X", RackEmbedDocument.KindCama, "C", Design(Bed(0.0))).Reason);
    }

    [Fact]
    public void TheWritePlansOnTheCamaOfTheDrawingAndRefusesWhatCannotBeWritten()
    {
        var write = new CamaDoorWrite(2, new SessionId(7), "CAMA-1", "Cama 1", Bed(110.0));

        var plan = CamaDoorRules.PlanApply(RackEmbedDocument.KindCama, Design(Bed()), write);
        Assert.True(plan.CanWrite, plan.Reason);
        Assert.Equal(96.0, plan.Source!.LaneDepth); // the document of the drawing, for its unknown fields

        Assert.Equal("No hay una cama válida que escribir.", CamaDoorRules.PlanApply(RackEmbedDocument.KindCama, Design(Bed()), null).Reason);
        Assert.Equal("No hay una cama válida que escribir.", CamaDoorRules.PlanApply(RackEmbedDocument.KindCama, Design(Bed()), write with { Bed = Bed(0.0) }).Reason);
        Assert.Equal("El rack del dibujo no es una cama legible: no se escribió nada.", CamaDoorRules.PlanApply(RackEmbedDocument.KindCabecera, Design(Bed()), write).Reason);
        Assert.False(CamaDoorRules.PlanApply(RackEmbedDocument.KindCama, "{", write).CanWrite);
    }

    [Fact]
    public void TheAnswersOfTheWritesSayWhatAutoCadDid()
    {
        var written = CamaDoorRules.AnswerForApply(5, "CAMA-1", redrawn: true, error: null);
        Assert.Equal((5L, "CAMA-1", true), (written.RequestId, written.RackId, written.Written));
        Assert.Equal("AutoCAD redibujó la cama con el borrador: todas sus copias en el dibujo reflejan el cambio.", written.Message);

        var not = CamaDoorRules.AnswerForApply(5, "CAMA-1", redrawn: false, error: "Falta el bloque.");
        Assert.False(not.Written);
        Assert.Equal("AutoCAD no redibujó la cama, así que el borrador sigue sin escribir. Falta el bloque.", not.Message);

        var placed = CamaDoorRules.AnswerForNewCama(6, "CAMA-9", placed: 1, name: " Cama 3 ");
        Assert.True(placed.Written);
        Assert.Equal("Cama 3", placed.Name);
        Assert.Equal("AutoCAD insertó la cama nueva «Cama 3». Para seguir editándola aquí, vuelve a leer la lista y ábrela.", placed.Message);

        var cancelled = CamaDoorRules.AnswerForNewCama(6, "CAMA-9", placed: 0, name: null);
        Assert.False(cancelled.Written);
        Assert.StartsWith("AutoCAD no insertó la cama nueva", cancelled.Message, StringComparison.Ordinal);

        var write = new CamaDoorWrite(6, new SessionId(7), "CAMA-9", null!, Bed(), NewRack: true);
        Assert.Null(CamaDoorRules.RefusalForNewCama(write, 0));
        Assert.Equal("Ya hay un rack con esa identidad en el dibujo (1): no se insertó nada.", CamaDoorRules.RefusalForNewCama(write, 1));
        Assert.Equal("No hay una cama válida que insertar.", CamaDoorRules.RefusalForNewCama(write with { Bed = Bed(0.0) }, 0));
    }

    // ------------------------------------------------------------------ the wiring (AutoCAD cannot run here)

    [Fact]
    public void TheReadIsACommandWithoutUndoThatComparesTheViewsBeforeItReadsTheFirstOne()
    {
        var code = WorkspaceHostSource.Code(Commands);
        Assert.Contains("[CommandMethod(\"RACKCAMAABRIR\", CommandFlags.NoUndoMarker | CommandFlags.NoHistory)]", code, StringComparison.Ordinal);
        InOrder(Method(code, "public void ReadCama()"),
            "RackCadRackDoorPort.TakePendingCamaRead()",
            "pending.Document != RackCadRackDoorPort.KeyOf(document)",
            "RackCommandSupport.FindRackBlocks(document, pending.RackId)",
            "RackSiblingDesigns.Compare(pending.RackId, Views(blocks))",
            "blocks[0]",
            "CamaDoorRules.AnswerForOpen(",
            "RackPropertiesDoorData.Read(document, blocks[0].BlockId)");
    }

    [Fact]
    public void TheUpdateTakesItsRequestSaysSoPlansAgainRedrawsWithTheDrawingHalfAndThenWritesTheProperties()
    {
        var code = WorkspaceHostSource.Code(Commands);
        Assert.Contains("[CommandMethod(\"RACKCAMAAPLICAR\")]", code, StringComparison.Ordinal);
        var write = Method(code, "public void WriteCama()");
        InOrder(write,
            "RackCadRackDoorPort.TakePendingCamaWrite()",
            "door?.Started(pending.RequestId)",
            "pending.Document != RackCadRackDoorPort.KeyOf(document)",
            "RackCommandSupport.FindRackBlocks(document, pending.RackId)",
            "RackSiblingDesigns.Compare(pending.RackId, Views(blocks))",
            "blocks[0]",
            "CamaDoorRules.PlanApply(",
            "RackCamaCommands.ApplyCamaUpdate(document, blockId, embed, plan.Source, pending.Write.Bed, pending.RackId, pending.Write.Name)",
            "RackPropertiesDoorData.WithRackProperties(",
            "CamaDoorRules.AnswerForApply(");

        // «Written» is only what the rule of the door says (I-08 of Dinámico): never a true by hand.
        Assert.DoesNotMatch(new Regex(@"new\s+RackDesignWritten\s*\([^;]*,\s*true\s*,"), write);
    }

    [Fact]
    public void TheNewCamaIsInsertedWithTheDispatchOfNuevoAndCountsWhatWasPlaced()
    {
        var code = WorkspaceHostSource.Code(Commands);
        Assert.Contains("[CommandMethod(\"RACKCAMANUEVO\")]", code, StringComparison.Ordinal);
        var insert = Method(code, "public void InsertNewCama()");
        InOrder(insert,
            "RackCadRackDoorPort.TakePendingCamaNew()",
            "door?.Started(pending.RequestId)",
            "pending.Document != RackCadRackDoorPort.KeyOf(document)",
            "CamaDoorRules.RefusalForNewCama(pending.Write, RackCommandSupport.FindRackBlocks(document, pending.RackId).Count)",
            "RackMenuCommands.DispatchInsertion(document,",
            "new RackCad.UI.Editor.FlowBedInsertionRequest(pending.Write.Bed, pending.RackId, pending.Write.Name, source)",
            "RackCommandSupport.FindRackBlocks(document, pending.RackId)",
            "CamaDoorRules.AnswerForNewCama(");
        Assert.DoesNotMatch(new Regex(@"new\s+RackDesignWritten\s*\([^;]*,\s*true\s*,"), insert);
    }

    [Fact]
    public void TheDrawingHalfRunsTheStepsOfActualizarOfRackEditarInTheSameOrder()
    {
        // A copy of what EditCama does after its window (RACKEDITAR does not change in this batch, and the guard of I-61 keeps reading it).
        var classic = Method(WorkspaceHostSource.Code(Classic), "internal static void EditCama(");
        var tail = classic.Substring(classic.IndexOf("if (!window.InsertRequested)", StringComparison.Ordinal))
            .Replace("window.FlowBedToInsert", "config").Replace("window.RackId", "rackId").Replace("window.RackName", "name");
        var half = Method(WorkspaceHostSource.Code(Half), "internal static HeaderPlacementResult ApplyCamaUpdate(");
        var steps = new[]
        {
            "var resolvedName = EditedRackNameResolver.Resolve(name, embed.Name);",
            "var result = new FlowBedDrawService().RedrawInPlace( document, blockId, config, BuildCamaPayload(config, rackId, resolvedName, embed, sourceDesign));",
            "if (result != null && result.Success) { RackBlockRenamer.SyncName(document, blockId, RackViewBaseName.LinkedBase(resolvedName)); }",
            "editor.WriteMessage(result != null && result.Success ? \"\\nRackCad: cama actualizada; todas sus copias reflejan el cambio.\" : \"\\nRackCad: no se pudo actualizar la cama. \" + (result?.ErrorMessage ?? string.Empty));",
        };
        InOrder(Flat(tail), steps.Select(Flat).ToArray());
        InOrder(Flat(half), steps.Select(Flat).ToArray());
        Assert.Contains("return result;", half, StringComparison.Ordinal);
        Assert.Contains("partial class RackCamaCommands", WorkspaceHostSource.Code(Classic), StringComparison.Ordinal);
    }

    [Fact]
    public void TheDoorKeepsTheCamaRequestsInTheirOwnSlotsWithItsOwnCopyAndAbandonsThem()
    {
        var port = WorkspaceHostSource.Code(Port);
        Assert.Contains("CamaReadCommand = \"RACKCAMAABRIR\"", port, StringComparison.Ordinal);
        Assert.Contains("CamaWriteCommand = \"RACKCAMAAPLICAR\"", port, StringComparison.Ordinal);
        Assert.Contains("CamaNewCommand = \"RACKCAMANUEVO\"", port, StringComparison.Ordinal);
        Assert.Contains("partial class RackCadRackDoorPort : IRackCadCamaDoorPort", port, StringComparison.Ordinal);
        Assert.Contains("Bed = CamaDraftRules.Copy(write.Bed)", port, StringComparison.Ordinal); // the copy that waits is the door's
        var abandon = Method(port, "private static void AbandonCama(long requestId)");
        Assert.Contains("_pendingCamaRead = null", abandon, StringComparison.Ordinal);
        Assert.Contains("_pendingCamaWrite = null", abandon, StringComparison.Ordinal);
        Assert.Contains("_pendingCamaNew = null", abandon, StringComparison.Ordinal);
        Assert.Contains("AbandonCama(requestId);", Method(WorkspaceHostSource.Code(MainPort), "public void Abandon(long requestId)"), StringComparison.Ordinal);

        // A new cama goes to its own command; an update to the other one.
        var request = Method(port, "public RackCadClassicResult RequestCamaWrite(CamaDoorWrite write)");
        InOrder(request, "if (write.NewRack)", "CamaNewCommand", "CamaWriteCommand");
    }

    private static string Flat(string text) => Regex.Replace(text, @"\s+", " ");

    private static void InOrder(string text, params string[] steps)
    {
        var at = -1;
        foreach (var step in steps)
        {
            var next = text.IndexOf(step, at + 1, StringComparison.Ordinal);
            Assert.True(next > at, "out of order or missing: " + step);
            at = next;
        }
    }

    private static string Method(string code, string signature)
    {
        var at = code.IndexOf(signature, StringComparison.Ordinal);
        Assert.True(at >= 0, "missing: " + signature);
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
