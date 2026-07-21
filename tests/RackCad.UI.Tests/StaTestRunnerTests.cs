using System.Threading;
using System.Windows;
using System.Windows.Controls;
using Xunit;

namespace RackCad.UI.Tests
{
    /// <summary>Proves the STA harness so the control tests can rely on it: the body runs on an STA thread
    /// and real WPF objects (including a <see cref="Window"/>, which strictly requires STA) construct.</summary>
    public sealed class StaTestRunnerTests
    {
        [Fact]
        public void Run_ExecutesOnStaThread()
        {
            var apartment = StaTestRunner.Run(() => Thread.CurrentThread.GetApartmentState());
            Assert.Equal(ApartmentState.STA, apartment);
        }

        [Fact]
        public void Run_CanConstructWpfControls()
        {
            var built = StaTestRunner.Run(() =>
            {
                var checkbox = new CheckBox { IsChecked = true };
                var grid = new Grid();
                grid.Children.Add(checkbox);
                return grid.Children.Count;
            });

            Assert.Equal(1, built);
        }

        [Fact]
        public void Run_CanConstructAWindow()
        {
            // A Window throws "Calling thread must be STA" off an STA thread; this asserts the harness holds.
            var title = StaTestRunner.Run(() =>
            {
                var window = new Window { Title = "smoke" };
                return window.Title;
            });

            Assert.Equal("smoke", title);
        }
    }
}
