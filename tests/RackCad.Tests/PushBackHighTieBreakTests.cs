using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Catalogs;
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
    /// DECISIÓN FINAL DEL DUEÑO — los desempates de la elección del troquel ALTO.
    ///
    /// <para>
    /// Esta clase SUSTITUYE a <c>PushBackLegacyTieBreakTests</c>, que fijaba los desempates de la política
    /// RETIRADA: aquella anclaba el larguero POSTERIOR y elegía el de ENTRADA, y su tercer criterio era «el
    /// candidato más cercano al resultado de la regla PRE-I-32». Ese criterio pertenecía a la selección del BAJO
    /// —no tiene equivalente para el alto— y el dueño lo retiró explícitamente junto con la política. Mantener
    /// aquellas pruebas habría dejado fijado en verde un contrato que el código ya no cumple.
    /// </para>
    ///
    /// <para>La política vigente elige el ALTO y desempata, en este orden exacto:</para>
    /// <list type="number">
    /// <item>MENOR error de pendiente contra 7/192;</item>
    /// <item>a igualdad, el más cercano al ALTO teórico (<c>contacto bajo + subida nominal</c>);</item>
    /// <item>a igualdad, el de MENOR elevación.</item>
    /// </list>
    /// </summary>
    public class PushBackHighTieBreakTests
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

        private readonly record struct Candidate(double Insertion, double Error, double ToTheoretical);

        /// <summary>
        /// Re-enumera la retícula DE FORMA INDEPENDIENTE del código de producción, con un rango deliberadamente
        /// amplio, para poder afirmar que el elegido es el óptimo GLOBAL y no el mejor de una ventana.
        /// </summary>
        private static List<Candidate> Candidates(
            PushBackSystem system, RackCatalog catalog, DynamicRackFront front, int level)
        {
            var pitch = SelectiveRackDefaults.TroquelPaso;
            var gridBase = PushBackTroquelGrid.Base(system.Structure, catalog);
            var railMate = CatalogLookup.Local(
                catalog, FlowBedDefaults.RailId, FlowBedDefaults.RailInOutMatePoint, FlowBedDefaults.View);
            var placements = PushBackPlacements.Resolve(system, front);
            var low = placements.First(p => p.LevelNumber == level && !p.IsEntrance);
            var rear = placements.First(p => p.LevelNumber == level && p.IsEntrance);
            var lowMate = PushBackLoadBeamGeometry
                .BedTangencyPointLocal(catalog, DynamicRackDefaults.InOutBeamCatalogId).Value;
            var lowContact = PushBackLoadBeamGeometry.BedTangencyPointWorld(lowMate, low.X, low.Y, low.MirroredX);
            var exitMate = new Point2D(lowContact.X, low.Y + lowMate.Y);
            var theoretical = lowContact.Y + PushBackBedSlope.Rise(PushBackCellDepth.BedLength(system, front, level));

            var result = new List<Candidate>();
            for (var insertion = gridBase; insertion <= exitMate.Y + 120.0; insertion += pitch)
            {
                var contact = PushBackLoadBeamGeometry.RearBeamTangencyPointWorld(
                    catalog, PushBackDefaults.HighEndBeamCatalogId, rear.X, insertion, rear.MirroredX);
                if (!contact.HasValue || contact.Value.Y <= exitMate.Y)
                {
                    continue;
                }

                var rotation = PushBackBedRotation.Solve(exitMate, contact.Value, railMate);
                if (!rotation.HasValue)
                {
                    continue;
                }

                result.Add(new Candidate(
                    insertion,
                    PushBackBedRotation.SlopeError(rotation.Value),
                    Math.Abs(contact.Value.Y - theoretical)));
            }

            return result;
        }

        /// <summary>
        /// El criterio PRINCIPAL: no existe ningún troquel de la retícula que dé menos error de pendiente que el
        /// elegido. Se comprueba contra un barrido propio y mucho más ancho que el del código, así que también
        /// demuestra que el rango que ese código recorre contiene al óptimo.
        /// </summary>
        [Theory]
        [MemberData(nameof(Fondos))]
        public void TheChosenHigh_IsTheGlobalSlopeOptimum(int palletsDeep)
        {
            var catalog = Catalog;
            var system = System(catalog, palletsDeep);
            var front = system.Structure.Fronts[0];

            foreach (var cell in PushBackElevations.Resolve(system, catalog, front).Values)
            {
                var candidates = Candidates(system, catalog, front, cell.LevelNumber);
                Assert.NotEmpty(candidates);

                var chosen = candidates.Single(c => Math.Abs(c.Insertion - cell.RearInsertion) < 1e-9);
                var bestError = candidates.Min(c => c.Error);
                Assert.Equal(bestError, chosen.Error, 9);
            }
        }

        /// <summary>
        /// SEGUNDO desempate: entre los que empatan en pendiente gana el más cercano al ALTO teórico. TERCERO: si
        /// también empatan ahí, el de menor elevación.
        /// </summary>
        [Theory]
        [MemberData(nameof(Fondos))]
        public void TheTieBreaks_ResolveInTheOwnerOrder(int palletsDeep)
        {
            var catalog = Catalog;
            var system = System(catalog, palletsDeep);
            var front = system.Structure.Fronts[0];

            foreach (var cell in PushBackElevations.Resolve(system, catalog, front).Values)
            {
                var candidates = Candidates(system, catalog, front, cell.LevelNumber);
                var expected = candidates
                    .OrderBy(c => Math.Round(c.Error, 9))
                    .ThenBy(c => Math.Round(c.ToTheoretical, 9))
                    .ThenBy(c => c.Insertion)
                    .First();

                Assert.Equal(expected.Insertion, cell.RearInsertion, 9);
            }
        }

        /// <summary>
        /// El desempate RETIRADO ya no manda. La referencia PRE-I-32 se reconstruye aquí con su fórmula original y
        /// se comprueba que apunta a otro troquel: la autoridad no la sigue, porque quien decide es el error de
        /// pendiente. Es la prueba de que la política antigua no quedó viva por una puerta lateral.
        /// </summary>
        [Fact]
        public void TheWithdrawnLegacyTieBreak_NoLongerSteersTheChoice()
        {
            var catalog = Catalog;
            var lowMate = PushBackLoadBeamGeometry
                .BedTangencyPointLocal(catalog, DynamicRackDefaults.InOutBeamCatalogId).Value;
            var diverged = 0;
            var compared = 0;

            foreach (var deep in new[] { 2, 3, 4, 5, 6, 7, 8 })
            {
                var system = System(catalog, deep);
                var front = system.Structure.Fronts[0];
                var gridBase = PushBackTroquelGrid.Base(system.Structure, catalog);

                foreach (var cell in PushBackElevations.Resolve(system, catalog, front).Values)
                {
                    // La fórmula PRE-I-32, literal: ajustar la subida nominal por debajo del contacto posterior.
                    var nominalRise = PushBackBedSlope.Rise(PushBackFlowBedGeometry.ResolveBedLength(system, front));
                    var legacyLow = PushBackTroquelGrid.Snap(
                        cell.RearContact.Y - nominalRise - lowMate.Y, gridBase);

                    compared++;
                    if (Math.Abs(legacyLow - cell.LowInsertion) > 1e-9)
                    {
                        diverged++;
                    }
                }
            }

            Assert.True(compared > 0);
            Assert.True(diverged > 0,
                "la referencia retirada coincidió con el resultado en todos los casos: el barrido no demuestra que " +
                "haya dejado de influir");
        }

        /// <summary>
        /// Y la consecuencia que motivó la decisión: cambiar el FONDO mueve el ALTO y NUNCA el bajo. Con la política
        /// anterior pasaba lo contrario y el larguero de entrada se hundía por debajo de la altura pedida.
        /// </summary>
        [Fact]
        public void ChangingTheFondo_MovesTheHigh_AndNeverTheLow()
        {
            var catalog = Catalog;
            var lows = new List<double>();
            var highs = new List<double>();

            foreach (var deep in new[] { 2, 3, 4, 5, 6, 7, 8 })
            {
                var system = System(catalog, deep);
                var front = system.Structure.Fronts[0];
                var cell = PushBackElevations.Resolve(system, catalog, front)[1];
                lows.Add(Math.Round(cell.LowInsertion, 6));
                highs.Add(Math.Round(cell.RearInsertion, 6));
            }

            Assert.Single(lows.Distinct());
            Assert.True(highs.Distinct().Count() > 1, "el alto tiene que seguir al fondo");
            Assert.Equal(highs.OrderBy(y => y).ToList(), highs);
        }
    }
}
