using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Systems.Selective;
using RackCad.Application.Systems.Shared;
using RackCad.UI.Systems.Selective;
using Xunit;
using S = RackCad.UI.Tests.SelectiveHeaderBatchTestSupport;

namespace RackCad.UI.Tests
{
    /// <summary>
    /// I-53S, G5 (ID6 REUSE + ID7 BATCH DISTRIBUTION en la ventana del Selectivo; Proposal V2 §3.3-§3.11, §6): el gesto
    /// «Tomar como origen» + «Postes destino» × «Fondos destino» + «Aplicar origen a destinos», y el EDIT de «Personalizar»,
    /// recorridos por sus handlers REALES.
    /// <para>
    /// El origen es una DIRECCION: su configuracion se lee al aplicar. PREPARE, el plan, el Outcome y la mutacion son de
    /// Application; la ventana solo recoge la intencion, respeta la frontera C4 y RR-01, pide confirmacion por la costura y
    /// abre UN ambito diferido que contiene exactamente MUTATE y su recompute.
    /// </para>
    /// </summary>
    public sealed class SelectiveHeaderBatchWindowTests
    {
        private static IDisposable Prompts(List<string> severe = null, List<string> informative = null, bool confirm = true)
            => SelectiveCabeceraHeightPrompt.Substitute(
                message => { severe?.Add(message); return confirm; },
                message => informative?.Add(message));

        /// <summary>Un rack de un fondo con una cabecera personalizada en «Poste <paramref name="post"/>», tomada como origen.</summary>
        private static RackSelectiveWindow WithSourceAt(int post, double? height = null, int frentes = 2)
        {
            var window = SelectiveWindowTestSupport.Open();
            if (frentes != 2)
            {
                EditorWindowTestSupport.SetText(window, "BayCountBox", frentes.ToString(System.Globalization.CultureInfo.InvariantCulture));
                SelectiveWindowTestSupport.RaiseLostFocus(window, "BayCountBox");
            }

            S.PlaceCustom(window, 1, post, height ?? window.CustomizeSeedHeightForTest(post - 1));
            S.SelectPost(window, post);
            S.TakeSource(window);
            return window;
        }

        // =============================================================================================================
        // Origen y destinos
        // =============================================================================================================

        [Fact]
        public void Distribute_CopiesTheSourceToTheChosenPosts_EachOneAnIndependentCopy()
        {
            var r = StaTestRunner.Run(() =>
            {
                var window = WithSourceAt(1);
                var height = window.EditorState.CabeceraAt(0, 0).Height;
                S.SetPostTargets(window, 2, 3);
                using (Prompts())
                {
                    S.Apply(window);
                }

                var state = window.EditorState;
                var source = state.CabeceraAt(0, 0);
                var a = state.CabeceraAt(0, 1);
                var b = state.CabeceraAt(0, 2);
                return (
                    Outcome: window.LastHeaderBatchOutcome,
                    Heights: new[] { a?.Height ?? 0.0, b?.Height ?? 0.0 },
                    Expected: height,
                    Independent: a != null && b != null && !ReferenceEquals(a, b) && !ReferenceEquals(a, source) && !ReferenceEquals(b, source));
            });

            var committed = Assert.IsType<HeaderBatchOutcome<SelectiveHeaderAddress>.Committed>(r.Outcome);
            Assert.Equal("0/1 0/2", S.Addresses(committed.Applied));
            Assert.Empty(committed.Omitted);
            Assert.Equal(new[] { r.Expected, r.Expected }, r.Heights);
            Assert.True(r.Independent, "Cada destino debe recibir su propia copia, distinta del origen y de los demas destinos.");
        }

        [Fact]
        public void Distribute_AllPostsTimesAllFondos_OmitsTheSourceAndThePostsAFondoDoesNotHave()
        {
            var r = StaTestRunner.Run(() =>
            {
                var window = SelectiveWindowTestSupport.Open(2);
                S.SetFrentesOfFondo(window, 1, 3); // postes 0..3
                S.SetFrentesOfFondo(window, 2, 1); // postes 0..1
                S.ShowFondo(window, 1);
                S.PlaceCustom(window, 1, 1, window.CustomizeSeedHeightForTest(0));
                S.SelectPost(window, 1);
                S.TakeSource(window);
                SelectiveTargetsTestSupport.SetAllTargets(window);
                S.SetAllPostTargets(window);
                var frentes = new[] { window.EditorState.Bays.Count, window.EditorState.FondoMatrices[1].Bays.Count };
                using (Prompts())
                {
                    S.Apply(window);
                }

                return (Outcome: window.LastHeaderBatchOutcome, Frentes: frentes, Status: window.StatusText.Text,
                    FondoTwoPostThree: window.EditorState.CabeceraAt(1, 2));
            });

            var committed = Assert.IsType<HeaderBatchOutcome<SelectiveHeaderAddress>.Committed>(r.Outcome);
            Assert.Equal("0/1 0/2 0/3 1/0 1/1", S.Addresses(committed.Applied));
            Assert.Equal("0/0:IsSource 1/2:AbsentInScope 1/3:AbsentInScope", S.Omissions(committed.Omitted));
            Assert.Null(r.FondoTwoPostThree);           // omitido: ni se crea ni se recorta a un vecino
            Assert.Equal(new[] { 3, 1 }, r.Frentes);    // y no se inventaron frentes para que existiera
            Assert.Contains("Omitid", r.Status);         // el informe no esconde las omisiones
        }

        [Fact]
        public void Distribute_TheSourceIsLive_EditingItAfterTakingItAppliesTheNewValue()
        {
            var r = StaTestRunner.Run(() =>
            {
                var window = WithSourceAt(1);
                var edited = window.EditorState.CabeceraAt(0, 0).Height + 24.0;
                S.PlaceCustom(window, 1, 1, edited); // el usuario edita ESA cabecera despues de tomarla
                S.SetPostTargets(window, 2);
                using (Prompts())
                {
                    S.Apply(window);
                }

                return (Outcome: window.LastHeaderBatchOutcome, Height: window.EditorState.CabeceraAt(0, 1)?.Height ?? 0.0, Expected: edited);
            });

            Assert.IsType<HeaderBatchOutcome<SelectiveHeaderAddress>.Committed>(r.Outcome);
            Assert.Equal(r.Expected, r.Height);
        }

        [Fact]
        public void Distribute_ASourceThatNoLongerExists_IsRejected_WithZeroMutationAndNoRecompute()
        {
            var r = StaTestRunner.Run(() =>
            {
                var window = WithSourceAt(4, frentes: 3);    // origen en «Poste 4»
                EditorWindowTestSupport.SetText(window, "BayCountBox", "2");
                SelectiveWindowTestSupport.RaiseLostFocus(window, "BayCountBox"); // el poste 4 deja de existir
                S.SelectPost(window, 1);
                S.SetPostTargets(window, 2);
                var rows = S.CabeceraRows(window);
                var before = window.RecomputeCount;
                using (Prompts())
                {
                    S.Apply(window);
                }

                return (Outcome: window.LastHeaderBatchOutcome, Same: rows == S.CabeceraRows(window),
                    Recomputes: window.RecomputeCount - before, Status: window.StatusText.Text);
            });

            var rejected = Assert.IsType<HeaderBatchOutcome<SelectiveHeaderAddress>.Rejected>(r.Outcome);
            Assert.Equal(HeaderRejectionCode.SourceNotFound, rejected.Code);
            Assert.True(r.Same);
            Assert.Equal(0, r.Recomputes);
            Assert.Contains("origen", r.Status);
        }

        [Fact]
        public void Distribute_AStandardSource_IsRejectedAsUnusable()
        {
            var r = StaTestRunner.Run(() =>
            {
                var window = SelectiveWindowTestSupport.Open();
                S.SelectPost(window, 1);
                S.TakeSource(window); // un poste ESTANDAR: la direccion se recuerda; la elegibilidad la decide PREPARE
                S.SetPostTargets(window, 2);
                var rows = S.CabeceraRows(window);
                using (Prompts())
                {
                    S.Apply(window);
                }

                return (Outcome: window.LastHeaderBatchOutcome, Same: rows == S.CabeceraRows(window));
            });

            var rejected = Assert.IsType<HeaderBatchOutcome<SelectiveHeaderAddress>.Rejected>(r.Outcome);
            Assert.Equal(HeaderRejectionCode.SourceUnusable, rejected.Code);
            Assert.True(r.Same);
        }

        [Fact]
        public void Apply_WithoutASource_SaysSoAndDoesNothing()
        {
            var r = StaTestRunner.Run(() =>
            {
                var window = SelectiveWindowTestSupport.Open();
                S.SetPostTargets(window, 2);
                var rows = S.CabeceraRows(window);
                var before = window.RecomputeCount;
                using (Prompts())
                {
                    S.Apply(window);
                }

                return (Outcome: window.LastHeaderBatchOutcome, Same: rows == S.CabeceraRows(window),
                    Recomputes: window.RecomputeCount - before, Status: window.StatusText.Text);
            });

            Assert.Null(r.Outcome);
            Assert.True(r.Same);
            Assert.Equal(0, r.Recomputes);
            Assert.Contains("origen", r.Status);
        }

        [Fact]
        public void TakeSource_RemembersOnlyTheAddress_AndTheCaptionNamesIt()
        {
            var r = StaTestRunner.Run(() =>
            {
                var window = SelectiveWindowTestSupport.Open();
                S.PlaceCustom(window, 1, 2, window.CustomizeSeedHeightForTest(1));
                S.SelectPost(window, 2);
                S.TakeSource(window);
                var caption = ((System.Windows.Controls.TextBlock)window.FindName("HeaderSourceText"))?.Text;
                return (Source: window.HeaderSourceForTest, Caption: caption);
            });

            Assert.Equal(new SelectiveHeaderAddress(0, 1), r.Source);
            Assert.Contains("Poste 2", r.Caption);
            Assert.Contains("personalizada", r.Caption);
        }

        [Fact]
        public void PostTargets_TheSelectorDrivesTheApplicationTargets_AndReconcilesAfterAStructuralChange()
        {
            var r = StaTestRunner.Run(() =>
            {
                var window = SelectiveWindowTestSupport.Open(); // 2 frentes: postes 1..3
                var initial = (S.PostTargetsCaption(window), S.PostTargetBoxes(window));

                S.SetPostTargets(window, 2, 3);
                var chosen = (S.PostTargetsCaption(window), window.PostTargetsForTest.Mode, window.PostTargetsForTest.ExplicitPosts.ToArray());

                S.SetPostTargets(window, 3);
                EditorWindowTestSupport.SetText(window, "BayCountBox", "1");
                SelectiveWindowTestSupport.RaiseLostFocus(window, "BayCountBox"); // el poste 3 desaparece: el explicito queda vacio
                var pruned = (S.PostTargetsCaption(window), window.PostTargetsForTest.Mode);

                S.SetAllPostTargets(window);
                EditorWindowTestSupport.SetText(window, "BayCountBox", "3");
                SelectiveWindowTestSupport.RaiseLostFocus(window, "BayCountBox"); // «Todos» se re-expande
                var all = (S.PostTargetsCaption(window), S.PostTargetBoxes(window), window.PostTargetsForTest.Mode);

                return (Initial: initial, Chosen: chosen, Pruned: pruned, All: all);
            });

            Assert.Equal("Actual", r.Initial.Item1);
            Assert.Equal(new[] { "Poste 1", "Poste 2", "Poste 3" }, r.Initial.Item2);
            Assert.Equal("Postes 2, 3", r.Chosen.Item1);
            Assert.Equal(SelectivePostTargetMode.Explicit, r.Chosen.Item2);
            Assert.Equal(new[] { 1, 2 }, r.Chosen.Item3);
            Assert.Equal("Actual", r.Pruned.Item1);
            Assert.Equal(SelectivePostTargetMode.FollowCurrent, r.Pruned.Item2);
            Assert.Equal("Todos", r.All.Item1);
            Assert.Equal(new[] { "Poste 1", "Poste 2", "Poste 3", "Poste 4" }, r.All.Item2);
            Assert.Equal(SelectivePostTargetMode.All, r.All.Item3);
        }

        // =============================================================================================================
        // S-27 — la frontera C4 es ANTERIOR al lote
        // =============================================================================================================

        [Fact]
        public void S27_AnInvalidPendingField_AbortsTheGesture_WithoutPlanOutcomeMutationOrRecompute()
        {
            var r = StaTestRunner.Run(() =>
            {
                var window = WithSourceAt(1);
                S.SetPostTargets(window, 2);
                EditorWindowTestSupport.SetText(window, "FondoBox", "abc"); // pendiente INVALIDO, sin salir del campo
                var rows = S.CabeceraRows(window);
                var before = window.RecomputeCount;
                using (Prompts())
                {
                    S.Apply(window);
                }

                return (Log: window.HeaderBatchLog.ToArray(), Outcome: window.LastHeaderBatchOutcome,
                    Same: rows == S.CabeceraRows(window), Recomputes: window.RecomputeCount - before, Status: window.StatusText.Text);
            });

            Assert.Contains("c4:abortado", r.Log);
            Assert.DoesNotContain(r.Log, entry => entry.StartsWith("precondiciones", StringComparison.Ordinal));
            Assert.DoesNotContain(r.Log, entry => entry.StartsWith("plan:", StringComparison.Ordinal));
            Assert.Null(r.Outcome);
            Assert.True(r.Same);
            Assert.Equal(0, r.Recomputes);
            Assert.Contains("Fondo de tarima inválido", r.Status);
        }

        [Fact]
        public void S27_AValidPendingField_IsCommittedFirst_AndItsRecomputeIsCountedApart()
        {
            var r = StaTestRunner.Run(() =>
            {
                var window = WithSourceAt(1);
                S.SetPostTargets(window, 2);
                EditorWindowTestSupport.SetText(window, "FondoBox", "60"); // pendiente VALIDO (48 -> 60)
                var before = window.RecomputeCount;
                using (Prompts())
                {
                    S.Apply(window);
                }

                var state = window.EditorState;
                return (Outcome: window.LastHeaderBatchOutcome, Recomputes: window.RecomputeCount - before,
                    Depth: state.CabeceraAt(0, 1)?.Depth ?? 0.0, FondoDepth: state.CabeceraDepthOfFondo(0));
            });

            Assert.IsType<HeaderBatchOutcome<SelectiveHeaderAddress>.Committed>(r.Outcome);
            Assert.Equal(2, r.Recomputes);          // uno de la frontera C4 + uno del lote
            Assert.Equal(54.0, r.FondoDepth);        // 60 - 6: PREPARE leyo el estado YA comprometido
            Assert.Equal(r.FondoDepth, r.Depth);
        }

        // =============================================================================================================
        // S-28 — un recompute por operacion confirmada; cero en Rejected y Cancelled
        // =============================================================================================================

        [Fact]
        public void S28_ACommittedDistribution_RunsExactlyOneRecompute()
        {
            var r = StaTestRunner.Run(() =>
            {
                var window = WithSourceAt(1);
                S.SetPostTargets(window, 2, 3);
                var before = window.RecomputeCount;
                using (Prompts())
                {
                    S.Apply(window);
                }

                return (Outcome: window.LastHeaderBatchOutcome, Recomputes: window.RecomputeCount - before);
            });

            Assert.IsType<HeaderBatchOutcome<SelectiveHeaderAddress>.Committed>(r.Outcome);
            Assert.Equal(1, r.Recomputes);
        }

        [Fact]
        public void S28_ARejectedPlan_RunsNoRecompute()
        {
            var r = StaTestRunner.Run(() =>
            {
                var window = SelectiveWindowTestSupport.Open();
                S.SelectPost(window, 1);
                S.TakeSource(window); // estandar: Rejected(SourceUnusable)
                S.SetPostTargets(window, 2);
                var before = window.RecomputeCount;
                using (Prompts())
                {
                    S.Apply(window);
                }

                return (Outcome: window.LastHeaderBatchOutcome, Recomputes: window.RecomputeCount - before);
            });

            Assert.IsType<HeaderBatchOutcome<SelectiveHeaderAddress>.Rejected>(r.Outcome);
            Assert.Equal(0, r.Recomputes);
        }

        [Fact]
        public void S28_ACancelledPlan_RunsNoRecompute_AndMutatesNothing()
        {
            var r = StaTestRunner.Run(() =>
            {
                var window = WithSourceAt(1, height: 20.0); // RIDICULAMENTE baja: severa en cada destino
                S.SetPostTargets(window, 2, 3);
                var rows = S.CabeceraRows(window);
                var before = window.RecomputeCount;
                var asked = new List<string>();
                using (Prompts(asked, confirm: false))
                {
                    S.Apply(window);
                }

                return (Outcome: window.LastHeaderBatchOutcome, Recomputes: window.RecomputeCount - before,
                    Same: rows == S.CabeceraRows(window), Asked: asked.Count);
            });

            Assert.IsType<HeaderBatchOutcome<SelectiveHeaderAddress>.Cancelled>(r.Outcome);
            Assert.Equal(1, r.Asked);
            Assert.Equal(0, r.Recomputes);
            Assert.True(r.Same);
        }

        [Fact]
        public void S28_APlanRejectedAsStaleAtMutate_RunsNoBatchRecompute()
        {
            var r = StaTestRunner.Run(() =>
            {
                var window = WithSourceAt(1, height: 20.0);
                S.SetPostTargets(window, 2);
                var afterDialog = -1;
                using (SelectiveCabeceraHeightPrompt.Substitute(_ =>
                {
                    window.Session.Recompute.Request(); // algo recalcula mientras el dialogo esta abierto
                    afterDialog = window.RecomputeCount;
                    return true;
                }))
                {
                    S.Apply(window);
                }

                return (Outcome: window.LastHeaderBatchOutcome, BatchRecomputes: window.RecomputeCount - afterDialog);
            });

            var rejected = Assert.IsType<HeaderBatchOutcome<SelectiveHeaderAddress>.Rejected>(r.Outcome);
            Assert.Equal(HeaderRejectionCode.StaleTargets, rejected.Code);
            Assert.Equal(0, r.BatchRecomputes);
        }

        [Fact]
        public void S28_ACommittedEdit_RunsExactlyOneRecompute()
        {
            var r = StaTestRunner.Run(() =>
            {
                var window = SelectiveWindowTestSupport.Open();
                S.SelectPost(window, 2);
                var height = window.CustomizeSeedHeightForTest(1) + 12.0;
                window.HeaderConfiguratorPresenter = S.EditIncrementally(configuration => configuration.Height = height);
                return S.WithShownWindow(window, () =>
                {
                    var before = window.RecomputeCount;
                    using (Prompts())
                    {
                        EditorWindowTestSupport.ClickNamed(window, "CustomizePostButton");
                    }

                    return (Outcome: window.LastHeaderBatchOutcome, Recomputes: window.RecomputeCount - before,
                        Height: window.EditorState.CabeceraAt(0, 1)?.Height ?? 0.0, Expected: height);
                });
            });

            Assert.IsType<HeaderBatchOutcome<SelectiveHeaderAddress>.Committed>(r.Outcome);
            Assert.Equal(1, r.Recomputes);
            Assert.Equal(r.Expected, r.Height);
        }

        // =============================================================================================================
        // S-32 — RR-01: PREPARE solo lee una resolucion vigente; C4 fuera del ambito del lote; stale entre PREPARE y MUTATE
        // =============================================================================================================

        [Fact]
        public void S32_WithARecomputePending_TheGestureEndsBeforeAnyPlan()
        {
            var r = StaTestRunner.Run(() =>
            {
                var window = WithSourceAt(1);
                S.SetPostTargets(window, 2);
                var rows = S.CabeceraRows(window);
                string[] log;
                HeaderBatchOutcome<SelectiveHeaderAddress> outcome;
                using (window.Session.Recompute.Defer())
                {
                    window.Session.Recompute.Request(); // recompute PENDIENTE
                    using (Prompts())
                    {
                        S.Apply(window);
                    }

                    log = window.HeaderBatchLog.ToArray();
                    outcome = window.LastHeaderBatchOutcome;
                }

                return (Log: log, Outcome: outcome, Same: rows == S.CabeceraRows(window));
            });

            Assert.Contains("fin:ResolutionNotCurrent", r.Log);
            Assert.DoesNotContain(r.Log, entry => entry.StartsWith("plan:", StringComparison.Ordinal));
            Assert.Null(r.Outcome);
            Assert.True(r.Same);
        }

        [Fact]
        public void S32_WithADeferredScopeOpen_TheGestureEndsBeforeAnyPlan()
        {
            var r = StaTestRunner.Run(() =>
            {
                var window = WithSourceAt(1);
                S.SetPostTargets(window, 2);
                var rows = S.CabeceraRows(window);
                string[] log;
                HeaderBatchOutcome<SelectiveHeaderAddress> outcome;
                using (window.Session.Recompute.Defer()) // ambito diferido ABIERTO, sin nada pendiente
                {
                    using (Prompts())
                    {
                        S.Apply(window);
                    }

                    log = window.HeaderBatchLog.ToArray();
                    outcome = window.LastHeaderBatchOutcome;
                }

                return (Log: log, Outcome: outcome, Same: rows == S.CabeceraRows(window));
            });

            Assert.Contains("fin:ResolutionNotCurrent", r.Log);
            Assert.DoesNotContain(r.Log, entry => entry.StartsWith("plan:", StringComparison.Ordinal));
            Assert.Null(r.Outcome);
            Assert.True(r.Same);
        }

        [Fact]
        public void S32_WithNoResolvedSystem_TheGestureEndsBeforeAnyPlan()
        {
            var r = StaTestRunner.Run(() =>
            {
                var window = WithSourceAt(1);
                S.SetPostTargets(window, 2);
                EditorWindowTestSupport.SetText(window, "PostPeralteBox", "0");
                SelectiveWindowTestSupport.RaiseLostFocus(window, "PostPeralteBox"); // el build falla: lastSystem = null
                var rows = S.CabeceraRows(window);
                using (Prompts())
                {
                    S.Apply(window);
                }

                return (Log: window.HeaderBatchLog.ToArray(), Outcome: window.LastHeaderBatchOutcome, Same: rows == S.CabeceraRows(window));
            });

            Assert.Contains("fin:NoResolvedSystem", r.Log);
            Assert.DoesNotContain(r.Log, entry => entry.StartsWith("plan:", StringComparison.Ordinal));
            Assert.Null(r.Outcome);
            Assert.True(r.Same);
        }

        [Fact]
        public void S32_TheEditingBoundaryRunsOutsideTheBatchScope_SoPrepareReadsACurrentResolution()
        {
            var r = StaTestRunner.Run(() =>
            {
                var window = WithSourceAt(1);
                S.SetPostTargets(window, 2);
                EditorWindowTestSupport.SetText(window, "FondoBox", "60"); // pendiente valido: C4 recalcula ANTES de PREPARE
                using (Prompts())
                {
                    S.Apply(window);
                }

                return (Log: window.HeaderBatchLog.ToArray(), Outcome: window.LastHeaderBatchOutcome);
            });

            Assert.Contains(r.Log, entry => entry.StartsWith("precondiciones(diferido=False,pendiente=False,sistema=True", StringComparison.Ordinal));
            Assert.Contains(r.Log, entry => entry.StartsWith("mutate(diferido=True", StringComparison.Ordinal));
            Assert.IsType<HeaderBatchOutcome<SelectiveHeaderAddress>.Committed>(r.Outcome);
        }

        [Fact]
        public void S32_ARecomputeBetweenPrepareAndMutate_IsRejectedAsStale_WithZeroMutation()
        {
            var r = StaTestRunner.Run(() =>
            {
                var window = WithSourceAt(1, height: 20.0);
                S.SetPostTargets(window, 2);
                var rows = S.CabeceraRows(window);
                using (SelectiveCabeceraHeightPrompt.Substitute(_ =>
                {
                    window.Session.Recompute.Request();
                    return true;
                }))
                {
                    S.Apply(window);
                }

                return (Log: window.HeaderBatchLog.ToArray(), Outcome: window.LastHeaderBatchOutcome, Same: rows == S.CabeceraRows(window));
            });

            var rejected = Assert.IsType<HeaderBatchOutcome<SelectiveHeaderAddress>.Rejected>(r.Outcome);
            Assert.Equal(HeaderRejectionCode.StaleTargets, rejected.Code);
            Assert.True(r.Same);
            var asked = Array.IndexOf(r.Log, "confirmacion:pedida");
            var stale = Array.IndexOf(r.Log, "outcome:Rejected(StaleTargets)");
            Assert.True(asked >= 0 && stale > asked, "La firma se verifica en MUTATE, despues de la confirmacion: " + string.Join(" | ", r.Log));
        }

        // =============================================================================================================
        // L-7 — el EDIT escribe el peralte del poste SOLO si queda aplicado, y lo escribe Application
        // =============================================================================================================

        [Fact]
        public void L7_AnEditWhoseTargetsAreAllOmitted_LeavesThePostPeralteUntouched()
        {
            var r = StaTestRunner.Run(() =>
            {
                var window = SelectiveWindowTestSupport.Open(2);
                S.SetFrentesOfFondo(window, 1, 3); // postes 0..3
                S.SetFrentesOfFondo(window, 2, 1); // postes 0..1
                S.ShowFondo(window, 1);
                S.SelectPost(window, 4);
                SelectiveTargetsTestSupport.SetTargets(window, 2); // solo el fondo 2, que NO tiene el poste 4
                window.EditorState.SyncPostCabeceras();
                var peralte = window.EditorState.PostPeraltes[3];
                var rows = S.CabeceraRows(window);

                var configuration = S.Recipe(window, window.CustomizeSeedHeightForTest(3));
                configuration.PostPeralte = 7.0;
                using (Prompts())
                {
                    window.ApplyCustomizedCabeceraForTest(3, configuration, 0.0);
                }

                return (Before: peralte, After: window.EditorState.PostPeraltes[3], Same: rows == S.CabeceraRows(window),
                    Outcome: window.LastHeaderBatchOutcome);
            });

            Assert.Equal(r.Before, r.After);
            Assert.True(r.Same);
            var rejected = Assert.IsType<HeaderBatchOutcome<SelectiveHeaderAddress>.Rejected>(r.Outcome);
            Assert.Equal(HeaderRejectionCode.NoApplicableTargets, rejected.Code);
        }

        [Fact]
        public void L7_ACommittedEdit_WritesThePostPeralteByTheApplicationRule()
        {
            var r = StaTestRunner.Run(() =>
            {
                var window = SelectiveWindowTestSupport.Open();
                var own = S.Recipe(window, window.CustomizeSeedHeightForTest(1));
                own.PostPeralte = 7.0;
                using (Prompts())
                {
                    window.ApplyCustomizedCabeceraForTest(1, own, 0.0);
                }

                var outcomeOwn = window.LastHeaderBatchOutcome;
                var peralteOwn = window.EditorState.PostPeraltes[1];

                var run = S.Recipe(window, window.CustomizeSeedHeightForTest(2));
                run.PostPeralte = 3.0; // igual al peralte del tramo: hereda
                using (Prompts())
                {
                    window.ApplyCustomizedCabeceraForTest(2, run, 0.0);
                }

                return (OutcomeOwn: outcomeOwn, PeralteOwn: peralteOwn, OutcomeRun: window.LastHeaderBatchOutcome,
                    PeralteRun: window.EditorState.PostPeraltes[2]);
            });

            Assert.IsType<HeaderBatchOutcome<SelectiveHeaderAddress>.Committed>(r.OutcomeOwn);
            Assert.Equal(7.0, r.PeralteOwn);
            Assert.IsType<HeaderBatchOutcome<SelectiveHeaderAddress>.Committed>(r.OutcomeRun);
            Assert.Equal(0.0, r.PeralteRun);
        }
    }
}
