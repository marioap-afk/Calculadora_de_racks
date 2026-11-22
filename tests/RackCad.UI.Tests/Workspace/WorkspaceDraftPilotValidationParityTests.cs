using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using RackCad.Application.Systems.PushBack;
using RackCad.Application.Systems.PushBack.Pilot;
using RackCad.UI.Controls;
using Xunit;

namespace RackCad.UI.Tests
{
    /// <summary>
    /// I-64 F4-T2A-PILOT-STATE (INV-F4T2A-02): the validation of the one field of the pilot is the validation of the classic
    /// AnnotationScaleBox. The adapter of the pilot (Application) and NumericFieldValidation (UI) must give the same verdict, and
    /// the same message, for every text in every culture.
    /// </summary>
    public class WorkspaceDraftPilotValidationParityTests
    {
        [Fact]
        public void TheClassicControlIsARequiredNumberGreaterThanZero()
        {
            // The parity above is only meaningful while the classic box keeps the rule it is compared with.
            var dir = new DirectoryInfo(AppContext.BaseDirectory);
            while (dir != null && !File.Exists(Path.Combine(dir.FullName, "RackCad.sln"))) dir = dir.Parent;
            Assert.NotNull(dir);
            var xaml = File.ReadAllText(Path.Combine(dir.FullName, "src", "RackCad.UI", "Systems", "PushBack", "RackPushBackSystemWindow.xaml"));
            var start = xaml.IndexOf("x:Name=\"AnnotationScaleBox\"", StringComparison.Ordinal);
            Assert.True(start >= 0);
            var element = xaml.Substring(start, xaml.IndexOf("/>", start, StringComparison.Ordinal) - start);

            Assert.Contains("Minimum=\"0\"", element);
            Assert.Contains("MinimumInclusive=\"False\"", element);
            Assert.DoesNotContain("IsOptional", element);
            Assert.DoesNotContain("IntegerOnly", element);
            Assert.DoesNotContain("Maximum", element);
        }
    }
}
