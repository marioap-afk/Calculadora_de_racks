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
    /// I-46 (ID12) — the BINDING RULE of the tope's side, on racks with 2 and 3 fondos. Unlike
    /// <see cref="SelectiveTopeMultiFondoCharacterizationTests"/>, which measures what the build does today, every
    /// expectation here states what the rule REQUIRES:
    /// <list type="number">
    /// <item>resolve <c>c</c> = a valid <c>TopeFondo</c>, else <c>CentralFondo</c>;</item>
    /// <item><c>Side = None</c> ⇒ zero spots, always;</item>
    /// <item>when <c>c + 1</c> EXISTS the reference span is the central gap — <c>LOW = back(c)</c>,
    /// <c>HIGH = front(c + 1)</c> — and with <c>TopeShared = true</c> there is ONE shared piece at <c>LOW</c> for any
    /// non-None Side, with Side DORMANT. That is historic behaviour and it does not change;</item>
    /// <item>when <c>c + 1</c> does NOT exist the span degenerates to the fondo itself — <c>LOW = front(c)</c>,
    /// <c>HIGH = back(c)</c> — and <c>TopeShared</c> is INERT: Left = LOW, Right = HIGH, Both = both.</item>
    /// </list>
    /// Positions are read on the rack's own depth axis (<see cref="SelectiveDepthLayout.Offsets"/> puts the frontmost
    /// post at 0), never on a world coordinate. The degenerate cases below are RED until the model lands; the shared
    /// sentinel is GREEN today and must STAY green — it is the guard against "fixing" what was never broken.
    /// </summary>
    public class SelectiveTopeSideMultiFondoContractTests
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

        private static IReadOnlyList<double> Parse(string xs)
            => string.IsNullOrEmpty(xs)
                ? new List<double>()
                : xs.Split(',').Select(v => double.Parse(v, CultureInfo.InvariantCulture)).ToList();

        /// <summary>The mate X of the FRONT and BACK post of fondo <paramref name="k"/> on the local depth axis.</summary>
        private static (double Front, double Back) Posts(SelectiveRackSystem system, int k)
        {
            var offsets = SelectiveDepthLayout.Offsets(system);
            var troquel = CatalogLookup.Local(
                Catalog, system.PostId, SelectiveSafetyPlacement.TopePostPoint, SelectiveLateralBuilder.LateralView);
            var front = offsets[k];
            var back = front + SelectiveDepthLayout.CabeceraDepthOfFondo(system, k);
            return (front + troquel.X, back - troquel.X);
        }

        // ================= Rule 4: TopeFondo pointing at the LAST fondo — the span degenerates =================
        // "Derecha" today has nowhere to go there, so these are RED. TopeShared is INERT: both modes must agree.

        [Theory]
        [InlineData(2, 1, false)]
        [InlineData(2, 1, true)]
        [InlineData(3, 2, false)]
        [InlineData(3, 2, true)]
        public void LastFondo_Left_IsTheFondosOwnFrontPost(int fondos, int last, bool shared)
        {
            var system = Rack(fondos, shared, topeFondo: last, side: SafetySide.Left);

            Assert.Equal(new[] { Posts(system, last).Front }, TopeXs(system));
            Assert.Equal(1, TopePieces(system));
        }

        [Theory]
        [InlineData(2, 1, false)]
        [InlineData(2, 1, true)]
        [InlineData(3, 2, false)]
        [InlineData(3, 2, true)]
        public void LastFondo_Right_IsTheFondosOwnBackPost(int fondos, int last, bool shared)
        {
            var system = Rack(fondos, shared, topeFondo: last, side: SafetySide.Right);

            Assert.Equal(new[] { Posts(system, last).Back }, TopeXs(system));
            Assert.Equal(1, TopePieces(system));
        }

        [Theory]
        [InlineData(2, 1, false)]
        [InlineData(2, 1, true)]
        [InlineData(3, 2, false)]
        [InlineData(3, 2, true)]
        public void LastFondo_Both_IsTheExactUnionOfTheTwoEnds(int fondos, int last, bool shared)
        {
            var system = Rack(fondos, shared, topeFondo: last, side: SafetySide.Both);
            var (front, back) = Posts(system, last);

            Assert.Equal(new[] { front, back }, TopeXs(system));
            Assert.Equal(2, TopePieces(system)); // "Ambas" bills both ends, never one
        }

        // ---- ...and with no gap there is nothing to share, so the two modes must be indistinguishable ----
        [Theory]
        [InlineData(2, 1, SafetySide.Left)]
        [InlineData(2, 1, SafetySide.Right)]
        [InlineData(2, 1, SafetySide.Both)]
        [InlineData(3, 2, SafetySide.Left)]
        [InlineData(3, 2, SafetySide.Right)]
        [InlineData(3, 2, SafetySide.Both)]
        public void LastFondo_SharedIsInert(int fondos, int last, SafetySide side)
        {
            Assert.Equal(
                TopeXs(Rack(fondos, shared: false, topeFondo: last, side: side)),
                TopeXs(Rack(fondos, shared: true, topeFondo: last, side: side)));
        }

        // ================= Rule 3: with a REAL GAP the shared tope is HISTORIC — the green sentinel =================
        // One piece at LOW = back(c) for every non-None Side, because Side is DORMANT in that state. This passes
        // today and must keep passing: it is what stops the fix from "correcting" behaviour that was never ID12.

        [Theory]
        [InlineData(2, -1, 0, SafetySide.Left)]
        [InlineData(2, -1, 0, SafetySide.Right)]
        [InlineData(2, -1, 0, SafetySide.Both)]
        [InlineData(2, 0, 0, SafetySide.Right)]
        [InlineData(3, -1, 1, SafetySide.Left)]
        [InlineData(3, -1, 1, SafetySide.Right)]
        [InlineData(3, -1, 1, SafetySide.Both)]
        [InlineData(3, 0, 0, SafetySide.Right)]
        [InlineData(3, 1, 1, SafetySide.Both)]
        public void Shared_WithARealGap_IsOneHistoricPieceAtLow_AndTheSideIsDormant(
            int fondos, int topeFondo, int resolvedC, SafetySide side)
        {
            var system = Rack(fondos, shared: true, topeFondo: topeFondo, side: side);

            // LOW of the central gap = the BACK post of the resolved fondo c, whatever the side asked for.
            Assert.Equal(new[] { Posts(system, resolvedC).Back }, TopeXs(system));
            Assert.Equal(1, TopePieces(system));
        }

        // ---- Rule 2/7: "Ninguno" is zero spots in the PLAN itself, in both modes and at any fondo ----
        [Theory]
        [InlineData(2, -1, false)]
        [InlineData(2, -1, true)]
        [InlineData(2, 1, true)]
        [InlineData(3, -1, true)]
        [InlineData(3, 2, true)]
        public void Ninguno_IsZeroSpots_InThePlan(int fondos, int topeFondo, bool shared)
        {
            var system = Rack(fondos, shared, topeFondo, SafetySide.None);

            Assert.Empty(SelectiveTopePlan.Build(system, Catalog));
            Assert.Empty(TopeXs(system));
            Assert.Equal(0, TopePieces(system));
        }
    }
}
