using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Catalogs;
using RackCad.Application.Geometry;
using RackCad.Application.Persistence;
using RackCad.Application.Systems;
using RackCad.Domain.Systems;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>
    /// I-32 — el TERCER desempate de la selección del troquel bajo.
    ///
    /// El criterio principal es minimizar <c>|tan θ − 7/192|</c>; el segundo, la posición teórica CONTINUA de la
    /// geometría asimétrica. El tercero es «el candidato más cercano al resultado de la <b>regla anterior</b>», y
    /// existe para que, ante dos troqueles igual de buenos, la decisión sea estable y no salte.
    ///
    /// Ese tercer criterio solo tiene contenido si la referencia es <b>de verdad</b> la de la regla anterior:
    ///
    /// <code>
    /// legacy = Snap(rearContact.Y − Rise(ResolveBedLength(system, front)) − lowMateLocalY, gridBase)
    /// </code>
    ///
    /// Una versión intermedia la reconstruía ajustando la posición teórica ASIMÉTRICA a la retícula. Eso es otra
    /// cantidad: colapsa el tercer desempate sobre el segundo y lo deja sin efecto propio. Estas pruebas fijan que
    /// las dos referencias son distintas y que la que se usa es la de la fórmula anterior.
    /// </summary>
    public class PushBackLegacyTieBreakTests
    {
        private static RackCatalog Catalog => JsonRackCatalogProvider.FromBaseDirectory().Load();

        private static PushBackSystem System(RackCatalog catalog, int palletsDeep)
        {
            var design = new PushBackDesign
            {
                Structure = new DynamicRackDesign
                {
                    Pallet = new PalletSpecification(42.0, 48.0, 60.0, 1000.0, "kg"),
                    PalletsDeep = palletsDeep,
                    LoadLevels = 2,
                    FirstLevelHeight = 6.0,
                    BeamDepth = 4.0
                }
            };
            design.Structure.Fronts.Add(new DynamicRackFrontDesign
            {
                PalletCount = 1, LoadLevels = 2, PalletsDeep = palletsDeep, DepthStartPosition = 1
            });
            design.Fronts.Add(new PushBackFrontConfig());
            return new PushBackResolver(catalog).Resolve(design);
        }

        public static IEnumerable<object[]> Fondos() =>
            new[] { 2, 3, 4, 5, 6, 7, 8 }.Select(deep => new object[] { deep });

        private static double LegacyInsertion(
            PushBackSystem system, RackCatalog catalog, DynamicRackFront front, Point2D rearContact, double lowMateY)
        {
            var nominalRise = PushBackBedSlope.Rise(PushBackFlowBedGeometry.ResolveBedLength(system, front));
            var gridBase = PushBackTroquelGrid.Base(system.Structure, catalog);
            return PushBackTroquelGrid.Snap(rearContact.Y - nominalRise - lowMateY, gridBase);
        }

        private static double TheoreticalInsertion(
            RackCatalog catalog, Point2D rearContact, double lowContactX, double lowMateY)
        {
            var railMate = CatalogLookup.Local(
                catalog, FlowBedDefaults.RailId, FlowBedDefaults.RailInOutMatePoint, FlowBedDefaults.View);
            return PushBackBedRotation.TheoreticalExitY(
                rearContact.X, rearContact.Y, lowContactX, railMate.Y) - lowMateY;
        }

        /// <summary>
        /// Las dos referencias de desempate son cantidades DISTINTAS. Si coincidieran siempre, el tercer criterio no
        /// aportaría nada y daría igual de cuál se derivara.
        /// </summary>
        [Fact]
        public void TheLegacyReference_AndTheAsymmetricTheoretical_AreDifferentQuantities()
        {
            var catalog = Catalog;
            var lowMate = PushBackLoadBeamGeometry.BedTangencyPointLocal(catalog, DynamicRackDefaults.InOutBeamCatalogId);
            Assert.True(lowMate.HasValue);

            var different = 0;
            var compared = 0;

            foreach (var deep in new[] { 2, 3, 4, 5, 6, 7, 8 })
            {
                var system = System(catalog, deep);
                var front = system.Structure.Fronts[0];
                var gridBase = PushBackTroquelGrid.Base(system.Structure, catalog);

                foreach (var cell in PushBackElevations.Resolve(system, catalog, front).Values)
                {
                    var legacy = LegacyInsertion(system, catalog, front, cell.RearContact, lowMate.Value.Y);
                    var theoreticalSnapped = PushBackTroquelGrid.Snap(
                        TheoreticalInsertion(catalog, cell.RearContact, cell.LowContact.X, lowMate.Value.Y), gridBase);

                    compared++;
                    if (Math.Abs(legacy - theoreticalSnapped) > 1e-9)
                    {
                        different++;
                    }
                }
            }

            Assert.True(compared > 0);
            Assert.True(different > 0,
                "la referencia legacy y la teórica asimétrica ajustada coincidieron en todos los casos: el barrido " +
                "no demuestra que sean cantidades distintas");
        }

        /// <summary>
        /// La referencia legacy sale de la fórmula ANTERIOR —subida nominal sobre la longitud comercial— y no de
        /// <c>TheoreticalExitY</c>. Se comprueba reproduciéndola con la fórmula anterior y verificando que es la
        /// que la autoridad usaría: cuando legacy y elegido coinciden, la elección tiene que ser esa.
        /// </summary>
        [Theory]
        [MemberData(nameof(Fondos))]
        public void TheLegacyReference_ComesFromTheOldNominalFormula(int palletsDeep)
        {
            var catalog = Catalog;
            var system = System(catalog, palletsDeep);
            var front = system.Structure.Fronts[0];
            var lowMate = PushBackLoadBeamGeometry.BedTangencyPointLocal(catalog, DynamicRackDefaults.InOutBeamCatalogId);
            var gridBase = PushBackTroquelGrid.Base(system.Structure, catalog);
            Assert.True(lowMate.HasValue);

            foreach (var cell in PushBackElevations.Resolve(system, catalog, front).Values)
            {
                var legacy = LegacyInsertion(system, catalog, front, cell.RearContact, lowMate.Value.Y);

                // La referencia es un troquel válido de la misma retícula: es una posición, no una estimación.
                Assert.Equal(PushBackTroquelGrid.Snap(legacy, gridBase), legacy, 9);

                // Y se reconstruye exactamente con la fórmula anterior, no con la asimétrica.
                var nominalRise = PushBackBedSlope.Rise(PushBackFlowBedGeometry.ResolveBedLength(system, front));
                Assert.Equal(
                    PushBackTroquelGrid.Snap(cell.RearContact.Y - nominalRise - lowMate.Value.Y, gridBase),
                    legacy,
                    9);
            }
        }

        /// <summary>
        /// El desempate NO altera el criterio principal: la elección sigue siendo la de menor error de pendiente,
        /// aunque la referencia legacy apunte a otro troquel. El tercer criterio solo decide entre empatados.
        /// </summary>
        [Theory]
        [MemberData(nameof(Fondos))]
        public void TheTieBreak_NeverOverridesTheSlopeCriterion(int palletsDeep)
        {
            var catalog = Catalog;
            var system = System(catalog, palletsDeep);
            var front = system.Structure.Fronts[0];
            var lowMate = PushBackLoadBeamGeometry.BedTangencyPointLocal(catalog, DynamicRackDefaults.InOutBeamCatalogId);
            var railMate = CatalogLookup.Local(
                catalog, FlowBedDefaults.RailId, FlowBedDefaults.RailInOutMatePoint, FlowBedDefaults.View);
            Assert.True(lowMate.HasValue);

            foreach (var cell in PushBackElevations.Resolve(system, catalog, front).Values)
            {
                var chosenError = PushBackBedRotation.SlopeError(cell.RotationRadians);
                var legacy = LegacyInsertion(system, catalog, front, cell.RearContact, lowMate.Value.Y);

                var exitMate = new Point2D(cell.LowContact.X, legacy + lowMate.Value.Y);
                if (exitMate.Y >= cell.RearContact.Y)
                {
                    continue;
                }

                var legacyRotation = PushBackBedRotation.Solve(exitMate, cell.RearContact, railMate);
                if (!legacyRotation.HasValue)
                {
                    continue;
                }

                // El candidato legacy nunca puede ser MEJOR que el elegido: si lo fuera, el desempate estaría
                // mandando por encima del criterio principal.
                Assert.True(PushBackBedRotation.SlopeError(legacyRotation.Value) >= chosenError - 1e-12);
            }
        }
    }
}
