using System.Linq;
using RackCad.Domain.Systems;
using RackCad.UI;
using Xunit;

namespace RackCad.UI.Tests
{
    /// <summary>
    /// PB-008 / PB-010 (I-32) — the forklift-defence dialog Push Back opens.
    ///
    /// PB-008: Push Back is LIFO, so the low end is where loading AND unloading happen. Naming its two ends "Salida"
    /// and "Entrada" described a rack it is not; they are "Entrada/Salida" and "Posterior". The physical mapping does
    /// not move — the low end is still the one the plan calls the exit — only the words the user reads.
    ///
    /// PB-010: an "Auto" box per end, so the 12"/36" rule is recomputed when a post stops being an edge.
    /// </summary>
    public sealed class PushBackDefensaDialogTests
    {
        [Fact]
        public void PushBack_NamesTheEndsEntradaSalidaAndPosterior()
        {
            var r = StaTestRunner.Run(() =>
            {
                var pushBack = new SafetyDefensaGridWindow(
                    "Defensa de montacargas", 3, null, lowEndOnly: true, autoPerEnd: true);
                var dynamicOne = new SafetyDefensaGridWindow("Defensa de montacargas", 3, null);

                return (
                    pbLow: SafetyDialogTestSupport.HasText(pushBack, "Entrada/Salida"),
                    pbHigh: SafetyDialogTestSupport.HasText(pushBack, "Posterior"),
                    pbOldLow: SafetyDialogTestSupport.HasText(pushBack, "Salida"),
                    pbOldHigh: SafetyDialogTestSupport.HasText(pushBack, "Entrada"),
                    dynLow: SafetyDialogTestSupport.HasText(dynamicOne, "Salida"),
                    dynHigh: SafetyDialogTestSupport.HasText(dynamicOne, "Entrada"),
                    dynAuto: SafetyDialogTestSupport.HasText(dynamicOne, "Auto"),
                    pbAuto: SafetyDialogTestSupport.HasText(pushBack, "Auto"));
            });

            Assert.True(r.pbLow);
            Assert.True(r.pbHigh);
            Assert.False(r.pbOldLow);    // the bare "Salida"/"Entrada" headers are gone in Push Back
            Assert.False(r.pbOldHigh);
            Assert.True(r.pbAuto);       // PB-010: the per-end Auto is offered

            // The default path (Dinámico) keeps its own two names and gains no Auto column.
            Assert.True(r.dynLow);
            Assert.True(r.dynHigh);
            Assert.False(r.dynAuto);
        }

        /// <summary>
        /// A brand-new Push Back rack: every post is fully automatic at the low end and OFF at the rear, so the dialog
        /// stores NO record — "no record" is exactly what makes the plan recompute 12/36 as fronts are added.
        /// </summary>
        [Fact]
        public void PushBack_ANewRackStoresNoRecord_AndTheRearStartsOff()
        {
            var r = StaTestRunner.Run(() =>
            {
                var dialog = new SafetyDefensaGridWindow(
                    "Defensa de montacargas", 4, null, lowEndOnly: true, autoPerEnd: true);
                dialog.BuildResultForTest();
                return dialog.Result.ToList();
            });

            Assert.Empty(r);
        }

        /// <summary>
        /// The Dinámico path is byte-identical in DATA too: with no Auto columns it never emits an Auto flag, so its
        /// records keep meaning exactly what they always meant.
        /// </summary>
        [Fact]
        public void Dynamic_NeverEmitsAutoFlags()
        {
            var r = StaTestRunner.Run(() =>
            {
                var current = new[] { new SafetyPostDefense { PostIndex = 1, ExitLength = 18.0, EntranceLength = 24.0 } };
                var dialog = new SafetyDefensaGridWindow("Defensa de montacargas", 3, current);
                dialog.BuildResultForTest();
                return dialog.Result.ToList();
            });

            Assert.Single(r);
            Assert.Equal(18.0, r[0].ExitLength, 6);
            Assert.Equal(24.0, r[0].EntranceLength, 6);
            Assert.False(r[0].ExitAuto);
            Assert.False(r[0].EntranceAuto);
        }
    }
}
