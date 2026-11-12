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

        private static string Fixture(string name)
        {
            var path = Path.Combine(RepoRoot().FullName, "tests", "RackCad.Tests", "Fixtures", "I64", name + ".json");
            Assert.True(File.Exists(path), "missing fixture " + path);
            return File.ReadAllText(path);
        }

        private static string Persist(RackCad.Domain.Systems.PushBack.PushBackDesign design) =>
            new RackProjectStore().Serialize(RackProject.ForPushBack(design)).Replace("\r\n", "\n");

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
        public void ClassAIsEvidencedWithDataTheScaleAndACellValueWithMoreThanThreeDecimalsAreRoundedByTheControls()
        {
            var delta = Expected["pb-08-redondeo"];

            Assert.Equal(2, delta.Length);
            Assert.All(delta, line => Assert.Equal("a", ClassOf(line)));
            Assert.Contains(delta, line => line.StartsWith("$.PushBack.Structure.AnnotationScale: ", StringComparison.Ordinal));
            Assert.Contains(delta, line => line.StartsWith("$.PushBack.Structure.Fronts[0].Levels[0].PalletHeight: ", StringComparison.Ordinal));
            Assert.Contains(Expected.Values.SelectMany(lines => lines), line => ClassOf(line) == "b");
        }

        /// <summary>
        /// What the pilot writes after Keep-mine (V2 changed content the pilot does not edit; the draft changed the scale) opens in
        /// the classic window with the content of V2 and the scale of the draft: nothing of V2 is reverted on the way.
        /// </summary>
        [Fact]
        public void TheDocumentThePilotWritesAfterKeepMineOpensInTheClassicWindowWithTheNewBaseAndTheScaleOfTheDraft()
        {
            var v1 = Fixture("pb-01-base");
            var catalog = LoadInWindow(v1).Catalog;

            // V2: the same rack with another pallet tolerance, assembled by the product itself.
            var changed = PushBackPilotMaterializer.Materialize(v1, catalog).State;
            changed.Inputs.PalletTolerance += 2.0;
            var store = new RackEmbedStore();
            var embed = store.Deserialize(v1);
            embed.Design = new RackProjectStore().Serialize(RackProject.ForPushBack(
                new PushBackCompositeEditorAssembler(catalog).BuildDesign(changed.Editor, changed.Inputs)));
            var v2 = store.Serialize(embed);
            var v1Tolerance = DesignNumber(LoadInWindow(v1).Design, "PalletTolerance");
            var v2Tolerance = DesignNumber(LoadInWindow(v2).Design, "PalletTolerance");
            Assert.NotEqual(v1Tolerance, v2Tolerance);

            // The draft opens V1, edits the scale and keeps its edits on V2.
            var draft = new DraftRegistry().Begin(
                new SessionId(1), "R1",
                DraftIntentRead<PushBackPilotDraftState>.Admitted(
                    DraftStaleBase.Create(new[] { new DraftStaleBaseEntry("D1", DraftMembership.Mutable, "frontal", "-1", v1) }),
                    PushBackPilotMaterializer.Materialize(v1, catalog).State),
                PushBackPilotStateComparer.Instance, PushBackPilotStateComparer.Copy).Draft;
            draft.CurrentDraftState.Inputs.Annotations.AnnotationScale = 2.25;
            var second = PushBackPilotMaterializer.Materialize(v2, catalog).State;
            var keep = draft.KeepMineOnCurrentVersion(
                DraftIntentRead<PushBackPilotDraftState>.Admitted(
                    DraftStaleBase.Create(new[] { new DraftStaleBaseEntry("D1", DraftMembership.Mutable, "frontal", "-1", v2) }), second),
                confirmed: true);
            Assert.Equal(DraftReloadStatus.Reloaded, keep.Status);
            draft.SetCurrentDraftState(PushBackPilotMaterializer.KeepPilotEditsOn(second, draft.CurrentDraftState));

            var composition = PushBackPilotMaterializer.ComposeForWrite(draft.InitialDraftState, draft.CurrentDraftState, catalog);
            Assert.True(composition.IsComposed, composition.Reason);
            embed.Design = new RackProjectStore().Serialize(RackProject.ForPushBack(
                new PushBackCompositeEditorAssembler(catalog).BuildDesign(composition.Editor, composition.Inputs)));
            var opened = LoadInWindow(store.Serialize(embed));

            Assert.NotNull(opened.Design);
            Assert.Equal(2.25, DesignNumber(opened.Design, "AnnotationScale"));
            Assert.Equal(v2Tolerance, DesignNumber(opened.Design, "PalletTolerance"));
        }

        private static double DesignNumber(string design, string structureProperty)
        {
            using (var document = JsonDocument.Parse(design))
            {
                return document.RootElement.GetProperty("PushBack").GetProperty("Structure").GetProperty(structureProperty).GetDouble();
            }
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
