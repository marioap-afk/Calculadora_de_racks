using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using RackCad.Application.Systems.Shared;
using RackCad.Application.RackFrames;
using RackCad.Application.Views.Policy;
using RackCad.Domain.RackFrames;
using RackCad.Domain.Systems.Shared;
using RackCad.UI.Editor;
using RackCad.UI.RackFrames;
using RackCad.UI.Systems.Dynamic;
using RackCad.UI.Systems.Selective;
using Xunit;

namespace RackCad.UI.Tests
{
    /// <summary>I-55 G10: every editor exposes each first view allowed by the product policy.</summary>
    public sealed class FirstViewFreedomTests
    {
        [Fact]
        public void Selective_NewRack_ExposesFrontalLateralAndPlanta()
        {
            StaTestRunner.Run(() =>
            {
                var window = SelectiveWindowTestSupport.Open(canInsertInAutoCad: true);
                try
                {
                    Assert.True(Button(window, "Insertar frontal").IsEnabled);
                    Assert.True(Named<Button>(window, "InsertLateralButton").IsEnabled);
                    Assert.True(Named<Button>(window, "InsertPlantaButton").IsEnabled);
                }
                finally { window.Close(); }
            });
        }

        [Fact]
        public void Dynamic_NewRack_ExposesEntranceExitPlantaAndLateral()
        {
            StaTestRunner.Run(() =>
            {
                var window = new RackDynamicSystemWindow(canInsertInAutoCad: true);
                try
                {
                    Assert.True(Named<Button>(window, "InsertLateralButton").IsEnabled);
                    Assert.True(Named<Button>(window, "InsertExitButton").IsEnabled);
                    Assert.True(Named<Button>(window, "InsertEntranceButton").IsEnabled);
                    Assert.True(Named<Button>(window, "InsertPlantaButton").IsEnabled);
                }
                finally { window.Close(); }
            });
        }

        [Theory]
        [InlineData("InsertLateralButton", "lateral", -1)]
        [InlineData("InsertExitButton", "frontal", 0)]
        [InlineData("InsertEntranceButton", "frontal", 1)]
        [InlineData("InsertPlantaButton", "planta", -1)]
        public void Dynamic_AlternateFirstView_BuildsOneTypedRequest(string button, string view, int section)
        {
            var result = StaTestRunner.Run(() =>
            {
                var window = new RackDynamicSystemWindow(canInsertInAutoCad: true);
                EditorWindowTestSupport.ClickNamed(window, button);
                return (window.InsertRequested, window.InsertView, window.InsertSection, window.RackId);
            });

            Assert.True(result.InsertRequested);
            Assert.Equal(view, result.InsertView);
            Assert.Equal(section, result.InsertSection);
            Assert.True(Guid.TryParse(result.RackId, out _));
        }

        [Theory]
        [InlineData("Insertar frontal", "frontal")]
        [InlineData("InsertLateralButton", "lateral")]
        [InlineData("InsertPlantaButton", "planta")]
        public void Selective_AlternateFirstView_BuildsOneTypedRequest(string action, string view)
        {
            var result = StaTestRunner.Run(() =>
            {
                var window = SelectiveWindowTestSupport.Open(canInsertInAutoCad: true);
                if (action.StartsWith("Insertar", StringComparison.Ordinal))
                    EditorWindowTestSupport.ClickByContent(window, action);
                else
                    EditorWindowTestSupport.ClickNamed(window, action);
                return (window.InsertRequested, window.InsertView, window.RackId);
            });

            Assert.True(result.InsertRequested);
            Assert.Equal(view, result.InsertView);
            Assert.True(Guid.TryParse(result.RackId, out _));
        }

        [Fact]
        public void Header_NewRack_ExposesLateralAndPlanta()
        {
            StaTestRunner.Run(() =>
            {
                var configuration = new HardcodedStandardRackFrameService().CreateDefault();
                var window = new RackFrameConfiguratorWindow(configuration, canInsertInAutoCad: true);
                try
                {
                    RaiseLoaded(window);
                    Assert.True(Button(window, "Insertar lateral").IsEnabled);
                    Assert.True(Named<Button>(window, "InsertPlantaButton").IsEnabled);
                }
                finally { window.Close(); }
            });
        }

        [Fact]
        public void Header_PlantaFirst_MintsOneIdentityAndCarriesItsTypedAddress()
        {
            var result = StaTestRunner.Run(() =>
            {
                var configuration = new HardcodedStandardRackFrameService().CreateDefault();
                var window = new RackFrameConfiguratorWindow(configuration, canInsertInAutoCad: true);
                RaiseLoaded(window);
                EditorWindowTestSupport.ClickNamed(window, "InsertPlantaButton");
                return (window.InsertRequested, window.RackId, window.InsertAddress);
            });

            Assert.True(result.InsertRequested);
            Assert.True(Guid.TryParse(result.RackId, out _));
            Assert.Equal(RackViewAddress.Whole(DimensionViewKind.Planta), result.InsertAddress);
        }

        [Theory]
        [InlineData(RackSystemKind.SelectiveRack, DimensionViewKind.Frontal, RackViewVariantKind.Fondo)]
        [InlineData(RackSystemKind.SelectiveRack, DimensionViewKind.Lateral, RackViewVariantKind.Post)]
        [InlineData(RackSystemKind.SelectiveRack, DimensionViewKind.Planta, RackViewVariantKind.Whole)]
        [InlineData(RackSystemKind.PalletFlow, DimensionViewKind.Frontal, RackViewVariantKind.FlowEnd)]
        [InlineData(RackSystemKind.PalletFlow, DimensionViewKind.Lateral, RackViewVariantKind.Post)]
        [InlineData(RackSystemKind.PalletFlow, DimensionViewKind.Planta, RackViewVariantKind.Whole)]
        [InlineData(RackSystemKind.Selective, DimensionViewKind.Lateral, RackViewVariantKind.Whole)]
        [InlineData(RackSystemKind.Selective, DimensionViewKind.Planta, RackViewVariantKind.Whole)]
        public void AlternateFirstView_IsOwnedByExposurePolicy(
            RackSystemKind systemKind,
            DimensionViewKind kind,
            RackViewVariantKind variant)
        {
            Assert.True(RackViewExposure.IsExposed(systemKind, Address(kind, variant), RackViewProductOperation.CreateFirst));
        }

        [Fact]
        public void HeaderRequest_CarriesOneIdentityAndTypedFirstAddress()
        {
            var address = RackViewAddress.Whole(DimensionViewKind.Planta);
            var request = new HeaderInsertionRequest(null, null, "H-17", address);

            Assert.Equal("H-17", request.RackId);
            Assert.Equal(address, request.InitialAddress);
        }

        [Fact]
        public void EntryPathsDoNotRetainTheHistoricFirstViewRejections()
        {
            var selective = Read("src", "RackCad.UI", "Systems", "Selective", "RackSelectiveWindow.xaml.cs");
            var dynamic = Read("src", "RackCad.UI", "Systems", "Dynamic", "RackDynamicSystemWindow.xaml.cs");
            var header = Read("src", "RackCad.UI", "RackFrames", "RackFrameConfiguratorWindow.xaml.cs");

            Assert.DoesNotContain("Primero inserta la vista frontal", selective);
            Assert.DoesNotContain("Primero inserta la vista lateral", dynamic);
            Assert.DoesNotContain("Primero inserta la cabecera lateral", header);
            Assert.Contains("RackViewExposure.IsExposed", selective);
            Assert.Contains("RackViewExposure.IsExposed", dynamic);
            Assert.Contains("RackViewExposure.IsExposed", header);
        }

        private static RackViewAddress Address(DimensionViewKind kind, RackViewVariantKind variant)
        {
            if (variant == RackViewVariantKind.Fondo) return RackViewAddress.Fondo(0);
            if (variant == RackViewVariantKind.Post) return RackViewAddress.Post(0);
            if (variant == RackViewVariantKind.FlowEnd) return RackViewAddress.FlowEnd(RackFlowEnd.Exit);
            return RackViewAddress.Whole(kind);
        }

        private static T Named<T>(FrameworkElement root, string name) where T : FrameworkElement
            => (T)root.FindName(name);

        private static Button Button(FrameworkElement root, string content)
            => EditorWindowTestSupport.Find<Button>(root, b => string.Equals(b.Content as string, content, StringComparison.Ordinal));

        private static void RaiseLoaded(Window window)
            => window.RaiseEvent(new RoutedEventArgs(FrameworkElement.LoadedEvent));

        private static string Read(params string[] relative)
        {
            var dir = new DirectoryInfo(AppContext.BaseDirectory);
            while (dir != null && !File.Exists(Path.Combine(dir.FullName, "RackCad.sln"))) dir = dir.Parent;
            Assert.NotNull(dir);
            return File.ReadAllText(Path.Combine(dir.FullName, Path.Combine(relative)));
        }
    }
}
