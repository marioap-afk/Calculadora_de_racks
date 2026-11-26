#nullable enable
using System;
using System.Linq;
using System.Text.RegularExpressions;
using RackCad.Application.Persistence;
using RackCad.Application.RackFrames;
using RackCad.Application.VentanaRackCad;
using RackCad.Application.Workspace;
using RackCad.Domain.RackFrames;
using Xunit;

namespace RackCad.Tests;

/// <summary>
/// I-64, fourth batch, part cb (CB-01 A): the door of a cabecera of its own. The rules are pure and decided here; the commands of AutoCAD
/// (RACKCABECERAABRIR, RACKCABECERAAPLICAR) cannot run here, so their wiring is fixed by their source, as the doors of Dinámico and Selectivo.
/// </summary>
public class CabeceraDoorTests
{
    private const string Commands = "src/RackCad.Plugin/VentanaRackCad/RackCadCabeceraDoorCommands.cs";
    private const string Port = "src/RackCad.Plugin/VentanaRackCad/RackCadRackDoorPort.Cabecera.cs";
    private const string MainPort = "src/RackCad.Plugin/VentanaRackCad/RackCadRackDoorPort.cs";
    private const string Half = "src/RackCad.Plugin/RackCabeceraCommands.Door.cs";
    private const string Classic = "src/RackCad.Plugin/RackCabeceraCommands.cs";

    private static RackFrameConfiguration Header(string? name = null)
    {
        var header = new RackFrameConfigurationFactory(new RackCad.Application.Catalogs.RackCatalog()).Build(RackFrameTemplateCatalog.FindStandardOrDefault(), "POSTE", 132.0, 42.0);
        header.Name = name!;
        return header;
    }

    private static string Envelope(RackFrameConfiguration header) => new RackProjectStore().Serialize(RackProject.ForSelective(header));

    // ------------------------------------------------------------------ the rules

    [Fact]
    public void ACabeceraOpensWithItsHeaderAsItIsAndShowsTheNameOfTheEnvelopeWhenItHasNone()
    {
        var read = CabeceraDoorRules.AnswerForOpen(3, "CAB-1", RackEmbedDocument.KindCabecera, "Cabecera 4", Envelope(Header()));

        Assert.True(read.Opens, read.Reason);
        Assert.Equal(3, read.RequestId);
        Assert.Equal("Cabecera 4", read.Name);
        var header = read.Header();
        Assert.NotNull(header);
        // Correction 7 of the review (the rule of the classic editor): the inner name is not touched; AUTH-15 admits it blank.
        Assert.True(string.IsNullOrEmpty(header!.Name));
        Assert.Equal(new[] { 4.0, 48.0, 92.0, 110.0, 128.0 }, header.Horizontals.Select(h => h.Elevation).ToArray());
        Assert.NotSame(header, read.Header()); // every read is a new object

        var named = CabeceraDoorRules.AnswerForOpen(3, "CAB-1", RackEmbedDocument.KindCabecera, "Cabecera 4", Envelope(Header("Propia")));
        Assert.Equal("Propia", named.Name);
    }

    [Fact]
    public void WhatIsNotAReadableCabeceraDoesNotOpenAndSaysWhy()
    {
        var other = CabeceraDoorRules.AnswerForOpen(1, "X", RackEmbedDocument.KindSelective, "S", Envelope(Header()));
        Assert.False(other.Opens);
        Assert.Equal("El rack no es una cabecera: este editor no lo abre.", other.Reason);
        Assert.Null(other.Header());

        var broken = CabeceraDoorRules.AnswerForOpen(1, "X", RackEmbedDocument.KindCabecera, "C", "no es json");
        Assert.False(broken.Opens);
        Assert.StartsWith("No se pudieron leer los datos de la cabecera.", broken.Reason, StringComparison.Ordinal);
    }

    [Fact]
    public void TheWritePlansOnTheRackOfTheDrawingAndRefusesWhatCannotBeWritten()
    {
        var write = new CabeceraDoorWrite(2, new SessionId(7), "CAB-1", "Cabecera 1", Header("Cabecera 1"));

        var plan = CabeceraDoorRules.PlanApply(RackEmbedDocument.KindCabecera, Envelope(Header()), write);
        Assert.True(plan.CanWrite, plan.Reason);
        Assert.NotNull(plan.Project?.Header);

        Assert.Equal("No hay una cabecera válida que escribir.", CabeceraDoorRules.PlanApply(RackEmbedDocument.KindCabecera, Envelope(Header()), null).Reason);
        Assert.Equal("El rack del dibujo no es una cabecera legible: no se escribió nada.",
            CabeceraDoorRules.PlanApply(RackEmbedDocument.KindDynamic, Envelope(Header()), write).Reason);
        Assert.False(CabeceraDoorRules.PlanApply(RackEmbedDocument.KindCabecera, "{", write).CanWrite);
    }

    // ------------------------------------------------------------------ the wiring (AutoCAD cannot run here)

    [Fact]
    public void TheReadIsACommandWithoutUndoThatComparesTheViewsBeforeItReadsTheFirstOne()
    {
        var code = WorkspaceHostSource.Code(Commands);
        Assert.Contains("[CommandMethod(\"RACKCABECERAABRIR\", CommandFlags.NoUndoMarker | CommandFlags.NoHistory)]", code, StringComparison.Ordinal);
        InOrder(Method(code, "public void ReadCabecera()"),
            "RackCadRackDoorPort.TakePendingCabeceraRead()",
            "pending.Document != RackCadRackDoorPort.KeyOf(document)",
            "RackCommandSupport.FindRackBlocks(document, pending.RackId)",
            "RackSiblingDesigns.Compare(pending.RackId, Views(blocks))",
            "blocks[0]",
            "CabeceraDoorRules.AnswerForOpen(");
    }

    [Fact]
    public void TheWriteTakesItsRequestSaysSoPlansAgainAndRedrawsWithTheDrawingHalf()
    {
        var code = WorkspaceHostSource.Code(Commands);
        Assert.Contains("[CommandMethod(\"RACKCABECERAAPLICAR\")]", code, StringComparison.Ordinal);
        var write = Method(code, "public void WriteCabecera()");
        InOrder(write,
            "RackCadRackDoorPort.TakePendingCabeceraWrite()",
            "door?.Started(pending.RequestId)",
            "pending.Document != RackCadRackDoorPort.KeyOf(document)",
            "RackCommandSupport.FindRackBlocks(document, pending.RackId)",
            "RackSiblingDesigns.Compare(pending.RackId, Views(blocks))",
            "blocks[0]",
            "CabeceraDoorRules.PlanApply(",
            "RackCabeceraCommands.ApplyCabeceraUpdate(",
            "RackDoorRules.AnswerForApply(");

        // «Written» is only what the rule of the door says (I-08 of Dinámico): never a true by hand.
        Assert.DoesNotMatch(new Regex(@"new\s+RackDesignWritten\s*\([^;]*,\s*true\s*,"), write);

        // Correction 7 of the review: the name of the blocks is the rule of EditCabecera (the inner one, else the one of the envelope), and
        // the inner name of the cabecera that is written is not touched.
        Assert.Contains("var name = string.IsNullOrWhiteSpace(cabecera.Name) ? embed.Name : cabecera.Name;", write, StringComparison.Ordinal);
        Assert.Contains("var name = string.IsNullOrWhiteSpace(config?.Name) ? embed.Name : config.Name;", Method(WorkspaceHostSource.Code(Classic), "internal static void EditCabecera("), StringComparison.Ordinal);
        Assert.DoesNotMatch(new Regex(@"cabecera\.Name\s*=[^=]"), write);
    }

    [Fact]
    public void TheDrawingHalfRunsTheStepsOfActualizarOfRackEditarInTheSameOrder()
    {
        // A copy of the branch UpdateOnly of EditCabecera (RACKEDITAR does not change in this batch): the same steps, in the same order.
        var classic = Method(WorkspaceHostSource.Code(Classic), "internal static void EditCabecera(");
        var half = Method(WorkspaceHostSource.Code(Half), "internal static (int Redrawn, int Erased) ApplyCabeceraUpdate(");
        var steps = new[]
        {
            "lateralBlocks = blocks.Where(b => !RackCommandSupport.IsPlantaView(b.Embed)).ToList();",
            "plantaBlocks = blocks.Where(b => RackCommandSupport.IsPlantaView(b.Embed)).ToList();",
            "RackCommandSupport.PreflightInnerSources(",
            "new LateralHeaderDrawService().RedrawInPlace(",
            "BuildCabeceraPayload(config, id, name, RackEmbedDocument.ViewLateral, lat.Embed, preflight.ResolvedByBlock[lat.BlockId]), regen: false);",
            "RackBlockRenamer.SyncName(document, lat.BlockId, RackViewBaseName.LinkedBase(baseName));",
            "new PlantaHeaderDrawService().RedrawInPlace(",
            "BuildCabeceraPayload(config, id, name, RackEmbedDocument.ViewPlanta, pb.Embed, preflight.ResolvedByBlock[pb.BlockId]), regen: false);",
            "RackBlockRenamer.SyncName(document, pb.BlockId, RackViewBaseName.LinkedPlanta(baseName));",
            "document.Editor.Regen();",
        };
        InOrder(Flat(classic), steps.Select(Flat).ToArray());
        InOrder(Flat(half), steps.Select(Flat).ToArray());
        Assert.Contains("RackCommandSupport.FindRackBlocks(document, id)", half, StringComparison.Ordinal);
        Assert.Contains("RackSystemKind.Selective, project", Flat(half), StringComparison.Ordinal);
        Assert.Contains("return (updated, 0);", half, StringComparison.Ordinal);
    }

    [Fact]
    public void TheDoorKeepsTheCabeceraRequestsInTheirOwnSlotsAndAbandonsThem()
    {
        var port = WorkspaceHostSource.Code(Port);
        Assert.Contains("CabeceraReadCommand = \"RACKCABECERAABRIR\"", port, StringComparison.Ordinal);
        Assert.Contains("CabeceraWriteCommand = \"RACKCABECERAAPLICAR\"", port, StringComparison.Ordinal);
        Assert.Contains("partial class RackCadRackDoorPort : IRackCadCabeceraDoorPort", port, StringComparison.Ordinal);
        Assert.Contains("new RackFrameProjectStore().DeepCopy(write.Cabecera)", port, StringComparison.Ordinal); // the copy that waits is the door's
        var abandon = Method(port, "private static void AbandonCabecera(long requestId)");
        Assert.Contains("_pendingCabeceraRead = null", abandon, StringComparison.Ordinal);
        Assert.Contains("_pendingCabeceraWrite = null", abandon, StringComparison.Ordinal);
        Assert.Contains("AbandonCabecera(requestId);", Method(WorkspaceHostSource.Code(MainPort), "public void Abandon(long requestId)"), StringComparison.Ordinal);
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
