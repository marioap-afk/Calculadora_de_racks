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
        /// exactly one piece per position, so a piece count reads as a position count.</summary>
        private static SelectiveRackSystem OneFondoRack(SafetySide side)
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
                ElementId = TopeId, Quantity = 1, Side = side, TopeShared = false
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
    }
}
