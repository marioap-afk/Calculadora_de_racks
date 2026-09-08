using System.Linq;
using System.Windows.Controls;
using RackCad.Domain.Systems.Selective;
using RackCad.UI;
using Xunit;

namespace RackCad.UI.Tests
{
    /// <summary>
    /// I-46 (ID12) — the tope's side selector in the Selectivo. The family now has FOUR options and "Ninguno" is one
    /// of them, so the dialog must both offer it and give it back unchanged: before I-46 the list held three entries
    /// and <c>SelectiveSafetyWindow</c> rewrote a stored <c>None</c> into <c>Both</c> when it reopened the row, which
    /// made "no tope on this rack" impossible to express and impossible to keep.
    /// <para>
    /// The Push Back path shares this dialog and passes <c>showSharedAndSide: false</c>; the last test here is the
    /// sentinel that the extra option did not leak a side control into it.
    /// </para>
    /// </summary>
    public sealed class SelectiveTopeSideOptionsTests
    {
        private static SafetyTopeGridWindow Dialog(SafetySide side, bool showSharedAndSide = true)
            => new SafetyTopeGridWindow(
                "Tope", new[] { 2 }, shared: false, side: side, saque: 3.0, frontal: false,
                offCells: null, fondoCount: 1, fondo: -1, showSharedAndSide: showSharedAndSide);

        private static ComboBox SideBox(SafetyTopeGridWindow window)
            => SafetyDialogTestSupport.Descendants(window)
                .OfType<ComboBox>()
                .Single(box => box.Items.Contains("Izquierda"));

        // ---- The four options, in the order of the SafetySide ordinals ----
        [Fact]
        public void SideSelector_OffersExactlyTheFourOptions()
        {
            var items = StaTestRunner.Run(() => SideBox(Dialog(SafetySide.Both)).Items.Cast<string>().ToArray());

            Assert.Equal(new[] { "Ninguno", "Izquierda", "Derecha", "Ambas" }, items);
        }

        // ---- Every side survives the dialog untouched: opened on it, accepted, returned as itself ----
        [Theory]
        [InlineData(SafetySide.None)]
        [InlineData(SafetySide.Left)]
        [InlineData(SafetySide.Right)]
        [InlineData(SafetySide.Both)]
        public void EverySide_RoundTripsThroughTheDialog(SafetySide side)
        {
            var result = StaTestRunner.Run(() => Dialog(side).BuildResult().Side);

            Assert.Equal(side, result);
        }

        // ---- Picking each entry yields its own side; "Ninguno" is reachable, not just representable ----
        [Theory]
        [InlineData(0, SafetySide.None)]
        [InlineData(1, SafetySide.Left)]
        [InlineData(2, SafetySide.Right)]
        [InlineData(3, SafetySide.Both)]
        public void PickingAnEntry_YieldsItsOwnSide(int index, SafetySide expected)
        {
            var result = StaTestRunner.Run(() =>
            {
                var window = Dialog(SafetySide.Both);
                SideBox(window).SelectedIndex = index;
                return window.BuildResult().Side;
            });

            Assert.Equal(expected, result);
        }

        // ---- Reopening a row stored as "Ninguno" must NOT promote it to "Ambas" (the coercion I-46 removed) ----
        [Fact]
        public void ReopeningNinguno_KeepsNinguno()
        {
            var reopened = StaTestRunner.Run(() =>
            {
                var stored = Dialog(SafetySide.None).BuildResult().Side; // what the row would have persisted
                return Dialog(stored).BuildResult().Side;                // ...and what reopening it gives back
            });

            Assert.Equal(SafetySide.None, reopened);
        }

        // ---- Sentinel: the shared dialog still offers Push Back neither the side nor "Compartido" ----
        [Fact]
        public void PushBackPath_StillOffersNeitherSideNorCompartido()
        {
            var r = StaTestRunner.Run(() =>
            {
                var pushBack = Dialog(SafetySide.Left, showSharedAndSide: false);
                return (
                    side: SafetyDialogTestSupport.HasSideSelector(pushBack),
                    shared: SafetyDialogTestSupport.HasCheckBox(pushBack, "Compartido (uno central)"),
                    saque: SafetyDialogTestSupport.HasText(pushBack, "Saque (in):"),
                    result: pushBack.BuildResult().Side);
            });

            Assert.False(r.side);
            Assert.False(r.shared);
            Assert.True(r.saque);                      // the walk does reach the options row, so the absences count
            Assert.Equal(SafetySide.Left, r.result);   // and Push Back still gets back the side it passed in
        }
    }
}
