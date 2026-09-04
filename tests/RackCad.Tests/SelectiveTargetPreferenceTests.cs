using System.IO;
using System.Linq;
using RackCad.Application.Settings;
using RackCad.Application.Systems.Selective;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>
    /// I-43, gate 8 correction: the "Fondos destino" choice is remembered between openings as an EDITOR preference.
    /// What is stored is the INTENT — "Todos", "Actual" or a specific set — and it is re-resolved against the rack
    /// being opened, so it survives onto racks with a different number of fondos. Nothing about it belongs to the
    /// design, so these tests never touch a pallet design or a rack.
    /// </summary>
    public class SelectiveTargetPreferenceTests
    {
        private static SelectiveEditorState StateWith(int fondos)
        {
            var state = new SelectiveEditorState();
            for (var k = 0; k < fondos; k++)
            {
                state.InitMatrix(2, 2);
                state.FondoMatrices.Add(state.SnapshotWorking(48.0, 0.0));
            }

            state.SelectedFondo = 0;
            state.LoadFondo(0);
            return state;
        }

        // ---- The default: an installation that has never chosen ----

        [Fact]
        public void WithNoStoredPreference_TheDefaultIsTodos()
        {
            Assert.Equal(SelectiveTargetMode.All, SelectiveTargetPreference.Decode(null).Mode);
            Assert.Equal(SelectiveTargetMode.All, SelectiveTargetPreference.Decode(string.Empty).Mode);
            Assert.Equal(SelectiveTargetMode.All, SelectiveTargetPreference.Decode("   ").Mode);
            Assert.Equal(SelectiveTargetMode.All, SelectiveTargetPreference.Decode(new UserSettings().SelectiveTargetFondos).Mode);
        }

        [Fact]
        public void TheDefaultAimsAtEveryFondoOfTheRackBeingOpened()
        {
            var state = StateWith(3);
            SelectiveTargetPreference.Decode(null).ApplyTo(state);

            Assert.Equal(SelectiveTargetMode.All, state.TargetMode);
            Assert.Equal(new[] { 0, 1, 2 }, state.TargetFondos.Fondos);
        }

        [Fact]
        public void WithASingleFondo_TodosIsNaturallyThatOneFondo()
        {
            var state = StateWith(1);
            SelectiveTargetPreference.All.ApplyTo(state);

            Assert.Equal(new[] { 0 }, state.TargetFondos.Fondos);
        }

        // ---- Round-trip of each intent ----

        [Fact]
        public void ClosingAndReopeningAfterActual_ComesBackAsActual()
        {
            var first = StateWith(3);
            first.SelectFondo(1);
            first.FollowCurrentFondo();

            var stored = SelectiveTargetPreference.Capture(first).Encode();
            Assert.Equal("Actual", stored);

            // A different rack, opened on ITS visible fondo: "Actual" is a mode, so it re-aims rather than restoring
            // the fondo the previous session happened to be on.
            var second = StateWith(4);
            second.SelectFondo(2);
            SelectiveTargetPreference.Decode(stored).ApplyTo(second);

            Assert.Equal(SelectiveTargetMode.FollowCurrent, second.TargetMode);
            Assert.Equal(new[] { 2 }, second.TargetFondos.Fondos);
        }

        [Fact]
        public void ClosingAndReopeningAfterAnExplicitSet_ComesBackAsThatSet()
        {
            var first = StateWith(4);
            first.SetTargetFondos(new[] { 1, 3 });

            var stored = SelectiveTargetPreference.Capture(first).Encode();
            Assert.Equal("1,3", stored); // 0-based, the same indices the model uses

            var second = StateWith(4);
            SelectiveTargetPreference.Decode(stored).ApplyTo(second);

            Assert.Equal(SelectiveTargetMode.Explicit, second.TargetMode);
            Assert.Equal(new[] { 1, 3 }, second.TargetFondos.Fondos);
        }

        [Fact]
        public void ClosingAndReopeningAfterTodos_ComesBackAsTodos_AndExpandsToTheNewRack()
        {
            var first = StateWith(2);
            first.FollowAllFondos();

            var stored = SelectiveTargetPreference.Capture(first).Encode();
            Assert.Equal("Todos", stored); // the INTENT, not the two indices it resolved to

            var second = StateWith(5);
            SelectiveTargetPreference.Decode(stored).ApplyTo(second);

            Assert.Equal(SelectiveTargetMode.All, second.TargetMode);
            Assert.Equal(new[] { 0, 1, 2, 3, 4 }, second.TargetFondos.Fondos);
        }

        // ---- Resolving against a rack that does not have those fondos ----

        [Fact]
        public void AnExplicitPreferenceKeepsOnlyTheFondosTheRackHas()
        {
            var state = StateWith(2); // {1,3} over a rack with fondos 0..1
            SelectiveTargetPreference.Decode("1,3").ApplyTo(state);

            Assert.Equal(SelectiveTargetMode.Explicit, state.TargetMode);
            Assert.Equal(new[] { 1 }, state.TargetFondos.Fondos); // fondo 3 is simply not there
        }

        [Fact]
        public void AnExplicitPreferenceEntirelyOutOfRange_FallsBackToTodos()
        {
            var state = StateWith(2);
            SelectiveTargetPreference.Decode("7,9").ApplyTo(state);

            // NOT the visible fondo: nothing of the stored choice survived, so the default takes over rather than
            // aiming the editor at a fondo the user never picked.
            Assert.Equal(SelectiveTargetMode.All, state.TargetMode);
            Assert.Equal(new[] { 0, 1 }, state.TargetFondos.Fondos);
        }

        [Theory]
        [InlineData("basura")]
        [InlineData("1,basura")]
        [InlineData("-1")]
        [InlineData("2.5")]
        public void AnUnusableStoredValue_IsTodos(string stored)
        {
            var state = StateWith(3);
            SelectiveTargetPreference.Decode(stored).ApplyTo(state);

            Assert.Equal(SelectiveTargetMode.All, state.TargetMode);
            Assert.Equal(new[] { 0, 1, 2 }, state.TargetFondos.Fondos);
        }

        // ---- "Todos" is a LIVING mode, not a snapshot ----

        [Fact]
        public void Todos_TakesInAFondoAddedAfterwards()
        {
            var state = StateWith(2);
            state.FollowAllFondos();

            state.InitMatrix(2, 2);
            state.FondoMatrices.Add(state.SnapshotWorking(48.0, 0.0)); // the rack grows to 3
            state.SyncTargetFondos();

            Assert.Equal(new[] { 0, 1, 2 }, state.TargetFondos.Fondos);
        }

        [Fact]
        public void Todos_DoesNotFollowTheFondoOnScreen()
        {
            var state = StateWith(3);
            state.FollowAllFondos();
            state.SelectFondo(2);

            Assert.Equal(SelectiveTargetMode.All, state.TargetMode);
            Assert.Equal(new[] { 0, 1, 2 }, state.TargetFondos.Fondos);
        }

        [Fact]
        public void TodosIsStoredAsTheIntent_NotAsTheIndicesItResolvedTo()
        {
            // The editor shows it as "Todos", so storing the set would make the caption and the remembered value
            // disagree the moment the next rack has a different number of fondos.
            var state = StateWith(3);
            state.FollowAllFondos();

            Assert.Equal("Todos", SelectiveTargetPreference.Capture(state).Encode());
        }

        // ---- The preference travels with the OTHER user settings, and only there ----

        [Fact]
        public void ThePreferenceRoundTripsThroughTheRealSettingsFile()
        {
            var path = Path.Combine(Path.GetTempPath(), "rackcad-pref-" + Path.GetRandomFileName() + ".json");
            try
            {
                UserSettingsStore.Save(new UserSettings { SelectiveTargetFondos = "1,3" }, path);
                var loaded = UserSettingsStore.Load(path);

                Assert.Equal("1,3", loaded.SelectiveTargetFondos);
                Assert.Equal(new[] { 1, 3 }, SelectiveTargetPreference.Decode(loaded.SelectiveTargetFondos).Fondos.ToArray());
            }
            finally
            {
                File.Delete(path);
            }
        }

        [Fact]
        public void AnOlderSettingsFileWithoutThePreference_StillLoads()
        {
            var path = Path.Combine(Path.GetTempPath(), "rackcad-pref-" + Path.GetRandomFileName() + ".json");
            try
            {
                File.WriteAllText(path, "{ \"BlockLibraryPath\": \"C:\\\\lib\\\\blocks.dwg\" }");
                var loaded = UserSettingsStore.Load(path);

                Assert.Equal(@"C:\lib\blocks.dwg", loaded.BlockLibraryPath);
                Assert.Null(loaded.SelectiveTargetFondos);                                        // additive, not required
                Assert.Equal(SelectiveTargetMode.All, SelectiveTargetPreference.Decode(loaded.SelectiveTargetFondos).Mode);
            }
            finally
            {
                File.Delete(path);
            }
        }
    }
}
