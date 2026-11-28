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
        Assert.Contains("_pendingCabeceraInsert = null", abandon, StringComparison.Ordinal); // T7 cab
        Assert.Contains("_pendingCabeceraNew = null", abandon, StringComparison.Ordinal); // T7 cab
        Assert.Contains("AbandonCabecera(requestId);", Method(WorkspaceHostSource.Code(MainPort), "public void Abandon(long requestId)"), StringComparison.Ordinal);

        // T7 cab: the rule of the door refuses first; a new cabecera and «Insertar vista…» have their own commands and slots, and the request
        // that waits keeps its own copy of the views.
        Assert.Contains("CabeceraInsertCommand = \"RACKCABECERAINSERTAR\"", port, StringComparison.Ordinal);
        Assert.Contains("CabeceraNewCommand = \"RACKCABECERANUEVO\"", port, StringComparison.Ordinal);
        var update = Method(port, "public RackCadClassicResult RequestCabeceraUpdate(CabeceraDoorWrite write)");
        InOrder(update, "CabeceraDoorRules.RefusalOf(write)", "write.NewRack ? CabeceraNewCommand : write.Inserts ? CabeceraInsertCommand : CabeceraWriteCommand",
            "new List<RackViewAddress>(write.InsertViews)", "SetCabeceraPending(write, null)");
        var slots = Method(port, "private static void SetCabeceraPending(CabeceraDoorWrite write, PendingCabeceraWrite pending)");
        InOrder(slots, "if (write.NewRack) _pendingCabeceraNew = pending;", "else if (write.Inserts) _pendingCabeceraInsert = pending;", "else _pendingCabeceraWrite = pending;");
    }

    // ================================================================== T7 cab (séptima tanda): «Nuevo» › Cabecera, «Insertar vista…», «Propiedades»

    private static readonly RackCad.Application.Systems.Shared.RackViewAddress[] Lateral =
        { RackCad.Application.Systems.Shared.RackViewAddress.Whole(RackCad.Application.Systems.Shared.DimensionViewKind.Lateral) };

    [Fact]
    public void TheRuleOfTheDoorRefusesAWriteWithNoCabeceraAndANewOneWithNoViews()
    {
        var header = Header("Cabecera 1");
        var session = new SessionId(7);
        Assert.Null(CabeceraDoorRules.RefusalOf(new CabeceraDoorWrite(1, session, "CAB-1", "Cabecera 1", header)));
        Assert.Null(CabeceraDoorRules.RefusalOf(new CabeceraDoorWrite(1, session, "CAB-1", "", header, Lateral, NewRack: true)));
        Assert.Equal("No hay una cabecera válida que escribir.", CabeceraDoorRules.RefusalOf(null!));
        Assert.Equal("No hay una cabecera válida que escribir.", CabeceraDoorRules.RefusalOf(new CabeceraDoorWrite(1, session, " ", "", header)));
        Assert.Equal("No hay una cabecera válida que escribir.", CabeceraDoorRules.RefusalOf(new CabeceraDoorWrite(1, session, "CAB-1", "", null!)));
        Assert.Equal("Elige al menos una vista para insertar la cabecera.",
            CabeceraDoorRules.RefusalOf(new CabeceraDoorWrite(1, session, "CAB-1", "", header, NewRack: true)));
        Assert.Equal("Elige al menos una vista para insertar la cabecera.",
            CabeceraDoorRules.RefusalOf(new CabeceraDoorWrite(1, session, "CAB-1", "", header, Array.Empty<RackCad.Application.Systems.Shared.RackViewAddress>(), NewRack: true)));
        Assert.True(new CabeceraDoorWrite(1, session, "CAB-1", "", header, Lateral).Inserts);
        Assert.False(new CabeceraDoorWrite(1, session, "CAB-1", "", header).Inserts);
    }

    [Fact]
    public void TheReadCarriesThePropertiesOfTheRackForTheTabPropiedadesAndTheWriteWritesThem()
    {
        var properties = new RackPropertiesRead(true, false, null, Array.Empty<RackCad.Application.CustomProperties.CustomPropertyDraftEntry>(),
            Array.Empty<RackPropertiesViewSummary>(), 1, "fp");
        var read = CabeceraDoorRules.AnswerForOpen(3, "CAB-1", RackEmbedDocument.KindCabecera, "Cabecera 4", Envelope(Header()), properties);
        Assert.True(read.Opens, read.Reason);
        Assert.Same(properties, read.Properties);
        Assert.Null(CabeceraDoorRules.AnswerForOpen(3, "CAB-1", RackEmbedDocument.KindCabecera, "Cabecera 4", Envelope(Header())).Properties);

        // The command reads them from the first view, after comparing the views; RACKCABECERAAPLICAR writes the tab after the redraw (VA-07).
        var code = WorkspaceHostSource.Code(Commands);
        InOrder(Method(code, "public void ReadCabecera()"), "RackSiblingDesigns.Compare(pending.RackId, Views(blocks))", "var (blockId, embed) = blocks[0];",
            "CabeceraDoorRules.AnswerForOpen(", "RackPropertiesDoorData.Read(document, blockId)");
        InOrder(Method(code, "public void WriteCabecera()"), "RackCabeceraCommands.ApplyCabeceraUpdate(",
            "RackPropertiesDoorData.WithRackProperties(document, pending.RackId, pending.Write.Properties,", "RackDoorRules.AnswerForApply(");
    }

    [Fact]
    public void InsertarVistaTakesItsRequestChecksAsTheWriteAndInsertsWithTheDrawingHalfOfInsertar()
    {
        var code = WorkspaceHostSource.Code(Commands);
        Assert.Contains("[CommandMethod(\"RACKCABECERAINSERTAR\")]", code, StringComparison.Ordinal);
        var insert = Method(code, "public void InsertCabeceraViews()");
        InOrder(insert,
            "RackCadRackDoorPort.TakePendingCabeceraInsert()",
            "door?.Started(pending.RequestId)",
            "pending.Document != RackCadRackDoorPort.KeyOf(document)",
            "RackCommandSupport.FindRackBlocks(document, pending.RackId)",
            "RackSiblingDesigns.Compare(pending.RackId, Views(blocks))",
            "var (blockId, embed) = blocks[0];",
            "CabeceraDoorRules.PlanApply(",
            "var name = string.IsNullOrWhiteSpace(cabecera.Name) ? embed.Name : cabecera.Name;",
            "var id = string.IsNullOrWhiteSpace(embed.Id) ? pending.RackId : embed.Id;",
            "RackCabeceraCommands.ApplyCabeceraInsert(document, blockId, embed, plan.Project, cabecera, id, name, pending.Write.InsertViews);",
            "RackCommandSupport.FindRackBlocks(document, pending.RackId).Count - (blocks.Count - erased)",
            "RackDoorRules.AnswerForInsert(pending.RequestId, pending.RackId, redrawn, erased, inserted)");
        Assert.DoesNotMatch(new Regex(@"new\s+RackDesignWritten\s*\([^;]*,\s*true\s*,"), insert);
        Assert.DoesNotMatch(new Regex(@"cabecera\.Name\s*=[^=]"), insert);
        Assert.DoesNotContain("WithRackProperties", insert, StringComparison.Ordinal); // VA-S10: «Insertar vista…» writes no properties, as the other doors
    }

    [Fact]
    public void ANewCabeceraIsInsertedByTheDispatchOfNuevoWithTheIdentityOfTheWindowAndThenItsProperties()
    {
        var code = WorkspaceHostSource.Code(Commands);
        Assert.Contains("[CommandMethod(\"RACKCABECERANUEVO\")]", code, StringComparison.Ordinal);
        var nuevo = Method(code, "public void InsertNewCabecera()");
        InOrder(nuevo,
            "RackCadRackDoorPort.TakePendingCabeceraNew()",
            "door?.Started(pending.RequestId)",
            "pending.Document != RackCadRackDoorPort.KeyOf(document)",
            "RackCommandSupport.FindRackBlocks(document, pending.RackId).Count > 0",
            "RackMenuCommands.DispatchInsertion(document,",
            "RackCad.UI.VentanaRackCad.CabeceraDoorInsertion.NewRackRequest(write.RackId, write.Cabecera, write.InsertViews)",
            "var placed = RackCommandSupport.FindRackBlocks(document, pending.RackId);",
            "RackPropertiesDoorData.WithNewRackProperties(document, pending.RackId, write.Properties,",
            "RackDoorRules.AnswerForNewRack(pending.RequestId, pending.RackId, placed.Count,");
        Assert.DoesNotMatch(new Regex(@"new\s+RackDesignWritten\s*\([^;]*,\s*true\s*,"), nuevo);

        // The request of a new rack is the one of the classic configurator opened from «Nuevo»: the cabecera, the identity of the window, no
        // project, and every view in its order (the dispatch names it with I-60 and places the batch).
        var insertion = WorkspaceHostSource.Code("src/RackCad.UI/VentanaRackCad/CabeceraDoorInsertion.cs");
        InOrder(insertion, "if (cabecera == null || views == null || views.Count == 0) return null;",
            "new HeaderInsertionRequest(cabecera, sourceProject: null, rackId, views[0]);", "request.SetViews(views);");
    }

    // Review of 258c16b1, I-1: the half of «Insertar» was a COPY of the branch of EditCabecera, and a guard of fragments in order did not see
    // a missing return after a refusal. The classic stays (order of the Owner), so the copy would have been for good: it was EXTRACTED.
    // RACKEDITAR «Insertar» and RACKCABECERAINSERTAR run the ONE drawing half ApplyCabeceraInsert, which is the old branch as it was.
    [Fact]
    public void RackEditarInsertarAndInsertarVistaOfTheWindowRunTheOneDrawingHalfOfInsertar()
    {
        var classic = Flat(Method(WorkspaceHostSource.Code(Classic), "internal static void EditCabecera("));
        // The branch «Insertar» of the classic is the call, and only it: after the identity, before the views of «Actualizar».
        const string Call = "if (!window.UpdateOnly) { ApplyCabeceraInsert(document, blockId, embed, project, config, id, name, window.InsertViews); return; }";
        Assert.Single(Regex.Matches(classic, Regex.Escape(Call)));
        InOrder(classic, "var name = string.IsNullOrWhiteSpace(config?.Name) ? embed.Name : config.Name;",
            "var id = !window.UpdateOnly && string.IsNullOrWhiteSpace(embed.Id) ? window.RackId :", Call, "RackCommandSupport.FindRackBlocks(document, id)");
        // What is left of EditCabecera is «Actualizar»: it authorizes nothing, prepares no queue and does not warn about the units (I-05).
        foreach (var gone in new[] { "TryAuthorize", "authorized", "RackViewBatch", "RackUnitsGuard", "PrepareAll", "insertViews" })
            Assert.DoesNotContain(gone, classic, StringComparison.Ordinal);

        // The door of the window calls the same half, with the identity of the envelope (the request only as a fallback, as RACKEDITAR).
        var door = Flat(Method(WorkspaceHostSource.Code(Commands), "public void InsertCabeceraViews()"));
        InOrder(door, "var id = string.IsNullOrWhiteSpace(embed.Id) ? pending.RackId : embed.Id;",
            "RackCabeceraCommands.ApplyCabeceraInsert(document, blockId, embed, plan.Project, cabecera, id, name, pending.Write.InsertViews);");

        // Nobody else calls it, and there is one drawing half of «Insertar» of a cabecera.
        var callers = WorkspaceHostSource.Files("src")
            .SelectMany(f => Regex.Matches(f.Code, @"\bApplyCabeceraInsert\s*\(").Select(_ => f.Path))
            .OrderBy(n => n, StringComparer.Ordinal).ToArray();
        Assert.Equal(new[] { Classic, Half, Commands }.OrderBy(n => n, StringComparer.Ordinal).ToArray(), callers); // two calls and the definition
    }

    // Review of 258c16b1, I-1: the drawing half of «Insertar» stops BEFORE any geometry when the authored edge (AUTH-13), the preflight of the
    // inner designs (I-11) or the queue of ID18 (B-1) refuse; each refusal is the exact block, with its return, and comes before the first
    // redraw. Then the steps of the old branch, in their order.
    [Fact]
    public void TheDrawingHalfOfInsertarStopsBeforeAnyGeometryWhenTheEdgeThePreflightOrTheQueueRefuse()
    {
        var half = Flat(Method(WorkspaceHostSource.Code(Half), "internal static (int Redrawn, int Erased) ApplyCabeceraInsert("));
        Assert.Empty(StopViolations(half));

        // The guard sees a half that goes on after a refusal (it would redraw the views and place nothing, or place from the old design).
        Assert.NotEmpty(StopViolations(half.Replace("{ editor.WriteMessage(\"\\nRackCad: \" + preflight.ErrorMessage); return (0, 0); }",
            "{ editor.WriteMessage(\"\\nRackCad: \" + preflight.ErrorMessage); }")));
        Assert.NotEmpty(StopViolations(half.Replace("+ batchDiagnostic); return (0, 0); }", "+ batchDiagnostic); }")));
        Assert.NotEmpty(StopViolations(half.Replace("out var authorized)) return (0, 0);", "out var authorized)) { }")));
        Assert.NotEmpty(StopViolations(half.Replace("RackUnitsGuard.WarnIfNotInches(document);", string.Empty)));
    }

    private static System.Collections.Generic.List<string> StopViolations(string half)
    {
        var violations = new System.Collections.Generic.List<string>();
        var stops = new[]
        {
            "if (!RackUnsupportedSiblingInsert.TryAuthorize(document, blockId, embed, id, out var authorized)) return (0, 0);",
            "if (preflight.Aborted) { editor.WriteMessage(\"\\nRackCad: \" + preflight.ErrorMessage); return (0, 0); }",
            "if (!batchProducts.PrepareAll(batchRequest, out var batchDiagnostic)) { editor.WriteMessage(\"\\nRackCad: no se preparo la cola ID18; no se modifico ningun bloque. \" + batchDiagnostic); return (0, 0); }",
        };
        var firstRedraw = half.IndexOf("RedrawInPlace(", StringComparison.Ordinal);
        foreach (var stop in stops)
        {
            var at = half.IndexOf(stop, StringComparison.Ordinal);
            if (at < 0) violations.Add("falta la parada: " + stop);
            else if (firstRedraw >= 0 && at > firstRedraw) violations.Add("la parada va después del primer redibujo: " + stop);
        }

        var steps = new[]
        {
            "RackUnsupportedSiblingInsert.TryAuthorize(document, blockId, embed, id, out",
            "var blocks = authorized.Blocks.ToList();",
            "lateralBlocks = blocks.Where(b => !RackCommandSupport.IsPlantaView(b.Embed)).ToList();",
            "plantaBlocks = blocks.Where(b => RackCommandSupport.IsPlantaView(b.Embed)).ToList();",
            "if (lateralBlocks.Count == 0 && plantaBlocks.All(b => b.BlockId != blockId) && !RackCommandSupport.IsPlantaView(embed))",
            "lateralBlocks.Add((blockId, embed));",
            "RackCommandSupport.PreflightInnerSources(",
            "RackUnitsGuard.WarnIfNotInches(document);",
            "RackViewBatchProducts.Header( document, config, id, name, embed, project, authorized.AuthoredInput);",
            "RackCad.Application.Views.Preparation.RackProductSourceKind.ExistingRack, RackSystemKind.Selective, id, insertViews);",
            ".PrepareAll(batchRequest, out var batchDiagnostic)",
            "new LateralHeaderDrawService().RedrawInPlace(",
            "BuildCabeceraPayload(config, id, name, RackEmbedDocument.ViewLateral, lat.Embed, preflight.ResolvedByBlock[lat.BlockId]), regen: false);",
            "RackBlockRenamer.SyncName(document, lat.BlockId, RackViewBaseName.LinkedBase(baseName));",
            "new PlantaHeaderDrawService().RedrawInPlace(",
            "BuildCabeceraPayload(config, id, name, RackEmbedDocument.ViewPlanta, pb.Embed, preflight.ResolvedByBlock[pb.BlockId]), regen: false);",
            "RackBlockRenamer.SyncName(document, pb.BlockId, RackViewBaseName.LinkedPlanta(baseName));",
            "document.Editor.Regen();",
            "RackViewBatchExecution.Run(document, batchProducts, batchRequest,",
            "RackInsertRedrawOutcome.Of(lateralBlocks.Count + plantaBlocks.Count, 0, updated, 0)",
            "return (updated, 0);",
        };
        var position = -1;
        foreach (var step in steps.Select(Flat))
        {
            var next = half.IndexOf(step, position + 1, StringComparison.Ordinal);
            if (next <= position)
            {
                violations.Add("falta o está fuera de orden: " + step);
                continue;
            }

            position = next;
        }

        // Three refusals and the end: no other return; nothing writes the name of the cabecera and nothing is modal.
        if (Regex.Matches(half, @"\breturn\b").Count != 4) violations.Add("otro return");
        if (Regex.IsMatch(half, @"config\.Name\s*=[^=]")) violations.Add("escribe el nombre");
        if (half.Contains("ShowModalWindow", StringComparison.Ordinal)) violations.Add("una ventana modal");
        return violations;
    }

    [Fact]
    public void RackCabeceraOpensTheWindowAndTheClassicConfiguratorStaysReachableFromTheClassicMenu()
    {
        // The Owner on 2026-11-27: «Abren la ventana». RACKCABECERA and its alias open «Nuevo» › «Cabecera en esta ventana», with the flags of
        // the shortcuts of rl (they read nothing and write nothing); the body of the classic stays, not registered.
        var code = WorkspaceHostSource.Code(Classic);
        Assert.Contains("[CommandMethod(\"RCB\", RackCadWindowShortcuts.WindowFlags)] public void AliasRackCabecera() => RackCabecera();", code, StringComparison.Ordinal);
        Assert.Contains("[CommandMethod(\"RACKCABECERA\", RackCadWindowShortcuts.WindowFlags)]", code, StringComparison.Ordinal);
        Assert.Single(Regex.Matches(code, @"\[CommandMethod\(""RACKCABECERA"""));
        var open = Method(code, "public void RackCabecera()");
        Assert.Contains("RackCadWindowShortcuts.Open(document, new RackCad.Application.VentanaRackCad.RackCadShortcutAsk(", open, StringComparison.Ordinal);
        Assert.Contains("RackCad.Application.VentanaRackCad.RackCadShortcutTarget.NewCabecera, null, null));", Flat(open), StringComparison.Ordinal);
        Assert.DoesNotContain("ShowModalWindow", open, StringComparison.Ordinal);
        Assert.DoesNotContain("DrawAndPlace", open, StringComparison.Ordinal);
        InOrder(Method(code, "internal void RackCabeceraClassic()"), "new HardcodedStandardRackFrameService().CreateDefault()",
            "new RackFrameConfiguratorWindow(configuration, canInsertInAutoCad: true)", "AcApplication.ShowModalWindow(window);", "DrawAndPlace(");

        // The classic menu opens the configurator with its button «Diseñar cabecera», through the registry of modules (not RACKCABECERA).
        Assert.Contains("private void DesignHeader_Click(object sender, RoutedEventArgs e) => LaunchDesignModule(RackSystemKind.Selective);",
            WorkspaceHostSource.Code("src/RackCad.UI/RackMainMenuWindow.xaml.cs"), StringComparison.Ordinal);
        var module = Method(WorkspaceHostSource.Code("src/RackCad.UI/Editor/EditorModules.cs"), "public sealed class HeaderEditorModule : IRackEditorModule");
        InOrder(module, "public RackInsertionRequest OpenForNew(RackEditorLaunchContext context)", "new HardcodedStandardRackFrameService().CreateDefault()",
            "new RackFrameConfiguratorWindow(configuration, context.CanInsertInAutoCad)");
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
