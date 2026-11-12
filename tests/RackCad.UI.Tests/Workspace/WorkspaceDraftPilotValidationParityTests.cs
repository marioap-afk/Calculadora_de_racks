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
        private static readonly string[] Texts =
        {
            "", " ", "   ", "\t", "abc", "1x", "x1", "1.5", "1,5", "1.50", "1,50", " 2 ", "+3", "-3", "-0.5", "0", "0.0", "0,0", "-0",
            "0.001", "0,001", "1000000", "1e3", "1E-2", "1e999", "NaN", "Infinity", "-Infinity", "∞", "1.2.3", "1,2,3", "1 000", "1.000,5",
            "1,000.5", "١٢", "12٫5", ".5", ",5", "5.", "5,", "--1", "+-1", "0x10", "1_0", "1/2", "٣", "１２",
            "9999999999999999999999", "0.0000000000000000000000000001", "-0.0000001", "4.125", "4,125",
        };

        public static IEnumerable<object[]> Cultures() =>
            new[] { "es-MX", "en-US", "de-DE", string.Empty }.Select(name => new object[] { name });

        private static T InCulture<T>(string name, Func<T> body)
        {
            var previous = CultureInfo.CurrentCulture;
            try
            {
                CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo(name);
                return body();
            }
            finally
            {
                CultureInfo.CurrentCulture = previous;
            }
        }

        [Theory]
        [MemberData(nameof(Cultures))]
        public void TheAdapterGivesTheSameVerdictAndMessageAsTheClassicBoxInEveryCulture(string culture)
        {
            var adapter = new PushBackPilotFieldAdapter();

            foreach (var text in Texts)
            {
                var classic = InCulture(culture, () => NumericFieldValidation.Validate(text, false, false, 0, null, minimumInclusive: false));
                var pilot = InCulture(culture, () => adapter.Validate(PushBackPilotFieldAdapter.AnnotationScaleKey, text));

                Assert.True(classic.IsValid == pilot.IsValid, $"[{culture}] '{text}': classic={classic.IsValid} pilot={pilot.IsValid}");
                Assert.True(classic.Message == pilot.Message, $"[{culture}] '{text}': classic='{classic.Message}' pilot='{pilot.Message}'");
            }
        }

        [Theory]
        [MemberData(nameof(Cultures))]
        public void AValidTextAppliesTheSameNumberTheClassicBoxParses(string culture)
        {
            foreach (var text in Texts)
            {
                var classic = InCulture(culture, () => NumericFieldValidation.Validate(text, false, false, 0, null, minimumInclusive: false));
                if (!classic.IsValid) continue;

                var applied = InCulture(culture, () =>
                {
                    var state = new PushBackPilotDraftState(new PushBackCompositeEditorState(), PushBackEditorInputs.NewDesign(), "origin");
                    new PushBackPilotFieldAdapter().Apply(state, new Dictionary<string, string> { [PushBackPilotFieldAdapter.AnnotationScaleKey] = text });
                    return state.Inputs.Annotations.AnnotationScale;
                });

                Assert.Equal(classic.Value.Value, applied);
            }
        }

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
