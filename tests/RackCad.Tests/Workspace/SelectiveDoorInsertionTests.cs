using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Application.ProjectVariables;
using RackCad.Application.Systems.Selective;
using RackCad.Application.Systems.Shared;
using RackCad.Application.VentanaRackCad;
using RackCad.Domain.Systems.Selective;
using RackCad.Domain.Systems.Shared;
using Xunit;
using static RackCad.Tests.DimensionViewScenarios;

namespace RackCad.Tests
{
    /// <summary>
    /// Selectivo E2, part A (I-64 A-14, line D): the rules of «Insertar vista…» and of the new Selectivo of the RackCad window that need
    /// no AutoCAD (<see cref="SelectiveDoorInsertion"/>), and the write of the door that carries them (<see cref="SelectiveDoorWrite"/>).
    /// </summary>
    public sealed class SelectiveDoorInsertionTests
    {
        private static SelectiveDoorWrite Write(SelectiveRackSystem system, IReadOnlyList<RackViewAddress> views = null, bool newRack = false, string rackId = "rack-1")
            => new SelectiveDoorWrite(3, default, rackId, "Selectivo A", new SelectivePalletDesign(), system, null, views, newRack);

        [Fact]
        public void ViewOptions_ListAFrontalPerFondo_ThenTheCuts_ThenThePlan_AsTheClassicInsertarVarias()
        {
            var catalog = Catalog;
            var system = Selective(DimensionDetail.Standard, twoFondos: true, catalog);
            var options = SelectiveDoorInsertion.ViewOptions(system, catalog);

            var fondos = SelectiveDepthLayout.Count(system);
            var cuts = new SelectiveLateralBuilder().Cortes(system, catalog);
            var expected = Enumerable.Range(0, fondos).Select(i => ("Frontal · fondo " + (i + 1), RackViewAddress.Fondo(i)))
                .Concat(cuts.Select(cut => ("Lateral · poste " + (cut.PostIndex + 1), RackViewAddress.Post(cut.PostIndex))))
                .Append(("Planta", RackViewAddress.Whole(DimensionViewKind.Planta)))
                .ToList();

            Assert.Equal(2, fondos);
            Assert.NotEmpty(cuts);
            Assert.Equal(expected, options.Select(option => (option.Label, option.Address)).ToList());
        }

        [Fact]
        public void ViewOptions_WithoutASystem_OfferOneFrontalAndThePlan()
        {
            var options = SelectiveDoorInsertion.ViewOptions(null, null);
            Assert.Equal(new[] { "Frontal · fondo 1", "Planta" }, options.Select(option => option.Label).ToArray());
        }

        [Fact]
        public void DrawableViews_KeepTheOrder_DropRepeats_AndViewsTheRackCannotDraw()
        {
            var catalog = Catalog;
            var system = Selective(DimensionDetail.Standard, twoFondos: false, catalog);
            var post = new SelectiveLateralBuilder().Cortes(system, catalog).First().PostIndex;
            var chosen = new[]
            {
                RackViewAddress.Whole(DimensionViewKind.Planta),
                RackViewAddress.Fondo(0),
                RackViewAddress.Fondo(1),          // the rack has one fondo
                RackViewAddress.Post(post),
                RackViewAddress.Post(999),         // no cut there
                RackViewAddress.Fondo(0),          // repeated
            };

            var drawable = SelectiveDoorInsertion.DrawableViews(chosen, system, catalog);

            Assert.Equal(new[] { RackViewAddress.Whole(DimensionViewKind.Planta), RackViewAddress.Fondo(0), RackViewAddress.Post(post) }, drawable.ToArray());
            Assert.Empty(SelectiveDoorInsertion.DrawableViews(null, system, catalog));
        }

        [Fact]
        public void Write_WithoutViews_IsTheUpdateOfE1_AndPasses()
        {
            var write = new SelectiveDoorWrite(3, default, "rack-1", "Selectivo A", new SelectivePalletDesign(), null, null);
            Assert.False(write.Inserts);
            Assert.False(write.NewRack);
            Assert.Null(SelectiveDoorInsertion.RefusalOf(write));
        }

        [Fact]
        public void Write_WithViews_Inserts_AndANewRackNeedsViewsAndAnIdentity()
        {
            var views = new[] { RackViewAddress.Fondo(0) };
            Assert.True(Write(null, views).Inserts);
            Assert.Null(SelectiveDoorInsertion.RefusalOf(Write(null, views)));
            Assert.Null(SelectiveDoorInsertion.RefusalOf(Write(null, views, newRack: true)));
            Assert.NotNull(SelectiveDoorInsertion.RefusalOf(Write(null, Array.Empty<RackViewAddress>(), newRack: true)));
            Assert.NotNull(SelectiveDoorInsertion.RefusalOf(Write(null, views, newRack: true, rackId: " ")));
            Assert.NotNull(SelectiveDoorInsertion.RefusalOf(null));
        }

        [Theory]
        [InlineData(2, 0, 1)]
        [InlineData(2, 1, 0)]
        [InlineData(0, 0, 2)]
        [InlineData(0, 0, 0)]
        public void AnswerForInsert_IsTheAnswerOfDinamico(int redrawn, int erased, int inserted)
        {
            Assert.Equal(RackDoorRules.AnswerForInsert(5, "r", redrawn, erased, inserted), SelectiveDoorInsertion.AnswerForInsert(5, "r", redrawn, erased, inserted));
        }

        [Theory]
        [InlineData(3, "Selectivo 4")]
        [InlineData(1, null)]
        [InlineData(0, "Selectivo 4")]
        public void AnswerForNewRack_IsTheAnswerOfDinamico_WithTheAutomaticName(int placed, string name)
        {
            var answer = SelectiveDoorInsertion.AnswerForNewRack(5, "r", placed, name);
            Assert.Equal(RackDoorRules.AnswerForNewRack(5, "r", placed, name), answer);
            Assert.Equal(placed > 0, answer.Written);
        }

        // ------------------------------------------------------------------ RACKSELECTIVONUEVO (Selectivo E2, A2)

        private static readonly PropertyId Clearance = ProjectPropertyIds.SelectiveVerticalClearance;

        private static SelectiveDoorWrite NewWrite(SelectiveRackSystem system, IReadOnlyList<RackViewAddress> views,
            IReadOnlyDictionary<PropertyId, LinkedPropertyEditState> finals = null, string rackId = "nuevo-1", string name = "")
            => new SelectiveDoorWrite(9, default, rackId, name, new SelectivePalletDesign(), system, finals, views, NewRack: true);

        [Fact]
        public void PlanNewRack_InsertsTheChosenViewsTheDraftCanDraw_InTheirOrder_WithoutRepeats()
        {
            var catalog = Catalog;
            var system = Selective(DimensionDetail.Standard, twoFondos: true, catalog);
            var post = new SelectiveLateralBuilder().Cortes(system, catalog).Last().PostIndex;
            var chosen = new[]
            {
                RackViewAddress.Post(post),
                RackViewAddress.Fondo(1),
                RackViewAddress.Fondo(2),          // the draft has two fondos
                RackViewAddress.Whole(DimensionViewKind.Planta),
                RackViewAddress.Post(post),        // repeated
            };
            var literal = new Dictionary<PropertyId, LinkedPropertyEditState> { [Clearance] = LinkedPropertyEditState.Literal(6.0) };

            var plan = SelectiveDoorInsertion.PlanNewRack(NewWrite(system, chosen, literal), identityInDrawing: false, catalog);

            Assert.True(plan.CanInsert);
            Assert.Null(plan.Reason);
            Assert.Equal(new[] { RackViewAddress.Post(post), RackViewAddress.Fondo(1), RackViewAddress.Whole(DimensionViewKind.Planta) }, plan.Views.ToArray());
            Assert.Equal(SelectiveDoorInsertion.DrawableViews(chosen, system, catalog), plan.Views);
        }

        [Fact]
        public void PlanNewRack_RefusesWithoutADraft_ANonNewWrite_WithoutViewsOrIdentity_AndWithNothingDrawable()
        {
            var catalog = Catalog;
            var system = Selective(DimensionDetail.Standard, twoFondos: false, catalog);
            var views = new[] { RackViewAddress.Fondo(0) };

            static void Refused(SelectiveNewRackPlan plan, string reason)
            {
                Assert.False(plan.CanInsert);
                Assert.Empty(plan.Views);
                Assert.Equal(reason, plan.Reason);
            }

            Refused(SelectiveDoorInsertion.PlanNewRack(null, false, catalog), "No hay un borrador válido que insertar.");
            Refused(SelectiveDoorInsertion.PlanNewRack(NewWrite(null, views), false, catalog), "No hay un borrador válido que insertar.");
            Refused(SelectiveDoorInsertion.PlanNewRack(NewWrite(system, views) with { Design = null }, false, catalog), "No hay un borrador válido que insertar.");
            Refused(SelectiveDoorInsertion.PlanNewRack(NewWrite(system, views) with { NewRack = false }, false, catalog),
                "La petición no es la de un rack nuevo: no se insertó nada.");
            Refused(SelectiveDoorInsertion.PlanNewRack(NewWrite(system, Array.Empty<RackViewAddress>()), false, catalog),
                SelectiveDoorInsertion.RefusalOf(NewWrite(system, Array.Empty<RackViewAddress>())));
            Refused(SelectiveDoorInsertion.PlanNewRack(NewWrite(system, views, rackId: " "), false, catalog),
                "El rack nuevo no tiene identidad: no se insertó nada.");
            Refused(SelectiveDoorInsertion.PlanNewRack(NewWrite(system, new[] { RackViewAddress.Fondo(3), RackViewAddress.Post(999) }), false, catalog),
                "Ninguna de las vistas elegidas se puede dibujar con el borrador: no se insertó nada.");
        }

        [Fact]
        public void PlanNewRack_NeverInsertsOverARackOfTheDrawingWithTheSameIdentity_WithTheTextOfDinamico()
        {
            var catalog = Catalog;
            var system = Selective(DimensionDetail.Standard, twoFondos: false, catalog);

            var plan = SelectiveDoorInsertion.PlanNewRack(NewWrite(system, new[] { RackViewAddress.Fondo(0) }), identityInDrawing: true, catalog);

            Assert.False(plan.CanInsert);
            Assert.Equal(SelectiveDoorInsertion.IdentityInDrawingText, plan.Reason);
            // RACKDINAMICONUEVO refuses the same case with the same words.
            Assert.Contains("\"" + SelectiveDoorInsertion.IdentityInDrawingText + "\"", WorkspaceHostSource.Code(Commands), StringComparison.Ordinal);
        }

        [Fact]
        public void PlanNewRack_RefusesAHolguraBoundToAVariable_BecauseTheDispatchOfNuevoWritesNoBinding()
        {
            var catalog = Catalog;
            var system = Selective(DimensionDetail.Standard, twoFondos: false, catalog);
            var views = new[] { RackViewAddress.Fondo(0) };
            var bound = new Dictionary<PropertyId, LinkedPropertyEditState>
            {
                [Clearance] = LinkedPropertyEditState.Reference(6.0, VariableId.Parse("8a1d4e77-2c93-4b60-8f15-6e0b93a7c221")),
            };

            var refused = SelectiveDoorInsertion.PlanNewRack(NewWrite(system, views, bound), false, catalog);
            Assert.False(refused.CanInsert);
            Assert.Equal(SelectiveDoorInsertion.LinkedNewRackText, refused.Reason);

            // Literal states, or none, are what a new draft of the window carries: they pass.
            Assert.True(SelectiveDoorInsertion.PlanNewRack(NewWrite(system, views), false, catalog).CanInsert);
            Assert.True(SelectiveDoorInsertion.PlanNewRack(NewWrite(system, views,
                new Dictionary<PropertyId, LinkedPropertyEditState> { [Clearance] = LinkedPropertyEditState.Literal(6.0), [ProjectPropertyIds.SelectivePalletTolerance] = null }),
                false, catalog).CanInsert);
        }

        // ------------------------------------------------------------------ the wiring, on the source (the Plugin needs AutoCAD)

        private const string Commands = "src/RackCad.Plugin/VentanaRackCad/RackCadRackDoorCommands.cs";
        private const string Port = "src/RackCad.Plugin/VentanaRackCad/RackCadRackDoorPort.cs";

        [Fact]
        public void TheNewSelectivoIsACommandThatPlansWithTheRuleThenUsesTheDispatchOfNuevoAndAnswersWithTheRule()
        {
            var code = WorkspaceHostSource.Code(Commands);
            var command = WorkspaceHostSource.BodyOf(code, @"public void InsertNewSelective\(\)");

            Assert.Contains("[CommandMethod(\"RACKSELECTIVONUEVO\")]", code, StringComparison.Ordinal);
            InOrder(command,
                "RackCadRackDoorPort.TakePendingSelectiveNew()",
                "door?.Started(pending.RequestId)",
                "pending.Document != RackCadRackDoorPort.KeyOf(document)",
                "SelectiveDoorInsertion.PlanNewRack(",
                "RackCommandSupport.FindRackBlocks(document, pending.RackId).Count > 0",
                "if (!plan.CanInsert)",
                "RackMenuCommands.DispatchInsertion(document,",
                "SelectiveDraftOutcome.NewRackRequest(write.RackId, write.Name, write.Design, write.System, plan.Views)",
                "RackCommandSupport.FindRackBlocks(document, pending.RackId)",
                "SelectiveDoorInsertion.AnswerForNewRack(");
            // The dispatch of «Nuevo» warns about the units itself; the command does not warn twice.
            Assert.DoesNotContain("RackUnitsGuard.WarnIfNotInches", command, StringComparison.Ordinal);
            Assert.DoesNotMatch(new System.Text.RegularExpressions.Regex(@"new\s+RackDesignWritten\s*\([^;]*,\s*true\s*,"), command);
        }

        [Fact]
        public void InsertarVistaOfASelectivoPlansAgainThenUsesTheExtractedInsertarHalfOfRackEditar()
        {
            var code = WorkspaceHostSource.Code(Commands);
            var command = WorkspaceHostSource.BodyOf(code, @"public void InsertSelectiveViews\(\)");

            Assert.Contains("[CommandMethod(\"RACKSELECTIVOINSERTAR\")]", code, StringComparison.Ordinal);
            InOrder(command,
                "RackCadRackDoorPort.TakePendingSelectiveInsert()",
                "door?.Started(pending.RequestId)",
                "pending.Document != RackCadRackDoorPort.KeyOf(document)",
                "RackCommandSupport.FindRackBlocks(document, pending.RackId)",
                "SelectiveDoorInsertion.RefusalOf(write)",
                "RackSelectivoCommands.ReadProjectVariables(document, out _)",
                "RackDoorRules.PlanSelectiveApply(",
                "SelectiveDoorInsertion.DrawableViews(",
                "RackUnitsGuard.WarnIfNotInches(document)",
                "RackSelectivoCommands.InsertSelectiveViews(",
                "SelectiveDoorInsertion.AnswerForInsert(");
            Assert.DoesNotMatch(new System.Text.RegularExpressions.Regex(@"new\s+RackDesignWritten\s*\([^;]*,\s*true\s*,"), command);
        }

        [Fact]
        public void RackEditarInsertsThroughTheExtractedHalfAsBefore()
        {
            var code = WorkspaceHostSource.Code("src/RackCad.Plugin/RackSelectivoCommands.cs");
            var edit = WorkspaceHostSource.BodyOf(code, @"internal static void EditSelective\(");
            var half = WorkspaceHostSource.BodyOf(code, @"internal static int InsertSelectiveViews\(");

            // RACKEDITAR: the units warning before any block, then «Insertar» through the extracted half with the views of its window, and
            // nothing after it (the «Actualizar» half is not reached).
            InOrder(edit,
                "if (!window.UpdateOnly)",
                "RackUnitsGuard.WarnIfNotInches(document);",
                "if (!window.UpdateOnly)",
                "InsertSelectiveViews(document, blockId, embed, saved, registry, window, window.InsertionRequest.Views);",
                "return;",
                "ApplySelectiveEdit(document, blockId, embed, saved, registry, window);");
            // The half: the steps of the former branch, in its order.
            InOrder(half,
                "var insertId = string.IsNullOrWhiteSpace(embed.Id) ? window.RackId : embed.Id;",
                "LinkedPropertyReconciler.Reconcile(",
                "SerializeSelectiveAuthored(insertReconciled.Authored)",
                "RackViewCodec.Encode(RackSystemKind.SelectiveRack, requested[0])",
                "new SelectiveInsertPort(",
                "port.ScanAndClassifyOnce()",
                "port.CheckCustomProperties(membership)",
                "port.TryPrepare(address, membership, out var product, out var diagnostic)",
                "port.CreateRedrawPort(prepared[requested[0]])",
                "RackSiblingRedrawRun.Execute(membership, redrawPort)",
                "RackSiblingRedrawDisposition.DeferToFirstPlacement",
                "RackProductSourceKind.ExistingRack, RackSystemKind.SelectiveRack, insertId, requested",
                "driver.Execute(request)",
                "return batch.Report.PlacedCount;");
            // Each «return;» of the former branch before any view was placed is a refusal now (the reconciliation, the gate of the custom
            // properties, a preparation, the redraw and its mutation), and RACKEDITAR ignores it.
            Assert.Equal(5, WorkspaceHostSource.Count(half, @"return -1;"));
        }

        [Fact]
        public void TheDoorSendsANewSelectivoToItsOwnCommandFirst_NeverWithoutViews_AndAbandonsItsSlot()
        {
            var port = WorkspaceHostSource.Code(Port);
            var request = WorkspaceHostSource.BodyOf(port, @"public RackCadClassicResult RequestSelectiveUpdate\(SelectiveDoorWrite write\)");

            Assert.Contains("SelectiveInsertCommand = \"RACKSELECTIVOINSERTAR\"", port, StringComparison.Ordinal);
            Assert.Contains("SelectiveNewCommand = \"RACKSELECTIVONUEVO\"", port, StringComparison.Ordinal);
            InOrder(request,
                "if (write.NewRack)",
                "SelectiveDoorInsertion.RefusalOf(write)",
                "SelectiveNewCommand + \" \"",
                "_pendingSelectiveNew = PendingCopy(write, document)",
                "var insert = write.Inserts;");
            // The refusal of part A («todavía no se puede insertar») is gone: the new Selectivo has its command.
            Assert.DoesNotContain("todavía no se puede insertar", port, StringComparison.Ordinal);

            var abandon = WorkspaceHostSource.BodyOf(port, @"public void Abandon\(long requestId\)");
            Assert.Contains("_pendingSelectiveInsert = null", abandon, StringComparison.Ordinal);
            Assert.Contains("_pendingSelectiveNew = null", abandon, StringComparison.Ordinal);
            Assert.Contains("internal static PendingSelectiveWrite TakePendingSelectiveNew()", port, StringComparison.Ordinal);
        }

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
    }
}
