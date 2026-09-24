using System;
using System.IO;
using System.Linq;
using RackCad.Application.Systems.Shared;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>Static complements for boundaries that Core cannot execute without loading AutoCAD.</summary>
    public sealed class I59F2BoundaryGuardTests
    {
        [Fact]
        public void CT59_01_APPLICATION_CONTRACT_HAS_NO_AUTODESK_ASSEMBLY_REFERENCE()
        {
            var references = typeof(RackSourcePlacementInput).Assembly
                .GetReferencedAssemblies()
                .Select(reference => reference.Name);

            Assert.DoesNotContain(references, name =>
                name != null && name.StartsWith("Autodesk.AutoCAD", StringComparison.Ordinal));
        }

        [Fact]
        public void CT59_01_PLUGIN_ADAPTER_ONLY_COPIES_THE_EXPLICIT_AUTOCAD_VALUES()
        {
            var source = ReadSource(
                "src", "RackCad.Plugin", "Drawing", "RackSourcePlacementCaptureAdapter.cs");

            Assert.Contains("reference.Position", source, StringComparison.Ordinal);
            Assert.Contains("reference.Rotation", source, StringComparison.Ordinal);
            Assert.Contains("reference.ScaleFactors", source, StringComparison.Ordinal);
            Assert.Contains("reference.Normal", source, StringComparison.Ordinal);
            Assert.Contains("definition.Origin", source, StringComparison.Ordinal);
            Assert.Contains("new RackSourcePlacementInput(", source, StringComparison.Ordinal);
            Assert.DoesNotContain("RackSourceTransformClassifier", source, StringComparison.Ordinal);
            Assert.DoesNotContain("Transaction", source, StringComparison.Ordinal);
        }

        private static string ReadSource(params string[] parts)
        {
            var directory = new DirectoryInfo(AppContext.BaseDirectory);
            while (directory != null && !File.Exists(Path.Combine(directory.FullName, "RackCad.sln")))
            {
                directory = directory.Parent;
            }

            Assert.NotNull(directory);
            var path = Path.Combine(directory.FullName, Path.Combine(parts));
            Assert.True(File.Exists(path), "No existe el archivo: " + path);
            return File.ReadAllText(path);
        }
    }
}
