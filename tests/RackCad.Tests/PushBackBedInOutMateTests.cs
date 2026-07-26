using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Catalogs;
using RackCad.Application.Geometry;
using RackCad.Application.Headers;
using RackCad.Application.Persistence;
using RackCad.Application.Systems;
using RackCad.Domain.Systems;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>
    /// I-32, aclaración FINAL del Owner — la cama se atornilla por su <c>TROQUEL_IN</c>.
    ///
    /// El mate obligatorio del extremo de entrada/salida es <c>LARGUERO_IN_OUT.TROQUEL_CAMA</c> con
    /// <c>RIEL_DE_CINTA_CALIBRE_12.TROQUEL_IN</c>: la cama se transforma hasta que ese punto local cae sobre
    /// <see cref="PushBackFlowBedAxis.ExitMate"/>. Y la <c>LONGITUD</c> del riel es SIEMPRE el <b>fondo
    /// estructural completo</b> (<see cref="PushBackFlowBedLateralBuilder.ResolveBedLength"/>).
    ///
    /// Hay <b>una sola</b> longitud de cama: la misma dibuja el riel, alimenta el BOM y mide la subida nominal.
    ///
    /// Dos consecuencias son ESPERADAS y no se recortan:
    /// <list type="bullet">
    /// <item>queda geometría del riel <b>antes</b> de <c>TROQUEL_IN</c> — el riel empieza antes de su primer
    /// troquel de sujeción;</item>
    /// <item>el riel <b>sobresale</b> del larguero posterior, porque su longitud es el fondo completo.</item>
    /// </list>
    ///
    /// Una corrida anterior las trató como penetración y coloco la cama por su origen con
    /// <c>LONGITUD = axis.Length</c>. El Owner <b>rechazó</b> esa interpretación; estas pruebas fijan la regla
    /// vigente para que no vuelva.
    /// </summary>
    public class PushBackBedInOutMateTests
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
            new[] { 3, 4, 6, 8 }.Select(deep => new object[] { deep });

        private static DynamicSystemPlan Plan(PushBackSystem system, RackCatalog catalog, DynamicRackFront front)
            => front == null
                ? new PushBackSystemLateralBuilder().Build(system, catalog)
                : new PushBackSystemLateralBuilder().Build(system, catalog, front.Index);

        private static List<HeaderBlockInstance> Rails(PushBackSystem system, RackCatalog catalog, DynamicRackFront front)
            => Plan(system, catalog, front).Flatten().Instances
                .Where(i => string.Equals(i.PieceId, FlowBedDefaults.RailId, StringComparison.OrdinalIgnoreCase))
                .OrderBy(i => i.Insertion.Y)
                .ToList();

        /// <summary>El <c>TROQUEL_IN</c> del riel, transformado a mundo desde la instancia ya aplanada.</summary>
        private static Point2D InMateWorld(HeaderBlockInstance rail, Point2D localMate)
        {
            var cos = Math.Cos(rail.RotationRadians);
            var sin = Math.Sin(rail.RotationRadians);
            return new Point2D(
                rail.Insertion.X + localMate.X * cos - localMate.Y * sin,
                rail.Insertion.Y + localMate.X * sin + localMate.Y * cos);
        }

        private static Point2D LocalMate(RackCatalog catalog)
            => CatalogLookup.Local(catalog, FlowBedDefaults.RailId, FlowBedDefaults.RailInOutMatePoint, FlowBedDefaults.View);

        // ---------------------------------------------------------------------------------------------------
        // 1. El mate obligatorio: TROQUEL_IN transformado == ExitMate
        // ---------------------------------------------------------------------------------------------------

        [Theory]
        [MemberData(nameof(Fondos))]
        public void TheRailInMate_LandsExactlyOnTheLowBeamContact_PerFront(int palletsDeep)
        {
            var catalog = Catalog;
            var system = System(catalog, palletsDeep);
            var front = system.Structure.Fronts[0];
            var localMate = LocalMate(catalog);

            var axes = PushBackFlowBedGeometry.Resolve(system, catalog, front).OrderBy(a => a.ExitMate.Y).ToList();
            var rails = Rails(system, catalog, front);
            Assert.NotEmpty(rails);
            Assert.Equal(axes.Count, rails.Count);

            for (var i = 0; i < rails.Count; i++)
            {
                var mate = InMateWorld(rails[i], localMate);
                Assert.Equal(axes[i].ExitMate.X, mate.X, 9);
                Assert.Equal(axes[i].ExitMate.Y, mate.Y, 9);
            }
        }

        [Theory]
        [MemberData(nameof(Fondos))]
        public void TheRailInMate_LandsExactlyOnTheLowBeamContact_WholeLateral(int palletsDeep)
        {
            var catalog = Catalog;
            var system = System(catalog, palletsDeep);
            var localMate = LocalMate(catalog);

            var axes = PushBackFlowBedGeometry.Resolve(system, catalog, null).OrderBy(a => a.ExitMate.Y).ToList();
            var rails = Rails(system, catalog, null);
            Assert.NotEmpty(rails);
            Assert.Equal(axes.Count, rails.Count);

            for (var i = 0; i < rails.Count; i++)
            {
                var mate = InMateWorld(rails[i], localMate);
                Assert.Equal(axes[i].ExitMate.X, mate.X, 9);
                Assert.Equal(axes[i].ExitMate.Y, mate.Y, 9);
            }
        }

        // ---------------------------------------------------------------------------------------------------
        // 2. UNA sola longitud: el fondo estructural completo
        // ---------------------------------------------------------------------------------------------------

        [Theory]
        [MemberData(nameof(Fondos))]
        public void TheRailLength_IsAlwaysTheFullStructuralSpan(int palletsDeep)
        {
            var catalog = Catalog;
            var system = System(catalog, palletsDeep);

            foreach (var front in new[] { system.Structure.Fronts[0], null })
            {
                var expected = PushBackFlowBedLateralBuilder.ResolveBedLength(system, front);
                Assert.True(expected > 0.0);

                foreach (var rail in Rails(system, catalog, front))
                {
                    Assert.Equal(expected, rail.DynamicParameters[SelectiveRackDefaults.LengthParam], 9);
                }
            }
        }

        /// <summary>
        /// Y NO es la distancia entre contactos. La corrida rechazada usaba <c>axis.Length</c>, que es mas corta:
        /// esta prueba es el centinela de que esa interpretacion no vuelve.
        /// </summary>
        [Theory]
        [MemberData(nameof(Fondos))]
        public void TheRailLength_IsNotTheDistanceBetweenContacts(int palletsDeep)
        {
            var catalog = Catalog;
            var system = System(catalog, palletsDeep);
            var front = system.Structure.Fronts[0];

            var axes = PushBackFlowBedGeometry.Resolve(system, catalog, front).ToList();
            var fullSpan = PushBackFlowBedLateralBuilder.ResolveBedLength(system, front);

            Assert.All(axes, axis => Assert.True(
                axis.RearContactAlongOrigin < fullSpan - 1e-6,
                $"el contacto posterior ({axis.RearContactAlongOrigin:F4}) deberia caer ANTES del fondo ({fullSpan:F4})"));

            foreach (var rail in Rails(system, catalog, front))
            {
                Assert.NotEqual(axes[0].RearContactAlongOrigin, rail.DynamicParameters[SelectiveRackDefaults.LengthParam], 3);
            }
        }

        // ---------------------------------------------------------------------------------------------------
        // 3. Lo que es ESPERADO y no se recorta
        // ---------------------------------------------------------------------------------------------------

        /// <summary>
        /// El riel empieza ANTES de su <c>TROQUEL_IN</c>, y por tanto antes del contacto. Es como esta hecha la
        /// pieza: su primer troquel de sujeción no está en su punta.
        /// </summary>
        [Theory]
        [MemberData(nameof(Fondos))]
        public void ThereIsRailGeometryBeforeTheInMate_AndThatIsExpected(int palletsDeep)
        {
            var catalog = Catalog;
            var system = System(catalog, palletsDeep);
            var front = system.Structure.Fronts[0];
            var localMate = LocalMate(catalog);
            Assert.True(localMate.X > 0.0, "el TROQUEL_IN del riel debe estar adelantado respecto de su origen");

            var axes = PushBackFlowBedGeometry.Resolve(system, catalog, front).OrderBy(a => a.ExitMate.Y).ToList();
            var rails = Rails(system, catalog, front);

            for (var i = 0; i < rails.Count; i++)
            {
                var axis = axes[i];
                var cos = Math.Cos(axis.RotationRadians);
                var sin = Math.Sin(axis.RotationRadians);
                var along = (rails[i].Insertion.X - axis.ExitMate.X) * cos + (rails[i].Insertion.Y - axis.ExitMate.Y) * sin;
                Assert.True(along < 0.0, "el origen del riel debe quedar ANTES del contacto, no sobre el");

                // Y ese retroceso es exactamente el mate local proyectado sobre el eje: ni mas ni menos.
                Assert.Equal(-localMate.X, along, 6);
            }
        }

        /// <summary>El riel sobresale del larguero posterior, porque su longitud es el fondo completo.</summary>
        [Theory]
        [MemberData(nameof(Fondos))]
        public void TheRailOvershootsTheRearBeam_AndThatIsExpected(int palletsDeep)
        {
            var catalog = Catalog;
            var system = System(catalog, palletsDeep);
            var front = system.Structure.Fronts[0];

            var axes = PushBackFlowBedGeometry.Resolve(system, catalog, front).OrderBy(a => a.ExitMate.Y).ToList();
            var rails = Rails(system, catalog, front);

            for (var i = 0; i < rails.Count; i++)
            {
                var longitud = rails[i].DynamicParameters[SelectiveRackDefaults.LengthParam];
                var cos = Math.Cos(rails[i].RotationRadians);
                var sin = Math.Sin(rails[i].RotationRadians);
                var end = new Point2D(rails[i].Insertion.X + longitud * cos, rails[i].Insertion.Y + longitud * sin);

                var axis = axes[i];
                var endAlong = (end.X - axis.RailOrigin.X) * cos + (end.Y - axis.RailOrigin.Y) * sin;
                Assert.True(endAlong > axis.RearContactAlongOrigin,
                    $"el riel deberia sobrepasar el contacto posterior: acaba en {endAlong:F4} y el contacto esta en {axis.RearContactAlongOrigin:F4}");
            }
        }

        /// <summary>
        /// La cama PASA por la tangencia posterior: el contacto del larguero posterior cae sobre la recta del riel
        /// y dentro de su longitud. Sobresalir por detras no rompe la tangencia — la contiene.
        /// </summary>
        [Theory]
        [MemberData(nameof(Fondos))]
        public void TheBedPassesThroughTheRearTangency(int palletsDeep)
        {
            var catalog = Catalog;
            var system = System(catalog, palletsDeep);
            var localMate = LocalMate(catalog);

            foreach (var front in new[] { system.Structure.Fronts[0], null })
            {
                var axes = PushBackFlowBedGeometry.Resolve(system, catalog, front).OrderBy(a => a.ExitMate.Y).ToList();
                var rails = Rails(system, catalog, front);

                for (var i = 0; i < rails.Count; i++)
                {
                    var cos = Math.Cos(rails[i].RotationRadians);
                    var sin = Math.Sin(rails[i].RotationRadians);

                    // La tangencia posterior se mide desde el ORIGEN del riel, NO desde su TROQUEL_IN. Las dos
                    // rectas son paralelas pero distintas; esta prueba usaba la equivocada y por eso fijaba el
                    // defecto (aclaracion final del Owner, I-32).
                    var dx = axes[i].HighMate.X - rails[i].Insertion.X;
                    var dy = axes[i].HighMate.Y - rails[i].Insertion.Y;
                    Assert.Equal(0.0, dx * -sin + dy * cos, 9);

                    // Y el contacto queda DENTRO del riel, medido desde ese mismo origen.
                    var fromOrigin = dx * cos + dy * sin;
                    Assert.InRange(fromOrigin, 0.0, rails[i].DynamicParameters[SelectiveRackDefaults.LengthParam]);
                }
            }
        }

        // ---------------------------------------------------------------------------------------------------
        // 4. Lo que NO se toca
        // ---------------------------------------------------------------------------------------------------

        /// <summary>Los intermedios siguen tangentes a la línea del ORIGEN del riel.</summary>
        [Theory]
        [MemberData(nameof(Fondos))]
        public void TheIntermediateSupports_StayTangentToTheRailOriginLine(int palletsDeep)
        {
            const string infinito = "LARGUERO_ESCALON_INFINITO";
            var catalog = Catalog;
            var system = System(catalog, palletsDeep);
            var axes = PushBackFlowBedGeometry.Resolve(system, catalog).ToList();
            var left = CatalogLookup.Local(catalog, infinito, "INICIO_IZQUIERDO", FlowBedDefaults.View);
            var right = CatalogLookup.Local(catalog, infinito, "INICIO_DERECHO", FlowBedDefaults.View);

            var intermediates = new PushBackSystemLateralBuilder().Build(system, catalog).Flatten().Instances
                .Where(i => string.Equals(i.PieceId, infinito, StringComparison.OrdinalIgnoreCase))
                .ToList();
            Assert.NotEmpty(intermediates);

            Assert.All(intermediates, beam =>
            {
                var mate = beam.MirroredX ? right : left;
                var contactX = beam.Insertion.X + (beam.MirroredX ? -mate.X : mate.X);
                Assert.Contains(axes, axis =>
                    Math.Abs((axis.RailOriginYAt(contactX) - mate.Y) - beam.Insertion.Y) < 1e-6);
            });
        }

        [Theory]
        [MemberData(nameof(Fondos))]
        public void BothBeamsStayOnTheirTroqueles_AndTheResultingSlopeIsUntouched(int palletsDeep)
        {
            var catalog = Catalog;
            var system = System(catalog, palletsDeep);
            var front = system.Structure.Fronts[0];
            var grid = PushBackTroquelGrid.Base(system.Structure, catalog);
            var cells = PushBackElevations.Resolve(system, catalog, front);

            foreach (var cell in cells.Values)
            {
                Assert.Equal(PushBackTroquelGrid.Snap(cell.LowInsertion, grid), cell.LowInsertion, 9);
                Assert.Equal(PushBackTroquelGrid.Snap(cell.RearInsertion, grid), cell.RearInsertion, 9);
            }

            foreach (var axis in PushBackFlowBedGeometry.Resolve(system, catalog, front))
            {
                Assert.Equal(cells[axis.LevelNumber].RotationRadians, axis.RotationRadians, 12);
                Assert.Equal(cells[axis.LevelNumber].ResultingSlope, axis.Slope, 12);
            }
        }

        /// <summary>El BOM de la cama usa esa MISMA longitud: no hay dos longitudes que conciliar.</summary>
        [Theory]
        [MemberData(nameof(Fondos))]
        public void TheBom_UsesTheSameFullSpanLength(int palletsDeep)
        {
            var catalog = Catalog;
            var system = System(catalog, palletsDeep);
            var front = system.Structure.Fronts[0];
            var expected = Math.Round(PushBackFlowBedLateralBuilder.ResolveBedLength(system, front), 4);

            var beds = PushBackBomBuilder.Build(system, catalog).Components
                .Where(c => string.Equals(c.Category, SystemBomBuilder.Cama, StringComparison.OrdinalIgnoreCase))
                .ToList();
            Assert.NotEmpty(beds);
            Assert.All(beds, bed => Assert.Equal(expected, Math.Round(bed.Length, 4)));

            // Y coincide con la LONGITUD que se dibuja: una sola longitud de cama.
            foreach (var rail in Rails(system, catalog, front))
            {
                Assert.Equal(expected, Math.Round(rail.DynamicParameters[SelectiveRackDefaults.LengthParam], 4));
            }
        }

        /// <summary>La corrección del protector lateral se conserva íntegra: primero sin espejo, último espejado.</summary>
        [Fact]
        public void TheLateralGuardDefault_IsStillCorrected()
        {
            var lowEnd = new SelectiveSafetySelection { ElementId = "X", Side = SafetySide.None, LowEndOnly = true };

            var first = Assert.Single(DynamicLateralGuardPlan.CopiesAt(lowEnd, 0, 3));
            Assert.False(first.AtHighEnd);
            Assert.False(first.Mirrored);

            var last = Assert.Single(DynamicLateralGuardPlan.CopiesAt(lowEnd, 2, 3));
            Assert.False(last.AtHighEnd);
            Assert.True(last.Mirrored);

            Assert.Empty(DynamicLateralGuardPlan.CopiesAt(lowEnd, 1, 3));
        }
    }
}
