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
    /// I-46 (ID12) — CHARACTERIZATION, not a contract. Freezes what the tope family does TODAY for 2 and 3 fondos
    /// across <c>TopeShared</c> × <c>Side</c> × <c>TopeFondo</c> (automatic and explicit), so the model change of I-46
    /// can prove which cells it preserves and which it deliberately moves. Every expectation here is a MEASUREMENT of
    /// the current build, not a statement of what the behaviour ought to be.
    /// <para>
    /// The split that matters, and it is the binding rule of I-46, not a guess: a cell only moves when the chosen
    /// fondo has NO fondo after it (<c>c + 1</c> does not exist), which is where "Derecha" has nowhere to go today.
    /// Everything with a REAL GAP is historic behaviour that the fix PRESERVES — including the shared tope, whose
    /// single piece at LOW with a dormant Side is correct and must not be read as ID12.
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
        // Cells marked BROKEN are the ones I-46 will move; every other cell must survive the fix untouched.
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
        // TopeFondo = the LAST fondo: no fondo follows it, so "Derecha" has nowhere to go. BROKEN — this is ID12.
        [InlineData(2, 1, SafetySide.None, "")]
        [InlineData(2, 1, SafetySide.Left, "95.125")]
        [InlineData(2, 1, SafetySide.Right, "")]
        [InlineData(2, 1, SafetySide.Both, "95.125")]
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
        // TopeFondo = the LAST fondo again. BROKEN, same shape as the 2-fondo case.
        [InlineData(3, 2, SafetySide.None, "")]
        [InlineData(3, 2, SafetySide.Left, "149.125")]
        [InlineData(3, 2, SafetySide.Right, "")]
        [InlineData(3, 2, SafetySide.Both, "149.125")]
        public void PerFondo_Today_PlacesAtTheCentralGap(int fondos, int topeFondo, SafetySide side, string expected)
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

        // ---- Shared mode on the LAST fondo (no c + 1): today it still collapses, and THIS one is ID12 ----
        // With no gap there is nothing to share, so the binding rule makes TopeShared inert here: Left → the fondo's
        // FRONT post, Right → its BACK post, Both → the two. These rows therefore WILL move when the fix lands.
        [Theory]
        [InlineData(2, 1, SafetySide.Left, "95.125")]  // → will become 54.875 (front of f1)
        [InlineData(2, 1, SafetySide.Right, "95.125")] // → stays 95.125 (back of f1)
        [InlineData(2, 1, SafetySide.Both, "95.125")]  // → will become 54.875 + 95.125
        [InlineData(3, 2, SafetySide.Left, "149.125")] // → will become 108.875 (front of f2)
        [InlineData(3, 2, SafetySide.Right, "149.125")]// → stays 149.125 (back of f2)
        [InlineData(3, 2, SafetySide.Both, "149.125")] // → will become 108.875 + 149.125
        public void Shared_OnTheLastFondo_Today_StillCollapses(int fondos, int topeFondo, SafetySide side, string expected)
        {
            var system = Rack(fondos, shared: true, topeFondo: topeFondo, side: side);

            Assert.Equal(Parse(expected), TopeXs(system));
            Assert.Equal(1, TopePieces(system));
        }

        // ---- Shared + "Ninguno": the PLAN still resolves a spot, and only the downstream drawable gate hides it ----
        // The plan documents itself as "the physical topes, resolved once", so this disagreement is a defect of the
        // plan, not of its consumers. Pinned here because the fix must remove it without disturbing the consumers.
        [Theory]
        [InlineData(2)]
        [InlineData(3)]
        public void Shared_Ninguno_Today_LeavesAStaleSpotInThePlan(int fondos)
        {
            var system = Rack(fondos, shared: true, topeFondo: -1, side: SafetySide.None);

            Assert.Single(SelectiveTopePlan.Build(system, Catalog)); // the plan says one physical tope...
            Assert.Empty(TopeXs(system));                            // ...and nothing is drawn
            Assert.Equal(0, TopePieces(system));                     // ...and nothing is billed
        }
    }
}
