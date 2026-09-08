using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using RackCad.Application.Catalogs;
using RackCad.Application.Drawing;
using RackCad.Application.Systems.Selective;
using RackCad.Domain.Systems.Selective;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>
    /// I-46 (ID12) — CHARACTERIZATION, not a contract. Measures the tope family for 2 and 3 fondos across
    /// <c>TopeShared</c> × <c>Side</c> × <c>TopeFondo</c> (automatic and explicit). It was written BEFORE the model
    /// change, to freeze the build as it stood, and is the ledger of what that change actually moved.
    /// <para>
    /// Of its rows, <b>12 moved and every one of them says so and carries its pre-fix value</b>: the six per-fondo and
    /// four shared rows whose <c>TopeFondo</c> is the LAST fondo (no <c>c + 1</c>, so the span degenerates — rule 4),
    /// plus the two that pinned the stale spot the plan used to resolve for "Ninguno" (rule 7, which changed no drawn
    /// or billed piece). <b>Everything with a REAL GAP is untouched</b>, above all
    /// <see cref="Shared_WithARealGap_KeepsTheHistoricSinglePieceAtLow"/>: the historic shared tope — one piece at LOW
    /// with a dormant Side — was never ID12 and the fix preserves it verbatim.
    /// </para>
    /// <para>
    /// Positions are the tope mate X on the rack's own DEPTH axis (<see cref="SelectiveDepthLayout.Offsets"/> puts the
    /// frontmost post at 0), never a world coordinate. With <c>PalletDepth = 48</c> the cabecera is 42" deep and the
    /// fondos sit 54" apart, so the posts are f0[0, 42], f1[54, 96], f2[108, 150] and the TROQUEL_TOPE mate is 0.875"
    /// inside each: a BACK post mates at <c>back − 0.875</c> and a FRONT post at <c>front + 0.875</c>.
    /// </para>
    /// </summary>
    public class SelectiveTopeMultiFondoCharacterizationTests
    {
        private const string PostId = TestCatalogIds.Profiles.Posts.Standard;
        private const string BeamId = TestCatalogIds.Profiles.Beams.SelectiveThreeRivet;
        private const string TopeId = TestCatalogIds.Safety.Stops.Post;

        private static RackCatalog Catalog => JsonRackCatalogProvider.FromBaseDirectory().Load();

        private static SelectiveRackSystem Rack(int fondos, bool shared, int topeFondo, SafetySide side)
        {
            var design = new SelectivePalletDesign
            {
                PostId = PostId, PostPeralte = 3.0, PalletTolerance = 4.0, VerticalClearance = 6.0,
                PalletDepth = 48.0, DepthCount = fondos
            };
            var bay = new SelectiveBayDesign { FloorBeam = true };
            bay.Levels.Add(new SelectiveCell
            {
                Pallet = new Tarima { Frente = 40.0, Alto = 45.0 },
                PalletCount = 2, BeamId = BeamId, BeamPeralte = 4.0
            });
            design.Bays.Add(bay);
            design.SafetySelections.Add(new SelectiveSafetySelection
            {
                ElementId = TopeId, Quantity = 1, Side = side, TopeShared = shared, TopeFondo = topeFondo
            });
            return new SelectiveGeometryResolver().Resolve(design, Catalog);
        }

        private static IReadOnlyList<double> TopeXs(SelectiveRackSystem system)
            => new SelectiveLateralBuilder().Cortes(system, Catalog)
                .SelectMany(corte => corte.Largueros)
                .Where(instance => instance.Role == HeaderBlockRole.Tope)
                .Select(instance => System.Math.Round(instance.Insertion.X, 3))
                .Distinct()
                .OrderBy(x => x)
                .ToList();

        private static int TopePieces(SelectiveRackSystem system)
            => SelectiveBomBuilder.Build(system, Catalog).Components
                .Where(component => component.Category == SelectiveBomBuilder.Tope)
                .Sum(component => component.Quantity);

        /// <summary>Parses the expected position list ("41.125,54.875", or "" for none) written inline per case.</summary>
        private static IReadOnlyList<double> Parse(string xs)
            => string.IsNullOrEmpty(xs)
                ? new List<double>()
                : xs.Split(',').Select(v => double.Parse(v, CultureInfo.InvariantCulture)).ToList();

        // ---- The geometry every expectation below is read against; if this moves, the numbers move with it ----
        [Theory]
        [InlineData(2, "0,42,54,96")]
        [InlineData(3, "0,42,54,96,108,150")]
        public void LocalDepthFrame_PutsTheFrontmostPostAtZero(int fondos, string expected)
        {
            var system = Rack(fondos, shared: false, topeFondo: -1, side: SafetySide.Both);
            var posts = new List<double>();
            var offsets = SelectiveDepthLayout.Offsets(system);
            for (var k = 0; k < offsets.Count; k++)
            {
                posts.Add(offsets[k]);
                posts.Add(offsets[k] + SelectiveDepthLayout.CabeceraDepthOfFondo(system, k));
            }

            Assert.Equal(Parse(expected), posts);
        }

        // ---- 2 and 3 fondos, per-fondo mode (TopeShared = false): the side picks a post of the CENTRAL GAP ----
        // Only the rows whose TopeFondo is the LAST fondo moved with the fix; each says so and carries its old value.
        [Theory]
        // 2 fondos, central gap = f0's back (41.125) ↔ f1's front (54.875). Automatic central fondo = 0.
        [InlineData(2, -1, SafetySide.None, "")]
        [InlineData(2, -1, SafetySide.Left, "41.125")]
        [InlineData(2, -1, SafetySide.Right, "54.875")]
        [InlineData(2, -1, SafetySide.Both, "41.125,54.875")]
        [InlineData(2, 0, SafetySide.None, "")]
        [InlineData(2, 0, SafetySide.Left, "41.125")]
        [InlineData(2, 0, SafetySide.Right, "54.875")]
        [InlineData(2, 0, SafetySide.Both, "41.125,54.875")]
        // TopeFondo = the LAST fondo: no gap follows, so the span degenerates to f1's own frame (rule 4). These four
        // rows MOVED with the fix — before it they read "" / 95.125 / "" / 95.125, with "Derecha" drawing nothing.
        [InlineData(2, 1, SafetySide.None, "")]
        [InlineData(2, 1, SafetySide.Left, "54.875")]
        [InlineData(2, 1, SafetySide.Right, "95.125")]
        [InlineData(2, 1, SafetySide.Both, "54.875,95.125")]
        // 3 fondos, automatic central fondo = 1, gap = f1's back (95.125) ↔ f2's front (108.875).
        [InlineData(3, -1, SafetySide.None, "")]
        [InlineData(3, -1, SafetySide.Left, "95.125")]
        [InlineData(3, -1, SafetySide.Right, "108.875")]
        [InlineData(3, -1, SafetySide.Both, "95.125,108.875")]
        [InlineData(3, 0, SafetySide.Left, "41.125")]
        [InlineData(3, 0, SafetySide.Right, "54.875")]
        [InlineData(3, 0, SafetySide.Both, "41.125,54.875")]
        [InlineData(3, 1, SafetySide.Left, "95.125")]
        [InlineData(3, 1, SafetySide.Right, "108.875")]
        [InlineData(3, 1, SafetySide.Both, "95.125,108.875")]
        // TopeFondo = the LAST fondo again, same shape. Before the fix: "" / 149.125 / "" / 149.125.
        [InlineData(3, 2, SafetySide.None, "")]
        [InlineData(3, 2, SafetySide.Left, "108.875")]
        [InlineData(3, 2, SafetySide.Right, "149.125")]
        [InlineData(3, 2, SafetySide.Both, "108.875,149.125")]
        public void PerFondo_PlacesAtTheCentralGap_OrAtTheFondosOwnEnds(int fondos, int topeFondo, SafetySide side, string expected)
        {
            var system = Rack(fondos, shared: false, topeFondo: topeFondo, side: side);
            var positions = Parse(expected);

            Assert.Equal(positions, TopeXs(system));
            Assert.Equal(positions.Count, TopePieces(system)); // the BOM bills exactly the drawn positions
        }

        // ---- Shared mode WITH A REAL GAP (c + 1 exists): ONE shared piece at LOW, and Side is DORMANT there ----
        // This is NOT ID12 and NOT a defect: it is the historic shared tope, and the binding rule of I-46 PRESERVES
        // it exactly. Every row below must keep passing after the fix — see the contract sentinel in
        // SelectiveTopeSideMultiFondoContractTests, which pins the same behaviour as a rule instead of a measurement.
        [Theory]
        [InlineData(2, -1, SafetySide.Left, "41.125")]
        [InlineData(2, -1, SafetySide.Right, "41.125")]
        [InlineData(2, -1, SafetySide.Both, "41.125")]
        [InlineData(2, 0, SafetySide.Right, "41.125")]
        [InlineData(3, -1, SafetySide.Left, "95.125")]
        [InlineData(3, -1, SafetySide.Right, "95.125")]
        [InlineData(3, -1, SafetySide.Both, "95.125")]
        [InlineData(3, 0, SafetySide.Right, "41.125")]
        [InlineData(3, 0, SafetySide.Both, "41.125")]
        [InlineData(3, 1, SafetySide.Right, "95.125")]
        public void Shared_WithARealGap_KeepsTheHistoricSinglePieceAtLow(int fondos, int topeFondo, SafetySide side, string expected)
        {
            var system = Rack(fondos, shared: true, topeFondo: topeFondo, side: side);

            Assert.Equal(Parse(expected), TopeXs(system));
            Assert.Equal(1, TopePieces(system));
        }

        // ---- Shared mode on the LAST fondo (no c + 1): nothing to share, so TopeShared is INERT (rule 4) ----
        // This is the half of ID12 seen from the shared side, and it MOVED with the fix. Before it, every row below
        // collapsed onto the fondo's BACK post and billed exactly one piece.
        [Theory]
        [InlineData(2, 1, SafetySide.Left, "54.875")]          // was 95.125 (the back post)
        [InlineData(2, 1, SafetySide.Right, "95.125")]         // unchanged
        [InlineData(2, 1, SafetySide.Both, "54.875,95.125")]   // was one piece at 95.125
        [InlineData(3, 2, SafetySide.Left, "108.875")]         // was 149.125
        [InlineData(3, 2, SafetySide.Right, "149.125")]        // unchanged
        [InlineData(3, 2, SafetySide.Both, "108.875,149.125")] // was one piece at 149.125
        public void Shared_OnTheLastFondo_IsInert(int fondos, int topeFondo, SafetySide side, string expected)
        {
            var system = Rack(fondos, shared: true, topeFondo: topeFondo, side: side);
            var positions = Parse(expected);

            Assert.Equal(positions, TopeXs(system));
            Assert.Equal(positions.Count, TopePieces(system));

            // Inert means indistinguishable: the per-fondo mode must give exactly the same thing.
            Assert.Equal(TopeXs(Rack(fondos, shared: false, topeFondo: topeFondo, side: side)), TopeXs(system));
        }

        // ---- Shared + "Ninguno": the PLAN itself is empty (rule 7) ----
        // Before the fix the plan resolved a STALE spot here and only the downstream drawable gate hid it, so the
        // plan disagreed with its own documentation. The drawn and billed results below never changed.
        [Theory]
        [InlineData(2)]
        [InlineData(3)]
        public void Shared_Ninguno_LeavesThePlanEmpty(int fondos)
        {
            var system = Rack(fondos, shared: true, topeFondo: -1, side: SafetySide.None);

            Assert.Empty(SelectiveTopePlan.Build(system, Catalog)); // was: exactly one stale spot
            Assert.Empty(TopeXs(system));                           // unchanged: nothing drawn
            Assert.Equal(0, TopePieces(system));                    // unchanged: nothing billed
        }
    }
}
