using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Catalogs;
using RackCad.Application.Persistence;
using RackCad.Domain.Systems.Selective;
using RackCad.UI;
using Xunit;

namespace RackCad.UI.Tests
{
    /// <summary>
    /// I-46 (ID12) — the tope's side across the EDITOR round trip: what RACKEDITAR loads into the safety window, what
    /// the window gives back when the user accepts without touching anything, and what survives a save and a reopen.
    /// Before I-46 a stored <c>Ninguno</c> could not make this trip: the window rewrote it to <c>Ambas</c> while
    /// building the row, so the intent died the first time the editor was opened.
    /// <para>
    /// The window is driven through <c>BuildResultForTest</c>, the existing non-modal seam, and the selections it is
    /// handed are the ones a design carries — so this exercises the real load/commit path, not a stand-in.
    /// </para>
    /// </summary>
    public sealed class SelectiveTopeSideEditorFlowTests
    {
        private const string TopeId = "POSTE_3_1_5_8_TOPE";

        private static RackCatalog Catalog => JsonRackCatalogProvider.FromBaseDirectory().Load();

        private static SelectiveSafetySelection Tope(SafetySide side, bool shared = false, int fondo = -1)
            => new SelectiveSafetySelection
            {
                ElementId = TopeId, Quantity = 1, Side = side, TopeShared = shared, TopeFondo = fondo, TopeSaque = 6.0
            };

        /// <summary>The safety window as the Selectivo host opens it, seeded with <paramref name="current"/>.</summary>
        private static SelectiveSafetyWindow Window(IEnumerable<SelectiveSafetySelection> current, int fondoCount)
            => new SelectiveSafetyWindow(
                Catalog?.SafetyElements ?? new List<SafetyElementCatalogEntry>(),
                current.ToList(),
                postCount: 3,
                levelsPerFrente: new[] { 2, 2 },
                fondoCount: fondoCount,
                parrillaPlan: null,
                catalog: Catalog,
                resolvedSystem: null);

        /// <summary>Open the window on <paramref name="current"/>, accept WITHOUT touching a control, return the tope.</summary>
        private static SelectiveSafetySelection AcceptUnchanged(SelectiveSafetySelection current, int fondoCount)
            => StaTestRunner.Run(() =>
            {
                var window = Window(new[] { current }, fondoCount);
                window.BuildResultForTest();
                return window.Result.Single(selection => selection.ElementId == TopeId);
            });

        // ---- Loading a design reconstructs EXACTLY the stored side, and accepting untouched keeps it ----
        [Theory]
        [InlineData(SafetySide.None, 1)]
        [InlineData(SafetySide.Left, 1)]
        [InlineData(SafetySide.Right, 1)]
        [InlineData(SafetySide.Both, 1)]
        [InlineData(SafetySide.None, 3)]
        [InlineData(SafetySide.Left, 3)]
        [InlineData(SafetySide.Right, 3)]
        [InlineData(SafetySide.Both, 3)]
        public void LoadingADesign_AndAcceptingUnchanged_KeepsTheSide(SafetySide side, int fondoCount)
            => Assert.Equal(side, AcceptUnchanged(Tope(side), fondoCount).Side);

        // ---- ...and it keeps the tope's neighbours too, so "unchanged" means unchanged ----
        [Theory]
        [InlineData(1)]
        [InlineData(3)]
        public void AcceptingUnchanged_KeepsSharedAndFondoAlongsideTheSide(int fondoCount)
        {
            var restored = AcceptUnchanged(Tope(SafetySide.Right, shared: true, fondo: 0), fondoCount);

            Assert.Equal(SafetySide.Right, restored.Side);
            Assert.True(restored.TopeShared);
            Assert.Equal(0, restored.TopeFondo);
            Assert.Equal(6.0, restored.TopeSaque, 6);
        }

        // ---- Save, reopen, and open the editor again: the side is still the one that was stored ----
        [Theory]
        [InlineData(SafetySide.None)]
        [InlineData(SafetySide.Left)]
        [InlineData(SafetySide.Right)]
        [InlineData(SafetySide.Both)]
        public void SavingAndReopening_KeepsTheSideThroughTheEditor(SafetySide side)
        {
            var design = new SelectivePalletDesign { PostId = "P", PostPeralte = 3.0, DepthCount = 3 };
            design.Bays.Add(new SelectiveBayDesign());
            design.SafetySelections.Add(Tope(side, shared: false, fondo: 1));

            var store = new SelectivePalletDesignStore();
            var reopened = store
                .Deserialize(store.Serialize(SelectivePalletDesignDocument.From(design, "id-46", "Rack")))
                .ToDomain();

            // The design came back from disk...
            var stored = reopened.SafetySelections.Single(selection => selection.ElementId == TopeId);
            Assert.Equal(side, stored.Side);

            // ...and re-opening the editor on it, then accepting, still yields the same side and fondo.
            var committed = AcceptUnchanged(stored, fondoCount: 3);
            Assert.Equal(side, committed.Side);
            Assert.Equal(1, committed.TopeFondo);
        }

        // ---- Every side is REACHABLE from the row the editor loaded: the dialog opens on it and returns it ----
        // This is the hop the window performs when the user presses "Configurar…": the grid dialog is seeded from the
        // row's current side. Changing it is covered by SelectiveTopeSideOptionsTests.PickingAnEntry_YieldsItsOwnSide.
        [Theory]
        [InlineData(SafetySide.None)]
        [InlineData(SafetySide.Left)]
        [InlineData(SafetySide.Right)]
        [InlineData(SafetySide.Both)]
        public void TheGridDialogIsSeededFromTheLoadedSide(SafetySide side)
        {
            var seeded = StaTestRunner.Run(() => new SafetyTopeGridWindow(
                "Tope", new[] { 2 }, shared: false, side: side, saque: 6.0, frontal: false,
                offCells: null, fondoCount: 3, fondo: 1).BuildResult().Side);

            Assert.Equal(side, seeded);
        }
    }
}
