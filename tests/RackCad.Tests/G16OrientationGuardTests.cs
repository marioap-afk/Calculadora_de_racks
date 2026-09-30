using System;
using System.IO;
using System.Linq;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>
    /// G16 C16-06, obligation O-9 and contract H: the Plugin asks the orientation after the class and before any read or point,
    /// passes it to the pure request, and applies the rotation the pure plan computed; the write scope computes nothing; the
    /// orientation authority never touches a definition plan, a builder, AUTH-15 or the Foundation.
    /// </summary>
    public class G16OrientationGuardTests
    {
        [Fact]
        public void C16_06_TheCommandAsksTheOrientationAfterTheClassAndBeforeTheOneRead()
        {
            var port = Code("src/RackCad.Plugin/Views/RackProjectionCommandPort.cs");

            var cls = port.IndexOf("Clase de vista a proyectar", StringComparison.Ordinal);
            var orientation = port.IndexOf("Orientacion\") { AllowNone = true }", StringComparison.Ordinal);
            var read = port.IndexOf("RackProjectionSnapshotReader.Read(", StringComparison.Ordinal);
            var point = port.IndexOf("GetPoint(", StringComparison.Ordinal);
            Assert.True(cls >= 0 && orientation > cls && read > orientation && point > read, "class -> orientation -> read -> points");

            // Distinct shortcuts: the two words of the Owner start with «P», so the capitals are PR and PRE.
            Assert.Contains("ProjectedKeyword = \"PRoyectada\"", port);
            Assert.Contains("CanonicalKeyword = \"PREdeterminada\"", port);
            Assert.Contains("orientation.Keywords.Add(ProjectedKeyword);", port);
            Assert.Contains("orientation.Keywords.Add(CanonicalKeyword);", port);
            Assert.Contains("Keywords.Default = ProjectedKeyword", port);
            Assert.Contains("RackProjectionOrientationMode.Projected", port);
        }

        [Fact]
        public void C16_06_EnterIsProjectedAndEscCancelsBeforeTheRead()
        {
            var port = Code("src/RackCad.Plugin/Views/RackProjectionCommandPort.cs");

            Assert.Contains("mode.Status != PromptStatus.OK && mode.Status != PromptStatus.None", port);
            Assert.Contains("RackProjectionSnapshot.Unavailable(RackProjectionSnapshotFailure.Cancelled, null)", port);
            // Only an explicit «PREdeterminada» selects Canonical; OK with the default and None are Projected.
            Assert.Contains("string.Equals(result.StringResult, CanonicalKeyword, StringComparison.OrdinalIgnoreCase)", port);

            // The answer really reaches the read, and the mapping is exactly: OK + PREdeterminada -> Canonical, anything else -> Projected.
            Assert.Contains("KindOf(kind.StringResult), OrientationOf(mode))", port);
            var mapping = port.Substring(port.IndexOf("private static RackProjectionOrientationMode OrientationOf(", StringComparison.Ordinal));
            mapping = mapping.Substring(0, mapping.IndexOf(';') + 1);
            Assert.Contains("result.Status == PromptStatus.OK", mapping);
            Assert.Contains("? RackProjectionOrientationMode.Canonical", mapping);
            Assert.Contains(": RackProjectionOrientationMode.Projected;", mapping);
            Assert.DoesNotContain("PromptStatus.None", mapping);
        }

        [Fact]
        public void C16_06_TheReaderPassesTheModeIntoThePureRequest()
        {
            var reader = Code("src/RackCad.Plugin/Views/RackProjectionSnapshotReader.cs");

            Assert.Contains("RackProjectionOrientationMode orientation", reader);
            Assert.Contains("new RackProjectionRequest(selection.Selection, targetKind, sourceFacts, services, orientation)", reader);
        }

        [Fact]
        public void C16_06_TheWriteScopeAppliesTheRotationAndComputesNothing()
        {
            var scope = Code("src/RackCad.Plugin/Views/RackProjectionWriteScope.cs");

            Assert.Contains("Rotation = placement.RotationRadians,", scope);
            Assert.DoesNotContain("Math.", scope);
            Assert.DoesNotContain("Orientation", scope);
            Assert.DoesNotContain("TransformBy", scope);
        }

        [Fact]
        public void C16_06_TheOrientationAuthorityNeverTouchesDefinitionsBuildersAuth15OrTheFoundation()
        {
            var authority = Code("src/RackCad.Application/Views/Placement/RackProjectionOrientation.cs");

            foreach (var forbidden in new[]
            {
                "HeaderRunPlan", "CantileverViewPlan", "Builder", "RackDefinitionCreator", "RackEmbedDocument", "Autodesk",
            })
            {
                Assert.DoesNotContain(forbidden, authority);
            }

            Assert.Contains("RackViewFrameSemantics.TryAxisDirection(", authority);
            Assert.Contains("Placement.Linear", authority);
        }

        private static string Code(string relative)
        {
            var dir = new DirectoryInfo(AppContext.BaseDirectory);
            while (dir != null && !File.Exists(Path.Combine(dir.FullName, "RackCad.sln"))) dir = dir.Parent;
            Assert.NotNull(dir);
            return string.Join("\n", File.ReadAllLines(Path.Combine(dir.FullName, relative.Replace('/', Path.DirectorySeparatorChar)))
                .Select(line => line.Contains("//") ? line.Substring(0, line.IndexOf("//", StringComparison.Ordinal)) : line));
        }
    }
}
