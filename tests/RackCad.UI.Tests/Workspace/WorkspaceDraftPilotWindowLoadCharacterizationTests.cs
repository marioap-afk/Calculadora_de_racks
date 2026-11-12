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
            "pb-01-base", "pb-02-modulos", "pb-03-fondos", "pb-04-blanco", "pb-05-topes", "pb-06-seguridad", "pb-07-combinado",
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

        private static string Fixture(string name)
        {
            var path = Path.Combine(RepoRoot().FullName, "tests", "RackCad.Tests", "Fixtures", "I64", name + ".json");
            Assert.True(File.Exists(path), "missing fixture " + path);
            return File.ReadAllText(path);
        }

        private static string Persist(RackCad.Domain.Systems.PushBack.PushBackDesign design) =>
            new RackProjectStore().Serialize(RackProject.ForPushBack(design));

        /// <summary>What the classic window ends up with after LoadExisting; Design is null when it rejects the values.</summary>
        private sealed class WindowResult
        {
            public string Design;
            public RackCatalog Catalog;
        }

        private static WindowResult LoadInWindow(string text)
        {
            return StaTestRunner.Run(() =>
            {
                var embed = new RackEmbedStore().Deserialize(text);
                var project = new RackProjectStore().Deserialize(embed.Design);
                var window = new RackPushBackSystemWindow(canInsertInAutoCad: true);
                window.LoadExisting(project.PushBackDesign, embed.Id, embed.Name, project);
                var result = new WindowResult { Catalog = window.Session.Catalog };
                if (window.CurrentInputsAreValid && window.LastComputation != null && window.LastComputation.IsValid)
                {
                    result.Design = Persist(window.LastComputation.Design);
                }

                return result;
            });
        }

        private static string PilotWrites(string text, RackCatalog catalog)
        {
            var materialized = PushBackPilotMaterializer.Materialize(text, catalog);
            Assert.Equal(PushBackPilotMaterializationKind.Admitted, materialized.Kind);
            var composition = PushBackPilotMaterializer.ComposeForWrite(materialized.State, materialized.State, catalog);
            Assert.True(composition.IsComposed, composition.Reason);
            return Persist(new PushBackCompositeEditorAssembler(catalog).BuildDesign(composition.Editor, composition.Inputs));
        }

        // ---- JSON delta ---------------------------------------------------------------------------------------------

        private static List<string> Diff(string window, string pilot)
        {
            var into = new List<string>();
            using (var a = JsonDocument.Parse(window))
            using (var b = JsonDocument.Parse(pilot))
            {
                Walk(a.RootElement, b.RootElement, "$", into);
            }

            return into;
        }

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
                    else into.Add(path + "." + name + ": " + (hasA ? Short(left) : "<absent>") + " -> " + (hasB ? Short(right) : "<absent>"));
                }

                return;
            }

            if (a.ValueKind == JsonValueKind.Array && b.ValueKind == JsonValueKind.Array && a.GetArrayLength() == b.GetArrayLength())
            {
                var index = 0;
                foreach (var pair in a.EnumerateArray().Zip(b.EnumerateArray(), (x, y) => (First: x, Second: y)))
                {
                    Walk(pair.First, pair.Second, path + "[" + index++ + "]", into);
                }

                return;
            }

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

        private static string Short(JsonElement element)
        {
            switch (element.ValueKind)
            {
                case JsonValueKind.Object: return "{object}";
                case JsonValueKind.Array: return "[" + element.GetArrayLength() + " items]";
                case JsonValueKind.Number: return element.GetDouble().ToString("R", CultureInfo.InvariantCulture);
                default: return element.GetRawText();
            }
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
            ["pb-10-sin-datum"] = new string[0],
            ["pb-11-sin-i41"] = new string[0],
            ["pb-12-poste-nulo"] = new string[0],
            ["pb-13-sideb-no-presente"] = new string[0],
            ["pb-14-peralte-heredado"] = new string[0],
            ["pb-15-sin-composite"] = new string[0],
        };

        /// <summary>
        /// The class of a delta line, or "OUTSIDE" when it belongs to none of the four known classes. Only class (b) shows in
        /// this corpus: ReadInputs rebuilds Annotations without DimensionStyle, so the window loses it and the pilot keeps it. The
        /// corpus uses values the "0.###" controls display exactly, so class (a) does not show, and the substitutions and the cell
        /// commit of classes (c) and (d) leave these designs unchanged.
        /// </summary>
        private static string ClassOf(string line)
        {
            if (line.StartsWith("$.PushBack.Structure.DimensionStyle: ", StringComparison.Ordinal)) return "b";
            return "OUTSIDE";
        }

        [Theory]
        [MemberData(nameof(Corpus))]
        public void TheWindowAndThePilotDifferOnlyInTheKnownClasses(string fixture)
        {
            var text = Fixture(fixture);
            var window = LoadInWindow(text);
            if (window.Design == null)
            {
                // The window rejects the values: the case is recorded as "the window does not admit it", without comparing.
                Assert.True(Expected.TryGetValue(fixture, out var recorded) && recorded.Length == 1 && recorded[0] == "WINDOW-NOT-ADMITTED", fixture + " WINDOW-NOT-ADMITTED");
                return;
            }

            var pilot = PilotWrites(text, window.Catalog);
            var delta = Diff(window.Design, pilot).OrderBy(line => line, StringComparer.Ordinal).ToArray();

            // A delta outside classes (a) to (d) is a stop condition (F4T2A-C03): it is never pinned.
            Assert.DoesNotContain(delta, line => ClassOf(line) == "OUTSIDE");
            Assert.True(
                Expected.TryGetValue(fixture, out var pinned) && pinned.SequenceEqual(delta),
                fixture + " delta (window -> pilot):" + Environment.NewLine + string.Join(Environment.NewLine, delta));
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
