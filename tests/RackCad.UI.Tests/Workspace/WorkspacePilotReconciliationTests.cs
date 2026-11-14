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
    public class WorkspacePilotReconciliationTests
    {
        private static readonly SessionId A = new(7);
        private static readonly SessionId B = new(8);
        private const string Rack = "R-1";

        private const string A201 = "Sin cambios: el dibujo coincide con la base del borrador.";
        private const string A202 = "La base vuelve a coincidir con el dibujo, pero el borrador sigue en conflicto: elige cómo reconciliarlo.";
        private const string A203 = "La base coincide con el dibujo, pero el rack ya no es editable aquí.";
        private const string A204 = "El dibujo cambió fuera del panel y el borrador no tiene cambios propios: «Descartar y recargar» lo pone al día.";
        private const string A205 = "El dibujo cambió fuera del panel y el borrador tiene cambios propios: elige «Conservar mi borrador» o «Descartar y recargar».";
        private const string A205b = "El dibujo cambió fuera del panel y el borrador tiene texto pendiente: aplícalo o corrígelo para poder conservarlo, o elige «Descartar y recargar».";
        private const string A206 = "El dibujo cambió y el rack ya no es editable aquí: no se puede recargar ni conservar el borrador.";
        private const string A207 = "El cambio no llegó a aplicarse: el borrador recupera el estado que tenía antes.";
        private const string A208 = " No se comprobó.";
        private const string A209 = "El dibujo no coincide con la base anterior al intento: el borrador pasa a conflicto y no se supone aplicado.";
        private const string A211 = "Estado desconocido; comprueba.";
        private const string A212 = "Aplicado; recarga para seguir editando.";
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

        private static PilotFieldView Clean(bool stale = false, string? committed = "1:50") =>
            WorkspacePilotEditPortDouble.View(PilotFieldState.Clean, committed, stale: stale);

        private static PilotFieldView Dirty(bool stale = false, string? committed = "2.5", string? pending = null) =>
            WorkspacePilotEditPortDouble.View(PilotFieldState.Dirty, committed, pending, null, stale, true);

        private static PilotFieldView Conflict(bool isProtected, string? pending = null, string? committed = "2.5", string? conflictText = "A1:EnvelopeText", string? lastCause = null) =>
            WorkspacePilotEditPortDouble.View(
                PilotFieldState.Conflict, committed, pending, pending == null ? null : "El texto no es válido.", false, isProtected, true,
                null, null, PilotUnknownOrigin.None, conflictText, lastCause);

        private static PilotFieldView Unknown(PilotUnknownOrigin origin, string? lastCause = null) =>
            WorkspacePilotEditPortDouble.View(PilotFieldState.Unknown, "2.5", null, null, false, true, true, null, null, origin, null, lastCause);

        private static PilotFieldView OfState(PilotFieldState state) =>
            WorkspacePilotEditPortDouble.View(state, "2.5", isProtected: true);

        private static readonly Dictionary<string, Func<PilotFieldView>> Views = new(StringComparer.Ordinal)
        {
            ["Clean"] = () => Clean(),
            ["Dirty"] = () => Dirty(),
            ["Conflict"] = () => Conflict(true),
            ["UnknownCommitThrew"] = () => Unknown(PilotUnknownOrigin.CommitThrew),
            ["UnknownPostReread"] = () => Unknown(PilotUnknownOrigin.PostReread),
            ["UnknownNoOrigin"] = () => Unknown(PilotUnknownOrigin.None),
            ["Applying"] = () => OfState(PilotFieldState.Applying),
            ["Error"] = () => OfState(PilotFieldState.Error),
            ["Undecidable"] = () => OfState(PilotFieldState.Undecidable),
            ["NotOpen"] = () => OfState(PilotFieldState.NotOpen),
        };

        public static TheoryData<string> ViewNames()
        {
            var data = new TheoryData<string>();
            foreach (var name in Views.Keys) data.Add(name);
            return data;
        }

        // An answer with a status that no enumerator names, built through the private constructor of the value.
        private static T Forge<T>(params object?[] arguments)
            where T : struct =>
            (T)Activator.CreateInstance(typeof(T), BindingFlags.Instance | BindingFlags.NonPublic, null, arguments, null)!;

        private static EditConfirmOutcome UnknownConfirm() => Forge<EditConfirmOutcome>((EditConfirmStatus)99, "forged", Array.Empty<string>(), null, null);

        private static EditCheckOutcome UnknownCheck() => Forge<EditCheckOutcome>((EditCheckStatus)99, "forged", Array.Empty<string>());

        private static EditIntentOutcome UnknownIntent() =>
            Forge<EditIntentOutcome>((EditIntentStatus)99, EditWitnessStatus.NotEvaluated, "forged", null, null, null);

        // ------------------------------------------------------------------ the offer of the check (D-T4A2-01) and its dispatch (INV-F4T4A2-03)

        [Theory]
        [MemberData(nameof(ViewNames))]
        public void TheCheckIsOfferedOnAPinnedSurfaceOnlyForCleanDirtyConflictAndUnknownAfterACommitThatThrew(string name)
        {
            var rig = new Rig(Views[name]());
            var offered = name is "Clean" or "Dirty" or "Conflict" or "UnknownCommitThrew";

            Assert.Equal(offered, rig.Field.ShowCheckButton);
        }

        [Fact]
        public void TheCheckIsNotOfferedOnAPreviewTabWithAFailureOrWithAChoiceThatWaitsAndIsOfferedWithAPersistentDiagnostic()
        {
            var preview = new Rig(Dirty(), preview: true);
            Assert.False(preview.Field.ShowCheckButton);
            preview.Field.Show(A, Rack, false);
            Assert.True(preview.Field.ShowCheckButton);

            var failed = new Rig(Dirty());
            failed.Field.Fail(new InvalidOperationException("boom"));
            Assert.False(failed.Field.ShowCheckButton);

            var asked = new Rig(Conflict(true));
            asked.Field.RequestDiscardAndReload();
            Assert.True(asked.Field.HasChoice);
            Assert.False(asked.Field.ShowCheckButton);

            var diagnostic = new Rig(Dirty(stale: true));
            diagnostic.Port.ConfirmBaseResponder = _ => EditConfirmOutcome.Confirmed("DIAG", "HINT");
            diagnostic.Field.CheckNow();
            Assert.Equal("DIAG", diagnostic.Field.NotEditableText);
            Assert.True(diagnostic.Field.ShowCheckButton);
        }

        [Theory]
        [InlineData("Clean")]
        [InlineData("Dirty")]
        [InlineData("Conflict")]
        public void TheCheckOfACleanDirtyOrConflictingDraftCallsTheGeneralCheckOnceWithTheSessionAndTheRackAndNothingElse(string name)
        {
            var rig = new Rig(Views[name]());
            rig.Port.ConfirmBaseResponder = _ => EditConfirmOutcome.Confirmed();

            rig.Field.CheckNow();

            var call = Assert.Single(rig.Port.Of(WorkspacePilotEditPortDouble.ConfirmBaseOp));
            Assert.Equal(A, call.Origin);
            Assert.Equal(Rack, call.RackId);
            Assert.Equal(new[] { WorkspacePilotEditPortDouble.ConfirmBaseOp }, rig.Operations());
            Assert.Equal(1, rig.Changed);
        }

        [Fact]
        public void TheCheckOfAnUnknownDraftAfterACommitThatThrewCallsTheCheckOfThatStateOnceAndNothingElse()
        {
            var rig = new Rig(Unknown(PilotUnknownOrigin.CommitThrew));
            rig.Port.CheckNowResponder = _ => EditCheckOutcome.Recovered();

            rig.Field.CheckNow();

            var call = Assert.Single(rig.Port.Of(WorkspacePilotEditPortDouble.CheckOp));
            Assert.Equal(A, call.Origin);
            Assert.Equal(Rack, call.RackId);
            Assert.Equal(new[] { WorkspacePilotEditPortDouble.CheckOp }, rig.Operations());
        }

        [Theory]
        [MemberData(nameof(ViewNames))]
        public void ACheckThatIsNotOfferedCallsNothingAndChangesNothing(string name)
        {
            if (name is "Clean" or "Dirty" or "Conflict" or "UnknownCommitThrew") return;
            var rig = new Rig(Views[name]());

            rig.Field.CheckNow();

            Assert.Empty(rig.Port.Calls);
            Assert.Equal(string.Empty, rig.Field.NoticeText);
        }

        [Fact]
        public void TheCheckReadsTheMemoryOfThePortOnceBeforeTheOperationAndOnceAfterItAndNeverReadsWithoutTheButton()
        {
            var rig = new Rig(Dirty());
            rig.Port.ConfirmBaseResponder = _ => EditConfirmOutcome.Confirmed();

            rig.Field.Show(A, Rack, false);
            rig.Field.Show(A, Rack, false);
            Assert.Empty(rig.Port.Reads());

            rig.Port.Calls.Clear();
            rig.Field.CheckNow();

            Assert.Single(rig.Port.Reads());
            Assert.Equal(2, rig.Count(WorkspacePilotEditPortDouble.GetFieldOp));
        }

        // ------------------------------------------------------------------ the classes of the general check (section 3 of the package)

        [Fact]
        public void AMatchOfACleanOrDirtyDraftIsTheTextOfNoChangesAndOnlyThenAndTheMarkOfStalenessStillComesFromTheView()
        {
            foreach (var view in new[] { Clean(), Dirty(), Dirty(stale: true), Clean(stale: false) })
            {
                var rig = new Rig(view);
                rig.Port.ConfirmBaseResponder = _ => EditConfirmOutcome.Confirmed();

                rig.Field.CheckNow();

                Assert.Equal(A201, rig.Field.NoticeText);
                Assert.Equal(string.Empty, rig.Field.NotEditableText);
            }
        }

        [Fact]
        public void AMatchThatTheHostReportedWhileTheViewIsInConflictSaysTheDraftStaysInConflict()
        {
            var rig = new Rig(Conflict(true));
            rig.Port.ConfirmBaseResponder = _ => EditConfirmOutcome.Confirmed();

            rig.Field.CheckNow();

            Assert.Equal(A202, rig.Field.NoticeText);
            Assert.Equal("Conflicto", rig.Field.StateText);
        }

        [Theory]
        [InlineData("Applying")]
        [InlineData("Error")]
        [InlineData("Undecidable")]
        [InlineData("UnknownCommitThrew")]
        [InlineData("UnknownPostReread")]
        public void AMatchWhileTheDraftChangedDuringTheCheckIsNeverNoChangesAndAsksToCheckAgain(string after)
        {
            var rig = new Rig(Dirty());
            rig.Port.ConfirmBaseResponder = _ =>
            {
                rig.Set(Views[after]());
                return EditConfirmOutcome.Confirmed();
            };

            rig.Field.CheckNow();

            Assert.Equal(A235, rig.Field.NoticeText);
            Assert.DoesNotContain("Sin cambios", rig.Field.NoticeText, StringComparison.Ordinal);
        }

        [Fact]
        public void AMatchWhileTheDraftNoLongerExistsSaysSo()
        {
            var rig = new Rig(Dirty());
            rig.Port.ConfirmBaseResponder = _ =>
            {
                rig.Port.Remove(A, Rack);
                return EditConfirmOutcome.Confirmed();
            };

            rig.Field.CheckNow();

            Assert.Equal(A237, rig.Field.NoticeText);
        }

        [Fact]
        public void AConflictWithoutDiagnosticIsTheExternalChangeTheOwnWorkOrTheOwnPendingTextByWhatTheViewHoldsAfterwards()
        {
            string Run(PilotFieldView before, PilotFieldView after)
            {
                var rig = new Rig(before);
                rig.Port.ConfirmBaseResponder = _ =>
                {
                    rig.Set(after);
                    return EditConfirmOutcome.Conflict(new[] { "A1:EnvelopeText" });
                };
                rig.Field.CheckNow();
                return rig.Field.NoticeText;
            }

            Assert.Equal(A204, Run(Clean(), Conflict(false)));
            Assert.Equal(A205, Run(Dirty(), Conflict(true)));
            Assert.Equal(A205b, Run(Dirty(), Conflict(true, pending: "2x")));
            Assert.Equal(A205b, Run(Dirty(), Conflict(false, pending: "2x")));
        }

        [Fact]
        public void ANotAdmittedReadIsAPersistentDiagnosticWithItsHintAndTheTextOfTheNotEditableRackForAMatchAndForAConflict()
        {
            var confirmed = new Rig(Dirty(stale: true));
            confirmed.Port.ConfirmBaseResponder = _ => EditConfirmOutcome.Confirmed("DIAG-A", "HINT-A");
            confirmed.Field.CheckNow();

            Assert.Equal(A203, confirmed.Field.NoticeText);
            Assert.Equal("DIAG-A", confirmed.Field.NotEditableText);
            Assert.Equal("HINT-A", confirmed.Field.ClassicEditorHintText);
            Assert.True(confirmed.Field.IsReadOnly);

            var conflict = new Rig(Dirty());
            conflict.Port.ConfirmBaseResponder = _ =>
            {
                conflict.Set(Conflict(false));
                return EditConfirmOutcome.Conflict(new[] { "A1:EnvelopeText" }, "DIAG-B", "HINT-B");
            };
            conflict.Field.CheckNow();

            Assert.Equal(A206, conflict.Field.NoticeText);
            Assert.Equal("DIAG-B", conflict.Field.NotEditableText);
            Assert.Equal("HINT-B", conflict.Field.ClassicEditorHintText);
            Assert.True(conflict.Field.IsReadOnly);
            Assert.False(conflict.Field.ShowDiscardAndReloadButton);
            Assert.False(conflict.Field.ShowKeepMineButton);
        }

        public static IEnumerable<object[]> ConfirmStatusesThatDidNotReadTheDrawing() => new[]
        {
            new object[] { EditConfirmOutcome.Unavailable("no hay lector."), "No disponible: no hay lector." + A208 },
            new object[] { EditConfirmOutcome.Waiting("hay un comando."), "En espera: hay un comando." + A208 },
            new object[] { EditConfirmOutcome.Rejected("otro documento."), "Rechazado: otro documento." + A208 },
            new object[] { EditConfirmOutcome.NotFound("sin borrador."), "Sin borrador: sin borrador." + A208 },
            new object[] { EditConfirmOutcome.NotApplicable("no aplica."), "No aplicable: no aplica." + A208 },
            new object[] { EditConfirmOutcome.Failed("falló."), "Fallo: falló." + A208 },
            new object[] { EditConfirmOutcome.Failed(string.Empty), "Fallo: sin detalle." + A208 },
            new object[] { default(EditConfirmOutcome), "No disponible: sin detalle." + A208 },
            new object[] { UnknownConfirm(), "No se pudo comprobar: forged" + A208 },
        };

        [Theory]
        [MemberData(nameof(ConfirmStatusesThatDidNotReadTheDrawing))]
        public void AnAnswerOfTheGeneralCheckThatDidNotReadTheDrawingHasItsPrefixItsReasonAndTheTextOfNotChecked(EditConfirmOutcome outcome, string expected)
        {
            var rig = new Rig(Dirty(stale: true));
            rig.Port.ConfirmBaseResponder = _ => outcome;
            var before = rig.Port.Current(A, Rack);

            rig.Field.CheckNow();

            Assert.Equal(expected, rig.Field.NoticeText);
            Assert.Equal(before, rig.Port.Current(A, Rack));
            Assert.Equal("Modificado (posiblemente obsoleto)", rig.Field.StateText);
            Assert.Equal(string.Empty, rig.Field.NotEditableText);
        }

        // ------------------------------------------------------------------ no false clean (INV-F4T4A2-04)

        [Fact]
        public void NoAnswerOfTheGeneralCheckButAMatchWithoutDiagnosticOverACleanOrDirtyViewEverSaysNoChanges()
        {
            var statuses = Enum.GetValues<EditConfirmStatus>().Cast<int>().Concat(new[] { 99 }).ToArray();
            var after = new[] { Clean(), Dirty(), Conflict(true), Unknown(PilotUnknownOrigin.CommitThrew), OfState(PilotFieldState.Applying) };
            var seen = 0;

            foreach (var status in statuses)
            {
                foreach (var view in after)
                {
                    EditConfirmOutcome Answer() => status switch
                    {
                        (int)EditConfirmStatus.Unavailable => EditConfirmOutcome.Unavailable("r"),
                        (int)EditConfirmStatus.Waiting => EditConfirmOutcome.Waiting("r"),
                        (int)EditConfirmStatus.Rejected => EditConfirmOutcome.Rejected("r"),
                        (int)EditConfirmStatus.NotFound => EditConfirmOutcome.NotFound("r"),
                        (int)EditConfirmStatus.NotApplicable => EditConfirmOutcome.NotApplicable("r"),
                        (int)EditConfirmStatus.Confirmed => EditConfirmOutcome.Confirmed("DIAG", "HINT"),
                        (int)EditConfirmStatus.Conflict => EditConfirmOutcome.Conflict(new[] { "A1:EnvelopeText" }, "DIAG", "HINT"),
                        (int)EditConfirmStatus.Failed => EditConfirmOutcome.Failed("r"),
                        _ => UnknownConfirm(),
                    };

                    var rig = new Rig(Dirty(stale: true));
                    rig.Port.ConfirmBaseResponder = _ =>
                    {
                        rig.Set(view);
                        return Answer();
                    };

                    rig.Field.CheckNow();
                    seen++;

                    Assert.DoesNotContain("Sin cambios", rig.Field.NoticeText, StringComparison.Ordinal);
                    Assert.DoesNotContain("el dibujo coincide con la base del borrador", rig.Field.NoticeText, StringComparison.Ordinal);
                    Assert.DoesNotContain("El cambio no llegó a aplicarse", rig.Field.NoticeText, StringComparison.Ordinal);
                }
            }

            Assert.Equal(statuses.Length * after.Length, seen);
        }

        [Fact]
        public void TheDefaultValueTheUnknownValueAndAnExceptionOfThePortNeverLeaveTheSurfaceCleanerThanItWas()
        {
            foreach (var answer in new[] { default(EditConfirmOutcome), UnknownConfirm() })
            {
                var rig = new Rig(Dirty(stale: true));
                rig.Port.ConfirmBaseResponder = _ => answer;
                rig.Field.CheckNow();
                Assert.DoesNotContain("Sin cambios", rig.Field.NoticeText, StringComparison.Ordinal);
                Assert.Equal("Modificado (posiblemente obsoleto)", rig.Field.StateText);
            }

            var thrown = new Rig(Dirty(stale: true));
            thrown.Port.Throws[WorkspacePilotEditPortDouble.ConfirmBaseOp] = new InvalidOperationException("boom");
            thrown.Field.CheckNow();

            Assert.Equal("Error: boom", thrown.Field.StateText);
            Assert.True(thrown.Field.IsReadOnly);
            Assert.False(thrown.Field.ShowCheckButton);
            Assert.DoesNotContain("Sin cambios", thrown.Field.NoticeText, StringComparison.Ordinal);
            Assert.True(thrown.Port.Current(A, Rack).IsPossiblyStale);
        }

        [Fact]
        public void TheCheckOfAnUnknownDraftNeverSaysTheChangeWasNotAppliedUnlessTheHostRecoveredIt()
        {
            var statuses = Enum.GetValues<EditCheckStatus>().Where(s => s != EditCheckStatus.Recovered).ToArray();
            var answers = statuses.Select(s => s switch
            {
                EditCheckStatus.Unavailable => EditCheckOutcome.Unavailable("r"),
                EditCheckStatus.Waiting => EditCheckOutcome.Waiting("r"),
                EditCheckStatus.Rejected => EditCheckOutcome.Rejected("r"),
                EditCheckStatus.NotFound => EditCheckOutcome.NotFound("r"),
                EditCheckStatus.NotApplicable => EditCheckOutcome.NotApplicable("r"),
                EditCheckStatus.Conflict => EditCheckOutcome.Conflict(new[] { "A1:EnvelopeText" }),
                _ => EditCheckOutcome.Failed("r"),
            }).Concat(new[] { default(EditCheckOutcome), UnknownCheck() }).ToArray();

            foreach (var answer in answers)
            {
                var rig = new Rig(Unknown(PilotUnknownOrigin.CommitThrew));
                rig.Port.CheckNowResponder = _ => answer;

                rig.Field.CheckNow();

                Assert.DoesNotContain("El cambio no llegó a aplicarse", rig.Field.NoticeText, StringComparison.Ordinal);
                Assert.DoesNotContain("Sin cambios", rig.Field.NoticeText, StringComparison.Ordinal);
                Assert.Equal("Desconocido", rig.Field.StateText);
            }

            var thrown = new Rig(Unknown(PilotUnknownOrigin.CommitThrew));
            thrown.Port.Throws[WorkspacePilotEditPortDouble.CheckOp] = new InvalidOperationException("boom");
            thrown.Field.CheckNow();
            Assert.Equal("Error: boom", thrown.Field.StateText);
            Assert.DoesNotContain("El cambio no llegó a aplicarse", thrown.Field.NoticeText, StringComparison.Ordinal);
        }

        // ------------------------------------------------------------------ the check of an unknown draft (the second table of section 3)

        [Fact]
        public void ARecoveredCheckSaysTheChangeWasNotAppliedAndAConflictSaysNeitherThatNorThatItWasApplied()
        {
            var recovered = new Rig(Unknown(PilotUnknownOrigin.CommitThrew));
            recovered.Port.CheckNowResponder = _ =>
            {
                recovered.Set(Dirty());
                return EditCheckOutcome.Recovered();
            };
            recovered.Field.CheckNow();
            Assert.Equal(A207, recovered.Field.NoticeText);
            Assert.Equal("Modificado", recovered.Field.StateText);

            var conflict = new Rig(Unknown(PilotUnknownOrigin.CommitThrew));
            conflict.Port.CheckNowResponder = _ =>
            {
                conflict.Set(Conflict(true));
                return EditCheckOutcome.Conflict(new[] { "A1:EnvelopeText" });
            };
            conflict.Field.CheckNow();
            Assert.Equal(A209, conflict.Field.NoticeText);
            Assert.Equal("Conflicto", conflict.Field.StateText);
        }

        public static IEnumerable<object[]> CheckStatusesThatDidNotReadTheDrawing() => new[]
        {
            new object[] { EditCheckOutcome.Unavailable("no hay lector."), "No disponible: no hay lector." + A208 },
            new object[] { EditCheckOutcome.Waiting("hay un comando."), "En espera: hay un comando." + A208 },
            new object[] { EditCheckOutcome.Rejected("otro documento."), "Rechazado: otro documento." + A208 },
            new object[] { EditCheckOutcome.NotFound("sin borrador."), "Sin borrador: sin borrador." + A208 },
            new object[] { EditCheckOutcome.NotApplicable("no aplica."), "No aplicable: no aplica." + A208 },
            new object[] { EditCheckOutcome.Failed("falló."), "Fallo: falló." + A208 },
            new object[] { default(EditCheckOutcome), "No disponible: sin detalle." + A208 },
            new object[] { UnknownCheck(), "No se pudo comprobar: forged" + A208 },
        };

        [Theory]
        [MemberData(nameof(CheckStatusesThatDidNotReadTheDrawing))]
        public void AnAnswerOfTheCheckOfAnUnknownDraftThatDidNotReadTheDrawingHasItsPrefixAndTheTextOfNotChecked(EditCheckOutcome outcome, string expected)
        {
            var rig = new Rig(Unknown(PilotUnknownOrigin.CommitThrew));
            rig.Port.CheckNowResponder = _ => outcome;

            rig.Field.CheckNow();

            Assert.Equal(expected, rig.Field.NoticeText);
            Assert.Equal("Desconocido", rig.Field.StateText);
            Assert.Equal(A211, rig.Field.ReconcileText);
        }

        // ------------------------------------------------------------------ a surface that changes during the read

        [Fact]
        public void ASurfaceThatChangesDuringTheReadShowsTheExistingNoticeAndNeverTheResultOfTheOtherSurface()
        {
            var rig = new Rig(Dirty(stale: true));
            rig.Port.ConfirmBaseResponder = _ => EditConfirmOutcome.Confirmed();
            rig.Port.ReadHook = _ => rig.Field.Show(B, "R-2", false);

            rig.Field.CheckNow();

            Assert.Equal(SurfaceChanged, rig.Field.NoticeText);
            Assert.DoesNotContain("Sin cambios", rig.Field.NoticeText, StringComparison.Ordinal);
            Assert.Equal("Sin borrador.", rig.Field.StateText);
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

        // ------------------------------------------------------------------ the pending text (D-T4A2-05, INV-F4T4A2-08)

        [Fact]
        public void TheCheckKeepsAnInvalidPendingTextByteForByteAndNeverCommitsOrRepairsIt()
        {
            const string typed = "  1:5x \t";
            var rig = new Rig(Conflict(true, pending: typed));
            rig.Port.ConfirmBaseResponder = _ => EditConfirmOutcome.Conflict(new[] { "A1:EnvelopeText" });

            rig.Field.CheckNow();

            Assert.Equal(typed, rig.Port.Current(A, Rack).PendingText);
            Assert.Equal(typed, rig.Field.Text);
            Assert.Equal(0, rig.Count(WorkspacePilotEditPortDouble.CommitOp));
            Assert.Equal(0, rig.Count(WorkspacePilotEditPortDouble.SetPendingOp));
            Assert.Equal(A205b, rig.Field.NoticeText);
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
        public void DiscardAndReloadWarnsThatThePendingTextIsLostAndItIsLostOnlyAfterTheYes()
        {
            var rig = new Rig(Conflict(true, pending: "2x"));
            rig.Port.DiscardAndReloadResponder = _ =>
            {
                rig.Set(Clean());
                return EditIntentOutcome.Reloaded(EditWitnessStatus.NotSupplied);
            };

            rig.Field.RequestDiscardAndReload();

            Assert.Equal(A220 + A220Pending, rig.Field.ChoiceText);
            Assert.Equal("2x", rig.Port.Current(A, Rack).PendingText);
            Assert.Equal(0, rig.Count(WorkspacePilotEditPortDouble.DiscardAndReloadOp));

            rig.Field.ConfirmChoice();

            Assert.Equal(1, rig.Count(WorkspacePilotEditPortDouble.DiscardAndReloadOp));
            Assert.Null(rig.Port.Current(A, Rack).PendingText);
            Assert.Equal(A230, rig.Field.NoticeText);
        }

        // ------------------------------------------------------------------ the options and the text of their confirmation (D-T4A2-02)

        [Theory]
        [MemberData(nameof(ViewNames))]
        public void TheOptionsAreOfferedByTheStateOfTheDraftAndKeepOnlyForAConflictWithAProtectedChangeAndNoPendingText(string name)
        {
            var rig = new Rig(Views[name]());

            Assert.Equal(name is "Conflict" or "UnknownPostReread", rig.Field.ShowDiscardAndReloadButton);
            Assert.Equal(name == "Conflict", rig.Field.ShowKeepMineButton);
        }

        [Fact]
        public void KeepIsOfferedOnlyWhenTheConflictHasACommittedChangeAndNoPendingText()
        {
            Assert.True(new Rig(Conflict(true)).Field.ShowKeepMineButton);
            Assert.False(new Rig(Conflict(false)).Field.ShowKeepMineButton);
            Assert.False(new Rig(Conflict(true, pending: "2x")).Field.ShowKeepMineButton);
            Assert.False(new Rig(Conflict(false, pending: "2x")).Field.ShowKeepMineButton);
            Assert.False(new Rig(Dirty()).Field.ShowKeepMineButton);
            Assert.False(new Rig(Dirty(stale: true)).Field.ShowKeepMineButton);
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

        // ------------------------------------------------------------------ the choice (D-T4A2-03, INV-F4T4A2-05): the inversion tests

        [Fact]
        public void YesOnDiscardAndReloadCallsThatOperationOnceWithTheSessionAndTheRackAndNeverTheKeep()
        {
            var rig = new Rig(Conflict(true));
            rig.Port.DiscardAndReloadResponder = _ =>
            {
                rig.Set(Clean());
                return EditIntentOutcome.Reloaded(EditWitnessStatus.NotSupplied);
            };
            rig.Field.RequestDiscardAndReload();

            rig.Field.ConfirmChoice();

            var call = Assert.Single(rig.Port.Of(WorkspacePilotEditPortDouble.DiscardAndReloadOp));
            Assert.Equal(A, call.Origin);
            Assert.Equal(Rack, call.RackId);
            Assert.Equal(0, rig.Count(WorkspacePilotEditPortDouble.KeepMineOp));
            Assert.Equal(new[] { WorkspacePilotEditPortDouble.DiscardAndReloadOp }, rig.Operations());
            Assert.Equal(A230, rig.Field.NoticeText);
            Assert.False(rig.Field.HasChoice);
            Assert.Equal("Limpio", rig.Field.StateText);
            Assert.Equal(1, rig.Changed);
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
        public void YesOnAnUnknownDraftThatWasAppliedReloadsItWithItsOwnQuestion()
        {
            var rig = new Rig(Unknown(PilotUnknownOrigin.PostReread));
            rig.Port.DiscardAndReloadResponder = _ =>
            {
                rig.Set(Clean());
                return EditIntentOutcome.Reloaded(EditWitnessStatus.NotSupplied);
            };

            rig.Field.RequestDiscardAndReload();
            Assert.Equal(A212, rig.Field.ReconcileText);
            rig.Field.ConfirmChoice();

            Assert.Equal(1, rig.Count(WorkspacePilotEditPortDouble.DiscardAndReloadOp));
            Assert.Equal(A230, rig.Field.NoticeText);
        }

        [Fact]
        public void NoCallsNothingChangesNothingAndSaysSoForEachOfTheTwoChoices()
        {
            foreach (var keep in new[] { false, true })
            {
                var rig = new Rig(Conflict(true, committed: "2.5"));
                if (keep) rig.Field.RequestKeepMine();
                else rig.Field.RequestDiscardAndReload();
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
        public void YesAndNoWithoutAChoiceDoNothing()
        {
            var rig = new Rig(Conflict(true));

            rig.Field.ConfirmChoice();
            rig.Field.CancelChoice();

            Assert.Empty(rig.Port.Calls);
            Assert.Equal(string.Empty, rig.Field.NoticeText);
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
        public void ThePendingTextThatThereWasBeforeAskingIsLeftAsItWasWhileTheChoiceWaits()
        {
            var rig = new Rig(Conflict(false, pending: "2x"));

            rig.Field.RequestDiscardAndReload();
            rig.Field.CommitPending();

            Assert.Equal("2x", rig.Field.Text);
            Assert.Equal("2x", rig.Port.Current(A, Rack).PendingText);
            Assert.Equal(0, rig.Count(WorkspacePilotEditPortDouble.CommitOp));
        }

        [Theory]
        [InlineData("state")]
        [InlineData("protected")]
        [InlineData("pending")]
        [InlineData("committed")]
        [InlineData("origin")]
        public void IfTheViewDiffersFromWhatJustifiedTheChoiceYesCallsNothingAndSaysTheSituationChanged(string change)
        {
            var rig = new Rig(change == "origin" ? Unknown(PilotUnknownOrigin.PostReread) : Conflict(true, committed: "2.5"));
            rig.Field.RequestDiscardAndReload();
            rig.Port.Calls.Clear();
            rig.Set(change switch
            {
                "state" => Dirty(),
                "protected" => Conflict(false),
                "pending" => Conflict(true, pending: "7"),
                "committed" => Conflict(true, committed: "3.0"),
                _ => Unknown(PilotUnknownOrigin.CommitThrew),
            });

            rig.Field.ConfirmChoice();

            Assert.Equal(A234, rig.Field.NoticeText);
            Assert.Empty(rig.Operations());
            Assert.False(rig.Field.HasChoice);
        }

        [Fact]
        public void IfTheViewChangedAfterAskingForKeepYesCallsNeitherOperation()
        {
            var rig = new Rig(Conflict(true, committed: "2.5"));
            rig.Field.RequestKeepMine();
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
            surface.Field.Show(B, "R-2", false);
            Assert.False(surface.Field.HasChoice);
            Assert.Equal(string.Empty, surface.Field.NoticeText);
            Assert.Equal(string.Empty, surface.Field.ChoiceText);

            var cleared = new Rig(Conflict(true));
            cleared.Field.RequestDiscardAndReload();
            cleared.Field.Clear();
            Assert.False(cleared.Field.HasChoice);
            Assert.Equal(string.Empty, cleared.Field.NoticeText);

            var failed = new Rig(Conflict(true));
            failed.Field.RequestDiscardAndReload();
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
            gone.Port.Remove(A, Rack);
            gone.Field.Show(A, Rack, false);
            Assert.False(gone.Field.HasChoice);
            Assert.Equal(A234, gone.Field.NoticeText);

            var changed = new Rig(Conflict(true));
            changed.Field.RequestDiscardAndReload();
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

        // ------------------------------------------------------------------ the persistent diagnostic (D-T4A2-06, INV-F4T4A2-09)

        private static Rig WithPersistentDiagnostic()
        {
            var rig = new Rig(Dirty(stale: true));
            rig.Port.ConfirmBaseResponder = _ => EditConfirmOutcome.Confirmed("DIAG", "HINT");
            rig.Field.CheckNow();
            Assert.Equal("DIAG", rig.Field.NotEditableText);
            rig.Port.Calls.Clear();
            return rig;
        }

        [Fact]
        public void AKeyOnTheBoxOrAnotherGestureNeverClearsThePersistentDiagnosticAndTheBoxStaysReadOnly()
        {
            var rig = WithPersistentDiagnostic();

            rig.Field.Text = "9";
            rig.Field.CommitPending();
            rig.Field.BeginEdit();
            rig.Field.Rematerialize();
            rig.Field.ShowNotice("otro aviso");

            Assert.Equal("DIAG", rig.Field.NotEditableText);
            Assert.Equal("HINT", rig.Field.ClassicEditorHintText);
            Assert.True(rig.Field.IsReadOnly);
            Assert.False(rig.Field.ShowEditButton);
            Assert.False(rig.Field.CanOpenByGesture);
            Assert.Equal(0, rig.Count(WorkspacePilotEditPortDouble.SetPendingOp));
        }

        [Fact]
        public void ADraftThatIsCleanAndPossiblyStaleKeepsTheDiagnosticAndStaysReadOnly()
        {
            var rig = new Rig(Clean(stale: true));
            rig.Port.ConfirmBaseResponder = _ => EditConfirmOutcome.Confirmed("DIAG", "HINT");

            rig.Field.CheckNow();

            Assert.Equal("DIAG", rig.Field.NotEditableText);
            Assert.True(rig.Field.IsReadOnly);
            Assert.False(rig.Field.ShowEditButton);
            Assert.Equal(A203, rig.Field.NoticeText);
        }

        [Fact]
        public void AnAnswerThatDidNotRereadTheDrawingOrAnExceptionKeepsThePersistentDiagnostic()
        {
            foreach (var answer in new[]
                     {
                         EditConfirmOutcome.Waiting("x"), EditConfirmOutcome.Rejected("x"), EditConfirmOutcome.Unavailable("x"), EditConfirmOutcome.NotFound("x"),
                         EditConfirmOutcome.NotApplicable("x"), EditConfirmOutcome.Failed("x"), default, UnknownConfirm(),
                     })
            {
                var rig = WithPersistentDiagnostic();
                rig.Port.ConfirmBaseResponder = _ => answer;

                rig.Field.CheckNow();

                Assert.Equal("DIAG", rig.Field.NotEditableText);
                Assert.Equal("HINT", rig.Field.ClassicEditorHintText);
                Assert.True(rig.Field.IsReadOnly);
            }

            var thrown = WithPersistentDiagnostic();
            thrown.Port.Throws[WorkspacePilotEditPortDouble.ConfirmBaseOp] = new InvalidOperationException("boom");
            thrown.Field.CheckNow();
            Assert.Equal("DIAG", thrown.Field.NotEditableText);
            Assert.Equal("Error: boom", thrown.Field.StateText);
        }

        [Fact]
        public void AnAnswerThatRereadTheDrawingReplacesThePersistentDiagnosticWithItsOwnOrWithNone()
        {
            var cleared = WithPersistentDiagnostic();
            cleared.Port.ConfirmBaseResponder = _ => EditConfirmOutcome.Confirmed();
            cleared.Field.CheckNow();
            Assert.Equal(string.Empty, cleared.Field.NotEditableText);
            Assert.Equal(string.Empty, cleared.Field.ClassicEditorHintText);
            Assert.Equal(A201, cleared.Field.NoticeText);
            Assert.True(cleared.Field.IsEditable);

            var replaced = WithPersistentDiagnostic();
            replaced.Port.ConfirmBaseResponder = _ => EditConfirmOutcome.Conflict(new[] { "A1:EnvelopeText" }, "OTHER", "OTHER-HINT");
            replaced.Field.CheckNow();
            Assert.Equal("OTHER", replaced.Field.NotEditableText);
            Assert.Equal("OTHER-HINT", replaced.Field.ClassicEditorHintText);

            var conflictWithout = WithPersistentDiagnostic();
            conflictWithout.Port.ConfirmBaseResponder = _ => EditConfirmOutcome.Conflict(new[] { "A1:EnvelopeText" });
            conflictWithout.Field.CheckNow();
            Assert.Equal(string.Empty, conflictWithout.Field.NotEditableText);
        }

        [Fact]
        public void TheCheckOfAnUnknownDraftRetiresThePersistentDiagnosticOnlyWhenItRereadTheDrawing()
        {
            foreach (var reread in new[] { true, false })
            {
                var rig = new Rig(Unknown(PilotUnknownOrigin.CommitThrew));
                rig.Port.ConfirmBaseResponder = _ => EditConfirmOutcome.Confirmed("DIAG", "HINT");
                rig.Set(Dirty(stale: true));
                rig.Field.Show(A, Rack, false);
                rig.Field.CheckNow();
                Assert.Equal("DIAG", rig.Field.NotEditableText);

                rig.Set(Unknown(PilotUnknownOrigin.CommitThrew));
                rig.Field.Show(A, Rack, false);
                rig.Port.CheckNowResponder = _ => reread ? EditCheckOutcome.Recovered() : EditCheckOutcome.Waiting("x");
                rig.Field.CheckNow();

                Assert.Equal(reread ? string.Empty : "DIAG", rig.Field.NotEditableText);
            }
        }

        [Fact]
        public void TheDiagnosticIsRemovedByAChangeOfSurfaceByClearAndByADraftThatNoLongerExists()
        {
            var surface = WithPersistentDiagnostic();
            surface.Field.Show(B, "R-2", false);
            surface.Field.Show(A, Rack, false);
            Assert.Equal(string.Empty, surface.Field.NotEditableText);
            Assert.Equal(string.Empty, surface.Field.ClassicEditorHintText);

            var cleared = WithPersistentDiagnostic();
            cleared.Field.Clear();
            Assert.Equal(string.Empty, cleared.Field.NotEditableText);

            var gone = WithPersistentDiagnostic();
            gone.Port.Remove(A, Rack);
            gone.Field.Show(A, Rack, false);
            Assert.Equal(string.Empty, gone.Field.NotEditableText);
            Assert.Equal(string.Empty, gone.Field.ClassicEditorHintText);
            Assert.True(gone.Field.ShowEditButton);

            var stillThere = WithPersistentDiagnostic();
            stillThere.Field.Show(A, Rack, false);
            Assert.Equal("DIAG", stillThere.Field.NotEditableText);
        }

        [Fact]
        public void TheDiagnosticOfTheFirstGestureComesBeforeThePersistentOneAndTheTwoDoNotMix()
        {
            var rig = new Rig(Dirty(stale: true));
            rig.Port.ConfirmBaseResponder = _ => EditConfirmOutcome.Confirmed("PERSISTENT", "PERSISTENT-HINT");
            rig.Field.CheckNow();
            rig.Port.BeginResponder = _ => EditIntentOutcome.NotAdmitted("FIRST", null, "FIRST-HINT", EditWitnessStatus.NotSupplied);

            rig.Field.BeginEdit();

            Assert.Equal("FIRST", rig.Field.NotEditableText);
            Assert.Equal("FIRST-HINT", rig.Field.ClassicEditorHintText);

            rig.Field.Rematerialize();

            Assert.Equal("PERSISTENT", rig.Field.NotEditableText);
            Assert.Equal("PERSISTENT-HINT", rig.Field.ClassicEditorHintText);
        }

        // ------------------------------------------------------------------ the line of the reconciliation (D-T4A2-06)

        [Fact]
        public void TheLineOfAConflictSaysWhatChangedAndWhyAndTheLineOfAnUnknownDraftSaysWhatToDo()
        {
            Assert.Equal("Qué cambió: A1:EnvelopeText", new Rig(Conflict(true)).Field.ReconcileText);
            Assert.Equal("Qué cambió: sin detalle.", new Rig(Conflict(true, conflictText: null)).Field.ReconcileText);
            Assert.Equal("Qué cambió: sin detalle.", new Rig(Conflict(true, conflictText: "  ")).Field.ReconcileText);
            Assert.Equal("Qué cambió: A1:EnvelopeText Causa: se cayó", new Rig(Conflict(true, lastCause: "se cayó")).Field.ReconcileText);
            Assert.Equal("Qué cambió: A1:EnvelopeText", new Rig(Conflict(true, lastCause: " ")).Field.ReconcileText);
            Assert.Equal(A211, new Rig(Unknown(PilotUnknownOrigin.CommitThrew)).Field.ReconcileText);
            Assert.Equal(A212, new Rig(Unknown(PilotUnknownOrigin.PostReread)).Field.ReconcileText);
            Assert.Equal(string.Empty, new Rig(Unknown(PilotUnknownOrigin.None)).Field.ReconcileText);
            Assert.Equal(string.Empty, new Rig(Dirty()).Field.ReconcileText);
            Assert.Equal(string.Empty, new Rig(Clean()).Field.ReconcileText);
            Assert.Equal(string.Empty, new Rig(WorkspacePilotEditPortDouble.NoDraft()).Field.ReconcileText);
        }

        [Fact]
        public void ThePendingLineIsAddedWithAPendingTextAndNotWithAPersistentDiagnosticOrAChoice()
        {
            var pending = new Rig(Conflict(true, pending: "2x"));
            Assert.Equal("Qué cambió: A1:EnvelopeText " + A214, pending.Field.ReconcileText);

            var withCause = new Rig(Conflict(true, pending: "2x", lastCause: "falló"));
            Assert.Equal("Qué cambió: A1:EnvelopeText Causa: falló " + A214, withCause.Field.ReconcileText);

            var choice = new Rig(Conflict(false, pending: "2x"));
            choice.Field.RequestDiscardAndReload();
            Assert.Equal("Qué cambió: A1:EnvelopeText", choice.Field.ReconcileText);

            var diagnostic = new Rig(Dirty());
            diagnostic.Port.ConfirmBaseResponder = _ =>
            {
                diagnostic.Set(Conflict(false, pending: "2x"));
                return EditConfirmOutcome.Conflict(new[] { "A1:EnvelopeText" }, "DIAG", "HINT");
            };
            diagnostic.Field.CheckNow();
            Assert.Equal("Qué cambió: A1:EnvelopeText", diagnostic.Field.ReconcileText);
        }

        [Fact]
        public void TheStateTextIsTheOneOfBefore()
        {
            Assert.Equal("Conflicto", new Rig(Conflict(true)).Field.StateText);
            Assert.Equal("Desconocido", new Rig(Unknown(PilotUnknownOrigin.CommitThrew)).Field.StateText);
            Assert.Equal("Desconocido", new Rig(Unknown(PilotUnknownOrigin.PostReread)).Field.StateText);
            Assert.Equal("Modificado", new Rig(Dirty()).Field.StateText);
            Assert.Equal("Limpio", new Rig(Clean()).Field.StateText);
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

        [Fact]
        public void TheFieldNeverCallsAnOperationOfTheDrawingWithoutAGestureOfTheUser()
        {
            var rig = new Rig(Conflict(true, pending: "2x"));

            rig.Field.Show(A, Rack, false);
            rig.Field.Show(A, Rack, true);
            rig.Field.Show(A, Rack, false);
            rig.Field.Rematerialize();
            rig.Field.ShowNotice("aviso");
            rig.Field.Clear();

            Assert.Empty(rig.Port.Reads());
            Assert.Empty(rig.Operations());
        }
    }
}
