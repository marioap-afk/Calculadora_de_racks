using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using RackCad.Application.Catalogs;
using RackCad.Domain.Systems.Selective;
using RackCad.UI;
using Xunit;

namespace RackCad.UI.Tests
{
    /// <summary>
    /// I-46 (G5.1) — the ONE hop G5 could not reach: pressing the tope's real "Configurar…" button, changing the side
    /// in the real grid dialog, and watching that choice travel through <c>EditTope</c> into the row and out of
    /// <see cref="SelectiveSafetyWindow.Result"/>. G5 covered both ENDS of this chain with real components but not the
    /// seven assignment lines in between, because the dialog was created inline with <c>ShowDialog</c>.
    /// <para>
    /// The <c>TopeDialog</c> seam replaces the MODALIZATION and nothing else: the window handed to the lambda is the
    /// real one, built by production with the row's own arguments. The test never writes the row's state — it drives
    /// the dialog's side selector and lets <c>EditTope</c> do the writing, which is the whole point.
    /// </para>
    /// </summary>
    public sealed class SelectiveTopeSideEditTopeChainTests
    {
        private const string TopeId = "POSTE_3_1_5_8_TOPE";

        private static RackCatalog Catalog => JsonRackCatalogProvider.FromBaseDirectory().Load();

        private static SelectiveSafetySelection Tope(SafetySide side)
            => new SelectiveSafetySelection
            {
                ElementId = TopeId, Quantity = 1, Side = side, TopeShared = false, TopeFondo = -1, TopeSaque = 6.0
            };

        private static SelectiveSafetyWindow Window(SelectiveSafetySelection current, int fondoCount)
            => new SelectiveSafetyWindow(
                Catalog?.SafetyElements ?? new List<SafetyElementCatalogEntry>(),
                new List<SelectiveSafetySelection> { current },
                postCount: 3,
                levelsPerFrente: new[] { 2, 2 },
                fondoCount: fondoCount,
                parrillaPlan: null,
                catalog: Catalog,
                resolvedSystem: null);

        /// <summary>The tope family's own "Configurar…" button, told apart by the tooltip only it carries.</summary>
        private static Button TopeButton(SelectiveSafetyWindow window)
            => SafetyDialogTestSupport.Descendants(window)
                .OfType<Button>()
                .Single(button => (button.ToolTip as string ?? string.Empty).StartsWith("Tope trasero:"));

        /// <summary>The grid dialog's side selector — the only combo that offers the four sides.</summary>
        private static ComboBox SideBox(SafetyTopeGridWindow dialog)
            => SafetyDialogTestSupport.Descendants(dialog)
                .OfType<ComboBox>()
                .Single(box => box.Items.Contains("Izquierda"));

        /// <summary>
        /// Runs the dialog's REAL accept path from inside the seam. Its <c>OnOk</c> does exactly two things —
        /// <c>Result = BuildResult()</c> and <c>DialogResult = true</c> — and the second throws on a window that was
        /// never shown modally, which is precisely what the seam exists to replace. So this performs the first (with
        /// the real <c>BuildResult</c>, which reads the live combo, checkbox, SAQUE and grid) and skips the second.
        /// </summary>
        private static bool? Accept(SafetyTopeGridWindow dialog)
        {
            var built = dialog.BuildResult();
            if (built == null)
            {
                return false; // invalid SAQUE; production would keep the dialog open
            }

            typeof(SafetyTopeGridWindow)
                .GetProperty(nameof(SafetyTopeGridWindow.Result))
                .GetSetMethod(nonPublic: true)
                .Invoke(dialog, new object[] { built });
            return true;
        }

        /// <summary>Presses the real tope button on a SHOWN window. Production parents the grid dialog with
        /// <c>Owner = this</c>, and WPF refuses that on a window that was never shown, so the host is displayed
        /// off-screen for the gesture and closed after — the same idiom the Push Back safety-flow tests use.</summary>
        private static void ClickTope(SelectiveSafetyWindow window)
        {
            window.WindowStartupLocation = WindowStartupLocation.Manual;
            window.ShowInTaskbar = false;
            window.Left = -10000;
            window.Top = -10000;
            window.Show();
            window.UpdateLayout();
            try
            {
                var button = TopeButton(window);
                button.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent, button));
            }
            finally
            {
                window.Close();
            }
        }

        /// <summary>Opens the safety window on <paramref name="from"/>, presses the real tope button, picks
        /// <paramref name="to"/> in the real dialog, accepts or cancels, and returns what the window commits — plus
        /// the side the dialog was SEEDED with, which proves the chain started from the row and not from a default.</summary>
        private static (SafetySide Committed, SafetySide Seeded) Transition(
            SafetySide from, SafetySide to, int fondoCount, bool accept)
            => StaTestRunner.Run(() =>
            {
                var window = Window(Tope(from), fondoCount);
                var seeded = SafetySide.None;

                window.TopeDialog = dialog =>
                {
                    seeded = SideBox(dialog).SelectedIndex >= 0
                        ? (SafetySide)SideBox(dialog).SelectedIndex
                        : SafetySide.None;
                    SideBox(dialog).SelectedIndex = (int)to; // the user picks another entry
                    return accept ? Accept(dialog) : false;
                };

                ClickTope(window);

                window.BuildResultForTest();
                return (window.Result.Single(selection => selection.ElementId == TopeId).Side, seeded);
            });

        // ---- Accepting propagates the newly picked side all the way to the window's Result ----
        [Theory]
        [InlineData(SafetySide.Left, SafetySide.Right, 1)]
        [InlineData(SafetySide.Left, SafetySide.Right, 3)]
        [InlineData(SafetySide.Right, SafetySide.Both, 1)]
        [InlineData(SafetySide.Right, SafetySide.Both, 3)]
        [InlineData(SafetySide.Both, SafetySide.None, 1)]
        [InlineData(SafetySide.Both, SafetySide.None, 3)]
        public void ChangingTheSide_ReachesTheBuiltSelection(SafetySide from, SafetySide to, int fondoCount)
        {
            var (committed, seeded) = Transition(from, to, fondoCount, accept: true);

            Assert.Equal(from, seeded);   // the dialog opened on the row's current side...
            Assert.Equal(to, committed);  // ...and the new one reached the built selection
        }

        // ---- Cancelling leaves the original side untouched, however far the user got in the dialog ----
        [Theory]
        [InlineData(SafetySide.Left, SafetySide.Right, 1)]
        [InlineData(SafetySide.Right, SafetySide.Both, 3)]
        [InlineData(SafetySide.Both, SafetySide.None, 1)]
        [InlineData(SafetySide.None, SafetySide.Both, 3)]
        public void CancellingKeepsTheOriginalSide(SafetySide from, SafetySide to, int fondoCount)
        {
            var (committed, seeded) = Transition(from, to, fondoCount, accept: false);

            Assert.Equal(from, seeded);
            Assert.Equal(from, committed); // the picked value is discarded with the dialog
        }

        // ---- The seam is the only thing replaced: production still owns which dialog is built and with what ----
        [Fact]
        public void TheSeamReceivesTheRealDialog_SeededFromTheRow()
        {
            var seen = StaTestRunner.Run(() =>
            {
                var window = Window(Tope(SafetySide.Right), fondoCount: 3);
                SafetyTopeGridWindow captured = null;

                window.TopeDialog = dialog =>
                {
                    captured = dialog;
                    return false; // cancel: this test only inspects what production handed over
                };

                ClickTope(window);

                return (
                    isReal: captured != null,
                    side: captured == null ? -1 : SideBox(captured).SelectedIndex,
                    saque: captured?.BuildResult()?.Saque ?? 0.0,
                    owner: ReferenceEquals(captured?.Owner, window));
            });

            Assert.True(seen.isReal);
            Assert.Equal((int)SafetySide.Right, seen.side); // seeded from the row, not from a default
            Assert.Equal(6.0, seen.saque, 6);               // ...and so is every other argument
            Assert.True(seen.owner);                        // it is the window production built, parented as production does
        }
    }
}
