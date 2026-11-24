#nullable enable
using System;
using System.Windows;
using System.Windows.Threading;
using RackCad.UI.VentanaRackCad;
using Xunit;

namespace RackCad.UI.Tests
{
    /// <summary>
    /// Serializes the tests of the modal host of the RackCad window (M-21 of the third review). Every test body of this suite runs on ONE
    /// STA thread, handed over by <see cref="StaTestRunner"/> with the highest priority of the dispatcher, from the parallel collections
    /// of xUnit. The modal loop of <c>ShowDialog</c> of these tests ran the bodies of OTHER test classes inside itself, for more than 20 s,
    /// so neither the work of the host nor the safety timer could run on time, and the CI of <c>ff0da9b1</c> failed. Alone, the modal loop
    /// only has the work of the test. It is the narrowest boundary that closes it: one class; the rest of the suite keeps its parallelism.
    /// </summary>
    [CollectionDefinition(Name, DisableParallelization = true)]
    public sealed class RackModalHostWindowCollection
    {
        public const string Name = "rackcad-modal-host";
    }

    /// <summary>I-64 A-14 (phase 1): the invisible owner of the editors that the RackCad window opens through a command.</summary>
    [Collection(RackModalHostWindowCollection.Name)]
    public class RackModalHostWindowTests
    {
        [Fact]
        public void TheModalHostRunsTheWorkWithItsWindowAsOwnerAndCloses()
        {
            StaTestRunner.Run(() =>
            {
                Window? seen = null;
                var childShown = false;
                var host = new RackModalHostWindow(owner =>
                {
                    seen = owner;
                    var child = new Window { Owner = owner, Width = 50, Height = 50, ShowInTaskbar = false };
                    child.Loaded += (_, _) => { childShown = true; child.Close(); };
                    child.ShowDialog();
                    return 42;
                });

                ShowWithSafetyTimer(host.Window);

                Assert.Same(host.Window, seen);
                Assert.True(childShown);
                Assert.Equal(42, host.Result);
                Assert.Null(host.Failure);
                Assert.False(host.Window.IsVisible);
                Assert.NotNull(host.Window.Content);
            });
        }

        [Fact]
        public void AFailingWorkOfTheModalHostClosesItAndIsRethrown()
        {
            StaTestRunner.Run(() =>
            {
                var host = new RackModalHostWindow(_ => throw new InvalidOperationException("boom"));

                ShowWithSafetyTimer(host.Window);

                Assert.IsType<InvalidOperationException>(host.Failure);
                var thrown = Assert.Throws<InvalidOperationException>(() => host.ThrowIfFailed());
                Assert.Equal("boom", thrown.Message);
                Assert.Null(host.Result);
            });
        }

        [Fact]
        public void TheModalHostIsNotANewWindowType()
        {
            Assert.False(typeof(Window).IsAssignableFrom(typeof(RackModalHostWindow)));
        }

        // A host that never runs its work would leave ShowDialog open for ever: the timer closes the window and the test fails instead of
        // hanging the job of UI (M-01 of the second review). The timer runs at Normal priority, as the work of the host does, so it
        // measures its 15 s and is not left behind it (M-21 of the third review).
        private static void ShowWithSafetyTimer(Window window)
        {
            var timedOut = false;
            var timer = new DispatcherTimer(DispatcherPriority.Normal) { Interval = TimeSpan.FromSeconds(15) };
            timer.Tick += (_, _) =>
            {
                timer.Stop();
                timedOut = true;
                window.Close();
            };
            timer.Start();
            try
            {
                window.ShowDialog();
            }
            finally
            {
                timer.Stop();
            }

            Assert.False(timedOut, "the modal host did not run its work and close by itself.");
        }
    }
}
