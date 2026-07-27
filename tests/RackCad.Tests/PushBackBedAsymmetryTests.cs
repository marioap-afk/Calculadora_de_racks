using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Catalogs;
using RackCad.Application.Drawing;
using RackCad.Application.Geometry;
using RackCad.Application.Persistence;
using RackCad.Application.Systems.Dynamic;
using RackCad.Application.Systems.PushBack;
using RackCad.Domain.Systems.Dynamic;
using RackCad.Domain.Systems.FlowBed;
using RackCad.Domain.Systems.PushBack;
using RackCad.Domain.Systems.Selective;
using RackCad.Domain.Systems.Shared;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>
    /// I-32, aclaración final del Owner — la cama Push Back tiene geometría <b>ASIMÉTRICA</b>.
    ///
    /// No usa la misma referencia física en sus dos extremos:
    /// <list type="bullet">
    /// <item><b>Entrada/Salida</b>: mate por <c>TROQUEL_IN</c> sobre el <c>TROQUEL_CAMA</c> del larguero In/Out;</item>
    /// <item><b>Intermedios y posterior</b>: tangentes a la línea del <b>ORIGEN</b> del bloque.</item>
    /// </list>
    ///
    /// Las dos líneas son PARALELAS —mismo bloque rígido, misma rotación y misma pendiente— pero están desplazadas
    /// entre sí por la componente perpendicular del mate local.
    ///
    /// <b>El defecto:</b> la rotación se derivaba como <c>atan2(HighMate − ExitMate)</c>, tratando los dos contactos
    /// como si compartieran recta. Consecuencia medida: el contacto posterior quedaba <b>exactamente 1.25"</b> fuera
    /// de la línea del origen — justo la separación entre las dos paralelas.
    ///
    /// Y la selección del troquel bajo pasa a minimizar el error de PENDIENTE contra 7/192, sobre la retícula de 2",
    /// en vez de ajustar una subida nominal.
    /// </summary>
    public class PushBackBedAsymmetryTests
    {
        private const double Target = 7.0 / 192.0;

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

        private static Point2D RailMate(RackCatalog catalog)
            => CatalogLookup.Local(catalog, FlowBedDefaults.RailId, FlowBedDefaults.RailInOutMatePoint, FlowBedDefaults.View);

        private static List<HeaderBlockInstance> Rails(PushBackSystem system, RackCatalog catalog, DynamicRackFront front)
            => (front == null
                    ? new PushBackSystemLateralBuilder().Build(system, catalog)
                    : new PushBackSystemLateralBuilder().Build(system, catalog, front.Index))
                .Flatten().Instances
                .Where(i => string.Equals(i.PieceId, FlowBedDefaults.RailId, StringComparison.OrdinalIgnoreCase))
                .OrderBy(i => i.Insertion.Y)
                .ToList();

        // ---------------------------------------------------------------------------------------------------
        // 1. El fixture separa de verdad las DOS lineas paralelas
        // ---------------------------------------------------------------------------------------------------

        [Fact]
        public void TheTwoLines_AreGenuinelyDistinct_AndParallel()
        {
            var catalog = Catalog;
            var mate = RailMate(catalog);

            // La separacion perpendicular entre la linea de TROQUEL_IN y la del ORIGEN es su componente Y local.
            Assert.True(Math.Abs(mate.Y) > 1e-6,
                "si TROQUEL_IN estuviera sobre la propia linea del origen, no habria asimetria que probar");
            Assert.Equal(1.25, mate.Y, 6);

            // Y son paralelas por construccion: mismo bloque rigido, misma rotacion.
            var exitMate = new Point2D(10.0, 20.0);
            var rear = new Point2D(210.0, 27.0);
            var theta = PushBackBedRotation.Solve(exitMate, rear, mate);
            Assert.True(theta.HasValue);

            var origin = PushBackBedRotation.OriginFor(exitMate, mate, theta.Value);
            // El mate esta a distancia mate.Y de la linea del origen, perpendicularmente.
            Assert.Equal(-mate.Y, PushBackBedRotation.PerpendicularDistanceToOriginLine(exitMate, exitMate, mate, theta.Value), 9);
            Assert.NotEqual(origin.Y, exitMate.Y, 3);
        }

        // ---------------------------------------------------------------------------------------------------
        // 2. El mate bajo: TROQUEL_IN transformado == ExitMate
        // ---------------------------------------------------------------------------------------------------

        [Theory]
        [MemberData(nameof(Fondos))]
        public void TheRailInMate_StillLandsOnTheLowBeamContact(int palletsDeep)
        {
            var catalog = Catalog;
            var system = System(catalog, palletsDeep);
            var mate = RailMate(catalog);

            foreach (var front in new[] { system.Structure.Fronts[0], null })
            {
                var axes = PushBackFlowBedGeometry.Resolve(system, catalog, front).OrderBy(a => a.ExitMate.Y).ToList();
                var rails = Rails(system, catalog, front);
                Assert.Equal(axes.Count, rails.Count);

                for (var i = 0; i < rails.Count; i++)
                {
                    var cos = Math.Cos(rails[i].RotationRadians);
                    var sin = Math.Sin(rails[i].RotationRadians);
                    var world = new Point2D(
                        rails[i].Insertion.X + mate.X * cos - mate.Y * sin,
                        rails[i].Insertion.Y + mate.X * sin + mate.Y * cos);

                    Assert.Equal(axes[i].ExitMate.X, world.X, 9);
                    Assert.Equal(axes[i].ExitMate.Y, world.Y, 9);
                }
            }
        }

        // ---------------------------------------------------------------------------------------------------
        // 3. El contacto POSTERIOR pertenece a la linea del ORIGEN, no a la de TROQUEL_IN
        // ---------------------------------------------------------------------------------------------------

        [Theory]
        [MemberData(nameof(Fondos))]
        public void TheRearContact_LiesOnTheOriginLine(int palletsDeep)
        {
            var catalog = Catalog;
            var system = System(catalog, palletsDeep);
            var mate = RailMate(catalog);

            foreach (var front in new[] { system.Structure.Fronts[0], null })
            {
                var axes = PushBackFlowBedGeometry.Resolve(system, catalog, front).ToList();
                Assert.NotEmpty(axes);

                foreach (var axis in axes)
                {
                    var perpendicular = PushBackBedRotation.PerpendicularDistanceToOriginLine(
                        axis.HighMate, axis.ExitMate, mate, axis.RotationRadians);
                    Assert.Equal(0.0, perpendicular, 9);
                }
            }
        }

        /// <summary>
        /// Y NO pertenece a la de <c>TROQUEL_IN</c>. Con la rotacion vigente queda separado de ella exactamente por
        /// la distancia entre las dos paralelas: es la contrapartida exacta del defecto anterior.
        /// </summary>
        [Theory]
        [MemberData(nameof(Fondos))]
        public void TheRearContact_DoesNotLieOnTheInMateLine(int palletsDeep)
        {
            var catalog = Catalog;
            var system = System(catalog, palletsDeep);
            var mate = RailMate(catalog);

            foreach (var axis in PushBackFlowBedGeometry.Resolve(system, catalog, system.Structure.Fronts[0]))
            {
                var sin = Math.Sin(axis.RotationRadians);
                var cos = Math.Cos(axis.RotationRadians);
                var perpendicularToInLine =
                    (axis.HighMate.X - axis.ExitMate.X) * sin - (axis.HighMate.Y - axis.ExitMate.Y) * cos;

                // Es la propia ecuacion de la rotacion: E.X sin - E.Y cos = m.Y. El posterior queda al otro lado
                // de la linea de TROQUEL_IN, a exactamente la separacion entre las dos paralelas.
                Assert.Equal(mate.Y, perpendicularToInLine, 9);
            }
        }

        /// <summary>
        /// La rotacion NO sale de <c>atan2(HighMate − ExitMate)</c>. Es la comprobacion directa del defecto: aquella
        /// formula es la solucion del caso degenerado en el que el mate estuviera sobre la linea del origen.
        /// </summary>
        [Theory]
        [MemberData(nameof(Fondos))]
        public void TheRotation_IsNotTheAngleBetweenTheTwoContacts(int palletsDeep)
        {
            var catalog = Catalog;
            var system = System(catalog, palletsDeep);
            var mate = RailMate(catalog);

            foreach (var axis in PushBackFlowBedGeometry.Resolve(system, catalog, system.Structure.Fronts[0]))
            {
                var naive = Math.Atan2(
                    axis.HighMate.Y - axis.ExitMate.Y,
                    axis.HighMate.X - axis.ExitMate.X);

                Assert.NotEqual(naive, axis.RotationRadians, 6);

                // Con la formula ingenua el posterior quedaba 1.25" fuera de la linea del origen.
                Assert.Equal(
                    -mate.Y,
                    PushBackBedRotation.PerpendicularDistanceToOriginLine(axis.HighMate, axis.ExitMate, mate, naive),
                    6);
            }
        }

        // ---------------------------------------------------------------------------------------------------
        // 4. Los INTERMEDIOS usan esa misma linea del origen
        // ---------------------------------------------------------------------------------------------------

        [Theory]
        [MemberData(nameof(Fondos))]
        public void TheIntermediates_ShareTheSameOriginLine(int palletsDeep)
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

        // ---------------------------------------------------------------------------------------------------
        // 5. La seleccion del troquel bajo minimiza el error de pendiente contra 7/192
        // ---------------------------------------------------------------------------------------------------

        /// <summary>
        /// Recorre TODO el rango valido de la retícula y comprueba que la posicion elegida es el minimo GLOBAL, no
        /// un minimo local ni un acierto accidental. Incluye explicitamente los vecinos ±2".
        /// </summary>
        [Theory]
        [MemberData(nameof(Fondos))]
        public void TheChosenTroquel_GloballyMinimisesTheSlopeErrorAgainstTheTarget(int palletsDeep)
        {
            var catalog = Catalog;
            var system = System(catalog, palletsDeep);
            var front = system.Structure.Fronts[0];
            var mate = RailMate(catalog);
            var grid = PushBackTroquelGrid.Base(system.Structure, catalog);
            var lowMate = PushBackLoadBeamGeometry.BedTangencyPointLocal(catalog, DynamicRackDefaults.InOutBeamCatalogId).Value;
            var pitch = SelectiveRackDefaults.TroquelPaso;

            foreach (var cell in PushBackElevations.Resolve(system, catalog, front).Values)
            {
                var chosenError = PushBackBedRotation.SlopeError(cell.RotationRadians);

                // Todo el rango valido: desde el primer troquel por encima del suelo hasta que la subida se anula.
                var neighboursChecked = 0;
                for (var candidate = PushBackTroquelGrid.Snap(grid, grid); ; candidate += pitch)
                {
                    var exitMate = new Point2D(cell.LowContact.X, candidate + lowMate.Y);
                    if (exitMate.Y >= cell.RearContact.Y)
                    {
                        break;
                    }

                    var theta = PushBackBedRotation.Solve(exitMate, cell.RearContact, mate);
                    if (!theta.HasValue)
                    {
                        continue;
                    }

                    var error = PushBackBedRotation.SlopeError(theta.Value);
                    Assert.True(error >= chosenError - 1e-12,
                        $"el troquel {candidate:F4} da error {error:E4}, menor que el elegido {cell.LowInsertion:F4} ({chosenError:E4})");
                    neighboursChecked++;
                }

                Assert.True(neighboursChecked > 3, "el rango de candidatos recorrido es demasiado corto para ser concluyente");

                // Y los dos vecinos inmediatos, dichos explicitamente.
                foreach (var delta in new[] { -pitch, pitch })
                {
                    var exitMate = new Point2D(cell.LowContact.X, cell.LowInsertion + delta + lowMate.Y);
                    var theta = PushBackBedRotation.Solve(exitMate, cell.RearContact, mate);
                    if (theta.HasValue)
                    {
                        Assert.True(PushBackBedRotation.SlopeError(theta.Value) >= chosenError - 1e-12);
                    }
                }
            }
        }

        [Theory]
        [MemberData(nameof(Fondos))]
        public void TheResultingSlope_IsWithinReachOfTheTarget(int palletsDeep)
        {
            var catalog = Catalog;
            var system = System(catalog, palletsDeep);

            foreach (var axis in PushBackFlowBedGeometry.Resolve(system, catalog, system.Structure.Fronts[0]))
            {
                // Con paso de 2" sobre un fondo de este orden, el mejor troquel siempre queda muy cerca del objetivo.
                Assert.True(PushBackBedRotation.SlopeError(axis.RotationRadians) < 0.01,
                    $"pendiente {Math.Tan(axis.RotationRadians):F6} contra el objetivo {Target:F6}");
            }
        }

        // ---------------------------------------------------------------------------------------------------
        // 6. Los troqueles y la longitud no se tocan
        // ---------------------------------------------------------------------------------------------------

        [Theory]
        [MemberData(nameof(Fondos))]
        public void BothEndBeamsStayOnValidTroqueles_AndTheRearKeepsItsAnchor(int palletsDeep)
        {
            var catalog = Catalog;
            var system = System(catalog, palletsDeep);
            var front = system.Structure.Fronts[0];
            var grid = PushBackTroquelGrid.Base(system.Structure, catalog);

            var placements = DynamicLoadBeamGeometry.Placements(system.Structure, front);
            foreach (var cell in PushBackElevations.Resolve(system, catalog, front).Values)
            {
                Assert.Equal(PushBackTroquelGrid.Snap(cell.LowInsertion, grid), cell.LowInsertion, 9);
                Assert.Equal(PushBackTroquelGrid.Snap(cell.RearInsertion, grid), cell.RearInsertion, 9);

                // El posterior es el ANCLA: conserva EXACTAMENTE la elevacion que le dio el resolver.
                var rear = placements.First(p => p.LevelNumber == cell.LevelNumber && p.IsEntrance);
                Assert.Equal(rear.Y, cell.RearInsertion, 9);
            }
        }

        [Theory]
        [MemberData(nameof(Fondos))]
        public void TheRailLength_IsStillTheFullStructuralSpan_AndTheRearContactSitsInsideIt(int palletsDeep)
        {
            var catalog = Catalog;
            var system = System(catalog, palletsDeep);

            foreach (var front in new[] { system.Structure.Fronts[0], null })
            {
                var expected = PushBackFlowBedLateralBuilder.ResolveBedLength(system, front);
                var axes = PushBackFlowBedGeometry.Resolve(system, catalog, front).OrderBy(a => a.ExitMate.Y).ToList();
                var rails = Rails(system, catalog, front);

                for (var i = 0; i < rails.Count; i++)
                {
                    var longitud = rails[i].DynamicParameters[SelectiveRackDefaults.LengthParam];
                    Assert.Equal(expected, longitud, 9);

                    // El contacto posterior queda DENTRO del riel; el sobrepaso por detras sigue permitido.
                    var cos = Math.Cos(rails[i].RotationRadians);
                    var sin = Math.Sin(rails[i].RotationRadians);
                    var along = (axes[i].HighMate.X - rails[i].Insertion.X) * cos
                        + (axes[i].HighMate.Y - rails[i].Insertion.Y) * sin;
                    Assert.InRange(along, 0.0, longitud);
                    Assert.True(along < longitud, "debe sobrar riel por detras del contacto posterior");
                }
            }
        }

        // ---------------------------------------------------------------------------------------------------
        // 7. La seguridad ya aprobada no se toca
        // ---------------------------------------------------------------------------------------------------

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
