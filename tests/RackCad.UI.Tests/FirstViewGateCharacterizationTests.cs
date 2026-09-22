using System;
using System.IO;
using Xunit;

namespace RackCad.UI.Tests
{
    /// <summary>I-55 G3: first-view actions and their current AutoCAD gates, without opening modal paths.</summary>
    public sealed class FirstViewGateCharacterizationTests
    {
        private static string Read(params string[] relative)
        {
            var dir = new DirectoryInfo(AppContext.BaseDirectory);
            while (dir != null && !File.Exists(Path.Combine(dir.FullName, "RackCad.sln"))) dir = dir.Parent;
            Assert.NotNull(dir);
            return File.ReadAllText(Path.Combine(dir.FullName, Path.Combine(relative)));
        }

        [Theory]
        [InlineData("Systems/Selective/RackSelectiveWindow.xaml", "Insertar frontal", "Insertar lateral", "Insertar planta")]
        [InlineData("Systems/Dynamic/RackDynamicSystemWindow.xaml", "Insertar lateral", "Insertar planta", "Actualizar")]
        public void MultiViewEditorsExposeAFirstViewAndLinkedAdditionalViews(string file, string first, string second, string third)
        {
            var xaml = Read("src", "RackCad.UI", file.Replace('/', Path.DirectorySeparatorChar));
            Assert.Contains(first, xaml);
            Assert.Contains(second, xaml);
            Assert.Contains(third, xaml);
            Assert.Contains("ToolTipService.ShowOnDisabled=\"True\"", xaml);
        }

        [Fact]
        public void SelectiveExposesEverySupportedFirstViewThroughTheSharedPolicy()
        {
            var source = Read("src", "RackCad.UI", "Systems", "Selective", "RackSelectiveWindow.xaml.cs");
            Assert.Contains("RackViewExposure.IsExposed", source);
            Assert.Contains("RackViewAddress.Post(0)", source);
            Assert.Contains("RackViewAddress.Whole(DimensionViewKind.Planta)", source);
            Assert.DoesNotContain("Primero inserta la vista frontal", source);
        }

        [Fact]
        public void FlowBedCurrentFirstViewGateRequiresAutoCadAndAValidModel()
        {
            var source = Read("src", "RackCad.UI", "Systems", "FlowBed", "RackFlowBedWindow.xaml.cs");
            Assert.Contains("canInsertInAutoCad", source);
            Assert.Contains("InsertButton.IsEnabled", source);
            Assert.Contains("ToolTip", source);
        }
    }
}
