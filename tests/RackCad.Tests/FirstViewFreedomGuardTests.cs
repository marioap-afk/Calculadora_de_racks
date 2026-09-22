using System;
using System.IO;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>I-55 G10 source boundaries: policy chooses; modules route; the host places one typed first view.</summary>
    public sealed class FirstViewFreedomGuardTests
    {
        [Theory]
        [InlineData("Systems/Selective/RackSelectiveWindow.xaml.cs")]
        [InlineData("Systems/Dynamic/RackDynamicSystemWindow.xaml.cs")]
        [InlineData("RackFrames/RackFrameConfiguratorWindow.xaml.cs")]
        public void FirstViewEditorsConsumeTheSharedExposurePolicy(string relative)
        {
            var source = Read("src", "RackCad.UI", relative.Replace('/', Path.DirectorySeparatorChar));
            Assert.Contains("RackViewExposure.IsExposed", source);
            Assert.Contains("RackViewProductOperation.CreateFirst", source);
        }

        [Fact]
        public void HeaderRequestAndModulesCarryOneIdentityAndTypedAddress()
        {
            var request = Read("src", "RackCad.UI", "Editor", "RackInsertionRequest.cs");
            var modules = Read("src", "RackCad.UI", "Editor", "EditorModules.cs");

            Assert.Contains("public string RackId { get; }", request);
            Assert.Contains("public RackViewAddress InitialAddress { get; }", request);
            Assert.Contains("window.RackId, window.InsertAddress.Value", modules);
            // G11 adds the ordered batch contract to the common request while G10's Header request keeps its one
            // explicit identity/address pair. Visible multi-view activation remains owned by G12.
            Assert.Contains("public IReadOnlyList<RackViewAddress> Views", request);
        }

        [Fact]
        public void HeaderEntryPathsDispatchTheAddressWithoutChoosingAViewInTheRegistry()
        {
            var commands = Read("src", "RackCad.Plugin", "RackCabeceraCommands.cs");
            var menu = Read("src", "RackCad.Plugin", "RackMenuCommands.cs");
            var modules = Read("src", "RackCad.UI", "Editor", "EditorModules.cs");

            Assert.Contains("initialAddress: window.InsertAddress", commands);
            Assert.Contains("address.Kind == DimensionViewKind.Planta", commands);
            Assert.Contains("header.InitialAddress", menu);
            Assert.DoesNotContain("DimensionViewKind", modules);
        }

        private static string Read(params string[] relative)
        {
            var dir = new DirectoryInfo(AppContext.BaseDirectory);
            while (dir != null && !File.Exists(Path.Combine(dir.FullName, "RackCad.sln"))) dir = dir.Parent;
            Assert.NotNull(dir);
            return File.ReadAllText(Path.Combine(dir.FullName, Path.Combine(relative)));
        }
    }
}
