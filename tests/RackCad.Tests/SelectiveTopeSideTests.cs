using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Catalogs;
using RackCad.Application.Drawing;
using RackCad.Application.Systems.Selective;
using RackCad.Domain.Systems.Selective;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>
    /// I-46 (ID12) — REPRODUCCION. The tope de tarima must be orderable BY SIDE, and the side must show up in the
    /// PHYSICAL result: the pieces the BOM counts and the positions the drawing places, not the intent stored in the
    /// document. The rack under test has ONE fondo, which is where today's model cannot express the axis at all:
    /// <see cref="SelectiveSafetyPlacement.TopeSpots"/> reads the side as WHICH FONDO of the central pair carries the
    /// piece (Left = fondo c's back post, Right = fondo c+1's FRONT post, and the latter only when
    /// <c>c + 1 &lt; fondoCount</c>), so with a single fondo the second position simply does not exist.
    /// <para>
    /// The owner's contract, verified here: <c>Ninguno</c> = 0 pieces, <c>Izquierda</c> = 1, <c>Derecha</c> = 1 at the
    /// OPPOSITE position, <c>Ambas</c> = 2. These tests are RED on purpose until the model of I-46 lands; the failure
    /// IS the evidence the bugfix rule of AGENTS.md demands.
    /// </para>
    /// <para>
    /// The side is only consulted in the PER-FONDO mode, so every case here sets <c>TopeShared = false</c>: with the
    /// shared central tope the family yields one position before looking at the side at all.
    /// </para>
    /// </summary>
    public class SelectiveTopeSideTests
    {
        private const string PostId = TestCatalogIds.Profiles.Posts.Standard;
        private const string BeamId = TestCatalogIds.Profiles.Beams.SelectiveThreeRivet;
        private const string TopeId = TestCatalogIds.Safety.Stops.Post;

        private static RackCatalog Catalog => JsonRackCatalogProvider.FromBaseDirectory().Load();

        /// <summary>A rack with ONE fondo, one frente and one larguero level: the smallest shape in which a tope has
        /// exactly one piece per position, so a piece count reads as a position count. With a single fondo there is
        /// nothing to share, so <paramref name="shared"/> must not change any result (G3).</summary>
        private static SelectiveRackSystem OneFondoRack(SafetySide side, bool shared = false)
        {
            var design = new SelectivePalletDesign
            {
                PostId = PostId, PostPeralte = 3.0, PalletTolerance = 4.0, VerticalClearance = 6.0, PalletDepth = 48.0
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
                ElementId = TopeId, Quantity = 1, Side = side, TopeShared = shared
            });

            var system = new SelectiveGeometryResolver().Resolve(design, Catalog);
            Assert.Equal(1, SelectiveDepthLayout.Count(system)); // the premise of every case below
            return system;
        }

        /// <summary>The PHYSICAL pieces the BOM bills for the tope family (its quantity, across every length group).</summary>
        private static int TopePieces(SelectiveRackSystem system)
            => SelectiveBomBuilder.Build(system, Catalog).Components
                .Where(component => component.Category == SelectiveBomBuilder.Tope)
                .Sum(component => component.Quantity);

        /// <summary>The DISTINCT depth positions the LATERAL draws a tope at. A corte draws each physical piece it
        /// bounds, so the same piece appears on both cortes of a single frente: the distinct X is the position.</summary>
        private static IReadOnlyList<double> TopeXs(SelectiveRackSystem system)
            => new SelectiveLateralBuilder().Cortes(system, Catalog)
                .SelectMany(corte => corte.Largueros)
                .Where(instance => instance.Role == HeaderBlockRole.Tope)
                .Select(instance => System.Math.Round(instance.Insertion.X, 4))
                .Distinct()
                .OrderBy(x => x)
                .ToList();

        // ---- The four cases of ID12, counted on the physical result of a rack with ONE fondo ----
        [Theory]
        [InlineData(SafetySide.None, 0)]
        [InlineData(SafetySide.Left, 1)]
        [InlineData(SafetySide.Right, 1)]
        [InlineData(SafetySide.Both, 2)]
        public void OneFondo_EverySide_BillsItsOwnPieces(SafetySide side, int expected)
            => Assert.Equal(expected, TopePieces(OneFondoRack(side)));

        // ---- ...and the drawing agrees with the BOM, piece for piece ----
        [Theory]
        [InlineData(SafetySide.None, 0)]
        [InlineData(SafetySide.Left, 1)]
        [InlineData(SafetySide.Right, 1)]
        [InlineData(SafetySide.Both, 2)]
        public void OneFondo_EverySide_DrawsItsOwnPositions(SafetySide side, int expected)
            => Assert.Equal(expected, TopeXs(OneFondoRack(side)).Count);

        // ---- "Derecha" is the OPPOSITE position, not a second copy of "Izquierda" nor an absent piece ----
        [Fact]
        public void OneFondo_RightSitsOppositeLeft_AndBothIsTheTwoOfThem()
        {
            var left = Assert.Single(TopeXs(OneFondoRack(SafetySide.Left)));
            var right = Assert.Single(TopeXs(OneFondoRack(SafetySide.Right)));

            // Opposite ends of the same fondo: the two positions must not collapse onto one another.
            Assert.NotEqual(left, right);

            // "Ambas" is exactly the union of the two single-sided results — it adds no third position and drops none.
            Assert.Equal(
                new List<double> { left, right }.OrderBy(x => x).ToList(),
                TopeXs(OneFondoRack(SafetySide.Both)));
        }

        // ================================ Gate 3 — the owner's fixed contract ================================
        // "Izquierda = extremo BAJO del marco local del rack; Derecha = extremo ALTO; Ambas = union exacta; nunca
        // World X." The frame is the rack's own DEPTH axis, where SelectiveDepthLayout puts the frontmost post at
        // offset 0 and every further fondo above it, so LOW/HIGH are read off that axis and never off world coords.

        /// <summary>The rack's local depth extent: the frontmost post (always 0) and the backmost post across fondos.</summary>
        private static (double Low, double High) LocalDepthExtent(SelectiveRackSystem system)
        {
            var offsets = SelectiveDepthLayout.Offsets(system);
            var low = offsets.Min();
            var high = offsets.Select((o, k) => o + SelectiveDepthLayout.CabeceraDepthOfFondo(system, k)).Max();
            return (low, high);
        }

        // ---- Izquierda is the LOW end and Derecha the HIGH end, ordered on the rack's own depth axis ----
        [Fact]
        public void OneFondo_LeftIsTheLowEnd_AndRightIsTheHighEnd()
        {
            var left = Assert.Single(TopeXs(OneFondoRack(SafetySide.Left)));
            var right = Assert.Single(TopeXs(OneFondoRack(SafetySide.Right)));
            var (low, high) = LocalDepthExtent(OneFondoRack(SafetySide.Both));

            // Ordered on the local axis: the LOW-end piece sits below the HIGH-end one, never the other way round.
            Assert.True(left < right, $"Izquierda ({left}) debe quedar por debajo de Derecha ({right}) en el eje local");

            // ...and each one belongs to its own end of the rack, not both to the same post.
            Assert.True(left < (low + high) / 2.0, $"Izquierda ({left}) debe caer en la mitad BAJA de [{low}, {high}]");
            Assert.True(right > (low + high) / 2.0, $"Derecha ({right}) debe caer en la mitad ALTA de [{low}, {high}]");
        }

        // ---- With ONE fondo there is nothing to share, so "Compartido" must not change a single result ----
        [Theory]
        [InlineData(SafetySide.None, 0)]
        [InlineData(SafetySide.Left, 1)]
        [InlineData(SafetySide.Right, 1)]
        [InlineData(SafetySide.Both, 2)]
        public void OneFondo_Shared_BillsExactlyTheSameAsPerFondo(SafetySide side, int expected)
        {
            Assert.Equal(expected, TopePieces(OneFondoRack(side, shared: true)));
            Assert.Equal(
                TopeXs(OneFondoRack(side, shared: false)),
                TopeXs(OneFondoRack(side, shared: true)));
        }

        // ---- "Ninguno" must be empty in the PLAN itself, not only in the consumers that happen to gate downstream ----
        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void OneFondo_Ninguno_LeavesThePlanEmpty(bool shared)
            => Assert.Empty(SelectiveTopePlan.Build(OneFondoRack(SafetySide.None, shared), Catalog));

        // ---- The FRONTAL must bill what it draws: today it ignores the side entirely and draws a piece nobody counts ----
        [Theory]
        [InlineData(SafetySide.None)]
        [InlineData(SafetySide.Left)]
        [InlineData(SafetySide.Right)]
        [InlineData(SafetySide.Both)]
        public void OneFondo_FrontalAgreesWithTheBom(SafetySide side)
        {
            var system = OneFondoRack(side);
            foreach (var selection in system.SafetySelections) selection.TopeFrontal = true;

            var frontal = new SelectiveFrontalBuilder().Build(system, Catalog)
                .Count(instance => instance.Role == HeaderBlockRole.Tope);

            // One frente and one level, so the frontal shows exactly the pieces the BOM bills — no more, no fewer.
            Assert.Equal(TopePieces(system), frontal);
        }
    }
}
