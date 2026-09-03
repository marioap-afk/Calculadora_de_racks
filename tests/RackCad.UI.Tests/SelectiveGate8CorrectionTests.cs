using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using RackCad.Application.RackFrames;
using RackCad.Application.Settings;
using RackCad.Application.Systems.Selective;
using RackCad.Domain.RackFrames;
using RackCad.UI.Systems.Selective;
using Xunit;

namespace RackCad.UI.Tests
{
    /// <summary>
    /// STA tests for the REAL <see cref="RackSelectiveWindow"/> after the gate-8 Owner correction: the single
    /// "Fondos destino" selector sits in the left panel right before "Tramo", it defaults to "Todos" and is remembered
    /// between openings, the cabecera status always describes the fondo ON SCREEN, and "Personalizar" seeds from that
    /// fondo's cabecera depth instead of fondo 0's.
    /// </summary>
    public sealed class SelectiveGate8CorrectionTests
    {
        /// <summary>An in-memory stand-in for <c>%APPDATA%\RackCad\settings.json</c>, so the remembered preference is
        /// deterministic and the developer's real settings are never read or written.</summary>
        private sealed class FakeSettings : IUserSettingsGateway
        {
            public FakeSettings(string stored = null) => Stored = new UserSettings { SelectiveTargetFondos = stored };

            public UserSettings Stored { get; private set; }

            public int Saves { get; private set; }

            public UserSettings Load() => Stored;

            public void Save(UserSettings settings)
            {
                Stored = settings;
                Saves++;
            }
        }

        private static RackSelectiveWindow Open(int fondos, IUserSettingsGateway gateway = null)
        {
            var window = new RackSelectiveWindow(canInsertInAutoCad: false, gateway ?? new FakeSettings());
            if (fondos > 1)
            {
                EditorWindowTestSupport.SetText(window, "FondosBox", fondos.ToString());
                RaiseLostFocus(window, "FondosBox");
            }

            return window;
        }

        private static void RaiseLostFocus(Window window, string name)
        {
            var box = (TextBox)window.FindName(name);
            box.RaiseEvent(new RoutedEventArgs(UIElement.LostFocusEvent, box));
        }

        /// <summary>Open a rack that ALREADY has <paramref name="fondos"/> fondos, the way RACKEDITAR and the design
        /// library do. A fresh editor starts on one fondo, so this is the only path where a remembered explicit set has
        /// fondos to survive onto.</summary>
        private static RackSelectiveWindow OpenExistingRack(int fondos, IUserSettingsGateway gateway)
        {
            var source = Open(fondos, new FakeSettings(stored: "Todos"));
            var document = RackCad.Application.Persistence.SelectivePalletDesignDocument.From(
                source.BuildDesignForTest(out _), "rack-" + fondos, "Rack " + fondos);

            var window = new RackSelectiveWindow(canInsertInAutoCad: false, gateway);
            window.LoadForNew(document);
            return window;
        }

        /// <summary>Write the pallet depth of ONE fondo. The target has to be narrowed first: with the new default
        /// ("Todos") a depth typed into the box lands on every fondo, which is the whole point of the axis.</summary>
        private static void SetDepthOfFondo(RackSelectiveWindow window, int oneBased, double pallet)
        {
            ((ComboBox)window.FindName("FondoSelectorBox")).SelectedIndex = oneBased - 1;
            SelectiveTargetsTestSupport.SetTargets(window, oneBased);
            EditorWindowTestSupport.SetText(window, "FondoBox", pallet.ToString(System.Globalization.CultureInfo.InvariantCulture));
            RaiseLostFocus(window, "FondoBox");
        }

        // =====================================================================
        // 1. The selector is ONE control, in the left panel, just before "Tramo"
        // =====================================================================

        [Fact]
        public void ThereIsExactlyOneTargetFondosSelector_AndItLivesInTheLeftPanel()
        {
            var (panels, buttons) = StaTestRunner.Run(() =>
            {
                var window = Open(3);
                var all = EditorWindowTestSupport.Descendants(window).OfType<FrameworkElement>().ToList();
                return (
                    all.Count(e => e.Name == "TargetFondosPanel"),
                    all.Count(e => e.Name == "TargetFondosButton"));
            });

            Assert.Equal(1, panels);   // no second selector was introduced
            Assert.Equal(1, buttons);
        }

        [Fact]
        public void TheSelectorSitsImmediatelyBeforeTheTramoSection()
        {
            var order = StaTestRunner.Run(() =>
            {
                var window = Open(3);
                var panel = (StackPanel)((FrameworkElement)window.FindName("TargetFondosPanel")).Parent;
                var children = panel.Children.Cast<UIElement>().ToList();

                var selector = children.FindIndex(c => (c as FrameworkElement)?.Name == "TargetFondosPanel");
                var tramo = children.FindIndex(c => c is TextBlock text && text.Text == "Tramo");
                var reset = children.FindIndex(c => c is Button button && (button.Content as string) == "Restablecer poste");
                return (selector, tramo, reset);
            });

            Assert.True(order.Item1 >= 0 && order.Item2 >= 0 && order.Item3 >= 0);
            Assert.Equal(order.Item1 + 1, order.Item2);   // "Fondos destino" is the row right before "Tramo"
            Assert.True(order.Item3 < order.Item1);       // and it comes after "Restablecer poste"
        }

        [Fact]
        public void EveryOperationStillReadsTheSameTargetFondos()
        {
            // One selector, one axis: a cell edit, a frente edit and a cabecera edit all land on the same set.
            var (cells, rises, cabeceras) = StaTestRunner.Run(() =>
            {
                var window = Open(3);
                SelectiveTargetsTestSupport.SetTargets(window, 1, 3);
                var state = window.EditorState;

                state.ApplyToTargets(SelectiveApplyScope.All, new SelectiveEditorCell { Frente = 55.0 });
                state.ApplyFloorBeamRiseToTargets(SelectiveFrontApplyScope.All, 0, 15.0);
                state.ApplyCabeceraToTargets(0, Custom(window, 250.0), c => c);

                return (
                    Enumerable.Range(0, 3).Select(k => CellsOf(state, k).All(c => c.Frente == 55.0)).ToArray(),
                    Enumerable.Range(0, 3).Select(k => state.FloorBeamRiseOverrideAt(k, 0)).ToArray(),
                    Enumerable.Range(0, 3).Select(k => state.CabeceraAt(k, 0) != null).ToArray());
            });

            Assert.Equal(new[] { true, false, true }, cells);
            Assert.Equal(new double?[] { 15.0, null, 15.0 }, rises);
            Assert.Equal(new[] { true, false, true }, cabeceras);
        }

        // ==============================================
        // 2. The default is "Todos", not "Actual"
        // ==============================================

        [Fact]
        public void WithNoStoredPreference_TheEditorOpensOnTodos()
        {
            var (mode, caption, targets) = StaTestRunner.Run(() =>
            {
                var window = Open(3, new FakeSettings(stored: null));
                return (window.EditorState.TargetMode, SelectiveTargetsTestSupport.Caption(window), window.EditorState.TargetFondos.Fondos.ToArray());
            });

            Assert.Equal(SelectiveTargetMode.All, mode);
            Assert.Equal("Todos", caption);
            Assert.Equal(new[] { 0, 1, 2 }, targets);
        }

        [Fact]
        public void TheDefaultTodos_ExpandsWhenTheRackGrows()
        {
            // It is a MODE: growing the rack from 1 to 4 fondos must bring the new ones in, not leave the editor
            // aimed at the single fondo that existed when the window opened.
            var targets = StaTestRunner.Run(() =>
            {
                var window = Open(1, new FakeSettings(stored: null));
                EditorWindowTestSupport.SetText(window, "FondosBox", "4");
                RaiseLostFocus(window, "FondosBox");
                return window.EditorState.TargetFondos.Fondos.ToArray();
            });

            Assert.Equal(new[] { 0, 1, 2, 3 }, targets);
        }

        [Fact]
        public void NavigatingBetweenFondos_DoesNotNarrowTheDefaultTodos()
        {
            // The counterpart of "Actual follows the visible fondo": "Todos" does not depend on what is on screen,
            // so walking the fondo selector must leave every fondo targeted.
            var (targets, caption) = StaTestRunner.Run(() =>
            {
                var window = Open(3, new FakeSettings(stored: null));
                ((ComboBox)window.FindName("FondoSelectorBox")).SelectedIndex = 2;
                return (window.EditorState.TargetFondos.Fondos.ToArray(), SelectiveTargetsTestSupport.Caption(window));
            });

            Assert.Equal(new[] { 0, 1, 2 }, targets);
            Assert.Equal("Todos", caption);
        }

        [Fact]
        public void WithASingleFondo_TodosIsThatFondoAndTheControlStaysHidden()
        {
            var (targets, visibility) = StaTestRunner.Run(() =>
            {
                var window = Open(1, new FakeSettings(stored: null));
                return (
                    window.EditorState.TargetFondos.Fondos.ToArray(),
                    ((FrameworkElement)window.FindName("TargetFondosPanel")).Visibility);
            });

            Assert.Equal(new[] { 0 }, targets);
            Assert.Equal(Visibility.Collapsed, visibility); // nothing to choose between
        }

        // ==================================================
        // 3. The choice is remembered between openings
        // ==================================================

        [Fact]
        public void ChoosingActual_IsRemembered_AndComesBackAsActual()
        {
            var (stored, reopened, caption) = StaTestRunner.Run(() =>
            {
                var gateway = new FakeSettings(stored: null);
                var first = Open(3, gateway);
                SelectiveTargetsTestSupport.SetCurrentTarget(first);
                var written = gateway.Stored.SelectiveTargetFondos;

                var second = Open(3, gateway); // "next time the user opens the editor"
                return (written, second.EditorState.TargetMode, SelectiveTargetsTestSupport.Caption(second));
            });

            Assert.Equal("Actual", stored);
            Assert.Equal(SelectiveTargetMode.FollowCurrent, reopened);
            Assert.Equal("Actual", caption);
        }

        [Fact]
        public void ChoosingAnExplicitSet_IsRemembered_AndComesBack()
        {
            var (stored, targets, mode) = StaTestRunner.Run(() =>
            {
                var gateway = new FakeSettings(stored: null);
                var first = Open(4, gateway);
                SelectiveTargetsTestSupport.SetTargets(first, 2, 4); // one-based in the UI
                var written = gateway.Stored.SelectiveTargetFondos;

                var second = OpenExistingRack(4, gateway); // "next time the user opens a 4-fondo rack"
                return (written, second.EditorState.TargetFondos.Fondos.ToArray(), second.EditorState.TargetMode);
            });

            Assert.Equal("1,3", stored); // stored 0-based, as the model indexes them
            Assert.Equal(new[] { 1, 3 }, targets);
            Assert.Equal(SelectiveTargetMode.Explicit, mode);
        }

        [Fact]
        public void ChoosingTodos_IsRememberedAsTheIntent_AndCoversTheNextRacksFondos()
        {
            // Storing the two indices it resolved to would leave the next, bigger rack partly out of "todos".
            var (stored, targets, caption) = StaTestRunner.Run(() =>
            {
                var gateway = new FakeSettings(stored: "Actual");
                var first = Open(2, gateway);
                SelectiveTargetsTestSupport.SetAllTargets(first);
                var written = gateway.Stored.SelectiveTargetFondos;

                var second = OpenExistingRack(4, gateway);
                return (written, second.EditorState.TargetFondos.Fondos.ToArray(), SelectiveTargetsTestSupport.Caption(second));
            });

            Assert.Equal("Todos", stored);
            Assert.Equal(new[] { 0, 1, 2, 3 }, targets);
            Assert.Equal("Todos", caption);
        }

        [Fact]
        public void AnExplicitPreference_KeepsOnlyTheFondosTheNewRackHas()
        {
            var (targets, mode) = StaTestRunner.Run(() =>
            {
                var window = OpenExistingRack(2, new FakeSettings(stored: "1,3")); // a 2-fondo rack has no fondo index 3
                return (window.EditorState.TargetFondos.Fondos.ToArray(), window.EditorState.TargetMode);
            });

            Assert.Equal(new[] { 1 }, targets);
            Assert.Equal(SelectiveTargetMode.Explicit, mode);
        }

        [Fact]
        public void APreferenceEntirelyOutOfRange_FallsBackToTodos()
        {
            var (targets, mode, caption) = StaTestRunner.Run(() =>
            {
                var window = OpenExistingRack(2, new FakeSettings(stored: "7,9"));
                return (window.EditorState.TargetFondos.Fondos.ToArray(), window.EditorState.TargetMode, SelectiveTargetsTestSupport.Caption(window));
            });

            Assert.Equal(new[] { 0, 1 }, targets);
            Assert.Equal(SelectiveTargetMode.All, mode);
            Assert.Equal("Todos", caption);
        }

        [Fact]
        public void ThePreferenceNeverTouchesTheDesign()
        {
            // It is an editor preference: two racks saved with different "Fondos destino" must serialize identically.
            var (a, b) = StaTestRunner.Run(() =>
            {
                var first = Open(3, new FakeSettings(stored: "Todos"));
                var second = Open(3, new FakeSettings(stored: "1"));
                var store = new RackCad.Application.Persistence.SelectivePalletDesignStore();
                return (
                    store.Serialize(RackCad.Application.Persistence.SelectivePalletDesignDocument.From(first.BuildDesignForTest(out _), "id", "Rack")),
                    store.Serialize(RackCad.Application.Persistence.SelectivePalletDesignDocument.From(second.BuildDesignForTest(out _), "id", "Rack")));
            });

            Assert.Equal(a, b);
        }

        // ==========================================================
        // 4. The text gate 8A made false is gone
        // ==========================================================

        [Fact]
        public void TheObsoleteMatrixHeaderHintIsGone()
        {
            var texts = StaTestRunner.Run(() =>
                EditorWindowTestSupport.Descendants(Open(2)).OfType<TextBlock>().Select(t => t.Text ?? string.Empty).ToList());

            Assert.DoesNotContain(texts, t => t.Contains("encabezado de la matriz"));
            Assert.DoesNotContain(texts, t => t.Contains("El piso arranca en 0"));
            Assert.DoesNotContain(texts, t => t.Contains("la altura y 'larguero a piso'"));
        }

        // ==========================================================================
        // 5. The cabecera status ALWAYS describes (fondo visible, poste seleccionado)
        // ==========================================================================

        [Fact]
        public void NavigatingF1ToF2ToF3_MovesTheStatusAndTheData_NeverStickingOnFondoOne()
        {
            // The Owner's exact gesture: poste 1 custom on fondo 1 only, then the "Editando fondo" combo walked
            // F1 -> F2 -> F3. Hard-coding fondo 0 anywhere in the status or the lookup fails this.
            var (statuses, customs, visibleDepths) = StaTestRunner.Run(() =>
            {
                var window = Open(3, new FakeSettings(stored: null));

                // Give each fondo its own pallet depth, so "the matrix really changed fondo" is observable.
                SetDepthOfFondo(window, 1, 48.0);
                SetDepthOfFondo(window, 2, 60.0);
                SetDepthOfFondo(window, 3, 72.0);

                var combo = (ComboBox)window.FindName("FondoSelectorBox");
                combo.SelectedIndex = 0;
                SelectiveTargetsTestSupport.SetTargets(window, 1); // only fondo 1 gets the custom
                window.EditorState.ApplyCabeceraToTargets(0, Custom(window, 250.0), c => c);
                window.RefreshPostStatusForTest();

                var status = (TextBlock)window.FindName("PostCabeceraStatus");
                var seenStatus = new List<string>();
                var seenCustom = new List<bool>();
                var seenDepth = new List<string>();

                for (var k = 0; k < 3; k++)
                {
                    combo.SelectedIndex = k;
                    seenStatus.Add(status.Text);
                    seenCustom.Add(window.EditorState.CabeceraAt(window.EditorState.SelectedFondo, 0) != null);
                    seenDepth.Add(((TextBox)window.FindName("FondoBox")).Text);
                }

                return (seenStatus, seenCustom, seenDepth);
            });

            Assert.Equal("Personalizada · fondo 1", statuses[0]);
            Assert.Equal("Por defecto (del tramo) · fondo 2", statuses[1]);
            Assert.Equal("Por defecto (del tramo) · fondo 3", statuses[2]);
            Assert.Equal(new[] { true, false, false }, customs);          // status and data agree on the same fondo
            Assert.Equal(new[] { "48", "60", "72" }, visibleDepths);      // the matrix really moved to each fondo
        }

        [Fact]
        public void CustomizingTheThirdFondoAfterwards_TurnsItsOwnStatusCustom()
        {
            var statuses = StaTestRunner.Run(() =>
            {
                var window = Open(3, new FakeSettings(stored: null));
                var combo = (ComboBox)window.FindName("FondoSelectorBox");
                var status = (TextBlock)window.FindName("PostCabeceraStatus");

                combo.SelectedIndex = 2;                             // stand on fondo 3
                SelectiveTargetsTestSupport.SetTargets(window, 3);   // and aim at it
                window.EditorState.ApplyCabeceraToTargets(0, Custom(window, 260.0), c => c);
                window.RefreshPostStatusForTest();
                var onThree = status.Text;

                combo.SelectedIndex = 0;
                var backOnOne = status.Text;
                return new[] { onThree, backOnOne };
            });

            Assert.Equal("Personalizada · fondo 3", statuses[0]);
            Assert.Equal("Por defecto (del tramo) · fondo 1", statuses[1]);
        }

        [Fact]
        public void TheStatusFollowsThePostToo_OnTheVisibleFondo()
        {
            var statuses = StaTestRunner.Run(() =>
            {
                var window = Open(2, new FakeSettings(stored: null));
                SelectiveTargetsTestSupport.SetTargets(window, 2);
                window.EditorState.ApplyCabeceraToTargets(1, Custom(window, 250.0), c => c); // poste 2 of fondo 2

                var posts = (ComboBox)window.FindName("PostSelectBox");
                var combo = (ComboBox)window.FindName("FondoSelectorBox");
                var status = (TextBlock)window.FindName("PostCabeceraStatus");

                combo.SelectedIndex = 1; // fondo 2
                posts.SelectedIndex = 1; // poste 2 -> custom here
                var customPair = status.Text;
                posts.SelectedIndex = 0; // poste 1 -> standard on the same fondo
                var standardPair = status.Text;
                return new[] { customPair, standardPair };
            });

            Assert.Equal("Personalizada · fondo 2", statuses[0]);
            Assert.Equal("Por defecto (del tramo) · fondo 2", statuses[1]);
        }

        // ================================================================
        // 6. "Personalizar" seeds from the VISIBLE fondo's cabecera depth
        // ================================================================

        [Fact]
        public void TheCustomizeSeedUsesTheVisibleFondosCabeceraDepth_NotFondoZeros()
        {
            // Tarimas 48 / 60 / 72 -> cabeceras 42 / 54 / 66. Reading fondo 0 would answer 42 on all three.
            var seeds = StaTestRunner.Run(() =>
            {
                var window = Open(3, new FakeSettings(stored: null));
                SetDepthOfFondo(window, 1, 48.0);
                SetDepthOfFondo(window, 2, 60.0);
                SetDepthOfFondo(window, 3, 72.0);

                var combo = (ComboBox)window.FindName("FondoSelectorBox");
                var seen = new List<double>();
                for (var k = 0; k < 3; k++)
                {
                    combo.SelectedIndex = k;
                    seen.Add(window.CustomizeSeedDepthForTest());
                }

                return seen;
            });

            Assert.Equal(new[] { 42.0, 54.0, 66.0 }, seeds);
        }

        [Fact]
        public void AnExplicitCabeceraOverrideWins_OverTarimaMinusSix()
        {
            var seed = StaTestRunner.Run(() =>
            {
                var window = Open(2, new FakeSettings(stored: null));
                SetDepthOfFondo(window, 2, 60.0);                           // would derive 54
                EditorWindowTestSupport.SetText(window, "CabeceraFondoBox", "50");
                RaiseLostFocus(window, "CabeceraFondoBox");
                return window.CustomizeSeedDepthForTest();
            });

            Assert.Equal(50.0, seed); // the fondo's own override, not the derived value
        }

        [Fact]
        public void TheSeedUsesTheDepthJustTypedEvenBeforeItIsCommittedElsewhere()
        {
            // CabeceraDepthOfFondo reads the fondo's SLOT, so the seed must commit the working matrix first.
            var seed = StaTestRunner.Run(() =>
            {
                var window = Open(2, new FakeSettings(stored: null));
                SelectiveTargetsTestSupport.SetTargets(window, 1);
                ((TextBox)window.FindName("FondoBox")).Text = "72"; // typed, never lost focus
                return window.CustomizeSeedDepthForTest();
            });

            Assert.Equal(66.0, seed);
        }

        [Fact]
        public void ACabeceraAppliedOverTwoFondosOfDifferentDepth_TakesEachFondosOwnDepth()
        {
            // The seed decides what the user SEES; the authority still imposes each target's own depth on accept.
            var (depths, independent) = StaTestRunner.Run(() =>
            {
                var window = Open(3, new FakeSettings(stored: null));
                SetDepthOfFondo(window, 1, 48.0);
                SetDepthOfFondo(window, 2, 60.0);
                SetDepthOfFondo(window, 3, 72.0);

                ((ComboBox)window.FindName("FondoSelectorBox")).SelectedIndex = 0;
                SelectiveTargetsTestSupport.SetTargets(window, 1, 3);
                var state = window.EditorState;
                state.ApplyCabeceraToTargets(0, Custom(window, 250.0), c => new RackCad.Application.Persistence.RackFrameProjectStore().DeepCopy(c));

                var a = state.CabeceraAt(0, 0);
                var c3 = state.CabeceraAt(2, 0);
                a.Height = 999.0; // the copies must be independent
                return (
                    new[] { a.Depth, c3.Depth },
                    c3.Height);
            });

            Assert.Equal(new[] { 42.0, 66.0 }, depths);
            Assert.Equal(250.0, independent);
        }

        // ==========================================
        // 7. Cabecera x target fondos, integrated
        // ==========================================

        [Fact]
        public void SubsetThenNarrowThenReset_LeavesEachFondoWithItsOwnCabecera()
        {
            var (afterA, afterB, afterReset) = StaTestRunner.Run(() =>
            {
                var window = Open(3, new FakeSettings(stored: null));
                var state = window.EditorState;

                // A) targets {1,3}: poste 1 -> A
                SelectiveTargetsTestSupport.SetTargets(window, 1, 3);
                state.ApplyCabeceraToTargets(0, Custom(window, 250.0), Copy);
                var a = Heights(state);

                // B) targets {3}: poste 1 -> B. Fondo 1 keeps A, fondo 2 stays standard.
                SelectiveTargetsTestSupport.SetTargets(window, 3);
                state.ApplyCabeceraToTargets(0, Custom(window, 260.0), Copy);
                var b = Heights(state);

                // C) targets {1}: reset. Fondo 3 keeps B.
                SelectiveTargetsTestSupport.SetTargets(window, 1);
                state.ApplyCabeceraToTargets(0, null, Copy);
                return (a, b, Heights(state));
            });

            Assert.Equal(new double[] { 250.0, 0.0, 250.0 }, afterA);
            Assert.Equal(new double[] { 250.0, 0.0, 260.0 }, afterB);
            Assert.Equal(new double[] { 0.0, 0.0, 260.0 }, afterReset);
        }

        [Fact]
        public void ThePerFondoCabecerasSurviveASaveAndLoad()
        {
            var heights = StaTestRunner.Run(() =>
            {
                var window = Open(3, new FakeSettings(stored: null));
                var state = window.EditorState;
                SelectiveTargetsTestSupport.SetTargets(window, 1, 3);
                state.ApplyCabeceraToTargets(0, Custom(window, 250.0), Copy);
                SelectiveTargetsTestSupport.SetTargets(window, 3);
                state.ApplyCabeceraToTargets(0, Custom(window, 260.0), Copy);

                var store = new RackCad.Application.Persistence.SelectivePalletDesignStore();
                var reloaded = store
                    .Deserialize(store.Serialize(RackCad.Application.Persistence.SelectivePalletDesignDocument.From(window.BuildDesignForTest(out _), "id", "Rack")))
                    .ToDomain();
                return Enumerable.Range(0, 3)
                    .Select(k => SelectiveCabeceraAuthority.CustomAt(reloaded, k, 0)?.Height ?? 0.0)
                    .ToArray();
            });

            Assert.Equal(new double[] { 250.0, 0.0, 260.0 }, heights);
        }

        [Fact]
        public void ThePerPostPeralteStaysGlobal_AndIsNotUsedAsAPerFondoAuthority()
        {
            // Item 8 of the correction: "Peralte de este poste" is deliberately NOT per fondo. Locking that here
            // keeps a later change from quietly turning it into one.
            var peraltes = StaTestRunner.Run(() =>
            {
                var window = Open(3, new FakeSettings(stored: null));
                var state = window.EditorState;
                state.SyncPostCabeceras();
                SelectiveTargetsTestSupport.SetTargets(window, 2);
                state.PostPeraltes[0] = 7.0;

                state.ApplyCabeceraToTargets(0, Custom(window, 250.0), Copy);
                return state.PostPeraltes.ToArray();
            });

            Assert.Equal(7.0, peraltes[0]); // one global value per post, untouched by a per-fondo cabecera write
        }

        // ---- helpers ----

        private static RackFrameConfiguration Copy(RackFrameConfiguration configuration)
            => new RackCad.Application.Persistence.RackFrameProjectStore().DeepCopy(configuration);

        private static double[] Heights(SelectiveEditorState state)
            => Enumerable.Range(0, 3).Select(k => state.CabeceraAt(k, 0)?.Height ?? 0.0).ToArray();

        private static IEnumerable<SelectiveEditorCell> CellsOf(SelectiveEditorState state, int fondo)
            => (fondo == state.SelectedFondo ? state.Bays : state.FondoMatrices[fondo].Bays).SelectMany(column => column);

        private static RackFrameConfiguration Custom(RackSelectiveWindow window, double height)
            => new RackFrameConfigurationFactory(window.Session.Catalog).Build(
                RackFrameTemplateCatalog.FindStandardOrDefault(),
                "POSTE_OMEGA_ATORNILLABLE_CON_TROQUEL_GOTA_DE_AGUA",
                height,
                42.0);
    }
}
