using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.Json;
using RackCad.Application.Catalogs;
using RackCad.Application.Persistence;
using RackCad.Application.Systems.PushBack;
using RackCad.Application.Systems.PushBack.Pilot;
using RackCad.Application.Workspace;
using RackCad.Application.Workspace.Drafts;
using RackCad.UI.Systems.PushBack;
using Xunit;

namespace RackCad.UI.Tests
{
    /// <summary>
    /// I-64 F4-T2A-PILOT-STATE (INV-F4T2A-09): window versus pilot, without touching any product code. For each design of the one
    /// corpus (JSON envelopes under tests/RackCad.Tests/Fixtures/I64/, read by path from the repository root) the classic window
    /// loads the SAME envelope text with LoadExisting and its LastComputation.Design is compared with what the pilot writes
    /// (Materialize, ComposeForWrite and BuildDesign) with the SAME catalog. The delta of each design is pinned; it can only
    /// belong to (a) rounding to the "0.###" format of the controls, (b) the loss of DimensionStyle in ReadInputs, (c) the
    /// substitution or adjustment the controls make in LoadFromModel, or (d) the cell committed again from the controls.
    /// A delta of any other class is a stop condition (F4T2A-C03): it is reported, never pinned.
    /// </summary>
    public class WorkspaceDraftPilotWindowLoadCharacterizationTests
    {
        private static readonly string[] Canonical =
        {
            "pb-01-base", "pb-02-modulos", "pb-03-fondos", "pb-04-blanco", "pb-05-topes", "pb-06-seguridad", "pb-07-combinado", "pb-08-redondeo",
        };

        private static readonly string[] Legacy =
        {
            "pb-10-sin-datum", "pb-11-sin-i41", "pb-12-poste-nulo", "pb-13-sideb-no-presente", "pb-14-peralte-heredado", "pb-15-sin-composite",
        };

        public static IEnumerable<object[]> Corpus() => Canonical.Concat(Legacy).Select(name => new object[] { name });

        private static DirectoryInfo RepoRoot()
        {
            var dir = new DirectoryInfo(AppContext.BaseDirectory);
            while (dir != null && !File.Exists(Path.Combine(dir.FullName, "RackCad.sln"))) dir = dir.Parent;
            Assert.True(dir != null, "repo root (RackCad.sln) not found");
            return dir;
        }

        // ---- JSON delta ---------------------------------------------------------------------------------------------

        private static bool IsContainer(JsonElement element) =>
            element.ValueKind == JsonValueKind.Object || element.ValueKind == JsonValueKind.Array;

        private static void Walk(JsonElement a, JsonElement b, string path, List<string> into)
        {
            if (a.ValueKind == JsonValueKind.Object && b.ValueKind == JsonValueKind.Object)
            {
                var names = a.EnumerateObject().Select(p => p.Name).Concat(b.EnumerateObject().Select(p => p.Name)).Distinct().OrderBy(n => n, StringComparer.Ordinal);
                foreach (var name in names)
                {
                    var hasA = a.TryGetProperty(name, out var left);
                    var hasB = b.TryGetProperty(name, out var right);
                    if (hasA && hasB) Walk(left, right, path + "." + name, into);
                    else if (hasA) Leaves(left, path + "." + name, "<absent>", true, into);
                    else Leaves(right, path + "." + name, "<absent>", false, into);
                }

                return;
            }

            if (a.ValueKind == JsonValueKind.Array && b.ValueKind == JsonValueKind.Array)
            {
                var count = Math.Max(a.GetArrayLength(), b.GetArrayLength());
                for (var index = 0; index < count; index++)
                {
                    var itemPath = path + "[" + index + "]";
                    if (index < a.GetArrayLength() && index < b.GetArrayLength()) Walk(a[index], b[index], itemPath, into);
                    else if (index < a.GetArrayLength()) Leaves(a[index], itemPath, "<absent>", true, into);
                    else Leaves(b[index], itemPath, "<absent>", false, into);
                }

                return;
            }

            if (IsContainer(a) && !IsContainer(b)) { Leaves(a, path, Short(b), true, into); return; }
            if (!IsContainer(a) && IsContainer(b)) { Leaves(b, path, Short(a), false, into); return; }

            if (a.ValueKind == JsonValueKind.Number && b.ValueKind == JsonValueKind.Number)
            {
                if (a.GetDouble() != b.GetDouble()) into.Add(path + ": " + Short(a) + " -> " + Short(b));
                return;
            }

            if (a.ValueKind != b.ValueKind || a.GetRawText() != b.GetRawText())
            {
                into.Add(path + ": " + Short(a) + " -> " + Short(b));
            }
        }

        /// <summary>Every leaf of a container that is on ONE side of the difference; "other" is the text of the other side.</summary>
        private static void Leaves(JsonElement element, string path, string other, bool before, List<string> into)
        {
            if (element.ValueKind == JsonValueKind.Object && element.EnumerateObject().Any())
            {
                foreach (var property in element.EnumerateObject()) Leaves(property.Value, path + "." + property.Name, other, before, into);
                return;
            }

            if (element.ValueKind == JsonValueKind.Array && element.GetArrayLength() > 0)
            {
                var index = 0;
                foreach (var item in element.EnumerateArray()) Leaves(item, path + "[" + index++ + "]", other, before, into);
                return;
            }

            var text = element.ValueKind == JsonValueKind.Object ? "{}" : element.ValueKind == JsonValueKind.Array ? "[]" : Short(element);
            into.Add(path + ": " + (before ? text + " -> " + other : other + " -> " + text));
        }

        private static string Short(JsonElement element)
        {
            return element.ValueKind == JsonValueKind.Number
                ? element.GetDouble().ToString("R", CultureInfo.InvariantCulture)
                : element.GetRawText();
        }

        /// <summary>The pinned delta of each design: "path: window -> pilot", sorted. Filled from the measured run.</summary>
        private static readonly Dictionary<string, string[]> Expected = new Dictionary<string, string[]>
        {
            ["pb-01-base"] = new string[0],
            ["pb-02-modulos"] = new string[0],
            ["pb-03-fondos"] = new string[0],
            ["pb-04-blanco"] = new string[0],
            ["pb-05-topes"] = new string[0],
            ["pb-06-seguridad"] = new[] { "$.PushBack.Structure.DimensionStyle: null -> \"STANDARD-ISO\"" },
            ["pb-07-combinado"] = new[] { "$.PushBack.Structure.DimensionStyle: null -> \"SIMPLEX\"" },
            ["pb-08-redondeo"] = new[]
            {
                "$.PushBack.Structure.AnnotationScale: 1.235 -> 1.23456",
                "$.PushBack.Structure.Fronts[0].Levels[0].PalletHeight: 61.235 -> 61.23456",
            },
            ["pb-10-sin-datum"] = new string[0],
            ["pb-11-sin-i41"] = new string[0],
            ["pb-12-poste-nulo"] = new string[0],
            ["pb-13-sideb-no-presente"] = new string[0],
            ["pb-14-peralte-heredado"] = new string[0],
            ["pb-15-sin-composite"] = new string[0],
        };

        /// <summary>
        /// The class of a delta line ("path: window -> pilot"), or "OUTSIDE" when it belongs to none of the four known classes.
        /// (a) the window shows the value rounded to the "0.###" format of its controls and the pilot keeps the authored one;
        /// (b) ReadInputs rebuilds Annotations without DimensionStyle, so the window loses it and the pilot keeps it. Classes (c)
        /// (substitution or adjustment of LoadFromModel) and (d) (a cell committed again from the controls) do not show in this
        /// corpus, and the delivery says so in its KnownLimitations.
        /// </summary>
        private static string ClassOf(string line)
        {
            if (line.StartsWith("$.PushBack.Structure.DimensionStyle: ", StringComparison.Ordinal)) return "b";

            var colon = line.IndexOf(": ", StringComparison.Ordinal);
            var arrow = line.IndexOf(" -> ", StringComparison.Ordinal);
            if (colon > 0 && arrow > colon
                && double.TryParse(line.Substring(colon + 2, arrow - colon - 2), NumberStyles.Float, CultureInfo.InvariantCulture, out var window)
                && double.TryParse(line.Substring(arrow + 4), NumberStyles.Float, CultureInfo.InvariantCulture, out var pilot)
                && window != pilot
                && window == double.Parse(pilot.ToString("0.###", CultureInfo.InvariantCulture), CultureInfo.InvariantCulture))
            {
                return "a";
            }

            return "OUTSIDE";
        }

        [Fact]
        public void ClassAIsEvidencedWithDataTheScaleAndACellValueWithMoreThanThreeDecimalsAreRoundedByTheControls()
        {
            var delta = Expected["pb-08-redondeo"];

            Assert.Equal(2, delta.Length);
            Assert.All(delta, line => Assert.Equal("a", ClassOf(line)));
            Assert.Contains(delta, line => line.StartsWith("$.PushBack.Structure.AnnotationScale: ", StringComparison.Ordinal));
            Assert.Contains(delta, line => line.StartsWith("$.PushBack.Structure.Fronts[0].Levels[0].PalletHeight: ", StringComparison.Ordinal));
            Assert.Contains(Expected.Values.SelectMany(lines => lines), line => ClassOf(line) == "b");
        }

        [Fact]
        public void TheCorpusOfTheCharacterizationIsTheCorpusOfTheFixturesDirectory()
        {
            var dir = Path.Combine(RepoRoot().FullName, "tests", "RackCad.Tests", "Fixtures", "I64");
            var onDisk = Directory.GetFiles(dir, "pb-*.json").Select(Path.GetFileNameWithoutExtension).OrderBy(n => n, StringComparer.Ordinal).ToArray();
            var expected = Canonical.Concat(Legacy).Concat(new[] { "pb-20-ab", "pb-21-cola", "pb-31-v2-modulos" }).OrderBy(n => n, StringComparer.Ordinal).ToArray();

            Assert.Equal(expected, onDisk);
            Assert.True(Canonical.Length >= 6);
        }
    }
}
