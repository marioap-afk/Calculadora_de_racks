using System;
using System.Windows;
using RackCad.Application.RackFrames;
using RackCad.Application.Systems.Shared;
using RackCad.Domain.Systems.Shared;
using RackCad.UI.Editor;
using RackCad.UI.RackFrames;
using RackCad.UI.Systems.Dynamic;
using RackCad.UI.Systems.Selective;
using Xunit;

namespace RackCad.UI.Tests
{
    /// <summary>
    /// G16 corrective round (C16-01). The Owner's Selective lateral-first insertion reached the Plugin with an EMPTY ordered
    /// view list (a Selective lateral with no section decoded to no address), so the new-rack batch reported
    /// ONE_RACK_REQUIRED (0/0). G10 pinned <c>InsertView</c> and <c>InsertSection</c> but never the ordered views the Plugin
    /// actually consumes. Every supported first view must hand the host exactly ONE concrete, typed view.
    /// </summary>
    public sealed class FirstViewRequestViewsTests
    {
        [Theory]
        [InlineData("Insertar frontal", "Frontal")]
        [InlineData("InsertLateralButton", "Lateral")]
        [InlineData("InsertPlantaButton", "Planta")]
        public void Selective_EveryFirstViewCarriesOneConcreteView(string action, string expected)
        {
            var views = StaTestRunner.Run(() =>
            {
                var window = SelectiveWindowTestSupport.Open(canInsertInAutoCad: true);
                if (action.StartsWith("Insertar", StringComparison.Ordinal))
                    EditorWindowTestSupport.ClickByContent(window, action);
                else
                    EditorWindowTestSupport.ClickNamed(window, action);
                return window.InsertionRequest.Views;
            });

            var address = Assert.Single(views);
            Assert.Equal(expected, address.Kind.ToString());
        }

        [Fact]
        public void Selective_ALateralFirstViewIsAPostViewNotAnEmptyList()
        {
            var views = StaTestRunner.Run(() =>
            {
                var window = SelectiveWindowTestSupport.Open(canInsertInAutoCad: true);
                EditorWindowTestSupport.ClickNamed(window, "InsertLateralButton");
                return window.InsertionRequest.Views;
            });

            Assert.Equal(RackViewAddress.Post(0), Assert.Single(views));
        }

        [Theory]
        [InlineData("InsertExitButton", DimensionViewKind.Frontal, RackFlowEnd.Exit)]
        [InlineData("InsertEntranceButton", DimensionViewKind.Frontal, RackFlowEnd.Entrance)]
        public void Dynamic_EveryFrontalFirstViewCarriesItsEnd(string button, DimensionViewKind kind, RackFlowEnd end)
        {
            var views = StaTestRunner.Run(() =>
            {
                var window = new RackDynamicSystemWindow(canInsertInAutoCad: true);
                EditorWindowTestSupport.ClickNamed(window, button);
                return window.InsertionRequest.Views;
            });

            Assert.Equal(RackViewAddress.FlowEnd(end), Assert.Single(views));
        }

        [Theory]
        [InlineData("InsertLateralButton")]
        [InlineData("InsertPlantaButton")]
        public void Dynamic_LateralAndPlantaFirstViewsCarryOneConcreteView(string button)
        {
            var views = StaTestRunner.Run(() =>
            {
                var window = new RackDynamicSystemWindow(canInsertInAutoCad: true);
                EditorWindowTestSupport.ClickNamed(window, button);
                return window.InsertionRequest.Views;
            });

            Assert.Single(views);
        }

        [Theory]
        [InlineData("InsertPlantaButton", DimensionViewKind.Planta)]
        public void Header_PlantaFirstCarriesItsViewInTheRequest(string button, DimensionViewKind kind)
        {
            var views = StaTestRunner.Run(() =>
            {
                var configuration = new HardcodedStandardRackFrameService().CreateDefault();
                var window = new RackFrameConfiguratorWindow(configuration, canInsertInAutoCad: true);
                window.RaiseEvent(new RoutedEventArgs(FrameworkElement.LoadedEvent));
                EditorWindowTestSupport.ClickNamed(window, button);
                return window.InsertViews;
            });

            Assert.Equal(RackViewAddress.Whole(kind), Assert.Single(views));
        }

        [Theory]
        [InlineData(RackSystemKind.PushBack, DimensionViewKind.Planta)]
        [InlineData(RackSystemKind.PushBack, DimensionViewKind.Lateral)]
        [InlineData(RackSystemKind.PushBack, DimensionViewKind.Frontal)]
        [InlineData(RackSystemKind.Cantilever, DimensionViewKind.Planta)]
        [InlineData(RackSystemKind.Cantilever, DimensionViewKind.Lateral)]
        [InlineData(RackSystemKind.Cantilever, DimensionViewKind.Frontal)]
        public void PushBackAndCantilever_EveryFirstViewCarriesOneConcreteView(RackSystemKind kind, DimensionViewKind view)
        {
            var address = Address(kind, view);
            var syntax = RackViewCodec.Encode(kind, address);

            RackInsertionRequest request = kind == RackSystemKind.PushBack
                ? new PushBackInsertionRequest(null, null, "P", "", syntax.View, syntax.Section, null)
                : (RackInsertionRequest)new CantileverInsertionRequest(null, null, "C", "", syntax.View, syntax.Section, null);

            Assert.Equal(address, Assert.Single(request.Views));
        }

        private static RackViewAddress Address(RackSystemKind kind, DimensionViewKind view)
        {
            switch (view)
            {
                case DimensionViewKind.Planta:
                    return RackViewAddress.Whole(DimensionViewKind.Planta);
                case DimensionViewKind.Lateral:
                    return kind == RackSystemKind.Cantilever ? RackViewAddress.Station(0) : RackViewAddress.Post(0);
                default:
                    return kind == RackSystemKind.Cantilever
                        ? RackViewAddress.Whole(DimensionViewKind.Frontal)
                        : RackViewAddress.PushBackCut(RackPushBackEnd.EntradaSalida, RackPushBackSide.A);
            }
        }
    }
}
