#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using RackCad.Application.Workspace;
using RackCad.UI.Workspace;
using Xunit;

namespace RackCad.UI.Tests
{
    /// <summary>
    /// I-64 F4-T4A2-PREPARE-UI (INV-F4T4A2-01 to 09): the visible, read-only reconciliation of the field. Every test drives the real view-model
    /// of the field over the double of the edit port, so what is proved is what the view-model asks of the port and what it shows; nothing
    /// reads a drawing and nothing is written. The texts are the ones of the table T-A2 of the package, written here in full and byte for
    /// byte: a text that changes in the product fails here.
    /// </summary>
    public class FullPanelCopyWorkspacePilotReconciliationGreenTests
    {
        private static readonly SessionId A = new(7);
        private static readonly SessionId B = new(8);
        private const string Rack = "R-1";

        private const string A201 = "Sin cambios: el dibujo coincide con la base del borrador.";
        private const string A202 = "La base vuelve a coincidir con el dibujo, pero el borrador sigue en conflicto: elige cómo reconciliarlo.";
        private const string A207 = "El cambio no llegó a aplicarse: el borrador recupera el estado que tenía antes.";
        private const string A214 = "Con texto pendiente no se puede conservar el borrador: aplícalo o corrígelo antes.";
        private const string A220 = "¿Descartar los cambios sin aplicar del borrador y recargarlo desde el dibujo?";
        private const string A220Pending = " También se pierde el texto pendiente.";
        private const string A221 = "¿Recargar el borrador desde el dibujo? No tiene cambios propios que perder.";
        private const string A223 = "¿Recargar el borrador desde el dibujo? El cambio ya se aplicó y el borrador vuelve a leerse del dibujo.";
        private const string A230 = "Borrador recargado desde el dibujo.";
        private const string A231 = "Borrador conservado sobre la versión actual del dibujo.";
        private const string A232 = "No se aplicó la elección: el borrador se conserva tal como estaba.";
        private const string A233 = " No se hizo nada.";
        private const string A234 = "La situación del borrador cambió; no se hizo nada.";
        private const string A235 = "Se comprobó, pero el borrador cambió durante la comprobación; vuelve a comprobarlo.";
        private const string A236 = "Elección cancelada: el borrador no cambió.";
        private const string A237 = "Se comprobó, pero el borrador ya no existe.";
        private const string SurfaceChanged = "La superficie cambió durante la lectura; el resultado no se mostró.";

        private static string A222(string committed) =>
            "¿Conservar tu escala de anotación (" + committed + ") sobre la versión actual del dibujo? El resto del rack se toma del dibujo y tu escala sustituye a la del dibujo.";

        // ------------------------------------------------------------------ the rig and the views

        private sealed class Rig
        {
            public readonly WorkspacePilotEditPortDouble Port = new();
            public readonly WorkspacePilotFieldViewModel Field;
            public int Changed;

            public Rig(PilotFieldView view, bool preview = false)
            {
                Field = new WorkspacePilotFieldViewModel(Port, () => Changed++);
                Port.Set(A, Rack, view);
                Field.Show(A, Rack, preview);
                Port.Calls.Clear();
            }

            public void Set(PilotFieldView view) => Port.Set(A, Rack, view);

            public int Count(string operation) => Port.Count(operation);

            // Every operation that was called, apart from the reads of the memory of the port.
            public string[] Operations() =>
                Port.Operations().Where(o => o != WorkspacePilotEditPortDouble.GetFieldOp).OrderBy(o => o, StringComparer.Ordinal).ToArray();
        }

        private static PilotFieldView Dirty(bool stale = false, string? committed = "2.5", string? pending = null) =>
            WorkspacePilotEditPortDouble.View(PilotFieldState.Dirty, committed, pending, null, stale, true);

        private static PilotFieldView Conflict(bool isProtected, string? pending = null, string? committed = "2.5", string? conflictText = "A1:EnvelopeText", string? lastCause = null) =>
            WorkspacePilotEditPortDouble.View(
                PilotFieldState.Conflict, committed, pending, pending == null ? null : "El texto no es válido.", false, isProtected, true,
                null, null, PilotUnknownOrigin.None, conflictText, lastCause);

        private static PilotFieldView Unknown(PilotUnknownOrigin origin, string? lastCause = null) =>
            WorkspacePilotEditPortDouble.View(PilotFieldState.Unknown, "2.5", null, null, false, true, true, null, null, origin, null, lastCause);

        // An answer with a status that no enumerator names, built through the private constructor of the value.
        private static T Forge<T>(params object?[] arguments)
            where T : struct =>
            (T)Activator.CreateInstance(typeof(T), BindingFlags.Instance | BindingFlags.NonPublic, null, arguments, null)!;

        private static EditIntentOutcome UnknownIntent() =>
            Forge<EditIntentOutcome>((EditIntentStatus)99, EditWitnessStatus.NotEvaluated, "forged", null, null, null);

        // I-64 F4-T4A2 (correction, M-1): the memory of the port that cannot be read again after the operation leaves the field in its Error.
        // The answer is not presented against the view of before: no text of the tables of section 3 is shown and the state starts with Error.
        private static void AssertOnlyTheErrorIsShown(WorkspacePilotFieldViewModel field)
        {
            foreach (var text in new[] { A201, A202, A235, A237, A207, A230, A231 })
                Assert.NotEqual(text, field.NoticeText);

            Assert.DoesNotContain("Sin cambios", field.NoticeText, StringComparison.Ordinal);
            Assert.StartsWith("Error: ", field.StateText, StringComparison.Ordinal);
            Assert.True(field.IsReadOnly);
            Assert.False(field.HasChoice);
        }

        public static IEnumerable<object[]> ChoiceStatusesThatDidNothing() => new[]
        {
            new object[] { EditIntentOutcome.Unavailable("no hay lector."), "No disponible: no hay lector." + A233 },
            new object[] { EditIntentOutcome.Waiting("hay un comando."), "En espera: hay un comando." + A233 },
            new object[] { EditIntentOutcome.Rejected("otro documento."), "Rechazado: otro documento." + A233 },
            new object[] { EditIntentOutcome.NotFound("sin borrador."), "Sin borrador: sin borrador." + A233 },
            new object[] { EditIntentOutcome.Failed("falló."), "Fallo: falló." + A233 },
            new object[] { EditIntentOutcome.NotNeeded("no hace falta."), "No aplicable: no hace falta." + A233 },
            new object[] { EditIntentOutcome.AlreadyOpen(), "No se aplicó la elección: sin detalle." + A233 },
            new object[] { EditIntentOutcome.Admitted(EditWitnessStatus.NotSupplied), "No se aplicó la elección: sin detalle." + A233 },
            new object[] { default(EditIntentOutcome), "No disponible: sin detalle." + A233 },
            new object[] { UnknownIntent(), "No se aplicó la elección: forged" + A233 },
        };

        [Fact]
        public void AChoiceWhoseMemoryCannotBeReadAgainAfterTheOperationIsNeverPresentedAsDone()
        {
            var discard = new Rig(Conflict(true));
            discard.Port.DiscardAndReloadResponder = _ => EditIntentOutcome.Reloaded(EditWitnessStatus.NotSupplied);
            discard.Port.ReadHook = _ => discard.Port.Throws[WorkspacePilotEditPortDouble.GetFieldOp] = new InvalidOperationException("boom");
            discard.Field.RequestDiscardAndReload();
            Assert.True(discard.Field.HasChoice);
            discard.Field.ConfirmChoice();
            Assert.Equal(1, discard.Count(WorkspacePilotEditPortDouble.DiscardAndReloadOp));
            AssertOnlyTheErrorIsShown(discard.Field);

            var keep = new Rig(Conflict(true));
            keep.Port.KeepMineResponder = _ => EditIntentOutcome.Kept(EditWitnessStatus.NotSupplied);
            keep.Port.ReadHook = _ => keep.Port.Throws[WorkspacePilotEditPortDouble.GetFieldOp] = new InvalidOperationException("boom");
            keep.Field.RequestKeepMine();
            Assert.True(keep.Field.HasChoice);
            keep.Field.ConfirmChoice();
            Assert.Equal(1, keep.Count(WorkspacePilotEditPortDouble.KeepMineOp));
            AssertOnlyTheErrorIsShown(keep.Field);
        }

        [Fact]
        public void ASurfaceThatChangesDuringTheReadOfAnUnknownDraftAndDuringAChoiceShowsTheSameNotice()
        {
            var unknown = new Rig(Unknown(PilotUnknownOrigin.CommitThrew));
            unknown.Port.CheckNowResponder = _ => EditCheckOutcome.Recovered();
            unknown.Port.ReadHook = _ => unknown.Field.Show(B, "R-2", false);
            unknown.Field.CheckNow();
            Assert.Equal(SurfaceChanged, unknown.Field.NoticeText);

            var choice = new Rig(Conflict(true));
            choice.Field.RequestKeepMine();
            choice.Port.KeepMineResponder = _ => EditIntentOutcome.Kept(EditWitnessStatus.NotSupplied);
            choice.Port.ReadHook = _ => choice.Field.Show(B, "R-2", false);
            choice.Field.ConfirmChoice();
            Assert.Equal(SurfaceChanged, choice.Field.NoticeText);
            Assert.False(choice.Field.HasChoice);
        }

        [Fact]
        public void KeepIsNotOfferedWithAPendingTextWhetherItIsValidOrNotAndTheLineSaysWhy()
        {
            foreach (var pending in new[] { "2x", "3.5" })
            {
                var rig = new Rig(Conflict(true, pending: pending, conflictText: "A1:EnvelopeText"));

                Assert.False(rig.Field.ShowKeepMineButton);
                Assert.True(rig.Field.ShowDiscardAndReloadButton);
                Assert.Equal("Qué cambió: A1:EnvelopeText " + A214, rig.Field.ReconcileText);

                rig.Field.RequestKeepMine();

                Assert.False(rig.Field.HasChoice);
                Assert.DoesNotContain(rig.Port.Calls, c => c.Operation != WorkspacePilotEditPortDouble.GetFieldOp);
            }
        }

        [Fact]
        public void TheOptionsAreNotOfferedOnAPreviewTabWithAFailureWithAPersistentDiagnosticOrWithAChoiceThatWaits()
        {
            var preview = new Rig(Conflict(true), preview: true);
            Assert.False(preview.Field.ShowDiscardAndReloadButton);
            Assert.False(preview.Field.ShowKeepMineButton);
            preview.Field.Show(A, Rack, false);
            Assert.True(preview.Field.ShowDiscardAndReloadButton);
            Assert.True(preview.Field.ShowKeepMineButton);

            var failed = new Rig(Conflict(true));
            failed.Field.Fail(new InvalidOperationException("boom"));
            Assert.False(failed.Field.ShowDiscardAndReloadButton);
            Assert.False(failed.Field.ShowKeepMineButton);

            var asked = new Rig(Conflict(true));
            asked.Field.RequestKeepMine();
            Assert.False(asked.Field.ShowDiscardAndReloadButton);
            Assert.False(asked.Field.ShowKeepMineButton);
        }

        [Fact]
        public void TheTextOfEachConfirmationIsTheOneOfTheTableForTheStateTheOwnWorkAndThePendingText()
        {
            var own = new Rig(Conflict(true, committed: "2.5"));
            own.Field.RequestDiscardAndReload();
            Assert.Equal(A220, own.Field.ChoiceText);

            var external = new Rig(Conflict(false));
            external.Field.RequestDiscardAndReload();
            Assert.Equal(A221, external.Field.ChoiceText);

            var pending = new Rig(Conflict(false, pending: "2x"));
            pending.Field.RequestDiscardAndReload();
            Assert.Equal(A220 + A220Pending, pending.Field.ChoiceText);

            var keep = new Rig(Conflict(true, committed: "2.5"));
            keep.Field.RequestKeepMine();
            Assert.Equal(A222("2.5"), keep.Field.ChoiceText);

            var keepWithoutValue = new Rig(Conflict(true, committed: null));
            keepWithoutValue.Field.RequestKeepMine();
            Assert.Equal(A222("sin valor"), keepWithoutValue.Field.ChoiceText);

            var unknown = new Rig(Unknown(PilotUnknownOrigin.PostReread));
            unknown.Field.RequestDiscardAndReload();
            Assert.Equal(A223, unknown.Field.ChoiceText);
        }

        [Fact]
        public void AskingForAnOptionCallsNoOperationOfThePortAndOnlyReadsItsMemory()
        {
            foreach (var keep in new[] { false, true })
            {
                var rig = new Rig(Conflict(true));

                if (keep) rig.Field.RequestKeepMine();
                else rig.Field.RequestDiscardAndReload();

                Assert.True(rig.Field.HasChoice);
                Assert.Empty(rig.Operations());
                Assert.All(rig.Port.Calls, c => Assert.Equal(WorkspacePilotEditPortDouble.GetFieldOp, c.Operation));
                Assert.Empty(rig.Port.Reads());
            }
        }

        [Fact]
        public void AnOptionThatIsNotOfferedDoesNothingWhenItIsAsked()
        {
            var rig = new Rig(Dirty());

            rig.Field.RequestDiscardAndReload();
            rig.Field.RequestKeepMine();

            Assert.False(rig.Field.HasChoice);
            Assert.Empty(rig.Port.Calls);
            Assert.Equal(string.Empty, rig.Field.NoticeText);
        }

        [Fact]
        public void YesOnKeepCallsThatOperationOnceWithTheSessionAndTheRackAndNeverTheDiscard()
        {
            var rig = new Rig(Conflict(true));
            rig.Port.KeepMineResponder = _ =>
            {
                rig.Set(Dirty());
                return EditIntentOutcome.Kept(EditWitnessStatus.NotSupplied);
            };
            rig.Field.RequestKeepMine();

            rig.Field.ConfirmChoice();

            var call = Assert.Single(rig.Port.Of(WorkspacePilotEditPortDouble.KeepMineOp));
            Assert.Equal(A, call.Origin);
            Assert.Equal(Rack, call.RackId);
            Assert.Equal(0, rig.Count(WorkspacePilotEditPortDouble.DiscardAndReloadOp));
            Assert.Equal(new[] { WorkspacePilotEditPortDouble.KeepMineOp }, rig.Operations());
            Assert.Equal(A231, rig.Field.NoticeText);
            Assert.False(rig.Field.HasChoice);
            Assert.Equal(1, rig.Changed);
        }

        [Fact]
        public void NoCallsNothingChangesNothingAndSaysSoForEachOfTheTwoChoices()
        {
            foreach (var keep in new[] { false, true })
            {
                var rig = new Rig(Conflict(true, committed: "2.5"));
                if (keep) rig.Field.RequestKeepMine();
                else rig.Field.RequestDiscardAndReload();
                Assert.True(rig.Field.HasChoice);
                var before = rig.Port.Current(A, Rack);
                rig.Port.Calls.Clear();

                rig.Field.CancelChoice();

                Assert.Empty(rig.Port.Calls);
                Assert.Equal(before, rig.Port.Current(A, Rack));
                Assert.False(rig.Field.HasChoice);
                Assert.Equal(string.Empty, rig.Field.ChoiceText);
                Assert.Equal(A236, rig.Field.NoticeText);
                Assert.Equal("Conflicto", rig.Field.StateText);
                Assert.True(rig.Field.ShowDiscardAndReloadButton);
            }
        }

        [Fact]
        public void AChoiceThatWaitsMakesTheBoxReadOnlyAndOffersOnlyYesAndNo()
        {
            var rig = new Rig(Conflict(true));
            Assert.True(rig.Field.IsEditable);

            rig.Field.RequestKeepMine();

            Assert.True(rig.Field.IsReadOnly);
            Assert.False(rig.Field.IsEditable);
            Assert.False(rig.Field.ShowEditButton);
            Assert.False(rig.Field.CanOpenByGesture);
            Assert.False(rig.Field.ShowCheckButton);
            Assert.False(rig.Field.ShowDiscardAndReloadButton);
            Assert.False(rig.Field.ShowKeepMineButton);
            Assert.True(rig.Field.HasChoice);

            rig.Field.Text = "9";
            rig.Field.CommitPending();

            Assert.Equal(0, rig.Count(WorkspacePilotEditPortDouble.SetPendingOp));
            Assert.Equal(0, rig.Count(WorkspacePilotEditPortDouble.CommitOp));
        }

        [Fact]
        public void IfTheViewChangedAfterAskingForKeepYesCallsNeitherOperation()
        {
            var rig = new Rig(Conflict(true, committed: "2.5"));
            rig.Field.RequestKeepMine();
            Assert.True(rig.Field.HasChoice);
            rig.Set(Conflict(true, committed: "3.0"));

            rig.Field.ConfirmChoice();

            Assert.Equal(A234, rig.Field.NoticeText);
            Assert.Equal(0, rig.Count(WorkspacePilotEditPortDouble.KeepMineOp));
            Assert.Equal(0, rig.Count(WorkspacePilotEditPortDouble.DiscardAndReloadOp));
        }

        [Fact]
        public void TheChoiceIsWithdrawnInSilenceByAChangeOfSurfaceByClearAndByAFailure()
        {
            var surface = new Rig(Conflict(true));
            surface.Field.RequestKeepMine();
            Assert.True(surface.Field.HasChoice);
            surface.Field.Show(B, "R-2", false);
            Assert.False(surface.Field.HasChoice);
            Assert.Equal(string.Empty, surface.Field.NoticeText);
            Assert.Equal(string.Empty, surface.Field.ChoiceText);

            var cleared = new Rig(Conflict(true));
            cleared.Field.RequestDiscardAndReload();
            Assert.True(cleared.Field.HasChoice);
            cleared.Field.Clear();
            Assert.False(cleared.Field.HasChoice);
            Assert.Equal(string.Empty, cleared.Field.NoticeText);

            var failed = new Rig(Conflict(true));
            failed.Field.RequestDiscardAndReload();
            Assert.True(failed.Field.HasChoice);
            failed.Field.Fail(new InvalidOperationException("boom"));
            Assert.False(failed.Field.HasChoice);
            Assert.Equal(string.Empty, failed.Field.NoticeText);
            Assert.Equal("Error: boom", failed.Field.StateText);
        }

        [Fact]
        public void TheChoiceIsWithdrawnByAnyNewGestureOfTheField()
        {
            var rig = new Rig(Conflict(true));
            rig.Field.RequestKeepMine();
            Assert.True(rig.Field.HasChoice);

            rig.Field.BeginEdit();

            Assert.False(rig.Field.HasChoice);
            Assert.Equal(string.Empty, rig.Field.ChoiceText);

            rig.Field.RequestKeepMine();
            Assert.True(rig.Field.HasChoice);
            rig.Field.Rematerialize();
            Assert.False(rig.Field.HasChoice);
        }

        [Fact]
        public void TheChoiceIsWithdrawnWithTheSituationNoticeWhenThePresentationFindsThatTheViewNoLongerOffersIt()
        {
            var gone = new Rig(Conflict(true));
            gone.Field.RequestKeepMine();
            Assert.True(gone.Field.HasChoice);
            gone.Port.Remove(A, Rack);
            gone.Field.Show(A, Rack, false);
            Assert.False(gone.Field.HasChoice);
            Assert.Equal(A234, gone.Field.NoticeText);

            var changed = new Rig(Conflict(true));
            changed.Field.RequestDiscardAndReload();
            Assert.True(changed.Field.HasChoice);
            changed.Set(Dirty());
            changed.Field.Show(A, Rack, false);
            Assert.False(changed.Field.HasChoice);
            Assert.Equal(A234, changed.Field.NoticeText);

            var kept = new Rig(Conflict(true));
            kept.Field.RequestKeepMine();
            kept.Field.Show(A, Rack, false);
            Assert.True(kept.Field.HasChoice);
            Assert.Equal(string.Empty, kept.Field.NoticeText);
        }

        [Fact]
        public void ADiscardFromTheBrowserOfTheDraftThatTheChoiceWasAboutWithdrawsTheChoiceToo()
        {
            var host = new WorkspaceBrowserSummaryPortDouble();
            var port = new WorkspacePilotEditPortDouble();
            port.DiscardAsTheHostDoes();
            port.Set(A, Rack, Conflict(true));
            var browser = new WorkspaceBrowserViewModel(host, null, port);
            browser.Present(A, SelectionContext.One(Rack), true, WorkspaceActionBlock.None, string.Empty);
            browser.PinTab(Rack);
            browser.Field.RequestKeepMine();
            Assert.True(browser.Field.HasChoice);

            browser.DiscardField();
            browser.ConfirmDiscard();

            Assert.False(browser.Field.HasChoice);
            Assert.Equal(0, port.Count(WorkspacePilotEditPortDouble.KeepMineOp));

            // A declared deviation of the package (M-3): the browser rebuilds the field and then rematerializes it, and that last gesture
            // withdraws the choice in silence, so the notice of the situation that changed (A2-34) is never seen in this flow.
            Assert.Equal(string.Empty, browser.Field.NoticeText);
        }

        // ------------------------------------------------------------------ the answer of the choice (D-T4A2-03, step 5)

        [Fact]
        public void AnAnswerThatDoesNotMatchTheChoiceIsNeverPresentedAsDone()
        {
            var discard = new Rig(Conflict(true));
            discard.Port.DiscardAndReloadResponder = _ => EditIntentOutcome.Kept(EditWitnessStatus.NotSupplied);
            discard.Field.RequestDiscardAndReload();
            discard.Field.ConfirmChoice();
            Assert.Equal("No se aplicó la elección: sin detalle." + A233, discard.Field.NoticeText);

            var keep = new Rig(Conflict(true));
            keep.Port.KeepMineResponder = _ => EditIntentOutcome.Reloaded(EditWitnessStatus.NotSupplied);
            keep.Field.RequestKeepMine();
            keep.Field.ConfirmChoice();
            Assert.Equal("No se aplicó la elección: sin detalle." + A233, keep.Field.NoticeText);
        }

        [Theory]
        [MemberData(nameof(ChoiceStatusesThatDidNothing))]
        public void AnAnswerOfTheChoiceThatDidNothingHasItsPrefixItsReasonAndTheTextOfNothingDone(EditIntentOutcome outcome, string expected)
        {
            foreach (var keep in new[] { false, true })
            {
                var rig = new Rig(Conflict(true));
                rig.Port.KeepMineResponder = _ => outcome;
                rig.Port.DiscardAndReloadResponder = _ => outcome;
                if (keep) rig.Field.RequestKeepMine();
                else rig.Field.RequestDiscardAndReload();
                var before = rig.Port.Current(A, Rack);

                rig.Field.ConfirmChoice();

                Assert.Equal(expected, rig.Field.NoticeText);
                Assert.Equal(before, rig.Port.Current(A, Rack));
                Assert.Equal("Conflicto", rig.Field.StateText);
            }
        }

        [Fact]
        public void AnExceptionOfTheChosenOperationIsTheErrorOfTheFieldAndTheChoiceIsGone()
        {
            var rig = new Rig(Conflict(true));
            rig.Port.Throws[WorkspacePilotEditPortDouble.KeepMineOp] = new InvalidOperationException("boom");
            rig.Field.RequestKeepMine();

            rig.Field.ConfirmChoice();

            Assert.Equal("Error: boom", rig.Field.StateText);
            Assert.False(rig.Field.HasChoice);
            Assert.True(rig.Field.IsReadOnly);
            Assert.Equal(1, rig.Count(WorkspacePilotEditPortDouble.KeepMineOp));
        }

        [Fact]
        public void ARefusalOfTheChoiceForANotAdmittedReadIsAPersistentDiagnosticAndTheDraftIsKeptAsItWas()
        {
            foreach (var keep in new[] { false, true })
            {
                var rig = new Rig(Conflict(true, committed: "2.5"));
                var refusal = EditIntentOutcome.NotAdmitted("DIAG-C", "detalle", "HINT-C", EditWitnessStatus.NotSupplied);
                rig.Port.KeepMineResponder = _ => refusal;
                rig.Port.DiscardAndReloadResponder = _ => refusal;
                if (keep) rig.Field.RequestKeepMine();
                else rig.Field.RequestDiscardAndReload();
                var before = rig.Port.Current(A, Rack);

                rig.Field.ConfirmChoice();

                Assert.Equal(A232, rig.Field.NoticeText);
                Assert.Equal("DIAG-C", rig.Field.NotEditableText);
                Assert.Equal("HINT-C", rig.Field.ClassicEditorHintText);
                Assert.Equal(before, rig.Port.Current(A, Rack));
                Assert.True(rig.Field.IsReadOnly);
                Assert.False(rig.Field.ShowEditButton);
                Assert.False(rig.Field.ShowDiscardAndReloadButton);
                Assert.False(rig.Field.ShowKeepMineButton);
                Assert.True(rig.Field.ShowCheckButton);
            }
        }

        // ------------------------------------------------------------------ the check on a preview tab and without a port

        [Fact]
        public void APreviewTabOffersNeitherTheCheckNorTheOptionsAndCallsNothing()
        {
            var rig = new Rig(Conflict(true), preview: true);

            rig.Field.CheckNow();
            rig.Field.RequestDiscardAndReload();
            rig.Field.RequestKeepMine();

            Assert.Empty(rig.Port.Calls);
            Assert.False(rig.Field.HasChoice);
        }

        [Fact]
        public void AFieldWithoutThePortOffersNothingAndDoesNothing()
        {
            var field = new WorkspacePilotFieldViewModel(null);

            field.CheckNow();
            field.RequestDiscardAndReload();
            field.RequestKeepMine();
            field.ConfirmChoice();
            field.CancelChoice();

            Assert.False(field.ShowCheckButton);
            Assert.False(field.ShowDiscardAndReloadButton);
            Assert.False(field.ShowKeepMineButton);
            Assert.False(field.HasChoice);
            Assert.Equal(string.Empty, field.ChoiceText);
            Assert.Equal(string.Empty, field.ReconcileText);
        }
    }
}
